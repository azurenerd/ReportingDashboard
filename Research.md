# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-16 21:17 UTC_

### Summary

This project is a lightweight, single-page executive reporting dashboard built with **Blazor Server on .NET 8**. It renders a timeline/milestone view and a color-coded heatmap grid showing project status (Shipped, In Progress, Carryover, Blockers) by month — designed to be screenshot-friendly for PowerPoint decks. Data is loaded from a local `data.json` file with no database, no authentication, and no cloud dependencies. The architecture is intentionally minimal: one Blazor component page, a JSON deserialization service, and inline SVG rendering for the timeline. This is not an enterprise app — it's a polished visual reporting tool that runs on `localhost`. **Primary Recommendation:** Use a single Blazor Server project with `System.Text.Json` for data loading, pure CSS Grid/Flexbox for layout (matching the reference HTML exactly), and inline SVG generated via Razor markup for the timeline. No charting library is needed — the design is simple enough to render with hand-crafted SVG and CSS, which gives pixel-perfect control for screenshot fidelity. --- | Package | Version | Purpose | License | Required? | |---------|---------|---------|---------|-----------| | `Microsoft.AspNetCore.Components` | 8.0.x | Blazor Server framework | MIT | Yes (implicit via SDK) | | `System.Text.Json` | 8.0.x | JSON deserialization | MIT | Yes (implicit via SDK) | | `bunit` | 1.31.x | Component testing | MIT | Optional (testing) | | `xunit` | 2.9.x | Test framework | Apache-2.0 | Optional (testing) | | `Verify.Bunit` | 26.x | Snapshot testing | MIT | Optional (testing) | **Total external NuGet dependencies for production: 0.** Everything needed ships with the .NET 8 SDK.

### Key Findings

- The reference design (`OriginalDesignConcept.html`) uses a fixed 1920×1080 layout optimized for screenshots, not responsive design. The Blazor implementation should preserve this fixed-dimension approach.
- The timeline section uses inline SVG with simple geometric primitives (lines, circles, diamonds/polygons, text labels). This is trivially reproducible in Razor markup without any charting library.
- The heatmap grid is pure CSS Grid (`grid-template-columns: 160px repeat(4, 1fr)`) with color-coded cells. No JavaScript or complex interactivity is needed.
- `data.json` as the sole data source eliminates all database, ORM, and migration complexity. `System.Text.Json` (built into .NET 8) handles deserialization with zero additional dependencies.
- Blazor Server's SignalR circuit is irrelevant for this use case (no real-time updates needed), but it's the mandated stack and works fine for local single-user scenarios.
- The color palette, typography (Segoe UI), and spacing from the HTML reference must be replicated exactly in the Blazor app's CSS — this is the primary fidelity requirement.
- No third-party NuGet packages are strictly required. The entire app can be built with the .NET 8 SDK alone.
- The design includes a legend (PoC Milestone, Production Release, Checkpoint, Now marker) that must be data-driven from `data.json` to allow reuse across projects.
- Screenshot quality at 1920×1080 is the key acceptance criterion — the page must look identical to the reference design when captured via browser screenshot tools. --- **Goal:** Pixel-accurate reproduction of the reference design with data from `data.json`.
- **Scaffold the project** (15 min)
- `dotnet new blazor --interactivity Server -n ReportingDashboard`
- Create `.sln` file, set up folder structure
- Strip out default template pages (Weather, Counter)
- **Define data models + service** (30 min)
- Create C# `record` types matching the JSON schema above
- Implement `DashboardDataService` with `System.Text.Json` deserialization
- Create `data.json` with fictional project data
- **Build the Header component** (30 min)
- Title, subtitle, backlog link, legend icons
- Copy CSS directly from reference HTML
- **Build the Timeline component** (1.5 hours)
- Left panel with milestone labels
- SVG viewport with month gridlines, "NOW" marker
- Milestone event rendering (circles, diamonds, connecting lines)
- Date-to-X coordinate calculation logic
- **Build the Heatmap Grid component** (1.5 hours)
- CSS Grid layout with dynamic column count
- Row headers with category-specific colors
- Cell rendering with bulleted item lists
- Current-month highlighting
- **CSS polish and screenshot verification** (1 hour)
- Compare side-by-side with reference design at 1920×1080
- Adjust spacing, font sizes, colors to match exactly
- Test screenshot capture workflow
- Multi-project file selector dropdown
- Print-friendly CSS (`@media print`)
- Subtle hover tooltips on timeline milestones (shows full date/description)
- Animated "NOW" line pulse effect
- Dark mode variant for variety in presentations
- **Copy the reference CSS verbatim** as the starting point for `dashboard.css`. The reference HTML's `<style>` block is production-ready CSS — don't rewrite it, adapt it.
- **Use `dotnet watch`** from the start for rapid iteration on CSS and layout.
- **Create `data.json` first** before writing any C# code. Validate the JSON schema, then build models to match. This avoids schema-code mismatches.
- **Test at 1920×1080 continuously.** Use Chrome DevTools → Device Toolbar → set to 1920×1080 and verify after every component addition. No prototyping needed. The reference HTML *is* the prototype. The Blazor implementation is a direct translation from static HTML to data-driven Razor components. The risk is low and the path is clear.
```json
{
  "title": "Phoenix Platform Release Roadmap",
  "subtitle": "Engineering Excellence · Phoenix Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/contoso/phoenix/_backlogs",
  "currentDate": "2026-04-10",
  "timelineStartMonth": "2026-01",
  "timelineEndMonth": "2026-06",
  "milestones": [
    {
      "id": "M1",
      "label": "API Gateway & Auth",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-15", "type": "checkpoint", "label": "Design Review" },
        { "date": "2026-02-28", "type": "poc", "label": "Feb 28 PoC" },
        { "date": "2026-04-15", "type": "production", "label": "Apr GA" }
      ]
    },
    {
      "id": "M2",
      "label": "Data Pipeline v2",
      "color": "#00897B",
      "events": [
        { "date": "2025-12-19", "type": "checkpoint", "label": "Kickoff" },
        { "date": "2026-02-11", "type": "checkpoint", "label": "Schema Lock" },
        { "date": "2026-03-20", "type": "poc", "label": "Mar 20 PoC" },
        { "date": "2026-05-15", "type": "production", "label": "May Prod" }
      ]
    },
    {
      "id": "M3",
      "label": "Dashboard & Reporting",
      "color": "#546E7A",
      "events": [
        { "date": "2026-03-01", "type": "checkpoint", "label": "UX Approved" },
        { "date": "2026-04-30", "type": "poc", "label": "Apr 30 PoC" },
        { "date": "2026-06-15", "type": "production", "label": "Jun Prod" }
      ]
    }
  ],
  "heatmapMonths": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["API Gateway scaffold", "Auth token service"],
        "Feb": ["Rate limiting middleware", "Pipeline schema v2"],
        "Mar": ["Batch ingestion engine", "Monitoring dashboards", "Load test framework"],
        "Apr": ["Gateway GA release", "SSO integration"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": {
        "Jan": ["Token refresh flow"],
        "Feb": ["CDC connector", "Schema migration tool"],
        "Mar": ["Real-time streaming PoC"],
        "Apr": ["Dashboard UX build", "Pipeline perf tuning", "E2E test suite"]
      }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": {
        "Jan": [],
        "Feb": ["Retry policy config"],
        "Mar": ["Retry policy config", "Legacy API deprecation"],
        "Apr": ["Legacy API deprecation"]
      }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": {
        "Jan": [],
        "Feb": [],
        "Mar": ["Vendor SDK delay"],
        "Apr": ["Vendor SDK delay", "Infra quota approval"]
      }
    }
  ]
}
``` ---

### Recommended Tools & Technologies

- **Project:** Executive Reporting Dashboard — Single-Page Milestone & Progress View **Stack:** C# .NET 8 / Blazor Server / Local-only / .sln structure **Date:** April 16, 2026 **Author:** Technical Research Team --- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Mandated stack. Use `dotnet new blazor --interactivity Server` template | | **CSS Layout** | CSS Grid + Flexbox | Native CSS3 | Matches reference HTML exactly. No CSS framework needed | | **Timeline Rendering** | Inline SVG via Razor | N/A | `<svg>` elements generated directly in `.razor` files. No JS charting library | | **Font** | Segoe UI | System font | Available on Windows by default. Fallback: `Arial, sans-serif` | | **Icons/Shapes** | SVG primitives | N/A | Diamonds via `<polygon>`, circles via `<circle>`, lines via `<line>` | **Why no charting library?** Libraries like BlazorApexCharts, Radzen Charts, or ChartJs.Blazor add complexity and fight you on pixel-perfect control. The reference design uses ~15 SVG elements total. Hand-coded SVG in Razor gives exact control over every coordinate, color, and label position — critical for screenshot fidelity. **Alternative considered:** `Blazorise Charts` (v1.6+) — mature, MIT licensed, wraps Chart.js. Rejected because the timeline is not a standard chart type (it's a custom Gantt-like visualization) and the overhead isn't justified for this scope. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Zero-dependency. Use `JsonSerializer.Deserialize<T>()` with source generators for AOT compatibility | | **File I/O** | `System.IO.File.ReadAllTextAsync` | Built into .NET 8 | Read `data.json` from `wwwroot/` or app root | | **Data Models** | C# records | .NET 8 | Immutable POCOs. Use `record` types for clean JSON mapping | | **DI Registration** | `IServiceCollection` | Built into .NET 8 | Register a `DashboardDataService` as Singleton (data rarely changes) | **Why not a database?** The user explicitly wants JSON file-based data. A SQLite or LiteDB layer would add migration headaches, schema management, and deployment complexity for zero benefit. The data set is small (dozens of items at most). | Approach | Technology | Notes | |----------|-----------|-------| | **Primary** | `data.json` flat file | Stored in project root or `wwwroot/data/`. Edited by hand or tooling | | **Format** | JSON | Human-readable, easy to version control, trivial to deserialize | | **Backup** | Git | The JSON file lives in the repo alongside the code | | Layer | Technology | Notes | |-------|-----------|-------| | **Runtime** | .NET 8 SDK + Kestrel | Local-only. `dotnet run` serves the app on `https://localhost:5001` | | **Hosting** | Kestrel (built-in) | No IIS, no Nginx, no Docker needed | | **Screenshot Tool** | Browser DevTools or Snipping Tool | Capture at 1920×1080 for PowerPoint | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Tests** | xUnit | 2.9.x | .NET ecosystem standard. Test data service and model deserialization | | **Component Tests** | bUnit | 1.31.x+ | Blazor-specific component testing. Test that components render correct SVG/HTML from test data | | **Snapshot Tests** | Verify | 26.x | Optional. Compare rendered HTML output against golden snapshots | | Tool | Version | Notes | |------|---------|-------| | **IDE** | Visual Studio 2022 17.9+ or VS Code + C# Dev Kit | Full Blazor tooling support | | **Hot Reload** | Built into `dotnet watch` | `dotnet watch run` for rapid CSS/Razor iteration | | **CSS Isolation** | Blazor CSS isolation (`.razor.css`) | Scoped styles per component. Avoids global CSS conflicts | ---
```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Models/
│       │   └── DashboardData.cs          # C# records matching data.json schema
│       ├── Services/
│       │   └── DashboardDataService.cs   # Reads and deserializes data.json
│       ├── Components/
│       │   ├── App.razor
│       │   ├── Pages/
│       │   │   └── Dashboard.razor       # Main single-page layout
│       │   └── Shared/
│       │       ├── Header.razor          # Title, subtitle, legend
│       │       ├── Timeline.razor        # SVG milestone timeline
│       │       ├── HeatmapGrid.razor     # CSS Grid status matrix
│       │       └── HeatmapCell.razor     # Individual cell with item bullets
│       └── wwwroot/
│           ├── css/
│           │   └── dashboard.css         # Global styles matching reference
│           └── data/
│               └── data.json             # Project data
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── ...
```
```
data.json → DashboardDataService (Singleton) → Dashboard.razor → Child Components
```
- `DashboardDataService` reads `data.json` on first request (lazy load, cached in memory)
- `Dashboard.razor` injects the service, calls `GetDashboardDataAsync()`
- Data is passed to child components via `[Parameter]` properties
- Components render HTML/SVG based on the data
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-10",
  "timelineStartMonth": "2026-01",
  "timelineEndMonth": "2026-06",
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
  "heatmapMonths": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Item A", "Item B"],
        "Feb": ["Item C"],
        "Mar": ["Item D", "Item E", "Item F"],
        "Apr": ["Item G"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": { ... }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": { ... }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": { ... }
    }
  ]
}
``` | Design Section | Blazor Component | Rendering Approach | |---------------|-----------------|-------------------| | Header bar (title + legend) | `Header.razor` | Standard HTML/CSS. Flexbox layout with `justify-content: space-between` | | Timeline area (left labels + SVG) | `Timeline.razor` | Left panel: HTML divs. Right panel: inline `<svg>` with computed coordinates | | Heatmap grid | `HeatmapGrid.razor` | CSS Grid container. Iterates `categories` and `heatmapMonths` to render cells | | Individual heatmap cell | `HeatmapCell.razor` | Renders bullet list with colored dot pseudo-elements | The timeline SVG maps dates to X coordinates within a fixed-width viewport:
```csharp
private double DateToX(DateOnly date)
{
    var totalDays = (timelineEnd - timelineStart).Days;
    var elapsed = (date - timelineStart).Days;
    return (elapsed / (double)totalDays) * svgWidth;
}
``` Each milestone row gets a fixed Y offset (e.g., row 0 = y:42, row 1 = y:98, row 2 = y:154). Diamond markers use `<polygon>` with calculated center points. The "NOW" line is a dashed vertical line at the current date's X position. **Recommendation:** Use a single `dashboard.css` file in `wwwroot/css/` rather than Blazor CSS isolation. Rationale:
- The reference HTML uses a flat CSS structure — translating it directly to one CSS file is fastest
- CSS isolation adds `.b-xxxxx` attribute selectors that complicate debugging
- For a single-page app with ~100 lines of CSS, isolation provides no organizational benefit
- The CSS from the reference HTML can be copied almost verbatim
- `body { width: 1920px; height: 1080px; overflow: hidden; }` — fixed viewport for screenshot fidelity
- `.hm-grid { grid-template-columns: 160px repeat(4, 1fr); }` — dynamic column count based on `heatmapMonths.length`
- Color-coded row classes: `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` with matching backgrounds and bullet colors
- `.hm-cell .it::before` pseudo-element for the colored bullet dots **Dynamic column count:** The grid's column template should be set via a `style` attribute in Razor since the number of months comes from `data.json`:
```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Data.HeatmapMonths.Count, 1fr);">
``` ---

### Considerations & Risks

- **Recommendation: None.** This is explicitly a local-only tool with no auth requirements. Do not add Identity, cookie auth, or any middleware. The app runs on `localhost` and is accessed only by the person running it. If future sharing is needed (e.g., running on a team server), the simplest upgrade path is:
- Add Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (one line in `Program.cs`)
- This uses existing AD credentials with zero user management
- `data.json` contains project metadata, not PII or secrets. No encryption needed.
- If sensitive project names are a concern, keep `data.json` out of version control (add to `.gitignore`) and distribute separately. | Scenario | Approach | Command | |----------|----------|---------| | **Local dev** | `dotnet run` | `dotnet run --project src/ReportingDashboard` | | **Local with hot reload** | `dotnet watch` | `dotnet watch run --project src/ReportingDashboard` | | **Share with team** | Self-contained publish | `dotnet publish -c Release -r win-x64 --self-contained` | | **Portable** | Single-file publish | `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` | The self-contained single-file publish produces one `.exe` (~80MB) that anyone can run without installing .NET. Place `data.json` next to the executable and double-click to launch. **$0.** This runs locally on developer hardware. No cloud, no hosting, no licenses beyond what's already available (Visual Studio, .NET SDK). --- | Risk | Likelihood | Impact | Mitigation | |------|-----------|--------|-----------| | **SVG coordinate math errors** | Medium | Medium | Build the timeline component incrementally. Test with known dates and visually verify against the reference HTML at each step | | **CSS Grid rendering differences across browsers** | Low | Medium | Target Chromium-based browsers (Edge/Chrome) for screenshots. Test at exactly 1920×1080 | | **Blazor Server overhead for a static page** | Low | Low | Negligible for single-user local use. The SignalR circuit adds ~50KB overhead but doesn't affect screenshot quality | | **data.json schema drift** | Medium | Low | Define C# record types with `[JsonPropertyName]` attributes. Add a validation step in `DashboardDataService` that logs warnings for missing fields | | **Font availability (Segoe UI)** | Low | Medium | Segoe UI is pre-installed on all Windows machines. For non-Windows screenshot capture, bundle the font or use Arial fallback | | Decision | Trade-off | Reasoning | |----------|----------|-----------| | No charting library | Must hand-code SVG coordinates | Full pixel control outweighs development speed for a small, fixed design | | No database | Must edit JSON manually | Appropriate for infrequent updates; JSON is human-readable and version-controllable | | Fixed 1920×1080 layout | Not responsive on mobile | Explicitly optimized for screenshot capture, not general web browsing | | Blazor Server (not Static SSR) | SignalR circuit overhead | Mandated stack. Overhead is negligible for local use. Enables future interactivity if needed | | Single CSS file | No style encapsulation | Simpler debugging for ~100 lines of CSS. Acceptable for a single-page app | Not applicable. This is a single-user, local-only, read-only dashboard. The "scalability" dimension is how many different projects can be reported on — solved by having multiple `data.json` files and a file selector, not by horizontal scaling. ---
- **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json` (e.g., 3, 6, or 12 months)? Recommendation: Make it dynamic based on `heatmapMonths` array length, with the CSS grid adapting automatically.
- **Should the "current month" highlighting (amber column headers, darker cell backgrounds) be automatic or configured?** Recommendation: Derive from `currentDate` in `data.json` — highlight the column matching that month.
- **Multiple project support?** Should the app support switching between different `data.json` files (e.g., `project-a.json`, `project-b.json`) via a dropdown? Or is one file sufficient? Recommendation: Start with one file. Add a file selector in Phase 2 if needed.
- **ADO backlog link behavior?** The reference includes a "→ ADO Backlog" link in the header. Should this be a live link to Azure DevOps, or just a visual element for the screenshot? Recommendation: Make it a configurable URL in `data.json`. It works as a link when viewed in-browser but is static in screenshots.
- **Screenshot capture workflow?** Should the app include a built-in screenshot/export button (e.g., render to PNG server-side), or will the user use browser/OS screenshot tools? Recommendation: Use browser DevTools device emulation at 1920×1080 + screenshot. No built-in export needed for MVP.
- **Timeline date range?** The reference shows Jan–Jun. Should the timeline always show 6 months, or should it adapt to the data? Recommendation: Configure `timelineStartMonth` and `timelineEndMonth` in `data.json`. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Reporting Dashboard — Single-Page Milestone & Progress View  
**Stack:** C# .NET 8 / Blazor Server / Local-only / .sln structure  
**Date:** April 16, 2026  
**Author:** Technical Research Team

---

## 1. Executive Summary

This project is a lightweight, single-page executive reporting dashboard built with **Blazor Server on .NET 8**. It renders a timeline/milestone view and a color-coded heatmap grid showing project status (Shipped, In Progress, Carryover, Blockers) by month — designed to be screenshot-friendly for PowerPoint decks. Data is loaded from a local `data.json` file with no database, no authentication, and no cloud dependencies. The architecture is intentionally minimal: one Blazor component page, a JSON deserialization service, and inline SVG rendering for the timeline. This is not an enterprise app — it's a polished visual reporting tool that runs on `localhost`.

**Primary Recommendation:** Use a single Blazor Server project with `System.Text.Json` for data loading, pure CSS Grid/Flexbox for layout (matching the reference HTML exactly), and inline SVG generated via Razor markup for the timeline. No charting library is needed — the design is simple enough to render with hand-crafted SVG and CSS, which gives pixel-perfect control for screenshot fidelity.

---

## 2. Key Findings

- The reference design (`OriginalDesignConcept.html`) uses a fixed 1920×1080 layout optimized for screenshots, not responsive design. The Blazor implementation should preserve this fixed-dimension approach.
- The timeline section uses inline SVG with simple geometric primitives (lines, circles, diamonds/polygons, text labels). This is trivially reproducible in Razor markup without any charting library.
- The heatmap grid is pure CSS Grid (`grid-template-columns: 160px repeat(4, 1fr)`) with color-coded cells. No JavaScript or complex interactivity is needed.
- `data.json` as the sole data source eliminates all database, ORM, and migration complexity. `System.Text.Json` (built into .NET 8) handles deserialization with zero additional dependencies.
- Blazor Server's SignalR circuit is irrelevant for this use case (no real-time updates needed), but it's the mandated stack and works fine for local single-user scenarios.
- The color palette, typography (Segoe UI), and spacing from the HTML reference must be replicated exactly in the Blazor app's CSS — this is the primary fidelity requirement.
- No third-party NuGet packages are strictly required. The entire app can be built with the .NET 8 SDK alone.
- The design includes a legend (PoC Milestone, Production Release, Checkpoint, Now marker) that must be data-driven from `data.json` to allow reuse across projects.
- Screenshot quality at 1920×1080 is the key acceptance criterion — the page must look identical to the reference design when captured via browser screenshot tools.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server UI)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Mandated stack. Use `dotnet new blazor --interactivity Server` template |
| **CSS Layout** | CSS Grid + Flexbox | Native CSS3 | Matches reference HTML exactly. No CSS framework needed |
| **Timeline Rendering** | Inline SVG via Razor | N/A | `<svg>` elements generated directly in `.razor` files. No JS charting library |
| **Font** | Segoe UI | System font | Available on Windows by default. Fallback: `Arial, sans-serif` |
| **Icons/Shapes** | SVG primitives | N/A | Diamonds via `<polygon>`, circles via `<circle>`, lines via `<line>` |

**Why no charting library?** Libraries like BlazorApexCharts, Radzen Charts, or ChartJs.Blazor add complexity and fight you on pixel-perfect control. The reference design uses ~15 SVG elements total. Hand-coded SVG in Razor gives exact control over every coordinate, color, and label position — critical for screenshot fidelity.

**Alternative considered:** `Blazorise Charts` (v1.6+) — mature, MIT licensed, wraps Chart.js. Rejected because the timeline is not a standard chart type (it's a custom Gantt-like visualization) and the overhead isn't justified for this scope.

### Backend (Data Layer)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Zero-dependency. Use `JsonSerializer.Deserialize<T>()` with source generators for AOT compatibility |
| **File I/O** | `System.IO.File.ReadAllTextAsync` | Built into .NET 8 | Read `data.json` from `wwwroot/` or app root |
| **Data Models** | C# records | .NET 8 | Immutable POCOs. Use `record` types for clean JSON mapping |
| **DI Registration** | `IServiceCollection` | Built into .NET 8 | Register a `DashboardDataService` as Singleton (data rarely changes) |

**Why not a database?** The user explicitly wants JSON file-based data. A SQLite or LiteDB layer would add migration headaches, schema management, and deployment complexity for zero benefit. The data set is small (dozens of items at most).

### Data Storage

| Approach | Technology | Notes |
|----------|-----------|-------|
| **Primary** | `data.json` flat file | Stored in project root or `wwwroot/data/`. Edited by hand or tooling |
| **Format** | JSON | Human-readable, easy to version control, trivial to deserialize |
| **Backup** | Git | The JSON file lives in the repo alongside the code |

### Infrastructure

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Runtime** | .NET 8 SDK + Kestrel | Local-only. `dotnet run` serves the app on `https://localhost:5001` |
| **Hosting** | Kestrel (built-in) | No IIS, no Nginx, no Docker needed |
| **Screenshot Tool** | Browser DevTools or Snipping Tool | Capture at 1920×1080 for PowerPoint |

### Testing

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Tests** | xUnit | 2.9.x | .NET ecosystem standard. Test data service and model deserialization |
| **Component Tests** | bUnit | 1.31.x+ | Blazor-specific component testing. Test that components render correct SVG/HTML from test data |
| **Snapshot Tests** | Verify | 26.x | Optional. Compare rendered HTML output against golden snapshots |

### Development Tooling

| Tool | Version | Notes |
|------|---------|-------|
| **IDE** | Visual Studio 2022 17.9+ or VS Code + C# Dev Kit | Full Blazor tooling support |
| **Hot Reload** | Built into `dotnet watch` | `dotnet watch run` for rapid CSS/Razor iteration |
| **CSS Isolation** | Blazor CSS isolation (`.razor.css`) | Scoped styles per component. Avoids global CSS conflicts |

---

## 4. Architecture Recommendations

### Overall Architecture: Single-Project, Component-Based

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Models/
│       │   └── DashboardData.cs          # C# records matching data.json schema
│       ├── Services/
│       │   └── DashboardDataService.cs   # Reads and deserializes data.json
│       ├── Components/
│       │   ├── App.razor
│       │   ├── Pages/
│       │   │   └── Dashboard.razor       # Main single-page layout
│       │   └── Shared/
│       │       ├── Header.razor          # Title, subtitle, legend
│       │       ├── Timeline.razor        # SVG milestone timeline
│       │       ├── HeatmapGrid.razor     # CSS Grid status matrix
│       │       └── HeatmapCell.razor     # Individual cell with item bullets
│       └── wwwroot/
│           ├── css/
│           │   └── dashboard.css         # Global styles matching reference
│           └── data/
│               └── data.json             # Project data
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── ...
```

### Data Flow

```
data.json → DashboardDataService (Singleton) → Dashboard.razor → Child Components
```

1. `DashboardDataService` reads `data.json` on first request (lazy load, cached in memory)
2. `Dashboard.razor` injects the service, calls `GetDashboardDataAsync()`
3. Data is passed to child components via `[Parameter]` properties
4. Components render HTML/SVG based on the data

### Data Model Design (`data.json` schema)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-10",
  "timelineStartMonth": "2026-01",
  "timelineEndMonth": "2026-06",
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
  "heatmapMonths": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Item A", "Item B"],
        "Feb": ["Item C"],
        "Mar": ["Item D", "Item E", "Item F"],
        "Apr": ["Item G"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": { ... }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": { ... }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": { ... }
    }
  ]
}
```

### Component Architecture Mapping to Design

| Design Section | Blazor Component | Rendering Approach |
|---------------|-----------------|-------------------|
| Header bar (title + legend) | `Header.razor` | Standard HTML/CSS. Flexbox layout with `justify-content: space-between` |
| Timeline area (left labels + SVG) | `Timeline.razor` | Left panel: HTML divs. Right panel: inline `<svg>` with computed coordinates |
| Heatmap grid | `HeatmapGrid.razor` | CSS Grid container. Iterates `categories` and `heatmapMonths` to render cells |
| Individual heatmap cell | `HeatmapCell.razor` | Renders bullet list with colored dot pseudo-elements |

### SVG Timeline Coordinate Calculation

The timeline SVG maps dates to X coordinates within a fixed-width viewport:

```csharp
private double DateToX(DateOnly date)
{
    var totalDays = (timelineEnd - timelineStart).Days;
    var elapsed = (date - timelineStart).Days;
    return (elapsed / (double)totalDays) * svgWidth;
}
```

Each milestone row gets a fixed Y offset (e.g., row 0 = y:42, row 1 = y:98, row 2 = y:154). Diamond markers use `<polygon>` with calculated center points. The "NOW" line is a dashed vertical line at the current date's X position.

### CSS Strategy

**Recommendation:** Use a single `dashboard.css` file in `wwwroot/css/` rather than Blazor CSS isolation. Rationale:

1. The reference HTML uses a flat CSS structure — translating it directly to one CSS file is fastest
2. CSS isolation adds `.b-xxxxx` attribute selectors that complicate debugging
3. For a single-page app with ~100 lines of CSS, isolation provides no organizational benefit
4. The CSS from the reference HTML can be copied almost verbatim

**Key CSS patterns to preserve from the reference:**

- `body { width: 1920px; height: 1080px; overflow: hidden; }` — fixed viewport for screenshot fidelity
- `.hm-grid { grid-template-columns: 160px repeat(4, 1fr); }` — dynamic column count based on `heatmapMonths.length`
- Color-coded row classes: `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` with matching backgrounds and bullet colors
- `.hm-cell .it::before` pseudo-element for the colored bullet dots

**Dynamic column count:** The grid's column template should be set via a `style` attribute in Razor since the number of months comes from `data.json`:

```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Data.HeatmapMonths.Count, 1fr);">
```

---

## 5. Security & Infrastructure

### Authentication & Authorization

**Recommendation: None.** This is explicitly a local-only tool with no auth requirements. Do not add Identity, cookie auth, or any middleware. The app runs on `localhost` and is accessed only by the person running it.

If future sharing is needed (e.g., running on a team server), the simplest upgrade path is:
- Add Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (one line in `Program.cs`)
- This uses existing AD credentials with zero user management

### Data Protection

- `data.json` contains project metadata, not PII or secrets. No encryption needed.
- If sensitive project names are a concern, keep `data.json` out of version control (add to `.gitignore`) and distribute separately.

### Hosting & Deployment

| Scenario | Approach | Command |
|----------|----------|---------|
| **Local dev** | `dotnet run` | `dotnet run --project src/ReportingDashboard` |
| **Local with hot reload** | `dotnet watch` | `dotnet watch run --project src/ReportingDashboard` |
| **Share with team** | Self-contained publish | `dotnet publish -c Release -r win-x64 --self-contained` |
| **Portable** | Single-file publish | `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` |

The self-contained single-file publish produces one `.exe` (~80MB) that anyone can run without installing .NET. Place `data.json` next to the executable and double-click to launch.

### Infrastructure Costs

**$0.** This runs locally on developer hardware. No cloud, no hosting, no licenses beyond what's already available (Visual Studio, .NET SDK).

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|-----------|
| **SVG coordinate math errors** | Medium | Medium | Build the timeline component incrementally. Test with known dates and visually verify against the reference HTML at each step |
| **CSS Grid rendering differences across browsers** | Low | Medium | Target Chromium-based browsers (Edge/Chrome) for screenshots. Test at exactly 1920×1080 |
| **Blazor Server overhead for a static page** | Low | Low | Negligible for single-user local use. The SignalR circuit adds ~50KB overhead but doesn't affect screenshot quality |
| **data.json schema drift** | Medium | Low | Define C# record types with `[JsonPropertyName]` attributes. Add a validation step in `DashboardDataService` that logs warnings for missing fields |
| **Font availability (Segoe UI)** | Low | Medium | Segoe UI is pre-installed on all Windows machines. For non-Windows screenshot capture, bundle the font or use Arial fallback |

### Trade-offs Made

| Decision | Trade-off | Reasoning |
|----------|----------|-----------|
| No charting library | Must hand-code SVG coordinates | Full pixel control outweighs development speed for a small, fixed design |
| No database | Must edit JSON manually | Appropriate for infrequent updates; JSON is human-readable and version-controllable |
| Fixed 1920×1080 layout | Not responsive on mobile | Explicitly optimized for screenshot capture, not general web browsing |
| Blazor Server (not Static SSR) | SignalR circuit overhead | Mandated stack. Overhead is negligible for local use. Enables future interactivity if needed |
| Single CSS file | No style encapsulation | Simpler debugging for ~100 lines of CSS. Acceptable for a single-page app |

### Scalability Bottlenecks

Not applicable. This is a single-user, local-only, read-only dashboard. The "scalability" dimension is how many different projects can be reported on — solved by having multiple `data.json` files and a file selector, not by horizontal scaling.

---

## 7. Open Questions

1. **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json` (e.g., 3, 6, or 12 months)? Recommendation: Make it dynamic based on `heatmapMonths` array length, with the CSS grid adapting automatically.

2. **Should the "current month" highlighting (amber column headers, darker cell backgrounds) be automatic or configured?** Recommendation: Derive from `currentDate` in `data.json` — highlight the column matching that month.

3. **Multiple project support?** Should the app support switching between different `data.json` files (e.g., `project-a.json`, `project-b.json`) via a dropdown? Or is one file sufficient? Recommendation: Start with one file. Add a file selector in Phase 2 if needed.

4. **ADO backlog link behavior?** The reference includes a "→ ADO Backlog" link in the header. Should this be a live link to Azure DevOps, or just a visual element for the screenshot? Recommendation: Make it a configurable URL in `data.json`. It works as a link when viewed in-browser but is static in screenshots.

5. **Screenshot capture workflow?** Should the app include a built-in screenshot/export button (e.g., render to PNG server-side), or will the user use browser/OS screenshot tools? Recommendation: Use browser DevTools device emulation at 1920×1080 + screenshot. No built-in export needed for MVP.

6. **Timeline date range?** The reference shows Jan–Jun. Should the timeline always show 6 months, or should it adapt to the data? Recommendation: Configure `timelineStartMonth` and `timelineEndMonth` in `data.json`.

---

## 8. Implementation Recommendations

### Phased Approach

#### Phase 1: Core Dashboard (MVP) — ~4-6 hours of development

**Goal:** Pixel-accurate reproduction of the reference design with data from `data.json`.

1. **Scaffold the project** (15 min)
   - `dotnet new blazor --interactivity Server -n ReportingDashboard`
   - Create `.sln` file, set up folder structure
   - Strip out default template pages (Weather, Counter)

2. **Define data models + service** (30 min)
   - Create C# `record` types matching the JSON schema above
   - Implement `DashboardDataService` with `System.Text.Json` deserialization
   - Create `data.json` with fictional project data

3. **Build the Header component** (30 min)
   - Title, subtitle, backlog link, legend icons
   - Copy CSS directly from reference HTML

4. **Build the Timeline component** (1.5 hours)
   - Left panel with milestone labels
   - SVG viewport with month gridlines, "NOW" marker
   - Milestone event rendering (circles, diamonds, connecting lines)
   - Date-to-X coordinate calculation logic

5. **Build the Heatmap Grid component** (1.5 hours)
   - CSS Grid layout with dynamic column count
   - Row headers with category-specific colors
   - Cell rendering with bulleted item lists
   - Current-month highlighting

6. **CSS polish and screenshot verification** (1 hour)
   - Compare side-by-side with reference design at 1920×1080
   - Adjust spacing, font sizes, colors to match exactly
   - Test screenshot capture workflow

#### Phase 2: Enhancements (Optional) — ~2-3 hours

- Multi-project file selector dropdown
- Print-friendly CSS (`@media print`)
- Subtle hover tooltips on timeline milestones (shows full date/description)
- Animated "NOW" line pulse effect
- Dark mode variant for variety in presentations

### Quick Wins

1. **Copy the reference CSS verbatim** as the starting point for `dashboard.css`. The reference HTML's `<style>` block is production-ready CSS — don't rewrite it, adapt it.
2. **Use `dotnet watch`** from the start for rapid iteration on CSS and layout.
3. **Create `data.json` first** before writing any C# code. Validate the JSON schema, then build models to match. This avoids schema-code mismatches.
4. **Test at 1920×1080 continuously.** Use Chrome DevTools → Device Toolbar → set to 1920×1080 and verify after every component addition.

### Prototyping Recommendation

No prototyping needed. The reference HTML *is* the prototype. The Blazor implementation is a direct translation from static HTML to data-driven Razor components. The risk is low and the path is clear.

### Recommended `data.json` Example (Fictional Project)

```json
{
  "title": "Phoenix Platform Release Roadmap",
  "subtitle": "Engineering Excellence · Phoenix Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/contoso/phoenix/_backlogs",
  "currentDate": "2026-04-10",
  "timelineStartMonth": "2026-01",
  "timelineEndMonth": "2026-06",
  "milestones": [
    {
      "id": "M1",
      "label": "API Gateway & Auth",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-15", "type": "checkpoint", "label": "Design Review" },
        { "date": "2026-02-28", "type": "poc", "label": "Feb 28 PoC" },
        { "date": "2026-04-15", "type": "production", "label": "Apr GA" }
      ]
    },
    {
      "id": "M2",
      "label": "Data Pipeline v2",
      "color": "#00897B",
      "events": [
        { "date": "2025-12-19", "type": "checkpoint", "label": "Kickoff" },
        { "date": "2026-02-11", "type": "checkpoint", "label": "Schema Lock" },
        { "date": "2026-03-20", "type": "poc", "label": "Mar 20 PoC" },
        { "date": "2026-05-15", "type": "production", "label": "May Prod" }
      ]
    },
    {
      "id": "M3",
      "label": "Dashboard & Reporting",
      "color": "#546E7A",
      "events": [
        { "date": "2026-03-01", "type": "checkpoint", "label": "UX Approved" },
        { "date": "2026-04-30", "type": "poc", "label": "Apr 30 PoC" },
        { "date": "2026-06-15", "type": "production", "label": "Jun Prod" }
      ]
    }
  ],
  "heatmapMonths": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["API Gateway scaffold", "Auth token service"],
        "Feb": ["Rate limiting middleware", "Pipeline schema v2"],
        "Mar": ["Batch ingestion engine", "Monitoring dashboards", "Load test framework"],
        "Apr": ["Gateway GA release", "SSO integration"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": {
        "Jan": ["Token refresh flow"],
        "Feb": ["CDC connector", "Schema migration tool"],
        "Mar": ["Real-time streaming PoC"],
        "Apr": ["Dashboard UX build", "Pipeline perf tuning", "E2E test suite"]
      }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": {
        "Jan": [],
        "Feb": ["Retry policy config"],
        "Mar": ["Retry policy config", "Legacy API deprecation"],
        "Apr": ["Legacy API deprecation"]
      }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": {
        "Jan": [],
        "Feb": [],
        "Mar": ["Vendor SDK delay"],
        "Apr": ["Vendor SDK delay", "Infra quota approval"]
      }
    }
  ]
}
```

---

## Appendix: NuGet Package Summary

| Package | Version | Purpose | License | Required? |
|---------|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.Components` | 8.0.x | Blazor Server framework | MIT | Yes (implicit via SDK) |
| `System.Text.Json` | 8.0.x | JSON deserialization | MIT | Yes (implicit via SDK) |
| `bunit` | 1.31.x | Component testing | MIT | Optional (testing) |
| `xunit` | 2.9.x | Test framework | Apache-2.0 | Optional (testing) |
| `Verify.Bunit` | 26.x | Snapshot testing | MIT | Optional (testing) |

**Total external NuGet dependencies for production: 0.** Everything needed ships with the .NET 8 SDK.

## Visual Design References

The following design reference files were found in the repository. These MUST be used as the canonical visual specification when building UI components.

### `docs/design-screenshots/OriginalDesignConcept.png`

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/9f3815bccf907793707f7aa4675c6589973934ad/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
