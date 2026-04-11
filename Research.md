# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-11 18:59 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipping status, and progress in a format optimized for PowerPoint screenshot capture at 1920×1080. The architecture is intentionally simple: a local-only Blazor Server app that reads from a `data.json` configuration file—no database, no authentication, no cloud services. The primary technical challenge is faithfully reproducing the heatmap grid, SVG timeline, and color-coded status system from the HTML design reference using Blazor components with pure CSS and inline SVG. Given the simplicity requirements, the recommendation is a **single-project .sln** with minimal dependencies: just `System.Text.Json` for data loading and native Blazor rendering for all UI. No charting library is needed—the design is achievable with CSS Grid, Flexbox, and hand-crafted SVG, which gives pixel-perfect control for screenshot fidelity. ---

### Key Findings

- The original HTML design is entirely self-contained (no JS frameworks, no charting libs)—the Blazor implementation should mirror this simplicity using pure CSS and inline SVG rendered server-side.
- CSS Grid with `grid-template-columns: 160px repeat(4, 1fr)` and a 5-row layout (header + 4 status rows) is the core layout primitive for the heatmap section; Blazor can emit this markup directly.
- The SVG timeline uses basic primitives (lines, circles, polygons, text) with a drop-shadow filter—no charting library is needed; Blazor's `RenderTreeBuilder` or `.razor` markup can emit SVG natively.
- `System.Text.Json` (built into .NET 8) is sufficient for deserializing `data.json`; no need for Newtonsoft.Json or any ORM.
- Blazor Server's SignalR circuit is irrelevant here since there's no interactivity beyond page load—the app is essentially a server-rendered static page, making Blazor Server's overhead negligible.
- The color palette is fixed and small (15 colors), best implemented as CSS custom properties for maintainability and easy theming per-project.
- Screenshot fidelity at 1920×1080 requires a fixed `width: 1920px; height: 1080px; overflow: hidden` on the body, exactly as the original design does—this is non-negotiable for PowerPoint use.
- A `data.json` schema should capture: project metadata, milestones (with dates, types, positions), timeline tracks, and heatmap items (categorized by Shipped/In Progress/Carryover/Blockers × Month).
- Hot reload (`dotnet watch`) works out of the box with .NET 8 Blazor Server for rapid CSS/markup iteration.
- No NuGet packages beyond the default Blazor Server template are required. ---
- `dotnet new blazor --interactivity Server --no-https -n ReportingDashboard`
- Create the `data.json` schema and sample data for a fictional project
- Build `MainLayout.razor` with fixed 1920×1080 container
- Implement `DashboardHeader.razor` with title, subtitle, legend
- Port the CSS from the original HTML design into `app.css` using CSS custom properties
- **Deliverable**: Header renders correctly at target resolution
- Implement `HeatmapSection.razor` with CSS Grid layout
- Build `HeatmapRow.razor` and `HeatmapCell.razor` components
- Data-bind from `data.json` → heatmap categories → cells
- Match all 4 row color schemes (Shipped/In Progress/Carryover/Blockers)
- **Deliverable**: Full heatmap grid renders with sample data, visually matches reference
- Implement `TimelineSection.razor` with dynamic SVG generation
- Calculate milestone positions from `data.json` date/position values
- Render track lines, checkpoint circles, PoC diamonds, production diamonds
- Add the NOW vertical dashed line
- Add drop-shadow filter for diamond milestones
- **Deliverable**: Complete page renders matching reference design
- Fine-tune spacing, font sizes, colors against original design
- Add subtle improvements: hover tooltips (optional), smooth font rendering
- Test screenshot workflow end-to-end
- Write a `README.md` with instructions for editing `data.json` and taking screenshots
- **Deliverable**: Production-ready local tool
- **Start from the CSS, not the components**: Copy the original design's CSS verbatim into `app.css`, then wrap the HTML structure in Blazor components. This guarantees visual fidelity from the start.
- **Use `dotnet watch` from minute one**: Hot reload on .razor and .css files makes iteration instant.
- **Create `data.json` first**: Define the schema before writing any Blazor code. This clarifies the component data flow.
- **Browser DevTools Grid overlay**: Enable CSS Grid overlay in Edge/Chrome DevTools to debug the heatmap layout visually.
```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "#",
    "currentMonth": "Apr"
  },
  "timeline": {
    "months": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"],
    "nowPosition": 0.53
  },
  "tracks": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "milestones": [
        { "date": "Jan 12", "type": "checkpoint", "position": 0.067 },
        { "date": "Mar 26", "type": "poc", "position": 0.48, "label": "PoC" },
        { "date": "May 1", "type": "production", "position": 0.667, "label": "Prod (TBD)" }
      ]
    }
  ],
  "heatmap": {
    "months": ["January", "February", "March", "April"],
    "categories": [
      {
        "name": "Shipped",
        "cssClass": "ship",
        "items": {
          "January": ["Chatbot basic scaffolding", "MS Graph role sync"],
          "February": ["Intent recognition v1"],
          "March": ["PDS integration", "Bulk export API"],
          "April": ["Auto-DFD generator v1"]
        }
      }
    ]
  }
}
```
```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── Data/
│   │   └── data.json
│   ├── Models/
│   │   └── DashboardData.cs          # C# records for JSON deserialization
│   ├── Services/
│   │   └── DashboardDataService.cs    # Reads and deserializes data.json
│   ├── Components/
│   │   ├── App.razor
│   │   ├── Layout/
│   │   │   └── MainLayout.razor       # Fixed 1920×1080 container
│   │   └── Pages/
│   │       └── Dashboard.razor        # Main page
│   │       ├── DashboardHeader.razor
│   │       ├── TimelineSection.razor
│   │       ├── HeatmapSection.razor
│   │       ├── HeatmapRow.razor
│   │       └── HeatmapCell.razor
│   └── wwwroot/
│       └── css/
│           └── app.css                # All styles, CSS custom properties
└── README.md
``` This structure keeps the project flat and navigable—one solution, one project, clear separation of data/models/services/components. No over-engineering.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (.razor components) | .NET 8.0 (LTS) | Ships with `Microsoft.AspNetCore.App` | | **CSS Layout** | Pure CSS Grid + Flexbox | Native | Matches original design's layout approach exactly | | **SVG Rendering** | Inline SVG in .razor files | Native | Timeline milestones, diamonds, circles—no library needed | | **Font** | Segoe UI | System font | Available on all Windows machines; no web font loading | | **CSS Architecture** | Component-scoped CSS (`.razor.css`) + global `app.css` | Native Blazor | Scoped styles for components, global for design tokens | **Do NOT use**: MudBlazor, Radzen, Syncfusion, or any component library. They add weight, override styles, and fight against the pixel-perfect custom design. The design is simple enough that raw HTML/CSS in Blazor is faster and more controllable. **Do NOT use**: Chart.js, ApexCharts, or any JavaScript charting library. The timeline is simple SVG; interop adds complexity for zero benefit in a screenshot-oriented tool. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Native, fast, no additional package | | **Configuration** | `IConfiguration` / direct file read | Built into .NET 8 | Load `data.json` at startup or on-demand | | **Data Storage** | `data.json` flat file | N/A | No database—read-only JSON file in project root or `wwwroot/data` | | Component | Technology | Notes | |-----------|-----------|-------| | **Solution** | Single `.sln` with one `.csproj` | e.g., `ReportingDashboard.sln` → `ReportingDashboard.csproj` | | **Project Template** | `blazorserver` (or `blazor` with `--interactivity Server`) | `dotnet new blazor --interactivity Server --no-https` | | **SDK** | .NET 8.0.x (latest LTS patch) | Currently 8.0.11 | | Tool | Version | Purpose | |------|---------|---------| | **Hot Reload** | `dotnet watch` | Built-in; instant CSS/razor iteration | | **Unit Tests** | `xUnit` 2.9.x + `bUnit` 1.31.x | Component rendering tests if desired | | **Screenshot Validation** | Browser DevTools (manual) | Set viewport to 1920×1080, screenshot |
```xml
<!-- ReportingDashboard.csproj - NO additional packages needed beyond template -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
``` Literally zero NuGet additions. The default Blazor Server template in .NET 8 includes everything needed. --- This is not a CRUD app. It's a **read-only renderer** that transforms JSON → HTML. The architecture should reflect this simplicity:
```
data.json → DashboardDataService → Blazor Components → Fixed-size HTML/CSS/SVG page
```
```
App.razor
└── MainLayout.razor (fixed 1920×1080 container)
    └── Dashboard.razor (single page)
        ├── DashboardHeader.razor (title, subtitle, legend icons)
        ├── TimelineSection.razor (SVG milestone visualization)
        │   ├── TimelineTrack.razor (one per milestone track: M1, M2, M3)
        │   └── TimelineMilestone.razor (diamond/circle/checkpoint markers)
        └── HeatmapSection.razor (status grid)
            ├── HeatmapHeader.razor (month column headers)
            └── HeatmapRow.razor (one per status category)
                └── HeatmapCell.razor (items within a month×status cell)
```
```csharp
public record DashboardData(
    ProjectInfo Project,
    TimelineConfig Timeline,
    List<MilestoneTrack> Tracks,
    HeatmapData Heatmap
);

public record ProjectInfo(string Title, string Subtitle, string BacklogUrl, string CurrentMonth);

public record TimelineConfig(
    List<string> Months,        // ["Jan","Feb","Mar","Apr","May","Jun"]
    string CurrentMonth,         // "Apr"
    int CurrentDayOffset         // pixel or percentage offset for NOW line
);

public record MilestoneTrack(
    string Id,                   // "M1"
    string Label,                // "Chatbot & MS Role"
    string Color,                // "#0078D4"
    List<Milestone> Milestones
);

public record Milestone(
    string Date,                 // "Jan 12"
    string Type,                 // "checkpoint" | "poc" | "production"
    double Position,             // 0.0-1.0 relative position on timeline
    string? Label                // optional label like "PoC" or "Prod (TBD)"
);

public record HeatmapData(
    List<string> Months,         // column headers
    List<HeatmapCategory> Categories
);

public record HeatmapCategory(
    string Name,                 // "Shipped" | "In Progress" | "Carryover" | "Blockers"
    string CssClass,             // "ship" | "prog" | "carry" | "block"
    Dictionary<string, List<string>> Items  // month → list of item names
);
```
- **Startup**: `DashboardDataService` registered as singleton reads `data.json` from disk via `System.Text.Json`.
- **Page Load**: `Dashboard.razor` injects `DashboardDataService`, passes data down to child components via `[Parameter]`.
- **Rendering**: Each component emits semantic HTML with CSS classes matching the original design's naming convention.
- **No interactivity**: No button handlers, no forms, no SignalR callbacks. Pure server-side render on first load. Use **CSS custom properties** for the color palette to enable easy per-project theming:
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
    --font-family: 'Segoe UI', Arial, sans-serif;
    --page-width: 1920px;
    --page-height: 1080px;
}
``` The timeline SVG should be generated dynamically in `TimelineSection.razor` using Blazor markup:
```razor
<svg width="1560" height="185" style="overflow:visible;display:block">
    @foreach (var month in Timeline.Months)
    {
        <line x1="@monthX" y1="0" x2="@monthX" y2="185" stroke="#bbb" stroke-opacity="0.4" />
        <text x="@(monthX+5)" y="14" fill="#666" font-size="11">@month</text>
    }
    @* NOW line *@
    <line x1="@nowX" y1="0" x2="@nowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
    
    @foreach (var track in Tracks)
    {
        @* Track line + milestones *@
    }
</svg>
``` This gives pixel-perfect control without any charting library overhead. ---

### Considerations & Risks

- | Concern | Decision | Rationale | |---------|----------|-----------| | **Authentication** | None | Explicitly out of scope; local-only tool | | **Authorization** | None | Single-user, local access | | **HTTPS** | Disabled | `--no-https` flag on project creation; localhost only | | **CORS** | N/A | No API calls, no cross-origin requests | | **Input Validation** | JSON schema validation on `data.json` | Prevent malformed data from crashing render | | **Data Sensitivity** | Low | Project names/milestones are not PII; screenshots go to PowerPoint | | Aspect | Recommendation | |--------|---------------| | **Runtime** | Local `dotnet run` or `dotnet watch` | | **Port** | `http://localhost:5000` (Kestrel default without HTTPS) | | **Deployment** | None needed—run from source or `dotnet publish` to a local folder | | **Containerization** | Not needed for local-only use | | **Infrastructure Cost** | $0—runs on developer's existing machine |
- Edit `data.json` with current project status.
- Run `dotnet run` (or keep `dotnet watch` running).
- Open `http://localhost:5000` in browser.
- Set browser window to 1920×1080 (or use DevTools device mode).
- Take screenshot → paste into PowerPoint. --- | Risk | Severity | Mitigation | |------|----------|------------| | **SVG positioning drift across browsers** | Medium | Test in Edge/Chrome only (executive screenshots); use fixed pixel values, not percentages for milestone positions | | **Blazor Server overhead for a static page** | Low | Negligible for local single-user; alternatively could use Blazor Static SSR (new in .NET 8) to eliminate SignalR entirely | | **Segoe UI not available on non-Windows** | Low | Target is Windows developer machines; fallback to Arial in CSS | | **data.json schema changes break rendering** | Medium | Add null checks and fallback rendering in components; validate JSON on load | | **Hot reload doesn't catch CSS Grid layout issues** | Low | Browser DevTools Grid inspector is the best debugging tool | | Decision | Benefit | Cost | |----------|---------|------| | No charting library | Zero dependencies, pixel-perfect SVG control | Must hand-code SVG coordinate math | | No component library (MudBlazor etc.) | No style conflicts, smaller bundle, exact design match | No pre-built components to accelerate dev | | Blazor Server instead of static HTML | Templating, data binding, component reuse | Requires `dotnet` runtime to serve page | | Fixed 1920×1080 layout | Perfect PowerPoint screenshots | Not responsive—bad on mobile (acceptable for this use case) | | Single `data.json` file | Simple to edit, no database setup | No history, no multi-user editing, no validation UI | .NET 8 introduced **Static Server-Side Rendering (SSR)** where pages render once with no persistent SignalR circuit. Since this dashboard has zero interactivity, Static SSR is the ideal render mode:
```csharp
// In App.razor or page-level
@attribute [StreamRendering(false)]
// No @rendermode needed - static SSR is the default in .NET 8
``` This eliminates the SignalR connection entirely, making the page load faster and use less memory. The page renders once on the server and ships as pure HTML/CSS/SVG to the browser. --- | # | Question | Stakeholder | Impact | |---|----------|-------------|--------| | 1 | **How many milestone tracks should be supported?** The reference shows 3 (M1, M2, M3). Should `data.json` support N tracks with dynamic SVG height? | Product Owner | Affects SVG layout math and page height | | 2 | **Should the month columns be configurable?** The reference shows Jan–Jun (6 months). Should the heatmap support 3, 6, or 12 months? | Product Owner | Affects grid column definitions and data schema | | 3 | **Is the "current month" always the highlighted column?** The April column has distinct background colors. Should this auto-detect from system date or be set in `data.json`? | Engineer | Minor: recommend `data.json` explicit config for reproducible screenshots | | 4 | **Will multiple projects share the same app instance?** If so, need URL routing (`/project/alpha`, `/project/beta`) each reading from separate JSON files. | Product Owner | Affects routing and file organization | | 5 | **What's the maximum number of items per heatmap cell?** The reference shows ~5–8 items per cell. Need overflow strategy (scroll, truncate, shrink font) for cells with 15+ items. | Designer | Affects CSS overflow rules | | 6 | **Should the ADO Backlog link be clickable?** The reference includes a link in the header. For screenshot use, this is cosmetic—but should it actually open ADO? | Product Owner | Trivial: just set `href` in `data.json` | ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipping status, and progress in a format optimized for PowerPoint screenshot capture at 1920×1080. The architecture is intentionally simple: a local-only Blazor Server app that reads from a `data.json` configuration file—no database, no authentication, no cloud services. The primary technical challenge is faithfully reproducing the heatmap grid, SVG timeline, and color-coded status system from the HTML design reference using Blazor components with pure CSS and inline SVG. Given the simplicity requirements, the recommendation is a **single-project .sln** with minimal dependencies: just `System.Text.Json` for data loading and native Blazor rendering for all UI. No charting library is needed—the design is achievable with CSS Grid, Flexbox, and hand-crafted SVG, which gives pixel-perfect control for screenshot fidelity.

---

## 2. Key Findings

- The original HTML design is entirely self-contained (no JS frameworks, no charting libs)—the Blazor implementation should mirror this simplicity using pure CSS and inline SVG rendered server-side.
- CSS Grid with `grid-template-columns: 160px repeat(4, 1fr)` and a 5-row layout (header + 4 status rows) is the core layout primitive for the heatmap section; Blazor can emit this markup directly.
- The SVG timeline uses basic primitives (lines, circles, polygons, text) with a drop-shadow filter—no charting library is needed; Blazor's `RenderTreeBuilder` or `.razor` markup can emit SVG natively.
- `System.Text.Json` (built into .NET 8) is sufficient for deserializing `data.json`; no need for Newtonsoft.Json or any ORM.
- Blazor Server's SignalR circuit is irrelevant here since there's no interactivity beyond page load—the app is essentially a server-rendered static page, making Blazor Server's overhead negligible.
- The color palette is fixed and small (15 colors), best implemented as CSS custom properties for maintainability and easy theming per-project.
- Screenshot fidelity at 1920×1080 requires a fixed `width: 1920px; height: 1080px; overflow: hidden` on the body, exactly as the original design does—this is non-negotiable for PowerPoint use.
- A `data.json` schema should capture: project metadata, milestones (with dates, types, positions), timeline tracks, and heatmap items (categorized by Shipped/In Progress/Carryover/Blockers × Month).
- Hot reload (`dotnet watch`) works out of the box with .NET 8 Blazor Server for rapid CSS/markup iteration.
- No NuGet packages beyond the default Blazor Server template are required.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server UI)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (.razor components) | .NET 8.0 (LTS) | Ships with `Microsoft.AspNetCore.App` |
| **CSS Layout** | Pure CSS Grid + Flexbox | Native | Matches original design's layout approach exactly |
| **SVG Rendering** | Inline SVG in .razor files | Native | Timeline milestones, diamonds, circles—no library needed |
| **Font** | Segoe UI | System font | Available on all Windows machines; no web font loading |
| **CSS Architecture** | Component-scoped CSS (`.razor.css`) + global `app.css` | Native Blazor | Scoped styles for components, global for design tokens |

**Do NOT use**: MudBlazor, Radzen, Syncfusion, or any component library. They add weight, override styles, and fight against the pixel-perfect custom design. The design is simple enough that raw HTML/CSS in Blazor is faster and more controllable.

**Do NOT use**: Chart.js, ApexCharts, or any JavaScript charting library. The timeline is simple SVG; interop adds complexity for zero benefit in a screenshot-oriented tool.

### Backend (Data Layer)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Native, fast, no additional package |
| **Configuration** | `IConfiguration` / direct file read | Built into .NET 8 | Load `data.json` at startup or on-demand |
| **Data Storage** | `data.json` flat file | N/A | No database—read-only JSON file in project root or `wwwroot/data` |

### Project Structure

| Component | Technology | Notes |
|-----------|-----------|-------|
| **Solution** | Single `.sln` with one `.csproj` | e.g., `ReportingDashboard.sln` → `ReportingDashboard.csproj` |
| **Project Template** | `blazorserver` (or `blazor` with `--interactivity Server`) | `dotnet new blazor --interactivity Server --no-https` |
| **SDK** | .NET 8.0.x (latest LTS patch) | Currently 8.0.11 |

### Testing & Dev Tools

| Tool | Version | Purpose |
|------|---------|---------|
| **Hot Reload** | `dotnet watch` | Built-in; instant CSS/razor iteration |
| **Unit Tests** | `xUnit` 2.9.x + `bUnit` 1.31.x | Component rendering tests if desired |
| **Screenshot Validation** | Browser DevTools (manual) | Set viewport to 1920×1080, screenshot |

### Specific NuGet Packages

```xml
<!-- ReportingDashboard.csproj - NO additional packages needed beyond template -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

Literally zero NuGet additions. The default Blazor Server template in .NET 8 includes everything needed.

---

## 4. Architecture Recommendations

### Overall Pattern: **Static Data Renderer**

This is not a CRUD app. It's a **read-only renderer** that transforms JSON → HTML. The architecture should reflect this simplicity:

```
data.json → DashboardDataService → Blazor Components → Fixed-size HTML/CSS/SVG page
```

### Component Hierarchy

```
App.razor
└── MainLayout.razor (fixed 1920×1080 container)
    └── Dashboard.razor (single page)
        ├── DashboardHeader.razor (title, subtitle, legend icons)
        ├── TimelineSection.razor (SVG milestone visualization)
        │   ├── TimelineTrack.razor (one per milestone track: M1, M2, M3)
        │   └── TimelineMilestone.razor (diamond/circle/checkpoint markers)
        └── HeatmapSection.razor (status grid)
            ├── HeatmapHeader.razor (month column headers)
            └── HeatmapRow.razor (one per status category)
                └── HeatmapCell.razor (items within a month×status cell)
```

### Data Model (C# records for `data.json`)

```csharp
public record DashboardData(
    ProjectInfo Project,
    TimelineConfig Timeline,
    List<MilestoneTrack> Tracks,
    HeatmapData Heatmap
);

public record ProjectInfo(string Title, string Subtitle, string BacklogUrl, string CurrentMonth);

public record TimelineConfig(
    List<string> Months,        // ["Jan","Feb","Mar","Apr","May","Jun"]
    string CurrentMonth,         // "Apr"
    int CurrentDayOffset         // pixel or percentage offset for NOW line
);

public record MilestoneTrack(
    string Id,                   // "M1"
    string Label,                // "Chatbot & MS Role"
    string Color,                // "#0078D4"
    List<Milestone> Milestones
);

public record Milestone(
    string Date,                 // "Jan 12"
    string Type,                 // "checkpoint" | "poc" | "production"
    double Position,             // 0.0-1.0 relative position on timeline
    string? Label                // optional label like "PoC" or "Prod (TBD)"
);

public record HeatmapData(
    List<string> Months,         // column headers
    List<HeatmapCategory> Categories
);

public record HeatmapCategory(
    string Name,                 // "Shipped" | "In Progress" | "Carryover" | "Blockers"
    string CssClass,             // "ship" | "prog" | "carry" | "block"
    Dictionary<string, List<string>> Items  // month → list of item names
);
```

### Data Flow

1. **Startup**: `DashboardDataService` registered as singleton reads `data.json` from disk via `System.Text.Json`.
2. **Page Load**: `Dashboard.razor` injects `DashboardDataService`, passes data down to child components via `[Parameter]`.
3. **Rendering**: Each component emits semantic HTML with CSS classes matching the original design's naming convention.
4. **No interactivity**: No button handlers, no forms, no SignalR callbacks. Pure server-side render on first load.

### CSS Strategy

Use **CSS custom properties** for the color palette to enable easy per-project theming:

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
    --font-family: 'Segoe UI', Arial, sans-serif;
    --page-width: 1920px;
    --page-height: 1080px;
}
```

### SVG Timeline Approach

The timeline SVG should be generated dynamically in `TimelineSection.razor` using Blazor markup:

```razor
<svg width="1560" height="185" style="overflow:visible;display:block">
    @foreach (var month in Timeline.Months)
    {
        <line x1="@monthX" y1="0" x2="@monthX" y2="185" stroke="#bbb" stroke-opacity="0.4" />
        <text x="@(monthX+5)" y="14" fill="#666" font-size="11">@month</text>
    }
    @* NOW line *@
    <line x1="@nowX" y1="0" x2="@nowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
    
    @foreach (var track in Tracks)
    {
        @* Track line + milestones *@
    }
</svg>
```

This gives pixel-perfect control without any charting library overhead.

---

## 5. Security & Infrastructure

### Security (Minimal by Design)

| Concern | Decision | Rationale |
|---------|----------|-----------|
| **Authentication** | None | Explicitly out of scope; local-only tool |
| **Authorization** | None | Single-user, local access |
| **HTTPS** | Disabled | `--no-https` flag on project creation; localhost only |
| **CORS** | N/A | No API calls, no cross-origin requests |
| **Input Validation** | JSON schema validation on `data.json` | Prevent malformed data from crashing render |
| **Data Sensitivity** | Low | Project names/milestones are not PII; screenshots go to PowerPoint |

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Local `dotnet run` or `dotnet watch` |
| **Port** | `http://localhost:5000` (Kestrel default without HTTPS) |
| **Deployment** | None needed—run from source or `dotnet publish` to a local folder |
| **Containerization** | Not needed for local-only use |
| **Infrastructure Cost** | $0—runs on developer's existing machine |

### Operational Workflow

1. Edit `data.json` with current project status.
2. Run `dotnet run` (or keep `dotnet watch` running).
3. Open `http://localhost:5000` in browser.
4. Set browser window to 1920×1080 (or use DevTools device mode).
5. Take screenshot → paste into PowerPoint.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **SVG positioning drift across browsers** | Medium | Test in Edge/Chrome only (executive screenshots); use fixed pixel values, not percentages for milestone positions |
| **Blazor Server overhead for a static page** | Low | Negligible for local single-user; alternatively could use Blazor Static SSR (new in .NET 8) to eliminate SignalR entirely |
| **Segoe UI not available on non-Windows** | Low | Target is Windows developer machines; fallback to Arial in CSS |
| **data.json schema changes break rendering** | Medium | Add null checks and fallback rendering in components; validate JSON on load |
| **Hot reload doesn't catch CSS Grid layout issues** | Low | Browser DevTools Grid inspector is the best debugging tool |

### Trade-offs Made

| Decision | Benefit | Cost |
|----------|---------|------|
| No charting library | Zero dependencies, pixel-perfect SVG control | Must hand-code SVG coordinate math |
| No component library (MudBlazor etc.) | No style conflicts, smaller bundle, exact design match | No pre-built components to accelerate dev |
| Blazor Server instead of static HTML | Templating, data binding, component reuse | Requires `dotnet` runtime to serve page |
| Fixed 1920×1080 layout | Perfect PowerPoint screenshots | Not responsive—bad on mobile (acceptable for this use case) |
| Single `data.json` file | Simple to edit, no database setup | No history, no multi-user editing, no validation UI |

### Blazor Server vs. Static SSR (.NET 8)

.NET 8 introduced **Static Server-Side Rendering (SSR)** where pages render once with no persistent SignalR circuit. Since this dashboard has zero interactivity, Static SSR is the ideal render mode:

```csharp
// In App.razor or page-level
@attribute [StreamRendering(false)]
// No @rendermode needed - static SSR is the default in .NET 8
```

This eliminates the SignalR connection entirely, making the page load faster and use less memory. The page renders once on the server and ships as pure HTML/CSS/SVG to the browser.

---

## 7. Open Questions

| # | Question | Stakeholder | Impact |
|---|----------|-------------|--------|
| 1 | **How many milestone tracks should be supported?** The reference shows 3 (M1, M2, M3). Should `data.json` support N tracks with dynamic SVG height? | Product Owner | Affects SVG layout math and page height |
| 2 | **Should the month columns be configurable?** The reference shows Jan–Jun (6 months). Should the heatmap support 3, 6, or 12 months? | Product Owner | Affects grid column definitions and data schema |
| 3 | **Is the "current month" always the highlighted column?** The April column has distinct background colors. Should this auto-detect from system date or be set in `data.json`? | Engineer | Minor: recommend `data.json` explicit config for reproducible screenshots |
| 4 | **Will multiple projects share the same app instance?** If so, need URL routing (`/project/alpha`, `/project/beta`) each reading from separate JSON files. | Product Owner | Affects routing and file organization |
| 5 | **What's the maximum number of items per heatmap cell?** The reference shows ~5–8 items per cell. Need overflow strategy (scroll, truncate, shrink font) for cells with 15+ items. | Designer | Affects CSS overflow rules |
| 6 | **Should the ADO Backlog link be clickable?** The reference includes a link in the header. For screenshot use, this is cosmetic—but should it actually open ADO? | Product Owner | Trivial: just set `href` in `data.json` |

---

## 8. Implementation Recommendations

### Phasing

#### Phase 1: Scaffold & Static Layout (Day 1)
- `dotnet new blazor --interactivity Server --no-https -n ReportingDashboard`
- Create the `data.json` schema and sample data for a fictional project
- Build `MainLayout.razor` with fixed 1920×1080 container
- Implement `DashboardHeader.razor` with title, subtitle, legend
- Port the CSS from the original HTML design into `app.css` using CSS custom properties
- **Deliverable**: Header renders correctly at target resolution

#### Phase 2: Heatmap Grid (Day 1–2)
- Implement `HeatmapSection.razor` with CSS Grid layout
- Build `HeatmapRow.razor` and `HeatmapCell.razor` components
- Data-bind from `data.json` → heatmap categories → cells
- Match all 4 row color schemes (Shipped/In Progress/Carryover/Blockers)
- **Deliverable**: Full heatmap grid renders with sample data, visually matches reference

#### Phase 3: SVG Timeline (Day 2)
- Implement `TimelineSection.razor` with dynamic SVG generation
- Calculate milestone positions from `data.json` date/position values
- Render track lines, checkpoint circles, PoC diamonds, production diamonds
- Add the NOW vertical dashed line
- Add drop-shadow filter for diamond milestones
- **Deliverable**: Complete page renders matching reference design

#### Phase 4: Polish & Improvements (Day 3)
- Fine-tune spacing, font sizes, colors against original design
- Add subtle improvements: hover tooltips (optional), smooth font rendering
- Test screenshot workflow end-to-end
- Write a `README.md` with instructions for editing `data.json` and taking screenshots
- **Deliverable**: Production-ready local tool

### Quick Wins

1. **Start from the CSS, not the components**: Copy the original design's CSS verbatim into `app.css`, then wrap the HTML structure in Blazor components. This guarantees visual fidelity from the start.
2. **Use `dotnet watch` from minute one**: Hot reload on .razor and .css files makes iteration instant.
3. **Create `data.json` first**: Define the schema before writing any Blazor code. This clarifies the component data flow.
4. **Browser DevTools Grid overlay**: Enable CSS Grid overlay in Edge/Chrome DevTools to debug the heatmap layout visually.

### Sample `data.json` Structure

```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "#",
    "currentMonth": "Apr"
  },
  "timeline": {
    "months": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"],
    "nowPosition": 0.53
  },
  "tracks": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "milestones": [
        { "date": "Jan 12", "type": "checkpoint", "position": 0.067 },
        { "date": "Mar 26", "type": "poc", "position": 0.48, "label": "PoC" },
        { "date": "May 1", "type": "production", "position": 0.667, "label": "Prod (TBD)" }
      ]
    }
  ],
  "heatmap": {
    "months": ["January", "February", "March", "April"],
    "categories": [
      {
        "name": "Shipped",
        "cssClass": "ship",
        "items": {
          "January": ["Chatbot basic scaffolding", "MS Graph role sync"],
          "February": ["Intent recognition v1"],
          "March": ["PDS integration", "Bulk export API"],
          "April": ["Auto-DFD generator v1"]
        }
      }
    ]
  }
}
```

### File Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── Data/
│   │   └── data.json
│   ├── Models/
│   │   └── DashboardData.cs          # C# records for JSON deserialization
│   ├── Services/
│   │   └── DashboardDataService.cs    # Reads and deserializes data.json
│   ├── Components/
│   │   ├── App.razor
│   │   ├── Layout/
│   │   │   └── MainLayout.razor       # Fixed 1920×1080 container
│   │   └── Pages/
│   │       └── Dashboard.razor        # Main page
│   │       ├── DashboardHeader.razor
│   │       ├── TimelineSection.razor
│   │       ├── HeatmapSection.razor
│   │       ├── HeatmapRow.razor
│   │       └── HeatmapCell.razor
│   └── wwwroot/
│       └── css/
│           └── app.css                # All styles, CSS custom properties
└── README.md
```

This structure keeps the project flat and navigable—one solution, one project, clear separation of data/models/services/components. No over-engineering.

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
