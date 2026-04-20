# Architecture

## Overview & Goals

"My Project" (ReportingDashboard) is a deliberately minimal, single-page, screenshot-optimized executive reporting dashboard. It renders a fixed **1920x1080** view mirroring `OriginalDesignConcept.html`: a header band with legend, a Gantt-style SVG milestone timeline, and a 4-row x 4-column execution heatmap (Shipped / In Progress / Carryover / Blockers). The entire data contract is a single, hand-edited `data.json` file. The rendered page exists to be screenshotted into a 16:9 PowerPoint slide - it is a **parameterized design artifact**, not an interactive application.

### Architectural Goals

1. **Pixel fidelity** - rendered output at 1920x1080 is visually indistinguishable from `OriginalDesignConcept.png`. CSS is ported near-verbatim from the reference HTML into scoped Razor CSS.
2. **Zero-friction local run** - `dotnet run` from `src/ReportingDashboard.Web/` produces a working dashboard at `http://localhost:5080/` with no cloud, no DB, no auth, no build pipeline.
3. **Single source of truth** - `wwwroot/data.json` drives 100% of rendered content; no hardcoded strings, coordinates, or dates in Razor.
4. **Hot-reload for the PM** - `FileSystemWatcher` picks up edits to `data.json` within ~250ms; browser refresh shows changes without restart.
5. **Fail-safe rendering** - malformed/missing `data.json` produces an in-page red banner, never a YSOD or process crash.
6. **Static SSR only** - Blazor's interactive render modes are forbidden; no SignalR circuit, no client-side loading flicker, no reconnection UI in screenshots.
7. **Minimal footprint** - <1,000 LOC of Razor+C#, <150KB total page payload, no third-party UI/charting libraries.
8. **Reusable template** - swapping `data.json` produces a dashboard for a different project with zero code changes.

### Non-Goals (Explicit)

Authentication, authorization, HTTPS, responsive design, interactivity (tooltips/drilldown/filtering), multi-project routing, databases/ORMs, cloud hosting, containerization, charting libraries, component libraries (MudBlazor/Radzen/FluentUI), in-app editing of `data.json`, PDF/PNG export buttons, i18n/l10n, ADO/Jira integration, accessibility beyond baseline semantic HTML.

---

## System Components

The solution is a single Blazor Server (.NET 8) project plus a test project. Logical components inside the web project are organized as: **models**, **services**, **pages**, and **partial components**.

### C1. `ReportingDashboard.Web` (Blazor Server host)

**Responsibility:** ASP.NET Core / Kestrel host configured for static SSR. Wires DI, routing, middleware, and Kestrel endpoint. Single minimal-hosting `Program.cs`.

**Dependencies:** `Microsoft.AspNetCore.App` (shared framework), `Microsoft.Extensions.Caching.Memory`, `Microsoft.Extensions.FileProviders.Physical`.

**Interfaces:** `WebApplication` (framework), binds HTTP endpoint `http://localhost:5080`.

**Key behaviors:**
- `builder.Services.AddRazorComponents()` (no `.AddInteractiveServerComponents()` - static SSR only).
- `builder.Services.AddSingleton<IDashboardDataService, DashboardDataService>()`.
- `builder.Services.AddMemoryCache()`.
- `builder.WebHost.UseUrls("http://localhost:5080")` (or `Kestrel:Endpoints:Http:Url` in `appsettings.json`).
- `app.MapRazorComponents<App>()` - no render-mode extension methods.
- `app.MapGet("/healthz", () => "ok")` - tiny liveness probe (optional).
- No `UseHttpsRedirection`, no `UseAuthentication`, no `UseAuthorization`.
- Antiforgery middleware kept at framework default (no-op here).

### C2. `DashboardDataService` (singleton)

**Responsibility:** Sole reader of `wwwroot/data.json`. Parses, validates, caches, and hot-reloads the file. Provides either a valid `DashboardData` object or a `DashboardLoadError` to consumers.

**Interface:**

```csharp
public interface IDashboardDataService
{
    DashboardLoadResult GetCurrent();      // cached; always non-null
    event EventHandler? DataChanged;       // optional - for future SSE/extension
}

public sealed record DashboardLoadResult(
    DashboardData? Data,
    DashboardLoadError? Error,
    DateTimeOffset LoadedAt);

public sealed record DashboardLoadError(
    string FilePath,
    string Message,
    int? Line,
    int? Column,
    string Kind);  // "NotFound" | "ParseError" | "ValidationError"
```

**Dependencies:** `IMemoryCache`, `IWebHostEnvironment` (for `WebRootPath`), `ILogger<DashboardDataService>`, `System.Text.Json`, `FileSystemWatcher` (wrapped internally).

**Behaviors:**
- On construction: reads `{WebRootPath}/data.json`, deserializes with `JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }`, runs `DashboardDataValidator`, caches result in `IMemoryCache` under key `"dashboard:current"`.
- `FileSystemWatcher` on `wwwroot/` filter `data.json`, events `Changed | Created | Renamed`. Debounced 250ms via a `System.Timers.Timer` (single-shot reset per event).
- On debounced trigger: re-reads and re-validates. Swaps cache entry atomically (single `IMemoryCache.Set`). On parse/validation failure, caches a `DashboardLoadResult` with populated `Error` and `Data = null` - **never throws** to callers.
- Reads use `FileShare.ReadWrite` and retry up to 3 times (50ms backoff) to handle editors that lock during save.
- Logs (Information): load success + size + duration; (Warning): validation errors; (Error): IO exceptions.
- Thread-safe: a single `SemaphoreSlim(1,1)` serializes reloads.

### C3. `DashboardDataValidator`

**Responsibility:** Pure, stateless validation of a deserialized `DashboardData`. Returns a list of human-readable error strings or empty (valid).

**Interface:**

```csharp
public static class DashboardDataValidator
{
    public static IReadOnlyList<string> Validate(DashboardData data);
}
```

**Rules enforced (v1):**
- `project.title` non-empty; `project.backlogUrl` parseable `Uri` or null.
- `timeline.start < timeline.end`.
- `timeline.lanes` count 1..6; each lane `id` non-empty and unique; `color` matches `^#[0-9A-Fa-f]{6}$`.
- Each `milestone.date` falls within `[timeline.start, timeline.end]`.
- `milestone.type`  {`poc`, `prod`, `checkpoint`}.
- `heatmap.months.Length == 4` (v1 default; configurable but must equal `heatmap.rows[i].cells.Length`).
- `heatmap.rows` has exactly 4 rows with categories {`shipped`, `inProgress`, `carryover`, `blockers`} (case-insensitive; order enforced in render, not JSON).
- `heatmap.currentMonthIndex`  `[0, months.Length)` **or** omitted (null) to auto-compute from `DateTime.Today`.
- `heatmap.maxItemsPerCell` optional int �1 (default 4).

### C4. `TimelineLayoutEngine` (pure computation)

**Responsibility:** Converts timeline data + "today" into SVG geometry. No rendering - returns a view model consumed by `TimelineSvg.razor`.

**Interface:**

```csharp
public static class TimelineLayoutEngine
{
    public const int SvgWidth = 1560;
    public const int SvgHeight = 185;
    public const int TopPad = 20;    // reserve for month labels
    public const int BottomPad = 20;

    public static TimelineViewModel Build(
        Timeline timeline,
        DateOnly today,
        int svgWidth = SvgWidth,
        int svgHeight = SvgHeight);
}

public sealed record TimelineViewModel(
    IReadOnlyList<MonthGridline> Gridlines,
    IReadOnlyList<LaneGeometry> Lanes,
    NowMarker Now);

public sealed record MonthGridline(double X, string Label);
public sealed record LaneGeometry(
    string Id, string Label, string Color, double Y,
    IReadOnlyList<MilestoneGeometry> Milestones);
public sealed record MilestoneGeometry(
    double X, double Y, MilestoneType Type, string Caption,
    CaptionPosition CaptionPosition);  // Above | Below (alternates to avoid overlap)
public sealed record NowMarker(double X, bool InRange);
```

**Key math (deterministic, unit-testable):**
- `xOf(date) = (date - start).TotalDays / (end - start).TotalDays * svgWidth`.
- `NowMarker.InRange = today in [start, end]`.
- Lanes Y evenly spaced: `Y_i = TopPad + (i + 0.5) * (svgHeight - TopPad - BottomPad) / laneCount`.
- Month gridlines: one per month boundary from `start` to `end` inclusive.
- Caption position: alternates Above/Below per adjacent milestone on same lane when horizontal distance < 50px, else defaults to Above.

### C5. `HeatmapLayoutEngine` (pure computation)

**Responsibility:** Normalizes heatmap rows into a fixed 4-row grid model, applies overflow truncation, and computes the current-month column index.

**Interface:**

```csharp
public static class HeatmapLayoutEngine
{
    public static HeatmapViewModel Build(
        Heatmap heatmap,
        DateOnly today,
        int defaultMaxItems = 4);
}

public sealed record HeatmapViewModel(
    IReadOnlyList<string> Months,
    int CurrentMonthIndex,            // -1 if none matches
    IReadOnlyList<HeatmapRowView> Rows);

public sealed record HeatmapRowView(
    HeatmapCategory Category,         // Shipped | InProgress | Carryover | Blockers
    string HeaderLabel,               // uppercase display label
    IReadOnlyList<HeatmapCellView> Cells);

public sealed record HeatmapCellView(
    IReadOnlyList<string> Items,      // first N items
    int OverflowCount,                // >0 => render "+K more" as final .it
    bool IsEmpty);                    // true => render "-" in #AAA
```

**Truncation rule:** If `items.Count > maxItems`, take first `maxItems-1` and append one synthetic item `"+K more"` where `K = items.Count - (maxItems-1)`. Empty cells (`items.Count == 0`) flagged `IsEmpty = true`.

### C6. `Dashboard.razor` (the one page)

**Responsibility:** The `@page "/"` root. Reads `IDashboardDataService`, branches on `Error != null`, and composes three child components. No `@rendermode` directive (static SSR).

**Layout:**

```razor
@page "/"
@inject IDashboardDataService Data
<!DOCTYPE html>
<html lang="en"><head>... scoped CSS ...</head>
<body>
  @if (result.Error is not null) { <ErrorBanner Error="result.Error" /> }
  @if (result.Data is { } d)
  {
    <DashboardHeader Project="d.Project" NowLabel="@nowLabel" />
    <TimelineSvg Model="timelineVm" />
    <Heatmap Model="heatmapVm" />
  }
  else
  {
    <DashboardHeader Project="Project.Placeholder" NowLabel="@nowLabel" />
    <TimelineSvg Model="TimelineViewModel.Empty" />
    <Heatmap Model="HeatmapViewModel.Empty" />
  }
</body></html>
```

Responsible for calling the layout engines once per request in `OnInitialized` (synchronous; no async I/O - service is already cached).

### C7. `DashboardHeader.razor`

Renders `.hdr` band: title + inline backlog link, subtitle, right-aligned legend (4 items). Stateless. Parameters: `Project Project`, `string NowLabel`.

### C8. `TimelineSvg.razor`

Renders `.tl-area`: 230px lane-label column (flex) + 1560x185 inline `<svg>` with `<defs><filter id="sh">`, month gridlines, per-lane `<line>` + milestone `<polygon>`/`<circle>` + caption `<text>`, and dashed NOW line. Parameter: `TimelineViewModel Model`.

### C9. `Heatmap.razor`

Renders `.hm-wrap`: `.hm-title` header + `.hm-grid` 5x5 CSS grid (1 header row + 4 data rows; 1 status column + 4 month columns). Applies `.current` modifier to cells where column index equals `Model.CurrentMonthIndex`. Parameter: `HeatmapViewModel Model`.

### C10. `ErrorBanner.razor`

Full-width red banner at top of `<body>`. Displays file path, error kind, line/column (if any), and message. Pure presentational; parameter: `DashboardLoadError Error`.

### C11. `App.razor` / `Routes.razor` / `MainLayout.razor`

Minimal framework glue from the `blazor --empty --interactivity None` template. `MainLayout` is effectively a pass-through (`@Body`) - no nav, no chrome. The canonical `<html>`/`<head>`/`<body>` live in `Dashboard.razor` to keep the dashboard's CSS reset in control.

### C12. Scoped CSS: `Dashboard.razor.css`

Verbatim port of `OriginalDesignConcept.html`'s `<style>` block, with these documented renames:

| Original class | Ported class | Reason |
|---|---|---|
| `.apr`, `.apr-hdr` | `.current`, `.current-hdr` | semantic (not month-specific) |

All color hex codes, `grid-template-columns:160px repeat(4,1fr)`, `grid-template-rows:36px repeat(4,1fr)`, font-size/weight/letter-spacing values, and the `*{margin:0;padding:0;box-sizing:border-box}` reset are preserved exactly.

### C13. `ReportingDashboard.Web.Tests`

xUnit + bUnit project. Test suites:

- `TimelineLayoutEngineTests` - `xOf`, NOW-in/out of range, lane Y distribution, caption alternation.
- `HeatmapLayoutEngineTests` - truncation ("+K more"), empty-cell flag, `CurrentMonthIndex` resolution.
- `DashboardDataValidatorTests` - each rule (positive + negative).
- `DashboardDataServiceTests` - load happy path, malformed JSON  `Error.Kind="ParseError"`, missing file  `Error.Kind="NotFound"`, hot-reload via temp file (FileSystemWatcher).
- `DashboardRenderTests` (bUnit) - renders `Dashboard.razor` against sample `data.json` without exception; asserts presence of key DOM markers (`.hdr h1`, `.tl-area svg`, `.hm-grid`, 17 grid cells).
- Optional `TimelineSvgSnapshotTests` via `Verify.Xunit` - pins the SVG string for the sample dataset.

---

## Component Interactions

### Startup sequence

```
Program.cs
   Kestrel bind http://localhost:5080
   DI registration (AddRazorComponents, AddMemoryCache, AddSingleton<IDashboardDataService>)
   WebApplication.Run()
       DashboardDataService ctor (singleton, lazy-safe)
            read wwwroot/data.json
            deserialize + validate
            cache DashboardLoadResult
            start FileSystemWatcher on wwwroot/data.json
```

### Request handling (`GET /`)

```
Browser  -->  Kestrel  -->  Razor Components endpoint
                                   Dashboard.razor.OnInitialized()
                                       IDashboardDataService.GetCurrent()  (cache hit, O(1))
                                       TimelineLayoutEngine.Build(data.Timeline, DateOnly.Today)
                                       HeatmapLayoutEngine.Build(data.Heatmap, DateOnly.Today)
                                       render DashboardHeader + TimelineSvg + Heatmap
                                   Fully rendered HTML flushed on first byte (static SSR)
Browser  <--  HTML (~50-100KB, no JS bundle needed for this page)
```

No SignalR handshake. No `_framework/blazor.server.js` execution path for interactivity. The Blazor framework's static SSR emits the HTML synchronously.

### Hot-reload sequence

```
PM saves wwwroot/data.json
   OS file-change event
       FileSystemWatcher.Changed -> DashboardDataService.OnChanged()
            restart 250ms debounce timer
       (250ms elapses, no further events)
            timer.Elapsed -> ReloadAsync()
                 acquire semaphore
                 re-read + re-validate + cache.Set("dashboard:current", result)
                 release semaphore
                 log "Reloaded data.json in {ms}ms"
PM hits F5 in browser  ->  GET /  ->  Dashboard reads new cache entry  ->  new HTML
```

There is no push to the browser (no SignalR, no SSE in v1). Reload requires a browser refresh - this is an intentional, documented behavior.

### Error-path rendering

```
GetCurrent() returns { Data=null, Error=<NotFound|ParseError|ValidationError> }
   Dashboard.razor:
       render <ErrorBanner Error=... /> at top of <body>
       render placeholder DashboardHeader (title="(data.json error)"),
              empty TimelineViewModel.Empty, empty HeatmapViewModel.Empty
   Layout dimensions preserved (1920x1080, no scrollbars) so the page still
   screenshots cleanly as evidence of the error.
```

### Sequence diagram (text)

```
PM          Browser         Kestrel        Dashboard.razor     DataService    data.json
 |             |                |                 |                 |              |
 |  edit+save ------------------|-----------------|-----------------|-----(write)->|
 |             |                |                 |       FSW Changed event -------|
 |             |                |                 |   debounce 250ms                |
 |             |                |                 |   reload+cache.Set              |
 |   F5        |                |                 |                 |              |
 |------------->  GET /  ------->                 |                 |              |
 |             |                |--render request->                 |              |
 |             |                |                 | GetCurrent() -->|              |
 |             |                |                 |<-- result ------|              |
 |             |                |                 | Build VMs                       |
 |             |                |<--- HTML -------|                 |              |
 |             |<-- 200 OK -----|                 |                 |              |
```

---

## Data Model

### Storage

- **Single file**: `src/ReportingDashboard.Web/wwwroot/data.json`.
- **No database**, no ORM, no migrations. The JSON file **is** the persistence layer, versionable via git.
- **Encoding**: UTF-8 (no BOM required; BOM tolerated).
- **Size budget**: v1 target <50KB; design target up to 200KB without degradation.

### C# POCOs (`Models/`)

All types are `sealed`. Properties use `required` where applicable. Dates are `DateOnly` for day-resolution values (`"2026-03-26"`) - never `DateTime`.

```csharp
public sealed class DashboardData
{
    public required Project Project { get; init; }
    public required Timeline Timeline { get; init; }
    public required Heatmap Heatmap { get; init; }
    public Theme? Theme { get; init; }  // optional future re-skin hook
}

public sealed class Project
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public string? BacklogUrl { get; init; }
    public string BacklogLinkText { get; init; } = "\u2192 ADO Backlog";

    public static Project Placeholder { get; } = new()
    {
        Title = "(data.json error)",
        Subtitle = "see error banner above"
    };
}

public sealed class Timeline
{
    public required DateOnly Start { get; init; }
    public required DateOnly End { get; init; }
    public required IReadOnlyList<TimelineLane> Lanes { get; init; }
}

public sealed class TimelineLane
{
    public required string Id { get; init; }        // "M1"
    public required string Label { get; init; }     // "Chatbot & MS Role"
    public required string Color { get; init; }     // "#0078D4"
    public required IReadOnlyList<Milestone> Milestones { get; init; }
}

public sealed class Milestone
{
    public required DateOnly Date { get; init; }
    public required MilestoneType Type { get; init; }   // enum
    public required string Label { get; init; }         // "Mar 26 PoC"
    public CaptionPosition? CaptionPosition { get; init; }  // optional override
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MilestoneType { Checkpoint, Poc, Prod }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CaptionPosition { Above, Below }

public sealed class Heatmap
{
    public required IReadOnlyList<string> Months { get; init; }   // ["Jan","Feb","Mar","Apr"]
    public int? CurrentMonthIndex { get; init; }                  // null => auto
    public int MaxItemsPerCell { get; init; } = 4;
    public required IReadOnlyList<HeatmapRow> Rows { get; init; } // exactly 4
}

public sealed class HeatmapRow
{
    public required HeatmapCategory Category { get; init; }
    public required IReadOnlyList<IReadOnlyList<string>> Cells { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HeatmapCategory { Shipped, InProgress, Carryover, Blockers }

public sealed class Theme  // optional, v1 unused
{
    public string? Font { get; init; }
}
```

### Entity relationships

```
DashboardData (1)
   Project (1)
   Timeline (1)
      TimelineLane (1..6)
         Milestone (0..N)
   Heatmap (1)
      Row (exactly 4; one per HeatmapCategory)
         Cell (exactly Months.Length; default 4)
            item string (0..N)
   Theme (0..1)  -- reserved
```

### Canonical `data.json` shape

```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform \u2022 Privacy Automation Workstream \u2022 April 2026",
    "backlogUrl": "https://dev.azure.com/contoso/privacy/_backlogs/backlog/"
  },
  "timeline": {
    "start": "2026-01-01",
    "end":   "2026-06-30",
    "lanes": [
      { "id":"M1", "label":"Chatbot & MS Role", "color":"#0078D4",
        "milestones":[
          {"date":"2026-01-12","type":"checkpoint","label":"Jan 12"},
          {"date":"2026-03-26","type":"poc","label":"Mar 26 PoC"},
          {"date":"2026-04-30","type":"prod","label":"Apr Prod (TBD)"}
        ]},
      { "id":"M2", "label":"PDS & Data Inventory", "color":"#00897B", "milestones":[ ] },
      { "id":"M3", "label":"Auto Review DFD",     "color":"#546E7A", "milestones":[ ] }
    ]
  },
  "heatmap": {
    "months": ["Jan","Feb","Mar","Apr"],
    "currentMonthIndex": null,
    "maxItemsPerCell": 4,
    "rows": [
      {"category":"shipped",    "cells":[["Item A"],["Item B"],[],["Item C"]]},
      {"category":"inProgress", "cells":[[],[],["X"],["Y","Z"]]},
      {"category":"carryover",  "cells":[[],[],[],["Legacy API"]]},
      {"category":"blockers",   "cells":[[],[],[],["Vendor SLA"]]}
    ]
  }
}
```

### Derived (non-persisted) data

- `NowMarker.X` - computed per request from `DateOnly.FromDateTime(DateTime.Today)`.
- `CurrentMonthIndex` - if null in JSON, computed as `DateTime.Today.Month` mapped through the `Months` array (by abbreviation) or -1 if no match.
- `HeatmapCellView.OverflowCount` - derived by `HeatmapLayoutEngine` from `Cells` + `MaxItemsPerCell`.

---

## API Contracts

This is a server-rendered page, not a REST API. The surface area is intentionally tiny.

### Endpoint: `GET /`

- **Response:** `200 OK`, `Content-Type: text/html; charset=utf-8`, fully rendered HTML body (~50-100KB gzipped). No JS framework payload required for rendering.
- **Caching:** `Cache-Control: no-cache` (default). The page always reflects current `data.json` state - browser refresh must fetch fresh.
- **Errors:**
  - Application-level errors (bad `data.json`) are **never** surfaced as HTTP 5xx. They render as `200 OK` with an in-page red `ErrorBanner`. This preserves screenshot-worthiness even in the error state.
  - Unhandled framework exceptions (shouldn't happen) fall back to the default ASP.NET Core developer exception page in Development, or a blank `500` in Production. `appsettings.json` sets environment to `Production` by default; Development is opt-in via `DOTNET_ENVIRONMENT`.

### Endpoint: `GET /data.json`

- **Implementation:** Served by `UseStaticFiles()` from `wwwroot/`. No special middleware needed.
- **Response:** `200 OK`, `Content-Type: application/json`, file contents verbatim.
- **Purpose:** Convenience only - lets a PM open `http://localhost:5080/data.json` to sanity-check the currently-served file without opening the file system.
- **Error:** `404 Not Found` (plain) if file absent - that path is **not** the dashboard route, so no banner treatment needed.

### Endpoint: `GET /healthz` (optional)

- **Response:** `200 OK`, `text/plain`, body `ok`. For process-liveness scripts; no business impact.

### Endpoint: `GET /_framework/*`, `GET /_content/*`

- Served by Blazor framework. Contains minimal static assets. No custom contract.

### Error-banner payload contract (in-page, not HTTP)

Rendered as DOM by `ErrorBanner.razor`. Stable CSS selectors for troubleshooting:

```html
<div class="error-banner" role="alert">
  <strong>Failed to load data.json</strong>
  <span class="error-kind">ParseError</span>
  <span class="error-path">src/ReportingDashboard.Web/wwwroot/data.json</span>
  <span class="error-location">line 42, column 3</span>
  <span class="error-message">Unexpected end of JSON input</span>
</div>
```

Banner CSS: full width, `background:#FEF2F2`, `color:#991B1B`, `padding:10px 44px`, `font-size:13px`, 1px `#EA4335` bottom border. Pushes the rest of the dashboard down by its own height; heatmap `flex:1` absorbs the reduction.

### No public API

No REST, no GraphQL, no gRPC. No authentication. No versioning concerns.

---

## Infrastructure Requirements

### Hosting

- **Runtime:** .NET 8 LTS (8.0.11+). `<TargetFramework>net8.0</TargetFramework>`.
- **Web server:** Kestrel embedded in the `WebApplication`. No IIS, no reverse proxy, no fronting CDN.
- **Process model:** Single `dotnet run` or single `ReportingDashboard.Web.exe` process on the developer/PM workstation.
- **OS:**
  - **Canonical (screenshot) target:** Windows 10/11 + Edge/Chrome. Segoe UI font present.
  - **Developer dev targets:** macOS and Linux build + run; rendering is correct but font fallback changes text metrics slightly - not acceptable for final screenshots.

### Networking

- **Bind:** `http://localhost:5080` only. Configured in `appsettings.json`:
  ```json
  { "Kestrel": { "Endpoints": { "Http": { "Url": "http://localhost:5080" } } } }
  ```
- **Port rationale:** 5080 avoids default dev ports (5000/5001, 7000s) to reduce collisions when a PM runs alongside other .NET tools.
- **HTTPS:** Disabled. `UseHttpsRedirection()` NOT called. No dev cert friction.
- **External exposure:** None. Kestrel bound to loopback, so the machine's LAN cannot reach it.
- **Outbound:** No runtime outbound calls. (Browser may fetch `https://fonts`? - **no**, font stack uses system fonts only; no remote fonts.)
- **Firewall:** No inbound rules needed (loopback).

### Storage

- **`wwwroot/data.json`** - the only application data. Size <200KB. Backed up via git.
- **No temp/upload/cache directory** on disk. `IMemoryCache` is in-process RAM.
- **Logs:** Console only (stdout/stderr). No file logs by default. If logs need to persist for debugging, the PM redirects stdout: `dotnet run > dashboard.log 2>&1`.

### Build & local run

```
# From repo root:
dotnet restore ReportingDashboard.sln
dotnet build   ReportingDashboard.sln -c Release
dotnet run     --project src/ReportingDashboard.Web/ReportingDashboard.Web.csproj
# opens http://localhost:5080
```

Watch mode for PM-side iteration:

```
cd src/ReportingDashboard.Web
dotnet watch run
```

### Publishing (optional)

Self-contained single-file Windows executable (deliverable to PMs/execs who don't have the SDK):

```
dotnet publish src/ReportingDashboard.Web \
    -c Release -r win-x64 --self-contained true \
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
    -o publish/win-x64
```

Output: `publish/win-x64/ReportingDashboard.Web.exe` (~70MB) + `wwwroot/` folder alongside (data.json stays external & editable). Double-click to launch; browser opens to `http://localhost:5080` (no auto-open - README instructs the user).

### CI/CD

Single GitHub Actions workflow `.github/workflows/ci.yml`:

```yaml
name: CI
on: [push, pull_request]
jobs:
  build:
    runs-on: windows-latest   # canonical OS for Segoe UI / screenshot fidelity
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - run: dotnet restore ReportingDashboard.sln
      - run: dotnet build   ReportingDashboard.sln -c Release --no-restore
      - run: dotnet test    ReportingDashboard.sln -c Release --no-build
                            --collect:"XPlat Code Coverage"
      - run: dotnet format  ReportingDashboard.sln --verify-no-changes
```

No release/deploy jobs - the artifact is the source repo. No Docker image built, no registry push, no cloud deploy.

### Environment requirements

| Requirement | Version | Notes |
|---|---|---|
| .NET SDK | 8.0.x | 8.0.400+ recommended |
| OS (canonical) | Windows 10/11 | Segoe UI |
| Browser | Edge 120+ or Chrome 120+ | 1920-wide viewport |
| Disk | ~300MB | repo + build output |
| RAM | negligible (<100MB at runtime) | - |
| Network | none at runtime | offline-friendly |

### Configuration surface

`appsettings.json` (checked into repo):

```json
{
  "Kestrel": {
    "Endpoints": { "Http": { "Url": "http://localhost:5080" } }
  },
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
  },
  "Dashboard": {
    "DataFilePath": "wwwroot/data.json",
    "HotReloadDebounceMs": 250
  }
}
```

`DataFilePath` resolution: relative paths are resolved against `IWebHostEnvironment.ContentRootPath`. Overridable by env var `Dashboard__DataFilePath` for testing.

---

## Technology Stack Decisions

All choices are locked to the mandated stack.

### Framework & runtime

| Concern | Decision | Justification |
|---|---|---|
| Language / runtime | **C# 12 on .NET 8 LTS** (`net8.0`) | Mandated. LTS through Nov 2026. `DateOnly`, `required` members, primary constructors all available. |
| Web host | **`WebApplication` minimal hosting** in `Program.cs` | Single-file startup; ~25 lines. No `Startup.cs` ceremony. |
| UI framework | **Blazor Server, Static SSR only** | Mandated. No `AddInteractiveServerComponents()` call. First GET returns complete HTML - required for screenshot workflow. |
| Routing | **Razor Components route `@page "/"`** | Single route. No `MapControllers`, no minimal-API pages. |
| Static files | **`UseStaticFiles()`** (framework default) | Serves `wwwroot/data.json`, scoped CSS bundles, any future image assets. |

### Data & parsing

| Concern | Decision | Justification |
|---|---|---|
| Persistence | **Single `wwwroot/data.json`** | Mandated "no cloud, local only." Non-developers can edit in VS Code; git-diffable. |
| JSON parser | **`System.Text.Json`** | Built-in. `PropertyNameCaseInsensitive=true`, `ReadCommentHandling=Skip`, `AllowTrailingCommas=true`. `DateOnly` supported since .NET 8. Enum converters via `JsonStringEnumConverter`. |
| Validation | **Hand-rolled `DashboardDataValidator` static class** | Adding FluentValidation would be a 1-rule-per-use dependency; manual validation for ~8 rules is <120 LOC and dependency-free. Deferred to FluentValidation 11.x only if rules exceed 20. |
| Caching | **`IMemoryCache`** (`AddMemoryCache`) | Built-in. Absolute expiration not needed; we `Set` on reload. |
| File watching | **`System.IO.FileSystemWatcher`** | Built-in. Debounce via `System.Timers.Timer` (single-shot reset) - 250ms. |
| Date/time | **`DateOnly`** for all day-resolution values; `DateTime.Today` only for "now" comparison | Cleaner JSON (`"2026-03-26"`), no TZ ambiguity. |

### Rendering

| Concern | Decision | Justification |
|---|---|---|
| Render mode | **Static SSR** (no `@rendermode`) | No SignalR circuit, no reconnect UI, no `blazor.server.js` runtime interactivity - critical for clean screenshots. |
| Component library | **None** | MudBlazor / Radzen / FluentUI would override the custom CSS. Hand-authored CSS matches `OriginalDesignConcept.html` 1:1. |
| Charting | **Hand-rolled inline SVG** in `TimelineSvg.razor` | Full control of bespoke look. ChartJs.Blazor is abandoned (last release 2021); Plotly.Blazor is heavy and misaligned aesthetically; ApexCharts.Blazor works but adds weight. |
| CSS strategy | **Scoped CSS via `Dashboard.razor.css`** + minimal global reset in `app.css` | Scoped CSS co-locates styles with the component and prevents leakage. |
| Icons | **Inline SVG** (diamonds, NOW bar) | Avoid icon-font dependency for 1-2 glyphs. `\u2192` Unicode arrow used for backlog link. |
| Fonts | **System `'Segoe UI'` fallback stack** | Windows canonical. No remote fonts (offline-safe, no network). |

### Tooling

| Concern | Decision | Version |
|---|---|---|
| Formatter | `dotnet format` | Built-in |
| Analyzers | `Microsoft.CodeAnalysis.NetAnalyzers` | Built-in in SDK |
| Unit tests | **xUnit** | 2.9.2 |
| Assertions | **FluentAssertions 6.12.x** (**hard-pinned**) | v8+ adopted a commercial license - avoid. Pin `<PackageReference Version="6.12.*" />`. |
| Blazor component tests | **bUnit** | 1.33.3 |
| Snapshot (optional) | **Verify.Xunit** | 26.x - for SVG string stability |
| Coverage | **coverlet.collector** | 6.0.x |
| CI | **GitHub Actions** + `actions/setup-dotnet@v4` | build + test only; Windows runner for font-sensitive snapshot tests |

### Deliberately excluded (with rationale)

| Excluded | Why |
|---|---|
| EF Core / SQLite / Dapper | JSON file is the database. |
| Identity / cookies / JWT | No auth required. |
| Docker / Kubernetes / Azure / AWS | "Local only" per mandate. |
| Redis / distributed cache | Dataset <200KB; `IMemoryCache` is more than enough. |
| SignalR / interactive render modes | Static SSR is sufficient; interactivity is an anti-goal. |
| MudBlazor / Radzen / FluentUI Blazor | Would override the custom CSS. |
| Serilog / Application Insights / OpenTelemetry | Console logging is sufficient for a local tool. |
| Newtonsoft.Json | `System.Text.Json` is faster and built-in. |
| AutoMapper | POCO  view model mapping is trivial; hand-written. |
| MediatR / CQRS | One component reads one service - no pipeline complexity needed. |

---

## Security Considerations

### Authentication & authorization

- **None.** Explicit product decision. Documented in README: "This tool is a local-only, single-user screenshot utility. Do not deploy to a shared server."
- No `AddAuthentication`, `AddAuthorization`, `UseAuthentication`, or `UseAuthorization` calls.

### Network exposure

- Kestrel bound to `http://localhost:5080` (loopback `127.0.0.1`). Not reachable from LAN, not bound to `0.0.0.0`.
- If someone intentionally changes the bind URL to `http://0.0.0.0:5080`, README flags that this removes the primary defense and is unsupported for v1.

### Transport security

- **HTTPS disabled.** Dev certs cause friction for non-developer PMs. Loopback-only binding makes TLS unnecessary.
- No `UseHttpsRedirection()` call.
- HSTS not configured.

### Data protection

- `data.json` is plaintext, non-sensitive executive summary text.
- **No PII expected.** README explicitly states: "Do not place PII in `data.json`. If this changes, the security model must be revisited."
- No secrets in source or in `data.json`. No API keys, no connection strings, no tokens - the app has no outbound dependencies to authenticate against.
- `.gitignore` excludes nothing sensitive from the project; if a user's local `data.json` contains project-specific content they don't want in git, they can untrack via `.git/info/exclude`.

### Input validation

- **JSON deserialization** uses safe defaults: no type-name handling (not applicable to `System.Text.Json`), no polymorphic deserialization, enums via `JsonStringEnumConverter` with strict value set.
- **Validator rules** (C3) enforce ranges, uniqueness, and value whitelists before the data reaches any rendering path.
- Rendered item strings are **HTML-encoded by Razor** (`@item` not `@((MarkupString)item)`), so a malicious `"<script>"` in `data.json` is inert.
- Hex color strings are regex-validated before being inlined into SVG `stroke`/`fill` attributes (prevents CSS/SVG injection in `Dashboard.razor.css` variable paths - though we don't emit CSS from JSON in v1, the validator is defense-in-depth).
- URLs in `project.backlogUrl` parsed with `Uri.TryCreate(..., UriKind.Absolute)` and restricted to `http/https` schemes; otherwise the link renders as a disabled span.

### Anti-forgery / CORS / CSP

- No forms, no `<form method="post">` - CSRF not applicable. Framework's default antiforgery middleware left enabled (no-op here, no cost).
- No CORS - no cross-origin fetches.
- **CSP (optional hardening):** Response header `Content-Security-Policy: default-src 'self'; style-src 'self' 'unsafe-inline'; script-src 'self'; img-src 'self' data:;` added via a minimal middleware. `'unsafe-inline'` for `style` is required because scoped CSS is inlined in `<style>` blocks by Blazor. Low priority for v1; flagged for v1.1.

### Dependency hygiene

- Only first-party Microsoft packages for the web project; test project adds pinned `FluentAssertions 6.12.*` and `bUnit 1.33.*`.
- GitHub Dependabot enabled for `nuget` ecosystem (alerts only; updates reviewed manually - pinned FluentAssertions must not bump to v8).
- No `npm`, no client-side package manager.

### Logging & PII

- Log events include file paths and error messages but never the contents of `data.json` (which could contain confidential project status even if not "PII").
- Validation errors log field names and rule violations, not field values.

### Threat model (brief)

| Threat | Relevance | Mitigation |
|---|---|---|
| Remote attacker | Low - loopback binding | Not reachable from network. |
| Local malicious user | Low - same trust boundary as the PM's workstation | Out of scope for a local screenshot tool. |
| XSS via `data.json` | Medium | Razor auto-encoding + validated enums/URLs/hex colors. |
| DoS via huge `data.json` | Low | File size typically <200KB; `FileStream` reads up to a 10MB cap before rejecting with a banner. |
| Path traversal via config override | Low | `DataFilePath` resolved against `ContentRootPath`; validator rejects paths containing `..`. |

---

## Scaling Strategy

This system is intentionally **non-scalable** in the cloud sense. "Scaling" here means preserving correctness and performance as the `data.json` grows and as the dashboard template is reused across more projects.

### Vertical (data) scaling

- **Target:** `data.json` up to 200KB; 20 items per heatmap cell (pre-truncation); up to 6 timeline lanes with up to 20 milestones each.
- **Render cost:** O(lanes + milestones + 16 cells + total items). At worst ~500 render operations per request - well under 10ms on modest hardware.
- **Memory:** O(JSON size) in-process; `IMemoryCache` holds one `DashboardData` instance (~1MB worst-case).
- **If `data.json` grows beyond 1MB:** the file-read retry loop and JSON parser both handle it, but the heatmap overflow behavior is the real bottleneck (UI cell overflow, not CPU). Truncation keeps render output bounded.

### Horizontal (user) scaling

- **Design target:** one user, one process. Single-tenant.
- **Concurrent users:** Kestrel handles 100+ concurrent GETs trivially (the rendered HTML is <150KB and render is sub-10ms), but this is not a design goal. A single PM presses F5; an entire exec team does not browse live.
- **Out of scope:** load balancing, session affinity, connection pooling, autoscaling. No cloud runtime, no orchestrator.

### Multi-project scaling

- **v1:** one process instance = one project. Copy the folder, replace `data.json`, `dotnet run`.
- **Future (out of scope for v1):** `/projects/{slug}` routing with a `data/` folder of JSON files. The architecture supports this with minimal refactor:
  1. `DashboardDataService` becomes keyed: `GetCurrent(string slug)`.
  2. `FileSystemWatcher` watches the `data/` folder.
  3. `Dashboard.razor` becomes `@page "/{slug?}"`.
  No rendering or CSS changes required.

### Performance budgets (enforced)

| Metric | Budget | How measured |
|---|---|---|
| TTFB `GET /` | <200ms | Stopwatch around `OnInitialized`; unit test |
| Server render | <10ms for 50KB `data.json` | `BenchmarkDotNet` (optional) or a timing log line |
| Page payload | <150KB | Manual inspection via browser devtools, or a test that captures `HttpClient` response size |
| Hot-reload latency | <500ms file-save to cache-swap | Debounce is 250ms; reload itself ~50ms |
| Cold-start to first render | <2s on `dotnet run` | Manual |

### Caching strategy

- `IMemoryCache` with a single key `"dashboard:current"` holding the `DashboardLoadResult`.
- Entry lifetime: from app start or last reload until the next reload. No absolute/sliding expiration needed - the `FileSystemWatcher` drives invalidation.
- No output caching, no response compression in v1 (payload is already small). If payload ever exceeds 150KB, add `app.UseResponseCompression()` with Brotli/Gzip - zero-cost framework feature.

### Future scale-out (explicitly not implemented)

If this ever became a shared internal tool (violating the "local only" mandate):
- Add `UseAuthentication()` + Azure Entra ID / OIDC.
- Move to containerized Kestrel behind a reverse proxy.
- Move `data.json` to Azure Blob Storage with `IFileProvider` abstraction.
- Introduce per-project tenant routing.

None of the above is in v1 scope and the current architecture would need modest surgery to support them.

---

## Risks & Mitigations

| # | Risk | Severity | Likelihood | Mitigation |
|---|---|---|---|---|
| R1 | **Blazor interactive mode accidentally enabled** (e.g., `.AddInteractiveServerComponents()` added later) injects `blazor.server.js`, SignalR handshake, and reconnect UI, which appear in screenshots | High | Medium | Code review checklist; a bUnit/integration test that asserts the rendered HTML does NOT contain `_framework/blazor.server.js` script tag; README's "DO NOT" section; analyzer suppression-free build. |
| R2 | **Hand-rolled SVG math errors** (NOW line, month gridlines, milestone X) produce visibly wrong dates | High | Medium | `TimelineLayoutEngine` is pure and unit-tested with `[Theory]` across date ranges (same month, 6 months, 12 months, edge cases: `today == start`, `today == end`, `today > end`). Optional `Verify.Xunit` snapshot of rendered SVG. |
| R3 | **Heatmap cell overflow** (month with 10+ items) blows the 1080px vertical budget | High | High | Truncation via `HeatmapLayoutEngine` to `maxItemsPerCell` (default 4) + "+K more". `overflow:hidden` on `.hm-cell` as last-line-of-defense. Unit tests for truncation at 1, N, N+1, N+5 items. |
| R4 | **`data.json` malformed**  YSOD / blank page / process crash | High | High | `DashboardDataService` never throws to callers; wraps all IO/parse/validate in try/catch and returns `DashboardLoadResult` with `Error` populated. `Dashboard.razor` renders `ErrorBanner` + placeholders. Integration test with deliberately-bad fixtures (missing brace, wrong type, out-of-range date). |
| R5 | **Segoe UI absent** on non-Windows dev/render machine - SVG `<text>` widths shift, captions collide | Medium | High on Linux/macOS | Pin fallback stack `'Segoe UI', -apple-system, 'Helvetica Neue', Arial, sans-serif`. Declare Windows + Edge/Chrome the only supported screenshot configuration. README explicitly calls this out. CI runner is `windows-latest` to guarantee Segoe UI during snapshot tests. |
| R6 | **FileSystemWatcher misses events** on some platforms (network drives, WSL mounts) or fires twice per save | Medium | Medium | Debounce 250ms coalesces duplicate events. Retry-on-IOException (editor lock) with 3x50ms backoff. README notes: "If hot-reload stops working, restart the process; network drives are not supported." |
| R7 | **FluentAssertions v8 commercial license** inadvertently adopted via transitive upgrade | Low | Low | Pin explicit `<PackageReference ... Version="6.12.*" />` in test `.csproj`. Add a build step `dotnet list package --include-transitive | findstr FluentAssertions` to fail build if v8 appears. Dependabot update PRs reviewed manually. |
| R8 | **Fixed 1920x1080 layout breaks on laptop demos** (content clips or requires horizontal scroll) | Medium | High | Accepted trade-off - screenshots require a 1920-wide window. README documents: use external monitor, or DevTools device toolbar at 1920x1080, or browser zoom (zoom is preview-only, not deck-ready). Optional `?zoom=0.75` query param (v1.1) applies a `transform:scale(0.75); transform-origin:top left;` to `<body>`. |
| R9 | **Scope creep to "real BI dashboard"** (tooltips, drill-down, filters, multi-project) | High (schedule) | High | PM spec explicitly forbids interactivity. Architecture lists these in "Non-Goals." PRs introducing `@rendermode` are rejected. |
| R10 | **Concurrent `data.json` writes** while `FileSystemWatcher` fires mid-write  parse failure on half-written file | Medium | Medium | Retry-on-IOException (R6 mitigation). `File.ReadAllText` with `FileShare.ReadWrite`. Validator's ParseError banner is transient - next save completes and next event reloads cleanly. |
| R11 | **Race condition: reload completes during an in-flight request** | Low | Low | `IMemoryCache.Set` is thread-safe; each request captures the reference once in `OnInitialized` and uses it throughout rendering. No torn reads. |
| R12 | **Default port 5080 collides** with another local service | Low | Low | Configurable via `appsettings.json` or env var `ASPNETCORE_URLS=http://localhost:5081`. README documents how to change. |
| R13 | **.NET 8 reaches end of LTS (Nov 2026)** | Low (today) | Certain (eventually) | Codebase is trivial to retarget. Plan: bump `<TargetFramework>` to `net10.0` (next LTS) in Q3 2026. No architectural changes expected. |
| R14 | **CSS scoping leaks / Blazor CSS isolation bugs** cause style drift vs. reference HTML | Medium | Low | Verify pixel parity against `OriginalDesignConcept.png` at end of Phase 1 before any data binding. Optional Playwright-based visual diff test in v1.1. |
| R15 | **Red/yellow/green palette is not color-blind-safe** | Medium (accessibility) | High (for affected users) | Flagged as open question in PM spec. v1.1: add category glyph (?, ?, !, x) alongside color dot as defense-in-depth. |
| R16 | **`DateTime.Today` uses server local time** - if app runs in one TZ, viewed by exec in another | Low | Low (single-user, local) | Accepted. README notes "NOW uses your machine's local date." If ever shared, switch to `DateTime.UtcNow.Date` or make TZ configurable in `data.json`. |
| R17 | **Single-file publish bloat** (~70MB) annoys execs | Low | Low | Use `-p:PublishTrimmed=false` (Blazor/Razor incompatible with trimming). Document size expectation in README. Alternative: ship the 200KB framework-dependent publish and require .NET 8 runtime - not adopted because execs don't install runtimes. |
| R18 | **Scoped CSS class-hash mismatch** after rename of `.apr`  `.current` causes stale references | Low | Low | Single `Dashboard.razor.css` file; rename performed once; bUnit render test asserts `.current` class appears on the correct column cell. |

### Residual accepted risks

- **No responsive design** - accepted; this is a 1920x1080 screenshot artifact.
- **No auth** - accepted; loopback binding is the security boundary.
- **No cloud redundancy** - accepted; "no uptime SLA" is an explicit NFR.
- **Font fidelity depends on Windows** - accepted; Windows is the canonical screenshot OS.

---

## UI Component Architecture

This section maps every visual section of `OriginalDesignConcept.html` to a concrete Razor component, its CSS strategy, data bindings, and (non-)interactions. All components live in `src/ReportingDashboard.Web/Components/Pages/` (the root `Dashboard.razor`) and `.../Pages/Partials/` (children). All styles are scoped in `Dashboard.razor.css`. All components are **static SSR**; no `@rendermode`, no event handlers, no JS interop.

### Root: `Dashboard.razor`  the full page

| Aspect | Detail |
|---|---|
| Visual section | Entire `<body>` of `OriginalDesignConcept.html` |
| Route | `@page "/"` |
| CSS layout | `body { width:1920px; height:1080px; overflow:hidden; display:flex; flex-direction:column; background:#FFFFFF; color:#111; font-family:'Segoe UI',-apple-system,'Helvetica Neue',Arial,sans-serif; }` |
| Children (in order) | `ErrorBanner` (conditional)  `DashboardHeader`  `TimelineSvg`  `Heatmap` |
| Data bindings | Injects `IDashboardDataService`; calls `GetCurrent()` once in `OnInitialized`; passes slices to children |
| Interactions | None |

### Section 1: `DashboardHeader.razor`  the `.hdr` band

| Aspect | Detail |
|---|---|
| Visual section | `.hdr` from reference HTML (top band, ~48px) |
| Parameters | `[Parameter] public required Project Project { get; set; }` ; `[Parameter] public required string NowLabel { get; set; }` |
| CSS layout | `.hdr { padding:12px 44px 10px; border-bottom:1px solid #E0E0E0; display:flex; align-items:center; justify-content:space-between; flex-shrink:0; }` |
| Left cluster | `<div><h1>@Project.Title <a href="@Project.BacklogUrl">? ADO Backlog</a></h1><div class="sub">@Project.Subtitle</div></div>` |
| Right cluster (legend) | Inline flex row, `gap:22px`, four `<span>` legend items; markup ported verbatim from reference HTML with the NOW label bound to `@NowLabel` (e.g., `"Now (Apr 2026)"`) |
| Typography | `h1` 24px/700/#111 ; `.sub` 12px/400/#888 ; legend 12px/400/#111 ; backlog link #0078D4 |
| Shapes | Amber diamond 12x12 (rotate 45), green diamond 12x12, grey circle 8x8, red vertical bar 2x14 - all as inline `<span>` elements styled via CSS |
| Data bindings | `Project.Title`, `Project.BacklogUrl`, `Project.Subtitle`, computed `NowLabel` (format `"Now (MMM yyyy)"` from `DateTime.Today`) |
| Interactions | Only the `<a href="@Project.BacklogUrl">` link is clickable (opens in same tab). No hover effects, no tooltips. |
| Edge cases | Null/empty `BacklogUrl`: renders plain text, no `<a>` tag. Non-http(s) URL: same (validator strips it). |

### Section 2: `TimelineSvg.razor`  the `.tl-area` (lane labels + SVG)

| Aspect | Detail |
|---|---|
| Visual section | `.tl-area` (196px fixed height), containing 230px lane-label column + 1560x185 SVG |
| Parameters | `[Parameter] public required TimelineViewModel Model { get; set; }` |
| CSS layout - container | `.tl-area { display:flex; align-items:stretch; padding:6px 44px 0; flex-shrink:0; height:196px; border-bottom:2px solid #E8E8E8; background:#FAFAFA; }` |
| CSS layout - lane labels | `.tl-labels { width:230px; flex-shrink:0; display:flex; flex-direction:column; justify-content:space-around; padding:16px 12px 16px 0; border-right:1px solid #E0E0E0; }` ; per lane `<div style="color:@lane.Color; font-size:12px; font-weight:600; line-height:1.4;">@lane.Id<br/><span style="font-weight:400;color:#444;">@lane.Label</span></div>` |
| CSS layout - SVG wrapper | `.tl-svg-box { flex:1; padding-left:12px; padding-top:6px; }` |
| SVG structure | `<svg xmlns="http://www.w3.org/2000/svg" width="1560" height="185" style="overflow:visible;display:block"><defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs> ... </svg>` |
| SVG elements (rendered from `Model`) | 1. **Month gridlines**: for each `MonthGridline g`  `<line x1="@g.X" y1="0" x2="@g.X" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>` + `<text x="@(g.X+5)" y="14" fill="#666" font-size="11" font-weight="600">@g.Label</text>`<br>2. **Lane track**: for each `LaneGeometry l`  `<line x1="0" y1="@l.Y" x2="1560" y2="@l.Y" stroke="@l.Color" stroke-width="3"/>`<br>3. **Milestones** per lane: switch on `m.Type`: `Poc`  amber `<polygon points="...diamond..." fill="#F4B400" filter="url(#sh)"/>`; `Prod`  green `<polygon ... fill="#34A853" ...>`; `Checkpoint`  `<circle r="5" fill="white" stroke="@l.Color" stroke-width="2.5"/>` or filled grey `<circle r="4" fill="#999"/>`<br>4. **Caption**: `<text x="@m.X" y="@captionY" text-anchor="middle" fill="#666" font-size="10">@m.Caption</text>` where `captionY` depends on `m.CaptionPosition`<br>5. **NOW line** (only if `Now.InRange`): `<line x1="@Now.X" y1="0" x2="@Now.X" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>` + `<text x="@(Now.X+4)" y="14" fill="#EA4335" font-size="10" font-weight="700">NOW</text>` |
| Data bindings | `Model.Gridlines[]`, `Model.Lanes[].{Id,Label,Color,Y,Milestones}`, `Model.Lanes[].Milestones[].{X,Type,Caption,CaptionPosition}`, `Model.Now.{X,InRange}` |
| Computations | Done in `TimelineLayoutEngine` (C4); component is pure view-binding |
| Interactions | None. No `@onclick`, no `<title>` tooltips. |
| Edge cases | Empty lanes array  only gridlines + optional NOW line render. `Now.InRange=false`  NOW line omitted. Milestone outside range  skipped by layout engine with a warning log. |

### Section 3: `Heatmap.razor`  the `.hm-wrap` + `.hm-grid`

| Aspect | Detail |
|---|---|
| Visual section | `.hm-wrap` (flex:1, ~836px remaining height) containing `.hm-title` + `.hm-grid` |
| Parameters | `[Parameter] public required HeatmapViewModel Model { get; set; }` |
| CSS layout - wrapper | `.hm-wrap { flex:1; min-height:0; display:flex; flex-direction:column; padding:10px 44px 10px; }` |
| CSS layout - title | `.hm-title { font-size:14px; font-weight:700; color:#888; letter-spacing:.5px; text-transform:uppercase; margin-bottom:8px; flex-shrink:0; }` ; literal text `"Monthly Execution Heatmap  Shipped  In Progress  Carryover  Blockers"` |
| CSS layout - grid | `.hm-grid { flex:1; min-height:0; display:grid; grid-template-columns:160px repeat(4,1fr); grid-template-rows:36px repeat(4,1fr); border:1px solid #E0E0E0; }` |
| Header row (row 0) | `<div class="hm-corner">Status</div>` + 4x `<div class="hm-col-hdr @(i == Model.CurrentMonthIndex ? "current-hdr" : "")">@month</div>` |
| Data rows 1-4 | For each `HeatmapRowView row` (order enforced: Shipped, InProgress, Carryover, Blockers): `<div class="hm-row-hdr @catHdrClass(row)">@row.HeaderLabel</div>` then 4x `<div class="hm-cell @catCellClass(row) @(i == CurrentMonthIndex ? "current" : "")">` with cell content |
| Cell content | If `cell.IsEmpty`: `<div class="it empty">-</div>`. Else: foreach item in `cell.Items`  `<div class="it">@item</div>`. If `cell.OverflowCount > 0`: final `<div class="it overflow">+@cell.OverflowCount more</div>`. |
| Category CSS classes | Ship: `.ship-hdr` / `.ship-cell` ; InProgress: `.prog-hdr` / `.prog-cell` ; Carryover: `.carry-hdr` / `.carry-cell` ; Blockers: `.block-hdr` / `.block-cell` - all CSS rules ported verbatim from `OriginalDesignConcept.html` with `.apr`  `.current` |
| Dot rendering | Via `::before` pseudo-element on `.it` - 6x6px circle at `left:0; top:7px;` with `background` color per category (set in the `.ship-cell .it::before {...}` etc. rules). `.empty` class suppresses the dot (and the `::before` for empty cells uses `background:transparent` or is overridden). |
| Color bindings (verbatim hex) | **Shipped**: hdr text `#1B7A28`, hdr bg `#E8F5E9`, cell bg `#F0FBF0`, cell current bg `#D8F2DA`, dot `#34A853`  **InProgress**: `#1565C0` / `#E3F2FD` / `#EEF4FE` / `#DAE8FB` / `#0078D4`  **Carryover**: `#B45309` / `#FFF8E1` / `#FFFDE7` / `#FFF0B0` / `#F4B400`  **Blockers**: `#991B1B` / `#FEF2F2` / `#FFF5F5` / `#FFE4E4` / `#EA4335`  **Current-month col hdr**: `#FFF0D0` / `#C07700`  **Empty item**: `#AAA` |
| Data bindings | `Model.Months[]`, `Model.CurrentMonthIndex`, `Model.Rows[].{Category,HeaderLabel,Cells}`, `Model.Rows[].Cells[].{Items,OverflowCount,IsEmpty}` |
| Interactions | None. Plain text, no click, no hover. |
| Edge cases | `CurrentMonthIndex = -1`  no `.current`/`.current-hdr` class applied anywhere. All-empty row  4 cells each rendering single `-`. Row count != 4  layout engine pads/truncates to 4 with a validator warning. |

### Section 4: `ErrorBanner.razor`  conditional red banner on failure

| Aspect | Detail |
|---|---|
| Visual section | Not in `OriginalDesignConcept.html`; defined by PM spec Story 6 & Scenarios 11-12 |
| Parameters | `[Parameter] public required DashboardLoadError Error { get; set; }` |
| CSS layout | `.error-banner { flex-shrink:0; background:#FEF2F2; color:#991B1B; border-bottom:1px solid #EA4335; padding:10px 44px; font-size:13px; display:flex; gap:16px; align-items:baseline; }` |
| Markup | `<div class="error-banner" role="alert"><strong>? Failed to load data.json</strong> <span class="error-path">@Error.FilePath</span> @if (Error.Line is { } l) { <span>line @l, col @Error.Column</span> } <span class="error-message">@Error.Message</span></div>` |
| Data bindings | `Error.FilePath`, `Error.Line?`, `Error.Column?`, `Error.Message`, `Error.Kind` (drives heading text: "not found" vs "parse error" vs "validation error") |
| Interactions | None |
| Placement | Rendered as first child of `<body>`, above `DashboardHeader`. It occupies its own height; `.hm-wrap` (`flex:1`) absorbs the reduction so total page is still 1080px with `overflow:hidden`. |

### Section 5: global styling  `app.css` + `Dashboard.razor.css`

| Aspect | Detail |
|---|---|
| Global (`wwwroot/app.css`) | Only the universal reset: `*{margin:0;padding:0;box-sizing:border-box;}` and the `body` rule (size + font + color). Kept tiny; intentionally not scoped. |
| Scoped (`Components/Pages/Dashboard.razor.css`) | All component-specific CSS: `.hdr`, `.sub`, `.tl-area`, `.tl-svg-box`, `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`, `.it`, row-color modifiers, `.current-hdr`, `.current`, `.error-banner`. Ported verbatim from `OriginalDesignConcept.html` `<style>` block with `.apr`  `.current` rename. |
| Scoped CSS scope behavior | Blazor's CSS isolation emits `_content/ReportingDashboard.Web/Components/Pages/Dashboard.razor.rz.scp.css` with generated attribute selectors. Children (`DashboardHeader`, `TimelineSvg`, `Heatmap`, `ErrorBanner`) inherit the same scope because they are composed within `Dashboard.razor`. Child `.razor.css` files are co-located but typically empty in v1 - all rules live in `Dashboard.razor.css` to match the reference HTML's single-stylesheet structure. |
| SVG filter | `<filter id="sh">` lives inside the SVG `<defs>` inside `TimelineSvg.razor` - not a separate CSS concern. |

### Component-to-visual-section traceability matrix

| Visual section (`OriginalDesignConcept.html`) | Size | Component | CSS class | Data source |
|---|---|---|---|---|
| `<body>` container | 1920x1080 | `Dashboard.razor` | `body` rule | - |
| Header band | full-width x ~48px | `DashboardHeader.razor` | `.hdr` | `DashboardData.Project` + computed NOW label |
| Title + backlog link | left cluster | `DashboardHeader.razor` | `.hdr h1`, `<a>` | `Project.Title`, `Project.BacklogUrl` |
| Subtitle | below title | `DashboardHeader.razor` | `.sub` | `Project.Subtitle` |
| Legend (4 items) | right cluster | `DashboardHeader.razor` | inline flex | static markup |
| Timeline area | full-width x 196px | `TimelineSvg.razor` | `.tl-area` | `TimelineViewModel` |
| Lane labels column | 230x196px | `TimelineSvg.razor` | `.tl-labels` (new) | `TimelineViewModel.Lanes[].{Id,Label,Color}` |
| SVG timeline canvas | 1560x185 | `TimelineSvg.razor` | `.tl-svg-box` + inline SVG | `TimelineViewModel.{Gridlines,Lanes,Now}` |
| Month gridlines | 6 lines in SVG | `TimelineSvg.razor` | `<line>` elements | `TimelineViewModel.Gridlines[]` |
| Lane tracks | 3 horizontal lines | `TimelineSvg.razor` | `<line>` elements | `TimelineViewModel.Lanes[].{Y,Color}` |
| Milestone diamonds / circles | per milestone | `TimelineSvg.razor` | `<polygon>` / `<circle>` | `TimelineViewModel.Lanes[].Milestones[]` |
| NOW line + label | dashed red | `TimelineSvg.razor` | `<line stroke-dasharray>` | `TimelineViewModel.Now` |
| Heatmap wrapper | flex:1 | `Heatmap.razor` | `.hm-wrap` | `HeatmapViewModel` |
| Heatmap title bar | ~22px | `Heatmap.razor` | `.hm-title` | static literal |
| Heatmap grid (5x5) | remaining height | `Heatmap.razor` | `.hm-grid` | `HeatmapViewModel` |
| Corner cell ("Status") | 160x36px | `Heatmap.razor` | `.hm-corner` | static literal |
| Month headers (4) | (1fr)x36px each | `Heatmap.razor` | `.hm-col-hdr` (+ `.current-hdr`) | `HeatmapViewModel.Months`, `CurrentMonthIndex` |
| Row header (Shipped) | 160px x (1fr) | `Heatmap.razor` | `.hm-row-hdr.ship-hdr` | `HeatmapViewModel.Rows[0]` |
| Row header (In Progress) | 160px x (1fr) | `Heatmap.razor` | `.hm-row-hdr.prog-hdr` | `HeatmapViewModel.Rows[1]` |
| Row header (Carryover) | 160px x (1fr) | `Heatmap.razor` | `.hm-row-hdr.carry-hdr` | `HeatmapViewModel.Rows[2]` |
| Row header (Blockers) | 160px x (1fr) | `Heatmap.razor` | `.hm-row-hdr.block-hdr` | `HeatmapViewModel.Rows[3]` |
| Data cells (16 total) | 1fr x 1fr | `Heatmap.razor` | `.hm-cell.<cat>-cell` (+ `.current`) | `HeatmapViewModel.Rows[].Cells[]` |
| Cell items (dotted list) | per item | `Heatmap.razor` | `.it` (with `::before` dot) | `HeatmapCellView.Items` |
| Empty-cell placeholder | single dash | `Heatmap.razor` | `.it.empty` | `HeatmapCellView.IsEmpty` |
| Overflow "+K more" | last item | `Heatmap.razor` | `.it.overflow` | `HeatmapCellView.OverflowCount` |
| Error banner (conditional) | full-width x ~36px | `ErrorBanner.razor` | `.error-banner` | `DashboardLoadError` |

### Interaction summary

The only interactive element on the entire page is the `<a href="@Project.BacklogUrl">` in the header. Everything else is intentionally inert to preserve screenshot cleanliness (Scenario 15). No `@onclick`, no `<button>`, no JavaScript, no SignalR circuit. If a future version adds interactivity, it must be opt-in via a query param (e.g., `?mode=interactive`) that adds `@rendermode InteractiveServer` - and such a mode is explicitly disabled for the screenshot path.