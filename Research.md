# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-20 04:10 UTC_

### Summary

"My Project" is a **single-page, screenshot-optimized executive reporting dashboard** rendering a fixed 1920×1080 view that mirrors the provided `OriginalDesignConcept.html`: a header with legend, a top Gantt/milestone timeline (SVG), and a 4-row × 4-column heatmap (Shipped / In Progress / Carryover / Blockers across months) with the current month highlighted. Given the mandatory stack (**C# .NET 8 + Blazor Server, local-only, `.sln` structure, no cloud**), the primary recommendation is a **deliberately minimal Blazor Server app** that loads a single `data.json` file at startup (and on file-change), binds it to strongly-typed models, and renders the layout using **plain CSS Grid + inline SVG inside Razor components** — no charting library, no database, no auth, no JS interop. The existing HTML/CSS from `OriginalDesignConcept.html` should be ported almost verbatim into a Razor component; this preserves pixel fidelity for PowerPoint screenshots and avoids the styling drift that comes with component libraries (MudBlazor, Radzen, etc.). The dashboard is not an "app" — it is a **parameterized design artifact**. Every architectural decision below is biased toward (a) screenshot fidelity, (b) zero-friction local run (`dotnet run`), and (c) editing `data.json` as the only "admin UI." ---

### Key Findings

- The design is a **static infographic**, not an interactive BI tool. Interactivity (drill-down, filtering, tooltips) is an anti-goal — it adds visual noise that harms screenshot quality. Render once, render exactly.
- **Blazor Server is overkill for the rendering need but correct given the mandated stack.** Server-side rendering (SSR) via Blazor's static render mode is preferable to interactive SignalR circuits for a screenshot page — it produces clean HTML on first byte, no loading flicker, no WebSocket overhead.
- **No charting library is needed.** The timeline is hand-authored SVG with ~6 month gridlines, ~3 swim lanes, diamonds/circles for milestones, and a dashed "NOW" line. Libraries like Plotly.Blazor, ChartJs.Blazor, or ApexCharts add weight and fight the custom aesthetic. Hand-rolled SVG in a `@code` block gives exact control.
- **CSS Grid (`grid-template-columns:160px repeat(4,1fr)`) is load-bearing** for the heatmap — this must be preserved exactly. Flexbox handles the header legend and timeline row-label column.
- **`data.json` is the entire "data layer."** No EF Core, no SQLite, no repositories. A single `DashboardData` POCO deserialized via `System.Text.Json` with `PropertyNameCaseInsensitive = true` is the right abstraction.
- **Fixed 1920×1080 viewport is a feature, not a bug.** The body should retain `width:1920px;height:1080px;overflow:hidden;` so screenshots are reproducible. Responsive design is explicitly out of scope.
- **Segoe UI** is the specified font — safe on Windows (the screenshot environment) but fragile on Linux/macOS. Ship with `font-family:'Segoe UI',-apple-system,'Helvetica Neue',Arial,sans-serif;` and accept Windows as the canonical render target.
- **Hot-reload of `data.json`** (via `FileSystemWatcher` + `IMemoryCache` invalidation) is a high-value quick win: edit JSON, refresh browser, screenshot. No rebuild.
- **The "NOW" marker position** is a function of today's date vs. the timeline start/end. This must be computed server-side in C#, not hardcoded as `x1="823"` like the reference HTML.
- Heatmap cells with many items risk overflow at 1080px height. The design needs **explicit overflow behavior** (truncate with "+N more" or shrink font) — an open question for the exec audience. ---
- `dotnet new sln -n ReportingDashboard`
- `dotnet new blazor -n ReportingDashboard.Web -o src/ReportingDashboard.Web --interactivity None --empty`
- `dotnet sln add src/ReportingDashboard.Web`
- Add `tests/ReportingDashboard.Web.Tests` (xUnit + bUnit).
- Copy `OriginalDesignConcept.html` CSS verbatim into `Dashboard.razor.css`.
- Port the reference HTML into `Dashboard.razor` with hardcoded values matching the design exactly.
- **Verify pixel parity** against `OriginalDesignConcept.png` at 1920×1080.
- This is the "screenshot works" milestone.
- Define `DashboardData`, `Milestone`, `HeatmapRow` POCOs.
- `DashboardDataService` reads `wwwroot/data.json` on startup.
- Replace hardcoded Razor values with `@Model.*` bindings.
- Compute NOW line, month gridlines, current-month column in C#.
- `FileSystemWatcher` hot-reload.
- JSON schema validation with user-friendly error banner.
- README with "how to edit data.json" and "how to screenshot."
- xUnit: coordinate math, NOW-line positioning, overflow truncation.
- bUnit: `Dashboard.razor` renders without exceptions given a sample `data.json`.
- (Optional) Verify.Xunit snapshot of the SVG string for the sample data. ✅ Single page, fixed 1920×1080, loads `data.json`, renders header + timeline + 4×4 heatmap, hot-reload on file change, Windows-only screenshot canonical. **Nothing else.**
- **Ship Phase 1 (static port) first** — gets to "screenshot matches design" in one day; de-risks the visual spec before touching data.
- **Hot-reload `data.json`** — lets the PM iterate content in real time without a dev in the loop.
- **Self-contained single-file publish** — hand execs a 70MB `.exe` + `data.json`; they double-click and screenshot.
- **Sample `data.json` committed to repo** — fictional project fake data (per user request) doubles as a test fixture and a "how to use" example.
- **The SVG timeline math.** Build a tiny standalone Razor page with 3–4 hardcoded date ranges and verify the NOW line, milestone diamonds, and month gridlines land correctly before wiring up `data.json`. This is the only technically non-trivial piece; everything else is CSS.
- **Heatmap overflow behavior.** Prototype with a deliberately overstuffed JSON to settle the open question before design sign-off.
- ❌ Do not introduce MudBlazor/Radzen — they will fight the custom CSS.
- ❌ Do not add a database or EF Core — the user asked for "super simple."
- ❌ Do not make the layout responsive — fixed 1920×1080 is the spec.
- ❌ Do not add interactivity (tooltips, filters, drill-downs) — ruins screenshot cleanliness.
- ❌ Do not use `InteractiveServer` render mode — SSR is sufficient and faster.
- ❌ Do not add auth — explicitly out of scope.

### Recommended Tools & Technologies

- | Concern | Choice | Version | Rationale | |---|---|---|---| | UI framework | **Blazor Server** with **Static SSR** render mode | .NET 8.0.x (LTS, patched to latest 8.0.11+) | Mandated. Static SSR avoids SignalR circuit for a non-interactive page. | | Component library | **None** — raw Razor + CSS | n/a | MudBlazor/Radzen/FluentUI Blazor would override the custom look. Hand-authored CSS matches design 1:1. | | Icons | Inline SVG only | n/a | Avoid icon-font dependencies for 1–2 glyphs. | | Charting | **Hand-rolled inline SVG** in a `TimelineSvg.razor` component | n/a | Full control; no library fights the bespoke look. Alternatives considered and rejected: ChartJs.Blazor (abandoned, last release 2021), Plotly.Blazor (heavy, wrong aesthetic), ApexCharts.Blazor (good but unnecessary). | | CSS strategy | **Scoped CSS** (`Dashboard.razor.css`) + a single global `app.css` for resets | n/a | Co-locates styles with component; avoids leakage. | | Concern | Choice | Version | Rationale | |---|---|---|---| | Runtime | .NET 8 LTS | 8.0.11+ | Mandated. LTS support through Nov 2026. | | Web host | `WebApplication` minimal hosting (`Program.cs`) | built-in | Single-file startup; no `Startup.cs`. | | JSON | `System.Text.Json` | built-in | Faster than Newtonsoft; sufficient for this schema. Enable `PropertyNameCaseInsensitive` and `ReadCommentHandling=Skip`. | | Config reload | `FileSystemWatcher` wrapped in a singleton `DashboardDataService` | built-in | Debounced 250ms; reloads `data.json` without restart. | | Date/time | `DateOnly` (.NET 6+) for milestone dates | built-in | Cleaner serialization than `DateTime`; maps to `"2026-03-26"`. | | Logging | `Microsoft.Extensions.Logging.Console` | built-in | No Serilog needed at this scope. | | Concern | Choice | Rationale | |---|---|---| | Storage | **Single `data.json` file** in `wwwroot/` (or app root) | No DB. Editable by non-developers. Version-controllable. | | Schema | Strongly-typed C# POCOs with `required` members | Compile-time guarantees; crashes fast on bad JSON. | | Validation | **FluentValidation 11.10.0** (optional) OR DataAnnotations | Surface "bad JSON" errors in a friendly banner at the top of the page rather than a YSOD. | | Concern | Choice | Version | |---|---|---| | Unit tests | **xUnit** | 2.9.2 | | Assertions | **FluentAssertions** | 6.12.x (note: v8+ is commercially licensed — **pin to 6.12.x**) | | Blazor component tests | **bUnit** | 1.33.3 | | Snapshot testing (optional) | **Verify.Xunit** | 26.x — useful for asserting the SVG timeline output stays pixel-stable | | Coverage | **coverlet.collector** | 6.0.x | | Concern | Choice | |---|---| | Formatter | `dotnet format` (built-in) | | Analyzers | `Microsoft.CodeAnalysis.NetAnalyzers` (built-in in SDK) | | CI (if any) | GitHub Actions `actions/setup-dotnet@v4` with `dotnet-version: 8.0.x` — build + test only | | Local run | `dotnet watch run` from `src/ReportingDashboard.Web/` |
- **No** EF Core, SQLite, Dapper — `data.json` is the database.
- **No** Identity, cookies, JWT — no auth.
- **No** Docker, Kubernetes, Azure — "local only" per mandate.
- **No** Redis/caching layer — `IMemoryCache` is fine, and the dataset is <50KB.
- **No** SignalR / interactivity — static SSR only. ---
```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard.Web/          (Blazor Server, net8.0)
│       ├── Program.cs
│       ├── Components/
│       │   ├── App.razor
│       │   ├── Routes.razor
│       │   ├── Layout/MainLayout.razor
│       │   └── Pages/
│       │       ├── Dashboard.razor          (the one page)
│       │       ├── Dashboard.razor.css
│       │       ├── Partials/
│       │       │   ├── DashboardHeader.razor
│       │       │   ├── TimelineSvg.razor    (pure SVG render)
│       │       │   └── Heatmap.razor        (CSS grid render)
│       ├── Models/
│       │   ├── DashboardData.cs
│       │   ├── Milestone.cs
│       │   ├── TimelineLane.cs
│       │   └── HeatmapItem.cs
│       ├── Services/
│       │   └── DashboardDataService.cs   (singleton, hot-reloads data.json)
│       ├── wwwroot/
│       │   ├── data.json                 (the single source of truth)
│       │   └── app.css
│       └── ReportingDashboard.Web.csproj
└── tests/
    └── ReportingDashboard.Web.Tests/     (xUnit + bUnit)
```
- **Static Server Render** (`@rendermode` omitted / `InteractiveServer` NOT used). First GET returns fully-rendered HTML — ideal for browser screenshots and PDF export.
- `Dashboard.razor` is the only route (`@page "/"`). No navigation, no layout chrome beyond `<body>`.
- Three child components: `<DashboardHeader Data="..."/>`, `<TimelineSvg Data="..."/>`, `<Heatmap Data="..."/>` — each takes a strongly-typed slice of `DashboardData`.
```
data.json  ──▶  FileSystemWatcher  ──▶  DashboardDataService (singleton)
                                              │
                                              ▼
                         Dashboard.razor.OnInitializedAsync()
                                              │
                                              ▼
                   ┌──────────────────────────┼──────────────────────────┐
                   ▼                          ▼                          ▼
          DashboardHeader              TimelineSvg                   Heatmap
          (title, sub, legend)         (SVG computed from            (CSS grid,
                                        DateOnly milestones)          4 rows × 4 cols)
```
- **"NOW" x-coordinate** = `(DateTime.Today - timeline.StartDate).TotalDays / (timeline.EndDate - timeline.StartDate).TotalDays * svgWidth`. Never hardcode.
- **Month column boundaries** computed from timeline range, not assumed to be 6.
- **Current-month heatmap column** flagged via `item.Month == DateTime.Today.Month` → adds `.apr` (rename to `.current`) CSS class.
```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · April 2026",
    "backlogUrl": "https://..."
  },
  "timeline": {
    "start": "2026-01-01",
    "end":   "2026-06-30",
    "lanes": [
      { "id":"M1", "label":"Chatbot & MS Role", "color":"#0078D4",
        "milestones":[
          {"date":"2026-01-12","type":"checkpoint","label":"Kickoff"},
          {"date":"2026-03-26","type":"poc","label":"PoC"},
          {"date":"2026-04-30","type":"prod","label":"Prod (TBD)"}
        ]}
    ]
  },
  "heatmap": {
    "months": ["Jan","Feb","Mar","Apr"],
    "currentMonthIndex": 3,
    "rows": [
      {"category":"shipped",    "cells":[["Item A"],["Item B"],[],["Item C"]]},
      {"category":"inProgress", "cells":[[],[],["X"],["Y","Z"]]},
      {"category":"carryover",  "cells":[[],[],[],["Legacy API"]]},
      {"category":"blockers",   "cells":[[],[],[],["Vendor SLA"]]}
    ]
  }
}
```
- `IMemoryCache` with absolute expiration on file-change token — `services.AddMemoryCache()`.
- Dataset is tiny (<50KB) and render is <10ms — no need for output caching, response compression, or CDN.
- Page size budget: **<150KB total** (no framework JS bundles since interactivity is off; Blazor static SSR ships essentially no client JS for this page).
- **No API.** This is a server-rendered page. If an API is ever needed for external consumers, expose a single `GET /data.json` minimal-API endpoint that returns the raw file. ---

### Considerations & Risks

- **Authentication:** None. Per explicit user requirement. Bind Kestrel to `http://localhost:5080` only (`appsettings.json` → `Kestrel:Endpoints:Http:Url`) so it's not network-reachable by default.
- **Authorization:** None.
- **HTTPS:** Not required for localhost. Skip `UseHttpsRedirection()` to avoid dev-cert friction.
- **Anti-forgery / CORS / CSP:** Not applicable (no forms, no cross-origin, no JS). Blazor's default antiforgery middleware can be left enabled (it's free) but has no effect.
- **Data protection:** `data.json` is plaintext, non-sensitive executive summary text. No PII expected; if PII ever added, the design assumption breaks — flag as an open question.
- **Hosting:** **`dotnet run` on the user's workstation.** That is the entire deployment story. Optionally package as a **self-contained single-file publish**:
  ```
  dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
  ``` yielding one `ReportingDashboard.Web.exe` (~70MB) that can be double-clicked.
- **Observability:** Console logging only. No App Insights, no OpenTelemetry — out of scope for a local screenshot tool.
- **Infrastructure cost:** **$0.** Local only. --- | Risk | Severity | Mitigation | |---|---|---| | **Heatmap cell overflow** when month has many items at 1080px | High (visual) | Cap items-per-cell at N (e.g., 4); render "+K more" suffix. Make N configurable in JSON. | | **Fixed 1920×1080 layout** breaks on laptop screens during demos | Medium | Add a `?zoom=0.75` query param or a browser-zoom instruction. Document "screenshot in a 1920-wide window" in README. | | **Segoe UI absent on non-Windows render** → font metrics shift, SVG text misaligns | Medium | Pin fallback stack; declare Windows as canonical screenshot OS. | | **Hand-rolled SVG math errors** (NOW line, month gridlines) | Medium | Unit-test coordinate calculations via xUnit; use Verify snapshot test on the rendered SVG string. | | **`data.json` malformed** → YSOD or blank page | Medium | Validate on load; render an in-page red banner with the parse error instead of crashing. | | **Blazor Server circuit accidentally enabled** → WebSocket overhead, loading spinner appears in screenshots | Low-Medium | Explicitly do NOT call `AddInteractiveServerComponents()`. Use static SSR only. | | **FluentAssertions v8 license** inadvertently adopted | Low | Pin `<PackageReference ... Version="6.12.*" />` in `.csproj`. | | **Scope creep toward "real dashboard"** (filtering, multi-project) | High (schedule) | Explicitly reject; this is a PowerPoint screenshot tool. | | **.NET 8 EOL Nov 2026** | Low (for now) | Plan a .NET 10 LTS bump in ~12 months; trivial for this codebase. |
- **No responsive design** — accepted because screenshots are the delivery medium.
- **No component library** — accepted because design fidelity > development speed; the page is <500 lines of Razor.
- **No database** — accepted because JSON is easier to edit and diff for executives/PMs. ---
- **Number of months visible** — design shows 4 columns in heatmap but 6 in timeline. Should both be configurable independently? (Recommend: yes, but default to current design.)
- **Heatmap cell overflow policy** — truncate, shrink, scroll, or overflow? Screenshots demand a deterministic choice.
- **Multiple projects / multi-page support** — is this truly one project per app instance, or do we need `/projects/{slug}`? (Assumed: one project, one `data.json`.)
- **Who edits `data.json`?** PM manually? Export from ADO? If ADO export is desired later, we'd need a small CLI companion — out of scope for v1.
- **Export format** — PNG via browser screenshot is assumed. Do we want a built-in "Print to PDF" button or headless-chrome export? (Recommend: no; defer to OS screenshot tool.)
- **Color accessibility** — the red/yellow/green palette is not colorblind-safe. Is the exec audience okay with this, or do we need icons/patterns in addition to color?
- **Time zone** — "NOW" line uses server local time. Acceptable for a single-user local tool; flag if ever shared.
- **Branding** — is "Segoe UI + Microsoft blue #0078D4" the intended brand, or should colors be themeable via `data.json`? (Recommend: theme block in JSON for easy re-skinning.) ---

### Detailed Analysis

# Research.md — Executive Project Reporting Dashboard ("My Project")

## 1. Executive Summary

"My Project" is a **single-page, screenshot-optimized executive reporting dashboard** rendering a fixed 1920×1080 view that mirrors the provided `OriginalDesignConcept.html`: a header with legend, a top Gantt/milestone timeline (SVG), and a 4-row × 4-column heatmap (Shipped / In Progress / Carryover / Blockers across months) with the current month highlighted.

Given the mandatory stack (**C# .NET 8 + Blazor Server, local-only, `.sln` structure, no cloud**), the primary recommendation is a **deliberately minimal Blazor Server app** that loads a single `data.json` file at startup (and on file-change), binds it to strongly-typed models, and renders the layout using **plain CSS Grid + inline SVG inside Razor components** — no charting library, no database, no auth, no JS interop. The existing HTML/CSS from `OriginalDesignConcept.html` should be ported almost verbatim into a Razor component; this preserves pixel fidelity for PowerPoint screenshots and avoids the styling drift that comes with component libraries (MudBlazor, Radzen, etc.).

The dashboard is not an "app" — it is a **parameterized design artifact**. Every architectural decision below is biased toward (a) screenshot fidelity, (b) zero-friction local run (`dotnet run`), and (c) editing `data.json` as the only "admin UI."

---

## 2. Key Findings

- The design is a **static infographic**, not an interactive BI tool. Interactivity (drill-down, filtering, tooltips) is an anti-goal — it adds visual noise that harms screenshot quality. Render once, render exactly.
- **Blazor Server is overkill for the rendering need but correct given the mandated stack.** Server-side rendering (SSR) via Blazor's static render mode is preferable to interactive SignalR circuits for a screenshot page — it produces clean HTML on first byte, no loading flicker, no WebSocket overhead.
- **No charting library is needed.** The timeline is hand-authored SVG with ~6 month gridlines, ~3 swim lanes, diamonds/circles for milestones, and a dashed "NOW" line. Libraries like Plotly.Blazor, ChartJs.Blazor, or ApexCharts add weight and fight the custom aesthetic. Hand-rolled SVG in a `@code` block gives exact control.
- **CSS Grid (`grid-template-columns:160px repeat(4,1fr)`) is load-bearing** for the heatmap — this must be preserved exactly. Flexbox handles the header legend and timeline row-label column.
- **`data.json` is the entire "data layer."** No EF Core, no SQLite, no repositories. A single `DashboardData` POCO deserialized via `System.Text.Json` with `PropertyNameCaseInsensitive = true` is the right abstraction.
- **Fixed 1920×1080 viewport is a feature, not a bug.** The body should retain `width:1920px;height:1080px;overflow:hidden;` so screenshots are reproducible. Responsive design is explicitly out of scope.
- **Segoe UI** is the specified font — safe on Windows (the screenshot environment) but fragile on Linux/macOS. Ship with `font-family:'Segoe UI',-apple-system,'Helvetica Neue',Arial,sans-serif;` and accept Windows as the canonical render target.
- **Hot-reload of `data.json`** (via `FileSystemWatcher` + `IMemoryCache` invalidation) is a high-value quick win: edit JSON, refresh browser, screenshot. No rebuild.
- **The "NOW" marker position** is a function of today's date vs. the timeline start/end. This must be computed server-side in C#, not hardcoded as `x1="823"` like the reference HTML.
- Heatmap cells with many items risk overflow at 1080px height. The design needs **explicit overflow behavior** (truncate with "+N more" or shrink font) — an open question for the exec audience.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server, .NET 8)
| Concern | Choice | Version | Rationale |
|---|---|---|---|
| UI framework | **Blazor Server** with **Static SSR** render mode | .NET 8.0.x (LTS, patched to latest 8.0.11+) | Mandated. Static SSR avoids SignalR circuit for a non-interactive page. |
| Component library | **None** — raw Razor + CSS | n/a | MudBlazor/Radzen/FluentUI Blazor would override the custom look. Hand-authored CSS matches design 1:1. |
| Icons | Inline SVG only | n/a | Avoid icon-font dependencies for 1–2 glyphs. |
| Charting | **Hand-rolled inline SVG** in a `TimelineSvg.razor` component | n/a | Full control; no library fights the bespoke look. Alternatives considered and rejected: ChartJs.Blazor (abandoned, last release 2021), Plotly.Blazor (heavy, wrong aesthetic), ApexCharts.Blazor (good but unnecessary). |
| CSS strategy | **Scoped CSS** (`Dashboard.razor.css`) + a single global `app.css` for resets | n/a | Co-locates styles with component; avoids leakage. |

### Backend (.NET 8)
| Concern | Choice | Version | Rationale |
|---|---|---|---|
| Runtime | .NET 8 LTS | 8.0.11+ | Mandated. LTS support through Nov 2026. |
| Web host | `WebApplication` minimal hosting (`Program.cs`) | built-in | Single-file startup; no `Startup.cs`. |
| JSON | `System.Text.Json` | built-in | Faster than Newtonsoft; sufficient for this schema. Enable `PropertyNameCaseInsensitive` and `ReadCommentHandling=Skip`. |
| Config reload | `FileSystemWatcher` wrapped in a singleton `DashboardDataService` | built-in | Debounced 250ms; reloads `data.json` without restart. |
| Date/time | `DateOnly` (.NET 6+) for milestone dates | built-in | Cleaner serialization than `DateTime`; maps to `"2026-03-26"`. |
| Logging | `Microsoft.Extensions.Logging.Console` | built-in | No Serilog needed at this scope. |

### Data Layer
| Concern | Choice | Rationale |
|---|---|---|
| Storage | **Single `data.json` file** in `wwwroot/` (or app root) | No DB. Editable by non-developers. Version-controllable. |
| Schema | Strongly-typed C# POCOs with `required` members | Compile-time guarantees; crashes fast on bad JSON. |
| Validation | **FluentValidation 11.10.0** (optional) OR DataAnnotations | Surface "bad JSON" errors in a friendly banner at the top of the page rather than a YSOD. |

### Testing
| Concern | Choice | Version |
|---|---|---|
| Unit tests | **xUnit** | 2.9.2 |
| Assertions | **FluentAssertions** | 6.12.x (note: v8+ is commercially licensed — **pin to 6.12.x**) |
| Blazor component tests | **bUnit** | 1.33.3 |
| Snapshot testing (optional) | **Verify.Xunit** | 26.x — useful for asserting the SVG timeline output stays pixel-stable |
| Coverage | **coverlet.collector** | 6.0.x |

### Tooling / CI
| Concern | Choice |
|---|---|
| Formatter | `dotnet format` (built-in) |
| Analyzers | `Microsoft.CodeAnalysis.NetAnalyzers` (built-in in SDK) |
| CI (if any) | GitHub Actions `actions/setup-dotnet@v4` with `dotnet-version: 8.0.x` — build + test only |
| Local run | `dotnet watch run` from `src/ReportingDashboard.Web/` |

### Deliberately Excluded
- **No** EF Core, SQLite, Dapper — `data.json` is the database.
- **No** Identity, cookies, JWT — no auth.
- **No** Docker, Kubernetes, Azure — "local only" per mandate.
- **No** Redis/caching layer — `IMemoryCache` is fine, and the dataset is <50KB.
- **No** SignalR / interactivity — static SSR only.

---

## 4. Architecture Recommendations

### Solution Structure (`.sln`)
```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard.Web/          (Blazor Server, net8.0)
│       ├── Program.cs
│       ├── Components/
│       │   ├── App.razor
│       │   ├── Routes.razor
│       │   ├── Layout/MainLayout.razor
│       │   └── Pages/
│       │       ├── Dashboard.razor          (the one page)
│       │       ├── Dashboard.razor.css
│       │       ├── Partials/
│       │       │   ├── DashboardHeader.razor
│       │       │   ├── TimelineSvg.razor    (pure SVG render)
│       │       │   └── Heatmap.razor        (CSS grid render)
│       ├── Models/
│       │   ├── DashboardData.cs
│       │   ├── Milestone.cs
│       │   ├── TimelineLane.cs
│       │   └── HeatmapItem.cs
│       ├── Services/
│       │   └── DashboardDataService.cs   (singleton, hot-reloads data.json)
│       ├── wwwroot/
│       │   ├── data.json                 (the single source of truth)
│       │   └── app.css
│       └── ReportingDashboard.Web.csproj
└── tests/
    └── ReportingDashboard.Web.Tests/     (xUnit + bUnit)
```

### Rendering Model
- **Static Server Render** (`@rendermode` omitted / `InteractiveServer` NOT used). First GET returns fully-rendered HTML — ideal for browser screenshots and PDF export.
- `Dashboard.razor` is the only route (`@page "/"`). No navigation, no layout chrome beyond `<body>`.
- Three child components: `<DashboardHeader Data="..."/>`, `<TimelineSvg Data="..."/>`, `<Heatmap Data="..."/>` — each takes a strongly-typed slice of `DashboardData`.

### Data Flow
```
data.json  ──▶  FileSystemWatcher  ──▶  DashboardDataService (singleton)
                                              │
                                              ▼
                         Dashboard.razor.OnInitializedAsync()
                                              │
                                              ▼
                   ┌──────────────────────────┼──────────────────────────┐
                   ▼                          ▼                          ▼
          DashboardHeader              TimelineSvg                   Heatmap
          (title, sub, legend)         (SVG computed from            (CSS grid,
                                        DateOnly milestones)          4 rows × 4 cols)
```

### Key Computations (server-side C#)
- **"NOW" x-coordinate** = `(DateTime.Today - timeline.StartDate).TotalDays / (timeline.EndDate - timeline.StartDate).TotalDays * svgWidth`. Never hardcode.
- **Month column boundaries** computed from timeline range, not assumed to be 6.
- **Current-month heatmap column** flagged via `item.Month == DateTime.Today.Month` → adds `.apr` (rename to `.current`) CSS class.

### Proposed `data.json` Schema (sketch)
```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · April 2026",
    "backlogUrl": "https://..."
  },
  "timeline": {
    "start": "2026-01-01",
    "end":   "2026-06-30",
    "lanes": [
      { "id":"M1", "label":"Chatbot & MS Role", "color":"#0078D4",
        "milestones":[
          {"date":"2026-01-12","type":"checkpoint","label":"Kickoff"},
          {"date":"2026-03-26","type":"poc","label":"PoC"},
          {"date":"2026-04-30","type":"prod","label":"Prod (TBD)"}
        ]}
    ]
  },
  "heatmap": {
    "months": ["Jan","Feb","Mar","Apr"],
    "currentMonthIndex": 3,
    "rows": [
      {"category":"shipped",    "cells":[["Item A"],["Item B"],[],["Item C"]]},
      {"category":"inProgress", "cells":[[],[],["X"],["Y","Z"]]},
      {"category":"carryover",  "cells":[[],[],[],["Legacy API"]]},
      {"category":"blockers",   "cells":[[],[],[],["Vendor SLA"]]}
    ]
  }
}
```

### Caching & Performance
- `IMemoryCache` with absolute expiration on file-change token — `services.AddMemoryCache()`.
- Dataset is tiny (<50KB) and render is <10ms — no need for output caching, response compression, or CDN.
- Page size budget: **<150KB total** (no framework JS bundles since interactivity is off; Blazor static SSR ships essentially no client JS for this page).

### API Design
- **No API.** This is a server-rendered page. If an API is ever needed for external consumers, expose a single `GET /data.json` minimal-API endpoint that returns the raw file.

---

## 5. Security & Infrastructure

- **Authentication:** None. Per explicit user requirement. Bind Kestrel to `http://localhost:5080` only (`appsettings.json` → `Kestrel:Endpoints:Http:Url`) so it's not network-reachable by default.
- **Authorization:** None.
- **HTTPS:** Not required for localhost. Skip `UseHttpsRedirection()` to avoid dev-cert friction.
- **Anti-forgery / CORS / CSP:** Not applicable (no forms, no cross-origin, no JS). Blazor's default antiforgery middleware can be left enabled (it's free) but has no effect.
- **Data protection:** `data.json` is plaintext, non-sensitive executive summary text. No PII expected; if PII ever added, the design assumption breaks — flag as an open question.
- **Hosting:** **`dotnet run` on the user's workstation.** That is the entire deployment story. Optionally package as a **self-contained single-file publish**:
  ```
  dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
  ```
  yielding one `ReportingDashboard.Web.exe` (~70MB) that can be double-clicked.
- **Observability:** Console logging only. No App Insights, no OpenTelemetry — out of scope for a local screenshot tool.
- **Infrastructure cost:** **$0.** Local only.

---

## 6. Risks & Trade-offs

| Risk | Severity | Mitigation |
|---|---|---|
| **Heatmap cell overflow** when month has many items at 1080px | High (visual) | Cap items-per-cell at N (e.g., 4); render "+K more" suffix. Make N configurable in JSON. |
| **Fixed 1920×1080 layout** breaks on laptop screens during demos | Medium | Add a `?zoom=0.75` query param or a browser-zoom instruction. Document "screenshot in a 1920-wide window" in README. |
| **Segoe UI absent on non-Windows render** → font metrics shift, SVG text misaligns | Medium | Pin fallback stack; declare Windows as canonical screenshot OS. |
| **Hand-rolled SVG math errors** (NOW line, month gridlines) | Medium | Unit-test coordinate calculations via xUnit; use Verify snapshot test on the rendered SVG string. |
| **`data.json` malformed** → YSOD or blank page | Medium | Validate on load; render an in-page red banner with the parse error instead of crashing. |
| **Blazor Server circuit accidentally enabled** → WebSocket overhead, loading spinner appears in screenshots | Low-Medium | Explicitly do NOT call `AddInteractiveServerComponents()`. Use static SSR only. |
| **FluentAssertions v8 license** inadvertently adopted | Low | Pin `<PackageReference ... Version="6.12.*" />` in `.csproj`. |
| **Scope creep toward "real dashboard"** (filtering, multi-project) | High (schedule) | Explicitly reject; this is a PowerPoint screenshot tool. |
| **.NET 8 EOL Nov 2026** | Low (for now) | Plan a .NET 10 LTS bump in ~12 months; trivial for this codebase. |

### Trade-offs Accepted
- **No responsive design** — accepted because screenshots are the delivery medium.
- **No component library** — accepted because design fidelity > development speed; the page is <500 lines of Razor.
- **No database** — accepted because JSON is easier to edit and diff for executives/PMs.

---

## 7. Open Questions

1. **Number of months visible** — design shows 4 columns in heatmap but 6 in timeline. Should both be configurable independently? (Recommend: yes, but default to current design.)
2. **Heatmap cell overflow policy** — truncate, shrink, scroll, or overflow? Screenshots demand a deterministic choice.
3. **Multiple projects / multi-page support** — is this truly one project per app instance, or do we need `/projects/{slug}`? (Assumed: one project, one `data.json`.)
4. **Who edits `data.json`?** PM manually? Export from ADO? If ADO export is desired later, we'd need a small CLI companion — out of scope for v1.
5. **Export format** — PNG via browser screenshot is assumed. Do we want a built-in "Print to PDF" button or headless-chrome export? (Recommend: no; defer to OS screenshot tool.)
6. **Color accessibility** — the red/yellow/green palette is not colorblind-safe. Is the exec audience okay with this, or do we need icons/patterns in addition to color?
7. **Time zone** — "NOW" line uses server local time. Acceptable for a single-user local tool; flag if ever shared.
8. **Branding** — is "Segoe UI + Microsoft blue #0078D4" the intended brand, or should colors be themeable via `data.json`? (Recommend: theme block in JSON for easy re-skinning.)

---

## 8. Implementation Recommendations

### Phasing

**Phase 0 — Scaffold (0.5 day)**
- `dotnet new sln -n ReportingDashboard`
- `dotnet new blazor -n ReportingDashboard.Web -o src/ReportingDashboard.Web --interactivity None --empty`
- `dotnet sln add src/ReportingDashboard.Web`
- Add `tests/ReportingDashboard.Web.Tests` (xUnit + bUnit).
- Copy `OriginalDesignConcept.html` CSS verbatim into `Dashboard.razor.css`.

**Phase 1 — Static port (1 day)**
- Port the reference HTML into `Dashboard.razor` with hardcoded values matching the design exactly.
- **Verify pixel parity** against `OriginalDesignConcept.png` at 1920×1080.
- This is the "screenshot works" milestone.

**Phase 2 — Data binding (1 day)**
- Define `DashboardData`, `Milestone`, `HeatmapRow` POCOs.
- `DashboardDataService` reads `wwwroot/data.json` on startup.
- Replace hardcoded Razor values with `@Model.*` bindings.
- Compute NOW line, month gridlines, current-month column in C#.

**Phase 3 — Polish (0.5 day)**
- `FileSystemWatcher` hot-reload.
- JSON schema validation with user-friendly error banner.
- README with "how to edit data.json" and "how to screenshot."

**Phase 4 — Tests (0.5 day)**
- xUnit: coordinate math, NOW-line positioning, overflow truncation.
- bUnit: `Dashboard.razor` renders without exceptions given a sample `data.json`.
- (Optional) Verify.Xunit snapshot of the SVG string for the sample data.

**Total: ~3.5 engineer-days for v1.**

### MVP Scope (v1.0)
✅ Single page, fixed 1920×1080, loads `data.json`, renders header + timeline + 4×4 heatmap, hot-reload on file change, Windows-only screenshot canonical. **Nothing else.**

### Quick Wins
1. **Ship Phase 1 (static port) first** — gets to "screenshot matches design" in one day; de-risks the visual spec before touching data.
2. **Hot-reload `data.json`** — lets the PM iterate content in real time without a dev in the loop.
3. **Self-contained single-file publish** — hand execs a 70MB `.exe` + `data.json`; they double-click and screenshot.
4. **Sample `data.json` committed to repo** — fictional project fake data (per user request) doubles as a test fixture and a "how to use" example.

### Prototype Before Committing
- **The SVG timeline math.** Build a tiny standalone Razor page with 3–4 hardcoded date ranges and verify the NOW line, milestone diamonds, and month gridlines land correctly before wiring up `data.json`. This is the only technically non-trivial piece; everything else is CSS.
- **Heatmap overflow behavior.** Prototype with a deliberately overstuffed JSON to settle the open question before design sign-off.

### Anti-Recommendations (things explicitly NOT to do)
- ❌ Do not introduce MudBlazor/Radzen — they will fight the custom CSS.
- ❌ Do not add a database or EF Core — the user asked for "super simple."
- ❌ Do not make the layout responsive — fixed 1920×1080 is the spec.
- ❌ Do not add interactivity (tooltips, filters, drill-downs) — ruins screenshot cleanliness.
- ❌ Do not use `InteractiveServer` render mode — SSR is sufficient and faster.
- ❌ Do not add auth — explicitly out of scope.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/026f2c428c0c16a0dc7f0d8a4dc83be37193ee22/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
