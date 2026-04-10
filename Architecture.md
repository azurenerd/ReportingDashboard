# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a local-only, single-page Blazor Server application built on .NET 8 that renders project status data from a `data.json` file into a screenshot-optimized view for PowerPoint executive decks.

**Primary architectural goals:**

1. **Minimal complexity** — Single project, single page, single data file. No layers, no abstractions, no patterns beyond what the problem demands.
2. **Screenshot fidelity** — Every architectural decision prioritizes deterministic, pixel-consistent rendering at 1280×900 resolution.
3. **Zero infrastructure** — No cloud, no database, no Docker, no reverse proxy. Runs on `localhost` via Kestrel.
4. **Sub-2-minute reporting cycle** — Edit `data.json` → auto-refresh → screenshot → paste into PowerPoint.
5. **Zero external dependencies** — Only built-in .NET 8 libraries (`System.Text.Json`, `System.IO.FileSystemWatcher`, ASP.NET Core).

**Architecture style:** Monolithic single-project application with a flat service-and-component structure. This is deliberately not Clean Architecture, not CQRS, not hexagonal. The problem is a file-to-pixels pipeline and the architecture reflects that.

```
data.json ──▸ DashboardDataService ──▸ Dashboard.razor ──▸ Browser (screenshot)
                 (singleton, DI)        (component tree)     (1280×900 fixed)
```

---

## System Components

### 1. Data Models (`Models/DashboardData.cs`)

**Responsibility:** Define the strongly-typed contract between the JSON file and the rest of the application. Serves as the schema definition — if a field doesn't exist in these records, it doesn't exist in the system.

**Interfaces:** None. Pure data transfer objects (C# records).

**Dependencies:** None.

**Data:**

```csharp
// Root container — maps 1:1 to the top-level data.json structure
public record DashboardData(
    ProjectInfo Project,
    List<Milestone> Milestones,
    List<WorkItem> Shipped,
    List<WorkItemInProgress> InProgress,
    List<CarriedOverItem> CarriedOver,
    List<KeyMetric> Metrics
);

public record ProjectInfo(
    string Name,
    string Executive,
    string Status,           // "On Track" | "At Risk" | "Off Track"
    string ReportDate,       // ISO 8601: "2026-04-10"
    string ReportingPeriod   // Display string: "March 2026"
);

public record Milestone(
    string Title,
    string Date,             // ISO 8601: "2026-01-15"
    string Status,           // "Completed" | "In Progress" | "Upcoming" | "Delayed"
    string Description
);

public record WorkItem(
    string Title,
    string Description,
    string Category,
    string Priority          // "P0" | "P1" | "P2" | "P3"
);

public record WorkItemInProgress(
    string Title,
    string Description,
    string Category,
    string Priority,
    int PercentComplete      // 0–100
);

public record CarriedOverItem(
    string Title,
    string Description,
    string Category,
    string Priority,
    string OriginalTarget,   // Display string: "February 2026"
    string Reason
);

public record KeyMetric(
    string Label,
    string Value,            // Display string: "42 pts", "2.1%", "85%"
    string Trend             // "up" | "down" | "stable"
);
```

**Design decisions:**
- Separate record types for `WorkItem`, `WorkItemInProgress`, and `CarriedOverItem` rather than a single polymorphic type. Each has distinct fields (`PercentComplete`, `OriginalTarget`, `Reason`) and conflating them would require nullable properties that obscure the contract.
- All date fields are strings, not `DateTime` or `DateOnly`. The dashboard displays dates as-is from JSON; parsing is only needed for the timeline positioning logic, which is handled in the component.
- `Status` and `Trend` are strings, not enums. This keeps JSON deserialization simple and avoids crashes on unexpected values — the rendering layer applies fallback styling for unknown values.

---

### 2. Dashboard Data Service (`Services/DashboardDataService.cs`)

**Responsibility:** Single point of data access. Reads `data.json`, deserializes it, watches for changes, and notifies subscribers when data updates. This is the only component that touches the file system.

**Interface:**

```csharp
public interface IDashboardDataService : IDisposable
{
    DashboardData? Data { get; }
    string? LoadError { get; }
    bool IsLoaded { get; }
    event Action? OnDataChanged;
    Task LoadAsync();
}
```

**Dependencies:**
- `IConfiguration` — to read the `data.json` file path from `appsettings.json`
- `ILogger<DashboardDataService>` — structured logging for load/watch events
- `System.IO.FileSystemWatcher` — file change detection
- `System.Text.Json.JsonSerializer` — deserialization

**Registration:** Singleton in DI container. One instance for the lifetime of the application.

**Behavior:**

```
Startup
  ├── Read DataFilePath from IConfiguration (default: "wwwroot/data/data.json")
  ├── LoadAsync() — read file, deserialize, set Data property
  ├── Start FileSystemWatcher on the file's directory
  └── Start fallback polling timer (5-second interval)

On File Change (watcher or poll)
  ├── Debounce (300ms) to avoid duplicate events from editors
  ├── Re-read and deserialize file
  ├── If success: update Data, clear LoadError, fire OnDataChanged
  └── If failure: set LoadError with message, keep previous Data, fire OnDataChanged

On Dispose
  ├── Stop FileSystemWatcher
  └── Stop polling timer
```

**Error handling strategy:**
- Malformed JSON → `LoadError` is set to the `JsonException.Message`. Previous valid `Data` is retained. The dashboard displays an error banner but continues showing the last good data.
- Missing file → `LoadError` = "data.json not found at {path}". `Data` remains null. Dashboard shows a full-page error with instructions.
- File locked → Retry up to 3 times with 200ms delay (editors sometimes hold write locks briefly).

**Implementation details:**

```csharp
public class DashboardDataService : IDashboardDataService
{
    private readonly string _filePath;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private FileSystemWatcher? _watcher;
    private Timer? _pollingTimer;
    private string? _lastFileHash;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public DashboardData? Data { get; private set; }
    public string? LoadError { get; private set; }
    public bool IsLoaded => Data != null;
    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration configuration, ILogger<DashboardDataService> logger)
    {
        _filePath = configuration.GetValue<string>("DashboardDataPath")
                    ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "data.json");
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    // LoadAsync, StartWatcher, StartPolling, OnFileChanged, Dispose...
}
```

**Configuration (`appsettings.json`):**

```json
{
  "DashboardDataPath": "wwwroot/data/data.json",
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

---

### 3. Blazor Component Tree (`Components/`)

The component hierarchy is flat and data flows strictly downward via parameters. No component fetches its own data. No component writes data. No component communicates with siblings.

```
App.razor
 └── Routes.razor
      └── MainLayout.razor (minimal — no nav, no sidebar)
           └── Dashboard.razor (@page "/")
                ├── ErrorBanner.razor          (conditional: shown when LoadError is set)
                ├── ProjectHeader.razor         (project name, sponsor, status, dates)
                ├── MilestoneTimeline.razor     (horizontal timeline strip)
                ├── WorkItemSection.razor        (×3: Shipped, In Progress, Carried Over)
                │    └── WorkItemCard.razor      (individual item card — repeated)
                └── MetricsBar.razor            (row of metric badges)
                     └── MetricBadge.razor       (individual metric — repeated)
```

#### 3a. `Dashboard.razor` (Page Component)

**Responsibility:** Root page. Injects `IDashboardDataService`, subscribes to `OnDataChanged`, passes data slices to child components as `[Parameter]` values.

**Route:** `@page "/"`

**Key behavior:**
- On initialization, subscribes to `IDashboardDataService.OnDataChanged`.
- On change notification, calls `InvokeAsync(StateHasChanged)` to trigger Blazor re-render.
- Reads `?print=true` query parameter from `NavigationManager` to toggle print mode CSS class.
- Disposes event subscription on teardown.

```razor
@page "/"
@implements IDisposable
@inject IDashboardDataService DataService
@inject NavigationManager Navigation

<div class="dashboard @(_printMode ? "print-mode" : "")">
    @if (DataService.LoadError is not null)
    {
        <ErrorBanner Message="@DataService.LoadError" />
    }

    @if (DataService.Data is not null)
    {
        var data = DataService.Data;
        <ProjectHeader Project="@data.Project" />
        <MilestoneTimeline Milestones="@data.Milestones" />

        <div class="content-grid">
            <WorkItemSection Title="Shipped" Items="@MapShipped(data.Shipped)"
                             StatusClass="shipped" Icon="checkmark" />
            <WorkItemSection Title="In Progress" InProgressItems="@data.InProgress"
                             StatusClass="in-progress" Icon="arrow-right" />
            <WorkItemSection Title="Carried Over" CarriedOverItems="@data.CarriedOver"
                             StatusClass="carried-over" Icon="alert" />
        </div>

        <MetricsBar Metrics="@data.Metrics" />
    }
</div>
```

#### 3b. `ProjectHeader.razor`

**Parameter:** `ProjectInfo Project`

**Renders:** Project name (h1), executive sponsor, reporting period, report date, status badge with color coding.

**Status badge mapping:**
| Status | CSS Class | Background | Text |
|--------|-----------|------------|------|
| On Track | `status-on-track` | `#28a745` (green) | White |
| At Risk | `status-at-risk` | `#ffc107` (amber) | Black |
| Off Track | `status-off-track` | `#dc3545` (red) | White |
| _unknown_ | `status-unknown` | `#6c757d` (gray) | White |

#### 3c. `MilestoneTimeline.razor`

**Parameter:** `List<Milestone> Milestones`

**Renders:** Horizontal timeline strip with positioned milestone markers.

**Layout algorithm:**
1. Parse all milestone dates to `DateOnly`.
2. Determine the timeline range: `min(all dates) - 30 days` to `max(all dates) + 30 days`.
3. Calculate each milestone's horizontal position as a percentage: `(date - rangeStart) / (rangeEnd - rangeStart) * 100`.
4. Position a "Today" marker using the same calculation with `DateOnly.FromDateTime(DateTime.Today)`.
5. Render using CSS `position: relative` on the container with `position: absolute; left: {percent}%` on each marker.

**Milestone status indicators:**
| Status | Visual | CSS |
|--------|--------|-----|
| Completed | Filled green circle | `background: var(--status-completed); border-radius: 50%` |
| In Progress | Half-filled blue circle | `background: linear-gradient(90deg, var(--status-in-progress) 50%, transparent 50%); border: 2px solid var(--status-in-progress)` |
| Upcoming | Hollow gray circle | `border: 2px solid var(--status-upcoming); background: white` |
| Delayed | Red-outlined circle | `border: 2px solid var(--status-delayed); background: white` |

**Today marker:** Vertical dashed line, red, with "Today" label above, `z-index: 10`.

#### 3d. `WorkItemSection.razor`

**Parameters:**
- `string Title`
- `string StatusClass` — CSS modifier class ("shipped", "in-progress", "carried-over")
- `string Icon` — icon identifier for section header
- `List<WorkItemDisplay>? Items` — for shipped items
- `List<WorkItemInProgress>? InProgressItems` — for in-progress items
- `List<CarriedOverItem>? CarriedOverItems` — for carried-over items

**Renders:** Section with header, left-colored border per status class, and a list of item cards.

**WorkItemCard sub-rendering:**
- **Shipped:** Green left border, green checkmark, title, description, category badge, priority badge.
- **In Progress:** Blue left border, progress bar showing `PercentComplete`, title, description, category badge, priority badge.
- **Carried Over:** Amber/orange left border, title, description, category badge, priority badge, original target date, reason (prominently displayed, not truncated).

#### 3e. `MetricsBar.razor` and `MetricBadge.razor`

**Parameter (MetricsBar):** `List<KeyMetric> Metrics`

**Renders:** Horizontal row of metric cards using CSS flexbox with equal spacing.

**MetricBadge renders:**
- Label (small, muted text above)
- Value (large, bold number)
- Trend arrow:
  - `up` → green upward arrow (▲) or CSS triangle
  - `down` → red downward arrow (▼) or CSS triangle
  - `stable` → gray horizontal dash (―) or CSS line

#### 3f. `ErrorBanner.razor`

**Parameter:** `string Message`

**Renders:** Fixed-position banner at top of page with red background, white text, and the error message. Dismissible only by fixing the data error (auto-clears on successful reload).

---

### 4. CSS Architecture (`wwwroot/css/app.css`)

**Responsibility:** All visual styling. Single global stylesheet plus scoped `.razor.css` files for component-specific styles.

**Design system:**

```css
:root {
    /* Status colors */
    --status-shipped: #28a745;
    --status-in-progress: #007bff;
    --status-carried-over: #fd7e14;
    --status-at-risk: #ffc107;
    --status-off-track: #dc3545;
    --status-completed: #28a745;
    --status-upcoming: #6c757d;
    --status-delayed: #dc3545;

    /* Priority colors */
    --priority-p0: #dc3545;
    --priority-p1: #fd7e14;
    --priority-p2: #ffc107;
    --priority-p3: #6c757d;

    /* Layout */
    --dashboard-width: 1280px;
    --dashboard-min-height: 900px;

    /* Typography */
    --font-family: 'Segoe UI', -apple-system, BlinkMacSystemFont, sans-serif;
    --font-size-title: 24px;
    --font-size-section-header: 16px;
    --font-size-body: 13px;
    --font-size-small: 11px;

    /* Surfaces */
    --bg-page: #f4f6f9;
    --bg-card: #ffffff;
    --bg-header: #1a2332;
    --text-primary: #212529;
    --text-secondary: #6c757d;
    --text-on-dark: #ffffff;
    --border-color: #dee2e6;
    --shadow-card: 0 1px 3px rgba(0, 0, 0, 0.08);
}
```

**Fixed layout rules:**

```css
.dashboard {
    width: var(--dashboard-width);
    min-height: var(--dashboard-min-height);
    margin: 0 auto;
    background-color: var(--bg-page);
    font-family: var(--font-family);
    font-size: var(--font-size-body);
    color: var(--text-primary);
    overflow: hidden; /* Prevent scrollbars in screenshots */
}
```

**Print and screenshot mode:**

```css
@media print {
    body { margin: 0; padding: 0; }
    .dashboard { width: 100%; box-shadow: none; }
    .blazor-error-ui, #blazor-error-ui { display: none !important; }
}

.dashboard.print-mode {
    /* Hides Blazor reconnection UI and any non-content elements */
}

.dashboard.print-mode #blazor-error-ui,
.dashboard.print-mode .blazor-reconnect-modal {
    display: none !important;
}
```

**Typography scale:** Enforced minimums per the PM spec — 12px body, 16px section headers, 24px project title.

---

### 5. Application Host (`Program.cs`)

**Responsibility:** Minimal host configuration. Register services, configure Kestrel, map Blazor.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<IDashboardDataService, DashboardDataService>();

// Configure Kestrel for localhost-only binding
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // HTTP only — no HTTPS cert warnings in screenshots
});

var app = builder.Build();

// Initialize data service — load data.json before accepting requests
var dataService = app.Services.GetRequiredService<IDashboardDataService>();
await dataService.LoadAsync();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

**Key decisions:**
- HTTP only (port 5000) — avoids HTTPS certificate warnings that would appear in screenshots.
- `ListenLocalhost` — not network-accessible, as specified.
- Data is loaded before `app.Run()` — the first request always has data available.
- No authentication middleware. No CORS. No CSP. No HSTS.

---

## Component Interactions

### Data Flow (Read Path)

```
┌──────────────┐     ┌───────────────────────┐     ┌──────────────────┐
│  data.json   │────▸│  DashboardDataService  │────▸│  Dashboard.razor │
│  (file)      │     │  (singleton in DI)     │     │  (page component)│
└──────────────┘     └───────────────────────┘     └──────────────────┘
                              │                            │
                     Exposes: Data property        Passes data via
                     Fires: OnDataChanged          [Parameter] to:
                              │                            │
                              ▼                            ▼
                     ┌─────────────┐              ┌────────────────┐
                     │ FileSystem  │              │ Child components│
                     │ Watcher +   │              │ (pure render)   │
                     │ Poll Timer  │              └────────────────┘
                     └─────────────┘
```

### Auto-Refresh Flow

```
1. User edits data.json in text editor and saves
2. FileSystemWatcher fires Changed event (or polling timer detects file hash change)
3. DashboardDataService.OnFileChanged():
   a. Debounce 300ms (ignore rapid successive events)
   b. Acquire _loadLock semaphore
   c. Read file bytes, compute hash
   d. If hash unchanged from _lastFileHash → skip (duplicate event)
   e. Deserialize JSON into DashboardData
   f. If success → update Data, clear LoadError
   g. If failure → set LoadError, retain previous Data
   h. Release semaphore
   i. Fire OnDataChanged event
4. Dashboard.razor receives OnDataChanged:
   a. Calls InvokeAsync(StateHasChanged)
   b. Blazor Server pushes DOM diff over SignalR to browser
5. Browser updates without page reload — under 5 seconds end-to-end
```

### Component Communication Pattern

**Strictly unidirectional, top-down.** No child-to-parent callbacks. No shared state service beyond `IDashboardDataService`. No event bus.

```
Dashboard.razor
  │
  ├── ProjectHeader    ← receives: ProjectInfo (immutable record)
  ├── MilestoneTimeline ← receives: List<Milestone> (immutable list)
  ├── WorkItemSection   ← receives: typed item lists + display config
  │    └── WorkItemCard ← receives: single item record
  └── MetricsBar        ← receives: List<KeyMetric>
       └── MetricBadge  ← receives: single KeyMetric record
```

No component stores local state beyond what it receives via parameters. This guarantees that every re-render from a data change propagates consistently through the entire tree.

---

## Data Model

### Entity Relationship

```
DashboardData (root)
 ├── 1:1  ProjectInfo
 ├── 1:N  Milestone[]
 ├── 1:N  WorkItem[]           (shipped)
 ├── 1:N  WorkItemInProgress[] (in progress)
 ├── 1:N  CarriedOverItem[]    (carried over)
 └── 1:N  KeyMetric[]
```

There are no relationships between entities. No foreign keys. No joins. Each array is independent. This is a document model, not a relational model.

### Storage

**Format:** Single JSON file (`data.json`)

**Location:** Configurable via `appsettings.json` key `DashboardDataPath`. Default: `wwwroot/data/data.json`.

**Schema enforcement:** The C# record types are the schema. `System.Text.Json` deserialization with `PropertyNameCaseInsensitive = true` handles the mapping. There is no separate JSON Schema file — the C# types are the source of truth.

**Validation rules (applied at load time):**

| Field | Rule | On Violation |
|-------|------|-------------|
| `project` | Required, non-null | `LoadError`: "Missing required 'project' section" |
| `project.name` | Required, non-empty string | `LoadError`: "Project name is required" |
| `project.status` | Should be one of: "On Track", "At Risk", "Off Track" | Log warning, render with gray fallback badge |
| `milestones[].date` | Should be parseable as `DateOnly` (yyyy-MM-dd) | Log warning, exclude from timeline positioning |
| `milestones[].status` | Should be one of: "Completed", "In Progress", "Upcoming", "Delayed" | Log warning, render as "Upcoming" (gray) |
| `inProgress[].percentComplete` | Should be 0–100 | Clamp to 0–100 range |
| `metrics[].trend` | Should be one of: "up", "down", "stable" | Render as "stable" (gray dash) |
| All arrays | Optional (can be empty or missing) | Render empty section; do not crash |

### Sample `data.json`

Provided as `wwwroot/data/data.json` in the project with fictional "Project Phoenix" data. Contains:
- 1 project info block
- 5–6 milestones spanning past and future dates
- 3–4 shipped items
- 2–3 in-progress items with varying completion percentages
- 1–2 carried-over items with reasons
- 3–4 key metrics with mixed trends

---

## API Contracts

### External APIs

**None.** This application exposes no REST, GraphQL, gRPC, or WebSocket endpoints. There is no API layer.

### Internal Service Contract

The only internal contract is the `IDashboardDataService` interface:

```csharp
public interface IDashboardDataService : IDisposable
{
    /// <summary>
    /// The current deserialized dashboard data. Null if never successfully loaded.
    /// </summary>
    DashboardData? Data { get; }

    /// <summary>
    /// Human-readable error message from the last failed load attempt.
    /// Null when data is successfully loaded.
    /// </summary>
    string? LoadError { get; }

    /// <summary>
    /// True if Data is non-null (at least one successful load has occurred).
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Fired when Data or LoadError changes. Subscribers must call
    /// InvokeAsync(StateHasChanged) to update the Blazor UI.
    /// </summary>
    event Action? OnDataChanged;

    /// <summary>
    /// Reads and deserializes data.json. Called once at startup.
    /// Starts the FileSystemWatcher and polling timer.
    /// </summary>
    Task LoadAsync();
}
```

### JSON File Contract

The `data.json` file is the only "API" the project manager interacts with. Its contract is defined by the C# record types in `Models/DashboardData.cs`. The JSON property names use camelCase:

```
Root Object
├── project: object (required)
│   ├── name: string (required)
│   ├── executive: string (required)
│   ├── status: string (required) — "On Track" | "At Risk" | "Off Track"
│   ├── reportDate: string (required) — "yyyy-MM-dd"
│   └── reportingPeriod: string (required) — display text
├── milestones: array (optional, default: [])
│   └── each: { title, date, status, description }
├── shipped: array (optional, default: [])
│   └── each: { title, description, category, priority }
├── inProgress: array (optional, default: [])
│   └── each: { title, description, category, priority, percentComplete }
├── carriedOver: array (optional, default: [])
│   └── each: { title, description, category, priority, originalTarget, reason }
└── metrics: array (optional, default: [])
    └── each: { label, value, trend }
```

### Error Responses

There are no HTTP error responses. Errors are rendered inline in the dashboard:

| Condition | User-Visible Behavior |
|-----------|----------------------|
| `data.json` not found | Full-page message: "Dashboard data file not found. Expected at: {path}" |
| `data.json` is invalid JSON | Red banner at top: "Error loading data: {JsonException.Message}". Last valid data remains displayed below. |
| `data.json` is valid JSON but missing `project` | Red banner: "Invalid data: missing required 'project' section". |
| File locked by another process | Transparent retry (3 attempts × 200ms). If all fail, red banner with retry message. |

---

## Infrastructure Requirements

### Hosting

| Requirement | Specification |
|-------------|---------------|
| **Runtime** | .NET 8 SDK (development) or self-contained publish (distribution) |
| **Web Server** | Kestrel (built into ASP.NET Core) — no IIS, no Nginx, no Apache |
| **Binding** | `http://localhost:5000` only |
| **Protocol** | HTTP (no TLS) — avoids certificate warnings in screenshots |
| **Process Model** | Single process, single thread pool (Kestrel default) |

### Networking

| Requirement | Specification |
|-------------|---------------|
| **Network access** | None required. Localhost only. |
| **Firewall rules** | None. Port 5000 on loopback only. |
| **DNS** | Not applicable |
| **Proxy** | Not applicable |

### Storage

| Requirement | Specification |
|-------------|---------------|
| **Disk space** | < 50 MB for self-contained publish |
| **File I/O** | Read-only access to `data.json` (configurable path) |
| **Database** | None |
| **Temp files** | None generated by the application |

### CI/CD

| Requirement | Specification |
|-------------|---------------|
| **Build** | `dotnet build` locally |
| **Test** | `dotnet test` locally (optional for MVP) |
| **Publish** | `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish` |
| **Distribution** | Copy the single `.exe` file. No installer needed. |
| **Pipeline** | None. Local build only. |

### Development Environment

| Requirement | Specification |
|-------------|---------------|
| **.NET SDK** | 8.0.x (LTS) |
| **IDE** | Visual Studio 2022 17.8+ or VS Code with C# Dev Kit |
| **Browser** | Chromium-based (Edge or Chrome) for screenshot consistency |
| **OS** | Windows (primary target — Segoe UI font, FileSystemWatcher behavior) |
| **Hot reload** | `dotnet watch run` for development iteration |

---

## Technology Stack Decisions

| Layer | Choice | Justification |
|-------|--------|---------------|
| **Runtime** | .NET 8 (LTS) | Mandated by project. LTS support through Nov 2026. |
| **Web Framework** | Blazor Server (Interactive Server) | Mandated. Enables C#-only development with real-time UI updates via SignalR — critical for FileSystemWatcher-driven auto-refresh without JavaScript. |
| **JSON Parser** | `System.Text.Json` (built-in) | Zero dependencies. High performance. Case-insensitive deserialization with `PropertyNameCaseInsensitive`. Supports comments and trailing commas for hand-edited files. |
| **File Watching** | `System.IO.FileSystemWatcher` + polling fallback | Built-in. FileSystemWatcher provides near-instant detection; 5-second polling timer covers known Windows reliability gaps. |
| **CSS** | Hand-rolled custom CSS | Full control over screenshot rendering. No CSS framework bloat. No unpredictable theme overrides. No build pipeline. |
| **CSS Methodology** | Scoped CSS (`.razor.css`) + global `app.css` | Component isolation via Blazor's built-in CSS isolation. Global styles for layout, typography, and CSS custom properties. |
| **Layout** | CSS Grid + Flexbox | Native browser support. No library needed. Grid for page structure, Flexbox for component internals. |
| **Icons** | CSS-only indicators (colored shapes, Unicode arrows) | Zero dependency. Deterministic rendering. No icon font CDN. No SVG sprite sheet. |
| **Font** | `Segoe UI` system font stack | Matches Windows and PowerPoint rendering. No web font download. Instant rendering. |
| **Data Models** | C# records | Immutable, concise, value-equal. Perfect for deserialization targets. |
| **DI** | Built-in ASP.NET Core DI | Singleton registration for `DashboardDataService`. No third-party container. |
| **Testing (optional)** | xUnit + bUnit | Standard .NET testing stack. Only recommended for JSON parsing layer. |
| **Package Manager** | NuGet | Only if a specific component demands it. MVP target: zero external packages. |

### Explicitly Rejected Technologies

| Technology | Reason for Rejection |
|------------|---------------------|
| MudBlazor / Radzen | Injects unpredictable CSS and JS that could shift layout between renders. Progress bars and timelines are achievable with pure CSS. |
| Tailwind CSS | Requires a build pipeline. Class-name noise in Razor markup. Overkill for a fixed-layout single page. |
| Bootstrap | Opinionated responsive grid fights the fixed 1280px requirement. Would need extensive overrides. |
| Newtonsoft.Json | Unnecessary. `System.Text.Json` handles all requirements with zero additional dependencies. |
| Entity Framework / SQLite | No database is needed. A single JSON file is the correct storage for this use case. |
| MediatR / AutoMapper | Enterprise patterns for a screenshot tool. Adds indirection with zero benefit. |
| SignalR (explicit) | Already included in Blazor Server circuit. No additional SignalR hubs needed. |
| Blazor WebAssembly | Requires downloading the .NET runtime to the browser. Slower startup. No server-side file watching. |
| Blazor Static SSR | Cannot push updates to the browser when `data.json` changes. Requires manual refresh. |

---

## Security Considerations

### Threat Model

This application has a minimal attack surface by design:

| Attack Vector | Risk Level | Assessment |
|---------------|-----------|------------|
| **Network exposure** | None | Binds to `localhost` only. Not reachable from other machines. |
| **Authentication bypass** | N/A | No authentication exists or is needed. |
| **Data exfiltration** | Negligible | `data.json` contains project names and dates, not PII or credentials. |
| **XSS via data.json** | Low | Blazor's default rendering escapes HTML. Razor `@` expressions are HTML-encoded by default. No `@((MarkupString)...)` usage. |
| **Path traversal** | Low | `DashboardDataPath` from `appsettings.json` is read once at startup. No user input controls file paths at runtime. |
| **Denial of Service** | Negligible | Single user, localhost only. |

### Data Protection

- `data.json` is **not encrypted**. It contains non-sensitive project metadata.
- If project names are sensitive, configure `DashboardDataPath` to a location outside `wwwroot/` (e.g., `C:\Reports\data.json`). Files in `wwwroot/` are served as static files by default.
- **Recommendation:** The `DashboardDataService` should read `data.json` from the configured path via direct file I/O, not via the static file middleware. This means even if `data.json` is in `wwwroot/`, the raw JSON is only accessible through the rendered dashboard, not via `http://localhost:5000/data/data.json` — unless static file serving is enabled for that path. To prevent direct access, either:
  - Place `data.json` outside `wwwroot/`, or
  - Add a static file filter that blocks `*.json` in `wwwroot/data/`.

### Input Validation

- All data enters the system through `data.json` deserialization. There are no user-submitted forms, no URL parameters (except the read-only `?print=true`), no file uploads.
- `System.Text.Json` handles malformed JSON safely — throws `JsonException` rather than executing arbitrary code.
- String values from JSON are rendered via Blazor's default `@` syntax, which HTML-encodes output. **Do not use `@((MarkupString)...)` on any data.json values.**

---

## Scaling Strategy

### Current Scale

This application is designed for exactly **one user on one machine viewing one project**. There is no scaling requirement.

| Dimension | Current Target | Scaling Path (if ever needed) |
|-----------|---------------|-------------------------------|
| **Users** | 1 | Not applicable. If multiple users need dashboards, each runs their own local instance. |
| **Projects** | 1 per `data.json` | Phase 3: Multiple `data.json` files with route-based switching (`/project/phoenix`, `/project/atlas`). |
| **Data volume** | < 1 MB JSON | Sufficient for hundreds of milestones and work items. No pagination needed. |
| **Concurrency** | 1 browser tab | Blazor Server handles multiple circuits, but only one is expected. |
| **Rendering** | 1 page, ~50 DOM elements | No virtualization or lazy loading needed. |

### Performance Budget

| Operation | Budget | Implementation |
|-----------|--------|----------------|
| `data.json` read + deserialize | < 100ms | `System.Text.Json` with `ReadAllBytesAsync` |
| Full page render | < 200ms | Flat component tree, no async loading, no lazy components |
| File change → UI update | < 5 seconds | FileSystemWatcher event → debounce 300ms → deserialize → SignalR push |
| Application startup | < 2 seconds | Minimal DI, no EF migrations, no health checks |
| Memory footprint | < 100 MB | No caching layer, no in-memory collections beyond the single `DashboardData` object |

---

## Risks & Mitigations

| # | Risk | Severity | Probability | Mitigation |
|---|------|----------|-------------|------------|
| 1 | **FileSystemWatcher misses change events on Windows** | Medium | Medium | Implement a 5-second polling fallback using `Timer` that computes a file content hash and triggers reload if changed. The watcher provides fast detection; polling provides reliability. Both paths converge on the same `OnFileChanged` handler. |
| 2 | **JSON schema drift between data.json and C# models** | Medium | Medium | C# records are the single source of truth. Provide a documented sample `data.json` with comments explaining each field. Use `PropertyNameCaseInsensitive = true` and `AllowTrailingCommas = true` to be forgiving. Log warnings for unexpected properties via a custom `JsonConverter` or post-deserialization validation. |
| 3 | **Screenshot visual inconsistency across displays** | Medium | Low | Fixed 1280px width eliminates responsive layout variability. Explicit `background-color` on all elements prevents transparency artifacts. Segoe UI font is pre-installed on all Windows machines. Test on 100% and 125% display scaling — document that 100% scaling produces the cleanest 1:1 screenshots. |
| 4 | **Over-engineering / scope creep** | High | High | Architecture deliberately constrains the solution: single project, single page, no API, no database, no auth. Any feature request that doesn't serve "edit JSON → screenshot → PowerPoint" is deferred to Phase 3 or rejected. |
| 5 | **Blazor Server SignalR circuit overhead** | Low | Low | Approximately 50KB of JS for the SignalR client. Negligible on localhost. The circuit enables the critical auto-refresh feature (server pushes DOM diff when data changes). The alternative (Blazor Static SSR) cannot push updates. This overhead is the correct trade-off. |
| 6 | **data.json file locked by editor during read** | Low | Medium | `DashboardDataService` retries file reads up to 3 times with 200ms delays using `FileShare.ReadWrite` mode. Most text editors release write locks immediately after save. VS Code and Notepad++ are confirmed compatible. |
| 7 | **Large data.json causes viewport overflow** | Medium | Low | The 1280×900 viewport has a finite capacity. With default font sizes: ~5 milestones, ~4 shipped items, ~3 in-progress items, ~2 carried-over items, and ~4 metrics fit without scrolling. If data exceeds this, the layout overflows. **Mitigation:** Add `overflow: hidden` on `.dashboard` for screenshot mode. Document recommended maximum item counts in the sample `data.json`. Future: add a `compact` mode that reduces font sizes and spacing. |
| 8 | **Project manager edits JSON incorrectly** | Medium | High | Provide a well-commented sample `data.json` with all fields. Configure `System.Text.Json` to be forgiving: `ReadCommentHandling.Skip`, `AllowTrailingCommas`, `PropertyNameCaseInsensitive`. On parse failure, show the exact error message including line number in the `ErrorBanner`. Recommend VS Code with its built-in JSON validation for editing. |
| 9 | **Blazor error UI overlays the dashboard during screenshot** | Low | Medium | Override default Blazor error UI CSS: `#blazor-error-ui { display: none; }`. In `?print=true` mode, hide all Blazor framework UI elements. Handle errors in the `ErrorBanner` component instead of relying on Blazor's default error boundary. |
| 10 | **Single-file publish produces large executable** | Low | Low | Self-contained .NET 8 publish for `win-x64` produces ~60-80MB executable. Acceptable for a local tool. If size is a concern, use framework-dependent publish (~5MB) and require .NET 8 runtime pre-installed. Document both options. |