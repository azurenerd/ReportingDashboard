# Architecture

## Overview & Goals

This system is a **single-page executive reporting dashboard** that renders a project release roadmap with a timeline/Gantt visualization and a color-coded heatmap grid. It is built with C# .NET 8 Blazor Server, runs entirely on localhost with zero cloud dependencies, and is optimized for pixel-perfect 1920×1080 screenshot capture for PowerPoint decks.

### Primary Goals

1. **Visual Fidelity** — Reproduce the `OriginalDesignConcept.html` layout pixel-for-pixel in a Blazor Server application.
2. **Data-Driven Rendering** — All dashboard content (title, milestones, heatmap items) is driven by a single `data.json` file, enabling reuse across teams and reporting periods.
3. **Zero Infrastructure** — No database, no cloud services, no authentication. Run with `dotnet run`, open browser, take screenshot.
4. **Minimal Complexity** — Single `.sln`, single `.csproj`, zero additional NuGet packages. The entire codebase should be understandable in under 30 minutes.

### Non-Goals

- User interaction beyond browser navigation
- Multi-user or concurrent access
- Persistent state or data mutation
- Mobile or responsive design (fixed 1920×1080 only)
- PDF/PowerPoint export (screenshots are the workflow)

---

## System Components

### 1. Data Layer

#### `Models/DashboardData.cs` — Data Model Records

**Responsibility:** Define the shape of all dashboard data as immutable C# records.

**Interfaces:** None (POCOs consumed by service and components).

**Dependencies:** `System.Text.Json` serialization attributes (built-in).

**Data:**

```csharp
public record DashboardData
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogLink { get; init; } = "";
    public DateOnly? ReportDate { get; init; }          // null = use DateTime.Now
    public DateOnly TimelineStart { get; init; }         // left edge of timeline
    public DateOnly TimelineEnd { get; init; }           // right edge of timeline
    public List<MilestoneTrack> Tracks { get; init; } = [];
    public List<string> HeatmapMonths { get; init; } = [];    // column headers
    public string? CurrentMonthHighlight { get; init; }       // which month gets "current" styling
    public List<HeatmapCategory> Categories { get; init; } = [];
    public LegendConfig Legend { get; init; } = new();
}

public record MilestoneTrack
{
    public string Id { get; init; } = "";              // "M1", "M2", "M3"
    public string Label { get; init; } = "";            // "Chatbot & MS Role"
    public string Color { get; init; } = "#0078D4";     // track line & label color
    public List<MilestoneEvent> Events { get; init; } = [];
}

public record MilestoneEvent
{
    public DateOnly Date { get; init; }
    public string Label { get; init; } = "";            // "Mar 26 PoC"
    public MilestoneType Type { get; init; }            // enum: Poc, Production, Checkpoint, Start
    public string? LabelPosition { get; init; }         // "above" | "below" (default: below)
}

public enum MilestoneType
{
    Start,          // open circle on the track line
    Checkpoint,     // small filled circle
    Poc,            // yellow diamond
    Production      // green diamond
}

public record HeatmapCategory
{
    public string Name { get; init; } = "";             // "Shipped"
    public string Type { get; init; } = "";             // "shipped" | "in-progress" | "carryover" | "blockers"
    public Dictionary<string, List<string>> ItemsByMonth { get; init; } = new();
}

public record LegendConfig
{
    public bool ShowPocMilestone { get; init; } = true;
    public bool ShowProductionRelease { get; init; } = true;
    public bool ShowCheckpoint { get; init; } = true;
    public bool ShowNowLine { get; init; } = true;
}
```

#### `Services/DashboardDataService.cs` — Data Access Service

**Responsibility:** Read, deserialize, and cache `data.json`. Optionally watch for file changes and trigger reload.

**Interface:**

```csharp
public interface IDashboardDataService
{
    DashboardData GetData();
    event Action? OnDataChanged;
}
```

**Dependencies:** `System.Text.Json`, `System.IO.FileSystemWatcher` (optional).

**Registration:** Singleton in DI container.

**Behavior:**
- On construction, reads `data.json` from a configurable path (default: project root or `wwwroot/data/`).
- Caches the deserialized `DashboardData` in memory.
- If `FileSystemWatcher` is enabled, watches `data.json` for changes, re-reads, and fires `OnDataChanged`.
- Uses `JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }`.
- All properties have safe defaults; missing JSON fields do not cause exceptions.

#### `Services/TimelineCalculator.cs` — SVG Coordinate Math

**Responsibility:** Convert dates to pixel positions for SVG rendering. Isolates all math from Razor templates.

**Interface:**

```csharp
public class TimelineCalculator
{
    public TimelineCalculator(DateOnly start, DateOnly end, double svgWidth);
    public double DateToX(DateOnly date);
    public double NowX(DateOnly? overrideDate = null);
    public List<MonthGridLine> GetMonthGridLines();
}

public record MonthGridLine(double X, string Label);
```

**Dependencies:** None (pure calculation).

**Key Implementation Detail:** All numeric output uses `CultureInfo.InvariantCulture` to avoid locale-specific decimal separators (e.g., comma vs. period) which would break SVG `x`/`y` attributes.

---

### 2. UI Layer — Blazor Razor Components

All components live under `Components/` in the project. Each has a corresponding `.razor.css` scoped stylesheet.

#### `Components/Layout/MainLayout.razor`

**Responsibility:** Root layout. Sets the 1920×1080 fixed viewport body. Contains no navigation chrome.

**CSS Strategy:** Fixed body dimensions, overflow hidden, Segoe UI font stack.

#### `Components/Pages/Dashboard.razor`

**Responsibility:** The single routable page (`@page "/"`). Injects `IDashboardDataService`, retrieves data, and composes all child components.

**Interactions:**
- Subscribes to `OnDataChanged` event for live reload (if enabled).
- Passes data down to children via `[Parameter]` attributes.

#### `Components/Dashboard/DashboardHeader.razor`

**Responsibility:** Renders the top header bar: title with backlog link, subtitle, and milestone legend icons.

**Maps to Design:** The `.hdr` div — full-width flex row with title on the left and legend on the right.

**Parameters:**

```csharp
[Parameter] public string Title { get; set; } = "";
[Parameter] public string Subtitle { get; set; } = "";
[Parameter] public string BacklogLink { get; set; } = "";
[Parameter] public LegendConfig Legend { get; set; } = new();
```

**CSS:** Flexbox with `justify-content: space-between`. Border-bottom separator. Legend items use inline SVG shapes (diamond via rotated square, circle, vertical line).

#### `Components/Dashboard/TimelineSection.razor`

**Responsibility:** Container for the entire timeline area. Renders the track label sidebar and the SVG timeline box.

**Maps to Design:** The `.tl-area` div — flex row with 230px label sidebar and flexible SVG area.

**Parameters:**

```csharp
[Parameter] public List<MilestoneTrack> Tracks { get; set; } = [];
[Parameter] public DateOnly TimelineStart { get; set; }
[Parameter] public DateOnly TimelineEnd { get; set; }
[Parameter] public DateOnly? ReportDate { get; set; }
```

**Children:** Contains `TimelineLane` and `MilestoneMarker` components rendered inside an `<svg>` element.

**SVG Rendering Logic:**
1. Instantiates `TimelineCalculator` with start/end dates and SVG width (1560px per design).
2. Renders month grid lines via `GetMonthGridLines()`.
3. Renders the "NOW" indicator line at `NowX()`.
4. Iterates `Tracks` to render horizontal track lines, spaced evenly across the SVG height.
5. For each track, iterates `Events` to render `MilestoneMarker` components at calculated X positions.

#### `Components/Dashboard/TimelineLane.razor`

**Responsibility:** Renders a single horizontal track line within the SVG.

**Parameters:**

```csharp
[Parameter] public double Y { get; set; }           // vertical position
[Parameter] public string Color { get; set; } = "";
[Parameter] public double Width { get; set; }
```

**Output:** `<line x1="0" y1="{Y}" x2="{Width}" y2="{Y}" stroke="{Color}" stroke-width="3"/>`

#### `Components/Dashboard/MilestoneMarker.razor`

**Responsibility:** Renders a single milestone event as an SVG shape (diamond, circle, or open circle) with a text label.

**Parameters:**

```csharp
[Parameter] public double X { get; set; }
[Parameter] public double Y { get; set; }
[Parameter] public MilestoneEvent Event { get; set; } = default!;
[Parameter] public string TrackColor { get; set; } = "";
```

**Rendering Rules by `MilestoneType`:**

| Type | SVG Element | Fill | Stroke | Size |
|------|------------|------|--------|------|
| `Start` | `<circle>` | white | TrackColor, 2.5px | r=7 |
| `Checkpoint` | `<circle>` | #999 | none | r=4 |
| `Poc` | `<polygon>` (diamond) | #F4B400 | none, drop-shadow | 11px |
| `Production` | `<polygon>` (diamond) | #34A853 | none, drop-shadow | 11px |

**Label Positioning:** Text rendered above (`y - 16`) or below (`y + 24`) the marker based on `LabelPosition` parameter.

#### `Components/Dashboard/HeatmapSection.razor`

**Responsibility:** Renders the full heatmap grid including title, column headers, row headers, and data cells.

**Maps to Design:** The `.hm-wrap` and `.hm-grid` divs — CSS Grid layout.

**Parameters:**

```csharp
[Parameter] public List<string> Months { get; set; } = [];
[Parameter] public string? CurrentMonthHighlight { get; set; }
[Parameter] public List<HeatmapCategory> Categories { get; set; } = [];
```

**CSS Grid Definition:**
```css
grid-template-columns: 160px repeat(var(--month-count), 1fr);
grid-template-rows: 36px repeat(var(--category-count), 1fr);
```

The `--month-count` and `--category-count` CSS custom properties are set dynamically via `style` attribute from C# parameters.

**Children:** Composes `HeatmapRow` components for each category.

#### `Components/Dashboard/HeatmapRow.razor`

**Responsibility:** Renders one category row (row header + data cells for each month).

**Parameters:**

```csharp
[Parameter] public HeatmapCategory Category { get; set; } = default!;
[Parameter] public List<string> Months { get; set; } = [];
[Parameter] public string? CurrentMonthHighlight { get; set; }
```

**CSS Class Mapping:**

| Category Type | Header Class | Cell Class | Current Month Class | Bullet Color |
|--------------|-------------|-----------|-------------------|-------------|
| `shipped` | `ship-hdr` | `ship-cell` | `ship-cell apr` | #34A853 |
| `in-progress` | `prog-hdr` | `prog-cell` | `prog-cell apr` | #0078D4 |
| `carryover` | `carry-hdr` | `carry-cell` | `carry-cell apr` | #F4B400 |
| `blockers` | `block-hdr` | `block-cell` | `block-cell apr` | #EA4335 |

#### `Components/Dashboard/HeatmapCell.razor`

**Responsibility:** Renders individual items within a heatmap cell as bulleted text.

**Parameters:**

```csharp
[Parameter] public List<string> Items { get; set; } = [];
[Parameter] public string CssClass { get; set; } = "";
```

**Output:** Each item rendered as `<div class="it">{item text}</div>` with a `::before` pseudo-element colored bullet.

---

### 3. Application Entry Point

#### `Program.cs`

**Responsibility:** Configure and start the Blazor Server application.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Bind to localhost only
builder.WebHost.UseUrls("https://localhost:5001");

// Register singleton data service
builder.Services.AddSingleton<IDashboardDataService, DashboardDataService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

---

## UI Component Architecture

This section maps each visual section from the `OriginalDesignConcept.html` design to a specific Blazor component.

### Visual Section → Component Mapping

| Design Section | HTML Class/Element | Blazor Component | CSS Layout | Data Bindings | Interactions |
|---|---|---|---|---|---|
| **Top header bar** | `.hdr` | `DashboardHeader.razor` | Flexbox row, `justify-content: space-between`, `padding: 12px 44px`, `border-bottom: 1px solid #E0E0E0` | `Title`, `Subtitle`, `BacklogLink`, `Legend` | Backlog link is `<a>` tag (static) |
| **Legend icons** | Inline spans in `.hdr` | Part of `DashboardHeader.razor` | Flexbox row, `gap: 22px`, inline SVG shapes | `LegendConfig` booleans control visibility | None |
| **Timeline sidebar labels** | 230px div in `.tl-area` | Part of `TimelineSection.razor` | Flex column, `justify-content: space-around`, `width: 230px`, `border-right: 1px solid #E0E0E0` | `Tracks[].Id`, `Tracks[].Label`, `Tracks[].Color` | None |
| **SVG timeline chart** | `.tl-svg-box > svg` | `TimelineSection.razor` + `TimelineLane.razor` + `MilestoneMarker.razor` | SVG 1560×185, positioned via `TimelineCalculator` date→pixel math | `Tracks[]`, `TimelineStart`, `TimelineEnd`, `ReportDate` | None |
| **Month grid lines** | `<line>` + `<text>` in SVG | Rendered in `TimelineSection.razor` loop | SVG absolute positioning from `TimelineCalculator.GetMonthGridLines()` | Derived from `TimelineStart`/`TimelineEnd` | None |
| **"NOW" indicator** | Red dashed `<line>` in SVG | Rendered in `TimelineSection.razor` | SVG line at `NowX()`, `stroke-dasharray: 5,3`, `stroke: #EA4335` | `ReportDate` (or `DateTime.Now`) | None |
| **Milestone diamonds/circles** | `<polygon>`, `<circle>` in SVG | `MilestoneMarker.razor` | SVG absolute positioning, drop-shadow filter | `MilestoneEvent.Date`, `.Type`, `.Label` | None |
| **Heatmap title** | `.hm-title` | Part of `HeatmapSection.razor` | Block, uppercase, `font-size: 14px`, `color: #888` | Hardcoded "DELIVERY STATUS BY MONTH" or data-driven | None |
| **Heatmap grid** | `.hm-grid` | `HeatmapSection.razor` | CSS Grid: `160px repeat(N, 1fr)` columns, `36px repeat(4, 1fr)` rows | `Months[]`, `Categories[]` | None |
| **Corner cell** | `.hm-corner` | Part of `HeatmapSection.razor` | Grid cell `[1,1]`, `background: #F5F5F5`, centered text | Static label "STATUS" | None |
| **Month column headers** | `.hm-col-hdr` | Part of `HeatmapSection.razor` loop | Grid row 1, `font-size: 16px`, `font-weight: 700`, current month gets `.apr-hdr` styling (`background: #FFF0D0`, `color: #C07700`) | `HeatmapMonths[]`, `CurrentMonthHighlight` | None |
| **Category row headers** | `.hm-row-hdr` + type class | Part of `HeatmapRow.razor` | `padding: 0 12px`, uppercase, `letter-spacing: 0.7px`, colored background per type | `Category.Name`, `Category.Type` | None |
| **Data cells** | `.hm-cell` + type class | `HeatmapCell.razor` | `padding: 8px 12px`, colored background, items as bulleted `<div class="it">` with `::before` dot | `Category.ItemsByMonth[month]` | None |

### CSS Architecture

```
wwwroot/
├── css/
│   └── app.css                    # CSS custom properties, body reset, fixed viewport
Components/
├── Layout/
│   ├── MainLayout.razor
│   └── MainLayout.razor.css       # Root layout scoped styles
├── Pages/
│   ├── Dashboard.razor
│   └── Dashboard.razor.css        # Page-level scoped styles (minimal)
├── Dashboard/
│   ├── DashboardHeader.razor.css  # Header flex layout, legend styles
│   ├── TimelineSection.razor.css  # Timeline area layout, SVG container
│   ├── HeatmapSection.razor.css   # Grid definition, header cell styles
│   ├── HeatmapRow.razor.css       # Row header + cell color themes
│   └── HeatmapCell.razor.css      # Item bullet styles
```

**Global CSS (`app.css`):**

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-shipped-hdr-bg: #E8F5E9;
    --color-shipped-hdr-text: #1B7A28;

    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-progress-hdr-bg: #E3F2FD;
    --color-progress-hdr-text: #1565C0;

    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-carryover-hdr-bg: #FFF8E1;
    --color-carryover-hdr-text: #B45309;

    --color-blocker: #EA4335;
    --color-blocker-bg: #FFF5F5;
    --color-blocker-bg-current: #FFE4E4;
    --color-blocker-hdr-bg: #FEF2F2;
    --color-blocker-hdr-text: #991B1B;

    --color-link: #0078D4;
    --color-border: #E0E0E0;
    --color-border-heavy: #CCC;
    --color-text: #111;
    --color-text-secondary: #888;
    --color-text-item: #333;
    --color-bg-header: #F5F5F5;
    --color-bg-timeline: #FAFAFA;
    --color-current-month-hdr-bg: #FFF0D0;
    --color-current-month-hdr-text: #C07700;
}

*, *::before, *::after { margin: 0; padding: 0; box-sizing: border-box; }

body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
    background: #FFFFFF;
    font-family: 'Segoe UI', Arial, sans-serif;
    color: var(--color-text);
    display: flex;
    flex-direction: column;
}

a { color: var(--color-link); text-decoration: none; }
```

---

## Component Interactions

### Data Flow

```
                    ┌─────────────┐
                    │  data.json  │
                    └──────┬──────┘
                           │ read + deserialize
                    ┌──────▼──────────────────┐
                    │  DashboardDataService    │
                    │  (singleton, cached)     │
                    │  ┌─ FileSystemWatcher ─┐ │  ← optional live reload
                    └──────┬──────────────────┘
                           │ inject via DI
                    ┌──────▼──────┐
                    │  Dashboard  │  (@page "/")
                    │   .razor    │
                    └──┬───┬───┬──┘
                       │   │   │     [Parameter] passing
          ┌────────────┘   │   └────────────┐
          ▼                ▼                ▼
   DashboardHeader   TimelineSection   HeatmapSection
                       │                    │
                  ┌────┴────┐          ┌────┴────┐
                  ▼         ▼          ▼         ▼
           TimelineLane  Milestone   HeatmapRow  ...
                         Marker         │
                                        ▼
                                   HeatmapCell
```

### Communication Patterns

1. **Unidirectional data flow only.** Data flows from `DashboardDataService` → `Dashboard.razor` → child components via `[Parameter]` properties. No callbacks, no two-way binding, no event bubbling.

2. **No component state.** Components are pure render functions of their parameters. No `@onclick`, no form inputs, no user-triggered state changes.

3. **Optional live reload path:** If `FileSystemWatcher` is enabled, `DashboardDataService` fires `OnDataChanged`. `Dashboard.razor` subscribes in `OnInitialized`, calls `InvokeAsync(StateHasChanged)`, which triggers a full re-render down the component tree.

4. **SVG coordinate calculation** is performed in `TimelineSection.razor`'s code block using `TimelineCalculator`, then passed to child SVG components as numeric `[Parameter]` values. No calculation happens in child components.

---

## Data Model

### Primary Entity: `DashboardData`

This is the root object deserialized from `data.json`. It is **read-only** and **immutable** (C# records with `init`-only setters).

### Entity Relationship

```
DashboardData (1)
├── has many → MilestoneTrack (N)
│   └── has many → MilestoneEvent (N)
├── has many → HeatmapCategory (N)
│   └── has dictionary → ItemsByMonth: { monthKey → [item strings] }
├── has one → LegendConfig
└── has list → HeatmapMonths (ordered column headers)
```

### Storage

- **Format:** Single JSON file (`data.json`)
- **Location:** `wwwroot/data/data.json` (served as static file but primarily read by server-side service)
- **Size:** < 10 KB typical (fewer than 100 items across all categories)
- **Schema versioning:** Forward-compatible via default property values and `JsonIgnoreCondition.WhenWritingNull`

### Sample `data.json` Structure

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogLink": "https://dev.azure.com/org/project/_backlogs",
  "reportDate": "2026-04-10",
  "timelineStart": "2026-01-01",
  "timelineEnd": "2026-06-30",
  "tracks": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "Start" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "Poc" },
        { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "Production", "labelPosition": "above" }
      ]
    }
  ],
  "heatmapMonths": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonthHighlight": "Apr",
  "categories": [
    {
      "name": "Shipped",
      "type": "shipped",
      "itemsByMonth": {
        "Jan": ["Agent framework v1", "Prompt templates"],
        "Feb": ["MS Role lookup API"],
        "Mar": ["Data classification engine"],
        "Apr": []
      }
    }
  ],
  "legend": {
    "showPocMilestone": true,
    "showProductionRelease": true,
    "showCheckpoint": true,
    "showNowLine": true
  }
}
```

---

## API Contracts

This application has **no REST API, no GraphQL, no gRPC**. It is a server-rendered Blazor application that serves a single HTML page.

### HTTP Endpoints

| Method | Path | Purpose | Response |
|--------|------|---------|----------|
| GET | `/` | Main dashboard page | Server-rendered HTML (Blazor component tree) |
| GET | `/_framework/*` | Blazor framework files (SignalR, JS interop) | Static assets (auto-managed by framework) |
| GET | `/css/*` | Static CSS files | Bundled CSS including scoped component styles |

### SignalR Circuit (Blazor Server)

Blazor Server establishes a SignalR WebSocket connection for interactive mode. For this read-only dashboard:

- **Recommendation:** Use Static SSR (no `@rendermode` attribute on the root component) to eliminate SignalR entirely. This renders pure HTML with zero JavaScript overhead.
- **Alternative:** If `FileSystemWatcher` live reload is desired, use Interactive Server mode so `StateHasChanged` can push re-renders to the browser.

### Error Handling

| Scenario | Behavior |
|----------|----------|
| `data.json` not found | Application logs error at startup, renders empty dashboard with placeholder message |
| `data.json` malformed JSON | `JsonException` caught in `DashboardDataService`, logged, returns `DashboardData` with defaults |
| `data.json` missing fields | Default values on all record properties ensure partial data still renders |
| Browser disconnects (SignalR) | N/A for Static SSR; for Interactive mode, Blazor shows reconnection UI (acceptable for local use) |

---

## Infrastructure Requirements

### Development Environment

| Requirement | Specification |
|------------|--------------|
| **SDK** | .NET 8.0 SDK (LTS) — any 8.0.x patch |
| **OS** | Windows 10/11 (Segoe UI font dependency for visual fidelity) |
| **IDE** | Visual Studio 2022 17.8+ or VS Code with C# Dev Kit |
| **Browser** | Microsoft Edge or Google Chrome (Chromium-based, for consistent screenshot rendering) |

### Runtime Environment

| Requirement | Specification |
|------------|--------------|
| **Host** | Kestrel (built-in, default) |
| **Binding** | `https://localhost:5001` (localhost only) |
| **Memory** | < 100 MB (single user, single circuit, cached data) |
| **Disk** | < 50 MB installed (self-contained publish ~65 MB, framework-dependent < 5 MB) |
| **Network** | None required. Fully offline after `dotnet restore`. |

### Project Structure (Filesystem)

```
ReportingDashboard.sln
src/
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Models/
    │   └── DashboardData.cs          # All record types
    ├── Services/
    │   ├── IDashboardDataService.cs
    │   ├── DashboardDataService.cs
    │   └── TimelineCalculator.cs
    ├── Components/
    │   ├── _Imports.razor
    │   ├── App.razor
    │   ├── Routes.razor
    │   ├── Layout/
    │   │   ├── MainLayout.razor
    │   │   └── MainLayout.razor.css
    │   ├── Pages/
    │   │   ├── Dashboard.razor
    │   │   └── Dashboard.razor.css
    │   └── Dashboard/
    │       ├── DashboardHeader.razor
    │       ├── DashboardHeader.razor.css
    │       ├── TimelineSection.razor
    │       ├── TimelineSection.razor.css
    │       ├── TimelineLane.razor
    │       ├── MilestoneMarker.razor
    │       ├── MilestoneMarker.razor.css
    │       ├── HeatmapSection.razor
    │       ├── HeatmapSection.razor.css
    │       ├── HeatmapRow.razor
    │       ├── HeatmapRow.razor.css
    │       ├── HeatmapCell.razor
    │       └── HeatmapCell.razor.css
    └── wwwroot/
        ├── css/
        │   └── app.css               # Global styles + CSS custom properties
        └── data/
            ├── data.json             # Active dashboard data (gitignored)
            └── data.template.json    # Example with documentation
```

### CI/CD

Not required for local-only usage. If desired:

```yaml
# .github/workflows/build.yml (optional)
- dotnet restore
- dotnet build --no-restore --configuration Release
- dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### Distribution (Optional)

For sharing with non-developers:

```shell
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

Ship `./publish/ReportingDashboard.exe` + `data.json`. User double-clicks exe, opens browser to `https://localhost:5001`.

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|--------------|
| **UI Framework** | Blazor Server (.NET 8) | Mandated by project requirements. Ideal for server-rendered, single-user local app. |
| **Render Mode** | Static SSR (default) with optional Interactive Server | Static SSR eliminates all JavaScript/SignalR overhead. Interactive mode only if live reload is desired. |
| **CSS Approach** | Scoped CSS (`.razor.css`) + global custom properties | Built-in to Blazor. No build pipeline. Isolation prevents style conflicts. Custom properties enable theming. |
| **CSS Framework** | None | Bootstrap/Tailwind would fight the pixel-perfect custom design. The original design is ~100 lines of CSS. |
| **Charting Library** | None (hand-crafted SVG) | The timeline is ~40 lines of SVG. Libraries add 200KB+ of JS, impose styling opinions, and reduce pixel control. |
| **Component Library** | None (MudBlazor, Radzen, etc. rejected) | The design has 8 simple components. Libraries impose themes that conflict with the exact design specification. |
| **Data Storage** | Flat JSON file | Read-only, < 100 items, manually edited. Any database adds complexity with zero benefit. |
| **Serialization** | `System.Text.Json` (built-in) | Ships with .NET 8. Zero additional packages. Handles all needs. |
| **Authentication** | None | Local-only, single-user tool. No auth surface to attack. |
| **NuGet Packages** | Zero additional | Everything required ships in the .NET 8 SDK default template. |
| **JavaScript** | Zero | All rendering is server-side C#/Razor. No JS interop needed. |
| **Testing** | Optional xUnit + bUnit | Project is simple enough that manual visual verification suffices. Tests can be added later for regression. |

---

## Security Considerations

### Threat Model

This is a **local-only, single-user, read-only** application. The threat surface is minimal.

| Threat | Risk Level | Mitigation |
|--------|-----------|------------|
| **Network exposure** | Low | Bind to `localhost` only via `builder.WebHost.UseUrls("https://localhost:5001")`. Kestrel will refuse external connections. |
| **Data exfiltration** | N/A | No network calls. No telemetry. No external services. Data never leaves the machine. |
| **XSS via data.json** | Low | Blazor's Razor engine HTML-encodes all `@expressions` by default. Even malicious strings in `data.json` are rendered as text, not executed. |
| **JSON injection** | N/A | Data is deserialized into strongly-typed records. No dynamic evaluation. |
| **Sensitive data in JSON** | Low | Add `data.json` to `.gitignore`. Ship `data.template.json` with example data only. Document that users should not commit production data. |
| **Dependency supply chain** | Very Low | Zero additional NuGet packages. Only the .NET 8 SDK built-in libraries are used. |
| **HTTPS certificate** | Low | The .NET development certificate (`dotnet dev-certs https --trust`) is sufficient for localhost. No public CA cert needed. |

### Data Protection

- No PII in scope.
- No passwords, tokens, or secrets.
- No encryption at rest or in transit required (localhost + dev cert is appropriate).
- If the JSON contains sensitive project names: gitignore it and educate users.

### Input Validation

- `data.json` is the sole input. Validated implicitly by `System.Text.Json` deserialization.
- Invalid dates, missing fields, or malformed JSON result in default values or logged exceptions — never crashes or undefined behavior.
- No user input of any kind at runtime.

---

## Scaling Strategy

### This Application Does Not Scale — By Design

This is a single-user, local-only rendering tool. Scaling is not a concern and should not influence architectural decisions.

| Dimension | Current Capacity | Sufficient? |
|-----------|-----------------|-------------|
| **Users** | 1 (localhost) | Yes — designed for single-user screenshot capture |
| **Data Volume** | < 100 heatmap items, < 20 milestones | Yes — far below any rendering performance threshold |
| **Concurrent Requests** | 1 browser tab | Yes — Kestrel handles this trivially |
| **Render Time** | < 50ms for full page | Yes — no optimization needed |

### If Requirements Change

If this dashboard later needs to serve a team or be hosted centrally:

1. **Multiple users:** Blazor Server supports ~5,000 concurrent circuits per server. A single instance handles a full team.
2. **Multiple projects:** Create separate `data.json` files, add a project selector dropdown, or run multiple instances on different ports.
3. **Automated screenshots:** Integrate Puppeteer Sharp or Playwright to programmatically capture the page. No architecture change needed.
4. **Data from API:** Replace `DashboardDataService`'s JSON file read with an HTTP call. The service interface is already abstracted.

---

## Risks & Mitigations

### Risk 1: SVG Coordinate Locale Issues

**Severity:** Medium — Can produce broken SVG silently.

**Description:** C# `ToString()` on `double` values uses the thread's `CultureInfo`. In locales that use commas as decimal separators (e.g., German, French), `123.45` renders as `123,45`, which is invalid in SVG attributes.

**Mitigation:** `TimelineCalculator` returns all coordinates as `double`. Razor templates format them explicitly:

```csharp
@($"{x.ToString("F1", CultureInfo.InvariantCulture)}")
```

This is enforced in code review. The `TimelineCalculator` class includes a unit test that verifies output under `CultureInfo("de-DE")`.

### Risk 2: Browser Rendering Differences for Screenshots

**Severity:** Low — Only affects screenshot workflow.

**Description:** Different browsers render fonts and SVG sub-pixel positioning differently. Segoe UI metrics vary between browsers and OS versions.

**Mitigation:**
- Document that screenshots must be taken in **Microsoft Edge or Google Chrome** on Windows.
- Fix viewport to exactly 1920×1080 (browser DevTools device mode or fullscreen on a 1080p display).
- Include screenshot instructions in `README.md`.

### Risk 3: JSON Schema Evolution

**Severity:** Low — Affects data file compatibility across versions.

**Description:** Adding new fields to `DashboardData` may break existing `data.json` files if required properties are added.

**Mitigation:**
- All properties use `{ get; init; }` with default values (empty strings, empty lists, null for optional).
- `JsonSerializerOptions` configured with `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`.
- New features use nullable/optional properties with fallback behavior.
- Ship `data.template.json` with documentation of all fields.

### Risk 4: Blazor SignalR Overhead

**Severity:** Very Low — Negligible for local single-user.

**Description:** Blazor Server maintains a WebSocket circuit per browser tab, consuming ~256 KB memory and holding a persistent connection.

**Mitigation:**
- Use Static SSR render mode (no `@rendermode` attribute) for the dashboard page. This serves pure HTML with zero SignalR.
- If interactive mode is needed for `FileSystemWatcher` live reload, the overhead is irrelevant for a single local user.

### Risk 5: File Locking on `data.json`

**Severity:** Low — Only affects live reload scenario.

**Description:** If the user edits `data.json` in an editor while `FileSystemWatcher` is active, file locking or rapid successive save events could cause read failures.

**Mitigation:**
- `DashboardDataService` reads with `FileShare.ReadWrite` to avoid blocking editors.
- `FileSystemWatcher` handler debounces events (500ms delay before re-read).
- Read failures are caught and logged; the previously cached data continues to render.

### Risk 6: .NET 8 LTS End of Life

**Severity:** Very Low — Long timeline.

**Description:** .NET 8 LTS support ends November 2026. After that, no security patches.

**Mitigation:** .NET 8 to .NET 10 (next LTS, November 2025) migration for a project of this size is a 15-minute `TargetFramework` change. No architectural concern.