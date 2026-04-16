# Architecture

## Overview & Goals

This document defines the system architecture for the **Executive Reporting Dashboard**, a single-page Blazor Server (.NET 8) web application that visualizes project milestones on an SVG timeline and displays work item status in a color-coded heatmap grid. The dashboard reads all data from a local `data.json` file, requires no authentication or cloud infrastructure, and is optimized for pixel-perfect 1920×1080 screenshots to be embedded in PowerPoint decks.

### Architectural Goals

1. **Pixel-perfect visual fidelity** — Match the `OriginalDesignConcept.html` design exactly at 1920×1080 in Microsoft Edge
2. **Data-driven rendering** — All displayed content is driven from a single `data.json` file; zero code changes required for data updates
3. **Zero-dependency simplicity** — No external NuGet packages, no database, no cloud services; the entire app runs with `dotnet run`
4. **Sub-10-file solution** — The complete application (excluding build artifacts) consists of fewer than 10 source files
5. **Screenshot-first design** — Optimized for static capture, not interactive use; no client-side JavaScript, tooltips, or animations

### Architectural Pattern

**Minimal Single-Page Read-Only Application** — This is the simplest viable architecture for the problem:

```
data.json (flat file on disk)
       │ read once on startup
       ▼
DashboardDataService (singleton, DI-registered, in-memory cache)
       │ injected into
       ▼
Dashboard.razor (single Razor page component)
       │ renders
       ▼
HTML + CSS + inline SVG (pixel-perfect 1920×1080 dashboard)
```

There is no backend API, no database, no state management, no client-side interactivity. The entire application is a data-loading service feeding a single Razor component that renders static HTML.

---

## System Components

### Component 1: `Program.cs` — Application Host

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure and start the Blazor Server application; register services in DI |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService` (registers as singleton) |
| **Data** | None directly; configures `IWebHostEnvironment` which provides file paths |

**Behavior:**
- Calls `builder.Services.AddSingleton<DashboardDataService>()` to register the data service
- Calls `builder.Services.AddRazorComponents().AddInteractiveServerComponents()` for Blazor Server
- Maps Razor components with `app.MapRazorComponents<App>().AddInteractiveServerRenderMode()`
- Configures static file serving for `wwwroot/` (CSS, `data.json`)
- No authentication middleware, no CORS, no API controllers

**Implementation Specification:**
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

---

### Component 2: `DashboardDataService` — Data Loading & Caching

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Read `data.json` from disk, deserialize to `DashboardData`, cache in memory, surface errors gracefully |
| **Interfaces** | `Task<DashboardData> GetDataAsync()` — returns cached data or error wrapper |
| **Dependencies** | `IWebHostEnvironment` (injected, provides `WebRootPath` for file location) |
| **Data** | Reads `wwwroot/data.json`; caches deserialized `DashboardData` as a private field |

**Behavior:**
- On first call to `GetDataAsync()`, reads `{WebRootPath}/data.json` using `File.ReadAllTextAsync()`
- Deserializes using `System.Text.Json.JsonSerializer.Deserialize<DashboardData>()` with `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`
- Caches the result in a private `DashboardData?` field; subsequent calls return the cached instance
- Uses `SemaphoreSlim(1,1)` to ensure thread-safe initialization (Blazor Server may serve multiple circuits)
- If the file is missing: sets `ErrorMessage` property to `"Unable to load dashboard data. Please check that data.json exists at: {expectedPath}"`
- If the file is malformed JSON: catches `JsonException` and sets `ErrorMessage` to `"data.json contains invalid JSON: {exception.Message}"`
- If the file deserializes but has null required fields (e.g., `Metadata` is null): sets `ErrorMessage` with a descriptive validation failure
- Exposes `string? ErrorMessage { get; }` for the Razor component to check before rendering

**Implementation Specification:**
```csharp
public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private DashboardData? _data;
    private string? _errorMessage;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _initialized;

    public string? ErrorMessage => _errorMessage;

    public DashboardDataService(IWebHostEnvironment env) => _env = env;

    public async Task<DashboardData?> GetDataAsync()
    {
        if (_initialized) return _data;

        await _semaphore.WaitAsync();
        try
        {
            if (_initialized) return _data;

            var path = Path.Combine(_env.WebRootPath, "data.json");
            if (!File.Exists(path))
            {
                _errorMessage = $"Unable to load dashboard data. Please check that data.json exists at: {path}";
                _initialized = true;
                return null;
            }

            var json = await File.ReadAllTextAsync(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _data = JsonSerializer.Deserialize<DashboardData>(json, options);

            if (_data?.Metadata == null)
            {
                _errorMessage = "data.json is missing required 'metadata' section.";
                _data = null;
            }

            _initialized = true;
            return _data;
        }
        catch (JsonException ex)
        {
            _errorMessage = $"data.json contains invalid JSON: {ex.Message}";
            _initialized = true;
            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

---

### Component 3: `DashboardData.cs` — Data Models

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define the strongly-typed shape of `data.json` using C# records |
| **Interfaces** | N/A (POCO records) |
| **Dependencies** | None |
| **Data** | Immutable record types mirroring the JSON schema |

**Record Definitions:**

```csharp
public record DashboardData
{
    public DashboardMetadata Metadata { get; init; } = default!;
    public List<Track> Tracks { get; init; } = [];
    public List<Milestone> Milestones { get; init; } = [];
    public List<WorkItem> Items { get; init; } = [];
}

public record DashboardMetadata
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogUrl { get; init; } = "";
    public string CurrentMonth { get; init; } = "";
    public string? CurrentDate { get; init; }       // Optional: "2026-04-16" override for NOW line
    public int Year { get; init; } = 2026;          // Year context for month-to-date conversion
    public List<string> Months { get; init; } = [];  // ["Jan","Feb","Mar","Apr","May","Jun"]
}

public record Track
{
    public string Id { get; init; } = "";            // "m1", "m2", "m3"
    public string Name { get; init; } = "";          // "Chatbot & MS Role"
    public string Color { get; init; } = "#999";     // "#0078D4"
}

public record Milestone
{
    public string TrackId { get; init; } = "";       // references Track.Id
    public string Label { get; init; } = "";         // "Mar 26 PoC"
    public string Date { get; init; } = "";          // "2026-03-26" (ISO 8601)
    public string Type { get; init; } = "";          // "poc" | "production" | "checkpoint" | "start"
    public string? LabelPosition { get; init; }      // "above" | "below" (default: "above")
}

public record WorkItem
{
    public string Name { get; init; } = "";          // "Chatbot v2 GA"
    public string Status { get; init; } = "";        // "shipped" | "in-progress" | "carryover" | "blocked"
    public string Month { get; init; } = "";         // "Jan" | "Feb" | "Mar" | "Apr"
}
```

**Design Decisions:**
- Records are used for immutability — data is read-only after deserialization
- Default values (`= ""`, `= []`) prevent null reference exceptions if JSON fields are omitted
- `CurrentDate` is optional — when null, the app uses `DateTime.Now`; when set, it overrides for historical snapshot generation
- `LabelPosition` on `Milestone` allows per-milestone control of label placement to avoid overlapping text
- `Year` on metadata provides context for converting month abbreviations ("Jan", "Feb") to full `DateTime` values for timeline positioning

---

### Component 4: `TimelineCalculator` — Date-to-Pixel Mapping

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Convert ISO date strings to SVG x-coordinates within the timeline viewport; calculate track y-positions |
| **Interfaces** | `double DateToX(string isoDate)` — maps a date to an x-pixel coordinate |
| **Dependencies** | `DashboardMetadata` (for months array and year) |
| **Data** | Computed date range boundaries and pixel-per-day ratio |

**This is a helper class instantiated in `Dashboard.razor`'s `@code` block**, not a DI-registered service. It is stateless and constructed with the metadata for each render.

**Implementation Specification:**

```csharp
public class TimelineCalculator
{
    private readonly DateTime _rangeStart;
    private readonly DateTime _rangeEnd;
    private readonly double _svgWidth;

    public const double SvgHeight = 185.0;
    public const double SvgDefaultWidth = 1560.0;

    public TimelineCalculator(DashboardMetadata metadata, double svgWidth = SvgDefaultWidth)
    {
        _svgWidth = svgWidth;

        // Convert first month to start of that month
        _rangeStart = ParseMonthStart(metadata.Months[0], metadata.Year);

        // Convert last month to end of that month
        var lastMonthStart = ParseMonthStart(metadata.Months[^1], metadata.Year);
        _rangeEnd = lastMonthStart.AddMonths(1);
    }

    public double DateToX(string isoDate)
    {
        var date = DateTime.Parse(isoDate);
        var totalDays = (_rangeEnd - _rangeStart).TotalDays;
        var elapsed = (date - _rangeStart).TotalDays;
        return Math.Clamp(elapsed / totalDays * _svgWidth, 0, _svgWidth);
    }

    public double MonthToX(string monthAbbrev, int year)
    {
        var date = ParseMonthStart(monthAbbrev, year);
        return DateToX(date.ToString("yyyy-MM-dd"));
    }

    public double GetNowX(string? currentDateOverride)
    {
        var now = currentDateOverride != null
            ? DateTime.Parse(currentDateOverride)
            : DateTime.Now;
        return DateToX(now.ToString("yyyy-MM-dd"));
    }

    public static double GetTrackY(int trackIndex, int trackCount)
    {
        // Evenly space tracks between y=30 and y=170
        var usableHeight = SvgHeight - 30;
        var spacing = usableHeight / (trackCount + 1);
        return 30 + spacing * (trackIndex + 1);
    }

    private static DateTime ParseMonthStart(string monthAbbrev, int year)
    {
        var monthNum = DateTime.ParseExact(monthAbbrev, "MMM",
            System.Globalization.CultureInfo.InvariantCulture).Month;
        return new DateTime(year, monthNum, 1);
    }
}
```

**Key Formula:** `x = (date - rangeStart) / (rangeEnd - rangeStart) × svgWidth`

This linear interpolation maps any date within the month range to a proportional x-coordinate. Dates before the range clamp to 0; dates after clamp to `svgWidth`.

---

### Component 5: `Dashboard.razor` — The Single Page Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the complete dashboard: header, SVG timeline, and CSS Grid heatmap |
| **Interfaces** | Blazor route `@page "/"` — the only page in the application |
| **Dependencies** | `DashboardDataService` (injected), `TimelineCalculator` (instantiated in code block) |
| **Data** | Receives `DashboardData` from the service; renders all visual elements |

**Rendering Strategy:**
The component is structured as three sequential HTML sections within a single flex-column container. All rendering logic lives in the `@code` block as private helper methods.

**Razor Structure (pseudocode):**
```razor
@page "/"
@inject DashboardDataService DataService
@rendermode InteractiveServer

@if (errorMessage != null)
{
    <div class="error-banner">@errorMessage</div>
}
else if (data != null)
{
    <!-- Section 1: Header -->
    <div class="hdr"> ... title, subtitle, legend ... </div>

    <!-- Section 2: Timeline -->
    <div class="tl-area">
        <div class="tl-sidebar"> ... track labels ... </div>
        <div class="tl-svg-box">
            <svg width="@SvgWidth" height="185">
                @* Month gridlines *@
                @* Track lines *@
                @* Milestone markers (diamonds, circles) *@
                @* NOW line *@
            </svg>
        </div>
    </div>

    <!-- Section 3: Heatmap -->
    <div class="hm-wrap">
        <div class="hm-title">Monthly Execution Heatmap...</div>
        <div class="hm-grid" style="grid-template-columns: 160px repeat(@monthCount, 1fr)">
            @* Corner cell + month column headers *@
            @* 4 status rows × N month cells *@
        </div>
    </div>
}
```

**Code Block Helpers:**

| Method | Purpose |
|--------|---------|
| `OnInitializedAsync()` | Calls `DataService.GetDataAsync()`, initializes `TimelineCalculator` |
| `GetItemsForCell(string status, string month)` | Filters `data.Items` by status and month; returns `List<WorkItem>` |
| `IsCurrentMonth(string month)` | Compares month against `data.Metadata.CurrentMonth` for highlight styling |
| `GetCellClass(string statusPrefix, string month)` | Returns CSS class string (e.g., `"ship-cell apr"` for current month shipped cells) |
| `RenderMilestoneMarker(Milestone m, double x, double y)` | Returns SVG markup fragment based on `m.Type` |
| `GetDiamondPoints(double cx, double cy, double r)` | Returns polygon points string for diamond shape: `"cx,cy-r cx+r,cy cx,cy+r cx-r,cy"` |

---

### Component 6: `App.razor` — Root Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Blazor root component; includes `<head>` content, CSS references, and component outlet |
| **Interfaces** | N/A (framework root) |
| **Dependencies** | `Routes.razor` |
| **Data** | None |

**Key Content:**
```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=1920" />
    <link rel="stylesheet" href="css/dashboard.css" />
    <HeadOutlet @rendermode="InteractiveServer" />
</head>
<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

**Critical Detail:** The `<meta name="viewport">` is set to `width=1920` (not the typical responsive viewport tag) to enforce the fixed 1920px layout width.

---

### Component 7: `Routes.razor` — Router

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Blazor router configuration; maps URL `/` to `Dashboard.razor` |
| **Interfaces** | N/A (framework component) |
| **Dependencies** | `Dashboard.razor` |
| **Data** | None |

Standard Blazor router — no customization needed:
```razor
<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" />
    </Found>
</Router>
```

---

### Component 8: `dashboard.css` — Stylesheet

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | All visual styling; ported directly from `OriginalDesignConcept.html` `<style>` block |
| **Interfaces** | CSS classes consumed by `Dashboard.razor` |
| **Dependencies** | None |
| **Data** | N/A |

**CSS Architecture:**
- Direct port of the original HTML design's `<style>` block with minimal modifications
- No CSS custom properties (variables) — colors are hardcoded semantic values
- No CSS preprocessor (Sass/LESS) — plain CSS only
- No third-party CSS framework

**Modifications from Original Design:**
1. Replace `body { width: 1920px; height: 1080px; }` with `body { width: 1920px; height: 1080px; margin: 0 auto; }` for centering
2. The heatmap grid column count (`repeat(4, 1fr)`) becomes dynamic via inline `style` attribute set from Razor code: `style="grid-template-columns: 160px repeat(@monthCount, 1fr)"`
3. Add `.current-month-hdr` class (replaces hardcoded `.apr-hdr`) for dynamic current-month highlighting
4. Add `.error-banner` class for the error state display

**Full Class Inventory (from design, preserved exactly):**

| Class | Section | Purpose |
|-------|---------|---------|
| `.hdr` | Header | Flex container for title + legend |
| `.sub` | Header | Subtitle text styling |
| `.tl-area` | Timeline | Flex container for sidebar + SVG |
| `.tl-svg-box` | Timeline | SVG container |
| `.hm-wrap` | Heatmap | Flex wrapper for title + grid |
| `.hm-title` | Heatmap | Section title styling |
| `.hm-grid` | Heatmap | CSS Grid container |
| `.hm-corner` | Heatmap | Top-left "STATUS" cell |
| `.hm-col-hdr` | Heatmap | Month column headers |
| `.current-month-hdr` | Heatmap | Gold highlight for current month (replaces `.apr-hdr`) |
| `.hm-row-hdr` | Heatmap | Status row headers |
| `.hm-cell` | Heatmap | Data cells |
| `.it` | Heatmap | Individual work item text with `::before` dot |
| `.ship-hdr`, `.ship-cell` | Heatmap | Shipped row (green) |
| `.prog-hdr`, `.prog-cell` | Heatmap | In Progress row (blue) |
| `.carry-hdr`, `.carry-cell` | Heatmap | Carryover row (amber) |
| `.block-hdr`, `.block-cell` | Heatmap | Blockers row (red) |
| `.current-month` | Heatmap | Deeper tint modifier for current month cells |
| `.empty-cell` | Heatmap | Muted dash for empty cells |
| `.error-banner` | Error | Full-page error message display |

---

### Component 9: `data.json` — Data File

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Single source of truth for all dashboard content |
| **Interfaces** | JSON schema consumed by `DashboardDataService` |
| **Dependencies** | None |
| **Data** | Metadata, tracks, milestones, work items |
| **Location** | `wwwroot/data.json` |

Schema is defined by the C# record types in `DashboardData.cs`. See **Data Model** section for full specification.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Startup                       │
│                                                                   │
│  1. Program.cs starts Kestrel on localhost:5001                  │
│  2. Registers DashboardDataService as Singleton in DI            │
│  3. Maps Razor components                                        │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                    Browser navigates to /
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Dashboard.razor (Page)                        │
│                                                                   │
│  OnInitializedAsync():                                           │
│  ┌──────────────────────────────────────────────────────┐       │
│  │ 1. var data = await DataService.GetDataAsync();       │       │
│  │                        │                               │       │
│  │                        ▼                               │       │
│  │    ┌─────────────────────────────────┐                │       │
│  │    │   DashboardDataService          │                │       │
│  │    │                                  │                │       │
│  │    │  First call:                     │                │       │
│  │    │  ┌────────────────────────┐     │                │       │
│  │    │  │ Read wwwroot/data.json │     │                │       │
│  │    │  │ File.ReadAllTextAsync()│     │                │       │
│  │    │  └──────────┬─────────────┘     │                │       │
│  │    │             ▼                    │                │       │
│  │    │  JsonSerializer.Deserialize<>() │                │       │
│  │    │             │                    │                │       │
│  │    │             ▼                    │                │       │
│  │    │  Cache in _data field            │                │       │
│  │    │  Return DashboardData            │                │       │
│  │    └──────────────┬──────────────────┘                │       │
│  │                   │                                    │       │
│  │                   ▼                                    │       │
│  │ 2. calculator = new TimelineCalculator(data.Metadata) │       │
│  │                                                        │       │
│  │ 3. Render three sections:                              │       │
│  │    ├── Header (title, subtitle, legend)                │       │
│  │    ├── Timeline SVG (tracks, milestones, NOW line)     │       │
│  │    └── Heatmap Grid (status rows × month columns)     │       │
│  └──────────────────────────────────────────────────────┘       │
│                                                                   │
│  Output: Complete 1920×1080 HTML page                            │
└─────────────────────────────────────────────────────────────────┘
```

### Communication Patterns

| From | To | Mechanism | Frequency |
|------|----|-----------|-----------|
| `Program.cs` | `DashboardDataService` | DI registration (`AddSingleton`) | Once at startup |
| `Dashboard.razor` | `DashboardDataService` | Constructor injection + `GetDataAsync()` | Once per circuit (page load) |
| `DashboardDataService` | `data.json` | `File.ReadAllTextAsync()` | Once (cached after first read) |
| `Dashboard.razor` | `TimelineCalculator` | Direct instantiation in `@code` | Once per render |
| `Dashboard.razor` | Browser | Blazor Server SignalR (HTML rendering) | Initial page load |

### Error Flow

```
data.json missing or corrupt
       │
       ▼
DashboardDataService.GetDataAsync()
  → catches FileNotFoundException / JsonException
  → sets ErrorMessage property
  → returns null
       │
       ▼
Dashboard.razor checks: data == null && errorMessage != null
  → renders error banner instead of dashboard
  → "Unable to load dashboard data. Please check that data.json exists..."
```

---

## Data Model

### Entity Relationship

```
DashboardData (root)
├── 1:1 DashboardMetadata
│         ├── Title: string
│         ├── Subtitle: string
│         ├── BacklogUrl: string
│         ├── CurrentMonth: string
│         ├── CurrentDate: string? (optional NOW override)
│         ├── Year: int
│         └── Months: string[] (ordered month abbreviations)
│
├── 1:N Track
│         ├── Id: string (PK, e.g., "m1")
│         ├── Name: string
│         └── Color: string (hex color)
│
├── 1:N Milestone
│         ├── TrackId: string (FK → Track.Id)
│         ├── Label: string
│         ├── Date: string (ISO 8601)
│         ├── Type: string (enum: "poc"|"production"|"checkpoint"|"start")
│         └── LabelPosition: string? ("above"|"below")
│
└── 1:N WorkItem
          ├── Name: string
          ├── Status: string (enum: "shipped"|"in-progress"|"carryover"|"blocked")
          └── Month: string (FK → Months array value)
```

### Storage

| Aspect | Detail |
|--------|--------|
| **Storage type** | Flat JSON file (`wwwroot/data.json`) |
| **Persistence** | File on disk; survives app restarts |
| **Caching** | In-memory singleton; loaded once per app lifetime |
| **Size limit** | Designed for files up to 50KB (sufficient for ~200 work items and ~50 milestones) |
| **Schema evolution** | None; schema is stable for MVP. New fields are additive (optional properties with defaults) |

### Sample `data.json` Structure

```json
{
  "metadata": {
    "title": "Project Atlas Release Roadmap",
    "subtitle": "Platform Engineering · Atlas Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "currentMonth": "Apr",
    "currentDate": null,
    "year": 2026,
    "months": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"]
  },
  "tracks": [
    { "id": "m1", "name": "Chatbot & MS Role", "color": "#0078D4" },
    { "id": "m2", "name": "PDS & Data Inventory", "color": "#00897B" },
    { "id": "m3", "name": "Auto Review DFD", "color": "#546E7A" }
  ],
  "milestones": [
    { "trackId": "m1", "label": "Jan 12", "date": "2026-01-12", "type": "start" },
    { "trackId": "m1", "label": "Mar 26 PoC", "date": "2026-03-26", "type": "poc", "labelPosition": "below" },
    { "trackId": "m1", "label": "Apr Prod (TBD)", "date": "2026-04-30", "type": "production", "labelPosition": "above" },
    { "trackId": "m2", "label": "Dec 19", "date": "2025-12-19", "type": "start" },
    { "trackId": "m2", "label": "Feb 11", "date": "2026-02-11", "type": "checkpoint" },
    { "trackId": "m2", "label": "Mar 18 PoC", "date": "2026-03-18", "type": "poc" }
  ],
  "items": [
    { "name": "Chatbot v2 GA", "status": "shipped", "month": "Jan" },
    { "name": "PDS Onboarding", "status": "in-progress", "month": "Apr" },
    { "name": "DFD Classifier", "status": "carryover", "month": "Apr" },
    { "name": "Vendor API Dep", "status": "blocked", "month": "Apr" }
  ]
}
```

### Data Validation Rules

| Rule | Enforcement | Error Behavior |
|------|------------|----------------|
| `metadata` must be present | Check `_data?.Metadata == null` after deserialization | Error message displayed |
| `metadata.months` must be non-empty | Check `Metadata.Months.Count == 0` | Render empty grid (no crash) |
| `milestone.trackId` must reference existing track | No validation (orphaned milestones are silently skipped in rendering via LINQ join) | Graceful degradation |
| `milestone.date` must be valid ISO date | `DateTime.TryParse()` in `TimelineCalculator`; invalid dates are skipped | Graceful degradation |
| `item.status` must be one of 4 valid values | Unrecognized statuses are silently excluded by LINQ filter | Graceful degradation |
| `item.month` must match a month in `metadata.months` | Items with unmatched months don't appear in any cell | Graceful degradation |

---

## API Contracts

This application has **no API endpoints**. There are no REST controllers, no minimal API routes, no GraphQL endpoints, and no gRPC services. This is explicitly out of scope per the PM specification.

### Internal Service Contract

The only internal contract is the `DashboardDataService` interface:

```csharp
// Consumed by Dashboard.razor via DI injection
public class DashboardDataService
{
    /// <summary>
    /// Returns the deserialized dashboard data, or null if loading failed.
    /// Check ErrorMessage property when null is returned.
    /// </summary>
    public Task<DashboardData?> GetDataAsync();

    /// <summary>
    /// Non-null when data loading failed. Contains a user-friendly error message.
    /// </summary>
    public string? ErrorMessage { get; }
}
```

### External Link Contract

The only external interaction is the ADO Backlog hyperlink rendered in the header:

| Element | Behavior |
|---------|----------|
| ADO Backlog link | `<a href="{data.Metadata.BacklogUrl}" target="_blank">` — opens in new tab |
| No JavaScript | Link is a standard `<a>` tag; no `onclick` handler or JS navigation |

### Error Responses

| Condition | User-Visible Output |
|-----------|-------------------|
| `data.json` missing | Red banner: "Unable to load dashboard data. Please check that data.json exists at: {path}" |
| `data.json` malformed | Red banner: "data.json contains invalid JSON: {parser error details}" |
| `metadata` section missing | Red banner: "data.json is missing required 'metadata' section." |
| Empty arrays (no items/tracks) | Dashboard renders with empty timeline and dashes in all heatmap cells |

---

## Infrastructure Requirements

### Runtime Requirements

| Requirement | Specification |
|-------------|---------------|
| **.NET SDK** | .NET 8.0 SDK (any 8.0.x patch) |
| **OS** | Windows 10 or Windows 11 |
| **Browser** | Microsoft Edge (Chromium), latest stable |
| **Display** | 1920×1080 resolution at 100% DPI scaling |
| **Font** | Segoe UI (pre-installed on Windows) |
| **Disk space** | < 50MB (SDK excluded) |
| **Memory** | < 100MB RSS at runtime |
| **Network** | Localhost only; no outbound connections required |

### Hosting

| Aspect | Detail |
|--------|--------|
| **Web server** | Kestrel (built into .NET 8 SDK) |
| **HTTP port** | `http://localhost:5000` |
| **HTTPS port** | `https://localhost:5001` (self-signed dev cert) |
| **Process model** | Single Kestrel process via `dotnet run` |
| **Deployment** | None. Run from source directory. |

### Storage

| File | Location | Size | Purpose |
|------|----------|------|---------|
| `data.json` | `wwwroot/data.json` | < 50KB | All dashboard content |
| `dashboard.css` | `wwwroot/css/dashboard.css` | ~3KB | All styles |
| Application DLLs | `bin/Debug/net8.0/` | ~5MB | Build output (git-ignored) |

### CI/CD

**None.** CI/CD is explicitly out of scope. The application is built and run locally with:

```powershell
# Build and run
dotnet run --project src/ReportingDashboard.Web

# Development with hot reload
dotnet watch --project src/ReportingDashboard.Web
```

### Network

- No inbound ports exposed beyond localhost
- No outbound API calls
- No DNS resolution required
- SignalR WebSocket connection is localhost-only (Blazor Server framework requirement)

---

## Technology Stack Decisions

| Layer | Choice | Justification |
|-------|--------|---------------|
| **Framework** | Blazor Server (.NET 8) | Mandated technology stack. Provides server-side rendering with C# — no JavaScript required. |
| **CSS Layout** | Native CSS Grid + Flexbox | Matches the original HTML design 1:1. No framework overhead. Exact pixel control for screenshot fidelity. |
| **Timeline Visualization** | Hand-written inline SVG in Razor | Gives pixel-precise control over diamond shapes, track lines, and label positioning. Charting libraries (Radzen, ApexCharts) would fight the exact visual spec. |
| **Heatmap Grid** | CSS Grid with dynamic column count | `grid-template-columns: 160px repeat(N, 1fr)` adapts to any number of months. Native CSS, no JS. |
| **Data Format** | JSON flat file (`data.json`) | Simplest possible data store. Editable by any team member with a text editor. No database overhead for a read-only dashboard. |
| **Serialization** | `System.Text.Json` | Built into .NET 8 SDK. Zero external packages. Performant for small files. |
| **Data Models** | C# `record` types | Immutable by default, concise syntax, value equality. Perfect for deserializing JSON into read-only structures. |
| **Component Library** | None | The page has no forms, no dialogs, no user input. A component library (MudBlazor, Radzen) adds 2-5MB of CSS/JS and fights pixel-exact styling. |
| **CSS Framework** | None | Bootstrap/Tailwind would override the design's carefully specified colors, spacing, and typography. Raw CSS ported from the HTML design is more accurate and smaller. |
| **JavaScript** | None (zero JS) | Blazor Server handles all rendering. The only JS is the framework's `blazor.web.js` for SignalR transport. |
| **Testing** | Manual visual inspection (primary), xUnit + bUnit (optional) | ROI of automated tests is low for a single read-only page. The "test" is whether the screenshot looks right. |
| **Hosting** | Kestrel (built-in) | Built into the .NET 8 SDK. No IIS, no Nginx, no Docker. `dotnet run` is the entire deployment. |

### Decisions NOT Made (Explicitly Excluded)

| Technology | Why Excluded |
|-----------|-------------|
| SQLite / LiteDB / EF Core | No database needed. `data.json` is the data store. |
| REST API / Minimal API | No consumers. The Razor component reads data directly from the service. |
| SignalR (beyond Blazor default) | No real-time features. Blazor's default SignalR is a framework requirement, not a feature. |
| Docker | Local-only app. `dotnet run` is simpler than container setup. |
| Sass / LESS | One CSS file with ~80 rules. A preprocessor adds build complexity for zero benefit. |
| CSS Custom Properties | Colors are semantic and not themeable. Hardcoded hex values match the design and the simplicity goal. |

---

## Security Considerations

### Threat Model

This application has an **extremely small attack surface** by design:

| Threat | Applies? | Detail |
|--------|----------|--------|
| Unauthorized access | **No** | App runs on localhost only. No network exposure. |
| Authentication bypass | **No** | No authentication exists; none is needed. |
| SQL injection | **No** | No database. |
| XSS (Cross-Site Scripting) | **Low risk** | Data comes from a local JSON file edited by trusted users. Blazor's Razor engine HTML-encodes all rendered values by default, preventing injection even if `data.json` contained malicious content. |
| CSRF | **No** | No state-changing operations. Dashboard is read-only. |
| Data exfiltration | **No** | No PII, credentials, or secrets in `data.json`. Contains only project status metadata. |
| Supply chain attack | **Minimal** | Zero external NuGet packages. Only SDK-included libraries. |
| File path traversal | **No** | `data.json` path is hardcoded to `{WebRootPath}/data.json`. No user-supplied file paths. |

### Data Protection

- `data.json` contains non-sensitive project status information (milestone names, work item titles, dates)
- No PII, no credentials, no API keys, no secrets in any project file
- No encryption at rest or in transit beyond HTTPS localhost (dev cert)
- The `.gitignore` should exclude any files that might contain sensitive data (none expected)

### Input Validation

- `data.json` is the only input. It is deserialized with `System.Text.Json` which rejects malformed JSON
- All string values rendered in Razor are auto-encoded by the Blazor rendering engine
- Invalid dates or unrecognized enum values result in graceful degradation (items skipped), not exceptions
- No user-supplied input at runtime (no forms, no query parameters, no URL parameters)

### Future Consideration

If the dashboard is ever deployed to a shared server:
1. Move `data.json` out of `wwwroot/` to prevent HTTP access (e.g., to `App_Data/`)
2. Add Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (one line in `Program.cs`)
3. Add `[Authorize]` attribute to the Dashboard page

These changes are **not needed for MVP** and should not be implemented preemptively.

---

## Scaling Strategy

### Current Scale Target

This application is designed for **exactly one user on one machine**. There is no scaling requirement.

| Dimension | Current Target | Max Supported |
|-----------|---------------|---------------|
| Concurrent users | 1 | ~5 (Blazor Server SignalR circuits) |
| Data size | ~5KB | 50KB (`data.json`) |
| Tracks | 3 | 10 (limited by 196px timeline height) |
| Milestones | ~15 | 50 (limited by SVG label overlap) |
| Work items | ~20 | 100 (limited by heatmap cell height) |
| Month columns | 6 | 12 (limited by 1920px width) |

### Visual Scaling Limits

The fixed 1920×1080 viewport imposes hard visual limits:

- **Tracks:** At 196px timeline height, 3 tracks use ~56px spacing. Up to 6-7 tracks are feasible before lines overlap. Beyond 7, the timeline height would need to increase (breaking the 1080px total).
- **Milestones per track:** Labels at 10px font can overlap when milestones are within ~30px of each other. The `LabelPosition` field ("above"/"below") mitigates this for pairs.
- **Work items per cell:** At 12px font with 1.35 line-height, each item is ~16px tall. A heatmap cell in a 4-row grid within ~600px of remaining height gives ~150px per cell, supporting ~8-9 items per cell before clipping.
- **Month columns:** At `repeat(N, 1fr)` with 160px for the row header, 6 months get ~293px each. 12 months would get ~147px each — still usable but tighter.

### Scaling Path (if ever needed)

| Scenario | Approach |
|----------|----------|
| Multiple viewers | Deploy to IIS with Windows Auth; Blazor Server supports multiple concurrent circuits |
| Larger data sets | Pagination is not appropriate for a single-page screenshot tool; instead, split into multiple `data.json` files for different time windows |
| Multiple projects | Clone the repo; swap `data.json` for each project. Or add a query parameter `?project=atlas` that selects from multiple JSON files |
| Automated data updates | Add a simple CLI script that queries ADO API and writes `data.json`; keep the dashboard itself unchanged |

---

## Risks & Mitigations

### Risk 1: SVG Timeline Date-to-Pixel Math Errors

**Severity: Medium** | **Likelihood: Medium**

The most complex logic in the application is converting ISO dates to SVG x-coordinates. Edge cases include:
- Milestones with dates outside the month range (e.g., "Dec 19" when months start at "Jan")
- Months spanning year boundaries
- The "NOW" line position for dates at month boundaries

**Mitigation:**
- `TimelineCalculator` clamps x-values to `[0, svgWidth]` so out-of-range dates don't break the SVG
- The `Year` field in metadata resolves year ambiguity
- Unit test the calculator with edge-case dates: before range, after range, at boundaries, leap year dates
- Formula is simple linear interpolation: `x = (date - start) / (end - start) × width`

---

### Risk 2: Blazor Server SignalR Overhead

**Severity: Low** | **Likelihood: Certain**

Blazor Server maintains a persistent WebSocket connection per browser tab. This is unnecessary for a read-only, no-interaction dashboard and adds ~150KB of JavaScript and a background connection.

**Mitigation:**
- Accept the overhead. For a localhost app with 1 user, the ~2MB memory per circuit is negligible.
- The Blazor Server template is simpler to scaffold than Blazor Static SSR, and the mandated stack specifies Blazor Server.
- No action required.

---

### Risk 3: data.json Schema Errors Crash the App

**Severity: Medium** | **Likelihood: Medium**

Non-technical team members editing `data.json` may introduce typos, missing commas, or structural errors.

**Mitigation:**
- `DashboardDataService` wraps deserialization in try/catch and surfaces a clear error message
- The error message includes the file path so the user knows which file to fix
- The sample `data.json` is well-documented with comments (where JSON5 is not supported, use a separate `data.schema.md` documenting each field)
- Records use default values (`= ""`, `= []`) so missing optional fields don't cause null reference exceptions
- Unrecognized `status` or `type` values are silently filtered out rather than throwing

---

### Risk 4: Screenshot Fidelity Variation

**Severity: Low** | **Likelihood: Low**

Different browsers, DPI settings, or zoom levels could produce slightly different screenshots.

**Mitigation:**
- Document the required setup: Microsoft Edge, 1920×1080 resolution, 100% DPI scaling, 100% zoom
- The viewport meta tag forces `width=1920` to prevent responsive scaling
- Segoe UI is guaranteed available on Windows (the only target OS)
- Test screenshot capture in Edge before each executive presentation

---

### Risk 5: Milestone Label Overlap in SVG

**Severity: Medium** | **Likelihood: Medium**

When two milestones on the same track have dates close together, their text labels will overlap and become unreadable.

**Mitigation:**
- The `LabelPosition` field allows per-milestone control: set one to `"above"` and the adjacent one to `"below"`
- For clusters of checkpoints (like the original design's 4 gray dots on track M2), omit labels entirely — the dots communicate progress without text
- This is a data-authoring concern, not a code concern. Document the recommendation in the `data.json` schema guide.

---

### Risk 6: Heatmap Cell Overflow with Many Work Items

**Severity: Low** | **Likelihood: Low**

If a single cell has more than ~8 work items, the text will overflow the cell's available height and be clipped (`overflow: hidden`).

**Mitigation:**
- The CSS already sets `overflow: hidden` on `.hm-cell`, so overflow is visually clean (no layout breakage)
- For executive dashboards, cells with 8+ items indicate a granularity problem — the data author should aggregate items or use higher-level names
- Document the practical limit of ~8 items per cell in the schema guide

---

## UI Component Architecture

This section maps each visual section from the `OriginalDesignConcept.html` design to specific Razor markup within `Dashboard.razor`, CSS classes, data bindings, and layout strategy.

### Component Map

| Visual Section | Razor Region | CSS Class(es) | Layout Strategy | Data Binding |
|---------------|-------------|---------------|-----------------|-------------|
| **Header Bar** | `<div class="hdr">` | `.hdr`, `.sub` | Flexbox row, `space-between` | `data.Metadata.Title`, `.Subtitle`, `.BacklogUrl` |
| **Legend (top-right)** | Inline `<span>` elements within `.hdr` | Inline styles (matching design) | Flexbox row, `gap: 22px` | Static content (PoC, Production, Checkpoint, Now labels) |
| **Timeline Sidebar** | `<div>` within `.tl-area` | Inline styles (230px width) | Flexbox column, `space-around` | `@foreach (var track in data.Tracks)` → `track.Id`, `track.Name`, `track.Color` |
| **SVG Timeline** | `<svg>` within `.tl-svg-box` | `.tl-svg-box` | SVG coordinate system, 1560×185 | `TimelineCalculator` computes all x/y positions from `data.Milestones`, `data.Tracks`, `data.Metadata.Months` |
| **Month Gridlines** | `<line>` + `<text>` in SVG | SVG attributes | Vertical lines at `MonthToX()` intervals | `@foreach (var month in data.Metadata.Months)` |
| **Track Lines** | `<line>` in SVG | SVG attributes (stroke=track color) | Horizontal lines at `GetTrackY()` positions | `@foreach (var track in data.Tracks)` |
| **Milestone Markers** | `<polygon>` (diamonds), `<circle>` (start/checkpoint) in SVG | SVG with `filter="url(#sh)"` for shadows | Positioned at `(DateToX(), trackY)` | `@foreach (var m in data.Milestones)` → type selects shape |
| **NOW Line** | `<line>` + `<text>` in SVG | SVG dashed stroke | Vertical at `GetNowX()` | `data.Metadata.CurrentDate` ?? `DateTime.Now` |
| **Heatmap Title** | `<div class="hm-title">` | `.hm-title` | Block, flex-shrink: 0 | Static text with status emoji separators |
| **Heatmap Grid** | `<div class="hm-grid">` | `.hm-grid` | CSS Grid: `160px repeat(N, 1fr)` / `36px repeat(4, 1fr)` | `style="grid-template-columns: 160px repeat(@data.Metadata.Months.Count, 1fr)"` |
| **Corner Cell** | `<div class="hm-corner">` | `.hm-corner` | Grid cell (1,1) | Static text: "STATUS" |
| **Month Headers** | `<div class="hm-col-hdr">` per month | `.hm-col-hdr`, `.current-month-hdr` | Grid cells (1, 2..N+1) | `@foreach month` → `IsCurrentMonth()` adds highlight class |
| **Shipped Row** | Row header `.ship-hdr` + cells `.ship-cell` | `.hm-row-hdr.ship-hdr`, `.hm-cell.ship-cell` | Grid row 2 | `GetItemsForCell("shipped", month)` |
| **In Progress Row** | Row header `.prog-hdr` + cells `.prog-cell` | `.hm-row-hdr.prog-hdr`, `.hm-cell.prog-cell` | Grid row 3 | `GetItemsForCell("in-progress", month)` |
| **Carryover Row** | Row header `.carry-hdr` + cells `.carry-cell` | `.hm-row-hdr.carry-hdr`, `.hm-cell.carry-cell` | Grid row 4 | `GetItemsForCell("carryover", month)` |
| **Blockers Row** | Row header `.block-hdr` + cells `.block-cell` | `.hm-row-hdr.block-hdr`, `.hm-cell.block-cell` | Grid row 5 | `GetItemsForCell("blocked", month)` |
| **Work Item Dots** | `<div class="it">` within cells | `.it` + `::before` pseudo-element | Relative positioning with absolute dot | `@foreach (var item in cellItems)` → `item.Name` |
| **Empty Cell** | `<span>` with dash | `.empty-cell` | Centered muted dash | `cellItems.Count == 0` → render `"-"` |
| **Error State** | `<div class="error-banner">` | `.error-banner` | Full-page centered block | `DataService.ErrorMessage` |

### Heatmap Rendering Logic (Pseudocode)

```razor
@{ var statuses = new[] {
    ("shipped", "SHIPPED", "ship"),
    ("in-progress", "IN PROGRESS", "prog"),
    ("carryover", "CARRYOVER", "carry"),
    ("blocked", "BLOCKERS", "block")
}; }

@foreach (var (status, label, prefix) in statuses)
{
    <div class="hm-row-hdr @(prefix)-hdr">@label</div>
    @foreach (var month in data.Metadata.Months)
    {
        var items = data.Items.Where(i => i.Status == status && i.Month == month).ToList();
        var isCurrent = month == data.Metadata.CurrentMonth;
        <div class="hm-cell @(prefix)-cell @(isCurrent ? "current-month" : "")">
            @if (items.Any())
            {
                @foreach (var item in items)
                {
                    <div class="it">@item.Name</div>
                }
            }
            else
            {
                <span class="empty-cell">-</span>
            }
        </div>
    }
}
```

### SVG Milestone Rendering Logic (Pseudocode)

```razor
@foreach (var milestone in data.Milestones)
{
    var track = data.Tracks.FirstOrDefault(t => t.Id == milestone.TrackId);
    if (track == null) continue;

    var x = calculator.DateToX(milestone.Date);
    var y = TimelineCalculator.GetTrackY(trackIndex, data.Tracks.Count);
    var labelY = (milestone.LabelPosition == "below") ? y + 24 : y - 16;

    @switch (milestone.Type)
    {
        case "start":
            <circle cx="@x" cy="@y" r="7" fill="white" stroke="@track.Color" stroke-width="2.5" />
            break;
        case "checkpoint":
            <circle cx="@x" cy="@y" r="4" fill="#999" />
            break;
        case "poc":
            <polygon points="@GetDiamondPoints(x, y, 11)" fill="#F4B400" filter="url(#sh)" />
            break;
        case "production":
            <polygon points="@GetDiamondPoints(x, y, 11)" fill="#34A853" filter="url(#sh)" />
            break;
    }

    @if (!string.IsNullOrEmpty(milestone.Label))
    {
        <text x="@x" y="@labelY" text-anchor="middle" fill="#666" font-size="10">@milestone.Label</text>
    }
}
```

---

## Solution File Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
└── src/
    └── ReportingDashboard.Web/
        ├── ReportingDashboard.Web.csproj     [1] Project file (net8.0)
        ├── Program.cs                         [2] Host configuration + DI
        ├── Components/
        │   ├── App.razor                      [3] Root HTML shell
        │   ├── Routes.razor                   [4] Router
        │   └── Pages/
        │       └── Dashboard.razor            [5] THE page (all rendering)
        ├── Models/
        │   └── DashboardData.cs               [6] Record types + TimelineCalculator
        ├── Services/
        │   └── DashboardDataService.cs        [7] JSON loader + cache
        ├── wwwroot/
        │   ├── css/
        │   │   └── dashboard.css              [8] All styles
        │   └── data.json                      [9] Dashboard data
        └── Properties/
            └── launchSettings.json
```

**Total source files: 9** (meets the sub-10-file constraint)