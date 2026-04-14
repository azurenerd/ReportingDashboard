# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server application that renders project milestone timelines and monthly execution heatmaps at a fixed 1920×1080 resolution, optimized for PowerPoint screenshot capture. It reads all data from a local `data.json` file, requires zero infrastructure, and runs entirely on localhost via `dotnet run`.

**Architectural Goals:**

1. **Pixel-perfect fidelity** — The rendered output must be visually indistinguishable from `OriginalDesignConcept.html` at 1920×1080.
2. **Zero-dependency production build** — No external NuGet packages, no databases, no cloud services. Everything ships with the .NET 8 SDK.
3. **Data-driven rendering** — All visible content is sourced from `data.json`. No hardcoded text in UI components.
4. **Hot-reload workflow** — File changes to `data.json` propagate to the running dashboard without application restart.
5. **Multi-project support** — Different `data.json` files can be loaded via `?project=` query parameter.
6. **Sub-second render** — First meaningful paint completes in under 500ms on localhost.

**Architectural Pattern:** This is a **read-only server-side render pipeline**. Data flows one direction: `JSON file → Service → Page → Components → HTML`. There is no user input, no state mutation, no API surface, and no persistence beyond the source JSON file.

---

## System Components

### 1. `Program.cs` — Application Host

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Configure Kestrel, register DI services, set up middleware pipeline, configure static file serving |
| **Interfaces** | N/A — entry point only |
| **Dependencies** | `DashboardDataService`, `IConfiguration` |
| **Data** | Reads `appsettings.json` for configuration (data file path, Kestrel port) |

**Key behaviors:**
- Registers `DashboardDataService` as a **Singleton** in the DI container.
- Configures Kestrel to bind to `localhost` only (no external network exposure).
- Adds Blazor Server services via `builder.Services.AddRazorComponents().AddInteractiveServerComponents()`.
- Maps the Blazor hub endpoint and static files middleware.
- Calls `DashboardDataService.Initialize()` at startup to validate `data.json` exists and is parseable; logs a clear error and exits gracefully if not.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

// Validate data.json is loadable at startup
var dataService = app.Services.GetRequiredService<DashboardDataService>();
dataService.Initialize();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

---

### 2. `DashboardDataService` — Data Access Layer

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Load, parse, cache, validate, and hot-reload `data.json`; resolve multi-project file paths |
| **Interfaces** | `GetData(string? projectName = null): DashboardData`, `OnDataChanged: event Action` |
| **Dependencies** | `IConfiguration`, `IWebHostEnvironment`, `FileSystemWatcher` |
| **Data** | In-memory `DashboardData` object(s), cached per project name |

**Key behaviors:**

- **Initialization:** On first call or at startup, reads `data.json` (or `data.{project}.json`) from the content root directory, deserializes via `System.Text.Json` into strongly-typed `DashboardData` model.
- **Caching:** Stores deserialized data in a `ConcurrentDictionary<string, DashboardData>` keyed by project name (empty string for default). Cache is invalidated on file change.
- **File watching:** Creates a `FileSystemWatcher` on the content root directory, filtering for `data*.json`. On change, re-reads the affected file, updates the cache, and raises `OnDataChanged` event.
- **Validation:** After deserialization, validates required fields (`title`, `months`, `categories` with exactly 4 entries, at least 1 milestone). Throws `DashboardDataException` with a human-readable message on failure.
- **Multi-project resolution:** Given `projectName`, resolves to file path `data.{projectName}.json` in the content root. Falls back to checking `projects/{projectName}.json` subfolder. Returns a clear error if neither exists.
- **Thread safety:** Uses `lock` or `ConcurrentDictionary` for cache access since `FileSystemWatcher` callbacks arrive on thread pool threads.

```csharp
public class DashboardDataService : IDisposable
{
    private readonly string _contentRoot;
    private readonly ConcurrentDictionary<string, DashboardData> _cache = new();
    private readonly FileSystemWatcher _watcher;
    private readonly ILogger<DashboardDataService> _logger;

    public event Action? OnDataChanged;

    public DashboardData GetData(string? projectName = null)
    {
        var key = projectName ?? string.Empty;
        return _cache.GetOrAdd(key, k => LoadAndValidate(k));
    }

    private DashboardData LoadAndValidate(string projectName)
    {
        var path = ResolveFilePath(projectName);
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions)
            ?? throw new DashboardDataException($"Failed to deserialize {path}");
        Validate(data, path);
        return data;
    }

    private string ResolveFilePath(string projectName)
    {
        if (string.IsNullOrEmpty(projectName))
            return Path.Combine(_contentRoot, "data.json");

        var primary = Path.Combine(_contentRoot, $"data.{projectName}.json");
        if (File.Exists(primary)) return primary;

        var secondary = Path.Combine(_contentRoot, "projects", $"{projectName}.json");
        if (File.Exists(secondary)) return secondary;

        throw new DashboardDataException(
            $"Project '{projectName}' not found. Searched: {primary}, {secondary}");
    }

    public void Dispose() => _watcher?.Dispose();
}
```

---

### 3. `MainLayout.razor` — Layout Shell

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Provide the outermost HTML shell with no navigation chrome — just a full-width body container |
| **Interfaces** | Renders `@Body` |
| **Dependencies** | None |
| **Data** | None |

**Key behaviors:**
- Strips all default Blazor template navigation (sidebar, top bar, About link).
- Renders a single `<div>` wrapper with `@Body` inside.
- The associated `MainLayout.razor.css` sets the body wrapper to `width: 1920px; height: 1080px; overflow: hidden; display: flex; flex-direction: column`.

---

### 4. `Dashboard.razor` — Page Orchestrator

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Route handler for `/`; reads `?project=` query parameter; loads data from service; distributes data to child components; handles error states |
| **Interfaces** | Route: `/`, Query: `?project={name}` |
| **Dependencies** | `DashboardDataService`, `NavigationManager` |
| **Data** | `DashboardData` (full model passed down as parameters) |

**Key behaviors:**
- On `OnInitialized`, reads the `project` query parameter from `NavigationManager.Uri`.
- Calls `DashboardDataService.GetData(projectName)`.
- If a `DashboardDataException` is caught, sets an error state and renders a user-friendly error message panel instead of the dashboard.
- Passes data down to child components via `[Parameter]` properties — no cascading values.
- Subscribes to `DashboardDataService.OnDataChanged` and calls `InvokeAsync(StateHasChanged)` to push UI updates over the SignalR circuit when `data.json` changes.

```csharp
@page "/"
@inject DashboardDataService DataService
@inject NavigationManager Navigation
@implements IDisposable

@if (_error is not null)
{
    <div class="error-panel">@_error</div>
}
else if (_data is not null)
{
    <DashboardHeader Data="@_data" />
    <Timeline Milestones="@_data.Milestones"
              TimelineStart="@_data.TimelineStart"
              TimelineEnd="@_data.TimelineEnd"
              CurrentDate="@_data.CurrentDate" />
    <Heatmap Categories="@_data.Categories"
             Months="@_data.Months"
             CurrentMonthIndex="@_data.CurrentMonthIndex" />
}

@code {
    private DashboardData? _data;
    private string? _error;

    protected override void OnInitialized()
    {
        var uri = new Uri(Navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var project = query["project"];

        try
        {
            _data = DataService.GetData(project);
            DataService.OnDataChanged += HandleDataChanged;
        }
        catch (DashboardDataException ex)
        {
            _error = ex.Message;
        }
    }

    private async void HandleDataChanged()
    {
        var uri = new Uri(Navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var project = query["project"];
        _data = DataService.GetData(project);
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose() => DataService.OnDataChanged -= HandleDataChanged;
}
```

---

### 5. `DashboardHeader.razor` — Header Component

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render project title, ADO backlog link, subtitle, and legend markers |
| **Interfaces** | `[Parameter] DashboardData Data` |
| **Dependencies** | None |
| **Data** | `Data.Title`, `Data.Subtitle`, `Data.BacklogUrl` |

**Renders:** A flexbox row with left side (title + subtitle) and right side (4 legend items). Maps directly to the `.hdr` section of the reference design.

---

### 6. `Timeline.razor` — SVG Timeline Component

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render the milestone label sidebar and the SVG timeline with tracks, markers, gridlines, and NOW line |
| **Interfaces** | `[Parameter] List<Milestone> Milestones`, `[Parameter] DateTime TimelineStart`, `[Parameter] DateTime TimelineEnd`, `[Parameter] DateTime CurrentDate` |
| **Dependencies** | None |
| **Data** | Milestone tracks, markers, date range |

**Key behaviors:**
- Renders a 230px-wide sidebar with milestone labels (ID in color, description in gray).
- Generates an inline `<svg>` element (width=1560, height=185) containing:
  - Month gridlines as vertical `<line>` elements at calculated X positions.
  - Month labels as `<text>` elements.
  - Milestone track horizontal `<line>` elements with Y positions evenly distributed: `Y = 28 + (index * (185 - 28) / (trackCount))` (leaving room for month labels at top).
  - Marker elements dispatched by type: `checkpoint` → `<circle>`, `poc` → `<polygon>` diamond with gold fill, `production` → `<polygon>` diamond with green fill, `smallCheckpoint` → `<circle>` with gray fill.
  - Drop shadow SVG filter in `<defs>`.
  - NOW dashed vertical line at the X position corresponding to `CurrentDate`.
  - Text labels adjacent to each marker.

**SVG coordinate system:**
- X-axis: Linear interpolation from `TimelineStart` to `TimelineEnd` across 1560px.
- Y-axis: Tracks evenly spaced within 185px, offset 28px from top for month labels.
- Month gridlines: Positioned at the 1st of each month within the timeline range.

```csharp
@code {
    private const double SvgWidth = 1560;
    private const double SvgHeight = 185;
    private const double TopMargin = 28;

    private double GetXPosition(DateTime date)
    {
        var totalDays = (TimelineEnd - TimelineStart).TotalDays;
        var elapsed = (date - TimelineStart).TotalDays;
        return Math.Clamp((elapsed / totalDays) * SvgWidth, 0, SvgWidth);
    }

    private double GetTrackY(int trackIndex)
    {
        var count = Milestones.Count;
        if (count == 1) return SvgHeight / 2.0;
        var usableHeight = SvgHeight - TopMargin - 16;
        return TopMargin + 14 + (trackIndex * usableHeight / (count - 1));
    }

    private string DiamondPoints(double cx, double cy, double r = 11)
    {
        return $"{cx},{cy - r} {cx + r},{cy} {cx},{cy + r} {cx - r},{cy}";
    }
}
```

---

### 7. `Heatmap.razor` — Grid Container Component

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render the heatmap title bar and CSS Grid container with column headers, row headers, and data cells |
| **Interfaces** | `[Parameter] List<HeatmapCategory> Categories`, `[Parameter] List<string> Months`, `[Parameter] int CurrentMonthIndex` |
| **Dependencies** | `HeatmapCell` child component |
| **Data** | 4 categories × N months = 4N data cells + 4 row headers + N column headers + 1 corner cell |

**Key behaviors:**
- Renders CSS Grid with `grid-template-columns: 160px repeat({Months.Count}, 1fr)` and `grid-template-rows: 36px repeat(4, 1fr)`.
- Corner cell renders "STATUS" in uppercase.
- Column headers render month names; the column at `CurrentMonthIndex` gets the highlighted `.current-month-hdr` class (gold background, amber text, appends " ▸ Now").
- Row headers render category names with category-specific CSS classes for colors.
- Data cells delegate to `HeatmapCell` component, passing the items array and category key.

**Category-to-CSS-class mapping:**

| Category Key | Row Header Class | Cell Class | Current Month Cell Class |
|-------------|-----------------|------------|------------------------|
| `shipped` | `ship-hdr` | `ship-cell` | `ship-cell current` |
| `inProgress` | `prog-hdr` | `prog-cell` | `prog-cell current` |
| `carryover` | `carry-hdr` | `carry-cell` | `carry-cell current` |
| `blockers` | `block-hdr` | `block-cell` | `block-cell current` |

---

### 8. `HeatmapCell.razor` — Cell Renderer Component

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render a single heatmap data cell with bulleted item list or empty-state dash |
| **Interfaces** | `[Parameter] List<string> Items`, `[Parameter] string CategoryKey`, `[Parameter] bool IsCurrentMonth` |
| **Dependencies** | None |
| **Data** | 0-8 string items |

**Key behaviors:**
- If `Items` is null or empty, renders a single `<div class="empty">—</div>` in muted gray (#AAA).
- Otherwise, renders each item as `<div class="it">ItemText</div>` where the `::before` pseudo-element provides the colored bullet dot.
- The bullet color is determined by the parent cell's CSS class (inherited from the category row), not set inline.

---

### 9. `app.css` — Global Stylesheet & Design Tokens

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Define CSS custom properties (design tokens), global resets, body dimensions, and shared utility styles |
| **Interfaces** | Available to all components via CSS custom property inheritance |
| **Dependencies** | None |
| **Data** | 24 CSS custom properties defining the full color palette |

---

### 10. Error Panel Component (inline in `Dashboard.razor`)

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Display a user-friendly error message when `data.json` is missing, malformed, or invalid |
| **Interfaces** | Receives error string from `Dashboard.razor` catch block |
| **Dependencies** | None |
| **Data** | Error message string |

**Renders:** A centered panel at 1920×1080 with the error message, file path searched, and guidance to check the JSON file. Prevents blank-page scenarios.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│  File System                                                     │
│  ┌──────────────────┐                                           │
│  │  data.json        │◄── PM edits in text editor               │
│  │  data.atlas.json  │                                           │
│  │  data.phoenix.json│                                           │
│  └────────┬─────────┘                                           │
│           │ FileSystemWatcher (on change)                        │
│           ▼                                                      │
│  ┌──────────────────────────┐                                   │
│  │  DashboardDataService    │ (Singleton)                       │
│  │  ┌────────────────────┐  │                                   │
│  │  │ ConcurrentDictionary│  │                                   │
│  │  │ <project, Data>    │  │                                   │
│  │  └────────────────────┘  │                                   │
│  │  + GetData(project?)     │──── raises OnDataChanged event    │
│  │  + Validate(data)        │                                   │
│  └────────┬─────────────────┘                                   │
│           │ DI injection                                         │
│           ▼                                                      │
│  ┌──────────────────────────┐                                   │
│  │  Dashboard.razor (Page)  │ Route: /  Query: ?project=X       │
│  │  + OnInitialized()       │──── calls GetData(project)        │
│  │  + HandleDataChanged()   │──── calls StateHasChanged()       │
│  └────┬────────┬────────┬───┘                                   │
│       │        │        │     [Parameter] passing                │
│       ▼        ▼        ▼                                        │
│  ┌────────┐ ┌──────┐ ┌───────┐                                  │
│  │Header  │ │Time- │ │Heat-  │                                   │
│  │.razor  │ │line  │ │map    │                                   │
│  │        │ │.razor│ │.razor │                                   │
│  └────────┘ └──────┘ └───┬───┘                                  │
│                           │ [Parameter] passing                  │
│                           ▼                                      │
│                     ┌───────────┐                                │
│                     │HeatmapCell│ × (4 categories × N months)   │
│                     │.razor     │                                │
│                     └───────────┘                                │
│                                                                  │
│  ════════════════════════════════════════════════════════════    │
│  Browser (Chromium) ◄──── SignalR circuit ────► Kestrel:5001    │
│  Renders 1920×1080 HTML ──► Screenshot to PowerPoint            │
└─────────────────────────────────────────────────────────────────┘
```

### Communication Patterns

| From | To | Mechanism | Trigger |
|------|----|-----------|---------|
| `data.json` (file) | `DashboardDataService` | `FileSystemWatcher` callback | File save |
| `DashboardDataService` | `Dashboard.razor` | C# event `OnDataChanged` → `StateHasChanged()` | File reload |
| `Dashboard.razor` | Child components | `[Parameter]` properties | Initial render / re-render |
| Browser | Kestrel | SignalR WebSocket | Page navigation, `StateHasChanged` push |
| User | Browser | URL navigation (`/?project=X`) | Manual URL entry |

### Render Pipeline (Single Pass)

1. Browser requests `/` or `/?project=X`.
2. Blazor Server creates circuit, instantiates `Dashboard.razor`.
3. `OnInitialized()` reads query string, calls `DataService.GetData(project)`.
4. `Dashboard.razor` sets `[Parameter]` values on child components.
5. `DashboardHeader`, `Timeline`, `Heatmap` each render synchronously — no async loading.
6. Full HTML is pushed to browser in a single SignalR message.
7. Browser paints the 1920×1080 layout. Done.

**There are no callbacks from child to parent.** This is a pure top-down render tree with no bidirectional communication.

---

## Data Model

### Entity Relationship Diagram

```
DashboardData (root)
├── title: string
├── subtitle: string
├── backlogUrl: string
├── currentDate: DateTime
├── months: string[]
├── currentMonthIndex: int
├── timelineStart: DateTime
├── timelineEnd: DateTime
├── milestones: Milestone[]          ──── 1:N
│   ├── id: string
│   ├── label: string
│   ├── description: string
│   ├── color: string (hex)
│   └── markers: MilestoneMarker[]   ──── 1:N
│       ├── date: DateTime
│       ├── type: string (enum)
│       └── label: string
└── categories: HeatmapCategory[]    ──── 1:4 (fixed)
    ├── name: string
    ├── key: string (enum)
    └── items: Dictionary<string, string[]>  ──── month → items
```

### C# Model Definitions

```csharp
// Models/DashboardData.cs
public class DashboardData
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;

    [JsonPropertyName("backlogUrl")]
    public string BacklogUrl { get; set; } = string.Empty;

    [JsonPropertyName("currentDate")]
    public DateTime CurrentDate { get; set; }

    [JsonPropertyName("months")]
    public List<string> Months { get; set; } = new();

    [JsonPropertyName("currentMonthIndex")]
    public int CurrentMonthIndex { get; set; }

    [JsonPropertyName("timelineStart")]
    public DateTime TimelineStart { get; set; }

    [JsonPropertyName("timelineEnd")]
    public DateTime TimelineEnd { get; set; }

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("categories")]
    public List<HeatmapCategory> Categories { get; set; } = new();
}

// Models/Milestone.cs
public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("markers")]
    public List<MilestoneMarker> Markers { get; set; } = new();
}

// Models/MilestoneMarker.cs
public class MilestoneMarker
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "checkpoint", "poc", "production", "smallCheckpoint"

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}

// Models/HeatmapCategory.cs
public class HeatmapCategory
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty; // "shipped", "inProgress", "carryover", "blockers"

    [JsonPropertyName("items")]
    public Dictionary<string, List<string>> Items { get; set; } = new();
}
```

### Storage

- **Format:** JSON flat file (`data.json`)
- **Location:** Application content root directory (same directory as the `.csproj`)
- **Access pattern:** Read-only at runtime; written by humans in a text editor
- **Size constraint:** Up to 500 KB (sufficient for hundreds of items)
- **No database.** The data model is too simple and the access pattern too narrow to justify any persistence layer beyond a file.

### Validation Rules

| Field | Rule | Error Message |
|-------|------|---------------|
| `title` | Required, non-empty | `"'title' is required in {filepath}"` |
| `months` | Required, 1-12 entries | `"'months' must contain 1-12 entries"` |
| `currentMonthIndex` | Must be valid index into `months` | `"'currentMonthIndex' ({value}) is out of range for {months.Count} months"` |
| `timelineStart` | Must be before `timelineEnd` | `"'timelineStart' must be before 'timelineEnd'"` |
| `milestones` | Required, 1-5 entries | `"'milestones' must contain 1-5 entries"` |
| `milestones[].color` | Must be valid hex color | `"Milestone '{id}' has invalid color '{color}'"` |
| `milestones[].markers[].type` | Must be one of: checkpoint, poc, production, smallCheckpoint | `"Marker type '{type}' is not recognized"` |
| `categories` | Required, exactly 4 entries | `"'categories' must contain exactly 4 entries"` |
| `categories[].key` | Must be one of: shipped, inProgress, carryover, blockers | `"Category key '{key}' is not recognized"` |
| `categories[].items` keys | Each key should exist in `months` | Warning logged, not a fatal error |

---

## API Contracts

This application has **no REST API, no GraphQL, and no RPC endpoints**. It is a server-rendered Blazor application that communicates with the browser exclusively via SignalR (managed by the Blazor framework — no custom hub code).

### URL Routing Contract

| URL | Behavior | Response |
|-----|----------|----------|
| `GET /` | Load default `data.json`, render dashboard | Full HTML page (1920×1080) |
| `GET /?project={name}` | Load `data.{name}.json`, render dashboard | Full HTML page (1920×1080) |
| `GET /?project={name}` (file not found) | Display error panel | HTML page with error message |
| `GET /_blazor` | SignalR negotiation (framework-managed) | WebSocket upgrade |
| `GET /css/app.css` | Static file serving | CSS stylesheet |
| `GET /_framework/*` | Blazor framework assets | JS/CSS |

### Error Response Contract

Errors are rendered as HTML within the 1920×1080 page body, not as HTTP error codes. The SignalR circuit always returns 200 OK for the initial page load.

| Error Condition | Displayed Message | Console Log |
|----------------|-------------------|-------------|
| `data.json` missing | "Dashboard data file not found: {path}" | `error: DashboardDataService — File not found: {path}` |
| JSON parse failure | "Failed to parse dashboard data: {error detail}" | `error: DashboardDataService — JSON parse error: {exception}` |
| Missing required field | "Invalid dashboard data: {field} is required" | `error: DashboardDataService — Validation: {detail}` |
| Project file not found | "Project '{name}' not found. Checked: {paths}" | `warn: DashboardDataService — Project file not found: {name}` |

### Multi-Project File Resolution Contract

Given `?project=phoenix`, the service resolves files in this order:

1. `{contentRoot}/data.phoenix.json` — **primary** (recommended convention)
2. `{contentRoot}/projects/phoenix.json` — **secondary** (subfolder convention)
3. If neither exists → render error panel

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|---------------|
| **Runtime** | .NET 8 SDK (8.0.x latest patch) |
| **OS** | Windows 10/11 (primary); macOS/Linux compatible but untested |
| **Browser** | Any Chromium-based (Chrome 120+, Edge 120+) |
| **Network** | None — localhost only, no outbound calls |
| **Disk** | < 50 MB for application + SDK (SDK assumed pre-installed) |
| **Memory** | < 100 MB RSS at runtime |
| **CPU** | Minimal — single-threaded render, no background computation |

### Hosting

| Aspect | Configuration |
|--------|---------------|
| **Web server** | Kestrel (built into .NET 8, no IIS required) |
| **Binding** | `https://localhost:5001` and `http://localhost:5000` |
| **TLS** | .NET dev certificate (auto-generated by `dotnet dev-certs https --trust`) |
| **Process model** | Single process, in-process hosting |
| **Launch command** | `dotnet run` or `dotnet watch` (for hot reload during development) |

### Kestrel Configuration (`appsettings.json`)

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001"
      }
    }
  },
  "Dashboard": {
    "DataFilePath": "data.json"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Storage

| Type | Location | Purpose |
|------|----------|---------|
| `data.json` | Content root (`src/ReportingDashboard/`) | Dashboard data |
| `data.{project}.json` | Content root | Per-project data files |
| `projects/{name}.json` | Content root subfolder | Alternative multi-project convention |
| `appsettings.json` | Content root | App configuration |
| `wwwroot/css/app.css` | Static files | Global CSS |

### CI/CD

**None.** This is a local developer tool. The "deployment" is `dotnet run`. No build pipeline, no artifact publishing, no container registry, no infrastructure-as-code.

### Solution Structure on Disk

```
ReportingDashboard.sln
│
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── data.json                         # Default project data
│       ├── data.phoenix.json                 # Example: multi-project
│       │
│       ├── Models/
│       │   ├── DashboardData.cs
│       │   ├── Milestone.cs
│       │   ├── MilestoneMarker.cs
│       │   └── HeatmapCategory.cs
│       │
│       ├── Services/
│       │   ├── DashboardDataService.cs
│       │   └── DashboardDataException.cs
│       │
│       ├── Components/
│       │   ├── _Imports.razor
│       │   ├── App.razor
│       │   ├── Routes.razor
│       │   ├── Pages/
│       │   │   ├── Dashboard.razor
│       │   │   └── Dashboard.razor.css
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor
│       │   │   └── MainLayout.razor.css
│       │   ├── DashboardHeader.razor
│       │   ├── DashboardHeader.razor.css
│       │   ├── Timeline.razor
│       │   ├── Timeline.razor.css
│       │   ├── Heatmap.razor
│       │   ├── Heatmap.razor.css
│       │   ├── HeatmapCell.razor
│       │   └── HeatmapCell.razor.css
│       │
│       └── wwwroot/
│           └── css/
│               └── app.css
│
├── tests/
│   └── ReportingDashboard.Tests/
│       ├── ReportingDashboard.Tests.csproj
│       ├── DataModelDeserializationTests.cs
│       ├── DataValidationTests.cs
│       └── TestData/
│           ├── valid-minimal.json
│           ├── valid-full.json
│           ├── invalid-missing-title.json
│           └── invalid-bad-milestone.json
│
└── docs/
    └── design-screenshots/
        └── OriginalDesignConcept.png
```

### `.csproj` Files

**`src/ReportingDashboard/ReportingDashboard.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

Zero external NuGet packages. The `Microsoft.NET.Sdk.Web` SDK includes everything needed: Blazor Server, `System.Text.Json`, Kestrel, static file middleware, and CSS isolation.

**`tests/ReportingDashboard.Tests/ReportingDashboard.Tests.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.*" />
    <PackageReference Include="xunit" Version="2.7.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\ReportingDashboard\ReportingDashboard.csproj" />
  </ItemGroup>
</Project>
```

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **Framework** | .NET 8 Blazor Server | Mandated by project constraints. Team expertise exists. Server-side rendering avoids WASM download overhead and renders the full page in one pass. |
| **Render mode** | Interactive Server (with Static SSR option) | Enables SignalR-based live push when `data.json` changes via `FileSystemWatcher`. If hot reload is not needed, Static SSR eliminates the SignalR circuit entirely for pure screenshot use. |
| **CSS architecture** | Blazor CSS Isolation (`.razor.css`) + CSS Custom Properties in `app.css` | Maps 1:1 with the component-per-section design. Scoped styles prevent conflicts. Custom properties centralize the 24-color design token palette. |
| **SVG rendering** | Inline SVG in Razor markup | The timeline has ~50 SVG elements (lines, circles, polygons, text). A charting library would impose its own styling model and fight the reference design. Server-side Razor generation gives pixel-level control. |
| **JSON serializer** | `System.Text.Json` (built-in) | Ships with .NET 8. Source-generated serialization is available if performance tuning is ever needed (it won't be at this scale). Zero external dependency. |
| **File watching** | `FileSystemWatcher` | Built into .NET. Lightweight, event-driven. Debounce with 300ms delay to handle rapid saves. No polling overhead. |
| **Data storage** | Flat JSON file | The data is small (<500KB), read-only at runtime, and human-edited. A database would add setup steps, migration concerns, and zero value. |
| **Testing** | xUnit | Industry standard for .NET. Included in SDK templates. Minimal configuration. |
| **UI libraries** | None (zero external) | The reference design is pure HTML/CSS/SVG. MudBlazor or Radzen would add ~2MB of dependencies, impose their own design language, and make pixel-matching harder. |
| **Charting libraries** | None | The SVG timeline is 50 lines of markup. ApexCharts.Blazor would add a JS interop dependency and fight the custom diamond/circle marker design. |
| **Web server** | Kestrel (built-in) | No IIS, no Nginx, no reverse proxy. Kestrel binds to localhost and serves the single page. |
| **Font** | Segoe UI (system font) | Pre-installed on Windows. No web font download = no FOUT, no network dependency, no licensing concern. |

### Technology Stack Summary

```
┌─────────────────────────────────────────────────┐
│              Presentation Layer                   │
│  Blazor Server Components (.razor + .razor.css)  │
│  Inline SVG (Timeline)                           │
│  CSS Grid + Flexbox (Heatmap + Layout)           │
│  CSS Custom Properties (Design Tokens)           │
├─────────────────────────────────────────────────┤
│              Application Layer                    │
│  DashboardDataService (Singleton, DI-registered) │
│  System.Text.Json (Deserialization)              │
│  FileSystemWatcher (Hot Reload)                  │
├─────────────────────────────────────────────────┤
│              Data Layer                           │
│  data.json (flat file, human-edited)             │
│  data.{project}.json (multi-project)             │
├─────────────────────────────────────────────────┤
│              Infrastructure                       │
│  Kestrel (localhost:5001)                        │
│  .NET 8 SDK                                      │
│  No containers, no cloud, no CI/CD              │
└─────────────────────────────────────────────────┘
```

---

## UI Component Architecture

This section maps each visual section from the `OriginalDesignConcept.html` reference design to a specific Blazor component, its CSS layout strategy, data bindings, and interactions.

### Component-to-Design Mapping

| Visual Section | Component File | CSS Layout | Data Bindings | Interactions |
|---------------|----------------|------------|---------------|--------------|
| **Page body** (1920×1080 flex column) | `MainLayout.razor` + `MainLayout.razor.css` | `display: flex; flex-direction: column; width: 1920px; height: 1080px; overflow: hidden` | None — pure shell | None |
| **Header bar** (`.hdr`) | `DashboardHeader.razor` + `DashboardHeader.razor.css` | Flexbox row, `justify-content: space-between`, padding `12px 44px 10px`, border-bottom 1px `#E0E0E0` | `Data.Title`, `Data.BacklogUrl`, `Data.Subtitle` | ADO Backlog link (`<a>` tag, navigates to external URL) |
| **Header left** (title + subtitle) | Inside `DashboardHeader.razor` | Block layout, `<h1>` at 24px/700 + `<div class="sub">` at 12px/#888 | `Data.Title` → h1 text, `Data.BacklogUrl` → href, `Data.Subtitle` → subtitle div | None |
| **Header right** (legend) | Inside `DashboardHeader.razor` | Flexbox row, gap 22px, font-size 12px | Static legend items (4 marker types), label text hardcoded to design spec | None |
| **Legend: PoC diamond** | Inline span in legend | 12×12px square, `transform: rotate(45deg)`, background `#F4B400` | None — static decorative element | None |
| **Legend: Production diamond** | Inline span in legend | 12×12px square, `transform: rotate(45deg)`, background `#34A853` | None — static | None |
| **Legend: Checkpoint circle** | Inline span in legend | 8×8px circle, border-radius 50%, background `#999` | None — static | None |
| **Legend: Now bar** | Inline span in legend | 2×14px rectangle, background `#EA4335` | None — static | None |
| **Timeline area** (`.tl-area`) | `Timeline.razor` + `Timeline.razor.css` | Flexbox row, height 196px, background `#FAFAFA`, padding `6px 44px 0`, border-bottom 2px `#E8E8E8` | `Milestones`, `TimelineStart`, `TimelineEnd`, `CurrentDate` | None |
| **Milestone sidebar** (left 230px) | Inside `Timeline.razor` | Flexbox column, width 230px, `justify-content: space-around`, padding `16px 12px 16px 0`, border-right 1px `#E0E0E0` | `Milestones[].Id` → bold colored ID, `Milestones[].Description` → gray description, `Milestones[].Color` → text color | None |
| **SVG timeline** (`.tl-svg-box`) | Inside `Timeline.razor` | `flex: 1`, padding-left 12px, `<svg width="1560" height="185" overflow="visible">` | All milestone data drives SVG element generation | None |
| **Month gridlines** | SVG `<line>` elements in `Timeline.razor` | Vertical lines, stroke `#bbb`, opacity 0.4, width 1, positioned at 1st of each month | Derived from `TimelineStart`/`TimelineEnd` date range | None |
| **Month labels** | SVG `<text>` elements in `Timeline.razor` | Font-size 11, font-weight 600, fill `#666`, y=14 | Month names derived from date range | None |
| **Milestone tracks** | SVG `<line>` elements in `Timeline.razor` | Horizontal, stroke-width 3, stroke = `Milestone.Color`, y = calculated per track | `Milestones[].Color` → stroke color, track index → Y position | None |
| **Checkpoint markers** | SVG `<circle>` in `Timeline.razor` | r=5-7, fill white, stroke = milestone color, stroke-width 2.5 | `Marker.Date` → X position via `GetXPosition()` | None |
| **PoC diamond markers** | SVG `<polygon>` in `Timeline.razor` | Diamond shape (4-point polygon), fill `#F4B400`, filter `url(#sh)` drop shadow | `Marker.Date` → X position, `Marker.Label` → adjacent text | None |
| **Production diamond markers** | SVG `<polygon>` in `Timeline.razor` | Diamond shape, fill `#34A853`, filter `url(#sh)` drop shadow | `Marker.Date` → X position, `Marker.Label` → adjacent text | None |
| **Small checkpoint dots** | SVG `<circle>` in `Timeline.razor` | r=4, fill `#999`, no stroke | `Marker.Date` → X position | None |
| **NOW line** | SVG `<line>` + `<text>` in `Timeline.razor` | Vertical dashed line, stroke `#EA4335`, width 2, dasharray "5,3"; label "NOW" in 10px bold red | `CurrentDate` → X position via `GetXPosition()` | None |
| **Marker labels** | SVG `<text>` in `Timeline.razor` | Font-size 10, fill `#666`, text-anchor middle, positioned above/below marker | `Marker.Label` → text content | None |
| **Heatmap wrapper** (`.hm-wrap`) | `Heatmap.razor` + `Heatmap.razor.css` | `flex: 1; min-height: 0; display: flex; flex-direction: column; padding: 10px 44px 10px` | `Categories`, `Months`, `CurrentMonthIndex` | None |
| **Heatmap title** (`.hm-title`) | Inside `Heatmap.razor` | 14px, weight 700, color `#888`, uppercase, letter-spacing 0.5px, margin-bottom 8px | Static text: "MONTHLY EXECUTION HEATMAP..." | None |
| **Heatmap grid** (`.hm-grid`) | Inside `Heatmap.razor` | CSS Grid: `grid-template-columns: 160px repeat(N, 1fr)`, `grid-template-rows: 36px repeat(4, 1fr)`, border 1px `#E0E0E0` | `Months.Count` → column count | None |
| **Corner cell** (`.hm-corner`) | Inside `Heatmap.razor` | Background `#F5F5F5`, 11px bold uppercase `#999`, centered, border-bottom 2px `#CCC` | Static text: "STATUS" | None |
| **Column headers** (`.hm-col-hdr`) | Inside `Heatmap.razor` (loop over `Months`) | 16px bold, background `#F5F5F5`, centered, border-bottom 2px `#CCC`; current month: background `#FFF0D0`, color `#C07700` | `Months[i]` → display text, `CurrentMonthIndex` → highlight class | None |
| **Row headers** (`.hm-row-hdr`) | Inside `Heatmap.razor` (loop over `Categories`) | 11px bold uppercase, letter-spacing 0.7px, padding `0 12px`, border-right 2px `#CCC` | `Category.Name` → display text, `Category.Key` → CSS class for colors | None |
| **Data cells** | `HeatmapCell.razor` + `HeatmapCell.razor.css` | Padding `8px 12px`, border-right 1px `#E0E0E0`, border-bottom 1px `#E0E0E0`, overflow hidden | `Category.Items[month]` → item strings, `Category.Key` → bullet color class, `isCurrentMonth` → darker BG class | None |
| **Item bullets** (`.it::before`) | CSS pseudo-element in `HeatmapCell.razor.css` | 6×6px circle, position absolute, left 0, top 7px, border-radius 50% | Color set by parent cell's category class | None |
| **Empty cell dash** | Inside `HeatmapCell.razor` | Color `#AAA`, centered dash character "—" | Rendered when `Items` is empty | None |
| **Error panel** | Inside `Dashboard.razor` | Centered in 1920×1080, max-width 600px, padding 40px, border 1px `#EA4335`, background `#FFF5F5` | Error message string from catch block | None |

### CSS Custom Properties (Design Token System)

Defined in `wwwroot/css/app.css` and consumed by all component `.razor.css` files:

```css
:root {
    /* Category: Shipped (green) */
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-shipped-header-bg: #E8F5E9;
    --color-shipped-header-text: #1B7A28;

    /* Category: In Progress (blue) */
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-progress-header-bg: #E3F2FD;
    --color-progress-header-text: #1565C0;

    /* Category: Carryover (amber) */
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-carryover-header-bg: #FFF8E1;
    --color-carryover-header-text: #B45309;

    /* Category: Blockers (red) */
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-current: #FFE4E4;
    --color-blockers-header-bg: #FEF2F2;
    --color-blockers-header-text: #991B1B;

    /* Shared */
    --color-border: #E0E0E0;
    --color-border-heavy: #CCC;
    --color-text: #111;
    --color-text-muted: #888;
    --color-text-secondary: #444;
    --color-text-item: #333;
    --color-link: #0078D4;
    --color-bg-header: #F5F5F5;
    --color-bg-timeline: #FAFAFA;
    --color-current-month-bg: #FFF0D0;
    --color-current-month-text: #C07700;
    --font-family: 'Segoe UI', Arial, sans-serif;
}

/* Global reset matching reference design */
* { margin: 0; padding: 0; box-sizing: border-box; }
body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
    background: #FFFFFF;
    font-family: var(--font-family);
    color: var(--color-text);
    display: flex;
    flex-direction: column;
}
a { color: var(--color-link); text-decoration: none; }
```

### Component Hierarchy (Render Tree)

```
App.razor
└── Routes.razor
    └── MainLayout.razor                    [Layout shell: 1920×1080 flex column]
        └── Dashboard.razor                 [Page: route "/", orchestrator]
            ├── DashboardHeader.razor       [Header: title + legend, ~50px]
            │   ├── <h1> title + <a> backlog link
            │   ├── <div class="sub"> subtitle
            │   └── <div> legend (4 marker items)
            │
            ├── Timeline.razor              [Timeline: 196px fixed height]
            │   ├── <div> milestone sidebar (230px)
            │   │   └── @foreach milestone → label div
            │   └── <svg width="1560" height="185">
            │       ├── <defs> drop shadow filter
            │       ├── @foreach month → gridline + label
            │       ├── @foreach milestone → track line
            │       │   └── @foreach marker → circle/polygon + label
            │       └── NOW line + label
            │
            └── Heatmap.razor               [Heatmap: flex-1, fills remaining]
                ├── <div class="hm-title"> section title
                └── <div class="hm-grid">   [CSS Grid]
                    ├── corner cell ("STATUS")
                    ├── @foreach month → column header
                    └── @foreach category →
                        ├── row header
                        └── @foreach month →
                            └── HeatmapCell.razor
                                ├── @if items.Any() →
                                │   @foreach item → <div class="it">
                                └── @else → <div class="empty">—</div>
```

---

## Security Considerations

### Threat Model

This application has the smallest possible attack surface: a localhost-only web server rendering read-only data from a local file.

| Threat | Applicability | Mitigation |
|--------|--------------|------------|
| **Unauthorized access** | N/A | Kestrel binds to `localhost` only. No remote access possible without OS-level port forwarding. |
| **Data exfiltration** | N/A | No PII, no secrets, no credentials in `data.json`. Contains only project status text. |
| **Cross-site scripting (XSS)** | Low | Blazor Server automatically HTML-encodes all rendered content. The `@` syntax in Razor prevents raw HTML injection. The only unencoded output is the `backlogUrl` href attribute — validated as a URL format. |
| **Cross-site request forgery (CSRF)** | N/A | No state-mutating endpoints. The application is read-only. Blazor's built-in antiforgery middleware is included but has nothing to protect. |
| **Denial of service** | N/A | Single user on localhost. No external traffic. |
| **Supply chain attack** | Minimal | Zero external NuGet packages in production. Only the .NET 8 SDK (Microsoft-signed) is required. Test project uses xUnit (well-audited, widely used). |
| **Sensitive data in source control** | Low | If `data.json` contains sensitive project names, add it to `.gitignore`. Provide a `data.sample.json` template instead. |
| **JSON injection / malformed input** | Low | `System.Text.Json` with strongly-typed POCOs rejects unexpected types. Validation layer catches structural issues. No dynamic code execution from JSON values. |
| **SignalR circuit abuse** | N/A | Localhost-only. No external clients can connect. Default circuit limits (100KB message size, 1 concurrent circuit) are more than sufficient. |

### Authentication & Authorization

**None implemented. None required.**

The application runs on `localhost`, is used by one person at a time, and contains no sensitive data. Adding authentication would violate the "zero operational overhead" business goal.

**If future network sharing is needed:** Add a single middleware that checks for a `?key=` query parameter against a value in `appsettings.json`. This is ~10 lines of code and provides shared-secret access control without a full auth system.

### Input Validation

| Input Source | Validation |
|-------------|------------|
| `data.json` content | Strongly-typed deserialization rejects type mismatches. Custom validation checks required fields, value ranges, and enum values. |
| `?project=` query parameter | Sanitized to alphanumeric + hyphen + underscore only. Path traversal characters (`..`, `/`, `\`) are stripped. File resolution uses `Path.Combine` with the content root — no user-controlled path components reach the file system directly. |
| `backlogUrl` from JSON | Rendered in an `<a href="">` tag. Blazor's attribute encoding prevents attribute injection. No `javascript:` protocol URLs are permitted (validated to start with `http://` or `https://`). |

### Sensitive Data Handling

```gitignore
# .gitignore
data.json
data.*.json
projects/*.json
```

Provide a `data.sample.json` with fictional data for onboarding. Developers copy it to `data.json` and populate with real project data.

---

## Scaling Strategy

### Current Scale

This application is designed for **exactly one concurrent user** on **localhost**. It is not a service. Scaling in the traditional sense (horizontal pods, load balancers, database sharding) does not apply.

| Dimension | Current Capacity | Limit |
|-----------|-----------------|-------|
| Concurrent users | 1 | 1 (by design — single localhost session) |
| Projects | Unlimited (one `data.{name}.json` per project) | Bounded by disk space |
| Months in heatmap | 4-6 (data-driven) | 12 (visual constraint at 1920px width) |
| Milestone tracks | 1-5 (data-driven) | 5 (visual constraint at 196px height) |
| Items per heatmap cell | 0-8 (data-driven) | ~8 (visual constraint at available cell height) |
| JSON file size | < 500 KB | 500 KB (in-memory parse, single-threaded) |

### Data Volume Scaling

The `data.json` file grows linearly with: `months × categories × items_per_cell + milestones × markers`. A fully populated dashboard (6 months, 4 categories, 8 items per cell, 5 milestones with 10 markers each) produces a JSON file of approximately 15 KB. The 500 KB limit allows for roughly 30× this density — far beyond what's visually useful.

### Multi-Project Scaling

Each project is a separate JSON file. The application loads one file per page request and caches it in memory. With 100 project files at 15 KB each, total cache size is ~1.5 MB — negligible.

### If Network Sharing Were Ever Needed (Out of Scope)

If the dashboard needed to serve multiple users on a network:

1. Change Kestrel binding from `localhost` to `0.0.0.0`.
2. Add shared-secret auth middleware.
3. Blazor Server supports ~5,000 concurrent circuits per server (Microsoft benchmarks). A single instance would handle any reasonable executive audience.
4. No database, no session store, no distributed cache needed — the application is stateless (all state comes from the JSON file).

This path is documented but explicitly **not implemented** in v1.

---

## Risks & Mitigations

### Risk 1: SVG Positioning Drift

| Attribute | Value |
|-----------|-------|
| **Severity** | Medium |
| **Likelihood** | Medium |
| **Impact** | Milestone markers appear at wrong dates; screenshot doesn't match reference |

**Description:** The timeline SVG positions markers via `GetXPosition()` which maps dates to pixel coordinates. If the SVG `width` attribute, the container CSS width, or the date-to-pixel formula are miscalculated, markers drift from their intended positions.

**Mitigation:**
- Hard-code `<svg width="1560">` to match the reference design exactly (1920px viewport − 44px left padding − 44px right padding − 230px sidebar − 12px gap − border adjustments ≈ 1560px usable).
- Use the formula `x = ((date - timelineStart).TotalDays / (timelineEnd - timelineStart).TotalDays) * 1560` — the same linear interpolation used in the reference.
- **Verification:** Overlay the rendered SVG on the reference design in Chrome DevTools. Month gridlines must align within ±2px.
- Write a unit test that asserts `GetXPosition(timelineStart) == 0` and `GetXPosition(timelineEnd) == 1560`.

---

### Risk 2: CSS Isolation Leakage

| Attribute | Value |
|-----------|-------|
| **Severity** | Low |
| **Likelihood** | Medium |
| **Impact** | Styles from one component bleed into another; visual artifacts |

**Description:** Blazor CSS isolation scopes styles by appending a unique attribute (e.g., `b-abc123`) to the component's root elements. However, child components rendered via `<HeatmapCell>` do not inherit the parent's scope attribute. Styles defined in `Heatmap.razor.css` using `.hm-cell .it` will not match elements inside `HeatmapCell.razor`.

**Mitigation:**
- Use `::deep` combinator in parent `.razor.css` files when targeting child component elements: `::deep .it { ... }`.
- Alternatively, define item-level styles in `HeatmapCell.razor.css` (the component that owns the elements), not in the parent.
- Keep the component hierarchy shallow — only 2 levels deep (Heatmap → HeatmapCell). No deeper nesting.
- **Verification:** Inspect the rendered HTML in Chrome DevTools. Verify that every styled element has the expected `b-` attribute.

---

### Risk 3: Blazor Server SignalR Overhead

| Attribute | Value |
|-----------|-------|
| **Severity** | Low |
| **Likelihood** | Low |
| **Impact** | Slightly longer initial page load due to WebSocket negotiation; reconnection prompts if circuit drops |

**Description:** Blazor Server establishes a persistent SignalR WebSocket connection for each browser tab. For a read-only screenshot tool, this is unnecessary overhead. If the connection drops (e.g., laptop sleep/wake), the browser shows a "Reconnecting..." overlay that would ruin a screenshot.

**Mitigation:**
- For screenshot capture: Refresh the page to establish a fresh circuit before screenshotting.
- Consider using .NET 8 Static SSR mode (`@rendermode` not set, or `@attribute [StreamRendering(false)]`) for the `Dashboard.razor` page. This renders HTML server-side and sends it as a static response — no SignalR circuit. The page won't support live updates from `FileSystemWatcher`, but the user can manually refresh.
- **Recommended approach:** Use Interactive Server mode during data iteration (to benefit from `FileSystemWatcher` live reload), then switch to a simple page refresh for screenshot capture. The SignalR overhead is <50ms on localhost — imperceptible.

---

### Risk 4: JSON Schema Evolution / Breaking Changes

| Attribute | Value |
|-----------|-------|
| **Severity** | Low |
| **Likelihood** | High (over time) |
| **Impact** | Dashboard crashes or shows stale data after `data.json` format changes |

**Description:** As the dashboard evolves, new fields may be added to `data.json` (e.g., `schemaVersion`, `timelineMonths`, `categoryColors`). If the C# model and the JSON file get out of sync, deserialization silently drops unknown fields or uses default values for missing ones.

**Mitigation:**
- Add a `schemaVersion` field to `data.json` (start at `1`). The `DashboardDataService` checks this value on load and logs a warning if it's unrecognized.
- Use `[JsonPropertyName]` attributes on all model properties for explicit mapping (not relying on property name conventions).
- Validate required fields at startup and fail fast with a clear error message rather than rendering a broken layout.
- **Do not use `JsonSerializerOptions.PropertyNameCaseInsensitive`** — strict matching catches typos in `data.json`.
- Provide a `data.sample.json` that always reflects the current expected schema.

---

### Risk 5: FileSystemWatcher Reliability on Windows

| Attribute | Value |
|-----------|-------|
| **Severity** | Low |
| **Likelihood** | Low |
| **Impact** | Dashboard doesn't auto-refresh after `data.json` edit; user must manually reload |

**Description:** `FileSystemWatcher` on Windows can miss events if the file is saved by certain editors (e.g., those that write to a temp file and rename). It can also fire duplicate events for a single save.

**Mitigation:**
- Use a **debounce** strategy: After receiving a `Changed` event, wait 300ms before reloading. If another event arrives during the wait, reset the timer. This handles both duplicate events and rapid sequential saves.
- Watch for both `Changed` and `Renamed` events to catch editors that use the temp-file-rename pattern.
- If `FileSystemWatcher` fails to fire, the user can always manually refresh the browser (F5). Document this as a known behavior.

```csharp
private Timer? _debounceTimer;

private void OnFileChanged(object sender, FileSystemEventArgs e)
{
    _debounceTimer?.Dispose();
    _debounceTimer = new Timer(_ =>
    {
        _cache.Clear();
        OnDataChanged?.Invoke();
    }, null, 300, Timeout.Infinite);
}
```

---

### Risk 6: Heatmap Cell Content Overflow

| Attribute | Value |
|-----------|-------|
| **Severity** | Medium |
| **Likelihood** | Medium |
| **Impact** | Items in a heatmap cell overflow and get clipped by `overflow: hidden`, making text unreadable in screenshots |

**Description:** Each heatmap cell has a fixed height determined by `grid-template-rows: 36px repeat(4, 1fr)`. At 1080px total page height minus header (~50px) minus timeline (196px) minus heatmap title (~30px) minus grid header (36px) minus padding, each data row gets approximately 180px. With 12px font and 1.35 line-height, each item occupies ~20px. A cell can fit ~8-9 items before clipping.

**Mitigation:**
- Document the 8-item-per-cell limit in the `data.json` schema documentation and the `data.sample.json` comments.
- Add a validation warning (not error) if any cell exceeds 8 items: `"Warning: {category}/{month} has {count} items; cells support up to 8 without overflow"`.
- Apply `overflow: hidden` on cells (matching the reference design) so clipping is clean, not jagged.
- If a PM needs more than 8 items, they should aggregate (e.g., "5 bug fixes" instead of listing each one).

---

### Risk 7: Query Parameter Injection

| Attribute | Value |
|-----------|-------|
| **Severity** | Low |
| **Likelihood** | Low |
| **Impact** | Path traversal could read arbitrary JSON files from the file system |

**Description:** The `?project=` parameter is used to construct a file path. A malicious value like `../../etc/passwd` could attempt path traversal.

**Mitigation:**
- Sanitize the project parameter to allow only `[a-zA-Z0-9_-]` characters. Reject any value containing `.`, `/`, `\`, or whitespace.
- Use `Path.GetFileName()` on the resolved path to strip directory components.
- Verify the resolved file path starts with the content root directory before reading.

```csharp
private static readonly Regex ValidProjectName = new(@"^[a-zA-Z0-9_-]+$");

private string ResolveFilePath(string projectName)
{
    if (!string.IsNullOrEmpty(projectName) && !ValidProjectName.IsMatch(projectName))
        throw new DashboardDataException($"Invalid project name: '{projectName}'");
    // ... proceed with file resolution
}
```

---

### Decision Log

| # | Decision | Rationale | Date |
|---|----------|-----------|------|
| D1 | Use Blazor Server Interactive mode (not Static SSR) | Enables `FileSystemWatcher` → `StateHasChanged()` live push for hot-reload workflow. Overhead is negligible on localhost. | 2026-04-14 |
| D2 | Zero external NuGet packages for production | Business requirement. Eliminates supply chain risk and version management overhead. | 2026-04-14 |
| D3 | `data.{project}.json` as primary multi-project convention | Simpler than a subfolder; all data files are visible at the project root. Falls back to `projects/` subfolder for teams that prefer organization. | 2026-04-14 |
| D4 | Hard-code SVG width to 1560px | Matches reference design exactly. Dynamic calculation would introduce rounding errors and positioning drift. | 2026-04-14 |
| D5 | CSS custom properties over Sass/LESS | No build toolchain needed. CSS variables are natively supported in all Chromium browsers. Keeps the zero-dependency promise. | 2026-04-14 |
| D6 | `FileSystemWatcher` with 300ms debounce | Handles duplicate events and rapid saves. Simple, reliable, and built into .NET. | 2026-04-14 |
| D7 | Validate `data.json` at startup, not lazily | Fail fast with a clear error message. A PM should know immediately if their JSON is broken, not discover it when they open the browser. | 2026-04-14 |
| D8 | No `HeatmapItem.cs` model class | Items are plain strings (`List<string>`). A wrapper class adds complexity with no benefit — there are no item-level properties beyond the display text. | 2026-04-14 |