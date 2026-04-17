# Architecture

**Document Version:** 1.0
**Date:** April 17, 2026
**Project:** Executive Project Reporting Dashboard
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure

---

## Overview & Goals

The Executive Project Reporting Dashboard is a single-page Blazor Server application that renders a pixel-perfect 1920×1080 executive status view from a local `data.json` file. The architecture is intentionally minimal: one data service reads a JSON file, deserializes it into strongly-typed records, and passes the data to a small tree of Razor components that render HTML, CSS Grid, and inline SVG. There is no database, no authentication, no API, and no JavaScript.

**Architecture Goals:**

1. **Pixel-perfect visual fidelity** — Match `OriginalDesignConcept.html` at 95%+ similarity via a direct CSS port and programmatic SVG generation.
2. **Data-driven rendering** — 100% of displayed content sourced from `data.json`; zero hardcoded strings in components.
3. **Minimal moving parts** — One `.sln`, one web project, one service class, one model file, ~8 Razor components, one CSS file. No patterns beyond what the problem demands.
4. **Sub-5-minute setup** — Clone → `dotnet run` → browser → screenshot. No configuration, no secrets, no infrastructure.
5. **Screenshot-ready output** — Fixed viewport, hidden Blazor chrome, no scrollbars. Ready for PowerPoint paste.

**Architectural Pattern:** Flat read-through pipeline. `data.json` → `DashboardDataService` → Razor component tree → Browser. No layers, no abstractions, no domain logic.

---

## System Components

### 1. `DashboardDataService` (Singleton Service)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Read `data.json` from disk, deserialize into `DashboardData`, cache in memory, surface errors |
| **Interfaces** | `Task<DashboardData> GetDashboardDataAsync()` — returns cached data or reads from disk on first call |
| **Dependencies** | `System.IO.File`, `System.Text.Json.JsonSerializer`, `IWebHostEnvironment` (for `ContentRootPath`) |
| **Data** | Holds a single `DashboardData?` instance and an `string? ErrorMessage` |
| **Error handling** | Catches `FileNotFoundException`, `JsonException`, and surfaces a user-friendly message string. Never throws to callers. |
| **Lifetime** | Registered as singleton in DI. Reads file once at first request. App restart required to pick up changes. |

```csharp
public class DashboardDataService
{
    private readonly string _dataFilePath;
    private DashboardData? _cachedData;
    public string? ErrorMessage { get; private set; }

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataFilePath = Path.Combine(env.ContentRootPath, "data.json");
    }

    public async Task<DashboardData?> GetDashboardDataAsync()
    {
        if (_cachedData is not null) return _cachedData;

        try
        {
            var json = await File.ReadAllTextAsync(_dataFilePath);
            _cachedData = JsonSerializer.Deserialize<DashboardData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            });

            if (_cachedData is null)
                ErrorMessage = "data.json deserialized to null. Check file contents.";
        }
        catch (FileNotFoundException)
        {
            ErrorMessage = "data.json not found. Place a data.json file in the project root.";
        }
        catch (JsonException ex)
        {
            ErrorMessage = $"Invalid JSON in data.json: {ex.Message}";
        }

        return _cachedData;
    }
}
```

### 2. Data Model Records (`Models/DashboardData.cs`)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Strongly-typed shape of `data.json` for compile-time safety and IDE support |
| **Interfaces** | Pure data records; no methods beyond what's needed for SVG math |
| **Dependencies** | None (POCO records using `System.Text.Json` attributes) |
| **Data** | `DashboardData` → `TimelineConfig` → `MilestoneTrack[]` → `MilestoneEvent[]`; `HeatmapConfig` → `HeatmapCategory` × 4 |

Full model specification in [Data Model](#data-model) section below.

### 3. Razor Component Tree

All components live under `Components/` and render pure HTML/SVG from `DashboardData` parameters. No component fetches its own data — the page-level `Dashboard.razor` injects the service and cascades data downward via `[Parameter]` properties.

#### 3a. `Dashboard.razor` (Page Component)

| Attribute | Detail |
|-----------|--------|
| **Route** | `@page "/"` |
| **Responsibility** | Inject `DashboardDataService`, call `GetDashboardDataAsync()` in `OnInitializedAsync`, render error state or pass data to child components |
| **Children** | `Header`, `Timeline`, `HeatmapGrid` |
| **Error state** | If `ErrorMessage` is set, renders a centered error panel instead of the dashboard |

#### 3b. `Header.razor`

| Attribute | Detail |
|-----------|--------|
| **Parameters** | `string Title`, `string Subtitle`, `string BacklogLink`, `string CurrentMonthLabel` |
| **Responsibility** | Render `.hdr` div: title with ADO backlog `<a>` link (`target="_blank"`), subtitle line, and legend row (4 indicator items) |
| **CSS classes** | `.hdr`, `.sub` |
| **Data bindings** | `@Title`, `@Subtitle`, `@BacklogLink` from `data.json`; `@CurrentMonthLabel` derived from `timeline.currentDate` or `heatmap.highlightColumn` |
| **Layout** | Flexbox row, `justify-content: space-between` |

#### 3c. `Timeline.razor`

| Attribute | Detail |
|-----------|--------|
| **Parameters** | `TimelineConfig Timeline` |
| **Responsibility** | Render `.tl-area` div containing (1) track label sidebar and (2) inline `<svg>` with month grid lines, track lines, milestone shapes, date labels, and NOW indicator |
| **CSS classes** | `.tl-area`, `.tl-svg-box` |
| **SVG constants** | Width: `1560`, Height: `185` (matching reference) |
| **Key logic** | `DateToX()` — linear interpolation from date range to pixel X position; `TrackY(int index, int count)` — evenly space tracks within 185px SVG height, starting at y=42 with equal spacing |
| **Shapes** | Checkpoint → `<circle>`, PoC → `<polygon>` (diamond, fill `#F4B400`), Production → `<polygon>` (diamond, fill `#34A853`), NOW → `<line>` (dashed red) |
| **Tooltips** | SVG `<title>` element inside each milestone shape for native browser tooltip on hover |

#### 3d. `HeatmapGrid.razor`

| Attribute | Detail |
|-----------|--------|
| **Parameters** | `HeatmapConfig Heatmap` |
| **Responsibility** | Render `.hm-wrap` div with section title and CSS Grid (`.hm-grid`). Renders header row (corner + month headers), then 4 status rows via inline rendering or child components. |
| **CSS classes** | `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.apr-hdr` |
| **Dynamic grid** | `grid-template-columns` set via inline `style` attribute: `160px repeat(@Heatmap.Columns.Count, 1fr)` |
| **Highlight logic** | Compares each column name to `Heatmap.HighlightColumn` to apply `.apr-hdr` and `.apr` CSS classes |
| **Children** | Renders `HeatmapRowHeader` and `HeatmapCell` for each of the 4 categories × N months |

#### 3e. `HeatmapRowHeader.razor`

| Attribute | Detail |
|-----------|--------|
| **Parameters** | `string Label`, `string CssClass` |
| **Responsibility** | Render `.hm-row-hdr` div with the category-specific CSS class (e.g., `.ship-hdr`, `.prog-hdr`) |
| **Output** | `<div class="hm-row-hdr @CssClass">@Label</div>` |

#### 3f. `HeatmapCell.razor`

| Attribute | Detail |
|-----------|--------|
| **Parameters** | `List<string> Items`, `string CellCssClass`, `bool IsHighlight` |
| **Responsibility** | Render `.hm-cell` div. If items are empty, render a gray dash `"-"`. Otherwise render each item as a `<div class="it">` with the category-colored bullet via CSS `::before`. |
| **Highlight** | Applies `.apr` class when `IsHighlight` is true for the current-month darker tint |

### 4. Layout & App Shell

#### `App.razor`
Root HTML shell: `<!DOCTYPE html>`, `<head>` with `<link>` to `dashboard.css`, `<body>` renders `<Routes>`. Includes `<HeadOutlet>` for Blazor. **Does not** include any Blazor interactive script tag in static SSR mode, or includes it minimally for Server interactivity.

#### `MainLayout.razor`
Minimal layout — `@Body` only. No `<NavMenu>`, no sidebar, no header chrome. The layout is a pass-through to let `Dashboard.razor` control the full viewport.

```razor
@inherits LayoutComponentBase
@Body
```

### 5. `dashboard.css` (Global Stylesheet)

| Attribute | Detail |
|-----------|--------|
| **Location** | `wwwroot/css/dashboard.css` |
| **Responsibility** | All visual styling for the dashboard. Direct port from `OriginalDesignConcept.html` `<style>` block. |
| **Strategy** | Single global file. No scoped CSS per component (unnecessary for a 1-page app). |
| **Key rules** | `body { width:1920px; height:1080px; overflow:hidden; }`, CSS custom properties for color tokens, `.components-reconnect-modal { display:none!important; }` to suppress Blazor reconnection UI |

### 6. `data.json` (Data File)

| Attribute | Detail |
|-----------|--------|
| **Location** | Project root (next to `.csproj`) |
| **Responsibility** | Single source of truth for all dashboard content |
| **Format** | JSON file matching the `DashboardData` record schema |
| **Shipped sample** | "Project Aurora" fictional data with 3 tracks and 4 months |

### 7. `Program.cs` (Host Configuration)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure minimal Blazor Server app, register `DashboardDataService` as singleton, map Razor components |
| **Lines of code** | ~15 lines |

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

## Component Interactions

### Data Flow (Single Request Path)

```
┌─────────────┐     ┌──────────────────────┐     ┌───────────────────┐
│  data.json   │────▶│  DashboardDataService │────▶│  Dashboard.razor   │
│  (flat file) │     │  (singleton, cached)  │     │  (page component)  │
└─────────────┘     └──────────────────────┘     └────────┬──────────┘
                                                          │ passes DashboardData
                                        ┌─────────────────┼─────────────────┐
                                        ▼                 ▼                 ▼
                                  Header.razor     Timeline.razor    HeatmapGrid.razor
                                  (title, sub,     (SVG timeline,    (CSS Grid,
                                   legend)          track labels)     status rows)
                                                                          │
                                                          ┌───────────────┼───────────────┐
                                                          ▼               ▼               ▼
                                                   HeatmapRowHeader  HeatmapCell     HeatmapCell
                                                   (row label)       (items list)    (items list)
```

### Communication Pattern

- **Unidirectional data flow:** `DashboardDataService` → `Dashboard.razor` → child components via `[Parameter]` properties. No callbacks, no events, no two-way binding.
- **No inter-component communication:** Components do not talk to each other. Each receives its data slice from the parent.
- **No SignalR usage:** Although Blazor Server establishes a SignalR connection, this app performs no interactive updates. The connection exists only because it's the mandated rendering mode. Consider static SSR (`@rendermode` attribute) to eliminate it entirely.

### Request Lifecycle

1. Browser requests `GET /`
2. Kestrel routes to `Dashboard.razor`
3. `OnInitializedAsync` calls `DashboardDataService.GetDashboardDataAsync()`
4. Service reads `data.json` (first request) or returns cached data (subsequent)
5. Component tree renders HTML/SVG to the response stream
6. Browser paints the page. Done.

---

## Data Model

### Entity Hierarchy

```
DashboardData (root)
├── Title: string
├── Subtitle: string
├── BacklogLink: string
├── Timeline: TimelineConfig
│   ├── StartDate: DateOnly
│   ├── EndDate: DateOnly
│   ├── CurrentDate: DateOnly
│   └── Tracks: List<MilestoneTrack>
│       ├── Id: string ("M1", "M2", ...)
│       ├── Label: string
│       ├── Color: string (hex)
│       └── Events: List<MilestoneEvent>
│           ├── Date: DateOnly
│           ├── Label: string
│           └── Type: MilestoneType (enum: Checkpoint | PoC | Production)
└── Heatmap: HeatmapConfig
    ├── Columns: List<string> (["Jan", "Feb", ...])
    ├── HighlightColumn: string ("Apr")
    ├── Shipped: HeatmapCategory
    ├── InProgress: HeatmapCategory
    ├── Carryover: HeatmapCategory
    └── Blockers: HeatmapCategory
        └── Items: Dictionary<string, List<string>>
            (key = column name, value = list of work item labels)
```

### C# Record Definitions

```csharp
using System.Text.Json.Serialization;

public record DashboardData
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string BacklogLink { get; init; }
    public required TimelineConfig Timeline { get; init; }
    public required HeatmapConfig Heatmap { get; init; }
}

public record TimelineConfig
{
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public DateOnly? CurrentDate { get; init; }  // nullable; defaults to DateTime.Now
    public required List<MilestoneTrack> Tracks { get; init; }
}

public record MilestoneTrack
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Color { get; init; }
    public required List<MilestoneEvent> Events { get; init; }
}

public record MilestoneEvent
{
    public required DateOnly Date { get; init; }
    public required string Label { get; init; }
    public required MilestoneType Type { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MilestoneType
{
    Checkpoint,
    PoC,
    Production
}

public record HeatmapConfig
{
    public required List<string> Columns { get; init; }
    public required string HighlightColumn { get; init; }
    public required HeatmapCategory Shipped { get; init; }
    public required HeatmapCategory InProgress { get; init; }
    public required HeatmapCategory Carryover { get; init; }
    public required HeatmapCategory Blockers { get; init; }
}

public record HeatmapCategory
{
    public required Dictionary<string, List<string>> Items { get; init; }
}
```

### Storage

| Aspect | Detail |
|--------|--------|
| **Storage type** | Flat JSON file on local filesystem |
| **Location** | `{ContentRootPath}/data.json` |
| **Access pattern** | Read once on first HTTP request, cached in memory for app lifetime |
| **Size limit** | Designed for ≤100KB (5 tracks, 12 months). No pagination needed. |
| **Schema versioning** | Use nullable properties with defaults for forward compatibility |
| **Relationships** | Pure containment hierarchy (no foreign keys, no references) |

### JSON ↔ C# Mapping Notes

| JSON field | C# property | Notes |
|------------|-------------|-------|
| `startDate` | `DateOnly StartDate` | System.Text.Json handles `DateOnly` natively in .NET 8 |
| `currentDate` | `DateOnly? CurrentDate` | Nullable; service defaults to `DateOnly.FromDateTime(DateTime.Now)` if null |
| `type` (in events) | `MilestoneType Type` | `JsonStringEnumConverter` maps `"Checkpoint"` → enum value |
| `inProgress` | `HeatmapCategory InProgress` | `PropertyNameCaseInsensitive = true` handles camelCase → PascalCase |

---

## API Contracts

This application exposes **no API endpoints**. It is a server-rendered web page with a single route.

### HTTP Routes

| Method | Path | Response | Notes |
|--------|------|----------|-------|
| `GET` | `/` | Full HTML page (dashboard) | Blazor Server-rendered. Returns complete HTML document. |
| `GET` | `/_blazor` | SignalR negotiation | Blazor framework internal. Not used by the dashboard logic. Hidden from user. |
| `GET` | `/css/dashboard.css` | Static CSS file | Served by `UseStaticFiles()` middleware |

### Error Responses

| Condition | Behavior |
|-----------|----------|
| `data.json` missing | Page renders a styled error panel: "Unable to load dashboard data. Please check that data.json exists in the project root and contains valid JSON." |
| `data.json` malformed | Page renders error panel with the `JsonException` message (e.g., "Invalid JSON at line 42, position 15") |
| Missing required field | `System.Text.Json` throws `JsonException` for missing `required` properties. Caught and displayed as above. |
| `data.json` valid but empty tracks | Page renders with an empty timeline area and empty heatmap. No crash. |

### Error Panel Markup

```html
<div style="display:flex;align-items:center;justify-content:center;
            width:1920px;height:1080px;font-family:'Segoe UI',Arial,sans-serif;">
    <div style="max-width:600px;padding:40px;border:2px solid #EA4335;
                border-radius:8px;background:#FEF2F2;color:#991B1B;">
        <h2 style="margin-bottom:12px;">Dashboard Error</h2>
        <p>@ErrorMessage</p>
    </div>
</div>
```

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|--------------|
| **Runtime** | .NET 8.0 SDK (8.0.x, any patch) |
| **Web server** | Kestrel (built into .NET 8, no IIS/Nginx) |
| **OS** | Windows 10/11 (primary); macOS/Linux (untested but should work) |
| **Browser** | Chrome 120+ or Edge 120+ for screenshot capture |
| **Resolution** | 1920×1080 display or Chrome DevTools device emulation |

### Hosting

| Aspect | Detail |
|--------|--------|
| **Binding** | `http://localhost:5000` (Kestrel default for development) |
| **Network** | Localhost only. No external network access required or desired. |
| **Process model** | Single `dotnet run` process. No daemon, no service, no container. |
| **Startup time** | < 3 seconds cold start, < 1 second warm |

### Storage

| Item | Location | Size |
|------|----------|------|
| `data.json` | Project root | < 100KB |
| Static assets (`dashboard.css`) | `wwwroot/css/` | < 20KB |
| .NET runtime | System-installed SDK | ~500MB (pre-existing) |
| Published app (self-contained) | `bin/publish/` | ~80MB |

### CI/CD

**None.** This is a local tool with no deployment pipeline. The build/test workflow is:

```bash
dotnet build              # Verify compilation
dotnet test               # Run unit tests
dotnet run                # Launch dashboard
```

**Optional:** Self-contained publish for distribution without SDK:
```bash
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
```

### Networking

- **Inbound:** HTTP on localhost:5000 (configurable via `appsettings.json` or `--urls` CLI arg)
- **Outbound:** None. The app makes zero network calls.
- **Firewall:** No ports need to be opened. Localhost binding is inaccessible from other machines.

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **UI Framework** | Blazor Server (.NET 8) | Mandated by project requirements. Provides server-side rendering with strongly-typed C# components. |
| **Rendering mode** | Static SSR preferred, Interactive Server fallback | Static SSR eliminates SignalR connection and reconnection UI artifacts. Use `@rendermode InteractiveServer` only if component lifecycle methods are needed. |
| **CSS approach** | Single global `dashboard.css` | The reference design is one page with cohesive styling. Scoped CSS per component adds complexity for zero benefit here. |
| **SVG rendering** | Raw inline SVG in Razor markup | The timeline SVG is ~60 elements. A charting library would add configuration overhead exceeding the SVG itself. Direct markup gives pixel-level control. |
| **JSON library** | `System.Text.Json` (built-in) | Ships with .NET 8. Handles `DateOnly`, enums, case-insensitive deserialization. No third-party package needed. |
| **Data storage** | `data.json` flat file | Data is small, read-only, manually curated, and benefits from being human-editable and diff-friendly. A database adds migrations and tooling for zero benefit. |
| **DI pattern** | Singleton service | Data is read once and cached. Singleton is the correct lifetime for an immutable, shared data cache. |
| **Testing** | xUnit + bUnit + FluentAssertions | Industry-standard .NET testing stack. bUnit enables testing Razor components without a browser. |
| **Third-party UI libs** | None (rejected MudBlazor, Radzen, Syncfusion) | The design is custom. Component libraries impose their own design system and make pixel-perfect matching harder, not easier. |
| **JavaScript** | None | The design requires no client-side interactivity. All rendering is server-side HTML/CSS/SVG. |
| **Fonts** | Segoe UI (system font) | Available on Windows by default. No web font loading, no FOUT, no external requests. Fallback: Arial. |

### Rejected Alternatives

| Alternative | Why Rejected |
|-------------|-------------|
| Plain HTML + JS | Mandated stack is Blazor. Also, plain HTML lacks strongly-typed data binding from JSON. |
| Blazor WebAssembly | Heavier client download, longer startup, no benefit for a local server-rendered page. |
| React/Angular/Vue | Outside mandated stack. |
| Chart.js / D3.js | Requires JavaScript interop. The SVG is simple enough to render directly in Razor. |
| SQLite / EF Core | Adds migrations, ORM, connection management. The data is a flat file with ~50 items. |
| MediatR / CQRS | Architectural patterns for complex domain logic. This app has zero domain logic. |

---

## UI Component Architecture

Each visual section from `OriginalDesignConcept.html` maps to a specific Blazor component:

### Header Section → `Header.razor`

| Design Element | CSS Layout | Data Bindings | Interactions |
|----------------|-----------|---------------|-------------|
| `.hdr` container | `display:flex; justify-content:space-between; padding:12px 44px 10px` | — | — |
| Title `<h1>` | `font-size:24px; font-weight:700` | `@Data.Title` | — |
| ADO Backlog link `<a>` | `color:#0078D4; text-decoration:none` | `href="@Data.BacklogLink"` | `target="_blank"` opens new tab |
| Subtitle `.sub` | `font-size:12px; color:#888` | `@Data.Subtitle` | — |
| Legend row (right) | `display:flex; gap:22px; font-size:12px` | `CurrentMonthLabel` for Now indicator text | — |
| PoC diamond indicator | `width:12px; height:12px; background:#F4B400; transform:rotate(45deg)` | — | — |
| Production diamond | `width:12px; height:12px; background:#34A853; transform:rotate(45deg)` | — | — |
| Checkpoint circle | `width:8px; height:8px; border-radius:50%; background:#999` | — | — |
| Now bar | `width:2px; height:14px; background:#EA4335` | — | — |

### Timeline Section → `Timeline.razor`

| Design Element | CSS/SVG Layout | Data Bindings | Interactions |
|----------------|---------------|---------------|-------------|
| `.tl-area` container | `display:flex; height:196px; background:#FAFAFA; border-bottom:2px solid #E8E8E8` | — | — |
| Track label sidebar (230px) | `flex-direction:column; justify-content:space-around; padding:16px 12px 16px 0; border-right:1px solid #E0E0E0` | `@foreach track in Timeline.Tracks` → renders `track.Id` in `track.Color`, `track.Label` in `#444` | — |
| SVG canvas | `<svg width="1560" height="185" overflow="visible">` | — | — |
| Month grid lines | `<line>` at computed X for each month 1st; `<text>` label | Generated from `Timeline.StartDate`→`Timeline.EndDate` month boundaries via `DateToX()` | — |
| Track horizontal lines | `<line y1=Y y2=Y stroke=Color stroke-width="3">` | `track.Color`, Y from `TrackY(index, count)` | — |
| Checkpoint markers | `<circle r="4-7" fill="white/#999" stroke="trackColor/#888">` | `event.Date` → `DateToX()`, `event.Label` | `<title>` tooltip on hover |
| PoC diamonds | `<polygon>` diamond shape, `fill="#F4B400"`, `filter="url(#sh)"` | `event.Date` → `DateToX()` for center X | `<title>` tooltip, `cursor:pointer` |
| Production diamonds | `<polygon>` diamond shape, `fill="#34A853"`, `filter="url(#sh)"` | `event.Date` → `DateToX()` for center X | `<title>` tooltip, `cursor:pointer` |
| Date labels | `<text font-size="10" fill="#666" text-anchor="middle">` | `event.Label`, positioned above/below marker | — |
| NOW line | `<line stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3">` | `Timeline.CurrentDate` → `DateToX()` | — |
| NOW label | `<text fill="#EA4335" font-size="10" font-weight="700">` | Positioned at NOW line X + 4px | — |
| Drop shadow filter | `<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>` | — | — |

**SVG Coordinate Calculations:**

```csharp
// In Timeline.razor @code block
private const double SvgWidth = 1560;
private const double SvgHeight = 185;

private double DateToX(DateOnly date)
{
    var start = Timeline.StartDate.ToDateTime(TimeOnly.MinValue);
    var end = Timeline.EndDate.ToDateTime(TimeOnly.MinValue);
    var target = date.ToDateTime(TimeOnly.MinValue);
    var totalDays = (end - start).TotalDays;
    var dayOffset = (target - start).TotalDays;
    return Math.Clamp((dayOffset / totalDays) * SvgWidth, 0, SvgWidth);
}

private double TrackY(int index, int count)
{
    // Evenly distribute tracks in SVG height, with top/bottom padding
    double usableHeight = SvgHeight - 40; // 20px padding top/bottom
    double spacing = usableHeight / (count + 1);
    return 20 + spacing * (index + 1);
}

private string DiamondPoints(double cx, double cy, double radius = 11)
{
    return $"{cx},{cy - radius} {cx + radius},{cy} {cx},{cy + radius} {cx - radius},{cy}";
}
```

### Heatmap Section → `HeatmapGrid.razor`

| Design Element | CSS Layout | Data Bindings | Interactions |
|----------------|-----------|---------------|-------------|
| `.hm-wrap` container | `flex:1; min-height:0; display:flex; flex-direction:column; padding:10px 44px` | — | — |
| `.hm-title` | `font-size:14px; font-weight:700; color:#888; text-transform:uppercase; letter-spacing:.5px` | Static text: "Monthly Execution Heatmap..." | — |
| `.hm-grid` | `display:grid; grid-template-columns:160px repeat(N,1fr); grid-template-rows:36px repeat(4,1fr); border:1px solid #E0E0E0` | N = `Heatmap.Columns.Count` via inline style | — |
| `.hm-corner` | `background:#F5F5F5; font-size:11px; font-weight:700; color:#999; text-transform:uppercase` | Static text: "STATUS" | — |
| `.hm-col-hdr` | `font-size:16px; font-weight:700; background:#F5F5F5` | `@column` from `Heatmap.Columns` | Highlight: `.apr-hdr` class when `column == Heatmap.HighlightColumn` |
| Row headers × 4 | `.hm-row-hdr` + category class (`.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr`) | Static labels: "✅ Shipped", "🔄 In Progress", "⏳ Carryover", "🚫 Blockers" | — |
| Data cells × (4 × N) | `.hm-cell` + category class (`.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`) | `Items` from `Heatmap.[Category].Items[column]`; `.apr` class for highlight column | — |
| Work item bullets | `.it::before` pseudo-element: `width:6px; height:6px; border-radius:50%` | Color set by category CSS class | — |
| Empty cell dash | `<span style="color:#AAA">-</span>` | Rendered when `Items` list is empty or key is missing | — |

**Category-to-CSS Mapping (used by `HeatmapGrid.razor` to select classes):**

```csharp
private static readonly (string Label, string HeaderCss, string CellCss, HeatmapCategory Category)[] Categories = new[]
{
    ("✅ Shipped",       "ship-hdr",  "ship-cell",  data.Heatmap.Shipped),
    ("🔄 In Progress",  "prog-hdr",  "prog-cell",  data.Heatmap.InProgress),
    ("⏳ Carryover",    "carry-hdr", "carry-cell", data.Heatmap.Carryover),
    ("🚫 Blockers",     "block-hdr", "block-cell", data.Heatmap.Blockers),
};
```

---

## Security Considerations

### Authentication & Authorization

**None implemented.** This is an explicitly unauthenticated, localhost-only tool.

| Aspect | Decision |
|--------|----------|
| Authentication | None. No login page, no identity provider, no tokens. |
| Authorization | None. No roles, no policies, no claims. |
| Network exposure | Kestrel binds to `localhost` only. Unreachable from other machines. |

### Input Validation

| Input | Validation |
|-------|-----------|
| `data.json` content | Deserialization into `required` record properties provides structural validation. Missing fields throw `JsonException`, caught and displayed. |
| `data.json` values | No business rule validation (e.g., "end date must be after start date"). Invalid dates will render incorrectly but won't crash. |
| URL parameters | None accepted. The single route `/` takes no query parameters. |
| User input | None. The app has no forms, no text fields, no interactive input. |

### Data Protection

| Concern | Mitigation |
|---------|-----------|
| Data sensitivity | `data.json` contains non-confidential project metadata (milestone names, status labels). No PII, no secrets. |
| Data at rest | No encryption. The JSON file sits on the local filesystem with standard OS file permissions. |
| Data in transit | HTTP only (no HTTPS). Acceptable because traffic never leaves localhost. |
| XSS risk | Blazor's Razor rendering engine HTML-encodes all `@` expressions by default. No `@((MarkupString)...)` usage except for trusted SVG generation. |
| CSRF | Blazor's built-in antiforgery is active but irrelevant (no forms, no mutations). |

### Secrets Management

**No secrets exist.** There are no API keys, connection strings, tokens, or credentials anywhere in the application.

### Future Security Upgrade Path

If the tool is ever shared on a network:
1. Add `builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate()` for Windows Auth
2. Switch to HTTPS via `builder.WebHost.UseUrls("https://localhost:5001")`
3. Add `[Authorize]` to `Dashboard.razor`

---

## Scaling Strategy

### Current Design: Not Applicable

This is a **single-user, single-machine, local-only tool**. Scaling is explicitly out of scope per the PM specification. The app runs on one developer's laptop and serves one browser tab.

### Data Volume Limits

| Dimension | Practical Limit | Reason |
|-----------|----------------|--------|
| Timeline tracks | 5 | 196px height / 5 tracks = ~35px per track (minimum viable) |
| Heatmap month columns | 6 | At 1920px with 160px row header, 6 columns = ~290px each (minimum for readable items) |
| Items per heatmap cell | ~8 | Cell height is ~175px at 4 rows in 1080px viewport; 8 items × 16px line-height ≈ 128px |
| `data.json` file size | 100KB | Deserialization target: < 50ms. A 100KB JSON with 5 tracks and 72 cells is more than sufficient. |

### If Scaling Were Ever Needed

The architecture would not change significantly:

- **Multiple projects:** Add a `projects/` folder with one JSON per project, add routing (`/dashboard/{project}`), and a project picker page.
- **Multiple concurrent users:** Kestrel handles hundreds of concurrent connections natively. The singleton service is thread-safe (immutable cached data). No changes needed.
- **Larger datasets:** Not a UI concern — the dashboard is intentionally information-dense for a single page. Larger datasets would need a different UX (pagination, filtering), which is a redesign, not a scaling exercise.

---

## Risks & Mitigations

### Risk 1: SVG Timeline Visual Fidelity (Medium)

**Risk:** The programmatic SVG generation produces different visual output than the hand-crafted reference SVG. Date-to-pixel math may place elements at slightly different positions. Overlapping labels may be unreadable.

**Mitigation:**
1. Implement in two phases: first hardcode reference SVG coordinates to verify CSS/layout match, then replace with calculated positions.
2. Use `Math.Clamp()` to prevent elements from rendering outside the SVG viewport.
3. Alternate label positions (above/below track line) to reduce overlap. The reference design already does this.
4. Side-by-side screenshot comparison at each phase as acceptance gate.

### Risk 2: Blazor Reconnection UI in Screenshots (Medium)

**Risk:** Blazor Server's SignalR connection can show a reconnection modal overlay during screenshot capture, ruining the clean output.

**Mitigation:**
1. CSS rule: `.components-reconnect-modal { display: none !important; }` in `dashboard.css`
2. Also hide the error UI: `#blazor-error-ui { display: none !important; }`
3. Prefer static SSR rendering mode where possible to eliminate SignalR entirely
4. Document the screenshot workflow: load page → wait 2 seconds → capture

### Risk 3: `DateOnly` JSON Deserialization (Low)

**Risk:** `System.Text.Json` in .NET 8 supports `DateOnly` natively, but the JSON format must match (`"2026-04-17"`). Users may provide dates in other formats (`"04/17/2026"`, `"April 17, 2026"`).

**Mitigation:**
1. Document the required date format (`YYYY-MM-DD`) in the README and in a JSON comment-equivalent (adjacent `data.schema.md` file)
2. The `JsonException` for malformed dates will be caught and displayed with the field name
3. The sample `data.json` uses the correct format as a template

### Risk 4: Cross-Platform Font Rendering (Low)

**Risk:** The design depends on Segoe UI, which is only available on Windows. macOS/Linux users will fall back to Arial, which has different metrics (letter spacing, line height), potentially causing layout shifts.

**Mitigation:**
1. The font stack is `'Segoe UI', Arial, sans-serif` — reasonable fallback
2. The target audience is Windows users (per PM spec)
3. macOS/Linux is explicitly "should work but not tested"
4. Fixed-width containers and `overflow: hidden` prevent layout breakage even with different metrics

### Risk 5: Data Schema Evolution (Low)

**Risk:** Future requests add new fields to `data.json` (e.g., "risk items" row, "dependencies" section). Old JSON files may break with new required fields.

**Mitigation:**
1. New fields should be nullable (`DateOnly?`, `HeatmapCategory?`) with sensible defaults
2. Use `JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }` for forward compatibility
3. The record-based model makes it easy to add optional properties without breaking existing files

### Risk 6: Large Item Lists Overflow Cells (Low)

**Risk:** A heatmap cell with 10+ items overflows the CSS Grid cell height, causing content to be clipped or push the layout beyond 1080px.

**Mitigation:**
1. `overflow: hidden` on `.hm-cell` prevents layout breakage (matches reference CSS)
2. Document the practical limit (~8 items per cell) in the README
3. Clipped items are still partially visible, signaling to the user that the cell is overloaded

---

## Appendix A: Solution Structure

```
ReportingDashboard.sln
│
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj          Zero NuGet dependencies
│       ├── Program.cs                          ~15 lines, DI + Blazor setup
│       ├── appsettings.json                    Default Kestrel config
│       ├── data.json                           Sample "Project Aurora" data
│       │
│       ├── Models/
│       │   └── DashboardData.cs                All record types + MilestoneType enum
│       │
│       ├── Services/
│       │   └── DashboardDataService.cs         JSON file reader + cache + error handling
│       │
│       ├── Components/
│       │   ├── App.razor                       HTML shell, <head>, CSS link
│       │   ├── Routes.razor                    <Router> component
│       │   ├── _Imports.razor                  Global @using statements
│       │   │
│       │   ├── Layout/
│       │   │   └── MainLayout.razor            @Body only, no chrome
│       │   │
│       │   ├── Pages/
│       │   │   └── Dashboard.razor             @page "/", injects service, error/data branch
│       │   │
│       │   └── Dashboard/
│       │       ├── Header.razor                Title, subtitle, backlog link, legend
│       │       ├── Timeline.razor              Track labels + SVG (DateToX, TrackY, shapes)
│       │       ├── HeatmapGrid.razor           Section title + CSS Grid + row iteration
│       │       ├── HeatmapRowHeader.razor      Category label with color class
│       │       └── HeatmapCell.razor           Item list or empty dash
│       │
│       └── wwwroot/
│           └── css/
│               └── dashboard.css               Full stylesheet ported from reference HTML
│
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj      xUnit + bUnit + FluentAssertions
        ├── Services/
        │   └── DashboardDataServiceTests.cs     Valid JSON, missing file, malformed JSON
        ├── Models/
        │   └── DashboardDataDeserializationTests.cs  Round-trip serialization, edge cases
        └── Components/
            ├── HeaderTests.cs                   Verify title, link href, legend rendering
            ├── TimelineTests.cs                 Verify SVG elements, DateToX math, track count
            ├── HeatmapGridTests.cs              Verify grid columns, highlight class, category rows
            └── HeatmapCellTests.cs              Verify item rendering, empty dash
```

## Appendix B: CSS Custom Properties

```css
:root {
    /* Status category colors */
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-highlight: #D8F2DA;
    --color-shipped-header-bg: #E8F5E9;
    --color-shipped-header-text: #1B7A28;

    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-highlight: #DAE8FB;
    --color-progress-header-bg: #E3F2FD;
    --color-progress-header-text: #1565C0;

    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-highlight: #FFF0B0;
    --color-carryover-header-bg: #FFF8E1;
    --color-carryover-header-text: #B45309;

    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-highlight: #FFE4E4;
    --color-blockers-header-bg: #FEF2F2;
    --color-blockers-header-text: #991B1B;

    /* Shared UI colors */
    --color-link: #0078D4;
    --color-now-line: #EA4335;
    --color-poc-diamond: #F4B400;
    --color-prod-diamond: #34A853;
    --color-checkpoint: #999;
    --color-highlight-month-bg: #FFF0D0;
    --color-highlight-month-text: #C07700;
    --color-border: #E0E0E0;
    --color-border-heavy: #CCC;
    --color-text-primary: #111;
    --color-text-secondary: #888;
    --color-text-body: #333;
    --color-background: #FFFFFF;
    --color-timeline-bg: #FAFAFA;
}
```

## Appendix C: Test Strategy

| Test Category | Framework | What's Tested | Example |
|--------------|-----------|---------------|---------|
| **Service: Happy path** | xUnit | `DashboardDataService` reads valid `data.json`, returns populated `DashboardData` | Assert `Title == "Project Aurora..."`, `Tracks.Count == 3` |
| **Service: Missing file** | xUnit | Service returns `null` with `ErrorMessage` containing "not found" | Assert `result is null`, `ErrorMessage.Contains("not found")` |
| **Service: Malformed JSON** | xUnit | Service returns `null` with `ErrorMessage` containing parse error details | Assert `ErrorMessage.Contains("Invalid JSON")` |
| **Model: Deserialization** | xUnit | Sample JSON string deserializes to correct record structure | Assert `data.Timeline.Tracks[0].Events[1].Type == MilestoneType.PoC` |
| **Model: DateOnly parsing** | xUnit | `"2026-04-17"` deserializes to `DateOnly(2026, 4, 17)` | Assert date equality |
| **Model: Enum parsing** | xUnit | `"Production"` string maps to `MilestoneType.Production` | Assert enum value |
| **Component: Header** | bUnit | Header renders title, link, subtitle, legend items | `cut.Find("h1").TextContent.Should().Contain(title)` |
| **Component: Timeline SVG** | bUnit | SVG contains correct number of track lines, milestone shapes, NOW line | `cut.FindAll("line").Count.Should().Be(expectedCount)` |
| **Component: HeatmapGrid** | bUnit | Grid renders correct number of columns, highlight class applied | `cut.Find(".apr-hdr").Should().NotBeNull()` |
| **Component: HeatmapCell empty** | bUnit | Empty cell renders dash "-" | `cut.Find(".hm-cell").TextContent.Should().Contain("-")` |
| **Component: Error state** | bUnit | Dashboard shows error panel when service returns null | `cut.Find("h2").TextContent.Should().Contain("Error")` |