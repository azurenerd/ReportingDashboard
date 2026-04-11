# Architecture

## Overview & Goals

This document defines the system architecture for the **Executive Reporting Dashboard**, a single-page Blazor Server (.NET 8) web application that visualizes project milestones, timelines, and monthly execution status in a fixed 1920×1080 viewport optimized for PowerPoint screenshot capture.

### Primary Goals

1. **Pixel-perfect visual fidelity** — Reproduce the `OriginalDesignConcept.html` reference design exactly, using native Razor components, CSS Grid/Flexbox, and inline SVG.
2. **Data-driven rendering** — All dashboard content is sourced from a single `data.json` file; no code changes are required to update project data.
3. **Zero-infrastructure operation** — The application runs locally via `dotnet run` with no database, no authentication, no cloud services, and no external dependencies.
4. **Screenshot-ready output** — The page renders at exactly 1920×1080 with `overflow: hidden`, producing clean 16:9 captures for executive slide decks.
5. **Maintainable component architecture** — Each visual section (Header, Timeline, Heatmap) is an isolated Razor component for readability and independent modification.

### Architectural Principles

- **No JavaScript interop** — All rendering is pure Razor + CSS + inline SVG.
- **No third-party UI libraries** — No MudBlazor, Radzen, Chart.js, or any NuGet packages beyond the default SDK.
- **Reference CSS preservation** — CSS class names from `OriginalDesignConcept.html` are preserved verbatim in `app.css` for easy cross-referencing.
- **Read-only data flow** — Data flows one direction: JSON file → service → page → child components → rendered HTML/SVG.

---

## System Components

### 1. `Program.cs` — Application Entry Point

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure Kestrel, register services, set up the Blazor Server middleware pipeline. |
| **Interfaces** | None (entry point). |
| **Dependencies** | `DashboardDataService` (registered as singleton). |
| **Data** | Reads `appsettings.json` for port configuration only. |

**Key behaviors:**
- Registers `DashboardDataService` as a singleton via `builder.Services.AddSingleton<DashboardDataService>()`.
- Calls `app.MapBlazorHub()` and `app.MapFallbackToPage("/_Host")` for Blazor Server wiring.
- Configures Kestrel to listen on `http://localhost:5000` and `https://localhost:5001`.
- Removes all default Blazor error UI middleware that would interfere with the fixed viewport.

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

---

### 2. `DashboardDataService` — Data Access Layer

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Load, deserialize, validate, and cache `data.json`. Provide the in-memory `DashboardData` model to all components. |
| **Interfaces** | `Task<DashboardData?> GetDashboardDataAsync()`, `string? GetError()` |
| **Dependencies** | `System.Text.Json`, `System.IO`, `IWebHostEnvironment` (for resolving `wwwroot` path). |
| **Data** | Reads `wwwroot/data/data.json`; caches the deserialized `DashboardData` object in memory. |

**Key behaviors:**
- On first call to `GetDashboardDataAsync()`, reads and deserializes the JSON file.
- Uses `JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }`.
- Validates required fields (`title`, `timeline`, `heatmap`). If validation fails, stores a human-readable error string retrievable via `GetError()`.
- Implements a single retry with 1-second delay if the file is locked by another process (`IOException`).
- Caches the parsed model as a private field; subsequent calls return the cached instance.
- No `FileSystemWatcher` in MVP — data reloads on application restart.

```csharp
public class DashboardDataService
{
    private readonly string _dataPath;
    private DashboardData? _cachedData;
    private string? _error;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.WebRootPath, "data", "data.json");
    }

    public async Task<DashboardData?> GetDashboardDataAsync()
    {
        if (_cachedData != null) return _cachedData;
        try
        {
            var json = await ReadFileWithRetryAsync(_dataPath);
            _cachedData = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);
            _error = Validate(_cachedData);
            if (_error != null) _cachedData = null;
        }
        catch (Exception ex)
        {
            _error = $"Unable to load dashboard data: {ex.Message}";
        }
        return _cachedData;
    }

    public string? GetError() => _error;
}
```

---

### 3. `MainLayout.razor` — Page Shell

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Provide a full-viewport layout with no navigation sidebar, no Blazor default chrome. |
| **Interfaces** | Renders `@Body` as the sole content. |
| **Dependencies** | None. |
| **Data** | None. |

**Key behaviors:**
- Strips all default Blazor template elements (nav sidebar, top bar, "About" link, error UI).
- Renders only `@Body` inside a bare `<div>` or directly in the `<body>`.
- The associated `MainLayout.razor.css` is empty or minimal — all layout styles are in global `app.css`.

```razor
@inherits LayoutComponentBase
@Body
```

---

### 4. `Dashboard.razor` — Page Orchestrator

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | The single routable page (`/`). Loads data from `DashboardDataService` and delegates rendering to child components. Displays an error message if data loading fails. |
| **Interfaces** | Route: `/` |
| **Dependencies** | `DashboardDataService` (injected). Child components: `Header`, `Timeline`, `Heatmap`. |
| **Data** | Receives `DashboardData` model from service; passes slices as `[Parameter]` values to children. |

**Key behaviors:**
- Calls `DashboardDataService.GetDashboardDataAsync()` in `OnInitializedAsync()`.
- If data is `null`, renders a centered error message from `GetError()`.
- If data loads successfully, renders the three-section vertical layout: `<Header>`, `<Timeline>`, `<Heatmap>`.
- Computes `NowDate` as `DateTime.Now` unless overridden by `data.json` field `nowDateOverride`.

```razor
@page "/"
@inject DashboardDataService DataService

@if (_error != null)
{
    <div class="error-container">@_error</div>
}
else if (_data != null)
{
    <Header Data="_data" />
    <Timeline Config="_data.Timeline" NowDate="_nowDate" />
    <Heatmap Data="_data.Heatmap" Months="_data.Months"
             CurrentMonthIndex="_data.CurrentMonthIndex" />
}
```

---

### 5. `Header.razor` — Title Bar & Legend

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the project title, ADO Backlog link, subtitle (org/workstream/month), and the four-item legend. |
| **Interfaces** | `[Parameter] DashboardData Data` |
| **Dependencies** | None. |
| **Data** | Reads `Data.Title`, `Data.BacklogUrl`, `Data.Organization`, `Data.Workstream`, `Data.CurrentMonth`. |

**Visual mapping (from `.hdr` in reference design):**
- Left side: `<h1>` with title + `<a>` link; `<div class="sub">` with subtitle.
- Right side: Flexbox row of four `<span>` legend items with CSS-shaped icons (rotated squares, circles, vertical bar).

---

### 6. `Timeline.razor` — SVG Milestone Visualization

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the full SVG timeline with month gridlines, track lines, milestone markers, labels, and the NOW indicator. |
| **Interfaces** | `[Parameter] TimelineConfig Config`, `[Parameter] DateTime NowDate` |
| **Dependencies** | Child component: `TimelineMilestone.razor`. |
| **Data** | Reads `Config.StartDate`, `Config.EndDate`, `Config.Tracks[]` (each with milestones). |

**Visual mapping (from `.tl-area` in reference design):**
- Left sidebar (230px): Track labels rendered as `<div>` elements with track ID and description.
- Right SVG area (`.tl-svg-box`): An `<svg>` element (width=1560, height=185) containing:
  - Vertical month gridlines via `@foreach` over computed month positions.
  - Horizontal track lines (`<line>`) at Y positions 42, 98, 154.
  - Milestone markers delegated to `TimelineMilestone.razor`.
  - Dashed red NOW line at computed X position.

**Core calculation logic:**

```csharp
private const double SvgWidth = 1560.0;
private const double SvgHeight = 185.0;
private static readonly double[] TrackYPositions = { 42, 98, 154 };

private double DateToX(DateTime date)
{
    var totalDays = (Config.EndDate - Config.StartDate).TotalDays;
    var elapsed = (date - Config.StartDate).TotalDays;
    return Math.Clamp((elapsed / totalDays) * SvgWidth, 0, SvgWidth);
}

private IEnumerable<(string Label, double X)> GetMonthGridlines()
{
    var current = new DateTime(Config.StartDate.Year, Config.StartDate.Month, 1);
    while (current <= Config.EndDate)
    {
        yield return (current.ToString("MMM"), DateToX(current));
        current = current.AddMonths(1);
    }
}
```

---

### 7. `TimelineMilestone.razor` — Individual Milestone Marker

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render a single milestone marker (checkpoint circle, PoC diamond, or production diamond) with its text label. |
| **Interfaces** | `[Parameter] double X`, `[Parameter] double Y`, `[Parameter] MilestoneType Type`, `[Parameter] string Color`, `[Parameter] string Label`, `[Parameter] bool LabelAbove` |
| **Dependencies** | None. |
| **Data** | Receives all data via parameters. |

**SVG output by type:**
- `checkpoint` → `<circle>` with white fill, colored stroke (2.5px), radius 7.
- `checkpoint-minor` → `<circle>` radius 4, fill `#999`.
- `poc` → `<polygon>` diamond (11px), fill `#F4B400`, with `filter="url(#sh)"`.
- `production` → `<polygon>` diamond (11px), fill `#34A853`, with `filter="url(#sh)"`.
- Text label: `<text>` at 10px, fill `#666`, `text-anchor: middle`, offset above or below track line.

---

### 8. `Heatmap.razor` — CSS Grid Container

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the section title, CSS Grid structure, column headers, and delegate rows to `HeatmapRow`. |
| **Interfaces** | `[Parameter] HeatmapData Data`, `[Parameter] List<string> Months`, `[Parameter] int CurrentMonthIndex` |
| **Dependencies** | Child components: `HeatmapRow.razor`. |
| **Data** | Reads month list, current month index, and four category data sets. |

**Visual mapping (from `.hm-wrap` / `.hm-grid` in reference design):**
- Title bar: `<div class="hm-title">` with uppercase text.
- Grid: `<div class="hm-grid">` with dynamic `grid-template-columns: 160px repeat({N}, 1fr)`.
- Corner cell: `<div class="hm-corner">STATUS</div>`.
- Column headers: `<div class="hm-col-hdr">` for each month; current month gets additional `.apr-hdr` class and "★ Now" text.
- Four `<HeatmapRow>` components, one per category.

**Dynamic grid columns:**

```csharp
private string GridColumnsStyle =>
    $"grid-template-columns: 160px repeat({Months.Count}, 1fr);";
private string GridRowsStyle =>
    $"grid-template-rows: 36px repeat(4, 1fr);";
```

---

### 9. `HeatmapRow.razor` — Single Category Row

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the row header and delegate each month cell to `HeatmapCell`. |
| **Interfaces** | `[Parameter] CategoryType Category`, `[Parameter] Dictionary<string, List<string>> Items`, `[Parameter] List<string> Months`, `[Parameter] int CurrentMonthIndex` |
| **Dependencies** | Child component: `HeatmapCell.razor`. Uses `CategoryStyleMap` for CSS class lookup. |
| **Data** | Items dictionary keyed by lowercase month name. |

**CSS class mapping:**

```csharp
private static readonly Dictionary<CategoryType, (string HdrClass, string CellClass)> StyleMap = new()
{
    { CategoryType.Shipped,    ("ship-hdr",  "ship-cell")  },
    { CategoryType.InProgress, ("prog-hdr",  "prog-cell")  },
    { CategoryType.Carryover,  ("carry-hdr", "carry-cell") },
    { CategoryType.Blockers,   ("block-hdr", "block-cell") },
};
```

---

### 10. `HeatmapCell.razor` — Single Month×Category Cell

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render individual work items as bulleted text, or a gray dash for empty cells. |
| **Interfaces** | `[Parameter] List<string>? Items`, `[Parameter] string CellClass`, `[Parameter] bool IsCurrentMonth` |
| **Dependencies** | None. |
| **Data** | List of item strings. |

**Rendering logic:**
- If `Items` is null or empty → render `<span style="color:#AAA">-</span>`.
- Otherwise → render each item as `<div class="it">{item}</div>`.
- CSS class: `{CellClass}` plus `apr` class if `IsCurrentMonth` is true.

---

## Component Interactions

### Data Flow Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                        wwwroot/data/data.json                    │
└──────────────────────────┬───────────────────────────────────────┘
                           │ File.ReadAllTextAsync()
                           ▼
┌──────────────────────────────────────────────────────────────────┐
│                    DashboardDataService (Singleton)               │
│  ┌─────────────┐  ┌──────────────────┐  ┌───────────────────┐   │
│  │ Read & Retry │→ │ Deserialize JSON  │→ │ Validate Required │   │
│  └─────────────┘  └──────────────────┘  └───────┬───────────┘   │
│                                                  │ Cache         │
│                                         ┌────────▼────────┐     │
│                                         │  DashboardData   │     │
│                                         └────────┬────────┘     │
└──────────────────────────────────────────────────┼───────────────┘
                                                   │ Inject
                                                   ▼
┌──────────────────────────────────────────────────────────────────┐
│                     Dashboard.razor (Page: "/")                   │
│                                                                   │
│  ┌────────────┐    ┌──────────────┐    ┌────────────────────┐    │
│  │ Header.razor│    │Timeline.razor│    │  Heatmap.razor     │    │
│  │             │    │              │    │                    │    │
│  │ Data.Title  │    │ Config.Tracks│    │ Data.Heatmap       │    │
│  │ Data.Org    │    │ NowDate      │    │ Months             │    │
│  │ Data.Url    │    │              │    │ CurrentMonthIndex   │    │
│  └────────────┘    │  ┌─────────┐ │    │  ┌──────────────┐  │    │
│                     │  │Timeline │ │    │  │ HeatmapRow   │  │    │
│                     │  │Milestone│ │    │  │  ┌──────────┐│  │    │
│                     │  └─────────┘ │    │  │  │Heatmap   ││  │    │
│                     └──────────────┘    │  │  │Cell      ││  │    │
│                                         │  │  └──────────┘│  │    │
│                                         │  └──────────────┘  │    │
│                                         └────────────────────┘    │
└──────────────────────────────────────────────────────────────────┘
                           │
                           ▼
                   Rendered HTML/SVG/CSS
                   (1920×1080 viewport)
```

### Component Communication Pattern

All communication is **top-down via `[Parameter]` properties**. There are no events bubbling up, no shared state services between components, and no two-way binding. This is strictly a read-only rendering pipeline:

1. `Dashboard.razor` calls `DashboardDataService.GetDashboardDataAsync()` once during `OnInitializedAsync()`.
2. The resulting `DashboardData` model is decomposed and passed as parameters to `Header`, `Timeline`, and `Heatmap`.
3. `Timeline` computes SVG coordinates and passes per-milestone parameters to `TimelineMilestone`.
4. `Heatmap` iterates over categories and passes per-row data to `HeatmapRow`.
5. `HeatmapRow` iterates over months and passes per-cell data to `HeatmapCell`.

### Service Lifetime

| Service | Lifetime | Justification |
|---------|----------|---------------|
| `DashboardDataService` | Singleton | Data is loaded once at startup and cached. No per-request state. |

---

## Data Model

### C# Model Classes

```
DashboardData (root)
├── Title: string
├── Organization: string
├── Workstream: string
├── CurrentMonth: string
├── BacklogUrl: string
├── NowDateOverride: DateTime? (optional)
├── Months: List<string>
├── CurrentMonthIndex: int
├── Timeline: TimelineConfig
│   ├── StartDate: DateTime
│   ├── EndDate: DateTime
│   └── Tracks: List<Track>
│       ├── Id: string
│       ├── Label: string
│       ├── Color: string (hex)
│       └── Milestones: List<Milestone>
│           ├── Date: DateTime
│           ├── Type: string ("checkpoint" | "checkpoint-minor" | "poc" | "production")
│           └── Label: string
└── Heatmap: HeatmapData
    ├── Shipped: Dictionary<string, List<string>>
    ├── InProgress: Dictionary<string, List<string>>
    ├── Carryover: Dictionary<string, List<string>>
    └── Blockers: Dictionary<string, List<string>>
```

### Entity Definitions

#### `DashboardData.cs`

```csharp
public class DashboardData
{
    public string Title { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Workstream { get; set; } = string.Empty;
    public string CurrentMonth { get; set; } = string.Empty;
    public string BacklogUrl { get; set; } = string.Empty;
    public DateTime? NowDateOverride { get; set; }
    public List<string> Months { get; set; } = new();
    public int CurrentMonthIndex { get; set; }
    public TimelineConfig Timeline { get; set; } = new();
    public HeatmapData Heatmap { get; set; } = new();
}
```

#### `TimelineConfig.cs`

```csharp
public class TimelineConfig
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Track> Tracks { get; set; } = new();
}

public class Track
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";
    public List<Milestone> Milestones { get; set; } = new();
}

public class Milestone
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = "checkpoint";  // "checkpoint" | "checkpoint-minor" | "poc" | "production"
    public string Label { get; set; } = string.Empty;
}
```

#### `HeatmapData.cs`

```csharp
public class HeatmapData
{
    public Dictionary<string, List<string>> Shipped { get; set; } = new();
    public Dictionary<string, List<string>> InProgress { get; set; } = new();
    public Dictionary<string, List<string>> Carryover { get; set; } = new();
    public Dictionary<string, List<string>> Blockers { get; set; } = new();
}
```

#### `CategoryType.cs`

```csharp
public enum CategoryType
{
    Shipped,
    InProgress,
    Carryover,
    Blockers
}
```

### Canonical `data.json` Schema

```json
{
  "title": "Privacy Automation Release Roadmap",
  "organization": "Trusted Platform",
  "workstream": "Privacy Automation Workstream",
  "currentMonth": "April 2026",
  "backlogUrl": "https://dev.azure.com/contoso/project/_backlogs",
  "nowDateOverride": null,
  "months": ["January", "February", "March", "April"],
  "currentMonthIndex": 3,
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
        ]
      },
      {
        "id": "M2",
        "label": "PDS & Data Inventory",
        "color": "#00897B",
        "milestones": [
          { "date": "2025-12-19", "type": "checkpoint", "label": "Dec 19" },
          { "date": "2026-02-11", "type": "checkpoint", "label": "Feb 11" },
          { "date": "2026-03-05", "type": "checkpoint-minor", "label": "" },
          { "date": "2026-03-10", "type": "checkpoint-minor", "label": "" },
          { "date": "2026-03-15", "type": "checkpoint-minor", "label": "" },
          { "date": "2026-03-17", "type": "checkpoint-minor", "label": "" },
          { "date": "2026-03-20", "type": "poc", "label": "Mar 20 PoC" }
        ]
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
    "shipped": {
      "january": ["Item A", "Item B"],
      "february": ["Item C"],
      "march": ["Item D", "Item E"],
      "april": ["Item F"]
    },
    "inProgress": {
      "january": [],
      "february": ["Item G"],
      "march": ["Item H"],
      "april": ["Item I", "Item J"]
    },
    "carryover": {
      "january": [],
      "february": [],
      "march": ["Item K"],
      "april": ["Item L"]
    },
    "blockers": {
      "january": [],
      "february": [],
      "march": [],
      "april": ["Item M"]
    }
  }
}
```

### Storage

**No database.** All data is stored in `wwwroot/data/data.json` as a flat file. The file is read once at application startup, deserialized into the `DashboardData` object graph, and cached in memory for the lifetime of the process.

### Validation Rules

| Field | Rule | Error Message |
|-------|------|---------------|
| `title` | Required, non-empty | "Missing required field: title" |
| `timeline` | Required, non-null | "Missing required field: timeline" |
| `timeline.startDate` | Must be before `endDate` | "Timeline startDate must be before endDate" |
| `timeline.tracks` | At least 1, at most 5 | "Timeline must have between 1 and 5 tracks" |
| `heatmap` | Required, non-null | "Missing required field: heatmap" |
| `months` | Required, at least 1 entry | "At least one month must be specified" |
| `currentMonthIndex` | Must be valid index into `months` | "currentMonthIndex is out of range" |

---

## API Contracts

This application has **no REST API, no GraphQL, no WebSocket endpoints, and no external service calls**. It is a single-page Blazor Server application with a SignalR circuit for UI interactivity (managed by the Blazor framework automatically).

### Internal Service Contract

The only programmatic contract is the `DashboardDataService` interface:

```csharp
public class DashboardDataService
{
    /// <summary>
    /// Loads and returns the cached dashboard data.
    /// Returns null if data failed to load or validate.
    /// </summary>
    public Task<DashboardData?> GetDashboardDataAsync();

    /// <summary>
    /// Returns a human-readable error message if data loading failed.
    /// Returns null if data loaded successfully.
    /// </summary>
    public string? GetError();
}
```

### Error Handling Strategy

| Scenario | Behavior |
|----------|----------|
| `data.json` missing | Display: "Unable to load dashboard data. File not found: wwwroot/data/data.json" |
| `data.json` locked | Retry once after 1 second. If still locked, display: "Unable to load dashboard data. File is locked by another process." |
| Invalid JSON syntax | Display: "Unable to load dashboard data. Invalid JSON: {parser error message}" |
| Valid JSON, missing required fields | Display: "Unable to load dashboard data. {specific validation error}" |
| Deserialization type mismatch | Display: "Unable to load dashboard data. Schema error: {field} expected {type}" |

All errors render as a centered `<div>` on a white background with 16px text in `#EA4335` (red), replacing the entire dashboard. The application does not crash or show a stack trace.

### HTTP Routes

| Route | Method | Handler |
|-------|--------|---------|
| `/` | GET (Blazor) | `Dashboard.razor` — renders the full dashboard page |
| `/_blazor` | WebSocket | Blazor Server SignalR hub (framework-managed) |
| `/css/app.css` | GET (Static) | Global stylesheet |
| `/data/data.json` | GET (Static) | Raw JSON file (served by `UseStaticFiles()`) |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to specific Blazor components.

### Section 1: Header → `Header.razor`

| Visual Element | HTML Reference | Blazor Implementation |
|---------------|----------------|----------------------|
| Project title | `.hdr h1` | `<h1>@Data.Title <a href="@Data.BacklogUrl">🔗 ADO Backlog</a></h1>` |
| Subtitle line | `.hdr .sub` | `<div class="sub">@Data.Organization · @Data.Workstream · @Data.CurrentMonth</div>` |
| Legend area | `.hdr > div:last-child` | Inline `<span>` elements with CSS-shaped icons |
| PoC diamond icon | `span` with `rotate(45deg)` | `<span style="width:12px;height:12px;background:#F4B400;transform:rotate(45deg);display:inline-block;"></span>` |
| Prod diamond icon | `span` with `rotate(45deg)` | Same pattern, `background:#34A853` |
| Checkpoint circle | `span` with `border-radius:50%` | `<span style="width:8px;height:8px;border-radius:50%;background:#999;display:inline-block;"></span>` |
| Now bar | `span` 2×14px | `<span style="width:2px;height:14px;background:#EA4335;display:inline-block;"></span>` |

**CSS Layout:** Flexbox (`display:flex; align-items:center; justify-content:space-between`). Applied via `.hdr` class in `app.css`.

**Data Bindings:** `Data.Title`, `Data.BacklogUrl`, `Data.Organization`, `Data.Workstream`, `Data.CurrentMonth`.

**Interactions:** The ADO Backlog link is a standard `<a href>` — no Blazor click handling.

---

### Section 2: Timeline → `Timeline.razor` + `TimelineMilestone.razor`

| Visual Element | HTML Reference | Blazor Implementation |
|---------------|----------------|----------------------|
| Track labels sidebar | `.tl-area > div:first-child` | `@foreach (track in Config.Tracks)` rendering `<div>` with track ID and label |
| SVG container | `.tl-svg-box > svg` | `<svg xmlns="..." width="1560" height="185" style="overflow:visible;display:block">` |
| Drop shadow filter | `<defs><filter id="sh">` | Static `<defs>` block inside SVG |
| Month gridlines | `<line>` + `<text>` per month | `@foreach` over `GetMonthGridlines()` |
| Track lines | `<line>` per track | `<line x1="0" y1="@trackY" x2="1560" y2="@trackY" stroke="@track.Color" stroke-width="3"/>` |
| Milestone markers | `<circle>`, `<polygon>` | `<TimelineMilestone>` component per milestone |
| NOW line | Dashed `<line>` + `<text>` | `<line x1="@nowX" y1="0" x2="@nowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>` |

**CSS Layout:** Flexbox (`.tl-area`). SVG is absolutely positioned within the flex container.

**Data Bindings:** `Config.StartDate`, `Config.EndDate`, `Config.Tracks` (each with milestones). `NowDate` for the NOW line position.

**Interactions:** None in MVP. Future: `<title>` elements inside SVG shapes for native browser tooltips.

**Coordinate Calculation:**
- X axis: `DateToX(date)` maps dates to 0–1560px range.
- Y axis: Fixed positions per track index (42, 98, 154 for 3 tracks; formula: `28 + (trackIndex * 56)`).
- Month gridlines: Computed from first-of-month dates within the configured range.

---

### Section 3: Heatmap → `Heatmap.razor` + `HeatmapRow.razor` + `HeatmapCell.razor`

| Visual Element | HTML Reference | Blazor Implementation |
|---------------|----------------|----------------------|
| Section title | `.hm-title` | `<div class="hm-title">MONTHLY EXECUTION HEATMAP...</div>` |
| Grid container | `.hm-grid` | `<div class="hm-grid" style="@GridColumnsStyle @GridRowsStyle">` |
| Corner cell | `.hm-corner` | `<div class="hm-corner">STATUS</div>` |
| Month headers | `.hm-col-hdr` | `@foreach` over months; current month gets `.apr-hdr` class |
| Shipped row | `.ship-hdr` + `.ship-cell` | `<HeatmapRow Category="CategoryType.Shipped" .../>` |
| In Progress row | `.prog-hdr` + `.prog-cell` | `<HeatmapRow Category="CategoryType.InProgress" .../>` |
| Carryover row | `.carry-hdr` + `.carry-cell` | `<HeatmapRow Category="CategoryType.Carryover" .../>` |
| Blockers row | `.block-hdr` + `.block-cell` | `<HeatmapRow Category="CategoryType.Blockers" .../>` |
| Individual items | `.hm-cell .it` | `@foreach (item in Items) { <div class="it">@item</div> }` |
| Empty cells | `-` in `#AAA` | `<span style="color:#AAA">-</span>` |
| Current month highlight | `.apr` modifier class | Added via `@(IsCurrentMonth ? "apr" : "")` |

**CSS Layout:** CSS Grid. Columns: `160px repeat(N, 1fr)`. Rows: `36px repeat(4, 1fr)`.

**Data Bindings:** `Data.Heatmap.Shipped`, `.InProgress`, `.Carryover`, `.Blockers` — each a `Dictionary<string, List<string>>` keyed by lowercase month name.

**Interactions:** None. Pure read-only rendering.

**Dynamic Grid Sizing:** The number of columns adapts to `Months.Count`. CSS is set via inline `style` attribute:

```csharp
$"grid-template-columns: 160px repeat({Months.Count}, 1fr);"
```

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|---------------|
| **.NET SDK** | 8.0.x LTS (any 8.0 patch version) |
| **OS** | Windows 10/11 (Segoe UI font dependency) |
| **Browser** | Microsoft Edge (Chromium) — primary; Chrome/Firefox compatible |
| **Resolution** | 1920×1080 display (or browser zoom to match) |
| **Memory** | < 100 MB RAM |
| **Disk** | < 50 MB (SDK not included) |

### Hosting

**Kestrel (built-in)**. No IIS, no reverse proxy, no Docker, no cloud hosting.

```
Developer Machine
├── .NET 8 SDK installed
├── ReportingDashboard/
│   ├── dotnet run → Kestrel starts
│   └── Listens on:
│       ├── http://localhost:5000
│       └── https://localhost:5001
└── Browser → http://localhost:5000
```

### Networking

- **Localhost only.** No external network access required or expected.
- **No outbound API calls.** The application makes zero HTTP requests to external services.
- **No inbound traffic** beyond the local browser ↔ Kestrel connection.

### Storage

- **File system only.** One file: `wwwroot/data/data.json`.
- **No database.** No SQLite, no LiteDB, no in-memory DB.
- **No temporary files.** No logging to disk beyond console output.

### CI/CD

**None for MVP.** The project is built and run locally. If CI is added later:

```yaml
# Minimal GitHub Actions (future)
- dotnet restore
- dotnet build --no-restore --warnaserror
- dotnet test --no-build  # if tests exist
```

### Port Configuration

Configured in `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "ReportingDashboard": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## Technology Stack Decisions

| Layer | Choice | Justification |
|-------|--------|---------------|
| **Framework** | Blazor Server (.NET 8 LTS) | Mandated by project requirements. Provides Razor component model, built-in DI, and static file serving. |
| **Rendering** | Razor Components (`.razor`) | Direct HTML/SVG output with C# data binding. No JS interop needed. |
| **CSS Layout** | CSS Grid + Flexbox (native) | Matches `OriginalDesignConcept.html` exactly. No CSS framework needed. |
| **SVG** | Inline SVG in Razor | The reference design uses hand-authored SVG. Razor `@foreach` loops replace hardcoded milestone elements. No charting library. |
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8. Performant, zero-allocation for small files. No Newtonsoft.Json needed. |
| **Font** | Segoe UI (system) | Windows system font. No web font loading, no FOUT, no external requests. |
| **Web Server** | Kestrel (built-in) | Default ASP.NET Core server. No IIS, no Nginx, no Docker. |
| **CSS Strategy** | Global `app.css` | Reference design CSS class names preserved verbatim. Component-scoped `.razor.css` files used only for component-specific overrides. |
| **Package Management** | Zero additional NuGet packages | Everything needed is in `Microsoft.NET.Sdk.Web`. |

### Rejected Alternatives

| Technology | Reason for Rejection |
|-----------|---------------------|
| MudBlazor / Radzen | Custom design; component libraries would require extensive override work. |
| Chart.js / ApexCharts | Requires JS interop; custom SVG shapes (diamonds) are simpler in raw SVG. |
| Blazor WebAssembly | Slower startup (downloads runtime to browser); no benefit for local-only use. |
| Blazor Static SSR | Viable but less familiar to the team; Server mode is mandated. |
| Tailwind CSS | Requires PostCSS build pipeline; reference design already has complete CSS. |
| Bootstrap | Would conflict with custom design; requires overriding most defaults. |
| Newtonsoft.Json | Unnecessary; `System.Text.Json` is built-in and sufficient. |
| SQLite / LiteDB | No persistence needed; JSON file read-only, updated monthly. |

---

## Security Considerations

### Authentication & Authorization

**None required.** The application runs on `localhost` only, accessed by a single user on their local machine. No login screen, no roles, no middleware, no cookies, no tokens.

### Data Protection

- **No PII** is stored in `data.json`. Contents are project status data (milestone names, dates, work item descriptions).
- **No secrets** are stored anywhere in the application. No API keys, no connection strings, no credentials.
- **No encryption** is needed. Data is non-sensitive project reporting information.
- **File system permissions** are the only access control — standard OS-level protections apply.

### Input Validation

- `data.json` is the only external input. It is deserialized with `System.Text.Json` which is safe against injection attacks.
- All string values from JSON are rendered via Razor's built-in HTML encoding (`@variable`), which prevents XSS even if someone places HTML/script tags in `data.json`.
- URLs in `backlogUrl` are rendered inside `<a href="">` — Razor does not sanitize `href` values, but since this is a local-only tool with manually-edited JSON, the risk is negligible.

### Network Security

- The application listens on `localhost` only. It is not exposed to the network.
- HTTPS is available via the .NET dev certificate (`dotnet dev-certs https --trust`) but is optional for local use.
- No outbound network requests are made.

### Dependency Security

- **Zero third-party packages.** The attack surface is limited to the .NET 8 SDK itself, which receives regular security patches via Microsoft Update.

---

## Scaling Strategy

### Current Scale (MVP)

This application is designed for **single-user, single-machine, local execution**. Scaling is explicitly out of scope.

| Dimension | Current Capacity | Limit |
|-----------|-----------------|-------|
| Users | 1 | 1 (localhost) |
| Projects | 1 per `data.json` | 1 |
| Timeline tracks | 1–5 | 5 (viewport constraint) |
| Heatmap months | 1–6 | 6 (viewport constraint) |
| Items per cell | 1–6 | ~6 (overflow hidden at 1080px) |
| JSON file size | < 10 KB typical | 50 KB max (validated) |

### Data Volume Scaling

If heatmap data grows beyond what fits in the fixed viewport:

1. **Reduce font size** — from 12px to 11px or 10px (CSS-only change).
2. **Increase month columns** — change `currentMonthIndex` and `months` array to show a rolling window.
3. **Truncate items** — show top N items per cell with a "+X more" indicator.

### Future Scaling Paths (Not in MVP)

| Scenario | Approach |
|----------|----------|
| Multiple projects | Route parameter `/dashboard/{project}`, load `{project}.json` |
| Team access | Deploy to an internal web server; add basic auth middleware |
| Automated updates | Replace `data.json` with an ADO API call in `DashboardDataService` |
| Larger datasets | Paginate heatmap months; add horizontal scrolling (breaks screenshot constraint) |

---

## Risks & Mitigations

### Risk 1: SVG Coordinate Calculation Bugs

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium |
| **Impact** | High — milestones render at wrong positions, undermining dashboard credibility |
| **Mitigation** | Isolate `DateToX()` as a pure static method. Unit test with known date→pixel mappings. Visually compare against reference HTML. |
| **Detection** | Side-by-side screenshot comparison with `OriginalDesignConcept.html`. |

### Risk 2: JSON Schema Drift

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium |
| **Impact** | Medium — silent null values cause missing data in the rendered dashboard |
| **Mitigation** | Validate required fields in `DashboardDataService` after deserialization. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`. Display explicit error messages for missing fields. |
| **Detection** | Validation errors surface as on-page error messages, not silent failures. |

### Risk 3: Viewport Overflow with Large Data Sets

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Medium — content overflows the 1080px height, clipped by `overflow: hidden` |
| **Mitigation** | Document the supported data volume limits (max 5 tracks, max 6 items per cell, max 6 months). Include these constraints in `data.json` schema documentation. |
| **Detection** | Visual inspection. If items are clipped, reduce data or font size. |

### Risk 4: Blazor Server Overhead

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Low — SignalR circuit consumes ~2 MB RAM per connection for what is a static page |
| **Mitigation** | Acceptable for single-user local use. If overhead becomes a concern, consider switching to Blazor Static SSR (`RenderMode.Static`) which eliminates the SignalR circuit while staying within .NET 8. |
| **Detection** | Monitor memory usage via Task Manager. |

### Risk 5: Browser Rendering Differences

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Low — minor pixel differences in SVG rendering across browsers |
| **Mitigation** | Standardize on **Microsoft Edge (Chromium)** for all screenshots. Document this requirement. CSS Grid and Flexbox are consistent across modern Chromium-based browsers. |
| **Detection** | Cross-browser visual comparison if screenshots look different than expected. |

### Risk 6: File Locking on `data.json`

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Low — application fails to start if VS Code or another editor locks the file exclusively |
| **Mitigation** | Implement a single retry with 1-second delay in `DashboardDataService`. Most editors use shared read locks, so this is unlikely in practice. |
| **Detection** | Error message displayed on the page: "File is locked by another process." |

---

## Solution Structure

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj        # Microsoft.NET.Sdk.Web, net8.0, zero extra packages
    ├── Program.cs                        # App entry point, service registration
    │
    ├── Models/
    │   ├── DashboardData.cs              # Root model with all project metadata
    │   ├── TimelineConfig.cs             # Timeline date range and tracks
    │   ├── Track.cs                      # Individual track with milestones
    │   ├── Milestone.cs                  # Single milestone (date, type, label)
    │   ├── HeatmapData.cs               # Four category dictionaries
    │   └── CategoryType.cs              # Enum: Shipped, InProgress, Carryover, Blockers
    │
    ├── Services/
    │   └── DashboardDataService.cs       # Load, deserialize, validate, cache data.json
    │
    ├── Components/
    │   ├── _Imports.razor                # Global using statements
    │   ├── App.razor                     # Root component with <HeadOutlet> and <Routes>
    │   ├── Routes.razor                  # <Router> component
    │   ├── Layout/
    │   │   └── MainLayout.razor          # Full-viewport layout, no nav/sidebar
    │   ├── Pages/
    │   │   └── Dashboard.razor           # Single page ("/"), orchestrates children
    │   ├── Header.razor                  # Title, subtitle, ADO link, legend
    │   ├── Timeline.razor                # SVG timeline with gridlines + NOW line
    │   ├── TimelineMilestone.razor       # Individual SVG milestone marker
    │   ├── Heatmap.razor                 # CSS Grid container + column headers
    │   ├── HeatmapRow.razor              # Single category row (header + cells)
    │   └── HeatmapCell.razor             # Single month×category cell
    │
    ├── wwwroot/
    │   ├── css/
    │   │   └── app.css                   # Global styles ported from OriginalDesignConcept.html
    │   └── data/
    │       └── data.json                 # Sample project data (ships with the app)
    │
    └── Properties/
        └── launchSettings.json           # Port configuration (5000/5001)
```

### Build & Run Commands

```powershell
# First-time setup
dotnet new sln -n ReportingDashboard
dotnet new blazorserver -n ReportingDashboard -f net8.0
dotnet sln add ReportingDashboard\ReportingDashboard.csproj

# Development (with hot reload)
cd ReportingDashboard
dotnet watch run

# Production build
dotnet build -c Release --warnaserror

# Self-contained publish
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### CSS Organization

Global `app.css` preserves all class names from `OriginalDesignConcept.html`:

| CSS Class | Component | Purpose |
|-----------|-----------|---------|
| `.hdr`, `.sub` | Header.razor | Title bar layout |
| `.tl-area`, `.tl-svg-box` | Timeline.razor | Timeline container |
| `.hm-wrap`, `.hm-title`, `.hm-grid` | Heatmap.razor | Heatmap container |
| `.hm-corner`, `.hm-col-hdr`, `.apr-hdr` | Heatmap.razor | Grid headers |
| `.hm-row-hdr` | HeatmapRow.razor | Row headers |
| `.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr` | HeatmapRow.razor | Category row headers |
| `.hm-cell`, `.it` | HeatmapCell.razor | Data cells and items |
| `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` | HeatmapCell.razor | Category cell backgrounds |
| `.apr` (modifier) | HeatmapCell.razor | Current month highlight |