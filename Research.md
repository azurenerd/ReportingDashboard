# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-15 10:00 UTC_

### Summary

This project is a single-page executive reporting dashboard that visualizes project milestones, progress status, and delivery health. The design reference (`OriginalDesignConcept.html`) defines a fixed-resolution (1920×1080) layout with a timeline/Gantt visualization at the top and a color-coded heatmap grid below, optimized for PowerPoint screenshot capture. **Primary recommendation:** Build this as a minimal Blazor Server application with a single Razor page, reading all data from a local `data.json` file. Use pure CSS (Grid + Flexbox) for layout and inline SVG rendered via Blazor components for the timeline. No database, no authentication, no external services. The entire application should be under 10 files and deployable with a single `dotnet run`. This is intentionally a "sharp tool" — a focused utility, not a platform. ---

### Key Findings

- The design is a **pixel-perfect, screenshot-oriented** layout (1920×1080) — not a responsive web app. This simplifies implementation significantly: no breakpoints, no mobile, no fluid layouts needed.
- The timeline section uses **SVG** for milestone diamonds, checkpoint circles, progress lines, and a "NOW" marker. Blazor can render SVG natively in Razor components with no JS library needed.
- The heatmap grid is a **pure CSS Grid** layout (`160px repeat(4,1fr)`) with four category rows (Shipped, In Progress, Carryover, Blockers) × four month columns. Each cell contains bulleted items.
- **No charting library is needed.** The entire visualization is achievable with CSS Grid, Flexbox, and hand-crafted SVG — exactly as the HTML reference does it. Adding a charting library (e.g., Chart.js, Radzen charts) would add complexity with zero benefit.
- A `data.json` flat file is the correct data store. No database engine, no ORM, no migrations. `System.Text.Json` deserialization is all that's needed.
- Blazor Server's SignalR circuit is irrelevant for this use case (no interactivity beyond page load), but it's the mandated stack and works fine for local `dotnet run` scenarios.
- The Segoe UI font specified in the design is available on all Windows machines (the target environment). No web font loading needed.
- The color palette is fully defined in the HTML reference: green (#34A853/#E8F5E9) for shipped, blue (#0078D4/#E3F2FD) for in-progress, amber (#F4B400/#FFF8E1) for carryover, red (#EA4335/#FEF2F2) for blockers. --- **Goal:** Pixel-perfect reproduction of the reference HTML as a Blazor page with hardcoded data.
- `dotnet new blazorserver -n ReportingDashboard --framework net8.0`
- Strip the default template (remove Counter, Weather, NavMenu, etc.)
- Create `DashboardLayout.razor` — minimal layout with no sidebar/nav
- Port CSS from `OriginalDesignConcept.html` into `dashboard.css`
- Build `Header.razor`, `Timeline.razor`, `Heatmap.razor` with hardcoded HTML matching the reference
- Verify visual match with side-by-side screenshot comparison **Deliverable:** A running Blazor app that looks identical to the reference HTML. **Goal:** Replace hardcoded HTML with data-bound Blazor components reading from `data.json`.
- Define `DashboardData` model classes (records)
- Create sample `data.json` with fictional project data
- Build `DashboardDataService` to load and cache the JSON
- Refactor components to accept `[Parameter]` properties and render from data
- Implement SVG coordinate calculation for timeline positioning
- Verify that changing `data.json` values produces correct visual output **Deliverable:** A fully data-driven dashboard where editing `data.json` changes the display. **Goal:** Improve on the reference design for executive readability.
- Add summary statistics in the header (item counts per category)
- Improve typography: slightly larger heatmap text, better whitespace
- Highlight the current month column more prominently
- Add subtle CSS transitions for a polished feel
- Add `@media print` styles for direct browser-to-PDF if needed
- Create a well-documented sample `data.json` with comments explaining each field
- Write a `README.md` with setup instructions and screenshot workflow **Deliverable:** A polished, documented dashboard ready for production use.
- **Copy the reference CSS verbatim** as the starting point — don't rewrite it. This saves hours and guarantees visual fidelity.
- **Use C# records with `init` properties** for the data model — immutable, concise, and JSON-friendly.
- **Use `dotnet watch`** during development for instant feedback on Razor/CSS changes.
- **Screenshot comparison** is your test suite. Take a reference screenshot of the HTML file and diff it against the Blazor output using any image diff tool. | Temptation | Why to Resist | |------------|---------------| | Database (SQLite, EF Core) | JSON file is sufficient for monthly-updated data | | Authentication | No users, no roles, local-only | | REST API | No external consumers — Blazor reads data directly | | Component library (MudBlazor, Radzen) | Adds 500KB+ for zero UI benefit | | State management (Fluxor, Redux-style) | One page, one data load, no state to manage | | Docker/container | Local `dotnet run` is the deployment | | CI/CD pipeline | One developer, manual deployment | | JavaScript interop | SVG and CSS handle all visual needs | | Responsive breakpoints | Fixed 1920×1080 for screenshots | --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Blazor Server runtime | **Yes** (implicit) | | `System.Text.Json` | 8.0.x | JSON deserialization | **Yes** (built-in) | | `xunit` | 2.7+ | Unit testing | Optional | | `Microsoft.Playwright` | 1.41+ | Automated screenshots | Optional | **Total additional NuGet packages needed: 0** (for core functionality) Extracted from the design HTML for direct use in CSS:
```css
:root {
    /* Category: Shipped */
    --ship-text: #1B7A28;
    --ship-header-bg: #E8F5E9;
    --ship-cell-bg: #F0FBF0;
    --ship-cell-highlight: #D8F2DA;
    --ship-dot: #34A853;

    /* Category: In Progress */
    --prog-text: #1565C0;
    --prog-header-bg: #E3F2FD;
    --prog-cell-bg: #EEF4FE;
    --prog-cell-highlight: #DAE8FB;
    --prog-dot: #0078D4;

    /* Category: Carryover */
    --carry-text: #B45309;
    --carry-header-bg: #FFF8E1;
    --carry-cell-bg: #FFFDE7;
    --carry-cell-highlight: #FFF0B0;
    --carry-dot: #F4B400;

    /* Category: Blockers */
    --block-text: #991B1B;
    --block-header-bg: #FEF2F2;
    --block-cell-bg: #FFF5F5;
    --block-cell-highlight: #FFE4E4;
    --block-dot: #EA4335;

    /* Chrome */
    --bg: #FFFFFF;
    --text: #111;
    --link: #0078D4;
    --muted: #888;
    --border: #E0E0E0;
    --header-bg: #F5F5F5;
    --highlight-month-bg: #FFF0D0;
    --highlight-month-text: #C07700;
}
```
```json
{
  "title": "Project Phoenix Release Roadmap",
  "backlogLink": "https://dev.azure.com/org/project/_backlogs",
  "subtitle": "Platform Engineering • Phoenix Workstream • April 2026",
  "currentDate": "2026-04-15",
  "timelineRange": { "start": "2026-01-01", "end": "2026-06-30" },
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "highlightMonth": "Apr",
  "timelineTracks": [
    {
      "id": "m1",
      "name": "M1",
      "description": "Auth & Identity",
      "color": "#0078D4",
      "milestones": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "May Prod", "type": "production" }
      ]
    }
  ],
  "heatmap": {
    "categories": [
      {
        "name": "Shipped",
        "colorClass": "ship",
        "itemsByMonth": {
          "Jan": ["SSO Integration", "Token Refresh Flow"],
          "Feb": ["Role-Based Access"],
          "Mar": ["Audit Logging v1"],
          "Apr": ["Compliance Dashboard"]
        }
      }
    ]
  }
}
```

### Recommended Tools & Technologies

- **Project:** Executive Reporting Dashboard **Date:** April 15, 2026 **Stack:** C# .NET 8 / Blazor Server / Local-only / .sln structure --- | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Framework** | Blazor Server (.NET 8) | .NET 8.0.x (LTS) | Mandated stack. Use `dotnet new blazorserver` template. | | **CSS Strategy** | Scoped CSS + single `dashboard.css` | N/A | Use Blazor's built-in CSS isolation (`.razor.css` files). Mirror the design's existing CSS almost verbatim. | | **Layout Engine** | CSS Grid + Flexbox | N/A | Grid for the heatmap (`grid-template-columns: 160px repeat(4,1fr)`), Flexbox for header and timeline area. No CSS framework needed (no Bootstrap, no Tailwind). | | **SVG Rendering** | Inline SVG via Razor components | N/A | Render `<svg>` elements directly in `.razor` files. Use `@foreach` loops to emit milestone markers, timeline bars, and date labels from data. | | **Charting Library** | **None** | — | The design uses hand-drawn SVG, not chart abstractions. A charting library would fight the design rather than help it. | | **Component Library** | **None** (Radzen, MudBlazor, etc. not needed) | — | The UI is a single static-looking page. Component libraries add 500KB+ of CSS/JS for zero benefit here. | | **Icons** | Inline SVG shapes | N/A | The design uses simple geometric shapes (diamonds, circles, lines) — not an icon set. | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Runtime** | .NET 8 LTS | 8.0.x | Long-term support through November 2026. | | **JSON Deserialization** | `System.Text.Json` | Built-in | Ships with .NET 8. Use `JsonSerializer.Deserialize<T>()` with source generators for AOT-friendly deserialization. | | **Configuration** | `IConfiguration` + `data.json` | Built-in | Register `data.json` as an additional JSON configuration source, or simply read it as a file at startup. | | **File Watching** | `FileSystemWatcher` (optional) | Built-in | If you want the dashboard to auto-refresh when `data.json` is edited. Low priority — manual browser refresh is fine for v1. | | **Logging** | `Microsoft.Extensions.Logging` | Built-in | Console logger only. No Serilog, no Seq, no structured logging needed for this scope. | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Storage** | `data.json` flat file | N/A | Single JSON file in the project root or `wwwroot/data/` directory. | | **Schema** | Strongly-typed C# POCOs | N/A | `DashboardData`, `Milestone`, `HeatmapRow`, `HeatmapItem` record types. | | **Database** | **None** | — | A database is unjustified complexity. The data changes monthly (at most) and is hand-edited. | | **Caching** | `IMemoryCache` or singleton service | Built-in | Load `data.json` once at startup into a singleton. Optionally reload on file change. | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Unit Testing** | xUnit | 2.7+ | Standard .NET test framework. Test JSON deserialization and data model logic. | | **Blazor Component Testing** | bUnit | 1.25+ | If any component logic warrants testing. Likely overkill for this project. | | **Snapshot/Visual Testing** | Playwright for .NET | 1.41+ | Only if you want automated screenshot comparison. Manual screenshots are the stated workflow, so this is optional. | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **IDE** | Visual Studio 2022 or VS Code + C# Dev Kit | Latest | Standard .NET development. | | **Build** | `dotnet build` / `dotnet run` | Built-in | No custom build pipeline needed. | | **Hot Reload** | `dotnet watch` | Built-in | Blazor Server supports hot reload for Razor/CSS changes. | | **Screenshot Capture** | Browser DevTools or Playwright | N/A | Set viewport to 1920×1080, take full-page screenshot. | --- This is not a CRUD app. It's a read-only visualization of a JSON file. The architecture should reflect that simplicity.
```
data.json  →  DashboardDataService (singleton)  →  Razor Components  →  Browser
```
```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Models/
│       │   └── DashboardData.cs          # POCOs: DashboardData, Milestone, StatusCategory, StatusItem
│       ├── Services/
│       │   └── DashboardDataService.cs   # Reads and caches data.json
│       ├── Components/
│       │   ├── Pages/
│       │   │   └── Dashboard.razor       # Single page, route "/"
│       │   ├── Layout/
│       │   │   └── DashboardLayout.razor # Minimal layout (no nav, no sidebar)
│       │   ├── Header.razor              # Title, subtitle, legend
│       │   ├── Timeline.razor            # SVG timeline with milestones
│       │   └── Heatmap.razor             # CSS Grid status heatmap
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css         # Global styles matching design spec
│       │   └── data/
│       │       └── data.json             # Dashboard data file
│       └── appsettings.json
└── tests/
    └── ReportingDashboard.Tests/
        └── ReportingDashboard.Tests.csproj
```
```csharp
public record DashboardData
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogLink { get; init; } = "";
    public string CurrentMonth { get; init; } = "";    // e.g., "Apr 2026"
    public List<string> Months { get; init; } = [];     // Column headers: ["Jan","Feb","Mar","Apr"]
    public List<Milestone> Milestones { get; init; } = [];
    public List<TimelineTrack> TimelineTracks { get; init; } = [];
    public HeatmapData Heatmap { get; init; } = new();
}

public record Milestone
{
    public string Label { get; init; } = "";
    public string Date { get; init; } = "";         // ISO date string
    public string Type { get; init; } = "";          // "poc", "production", "checkpoint"
    public string TrackId { get; init; } = "";       // Which timeline track
}

public record TimelineTrack
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Color { get; init; } = "";         // Hex color for the track line
}

public record HeatmapData
{
    public List<StatusCategory> Categories { get; init; } = [];
}

public record StatusCategory
{
    public string Name { get; init; } = "";          // "Shipped", "In Progress", "Carryover", "Blockers"
    public string ColorClass { get; init; } = "";    // CSS class prefix: "ship", "prog", "carry", "block"
    public Dictionary<string, List<string>> ItemsByMonth { get; init; } = new();
}
``` The timeline is the most complex visual element. Recommended approach:
- **Define a date-to-pixel mapping function** based on the timeline's date range and SVG width (1560px in the reference).
- **Render month gridlines** with `<line>` and `<text>` elements in a `@foreach` loop.
- **Render the "NOW" marker** as a dashed red `<line>` positioned by current date.
- **Render each track** as a horizontal `<line>` with milestone markers (`<polygon>` for diamonds, `<circle>` for checkpoints).
- **All coordinates are calculated in C#** — no JavaScript needed.
```csharp
// In Timeline.razor
@code {
    private double DateToX(DateOnly date)
    {
        var totalDays = (EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
        var elapsed = (date.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
        return (elapsed / totalDays) * SvgWidth;
    }
}
``` **Do not use CSS-in-JS, Tailwind, or any CSS framework.** The reference HTML already defines the exact CSS needed. Port it directly:
- Copy the CSS from `OriginalDesignConcept.html` into `dashboard.css`.
- Adjust selectors to match Blazor component class names.
- Use Blazor CSS isolation (`.razor.css`) only if you want per-component scoping — but for a single-page app, a single global CSS file is simpler and more maintainable.
- Keep the fixed `1920px × 1080px` body dimensions for screenshot fidelity.
```
Startup:
  Program.cs registers DashboardDataService as singleton
  DashboardDataService reads wwwroot/data/data.json into DashboardData

Page Load:
  Dashboard.razor injects DashboardDataService
  Passes data down to Header, Timeline, Heatmap components via [Parameter]

No interactivity, no WebSocket state, no form submissions.
``` ---

### Considerations & Risks

- **None.** This is explicitly out of scope. The application runs locally on the developer's machine. No login page, no identity provider, no role-based access. The `Program.cs` should not include any auth middleware. If future access control is needed, the simplest approach would be a shared secret in `appsettings.json` checked via middleware — but do not build this now.
- `data.json` contains project names and status descriptions — not PII or secrets.
- No encryption needed for data at rest or in transit (localhost only).
- If the dashboard ever contains sensitive project details, ensure the machine's file permissions restrict access to `data.json`. | Aspect | Recommendation | |--------|---------------| | **Runtime** | `dotnet run` from command line or VS debug (F5) | | **Port** | Default Kestrel (https://localhost:5001 or http://localhost:5000) | | **Reverse Proxy** | None needed | | **Containerization** | Not needed for local-only use | | **Publishing** | `dotnet publish -c Release` produces a self-contained folder if sharing with teammates | Since the primary output is PowerPoint screenshots:
- Run `dotnet run`
- Open `https://localhost:5001` in Edge/Chrome
- Set browser zoom to 100%, window to 1920×1080 (or use DevTools device emulation)
- Take screenshot (Win+Shift+S, or DevTools capture)
- Paste into PowerPoint **Optional automation:** Add a Playwright script that captures the screenshot programmatically:
```csharp
// tools/Screenshot.csproj — standalone console app
using var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();
await page.SetViewportSizeAsync(1920, 1080);
await page.GotoAsync("https://localhost:5001");
await page.ScreenshotAsync(new() { Path = "dashboard.png" });
``` **$0.** This runs on the developer's existing workstation. No cloud resources, no licenses, no subscriptions. --- | Risk | Severity | Mitigation | |------|----------|------------| | **Blazor Server is overkill** for a static read-only page | Low | Accepted trade-off — it's the mandated stack. The overhead is minimal (~50MB memory for the SignalR circuit). A static HTML page would suffice, but Blazor provides component reuse and type-safe data binding. | | **SVG coordinate math errors** in timeline rendering | Medium | Write unit tests for the `DateToX()` mapping function. Verify against reference HTML coordinates. | | **CSS Grid inconsistencies** across browsers | Low | Target only Chromium-based browsers (Edge/Chrome) since screenshots are the deliverable. No need for Firefox/Safari testing. | | **data.json schema drift** over time | Medium | Define a strict C# model and validate on load. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` for flexibility but keep the schema documented. | | **Hot reload breaks SVG rendering** during development | Low | Known Blazor issue with complex SVG. Workaround: full page refresh (Ctrl+Shift+R). |
- **No real-time updates.** The dashboard shows a point-in-time snapshot. Editing `data.json` requires a page refresh. This is acceptable — the data changes weekly/monthly.
- **No responsive design.** Fixed 1920×1080 layout. Viewing on smaller screens will require scrolling. This is acceptable — the output is screenshots, not a web app.
- **No dark mode.** The design is light-themed for PowerPoint embedding on white/light slides.
- **SignalR circuit overhead.** Blazor Server maintains a WebSocket connection that's unnecessary for a read-only page. The memory/CPU impact is negligible for a single-user local app.
- **Over-engineering:** The biggest risk is adding unnecessary complexity (database, auth, component library, state management). Resist scope creep. This is a visualization of a JSON file.
- **Design fidelity:** The reference HTML has pixel-specific positioning. Getting the Blazor version to match exactly requires careful CSS porting. Diff screenshots side-by-side during development. ---
- **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always show the trailing 4 months?
- **How many timeline tracks?** The reference shows 3 tracks (M1, M2, M3). Should `data.json` support an arbitrary number, or cap at 3–5 for visual clarity?
- **Who maintains `data.json`?** Is this hand-edited by the project manager, or will a future integration populate it from ADO/Jira? This affects schema design (human-friendly vs. machine-friendly).
- **Should the "NOW" marker auto-calculate from system date, or be specified in `data.json`?** Auto-calculation is simpler but means the screenshot changes daily. A fixed date in `data.json` gives reproducible screenshots.
- **What is the timeline date range?** The reference shows Jan–Jun (6 months). Should this be configurable, or should it auto-calculate from milestone dates with padding?
- **Will there be multiple projects/dashboards?** If yes, `data.json` could become `data/{project-name}.json` with a route parameter. If it's always one project, keep it simple.
- **Desired improvements over the reference design:** The brief says "improve upon it." What specific improvements are desired? Suggestions:
- Better typography hierarchy
- Subtle animations on page load (CSS-only)
- Print-friendly `@media print` styles
- Percentage completion indicators per category
- Summary counts in the header (e.g., "12 shipped, 5 in progress, 3 blocked") ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Reporting Dashboard
**Date:** April 15, 2026
**Stack:** C# .NET 8 / Blazor Server / Local-only / .sln structure

---

## 1. Executive Summary

This project is a single-page executive reporting dashboard that visualizes project milestones, progress status, and delivery health. The design reference (`OriginalDesignConcept.html`) defines a fixed-resolution (1920×1080) layout with a timeline/Gantt visualization at the top and a color-coded heatmap grid below, optimized for PowerPoint screenshot capture.

**Primary recommendation:** Build this as a minimal Blazor Server application with a single Razor page, reading all data from a local `data.json` file. Use pure CSS (Grid + Flexbox) for layout and inline SVG rendered via Blazor components for the timeline. No database, no authentication, no external services. The entire application should be under 10 files and deployable with a single `dotnet run`. This is intentionally a "sharp tool" — a focused utility, not a platform.

---

## 2. Key Findings

- The design is a **pixel-perfect, screenshot-oriented** layout (1920×1080) — not a responsive web app. This simplifies implementation significantly: no breakpoints, no mobile, no fluid layouts needed.
- The timeline section uses **SVG** for milestone diamonds, checkpoint circles, progress lines, and a "NOW" marker. Blazor can render SVG natively in Razor components with no JS library needed.
- The heatmap grid is a **pure CSS Grid** layout (`160px repeat(4,1fr)`) with four category rows (Shipped, In Progress, Carryover, Blockers) × four month columns. Each cell contains bulleted items.
- **No charting library is needed.** The entire visualization is achievable with CSS Grid, Flexbox, and hand-crafted SVG — exactly as the HTML reference does it. Adding a charting library (e.g., Chart.js, Radzen charts) would add complexity with zero benefit.
- A `data.json` flat file is the correct data store. No database engine, no ORM, no migrations. `System.Text.Json` deserialization is all that's needed.
- Blazor Server's SignalR circuit is irrelevant for this use case (no interactivity beyond page load), but it's the mandated stack and works fine for local `dotnet run` scenarios.
- The Segoe UI font specified in the design is available on all Windows machines (the target environment). No web font loading needed.
- The color palette is fully defined in the HTML reference: green (#34A853/#E8F5E9) for shipped, blue (#0078D4/#E3F2FD) for in-progress, amber (#F4B400/#FFF8E1) for carryover, red (#EA4335/#FEF2F2) for blockers.

---

## 3. Recommended Technology Stack

### Frontend Layer (Blazor Server)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Framework** | Blazor Server (.NET 8) | .NET 8.0.x (LTS) | Mandated stack. Use `dotnet new blazorserver` template. |
| **CSS Strategy** | Scoped CSS + single `dashboard.css` | N/A | Use Blazor's built-in CSS isolation (`.razor.css` files). Mirror the design's existing CSS almost verbatim. |
| **Layout Engine** | CSS Grid + Flexbox | N/A | Grid for the heatmap (`grid-template-columns: 160px repeat(4,1fr)`), Flexbox for header and timeline area. No CSS framework needed (no Bootstrap, no Tailwind). |
| **SVG Rendering** | Inline SVG via Razor components | N/A | Render `<svg>` elements directly in `.razor` files. Use `@foreach` loops to emit milestone markers, timeline bars, and date labels from data. |
| **Charting Library** | **None** | — | The design uses hand-drawn SVG, not chart abstractions. A charting library would fight the design rather than help it. |
| **Component Library** | **None** (Radzen, MudBlazor, etc. not needed) | — | The UI is a single static-looking page. Component libraries add 500KB+ of CSS/JS for zero benefit here. |
| **Icons** | Inline SVG shapes | N/A | The design uses simple geometric shapes (diamonds, circles, lines) — not an icon set. |

### Backend Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Runtime** | .NET 8 LTS | 8.0.x | Long-term support through November 2026. |
| **JSON Deserialization** | `System.Text.Json` | Built-in | Ships with .NET 8. Use `JsonSerializer.Deserialize<T>()` with source generators for AOT-friendly deserialization. |
| **Configuration** | `IConfiguration` + `data.json` | Built-in | Register `data.json` as an additional JSON configuration source, or simply read it as a file at startup. |
| **File Watching** | `FileSystemWatcher` (optional) | Built-in | If you want the dashboard to auto-refresh when `data.json` is edited. Low priority — manual browser refresh is fine for v1. |
| **Logging** | `Microsoft.Extensions.Logging` | Built-in | Console logger only. No Serilog, no Seq, no structured logging needed for this scope. |

### Data Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Storage** | `data.json` flat file | N/A | Single JSON file in the project root or `wwwroot/data/` directory. |
| **Schema** | Strongly-typed C# POCOs | N/A | `DashboardData`, `Milestone`, `HeatmapRow`, `HeatmapItem` record types. |
| **Database** | **None** | — | A database is unjustified complexity. The data changes monthly (at most) and is hand-edited. |
| **Caching** | `IMemoryCache` or singleton service | Built-in | Load `data.json` once at startup into a singleton. Optionally reload on file change. |

### Testing Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | 2.7+ | Standard .NET test framework. Test JSON deserialization and data model logic. |
| **Blazor Component Testing** | bUnit | 1.25+ | If any component logic warrants testing. Likely overkill for this project. |
| **Snapshot/Visual Testing** | Playwright for .NET | 1.41+ | Only if you want automated screenshot comparison. Manual screenshots are the stated workflow, so this is optional. |

### Infrastructure & Tooling

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **IDE** | Visual Studio 2022 or VS Code + C# Dev Kit | Latest | Standard .NET development. |
| **Build** | `dotnet build` / `dotnet run` | Built-in | No custom build pipeline needed. |
| **Hot Reload** | `dotnet watch` | Built-in | Blazor Server supports hot reload for Razor/CSS changes. |
| **Screenshot Capture** | Browser DevTools or Playwright | N/A | Set viewport to 1920×1080, take full-page screenshot. |

---

## 4. Architecture Recommendations

### Overall Pattern: **Single-Page Read-Only Dashboard**

This is not a CRUD app. It's a read-only visualization of a JSON file. The architecture should reflect that simplicity.

```
data.json  →  DashboardDataService (singleton)  →  Razor Components  →  Browser
```

### Solution Structure

```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Models/
│       │   └── DashboardData.cs          # POCOs: DashboardData, Milestone, StatusCategory, StatusItem
│       ├── Services/
│       │   └── DashboardDataService.cs   # Reads and caches data.json
│       ├── Components/
│       │   ├── Pages/
│       │   │   └── Dashboard.razor       # Single page, route "/"
│       │   ├── Layout/
│       │   │   └── DashboardLayout.razor # Minimal layout (no nav, no sidebar)
│       │   ├── Header.razor              # Title, subtitle, legend
│       │   ├── Timeline.razor            # SVG timeline with milestones
│       │   └── Heatmap.razor             # CSS Grid status heatmap
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css         # Global styles matching design spec
│       │   └── data/
│       │       └── data.json             # Dashboard data file
│       └── appsettings.json
└── tests/
    └── ReportingDashboard.Tests/
        └── ReportingDashboard.Tests.csproj
```

### Data Model Design

```csharp
public record DashboardData
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogLink { get; init; } = "";
    public string CurrentMonth { get; init; } = "";    // e.g., "Apr 2026"
    public List<string> Months { get; init; } = [];     // Column headers: ["Jan","Feb","Mar","Apr"]
    public List<Milestone> Milestones { get; init; } = [];
    public List<TimelineTrack> TimelineTracks { get; init; } = [];
    public HeatmapData Heatmap { get; init; } = new();
}

public record Milestone
{
    public string Label { get; init; } = "";
    public string Date { get; init; } = "";         // ISO date string
    public string Type { get; init; } = "";          // "poc", "production", "checkpoint"
    public string TrackId { get; init; } = "";       // Which timeline track
}

public record TimelineTrack
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Color { get; init; } = "";         // Hex color for the track line
}

public record HeatmapData
{
    public List<StatusCategory> Categories { get; init; } = [];
}

public record StatusCategory
{
    public string Name { get; init; } = "";          // "Shipped", "In Progress", "Carryover", "Blockers"
    public string ColorClass { get; init; } = "";    // CSS class prefix: "ship", "prog", "carry", "block"
    public Dictionary<string, List<string>> ItemsByMonth { get; init; } = new();
}
```

### SVG Timeline Rendering Strategy

The timeline is the most complex visual element. Recommended approach:

1. **Define a date-to-pixel mapping function** based on the timeline's date range and SVG width (1560px in the reference).
2. **Render month gridlines** with `<line>` and `<text>` elements in a `@foreach` loop.
3. **Render the "NOW" marker** as a dashed red `<line>` positioned by current date.
4. **Render each track** as a horizontal `<line>` with milestone markers (`<polygon>` for diamonds, `<circle>` for checkpoints).
5. **All coordinates are calculated in C#** — no JavaScript needed.

```csharp
// In Timeline.razor
@code {
    private double DateToX(DateOnly date)
    {
        var totalDays = (EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
        var elapsed = (date.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
        return (elapsed / totalDays) * SvgWidth;
    }
}
```

### CSS Architecture

**Do not use CSS-in-JS, Tailwind, or any CSS framework.** The reference HTML already defines the exact CSS needed. Port it directly:

1. Copy the CSS from `OriginalDesignConcept.html` into `dashboard.css`.
2. Adjust selectors to match Blazor component class names.
3. Use Blazor CSS isolation (`.razor.css`) only if you want per-component scoping — but for a single-page app, a single global CSS file is simpler and more maintainable.
4. Keep the fixed `1920px × 1080px` body dimensions for screenshot fidelity.

### Data Flow

```
Startup:
  Program.cs registers DashboardDataService as singleton
  DashboardDataService reads wwwroot/data/data.json into DashboardData

Page Load:
  Dashboard.razor injects DashboardDataService
  Passes data down to Header, Timeline, Heatmap components via [Parameter]

No interactivity, no WebSocket state, no form submissions.
```

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope. The application runs locally on the developer's machine. No login page, no identity provider, no role-based access. The `Program.cs` should not include any auth middleware.

If future access control is needed, the simplest approach would be a shared secret in `appsettings.json` checked via middleware — but do not build this now.

### Data Protection

- `data.json` contains project names and status descriptions — not PII or secrets.
- No encryption needed for data at rest or in transit (localhost only).
- If the dashboard ever contains sensitive project details, ensure the machine's file permissions restrict access to `data.json`.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | `dotnet run` from command line or VS debug (F5) |
| **Port** | Default Kestrel (https://localhost:5001 or http://localhost:5000) |
| **Reverse Proxy** | None needed |
| **Containerization** | Not needed for local-only use |
| **Publishing** | `dotnet publish -c Release` produces a self-contained folder if sharing with teammates |

### Screenshot Workflow

Since the primary output is PowerPoint screenshots:

1. Run `dotnet run`
2. Open `https://localhost:5001` in Edge/Chrome
3. Set browser zoom to 100%, window to 1920×1080 (or use DevTools device emulation)
4. Take screenshot (Win+Shift+S, or DevTools capture)
5. Paste into PowerPoint

**Optional automation:** Add a Playwright script that captures the screenshot programmatically:

```csharp
// tools/Screenshot.csproj — standalone console app
using var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();
await page.SetViewportSizeAsync(1920, 1080);
await page.GotoAsync("https://localhost:5001");
await page.ScreenshotAsync(new() { Path = "dashboard.png" });
```

### Estimated Infrastructure Cost

**$0.** This runs on the developer's existing workstation. No cloud resources, no licenses, no subscriptions.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **Blazor Server is overkill** for a static read-only page | Low | Accepted trade-off — it's the mandated stack. The overhead is minimal (~50MB memory for the SignalR circuit). A static HTML page would suffice, but Blazor provides component reuse and type-safe data binding. |
| **SVG coordinate math errors** in timeline rendering | Medium | Write unit tests for the `DateToX()` mapping function. Verify against reference HTML coordinates. |
| **CSS Grid inconsistencies** across browsers | Low | Target only Chromium-based browsers (Edge/Chrome) since screenshots are the deliverable. No need for Firefox/Safari testing. |
| **data.json schema drift** over time | Medium | Define a strict C# model and validate on load. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` for flexibility but keep the schema documented. |
| **Hot reload breaks SVG rendering** during development | Low | Known Blazor issue with complex SVG. Workaround: full page refresh (Ctrl+Shift+R). |

### Trade-offs Accepted

1. **No real-time updates.** The dashboard shows a point-in-time snapshot. Editing `data.json` requires a page refresh. This is acceptable — the data changes weekly/monthly.
2. **No responsive design.** Fixed 1920×1080 layout. Viewing on smaller screens will require scrolling. This is acceptable — the output is screenshots, not a web app.
3. **No dark mode.** The design is light-themed for PowerPoint embedding on white/light slides.
4. **SignalR circuit overhead.** Blazor Server maintains a WebSocket connection that's unnecessary for a read-only page. The memory/CPU impact is negligible for a single-user local app.

### What Could Go Wrong

- **Over-engineering:** The biggest risk is adding unnecessary complexity (database, auth, component library, state management). Resist scope creep. This is a visualization of a JSON file.
- **Design fidelity:** The reference HTML has pixel-specific positioning. Getting the Blazor version to match exactly requires careful CSS porting. Diff screenshots side-by-side during development.

---

## 7. Open Questions

1. **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always show the trailing 4 months?

2. **How many timeline tracks?** The reference shows 3 tracks (M1, M2, M3). Should `data.json` support an arbitrary number, or cap at 3–5 for visual clarity?

3. **Who maintains `data.json`?** Is this hand-edited by the project manager, or will a future integration populate it from ADO/Jira? This affects schema design (human-friendly vs. machine-friendly).

4. **Should the "NOW" marker auto-calculate from system date, or be specified in `data.json`?** Auto-calculation is simpler but means the screenshot changes daily. A fixed date in `data.json` gives reproducible screenshots.

5. **What is the timeline date range?** The reference shows Jan–Jun (6 months). Should this be configurable, or should it auto-calculate from milestone dates with padding?

6. **Will there be multiple projects/dashboards?** If yes, `data.json` could become `data/{project-name}.json` with a route parameter. If it's always one project, keep it simple.

7. **Desired improvements over the reference design:** The brief says "improve upon it." What specific improvements are desired? Suggestions:
   - Better typography hierarchy
   - Subtle animations on page load (CSS-only)
   - Print-friendly `@media print` styles
   - Percentage completion indicators per category
   - Summary counts in the header (e.g., "12 shipped, 5 in progress, 3 blocked")

---

## 8. Implementation Recommendations

### Phase 1: Static Layout (Day 1) — MVP

**Goal:** Pixel-perfect reproduction of the reference HTML as a Blazor page with hardcoded data.

1. `dotnet new blazorserver -n ReportingDashboard --framework net8.0`
2. Strip the default template (remove Counter, Weather, NavMenu, etc.)
3. Create `DashboardLayout.razor` — minimal layout with no sidebar/nav
4. Port CSS from `OriginalDesignConcept.html` into `dashboard.css`
5. Build `Header.razor`, `Timeline.razor`, `Heatmap.razor` with hardcoded HTML matching the reference
6. Verify visual match with side-by-side screenshot comparison

**Deliverable:** A running Blazor app that looks identical to the reference HTML.

### Phase 2: Data-Driven Rendering (Day 2)

**Goal:** Replace hardcoded HTML with data-bound Blazor components reading from `data.json`.

1. Define `DashboardData` model classes (records)
2. Create sample `data.json` with fictional project data
3. Build `DashboardDataService` to load and cache the JSON
4. Refactor components to accept `[Parameter]` properties and render from data
5. Implement SVG coordinate calculation for timeline positioning
6. Verify that changing `data.json` values produces correct visual output

**Deliverable:** A fully data-driven dashboard where editing `data.json` changes the display.

### Phase 3: Polish & Improvements (Day 3)

**Goal:** Improve on the reference design for executive readability.

1. Add summary statistics in the header (item counts per category)
2. Improve typography: slightly larger heatmap text, better whitespace
3. Highlight the current month column more prominently
4. Add subtle CSS transitions for a polished feel
5. Add `@media print` styles for direct browser-to-PDF if needed
6. Create a well-documented sample `data.json` with comments explaining each field
7. Write a `README.md` with setup instructions and screenshot workflow

**Deliverable:** A polished, documented dashboard ready for production use.

### Quick Wins

- **Copy the reference CSS verbatim** as the starting point — don't rewrite it. This saves hours and guarantees visual fidelity.
- **Use C# records with `init` properties** for the data model — immutable, concise, and JSON-friendly.
- **Use `dotnet watch`** during development for instant feedback on Razor/CSS changes.
- **Screenshot comparison** is your test suite. Take a reference screenshot of the HTML file and diff it against the Blazor output using any image diff tool.

### What NOT to Build

| Temptation | Why to Resist |
|------------|---------------|
| Database (SQLite, EF Core) | JSON file is sufficient for monthly-updated data |
| Authentication | No users, no roles, local-only |
| REST API | No external consumers — Blazor reads data directly |
| Component library (MudBlazor, Radzen) | Adds 500KB+ for zero UI benefit |
| State management (Fluxor, Redux-style) | One page, one data load, no state to manage |
| Docker/container | Local `dotnet run` is the deployment |
| CI/CD pipeline | One developer, manual deployment |
| JavaScript interop | SVG and CSS handle all visual needs |
| Responsive breakpoints | Fixed 1920×1080 for screenshots |

---

## Appendix A: NuGet Packages

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Blazor Server runtime | **Yes** (implicit) |
| `System.Text.Json` | 8.0.x | JSON deserialization | **Yes** (built-in) |
| `xunit` | 2.7+ | Unit testing | Optional |
| `Microsoft.Playwright` | 1.41+ | Automated screenshots | Optional |

**Total additional NuGet packages needed: 0** (for core functionality)

## Appendix B: Color Palette Reference

Extracted from the design HTML for direct use in CSS:

```css
:root {
    /* Category: Shipped */
    --ship-text: #1B7A28;
    --ship-header-bg: #E8F5E9;
    --ship-cell-bg: #F0FBF0;
    --ship-cell-highlight: #D8F2DA;
    --ship-dot: #34A853;

    /* Category: In Progress */
    --prog-text: #1565C0;
    --prog-header-bg: #E3F2FD;
    --prog-cell-bg: #EEF4FE;
    --prog-cell-highlight: #DAE8FB;
    --prog-dot: #0078D4;

    /* Category: Carryover */
    --carry-text: #B45309;
    --carry-header-bg: #FFF8E1;
    --carry-cell-bg: #FFFDE7;
    --carry-cell-highlight: #FFF0B0;
    --carry-dot: #F4B400;

    /* Category: Blockers */
    --block-text: #991B1B;
    --block-header-bg: #FEF2F2;
    --block-cell-bg: #FFF5F5;
    --block-cell-highlight: #FFE4E4;
    --block-dot: #EA4335;

    /* Chrome */
    --bg: #FFFFFF;
    --text: #111;
    --link: #0078D4;
    --muted: #888;
    --border: #E0E0E0;
    --header-bg: #F5F5F5;
    --highlight-month-bg: #FFF0D0;
    --highlight-month-text: #C07700;
}
```

## Appendix C: Sample data.json Structure

```json
{
  "title": "Project Phoenix Release Roadmap",
  "backlogLink": "https://dev.azure.com/org/project/_backlogs",
  "subtitle": "Platform Engineering • Phoenix Workstream • April 2026",
  "currentDate": "2026-04-15",
  "timelineRange": { "start": "2026-01-01", "end": "2026-06-30" },
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "highlightMonth": "Apr",
  "timelineTracks": [
    {
      "id": "m1",
      "name": "M1",
      "description": "Auth & Identity",
      "color": "#0078D4",
      "milestones": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "May Prod", "type": "production" }
      ]
    }
  ],
  "heatmap": {
    "categories": [
      {
        "name": "Shipped",
        "colorClass": "ship",
        "itemsByMonth": {
          "Jan": ["SSO Integration", "Token Refresh Flow"],
          "Feb": ["Role-Based Access"],
          "Mar": ["Audit Logging v1"],
          "Apr": ["Compliance Dashboard"]
        }
      }
    ]
  }
}
```

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/de9a56fb850c70b79d540df9ab0444da5632b4f8/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
