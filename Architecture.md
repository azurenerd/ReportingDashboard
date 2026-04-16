# Architecture

**Project:** Executive Reporting Dashboard
**Version:** 1.0
**Date:** April 16, 2026
**Stack:** C# .NET 8 · Blazor Server · Local Only · .sln Structure

---

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server application that renders a pixel-perfect 1920×1080 project status visualization optimized for PowerPoint screenshot capture. It reads all display data from a local `data.json` file and renders three visual sections: a project header with legend, an SVG milestone timeline, and a color-coded monthly execution heatmap.

### Architectural Goals

| Goal | Approach |
|------|----------|
| **Screenshot fidelity** | Fixed 1920×1080 layout with no scrollbars; CSS ported pixel-for-pixel from `OriginalDesignConcept.html` |
| **Zero infrastructure** | Runs locally via `dotnet run`; no cloud, no database, no auth |
| **Data-driven rendering** | All visible content sourced from `data.json`; zero hardcoded project data |
| **Minimal complexity** | Single .csproj, ≤10 source files, zero NuGet dependencies beyond the default Blazor template |
| **Sub-2-minute updates** | Edit JSON → refresh browser → screenshot → paste into PowerPoint |

### Architecture Pattern

**Model → Service → Page** — the simplest viable pattern for a read-only, single-page application:

```
data.json ──→ DashboardDataService ──→ DashboardData (records) ──→ Dashboard.razor ──→ Browser
```

No layered architecture, no repository pattern, no CQRS, no mediator. This is a screenshot generator, not an enterprise application.

---

## System Components

### 1. `Program.cs` — Application Host

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Configure Kestrel, register services, set up Blazor Server middleware |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService`, ASP.NET Core framework |
| **Data** | Reads `appsettings.json` for data file path configuration |

**Key behaviors:**
- Registers `DashboardDataService` as a **singleton** (data is read-only, shared across all circuits)
- Configures Kestrel to bind to `localhost` only (`https://localhost:5001`)
- Adds Blazor Server services and maps the Blazor hub
- Removes default Blazor template pages (Counter, Weather)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

### 2. `Models/DashboardData.cs` — Data Model Records

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Strongly-typed representation of `data.json` schema |
| **Interfaces** | Immutable C# records; no methods beyond computed properties |
| **Dependencies** | `System.Text.Json.Serialization` for `[JsonPropertyName]` attributes |
| **Data** | Deserialized from `data.json` |

**Records defined:**
- `DashboardData` — root object (title, subtitle, backlogUrl, milestones, heatmap, timelineMonths, currentMonth)
- `MilestoneTrack` — one track (id, label, color, events[])
- `MilestoneEvent` — one event on a track (date, type, label)
- `HeatmapData` — heatmap container (columns[], rows[])
- `HeatmapRow` — one status category (category, items dictionary)
- `CategoryStyle` — static helper mapping category names to color tokens

### 3. `Services/DashboardDataService.cs` — Data Access

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Read, deserialize, validate, and cache `data.json`; optionally watch for file changes |
| **Interfaces** | `Task<DashboardData?> GetDashboardDataAsync()`, `string? GetError()` |
| **Dependencies** | `System.Text.Json`, `System.IO.FileSystemWatcher`, `IWebHostEnvironment` |
| **Data** | Reads `wwwroot/data/data.json` from disk |

**Key behaviors:**
- Reads `data.json` on first request and caches the deserialized `DashboardData` instance
- On deserialization failure: captures the error message, returns `null`, and exposes the error via `GetError()`
- On missing file: returns `null` with a descriptive error string
- On missing required fields: returns a `DashboardData` with sensible defaults (empty strings, empty arrays) and logs console warnings
- **Optional:** `FileSystemWatcher` monitors `data.json` for changes and invalidates the cache, triggering a re-read on next request
- Thread-safe via `SemaphoreSlim` for concurrent Blazor circuits (even though single-user, defensive coding costs nothing)

```csharp
public class DashboardDataService
{
    private readonly string _dataFilePath;
    private DashboardData? _cachedData;
    private string? _error;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<DashboardData?> GetDashboardDataAsync() { /* ... */ }
    public string? GetError() => _error;
}
```

### 4. `Components/Pages/Dashboard.razor` — The Single Page

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Render the entire dashboard: header, timeline SVG, heatmap grid |
| **Interfaces** | Blazor component lifecycle (`OnInitializedAsync`) |
| **Dependencies** | `DashboardDataService` (injected) |
| **Data** | `DashboardData` model instance |

**Key behaviors:**
- Calls `DashboardDataService.GetDashboardDataAsync()` in `OnInitializedAsync`
- If data is `null`, renders a centered error message from `GetError()`
- If data is valid, renders three sections: Header, Timeline, Heatmap
- Contains C# helper methods for SVG coordinate calculations:
  - `DateToX(DateOnly date)` — maps a date to an X pixel position within the 1560px SVG width
  - `GetTrackY(int trackIndex, int trackCount)` — evenly distributes tracks across 185px SVG height
  - `DiamondPoints(double cx, double cy, double size)` — generates polygon points for diamond shapes
  - `GetCategoryStyle(string category)` — returns color tokens for a heatmap category

### 5. `Components/Pages/Dashboard.razor.css` — Scoped Styles

| Attribute | Value |
|-----------|-------|
| **Responsibility** | All CSS for the dashboard, ported from `OriginalDesignConcept.html` |
| **Interfaces** | Blazor CSS isolation (auto-scoped via `b-{hash}` attribute) |
| **Dependencies** | None |
| **Data** | CSS custom properties for the color palette |

### 6. `wwwroot/css/app.css` — Global Styles

| Attribute | Value |
|-----------|-------|
| **Responsibility** | CSS reset, font stack declaration, body fixed dimensions |
| **Interfaces** | Loaded via `<link>` in `App.razor` |
| **Dependencies** | None |

**Key rules:**
```css
*, *::before, *::after { margin: 0; padding: 0; box-sizing: border-box; }
body { width: 1920px; height: 1080px; overflow: hidden;
       font-family: 'Segoe UI', Arial, sans-serif; color: #111;
       display: flex; flex-direction: column; background: #FFFFFF; }
a { color: #0078D4; text-decoration: none; }
```

### 7. `wwwroot/data/data.json` — Dashboard Data Source

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Single source of truth for all dashboard content |
| **Interfaces** | JSON file read by `DashboardDataService` |
| **Dependencies** | None |
| **Data** | Title, subtitle, backlog URL, timeline months, current month, milestones, heatmap |

### 8. `Components/App.razor` — Root Component

| Attribute | Value |
|-----------|-------|
| **Responsibility** | HTML document shell (`<html>`, `<head>`, `<body>`), CSS/JS references |
| **Interfaces** | Blazor root component |
| **Dependencies** | `Routes.razor` |

### 9. `Components/Routes.razor` — Router

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Route `/` to `Dashboard.razor` |
| **Interfaces** | Blazor `<Router>` component |
| **Dependencies** | `Dashboard.razor` (via route matching) |

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│  Developer's Machine                                            │
│                                                                 │
│  ┌──────────────┐    read     ┌─────────────────────────┐       │
│  │  data.json   │───────────→│  DashboardDataService    │       │
│  │  (wwwroot/   │            │  - LoadAsync()           │       │
│  │   data/)     │  ◄─watch─  │  - Cache + error state   │       │
│  └──────────────┘  (optional)│  - FileSystemWatcher     │       │
│                              └────────────┬─────────────┘       │
│                                           │                     │
│                              GetDashboardDataAsync()            │
│                                           │                     │
│                                           ▼                     │
│                              ┌─────────────────────────┐       │
│                              │  Dashboard.razor         │       │
│                              │  - OnInitializedAsync()  │       │
│                              │  - Render HTML + SVG     │       │
│                              │  - DateToX(), GetTrackY()│       │
│                              └────────────┬─────────────┘       │
│                                           │                     │
│                                   SignalR (WebSocket)           │
│                                           │                     │
│                                           ▼                     │
│                              ┌─────────────────────────┐       │
│                              │  Browser (Chrome/Edge)   │       │
│                              │  1920×1080 @ 100% zoom   │       │
│                              └────────────┬─────────────┘       │
│                                           │                     │
│                                      Screenshot                 │
│                                           │                     │
│                                           ▼                     │
│                              ┌─────────────────────────┐       │
│                              │  PowerPoint Slide        │       │
│                              │  (16:9 aspect ratio)     │       │
│                              └──────────────────────────┘       │
└─────────────────────────────────────────────────────────────────┘
```

### Communication Patterns

| From | To | Mechanism | When |
|------|----|-----------|------|
| `Program.cs` | `DashboardDataService` | DI registration (Singleton) | App startup |
| `Dashboard.razor` | `DashboardDataService` | Dependency injection, `await GetDashboardDataAsync()` | `OnInitializedAsync` |
| `DashboardDataService` | `data.json` | `File.ReadAllTextAsync()` | First request or cache invalidation |
| `FileSystemWatcher` | `DashboardDataService` | File change callback → cache invalidation | `data.json` saved to disk |
| Blazor Server | Browser | SignalR WebSocket (DOM diffs) | On render |
| Browser | Kestrel | HTTPS `localhost:5001` | Page navigation |

### Rendering Pipeline

1. User navigates to `https://localhost:5001`
2. Kestrel serves `App.razor` → `Routes.razor` → `Dashboard.razor`
3. `Dashboard.razor.OnInitializedAsync()` calls `DashboardDataService.GetDashboardDataAsync()`
4. Service reads `data.json` (or returns cached copy), deserializes to `DashboardData`
5. Razor component renders three sections using `@foreach` loops and computed SVG coordinates
6. Blazor Server pushes the rendered DOM to the browser via SignalR
7. Browser paints the 1920×1080 fixed layout with no scrollbars

### Error Flow

1. `DashboardDataService` catches `JsonException`, `FileNotFoundException`, or `IOException`
2. Sets `_error` string with a user-friendly message; `_cachedData` remains `null`
3. `Dashboard.razor` checks `Data == null`, renders centered error message instead of dashboard
4. No stack traces, no partial renders, no blank pages

---

## Data Model

### Entity: `DashboardData` (Root)

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `title` | `string` | Yes | `""` | Project name displayed in header H1 |
| `subtitle` | `string` | Yes | `""` | Team/workstream/date line below title |
| `backlogUrl` | `string?` | No | `null` | ADO backlog hyperlink; hidden if null/empty |
| `timelineMonths` | `string[]` | Yes | `[]` | Month labels for SVG gridlines (e.g., `["Jan","Feb",...]`) |
| `currentMonth` | `string` | Yes | `""` | Month name to highlight in heatmap and position NOW line |
| `milestones` | `MilestoneTrack[]` | Yes | `[]` | 1–5 milestone tracks for the timeline |
| `heatmap` | `HeatmapData` | Yes | Empty | Monthly execution heatmap data |

### Entity: `MilestoneTrack`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Track identifier (e.g., "M1", "M2") |
| `label` | `string` | Yes | Track description (e.g., "Chatbot & MS Role") |
| `color` | `string` | Yes | Hex color for track line and label (e.g., "#0078D4") |
| `events` | `MilestoneEvent[]` | Yes | Chronological events on this track |

### Entity: `MilestoneEvent`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `date` | `DateOnly` | Yes | Calendar date for X-axis positioning (ISO 8601: `"2026-03-26"`) |
| `type` | `string` | Yes | One of: `"checkpoint"`, `"checkpoint-minor"`, `"poc"`, `"production"` |
| `label` | `string` | Yes | Text rendered near the event shape (e.g., "Mar 26 PoC") |

### Entity: `HeatmapData`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `columns` | `string[]` | Yes | Month names for column headers (e.g., `["Jan","Feb","Mar","Apr"]`) |
| `rows` | `HeatmapRow[]` | Yes | Exactly 4 rows: shipped, in-progress, carryover, blockers |

### Entity: `HeatmapRow`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `category` | `string` | Yes | One of: `"shipped"`, `"in-progress"`, `"carryover"`, `"blockers"` |
| `items` | `Dictionary<string, string[]>` | Yes | Month name → array of work item descriptions |

### Static Mapping: `CategoryStyle`

| Category | Header Text | Header BG | Cell BG | Cell Accent | Bullet | CSS Prefix |
|----------|-------------|-----------|---------|-------------|--------|------------|
| `shipped` | `#1B7A28` | `#E8F5E9` | `#F0FBF0` | `#D8F2DA` | `#34A853` | `ship-` |
| `in-progress` | `#1565C0` | `#E3F2FD` | `#EEF4FE` | `#DAE8FB` | `#0078D4` | `prog-` |
| `carryover` | `#B45309` | `#FFF8E1` | `#FFFDE7` | `#FFF0B0` | `#F4B400` | `carry-` |
| `blockers` | `#991B1B` | `#FEF2F2` | `#FFF5F5` | `#FFE4E4` | `#EA4335` | `block-` |

### Storage

- **Format:** JSON flat file (`data.json`)
- **Location:** `wwwroot/data/data.json` (served as static file; also read by service via filesystem)
- **Schema enforcement:** C# record types with `System.Text.Json` deserialization; `[JsonPropertyName]` attributes for explicit mapping
- **No database.** No ORM. No migration framework.

### JSON Schema (Complete Example)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "timelineMonths": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"],
  "currentMonth": "Apr",
  "milestones": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
        { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
      ]
    },
    {
      "id": "M2",
      "label": "PDS & Data Inventory",
      "color": "#00897B",
      "events": [
        { "date": "2025-12-19", "type": "checkpoint", "label": "Dec 19" },
        { "date": "2026-02-11", "type": "checkpoint", "label": "Feb 11" },
        { "date": "2026-03-10", "type": "checkpoint-minor", "label": "" },
        { "date": "2026-03-15", "type": "checkpoint-minor", "label": "" },
        { "date": "2026-03-20", "type": "poc", "label": "Mar 20 PoC" },
        { "date": "2026-05-15", "type": "production", "label": "May Prod" }
      ]
    },
    {
      "id": "M3",
      "label": "Auto Review DFD",
      "color": "#546E7A",
      "events": [
        { "date": "2026-04-15", "type": "poc", "label": "Apr 15 PoC" },
        { "date": "2026-06-01", "type": "production", "label": "Jun Prod" }
      ]
    }
  ],
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "rows": [
      {
        "category": "shipped",
        "items": {
          "Jan": ["Auth module v1", "Config pipeline"],
          "Feb": ["Data connector beta"],
          "Mar": ["PDS integration", "Role mapping v2"],
          "Apr": ["Chatbot PoC demo"]
        }
      },
      {
        "category": "in-progress",
        "items": {
          "Jan": ["Chatbot prototype"],
          "Feb": ["PDS schema design", "Auth hardening"],
          "Mar": ["Chatbot UX iteration"],
          "Apr": ["DFD auto-review engine", "Production hardening"]
        }
      },
      {
        "category": "carryover",
        "items": {
          "Jan": [],
          "Feb": ["Perf benchmarks"],
          "Mar": ["Perf benchmarks", "Logging overhaul"],
          "Apr": ["Logging overhaul"]
        }
      },
      {
        "category": "blockers",
        "items": {
          "Jan": [],
          "Feb": [],
          "Mar": ["ICM: PDS API rate limit"],
          "Apr": ["ICM: PDS API rate limit", "Dependency on Team X"]
        }
      }
    ]
  }
}
```

---

## API Contracts

This application has **no REST API endpoints**. It is a Blazor Server application that communicates between server and browser exclusively via SignalR (managed by the Blazor framework). There are no custom controllers, no minimal API routes, and no gRPC services.

### Internal Service Interface

The only "contract" is the internal C# service interface used by the Razor component:

```csharp
// DashboardDataService (registered as Singleton)
public class DashboardDataService
{
    /// Returns the deserialized dashboard data, or null if loading failed.
    public Task<DashboardData?> GetDashboardDataAsync();

    /// Returns the error message if data loading failed, or null if successful.
    public string? GetError();

    /// Forces a re-read of data.json on the next GetDashboardDataAsync() call.
    public void InvalidateCache();
}
```

### Error Handling Contract

| Condition | Service Behavior | Component Behavior |
|-----------|-----------------|-------------------|
| `data.json` missing | `GetError()` returns `"Unable to load dashboard data. File not found: {path}"` | Renders centered error message |
| `data.json` unreadable (I/O) | `GetError()` returns `"Unable to load dashboard data. File read error: {message}"` | Renders centered error message |
| Invalid JSON syntax | `GetError()` returns `"Unable to load dashboard data. JSON parse error: {message}"` | Renders centered error message |
| Valid JSON, missing fields | Returns `DashboardData` with defaults; logs `Console.WriteLine` warnings | Renders partial dashboard (empty sections for missing data) |
| Valid JSON, all fields present | Returns fully populated `DashboardData`; `GetError()` returns `null` | Renders complete dashboard |

### Static File Routes (Framework-Managed)

| Path | Serves |
|------|--------|
| `/` | `Dashboard.razor` (via Blazor router) |
| `/_blazor` | SignalR hub (managed by framework) |
| `/css/app.css` | Global stylesheet |
| `/data/data.json` | Raw JSON file (accessible but not required by browser) |
| `/{project}.styles.css` | Blazor CSS isolation bundle |

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|---------------|
| **Operating System** | Windows 10/11 (primary); macOS/Linux supported if Segoe UI installed |
| **Runtime** | .NET 8.0 LTS SDK (8.0.x) |
| **Browser** | Chrome 120+ or Edge 120+ |
| **Display** | 1920×1080 resolution at 100% zoom for screenshot capture |
| **Disk** | < 50MB total (SDK not included) |
| **Memory** | < 100MB runtime footprint |
| **Network** | Loopback only (`localhost:5001`); no external network required |

### Hosting

| Concern | Configuration |
|---------|---------------|
| **Web server** | Kestrel (built into ASP.NET Core) |
| **Binding** | `https://localhost:5001` (configurable in `launchSettings.json`) |
| **HTTPS** | Dev certificate via `dotnet dev-certs https --trust` |
| **Process** | Foreground console process via `dotnet run` or `dotnet watch` |

### Storage

| Item | Location | Size |
|------|----------|------|
| `data.json` | `wwwroot/data/data.json` | < 100KB |
| Application files | Project directory | < 1MB source |
| Build output | `bin/` and `obj/` | ~50MB |

### CI/CD

**Not required.** This is a local-only tool. Build and run instructions:

```bash
dotnet build
dotnet run
# Navigate to https://localhost:5001 in Chrome/Edge
```

For distribution as a self-contained executable (optional):

```bash
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish
```

### Networking

- **Inbound:** Kestrel listens on loopback interface only; not accessible from other machines
- **Outbound:** None. The application makes zero network calls. The ADO backlog link is a client-side hyperlink navigated by the browser.

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **Framework** | Blazor Server (.NET 8) | Mandatory per project requirements. Provides C# rendering, CSS isolation, hot reload via `dotnet watch`, strongly-typed JSON models. |
| **CSS approach** | Hand-written CSS, ported from reference HTML | The design is 120 lines of CSS. A component library (MudBlazor, Radzen) would add 500KB+ and fight the fixed layout. No forms, no data tables, no interactive widgets. |
| **Charting/timeline** | Inline SVG in Razor markup | The timeline uses basic SVG primitives (`<line>`, `<circle>`, `<polygon>`, `<text>`). A charting library would impose unwanted styling, axis logic, and responsiveness. Hand-coded SVG gives pixel-level control matching the reference. |
| **Layout system** | CSS Grid + Flexbox | Grid for the heatmap (5-column layout), Flexbox for header and page chrome. Both are native CSS; no layout library needed. |
| **Data format** | JSON flat file | Human-editable, version-controllable, zero-infrastructure. `System.Text.Json` is built into .NET 8. |
| **Data access** | `System.Text.Json` deserialization | Built-in, zero NuGet dependencies. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` for forgiving deserialization. |
| **File watching** | `FileSystemWatcher` (optional) | Built into .NET BCL. Enables live reload when `data.json` is saved. Known to occasionally miss events on Windows; manual browser refresh is the fallback. |
| **Font** | Segoe UI (system font) | Specified in the design reference. Pre-installed on Windows. No web font loading, no FOUT, no external CDN dependency. |
| **NuGet packages** | Zero additional | The .NET 8 Blazor Server template includes everything: Kestrel, Razor, CSS isolation, `System.Text.Json`, `IConfiguration`. |
| **Project structure** | Single .csproj in .sln | A read-only dashboard with one page and one data source does not benefit from multi-project architecture. One project with `Models/`, `Services/`, `Components/` folders. |
| **Responsive design** | None | Intentionally omitted. The 1920×1080 fixed layout is a feature, not a limitation. It ensures screenshot fidelity for PowerPoint capture. |

### Rejected Alternatives

| Alternative | Why Rejected |
|-------------|-------------|
| Static HTML + JS `fetch()` | Simpler, but doesn't meet the .NET/.sln requirement; no strongly-typed models to catch JSON schema errors. |
| MudBlazor / Radzen | Massive CSS/JS payload interferes with pixel-perfect screenshot. No interactive widgets are needed. |
| ApexCharts.Blazor | Imposes its own chart styling, axes, and responsive behavior—all counterproductive for a fixed screenshot. |
| SQLite database | No historical data requirements. JSON is simpler, human-editable, and version-controllable. |
| Blazor WebAssembly | Heavier initial download, no benefit for a local single-user tool. Server mode has hot reload. |

---

## Security Considerations

### Authentication & Authorization

**None.** This is a local-only tool running on `localhost`. No user login, no roles, no cookies, no tokens, no OAuth. This is explicitly out of scope per the PM specification.

**Future consideration:** If the dashboard is ever shared on a network, add Windows Authentication via Kestrel's `HttpSys` server in `Program.cs`. This is a one-line configuration change, not an architectural change.

### Data Protection

| Concern | Mitigation |
|---------|------------|
| **Sensitive project names in `data.json`** | Add `data.json` to `.gitignore`. Ship a `data.sample.json` with fictional data for the repository. |
| **PII** | No PII is expected in this application. The data model contains project names, milestone descriptions, and work item titles—none of which should contain personal information. |
| **Encryption at rest** | Not needed. Local file on developer's machine, protected by OS-level disk encryption (BitLocker). |
| **Encryption in transit** | HTTPS via Kestrel dev certificate. Loopback only, so network sniffing is not a realistic threat. |

### Input Validation

| Input | Validation | Response |
|-------|-----------|----------|
| `data.json` content | `System.Text.Json` deserialization with `try/catch` | User-friendly error message on parse failure |
| Missing required fields | Null-coalescing to defaults (`?? ""`, `?? []`) | Partial render with empty sections; console warnings |
| Extremely large `data.json` (>100KB) | No explicit size limit; `System.Text.Json` handles efficiently | Performance may degrade; document 100KB as practical limit |
| Malicious JSON (prototype pollution, etc.) | Not applicable—`System.Text.Json` deserializes into strongly-typed records, no dynamic property expansion | N/A |

### Network Exposure

- Kestrel binds to `localhost` only via `launchSettings.json`
- No external API calls from server or client
- SignalR hub is only accessible on loopback
- The ADO backlog link is a standard HTML `<a>` tag with `target="_blank"`—navigated by the browser, not the server

### Dependency Supply Chain

Zero external NuGet packages beyond the .NET 8 SDK framework references. Attack surface is limited to the .NET runtime itself, which receives regular security patches via Microsoft Update.

---

## Scaling Strategy

### Current Scale

This application is designed for **exactly one user** running on **one machine**. There is no scaling requirement and no scaling strategy needed.

| Dimension | Current | Max Supported |
|-----------|---------|---------------|
| Concurrent users | 1 | ~20 (Blazor Server on localhost) |
| Data size | < 10KB typical | 100KB (practical JSON editing limit) |
| Milestone tracks | 3 | 5 (SVG height constraint) |
| Heatmap columns | 4 | 6 (1920px width constraint) |
| Heatmap items per cell | ~5 | ~10 (cell overflow hidden) |
| Pages | 1 | 1 |

### If Scale Were Ever Needed

These are **not planned**, but documented for completeness:

| Scenario | Approach |
|----------|----------|
| Multiple projects | Separate `data-{project}.json` files; add a route parameter `/dashboard/{project}` |
| Historical snapshots | Date-stamped JSON files (`data-2026-03.json`); add a month selector dropdown |
| Team-wide access | Deploy to IIS or Azure App Service; add Windows Authentication |
| Automated screenshots | Add a Playwright test project that navigates to the page and captures a screenshot programmatically |

---

## Risks & Mitigations

| # | Risk | Severity | Likelihood | Mitigation |
|---|------|----------|------------|------------|
| 1 | **Screenshot fidelity mismatch** — Dashboard doesn't match `OriginalDesignConcept.html` pixel-for-pixel | High | Medium | Port CSS directly from reference HTML. Use `dotnet watch` for rapid iteration. Compare screenshots side-by-side during development. Include pixel-comparison step in acceptance criteria. |
| 2 | **JSON schema drift** — `data.json` structure evolves but C# records don't match | Medium | Medium | Use `[JsonPropertyName]` attributes for explicit mapping. Add startup validation that logs warnings for unexpected/missing fields. Ship a `data.schema.json` for editor IntelliSense. |
| 3 | **Blazor Server overhead** — SignalR WebSocket is unnecessary for a read-only page | Low | Certain | Accepted trade-off. Overhead is ~50KB JS + one WebSocket. Developer experience (hot reload, CSS isolation, C# models) justifies the cost. |
| 4 | **SVG `<feDropShadow>` rendering** — Filter may render differently across browsers | Low | Low | Target Chrome/Edge only (same Chromium engine). Test once during development. Both browsers use the same SVG rendering engine. |
| 5 | **Segoe UI unavailability** — Font not installed on macOS/Linux | Medium | Low | Primary target is Windows. Document font installation for other platforms. Arial fallback is specified in the font stack. |
| 6 | **FileSystemWatcher misses events** — Known Windows issue with file change notifications | Low | Medium | FileSystemWatcher is a convenience feature. Manual browser refresh (F5) is the documented primary workflow. |
| 7 | **Malformed `data.json` crashes the page** — Invalid JSON or missing fields | High | Medium | Wrap deserialization in `try/catch`. Return user-friendly error message. Never render partial/broken layout. Test with empty file, invalid JSON, and missing fields. |
| 8 | **Browser zoom ≠ 100%** — Layout breaks at non-100% zoom levels | Medium | Medium | Document the screenshot procedure: "Set browser zoom to 100%. Set window size to 1920×1080." Consider adding a `<meta name="viewport">` tag. |
| 9 | **Scope creep** — Stakeholders request interactive features (tooltips, drill-down, filters) | High | High | Enforce the PM specification scope. This is a screenshot generator. Interactive features belong in a different tool (e.g., Power BI, ADO dashboards). |
| 10 | **DPI scaling on high-DPI monitors** — Windows display scaling affects browser rendering | Medium | Medium | Document: "Set display scaling to 100% or use Chrome DevTools device emulation at 1920×1080." |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to Blazor component markup, CSS strategy, data bindings, and interactions.

### Component: Dashboard Page (`Dashboard.razor`)

The entire dashboard is a **single Razor component** (no sub-components). The design is simple enough that splitting into sub-components would add indirection without benefit. All three sections render inline within one `@page "/"` component.

### Section 1: Header Bar

**Visual reference:** `.hdr` class in `OriginalDesignConcept.html`

| Aspect | Implementation |
|--------|---------------|
| **Razor markup** | `<div class="hdr">` containing left group (title + subtitle) and right group (legend) |
| **CSS layout** | Flexbox: `display: flex; align-items: center; justify-content: space-between` |
| **Padding** | `12px 44px 10px` |
| **Data bindings** | `@Data.Title` → `<h1>` text; `@Data.Subtitle` → subtitle div; `@Data.BacklogUrl` → conditional `<a>` tag |
| **Conditional rendering** | `@if (!string.IsNullOrEmpty(Data.BacklogUrl))` wraps the ADO backlog link |
| **Legend** | Static markup (symbols don't change); four flex items with CSS shapes (rotated squares for diamonds, circle, rectangle) |
| **Interactions** | Backlog link: `target="_blank"` opens in new tab. No other interactions. |

```razor
<div class="hdr">
    <div>
        <h1>@Data.Title
            @if (!string.IsNullOrEmpty(Data.BacklogUrl))
            {
                <a href="@Data.BacklogUrl" target="_blank">📋 ADO Backlog</a>
            }
        </h1>
        <div class="sub">@Data.Subtitle</div>
    </div>
    <div class="legend">
        <span class="legend-item"><span class="diamond poc-diamond"></span>PoC Milestone</span>
        <span class="legend-item"><span class="diamond prod-diamond"></span>Production Release</span>
        <span class="legend-item"><span class="checkpoint-dot"></span>Checkpoint</span>
        <span class="legend-item"><span class="now-marker"></span>Now (@Data.CurrentMonth 2026)</span>
    </div>
</div>
```

### Section 2: Timeline Area

**Visual reference:** `.tl-area` class and embedded SVG in `OriginalDesignConcept.html`

| Aspect | Implementation |
|--------|---------------|
| **Razor markup** | `<div class="tl-area">` containing left sidebar div + right SVG box |
| **CSS layout** | Flexbox horizontal: `display: flex; align-items: stretch; height: 196px` |
| **Left sidebar** | 230px wide flex column; `@foreach (var track in Data.Milestones)` renders track labels with inline color styles |
| **SVG viewport** | `<svg width="1560" height="185" style="overflow:visible">` |
| **Data bindings** | `@Data.TimelineMonths` → gridline positions and labels; `@Data.Milestones` → track lines and events; `@Data.CurrentMonth` → NOW line position |
| **SVG elements** | Month gridlines (`<line>`), month labels (`<text>`), track lines (`<line>`), checkpoint circles (`<circle>`), PoC diamonds (`<polygon>` fill `#F4B400`), production diamonds (`<polygon>` fill `#34A853`), NOW dashed line (`<line>` stroke `#EA4335`), event labels (`<text>`) |
| **Coordinate computation** | `DateToX()` maps dates to 0–1560px X range; `GetTrackY()` distributes tracks evenly across 185px |
| **Drop shadow filter** | `<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>` |
| **Interactions** | None. Static SVG rendering. |

**Key C# helper methods in `@code` block:**

```csharp
private const double SvgWidth = 1560.0;
private const double SvgHeight = 185.0;

private double DateToX(DateOnly date)
{
    var start = new DateOnly(timelineStartYear, 1, 1);
    var end = start.AddMonths(Data.TimelineMonths.Length);
    var totalDays = end.DayNumber - start.DayNumber;
    var elapsed = date.DayNumber - start.DayNumber;
    return Math.Clamp((elapsed / (double)totalDays) * SvgWidth, 0, SvgWidth);
}

private double GetTrackY(int index, int count)
{
    // Evenly distribute tracks; for 3 tracks: Y ≈ 42, 98, 154
    return (SvgHeight / (count + 1)) * (index + 1);
}

private string DiamondPoints(double cx, double cy, double size)
{
    return $"{cx},{cy - size} {cx + size},{cy} {cx},{cy + size} {cx - size},{cy}";
}

private double GetMonthGridX(int monthIndex)
{
    return monthIndex * (SvgWidth / Data.TimelineMonths.Length);
}
```

### Section 3: Heatmap Area

**Visual reference:** `.hm-wrap` and `.hm-grid` classes in `OriginalDesignConcept.html`

| Aspect | Implementation |
|--------|---------------|
| **Razor markup** | `<div class="hm-wrap">` containing title div + grid div |
| **CSS layout** | Flex column for wrapper; CSS Grid for data: `grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr)` |
| **Grid column count** | Dynamic: `repeat(@Data.Heatmap.Columns.Length, 1fr)` |
| **Section title** | Static text with category names, uppercase |
| **Corner cell** | `.hm-corner` — "STATUS / MONTH" label |
| **Column headers** | `@foreach (var col in Data.Heatmap.Columns)` → `<div class="hm-col-hdr @(col == Data.CurrentMonth ? "current-month-hdr" : "")">` |
| **Row headers** | `@foreach (var row in Data.Heatmap.Rows)` → `<div class="hm-row-hdr @GetRowHeaderClass(row.Category)">` |
| **Data cells** | Nested loop: rows × columns. Each cell gets category-specific background class + current-month accent class |
| **Cell items** | `@foreach (var item in items)` → `<div class="it">@item</div>` with `::before` pseudo-element for colored bullet |
| **Empty cells** | `@if (items.Length == 0)` → `<span style="color:#AAA">—</span>` |
| **Current month highlighting** | Column header: `background: #FFF0D0; color: #C07700`. Cell backgrounds: accent colors per category. |
| **Interactions** | None. Static grid rendering. |

```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Data.Heatmap.Columns.Length, 1fr);">
    <div class="hm-corner">Status / Month</div>
    @foreach (var col in Data.Heatmap.Columns)
    {
        var isCurrent = col == Data.CurrentMonth;
        <div class="hm-col-hdr @(isCurrent ? "current-month-hdr" : "")">@col</div>
    }
    @foreach (var row in Data.Heatmap.Rows)
    {
        var style = GetCategoryStyle(row.Category);
        <div class="hm-row-hdr" style="color:@style.HeaderText; background:@style.HeaderBg;">
            @style.DisplayName
        </div>
        @foreach (var col in Data.Heatmap.Columns)
        {
            var isCurrent = col == Data.CurrentMonth;
            var items = row.Items.GetValueOrDefault(col, Array.Empty<string>());
            var cellBg = isCurrent ? style.CellAccent : style.CellBg;
            <div class="hm-cell" style="background:@cellBg;">
                @if (items.Length == 0)
                {
                    <span style="color:#AAA">—</span>
                }
                else
                {
                    @foreach (var item in items)
                    {
                        <div class="it" style="--bullet-color:@style.Bullet;">@item</div>
                    }
                }
            </div>
        }
    }
</div>
```

### CSS Custom Properties (in `Dashboard.razor.css`)

```css
.it::before {
    content: '';
    position: absolute;
    left: 0;
    top: 7px;
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: var(--bullet-color);
}
```

### Error State Component

**Visual reference:** Not in `OriginalDesignConcept.html` (error state is not in the design)

| Aspect | Implementation |
|--------|---------------|
| **Condition** | `@if (Data == null)` |
| **Markup** | Centered `<div>` on white background with error message |
| **CSS** | `display: flex; align-items: center; justify-content: center; width: 1920px; height: 1080px;` |
| **Text** | `"Unable to load dashboard data. Please check data.json."` followed by specific error detail |
| **Interactions** | None |

---

## File Inventory

| # | File | Purpose | Est. Lines |
|---|------|---------|------------|
| 1 | `ReportingDashboard.sln` | Solution file | 25 |
| 2 | `ReportingDashboard.csproj` | Project file (net8.0, Blazor Server) | 12 |
| 3 | `Program.cs` | Host setup, DI registration | 15 |
| 4 | `Models/DashboardData.cs` | C# record types for JSON schema | 40 |
| 5 | `Services/DashboardDataService.cs` | JSON file read, cache, error handling | 70 |
| 6 | `Components/App.razor` | HTML shell, CSS/JS references | 20 |
| 7 | `Components/Routes.razor` | Blazor router | 8 |
| 8 | `Components/Pages/Dashboard.razor` | The single dashboard page | 250 |
| 9 | `Components/Pages/Dashboard.razor.css` | Scoped CSS ported from reference | 150 |
| 10 | `wwwroot/css/app.css` | Global reset, body fixed dimensions | 15 |
| 11 | `wwwroot/data/data.json` | Sample dashboard data | 80 |
| 12 | `Properties/launchSettings.json` | Kestrel binding config | 20 |
| | **Total** | | **~705** |

---

## Implementation Phases

| Phase | Scope | Duration | Verification |
|-------|-------|----------|-------------|
| **1: Skeleton** | `dotnet new blazor`, delete defaults, create `Dashboard.razor` with hardcoded HTML from reference, port CSS | Day 1 | Page renders at 1920×1080 matching reference |
| **2: Data Model** | Define records in `Models/`, create `data.json`, build `DashboardDataService`, register in DI | Day 1–2 | Service deserializes all fields correctly |
| **3: Dynamic Render** | Replace hardcoded HTML with `@foreach` loops, implement `DateToX()` and `GetTrackY()`, render SVG and heatmap from data | Day 2 | Changing `data.json` changes the dashboard |
| **4: Polish** | CSS custom properties, error handling, empty states, fine-tune pixel alignment, document screenshot procedure | Day 2–3 | Screenshot indistinguishable from reference |