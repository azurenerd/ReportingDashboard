# Architecture

**Document Version:** 1.0
**Date:** April 14, 2026
**Status:** Approved for Implementation
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure

---

## Overview & Goals

The Executive Reporting Dashboard is a single-page, read-only Blazor Server application that renders a pixel-perfect 1920×1080 project status view—combining a milestone timeline (inline SVG) and a monthly execution heatmap (CSS Grid)—driven entirely by a local `data.json` file. The output is optimized for screenshots destined for PowerPoint executive decks.

**Architectural philosophy:** This is a **read-only data renderer**, not a CRUD application. The architecture is intentionally flat—no repository pattern, no CQRS, no API layer, no database. Every architectural decision optimizes for visual fidelity, rapid iteration, and zero operational overhead.

**Primary goals the architecture must support:**

| # | Goal | Architectural Implication |
|---|------|--------------------------|
| 1 | Executive-grade project visibility | Fixed 1920×1080 canvas, pixel-perfect CSS, inline SVG timeline |
| 2 | Rapid deck preparation (<60s edit-to-screenshot) | `dotnet watch` hot reload, JSON-driven data, no build step for data changes |
| 3 | Zero operational overhead | Localhost-only Kestrel, no cloud, no auth, no database, no external NuGet packages |
| 4 | Multiple project snapshots | Query parameter `?data=filename.json` switching between wwwroot JSON files |
| 5 | Pixel-perfect design fidelity | Scoped CSS ported directly from `OriginalDesignConcept.html`, identical color tokens and layout values |

**Constraints governing all decisions:**

- Technology stack is **C# .NET 8 Blazor Server**—non-negotiable
- **Zero external NuGet packages** beyond the implicit `Microsoft.AspNetCore.App` framework reference
- **No JavaScript**—all rendering is server-side Blazor markup
- **No database**—`System.Text.Json` reads a flat file from `wwwroot/`
- **Single page**—one `.razor` component at route `/`
- **Local only**—Kestrel on `127.0.0.1`, no cloud hosting

---

## System Components

### Solution Structure

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj       # net8.0 Blazor Server, zero NuGet deps
    ├── Program.cs                       # Minimal hosting, DI registration
    ├── Models/
    │   └── DashboardData.cs            # C# record types mirroring data.json schema
    ├── Services/
    │   └── DashboardDataService.cs     # JSON file loading, deserialization, caching
    ├── Components/
    │   ├── App.razor                   # Root Blazor component (<html>, <head>, <body>)
    │   ├── Routes.razor                # Blazor router configuration
    │   ├── Pages/
    │   │   ├── Dashboard.razor         # The single dashboard page (route "/")
    │   │   └── Dashboard.razor.css     # Scoped CSS: all layout, colors, grid, heatmap
    │   └── _Imports.razor              # Global using directives for Razor components
    └── wwwroot/
        ├── data.json                   # Default project data (sample "Project Phoenix")
        ├── css/
        │   └── app.css                 # Global resets, body sizing, font stack, print/scale rules
        └── favicon.ico
```

### Component 1: `Program.cs` — Application Host

**Responsibility:** Configure Kestrel, register services, build the middleware pipeline.

**Interfaces:**
- Entry point: `dotnet run` / `dotnet watch`
- Output: Kestrel listening on `https://localhost:5001` (and `http://localhost:5000`)

**Key implementation details:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Bind to localhost only — no remote access
builder.WebHost.UseUrls("https://localhost:5001", "http://localhost:5000");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Singleton: data is static, loaded once, cached in memory
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

**Dependencies:** `Microsoft.AspNetCore.App` framework reference (implicit).

---

### Component 2: `Models/DashboardData.cs` — Data Model Records

**Responsibility:** Define the C# type system that mirrors the `data.json` schema. All types are immutable records with nullable properties for graceful degradation.

**Interfaces:** Consumed by `DashboardDataService` (deserialization target) and `Dashboard.razor` (rendering source).

**Record definitions:**

```csharp
namespace ReportingDashboard.Models;

public record DashboardData(
    string Title,
    string? Subtitle,
    string? BacklogUrl,
    DateOnly CurrentDate,
    string[] Months,
    int CurrentMonthIndex,
    DateOnly? TimelineStart,
    DateOnly? TimelineEnd,
    MilestoneTrack[] MilestoneTracks,
    HeatmapData Heatmap
)
{
    // Defaults for resilient deserialization
    public string Title { get; init; } = Title ?? "Untitled Dashboard";
    public string[] Months { get; init; } = Months ?? Array.Empty<string>();
    public MilestoneTrack[] MilestoneTracks { get; init; } = MilestoneTracks ?? Array.Empty<MilestoneTrack>();
    public HeatmapData Heatmap { get; init; } = Heatmap ?? new HeatmapData(new HeatmapRow(new()), new HeatmapRow(new()), new HeatmapRow(new()), new HeatmapRow(new()));

    // Computed: effective timeline range
    public DateOnly EffectiveTimelineStart => TimelineStart ?? new DateOnly(CurrentDate.Year, 1, 1);
    public DateOnly EffectiveTimelineEnd => TimelineEnd ?? new DateOnly(CurrentDate.Year, 6, 30);
}

public record MilestoneTrack(
    string Name,
    string? Description,
    string Color,
    MilestoneEvent[] Events
)
{
    public MilestoneEvent[] Events { get; init; } = Events ?? Array.Empty<MilestoneEvent>();
}

public record MilestoneEvent(
    DateOnly Date,
    string Label,
    string Type   // "checkpoint", "checkpoint-small", "poc", "production"
);

public record HeatmapData(
    HeatmapRow Shipped,
    HeatmapRow InProgress,
    HeatmapRow Carryover,
    HeatmapRow Blockers
);

public record HeatmapRow(
    Dictionary<string, string[]> Items
)
{
    public string[] GetItems(string month) =>
        Items.TryGetValue(month, out var list) ? list : Array.Empty<string>();

    public int TotalItemCount => Items.Values.Sum(v => v.Length);
}
```

**Data invariants:**
- `Months` array length determines heatmap column count (2–6 supported)
- `CurrentMonthIndex` is 0-based index into `Months`
- `MilestoneTracks` length determines timeline track count (1–5 supported)
- `Type` values on `MilestoneEvent` are: `"checkpoint"`, `"checkpoint-small"`, `"poc"`, `"production"`
- `HeatmapRow.Items` dictionary keys must match values in `Months` array

---

### Component 3: `Services/DashboardDataService.cs` — Data Loading Service

**Responsibility:** Load a JSON file from `wwwroot/`, deserialize it into `DashboardData`, validate the filename for path traversal, and surface errors as structured results (not exceptions).

**Interfaces:**

```csharp
namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Loads and deserializes a JSON data file from wwwroot.
    /// Returns a tuple: (data, errorMessage). On success, errorMessage is null.
    /// On failure, data is null and errorMessage describes the problem.
    /// </summary>
    public async Task<(DashboardData? Data, string? Error)> LoadAsync(string filename = "data.json")
    {
        // Path traversal prevention
        if (string.IsNullOrWhiteSpace(filename) ||
            filename.Contains("..") ||
            filename.Contains('/') ||
            filename.Contains('\\') ||
            filename != Path.GetFileName(filename))
        {
            return (null, $"Invalid filename: {filename}");
        }

        var filePath = Path.Combine(_env.WebRootPath, filename);

        if (!File.Exists(filePath))
        {
            return (null, $"Could not load data file: {filename}");
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            var data = JsonSerializer.Deserialize<DashboardData>(json, options);
            return data is null
                ? (null, "data.json deserialized to null. Check file contents.")
                : (data, null);
        }
        catch (JsonException ex)
        {
            return (null, $"Error loading dashboard data: {ex.Message}. Please check {filename}.");
        }
        catch (IOException ex)
        {
            return (null, $"Error reading file: {ex.Message}");
        }
    }
}
```

**DI Registration:** `builder.Services.AddSingleton<DashboardDataService>();`

**Design decisions:**
- **Singleton lifetime:** The service is stateless; it reads fresh from disk on each call. Singleton avoids unnecessary allocations. No in-memory caching—files are small (<100KB) and reads are <50ms.
- **No caching by design:** Since the user edits `data.json` and expects immediate results on refresh, caching would require invalidation logic. The file is tiny; re-reading is simpler and correct.
- **Tuple return, not exceptions:** The Razor page can pattern-match on `(data, error)` to render either the dashboard or the error banner. No try-catch needed in the component.
- **Lenient JSON options:** `AllowTrailingCommas` and `ReadCommentHandling.Skip` reduce friction for hand-edited files.

---

### Component 4: `Components/Pages/Dashboard.razor` — The Single Page

**Responsibility:** The entire UI. Reads query parameters, calls `DashboardDataService`, and renders the full dashboard as HTML/CSS/SVG. This is the only routable page in the application.

**Route:** `@page "/"`

**Interfaces:**
- **Input:** `[SupplyParameterFromQuery] string? Data` — optional filename override
- **Injected:** `DashboardDataService` via `@inject`
- **Output:** Complete HTML rendering of header, timeline SVG, and heatmap grid

**Lifecycle:**

```csharp
@code {
    [SupplyParameterFromQuery(Name = "data")]
    public string? DataFile { get; set; }

    private DashboardData? dashboardData;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        var filename = DataFile ?? "data.json";
        (dashboardData, errorMessage) = await DataService.LoadAsync(filename);
    }
}
```

**Rendering sections** (top-to-bottom in the markup):

1. **Error Banner** — Rendered conditionally when `errorMessage` is not null
2. **Header Bar** — Title, subtitle, backlog link, legend (all data-bound)
3. **Timeline Area** — Sidebar labels + inline `<svg>` with computed positions
4. **Heatmap Grid** — CSS Grid with dynamic columns and data-bound cells

**SVG Helper Methods in `@code` block:**

```csharp
private const double SvgWidth = 1560.0;
private const double SvgHeight = 185.0;

private double DateToX(DateOnly date)
{
    var start = dashboardData!.EffectiveTimelineStart;
    var end = dashboardData!.EffectiveTimelineEnd;
    var totalDays = end.DayNumber - start.DayNumber;
    if (totalDays <= 0) return 0;
    var elapsed = date.DayNumber - start.DayNumber;
    return Math.Clamp((elapsed / (double)totalDays) * SvgWidth, 0, SvgWidth);
}

private string DiamondPoints(double cx, double cy, double r = 11)
{
    return $"{cx},{cy - r} {cx + r},{cy} {cx},{cy + r} {cx - r},{cy}";
}

private IEnumerable<(string Label, double X)> GetMonthGridPositions()
{
    var start = dashboardData!.EffectiveTimelineStart;
    var end = dashboardData!.EffectiveTimelineEnd;
    var current = new DateOnly(start.Year, start.Month, 1);
    while (current <= end)
    {
        yield return (current.ToString("MMM"), DateToX(current));
        current = current.AddMonths(1);
    }
}

private IEnumerable<(MilestoneTrack Track, double Y)> GetTrackPositions()
{
    var tracks = dashboardData!.MilestoneTracks;
    var spacing = SvgHeight / (tracks.Length + 1);
    for (int i = 0; i < tracks.Length; i++)
    {
        yield return (tracks[i], spacing * (i + 1));
    }
}
```

**Lines of code budget:**
- `Dashboard.razor` markup: ~120 lines
- `Dashboard.razor` `@code` block: ~80 lines
- Total: ~200 lines

---

### Component 5: `Dashboard.razor.css` — Scoped Stylesheet

**Responsibility:** All visual styling for the dashboard page. Ported directly from `OriginalDesignConcept.html` CSS with identical hex values, pixel dimensions, and layout rules.

**CSS architecture:**

```
┌─ CSS Custom Properties (:root)  — Color tokens for theming
├─ .error-banner                   — Error state styling
├─ .hdr                            — Header bar layout
├─ .tl-area                        — Timeline container
├─ .tl-svg-box                     — SVG viewport
├─ .hm-wrap                        — Heatmap wrapper
├─ .hm-grid                        — CSS Grid definition
├─ .hm-corner, .hm-col-hdr        — Grid header cells
├─ .hm-row-hdr                    — Row label cells
├─ .hm-cell, .hm-cell .it         — Data cells and item bullets
├─ .ship-*, .prog-*, .carry-*, .block-*  — Row color themes
├─ .current-month                  — Current month highlight
└─ .hm-cell:hover                  — Subtle hover effect
```

**Color token system (CSS custom properties):**

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-highlight: #D8F2DA;
    --color-shipped-hdr-bg: #E8F5E9;
    --color-shipped-hdr-text: #1B7A28;

    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-highlight: #DAE8FB;
    --color-progress-hdr-bg: #E3F2FD;
    --color-progress-hdr-text: #1565C0;

    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-highlight: #FFF0B0;
    --color-carryover-hdr-bg: #FFF8E1;
    --color-carryover-hdr-text: #B45309;

    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-highlight: #FFE4E4;
    --color-blockers-hdr-bg: #FEF2F2;
    --color-blockers-hdr-text: #991B1B;

    --color-current-month-hdr-bg: #FFF0D0;
    --color-current-month-hdr-text: #C07700;
}
```

**Lines of code budget:** ~150 lines of scoped CSS.

---

### Component 6: `wwwroot/css/app.css` — Global Stylesheet

**Responsibility:** Page-level resets, body sizing, font stack, viewport scaling, and print media rules. These styles cannot be scoped because they target `<html>` and `<body>`.

```css
*, *::before, *::after {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
    background: #FFFFFF;
    font-family: 'Segoe UI', Arial, sans-serif;
    color: #111;
    display: flex;
    flex-direction: column;
}

a {
    color: #0078D4;
    text-decoration: none;
}

/* Auto-scale for smaller viewports */
@media (max-width: 1919px) {
    body {
        transform: scale(calc(100vw / 1920));
        transform-origin: top left;
    }
}

/* Print-friendly: hide Blazor reconnection UI */
@media print {
    #blazor-error-ui,
    .blazor-reconnect-modal {
        display: none !important;
    }
}
```

---

### Component 7: `wwwroot/data.json` — Configuration Data File

**Responsibility:** The single source of truth for all dashboard content. Edited by the user in any text editor. No code changes required to update the dashboard.

**Schema (by example):**

```json
{
    "title": "Project Phoenix Release Roadmap",
    "subtitle": "Cloud Platform · Migration Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "currentDate": "2026-04-14",
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonthIndex": 3,
    "timelineStart": "2026-01-01",
    "timelineEnd": "2026-06-30",
    "milestoneTracks": [
        {
            "name": "M1",
            "description": "Auth & Identity",
            "color": "#0078D4",
            "events": [
                { "date": "2026-01-15", "label": "Jan 15", "type": "checkpoint" },
                { "date": "2026-03-20", "label": "Mar 20 PoC", "type": "poc" },
                { "date": "2026-05-01", "label": "May Prod", "type": "production" }
            ]
        },
        {
            "name": "M2",
            "description": "Data Pipeline",
            "color": "#00897B",
            "events": [
                { "date": "2026-02-10", "label": "Feb 10", "type": "checkpoint" },
                { "date": "2026-04-15", "label": "Apr 15 PoC", "type": "poc" }
            ]
        },
        {
            "name": "M3",
            "description": "Reporting Engine",
            "color": "#546E7A",
            "events": [
                { "date": "2026-03-01", "label": "Mar 1", "type": "checkpoint" },
                { "date": "2026-05-15", "label": "May 15 Prod", "type": "production" }
            ]
        }
    ],
    "heatmap": {
        "shipped": {
            "items": {
                "Jan": ["Auth service v1", "SSO integration"],
                "Feb": ["Data connector", "Schema migration"],
                "Mar": ["Pipeline MVP", "Monitoring alerts"],
                "Apr": ["Report templates"]
            }
        },
        "inProgress": {
            "items": {
                "Jan": ["OAuth2 provider"],
                "Feb": ["Batch processor"],
                "Mar": ["Dashboard wireframes"],
                "Apr": ["Executive views", "Filter engine"]
            }
        },
        "carryover": {
            "items": {
                "Jan": [],
                "Feb": ["Legacy connector"],
                "Mar": ["OAuth2 provider"],
                "Apr": ["Batch retry logic"]
            }
        },
        "blockers": {
            "items": {
                "Jan": [],
                "Feb": [],
                "Mar": ["Vendor API access"],
                "Apr": ["Compliance review"]
            }
        }
    }
}
```

**Schema constraints:**

| Field | Type | Required | Default | Constraints |
|-------|------|----------|---------|-------------|
| `title` | string | Yes | `"Untitled Dashboard"` | Max ~80 chars for header fit |
| `subtitle` | string | No | `null` (hidden) | Max ~100 chars |
| `backlogUrl` | string | No | `null` (link hidden) | Must be valid URL |
| `currentDate` | ISO 8601 date | Yes | — | `YYYY-MM-DD` format |
| `months` | string[] | Yes | `[]` | 2–6 entries, short names |
| `currentMonthIndex` | int | Yes | — | 0-based, must be < `months.length` |
| `timelineStart` | ISO 8601 date | No | Jan 1 of `currentDate` year | Defines SVG left edge |
| `timelineEnd` | ISO 8601 date | No | Jun 30 of `currentDate` year | Defines SVG right edge |
| `milestoneTracks` | array | Yes | `[]` | 1–5 tracks |
| `milestoneTracks[].events[].type` | string | Yes | — | One of: `checkpoint`, `checkpoint-small`, `poc`, `production` |
| `heatmap` | object | Yes | Empty rows | Must have `shipped`, `inProgress`, `carryover`, `blockers` |
| `heatmap.*.items` | dict | Yes | `{}` | Keys must match `months` values |

---

### Component 8: `Components/App.razor` — Root Component

**Responsibility:** HTML document shell (`<html>`, `<head>`, `<body>`) that hosts the Blazor component tree. References `app.css` and the Blazor framework scripts.

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=1920" />
    <title>Executive Reporting Dashboard</title>
    <link rel="stylesheet" href="css/app.css" />
    <link rel="stylesheet" href="ReportingDashboard.styles.css" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

**Note:** `<meta name="viewport" content="width=1920">` forces mobile browsers (if ever used) to render at 1920px width rather than the device width.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────┐    ┌──────────────────────┐    ┌───────────────────┐    ┌──────────────┐
│  data.json  │───▶│ DashboardDataService │───▶│  Dashboard.razor  │───▶│  Browser DOM │
│  (wwwroot/) │    │  LoadAsync(filename)  │    │  OnInitializedAsync│    │  1920×1080   │
└─────────────┘    └──────────────────────┘    └───────────────────┘    └──────────────┘
       ▲                    ▲                           ▲
       │                    │                           │
   User edits         Singleton DI               Query parameter
   text editor        injection                  ?data=file.json
```

### Request Lifecycle (single page load)

```
1. Browser → GET / (or /?data=q3.json)
2. Kestrel → Blazor Server middleware
3. Blazor → Routes.razor → Dashboard.razor
4. Dashboard.razor.OnInitializedAsync():
   a. Read [SupplyParameterFromQuery] → filename (default: "data.json")
   b. Call DashboardDataService.LoadAsync(filename)
   c. DashboardDataService:
      i.   Validate filename (no path traversal)
      ii.  Resolve path: Path.Combine(webRootPath, filename)
      iii. Read file: File.ReadAllTextAsync(path)
      iv.  Deserialize: JsonSerializer.Deserialize<DashboardData>(json, options)
      v.   Return (data, null) or (null, errorMessage)
   d. Store result in component state
5. Dashboard.razor renders:
   - If error: error banner div
   - If success: header + timeline SVG + heatmap grid
6. Blazor serializes the rendered HTML → SignalR → Browser
7. Browser paints the DOM at 1920×1080
```

### Component Dependency Graph

```
Program.cs
  ├── registers → DashboardDataService (Singleton)
  └── maps → App.razor
                └── Routes.razor
                      └── Dashboard.razor
                            ├── injects → DashboardDataService
                            ├── reads → DashboardData (Models)
                            └── styled by → Dashboard.razor.css
```

### Communication Patterns

| From | To | Mechanism | Data |
|------|----|-----------|------|
| Browser | Blazor Server | SignalR WebSocket (implicit) | Initial connection, no user input |
| Dashboard.razor | DashboardDataService | C# method call (sync, in-process) | `LoadAsync(filename)` |
| DashboardDataService | File system | `System.IO.File.ReadAllTextAsync` | JSON string |
| DashboardDataService | Dashboard.razor | Return tuple `(DashboardData?, string?)` | Deserialized model or error |
| Dashboard.razor | Browser | Blazor render tree → HTML/SVG diff | Full page markup |

---

## Data Model

### Entity Relationship Diagram

```
DashboardData (root)
├── title: string
├── subtitle: string?
├── backlogUrl: string?
├── currentDate: DateOnly
├── months: string[]
├── currentMonthIndex: int
├── timelineStart: DateOnly?
├── timelineEnd: DateOnly?
│
├── milestoneTracks: MilestoneTrack[] ──── 1:N
│   ├── name: string
│   ├── description: string?
│   ├── color: string (hex)
│   └── events: MilestoneEvent[] ──── 1:N
│       ├── date: DateOnly
│       ├── label: string
│       └── type: string (enum-like)
│
└── heatmap: HeatmapData ──── 1:1
    ├── shipped: HeatmapRow ──── 1:1
    ├── inProgress: HeatmapRow ──── 1:1
    ├── carryover: HeatmapRow ──── 1:1
    └── blockers: HeatmapRow ──── 1:1
        └── items: Dictionary<string, string[]>
            key = month name (e.g., "Jan")
            value = array of work item names
```

### Storage

**Format:** JSON flat file in `wwwroot/` directory.

**Location:** `wwwroot/data.json` (default), or `wwwroot/{filename}.json` via query parameter.

**Size:** Typical file is 2–5KB. Maximum supported: 100KB (performance constraint).

**No database.** No Entity Framework. No migrations. No connection strings. The JSON file _is_ the data store.

### Serialization Configuration

```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,   // Accept camelCase JSON → PascalCase C#
    ReadCommentHandling = JsonCommentHandling.Skip,  // Allow // comments in JSON
    AllowTrailingCommas = true            // Forgive trailing commas
};
```

`DateOnly` serialization is natively supported in .NET 8's `System.Text.Json` with ISO 8601 format (`"2026-04-14"`).

### Data Validation Strategy

Validation is **defensive, not strict**. The goal is to render _something_ rather than crash:

| Condition | Behavior |
|-----------|----------|
| `data.json` missing | Error banner: "Could not load data file: data.json" |
| Malformed JSON | Error banner with `JsonException.Message` (no stack trace) |
| `title` missing/null | Defaults to `"Untitled Dashboard"` |
| `months` empty | Heatmap renders with no columns (just row headers) |
| `milestoneTracks` empty | Timeline area renders with no tracks (just grid lines) |
| `heatmap.shipped.items["Apr"]` missing | Cell renders dash placeholder (`"-"`) |
| `currentMonthIndex` out of range | No month highlighted; no crash |
| Event `type` unrecognized | Event rendered as small gray dot (fallback) |

---

## API Contracts

### URL Contract

This application has **no REST API**. It serves a single HTML page. The "API" is the URL contract:

| URL | Behavior |
|-----|----------|
| `https://localhost:5001/` | Loads `wwwroot/data.json`, renders dashboard |
| `https://localhost:5001/?data=q3-review.json` | Loads `wwwroot/q3-review.json`, renders dashboard |
| `https://localhost:5001/?data=nonexistent.json` | Renders error banner |
| `https://localhost:5001/?data=../../etc/passwd` | Rejected: error banner "Invalid filename" |

### Query Parameter Contract

| Parameter | Type | Required | Default | Validation |
|-----------|------|----------|---------|------------|
| `data` | string | No | `"data.json"` | Must be a bare filename (no slashes, no `..`, no path separators). Must match `Path.GetFileName(value)`. |

### Error Response Contract

Errors are rendered inline in the page HTML, not as HTTP status codes. The Blazor page always returns HTTP 200 with the full HTML shell. Error states render a banner `<div>`:

```html
<!-- Error state rendering -->
<div class="error-banner">
    <strong>⚠</strong> Error loading dashboard data: Unexpected character encountered
    while parsing value: }. Path '', line 3, position 1. Please check data.json.
</div>
```

**Error banner CSS:**

```css
.error-banner {
    background: #FEF2F2;
    color: #991B1B;
    padding: 16px 44px;
    font-size: 14px;
    border-bottom: 2px solid #EA4335;
    text-align: center;
}
```

### Static File Contract

| Path | File | Content-Type |
|------|------|-------------|
| `/css/app.css` | Global stylesheet | `text/css` |
| `/data.json` | Dashboard data (also accessible directly, but not intended for API use) | `application/json` |
| `/_framework/blazor.web.js` | Blazor runtime (auto-generated) | `application/javascript` |
| `/ReportingDashboard.styles.css` | Bundled scoped CSS (auto-generated) | `text/css` |

---

## Infrastructure Requirements

### Runtime Dependencies

| Dependency | Version | Installation |
|------------|---------|-------------|
| .NET 8 SDK | 8.0.x (latest patch) | `winget install Microsoft.DotNet.SDK.8` or [dot.net](https://dot.net) |
| Windows 10/11 | Any supported | Pre-installed (for Segoe UI font) |
| Edge or Chrome | Latest stable | Pre-installed |

### Hosting

- **Web server:** Kestrel (embedded in .NET 8, no separate install)
- **Binding:** `https://localhost:5001` and `http://localhost:5000`
- **Process model:** Single Kestrel process, in-process hosting
- **No IIS.** No Nginx. No Apache. No reverse proxy.

### Networking

- **Localhost only.** Kestrel binds to `127.0.0.1`. No LAN, no internet.
- **Ports:** 5000 (HTTP) and 5001 (HTTPS). Configurable via `launchSettings.json` or `--urls` CLI flag.
- **TLS:** Kestrel development certificate (`dotnet dev-certs https --trust`). Self-signed, localhost only.

### Storage

- **Disk footprint:** ~5MB (project files) + ~200MB (.NET SDK, already installed)
- **Published output:** ~1MB (framework-dependent) or ~80MB (self-contained)
- **Runtime memory:** <100MB (Kestrel + Blazor Server + one SignalR circuit)
- **Data files:** 2–50KB per `data.json` file in `wwwroot/`

### CI/CD

**None.** This is a local developer tool. The deployment is `dotnet run`. The CI/CD pipeline is the developer pressing F5.

If sharing with a colleague who lacks the .NET SDK:

```bash
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
```

This produces a self-contained folder (~80MB) that runs on any Windows 10/11 machine without .NET installed.

### Development Workflow

```bash
# Clone and run (zero-config startup)
git clone <repo>
cd ReportingDashboard
dotnet run

# Development with hot reload
dotnet watch

# Edit data.json in any editor → browser auto-refreshes
```

---

## Technology Stack Decisions

| Concern | Decision | Justification |
|---------|----------|---------------|
| **UI framework** | Blazor Server (.NET 8) | Mandated by PM spec. Provides C# data binding, hot reload, server-side rendering. No JavaScript needed. |
| **CSS approach** | Pure CSS (Grid + Flexbox) + Blazor scoped CSS | The design uses CSS Grid for the heatmap and Flexbox for the header/timeline. Scoped CSS prevents style leakage. No CSS framework (Bootstrap, Tailwind) needed—the design has exact pixel values. |
| **SVG rendering** | Inline `<svg>` in Razor markup | The timeline requires ~15-50 SVG elements (lines, circles, polygons, text). Blazor renders SVG natively. No charting library needed—the design specifies exact coordinates. |
| **JSON deserialization** | `System.Text.Json` (built-in) | Built into .NET 8. Zero NuGet packages. Supports `DateOnly`, `Dictionary<string, string[]>`, camelCase mapping, and lenient parsing. |
| **Data model** | C# `record` types (C# 12) | Immutable, concise, value-equality semantics. Perfect for deserializing JSON into read-only objects. |
| **File I/O** | `System.IO.File.ReadAllTextAsync` | Built-in. No `HttpClient` needed—the file is on the local filesystem. |
| **DI pattern** | Singleton service | `DashboardDataService` is stateless and thread-safe. Singleton avoids per-request allocation. |
| **Font** | Segoe UI (system font) | Pre-installed on Windows. No web font loading delay. Matches the design reference exactly. |
| **Hosting** | Kestrel (localhost) | Embedded in .NET 8. No IIS, no Docker, no cloud. Zero config. |
| **Hot reload** | `dotnet watch` | Built-in. Detects changes to `.razor`, `.css`, and static files. Ideal for the edit-screenshot workflow. |

### Explicitly Rejected Technologies

| Technology | Why Rejected |
|------------|-------------|
| MudBlazor / Radzen / Syncfusion | Opinionated component styles fight the custom design. Adds 500KB+ of assets. The design requires exact hex colors and pixel positions—component libraries get in the way. |
| ApexCharts / ChartJs.Blazor | The timeline is ~15 SVG elements. A charting library adds configuration complexity and JavaScript dependencies for something achievable in 50 lines of Razor. |
| Entity Framework / SQLite | There is no relational data. One JSON file. Adding a database adds migrations, connection strings, and schema management for zero benefit. |
| Newtonsoft.Json | `System.Text.Json` is built-in, faster, and has no external dependency. No features in Newtonsoft are needed that `System.Text.Json` lacks for this use case. |
| Blazor WebAssembly | 10MB+ download, slower startup, no filesystem access. Blazor Server is strictly superior for a local tool. |
| JavaScript / npm / webpack | Violates the "no JavaScript" constraint. All rendering is achievable with server-side Blazor. |
| Docker | The app runs on `dotnet run`. Docker adds image management complexity for a tool that runs on the developer's own machine. |
| Bootstrap / Tailwind CSS | The design specifies exact pixel values and hex colors. Utility classes add indirection without simplifying anything. Pure CSS is clearer. |

---

## Security Considerations

### Threat Model

This application has an extremely narrow threat surface. It runs on localhost, is accessed by a single user, serves no external traffic, stores no secrets, and processes no user input beyond a query parameter.

| Threat | Risk Level | Mitigation |
|--------|-----------|------------|
| **Path traversal via `?data=` parameter** | Medium | Filename validation: reject any value containing `..`, `/`, `\`, or not matching `Path.GetFileName()`. Only files directly in `wwwroot/` are loadable. |
| **Remote access to localhost** | Low | Kestrel binds to `127.0.0.1` only. Cannot be reached from the network. |
| **Malicious JSON payload** | Very Low | `System.Text.Json` is memory-safe. No dynamic code execution. Maximum file size is bounded by filesystem. |
| **XSS via data.json content** | Low | Blazor's Razor rendering auto-encodes all output by default. `@data.Title` produces HTML-encoded text, not raw HTML. Even if `data.json` contains `<script>` tags, they render as escaped text. |
| **Denial of service** | N/A | Single user, localhost. No external traffic. |
| **Data exfiltration** | N/A | No PII, no secrets, no sensitive data. `data.json` contains project names and milestone labels. |

### Path Traversal Prevention (Detail)

The `?data=` query parameter is the **only user-controlled input** in the application. The validation is:

```csharp
// Reject if:
// 1. Empty or whitespace
// 2. Contains ".." (parent directory traversal)
// 3. Contains "/" or "\" (subdirectory access)
// 4. Doesn't match its own filename (catches edge cases)
if (string.IsNullOrWhiteSpace(filename) ||
    filename.Contains("..") ||
    filename.Contains('/') ||
    filename.Contains('\\') ||
    filename != Path.GetFileName(filename))
{
    return (null, $"Invalid filename: {filename}");
}
```

This ensures only files in the `wwwroot/` root directory are accessible. No subdirectories, no parent directories, no absolute paths.

### Authentication & Authorization

**None.** Explicitly out of scope. The app is a single-user local tool. Adding auth would be over-engineering.

**Future-proofing:** If the team later wants to share the running instance on a LAN, add localhost-only middleware in `Program.cs`:

```csharp
app.Use(async (context, next) => {
    if (!IPAddress.IsLoopback(context.Connection.RemoteIpAddress!))
    {
        context.Response.StatusCode = 403;
        return;
    }
    await next();
});
```

### Data Protection

- No encryption needed—`data.json` is a local file with no sensitive content.
- No cookies, sessions, or user tracking.
- No logging of user data.
- Blazor Server's anti-forgery token is included by default (`app.UseAntiforgery()`) but serves no purpose here. It's harmless to leave enabled.

---

## Scaling Strategy

### This Application Does Not Scale (By Design)

This is a single-user, localhost-only tool. It serves one browser tab for one person on one machine. Scaling is not a concern, and designing for it would add unnecessary complexity.

**What "scaling" means in this context:**

| Dimension | Current | Maximum | Notes |
|-----------|---------|---------|-------|
| Concurrent users | 1 | 1 | Localhost only. One browser tab. |
| Data file size | 2–5KB | 100KB | Performance target: <50ms deserialization |
| Milestone tracks | 3 | 5 | SVG Y-axis spacing formula handles 1–5 |
| Events per track | 5–10 | 20 | Performance target: <200ms SVG render |
| Heatmap months | 4 | 6 | CSS Grid column formula handles 2–6 |
| Items per heatmap cell | 2–4 | 10 | CSS `overflow: hidden` clips excess |
| Data files in wwwroot | 1–3 | ~50 | Filesystem limit; switched via query param |

### Content Scaling (More Data in the Same Layout)

The architecture handles variable data through:

1. **Dynamic SVG track positioning:** `Y = SvgHeight / (trackCount + 1) * (i + 1)` — works for 1–5 tracks.
2. **Dynamic CSS Grid columns:** `grid-template-columns: 160px repeat(@months.Length, 1fr)` — works for 2–6 months.
3. **Dynamic heatmap items:** `@foreach` loops render 0–10 items per cell. Empty cells show `"-"`. Overflow is CSS-clipped.
4. **Dynamic timeline range:** `DateToX()` linear interpolation works for any date range defined in `data.json`.

### If Requirements Change

| Future Need | Recommended Approach |
|-------------|---------------------|
| Multiple users viewing simultaneously | Still works—Blazor Server handles multiple SignalR circuits. Bind to `0.0.0.0` instead of `127.0.0.1`. Add localhost check middleware. |
| Larger datasets (>100KB JSON) | Add in-memory caching in `DashboardDataService` with file-watcher invalidation. |
| Multiple dashboards per page | Not supported. Each `data.json` = one dashboard. Switch via `?data=` parameter. |
| Automated screenshot generation | Add Playwright as a dev dependency. Call after page load. Separate concern from the dashboard app. |

---

## UI Component Architecture

### Design-to-Component Mapping

The entire UI is rendered by a single Blazor component (`Dashboard.razor`). The component's markup is divided into logical sections that map 1:1 to the visual sections in `OriginalDesignConcept.html`.

#### Section 1: Error Banner (conditional)

| Property | Value |
|----------|-------|
| **Design section** | Not in original design (enhancement) |
| **Blazor markup** | `<div class="error-banner">` rendered when `errorMessage != null` |
| **CSS layout** | Block, `padding: 16px 44px`, `text-align: center` |
| **Background** | `#FEF2F2` |
| **Text color** | `#991B1B` |
| **Data binding** | `@errorMessage` |
| **Interaction** | None (static text) |
| **Render condition** | `@if (errorMessage is not null)` |

#### Section 2: Header Bar (`.hdr`)

| Property | Value |
|----------|-------|
| **Design section** | Top bar in `OriginalDesignConcept.html` — title left, legend right |
| **Blazor markup** | `<div class="hdr">` containing left div (title + subtitle) and right div (legend) |
| **CSS layout** | Flexbox row, `align-items: center`, `justify-content: space-between`, `padding: 12px 44px 10px` |
| **Border** | Bottom `1px solid #E0E0E0` |
| **Data bindings** | `@dashboardData.Title`, `@dashboardData.Subtitle`, `@dashboardData.BacklogUrl` |
| **Interactions** | ADO Backlog hyperlink (`<a href="@dashboardData.BacklogUrl">`) — only interactive element |

**Sub-components within Header:**

| Element | CSS | Data Binding |
|---------|-----|-------------|
| Title `<h1>` | `font-size: 24px; font-weight: 700` | `@dashboardData.Title` + conditional `<a>` for backlog link |
| Subtitle `<div class="sub">` | `font-size: 12px; color: #888; margin-top: 2px` | `@dashboardData.Subtitle` |
| Legend container | `display: flex; gap: 22px; align-items: center` | Static markup (legend items don't change) |
| PoC diamond icon | `width: 12px; height: 12px; background: #F4B400; transform: rotate(45deg)` | Static |
| Production diamond icon | `width: 12px; height: 12px; background: #34A853; transform: rotate(45deg)` | Static |
| Checkpoint circle icon | `width: 8px; height: 8px; border-radius: 50%; background: #999` | Static |
| Now line icon | `width: 2px; height: 14px; background: #EA4335` | Static |

#### Section 3: Timeline Area (`.tl-area`)

| Property | Value |
|----------|-------|
| **Design section** | Middle band in `OriginalDesignConcept.html` — sidebar + SVG area |
| **Blazor markup** | `<div class="tl-area">` containing sidebar div + SVG box div |
| **CSS layout** | Flexbox row, `align-items: stretch`, `height: 196px`, `background: #FAFAFA` |
| **Border** | Bottom `2px solid #E8E8E8` |
| **Padding** | `6px 44px 0` |

**Sub-component: Timeline Sidebar**

| Property | Value |
|----------|-------|
| **Width** | `230px`, `flex-shrink: 0` |
| **CSS layout** | Flex column, `justify-content: space-around`, `padding: 16px 12px 16px 0` |
| **Border** | Right `1px solid #E0E0E0` |
| **Data binding** | `@foreach (var track in dashboardData.MilestoneTracks)` |
| **Per-track rendering** | Track name (`<strong>` in `track.Color`) + description (`color: #444`) |

**Sub-component: Timeline SVG**

| Property | Value |
|----------|-------|
| **Container** | `<div class="tl-svg-box">` with `flex: 1; padding-left: 12px; padding-top: 6px` |
| **SVG element** | `<svg width="1560" height="185" style="overflow:visible">` |
| **SVG filter** | `<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>` |

**SVG child elements (data-bound):**

| SVG Element | CSS/Attributes | Data Binding | Rendering Logic |
|-------------|---------------|-------------|-----------------|
| Month grid lines | `stroke="#bbb"`, `stroke-opacity="0.4"`, `stroke-width="1"` | `@foreach` over `GetMonthGridPositions()` | Vertical `<line>` at each month X position |
| Month labels | `fill="#666"`, `font-size="11"`, `font-weight="600"` | Month name from grid positions | `<text x="@(x+5)" y="14">` |
| "NOW" dashed line | `stroke="#EA4335"`, `stroke-width="2"`, `stroke-dasharray="5,3"` | `DateToX(dashboardData.CurrentDate)` | Vertical `<line>` at current date X |
| "NOW" label | `fill="#EA4335"`, `font-size="10"`, `font-weight="700"` | Static text "NOW" | `<text>` above dashed line |
| Track lines | `stroke="@track.Color"`, `stroke-width="3"` | `@foreach` over `GetTrackPositions()` | Horizontal `<line x1="0" x2="1560">` |
| Checkpoint circles | `r="7"`, `fill="white"`, `stroke="@track.Color"`, `stroke-width="2.5"` | `evt.Type == "checkpoint"` | `<circle>` at `(DateToX(evt.Date), trackY)` |
| Small checkpoint dots | `r="4"`, `fill="#999"` | `evt.Type == "checkpoint-small"` | `<circle>` solid fill |
| PoC diamonds | `fill="#F4B400"`, `filter="url(#sh)"` | `evt.Type == "poc"` | `<polygon points="@DiamondPoints(x, y)">` |
| Production diamonds | `fill="#34A853"`, `filter="url(#sh)"` | `evt.Type == "production"` | `<polygon points="@DiamondPoints(x, y)">` |
| Event date labels | `text-anchor="middle"`, `fill="#666"`, `font-size="10"` | `evt.Label` | `<text>` positioned above (Y-16) or below (Y+24), alternating per event index to avoid overlap |

#### Section 4: Heatmap Area (`.hm-wrap`)

| Property | Value |
|----------|-------|
| **Design section** | Bottom section in `OriginalDesignConcept.html` — title + CSS Grid |
| **Blazor markup** | `<div class="hm-wrap">` containing title div + grid div |
| **CSS layout** | Flex column, `flex: 1`, `min-height: 0`, `padding: 10px 44px 10px` |
| **Data binding** | Month columns from `dashboardData.Months`, row data from `dashboardData.Heatmap` |

**Sub-component: Heatmap Title**

| Property | Value |
|----------|-------|
| **CSS class** | `.hm-title` |
| **Style** | `font-size: 14px; font-weight: 700; color: #888; letter-spacing: 0.5px; text-transform: uppercase` |
| **Content** | Static: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" |

**Sub-component: Heatmap Grid (`.hm-grid`)**

| Property | Value |
|----------|-------|
| **CSS** | `display: grid; border: 1px solid #E0E0E0; flex: 1; min-height: 0` |
| **Dynamic columns** | `style="grid-template-columns: 160px repeat(@dashboardData.Months.Length, 1fr)"` |
| **Rows** | `grid-template-rows: 36px repeat(4, 1fr)` |

**Grid cells (data-bound):**

| Cell Type | CSS Class | Content | Data Binding |
|-----------|-----------|---------|-------------|
| Corner cell | `.hm-corner` | "STATUS" (static) | None |
| Month headers | `.hm-col-hdr` + conditional `.current-month` | Month name, " ★ Now" suffix if current | `@foreach` over `Months` with index check against `CurrentMonthIndex` |
| Row headers | `.hm-row-hdr` + `.ship-hdr`/`.prog-hdr`/`.carry-hdr`/`.block-hdr` | "✅ SHIPPED (N)" etc. | Row label + `HeatmapRow.TotalItemCount` |
| Data cells | `.hm-cell` + `.ship-cell`/`.prog-cell`/`.carry-cell`/`.block-cell` + conditional `.current-month` | Bullet-pointed work items or "-" dash | `@foreach` over `row.GetItems(month)` |

**Heatmap row rendering pattern (repeated 4 times with different CSS classes):**

```razor
@{
    var rows = new[] {
        (Label: "✅ SHIPPED", Row: dashboardData.Heatmap.Shipped, HdrClass: "ship-hdr", CellClass: "ship-cell"),
        (Label: "🔵 IN PROGRESS", Row: dashboardData.Heatmap.InProgress, HdrClass: "prog-hdr", CellClass: "prog-cell"),
        (Label: "🟡 CARRYOVER", Row: dashboardData.Heatmap.Carryover, HdrClass: "carry-hdr", CellClass: "carry-cell"),
        (Label: "🔴 BLOCKERS", Row: dashboardData.Heatmap.Blockers, HdrClass: "block-hdr", CellClass: "block-cell"),
    };
}

@foreach (var (label, row, hdrClass, cellClass) in rows)
{
    <div class="hm-row-hdr @hdrClass">@label (@row.TotalItemCount)</div>
    @for (int i = 0; i < dashboardData.Months.Length; i++)
    {
        var month = dashboardData.Months[i];
        var isCurrent = i == dashboardData.CurrentMonthIndex;
        var items = row.GetItems(month);
        <div class="hm-cell @cellClass @(isCurrent ? "current-month" : "")">
            @if (items.Length == 0)
            {
                <span style="color:#AAA">-</span>
            }
            else
            {
                @foreach (var item in items)
                {
                    <div class="it">@item</div>
                }
            }
        </div>
    }
}
```

**Hover effect (CSS-only, non-essential):**

```css
.hm-cell:hover {
    filter: brightness(0.97);
    transition: filter 0.15s ease;
}
```

---

## Risks & Mitigations

### Risk 1: SVG Layout Drift from Design Reference

**Severity:** High (defeats the primary goal of pixel-perfect fidelity)

**Cause:** Hand-coding SVG coordinates in Razor may produce different visual output than the reference `OriginalDesignConcept.html`.

**Mitigation:**
1. Extract exact numeric values from the reference HTML: SVG width (1560), height (185), track Y positions (42, 98, 154 for 3 tracks), month X positions (0, 260, 520, 780, 1040, 1300), diamond point offsets (±11px).
2. Use the `DateToX()` linear interpolation function matching the reference's coordinate system.
3. **Verification gate:** During Phase 1, overlay the Blazor screenshot on `OriginalDesignConcept.png` at 50% opacity and verify alignment within ±2px.

### Risk 2: `data.json` Schema Errors Breaking the Page

**Severity:** Medium (blocks the user from seeing any output)

**Cause:** Hand-edited JSON is error-prone—trailing commas, missing fields, wrong types.

**Mitigation:**
1. `JsonSerializerOptions` enables `AllowTrailingCommas` and `ReadCommentHandling.Skip`.
2. `DashboardDataService.LoadAsync()` wraps deserialization in try-catch, returning a user-friendly error message.
3. All model properties have null-coalescing defaults (empty arrays, placeholder strings).
4. The `Dashboard.razor` page renders an error banner instead of throwing, ensuring the page always loads.

### Risk 3: Blazor Server Overhead for a Read-Only Page

**Severity:** Low (functional, but architecturally heavier than necessary)

**Cause:** Blazor Server maintains a SignalR WebSocket and server-side component tree for a page that has no interactivity.

**Mitigation:** Accepted trade-off. The overhead is negligible for a single local user (<100MB memory, <50ms latency). Blazor Server provides hot reload, C# data binding, and scoped CSS—features that significantly improve the development workflow. The alternative (static HTML generation from a console app) loses the live-preview capability that is critical for rapid screenshot iteration.

### Risk 4: Fixed 1920×1080 Layout Clips on Smaller Screens

**Severity:** Low (the design is intentionally fixed-size)

**Cause:** Laptop screens (1366×768, 1440×900) are smaller than the dashboard canvas.

**Mitigation:**
1. CSS auto-scaling: `@media (max-width: 1919px) { body { transform: scale(calc(100vw / 1920)); transform-origin: top left; } }` scales the entire dashboard proportionally.
2. For pixel-perfect screenshots, users should set their browser to 1920×1080 (DevTools device mode, external monitor, or browser zoom).
3. README documents the intended workflow.

### Risk 5: Hot Reload Doesn't Detect `data.json` Changes

**Severity:** Low (minor inconvenience)

**Cause:** `dotnet watch` monitors `.cs`, `.razor`, and `.css` files by default. Static files in `wwwroot/` may not trigger a re-render.

**Mitigation:**
1. Blazor Server's `DashboardDataService` reads `data.json` fresh on each page load (no caching). A manual browser refresh (`F5`) always picks up changes.
2. If `dotnet watch` is configured with `--project` flag and `StaticFileWatcher` is enabled, JSON changes trigger a browser refresh automatically.
3. Document in README: "After editing data.json, refresh the browser if hot-reload doesn't trigger automatically."

### Risk 6: Date-to-X Mapping Produces Off-Screen Markers

**Severity:** Low (visual artifact, not a crash)

**Cause:** If `data.json` contains events outside the `timelineStart`–`timelineEnd` range, markers render outside the visible SVG area.

**Mitigation:** `DateToX()` uses `Math.Clamp()` to bound the X position between 0 and 1560. Events outside the range stack at the edges rather than disappearing or overflowing.

### Risk 7: Large Number of Heatmap Items Overflows Cells

**Severity:** Low (visual truncation)

**Cause:** If a cell has >5-6 items, they overflow the cell height.

**Mitigation:** CSS `overflow: hidden` on `.hm-cell` clips excess items. The PM spec limits cells to 0–10 items. For >6 items, the font size (12px) and line height (1.35) may require the cell height to grow; the CSS Grid `repeat(4, 1fr)` evenly distributes remaining vertical space, which naturally accommodates more items as long as the total doesn't exceed the page height.