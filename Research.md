# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-20 03:34 UTC_

### Summary

This project is a **single-page executive reporting dashboard** that renders a fictional project's milestones, shipped items, in-progress work, carryovers, and blockers from a local `data.json` file. It is intentionally **simple, screenshot-optimized, and non-enterprise**: no auth, no database, no cloud services. The output must match the visual fidelity of `OriginalDesignConcept.html` (1920×1080, Segoe UI, CSS Grid heatmap + inline SVG Gantt timeline) so the page can be screenshotted directly into PowerPoint decks. The mandated stack — **C# .NET 8 + Blazor Server, local-only, .sln structure** — is a reasonable fit: Blazor Server's server-rendered Razor components produce clean, deterministic HTML/CSS (critical for pixel-perfect screenshots), C# handles JSON deserialization natively via `System.Text.Json`, and the inline SVG timeline maps cleanly to a Razor component that iterates milestone data. No charting library is needed — the design uses hand-authored SVG, which Blazor renders natively. **Primary recommendation:** Build a **single Blazor Server app** (one `.sln`, one web project, one optional shared model library, one test project) with a single `Dashboard.razor` page, a strongly-typed `DashboardData` record tree bound from `data.json` via `IOptionsMonitor<DashboardData>` (hot-reload on file save), and **zero JavaScript**. Serve at `http://localhost:5000` and screenshot in a 1920×1080 browser window. Total expected code footprint: ~600–900 lines. ---

### Key Findings

- The design is **static, presentational, and non-interactive** — there are no forms, no filtering, no user state. Blazor Server's SignalR circuit is overkill operationally but is the mandated stack; the page works fine as a single render with no callbacks.
- The heatmap is a **CSS Grid** (`160px repeat(4,1fr)` × 5 rows) with four status rows (Shipped / In Progress / Carryover / Blockers) crossed with month columns. This is a pure Razor `@foreach` over rows and columns — no grid library required.
- The timeline is **hand-authored inline SVG** with fixed pixel coordinates. The best approach is a Razor component that **computes SVG x-coordinates from milestone dates** (linear scale between timeline start/end) rather than hardcoding pixels — this keeps `data.json` as the single source of truth.
- **No charting library (ChartJS, Plotly, ApexCharts, LiveCharts2) is needed or recommended.** They add JS interop complexity, runtime weight, and visual styling that fights the custom design. Inline SVG authored in Razor is simpler and pixel-accurate.
- **`data.json` should live next to the executable and be hot-reloadable** via `.AddJsonFile("data.json", optional:false, reloadOnChange:true)` bound to a POCO tree. This lets the user edit JSON and F5 to refresh — ideal for last-minute exec-deck tweaks.
- **Screenshot fidelity is the #1 quality metric**, not responsiveness, accessibility, or interactivity. Fix the body to 1920×1080, disable scrollbars, use `overflow:hidden`, and embed Segoe UI as a web-safe fallback stack.
- **Local-only constraint eliminates** Azure App Service, Docker, CDN, HTTPS certs, auth providers, App Insights — the entire "enterprise" ring. `dotnet run` is the deployment.
- **Testing should be minimal**: one bUnit snapshot test confirming the page renders without exception given a valid `data.json`, and one test confirming graceful handling of a missing/malformed file. Unit tests on the date-to-SVG-x math are high-value.
- The OriginalDesignConcept truncated mid-SVG — the implementation must **reconstruct the full timeline logic** (milestone diamonds, checkpoint circles, "NOW" line, swimlane rows) from the visible structure and `ReportingDashboardDesign.png`.
- **No database.** Attempting to introduce SQLite/EF Core here is over-engineering; `data.json` *is* the database. ---
- **Scaffold solution**: `dotnet new sln -n ReportingDashboard`; `dotnet new blazor -n ReportingDashboard.Web -o src/ReportingDashboard.Web --interactivity Server --empty`; add to sln.
- **Copy CSS verbatim** from `OriginalDesignConcept.html` into `wwwroot/app.css` and `Pages/Dashboard.razor.css`.
- **Author POCOs** (`DashboardData`, `TimelineConfig`, `Swimlane`, `Milestone`, `Heatmap`) as `record` types with `System.Text.Json` attributes.
- **Create `data.json`** with the fictional project data matching the design reference (Privacy Automation Release Roadmap style — or a generic substitute).
- **Build `Header.razor`** — title, subtitle, legend (static).
- **Build `TimelineSvg.razor`** — inline SVG, `DateToX` helper, month gridlines, swimlane rows, milestone glyphs, NOW line.
- **Build `HeatmapGrid.razor` + `HeatmapCell.razor`** — CSS Grid iteration over rows × months, current-month column highlighted.
- **Wire `Dashboard.razor`** as the `/` route; inject `IOptionsMonitor<DashboardData>`; `StateHasChanged` on change.
- **Add one bUnit test**: renders with sample `data.json` without throwing.
- **README**: "Edit `data.json`, run `dotnet run`, open `http://localhost:5000`, screenshot at 1920×1080."
- **Phase 1 (MVP):** Items 1–10 above. Deliverable: a screenshot-ready page matching the design.
- **Phase 2 (polish):** Parameterize month count, milestone types, swimlane count. Add `FileSystemWatcher` fallback for hot reload.
- **Phase 3 (optional):** Playwright-based "export-to-PNG" CLI (`dotnet run -- --export out.png`) for automated deck generation.
- **Phase 4 (deferred):** Multi-project selector, historical snapshots, print-CSS stylesheet.
- **Browser print-to-PDF at 1920×1080** gives a vector-quality slide asset immediately — document in README.
- **`dotnet watch run`** gives live reload on both `.razor` and `data.json` changes — showcase to the user on day 1.
- **Ship a sample `data.json`** covering all 4 heatmap categories and all 3 milestone types so the user sees the full visual vocabulary on first run.
- **`DateToX` + SVG viewBox math** — spike this in isolation (10 min) before integrating; it's the only non-trivial logic in the app.
- **Scoped CSS vs global CSS** — try both with a single component; scoped-CSS's auto-generated attribute selectors can occasionally interact oddly with deeply nested SVG. If that happens, fall back to global CSS in `app.css`.
- **Static SSR vs Interactive Server** — build the MVP in Static SSR first; if the user reports "it doesn't feel like Blazor Server," flip the render mode. Zero code changes required.
- No authentication, no authorization, no HTTPS.
- No database, no EF Core, no migrations.
- No Docker, no Kubernetes, no cloud.
- No responsive design below 1920×1080.
- No accessibility compliance target (not a shipping product).
- No charting library.
- No component library (MudBlazor/Radzen/etc.).

### Recommended Tools & Technologies

- A single-project variant (collapse `.Models` into `.Web`) is acceptable and recommended for MVP given the project's size. | Concern | Choice | Version | Notes | |---|---|---|---| | Framework | `Microsoft.AspNetCore.App` (Blazor Server) | .NET 8.0.x LTS | Razor components, SignalR circuit built-in | | Rendering mode | **Interactive Server** (default Blazor Server) | — | Required by stack mandate; page is effectively static | | Layout | **CSS Grid + Flexbox** (hand-written) | — | Matches the design reference exactly | | Charts/Viz | **Inline SVG authored in Razor** | — | No library. `<svg>` emitted directly; x-coords computed from dates | | Fonts | Segoe UI → system stack fallback | — | `font-family:'Segoe UI',Arial,sans-serif;` | | CSS strategy | **Component-scoped CSS** (`Dashboard.razor.css`) + single `site.css` for globals | — | Built-in Blazor feature; no Sass/Tailwind needed | | Icons | None required (design uses shapes only) | — | — | | JS interop | **None** | — | Explicit non-goal |
- **MudBlazor 7.x / Radzen Blazor 5.x / Blazorise 1.6** — all excellent general-purpose component libraries, but they impose their own theming that will fight the custom design. **Do not use.**
- **ChartJs.Blazor / Blazor-ApexCharts / LiveCharts2** — the Gantt timeline is not a standard chart; custom SVG is simpler and more accurate.
- **Plotly.Blazor** — overkill; adds ~3 MB of JS. | Concern | Choice | Version | |---|---|---| | Runtime | .NET | 8.0 (LTS, supported through Nov 2026) | | Web host | Kestrel (built-in) | 8.0 | | Config binding | `Microsoft.Extensions.Configuration.Json` + `IOptionsMonitor<T>` | 8.0.x | | JSON | `System.Text.Json` (built-in) | 8.0 — no Newtonsoft needed | | Logging | `Microsoft.Extensions.Logging.Console` | 8.0 — console only, local dev | | Hot reload | `dotnet watch run` | built-in |
- **`data.json`** — single file at app content root. Schema (proposed):
  ```json
  {
    "project": { "title": "...", "subtitle": "...", "backlogUrl": "...", "asOfDate": "2026-04-19" },
    "timeline": {
      "startDate": "2026-01-01", "endDate": "2026-06-30",
      "nowDate": "2026-04-19",
      "swimlanes": [
        { "id":"M1","label":"Chatbot & MS Role","color":"#0078D4",
          "milestones":[
            {"date":"2026-01-12","type":"checkpoint","label":"Jan 12"},
            {"date":"2026-03-26","type":"poc","label":"Mar 26 PoC"},
            {"date":"2026-04-15","type":"prod","label":"Apr Prod (TBD)"}
          ] }
      ]
    },
    "heatmap": {
      "months": ["Jan","Feb","Mar","Apr"],
      "currentMonthIndex": 3,
      "rows": {
        "shipped":    [["item a","item b"], [...], [...], [...]],
        "inProgress": [...],
        "carryover":  [...],
        "blockers":   [...]
      }
    }
  }
  ```
- **No database, no EF Core, no SQLite.** Explicit non-goal.
- POCOs bound via `builder.Services.Configure<DashboardData>(builder.Configuration.GetSection("dashboard"))` OR read directly with `JsonSerializer.Deserialize<DashboardData>(File.ReadAllText(...))` wrapped in a singleton service with `FileSystemWatcher` for refresh. | Concern | Choice | |---|---| | Hosting | `dotnet run` on `http://localhost:5000` | | Containerization | **None** (local-only) | | CI/CD | **None required**; optional GitHub Actions `dotnet build` + `dotnet test` on PR | | Cloud | **None** (explicitly forbidden by constraints) | | Cost | **$0** | | Concern | Choice | Version | |---|---|---| | Test framework | xUnit | 2.9.x | | Component testing | **bUnit** | 1.33.x (supports .NET 8) | | Assertions | FluentAssertions | 6.12.x | | Coverage (optional) | coverlet.collector | 6.0.x | Scope: one render-smoke test, one "bad JSON" test, unit tests for `MilestoneLayout.DateToX(DateOnly, DateOnly, DateOnly, int width)`.
- **Visual Studio 2022 17.8+** or **JetBrains Rider 2024.x** or **VS Code + C# Dev Kit**.
- **EditorConfig** + built-in .NET 8 analyzers (no StyleCop required for a project this small).
- **`dotnet format`** for consistency. --- Not MVVM, not Clean Architecture, not CQRS. Those are over-engineering for a one-page static dashboard. Instead:
```
Program.cs
  ├─ builder.Configuration.AddJsonFile("data.json", reloadOnChange:true)
  ├─ builder.Services.Configure<DashboardData>(builder.Configuration.GetSection("dashboard"))
  └─ app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

Components/
  App.razor                        ← root, <HeadContent>, no layout chrome
  Pages/Dashboard.razor            ← routes to "/", injects IOptionsMonitor<DashboardData>
  Pages/Dashboard.razor.css        ← scoped CSS, copied verbatim from OriginalDesignConcept.html
  Shared/
    Header.razor                   ← title, subtitle, legend
    TimelineSvg.razor              ← inline <svg>, computes x-coords from dates
    HeatmapGrid.razor              ← CSS Grid, @foreach rows × months
    HeatmapCell.razor              ← renders bullet items with ::before dots

Models/
  DashboardData.cs                 ← record types matching data.json
  MilestoneLayout.cs               ← static helpers: DateToX, MonthLabels
```
- Kestrel starts → `data.json` loaded by Configuration system.
- User hits `/` → SignalR circuit established → `Dashboard.razor` renders once.
- `IOptionsMonitor<DashboardData>` fires `OnChange` when `data.json` is edited → component calls `StateHasChanged()` → browser updates via circuit diff (no page refresh needed for edits).
- User takes screenshot. Done.
- Fixed viewBox width (e.g., 1560×185 per the original).
- For each swimlane: horizontal line + milestone glyphs (diamond for PoC/Prod, circle for checkpoint).
- `DateToX(date, startDate, endDate, width)` = `(date - startDate).TotalDays / (endDate - startDate).TotalDays * width`.
- "NOW" vertical dashed line at `DateToX(nowDate, …)` in red (`#EA4335`).
- Month gridlines computed from `startDate` / `endDate` range.
- CSS Grid exactly as reference: `grid-template-columns:160px repeat(N,1fr)` where N = months count (typically 4).
- Highlight `currentMonthIndex` column with the warm "apr" class variants from the reference CSS.
- Each row uses its themed color tokens (shipped=green, inProgress=blue, carryover=amber, blockers=red).
- **Not a concern.** Single user, single page, local. Blazor Server circuit overhead is negligible at localhost.
- If screenshot workflow ever needs automation: use Playwright for .NET (`Microsoft.Playwright` 1.49.x) to headlessly render at 1920×1080 and save PNG. **Deferred; not MVP.**
- **No public API.** The only "endpoint" is `GET /` rendering the Razor page. No REST, no gRPC, no SignalR hubs beyond the built-in Blazor circuit. ---

### Considerations & Risks

- **Authentication:** None. Explicit non-goal. Bind Kestrel to `127.0.0.1` only (`--urls http://127.0.0.1:5000`) to ensure the app is not reachable from other machines on the LAN.
- **Authorization:** None.
- **Data protection:** `data.json` should not contain real PII or credentials — it's fictional demo data. Document this clearly in README.
- **Transport:** HTTP (not HTTPS) is acceptable for localhost. Disable the HTTPS redirect middleware (`app.UseHttpsRedirection()` should NOT be called).
- **Antiforgery:** Blazor's built-in antiforgery is fine; no action needed.
- **CSP / headers:** Not required for local-only screenshot use.
- **Hosting:** `dotnet run` from the solution root, or published self-contained single-file exe via `dotnet publish -c Release -r win-x64 --self-contained /p:PublishSingleFile=true` for zero-install distribution to other users on the team.
- **Deployment cost:** **$0.** No cloud, no containers, no CDN.
- **Observability:** Console logs via default `ILogger`. No App Insights, no Serilog sinks beyond console. If richer local logs are desired later: `Serilog.Sinks.Console` 6.0.x + `Serilog.AspNetCore` 8.0.x. --- | Risk | Likelihood | Impact | Mitigation | |---|---|---|---| | **Blazor Server overkill for static content** — SignalR circuit for a page with no interactivity is architecturally wasteful | High | Low | Accept it; mandated stack. Document that the page is effectively static. If later permitted, consider Static Server Rendering (SSR) mode in .NET 8 which renders HTML once without a circuit — **strongly recommended** for this use case (`@rendermode` can be omitted entirely on the page; keep project as Blazor Server but use static SSR for `Dashboard.razor`). | | **Pixel drift between design and implementation** | High | High (screenshot fidelity is the product) | Copy CSS verbatim from `OriginalDesignConcept.html`. Build with browser at exact 1920×1080. Do a side-by-side comparison against `ReportingDashboardDesign.png` before sign-off. | | **Segoe UI unavailable on non-Windows dev machines** | Medium | Medium | Fallback stack `'Segoe UI', 'Selawik', Arial, sans-serif`. Document Windows as target screenshot host. | | **`data.json` schema drift as content evolves** | Medium | Medium | Use `record` types with required properties; fail fast on deserialization with a clear error page. Add a bUnit test covering the sample `data.json`. | | **Truncated SVG in design reference** means timeline logic must be inferred | Medium | Medium | Reconstruct from `ReportingDashboardDesign.png` + visible CSS classes. Parameterize via `data.json` so fixes are declarative. | | **Hot-reload of `data.json` not firing** (known edge case with `reloadOnChange` on some editors that write via rename) | Low | Low | Fallback: manual browser refresh works; optionally add a `FileSystemWatcher` with `NotifyFilters.LastWrite \| NotifyFilters.FileName`. | | **Scope creep toward "real" dashboard** (filters, drill-downs, auth) | Medium | High | Explicitly scoped out in README. Redirect such requests to a v2. | | **Screenshot automation requested later** | Medium | Low | Playwright for .NET is a clean add-on; architecture doesn't need to change. | | **Skills gap: team unfamiliar with inline SVG math** | Low | Medium | `DateToX` is ~5 lines; single unit test covers it. Document in code comments. | ---
- **Static SSR vs Interactive Server rendering?** The mandate says "Blazor Server" — does the user accept **static server-side rendered** Razor components (no SignalR circuit) within the Blazor Server project, or strictly Interactive Server mode? Static SSR is a better fit technically; Interactive Server is what most people mean by "Blazor Server." **Recommendation: Static SSR** unless user objects.
- **Number of months shown** — design shows 4 in the heatmap and 6 in the timeline. Should these be configurable in `data.json` (recommended) or hardcoded?
- **Milestone types beyond `poc`, `prod`, `checkpoint`** — any others needed (e.g., `risk`, `decision`)?
- **Swimlane count** — design shows 3 (M1/M2/M3). Is this fixed or variable?
- **"Shipped" count display** — should cell items show counts/metrics, or just labels?
- **Backlog link behavior** — the `? ADO Backlog` link: static URL per project, or omit entirely for local demo?
- **Publish target** — is a self-contained `.exe` desired for sharing with other execs, or is `dotnet run` on the author's laptop sufficient?
- **Multiple projects** — will each project get its own `data.json`, or does the app need a project selector? MVP assumes single-project. ---

### Detailed Analysis

# Research.md — Executive Project Reporting Dashboard ("My Project")

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** that renders a fictional project's milestones, shipped items, in-progress work, carryovers, and blockers from a local `data.json` file. It is intentionally **simple, screenshot-optimized, and non-enterprise**: no auth, no database, no cloud services. The output must match the visual fidelity of `OriginalDesignConcept.html` (1920×1080, Segoe UI, CSS Grid heatmap + inline SVG Gantt timeline) so the page can be screenshotted directly into PowerPoint decks.

The mandated stack — **C# .NET 8 + Blazor Server, local-only, .sln structure** — is a reasonable fit: Blazor Server's server-rendered Razor components produce clean, deterministic HTML/CSS (critical for pixel-perfect screenshots), C# handles JSON deserialization natively via `System.Text.Json`, and the inline SVG timeline maps cleanly to a Razor component that iterates milestone data. No charting library is needed — the design uses hand-authored SVG, which Blazor renders natively.

**Primary recommendation:** Build a **single Blazor Server app** (one `.sln`, one web project, one optional shared model library, one test project) with a single `Dashboard.razor` page, a strongly-typed `DashboardData` record tree bound from `data.json` via `IOptionsMonitor<DashboardData>` (hot-reload on file save), and **zero JavaScript**. Serve at `http://localhost:5000` and screenshot in a 1920×1080 browser window. Total expected code footprint: ~600–900 lines.

---

## 2. Key Findings

- The design is **static, presentational, and non-interactive** — there are no forms, no filtering, no user state. Blazor Server's SignalR circuit is overkill operationally but is the mandated stack; the page works fine as a single render with no callbacks.
- The heatmap is a **CSS Grid** (`160px repeat(4,1fr)` × 5 rows) with four status rows (Shipped / In Progress / Carryover / Blockers) crossed with month columns. This is a pure Razor `@foreach` over rows and columns — no grid library required.
- The timeline is **hand-authored inline SVG** with fixed pixel coordinates. The best approach is a Razor component that **computes SVG x-coordinates from milestone dates** (linear scale between timeline start/end) rather than hardcoding pixels — this keeps `data.json` as the single source of truth.
- **No charting library (ChartJS, Plotly, ApexCharts, LiveCharts2) is needed or recommended.** They add JS interop complexity, runtime weight, and visual styling that fights the custom design. Inline SVG authored in Razor is simpler and pixel-accurate.
- **`data.json` should live next to the executable and be hot-reloadable** via `.AddJsonFile("data.json", optional:false, reloadOnChange:true)` bound to a POCO tree. This lets the user edit JSON and F5 to refresh — ideal for last-minute exec-deck tweaks.
- **Screenshot fidelity is the #1 quality metric**, not responsiveness, accessibility, or interactivity. Fix the body to 1920×1080, disable scrollbars, use `overflow:hidden`, and embed Segoe UI as a web-safe fallback stack.
- **Local-only constraint eliminates** Azure App Service, Docker, CDN, HTTPS certs, auth providers, App Insights — the entire "enterprise" ring. `dotnet run` is the deployment.
- **Testing should be minimal**: one bUnit snapshot test confirming the page renders without exception given a valid `data.json`, and one test confirming graceful handling of a missing/malformed file. Unit tests on the date-to-SVG-x math are high-value.
- The OriginalDesignConcept truncated mid-SVG — the implementation must **reconstruct the full timeline logic** (milestone diamonds, checkpoint circles, "NOW" line, swimlane rows) from the visible structure and `ReportingDashboardDesign.png`.
- **No database.** Attempting to introduce SQLite/EF Core here is over-engineering; `data.json` *is* the database.

---

## 3. Recommended Technology Stack

### Solution Structure (.sln)
```
ReportingDashboard.sln
├── src/
│   ├── ReportingDashboard.Web/         (Blazor Server app — net8.0)
│   └── ReportingDashboard.Models/      (optional shared POCOs — net8.0 classlib)
└── tests/
    └── ReportingDashboard.Tests/       (xUnit + bUnit — net8.0)
```
A single-project variant (collapse `.Models` into `.Web`) is acceptable and recommended for MVP given the project's size.

### Frontend (Blazor Server)
| Concern | Choice | Version | Notes |
|---|---|---|---|
| Framework | `Microsoft.AspNetCore.App` (Blazor Server) | .NET 8.0.x LTS | Razor components, SignalR circuit built-in |
| Rendering mode | **Interactive Server** (default Blazor Server) | — | Required by stack mandate; page is effectively static |
| Layout | **CSS Grid + Flexbox** (hand-written) | — | Matches the design reference exactly |
| Charts/Viz | **Inline SVG authored in Razor** | — | No library. `<svg>` emitted directly; x-coords computed from dates |
| Fonts | Segoe UI → system stack fallback | — | `font-family:'Segoe UI',Arial,sans-serif;` |
| CSS strategy | **Component-scoped CSS** (`Dashboard.razor.css`) + single `site.css` for globals | — | Built-in Blazor feature; no Sass/Tailwind needed |
| Icons | None required (design uses shapes only) | — | — |
| JS interop | **None** | — | Explicit non-goal |

**Rejected alternatives (within-stack):**
- **MudBlazor 7.x / Radzen Blazor 5.x / Blazorise 1.6** — all excellent general-purpose component libraries, but they impose their own theming that will fight the custom design. **Do not use.**
- **ChartJs.Blazor / Blazor-ApexCharts / LiveCharts2** — the Gantt timeline is not a standard chart; custom SVG is simpler and more accurate.
- **Plotly.Blazor** — overkill; adds ~3 MB of JS.

### Backend / App Host
| Concern | Choice | Version |
|---|---|---|
| Runtime | .NET | 8.0 (LTS, supported through Nov 2026) |
| Web host | Kestrel (built-in) | 8.0 |
| Config binding | `Microsoft.Extensions.Configuration.Json` + `IOptionsMonitor<T>` | 8.0.x |
| JSON | `System.Text.Json` (built-in) | 8.0 — no Newtonsoft needed |
| Logging | `Microsoft.Extensions.Logging.Console` | 8.0 — console only, local dev |
| Hot reload | `dotnet watch run` | built-in |

### Data Layer
- **`data.json`** — single file at app content root. Schema (proposed):
  ```json
  {
    "project": { "title": "...", "subtitle": "...", "backlogUrl": "...", "asOfDate": "2026-04-19" },
    "timeline": {
      "startDate": "2026-01-01", "endDate": "2026-06-30",
      "nowDate": "2026-04-19",
      "swimlanes": [
        { "id":"M1","label":"Chatbot & MS Role","color":"#0078D4",
          "milestones":[
            {"date":"2026-01-12","type":"checkpoint","label":"Jan 12"},
            {"date":"2026-03-26","type":"poc","label":"Mar 26 PoC"},
            {"date":"2026-04-15","type":"prod","label":"Apr Prod (TBD)"}
          ] }
      ]
    },
    "heatmap": {
      "months": ["Jan","Feb","Mar","Apr"],
      "currentMonthIndex": 3,
      "rows": {
        "shipped":    [["item a","item b"], [...], [...], [...]],
        "inProgress": [...],
        "carryover":  [...],
        "blockers":   [...]
      }
    }
  }
  ```
- **No database, no EF Core, no SQLite.** Explicit non-goal.
- POCOs bound via `builder.Services.Configure<DashboardData>(builder.Configuration.GetSection("dashboard"))` OR read directly with `JsonSerializer.Deserialize<DashboardData>(File.ReadAllText(...))` wrapped in a singleton service with `FileSystemWatcher` for refresh.

### Infrastructure
| Concern | Choice |
|---|---|
| Hosting | `dotnet run` on `http://localhost:5000` |
| Containerization | **None** (local-only) |
| CI/CD | **None required**; optional GitHub Actions `dotnet build` + `dotnet test` on PR |
| Cloud | **None** (explicitly forbidden by constraints) |
| Cost | **$0** |

### Testing
| Concern | Choice | Version |
|---|---|---|
| Test framework | xUnit | 2.9.x |
| Component testing | **bUnit** | 1.33.x (supports .NET 8) |
| Assertions | FluentAssertions | 6.12.x |
| Coverage (optional) | coverlet.collector | 6.0.x |

Scope: one render-smoke test, one "bad JSON" test, unit tests for `MilestoneLayout.DateToX(DateOnly, DateOnly, DateOnly, int width)`.

### Tooling
- **Visual Studio 2022 17.8+** or **JetBrains Rider 2024.x** or **VS Code + C# Dev Kit**.
- **EditorConfig** + built-in .NET 8 analyzers (no StyleCop required for a project this small).
- **`dotnet format`** for consistency.

---

## 4. Architecture Recommendations

### Pattern: **Minimal Vertical Slice + Options Pattern**
Not MVVM, not Clean Architecture, not CQRS. Those are over-engineering for a one-page static dashboard. Instead:

```
Program.cs
  ├─ builder.Configuration.AddJsonFile("data.json", reloadOnChange:true)
  ├─ builder.Services.Configure<DashboardData>(builder.Configuration.GetSection("dashboard"))
  └─ app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

Components/
  App.razor                        ← root, <HeadContent>, no layout chrome
  Pages/Dashboard.razor            ← routes to "/", injects IOptionsMonitor<DashboardData>
  Pages/Dashboard.razor.css        ← scoped CSS, copied verbatim from OriginalDesignConcept.html
  Shared/
    Header.razor                   ← title, subtitle, legend
    TimelineSvg.razor              ← inline <svg>, computes x-coords from dates
    HeatmapGrid.razor              ← CSS Grid, @foreach rows × months
    HeatmapCell.razor              ← renders bullet items with ::before dots

Models/
  DashboardData.cs                 ← record types matching data.json
  MilestoneLayout.cs               ← static helpers: DateToX, MonthLabels
```

### Data Flow
1. Kestrel starts → `data.json` loaded by Configuration system.
2. User hits `/` → SignalR circuit established → `Dashboard.razor` renders once.
3. `IOptionsMonitor<DashboardData>` fires `OnChange` when `data.json` is edited → component calls `StateHasChanged()` → browser updates via circuit diff (no page refresh needed for edits).
4. User takes screenshot. Done.

### Timeline SVG Component — Concrete Design
- Fixed viewBox width (e.g., 1560×185 per the original).
- For each swimlane: horizontal line + milestone glyphs (diamond for PoC/Prod, circle for checkpoint).
- `DateToX(date, startDate, endDate, width)` = `(date - startDate).TotalDays / (endDate - startDate).TotalDays * width`.
- "NOW" vertical dashed line at `DateToX(nowDate, …)` in red (`#EA4335`).
- Month gridlines computed from `startDate` / `endDate` range.

### Heatmap Component — Concrete Design
- CSS Grid exactly as reference: `grid-template-columns:160px repeat(N,1fr)` where N = months count (typically 4).
- Highlight `currentMonthIndex` column with the warm "apr" class variants from the reference CSS.
- Each row uses its themed color tokens (shipped=green, inProgress=blue, carryover=amber, blockers=red).

### Performance & Scalability
- **Not a concern.** Single user, single page, local. Blazor Server circuit overhead is negligible at localhost.
- If screenshot workflow ever needs automation: use Playwright for .NET (`Microsoft.Playwright` 1.49.x) to headlessly render at 1920×1080 and save PNG. **Deferred; not MVP.**

### API Design
- **No public API.** The only "endpoint" is `GET /` rendering the Razor page. No REST, no gRPC, no SignalR hubs beyond the built-in Blazor circuit.

---

## 5. Security & Infrastructure

- **Authentication:** None. Explicit non-goal. Bind Kestrel to `127.0.0.1` only (`--urls http://127.0.0.1:5000`) to ensure the app is not reachable from other machines on the LAN.
- **Authorization:** None.
- **Data protection:** `data.json` should not contain real PII or credentials — it's fictional demo data. Document this clearly in README.
- **Transport:** HTTP (not HTTPS) is acceptable for localhost. Disable the HTTPS redirect middleware (`app.UseHttpsRedirection()` should NOT be called).
- **Antiforgery:** Blazor's built-in antiforgery is fine; no action needed.
- **CSP / headers:** Not required for local-only screenshot use.
- **Hosting:** `dotnet run` from the solution root, or published self-contained single-file exe via `dotnet publish -c Release -r win-x64 --self-contained /p:PublishSingleFile=true` for zero-install distribution to other users on the team.
- **Deployment cost:** **$0.** No cloud, no containers, no CDN.
- **Observability:** Console logs via default `ILogger`. No App Insights, no Serilog sinks beyond console. If richer local logs are desired later: `Serilog.Sinks.Console` 6.0.x + `Serilog.AspNetCore` 8.0.x.

---

## 6. Risks & Trade-offs

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| **Blazor Server overkill for static content** — SignalR circuit for a page with no interactivity is architecturally wasteful | High | Low | Accept it; mandated stack. Document that the page is effectively static. If later permitted, consider Static Server Rendering (SSR) mode in .NET 8 which renders HTML once without a circuit — **strongly recommended** for this use case (`@rendermode` can be omitted entirely on the page; keep project as Blazor Server but use static SSR for `Dashboard.razor`). |
| **Pixel drift between design and implementation** | High | High (screenshot fidelity is the product) | Copy CSS verbatim from `OriginalDesignConcept.html`. Build with browser at exact 1920×1080. Do a side-by-side comparison against `ReportingDashboardDesign.png` before sign-off. |
| **Segoe UI unavailable on non-Windows dev machines** | Medium | Medium | Fallback stack `'Segoe UI', 'Selawik', Arial, sans-serif`. Document Windows as target screenshot host. |
| **`data.json` schema drift as content evolves** | Medium | Medium | Use `record` types with required properties; fail fast on deserialization with a clear error page. Add a bUnit test covering the sample `data.json`. |
| **Truncated SVG in design reference** means timeline logic must be inferred | Medium | Medium | Reconstruct from `ReportingDashboardDesign.png` + visible CSS classes. Parameterize via `data.json` so fixes are declarative. |
| **Hot-reload of `data.json` not firing** (known edge case with `reloadOnChange` on some editors that write via rename) | Low | Low | Fallback: manual browser refresh works; optionally add a `FileSystemWatcher` with `NotifyFilters.LastWrite \| NotifyFilters.FileName`. |
| **Scope creep toward "real" dashboard** (filters, drill-downs, auth) | Medium | High | Explicitly scoped out in README. Redirect such requests to a v2. |
| **Screenshot automation requested later** | Medium | Low | Playwright for .NET is a clean add-on; architecture doesn't need to change. |
| **Skills gap: team unfamiliar with inline SVG math** | Low | Medium | `DateToX` is ~5 lines; single unit test covers it. Document in code comments. |

---

## 7. Open Questions

1. **Static SSR vs Interactive Server rendering?** The mandate says "Blazor Server" — does the user accept **static server-side rendered** Razor components (no SignalR circuit) within the Blazor Server project, or strictly Interactive Server mode? Static SSR is a better fit technically; Interactive Server is what most people mean by "Blazor Server." **Recommendation: Static SSR** unless user objects.
2. **Number of months shown** — design shows 4 in the heatmap and 6 in the timeline. Should these be configurable in `data.json` (recommended) or hardcoded?
3. **Milestone types beyond `poc`, `prod`, `checkpoint`** — any others needed (e.g., `risk`, `decision`)?
4. **Swimlane count** — design shows 3 (M1/M2/M3). Is this fixed or variable?
5. **"Shipped" count display** — should cell items show counts/metrics, or just labels?
6. **Backlog link behavior** — the `? ADO Backlog` link: static URL per project, or omit entirely for local demo?
7. **Publish target** — is a self-contained `.exe` desired for sharing with other execs, or is `dotnet run` on the author's laptop sufficient?
8. **Multiple projects** — will each project get its own `data.json`, or does the app need a project selector? MVP assumes single-project.

---

## 8. Implementation Recommendations

### MVP Scope (target: 1–2 days of work)
1. **Scaffold solution**: `dotnet new sln -n ReportingDashboard`; `dotnet new blazor -n ReportingDashboard.Web -o src/ReportingDashboard.Web --interactivity Server --empty`; add to sln.
2. **Copy CSS verbatim** from `OriginalDesignConcept.html` into `wwwroot/app.css` and `Pages/Dashboard.razor.css`.
3. **Author POCOs** (`DashboardData`, `TimelineConfig`, `Swimlane`, `Milestone`, `Heatmap`) as `record` types with `System.Text.Json` attributes.
4. **Create `data.json`** with the fictional project data matching the design reference (Privacy Automation Release Roadmap style — or a generic substitute).
5. **Build `Header.razor`** — title, subtitle, legend (static).
6. **Build `TimelineSvg.razor`** — inline SVG, `DateToX` helper, month gridlines, swimlane rows, milestone glyphs, NOW line.
7. **Build `HeatmapGrid.razor` + `HeatmapCell.razor`** — CSS Grid iteration over rows × months, current-month column highlighted.
8. **Wire `Dashboard.razor`** as the `/` route; inject `IOptionsMonitor<DashboardData>`; `StateHasChanged` on change.
9. **Add one bUnit test**: renders with sample `data.json` without throwing.
10. **README**: "Edit `data.json`, run `dotnet run`, open `http://localhost:5000`, screenshot at 1920×1080."

### Phasing
- **Phase 1 (MVP):** Items 1–10 above. Deliverable: a screenshot-ready page matching the design.
- **Phase 2 (polish):** Parameterize month count, milestone types, swimlane count. Add `FileSystemWatcher` fallback for hot reload.
- **Phase 3 (optional):** Playwright-based "export-to-PNG" CLI (`dotnet run -- --export out.png`) for automated deck generation.
- **Phase 4 (deferred):** Multi-project selector, historical snapshots, print-CSS stylesheet.

### Quick Wins
- **Browser print-to-PDF at 1920×1080** gives a vector-quality slide asset immediately — document in README.
- **`dotnet watch run`** gives live reload on both `.razor` and `data.json` changes — showcase to the user on day 1.
- **Ship a sample `data.json`** covering all 4 heatmap categories and all 3 milestone types so the user sees the full visual vocabulary on first run.

### Areas to Prototype Before Committing
- **`DateToX` + SVG viewBox math** — spike this in isolation (10 min) before integrating; it's the only non-trivial logic in the app.
- **Scoped CSS vs global CSS** — try both with a single component; scoped-CSS's auto-generated attribute selectors can occasionally interact oddly with deeply nested SVG. If that happens, fall back to global CSS in `app.css`.
- **Static SSR vs Interactive Server** — build the MVP in Static SSR first; if the user reports "it doesn't feel like Blazor Server," flip the render mode. Zero code changes required.

### Explicit Non-Goals (document in README)
- No authentication, no authorization, no HTTPS.
- No database, no EF Core, no migrations.
- No Docker, no Kubernetes, no cloud.
- No responsive design below 1920×1080.
- No accessibility compliance target (not a shipping product).
- No charting library.
- No component library (MudBlazor/Radzen/etc.).

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/93b527ca57056a219192ba628500eb6a1a844a08/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
