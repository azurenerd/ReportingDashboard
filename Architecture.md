# Architecture

**Project:** Executive Project Reporting Dashboard
**Version:** 1.0
**Date:** April 16, 2026
**Stack:** C# .NET 8 · Blazor Server · Static SSR · Local Only

---

## Overview & Goals

The Executive Project Reporting Dashboard is a single-page, read-only Blazor Server application that renders project milestone timelines and monthly execution heatmaps from a local `data.json` file. The output is a pixel-exact 1920×1080 HTML page optimized for direct screenshot capture into PowerPoint decks.

### Architectural Goals

| # | Goal | Architectural Response |
|---|------|----------------------|
| 1 | **75% reduction in reporting prep time** | Single `data.json` edit → browser refresh → screenshot. No manual slide layout. |
| 2 | **Consistent visual format** | All rendering driven by a single CSS stylesheet extracted from the canonical HTML design reference. Zero variation between renders. |
| 3 | **Non-technical data updates** | JSON file is the sole data source. No code changes, no database migrations, no deployments. |
| 4 | **Screenshot-quality fidelity** | Fixed 1920×1080 viewport with `overflow: hidden`. Static SSR delivers complete HTML in a single response—no hydration, no spinners. |
| 5 | **$0 operational cost** | Runs entirely on localhost via `dotnet run`. Zero cloud services, zero licenses, zero NuGet packages beyond the SDK. |

### Architectural Principles

1. **Simplicity over abstraction.** No repository pattern, no CQRS, no state management, no service interfaces. One service, one page, one data file.
2. **Fidelity over flexibility.** The CSS and markup must match `OriginalDesignConcept.html` pixel-for-pixel. Do not introduce layout abstractions that deviate from the reference.
3. **Static over interactive.** Static SSR only. No SignalR WebSocket, no JS interop, no client-side interactivity beyond a single hyperlink.
4. **Complexity budget.** < 15 source files, < 1,000 LOC, 0 external NuGet packages, 0 JavaScript.

---

## System Components

### Component 1: `Program.cs` — Application Host

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure Kestrel, register services, map Razor component endpoints |
| **Interfaces** | None (entry point) |
| **Dependencies** | .NET 8 SDK, `DashboardDataService` |
| **Data** | Reads configuration from `appsettings.json` for port binding |

**Key implementation details:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register the single data service
builder.Services.AddSingleton<DashboardDataService>();

// Static SSR only — no interactive server components
builder.Services.AddRazorComponents();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>();

app.Run();
```

**Kestrel configuration** in `Properties/launchSettings.json`:
- `applicationUrl`: `http://localhost:5000`
- No HTTPS profile
- No launch browser (optional: enable for dev convenience)

---

### Component 2: `DashboardDataService` — Data Access Layer

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Read, deserialize, and validate `data.json`; expose strongly-typed `DashboardData` to components |
| **Interfaces** | `DashboardData? GetData()` and `string? GetError()` |
| **Dependencies** | `System.Text.Json`, `System.IO` |
| **Data** | Reads `Data/data.json` from the application's content root |
| **Lifetime** | Singleton (registered in DI) |
| **File Location** | `Data/DashboardDataService.cs` |

**Behavior:**

1. On first access (lazy initialization), reads `Data/data.json` from disk using `File.ReadAllText`.
2. Deserializes using `System.Text.Json` with `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`.
3. If the file is missing, stores a user-friendly error string: `"Error: data.json not found. Please create a data.json file in the Data/ directory."`
4. If the JSON is malformed, stores the `JsonException` message with line/column info: `"Error parsing data.json: {ex.Message}"`.
5. If required fields are null after deserialization, stores: `"Error: Required field '{fieldName}' is missing in data.json."`.
6. On each HTTP request, re-reads the file from disk. This ensures edits to `data.json` are reflected on browser refresh without restarting the server. The file is < 50KB and reads in < 1ms—no caching needed.

**Why re-read on every request (not cached singleton):** The PM spec requires that "changing `data.json` and refreshing the browser reflects updated data (no restart required)." Reading a < 50KB file per request is negligible overhead and dramatically simpler than `FileSystemWatcher` + cache invalidation.

```csharp
public class DashboardDataService
{
    private readonly string _dataFilePath;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataFilePath = Path.Combine(env.ContentRootPath, "Data", "data.json");
    }

    public (DashboardData? Data, string? Error) Load()
    {
        if (!File.Exists(_dataFilePath))
            return (null, "Error: data.json not found. Please create a data.json file in the Data/ directory.");

        try
        {
            var json = File.ReadAllText(_dataFilePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data is null)
                return (null, "Error: data.json deserialized to null.");

            var validation = Validate(data);
            if (validation is not null)
                return (null, validation);

            return (data, null);
        }
        catch (JsonException ex)
        {
            return (null, $"Error parsing data.json: {ex.Message}");
        }
    }

    private string? Validate(DashboardData data)
    {
        if (data.Header is null) return "Error: Required field 'header' is missing in data.json.";
        if (string.IsNullOrEmpty(data.Header.Title)) return "Error: Required field 'header.title' is missing in data.json.";
        if (data.Heatmap is null) return "Error: Required field 'heatmap' is missing in data.json.";
        if (data.Timeline is null) return "Error: Required field 'timeline' is missing in data.json.";
        return null;
    }
}
```

---

### Component 3: `DashboardData.cs` — Data Models

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Strongly-typed C# records representing the `data.json` schema |
| **Interfaces** | None (POCOs) |
| **Dependencies** | `System.Text.Json.Serialization` |
| **File Location** | `Data/DashboardData.cs` |

**Complete model hierarchy:**

```csharp
public record DashboardData
{
    public HeaderData Header { get; init; } = default!;
    public TimelineData Timeline { get; init; } = default!;
    public HeatmapData Heatmap { get; init; } = default!;
}

public record HeaderData
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogUrl { get; init; } = "";
    public string BacklogLinkText { get; init; } = "→ ADO Backlog";
    public string CurrentMonth { get; init; } = "";
    public List<LegendItem> Legend { get; init; } = new();
}

public record LegendItem
{
    public string Label { get; init; } = "";
    public string MarkerType { get; init; } = "";   // "poc-diamond", "prod-diamond", "checkpoint", "now-line"
    public string Color { get; init; } = "";
}

public record TimelineData
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public List<TimelineMonth> Months { get; init; } = new();
    public List<TimelineTrack> Tracks { get; init; } = new();
}

public record TimelineMonth
{
    public string Label { get; init; } = "";         // "Jan", "Feb", etc.
    public DateTime StartDate { get; init; }
}

public record TimelineTrack
{
    public string Code { get; init; } = "";          // "M1", "M2", "M3"
    public string Description { get; init; } = "";   // "Chatbot & MS Role"
    public string Color { get; init; } = "";         // "#0078D4"
    public List<Milestone> Milestones { get; init; } = new();
}

public record Milestone
{
    public string Label { get; init; } = "";
    public DateTime Date { get; init; }
    public string Type { get; init; } = "";          // "start", "checkpoint", "minor", "poc", "production"
    public string LabelPosition { get; init; } = "above"; // "above" or "below"
}

public record HeatmapData
{
    public List<string> Columns { get; init; } = new();    // ["Jan", "Feb", "Mar", "Apr"]
    public string HighlightColumn { get; init; } = "";     // "Apr"
    public List<HeatmapRow> Rows { get; init; } = new();
}

public record HeatmapRow
{
    public string Category { get; init; } = "";      // "Shipped", "In Progress", "Carryover", "Blockers"
    public string Theme { get; init; } = "";         // "shipped", "progress", "carryover", "blockers"
    public Dictionary<string, List<string>> Items { get; init; } = new();
}
```

---

### Component 4: `Dashboard.razor` — Page Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Top-level page; injects `DashboardDataService`, orchestrates child components or renders error state |
| **Interfaces** | Route: `/` (default page) |
| **Dependencies** | `DashboardDataService`, `Header.razor`, `Timeline.razor`, `Heatmap.razor` |
| **Render Mode** | Static SSR (no `@rendermode` directive) |
| **File Location** | `Components/Pages/Dashboard.razor` |

**Behavior:**

```razor
@page "/"
@inject DashboardDataService DataService

@{
    var (data, error) = DataService.Load();
}

@if (error is not null)
{
    <div class="error-container">
        <p class="error-message">@error</p>
    </div>
}
else
{
    <div class="hdr">
        <Header Data="@data!.Header" />
    </div>
    <div class="tl-area">
        <Timeline Data="@data!.Timeline" />
    </div>
    <div class="hm-wrap">
        <Heatmap Data="@data!.Heatmap" />
    </div>
}
```

---

### Component 5: `Header.razor` — Header & Legend

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render project title, subtitle, ADO backlog link, and legend markers |
| **Interfaces** | `[Parameter] public HeaderData Data { get; set; }` |
| **Dependencies** | None |
| **Visual Reference** | `.hdr` element in `OriginalDesignConcept.html` |
| **File Location** | `Components/Dashboard/Header.razor` |

**Renders:**
- Left side: `<h1>` with title + inline `<a>` for backlog link; subtitle `<div>` below
- Right side: Legend bar with four marker types using CSS shapes (rotated squares, circles, vertical bars)
- All text and URLs sourced from `HeaderData` parameter

---

### Component 6: `Timeline.razor` — SVG Milestone Timeline

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the complete SVG timeline: track label panel + inline SVG with month grid, track lines, milestones, and NOW line |
| **Interfaces** | `[Parameter] public TimelineData Data { get; set; }` |
| **Dependencies** | None |
| **Visual Reference** | `.tl-area` element in `OriginalDesignConcept.html` |
| **File Location** | `Components/Dashboard/Timeline.razor` |

**SVG generation logic (C# methods in `@code` block):**

```csharp
private const int SvgWidth = 1560;
private const int SvgHeight = 185;

// Date-to-X-coordinate calculation
private double DateToX(DateTime date)
{
    var totalDays = (Data.EndDate - Data.StartDate).TotalDays;
    var dayOffset = (date - Data.StartDate).TotalDays;
    return Math.Clamp(dayOffset / totalDays * SvgWidth, 0, SvgWidth);
}

// Y position per track (evenly spaced, matching reference)
private double TrackY(int trackIndex, int trackCount)
{
    // Reference positions: track 0 → y=42, track 1 → y=98, track 2 → y=154
    // Formula: 42 + (trackIndex * 56) for 3 tracks
    var spacing = (SvgHeight - 30) / (trackCount + 1);  // 30px reserved for month labels
    return 28 + (trackIndex + 1) * spacing / 1.1;
}

// Diamond polygon points for milestone markers
private string DiamondPoints(double cx, double cy, int radius = 11)
{
    return $"{cx},{cy - radius} {cx + radius},{cy} {cx},{cy + radius} {cx - radius},{cy}";
}
```

**SVG elements rendered (in order):**
1. `<defs>` with drop shadow filter
2. Month grid lines (`<line>` vertical, `stroke="#bbb"`, `stroke-opacity="0.4"`)
3. Month labels (`<text>`, `font-size="11"`, `fill="#666"`)
4. NOW line (`<line>` dashed, `stroke="#EA4335"`, with "NOW" `<text>` label)
5. Per-track: horizontal `<line>` in track color, then milestone markers
6. Milestone markers by type:
   - `start` → `<circle r="7" fill="white" stroke="{trackColor}" stroke-width="2.5">`
   - `checkpoint` → `<circle r="5" fill="white" stroke="#888" stroke-width="2.5">`
   - `minor` → `<circle r="4" fill="#999">`
   - `poc` → `<polygon>` diamond, `fill="#F4B400"`, `filter="url(#sh)"`
   - `production` → `<polygon>` diamond, `fill="#34A853"`, `filter="url(#sh)"`
7. Date labels for each milestone (`<text>`, `font-size="10"`, `fill="#666"`, positioned above or below)

**Track label panel** (left 230px):
- Rendered as a `<div>` with flex-column layout
- Each track: code in `font-weight: 600` + track color, description in `color: #444`

---

### Component 7: `Heatmap.razor` — Execution Heatmap Grid

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the CSS Grid heatmap with column headers, row headers, and data cells |
| **Interfaces** | `[Parameter] public HeatmapData Data { get; set; }` |
| **Dependencies** | None |
| **Visual Reference** | `.hm-wrap` and `.hm-grid` elements in `OriginalDesignConcept.html` |
| **File Location** | `Components/Dashboard/Heatmap.razor` |

**Renders:**
1. Section title: "MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS"
2. CSS Grid container with dynamic column count:
   - `grid-template-columns: 160px repeat(@Data.Columns.Count, 1fr)`
   - `grid-template-rows: 36px repeat(4, 1fr)`
3. Corner cell ("STATUS")
4. Column headers (month names), with highlight column getting `.highlight-hdr` class
5. For each row in `Data.Rows`:
   - Row header with category name, themed CSS class
   - For each column: cell with work items or empty-state dash

**CSS class mapping (theme → CSS prefix):**

| `Theme` value | Row header class | Cell class | Highlight cell class | Dot color |
|---------------|-----------------|------------|---------------------|-----------|
| `shipped` | `ship-hdr` | `ship-cell` | `ship-cell highlight` | `#34A853` |
| `progress` | `prog-hdr` | `prog-cell` | `prog-cell highlight` | `#0078D4` |
| `carryover` | `carry-hdr` | `carry-cell` | `carry-cell highlight` | `#F4B400` |
| `blockers` | `block-hdr` | `block-cell` | `block-cell highlight` | `#EA4335` |

**Empty cell handling:**
```razor
@if (items is null || items.Count == 0)
{
    <span style="color:#AAA;font-size:12px;">–</span>
}
else
{
    @foreach (var item in items)
    {
        <div class="it">@item</div>
    }
}
```

---

### Component 8: `App.razor` — Root Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | HTML document shell: `<html>`, `<head>`, `<body>`, CSS link, routes |
| **File Location** | `Components/App.razor` |

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
</body>
</html>
```

**Critical:** No `<script src="_framework/blazor.web.js">` tag. Static SSR does not require the Blazor JS runtime. Omitting it ensures no WebSocket connection attempt, no hydration delay, and a clean screenshot.

---

### Component 9: `dashboard.css` — Stylesheet

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | All visual styling; extracted from `OriginalDesignConcept.html` with CSS custom properties added |
| **File Location** | `wwwroot/css/dashboard.css` |

**Strategy:** Copy the `<style>` block from `OriginalDesignConcept.html` verbatim as the baseline. Then refactor color literals into CSS custom properties for maintainability. The class names (`.hdr`, `.tl-area`, `.hm-wrap`, `.hm-grid`, `.ship-cell`, etc.) remain identical to the reference to ensure pixel-exact fidelity.

**CSS custom properties added:**

```css
:root {
    /* Layout */
    --page-width: 1920px;
    --page-height: 1080px;
    --page-padding: 44px;

    /* Status colors */
    --shipped: #34A853;
    --shipped-header: #1B7A28;
    --shipped-header-bg: #E8F5E9;
    --shipped-cell-bg: #F0FBF0;
    --shipped-highlight-bg: #D8F2DA;

    --progress: #0078D4;
    --progress-header: #1565C0;
    --progress-header-bg: #E3F2FD;
    --progress-cell-bg: #EEF4FE;
    --progress-highlight-bg: #DAE8FB;

    --carryover: #F4B400;
    --carryover-header: #B45309;
    --carryover-header-bg: #FFF8E1;
    --carryover-cell-bg: #FFFDE7;
    --carryover-highlight-bg: #FFF0B0;

    --blockers: #EA4335;
    --blockers-header: #991B1B;
    --blockers-header-bg: #FEF2F2;
    --blockers-cell-bg: #FFF5F5;
    --blockers-highlight-bg: #FFE4E4;

    /* Current month highlight */
    --highlight-header-bg: #FFF0D0;
    --highlight-header-text: #C07700;
}
```

**Dynamic grid columns:** The heatmap grid's column count is data-driven. Since CSS Grid's `repeat()` value must match `data.json`, the `Heatmap.razor` component sets the `grid-template-columns` via an inline `style` attribute:

```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Data.Columns.Count, 1fr);">
```

---

### Component 10: `data.json` — Data File

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Sole data source for all dashboard content |
| **File Location** | `Data/data.json` |
| **Schema** | Maps 1:1 to `DashboardData` C# record hierarchy |

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────┐     File.ReadAllText()      ┌─────────────────────┐
│  data.json  │ ──────────────────────────→  │ DashboardDataService │
│  (Data/)    │                              │   .Load()            │
└─────────────┘                              └──────────┬──────────┘
                                                        │
                                            (DashboardData, Error)
                                                        │
                                                        ▼
                                             ┌────────────────────┐
                                             │  Dashboard.razor   │
                                             │  (@page "/")       │
                                             └──┬──────┬───────┬──┘
                                                │      │       │
                        ┌───────────────────────┘      │       └───────────────────────┐
                        ▼                              ▼                               ▼
              ┌──────────────────┐         ┌────────────────────┐           ┌────────────────────┐
              │  Header.razor    │         │  Timeline.razor    │           │  Heatmap.razor     │
              │  [Parameter]     │         │  [Parameter]       │           │  [Parameter]       │
              │  HeaderData      │         │  TimelineData      │           │  HeatmapData       │
              └──────────────────┘         └────────────────────┘           └────────────────────┘
```

### Request Lifecycle

1. **Browser** sends `GET /` to Kestrel on `localhost:5000`
2. **ASP.NET Core** routes to `Dashboard.razor` (the `@page "/"` component)
3. **Dashboard.razor** calls `DashboardDataService.Load()` synchronously during render
4. **DashboardDataService** reads `Data/data.json` from disk, deserializes, validates
5. If **error**: `Dashboard.razor` renders the error message `<div>` and returns
6. If **success**: `Dashboard.razor` renders child components, passing typed data via `[Parameter]`
7. **Header.razor**, **Timeline.razor**, **Heatmap.razor** each render their HTML/SVG fragment
8. **Kestrel** returns the complete HTML document (single response, no streaming)
9. **Browser** renders the page. No JavaScript executes. Page is screenshot-ready.

### Communication Patterns

| Pattern | Usage |
|---------|-------|
| **Dependency Injection** | `DashboardDataService` registered as singleton, injected via `@inject` in `Dashboard.razor` |
| **Component Parameters** | Parent-to-child data passing via `[Parameter]` attributes. No cascading values. |
| **No events/callbacks** | Child components are pure render-only. No `EventCallback`, no two-way binding. |
| **No inter-component communication** | Components do not communicate with each other. All data flows from `Dashboard.razor` downward. |

---

## Data Model

### Entity Relationship

```
DashboardData (root)
├── HeaderData (1:1)
│   └── LegendItem (1:N, typically 4)
├── TimelineData (1:1)
│   ├── TimelineMonth (1:N, typically 4-6)
│   └── TimelineTrack (1:N, typically 1-5)
│       └── Milestone (1:N per track)
└── HeatmapData (1:1)
    └── HeatmapRow (1:N, exactly 4: shipped/progress/carryover/blockers)
        └── Items: Dictionary<string, List<string>> (column → work item descriptions)
```

### Storage

- **Storage mechanism:** Single JSON file (`Data/data.json`) on the local filesystem.
- **No database.** No SQLite, no Entity Framework, no in-memory database.
- **File size:** Expected < 50KB for typical use (4-6 months, 3-5 tracks, 4 status rows).
- **Encoding:** UTF-8.
- **Read pattern:** Synchronous `File.ReadAllText()` on every HTTP request. No caching layer.

### Sample `data.json` Structure

```json
{
  "header": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "backlogLinkText": "→ ADO Backlog",
    "currentMonth": "Apr",
    "legend": [
      { "label": "PoC Milestone", "markerType": "poc-diamond", "color": "#F4B400" },
      { "label": "Production Release", "markerType": "prod-diamond", "color": "#34A853" },
      { "label": "Checkpoint", "markerType": "checkpoint", "color": "#999" },
      { "label": "Now (Apr 2026)", "markerType": "now-line", "color": "#EA4335" }
    ]
  },
  "timeline": {
    "startDate": "2025-12-15",
    "endDate": "2026-07-01",
    "months": [
      { "label": "Jan", "startDate": "2026-01-01" },
      { "label": "Feb", "startDate": "2026-02-01" },
      { "label": "Mar", "startDate": "2026-03-01" },
      { "label": "Apr", "startDate": "2026-04-01" },
      { "label": "May", "startDate": "2026-05-01" },
      { "label": "Jun", "startDate": "2026-06-01" }
    ],
    "tracks": [
      {
        "code": "M1",
        "description": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "label": "Jan 12", "date": "2026-01-12", "type": "start", "labelPosition": "above" },
          { "label": "Mar 26 PoC", "date": "2026-03-26", "type": "poc", "labelPosition": "below" },
          { "label": "Apr Prod (TBD)", "date": "2026-04-15", "type": "production", "labelPosition": "above" }
        ]
      },
      {
        "code": "M2",
        "description": "PDS & Data Inventory",
        "color": "#00897B",
        "milestones": [
          { "label": "Dec 19", "date": "2025-12-19", "type": "start", "labelPosition": "above" },
          { "label": "Feb 11", "date": "2026-02-11", "type": "checkpoint", "labelPosition": "above" },
          { "label": "Mar 18 PoC", "date": "2026-03-18", "type": "poc", "labelPosition": "above" }
        ]
      },
      {
        "code": "M3",
        "description": "Auto Review DFD",
        "color": "#546E7A",
        "milestones": [
          { "label": "Mar 3", "date": "2026-03-03", "type": "start", "labelPosition": "above" },
          { "label": "Jun Prod", "date": "2026-06-15", "type": "production", "labelPosition": "above" }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "highlightColumn": "Apr",
    "rows": [
      {
        "category": "✓ Shipped",
        "theme": "shipped",
        "items": {
          "Jan": ["Privacy chatbot v0.1 live", "MS Role mapping defined"],
          "Feb": ["PDS schema validated", "DI scanning pipeline"],
          "Mar": ["Chatbot PoC accepted", "4 PDS connectors live"],
          "Apr": ["Auto-review prototype"]
        }
      },
      {
        "category": "🔄 In Progress",
        "theme": "progress",
        "items": {
          "Jan": ["Chatbot prompt tuning"],
          "Feb": ["Chatbot context window"],
          "Mar": ["PDS auto-classify"],
          "Apr": ["MS Role integration", "DFD template engine"]
        }
      },
      {
        "category": "⏳ Carryover",
        "theme": "carryover",
        "items": {
          "Jan": [],
          "Feb": ["Chatbot multi-turn (from Jan)"],
          "Mar": ["PDS edge cases (from Feb)"],
          "Apr": ["PDS auto-classify (from Mar)"]
        }
      },
      {
        "category": "🚫 Blockers",
        "theme": "blockers",
        "items": {
          "Jan": [],
          "Feb": [],
          "Mar": ["AAD token refresh issue"],
          "Apr": ["Capacity: 2 engineers on PTO"]
        }
      }
    ]
  }
}
```

---

## API Contracts

This application has **no REST API, no GraphQL, no gRPC endpoints**. It is a server-rendered HTML page only.

### Single Endpoint

| Method | Path | Response | Content-Type |
|--------|------|----------|-------------|
| `GET` | `/` | Complete HTML document (1920×1080 dashboard or error message) | `text/html` |

### Request

No query parameters, no headers, no authentication tokens, no cookies.

```
GET / HTTP/1.1
Host: localhost:5000
```

### Response — Success (200 OK)

Complete HTML document containing:
- `<!DOCTYPE html>` with `<head>` (CSS link) and `<body>` (dashboard markup)
- All data rendered inline—no AJAX calls, no lazy loading
- No `<script>` tags
- Content-Length: typically 15-30KB depending on data volume

### Response — Error (200 OK with error content)

Same HTTP 200 status (not a 500), but the `<body>` contains only:

```html
<div class="error-container">
    <p class="error-message">Error: data.json not found. Please create a data.json file in the Data/ directory.</p>
</div>
```

**Error styling:**
```css
.error-container {
    width: 1920px; height: 1080px;
    display: flex; align-items: center; justify-content: center;
    background: #FFFFFF;
}
.error-message {
    font-size: 16px; color: #EA4335;
    font-family: 'Segoe UI', Arial, sans-serif;
}
```

### Error Scenarios

| Condition | Error Message |
|-----------|--------------|
| `data.json` missing | `Error: data.json not found. Please create a data.json file in the Data/ directory.` |
| Invalid JSON syntax | `Error parsing data.json: '{JsonException.Message}'` (includes line/column) |
| Missing required field | `Error: Required field '{fieldName}' is missing in data.json.` |
| Null deserialization | `Error: data.json deserialized to null.` |

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|--------------|
| **.NET SDK** | 8.0.300 or later |
| **OS** | Windows, macOS, or Linux (any OS supported by .NET 8) |
| **Browser** | Chrome 120+, Edge 120+, or Firefox 120+ |
| **Memory** | < 50MB RSS |
| **Disk** | < 5MB for source; < 65MB for self-contained publish |
| **Network** | `localhost` only. No outbound connections. No inbound connections from other machines. |
| **Port** | `5000` (HTTP). Configurable via `launchSettings.json`. |

### Hosting

**Local Kestrel web server** — built into ASP.NET Core. No IIS, no Nginx, no Docker, no cloud hosting.

```
dotnet run                    → Development mode (http://localhost:5000)
dotnet watch                  → Development mode with hot reload
dotnet publish -c Release     → Production-ready output
```

### Networking

- **Protocol:** HTTP only. No HTTPS.
- **Binding:** `localhost` only (not `0.0.0.0`). Not accessible from other machines on the network.
- **Firewall:** No ports need to be opened. No external connectivity.

### Storage

| Item | Location | Size |
|------|----------|------|
| Source code | Repository root | < 1MB |
| `data.json` | `Data/data.json` (relative to project) | < 50KB |
| Build output | `bin/Debug/net8.0/` | ~5MB |
| Published output | `publish/` | ~65MB (self-contained) or ~5MB (framework-dependent) |

### CI/CD

**None required.** This is a local-only tool. No build pipeline, no deployment pipeline, no artifact registry.

**Optional future enhancement:** A GitHub Action that builds and runs Playwright screenshot tests against the sample `data.json`.

### `launchSettings.json`

```json
{
  "profiles": {
    "ReportingDashboard": {
      "commandName": "Project",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## Technology Stack Decisions

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Runtime** | .NET 8 | 8.0.x (LTS) | Mandatory per project requirements. LTS ensures long-term support through Nov 2026. |
| **Web Framework** | ASP.NET Core Blazor Server | 8.0.x | Mandatory. Static SSR mode delivers complete HTML per request—ideal for screenshot capture. |
| **Render Mode** | Static SSR | N/A | No WebSocket, no hydration delay, no JS runtime. Page is a pure HTML document. |
| **Language** | C# 12 | .NET 8 default | Mandatory. Records, pattern matching, and string interpolation make the code concise. |
| **JSON Parser** | `System.Text.Json` | Built-in | Zero NuGet packages. `PropertyNameCaseInsensitive` handles JSON-to-C# property name mapping. |
| **CSS Framework** | None (hand-written CSS) | N/A | The reference HTML design is directly translatable. Any CSS framework would fight the pixel-exact layout. |
| **CSS Layout** | CSS Grid + Flexbox | Native | The reference design uses both. Grid for the heatmap, Flexbox for header and timeline. |
| **SVG** | Inline SVG in Razor | N/A | The reference design uses inline SVG. Server-side generation in Razor is cleaner than a JS charting library. |
| **Charting Library** | None | N/A | The timeline is simple enough (lines, circles, polygons) that a charting library adds complexity without benefit. |
| **UI Component Library** | None | N/A | MudBlazor/Radzen would introduce 200KB+ of CSS/JS, theming conflicts, and unnecessary abstraction. |
| **JavaScript** | None | N/A | Mandatory constraint. All logic is server-side C# and Razor. |
| **Database** | None | N/A | `data.json` is the sole data source. A database would be massive over-engineering. |
| **Authentication** | None | N/A | Local-only tool. No users, no roles, no tokens. |
| **Font** | Segoe UI (system) | N/A | Already on Windows. Falls back to Arial on macOS/Linux. No web font loading. |

### Decisions Explicitly Rejected

| Technology | Reason for Rejection |
|-----------|---------------------|
| Entity Framework | No database exists. Adding EF for a JSON file read is absurd. |
| MudBlazor / Radzen / Syncfusion | CSS/JS bloat, theming conflicts with pixel-exact design, unnecessary for static content. |
| Bootstrap / Tailwind | Reference design has precise pixel values. Utility classes would fight the spec. |
| SignalR / Interactive Server | Dashboard is static. WebSocket adds latency and complexity for zero benefit. |
| Serilog / Application Insights | Local-only tool. `Console.WriteLine` suffices for the rare debugging need. |
| Swagger / OpenAPI | No API exists. |
| Docker | Local `dotnet run` is simpler. Docker adds a layer of complexity for zero benefit. |
| Playwright (MVP) | Manual screenshot is sufficient for MVP. Playwright is a Phase 3 enhancement. |

---

## Security Considerations

### Threat Model

This application has a **minimal threat surface** because:
- It runs on `localhost` only—not exposed to any network
- It has no authentication, no session state, no cookies
- It reads a single local file and renders HTML—no user input processing
- It makes no outbound network connections

### Input Validation

| Input | Validation |
|-------|-----------|
| `data.json` content | `System.Text.Json` deserialization with `try/catch` for `JsonException`. Null-check on all required fields. |
| HTTP request parameters | None accepted. The single `GET /` endpoint takes no query strings, no headers, no body. |
| User-provided strings in JSON | Rendered via Razor's `@` syntax, which **automatically HTML-encodes** all output. No XSS risk. |

### Data Protection

- `data.json` stays on the local filesystem. Never transmitted over the network.
- No PII in the data model (project names, milestone dates, work item descriptions only).
- No secrets (API keys, connection strings, tokens) in the application or data file.
- No telemetry, no analytics, no phone-home behavior.

### Content Security

- No `<script>` tags in the rendered HTML.
- No external resource loading (no CDN links, no web fonts, no analytics pixels).
- The application could optionally add a strict CSP header, but it's unnecessary for localhost.

### HTTPS

Intentionally disabled. The application binds to `http://localhost:5000`. HTTPS adds certificate management complexity for zero security benefit on a local-only tool.

---

## Scaling Strategy

### This Application Does Not Scale (And Should Not)

This is a single-user, single-machine, local development tool. Scaling is **explicitly out of scope**.

| Dimension | Current State | If Ever Needed |
|-----------|--------------|----------------|
| **Concurrent users** | 1 (the person running it) | Not applicable—this is a local tool |
| **Data volume** | One `data.json` < 50KB | Support up to 12 months × 5 tracks × 20 items/cell without layout overflow |
| **Multiple projects** | One project per `dotnet run` instance | Future: multiple JSON files selected via URL parameter (`/?project=alpha`) |
| **Distribution** | Author's machine only | `dotnet publish --self-contained` produces a ~65MB distributable zip |
| **Render performance** | < 200ms server render time | Already exceeds requirements by 10x for expected data sizes |

### Horizontal Limits Built Into the Design

| Element | Supported Range | Layout Constraint |
|---------|----------------|-------------------|
| Heatmap columns (months) | 4–6 | Grid columns divide `(1920 - 160 - 88)px` / N. Below 4: too sparse. Above 6: cells become too narrow for text. |
| Timeline tracks | 1–5 | 196px container height / track spacing. 5 tracks = 31px per track (tight but viable). |
| Work items per cell | 0–8 | Each item is ~16px tall. Cell height = `(1080 - 50 - 196 - 48 - 36) / 4 ≈ 187px`. Max ~11 items before overflow clip. |
| Legend items | 4 (fixed) | Header height is auto-sized but designed for exactly 4 legend items. |

---

## Risks & Mitigations

### Risk 1: SVG Timeline Date-to-Pixel Calculation Errors

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium |
| **Impact** | Medium — milestones render at wrong positions |
| **Description** | The `DateToX()` function must correctly map dates to pixel positions across variable-length months. Off-by-one errors or timezone issues could misposition markers. |
| **Mitigation** | Use the reference SVG's hardcoded coordinates as test fixtures. For example, the reference places the Jan grid line at x=0, Feb at x=260, Mar at x=520 for a Jan–Jun span across 1560px. Build a unit test that verifies `DateToX(new DateTime(2026, 2, 1))` returns approximately 260 for the reference date range. Use `DateTime` (not `DateTimeOffset`) and ignore timezones—all dates are calendar dates without time components. |

### Risk 2: CSS Drift from Reference Design

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Medium — dashboard looks different from the approved design |
| **Description** | If CSS is written from scratch rather than extracted from the reference, subtle differences in padding, font sizes, or colors will accumulate. |
| **Mitigation** | Start by copying the `<style>` block from `OriginalDesignConcept.html` verbatim into `dashboard.css`. Only then refactor into custom properties. Keep the same class names (`.hdr`, `.hm-grid`, `.ship-cell`, etc.) to ensure 1:1 correspondence. Verify with side-by-side screenshot comparison. |

### Risk 3: `data.json` Schema Errors by Non-Technical Users

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium |
| **Impact** | Low — user sees error message and can self-diagnose |
| **Description** | PMs editing `data.json` by hand may introduce syntax errors (trailing commas, unquoted keys) or structural errors (wrong field names, missing sections). |
| **Mitigation** | (1) The `DashboardDataService` catches all `JsonException` errors and displays line/column info. (2) Field-level validation reports specific missing fields. (3) Provide a `data.schema.json` file (JSON Schema) that enables VS Code / Visual Studio IntelliSense and validation as the user types. (4) Include a comprehensive sample `data.json` that demonstrates every feature. |

### Risk 4: Over-Engineering

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | High |
| **Impact** | High — wasted time, unnecessary complexity, harder to maintain |
| **Description** | Developers may add abstractions (interfaces for the data service, repository pattern, DI registration gymnastics, state management) that are completely unnecessary for a single-page, single-data-source, read-only dashboard. |
| **Mitigation** | Enforce the complexity budget ruthlessly: < 15 files, < 1,000 LOC, 0 NuGet packages, 0 JavaScript. Every file and every abstraction must justify its existence. If someone proposes adding a NuGet package, the default answer is "no." |

### Risk 5: Blazor Static SSR Generates Unexpected Markup

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Low — extra attributes don't affect visual rendering |
| **Description** | Blazor may inject extra attributes (`blazor-enhanced-nav`, component markers) into the rendered HTML that could theoretically affect CSS selectors. |
| **Mitigation** | Test the rendered HTML by viewing source in the browser. Blazor static SSR produces clean HTML with minimal framework artifacts. If any framework-injected elements cause layout issues, override with CSS specificity. |

### Risk 6: Segoe UI Font Unavailable on macOS/Linux

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium (for macOS/Linux users) |
| **Impact** | Low — visual difference in font rendering only |
| **Description** | Segoe UI is a Windows system font. macOS and Linux will fall back to Arial, which has slightly different character widths. This could cause minor text wrapping differences. |
| **Mitigation** | The CSS `font-family` already specifies `'Segoe UI', Arial, sans-serif`. Arial is visually similar and available on all platforms. For pixel-perfect screenshots, use Windows (the primary target platform for this tool). |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component.

### Section 1: Header → `Header.razor`

| Aspect | Detail |
|--------|--------|
| **Visual section** | `.hdr` element — top bar with title, subtitle, legend |
| **Component** | `Components/Dashboard/Header.razor` |
| **CSS layout** | `display: flex; justify-content: space-between; align-items: center; padding: 12px 44px 10px;` |
| **CSS classes** | `.hdr`, `.hdr h1`, `.sub` (from reference) |
| **Data bindings** | `@Data.Title`, `@Data.BacklogUrl`, `@Data.BacklogLinkText`, `@Data.Subtitle`, `@foreach Data.Legend` |
| **Interactions** | Single `<a href="@Data.BacklogUrl">` — the only clickable element on the entire page |
| **Legend markers** | Rendered via inline CSS shapes: rotated `<span>` for diamonds, `border-radius: 50%` for circle, tall narrow `<span>` for NOW bar |

### Section 2: Timeline → `Timeline.razor`

| Aspect | Detail |
|--------|--------|
| **Visual section** | `.tl-area` — 196px-tall row with track labels (left) and SVG (right) |
| **Component** | `Components/Dashboard/Timeline.razor` |
| **CSS layout** | `display: flex; align-items: stretch; height: 196px; background: #FAFAFA;` |
| **CSS classes** | `.tl-area`, `.tl-svg-box` (from reference) |
| **Sub-elements** | Track label panel (230px `<div>`) + SVG panel (flex: 1) |
| **Data bindings** | `@foreach Data.Tracks` for labels; `@foreach Data.Months` for grid lines; `@foreach track.Milestones` for markers |
| **SVG generation** | `<svg width="1560" height="185">` with computed `<line>`, `<circle>`, `<polygon>`, `<text>` elements |
| **Key C# methods** | `DateToX(DateTime)` → pixel X; `TrackY(int index)` → pixel Y; `DiamondPoints(cx, cy)` → SVG polygon string |
| **Interactions** | None — pure static SVG. All labels permanently rendered. |

### Section 3: Heatmap → `Heatmap.razor`

| Aspect | Detail |
|--------|--------|
| **Visual section** | `.hm-wrap` + `.hm-grid` — fills remaining vertical space below timeline |
| **Component** | `Components/Dashboard/Heatmap.razor` |
| **CSS layout** | Outer: `flex: 1; display: flex; flex-direction: column;`. Grid: `display: grid; grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr);` |
| **CSS classes** | `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`, `.it` + row-specific: `.ship-hdr`, `.ship-cell`, `.prog-hdr`, `.prog-cell`, `.carry-hdr`, `.carry-cell`, `.block-hdr`, `.block-cell` |
| **Dynamic styling** | Column count set via inline style: `grid-template-columns: 160px repeat(@Data.Columns.Count, 1fr)`. Highlight column detected by comparing each column name to `@Data.HighlightColumn`. |
| **Data bindings** | `@foreach Data.Columns` for column headers; `@foreach Data.Rows` for status rows; `@row.Items[column]` for cell content |
| **Highlight logic** | If column == `Data.HighlightColumn`: header gets `highlight-hdr` class, cells get `highlight` class (darker background) |
| **Empty state** | If `items` is null/empty: render `<span style="color:#AAA">–</span>` |
| **Interactions** | None — pure static grid |

### Complete Component Tree

```
App.razor
└── Routes.razor
    └── MainLayout.razor (minimal: just @Body, no nav/sidebar)
        └── Dashboard.razor (@page "/")
            ├── [error state] → <div class="error-container">
            └── [success state]
                ├── Header.razor
                │   ├── <h1> with title + backlog <a>
                │   ├── <div class="sub"> with subtitle
                │   └── Legend bar (4× <span> markers)
                ├── Timeline.razor
                │   ├── Track label panel (230px div)
                │   │   └── N× track label (code + description)
                │   └── <svg> (1560×185)
                │       ├── <defs> with drop shadow filter
                │       ├── N× month grid <line> + <text>
                │       ├── NOW <line> + <text>
                │       └── N× track <line> + milestones
                └── Heatmap.razor
                    ├── <div class="hm-title">
                    └── <div class="hm-grid">
                        ├── Corner cell ("STATUS")
                        ├── N× column header (month name)
                        └── 4× status row
                            ├── Row header (category name)
                            └── N× data cell (work items or dash)
```

### File Inventory (< 15 files budget)

| # | File | Purpose | Est. LOC |
|---|------|---------|----------|
| 1 | `ReportingDashboard.csproj` | Project file | 10 |
| 2 | `Program.cs` | Host configuration | 20 |
| 3 | `Data/DashboardData.cs` | C# record models | 80 |
| 4 | `Data/DashboardDataService.cs` | JSON read + validation | 60 |
| 5 | `Data/data.json` | Sample data | (not counted) |
| 6 | `Components/App.razor` | HTML shell | 15 |
| 7 | `Components/Routes.razor` | Router | 5 |
| 8 | `Components/Layout/MainLayout.razor` | Layout wrapper | 5 |
| 9 | `Components/Pages/Dashboard.razor` | Main page | 30 |
| 10 | `Components/Dashboard/Header.razor` | Header + legend | 60 |
| 11 | `Components/Dashboard/Timeline.razor` | SVG timeline | 150 |
| 12 | `Components/Dashboard/Heatmap.razor` | CSS Grid heatmap | 100 |
| 13 | `wwwroot/css/dashboard.css` | All styles | 150 |
| 14 | `Properties/launchSettings.json` | Port config | 15 |
| | **Total** | **14 source files** | **~700 LOC** |