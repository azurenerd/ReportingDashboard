# Architecture

## Overview & Goals

This document defines the complete system architecture for the **Executive Reporting Dashboard**, a single-page Blazor Server (.NET 8) application that renders project milestone timelines and execution heatmaps from a local `data.json` file, optimized for 1920×1080 screenshot capture destined for PowerPoint executive decks.

### Architecture Goals

1. **Pixel-perfect rendering** — Match the canonical `OriginalDesignConcept.html` reference design exactly at 1920×1080
2. **Data-driven content** — All dashboard content is defined in a single `data.json` file; zero code changes needed to update reporting content
3. **Zero operational overhead** — No cloud services, no database, no authentication, no external dependencies; `dotnet run` and go
4. **Minimal codebase** — ≤ 15 files total; no unnecessary abstractions, layers, or patterns
5. **Sub-second rendering** — Full dashboard renders in < 1 second on localhost with no visible loading state

### Architecture Pattern

**Minimal Blazor Server — Single Service, Component Tree, File Data Source.** This is not a multi-tier application. It is a single-process, single-user, read-only visualization tool. The architecture deliberately avoids patterns like repository layers, CQRS, MediatR, or domain-driven design — all of which would be over-engineering for this scope.

```
┌─────────────────────────────────────────────────┐
│                   Browser (Edge)                │
│              1920×1080 viewport                 │
└──────────────────────┬──────────────────────────┘
                       │ SignalR (Blazor Server)
┌──────────────────────▼──────────────────────────┐
│              Kestrel (localhost:5000)            │
│  ┌────────────────────────────────────────────┐ │
│  │         Blazor Component Tree              │ │
│  │  Dashboard → Header + Timeline + Heatmap   │ │
│  └──────────────────┬─────────────────────────┘ │
│  ┌──────────────────▼─────────────────────────┐ │
│  │       DashboardDataService (Singleton)     │ │
│  └──────────────────┬─────────────────────────┘ │
│  ┌──────────────────▼─────────────────────────┐ │
│  │          Data/data.json (file system)      │ │
│  └────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘
```

---

## System Components

### 1. Solution Structure

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj      # net8.0, no external NuGet packages
    ├── Program.cs                      # Minimal hosting, DI registration
    ├── Data/
    │   ├── data.json                   # User-editable data source
    │   └── DashboardData.cs            # C# POCO models (all model classes)
    ├── Services/
    │   └── DashboardDataService.cs     # Singleton: loads, caches, watches data.json
    ├── Components/
    │   ├── App.razor                   # Root component (<html>, <head>, <body>)
    │   ├── Routes.razor                # Router configuration
    │   ├── Layout/
    │   │   └── MainLayout.razor        # Full-page layout (no nav, no sidebar)
    │   ├── Pages/
    │   │   └── Dashboard.razor         # Single page route "/"
    │   └── Shared/
    │       ├── Header.razor            # Project title, subtitle, legend
    │       ├── Timeline.razor          # SVG milestone visualization
    │       ├── HeatmapGrid.razor       # CSS Grid status matrix
    │       └── HeatmapCell.razor       # Individual cell with bullet items
    └── wwwroot/
        └── css/
            └── app.css                 # All styles ported from reference design
```

**File count: 14 files** (within the ≤ 15 file target).

---

### 2. Component: `Program.cs` — Application Host

**Responsibility:** Configure Kestrel, register DI services, set up the Blazor Server pipeline.

**Interface:** N/A (entry point)

**Dependencies:** `DashboardDataService`, ASP.NET Core framework

**Key Behaviors:**
- Binds Kestrel to `http://localhost:{port}` (port from `appsettings.json`, default `5000`)
- Binds to `127.0.0.1` only — not accessible from other machines
- Registers `DashboardDataService` as a singleton in the DI container
- Configures Blazor Server with `MapRazorComponents<App>().AddInteractiveServerRenderMode()`
- Disables HTTPS redirection (HTTP only)
- Reads `DataFilePath` from `appsettings.json` configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(
    $"http://localhost:{builder.Configuration.GetValue<int>("Port", 5000)}");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<DashboardDataService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var dataPath = config.GetValue<string>("DataFilePath")
        ?? Path.Combine(env.ContentRootPath, "Data", "data.json");
    return new DashboardDataService(dataPath);
});

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
```

---

### 3. Component: `DashboardDataService` — Data Access Singleton

**Responsibility:** Load `data.json` from disk, deserialize into `DashboardData`, cache in memory, optionally watch for file changes.

**Interface:**

```csharp
public class DashboardDataService : IDisposable
{
    // Current loaded data (null if load failed)
    public DashboardData? Data { get; }

    // Error message if loading failed (null if successful)
    public string? ErrorMessage { get; }

    // Whether data loaded successfully
    public bool HasData { get; }

    // Event fired when data.json changes on disk (Phase 2)
    public event Action? OnDataChanged;

    // Force reload from disk
    public void Reload();
}
```

**Dependencies:** `System.Text.Json`, `System.IO.FileSystemWatcher`

**Key Behaviors:**
- Loads and deserializes `data.json` in the constructor (fail-safe: captures exception message)
- Uses `System.Text.Json` with `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`
- Caches the deserialized `DashboardData` object in memory (singleton lifetime)
- If the file is missing: sets `ErrorMessage = "Dashboard data not found. Please ensure data.json exists in the Data/ directory."`
- If the file has invalid JSON: sets `ErrorMessage = "Error reading data.json: {exception.Message}. Please fix the JSON syntax and refresh."`
- Phase 2: `FileSystemWatcher` on the `data.json` file triggers `Reload()` and fires `OnDataChanged` event
- Phase 2: Components subscribe to `OnDataChanged` and call `InvokeAsync(StateHasChanged)`

**Deserialization options:**

```csharp
private static readonly JsonSerializerOptions JsonOptions = new()
{
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
};
```

The `AllowTrailingCommas` and `ReadCommentHandling.Skip` options provide resilience against common JSON editing mistakes without requiring a non-standard JSON parser.

---

### 4. Component: `App.razor` — Root Component

**Responsibility:** Define the HTML document shell (`<html>`, `<head>`, `<body>` tags), reference the stylesheet, suppress default Blazor chrome.

**Key Behaviors:**
- Sets `<meta charset="UTF-8">`
- References `css/app.css` via `<link>`
- Does NOT include any default Blazor CSS (`blazor-error-ui`, loading indicators, etc.)
- Uses `<HeadOutlet>` and `<Routes>` Blazor components

---

### 5. Component: `MainLayout.razor` — Page Layout

**Responsibility:** Provide a full-page layout with zero navigation chrome.

**Key Behaviors:**
- Renders only `@Body` — no `<NavMenu>`, no sidebar, no top bar
- No `<div class="page">` wrapper from the default template
- The body content fills the full 1920×1080 viewport

```razor
@inherits LayoutComponentBase
@Body
```

---

### 6. Component: `Dashboard.razor` — Page Orchestrator

**Responsibility:** The single routable page (`@page "/"`). Injects `DashboardDataService`, handles error states, and composes the three visual sections.

**Interface (Parameters):** None — this is a top-level page.

**Dependencies:** `DashboardDataService` (injected)

**Key Behaviors:**
- If `DashboardDataService.HasData == false`: renders the error state UI (centered message with gray border)
- If data is available: renders `<Header>`, `<Timeline>`, `<HeatmapGrid>` in a vertical flex column
- Passes data subsets to each child component via Blazor `[Parameter]` properties
- The outer container has `style="width:1920px;height:1080px;overflow:hidden;display:flex;flex-direction:column;"`

```razor
@page "/"
@inject DashboardDataService DataService

@if (!DataService.HasData)
{
    <div class="error-container">
        <p>@DataService.ErrorMessage</p>
    </div>
}
else
{
    <div class="dashboard">
        <Header Project="DataService.Data!.Project" />
        <Timeline Tracks="DataService.Data!.Timeline.Tracks"
                  StartDate="DataService.Data!.Timeline.StartDate"
                  EndDate="DataService.Data!.Timeline.EndDate"
                  NowDate="DataService.Data!.Timeline.NowDate" />
        <HeatmapGrid Heatmap="DataService.Data!.Heatmap" />
    </div>
}
```

---

### 7. Component: `Header.razor` — Project Header Band

**Responsibility:** Render the top header band with project title, ADO backlog link, subtitle, and legend indicators.

**Interface:**

```csharp
[Parameter] public required ProjectInfo Project { get; set; }
```

**Visual Reference:** `.hdr` section of `OriginalDesignConcept.html`

**Key Behaviors:**
- Left group: `<h1>` with project title + inline `<a href="{BacklogUrl}" target="_blank">` link
- Left group: `<div class="sub">` with subtitle text (org · workstream · month)
- Right group: Four legend items rendered as inline flex spans with CSS-styled indicator shapes
  - PoC Milestone: 12×12px amber diamond (CSS `transform: rotate(45deg)`)
  - Production Release: 12×12px green diamond
  - Checkpoint: 8×8px gray circle
  - Now line: 2×14px red vertical bar
- All text content comes from the `Project` parameter — no hardcoded strings

---

### 8. Component: `Timeline.razor` — SVG Milestone Visualization

**Responsibility:** Render the horizontal timeline SVG with track lines, milestones, month gridlines, and the NOW indicator.

**Interface:**

```csharp
[Parameter] public required List<TimelineTrack> Tracks { get; set; }
[Parameter] public required DateTime StartDate { get; set; }
[Parameter] public required DateTime EndDate { get; set; }
[Parameter] public DateTime? NowDate { get; set; }  // null = use DateTime.Now
```

**Visual Reference:** `.tl-area` section of `OriginalDesignConcept.html`

**Key Behaviors:**
- Left label column (230px): renders track labels with ID and name, color-coded per track
- Right SVG area: generates `<svg width="1560" height="185">` with all elements
- **Date-to-pixel calculation:** `xPos = (date - StartDate).TotalDays / (EndDate - StartDate).TotalDays * 1560.0`
- **Track Y positions:** Evenly distributed: `yPos = 42 + (trackIndex * (185 - 42) / (trackCount - 1))` for ≥2 tracks; single track at y=98
- **Month gridlines:** Iterate from StartDate to EndDate by month, drawing vertical lines at calculated X positions
- **NOW line:** Dashed red vertical line at `DateToX(NowDate ?? DateTime.Now)`
- **Milestone shapes by type:**
  - `"start"` → `<circle>` with white fill, track-color stroke, r=7
  - `"checkpoint"` → `<circle>` with `fill="#999"`, r=4 (or outlined variant with r=5, stroke="#888")
  - `"poc"` → `<polygon>` diamond, `fill="#F4B400"`, with SVG drop shadow filter
  - `"production"` → `<polygon>` diamond, `fill="#34A853"`, with SVG drop shadow filter
- **Diamond polygon generation** (centered at cx, cy with radius 11):
  ```
  points="{cx},{cy-11} {cx+11},{cy} {cx},{cy+11} {cx-11},{cy}"
  ```
- **Date labels:** `<text>` elements positioned above (y - 16) or below (y + 24) milestone shapes, alternating to avoid overlap
- **SVG filter definition:** `<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>`

**Helper method:**

```csharp
@code {
    private const double SvgWidth = 1560.0;
    private const double SvgHeight = 185.0;

    private double DateToX(DateTime date)
    {
        var totalDays = (EndDate - StartDate).TotalDays;
        if (totalDays <= 0) return 0;
        var elapsed = (date - StartDate).TotalDays;
        return Math.Clamp(elapsed / totalDays * SvgWidth, 0, SvgWidth);
    }

    private double TrackY(int index, int count)
    {
        if (count <= 1) return SvgHeight / 2.0;
        var usableHeight = SvgHeight - 42 - 31; // top/bottom margin
        return 42 + (index * usableHeight / (count - 1));
    }

    private string DiamondPoints(double cx, double cy, double r = 11)
        => $"{cx},{cy - r} {cx + r},{cy} {cx},{cy + r} {cx - r},{cy}";
}
```

---

### 9. Component: `HeatmapGrid.razor` — CSS Grid Status Matrix

**Responsibility:** Render the heatmap section title and the CSS Grid containing header row, category rows, and data cells.

**Interface:**

```csharp
[Parameter] public required HeatmapData Heatmap { get; set; }
```

**Visual Reference:** `.hm-wrap` and `.hm-grid` sections of `OriginalDesignConcept.html`

**Key Behaviors:**
- Renders section title: "MONTHLY EXECUTION HEATMAP"
- Dynamically sets CSS Grid columns: `grid-template-columns: 160px repeat({Heatmap.Columns.Count}, 1fr)`
- Header row: corner cell ("STATUS") + month headers; current month gets `.current-month-hdr` class
- Four category rows (Shipped, In Progress, Carryover, Blockers): each with row header + N data cells
- Delegates individual cell rendering to `<HeatmapCell>` component
- Maps category names to CSS class prefixes: `"Shipped"→"ship"`, `"In Progress"→"prog"`, `"Carryover"→"carry"`, `"Blockers"→"block"`

---

### 10. Component: `HeatmapCell.razor` — Individual Data Cell

**Responsibility:** Render a single heatmap cell containing bulleted item text or an empty-state dash.

**Interface:**

```csharp
[Parameter] public required List<string> Items { get; set; }
[Parameter] public required string CssClass { get; set; }    // e.g., "ship-cell" or "ship-cell apr"
```

**Key Behaviors:**
- If `Items` is empty or null: renders a single gray dash `<div class="empty-cell">-</div>`
- If `Items` has entries: renders `<div class="it">` for each item text
- The `.it::before` CSS pseudo-element provides the 6px colored bullet — no inline styles needed, the bullet color is inherited from the row's CSS class

---

## Component Interactions

### Data Flow (Startup)

```
1. Program.cs starts Kestrel
2. DashboardDataService constructor:
   a. Reads Data/data.json from disk
   b. Deserializes via System.Text.Json → DashboardData
   c. On success: stores in Data property, HasData = true
   d. On failure: stores ErrorMessage, HasData = false
3. Browser navigates to http://localhost:5000
4. Blazor Server establishes SignalR connection
5. Dashboard.razor renders:
   a. Injects DashboardDataService
   b. Checks HasData
   c. If true: passes data subsets to child components
   d. If false: renders error message
6. Child components render synchronously (no async loading)
7. Full page delivered in single render pass
```

### Data Flow (File Watch — Phase 2)

```
1. User edits and saves data.json
2. FileSystemWatcher detects change
3. DashboardDataService.Reload() re-reads and re-deserializes
4. DashboardDataService fires OnDataChanged event
5. Dashboard.razor's event handler calls InvokeAsync(StateHasChanged)
6. Blazor re-renders the component tree via SignalR
7. Browser updates without full page reload
```

### Component Parameter Flow

```
DashboardDataService.Data (DashboardData)
├─→ Dashboard.razor
│   ├─→ Header.razor         receives: ProjectInfo
│   ├─→ Timeline.razor        receives: List<TimelineTrack>, StartDate, EndDate, NowDate
│   └─→ HeatmapGrid.razor     receives: HeatmapData
│       └─→ HeatmapCell.razor  receives: List<string> Items, string CssClass
```

All data flows **top-down only** via Blazor `[Parameter]` properties. There are no callbacks, no EventCallbacks, no two-way binding, and no shared state beyond the singleton service.

---

## Data Model

### Entity Diagram

```
DashboardData
├── ProjectInfo                    (1:1)
├── TimelineConfig                 (1:1)
│   └── List<TimelineTrack>        (1:N)
│       └── List<TrackMilestone>   (1:N per track)
└── HeatmapData                    (1:1)
    └── List<HeatmapRow>           (1:4, fixed categories)
        └── Dictionary<string, List<string>>  (month → items)
```

### C# Model Classes (`Data/DashboardData.cs`)

```csharp
using System.Text.Json.Serialization;

namespace ReportingDashboard.Data;

public class DashboardData
{
    public required ProjectInfo Project { get; set; }
    public required TimelineConfig Timeline { get; set; }
    public required HeatmapData Heatmap { get; set; }
}

public class ProjectInfo
{
    public required string Title { get; set; }
    public required string Subtitle { get; set; }
    public required string BacklogUrl { get; set; }
    public string? BacklogLinkText { get; set; }  // default: "→ ADO Backlog"
}

public class TimelineConfig
{
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public DateTime? NowDate { get; set; }          // null = use DateTime.Now
    public required List<TimelineTrack> Tracks { get; set; }
}

public class TimelineTrack
{
    public required string Id { get; set; }         // "M1", "M2", "M3"
    public required string Name { get; set; }       // "Chatbot & MS Role"
    public required string Color { get; set; }      // "#0078D4"
    public required List<TrackMilestone> Milestones { get; set; }
}

public class TrackMilestone
{
    public required DateTime Date { get; set; }
    public required string Type { get; set; }       // "start" | "checkpoint" | "poc" | "production"
    public string? Label { get; set; }              // "Mar 26 PoC" (null = no label)

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LabelPosition LabelPosition { get; set; } = LabelPosition.Above;
}

public enum LabelPosition
{
    Above,
    Below
}

public class HeatmapData
{
    public required List<string> Columns { get; set; }   // ["Jan", "Feb", "Mar", "Apr"]
    public required string CurrentColumn { get; set; }   // "Apr"
    public required List<HeatmapRow> Rows { get; set; }
}

public class HeatmapRow
{
    public required string Category { get; set; }  // "Shipped" | "In Progress" | "Carryover" | "Blockers"
    public required Dictionary<string, List<string>> Items { get; set; }
}
```

### Storage

**There is no database.** The single data source is `Data/data.json`, a UTF-8 JSON file on the local file system. The file is:
- Read at application startup
- Cached in memory for the lifetime of the process
- Optionally watched for changes via `FileSystemWatcher` (Phase 2)
- Never written to by the application

### Sample `data.json` Schema

```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "backlogLinkText": "→ ADO Backlog"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-07-01",
    "nowDate": null,
    "tracks": [
      {
        "id": "M1",
        "name": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "start", "label": "Jan 12", "labelPosition": "Above" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC", "labelPosition": "Below" },
          { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)", "labelPosition": "Above" }
        ]
      },
      {
        "id": "M2",
        "name": "PDS & Data Inventory",
        "color": "#00897B",
        "milestones": [
          { "date": "2025-12-19", "type": "start", "label": "Dec 19", "labelPosition": "Above" },
          { "date": "2026-02-11", "type": "checkpoint", "label": "Feb 11", "labelPosition": "Above" },
          { "date": "2026-03-05", "type": "checkpoint", "label": null },
          { "date": "2026-03-15", "type": "checkpoint", "label": null },
          { "date": "2026-03-20", "type": "checkpoint", "label": null },
          { "date": "2026-03-22", "type": "checkpoint", "label": null },
          { "date": "2026-03-25", "type": "poc", "label": "Mar 25 PoC", "labelPosition": "Above" },
          { "date": "2026-06-01", "type": "production", "label": "Jun Prod", "labelPosition": "Below" }
        ]
      },
      {
        "id": "M3",
        "name": "Auto Review DFD",
        "color": "#546E7A",
        "milestones": [
          { "date": "2026-02-01", "type": "start", "label": "Feb 1", "labelPosition": "Above" },
          { "date": "2026-04-15", "type": "poc", "label": "Apr 15 PoC", "labelPosition": "Below" },
          { "date": "2026-06-15", "type": "production", "label": "Jun Prod", "labelPosition": "Above" }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "currentColumn": "Apr",
    "rows": [
      {
        "category": "Shipped",
        "items": {
          "Jan": ["Privacy chatbot MVP", "Intake form v2"],
          "Feb": ["MS Role mapping engine", "PDS connector alpha"],
          "Mar": ["Data inventory scanner", "Auto-classify pipeline"],
          "Apr": ["Review workflow engine", "DFD template library"]
        }
      },
      {
        "category": "In Progress",
        "items": {
          "Jan": ["MS Role mapping engine", "PDS connector"],
          "Feb": ["Data inventory scanner", "Auto-classify pipeline"],
          "Mar": ["Review workflow engine", "DFD template library"],
          "Apr": ["Compliance report gen", "Role-based access MVP"]
        }
      },
      {
        "category": "Carryover",
        "items": {
          "Jan": [],
          "Feb": ["Intake form v2 (from Jan)"],
          "Mar": ["PDS connector beta (from Feb)"],
          "Apr": ["Auto-classify tuning"]
        }
      },
      {
        "category": "Blockers",
        "items": {
          "Jan": [],
          "Feb": ["Azure AD tenant config"],
          "Mar": ["Legal review on data schema"],
          "Apr": ["Prod deployment slot unavailable"]
        }
      }
    ]
  }
}
```

---

## API Contracts

**This application has no REST API, no Web API controllers, and no HTTP endpoints beyond the Blazor Server default.** It is a single-page rendered application, not a service.

### Blazor Server Endpoints (Framework-Provided)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/` | GET | Serves the Blazor Server HTML host page |
| `/_blazor` | WebSocket | SignalR connection for Blazor Server interactive rendering |
| `/_framework/*` | GET | Blazor framework static assets |
| `/css/app.css` | GET | Application stylesheet (static file) |

### Error Contract

The application handles errors at the data loading layer and renders them inline — there are no HTTP error responses to define.

| Error Condition | User-Visible Behavior |
|----------------|----------------------|
| `data.json` missing | Centered message: "Dashboard data not found. Please ensure data.json exists in the Data/ directory." |
| `data.json` invalid JSON | Centered message: "Error reading data.json: {parse error details}. Please fix the JSON syntax and refresh." |
| `data.json` missing required fields | Centered message with details about which fields are missing or null |
| Application crash | User re-runs `dotnet run` — no persistent state to corrupt |

### `data.json` Schema Contract

The `data.json` file serves as the **implicit API** between the program manager (data author) and the application. The schema contract is defined by the C# model classes in `DashboardData.cs` and enforced at deserialization time.

**Required top-level properties:**
- `project` (object) — must contain `title`, `subtitle`, `backlogUrl`
- `timeline` (object) — must contain `startDate`, `endDate`, `tracks` (array)
- `heatmap` (object) — must contain `columns` (array), `currentColumn` (string), `rows` (array)

**Validation rules (enforced in `DashboardDataService`):**
- `timeline.tracks` must have ≥ 1 entry
- `heatmap.columns` must have ≥ 1 entry
- `heatmap.currentColumn` must match one of the `columns` values
- `heatmap.rows` must have exactly 4 entries with categories: "Shipped", "In Progress", "Carryover", "Blockers"
- Each `TrackMilestone.Type` must be one of: `"start"`, `"checkpoint"`, `"poc"`, `"production"`

---

## Infrastructure Requirements

### Hosting

| Requirement | Specification |
|-------------|---------------|
| **Web server** | Kestrel (built into ASP.NET Core) |
| **Binding** | `http://127.0.0.1:5000` (localhost only) |
| **Protocol** | HTTP only — no TLS/HTTPS |
| **Process model** | Single process, foreground — started by user, stopped by Ctrl+C |
| **No reverse proxy** | No IIS, no Nginx, no load balancer |

### Networking

| Requirement | Specification |
|-------------|---------------|
| **Inbound** | localhost only — firewall blocks external access by default |
| **Outbound** | Zero network calls at runtime |
| **DNS** | Not applicable — hardcoded to `localhost` |

### Storage

| Requirement | Specification |
|-------------|---------------|
| **Data file** | `Data/data.json` — read-only access by the application, ≤ 100KB |
| **Static files** | `wwwroot/css/app.css` — served by Kestrel's static file middleware |
| **Disk usage** | < 10MB total (compiled application + data + static files) |
| **No database** | No SQLite, no LiteDB, no SQL Server, no embedded DB |

### Build & Development

| Requirement | Specification |
|-------------|---------------|
| **SDK** | .NET 8 SDK (`8.0.400+`) |
| **Build command** | `dotnet build` |
| **Run command (dev)** | `dotnet watch --project ReportingDashboard` |
| **Run command (prod)** | `dotnet run --project ReportingDashboard -c Release` |
| **Publish command** | `dotnet publish -c Release -o ./publish` |
| **Hot reload** | Built into `dotnet watch` — Razor and CSS changes apply automatically |

### CI/CD

**No CI/CD pipeline is required.** This is a local-only tool with manual build and run. If desired in the future:
- Build: `dotnet build -c Release`
- Test: `dotnet test` (if test project exists)
- Publish: `dotnet publish -c Release -o ./artifacts`
- No container registry, no deployment target, no release pipeline

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **UI Framework** | Blazor Server (.NET 8) | Mandated stack. Server-side rendering keeps all logic in C#. No JS interop needed for this use case. |
| **Layout Engine** | CSS Grid + Flexbox | Matches the reference design exactly. The heatmap is a natural CSS Grid, the page layout is Flexbox. No framework needed. |
| **Timeline Rendering** | Inline SVG via Razor | The timeline is lines, circles, diamonds, and text — basic SVG primitives. A charting library would add complexity without benefit. Direct SVG gives pixel-perfect control. |
| **JSON Parser** | `System.Text.Json` | Built into .NET 8. Zero additional dependencies. Sufficient for deserializing a < 100KB config file. |
| **CSS Architecture** | Single `app.css` with CSS custom properties | One file, no preprocessor (no SASS/LESS), no CSS-in-JS. Custom properties enable the color palette to be defined once and referenced throughout. |
| **Font** | Segoe UI (system font) | Pre-installed on all Windows 10/11 systems. No web font loading delay. Matches the reference design. |
| **Package Dependencies** | Zero external NuGet packages | The PM spec explicitly requires zero dependencies beyond the default Blazor Server template. `System.Text.Json`, `FileSystemWatcher`, and all ASP.NET Core components are framework-included. |
| **Component Library** | None (no MudBlazor, no Radzen) | Third-party component libraries impose their own CSS and design language, making pixel-perfect matching of the reference design harder. The reference design is achievable with plain HTML/CSS. |
| **JavaScript** | None | No JS interop, no npm packages, no bundler. All rendering is server-side Razor → HTML/SVG/CSS. |
| **Testing Framework** | xUnit + bUnit (optional) | Not required for MVP. If added, xUnit for model/service tests, bUnit for component rendering tests. No external test dependencies in the core project. |
| **File Watching** | `System.IO.FileSystemWatcher` | Built into .NET. Watches `data.json` for changes and triggers re-render. Phase 2 feature. |

### Technologies Explicitly Excluded

| Technology | Reason for Exclusion |
|-----------|---------------------|
| **Database (any)** | Single JSON file is the data source. No persistence layer needed. |
| **Authentication** | Local-only, single-user tool. No login, no tokens, no cookies. |
| **HTTPS** | No sensitive data, no public network. HTTP on localhost is sufficient. |
| **Docker** | Local-only tool. Container adds complexity with no benefit. |
| **Cloud services** | Explicitly out of scope. No Azure, no AWS, no external APIs. |
| **Bootstrap / Tailwind** | Would fight the pixel-perfect reference design CSS. Net negative. |
| **Chart.js / D3 / ApexCharts** | The timeline is simple SVG. A charting library would be heavier and less controllable. |
| **SignalR (custom hubs)** | Blazor Server's built-in circuit is sufficient. No custom hubs needed. |
| **Entity Framework** | No database to map to. |
| **MediatR / AutoMapper** | Over-engineering for a 14-file application reading one JSON file. |

---

## Security Considerations

### Threat Model

This application has an intentionally minimal threat surface:

| Property | Value |
|----------|-------|
| **Network exposure** | `127.0.0.1` only — not reachable from other machines |
| **Authentication** | None — intentional design choice for a local tool |
| **Data sensitivity** | Low — project names and status text only; no PII, no credentials |
| **Input sources** | Single file (`data.json`) edited by the same person running the app |
| **Output** | Read-only HTML rendering — no data mutations, no writes |

### Input Validation

The only external input is `data.json`. Validation strategy:

1. **JSON parsing** — `System.Text.Json` handles syntax validation; malformed JSON is caught and reported
2. **Schema validation** — After deserialization, `DashboardDataService` checks:
   - Required properties are non-null (`Project`, `Timeline`, `Heatmap`)
   - `Timeline.Tracks` has ≥ 1 entry
   - `Heatmap.Columns` has ≥ 1 entry
   - `Heatmap.CurrentColumn` exists in `Heatmap.Columns`
   - `TrackMilestone.Type` is one of the four allowed values
3. **Error reporting** — Validation failures produce user-friendly messages, never raw stack traces
4. **No HTML injection risk** — Blazor's Razor rendering engine HTML-encodes all string output by default. Even if `data.json` contains `<script>` tags in item text, they will be rendered as escaped text, not executed.

### Data Protection

| Concern | Approach |
|---------|----------|
| **Secrets in code** | None. No API keys, connection strings, or credentials exist in the codebase. |
| **Secrets in data** | `data.json` should not contain secrets. It holds project names and status text only. |
| **File system access** | The application reads one file (`data.json`) and serves static CSS. No write operations. |
| **Blazor SignalR circuit** | Default Blazor Server security applies. Circuit is local-only and single-user. |

### Hardening (Not Required, But Good Practice)

```csharp
// Program.cs — optional security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});
```

---

## Scaling Strategy

### Current Scale

| Dimension | Value |
|-----------|-------|
| **Users** | 1 (single program manager) |
| **Concurrent sessions** | 1 |
| **Data volume** | 1 JSON file, < 100KB |
| **Pages** | 1 |
| **Render frequency** | On-demand (monthly reporting cycles) |

**This application does not need to scale.** It is a local tool for one person. The architecture deliberately avoids scalability patterns (load balancing, caching layers, database sharding) because they add complexity with zero benefit.

### If Scaling Were Ever Needed

The component-based architecture supports these future expansions without architectural changes:

| Scenario | Approach |
|----------|----------|
| **Multiple projects** | Add a dropdown selector that loads different JSON files from a `Data/` directory. Each file renders a separate dashboard. No architecture change. |
| **Multiple users** | Deploy to a shared server, add authentication. The Blazor Server model already supports multiple concurrent SignalR circuits. |
| **Larger datasets** | The current model supports 10+ timeline tracks and 12+ heatmap months. CSS Grid and SVG scale to these sizes without performance issues. |
| **Automated generation** | Add a Puppeteer-Sharp console app that launches the dashboard, captures screenshots, and outputs PNGs. The dashboard itself is unchanged. |

### Performance Budget

| Metric | Budget | Justification |
|--------|--------|---------------|
| **Page load** | < 1 second | Single render pass, no async data loading, no external resources |
| **JSON deserialization** | < 50ms | `System.Text.Json` is highly optimized; file is < 100KB |
| **Memory** | < 100MB RSS | One Blazor circuit, one cached data object, minimal static assets |
| **Startup** | < 3 seconds | Kestrel starts in ~1s; data loading adds < 50ms |

---

## UI Component Architecture

This section maps each visual section from the `OriginalDesignConcept.html` design to specific Blazor components, CSS strategies, data bindings, and interactions.

### Component Map

| Visual Section | HTML Reference | Blazor Component | CSS Strategy | Data Binding | Interactions |
|---------------|---------------|-----------------|-------------|-------------|-------------|
| **Header band** | `.hdr` | `Header.razor` | Flexbox (`justify-content: space-between`) | `ProjectInfo` → title, subtitle, backlogUrl | ADO link opens in new tab (`target="_blank"`) |
| **Header legend** | `.hdr` right div | Inline in `Header.razor` | Flex row with `gap: 22px` | Legend items are static (not data-driven) | None — read-only indicators |
| **Timeline band** | `.tl-area` | `Timeline.razor` | Outer: Flexbox; Inner: SVG coordinate system | `TimelineConfig` → tracks, dates | None — read-only visualization |
| **Track labels** | `.tl-area` left column | Inline in `Timeline.razor` | Flex column, `justify-content: space-around`, 230px wide | `TimelineTrack.Id`, `.Name`, `.Color` | None |
| **SVG timeline** | `.tl-svg-box svg` | Inline SVG in `Timeline.razor` | SVG coordinate system (1560×185) | Track milestones, date-to-pixel math | None |
| **Heatmap wrapper** | `.hm-wrap` | `HeatmapGrid.razor` | Flex column, `flex: 1` | `HeatmapData` | None |
| **Heatmap title** | `.hm-title` | Inline in `HeatmapGrid.razor` | Static text, uppercase | Static string | None |
| **Heatmap grid** | `.hm-grid` | Inline in `HeatmapGrid.razor` | CSS Grid: `160px repeat(N, 1fr)` / `36px repeat(4, 1fr)` | Dynamic column count from `Heatmap.Columns` | None |
| **Corner cell** | `.hm-corner` | Inline in `HeatmapGrid.razor` | Grid cell at (1,1) | Static "STATUS" text | None |
| **Month headers** | `.hm-col-hdr` | `@foreach` in `HeatmapGrid.razor` | Grid cells in row 1; current month gets amber override | `Heatmap.Columns`, `Heatmap.CurrentColumn` | None |
| **Row headers** | `.hm-row-hdr` (`.ship-hdr`, etc.) | `@foreach` in `HeatmapGrid.razor` | Grid cells in column 1; category-specific colors | `HeatmapRow.Category` → CSS class mapping | None |
| **Data cells** | `.hm-cell` | `HeatmapCell.razor` | Grid cells; `padding: 8px 12px; overflow: hidden` | `HeatmapRow.Items[month]` → item list | None |
| **Cell items** | `.hm-cell .it` | `@foreach` in `HeatmapCell.razor` | Relative positioning; `::before` pseudo-element for bullet | `List<string>` items | None |
| **Empty cells** | `.hm-cell` (no items) | Conditional in `HeatmapCell.razor` | Gray dash centered | Empty/null item list | None |
| **Error state** | N/A (not in design) | Inline in `Dashboard.razor` | Centered flex container with gray border | `DashboardDataService.ErrorMessage` | None |

### CSS Class Mapping (Category → CSS Prefix)

```csharp
private static readonly Dictionary<string, string> CategoryCssMap = new()
{
    ["Shipped"]     = "ship",
    ["In Progress"] = "prog",
    ["Carryover"]   = "carry",
    ["Blockers"]    = "block"
};
```

This mapping drives dynamic CSS class assignment:
- Row header: `{prefix}-hdr` (e.g., `ship-hdr`)
- Data cell: `{prefix}-cell` (e.g., `ship-cell`)
- Current month cell: `{prefix}-cell current` (e.g., `ship-cell current`)

### CSS File Structure (`wwwroot/css/app.css`)

```css
/* 1. CSS Custom Properties — color tokens and layout constants */
:root { ... }

/* 2. Reset and base styles — matches reference * selector */
* { margin: 0; padding: 0; box-sizing: border-box; }
body { width: var(--page-width); height: var(--page-height); overflow: hidden; ... }

/* 3. Header styles — .hdr, .sub, legend indicators */

/* 4. Timeline styles — .tl-area, .tl-svg-box, track labels */

/* 5. Heatmap styles — .hm-wrap, .hm-title, .hm-grid */

/* 6. Heatmap header cells — .hm-corner, .hm-col-hdr, .current-month-hdr */

/* 7. Heatmap row headers — .ship-hdr, .prog-hdr, .carry-hdr, .block-hdr */

/* 8. Heatmap data cells — .ship-cell, .prog-cell, .carry-cell, .block-cell */
/*    + .current variants for deeper tint */

/* 9. Cell items — .it, .it::before (bullet indicators per category) */

/* 10. Error state styles — .error-container */

/* 11. Blazor overrides — suppress framework chrome */
.blazor-error-boundary { display: none; }
#blazor-error-ui { display: none; }
```

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|------------|
| 1 | **Blazor Server SignalR overhead** — unnecessary WebSocket connection for what is essentially a static page render | Certain (by design) | Low — invisible for single-user localhost use; < 5MB memory overhead | Accept as cost of mandated stack. The overhead is unmeasurable for this use case. |
| 2 | **Browser rendering differences** — CSS Grid and SVG may render slightly differently across Edge, Chrome, Firefox | Medium | Medium — screenshot may not match reference pixel-for-pixel in all browsers | Standardize on Edge Chromium for all screenshots. Test in Chrome as secondary. Document the target browser. |
| 3 | **`data.json` schema drift** — user edits JSON incorrectly (wrong field names, missing properties, incorrect types) | High (expected with manual editing) | Medium — dashboard shows error or renders incorrectly | Implement defensive deserialization with `try/catch`. Validate required fields post-deserialization. Show descriptive error messages with field-level detail. Ship well-commented sample `data.json`. |
| 4 | **SVG date-to-pixel math errors** — off-by-one or rounding issues in milestone positioning | Medium | Low — milestones slightly misplaced | Extract `DateToX()` as a pure function. Unit test with known date/pixel pairs from the reference design. Use `double` precision throughout. |
| 5 | **Heatmap cell overflow** — too many items in a single cell cause text to overflow and get clipped | Medium | Low — text is cut off by `overflow: hidden` | Design `data.json` guidance: recommend ≤ 5 items per cell. CSS `overflow: hidden` prevents visual breakage. Consider `font-size` reduction for cells with > 5 items (future enhancement). |
| 6 | **FileSystemWatcher reliability (Phase 2)** — `FileSystemWatcher` is known to fire duplicate events or miss events on some file systems | Medium | Low — user can always manually refresh the browser | Debounce file change events (300ms delay). Always support manual browser refresh as fallback. |
| 7 | **Blazor reconnection banner** — if the SignalR circuit drops (unlikely on localhost), Blazor shows a reconnection overlay | Low | Medium — overlay ruins screenshot | Suppress Blazor error/reconnection UI via CSS: `#blazor-error-ui { display: none; }`. On localhost, circuit drops are extremely rare. |
| 8 | **Port conflict** — another application already using port 5000 | Low | Low — app fails to start with clear error | Make port configurable via `appsettings.json`. Document how to change it. Kestrel's error message for port conflicts is already descriptive. |
| 9 | **Segoe UI font unavailable** — running on a non-Windows OS or stripped Windows install | Very Low | Medium — fallback to Arial changes text metrics and spacing | Font stack includes `Arial, sans-serif` as fallbacks. Document that Windows 10/11 is the target OS. Segoe UI is pre-installed on all standard Windows installations. |
| 10 | **Large `data.json` causing slow startup** — file grows beyond expected size | Very Low | Low — startup takes > 50ms | `System.Text.Json` handles files up to several MB efficiently. Set a soft warning at 100KB in documentation. The realistic maximum is ~20KB for a 12-month, 10-track dashboard. |

### Non-Risks (Explicitly Dismissed)

| Concern | Why It's Not a Risk |
|---------|-------------------|
| **Scalability** | Single user, single machine. Not applicable. |
| **High availability** | Run on demand, close when done. No uptime SLA. |
| **Data integrity** | JSON file is manually edited. No concurrent writes. No transactions needed. |
| **Performance under load** | One user, one page, one file. Sub-100ms render guaranteed. |
| **Security breaches** | Localhost-only, no network exposure, no sensitive data, no authentication to bypass. |
| **Dependency vulnerabilities** | Zero external dependencies. The only attack surface is the .NET 8 framework itself, updated via standard Windows Update / SDK updates. |