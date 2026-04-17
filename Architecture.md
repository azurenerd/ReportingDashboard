# Architecture

## Overview & Goals

This is a single-page executive reporting dashboard that renders a pixel-perfect 1920×1080 visualization of project milestones (timeline) and monthly execution status (heatmap). All data is read from a local `data.json` file. The application is a .NET 8 Blazor Server app running locally via `dotnet run` — no cloud services, no database, no authentication. The primary output is browser screenshots pasted into PowerPoint.

**Architecture philosophy:** Three layers, one direction, no mutations.

```
data.json → DashboardDataService → Blazor Components → Rendered HTML/SVG/CSS
```

**Goals:**
1. Visual fidelity — pixel-match `OriginalDesignConcept.html` at 1920×1080
2. Data-driven — all content controlled by editing `data.json`
3. Minimal — 5–8 source files, zero external NuGet packages, zero infrastructure
4. Screenshot-ready — fixed viewport, no scrollbars, no Blazor UI chrome

---

## System Components

### 1. `Program.cs` — Application Entry Point

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure DI, register services, set up Kestrel, map Razor components |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService`, Blazor framework |
| **Data** | None directly |

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents(); // Static SSR only — no .AddInteractiveServerComponents()
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>();  // No .AddInteractiveServerRenderMode()
app.Run();
```

**Key decision:** No `AddInteractiveServerComponents()` and no `AddInteractiveServerRenderMode()`. This enables **Static SSR** — pure server-rendered HTML with zero SignalR WebSocket overhead. The dashboard is read-only; interactivity is unnecessary.

### 2. `DashboardDataService.cs` — Data Access & Layout Calculation

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Read `data.json`, deserialize to POCOs, pre-calculate SVG pixel positions |
| **Interfaces** | `DashboardData GetDashboardData()` (returns cached data), `string? GetError()` (returns load error) |
| **Dependencies** | `IWebHostEnvironment` (to resolve `wwwroot` path), `System.Text.Json` |
| **Data** | Owns the in-memory `DashboardData` singleton |
| **Lifetime** | Singleton — reads file once at first access, caches forever |

```csharp
public class DashboardDataService
{
    private DashboardData? _data;
    private string? _error;
    private readonly string _jsonPath;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _jsonPath = Path.Combine(env.WebRootPath, "data", "data.json");
        Load();
    }

    public DashboardData? GetDashboardData() => _data;
    public string? GetError() => _error;

    private void Load()
    {
        try
        {
            var json = File.ReadAllText(_jsonPath);
            _data = JsonSerializer.Deserialize<DashboardData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (_data != null) CalculateTimelinePositions(_data);
        }
        catch (Exception ex)
        {
            _error = $"Unable to load dashboard data: {ex.Message}";
        }
    }

    private void CalculateTimelinePositions(DashboardData data) { /* see Data Model */ }
}
```

### 3. `DashboardData.cs` — POCO Data Models

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Strongly-typed representation of `data.json` schema |
| **Interfaces** | Plain C# classes with nullable properties |
| **Dependencies** | None |
| **Data** | All dashboard state |

Single file containing all model classes (see Data Model section below).

### 4. `Dashboard.razor` — Page Composer

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Top-level page component. Injects `DashboardDataService`, renders error state or composes three sub-components |
| **Interfaces** | Blazor `@page "/"` route |
| **Dependencies** | `DashboardDataService`, `DashboardHeader`, `TimelineSection`, `HeatmapGrid` |
| **Data** | Receives `DashboardData` from service, passes slices to children via `[Parameter]` |

### 5. `DashboardHeader.razor` — Header Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render title, subtitle, ADO backlog link, and legend icons |
| **Interfaces** | `[Parameter] string Title`, `[Parameter] string? Subtitle`, `[Parameter] string? BacklogUrl`, `[Parameter] string? CurrentMonth` |
| **Dependencies** | None |
| **Data** | Read-only parameters from parent |
| **Visual mapping** | `.hdr` section of design reference |

### 6. `TimelineSection.razor` — SVG Timeline Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render inline SVG with milestone swimlanes, markers, gridlines, and NOW line |
| **Interfaces** | `[Parameter] List<Milestone> Milestones`, `[Parameter] TimelineLayout Layout` |
| **Dependencies** | None |
| **Data** | Pre-calculated pixel positions from `TimelineLayout` |
| **Visual mapping** | `.tl-area` section of design reference |

### 7. `HeatmapGrid.razor` — Heatmap Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render CSS Grid with status rows × month columns, current-month highlighting |
| **Interfaces** | `[Parameter] List<string> Months`, `[Parameter] int CurrentMonthIndex`, `[Parameter] List<StatusCategory> Categories` |
| **Dependencies** | None |
| **Data** | Category items keyed by month name |
| **Visual mapping** | `.hm-wrap` / `.hm-grid` section of design reference |

### 8. `dashboard.css` — Stylesheet

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | All visual styling, ported directly from `OriginalDesignConcept.html` |
| **Dependencies** | Segoe UI system font |
| **Data** | CSS custom properties for color theming |

### 9. `data.json` — Data File

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Single source of truth for all dashboard content |
| **Location** | `wwwroot/data/data.json` |
| **Format** | JSON, human-editable |

---

## Component Interactions

### Data Flow (single direction, no mutations)

```
┌─────────────┐     ┌──────────────────────┐     ┌─────────────────────┐
│  data.json  │────▶│ DashboardDataService  │────▶│   Dashboard.razor   │
│  (wwwroot)  │     │  • Deserialize JSON   │     │  • Error check      │
└─────────────┘     │  • Calculate SVG      │     │  • Compose children │
                    │    pixel positions    │     └──────┬──────────────┘
                    │  • Cache in memory    │            │
                    └──────────────────────┘            │ [Parameter] props
                                                        ├──────────────────┐
                                              ┌─────────▼──┐  ┌───────────▼────┐  ┌──────────▼───┐
                                              │ Dashboard   │  │  Timeline      │  │  Heatmap     │
                                              │ Header.razor│  │  Section.razor │  │  Grid.razor  │
                                              └─────────────┘  └────────────────┘  └──────────────┘
                                                        │               │                │
                                                        ▼               ▼                ▼
                                                   HTML/CSS         SVG elements     CSS Grid HTML
```

### Startup Sequence

1. `Program.cs` registers `DashboardDataService` as Singleton
2. `DashboardDataService` constructor reads `wwwroot/data/data.json` via `System.IO.File.ReadAllText`
3. JSON is deserialized into `DashboardData` POCO via `System.Text.Json`
4. Timeline pixel positions are pre-calculated and stored in `TimelineLayout`
5. On HTTP request to `/`, Blazor renders `Dashboard.razor` (Static SSR)
6. `Dashboard.razor` injects `DashboardDataService`, checks for errors
7. If error → renders styled error message
8. If success → renders `DashboardHeader`, `TimelineSection`, `HeatmapGrid` with `[Parameter]` data
9. Complete HTML response sent to browser in a single server render (no WebSocket)

### Communication Patterns

| Pattern | Used? | Notes |
|---------|-------|-------|
| **[Parameter] props** | ✅ | Parent-to-child data passing for all components |
| **CascadingValue** | ❌ | Unnecessary — only one level of nesting |
| **EventCallback** | ❌ | No user interactions that modify state |
| **SignalR** | ❌ | Static SSR mode — no WebSocket connection |
| **DI injection** | ✅ | Only `Dashboard.razor` injects `DashboardDataService` |
| **State management** | ❌ | Read-only dashboard — no mutable state |

---

## Data Model

### `data.json` Schema (v1)

```json
{
  "schemaVersion": 1,
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "currentMonth": "Apr",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonthIndex": 3,
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "timelineMonths": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"],
  "nowDate": "2026-04-17",
  "milestones": [
    {
      "id": "M1",
      "name": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        {
          "date": "2026-01-12",
          "label": "Jan 12",
          "type": "checkpoint",
          "labelPosition": "above"
        },
        {
          "date": "2026-03-26",
          "label": "Mar 26 PoC",
          "type": "poc",
          "labelPosition": "below"
        },
        {
          "date": "2026-05-01",
          "label": "Apr Prod (TBD)",
          "type": "production",
          "labelPosition": "above"
        }
      ]
    }
  ],
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["DSAR Chatbot v1 MVP", "MS Role auto-assign"],
        "Feb": ["PDS connector beta"],
        "Mar": ["Data Inventory scan"],
        "Apr": ["Review DFD engine"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": {
        "Jan": ["MS Role design"],
        "Feb": ["Chatbot prompt tuning"],
        "Mar": ["PDS perf testing"],
        "Apr": ["DFD edge cases"]
      }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": {
        "Jan": [],
        "Feb": ["MS Role auto-assign"],
        "Mar": ["Chatbot edge cases"],
        "Apr": ["PDS connector GA"]
      }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": {
        "Jan": [],
        "Feb": [],
        "Mar": ["AAD token refresh bug"],
        "Apr": ["Compliance API downtime"]
      }
    }
  ]
}
```

### C# POCO Classes (`Models/DashboardData.cs`)

All models in a single file to stay within the 5–8 file target:

```csharp
namespace ReportingDashboard.Models;

public class DashboardData
{
    public int SchemaVersion { get; set; } = 1;
    public string Title { get; set; } = "";
    public string? Subtitle { get; set; }
    public string? BacklogUrl { get; set; }
    public string? CurrentMonth { get; set; }
    public List<string> Months { get; set; } = new();
    public int CurrentMonthIndex { get; set; }
    public string? TimelineStartDate { get; set; }
    public string? TimelineEndDate { get; set; }
    public List<string> TimelineMonths { get; set; } = new();
    public string? NowDate { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<StatusCategory> Categories { get; set; } = new();

    // Pre-calculated by DashboardDataService (not from JSON)
    [System.Text.Json.Serialization.JsonIgnore]
    public TimelineLayout? Layout { get; set; }
}

public class Milestone
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Color { get; set; } = "#999";
    public List<MilestoneEvent> Events { get; set; } = new();
}

public class MilestoneEvent
{
    public string Date { get; set; } = "";
    public string Label { get; set; } = "";
    public string Type { get; set; } = "checkpoint"; // "checkpoint" | "poc" | "production" | "small-checkpoint"
    public string LabelPosition { get; set; } = "above"; // "above" | "below"
}

public class StatusCategory
{
    public string Name { get; set; } = "";
    public string CssClass { get; set; } = "";
    public Dictionary<string, List<string>> Items { get; set; } = new();
}

// Pre-calculated SVG positions — computed by DashboardDataService, not stored in JSON
public class TimelineLayout
{
    public double SvgWidth { get; set; } = 1560;
    public double SvgHeight { get; set; } = 185;
    public double NowX { get; set; }
    public List<MonthGridline> MonthGridlines { get; set; } = new();
    public List<SwimlaneLine> Swimlanes { get; set; } = new();
    public List<MarkerPosition> Markers { get; set; } = new();
}

public class MonthGridline
{
    public double X { get; set; }
    public string Label { get; set; } = "";
}

public class SwimlaneLine
{
    public double Y { get; set; }
    public string Color { get; set; } = "";
}

public class MarkerPosition
{
    public double X { get; set; }
    public double Y { get; set; }
    public string Type { get; set; } = "";      // checkpoint, poc, production, small-checkpoint
    public string Color { get; set; } = "";      // swimlane color (for checkpoint stroke)
    public string Label { get; set; } = "";
    public string LabelPosition { get; set; } = "above";
}
```

### Date-to-Pixel Mapping

The `DashboardDataService.CalculateTimelinePositions()` method converts dates to SVG x-coordinates:

```
x = ((date - timelineStartDate).TotalDays / (timelineEndDate - timelineStartDate).TotalDays) * 1560
```

Swimlane y-positions are evenly distributed:

```
y = svgHeight / (milestoneCount + 1) * (index + 1)
```

For 3 milestones in a 185px SVG: y = 42, 98, 154 (matching the reference design).

### Entity Relationships

```
DashboardData (root)
├── title, subtitle, backlogUrl, currentMonth, months[], currentMonthIndex
├── timelineStartDate, timelineEndDate, timelineMonths[], nowDate
├── Milestone[] (1:N)
│   ├── id, name, color
│   └── MilestoneEvent[] (1:N)
│       └── date, label, type, labelPosition
├── StatusCategory[] (exactly 4: Shipped, In Progress, Carryover, Blockers)
│   ├── name, cssClass
│   └── items: Dictionary<monthName, string[]>
└── TimelineLayout (computed, not persisted)
    ├── MonthGridline[] (computed)
    ├── SwimlaneLine[] (computed)
    └── MarkerPosition[] (computed)
```

### Storage

| Store | Technology | Location | Purpose |
|-------|-----------|----------|---------|
| `data.json` | Flat JSON file | `wwwroot/data/data.json` | All dashboard content |
| In-memory cache | C# Singleton | `DashboardDataService._data` | Deserialized + computed data |

No database. No ORM. No Entity Framework.

---

## API Contracts

### HTTP Endpoints

This application has exactly **one** endpoint:

| Method | Path | Response | Content-Type |
|--------|------|----------|-------------|
| `GET` | `/` | Full HTML page (1920×1080 dashboard) | `text/html` |

There are no REST APIs, no JSON endpoints, no GraphQL. The browser receives a complete HTML document rendered server-side.

### Static Files

| Path | Purpose |
|------|---------|
| `GET /css/dashboard.css` | Stylesheet |
| `GET /data/data.json` | Raw JSON (not used by app — served for debugging) |
| `GET /_framework/blazor.web.js` | Blazor framework JS (minimal in Static SSR mode) |

### Error Handling

| Scenario | Behavior |
|----------|----------|
| `data.json` missing | `DashboardDataService.GetError()` returns message; `Dashboard.razor` renders styled error div |
| `data.json` invalid JSON | Same — `JsonSerializer.Deserialize` throws, caught in `Load()`, error stored |
| Null/missing fields | C# nullable properties default gracefully: `backlogUrl` null → link hidden; `months` empty → no columns |
| `timelineStartDate` unparseable | `DateTime.TryParse` fails → timeline section renders without markers |

Error display format:
```html
<div style="padding:44px;font-family:'Segoe UI',Arial;color:#991B1B;font-size:16px;">
    <h2>Dashboard Error</h2>
    <p>Unable to load dashboard data. Check that wwwroot/data/data.json exists and contains valid JSON.</p>
    <pre style="color:#666;font-size:12px;">Detail: {error message}</pre>
</div>
```

---

## Infrastructure Requirements

### Hosting

| Requirement | Specification |
|-------------|--------------|
| **Runtime** | .NET 8 SDK (8.0+) |
| **Web Server** | Kestrel (built into .NET) |
| **Binding** | `http://localhost:5000` / `https://localhost:5001` |
| **Network** | Localhost only — not exposed to network |
| **OS** | Windows 10/11 (Segoe UI font dependency) |

### Networking

None. The application binds exclusively to `localhost`. No DNS, no TLS certificates (self-signed dev cert is optional), no reverse proxy, no load balancer.

### Storage

| Item | Size | Location |
|------|------|----------|
| Application binaries | ~5 MB | `bin/` (not committed) |
| `data.json` | < 10 KB | `wwwroot/data/` |
| `dashboard.css` | < 5 KB | `wwwroot/css/` |
| Total runtime memory | < 50 MB | In-process |

### CI/CD

None. This is a local utility. The workflow is:

```
git clone → dotnet run → open browser → edit data.json → refresh → screenshot → paste into PowerPoint
```

### Project File Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
└── src/
    └── ReportingDashboard/
        ├── ReportingDashboard.csproj          # 1. Project file (net8.0, no NuGet refs)
        ├── Program.cs                          # 2. Entry point + DI setup
        ├── Models/
        │   └── DashboardData.cs               # 3. All POCO classes
        ├── Services/
        │   └── DashboardDataService.cs        # 4. JSON loading + layout calc
        ├── Components/
        │   ├── App.razor                       # 5. Blazor root (html/head/body shell)
        │   ├── Pages/
        │   │   └── Dashboard.razor            # 6. Main page composer
        │   ├── DashboardHeader.razor          # 7. Header component
        │   ├── TimelineSection.razor          # 8. SVG timeline component
        │   └── HeatmapGrid.razor              # 9. CSS Grid heatmap component
        └── wwwroot/
            ├── css/
            │   └── dashboard.css              # 10. All styles
            └── data/
                └── data.json                  # 11. Dashboard data
```

**Source file count: 8** (Program.cs, DashboardData.cs, DashboardDataService.cs, App.razor, Dashboard.razor, DashboardHeader.razor, TimelineSection.razor, HeatmapGrid.razor) — plus CSS and JSON data files. Within the 5–8 file target.

---

## Technology Stack Decisions

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Framework** | .NET 8 Blazor Server | `net8.0` | Organizational mandate. Static SSR mode eliminates WebSocket overhead for this read-only use case |
| **Rendering mode** | Static SSR | .NET 8 built-in | No interactivity needed. Eliminates SignalR JS (~100KB) and persistent connection. Faster first paint |
| **JSON parsing** | `System.Text.Json` | Built into .NET 8 | Zero external dependency. Faster than Newtonsoft. Sufficient for flat JSON |
| **CSS layout** | Native CSS Grid + Flexbox | Browser-native | Direct port from `OriginalDesignConcept.html`. No framework overhead |
| **Timeline rendering** | Inline SVG via Razor | Browser-native | Exact match to reference design. No charting library needed — the SVG is hand-crafted with `<line>`, `<circle>`, `<polygon>`, `<text>` |
| **Font** | Segoe UI (system) | OS-provided | Design spec requires it. Pre-installed on all Windows machines. No web font loading |
| **Web server** | Kestrel | Built into .NET 8 | Default, zero-config, localhost-only |

### What was explicitly rejected

| Technology | Reason for rejection |
|-----------|---------------------|
| **MudBlazor / Radzen** | Adds complexity; design is custom CSS that doesn't map to component libraries |
| **Chart.js / Highcharts** | Timeline is a simple SVG — charting libraries are overkill and fight the pixel-perfect design |
| **Newtonsoft.Json** | External dependency; `System.Text.Json` is built-in and sufficient |
| **Entity Framework / SQLite** | No database needed. Data is a flat JSON file |
| **Bootstrap / Tailwind** | Fixed 1920×1080 layout. CSS frameworks add weight and fight the custom grid |
| **Blazor WebAssembly** | More complex setup, larger download, no server-side benefit for this use case |
| **Interactive Server mode** | Adds SignalR WebSocket unnecessarily. Static SSR is sufficient |

---

## Security Considerations

### Authentication & Authorization

**None.** This is a single-user, local-only tool. Adding authentication would violate the "zero operational overhead" business goal.

- No ASP.NET Identity
- No cookie authentication
- No JWT tokens
- No authorization policies
- No `[Authorize]` attributes

### Data Protection

| Concern | Assessment | Mitigation |
|---------|-----------|------------|
| **PII in `data.json`** | Low risk — contains project names and status, not personal data | None required |
| **Sensitive project names** | Possible if projects are confidential | Add `data.json` to `.gitignore` if needed |
| **Network exposure** | Kestrel binds to `localhost` only | No mitigation needed — not reachable from network |
| **HTTPS** | Optional for localhost | Dev cert auto-generated by .NET SDK |

### Input Validation

| Input | Validation |
|-------|-----------|
| `data.json` content | `System.Text.Json` rejects malformed JSON. Nullable C# properties handle missing fields. `DateTime.TryParse` handles bad dates |
| User HTTP requests | Only `GET /` is served. No form inputs, no query parameters, no POST bodies |
| File path | Hardcoded to `wwwroot/data/data.json` — no user-controlled path traversal |

### Supply Chain

Zero external NuGet packages. The only dependency is the .NET 8 SDK itself (Microsoft-published, LTS).

---

## Scaling Strategy

**Not applicable.** This is a single-user, single-machine, read-only local utility.

| Dimension | Status |
|-----------|--------|
| **Concurrent users** | 1 (the project lead) |
| **Data volume** | < 100 work items, < 10 milestones, < 12 months |
| **Request rate** | Manual browser refreshes — effectively 0 RPS |
| **Horizontal scaling** | N/A — runs on localhost |
| **Vertical scaling** | N/A — uses < 50MB RAM |
| **Caching** | Singleton in-memory cache of `data.json` — no cache invalidation needed since app restart handles updates |

If multi-project support is needed in the future, the simplest extension is a URL parameter (`?project=alpha`) that selects a different JSON file (`data-alpha.json`). This requires no architectural changes — just a conditional file path in `DashboardDataService`.

---

## UI Component Architecture

### Visual Section → Component Mapping

| Design Section | Component | CSS Classes | Layout Strategy | Data Bindings | Interactions |
|---------------|-----------|-------------|-----------------|---------------|-------------|
| **Header bar** (`.hdr`) | `DashboardHeader.razor` | `.hdr`, `.sub` | Flexbox row, `justify-content: space-between` | `@Title`, `@Subtitle`, `@BacklogUrl`, `@CurrentMonth` | ADO Backlog link (`<a href>`) |
| **Legend** (right side of `.hdr`) | Inline in `DashboardHeader.razor` | Inline styles matching reference | Flexbox row, `gap: 22px` | Static markup — legend items are fixed | None |
| **Timeline labels** (230px left column) | Inline in `TimelineSection.razor` | Inline styles | Flexbox column, `justify-content: space-around` | `@foreach milestone` → id, name, color | None |
| **Timeline SVG** (`.tl-svg-box`) | `TimelineSection.razor` | `.tl-area`, `.tl-svg-box` | Inline `<svg>` element, 1560×185px | `@foreach gridline`, `@foreach swimlane`, `@foreach marker` | None (future: `<title>` tooltips) |
| **Heatmap title** (`.hm-title`) | Inline in `HeatmapGrid.razor` | `.hm-title` | Block text | Static text | None |
| **Heatmap grid** (`.hm-grid`) | `HeatmapGrid.razor` | `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`, `.it` | CSS Grid: `160px repeat(N, 1fr)` / `36px repeat(4, 1fr)` | `@foreach month` → column headers; `@foreach category` → row + cells; `@foreach item` → `.it` divs | None |
| **Current month highlight** | Conditional CSS in `HeatmapGrid.razor` | `.apr-hdr` (dynamic class), `.apr` on cells | Same grid — class toggles change background colors | `currentMonthIndex` comparison per column | None |
| **Empty cells** | Conditional in `HeatmapGrid.razor` | `.it` with `color: #AAA` | Same grid cell | `@if (items.Count == 0)` → render dash | None |
| **Error state** | Conditional in `Dashboard.razor` | Inline styled `<div>` | Block layout, `padding: 44px` | `@if (error != null)` → show message | None |

### Dynamic CSS Grid Column Count

The heatmap grid column count is data-driven. `HeatmapGrid.razor` sets the `grid-template-columns` inline:

```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Months.Count, 1fr);">
```

### SVG Rendering Strategy in `TimelineSection.razor`

The component receives a `TimelineLayout` object with pre-calculated pixel coordinates. The Razor template contains zero math — only data binding:

```razor
<svg xmlns="http://www.w3.org/2000/svg" width="@Layout.SvgWidth" height="@Layout.SvgHeight"
     style="overflow:visible;display:block">
    <defs>
        <filter id="sh">
            <feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>
        </filter>
    </defs>

    @* Month gridlines *@
    @foreach (var grid in Layout.MonthGridlines)
    {
        <line x1="@grid.X" y1="0" x2="@grid.X" y2="@Layout.SvgHeight"
              stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
        <text x="@(grid.X + 5)" y="14" fill="#666" font-size="11"
              font-weight="600" font-family="Segoe UI,Arial">@grid.Label</text>
    }

    @* NOW line *@
    <line x1="@Layout.NowX" y1="0" x2="@Layout.NowX" y2="@Layout.SvgHeight"
          stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
    <text x="@(Layout.NowX + 4)" y="14" fill="#EA4335" font-size="10"
          font-weight="700" font-family="Segoe UI,Arial">NOW</text>

    @* Swimlane lines + markers rendered via @foreach *@
</svg>
```

### Marker Rendering by Type

```razor
@foreach (var m in Layout.Markers)
{
    @if (m.Type == "checkpoint")
    {
        <circle cx="@m.X" cy="@m.Y" r="7" fill="white" stroke="@m.Color" stroke-width="2.5"/>
    }
    else if (m.Type == "small-checkpoint")
    {
        <circle cx="@m.X" cy="@m.Y" r="4" fill="#999"/>
    }
    else if (m.Type == "poc")
    {
        <polygon points="@Diamond(m.X, m.Y, 11)" fill="#F4B400" filter="url(#sh)"/>
    }
    else if (m.Type == "production")
    {
        <polygon points="@Diamond(m.X, m.Y, 11)" fill="#34A853" filter="url(#sh)"/>
    }

    @* Label *@
    var ly = m.LabelPosition == "above" ? m.Y - 16 : m.Y + 20;
    <text x="@m.X" y="@ly" text-anchor="middle" fill="#666" font-size="10"
          font-family="Segoe UI,Arial">@m.Label</text>
}

@code {
    private string Diamond(double cx, double cy, double r)
        => $"{cx},{cy - r} {cx + r},{cy} {cx},{cy + r} {cx - r},{cy}";
}
```

---

## Risks & Mitigations

### Risk 1: Over-Engineering

| Aspect | Detail |
|--------|--------|
| **Risk** | Developers add repositories, CQRS, dependency injection abstractions, or service interfaces |
| **Likelihood** | High — natural instinct for .NET developers |
| **Impact** | Medium — adds files, complexity, and development time beyond the 5–8 hour budget |
| **Mitigation** | Architecture explicitly mandates: no interfaces, no repositories, no abstractions. `DashboardDataService` is the only service. Concrete class, registered directly. Code review should reject any `IDashboardDataService` interface |

### Risk 2: SVG Timeline Position Bugs

| Aspect | Detail |
|--------|--------|
| **Risk** | Date-to-pixel math produces incorrect marker positions or overlapping labels |
| **Likelihood** | Medium — date arithmetic and proportional scaling are error-prone |
| **Impact** | High — visual fidelity is the primary success metric |
| **Mitigation** | Pre-calculate ALL positions in `DashboardDataService` using the formula `x = (daysSinceStart / totalDays) * 1560`. Store as `double` pixel values in `TimelineLayout`. Razor templates only bind, never calculate. Validate against the reference SVG x-values (e.g., Jan gridline = 0, Feb = 260, Mar = 520, Apr = 780) |

### Risk 3: Blazor UI Chrome in Screenshots

| Aspect | Detail |
|--------|--------|
| **Risk** | Blazor connection indicator, reconnection banner, or loading spinner appears in screenshot |
| **Likelihood** | Medium if using Interactive Server mode; **Low if using Static SSR** |
| **Impact** | High — ruins screenshot fidelity |
| **Mitigation** | Use **Static SSR** (no `AddInteractiveServerComponents()`). This eliminates the SignalR connection entirely — no connection UI, no reconnection banner, no `blazor-error-ui`. Verify by checking that `blazor.server.js` is NOT loaded (only `blazor.web.js` for Static SSR) |

### Risk 4: CSS Grid Column Mismatch

| Aspect | Detail |
|--------|--------|
| **Risk** | Heatmap grid columns don't align when month count changes |
| **Likelihood** | Low — CSS Grid `repeat(N, 1fr)` handles this natively |
| **Impact** | Medium — visual layout breaks |
| **Mitigation** | Set `grid-template-columns` dynamically via inline style: `160px repeat(@Months.Count, 1fr)`. The CSS class `.hm-grid` defines rows; columns are always inline. Test with 4 months and 6 months |

### Risk 5: `data.json` Schema Drift

| Aspect | Detail |
|--------|--------|
| **Risk** | Users modify `data.json` in ways that break deserialization (wrong types, missing fields) |
| **Likelihood** | Medium — JSON is hand-edited by non-developers |
| **Impact** | Medium — dashboard shows error instead of data |
| **Mitigation** | All C# properties are nullable or have defaults. `schemaVersion` field enables future migrations. Error message in browser is clear and actionable. Include a well-commented sample `data.json` as the starting template |

### Risk 6: Content Overflow at 1920×1080

| Aspect | Detail |
|--------|--------|
| **Risk** | Too many work items or too many months cause content to overflow the fixed viewport |
| **Likelihood** | Medium — 6+ months with many items per cell could exceed vertical space |
| **Impact** | Medium — content clipped by `overflow: hidden`, invisible in screenshot |
| **Mitigation** | Document recommended limits in README: ≤ 6 months, ≤ 5 items per cell. Heatmap cells use `overflow: hidden` intentionally. If content exceeds space, the user must reduce items or abbreviate text. This is a known constraint of the fixed-viewport design |

### Risk 7: Stale Data After `data.json` Edit

| Aspect | Detail |
|--------|--------|
| **Risk** | User edits `data.json` but the Singleton cache still serves old data |
| **Likelihood** | High — Singleton caches on first access |
| **Impact** | Low — confusing but not breaking |
| **Mitigation** | Document in README: "Restart the app (`Ctrl+C` then `dotnet run`) after editing `data.json`." Alternatively, use `dotnet watch` which auto-restarts on file changes. Hot-reload via `FileSystemWatcher` is explicitly out of scope for v1 |