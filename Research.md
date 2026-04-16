# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-16 20:36 UTC_

### Summary

This project is a **single-page executive reporting dashboard** that visualizes project milestones, shipped items, in-progress work, carryover items, and blockers. The design is intentionally simple—optimized for 1920×1080 screenshots destined for PowerPoint decks. Data is sourced from a local `data.json` file, requiring no database, no authentication, and no cloud services. **Primary Recommendation:** Build a minimal Blazor Server application with a single Razor component page that reads `data.json` via `System.Text.Json`, renders inline SVG for the timeline/Gantt area, and uses pure CSS Grid + Flexbox for the heatmap layout. No charting library is needed—the design is achievable with hand-crafted SVG and CSS, which gives pixel-perfect control matching the HTML reference. The entire solution should be **under 10 files** and deliverable in a single sprint. --- | Package | Version | Required? | Purpose | |---------|---------|-----------|---------| | `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Yes (default) | Blazor Server runtime | | **No additional packages** | — | — | Everything needed is in the box | For optional testing: | Package | Version | Purpose | |---------|---------|---------| | `xunit` | 2.7.x | Unit test framework | | `xunit.runner.visualstudio` | 2.5.x | VS test runner integration | | `Microsoft.NET.Test.Sdk` | 17.9.x | Test SDK | | `bunit` | 1.28.x | Blazor component testing | | `Microsoft.Playwright` | 1.41.x | Screenshot automation & visual regression |

### Key Findings

- The original `OriginalDesignConcept.html` is a self-contained static page using CSS Grid (`grid-template-columns: 160px repeat(4,1fr)`), Flexbox, and inline SVG—all natively supported in Blazor without any JavaScript interop.
- **No charting library is needed.** The timeline uses simple SVG primitives (lines, circles, polygons, text) that are trivially rendered via Razor markup with `@foreach` loops over milestone data.
- The heatmap grid is pure CSS Grid with colored cells—no canvas or charting library required. Blazor's component model maps cleanly to this: one component per visual section (Header, Timeline, Heatmap).
- `System.Text.Json` (built into .NET 8) handles `data.json` deserialization with zero additional dependencies.
- Blazor Server's hot-reload (`dotnet watch`) provides a fast inner dev loop for iterating on the visual design.
- Since this is screenshot-targeted at 1920×1080, responsive design is explicitly **not needed**—the layout should be fixed-width like the original HTML.
- The total NuGet dependency count for this project should be **zero beyond the default Blazor Server template**. This is achievable and desirable. --- **Goal:** Pixel-perfect reproduction of the original HTML design, data-driven from `data.json`. | Step | Task | Time Estimate | |------|------|---------------| | 1 | `dotnet new blazorserver -n ReportingDashboard` + strip default template (remove Counter, Weather, NavMenu) | 30 min | | 2 | Create `Models/DashboardData.cs` — C# records matching the JSON schema above | 30 min | | 3 | Create `Services/DashboardDataService.cs` — reads + caches `data.json` | 20 min | | 4 | Port CSS from `OriginalDesignConcept.html` into `wwwroot/css/dashboard.css` | 30 min | | 5 | Build `Dashboard.razor` with header, timeline SVG, and heatmap grid | 3 hours | | 6 | Create `data.json` with fictional project data | 30 min | | 7 | Visual comparison against reference screenshot | 30 min |
- Use `dotnet watch` for instant feedback while porting the CSS and building components.
- Copy the original HTML's `<style>` block verbatim as the starting point, then parameterize with CSS custom properties.
- Start with hardcoded data in the Razor component, then swap to JSON-driven data once the visuals match. | Step | Task | |------|------| | 1 | Add CSS custom properties for theming (swap color palettes per project) | | 2 | Add `FileSystemWatcher` so editing `data.json` auto-refreshes the browser | | 3 | Add tooltip hover effects on timeline milestones (pure CSS, no JS) | | 4 | Add subtle CSS transitions for a more polished feel | | 5 | Create `data.sample.json` with documentation comments | | Step | Task | |------|------| | 1 | Add Playwright screenshot script: launches app, navigates, saves 1920×1080 PNG | | 2 | Optionally generate multiple PNGs for different `data-*.json` files |
- ❌ Authentication / authorization
- ❌ Database (SQLite, SQL Server, etc.)
- ❌ REST API endpoints
- ❌ Responsive / mobile layout
- ❌ Admin panel or CRUD forms (unless Phase 2 scope expands)
- ❌ Logging infrastructure (beyond `Console.WriteLine` for errors)
- ❌ Docker / containerization
- ❌ CI/CD pipeline (this is a local tool) --- | Token | Hex | Usage | |-------|-----|-------| | `--shipped-accent` | `#34A853` | Shipped items bullet, production milestone diamond | | `--shipped-bg` | `#F0FBF0` | Shipped row cell background | | `--shipped-bg-current` | `#D8F2DA` | Shipped row, current month highlight | | `--shipped-header` | `#E8F5E9` | Shipped row header background | | `--progress-accent` | `#0078D4` | In-progress bullet, stream M1 color, links | | `--progress-bg` | `#EEF4FE` | In-progress row cell background | | `--progress-bg-current` | `#DAE8FB` | In-progress row, current month highlight | | `--carryover-accent` | `#F4B400` | Carryover bullet, PoC milestone diamond | | `--carryover-bg` | `#FFFDE7` | Carryover row cell background | | `--carryover-bg-current` | `#FFF0B0` | Carryover row, current month highlight | | `--blocker-accent` | `#EA4335` | Blocker bullet, "NOW" line | | `--blocker-bg` | `#FFF5F5` | Blocker row cell background | | `--blocker-bg-current` | `#FFE4E4` | Blocker row, current month highlight | | `--header-bg` | `#F5F5F5` | Column/corner header background | | `--current-month-header` | `#FFF0D0` | Current month column header (amber tint) | | `--text-primary` | `#111` | Body text | | `--text-secondary` | `#666` | SVG labels, subtle text | | `--text-muted` | `#888` / `#999` | Subtitle, section titles | | `--border` | `#E0E0E0` | Grid borders | | `--border-heavy` | `#CCC` | Row/column header separator |

### Recommended Tools & Technologies

- **Date:** April 16, 2026 **Project:** Executive Reporting Dashboard (Single-Page Milestone & Progress View) **Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure --- | Component | Technology | Version | Notes | |-----------|-----------|---------|-------| | **Framework** | Blazor Server | .NET 8.0.x (LTS) | Built-in, no additional package needed | | **CSS Layout** | CSS Grid + Flexbox | Native | Matches original HTML design exactly | | **Timeline/Gantt** | Inline SVG via Razor | Native | `<svg>` elements with `@foreach` loops over milestones | | **Heatmap** | CSS Grid | Native | `grid-template-columns: 160px repeat(N,1fr)` with dynamic column count | | **Font** | Segoe UI | System font | Already specified in design; available on Windows | | **Color Theming** | CSS custom properties | Native | Define palette as `--color-shipped: #34A853` etc. in a single CSS file | **Why no charting library?** Libraries like Radzen, MudBlazor Charts, or ApexCharts add complexity and limit pixel-perfect control. The original design uses ~20 SVG primitives total. Hand-writing these in Razor is simpler than configuring a chart library and fighting its defaults. **Alternative considered — MudBlazor:** Provides a rich component library (v7.x for .NET 8), but it's overkill for this project. MudBlazor's grid, typography, and theming systems would add 2MB+ of CSS/JS and impose its own design language. The original HTML design is custom enough that you'd spend more time overriding MudBlazor defaults than building from scratch. | Component | Technology | Version | Notes | |-----------|-----------|---------|-------| | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | `JsonSerializer.Deserialize<DashboardData>()` | | **File I/O** | `System.IO.File` | Built into .NET 8 | `File.ReadAllTextAsync("data.json")` | | **Data Models** | C# Records | C# 12 / .NET 8 | Immutable, concise, perfect for config data | | **Dependency Injection** | Built-in DI | .NET 8 | Register a `DashboardDataService` as Singleton | | Component | Technology | Notes | |-----------|-----------|-------| | **Primary Store** | `data.json` file | Flat JSON file in the project root or `wwwroot/data/` | | **No database** | — | Explicitly not needed; JSON file is the single source of truth | | **File watching (optional)** | `FileSystemWatcher` | Auto-reload dashboard when `data.json` is edited externally | | Component | Technology | Version | Notes | |-----------|-----------|---------|-------| | **SDK** | .NET 8 SDK | 8.0.x LTS | `dotnet new blazorserver` template | | **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Latest | Both support Blazor hot-reload | | **Dev Server** | Kestrel (built-in) | .NET 8 | `dotnet run` or `dotnet watch` | | **Package Manager** | NuGet | Built-in | Expect **zero additional packages** | | Component | Technology | Version | Notes | |-----------|-----------|---------|-------| | **Unit Tests** | xUnit | 2.7.x | Test JSON deserialization and data model logic | | **Component Tests** | bUnit | 1.28.x+ | Test Blazor components in isolation (optional given simplicity) | | **Visual Regression** | Playwright for .NET | 1.41.x | Screenshot comparison at 1920×1080 (optional but valuable) | > **Recommendation on testing:** Given the extreme simplicity of this app, a single integration test that launches the app and takes a Playwright screenshot may be more valuable than unit tests. However, testing the JSON deserialization with xUnit is trivial and worth doing. --- This is not a complex application. Do **not** over-architect it.
```
ReportingDashboard.sln
├── ReportingDashboard/
│   ├── Program.cs                    # Minimal Blazor Server setup
│   ├── ReportingDashboard.csproj     # .NET 8, no extra NuGet refs
│   ├── Models/
│   │   └── DashboardData.cs          # C# records for JSON shape
│   ├── Services/
│   │   └── DashboardDataService.cs   # Reads & caches data.json
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Dashboard.razor       # The single page
│   │   ├── Layout/
│   │   │   └── MainLayout.razor      # Minimal shell (no nav)
│   │   ├── DashboardHeader.razor     # Title, subtitle, legend
│   │   ├── TimelineSection.razor     # SVG milestone timeline
│   │   └── HeatmapGrid.razor        # CSS Grid status matrix
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── dashboard.css         # All styles (port from HTML)
│   │   └── data/
│   │       └── data.json             # The data source
│   └── Properties/
│       └── launchSettings.json
└── ReportingDashboard.Tests/         # Optional xUnit project
    └── DataDeserializationTests.cs
```
```
data.json → DashboardDataService (singleton, cached) → Dashboard.razor → Child Components
```
- **Startup:** `DashboardDataService` is registered as a Singleton in `Program.cs`.
- **First Request:** Service reads `data.json` from disk, deserializes to `DashboardData` record, caches in memory.
- **Rendering:** `Dashboard.razor` injects the service, passes data down to `DashboardHeader`, `TimelineSection`, and `HeatmapGrid` as `[Parameter]` properties.
- **Refresh:** Optionally, a `FileSystemWatcher` triggers re-read when `data.json` changes, updating the cached data and calling `StateHasChanged()` via a circuit notification.
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-16",
  "timelineStart": "2026-01-01",
  "timelineEnd": "2026-06-30",
  "milestoneStreams": [
    {
      "id": "M1",
      "name": "Chatbot & MS Role",
      "color": "#0078D4",
      "milestones": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
      ]
    }
  ],
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "currentColumnIndex": 3,
    "rows": [
      {
        "category": "shipped",
        "items": {
          "Jan": ["Item A", "Item B"],
          "Feb": ["Item C"],
          "Mar": ["Item D", "Item E"],
          "Apr": ["Item F"]
        }
      },
      {
        "category": "inProgress",
        "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item G", "Item H"] }
      },
      {
        "category": "carryover",
        "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item I"] }
      },
      {
        "category": "blockers",
        "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item J"] }
      }
    ]
  }
}
``` Port the original HTML's `<style>` block directly into `wwwroot/css/dashboard.css`. Key decisions:
- **Fixed width:** `body { width: 1920px; height: 1080px; overflow: hidden; }` — matches the screenshot target exactly.
- **CSS custom properties** for the color palette — makes it easy to theme for different projects:
  ```css
  :root {
    --shipped-bg: #F0FBF0;
    --shipped-accent: #34A853;
    --progress-bg: #EEF4FE;
    --progress-accent: #0078D4;
    --carryover-bg: #FFFDE7;
    --carryover-accent: #F4B400;
    --blocker-bg: #FFF5F5;
    --blocker-accent: #EA4335;
  }
  ```
- **No CSS framework.** Bootstrap, Tailwind, etc. add bloat and fight the custom design. The original HTML is ~80 lines of CSS—keep it that way. The timeline section uses SVG rendered directly in Razor:
```razor
<svg width="1560" height="185" style="overflow:visible">
    @foreach (var stream in Data.MilestoneStreams)
    {
        var y = GetStreamY(stream);
        <line x1="0" y1="@y" x2="1560" y2="@y" stroke="@stream.Color" stroke-width="3"/>
        @foreach (var ms in stream.Milestones)
        {
            var x = DateToX(ms.Date);
            @if (ms.Type == "poc")
            {
                <polygon points="@DiamondPoints(x, y, 11)" fill="#F4B400"/>
            }
            else if (ms.Type == "production")
            {
                <polygon points="@DiamondPoints(x, y, 11)" fill="#34A853"/>
            }
            else
            {
                <circle cx="@x" cy="@y" r="5" fill="white" stroke="#888" stroke-width="2.5"/>
            }
            <text x="@x" y="@(y - 16)" text-anchor="middle" fill="#666" font-size="10">@ms.Label</text>
        }
    }
    @* "NOW" line *@
    <line x1="@NowX" y1="0" x2="@NowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
    <text x="@(NowX + 4)" y="14" fill="#EA4335" font-size="10" font-weight="700">NOW</text>
</svg>
``` The `DateToX()` helper linearly interpolates dates to pixel positions. This is ~10 lines of C# code. ---

### Considerations & Risks

- **None.** This is explicitly out of scope. The app runs locally, serves a single page, and has no user-specific data. Do not add authentication middleware. If in the future there's a need to restrict access (e.g., running on a shared server), the simplest approach would be Kestrel binding to `localhost` only (already the default).
- `data.json` contains project status information. If this is sensitive, ensure the file has appropriate filesystem permissions.
- No encryption needed for a local-only tool.
- No PII is expected in the data model. | Scenario | Approach | Details | |----------|----------|---------| | **Local dev** | `dotnet run` / `dotnet watch` | Kestrel on `https://localhost:5001` | | **Share with team** | `dotnet publish -c Release` | Copy `publish/` folder, run exe. Kestrel serves it. | | **Screenshot automation** | Playwright script | `dotnet run` → Playwright navigates to page → saves 1920×1080 PNG | **No Docker, no IIS, no reverse proxy needed.** Kestrel is sufficient for local use. **$0.** This runs on the developer's machine. No cloud services, no hosting costs, no licenses beyond what a .NET developer already has. --- A static HTML file with a build script that injects JSON data would technically be simpler. However, the stack decision is made, and Blazor Server provides real benefits: hot-reload, component reuse if additional dashboard pages are needed later, and C# type safety for the data model. The overhead is minimal (one `dotnet new` template). The original HTML includes precise SVG coordinates. When converting to dynamic Razor rendering, floating-point math for date-to-pixel conversion could introduce visual discrepancies. **Mitigation:** Write a `DateToX()` helper that uses the same linear scale as the original, and validate output against the reference screenshot using Playwright visual comparison. If someone edits `data.json` with missing fields or wrong types, the app will throw at runtime. **Mitigation:** Use nullable reference types on optional fields, add a simple validation method in `DashboardDataService` that logs clear error messages, and provide a `data.sample.json` with documentation. Blazor Server maintains a WebSocket connection per client. For a local tool with 1-2 browser tabs, this is a non-issue. If the goal were to serve 100+ concurrent executives, Blazor WebAssembly or static rendering would be better—but that's not this project. **Decision: No component library.** The design is custom, the page count is one, and the CSS is ~100 lines. A component library would add more complexity than it removes. Revisit only if the project grows to 5+ pages with forms and data tables. **Decision: Fixed width.** The explicit goal is PowerPoint screenshots at a known resolution. Responsive design adds CSS complexity with zero benefit for this use case. The `body` tag should enforce `width: 1920px; height: 1080px; overflow: hidden;` exactly as in the original. ---
- **How many milestone streams?** The original design shows 3 (M1, M2, M3). Should the data model support a variable number, or is 3 fixed? **Recommendation:** Make it dynamic (array in JSON) — minimal extra effort.
- **How many heatmap columns (months)?** The design shows 4 (Jan–Apr). Should this auto-expand based on data, or stay at 4? **Recommendation:** Drive from JSON data — the CSS Grid `repeat(N, 1fr)` adapts automatically.
- **Who edits `data.json`?** If non-technical users need to update it, consider adding a minimal edit form in a second Blazor page (`/edit`). If only developers edit it, a text editor is fine.
- **Screenshot automation priority?** If generating screenshots for every monthly update is a recurring task, investing 2 hours in a Playwright script that auto-generates PNGs is high-value. Decide now vs. later.
- **Multiple projects?** Is this dashboard for one project, or should it support switching between projects (e.g., `data-projectA.json`, `data-projectB.json`)? If multiple, add a URL parameter: `/dashboard?project=projectA`.
- **ADO backlog link:** The design includes a "→ ADO Backlog" link. Should this be a real link from the JSON data, or decorative? ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Date:** April 16, 2026  
**Project:** Executive Reporting Dashboard (Single-Page Milestone & Progress View)  
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure

---

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** that visualizes project milestones, shipped items, in-progress work, carryover items, and blockers. The design is intentionally simple—optimized for 1920×1080 screenshots destined for PowerPoint decks. Data is sourced from a local `data.json` file, requiring no database, no authentication, and no cloud services.

**Primary Recommendation:** Build a minimal Blazor Server application with a single Razor component page that reads `data.json` via `System.Text.Json`, renders inline SVG for the timeline/Gantt area, and uses pure CSS Grid + Flexbox for the heatmap layout. No charting library is needed—the design is achievable with hand-crafted SVG and CSS, which gives pixel-perfect control matching the HTML reference. The entire solution should be **under 10 files** and deliverable in a single sprint.

---

## 2. Key Findings

- The original `OriginalDesignConcept.html` is a self-contained static page using CSS Grid (`grid-template-columns: 160px repeat(4,1fr)`), Flexbox, and inline SVG—all natively supported in Blazor without any JavaScript interop.
- **No charting library is needed.** The timeline uses simple SVG primitives (lines, circles, polygons, text) that are trivially rendered via Razor markup with `@foreach` loops over milestone data.
- The heatmap grid is pure CSS Grid with colored cells—no canvas or charting library required. Blazor's component model maps cleanly to this: one component per visual section (Header, Timeline, Heatmap).
- `System.Text.Json` (built into .NET 8) handles `data.json` deserialization with zero additional dependencies.
- Blazor Server's hot-reload (`dotnet watch`) provides a fast inner dev loop for iterating on the visual design.
- Since this is screenshot-targeted at 1920×1080, responsive design is explicitly **not needed**—the layout should be fixed-width like the original HTML.
- The total NuGet dependency count for this project should be **zero beyond the default Blazor Server template**. This is achievable and desirable.

---

## 3. Recommended Technology Stack

### Frontend (UI Layer)

| Component | Technology | Version | Notes |
|-----------|-----------|---------|-------|
| **Framework** | Blazor Server | .NET 8.0.x (LTS) | Built-in, no additional package needed |
| **CSS Layout** | CSS Grid + Flexbox | Native | Matches original HTML design exactly |
| **Timeline/Gantt** | Inline SVG via Razor | Native | `<svg>` elements with `@foreach` loops over milestones |
| **Heatmap** | CSS Grid | Native | `grid-template-columns: 160px repeat(N,1fr)` with dynamic column count |
| **Font** | Segoe UI | System font | Already specified in design; available on Windows |
| **Color Theming** | CSS custom properties | Native | Define palette as `--color-shipped: #34A853` etc. in a single CSS file |

**Why no charting library?** Libraries like Radzen, MudBlazor Charts, or ApexCharts add complexity and limit pixel-perfect control. The original design uses ~20 SVG primitives total. Hand-writing these in Razor is simpler than configuring a chart library and fighting its defaults.

**Alternative considered — MudBlazor:** Provides a rich component library (v7.x for .NET 8), but it's overkill for this project. MudBlazor's grid, typography, and theming systems would add 2MB+ of CSS/JS and impose its own design language. The original HTML design is custom enough that you'd spend more time overriding MudBlazor defaults than building from scratch.

### Backend (Data Layer)

| Component | Technology | Version | Notes |
|-----------|-----------|---------|-------|
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | `JsonSerializer.Deserialize<DashboardData>()` |
| **File I/O** | `System.IO.File` | Built into .NET 8 | `File.ReadAllTextAsync("data.json")` |
| **Data Models** | C# Records | C# 12 / .NET 8 | Immutable, concise, perfect for config data |
| **Dependency Injection** | Built-in DI | .NET 8 | Register a `DashboardDataService` as Singleton |

### Data Storage

| Component | Technology | Notes |
|-----------|-----------|-------|
| **Primary Store** | `data.json` file | Flat JSON file in the project root or `wwwroot/data/` |
| **No database** | — | Explicitly not needed; JSON file is the single source of truth |
| **File watching (optional)** | `FileSystemWatcher` | Auto-reload dashboard when `data.json` is edited externally |

### Infrastructure & Tooling

| Component | Technology | Version | Notes |
|-----------|-----------|---------|-------|
| **SDK** | .NET 8 SDK | 8.0.x LTS | `dotnet new blazorserver` template |
| **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Latest | Both support Blazor hot-reload |
| **Dev Server** | Kestrel (built-in) | .NET 8 | `dotnet run` or `dotnet watch` |
| **Package Manager** | NuGet | Built-in | Expect **zero additional packages** |

### Testing

| Component | Technology | Version | Notes |
|-----------|-----------|---------|-------|
| **Unit Tests** | xUnit | 2.7.x | Test JSON deserialization and data model logic |
| **Component Tests** | bUnit | 1.28.x+ | Test Blazor components in isolation (optional given simplicity) |
| **Visual Regression** | Playwright for .NET | 1.41.x | Screenshot comparison at 1920×1080 (optional but valuable) |

> **Recommendation on testing:** Given the extreme simplicity of this app, a single integration test that launches the app and takes a Playwright screenshot may be more valuable than unit tests. However, testing the JSON deserialization with xUnit is trivial and worth doing.

---

## 4. Architecture Recommendations

### Overall Architecture: Single-Component Page

This is not a complex application. Do **not** over-architect it.

```
ReportingDashboard.sln
├── ReportingDashboard/
│   ├── Program.cs                    # Minimal Blazor Server setup
│   ├── ReportingDashboard.csproj     # .NET 8, no extra NuGet refs
│   ├── Models/
│   │   └── DashboardData.cs          # C# records for JSON shape
│   ├── Services/
│   │   └── DashboardDataService.cs   # Reads & caches data.json
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Dashboard.razor       # The single page
│   │   ├── Layout/
│   │   │   └── MainLayout.razor      # Minimal shell (no nav)
│   │   ├── DashboardHeader.razor     # Title, subtitle, legend
│   │   ├── TimelineSection.razor     # SVG milestone timeline
│   │   └── HeatmapGrid.razor        # CSS Grid status matrix
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── dashboard.css         # All styles (port from HTML)
│   │   └── data/
│   │       └── data.json             # The data source
│   └── Properties/
│       └── launchSettings.json
└── ReportingDashboard.Tests/         # Optional xUnit project
    └── DataDeserializationTests.cs
```

### Data Flow

```
data.json → DashboardDataService (singleton, cached) → Dashboard.razor → Child Components
```

1. **Startup:** `DashboardDataService` is registered as a Singleton in `Program.cs`.
2. **First Request:** Service reads `data.json` from disk, deserializes to `DashboardData` record, caches in memory.
3. **Rendering:** `Dashboard.razor` injects the service, passes data down to `DashboardHeader`, `TimelineSection`, and `HeatmapGrid` as `[Parameter]` properties.
4. **Refresh:** Optionally, a `FileSystemWatcher` triggers re-read when `data.json` changes, updating the cached data and calling `StateHasChanged()` via a circuit notification.

### Data Model Design (`data.json` shape)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentDate": "2026-04-16",
  "timelineStart": "2026-01-01",
  "timelineEnd": "2026-06-30",
  "milestoneStreams": [
    {
      "id": "M1",
      "name": "Chatbot & MS Role",
      "color": "#0078D4",
      "milestones": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
      ]
    }
  ],
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "currentColumnIndex": 3,
    "rows": [
      {
        "category": "shipped",
        "items": {
          "Jan": ["Item A", "Item B"],
          "Feb": ["Item C"],
          "Mar": ["Item D", "Item E"],
          "Apr": ["Item F"]
        }
      },
      {
        "category": "inProgress",
        "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item G", "Item H"] }
      },
      {
        "category": "carryover",
        "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item I"] }
      },
      {
        "category": "blockers",
        "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item J"] }
      }
    ]
  }
}
```

### CSS Architecture

Port the original HTML's `<style>` block directly into `wwwroot/css/dashboard.css`. Key decisions:

- **Fixed width:** `body { width: 1920px; height: 1080px; overflow: hidden; }` — matches the screenshot target exactly.
- **CSS custom properties** for the color palette — makes it easy to theme for different projects:
  ```css
  :root {
    --shipped-bg: #F0FBF0;
    --shipped-accent: #34A853;
    --progress-bg: #EEF4FE;
    --progress-accent: #0078D4;
    --carryover-bg: #FFFDE7;
    --carryover-accent: #F4B400;
    --blocker-bg: #FFF5F5;
    --blocker-accent: #EA4335;
  }
  ```
- **No CSS framework.** Bootstrap, Tailwind, etc. add bloat and fight the custom design. The original HTML is ~80 lines of CSS—keep it that way.

### SVG Timeline Rendering

The timeline section uses SVG rendered directly in Razor:

```razor
<svg width="1560" height="185" style="overflow:visible">
    @foreach (var stream in Data.MilestoneStreams)
    {
        var y = GetStreamY(stream);
        <line x1="0" y1="@y" x2="1560" y2="@y" stroke="@stream.Color" stroke-width="3"/>
        @foreach (var ms in stream.Milestones)
        {
            var x = DateToX(ms.Date);
            @if (ms.Type == "poc")
            {
                <polygon points="@DiamondPoints(x, y, 11)" fill="#F4B400"/>
            }
            else if (ms.Type == "production")
            {
                <polygon points="@DiamondPoints(x, y, 11)" fill="#34A853"/>
            }
            else
            {
                <circle cx="@x" cy="@y" r="5" fill="white" stroke="#888" stroke-width="2.5"/>
            }
            <text x="@x" y="@(y - 16)" text-anchor="middle" fill="#666" font-size="10">@ms.Label</text>
        }
    }
    @* "NOW" line *@
    <line x1="@NowX" y1="0" x2="@NowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
    <text x="@(NowX + 4)" y="14" fill="#EA4335" font-size="10" font-weight="700">NOW</text>
</svg>
```

The `DateToX()` helper linearly interpolates dates to pixel positions. This is ~10 lines of C# code.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope. The app runs locally, serves a single page, and has no user-specific data. Do not add authentication middleware.

If in the future there's a need to restrict access (e.g., running on a shared server), the simplest approach would be Kestrel binding to `localhost` only (already the default).

### Data Protection

- `data.json` contains project status information. If this is sensitive, ensure the file has appropriate filesystem permissions.
- No encryption needed for a local-only tool.
- No PII is expected in the data model.

### Hosting & Deployment

| Scenario | Approach | Details |
|----------|----------|---------|
| **Local dev** | `dotnet run` / `dotnet watch` | Kestrel on `https://localhost:5001` |
| **Share with team** | `dotnet publish -c Release` | Copy `publish/` folder, run exe. Kestrel serves it. |
| **Screenshot automation** | Playwright script | `dotnet run` → Playwright navigates to page → saves 1920×1080 PNG |

**No Docker, no IIS, no reverse proxy needed.** Kestrel is sufficient for local use.

### Infrastructure Costs

**$0.** This runs on the developer's machine. No cloud services, no hosting costs, no licenses beyond what a .NET developer already has.

---

## 6. Risks & Trade-offs

### Risk: Blazor Server May Be Overkill

**Severity: Low | Mitigation: Accept it**

A static HTML file with a build script that injects JSON data would technically be simpler. However, the stack decision is made, and Blazor Server provides real benefits: hot-reload, component reuse if additional dashboard pages are needed later, and C# type safety for the data model. The overhead is minimal (one `dotnet new` template).

### Risk: SVG Rendering Precision

**Severity: Medium | Mitigation: Port the original SVG exactly**

The original HTML includes precise SVG coordinates. When converting to dynamic Razor rendering, floating-point math for date-to-pixel conversion could introduce visual discrepancies. **Mitigation:** Write a `DateToX()` helper that uses the same linear scale as the original, and validate output against the reference screenshot using Playwright visual comparison.

### Risk: `data.json` Schema Drift

**Severity: Low | Mitigation: Use C# records with `[JsonPropertyName]`**

If someone edits `data.json` with missing fields or wrong types, the app will throw at runtime. **Mitigation:** Use nullable reference types on optional fields, add a simple validation method in `DashboardDataService` that logs clear error messages, and provide a `data.sample.json` with documentation.

### Risk: Blazor Server's SignalR Dependency

**Severity: Low | Mitigation: Irrelevant for local use**

Blazor Server maintains a WebSocket connection per client. For a local tool with 1-2 browser tabs, this is a non-issue. If the goal were to serve 100+ concurrent executives, Blazor WebAssembly or static rendering would be better—but that's not this project.

### Trade-off: No Component Library vs. MudBlazor/Radzen

**Decision: No component library.** The design is custom, the page count is one, and the CSS is ~100 lines. A component library would add more complexity than it removes. Revisit only if the project grows to 5+ pages with forms and data tables.

### Trade-off: Fixed 1920×1080 vs. Responsive

**Decision: Fixed width.** The explicit goal is PowerPoint screenshots at a known resolution. Responsive design adds CSS complexity with zero benefit for this use case. The `body` tag should enforce `width: 1920px; height: 1080px; overflow: hidden;` exactly as in the original.

---

## 7. Open Questions

1. **How many milestone streams?** The original design shows 3 (M1, M2, M3). Should the data model support a variable number, or is 3 fixed? **Recommendation:** Make it dynamic (array in JSON) — minimal extra effort.

2. **How many heatmap columns (months)?** The design shows 4 (Jan–Apr). Should this auto-expand based on data, or stay at 4? **Recommendation:** Drive from JSON data — the CSS Grid `repeat(N, 1fr)` adapts automatically.

3. **Who edits `data.json`?** If non-technical users need to update it, consider adding a minimal edit form in a second Blazor page (`/edit`). If only developers edit it, a text editor is fine.

4. **Screenshot automation priority?** If generating screenshots for every monthly update is a recurring task, investing 2 hours in a Playwright script that auto-generates PNGs is high-value. Decide now vs. later.

5. **Multiple projects?** Is this dashboard for one project, or should it support switching between projects (e.g., `data-projectA.json`, `data-projectB.json`)? If multiple, add a URL parameter: `/dashboard?project=projectA`.

6. **ADO backlog link:** The design includes a "→ ADO Backlog" link. Should this be a real link from the JSON data, or decorative?

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** Pixel-perfect reproduction of the original HTML design, data-driven from `data.json`.

| Step | Task | Time Estimate |
|------|------|---------------|
| 1 | `dotnet new blazorserver -n ReportingDashboard` + strip default template (remove Counter, Weather, NavMenu) | 30 min |
| 2 | Create `Models/DashboardData.cs` — C# records matching the JSON schema above | 30 min |
| 3 | Create `Services/DashboardDataService.cs` — reads + caches `data.json` | 20 min |
| 4 | Port CSS from `OriginalDesignConcept.html` into `wwwroot/css/dashboard.css` | 30 min |
| 5 | Build `Dashboard.razor` with header, timeline SVG, and heatmap grid | 3 hours |
| 6 | Create `data.json` with fictional project data | 30 min |
| 7 | Visual comparison against reference screenshot | 30 min |

**Quick Wins:**
- Use `dotnet watch` for instant feedback while porting the CSS and building components.
- Copy the original HTML's `<style>` block verbatim as the starting point, then parameterize with CSS custom properties.
- Start with hardcoded data in the Razor component, then swap to JSON-driven data once the visuals match.

### Phase 2: Polish & Improvements (1 day, optional)

| Step | Task |
|------|------|
| 1 | Add CSS custom properties for theming (swap color palettes per project) |
| 2 | Add `FileSystemWatcher` so editing `data.json` auto-refreshes the browser |
| 3 | Add tooltip hover effects on timeline milestones (pure CSS, no JS) |
| 4 | Add subtle CSS transitions for a more polished feel |
| 5 | Create `data.sample.json` with documentation comments |

### Phase 3: Automation (half day, optional)

| Step | Task |
|------|------|
| 1 | Add Playwright screenshot script: launches app, navigates, saves 1920×1080 PNG |
| 2 | Optionally generate multiple PNGs for different `data-*.json` files |

### What NOT to Build

- ❌ Authentication / authorization
- ❌ Database (SQLite, SQL Server, etc.)
- ❌ REST API endpoints
- ❌ Responsive / mobile layout
- ❌ Admin panel or CRUD forms (unless Phase 2 scope expands)
- ❌ Logging infrastructure (beyond `Console.WriteLine` for errors)
- ❌ Docker / containerization
- ❌ CI/CD pipeline (this is a local tool)

---

## Appendix A: Reference Color Palette

| Token | Hex | Usage |
|-------|-----|-------|
| `--shipped-accent` | `#34A853` | Shipped items bullet, production milestone diamond |
| `--shipped-bg` | `#F0FBF0` | Shipped row cell background |
| `--shipped-bg-current` | `#D8F2DA` | Shipped row, current month highlight |
| `--shipped-header` | `#E8F5E9` | Shipped row header background |
| `--progress-accent` | `#0078D4` | In-progress bullet, stream M1 color, links |
| `--progress-bg` | `#EEF4FE` | In-progress row cell background |
| `--progress-bg-current` | `#DAE8FB` | In-progress row, current month highlight |
| `--carryover-accent` | `#F4B400` | Carryover bullet, PoC milestone diamond |
| `--carryover-bg` | `#FFFDE7` | Carryover row cell background |
| `--carryover-bg-current` | `#FFF0B0` | Carryover row, current month highlight |
| `--blocker-accent` | `#EA4335` | Blocker bullet, "NOW" line |
| `--blocker-bg` | `#FFF5F5` | Blocker row cell background |
| `--blocker-bg-current` | `#FFE4E4` | Blocker row, current month highlight |
| `--header-bg` | `#F5F5F5` | Column/corner header background |
| `--current-month-header` | `#FFF0D0` | Current month column header (amber tint) |
| `--text-primary` | `#111` | Body text |
| `--text-secondary` | `#666` | SVG labels, subtle text |
| `--text-muted` | `#888` / `#999` | Subtitle, section titles |
| `--border` | `#E0E0E0` | Grid borders |
| `--border-heavy` | `#CCC` | Row/column header separator |

## Appendix B: NuGet Package Summary

| Package | Version | Required? | Purpose |
|---------|---------|-----------|---------|
| `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Yes (default) | Blazor Server runtime |
| **No additional packages** | — | — | Everything needed is in the box |

For optional testing:

| Package | Version | Purpose |
|---------|---------|---------|
| `xunit` | 2.7.x | Unit test framework |
| `xunit.runner.visualstudio` | 2.5.x | VS test runner integration |
| `Microsoft.NET.Test.Sdk` | 17.9.x | Test SDK |
| `bunit` | 1.28.x | Blazor component testing |
| `Microsoft.Playwright` | 1.41.x | Screenshot automation & visual regression |

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/a6745c48fa1d101b61657a518e672d62fa5936e5/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
