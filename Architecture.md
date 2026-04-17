# Architecture

## Overview & Goals

This document defines the complete system architecture for the **Executive Reporting Dashboard** — a single-page Blazor Server application that renders project milestone timelines, delivery status heatmaps, and progress indicators at a fixed 1920×1080 resolution optimized for PowerPoint screenshot capture.

**Architecture Philosophy:** Intentional simplicity. One solution, one project, one page, one JSON data file, zero external dependencies.

### Primary Goals

| Goal | Architecture Response |
|------|----------------------|
| Reduce reporting prep time by 80% | Screenshot-ready page; update JSON → refresh → screenshot |
| Standardize status communication | Fixed 4-row × 4-column heatmap structure enforced by data model |
| Milestone timeline visibility | SVG timeline generated from strongly-typed C# models |
| Zero-friction data updates | Single `data.json` file drives all rendering — no code changes needed |
| $0 infrastructure cost | Localhost-only Blazor Server; no cloud, no database, no auth |

### Architectural Constraints

- **Stack:** C# .NET 8, Blazor Server — non-negotiable
- **No JavaScript:** All interactivity is CSS-only (tooltips via `:hover::after`)
- **No database:** `data.json` flat file is the sole data source
- **No authentication:** Localhost-only binding
- **No third-party UI libraries:** No MudBlazor, Radzen, or Syncfusion
- **Fixed viewport:** 1920×1080px, non-responsive, `overflow: hidden`
- **Zero additional NuGet packages** beyond the default Blazor Server template

---

## System Components

### 1. Application Host (`Program.cs`)

**Responsibility:** Bootstrap the Blazor Server application, register services, configure Kestrel to bind exclusively to `localhost:5000`.

**Interfaces:**
- Entry point: `dotnet run` or `dotnet watch`
- HTTP listener: `http://localhost:5000`

**Dependencies:** ASP.NET Core 8.0 framework reference

**Key Behaviors:**
- Registers `DashboardDataService` as a Singleton in the DI container
- Calls `DashboardDataService.LoadAsync()` during startup to fail fast on missing/malformed JSON
- Configures Kestrel to bind only to `http://localhost:5000` (not `0.0.0.0`)
- Disables HTTPS (unnecessary for localhost tool)
- Maps Blazor Server hub and fallback to `Dashboard.razor`

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

// Fail fast: validate data.json at startup
var dataService = app.Services.GetRequiredService<DashboardDataService>();
dataService.Load();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Urls.Add("http://localhost:5000");
app.Run();
```

### 2. Data Service (`Services/DashboardDataService.cs`)

**Responsibility:** Load, deserialize, validate, and cache `data.json` as a strongly-typed `DashboardData` object. Provide computed properties (current month index, progress bar percentages, timeline x-positions).

**Interfaces:**
- `DashboardData Data { get; }` — the cached, deserialized root model
- `string? ErrorMessage { get; }` — non-null if loading failed
- `bool IsLoaded { get; }` — true if data loaded successfully
- `void Load()` — reads and deserializes `data.json`, called at startup

**Dependencies:** `System.Text.Json` (built-in), file system access to `data.json`

**Data:** Holds a single `DashboardData` instance in memory for the application lifetime.

**Key Behaviors:**
- Reads `data.json` from the application's content root directory (`builder.Environment.ContentRootPath`)
- Deserializes with `JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }`
- If file is missing: sets `ErrorMessage` to `"data.json not found at {path}. Please create a data.json file."`, logs to `ILogger<DashboardDataService>`
- If JSON is malformed: catches `JsonException`, extracts `LineNumber`/`BytePositionInLine`, sets `ErrorMessage` with parse location details
- If required fields (`title`, `categories`) are null after deserialization: logs warnings but does not throw — renders what it can
- Computes `CurrentMonthIndex` by matching `nowDate` (or `DateTime.Now` if `nowDate` is null) against the `months[]` array using 3-letter month abbreviation matching
- Computes progress bar segment percentages by summing item counts across all months per category

```csharp
public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardData? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsLoaded => Data != null;
    public int CurrentMonthIndex { get; private set; } = -1;

    public void Load()
    {
        var path = Path.Combine(_env.ContentRootPath, "data.json");
        if (!File.Exists(path))
        {
            ErrorMessage = $"data.json not found at {path}. Please create a data.json file.";
            _logger.LogError(ErrorMessage);
            return;
        }
        try
        {
            var json = File.ReadAllText(path);
            Data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);
            ComputeCurrentMonthIndex();
        }
        catch (JsonException ex)
        {
            ErrorMessage = $"Failed to parse data.json at line {ex.LineNumber}, position {ex.BytePositionInLine}: {ex.Message}";
            _logger.LogError(ex, ErrorMessage);
        }
    }
}
```

### 3. Page Component (`Components/Pages/Dashboard.razor`)

**Responsibility:** Root page component. Injects `DashboardDataService`, handles error states, and composes child components.

**Interfaces:**
- Route: `@page "/"` (the only page)
- Injects: `DashboardDataService` via `@inject`

**Dependencies:** All child components (`DashboardHeader`, `ProgressSummaryBar`, `TimelineSection`, `HeatmapGrid`)

**Key Behaviors:**
- If `DashboardDataService.IsLoaded` is false, renders error state HTML with `ErrorMessage`
- If loaded, renders the four child components in vertical order, passing data via `[Parameter]` properties
- No interactivity, no event handlers, no forms — pure read-only rendering

### 4. Header Component (`Components/DashboardHeader.razor`)

**Responsibility:** Render the top header bar with project title, ADO backlog link, subtitle, and legend icons.

**Interfaces:**
- `[Parameter] public string Title { get; set; }`
- `[Parameter] public string Subtitle { get; set; }`
- `[Parameter] public string BacklogUrl { get; set; }`
- `[Parameter] public string? NowLabel { get; set; }` — e.g., "Apr 2026" for the legend's Now indicator

**Dependencies:** None (leaf component)

**CSS Strategy (scoped `DashboardHeader.razor.css`):**
- Flexbox row layout matching `.hdr` from reference design
- Uses `var(--color-link)` for the ADO link color
- Legend items use inline-block diamond/circle shapes via CSS `transform: rotate(45deg)` and `border-radius: 50%`

### 5. Progress Summary Bar Component (`Components/ProgressSummaryBar.razor`)

**Responsibility:** Render a 6px-tall horizontal bar showing proportional breakdown of shipped/in-progress/carryover/blocked items.

**Interfaces:**
- `[Parameter] public List<StatusCategory> Categories { get; set; }`
- `[Parameter] public List<string> Months { get; set; }`

**Dependencies:** None (leaf component)

**Key Behaviors:**
- Computes total item count per category across all months: `category.Items.Values.Sum(list => list.Count)`
- Computes grand total across all categories
- Renders a flexbox row of `<div>` elements, each with `flex-basis` set to `{categoryCount / grandTotal * 100}%`
- Each segment uses `background-color: var(--color-{cssClass})` based on the category's `cssClass` field
- If grand total is 0, renders a single gray bar

### 6. Timeline Section Component (`Components/TimelineSection.razor`)

**Responsibility:** Render the milestone timeline area: left sidebar with stream labels + SVG timeline with month grid, stream lines, event markers, and NOW indicator.

**Interfaces:**
- `[Parameter] public List<MilestoneStream> Streams { get; set; }`
- `[Parameter] public string NowDate { get; set; }` — ISO date string
- `[Parameter] public string TimelineStartDate { get; set; }` — ISO date string
- `[Parameter] public string TimelineEndDate { get; set; }` — ISO date string

**Dependencies:** None (generates SVG via `MarkupString`)

**SVG Generation Strategy:**

The component builds an SVG string in a `@code` block using C# string interpolation, then renders it via `@((MarkupString)svgMarkup)`. Key calculations:

```
SVG width: 1560px (fixed, matching reference)
SVG height: 185px (fixed)
Month column width: svgWidth / totalMonths (260px for 6-month range)
Stream y-positions: distributed evenly, starting at y=42 with ~56px spacing
Event x-position: (eventDate - startDate).TotalDays / (endDate - startDate).TotalDays * svgWidth
NOW x-position: same formula applied to nowDate
```

**SVG Elements Generated:**
1. `<defs>` with `<filter id="sh">` for drop shadow on diamonds
2. Vertical month grid lines at computed intervals
3. Month labels (11px, `#666`, font-weight 600)
4. Per-stream horizontal line (stroke-width 3, stream color)
5. Per-event markers:
   - `checkpoint` → `<circle>` with white fill and colored/gray stroke
   - `poc` → `<polygon>` diamond with `#F4B400` fill and shadow filter
   - `production` → `<polygon>` diamond with `#34A853` fill and shadow filter
6. Date labels per event (10px, `#666`, text-anchor middle)
7. NOW dashed vertical line (`#EA4335`, stroke-dasharray `5,3`) with "NOW" label

**CSS-Only Tooltips on Diamond Markers:**

Each diamond is wrapped in a `<g>` element with a `<title>` for native browser tooltip, plus a CSS-driven tooltip using an overlaid `<rect>` with `:hover` styling:

```html
<g class="milestone-marker" data-tooltip="Mar 26 – PoC Milestone">
  <polygon points="..." fill="#F4B400" filter="url(#sh)"/>
  <!-- invisible hover target (larger than diamond for easy hovering) -->
  <rect x="{x-15}" y="{y-15}" width="30" height="30" fill="transparent" class="tooltip-trigger"/>
</g>
```

The tooltip is rendered via CSS `::after` on `.tooltip-trigger:hover` using `attr(data-tooltip)` for content. This satisfies the "no JavaScript" requirement.

### 7. Heatmap Grid Component (`Components/HeatmapGrid.razor`)

**Responsibility:** Render the CSS Grid container for the monthly execution heatmap, including the title bar, corner cell, column headers, and delegate row rendering to `HeatmapRow`.

**Interfaces:**
- `[Parameter] public List<StatusCategory> Categories { get; set; }`
- `[Parameter] public List<string> Months { get; set; }`
- `[Parameter] public int CurrentMonthIndex { get; set; }` — -1 if no month should be highlighted

**Dependencies:** `HeatmapRow` child component

**CSS Strategy (scoped `HeatmapGrid.razor.css`):**
- CSS Grid: `grid-template-columns: 160px repeat(4, 1fr)`
- CSS Grid: `grid-template-rows: 36px repeat(4, 1fr)`
- Outer border: `1px solid var(--color-border)`
- Column header cells use `.hm-col-hdr` styling; current month gets `background: var(--color-current-month-header-bg)` and `color: var(--color-current-month-header-text)`

**Key Behaviors:**
- Renders a title bar div ("MONTHLY EXECUTION STATUS") above the grid
- Renders corner cell + month column headers (with " ★ Now" suffix on current month)
- Iterates `Categories` and renders one `HeatmapRow` per category

### 8. Heatmap Row Component (`Components/HeatmapRow.razor`)

**Responsibility:** Render one status row (e.g., "Shipped") with a colored row header and 4 month cells.

**Interfaces:**
- `[Parameter] public StatusCategory Category { get; set; }`
- `[Parameter] public List<string> Months { get; set; }`
- `[Parameter] public int CurrentMonthIndex { get; set; }`

**Dependencies:** `HeatmapCell` child component

**CSS Strategy:**
- Row header uses category-specific CSS class: `.{cssClass}-hdr` (e.g., `.ship-hdr`)
- Cell background class: `.{cssClass}-cell` with `.current` modifier for highlighted month

### 9. Heatmap Cell Component (`Components/HeatmapCell.razor`)

**Responsibility:** Render a single cell in the heatmap grid containing a bulleted list of status items or an empty-state placeholder.

**Interfaces:**
- `[Parameter] public List<string>? Items { get; set; }`
- `[Parameter] public string CssClass { get; set; }` — category CSS class (e.g., "ship")
- `[Parameter] public bool IsCurrent { get; set; }` — whether this cell is in the current month column

**Dependencies:** None (leaf component)

**Key Behaviors:**
- If `Items` is null or empty: renders `<div class="empty-placeholder">–</div>` in muted gray (`#AAA`)
- If `Items` has entries: renders each as `<div class="it">{item text}</div>` — the `::before` pseudo-element provides the colored bullet dot
- Cell overflow is `hidden` to maintain the fixed 1080px page height

### 10. Layout Component (`Components/Layout/MainLayout.razor`)

**Responsibility:** Minimal layout wrapper that sets the fixed 1920×1080 viewport on `<body>` and includes the global stylesheet.

**Key Behaviors:**
- Sets `<body>` to `width: 1920px; height: 1080px; overflow: hidden`
- Includes `css/app.css` for CSS custom properties
- Renders `@Body` with no navigation chrome, no sidebar, no footer
- Suppresses the default Blazor error UI (no interactive components = no circuit errors)

### 11. Global Stylesheet (`wwwroot/css/app.css`)

**Responsibility:** Define all CSS custom properties (color tokens), global reset styles, and the fixed viewport constraint.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        data.json (file system)                  │
└──────────────────────────────┬──────────────────────────────────┘
                               │ File.ReadAllText + JsonSerializer.Deserialize<DashboardData>
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│              DashboardDataService (Singleton)                    │
│  ┌──────────────┐  ┌─────────────────┐  ┌──────────────────┐   │
│  │ DashboardData │  │ CurrentMonthIdx │  │ ErrorMessage     │   │
│  └──────────────┘  └─────────────────┘  └──────────────────┘   │
└──────────────────────────────┬──────────────────────────────────┘
                               │ @inject DashboardDataService
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                   Dashboard.razor (page)                         │
│                                                                  │
│  ┌─ if error ──────────────────────────────────────────────┐    │
│  │  <div class="error">ErrorMessage</div>                  │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                  │
│  ┌─ if loaded ─────────────────────────────────────────────┐    │
│  │                                                          │    │
│  │  ┌──────────────────────────────────────────────────┐   │    │
│  │  │ DashboardHeader                                   │   │    │
│  │  │  [Parameter] Title, Subtitle, BacklogUrl, NowLabel│   │    │
│  │  └──────────────────────────────────────────────────┘   │    │
│  │                                                          │    │
│  │  ┌──────────────────────────────────────────────────┐   │    │
│  │  │ ProgressSummaryBar                                │   │    │
│  │  │  [Parameter] Categories, Months                   │   │    │
│  │  └──────────────────────────────────────────────────┘   │    │
│  │                                                          │    │
│  │  ┌──────────────────────────────────────────────────┐   │    │
│  │  │ TimelineSection                                   │   │    │
│  │  │  [Parameter] Streams, NowDate, Start/EndDate     │   │    │
│  │  └──────────────────────────────────────────────────┘   │    │
│  │                                                          │    │
│  │  ┌──────────────────────────────────────────────────┐   │    │
│  │  │ HeatmapGrid                                       │   │    │
│  │  │  [Parameter] Categories, Months, CurrentMonthIdx │   │    │
│  │  │  ┌────────────────────────────────────────────┐  │   │    │
│  │  │  │ HeatmapRow × 4                             │  │   │    │
│  │  │  │  ┌──────────────────────────────────────┐  │  │   │    │
│  │  │  │  │ HeatmapCell × 4                      │  │  │   │    │
│  │  │  │  └──────────────────────────────────────┘  │  │   │    │
│  │  │  └────────────────────────────────────────────┘  │   │    │
│  │  └──────────────────────────────────────────────────┘   │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

### Communication Pattern

**Unidirectional data flow only.** There is no user input, no event bubbling, no two-way binding.

1. `DashboardDataService` loads `data.json` once at application startup
2. `Dashboard.razor` reads from the service via `@inject` and destructures the `DashboardData` model into parameters for child components
3. Each child component receives its data via `[Parameter]` properties — immutable, read-only
4. Child components render HTML/CSS/SVG from their parameters — no callbacks, no state mutations
5. The render pipeline executes exactly once per page load (no re-renders unless browser refresh)

### Startup Sequence

```
1. dotnet run
2. Program.cs → WebApplication.CreateBuilder()
3. Register DashboardDataService as Singleton
4. app.Build()
5. DashboardDataService.Load() ← fail-fast validation
   ├── data.json missing → log error, set ErrorMessage
   ├── data.json malformed → log parse error with line/column
   └── data.json valid → deserialize into DashboardData, compute CurrentMonthIndex
6. app.Run() → Kestrel starts on http://localhost:5000
7. Browser navigates to http://localhost:5000
8. Blazor Server renders Dashboard.razor
   ├── If error → render error message page
   └── If loaded → render full dashboard via component tree
9. Browser paints complete dashboard in <500ms
```

---

## Data Model

### Root Entity: `DashboardData`

```csharp
public class DashboardData
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string BacklogUrl { get; set; } = string.Empty;
    public List<string> Months { get; set; } = new();
    public List<MilestoneStream> MilestoneStreams { get; set; } = new();
    public string? NowDate { get; set; }
    public string TimelineStartDate { get; set; } = string.Empty;
    public string TimelineEndDate { get; set; } = string.Empty;
    public List<StatusCategory> Categories { get; set; } = new();
}
```

### Entity: `MilestoneStream`

```csharp
public class MilestoneStream
{
    public string Id { get; set; } = string.Empty;       // "M1", "M2", "M3"
    public string Label { get; set; } = string.Empty;     // "Chatbot & MS Role"
    public string Color { get; set; } = string.Empty;     // "#0078D4"
    public List<TimelineEvent> Events { get; set; } = new();
}
```

### Entity: `TimelineEvent`

```csharp
public class TimelineEvent
{
    public string Date { get; set; } = string.Empty;      // "2026-03-26" (ISO 8601)
    public string Label { get; set; } = string.Empty;     // "Mar 26 PoC"
    public string Type { get; set; } = string.Empty;      // "checkpoint" | "poc" | "production"
    public string? LabelPosition { get; set; }            // "above" | "below" (optional, default: "above")
}
```

### Entity: `StatusCategory`

```csharp
public class StatusCategory
{
    public string Name { get; set; } = string.Empty;      // "Shipped", "In Progress", "Carryover", "Blockers"
    public string CssClass { get; set; } = string.Empty;  // "ship", "prog", "carry", "block"
    public Dictionary<string, List<string>> Items { get; set; } = new();
    // Key = month abbreviation ("Jan", "Feb"), Value = list of item descriptions
}
```

### Entity Relationships

```
DashboardData (1)
├── has many → MilestoneStream (1..5)
│   └── has many → TimelineEvent (0..N)
├── has many → StatusCategory (exactly 4)
│   └── has dictionary → Items[month] → List<string> (0..N per month)
└── has list → Months (exactly 4 strings for heatmap columns)
```

### Storage

| Aspect | Decision |
|--------|----------|
| **Storage medium** | Single `data.json` flat file in project content root |
| **Read pattern** | Read once at startup, cached in memory for application lifetime |
| **Write pattern** | Manual editing by project lead in any text editor |
| **Schema enforcement** | C# model classes with default values; `System.Text.Json` deserialization |
| **File location** | `{ContentRootPath}/data.json` — same directory as `.csproj` |
| **Maximum size** | 100KB (well within `System.Text.Json` default limits) |
| **Encoding** | UTF-8 (standard JSON) |

### Complete `data.json` Schema

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "milestoneStreams": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc", "labelPosition": "below" },
        { "date": "2026-04-01", "label": "Apr Prod (TBD)", "type": "production", "labelPosition": "above" }
      ]
    },
    {
      "id": "M2",
      "label": "PDS & Data Inventory",
      "color": "#00897B",
      "events": []
    },
    {
      "id": "M3",
      "label": "Auto Review DFD",
      "color": "#546E7A",
      "events": []
    }
  ],
  "nowDate": "2026-04-10",
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Privacy chatbot v1 deployed", "MS role mapping complete"],
        "Feb": ["Data inventory schema finalized"],
        "Mar": ["PDS integration PoC passed"],
        "Apr": ["Auto-review pipeline live"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": {
        "Jan": ["API gateway prototype"],
        "Feb": ["PDS connector build"],
        "Mar": ["Data classification engine"],
        "Apr": ["DPIA automation module", "Consent flow v2"]
      }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": {
        "Jan": [],
        "Feb": ["Legacy migration script"],
        "Mar": ["Vendor onboarding delay"],
        "Apr": ["Cross-team dependency on Auth"]
      }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": {
        "Jan": [],
        "Feb": [],
        "Mar": ["Awaiting legal review on DFD scope"],
        "Apr": ["Prod environment access pending"]
      }
    }
  ]
}
```

### Design Decisions

- **`months` is explicit, not computed.** Executives may want a rolling window (Feb–May) or a fiscal quarter view. The data model does not assume Jan–Apr.
- **`cssClass` on categories** eliminates switch statements in components. The Razor template simply applies `{cssClass}-cell`, `{cssClass}-hdr` classes directly.
- **`nowDate` is optional.** If omitted, the service uses `DateTime.Now`. This allows both auto-detection and manual override for generating dashboards for past/future dates.
- **`items` is a `Dictionary<string, List<string>>`** keyed by month abbreviation. This keeps the JSON flat and avoids nested objects for what is fundamentally a lookup table.
- **`labelPosition` on timeline events** allows control over whether date labels render above or below the stream line, preventing overlapping text on dense timelines.

---

## API Contracts

This application has **no REST API, no GraphQL API, and no external-facing endpoints**. The only HTTP endpoint is the Blazor Server page itself.

### HTTP Endpoints

| Method | Path | Purpose | Response |
|--------|------|---------|----------|
| GET | `/` | Render the dashboard page | HTML (Blazor Server SSR + SignalR) |
| GET | `/_blazor` | Blazor SignalR hub (framework-managed) | WebSocket upgrade |
| GET | `/_framework/*` | Blazor framework static assets | JS/CSS/WASM files |
| GET | `/css/app.css` | Global stylesheet with CSS custom properties | CSS file |

### Internal Service Contract

The `DashboardDataService` exposes a C# interface (not HTTP) consumed only by Blazor components:

```csharp
// Consumed via @inject in Razor components
public class DashboardDataService
{
    // Properties
    public DashboardData? Data { get; }
    public string? ErrorMessage { get; }
    public bool IsLoaded { get; }
    public int CurrentMonthIndex { get; }

    // Methods
    public void Load();  // Called once at startup

    // Computed helpers
    public double GetTimelineXPosition(string isoDate);
    public double GetStreamYPosition(int streamIndex, int totalStreams);
    public (double Shipped, double InProgress, double Carryover, double Blocked) GetProgressPercentages();
}
```

### Error Handling Contract

| Error Condition | Behavior | User-Facing Message |
|----------------|----------|---------------------|
| `data.json` missing | `ErrorMessage` set, `IsLoaded = false` | "Dashboard data not found. Please ensure data.json exists in the application directory." |
| `data.json` malformed JSON | `ErrorMessage` set with line/column, `IsLoaded = false` | "Failed to load dashboard data. Check application logs for details." |
| Required field null (`title`, `categories`) | Warning logged, partial render | Dashboard renders with empty strings/default values where data is missing |
| Empty `items` array for a month | Normal operation | Cell shows "–" placeholder in `#AAA` muted text |
| `nowDate` outside displayed months | Normal operation | No column highlighted; `CurrentMonthIndex = -1` |
| Unknown fields in JSON | Ignored silently | Dashboard renders normally (forward-compatible) |

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|---------------|
| **Operating System** | Windows 10/11 (Segoe UI font dependency) |
| **Runtime** | .NET 8 SDK (8.0.x latest patch) |
| **Memory** | < 100MB RSS |
| **Disk** | < 50MB (published app + data.json) |
| **Network** | Loopback only (127.0.0.1:5000) — no internet required |

### Hosting

| Aspect | Decision |
|--------|----------|
| **Web server** | Kestrel (built into ASP.NET Core) |
| **Reverse proxy** | None — direct Kestrel on localhost |
| **Process model** | On-demand: `dotnet run` → use → Ctrl+C |
| **Service registration** | None — not a Windows Service, not IIS-hosted |
| **Port** | `http://localhost:5000` (configurable in `launchSettings.json` or `Program.cs`) |

### Networking

- **Inbound:** HTTP on `localhost:5000` only. Kestrel binds to `127.0.0.1`, not `0.0.0.0`.
- **Outbound:** Zero. No telemetry, no package downloads at runtime, no API calls.
- **Firewall:** No ports need to be opened. Entirely loopback traffic.

### Storage

| Item | Location | Size | Access Pattern |
|------|----------|------|----------------|
| `data.json` | `{project root}/data.json` | < 100KB | Read once at startup |
| Static assets | `wwwroot/css/app.css` | < 10KB | Served by Kestrel static file middleware |
| Blazor framework JS | `_framework/blazor.server.js` | ~250KB | Served automatically by Blazor middleware |

### Publishing & Distribution

```bash
# Development
dotnet watch --project ReportingDashboard

# Production (self-contained, no .NET required on target)
dotnet publish -c Release -r win-x64 --self-contained -o ./publish

# Resulting artifact
./publish/
  ├── ReportingDashboard.exe    # Self-contained executable
  ├── data.json                 # Copy to publish folder
  └── wwwroot/                  # Static assets
```

### CI/CD

**Not required for MVP.** This is a local tool. If CI/CD is added later:

- **Build:** `dotnet build -c Release`
- **Test:** `dotnet test` (if test project added)
- **Publish:** `dotnet publish -c Release -r win-x64 --self-contained`
- **Artifact:** ZIP of publish folder

---

## Technology Stack Decisions

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Framework** | Blazor Server (.NET 8) | `net8.0` | Mandated by project stack. Provides component model, CSS isolation, hot-reload. LTS through Nov 2026. |
| **Language** | C# 12 | .NET 8 SDK | Latest stable C# with the .NET 8 SDK. Pattern matching for SVG generation, raw string literals for SVG templates. |
| **JSON Parser** | `System.Text.Json` | Built-in | Zero additional NuGet packages. High-performance, case-insensitive deserialization, clear error messages with line/column positions. |
| **CSS Layout** | CSS Grid + Flexbox | Native browser | Grid for the 5-column heatmap (`160px repeat(4, 1fr)`). Flexbox for header row and timeline area. Matches reference design exactly. |
| **SVG Rendering** | Hand-built via `MarkupString` | N/A | The timeline SVG is ~80 lines of basic primitives (line, circle, polygon, text). A charting library would add 500KB+ for no benefit. |
| **CSS Architecture** | CSS Custom Properties + Scoped CSS | Built-in | All 24 color tokens defined as `--color-*` variables in `:root`. Scoped `.razor.css` files for component-specific styles. Zero risk of style leaking. |
| **Font** | Segoe UI (system) | N/A | Available on all Windows machines. No web font download = faster render, air-gapped compatibility. |
| **Component Library** | None | - | MudBlazor/Radzen would fight the pixel-perfect reference design. 150 lines of purpose-built CSS beats importing a 500KB library and overriding its defaults. |
| **DI Container** | Built-in `Microsoft.Extensions.DependencyInjection` | Built-in | Single `Singleton<DashboardDataService>` registration. No need for Autofac/StructureMap. |
| **Logging** | Built-in `ILogger<T>` | Built-in | Console logger for startup errors. No Serilog, no Application Insights — console output is sufficient for a local tool. |

### Rejected Alternatives

| Alternative | Reason Rejected |
|-------------|----------------|
| Blazor WebAssembly | Larger download, slower startup. No benefit for localhost tool. |
| Blazor Static SSR | Would eliminate SignalR overhead but loses hot-reload during development. Blazor Server overhead is imperceptible for single-user local use. |
| MudBlazor / Radzen | Component libraries impose their own design system. The reference design requires pixel-exact control that is easier to achieve with raw CSS. |
| Chart.js / Plotly | The SVG timeline is simple geometric primitives. A charting library would add complexity without visual benefit. |
| SQLite / LiteDB | A database for a single JSON file is absurd over-engineering. |
| React / Vue | Non-.NET stack violates the mandate. |

---

## Security Considerations

### Authentication & Authorization

**Decision: None.** Zero authentication, zero authorization.

**Justification:** This is a single-user tool running on `localhost:5000`. Adding auth would increase complexity with zero security benefit — the threat model is a developer's own laptop.

**Binding restriction in `Program.cs`:**
```csharp
app.Urls.Add("http://localhost:5000");
```
This ensures Kestrel only listens on the loopback interface, not on any network-accessible address.

### Data Protection

| Concern | Mitigation |
|---------|------------|
| **PII in data.json** | The data model contains project status text, not personal data. Document in README that PII should not be placed in `data.json`. |
| **Sensitive project names** | Add `data.json` to `.gitignore`. Ship a `data.sample.json` with fictional data. |
| **Credentials in data.json** | The `backlogUrl` field may contain an ADO URL. No tokens or passwords are needed — the link is purely for display. |
| **data.json tampering** | Not a threat — the file is on the user's own machine. No integrity checks needed. |

### Input Validation

| Input | Validation |
|-------|------------|
| `data.json` content | `System.Text.Json` deserialization with `PropertyNameCaseInsensitive = true`. Malformed JSON throws `JsonException` with line/column info. |
| Required fields | Null checks on `Title` and `Categories` after deserialization. Missing values log warnings; app renders gracefully with defaults. |
| Date fields | Parsed via `DateTime.TryParse()`. Invalid dates result in events not being plotted (no crash). |
| `cssClass` values | Used directly in CSS class names. Since data.json is locally maintained (not user-submitted), there is no injection risk. Rendered via Blazor's built-in HTML encoding for `class` attributes. |
| `backlogUrl` | Rendered as `href` in an `<a>` tag with `target="_blank"`. Blazor's Razor engine HTML-encodes attribute values, preventing XSS. |

### Network Security

- **No inbound network access.** Kestrel binds to `127.0.0.1` only.
- **No outbound network calls.** Zero telemetry, zero CDN, zero external API calls.
- **No HTTPS.** Unnecessary for loopback traffic. Adding HTTPS would require certificate management for zero benefit.
- **No CORS.** No API endpoints to protect.

---

## Scaling Strategy

### Scaling is Explicitly Not Required

This application is a **local, single-user, on-demand screenshot tool**. It serves exactly one browser tab on one machine. Scaling considerations are intentionally minimal.

### Current Capacity

| Dimension | Current Limit | Justification |
|-----------|--------------|---------------|
| Concurrent users | 1 | Single browser tab on localhost |
| Data volume | 100KB JSON file | ~50 items across 4 categories × 4 months |
| Milestone streams | 5 maximum | SVG vertical space constraint at 185px height |
| Months displayed | 4 columns | Heatmap grid designed for 4-column layout |
| Items per cell | ~10 maximum | Cell overflow hidden at fixed 1080px page height |
| Render time | < 500ms | Single JSON deserialize + component render pass |

### Future Scaling Paths (If Ever Needed)

| Scenario | Approach | Effort |
|----------|----------|--------|
| **Multiple projects** | CLI argument `--data=projectA.json`; load different JSON files | 30 min |
| **More than 4 months** | Change CSS Grid to `repeat(N, 1fr)` driven by `months.length` | 1 hour |
| **More than 5 streams** | Dynamic SVG height calculation: `height = streams.Count * 56 + 20` | 1 hour |
| **Live data from ADO** | Replace `DashboardDataService.Load()` with an API call; component layer unchanged | 4 hours |
| **Team-shared instance** | Bind to `0.0.0.0:5000`, add `--network` CLI flag, consider basic auth | 2 hours |
| **Automated screenshot pipeline** | Add Playwright project; `dotnet run` → screenshot → email/Teams | 4 hours |

### Performance Budget

| Component | Budget | Notes |
|-----------|--------|-------|
| JSON deserialization | < 50ms | `System.Text.Json` handles 100KB trivially |
| SVG string generation | < 10ms | ~200 string concatenations for 3 streams × 5 events |
| Blazor component render | < 100ms | ~30 components total in the tree |
| CSS paint | < 50ms | No animations, no transitions, fixed layout |
| **Total page load** | **< 500ms** | From browser navigation to full paint |

---

## Risks & Mitigations

### Risk 1: Blazor Server SignalR Overhead

**Severity:** Low
**Impact:** Extra ~250KB JS download, persistent WebSocket connection for a read-only page.
**Probability:** 100% (inherent to Blazor Server architecture)

**Mitigation:** Accepted as a trade-off of the mandated stack. The overhead is imperceptible for a single-user localhost tool. If screenshot automation requires faster loads, switch to Blazor Static SSR by removing `AddInteractiveServerComponents()` and using `@rendermode` static — a 5-minute change.

### Risk 2: JSON Schema Drift

**Severity:** Medium
**Impact:** Silent data loss if new fields are added to `data.json` but not to C# models, or if existing fields are renamed.
**Probability:** Low-Medium (schema is simple, but will evolve over months)

**Mitigation:**
1. All C# model properties use default values (`string.Empty`, `new List<T>()`) so missing fields don't crash the app
2. `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` prevents casing mismatches
3. Startup validation logs warnings for null required fields (`Title`, `Categories`)
4. Ship a `data.sample.json` with comments documenting each field's purpose and expected format
5. Unknown fields in JSON are silently ignored (forward-compatible)

### Risk 3: SVG Text Overlap on Dense Timelines

**Severity:** Medium
**Impact:** Event labels overlap when multiple milestones are close together in date, making text unreadable.
**Probability:** Medium (depends on data density)

**Mitigation:**
1. `labelPosition` field on `TimelineEvent` allows manual above/below placement
2. Default alternating: odd events above, even events below
3. For very dense clusters, use smaller gray dot markers (`r=4`, no label) — matching the reference design's approach for M2's dense checkpoint cluster
4. Maximum ~10 events per stream keeps the timeline readable

### Risk 4: Fixed 1920×1080 Layout on Non-Standard Displays

**Severity:** Low
**Impact:** On laptop screens (1366×768, 1440×900), the page requires scrolling. On 4K displays, it appears small.
**Probability:** High (developers often use laptops)

**Mitigation:**
1. This is explicitly by design — the dashboard is a screenshot tool, not a responsive web app
2. Use browser zoom (Ctrl+/Ctrl-) to fit the viewport on smaller screens
3. For screenshot capture, use DevTools device emulation at exactly 1920×1080
4. Document in README: "Set browser to 1920×1080 viewport for screenshot capture"

### Risk 5: Stale Data After JSON Edit

**Severity:** Low
**Impact:** User edits `data.json` but forgets to restart the app; dashboard shows old data.
**Probability:** Medium

**Mitigation:**
1. During development: `dotnet watch` automatically reloads on file changes
2. During production use: document that `F5` (browser refresh) after `dotnet run` restart reflects changes
3. Future enhancement: add `IFileSystemWatcher` to detect `data.json` changes and reload automatically (not MVP)

### Risk 6: Content Overflow in Heatmap Cells

**Severity:** Medium
**Impact:** If a category has too many items in a single month (>10), text overflows the cell and is clipped by `overflow: hidden`, losing information.
**Probability:** Low (executives prefer concise bullet points)

**Mitigation:**
1. Document maximum ~10 items per cell in the `data.sample.json` comments
2. CSS `overflow: hidden` ensures the page never exceeds 1080px height
3. Items are 12px with 1.35 line-height ≈ 16.2px per item. Each row gets roughly `(1080 - 48 - 6 - 196 - 36 - 40) / 4 ≈ 188px`, fitting ~11 items per cell
4. If more items are needed, reduce `font-size` to 11px or `line-height` to 1.2 — a CSS-only change

### Risk 7: Browser Rendering Differences

**Severity:** Low
**Impact:** Subtle pixel differences between Chrome and Edge could produce inconsistent screenshots.
**Probability:** Very Low (both are Chromium-based)

**Mitigation:**
1. Chrome and Edge use the same Blink rendering engine — identical output
2. Firefox may have 1-2px differences in SVG text positioning; not supported for screenshots
3. Standardize on Edge for official screenshots (pre-installed on Windows)

---

## UI Component Architecture

This section maps each visual section from the `OriginalDesignConcept.html` reference design to a specific Blazor component.

### Component-to-Visual-Section Mapping

| Visual Section | Reference CSS Class | Blazor Component | File Path |
|---------------|-------------------|-----------------|-----------|
| Header bar | `.hdr` | `DashboardHeader.razor` | `Components/DashboardHeader.razor` |
| Progress bar | _(enhancement)_ | `ProgressSummaryBar.razor` | `Components/ProgressSummaryBar.razor` |
| Timeline area | `.tl-area` | `TimelineSection.razor` | `Components/TimelineSection.razor` |
| Milestone sidebar | inline `<div>` 230px | Part of `TimelineSection.razor` | (embedded, not separate component) |
| SVG timeline | `.tl-svg-box > svg` | Part of `TimelineSection.razor` | (SVG generated via `MarkupString`) |
| Heatmap wrapper | `.hm-wrap` | `HeatmapGrid.razor` | `Components/HeatmapGrid.razor` |
| Heatmap grid | `.hm-grid` | Part of `HeatmapGrid.razor` | (CSS Grid container) |
| Status row | `.hm-row-hdr` + cells | `HeatmapRow.razor` | `Components/HeatmapRow.razor` |
| Data cell | `.hm-cell` | `HeatmapCell.razor` | `Components/HeatmapCell.razor` |
| Error page | _(not in reference)_ | Part of `Dashboard.razor` | (conditional rendering) |

### Component Detail Specifications

#### `DashboardHeader.razor` → `.hdr` Section

**CSS Layout:** `display: flex; justify-content: space-between; align-items: center; padding: 12px 44px 10px; border-bottom: 1px solid var(--color-border);`

**Data Bindings:**
- `@Title` → `<h1>` inner text
- `@BacklogUrl` → `<a href="@BacklogUrl" target="_blank">` inline with title
- `@Subtitle` → `<div class="sub">` text

**Interactions:** ADO Backlog link opens in new tab. No other interactions.

**Rendered HTML Structure:**
```html
<div class="hdr">
  <div>
    <h1>@Title <a href="@BacklogUrl" target="_blank">→ ADO Backlog</a></h1>
    <div class="sub">@Subtitle</div>
  </div>
  <div class="legend">
    <span class="legend-item"><span class="diamond poc"></span>PoC Milestone</span>
    <span class="legend-item"><span class="diamond prod"></span>Production Release</span>
    <span class="legend-item"><span class="circle"></span>Checkpoint</span>
    <span class="legend-item"><span class="now-bar"></span>Now (@NowLabel)</span>
  </div>
</div>
```

#### `ProgressSummaryBar.razor` → Enhancement (between header and timeline)

**CSS Layout:** `display: flex; height: 6px; width: 100%;`

**Data Bindings:**
- Each segment: `flex-basis: @(percentage)%` with `background-color: var(--color-{cssClass})`

**Interactions:** None (visual indicator only).

#### `TimelineSection.razor` → `.tl-area` Section

**CSS Layout:** `display: flex; align-items: stretch; height: 196px; background: #FAFAFA; border-bottom: 2px solid #E8E8E8; padding: 6px 44px 0;`

**Sub-sections:**
1. **Left sidebar (230px):** Flexbox column with `justify-content: space-around`. Each stream renders as:
   ```html
   <div style="color: @stream.Color; font-size: 12px; font-weight: 600;">
     @stream.Id<br/><span style="font-weight: 400; color: #444;">@stream.Label</span>
   </div>
   ```

2. **SVG container (flex: 1):** Renders `@((MarkupString)GenerateSvg())`

**Data Bindings:**
- `@Streams` → sidebar labels + SVG stream lines and events
- `@NowDate` → red dashed NOW line x-position
- `@TimelineStartDate` / `@TimelineEndDate` → x-axis scaling

**Interactions:** CSS-only tooltips on diamond markers via `:hover::after` pseudo-element on SVG `<foreignObject>` or overlay `<div>` elements.

**SVG Tooltip Implementation:**
```css
/* In TimelineSection.razor.css */
.tooltip-trigger:hover::after {
    content: attr(data-tooltip);
    position: absolute;
    background: white;
    border: 1px solid var(--color-border);
    box-shadow: 0 2px 8px rgba(0,0,0,0.15);
    padding: 4px 8px;
    font-size: 11px;
    white-space: nowrap;
    z-index: 10;
    pointer-events: none;
}
```

#### `HeatmapGrid.razor` → `.hm-wrap` Section

**CSS Layout:**
```css
.hm-wrap { flex: 1; min-height: 0; display: flex; flex-direction: column; padding: 10px 44px 10px; }
.hm-grid { flex: 1; min-height: 0; display: grid;
           grid-template-columns: 160px repeat(4, 1fr);
           grid-template-rows: 36px repeat(4, 1fr);
           border: 1px solid var(--color-border); }
```

**Data Bindings:**
- `@Months` → column header text (with current month getting gold highlight)
- `@CurrentMonthIndex` → applies `.current-month-hdr` class to the matching column header
- `@Categories` → iterated to render `HeatmapRow` instances

**Rendered Structure:**
```html
<div class="hm-wrap">
  <div class="hm-title">MONTHLY EXECUTION STATUS</div>
  <div class="hm-grid">
    <div class="hm-corner">STATUS</div>
    @foreach month in Months → <div class="hm-col-hdr @(isCurrentMonth ? "current-month-hdr" : "")">@month</div>
    @foreach category in Categories → <HeatmapRow ... />
  </div>
</div>
```

#### `HeatmapRow.razor` → One row per status category

**CSS Layout:** Inherits grid placement from parent. Row header + 4 cells span one grid row.

**Data Bindings:**
- `@Category.Name` → row header text (uppercased via CSS `text-transform: uppercase`)
- `@Category.CssClass` → CSS class for row header background (`{cssClass}-hdr`) and cell backgrounds (`{cssClass}-cell`)
- `@Category.Items[month]` → passed to each `HeatmapCell`
- `@CurrentMonthIndex` → adds `.current` class to the matching cell

#### `HeatmapCell.razor` → Single data cell

**CSS Layout:** `padding: 8px 12px; border-right: 1px solid var(--color-border); border-bottom: 1px solid var(--color-border); overflow: hidden;`

**Data Bindings:**
- `@Items` → list of strings, each rendered as `<div class="it">@item</div>`
- `@CssClass` → determines `::before` bullet dot color via `background: var(--color-{cssClass})`
- `@IsCurrent` → adds `.current` class for darker background tint

**Empty State:** If `Items` is null or empty, renders `<div class="empty">–</div>` in `color: #AAA`.

### CSS Custom Property Strategy

All color values are defined in `wwwroot/css/app.css` `:root` block. Component `.razor.css` files reference these via `var()`. No hex values appear in component CSS.

```css
:root {
    /* Status colors */
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-shipped-header-bg: #E8F5E9;
    --color-shipped-header-text: #1B7A28;

    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-progress-header-bg: #E3F2FD;
    --color-progress-header-text: #1565C0;

    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-carryover-header-bg: #FFF8E1;
    --color-carryover-header-text: #B45309;

    --color-blocker: #EA4335;
    --color-blocker-bg: #FFF5F5;
    --color-blocker-bg-current: #FFE4E4;
    --color-blocker-header-bg: #FEF2F2;
    --color-blocker-header-text: #991B1B;

    /* Structural colors */
    --color-border: #E0E0E0;
    --color-border-heavy: #CCC;
    --color-text-primary: #111;
    --color-text-secondary: #888;
    --color-text-item: #333;
    --color-text-muted: #AAA;
    --color-link: #0078D4;
    --color-background: #FFFFFF;
    --color-background-alt: #FAFAFA;
    --color-background-header: #F5F5F5;

    /* Current month highlight */
    --color-current-month-bg: #FFF0D0;
    --color-current-month-text: #C07700;

    /* Timeline markers */
    --color-poc: #F4B400;
    --color-production: #34A853;
    --color-checkpoint: #999;
    --color-now: #EA4335;

    /* Typography */
    --font-family: 'Segoe UI', Arial, sans-serif;
}
```

### Project File Structure

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj         Target: net8.0, no extra NuGet packages
    ├── Program.cs                         Host bootstrap, DI registration, Kestrel config
    ├── data.json                          Live data (gitignored if sensitive)
    ├── data.sample.json                   Fictional sample data (committed to repo)
    ├── Models/
    │   ├── DashboardData.cs              Root model with all nested types
    │   ├── MilestoneStream.cs            Timeline stream with events
    │   ├── TimelineEvent.cs              Single event (checkpoint/poc/production)
    │   └── StatusCategory.cs             Heatmap row (name, cssClass, items dict)
    ├── Services/
    │   └── DashboardDataService.cs       JSON loader, validator, computed properties
    ├── Components/
    │   ├── App.razor                     Root component (<html>, <head>, <body>)
    │   ├── Routes.razor                  Router configuration
    │   ├── Pages/
    │   │   └── Dashboard.razor           The single page (route: "/")
    │   ├── Layout/
    │   │   ├── MainLayout.razor          Minimal wrapper (1920×1080 body)
    │   │   └── MainLayout.razor.css      Body sizing, overflow hidden
    │   ├── DashboardHeader.razor         Title, subtitle, legend
    │   ├── DashboardHeader.razor.css     Scoped header styles
    │   ├── ProgressSummaryBar.razor      6px health bar
    │   ├── ProgressSummaryBar.razor.css  Scoped bar styles
    │   ├── TimelineSection.razor         SVG timeline + sidebar
    │   ├── TimelineSection.razor.css     Scoped timeline styles + tooltip CSS
    │   ├── HeatmapGrid.razor             CSS Grid container + headers
    │   ├── HeatmapGrid.razor.css         Grid layout, header cell styles
    │   ├── HeatmapRow.razor              Single status row
    │   ├── HeatmapRow.razor.css          Row header colors
    │   ├── HeatmapCell.razor             Single cell with items
    │   └── HeatmapCell.razor.css         Cell styles, bullet dots, empty state
    ├── wwwroot/
    │   └── css/
    │       └── app.css                   CSS custom properties, global reset
    └── Properties/
        └── launchSettings.json           Dev profile: http://localhost:5000
```