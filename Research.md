# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-21 06:34 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, and categorizes work items into Shipped, In Progress, Carryover, and Blockers via a color-coded heatmap grid. Data is driven entirely from a local `data.json` file. **Primary Recommendation:** Build this as a minimal Blazor Server application with zero external database dependencies. Use `System.Text.Json` for data loading, inline SVG for the timeline, and pure CSS Grid/Flexbox for layout — mirroring the original HTML design exactly. No charting library is needed; the design is simple enough to implement with raw SVG and CSS. The entire solution should be 3-5 files beyond scaffolding, optimized for `dotnet run` → screenshot workflow. --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.App` | 8.0.x | Blazor Server runtime | ✅ Implicit | | `System.Text.Json` | 8.0.x | JSON deserialization | ✅ Included in framework | | `bunit` | 1.25+ | Component testing | Optional (tests) | | `xunit` | 2.7+ | Test framework | Optional (tests) | | `FluentAssertions` | 6.12+ | Test assertions | Optional (tests) | | `Microsoft.Playwright` | 1.41+ | Automated screenshots | Optional (Phase 4) | --- *Research completed: April 2026. All recommendations target .NET 8 LTS (supported through November 2026). The solution is intentionally minimal — a reporting dashboard that loads JSON and renders HTML/CSS/SVG, nothing more.*

### Key Findings

- The original design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) and **Flexbox** for all layout — no charting library is involved. This maps directly to Blazor components with scoped CSS.
- The timeline/Gantt section is **hand-authored SVG** with lines, circles, diamonds, and text — not a chart library output. This is straightforward to generate from data in a Blazor component using `@foreach` loops over milestone data.
- The color palette is small and fixed (greens for shipped, blues for in-progress, ambers for carryover, reds for blockers). This maps perfectly to CSS classes with no theming engine needed.
- A `data.json` file is the only data source. No database, no API, no authentication. `System.Text.Json` deserialization at startup is sufficient.
- The target resolution is **1920×1080** for screenshot capture. The page should use fixed pixel widths (matching the original `body{width:1920px;height:1080px;overflow:hidden}`) to guarantee pixel-perfect screenshots.
- Blazor Server is slightly over-engineered for a static data display page, but it provides a clean component model, hot reload during development, and easy future extensibility if the user wants filtering or interactivity later.
- No existing Blazor component library (MudBlazor, Radzen, etc.) is needed or recommended — they would add weight and fight against the precise pixel-level design already specified in the HTML template.
- The font **Segoe UI** is a Windows system font, so no web font loading is needed for local use. --- **Goal:** Reproduce the `OriginalDesignConcept.html` as a Blazor page with hardcoded data.
- `dotnet new blazor -n ReportingDashboard.Web --interactivity None` (Static SSR, no SignalR)
- Create `MainLayout.razor` — strip default nav, full-width body
- Create `Dashboard.razor` — port the HTML structure verbatim
- Copy all CSS from the HTML template into `app.css` or scoped `.razor.css` files
- Hardcode the SVG timeline and heatmap data
- Verify 1920×1080 screenshot matches the original design
- **Deliverable:** Running app that looks identical to the HTML template **Goal:** Replace hardcoded data with `data.json`-driven components.
- Define C# models (`DashboardData`, `TimelineTrack`, `Milestone`, `HeatmapCategory`)
- Create `data.json` with fictional project data (e.g., "Project Phoenix — Cloud Migration")
- Build `JsonFileDataService` — singleton, reads file on startup
- Refactor `Dashboard.razor` to loop over model data
- Implement `DateToX()` calculation for SVG milestone positioning
- Implement dynamic month columns in heatmap grid
- **Deliverable:** Change `data.json`, refresh browser, see updated dashboard **Goal:** Improve upon the original design while maintaining executive-friendly simplicity. Suggested improvements over the original HTML:
- **Progress indicators:** Add a small percentage or fraction next to "In Progress" items (e.g., "3/5 tasks")
- **Today marker:** Make the "NOW" line position computed from `currentDate` instead of hardcoded
- **Milestone tooltips:** CSS-only tooltips on hover showing full milestone description
- **Print/screenshot optimization:** Add a `@media print` stylesheet that hides browser chrome
- **Color-blind accessibility:** Add subtle patterns (hatching) in addition to colors for the 4 categories
- **Summary counts:** Add a small badge in each row header showing item count (e.g., "SHIPPED (7)")
- **Automated screenshots:** Add `Microsoft.Playwright` (NuGet: `Microsoft.Playwright`, v1.41+) to generate PNGs via `dotnet run -- --screenshot output.png`
- **Multiple projects:** Support a dropdown or command-line switch to load different `data.json` files (e.g., `data.project-phoenix.json`, `data.privacy-automation.json`)
- **FileSystemWatcher:** Auto-refresh the browser when `data.json` is saved, enabling live editing workflow
- **Dark mode:** Add a `prefers-color-scheme: dark` media query variant for on-screen viewing (keep light mode for screenshots)
- **Copy-paste the CSS** from `OriginalDesignConcept.html` — this gets you 80% of the visual fidelity in minutes.
- **Use `dotnet watch`** — instant feedback loop while tweaking layout.
- **Use Static SSR mode** (`--interactivity None`) — eliminates SignalR complexity and produces the fastest page load.
- **Generate sample `data.json` first** — agree on the schema before building components. The schema IS the contract.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (.NET 8) | `net8.0` | Ships with `Microsoft.AspNetCore.App` — no extra NuGet package | | **Layout** | Pure CSS Grid + Flexbox | N/A | Matches original design exactly; no CSS framework needed | | **Timeline Visualization** | Inline SVG via Razor markup | N/A | Generate `<svg>`, `<line>`, `<polygon>`, `<circle>`, `<text>` elements from data model | | **Heatmap Grid** | CSS Grid with semantic HTML | N/A | `grid-template-columns: 160px repeat(N,1fr)` where N = number of months | | **Icons/Markers** | CSS transforms + SVG shapes | N/A | Diamond = `transform:rotate(45deg)` on a square `<span>`, matching the design | | **CSS Architecture** | Blazor CSS isolation (`.razor.css`) | Built-in | One scoped CSS file per component; no Sass/PostCSS needed | The design is a fixed-layout, screenshot-optimized page. Component libraries introduce CSS resets, theme systems, and layout opinions that conflict with the pixel-precise design. The effort to override a library's styles exceeds the effort of writing ~150 lines of CSS from the HTML template. The timeline is not a chart — it's a horizontal SVG with positioned markers at calculated x-coordinates. A charting library would impose axes, legends, and responsive behavior that fight the fixed-resolution design. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Ships with .NET 8 | Native, fast, zero dependencies | | **Data Service** | Singleton `IDataService` | Custom | Reads `data.json` at startup, provides strongly-typed model to components | | **File Watching (optional)** | `FileSystemWatcher` | .NET BCL | Auto-reload `data.json` on save without restarting the app | | **Configuration** | `appsettings.json` | Built-in | Path to `data.json`, app title override, etc. | | Layer | Technology | Notes | |-------|-----------|-------| | **Primary Store** | `data.json` file on disk | No database. Period. | | **Schema Validation** | C# POCO model with `System.Text.Json` attributes | `JsonPropertyName`, `JsonRequired` for safety | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Tests** | xUnit | 2.7+ | .NET ecosystem standard | | **Blazor Component Tests** | bUnit | 1.25+ (`bunit` on NuGet) | Test components in isolation without browser | | **Assertions** | FluentAssertions | 6.12+ | Readable test assertions | | **Snapshot Testing (optional)** | Verify | 23.x | Useful for verifying SVG output hasn't regressed | | Tool | Version | Purpose | |------|---------|---------| | **.NET SDK** | 8.0.x (latest patch) | Build and run | | **Visual Studio 2022** or **VS Code + C# Dev Kit** | Latest | IDE | | **dotnet watch** | Built into SDK | Hot reload during development | | **Browser DevTools** | Any Chromium | Inspect CSS Grid, verify 1920×1080 rendering | ---
```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard.Web/
│       ├── ReportingDashboard.Web.csproj      # Blazor Server app
│       ├── Program.cs                          # Minimal hosting, register services
│       ├── appsettings.json                    # dataFilePath: "./data.json"
│       ├── data.json                           # THE data source
│       ├── Models/
│       │   ├── DashboardData.cs                # Root model
│       │   ├── Milestone.cs                    # Timeline milestones
│       │   ├── HeatmapCategory.cs              # Shipped/InProgress/Carryover/Blockers
│       │   └── WorkItem.cs                     # Individual items in heatmap cells
│       ├── Services/
│       │   ├── IDataService.cs
│       │   └── JsonFileDataService.cs          # Reads & deserializes data.json
│       ├── Components/
│       │   ├── Pages/
│       │   │   └── Dashboard.razor             # The single page (route: "/")
│       │   ├── Layout/
│       │   │   └── MainLayout.razor            # Minimal shell, no nav
│       │   ├── DashboardHeader.razor           # Title, subtitle, legend
│       │   ├── TimelineSection.razor           # SVG timeline with milestones
│       │   ├── HeatmapGrid.razor               # CSS Grid heatmap
│       │   └── HeatmapCell.razor               # Individual cell with work items
│       └── wwwroot/
│           └── css/
│               └── app.css                     # Global resets + body sizing
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        ├── Services/
        │   └── JsonFileDataServiceTests.cs
        └── Components/
            └── TimelineSectionTests.cs
``` Map the HTML design sections directly to Blazor components:
```
Dashboard.razor (page)
├── DashboardHeader.razor          → .hdr div
│   └── Legend (inline)            → legend spans
├── TimelineSection.razor          → .tl-area div
│   ├── MilestoneLabels (inline)   → left sidebar with M1, M2, M3
│   └── TimelineSvg (inline)      → <svg> with computed positions
└── HeatmapGrid.razor             → .hm-wrap + .hm-grid
    ├── Column Headers (inline)    → .hm-col-hdr cells
    └── @foreach category:
        ├── Row Header             → .hm-row-hdr
        └── @foreach month:
            └── HeatmapCell.razor  → .hm-cell with work items
```
```
data.json  →  JsonFileDataService (Singleton)  →  Dashboard.razor (OnInitialized)
                                                      ↓
                                               Child Components (via [Parameter])
``` **No state management library needed.** Data flows one direction: file → service → page → child components via `[Parameter]` attributes. There is no user interaction that mutates state. The critical algorithmic piece is mapping dates to x-coordinates in the SVG:
```csharp
// In TimelineSection.razor
private double DateToX(DateTime date)
{
    double totalDays = (TimelineEnd - TimelineStart).TotalDays;
    double dayOffset = (date - TimelineStart).TotalDays;
    return (dayOffset / totalDays) * SvgWidth;
}
``` Where `SvgWidth` matches the original design (~1560px), and `TimelineStart`/`TimelineEnd` are derived from the data's date range. Months should be computed from the data, not hardcoded:
```csharp
var months = data.Categories
    .SelectMany(c => c.Items)
    .Select(i => new DateTime(i.Date.Year, i.Date.Month, 1))
    .Distinct()
    .OrderBy(d => d)
    .ToList();
``` The CSS Grid `grid-template-columns` should be set dynamically: `160px repeat({months.Count}, 1fr)`.
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-10",
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonth": "Apr",
    "categories": [
      {
        "name": "Shipped",
        "colorClass": "ship",
        "items": {
          "Jan": ["Item A completed", "Item B completed"],
          "Feb": ["Item C completed"],
          "Mar": [],
          "Apr": ["Item D completed"]
        }
      },
      {
        "name": "In Progress",
        "colorClass": "prog",
        "items": { ... }
      },
      {
        "name": "Carryover",
        "colorClass": "carry",
        "items": { ... }
      },
      {
        "name": "Blockers",
        "colorClass": "block",
        "items": { ... }
      }
    ]
  }
}
``` ---

### Considerations & Risks

- **None.** This is an intentional design choice. The dashboard runs locally and is used to generate screenshots for PowerPoint. Adding auth would be wasted complexity.
- If future sharing is needed, the simplest option is `dotnet publish` → copy folder → `dotnet ReportingDashboard.Web.dll` on any Windows machine.
- `data.json` is a local file. No encryption needed.
- If the data contains sensitive project names, the user controls physical access to the machine.
- **Do not commit `data.json` with real project data to a public repo.** Include a `data.sample.json` and add `data.json` to `.gitignore`.
- **Local only.** `dotnet run` or `dotnet watch` during development.
- For "production" use: `dotnet publish -c Release -o ./publish` → run the executable.
- No Docker, no IIS, no reverse proxy needed.
- Kestrel (built into ASP.NET Core) serves the page on `https://localhost:5001` or `http://localhost:5000`.
- Open `http://localhost:5000` in Chrome/Edge.
- Browser window at 1920×1080 (use DevTools device toolbar or `--window-size=1920,1080` flag).
- Full-page screenshot via DevTools (`Ctrl+Shift+P` → "Capture full size screenshot") or Snipping Tool.
- **Optional enhancement:** Add a `/screenshot` endpoint using Playwright for automated PNG generation (see Implementation Recommendations).
- **$0.** Runs on the developer's existing Windows machine. ---
- **Impact:** Medium. Blazor Server maintains a SignalR WebSocket connection per client, which is unnecessary for a page with no interactivity.
- **Mitigation:** This is acceptable for local, single-user use. The SignalR overhead is negligible. The benefit is a clean component model and hot reload during development.
- **Alternative considered:** Blazor Static SSR (new in .NET 8) renders HTML without SignalR. This would eliminate the WebSocket but also removes any future interactivity. **Recommendation: Use Blazor Static SSR (`@rendermode` not set or `@attribute [StreamRendering]`) since interactivity is not needed.** This is the simplest mode and produces pure HTML.
- **Impact:** High. The user needs pixel-accurate screenshots matching the design concept.
- **Mitigation:** Copy the CSS from `OriginalDesignConcept.html` verbatim into component CSS files. Do not abstract or "improve" the CSS until the baseline matches exactly. Test at 1920×1080 in the same browser used for screenshots.
- **Impact:** Medium. Date-to-pixel math rounding could misplace milestones.
- **Mitigation:** Use `double` for all coordinate calculations. Round only at the final render step (`x="{Math.Round(xPos, 1)}"`). Write unit tests for `DateToX()` with known date/expected-pixel pairs from the original design.
- **Impact:** Low. Only one user editing one file.
- **Mitigation:** Deserialize into strongly-typed C# models. Missing required fields throw at startup with clear error messages. Include a `data.sample.json` with documentation comments.
- **Impact:** Low. Segoe UI is a Windows system font and will render identically across Windows machines.
- **Mitigation:** None needed for local Windows use. If ever served to macOS/Linux users, add a fallback: `font-family: 'Segoe UI', -apple-system, 'Helvetica Neue', Arial, sans-serif`.
- **Pro:** Zero setup, zero dependencies, instant startup, trivially portable.
- **Con:** No history tracking, no concurrent editing, no audit trail.
- **Verdict:** Correct choice for this use case. The user edits `data.json` in a text editor and takes a screenshot. A database would add complexity with no benefit.
- **Pro:** Full control over pixel-level output. No CSS conflicts. Tiny bundle size. Fast load.
- **Con:** No pre-built date pickers, tooltips, or modals if features expand later.
- **Verdict:** Correct choice. The deliverable is a screenshot, not an interactive app. ---
- **How many months should the heatmap display?** The original design shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always show a fixed window (e.g., current month ± 3)?
- **How many timeline tracks are expected?** The original shows 3 (M1, M2, M3). The SVG height calculation depends on this. Should it auto-scale, or is 3-5 tracks the expected max?
- **Should the "current month" highlighting (amber column headers, darker cell backgrounds) be automatic based on system date, or specified in `data.json`?** Recommendation: Derive from `currentDate` in `data.json` so screenshots are reproducible.
- **Is the ADO Backlog link functional or decorative?** If functional, it just needs to be an `<a href>` to the URL in `data.json`. No API integration needed.
- **Will you want to generate screenshots programmatically (e.g., run a command that produces a PNG), or is manual browser screenshot sufficient?** This affects whether we add Playwright as an optional tool.
- **Should `data.json` live alongside the app binary, or in a user-specified path (e.g., `--data C:\Reports\q2.json`)?** Recommendation: Support both — default to `./data.json`, override via `appsettings.json` or command-line argument. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, and categorizes work items into Shipped, In Progress, Carryover, and Blockers via a color-coded heatmap grid. Data is driven entirely from a local `data.json` file.

**Primary Recommendation:** Build this as a minimal Blazor Server application with zero external database dependencies. Use `System.Text.Json` for data loading, inline SVG for the timeline, and pure CSS Grid/Flexbox for layout — mirroring the original HTML design exactly. No charting library is needed; the design is simple enough to implement with raw SVG and CSS. The entire solution should be 3-5 files beyond scaffolding, optimized for `dotnet run` → screenshot workflow.

---

## 2. Key Findings

- The original design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) and **Flexbox** for all layout — no charting library is involved. This maps directly to Blazor components with scoped CSS.
- The timeline/Gantt section is **hand-authored SVG** with lines, circles, diamonds, and text — not a chart library output. This is straightforward to generate from data in a Blazor component using `@foreach` loops over milestone data.
- The color palette is small and fixed (greens for shipped, blues for in-progress, ambers for carryover, reds for blockers). This maps perfectly to CSS classes with no theming engine needed.
- A `data.json` file is the only data source. No database, no API, no authentication. `System.Text.Json` deserialization at startup is sufficient.
- The target resolution is **1920×1080** for screenshot capture. The page should use fixed pixel widths (matching the original `body{width:1920px;height:1080px;overflow:hidden}`) to guarantee pixel-perfect screenshots.
- Blazor Server is slightly over-engineered for a static data display page, but it provides a clean component model, hot reload during development, and easy future extensibility if the user wants filtering or interactivity later.
- No existing Blazor component library (MudBlazor, Radzen, etc.) is needed or recommended — they would add weight and fight against the precise pixel-level design already specified in the HTML template.
- The font **Segoe UI** is a Windows system font, so no web font loading is needed for local use.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components + CSS)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (.NET 8) | `net8.0` | Ships with `Microsoft.AspNetCore.App` — no extra NuGet package |
| **Layout** | Pure CSS Grid + Flexbox | N/A | Matches original design exactly; no CSS framework needed |
| **Timeline Visualization** | Inline SVG via Razor markup | N/A | Generate `<svg>`, `<line>`, `<polygon>`, `<circle>`, `<text>` elements from data model |
| **Heatmap Grid** | CSS Grid with semantic HTML | N/A | `grid-template-columns: 160px repeat(N,1fr)` where N = number of months |
| **Icons/Markers** | CSS transforms + SVG shapes | N/A | Diamond = `transform:rotate(45deg)` on a square `<span>`, matching the design |
| **CSS Architecture** | Blazor CSS isolation (`.razor.css`) | Built-in | One scoped CSS file per component; no Sass/PostCSS needed |

**Why no component library (MudBlazor, Radzen, Syncfusion, etc.):**
The design is a fixed-layout, screenshot-optimized page. Component libraries introduce CSS resets, theme systems, and layout opinions that conflict with the pixel-precise design. The effort to override a library's styles exceeds the effort of writing ~150 lines of CSS from the HTML template.

**Why no charting library (ChartJs.Blazor, ApexCharts.Blazor, etc.):**
The timeline is not a chart — it's a horizontal SVG with positioned markers at calculated x-coordinates. A charting library would impose axes, legends, and responsive behavior that fight the fixed-resolution design.

### Backend (Data Loading)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Ships with .NET 8 | Native, fast, zero dependencies |
| **Data Service** | Singleton `IDataService` | Custom | Reads `data.json` at startup, provides strongly-typed model to components |
| **File Watching (optional)** | `FileSystemWatcher` | .NET BCL | Auto-reload `data.json` on save without restarting the app |
| **Configuration** | `appsettings.json` | Built-in | Path to `data.json`, app title override, etc. |

### Data Storage

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Primary Store** | `data.json` file on disk | No database. Period. |
| **Schema Validation** | C# POCO model with `System.Text.Json` attributes | `JsonPropertyName`, `JsonRequired` for safety |

### Testing

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Tests** | xUnit | 2.7+ | .NET ecosystem standard |
| **Blazor Component Tests** | bUnit | 1.25+ (`bunit` on NuGet) | Test components in isolation without browser |
| **Assertions** | FluentAssertions | 6.12+ | Readable test assertions |
| **Snapshot Testing (optional)** | Verify | 23.x | Useful for verifying SVG output hasn't regressed |

### Development Tools

| Tool | Version | Purpose |
|------|---------|---------|
| **.NET SDK** | 8.0.x (latest patch) | Build and run |
| **Visual Studio 2022** or **VS Code + C# Dev Kit** | Latest | IDE |
| **dotnet watch** | Built into SDK | Hot reload during development |
| **Browser DevTools** | Any Chromium | Inspect CSS Grid, verify 1920×1080 rendering |

---

## 4. Architecture Recommendations

### 4.1 Solution Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard.Web/
│       ├── ReportingDashboard.Web.csproj      # Blazor Server app
│       ├── Program.cs                          # Minimal hosting, register services
│       ├── appsettings.json                    # dataFilePath: "./data.json"
│       ├── data.json                           # THE data source
│       ├── Models/
│       │   ├── DashboardData.cs                # Root model
│       │   ├── Milestone.cs                    # Timeline milestones
│       │   ├── HeatmapCategory.cs              # Shipped/InProgress/Carryover/Blockers
│       │   └── WorkItem.cs                     # Individual items in heatmap cells
│       ├── Services/
│       │   ├── IDataService.cs
│       │   └── JsonFileDataService.cs          # Reads & deserializes data.json
│       ├── Components/
│       │   ├── Pages/
│       │   │   └── Dashboard.razor             # The single page (route: "/")
│       │   ├── Layout/
│       │   │   └── MainLayout.razor            # Minimal shell, no nav
│       │   ├── DashboardHeader.razor           # Title, subtitle, legend
│       │   ├── TimelineSection.razor           # SVG timeline with milestones
│       │   ├── HeatmapGrid.razor               # CSS Grid heatmap
│       │   └── HeatmapCell.razor               # Individual cell with work items
│       └── wwwroot/
│           └── css/
│               └── app.css                     # Global resets + body sizing
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        ├── Services/
        │   └── JsonFileDataServiceTests.cs
        └── Components/
            └── TimelineSectionTests.cs
```

### 4.2 Component Architecture

Map the HTML design sections directly to Blazor components:

```
Dashboard.razor (page)
├── DashboardHeader.razor          → .hdr div
│   └── Legend (inline)            → legend spans
├── TimelineSection.razor          → .tl-area div
│   ├── MilestoneLabels (inline)   → left sidebar with M1, M2, M3
│   └── TimelineSvg (inline)      → <svg> with computed positions
└── HeatmapGrid.razor             → .hm-wrap + .hm-grid
    ├── Column Headers (inline)    → .hm-col-hdr cells
    └── @foreach category:
        ├── Row Header             → .hm-row-hdr
        └── @foreach month:
            └── HeatmapCell.razor  → .hm-cell with work items
```

### 4.3 Data Flow Pattern

```
data.json  →  JsonFileDataService (Singleton)  →  Dashboard.razor (OnInitialized)
                                                      ↓
                                               Child Components (via [Parameter])
```

**No state management library needed.** Data flows one direction: file → service → page → child components via `[Parameter]` attributes. There is no user interaction that mutates state.

### 4.4 SVG Timeline Coordinate Calculation

The critical algorithmic piece is mapping dates to x-coordinates in the SVG:

```csharp
// In TimelineSection.razor
private double DateToX(DateTime date)
{
    double totalDays = (TimelineEnd - TimelineStart).TotalDays;
    double dayOffset = (date - TimelineStart).TotalDays;
    return (dayOffset / totalDays) * SvgWidth;
}
```

Where `SvgWidth` matches the original design (~1560px), and `TimelineStart`/`TimelineEnd` are derived from the data's date range.

### 4.5 Heatmap Dynamic Column Generation

Months should be computed from the data, not hardcoded:

```csharp
var months = data.Categories
    .SelectMany(c => c.Items)
    .Select(i => new DateTime(i.Date.Year, i.Date.Month, 1))
    .Distinct()
    .OrderBy(d => d)
    .ToList();
```

The CSS Grid `grid-template-columns` should be set dynamically: `160px repeat({months.Count}, 1fr)`.

### 4.6 `data.json` Schema Design

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-10",
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonth": "Apr",
    "categories": [
      {
        "name": "Shipped",
        "colorClass": "ship",
        "items": {
          "Jan": ["Item A completed", "Item B completed"],
          "Feb": ["Item C completed"],
          "Mar": [],
          "Apr": ["Item D completed"]
        }
      },
      {
        "name": "In Progress",
        "colorClass": "prog",
        "items": { ... }
      },
      {
        "name": "Carryover",
        "colorClass": "carry",
        "items": { ... }
      },
      {
        "name": "Blockers",
        "colorClass": "block",
        "items": { ... }
      }
    ]
  }
}
```

---

## 5. Security & Infrastructure

### Authentication & Authorization
- **None.** This is an intentional design choice. The dashboard runs locally and is used to generate screenshots for PowerPoint. Adding auth would be wasted complexity.
- If future sharing is needed, the simplest option is `dotnet publish` → copy folder → `dotnet ReportingDashboard.Web.dll` on any Windows machine.

### Data Protection
- `data.json` is a local file. No encryption needed.
- If the data contains sensitive project names, the user controls physical access to the machine.
- **Do not commit `data.json` with real project data to a public repo.** Include a `data.sample.json` and add `data.json` to `.gitignore`.

### Hosting & Deployment
- **Local only.** `dotnet run` or `dotnet watch` during development.
- For "production" use: `dotnet publish -c Release -o ./publish` → run the executable.
- No Docker, no IIS, no reverse proxy needed.
- Kestrel (built into ASP.NET Core) serves the page on `https://localhost:5001` or `http://localhost:5000`.

### Screenshot Workflow
- Open `http://localhost:5000` in Chrome/Edge.
- Browser window at 1920×1080 (use DevTools device toolbar or `--window-size=1920,1080` flag).
- Full-page screenshot via DevTools (`Ctrl+Shift+P` → "Capture full size screenshot") or Snipping Tool.
- **Optional enhancement:** Add a `/screenshot` endpoint using Playwright for automated PNG generation (see Implementation Recommendations).

### Infrastructure Costs
- **$0.** Runs on the developer's existing Windows machine.

---

## 6. Risks & Trade-offs

### Risk: Blazor Server Overhead for a Static Page
- **Impact:** Medium. Blazor Server maintains a SignalR WebSocket connection per client, which is unnecessary for a page with no interactivity.
- **Mitigation:** This is acceptable for local, single-user use. The SignalR overhead is negligible. The benefit is a clean component model and hot reload during development.
- **Alternative considered:** Blazor Static SSR (new in .NET 8) renders HTML without SignalR. This would eliminate the WebSocket but also removes any future interactivity. **Recommendation: Use Blazor Static SSR (`@rendermode` not set or `@attribute [StreamRendering]`) since interactivity is not needed.** This is the simplest mode and produces pure HTML.

### Risk: CSS Fidelity to Original Design
- **Impact:** High. The user needs pixel-accurate screenshots matching the design concept.
- **Mitigation:** Copy the CSS from `OriginalDesignConcept.html` verbatim into component CSS files. Do not abstract or "improve" the CSS until the baseline matches exactly. Test at 1920×1080 in the same browser used for screenshots.

### Risk: SVG Coordinate Precision
- **Impact:** Medium. Date-to-pixel math rounding could misplace milestones.
- **Mitigation:** Use `double` for all coordinate calculations. Round only at the final render step (`x="{Math.Round(xPos, 1)}"`). Write unit tests for `DateToX()` with known date/expected-pixel pairs from the original design.

### Risk: data.json Schema Drift
- **Impact:** Low. Only one user editing one file.
- **Mitigation:** Deserialize into strongly-typed C# models. Missing required fields throw at startup with clear error messages. Include a `data.sample.json` with documentation comments.

### Risk: Font Rendering Differences
- **Impact:** Low. Segoe UI is a Windows system font and will render identically across Windows machines.
- **Mitigation:** None needed for local Windows use. If ever served to macOS/Linux users, add a fallback: `font-family: 'Segoe UI', -apple-system, 'Helvetica Neue', Arial, sans-serif`.

### Trade-off: No Database
- **Pro:** Zero setup, zero dependencies, instant startup, trivially portable.
- **Con:** No history tracking, no concurrent editing, no audit trail.
- **Verdict:** Correct choice for this use case. The user edits `data.json` in a text editor and takes a screenshot. A database would add complexity with no benefit.

### Trade-off: No Component Library
- **Pro:** Full control over pixel-level output. No CSS conflicts. Tiny bundle size. Fast load.
- **Con:** No pre-built date pickers, tooltips, or modals if features expand later.
- **Verdict:** Correct choice. The deliverable is a screenshot, not an interactive app.

---

## 7. Open Questions

1. **How many months should the heatmap display?** The original design shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always show a fixed window (e.g., current month ± 3)?

2. **How many timeline tracks are expected?** The original shows 3 (M1, M2, M3). The SVG height calculation depends on this. Should it auto-scale, or is 3-5 tracks the expected max?

3. **Should the "current month" highlighting (amber column headers, darker cell backgrounds) be automatic based on system date, or specified in `data.json`?** Recommendation: Derive from `currentDate` in `data.json` so screenshots are reproducible.

4. **Is the ADO Backlog link functional or decorative?** If functional, it just needs to be an `<a href>` to the URL in `data.json`. No API integration needed.

5. **Will you want to generate screenshots programmatically (e.g., run a command that produces a PNG), or is manual browser screenshot sufficient?** This affects whether we add Playwright as an optional tool.

6. **Should `data.json` live alongside the app binary, or in a user-specified path (e.g., `--data C:\Reports\q2.json`)?** Recommendation: Support both — default to `./data.json`, override via `appsettings.json` or command-line argument.

---

## 8. Implementation Recommendations

### Phase 1: Pixel-Perfect Static Replica (Day 1)
**Goal:** Reproduce the `OriginalDesignConcept.html` as a Blazor page with hardcoded data.

1. `dotnet new blazor -n ReportingDashboard.Web --interactivity None` (Static SSR, no SignalR)
2. Create `MainLayout.razor` — strip default nav, full-width body
3. Create `Dashboard.razor` — port the HTML structure verbatim
4. Copy all CSS from the HTML template into `app.css` or scoped `.razor.css` files
5. Hardcode the SVG timeline and heatmap data
6. Verify 1920×1080 screenshot matches the original design
7. **Deliverable:** Running app that looks identical to the HTML template

### Phase 2: Data-Driven Rendering (Day 1-2)
**Goal:** Replace hardcoded data with `data.json`-driven components.

1. Define C# models (`DashboardData`, `TimelineTrack`, `Milestone`, `HeatmapCategory`)
2. Create `data.json` with fictional project data (e.g., "Project Phoenix — Cloud Migration")
3. Build `JsonFileDataService` — singleton, reads file on startup
4. Refactor `Dashboard.razor` to loop over model data
5. Implement `DateToX()` calculation for SVG milestone positioning
6. Implement dynamic month columns in heatmap grid
7. **Deliverable:** Change `data.json`, refresh browser, see updated dashboard

### Phase 3: Design Improvements (Day 2-3)
**Goal:** Improve upon the original design while maintaining executive-friendly simplicity.

Suggested improvements over the original HTML:
- **Progress indicators:** Add a small percentage or fraction next to "In Progress" items (e.g., "3/5 tasks")
- **Today marker:** Make the "NOW" line position computed from `currentDate` instead of hardcoded
- **Milestone tooltips:** CSS-only tooltips on hover showing full milestone description
- **Print/screenshot optimization:** Add a `@media print` stylesheet that hides browser chrome
- **Color-blind accessibility:** Add subtle patterns (hatching) in addition to colors for the 4 categories
- **Summary counts:** Add a small badge in each row header showing item count (e.g., "SHIPPED (7)")

### Phase 4: Optional Enhancements (Future)
- **Automated screenshots:** Add `Microsoft.Playwright` (NuGet: `Microsoft.Playwright`, v1.41+) to generate PNGs via `dotnet run -- --screenshot output.png`
- **Multiple projects:** Support a dropdown or command-line switch to load different `data.json` files (e.g., `data.project-phoenix.json`, `data.privacy-automation.json`)
- **FileSystemWatcher:** Auto-refresh the browser when `data.json` is saved, enabling live editing workflow
- **Dark mode:** Add a `prefers-color-scheme: dark` media query variant for on-screen viewing (keep light mode for screenshots)

### Quick Wins
1. **Copy-paste the CSS** from `OriginalDesignConcept.html` — this gets you 80% of the visual fidelity in minutes.
2. **Use `dotnet watch`** — instant feedback loop while tweaking layout.
3. **Use Static SSR mode** (`--interactivity None`) — eliminates SignalR complexity and produces the fastest page load.
4. **Generate sample `data.json` first** — agree on the schema before building components. The schema IS the contract.

### NuGet Packages Summary

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.App` | 8.0.x | Blazor Server runtime | ✅ Implicit |
| `System.Text.Json` | 8.0.x | JSON deserialization | ✅ Included in framework |
| `bunit` | 1.25+ | Component testing | Optional (tests) |
| `xunit` | 2.7+ | Test framework | Optional (tests) |
| `FluentAssertions` | 6.12+ | Test assertions | Optional (tests) |
| `Microsoft.Playwright` | 1.41+ | Automated screenshots | Optional (Phase 4) |

**Total required NuGet packages beyond the default template: 0.**

---

*Research completed: April 2026. All recommendations target .NET 8 LTS (supported through November 2026). The solution is intentionally minimal — a reporting dashboard that loads JSON and renders HTML/CSS/SVG, nothing more.*

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/70de7a83d453bc2de0ded824b642c095db27f834/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
