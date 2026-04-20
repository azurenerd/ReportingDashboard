# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-20 00:31 UTC_

### Summary

"My Project" is a **single-page executive status dashboard** rendered by a local **Blazor Server (.NET 8)** application that reads a hand-maintained `data.json` file and produces a 1920×1080 screenshot-ready view matching `OriginalDesignConcept.html`. The page shows (a) a header with title/subtitle/legend, (b) a horizontal multi-track SVG milestone timeline with a "NOW" marker, and (c) a 4-row × 4-column heatmap grid (Shipped / In Progress / Carryover / Blockers × months) with color-coded bullet items and a highlighted "current month" column. **Primary recommendation:** build a single Blazor Server app (`AgentSquad.ReportingDashboard.sln`) with **one Razor page** (`/`), a strongly-typed `DashboardModel` deserialized from `data.json` via **System.Text.Json**, a hand-written **inline SVG** component for the timeline (no charting library — the design is bespoke and small), and a pure **CSS Grid + Flexbox** heatmap that mirrors the reference HTML class-for-class. Ship it with **Kestrel on localhost**, no auth, no database, no Docker. Keep the surface area tiny so the screenshot fidelity is the only thing that matters. Do **not** introduce MudBlazor, Radzen, Blazorise, or any component kit — they will fight the pixel-perfect Segoe UI / custom palette design and add weight the project does not need. Do **not** introduce a charting library (ChartJs.Blazor, Blazor-ApexCharts, Plotly) — the timeline is a static, opinionated SVG that is easier and more faithful to render by hand. ---

### Key Findings

- The reference design is **not a generic chart**; it's a hand-tuned SVG timeline + CSS Grid heatmap. Any component library will obstruct, not accelerate, fidelity.
- The dashboard is **read-only, single-user, screenshot-oriented**. That eliminates ~90% of the concerns a normal Blazor Server app has (auth, SignalR scale-out, CSRF, tenancy, persistence).
- **Blazor Server** is the right choice over Blazor WASM here: faster first paint (critical for screenshots), trivial local file I/O against `data.json`, no WASM payload, no API layer needed.
- `data.json` should be **hot-reloaded via `FileSystemWatcher`** so the author can tweak JSON and hit F5 to re-screenshot without restarting — biggest quality-of-life win available.
- The design uses **fixed 1920×1080** body dimensions. The app must render at that viewport regardless of the user's monitor; enforce with a `body{width:1920px;height:1080px}` rule copied verbatim from the reference and a browser zoom/window-size note in the README.
- The **timeline month positions are computed** (`x = (monthIndex + dayFraction) * 260`). This math belongs in a small pure C# helper (`TimelineLayout.cs`) with unit tests — it is the only non-trivial logic in the app.
- The heatmap's "current month" column (April in the sample) uses **different background shades per row** (`.apr` modifier classes). Model this as a `CurrentMonth` property on the dashboard root and apply the modifier class in Razor with a conditional.
- **Segoe UI** is a Windows system font and will render correctly on the screenshotting machine; no web font loading is required, which also guarantees screenshot consistency.
- The repo already contains `agentsquad_azurenerd_ReportingDashboard.db` — ignore it. The spec explicitly says JSON config, not a database.
- No compliance regime applies (no PII, no auth, no network egress). This is a **local authoring tool**, not a product. ---
- `dotnet new sln -n ReportingDashboard`
- `dotnet new blazorserver -n ReportingDashboard.Web -o src/ReportingDashboard.Web -f net8.0`
- `dotnet new xunit -n ReportingDashboard.Tests -o tests/ReportingDashboard.Tests`; add `bUnit`, `FluentAssertions`.
- Strip the template: delete `Counter.razor`, `FetchData.razor`, `NavMenu.razor`, `MainLayout.razor` sidebar. Keep a minimal shell.
- **Paste `OriginalDesignConcept.html`'s `<style>` block into `wwwroot/css/site.css` verbatim.** This is the most important step.
- Hard-code a `DashboardModel` in C# that mirrors the sample HTML.
- Build `DashboardHeader.razor`, `Heatmap.razor`, `TimelineSvg.razor` against it.
- Goal: pixel-identical render to `OriginalDesignConcept.html` at 1920×1080.
- **Demo:** screenshot side-by-side with the reference PNG; get exec sign-off.
- Implement `DashboardDataService` with `System.Text.Json` + `FileSystemWatcher`.
- Create fictional `data.json` ("Project Aurora — Customer Portal Revamp" or similar) with realistic Shipped/InProgress/Carryover/Blocker items and 3 milestone tracks.
- Wire `Index.razor` to the service.
- JSON validation + friendly error page.
- Unit tests for `TimelineLayout`.
- bUnit snapshot test for `Heatmap` and `TimelineSvg`.
- Hide SignalR reconnect modal via CSS.
- `tools/screenshot.ps1` using Playwright: launches the app, navigates to `http://localhost:5000`, sets viewport 1920×1080, waits for `networkidle`, saves `docs/screenshots/{date}.png`.
- One-command PowerPoint refresh.
- **SVG timeline coordinate math** — spike it in a LINQPad script or a 30-line console app before embedding in a component.
- **FileSystemWatcher reload** — verify it doesn't double-fire on Windows (it usually does; debounce with a 250ms timer).
- Phase 1 pixel match — instantly shows execs what they'll get.
- Hot reload of `data.json` — edit JSON, screen updates live; strong "wow" moment for the author.
- Playwright one-click PNG export — removes the last manual step between JSON edit and PowerPoint slide.
- User accounts, editing UI, history browser, multi-project selector, PDF export, themes. All are trivial extensions once the core is solid; defer until a real need arises.

### Recommended Tools & Technologies

- | Concern | Choice | Version | Notes | |---|---|---|---| | UI framework | Blazor Server | .NET 8.0 (LTS, through Nov 2026) | Interactive Server render mode; no WASM | | Component model | Plain `.razor` components | — | No MudBlazor/Radzen/Blazorise. They override CSS and hurt fidelity. | | Styling | Hand-written CSS in `wwwroot/css/site.css` | — | Copy class names verbatim from `OriginalDesignConcept.html` | | Icons / glyphs | None needed | — | Reference uses CSS shapes (rotated squares, circles) only | | SVG rendering | Inline SVG inside a `TimelineSvg.razor` component | — | No SVG.NET, no Blazor-ApexCharts, no ChartJs.Blazor | | Fonts | Segoe UI (system) | — | Windows-only target; acceptable per spec | | Concern | Choice | Version | Notes | |---|---|---|---| | Runtime | .NET 8 SDK | 8.0.x (latest patch) | LTS | | Web host | ASP.NET Core Kestrel | 8.0 | `dotnet run`, binds `http://localhost:5000` | | JSON | `System.Text.Json` | built-in 8.0 | Source-gen serializer optional; not required | | File watching | `Microsoft.Extensions.FileProviders.Physical` + `PhysicalFileProvider.Watch` | built-in | Triggers reload of `DashboardState` on `data.json` change | | Config | `Microsoft.Extensions.Configuration.Json` | built-in | Only for Kestrel port, not the dashboard data | | Logging | `Microsoft.Extensions.Logging.Console` | built-in | Debug-level is fine locally | | Concern | Choice | Notes | |---|---|---| | Storage | Single file `data.json` in app root | Loaded at startup + on file change | | Schema | POCOs with `JsonPropertyName` attributes | See §4 below | | Validation | `System.ComponentModel.DataAnnotations` + a custom `DashboardValidator` | Fail fast with a friendly error page if JSON is malformed | | Concern | Choice | Version | |---|---|---| | Test framework | xUnit | 2.9.x | | Assertions | FluentAssertions | 6.12.x | | Component tests | bUnit | 1.33.x (targets .NET 8) | | Snapshot (optional) | Verify.Xunit | 26.x — snapshot the generated SVG string to detect regressions | | Concern | Choice | Version | |---|---|---| | Solution layout | `.sln` with `src/` and `tests/` | See §4 | | Formatting | `dotnet format` + `.editorconfig` | built-in | | Analyzers | `Microsoft.CodeAnalysis.NetAnalyzers` | 8.0.x | | Screenshot automation (optional) | Playwright for .NET | 1.47.x — one script to render and save PNG at 1920×1080 for PowerPoint |
- **MudBlazor 7.x / Radzen Blazor 5.x / Blazorise 1.5.x** — opinionated component kits that re-skin everything; they fight the reference design.
- **ChartJs.Blazor / Blazor-ApexCharts / Plotly.Blazor** — the timeline is not a chart, it is a bespoke illustration.
- **HTMX / AlpineJS / any JS framework** — Blazor Server already provides interactivity; none needed anyway.
- **Entity Framework Core / SQLite / LiteDB** — there is no data to persist.
- **Docker / IIS / Azure App Service** — spec says local only.
- **ASP.NET Identity / Azure AD / cookie auth** — spec says no auth. ---
```
ReportingDashboard.sln
├─ src/
│  └─ ReportingDashboard.Web/              (net8.0, Blazor Server)
│     ├─ Program.cs
│     ├─ App.razor
│     ├─ Pages/
│     │  └─ Index.razor                    (the one and only page, route "/")
│     ├─ Components/
│     │  ├─ DashboardHeader.razor          (title + subtitle + legend)
│     │  ├─ TimelineSvg.razor              (inline SVG, takes MilestoneTrack[])
│     │  ├─ TimelineLegend.razor
│     │  └─ Heatmap.razor                  (CSS Grid; 4 rows × 4 month cols)
│     ├─ Models/
│     │  ├─ DashboardModel.cs
│     │  ├─ MilestoneTrack.cs
│     │  ├─ Milestone.cs                   (Kind: PoC | Production | Checkpoint)
│     │  └─ HeatmapItem.cs                 (Category: Shipped|InProgress|Carryover|Blocker)
│     ├─ Services/
│     │  ├─ DashboardDataService.cs        (loads+watches data.json, exposes IObservable-ish event)
│     │  └─ TimelineLayout.cs              (pure: date -> x pixel within SVG)
│     ├─ wwwroot/
│     │  └─ css/site.css                   (verbatim port of OriginalDesignConcept styles)
│     ├─ data.json                         (the only input)
│     └─ appsettings.json
└─ tests/
   └─ ReportingDashboard.Tests/            (xUnit + bUnit + FluentAssertions)
      ├─ TimelineLayoutTests.cs
      ├─ DashboardDataServiceTests.cs
      └─ HeatmapRenderTests.cs             (bUnit snapshot)
```
- `Program.cs` registers `DashboardDataService` as a **singleton**; constructor loads `data.json` once and starts a `FileSystemWatcher`.
- `Index.razor` `@inject`s the service and subscribes to its `OnChanged` event; calls `StateHasChanged()` on change.
- `Index.razor` composes: `<DashboardHeader/>`, `<TimelineSvg/>`, `<Heatmap/>` — each takes its slice of the model as a `[Parameter]`.
- SVG coordinate math lives only in `TimelineLayout.ComputeX(DateOnly date, DateOnly rangeStart, DateOnly rangeEnd, int pixelWidth)` — unit-tested.
- Heatmap iterates `model.Months` (4 entries) × `model.Categories` (4 fixed) and emits a `<div class="hm-cell @(IsCurrent ? "apr" : "")">` — the `apr` class name is kept from the reference for CSS-compat but semantically means "current month".
```jsonc
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform • Privacy Automation Workstream • April 2026",
  "backlogUrl": "https://…",
  "currentDate": "2026-04-19",
  "timeline": {
    "rangeStart": "2026-01-01",
    "rangeEnd": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "kind": "Checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "kind": "PoC",        "label": "Mar 26 PoC" },
          { "date": "2026-04-30", "kind": "Production", "label": "Apr Prod (TBD)" }
        ]
      }
      // M2, M3 …
    ]
  },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonthIndex": 3,
    "rows": {
      "shipped":     [["…"], ["…"], ["…"], ["…"]],
      "inProgress":  [[],    [],    ["…"], ["…"]],
      "carryover":   [[],    [],    [],    ["…"]],
      "blockers":    [[],    [],    [],    ["…"]]
    }
  }
}
``` `data.json` → `FileSystemWatcher` → `DashboardDataService.Reload()` → event → `Index.razor.StateHasChanged()` → Blazor Server diff → SignalR push → browser DOM update. No database, no HTTP API, no DTO layer. None needed. The model is tiny (< 10 KB). Recompute on every render; it's microseconds. There is no API. The Razor page consumes the service directly via DI. If a future need for an HTTP endpoint arises (e.g., an external tool POSTs updated JSON), add a single minimal-API endpoint `PUT /data` that writes `data.json` — deferred. ---

### Considerations & Risks

- **Authentication:** none. `app.UseAuthentication()`/`UseAuthorization()` are **not** registered. Binding is `http://localhost:5000` only; do not bind `0.0.0.0`.
- **HTTPS:** not required for localhost; disable the redirect middleware to avoid certificate prompts (`// app.UseHttpsRedirection();`).
- **CSRF / antiforgery:** no forms, no POSTs — leave default Blazor protections on, do nothing extra.
- **Content Security Policy:** optional; a permissive default is fine since nothing loads from the network.
- **Data protection:** no secrets, no PII. `data.json` may be committed to the repo alongside the app, or kept local — author's choice.
- **Hosting:** `dotnet run` from the solution folder. Optionally publish a self-contained single-file exe with `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true` for double-click launch.
- **Deployment:** copy the published folder + `data.json` to the author's machine. No CI/CD required; a GitHub Actions workflow that runs `dotnet build` + `dotnet test` on PR is sufficient hygiene.
- **Cost:** $0. No cloud services.
- **Observability:** console logging at Information level. No App Insights, no OpenTelemetry — overkill. --- | Risk | Likelihood | Impact | Mitigation | |---|---|---|---| | Screenshot fidelity drifts from reference HTML | Med | High (the whole point of the app) | bUnit snapshot tests assert rendered HTML against a golden file; CI fails on diff | | Timeline x-coordinate math is off-by-one across month boundaries | Med | Med | Unit tests for `TimelineLayout.ComputeX` covering month starts, ends, and leap-year Feb | | `data.json` malformed → blank/broken page right before a screenshot | Med | High | Validate on load, render a human-readable error banner with line/column from `JsonException`; keep last-known-good in memory | | Blazor Server SignalR reconnection overlay appears in screenshots | Low | High | Hide `#components-reconnect-modal` via CSS; the app is local so reconnection is never a real concern | | Segoe UI missing on non-Windows authoring machine | Low | Med | Font stack `'Segoe UI', 'Selawik', Arial, sans-serif`; document Windows-only | | Viewport is not exactly 1920×1080 when screenshotting | High | High | Optional Playwright script `tools/screenshot.ps1` that launches headless Chromium at exactly 1920×1080 and saves PNG | | Component libraries sneak in later and break the design | Med | High | Add an ADR in `/docs/adr/0001-no-component-libraries.md` | | "Current month" logic hard-coded to April | High | Med | Drive the highlighted column from `currentMonthIndex` in JSON; do not hard-code | | Timeline track count / row count grows beyond the design (e.g., 6 tracks) | Med | Med | Keep SVG height parametric: `height = tracks.length * 56 + 40` |
- No component library → more CSS to write, but total fidelity to the reference design.
- No charting library → hand-rolled SVG, but the timeline is simple and bespoke; libraries would be heavier to wrangle.
- Blazor Server (not WASM) → requires a running .NET process, but that is fine locally and gives us trivial file I/O + fast TTFB for screenshots. ---
- **Scope of months visible.** Reference shows 4 columns; timeline SVG shows 6 (Jan–Jun). Should the heatmap auto-window to `[currentMonth-2, currentMonth+1]`, or is it always a fixed quarter? → Recommend making it a 4-month sliding window driven by `currentMonthIndex` and `timeline.rangeStart`.
- **Who edits `data.json`?** Only the executive author, or engineers via PR? Affects whether we invest in a simple `/edit` form later.
- **Multiple projects.** Is "My Project" literally one project forever, or will there be N project JSON files? If N, a dropdown loader is a trivial add.
- **Export format.** PNG screenshot only, or also PDF? If PDF, add a Playwright `page.pdf()` path.
- **Branding.** Keep Microsoft Fluent palette (`#0078D4` etc.) or swap per project? → Expose `theme.primaryColor` in JSON.
- **Historical snapshots.** Keep month-over-month copies of `data.json` in `/history/YYYY-MM.json`? Useful for trend decks; cheap to add.
- **Offline font guarantee.** Is every viewer on Windows with Segoe UI, or ship Selawik (Segoe-compatible OFL font) in `wwwroot/fonts/`? ---

### Detailed Analysis

# Research.md — Executive Project Reporting Dashboard ("My Project")

## 1. Executive Summary

"My Project" is a **single-page executive status dashboard** rendered by a local **Blazor Server (.NET 8)** application that reads a hand-maintained `data.json` file and produces a 1920×1080 screenshot-ready view matching `OriginalDesignConcept.html`. The page shows (a) a header with title/subtitle/legend, (b) a horizontal multi-track SVG milestone timeline with a "NOW" marker, and (c) a 4-row × 4-column heatmap grid (Shipped / In Progress / Carryover / Blockers × months) with color-coded bullet items and a highlighted "current month" column.

**Primary recommendation:** build a single Blazor Server app (`AgentSquad.ReportingDashboard.sln`) with **one Razor page** (`/`), a strongly-typed `DashboardModel` deserialized from `data.json` via **System.Text.Json**, a hand-written **inline SVG** component for the timeline (no charting library — the design is bespoke and small), and a pure **CSS Grid + Flexbox** heatmap that mirrors the reference HTML class-for-class. Ship it with **Kestrel on localhost**, no auth, no database, no Docker. Keep the surface area tiny so the screenshot fidelity is the only thing that matters.

Do **not** introduce MudBlazor, Radzen, Blazorise, or any component kit — they will fight the pixel-perfect Segoe UI / custom palette design and add weight the project does not need. Do **not** introduce a charting library (ChartJs.Blazor, Blazor-ApexCharts, Plotly) — the timeline is a static, opinionated SVG that is easier and more faithful to render by hand.

---

## 2. Key Findings

- The reference design is **not a generic chart**; it's a hand-tuned SVG timeline + CSS Grid heatmap. Any component library will obstruct, not accelerate, fidelity.
- The dashboard is **read-only, single-user, screenshot-oriented**. That eliminates ~90% of the concerns a normal Blazor Server app has (auth, SignalR scale-out, CSRF, tenancy, persistence).
- **Blazor Server** is the right choice over Blazor WASM here: faster first paint (critical for screenshots), trivial local file I/O against `data.json`, no WASM payload, no API layer needed.
- `data.json` should be **hot-reloaded via `FileSystemWatcher`** so the author can tweak JSON and hit F5 to re-screenshot without restarting — biggest quality-of-life win available.
- The design uses **fixed 1920×1080** body dimensions. The app must render at that viewport regardless of the user's monitor; enforce with a `body{width:1920px;height:1080px}` rule copied verbatim from the reference and a browser zoom/window-size note in the README.
- The **timeline month positions are computed** (`x = (monthIndex + dayFraction) * 260`). This math belongs in a small pure C# helper (`TimelineLayout.cs`) with unit tests — it is the only non-trivial logic in the app.
- The heatmap's "current month" column (April in the sample) uses **different background shades per row** (`.apr` modifier classes). Model this as a `CurrentMonth` property on the dashboard root and apply the modifier class in Razor with a conditional.
- **Segoe UI** is a Windows system font and will render correctly on the screenshotting machine; no web font loading is required, which also guarantees screenshot consistency.
- The repo already contains `agentsquad_azurenerd_ReportingDashboard.db` — ignore it. The spec explicitly says JSON config, not a database.
- No compliance regime applies (no PII, no auth, no network egress). This is a **local authoring tool**, not a product.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server rendering layer)
| Concern | Choice | Version | Notes |
|---|---|---|---|
| UI framework | Blazor Server | .NET 8.0 (LTS, through Nov 2026) | Interactive Server render mode; no WASM |
| Component model | Plain `.razor` components | — | No MudBlazor/Radzen/Blazorise. They override CSS and hurt fidelity. |
| Styling | Hand-written CSS in `wwwroot/css/site.css` | — | Copy class names verbatim from `OriginalDesignConcept.html` |
| Icons / glyphs | None needed | — | Reference uses CSS shapes (rotated squares, circles) only |
| SVG rendering | Inline SVG inside a `TimelineSvg.razor` component | — | No SVG.NET, no Blazor-ApexCharts, no ChartJs.Blazor |
| Fonts | Segoe UI (system) | — | Windows-only target; acceptable per spec |

### Backend (.NET 8)
| Concern | Choice | Version | Notes |
|---|---|---|---|
| Runtime | .NET 8 SDK | 8.0.x (latest patch) | LTS |
| Web host | ASP.NET Core Kestrel | 8.0 | `dotnet run`, binds `http://localhost:5000` |
| JSON | `System.Text.Json` | built-in 8.0 | Source-gen serializer optional; not required |
| File watching | `Microsoft.Extensions.FileProviders.Physical` + `PhysicalFileProvider.Watch` | built-in | Triggers reload of `DashboardState` on `data.json` change |
| Config | `Microsoft.Extensions.Configuration.Json` | built-in | Only for Kestrel port, not the dashboard data |
| Logging | `Microsoft.Extensions.Logging.Console` | built-in | Debug-level is fine locally |

### Data
| Concern | Choice | Notes |
|---|---|---|
| Storage | Single file `data.json` in app root | Loaded at startup + on file change |
| Schema | POCOs with `JsonPropertyName` attributes | See §4 below |
| Validation | `System.ComponentModel.DataAnnotations` + a custom `DashboardValidator` | Fail fast with a friendly error page if JSON is malformed |

### Testing
| Concern | Choice | Version |
|---|---|---|
| Test framework | xUnit | 2.9.x |
| Assertions | FluentAssertions | 6.12.x |
| Component tests | bUnit | 1.33.x (targets .NET 8) |
| Snapshot (optional) | Verify.Xunit | 26.x — snapshot the generated SVG string to detect regressions |

### Tooling
| Concern | Choice | Version |
|---|---|---|
| Solution layout | `.sln` with `src/` and `tests/` | See §4 |
| Formatting | `dotnet format` + `.editorconfig` | built-in |
| Analyzers | `Microsoft.CodeAnalysis.NetAnalyzers` | 8.0.x |
| Screenshot automation (optional) | Playwright for .NET | 1.47.x — one script to render and save PNG at 1920×1080 for PowerPoint |

### Explicitly rejected
- **MudBlazor 7.x / Radzen Blazor 5.x / Blazorise 1.5.x** — opinionated component kits that re-skin everything; they fight the reference design.
- **ChartJs.Blazor / Blazor-ApexCharts / Plotly.Blazor** — the timeline is not a chart, it is a bespoke illustration.
- **HTMX / AlpineJS / any JS framework** — Blazor Server already provides interactivity; none needed anyway.
- **Entity Framework Core / SQLite / LiteDB** — there is no data to persist.
- **Docker / IIS / Azure App Service** — spec says local only.
- **ASP.NET Identity / Azure AD / cookie auth** — spec says no auth.

---

## 4. Architecture Recommendations

### Solution layout
```
ReportingDashboard.sln
├─ src/
│  └─ ReportingDashboard.Web/              (net8.0, Blazor Server)
│     ├─ Program.cs
│     ├─ App.razor
│     ├─ Pages/
│     │  └─ Index.razor                    (the one and only page, route "/")
│     ├─ Components/
│     │  ├─ DashboardHeader.razor          (title + subtitle + legend)
│     │  ├─ TimelineSvg.razor              (inline SVG, takes MilestoneTrack[])
│     │  ├─ TimelineLegend.razor
│     │  └─ Heatmap.razor                  (CSS Grid; 4 rows × 4 month cols)
│     ├─ Models/
│     │  ├─ DashboardModel.cs
│     │  ├─ MilestoneTrack.cs
│     │  ├─ Milestone.cs                   (Kind: PoC | Production | Checkpoint)
│     │  └─ HeatmapItem.cs                 (Category: Shipped|InProgress|Carryover|Blocker)
│     ├─ Services/
│     │  ├─ DashboardDataService.cs        (loads+watches data.json, exposes IObservable-ish event)
│     │  └─ TimelineLayout.cs              (pure: date -> x pixel within SVG)
│     ├─ wwwroot/
│     │  └─ css/site.css                   (verbatim port of OriginalDesignConcept styles)
│     ├─ data.json                         (the only input)
│     └─ appsettings.json
└─ tests/
   └─ ReportingDashboard.Tests/            (xUnit + bUnit + FluentAssertions)
      ├─ TimelineLayoutTests.cs
      ├─ DashboardDataServiceTests.cs
      └─ HeatmapRenderTests.cs             (bUnit snapshot)
```

### Render pipeline
1. `Program.cs` registers `DashboardDataService` as a **singleton**; constructor loads `data.json` once and starts a `FileSystemWatcher`.
2. `Index.razor` `@inject`s the service and subscribes to its `OnChanged` event; calls `StateHasChanged()` on change.
3. `Index.razor` composes: `<DashboardHeader/>`, `<TimelineSvg/>`, `<Heatmap/>` — each takes its slice of the model as a `[Parameter]`.
4. SVG coordinate math lives only in `TimelineLayout.ComputeX(DateOnly date, DateOnly rangeStart, DateOnly rangeEnd, int pixelWidth)` — unit-tested.
5. Heatmap iterates `model.Months` (4 entries) × `model.Categories` (4 fixed) and emits a `<div class="hm-cell @(IsCurrent ? "apr" : "")">` — the `apr` class name is kept from the reference for CSS-compat but semantically means "current month".

### `data.json` schema (opinionated)
```jsonc
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform • Privacy Automation Workstream • April 2026",
  "backlogUrl": "https://…",
  "currentDate": "2026-04-19",
  "timeline": {
    "rangeStart": "2026-01-01",
    "rangeEnd": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "kind": "Checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "kind": "PoC",        "label": "Mar 26 PoC" },
          { "date": "2026-04-30", "kind": "Production", "label": "Apr Prod (TBD)" }
        ]
      }
      // M2, M3 …
    ]
  },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonthIndex": 3,
    "rows": {
      "shipped":     [["…"], ["…"], ["…"], ["…"]],
      "inProgress":  [[],    [],    ["…"], ["…"]],
      "carryover":   [[],    [],    [],    ["…"]],
      "blockers":    [[],    [],    [],    ["…"]]
    }
  }
}
```

### Data flow
`data.json` → `FileSystemWatcher` → `DashboardDataService.Reload()` → event → `Index.razor.StateHasChanged()` → Blazor Server diff → SignalR push → browser DOM update. No database, no HTTP API, no DTO layer.

### Caching / performance
None needed. The model is tiny (< 10 KB). Recompute on every render; it's microseconds.

### API design
There is no API. The Razor page consumes the service directly via DI. If a future need for an HTTP endpoint arises (e.g., an external tool POSTs updated JSON), add a single minimal-API endpoint `PUT /data` that writes `data.json` — deferred.

---

## 5. Security & Infrastructure

- **Authentication:** none. `app.UseAuthentication()`/`UseAuthorization()` are **not** registered. Binding is `http://localhost:5000` only; do not bind `0.0.0.0`.
- **HTTPS:** not required for localhost; disable the redirect middleware to avoid certificate prompts (`// app.UseHttpsRedirection();`).
- **CSRF / antiforgery:** no forms, no POSTs — leave default Blazor protections on, do nothing extra.
- **Content Security Policy:** optional; a permissive default is fine since nothing loads from the network.
- **Data protection:** no secrets, no PII. `data.json` may be committed to the repo alongside the app, or kept local — author's choice.
- **Hosting:** `dotnet run` from the solution folder. Optionally publish a self-contained single-file exe with `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true` for double-click launch.
- **Deployment:** copy the published folder + `data.json` to the author's machine. No CI/CD required; a GitHub Actions workflow that runs `dotnet build` + `dotnet test` on PR is sufficient hygiene.
- **Cost:** $0. No cloud services.
- **Observability:** console logging at Information level. No App Insights, no OpenTelemetry — overkill.

---

## 6. Risks & Trade-offs

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Screenshot fidelity drifts from reference HTML | Med | High (the whole point of the app) | bUnit snapshot tests assert rendered HTML against a golden file; CI fails on diff |
| Timeline x-coordinate math is off-by-one across month boundaries | Med | Med | Unit tests for `TimelineLayout.ComputeX` covering month starts, ends, and leap-year Feb |
| `data.json` malformed → blank/broken page right before a screenshot | Med | High | Validate on load, render a human-readable error banner with line/column from `JsonException`; keep last-known-good in memory |
| Blazor Server SignalR reconnection overlay appears in screenshots | Low | High | Hide `#components-reconnect-modal` via CSS; the app is local so reconnection is never a real concern |
| Segoe UI missing on non-Windows authoring machine | Low | Med | Font stack `'Segoe UI', 'Selawik', Arial, sans-serif`; document Windows-only |
| Viewport is not exactly 1920×1080 when screenshotting | High | High | Optional Playwright script `tools/screenshot.ps1` that launches headless Chromium at exactly 1920×1080 and saves PNG |
| Component libraries sneak in later and break the design | Med | High | Add an ADR in `/docs/adr/0001-no-component-libraries.md` |
| "Current month" logic hard-coded to April | High | Med | Drive the highlighted column from `currentMonthIndex` in JSON; do not hard-code |
| Timeline track count / row count grows beyond the design (e.g., 6 tracks) | Med | Med | Keep SVG height parametric: `height = tracks.length * 56 + 40` |

### Trade-offs accepted
- No component library → more CSS to write, but total fidelity to the reference design.
- No charting library → hand-rolled SVG, but the timeline is simple and bespoke; libraries would be heavier to wrangle.
- Blazor Server (not WASM) → requires a running .NET process, but that is fine locally and gives us trivial file I/O + fast TTFB for screenshots.

---

## 7. Open Questions

1. **Scope of months visible.** Reference shows 4 columns; timeline SVG shows 6 (Jan–Jun). Should the heatmap auto-window to `[currentMonth-2, currentMonth+1]`, or is it always a fixed quarter? → Recommend making it a 4-month sliding window driven by `currentMonthIndex` and `timeline.rangeStart`.
2. **Who edits `data.json`?** Only the executive author, or engineers via PR? Affects whether we invest in a simple `/edit` form later.
3. **Multiple projects.** Is "My Project" literally one project forever, or will there be N project JSON files? If N, a dropdown loader is a trivial add.
4. **Export format.** PNG screenshot only, or also PDF? If PDF, add a Playwright `page.pdf()` path.
5. **Branding.** Keep Microsoft Fluent palette (`#0078D4` etc.) or swap per project? → Expose `theme.primaryColor` in JSON.
6. **Historical snapshots.** Keep month-over-month copies of `data.json` in `/history/YYYY-MM.json`? Useful for trend decks; cheap to add.
7. **Offline font guarantee.** Is every viewer on Windows with Segoe UI, or ship Selawik (Segoe-compatible OFL font) in `wwwroot/fonts/`?

---

## 8. Implementation Recommendations

### Phase 0 — Scaffolding (½ day)
- `dotnet new sln -n ReportingDashboard`
- `dotnet new blazorserver -n ReportingDashboard.Web -o src/ReportingDashboard.Web -f net8.0`
- `dotnet new xunit -n ReportingDashboard.Tests -o tests/ReportingDashboard.Tests`; add `bUnit`, `FluentAssertions`.
- Strip the template: delete `Counter.razor`, `FetchData.razor`, `NavMenu.razor`, `MainLayout.razor` sidebar. Keep a minimal shell.
- **Paste `OriginalDesignConcept.html`'s `<style>` block into `wwwroot/css/site.css` verbatim.** This is the most important step.

### Phase 1 — Static render (1 day)  ✅ Quick win
- Hard-code a `DashboardModel` in C# that mirrors the sample HTML.
- Build `DashboardHeader.razor`, `Heatmap.razor`, `TimelineSvg.razor` against it.
- Goal: pixel-identical render to `OriginalDesignConcept.html` at 1920×1080.
- **Demo:** screenshot side-by-side with the reference PNG; get exec sign-off.

### Phase 2 — JSON loader (½ day)
- Implement `DashboardDataService` with `System.Text.Json` + `FileSystemWatcher`.
- Create fictional `data.json` ("Project Aurora — Customer Portal Revamp" or similar) with realistic Shipped/InProgress/Carryover/Blocker items and 3 milestone tracks.
- Wire `Index.razor` to the service.

### Phase 3 — Robustness (½ day)
- JSON validation + friendly error page.
- Unit tests for `TimelineLayout`.
- bUnit snapshot test for `Heatmap` and `TimelineSvg`.
- Hide SignalR reconnect modal via CSS.

### Phase 4 — Screenshot tooling (optional, ½ day)
- `tools/screenshot.ps1` using Playwright: launches the app, navigates to `http://localhost:5000`, sets viewport 1920×1080, waits for `networkidle`, saves `docs/screenshots/{date}.png`.
- One-command PowerPoint refresh.

### Areas to prototype first
- **SVG timeline coordinate math** — spike it in a LINQPad script or a 30-line console app before embedding in a component.
- **FileSystemWatcher reload** — verify it doesn't double-fire on Windows (it usually does; debounce with a 250ms timer).

### Quick wins to demo early
1. Phase 1 pixel match — instantly shows execs what they'll get.
2. Hot reload of `data.json` — edit JSON, screen updates live; strong "wow" moment for the author.
3. Playwright one-click PNG export — removes the last manual step between JSON edit and PowerPoint slide.

### Do not build (yet)
- User accounts, editing UI, history browser, multi-project selector, PDF export, themes. All are trivial extensions once the core is solid; defer until a real need arises.

## Visual Design References

The following design reference files were found in the repository. These MUST be used as the canonical visual specification when building UI components.

### `OriginalDesignConcept.html`

**Type:** HTML Design Template

**Layout Structure:**
- **Header section** with title, subtitle, and legend
- **Timeline/Gantt section** with SVG milestone visualization
- **Heatmap grid** — status rows × month columns, color-coded by category
  - Shipped row (green tones)
  - In Progress row (blue tones)
  - Carryover row (yellow/amber tones)
  - Blockers row (red tones)

**Key CSS Patterns:**
- Uses CSS Grid layout
- Uses Flexbox layout
- Color palette: #FFFFFF, #111, #0078D4, #888, #FAFAFA, #F5F5F5, #999, #FFF0D0, #C07700, #333, #1B7A28, #E8F5E9, #F0FBF0, #D8F2DA, #34A853
- Font: Segoe UI
- Grid columns: `160px repeat(4,1fr)`
- Designed for 1920×1080 screenshot resolution

<details><summary>Full HTML Source</summary>

```html
<!DOCTYPE html><html lang="en"><head><meta charset="UTF-8">
<style>
*{margin:0;padding:0;box-sizing:border-box;}
body{width:1920px;height:1080px;overflow:hidden;background:#FFFFFF;
     font-family:'Segoe UI',Arial,sans-serif;color:#111;display:flex;flex-direction:column;}
a{color:#0078D4;text-decoration:none;}
.hdr{padding:12px 44px 10px;border-bottom:1px solid #E0E0E0;display:flex;
      align-items:center;justify-content:space-between;flex-shrink:0;}
.hdr h1{font-size:24px;font-weight:700;}
.sub{font-size:12px;color:#888;margin-top:2px;}
.tl-area{display:flex;align-items:stretch;padding:6px 44px 0;flex-shrink:0;height:196px;
          border-bottom:2px solid #E8E8E8;background:#FAFAFA;}
.tl-svg-box{flex:1;padding-left:12px;padding-top:6px;}
/* heatmap */
.hm-wrap{flex:1;min-height:0;display:flex;flex-direction:column;padding:10px 44px 10px;}
.hm-title{font-size:14px;font-weight:700;color:#888;letter-spacing:.5px;text-transform:uppercase;margin-bottom:8px;flex-shrink:0;}
.hm-grid{flex:1;min-height:0;display:grid;
          grid-template-columns:160px repeat(4,1fr);
          grid-template-rows:36px repeat(4,1fr);
          border:1px solid #E0E0E0;}
/* header cells */
.hm-corner{background:#F5F5F5;display:flex;align-items:center;justify-content:center;
            font-size:11px;font-weight:700;color:#999;text-transform:uppercase;
            border-right:1px solid #E0E0E0;border-bottom:2px solid #CCC;}
.hm-col-hdr{display:flex;align-items:center;justify-content:center;
             font-size:16px;font-weight:700;background:#F5F5F5;
             border-right:1px solid #E0E0E0;border-bottom:2px solid #CCC;}
.hm-col-hdr.apr-hdr{background:#FFF0D0;color:#C07700;}
/* row header */
.hm-row-hdr{display:flex;align-items:center;padding:0 12px;
             font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.7px;
             border-right:2px solid #CCC;border-bottom:1px solid #E0E0E0;}
/* data cells */
.hm-cell{padding:8px 12px;border-right:1px solid #E0E0E0;border-bottom:1px solid #E0E0E0;overflow:hidden;}
.hm-cell .it{font-size:12px;color:#333;padding:2px 0 2px 12px;position:relative;line-height:1.35;}
.hm-cell .it::before{content:'';position:absolute;left:0;top:7px;width:6px;height:6px;border-radius:50%;}
/* row colors */
.ship-hdr{color:#1B7A28;background:#E8F5E9;border-right:2px solid #CCC;}
.ship-cell{background:#F0FBF0;} .ship-cell.apr{background:#D8F2DA;}
.ship-cell .it::before{background:#34A853;}
.prog-hdr{color:#1565C0;background:#E3F2FD;border-right:2px solid #CCC;}
.prog-cell{background:#EEF4FE;} .prog-cell.apr{background:#DAE8FB;}
.prog-cell .it::before{background:#0078D4;}
.carry-hdr{color:#B45309;background:#FFF8E1;border-right:2px solid #CCC;}
.carry-cell{background:#FFFDE7;} .carry-cell.apr{background:#FFF0B0;}
.carry-cell .it::before{background:#F4B400;}
.block-hdr{color:#991B1B;background:#FEF2F2;border-right:2px solid #CCC;}
.block-cell{background:#FFF5F5;} .block-cell.apr{background:#FFE4E4;}
.block-cell .it::before{background:#EA4335;}
</style></head><body>
<div class="hdr">
  <div>
    <h1>Privacy Automation Release Roadmap <a href="#">⧉ ADO Backlog</a></h1>
    <div class="sub">Trusted Platform · Privacy Automation Workstream · April 2026</div>
  </div>
  
<div style="display:flex;gap:22px;align-items:center;">
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:12px;height:12px;background:#F4B400;transform:rotate(45deg);display:inline-block;flex-shrink:0;"></span>PoC Milestone
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:12px;height:12px;background:#34A853;transform:rotate(45deg);display:inline-block;flex-shrink:0;"></span>Production Release
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:8px;height:8px;border-radius:50%;background:#999;display:inline-block;flex-shrink:0;"></span>Checkpoint
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:2px;height:14px;background:#EA4335;display:inline-block;flex-shrink:0;"></span>Now (Apr 2026)
  </span>
</div>
</div>
<div class="tl-area">
  
<div style="width:230px;flex-shrink:0;display:flex;flex-direction:column;
            justify-content:space-around;padding:16px 12px 16px 0;
            border-right:1px solid #E0E0E0;">
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#0078D4;">
    M1<br/><span style="font-weight:400;color:#444;">Chatbot &amp; MS Role</span></div>
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#00897B;">
    M2<br/><span style="font-weight:400;color:#444;">PDS &amp; Data Inventory</span></div>
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#546E7A;">
    M3<br/><span style="font-weight:400;color:#444;">Auto Review DFD</span></div>
</div>
  <div class="tl-svg-box"><svg xmlns="http://www.w3.org/2000/svg" width="1560" height="185" style="overflow:visible;display:block">
<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>
<line x1="0" y1="0" x2="0" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="5" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Jan</text>
<line x1="260" y1="0" x2="260" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="265" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Feb</text>
<line x1="520" y1="0" x2="520" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="525" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Mar</text>
<line x1="780" y1="0" x2="780" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="785" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Apr</text>
<line x1="1040" y1="0" x2="1040" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="1045" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">May</text>
<line x1="1300" y1="0" x2="1300" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="1305" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Jun</text>
<line x1="823" y1="0" x2="823" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
<text x="827" y="14" fill="#EA4335" font-size="10" font-weight="700" font-family="Segoe UI,Arial">NOW</text>
<line x1="0" y1="42" x2="1560" y2="42" stroke="#0078D4" stroke-width="3"/>
<circle cx="104" cy="42" r="7" fill="white" stroke="#0078D4" stroke-width="2.5"/>
<text x="104" y="26" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Jan 12</text>
<polygon points="745,31 756,42 745,53 734,42" fill="#F4B400" filter="url(#sh)"/><text x="745" y="66" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Mar 26 PoC</text>
<polygon points="1040,31 1051,42 1040,53 1029,42" fill="#34A853" filter="url(#sh)"/><text x="1040" y="18" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Apr Prod (TBD)</text>
<line x1="0" y1="98" x2="1560" y2="98" stroke="#00897B" stroke-width="3"/>
<circle cx="0" cy="98" r="7" fill="white" stroke="#00897B" stroke-width="2.5"/>
<text x="10" y="82" fill="#666" font-size="10" font-family="Segoe UI,Arial">Dec 19</text>
<circle cx="355" cy="98" r="5" fill="white" stroke="#888" stroke-width="2.5"/>
<text x="355" y="82" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Feb 11</text>
<circle cx="546" cy="98" r="4" fill="#999"/>
<circle cx="607" cy="98" r="4" fill="#999"/>
<circle cx="650" cy="98" r="4" fill="#999"/>
<circle cx="667" cy="98" r="4" fill="#999"/>
<polygon points="693,87 704,98 693,109 682,98" fill="#F4B400" filter="url(#sh)"/><text x="693" y="74" text-anchor="middle" fill="#666" font-size="10" font-family="Seg
<!-- truncated -->
```
</details>


## Design Visual Previews

The following screenshots were rendered from the HTML design reference files. Engineers MUST match these visuals exactly.

### OriginalDesignConcept.html

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/53e4c4a3aadc1da0e6b976d5402f1d2f24d57291/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
