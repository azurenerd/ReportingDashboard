# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-17 09:09 UTC_

### Summary

This project is a single-page executive reporting dashboard that visualizes project milestones, progress status, and delivery health. The design calls for a timeline/Gantt visualization at the top, a color-coded heatmap grid (Shipped / In Progress / Carryover / Blockers by month), and a clean header with legend — all driven by a `data.json` configuration file. **Primary recommendation:** Build this as a minimal Blazor Server application with zero external database dependencies. Read all data from a local `data.json` file at startup. Render the timeline SVG and heatmap grid entirely with Blazor components and inline CSS that faithfully reproduces the `OriginalDesignConcept.html` design. No authentication, no cloud services, no JavaScript charting libraries. The entire solution should be under 15 files and deployable with `dotnet run`. This is intentionally a "screenshot-grade" dashboard — optimized for visual clarity in PowerPoint decks rather than interactive exploration. Every technology decision should favor simplicity and pixel-perfect rendering over extensibility. --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.Components` | 8.0.x (implicit) | Blazor Server framework | Yes (included in SDK) | | `System.Text.Json` | 8.0.x (implicit) | JSON deserialization | Yes (included in SDK) | | `xunit` | 2.7.x | Unit testing | Optional | | `bunit` | 1.25.x | Blazor component testing | Optional | | `FluentAssertions` | 6.12.x | Readable test assertions | Optional | **Total external NuGet dependencies for the core app: 0.** Everything needed is included in the .NET 8 SDK.

### Key Findings

- The `OriginalDesignConcept.html` design uses pure CSS Grid + Flexbox + inline SVG with no JavaScript dependencies — this translates directly to Blazor components with zero JS interop needed.
- The design targets a fixed 1920×1080 viewport (presentation resolution), which eliminates responsive design complexity entirely.
- A `data.json` flat file is sufficient for this use case — no database is needed. `System.Text.Json` (built into .NET 8) handles all serialization.
- The SVG timeline with diamond milestones, circle checkpoints, and a "NOW" marker line can be rendered entirely as a Blazor component emitting raw SVG markup — no charting library required.
- The heatmap grid is a CSS Grid layout with 5 columns (`160px repeat(4,1fr)`) and 5 rows (header + 4 status categories) — this maps 1:1 to a Blazor `@foreach` loop over data categories.
- The color palette is fully defined in the HTML reference: green (#34A853) for Shipped, blue (#0078D4) for In Progress, amber (#F4B400) for Carryover, red (#EA4335) for Blockers. These should be CSS custom properties for maintainability.
- Blazor Server's SignalR connection is irrelevant for this use case (static data, single user) but is the simplest hosting model — no WASM download, instant render.
- The font stack is "Segoe UI, Arial, sans-serif" — available on all Windows machines where this will run locally, no web font loading needed.
- Hot reload in .NET 8 Blazor Server works well for iterating on layout — critical for matching the design pixel-by-pixel.
- The solution structure should be minimal: one project, one page, a handful of components, one CSS file, one data model, one JSON file. --- **Goal:** Reproduce the `OriginalDesignConcept.html` design as a Blazor app with hardcoded data.
- `dotnet new blazorserver -n ReportingDashboard --framework net8.0`
- Delete all default pages (Counter, Weather, etc.)
- Copy the CSS from `OriginalDesignConcept.html` into `app.css`
- Create `Dashboard.razor` as the single page at route `/`
- Create `Header.razor`, `Timeline.razor`, `HeatmapGrid.razor`, `HeatmapCell.razor`
- Hardcode sample data directly in the components
- **Verify:** Visual diff against the original design at 1920×1080 **Quick win:** At the end of Phase 1, you have a working screenshot-grade dashboard. **Goal:** Replace hardcoded data with `data.json`.
- Define C# record types in `Models/ProjectData.cs`
- Create `Services/ProjectDataService.cs` that reads `wwwroot/data/data.json`
- Register as singleton in `Program.cs`
- Inject into `Dashboard.razor` and pass data to child components
- Create example `data.json` with fictional project data
- **Verify:** Changing `data.json` and restarting the app updates the dashboard **Goal:** Render the timeline from data instead of hardcoded SVG coordinates.
- Implement date-to-pixel mapping in `Timeline.razor` code-behind
- Support configurable date range (derived from earliest/latest milestone dates, with padding)
- Render month gridlines, track lines, milestone markers, and NOW line dynamically
- **Verify:** Add/remove milestones in `data.json` and confirm the timeline updates correctly **Goal:** Final visual polish and a compelling example dataset.
- Fine-tune spacing, font sizes, and colors to match the design exactly
- Add CSS drop shadows on milestone diamonds (SVG `<filter>` as in the original)
- Create a rich example `data.json` for "Project Phoenix" with 20+ status items
- Test screenshot workflow: open in Chrome → DevTools → set viewport 1920×1080 → capture
- **Verify:** Screenshot is presentation-ready for PowerPoint **Goal:** Tasteful enhancements that make the dashboard better than the HTML original.
- Add subtle CSS transitions for a polished feel (not needed for screenshots but nice for live demos)
- Add a print stylesheet (`@media print`) that removes margins for clean PDF export
- Add a `"lastUpdated"` timestamp in the footer from `data.json`
- Support a second page/tab for a different project (stretch goal, likely unnecessary)
- ❌ Database (SQLite, SQL Server, etc.) — `data.json` is sufficient
- ❌ Authentication — local-only tool
- ❌ API controllers — Blazor components read data directly from the service
- ❌ Entity Framework — no database means no ORM
- ❌ SignalR hubs — the default Blazor circuit is enough
- ❌ Docker containerization — `dotnet run` is simpler
- ❌ CI/CD pipeline — single developer, local tool
- ❌ Logging infrastructure — `Console.WriteLine` is fine for debugging
- ❌ CSS framework (Bootstrap, Tailwind) — the design is custom and fixed-width
- ❌ JavaScript charting library — raw SVG in Blazor is cleaner for this design
- ❌ Component library (MudBlazor, Radzen) — adds complexity without benefit for a custom layout ---

### Recommended Tools & Technologies

- **Project:** Executive Project Reporting Dashboard **Date:** April 17, 2026 **Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure --- | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **UI Framework** | Blazor Server (built-in) | .NET 8.0 LTS | No additional packages. Server-side rendering, no WASM payload. | | **CSS Strategy** | Single `app.css` + CSS custom properties | N/A | Replicate the design's CSS Grid/Flexbox layout verbatim. No CSS framework (Bootstrap/Tailwind) — the design is fixed-width and doesn't need a grid system. | | **SVG Timeline** | Raw SVG markup in Blazor component | N/A | The `OriginalDesignConcept.html` already uses inline SVG. Translate `<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` elements directly into a `Timeline.razor` component with `@foreach` loops over milestone data. | | **Heatmap Grid** | CSS Grid rendered by Blazor component | N/A | `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months from data.json. Each cell rendered via nested loops. | | **Icons/Shapes** | Inline SVG / CSS transforms | N/A | Diamond shapes via `transform: rotate(45deg)` on `<span>` or SVG `<polygon>`. No icon library needed. | | **Component Library** | **None** | — | Do NOT use MudBlazor, Radzen, Syncfusion, or any component library. The design is a specific, custom layout. Component libraries add weight and fight against custom designs. Build 4-5 small razor components instead. | | **Charting Library** | **None** | — | The timeline is simple enough that hand-crafted SVG is cleaner and more controllable than any charting library. Libraries like ApexCharts.Blazor or BlazorChart would over-engineer this. | **Why no component/charting library:** The design is a fixed, presentation-grade layout. Charting libraries add 200KB+ of JS, impose their own styling opinions, and make pixel-perfect reproduction harder. The SVG in the original HTML is ~40 lines — a Blazor component rendering this is simpler than configuring a chart library. | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Runtime** | .NET 8 LTS | 8.0.x | Long-term support until November 2026. | | **JSON Serialization** | `System.Text.Json` | Built-in (.NET 8) | Native, fast, zero dependencies. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` for flexible JSON keys. | | **Data Access** | Direct file read via `IFileProvider` or `File.ReadAllTextAsync` | Built-in | Read `data.json` from `wwwroot/` or project root at startup. Register as a singleton service. | | **Configuration** | `IConfiguration` + custom JSON | Built-in | Optionally bind `data.json` to the .NET configuration system, but a dedicated `DataService` is cleaner for structured project data. | | **Dependency Injection** | Built-in DI container | Built-in | Register `IProjectDataService` as singleton. | | Component | Recommendation | Notes | |-----------|---------------|-------| | **Primary Store** | `data.json` flat file | No database. File lives in project directory or `wwwroot/data/`. | | **File Watching** | `FileSystemWatcher` (optional) | If you want live reload when `data.json` is edited, watch the file and re-parse. Otherwise, restart the app. | | **Schema** | Strongly-typed C# records | Define `ProjectData`, `Milestone`, `StatusCategory`, `StatusItem` as C# record types. Deserialize JSON into these. | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Unit Testing** | xUnit | 2.7.x | Most popular .NET test framework. Use for data model and service tests. | | **Blazor Component Testing** | bUnit | 1.25.x+ | Test Razor components in isolation. Verify SVG output, grid cell counts, CSS classes. | | **Assertions** | FluentAssertions | 6.12.x | Readable assertion syntax: `result.Milestones.Should().HaveCount(3)`. MIT license. | | **Snapshot Testing** | Verify | 23.x | Optional. Snapshot test the rendered HTML output to catch visual regressions. | | Tool | Version | Notes | |------|---------|-------| | **SDK** | .NET 8.0 SDK | `dotnet new blazorserver` scaffolds the project. | | **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Hot reload support for Blazor. | | **Browser** | Any modern browser | Render at 1920×1080 for screenshots. Chrome DevTools device toolbar can lock viewport. | ---
```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── Models/
│   │   └── ProjectData.cs          # C# records for all data shapes
│   ├── Services/
│   │   └── ProjectDataService.cs   # Reads and caches data.json
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Dashboard.razor     # The single page
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor
│   │   │   └── MainLayout.razor.css
│   │   ├── Header.razor            # Title, subtitle, legend
│   │   ├── Timeline.razor          # SVG milestone timeline
│   │   ├── HeatmapGrid.razor       # CSS Grid status matrix
│   │   └── HeatmapCell.razor       # Individual cell with items
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── app.css             # All styles (from design HTML)
│   │   └── data/
│   │       └── data.json           # Project data
│   └── appsettings.json
└── ReportingDashboard.Tests/       # Optional test project
    ├── ReportingDashboard.Tests.csproj
    └── ...
```
```csharp
public record ProjectData
{
    public string ProjectName { get; init; } = "";
    public string Workstream { get; init; } = "";
    public string Period { get; init; } = "";       // e.g., "April 2026"
    public string BacklogUrl { get; init; } = "";
    public List<string> Months { get; init; } = [];  // ["Jan","Feb","Mar","Apr"]
    public int CurrentMonthIndex { get; init; }       // 0-based, which month is "now"
    public List<MilestoneTrack> Tracks { get; init; } = [];
    public List<StatusCategory> Categories { get; init; } = [];
}

public record MilestoneTrack
{
    public string Id { get; init; } = "";            // "M1", "M2", "M3"
    public string Label { get; init; } = "";
    public string Color { get; init; } = "";         // Track color hex
    public List<MilestoneEvent> Events { get; init; } = [];
}

public record MilestoneEvent
{
    public string Date { get; init; } = "";          // "2026-01-12"
    public string Label { get; init; } = "";
    public string Type { get; init; } = "";          // "checkpoint", "poc", "production"
}

public record StatusCategory
{
    public string Name { get; init; } = "";          // "Shipped", "In Progress", etc.
    public string CssClass { get; init; } = "";      // "ship", "prog", "carry", "block"
    public List<MonthItems> MonthData { get; init; } = [];
}

public record MonthItems
{
    public string Month { get; init; } = "";
    public List<string> Items { get; init; } = [];
}
```
```
data.json  →  ProjectDataService (singleton, reads once)  →  Dashboard.razor (cascading parameter)
                                                            ├── Header.razor
                                                            ├── Timeline.razor (SVG generation)
                                                            └── HeatmapGrid.razor
                                                                └── HeatmapCell.razor (×N)
``` No API controllers, no HTTP endpoints, no SignalR hubs beyond the default Blazor circuit. The `ProjectDataService` reads the file once and holds the deserialized object in memory. The CSS from `OriginalDesignConcept.html` should be extracted almost verbatim into `app.css`. Key mappings: | Design CSS Class | Blazor Component | Purpose | |-----------------|-----------------|---------| | `.hdr` | `Header.razor` | Top bar with title + legend | | `.tl-area`, `.tl-svg-box` | `Timeline.razor` | Timeline container | | `.hm-wrap`, `.hm-grid` | `HeatmapGrid.razor` | Heatmap container + grid | | `.hm-cell`, `.it` | `HeatmapCell.razor` | Individual cell with bullet items | | `.ship-*`, `.prog-*`, `.carry-*`, `.block-*` | CSS classes applied dynamically | Row color theming | **CSS custom properties** for the color palette:
```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-blocker: #EA4335;
    --color-blocker-bg: #FFF5F5;
    --color-blocker-bg-current: #FFE4E4;
    --color-now-line: #EA4335;
}
``` The timeline is the most complex visual element. Recommended approach:
- **Calculate pixel positions** from date ranges in C# code-behind. Define the SVG viewport as the full width of the container (use `@ref` to measure, or set a fixed width like the design's 1560px).
- **Map dates to X coordinates:** `xPos = (date - startDate) / (endDate - startDate) * svgWidth`.
- **Render track lines** as `<line>` elements at fixed Y positions (e.g., Y=42, Y=98, Y=154 for 3 tracks).
- **Render events** based on type:
- `checkpoint` → `<circle>` (small, filled #999)
- `poc` → `<polygon>` diamond, filled #F4B400 with drop shadow
- `production` → `<polygon>` diamond, filled #34A853 with drop shadow
- **Render "NOW" line** as a dashed vertical `<line>` at the current date's X position.
- **Month gridlines** as vertical `<line>` elements with month labels via `<text>`. All of this is templatable in Razor without any JS:
```razor
<svg width="@SvgWidth" height="@SvgHeight" xmlns="http://www.w3.org/2000/svg">
    @foreach (var month in Data.Months)
    {
        <line x1="@GetMonthX(month)" y1="0" x2="@GetMonthX(month)" y2="@SvgHeight" 
              stroke="#bbb" stroke-opacity="0.4" />
        <text x="@(GetMonthX(month)+5)" y="14" fill="#666" font-size="11">@month</text>
    }
    @* NOW line *@
    <line x1="@NowX" y1="0" x2="@NowX" y2="@SvgHeight" 
          stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3" />
    @foreach (var track in Data.Tracks)
    {
        <line x1="0" y1="@GetTrackY(track)" x2="@SvgWidth" y2="@GetTrackY(track)" 
              stroke="@track.Color" stroke-width="3" />
        @foreach (var evt in track.Events)
        {
            @* Render checkpoint/milestone based on evt.Type *@
        }
    }
</svg>
``` ---

### Considerations & Risks

- This is explicitly a local-only, single-user tool for generating executive screenshots. Adding authentication would be over-engineering.
- No login page
- No user management
- No role-based access
- No HTTPS certificate management (HTTP on localhost is fine) If this ever needs to be shared on a network, add a single shared password via `app.UseMiddleware<BasicAuthMiddleware>()` — but defer this entirely until needed.
- `data.json` contains project names and milestone descriptions — not PII or secrets
- No encryption needed for local file storage
- If sensitive project data is involved, ensure the machine's disk encryption (BitLocker) is enabled — this is an OS concern, not an app concern | Aspect | Recommendation | |--------|---------------| | **Runtime** | `dotnet run` from the project directory, or `dotnet publish -c Release` for a self-contained deploy | | **Hosting** | Kestrel (built-in), localhost only | | **Port** | Default `https://localhost:5001` or `http://localhost:5000` | | **Process Management** | Manual start/stop. No Windows Service, no IIS, no reverse proxy. | | **Publishing** | `dotnet publish -c Release -o ./publish` produces a folder you can xcopy to any machine with .NET 8 runtime | | **Self-Contained** | `dotnet publish -c Release --self-contained -r win-x64` for machines without .NET installed | **$0.** This runs on the developer's local machine. No cloud services, no hosting fees, no licenses. --- **Risk Level:** Medium **Description:** The timeline SVG requires date-to-pixel coordinate mapping, which involves careful math for positioning milestones across a 6-month span. Off-by-one errors or incorrect date parsing could misplace markers. **Mitigation:** Write unit tests for the coordinate mapping functions. Use `DateOnly` (not `DateTime`) for date-only operations. Test with edge cases: milestones at month boundaries, milestones on the same day, tracks with zero events. **Risk Level:** Low **Description:** The heatmap grid uses CSS Grid, which renders slightly differently across browsers. Since this is screenshot-grade, the browser choice matters. **Mitigation:** Standardize on Chrome for screenshots. The design's `1920px` fixed width eliminates responsive layout concerns. Test the grid in Chrome at exactly 1920×1080 using DevTools device mode. **Risk Level:** Low **Description:** As the dashboard evolves, the JSON schema may change, breaking existing data files. **Mitigation:** Use strongly-typed C# records with default values (empty lists, empty strings) so missing fields don't crash the app. Add a `"schemaVersion": 1` field to `data.json` for future migration support. **Risk Level:** High (Ironic) **Description:** The biggest risk is making this more complex than it needs to be. Adding a database, authentication, real-time updates, or interactive features would all be scope creep for a screenshot tool. **Mitigation:** Enforce the constraint: if it doesn't help produce a better PowerPoint screenshot, don't build it. No entity framework, no database, no identity server, no SignalR features. **Accepted trade-off:** A static HTML file (like the original design) would be even simpler. Blazor Server adds a runtime dependency and SignalR connection overhead. However, Blazor provides: (1) data binding from `data.json` so you don't hand-edit HTML, (2) component reuse for the heatmap cells, and (3) a path to adding interactivity later if needed. The overhead is justified. **Accepted trade-off:** Blazor Server round-trips to the server for every interaction. For a screenshot tool with no user interaction, this is irrelevant. If tooltips or drill-down are added later, latency could be noticeable — but that's a future concern. ---
- **How many months should the heatmap display?** The original design shows 4 months (Jan–Apr). Should this be configurable in `data.json`? Recommendation: Yes, make it data-driven with `"months": ["Jan","Feb","Mar","Apr"]`.
- **How many milestone tracks?** The original shows 3 tracks (M1, M2, M3). Should the number of tracks be fixed or dynamic from data? Recommendation: Dynamic — loop over whatever tracks exist in `data.json`.
- **Should the "current month" highlighting be automatic or configured?** The design highlights April with a different background color. Options: (a) auto-detect from system date, (b) explicit `"currentMonthIndex": 3` in JSON. Recommendation: Explicit in JSON, since the dashboard may be prepared days before the presentation.
- **What fictional project should the example data represent?** Need a project name, 3-4 milestones, and ~20 status items across the 4 categories. Recommendation: Use something like "Project Phoenix — Cloud Migration Platform" with realistic-sounding work items.
- **Should the "ADO Backlog" link in the header be functional?** The original design has a link. For a local tool, this could link to a real Azure DevOps URL or be purely decorative. Recommendation: Make it a configurable URL in `data.json`; if empty, don't render the link.
- **Print / export workflow?** If screenshots are the primary output, should the app include a "screenshot mode" that hides the browser chrome? Recommendation: No — use Chrome's `--window-size=1920,1080 --screenshot` flag or the DevTools capture feature. Don't build screenshot logic into the app. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Project Reporting Dashboard
**Date:** April 17, 2026
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure

---

## 1. Executive Summary

This project is a single-page executive reporting dashboard that visualizes project milestones, progress status, and delivery health. The design calls for a timeline/Gantt visualization at the top, a color-coded heatmap grid (Shipped / In Progress / Carryover / Blockers by month), and a clean header with legend — all driven by a `data.json` configuration file.

**Primary recommendation:** Build this as a minimal Blazor Server application with zero external database dependencies. Read all data from a local `data.json` file at startup. Render the timeline SVG and heatmap grid entirely with Blazor components and inline CSS that faithfully reproduces the `OriginalDesignConcept.html` design. No authentication, no cloud services, no JavaScript charting libraries. The entire solution should be under 15 files and deployable with `dotnet run`.

This is intentionally a "screenshot-grade" dashboard — optimized for visual clarity in PowerPoint decks rather than interactive exploration. Every technology decision should favor simplicity and pixel-perfect rendering over extensibility.

---

## 2. Key Findings

- The `OriginalDesignConcept.html` design uses pure CSS Grid + Flexbox + inline SVG with no JavaScript dependencies — this translates directly to Blazor components with zero JS interop needed.
- The design targets a fixed 1920×1080 viewport (presentation resolution), which eliminates responsive design complexity entirely.
- A `data.json` flat file is sufficient for this use case — no database is needed. `System.Text.Json` (built into .NET 8) handles all serialization.
- The SVG timeline with diamond milestones, circle checkpoints, and a "NOW" marker line can be rendered entirely as a Blazor component emitting raw SVG markup — no charting library required.
- The heatmap grid is a CSS Grid layout with 5 columns (`160px repeat(4,1fr)`) and 5 rows (header + 4 status categories) — this maps 1:1 to a Blazor `@foreach` loop over data categories.
- The color palette is fully defined in the HTML reference: green (#34A853) for Shipped, blue (#0078D4) for In Progress, amber (#F4B400) for Carryover, red (#EA4335) for Blockers. These should be CSS custom properties for maintainability.
- Blazor Server's SignalR connection is irrelevant for this use case (static data, single user) but is the simplest hosting model — no WASM download, instant render.
- The font stack is "Segoe UI, Arial, sans-serif" — available on all Windows machines where this will run locally, no web font loading needed.
- Hot reload in .NET 8 Blazor Server works well for iterating on layout — critical for matching the design pixel-by-pixel.
- The solution structure should be minimal: one project, one page, a handful of components, one CSS file, one data model, one JSON file.

---

## 3. Recommended Technology Stack

### Frontend Layer (Blazor Components + CSS)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 LTS | No additional packages. Server-side rendering, no WASM payload. |
| **CSS Strategy** | Single `app.css` + CSS custom properties | N/A | Replicate the design's CSS Grid/Flexbox layout verbatim. No CSS framework (Bootstrap/Tailwind) — the design is fixed-width and doesn't need a grid system. |
| **SVG Timeline** | Raw SVG markup in Blazor component | N/A | The `OriginalDesignConcept.html` already uses inline SVG. Translate `<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` elements directly into a `Timeline.razor` component with `@foreach` loops over milestone data. |
| **Heatmap Grid** | CSS Grid rendered by Blazor component | N/A | `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months from data.json. Each cell rendered via nested loops. |
| **Icons/Shapes** | Inline SVG / CSS transforms | N/A | Diamond shapes via `transform: rotate(45deg)` on `<span>` or SVG `<polygon>`. No icon library needed. |
| **Component Library** | **None** | — | Do NOT use MudBlazor, Radzen, Syncfusion, or any component library. The design is a specific, custom layout. Component libraries add weight and fight against custom designs. Build 4-5 small razor components instead. |
| **Charting Library** | **None** | — | The timeline is simple enough that hand-crafted SVG is cleaner and more controllable than any charting library. Libraries like ApexCharts.Blazor or BlazorChart would over-engineer this. |

**Why no component/charting library:** The design is a fixed, presentation-grade layout. Charting libraries add 200KB+ of JS, impose their own styling opinions, and make pixel-perfect reproduction harder. The SVG in the original HTML is ~40 lines — a Blazor component rendering this is simpler than configuring a chart library.

### Backend Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Runtime** | .NET 8 LTS | 8.0.x | Long-term support until November 2026. |
| **JSON Serialization** | `System.Text.Json` | Built-in (.NET 8) | Native, fast, zero dependencies. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` for flexible JSON keys. |
| **Data Access** | Direct file read via `IFileProvider` or `File.ReadAllTextAsync` | Built-in | Read `data.json` from `wwwroot/` or project root at startup. Register as a singleton service. |
| **Configuration** | `IConfiguration` + custom JSON | Built-in | Optionally bind `data.json` to the .NET configuration system, but a dedicated `DataService` is cleaner for structured project data. |
| **Dependency Injection** | Built-in DI container | Built-in | Register `IProjectDataService` as singleton. |

### Data Storage

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **Primary Store** | `data.json` flat file | No database. File lives in project directory or `wwwroot/data/`. |
| **File Watching** | `FileSystemWatcher` (optional) | If you want live reload when `data.json` is edited, watch the file and re-parse. Otherwise, restart the app. |
| **Schema** | Strongly-typed C# records | Define `ProjectData`, `Milestone`, `StatusCategory`, `StatusItem` as C# record types. Deserialize JSON into these. |

### Testing

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | 2.7.x | Most popular .NET test framework. Use for data model and service tests. |
| **Blazor Component Testing** | bUnit | 1.25.x+ | Test Razor components in isolation. Verify SVG output, grid cell counts, CSS classes. |
| **Assertions** | FluentAssertions | 6.12.x | Readable assertion syntax: `result.Milestones.Should().HaveCount(3)`. MIT license. |
| **Snapshot Testing** | Verify | 23.x | Optional. Snapshot test the rendered HTML output to catch visual regressions. |

### Build & Tooling

| Tool | Version | Notes |
|------|---------|-------|
| **SDK** | .NET 8.0 SDK | `dotnet new blazorserver` scaffolds the project. |
| **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Hot reload support for Blazor. |
| **Browser** | Any modern browser | Render at 1920×1080 for screenshots. Chrome DevTools device toolbar can lock viewport. |

---

## 4. Architecture Recommendations

### Overall Architecture: Flat Single-Project

```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── Models/
│   │   └── ProjectData.cs          # C# records for all data shapes
│   ├── Services/
│   │   └── ProjectDataService.cs   # Reads and caches data.json
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Dashboard.razor     # The single page
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor
│   │   │   └── MainLayout.razor.css
│   │   ├── Header.razor            # Title, subtitle, legend
│   │   ├── Timeline.razor          # SVG milestone timeline
│   │   ├── HeatmapGrid.razor       # CSS Grid status matrix
│   │   └── HeatmapCell.razor       # Individual cell with items
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── app.css             # All styles (from design HTML)
│   │   └── data/
│   │       └── data.json           # Project data
│   └── appsettings.json
└── ReportingDashboard.Tests/       # Optional test project
    ├── ReportingDashboard.Tests.csproj
    └── ...
```

### Data Model Design

```csharp
public record ProjectData
{
    public string ProjectName { get; init; } = "";
    public string Workstream { get; init; } = "";
    public string Period { get; init; } = "";       // e.g., "April 2026"
    public string BacklogUrl { get; init; } = "";
    public List<string> Months { get; init; } = [];  // ["Jan","Feb","Mar","Apr"]
    public int CurrentMonthIndex { get; init; }       // 0-based, which month is "now"
    public List<MilestoneTrack> Tracks { get; init; } = [];
    public List<StatusCategory> Categories { get; init; } = [];
}

public record MilestoneTrack
{
    public string Id { get; init; } = "";            // "M1", "M2", "M3"
    public string Label { get; init; } = "";
    public string Color { get; init; } = "";         // Track color hex
    public List<MilestoneEvent> Events { get; init; } = [];
}

public record MilestoneEvent
{
    public string Date { get; init; } = "";          // "2026-01-12"
    public string Label { get; init; } = "";
    public string Type { get; init; } = "";          // "checkpoint", "poc", "production"
}

public record StatusCategory
{
    public string Name { get; init; } = "";          // "Shipped", "In Progress", etc.
    public string CssClass { get; init; } = "";      // "ship", "prog", "carry", "block"
    public List<MonthItems> MonthData { get; init; } = [];
}

public record MonthItems
{
    public string Month { get; init; } = "";
    public List<string> Items { get; init; } = [];
}
```

### Data Flow (Simple & Linear)

```
data.json  →  ProjectDataService (singleton, reads once)  →  Dashboard.razor (cascading parameter)
                                                            ├── Header.razor
                                                            ├── Timeline.razor (SVG generation)
                                                            └── HeatmapGrid.razor
                                                                └── HeatmapCell.razor (×N)
```

No API controllers, no HTTP endpoints, no SignalR hubs beyond the default Blazor circuit. The `ProjectDataService` reads the file once and holds the deserialized object in memory.

### CSS Architecture: Design-Faithful Reproduction

The CSS from `OriginalDesignConcept.html` should be extracted almost verbatim into `app.css`. Key mappings:

| Design CSS Class | Blazor Component | Purpose |
|-----------------|-----------------|---------|
| `.hdr` | `Header.razor` | Top bar with title + legend |
| `.tl-area`, `.tl-svg-box` | `Timeline.razor` | Timeline container |
| `.hm-wrap`, `.hm-grid` | `HeatmapGrid.razor` | Heatmap container + grid |
| `.hm-cell`, `.it` | `HeatmapCell.razor` | Individual cell with bullet items |
| `.ship-*`, `.prog-*`, `.carry-*`, `.block-*` | CSS classes applied dynamically | Row color theming |

**CSS custom properties** for the color palette:

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-blocker: #EA4335;
    --color-blocker-bg: #FFF5F5;
    --color-blocker-bg-current: #FFE4E4;
    --color-now-line: #EA4335;
}
```

### SVG Timeline Rendering Strategy

The timeline is the most complex visual element. Recommended approach:

1. **Calculate pixel positions** from date ranges in C# code-behind. Define the SVG viewport as the full width of the container (use `@ref` to measure, or set a fixed width like the design's 1560px).
2. **Map dates to X coordinates:** `xPos = (date - startDate) / (endDate - startDate) * svgWidth`.
3. **Render track lines** as `<line>` elements at fixed Y positions (e.g., Y=42, Y=98, Y=154 for 3 tracks).
4. **Render events** based on type:
   - `checkpoint` → `<circle>` (small, filled #999)
   - `poc` → `<polygon>` diamond, filled #F4B400 with drop shadow
   - `production` → `<polygon>` diamond, filled #34A853 with drop shadow
5. **Render "NOW" line** as a dashed vertical `<line>` at the current date's X position.
6. **Month gridlines** as vertical `<line>` elements with month labels via `<text>`.

All of this is templatable in Razor without any JS:

```razor
<svg width="@SvgWidth" height="@SvgHeight" xmlns="http://www.w3.org/2000/svg">
    @foreach (var month in Data.Months)
    {
        <line x1="@GetMonthX(month)" y1="0" x2="@GetMonthX(month)" y2="@SvgHeight" 
              stroke="#bbb" stroke-opacity="0.4" />
        <text x="@(GetMonthX(month)+5)" y="14" fill="#666" font-size="11">@month</text>
    }
    @* NOW line *@
    <line x1="@NowX" y1="0" x2="@NowX" y2="@SvgHeight" 
          stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3" />
    @foreach (var track in Data.Tracks)
    {
        <line x1="0" y1="@GetTrackY(track)" x2="@SvgWidth" y2="@GetTrackY(track)" 
              stroke="@track.Color" stroke-width="3" />
        @foreach (var evt in track.Events)
        {
            @* Render checkpoint/milestone based on evt.Type *@
        }
    }
</svg>
```

---

## 5. Security & Infrastructure

### Authentication & Authorization

**Recommendation: None.**

This is explicitly a local-only, single-user tool for generating executive screenshots. Adding authentication would be over-engineering.

- No login page
- No user management
- No role-based access
- No HTTPS certificate management (HTTP on localhost is fine)

If this ever needs to be shared on a network, add a single shared password via `app.UseMiddleware<BasicAuthMiddleware>()` — but defer this entirely until needed.

### Data Protection

- `data.json` contains project names and milestone descriptions — not PII or secrets
- No encryption needed for local file storage
- If sensitive project data is involved, ensure the machine's disk encryption (BitLocker) is enabled — this is an OS concern, not an app concern

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | `dotnet run` from the project directory, or `dotnet publish -c Release` for a self-contained deploy |
| **Hosting** | Kestrel (built-in), localhost only |
| **Port** | Default `https://localhost:5001` or `http://localhost:5000` |
| **Process Management** | Manual start/stop. No Windows Service, no IIS, no reverse proxy. |
| **Publishing** | `dotnet publish -c Release -o ./publish` produces a folder you can xcopy to any machine with .NET 8 runtime |
| **Self-Contained** | `dotnet publish -c Release --self-contained -r win-x64` for machines without .NET installed |

### Infrastructure Costs

**$0.** This runs on the developer's local machine. No cloud services, no hosting fees, no licenses.

---

## 6. Risks & Trade-offs

### Risk: SVG Timeline Complexity

**Risk Level:** Medium
**Description:** The timeline SVG requires date-to-pixel coordinate mapping, which involves careful math for positioning milestones across a 6-month span. Off-by-one errors or incorrect date parsing could misplace markers.
**Mitigation:** Write unit tests for the coordinate mapping functions. Use `DateOnly` (not `DateTime`) for date-only operations. Test with edge cases: milestones at month boundaries, milestones on the same day, tracks with zero events.

### Risk: CSS Grid Browser Rendering Differences

**Risk Level:** Low
**Description:** The heatmap grid uses CSS Grid, which renders slightly differently across browsers. Since this is screenshot-grade, the browser choice matters.
**Mitigation:** Standardize on Chrome for screenshots. The design's `1920px` fixed width eliminates responsive layout concerns. Test the grid in Chrome at exactly 1920×1080 using DevTools device mode.

### Risk: data.json Schema Drift

**Risk Level:** Low
**Description:** As the dashboard evolves, the JSON schema may change, breaking existing data files.
**Mitigation:** Use strongly-typed C# records with default values (empty lists, empty strings) so missing fields don't crash the app. Add a `"schemaVersion": 1` field to `data.json` for future migration support.

### Risk: Over-Engineering

**Risk Level:** High (Ironic)
**Description:** The biggest risk is making this more complex than it needs to be. Adding a database, authentication, real-time updates, or interactive features would all be scope creep for a screenshot tool.
**Mitigation:** Enforce the constraint: if it doesn't help produce a better PowerPoint screenshot, don't build it. No entity framework, no database, no identity server, no SignalR features.

### Trade-off: Blazor Server vs. Static HTML

**Accepted trade-off:** A static HTML file (like the original design) would be even simpler. Blazor Server adds a runtime dependency and SignalR connection overhead. However, Blazor provides: (1) data binding from `data.json` so you don't hand-edit HTML, (2) component reuse for the heatmap cells, and (3) a path to adding interactivity later if needed. The overhead is justified.

### Trade-off: No Client-Side Interactivity

**Accepted trade-off:** Blazor Server round-trips to the server for every interaction. For a screenshot tool with no user interaction, this is irrelevant. If tooltips or drill-down are added later, latency could be noticeable — but that's a future concern.

---

## 7. Open Questions

1. **How many months should the heatmap display?** The original design shows 4 months (Jan–Apr). Should this be configurable in `data.json`? Recommendation: Yes, make it data-driven with `"months": ["Jan","Feb","Mar","Apr"]`.

2. **How many milestone tracks?** The original shows 3 tracks (M1, M2, M3). Should the number of tracks be fixed or dynamic from data? Recommendation: Dynamic — loop over whatever tracks exist in `data.json`.

3. **Should the "current month" highlighting be automatic or configured?** The design highlights April with a different background color. Options: (a) auto-detect from system date, (b) explicit `"currentMonthIndex": 3` in JSON. Recommendation: Explicit in JSON, since the dashboard may be prepared days before the presentation.

4. **What fictional project should the example data represent?** Need a project name, 3-4 milestones, and ~20 status items across the 4 categories. Recommendation: Use something like "Project Phoenix — Cloud Migration Platform" with realistic-sounding work items.

5. **Should the "ADO Backlog" link in the header be functional?** The original design has a link. For a local tool, this could link to a real Azure DevOps URL or be purely decorative. Recommendation: Make it a configurable URL in `data.json`; if empty, don't render the link.

6. **Print / export workflow?** If screenshots are the primary output, should the app include a "screenshot mode" that hides the browser chrome? Recommendation: No — use Chrome's `--window-size=1920,1080 --screenshot` flag or the DevTools capture feature. Don't build screenshot logic into the app.

---

## 8. Implementation Recommendations

### Phase 1: Scaffold & Static Layout (Day 1)

**Goal:** Reproduce the `OriginalDesignConcept.html` design as a Blazor app with hardcoded data.

1. `dotnet new blazorserver -n ReportingDashboard --framework net8.0`
2. Delete all default pages (Counter, Weather, etc.)
3. Copy the CSS from `OriginalDesignConcept.html` into `app.css`
4. Create `Dashboard.razor` as the single page at route `/`
5. Create `Header.razor`, `Timeline.razor`, `HeatmapGrid.razor`, `HeatmapCell.razor`
6. Hardcode sample data directly in the components
7. **Verify:** Visual diff against the original design at 1920×1080

**Quick win:** At the end of Phase 1, you have a working screenshot-grade dashboard.

### Phase 2: Data Model & JSON Binding (Day 1-2)

**Goal:** Replace hardcoded data with `data.json`.

1. Define C# record types in `Models/ProjectData.cs`
2. Create `Services/ProjectDataService.cs` that reads `wwwroot/data/data.json`
3. Register as singleton in `Program.cs`
4. Inject into `Dashboard.razor` and pass data to child components
5. Create example `data.json` with fictional project data
6. **Verify:** Changing `data.json` and restarting the app updates the dashboard

### Phase 3: Dynamic SVG Timeline (Day 2)

**Goal:** Render the timeline from data instead of hardcoded SVG coordinates.

1. Implement date-to-pixel mapping in `Timeline.razor` code-behind
2. Support configurable date range (derived from earliest/latest milestone dates, with padding)
3. Render month gridlines, track lines, milestone markers, and NOW line dynamically
4. **Verify:** Add/remove milestones in `data.json` and confirm the timeline updates correctly

### Phase 4: Polish & Fictitious Data (Day 2-3)

**Goal:** Final visual polish and a compelling example dataset.

1. Fine-tune spacing, font sizes, and colors to match the design exactly
2. Add CSS drop shadows on milestone diamonds (SVG `<filter>` as in the original)
3. Create a rich example `data.json` for "Project Phoenix" with 20+ status items
4. Test screenshot workflow: open in Chrome → DevTools → set viewport 1920×1080 → capture
5. **Verify:** Screenshot is presentation-ready for PowerPoint

### Phase 5 (Optional): Improvements Over Original Design

**Goal:** Tasteful enhancements that make the dashboard better than the HTML original.

- Add subtle CSS transitions for a polished feel (not needed for screenshots but nice for live demos)
- Add a print stylesheet (`@media print`) that removes margins for clean PDF export
- Add a `"lastUpdated"` timestamp in the footer from `data.json`
- Support a second page/tab for a different project (stretch goal, likely unnecessary)

### What NOT to Build

- ❌ Database (SQLite, SQL Server, etc.) — `data.json` is sufficient
- ❌ Authentication — local-only tool
- ❌ API controllers — Blazor components read data directly from the service
- ❌ Entity Framework — no database means no ORM
- ❌ SignalR hubs — the default Blazor circuit is enough
- ❌ Docker containerization — `dotnet run` is simpler
- ❌ CI/CD pipeline — single developer, local tool
- ❌ Logging infrastructure — `Console.WriteLine` is fine for debugging
- ❌ CSS framework (Bootstrap, Tailwind) — the design is custom and fixed-width
- ❌ JavaScript charting library — raw SVG in Blazor is cleaner for this design
- ❌ Component library (MudBlazor, Radzen) — adds complexity without benefit for a custom layout

---

## Appendix: Key NuGet Packages Summary

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.Components` | 8.0.x (implicit) | Blazor Server framework | Yes (included in SDK) |
| `System.Text.Json` | 8.0.x (implicit) | JSON deserialization | Yes (included in SDK) |
| `xunit` | 2.7.x | Unit testing | Optional |
| `bunit` | 1.25.x | Blazor component testing | Optional |
| `FluentAssertions` | 6.12.x | Readable test assertions | Optional |

**Total external NuGet dependencies for the core app: 0.** Everything needed is included in the .NET 8 SDK.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/ddb5c02953f64c69f63e52a53dfffd4425831829/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
