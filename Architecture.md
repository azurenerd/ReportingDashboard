# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server application that renders project milestone timelines and work-item status heatmaps at a fixed 1920×1080 resolution, optimized for PowerPoint screenshot capture. It runs exclusively on localhost, reads all data from a single `data.json` file, and has zero cloud dependencies, zero authentication, and zero third-party UI libraries.

**Architecture Goals:**

1. **Pixel-perfect visual fidelity** — Reproduce the `OriginalDesignConcept.html` design exactly using Blazor Razor components and a single CSS file ported verbatim from the reference.
2. **Data-driven rendering** — Every displayed string, milestone, and heatmap item originates from `data.json`; zero hardcoded content in Razor components.
3. **Minimal complexity** — Single `.csproj` (plus test project), zero NuGet dependencies in the main project, < 10 source files total.
4. **Instant feedback loop** — `dotnet watch` enables sub-second CSS/Razor hot reload; `data.json` changes reflect on browser refresh.
5. **Graceful degradation** — Missing or malformed data produces error messages, not crashes.

**Solution Structure:**

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Components/
│       │   ├── App.razor
│       │   ├── Pages/
│       │   │   └── Dashboard.razor
│       │   └── Layout/
│       │       ├── MainLayout.razor
│       │       ├── DashboardHeader.razor
│       │       ├── TimelineSection.razor
│       │       └── HeatmapGrid.razor
│       ├── Models/
│       │   └── DashboardData.cs
│       ├── Services/
│       │   └── DashboardDataService.cs
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css
│       │   └── data.json
│       └── Properties/
│           └── launchSettings.json
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        ├── Models/
        │   └── DashboardDataTests.cs
        ├── Services/
        │   └── DashboardDataServiceTests.cs
        └── Components/
            ├── DashboardHeaderTests.cs
            ├── TimelineSectionTests.cs
            └── HeatmapGridTests.cs
```

---

## System Components

### 1. `Program.cs` — Application Entry Point

**Responsibility:** Configure and start the Blazor Server application with minimal middleware.

**Interfaces:** None (entry point only).

**Dependencies:** `DashboardDataService` (registered in DI), ASP.NET Core built-in middleware.

**Key Implementation:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register the data service as a singleton — data is loaded once and cached
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

**Configuration (`appsettings.json`):**

```json
{
  "Dashboard": {
    "DataFilePath": "wwwroot/data.json"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

The `DataFilePath` is configurable so users can point to a `data.json` outside `wwwroot` if desired.

---

### 2. `DashboardDataService` — Data Loading Service

**Responsibility:** Read, deserialize, validate, and cache `data.json`. Provide the deserialized `DashboardData` to Razor components via dependency injection.

**Interface:**

```csharp
public class DashboardDataService
{
    public Task<DashboardData?> GetDashboardDataAsync();
    public string? ErrorMessage { get; }
}
```

**Dependencies:** `IWebHostEnvironment` (for `wwwroot` path resolution), `IConfiguration` (for custom data path), `System.Text.Json`, `System.IO`.

**Behavior:**

- On first call to `GetDashboardDataAsync()`, reads the file from disk, deserializes with `System.Text.Json`, and caches the result in a private field.
- If the file is missing, sets `ErrorMessage` to `"Unable to load dashboard data. File not found: {path}"` and returns `null`.
- If the JSON is malformed, sets `ErrorMessage` to `"Unable to load dashboard data. Invalid JSON in {path}: {exception.Message}"` and returns `null`.
- Uses `JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }` for resilient deserialization.
- Data is re-read on each request (no aggressive caching) so that `data.json` edits are reflected on browser refresh. The file is < 100KB, so re-reading on each page load is negligible (< 1ms).

**Detailed Implementation:**

```csharp
public class DashboardDataService
{
    private readonly string _dataFilePath;
    public string? ErrorMessage { get; private set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DashboardDataService(IWebHostEnvironment env, IConfiguration config)
    {
        var configPath = config.GetValue<string>("Dashboard:DataFilePath");
        _dataFilePath = configPath != null
            ? Path.IsPathRooted(configPath) ? configPath : Path.Combine(env.ContentRootPath, configPath)
            : Path.Combine(env.WebRootPath, "data.json");
    }

    public async Task<DashboardData?> GetDashboardDataAsync()
    {
        ErrorMessage = null;

        if (!File.Exists(_dataFilePath))
        {
            ErrorMessage = $"Unable to load dashboard data. File not found: {_dataFilePath}";
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_dataFilePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (data is null)
            {
                ErrorMessage = "Unable to load dashboard data. The file deserialized to null.";
                return null;
            }

            return data;
        }
        catch (JsonException ex)
        {
            ErrorMessage = $"Unable to load dashboard data. Invalid JSON: {ex.Message}";
            return null;
        }
        catch (IOException ex)
        {
            ErrorMessage = $"Unable to load dashboard data. File read error: {ex.Message}";
            return null;
        }
    }
}
```

---

### 3. `DashboardData.cs` — Data Model (C# Records)

**Responsibility:** Define the strongly-typed schema for `data.json`. Serves as both the deserialization target and the living documentation of the data contract.

**Full Model:**

```csharp
namespace ReportingDashboard.Models;

public record DashboardData
{
    public ProjectInfo Project { get; init; } = new();
    public TimelineConfig Timeline { get; init; } = new();
    public HeatmapData Heatmap { get; init; } = new();
}

public record ProjectInfo
{
    public string Title { get; init; } = "[No title]";
    public string Subtitle { get; init; } = "";
    public string BacklogUrl { get; init; } = "#";
    public string BacklogLinkText { get; init; } = "→ ADO Backlog";
}

public record TimelineConfig
{
    public string StartDate { get; init; } = "2026-01-01";
    public string EndDate { get; init; } = "2026-06-30";
    public string? NowDate { get; init; }  // null = use DateTime.Now
    public List<TimelineTrack> Tracks { get; init; } = new();
}

public record TimelineTrack
{
    public string Id { get; init; } = "";
    public string Label { get; init; } = "";
    public string Color { get; init; } = "#0078D4";
    public List<Milestone> Milestones { get; init; } = new();
}

public record Milestone
{
    public string Label { get; init; } = "";
    public string Date { get; init; } = "";
    public string Type { get; init; } = "checkpoint"; // "poc", "production", "checkpoint", "checkpoint-small"
    public string? LabelPosition { get; init; }        // "above" or "below"; null = auto
}

public record HeatmapData
{
    public string SectionTitle { get; init; } = "Monthly Execution Heatmap";
    public List<string> Columns { get; init; } = new();       // e.g., ["Jan", "Feb", "Mar", "Apr"]
    public string HighlightColumn { get; init; } = "";         // e.g., "Apr"
    public List<HeatmapRow> Rows { get; init; } = new();
}

public record HeatmapRow
{
    public string Category { get; init; } = "";                 // "Shipped", "In Progress", "Carryover", "Blockers"
    public string Emoji { get; init; } = "";                    // "✅", "🔄", "📋", "🚫"
    public string Theme { get; init; } = "ship";                // "ship", "prog", "carry", "block"
    public Dictionary<string, List<string>> Items { get; init; } = new();
}
```

**Design Decisions:**

- All properties have default values (`= ""`, `= new()`) so missing optional fields in `data.json` produce graceful defaults rather than `NullReferenceException`.
- `NowDate` is nullable: when `null`, the timeline uses `DateTime.Now`; when set (e.g., `"2026-04-10"`), it uses the specified date, enabling historical snapshot generation.
- `Milestone.LabelPosition` allows explicit control over label placement; when `null`, the component auto-selects based on collision avoidance logic.
- `HeatmapRow.Theme` maps directly to CSS class prefixes (`.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`).

---

### 4. `App.razor` — Blazor Root Component

**Responsibility:** HTML document shell with `<head>` and `<body>`, linking `dashboard.css`.

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <base href="/" />
    <link rel="stylesheet" href="css/dashboard.css" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

**Key:** No `app.css` or `bootstrap.css`. The only stylesheet is `dashboard.css`, ported from the design reference.

---

### 5. `MainLayout.razor` — Layout Wrapper

**Responsibility:** Minimal layout that renders the page body with no navigation chrome.

```razor
@inherits LayoutComponentBase
@Body
```

No `<nav>`, no sidebar, no footer. The dashboard is the only page.

---

### 6. `Dashboard.razor` — Page Composition Component

**Responsibility:** Load data from `DashboardDataService`, handle error states, and compose the three visual sections.

**Route:** `@page "/"`

**Dependencies:** `DashboardDataService` (injected).

**Behavior:**

- On initialization, calls `GetDashboardDataAsync()`.
- If data is `null`, renders a centered error message using the service's `ErrorMessage`.
- If data is valid, renders `DashboardHeader`, `TimelineSection`, and `HeatmapGrid` in vertical flex order, passing the relevant data slices as component parameters.

```razor
@page "/"
@inject DashboardDataService DataService

@if (_data is null)
{
    <div class="error-state">
        <p>@(DataService.ErrorMessage ?? "Unable to load dashboard data. Please check data.json.")</p>
    </div>
}
else
{
    <DashboardHeader Project="_data.Project" />
    <TimelineSection Timeline="_data.Timeline" />
    <HeatmapGrid Heatmap="_data.Heatmap" />
}

@code {
    private DashboardData? _data;

    protected override async Task OnInitializedAsync()
    {
        _data = await DataService.GetDashboardDataAsync();
    }
}
```

---

### 7. `DashboardHeader.razor` — Header Component

**Responsibility:** Render the project title, subtitle, ADO backlog link, and milestone legend.

**Parameters:**

```csharp
[Parameter] public ProjectInfo Project { get; set; }
```

**Visual Mapping → `OriginalDesignConcept.html` `.hdr` section:**

| Design Element | Data Binding | CSS Class |
|---|---|---|
| Project title (`<h1>`) | `Project.Title` | `.hdr h1` |
| Backlog link (`<a>`) | `Project.BacklogUrl`, `Project.BacklogLinkText` | `a` (color: `#0078D4`) |
| Subtitle | `Project.Subtitle` | `.sub` |
| Legend: PoC diamond | Static icon + "PoC Milestone" | Inline flex, `#F4B400` rotated square |
| Legend: Production diamond | Static icon + "Production Release" | Inline flex, `#34A853` rotated square |
| Legend: Checkpoint circle | Static icon + "Checkpoint" | Inline flex, `#999` circle |
| Legend: Now indicator | Static icon + "Now ({current month})" | Inline flex, `#EA4335` bar |

**Layout:** Flexbox row (`justify-content: space-between`), left side has title/subtitle, right side has legend. Padding `12px 44px 10px`. Bottom border `1px solid #E0E0E0`.

---

### 8. `TimelineSection.razor` — SVG Timeline Component

**Responsibility:** Render the track label panel and the SVG timeline with dynamic milestone positioning.

**Parameters:**

```csharp
[Parameter] public TimelineConfig Timeline { get; set; }
```

**Sub-elements:**

#### Track Label Panel (HTML, left side)
- 230px fixed-width flex column
- Iterates `Timeline.Tracks` and renders each track's `Id`, `Label` in the track's `Color`

#### SVG Canvas (right side)
- Fixed dimensions: `width="1560" height="185"`
- Contains `<defs>` with drop shadow filter

**Date-to-X Mapping Algorithm:**

```csharp
private double DateToX(DateTime date)
{
    var start = DateTime.Parse(Timeline.StartDate);
    var end = DateTime.Parse(Timeline.EndDate);
    var totalDays = (end - start).TotalDays;
    var elapsed = (date - start).TotalDays;
    return Math.Clamp(elapsed / totalDays * SvgWidth, 0, SvgWidth);
}

private const double SvgWidth = 1560.0;
```

**SVG Elements Rendered (in order):**

1. **Month grid lines** — Calculate the 1st of each month within the date range; render vertical `<line>` at the computed X position with `<text>` label.
2. **NOW line** — Parse `Timeline.NowDate ?? DateTime.Now.ToString("yyyy-MM-dd")`; render dashed red `<line>` and "NOW" `<text>`.
3. **Track lines** — For each track (index `i`), render horizontal `<line>` at `y = 42 + (i * 56)` spanning full width in the track's color.
4. **Milestones** — For each track, iterate milestones. Compute X from date. Render:
   - `type == "poc"` → `<polygon>` diamond, fill `#F4B400`, with shadow filter
   - `type == "production"` → `<polygon>` diamond, fill `#34A853`, with shadow filter
   - `type == "checkpoint"` → `<circle>` r=7, white fill, track-color stroke
   - `type == "checkpoint-small"` → `<circle>` r=4, fill `#999`
5. **Date labels** — `<text>` at 10px, `text-anchor: middle`, positioned above or below track line.

**Label Collision Avoidance:**

```csharp
private string GetLabelPosition(Milestone milestone, int trackIndex, List<Milestone> allTrackMilestones)
{
    if (milestone.LabelPosition != null)
        return milestone.LabelPosition;  // Explicit override from data.json

    var trackY = 42 + (trackIndex * 56);
    var thisX = DateToX(DateTime.Parse(milestone.Date));

    // Check for nearby milestones (within 60px)
    var nearbyAbove = allTrackMilestones
        .Where(m => m != milestone && Math.Abs(DateToX(DateTime.Parse(m.Date)) - thisX) < 60
                     && (m.LabelPosition == "above" || m.LabelPosition == null))
        .Any();

    return nearbyAbove ? "below" : "above";
}
```

Labels default to "above" the track line (`y = trackY - 16`). If a nearby milestone already has a label above, the current milestone's label shifts below (`y = trackY + 24`). This is a simple greedy algorithm sufficient for 3-10 milestones per track.

---

### 9. `HeatmapGrid.razor` — Heatmap Grid Component

**Responsibility:** Render the CSS Grid heatmap with dynamic columns, rows, and highlighted current month.

**Parameters:**

```csharp
[Parameter] public HeatmapData Heatmap { get; set; }
```

**Rendering Logic:**

1. **Section title** — Render `Heatmap.SectionTitle` in `.hm-title`.
2. **Dynamic grid columns** — Set `grid-template-columns: 160px repeat({Heatmap.Columns.Count}, 1fr)` via inline style.
3. **Dynamic grid rows** — Set `grid-template-rows: 36px repeat({Heatmap.Rows.Count}, 1fr)` via inline style.
4. **Corner cell** — Render "STATUS" in `.hm-corner`.
5. **Column headers** — For each column name in `Heatmap.Columns`, render `.hm-col-hdr`. If column name matches `Heatmap.HighlightColumn`, add the `.highlight-hdr` class (styled like `.apr-hdr`).
6. **Row headers** — For each row, render `.hm-row-hdr` with the category's emoji and name, applying theme class (e.g., `.ship-hdr`).
7. **Data cells** — For each row × column combination:
   - Look up `row.Items[columnName]`. If the key exists and the list is non-empty, render each item as `<div class="it">{item}</div>`.
   - If the key doesn't exist or the list is empty, render `<span style="color:#AAA">-</span>`.
   - Apply theme class (e.g., `.ship-cell`) and highlight class if column matches `HighlightColumn`.

**CSS Class Mapping:**

| `row.Theme` | Row Header Class | Cell Class | Highlight Cell Class |
|---|---|---|---|
| `"ship"` | `.ship-hdr` | `.ship-cell` | `.ship-cell.highlight` |
| `"prog"` | `.prog-hdr` | `.prog-cell` | `.prog-cell.highlight` |
| `"carry"` | `.carry-hdr` | `.carry-cell` | `.carry-cell.highlight` |
| `"block"` | `.block-hdr` | `.block-cell` | `.block-cell.highlight` |

The `.highlight` class replaces the hardcoded `.apr` class from the design reference, making it data-driven. In `dashboard.css`, the highlight styles are applied via a combined selector (e.g., `.ship-cell.highlight { background: #D8F2DA; }`).

---

### 10. `dashboard.css` — Stylesheet

**Responsibility:** All visual styling, ported directly from `OriginalDesignConcept.html` `<style>` block.

**Strategy:** Copy the CSS verbatim from the design reference, then make these modifications:

1. Replace `.apr` class with `.highlight` class for data-driven month highlighting.
2. Replace `.apr-hdr` with `.highlight-hdr`.
3. Add `.error-state` styles for the error message display.
4. No CSS isolation (`.razor.css` files). One global stylesheet.

**Error State CSS:**

```css
.error-state {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 1920px;
    height: 1080px;
    font-size: 18px;
    color: #991B1B;
    background: #FEF2F2;
    font-family: 'Segoe UI', Arial, sans-serif;
}
```

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│  Browser (localhost:5000)                                        │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Dashboard.razor  (@page "/")                              │  │
│  │    │                                                       │  │
│  │    │ OnInitializedAsync()                                  │  │
│  │    ▼                                                       │  │
│  │  DashboardDataService.GetDashboardDataAsync()              │  │
│  │    │                                                       │  │
│  │    │ File.ReadAllTextAsync() → JsonSerializer.Deserialize  │  │
│  │    ▼                                                       │  │
│  │  ┌──────────────┐                                          │  │
│  │  │ data.json    │  (wwwroot/data.json or configured path)  │  │
│  │  └──────────────┘                                          │  │
│  │    │                                                       │  │
│  │    ▼  DashboardData (C# record)                            │  │
│  │    │                                                       │  │
│  │    ├──→ DashboardHeader.razor  ← Project property          │  │
│  │    │      (renders .hdr section)                           │  │
│  │    │                                                       │  │
│  │    ├──→ TimelineSection.razor  ← Timeline property         │  │
│  │    │      (renders .tl-area + SVG)                         │  │
│  │    │                                                       │  │
│  │    └──→ HeatmapGrid.razor     ← Heatmap property          │  │
│  │           (renders .hm-wrap + CSS Grid)                    │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Communication Pattern

- **Unidirectional data flow:** `data.json` → `DashboardDataService` → `Dashboard.razor` → child components. No callbacks, no events, no two-way binding.
- **Component parameters:** Each child component receives a single strongly-typed parameter (`ProjectInfo`, `TimelineConfig`, `HeatmapData`). No cascading values, no shared state beyond the DI-registered service.
- **No inter-component communication:** The three visual sections are independent. `DashboardHeader` does not communicate with `TimelineSection` or `HeatmapGrid`.
- **Blazor Server SignalR circuit:** Exists by default but is functionally irrelevant. The page is static after initial render. No user interactions trigger server-side updates (the backlog link is a plain `<a>` tag with `target="_blank"`).

### Request Lifecycle

1. User navigates to `http://localhost:5000`
2. Kestrel receives HTTP request, routes to Blazor Server middleware
3. Blazor Server renders `App.razor` → `MainLayout.razor` → `Dashboard.razor`
4. `Dashboard.razor.OnInitializedAsync()` calls `DashboardDataService.GetDashboardDataAsync()`
5. Service reads `data.json` from disk, deserializes to `DashboardData`
6. `Dashboard.razor` passes data slices to child components as parameters
7. Child components render HTML/SVG using the data
8. Complete HTML is sent to the browser over the SignalR circuit
9. Browser renders at 1920×1080; page is screenshot-ready

**Total time: < 2 seconds** (dominated by .NET cold start; subsequent navigations < 100ms).

---

## Data Model

### Entity Relationship

```
DashboardData (root)
├── ProjectInfo (1:1)
│     ├── Title: string
│     ├── Subtitle: string
│     ├── BacklogUrl: string
│     └── BacklogLinkText: string
├── TimelineConfig (1:1)
│     ├── StartDate: string (ISO date)
│     ├── EndDate: string (ISO date)
│     ├── NowDate: string? (ISO date, nullable)
│     └── Tracks: TimelineTrack[] (1:N)
│           ├── Id: string
│           ├── Label: string
│           ├── Color: string (hex)
│           └── Milestones: Milestone[] (1:N)
│                 ├── Label: string
│                 ├── Date: string (ISO date)
│                 ├── Type: string (enum: "poc"|"production"|"checkpoint"|"checkpoint-small")
│                 └── LabelPosition: string? ("above"|"below"|null)
└── HeatmapData (1:1)
      ├── SectionTitle: string
      ├── Columns: string[] (month names)
      ├── HighlightColumn: string
      └── Rows: HeatmapRow[] (1:N)
            ├── Category: string
            ├── Emoji: string
            ├── Theme: string (enum: "ship"|"prog"|"carry"|"block")
            └── Items: Dictionary<string, string[]> (month → work items)
```

### Storage

- **Format:** Single `data.json` file (flat file, no database)
- **Location:** `wwwroot/data.json` by default, configurable via `appsettings.json` key `Dashboard:DataFilePath`
- **Size:** Expected < 10KB for typical use; tested up to 100KB
- **Encoding:** UTF-8
- **Schema enforcement:** C# record types with default values; no JSON Schema validation file (the C# types _are_ the schema)

### Example `data.json`

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
    "endDate": "2026-06-30",
    "nowDate": null,
    "tracks": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "label": "Jan 12", "date": "2026-01-12", "type": "checkpoint" },
          { "label": "Mar 26 PoC", "date": "2026-03-26", "type": "poc", "labelPosition": "below" },
          { "label": "Apr Prod (TBD)", "date": "2026-05-01", "type": "production", "labelPosition": "above" }
        ]
      },
      {
        "id": "M2",
        "label": "PDS & Data Inventory",
        "color": "#00897B",
        "milestones": [
          { "label": "Dec 19", "date": "2025-12-19", "type": "checkpoint" },
          { "label": "Feb 11", "date": "2026-02-11", "type": "checkpoint" },
          { "label": "Mar 20 PoC", "date": "2026-03-20", "type": "poc" },
          { "label": "May Prod", "date": "2026-05-15", "type": "production" }
        ]
      },
      {
        "id": "M3",
        "label": "Auto Review DFD",
        "color": "#546E7A",
        "milestones": [
          { "label": "Apr PoC", "date": "2026-04-15", "type": "poc" },
          { "label": "Jun Prod", "date": "2026-06-15", "type": "production" }
        ]
      }
    ]
  },
  "heatmap": {
    "sectionTitle": "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers",
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "highlightColumn": "Apr",
    "rows": [
      {
        "category": "Shipped",
        "emoji": "✅",
        "theme": "ship",
        "items": {
          "Jan": ["Privacy chatbot v0.1 MVP", "MS role schema draft"],
          "Feb": ["PDS connector alpha", "Data classification rules"],
          "Mar": ["Chatbot Azure OpenAI integration", "PDS API v1 endpoints"],
          "Apr": ["MS Role assignment engine", "Privacy review template v2"]
        }
      },
      {
        "category": "In Progress",
        "emoji": "🔄",
        "theme": "prog",
        "items": {
          "Jan": ["PDS connector design", "Data inventory schema"],
          "Feb": ["Chatbot prompt engineering", "Auto review DFD research"],
          "Mar": ["MS role integration testing", "PDS data sync pipeline"],
          "Apr": ["DFD auto-generation engine", "Chatbot multi-turn memory"]
        }
      },
      {
        "category": "Carryover",
        "emoji": "📋",
        "theme": "carry",
        "items": {
          "Jan": [],
          "Feb": ["Privacy chatbot error handling"],
          "Mar": ["PDS connector retry logic"],
          "Apr": ["Data inventory gap analysis", "PDS perf optimization"]
        }
      },
      {
        "category": "Blockers",
        "emoji": "🚫",
        "theme": "block",
        "items": {
          "Jan": [],
          "Feb": [],
          "Mar": ["Azure OpenAI quota approval"],
          "Apr": ["DFD schema spec pending legal review"]
        }
      }
    ]
  }
}
```

---

## API Contracts

This application has **no REST API, no GraphQL endpoint, and no programmatic interface**. The only "contract" is between `data.json` and the C# data model.

### Data Contract: `data.json` ↔ C# Records

**Serialization Settings:**

```csharp
new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
}
```

**Required Fields (will show defaults if missing):**

| JSON Path | C# Type | Default if Missing | Behavior |
|---|---|---|---|
| `project.title` | `string` | `"[No title]"` | Renders placeholder text |
| `project.subtitle` | `string` | `""` | Subtitle area is empty |
| `timeline.startDate` | `string` | `"2026-01-01"` | Uses default date range |
| `timeline.endDate` | `string` | `"2026-06-30"` | Uses default date range |
| `timeline.tracks` | `List<TimelineTrack>` | `[]` | Timeline renders with no tracks |
| `heatmap.columns` | `List<string>` | `[]` | Grid renders with no data columns |
| `heatmap.rows` | `List<HeatmapRow>` | `[]` | Grid renders with no data rows |

**Error Handling:**

| Condition | Response |
|---|---|
| `data.json` file not found | Full-page error: "Unable to load dashboard data. File not found: {path}" |
| Invalid JSON syntax | Full-page error: "Unable to load dashboard data. Invalid JSON: {details}" |
| File I/O error | Full-page error: "Unable to load dashboard data. File read error: {details}" |
| Missing optional fields | Graceful defaults (see table above) |
| Unknown JSON properties | Silently ignored (default `System.Text.Json` behavior) |
| Date parsing failure in milestone | Milestone is skipped; no crash. Service logs a warning. |

### Internal Component Contracts (Blazor Parameters)

| Component | Parameter | Type | Required |
|---|---|---|---|
| `DashboardHeader` | `Project` | `ProjectInfo` | Yes |
| `TimelineSection` | `Timeline` | `TimelineConfig` | Yes |
| `HeatmapGrid` | `Heatmap` | `HeatmapData` | Yes |

These are compile-time contracts enforced by Blazor's `[Parameter]` attribute.

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|---|---|
| **Runtime** | .NET 8.0 SDK (build) or .NET 8.0 Runtime (published executable) |
| **OS** | Windows 10+ (primary), macOS 12+, Linux (Ubuntu 20.04+) |
| **Memory** | < 100MB at idle |
| **Disk** | < 50MB (published self-contained), < 5MB (framework-dependent) |
| **Network** | None required after `dotnet restore`. Binds to `localhost` only. |
| **Browser** | Chrome 90+ (primary, for DevTools screenshots), Edge, Firefox |
| **Font** | Segoe UI (pre-installed on Windows; may need manual install on macOS/Linux) |

### Hosting

| Concern | Configuration |
|---|---|
| **Web server** | Kestrel (built-in, no IIS/Nginx) |
| **Binding** | `http://localhost:5000` (configurable in `appsettings.json` or `launchSettings.json`) |
| **HTTPS** | Not required for localhost; optionally `https://localhost:5001` |
| **Static files** | Served from `wwwroot/` via `app.UseStaticFiles()` |
| **Reverse proxy** | Not needed |
| **Docker** | Not needed |

### Build & Publish

```bash
# Development
dotnet watch --project src/ReportingDashboard

# Build
dotnet build ReportingDashboard.sln

# Test
dotnet test ReportingDashboard.sln

# Publish (self-contained, single file)
dotnet publish src/ReportingDashboard/ReportingDashboard.csproj \
  -c Release -r win-x64 --self-contained -o ./publish
```

### CI/CD

Not required for this scope. The build/test commands above can be added to any CI system if desired in the future.

### `launchSettings.json`

```json
{
  "profiles": {
    "ReportingDashboard": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### Project Files

**`ReportingDashboard.csproj`** — Zero third-party dependencies:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**`ReportingDashboard.Tests.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="bunit" Version="1.31.3" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\ReportingDashboard\ReportingDashboard.csproj" />
  </ItemGroup>
</Project>
```

**`ReportingDashboard.sln`:**

```
Solution structure:
├── src/ReportingDashboard/ReportingDashboard.csproj
└── tests/ReportingDashboard.Tests/ReportingDashboard.Tests.csproj
```

---

## Technology Stack Decisions

| Layer | Choice | Alternatives Rejected | Justification |
|---|---|---|---|
| **UI Framework** | Blazor Server (.NET 8 built-in) | Blazor WASM, Blazor Static SSR, MVC Razor Pages | Server-side rendering provides `dotnet watch` hot reload. WASM adds download overhead. Static SSR lacks the interactive server component model. |
| **CSS Approach** | Single global `dashboard.css` | CSS isolation (`.razor.css`), Tailwind, SASS | Design is page-level; CSS isolation adds complexity for no benefit. The CSS is ported 1:1 from the HTML reference. No build toolchain needed. |
| **Timeline Rendering** | Hand-crafted SVG in Razor | BlazorApexCharts, ChartJs.Blazor, Plotly | No charting library provides diamond milestones, dashed NOW lines, and positioned labels. The SVG is ~30 elements. A library would add 500KB+ and still require custom SVG for the exact design. |
| **Heatmap Rendering** | CSS Grid in Razor | HTML `<table>`, MudBlazor DataGrid | CSS Grid matches the design reference exactly. `<table>` semantics don't map to this layout. UI libraries impose their own styles. |
| **JSON Parsing** | `System.Text.Json` (built-in) | Newtonsoft.Json | Built into .NET 8; no NuGet dependency. Performance is superior for this use case. `Newtonsoft.Json` would be the only third-party dependency and is unnecessary. |
| **Data Storage** | `data.json` flat file | SQLite, LiteDB, EF Core | Data volume is < 100 items. Updates are manual (text editor). No queries, no concurrent writes, no relationships. A database would add complexity for zero benefit. |
| **Data Model** | C# `record` types | `class` types, `dynamic`, `JsonDocument` | Records provide immutability, value equality, and serve as living schema documentation. `init`-only properties prevent accidental mutation. |
| **DI Lifetime** | Singleton `DashboardDataService` | Scoped, Transient | Single-user local app; one service instance is sufficient. Data is re-read from disk on each call (no stale cache). |
| **Testing** | xUnit + bUnit + FluentAssertions | NUnit, MSTest, Playwright | xUnit is the .NET standard. bUnit provides Blazor component testing without a browser. FluentAssertions improves test readability. |
| **Third-party UI Library** | None | MudBlazor, Radzen, Syncfusion | The design is custom. UI libraries impose CSS that conflicts with the pixel-perfect reference design. Raw HTML/CSS is simpler and faster for < 10 components. |
| **JavaScript** | None (zero JS) | JS interop for tooltips, animations | PM spec explicitly prohibits custom JavaScript. All rendering is server-side Razor/SVG. |

---

## Security Considerations

### Authentication & Authorization

**None required.** The application binds to `localhost` only and is accessed by the person running it on their machine. No user accounts, no roles, no tokens, no cookies, no middleware.

**If future requirements add network sharing:**
1. Change Kestrel binding from `localhost` to `0.0.0.0`
2. Add a shared secret in `appsettings.json` checked via custom middleware
3. This is explicitly out of scope and should not be pre-built

### Data Protection

- `data.json` contains project status metadata (milestone names, work items). No PII, no credentials, no secrets.
- If sensitive data is ever placed in `data.json`, use Windows DPAPI via `Microsoft.AspNetCore.DataProtection` (built into .NET 8) for at-rest encryption.
- `data.json` is **not** served as a static file to the browser — it is read server-side by `DashboardDataService`. However, since it resides in `wwwroot/`, it _is_ accessible via `http://localhost:5000/data.json`. To prevent this (defense in depth), add a static file filter:

```csharp
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.Equals("data.json", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.StatusCode = 404;
            ctx.Context.Response.ContentLength = 0;
            ctx.Context.Response.Body = Stream.Null;
        }
    }
});
```

Alternatively, move `data.json` outside `wwwroot/` and configure the path via `appsettings.json`.

### Input Validation

- `data.json` is the only input. It is deserialized into strongly-typed records. Unknown properties are ignored. Missing properties receive safe defaults.
- Date strings are parsed with `DateTime.Parse()` wrapped in try/catch. Invalid dates cause the milestone to be skipped, not a crash.
- No user-provided input reaches the browser (no forms, no query parameters, no POST endpoints). The only rendered content comes from `data.json`.
- SVG rendering uses Blazor's built-in HTML encoding. Even if `data.json` contained `<script>` tags in a label, Blazor would encode them as `&lt;script&gt;`.

### Network Exposure

- Kestrel binds to `localhost:5000` only. The application is not accessible from other machines on the network.
- No outbound network calls are made. The application is fully offline-capable after `dotnet restore`.

---

## Scaling Strategy

This application **does not need to scale**. It is a single-user, local-only tool that renders a single page from a file on disk.

### Scaling Dimensions (and why they don't apply)

| Dimension | Current State | Scaling Path (if ever needed) |
|---|---|---|
| **Concurrent users** | 1 (the person running `dotnet run`) | Not applicable. If needed: Kestrel handles 10K+ concurrent connections out of the box. |
| **Data volume** | < 100 items, < 100KB JSON | Not a concern. `System.Text.Json` deserializes 100KB in < 1ms. If data grew to 10MB (unlikely), streaming deserialization could be added. |
| **Page complexity** | ~50 SVG elements + ~20 grid cells | Well within browser rendering limits. No virtualization needed. |
| **Multiple projects** | 1 `data.json` = 1 project | If multi-project is needed, accept a query parameter `?data=project2.json` and resolve the file path in `DashboardDataService`. No architectural changes needed. |
| **Multiple instances** | Not applicable | Each user runs their own `dotnet run`. No shared state. |

### Performance Budget

| Operation | Budget | Actual (estimated) |
|---|---|---|
| .NET cold start | < 2000ms | ~800ms |
| `data.json` read + deserialize | < 50ms | < 5ms |
| Blazor component render | < 100ms | < 20ms |
| SVG render (browser) | < 50ms | < 10ms |
| **Total first load** | **< 2000ms** | **~900ms** |

---

## Risks & Mitigations

| # | Risk | Severity | Likelihood | Mitigation |
|---|---|---|---|---|
| 1 | **SVG label collision** — Milestones with close dates produce overlapping date labels, making them unreadable | Medium | High (will happen with real data) | Implement label collision avoidance: alternate labels above/below track lines. Allow explicit `labelPosition` override in `data.json` for manual control. |
| 2 | **CSS drift from design reference** — Engineers modify `dashboard.css` and diverge from the `OriginalDesignConcept.html` design | Medium | Medium | Port CSS verbatim from the HTML reference. Include the reference file in the repo. Add a visual regression test note: screenshot comparison before/after changes. |
| 3 | **Blazor Server SignalR disconnection** — Browser shows "Attempting to reconnect" overlay | Low | Low (localhost only) | Irrelevant for localhost-to-localhost. If it occurs, browser refresh resolves it. No mitigation needed. |
| 4 | **JSON schema drift** — `data.json` structure changes but C# records are not updated (or vice versa) | Low | Medium | C# records _are_ the schema. Use `PropertyNameCaseInsensitive = true` for resilience. Default values on all properties prevent crashes from missing fields. Add unit tests for deserialization of complete and partial JSON. |
| 5 | **Segoe UI font missing on macOS/Linux** — Dashboard uses Arial fallback, which has different metrics, causing layout shifts | Low | Medium (macOS/Linux users) | Document the Segoe UI requirement. Provide installation instructions for non-Windows platforms. Arial fallback is acceptable — metrics are close enough for screenshots. |
| 6 | **`data.json` accidentally served as static file** — Browser can access `http://localhost:5000/data.json` directly | Low | High (default static file behavior) | Add `OnPrepareResponse` filter to block `data.json` in static file middleware, or move `data.json` outside `wwwroot/` and configure path via `appsettings.json`. |
| 7 | **Heatmap grid overflow** — Too many work items in a single cell cause text to overflow the fixed-height cell | Medium | Medium | CSS `overflow: hidden` on `.hm-cell` (already in design). Recommend limiting items to 6-8 per cell in documentation. Very long item names truncate visually. |
| 8 | **Dynamic column count breaks grid layout** — More than 4-5 heatmap columns causes columns to become too narrow | Low | Low | Grid uses `repeat(N, 1fr)` so columns shrink proportionally. For > 6 columns, recommend reducing font size or abbreviating month names. Document the recommended maximum. |
| 9 | **Date parsing failures** — Invalid date strings in `data.json` cause `FormatException` | Medium | Medium | Wrap all `DateTime.Parse()` calls in try/catch. Skip milestones with invalid dates. Log warnings. Add unit tests for malformed dates. |
| 10 | **Hot reload limitations** — Some Blazor Server changes (e.g., `Program.cs`, DI registration) require full restart, confusing developers | Low | Low | Document which changes require restart vs. hot reload. CSS and Razor changes reload automatically with `dotnet watch`. Model and service changes require restart. |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component, its CSS layout strategy, data bindings, and interactions.

### Component-to-Visual Mapping

```
┌──────────────────────────────────────────────────────────────────────────┐
│  1920 × 1080 Fixed Viewport (body)                                       │
│  display: flex; flex-direction: column;                                   │
│                                                                          │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │  DashboardHeader.razor  (.hdr)  ~50px                              │  │
│  │  ┌─────────────────────────┐  ┌─────────────────────────────────┐ │  │
│  │  │ Title + Subtitle + Link │  │ Legend (PoC·Prod·Check·Now)     │ │  │
│  │  └─────────────────────────┘  └─────────────────────────────────┘ │  │
│  └────────────────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │  TimelineSection.razor  (.tl-area)  196px                          │  │
│  │  ┌──────────┐  ┌───────────────────────────────────────────────┐  │  │
│  │  │ Track    │  │ SVG Canvas (1560×185)                         │  │  │
│  │  │ Labels   │  │   Month lines · Track lines · Milestones     │  │  │
│  │  │ (230px)  │  │   NOW line · Date labels                     │  │  │
│  │  └──────────┘  └───────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │  HeatmapGrid.razor  (.hm-wrap)  flex: 1 (fills remaining ~834px)  │  │
│  │  ┌─────────────────────────────────────────────────────────────┐   │  │
│  │  │ Section Title (.hm-title)                                   │   │  │
│  │  ├──────┬──────┬──────┬──────┬──────┐                          │   │  │
│  │  │STATUS│ Jan  │ Feb  │ Mar  │ Apr* │  ← Column Headers        │   │  │
│  │  ├──────┼──────┼──────┼──────┼──────┤     (* = highlighted)    │   │  │
│  │  │SHIP  │ items│ items│ items│ items│  ← Green row              │   │  │
│  │  ├──────┼──────┼──────┼──────┼──────┤                          │   │  │
│  │  │PROG  │ items│ items│ items│ items│  ← Blue row               │   │  │
│  │  ├──────┼──────┼──────┼──────┼──────┤                          │   │  │
│  │  │CARRY │ items│ items│ items│ items│  ← Amber row              │   │  │
│  │  ├──────┼──────┼──────┼──────┼──────┤                          │   │  │
│  │  │BLOCK │ items│ items│ items│ items│  ← Red row                │   │  │
│  │  └──────┴──────┴──────┴──────┴──────┘                          │   │  │
│  └────────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────┘
```

### Detailed Component Specifications

#### `DashboardHeader.razor`

| Attribute | Value |
|---|---|
| **Visual Section** | `.hdr` in design reference |
| **CSS Layout** | `display: flex; align-items: center; justify-content: space-between` |
| **Height** | Auto (~50px, content-driven) |
| **Padding** | `12px 44px 10px` |
| **Border** | `border-bottom: 1px solid #E0E0E0` |
| **Data Bindings** | `Project.Title` → `<h1>`, `Project.BacklogUrl` → `<a href>`, `Project.BacklogLinkText` → `<a>` text, `Project.Subtitle` → `.sub` div |
| **Interactions** | Backlog link: `<a href="{Project.BacklogUrl}" target="_blank">` opens in new tab |
| **Static Elements** | Legend icons (PoC diamond, Production diamond, Checkpoint circle, Now bar) — rendered as inline `<span>` with CSS transforms |

#### `TimelineSection.razor`

| Attribute | Value |
|---|---|
| **Visual Section** | `.tl-area` in design reference |
| **CSS Layout** | `display: flex; align-items: stretch` |
| **Height** | Fixed `196px` |
| **Background** | `#FAFAFA` |
| **Data Bindings** | `Timeline.Tracks[i].Id` + `.Label` → track label panel; `Timeline.Tracks[i].Milestones` → SVG shapes; `Timeline.StartDate`/`.EndDate` → date-to-X mapping; `Timeline.NowDate` → NOW line position |
| **Interactions** | None (read-only SVG) |
| **SVG Dimensions** | `width="1560" height="185"` with `overflow: visible` |
| **Dynamic Calculations** | `DateToX()` — linear interpolation from date range to pixel X coordinate; `GetLabelPosition()` — collision avoidance for date labels; Track Y positions: `42 + (trackIndex * 56)` |

#### `HeatmapGrid.razor`

| Attribute | Value |
|---|---|
| **Visual Section** | `.hm-wrap` + `.hm-grid` in design reference |
| **CSS Layout** | CSS Grid: `grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(M, 1fr)` where N = `Heatmap.Columns.Count` and M = `Heatmap.Rows.Count` |
| **Flex** | `flex: 1; min-height: 0` (fills remaining viewport) |
| **Data Bindings** | `Heatmap.SectionTitle` → `.hm-title`; `Heatmap.Columns` → column headers; `Heatmap.HighlightColumn` → `.highlight-hdr` / `.highlight` classes; `Heatmap.Rows[i].Category` → row header text; `Heatmap.Rows[i].Theme` → CSS class prefix; `Heatmap.Rows[i].Items[column]` → cell content |
| **Interactions** | None (read-only grid) |
| **Empty State** | Cells with no items render `<span style="color:#AAA">-</span>` |
| **Dynamic CSS** | Grid template columns/rows set via inline `style` attribute to support N columns and M rows from data |

### CSS Class Architecture

The CSS uses a theme-based class naming convention that maps directly to `HeatmapRow.Theme`:

```
Theme: "ship" → Classes: .ship-hdr, .ship-cell, .ship-cell.highlight
Theme: "prog" → Classes: .prog-hdr, .prog-cell, .prog-cell.highlight
Theme: "carry" → Classes: .carry-hdr, .carry-cell, .carry-cell.highlight
Theme: "block" → Classes: .block-hdr, .block-cell, .block-cell.highlight
```

Components construct class names dynamically:

```razor
<div class="hm-row-hdr @(row.Theme)-hdr">@row.Emoji @row.Category</div>
<div class="hm-cell @(row.Theme)-cell @(col == Heatmap.HighlightColumn ? "highlight" : "")">
    @if (items.Any())
    {
        @foreach (var item in items)
        {
            <div class="it">@item</div>
        }
    }
    else
    {
        <span style="color:#AAA">-</span>
    }
</div>
```

### Test Strategy for Components

| Component | Test Type | What Is Verified |
|---|---|---|
| `DashboardHeader` | bUnit component test | Title renders in `<h1>`, backlog link has correct `href` and `target="_blank"`, subtitle renders in `.sub`, legend icons are present |
| `TimelineSection` | bUnit component test | SVG element present with correct dimensions, track lines rendered per track count, milestone shapes match type (polygon for poc/production, circle for checkpoint), NOW line positioned correctly |
| `TimelineSection` | Unit test | `DateToX()` returns correct pixel positions for known dates, edge cases (date at start, date at end, date outside range) |
| `TimelineSection` | Unit test | `GetLabelPosition()` returns "below" when nearby milestone already has "above" label |
| `HeatmapGrid` | bUnit component test | Grid has correct column count, highlight class applied to correct column, items render in correct cells, empty cells show dash |
| `DashboardDataService` | Unit test | Valid JSON deserializes correctly, missing file returns null + error message, malformed JSON returns null + error message, missing optional fields use defaults |
| `DashboardData` records | Unit test | Round-trip serialization/deserialization preserves all fields, default values are correct when properties are omitted |