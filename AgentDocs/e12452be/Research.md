# Research

_No research has been documented yet._

## Research technology stack for Reporting Dashboard

_Researched on 2026-04-30 03:16 UTC_

### Summary

The Reporting Dashboard is a single-page, screenshot-friendly executive status view built on **Blazor Server (.NET 8)**. It renders a timeline/Gantt milestone bar and a color-coded heatmap grid showing shipped, in-progress, carryover, and blocker items by month. All display data is loaded from a checked-in **JSON configuration file** (`dashboard-data.json`), making the app trivially deployable with zero database, zero authentication, and zero cloud dependencies. **Primary recommendation:** Build this as a single Blazor Server project with inline SVG rendering for the timeline, CSS Grid/Flexbox for the heatmap, and `System.Text.Json` deserialization of a flat JSON config file. No third-party charting library is needed — the design is simple enough that hand-crafted SVG in Razor components will produce pixel-perfect results matching the HTML reference, with far less dependency overhead. Target total implementation time: **2–3 days for an experienced .NET developer**. --- | Package | Version | Required? | Purpose | |---------|---------|-----------|---------| | `Microsoft.AspNetCore.App` | 8.0.x | ✅ (implicit) | Blazor Server runtime | | `System.Text.Json` | 8.0.x | ✅ (implicit) | JSON deserialization | | `bUnit` | 1.28.x | Optional | Component testing | | `xUnit` | 2.7.x | Optional | Test framework | | `FluentAssertions` | 6.12.x | Optional | Test assertions | | CSS Class in Reference | Blazor Component | Notes | |------------------------|-----------------|-------| | `.hdr` | `DashboardHeader.razor` | Title bar with flex layout | | `.tl-area` | `TimelineSection.razor` | Contains sidebar labels + SVG | | `.tl-svg-box` | Inside `TimelineSection.razor` | SVG rendering area | | `.hm-wrap` | `HeatmapGrid.razor` | Outer wrapper | | `.hm-grid` | Inside `HeatmapGrid.razor` | CSS Grid container | | `.hm-corner`, `.hm-col-hdr` | Rendered in `HeatmapGrid.razor` | Header cells | | `.hm-row-hdr` | Rendered per row | Category label cells | | `.hm-cell` | Rendered per cell | Data cells with item lists | | `.ship-*`, `.prog-*`, `.carry-*`, `.block-*` | Dynamic CSS classes | Applied based on `category` from JSON | | Token | Hex | Usage | |-------|-----|-------| | Shipped (primary) | `#34A853` | Bullet dots, header text | | Shipped (row bg) | `#F0FBF0` | Default month cells | | Shipped (current month bg) | `#D8F2DA` | Highlighted current month | | Shipped (header bg) | `#E8F5E9` | Row label background | | In Progress (primary) | `#0078D4` | Bullet dots, header text, links | | In Progress (row bg) | `#EEF4FE` | Default month cells | | In Progress (current month bg) | `#DAE8FB` | Highlighted current month | | In Progress (header bg) | `#E3F2FD` | Row label background | | Carryover (primary) | `#F4B400` | Bullet dots, PoC diamonds | | Carryover (row bg) | `#FFFDE7` | Default month cells | | Carryover (current month bg) | `#FFF0B0` | Highlighted current month | | Carryover (header bg) | `#FFF8E1` | Row label background | | Blocker (primary) | `#EA4335` | Bullet dots, NOW line | | Blocker (row bg) | `#FFF5F5` | Default month cells | | Blocker (current month bg) | `#FFE4E4` | Highlighted current month | | Blocker (header bg) | `#FEF2F2` | Row label background | | Background | `#FFFFFF` | Page background | | Text primary | `#111` | Main text | | Text secondary | `#888` | Subtitles, muted text | | Text detail | `#333` | Cell item text | | Border | `#E0E0E0` | Grid lines | | Current month header bg | `#FFF0D0` | April column header | | Current month header text | `#C07700` | April column header text |

### Key Findings

- The original HTML design is entirely implementable with native Blazor components, inline SVG, and CSS — no JavaScript interop or charting libraries are required.
- **JSON is the optimal config format** over YAML or XML: native `System.Text.Json` support in .NET 8, no extra dependencies, easy for non-developers to hand-edit, and diffs cleanly in Git.
- The design uses a fixed 1920×1080 layout optimized for screenshots — this simplifies responsive concerns but means we should set explicit viewport dimensions and avoid fluid breakpoints.
- Blazor Server's SignalR circuit is irrelevant for this use case (static data, no interactivity), but it's the mandated stack and still works fine for serving a read-only page.
- The SVG timeline with diamonds (milestones), circles (checkpoints), and a "NOW" line is straightforward to generate programmatically from milestone data in the config file.
- The heatmap grid maps directly to CSS Grid with `grid-template-columns: 160px repeat(N, 1fr)` where N is the number of months — this should be dynamically calculated from the config data.
- The color palette is fully defined in the HTML reference: green (shipped), blue (in-progress), amber (carryover), red (blockers) — these should be defined as CSS custom properties for easy theming.
- **No database is needed.** The JSON file is the entire data layer. For future extensibility, the deserialization model could later back onto SQLite, but that's unnecessary for MVP.
- The Segoe UI font specified in the design is available on all Windows machines, which is the target environment. No web font loading needed.
- `IFileProvider` or simple `File.ReadAllText` with `IWebHostEnvironment.ContentRootPath` is sufficient for loading the config file — no need for `IOptions<T>` pattern overhead for a single static file. --- **Goal:** Display the dashboard with hardcoded data to validate layout fidelity against the HTML reference.
- `dotnet new blazor -n ReportingDashboard --interactivity Server`
- Create `Models/DashboardData.cs` with all model classes
- Create `dashboard.css` porting all styles from `OriginalDesignConcept.html`
- Build `DashboardHeader.razor` with hardcoded title/subtitle
- Build `HeatmapGrid.razor` with hardcoded data matching the reference
- **Validation:** Side-by-side screenshot comparison with `OriginalDesignConcept.html` **Goal:** Render the milestone timeline section with programmatic SVG.
- Build `TimelineSection.razor` with SVG generation
- Implement X-position calculation from dates
- Render milestone diamonds, checkpoint circles, track lines, month grid, NOW indicator
- **Validation:** Compare timeline section screenshot against reference **Goal:** Replace all hardcoded data with config file loading.
- Create `Data/dashboard-data.json` with sample data matching the reference
- Build `DashboardDataService` with `System.Text.Json` deserialization
- Register service in `Program.cs` as Singleton
- Wire up all components to use injected data
- **Validation:** Modify JSON file, restart app, confirm changes render **Goal:** Ensure pixel-perfect rendering at 1920×1080 for PowerPoint screenshots.
- Add `<meta name="viewport" content="width=1920">` or equivalent fixed-width styling
- Fine-tune spacing, font sizes, colors to match reference exactly
- Add `@media print` styles for clean printing
- Test in Edge at exactly 1920×1080 viewport
- Create `README.md` with usage instructions
- **Immediate value:** The static HTML reference already exists. Phase 1 is essentially porting it to Blazor — the visual result is available within hours.
- **Data flexibility:** Once Phase 3 is complete, anyone can update the dashboard by editing a JSON file and restarting the app. No development skills needed for data updates.
- **Multiple projects:** After Phase 3, supporting multiple project dashboards is trivial — add a second JSON file and a route parameter.
- ❌ Azure DevOps API integration for automatic data population
- ❌ Authentication or user management
- ❌ Database or persistent storage beyond the JSON file
- ❌ Real-time updates or SignalR push notifications
- ❌ Historical trend analysis or time-series charts
- ❌ Multi-tenant or multi-user editing
- ❌ Docker containerization or CI/CD pipeline
- ❌ Server-side PDF generation ---

### Recommended Tools & Technologies

- **Project:** Executive Reporting Dashboard **Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure **Date:** April 29, 2026 **Author:** Technical Research Team --- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server | .NET 8.0 (LTS) | Ships with `Microsoft.AspNetCore.App` — no extra NuGet package | | **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches the original HTML design exactly | | **Timeline/SVG** | Inline SVG via Razor | N/A | Programmatically generated `<svg>` elements in `.razor` components | | **Icons/Shapes** | Hand-coded SVG | N/A | Diamonds, circles, lines per the design spec | | **Fonts** | Segoe UI (system font) | N/A | Pre-installed on Windows; fallback to Arial | **Why no charting library:** The timeline is a horizontal bar with positioned markers — this is ~50 lines of SVG generation code. Pulling in a charting library (ApexCharts.Blazor, Radzen Charts, etc.) would add 500KB+ of JavaScript, introduce JS interop complexity, and give less pixel-level control than raw SVG. The heatmap is a CSS Grid with colored cells — no charting library handles this pattern well anyway. **Alternative considered:** `BlazorSvgHelper` (NuGet) — a thin wrapper for SVG in Blazor. Rejected because it adds abstraction without value for this simple use case. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Zero dependency; use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` | | **File Access** | `IWebHostEnvironment.ContentRootPath` | Built into .NET 8 | Resolve path to `wwwroot/data/dashboard-data.json` or project root `Data/` folder | | **Dependency Injection** | Built-in DI | .NET 8 | Register a `DashboardDataService` as Singleton (data is static per app start) | **Config file recommendation:** `Data/dashboard-data.json` placed in the project root (not `wwwroot`), copied to output on build via `.csproj` `<Content>` item. This keeps data out of the publicly-served static files directory and loads via server-side code only. **File:** `Data/dashboard-data.json`
- **vs. YAML:** .NET 8 has no built-in YAML parser; would require `YamlDotNet` (6.x) NuGet dependency. JSON is native.
- **vs. XML:** More verbose, harder to hand-edit, no advantage for this flat data structure.
- **vs. TOML:** No mature .NET parser; niche format.
- **vs. CSV:** Cannot represent nested structures (milestones with properties, heatmap items per cell).
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "currentDate": "2026-04-15",
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "milestoneStreams": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "milestones": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
      ]
    }
  ],
  "heatmapMonths": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "heatmapRows": [
    {
      "category": "shipped",
      "items": {
        "Jan": ["DSR v2 rollout", "Retention policy engine"],
        "Feb": ["Bulk export API"],
        "Mar": ["Consent SDK 3.1", "Privacy portal refresh"],
        "Apr": ["Auto-classification GA", "DPO dashboard v2"]
      }
    },
    {
      "category": "in-progress",
      "items": {
        "Jan": ["Data inventory scan"],
        "Feb": ["PDS integration", "Role mapping"],
        "Mar": ["DFD auto-review"],
        "Apr": ["Cross-border flow analysis", "Vendor assessment tool"]
      }
    },
    {
      "category": "carryover",
      "items": {
        "Jan": [],
        "Feb": ["Legacy DSAR migration"],
        "Mar": ["Legacy DSAR migration"],
        "Apr": ["Third-party cookie deprecation"]
      }
    },
    {
      "category": "blockers",
      "items": {
        "Jan": [],
        "Feb": [],
        "Mar": ["Legal review pending EU AI Act"],
        "Apr": ["Vendor API downtime", "Compliance team bandwidth"]
      }
    }
  ]
}
``` | Tool | Version | Purpose | |------|---------|---------| | **xUnit** | 2.7.x | Unit testing framework (default for .NET 8 templates) | | **bUnit** | 1.28.x+ | Blazor component unit testing — render components, assert markup | | **FluentAssertions** | 6.12.x | Readable test assertions | | **Verify** | 23.x (optional) | Snapshot testing for rendered HTML output — excellent for screenshot-oriented apps | **Why bUnit:** It renders Blazor components to HTML strings without a browser. You can assert that the SVG timeline generates correct `<polygon>` elements for milestones and that heatmap cells contain the right items. This is the gold standard for Blazor component testing. | Tool | Version | Purpose | |------|---------|---------| | **dotnet CLI** | 8.0.x | Build, run, test, publish | | **Visual Studio 2022** | 17.9+ | IDE with Blazor hot-reload support | | **VS Code + C# Dev Kit** | Latest | Lightweight alternative | ---
```
ReportingDashboard.sln
└── src/
    └── ReportingDashboard/
        ├── ReportingDashboard.csproj
        ├── Program.cs
        ├── Data/
        │   └── dashboard-data.json          ← Checked-in config file
        ├── Models/
        │   ├── DashboardData.cs             ← Root deserialization model
        │   ├── MilestoneStream.cs
        │   ├── Milestone.cs
        │   └── HeatmapRow.cs
        ├── Services/
        │   └── DashboardDataService.cs      ← Loads & caches JSON
        ├── Components/
        │   ├── Pages/
        │   │   └── Dashboard.razor          ← Single page (route: "/")
        │   ├── Layout/
        │   │   └── MainLayout.razor         ← Minimal shell
        │   ├── DashboardHeader.razor        ← Title, subtitle, legend
        │   ├── TimelineSection.razor        ← SVG milestone Gantt
        │   └── HeatmapGrid.razor            ← CSS Grid status matrix
        ├── wwwroot/
        │   └── css/
        │       └── dashboard.css            ← All styles (single file)
        └── Properties/
            └── launchSettings.json
``` **Why single project, not Clean Architecture / multi-layer:** This is a read-only, single-page, no-database app. Introducing `Application`, `Domain`, `Infrastructure` layers would be absurd over-engineering. A single project with `Models/`, `Services/`, and `Components/` folders provides sufficient separation. Map directly to the visual sections of the design:
- **`DashboardHeader.razor`** — Title bar with project name, ADO backlog link, subtitle, and legend icons. Pure HTML/CSS, receives `DashboardData` as a `[Parameter]`.
- **`TimelineSection.razor`** — Left sidebar with milestone stream labels + right SVG area with horizontal timeline bars, month grid lines, milestone diamonds/circles, and "NOW" indicator. This component takes `List<MilestoneStream>`, `DateTime currentDate`, `DateTime startDate`, `DateTime endDate` as parameters and calculates X positions proportionally.
- **`HeatmapGrid.razor`** — CSS Grid rendering the month columns and category rows. Each cell contains a list of item bullets with category-colored dot indicators. Receives `List<HeatmapRow>`, `List<string> months`, `string currentMonth` as parameters.
```
dashboard-data.json
    → DashboardDataService.LoadAsync() [called once at startup]
        → System.Text.Json deserialize → DashboardData model
            → Injected into Dashboard.razor via @inject
                → Passed as [Parameter] to child components
``` **Caching strategy:** Load the JSON file once in `DashboardDataService` constructor (or `OnInitializedAsync`), store in a private field. Since this is local-only with static data, no cache invalidation is needed. If hot-reload of data is desired later, add a `FileSystemWatcher` on the JSON file. The timeline SVG should be generated in `TimelineSection.razor` using Razor markup:
```razor
<svg width="@svgWidth" height="@svgHeight" xmlns="http://www.w3.org/2000/svg">
    @foreach (var monthLine in monthLines)
    {
        <line x1="@monthLine.X" y1="0" x2="@monthLine.X" y2="@svgHeight" 
              stroke="#bbb" stroke-opacity="0.4" />
        <text x="@(monthLine.X + 5)" y="14" fill="#666" font-size="11">
            @monthLine.Label
        </text>
    }
    @* NOW indicator *@
    <line x1="@nowX" y1="0" x2="@nowX" y2="@svgHeight" 
          stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3" />
    
    @foreach (var stream in MilestoneStreams)
    {
        @* Horizontal track line *@
        <line x1="0" y1="@stream.TrackY" x2="@svgWidth" y2="@stream.TrackY" 
              stroke="@stream.Color" stroke-width="3" />
        @foreach (var ms in stream.Milestones)
        {
            @if (ms.Type == "poc")
            {
                @* Diamond shape *@
                <polygon points="@DiamondPoints(ms.X, stream.TrackY)" 
                         fill="#F4B400" />
            }
        }
    }
</svg>
``` **X-position calculation:** `xPos = (milestone.Date - timelineStart).TotalDays / (timelineEnd - timelineStart).TotalDays * svgWidth` Use CSS custom properties for the color palette to enable easy theming:
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
    
    --font-primary: 'Segoe UI', Arial, sans-serif;
}
``` The heatmap grid CSS should mirror the original design exactly:
```css
.hm-grid {
    display: grid;
    grid-template-columns: 160px repeat(var(--month-count), 1fr);
    grid-template-rows: 36px repeat(4, 1fr);
    border: 1px solid #E0E0E0;
}
``` Use `var(--month-count)` set dynamically via Blazor `style` attribute based on the number of months in the config. ---

### Considerations & Risks

- **Recommendation: None.** Per requirements, this is a simple local tool. No auth middleware, no login, no user management. If minimal protection is ever desired in the future, the simplest addition would be a single shared key in `launchSettings.json` checked via query string — but this is explicitly out of scope.
- **No sensitive data:** The dashboard displays project status information, not PII or credentials.
- **No encryption needed:** The JSON config file is checked into source control alongside the code.
- **Git-based access control:** Whoever has access to the repo can see and edit the data. This is the desired model.
```bash
cd src/ReportingDashboard
dotnet run
# Opens at https://localhost:5001 or http://localhost:5000
``` **For sharing screenshots:** Run locally, navigate to `http://localhost:5000`, take screenshot at 1920×1080 browser window, paste into PowerPoint.
- Publish as a self-contained executable: `dotnet publish -c Release -r win-x64 --self-contained`
- Produces a single folder that can be copied to a shared network drive or another developer's machine
- No IIS, no Docker, no cloud deployment needed
- Run on an internal VM or shared dev box
- Kestrel serves directly — no reverse proxy needed for <10 concurrent users
- Configure `launchSettings.json` to bind to `0.0.0.0:5000` instead of `localhost` **$0.** This runs on existing developer hardware. No cloud resources, no database servers, no container registries. --- **Risk Level:** Low **Description:** Blazor Server maintains a SignalR WebSocket connection per client, which is unnecessary for a read-only dashboard with no interactivity. **Mitigation:** This is a mandated stack constraint and the overhead is negligible for <10 concurrent users. The SignalR circuit adds ~50KB of JS and one WebSocket connection. For a local screenshot tool, this is irrelevant. **Alternative if it becomes a problem:** Blazor Static SSR (new in .NET 8) can render the page server-side without SignalR. Add `@attribute [StreamRendering(false)]` and `@rendermode @(new ServerRenderMode(prerender: true))` to the page. This delivers pure HTML with no WebSocket — perfect for screenshot capture. **Risk Level:** Medium **Description:** Non-developers editing `dashboard-data.json` may introduce JSON syntax errors (missing commas, unclosed brackets).
- Include a JSON Schema file (`dashboard-data.schema.json`) for IDE validation
- Add startup validation in `DashboardDataService` that logs clear error messages on parse failure
- Provide a sample file with extensive comments (via `//` which `System.Text.Json` can handle with `JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip`) **Risk Level:** Low **Description:** SVG text positioning and font metrics vary slightly between Chrome, Edge, and Firefox. **Mitigation:** Since the primary use case is screenshots from a single developer's browser, this is a non-issue. Pin the "screenshot browser" to Edge (Chromium-based, ships with Windows, renders Segoe UI natively). **Risk Level:** High **Description:** Once executives see a polished dashboard, requests for real-time ADO integration, historical trend charts, multi-project comparison, and automated data population are likely. **Mitigation:** Document explicitly that this is a **manual, config-file-driven screenshot tool**. Future enhancements (ADO API integration, multi-page views) should be evaluated as separate work items with separate effort estimates. | Factor | Hand-Coded SVG | Charting Library (e.g., ApexCharts.Blazor) | |--------|---------------|-------------------------------------------| | Pixel-perfect control | ✅ Full control | ❌ Fight library defaults | | Bundle size | ✅ Zero JS | ❌ 200-500KB JS | | Maintenance | ⚠️ Must maintain SVG generation code | ✅ Library handles rendering | | Learning curve | ⚠️ Need SVG knowledge | ✅ Declarative API | | Interactivity | ❌ Manual implementation | ✅ Built-in tooltips, hover | **Decision: Hand-coded SVG.** The design is a static screenshot tool. Interactivity is not needed. Pixel-perfect fidelity to the reference design is paramount. ---
- **How often will the data be updated?** If weekly before exec reviews, JSON file editing is fine. If daily or more, consider adding a simple YAML-to-JSON converter or a minimal edit UI in a future phase.
- **Should the month columns be configurable or always show 4 months?** The reference design shows 4 months (Jan–Apr). Recommendation: make it data-driven from the JSON config so it can show 3–6 months without code changes.
- **Should the timeline show 6 months (Jan–Jun) while the heatmap shows 4 months (Jan–Apr)?** The reference design has different time ranges for these sections. Recommendation: let each section's date range be independently configurable in the JSON.
- **Multiple projects or single project?** The current design is one project per dashboard instance. If multiple projects are needed, the simplest approach is multiple JSON files with a route parameter: `/dashboard/project-alpha`, `/dashboard/project-beta`.
- **Print/PDF export?** If executives want PDF versions in addition to screenshots, the simplest approach is browser `Ctrl+P` with a `@media print` CSS block. No server-side PDF generation needed.
- **Should the "ADO Backlog" link in the header actually link to a real Azure DevOps backlog URL?** The config file includes a `backlogUrl` field for this purpose — confirm whether this should be a live link or just a visual indicator. ---

### Detailed Analysis

# Research: Technology Stack for Reporting Dashboard

**Project:** Executive Reporting Dashboard
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure
**Date:** April 29, 2026
**Author:** Technical Research Team

---

## 1. Executive Summary

The Reporting Dashboard is a single-page, screenshot-friendly executive status view built on **Blazor Server (.NET 8)**. It renders a timeline/Gantt milestone bar and a color-coded heatmap grid showing shipped, in-progress, carryover, and blocker items by month. All display data is loaded from a checked-in **JSON configuration file** (`dashboard-data.json`), making the app trivially deployable with zero database, zero authentication, and zero cloud dependencies.

**Primary recommendation:** Build this as a single Blazor Server project with inline SVG rendering for the timeline, CSS Grid/Flexbox for the heatmap, and `System.Text.Json` deserialization of a flat JSON config file. No third-party charting library is needed — the design is simple enough that hand-crafted SVG in Razor components will produce pixel-perfect results matching the HTML reference, with far less dependency overhead. Target total implementation time: **2–3 days for an experienced .NET developer**.

---

## 2. Key Findings

- The original HTML design is entirely implementable with native Blazor components, inline SVG, and CSS — no JavaScript interop or charting libraries are required.
- **JSON is the optimal config format** over YAML or XML: native `System.Text.Json` support in .NET 8, no extra dependencies, easy for non-developers to hand-edit, and diffs cleanly in Git.
- The design uses a fixed 1920×1080 layout optimized for screenshots — this simplifies responsive concerns but means we should set explicit viewport dimensions and avoid fluid breakpoints.
- Blazor Server's SignalR circuit is irrelevant for this use case (static data, no interactivity), but it's the mandated stack and still works fine for serving a read-only page.
- The SVG timeline with diamonds (milestones), circles (checkpoints), and a "NOW" line is straightforward to generate programmatically from milestone data in the config file.
- The heatmap grid maps directly to CSS Grid with `grid-template-columns: 160px repeat(N, 1fr)` where N is the number of months — this should be dynamically calculated from the config data.
- The color palette is fully defined in the HTML reference: green (shipped), blue (in-progress), amber (carryover), red (blockers) — these should be defined as CSS custom properties for easy theming.
- **No database is needed.** The JSON file is the entire data layer. For future extensibility, the deserialization model could later back onto SQLite, but that's unnecessary for MVP.
- The Segoe UI font specified in the design is available on all Windows machines, which is the target environment. No web font loading needed.
- `IFileProvider` or simple `File.ReadAllText` with `IWebHostEnvironment.ContentRootPath` is sufficient for loading the config file — no need for `IOptions<T>` pattern overhead for a single static file.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Components + CSS)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server | .NET 8.0 (LTS) | Ships with `Microsoft.AspNetCore.App` — no extra NuGet package |
| **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches the original HTML design exactly |
| **Timeline/SVG** | Inline SVG via Razor | N/A | Programmatically generated `<svg>` elements in `.razor` components |
| **Icons/Shapes** | Hand-coded SVG | N/A | Diamonds, circles, lines per the design spec |
| **Fonts** | Segoe UI (system font) | N/A | Pre-installed on Windows; fallback to Arial |

**Why no charting library:** The timeline is a horizontal bar with positioned markers — this is ~50 lines of SVG generation code. Pulling in a charting library (ApexCharts.Blazor, Radzen Charts, etc.) would add 500KB+ of JavaScript, introduce JS interop complexity, and give less pixel-level control than raw SVG. The heatmap is a CSS Grid with colored cells — no charting library handles this pattern well anyway.

**Alternative considered:** `BlazorSvgHelper` (NuGet) — a thin wrapper for SVG in Blazor. Rejected because it adds abstraction without value for this simple use case.

### Backend (Data Loading)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Zero dependency; use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` |
| **File Access** | `IWebHostEnvironment.ContentRootPath` | Built into .NET 8 | Resolve path to `wwwroot/data/dashboard-data.json` or project root `Data/` folder |
| **Dependency Injection** | Built-in DI | .NET 8 | Register a `DashboardDataService` as Singleton (data is static per app start) |

**Config file recommendation:** `Data/dashboard-data.json` placed in the project root (not `wwwroot`), copied to output on build via `.csproj` `<Content>` item. This keeps data out of the publicly-served static files directory and loads via server-side code only.

### Data Format: JSON Configuration File

**File:** `Data/dashboard-data.json`

**Why JSON over alternatives:**
- **vs. YAML:** .NET 8 has no built-in YAML parser; would require `YamlDotNet` (6.x) NuGet dependency. JSON is native.
- **vs. XML:** More verbose, harder to hand-edit, no advantage for this flat data structure.
- **vs. TOML:** No mature .NET parser; niche format.
- **vs. CSV:** Cannot represent nested structures (milestones with properties, heatmap items per cell).

**Sample `dashboard-data.json` structure:**

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "currentDate": "2026-04-15",
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "milestoneStreams": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "milestones": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
      ]
    }
  ],
  "heatmapMonths": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "heatmapRows": [
    {
      "category": "shipped",
      "items": {
        "Jan": ["DSR v2 rollout", "Retention policy engine"],
        "Feb": ["Bulk export API"],
        "Mar": ["Consent SDK 3.1", "Privacy portal refresh"],
        "Apr": ["Auto-classification GA", "DPO dashboard v2"]
      }
    },
    {
      "category": "in-progress",
      "items": {
        "Jan": ["Data inventory scan"],
        "Feb": ["PDS integration", "Role mapping"],
        "Mar": ["DFD auto-review"],
        "Apr": ["Cross-border flow analysis", "Vendor assessment tool"]
      }
    },
    {
      "category": "carryover",
      "items": {
        "Jan": [],
        "Feb": ["Legacy DSAR migration"],
        "Mar": ["Legacy DSAR migration"],
        "Apr": ["Third-party cookie deprecation"]
      }
    },
    {
      "category": "blockers",
      "items": {
        "Jan": [],
        "Feb": [],
        "Mar": ["Legal review pending EU AI Act"],
        "Apr": ["Vendor API downtime", "Compliance team bandwidth"]
      }
    }
  ]
}
```

### Testing

| Tool | Version | Purpose |
|------|---------|---------|
| **xUnit** | 2.7.x | Unit testing framework (default for .NET 8 templates) |
| **bUnit** | 1.28.x+ | Blazor component unit testing — render components, assert markup |
| **FluentAssertions** | 6.12.x | Readable test assertions |
| **Verify** | 23.x (optional) | Snapshot testing for rendered HTML output — excellent for screenshot-oriented apps |

**Why bUnit:** It renders Blazor components to HTML strings without a browser. You can assert that the SVG timeline generates correct `<polygon>` elements for milestones and that heatmap cells contain the right items. This is the gold standard for Blazor component testing.

### Build & Tooling

| Tool | Version | Purpose |
|------|---------|---------|
| **dotnet CLI** | 8.0.x | Build, run, test, publish |
| **Visual Studio 2022** | 17.9+ | IDE with Blazor hot-reload support |
| **VS Code + C# Dev Kit** | Latest | Lightweight alternative |

---

## 4. Architecture Recommendations

### Overall Architecture: Single-Project Blazor Server App

```
ReportingDashboard.sln
└── src/
    └── ReportingDashboard/
        ├── ReportingDashboard.csproj
        ├── Program.cs
        ├── Data/
        │   └── dashboard-data.json          ← Checked-in config file
        ├── Models/
        │   ├── DashboardData.cs             ← Root deserialization model
        │   ├── MilestoneStream.cs
        │   ├── Milestone.cs
        │   └── HeatmapRow.cs
        ├── Services/
        │   └── DashboardDataService.cs      ← Loads & caches JSON
        ├── Components/
        │   ├── Pages/
        │   │   └── Dashboard.razor          ← Single page (route: "/")
        │   ├── Layout/
        │   │   └── MainLayout.razor         ← Minimal shell
        │   ├── DashboardHeader.razor        ← Title, subtitle, legend
        │   ├── TimelineSection.razor        ← SVG milestone Gantt
        │   └── HeatmapGrid.razor            ← CSS Grid status matrix
        ├── wwwroot/
        │   └── css/
        │       └── dashboard.css            ← All styles (single file)
        └── Properties/
            └── launchSettings.json
```

**Why single project, not Clean Architecture / multi-layer:** This is a read-only, single-page, no-database app. Introducing `Application`, `Domain`, `Infrastructure` layers would be absurd over-engineering. A single project with `Models/`, `Services/`, and `Components/` folders provides sufficient separation.

### Component Architecture

Map directly to the visual sections of the design:

1. **`DashboardHeader.razor`** — Title bar with project name, ADO backlog link, subtitle, and legend icons. Pure HTML/CSS, receives `DashboardData` as a `[Parameter]`.

2. **`TimelineSection.razor`** — Left sidebar with milestone stream labels + right SVG area with horizontal timeline bars, month grid lines, milestone diamonds/circles, and "NOW" indicator. This component takes `List<MilestoneStream>`, `DateTime currentDate`, `DateTime startDate`, `DateTime endDate` as parameters and calculates X positions proportionally.

3. **`HeatmapGrid.razor`** — CSS Grid rendering the month columns and category rows. Each cell contains a list of item bullets with category-colored dot indicators. Receives `List<HeatmapRow>`, `List<string> months`, `string currentMonth` as parameters.

### Data Flow

```
dashboard-data.json
    → DashboardDataService.LoadAsync() [called once at startup]
        → System.Text.Json deserialize → DashboardData model
            → Injected into Dashboard.razor via @inject
                → Passed as [Parameter] to child components
```

**Caching strategy:** Load the JSON file once in `DashboardDataService` constructor (or `OnInitializedAsync`), store in a private field. Since this is local-only with static data, no cache invalidation is needed. If hot-reload of data is desired later, add a `FileSystemWatcher` on the JSON file.

### SVG Timeline Rendering Strategy

The timeline SVG should be generated in `TimelineSection.razor` using Razor markup:

```razor
<svg width="@svgWidth" height="@svgHeight" xmlns="http://www.w3.org/2000/svg">
    @foreach (var monthLine in monthLines)
    {
        <line x1="@monthLine.X" y1="0" x2="@monthLine.X" y2="@svgHeight" 
              stroke="#bbb" stroke-opacity="0.4" />
        <text x="@(monthLine.X + 5)" y="14" fill="#666" font-size="11">
            @monthLine.Label
        </text>
    }
    @* NOW indicator *@
    <line x1="@nowX" y1="0" x2="@nowX" y2="@svgHeight" 
          stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3" />
    
    @foreach (var stream in MilestoneStreams)
    {
        @* Horizontal track line *@
        <line x1="0" y1="@stream.TrackY" x2="@svgWidth" y2="@stream.TrackY" 
              stroke="@stream.Color" stroke-width="3" />
        @foreach (var ms in stream.Milestones)
        {
            @if (ms.Type == "poc")
            {
                @* Diamond shape *@
                <polygon points="@DiamondPoints(ms.X, stream.TrackY)" 
                         fill="#F4B400" />
            }
        }
    }
</svg>
```

**X-position calculation:** `xPos = (milestone.Date - timelineStart).TotalDays / (timelineEnd - timelineStart).TotalDays * svgWidth`

### CSS Architecture

**Single file: `wwwroot/css/dashboard.css`**

Use CSS custom properties for the color palette to enable easy theming:

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
    
    --font-primary: 'Segoe UI', Arial, sans-serif;
}
```

The heatmap grid CSS should mirror the original design exactly:

```css
.hm-grid {
    display: grid;
    grid-template-columns: 160px repeat(var(--month-count), 1fr);
    grid-template-rows: 36px repeat(4, 1fr);
    border: 1px solid #E0E0E0;
}
```

Use `var(--month-count)` set dynamically via Blazor `style` attribute based on the number of months in the config.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**Recommendation: None.** Per requirements, this is a simple local tool. No auth middleware, no login, no user management.

If minimal protection is ever desired in the future, the simplest addition would be a single shared key in `launchSettings.json` checked via query string — but this is explicitly out of scope.

### Data Protection

- **No sensitive data:** The dashboard displays project status information, not PII or credentials.
- **No encryption needed:** The JSON config file is checked into source control alongside the code.
- **Git-based access control:** Whoever has access to the repo can see and edit the data. This is the desired model.

### Hosting & Deployment

**Primary: `dotnet run` on the developer's local machine.**

```bash
cd src/ReportingDashboard
dotnet run
# Opens at https://localhost:5001 or http://localhost:5000
```

**For sharing screenshots:** Run locally, navigate to `http://localhost:5000`, take screenshot at 1920×1080 browser window, paste into PowerPoint.

**For sharing with a small team (optional):**
- Publish as a self-contained executable: `dotnet publish -c Release -r win-x64 --self-contained`
- Produces a single folder that can be copied to a shared network drive or another developer's machine
- No IIS, no Docker, no cloud deployment needed

**For persistent team access (optional future):**
- Run on an internal VM or shared dev box
- Kestrel serves directly — no reverse proxy needed for <10 concurrent users
- Configure `launchSettings.json` to bind to `0.0.0.0:5000` instead of `localhost`

### Infrastructure Costs

**$0.** This runs on existing developer hardware. No cloud resources, no database servers, no container registries.

---

## 6. Risks & Trade-offs

### Risk 1: Blazor Server Overhead for a Static Page
**Risk Level:** Low
**Description:** Blazor Server maintains a SignalR WebSocket connection per client, which is unnecessary for a read-only dashboard with no interactivity.
**Mitigation:** This is a mandated stack constraint and the overhead is negligible for <10 concurrent users. The SignalR circuit adds ~50KB of JS and one WebSocket connection. For a local screenshot tool, this is irrelevant.
**Alternative if it becomes a problem:** Blazor Static SSR (new in .NET 8) can render the page server-side without SignalR. Add `@attribute [StreamRendering(false)]` and `@rendermode @(new ServerRenderMode(prerender: true))` to the page. This delivers pure HTML with no WebSocket — perfect for screenshot capture.

### Risk 2: JSON Config File Editing Errors
**Risk Level:** Medium
**Description:** Non-developers editing `dashboard-data.json` may introduce JSON syntax errors (missing commas, unclosed brackets).
**Mitigation:**
- Include a JSON Schema file (`dashboard-data.schema.json`) for IDE validation
- Add startup validation in `DashboardDataService` that logs clear error messages on parse failure
- Provide a sample file with extensive comments (via `//` which `System.Text.Json` can handle with `JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip`)

### Risk 3: SVG Rendering Inconsistencies Across Browsers
**Risk Level:** Low
**Description:** SVG text positioning and font metrics vary slightly between Chrome, Edge, and Firefox.
**Mitigation:** Since the primary use case is screenshots from a single developer's browser, this is a non-issue. Pin the "screenshot browser" to Edge (Chromium-based, ships with Windows, renders Segoe UI natively).

### Risk 4: Scope Creep Toward a Full Project Management Tool
**Risk Level:** High
**Description:** Once executives see a polished dashboard, requests for real-time ADO integration, historical trend charts, multi-project comparison, and automated data population are likely.
**Mitigation:** Document explicitly that this is a **manual, config-file-driven screenshot tool**. Future enhancements (ADO API integration, multi-page views) should be evaluated as separate work items with separate effort estimates.

### Trade-off: Hand-Coded SVG vs. Charting Library

| Factor | Hand-Coded SVG | Charting Library (e.g., ApexCharts.Blazor) |
|--------|---------------|-------------------------------------------|
| Pixel-perfect control | ✅ Full control | ❌ Fight library defaults |
| Bundle size | ✅ Zero JS | ❌ 200-500KB JS |
| Maintenance | ⚠️ Must maintain SVG generation code | ✅ Library handles rendering |
| Learning curve | ⚠️ Need SVG knowledge | ✅ Declarative API |
| Interactivity | ❌ Manual implementation | ✅ Built-in tooltips, hover |

**Decision: Hand-coded SVG.** The design is a static screenshot tool. Interactivity is not needed. Pixel-perfect fidelity to the reference design is paramount.

---

## 7. Open Questions

1. **How often will the data be updated?** If weekly before exec reviews, JSON file editing is fine. If daily or more, consider adding a simple YAML-to-JSON converter or a minimal edit UI in a future phase.

2. **Should the month columns be configurable or always show 4 months?** The reference design shows 4 months (Jan–Apr). Recommendation: make it data-driven from the JSON config so it can show 3–6 months without code changes.

3. **Should the timeline show 6 months (Jan–Jun) while the heatmap shows 4 months (Jan–Apr)?** The reference design has different time ranges for these sections. Recommendation: let each section's date range be independently configurable in the JSON.

4. **Multiple projects or single project?** The current design is one project per dashboard instance. If multiple projects are needed, the simplest approach is multiple JSON files with a route parameter: `/dashboard/project-alpha`, `/dashboard/project-beta`.

5. **Print/PDF export?** If executives want PDF versions in addition to screenshots, the simplest approach is browser `Ctrl+P` with a `@media print` CSS block. No server-side PDF generation needed.

6. **Should the "ADO Backlog" link in the header actually link to a real Azure DevOps backlog URL?** The config file includes a `backlogUrl` field for this purpose — confirm whether this should be a live link or just a visual indicator.

---

## 8. Implementation Recommendations

### Phase 1: MVP (Day 1) — Skeleton + Static Rendering
**Goal:** Display the dashboard with hardcoded data to validate layout fidelity against the HTML reference.

1. `dotnet new blazor -n ReportingDashboard --interactivity Server`
2. Create `Models/DashboardData.cs` with all model classes
3. Create `dashboard.css` porting all styles from `OriginalDesignConcept.html`
4. Build `DashboardHeader.razor` with hardcoded title/subtitle
5. Build `HeatmapGrid.razor` with hardcoded data matching the reference
6. **Validation:** Side-by-side screenshot comparison with `OriginalDesignConcept.html`

### Phase 2: Timeline SVG (Day 1–2)
**Goal:** Render the milestone timeline section with programmatic SVG.

1. Build `TimelineSection.razor` with SVG generation
2. Implement X-position calculation from dates
3. Render milestone diamonds, checkpoint circles, track lines, month grid, NOW indicator
4. **Validation:** Compare timeline section screenshot against reference

### Phase 3: JSON Data Loading (Day 2)
**Goal:** Replace all hardcoded data with config file loading.

1. Create `Data/dashboard-data.json` with sample data matching the reference
2. Build `DashboardDataService` with `System.Text.Json` deserialization
3. Register service in `Program.cs` as Singleton
4. Wire up all components to use injected data
5. **Validation:** Modify JSON file, restart app, confirm changes render

### Phase 4: Polish & Screenshot Optimization (Day 2–3)
**Goal:** Ensure pixel-perfect rendering at 1920×1080 for PowerPoint screenshots.

1. Add `<meta name="viewport" content="width=1920">` or equivalent fixed-width styling
2. Fine-tune spacing, font sizes, colors to match reference exactly
3. Add `@media print` styles for clean printing
4. Test in Edge at exactly 1920×1080 viewport
5. Create `README.md` with usage instructions

### Quick Wins
- **Immediate value:** The static HTML reference already exists. Phase 1 is essentially porting it to Blazor — the visual result is available within hours.
- **Data flexibility:** Once Phase 3 is complete, anyone can update the dashboard by editing a JSON file and restarting the app. No development skills needed for data updates.
- **Multiple projects:** After Phase 3, supporting multiple project dashboards is trivial — add a second JSON file and a route parameter.

### What NOT to Build (Explicit Out of Scope)
- ❌ Azure DevOps API integration for automatic data population
- ❌ Authentication or user management
- ❌ Database or persistent storage beyond the JSON file
- ❌ Real-time updates or SignalR push notifications
- ❌ Historical trend analysis or time-series charts
- ❌ Multi-tenant or multi-user editing
- ❌ Docker containerization or CI/CD pipeline
- ❌ Server-side PDF generation

---

## Appendix A: NuGet Package Summary

| Package | Version | Required? | Purpose |
|---------|---------|-----------|---------|
| `Microsoft.AspNetCore.App` | 8.0.x | ✅ (implicit) | Blazor Server runtime |
| `System.Text.Json` | 8.0.x | ✅ (implicit) | JSON deserialization |
| `bUnit` | 1.28.x | Optional | Component testing |
| `xUnit` | 2.7.x | Optional | Test framework |
| `FluentAssertions` | 6.12.x | Optional | Test assertions |

**Total third-party NuGet dependencies for production: 0.**

## Appendix B: CSS-to-Component Mapping from Reference Design

| CSS Class in Reference | Blazor Component | Notes |
|------------------------|-----------------|-------|
| `.hdr` | `DashboardHeader.razor` | Title bar with flex layout |
| `.tl-area` | `TimelineSection.razor` | Contains sidebar labels + SVG |
| `.tl-svg-box` | Inside `TimelineSection.razor` | SVG rendering area |
| `.hm-wrap` | `HeatmapGrid.razor` | Outer wrapper |
| `.hm-grid` | Inside `HeatmapGrid.razor` | CSS Grid container |
| `.hm-corner`, `.hm-col-hdr` | Rendered in `HeatmapGrid.razor` | Header cells |
| `.hm-row-hdr` | Rendered per row | Category label cells |
| `.hm-cell` | Rendered per cell | Data cells with item lists |
| `.ship-*`, `.prog-*`, `.carry-*`, `.block-*` | Dynamic CSS classes | Applied based on `category` from JSON |

## Appendix C: Color Palette Reference

| Token | Hex | Usage |
|-------|-----|-------|
| Shipped (primary) | `#34A853` | Bullet dots, header text |
| Shipped (row bg) | `#F0FBF0` | Default month cells |
| Shipped (current month bg) | `#D8F2DA` | Highlighted current month |
| Shipped (header bg) | `#E8F5E9` | Row label background |
| In Progress (primary) | `#0078D4` | Bullet dots, header text, links |
| In Progress (row bg) | `#EEF4FE` | Default month cells |
| In Progress (current month bg) | `#DAE8FB` | Highlighted current month |
| In Progress (header bg) | `#E3F2FD` | Row label background |
| Carryover (primary) | `#F4B400` | Bullet dots, PoC diamonds |
| Carryover (row bg) | `#FFFDE7` | Default month cells |
| Carryover (current month bg) | `#FFF0B0` | Highlighted current month |
| Carryover (header bg) | `#FFF8E1` | Row label background |
| Blocker (primary) | `#EA4335` | Bullet dots, NOW line |
| Blocker (row bg) | `#FFF5F5` | Default month cells |
| Blocker (current month bg) | `#FFE4E4` | Highlighted current month |
| Blocker (header bg) | `#FEF2F2` | Row label background |
| Background | `#FFFFFF` | Page background |
| Text primary | `#111` | Main text |
| Text secondary | `#888` | Subtitles, muted text |
| Text detail | `#333` | Cell item text |
| Border | `#E0E0E0` | Grid lines |
| Current month header bg | `#FFF0D0` | April column header |
| Current month header text | `#C07700` | April column header text |

## Visual Design References

The following design reference files were found in the repository. These MUST be used as the canonical visual specification when building UI components.

### `AgentDocs/e12452be/ReportingDashboardDesign.png`

**Type:** Design Image — engineers should reference this file visually

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/7ea2dccf672cdc77798c1f43f9ba310c64e6cfc8/AgentDocs/e12452be/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
