# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page, screenshot-optimized Blazor Server application that visualizes project milestones on an SVG timeline and displays monthly execution status in a color-coded heatmap grid. It runs locally with zero cloud dependencies, reads all data from a single `data.json` file, and renders at exactly 1920×1080 pixels for direct screenshot-to-PowerPoint workflows.

**Architecture Principles:**

1. **Radical simplicity** — Single project, zero external NuGet dependencies, no database, no authentication.
2. **Data-driven rendering** — 100% of displayed content is sourced from `data.json`; no hardcoded text in components.
3. **Pixel fidelity** — CSS and SVG output must match `OriginalDesignConcept.html` at 1920×1080.
4. **Live iteration** — `FileSystemWatcher` enables sub-2-second refresh on data file edits without browser reload.
5. **Zero-config startup** — `dotnet run` from the project root is the only command needed.

---

## System Components

### Solution Structure

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── Program.cs
    ├── data.json
    ├── Models/
    │   └── DashboardData.cs
    ├── Services/
    │   └── DashboardDataService.cs
    ├── Components/
    │   ├── App.razor
    │   ├── Routes.razor
    │   ├── Layout/
    │   │   └── MainLayout.razor
    │   ├── Pages/
    │   │   └── Dashboard.razor
    │   └── Shared/
    │       ├── Header.razor
    │       ├── Timeline.razor
    │       ├── Heatmap.razor
    │       └── HeatmapCell.razor
    └── wwwroot/
        └── css/
            └── dashboard.css
```

### Component Specifications

#### 1. `Program.cs` — Application Entry Point

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure Kestrel, register services, parse CLI arguments, start the app |
| **Interfaces** | Accepts `--data <filepath>` CLI argument |
| **Dependencies** | `DashboardDataService` (registered as singleton) |
| **Key Behavior** | Parses `args` for `--data` flag; defaults to `data.json` in project root. Configures Kestrel to bind `localhost:5000` (HTTP only). Sets `DisconnectedCircuitRetentionPeriod` to 1 hour. Registers `DashboardDataService` as a singleton with the resolved file path. |

```csharp
// Pseudocode for Program.cs
var builder = WebApplication.CreateBuilder(args);

// Parse --data argument
string dataFilePath = "data.json"; // default
var dataArgIndex = Array.IndexOf(args, "--data");
if (dataArgIndex >= 0 && dataArgIndex + 1 < args.Length)
    dataFilePath = args[dataArgIndex + 1];

// Resolve to absolute path
dataFilePath = Path.GetFullPath(dataFilePath);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton(new DashboardDataServiceOptions { FilePath = dataFilePath });
builder.Services.AddSingleton<DashboardDataService>();

builder.Services.Configure<CircuitOptions>(options =>
{
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromHours(1);
});

var app = builder.Build();

// Trigger initial data load (validates file exists and is valid JSON)
var dataService = app.Services.GetRequiredService<DashboardDataService>();
dataService.Initialize();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

#### 2. `DashboardDataService` — Data Loading & File Watching

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Read, deserialize, cache, and watch `data.json`; notify subscribers on change |
| **Interfaces** | `DashboardData? Data` property, `string? ErrorMessage` property, `event Action OnDataChanged` |
| **Dependencies** | `System.Text.Json`, `System.IO.FileSystemWatcher` |
| **Lifetime** | Singleton (one instance per application) |

**Internal behavior:**

```csharp
public class DashboardDataServiceOptions
{
    public string FilePath { get; set; } = "data.json";
}

public class DashboardDataService : IDisposable
{
    private readonly string _filePath;
    private readonly FileSystemWatcher _watcher;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _lock = new();
    private DashboardData? _data;
    private string? _errorMessage;

    public DashboardData? Data { get { lock (_lock) return _data; } }
    public string? ErrorMessage { get { lock (_lock) return _errorMessage; } }
    public event Action? OnDataChanged;

    public DashboardDataService(DashboardDataServiceOptions options)
    {
        _filePath = options.FilePath;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        var directory = Path.GetDirectoryName(_filePath)!;
        var fileName = Path.GetFileName(_filePath);

        _watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        _watcher.Changed += OnFileChanged;
    }

    public void Initialize() => LoadData();

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce: wait 300ms for file write to complete
        Thread.Sleep(300);
        LoadData();
        OnDataChanged?.Invoke();
    }

    private void LoadData()
    {
        lock (_lock)
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _data = null;
                    _errorMessage = $"Data file not found: {_filePath}";
                    return;
                }

                var json = File.ReadAllText(_filePath);
                _data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);
                _errorMessage = _data == null ? "Data file is empty." : null;
            }
            catch (JsonException ex)
            {
                _data = null;
                _errorMessage = $"Invalid JSON in data file: {ex.Message}";
            }
            catch (IOException ex)
            {
                // File may be locked by editor; retain previous data
                _errorMessage = $"Could not read data file: {ex.Message}";
            }
        }
    }

    public void Dispose() => _watcher.Dispose();
}
```

**Debounce strategy:** The 300ms `Thread.Sleep` in `OnFileChanged` handles the common case where text editors write files in multiple steps (write temp → rename), which can trigger multiple `Changed` events. The lock ensures thread safety between the watcher callback and Blazor component reads.

#### 3. `Dashboard.razor` — Page Component (Single Page)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Top-level page; subscribes to `DashboardDataService`, passes data to child components |
| **Route** | `@page "/"` |
| **Dependencies** | `DashboardDataService` (injected) |
| **Render behavior** | If `Data` is null and `ErrorMessage` is set, renders error view. Otherwise renders Header → Timeline → Heatmap. |

```razor
@page "/"
@implements IDisposable
@inject DashboardDataService DataService

@if (DataService.ErrorMessage is not null && DataService.Data is null)
{
    <div class="error-container">
        <p>@DataService.ErrorMessage</p>
    </div>
}
else if (DataService.Data is { } data)
{
    <Header Data="data" />
    <Timeline TimelineData="data.Timeline" NowDate="@GetNowDate(data)" />
    <Heatmap HeatmapData="data.Heatmap" />
}

@code {
    protected override void OnInitialized()
    {
        DataService.OnDataChanged += HandleDataChanged;
    }

    private DateOnly GetNowDate(DashboardData data)
    {
        if (data.NowDate is { } overrideDate)
            return DateOnly.Parse(overrideDate);
        return DateOnly.FromDateTime(DateTime.Today);
    }

    private async void HandleDataChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        DataService.OnDataChanged -= HandleDataChanged;
    }
}
```

#### 4. `Header.razor` — Title Bar & Legend

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render project title, subtitle, ADO backlog link, and milestone legend |
| **Parameters** | `[Parameter] DashboardData Data` |
| **Dependencies** | None |
| **Visual mapping** | `.hdr` section from design reference |

**Layout:** Flexbox row with `justify-content: space-between`. Left side: title `<h1>` with optional `<a>` link, subtitle `<div>`. Right side: four legend items (PoC diamond, Production diamond, Checkpoint circle, Now bar) in a flex row with `gap: 22px`.

#### 5. `Timeline.razor` — SVG Milestone Timeline

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render inline SVG with month grid, track lines, milestone markers, and NOW line |
| **Parameters** | `[Parameter] TimelineConfig TimelineData`, `[Parameter] DateOnly NowDate` |
| **Dependencies** | None |
| **Visual mapping** | `.tl-area` section from design reference |

**SVG rendering logic:**

```csharp
@code {
    private const double SvgWidth = 1560;
    private const double SvgHeight = 185;

    private double GetXPosition(DateOnly date)
    {
        var start = DateOnly.ParseExact(TimelineData.StartMonth + "-01", "yyyy-MM-dd");
        var endMonth = DateOnly.ParseExact(TimelineData.EndMonth + "-01", "yyyy-MM-dd");
        var end = endMonth.AddMonths(1).AddDays(-1); // last day of end month
        var totalDays = end.DayNumber - start.DayNumber;
        if (totalDays <= 0) return 0;
        var offset = date.DayNumber - start.DayNumber;
        return Math.Clamp((offset / (double)totalDays) * SvgWidth, 0, SvgWidth);
    }

    private double GetTrackY(int trackIndex, int totalTracks)
    {
        // Evenly space tracks in the SVG height, leaving room for labels
        var usableHeight = SvgHeight - 30; // top padding for month labels
        var spacing = usableHeight / (totalTracks + 1);
        return 30 + spacing * (trackIndex + 1);
    }

    private string DiamondPoints(double cx, double cy, double radius = 11)
    {
        return $"{cx},{cy - radius} {cx + radius},{cy} {cx},{cy + radius} {cx - radius},{cy}";
    }

    private List<(DateOnly date, string label)> GetMonthGridLines()
    {
        var lines = new List<(DateOnly, string)>();
        var current = DateOnly.ParseExact(TimelineData.StartMonth + "-01", "yyyy-MM-dd");
        var endMonth = DateOnly.ParseExact(TimelineData.EndMonth + "-01", "yyyy-MM-dd");
        while (current <= endMonth)
        {
            lines.Add((current, current.ToString("MMM")));
            current = current.AddMonths(1);
        }
        return lines;
    }
}
```

**SVG elements rendered per milestone type:**

| Milestone Type | SVG Element | Fill/Stroke | Size |
|---------------|-------------|-------------|------|
| `checkpoint` (large) | `<circle>` | white fill, track-color stroke, `stroke-width="2.5"` | `r="7"` |
| `checkpoint` (small) | `<circle>` | `fill="#999"` | `r="4"` |
| `poc` | `<polygon>` (diamond) | `fill="#F4B400"`, `filter="url(#sh)"` | 11px radius |
| `production` | `<polygon>` (diamond) | `fill="#34A853"`, `filter="url(#sh)"` | 11px radius |
| NOW line | `<line>` | `stroke="#EA4335"`, `stroke-dasharray="5,3"`, `stroke-width="2"` | Full height |

The track label sidebar (230px) is rendered as HTML `<div>` elements to the left of the SVG, inside the `.tl-area` flex container.

#### 6. `Heatmap.razor` — CSS Grid Status Matrix

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the heatmap grid header row and delegate cells to `HeatmapCell` |
| **Parameters** | `[Parameter] HeatmapConfig HeatmapData` |
| **Dependencies** | `HeatmapCell.razor` |
| **Visual mapping** | `.hm-wrap` and `.hm-grid` sections from design reference |

**Grid layout:** `grid-template-columns: 160px repeat(N, 1fr)` where N = `HeatmapData.Months.Count`. The column count is set via an inline `style` attribute since CSS custom properties require JS interop in Blazor.

**Rendering order (row-major in grid):**
1. Corner cell ("STATUS")
2. Month header cells (N cells, current month gets `.current-month-hdr` class)
3. For each category (Shipped, In Progress, Carryover, Blockers):
   - Row header cell
   - N data cells (one per month, each rendered by `HeatmapCell`)

#### 7. `HeatmapCell.razor` — Individual Heatmap Cell

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render a list of work items with colored dot indicators, or a dash for empty cells |
| **Parameters** | `[Parameter] List<string> Items`, `[Parameter] string CssClass`, `[Parameter] bool IsCurrentMonth` |
| **Dependencies** | None |
| **Visual mapping** | `.hm-cell` and `.it` elements from design reference |

**Behavior:** If `Items` is null or empty, renders `<span style="color:#AAA">–</span>`. Otherwise renders each item as `<div class="it">@item</div>` where the `.it::before` pseudo-element provides the colored dot via the parent category's CSS class.

#### 8. `MainLayout.razor` — Shell Layout

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Minimal layout wrapper; renders `@Body` with no navigation chrome |
| **Dependencies** | None |

```razor
@inherits LayoutComponentBase

@Body
```

No `<nav>`, no sidebar, no footer in the layout. The dashboard page owns the full viewport.

#### 9. `App.razor` — Root Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | HTML document root with `<head>` (CSS link, viewport meta) and `<body>` (Blazor component outlet) |
| **Dependencies** | `dashboard.css` |

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=1920" />
    <title>Executive Dashboard</title>
    <link rel="stylesheet" href="css/dashboard.css" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.server.js"></script>
</body>
</html>
```

#### 10. `dashboard.css` — Global Stylesheet

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | All visual styling, ported verbatim from `OriginalDesignConcept.html` |
| **Strategy** | Single global CSS file (not scoped) for simplicity and screenshot fidelity |

The CSS is ported directly from the design reference with class names preserved (`.hdr`, `.tl-area`, `.hm-wrap`, `.hm-grid`, `.hm-cell`, `.it`, `.ship-*`, `.prog-*`, `.carry-*`, `.block-*`). No CSS-in-JS, no scoped CSS, no CSS custom properties that require JavaScript.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────┐     read       ┌──────────────────────────┐
│  data.json  │───────────────▶│  DashboardDataService    │
│  (flat file) │               │  (singleton)             │
└─────────────┘               │                          │
       │                       │  • DashboardData? Data   │
       │ FileSystemWatcher     │  • string? ErrorMessage  │
       │ .Changed event        │  • event OnDataChanged   │
       │                       └──────────┬───────────────┘
       └───────────────────────────────────┤
                                           │ Inject + Subscribe
                              ┌────────────▼────────────────┐
                              │  Dashboard.razor            │
                              │  @page "/"                  │
                              │  OnDataChanged → StateHasChanged
                              └────┬──────┬──────┬──────────┘
                                   │      │      │
                    ┌──────────────┘      │      └──────────────┐
                    ▼                     ▼                     ▼
            ┌───────────────┐  ┌──────────────────┐  ┌──────────────────┐
            │  Header.razor │  │  Timeline.razor   │  │  Heatmap.razor   │
            │  (title, legend)│ │  (SVG rendering) │  │  (CSS Grid)      │
            └───────────────┘  └──────────────────┘  └────────┬─────────┘
                                                              │
                                                    ┌─────────▼─────────┐
                                                    │ HeatmapCell.razor │
                                                    │ (per cell × N×4)  │
                                                    └───────────────────┘
```

### Communication Patterns

| Pattern | Mechanism | Detail |
|---------|-----------|--------|
| **File → Service** | `FileSystemWatcher.Changed` event | Service reads file on change with 300ms debounce |
| **Service → Page** | C# `event Action OnDataChanged` | Dashboard subscribes in `OnInitialized`, unsubscribes in `Dispose` |
| **Page → Components** | Blazor `[Parameter]` binding | One-way data flow: parent passes data objects as parameters |
| **Server → Browser** | SignalR (Blazor Server built-in) | `InvokeAsync(StateHasChanged)` triggers differential DOM update over the SignalR circuit |
| **Browser → Server** | SignalR (Blazor Server built-in) | Only for circuit keepalive; no user input events in MVP |

### Lifecycle Sequence: Initial Page Load

```
1. User navigates to http://localhost:5000
2. Kestrel handles HTTP request
3. Blazor Server renders Dashboard.razor (server-side prerender)
4. Dashboard.razor reads DashboardDataService.Data (already loaded at startup)
5. Dashboard passes data to Header, Timeline, Heatmap components
6. Full HTML is sent to browser in initial HTTP response
7. blazor.server.js establishes SignalR WebSocket connection
8. Page is interactive (circuit alive)
```

### Lifecycle Sequence: Data File Change

```
1. User saves data.json in text editor
2. FileSystemWatcher fires Changed event
3. DashboardDataService.OnFileChanged sleeps 300ms (debounce)
4. Service reads file, deserializes JSON, updates _data field
5. Service invokes OnDataChanged event
6. Dashboard.HandleDataChanged calls InvokeAsync(StateHasChanged)
7. Blazor computes component tree diff
8. Diff is sent to browser over SignalR
9. Browser DOM updates (< 2 seconds total from file save)
```

---

## Data Model

### JSON Schema (`data.json`)

```json
{
  "title": "string (required) — Dashboard title displayed in header",
  "subtitle": "string (required) — Team/workstream/date context line",
  "backlogUrl": "string (optional) — URL for ADO Backlog hyperlink",
  "nowDate": "string (optional) — ISO date 'YYYY-MM-DD' override for NOW line",
  "timeline": {
    "startMonth": "string (required) — 'YYYY-MM' format, first month on timeline",
    "endMonth": "string (required) — 'YYYY-MM' format, last month on timeline",
    "tracks": [
      {
        "id": "string (required) — Short identifier, e.g. 'M1'",
        "label": "string (required) — Description, e.g. 'Chatbot & MS Role'",
        "color": "string (required) — Hex color, e.g. '#0078D4'",
        "milestones": [
          {
            "date": "string (required) — 'YYYY-MM-DD' format",
            "type": "string (required) — 'checkpoint' | 'checkpoint-small' | 'poc' | 'production'",
            "label": "string (required) — Display label near marker"
          }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["string array (required) — Month names in display order"],
    "currentMonth": "string (required) — Name of the highlighted current month",
    "categories": [
      {
        "name": "string (required) — 'Shipped' | 'In Progress' | 'Carryover' | 'Blockers'",
        "cssClass": "string (required) — 'ship' | 'prog' | 'carry' | 'block'",
        "items": {
          "MonthName": ["string array — work item descriptions for this month"]
        }
      }
    ]
  }
}
```

### C# POCO Model (`Models/DashboardData.cs`)

```csharp
namespace ReportingDashboard.Models;

public class DashboardData
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string? BacklogUrl { get; set; }
    public string? NowDate { get; set; }
    public TimelineConfig Timeline { get; set; } = new();
    public HeatmapConfig Heatmap { get; set; } = new();
}

public class TimelineConfig
{
    public string StartMonth { get; set; } = "";
    public string EndMonth { get; set; } = "";
    public List<Track> Tracks { get; set; } = new();
}

public class Track
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Color { get; set; } = "#0078D4";
    public List<Milestone> Milestones { get; set; } = new();
}

public class Milestone
{
    public string Date { get; set; } = "";
    public string Type { get; set; } = "checkpoint";
    public string Label { get; set; } = "";
}

public class HeatmapConfig
{
    public List<string> Months { get; set; } = new();
    public string CurrentMonth { get; set; } = "";
    public List<HeatmapCategory> Categories { get; set; } = new();
}

public class HeatmapCategory
{
    public string Name { get; set; } = "";
    public string CssClass { get; set; } = "";
    public Dictionary<string, List<string>> Items { get; set; } = new();
}
```

### Entity Relationships

```
DashboardData (root)
├── title, subtitle, backlogUrl, nowDate
├── TimelineConfig (1:1)
│   ├── startMonth, endMonth
│   └── Track[] (1:N, 1-10 tracks)
│       ├── id, label, color
│       └── Milestone[] (1:N per track)
│           ├── date, type, label
└── HeatmapConfig (1:1)
    ├── months[], currentMonth
    └── HeatmapCategory[] (1:4 fixed categories)
        ├── name, cssClass
        └── items: Dictionary<month, string[]>
```

### Storage

| Concern | Implementation |
|---------|---------------|
| **Primary storage** | `data.json` flat file on local filesystem |
| **Location** | Project root (default) or user-specified via `--data` CLI arg |
| **Format** | UTF-8 JSON, supports comments and trailing commas |
| **Size constraint** | Must handle up to 1 MB without degradation |
| **Caching** | In-memory singleton; re-read only on file change or app restart |
| **No database** | No SQLite, no LiteDB, no Entity Framework |

---

## API Contracts

This application has no REST API, no GraphQL, and no RPC endpoints. All communication is internal.

### HTTP Endpoints (Kestrel)

| Endpoint | Method | Response | Purpose |
|----------|--------|----------|---------|
| `/` | GET | HTML (Blazor Server-rendered page) | Dashboard page |
| `/_framework/blazor.server.js` | GET | JavaScript | Blazor SignalR client library (framework-provided) |
| `/css/dashboard.css` | GET | CSS | Dashboard stylesheet |
| `/_blazor` | WebSocket | SignalR hub | Blazor Server circuit (framework-managed) |

### Internal Service Contract

```csharp
// DashboardDataService public interface (not a formal C# interface — concrete class)
public class DashboardDataService : IDisposable
{
    // Properties (thread-safe reads)
    public DashboardData? Data { get; }
    public string? ErrorMessage { get; }

    // Events
    public event Action? OnDataChanged;

    // Methods
    public void Initialize();  // Called once at startup
    public void Dispose();     // Cleans up FileSystemWatcher
}
```

### Error Handling Contract

| Condition | Behavior |
|-----------|----------|
| `data.json` missing | `Data = null`, `ErrorMessage = "Data file not found: {path}"` → Error view rendered |
| `data.json` invalid JSON | `Data = null`, `ErrorMessage = "Invalid JSON in data file: {details}"` → Error view rendered |
| `data.json` empty | `Data = null`, `ErrorMessage = "Data file is empty."` → Error view rendered |
| File locked by editor | Previous `Data` retained, `ErrorMessage` set with IO error detail |
| `--data` arg points to nonexistent file | Error message displayed on initial load |
| Valid JSON but missing fields | Deserialization uses default values (empty strings, empty lists); components render gracefully with missing data |

### Error View Rendering

```html
<!-- Rendered when Data is null and ErrorMessage is set -->
<div class="error-container"
     style="display:flex; align-items:center; justify-content:center;
            width:1920px; height:1080px; font-family:'Segoe UI',Arial,sans-serif;">
    <p style="font-size:18px; color:#666; max-width:600px; text-align:center;">
        Unable to load dashboard data. Please check that data.json exists and contains valid JSON.
        <br/><br/>
        <span style="font-size:14px; color:#999;">{ErrorMessage}</span>
    </p>
</div>
```

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|---------------|
| **Operating System** | Windows 10/11 (primary). macOS/Linux functional but not screenshot-guaranteed. |
| **Runtime** | .NET 8 SDK (`8.0.x`) for development; .NET 8 Runtime for published builds |
| **Browser** | Chromium-based (Chrome 120+, Edge 120+) at 1920×1080 viewport |
| **Font** | Segoe UI (pre-installed on Windows 10/11) |
| **Disk** | < 5 MB for project files; ~65 MB for self-contained publish |
| **Memory** | < 100 MB runtime footprint |
| **Network** | Loopback only (`localhost:5000`) — no LAN or internet access required |

### Hosting Configuration

```csharp
// In Program.cs or appsettings.json
// Kestrel binds to localhost only — not exposed on network
builder.WebHost.UseUrls("http://localhost:5000");
```

Alternatively, in `Properties/launchSettings.json`:
```json
{
  "profiles": {
    "ReportingDashboard": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5000"
    }
  }
}
```

### Build & Publish

| Command | Purpose |
|---------|---------|
| `dotnet build` | Compile; must produce zero warnings, zero errors |
| `dotnet run` | Run locally with hot reload support |
| `dotnet watch run` | Run with CSS/Razor hot reload for development |
| `dotnet run -- --data ./other.json` | Run with alternate data file |
| `dotnet publish -c Release -o ./publish` | Framework-dependent publish (~2 MB) |
| `dotnet publish -c Release --self-contained -r win-x64` | Self-contained publish (~65 MB, no .NET install needed) |

### CI/CD

**Not required for MVP.** The project is a local tool with no deployment pipeline. If added later:

- `dotnet build --warnaserrors` in a GitHub Actions workflow
- `dotnet test` once Phase 2 tests exist
- Playwright screenshot comparison for visual regression (Phase 2)

### Storage

| Storage | Type | Location |
|---------|------|----------|
| `data.json` | User-editable JSON file | Project root (default) or CLI-specified path |
| `wwwroot/css/dashboard.css` | Static CSS file | Served by Kestrel static file middleware |
| No database | — | — |
| No blob storage | — | — |
| No temp files | — | Application does not create temporary files |

---

## Technology Stack Decisions

| Layer | Choice | Justification |
|-------|--------|---------------|
| **Framework** | .NET 8 / Blazor Server | Mandatory per project constraints. Provides server-side rendering, SignalR live updates, strong typing, and C#-only codebase. No JavaScript required. |
| **CSS** | Pure CSS Grid + Flexbox | The design reference uses specific pixel values and a grid layout that maps 1:1 to CSS Grid. Any component library (Bootstrap, Tailwind, MudBlazor) would fight the pixel-perfect layout and add unnecessary complexity. |
| **SVG Timeline** | Inline SVG in Razor markup | The timeline is a simple composition of `<line>`, `<circle>`, `<polygon>`, and `<text>` elements. A charting library (Chart.js, D3) would require JavaScript interop, add 200KB+ payload, and provide no benefit for this simple shape composition. |
| **JSON** | `System.Text.Json` (built-in) | Zero-dependency JSON deserialization. Case-insensitive matching, comment support, trailing comma tolerance. Source generators available if AOT is needed later. |
| **File watching** | `System.IO.FileSystemWatcher` (built-in) | Native OS file change notifications. Sufficient for single-file watching on Windows. Known edge cases (missed events) mitigated by manual refresh fallback. |
| **Hosting** | Kestrel (built-in) | Default ASP.NET Core web server. No IIS, no Nginx, no reverse proxy. Binds to localhost only. |
| **State management** | Singleton service + C# events | Simplest possible pattern for single-page, single-user app. No Flux, no Redux, no MediatR. |
| **External NuGet packages** | **None** (MVP) | All functionality is achievable with .NET 8 built-in libraries. Zero dependencies means zero supply chain risk and zero version conflicts. |

### Rejected Alternatives

| Alternative | Why Rejected |
|-------------|-------------|
| Blazor WebAssembly | Larger download, no `FileSystemWatcher`, no server-side file access. Would require a separate API for file reading. |
| Static HTML + vanilla JS | Loses strong typing, hot reload, and C# ecosystem. Team is C#-first. |
| MudBlazor / Radzen | Component libraries add 500KB+ CSS/JS and impose their own grid/layout system. The pixel-perfect design doesn't map to any library's grid; manual CSS is less work. |
| SQLite / LiteDB | No query requirements. JSON file is human-editable and version-controllable. A database adds complexity with zero benefit. |
| `IOptions<T>` for JSON config | `IConfiguration` binding is designed for `appsettings.json` with key-value pairs, not deeply nested domain data. Direct `JsonSerializer.Deserialize<T>` is simpler and more explicit. |

---

## Security Considerations

### Threat Model

This is a **single-user, localhost-only, non-sensitive** tool. The attack surface is minimal.

| Threat | Risk Level | Mitigation |
|--------|------------|------------|
| **Network exposure** | None | Kestrel binds to `localhost:5000` only. Not accessible from LAN or internet. |
| **Data sensitivity** | None | `data.json` contains project status text (shipped items, milestones). No PII, no credentials, no secrets. |
| **JSON injection** | Low | All data is rendered via Blazor's built-in HTML encoding (`@variable`). No `@((MarkupString)...)` usage for user data. Blazor automatically prevents XSS. |
| **File path traversal** | Low | The `--data` CLI argument resolves to an absolute path via `Path.GetFullPath()`. No URL-based file selection. Only the operator (who runs `dotnet run`) controls the file path. |
| **Supply chain** | None | Zero external NuGet packages in MVP. Only .NET 8 SDK built-in libraries. |
| **SignalR hijacking** | None | SignalR circuit is localhost-only. No authentication needed because the only user is the person who started the process. |

### Authentication & Authorization

- **None required.** No middleware, no cookies, no tokens, no identity providers.
- `Program.cs` does not call `AddAuthentication()`, `AddAuthorization()`, or `UseAuthentication()`.

### Data Protection

- No encryption at rest (JSON is plaintext, non-sensitive).
- No encryption in transit (HTTP on localhost; HTTPS optional via `dotnet dev-certs https --trust`).
- OS-level NTFS ACLs are the sole access control for the `data.json` file.

### Input Validation

| Input | Validation |
|-------|------------|
| `data.json` content | `System.Text.Json` deserialization with `try/catch`. Malformed JSON → user-friendly error message, no crash. |
| `--data` CLI argument | `File.Exists()` check before initializing FileSystemWatcher. Missing file → error message. |
| Blazor rendering | All `@variable` expressions are HTML-encoded by default. No raw HTML injection points. |
| `backlogUrl` | Rendered as `<a href="@url">` — Blazor encodes the attribute. If URL is malicious, it's a self-inflicted issue (user controls their own `data.json`). |

---

## Scaling Strategy

### Current Scale

This is a **single-user, single-machine tool**. There is no multi-user scenario, no concurrent access, and no horizontal scaling requirement.

| Dimension | Current | Maximum Supported |
|-----------|---------|-------------------|
| Concurrent users | 1 | 1 (Blazor Server circuit is per-tab) |
| Browser tabs | 1-2 | ~10 (limited by SignalR circuits; each tab = 1 circuit) |
| Timeline tracks | 3 (typical) | 10 (SVG Y-spacing adjusts dynamically) |
| Heatmap months | 4 (typical) | 12 (CSS Grid columns adjust via `repeat(N, 1fr)`) |
| Heatmap items per cell | 3-5 (typical) | ~15 (limited by cell height at 1080px viewport) |
| `data.json` size | 5-20 KB (typical) | 1 MB (per NFR) |

### Future Scaling Paths (Not In Scope)

| Scenario | Approach |
|----------|----------|
| Multiple projects | Already supported via `--data` CLI arg. Future: tabbed navigation (Phase 3). |
| Historical snapshots | Store dated JSON files: `data/2026-04.json`, `data/2026-03.json`. Future: month picker UI (Phase 3). |
| Team-wide deployment | Publish as self-contained EXE. Distribute via file share. Each user runs locally. |
| Larger datasets | If heatmap exceeds 12 months, add horizontal scrolling or pagination. Unlikely for executive reporting. |

---

## Risks & Mitigations

| # | Risk | Severity | Probability | Impact | Mitigation |
|---|------|----------|-------------|--------|------------|
| 1 | **Screenshot fidelity varies across machines** | Medium | Medium | Executive sees different layout than developer | Document screenshot procedure: Chrome DevTools → Device Emulation → 1920×1080 → 100% DPI → Capture full size screenshot. Verify Segoe UI font is installed. |
| 2 | **SignalR circuit disconnects after inactivity** | Low | High | User sees "Reconnecting..." overlay | Set `DisconnectedCircuitRetentionPeriod` to 1 hour. Blazor's built-in reconnect UI handles reconnection automatically. On reconnect, latest data is rendered. |
| 3 | **FileSystemWatcher misses change events** | Low | Low | Dashboard shows stale data after file save | Manual browser refresh (F5) always reloads latest data from the service (which re-reads the file). 300ms debounce handles rapid successive events. |
| 4 | **Text editors trigger multiple file events** | Low | High | Service reloads data twice in rapid succession | 300ms debounce in `OnFileChanged`. Lock ensures thread-safe read/write of cached data. Double-reload is harmless (idempotent). |
| 5 | **Heatmap overflow with many items** | Low | Low | Items overflow cell, clipped by `overflow:hidden` | CSS `overflow: hidden` matches design reference behavior. Phase 2 tooltip shows full text on hover. Users should keep items to 3-5 per cell for readability. |
| 6 | **SVG timeline crowding with 10+ tracks** | Low | Very Low | Track lines overlap, labels unreadable | Dynamic Y-spacing calculation adapts to track count. For >6 tracks, reduce SVG label font size. Executive dashboards rarely exceed 3-5 tracks. |
| 7 | **Blazor Server overhead for static content** | Low | N/A | Slightly higher memory than static HTML | Overhead is ~50 MB RAM for Kestrel + Blazor runtime. Acceptable for a local developer tool. Benefit: hot reload, strong typing, C#-only codebase. |
| 8 | **JSON schema evolution breaks backward compatibility** | Medium | Low | Updated app can't read old data.json | Use default values in POCO classes (empty strings, empty lists). Missing fields deserialize to defaults rather than throwing. Document schema version in `data.json` (future). |
| 9 | **High-DPI displays distort 1920×1080 layout** | Medium | Medium | Screenshot at 2x DPI produces 3840×2160 image | Document: set browser zoom to 100%, use Chrome DevTools device emulation at 1920×1080 with DPR=1 for consistent screenshots. |
| 10 | **File locked by editor during read** | Low | Low | IOException on data reload | `try/catch` in `LoadData()` retains previous `_data` on IO failure. Error message is set but dashboard continues to display last-good data. |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component.

### Component-to-Design Mapping

| Visual Section | Component | CSS Class | Layout Strategy | Data Bindings | Interactions |
|----------------|-----------|-----------|-----------------|---------------|-------------|
| **Full page container** | `App.razor` → `<body>` | `body` | `width:1920px; height:1080px; overflow:hidden; display:flex; flex-direction:column` | None (structural) | None |
| **Header bar** | `Header.razor` | `.hdr` | Flexbox row, `justify-content:space-between`, `padding:12px 44px 10px` | `Data.Title`, `Data.Subtitle`, `Data.BacklogUrl` | Click on ADO link opens URL |
| **Title + Subtitle (left)** | `Header.razor` (inline) | `.hdr h1`, `.sub` | Block elements within left flex child | `Data.Title`, `Data.BacklogUrl` (optional `<a>`), `Data.Subtitle` | None |
| **Legend (right)** | `Header.razor` (inline) | Inline styles matching reference | Flex row with `gap:22px` | Static labels; "Now" label includes current month from data | None |
| **Timeline area** | `Timeline.razor` wrapper | `.tl-area` | Flexbox row, `height:196px`, `background:#FAFAFA` | `TimelineData` (full timeline config) | None (static SVG) |
| **Track label sidebar** | `Timeline.razor` (HTML div) | Inline styles (230px sidebar) | Flex column, `justify-content:space-around` | `TimelineData.Tracks[].Id`, `.Label`, `.Color` | None |
| **SVG timeline chart** | `Timeline.razor` (inline `<svg>`) | `.tl-svg-box` | `<svg width="1560" height="185">` with computed positions | `TimelineData.Tracks[].Milestones[]` — date→X position, type→shape | None (static SVG) |
| **Month grid lines** | `Timeline.razor` SVG `<line>` | N/A (SVG attributes) | Vertical lines at `GetXPosition(monthStart)` | `TimelineData.StartMonth`, `.EndMonth` → computed month boundaries | None |
| **Milestone markers** | `Timeline.razor` SVG elements | N/A (SVG attributes) | `<circle>` for checkpoints, `<polygon>` for diamonds | `Milestone.Type` → shape, `Milestone.Date` → X position, `Milestone.Label` → `<text>` | None |
| **NOW line** | `Timeline.razor` SVG `<line>` + `<text>` | N/A (SVG: `stroke:#EA4335`) | Vertical dashed line at `GetXPosition(nowDate)` | `NowDate` parameter (auto or override) | None |
| **Heatmap wrapper** | `Heatmap.razor` | `.hm-wrap` | Flex column, `flex:1`, `padding:10px 44px` | `HeatmapData` | None |
| **Heatmap title** | `Heatmap.razor` (inline) | `.hm-title` | Block, `text-transform:uppercase` | Static text with category names | None |
| **Heatmap grid** | `Heatmap.razor` | `.hm-grid` | CSS Grid: `160px repeat(N,1fr)` / `36px repeat(4,1fr)` | `HeatmapData.Months.Count` → column count | None |
| **Corner cell** | `Heatmap.razor` (inline) | `.hm-corner` | Grid cell (1,1) | Static text "STATUS" | None |
| **Month header cells** | `Heatmap.razor` `@foreach` | `.hm-col-hdr`, `.current-month-hdr` | Grid cells (1, 2..N+1) | `HeatmapData.Months[]`, `HeatmapData.CurrentMonth` for highlight | None |
| **Row header cells** | `Heatmap.razor` `@foreach` | `.hm-row-hdr`, `.ship-hdr` / `.prog-hdr` / `.carry-hdr` / `.block-hdr` | Grid cells (row, 1) | `Category.Name` | None |
| **Data cells** | `HeatmapCell.razor` | `.hm-cell`, `.ship-cell` / `.prog-cell` / `.carry-cell` / `.block-cell` | Grid cells with `padding:8px 12px`, `overflow:hidden` | `Category.Items[month]` → list of strings | Phase 2: hover tooltip |
| **Cell items** | `HeatmapCell.razor` `@foreach` | `.it` with `::before` colored dot | Block elements, `font-size:12px`, dot via CSS pseudo-element | Individual item string | Phase 2: `@onmouseover` tooltip |
| **Empty cell placeholder** | `HeatmapCell.razor` (conditional) | Inline `color:#AAA` | Centered dash "–" | Rendered when `Items` is null or empty | None |
| **Error view** | `Dashboard.razor` (conditional) | `.error-container` | Centered flex container, `1920×1080` | `DataService.ErrorMessage` | None |

### CSS Class Hierarchy

```
body (1920×1080, flex column)
├── .hdr (flex row, space-between)
│   ├── h1 (24px bold)
│   │   └── a (optional backlog link, #0078D4)
│   ├── .sub (12px, #888)
│   └── legend (flex row, gap:22px)
├── .tl-area (flex row, 196px height, #FAFAFA)
│   ├── sidebar div (230px, flex column)
│   └── .tl-svg-box (flex:1)
│       └── <svg> (1560×185)
└── .hm-wrap (flex:1, flex column)
    ├── .hm-title (14px bold uppercase, #888)
    └── .hm-grid (CSS Grid)
        ├── .hm-corner (STATUS label)
        ├── .hm-col-hdr (month names)
        │   └── .current-month-hdr (#FFF0D0 bg)
        ├── .hm-row-hdr.ship-hdr / .prog-hdr / .carry-hdr / .block-hdr
        └── .hm-cell.ship-cell / .prog-cell / .carry-cell / .block-cell
            └── .it (item with ::before dot)
```

### Dynamic CSS Application

The heatmap grid column count varies with data. Since CSS custom properties require JavaScript interop in Blazor Server, the grid template is set via inline style:

```razor
<div class="hm-grid"
     style="grid-template-columns: 160px repeat(@HeatmapData.Months.Count, 1fr);">
```

Current month highlighting uses conditional CSS class application:

```razor
<div class="hm-col-hdr @(month == HeatmapData.CurrentMonth ? "current-month-hdr" : "")">
    @month
</div>

<div class="hm-cell @(category.CssClass)-cell @(month == HeatmapData.CurrentMonth ? "current-month" : "")">
    <HeatmapCell Items="@GetItems(category, month)"
                 CssClass="@category.CssClass"
                 IsCurrentMonth="@(month == HeatmapData.CurrentMonth)" />
</div>
```