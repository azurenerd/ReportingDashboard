# Architecture

**Document Version:** 1.0
**Date:** April 16, 2026
**Project:** Executive Reporting Dashboard
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure
**Design Reference:** `OriginalDesignConcept.html` (canonical)

---

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server application that renders a pixel-perfect 1920×1080 project status visualization suitable for direct screenshot-to-PowerPoint workflows. It reads all data from a local `data.json` file, requires zero external dependencies, and runs entirely on localhost via Kestrel.

**Architecture Goals:**

1. **Minimal footprint** — Under 15 files total (excluding `bin/`, `obj/`, `.vs/`), zero NuGet dependencies beyond the default Blazor Server template
2. **Pixel-perfect fidelity** — Razor components produce HTML/SVG/CSS that is visually indistinguishable from `OriginalDesignConcept.html` at 1920×1080
3. **Data-driven rendering** — Every visible string, milestone, and heatmap item originates from `data.json`; zero hardcoded display content
4. **Zero-config launch** — `dotnet run` on any Windows machine with .NET 8 SDK produces a working dashboard at `https://localhost:5001`
5. **Sub-2-second load** — Local file read + JSON deserialization + Blazor Server render completes in under 2 seconds

**Architecture Principles:**

- **Do not over-architect.** This is a single-page, single-user, local-only tool. Every architectural decision optimizes for simplicity.
- **Port, don't reinvent.** The CSS and SVG structure from `OriginalDesignConcept.html` should be ported directly, then parameterized for data-driven rendering.
- **No JavaScript.** All rendering is Razor + CSS + inline SVG. No JS interop, no JS files.
- **No CSS frameworks.** Custom CSS only, ported from the design reference.
- **No charting libraries.** Hand-crafted SVG primitives via Razor markup.

---

## System Components

### Solution Structure

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── Program.cs
    ├── ReportingDashboard.csproj
    ├── Models/
    │   └── DashboardData.cs
    ├── Services/
    │   └── DashboardDataService.cs
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor
    │   ├── Layout/
    │   │   └── MainLayout.razor
    │   ├── DashboardHeader.razor
    │   ├── TimelineSection.razor
    │   └── HeatmapGrid.razor
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css
    │   └── data/
    │       ├── data.json
    │       └── data.sample.json
    └── Properties/
        └── launchSettings.json
```

### Component 1: `Program.cs` — Application Entry Point

**Responsibility:** Configure and start the Blazor Server application with minimal middleware.

**Interfaces:** None (entry point only).

**Dependencies:** `DashboardDataService` (registered in DI).

**Key Behaviors:**
- Registers `DashboardDataService` as a Singleton in the DI container
- Configures Kestrel to bind to `localhost` only
- Strips all unnecessary middleware (no auth, no HTTPS redirection in dev, no static file caching headers)
- Maps Blazor Server hub and fallback to `Dashboard.razor`

**Implementation Sketch:**
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

### Component 2: `Models/DashboardData.cs` — Data Model Records

**Responsibility:** Define strongly-typed C# records that mirror the `data.json` schema exactly.

**Interfaces:** Pure data records, no methods beyond what records provide.

**Dependencies:** `System.Text.Json.Serialization` for `[JsonPropertyName]` attributes.

**Records:**
```csharp
public record DashboardData(
    string Title,
    string Subtitle,
    string BacklogUrl,
    string CurrentDate,
    string TimelineStart,
    string TimelineEnd,
    List<MilestoneStream> MilestoneStreams,
    HeatmapData Heatmap
);

public record MilestoneStream(
    string Id,
    string Name,
    string Color,
    List<Milestone> Milestones
);

public record Milestone(
    string Date,
    string Label,
    string Type,
    string? LabelPosition  // "above" or "below", nullable (default: "above")
);

public record HeatmapData(
    List<string> Columns,
    int CurrentColumnIndex,
    List<HeatmapRow> Rows
);

public record HeatmapRow(
    string Category,
    Dictionary<string, List<string>> Items
);
```

**JSON Property Naming:** Use `JsonSerializerOptions` with `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` at the service level rather than annotating every property. This keeps the records clean.

### Component 3: `Services/DashboardDataService.cs` — Data Access Service

**Responsibility:** Read `data.json` from disk, deserialize it, cache it in memory, and provide it to Razor components. Handle errors gracefully.

**Interface (public API):**
```csharp
public class DashboardDataService
{
    public DashboardData? Data { get; }
    public string? ErrorMessage { get; }
    public bool HasError { get; }
    public Task LoadAsync();
}
```

**Dependencies:** `IWebHostEnvironment` (injected, to resolve `wwwroot/data/data.json` path).

**Lifecycle:** Registered as **Singleton**. `LoadAsync()` is called once during application startup (in `Program.cs` after `builder.Build()` but before `app.Run()`).

**Key Behaviors:**
- Reads `wwwroot/data/data.json` using `File.ReadAllTextAsync`
- Deserializes using `System.Text.Json.JsonSerializer.Deserialize<DashboardData>()` with `CamelCase` naming policy
- On success: populates `Data` property, sets `HasError = false`
- On `FileNotFoundException`: sets `ErrorMessage = "data.json not found. Place a valid data.json file in wwwroot/data/ and restart the application."`
- On `JsonException`: sets `ErrorMessage = "data.json is malformed: {exception.Message}. Check JSON syntax and required fields."`
- On any other exception: sets `ErrorMessage = "Unexpected error loading data.json: {exception.Message}"`
- Validates required fields after deserialization: `Title`, `MilestoneStreams`, `Heatmap`, `Heatmap.Columns`, `Heatmap.Rows` must be non-null. If any are null, sets a descriptive `ErrorMessage`.

**File Path Resolution:**
```csharp
var path = Path.Combine(_env.WebRootPath, "data", "data.json");
```

### Component 4: `Components/Layout/MainLayout.razor` — Minimal Layout Shell

**Responsibility:** Provide the outermost Blazor layout with zero chrome — no nav, no sidebar, no footer.

**Markup:**
```razor
@inherits LayoutComponentBase

<div style="width:1920px;height:1080px;overflow:hidden;">
    @Body
</div>
```

**Dependencies:** None.

**Key Behavior:** The layout wraps `@Body` in a fixed-size container. All visual structure is handled by child components. The default Blazor template's `NavMenu`, `MainLayout` sidebar, and `<PageTitle>` are **removed entirely**.

### Component 5: `Components/Pages/Dashboard.razor` — Page Orchestrator

**Responsibility:** The single routable page (`@page "/"`). Injects `DashboardDataService`, checks for errors, and renders the three visual sections via child components.

**Route:** `@page "/"`

**Dependencies:** `DashboardDataService` (injected via `@inject`).

**Key Behaviors:**
- If `DashboardDataService.HasError` is true, renders an error panel with the error message (styled simply: centered text, red border, readable font)
- If data loaded successfully, renders three child components in order:
  1. `<DashboardHeader Data="@service.Data" />`
  2. `<TimelineSection Data="@service.Data" />`
  3. `<HeatmapGrid Data="@service.Data" />`

**Error Display Markup:**
```razor
@if (service.HasError)
{
    <div style="padding:40px;margin:40px;border:2px solid #EA4335;border-radius:8px;
                font-family:'Segoe UI';font-size:16px;color:#991B1B;">
        <h2 style="margin-bottom:12px;">⚠ Dashboard Error</h2>
        <p>@service.ErrorMessage</p>
        <p style="margin-top:12px;color:#666;font-size:13px;">
            Fix the issue in <code>wwwroot/data/data.json</code> and restart the application.
        </p>
    </div>
}
```

### Component 6: `Components/DashboardHeader.razor` — Header Bar

**Responsibility:** Render the project title, subtitle, backlog link, and milestone legend.

**Parameter:** `[Parameter] public DashboardData Data { get; set; }`

**Visual Reference:** `.hdr` section in `OriginalDesignConcept.html`

**Markup Structure:**
- Outer `<div class="hdr">` with flexbox row layout
- Left side: `<h1>` with `Data.Title` and inline `<a href="@Data.BacklogUrl">` for backlog link
- Below title: `<div class="sub">` with `Data.Subtitle`
- Right side: four legend items (PoC diamond, Production diamond, Checkpoint circle, NOW line) — these are **static HTML** since the legend icons don't change per-project; only the "Now" label text is dynamic (derived from `Data.CurrentDate`)

**Data Bindings:**
| Element | Source Field |
|---------|-------------|
| Title text | `Data.Title` |
| Backlog link href | `Data.BacklogUrl` |
| Subtitle text | `Data.Subtitle` |
| "Now" label | Formatted from `Data.CurrentDate` (e.g., "Now (Apr 2026)") |

### Component 7: `Components/TimelineSection.razor` — SVG Milestone Timeline

**Responsibility:** Render the timeline sidebar labels and the SVG milestone visualization with dynamic streams, milestones, gridlines, and NOW indicator.

**Parameter:** `[Parameter] public DashboardData Data { get; set; }`

**Visual Reference:** `.tl-area` section in `OriginalDesignConcept.html`

**Sub-sections:**

**Left Sidebar (Stream Labels):**
- 230px-wide flex column listing each stream from `Data.MilestoneStreams`
- Each label shows stream ID (e.g., "M1") in `stream.Color` and stream name in `#444`
- Uses `@foreach` over `Data.MilestoneStreams`

**Right SVG Area:**
- `<svg width="1560" height="185" style="overflow:visible">`
- SVG `<defs>` with drop shadow filter
- Month gridlines: computed from `Data.TimelineStart` and `Data.TimelineEnd`, spaced evenly across 1560px
- Stream horizontal lines: one per stream, y-position computed by `GetStreamY(index, totalStreams)` — distributes streams evenly between y=42 and y=154 (for 3 streams) or adapts for more/fewer
- Milestone markers: rendered per milestone type (`checkpoint` → circle, `poc` → gold diamond, `production` → green diamond, `dot` → small filled circle)
- Milestone labels: `<text>` positioned above or below the stream line based on `milestone.LabelPosition` (default: above)
- NOW line: dashed red vertical line at x-position computed from `Data.CurrentDate`

**Code-Behind Helper Methods:**

```csharp
@code {
    private const double SvgWidth = 1560.0;
    private const double SvgHeight = 185.0;

    private double DateToX(string dateStr)
    {
        var date = DateTime.Parse(dateStr);
        var start = DateTime.Parse(Data.TimelineStart);
        var end = DateTime.Parse(Data.TimelineEnd);
        var totalDays = (end - start).TotalDays;
        var elapsed = (date - start).TotalDays;
        return Math.Round(elapsed / totalDays * SvgWidth, 1);
    }

    private double GetStreamY(int index, int total)
    {
        // First stream at y=42, last at y=154, evenly spaced
        if (total <= 1) return SvgHeight / 2;
        var spacing = (154.0 - 42.0) / (total - 1);
        return 42.0 + index * spacing;
    }

    private string DiamondPoints(double cx, double cy, double r)
    {
        return $"{cx},{cy - r} {cx + r},{cy} {cx},{cy + r} {cx - r},{cy}";
    }

    private double NowX => DateToX(Data.CurrentDate);

    private List<(double x, string label)> MonthGridlines()
    {
        var start = DateTime.Parse(Data.TimelineStart);
        var end = DateTime.Parse(Data.TimelineEnd);
        var lines = new List<(double, string)>();
        var current = new DateTime(start.Year, start.Month, 1);
        while (current <= end)
        {
            lines.Add((DateToX(current.ToString("yyyy-MM-dd")),
                        current.ToString("MMM")));
            current = current.AddMonths(1);
        }
        return lines;
    }
}
```

### Component 8: `Components/HeatmapGrid.razor` — CSS Grid Status Matrix

**Responsibility:** Render the monthly execution heatmap as a CSS Grid with dynamic columns and color-coded category rows.

**Parameter:** `[Parameter] public DashboardData Data { get; set; }`

**Visual Reference:** `.hm-wrap` / `.hm-grid` section in `OriginalDesignConcept.html`

**Markup Structure:**
- Section title: `<div class="hm-title">` with static text
- Grid container: `<div class="hm-grid" style="grid-template-columns: 160px repeat(@Data.Heatmap.Columns.Count, 1fr);">`
- Corner cell: "STATUS"
- Column headers: `@foreach` over `Data.Heatmap.Columns`, applying `.apr-hdr` class when column index matches `Data.Heatmap.CurrentColumnIndex`
- Four category rows, each with:
  - Row header with category-specific CSS class (`.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr`)
  - Data cells: `@foreach` over columns, lookup items from `row.Items[columnName]`, render as `.it` divs with bullet pseudo-elements
  - Empty cells: render a muted dash `<span style="color:#AAA">-</span>`
  - Current month cells: append `.apr` class (or dynamically: category-specific current-month class)

**Category Configuration (code-behind):**

```csharp
@code {
    private record CategoryConfig(
        string Key, string Label, string HeaderClass, string CellClass, string CurrentClass);

    private static readonly CategoryConfig[] Categories = new[]
    {
        new CategoryConfig("shipped",    "✅ Shipped",      "ship-hdr",  "ship-cell",  "apr"),
        new CategoryConfig("inProgress", "🔄 In Progress",  "prog-hdr",  "prog-cell",  "apr"),
        new CategoryConfig("carryover",  "⏳ Carryover",    "carry-hdr", "carry-cell", "apr"),
        new CategoryConfig("blockers",   "🚫 Blockers",     "block-hdr", "block-cell", "apr"),
    };

    private HeatmapRow? GetRow(string category) =>
        Data.Heatmap.Rows.FirstOrDefault(r =>
            string.Equals(r.Category, category, StringComparison.OrdinalIgnoreCase));

    private bool IsCurrentMonth(int colIndex) =>
        colIndex == Data.Heatmap.CurrentColumnIndex;
}
```

### Component 9: `wwwroot/css/dashboard.css` — Stylesheet

**Responsibility:** All visual styling for the dashboard. Ported directly from `OriginalDesignConcept.html` `<style>` block, enhanced with CSS custom properties.

**Approach:**
1. Copy the original CSS verbatim as the baseline
2. Replace hardcoded colors with CSS custom properties where it aids theming
3. Keep the dynamic `grid-template-columns` value in inline styles on the component (since it depends on data)
4. Total target: under 150 lines

**CSS Custom Properties (`:root`):**
```css
:root {
    --shipped-accent: #34A853;
    --shipped-bg: #F0FBF0;
    --shipped-bg-current: #D8F2DA;
    --shipped-header: #E8F5E9;
    --shipped-text: #1B7A28;
    --progress-accent: #0078D4;
    --progress-bg: #EEF4FE;
    --progress-bg-current: #DAE8FB;
    --progress-header: #E3F2FD;
    --progress-text: #1565C0;
    --carryover-accent: #F4B400;
    --carryover-bg: #FFFDE7;
    --carryover-bg-current: #FFF0B0;
    --carryover-header: #FFF8E1;
    --carryover-text: #B45309;
    --blocker-accent: #EA4335;
    --blocker-bg: #FFF5F5;
    --blocker-bg-current: #FFE4E4;
    --blocker-header: #FEF2F2;
    --blocker-text: #991B1B;
    --header-bg: #F5F5F5;
    --current-month-header: #FFF0D0;
    --current-month-text: #C07700;
    --text-primary: #111;
    --text-secondary: #666;
    --text-muted: #888;
    --border: #E0E0E0;
    --border-heavy: #CCC;
}
```

### Component 10: `wwwroot/data/data.json` — Data Source

**Responsibility:** Single source of truth for all dashboard content.

**Schema:** Matches the `DashboardData` record hierarchy exactly. See Data Model section.

### Component 11: `wwwroot/data/data.sample.json` — Documented Template

**Responsibility:** Provide a fully commented example showing every field, its type, valid values, and purpose. Users copy this to `data.json` and edit.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Startup                       │
│                                                                   │
│  Program.cs                                                       │
│    ├── builder.Services.AddSingleton<DashboardDataService>()     │
│    ├── app = builder.Build()                                      │
│    ├── app.Services.GetRequiredService<DashboardDataService>()   │
│    │     └── LoadAsync()                                          │
│    │           ├── File.ReadAllTextAsync("wwwroot/data/data.json")│
│    │           ├── JsonSerializer.Deserialize<DashboardData>()    │
│    │           └── Validate required fields                       │
│    └── app.Run()                                                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                     HTTP Request (Browser → Kestrel)             │
│                                                                   │
│  GET / → Blazor Server Hub → Dashboard.razor                     │
│    │                                                              │
│    ├── @inject DashboardDataService service                      │
│    │                                                              │
│    ├── if (service.HasError)                                     │
│    │     └── Render error panel                                  │
│    │                                                              │
│    └── else                                                      │
│          ├── <DashboardHeader Data="@service.Data" />            │
│          │     └── Renders: title, subtitle, backlog link, legend│
│          │                                                        │
│          ├── <TimelineSection Data="@service.Data" />            │
│          │     ├── Stream sidebar labels                          │
│          │     └── SVG: gridlines, stream lines, milestones, NOW │
│          │                                                        │
│          └── <HeatmapGrid Data="@service.Data" />                │
│                ├── Section title                                  │
│                ├── Column headers (with current-month highlight)  │
│                └── 4 category rows × N month columns              │
└─────────────────────────────────────────────────────────────────┘
```

### Communication Patterns

| From | To | Mechanism | Data |
|------|----|-----------|------|
| `Program.cs` | `DashboardDataService` | DI (Singleton registration) | Service instance |
| `DashboardDataService` | Filesystem | `File.ReadAllTextAsync` | Raw JSON string |
| `DashboardDataService` | `DashboardData` | `JsonSerializer.Deserialize` | Typed record |
| `Dashboard.razor` | `DashboardDataService` | `@inject` | `Data` property, `HasError`, `ErrorMessage` |
| `Dashboard.razor` | Child components | `[Parameter]` binding | `DashboardData` record |
| Child components | CSS | CSS class names | Visual styling |
| Browser | Kestrel | SignalR WebSocket | Blazor Server interactivity (minimal — static render) |

### Rendering Pipeline

1. Browser connects to `https://localhost:5001`
2. Blazor Server establishes SignalR connection
3. `Dashboard.razor` renders on the server, producing complete HTML
4. HTML is sent to browser via SignalR
5. Browser renders the page with static CSS — **no further server interaction** unless the user triggers a re-render
6. The page is effectively static after initial load (no interactive elements in MVP)

---

## Data Model

### Entity: `DashboardData` (Root)

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `title` | `string` | Yes | Project title displayed in header (e.g., "Privacy Automation Release Roadmap") |
| `subtitle` | `string` | Yes | Team/workstream/date line below title |
| `backlogUrl` | `string` | Yes | URL for the "↗ ADO Backlog" link |
| `currentDate` | `string` (ISO date) | Yes | Determines NOW line position and "Now (Mon YYYY)" legend text |
| `timelineStart` | `string` (ISO date) | Yes | Left edge of the timeline (e.g., "2026-01-01") |
| `timelineEnd` | `string` (ISO date) | Yes | Right edge of the timeline (e.g., "2026-06-30") |
| `milestoneStreams` | `MilestoneStream[]` | Yes | Array of 1–5 milestone streams |
| `heatmap` | `HeatmapData` | Yes | Heatmap configuration and data |

### Entity: `MilestoneStream`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `string` | Yes | Stream identifier (e.g., "M1", "M2") |
| `name` | `string` | Yes | Stream display name (e.g., "Chatbot & MS Role") |
| `color` | `string` (hex) | Yes | Stream accent color for line and labels |
| `milestones` | `Milestone[]` | Yes | Array of milestone markers on this stream |

### Entity: `Milestone`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `date` | `string` (ISO date) | Yes | Position on the timeline |
| `label` | `string` | Yes | Text displayed near the marker |
| `type` | `string` (enum) | Yes | One of: `"checkpoint"`, `"poc"`, `"production"`, `"dot"` |
| `labelPosition` | `string?` | No | `"above"` (default) or `"below"` — controls label placement |

**Milestone Type Rendering:**

| Type | SVG Element | Fill | Stroke | Size |
|------|-------------|------|--------|------|
| `checkpoint` | `<circle>` | `white` | Stream color or `#888` | r=5–7 |
| `poc` | `<polygon>` (diamond) | `#F4B400` | None (drop shadow) | 11px radius |
| `production` | `<polygon>` (diamond) | `#34A853` | None (drop shadow) | 11px radius |
| `dot` | `<circle>` | `#999` | None | r=4 |

### Entity: `HeatmapData`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `columns` | `string[]` | Yes | Month labels for column headers (e.g., `["Jan","Feb","Mar","Apr"]`) |
| `currentColumnIndex` | `int` | Yes | Zero-based index of the current month (gets amber highlight) |
| `rows` | `HeatmapRow[]` | Yes | Exactly 4 rows: shipped, inProgress, carryover, blockers |

### Entity: `HeatmapRow`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `category` | `string` | Yes | One of: `"shipped"`, `"inProgress"`, `"carryover"`, `"blockers"` |
| `items` | `Dictionary<string, string[]>` | Yes | Keys are column names, values are arrays of item display names |

### Storage

- **Format:** Single JSON file (`wwwroot/data/data.json`)
- **Size:** Typically 2–10 KB, maximum expected ~50 KB
- **Persistence:** Filesystem only — no database, no in-memory state beyond the cached deserialized object
- **Caching:** `DashboardDataService` reads once at startup and holds the `DashboardData` record in memory for the lifetime of the process

### Entity Relationship Diagram

```
DashboardData (1)
  ├── MilestoneStream (1..5)
  │     └── Milestone (0..N)
  └── HeatmapData (1)
        └── HeatmapRow (4, fixed)
              └── Items: Dict<string, string[]>
```

---

## API Contracts

This application has **no REST API**. There are no controllers, no API endpoints, and no programmatic interfaces beyond the internal C# component contracts.

### Internal Component Contracts

**DashboardDataService → Components:**

```csharp
// Service contract (consumed by Dashboard.razor via @inject)
public class DashboardDataService
{
    public DashboardData? Data { get; }       // null if load failed
    public string? ErrorMessage { get; }       // null if load succeeded
    public bool HasError => Data is null;
    public async Task LoadAsync();             // called once at startup
}
```

**Component Parameter Contracts:**

```csharp
// All child components accept the same parameter
[Parameter] public required DashboardData Data { get; set; }
```

### Error Handling Contract

| Condition | Behavior | User-Visible Output |
|-----------|----------|-------------------|
| `data.json` missing | `DashboardDataService.HasError = true` | Error panel: "data.json not found..." |
| `data.json` malformed JSON | `DashboardDataService.HasError = true` | Error panel: "data.json is malformed: {details}" |
| Required field null after deserialization | `DashboardDataService.HasError = true` | Error panel: "Missing required field '{fieldName}'" |
| Valid `data.json` | `DashboardDataService.Data` populated | Full dashboard renders |
| Unexpected exception | `DashboardDataService.HasError = true` | Error panel: "Unexpected error: {message}" |

### Static File Serving

| URL Path | File | Content-Type |
|----------|------|-------------|
| `/css/dashboard.css` | `wwwroot/css/dashboard.css` | `text/css` |
| `/data/data.json` | `wwwroot/data/data.json` | `application/json` |
| `/_framework/blazor.server.js` | (Blazor framework) | `application/javascript` |

---

## Infrastructure Requirements

### Runtime Requirements

| Requirement | Specification |
|-------------|--------------|
| **.NET SDK** | .NET 8.0.x LTS |
| **OS** | Windows 10/11 |
| **Browser** | Chrome (latest) or Edge (latest) |
| **Display** | 1920×1080 at 100% scaling |
| **Font** | Segoe UI (pre-installed on Windows) |
| **Network** | Localhost only — no internet required at runtime |
| **Disk** | < 50 MB for published output |
| **RAM** | < 100 MB RSS for Kestrel process |

### Hosting

| Component | Technology | Configuration |
|-----------|-----------|---------------|
| **Web Server** | Kestrel (built into .NET 8) | Binds to `https://localhost:5001` by default |
| **Reverse Proxy** | None | Not needed for local-only use |
| **Process Manager** | None | Manual `dotnet run` / `dotnet watch` |
| **HTTPS** | .NET dev certificate | `dotnet dev-certs https --trust` (one-time setup) |

### Networking

- Kestrel binds to `localhost` only (default Blazor Server behavior)
- No inbound firewall rules needed
- No outbound network calls
- SignalR WebSocket connection is localhost-only

### Storage

- `wwwroot/data/data.json` — user-editable data file (2–50 KB)
- `wwwroot/css/dashboard.css` — static stylesheet (~150 lines)
- No database, no temp files, no log files

### CI/CD

**Not required.** This is a local developer tool. No build pipeline, no deployment automation, no artifact publishing.

For optional future CI:
- `dotnet build` validates compilation
- `dotnet test` runs optional xUnit tests
- No Docker, no container registry, no deployment targets

### Development Workflow

```
# First time setup
dotnet dev-certs https --trust

# Development (with hot-reload)
dotnet watch --project ReportingDashboard

# Production-like run
dotnet run --project ReportingDashboard

# Publish for sharing
dotnet publish -c Release -o ./publish
```

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **Framework** | Blazor Server (.NET 8 LTS) | Mandated by project constraints. Provides hot-reload, C# type safety, component model. Minimal overhead for single-page app. |
| **CSS Layout** | CSS Grid + Flexbox (native) | Direct port from `OriginalDesignConcept.html`. No abstraction layer needed. |
| **Timeline Rendering** | Inline SVG via Razor | The original design uses ~20 SVG primitives. Hand-crafted SVG in Razor gives pixel-perfect control. Charting libraries (Radzen, MudBlazor, ApexCharts) would add 2MB+ of dependencies and fight the custom design. |
| **Heatmap Rendering** | CSS Grid via Razor | `grid-template-columns: 160px repeat(N, 1fr)` adapts to dynamic column count. No table or charting library needed. |
| **JSON Parsing** | `System.Text.Json` (built-in) | Zero-dependency JSON deserialization. CamelCase policy matches JSON conventions. |
| **Data Modeling** | C# Records | Immutable, concise, built-in equality. Perfect for configuration data. |
| **Dependency Injection** | Built-in .NET DI | Singleton registration for `DashboardDataService`. No IoC container needed. |
| **CSS Architecture** | Single custom CSS file with custom properties | Ported directly from the design reference. Custom properties enable future theming. No CSS framework (Bootstrap, Tailwind, MUI) — they add bloat and fight the pixel-perfect design. |
| **JavaScript** | None | All rendering is Razor + CSS + SVG. No JS interop, no JS files beyond Blazor's framework script. |
| **Component Library** | None | MudBlazor, Radzen, etc. are overkill for one page with ~100 lines of CSS. The design is fully custom. |
| **Testing** | xUnit (optional) | Trivial deserialization tests. bUnit for component tests if desired. Playwright for visual regression (Phase 3). |
| **Responsive Design** | None (fixed 1920×1080) | The explicit goal is PowerPoint screenshots at a known resolution. Responsive breakpoints add complexity with zero benefit. |

### Alternatives Considered and Rejected

| Alternative | Reason for Rejection |
|-------------|---------------------|
| **Static HTML + build script** | Would avoid Blazor overhead, but stack decision is final. Blazor provides hot-reload and type safety. |
| **Blazor WebAssembly** | Larger download, longer initial load, no benefit for localhost-only use. |
| **MudBlazor** | 2MB+ CSS/JS, imposes its own design language, would spend more time overriding defaults than building from scratch. |
| **Chart.js / ApexCharts** | Requires JS interop, limits pixel-perfect control, overkill for ~20 SVG primitives. |
| **Bootstrap / Tailwind** | Conflicts with the custom pixel-perfect design. Utility classes would obscure the direct CSS port. |

---

## Security Considerations

### Authentication & Authorization

**None.** Explicitly out of scope. No middleware, no login, no tokens, no RBAC.

**Rationale:** This is a single-user, local-only tool. Adding authentication would violate the "zero-config launch" business goal.

**Future consideration:** If ever deployed on a shared server, Kestrel's default `localhost` binding provides network-level isolation. For multi-user access, add a simple reverse proxy with auth rather than modifying the application.

### Data Protection

| Concern | Mitigation |
|---------|-----------|
| Sensitive project data in `data.json` | Filesystem permissions (user-level). No encryption needed for local files. |
| PII in dashboard | Not expected. Data model contains project names and milestone dates only. Document in `data.sample.json` that PII should not be included. |
| Secrets/credentials | No secrets in `data.json`. No API keys, no connection strings. Document this constraint. |
| Data in transit | HTTPS via .NET dev certificate for localhost. Self-signed is sufficient. |

### Input Validation

| Input | Validation | Location |
|-------|-----------|----------|
| `data.json` content | JSON schema validation via C# deserialization + null checks on required fields | `DashboardDataService.LoadAsync()` |
| `currentDate`, `timelineStart`, `timelineEnd` | `DateTime.TryParse` — if invalid, set error message rather than crash | `DashboardDataService.LoadAsync()` |
| `currentColumnIndex` | Bounds check: must be `>= 0` and `< columns.Count` | `HeatmapGrid.razor` code-behind |
| `milestone.type` | Validated against known set (`checkpoint`, `poc`, `production`, `dot`); unknown types render as checkpoint (graceful fallback) | `TimelineSection.razor` code-behind |
| Heatmap `items` keys | If a column name in `items` doesn't match a column in `columns`, it's silently ignored | `HeatmapGrid.razor` |

### Supply Chain

- Zero third-party NuGet packages beyond the default Blazor Server template
- No npm packages, no CDN references, no external scripts
- The only external dependency is the .NET 8 SDK itself (Microsoft-published, LTS)

### Content Security

- No user-generated content is rendered (all data comes from a local file edited by the same user)
- No `@Html.Raw()` usage — all data is rendered via Blazor's built-in encoding
- No XSS vector since there's no user input mechanism

---

## Scaling Strategy

### Current Scale Target

| Dimension | Target |
|-----------|--------|
| **Concurrent users** | 1 (single browser tab) |
| **Data volume** | 1 JSON file, < 50 KB |
| **Page count** | 1 |
| **Render frequency** | On-demand (page load / manual refresh) |

### Why Scaling Is Not a Concern

This is a local developer tool, not a production service. It serves one user on localhost. There is no scaling requirement, no load balancer, no horizontal scaling, and no database connection pool to tune.

### If Scaling Were Ever Needed

| Scenario | Approach |
|----------|---------|
| **Multiple projects** | Add URL parameter `?project=alpha` → service reads `data-alpha.json`. No architectural change needed. |
| **Multiple users (team server)** | Deploy as a published executable on a shared Windows server. Kestrel handles a few dozen concurrent SignalR connections without issue. |
| **100+ concurrent users** | Switch to Blazor Static SSR (server-side rendering without SignalR) or pre-render to static HTML. Eliminates per-connection WebSocket overhead. |
| **Automated screenshot generation** | Add Playwright script that iterates over `data-*.json` files and saves PNGs. No server scaling needed — runs locally. |

### Performance Budgets

| Operation | Budget | Achievability |
|-----------|--------|--------------|
| JSON read + deserialize | < 50ms | Trivial for < 50 KB file |
| Server-side Razor render | < 100ms | Simple component tree, no database queries |
| SignalR initial payload to browser | < 200ms | Localhost latency is negligible |
| Full page load (cold start) | < 2 seconds | Kestrel startup + first render |
| Hot-reload refresh | < 1 second | `dotnet watch` recompiles only changed files |

---

## Risks & Mitigations

### Risk 1: SVG Rendering Precision Drift

**Severity:** Medium
**Probability:** Medium
**Impact:** Timeline milestones appear at wrong positions; visual fidelity fails side-by-side comparison.

**Description:** The original HTML uses hardcoded SVG pixel coordinates (e.g., `cx="745"` for a milestone). The Blazor version computes positions dynamically via `DateToX()` using floating-point math. Rounding errors or off-by-one issues could shift markers by several pixels.

**Mitigation:**
1. `DateToX()` uses `Math.Round(value, 1)` to produce clean SVG coordinates
2. The reference design's coordinate system is reverse-engineered: 260px per month across a Jan–Jun timeline = 1560px / 6 months. The `DateToX()` linear interpolation uses the same scale.
3. **Validation:** Open `OriginalDesignConcept.html` and the Blazor app side-by-side in Chrome DevTools, compare SVG element positions numerically
4. **Phase 3:** Playwright visual regression test captures both pages at 1920×1080 and diffs them

### Risk 2: `data.json` Schema Errors at Runtime

**Severity:** Medium
**Probability:** High (users will make typos)
**Impact:** Dashboard crashes or renders incorrectly instead of showing a helpful error.

**Description:** Since `data.json` is hand-edited, users will inevitably introduce syntax errors, missing fields, wrong types, or mismatched column names.

**Mitigation:**
1. `DashboardDataService` wraps all deserialization in try-catch and sets human-readable `ErrorMessage`
2. Post-deserialization validation checks: `Title` not null, `MilestoneStreams` not empty, `Heatmap.Columns` not empty, `CurrentColumnIndex` in bounds
3. `data.sample.json` provided with every field documented and example values
4. Graceful fallbacks: unknown milestone types render as checkpoints; missing heatmap items render as dashes

### Risk 3: Blazor Server SignalR Overhead

**Severity:** Low
**Probability:** Low
**Impact:** Unnecessary WebSocket connection for what is effectively a static page.

**Description:** Blazor Server maintains a persistent SignalR WebSocket even though the dashboard has no interactive elements (no buttons, no forms, no event handlers in MVP).

**Mitigation:**
1. Accept it — for a single-user localhost tool, the overhead is negligible (~50 KB memory per connection)
2. The page renders fully on first load; the SignalR connection exists but carries no traffic after initial render
3. If this ever becomes a concern, migrate to Blazor Static SSR (`@rendermode` attribute) in .NET 8, which pre-renders HTML without SignalR

### Risk 4: CSS Port Introduces Visual Differences

**Severity:** Medium
**Probability:** Low
**Impact:** Heatmap grid or header layout doesn't match the reference pixel-for-pixel.

**Description:** Blazor's default template includes its own CSS (`app.css`, `bootstrap.min.css`) that could conflict with the custom dashboard CSS.

**Mitigation:**
1. **Remove all default Blazor CSS.** Delete `app.css`, remove Bootstrap references from `_Host.cshtml` / `App.razor`
2. Reference only `dashboard.css` in the `<head>`
3. Use a CSS reset (`* { margin: 0; padding: 0; box-sizing: border-box; }`) as the first rule, matching the original HTML
4. Set `body` to `width: 1920px; height: 1080px; overflow: hidden;` explicitly

### Risk 5: Hot-Reload Doesn't Refresh Data

**Severity:** Low
**Probability:** Medium
**Impact:** User edits `data.json`, expects to see changes, but the Singleton service has cached the old data.

**Description:** `DashboardDataService` reads `data.json` once at startup and caches it. `dotnet watch` hot-reload recompiles Razor/CSS changes but does not re-run `Program.cs`, so the service retains stale data.

**Mitigation:**
1. **MVP:** Document that `data.json` changes require an app restart (`Ctrl+C` + `dotnet run`). This is acceptable for a < 2-second startup time.
2. **Phase 2:** Add `FileSystemWatcher` on `data.json`. On change, re-read and re-deserialize. Notify connected Blazor circuits via an injected event or `IHostedService` pattern.

### Risk 6: Font Rendering Differences Across Machines

**Severity:** Low
**Probability:** Low
**Impact:** Text wrapping or spacing differs slightly between developer machines, causing layout overflow.

**Description:** Segoe UI is a Windows system font and should render identically on all Windows 10/11 machines. However, font rendering can vary by Windows version, ClearType settings, or display scaling.

**Mitigation:**
1. Target only Windows 10/11 at 100% display scaling (documented requirement)
2. Use the same font stack as the original: `'Segoe UI', Arial, sans-serif`
3. Keep text content short (< 60 chars per item, documented in `data.sample.json`)
4. `overflow: hidden` on cells prevents layout breakage even if text wraps unexpectedly

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component with its CSS layout strategy, data bindings, and interactions.

### Section → Component Mapping

| Visual Section (from design) | Blazor Component | CSS Class(es) | Layout Strategy |
|------------------------------|-----------------|---------------|----------------|
| Header bar (`.hdr`) | `DashboardHeader.razor` | `.hdr`, `.sub` | Flexbox row, `justify-content: space-between` |
| Timeline area (`.tl-area`) | `TimelineSection.razor` | `.tl-area`, `.tl-svg-box` | Flexbox row; left sidebar (230px fixed) + SVG area (flex: 1) |
| Heatmap wrapper (`.hm-wrap`) | `HeatmapGrid.razor` | `.hm-wrap`, `.hm-grid`, `.hm-title` | Flex column (wrapper) → CSS Grid (grid) |
| Full page layout | `MainLayout.razor` + `Dashboard.razor` | `body` styles | Flex column, fixed 1920×1080, `overflow: hidden` |

### Component: `DashboardHeader.razor`

**Design Section:** `.hdr` (header bar, ~50px tall)

**CSS Layout:**
```css
.hdr {
    padding: 12px 44px 10px;
    border-bottom: 1px solid var(--border);
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-shrink: 0;
}
```

**Data Bindings:**

| Element | Binding | Source |
|---------|---------|--------|
| `<h1>` text | `@Data.Title` | `data.json → title` |
| `<a>` href | `@Data.BacklogUrl` | `data.json → backlogUrl` |
| `.sub` text | `@Data.Subtitle` | `data.json → subtitle` |
| Legend "Now" label | Computed from `@Data.CurrentDate` | Formatted as "Now (Mon YYYY)" |

**Interactions:** Click on backlog link navigates to external URL (standard `<a>` behavior).

**Sub-elements:**
- **Left block:** Title (`h1`, 24px, bold) + subtitle (`div.sub`, 12px, `#888`)
- **Right block:** Four legend items in a horizontal flex with `gap: 22px`
  - PoC: 12×12px gold diamond (CSS `transform: rotate(45deg)`)
  - Production: 12×12px green diamond
  - Checkpoint: 8×8px gray circle
  - Now: 2×14px red vertical bar + dynamic label

### Component: `TimelineSection.razor`

**Design Section:** `.tl-area` (196px tall, `#FAFAFA` background)

**CSS Layout:**
```css
.tl-area {
    display: flex;
    align-items: stretch;
    padding: 6px 44px 0;
    flex-shrink: 0;
    height: 196px;
    border-bottom: 2px solid #E8E8E8;
    background: #FAFAFA;
}
```

**Sub-components (rendered inline, not separate files):**

**Stream Sidebar (left, 230px):**
- Flex column, `justify-content: space-around`
- `@foreach (var stream in Data.MilestoneStreams)` renders stream ID in `stream.Color` and name in `#444`
- Dynamic: adapts to 1–5 streams automatically

**SVG Canvas (right, flex: 1):**
- `<svg width="1560" height="185">`
- **Month gridlines:** Vertical lines computed from `timelineStart`/`timelineEnd`, spaced by `DateToX(firstOfMonth)`
- **Stream lines:** Horizontal lines at computed Y positions, color from `stream.Color`
- **Milestones:** Markers rendered per type (see Data Model → Milestone Type Rendering table)
- **NOW line:** Dashed red vertical at `DateToX(Data.CurrentDate)`

**Data Bindings:**

| SVG Element | Binding | Source |
|-------------|---------|--------|
| Stream line y-position | `GetStreamY(index, total)` | Computed from stream index |
| Stream line color | `stream.Color` | `data.json → milestoneStreams[].color` |
| Milestone x-position | `DateToX(milestone.Date)` | `data.json → milestoneStreams[].milestones[].date` |
| Milestone label text | `milestone.Label` | `data.json → milestoneStreams[].milestones[].label` |
| Milestone marker type | `milestone.Type` | `data.json → milestoneStreams[].milestones[].type` |
| NOW line x-position | `DateToX(Data.CurrentDate)` | `data.json → currentDate` |
| Month gridline positions | `DateToX(firstOfMonth)` | Computed from `timelineStart`/`timelineEnd` |

**Interactions:** None in MVP. Phase 2: CSS-only tooltips on hover over milestone diamonds.

### Component: `HeatmapGrid.razor`

**Design Section:** `.hm-wrap` (fills remaining viewport height)

**CSS Layout:**
```css
.hm-wrap {
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;
    padding: 10px 44px 10px;
}

.hm-grid {
    flex: 1;
    min-height: 0;
    display: grid;
    /* grid-template-columns set via inline style: 160px repeat(N, 1fr) */
    grid-template-rows: 36px repeat(4, 1fr);
    border: 1px solid var(--border);
}
```

**Dynamic CSS (inline style on `.hm-grid`):**
```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Data.Heatmap.Columns.Count, 1fr);">
```

**Data Bindings:**

| Element | Binding | Source |
|---------|---------|--------|
| Column count | `Data.Heatmap.Columns.Count` | `data.json → heatmap.columns[]` |
| Column header text | `@column` | `data.json → heatmap.columns[i]` |
| Current month highlight | `i == Data.Heatmap.CurrentColumnIndex` | `data.json → heatmap.currentColumnIndex` |
| Row category | `row.Category` | `data.json → heatmap.rows[].category` |
| Cell items | `row.Items[columnName]` | `data.json → heatmap.rows[].items[columnName]` |
| Empty cell | Items array is empty → render dash | Absence of items for a column |

**Grid Cell Rendering Logic:**
```razor
@foreach (var category in Categories)
{
    var row = GetRow(category.Key);
    <div class="hm-row-hdr @category.HeaderClass">@category.Label</div>
    @for (int i = 0; i < Data.Heatmap.Columns.Count; i++)
    {
        var col = Data.Heatmap.Columns[i];
        var isCurrent = IsCurrentMonth(i);
        var cellClass = $"{category.CellClass}{(isCurrent ? " " + category.CurrentClass : "")}";
        var items = row?.Items.GetValueOrDefault(col) ?? new List<string>();
        <div class="hm-cell @cellClass">
            @if (items.Count == 0)
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

**Interactions:** None. Pure visual output. Cell overflow is clipped via `overflow: hidden`.

### Component: `MainLayout.razor`

**Design Section:** Outermost container (`body` equivalent)

**CSS Layout:**
```css
body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
    background: #FFFFFF;
    font-family: 'Segoe UI', Arial, sans-serif;
    color: var(--text-primary);
    display: flex;
    flex-direction: column;
}
```

**Markup:**
```razor
@inherits LayoutComponentBase
@Body
```

**Data Bindings:** None — pure structural wrapper.

**Interactions:** None.

### Visual Fidelity Checklist

| Design Element | CSS Property | Value | Component |
|---------------|-------------|-------|-----------|
| Body fixed size | `width; height; overflow` | `1920px; 1080px; hidden` | `dashboard.css` (body) |
| Horizontal padding | `padding-left; padding-right` | `44px` | `.hdr`, `.tl-area`, `.hm-wrap` |
| Title font | `font-size; font-weight` | `24px; 700` | `.hdr h1` |
| Link color | `color` | `#0078D4` | `a` |
| Subtitle | `font-size; color` | `12px; #888` | `.sub` |
| Timeline height | `height` | `196px` | `.tl-area` |
| Timeline background | `background` | `#FAFAFA` | `.tl-area` |
| Grid header row height | `grid-template-rows` | `36px repeat(4, 1fr)` | `.hm-grid` |
| Current month header | `background; color` | `#FFF0D0; #C07700` | `.hm-col-hdr.apr-hdr` |
| Cell bullet size | `width; height` | `6px; 6px` | `.hm-cell .it::before` |
| NOW line style | `stroke; stroke-dasharray` | `#EA4335; 5,3` | SVG inline |
| Drop shadow on diamonds | `filter` | `feDropShadow(0, 1, 1.5, 0.3)` | SVG `<defs>` filter |