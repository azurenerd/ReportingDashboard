# Architecture

## Overview & Goals

This document describes the architecture for **ReportingDashboard**, a single-page executive reporting dashboard that renders a milestone Gantt timeline and a monthly execution heatmap from a local `data.json` file. The product is optimized for one thing: producing a pixel-perfect 1920x1080 visual that an executive or project lead can screenshot directly into a PowerPoint slide.

**Primary goals of this architecture:**

1. **Pixel-perfect fidelity** to `OriginalDesignConcept.html` at 1920x1080 - screenshot quality is the #1 success metric.
2. **Zero-friction local execution** - `dotnet run` must produce a screenshot-ready page in under 5 seconds with no auth, no cloud, no database, no build pipeline.
3. **Data-driven rendering** - every on-screen value (except static legend labels) originates from a single `data.json` file that a non-developer can edit.
4. **Hot reload on save** - editing `data.json` and refreshing the browser reflects the change without restarting the app (via `IOptionsMonitor<T>` + `reloadOnChange:true`).
5. **Zero JavaScript** - all visuals rendered via server-side Razor + inline SVG + CSS. No JS interop.
6. **Minimal footprint** - ≤900 LOC across the whole solution; single `.sln`, 2 projects (web + tests).
7. **Graceful failure** on missing/malformed `data.json` - clear developer error page, never a half-rendered screenshot.
8. **Local-only security posture** - Kestrel bound to `127.0.0.1:5000`, HTTP only, no inbound LAN exposure.

**Architectural pattern:** *Minimal vertical slice* + *Options pattern*. No MVVM, no Clean Architecture layers, no CQRS. A single page, a single configured POCO tree, a handful of presentational Razor components.

**Rendering mode decision:** **Static Server-Side Rendering (Static SSR)** is used for `Dashboard.razor` and all child components. The page has zero interactivity; avoiding the SignalR circuit reduces rendered HTML noise, removes the blazor JS `<script>` requirement in practice (the framework still injects it, which is acceptable - "zero custom JS" is the requirement), simplifies the rendered DOM for screenshotting, and eliminates the reconnect overlay on network blips. The project is scaffolded as a Blazor Server app (per mandate) and simply omits `@rendermode InteractiveServer` on the root page. If the user later needs interactivity, flipping one attribute enables Interactive Server mode with no other code changes.

---

## System Components

The solution is composed of two .NET projects and a handful of Razor components, C# records, and a single JSON data file. All components live inside `ReportingDashboard.Web` except tests.

### Solution Layout

```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard.Web/              (net8.0, Blazor Server app)
│       ├── Program.cs
│       ├── appsettings.json
│       ├── data.json                         (content source; copied to output)
│       ├── Models/
│       │   ├── DashboardData.cs
│       │   ├── ProjectInfo.cs
│       │   ├── TimelineConfig.cs
│       │   ├── Swimlane.cs
│       │   ├── Milestone.cs
│       │   ├── MilestoneType.cs             (enum: Poc, Prod, Checkpoint)
│       │   ├── HeatmapConfig.cs
│       │   └── HeatmapRows.cs
│       ├── Services/
│       │   ├── DashboardDataProvider.cs     (wraps IOptionsMonitor + validation)
│       │   └── MilestoneLayout.cs           (static: DateToX, ClampX, SwimlaneY)
│       ├── Components/
│       │   ├── App.razor
│       │   ├── Routes.razor
│       │   ├── _Imports.razor
│       │   ├── Pages/
│       │   │   ├── Dashboard.razor          (route: "/")
│       │   │   ├── Dashboard.razor.css
│       │   │   └── Error.razor              (route: "/error")
│       │   └── Shared/
│       │       ├── Header.razor
│       │       ├── Header.razor.css
│       │       ├── TimelineSvg.razor
│       │       ├── TimelineSvg.razor.css
│       │       ├── HeatmapGrid.razor
│       │       ├── HeatmapGrid.razor.css
│       │       ├── HeatmapCell.razor
│       │       └── HeatmapCell.razor.css
│       └── wwwroot/
│           └── app.css                       (global reset + body sizing)
└── tests/
    └── ReportingDashboard.Tests/             (net8.0, xUnit + bUnit)
        ├── MilestoneLayoutTests.cs
        ├── DashboardDataProviderTests.cs
        ├── DashboardRenderSmokeTests.cs
        └── Fixtures/
            ├── sample-data.json
            └── malformed-data.json
```

### Component Catalog

#### 1. `Program.cs` (composition root)

**Responsibility:** Configure Kestrel, register services, wire Razor component endpoints, install a global exception handler.

**Key configuration:**
- `builder.WebHost.UseUrls("http://127.0.0.1:5000")` - hard-bind to loopback.
- `builder.Configuration.AddJsonFile("data.json", optional:false, reloadOnChange:true)` - the single source of truth.
- `builder.Services.Configure<DashboardData>(builder.Configuration)` - bind whole root to `DashboardData`.
- `builder.Services.AddSingleton<DashboardDataProvider>()`.
- `builder.Services.AddRazorComponents()` - **no** `.AddInteractiveServerComponents()` (we render statically).
- `app.UseExceptionHandler("/error")` + `app.UseStatusCodePagesWithReExecute("/error")`.
- **No** `app.UseHttpsRedirection()`, **no** `app.UseAuthentication()`, **no** `app.UseAuthorization()`.
- `app.MapRazorComponents<App>()` - static SSR.

**Dependencies:** `Microsoft.AspNetCore.App` (8.0), `Microsoft.Extensions.Configuration.Json`.

#### 2. `DashboardData` and child records (Models)

**Responsibility:** Strongly-typed POCO tree that mirrors `data.json`. Immutable `record` types.

**Key types:**
- `DashboardData { Project, Timeline, Heatmap }`
- `ProjectInfo { Title, Subtitle, BacklogUrl?, AsOfDate }`
- `TimelineConfig { StartDate, EndDate, NowDate, Swimlanes[], MonthLabels[]? }`
- `Swimlane { Id, Label, Color, Milestones[] }`
- `Milestone { Date, Type, Label, LabelPosition? }` (enum `LabelPosition { Above, Below }`)
- `MilestoneType` enum: `Poc | Prod | Checkpoint | CheckpointMinor`
- `HeatmapConfig { Months[], CurrentMonthIndex, Rows }`
- `HeatmapRows { Shipped, InProgress, Carryover, Blockers }` (each is `IReadOnlyList<IReadOnlyList<string>>`, outer = months, inner = items)

**Dependencies:** `System.Text.Json` only. All dates bound as `DateOnly` via a `JsonConverter` (`DateOnlyJsonConverter`) registered in `Program.cs`.

#### 3. `DashboardDataProvider` (service)

**Responsibility:** A thin wrapper around `IOptionsMonitor<DashboardData>` that adds:
- Validation (required fields present, `StartDate < EndDate`, non-empty swimlanes, `HeatmapRows.*.Count == Months.Count`).
- A `CurrentOrThrow()` method that throws `DashboardDataException` with a clear message if invalid.
- An `OnChange` event surfaced through a `ChangeToken`, which the optional `FileSystemWatcher` fallback can hook.
- Logs validation warnings (e.g., milestone out of `[StartDate, EndDate]` range).

**Interface:**
```csharp
public sealed class DashboardDataProvider
{
    public DashboardData Current { get; }
    public IDisposable OnChange(Action<DashboardData> listener);
    public IReadOnlyList<string> Warnings { get; }  // e.g., out-of-range milestones
}
```

**Dependencies:** `IOptionsMonitor<DashboardData>`, `ILogger<DashboardDataProvider>`.

**Lifecycle:** Singleton. Constructed at first resolve; subscribes to `IOptionsMonitor.OnChange`.

#### 4. `MilestoneLayout` (static helper)

**Responsibility:** Pure, side-effect-free math for mapping dates to SVG x-coordinates and computing swimlane y-coordinates.

**Public surface:**
```csharp
public static class MilestoneLayout
{
    public const int SvgWidth  = 1560;
    public const int SvgHeight = 185;
    public const int FirstLaneY = 42;
    public const int LastLaneY  = 154;

    public static double DateToX(DateOnly date, DateOnly start, DateOnly end, int width = SvgWidth);
    public static (double X, bool Clamped) DateToClampedX(DateOnly date, DateOnly start, DateOnly end, int width = SvgWidth);
    public static int SwimlaneY(int index, int count);                // 42 + i*((154-42)/max(1,N-1)) when N>1 else 98
    public static IEnumerable<(string Label, double X)> MonthTicks(DateOnly start, DateOnly end, int width = SvgWidth);
}
```

Unit-tested against Scenario 4 (Jan-Jun 2026 → Mar 26 ≈ 745, Apr 19 ≈ 823).

**Dependencies:** None (stateless).

#### 5. `App.razor` + `Routes.razor`

**Responsibility:** Root component + router. Emits `<html>`/`<head>`/`<body>` with `<link href="app.css">` and nothing else. `Routes.razor` uses `<Router AppAssembly="typeof(Program).Assembly">` and a `<RouteView RouteData="@routeData" DefaultLayout="null" />` - **no layout chrome** (no nav, no sidebar, no footer).

#### 6. `Dashboard.razor` (route `"/"`)

**Responsibility:** The single page. Declares `@page "/"`, injects `DashboardDataProvider`, renders three child components inside the fixed 1920x1080 body flex-column.

**Render output (skeleton):**
```razor
@page "/"
@inject DashboardDataProvider Data
<Header Project="Data.Current.Project" />
<TimelineSvg Timeline="Data.Current.Timeline" />
<HeatmapGrid Heatmap="Data.Current.Heatmap" />
```

**Dependencies:** `DashboardDataProvider`.

**Render mode:** Static SSR (no `@rendermode` attribute).

**Error handling:** If `Data.Current` throws `DashboardDataException`, `UseExceptionHandler` redirects to `/error`.

#### 7. `Header.razor`

**Responsibility:** Renders Section 1 of the design (`.hdr`).

**Parameters:** `[Parameter] public ProjectInfo Project { get; set; }`

**Render:** title + optional backlog anchor + subtitle on the left; static legend on the right (4 legend items, hex colors hardcoded because legend labels are static per the PM spec).

**Styling:** `Header.razor.css` (scoped) - padding, flex, border-bottom `#E0E0E0`.

#### 8. `TimelineSvg.razor`

**Responsibility:** Renders Section 2 of the design (`.tl-area`). Left 230px swimlane legend + right inline `<svg width="1560" height="185">`.

**Parameters:** `[Parameter] public TimelineConfig Timeline { get; set; }`

**Render passes:**
1. `<defs><filter id="sh">` drop shadow.
2. `@foreach` month tick from `MilestoneLayout.MonthTicks` → vertical gridline + month label.
3. NOW line: `<line stroke="#EA4335" stroke-dasharray="5,3">` at `DateToX(nowDate)` + `<text>NOW</text>`.
4. `@foreach` swimlane: horizontal `<line>` at `SwimlaneY(i, N)` in swimlane color.
5. `@foreach` milestone: compute `(x, clamped)` via `DateToClampedX`; emit `<polygon>` diamond for Poc/Prod or `<circle>` for Checkpoint; emit label `<text>` above (y-16) or below (y+24) per `Milestone.LabelPosition` (default: alternating above/below within the same lane to reduce collision).

**Dependencies:** `MilestoneLayout`.

**Styling:** `TimelineSvg.razor.css` for the `.tl-area`, `.tl-svg-box`, and left-column flex layout. SVG styling is inline (stroke, fill) because Blazor scoped CSS does not reliably cascade into SVG child elements - see Risk #3 below.

#### 9. `HeatmapGrid.razor`

**Responsibility:** Renders Section 3 of the design (`.hm-wrap`). Title + CSS Grid of (1 + N_months) × (1 + 4) cells.

**Parameters:** `[Parameter] public HeatmapConfig Heatmap { get; set; }`

**Render logic:**
- Emits `<div class="hm-wrap">` → title `<div class="hm-title">` → `<div class="hm-grid" style="grid-template-columns:160px repeat(@N,1fr);">`.
- Header row: `.hm-corner` + `@for` month → `<div class="hm-col-hdr @(isCurrent ? "apr-hdr" : "")">@month</div>`.
- 4 data rows (`ship`, `prog`, `carry`, `block`), each: `<div class="hm-row-hdr @rowClass-hdr">` + `@for` month → `<HeatmapCell Items="row[m]" RowClass="@rowClass" IsCurrentMonth="m == currentIndex" />`.

**Dependencies:** `HeatmapCell`.

#### 10. `HeatmapCell.razor`

**Responsibility:** Renders a single cell. Composes class `hm-cell @RowClass-cell @(IsCurrentMonth ? "apr" : "")`. Iterates items emitting `<div class="it">@item</div>`. If `Items.Count == 0`, emits a dim dash `<div class="it empty">—</div>` (per Scenario 7). Overflow is clipped by CSS (`overflow:hidden`).

**Parameters:** `Items`, `RowClass` ("ship" | "prog" | "carry" | "block"), `IsCurrentMonth`.

#### 11. `Error.razor` (route `"/error"`)

**Responsibility:** Developer-facing error page. Reads the `IExceptionHandlerFeature` from `HttpContext`, displays the offending file (`data.json`), the exception type, and the message (including `JsonException.LineNumber` / `BytePositionInLine` for malformed JSON, and `FileNotFoundException.FileName` for missing file). No styling - plain `<pre>` is sufficient; this is never screenshotted.

#### 12. `wwwroot/app.css`

**Responsibility:** Global reset + body sizing. Copied verbatim from `OriginalDesignConcept.html`:
```css
*{margin:0;padding:0;box-sizing:border-box;}
body{width:1920px;height:1080px;overflow:hidden;background:#FFFFFF;
     font-family:'Segoe UI','Selawik',Arial,sans-serif;color:#111;
     display:flex;flex-direction:column;}
a{color:#0078D4;text-decoration:none;}
```

All section-specific CSS lives in component-scoped `*.razor.css` files, also copied verbatim from the reference.

#### 13. `data.json`

**Responsibility:** Single content source. Sits next to the executable (content-root). `CopyToOutputDirectory=PreserveNewest` in the `.csproj`. A ship-ready sample is included covering all 3 milestone types and all 4 heatmap rows.

#### 14. `ReportingDashboard.Tests`

**Responsibility:** Minimal test coverage. Three test classes:

- **`MilestoneLayoutTests`** - `DateToX`, `DateToClampedX`, `SwimlaneY` for N=1..5, `MonthTicks` for Jan-Jun 2026. Spot-checks: Mar 26 2026 on [Jan 1, Jun 30] → x ≈ 745 ± 2; Apr 19 → x ≈ 823 ± 2.
- **`DashboardDataProviderTests`** - loads valid fixture → no warnings; loads malformed fixture → throws `DashboardDataException` with descriptive message; loads fixture with milestone outside range → logs warning and clamps.
- **`DashboardRenderSmokeTests`** (bUnit) - `RenderComponent<Dashboard>()` with a test-injected `DashboardDataProvider`; asserts the DOM contains the project title, one `<svg>`, and exactly 4 `.hm-row-hdr` elements.

**Dependencies:** xUnit 2.9.x, bUnit 1.33.x, FluentAssertions 6.12.x, coverlet.collector 6.0.x.

---

## UI Component Architecture

This section maps each visual section of `OriginalDesignConcept.html` to a specific Razor component, its layout strategy, data bindings, and interactions. **There are no interactions** in MVP - every component is presentational/static. Hover, click, drag, and keyboard events are explicit non-goals (Scenario 12).

| # | Visual Section (CSS selector)    | Component              | Layout Strategy                                                                                               | Data Bindings                                                                                                       | Interactions |
|---|----------------------------------|------------------------|---------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------|--------------|
| 1 | `.hdr` left block                | `Header.razor`         | Flexbox row; padding `12px 44px 10px`; `border-bottom:1px solid #E0E0E0`; `flex-shrink:0`                      | `Project.Title` → `<h1>`; `Project.BacklogUrl` → `<a>` (omit if null/empty); `Project.Subtitle` → `.sub` div         | Backlog anchor `<a href>` opens in new tab (`target="_blank" rel="noopener"`) |
| 2 | `.hdr` right legend              | `Header.razor` (inline)| Flexbox row `gap:22px align-items:center`; each item flex row `gap:6px font-size:12px`                         | **Static** - 4 legend entries hardcoded (yellow diamond, green diamond, gray circle, red bar). Not in `data.json`.   | None |
| 3 | `.tl-area` left swimlane legend  | `TimelineSvg.razor`    | Inline flex-column 230px, `justify-content:space-around padding:16px 12px 16px 0 border-right:1px solid #E0E0E0` | `Timeline.Swimlanes[i].Id` (colored, 12px/600), `Swimlanes[i].Label` (12px/400 `#444`); lane color = `Swimlane.Color` | None |
| 4 | `.tl-svg-box` month gridlines    | `TimelineSvg.razor`    | Inline `<svg width=1560 height=185 overflow:visible>`; gridlines as `<line>` stroke `#bbb` 0.4 opacity          | `MilestoneLayout.MonthTicks(StartDate, EndDate)` enumerates (label, x); labels at 11px/600 `#666` Segoe UI x+5         | None |
| 5 | NOW line                         | `TimelineSvg.razor`    | `<line stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3" />` at `DateToX(NowDate)`                      | `Timeline.NowDate`                                                                                                   | None |
| 6 | Swimlane horizontal lines        | `TimelineSvg.razor`    | Per-lane `<line x1=0 x2=1560 stroke-width=3 stroke="@Swimlane.Color" />` at `SwimlaneY(i,N)`                   | `Timeline.Swimlanes` enumeration                                                                                     | None |
| 7 | Milestone glyphs (PoC/Prod/Chk)  | `TimelineSvg.razor`    | PoC/Prod: `<polygon points="x,y-11 x+11,y x,y+11 x-11,y" filter="url(#sh)" fill="#F4B400"/#34A853">`; Checkpoint: `<circle r=7 fill=white stroke=@laneColor stroke-width=2.5>` (major) or `<circle r=4 fill=#999>` (minor) | `Swimlane.Milestones[]`: `Milestone.Date` → x via `DateToClampedX`; `Milestone.Type` → glyph                          | None |
| 8 | Milestone text labels            | `TimelineSvg.razor`    | `<text text-anchor=middle font-size=10 fill=#666 font-family="Segoe UI,Arial">` at `y-16` (above) or `y+24` (below) | `Milestone.Label`; position alternates per lane by milestone index parity (or `LabelPosition` override)              | None |
| 9 | `.hm-title`                      | `HeatmapGrid.razor`    | `margin-bottom:8px flex-shrink:0`; 14px/700 `#888` uppercase `.5px` letter-spacing                             | **Static string** "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"                         | None |
| 10 | `.hm-grid` header row            | `HeatmapGrid.razor`    | CSS Grid cell `.hm-corner` + N `.hm-col-hdr`; header row height 36px                                           | `Heatmap.Months[]`; `Heatmap.CurrentMonthIndex` toggles `.apr-hdr` class on one column                                | None |
| 11 | `.hm-row-hdr` (4 rows)           | `HeatmapGrid.razor`    | Grid cells with themed classes `.ship-hdr .prog-hdr .carry-hdr .block-hdr`                                     | **Static labels** "▲ Shipped", "◆ In Progress", "↻ Carryover", "⚠ Blockers"                                         | None |
| 12 | `.hm-cell` data cells            | `HeatmapCell.razor`    | Grid cell `padding:8px 12px overflow:hidden`; themed classes `.ship-cell .prog-cell .carry-cell .block-cell`; current-month variant adds `.apr` | `Heatmap.Rows.{Shipped/InProgress/Carryover/Blockers}[monthIndex]` (a list of strings) | None |
| 13 | `.it` item line + bullet          | `HeatmapCell.razor`    | Flow layout; `padding:2px 0 2px 12px`; `::before` 6x6 circle absolute `left:0 top:7px` in row accent color     | `item` text as `@item`                                                                                                | None |
| 14 | Empty cell                       | `HeatmapCell.razor`    | Single `<div class="it empty">—</div>` at `#AAA` when `Items.Count == 0`                                       | `Items.Count == 0`                                                                                                   | None |

**Layout strategy summary:**
- **Outer shell:** `<body>` is `flex-direction:column` with three stacked children: Header (natural height), Timeline (fixed 196px), Heatmap (`flex:1 min-height:0`). Body is locked to 1920x1080 with `overflow:hidden`.
- **Header:** Flexbox row, `justify-content:space-between`.
- **Timeline:** Flexbox row (230px fixed left legend + flex:1 SVG area). The SVG itself uses absolute pixel coordinates (1560x185) - all "layout" inside the SVG is math-computed x/y.
- **Heatmap:** CSS Grid `grid-template-columns:160px repeat(N,1fr) grid-template-rows:36px repeat(4,1fr)`.

**Scoped CSS vs global CSS:** Each component has a `*.razor.css` file for its own section styling. The global reset + body sizing + link color live in `wwwroot/app.css`. **Exception:** SVG interior styling (stroke, fill, filter) is applied via inline attributes, not CSS classes, because Blazor's scoped-CSS attribute selectors do not reliably match SVG child elements in all browsers. This is explicitly called out in `TimelineSvg.razor` code comments.

---

## Component Interactions

There is no traditional request/response flow - the only public endpoint is `GET /`. The "interactions" are startup, render, and file-change observation.

### 1. Startup flow

```
dotnet run
  │
  ▼
Program.cs
  │  ├─ builder.Configuration.AddJsonFile("data.json", optional:false, reloadOnChange:true)
  │  ├─ builder.Services.Configure<DashboardData>(builder.Configuration)
  │  ├─ builder.Services.AddSingleton<DashboardDataProvider>()
  │  ├─ builder.Services.AddRazorComponents()
  │  └─ builder.WebHost.UseUrls("http://127.0.0.1:5000")
  │
  ▼
Kestrel listening on 127.0.0.1:5000
  │
  ▼  (cold start < 5s)
Ready to serve
```

Startup fails fast if `data.json` is missing (because `optional:false`). The exception propagates out of `app.Run()` and the process exits with non-zero code and a logged message: `"data.json not found next to executable."` (Scenario 11)

### 2. Render flow (Scenario 1)

```
Browser GET http://localhost:5000/
  │
  ▼
Kestrel ──▶ Endpoint routing ──▶ RazorComponentsEndpoint
  │
  ▼
App.razor ──▶ Routes.razor ──▶ matches "/" ──▶ Dashboard.razor
  │
  ▼  (static SSR - no circuit)
Dashboard.razor
  │  @inject DashboardDataProvider Data
  │
  ├─▶ <Header Project="Data.Current.Project" />
  │     └─ renders .hdr block
  │
  ├─▶ <TimelineSvg Timeline="Data.Current.Timeline" />
  │     ├─ calls MilestoneLayout.MonthTicks(start, end)
  │     ├─ calls MilestoneLayout.SwimlaneY(i, N) per lane
  │     └─ calls MilestoneLayout.DateToClampedX(date, start, end) per milestone
  │
  └─▶ <HeatmapGrid Heatmap="Data.Current.Heatmap" />
        └─ 4 × N HeatmapCell components, each rendering bullet items
  │
  ▼
HTML response flushed to browser
  │
  ▼  (< 500ms FCP on localhost)
Rendered 1920x1080 dashboard
```

No SignalR circuit is opened. No client-to-server callbacks. The response is a complete static HTML document with inline SVG and scoped-CSS stylesheets referenced via `<link>` tags auto-emitted by the scoped-CSS bundler (`ReportingDashboard.Web.styles.css`).

### 3. Hot reload flow (Scenario 9)

```
User edits data.json in editor, saves
  │
  ▼
File-system write event
  │
  ▼
reloadOnChange:true watcher (built into JsonConfigurationProvider)
  │
  ▼
IOptionsMonitor<DashboardData>.OnChange fires
  │
  ▼
DashboardDataProvider.Current is now the new data
  │
  ▼
(MVP) - nothing else; user hits F5
  │
  ▼
Browser re-requests "/" → full re-render flow above → new content
```

**MVP contract:** F5 is required to see updates (the PM spec explicitly allows this and says live push via SignalR is *not* guaranteed).

**Phase 2 enhancement (optional):** If `reloadOnChange` proves unreliable on certain editors (e.g., editors that write via rename), add a `FileSystemWatcher` in `DashboardDataProvider` watching `data.json` with `NotifyFilters.LastWrite | NotifyFilters.FileName`. On change, manually re-read and re-deserialize; fire `OnChange`. Still requires F5 in static SSR; only matters if we later flip to Interactive Server and want server-push.

### 4. Error flow (Scenarios 7, 10, 11)

```
Malformed data.json saved
  │
  ▼
IOptionsMonitor re-binds → JsonException thrown lazily on next .Current access
  │
  ▼
Browser requests "/" → Dashboard.razor calls Data.Current → DashboardDataException thrown
  │
  ▼
ExceptionHandlerMiddleware catches → redirects internally to /error
  │
  ▼
Error.razor reads IExceptionHandlerFeature.Error
  │  ├─ extracts JsonException.LineNumber, BytePositionInLine, Path
  │  └─ or FileNotFoundException.FileName
  │
  ▼
Renders <pre> with file path + line/column + message
```

The error page is intentionally unstyled - it's a developer diagnostic, not something that will be screenshotted.

### 5. Out-of-range milestone warning (Scenario 14)

```
data.json has a milestone date before timeline.startDate
  │
  ▼
DashboardDataProvider.Validate() detects it
  │
  ├─ Adds a message to Warnings list
  ├─ Logs via ILogger.LogWarning
  └─ Renders continue - MilestoneLayout.DateToClampedX returns (0, true) or (width, true)
  │
  ▼
Page renders; glyph is clamped to edge; browser console shows warning via <script> emitted
only when Warnings is non-empty (a small `<script>console.warn(...)</script>` block in Dashboard.razor;
this is *emitted* JS but not custom application JS per the PM spec's zero-JS goal)
```

**Alternative (preferred, truly zero-JS):** Warnings are logged server-side only (ILogger → Console). PM spec's "emit a console warning" requirement refers to the *server console*, not the browser console. We implement the server-console-only approach; documented in README.

---

## Data Model

There is no database. The data model is a single JSON document (`data.json`) bound to an immutable C# `record` tree via `System.Text.Json`.

### Entity Diagram

```
DashboardData (root)
├── Project : ProjectInfo
│     ├── Title       : string                (required)
│     ├── Subtitle    : string                (required)
│     ├── BacklogUrl  : string?               (optional; anchor omitted if null/empty)
│     └── AsOfDate    : DateOnly              (required)
│
├── Timeline : TimelineConfig
│     ├── StartDate   : DateOnly              (required; must be < EndDate)
│     ├── EndDate     : DateOnly              (required)
│     ├── NowDate     : DateOnly              (required; clamped to [Start, End] if out of range)
│     ├── MonthLabels : string[]?             (optional; auto-derived from Start..End if null)
│     └── Swimlanes   : Swimlane[]            (required; 1..N, default 3)
│            ├── Id             : string       (required, e.g., "M1")
│            ├── Label          : string       (required)
│            ├── Color          : string       (required, hex; e.g., "#0078D4")
│            └── Milestones     : Milestone[]  (0..N)
│                   ├── Date          : DateOnly               (required)
│                   ├── Type          : MilestoneType enum     (poc | prod | checkpoint | checkpointMinor)
│                   ├── Label         : string                 (required)
│                   └── LabelPosition : LabelPosition enum?    (above | below; optional, auto-alternated if null)
│
└── Heatmap : HeatmapConfig
      ├── Months             : string[]       (required, length = N; default 4)
      ├── CurrentMonthIndex  : int            (required; 0..N-1)
      └── Rows               : HeatmapRows
            ├── Shipped     : string[][]      (outer length = N months; inner = items per month)
            ├── InProgress  : string[][]
            ├── Carryover   : string[][]
            └── Blockers    : string[][]
```

### JSON Schema (sample, matches the PM spec exactly)

```json
{
  "Project": {
    "Title": "Privacy Automation Release Roadmap",
    "Subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "BacklogUrl": "https://ado.example.com/backlog",
    "AsOfDate": "2026-04-19"
  },
  "Timeline": {
    "StartDate": "2026-01-01",
    "EndDate": "2026-06-30",
    "NowDate": "2026-04-19",
    "Swimlanes": [
      {
        "Id": "M1",
        "Label": "Chatbot & MS Role",
        "Color": "#0078D4",
        "Milestones": [
          { "Date": "2026-01-12", "Type": "checkpoint",       "Label": "Jan 12" },
          { "Date": "2026-03-26", "Type": "poc",              "Label": "Mar 26 PoC" },
          { "Date": "2026-04-15", "Type": "prod",             "Label": "Apr Prod (TBD)" }
        ]
      },
      {
        "Id": "M2",
        "Label": "PDS & Data Inventory",
        "Color": "#00897B",
        "Milestones": [
          { "Date": "2026-02-11", "Type": "checkpoint",       "Label": "Feb 11" },
          { "Date": "2026-04-02", "Type": "poc",              "Label": "Apr 2 PoC" }
        ]
      },
      {
        "Id": "M3",
        "Label": "Auto Review DFD",
        "Color": "#546E7A",
        "Milestones": [
          { "Date": "2026-03-10", "Type": "checkpointMinor",  "Label": "" },
          { "Date": "2026-05-22", "Type": "prod",             "Label": "May Prod" }
        ]
      }
    ]
  },
  "Heatmap": {
    "Months": ["Jan", "Feb", "Mar", "Apr"],
    "CurrentMonthIndex": 3,
    "Rows": {
      "Shipped":    [ ["Consent API v1"], ["MS Role GA"], ["PDS schema v2"], ["Chatbot beta"] ],
      "InProgress": [ [],                 ["Review DFD spike"], ["Auto-Review v0"], ["Chatbot RC"] ],
      "Carryover":  [ [],                 [],            ["Telemetry dashboard"], ["Docs cleanup"] ],
      "Blockers":   [ [],                 [],            [],                  ["Legal review of AI prompts"] ]
    }
  }
}
```

### Relationships & Invariants

- `Timeline.Swimlanes` 1..N (default N=3). Order is preserved; index determines vertical position via `SwimlaneY(i, N)`.
- `Swimlane.Milestones` 0..N. Order does not matter for rendering (each has absolute date); order can be used to tiebreak label above/below alternation.
- `Heatmap.Rows.*` all must have length equal to `Heatmap.Months.Length`. Validated in `DashboardDataProvider.Validate()`.
- `Heatmap.CurrentMonthIndex` must be in `[0, Months.Length - 1]`.
- `Timeline.NowDate` may be outside `[StartDate, EndDate]`; if so, the NOW line is clamped to the edge and a warning is logged.
- **No foreign keys, no joins, no denormalization.** The JSON is the wire format, the storage format, and the render format.

### Storage

- **Single file:** `data.json` co-located with the executable (content root).
- `.csproj`: `<None Update="data.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>`.
- **No database, no EF Core, no SQLite, no blob storage, no local DB file.**
- The existing `agentsquad_azurenerd_ReportingDashboard.db` file in the runner's working directory is **unrelated** to this architecture and is not touched.
- **Serialization:** `System.Text.Json` via `Microsoft.Extensions.Configuration.Json`. A custom `DateOnlyJsonConverter` is registered to handle `"YYYY-MM-DD"` strings as `DateOnly`. Enum binding is case-insensitive via `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)`.

---

## API Contracts

**There is no public API** in the traditional sense. The application exposes only the following endpoints, all served by Kestrel on `127.0.0.1:5000`.

| Method | Path                                 | Rendered by            | Purpose                                                                                                          |
|--------|--------------------------------------|------------------------|------------------------------------------------------------------------------------------------------------------|
| GET    | `/`                                  | `Dashboard.razor`      | The dashboard. Response: `200 OK text/html; charset=utf-8` with a complete static HTML document.                  |
| GET    | `/error`                             | `Error.razor`          | Error page. Response: `500 Internal Server Error text/html` with `<pre>` describing the underlying exception.     |
| GET    | `/_framework/*`                      | Blazor framework       | Framework asset delivery (blazor.web.js, etc.). **Not our code.**                                                 |
| GET    | `/ReportingDashboard.Web.styles.css` | Scoped-CSS bundler     | Auto-emitted aggregate of all `*.razor.css` files.                                                                |
| GET    | `/app.css`                           | Static file middleware | Global reset + body sizing.                                                                                       |
| GET    | `/favicon.ico`                       | Static file middleware | Standard favicon (optional; 404 is harmless).                                                                     |

**No REST API.** **No gRPC.** **No custom SignalR hubs** (the only SignalR traffic would be Blazor's own circuit hub, and we disable that by using Static SSR). **No JSON endpoints returning `data.json`** - the data is rendered server-side only.

### Response headers

Default ASP.NET Core headers only. No CORS configuration (there are no cross-origin clients). No CSP headers (not required for local-only use - documented as non-goal).

### Error handling contract

| Scenario                                          | HTTP status | Response body                                                                                    |
|---------------------------------------------------|-------------|--------------------------------------------------------------------------------------------------|
| `data.json` missing at startup                    | N/A         | Process exits; `ILogger.LogCritical("data.json not found at <path>")`.                            |
| `data.json` malformed, detected on render         | 500         | `Error.razor` shows file name, `JsonException.Path`, `LineNumber`, `BytePositionInLine`.          |
| Required field missing (e.g., `Timeline.StartDate`) | 500       | `Error.razor` shows `DashboardDataException("Timeline.StartDate is required")`.                   |
| Milestone date outside `[StartDate, EndDate]`     | 200         | Renders with glyph clamped to edge; server logs a warning via `ILogger.LogWarning`.               |
| `Heatmap.Rows.Shipped.Length != Months.Length`    | 500         | `Error.razor` shows `DashboardDataException("Heatmap row 'Shipped' has 3 months, expected 4")`.   |
| `CurrentMonthIndex` out of range                  | 500         | `Error.razor` explains.                                                                            |
| Request to any other path                         | 404         | Default ASP.NET Core 404 re-executed through `/error`, which displays a simple "Not Found".       |

**No retry semantics, no rate limits, no auth tokens** - this is a single-user localhost tool.

---

## Infrastructure Requirements

### Runtime environment

| Concern           | Requirement                                                                                            |
|-------------------|--------------------------------------------------------------------------------------------------------|
| OS                | Windows 10/11 (primary screenshot target). macOS and Linux supported functionally but Segoe UI fidelity not guaranteed. |
| Runtime           | .NET 8 SDK (8.0.100 or later LTS).                                                                     |
| Fonts             | Segoe UI (Windows built-in). Fallback `Selawik` → `Arial` → `sans-serif`.                              |
| Browser           | Chromium-based (Edge, Chrome). Firefox acceptable. 1920x1080 viewport required for screenshot fidelity.|
| Disk              | <50 MB for solution + dependencies.                                                                    |
| Memory            | <200 MB at runtime.                                                                                    |
| Network           | None at runtime. Compile-time only for initial `dotnet restore`.                                       |

### Hosting

- **`dotnet run`** from solution root or `src/ReportingDashboard.Web/`.
- **`dotnet watch run`** for development (automatic rebuild on `.razor`/`.cs` change, automatic `IOptionsMonitor` reload on `data.json` change).
- **Kestrel** bound to `http://127.0.0.1:5000` via `builder.WebHost.UseUrls("http://127.0.0.1:5000")`. Not reachable from the LAN.
- **No reverse proxy** (no IIS, no nginx).
- **No HTTPS.** `app.UseHttpsRedirection()` is deliberately omitted.
- **No containerization.** No Dockerfile in MVP.

### Networking

- **Inbound:** Single TCP listener on 127.0.0.1:5000. Explicitly not 0.0.0.0.
- **Outbound:** None at runtime.
- **Firewall:** No inbound rule required (loopback-only).

### Storage

- **`data.json`** co-located with the executable. Hot-reloadable.
- **No database files.**
- **Logs:** stdout/stderr only, no file-based logging in MVP.

### Distribution (optional, Phase 2+)

- `dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true` produces a single `.exe` that can be copied to a teammate's machine and run without installing .NET. Include `data.json` alongside.

### CI/CD

- **Not required for MVP.** Repo developers use `dotnet build` and `dotnet test` locally.
- **Optional GitHub Actions workflow** (if requested by the team): on PR, run `dotnet restore`, `dotnet build -c Release`, `dotnet test`. No deployment step, no container build, no artifact publish. File: `.github/workflows/ci.yml`. ~30 lines.

### Observability

- **Logging:** `ILogger` → default `ConsoleLoggerProvider`. Levels: `Warning` for data validation issues, `Information` for startup, `Debug` off by default.
- **Metrics:** None.
- **Tracing:** None. No OpenTelemetry, no Application Insights.
- **Health checks:** None. Not applicable for a single-user local tool.

---

## Technology Stack Decisions

All decisions conform to the mandatory stack: **C# .NET 8, Blazor Server, local only, no cloud, single `.sln`.**

| Concern                          | Decision                                                                       | Justification                                                                                                                                                               |
|----------------------------------|--------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Runtime / Framework              | **.NET 8.0 LTS**                                                                | Mandated. LTS until Nov 2026.                                                                                                                                                |
| Web framework                    | **ASP.NET Core 8.0 + Razor Components (Blazor Server project)**                 | Mandated.                                                                                                                                                                    |
| Rendering mode                   | **Static Server-Side Rendering** (no `@rendermode InteractiveServer`)           | Page is 100% static/presentational. Static SSR eliminates SignalR circuit overhead and the "reconnecting" UI overlay that can interfere with screenshots. One-attribute flip if interactivity needed. |
| Web host                         | **Kestrel** on `http://127.0.0.1:5000`                                          | Built in; no external dependency. Loopback bind satisfies NFR security.                                                                                                      |
| Configuration + hot reload       | **`Microsoft.Extensions.Configuration.Json` + `IOptionsMonitor<DashboardData>`** with `reloadOnChange:true` | Built-in, zero dependencies, already wired to file system watchers. Meets Story 5 AC.                                                                                |
| JSON serialization               | **`System.Text.Json`** (built-in)                                               | No Newtonsoft needed. Fast, AOT-friendly. Custom `DateOnlyJsonConverter` for dates.                                                                                          |
| Dates                            | **`DateOnly`**                                                                  | Timeline is per-day resolution; `DateOnly` avoids timezone bugs that `DateTime` would introduce.                                                                              |
| Layout                           | **CSS Grid + Flexbox**, hand-written                                            | Matches the reference design exactly. No library fights the custom visual.                                                                                                   |
| Charting / visualization         | **Inline SVG authored in Razor** (no library)                                   | Gantt timeline is not a standard chart shape; the reference uses hand-drawn glyphs at specific pixel offsets. Libraries (ChartJS, Plotly, ApexCharts, LiveCharts2) would add JS/dependency weight and fight the custom styling. |
| CSS strategy                     | **Component-scoped CSS** (`*.razor.css`) + single global `app.css`              | Built-in Blazor feature. No Sass/Less/Tailwind.                                                                                                                               |
| Icons                            | **None required**                                                                | Design uses geometric shapes only (diamonds, circles, bars).                                                                                                                  |
| JS interop                       | **None**                                                                         | Explicit PM non-goal. "Zero custom JS" success metric.                                                                                                                        |
| Component library                | **None**                                                                         | MudBlazor/Radzen/Blazorise impose theming that fights the custom design.                                                                                                     |
| Logging                          | **`Microsoft.Extensions.Logging.Console`**                                      | Built-in, sufficient for local dev.                                                                                                                                          |
| Storage                          | **Single `data.json`** on disk; copied to output                                | Explicit PM non-goal: no database.                                                                                                                                            |
| Testing framework                | **xUnit 2.9.x**                                                                  | Microsoft-standard test runner for .NET.                                                                                                                                      |
| Component testing                | **bUnit 1.33.x**                                                                 | Only mature option for testing Razor components.                                                                                                                              |
| Assertions                       | **FluentAssertions 6.12.x**                                                      | Readable test failures.                                                                                                                                                       |
| Coverage                         | **coverlet.collector 6.0.x** (optional)                                          | Standard .NET coverage tool.                                                                                                                                                  |
| Tooling                          | **Visual Studio 2022 17.8+**, **VS Code + C# Dev Kit**, or **Rider 2024.x**      | Developer choice; all three work with .NET 8 + Blazor.                                                                                                                        |
| Code style                       | **EditorConfig** + built-in .NET 8 analyzers + `dotnet format`                   | Minimal overhead; no StyleCop for a ~900 LOC project.                                                                                                                         |

### Explicit rejections (documented so the team doesn't re-debate them)

- **No MudBlazor / Radzen / Blazorise** - component theming fights the custom design.
- **No Chart.js / Plotly / ApexCharts / LiveCharts2 / ChartJs.Blazor** - custom SVG is simpler and pixel-accurate.
- **No Tailwind / Bootstrap / Sass** - plain CSS suffices and matches the reference verbatim.
- **No Entity Framework Core / Dapper / SQLite** - `data.json` is the data store.
- **No AutoMapper** - POCOs are bound directly from configuration.
- **No Serilog / NLog** - default console logger is sufficient.
- **No MediatR / FluentValidation** - a single hand-written `Validate()` method is smaller than the library setup.
- **No Newtonsoft.Json** - `System.Text.Json` handles everything.
- **No Docker / Kubernetes / Helm** - local only.
- **No Azure / AWS / GCP** - explicit non-goal.

---

## Security Considerations

This is a **single-user local developer tool**. The security model is intentionally minimal; enterprise controls (auth, HTTPS, CSP, auditing) are explicit non-goals per the PM spec. Below is the explicit posture.

### Network exposure

- **Kestrel bound to `http://127.0.0.1:5000`** via `builder.WebHost.UseUrls("http://127.0.0.1:5000")`. This uses the loopback adapter only. The app is not reachable from other machines on the LAN or VPN.
- The `launchSettings.json` profiles (both "http" and any IIS Express profile) use `127.0.0.1:5000` - `localhost:*` is avoided because on some Windows configurations `localhost` resolves to `::1` and opens the IPv6 socket in addition to IPv4, which can be broader than intended.
- No `UseHttpsRedirection()`.
- No inbound firewall rule is created; Windows' default block for non-loopback traffic to `127.0.0.1:5000` is the enforcement.

### Authentication / Authorization

- **None.** No login page, no cookies, no identity middleware. Navigating to `/` immediately renders the dashboard.
- No authorization policies. No `[Authorize]` attributes.

### Transport security

- **HTTP only.** HTTPS is not configured. Acceptable because:
  - Traffic never leaves the local machine.
  - No credentials, tokens, or PII in transit.
  - No external networks can reach the socket.
- No HSTS, no HPKP, no secure cookies (no cookies at all).

### Input validation

- `data.json` is the only input. Validated at load/bind time:
  - Required properties present (null checks).
  - `StartDate < EndDate`.
  - `Swimlanes.Count >= 1`.
  - `Heatmap.Rows.*.Count == Months.Count` (all 4 rows).
  - `0 <= CurrentMonthIndex < Months.Count`.
  - `Swimlane.Color` matches `^#[0-9A-Fa-f]{6}$` (prevents CSS injection via the `style` attribute - see below).
- Enum values (`MilestoneType`, `LabelPosition`) validated via `JsonStringEnumConverter` - unknown values throw.
- All string values from `data.json` are rendered via Razor's default `@value` expression, which HTML-encodes. **No `@Html.Raw` or `MarkupString` usage anywhere.** This prevents any XSS that a malicious `data.json` could otherwise inject.
- **Swimlane color validation:** because `Swimlane.Color` is injected into an inline SVG `stroke="@color"` attribute, we validate it strictly matches `#XXXXXX` before rendering. This prevents attribute-injection (e.g., `" onload="alert(1)`).
- **Backlog URL validation:** `Project.BacklogUrl` is parsed via `Uri.TryCreate(url, UriKind.Absolute, out _)` and restricted to `http` / `https` / `mailto` schemes before being emitted as `href`. Prevents `javascript:` scheme injection.

### Data protection

- `data.json` must contain **fictional/non-sensitive content only** (explicit PM requirement). README documents this clearly. There is no encryption, no secret store (none needed - no secrets exist).
- No logging of `data.json` contents beyond summary counts (e.g., "loaded 3 swimlanes, 9 milestones") - avoids leaking anything sensitive into console output if the user accidentally puts sensitive data in.

### Antiforgery

- Blazor's built-in antiforgery token generator is left enabled (default). Since there are no POST forms in the app, it has no effect on functionality but remains on for defense-in-depth in case a future interactive component is added.

### CSP / Other security headers

- **Not configured.** Acceptable for local-only, single-user use. Documented as non-goal.

### Dependencies / Supply chain

- All NuGet packages come from `nuget.org`:
  - `Microsoft.AspNetCore.App` (framework reference, no explicit package).
  - `Microsoft.Extensions.Configuration.Json` (8.0.x).
  - Test packages: `xunit`, `xunit.runner.visualstudio`, `bunit`, `FluentAssertions`, `coverlet.collector`.
- No preview/alpha packages.
- Consider enabling `dotnet list package --vulnerable` check in CI.

### Threat model summary

| Threat                                        | Applicable? | Mitigation                                                                                          |
|-----------------------------------------------|-------------|-----------------------------------------------------------------------------------------------------|
| Remote attacker on LAN                        | No          | Kestrel bound to 127.0.0.1.                                                                         |
| Malicious `data.json` (XSS)                   | Yes (low)   | Razor HTML-encoding everywhere; no `MarkupString`; color + URL + enum validation.                    |
| Dependency vulnerabilities                    | Low         | Minimal dependencies, all from official feeds; optional `dotnet list package --vulnerable` in CI.    |
| Secret leak via `data.json` check-in          | Yes (policy)| README prohibits real PII/secrets in `data.json`; sample file contains only fictional content.       |
| DoS from extremely large `data.json`          | Negligible  | Local tool; user controls their own file. No soft limit enforced.                                    |
| Browser-side code execution from framework JS | Accepted    | Blazor framework JS is first-party Microsoft code.                                                    |

---

## Scaling Strategy

**Scaling is explicitly not a requirement.** This is a single-user local tool that serves one request per screenshot. The PM spec lists "Scalability: Not applicable. Single user, single page, single project." under NFRs.

### Capacity profile

- **Concurrent users:** 1 (the developer at their own machine).
- **Requests per second:** <1 (bursty: one GET per browser refresh).
- **Peak data size:** `data.json` expected to be <50 KB; no enforced upper bound.
- **Render time budget:** <500 ms FCP, <1 s complete.

### Vertical scaling

Not relevant. Any machine that runs .NET 8 comfortably (Win10/11 with ≥4 GB RAM) will run this app.

### Horizontal scaling

Not applicable. Each user runs their own local instance. Multiple executives running the app simultaneously on their own laptops is "horizontal" in the trivial sense - no shared state, no coordination needed.

### Data scaling

- `Timeline.Swimlanes`: rendering cost is O(lanes × milestones). Practical upper bound on a 1560x185 SVG for visual clarity: ~10 lanes, ~20 milestones per lane. Well below any performance concern.
- `Heatmap.Months`: grid grows horizontally with month count. Practical upper bound at 1920px wide: ~12 months before text wraps awkwardly. Validated at render; no hard limit.
- **No pagination, no virtualization, no lazy loading.** Everything renders in one pass.

### If requirements change later

- **Multi-project selector:** Introduce a `projects/` folder of `*.json` files and a dropdown; still single-user, still no backend scaling.
- **Team-shared hosting:** The app could be containerized and hosted behind a reverse proxy. Blazor Server scales via sticky sessions; Static SSR scales statelessly (trivial). Not in scope; called out in Phase 4.
- **Screenshot automation:** Add a Playwright CLI that spins up a headless Chromium, navigates to `http://127.0.0.1:5000/`, saves PNG. Scales with number of dashboards processed in a batch. Not in scope for MVP.

---

## Risks & Mitigations

| # | Risk                                                                                          | Likelihood | Impact  | Mitigation                                                                                                                                                                                                       |
|---|-----------------------------------------------------------------------------------------------|------------|---------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 1 | **Pixel drift from `OriginalDesignConcept.html`** - the #1 product risk, since screenshot fidelity is the product. | High       | High    | Copy all CSS verbatim from the reference into `app.css` + `*.razor.css`. Use exact hex values. Build + screenshot at 1920x1080 DevTools viewport. Side-by-side compare against `OriginalDesignConcept.png` before each merge. Include a manual visual-diff checklist in the PR template. |
| 2 | **Segoe UI unavailable on non-Windows dev/CI machines** causes font metric drift.              | Medium     | Medium  | Font stack `'Segoe UI','Selawik',Arial,sans-serif`. README declares Windows as the canonical screenshot host. Tests do not assert pixel widths - only structural assertions.                                       |
| 3 | **Scoped CSS does not cascade into SVG children** in some browsers (Blazor adds `b-xxxxx` attribute selectors that SVG ignores). | Medium     | Medium  | Use inline SVG attributes (`stroke`, `fill`, `filter`) rather than CSS classes for SVG internals. Keep `*.razor.css` for the outer containers only. Documented in code comments near each SVG block.             |
| 4 | **`reloadOnChange` misses file writes** from editors that rename-over-original (some editors/IDEs). | Low      | Low     | Phase 2 fallback: add a `FileSystemWatcher` in `DashboardDataProvider` with `NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size`. F5-to-refresh still works without it. Documented in README. |
| 5 | **Truncated SVG in the design reference** means some timeline glyph math is inferred.          | Medium     | Medium  | Reconstructed from the visible portion + the rendered `OriginalDesignConcept.png`. All glyph dimensions (diamond 22px, circle r=7/4) are parameters in `TimelineSvg.razor` and can be tuned quickly.               |
| 6 | **Malformed `data.json` bricks the app** (half-render, crash loop).                            | Medium     | High    | `DashboardDataProvider.Validate()` throws `DashboardDataException` with a specific field-path message; `UseExceptionHandler("/error")` shows a readable error page. Unit test covers malformed JSON (Story 7 AC). |
| 7 | **Blazor Server SignalR circuit overhead** / reconnect overlay interferes with screenshots.    | Medium     | Low     | Use Static SSR (no `@rendermode`). No circuit means no reconnect overlay.                                                                                                                                         |
| 8 | **XSS via malicious `data.json`** (string values, color attributes, backlog URL).              | Low        | Medium  | All text rendered through Razor's HTML-encoding. `Swimlane.Color` validated against `^#[0-9A-Fa-f]{6}$`. `BacklogUrl` scheme-restricted to http/https/mailto. No `MarkupString`.                                   |
| 9 | **Port 5000 conflict** with another local service (IIS Express, PowerShell web host).          | Low        | Low     | README documents how to override via `--urls http://127.0.0.1:NNNN`. `launchSettings.json` could allow an env-var override.                                                                                       |
| 10 | **Scope creep toward "real" dashboard** (interactivity, filters, multi-project).              | Medium     | High    | Non-goals documented in README. Any such request is a v2. Each exception requires PM sign-off.                                                                                                                    |
| 11 | **Team unfamiliar with inline SVG coordinate math**.                                          | Low        | Medium  | `MilestoneLayout.DateToX` is ~5 lines; unit-tested with concrete expected values from Scenario 4; documented with code comments referencing the reference design's x-values (Jan=0, Feb=260, … Jun=1300).         |
| 12 | **Dates mis-parsed due to timezone** (using `DateTime` instead of `DateOnly`).                 | Medium     | Medium  | Use `DateOnly` throughout; custom `DateOnlyJsonConverter` reads/writes `"YYYY-MM-DD"`. Unit test covers round-trip.                                                                                                 |
| 13 | **Variable swimlane count** (Scenario 15) breaks hardcoded y=42/98/154 layout.                | Medium     | Medium  | `MilestoneLayout.SwimlaneY(i, N)` uses `42 + i*((154-42)/max(1,N-1))`. Unit-tested for N=1..5. Left swimlane legend uses `justify-content:space-around` so it auto-distributes.                                     |
| 14 | **`data.json` not copied to output** after `dotnet publish`, causing "file not found" in distributed exe. | Low  | Medium  | `.csproj` explicit `<None Update="data.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>`. Integration test-run exercises a published build.                                              |
| 15 | **Screenshot automation (Playwright) requested after MVP** requires architecture changes.     | Low        | Low     | Static SSR already serves a complete HTML document in one request - Playwright simply navigates to `/` and screenshots. Zero architecture changes. Documented in Phase 3.                                         |
| 16 | **Hot reload stops working in `dotnet publish` self-contained single-file exe** because `reloadOnChange` requires a writable directory to watch. | Low | Low | For the published exe, `data.json` is still next to the exe in the publish folder (writable). `FileSystemWatcher` works there. Tested manually; documented in README. |