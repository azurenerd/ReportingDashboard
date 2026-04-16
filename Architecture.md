# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server (.NET 8) web application that renders a pixel-perfect 1920×1080 project status visualization optimized for screenshot capture into PowerPoint decks. It reads all data from a local `data.json` file, requires zero authentication, zero cloud dependencies, and zero additional NuGet packages beyond the default .NET 8 Blazor Server template.

**Architecture Goals:**

1. **Pixel-perfect rendering** — Reproduce the `OriginalDesignConcept.html` reference design exactly, driven entirely by data from `data.json`.
2. **Zero-dependency simplicity** — Single .NET 8 project, no external NuGet packages, no database, no JavaScript interop.
3. **Screenshot-first design** — Fixed 1920×1080 layout with no animations, scrollbars, or progressive rendering. The page is screenshot-ready on first paint.
4. **Data-driven configuration** — All dashboard content (title, milestones, heatmap items) defined in a single JSON file. Edit and refresh — no rebuild required.
5. **Minimal file count** — Entire solution under 15 files (excluding `bin/`, `obj/`, `.gitignore`).

**Architecture Pattern:** Single-project, component-based Blazor Server application using Static SSR (no SignalR circuit) with a singleton data service reading from a flat JSON file.

---

## System Components

### 1. `Program.cs` — Application Entry Point

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure Kestrel, register services, set up Blazor middleware, bind to `localhost:5000` |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService`, Blazor framework |
| **Data** | None directly; delegates to service layer |

**Key Configuration:**
- Bind Kestrel to `http://localhost:5000` only (no network exposure)
- Register `DashboardDataService` as a singleton via `builder.Services.AddSingleton<DashboardDataService>()`
- Map Blazor Static SSR endpoints — do NOT add `@rendermode InteractiveServer`
- Enable static file serving for `wwwroot/`
- No authentication middleware, no CORS, no HTTPS redirection

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddSingleton<DashboardDataService>();
builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>();
app.Run();
```

### 2. `DashboardDataService` — Data Access Layer

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Read, deserialize, validate, and cache `data.json` from disk; provide typed data to components |
| **Interfaces** | `DashboardData? GetDashboardData()`, `string? GetError()` |
| **Dependencies** | `IWebHostEnvironment` (to resolve `wwwroot/` path), `System.Text.Json` |
| **Data** | Reads `wwwroot/data.json`; holds deserialized `DashboardData` in memory |
| **Lifetime** | Singleton — reads file once at first request, re-reads on each HTTP request for edit-and-refresh workflow |

**Behavior:**
- On each call to `GetDashboardData()`, read `wwwroot/data.json` from disk (enables edit-and-refresh without app restart)
- Deserialize using `System.Text.Json` with `PropertyNameCaseInsensitive = true`
- If file is missing, return `null` and set error: `"data.json not found. Please place a valid data.json file in the wwwroot/ directory."`
- If JSON is malformed, return `null` and set error with the `JsonException.Message` for debugging
- No caching across requests — the file is small (<100KB) and reads are <1ms on localhost

```csharp
public class DashboardDataService
{
    private readonly string _dataFilePath;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataFilePath = Path.Combine(env.WebRootPath, "data.json");
    }

    public (DashboardData? Data, string? Error) LoadDashboard()
    {
        if (!File.Exists(_dataFilePath))
            return (null, "data.json not found. Please place a valid data.json file in the wwwroot/ directory. See data.sample.json for the expected format.");

        try
        {
            var json = File.ReadAllText(_dataFilePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<DashboardData>(json, options);
            return (data, null);
        }
        catch (JsonException ex)
        {
            return (null, $"Unable to load dashboard data. Please check data.json for syntax errors.\n{ex.Message}");
        }
    }
}
```

### 3. `App.razor` — Blazor Root Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define the HTML document shell: `<html>`, `<head>`, `<body>` with fixed 1920×1080 dimensions, viewport meta tag, CSS references |
| **Interfaces** | Blazor root component convention |
| **Dependencies** | `MainLayout.razor`, `app.css` |
| **Data** | None |

**Key Markup:**
- `<meta name="viewport" content="width=1920">`
- `<link href="app.css" rel="stylesheet" />`
- `<link href="ReportingDashboard.styles.css" rel="stylesheet" />` (Blazor CSS isolation bundle)
- Body fixed at `width: 1920px; height: 1080px; overflow: hidden`

### 4. `MainLayout.razor` — Layout Shell

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Provide the outermost layout wrapper; render `@Body` with no nav, no sidebar, no chrome |
| **Interfaces** | Blazor `LayoutComponentBase` |
| **Dependencies** | None |
| **Data** | None |

This is a pass-through layout — it renders `@Body` directly with no additional markup. The `Dashboard.razor` page handles all visual structure.

### 5. `Dashboard.razor` — Main Page Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Orchestrate the full dashboard: inject data service, handle error states, compose child components |
| **Interfaces** | Blazor page at route `/` |
| **Dependencies** | `DashboardDataService`, `Header.razor`, `Timeline.razor`, `Heatmap.razor` |
| **Data** | Receives `DashboardData` from service; passes typed subsets to child components via `[Parameter]` |

**Behavior:**
- On initialization, call `DashboardDataService.LoadDashboard()`
- If error, render a centered error message (white background, gray text) — no stack trace
- If success, render three child components in vertical flex layout:
  1. `<Header>` — receives title, subtitle, backlogUrl
  2. `<Timeline>` — receives milestones, timelineStartMonth, timelineEndMonth, currentDate
  3. `<Heatmap>` — receives heatmap data (months, currentMonthIndex, rows)

**Error State Rendering:**
```html
<div style="display:flex;align-items:center;justify-content:center;width:1920px;height:1080px;flex-direction:column;">
    <div style="font-size:20px;font-weight:600;color:#333;">@errorTitle</div>
    <div style="font-size:13px;color:#888;margin-top:8px;max-width:600px;text-align:center;">@errorDetail</div>
</div>
```

### 6. `Header.razor` — Header Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render project title, subtitle, ADO backlog link, and milestone legend icons |
| **Interfaces** | `[Parameter] string Title`, `[Parameter] string Subtitle`, `[Parameter] string BacklogUrl` |
| **Dependencies** | None |
| **Data** | Display-only from parameters |
| **CSS** | `Header.razor.css` (scoped) — ports `.hdr`, `.sub` styles from reference HTML |

### 7. `Timeline.razor` — SVG Timeline Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the full SVG timeline: month gridlines, milestone swim lanes, event markers (checkpoints, PoC diamonds, production diamonds), NOW line, event labels |
| **Interfaces** | `[Parameter] List<Milestone> Milestones`, `[Parameter] DateOnly TimelineStart`, `[Parameter] DateOnly TimelineEnd`, `[Parameter] DateOnly CurrentDate` |
| **Dependencies** | None |
| **Data** | Computed SVG coordinates from date-based position calculations |
| **CSS** | `Timeline.razor.css` (scoped) — ports `.tl-area`, `.tl-svg-box` styles |

**Coordinate Calculation Logic:**
```csharp
private const double SvgWidth = 1560.0;
private const double SvgHeight = 185.0;

private double GetXPosition(DateOnly date)
{
    var totalDays = TimelineEnd.DayNumber - TimelineStart.DayNumber;
    if (totalDays <= 0) return 0;
    var dayOffset = date.DayNumber - TimelineStart.DayNumber;
    return Math.Clamp((dayOffset / (double)totalDays) * SvgWidth, 0, SvgWidth);
}

private double GetYPosition(int milestoneIndex, int milestoneCount)
{
    // First milestone at y=42, distribute evenly across 185px height
    // Reserve top 25px for month labels, bottom 10px for padding
    var usableHeight = SvgHeight - 35;
    var spacing = usableHeight / Math.Max(milestoneCount, 1);
    return 35 + (milestoneIndex * spacing);
}
```

**SVG Element Mapping:**
| Event Type | SVG Element | Attributes |
|-----------|-------------|------------|
| Milestone line | `<line>` | `stroke=[color]; stroke-width=3; x1=0; x2=1560` |
| Checkpoint (standard) | `<circle>` | `r=7; fill=white; stroke=[milestone-color]; stroke-width=2.5` |
| Checkpoint (small dot) | `<circle>` | `r=4; fill=#999` |
| PoC milestone | `<polygon>` | Diamond shape, `fill=#F4B400; filter=url(#sh)` |
| Production milestone | `<polygon>` | Diamond shape, `fill=#34A853; filter=url(#sh)` |
| Month gridline | `<line>` | `stroke=#bbb; stroke-opacity=0.4; stroke-width=1` |
| NOW line | `<line>` | `stroke=#EA4335; stroke-width=2; stroke-dasharray=5,3` |
| Event label | `<text>` | `fill=#666; font-size=10; text-anchor=middle` |

**Diamond Polygon Generation:**
```csharp
private string GetDiamondPoints(double cx, double cy, double size = 11)
{
    return $"{cx},{cy - size} {cx + size},{cy} {cx},{cy + size} {cx - size},{cy}";
}
```

**Label Collision Avoidance:**
- Track the last label position per milestone lane
- If a new label's x-position is within 60px of the previous label, alternate between rendering above (`cy - 16`) and below (`cy + 20`) the milestone line
- Default label position is above the line; toggled per collision

### 8. `Heatmap.razor` — Heatmap Grid Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the CSS Grid heatmap: section title, column headers (months), row headers (categories), and data cells |
| **Interfaces** | `[Parameter] HeatmapData Heatmap` |
| **Dependencies** | `HeatmapCell.razor` |
| **Data** | Month names, current month index, category rows with item lists |
| **CSS** | `Heatmap.razor.css` (scoped) — ports `.hm-wrap`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr` styles |

**Dynamic Grid Columns:**
```csharp
private string GridTemplateColumns => $"160px repeat({Heatmap.Months.Count}, 1fr)";
```

**Category-to-CSS Mapping (computed in `@code` block):**

| Category | Row Header CSS | Cell CSS | Current Cell CSS | Bullet Color |
|----------|---------------|----------|-----------------|-------------|
| `shipped` | `ship-hdr` | `ship-cell` | `ship-cell apr` | `#34A853` |
| `in-progress` | `prog-hdr` | `prog-cell` | `prog-cell apr` | `#0078D4` |
| `carryover` | `carry-hdr` | `carry-cell` | `carry-cell apr` | `#F4B400` |
| `blockers` | `block-hdr` | `block-cell` | `block-cell apr` | `#EA4335` |

### 9. `HeatmapCell.razor` — Individual Cell Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render a single heatmap data cell with bulleted work items or an empty-state dash |
| **Interfaces** | `[Parameter] List<string> Items`, `[Parameter] string CssClass` |
| **Dependencies** | None |
| **Data** | List of item strings from parent |
| **CSS** | `HeatmapCell.razor.css` (scoped) — ports `.hm-cell`, `.it`, `.it::before` styles |

**Empty State:** If `Items` is null or empty, render `<span style="color:#999;">-</span>`.

### 10. `DashboardData.cs` — Data Model

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Strongly-typed C# model mirroring the `data.json` schema |
| **Interfaces** | Plain C# record types (immutable) |
| **Dependencies** | `System.Text.Json.Serialization` for `[JsonPropertyName]` attributes |
| **Data** | All dashboard configuration fields |

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        Browser (Edge/Chrome)                 │
│                     http://localhost:5000                     │
└─────────────────────┬───────────────────────────────────────┘
                      │ HTTP GET /
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                    Kestrel (localhost:5000)                   │
│                     Static SSR Pipeline                      │
└─────────────────────┬───────────────────────────────────────┘
                      │ Renders Dashboard.razor
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                    Dashboard.razor (Page)                     │
│  ┌──────────────────────────────────────────────────────┐   │
│  │         DashboardDataService.LoadDashboard()          │   │
│  │                                                        │   │
│  │  ┌──────────┐    ┌──────────────┐    ┌────────────┐  │   │
│  │  │ Read file │───▶│ Deserialize  │───▶│ Return     │  │   │
│  │  │ from disk │    │ System.Text  │    │ typed data │  │   │
│  │  └──────────┘    │ .Json        │    └────────────┘  │   │
│  │                   └──────────────┘                     │   │
│  └──────────────────────────────────────────────────────┘   │
│                          │                                    │
│          ┌───────────────┼───────────────┐                   │
│          ▼               ▼               ▼                   │
│  ┌──────────────┐ ┌────────────┐ ┌──────────────┐          │
│  │ Header.razor  │ │ Timeline   │ │ Heatmap.razor │          │
│  │               │ │ .razor     │ │               │          │
│  │ [Parameter]:  │ │            │ │ [Parameter]:  │          │
│  │ Title         │ │ [Parameter]│ │ HeatmapData   │          │
│  │ Subtitle      │ │ Milestones │ │               │          │
│  │ BacklogUrl    │ │ Start/End  │ │  ┌──────────┐│          │
│  └──────────────┘ │ CurrentDate│ │  │HeatmapCell││          │
│                    └────────────┘ │  │  .razor   ││          │
│                                    │  └──────────┘│          │
│                                    └──────────────┘          │
└─────────────────────────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│              wwwroot/data.json (flat file on disk)            │
└─────────────────────────────────────────────────────────────┘
```

### Communication Patterns

1. **File → Service:** `DashboardDataService` reads `data.json` synchronously from disk via `File.ReadAllText()`. No async I/O needed — file is local and <100KB.

2. **Service → Page:** `Dashboard.razor` calls `DashboardDataService.LoadDashboard()` during component initialization (`OnInitialized`). Returns a tuple of `(DashboardData?, string?)`.

3. **Page → Child Components:** `Dashboard.razor` passes typed data subsets to child components via Blazor `[Parameter]` properties. Data flows one-way (parent → child). No callbacks, no events, no two-way binding.

4. **Server → Browser:** Blazor Static SSR renders the full HTML on the server and sends it as a single HTTP response. No SignalR circuit, no WebSocket, no incremental updates.

5. **Edit-and-Refresh Cycle:** User edits `data.json` on disk → refreshes browser → new HTTP GET → `DashboardDataService` re-reads file → fresh render. No file watching, no hot reload of data.

---

## Data Model

### Entity Definitions

```csharp
// Models/DashboardData.cs

public record DashboardData
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogUrl { get; init; } = "";
    public string CurrentDate { get; init; } = "";          // "2026-04-10" (ISO 8601)
    public string TimelineStartMonth { get; init; } = "";   // "2026-01" (YYYY-MM)
    public string TimelineEndMonth { get; init; } = "";     // "2026-06" (YYYY-MM)
    public List<Milestone> Milestones { get; init; } = new();
    public HeatmapData Heatmap { get; init; } = new();
}

public record Milestone
{
    public string Id { get; init; } = "";          // "M1", "M2", "M3"
    public string Label { get; init; } = "";       // "Chatbot & MS Role"
    public string Color { get; init; } = "";       // "#0078D4" (hex color)
    public List<MilestoneEvent> Events { get; init; } = new();
}

public record MilestoneEvent
{
    public string Date { get; init; } = "";        // "2026-01-12" (ISO 8601)
    public string Type { get; init; } = "";        // "checkpoint" | "checkpoint-small" | "poc" | "production"
    public string Label { get; init; } = "";       // "Jan 12", "Mar 26 PoC"
    public string? LabelPosition { get; init; }    // "above" | "below" (optional override)
}

public record HeatmapData
{
    public List<string> Months { get; init; } = new();     // ["Jan", "Feb", "Mar", "Apr"]
    public int CurrentMonthIndex { get; init; }             // 0-based index (3 = April)
    public List<HeatmapRow> Rows { get; init; } = new();
}

public record HeatmapRow
{
    public string Category { get; init; } = "";                              // "shipped" | "in-progress" | "carryover" | "blockers"
    public Dictionary<string, List<string>> Items { get; init; } = new();   // { "Jan": ["Item A"], "Feb": [] }
}
```

### Entity Relationships

```
DashboardData (root)
├── title, subtitle, backlogUrl, currentDate, timelineStartMonth, timelineEndMonth
├── Milestones[] (1:N)
│   ├── id, label, color
│   └── Events[] (1:N)
│       └── date, type, label, labelPosition?
└── Heatmap (1:1)
    ├── months[], currentMonthIndex
    └── Rows[] (1:N, exactly 4)
        ├── category
        └── Items{} (month → string[])
```

### Storage

| Aspect | Detail |
|--------|--------|
| **Format** | JSON flat file |
| **Location** | `wwwroot/data.json` |
| **Size** | Typically 2-10KB; max supported ~100KB |
| **Access** | Read-only by the application; manually edited by users |
| **Versioning** | `data.json` in `.gitignore`; `data.sample.json` committed as template |
| **Backup** | User responsibility; suggest versioned copies (e.g., `data.2026-04.json`) |

### Sample `data.json` Schema

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "currentDate": "2026-04-10",
  "timelineStartMonth": "2026-01",
  "timelineEndMonth": "2026-06",
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
        { "date": "2026-03-15", "type": "checkpoint-small", "label": "" },
        { "date": "2026-03-21", "type": "poc", "label": "Mar 21 PoC" },
        { "date": "2026-04-25", "type": "production", "label": "Apr 25 Prod" }
      ]
    },
    {
      "id": "M3",
      "label": "Auto Review DFD",
      "color": "#546E7A",
      "events": [
        { "date": "2026-04-15", "type": "poc", "label": "Apr 15 PoC" },
        { "date": "2026-06-01", "type": "production", "label": "Jun 1 Prod" }
      ]
    }
  ],
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonthIndex": 3,
    "rows": [
      {
        "category": "shipped",
        "items": {
          "Jan": ["Chatbot v1 launch", "Role mapping complete"],
          "Feb": ["PDS integration"],
          "Mar": ["Data inventory audit"],
          "Apr": ["Auto-classification v1"]
        }
      },
      {
        "category": "in-progress",
        "items": {
          "Jan": ["PDS connector"],
          "Feb": ["DFD prototype"],
          "Mar": ["Review pipeline"],
          "Apr": ["DFD automation", "Compliance API"]
        }
      },
      {
        "category": "carryover",
        "items": {
          "Jan": [],
          "Feb": ["Chatbot edge cases"],
          "Mar": [],
          "Apr": ["PDS edge cases"]
        }
      },
      {
        "category": "blockers",
        "items": {
          "Jan": [],
          "Feb": [],
          "Mar": ["API access pending"],
          "Apr": []
        }
      }
    ]
  }
}
```

---

## API Contracts

This application has **no REST API, no GraphQL, no custom endpoints**. It is a single-page Blazor Server application using Static SSR. All data flows through in-process C# method calls.

### HTTP Endpoints (Kestrel)

| Method | Path | Response | Description |
|--------|------|----------|-------------|
| `GET` | `/` | `200 OK` — Full HTML page | Blazor renders `Dashboard.razor` via Static SSR. Returns complete HTML document. |
| `GET` | `/data.json` | `200 OK` — JSON file | Static file serving from `wwwroot/`. Allows browser to fetch raw JSON if needed. |
| `GET` | `/data.sample.json` | `200 OK` — JSON file | Static file serving. Template file for user reference. |
| `GET` | `/app.css` | `200 OK` — CSS file | Static file serving. Global stylesheet. |
| `GET` | `/ReportingDashboard.styles.css` | `200 OK` — CSS file | Blazor CSS isolation bundle (auto-generated). |

### Internal Service Contract

```csharp
public class DashboardDataService
{
    /// <summary>
    /// Reads and deserializes data.json from wwwroot/.
    /// Returns (data, null) on success or (null, errorMessage) on failure.
    /// </summary>
    public (DashboardData? Data, string? Error) LoadDashboard();
}
```

### Error Handling Contract

| Scenario | Behavior | User-Visible Message |
|----------|----------|---------------------|
| `data.json` missing | Service returns `(null, error)` | "data.json not found. Please place a valid data.json file in the wwwroot/ directory. See data.sample.json for the expected format." |
| `data.json` malformed JSON | Service catches `JsonException` | "Unable to load dashboard data. Please check data.json for syntax errors." + `ex.Message` in gray subtext |
| `data.json` missing required fields | `System.Text.Json` deserializes with defaults (empty strings, empty lists) | Dashboard renders with blank sections — no crash. Fields use safe defaults. |
| Unexpected server error | ASP.NET default error page (development) or generic 500 (production) | Standard .NET 8 error handling |

---

## Infrastructure Requirements

### Hosting

| Requirement | Specification |
|-------------|--------------|
| **Runtime** | .NET 8 SDK (8.0.x) installed on developer machine |
| **Web Server** | Kestrel (built into ASP.NET Core) — no IIS, no Nginx, no Apache |
| **Binding** | `http://localhost:5000` — loopback only, no network exposure |
| **OS** | Windows 10/11 (required for Segoe UI font) |
| **HTTPS** | Not required — HTTP is acceptable for localhost |

### Networking

| Requirement | Specification |
|-------------|--------------|
| **Inbound** | Localhost only — no firewall rules needed |
| **Outbound** | None — no API calls, no telemetry, no package downloads at runtime |
| **DNS** | Not applicable |
| **Load Balancer** | Not applicable |

### Storage

| Requirement | Specification |
|-------------|--------------|
| **Disk space** | < 10MB (project files + SDK cached assets) |
| **Data file** | `wwwroot/data.json` — typically 2-10KB |
| **Database** | None |
| **Persistent storage** | None beyond the project directory on disk |

### CI/CD

Not required. This is a local developer tool. The deployment workflow is:

```
git clone <repo>
cd ReportingDashboard/ReportingDashboard
dotnet run
# Open http://localhost:5000 in Edge/Chrome
```

### Development Tooling

| Tool | Purpose | Required |
|------|---------|----------|
| .NET 8 SDK | Build and run | Yes |
| Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | IDE | Recommended |
| Edge or Chrome | View and screenshot | Yes |
| `dotnet watch` | Hot reload during development | Recommended |

---

## Technology Stack Decisions

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Framework** | Blazor Server (.NET 8) | `net8.0` | Mandated stack. Enables server-side C# rendering with zero JavaScript. Static SSR mode eliminates SignalR overhead for this read-only use case. |
| **Render Mode** | Static SSR | Built-in | No interactivity needed. Avoids SignalR circuit, reduces page weight, faster first paint. Page is pure HTML/CSS/SVG — no client-side framework needed. |
| **CSS Layout** | Native CSS Grid + Flexbox | N/A | The HTML reference design already uses these exclusively. Direct port with no CSS framework dependency. |
| **SVG Rendering** | Inline SVG in Razor | N/A | Timeline visualization requires positioned shapes (lines, circles, polygons). SVG is the natural choice — renders crisply at any zoom, no canvas/JS needed. Razor `@foreach` loops generate elements data-driven. |
| **JSON Deserialization** | `System.Text.Json` | 8.0.x (built-in) | Native to .NET 8, zero additional packages. Fast, well-documented, supports case-insensitive property matching. |
| **Data Storage** | Flat JSON file | N/A | The simplest possible data layer. No ORM, no migrations, no connection strings. Users edit a text file — any text editor works. |
| **CSS Architecture** | Blazor CSS Isolation (`.razor.css`) + global `app.css` | Built-in | Scoped CSS prevents style leakage between components. Global `app.css` holds CSS custom properties for theming. |
| **Font** | Segoe UI (system font) | N/A | Available on all Windows machines. No web font loading, no FOUT, no additional HTTP requests. |
| **Testing (optional)** | xUnit + bUnit | 2.7+ / 1.25+ | Optional Phase 2. xUnit for model/service unit tests, bUnit for component rendering tests. |

### Packages Not Used (and Why)

| Package | Reason for Exclusion |
|---------|---------------------|
| Entity Framework | No database |
| MudBlazor / Radzen | Adds 500KB+ CSS; design is custom, not component-library aligned |
| Chart.js / ApexCharts | Would require JS interop; SVG rendering is sufficient and simpler |
| SignalR (interactive) | Read-only page; Static SSR is lighter |
| Bootstrap / Tailwind | Design uses custom CSS from reference HTML; framework would conflict |
| Serilog / NLog | Overkill for a local tool; `Console.WriteLine` suffices for debugging |

---

## Security Considerations

### Authentication & Authorization

**None.** This is an intentional design decision. The application:
- Binds exclusively to `localhost` — not reachable from other machines
- Has no user accounts, roles, or permissions
- Has no login page, no JWT tokens, no cookies
- Serves a single read-only page with no user input

**Future consideration:** If the dashboard is ever shared on a network, add Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (single line in `Program.cs`).

### Data Protection

| Concern | Mitigation |
|---------|-----------|
| **Sensitive project data in `data.json`** | Add `data.json` to `.gitignore`. Provide `data.sample.json` as a committed template with fictional data. |
| **Data at rest** | No encryption needed — file lives on the user's local machine, protected by OS-level file permissions. |
| **Data in transit** | Localhost only — no network transmission. HTTP (not HTTPS) is acceptable. |
| **Secrets/credentials** | None exist. No API keys, no connection strings, no tokens. |
| **XSS/injection** | Blazor Razor components HTML-encode all `@` expressions by default. No raw HTML rendering (`@((MarkupString)...)`) is needed. SVG attributes use numeric values and hex colors only. |
| **CSRF** | Blazor includes `AntiforgeryToken` middleware by default. No forms exist, but the middleware is harmless. |

### Input Validation

| Input | Validation Strategy |
|-------|-------------------|
| `data.json` structure | `System.Text.Json` deserialization catches malformed JSON. Missing fields default to empty strings/lists — no crash. |
| Date fields (`currentDate`, `timelineStartMonth`, etc.) | Parse with `DateOnly.TryParse()` in `Timeline.razor`. Invalid dates result in a NOW line at x=0 (graceful degradation). |
| Color fields (`milestone.color`) | Passed directly to SVG `stroke`/`fill` attributes. Invalid colors render as black (browser default). No security risk. |
| URL fields (`backlogUrl`) | Rendered in an `<a href="...">` tag with `target="_blank"`. Blazor HTML-encodes the value. No script injection possible. |
| Heatmap item strings | Rendered as text content in `<div>` elements. Blazor auto-escapes HTML entities. |

---

## Scaling Strategy

**This application does not scale and is not designed to.** It is a single-user, single-machine, localhost-only tool.

| Dimension | Strategy |
|-----------|---------|
| **Users** | Single user. No concurrent access. No session management. |
| **Data volume** | Single `data.json` file <100KB. No pagination, no lazy loading. |
| **Page complexity** | Fixed 3-5 milestones, 4-6 months, 4 heatmap rows. The grid is finite and bounded. |
| **Multiple projects** | Out of scope. If needed in the future, support multiple named JSON files (e.g., `data.project-a.json`) and a route parameter (`/project-a`). |
| **Historical reports** | Save timestamped copies of `data.json` (e.g., `data.2026-04.json`). No database, no archival system. |

### Performance Budget

| Metric | Budget | How Achieved |
|--------|--------|-------------|
| First meaningful paint | < 500ms | Static SSR sends complete HTML in first response. No JS framework boot. No second round-trip. |
| data.json load | < 10ms | Synchronous `File.ReadAllText()` on a local SSD. File is <100KB. |
| Full page render | < 1s | All rendering is server-side string concatenation. No virtual DOM diffing, no hydration. |
| Memory | < 150MB | Kestrel + single Blazor request. No persistent circuits, no WebSocket connections in Static SSR mode. |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component.

### Visual Section → Component Mapping

| Visual Section | Component | CSS Layout | Data Bindings | Interactions |
|---------------|-----------|-----------|---------------|-------------|
| **Header bar** (`.hdr`) | `Header.razor` | `display: flex; align-items: center; justify-content: space-between; padding: 12px 44px 10px` | `Title` → `<h1>`, `Subtitle` → `.sub <div>`, `BacklogUrl` → `<a href>` | ADO link opens in new tab (`target="_blank"`) |
| **Legend icons** (header right) | `Header.razor` (inline) | `display: flex; gap: 22px; align-items: center` | Static — no data binding. Four hardcoded legend items. | None (read-only) |
| **Timeline sidebar** (milestone labels) | `Timeline.razor` (left div) | `width: 230px; flex-shrink: 0; display: flex; flex-direction: column; justify-content: space-around` | `Milestones[].Id` → bold colored text, `Milestones[].Label` → gray subtext | None |
| **SVG timeline** (`.tl-svg-box`) | `Timeline.razor` (SVG element) | `flex: 1; padding-left: 12px` — SVG is `width="1560" height="185"` | `Milestones[]` → swim lane lines + events; `TimelineStart/End` → month gridlines; `CurrentDate` → NOW line | None |
| **Heatmap title** (`.hm-title`) | `Heatmap.razor` (static div) | `font-size: 14px; font-weight: 700; color: #888; text-transform: uppercase; letter-spacing: 0.5px` | Static text | None |
| **Heatmap grid** (`.hm-grid`) | `Heatmap.razor` | `display: grid; grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr)` | `Heatmap.Months` → column count + headers; `Heatmap.CurrentMonthIndex` → amber highlight class; `Heatmap.Rows` → 4 data rows | None |
| **Corner cell** (`.hm-corner`) | `Heatmap.razor` (inline) | `background: #F5F5F5; font-size: 11px; font-weight: 700; color: #999; text-transform: uppercase` | Static "STATUS" text | None |
| **Month column headers** (`.hm-col-hdr`) | `Heatmap.razor` (`@foreach`) | `font-size: 16px; font-weight: 700; background: #F5F5F5` — current month gets `.apr-hdr` class | `Heatmap.Months[i]` → month name; current month shows "▼ Now" suffix | None |
| **Row headers** (`.hm-row-hdr`) | `Heatmap.razor` (`@foreach`) | `font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.7px` + category color classes | `Row.Category` → display name + emoji prefix + CSS class | None |
| **Data cells** (`.hm-cell`) | `HeatmapCell.razor` | `padding: 8px 12px; overflow: hidden` + category background class | `Items` → bulleted list of strings; empty → gray dash "-" | None |
| **Cell bullet items** (`.it`) | `HeatmapCell.razor` (inline) | `font-size: 12px; color: #333; padding: 2px 0 2px 12px; position: relative` — `::before` pseudo-element for 6×6px colored bullet | Each `Items[i]` → one `.it` div | None |

### Component Hierarchy

```
App.razor
└── MainLayout.razor
    └── Dashboard.razor (route: "/")
        ├── Header.razor
        │   ├── Title + ADO Link (inline h1 + a)
        │   ├── Subtitle (inline div)
        │   └── Legend (inline flex container with 4 icon spans)
        ├── Timeline.razor
        │   ├── Milestone Sidebar (left div, one entry per milestone)
        │   └── SVG Element (1560×185)
        │       ├── <defs> (drop shadow filter)
        │       ├── Month Gridlines (@foreach months → <line> + <text>)
        │       ├── NOW Line (<line> + <text>)
        │       └── Milestone Lanes (@foreach milestones)
        │           ├── Swim Lane Line (<line>)
        │           └── Events (@foreach events)
        │               ├── checkpoint → <circle>
        │               ├── checkpoint-small → <circle r=4>
        │               ├── poc → <polygon> (diamond, amber)
        │               ├── production → <polygon> (diamond, green)
        │               └── Label → <text>
        └── Heatmap.razor
            ├── Section Title (static div)
            ├── Corner Cell (inline div)
            ├── Month Headers (@foreach months → div)
            └── Data Rows (@foreach rows)
                ├── Row Header (inline div with category styling)
                └── Cells (@foreach months)
                    └── HeatmapCell.razor
                        └── Items (@foreach items → div.it) or dash
```

### CSS File Strategy

| File | Scope | Contents |
|------|-------|----------|
| `wwwroot/app.css` | Global | CSS reset (`* { margin:0; padding:0; box-sizing:border-box }`), body fixed dimensions, CSS custom properties (`:root`), font-smoothing, link styles |
| `Header.razor.css` | Scoped to Header | `.hdr`, `.sub`, legend icon styles |
| `Timeline.razor.css` | Scoped to Timeline | `.tl-area`, `.tl-svg-box`, milestone sidebar styles |
| `Heatmap.razor.css` | Scoped to Heatmap | `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-col-hdr.current`, `.hm-row-hdr`, category row color classes (`.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr`) |
| `HeatmapCell.razor.css` | Scoped to HeatmapCell | `.hm-cell`, `.it`, `.it::before`, category cell background classes (`.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`), `.current` modifier for current-month highlighting |

---

## Risks & Mitigations

| # | Risk | Severity | Likelihood | Impact | Mitigation |
|---|------|----------|-----------|--------|-----------|
| 1 | **SVG label overlap** — Milestone events close in date produce overlapping text labels | Medium | High | Labels become unreadable in screenshot | Implement label collision detection: track last label x-position per lane; alternate above/below placement when labels are within 60px. Allow `labelPosition` override in `data.json` for manual control. |
| 2 | **Blazor CSS isolation and `::before` pseudo-elements** — Blazor's CSS isolation rewrites selectors with a unique attribute, but `::before` on child elements may not scope correctly | Medium | Medium | Bullet circles don't render in heatmap cells | Test early. If scoped CSS fails for `::before`, move bullet styles to global `app.css` with specific class selectors (`.ship-cell .it::before`). |
| 3 | **Screenshot pixel mismatch** — Browser rendering differs slightly from the HTML reference due to Blazor's HTML structure (extra wrapper divs, different attribute order) | Medium | Medium | Failed visual acceptance criteria (<5% pixel deviation) | Use `dotnet watch` to iterate rapidly. Compare screenshots at each milestone. Port CSS class names exactly from reference HTML. Avoid Blazor-generated wrapper elements by using `@inherits ComponentBase` and minimal markup. |
| 4 | **Hot reload breaks SVG rendering** — Known .NET 8 issue where `dotnet watch` hot reload doesn't always re-render SVG elements | Low | Medium | Developer frustration during iteration | Document workaround: use `dotnet watch --no-hot-reload` or full page refresh (`Ctrl+Shift+R`). |
| 5 | **Segoe UI unavailable on non-Windows** — macOS/Linux developers see Arial fallback, producing different text metrics | Low | Low | Timeline labels shift position; screenshot doesn't match reference | Document Windows requirement in README. The font stack (`'Segoe UI', Arial, sans-serif`) provides graceful fallback. |
| 6 | **data.json schema drift** — User adds unexpected fields or uses wrong types (e.g., number instead of string for date) | Low | Medium | Silent data loss or rendering gaps | Use `record` types with default values — missing fields produce empty content, not crashes. Add a `data.sample.json` with inline comments (via a companion `data.schema.md` since JSON doesn't support comments). |
| 7 | **Heatmap cell overflow** — More than 8 items in a single cell exceed the available vertical space | Low | Low | Text is clipped (`overflow: hidden`) | Document the 8-item limit in `data.sample.json`. The CSS `overflow: hidden` ensures no layout breakage — items beyond the visible area are simply clipped. |
| 8 | **Static SSR and `OnInitialized` timing** — In Static SSR mode, `OnInitializedAsync` completes before the response is sent. Synchronous file I/O in `OnInitialized` blocks the request thread briefly. | Low | Low | Negligible delay (<10ms for local file read) | Use synchronous `File.ReadAllText()` — it's a local file <100KB. Async would add complexity for no measurable benefit on localhost. |

### Decisions Log

| Decision | Alternative Considered | Rationale for Choice |
|----------|----------------------|---------------------|
| Static SSR (no `@rendermode InteractiveServer`) | Interactive Server mode | Read-only page needs no real-time updates. Static SSR eliminates SignalR circuit overhead, reduces page size, and produces cleaner HTML for screenshots. |
| Re-read `data.json` on every request (no caching) | Cache with `IMemoryCache` + file watcher invalidation | File is <100KB, read takes <1ms on SSD. Caching adds complexity (invalidation, race conditions) for zero perceptible benefit. Fresh read guarantees edit-and-refresh works. |
| Inline SVG in Razor (not a .svg file or JS canvas) | Chart.js, D3.js, or static SVG file | Razor `@foreach` loops make the SVG data-driven. No JS interop needed. Full control over every SVG element matches the reference HTML exactly. |
| `record` types for data model | `class` with `[Required]` attributes | Records are immutable, concise, and provide value-based equality. Default values prevent null crashes without explicit validation. |
| CSS isolation per component + global `app.css` | All styles in global `app.css` | CSS isolation prevents accidental style conflicts as components are developed independently. Global file holds only shared custom properties. |
| Single `.csproj` (no test project in MVP) | Separate `ReportingDashboard.Tests.csproj` | Tests are optional for Phase 2. A single project keeps the file count under 15 and the solution simple. Test project can be added later without architectural changes. |

---

## Solution Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── .gitignore                              # Includes data.json, bin/, obj/
├── README.md                               # Setup instructions, screenshot workflow
└── ReportingDashboard/
    ├── ReportingDashboard.csproj            # net8.0, no additional PackageReferences
    ├── Program.cs                           # Kestrel config, DI registration, middleware
    ├── Models/
    │   └── DashboardData.cs                 # All record types (DashboardData, Milestone, etc.)
    ├── Services/
    │   └── DashboardDataService.cs          # File reader + JSON deserializer
    ├── Components/
    │   ├── App.razor                        # HTML shell (<html>, <head>, <body>)
    │   ├── Routes.razor                     # Blazor router
    │   ├── Layout/
    │   │   └── MainLayout.razor             # Pass-through layout (renders @Body)
    │   ├── Pages/
    │   │   └── Dashboard.razor              # Main page (route: "/")
    │   └── Shared/
    │       ├── Header.razor                 # Title, subtitle, legend
    │       ├── Header.razor.css             # Scoped header styles
    │       ├── Timeline.razor               # SVG timeline with milestones
    │       ├── Timeline.razor.css           # Scoped timeline styles
    │       ├── Heatmap.razor                # CSS Grid heatmap
    │       ├── Heatmap.razor.css            # Scoped heatmap styles
    │       ├── HeatmapCell.razor            # Individual cell with items
    │       └── HeatmapCell.razor.css        # Scoped cell styles
    ├── wwwroot/
    │   ├── app.css                          # Global styles + CSS custom properties
    │   ├── data.json                        # Dashboard data (gitignored)
    │   └── data.sample.json                 # Template with fictional data (committed)
    └── Properties/
        └── launchSettings.json              # http://localhost:5000
```

**Total file count: 19** (excluding `bin/`, `obj/`, `.gitignore` internals). The `.razor.css` files are necessary for CSS isolation but could be consolidated into `app.css` to reduce count to 14 if the 15-file budget is strict.