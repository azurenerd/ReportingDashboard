# Architecture

**Document Version:** 1.0
**Date:** April 16, 2026
**Author:** Architecture Team
**Stack:** C# .NET 8 / Blazor Server / Local-only / .sln structure
**Status:** Approved for Implementation

---

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server application that renders a pixel-perfect 1920×1080 project status view for screenshot capture and inclusion in executive PowerPoint decks. It is a direct, data-driven translation of the approved `OriginalDesignConcept.html` reference into Razor components.

The page consists of three vertically stacked sections—Header, Timeline, and Heatmap Grid—all driven by a single `data.json` file. There is no database, no authentication, no cloud dependency, and no JavaScript. The application runs locally via `dotnet run` on Kestrel.

### Architectural Goals

| # | Goal | Architectural Response |
|---|------|----------------------|
| 1 | Executive-ready visibility | Fixed 1920×1080 layout with pixel-accurate CSS from the reference design |
| 2 | Screenshot-friendly output | `overflow: hidden`, no scrollbars, no Blazor framework UI chrome |
| 3 | Minimal operational overhead | Zero external NuGet packages, zero infrastructure, single JSON data source |
| 4 | Cross-project reuse | 100% data-driven rendering; swap `data.json` to change projects |
| 5 | Fast reporting turnaround | Edit JSON → refresh browser → screenshot; no build step required for data changes |

### Architectural Principles

- **No abstraction beyond what's needed.** This is a read-only visualization tool, not an enterprise app. One project, one service, one page.
- **CSS fidelity is the primary constraint.** The reference HTML's `<style>` block is copied verbatim into `dashboard.css`. Razor components emit the same DOM structure as the reference HTML.
- **Data shapes match JSON exactly.** C# record types mirror the `data.json` schema 1:1. No transformation layer, no mapping, no DTO conversion.

---

## System Components

### 1. Solution Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj        (.NET 8, Blazor Server)
│       ├── Program.cs                        (Host builder, DI registration)
│       ├── Models/
│       │   └── DashboardData.cs             (C# records for JSON schema)
│       ├── Services/
│       │   └── DashboardDataService.cs      (JSON file reader + deserializer)
│       ├── Components/
│       │   ├── App.razor                    (Root component, <head> refs)
│       │   ├── Routes.razor                 (Router configuration)
│       │   ├── Pages/
│       │   │   └── Dashboard.razor          (Single page layout, orchestrator)
│       │   └── Shared/
│       │       ├── Header.razor             (Title, subtitle, backlog link, legend)
│       │       ├── Timeline.razor           (SVG milestone timeline + left labels)
│       │       ├── HeatmapGrid.razor        (CSS Grid status matrix)
│       │       └── HeatmapCell.razor        (Individual cell with item bullets)
│       └── wwwroot/
│           ├── css/
│           │   └── dashboard.css            (Global styles from reference HTML)
│           └── data/
│               └── data.json                (Project data - Phoenix Platform example)
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        ├── Services/
        │   └── DashboardDataServiceTests.cs
        └── Models/
            └── DashboardDataTests.cs
```

### 2. Component Detail

#### 2.1 `Program.cs` — Application Host

**Responsibilities:**
- Configure Kestrel for localhost HTTPS
- Register `DashboardDataService` as a Singleton in DI
- Add Blazor Server services (`AddRazorComponents().AddInteractiveServerComponents()`)
- Configure `IWebHostEnvironment` for `wwwroot` file access
- Map Blazor hub and static files

**Interfaces:** None (entry point)
**Dependencies:** .NET 8 SDK, `Microsoft.AspNetCore.Builder`
**Data:** None directly; configures DI container

```csharp
var builder = WebApplication.CreateBuilder(args);
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

#### 2.2 `DashboardData.cs` — Data Models

**Responsibilities:** Define immutable C# record types that map 1:1 to the `data.json` schema. Used for deserialization and parameter passing to components.

**Interfaces:** None (POCOs)
**Dependencies:** `System.Text.Json.Serialization` for `[JsonPropertyName]` attributes
**Data:** Represents the complete dashboard state

```csharp
namespace ReportingDashboard.Models;

public record DashboardData(
    string Title,
    string Subtitle,
    string BacklogUrl,
    string CurrentDate,
    string TimelineStartMonth,
    string TimelineEndMonth,
    List<Milestone> Milestones,
    List<string> HeatmapMonths,
    string CurrentMonth,
    List<Category> Categories
);

public record Milestone(
    string Id,
    string Label,
    string Color,
    List<MilestoneEvent> Events
);

public record MilestoneEvent(
    string Date,
    string Type,    // "checkpoint" | "poc" | "production"
    string Label
);

public record Category(
    string Name,
    string CssClass,
    Dictionary<string, List<string>> Items
);
```

**Design Decisions:**
- Use `record` types for immutability and structural equality
- Use `string` for dates (parsed to `DateOnly` in components where coordinate math is needed) to keep deserialization simple and error-tolerant
- `Dictionary<string, List<string>>` for `Items` maps month names to work item lists directly from JSON
- All property names use PascalCase with `System.Text.Json`'s `PropertyNameCaseInsensitive = true` option, matching the camelCase JSON without explicit `[JsonPropertyName]` attributes

#### 2.3 `DashboardDataService.cs` — Data Access Service

**Responsibilities:**
- Read `wwwroot/data/data.json` from disk on first request
- Deserialize JSON into `DashboardData` record
- Cache the result in memory (lazy initialization)
- Handle missing file, malformed JSON, and invalid field scenarios gracefully
- Log errors via `ILogger<DashboardDataService>`

**Interfaces:**
```csharp
public class DashboardDataService
{
    public Task<DashboardData?> GetDashboardDataAsync();
    public string? ErrorMessage { get; }
}
```

**Dependencies:** `IWebHostEnvironment` (for `WebRootPath`), `ILogger<DashboardDataService>`
**Data:** Reads `wwwroot/data/data.json`, caches `DashboardData?` in a private field

**Implementation:**
```csharp
namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;
    private DashboardData? _cachedData;
    private bool _loaded;

    public string? ErrorMessage { get; private set; }

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<DashboardData?> GetDashboardDataAsync()
    {
        if (_loaded) return _cachedData;

        var path = Path.Combine(_env.WebRootPath, "data", "data.json");
        try
        {
            if (!File.Exists(path))
            {
                ErrorMessage = "Unable to load dashboard data. Please ensure data.json exists in wwwroot/data/ and contains valid JSON.";
                _logger.LogError("data.json not found at {Path}", path);
                _loaded = true;
                return null;
            }

            var json = await File.ReadAllTextAsync(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _cachedData = JsonSerializer.Deserialize<DashboardData>(json, options);

            if (_cachedData is null)
            {
                ErrorMessage = "data.json was read but deserialized to null. Check the file contents.";
                _logger.LogError("data.json deserialized to null");
            }
        }
        catch (JsonException ex)
        {
            ErrorMessage = "Unable to load dashboard data. data.json contains invalid JSON.";
            _logger.LogError(ex, "Failed to deserialize data.json");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unexpected error loading data.json: {ex.Message}";
            _logger.LogError(ex, "Unexpected error reading data.json");
        }

        _loaded = true;
        return _cachedData;
    }
}
```

**Error Handling Strategy:**
- File not found → `ErrorMessage` set, `null` returned, logged at Error level
- Invalid JSON → `JsonException` caught, `ErrorMessage` set, `null` returned
- Invalid date fields in milestones → Handled at the component level (skip individual events with `DateOnly.TryParse`), not in the service

#### 2.4 `App.razor` — Root Component

**Responsibilities:** Define the HTML `<head>` with CSS reference and Blazor script tag. Wrap the router.

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <link rel="stylesheet" href="css/dashboard.css" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

**Key Detail:** No `app.css` or Bootstrap references. Only `dashboard.css` is loaded.

#### 2.5 `Dashboard.razor` — Page Orchestrator

**Responsibilities:**
- Inject `DashboardDataService`
- Call `GetDashboardDataAsync()` in `OnInitializedAsync`
- Render error state if data is null
- Pass data to child components via `[Parameter]` properties
- Define the three-section vertical flex layout

**Interfaces:** Blazor page at route `/`
**Dependencies:** `DashboardDataService`, child components (`Header`, `Timeline`, `HeatmapGrid`)
**Data:** Receives `DashboardData` from service, distributes to children

```razor
@page "/"
@using ReportingDashboard.Models
@using ReportingDashboard.Services
@inject DashboardDataService DataService
@rendermode InteractiveServer

@if (_data is not null)
{
    <Header Data="_data" />
    <Timeline Data="_data" />
    <HeatmapGrid Data="_data" />
}
else if (_errorMessage is not null)
{
    <div style="display:flex;align-items:center;justify-content:center;height:100vh;
                font-size:16px;color:#888;font-family:'Segoe UI',Arial,sans-serif;">
        @_errorMessage
    </div>
}

@code {
    private DashboardData? _data;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        _data = await DataService.GetDashboardDataAsync();
        _errorMessage = DataService.ErrorMessage;
    }
}
```

#### 2.6 `Header.razor` — Header Bar Component

**Responsibilities:**
- Render project title with inline ADO Backlog link
- Render subtitle line
- Render legend with four marker type icons (PoC diamond, Production diamond, Checkpoint circle, NOW line)

**Interfaces:** `[Parameter] public DashboardData Data { get; set; }`
**Dependencies:** None
**Data:** Reads `Data.Title`, `Data.Subtitle`, `Data.BacklogUrl`, `Data.CurrentMonth`

**DOM Output:** Maps to `.hdr` div from `OriginalDesignConcept.html`

```razor
<div class="hdr">
    <div>
        <h1>@Data.Title <a href="@Data.BacklogUrl">📋 ADO Backlog</a></h1>
        <div class="sub">@Data.Subtitle</div>
    </div>
    <div style="display:flex;gap:22px;align-items:center;">
        @* PoC Milestone legend *@
        <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
            <span style="width:12px;height:12px;background:#F4B400;transform:rotate(45deg);
                         display:inline-block;flex-shrink:0;"></span>PoC Milestone
        </span>
        @* Production Release legend *@
        <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
            <span style="width:12px;height:12px;background:#34A853;transform:rotate(45deg);
                         display:inline-block;flex-shrink:0;"></span>Production Release
        </span>
        @* Checkpoint legend *@
        <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
            <span style="width:8px;height:8px;border-radius:50%;background:#999;
                         display:inline-block;flex-shrink:0;"></span>Checkpoint
        </span>
        @* NOW marker legend *@
        <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
            <span style="width:2px;height:14px;background:#EA4335;
                         display:inline-block;flex-shrink:0;"></span>Now (@Data.CurrentMonth 2026)
        </span>
    </div>
</div>
```

#### 2.7 `Timeline.razor` — SVG Milestone Timeline

**Responsibilities:**
- Render the left panel with milestone IDs and labels (color-coded)
- Render the SVG viewport with:
  - Month gridlines (vertical lines at equal intervals)
  - Month labels (Jan, Feb, Mar, etc.)
  - Horizontal milestone row lines
  - Event markers: checkpoint circles, PoC diamonds (#F4B400), production diamonds (#34A853)
  - Date labels above/below markers (alternating to avoid overlap)
  - Red dashed "NOW" line at current date position
- Calculate X coordinates from dates using the `DateToX` formula
- Calculate Y offsets for milestone rows dynamically based on milestone count
- Clamp events outside the visible date range to boundary positions (x ≤ 0 renders at x=0)

**Interfaces:** `[Parameter] public DashboardData Data { get; set; }`
**Dependencies:** None
**Data:** Reads `Data.Milestones`, `Data.TimelineStartMonth`, `Data.TimelineEndMonth`, `Data.CurrentDate`

**Key Computation Logic:**

```csharp
@code {
    [Parameter] public required DashboardData Data { get; set; }

    private const double SvgWidth = 1560;
    private const double SvgHeight = 185;
    private const double FirstRowY = 42;
    private const double RowSpacing = 56;
    private const double DiamondRadius = 11;

    private DateOnly _timelineStart;
    private DateOnly _timelineEnd;
    private List<MonthGridline> _gridlines = new();

    protected override void OnParametersSet()
    {
        // Parse timeline bounds: "2026-01" → first day of month
        _timelineStart = DateOnly.ParseExact(Data.TimelineStartMonth + "-01", "yyyy-MM-dd");
        // End month: parse as first day, then use last day for full range
        var endFirstDay = DateOnly.ParseExact(Data.TimelineEndMonth + "-01", "yyyy-MM-dd");
        _timelineEnd = endFirstDay.AddMonths(1).AddDays(-1);

        // Generate month gridlines
        _gridlines.Clear();
        var current = _timelineStart;
        while (current <= _timelineEnd)
        {
            _gridlines.Add(new MonthGridline(
                X: DateToX(current),
                Label: current.ToString("MMM")
            ));
            current = current.AddMonths(1);
        }
    }

    private double DateToX(DateOnly date)
    {
        var totalDays = (_timelineEnd.ToDateTime(TimeOnly.MinValue) - _timelineStart.ToDateTime(TimeOnly.MinValue)).TotalDays;
        var elapsed = (date.ToDateTime(TimeOnly.MinValue) - _timelineStart.ToDateTime(TimeOnly.MinValue)).TotalDays;
        var x = (elapsed / totalDays) * SvgWidth;
        return Math.Clamp(x, 0, SvgWidth);
    }

    private double GetRowY(int index) => FirstRowY + (index * RowSpacing);

    private string DiamondPoints(double cx, double cy)
    {
        var r = DiamondRadius;
        return $"{cx},{cy - r} {cx + r},{cy} {cx},{cy + r} {cx - r},{cy}";
    }

    private record MonthGridline(double X, string Label);
}
```

**SVG Structure:** The Razor markup generates `<svg>` elements matching the reference HTML exactly—gridlines, milestone lines, markers, labels, and the NOW indicator. Event markers use `<circle>` for checkpoints and `<polygon>` for diamonds, with a `<defs><filter>` for the drop shadow.

**Date label collision avoidance:** Labels alternate between above (y - 16) and below (y + 24) the marker based on event index within each milestone row.

#### 2.8 `HeatmapGrid.razor` — CSS Grid Status Matrix

**Responsibilities:**
- Render section title ("Monthly Execution Heatmap")
- Render the CSS Grid container with dynamic column count
- Render header row: corner cell ("Status") + month column headers
- Highlight current month column header with amber styling
- Render four category rows by iterating `Data.Categories`
- Delegate cell rendering to `HeatmapCell.razor`

**Interfaces:** `[Parameter] public DashboardData Data { get; set; }`
**Dependencies:** `HeatmapCell` component
**Data:** Reads `Data.HeatmapMonths`, `Data.CurrentMonth`, `Data.Categories`

```razor
<div class="hm-wrap">
    <div class="hm-title">Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers</div>
    <div class="hm-grid" style="grid-template-columns: 160px repeat(@Data.HeatmapMonths.Count, 1fr);
                                 grid-template-rows: 36px repeat(@Data.Categories.Count, 1fr);">
        @* Header row *@
        <div class="hm-corner">Status</div>
        @foreach (var month in Data.HeatmapMonths)
        {
            var isCurrent = month == Data.CurrentMonth;
            <div class="hm-col-hdr @(isCurrent ? "apr-hdr" : "")">
                @month @(isCurrent ? "◀ Now" : "")
            </div>
        }

        @* Data rows *@
        @foreach (var category in Data.Categories)
        {
            <div class="hm-row-hdr @(category.CssClass)-hdr">
                @GetEmoji(category.CssClass) @category.Name.ToUpperInvariant()
            </div>
            @foreach (var month in Data.HeatmapMonths)
            {
                var isCurrent = month == Data.CurrentMonth;
                var items = category.Items.GetValueOrDefault(month, new List<string>());
                <HeatmapCell CssClass="@category.CssClass"
                             IsCurrent="@isCurrent"
                             CurrentMonthClass="@(Data.CurrentMonth.ToLowerInvariant())"
                             Items="@items" />
            }
        }
    </div>
</div>

@code {
    [Parameter] public required DashboardData Data { get; set; }

    private static string GetEmoji(string cssClass) => cssClass switch
    {
        "ship" => "✅",
        "prog" => "🔄",
        "carry" => "🔁",
        "block" => "🚫",
        _ => ""
    };
}
```

#### 2.9 `HeatmapCell.razor` — Individual Data Cell

**Responsibilities:**
- Render work items as bulleted entries with colored dots (via CSS `::before` pseudo-element)
- Render empty cells with a gray dash "–"
- Apply current-month darker background class

**Interfaces:**
```csharp
[Parameter] public string CssClass { get; set; }        // e.g., "ship", "prog"
[Parameter] public bool IsCurrent { get; set; }          // true if this is the current month
[Parameter] public string CurrentMonthClass { get; set; } // e.g., "apr"
[Parameter] public List<string> Items { get; set; }
```

**Dependencies:** None
**Data:** Receives item list and styling parameters from `HeatmapGrid`

```razor
<div class="hm-cell @(CssClass)-cell @(IsCurrent ? CurrentMonthClass : "")">
    @if (Items.Count > 0)
    {
        @foreach (var item in Items)
        {
            <div class="it">@item</div>
        }
    }
    else
    {
        <div style="color:#CCC;font-size:14px;text-align:center;">–</div>
    }
</div>

@code {
    [Parameter] public required string CssClass { get; set; }
    [Parameter] public bool IsCurrent { get; set; }
    [Parameter] public string CurrentMonthClass { get; set; } = "";
    [Parameter] public required List<string> Items { get; set; }
}
```

#### 2.10 `dashboard.css` — Global Stylesheet

**Responsibilities:** Define all visual styling. Copied verbatim from `OriginalDesignConcept.html` `<style>` block with minimal adaptation (no changes to selectors, colors, or spacing).

**Source:** The CSS from the reference HTML is the production CSS. No rewriting, no CSS framework, no Blazor CSS isolation.

**Key rules preserved exactly:**
- `body { width: 1920px; height: 1080px; overflow: hidden; }` — fixed viewport
- `.hm-grid` column/row template — CSS Grid layout
- All color-coded cell classes (`.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`)
- `.hm-cell .it::before` — colored bullet dots via pseudo-element
- `.apr-hdr`, `.ship-cell.apr`, `.prog-cell.apr`, etc. — current-month highlighting

**Adaptation notes:**
- The `grid-template-columns` value in `.hm-grid` is overridden via inline `style` in Razor to support dynamic month counts
- The `grid-template-rows` count is dynamically set to match `categories.length` (always 4 for MVP, but data-driven)

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────┐     ┌──────────────────────┐     ┌──────────────────┐
│  data.json  │────▶│ DashboardDataService │────▶│  Dashboard.razor │
│ (wwwroot/)  │     │ (Singleton, cached)  │     │  (Page, route /) │
└─────────────┘     └──────────────────────┘     └────────┬─────────┘
                                                          │
                              ┌────────────────────────────┼────────────────────────┐
                              │                            │                        │
                              ▼                            ▼                        ▼
                    ┌─────────────────┐       ┌──────────────────┐     ┌──────────────────────┐
                    │  Header.razor   │       │ Timeline.razor   │     │ HeatmapGrid.razor    │
                    │ (Title, Legend) │       │ (SVG Timeline)   │     │ (CSS Grid Matrix)    │
                    └─────────────────┘       └──────────────────┘     └───────────┬──────────┘
                                                                                   │
                                                                    ┌──────────────┼──────────────┐
                                                                    ▼              ▼              ▼
                                                              ┌───────────┐  ┌───────────┐  ┌───────────┐
                                                              │HeatmapCell│  │HeatmapCell│  │HeatmapCell│
                                                              │ (Jan/Ship)│  │ (Feb/Ship)│  │ (Mar/Ship)│
                                                              └───────────┘  └───────────┘  └───────────┘
                                                                   ...          ...          ...
```

### Communication Patterns

| From | To | Mechanism | Data |
|------|----|-----------|------|
| `Program.cs` | `DashboardDataService` | DI registration (Singleton) | Service lifetime config |
| `Dashboard.razor` | `DashboardDataService` | Constructor injection (`@inject`) | Calls `GetDashboardDataAsync()` |
| `Dashboard.razor` | `Header.razor` | Blazor `[Parameter]` | Full `DashboardData` object |
| `Dashboard.razor` | `Timeline.razor` | Blazor `[Parameter]` | Full `DashboardData` object |
| `Dashboard.razor` | `HeatmapGrid.razor` | Blazor `[Parameter]` | Full `DashboardData` object |
| `HeatmapGrid.razor` | `HeatmapCell.razor` | Blazor `[Parameter]` | `CssClass`, `IsCurrent`, `Items` list |

### Lifecycle Sequence

1. **Application Startup:** `Program.cs` builds the host, registers services, starts Kestrel on `https://localhost:5001`
2. **First HTTP Request:** Browser navigates to `/`. Blazor Server establishes SignalR WebSocket connection.
3. **Component Initialization:** `Dashboard.razor.OnInitializedAsync()` calls `DashboardDataService.GetDashboardDataAsync()`
4. **Data Loading:** Service reads `wwwroot/data/data.json`, deserializes to `DashboardData`, caches result
5. **Render Cascade:** `Dashboard.razor` passes data to `Header`, `Timeline`, and `HeatmapGrid` via parameters
6. **SVG Computation:** `Timeline.razor.OnParametersSet()` calculates gridlines, marker positions, NOW line X coordinate
7. **Grid Rendering:** `HeatmapGrid` iterates categories × months, instantiates `HeatmapCell` for each intersection
8. **Browser Paint:** Blazor Server sends rendered HTML over SignalR; browser paints the 1920×1080 page
9. **Subsequent Requests:** Data is served from cache. No file I/O on refresh (until app restart).

### Error Flow

```
data.json missing ──▶ Service sets ErrorMessage ──▶ Dashboard.razor renders error div
data.json malformed ──▶ Service catches JsonException ──▶ Same error path
Invalid event date ──▶ Timeline.razor skips event (DateOnly.TryParse) ──▶ Rest renders normally
```

---

## Data Model

### Entity Relationship

```
DashboardData (root)
├── title: string
├── subtitle: string
├── backlogUrl: string
├── currentDate: string (YYYY-MM-DD)
├── timelineStartMonth: string (YYYY-MM)
├── timelineEndMonth: string (YYYY-MM)
├── currentMonth: string
├── milestones: Milestone[] (1..5)
│   ├── id: string
│   ├── label: string
│   ├── color: string (hex)
│   └── events: MilestoneEvent[] (0..10)
│       ├── date: string (YYYY-MM-DD)
│       ├── type: string (checkpoint|poc|production)
│       └── label: string
├── heatmapMonths: string[] (2..6)
└── categories: Category[] (exactly 4)
    ├── name: string
    ├── cssClass: string (ship|prog|carry|block)
    └── items: Dictionary<string, string[]>
        └── key = month name, value = work item descriptions
```

### Storage

| Aspect | Detail |
|--------|--------|
| **Format** | JSON file (`data.json`) |
| **Location** | `wwwroot/data/data.json` within the project |
| **Size** | 1-50KB typical |
| **Access Pattern** | Read once on first request, cached in-memory for application lifetime |
| **Modification** | Manual editing by report author; changes require page refresh (or app restart without `dotnet watch`) |
| **Versioning** | Git (file lives in the repository) |
| **Backup** | Git history |

### C# Record Definitions

```csharp
namespace ReportingDashboard.Models;

using System.Text.Json.Serialization;

public record DashboardData(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("subtitle")] string Subtitle,
    [property: JsonPropertyName("backlogUrl")] string BacklogUrl,
    [property: JsonPropertyName("currentDate")] string CurrentDate,
    [property: JsonPropertyName("timelineStartMonth")] string TimelineStartMonth,
    [property: JsonPropertyName("timelineEndMonth")] string TimelineEndMonth,
    [property: JsonPropertyName("milestones")] List<Milestone> Milestones,
    [property: JsonPropertyName("heatmapMonths")] List<string> HeatmapMonths,
    [property: JsonPropertyName("currentMonth")] string CurrentMonth,
    [property: JsonPropertyName("categories")] List<Category> Categories
);

public record Milestone(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("color")] string Color,
    [property: JsonPropertyName("events")] List<MilestoneEvent> Events
);

public record MilestoneEvent(
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("label")] string Label
);

public record Category(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("cssClass")] string CssClass,
    [property: JsonPropertyName("items")] Dictionary<string, List<string>> Items
);
```

### Canonical Example: `data.json`

The project ships with a fictional "Phoenix Platform" example:

```json
{
  "title": "Phoenix Platform Release Roadmap",
  "subtitle": "Engineering Excellence · Phoenix Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/contoso/phoenix/_backlogs",
  "currentDate": "2026-04-10",
  "timelineStartMonth": "2026-01",
  "timelineEndMonth": "2026-06",
  "milestones": [
    {
      "id": "M1",
      "label": "API Gateway & Auth",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-15", "type": "checkpoint", "label": "Design Review" },
        { "date": "2026-02-28", "type": "poc", "label": "Feb 28 PoC" },
        { "date": "2026-04-15", "type": "production", "label": "Apr GA" }
      ]
    },
    {
      "id": "M2",
      "label": "Data Pipeline v2",
      "color": "#00897B",
      "events": [
        { "date": "2025-12-19", "type": "checkpoint", "label": "Kickoff" },
        { "date": "2026-02-11", "type": "checkpoint", "label": "Schema Lock" },
        { "date": "2026-03-20", "type": "poc", "label": "Mar 20 PoC" },
        { "date": "2026-05-15", "type": "production", "label": "May Prod" }
      ]
    },
    {
      "id": "M3",
      "label": "Dashboard & Reporting",
      "color": "#546E7A",
      "events": [
        { "date": "2026-03-01", "type": "checkpoint", "label": "UX Approved" },
        { "date": "2026-04-30", "type": "poc", "label": "Apr 30 PoC" },
        { "date": "2026-06-15", "type": "production", "label": "Jun Prod" }
      ]
    }
  ],
  "heatmapMonths": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["API Gateway scaffold", "Auth token service"],
        "Feb": ["Rate limiting middleware", "Pipeline schema v2"],
        "Mar": ["Batch ingestion engine", "Monitoring dashboards", "Load test framework"],
        "Apr": ["Gateway GA release", "SSO integration"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": {
        "Jan": ["Token refresh flow"],
        "Feb": ["CDC connector", "Schema migration tool"],
        "Mar": ["Real-time streaming PoC"],
        "Apr": ["Dashboard UX build", "Pipeline perf tuning", "E2E test suite"]
      }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": {
        "Jan": [],
        "Feb": ["Retry policy config"],
        "Mar": ["Retry policy config", "Legacy API deprecation"],
        "Apr": ["Legacy API deprecation"]
      }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": {
        "Jan": [],
        "Feb": [],
        "Mar": ["Vendor SDK delay"],
        "Apr": ["Vendor SDK delay", "Infra quota approval"]
      }
    }
  ]
}
```

### Data Validation Rules

| Field | Validation | Behavior on Invalid |
|-------|-----------|-------------------|
| `title`, `subtitle` | Non-null string | Deserialization failure → error page |
| `backlogUrl` | Non-null string (URL format not enforced) | Renders as-is; broken links are the author's responsibility |
| `currentDate` | `YYYY-MM-DD` parseable to `DateOnly` | NOW line not rendered if parse fails; warning logged |
| `timelineStartMonth`, `timelineEndMonth` | `YYYY-MM` format | Timeline section not rendered if parse fails; error logged |
| `milestones[].events[].date` | `YYYY-MM-DD` parseable to `DateOnly` | Individual event skipped; warning logged; other events render normally |
| `milestones[].events[].type` | One of `checkpoint`, `poc`, `production` | Unknown types render as checkpoint (circle) with warning logged |
| `categories[].items` | Dictionary with month-name keys | Missing month keys → treated as empty list → renders dash |

---

## API Contracts

This application has no REST API, GraphQL endpoint, or external service integration. All data flows are internal.

### Internal Service Contract

```csharp
// DashboardDataService — the only "API" in the system
public class DashboardDataService
{
    /// <summary>
    /// Returns the dashboard data loaded from data.json, or null if loading failed.
    /// Result is cached after first call. Thread-safe for Blazor Server circuits.
    /// </summary>
    public Task<DashboardData?> GetDashboardDataAsync();

    /// <summary>
    /// Non-null if data loading failed. Contains a user-friendly error message
    /// suitable for display on the dashboard page.
    /// </summary>
    public string? ErrorMessage { get; }
}
```

### Static File Endpoints (Kestrel built-in)

| Endpoint | Method | Response | Purpose |
|----------|--------|----------|---------|
| `/` | GET | HTML (Blazor Server page) | Dashboard page |
| `/_framework/blazor.web.js` | GET | JavaScript | Blazor Server SignalR bootstrap |
| `/css/dashboard.css` | GET | CSS | Dashboard styles |
| `/data/data.json` | GET | JSON | Raw data file (served as static file, but not consumed by browser directly) |
| `/_blazor` | WebSocket | SignalR | Blazor Server interactive circuit |

### Error Responses

| Scenario | HTTP Status | Page Content |
|----------|-------------|-------------|
| Normal operation | 200 | Full dashboard |
| `data.json` missing | 200 | Centered error message: "Unable to load dashboard data..." |
| `data.json` malformed | 200 | Centered error message with details |
| Blazor circuit disconnect | N/A | Browser shows Blazor reconnect overlay (suppressed in production via CSS if needed) |

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|--------------|
| **Runtime** | .NET 8 SDK (8.0.x LTS) |
| **OS** | Windows 10/11 (primary), macOS/Linux (secondary) |
| **Memory** | < 100MB resident |
| **Disk** | ~200MB (.NET SDK) + ~5MB (application) |
| **Network** | Localhost only; no outbound connections required |
| **Ports** | HTTPS: 5001 (configurable), HTTP: 5000 (configurable) |

### Hosting

| Aspect | Detail |
|--------|--------|
| **Web Server** | Kestrel (built into .NET 8) |
| **Reverse Proxy** | None required |
| **Process Model** | Single process, single-user |
| **Startup** | `dotnet run --project src/ReportingDashboard` |
| **Hot Reload** | `dotnet watch run --project src/ReportingDashboard` |

### Storage

| Artifact | Location | Size | Persistence |
|----------|----------|------|------------|
| Application code | `src/ReportingDashboard/` | ~50KB source | Git repository |
| `data.json` | `wwwroot/data/data.json` | 1-50KB | Git or manual distribution |
| `dashboard.css` | `wwwroot/css/dashboard.css` | ~3KB | Git repository |
| Build output | `bin/Debug/net8.0/` | ~5MB | Regenerated on build |
| Published output | `bin/Release/net8.0/publish/` | ~80MB (self-contained) | Release artifact |

### CI/CD

Not required for MVP. The application is built and run locally. If desired:

```bash
# Build verification (zero warnings, zero errors)
dotnet build src/ReportingDashboard/ReportingDashboard.csproj --warnaserrors

# Optional: Run tests
dotnet test tests/ReportingDashboard.Tests/ReportingDashboard.Tests.csproj

# Portable distribution
dotnet publish src/ReportingDashboard/ReportingDashboard.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### Deployment Scenarios

| Scenario | Command | Output |
|----------|---------|--------|
| **Development** | `dotnet watch run --project src/ReportingDashboard` | Live at `https://localhost:5001` with hot reload |
| **Production (local)** | `dotnet run --project src/ReportingDashboard -c Release` | Live at `https://localhost:5001` |
| **Portable EXE** | `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` | Single `ReportingDashboard.exe` (~80MB) |
| **Team sharing** | Copy published folder + `data.json` to colleague's machine | No .NET SDK required on target machine |

---

## Technology Stack Decisions

| Layer | Choice | Justification |
|-------|--------|--------------|
| **Framework** | Blazor Server (.NET 8 LTS) | Mandated by project requirements. Provides server-side Razor rendering with no client-side WASM download. Appropriate for local single-user use. |
| **CSS Layout** | CSS Grid + Flexbox (native) | Matches the reference HTML exactly. CSS Grid for the heatmap (`grid-template-columns: 160px repeat(N, 1fr)`), Flexbox for header and page-level vertical layout. No CSS framework (Bootstrap, Tailwind) needed or permitted. |
| **Timeline Rendering** | Inline SVG via Razor markup | The reference design uses ~20 SVG primitives (lines, circles, polygons, text). Hand-coded SVG in `.razor` files gives pixel-perfect control over every coordinate. Charting libraries (BlazorApexCharts, Radzen) were rejected—they add complexity, fight pixel control, and the timeline is not a standard chart type. |
| **JSON Deserialization** | `System.Text.Json` (built-in) | Zero external dependencies. Built into .NET 8. `PropertyNameCaseInsensitive = true` handles camelCase JSON → PascalCase C# mapping. No need for Newtonsoft.Json. |
| **Data Models** | C# `record` types | Immutable by default. Structural equality for comparison. Concise syntax. Perfect for deserialized DTOs that are passed as parameters and never mutated. |
| **Data Storage** | `data.json` flat file | Explicit requirement. Eliminates database complexity. Human-readable, version-controllable, trivial to swap for different projects. |
| **CSS Strategy** | Single `dashboard.css` (no CSS isolation) | The reference HTML has ~100 lines of flat CSS. Copying it into one file is fastest and most maintainable. Blazor CSS isolation adds `.b-xxxxx` attribute selectors that complicate debugging for no benefit in a single-page app. |
| **DI Lifetime** | Singleton for `DashboardDataService` | Data is read once and cached. No per-request or per-circuit lifetime needed. Single-user app with no concurrency concerns. |
| **Interactivity** | Blazor Server interactive render mode | Required for `OnInitializedAsync` to call the data service. Static SSR would also work but interactive mode is mandated and enables Phase 2 features (tooltips, file selector). |

### Technologies Explicitly Rejected

| Technology | Reason for Rejection |
|------------|---------------------|
| **React, Angular, Vue** | Stack is mandated as Blazor Server |
| **BlazorApexCharts / Radzen Charts** | Overkill for ~20 SVG elements; fights pixel-perfect control |
| **Entity Framework / SQLite / LiteDB** | No database needed; `data.json` is the sole data source |
| **Bootstrap / Tailwind CSS** | Reference CSS is custom and must be preserved verbatim |
| **JavaScript / JS Interop** | Explicitly prohibited by constraints |
| **Blazor CSS Isolation** | Adds debugging complexity with no benefit for single-page, single-CSS-file app |
| **Newtonsoft.Json** | `System.Text.Json` is built-in and sufficient |
| **SignalR for data push** | No real-time updates needed |
| **Docker** | Not needed for local-only tool |

### NuGet Package Inventory

| Package | Version | Required? | Notes |
|---------|---------|-----------|-------|
| `Microsoft.AspNetCore.Components` | 8.0.x | Yes (implicit) | Ships with .NET 8 SDK |
| `System.Text.Json` | 8.0.x | Yes (implicit) | Ships with .NET 8 SDK |

**Total external NuGet dependencies for production: 0.**

---

## Security Considerations

### Threat Model

This is a **local-only, single-user, read-only** application with no network exposure beyond `localhost`. The threat surface is minimal.

| Threat | Risk | Mitigation |
|--------|------|-----------|
| **Unauthorized network access** | Very Low | Kestrel binds to `localhost` only by default. No external network access. |
| **Malicious `data.json`** | Very Low | JSON is deserialized with `System.Text.Json` (no `eval`, no code execution). Razor components HTML-encode all output by default, preventing XSS from JSON content. |
| **XSS via work item text** | Very Low | Blazor's Razor engine HTML-encodes all `@variable` output automatically. No `@((MarkupString)...)` or `innerHTML` is used. |
| **HTTPS certificate** | Informational | Kestrel uses a self-signed development certificate. Browser will show a certificate warning on first visit. Acceptable for localhost. |
| **Credential exposure** | None | No credentials, API keys, tokens, or PII exist in the system. `data.json` contains only project metadata. |
| **Dependency supply chain** | None | Zero external NuGet packages. Attack surface is limited to the .NET 8 SDK itself. |

### Authentication & Authorization

**None.** Explicitly out of scope per PM specification. The application has no login page, no identity provider, no cookie auth, no Windows auth, and no authorization middleware.

**Future upgrade path (if needed for team sharing):**
```csharp
// Add to Program.cs for Windows Authentication
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();
builder.Services.AddAuthorization();
```

### Data Protection

- `data.json` contains project names, milestone labels, and work item descriptions. No PII, no credentials, no financial data.
- If project names are sensitive, add `data.json` to `.gitignore` and distribute the file separately.
- No encryption at rest or in transit beyond Kestrel's default HTTPS.

### Input Validation

| Input Source | Validation Approach |
|-------------|-------------------|
| `data.json` (file system) | `System.Text.Json` deserialization with type-safe records. Invalid JSON throws `JsonException`, caught by service. |
| Browser (user) | **No user input.** The dashboard is read-only with no forms, text fields, or interactive controls. The only browser-to-server communication is Blazor's SignalR circuit for rendering. |
| URL parameters | None used. Single route `/` with no query parameters. |

### Content Security

Blazor Server's Razor engine automatically HTML-encodes all rendered content, preventing injection attacks from malicious `data.json` values. The `backlogUrl` field is rendered inside an `<a href="">` tag—Blazor does not prevent `javascript:` URLs, but this is acceptable given the single-user, local-only context where the user controls the JSON file.

---

## Scaling Strategy

### Not Applicable

This is a single-user, local-only, read-only dashboard. There are no scaling requirements. The application serves one user on `localhost` and reads one JSON file.

### Practical Limits

| Dimension | Limit | Constraint |
|-----------|-------|-----------|
| **Concurrent users** | 1 | Single-user localhost tool |
| **Milestone rows** | 1-5 | Vertical space in 1080px viewport; 5 rows fit with 28px row spacing |
| **Heatmap months** | 2-6 | Horizontal space with `160px + repeat(N, 1fr)` columns; 6 months fit at ~240px each |
| **Items per cell** | 4-6 | Vertical space within flex-distributed grid rows; overflow hidden at cell boundary |
| **Events per milestone** | ~10 | SVG viewport width (1560px); label overlap becomes an issue beyond 10 |
| **`data.json` file size** | ≤ 50KB | `System.Text.Json` parses 50KB in < 10ms |

### Multi-Project "Scaling"

The system "scales" to multiple projects by swapping `data.json` files:

1. Create `phoenix.json`, `atlas.json`, `horizon.json` with different project data
2. Copy desired file to `wwwroot/data/data.json`
3. Refresh the browser

Phase 2 enhancement: Add a dropdown file selector that lists JSON files in `wwwroot/data/` and dynamically loads the selected one.

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|-----------|
| 1 | **SVG coordinate math errors** — Timeline markers render at wrong positions due to date-to-pixel calculation bugs | Medium | Medium | Implement `DateToX()` as a pure function with unit tests. Test with known dates from the reference HTML (e.g., Jan 12 → x:104, Mar 26 → x:745). Compare rendered output side-by-side with the reference at each milestone. |
| 2 | **CSS fidelity drift** — Blazor-rendered DOM structure doesn't match reference HTML, causing CSS rules to not apply | Medium | High | Copy CSS verbatim from reference HTML. Ensure Razor components emit identical DOM structure (same class names, same nesting). Use browser DevTools element inspector to compare DOM trees. |
| 3 | **Blazor framework UI leaking into screenshots** — Reconnection overlay, error boundaries, or loading indicators appear in the viewport | Low | High | In `App.razor`, do not include `<ErrorBoundary>` wrappers. Suppress Blazor's reconnect UI with CSS: `#components-reconnect-modal { display: none; }`. Test screenshot capture after a clean page load. |
| 4 | **`data.json` schema changes break deserialization** — Adding/removing fields causes `JsonException` | Medium | Low | Use `PropertyNameCaseInsensitive = true` and avoid `[JsonRequired]` attributes. Missing optional fields deserialize to `null` or empty collections. Add graceful handling for nulls in components. |
| 5 | **Font rendering differences** — Segoe UI not available on macOS/Linux, causing layout shifts | Low | Medium | Segoe UI is guaranteed on Windows (the primary platform). CSS fallback to Arial. Document that screenshots must be captured on Windows for pixel-perfect fidelity. |
| 6 | **CSS Grid rendering differences across browsers** — Edge and Chrome render grid slightly differently | Low | Medium | Both Edge and Chrome use the Blink rendering engine (Chromium). Test at exactly 1920×1080 device emulation in both browsers. Document Chrome as the reference browser for screenshots. |
| 7 | **Heatmap overflow with many items** — More than 6 items in a cell cause vertical overflow, hidden by CSS | Medium | Low | Document the practical limit (4-6 items per cell) in `data.json` authoring guidelines. CSS `overflow: hidden` prevents layout breakage. Report authors are responsible for keeping item counts within limits. |
| 8 | **`dotnet watch` doesn't refresh on `data.json` changes** — Static files in `wwwroot` may not trigger hot reload | Low | Low | `dotnet watch` does detect changes to `wwwroot` files and triggers a browser refresh. If not working, a manual browser refresh suffices. Document this in the README. |
| 9 | **Blazor Server SignalR circuit timeout** — Dashboard goes stale if left open too long | Low | Low | Not a concern for screenshot workflow (open → capture → close). Default circuit timeout is 3 minutes. If needed, increase in `Program.cs`: `options.DisconnectedCircuitMaxRetained`. |
| 10 | **Date label overlap in SVG timeline** — Multiple events on close dates produce overlapping text | Medium | Low | Alternate label positions above/below markers based on event index. For extreme cases (events on same day), the report author should adjust labels in `data.json`. |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component with its CSS layout strategy, data bindings, and interactions.

### Visual Section → Component Mapping

#### Section 1: Header Bar (`Header.razor`)

**Visual Reference:** `.hdr` div in `OriginalDesignConcept.html`

| Aspect | Detail |
|--------|--------|
| **Component** | `Components/Shared/Header.razor` |
| **CSS Layout** | Flexbox row: `display: flex; align-items: center; justify-content: space-between` |
| **CSS Class** | `.hdr` |
| **Dimensions** | Full width (1920px), height determined by content (~50px), `flex-shrink: 0` |
| **Data Bindings** | `Data.Title` → `<h1>` text, `Data.BacklogUrl` → `<a href>`, `Data.Subtitle` → `.sub` div, `Data.CurrentMonth` → NOW legend label |
| **Interactions** | ADO Backlog link is a standard `<a>` tag navigating to `Data.BacklogUrl` on click |
| **Left side** | `<h1>` with 24px bold title + inline anchor link in #0078D4 |
| **Right side** | Legend row: 4 `<span>` elements with inline-styled marker icons (rotated squares for diamonds, circle for checkpoint, rectangle for NOW) |
| **Border** | Bottom: 1px solid #E0E0E0 |

#### Section 2: Timeline Area (`Timeline.razor`)

**Visual Reference:** `.tl-area` div containing left labels and SVG viewport

| Aspect | Detail |
|--------|--------|
| **Component** | `Components/Shared/Timeline.razor` |
| **CSS Layout** | Flexbox row: `display: flex; align-items: stretch` |
| **CSS Class** | `.tl-area` (outer), `.tl-svg-box` (SVG container) |
| **Dimensions** | Full width, fixed height 196px, `flex-shrink: 0`, background #FAFAFA |
| **Data Bindings** | `Data.Milestones` → left labels + SVG rows, `Data.TimelineStartMonth`/`TimelineEndMonth` → X axis range, `Data.CurrentDate` → NOW line position |
| **Interactions** | None (read-only SVG). Phase 2: hover tooltips on markers. |

**Sub-sections:**

| Sub-section | DOM Structure | Layout | Data |
|-------------|---------------|--------|------|
| **Left Panel** (230px) | `<div>` with milestone labels | Flexbox column, `justify-content: space-around` | `Data.Milestones[].Id`, `Label`, `Color` |
| **SVG Viewport** (flex: 1) | `<svg width="1560" height="185">` | Inline SVG with computed coordinates | Gridlines, milestone lines, event markers, NOW line |

**SVG Element Generation:**

| SVG Element | Source Data | Rendering Logic |
|------------|-------------|----------------|
| Month gridlines | Computed from `TimelineStartMonth`/`TimelineEndMonth` | `<line>` at equal X intervals + `<text>` month labels |
| Milestone rows | `Data.Milestones[i]` | `<line>` spanning full width at Y = 42 + (i × 56), stroke = milestone color, width 3 |
| Checkpoint markers | Events where `type == "checkpoint"` | `<circle>` r=5-7, fill white, stroke = milestone color or #888 |
| PoC diamonds | Events where `type == "poc"` | `<polygon>` diamond shape, fill #F4B400, filter drop shadow |
| Production diamonds | Events where `type == "production"` | `<polygon>` diamond shape, fill #34A853, filter drop shadow |
| Date labels | All events | `<text>` 10px, anchor middle, alternating above/below marker |
| NOW line | `Data.CurrentDate` | `<line>` dashed, stroke #EA4335, width 2 + `<text>` "NOW" label |

#### Section 3: Heatmap Grid (`HeatmapGrid.razor` + `HeatmapCell.razor`)

**Visual Reference:** `.hm-wrap` and `.hm-grid` divs

| Aspect | Detail |
|--------|--------|
| **Component** | `Components/Shared/HeatmapGrid.razor` (container) + `HeatmapCell.razor` (cells) |
| **CSS Layout** | CSS Grid: `grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr)` |
| **CSS Class** | `.hm-wrap` (outer flex container), `.hm-grid` (grid), `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell` |
| **Dimensions** | `flex: 1` (fills remaining vertical space below timeline), padding 10px 44px |
| **Data Bindings** | `Data.HeatmapMonths` → column headers + column count, `Data.CurrentMonth` → amber highlight, `Data.Categories` → row headers + cell content |
| **Interactions** | None (read-only grid) |

**Grid Cell Rendering:**

| Cell Type | Component | CSS Classes | Data Source |
|-----------|-----------|-------------|------------|
| Corner | Inline `<div>` | `.hm-corner` | Static text "Status" |
| Month header | Inline `<div>` | `.hm-col-hdr`, `.apr-hdr` (if current) | `Data.HeatmapMonths[i]` |
| Row header | Inline `<div>` | `.hm-row-hdr`, `.{cssClass}-hdr` | `Data.Categories[i].Name` with emoji prefix |
| Data cell | `HeatmapCell.razor` | `.hm-cell`, `.{cssClass}-cell`, `.{currentMonth}` (if current) | `Data.Categories[i].Items[month]` |

**HeatmapCell.razor Internal Structure:**

```html
<!-- Non-empty cell -->
<div class="hm-cell ship-cell apr">
    <div class="it">Gateway GA release</div>     <!-- ::before pseudo-element adds green dot -->
    <div class="it">SSO integration</div>
</div>

<!-- Empty cell -->
<div class="hm-cell ship-cell">
    <div style="color:#CCC;font-size:14px;text-align:center;">–</div>
</div>
```

### Color System Reference

| Category | Row Header BG | Row Header Text | Cell BG | Current Cell BG | Bullet Color | CSS Prefix |
|----------|--------------|----------------|---------|----------------|-------------|-----------|
| Shipped | #E8F5E9 | #1B7A28 | #F0FBF0 | #D8F2DA | #34A853 | `ship` |
| In Progress | #E3F2FD | #1565C0 | #EEF4FE | #DAE8FB | #0078D4 | `prog` |
| Carryover | #FFF8E1 | #B45309 | #FFFDE7 | #FFF0B0 | #F4B400 | `carry` |
| Blockers | #FEF2F2 | #991B1B | #FFF5F5 | #FFE4E4 | #EA4335 | `block` |

### Dynamic CSS Behavior

The following CSS properties are set dynamically via inline `style` attributes in Razor, since they depend on `data.json` content:

| Element | Dynamic Style | Source |
|---------|--------------|--------|
| `.hm-grid` | `grid-template-columns: 160px repeat(@count, 1fr)` | `Data.HeatmapMonths.Count` |
| `.hm-grid` | `grid-template-rows: 36px repeat(@count, 1fr)` | `Data.Categories.Count` |
| Milestone label color | `color: @milestone.Color` | `Data.Milestones[i].Color` |
| SVG milestone line stroke | `stroke="@milestone.Color"` | `Data.Milestones[i].Color` |
| SVG element positions | `cx`, `cy`, `x1`, `y1`, `points` attributes | Computed from `DateToX()` and row index |