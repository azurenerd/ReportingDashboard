# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-17 02:42 UTC_

### Summary

This project is a **single-page Blazor Server application** that renders an executive-facing project reporting dashboard. It reads all data from a local `data.json` file and renders a timeline/milestone visualization (SVG) with a color-coded heatmap grid showing shipped, in-progress, carryover, and blocker items — designed for 1920×1080 screenshot capture for PowerPoint decks. **Primary Recommendation:** Build this as a minimal Blazor Server app with zero authentication, zero database, and zero cloud dependencies. The entire data layer is a single `data.json` file deserialized with `System.Text.Json`. The SVG timeline and CSS Grid heatmap should be implemented as pure Blazor components with inline SVG rendering — no JavaScript charting libraries needed. This keeps the project dead simple, screenshot-friendly, and maintainable by anyone on the team. The total solution should be **under 15 files** including the `.sln`, `.csproj`, `Program.cs`, one Razor page, a few Blazor components, a CSS file, a data model, a JSON service, and the `data.json` config. ---

### Key Findings

- **No charting library is needed.** The design uses simple SVG elements (lines, circles, diamonds, text) and CSS Grid — all achievable with native Blazor markup and inline SVG. Adding a charting library would be over-engineering.
- **`System.Text.Json` (built into .NET 8) is the only data library required.** The data source is a single JSON file; no ORM, no database, no Entity Framework.
- **Blazor Server is ideal for this use case.** The app runs locally, serves one user at a time (the report author), and needs zero offline capability. Server-side rendering avoids WASM download overhead and keeps the project trivially simple.
- **The design is pixel-precise and fixed-resolution (1920×1080).** This actually simplifies implementation — no responsive breakpoints needed. Target the exact viewport size for screenshot fidelity.
- **CSS isolation (`.razor.css` files) in Blazor provides scoped styling** that maps cleanly to the design's component structure (header, timeline, heatmap).
- **Hot reload in .NET 8 Blazor** allows rapid iteration on the visual design — critical for matching the reference mockup.
- **The `data.json` approach means non-developers can update project data** by editing a JSON file without touching code or redeploying.
- **No authentication, authorization, or security hardening is needed.** This is a local tool for generating screenshots. Adding auth would be wasted effort.
- **SVG rendering directly in Razor components** gives full control over milestone diamonds, checkpoint circles, timeline bars, and the "NOW" marker — exactly matching the reference design.
- **The existing HTML design translates almost 1:1 to Blazor components.** Each visual section (header, legend, timeline area, heatmap grid) becomes a Razor component with the same CSS. --- **Goal:** Render the dashboard with fake data, matching the reference design.
- `dotnet new blazorserver -n ReportingDashboard --framework net8.0`
- Create `data.json` with fictional project data matching the reference design's structure
- Create `DashboardData` model classes and `DashboardDataService`
- Build `Dashboard.razor` page with inline structure (no sub-components yet)
- Port the reference CSS directly into `site.css`
- Render the heatmap grid with CSS Grid
- Render the SVG timeline with hardcoded positions
- Visual comparison against reference screenshot — iterate until pixel-match **Deliverable:** A running page that looks like the reference when viewed at 1920×1080. **Goal:** All visual elements driven by `data.json`.
- Replace hardcoded items with data-bound loops
- Implement SVG date-to-pixel coordinate mapping
- Dynamic month columns (support 3–6 months)
- Current month highlighting based on `currentDate`
- "NOW" line positioned from `currentDate` **Deliverable:** Change `data.json`, refresh browser, see updated dashboard. **Goal:** Clean component architecture and quality-of-life features.
- Extract sub-components (`DashboardHeader`, `TimelineSection`, `HeatmapGrid`, `HeatmapCell`)
- Add `FileSystemWatcher` for auto-reload on `data.json` changes
- Add CSS custom properties for the color palette
- Add `<title>` and favicon
- Write 3–5 unit tests for JSON deserialization and date-to-pixel math **Deliverable:** Clean, maintainable codebase ready for ongoing use. | Win | Effort | Value | |-----|--------|-------| | **Copy the reference HTML's CSS verbatim** into `site.css` | 10 min | Instant visual match; iterate from there | | **Use `dotnet watch`** during development | 0 min | Live reload on save; dramatically speeds UI iteration | | **Start with a single `Dashboard.razor` file** | — | Get the visual right first, refactor into components later | | **Use browser DevTools at 1920×1080** | 2 min | Verify screenshot dimensions without resizing your monitor | | **Add a `sample-data.json`** with realistic-looking fictional data | 15 min | Immediately demonstrates value to stakeholders |
- **SVG date positioning math** — Build a small prototype that takes a date range and renders month lines + event markers. This is the trickiest part of the implementation and should be validated early.
- **CSS Grid heatmap with variable content heights** — The reference design uses `grid-template-rows: 36px repeat(4,1fr)`. Verify that `1fr` rows handle varying numbers of bullet items gracefully, or switch to `auto` rows.
- **`FileSystemWatcher` + Blazor re-render** — Test that file change detection triggers `InvokeAsync(StateHasChanged)` reliably on Windows. Known edge case: some editors (VS Code) write to temp files then rename, which may fire multiple events.
- ❌ Authentication / authorization
- ❌ Database or Entity Framework
- ❌ REST API endpoints
- ❌ Responsive/mobile layout
- ❌ Dark mode
- ❌ Export to PDF/PNG (use browser screenshots instead)
- ❌ Historical data tracking
- ❌ Multi-user collaboration
- ❌ CI/CD pipeline (it's a local tool)
- ❌ Docker container (unless specifically requested later)
- ❌ Any NuGet packages beyond the default Blazor Server template **Zero additional packages.** The default `dotnet new blazorserver` template on .NET 8 includes everything needed:
- `Microsoft.AspNetCore.App` (framework reference) — Blazor Server, Kestrel, DI, configuration
- `System.Text.Json` — JSON serialization (part of the framework) If tests are added:
```xml
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="bunit" Version="1.31.3" />
<PackageReference Include="FluentAssertions" Version="7.0.0" />
``` --- | Category | Text Color | Header BG | Cell BG | Current Month BG | Dot Color | |----------|-----------|-----------|---------|-------------------|-----------| | Shipped | `#1B7A28` | `#E8F5E9` | `#F0FBF0` | `#D8F2DA` | `#34A853` | | In Progress | `#1565C0` | `#E3F2FD` | `#EEF4FE` | `#DAE8FB` | `#0078D4` | | Carryover | `#B45309` | `#FFF8E1` | `#FFFDE7` | `#FFF0B0` | `#F4B400` | | Blockers | `#991B1B` | `#FEF2F2` | `#FFF5F5` | `#FFE4E4` | `#EA4335` | | Design Element | SVG Element | Key Attributes | |---------------|------------|----------------| | Month grid line | `<line>` | `stroke="#bbb" stroke-opacity="0.4"` | | Milestone bar | `<line>` | `stroke-width="3"` with milestone color | | Checkpoint | `<circle>` | `r="4-7" fill="white" stroke="color"` | | PoC milestone | `<polygon>` | Diamond shape, `fill="#F4B400"` | | Production milestone | `<polygon>` | Diamond shape, `fill="#34A853"` | | "NOW" line | `<line>` | `stroke="#EA4335" stroke-dasharray="5,3"` | | Drop shadow | `<filter>` | `<feDropShadow dx="0" dy="1" stdDeviation="1.5">` |

### Recommended Tools & Technologies

- **Project:** Executive Reporting Dashboard — Single-Page Milestone & Progress View **Stack:** C# .NET 8 / Blazor Server / Local-only / .sln structure **Date:** April 17, 2026 **Author:** Technical Research Team --- | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **Framework** | Blazor Server (.NET 8) | `net8.0` | Server-side interactive rendering | | **CSS Layout** | Native CSS Grid + Flexbox | — | Heatmap grid (`grid-template-columns: 160px repeat(4,1fr)`) and header/legend layout | | **Charting/Viz** | Inline SVG in Razor | — | Timeline milestones, bars, diamonds, circles, "NOW" line | | **Styling** | Blazor CSS Isolation (`.razor.css`) | Built-in | Scoped per-component styles matching the design's color palette | | **Icons/Shapes** | Raw SVG markup | — | Diamond markers (`<polygon>`), circles (`<circle>`), dashed lines | | **Font** | Segoe UI (system font) | — | Matches design spec; no web font loading needed | **Why no charting library?** Libraries like `Radzen.Blazor`, `MudBlazor Charts`, or `BlazorApexCharts` are designed for interactive data charts (bar, line, pie). The design's timeline is a **custom SVG layout** with positioned elements at specific x-coordinates mapped to dates — this is easier to build with raw SVG than to configure in any charting library. The heatmap is a CSS Grid with colored cells containing bullet lists, not a data visualization. | Library | Why Rejected | |---------|-------------| | `Radzen.Blazor` (v5.x) | Adds 2MB+ dependency for chart components we wouldn't use. Their grid is for data tables, not heatmaps. | | `MudBlazor` (v7.x) | Excellent component library but massive overkill — we need zero form inputs, zero dialogs, zero data tables. | | `BlazorApexCharts` (v3.x) | JavaScript interop dependency for charts that don't match our timeline layout. | | `ChartJs.Blazor` | Abandoned; last update 2022. | | `Blazorise` (v1.6+) | Full UI framework; licensing complexity (commercial for some features). | | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Read and parse `data.json` | | **File Watching** | `FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload when `data.json` changes | | **Configuration** | `IOptions<T>` pattern | Built into .NET 8 | Bind JSON config to strongly-typed C# models | | **Dependency Injection** | Built-in DI container | Built into .NET 8 | Register data service as singleton | **No database.** The entire data store is `data.json` sitting in the app's content root. `System.Text.Json` with source generators (for AOT-friendly serialization) is the right choice over `Newtonsoft.Json` — it's faster, built-in, and has no licensing considerations. | Approach | Technology | Rationale | |----------|-----------|-----------| | **Primary** | `data.json` flat file | Single file, human-editable, no install dependencies | | **Alternative** | SQLite via `Microsoft.Data.Sqlite` (v8.0.x) | Only if future requirements demand querying; overkill for now | **Recommendation:** Stay with `data.json`. The data volume is trivially small (dozens of items across 4 categories × a few months). SQLite adds migration complexity and tooling requirements for zero benefit. | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **Runtime** | .NET 8 SDK | `8.0.400+` | Build and run | | **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Latest | Development | | **Project Structure** | `.sln` + single `.csproj` | — | Standard .NET solution | | **Hot Reload** | `dotnet watch` | Built-in | Rapid UI iteration | | **Screenshot** | Browser DevTools / Snipping Tool | — | Capture 1920×1080 for PowerPoint | | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **Unit Tests** | `xUnit` | `2.9.x` | Test JSON parsing, data model mapping | | **Assertions** | `FluentAssertions` | `7.x` | Readable test assertions | | **UI Tests** | `bUnit` | `1.31.x` | Blazor component rendering tests | | **Integration** | `Microsoft.AspNetCore.Mvc.Testing` | `8.0.x` | Full app integration tests | **Pragmatic note:** For a tool this simple, unit tests on the JSON deserialization logic and a few bUnit smoke tests on component rendering are sufficient. Don't over-invest in test infrastructure. ---
```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── Program.cs                    # Minimal startup
    ├── ReportingDashboard.csproj     # Single project, net8.0
    ├── wwwroot/
    │   ├── css/site.css              # Global styles (colors, grid, typography)
    │   └── data.json                 # THE data file
    ├── Models/
    │   ├── DashboardData.cs          # Root model
    │   ├── Milestone.cs              # Timeline milestone
    │   ├── HeatmapCategory.cs        # Shipped/InProgress/Carryover/Blocker
    │   └── HeatmapItem.cs            # Individual item in a cell
    ├── Services/
    │   └── DashboardDataService.cs   # Reads & deserializes data.json
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor       # The single page
    │   ├── Layout/
    │   │   └── MainLayout.razor      # Minimal shell
    │   ├── DashboardHeader.razor     # Title, subtitle, legend
    │   ├── TimelineSection.razor     # SVG milestone timeline
    │   ├── HeatmapGrid.razor         # CSS Grid heatmap
    │   └── HeatmapCell.razor         # Individual cell with bullet items
    └── Properties/
        └── launchSettings.json
```
```
data.json → DashboardDataService (System.Text.Json) → DashboardData model
    → Dashboard.razor (page) 
        → DashboardHeader.razor (props: title, subtitle, date, legend)
        → TimelineSection.razor (props: milestones[], date range, "now" date)
        → HeatmapGrid.razor (props: categories[], months[])
            → HeatmapCell.razor × N (props: items[], categoryColor)
``` | Design Section | Blazor Component | CSS Strategy | |---------------|-----------------|--------------| | Header bar (title + legend) | `DashboardHeader.razor` | Flexbox (`justify-content: space-between`) | | Legend icons (diamond, circle, line) | Inline SVG in `DashboardHeader` | Inline styles matching design | | Timeline area (left labels + SVG) | `TimelineSection.razor` | Flexbox container; inline SVG for the timeline | | SVG milestone visualization | Inline `<svg>` in `TimelineSection` | SVG positioned elements; x-coords calculated from date math | | Month grid lines | `<line>` elements in SVG | Calculated x-positions based on date range | | "NOW" marker | `<line>` + `<text>` in SVG | Red dashed line (`stroke-dasharray: 5,3`) | | Diamond milestones | `<polygon points="...">` in SVG | Gold (#F4B400) for PoC, Green (#34A853) for Production | | Heatmap grid | `HeatmapGrid.razor` | CSS Grid: `grid-template-columns: 160px repeat(N, 1fr)` | | Row headers (Shipped, In Progress, etc.) | Part of `HeatmapGrid` | Color-coded backgrounds per category | | Data cells | `HeatmapCell.razor` | Bullet items with colored dots via `::before` pseudo-element | | Current month highlight | `.apr` class equivalent | Slightly darker background on current month column |
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-10",
  "months": ["Jan 2026", "Feb 2026", "Mar 2026", "Apr 2026"],
  "currentMonthIndex": 3,
  "milestones": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
        { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
      ]
    }
  ],
  "categories": [
    {
      "name": "Shipped",
      "colorScheme": "green",
      "rows": {
        "Jan 2026": ["Item A shipped", "Item B shipped"],
        "Feb 2026": ["Item C shipped"],
        "Mar 2026": [],
        "Apr 2026": ["Item D shipped"]
      }
    },
    {
      "name": "In Progress",
      "colorScheme": "blue",
      "rows": { ... }
    },
    {
      "name": "Carryover",
      "colorScheme": "amber",
      "rows": { ... }
    },
    {
      "name": "Blockers",
      "colorScheme": "red",
      "rows": { ... }
    }
  ]
}
``` The timeline SVG should be rendered server-side in Razor with calculated positions:
- **Define the date range** (e.g., Jan 1 → Jun 30 = 181 days).
- **Map each date to an x-coordinate:** `x = (daysSinceStart / totalDays) * svgWidth`.
- **Render month grid lines** at day-of-month boundaries.
- **Render milestone bars** as horizontal `<line>` elements spanning their date range.
- **Render events** as `<circle>` (checkpoints), `<polygon>` (PoC diamonds, production diamonds), positioned at their date's x-coordinate.
- **Render the "NOW" marker** as a red dashed vertical line at today's date x-position. All of this is pure C# date math → SVG coordinate math. No JavaScript needed. Centralize the design's color palette in CSS custom properties for maintainability:
```css
:root {
  --color-shipped: #34A853;
  --color-shipped-bg: #F0FBF0;
  --color-shipped-bg-current: #D8F2DA;
  --color-shipped-header: #E8F5E9;
  --color-progress: #0078D4;
  --color-progress-bg: #EEF4FE;
  --color-progress-bg-current: #DAE8FB;
  --color-progress-header: #E3F2FD;
  --color-carryover: #F4B400;
  --color-carryover-bg: #FFFDE7;
  --color-carryover-bg-current: #FFF0B0;
  --color-carryover-header: #FFF8E1;
  --color-blocker: #EA4335;
  --color-blocker-bg: #FFF5F5;
  --color-blocker-bg-current: #FFE4E4;
  --color-blocker-header: #FEF2F2;
  --color-poc-milestone: #F4B400;
  --color-prod-milestone: #34A853;
  --color-now-line: #EA4335;
  --color-link: #0078D4;
  --color-border: #E0E0E0;
  --color-text-primary: #111;
  --color-text-secondary: #888;
  --color-text-muted: #999;
}
``` ---

### Considerations & Risks

- **None.** This is explicitly a local-only tool for a single user generating screenshots. Adding authentication would be wasted effort and would complicate the "just run it and take a screenshot" workflow. If future requirements demand multi-user access, the simplest path would be Windows Authentication (`Negotiate`) since this is a local/intranet scenario — zero user management, leverages existing domain credentials. But **do not build this now.**
- **No sensitive data.** The `data.json` contains project status information — item names, dates, and categories. This is the same information that ends up in PowerPoint slides sent to executives.
- **No encryption needed** for data at rest or in transit (localhost only).
- **No PII** is stored or processed. | Aspect | Recommendation | |--------|---------------| | **Runtime** | `dotnet run` from command line or Visual Studio F5 | | **Port** | Default Kestrel on `https://localhost:5001` or `http://localhost:5000` | | **Deployment** | None — run from source. Optionally `dotnet publish -c Release` for a self-contained folder. | | **Containerization** | Not needed. If desired later, a simple `Dockerfile` with `mcr.microsoft.com/dotnet/aspnet:8.0` base. | | **Reverse Proxy** | Not needed for local use. | **$0.** This runs on the developer's local machine. No cloud resources, no hosting costs, no licenses beyond what the team already has (Visual Studio, .NET SDK). The intended workflow is:
- Edit `data.json` with current project data
- Run `dotnet run` (or `dotnet watch` for live reload)
- Open browser to `http://localhost:5000`
- Set browser window to 1920×1080 (or use DevTools device emulation)
- Take screenshot (Win+Shift+S, or browser DevTools full-page capture)
- Paste into PowerPoint **Tip:** Chrome DevTools → Toggle Device Toolbar → set to 1920×1080 → Capture screenshot gives pixel-perfect results without resizing the actual browser window. --- | Risk | Likelihood | Impact | Mitigation | |------|-----------|--------|------------| | **SVG rendering differences across browsers** | Low | Medium | Target Chrome only for screenshots; test in Edge as backup. Document the "official" browser. | | **CSS Grid rendering at exactly 1920×1080** | Low | High | Use fixed `width: 1920px; height: 1080px` on body (matching the original design). Don't attempt responsive layout. | | **`data.json` schema drift** | Medium | Low | Define strong C# model types; `System.Text.Json` will throw on missing required properties. Add a JSON Schema file for documentation. | | **Blazor Server SignalR overhead** | Low | Low | For a single local user, SignalR connection overhead is negligible. The page is essentially static after initial render. | | **Over-engineering temptation** | High | Medium | This is a screenshot tool, not a SaaS product. Resist adding features. The original HTML file is ~200 lines — the Blazor version should be comparably simple. | | Decision | Trade-off | Rationale | |----------|-----------|-----------| | **No charting library** | Must hand-code SVG positions | Full design control; avoids fighting library defaults to match pixel-perfect mockup | | **No database** | Can't query historical data | Not needed; each screenshot is a point-in-time report. Historical tracking lives in PowerPoint/SharePoint. | | **No responsive design** | Only looks right at 1920×1080 | The output is screenshots, not a web app. Fixed dimensions ensure consistent captures. | | **Blazor Server over Blazor WASM** | Requires `dotnet run` process | Simpler project structure; no WASM download; better hot reload experience | | **No component library (MudBlazor etc.)** | Must write CSS from scratch | The CSS is already written in the reference HTML; porting it is trivial and avoids 2MB+ of unused dependencies | **Not applicable.** This serves one user on localhost. If the need arises to serve multiple users:
- Deploy behind IIS with Windows Auth
- Move `data.json` to a shared network path or SQLite database
- Add `ResponseCache` attributes for the rendered page But this is hypothetical and should not influence the current design. --- | # | Question | Who Decides | Impact | |---|----------|-------------|--------| | 1 | **Should `data.json` support multiple projects or just one?** | Product Owner | Affects data model complexity. Recommend: one project per `data.json` file; switch projects by swapping files. | | 2 | **How many months should the heatmap show?** | Report Author | The reference shows 4 months. The schema should support N months but default to 4. | | 3 | **Should the timeline date range be configurable or auto-calculated?** | Engineering | Recommend: specify `startDate` and `endDate` in `data.json`; auto-calculate SVG positions from those. | | 4 | **Is the "ADO Backlog" link in the header clickable or just decorative?** | Product Owner | If clickable, include `backlogUrl` in `data.json`. If decorative, hardcode or omit. | | 5 | **Should the app auto-refresh when `data.json` changes?** | Engineering | Nice-to-have via `FileSystemWatcher` + Blazor `StateHasChanged()`. Low effort, high convenience during editing. Recommend: yes, implement it. | | 6 | **Target browser for screenshots?** | Report Author | Recommend: Chrome. Document this as the "official" rendering browser for consistent output. | | 7 | **Should the page include a "Last Updated" timestamp?** | Product Owner | Easy to add from `data.json` file's last-modified date or an explicit field. | ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Reporting Dashboard — Single-Page Milestone & Progress View  
**Stack:** C# .NET 8 / Blazor Server / Local-only / .sln structure  
**Date:** April 17, 2026  
**Author:** Technical Research Team

---

## 1. Executive Summary

This project is a **single-page Blazor Server application** that renders an executive-facing project reporting dashboard. It reads all data from a local `data.json` file and renders a timeline/milestone visualization (SVG) with a color-coded heatmap grid showing shipped, in-progress, carryover, and blocker items — designed for 1920×1080 screenshot capture for PowerPoint decks.

**Primary Recommendation:** Build this as a minimal Blazor Server app with zero authentication, zero database, and zero cloud dependencies. The entire data layer is a single `data.json` file deserialized with `System.Text.Json`. The SVG timeline and CSS Grid heatmap should be implemented as pure Blazor components with inline SVG rendering — no JavaScript charting libraries needed. This keeps the project dead simple, screenshot-friendly, and maintainable by anyone on the team.

The total solution should be **under 15 files** including the `.sln`, `.csproj`, `Program.cs`, one Razor page, a few Blazor components, a CSS file, a data model, a JSON service, and the `data.json` config.

---

## 2. Key Findings

- **No charting library is needed.** The design uses simple SVG elements (lines, circles, diamonds, text) and CSS Grid — all achievable with native Blazor markup and inline SVG. Adding a charting library would be over-engineering.
- **`System.Text.Json` (built into .NET 8) is the only data library required.** The data source is a single JSON file; no ORM, no database, no Entity Framework.
- **Blazor Server is ideal for this use case.** The app runs locally, serves one user at a time (the report author), and needs zero offline capability. Server-side rendering avoids WASM download overhead and keeps the project trivially simple.
- **The design is pixel-precise and fixed-resolution (1920×1080).** This actually simplifies implementation — no responsive breakpoints needed. Target the exact viewport size for screenshot fidelity.
- **CSS isolation (`.razor.css` files) in Blazor provides scoped styling** that maps cleanly to the design's component structure (header, timeline, heatmap).
- **Hot reload in .NET 8 Blazor** allows rapid iteration on the visual design — critical for matching the reference mockup.
- **The `data.json` approach means non-developers can update project data** by editing a JSON file without touching code or redeploying.
- **No authentication, authorization, or security hardening is needed.** This is a local tool for generating screenshots. Adding auth would be wasted effort.
- **SVG rendering directly in Razor components** gives full control over milestone diamonds, checkpoint circles, timeline bars, and the "NOW" marker — exactly matching the reference design.
- **The existing HTML design translates almost 1:1 to Blazor components.** Each visual section (header, legend, timeline area, heatmap grid) becomes a Razor component with the same CSS.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server UI)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Framework** | Blazor Server (.NET 8) | `net8.0` | Server-side interactive rendering |
| **CSS Layout** | Native CSS Grid + Flexbox | — | Heatmap grid (`grid-template-columns: 160px repeat(4,1fr)`) and header/legend layout |
| **Charting/Viz** | Inline SVG in Razor | — | Timeline milestones, bars, diamonds, circles, "NOW" line |
| **Styling** | Blazor CSS Isolation (`.razor.css`) | Built-in | Scoped per-component styles matching the design's color palette |
| **Icons/Shapes** | Raw SVG markup | — | Diamond markers (`<polygon>`), circles (`<circle>`), dashed lines |
| **Font** | Segoe UI (system font) | — | Matches design spec; no web font loading needed |

**Why no charting library?** Libraries like `Radzen.Blazor`, `MudBlazor Charts`, or `BlazorApexCharts` are designed for interactive data charts (bar, line, pie). The design's timeline is a **custom SVG layout** with positioned elements at specific x-coordinates mapped to dates — this is easier to build with raw SVG than to configure in any charting library. The heatmap is a CSS Grid with colored cells containing bullet lists, not a data visualization.

**Alternatives evaluated and rejected:**

| Library | Why Rejected |
|---------|-------------|
| `Radzen.Blazor` (v5.x) | Adds 2MB+ dependency for chart components we wouldn't use. Their grid is for data tables, not heatmaps. |
| `MudBlazor` (v7.x) | Excellent component library but massive overkill — we need zero form inputs, zero dialogs, zero data tables. |
| `BlazorApexCharts` (v3.x) | JavaScript interop dependency for charts that don't match our timeline layout. |
| `ChartJs.Blazor` | Abandoned; last update 2022. |
| `Blazorise` (v1.6+) | Full UI framework; licensing complexity (commercial for some features). |

### Backend (Data Layer)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Read and parse `data.json` |
| **File Watching** | `FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload when `data.json` changes |
| **Configuration** | `IOptions<T>` pattern | Built into .NET 8 | Bind JSON config to strongly-typed C# models |
| **Dependency Injection** | Built-in DI container | Built into .NET 8 | Register data service as singleton |

**No database.** The entire data store is `data.json` sitting in the app's content root. `System.Text.Json` with source generators (for AOT-friendly serialization) is the right choice over `Newtonsoft.Json` — it's faster, built-in, and has no licensing considerations.

### Data Storage

| Approach | Technology | Rationale |
|----------|-----------|-----------|
| **Primary** | `data.json` flat file | Single file, human-editable, no install dependencies |
| **Alternative** | SQLite via `Microsoft.Data.Sqlite` (v8.0.x) | Only if future requirements demand querying; overkill for now |

**Recommendation:** Stay with `data.json`. The data volume is trivially small (dozens of items across 4 categories × a few months). SQLite adds migration complexity and tooling requirements for zero benefit.

### Infrastructure & Tooling

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Runtime** | .NET 8 SDK | `8.0.400+` | Build and run |
| **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Latest | Development |
| **Project Structure** | `.sln` + single `.csproj` | — | Standard .NET solution |
| **Hot Reload** | `dotnet watch` | Built-in | Rapid UI iteration |
| **Screenshot** | Browser DevTools / Snipping Tool | — | Capture 1920×1080 for PowerPoint |

### Testing

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Unit Tests** | `xUnit` | `2.9.x` | Test JSON parsing, data model mapping |
| **Assertions** | `FluentAssertions` | `7.x` | Readable test assertions |
| **UI Tests** | `bUnit` | `1.31.x` | Blazor component rendering tests |
| **Integration** | `Microsoft.AspNetCore.Mvc.Testing` | `8.0.x` | Full app integration tests |

**Pragmatic note:** For a tool this simple, unit tests on the JSON deserialization logic and a few bUnit smoke tests on component rendering are sufficient. Don't over-invest in test infrastructure.

---

## 4. Architecture Recommendations

### Overall Architecture: Minimal Layered Monolith

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── Program.cs                    # Minimal startup
    ├── ReportingDashboard.csproj     # Single project, net8.0
    ├── wwwroot/
    │   ├── css/site.css              # Global styles (colors, grid, typography)
    │   └── data.json                 # THE data file
    ├── Models/
    │   ├── DashboardData.cs          # Root model
    │   ├── Milestone.cs              # Timeline milestone
    │   ├── HeatmapCategory.cs        # Shipped/InProgress/Carryover/Blocker
    │   └── HeatmapItem.cs            # Individual item in a cell
    ├── Services/
    │   └── DashboardDataService.cs   # Reads & deserializes data.json
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor       # The single page
    │   ├── Layout/
    │   │   └── MainLayout.razor      # Minimal shell
    │   ├── DashboardHeader.razor     # Title, subtitle, legend
    │   ├── TimelineSection.razor     # SVG milestone timeline
    │   ├── HeatmapGrid.razor         # CSS Grid heatmap
    │   └── HeatmapCell.razor         # Individual cell with bullet items
    └── Properties/
        └── launchSettings.json
```

### Data Flow

```
data.json → DashboardDataService (System.Text.Json) → DashboardData model
    → Dashboard.razor (page) 
        → DashboardHeader.razor (props: title, subtitle, date, legend)
        → TimelineSection.razor (props: milestones[], date range, "now" date)
        → HeatmapGrid.razor (props: categories[], months[])
            → HeatmapCell.razor × N (props: items[], categoryColor)
```

### Component Design Mapping to the Reference HTML

| Design Section | Blazor Component | CSS Strategy |
|---------------|-----------------|--------------|
| Header bar (title + legend) | `DashboardHeader.razor` | Flexbox (`justify-content: space-between`) |
| Legend icons (diamond, circle, line) | Inline SVG in `DashboardHeader` | Inline styles matching design |
| Timeline area (left labels + SVG) | `TimelineSection.razor` | Flexbox container; inline SVG for the timeline |
| SVG milestone visualization | Inline `<svg>` in `TimelineSection` | SVG positioned elements; x-coords calculated from date math |
| Month grid lines | `<line>` elements in SVG | Calculated x-positions based on date range |
| "NOW" marker | `<line>` + `<text>` in SVG | Red dashed line (`stroke-dasharray: 5,3`) |
| Diamond milestones | `<polygon points="...">` in SVG | Gold (#F4B400) for PoC, Green (#34A853) for Production |
| Heatmap grid | `HeatmapGrid.razor` | CSS Grid: `grid-template-columns: 160px repeat(N, 1fr)` |
| Row headers (Shipped, In Progress, etc.) | Part of `HeatmapGrid` | Color-coded backgrounds per category |
| Data cells | `HeatmapCell.razor` | Bullet items with colored dots via `::before` pseudo-element |
| Current month highlight | `.apr` class equivalent | Slightly darker background on current month column |

### `data.json` Schema Design

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-10",
  "months": ["Jan 2026", "Feb 2026", "Mar 2026", "Apr 2026"],
  "currentMonthIndex": 3,
  "milestones": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
        { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
      ]
    }
  ],
  "categories": [
    {
      "name": "Shipped",
      "colorScheme": "green",
      "rows": {
        "Jan 2026": ["Item A shipped", "Item B shipped"],
        "Feb 2026": ["Item C shipped"],
        "Mar 2026": [],
        "Apr 2026": ["Item D shipped"]
      }
    },
    {
      "name": "In Progress",
      "colorScheme": "blue",
      "rows": { ... }
    },
    {
      "name": "Carryover",
      "colorScheme": "amber",
      "rows": { ... }
    },
    {
      "name": "Blockers",
      "colorScheme": "red",
      "rows": { ... }
    }
  ]
}
```

### SVG Timeline Rendering Strategy

The timeline SVG should be rendered server-side in Razor with calculated positions:

1. **Define the date range** (e.g., Jan 1 → Jun 30 = 181 days).
2. **Map each date to an x-coordinate:** `x = (daysSinceStart / totalDays) * svgWidth`.
3. **Render month grid lines** at day-of-month boundaries.
4. **Render milestone bars** as horizontal `<line>` elements spanning their date range.
5. **Render events** as `<circle>` (checkpoints), `<polygon>` (PoC diamonds, production diamonds), positioned at their date's x-coordinate.
6. **Render the "NOW" marker** as a red dashed vertical line at today's date x-position.

All of this is pure C# date math → SVG coordinate math. No JavaScript needed.

### Color System

Centralize the design's color palette in CSS custom properties for maintainability:

```css
:root {
  --color-shipped: #34A853;
  --color-shipped-bg: #F0FBF0;
  --color-shipped-bg-current: #D8F2DA;
  --color-shipped-header: #E8F5E9;
  --color-progress: #0078D4;
  --color-progress-bg: #EEF4FE;
  --color-progress-bg-current: #DAE8FB;
  --color-progress-header: #E3F2FD;
  --color-carryover: #F4B400;
  --color-carryover-bg: #FFFDE7;
  --color-carryover-bg-current: #FFF0B0;
  --color-carryover-header: #FFF8E1;
  --color-blocker: #EA4335;
  --color-blocker-bg: #FFF5F5;
  --color-blocker-bg-current: #FFE4E4;
  --color-blocker-header: #FEF2F2;
  --color-poc-milestone: #F4B400;
  --color-prod-milestone: #34A853;
  --color-now-line: #EA4335;
  --color-link: #0078D4;
  --color-border: #E0E0E0;
  --color-text-primary: #111;
  --color-text-secondary: #888;
  --color-text-muted: #999;
}
```

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly a local-only tool for a single user generating screenshots. Adding authentication would be wasted effort and would complicate the "just run it and take a screenshot" workflow.

If future requirements demand multi-user access, the simplest path would be Windows Authentication (`Negotiate`) since this is a local/intranet scenario — zero user management, leverages existing domain credentials. But **do not build this now.**

### Data Protection

- **No sensitive data.** The `data.json` contains project status information — item names, dates, and categories. This is the same information that ends up in PowerPoint slides sent to executives.
- **No encryption needed** for data at rest or in transit (localhost only).
- **No PII** is stored or processed.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | `dotnet run` from command line or Visual Studio F5 |
| **Port** | Default Kestrel on `https://localhost:5001` or `http://localhost:5000` |
| **Deployment** | None — run from source. Optionally `dotnet publish -c Release` for a self-contained folder. |
| **Containerization** | Not needed. If desired later, a simple `Dockerfile` with `mcr.microsoft.com/dotnet/aspnet:8.0` base. |
| **Reverse Proxy** | Not needed for local use. |

### Infrastructure Costs

**$0.** This runs on the developer's local machine. No cloud resources, no hosting costs, no licenses beyond what the team already has (Visual Studio, .NET SDK).

### Screenshot Workflow

The intended workflow is:

1. Edit `data.json` with current project data
2. Run `dotnet run` (or `dotnet watch` for live reload)
3. Open browser to `http://localhost:5000`
4. Set browser window to 1920×1080 (or use DevTools device emulation)
5. Take screenshot (Win+Shift+S, or browser DevTools full-page capture)
6. Paste into PowerPoint

**Tip:** Chrome DevTools → Toggle Device Toolbar → set to 1920×1080 → Capture screenshot gives pixel-perfect results without resizing the actual browser window.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| **SVG rendering differences across browsers** | Low | Medium | Target Chrome only for screenshots; test in Edge as backup. Document the "official" browser. |
| **CSS Grid rendering at exactly 1920×1080** | Low | High | Use fixed `width: 1920px; height: 1080px` on body (matching the original design). Don't attempt responsive layout. |
| **`data.json` schema drift** | Medium | Low | Define strong C# model types; `System.Text.Json` will throw on missing required properties. Add a JSON Schema file for documentation. |
| **Blazor Server SignalR overhead** | Low | Low | For a single local user, SignalR connection overhead is negligible. The page is essentially static after initial render. |
| **Over-engineering temptation** | High | Medium | This is a screenshot tool, not a SaaS product. Resist adding features. The original HTML file is ~200 lines — the Blazor version should be comparably simple. |

### Trade-offs Made

| Decision | Trade-off | Rationale |
|----------|-----------|-----------|
| **No charting library** | Must hand-code SVG positions | Full design control; avoids fighting library defaults to match pixel-perfect mockup |
| **No database** | Can't query historical data | Not needed; each screenshot is a point-in-time report. Historical tracking lives in PowerPoint/SharePoint. |
| **No responsive design** | Only looks right at 1920×1080 | The output is screenshots, not a web app. Fixed dimensions ensure consistent captures. |
| **Blazor Server over Blazor WASM** | Requires `dotnet run` process | Simpler project structure; no WASM download; better hot reload experience |
| **No component library (MudBlazor etc.)** | Must write CSS from scratch | The CSS is already written in the reference HTML; porting it is trivial and avoids 2MB+ of unused dependencies |

### Scalability Considerations

**Not applicable.** This serves one user on localhost. If the need arises to serve multiple users:

1. Deploy behind IIS with Windows Auth
2. Move `data.json` to a shared network path or SQLite database
3. Add `ResponseCache` attributes for the rendered page

But this is hypothetical and should not influence the current design.

---

## 7. Open Questions

| # | Question | Who Decides | Impact |
|---|----------|-------------|--------|
| 1 | **Should `data.json` support multiple projects or just one?** | Product Owner | Affects data model complexity. Recommend: one project per `data.json` file; switch projects by swapping files. |
| 2 | **How many months should the heatmap show?** | Report Author | The reference shows 4 months. The schema should support N months but default to 4. |
| 3 | **Should the timeline date range be configurable or auto-calculated?** | Engineering | Recommend: specify `startDate` and `endDate` in `data.json`; auto-calculate SVG positions from those. |
| 4 | **Is the "ADO Backlog" link in the header clickable or just decorative?** | Product Owner | If clickable, include `backlogUrl` in `data.json`. If decorative, hardcode or omit. |
| 5 | **Should the app auto-refresh when `data.json` changes?** | Engineering | Nice-to-have via `FileSystemWatcher` + Blazor `StateHasChanged()`. Low effort, high convenience during editing. Recommend: yes, implement it. |
| 6 | **Target browser for screenshots?** | Report Author | Recommend: Chrome. Document this as the "official" rendering browser for consistent output. |
| 7 | **Should the page include a "Last Updated" timestamp?** | Product Owner | Easy to add from `data.json` file's last-modified date or an explicit field. |

---

## 8. Implementation Recommendations

### Phasing

#### Phase 1: Core MVP (1–2 days)

**Goal:** Render the dashboard with fake data, matching the reference design.

1. `dotnet new blazorserver -n ReportingDashboard --framework net8.0`
2. Create `data.json` with fictional project data matching the reference design's structure
3. Create `DashboardData` model classes and `DashboardDataService`
4. Build `Dashboard.razor` page with inline structure (no sub-components yet)
5. Port the reference CSS directly into `site.css`
6. Render the heatmap grid with CSS Grid
7. Render the SVG timeline with hardcoded positions
8. Visual comparison against reference screenshot — iterate until pixel-match

**Deliverable:** A running page that looks like the reference when viewed at 1920×1080.

#### Phase 2: Dynamic Data (0.5–1 day)

**Goal:** All visual elements driven by `data.json`.

1. Replace hardcoded items with data-bound loops
2. Implement SVG date-to-pixel coordinate mapping
3. Dynamic month columns (support 3–6 months)
4. Current month highlighting based on `currentDate`
5. "NOW" line positioned from `currentDate`

**Deliverable:** Change `data.json`, refresh browser, see updated dashboard.

#### Phase 3: Polish & Refactor (0.5 day)

**Goal:** Clean component architecture and quality-of-life features.

1. Extract sub-components (`DashboardHeader`, `TimelineSection`, `HeatmapGrid`, `HeatmapCell`)
2. Add `FileSystemWatcher` for auto-reload on `data.json` changes
3. Add CSS custom properties for the color palette
4. Add `<title>` and favicon
5. Write 3–5 unit tests for JSON deserialization and date-to-pixel math

**Deliverable:** Clean, maintainable codebase ready for ongoing use.

### Quick Wins

| Win | Effort | Value |
|-----|--------|-------|
| **Copy the reference HTML's CSS verbatim** into `site.css` | 10 min | Instant visual match; iterate from there |
| **Use `dotnet watch`** during development | 0 min | Live reload on save; dramatically speeds UI iteration |
| **Start with a single `Dashboard.razor` file** | — | Get the visual right first, refactor into components later |
| **Use browser DevTools at 1920×1080** | 2 min | Verify screenshot dimensions without resizing your monitor |
| **Add a `sample-data.json`** with realistic-looking fictional data | 15 min | Immediately demonstrates value to stakeholders |

### Areas for Prototyping

1. **SVG date positioning math** — Build a small prototype that takes a date range and renders month lines + event markers. This is the trickiest part of the implementation and should be validated early.
2. **CSS Grid heatmap with variable content heights** — The reference design uses `grid-template-rows: 36px repeat(4,1fr)`. Verify that `1fr` rows handle varying numbers of bullet items gracefully, or switch to `auto` rows.
3. **`FileSystemWatcher` + Blazor re-render** — Test that file change detection triggers `InvokeAsync(StateHasChanged)` reliably on Windows. Known edge case: some editors (VS Code) write to temp files then rename, which may fire multiple events.

### What NOT to Build

- ❌ Authentication / authorization
- ❌ Database or Entity Framework
- ❌ REST API endpoints
- ❌ Responsive/mobile layout
- ❌ Dark mode
- ❌ Export to PDF/PNG (use browser screenshots instead)
- ❌ Historical data tracking
- ❌ Multi-user collaboration
- ❌ CI/CD pipeline (it's a local tool)
- ❌ Docker container (unless specifically requested later)
- ❌ Any NuGet packages beyond the default Blazor Server template

### NuGet Packages Required

**Zero additional packages.** The default `dotnet new blazorserver` template on .NET 8 includes everything needed:

- `Microsoft.AspNetCore.App` (framework reference) — Blazor Server, Kestrel, DI, configuration
- `System.Text.Json` — JSON serialization (part of the framework)

If tests are added:

```xml
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
<PackageReference Include="bunit" Version="1.31.3" />
<PackageReference Include="FluentAssertions" Version="7.0.0" />
```

---

## Appendix A: Reference Design Color Mapping

| Category | Text Color | Header BG | Cell BG | Current Month BG | Dot Color |
|----------|-----------|-----------|---------|-------------------|-----------|
| Shipped | `#1B7A28` | `#E8F5E9` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#1565C0` | `#E3F2FD` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#B45309` | `#FFF8E1` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#991B1B` | `#FEF2F2` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

## Appendix B: SVG Element Reference

| Design Element | SVG Element | Key Attributes |
|---------------|------------|----------------|
| Month grid line | `<line>` | `stroke="#bbb" stroke-opacity="0.4"` |
| Milestone bar | `<line>` | `stroke-width="3"` with milestone color |
| Checkpoint | `<circle>` | `r="4-7" fill="white" stroke="color"` |
| PoC milestone | `<polygon>` | Diamond shape, `fill="#F4B400"` |
| Production milestone | `<polygon>` | Diamond shape, `fill="#34A853"` |
| "NOW" line | `<line>` | `stroke="#EA4335" stroke-dasharray="5,3"` |
| Drop shadow | `<filter>` | `<feDropShadow dx="0" dy="1" stdDeviation="1.5">` |

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/b0b9f13f6bd22427f49f134c156e88c7575267f0/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
