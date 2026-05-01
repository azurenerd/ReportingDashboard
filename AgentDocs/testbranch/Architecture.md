# Architecture

## Overview & Goals

The ReportingDashboard is a local-only, zero-cost developer tool that renders a composite release roadmap visualization—combining a Gantt-style timeline with milestone markers and a color-coded monthly execution heatmap—driven by Azure DevOps work-item data. It runs entirely on `localhost` with no cloud services, no hosting infrastructure, and no operational cost.

**Architecture Goals:**

1. **Eliminate manual status report assembly** — Reduce roadmap generation from ~30 minutes to <2 minutes via one-click sync + browser screenshot.
2. **Single-view leadership dashboard** — One 1920×1080 browser screenshot captures full team status: workstream milestones, shipped items, in-progress work, carryover, and blockers.
3. **Zero operational cost** — No cloud hosting, no database servers, no licenses. Runs locally on any Windows machine with .NET 8.
4. **Single-artifact distribution** — Self-contained EXE (<40MB) with embedded frontend assets, .NET runtime, and SQLite.
5. **Pixel-perfect design fidelity** — Dashboard matches `OriginalDesignConcept.html` within 2px tolerance at 1920×1080.

**System Context:**

```
┌─────────────────────────────────────────────────────┐
│                  Developer Machine                   │
│                                                     │
│  ┌──────────────┐  localhost:5000  ┌──────────────┐ │
│  │   Browser     │◄──────────────►│ ASP.NET Core  │ │
│  │  (TypeScript  │  JSON / HTTP    │ Minimal API   │ │
│  │   + D3.js)    │                 │               │ │
│  └──────────────┘                 └───────┬───────┘ │
│                                           │         │
│                                  ┌────────▼───────┐ │
│                                  │ SQLite (EF Core)│ │
│                                  │ dashboard.db    │ │
│                                  └────────────────┘ │
└─────────────────────────────────────────────────────┘
                                           │ HTTPS (PAT auth)
                                           ▼
                                 ┌──────────────────┐
                                 │  Azure DevOps     │
                                 │  REST API v7.1    │
                                 │  (WIQL + Batch)   │
                                 └──────────────────┘
```

---

## System Components

### Backend Components

#### 1. `Program.cs` (Composition Root)

**Responsibility:** Wire all dependencies, configure middleware pipeline, start Kestrel on localhost:5000.

- Registers `DashboardDbContext`, `IAdoSyncService`, `StateMappingService`, `CredentialStore`, `IMemoryCache`, `IHttpClientFactory`
- Configures Kestrel (`ListenLocalhost(5000)`), Serilog (file sink with PAT exclusion), Swagger (Development only)
- Maps static files + SPA proxy + API endpoints
- Runs `db.Database.MigrateAsync()` + conditional seed on startup

**Dependencies:** All registered services.
**Data Owned:** None directly. Orchestrates startup sequence.

---

#### 2. `DashboardDbContext`

**Responsibility:** Define the EF Core data model, manage SQLite connection, provide `DbSet<T>` access to all entities. Single source of truth for schema definition.

```csharp
public class DashboardDbContext : DbContext
{
    public DbSet<Workstream> Workstreams => Set<Workstream>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    protected override void OnModelCreating(ModelBuilder modelBuilder);
}
```

**Dependencies:** `Microsoft.EntityFrameworkCore.Sqlite`, connection string from `IConfiguration`.
**Data Owned:** SQLite file at `%LOCALAPPDATA%\ReportingDashboard\dashboard.db` with tables `Workstreams`, `Milestones`, `WorkItems` and indexes `IX_WorkItems_Status_Month`, `IX_Milestones_WorkstreamId`.

---

#### 3. `SeedData`

**Responsibility:** Populate the database with representative sample data matching the design reference when the database is empty. Idempotent.

```csharp
public static class SeedData
{
    public static async Task SeedAsync(DashboardDbContext db);
}
```

**Dependencies:** `DashboardDbContext`, embedded `sample-data.json`.
**Data Owned:** Canonical sample dataset: 3 workstreams, ~12 milestones, ~40 work items.

---

#### 4. `RoadmapEndpoint`

**Responsibility:** Handle `GET /api/roadmap` — assemble the full dashboard payload from the database, apply memory caching (60s TTL), return a single JSON response.

```csharp
public static class RoadmapEndpoint
{
    public static void MapRoadmapEndpoints(this WebApplication app);
}
```

**Dependencies:** `DashboardDbContext`, `IMemoryCache`, `IOptions<DashboardSettings>`.
**Data Owned:** None. Reads from DB, caches assembled DTO.

---

#### 5. `WorkItemsEndpoint`

**Responsibility:** Handle `GET /api/workitems?status=X&month=Y` — return filtered work items for drill-down. Validates query parameters against allowlists.

```csharp
public static class WorkItemsEndpoint
{
    public static void MapWorkItemEndpoints(this WebApplication app);
}
```

**Dependencies:** `DashboardDbContext`.
**Data Owned:** None. Filtered read from `WorkItems` table.

---

#### 6. `SyncEndpoint`

**Responsibility:** Handle `POST /api/sync` — delegate to `IAdoSyncService`, invalidate cache on success, return structured error responses on failure.

```csharp
public static class SyncEndpoint
{
    public static void MapSyncEndpoints(this WebApplication app);
}
```

**Dependencies:** `IAdoSyncService`, `IMemoryCache`, `ILogger`.
**Data Owned:** None. Orchestrates sync and cache invalidation.

---

#### 7. `IAdoSyncService` / `AdoSyncService`

**Responsibility:** Execute the full ADO sync pipeline: WIQL query → batch fetch → state mapping → SQLite upsert. Encapsulates all ADO REST API interaction.

```csharp
public interface IAdoSyncService
{
    Task<SyncResult> SyncAsync(CancellationToken ct = default);
}
public record SyncResult(int ItemCount, string SyncedAtUtc);
```

**Internal pipeline:**
1. `ExecuteWiqlAsync()` — POST to `/_apis/wit/wiql`, returns work item IDs
2. `FetchBatchAsync()` — POST to `/_apis/wit/workitemsbatch` in chunks of 200
3. `StateMappingService.MapStatus()` — map ADO fields to dashboard categories
4. `UpsertAsync()` — insert new / update existing work items by `AdoId`

**Dependencies:** `HttpClient` (via `IHttpClientFactory`), `DashboardDbContext`, `StateMappingService`, `IOptions<AdoSettings>`, `ILogger`.

**Custom exceptions:** `AdoNotConfiguredException`, `AdoAuthenticationException`, `AdoNetworkException`.

---

#### 8. `StateMappingService`

**Responsibility:** Map a single ADO work item's state, tags, and iteration path to one of four dashboard status categories. All rules configurable via `AdoSettings`.

```csharp
public class StateMappingService
{
    public WorkItemStatus MapStatus(string adoState, string tags, string iterationPath);
    public string ExtractMonth(string iterationPath, DateTime? changedDate);
}
```

**Mapping priority (highest to lowest):**
1. Tag `[blocked]` (case-insensitive) → `Blocked`
2. Tag `[carryover]` (case-insensitive) → `Carryover`
3. State ∈ `ShippedStates` config list → `Shipped`
4. State ∈ `InProgressStates` config list → `InProgress`
5. Past iteration + not shipped → `Carryover`
6. Default → `InProgress`

**Dependencies:** `IOptions<AdoSettings>`.

---

#### 9. `CredentialStore`

**Responsibility:** Encrypt and decrypt the ADO PAT using Windows DPAPI. At-rest protection for the distributed EXE scenario.

```csharp
public class CredentialStore
{
    public void StorePat(string pat);
    public string? LoadPat();
    public void ClearPat();
}
```

**Dependencies:** `System.Security.Cryptography.ProtectedData`.
**Data Owned:** `%LOCALAPPDATA%\ReportingDashboard\cred.dat` (DPAPI-encrypted PAT bytes).

---

#### 10. `AdoSettings` / `DashboardSettings`

**Responsibility:** Strongly-typed configuration POCOs bound to `appsettings.json` sections.

```csharp
public class AdoSettings
{
    public string Organization { get; set; } = "";
    public string Project { get; set; } = "";
    public string AreaPath { get; set; } = "";
    public string Pat { get; set; } = "";
    public string BacklogUrl { get; set; } = "";
    public string[] ShippedStates { get; set; } = ["Closed", "Resolved", "Done"];
    public string[] InProgressStates { get; set; } = ["Active", "Committed", "In Progress"];
    public string CurrentIteration { get; set; } = "";
}

public class DashboardSettings
{
    public string Title { get; set; } = "Privacy Automation Release Roadmap";
    public string Subtitle { get; set; } = "Trusted Platform · Privacy Automation Workstream";
    public int MonthCount { get; set; } = 4;
    public int DateRangeMonths { get; set; } = 6;
}
```

---

### Frontend Components

#### 11. `main.ts` (Application Orchestrator)

**Responsibility:** Entry point. Owns the fetch → render pipeline, error/loading state management, re-render cycle after sync.

```typescript
async function init(): Promise<void>;
function rerenderAll(data: RoadmapData): void;
function showLoading(): void;
function hideLoading(): void;
function showError(message: string): void;
```

**Dependencies:** `api/roadmapApi`, `components/header`, `components/timeline`, `components/heatmap`, `components/drilldown`.

---

#### 12. `roadmapApi.ts` (HTTP Client Layer)

**Responsibility:** Typed fetch wrappers for all API endpoints. Single location for all HTTP calls.

```typescript
export async function fetchRoadmap(): Promise<RoadmapData>;
export async function fetchWorkItems(status: string, month: string): Promise<WorkItemDto[]>;
export async function triggerSync(): Promise<SyncResult>;
```

**Dependencies:** `models/types`, browser `fetch` API.

---

#### 13. `header.ts` (Header Bar Renderer)

**Responsibility:** Render Section A of the design: title, ADO Backlog link, subtitle, legend markers, Sync button with feedback.

```typescript
export function renderHeader(
    container: HTMLElement,
    data: RoadmapData,
    onSync: () => Promise<void>
): void;
```

**Dependencies:** `models/colors`, `models/types`.

---

#### 14. `timeline.ts` (SVG Gantt Renderer)

**Responsibility:** Render Section B: D3.js-driven SVG timeline with workstream lanes, milestone diamonds, checkpoint circles, date labels, month grid lines, NOW dashed line, and workstream labels in the left panel.

```typescript
export function renderTimeline(
    svg: SVGSVGElement,
    workstreams: Workstream[],
    milestones: Milestone[],
    dateRange: [Date, Date]
): void;
```

**Dependencies:** `d3-scale`, `d3-selection`, `d3-time`, `d3-time-format`, `models/colors`, `models/types`.

---

#### 15. `heatmap.ts` (CSS Grid Renderer)

**Responsibility:** Render Section C: monthly execution heatmap grid with status rows × month columns, color-coded cells, work item bullet lists, click handling for drill-down.

```typescript
export function renderHeatmap(
    container: HTMLElement,
    data: RoadmapData,
    onCellClick: (status: string, month: string) => void
): void;
```

**Dependencies:** `models/colors`, `models/types`.

---

#### 16. `drilldown.ts` (Drill-Down Panel)

**Responsibility:** Display overlay panel showing work items for a specific heatmap cell with clickable ADO URLs. Handle dismissal via click-outside, Escape, close button.

```typescript
export function showDrilldown(status: string, month: string, items: WorkItemDto[]): void;
export function hideDrilldown(): void;
```

**Dependencies:** `models/colors`, `models/types`.

---

#### 17. `types.ts` (Type Definitions)

**Responsibility:** All shared TypeScript interfaces. No runtime code.

```typescript
export interface RoadmapData {
    workstreams: Workstream[];
    milestones: Milestone[];
    workItems: WorkItem[];
    months: MonthColumn[];
    dateRange: { start: string; end: string };
    lastSyncUtc: string | null;
}
export interface Workstream { id: string; name: string; color: string; sortOrder: number; }
export interface Milestone { id: string; workstreamId: string; name: string; date: string; type: 'PoC' | 'Production' | 'Checkpoint'; subType: 'Major' | 'Minor'; }
export interface WorkItem { id: string; title: string; status: 'Shipped' | 'InProgress' | 'Carryover' | 'Blocked'; month: string; workstreamId: string; adoUrl: string; }
export interface MonthColumn { name: string; isCurrent: boolean; }
export interface SyncResult { itemCount: number; syncedAtUtc: string; }
export interface WorkItemDto { id: string; title: string; status: string; month: string; adoUrl: string; }
```

---

#### 18. `colors.ts` (Design Token Registry)

**Responsibility:** Single source of truth for every color value in the dashboard.

```typescript
export const COLORS = {
    shipped:    { bg: '#F0FBF0', bgActive: '#D8F2DA', dot: '#34A853', header: '#E8F5E9', text: '#1B7A28' },
    inProgress: { bg: '#EEF4FE', bgActive: '#DAE8FB', dot: '#0078D4', header: '#E3F2FD', text: '#1565C0' },
    carryover:  { bg: '#FFFDE7', bgActive: '#FFF0B0', dot: '#F4B400', header: '#FFF8E1', text: '#B45309' },
    blocked:    { bg: '#FFF5F5', bgActive: '#FFE4E4', dot: '#EA4335', header: '#FEF2F2', text: '#991B1B' },
    milestone:  { poc: '#F4B400', production: '#34A853', checkpoint: '#999' },
    ui: {
        nowLine: '#EA4335', link: '#0078D4', gridBorder: '#E0E0E0', headerBg: '#F5F5F5',
        timelineBg: '#FAFAFA', bodyText: '#111', subtitleText: '#888', itemText: '#333',
        currentMonthHeaderBg: '#FFF0D0', currentMonthHeaderText: '#C07700',
    },
} as const;
```

---

#### 19. `dashboard.css` (Stylesheet)

**Responsibility:** All CSS rules ported from `OriginalDesignConcept.html`. Components create DOM nodes with class names; this file provides all visual styling.

---

## Component Interactions

### Dependency Graph

```
                    Program.cs (Composition Root)
                    ┌──────────┬──────────┐
                    │          │          │
                    ▼          ▼          ▼
            DashboardDbContext  IMemoryCache  Serilog
                    │
                 ┌──┼──┐
    ┌────────────┘  │  └────────────┐
    │               │               │
    ▼               ▼               ▼
RoadmapEndpoint  WorkItemsEndpoint  SyncEndpoint
                                      │
                                      ▼
                                IAdoSyncService
                                 (AdoSyncService)
                              ┌────────┬────────┐
                              │        │        │
                              ▼        ▼        ▼
                         HttpClient  DbContext  StateMappingService
                                                    │
                                                    ▼
                                               AdoSettings

Frontend:
                    main.ts (Orchestrator)
                 ┌─────┬──────┬──────┬────────┐
                 │     │      │      │        │
                 ▼     ▼      ▼      ▼        ▼
           roadmapApi header timeline heatmap drilldown
              │        │       │       │        │
              ▼        ▼       ▼       ▼        ▼
           types    colors   d3     colors    colors
                    types   colors   types     types
                            types
```

**Module boundary rules:**
- `main.ts` → `api/*`, `components/*` (orchestration only)
- `components/*` → `models/*` (data types + colors)
- `components/*` → `d3` (timeline.ts only)
- `api/*` → `models/types` (return types)
- `models/*` → nothing (leaf modules)
- **No component imports another component** (flat, not nested)
- **No component calls `fetch()`** (all HTTP goes through `api/roadmapApi.ts`)

---

### Data Flow: Primary Use Cases

#### Flow 1: Initial Page Load

```
Browser                  ASP.NET Core              SQLite
  │  GET /api/roadmap       │                        │
  │ ───────────────────────>│  Check IMemoryCache    │
  │                         │  (MISS on first load)  │
  │                         │  SELECT * FROM         │
  │                         │  Workstreams,          │
  │                         │  Milestones, WorkItems │
  │                         │ ──────────────────────>│
  │                         │  Assemble RoadmapDto   │
  │                         │  Store in cache (60s)  │
  │  200 OK (RoadmapDto)    │                        │
  │ <───────────────────────│                        │
  │                                                  │
  │  main.ts:                                        │
  │  ├── renderHeader(data)                          │
  │  ├── renderTimeline(svg, workstreams, milestones)│
  │  │   └── D3 scaleTime() maps dates → pixels     │
  │  └── renderHeatmap(data, onCellClick)            │
  │      └── CSS Grid populated with work items      │
```

**Components:** `main.ts` → `roadmapApi` → `RoadmapEndpoint` → `DashboardDbContext` → `header.ts` + `timeline.ts` + `heatmap.ts`

#### Flow 2: First Run (Empty Database)

```
EXE starts → Program.cs
  ├── db.Database.MigrateAsync() → SQLite file created, schema applied
  ├── PRAGMA journal_mode=WAL
  ├── if (!db.Workstreams.Any()) → SeedData.SeedAsync(db)
  │   └── INSERT 3 workstreams, ~12 milestones, ~40 work items
  └── Kestrel starts on localhost:5000
      └── [Flow 1 continues]
```

**Components:** `Program.cs` → `DashboardDbContext` → `SeedData` → [Flow 1]

#### Flow 3: ADO Sync

```
Browser                  ASP.NET Core              Azure DevOps          SQLite
  │  Click "Sync" button    │                         │                    │
  │  POST /api/sync         │                         │                    │
  │ ───────────────────────>│                         │                    │
  │                   SyncEndpoint → AdoSyncService   │                    │
  │                         │  WIQL query             │                    │
  │                         │ ───────────────────────>│                    │
  │                         │  [work item IDs]        │                    │
  │                         │ <───────────────────────│                    │
  │                         │  Batch fetch ×N         │                    │
  │                         │ ───────────────────────>│                    │
  │                         │  [item details]         │                    │
  │                         │ <───────────────────────│                    │
  │                         │  StateMappingService.MapStatus() per item   │
  │                         │  UPSERT into WorkItems                     │
  │                         │ ──────────────────────────────────────────>│
  │                         │  cache.Remove("roadmap")                   │
  │  200 {itemCount, time}  │                                            │
  │ <───────────────────────│                                            │
  │  main.ts: fetchRoadmap() → re-render all sections                   │
```

**Components:** `header.ts` → `main.ts` → `roadmapApi` → `SyncEndpoint` → `AdoSyncService` → `StateMappingService` → `DashboardDbContext` → cache invalidation → [re-render all]

#### Flow 4: Heatmap Cell Drill-Down

```
Browser                  ASP.NET Core              SQLite
  │  Click "InProgress×Apr" │                        │
  │  GET /api/workitems     │                        │
  │  ?status=InProgress     │                        │
  │  &month=Apr             │                        │
  │ ───────────────────────>│                        │
  │                   WorkItemsEndpoint              │
  │                   ├── Validate params            │
  │                   └── SELECT WHERE Status+Month  │
  │                       ─────────────────────────>│
  │  200 OK (WorkItemDto[]) │                        │
  │ <───────────────────────│                        │
  │  showDrilldown(items)   │                        │
  │  └── Overlay with ADO links                     │
```

**Components:** `heatmap.ts` → `main.ts` → `roadmapApi` → `WorkItemsEndpoint` → `DashboardDbContext` → `drilldown.ts`

#### Flow 5: Sync Failure (Expired PAT)

```
Browser                  ASP.NET Core              Azure DevOps
  │  POST /api/sync         │                         │
  │ ───────────────────────>│  WIQL query             │
  │                         │ ───────────────────────>│
  │                         │  401 Unauthorized       │
  │                         │ <───────────────────────│
  │                         │  throw AdoAuthenticationException
  │  401 { error: "..." }   │                         │
  │ <───────────────────────│                         │
  │  Show error in header; existing data remains visible
```

**Guiding principle:** No error condition blanks the dashboard. The user always sees the last successfully loaded data.

---

## Data Model

### Entity Definitions

#### `Workstream`

```csharp
public class Workstream
{
    [Key, MaxLength(10)]
    public string Id { get; set; } = "";              // "M1", "M2", "M3"
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";            // "Chatbot & MS Role"
    [Required, MaxLength(7)]
    public string Color { get; set; } = "";           // "#0078D4"
    public int SortOrder { get; set; }
    public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
```

#### `Milestone`

```csharp
public class Milestone
{
    [Key, MaxLength(50)]
    public string Id { get; set; } = "";              // "m1-poc-mar26"
    [Required, MaxLength(10)]
    public string WorkstreamId { get; set; } = "";    // FK → Workstream.Id
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";            // "Mar 26 PoC"
    public DateTime Date { get; set; }
    [MaxLength(20)]
    public string Type { get; set; } = "";            // "PoC" | "Production" | "Checkpoint"
    [MaxLength(10)]
    public string SubType { get; set; } = "Major";    // "Major" | "Minor"
    public Workstream Workstream { get; set; } = null!;
}
```

#### `WorkItem`

```csharp
public class WorkItem
{
    [Key, MaxLength(20)]
    public string Id { get; set; } = "";              // ADO ID or "seed-001"
    [Required, MaxLength(300)]
    public string Title { get; set; } = "";
    [Required, MaxLength(20)]
    public string Status { get; set; } = "";          // Shipped|InProgress|Carryover|Blocked
    [Required, MaxLength(3)]
    public string Month { get; set; } = "";           // "Jan"–"Dec"
    [Required, MaxLength(10)]
    public string WorkstreamId { get; set; } = "";    // FK → Workstream.Id
    [MaxLength(500)]
    public string AdoUrl { get; set; } = "";
    public int? AdoId { get; set; }                   // Nullable; null for seeded data
    [MaxLength(50)]
    public string? AdoState { get; set; }             // Raw ADO state for debugging
    [MaxLength(500)]
    public string? AdoTags { get; set; }              // Raw tags for debugging
    [MaxLength(200)]
    public string? AdoIterationPath { get; set; }     // Raw iteration path
    public DateTime? LastSyncedUtc { get; set; }
    public Workstream Workstream { get; set; } = null!;
}
```

### Relationships

```
Workstream (1) ────< (N) Milestone     FK: WorkstreamId, ON DELETE CASCADE
Workstream (1) ────< (N) WorkItem      FK: WorkstreamId, ON DELETE CASCADE
```

### Indexes

| Index | Columns | Purpose |
|---|---|---|
| `IX_WorkItems_Status_Month` | `(Status, Month)` | Heatmap GROUP BY and drill-down WHERE |
| `IX_WorkItems_AdoId` | `AdoId` (UNIQUE, filtered NOT NULL) | Upsert matching during ADO sync |
| `IX_Milestones_WorkstreamId` | `WorkstreamId` | Timeline query per workstream |

### EF Core Configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Workstream>(entity =>
    {
        entity.HasKey(w => w.Id);
        entity.Property(w => w.Name).IsRequired().HasMaxLength(100);
        entity.Property(w => w.Color).IsRequired().HasMaxLength(7);
    });

    modelBuilder.Entity<Milestone>(entity =>
    {
        entity.HasKey(m => m.Id);
        entity.Property(m => m.SubType).HasDefaultValue("Major");
        entity.HasIndex(m => m.WorkstreamId).HasDatabaseName("IX_Milestones_WorkstreamId");
        entity.HasOne(m => m.Workstream).WithMany(w => w.Milestones)
              .HasForeignKey(m => m.WorkstreamId).OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<WorkItem>(entity =>
    {
        entity.HasKey(wi => wi.Id);
        entity.HasIndex(wi => new { wi.Status, wi.Month }).HasDatabaseName("IX_WorkItems_Status_Month");
        entity.HasIndex(wi => wi.AdoId).HasDatabaseName("IX_WorkItems_AdoId")
              .IsUnique().HasFilter("[AdoId] IS NOT NULL");
        entity.HasOne(wi => wi.Workstream).WithMany(w => w.WorkItems)
              .HasForeignKey(wi => wi.WorkstreamId).OnDelete(DeleteBehavior.Cascade);
    });
}
```

### Storage Strategy

**Engine:** SQLite via `Microsoft.EntityFrameworkCore.Sqlite` 8.0.x

**Location:** `%LOCALAPPDATA%\ReportingDashboard\dashboard.db`

**Pragmas (applied on startup):**

```sql
PRAGMA journal_mode=WAL;       -- Concurrent reads during sync writes
PRAGMA cache_size=-8000;       -- 8MB page cache
PRAGMA synchronous=NORMAL;     -- Balance safety and speed
PRAGMA foreign_keys=ON;        -- Enforce FK constraints
```

**Auto-migration on startup:**

```csharp
Directory.CreateDirectory(Path.GetDirectoryName(GetDbPath())!);
await db.Database.MigrateAsync();
```

**Estimated volumes:** 3–10 workstreams, 10–50 milestones, 300–6,000 work items. Total DB file <5MB.

**Seed on first run:**

```csharp
if (!await db.Workstreams.AnyAsync())
    await SeedData.SeedAsync(db);
```

---

## API Contracts

### `GET /api/roadmap`

Full dashboard payload. Frontend renders all three sections from this single response.

**Request:** No parameters.

**Response (200 OK):**

```typescript
interface RoadmapResponse {
    workstreams: { id: string; name: string; color: string; sortOrder: number }[];
    milestones: { id: string; workstreamId: string; name: string; date: string; type: string; subType: string }[];
    workItems: { id: string; title: string; status: string; month: string; workstreamId: string; adoUrl: string }[];
    months: { name: string; isCurrent: boolean }[];
    dateRange: { start: string; end: string };
    lastSyncUtc: string | null;
}
```

```csharp
public sealed record RoadmapDto
{
    public required List<WorkstreamDto> Workstreams { get; init; }
    public required List<MilestoneDto> Milestones { get; init; }
    public required List<WorkItemSummaryDto> WorkItems { get; init; }
    public required List<MonthColumnDto> Months { get; init; }
    public required DateRangeDto DateRange { get; init; }
    public string? LastSyncUtc { get; init; }
}
```

**Behavior:**
- Empty database → 200 OK with empty arrays, `lastSyncUtc: null`
- Cached in `IMemoryCache` with key `"roadmap"`, 60-second TTL
- `months` array computed at request time from `Dashboard:MonthCount` config
- Response size: <50KB for 500 work items

---

### `GET /api/workitems?status=X&month=Y`

Filtered work items for drill-down panel.

**Parameters:**

| Parameter | Required | Valid Values |
|---|---|---|
| `status` | Yes | `Shipped`, `InProgress`, `Carryover`, `Blocked` (case-insensitive) |
| `month` | Yes | `Jan`–`Dec` (3-letter abbreviation) |

**Response (200 OK):**

```json
[
    { "id": "12345", "title": "Role-based access matrix", "status": "InProgress", "month": "Apr",
      "adoUrl": "https://dev.azure.com/msazure/One/_workitems/edit/12345" }
]
```

**Response (400 Bad Request):**

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "Validation Error",
    "status": 400,
    "errors": {
        "status": ["Invalid status 'Doing'. Valid values: Shipped, InProgress, Carryover, Blocked"]
    }
}
```

---

### `POST /api/sync`

Trigger ADO data sync. Empty request body.

**Response (200 OK):**

```json
{ "itemCount": 342, "syncedAtUtc": "2026-05-01T06:15:00.0000000Z" }
```

**Error Responses:**

| Status | Condition | Response |
|---|---|---|
| `400` | PAT not configured | `{ "error": "ADO PAT not configured. Set via: dotnet user-secrets set \"Ado:Pat\" \"<pat>\"" }` |
| `401` | Invalid/expired PAT | `{ "error": "Invalid or expired ADO Personal Access Token. Generate a new PAT with 'Work Items (Read)' scope." }` |
| `502` | ADO unreachable | `{ "error": "Could not reach Azure DevOps. Check your network connection and VPN status." }` |
| `500` | Unexpected error | `{ "error": "Sync failed unexpectedly. Check logs at %LOCALAPPDATA%\\ReportingDashboard\\logs\\" }` |

**Side effects:** Clears `IMemoryCache` key `"roadmap"` after successful sync.

---

### API Summary

| Endpoint | Method | Success | Errors | Cache |
|---|---|---|---|---|
| `/api/roadmap` | GET | 200 `RoadmapDto` | None (always 200) | `IMemoryCache` 60s |
| `/api/workitems` | GET | 200 `WorkItemDto[]` | 400 invalid params | None |
| `/api/sync` | POST | 200 `SyncResultDto` | 400/401/502/500 | Invalidates roadmap |

### JSON Serialization

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});
```

---

### ADO REST API Contracts (External)

**WIQL:** `POST https://dev.azure.com/{org}/{project}/_apis/wit/wiql?api-version=7.1` — returns work item IDs matching the configured area path.

**Batch Fetch:** `POST https://dev.azure.com/{org}/{project}/_apis/wit/workitemsbatch?api-version=7.1` — fetches details for up to 200 IDs per call. Fields: `System.Id`, `System.Title`, `System.State`, `System.IterationPath`, `System.Tags`, `System.ChangedDate`.

---

## Infrastructure Requirements

### Hosting

| Requirement | Specification |
|---|---|
| Platform | Windows 10/11 x64 |
| Runtime | .NET 8.0 LTS (bundled in self-contained EXE) |
| Ports | `localhost:5000` (API), `localhost:5173` (Vite dev, development only) |
| Network binding | `Kestrel.ListenLocalhost(5000)` — rejects non-loopback |
| Memory | <50MB working set |
| Disk | <40MB EXE + <5MB database |

### Networking

| Channel | Protocol | Direction | Auth |
|---|---|---|---|
| Browser → API | HTTP/1.1 | localhost | None |
| Browser → Vite | HTTP/1.1 + WebSocket | localhost (dev only) | None |
| API → ADO | HTTPS/1.1 | Outbound | Basic Auth (PAT) |

**CORS:** Not required. Vite proxies `/api/*` in development; same-origin in production.

### Storage

| Artifact | Location | Size |
|---|---|---|
| SQLite database | `%LOCALAPPDATA%\ReportingDashboard\dashboard.db` | <5MB |
| DPAPI credential | `%LOCALAPPDATA%\ReportingDashboard\cred.dat` | <1KB |
| Log files | `%LOCALAPPDATA%\ReportingDashboard\logs\log-{date}.txt` | <10MB/day |
| Self-contained EXE | `publish/` output | <40MB |

### CI/CD

**GitHub Actions workflow with parallel jobs:**

```yaml
name: CI
on: [push, pull_request]
jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet test --no-build -c Release --collect:"XPlat Code Coverage"

  frontend:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: src/ReportingDashboard.Client
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with: { node-version: '20', cache: 'npm', cache-dependency-path: src/ReportingDashboard.Client/package-lock.json }
      - run: npm ci
      - run: npx tsc --noEmit
      - run: npm run lint
      - run: npm run test -- --reporter=verbose --coverage
      - run: npm run build

  publish:
    runs-on: windows-latest
    needs: [backend, frontend]
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - uses: actions/setup-node@v4
        with: { node-version: '20' }
      - run: dotnet publish src/ReportingDashboard.Api/ReportingDashboard.Api.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
      - run: |
          $size = (Get-Item publish/ReportingDashboard.Api.exe).Length / 1MB
          if ($size -gt 40) { throw "EXE exceeds 40MB: $([math]::Round($size,1))MB" }
      - uses: actions/upload-artifact@v4
        with: { name: ReportingDashboard-win-x64, path: publish/ReportingDashboard.Api.exe }
```

**Timing:** Backend ~90s, Frontend ~60s (parallel). Total <2 minutes.

### Monitoring & Diagnostics

No cloud monitoring. Serilog structured logging with file sink:

```csharp
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .Destructure.ByTransforming<AdoSettings>(s => new { s.Organization, s.Project, Pat = "***REDACTED***" }));
```

| Event | Level |
|---|---|
| App startup, DB migrated/seeded, sync completed | Information |
| Sync failed (auth) | Warning |
| Sync failed (network/unexpected) | Error |
| WIQL executed, batch fetched, cache hit/miss | Debug |

---

## Technology Stack Decisions

### Frontend

| Component | Technology | Justification |
|---|---|---|
| Language | **TypeScript** ~5.4+ | Constraint C6: "TypeScript preferred". Strict mode catches errors at compile time. |
| Build | **Vite** ~5.x | Dev server with HMR on :5173; production builds to `dist/`. Zero-config for TypeScript. |
| Timeline visualization | **D3.js** ~7.9 | `scaleTime()` for date→pixel mapping; data joins for declarative SVG. ~17KB gzipped (tree-shaken). Only 5 modules needed. |
| Heatmap layout | **CSS Grid + Flexbox** | Design reference already uses CSS Grid. `grid-template-columns: 160px repeat(4,1fr)`. Zero library needed. |
| Framework | **None (Vanilla TS)** | Single-view dashboard with ~300 DOM nodes. React/Vue/Angular add 40–100KB overhead with no benefit. |
| CSS | **Plain CSS** | Ported directly from `OriginalDesignConcept.html`. Tailwind/Bootstrap would fight the custom grid. |
| Font | **Segoe UI** (system) | Windows system font. Zero web-font loading. Arial as CSS fallback. |

**Why not Canvas/Phaser.js:** The design uses CSS Grid with `::before` pseudo-elements, rich text with mixed font weights, and SVG `<polygon>` with drop-shadow filters. Canvas cannot render CSS pseudo-elements or native text wrapping. Phaser adds 1.2MB of unused game-engine code. SVG + DOM is the only approach that matches the design without reimplementation.

### Backend

| Component | Technology | Justification |
|---|---|---|
| Runtime | **.NET 8.0 LTS** | Constraint C5. Support through November 2026. |
| Web framework | **ASP.NET Core Minimal API** | 3 endpoints, ~400 LOC total. <200ms startup. Blazor fights D3/TypeScript/SVG. Razor Pages adds unnecessary ceremony. |
| JSON | **System.Text.Json** (in-box) | Source generators for AOT-friendly serialization. |
| HTTP client | **IHttpClientFactory** (in-box) | Pooled connections for ADO REST API. |
| Logging | **Serilog.AspNetCore** ~8.0 | Structured logging with PAT exclusion policy. File sink in `%LOCALAPPDATA%`. |
| OpenAPI | **Swashbuckle.AspNetCore** ~6.5 | Swagger UI at `/swagger` for development. |
| SPA proxy | **Microsoft.AspNetCore.SpaProxy** 8.0 | Proxies to Vite dev server during development. Official pattern from `dotnet new react`. |

### Data

| Component | Technology | Justification |
|---|---|---|
| Engine | **SQLite** (bundled) | Zero-install. Single `.db` file. Relational queries (GROUP BY for heatmap). WAL for concurrent reads. |
| ORM | **EF Core Sqlite** 8.0 | LINQ queries, migrations, strongly-typed entities. |
| PAT encryption | **System.Security.Cryptography.ProtectedData** 8.0 | DPAPI; `CurrentUser` scope; Windows-only (acceptable per C1). |

### Testing

| Layer | Tool | Notes |
|---|---|---|
| Backend unit/integration | **xUnit** ~2.8 + **FluentAssertions** ~6.12 + **NSubstitute** ~5.1 | `WebApplicationFactory` for API integration tests. |
| HTTP mocking | **MockHttpMessageHandler** | Mock ADO REST API responses. |
| Frontend unit | **Vitest** ~1.6 + **jsdom** ~24 | Vite-native. DOM testing environment. |
| Coverage | **@vitest/coverage-v8** + **XPlat Code Coverage** | >70% line coverage target for both stacks. |

---

## Security Considerations

### Threat Model

| Asset | Sensitivity | Primary Threat |
|---|---|---|
| ADO PAT | **Critical** | Accidental commit; leaked via logs |
| SQLite database | Low-Medium | Another local user reads work item titles |
| HTTP API | Low | Malicious local process queries localhost |
| Frontend assets | None | Static files, no secrets |

### Authentication

**Not implemented. Not needed.** The tool binds exclusively to `localhost`. Any process that can reach `localhost:5000` already has full access to the user's machine. Adding JWT/cookies adds complexity with zero security benefit.

### PAT Protection (Three Layers)

| Layer | Location | Encryption |
|---|---|---|
| .NET User Secrets (dev) | `%APPDATA%\Microsoft\UserSecrets\{guid}\` | OS ACLs |
| Environment variable | Process environment | Process-scoped |
| DPAPI `cred.dat` (dist) | `%LOCALAPPDATA%\ReportingDashboard\` | AES via Windows DPAPI, `CurrentUser` scope |

Resolution order: User Secrets → env var → DPAPI. First non-empty wins.

```csharp
public class CredentialStore
{
    private static readonly byte[] Entropy = "ReportingDashboard-v1"u8.ToArray();

    public void StorePat(string pat)
    {
        var encrypted = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(pat), Entropy, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(CredPath, encrypted);
    }

    public string? LoadPat()
    {
        if (!File.Exists(CredPath)) return null;
        try {
            var plain = ProtectedData.Unprotect(
                File.ReadAllBytes(CredPath), Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plain);
        } catch (CryptographicException) { return null; }
    }
}
```

### Network Isolation

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // 127.0.0.1 + [::1] only
});
```

Hardcoded. Cannot be overridden by config.

### Input Validation

- `GET /api/workitems` validates `status` and `month` against closed allowlists. Invalid values → HTTP 400.
- EF Core parameterizes all queries (no SQL injection).
- Frontend uses `textContent` (not `innerHTML`) for all user-sourced text (no XSS).
- ADO URLs are constructed server-side from integer IDs (no user input in URL construction).
- All ADO links rendered with `rel="noopener noreferrer"`.

### Source Control

```gitignore
*.db
*.db-wal
*.db-shm
cred.dat
appsettings.*.json
!appsettings.json
!appsettings.Development.json
```

### Security Checklist

| Control | Implementation |
|---|---|
| PAT never in source control | `.gitignore` + User Secrets outside repo |
| PAT never in logs | Serilog destructure policy masks `Ado:Pat` |
| PAT encrypted at rest | DPAPI with `DataProtectionScope.CurrentUser` |
| Localhost-only binding | `Kestrel.ListenLocalhost(5000)` hardcoded |
| Input validation | Allowlist for `status` and `month` |
| No SQL injection | EF Core parameterized queries |
| No XSS | `textContent` for all text; JSON-only API |
| Minimal PAT scope | `Work Items (Read)` only |
| Database user-scoped | `%LOCALAPPDATA%` with Windows ACLs |

---

## Scaling Strategy

### Design Constraints

This is a **single-user, single-machine, local-only tool**. Traditional scaling (horizontal, load balancing, multi-region) does not apply. The strategy ensures responsiveness as data grows within the design envelope.

| Dimension | Limit | Approach |
|---|---|---|
| Concurrent users | 1 | Not applicable |
| Work items | ~10,000 | SQLite indexes; cell truncation |
| Workstreams | ~20 | SVG height adjustment |
| Months displayed | 4–12 | Configurable `Dashboard:MonthCount` |
| ADO sync volume | ~2,000 | Batch chunking; timeout config |

### Caching

| Layer | Mechanism | TTL | Invalidation |
|---|---|---|---|
| Backend | `IMemoryCache` on `GET /api/roadmap` | 60 seconds | `cache.Remove("roadmap")` after sync |
| Frontend | In-memory `RoadmapData` in closure | Until sync/reload | Re-fetch after sync |
| Static assets | Vite content-hashed filenames | Indefinite | New build = new hashes |
| SQLite | Page cache `PRAGMA cache_size=-8000` | Session lifetime | Automatic LRU |

### Bottleneck Mitigations

| Bottleneck | Mitigation |
|---|---|
| ADO sync latency (>500 items) | Batch chunking (200/request); 30s timeout; progress logging |
| Large heatmap cells (>50 items) | Truncate to 15 items with "+N more" link; drill-down shows full list |
| SQLite write lock during sync | WAL mode enables concurrent reads during writes |
| Frontend DOM explosion (>2K items) | Cell truncation; timeline milestones bounded by workstream count |

**Cell truncation implementation:**

```typescript
const MAX_ITEMS_PER_CELL = 15;
const displayed = items.slice(0, MAX_ITEMS_PER_CELL);
if (items.length > MAX_ITEMS_PER_CELL) {
    // Append "+N more" indicator
}
```

### Performance Budget

| Operation | Target |
|---|---|
| `GET /api/roadmap` (cached) | <5ms p99 |
| `GET /api/roadmap` (DB query) | <50ms p99 |
| `GET /api/workitems` (filtered) | <10ms p99 |
| `POST /api/sync` (500 items) | <10s end-to-end |
| Frontend full render | <50ms |
| Backend cold start | <500ms |
| Vite HMR update | <500ms |

---

## Risks & Mitigations

### R-1: D3.js Learning Curve — Medium Severity, Medium Probability

**Impact:** Phase 1 timeline takes 2× estimated time.

**Mitigations:**
1. Provide annotated `timeline.ts` reference implementation as Phase 1 spike deliverable.
2. Limit to 5 D3 modules: `d3-scale`, `d3-selection`, `d3-time`, `d3-time-format`, `d3-axis`.
3. Design reference provides exact SVG structure to replicate element-by-element.
4. **Fallback:** Raw `document.createElementNS()` + hand-written `dateToPixel()` if D3 proves too costly.

### R-2: Vite + MSBuild Integration Fragility — Medium Severity, Low Probability

**Impact:** `dotnet publish` produces EXE without frontend assets.

**Mitigations:**
1. Use official `Microsoft.AspNetCore.SpaProxy` pattern (tested by .NET team against every SDK release).
2. Pin Node.js version (`20`) in CI via `actions/setup-node`.
3. CI publish job verifies `wwwroot/index.html` exists in output.
4. **Fallback:** Decouple builds — run `npm run build` as separate CI step.

### R-3: ADO State Mapping Ambiguity — Medium Severity, High Probability

**Impact:** Work items miscategorized in heatmap.

**Mitigations:**
1. **Tag-first mapping:** `[blocked]` and `[carryover]` tags take highest priority.
2. **Configurable state lists** in `appsettings.json`:
   ```json
   { "ShippedStates": ["Closed", "Resolved", "Done"], "InProgressStates": ["Active", "Committed"] }
   ```
3. **Preserve raw ADO fields** (`AdoState`, `AdoTags`, `AdoIterationPath`) for debugging.
4. **Require stakeholder decision** (OQ-1) before Phase 3 implementation.

### R-4: ADO API Rate Limiting — Low Severity, Low Probability

**Impact:** HTTP 429 during sync.

**Mitigations:**
1. Batch chunking: 200 items/request. 500 items = 3–4 requests.
2. Configurable inter-batch `Task.Delay(100ms)`.
3. 429 retry with `Retry-After` header backoff.
4. WIQL `ChangedDate >= @Today - 180` filter reduces item count.

### R-5: SQLite Corruption — Low Severity, Low Probability

**Impact:** Dashboard fails to load.

**Mitigations:**
1. WAL mode provides crash safety.
2. `PRAGMA synchronous=NORMAL` balances safety and speed.
3. **Recovery:** Delete `.db` file → restart → auto-seed → re-sync from ADO.
4. Database is a cache, not source of truth. All data is reproducible from ADO.

### R-6: D3.js Breaking Changes — Low Severity, Low Probability

**Mitigations:** Pin exact version `"d3": "7.9.0"`; commit `package-lock.json`; minimal surface area (~50 lines affected).

### R-7: Database Encryption Deferred — Low Severity, Medium Probability

**Mitigations:** Architecture supports drop-in SQLCipher (`SQLitePCLRaw.bundle_e_sqlcipher` + `Password=` connection string). Data is low-sensitivity (work item titles, not PII).

### R-8: .NET 8 LTS End-of-Life — Low Severity, Low Probability

**Mitigations:** 6-month runway from May 2026. Migration to .NET 10 is a `TargetFramework` change + package bumps.

### R-9: PAT Expiration Without Warning — Medium Severity, High Probability

**Impact:** Sync fails; dashboard shows stale data.

**Mitigations:**
1. Clear 401 error message with exact remediation command.
2. Dashboard renders existing data — error only affects sync.
3. `lastSyncUtc` in header shows data freshness.

### R-10: ADO Org/Project Misconfiguration — Low Severity, Medium Probability

**Mitigations:**
1. Sync returns `itemCount: 0` — obvious signal.
2. Log exact WIQL query at Debug level for copy-paste verification.
3. README documents finding correct org/project/area path from ADO URL.

### Risk Summary

| ID | Risk | Severity | Probability | Blocks |
|---|---|---|---|---|
| R-1 | D3.js learning curve | Medium | Medium | Phase 1 |
| R-2 | Vite + MSBuild integration | Medium | Low | Phase 1 |
| R-3 | ADO state mapping ambiguity | Medium | High | Phase 3 |
| R-4 | ADO API rate limiting | Low | Low | Phase 3 |
| R-5 | SQLite corruption | Low | Low | Phase 2 |
| R-6 | D3.js breaking changes | Low | Low | Phase 1 |
| R-7 | Database encryption deferred | Low | Medium | Phase 2 |
| R-8 | .NET 8 LTS EOL | Low | Low | None |
| R-9 | PAT expiration | Medium | High | Phase 3 |
| R-10 | ADO misconfiguration | Low | Medium | Phase 3 |

### Error Response Principle

**No error condition blanks the dashboard.** The user always sees the last successfully loaded data. Errors appear as overlay notifications with remediation steps.

```
Sync clicked → PAT missing?     → 400 + fix command → data visible
             → PAT expired?     → 401 + fix command → data visible
             → ADO unreachable? → 502 + check network → data visible
             → Rate limited?    → retry once with backoff → data visible
             → Success          → re-render with fresh data
```