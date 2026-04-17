# Architecture

## Overview & Goals

This is a single-page executive reporting dashboard built with C# .NET 8 Blazor Server. It visualizes project milestones on an SVG timeline and displays a color-coded heatmap grid of work items by delivery status. All data is read from a local `data.json` file. The application runs locally via `dotnet run` with zero cloud dependencies and is optimized for pixel-perfect 1920×1080 screenshots for PowerPoint decks.

**Architecture Principles:**

1. **Intentional simplicity** — No databases, no authentication, no third-party packages, no JavaScript. The entire codebase stays under 10 files.
2. **Data-driven rendering** — Every visible element is driven from `data.json`. Change the file, see the change.
3. **Pixel-perfect fidelity** — The CSS and SVG output must be visually indistinguishable from `OriginalDesignConcept.html` at 1920×1080.
4. **Zero infrastructure** — `dotnet run` is the only command needed. No build tools, no containers, no cloud services.

**Solution Structure:**

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── Program.cs
    ├── Components/
    │   ├── App.razor
    │   ├── _Imports.razor
    │   ├── Pages/
    │   │   └── Dashboard.razor
    │   ├── Layout/
    │   │   └── MainLayout.razor
    │   └── Shared/
    │       ├── Timeline.razor
    │       ├── Heatmap.razor
    │       └── HeatmapCell.razor
    ├── Models/
    │   └── DashboardData.cs
    ├── Services/
    │   └── DashboardDataService.cs
    ├── wwwroot/
    │   ├── css/
    │   │   └── app.css
    │   └── data/
    │       └── data.json
    └── ReportingDashboard.csproj
```

---

## System Components

### 1. `Program.cs` — Application Host

**Responsibility:** Minimal ASP.NET Core host builder. Registers `DashboardDataService` as a singleton, adds Blazor Server services, maps Razor components.

**Interfaces:** None exposed. This is the entry point.

**Dependencies:** `Microsoft.AspNetCore.App` framework (ships with .NET 8 SDK).

**Key behavior:**
- Registers `DashboardDataService` as a singleton via `builder.Services.AddSingleton<DashboardDataService>()`
- Configures Kestrel to bind to `http://localhost:5000` by default
- Adds Razor Components with `AddServerComponents()` interactivity
- No HTTPS redirect (localhost-only, no TLS needed for local dev)

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

### 2. `DashboardDataService` — Data Loading & Caching Service

**Responsibility:** Reads, deserializes, and caches `data.json`. Watches the file for changes and notifies subscribers when data is reloaded.

**Interfaces:**

| Member | Type | Description |
|--------|------|-------------|
| `Data` | `DashboardData?` | The current deserialized dashboard data (null if file missing) |
| `ErrorMessage` | `string?` | Error message if data could not be loaded |
| `OnDataChanged` | `event Action` | Fires when data is reloaded from disk |

**Dependencies:** `System.Text.Json`, `System.IO.FileSystemWatcher`, `IWebHostEnvironment` (for resolving `wwwroot` path), `ILogger<DashboardDataService>`

**Lifecycle:** Registered as a **singleton**. Initializes on first access. The `FileSystemWatcher` runs for the application's lifetime.

**Key behavior:**

```
Startup:
  1. Resolve path: {ContentRootPath}/wwwroot/data/data.json
  2. If file exists → deserialize with System.Text.Json → store in Data property
  3. If file missing → set ErrorMessage = "data.json not found at {path}"
  4. If JSON malformed → set ErrorMessage = parse error message, Data = null
  5. Start FileSystemWatcher on wwwroot/data/ directory, filter: "data.json"

On FileSystemWatcher.Changed:
  1. Wait 100ms debounce (file writes can trigger multiple events)
  2. Re-read and deserialize data.json
  3. If success → update Data, clear ErrorMessage, fire OnDataChanged
  4. If malformed → log warning, retain previous Data, do NOT fire OnDataChanged
```

**Thread safety:** The `Data` property is read by Blazor components on the render thread. Use `volatile` or a simple lock around the swap to ensure components always see a consistent object reference. Since this is single-user local, a simple reference swap is sufficient — no concurrent write contention exists.

**Error handling contract:**
- Missing file on startup → `Data` is null, `ErrorMessage` is set
- Malformed JSON on startup → `Data` is null, `ErrorMessage` is set with parse details
- Malformed JSON on hot-reload → previous `Data` retained, warning logged to console
- File deleted while running → previous `Data` retained, warning logged

### 3. `Dashboard.razor` — Page Component (Route: `/`)

**Responsibility:** Top-level page layout. Injects `DashboardDataService`, subscribes to `OnDataChanged`, renders the header section directly, and delegates timeline and heatmap to child components.

**Interfaces (Parameters):** None — this is the routed page.

**Dependencies:** `DashboardDataService` (injected), `Timeline.razor`, `Heatmap.razor`

**Renders:**
- Error state (centered message) when `Data` is null
- Header section (`.hdr`): title, subtitle, backlog link, legend
- `<Timeline>` component with timeline data passed as parameters
- `<Heatmap>` component with heatmap data passed as parameters

**Hot-reload subscription:**

```csharp
@implements IDisposable

protected override void OnInitialized()
{
    DataService.OnDataChanged += HandleDataChanged;
}

private void HandleDataChanged()
{
    InvokeAsync(StateHasChanged);
}

public void Dispose()
{
    DataService.OnDataChanged -= HandleDataChanged;
}
```

### 4. `Timeline.razor` — SVG Timeline Component

**Responsibility:** Renders the complete SVG timeline visualization including month grid lines, track lines, milestone markers, date labels, and the NOW indicator.

**Interfaces (Parameters):**

| Parameter | Type | Description |
|-----------|------|-------------|
| `TimelineData` | `TimelineConfig` | Timeline configuration from data.json |
| `CurrentDate` | `DateTime` | Controls the NOW line position |

**Dependencies:** None (pure rendering component).

**Key rendering logic:**

```
SVG coordinate system:
  - ViewBox width: computed as (number of months) × 260px, default ~1560px for 6 months
  - ViewBox height: 185px (fixed)
  - Tracks spaced evenly: trackY = 28 + (trackIndex × (150 / trackCount))

Date-to-X mapping:
  double DateToX(DateTime date)
  {
      double totalDays = (EndDate - StartDate).TotalDays;
      double elapsed = (date - StartDate).TotalDays;
      return (elapsed / totalDays) * SvgWidth;
  }

Month grid lines:
  For each month boundary between StartDate and EndDate:
    x = DateToX(firstDayOfMonth)
    Render: <line x1="{x}" y1="0" x2="{x}" y2="185" stroke="#bbb" stroke-opacity="0.4"/>
    Render: <text x="{x+5}" y="14" ...>{monthAbbreviation}</text>

Milestone shapes by type:
  "checkpoint" → <circle cx="{x}" cy="{trackY}" r="5-7" fill="white" stroke="{trackColor}" stroke-width="2.5"/>
  "poc"        → <polygon points="{diamond(x, trackY, 11)}" fill="#F4B400" filter="url(#sh)"/>
  "production" → <polygon points="{diamond(x, trackY, 11)}" fill="#34A853" filter="url(#sh)"/>

Diamond point calculation:
  string Diamond(double cx, double cy, double r)
      => $"{cx},{cy-r} {cx+r},{cy} {cx},{cy+r} {cx-r},{cy}";

NOW line:
  x = DateToX(CurrentDate)
  <line x1="{x}" y1="0" x2="{x}" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
  <text x="{x+4}" y="14" fill="#EA4335" font-size="10" font-weight="700">NOW</text>

SVG tooltip (native):
  Each milestone shape wraps in <g> with <title>{label} - {date}</title>
```

**Dynamic track count handling:** The SVG height remains fixed at 185px. Track Y positions are calculated as `28 + (index * verticalSpacing)` where `verticalSpacing = 150 / max(trackCount, 1)`. The left sidebar in `Dashboard.razor` uses `justify-content: space-around` to vertically align track labels with SVG tracks.

### 5. `Heatmap.razor` — CSS Grid Heatmap Component

**Responsibility:** Renders the full heatmap grid including the section title, header row (corner + month headers), and four status rows with cells.

**Interfaces (Parameters):**

| Parameter | Type | Description |
|-----------|------|-------------|
| `HeatmapData` | `HeatmapConfig` | Heatmap configuration from data.json |

**Dependencies:** `HeatmapCell.razor`

**Key rendering logic:**

```
Grid style (dynamic):
  grid-template-columns: 160px repeat({monthCount}, 1fr)
  grid-template-rows: 36px repeat(4, 1fr)

Header row:
  Corner cell: "STATUS" label
  For each month in HeatmapData.Months:
    If month == CurrentMonth → apply .current-month-hdr class (gold bg)
    Else → standard .hm-col-hdr class

Status rows (fixed order):
  1. "shipped"     → green palette
  2. "inprogress"  → blue palette
  3. "carryover"   → amber palette
  4. "blockers"    → red palette

For each status row:
  Render row header with status-specific CSS class
  For each month:
    Render <HeatmapCell> with items list and status CSS prefix
```

**Status row configuration (hardcoded in component):**

```csharp
private static readonly StatusRowConfig[] StatusRows = new[]
{
    new StatusRowConfig("shipped",    "SHIPPED",     "ship"),
    new StatusRowConfig("inprogress", "IN PROGRESS", "prog"),
    new StatusRowConfig("carryover",  "CARRYOVER",   "carry"),
    new StatusRowConfig("blockers",   "BLOCKERS",    "block"),
};
```

### 6. `HeatmapCell.razor` — Individual Grid Cell Component

**Responsibility:** Renders a single heatmap cell containing bullet-pointed work items or a dash for empty cells.

**Interfaces (Parameters):**

| Parameter | Type | Description |
|-----------|------|-------------|
| `Items` | `List<string>?` | Work item descriptions for this cell |
| `CssClass` | `string` | CSS class prefix (e.g., `ship-cell`, `prog-cell`) |
| `IsCurrentMonth` | `bool` | Whether to apply current-month highlight |

**Dependencies:** None.

**Rendering logic:**

```
If Items is null or empty:
  Render: <div class="{CssClass} {currentClass}"><span style="color:#999">—</span></div>
Else:
  Render: <div class="{CssClass} {currentClass}">
            @foreach item in Items:
              <div class="it">{item}</div>
          </div>
```

### 7. `App.razor` — Root Component

**Responsibility:** HTML document shell. Sets `<head>` with CSS link, viewport meta (none needed — fixed layout), and renders the `<body>` with the router outlet.

**Key markup:**
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <link rel="stylesheet" href="css/app.css" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

### 8. `MainLayout.razor` — Layout Wrapper

**Responsibility:** Minimal layout that renders `@Body` with no additional chrome (no nav bars, no sidebars). The dashboard IS the entire page.

```html
@inherits LayoutComponentBase
@Body
```

### 9. `DashboardData.cs` — POCO Data Model

**Responsibility:** C# classes that map 1:1 to the `data.json` schema. Used by `System.Text.Json` for deserialization. Serves as the schema documentation via XML comments.

(Full model defined in Data Model section below.)

### 10. `app.css` — Global Stylesheet

**Responsibility:** All CSS for the dashboard, ported directly from `OriginalDesignConcept.html`. Uses CSS custom properties for the color palette to enable easy theming.

**Key concerns:**
- `body` fixed at 1920×1080 with `overflow: hidden`
- CSS Grid for heatmap, Flexbox for header and timeline area
- All color hex values match the design spec exactly
- No media queries (fixed resolution only)

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────┐     read on startup      ┌──────────────────────┐
│  data.json  │ ───────────────────────── │ DashboardDataService │
│  (wwwroot/  │     + FileSystemWatcher   │    (Singleton)       │
│   data/)    │ ◄─── watches for changes  │                      │
└─────────────┘                           └──────────┬───────────┘
                                                     │
                                          Data property + OnDataChanged event
                                                     │
                                          ┌──────────▼───────────┐
                                          │   Dashboard.razor    │
                                          │   (Page, route "/")  │
                                          │                      │
                                          │  Renders header      │
                                          │  inline; passes      │
                                          │  data to children    │
                                          └───┬─────────────┬────┘
                                              │             │
                               TimelineData   │             │  HeatmapData
                                              │             │
                                   ┌──────────▼──┐   ┌─────▼──────────┐
                                   │  Timeline   │   │   Heatmap      │
                                   │  .razor     │   │   .razor       │
                                   │  (SVG)      │   │  (CSS Grid)    │
                                   └─────────────┘   └────┬───────────┘
                                                          │
                                                 For each cell:
                                                          │
                                                   ┌──────▼───────┐
                                                   │ HeatmapCell  │
                                                   │ .razor       │
                                                   └──────────────┘
```

### Communication Patterns

| From | To | Mechanism | Data |
|------|----|-----------|------|
| `data.json` | `DashboardDataService` | File I/O (`File.ReadAllText`) | Raw JSON string |
| `DashboardDataService` | `Dashboard.razor` | DI injection + property access | `DashboardData` object |
| `DashboardDataService` | `Dashboard.razor` | `OnDataChanged` event → `InvokeAsync(StateHasChanged)` | Signal only (no payload) |
| `Dashboard.razor` | `Timeline.razor` | Blazor `[Parameter]` binding | `TimelineConfig`, `DateTime` |
| `Dashboard.razor` | `Heatmap.razor` | Blazor `[Parameter]` binding | `HeatmapConfig` |
| `Heatmap.razor` | `HeatmapCell.razor` | Blazor `[Parameter]` binding | `List<string>`, `string`, `bool` |

### Hot-Reload Sequence

```
1. User saves data.json in text editor
2. FileSystemWatcher fires Changed event
3. DashboardDataService debounces (100ms), reads file, deserializes
4. If success: swaps Data reference, fires OnDataChanged
5. Dashboard.razor handler calls InvokeAsync(StateHasChanged)
6. Blazor Server re-renders component tree over existing SignalR connection
7. Browser DOM updates via SignalR diff — no full page reload
8. Total latency target: < 500ms from file save to visual update
```

### Error State Flow

```
Startup, file missing:
  DashboardDataService.Data = null
  DashboardDataService.ErrorMessage = "data.json not found..."
  Dashboard.razor checks Data == null → renders error message panel

Startup, malformed JSON:
  DashboardDataService.Data = null
  DashboardDataService.ErrorMessage = "Failed to parse data.json: {details}"
  Dashboard.razor renders error message panel

Hot-reload, malformed JSON:
  DashboardDataService.Data = (previous valid data, unchanged)
  Console logs: "Warning: Failed to reload data.json: {details}"
  Dashboard.razor continues showing last valid data (no re-render triggered)
```

---

## Data Model

### `data.json` Schema

The JSON file is the single source of truth for all dashboard content. The schema is designed to be human-editable by project leads who are not developers.

```json
{
  "title": "Project Atlas - Cloud Migration Platform",
  "subtitle": "Cloud Engineering · Migration Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "currentDate": "2026-04-10",
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "label": "API Gateway",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-15", "type": "checkpoint", "label": "Jan 15" },
          { "date": "2026-03-20", "type": "poc", "label": "Mar 20 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "May 1 GA" }
        ]
      },
      {
        "id": "M2",
        "label": "Data Migration Engine",
        "color": "#00897B",
        "milestones": [
          { "date": "2026-02-10", "type": "checkpoint", "label": "Feb 10" },
          { "date": "2026-03-15", "type": "poc", "label": "Mar 15 PoC" },
          { "date": "2026-05-15", "type": "production", "label": "May 15 GA" }
        ]
      },
      {
        "id": "M3",
        "label": "Monitoring Dashboard",
        "color": "#546E7A",
        "milestones": [
          { "date": "2026-03-01", "type": "checkpoint", "label": "Mar 1" },
          { "date": "2026-04-15", "type": "poc", "label": "Apr 15 PoC" }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["January", "February", "March", "April"],
    "currentMonth": "April",
    "rows": [
      {
        "status": "shipped",
        "items": {
          "January": ["Auth service migrated", "DNS cutover complete"],
          "February": ["Blob storage sync tool", "Config service v2"],
          "March": ["API Gateway PoC done", "Data migration dry-run"],
          "April": ["Monitoring agent v1"]
        }
      },
      {
        "status": "inprogress",
        "items": {
          "January": [],
          "February": [],
          "March": ["Load test framework"],
          "April": ["Gateway perf tuning", "Migration rollback proc", "Dashboard wireframes"]
        }
      },
      {
        "status": "carryover",
        "items": {
          "January": [],
          "February": [],
          "March": [],
          "April": ["SDK v2 upgrade (from Mar)"]
        }
      },
      {
        "status": "blockers",
        "items": {
          "January": [],
          "February": [],
          "March": [],
          "April": ["Waiting on VNet peering approval"]
        }
      }
    ]
  }
}
```

### C# POCO Model (`DashboardData.cs`)

```csharp
using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

/// <summary>
/// Root model for the dashboard data.json file.
/// All dashboard content is driven from this structure.
/// </summary>
public class DashboardData
{
    /// <summary>Project title displayed in the header (24px bold).</summary>
    public string Title { get; set; } = "";

    /// <summary>Subtitle line: org, workstream, and month (12px gray).</summary>
    public string Subtitle { get; set; } = "";

    /// <summary>URL for the ADO Backlog link. If null/empty, link is hidden.</summary>
    public string? BacklogUrl { get; set; }

    /// <summary>
    /// Controls the NOW line position on the timeline.
    /// Format: "yyyy-MM-dd". Uses this instead of system date for reproducible screenshots.
    /// </summary>
    public string CurrentDate { get; set; } = "";

    /// <summary>Timeline/Gantt chart configuration.</summary>
    public TimelineConfig Timeline { get; set; } = new();

    /// <summary>Monthly execution heatmap configuration.</summary>
    public HeatmapConfig Heatmap { get; set; } = new();
}

/// <summary>Configuration for the SVG timeline section.</summary>
public class TimelineConfig
{
    /// <summary>Timeline start date (left edge). Format: "yyyy-MM-dd".</summary>
    public string StartDate { get; set; } = "";

    /// <summary>Timeline end date (right edge). Format: "yyyy-MM-dd".</summary>
    public string EndDate { get; set; } = "";

    /// <summary>Milestone tracks (2-5 supported without overflow).</summary>
    public List<TrackConfig> Tracks { get; set; } = new();
}

/// <summary>A single milestone track (horizontal line with markers).</summary>
public class TrackConfig
{
    /// <summary>Track identifier displayed in the sidebar (e.g., "M1").</summary>
    public string Id { get; set; } = "";

    /// <summary>Track description displayed below the ID.</summary>
    public string Label { get; set; } = "";

    /// <summary>Track color as CSS hex (e.g., "#0078D4").</summary>
    public string Color { get; set; } = "#999";

    /// <summary>Milestones positioned on this track.</summary>
    public List<MilestoneConfig> Milestones { get; set; } = new();
}

/// <summary>A milestone marker on a timeline track.</summary>
public class MilestoneConfig
{
    /// <summary>Milestone date. Format: "yyyy-MM-dd".</summary>
    public string Date { get; set; } = "";

    /// <summary>
    /// Milestone type determines the shape:
    /// "checkpoint" → circle, "poc" → gold diamond, "production" → green diamond.
    /// </summary>
    public string Type { get; set; } = "checkpoint";

    /// <summary>Label displayed near the milestone marker.</summary>
    public string Label { get; set; } = "";
}

/// <summary>Configuration for the monthly execution heatmap.</summary>
public class HeatmapConfig
{
    /// <summary>Month column headers in display order (e.g., ["January", "February", ...]).</summary>
    public List<string> Months { get; set; } = new();

    /// <summary>Which month is "current" — gets highlighted styling.</summary>
    public string CurrentMonth { get; set; } = "";

    /// <summary>Status rows. Expected: shipped, inprogress, carryover, blockers.</summary>
    public List<HeatmapRow> Rows { get; set; } = new();
}

/// <summary>A single status row in the heatmap (e.g., "shipped").</summary>
public class HeatmapRow
{
    /// <summary>
    /// Status key: "shipped", "inprogress", "carryover", or "blockers".
    /// Maps to CSS class prefixes and row header labels.
    /// </summary>
    public string Status { get; set; } = "";

    /// <summary>
    /// Work items keyed by month name. Each month maps to a list of short descriptions.
    /// Missing months or empty arrays render as a dash in the cell.
    /// </summary>
    public Dictionary<string, List<string>> Items { get; set; } = new();
}
```

### Data Validation Rules

| Field | Rule | Error Behavior |
|-------|------|----------------|
| `title` | Required, non-empty | Empty string renders blank header |
| `currentDate` | Must parse as `yyyy-MM-dd` | If unparseable, NOW line is not rendered |
| `timeline.startDate`, `endDate` | Must parse as dates, start < end | Timeline section shows error if invalid |
| `timeline.tracks` | 0-5 items | Empty = no timeline rendered; >5 may clip |
| `milestone.type` | One of: `checkpoint`, `poc`, `production` | Unknown types render as checkpoints |
| `heatmap.months` | 1-6 items | Grid columns adjust dynamically |
| `heatmap.currentMonth` | Must match one entry in `months` | If no match, no column is highlighted |
| `heatmap.rows` | 0-4 items | Missing status rows render as empty |
| `items` dictionary | Keys should match month names in `months` | Missing keys render a dash in the cell |

### Storage

- **Location:** `wwwroot/data/data.json` (relative to project root)
- **Format:** UTF-8 JSON, no BOM
- **Size:** Expected < 5KB for typical use (3-5 tracks, 4-6 months, ~30 work items)
- **Versioning:** The file is intended to be checked into source control alongside the application
- **No database, no migration, no schema versioning** — the C# model IS the schema definition

---

## API Contracts

This application exposes **no REST API, no GraphQL, and no external endpoints**. The only "contract" is between the `data.json` file and the `DashboardDataService`.

### Internal Contract: `data.json` → `DashboardDataService`

**Input:** JSON file conforming to the `DashboardData` schema (defined above).

**Deserialization:**

```csharp
var options = new JsonSerializerOptions
{
    PropertyNameHandling = JsonNamingPolicy.CamelCase,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
};
var data = JsonSerializer.Deserialize<DashboardData>(json, options);
```

**Error responses:**

| Condition | Service State | Dashboard Behavior |
|-----------|---------------|-------------------|
| File not found | `Data = null`, `ErrorMessage` set | Renders: "No data file found. Place a data.json file in wwwroot/data/ and restart." |
| JSON parse error | `Data = null` (startup) or previous data (reload), `ErrorMessage` set | Startup: renders error panel. Reload: retains last valid view. |
| Valid JSON, missing fields | `Data` populated with defaults (empty strings, empty lists) | Dashboard renders with blank sections — no crash |

### Blazor SignalR Connection

Blazor Server uses a persistent SignalR WebSocket connection between the browser and the server. This is **not a custom API** — it is the Blazor framework's internal transport for UI diffs.

- **Endpoint:** `/_blazor` (auto-configured, not customized)
- **Protocol:** WebSocket over `ws://localhost:5000/_blazor`
- **Behavior:** Automatic reconnection with exponential backoff (Blazor default)
- **Relevance:** Single-user local use only; the SignalR connection is invisible to the user

### Static File Serving

| Path | Content | Notes |
|------|---------|-------|
| `/css/app.css` | Dashboard stylesheet | Served by `UseStaticFiles()` |
| `/data/data.json` | Dashboard data | Served as static file but also read directly by the service |
| `/_framework/blazor.web.js` | Blazor runtime | Auto-served by the framework |

---

## Infrastructure Requirements

### Runtime Requirements

| Requirement | Specification |
|-------------|--------------|
| **.NET SDK** | .NET 8.0 LTS (8.0.x) |
| **OS** | Windows 10/11 (primary); macOS/Linux functional with Arial font fallback |
| **Browser** | Microsoft Edge or Google Chrome (for viewing/screenshots) |
| **Port** | `http://localhost:5000` (configurable via `appsettings.json` or `--urls` flag) |
| **Disk** | < 10 MB total (published application + data file) |
| **Memory** | < 100 MB RSS at runtime |

### Hosting

- **Local Kestrel only** — no IIS, no reverse proxy, no container
- `dotnet run` from the project directory starts the application
- `dotnet publish -c Release` produces a deployable folder
- `dotnet publish -c Release --self-contained -r win-x64` for machines without .NET runtime

### Networking

- **Binds to `localhost` only** — not accessible from the network
- **No outbound connections** — no telemetry, no NuGet restore at runtime, no API calls
- **No TLS** — HTTP only; acceptable for localhost single-user use

### Storage

- **No database** — all data in a single JSON file
- **No temp files** — no caching layer, no log files (console output only)
- **File system access:** Read-only access to `wwwroot/data/data.json`; FileSystemWatcher requires read permissions on the directory

### CI/CD

- **Not required** — this is an internal productivity tool
- **If desired:** `dotnet build` → `dotnet test` (if tests exist) → `dotnet publish` is the complete pipeline
- **No Docker, no Kubernetes, no Azure Pipelines, no GitHub Actions** needed

### Build Configuration

**`ReportingDashboard.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**Critical:** Zero `<PackageReference>` entries. The entire application uses only the .NET 8 SDK built-in libraries.

---

## Technology Stack Decisions

| Layer | Choice | Justification |
|-------|--------|---------------|
| **Runtime** | .NET 8.0 LTS | Mandated by requirements. LTS support through Nov 2026. |
| **UI Framework** | Blazor Server (Interactive SSR) | Mandated. Server-side rendering means instant page load — ideal for screenshot capture. SignalR overhead is negligible for single-user local use. |
| **CSS Layout** | Native CSS Grid + Flexbox | The original HTML design uses these exact patterns. Zero abstraction overhead. ~100 lines of CSS total — no framework justified. |
| **SVG Rendering** | Inline SVG in Razor markup | The timeline is a simple set of lines, circles, diamonds, and text. Blazor renders SVG natively. A charting library (Radzen, MudBlazor, ApexCharts) would add 500KB+ of dependencies for less control over pixel-perfect output. |
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8. Reflection-based `Deserialize<T>()` is sufficient for a ~5KB file. Source generators are available but unnecessary for this scale. |
| **File Watching** | `System.IO.FileSystemWatcher` | Built into .NET. Provides the hot-reload capability for `data.json` without polling. Known quirks (duplicate events) are handled with a debounce timer. |
| **Styling** | Global `app.css` (not scoped `.razor.css`) | The design has one page with deeply nested elements. Scoped CSS would require `::deep` combinators for child components, adding complexity. A single `app.css` file ported from the HTML reference is simpler and matches the source material directly. |
| **Font** | Segoe UI (system font) | Windows system font — zero download, zero latency. Arial fallback for non-Windows systems. |
| **Testing (Phase 3)** | xUnit + bUnit | Standard .NET testing stack. bUnit enables in-memory rendering of Blazor components without a browser. Listed as Phase 3 — not required for MVP. |
| **Database** | None | A single JSON file with ~30 items does not warrant a database. The "admin interface" is a text editor. |
| **Authentication** | None | Localhost-only, single-user, no sensitive data. Auth would be pure overhead. |
| **JavaScript** | None | Blazor Server handles all rendering. No JS interop needed or permitted. |
| **CSS Framework** | None | Bootstrap (150KB+), Tailwind (build pipeline), or MudBlazor (component library) are all overkill for one page with ~100 lines of CSS. |

### Decisions Explicitly Rejected

| Option | Why Rejected |
|--------|-------------|
| Blazor WebAssembly | Slower initial load (downloads .NET runtime to browser). SSR is faster for screenshot-first use case. |
| Blazor Static SSR | No SignalR = no hot-reload push. Would require manual browser refresh after data changes. |
| SQLite / LiteDB | Adds a package dependency and query layer for a single flat file. Over-engineering. |
| Radzen.Blazor / MudBlazor charts | Less pixel control than hand-crafted SVG. Adds NuGet dependency. The timeline is simple enough to render directly. |
| `IOptions<T>` pattern | Adds DI complexity for a single config file. Direct file reading in a singleton service is simpler and equivalent. |
| Clean Architecture / CQRS | Three layers of abstraction for one page reading one file. The value of this project IS its simplicity. |

---

## Security Considerations

### Threat Model

This application has a minimal attack surface:

| Aspect | Assessment |
|--------|-----------|
| **Network exposure** | Localhost only (`http://localhost:5000`). Not reachable from the network. |
| **Authentication** | None required. Single user on a local machine. |
| **Data sensitivity** | Project status information only. No PII, no credentials, no financial data. |
| **Input validation** | The only input is `data.json`, written by the same user running the app. |
| **Outbound connections** | Zero. No telemetry, no package restore at runtime, no API calls. |

### Data Protection

- **`data.json` is local only** — never transmitted over a network
- **No encryption at rest** — the data (project status items) is not sensitive enough to warrant it
- **No secrets in `data.json`** — the schema contains titles, dates, and work item descriptions only
- **`backlogUrl`** may contain an internal ADO URL — this is acceptable as it's only rendered as an `<a href>` in the user's own browser

### Input Validation

Although `data.json` is authored by a trusted local user, the service validates defensively:

- **JSON parsing errors** are caught and logged; they never crash the application
- **Missing fields** default to empty strings/lists via C# property initializers — no `NullReferenceException`
- **`backlogUrl`** is rendered as an `<a href>` — Blazor's Razor engine HTML-encodes attribute values by default, preventing injection
- **Work item text** is rendered via Blazor's `@item` syntax, which HTML-encodes output by default — no XSS vector

### Kestrel Configuration

```csharp
// Program.cs - bind to localhost only
builder.WebHost.UseUrls("http://localhost:5000");
```

If the application must be shared on a LAN (future scope), change to `http://0.0.0.0:5000` — but this is explicitly out of scope.

### Dependency Supply Chain

- **Zero third-party NuGet packages** — eliminates supply chain risk entirely
- **Only `Microsoft.AspNetCore.App` shared framework** — maintained by Microsoft, included with the .NET 8 SDK
- **No npm, no webpack, no frontend build tools** — the CSS is hand-authored

---

## Scaling Strategy

### Current Scale

This application is designed for **exactly one user on one machine**. It does not need to scale.

| Dimension | Current | Maximum Supported |
|-----------|---------|-------------------|
| Concurrent users | 1 | 1 (Blazor Server holds per-connection state) |
| Data file size | ~3-5 KB | ~50 KB (larger files still parse in < 100ms) |
| Timeline tracks | 3 | 5 (beyond 5, SVG vertical spacing becomes cramped) |
| Heatmap months | 4 | 6 (beyond 6, column widths become too narrow for text) |
| Work items per cell | 3-5 | 8 (beyond 8, text overflows the cell at 1080p) |
| Projects | 1 per instance | 1 (by design; clone the repo for additional projects) |

### If Scaling Were Ever Needed

These are **not planned** — documented only for completeness.

| Scenario | Approach |
|----------|----------|
| Multiple projects | Clone the repository per project, each with its own `data.json`. No code changes needed. |
| Multiple users viewing | Blazor Server supports multiple SignalR connections. Current architecture works for ~10 concurrent viewers without modification. |
| Larger data sets | Replace `data.json` with a SQLite database and `IDbConnection`. This is a 2-hour refactor of `DashboardDataService` — the component layer is unaffected. |
| Automated screenshots | Add Playwright or Puppeteer as a post-build step. Out of scope but architecturally trivial. |

### Performance Budgets

| Operation | Budget | Rationale |
|-----------|--------|-----------|
| `data.json` deserialization | < 10ms | ~5KB file; `System.Text.Json` benchmarks at 1GB/s for simple schemas |
| Blazor component render | < 50ms | ~50 DOM elements total; Blazor's diff algorithm is optimized for small trees |
| Full page load (cold) | < 1 second | Kestrel serves the page locally; no network latency |
| Hot-reload round trip | < 500ms | FileSystemWatcher → deserialize → SignalR diff → DOM update |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component with its CSS layout strategy, data bindings, and interactions.

### Visual Section → Component Mapping

#### Section 1: Header Bar (`Dashboard.razor` — inline, not a separate component)

**Visual reference:** `.hdr` div in `OriginalDesignConcept.html`

**CSS layout:** `display: flex; justify-content: space-between; align-items: center; padding: 12px 44px 10px; border-bottom: 1px solid #E0E0E0;`

**Data bindings:**
- `@Data.Title` → `<h1>` text (24px, bold)
- `@Data.BacklogUrl` → `<a href="@Data.BacklogUrl" target="_blank">` (conditionally rendered if non-empty)
- `@Data.Subtitle` → `.sub` div (12px, #888)

**Legend:** Static markup with four inline `<span>` elements (gold diamond, green diamond, gray circle, red bar). No data binding — these are fixed legend entries.

**Interactions:** Clicking the ADO Backlog link opens the URL in a new tab (`target="_blank"`).

#### Section 2: Timeline Sidebar (rendered within `Dashboard.razor`)

**Visual reference:** 230px left panel inside `.tl-area`

**CSS layout:** `width: 230px; flex-shrink: 0; display: flex; flex-direction: column; justify-content: space-around; padding: 16px 12px 16px 0; border-right: 1px solid #E0E0E0;`

**Data bindings:**
```razor
@foreach (var track in Data.Timeline.Tracks)
{
    <div style="font-size:12px;font-weight:600;line-height:1.4;color:@track.Color;">
        @track.Id<br/>
        <span style="font-weight:400;color:#444;">@track.Label</span>
    </div>
}
```

**Interactions:** None — display only.

#### Section 3: SVG Timeline (`Timeline.razor`)

**Visual reference:** `.tl-svg-box` containing the `<svg>` element

**CSS layout:** `flex: 1; padding-left: 12px; padding-top: 6px;` — the SVG fills the remaining horizontal space.

**SVG dimensions:** `width` computed dynamically (default 1560px for the available space), `height="185"`, `overflow: visible`.

**Data bindings:**

| SVG Element | Data Source | Computation |
|-------------|-----------|-------------|
| Month grid lines | `Timeline.StartDate`, `Timeline.EndDate` | `DateToX(firstOfMonth)` for each month in range |
| Month labels | Derived from date range | Three-letter abbreviation (Jan, Feb, ...) |
| Track lines | `Timeline.Tracks[i]` | `y = 28 + (i * 150 / trackCount)`, full width, `stroke=track.Color` |
| Checkpoint circles | `milestone.Type == "checkpoint"` | `cx=DateToX(date)`, `cy=trackY`, `r=5-7`, white fill, track color stroke |
| PoC diamonds | `milestone.Type == "poc"` | `cx=DateToX(date)`, `cy=trackY`, gold fill `#F4B400`, drop shadow |
| Production diamonds | `milestone.Type == "production"` | `cx=DateToX(date)`, `cy=trackY`, green fill `#34A853`, drop shadow |
| Milestone labels | `milestone.Label` | `<text>` at `x=DateToX(date)`, alternating above/below track |
| NOW line | `Data.CurrentDate` | `x=DateToX(currentDate)`, red dashed vertical |

**Interactions:** Native SVG `<title>` tooltip on hover over milestone shapes. No JavaScript.

#### Section 4: Heatmap Title (rendered within `Heatmap.razor`)

**Visual reference:** `.hm-title`

**CSS layout:** `font-size: 14px; font-weight: 700; color: #888; letter-spacing: 0.5px; text-transform: uppercase; margin-bottom: 8px;`

**Content:** Static text: `"MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS"`

#### Section 5: Heatmap Grid (`Heatmap.razor`)

**Visual reference:** `.hm-grid`

**CSS layout:** `display: grid; grid-template-columns: 160px repeat(@monthCount, 1fr); grid-template-rows: 36px repeat(4, 1fr); border: 1px solid #E0E0E0; flex: 1; min-height: 0;`

The `@monthCount` is dynamically bound from `Data.Heatmap.Months.Count`.

**Data bindings:**

| Grid Element | Data Source | CSS Class |
|-------------|-----------|-----------|
| Corner cell | Static "STATUS" text | `.hm-corner` |
| Month headers | `@foreach month in Data.Heatmap.Months` | `.hm-col-hdr` + `.current-month-hdr` if `month == CurrentMonth` |
| Row headers | Hardcoded status labels (SHIPPED, IN PROGRESS, CARRYOVER, BLOCKERS) | `.hm-row-hdr` + status-specific class (`.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr`) |
| Data cells | `HeatmapCell` component for each (status × month) | Status-specific class + current-month modifier |

**Interactions:** None — display only.

#### Section 6: Heatmap Data Cells (`HeatmapCell.razor`)

**Visual reference:** `.hm-cell` with `.it` items

**CSS layout:** `padding: 8px 12px; border-right: 1px solid #E0E0E0; border-bottom: 1px solid #E0E0E0; overflow: hidden;`

**Data bindings:**
```razor
@if (Items is null || Items.Count == 0)
{
    <span style="color:#999">—</span>
}
else
{
    @foreach (var item in Items)
    {
        <div class="it">@item</div>
    }
}
```

The `.it::before` pseudo-element provides the colored bullet dot — this is pure CSS, no data binding needed. The bullet color is determined by the parent status row's CSS class (`.ship-cell .it::before { background: #34A853; }`).

**Interactions:** None — display only.

---

## Risks & Mitigations

| # | Risk | Severity | Likelihood | Impact | Mitigation |
|---|------|----------|-----------|--------|------------|
| 1 | **SVG milestone positions don't match the HTML reference** | Medium | Medium | Screenshots look "off" compared to the design | Build and test the `DateToX()` function with the exact dates from the HTML reference first. Compare coordinates against the hardcoded SVG values in `OriginalDesignConcept.html`. The reference uses `x=104` for Jan 12 on a 0-1560 scale with Jan 1–Jun 30 range — verify this maps correctly. |
| 2 | **CSS Grid column widths differ from the HTML reference** | Medium | Low | Heatmap cells are wider/narrower than expected | Use identical CSS: `grid-template-columns: 160px repeat(N, 1fr)`. Test with N=4 (matches reference) first, then validate N=3 and N=6. |
| 3 | **FileSystemWatcher fires duplicate events** | Low | High | Data reloads twice in quick succession | Debounce with a 100ms `System.Threading.Timer`. Only the last event in the window triggers a reload. This is a well-known FSW behavior on Windows. |
| 4 | **Blazor Server SignalR disconnects during idle** | Low | Low | Dashboard shows "reconnecting" overlay | Acceptable for local use. The default Blazor reconnection logic handles this automatically. If it becomes annoying, increase the `CircuitOptions.DisconnectedCircuitRetentionPeriod` in `Program.cs`. |
| 5 | **Font rendering differs between Edge and Chrome** | Low | Low | Screenshots from different browsers look slightly different | Standardize on a single browser (Edge recommended for Windows). Document this in the README. Segoe UI renders consistently across both Chromium-based browsers. |
| 6 | **data.json schema changes break existing files** | Low | Medium | Users update the app but not their data file | The C# model uses default property values. Missing fields default to empty strings/lists — the app won't crash. Document breaking changes in release notes. |
| 7 | **Over-engineering by future contributors** | Medium | Medium | Adds databases, auth, or DI layers that contradict the simplicity goal | Document the architecture principle: "This project's value is its simplicity." Add a comment in `Program.cs`: "Intentionally minimal — see Architecture.md." |
| 8 | **Work item text overflows heatmap cells** | Low | Medium | Long text wraps or clips, breaking the 1080p layout | CSS `overflow: hidden` prevents layout breakage. Document the 40-character guideline for item descriptions in the data.json schema comments. |
| 9 | **`data.json` file locked by text editor during write** | Low | Medium | FileSystemWatcher or read fails with IOException | Wrap file reads in a retry loop (3 attempts, 50ms delay). Most editors release locks within milliseconds of saving. |
| 10 | **Milestone labels overlap on the SVG timeline** | Low | Medium | Dates too close together produce unreadable text | Alternate label placement above/below the track line (odd index above, even index below). For milestones within 30px of each other, offset the label further. This is a rendering concern, not an architecture concern — handle in `Timeline.razor`. |