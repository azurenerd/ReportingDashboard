# Architecture

## Overview & Goals

"My Project" is a local, single-user, screenshot-oriented executive status dashboard built as a Blazor Server (.NET 8) application. It renders a fixed 1920x1080 single-page view — header, multi-track SVG milestone timeline, and 4x4 monthly heatmap — from a hand-edited `data.json` file. The architecture is deliberately minimal to maximize visual fidelity to `OriginalDesignConcept.html` and minimize engineering surface area.

**Architectural goals:**

1. **Pixel-faithful rendering** of the reference design at 1920x1080 via verbatim CSS port and hand-written inline SVG — no component libraries, no charting libraries.
2. **Zero operational footprint** — no auth, no database, no cloud, no Docker. Runs via `dotnet run` on `http://localhost:5000`.
3. **Content/presentation separation** — a single `data.json` drives all visible text, milestones, and heatmap items; hot-reloaded via `FileSystemWatcher` with a 250 ms debounce.
4. **Crash-proof authoring loop** — malformed JSON surfaces a friendly banner while the last-known-good render stays on screen; the process never crashes on bad input.
5. **Screenshot-clean output** — no Blazor reconnect modal, no dev banner, no scrollbars appear in the rendered page, enabling one-command Playwright capture.
6. **Testable visual contract** — `TimelineLayout.ComputeX` covered by xUnit; `Heatmap.razor` and `TimelineSvg.razor` guarded by bUnit snapshot tests.
7. **Tiny codebase** — target ≤ 1,500 LOC (excluding tests); published self-contained exe ≤ 100 MB.

**High-level shape:**

```
Browser (Chromium @ 1920x1080)
   │ SignalR (localhost)
   ▼
Kestrel  ──►  Blazor Server Circuit  ──►  Index.razor
                                             │
                                             ├─► DashboardHeader.razor
                                             ├─► TimelineSvg.razor ──► TimelineLayout (pure C#)
                                             └─► Heatmap.razor
                                             ▲
                                             │ OnChanged event
                               DashboardDataService (singleton)
                                             ▲
                                             │ FileSystemWatcher (debounced 250 ms)
                                             │
                                       data.json (app root)
```

---

## System Components

### Solution layout (`.sln`)

```
ReportingDashboard.sln
├─ src/
│  └─ ReportingDashboard.Web/              (net8.0, Microsoft.NET.Sdk.Web, Blazor Server)
│     ├─ Program.cs
│     ├─ App.razor
│     ├─ _Imports.razor
│     ├─ Pages/
│     │  ├─ _Host.cshtml                   (root host page; sets viewport, loads site.css)
│     │  ├─ Index.razor                    (route "/"; composes the three sections)
│     │  └─ Error.razor
│     ├─ Shared/
│     │  └─ MainLayout.razor               (bare shell: @Body only, no nav)
│     ├─ Components/
│     │  ├─ DashboardHeader.razor
│     │  ├─ TimelineLegend.razor
│     │  ├─ TimelineSvg.razor
│     │  ├─ Heatmap.razor
│     │  └─ ParseErrorBanner.razor
│     ├─ Models/
│     │  ├─ DashboardModel.cs
│     │  ├─ TimelineModel.cs
│     │  ├─ MilestoneTrack.cs
│     │  ├─ Milestone.cs                   (enum MilestoneKind: PoC | Production | Checkpoint)
│     │  ├─ HeatmapModel.cs
│     │  ├─ HeatmapCategory.cs             (enum: Shipped | InProgress | Carryover | Blockers)
│     │  └─ DashboardState.cs              (Model + ErrorInfo? + LoadedAt)
│     ├─ Services/
│     │  ├─ IDashboardDataService.cs
│     │  ├─ DashboardDataService.cs        (loads, watches, debounces, parses, exposes event)
│     │  ├─ TimelineLayout.cs              (static, pure: ComputeX, ComputeTrackY)
│     │  └─ JsonParseErrorExtractor.cs     (maps JsonException → line/col/message)
│     ├─ wwwroot/
│     │  ├─ css/site.css                   (verbatim port of OriginalDesignConcept styles + overrides)
│     │  └─ favicon.ico
│     ├─ data.json                         (sample: "Project Aurora — Customer Portal Revamp")
│     ├─ data.sample.json                  (read-only reference copy)
│     └─ appsettings.json
├─ tests/
│  └─ ReportingDashboard.Tests/            (net8.0, xUnit, bUnit, FluentAssertions, Verify.Xunit)
│     ├─ TimelineLayoutTests.cs
│     ├─ DashboardDataServiceTests.cs
│     ├─ HeatmapRenderTests.cs
│     ├─ TimelineSvgRenderTests.cs
│     └─ TestData/
│        ├─ golden.json
│        └─ snapshots/
├─ tools/
│  └─ screenshot.ps1                       (Playwright-for-.NET driver)
├─ docs/
│  ├─ adr/0001-no-component-libraries.md
│  ├─ design-screenshots/OriginalDesignConcept.png
│  └─ screenshots/                         (output of screenshot.ps1)
├─ .editorconfig
├─ .gitignore
└─ README.md
```

### Component catalog

| # | Component | Type | Responsibility | Key Interfaces | Dependencies | Data it owns |
|---|---|---|---|---|---|---|
| 1 | `Program.cs` | Startup | Build `WebApplication`, register DI, bind Kestrel to `http://localhost:5000`, disable HTTPS redirect, skip `UseAuthentication/UseAuthorization`, map Blazor Hub + `_Host` fallback. | — | ASP.NET Core | — |
| 2 | `_Host.cshtml` | Razor Page | Serve initial HTML at `/`; declare `<!DOCTYPE html>`, `<meta charset>`, link `site.css`, render `<component type="App" render-mode="ServerPrerendered"/>`, include `_framework/blazor.server.js`. | — | Blazor Server | — |
| 3 | `App.razor` | Router | Root `<Router>` component with `MainLayout` as `DefaultLayout`. | — | Blazor | — |
| 4 | `MainLayout.razor` | Layout | Renders only `@Body` in a bare `<div>`. No nav, no sidebar. | — | Blazor | — |
| 5 | `Index.razor` | Page | Route `/`. Subscribes to `IDashboardDataService.OnChanged`, composes `DashboardHeader`, `TimelineSvg`, `Heatmap`; renders `ParseErrorBanner` when `State.Error != null`. Implements `IDisposable` to unsubscribe. | — | `IDashboardDataService` | `DashboardState` snapshot |
| 6 | `DashboardHeader.razor` | Component | Renders `.hdr` block: `<h1>` title + " ADO Backlog" link, `.sub` subtitle, legend (PoC / Production / Checkpoint / NOW). | `[Parameter] string Title`, `Subtitle`, `BacklogUrl`, `CurrentMonthLabel` | — | — |
| 7 | `TimelineLegend.razor` | Component | Renders the 4-item legend row; static colors (does not read from model). | — | — | — |
| 8 | `TimelineSvg.razor` | Component | Renders `.tl-area`: 230px left gutter with per-track labels and the 1560x185 inline `<svg>` with month gridlines, track baselines, milestones (diamonds/circles), NOW line. Uses `TimelineLayout.ComputeX`. | `[Parameter] TimelineModel Timeline`, `DateOnly CurrentDate` | `TimelineLayout` | — |
| 9 | `Heatmap.razor` | Component | Renders `.hm-wrap`: title, 5-col × 5-row CSS Grid (`160px repeat(4,1fr)` × `36px repeat(4,1fr)`), headers (`.hm-corner`, `.hm-col-hdr[.apr-hdr]`), row headers, data cells with category bullets, empty-cell dash. | `[Parameter] HeatmapModel Heatmap` | — | — |
| 10 | `ParseErrorBanner.razor` | Component | Top-anchored `position:fixed` banner showing "data.json parse error at line N, column M: {message}". Hidden when `Error == null`. | `[Parameter] ParseError? Error` | — | — |
| 11 | `IDashboardDataService` | Interface | Abstract seam: `DashboardState Current { get; }`, `event Action OnChanged`, `void Reload()`. | — | — | — |
| 12 | `DashboardDataService` | Singleton service | Owns `data.json` lifecycle: initial load in ctor, starts `FileSystemWatcher` on `Changed/Created/Renamed`, 250 ms debounce `Timer`, re-parses on fire, swaps `Current` atomically, keeps last-known-good on failure, raises `OnChanged`. Thread-safe via single `lock` around swap. Logs `Information` on success, `Warning` on parse failure. | `IDashboardDataService` | `IHostEnvironment`, `ILogger`, `System.Text.Json`, `FileSystemWatcher` | `DashboardState` (in-memory) |
| 13 | `TimelineLayout` | Static pure class | `ComputeX(DateOnly date, DateOnly rangeStart, DateOnly rangeEnd, int pixelWidth)` → `double`. `ComputeTrackY(int trackIndex)` → 42 + index*56. `ComputeSvgHeight(int trackCount)` → trackCount*56 + 40. No I/O, fully testable. | — | — | — |
| 14 | `JsonParseErrorExtractor` | Static helper | Maps `JsonException` → `ParseError { Line, Column, Message, Path }` by reading `JsonException.LineNumber`, `BytePositionInLine`, `Path`. Also handles `FileNotFoundException` → friendly "file missing" error. | — | `System.Text.Json` | — |
| 15 | `site.css` | Static asset | Verbatim port of `OriginalDesignConcept.html`'s `<style>` block + additions: `#components-reconnect-modal{display:none!important;}`, `#blazor-error-ui{display:none!important;}`, `.err-banner` styling, `.it.empty` dashed placeholder. | — | — | — |
| 16 | `data.json` | Data file | Canonical content source. Ships with Project Aurora sample. | — | — | Dashboard content |
| 17 | `screenshot.ps1` | Dev tool | Playwright-for-.NET script: headless Chromium, viewport 1920x1080, goto `http://localhost:5000/`, `WaitForLoadStateAsync(NetworkIdle)`, `ScreenshotAsync` → `docs/screenshots/{yyyy-MM-dd}.png`. | — | Playwright 1.47.x | — |
| 18 | `ReportingDashboard.Tests` | Test project | xUnit tests for `TimelineLayout`, `DashboardDataService`, `HeatmapRenderTests` (bUnit snapshots), `TimelineSvgRenderTests` (bUnit snapshots + Verify). | — | xUnit, bUnit, FluentAssertions, Verify.Xunit | Golden fixtures |

---

## Component Interactions

### Startup sequence

1. `Program.cs` builds `WebApplication`:
   - `builder.WebHost.UseUrls("http://localhost:5000")` (explicit; no `0.0.0.0`).
   - `builder.Services.AddRazorPages()` + `AddServerSideBlazor()`.
   - `builder.Services.AddSingleton<IDashboardDataService, DashboardDataService>()`.
   - `builder.Logging.AddConsole()` at `Information`.
2. `app.UseStaticFiles()`, `app.MapBlazorHub()`, `app.MapFallbackToPage("/_Host")`. `UseHttpsRedirection` NOT called. `UseAuthentication/UseAuthorization` NOT called.
3. `DashboardDataService` ctor runs: resolves `ContentRootPath/data.json`, loads & parses, starts `FileSystemWatcher` on the directory with `Filter = "data.json"`, `NotifyFilter = LastWrite | FileName | Size`, attaches `Changed/Created/Renamed` handlers.
4. Kestrel listens on 5000.

### Request lifecycle (happy path)

1. Browser GETs `/` → `_Host.cshtml` server-prerenders `App` → router resolves `Index.razor`.
2. `Index.OnInitialized()`: `_state = DataService.Current; DataService.OnChanged += HandleChanged;`.
3. Razor renders `DashboardHeader`, `TimelineSvg`, `Heatmap` with the current model.
4. Response sent; Blazor Server starts a SignalR circuit; first paint target < 500 ms.

### Hot-reload sequence (Scenario 2)

```
User saves data.json
   │
   ▼
FileSystemWatcher fires Changed (possibly twice on Windows)
   │
   ▼
DashboardDataService: reset 250 ms Timer (debounce)
   │
   ▼  (timer elapsed)
ReloadInternal():
   read file bytes → JsonSerializer.Deserialize<DashboardJson>()
   → map to DashboardModel
   → build new DashboardState { Model = new, Error = null, LoadedAt = UtcNow }
   → lock swap _current = new
   → log "reloaded OK"
   → invoke OnChanged
   │
   ▼
Index.HandleChanged():
   _state = DataService.Current
   await InvokeAsync(StateHasChanged)
   │
   ▼
Blazor Server diffs render tree → SignalR sends DOM patches → browser updates in place
```

Total wall-clock: ~250 ms debounce + ~5 ms parse + ~10 ms render + SignalR roundtrip ≪ 1 s.

### Error sequence (Scenario 3)

On `JsonException` (or `IOException` for locked file):

1. `JsonParseErrorExtractor.Extract(ex)` → `ParseError`.
2. Build new `DashboardState` **reusing** `_current.Model` but setting `Error = parseError`.
3. Swap `_current`, invoke `OnChanged`.
4. `Index.razor` re-renders; `ParseErrorBanner` becomes visible; dashboard below continues to show last-known-good.
5. Next successful parse clears `Error`; banner disappears.

If `data.json` is missing at startup (Scenario 14), `DashboardDataService` ctor does **not** throw — it seeds `_current` with an empty-but-valid `DashboardModel` and an `Error` stating "data.json not found; create from data.sample.json".

### Shutdown

`DashboardDataService` implements `IDisposable`: stops watcher, disposes debounce timer. Registered via `AddSingleton` so DI calls `Dispose` on host shutdown.

### Communication patterns

- **In-process event**: `IDashboardDataService.OnChanged` (plain `Action`). No `IObservable`/Rx, no `IMediator` — overkill.
- **Blazor rendering**: Parameters flow **down** from `Index` to child components; no `CascadingValue`, no component-to-component events.
- **Transport**: SignalR circuit is the Blazor Server default; no custom hub, no HTTP API.

---

## Data Model

All dates are `DateOnly` (System) for timeline math; `DateTime` avoided to eliminate TZ ambiguity.

### C# model (POCOs in `Models/`)

```csharp
public sealed record DashboardModel(
    string Title,
    string Subtitle,
    string BacklogUrl,
    DateOnly CurrentDate,
    TimelineModel Timeline,
    HeatmapModel Heatmap);

public sealed record TimelineModel(
    DateOnly RangeStart,
    DateOnly RangeEnd,
    IReadOnlyList<MilestoneTrack> Tracks);

public sealed record MilestoneTrack(
    string Id,                 // "M1"
    string Label,              // "Chatbot & MS Role"
    string Color,              // "#0078D4" (hex; validated against ^#[0-9A-Fa-f]{6}$)
    IReadOnlyList<Milestone> Milestones);

public enum MilestoneKind { Checkpoint, PoC, Production }

public sealed record Milestone(
    DateOnly Date,
    MilestoneKind Kind,
    string Label);

public sealed record HeatmapModel(
    IReadOnlyList<string> Months,          // length == 4
    int CurrentMonthIndex,                 // 0..3
    IReadOnlyDictionary<HeatmapCategory, IReadOnlyList<IReadOnlyList<string>>> Rows);
    // Rows[cat].Count == 4 (one list per month); inner list == items in cell

public enum HeatmapCategory { Shipped, InProgress, Carryover, Blockers }

public sealed record ParseError(int Line, int Column, string Message, string? JsonPath);

public sealed record DashboardState(
    DashboardModel Model,
    ParseError? Error,
    DateTime LoadedAtUtc);
```

### JSON schema (`data.json`)

```jsonc
{
  "title":      "Project Aurora — Customer Portal Revamp",
  "subtitle":   "Trusted Platform · Customer Experience Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/contoso/aurora/_backlogs",
  "currentDate":"2026-04-19",
  "timeline": {
    "rangeStart": "2026-01-01",
    "rangeEnd":   "2026-06-30",
    "tracks": [
      {
        "id": "M1", "label": "Auth & Identity", "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "kind": "Checkpoint", "label": "Jan 12 design review" },
          { "date": "2026-03-26", "kind": "PoC",        "label": "Mar 26 PoC" },
          { "date": "2026-04-30", "kind": "Production", "label": "Apr Prod" }
        ]
      },
      { "id": "M2", "label": "Portal UI",       "color": "#00897B", "milestones": [ /* … */ ] },
      { "id": "M3", "label": "Data Migration",  "color": "#546E7A", "milestones": [ /* … */ ] }
    ]
  },
  "heatmap": {
    "months":            ["Jan", "Feb", "Mar", "Apr"],
    "currentMonthIndex": 3,
    "rows": {
      "shipped":    [["SSO rollout"], ["MFA prompt v2"], ["Settings UI"], ["Portal v3 GA"]],
      "inProgress": [[],              [],                ["Data import"], ["Billing widget", "Audit log"]],
      "carryover":  [[],              [],                [],              ["Legacy export"]],
      "blockers":   [[],              [],                [],              ["SAP cert expiry"]]
    }
  }
}
```

### Storage & persistence

- **Primary store**: `data.json` on local disk, co-located with the executable (`ContentRootPath`).
- **In-memory**: single `DashboardState` instance held by the singleton service; replaced atomically under `lock`.
- **No database, no cache layer, no disk-based persistence** beyond `data.json`.
- **Sample file**: `data.sample.json` ships read-only as a reference; never watched.

### Validation rules (applied in `DashboardDataService` after deserialization)

| Field | Rule | Behavior on violation |
|---|---|---|
| `title`, `subtitle` | Non-null string | Default `""`; log warning. |
| `backlogUrl` | Valid URI or empty | Render link as `#` on violation. |
| `currentDate` | Parseable `DateOnly` | Fallback to `DateOnly.FromDateTime(DateTime.Today)`. |
| `timeline.rangeStart` < `rangeEnd` | Must hold | Raise `ParseError`; keep last-known-good. |
| `tracks[].color` | Matches `^#[0-9A-Fa-f]{6}$` | Fallback to `#999`. |
| `milestones[].kind` | In enum | `JsonStringEnumConverter` throws → `ParseError`. |
| `heatmap.months` | Length == 4 | Raise `ParseError`. |
| `heatmap.currentMonthIndex` | 0 ≤ n < `months.Length` | Clamp + log warning. |
| `heatmap.rows` | Each category key present; each array length == `months.Length` | Missing keys → empty rows; wrong-length arrays → pad/truncate + warning. |

---

## API Contracts

**This application exposes no business HTTP API.** It is a single-page Blazor Server app; the only HTTP surface is what ASP.NET Core + Blazor require.

### HTTP endpoints (framework-level)

| Method | Path | Handler | Purpose | Response |
|---|---|---|---|---|
| GET | `/` | `Pages/_Host.cshtml` → `Index.razor` | Serve prerendered dashboard | `200 text/html` |
| GET | `/_framework/*` | Blazor static middleware | `blazor.server.js` + assets | `200` |
| GET | `/css/site.css` | Static files | Stylesheet | `200 text/css` |
| GET | `/favicon.ico` | Static files | Icon | `200`/`404` |
| WebSocket | `/_blazor` | `MapBlazorHub()` | SignalR circuit for Blazor Server | `101 Switching Protocols` |
| GET | `/Error` | `Error.razor` | Fallback error page (framework) | `200 text/html` |

### Internal service contract

```csharp
public interface IDashboardDataService
{
    DashboardState Current { get; }          // never null; may contain Error
    event Action OnChanged;                  // fired after every successful swap (valid or error state)
    void Reload();                           // forces a synchronous reload (used by tests)
}
```

**Thread-safety guarantees:**
- `Current` is replaced via `Interlocked.Exchange` (or single `lock`); readers always see a consistent snapshot.
- `OnChanged` is invoked synchronously on the watcher's timer thread; subscribers must marshal to their render context (`Index.razor` uses `InvokeAsync(StateHasChanged)`).

### Error handling contract

| Source | Exception | Mapped to | User-visible result |
|---|---|---|---|
| File missing at startup | `FileNotFoundException` | `ParseError(0, 0, "data.json not found at …")` | Banner; empty dashboard shell |
| File locked by editor | `IOException` | Retry up to 3× at 50 ms; if still failing, `ParseError` | Banner; last-known-good dashboard |
| Malformed JSON | `JsonException` | `ParseError(LineNumber+1, BytePositionInLine+1, Message, Path)` | Banner; last-known-good dashboard |
| Enum out-of-range | `JsonException` (via `JsonStringEnumConverter`) | Same as above | Banner |
| Schema rule violation | Custom validator | `ParseError(0, 0, "validation: …")` | Banner |
| Unhandled exception in render | Caught by Blazor | Framework `Error.razor` | Fallback error page (should never happen in practice) |

**Contract guarantee:** No exception path terminates the process. `DashboardDataService.ReloadInternal` catches `Exception` at the outer boundary, records the error into `DashboardState.Error`, logs at `Warning`, and raises `OnChanged`.

---

## Infrastructure Requirements

### Hosting

- **Web host**: Kestrel (in-process), launched by `dotnet run` from `src/ReportingDashboard.Web` or a published self-contained exe.
- **Bind address**: `http://localhost:5000` only. Loopback-only; `0.0.0.0` prohibited.
- **TLS**: none. `UseHttpsRedirection` is not registered; no dev certificate required.
- **Process model**: single process, single AppDomain, single circuit (one browser tab expected).
- **Startup command**: `dotnet run --project src/ReportingDashboard.Web -c Release`.
- **Self-contained publish**:
  ```
  dotnet publish src/ReportingDashboard.Web -c Release -r win-x64 \
      --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
  ```
  Produces a single `.exe` (≤ 100 MB) that the author double-clicks to launch.

### Networking

- Inbound: TCP 5000 on the loopback interface. Firewall rules unchanged (loopback is not prompted).
- Outbound: none. The app makes no network calls.

### Storage

- **Local disk only.** Files required adjacent to the executable:
  - `data.json` (author-editable)
  - `data.sample.json` (read-only reference)
  - `wwwroot/` (static assets: `css/site.css`, `favicon.ico`)
  - `appsettings.json` (minimal; Kestrel URL + logging level)
- **No temp files, no write-back paths.** The app only reads `data.json`.
- **File size expectation**: `data.json` < 50 KB typical; `DashboardDataService` enforces a 1 MB ceiling (rejects larger files with a `ParseError`).

### OS & runtime

- Windows 10 / Windows 11 with .NET 8 SDK 8.0.x (authoring) or the self-contained publish (end-user).
- Segoe UI assumed present; fallback `'Selawik', Arial, sans-serif`.
- Playwright (optional) requires `powershell 5.1+` and the bundled Chromium download.

### CI/CD

A single GitHub Actions workflow `.github/workflows/ci.yml`:

```yaml
name: ci
on: [push, pull_request]
jobs:
  build-test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - run: dotnet restore ReportingDashboard.sln
      - run: dotnet build  ReportingDashboard.sln -c Release --no-restore /warnaserror
      - run: dotnet test   ReportingDashboard.sln -c Release --no-build --logger "trx;LogFileName=test.trx"
      - run: dotnet format --verify-no-changes
```

- Branch protection requires the job to pass before merge to `main`.
- No deployment stage — distribution is manual copy of the published folder.

### Development environment

- `.editorconfig` enforces 4-space C#, `file_scoped_namespace`, `csharp_style_prefer_primary_constructors = true`.
- `Microsoft.CodeAnalysis.NetAnalyzers` enabled; `TreatWarningsAsErrors = true` in Release.
- `launchSettings.json` defines a single profile: `applicationUrl=http://localhost:5000`, `launchBrowser=true`.

---

## Technology Stack Decisions

All decisions below conform to the mandatory stack: **C# .NET 8, Blazor Server, local-only, no cloud services, .sln project structure.**

| Layer | Decision | Version | Justification |
|---|---|---|---|
| Runtime | .NET 8 SDK | 8.0.x LTS | Mandated; LTS through Nov 2026; `DateOnly`, source generators, improved System.Text.Json. |
| Solution structure | `.sln` with `src/` + `tests/` | — | Mandated; standard for .NET multi-project repos; keeps test project separate from shippable code. |
| UI framework | Blazor Server (Interactive Server render mode) | 8.0 | Mandated. Faster first paint than WASM (critical for screenshot performance ≤ 500 ms); trivial DI access to file I/O; no API tier needed. |
| Web host | Kestrel | 8.0 | Built-in; no IIS/Nginx dependency; binds localhost in one line. |
| JSON | `System.Text.Json` | built-in | Zero extra dependency; line/column info on `JsonException`; `JsonStringEnumConverter` handles `MilestoneKind` / `HeatmapCategory`; source generation optional but not required. |
| File watching | `System.IO.FileSystemWatcher` | built-in | Standard Windows pattern; combined with 250 ms `System.Threading.Timer` debounce to coalesce double-fires. |
| Logging | `Microsoft.Extensions.Logging.Console` | built-in | Sufficient for local debugging; structured messages for reload/parse events. No telemetry dependency. |
| Configuration | `appsettings.json` via default host builder | built-in | Only used for Kestrel URL and log level; dashboard content lives in `data.json`. |
| Styling | Hand-written CSS in `wwwroot/css/site.css` | — | Verbatim port of reference `<style>` block is the single most important fidelity lever. No Tailwind, no SCSS. |
| SVG rendering | Inline SVG inside `TimelineSvg.razor` | — | The design is a bespoke illustration, not a chart. Hand-writing guarantees pixel-exact control over diamond polygons, stroke widths, and NOW dasharray. |
| Test framework | xUnit | 2.9.x | Standard .NET choice; bUnit integration; parallel-friendly. |
| Component testing | bUnit | 1.33.x | Renders Razor components into a test host for snapshot assertions on HTML/SVG. Targets .NET 8. |
| Assertions | FluentAssertions | 6.12.x | Readable assertion DSL. |
| Snapshot testing | Verify.Xunit | 26.x | Persists rendered SVG strings as `.verified.txt` golden files; diff-reviewed in PRs. |
| Formatting | `dotnet format` + `.editorconfig` | built-in | Enforced in CI via `--verify-no-changes`. |
| Static analysis | `Microsoft.CodeAnalysis.NetAnalyzers` | 8.0.x | Catches common .NET pitfalls; zero cost to enable. |
| Screenshot tool (optional) | Microsoft.Playwright | 1.47.x | Only pulled into the `tools/` script, not referenced by `ReportingDashboard.Web`. Provides deterministic 1920x1080 Chromium for PNG capture. |

### Explicitly rejected (and why)

- **Blazor WASM** — slower first paint; cannot read local files without a server tier; eliminated by business need for instant screenshot.
- **MudBlazor / Radzen / Blazorise** — opinionated component kits re-skin markup and inject their own CSS, breaking pixel parity with the reference design. Enforced by `docs/adr/0001-no-component-libraries.md`.
- **ChartJs.Blazor / ApexCharts / Plotly** — the timeline is a static illustration, not a data-driven chart. Libraries add weight and fight the design.
- **HTMX / Alpine / any JS framework** — Blazor Server already provides interactivity; redundant.
- **Entity Framework Core / SQLite / LiteDB** — no data to persist.
- **ASP.NET Identity / cookie auth / Azure AD** — no auth per spec.
- **Docker / IIS / Azure App Service / Kubernetes** — local-only per spec.
- **App Insights / OpenTelemetry / Serilog sinks** — no cloud telemetry; console logging is sufficient.
- **AutoMapper / MediatR** — domain is too small; DI + direct calls are clearer.

---

## Security Considerations

### Threat model

- **Trust boundary**: the loopback interface on the author's Windows machine. Any local process can connect; no remote attackers are in scope because port 5000 is never exposed.
- **Assets**: `data.json` contents (author-authored, not confidential by policy — the author is responsible for not putting secrets in it).
- **Adversaries**: none in the threat model. This is a personal authoring tool.

### Authentication / authorization

- **None.** `app.UseAuthentication()` and `app.UseAuthorization()` are **not** registered in `Program.cs`. Any request reaching port 5000 is served. This is intentional and documented.

### Network exposure

- `builder.WebHost.UseUrls("http://localhost:5000")` — loopback only.
- Code review rule: any PR adding `"http://0.0.0.0:…"`, `"http://*:…"`, or `"https://+:…"` to `UseUrls`, `launchSettings.json`, or `appsettings.json` must be rejected. Enforced via a simple grep check in CI (optional):
  ```
  ! grep -r "0\.0\.0\.0" src/ || (echo "bind address violation"; exit 1)
  ```

### Transport

- **HTTP only**, no TLS, on loopback. HTTPS redirect middleware disabled to avoid dev-cert prompts. Acceptable because traffic never leaves the machine.

### Input validation

- `data.json` is the sole input. Validation rules (see **Data Model → Validation rules**) are enforced before the model is exposed to components.
- JSON size capped at 1 MB in `DashboardDataService` to prevent accidental OOM.
- All string fields are rendered by Razor via `@` interpolation, which HTML-encodes by default — prevents XSS even if the author pastes `<script>` into a heatmap item.
- `backlogUrl` is validated via `Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == "http" || u.Scheme == "https")`. Invalid URIs render as `href="#"` — prevents `javascript:` URL injection into the " ADO Backlog" anchor.
- Track `color` values are validated against `^#[0-9A-Fa-f]{6}$` before being written into SVG `stroke="…"` and inline style, preventing CSS/SVG injection.

### CSRF / antiforgery

- No forms, no POST endpoints. Default Blazor antiforgery stays enabled (it's free); nothing extra required.

### Data protection

- No PII, no secrets processed by the app. Data Protection keys (used internally by ASP.NET Core for cookie encryption) persist to `%LOCALAPPDATA%/ASP.NET/DataProtection-Keys` by default — acceptable.
- `data.json` may be committed to the author's repo or kept local. Content classification is the author's responsibility; the README documents this explicitly.

### Content Security Policy

- Optional header set in `_Host.cshtml` via `<meta http-equiv="Content-Security-Policy">` with a permissive default (`default-src 'self' 'unsafe-inline'`). `'unsafe-inline'` is required because SVG styling is inline. Acceptable given the loopback-only exposure.

### Dependency security

- CI runs `dotnet list package --vulnerable --include-transitive` weekly (scheduled workflow). Any non-zero exit fails the job.
- All NuGet packages come from the public `nuget.org` feed; no private feeds required.

---

## Scaling Strategy

This system is intentionally non-scaling: **one user, one process, one browser tab, one `data.json`.**

| Axis | Strategy | Rationale |
|---|---|---|
| Concurrent users | 1 | It is a personal authoring tool. No user accounts, no multi-tenancy. |
| Concurrent circuits | 1 expected; up to ~5 tolerated | Blazor Server defaults (in-memory circuit handler) comfortably handle this without tuning. |
| Data volume | `data.json` ≤ 1 MB (enforced); typical < 50 KB | Small enough to re-parse and re-render on every change in microseconds. |
| CPU / memory | Single-threaded render path; model re-read is O(n) over a tiny n | No profiling or optimization needed. Steady-state memory ≈ 60–80 MB. |
| Startup | Cold start ≈ 1–2 s; warm start ≈ 200 ms | Acceptable; screenshot automation keeps the process warm. |
| Process | One `dotnet` process | No load balancer, no reverse proxy, no SignalR backplane. |

### Bottlenecks & guardrails

- **Bounded inputs.** `DashboardDataService` rejects `data.json` > 1 MB. Track count > 10 and cell item count > 16 are logged as warnings (SVG height formula is parametric: `trackCount*56 + 40`, but visual fidelity degrades beyond ~6 tracks).
- **No caching needed.** Re-deserialization on every save is cheaper than any cache invalidation scheme.
- **SignalR circuit memory.** Default per-circuit buffer is ≤ 1 MB; one circuit is negligible.

### If (hypothetically) scaling were ever needed

- Multi-project dropdown → add `IEnumerable<string> ListProjectFiles()` and a query-string parameter `?p=aurora`; a day's work.
- External editors posting updates → add a single minimal-API `PUT /data` endpoint guarded by a local-only check (`HttpContext.Connection.RemoteIpAddress.IsLoopback`); deferred.
- Multi-user serving → explicitly out of scope and would require a rewrite (auth, per-user state, likely WASM).

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation | Owner |
|---|---|---|---|---|---|
| 1 | Screenshot fidelity drifts from `OriginalDesignConcept.html` over time | Medium | High (defeats the product's purpose) | bUnit snapshot tests + Verify golden files for `Heatmap.razor` and `TimelineSvg.razor`; CI fails on diff; ADR forbids component libraries. Side-by-side visual diff of `docs/screenshots/latest.png` vs `docs/design-screenshots/OriginalDesignConcept.png` in every PR touching `site.css` or a `Components/*.razor`. | Engineering |
| 2 | `TimelineLayout.ComputeX` off-by-one at month boundaries or leap-year Feb | Medium | Medium | Unit tests for month starts, month ends, Feb 29 (2024/2028), exact `rangeEnd`, and x=0 / x=pixelWidth cases. ≥ 90% branch coverage target. | Engineering |
| 3 | Malformed `data.json` right before a screenshot blanks the page | Medium | High | Last-known-good model retained in `DashboardDataService`; `ParseErrorBanner` with line/column; process never crashes. Covered by `DashboardDataServiceTests.Malformed_Keeps_LastGood`. | Engineering |
| 4 | Blazor SignalR reconnect modal appears in screenshots | Low | High | `site.css` forces `#components-reconnect-modal{display:none!important;}` and `#blazor-error-ui{display:none!important;}`. Playwright script also asserts neither element is visible before capturing. | Engineering |
| 5 | `FileSystemWatcher` double-fires and causes render thrash | High | Low | 250 ms `Timer`-based debounce in `DashboardDataService`; covered by `DashboardDataServiceTests.TwoChanges_WithinDebounce_ReloadsOnce`. | Engineering |
| 6 | File locked by the editor during save causes `IOException` | Medium | Medium | Retry read 3× at 50 ms intervals before surfacing error. | Engineering |
| 7 | Browser window not exactly 1920×1080 at screenshot time | High | High | `body{width:1920px;height:1080px;overflow:hidden}` hard-fixed; `tools/screenshot.ps1` sets viewport exactly 1920×1080 in Playwright, guaranteeing PNG dimensions regardless of the human's monitor. README documents the manual-browser workflow. | Author + Engineering |
| 8 | Segoe UI missing on non-Windows machine → wrong typography in screenshot | Low | Medium | Font stack `'Segoe UI', 'Selawik', Arial, sans-serif`; README declares Windows-only; Playwright screenshot uses bundled Chromium on Windows-only CI lane. | Engineering |
| 9 | Component library sneaks in via a future PR | Medium | High | `docs/adr/0001-no-component-libraries.md` + CODEOWNERS review; a CI grep guard (`! grep -E "MudBlazor\|Radzen\|Blazorise\|ChartJs\|ApexCharts\|Plotly" src/*/*.csproj`) fails builds that add them. | Engineering |
| 10 | "Current month" logic gets hardcoded to April via copy-paste | High | Medium | Driven by `heatmap.currentMonthIndex` in JSON; Heatmap component loops and applies `.apr-hdr` / `.apr` based on `@if (i == model.CurrentMonthIndex)` — never hardcoded. bUnit test shifts index and asserts class placement. | Engineering |
| 11 | Track count exceeds design (e.g., 6 tracks) and overflows SVG viewbox | Medium | Medium | `TimelineLayout.ComputeSvgHeight(trackCount)` returns `trackCount*56 + 40`; `TimelineSvg.razor` binds `height` to this value. Warning logged when `trackCount > 4` (design-verified envelope). | Engineering |
| 12 | Port 5000 already in use on author's machine | Low | Low | `appsettings.json → Kestrel:Endpoints:Http:Url` is configurable; README documents `--urls http://localhost:5050` override. | Author |
| 13 | Binding accidentally exposed to LAN (`0.0.0.0`) | Low | High | Hardcoded `UseUrls("http://localhost:5000")` in `Program.cs`; CI grep guard against `0.0.0.0` / `*:` / `+:` in `src/`. | Engineering |
| 14 | XSS via malicious JSON (author pastes `<script>` into a heatmap item) | Low | Medium | Razor's `@` auto-encodes; `backlogUrl` scheme validated (http/https only); colors regex-validated before injection into SVG attributes. | Engineering |
| 15 | JSON file growth (pathological `data.json`) causes long re-parse | Low | Low | 1 MB file-size cap enforced before deserialize; per-category cell count capped at 32 items with warning log beyond. | Engineering |
| 16 | Published self-contained exe grows beyond 100 MB target | Low | Low | `PublishTrimmed` evaluated only if size exceeds budget; currently expected 70–85 MB for `win-x64`. Measured in CI as a non-gating metric. | Engineering |
| 17 | Playwright's Chromium download blocked in restricted environments | Medium | Low (optional tool) | `tools/screenshot.ps1` is clearly optional; README documents manual screenshot fallback (Win+Shift+S). | Author |

---

## UI Component Architecture

Each visual section from `OriginalDesignConcept.html` maps to a specific `.razor` component below. All dimensions, colors, and class names are preserved verbatim from the reference CSS.

### Section map

| # | Visual section (reference) | Root element | Component | Layout strategy | Data bindings | Interactions |
|---|---|---|---|---|---|---|
| H | Header bar (`.hdr`) | `<header class="hdr">` | `DashboardHeader.razor` | Flexbox row, `justify-content: space-between`, `padding: 12px 44px 10px`, `border-bottom: 1px solid #E0E0E0`, `flex-shrink: 0`. Left stack (title + subtitle) + right stack (`TimelineLegend`). | `Title`, `Subtitle`, `BacklogUrl` from `DashboardModel` | Click " ADO Backlog" → `window.open(BacklogUrl, "_self")` (plain `<a href>`). |
| H1 | Page title + " ADO Backlog" link | `<h1>` inside `.hdr` left block | Inline in `DashboardHeader.razor` | `font-size:24px;font-weight:700;`. Inline `<a href="@BacklogUrl" style="color:#0078D4">` appended. | `Title`, `BacklogUrl` | Anchor click. |
| H2 | Subtitle | `<div class="sub">` | Inline in `DashboardHeader.razor` | `font-size:12px;color:#888;margin-top:2px;` | `Subtitle` | None. |
| H3 | Legend (PoC / Prod / Checkpoint / NOW) | `<div class="legend">` | `TimelineLegend.razor` | Flex row, `gap:22px;align-items:center;`. Each item: 12px text + colored shape (rotated square 12×12 for PoC #F4B400 and Prod #34A853; 8×8 circle for Checkpoint #999; 2×14 rectangle for NOW #EA4335). | Static (fixed palette per spec) | None. |
| T | Timeline area (`.tl-area`) | `<section class="tl-area">` | `TimelineSvg.razor` | Flex row, `height:196px`, `padding:6px 44px 0`, `background:#FAFAFA`, `border-bottom:2px solid #E8E8E8`. Child 1: 230px gutter; child 2: `.tl-svg-box` with `flex:1`. | `Timeline: TimelineModel`, `CurrentDate: DateOnly` | Native `<title>` tooltip on milestone hover. |
| T1 | Left gutter (track labels) | `<div style="width:230px;…">` | Loop in `TimelineSvg.razor` | Flex column, `justify-content:space-around`, `padding:16px 12px 16px 0`, `border-right:1px solid #E0E0E0`. One block per track. | `@foreach (var t in Timeline.Tracks)` → renders `t.Id` (color `t.Color`) + `t.Label` (color `#444`). | None. |
| T2 | SVG canvas | `<svg width="1560" height="@Height">` inside `.tl-svg-box` | `TimelineSvg.razor` | `Height = TimelineLayout.ComputeSvgHeight(Timeline.Tracks.Count)`. Contains `<defs><filter id="sh">…` (shared drop-shadow). | — | — |
| T3 | Month gridlines + labels | `<line>` × 6 + `<text>` × 6 | Loop in `TimelineSvg.razor` | At x = 0, 260, 520, 780, 1040, 1300. Stroke `#bbb` 1px @ opacity 0.4. Label at `(x+5, 14)`, 11px weight 600 `#666`. | Derived from `Timeline.RangeStart`/`RangeEnd` months | None. |
| T4 | Per-track baseline | `<line x1="0" x2="1560" y1="@Y" y2="@Y" stroke="@track.Color" stroke-width="3"/>` | Loop in `TimelineSvg.razor` | `Y = TimelineLayout.ComputeTrackY(trackIndex)` = `42 + trackIndex*56`. | `track.Color` | None. |
| T5 | Milestone diamond (PoC / Prod) | `<polygon points="x,y-11 x+11,y x,y+11 x-11,y" fill="@kindColor" filter="url(#sh)"><title>@label</title></polygon>` | Loop in `TimelineSvg.razor` | `x = TimelineLayout.ComputeX(milestone.Date, RangeStart, RangeEnd, 1560)`; `y = trackY`. Kind colors: PoC `#F4B400`, Prod `#34A853`. | `Milestone.Date`, `Kind`, `Label` | Native browser tooltip via `<title>`. |
| T6 | Milestone checkpoint | `<circle cx="x" cy="y" r="7" fill="white" stroke="@track.Color" stroke-width="2.5"/>` or filled `<circle r="4" fill="#999"/>` | Loop in `TimelineSvg.razor` | Same x/y math as T5; stroke color = `track.Color`. | `Milestone.Date`, `Label` | Native tooltip. |
| T7 | Milestone date label | `<text x="x" y="@(y-16)" text-anchor="middle" fill="#666" font-size="10">@Label</text>` | Loop in `TimelineSvg.razor` | Alternates above (`y-16`) / below (`y+24`) to avoid collisions when adjacent. | `Milestone.Label` | None. |
| T8 | NOW line + label | `<line stroke-dasharray="5,3" stroke="#EA4335" stroke-width="2"/>` + `<text fill="#EA4335" font-weight="700">NOW</text>` | `TimelineSvg.razor` | `x = TimelineLayout.ComputeX(CurrentDate, RangeStart, RangeEnd, 1560)`; label at `(x+4, 14)`. | `CurrentDate` from `DashboardModel` | None. |
| M | Heatmap wrap (`.hm-wrap`) | `<section class="hm-wrap">` | `Heatmap.razor` | Flex column, `flex:1;min-height:0;padding:10px 44px 10px;`. | `Heatmap: HeatmapModel` | None. |
| M1 | Heatmap title | `<div class="hm-title">` | Inline in `Heatmap.razor` | `font-size:14px;font-weight:700;color:#888;letter-spacing:.5px;text-transform:uppercase;margin-bottom:8px;` | Static text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" | None. |
| M2 | Heatmap grid | `<div class="hm-grid">` | `Heatmap.razor` | CSS Grid: `grid-template-columns:160px repeat(4,1fr); grid-template-rows:36px repeat(4,1fr); border:1px solid #E0E0E0;` | `Heatmap.Months`, `Heatmap.Rows` | None. |
| M3 | Corner cell | `<div class="hm-corner">STATUS</div>` | Inline in `Heatmap.razor` | `background:#F5F5F5;font-size:11px;font-weight:700;color:#999;text-transform:uppercase;` | Static "STATUS" | None. |
| M4 | Month column headers | `<div class="hm-col-hdr @(i==CurrentMonthIndex?"apr-hdr":"")">@month</div>` × 4 | Loop in `Heatmap.razor` | Class toggles on `CurrentMonthIndex`. Current: `background:#FFF0D0;color:#C07700`. | `Heatmap.Months[i]`, `Heatmap.CurrentMonthIndex` | None. |
| M5 | Row headers (Shipped/In Progress/Carryover/Blockers) | `<div class="hm-row-hdr @rowHdrClass">@label</div>` | Loop in `Heatmap.razor` | Fixed order of 4 categories. Class per row: `.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr`. Styling per row-color matrix. | `HeatmapCategory` enum → label | None. |
| M6 | Data cells | `<div class="hm-cell @rowCellClass @(i==CurrentMonthIndex?"apr":"")">` | Nested loop (4 categories × 4 months) in `Heatmap.razor` | Class per row: `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`. `.apr` modifier shifts bg shade. | `Heatmap.Rows[category][monthIndex]` → list of strings | None. |
| M7 | Cell items (bulleted) | `<div class="it">@item</div>` | Inner loop | `font-size:12px;color:#333;padding:2px 0 2px 12px;position:relative;`. `::before` renders 6px colored dot. Bullet color comes from row class (`.ship-cell .it::before{background:#34A853}` etc.). | `string item` | None. |
| M8 | Empty-cell placeholder | `<div class="it empty">—</div>` | Conditional in `Heatmap.razor` | When `Rows[cat][i].Count == 0`. `.it.empty{color:#AAA;}` (no `::before` bullet — overridden by `.it.empty::before{display:none}`). | — | None. |
| E | Parse-error banner | `<div class="err-banner">` | `ParseErrorBanner.razor` | `position:fixed;top:0;left:0;right:0;z-index:1000;background:#FEF2F2;color:#991B1B;padding:6px 16px;font-size:12px;border-bottom:1px solid #EA4335;`. Rendered only when `Error != null`. | `ParseError Error` | None (dismisses automatically on next successful reload). |

### CSS override additions (beyond verbatim port)

Appended to `site.css`:

```css
/* Hide Blazor Server chrome so screenshots are clean */
#components-reconnect-modal, #blazor-error-ui { display: none !important; }

/* Parse-error banner */
.err-banner {
  position: fixed; top: 0; left: 0; right: 0; z-index: 1000;
  background: #FEF2F2; color: #991B1B;
  padding: 6px 16px; font-size: 12px;
  border-bottom: 1px solid #EA4335;
  font-family: 'Segoe UI', 'Selawik', Arial, sans-serif;
}

/* Empty heatmap cell placeholder */
.hm-cell .it.empty { color: #AAA; padding-left: 0; }
.hm-cell .it.empty::before { display: none; }
```

### Component render contract (summary)

- `Index.razor` owns exactly one `DashboardState` snapshot per render; child components receive slices as `[Parameter]` values.
- No component calls back into `Index` or the data service; data flow is strictly unidirectional (service → Index → child).
- All interactive surface (hover tooltips via SVG `<title>`, the " ADO Backlog" anchor) is native HTML/SVG — no JS interop required.