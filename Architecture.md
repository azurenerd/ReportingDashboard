# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server web application that renders a pixel-perfect 1920×1080 project status visualization optimized for PowerPoint screenshot capture. The system reads all content from a local `data.json` file and renders three visual sections—header, SVG timeline, and CSS Grid heatmap—with zero external dependencies, zero cloud services, and zero authentication.

**Architecture Pattern:** Read-only data visualization pipeline. `data.json` → deserialization → singleton service → Razor component tree → static HTML/CSS/SVG output.

**Primary Goals:**

1. **Pixel-perfect rendering** at exactly 1920×1080 matching `OriginalDesignConcept.html`
2. **Data-driven content** from a single `data.json` file with no code changes required
3. **Zero-dependency local execution** via `dotnet run` with the .NET 8 SDK only
4. **Sub-500ms first paint** with all content rendered server-side on initial request
5. **Maintainable component hierarchy** mapping 1:1 to the three visual sections of the design

**Non-Goals:** Authentication, databases, cloud deployment, responsive design, interactivity, real-time updates, multi-project support.

---

## System Components

### 1. `Program.cs` — Application Entry Point

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure Kestrel, register services, set up the Blazor middleware pipeline |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService`, ASP.NET Core middleware |
| **Data** | Reads `appsettings.json` for port configuration |

**Behavior:**
- Registers `DashboardDataService` as a singleton
- Calls `DashboardDataService.LoadAsync()` during startup (fail-fast on bad data)
- Configures Kestrel to listen on `localhost:5000` (configurable via `appsettings.json`)
- Maps Blazor Server endpoints with static SSR render mode (no SignalR circuit)
- Adds static file middleware for `wwwroot/` assets

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

// Fail-fast: load and validate data.json at startup
var dataService = app.Services.GetRequiredService<DashboardDataService>();
await dataService.LoadAsync(app.Environment.WebRootPath);

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>();
app.Run();
```

### 2. `DashboardDataService` — Data Loading & Validation

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Load `data.json` from disk, deserialize, validate required fields, expose immutable data |
| **Interfaces** | `DashboardData GetData()` — returns the loaded, validated data model |
| **Dependencies** | `System.Text.Json`, `System.IO`, `IWebHostEnvironment` |
| **Data** | Reads `wwwroot/data.json`, produces `DashboardData` record graph |
| **Lifetime** | Singleton — data loaded once at startup |

**Behavior:**
- Reads `wwwroot/data.json` using `File.ReadAllTextAsync()`
- Deserializes with `JsonSerializer.Deserialize<DashboardData>()` using `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`
- Validates all required fields are non-null (project.title, timeline.startDate, heatmap.columns, etc.)
- Throws `InvalidOperationException` with descriptive message on validation failure (caught by `Program.cs` for clean startup error)
- Exposes `DashboardData` via a read-only property after successful load

**Error Handling:**
- Missing file → `FileNotFoundException` with full path in message
- Invalid JSON → `JsonException` with parse error details
- Missing required field → `InvalidOperationException` naming the specific field
- All errors logged to console and prevent app startup (fail-fast)

```csharp
public class DashboardDataService
{
    private DashboardData? _data;

    public DashboardData Data => _data
        ?? throw new InvalidOperationException("Dashboard data not loaded. Call LoadAsync first.");

    public async Task LoadAsync(string webRootPath)
    {
        var path = Path.Combine(webRootPath, "data.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Dashboard data file not found: {path}");

        var json = await File.ReadAllTextAsync(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _data = JsonSerializer.Deserialize<DashboardData>(json, options)
            ?? throw new InvalidOperationException("data.json deserialized to null.");

        Validate(_data);
    }

    private static void Validate(DashboardData data)
    {
        if (string.IsNullOrWhiteSpace(data.Project?.Title))
            throw new InvalidOperationException("Required field missing: project.title");
        if (data.Timeline?.Tracks == null || data.Timeline.Tracks.Count == 0)
            throw new InvalidOperationException("Required field missing: timeline.tracks (must have at least 1 track)");
        if (data.Heatmap?.Columns == null || data.Heatmap.Columns.Count < 2)
            throw new InvalidOperationException("Required field missing: heatmap.columns (must have at least 2 columns)");
        // Additional validations for startDate, endDate, etc.
    }
}
```

### 3. `Dashboard.razor` — Page Orchestrator

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Top-level page component; injects data service and distributes data to child components |
| **Interfaces** | Blazor route `/` |
| **Dependencies** | `DashboardDataService`, child components: `DashboardHeader`, `TimelineArea`, `HeatmapGrid` |
| **Data** | Receives `DashboardData` from injected service, passes sub-models to children via `[Parameter]` |

**Behavior:**
- Renders the three-section vertical flex layout (`display: flex; flex-direction: column`)
- Passes `Data.Project` to `DashboardHeader`
- Passes `Data.Timeline` to `TimelineArea`
- Passes `Data.Heatmap` to `HeatmapGrid`
- Uses static SSR render mode (no `@rendermode` attribute = static by default in .NET 8)

### 4. `DashboardHeader.razor` — Header Section

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render project title, backlog link, subtitle, and milestone legend |
| **Interfaces** | `[Parameter] ProjectInfo Project` |
| **Dependencies** | None |
| **Data** | `ProjectInfo` record (title, subtitle, backlogUrl, currentMonth) |

**Markup Structure:**
```
div.hdr
├── div (left)
│   ├── h1 → Project.Title + <a href="Project.BacklogUrl">↗ ADO Backlog</a>
│   └── div.sub → Project.Subtitle
└── div (right) — static legend
    ├── PoC Milestone (gold diamond)
    ├── Production Release (green diamond)
    ├── Checkpoint (gray circle)
    └── Now indicator (red bar + "Now ({Project.CurrentMonth} 2026)")
```

### 5. `TimelineArea.razor` — Timeline Container

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the timeline section: track labels sidebar + SVG timeline |
| **Interfaces** | `[Parameter] TimelineData Timeline` |
| **Dependencies** | `TimelineSvg.razor` child component |
| **Data** | `TimelineData` record (startDate, endDate, nowDate, tracks with milestones) |

**Behavior:**
- Renders the 230px-wide track label sidebar with dynamic track count (1–5)
- Each track label shows `track.Id` in `track.Color` and `track.Label` in `#444`
- Passes timeline data to `TimelineSvg` child for SVG rendering
- Dynamically adjusts `.tl-area` height: base 196px for ≤3 tracks; adds 56px per additional track

### 6. `TimelineSvg.razor` — SVG Timeline Renderer

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the inline SVG with month grid, track lanes, milestone shapes, and NOW marker |
| **Interfaces** | `[Parameter] TimelineData Timeline` |
| **Dependencies** | None (pure computational rendering) |
| **Data** | Track list with milestones, date range, optional nowDate override |

**Key Algorithms:**

*Date-to-X Coordinate Mapping:*
```csharp
private double DateToX(DateTime date)
{
    var totalDays = (Timeline.EndDate - Timeline.StartDate).TotalDays;
    var elapsed = (date - Timeline.StartDate).TotalDays;
    return Math.Clamp(elapsed / totalDays * SvgWidth, 0, SvgWidth);
}
// SvgWidth = 1560px (constant matching reference design)
```

*Track Y-Position:*
```csharp
private double TrackY(int trackIndex) => 42 + (trackIndex * 56);
// Track 0 → y=42, Track 1 → y=98, Track 2 → y=154
```

*SVG Height:*
```csharp
private int SvgHeight => Timeline.Tracks.Count * 56 + 17;
// 3 tracks → 185px (matches reference)
```

*Month Grid Lines:*
```csharp
private double MonthSpacing => SvgWidth / MonthCount;
// 6 months → 260px intervals
```

**SVG Elements Generated:**
- `<defs>` with `<filter id="sh">` for drop shadow (feDropShadow dx=0 dy=1 stdDeviation=1.5)
- Month grid `<line>` elements at computed intervals + `<text>` month labels at y=14
- Per-track horizontal `<line>` at TrackY with stroke=track.Color, stroke-width=3
- Per-milestone shape based on `milestone.Type`:
  - `"checkpoint"` → `<circle>` r=7, fill=white, stroke=track.Color, stroke-width=2.5
  - `"checkpoint-small"` → `<circle>` r=4, fill=#999, no stroke, no label
  - `"poc"` → `<polygon>` diamond 22px, fill=#F4B400, filter=url(#sh)
  - `"production"` → `<polygon>` diamond 22px, fill=#34A853, filter=url(#sh)
- Per-milestone `<text>` label, alternating above/below track line (odd index above, even below)
- NOW line: `<line>` stroke=#EA4335, stroke-width=2, stroke-dasharray=5,3 + `<text>` "NOW"

*Diamond Polygon Points (for center cx, cy with radius r=11):*
```csharp
private string DiamondPoints(double cx, double cy, double r = 11)
    => $"{cx},{cy - r} {cx + r},{cy} {cx},{cy + r} {cx - r},{cy}";
```

### 7. `HeatmapGrid.razor` — Heatmap Section

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the CSS Grid heatmap with header row, 4 status rows, and N month columns |
| **Interfaces** | `[Parameter] HeatmapData Heatmap` |
| **Dependencies** | `HeatmapRow.razor`, `HeatmapCell.razor` child components |
| **Data** | `HeatmapData` record (columns, highlightColumn, shipped/inProgress/carryover/blockers) |

**Behavior:**
- Renders section title: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"
- Generates dynamic CSS Grid columns: `grid-template-columns: 160px repeat({Heatmap.Columns.Count}, 1fr)`
- Generates dynamic CSS Grid rows: `grid-template-rows: 36px repeat(4, 1fr)`
- Renders corner cell ("STATUS") + N column header cells (highlighting current month with `.highlight-hdr` class)
- Renders 4 `HeatmapRow` components, one per status category, each with its color configuration

### 8. `HeatmapRow.razor` — Single Status Row

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render one status row (e.g., Shipped) with header cell + N data cells |
| **Interfaces** | `[Parameter] string Label`, `[Parameter] string CssPrefix`, `[Parameter] HeatmapCategory Category`, `[Parameter] List<string> Columns`, `[Parameter] string HighlightColumn` |
| **Dependencies** | `HeatmapCell.razor` |
| **Data** | Category items dictionary, column list, highlight column name |

**Behavior:**
- Renders row header cell with `.{cssPrefix}-hdr` class and uppercase label
- For each column, renders a `HeatmapCell` with the items for that month
- Applies `.{cssPrefix}-cell` base class + highlight class when column matches `HighlightColumn`

### 9. `HeatmapCell.razor` — Single Data Cell

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render a single heatmap cell with bulleted work items or empty dash |
| **Interfaces** | `[Parameter] List<string>? Items`, `[Parameter] string CssClass` |
| **Dependencies** | None |
| **Data** | List of item strings (nullable — null/empty renders dash) |

**Behavior:**
- If items is null or empty: renders `<span style="color:#AAA">—</span>`
- Otherwise: renders each item as `<div class="it">{item text}</div>` (CSS `::before` pseudo-element provides the colored dot)

---

## Component Interactions

### Data Flow (Startup)

```
┌──────────────┐     ┌─────────────────────┐     ┌────────────────────┐
│  Program.cs  │────▶│ DashboardDataService │────▶│  wwwroot/data.json │
│  (startup)   │     │    .LoadAsync()      │◀────│  (filesystem read) │
└──────┬───────┘     └──────────┬──────────┘     └────────────────────┘
       │                        │
       │  Register Singleton    │  Deserialize + Validate
       │                        ▼
       │              ┌──────────────────┐
       │              │  DashboardData   │ (immutable record graph)
       │              └──────────────────┘
       ▼
  App starts listening on localhost:5000
```

### Data Flow (Request)

```
Browser GET /  ──▶  Blazor SSR Pipeline
                          │
                    ┌─────▼──────┐
                    │ Dashboard  │ ◀── injects DashboardDataService.Data
                    │  .razor    │
                    └─────┬──────┘
                          │ passes sub-models via [Parameter]
              ┌───────────┼───────────────┐
              ▼           ▼               ▼
     ┌────────────┐ ┌──────────┐  ┌────────────┐
     │ Dashboard  │ │ Timeline │  │  Heatmap   │
     │  Header    │ │   Area   │  │   Grid     │
     └────────────┘ └────┬─────┘  └──────┬─────┘
                         ▼               │
                  ┌────────────┐   ┌─────┼──────┐ (x4 rows)
                  │ TimelineSvg│   │ HeatmapRow │
                  │  (SVG gen) │   └─────┬──────┘
                  └────────────┘         │
                                   ┌─────┼──────┐ (x N cells)
                                   │ HeatmapCell│
                                   └────────────┘
                          │
                    ┌─────▼──────┐
                    │  Complete  │
                    │  HTML/CSS  │ ──▶ Browser renders 1920×1080
                    └────────────┘
```

### Communication Patterns

| Pattern | Implementation |
|---------|---------------|
| **Service → Component** | Constructor injection via `@inject DashboardDataService DataService` |
| **Parent → Child Component** | Blazor `[Parameter]` properties (unidirectional data flow) |
| **Error Propagation** | Exceptions thrown during `LoadAsync()` bubble up to `Program.cs` and halt startup with console error |
| **No Child → Parent** | No callbacks, no events, no two-way binding. Components are pure renderers. |
| **No Inter-Component** | No shared state between siblings. All data flows down from `Dashboard.razor`. |

---

## Data Model

### Entity Definitions

```csharp
// Models/DashboardData.cs

/// <summary>Root model deserialized from data.json.</summary>
public record DashboardData
{
    public required ProjectInfo Project { get; init; }
    public required TimelineData Timeline { get; init; }
    public required HeatmapData Heatmap { get; init; }
}

/// <summary>Header section: project identity and context.</summary>
public record ProjectInfo
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string BacklogUrl { get; init; }
    public required string CurrentMonth { get; init; }
}

/// <summary>Timeline section: date range, tracks, and milestones.</summary>
public record TimelineData
{
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public DateTime? NowDate { get; init; }  // Optional override; defaults to DateTime.Today
    public required List<TimelineTrack> Tracks { get; init; }
}

/// <summary>A single horizontal track in the timeline (e.g., M1, M2).</summary>
public record TimelineTrack
{
    public required string Id { get; init; }       // "M1", "M2", "M3"
    public required string Label { get; init; }    // "Chatbot & MS Role"
    public required string Color { get; init; }    // "#0078D4"
    public required List<Milestone> Milestones { get; init; }
}

/// <summary>A single milestone marker on a track.</summary>
public record Milestone
{
    public required DateTime Date { get; init; }
    public required string Type { get; init; }     // "checkpoint", "checkpoint-small", "poc", "production"
    public required string Label { get; init; }    // "Jan 12", "Mar 26 PoC", "" (empty for small dots)
    public string LabelPosition { get; init; } = "above";  // "above" or "below"
}

/// <summary>Heatmap section: month columns and four status categories.</summary>
public record HeatmapData
{
    public required List<string> Columns { get; init; }      // ["Jan", "Feb", "Mar", "Apr"]
    public required string HighlightColumn { get; init; }    // "Apr"
    public required HeatmapCategory Shipped { get; init; }
    public required HeatmapCategory InProgress { get; init; }
    public required HeatmapCategory Carryover { get; init; }
    public required HeatmapCategory Blockers { get; init; }
}

/// <summary>Items for one status category, keyed by month column name.</summary>
public record HeatmapCategory
{
    /// <summary>Month name → list of work item descriptions. Missing keys = empty cell.</summary>
    public required Dictionary<string, List<string>> Items { get; init; }
}
```

### Entity Relationships

```
DashboardData (root)
├── ProjectInfo (1:1) — header content
├── TimelineData (1:1) — timeline configuration
│   └── TimelineTrack (1:N, 1–5 tracks)
│       └── Milestone (1:N per track)
└── HeatmapData (1:1) — heatmap content
    ├── Columns: List<string> (2–6 month names)
    ├── Shipped: HeatmapCategory (1:1)
    ├── InProgress: HeatmapCategory (1:1)
    ├── Carryover: HeatmapCategory (1:1)
    └── Blockers: HeatmapCategory (1:1)
        └── Items: Dictionary<string, List<string>> (month → items)
```

### Storage

| Aspect | Detail |
|--------|--------|
| **Storage Type** | Flat JSON file on local filesystem |
| **Location** | `wwwroot/data.json` (served as static file but also read server-side at startup) |
| **Format** | JSON, UTF-8, camelCase property names |
| **Size** | Expected < 10KB typical, < 50KB max |
| **Schema Enforcement** | C# `required` properties + explicit validation in `DashboardDataService.Validate()` |
| **Versioning** | Users manually version by copying files (e.g., `data.2026-04.json`). No built-in versioning. |
| **Template** | `wwwroot/data.sample.json` committed to repo with example data and inline comments |
| **Gitignore** | `wwwroot/data.json` added to `.gitignore`; `data.sample.json` is committed |

### Canonical `data.json` Schema

```json
{
  "project": {
    "title": "Project Phoenix Release Roadmap",
    "subtitle": "Engineering Platform · Core Infrastructure · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "currentMonth": "Apr"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "nowDate": null,
    "tracks": [
      {
        "id": "M1",
        "label": "API Gateway & Auth",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12", "labelPosition": "above" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC", "labelPosition": "below" },
          { "date": "2026-05-01", "type": "production", "label": "May Prod", "labelPosition": "above" }
        ]
      },
      {
        "id": "M2",
        "label": "PDS & Data Inventory",
        "color": "#00897B",
        "milestones": []
      },
      {
        "id": "M3",
        "label": "Auto Review DFD",
        "color": "#546E7A",
        "milestones": []
      }
    ]
  },
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "highlightColumn": "Apr",
    "shipped": {
      "Jan": ["Auth service v2", "Rate limiter"],
      "Feb": ["Token refresh flow"],
      "Mar": ["OAuth2 PKCE", "Session mgmt"],
      "Apr": ["Gateway routing"]
    },
    "inProgress": {
      "Jan": [],
      "Feb": ["API Gateway scaffolding"],
      "Mar": ["Load testing framework"],
      "Apr": ["mTLS rollout", "Canary deploy pipeline"]
    },
    "carryover": {
      "Jan": [],
      "Feb": [],
      "Mar": ["Cert rotation automation"],
      "Apr": ["Cert rotation automation"]
    },
    "blockers": {
      "Jan": [],
      "Feb": [],
      "Mar": [],
      "Apr": ["Vendor SDK license renewal"]
    }
  }
}
```

---

## API Contracts

This application has no REST API, no WebSocket endpoints, and no backend-to-frontend data fetching. It is a server-rendered page with a single HTTP endpoint.

### HTTP Endpoints

| Method | Path | Response | Description |
|--------|------|----------|-------------|
| `GET /` | `200 OK` — `text/html` | Full 1920×1080 dashboard page, server-rendered by Blazor SSR |
| `GET /_framework/*` | Blazor framework static assets | Blazor Server JS interop files (auto-generated) |
| `GET /css/dashboard.css` | Static CSS file | Dashboard stylesheet from `wwwroot/css/` |
| `GET /data.json` | Static JSON file | Raw data file (also readable by external tools) |

### Error Responses

| Condition | Behavior |
|-----------|----------|
| `data.json` missing | Application fails to start. Console logs: `FileNotFoundException: Dashboard data file not found: {fullPath}` |
| `data.json` invalid JSON | Application fails to start. Console logs: `JsonException: {parse error details}` |
| `data.json` missing required field | Application fails to start. Console logs: `InvalidOperationException: Required field missing: {fieldName}` |
| Runtime request after successful start | Always returns 200 with fully rendered HTML. No error states possible during normal operation. |

### Internal Service Contract

```csharp
// DashboardDataService — the only "API" in the system
public class DashboardDataService
{
    // Called once at startup. Throws on failure (fail-fast).
    public Task LoadAsync(string webRootPath);

    // Called by Razor components during rendering. Never null after successful LoadAsync.
    public DashboardData Data { get; }
}
```

---

## Infrastructure Requirements

### Hosting

| Requirement | Specification |
|-------------|--------------|
| **Runtime** | .NET 8.0 SDK (8.0.x LTS), installed on developer's local machine |
| **Web Server** | Kestrel (built into ASP.NET Core) — no IIS, no Nginx, no reverse proxy |
| **Binding** | `localhost:5000` by default (configurable in `appsettings.json` or `launchSettings.json`) |
| **Protocol** | HTTP only (no HTTPS required for local-only tool; HTTPS available via .NET dev-cert if needed) |
| **OS** | Windows 10/11 (primary — Segoe UI font dependency). macOS/Linux functional with Arial fallback. |

### Networking

- **Inbound:** `localhost:5000` only. No external network access required.
- **Outbound:** None. Zero network calls. No telemetry, no package fetching at runtime.
- **Firewall:** No ports need to be opened. Kestrel binds to loopback interface only.

### Storage

| Item | Location | Size | Committed to Git? |
|------|----------|------|--------------------|
| `data.json` | `wwwroot/data.json` | < 50KB | No (`.gitignore`) |
| `data.sample.json` | `wwwroot/data.sample.json` | < 10KB | Yes |
| `dashboard.css` | `wwwroot/css/dashboard.css` | ~5KB | Yes |
| Application binary | `bin/Debug/net8.0/` | ~5MB | No |

### CI/CD

**Not required.** This is a local developer tool. No build pipeline, no deployment automation, no artifact publishing.

**Optional stretch goal:** A GitHub Actions workflow that runs `dotnet build` on PRs to verify compilation.

### Development Commands

| Action | Command |
|--------|---------|
| Restore (implicit) | `dotnet restore` |
| Build | `dotnet build` |
| Run | `dotnet run --project src/ReportingDashboard` |
| Hot reload | `dotnet watch --project src/ReportingDashboard` |
| Screenshot (optional) | Playwright script or manual browser capture |

---

## Technology Stack Decisions

### Core Stack

| Decision | Choice | Justification |
|----------|--------|---------------|
| **Framework** | .NET 8.0 LTS | Mandatory per requirements. LTS support through Nov 2026. |
| **UI Model** | Blazor Server with Static SSR | Renders complete HTML server-side on first request. No SignalR circuit overhead for a read-only page. Static SSR is ideal: no WebSocket, no interactivity budget, sub-500ms render. |
| **Project Template** | `dotnet new blazor` (Blazor Web App) | .NET 8 unified template. Supports static SSR out of the box. |
| **Solution Structure** | Single `.sln` + single `.csproj` | Matches complexity of the project (single page, single data source). No multi-project overhead. |

### Frontend Rendering

| Decision | Choice | Justification |
|----------|--------|---------------|
| **CSS Layout** | CSS Grid + Flexbox (raw) | The reference design uses exactly these primitives. Direct port of the reference CSS ensures pixel-perfect match. |
| **Timeline Rendering** | Inline SVG in Razor markup | The reference design uses inline SVG. Hand-crafted SVG gives exact control over coordinates, shapes, and filters. No charting library can match this specificity without fighting defaults. |
| **Component Library** | None (rejected MudBlazor, Radzen) | A component library would fight the fixed-layout design. Every pixel is specified; a library's opinionated styling would require overrides that are harder to maintain than raw CSS. Zero external CSS dependencies. |
| **CSS Architecture** | Single `dashboard.css` + CSS custom properties | One page = one stylesheet. CSS custom properties for the 16-color palette enable retheming without touching component markup. Scoped CSS rejected because the design's class names are tightly coupled across components. |

### Data Layer

| Decision | Choice | Justification |
|----------|--------|---------------|
| **Serialization** | `System.Text.Json` (built-in) | Zero NuGet dependency. Built into .NET 8. Sufficient performance for < 50KB JSON. |
| **Data Storage** | Flat `data.json` file | Requirements explicitly mandate single JSON file, no database. Simplest possible data layer. |
| **Data Loading** | Read once at startup (singleton) | Data is static for the application's lifetime. No need for per-request reads or caching layers. |
| **Database** | None (rejected SQLite, LiteDB) | Explicitly out of scope. No historical data, no querying, no CRUD. A database would add complexity with zero value. |
| **File Watching** | Not in v1 (stretch goal) | Base implementation requires app restart on data change. `FileSystemWatcher` can be added later as singleton enhancement. |

### Rejected Alternatives

| Alternative | Why Rejected |
|-------------|-------------|
| Blazor WebAssembly | Adds ~2MB download, client-side rendering delay, and complexity. SSR renders faster for a read-only page. |
| Razor Pages (non-Blazor) | Would work, but Blazor's component model enables cleaner decomposition of header/timeline/heatmap. |
| Static HTML generator | Would eliminate the web server, but loses `dotnet watch` hot reload and dynamic date calculations. |
| Chart.js / D3.js via JS interop | The timeline SVG is simple enough to render in Razor. Adding JS interop for a few shapes adds complexity and breaks SSR rendering. |
| Newtonsoft.Json | Unnecessary; `System.Text.Json` is built-in and sufficient. Adding Newtonsoft would violate the zero-dependency goal. |

---

## Security Considerations

### Threat Model

This application has a **minimal attack surface** by design:

| Surface | Risk | Mitigation |
|---------|------|------------|
| **Network exposure** | Localhost only — no remote access | Kestrel binds to `127.0.0.1` by default. No external listening. |
| **User input** | None — no forms, no query parameters, no POST endpoints | Zero injection vectors. All content is read from a trusted local file. |
| **Data sensitivity** | `data.json` may contain internal project names | `.gitignore` excludes `data.json`. `data.sample.json` template uses fictional data. |
| **Dependencies** | Zero external NuGet packages | No supply-chain risk beyond the .NET 8 SDK itself. |
| **Authentication** | None required | Single-user, local-only tool. No secrets, no tokens, no session management. |

### Data Protection

- **At rest:** `data.json` resides on the developer's local filesystem. Standard OS-level file permissions apply. No encryption needed for a local reporting tool.
- **In transit:** Localhost HTTP. No sensitive data crosses a network boundary. HTTPS is available via .NET dev-cert if the user wants it, but not required.
- **In memory:** Data is held in a singleton `DashboardData` record graph. Immutable (`init`-only properties). No mutation, no concurrent write concerns.

### Input Validation

Although there is no user input, the `data.json` file is validated at startup:

1. **JSON syntax validation** — `System.Text.Json` throws `JsonException` on malformed JSON
2. **Required field validation** — `required` keyword on record properties + explicit null checks in `Validate()`
3. **Range validation** — Track count (1–5), column count (2–6), date range (startDate < endDate)
4. **No HTML rendering of raw strings** — Blazor's default encoding prevents XSS even if `data.json` contains HTML-like strings

### Content Security

- No `<script>` tags in the rendered output (Blazor SSR generates clean HTML)
- No inline event handlers
- Static CSS file — no dynamic style injection
- SVG content is generated from typed C# models, not from raw string interpolation

---

## Scaling Strategy

### Scaling Context

This application is a **single-user, local-only tool**. Scaling in the traditional sense (horizontal scaling, load balancing, database sharding) is not applicable.

### Relevant Scaling Dimensions

| Dimension | Range | Strategy |
|-----------|-------|----------|
| **Timeline tracks** | 1–5 | Dynamic SVG height: `tracks * 56 + 17` px. `.tl-area` height adjusts proportionally. |
| **Heatmap columns (months)** | 2–6 | Dynamic CSS Grid: `grid-template-columns: 160px repeat(N, 1fr)` generated from data. |
| **Items per heatmap cell** | 0–10+ | Cells use `overflow: hidden`. Items beyond visible area are clipped. Users curate data to fit. |
| **Milestones per track** | 0–15 | SVG renders all milestones. Label collision is managed by alternating above/below positioning and user-specified `labelPosition`. |
| **Concurrent users** | 1 | Blazor Server supports multiple connections, but this tool is designed for one user taking a screenshot. No contention concerns. |

### Future Scaling Paths (Out of Scope for v1)

| Scenario | Approach |
|----------|----------|
| **Multiple projects** | Multiple `data-{project}.json` files + route parameter `/project/{name}` |
| **Historical data** | Date-stamped JSON files + dropdown selector |
| **Team-wide use** | Deploy to an internal server (no code changes needed — Kestrel serves multiple clients) |
| **Larger datasets** | Pagination or scrollable heatmap (would break the 1920×1080 constraint) |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to specific Blazor components, their CSS layout strategy, data bindings, and interactions.

### Component-to-Design Mapping

| Visual Section (Design) | Blazor Component | CSS Class(es) | Layout Strategy |
|--------------------------|-----------------|---------------|-----------------|
| Full page container | `Dashboard.razor` | `body` styles | `display: flex; flex-direction: column; width: 1920px; height: 1080px; overflow: hidden` |
| Header bar | `DashboardHeader.razor` | `.hdr` | Flexbox row, `justify-content: space-between`, padding `12px 44px 10px` |
| Title + link (left) | Within `DashboardHeader` | `.hdr h1`, `.sub` | Block layout, h1 at 24px bold, subtitle at 12px #888 |
| Legend (right) | Within `DashboardHeader` | Inline styles | Flexbox row, gap 22px, 12px font, diamond/circle/bar shapes via CSS |
| Timeline area | `TimelineArea.razor` | `.tl-area` | Flexbox row, height 196px, background #FAFAFA |
| Track labels sidebar | Within `TimelineArea` | Inline styles (230px sidebar) | Flex column, `justify-content: space-around`, 230px fixed width |
| SVG timeline | `TimelineSvg.razor` | `.tl-svg-box` | SVG 1560×185, `overflow: visible`, positioned via padding-left 12px |
| Month grid lines | Within `TimelineSvg` | SVG `<line>` elements | Computed at `monthIndex * (1560 / monthCount)` intervals |
| NOW marker | Within `TimelineSvg` | SVG `<line>` + `<text>` | X position from `DateToX(nowDate)`, dashed red stroke |
| Track lanes | Within `TimelineSvg` | SVG `<line>` per track | Full-width horizontal lines at computed Y positions |
| Milestone shapes | Within `TimelineSvg` | SVG `<circle>`, `<polygon>` | X from `DateToX(milestone.Date)`, Y from `TrackY(trackIndex)` |
| Heatmap wrapper | `HeatmapGrid.razor` | `.hm-wrap` | Flex column, `flex: 1; min-height: 0`, padding `10px 44px` |
| Heatmap title | Within `HeatmapGrid` | `.hm-title` | Static text, 14px bold uppercase #888 |
| Heatmap grid | Within `HeatmapGrid` | `.hm-grid` | CSS Grid, dynamic columns/rows |
| Corner cell ("STATUS") | Within `HeatmapGrid` | `.hm-corner` | Grid cell [1,1], 11px bold uppercase #999, bg #F5F5F5 |
| Month column headers | Within `HeatmapGrid` | `.hm-col-hdr`, `.highlight-hdr` | Grid cells [1, 2..N+1], 16px bold, highlight column gets gold bg |
| Shipped row | `HeatmapRow.razor` (cssPrefix="ship") | `.ship-hdr`, `.ship-cell` | Row header + N cells, green palette |
| In Progress row | `HeatmapRow.razor` (cssPrefix="prog") | `.prog-hdr`, `.prog-cell` | Row header + N cells, blue palette |
| Carryover row | `HeatmapRow.razor` (cssPrefix="carry") | `.carry-hdr`, `.carry-cell` | Row header + N cells, amber palette |
| Blockers row | `HeatmapRow.razor` (cssPrefix="block") | `.block-hdr`, `.block-cell` | Row header + N cells, red palette |
| Individual data cell | `HeatmapCell.razor` | `.hm-cell` | Padding 8px 12px, overflow hidden |
| Work item entry | Within `HeatmapCell` | `.it` | 12px text with `::before` colored dot, line-height 1.35 |

### Data Bindings per Component

| Component | Bound Data | Binding Type |
|-----------|------------|-------------|
| `DashboardHeader` | `ProjectInfo.Title`, `.Subtitle`, `.BacklogUrl`, `.CurrentMonth` | `[Parameter]` → Razor `@Data.Project.Title` |
| `TimelineArea` | `TimelineData` (full object) | `[Parameter]` → iterates `Timeline.Tracks` for sidebar labels |
| `TimelineSvg` | `TimelineData.StartDate`, `.EndDate`, `.NowDate`, `.Tracks[].Milestones[]` | `[Parameter]` → C# methods compute SVG coordinates |
| `HeatmapGrid` | `HeatmapData.Columns`, `.HighlightColumn` | `[Parameter]` → dynamic `style="grid-template-columns: ..."` |
| `HeatmapRow` | `HeatmapCategory.Items`, column list, highlight column | `[Parameter]` → `@foreach (var col in Columns)` |
| `HeatmapCell` | `List<string>` items for one month | `[Parameter]` → `@foreach (var item in Items)` or dash for empty |

### Dynamic CSS Generation

The heatmap grid requires runtime CSS because column count varies:

```razor
@* In HeatmapGrid.razor *@
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Heatmap.Columns.Count, 1fr);
                             grid-template-rows: 36px repeat(4, 1fr);">
```

The highlight column class is applied conditionally:

```razor
@foreach (var col in Heatmap.Columns)
{
    var isHighlight = col == Heatmap.HighlightColumn;
    <div class="hm-col-hdr @(isHighlight ? "highlight-hdr" : "")">@col</div>
}
```

---

## Risks & Mitigations

### Risk 1: Pixel Mismatch Between Reference and Blazor Output

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | High |
| **Impact** | High — visual fidelity is a primary success metric |
| **Description** | Blazor's HTML rendering may introduce subtle differences from the reference HTML: extra wrapper `<div>` elements, different whitespace, Blazor's component boundaries adding DOM nodes. These can shift CSS Grid calculations or SVG positioning. |
| **Mitigation** | Port the reference CSS verbatim into `dashboard.css` and keep component HTML as flat as possible. Use browser DevTools to compare the rendered DOM structure against the reference HTML side-by-side. Test at 1920×1080 in Chrome after every component change. The `.hm-grid` and `.tl-area` selectors must target the exact same DOM hierarchy as the reference. |

### Risk 2: SVG Coordinate Drift from Date Calculations

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium |
| **Impact** | Medium — mispositioned milestones reduce dashboard credibility |
| **Description** | The `DateToX()` function maps dates to pixel positions. Rounding errors, timezone issues, or incorrect date parsing could place milestones at wrong positions. The reference design uses hardcoded X values, so any calculation error will be visible. |
| **Mitigation** | Unit test `DateToX()` with known date/position pairs from the reference SVG (e.g., Jan 12 at x=104, Mar 26 at x=745). Use `DateTime` without timezone (dates only). Clamp output to `[0, 1560]`. Compare computed positions against the reference SVG's hardcoded values during development. |

### Risk 3: Blazor Server SignalR Overhead

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Low — affects memory usage, not functionality |
| **Description** | Blazor Server maintains a WebSocket connection per client. For a local tool with one user, this is irrelevant, but it adds ~30MB baseline memory and a persistent connection. |
| **Mitigation** | Use Blazor Static SSR (no `@rendermode` directive on the page component). In .NET 8, components without an explicit render mode use static SSR by default — the page is rendered to HTML on the server and sent as a complete response with no WebSocket. Verify by checking that the rendered page has no `blazor.server.js` script tag. |

### Risk 4: `data.json` Schema Evolution Breaking Existing Files

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium (over time) |
| **Impact** | Medium — users' data files stop working after a code update |
| **Description** | If the C# data model adds new required fields, existing `data.json` files will fail deserialization. Users may not know which field to add. |
| **Mitigation** | Use `required` keyword only for truly mandatory fields. New features use optional fields with sensible defaults (e.g., `NowDate` defaults to `DateTime.Today` if null). Version the schema via an optional `"$schema": "1.0"` field. Validation errors must name the specific missing field and suggest the fix. |

### Risk 5: Heatmap Cell Overflow with Many Items

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium |
| **Impact** | Low — items are clipped, not broken |
| **Description** | At 1080px total height with 3 fixed sections, each heatmap row has approximately 170px of height. At ~18px per item (12px font + padding), each cell fits ~9 items. More items are clipped by `overflow: hidden`. |
| **Mitigation** | Document the practical item limit (~8–9 items per cell) in `data.sample.json`. This is a data curation issue, not a code issue. The `overflow: hidden` behavior matches the PM spec (Scenario 8). |

### Risk 6: Browser Rendering Differences (Edge vs Chrome)

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Low — both are Chromium-based |
| **Description** | SVG text metrics and drop shadow rendering can vary between browser versions. |
| **Mitigation** | Both Edge and Chrome use the Chromium engine, so rendering is identical. Document Chrome/Edge as the only supported browsers. Optional Playwright screenshot automation uses Chromium by default, ensuring consistency. |

### Risk 7: Segoe UI Font Missing on Non-Windows Systems

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low (Windows-primary tool) |
| **Impact** | Low — visual difference only, not functional |
| **Description** | Segoe UI is a Windows system font. On macOS/Linux, the browser falls back to Arial, which has different metrics (character width, line height). This may cause text overflow or alignment shifts. |
| **Mitigation** | The PM spec explicitly states Windows-primary. The CSS fallback chain (`'Segoe UI', Arial, sans-serif`) handles non-Windows gracefully. Document that pixel-perfect screenshots require Windows + Chrome/Edge. |

---

## Solution Structure

```
ReportingDashboard.sln
└── src/
    └── ReportingDashboard/
        ├── ReportingDashboard.csproj        ← .NET 8 Blazor Web App
        ├── Program.cs                        ← Entry point, DI, fail-fast data load
        ├── Components/
        │   ├── App.razor                     ← <html> shell, <head>, CSS link
        │   ├── Routes.razor                  ← Blazor router
        │   ├── Layout/
        │   │   └── MainLayout.razor          ← Minimal layout (just @Body, no nav)
        │   └── Pages/
        │       └── Dashboard.razor           ← Route "/" — page orchestrator
        ├── Components/Dashboard/
        │   ├── DashboardHeader.razor         ← Header: title, link, subtitle, legend
        │   ├── TimelineArea.razor            ← Timeline: sidebar + SVG container
        │   ├── TimelineSvg.razor             ← SVG: grid, tracks, milestones, NOW
        │   ├── HeatmapGrid.razor             ← Heatmap: title + CSS Grid container
        │   ├── HeatmapRow.razor              ← One status row (header + N cells)
        │   └── HeatmapCell.razor             ← One data cell (items or dash)
        ├── Models/
        │   └── DashboardData.cs              ← All record types (single file)
        ├── Services/
        │   └── DashboardDataService.cs       ← JSON load + validation
        ├── wwwroot/
        │   ├── css/
        │   │   └── dashboard.css             ← All styles (ported from reference)
        │   ├── data.json                     ← User's project data (gitignored)
        │   └── data.sample.json              ← Template with example data (committed)
        ├── appsettings.json                  ← Kestrel port config
        └── Properties/
            └── launchSettings.json           ← Dev server profiles
```

### Key File Responsibilities

| File | Purpose | Lines (est.) |
|------|---------|-------------|
| `Program.cs` | DI registration, startup data load, middleware pipeline | ~25 |
| `DashboardData.cs` | 7 record types defining the complete data schema | ~80 |
| `DashboardDataService.cs` | File read, deserialization, validation | ~60 |
| `Dashboard.razor` | Page route, injects service, passes data to children | ~20 |
| `DashboardHeader.razor` | Header markup with data bindings | ~35 |
| `TimelineArea.razor` | Timeline container + track label sidebar | ~30 |
| `TimelineSvg.razor` | SVG generation with coordinate math | ~120 |
| `HeatmapGrid.razor` | Grid container, header row, row iteration | ~40 |
| `HeatmapRow.razor` | Row header + cell iteration | ~20 |
| `HeatmapCell.razor` | Item list or empty dash | ~15 |
| `dashboard.css` | Complete stylesheet ported from reference | ~120 |
| `data.sample.json` | Documented example data | ~80 |
| **Total** | | **~645** |