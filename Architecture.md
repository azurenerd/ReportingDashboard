# Architecture

## Overview & Goals

This document defines the complete system architecture for the **Executive Reporting Dashboard** — a single-page Blazor Server application that renders project milestones, timelines, and monthly execution status optimized for 1920×1080 screenshot capture. The application reads all data from a local `data.json` file, requires zero infrastructure, and runs with `dotnet run`.

**Architecture Goals:**

1. **Pixel-perfect rendering** of the reference design (`OriginalDesignConcept.html`) at exactly 1920×1080
2. **Zero-dependency local operation** — no cloud, no database, no authentication, no additional NuGet packages
3. **Data-driven via JSON** — all displayed content sourced from a single `data.json` file
4. **Minimal codebase** — under 15 files, single `.sln` with one `.csproj`
5. **Auto-reload** — `FileSystemWatcher` detects `data.json` changes and triggers re-render without manual browser refresh
6. **SVG coordinate math** — date-to-pixel mapping for timeline markers computed server-side in C#

**Architecture Pattern:** Minimal layered monolith with three logical layers — Data (JSON file + service), Model (strongly-typed C# classes), and Presentation (Blazor Razor components). No abstractions beyond what is needed to keep components clean.

---

## System Components

### 1. `Program.cs` — Application Entry Point

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure Kestrel (localhost-only), register DI services, map Blazor hub and fallback page |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService` (registered as singleton) |
| **Data** | Reads `appsettings.json` for port config; delegates `data.json` to the service |

**Key behaviors:**
- Binds Kestrel to `http://localhost:5000` only (no HTTPS required for local tool)
- Registers `DashboardDataService` as a **singleton** in DI (one instance, one `FileSystemWatcher`)
- Calls `app.MapBlazorHub()` and `app.MapFallbackToPage("/_Host")` (or `app.MapRazorComponents<App>()` depending on Blazor Server template variant)
- Suppresses Blazor's default error UI in production mode

```csharp
// Pseudocode — Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

### 2. `DashboardDataService` — Data Access & File Watching

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Read, deserialize, cache, and watch `data.json`; notify subscribers on change |
| **Interfaces** | `DashboardData GetData()`, `event Action OnDataChanged` |
| **Dependencies** | `System.Text.Json`, `System.IO.FileSystemWatcher` |
| **Data** | Reads `wwwroot/data.json` (or configurable path); holds in-memory `DashboardData` cache |

**Key behaviors:**
- On construction: reads and deserializes `data.json` into `DashboardData`; starts `FileSystemWatcher`
- Exposes `DashboardData GetData()` — returns the cached model (no file I/O on every call)
- `FileSystemWatcher` listens for `Changed`, `Created` events on `data.json`
- **Debounce logic:** Uses a 300ms `System.Threading.Timer` to coalesce rapid file events (VS Code saves trigger multiple events)
- On debounced change: re-reads file, deserializes, swaps cached model, fires `OnDataChanged` event
- **Error handling:** If deserialization fails, sets an `ErrorMessage` string on the cached state instead of throwing; the UI reads this and displays a user-friendly error
- Implements `IDisposable` to clean up `FileSystemWatcher` and timer

```csharp
public class DashboardDataService : IDisposable
{
    public DashboardData? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public event Action? OnDataChanged;

    private readonly FileSystemWatcher _watcher;
    private readonly Timer _debounceTimer;
    private readonly string _filePath;

    // Constructor: initial load + start watcher
    // LoadData(): File.ReadAllText → JsonSerializer.Deserialize<DashboardData>
    // OnFileChanged(): reset debounce timer → 300ms → LoadData() → OnDataChanged?.Invoke()
}
```

### 3. `DashboardData` Model (and related models)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Strongly-typed C# representation of `data.json` |
| **Interfaces** | POCO classes with `System.Text.Json` attributes |
| **Dependencies** | None |
| **Data** | Mirrors the JSON schema exactly |

**Model classes (all in `Models/DashboardData.cs` — single file):**

```csharp
public class DashboardData
{
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string BacklogUrl { get; set; }
    public DateTime CurrentDate { get; set; }
    public DateTime StartDate { get; set; }       // Timeline range start
    public DateTime EndDate { get; set; }         // Timeline range end
    public List<string> Months { get; set; }      // Display labels: ["Jan 2026", ...]
    public int CurrentMonthIndex { get; set; }    // 0-based index for highlighting
    public List<Milestone> Milestones { get; set; }
    public List<HeatmapCategory> Categories { get; set; }
}

public class Milestone
{
    public string Id { get; set; }                // "M1", "M2", "M3"
    public string Label { get; set; }             // "Chatbot & MS Role"
    public string Color { get; set; }             // "#0078D4"
    public List<MilestoneEvent> Events { get; set; }
}

public class MilestoneEvent
{
    public DateTime Date { get; set; }
    public string Type { get; set; }              // "checkpoint", "checkpoint-minor", "poc", "production"
    public string? Label { get; set; }            // Optional: "Jan 12", "Mar 26 PoC"
    public string? LabelPosition { get; set; }    // Optional: "above" or "below" (default: "above")
}

public class HeatmapCategory
{
    public string Name { get; set; }              // "Shipped", "In Progress", "Carryover", "Blockers"
    public string ColorScheme { get; set; }       // "green", "blue", "amber", "red"
    public Dictionary<string, List<string>> Rows { get; set; }  // month label → items
}
```

### 4. `Dashboard.razor` — Page Component (Orchestrator)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Top-level page; injects `DashboardDataService`, subscribes to changes, renders child components or error state |
| **Interfaces** | Blazor `@page "/"` route |
| **Dependencies** | `DashboardDataService`, `DashboardHeader`, `TimelineSection`, `HeatmapGrid` |
| **Data** | Receives `DashboardData` from service; passes slices to children as `[Parameter]` props |

**Key behaviors:**
- `OnInitialized`: gets data from service; subscribes to `OnDataChanged`
- `OnDataChanged` handler: calls `InvokeAsync(StateHasChanged)` to trigger re-render on the Blazor sync context
- If `ErrorMessage` is set, renders a centered error `<div>` instead of the dashboard
- `Dispose`: unsubscribes from `OnDataChanged`

### 5. `DashboardHeader.razor` — Header Bar Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render title, backlog link, subtitle, and legend icons |
| **Interfaces** | `[Parameter] string Title`, `[Parameter] string Subtitle`, `[Parameter] string BacklogUrl`, `[Parameter] DateTime CurrentDate` |
| **Dependencies** | None |
| **Data** | Display-only; no state |

### 6. `TimelineSection.razor` — Timeline SVG Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render milestone label panel + inline SVG with date-positioned elements |
| **Interfaces** | `[Parameter] List<Milestone> Milestones`, `[Parameter] DateTime StartDate`, `[Parameter] DateTime EndDate`, `[Parameter] DateTime CurrentDate` |
| **Dependencies** | None |
| **Data** | Computes SVG coordinates from date math in `@code` block |

**SVG coordinate system (1560×185):**
- **`svgWidth`** = 1560px (constant)
- **`svgHeight`** = 185px (constant)
- **Date-to-X mapping:** `DateToX(DateTime date)` → `((date - StartDate).TotalDays / (EndDate - StartDate).TotalDays) * svgWidth`
- **Swim lane Y positions:** Evenly distributed based on milestone count. For 3 milestones: y=42, y=98, y=154. Formula: `y = 42 + (index * 56)` for up to 3 lanes; generalized: `y = (svgHeight / (count + 1)) * (index + 1)` clamped to leave room for labels.
- **Month grid lines:** Iterate from `StartDate` to `EndDate` by month; for each month boundary, compute x via `DateToX(new DateTime(year, month, 1))`
- **Diamond polygon points:** For center (cx, cy) with offset 11: `$"{cx},{cy-11} {cx+11},{cy} {cx},{cy+11} {cx-11},{cy}"`
- **Label alternation:** Events are sorted by date within each milestone; odd-indexed labels render below (`cy + 24`), even-indexed above (`cy - 16`). The `LabelPosition` field in JSON can override this.

### 7. `HeatmapGrid.razor` — Heatmap Grid Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the CSS Grid with header row, row headers, and data cells |
| **Interfaces** | `[Parameter] List<HeatmapCategory> Categories`, `[Parameter] List<string> Months`, `[Parameter] int CurrentMonthIndex` |
| **Dependencies** | `HeatmapCell` (child component) |
| **Data** | Passes per-cell item lists and color scheme to `HeatmapCell` |

**CSS Grid setup:** The grid columns are set dynamically via inline style: `grid-template-columns: 160px repeat(@Months.Count, 1fr)`. Rows: `grid-template-rows: 36px repeat(@Categories.Count, 1fr)`.

### 8. `HeatmapCell.razor` — Individual Cell Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render bullet-pointed items with colored dots, or a muted dash for empty cells |
| **Interfaces** | `[Parameter] List<string> Items`, `[Parameter] string ColorScheme`, `[Parameter] bool IsCurrentMonth` |
| **Dependencies** | None |
| **Data** | Display-only |

**Behavior:** If `Items` is null or empty, renders `<div class="it" style="color:#AAA;">-</div>`. Otherwise, renders each item as `<div class="it">@item</div>` where the `::before` CSS pseudo-element provides the colored dot.

### 9. `MainLayout.razor` — Minimal Shell

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Bare-minimum layout wrapper; renders `@Body` with no chrome |
| **Interfaces** | Blazor layout convention |
| **Dependencies** | None |
| **Data** | None |

**Content:** `@Body` only. No nav menu, no sidebar, no Blazor error UI. The layout adds no visual elements.

### 10. `wwwroot/css/site.css` — Global Stylesheet

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | All visual styling; CSS custom properties for color palette; ported verbatim from reference HTML |
| **Interfaces** | CSS classes matching reference design class names |
| **Dependencies** | None |
| **Data** | None |

### 11. `wwwroot/data.json` — Configuration Data File

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Sole data source for all dashboard content |
| **Interfaces** | JSON file conforming to `DashboardData` schema |
| **Dependencies** | None |
| **Data** | Project title, subtitle, dates, milestones, heatmap categories |

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────┐     FileSystemWatcher (debounced 300ms)
│  data.json      │────────────────────────────────────────┐
│  (wwwroot/)     │     Initial read on startup            │
└────────┬────────┘                                        │
         │                                                 │
         ▼                                                 ▼
┌─────────────────────────────────────────────────────────────┐
│  DashboardDataService (Singleton)                           │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────┐ │
│  │ LoadData()   │  │ DashboardData│  │ OnDataChanged     │ │
│  │ JSON → Model │→ │ (cached)     │  │ event Action      │ │
│  └──────────────┘  └──────────────┘  └─────────┬─────────┘ │
└────────────────────────────────────────────────┬────────────┘
                                                 │
                          InvokeAsync(StateHasChanged)
                                                 │
                                                 ▼
┌─────────────────────────────────────────────────────────────┐
│  Dashboard.razor (@page "/")                                │
│  ┌─────────────────┐ ┌──────────────┐ ┌──────────────────┐ │
│  │DashboardHeader   │ │TimelineSection│ │HeatmapGrid      │ │
│  │ [Parameter] props│ │ [Parameter]   │ │ [Parameter] props│ │
│  │ title, subtitle, │ │ milestones,   │ │ categories,     │ │
│  │ backlogUrl, date │ │ startDate,    │ │ months,         │ │
│  └─────────────────┘ │ endDate,      │ │ currentMonthIdx │ │
│                       │ currentDate   │ └────────┬─────────┘ │
│                       └──────────────┘          │           │
│                                                  ▼           │
│                                        ┌──────────────────┐ │
│                                        │HeatmapCell x N   │ │
│                                        │ [Parameter] items,│ │
│                                        │ colorScheme,      │ │
│                                        │ isCurrentMonth    │ │
│                                        └──────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Communication Patterns

1. **Startup:** `Program.cs` → DI registers `DashboardDataService` (singleton) → service reads `data.json` → caches `DashboardData`
2. **Page load:** Browser → Blazor SignalR → `Dashboard.razor` `OnInitialized` → `service.GetData()` → passes model slices to child components as `[Parameter]` values → server-side render → HTML to browser
3. **File change:** User saves `data.json` → `FileSystemWatcher` fires → debounce timer (300ms) → `LoadData()` → model replaced → `OnDataChanged` event → `Dashboard.razor` handler calls `InvokeAsync(StateHasChanged)` → Blazor diffs and pushes DOM update via SignalR
4. **Error state:** `LoadData()` catches `JsonException` → sets `ErrorMessage` → `Dashboard.razor` renders error `<div>` instead of dashboard components

### Component Parameter Contracts

| Parent | Child | Parameters Passed |
|--------|-------|-------------------|
| `Dashboard.razor` | `DashboardHeader.razor` | `Title`, `Subtitle`, `BacklogUrl`, `CurrentDate` |
| `Dashboard.razor` | `TimelineSection.razor` | `Milestones`, `StartDate`, `EndDate`, `CurrentDate` |
| `Dashboard.razor` | `HeatmapGrid.razor` | `Categories`, `Months`, `CurrentMonthIndex` |
| `HeatmapGrid.razor` | `HeatmapCell.razor` | `Items` (List<string>), `ColorScheme`, `IsCurrentMonth` |

---

## Data Model

### `data.json` Schema (Canonical)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "currentDate": "2026-04-10",
  "startDate": "2026-01-01",
  "endDate": "2026-06-30",
  "months": ["Jan 2026", "Feb 2026", "Mar 2026", "Apr 2026"],
  "currentMonthIndex": 3,
  "milestones": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12", "labelPosition": "above" },
        { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC", "labelPosition": "below" },
        { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)", "labelPosition": "above" }
      ]
    },
    {
      "id": "M2",
      "label": "PDS & Data Inventory",
      "color": "#00897B",
      "events": [
        { "date": "2025-12-19", "type": "checkpoint", "label": "Dec 19", "labelPosition": "above" },
        { "date": "2026-02-11", "type": "checkpoint", "label": "Feb 11", "labelPosition": "above" },
        { "date": "2026-03-05", "type": "checkpoint-minor" },
        { "date": "2026-03-15", "type": "checkpoint-minor" },
        { "date": "2026-03-20", "type": "checkpoint-minor" },
        { "date": "2026-03-22", "type": "checkpoint-minor" },
        { "date": "2026-03-27", "type": "poc", "label": "Mar 27 PoC", "labelPosition": "above" },
        { "date": "2026-04-04", "type": "production", "label": "Apr 4 Prod", "labelPosition": "below" }
      ]
    },
    {
      "id": "M3",
      "label": "Auto Review DFD",
      "color": "#546E7A",
      "events": [
        { "date": "2026-02-03", "type": "checkpoint", "label": "Feb 3", "labelPosition": "above" },
        { "date": "2026-04-15", "type": "poc", "label": "Apr 15 PoC", "labelPosition": "below" },
        { "date": "2026-06-01", "type": "production", "label": "Jun 1 Prod", "labelPosition": "above" }
      ]
    }
  ],
  "categories": [
    {
      "name": "Shipped",
      "colorScheme": "green",
      "rows": {
        "Jan 2026": ["Chatbot v1 scaffolding", "MS Role RBAC design"],
        "Feb 2026": ["PDS schema v1", "Data Inventory pilot"],
        "Mar 2026": ["Chatbot intent engine", "PDS API endpoints"],
        "Apr 2026": ["PDS production deploy", "Data Inventory GA"]
      }
    },
    {
      "name": "In Progress",
      "colorScheme": "blue",
      "rows": {
        "Jan 2026": ["PDS requirements gathering"],
        "Feb 2026": ["Auto Review DFD design", "Chatbot NLU training"],
        "Mar 2026": ["Auto Review parser", "E2E test framework"],
        "Apr 2026": ["Chatbot MS Role integration", "DFD auto-review PoC", "Regression test suite"]
      }
    },
    {
      "name": "Carryover",
      "colorScheme": "amber",
      "rows": {
        "Jan 2026": [],
        "Feb 2026": ["Chatbot edge cases from Jan"],
        "Mar 2026": ["PDS perf optimization"],
        "Apr 2026": ["Auto Review false positive tuning"]
      }
    },
    {
      "name": "Blockers",
      "colorScheme": "red",
      "rows": {
        "Jan 2026": [],
        "Feb 2026": [],
        "Mar 2026": ["Dependency on IAM team for token refresh"],
        "Apr 2026": ["Graph API throttling in staging"]
      }
    }
  ]
}
```

### Entity Relationships

```
DashboardData (root, 1)
├── has many → Milestone (1..N, typically 3)
│   └── has many → MilestoneEvent (1..N per milestone)
└── has many → HeatmapCategory (exactly 4: Shipped, In Progress, Carryover, Blockers)
    └── has map → Dictionary<string, List<string>> (month label → item strings)
```

### Storage

- **Format:** Single JSON flat file (`wwwroot/data.json`)
- **Size:** Typically 2-10 KB; max supported ~100 KB
- **Persistence:** File on local disk; no database
- **Caching:** In-memory singleton; refreshed on file change
- **Schema validation:** Implicit via `System.Text.Json` deserialization — missing required properties or type mismatches produce `JsonException`, caught and surfaced as `ErrorMessage`

### Event Type Enum (Logical — Mapped in Razor)

| `type` value | SVG rendering | Label shown |
|-------------|--------------|-------------|
| `"checkpoint"` | Open circle (r=5-7, fill white, stroke milestone color, stroke-width 2.5) | Yes |
| `"checkpoint-minor"` | Small filled circle (r=4, fill #999, no stroke) | No |
| `"poc"` | Gold diamond polygon (#F4B400) with drop shadow | Yes |
| `"production"` | Green diamond polygon (#34A853) with drop shadow | Yes |

### Color Scheme Mapping (Hardcoded in CSS/Component)

| `colorScheme` | Dot | Cell BG | Current BG | Header BG | Text |
|--------------|-----|---------|------------|-----------|------|
| `"green"` | #34A853 | #F0FBF0 | #D8F2DA | #E8F5E9 | #1B7A28 |
| `"blue"` | #0078D4 | #EEF4FE | #DAE8FB | #E3F2FD | #1565C0 |
| `"amber"` | #F4B400 | #FFFDE7 | #FFF0B0 | #FFF8E1 | #B45309 |
| `"red"` | #EA4335 | #FFF5F5 | #FFE4E4 | #FEF2F2 | #991B1B |

---

## API Contracts

This application has **no REST API endpoints**. It is a single-page Blazor Server application with no backend API surface.

### Internal Contracts

#### `DashboardDataService` Public Interface

```csharp
public class DashboardDataService : IDisposable
{
    /// Returns the current cached dashboard data, or null if loading failed.
    public DashboardData? Data { get; }

    /// Non-null when data.json failed to load or parse. Contains user-friendly message.
    public string? ErrorMessage { get; }

    /// Fired when data.json changes on disk and new data is loaded (or error is set).
    /// Subscribers MUST call InvokeAsync(StateHasChanged) — this fires on a threadpool thread.
    public event Action? OnDataChanged;
}
```

#### Error Contract

| Condition | Behavior |
|-----------|----------|
| `data.json` missing | `ErrorMessage = "Dashboard data file not found: data.json"` |
| `data.json` malformed JSON | `ErrorMessage = "Invalid JSON in data.json: {JsonException.Message}"` |
| `data.json` missing required field | `ErrorMessage = "Invalid dashboard data: {details}"` |
| `data.json` valid | `Data` is populated, `ErrorMessage` is null |

#### Blazor SignalR Hub

The default Blazor Server SignalR hub (`_blazor`) is used as-is. No custom hub methods. The framework handles DOM diff transport automatically.

| Endpoint | Purpose |
|----------|---------|
| `/_blazor` | Blazor Server SignalR hub (framework-managed) |
| `/` | Dashboard page (single route) |
| `/css/site.css` | Static file serving |
| `/data.json` | Static file (also read directly from disk by service) |

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|--------------|
| **.NET SDK** | .NET 8 SDK (8.0.400+) |
| **OS** | Windows 10/11 (primary); macOS/Linux (untested but expected to work) |
| **Browser** | Google Chrome (latest stable) — official screenshot target |
| **Backup browser** | Microsoft Edge (Chromium) |
| **Memory** | < 100 MB application footprint |
| **Disk** | < 5 MB for source + build output |
| **Network** | None — localhost only |

### Hosting

| Aspect | Configuration |
|--------|--------------|
| **Web server** | Kestrel (embedded, default) |
| **Binding** | `http://localhost:5000` only |
| **HTTPS** | Not required (local tool) |
| **Reverse proxy** | Not required |
| **Process model** | `dotnet run` from CLI or `dotnet watch` for development |

### Storage

| Item | Location | Size |
|------|----------|------|
| `data.json` | `wwwroot/data.json` (or app content root) | 2-10 KB typical |
| Static assets | `wwwroot/css/` | < 10 KB |
| Build output | `bin/` and `obj/` | ~50 MB (standard .NET) |

### CI/CD

**Not required.** This is a local tool. The workflow is:

1. `git clone` the repository
2. `dotnet run` (or `dotnet watch`)
3. Open `http://localhost:5000` in Chrome

Optional: `dotnet publish -c Release -o ./publish` for a self-contained deployment folder.

### Development Workflow

```
Developer machine
├── dotnet watch                    ← live reload during development
├── Edit data.json in VS Code      ← FileSystemWatcher triggers re-render
├── Chrome DevTools (1920x1080)    ← device emulation for screenshot
└── Capture screenshot → PowerPoint
```

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **Framework** | Blazor Server (.NET 8) | Mandated by team stack. Server-side rendering avoids WASM download. Single local user makes SignalR overhead negligible. Hot reload support speeds development. |
| **Rendering** | Server-side Razor + inline SVG | The reference design uses simple SVG elements (lines, circles, polygons) and CSS Grid — both achievable with native Blazor markup. No charting library needed. |
| **CSS layout** | CSS Grid + Flexbox | Directly matches the reference design's layout strategy. Grid for heatmap (`160px repeat(N,1fr)`), flexbox for header and timeline container. |
| **Data serialization** | `System.Text.Json` | Built into .NET 8. Faster than Newtonsoft. Zero additional packages. Handles `DateTime` and nested objects natively. |
| **Data storage** | Flat JSON file | Trivially small data volume (< 100 items). Human-editable by PMs. No query requirements. Database would add complexity for zero benefit. |
| **File watching** | `FileSystemWatcher` | Built into .NET BCL. Enables auto-reload workflow. Debounce timer handles VS Code's multi-event save pattern. |
| **Styling approach** | Single global `site.css` | The reference CSS is ~80 lines. CSS isolation (`.razor.css`) adds complexity for minimal benefit at this scale. A single file is easier to diff against the reference HTML. |
| **Component library** | None (rejected MudBlazor, Radzen, etc.) | The design requires no form inputs, dialogs, or data tables. Reference CSS is already written. Adding a component library introduces 2MB+ of unused dependencies and fights with pixel-perfect matching. |
| **JavaScript** | None | All rendering is achievable server-side. SVG coordinate math runs in C#. No interactive features require JS. |
| **Additional NuGet packages** | None | Constraint: zero packages beyond default Blazor Server template. `System.Text.Json` and `FileSystemWatcher` are both in-box. |
| **Font** | Segoe UI (system font) | Matches reference design. Installed on Windows by default. No web font download needed. Falls back to Arial on other platforms. |

### Rejected Alternatives

| Alternative | Why Rejected |
|-------------|-------------|
| Blazor WASM | Adds WASM download overhead; more complex hosting; no benefit for single-user local tool |
| MudBlazor / Radzen | 2MB+ unused dependencies; component styles would conflict with pixel-perfect reference CSS |
| BlazorApexCharts | Requires JS interop; chart types don't match our custom SVG timeline layout |
| SQLite / EF Core | Database adds migration complexity for trivially small data; JSON is human-editable |
| Newtonsoft.Json | Additional NuGet package; `System.Text.Json` is built-in and faster |

---

## Security Considerations

### Threat Model

**Attack surface: None.** This application binds exclusively to localhost, serves a single user, processes no credentials, and stores no sensitive data. It is functionally equivalent to opening a local HTML file in a browser.

### Authentication & Authorization

**Not implemented.** No login, no user accounts, no role-based access. The application is a local development tool. Adding authentication would violate the "zero overhead" design goal.

**Future consideration:** If the tool is ever deployed on an intranet, Windows Authentication (`Negotiate`) via Kestrel would be the lowest-friction option — zero user management, leverages existing AD credentials. This is not designed or built now.

### Data Protection

| Concern | Status |
|---------|--------|
| **Sensitive data** | None. `data.json` contains project status text equivalent to PowerPoint slides. |
| **PII** | None collected, stored, or processed. |
| **Encryption at rest** | Not required — no sensitive data. |
| **Encryption in transit** | Not required — localhost only. HTTPS can be enabled via Kestrel if desired but provides no security benefit for `127.0.0.1`. |
| **Network exposure** | Kestrel binds to `localhost` only. No external interfaces are exposed. |

### Input Validation

| Input | Validation |
|-------|-----------|
| `data.json` content | `System.Text.Json` deserialization validates structure. `JsonException` on malformed JSON or type mismatches. Service catches exceptions and sets `ErrorMessage`. |
| `data.json` path | Hardcoded to `wwwroot/data.json`. No user-supplied file paths. |
| URL parameters | None. Single route (`/`), no query parameters. |
| User input | None. The dashboard is display-only. No forms, no text fields, no uploads. |

### Blazor Framework Security

- Blazor Server's default antiforgery protection is enabled (no reason to disable it)
- No custom SignalR hub methods — only framework-managed DOM diffing
- No JS interop — eliminates XSS vector from JavaScript execution
- Static files served from `wwwroot/` only — no directory traversal risk

---

## Scaling Strategy

### Current Design: Not Applicable

This is a **single-user, single-machine, local tool**. There are no scaling requirements. The data volume is trivially small (< 100 items), the page renders in < 1 second, and memory usage is < 100 MB.

### Hypothetical Scale-Up Path (Not Built)

If future requirements demand serving multiple users:

| Scale Level | Change Required |
|-------------|----------------|
| **2-5 concurrent users** (team intranet) | Deploy behind IIS with Windows Auth. No code changes needed — Blazor Server handles multiple SignalR connections natively. |
| **Shared data source** | Move `data.json` to a shared network path (e.g., `\\server\reports\data.json`). `FileSystemWatcher` works over SMB. |
| **Multiple projects** | One `data.json` per project; route parameter (`/project/{name}`) selects the file. |
| **Historical tracking** | Migrate `data.json` to SQLite; add a `ReportDate` table. This would be a significant refactor and is explicitly out of scope. |

**These are design notes only. None of this should be built in the current iteration.**

---

## UI Component Architecture

### Visual Section → Component Mapping

| Visual Section (from `OriginalDesignConcept.html`) | Blazor Component | CSS Layout Strategy | Data Bindings | Interactions |
|-----------------------------------------------------|-----------------|---------------------|---------------|-------------|
| **Header bar** (`.hdr`) — title, subtitle, legend | `DashboardHeader.razor` | Flexbox: `display:flex; align-items:center; justify-content:space-between`. Padding `12px 44px 10px`. Border-bottom `1px solid #E0E0E0`. | `@Title`, `@Subtitle`, `@BacklogUrl`, `@CurrentDate` (for legend "Now" label) | Backlog link (`<a href="@BacklogUrl">`) is clickable |
| **Legend icons** (PoC diamond, Prod diamond, checkpoint circle, NOW bar) | Inline HTML/CSS in `DashboardHeader.razor` | Inline flex containers with `gap:22px`. Diamond = `12px span, rotate(45deg)`. Circle = `8px border-radius:50%`. Bar = `2x14px`. | `@CurrentDate` for dynamic "Now (Mon YYYY)" text | None (display-only) |
| **Timeline left panel** — milestone labels (230px) | Part of `TimelineSection.razor` (inline `<div>`) | Flex column, `width:230px; flex-shrink:0; justify-content:space-around`. Border-right `1px solid #E0E0E0`. | `@foreach (var m in Milestones)` → renders `m.Id` in `m.Color` and `m.Label` in #444 | None |
| **Timeline SVG** (`.tl-svg-box`) — grid lines, bars, markers, NOW line | Inline `<svg>` in `TimelineSection.razor` | SVG viewport `width="1560" height="185"`. Elements positioned via `DateToX()` C# method. | `@Milestones` (bars + events), `@StartDate`/`@EndDate` (grid lines), `@CurrentDate` (NOW line) | None (display-only) |
| **Month grid lines** | `<line>` elements in SVG | Vertical lines at `DateToX(monthStart)`, y1=0, y2=185. Stroke `#bbb`, opacity 0.4. | Computed from `@StartDate`/`@EndDate` month boundaries | None |
| **Milestone bars** | `<line>` elements in SVG | Horizontal lines spanning x1=0 to x2=1560, stroke-width 3. Y per swim lane. | `@foreach milestone` → color from `m.Color`, y from lane index | None |
| **Checkpoint markers** | `<circle>` elements in SVG | `r="5-7"`, `fill="white"`, `stroke="@milestoneColor"`, `stroke-width="2.5"`. Minor: `r="4"`, `fill="#999"`. | `DateToX(event.Date)` for cx; lane y for cy | None |
| **PoC/Production diamonds** | `<polygon>` elements in SVG | Diamond points computed from center (cx,cy) ± 11px. PoC: `fill="#F4B400"`. Prod: `fill="#34A853"`. Both: `filter="url(#sh)"` (drop shadow). | `DateToX(event.Date)` for center x | None |
| **NOW line** | `<line>` + `<text>` in SVG | Dashed vertical line: `stroke="#EA4335"`, `stroke-width="2"`, `stroke-dasharray="5,3"`. Text: "NOW" in #EA4335, font-size 10, font-weight 700. | `DateToX(@CurrentDate)` for x position | None |
| **Event labels** | `<text>` elements in SVG | `text-anchor="middle"`, `fill="#666"`, `font-size="10"`. Above: `y = laneY - 16`. Below: `y = laneY + 24`. | `event.Label`, `event.LabelPosition` (above/below) | None |
| **Heatmap title** (`.hm-title`) | Part of `HeatmapGrid.razor` (inline `<div>`) | `font-size:14px; font-weight:700; color:#888; text-transform:uppercase; letter-spacing:.5px; margin-bottom:8px` | Static text: "Monthly Execution Heatmap" | None |
| **Heatmap grid** (`.hm-grid`) | `HeatmapGrid.razor` | CSS Grid: `grid-template-columns: 160px repeat(@Months.Count, 1fr); grid-template-rows: 36px repeat(4, 1fr)`. Border `1px solid #E0E0E0`. | `@Months` (column count + headers), `@Categories` (row data), `@CurrentMonthIndex` (highlight) | None |
| **Corner cell** (`.hm-corner`) | Part of `HeatmapGrid.razor` | Background #F5F5F5, centered "STATUS" text in 11px bold uppercase #999 | Static | None |
| **Month column headers** (`.hm-col-hdr`) | Part of `HeatmapGrid.razor` | Background #F5F5F5, 16px bold centered. Current month: background `#FFF0D0`, color `#C07700` | `@Months[i]`, conditional class when `i == @CurrentMonthIndex` | None |
| **Row headers** (`.hm-row-hdr`) | Part of `HeatmapGrid.razor` | Per-category background and text colors. 11px bold uppercase, letter-spacing 0.7px. Border-right 2px solid #CCC. | `@category.Name` with emoji prefix based on `colorScheme` | None |
| **Data cells** | `HeatmapCell.razor` (rendered N × 4 times) | Padding `8px 12px`. Items as `<div class="it">` with `::before` colored dot (6×6px circle). Current month cells get darker background via conditional CSS class. | `@Items` list, `@ColorScheme` (determines dot/bg color), `@IsCurrentMonth` | None |

### CSS Class Naming Convention

CSS classes are ported directly from the reference HTML to minimize visual divergence:

- `.hdr`, `.sub` — header section
- `.tl-area`, `.tl-svg-box` — timeline section
- `.hm-wrap`, `.hm-title`, `.hm-grid` — heatmap wrapper
- `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell` — grid cells
- `.ship-hdr`, `.ship-cell`, `.prog-hdr`, `.prog-cell`, `.carry-hdr`, `.carry-cell`, `.block-hdr`, `.block-cell` — category-specific colors
- `.current-month` — replaces `.apr` / `.apr-hdr` (dynamic instead of hardcoded to April)
- `.it` — individual bullet item within a cell

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|------------|
| 1 | **SVG rendering differences between Chrome and Edge** | Low | Medium | Target Chrome as the official screenshot browser. Document this in README. Edge (Chromium) should render identically but is not the primary target. |
| 2 | **CSS Grid not filling exactly 1080px height** | Low | High | Use fixed `body { width: 1920px; height: 1080px; overflow: hidden }` matching the reference. Timeline area is fixed 196px; header is flex-shrink 0; heatmap fills remaining space with `flex: 1`. Test with Chrome DevTools device emulation at exactly 1920×1080. |
| 3 | **`FileSystemWatcher` fires multiple events on save** | High | Low | Debounce with a 300ms `System.Threading.Timer`. Reset timer on each event; only reload after 300ms of quiet. This handles VS Code's temp-file-then-rename save pattern. |
| 4 | **`FileSystemWatcher` file locked during save** | Medium | Low | Catch `IOException` in `LoadData()` and retry once after 100ms delay. If retry fails, keep previous data and log warning. |
| 5 | **`data.json` schema drift breaking existing files** | Medium | Medium | All model properties use `{ get; set; }` with defaults. New optional fields use `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`. Never remove or rename existing fields without a major version bump. |
| 6 | **Blazor reconnect dialog appearing in screenshots** | Low | High | Suppress Blazor's reconnect UI by adding `<div id="blazor-error-ui" style="display:none"></div>` in `_Host.cshtml` / `App.razor`. For screenshots, the page is loaded fresh — reconnect only triggers after network interruption, which doesn't happen on localhost. |
| 7 | **Over-engineering the solution** | High | Medium | Enforce the 15-file cap. No additional NuGet packages. No abstractions beyond the component/service split. The reference HTML is ~200 lines — the Blazor version should be comparably lean. Code review should reject unnecessary complexity. |
| 8 | **SVG label text overlapping** | Medium | Low | The `labelPosition` field in `data.json` gives the user explicit control over above/below placement. Default alternation (even=above, odd=below) handles most cases. For edge cases, the user adjusts JSON. |
| 9 | **Segoe UI not available on macOS/Linux** | Low | Low | Font stack falls back to Arial: `'Segoe UI', Arial, sans-serif`. Screenshots are taken on Windows (where Segoe UI is guaranteed). macOS/Linux rendering may differ slightly but this is documented as untested. |
| 10 | **Blazor Server SignalR connection overhead** | Low | Low | Negligible for single-user localhost. The page is essentially static after initial render. SignalR is only used for the initial render and `StateHasChanged` pushes (file change events). No ongoing interactivity. |

### Trade-off Summary

| Decision | What We Gain | What We Give Up |
|----------|-------------|-----------------|
| No charting library | Pixel-perfect control over SVG; zero dependencies | Must hand-code SVG coordinate math |
| No database | Zero setup; human-editable data file | Cannot query historical data |
| No responsive design | Simpler CSS; guaranteed 1920×1080 output | Only works at one resolution |
| No authentication | Zero friction; instant `dotnet run` | Cannot share as a web service |
| Single `site.css` (no CSS isolation) | Easy to diff against reference HTML; single source of truth | No component-scoped style encapsulation |
| Flat JSON (no JSON Schema validation) | Simpler implementation | Errors detected at deserialization time, not at edit time |

---

## File Inventory (14 files)

```
ReportingDashboard.sln                              # Solution file
ReportingDashboard/
├── ReportingDashboard.csproj                       # Project file (net8.0, no extra packages)
├── Program.cs                                      # Entry point, DI, Kestrel config
├── Models/
│   └── DashboardData.cs                            # All model classes (single file)
├── Services/
│   └── DashboardDataService.cs                     # JSON reader + FileSystemWatcher
├── Components/
│   ├── App.razor                                   # Root component (routes + head)
│   ├── Routes.razor                                # Router configuration
│   ├── Layout/
│   │   └── MainLayout.razor                        # Minimal layout (@Body only)
│   ├── Pages/
│   │   └── Dashboard.razor                         # The single page (orchestrator)
│   ├── DashboardHeader.razor                       # Header bar + legend
│   ├── TimelineSection.razor                       # SVG timeline + milestone labels
│   ├── HeatmapGrid.razor                           # CSS Grid + HeatmapCell instances
│   └── HeatmapCell.razor                           # Individual cell with bullet items
├── wwwroot/
│   ├── css/site.css                                # All styles (ported from reference)
│   └── data.json                                   # Sample data (ships with project)
└── Properties/
    └── launchSettings.json                         # Dev server config
```

**Total: 14 meaningful files** (excluding `bin/`, `obj/`, `.gitignore`). Well within the 15-file constraint.