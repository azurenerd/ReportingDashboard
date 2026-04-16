# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-16 19:17 UTC_

### Summary

This project is a **single-page, screenshot-friendly executive reporting dashboard** built with Blazor Server on .NET 8, running entirely locally with no cloud dependencies. The dashboard renders a project timeline with milestones (SVG-based Gantt-style visualization) and a color-coded heatmap grid showing work item status (Shipped, In Progress, Carryover, Blockers) across monthly columns. All data is driven from a local `data.json` file. **Primary Recommendation:** Keep this dead simple. Use a single Blazor Server project with zero external JavaScript dependencies, inline SVG generation via Blazor components, pure CSS Grid/Flexbox for layout (matching the reference HTML exactly), and `System.Text.Json` for config deserialization. No database, no auth, no API layer. The entire app should be ~8-12 files total. This is a **data-visualization-as-code** project, not an enterprise application. ---

### Key Findings

- The reference design (`OriginalDesignConcept.html`) is a **1920×1080 fixed-layout page** using CSS Grid (`160px repeat(4,1fr)`) and Flexbox, with inline SVG for the timeline — all of which translate directly to Blazor components with zero JavaScript needed.
- **No charting library is necessary.** The timeline is a simple SVG with lines, circles, diamonds, and text — easily generated with Blazor's `@` syntax inside `<svg>` elements. Adding a charting library would be overengineering.
- **JSON file as data source is the right call.** For a screenshot-oriented reporting tool, a `data.json` file provides the simplest edit-and-refresh workflow. No database overhead, no migration complexity.
- The color palette and layout from the HTML reference are production-ready. The design uses Microsoft's Segoe UI font family and Fluent-adjacent colors (#0078D4 blue, #34A853 green, #F4B400 amber, #EA4335 red) — these should be preserved exactly.
- **Blazor Server is ideal for this use case** because it runs locally, supports hot reload during development, renders server-side (great for consistent screenshot output), and requires no WASM download penalty.
- The heatmap grid pattern (status rows × month columns with colored cells containing bullet-pointed items) maps perfectly to a nested `@foreach` loop over a well-structured data model.
- **File watching for `data.json`** via `FileSystemWatcher` enables live-reload of data without restarting the app — a significant quality-of-life feature for iterating on report content. --- **Goal:** Pixel-perfect reproduction of the HTML reference design, driven by `data.json`. | Step | Task | Estimated Time | |------|------|---------------| | 1 | Create `.sln` and Blazor Server project from `dotnet new blazor --interactivity Server` | 15 min | | 2 | Define C# data models (`DashboardData`, `Milestone`, `HeatmapCategory`, `WorkItem`) | 30 min | | 3 | Create `DashboardDataService` that reads and deserializes `data.json` | 30 min | | 4 | Port reference CSS into `dashboard.css` verbatim | 20 min | | 5 | Build `Dashboard.razor` page with header section | 30 min | | 6 | Build `Timeline.razor` component with SVG generation | 1.5 hrs | | 7 | Build `Heatmap.razor` + `HeatmapCell.razor` components | 1.5 hrs | | 8 | Create `data.json` with fictional project data | 30 min | | 9 | Visual QA against reference screenshot at 1920×1080 | 30 min | | Step | Task | |------|------| | 1 | Add `FileSystemWatcher` for live `data.json` reload without app restart | | 2 | Add `?screenshot=true` mode that hides scrollbars and forces exact 1920×1080 | | 3 | Improve the design beyond the reference (subtle shadows, better typography spacing, refined color gradients) | | 4 | Add startup validation of `data.json` with friendly error messages | | 5 | Write xUnit tests for data model deserialization edge cases |
- Multi-project support with project selector dropdown
- Dark mode theme toggle
- Print-friendly CSS `@media print` styles
- Automated screenshot generation via Playwright
- Simple JSON editor UI for non-technical users
- **Start from the HTML reference, not from scratch.** The CSS is already written and battle-tested. Port it directly rather than re-inventing the layout.
- **Use `dotnet watch` during development** for instant hot reload when editing `.razor` files or CSS.
- **Fictional data first.** Create compelling fake data for a fictional "Project Phoenix" to demonstrate the dashboard before wiring up real project data. This lets you iterate on the visual design independently.
- **Single `.razor` page for v1.** Don't over-componentize. Start with everything in `Dashboard.razor` and extract components only when duplication appears. The reference HTML is ~150 lines — a single Razor page is fine.
```xml
<!-- ReportingDashboard.csproj — no additional packages needed for core functionality -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>

<!-- ReportingDashboard.Tests.csproj (optional) -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="bunit" Version="1.25.3" />
  </ItemGroup>
</Project>
``` **The total external dependency count for the main project is zero.** Everything needed — Blazor Server, System.Text.Json, Kestrel, FileSystemWatcher — ships with .NET 8. --- *Research completed April 16, 2026. Ready for architecture review and implementation planning.*

### Recommended Tools & Technologies

- **Date:** April 16, 2026 **Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure **Project:** Single-page executive reporting dashboard with timeline, heatmap, and milestone tracking --- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (built-in) | .NET 8.0 | No additional package needed. Ships with `Microsoft.AspNetCore.App` | | **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Match the reference design's `grid-template-columns: 160px repeat(4,1fr)` exactly | | **SVG Rendering** | Inline SVG via Razor | N/A | No charting library — hand-craft SVG in `.razor` components | | **Icons** | None (inline SVG shapes) | N/A | Diamonds, circles, lines are all defined inline per the reference | | **Fonts** | Segoe UI (system font) | N/A | Already available on Windows; no web font loading needed |
- MudBlazor, Radzen, or Syncfusion — they add massive dependency weight for a single-page dashboard that already has a pixel-perfect HTML reference
- Chart.js, Plotly, or any JS charting library — the SVG is simple enough to generate directly
- Bootstrap or Tailwind — the reference CSS is ~80 lines; a framework adds complexity without value | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` | | **File Watching** | `System.IO.FileSystemWatcher` | Built into .NET 8 | Watch `data.json` for changes, trigger UI refresh | | **Configuration** | Direct file read | N/A | Don't use `IConfiguration` — this is raw data, not app config | | **DI Registration** | `IServiceCollection` (built-in) | .NET 8 | Register a singleton `DashboardDataService` | | Layer | Technology | Notes | |-------|-----------|-------| | **Primary Store** | `data.json` flat file | Single JSON file in the project root or a configurable path | | **No Database** | — | SQLite, LiteDB, etc. are unnecessary for this use case | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **SDK** | .NET 8 SDK | 8.0.x (latest patch) | LTS release, supported through Nov 2026 | | **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | 17.8+ | Hot Reload support for Blazor | | **Unit Testing** | xUnit | 2.7.x | For data model deserialization tests | | **Assertions** | FluentAssertions | 6.12.x | MIT license, cleaner test syntax | | **Snapshot Testing** | Verify | 23.x | Optional — verify rendered HTML output against snapshots | | **Component Testing** | bUnit | 1.25.x | Optional — test Blazor components in isolation |
```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Data/
│       │   ├── DashboardDataService.cs
│       │   └── Models/
│       │       ├── DashboardData.cs
│       │       ├── Milestone.cs
│       │       ├── HeatmapRow.cs
│       │       └── WorkItem.cs
│       ├── Components/
│       │   ├── Pages/
│       │   │   └── Dashboard.razor
│       │   ├── Layout/
│       │   │   └── MainLayout.razor
│       │   ├── Timeline.razor
│       │   ├── Heatmap.razor
│       │   ├── HeatmapCell.razor
│       │   └── Legend.razor
│       ├── wwwroot/
│       │   └── css/
│       │       └── dashboard.css
│       └── data.json
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── DataServiceTests.cs
``` --- This is **not** an enterprise app. Do not apply Clean Architecture, CQRS, MediatR, or repository patterns. The architecture should be:
```
data.json → DashboardDataService (singleton) → Dashboard.razor → Child Components
```
- `DashboardDataService` reads `data.json` at startup and on file change
- Exposes a `DashboardData` property and an `OnDataChanged` event
- `Dashboard.razor` injects the service, subscribes to changes, calls `StateHasChanged()`
- Child components (`Timeline.razor`, `Heatmap.razor`) receive data via `[Parameter]` cascading
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project",
  "currentDate": "2026-04-10",
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
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Item A", "Item B"],
        "Feb": ["Item C"],
        "Mar": ["Item D", "Item E"],
        "Apr": ["Item F"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item G", "Item H"] }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item I"] }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item J"] }
    }
  ]
}
``` The reference SVG is 1560×185px with month columns at 260px intervals. Translate this directly into a Blazor component:
```razor
@* Timeline.razor - generates SVG inline *@
<svg xmlns="http://www.w3.org/2000/svg" width="1560" height="185">
    @foreach (var month in MonthPositions)
    {
        <line x1="@month.X" y1="0" x2="@month.X" y2="185" stroke="#bbb" stroke-opacity="0.4"/>
        <text x="@(month.X + 5)" y="14" fill="#666" font-size="11">@month.Label</text>
    }
    @* "NOW" line *@
    <line x1="@NowX" y1="0" x2="@NowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
    @foreach (var stream in Data.MilestoneStreams)
    {
        @* Render horizontal track line + milestone markers *@
    }
</svg>
``` Key calculation: `X = (date - timelineStart).TotalDays / (timelineEnd - timelineStart).TotalDays * 1560` **Copy the reference CSS almost verbatim.** The HTML design's CSS is clean, minimal (~80 lines), and purpose-built. Port it into `dashboard.css` with these adjustments:
- Change `body { width: 1920px; height: 1080px; }` to use a CSS container that can optionally scale
- Keep all the heatmap cell color classes (`.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`) exactly as-is
- Keep the grid definition `grid-template-columns: 160px repeat(4,1fr)` — but make the column count dynamic based on `data.json` month count
- Use CSS custom properties for the color palette to enable easy theming:
```css
:root {
    --color-shipped: #34A853;
    --color-progress: #0078D4;
    --color-carryover: #F4B400;
    --color-blocker: #EA4335;
}
``` Since the primary output is PowerPoint screenshots at 1920×1080:
- Set the page to render at exactly 1920×1080 using a fixed viewport
- Use `@media print` styles that hide browser chrome
- Consider adding a `?screenshot=true` query parameter that hides any interactive elements and forces the fixed layout ---

### Considerations & Risks

- **None.** This is explicitly out of scope. The app runs locally on `localhost:5000` (or similar) and is accessed only by the person running it. No auth middleware, no identity provider, no cookies. If this ever needs to be shared on a network, add a single line: `builder.Services.AddAuthentication()` with Windows Authentication — but defer this decision.
- `data.json` contains project names and status — no PII, no secrets
- No encryption needed for data at rest
- No HTTPS needed for local-only access (Kestrel HTTP on localhost is fine) | Aspect | Recommendation | |--------|---------------| | **Runtime** | Self-hosted Kestrel (built into .NET 8) | | **Launch** | `dotnet run` from project directory | | **Port** | `http://localhost:5050` (configurable in `launchSettings.json`) | | **Deployment** | `dotnet publish -c Release` → copy to target machine | | **Container** | Not needed — this is a local dev tool | | **CI/CD** | Not needed for v1 — optional GitHub Actions for build validation | **$0.** This runs on the developer's existing workstation. No cloud resources, no licenses, no subscriptions. --- | Risk | Likelihood | Impact | Mitigation | |------|-----------|--------|-----------| | **Over-engineering** | HIGH | Schedule slip, complexity | Enforce "no database, no auth, no API" rule. If a feature doesn't serve screenshots, cut it. | | **SVG rendering inconsistencies across browsers** | LOW | Visual bugs in screenshots | Standardize on Edge/Chrome for screenshots. Test SVG output once and lock it. | | **Blazor Server SignalR overhead** | LOW | Unnecessary for static content | Acceptable trade-off — the alternative (Blazor Static SSR) requires .NET 8 minimal APIs which adds complexity. Server mode gives us hot reload and `FileSystemWatcher` integration for free. | | **data.json schema drift** | MEDIUM | Runtime deserialization errors | Define a strong C# model with `[Required]` attributes. Add a startup validation step that logs clear errors if `data.json` is malformed. | | **Scope creep toward "real app"** | HIGH | Delays, complexity | This is a reporting screenshot tool. Resist adding interactivity, filtering, drill-downs, or multi-page navigation. |
- **Blazor Server vs. Static HTML generation:** Blazor adds a runtime dependency, but provides hot reload, component reuse, and C# data binding — worth it for maintainability.
- **No component library:** More upfront CSS work, but the design is already specified in the HTML reference. A component library would fight the custom layout rather than help it.
- **Fixed 1920×1080 layout vs. responsive:** The primary use case is screenshots for PowerPoint. Responsive design adds complexity with no value. Use fixed layout.
- **JSON file vs. database:** JSON is harder to query but trivially editable in any text editor. For a single-page dashboard with <100 data points, this is the right choice. ---
- **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always show a fixed window? **Recommendation:** Make it data-driven (render whatever months are in the JSON), but optimize the CSS grid for 4–6 columns.
- **Should the timeline scale be configurable?** The reference shows Jan–Jun (6 months). If projects span longer periods, the SVG width calculations need to accommodate 12+ months. **Recommendation:** Drive from `timelineStart`/`timelineEnd` in `data.json`.
- **Multiple projects or single project?** The current design is one project per page. If multiple projects are needed, should they be separate `data.json` files with a project selector, or multiple pages? **Recommendation:** Start with single project. Add a dropdown later if needed.
- **Who edits `data.json`?** If non-technical stakeholders need to update it, consider adding a simple JSON editor UI in a future phase. For v1, assume the developer edits it directly.
- **Print/export to PDF?** If executives want the dashboard as a PDF attachment (not just a screenshot), Blazor Server can't natively export to PDF. This would require a headless browser capture tool like Playwright. **Recommendation:** Defer — screenshots are sufficient for v1.
- **Design file at `C:/Pics/ReportingDashboardDesign.png`:** This file needs to be reviewed by the implementing team to capture any design differences from the HTML reference. Are there additional visual elements or layout changes beyond what `OriginalDesignConcept.html` shows? ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Date:** April 16, 2026
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure
**Project:** Single-page executive reporting dashboard with timeline, heatmap, and milestone tracking

---

## 1. Executive Summary

This project is a **single-page, screenshot-friendly executive reporting dashboard** built with Blazor Server on .NET 8, running entirely locally with no cloud dependencies. The dashboard renders a project timeline with milestones (SVG-based Gantt-style visualization) and a color-coded heatmap grid showing work item status (Shipped, In Progress, Carryover, Blockers) across monthly columns. All data is driven from a local `data.json` file.

**Primary Recommendation:** Keep this dead simple. Use a single Blazor Server project with zero external JavaScript dependencies, inline SVG generation via Blazor components, pure CSS Grid/Flexbox for layout (matching the reference HTML exactly), and `System.Text.Json` for config deserialization. No database, no auth, no API layer. The entire app should be ~8-12 files total. This is a **data-visualization-as-code** project, not an enterprise application.

---

## 2. Key Findings

- The reference design (`OriginalDesignConcept.html`) is a **1920×1080 fixed-layout page** using CSS Grid (`160px repeat(4,1fr)`) and Flexbox, with inline SVG for the timeline — all of which translate directly to Blazor components with zero JavaScript needed.
- **No charting library is necessary.** The timeline is a simple SVG with lines, circles, diamonds, and text — easily generated with Blazor's `@` syntax inside `<svg>` elements. Adding a charting library would be overengineering.
- **JSON file as data source is the right call.** For a screenshot-oriented reporting tool, a `data.json` file provides the simplest edit-and-refresh workflow. No database overhead, no migration complexity.
- The color palette and layout from the HTML reference are production-ready. The design uses Microsoft's Segoe UI font family and Fluent-adjacent colors (#0078D4 blue, #34A853 green, #F4B400 amber, #EA4335 red) — these should be preserved exactly.
- **Blazor Server is ideal for this use case** because it runs locally, supports hot reload during development, renders server-side (great for consistent screenshot output), and requires no WASM download penalty.
- The heatmap grid pattern (status rows × month columns with colored cells containing bullet-pointed items) maps perfectly to a nested `@foreach` loop over a well-structured data model.
- **File watching for `data.json`** via `FileSystemWatcher` enables live-reload of data without restarting the app — a significant quality-of-life feature for iterating on report content.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components + CSS)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 | No additional package needed. Ships with `Microsoft.AspNetCore.App` |
| **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Match the reference design's `grid-template-columns: 160px repeat(4,1fr)` exactly |
| **SVG Rendering** | Inline SVG via Razor | N/A | No charting library — hand-craft SVG in `.razor` components |
| **Icons** | None (inline SVG shapes) | N/A | Diamonds, circles, lines are all defined inline per the reference |
| **Fonts** | Segoe UI (system font) | N/A | Already available on Windows; no web font loading needed |

**Do NOT use:**
- MudBlazor, Radzen, or Syncfusion — they add massive dependency weight for a single-page dashboard that already has a pixel-perfect HTML reference
- Chart.js, Plotly, or any JS charting library — the SVG is simple enough to generate directly
- Bootstrap or Tailwind — the reference CSS is ~80 lines; a framework adds complexity without value

### Backend (Data Loading)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` |
| **File Watching** | `System.IO.FileSystemWatcher` | Built into .NET 8 | Watch `data.json` for changes, trigger UI refresh |
| **Configuration** | Direct file read | N/A | Don't use `IConfiguration` — this is raw data, not app config |
| **DI Registration** | `IServiceCollection` (built-in) | .NET 8 | Register a singleton `DashboardDataService` |

### Data Storage

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Primary Store** | `data.json` flat file | Single JSON file in the project root or a configurable path |
| **No Database** | — | SQLite, LiteDB, etc. are unnecessary for this use case |

### Development & Testing

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **SDK** | .NET 8 SDK | 8.0.x (latest patch) | LTS release, supported through Nov 2026 |
| **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | 17.8+ | Hot Reload support for Blazor |
| **Unit Testing** | xUnit | 2.7.x | For data model deserialization tests |
| **Assertions** | FluentAssertions | 6.12.x | MIT license, cleaner test syntax |
| **Snapshot Testing** | Verify | 23.x | Optional — verify rendered HTML output against snapshots |
| **Component Testing** | bUnit | 1.25.x | Optional — test Blazor components in isolation |

### Project Structure

```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Data/
│       │   ├── DashboardDataService.cs
│       │   └── Models/
│       │       ├── DashboardData.cs
│       │       ├── Milestone.cs
│       │       ├── HeatmapRow.cs
│       │       └── WorkItem.cs
│       ├── Components/
│       │   ├── Pages/
│       │   │   └── Dashboard.razor
│       │   ├── Layout/
│       │   │   └── MainLayout.razor
│       │   ├── Timeline.razor
│       │   ├── Heatmap.razor
│       │   ├── HeatmapCell.razor
│       │   └── Legend.razor
│       ├── wwwroot/
│       │   └── css/
│       │       └── dashboard.css
│       └── data.json
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── DataServiceTests.cs
```

---

## 4. Architecture Recommendations

### Pattern: Single-Component Page with Child Components

This is **not** an enterprise app. Do not apply Clean Architecture, CQRS, MediatR, or repository patterns. The architecture should be:

```
data.json → DashboardDataService (singleton) → Dashboard.razor → Child Components
```

**Data Flow:**
1. `DashboardDataService` reads `data.json` at startup and on file change
2. Exposes a `DashboardData` property and an `OnDataChanged` event
3. `Dashboard.razor` injects the service, subscribes to changes, calls `StateHasChanged()`
4. Child components (`Timeline.razor`, `Heatmap.razor`) receive data via `[Parameter]` cascading

### Data Model Design (for `data.json`)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project",
  "currentDate": "2026-04-10",
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
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Item A", "Item B"],
        "Feb": ["Item C"],
        "Mar": ["Item D", "Item E"],
        "Apr": ["Item F"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item G", "Item H"] }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item I"] }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Item J"] }
    }
  ]
}
```

### SVG Timeline Generation Strategy

The reference SVG is 1560×185px with month columns at 260px intervals. Translate this directly into a Blazor component:

```razor
@* Timeline.razor - generates SVG inline *@
<svg xmlns="http://www.w3.org/2000/svg" width="1560" height="185">
    @foreach (var month in MonthPositions)
    {
        <line x1="@month.X" y1="0" x2="@month.X" y2="185" stroke="#bbb" stroke-opacity="0.4"/>
        <text x="@(month.X + 5)" y="14" fill="#666" font-size="11">@month.Label</text>
    }
    @* "NOW" line *@
    <line x1="@NowX" y1="0" x2="@NowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
    @foreach (var stream in Data.MilestoneStreams)
    {
        @* Render horizontal track line + milestone markers *@
    }
</svg>
```

Key calculation: `X = (date - timelineStart).TotalDays / (timelineEnd - timelineStart).TotalDays * 1560`

### CSS Architecture

**Copy the reference CSS almost verbatim.** The HTML design's CSS is clean, minimal (~80 lines), and purpose-built. Port it into `dashboard.css` with these adjustments:

1. Change `body { width: 1920px; height: 1080px; }` to use a CSS container that can optionally scale
2. Keep all the heatmap cell color classes (`.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`) exactly as-is
3. Keep the grid definition `grid-template-columns: 160px repeat(4,1fr)` — but make the column count dynamic based on `data.json` month count
4. Use CSS custom properties for the color palette to enable easy theming:

```css
:root {
    --color-shipped: #34A853;
    --color-progress: #0078D4;
    --color-carryover: #F4B400;
    --color-blocker: #EA4335;
}
```

### Rendering for Screenshots

Since the primary output is PowerPoint screenshots at 1920×1080:
- Set the page to render at exactly 1920×1080 using a fixed viewport
- Use `@media print` styles that hide browser chrome
- Consider adding a `?screenshot=true` query parameter that hides any interactive elements and forces the fixed layout

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope. The app runs locally on `localhost:5000` (or similar) and is accessed only by the person running it. No auth middleware, no identity provider, no cookies.

If this ever needs to be shared on a network, add a single line: `builder.Services.AddAuthentication()` with Windows Authentication — but defer this decision.

### Data Protection

- `data.json` contains project names and status — no PII, no secrets
- No encryption needed for data at rest
- No HTTPS needed for local-only access (Kestrel HTTP on localhost is fine)

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Self-hosted Kestrel (built into .NET 8) |
| **Launch** | `dotnet run` from project directory |
| **Port** | `http://localhost:5050` (configurable in `launchSettings.json`) |
| **Deployment** | `dotnet publish -c Release` → copy to target machine |
| **Container** | Not needed — this is a local dev tool |
| **CI/CD** | Not needed for v1 — optional GitHub Actions for build validation |

### Infrastructure Costs

**$0.** This runs on the developer's existing workstation. No cloud resources, no licenses, no subscriptions.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|-----------|
| **Over-engineering** | HIGH | Schedule slip, complexity | Enforce "no database, no auth, no API" rule. If a feature doesn't serve screenshots, cut it. |
| **SVG rendering inconsistencies across browsers** | LOW | Visual bugs in screenshots | Standardize on Edge/Chrome for screenshots. Test SVG output once and lock it. |
| **Blazor Server SignalR overhead** | LOW | Unnecessary for static content | Acceptable trade-off — the alternative (Blazor Static SSR) requires .NET 8 minimal APIs which adds complexity. Server mode gives us hot reload and `FileSystemWatcher` integration for free. |
| **data.json schema drift** | MEDIUM | Runtime deserialization errors | Define a strong C# model with `[Required]` attributes. Add a startup validation step that logs clear errors if `data.json` is malformed. |
| **Scope creep toward "real app"** | HIGH | Delays, complexity | This is a reporting screenshot tool. Resist adding interactivity, filtering, drill-downs, or multi-page navigation. |

### Trade-offs Accepted

1. **Blazor Server vs. Static HTML generation:** Blazor adds a runtime dependency, but provides hot reload, component reuse, and C# data binding — worth it for maintainability.
2. **No component library:** More upfront CSS work, but the design is already specified in the HTML reference. A component library would fight the custom layout rather than help it.
3. **Fixed 1920×1080 layout vs. responsive:** The primary use case is screenshots for PowerPoint. Responsive design adds complexity with no value. Use fixed layout.
4. **JSON file vs. database:** JSON is harder to query but trivially editable in any text editor. For a single-page dashboard with <100 data points, this is the right choice.

---

## 7. Open Questions

1. **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always show a fixed window? **Recommendation:** Make it data-driven (render whatever months are in the JSON), but optimize the CSS grid for 4–6 columns.

2. **Should the timeline scale be configurable?** The reference shows Jan–Jun (6 months). If projects span longer periods, the SVG width calculations need to accommodate 12+ months. **Recommendation:** Drive from `timelineStart`/`timelineEnd` in `data.json`.

3. **Multiple projects or single project?** The current design is one project per page. If multiple projects are needed, should they be separate `data.json` files with a project selector, or multiple pages? **Recommendation:** Start with single project. Add a dropdown later if needed.

4. **Who edits `data.json`?** If non-technical stakeholders need to update it, consider adding a simple JSON editor UI in a future phase. For v1, assume the developer edits it directly.

5. **Print/export to PDF?** If executives want the dashboard as a PDF attachment (not just a screenshot), Blazor Server can't natively export to PDF. This would require a headless browser capture tool like Playwright. **Recommendation:** Defer — screenshots are sufficient for v1.

6. **Design file at `C:/Pics/ReportingDashboardDesign.png`:** This file needs to be reviewed by the implementing team to capture any design differences from the HTML reference. Are there additional visual elements or layout changes beyond what `OriginalDesignConcept.html` shows?

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** Pixel-perfect reproduction of the HTML reference design, driven by `data.json`.

| Step | Task | Estimated Time |
|------|------|---------------|
| 1 | Create `.sln` and Blazor Server project from `dotnet new blazor --interactivity Server` | 15 min |
| 2 | Define C# data models (`DashboardData`, `Milestone`, `HeatmapCategory`, `WorkItem`) | 30 min |
| 3 | Create `DashboardDataService` that reads and deserializes `data.json` | 30 min |
| 4 | Port reference CSS into `dashboard.css` verbatim | 20 min |
| 5 | Build `Dashboard.razor` page with header section | 30 min |
| 6 | Build `Timeline.razor` component with SVG generation | 1.5 hrs |
| 7 | Build `Heatmap.razor` + `HeatmapCell.razor` components | 1.5 hrs |
| 8 | Create `data.json` with fictional project data | 30 min |
| 9 | Visual QA against reference screenshot at 1920×1080 | 30 min |

### Phase 2: Polish (Optional, 0.5–1 day)

| Step | Task |
|------|------|
| 1 | Add `FileSystemWatcher` for live `data.json` reload without app restart |
| 2 | Add `?screenshot=true` mode that hides scrollbars and forces exact 1920×1080 |
| 3 | Improve the design beyond the reference (subtle shadows, better typography spacing, refined color gradients) |
| 4 | Add startup validation of `data.json` with friendly error messages |
| 5 | Write xUnit tests for data model deserialization edge cases |

### Phase 3: Future Enhancements (Defer)

- Multi-project support with project selector dropdown
- Dark mode theme toggle
- Print-friendly CSS `@media print` styles
- Automated screenshot generation via Playwright
- Simple JSON editor UI for non-technical users

### Quick Wins

1. **Start from the HTML reference, not from scratch.** The CSS is already written and battle-tested. Port it directly rather than re-inventing the layout.
2. **Use `dotnet watch` during development** for instant hot reload when editing `.razor` files or CSS.
3. **Fictional data first.** Create compelling fake data for a fictional "Project Phoenix" to demonstrate the dashboard before wiring up real project data. This lets you iterate on the visual design independently.
4. **Single `.razor` page for v1.** Don't over-componentize. Start with everything in `Dashboard.razor` and extract components only when duplication appears. The reference HTML is ~150 lines — a single Razor page is fine.

### NuGet Packages Required

```xml
<!-- ReportingDashboard.csproj — no additional packages needed for core functionality -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>

<!-- ReportingDashboard.Tests.csproj (optional) -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="bunit" Version="1.25.3" />
  </ItemGroup>
</Project>
```

**The total external dependency count for the main project is zero.** Everything needed — Blazor Server, System.Text.Json, Kestrel, FileSystemWatcher — ships with .NET 8.

---

*Research completed April 16, 2026. Ready for architecture review and implementation planning.*

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/3655027c5cc25f566f904d58d91fa28042dbd729/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
