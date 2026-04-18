# Architecture

## Overview & Goals

The **ReportingDashboard** is a single-page Blazor Server (.NET 8) web application that renders a pixel-faithful, 1920x1080 executive status dashboard from a local `data.json` file. It runs locally via Kestrel at `http://localhost:5000`, has no authentication, no cloud dependency, and no database. Its sole purpose is to produce screenshot-ready visuals for PowerPoint executive decks.

**Goals**
1. Deliver a visually pixel-faithful recreation of `OriginalDesignConcept.html` (header + SVG timeline + CSS Grid heatmap).
2. Drive 100% of content from `data.json` — PMs update data without touching code.
3. Render fully server-side in under 2 seconds with no loading spinner, no login, no client interaction required.
4. Keep the codebase minimal, dependency-light (no MudBlazor/Radzen/Newtonsoft/Chart.js), and easily runnable on any Windows box with the .NET 8 SDK.
5. Provide deterministic, unit-tested timeline math (`TimelineCalculator`) so SVG positioning cannot silently regress.
6. Fail gracefully on malformed/missing `data.json` with a clean centered error page.

**Non-Goals**: Auth, HTTPS, multi-project routing, DB, responsive design, PDF/image export, WCAG compliance, i18n, cloud deploy, hot-reload of `data.json`.

---

## System Components

The solution is a single `.sln` containing two projects: the Blazor Server web app and a test project.

```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/              (net8.0, Microsoft.NET.Sdk.Web)
│       ├── Program.cs
│       ├── App.razor
│       ├── _Imports.razor
│       ├── Pages/
│       │   ├── _Host.cshtml
│       │   ├── Dashboard.razor
│       │   └── Dashboard.razor.css      (scoped CSS)
│       ├── Components/
│       │   ├── HeaderBar.razor          (+ .razor.css)
│       │   ├── TimelineStrip.razor      (+ .razor.css)
│       │   ├── HeatmapGrid.razor        (+ .razor.css)
│       │   ├── HeatmapCell.razor
│       │   └── ErrorPanel.razor
│       ├── Services/
│       │   ├── IDashboardDataService.cs
│       │   ├── DashboardDataService.cs  (singleton)
│       │   ├── IClock.cs
│       │   ├── SystemClock.cs
│       │   └── TimelineCalculator.cs
│       ├── Models/
│       │   ├── DashboardData.cs
│       │   ├── HeaderInfo.cs
│       │   ├── LegendItem.cs
│       │   ├── MilestoneTrack.cs
│       │   ├── Milestone.cs
│       │   ├── MonthColumn.cs
│       │   ├── HeatmapRow.cs
│       │   ├── HeatmapItem.cs
│       │   └── RowCategory.cs           (enum: Shipped, InProgress, Carryover, Blockers)
│       ├── Styles/
│       │   └── theme.css                (CSS variables: colors, spacing)
│       ├── wwwroot/
│       │   ├── css/site.css
│       │   └── data/data.json           (sample fictional project)
│       ├── appsettings.json
│       └── ReportingDashboard.csproj
└── tests/
    └── ReportingDashboard.Tests/        (net8.0, xUnit + bUnit)
        ├── TimelineCalculatorTests.cs
        ├── DashboardDataServiceTests.cs
        ├── HeatmapGridTests.cs
        ├── TimelineStripTests.cs
        ├── TestData/data.valid.json
        ├── TestData/data.malformed.json
        └── ReportingDashboard.Tests.csproj
```

### Component Responsibilities

| Component | Type | Responsibility | Dependencies | Data In/Out |
|---|---|---|---|---|
| `Program.cs` | Startup | Configure Kestrel to bind `http://localhost:5000`, register DI services, register Blazor Server, map `/_Host`. | `Microsoft.AspNetCore.*` | N/A |
| `_Host.cshtml` | Razor Page | Root HTML shell; sets `<html>`, `<head>` (font, base href), renders `<component type=App>` server-prerendered. | `App.razor` | HTTP GET → HTML |
| `App.razor` | Root component | Serves `Dashboard` at `/`; no `<Router>` needed (single page) but included for default fallback. | `Dashboard.razor` | — |
| `Dashboard.razor` | Page | Top-level layout: body flex column, 1920×1080 canvas, `overflow:hidden`. Injects `IDashboardDataService`, fetches data once in `OnInitialized`, branches into either (HeaderBar + TimelineStrip + HeatmapGrid) or `ErrorPanel`. | `IDashboardDataService`, `IClock` | `DashboardData` → rendered HTML |
| `HeaderBar.razor` | Component | Renders `.hdr` — title, subtitle, optional ADO link, right-side legend. | — | `HeaderInfo`, `List<LegendItem>` |
| `TimelineStrip.razor` | Component | Renders `.tl-area` — 230px left-label panel + inline SVG (1560×185). Calls `TimelineCalculator` for all X coords, NOW line, diamond & circle markers, labels. | `TimelineCalculator`, `IClock` | `List<MilestoneTrack>`, `List<MonthColumn>`, `DateTime Now` |
| `HeatmapGrid.razor` | Component | Renders `.hm-wrap` title + `.hm-grid` CSS Grid. Builds `grid-template-columns: 160px repeat(N,1fr)` inline from month count. Iterates 4 rows × N months, emits `HeatmapCell` per cell. Applies current-month class via `IClock` comparison. | `HeatmapCell`, `IClock` | `List<MonthColumn>`, `List<HeatmapRow>` |
| `HeatmapCell.razor` | Component | Renders a single `.hm-cell` with row-specific class (`ship-cell`, `prog-cell`, `carry-cell`, `block-cell`), applies `.apr` class when `IsCurrentMonth`, emits bullet items or muted dash for empty/future. | — | `List<HeatmapItem>`, `RowCategory`, `IsCurrentMonth`, `IsFuture` |
| `ErrorPanel.razor` | Component | Centered message "Unable to load dashboard data. Please check data.json." plus the exception summary (message only, no stack). | — | `string errorMessage` |
| `IDashboardDataService` / `DashboardDataService` | Service (Singleton) | Loads `wwwroot/data/data.json` once at construction, deserializes with `JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = Skip, AllowTrailingCommas = true }`. Exposes `DashboardData? Data` and `string? LoadError`. Thread-safe via eager init. | `IWebHostEnvironment`, `ILogger<DashboardDataService>` | file → `DashboardData` |
| `TimelineCalculator` | Helper (Pure static or stateless class) | Pure functions for: `GetXForDate(DateTime d, DateTime start, DateTime end, double svgWidth)`, `GetMonthGridlines(...)`, `GetNowX(DateTime now, ...)`, clamp/out-of-range handling. No framework dependencies. | — | dates → doubles |
| `IClock` / `SystemClock` | Service (Singleton) | Wraps `DateTime.Now` so tests can inject a fixed time (critical for current-month highlight tests). | — | → `DateTime Now` |

---

## Component Interactions

**Startup-time flow (once per process)**
1. `Program.cs` registers `IClock → SystemClock`, `IDashboardDataService → DashboardDataService` (both **singleton**), `AddRazorPages()`, `AddServerSideBlazor()`.
2. During singleton activation (first request), `DashboardDataService` resolves `IWebHostEnvironment.WebRootPath`, reads `wwwroot/data/data.json` via `File.ReadAllText`, deserializes to `DashboardData`, and caches the object. On failure: catches `JsonException`/`IOException`/`FileNotFoundException`, logs via `ILogger`, sets `LoadError`, leaves `Data = null`.

**Request-time flow (per browser connection)**
1. Browser `GET http://localhost:5000/` → `_Host.cshtml` server-prerenders `App` → `Dashboard.razor`.
2. `Dashboard.OnInitialized` pulls `svc.Data` and `svc.LoadError`.
3. If `LoadError != null`: render `<ErrorPanel Message="@svc.LoadError"/>` inside the 1920×1080 canvas.
4. Else: render
   - `<HeaderBar Header=@data.Header Legend=@data.Legend/>`
   - `<TimelineStrip Tracks=@data.Tracks Months=@data.Months Now=@clock.Now/>`
   - `<HeatmapGrid Rows=@data.Heatmap Months=@data.Months Now=@clock.Now/>`
5. Blazor Server sends fully rendered HTML/CSS; the SignalR circuit stays open but no client-side interactions are wired (no `@onclick` handlers, no `StateHasChanged` loops). No spinner is shown because the page is server-prerendered on initial GET.

**Calculation flow inside `TimelineStrip`**
1. `TimelineCalculator.GetTimelineWindow(months)` returns `(startDate, endDate)` = first day of first month → last day of last month.
2. For each `Milestone`: `x = GetXForDate(m.Date, start, end, 1560)`, `y = track.Y` (track index → 42, 98, 154, …).
3. For month labels: `x = GetXForDate(firstOfMonth, ...)`.
4. For NOW: `x = GetXForDate(clock.Now, ...)` — omitted if out of range.

**Calculation flow inside `HeatmapGrid`**
1. Determine `currentMonthIndex` = index of month where `Year == Now.Year && Month == Now.Month`; `-1` if none.
2. For each row category × each month, pass `IsCurrentMonth = (i == currentMonthIndex)` and `IsFuture = monthStart > Now` to `HeatmapCell`.

**Communication pattern**: Pure parent → child parameter passing; no events, no `CascadingValue`, no `StateHasChanged` triggers, no JS interop.

---

## Data Model

All types are C# POCOs with `System.Text.Json` attributes where needed. No database; the "store" is the JSON file on disk.

### Entities

```csharp
public sealed class DashboardData
{
    public HeaderInfo Header { get; set; } = new();
    public List<LegendItem> Legend { get; set; } = new();
    public List<MilestoneTrack> Tracks { get; set; } = new();
    public List<MonthColumn> Months { get; set; } = new();
    public List<HeatmapRow> Heatmap { get; set; } = new();
}

public sealed class HeaderInfo
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string? LinkText { get; set; }   // e.g., "↗ ADO Backlog"
    public string? LinkUrl { get; set; }
}

public sealed class LegendItem
{
    public string Label { get; set; } = "";
    public string Shape { get; set; } = "";  // "diamond" | "circle" | "bar"
    public string Color { get; set; } = "";  // e.g. "#F4B400"
    public int Size { get; set; }            // px
}

public sealed class MilestoneTrack
{
    public string Id { get; set; } = "";      // "M1"
    public string Name { get; set; } = "";    // "Chatbot & MS Role"
    public string Color { get; set; } = "";   // "#0078D4"
    public int LaneY { get; set; }            // SVG y coord, e.g. 42
    public List<Milestone> Milestones { get; set; } = new();
}

public sealed class Milestone
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = "";    // "Mar 26 PoC"
    public MilestoneType Type { get; set; }    // enum
    public string? LabelPosition { get; set; } // "above" | "below" (default above)
}

public enum MilestoneType { PoC, Production, Checkpoint, MinorCheckpoint }

public sealed class MonthColumn
{
    public int Year { get; set; }
    public int Month { get; set; }            // 1-12
    public string ShortLabel { get; set; } = ""; // "Jan"
    public string HeaderLabel { get; set; } = ""; // displayed in heatmap header e.g. "APR"
}

public sealed class HeatmapRow
{
    public RowCategory Category { get; set; }  // enum
    public string Label { get; set; } = "";    // "✅ SHIPPED"
    // Cells keyed by "yyyy-MM" for predictable lookup
    public Dictionary<string, List<HeatmapItem>> Cells { get; set; } = new();
}

public enum RowCategory { Shipped, InProgress, Carryover, Blockers }

public sealed class HeatmapItem
{
    public string Text { get; set; } = "";
    public bool IsPlaceholder { get; set; }    // muted dash for future months
}
```

### Relationships

- `DashboardData` 1 → * `MilestoneTrack` 1 → * `Milestone`
- `DashboardData` 1 → * `MonthColumn` (ordered list defines heatmap columns and timeline window)
- `DashboardData` 1 → 4 `HeatmapRow` (one per `RowCategory`)
- `HeatmapRow.Cells` is keyed by `"yyyy-MM"` matching `MonthColumn.Year/Month`

### Storage

- **Location**: `wwwroot/data/data.json` (served from `ContentRootPath/wwwroot/data/data.json`; not served to the browser — read by server on startup).
- **Format**: UTF-8 JSON. Dates ISO-8601 (`2026-03-26`). Colors `"#RRGGBB"` strings.
- **Versioning**: no schema version field in v1; additive changes only. If breaking, add top-level `"schemaVersion": 2` and branch in the service.
- **Sample**: A fictional project (e.g., "Orion Platform Launch") ships covering Jan–Jun 2026 with 3 tracks, 8–10 milestones, and 4–6 items per heatmap cell including one future month with placeholder dashes.

### Validation on Load

`DashboardDataService` performs post-deserialization validation:
1. `Data != null`
2. `Months.Count >= 1` and months are contiguous (warn if not)
3. Each `Track.Milestones[*].Date` within `[firstOfFirstMonth, lastOfLastMonth]` (out-of-range logged as warning, not fatal — milestone is clamped/skipped by `TimelineCalculator`)
4. `Heatmap.Count == 4` with one row per `RowCategory`
Any fatal validation failure populates `LoadError`.

---

## API Contracts

The app exposes **no public HTTP API, no REST/JSON endpoints, no gRPC, no SignalR hubs of its own**. All endpoints are ASP.NET Core / Blazor infrastructure:

| Endpoint | Method | Purpose | Response |
|---|---|---|---|
| `/` | GET | Serves `_Host.cshtml` which pre-renders `Dashboard.razor`. | `200 text/html` — full 1920×1080 page |
| `/_blazor` | GET/WebSocket | Blazor Server SignalR circuit (framework-provided). | `101 Switching Protocols` |
| `/_blazor/negotiate` | POST | SignalR negotiate (framework-provided). | `200 application/json` |
| `/_framework/*` | GET | Blazor JS runtime assets. | `200` static |
| `/css/site.css`, `/css/theme.css` | GET | Static styles. | `200 text/css` |
| `/ReportingDashboard.styles.css` | GET | Scoped CSS bundle (auto-generated). | `200 text/css` |

**Internal service contracts (C# interfaces, not HTTP)**

```csharp
public interface IDashboardDataService
{
    DashboardData? Data { get; }
    string? LoadError { get; }   // null when load succeeded
}

public interface IClock
{
    DateTime Now { get; }
}

public static class TimelineCalculator
{
    public static double GetXForDate(DateTime date, DateTime start, DateTime end, double svgWidth);
    public static IReadOnlyList<(DateTime monthStart, double x)> GetMonthGridlines(
        IEnumerable<MonthColumn> months, DateTime start, DateTime end, double svgWidth);
    public static double? GetNowX(DateTime now, DateTime start, DateTime end, double svgWidth);
}
```

### Error Handling

- **File missing / IO error**: `DashboardDataService` catches and sets `LoadError = "data.json not found at <path>"`; `Dashboard.razor` renders `ErrorPanel`.
- **JSON malformed**: `JsonException` caught; `LoadError = "data.json is malformed: <message>"`.
- **Validation failures**: `LoadError = "data.json invalid: <reason>"`.
- **Unexpected exceptions inside components**: Blazor's default server-side error boundary UI is replaced by a `<ErrorBoundary>` wrapping the three UI components in `Dashboard.razor` that falls back to `ErrorPanel`.
- **HTTP**: Kestrel's default developer exception page is disabled in Release; any unhandled error returns `500` with a minimal text body (never reached in normal flow).

---

## Infrastructure Requirements

### Hosting
- **Server**: ASP.NET Core Kestrel bundled in-process, started via `dotnet run` (or `dotnet ReportingDashboard.dll` after publish).
- **Binding**: `http://localhost:5000` only. Configured in `Program.cs`:
  ```csharp
  builder.WebHost.UseKestrel().UseUrls("http://localhost:5000");
  ```
  `launchSettings.json` sets the same URL and disables HTTPS profile.
- **Process model**: Single OS process, single app; no IIS, no reverse proxy, no systemd/service wrapper.

### Networking
- Localhost-only loopback. No firewall exceptions required.
- No outbound calls (no telemetry, no CDN, no NuGet pulls at runtime).
- No HTTPS, no certificates.

### Storage
- **Filesystem only**. `wwwroot/data/data.json` is the sole data file (~5–50 KB expected).
- No log sinks beyond `ILogger` → Console (default `Microsoft.Extensions.Logging.Console`).
- No caching layer; the singleton service holds the parsed object in memory (< 1 MB).

### Build & Tooling
- **SDK**: .NET 8 SDK (any patch).
- **Build**: `dotnet build ReportingDashboard.sln -c Release`
- **Run**: `dotnet run --project src/ReportingDashboard`
- **Dev loop**: `dotnet watch --project src/ReportingDashboard run` (Razor hot reload).
- **Publish**: `dotnet publish src/ReportingDashboard -c Release -o ./publish` → xcopy `./publish` folder + `data.json` to any Windows machine with .NET 8 runtime.
- **Tests**: `dotnet test` runs bUnit + xUnit against `ReportingDashboard.Tests`.

### CI/CD
- GitHub Actions workflow (`.github/workflows/ci.yml`) — **build + test only** (no deploy, per spec):
  1. `actions/checkout@v4`
  2. `actions/setup-dotnet@v4` with `dotnet-version: 8.0.x`
  3. `dotnet restore`
  4. `dotnet build --configuration Release --no-restore`
  5. `dotnet test --configuration Release --no-build --verbosity normal`
- Runs on `windows-latest` to match the Segoe UI target environment; also on `ubuntu-latest` for OS-independence sanity (non-blocking).
- No deployment stage; the artifact is the `./publish` folder a developer can xcopy.

---

## Technology Stack Decisions

| Concern | Choice | Justification |
|---|---|---|
| Runtime/framework | **.NET 8 (`net8.0`), Blazor Server** | Mandated. Server-side prerender gives instant first paint with no WASM download — ideal for a screenshot tool. |
| Project structure | **Single `.sln` with `src/ReportingDashboard` + `tests/ReportingDashboard.Tests`** | Mandated (`.sln` required). Keeps discoverability simple; tests segregated. |
| UI composition | **Raw Razor components + scoped CSS (`*.razor.css`)** | Pixel-faithful reproduction of `OriginalDesignConcept.html` demands exact control of CSS; component libraries (MudBlazor, Radzen) inject their own styles/markup that would fight the design. |
| Charts/timeline | **Inline SVG authored in Razor** | The reference design is hand-drawn SVG; any chart library would be more complex and less faithful. Works in every browser without JS. |
| Heatmap layout | **CSS Grid (`grid-template-columns: 160px repeat(N,1fr)`)** | Exact match to the reference; natively supports dynamic N months via inline style. |
| JSON | **`System.Text.Json` (built-in)** | No NuGet dependency; `PropertyNameCaseInsensitive` and comment/trailing-comma tolerance cover PM-friendly editing. Newtonsoft explicitly disallowed. |
| DI lifetime | **Singleton for `IDashboardDataService` and `IClock`** | Data is static for the life of the process (no hot-reload). Singleton eliminates re-parse cost per request and matches `data load < 500ms` NFR. |
| Clock | **`IClock` abstraction with `SystemClock`** | Allows unit tests to fix "now" for deterministic current-month-highlight and NOW-line tests without touching `DateTime.Now`. |
| Timeline math | **Pure static `TimelineCalculator`** | No framework coupling, trivially unit-testable to 100% branch coverage (NFR). |
| Font | **Segoe UI (system font on Windows)** | Matches design; no CDN/web-font dependency per constraint. Fallback stack: `'Segoe UI', Arial, sans-serif`. |
| Color management | **CSS custom properties in `wwwroot/css/theme.css`** | Satisfies "no magic color strings scattered" maintainability NFR; also usable by scoped CSS via `var(--clr-ship-dot)`. |
| Routing | **`<Router>` included, single route `/` → `Dashboard`** | Default fallback page meets "no login / no loading spinner" story 2 criteria; no deep-linking required. |
| Test frameworks | **xUnit 2.9 + bUnit 1.x** | Industry-standard, explicitly recommended in research. xUnit for `TimelineCalculator`/`DashboardDataService`; bUnit for component render assertions (CSS classes, SVG attributes, current-month markup). |
| Hosting | **Kestrel, `http://localhost:5000`** | Matches Story 2 AC exactly; no IIS, no HTTPS per spec. |
| Logging | **`Microsoft.Extensions.Logging` Console provider** | Built-in; sufficient for a local tool. Critical for visibility into `data.json` load errors per Story 1 AC. |
| Hot reload | **`dotnet watch`** | Built-in to .NET 8 SDK; Razor + scoped-CSS hot reload works out of the box. |

### Rejected alternatives
- **Blazor WebAssembly** — rejected (spec mandates Server; WASM adds download latency hurting the 2-sec NFR).
- **MudBlazor / Radzen / Chart.js / D3** — rejected (violate raw-Razor constraint and visual fidelity).
- **Newtonsoft.Json** — rejected (constraint).
- **SQLite / EF Core** — rejected (out of scope; JSON is the store).
- **FileSystemWatcher on `data.json`** — rejected (out of scope per spec: restart-only refresh).

---

## UI Component Architecture

This section maps each visual section in `OriginalDesignConcept.html` to a concrete Blazor component, its CSS layout strategy, its data bindings (parameters), and its interactions (none — this is a static render).

### Canvas root: `Dashboard.razor` ↔ `<body>` in design

- **Visual section**: root 1920×1080 canvas, flex column, `overflow:hidden`, `#FFFFFF` background, Segoe UI.
- **Component**: `Pages/Dashboard.razor` wrapped in a single `<div class="dash-root">` (the Blazor app is hosted inside `_Host.cshtml`, and scoped CSS forces the exact dimensions).
- **CSS layout strategy** (`Dashboard.razor.css`):
  ```css
  .dash-root { width:1920px; height:1080px; overflow:hidden;
               background:#FFFFFF; color:#111;
               font-family:'Segoe UI', Arial, sans-serif;
               display:flex; flex-direction:column; }
  ```
  Global resets (`* { margin:0; padding:0; box-sizing:border-box }`) live in `wwwroot/css/site.css`.
- **Data bindings**: none directly; pulls `DashboardData` from `IDashboardDataService` and passes subtrees to children.
- **Interactions**: none (no `@onclick`, no state mutation).

### Section 1: Header ↔ `HeaderBar.razor` (`.hdr`)

- **Visual section**: Title + subtitle + optional link on the left; legend (PoC diamond / Prod diamond / Checkpoint circle / NOW bar) on the right.
- **CSS layout strategy**:
  ```css
  .hdr { padding:12px 44px 10px; border-bottom:1px solid #E0E0E0;
         display:flex; align-items:center; justify-content:space-between;
         flex-shrink:0; }
  .hdr h1 { font-size:24px; font-weight:700; }
  .hdr .sub { font-size:12px; color:#888; margin-top:2px; }
  .hdr a { color:#0078D4; text-decoration:none; }
  .legend { display:flex; gap:22px; align-items:center; }
  .legend .item { display:flex; align-items:center; gap:6px; font-size:12px; }
  .legend .diamond { width:12px; height:12px; transform:rotate(45deg); }
  .legend .dot    { width:8px; height:8px; border-radius:50%; }
  .legend .bar    { width:2px; height:14px; }
  ```
- **Data bindings**:
  - `[Parameter] public HeaderInfo Header { get; set; }` → `Header.Title`, `Header.Subtitle`, `Header.LinkText`, `Header.LinkUrl`.
  - `[Parameter] public List<LegendItem> Legend { get; set; }` → iterated; each item's `Shape` drives the `diamond|dot|bar` class and `Color` is applied inline as `background: @item.Color`.
- **Interactions**: none. Link is a plain `<a href>` (opens in same tab; not relevant for screenshot).

### Section 2: Timeline ↔ `TimelineStrip.razor` (`.tl-area`)

- **Visual section**: 196px-tall strip; 230px left label panel + inline SVG (1560×185) with month gridlines, per-track horizontal lines, diamond/circle markers, date labels, and NOW dashed line.
- **CSS layout strategy**:
  ```css
  .tl-area { display:flex; align-items:stretch; padding:6px 44px 0;
             flex-shrink:0; height:196px;
             border-bottom:2px solid #E8E8E8; background:#FAFAFA; }
  .tl-labels { width:230px; flex-shrink:0; display:flex; flex-direction:column;
               justify-content:space-around; padding:16px 12px 16px 0;
               border-right:1px solid #E0E0E0; }
  .tl-label { font-size:12px; font-weight:600; line-height:1.4; }
  .tl-label .sub { font-weight:400; color:#444; }
  .tl-svg-box { flex:1; padding-left:12px; padding-top:6px; }
  ```
  The SVG itself is authored inline with `width="1560" height="185" overflow="visible"`.
- **Data bindings**:
  - `[Parameter] public List<MilestoneTrack> Tracks`
  - `[Parameter] public List<MonthColumn> Months`
  - `[Parameter] public DateTime Now` (injected from `IClock`)
  - X coordinates computed via `TimelineCalculator.GetXForDate`; track Y from `Track.LaneY`; marker colors from `Track.Color` (lines) and `MilestoneType` (diamond fill `#F4B400`/`#34A853`, circle `#999`).
  - NOW line rendered only if `GetNowX != null`.
- **Interactions**: none. Pure SVG render; no hover tooltips (out of scope — screenshots don't capture hover).

### Section 3: Heatmap ↔ `HeatmapGrid.razor` + `HeatmapCell.razor` (`.hm-wrap` / `.hm-grid`)

- **Visual section**: Uppercase title, then a CSS Grid with a 160-px label column and N equal month columns, 36-px header row and 4 equal data rows (Shipped, In Progress, Carryover, Blockers). Current month column is amber-tinted.
- **CSS layout strategy** (`HeatmapGrid.razor.css`):
  ```css
  .hm-wrap  { flex:1; min-height:0; display:flex; flex-direction:column;
              padding:10px 44px 10px; }
  .hm-title { font-size:14px; font-weight:700; color:#888;
              letter-spacing:.5px; text-transform:uppercase; margin-bottom:8px;
              flex-shrink:0; }
  .hm-grid  { flex:1; min-height:0; display:grid;
              grid-template-rows:36px repeat(4, 1fr);
              border:1px solid #E0E0E0; }
  /* grid-template-columns is set inline via style="..." because N is dynamic */
  .hm-corner, .hm-col-hdr { /* as per spec, #F5F5F5 bg, borders */ }
  .hm-col-hdr.apr-hdr     { background:#FFF0D0; color:#C07700; }
  .ship-hdr, .ship-cell, .ship-cell.apr { /* green palette */ }
  .prog-hdr, .prog-cell, .prog-cell.apr { /* blue palette */ }
  .carry-hdr, .carry-cell, .carry-cell.apr { /* amber palette */ }
  .block-hdr, .block-cell, .block-cell.apr { /* red palette */ }
  .hm-cell .it { font-size:12px; color:#333; padding:2px 0 2px 12px;
                 position:relative; line-height:1.35; }
  .hm-cell .it::before { content:''; position:absolute; left:0; top:7px;
                         width:6px; height:6px; border-radius:50%; }
  .hm-cell .it.future { color:#AAA; }
  ```
  In `HeatmapGrid.razor`:
  ```razor
  <div class="hm-grid" style="grid-template-columns:160px repeat(@Months.Count, 1fr);">
  ```
- **Data bindings**:
  - `[Parameter] public List<HeatmapRow> Rows`
  - `[Parameter] public List<MonthColumn> Months`
  - `[Parameter] public DateTime Now`
  - Computes `currentMonthIndex`; renders corner cell, column headers (attaching `apr-hdr` when index matches), then for each `RowCategory` in fixed order → row header + N `<HeatmapCell>` children.
  - `HeatmapCell` parameters: `RowCategory Category`, `List<HeatmapItem> Items`, `bool IsCurrentMonth`, `bool IsFuture`. It resolves the class prefix (`ship`/`prog`/`carry`/`block`) from `Category` and emits `class="hm-cell @prefix-cell @(IsCurrentMonth ? "apr" : "")"`.
  - Empty cell or `IsFuture` with no items → emits a single `<div class="it future">—</div>`.
- **Interactions**: none.

### Section 4: Error ↔ `ErrorPanel.razor`

- **Visual section**: Centered message when `data.json` is missing/malformed.
- **CSS layout strategy**:
  ```css
  .err-wrap { flex:1; display:flex; align-items:center; justify-content:center;
              flex-direction:column; gap:12px; }
  .err-main { font-size:20px; font-weight:600; color:#991B1B; }
  .err-det  { font-size:12px; color:#888; max-width:900px; text-align:center; }
  ```
- **Data bindings**: `[Parameter] public string Message`.
- **Interactions**: none.

---

## Security Considerations

- **Authentication / authorization**: none by design. The tool is for a single local user producing screenshots; adding auth would violate Story 2 AC ("no login screen").
- **Network exposure**: Kestrel bound strictly to `http://localhost:5000` (loopback). Configured in both `Program.cs` (`UseUrls`) and `launchSettings.json`. Not reachable from other hosts.
- **Transport security**: no HTTPS. Acceptable because no sensitive data and no remote access. `app.UseHttpsRedirection()` is **not** added.
- **Data protection**: `data.json` contains only PM-authored status text (no PII, secrets, or tokens). The sample shipped with the repo is fictional. A `.gitignore` rule or a clear README note tells PMs not to commit sensitive project text into their own clones.
- **Input validation**: every field deserialized from `data.json` is treated as untrusted-but-local: 
  - All strings rendered via Razor's default `@value` escaping — no `MarkupString`, no `@((MarkupString)raw)`, no `@Html.Raw` anywhere. This prevents HTML/script injection even if a PM pastes HTML into an item.
  - URL fields (`HeaderInfo.LinkUrl`) rendered as `<a href="@url">` so Razor escapes attributes; additionally validated to start with `http://`, `https://`, or `mailto:` — otherwise rendered as plain text.
  - Colors validated by regex `^#([0-9A-Fa-f]{3}){1,2}$` before being interpolated into inline `style` attributes; invalid colors fall back to `#999`.
  - Dates parsed via `DateTime` deserialization; invalid dates → validation error → `ErrorPanel`.
- **Antiforgery / CSRF**: no forms, no POST endpoints; `AddAntiforgery` default is fine.
- **SignalR**: the default Blazor Server `/_blazor` endpoint is loopback-only because Kestrel is loopback-only. No additional hardening required.
- **Dependency supply chain**: no third-party runtime NuGet packages (only `Microsoft.*` and, in tests, `xunit` + `bunit`). This minimizes vulnerability surface.
- **Secrets**: none stored. `appsettings.json` contains only Logging config. No `.env` file, no user-secrets.
- **Logging**: errors are logged to console (including the exception message on bad `data.json`), but never to disk and never to a remote sink.

---

## Scaling Strategy

Per the PM spec, **scalability is explicitly Not Applicable** — this is a single-user local tool. The architecture nonetheless has the following capacity and scaling characteristics:

- **Concurrency**: Blazor Server can comfortably handle dozens of concurrent circuits on a dev laptop; only 1 is ever expected (the PM's browser tab). Singleton service is thread-safe (read-only after construction).
- **Vertical headroom**: memory footprint < 100 MB (app + parsed `DashboardData`). CPU cost per request is dominated by SVG/HTML generation, well under 100 ms on modern hardware; meets the 2-second full-render NFR with orders of magnitude of headroom.
- **Data size scaling**: `data.json` up to ~1 MB (hundreds of heatmap items, dozens of milestones) still parses in < 50 ms. Beyond that, the *visual* layout breaks before the code does (cells overflow, SVG labels collide) — a UX ceiling, not a performance ceiling.
- **Month-count scaling**: heatmap `grid-template-columns: 160px repeat(N, 1fr)` is N-agnostic; timeline SVG X-math is range-agnostic. Practical sweet spot is 4–6 months; 12 months will render but label density becomes poor.
- **Horizontal scaling**: not applicable. If a second user needs the dashboard they run their own `dotnet run`. There is no shared state to coordinate.
- **If scope ever expanded** (out of scope today, noted for future): move `data.json` to a shared file share and add a FileSystemWatcher to invalidate the singleton cache; replace singleton with `IMemoryCache` keyed by file hash; add a scoped `ILoadFailureState` to carry per-request errors. No other layer would need to change.

---

## Risks & Mitigations

| # | Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|---|
| 1 | SVG X-position math breaks for edge dates (milestone on last day of range, NOW before range start, empty months list) | Timeline visually wrong; broken screenshot | Medium | Encapsulate all math in `TimelineCalculator` (pure static); unit tests cover: date = start, date = end, date before start (clamp/omit), date after end (clamp/omit), single-month window, 1-year window, leap day, NOW outside range. Target 100% branch coverage (success metric). |
| 2 | `data.json` malformed or missing fields after PM edit | App crashes or renders partial/garbage state | High | `JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = Skip, AllowTrailingCommas = true }`; post-deserialization validation; catch `JsonException`/`IOException`; render `ErrorPanel` with exception message; log via `ILogger`. bUnit test forces malformed JSON and asserts `ErrorPanel` appears. |
| 3 | Color bleed / style clash between components | Heatmap background leaks into header or vice versa | Low | Use scoped CSS (`*.razor.css`) for component-specific styles; reserve `wwwroot/css/site.css` + `theme.css` for resets and design tokens only. |
| 4 | Current-month detection flaky around month boundaries or across time zones | Wrong column highlighted, contradicting Story 6 | Medium | `IClock` abstraction + `SystemClock`; compare `(Year, Month)` only (not day/time); bUnit tests with fixed clocks at month start, month end, 23:59 on last day, next month 00:00 UTC — all assert correct column is classed `apr`. Document that the tool uses server local time. |
| 5 | 1920-px fixed width causes horizontal scroll on smaller monitors, PMs mis-screenshot | Truncated slides | Medium | Explicitly accepted in NFRs. README "Screenshot Workflow" section documents browser zoom-out + full-page screenshot extensions (or remote session at 1920×1080). |
| 6 | Blazor Server prerender plus SignalR reconnect introduces a flash or spinner (violating Story 2 AC) | User sees spinner in screenshot window | Low | Configure `<component render-mode="ServerPrerendered">` in `_Host.cshtml` so first GET returns fully rendered HTML; disable the default `blazor-error-ui` element by styling `display:none`; remove the `#blazor-reconnect-modal` via CSS. Document a short settling wait before screenshotting. |
| 7 | HTML/script injection via PM-authored JSON text | XSS on the PM's own machine (low severity, still avoidable) | Low | Never use `MarkupString` / `@Html.Raw`; render all text via Razor default escaping; validate `LinkUrl` scheme; validate color strings by regex. |
| 8 | `data.json` location ambiguity (content root vs web root) | Works on dev box, missing after `dotnet publish` | Medium | Service reads from `Path.Combine(env.WebRootPath, "data", "data.json")`; `ReportingDashboard.csproj` marks the file `<Content Include="wwwroot\data\data.json" CopyToOutputDirectory="PreserveNewest" />`; README documents "edit then restart". |
| 9 | NuGet dependency drift (bUnit vs .NET 8 compat) | CI breaks | Low | Pin `bunit` 1.27+ and `xunit` 2.9.x in `tests/ReportingDashboard.Tests.csproj`; dependabot disabled for patch noise; CI locks `dotnet-version: 8.0.x`. |
| 10 | Scoped-CSS uniqueness clash with global classes (e.g., `.sub` used in `.hdr` and `.tl-labels`) | Style bleed | Low | Namespace global-ish classes (`.hdr-sub`, `.tl-label-sub`) when they cross component boundaries; rely on scoped CSS for truly local selectors. |
| 11 | PMs edit `data.json` expecting live refresh | Confusion / bug reports | Medium | Out of scope per spec. README "Workflow" section states explicitly: *edit → `Ctrl+C` → `dotnet run` → refresh browser.* |
| 12 | Segoe UI missing on non-Windows dev machines used by contributors | Visual drift in dev | Low | Fallback stack `'Segoe UI', Arial, sans-serif` yields a close-enough Arial on other OSes; the **canonical** screenshot is taken on Windows, which is the only supported runtime environment per NFR. |