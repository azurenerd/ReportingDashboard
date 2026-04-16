# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-16 23:36 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline and displays a heatmap grid of work items categorized as Shipped, In Progress, Carryover, and Blockers — designed for screenshot capture into PowerPoint decks. **Primary Recommendation:** Build a minimal Blazor Server app with a single Razor component page, read all data from a local `data.json` file at startup, and render the timeline/heatmap entirely with inline SVG and CSS Grid — no JavaScript charting libraries needed. The HTML design reference already provides the exact CSS patterns; the Blazor implementation should translate these directly into scoped CSS and parameterized Razor markup. Total solution should be under 10 files and deployable with `dotnet run`. --- | Package | Version | Required | Purpose | |---------|---------|----------|---------| | `Microsoft.AspNetCore.Components.Web` | 8.0.x | ✅ Built-in | Blazor Server framework | | `System.Text.Json` | 8.0.x | ✅ Built-in | JSON deserialization | | — | — | — | **That's it for MVP** | | `xUnit` | 2.7+ | Optional | Unit testing | | `bUnit` | 1.25+ | Optional | Blazor component testing | | `FluentAssertions` | 6.12+ | Optional | Test readability | | `JsonSchema.Net` | 7.x | Optional | JSON validation at startup | **The MVP requires zero additional NuGet packages beyond what ships with the .NET 8 Blazor Server template.** This is the strongest endorsement of the stack choice — everything needed is built-in.

### Key Findings

- The existing `OriginalDesignConcept.html` is a self-contained, pixel-perfect design using only CSS Grid, Flexbox, and inline SVG — no JavaScript libraries. This means the Blazor implementation needs **zero JS interop** for rendering.
- The design targets a fixed **1920×1080 viewport** optimized for screenshots, not responsive use. This simplifies layout work significantly — no breakpoints or mobile considerations needed.
- Blazor Server's real-time SignalR connection is overkill for this use case, but it's the mandated stack and still works well for local-only scenarios with negligible latency.
- A `data.json` file is the ideal data source — no database needed. `System.Text.Json` (built into .NET 8) handles deserialization natively with no additional packages.
- The SVG timeline with milestones, diamonds (PoC), circles (checkpoints), and a "NOW" line can be generated entirely in Razor markup using `@foreach` loops and calculated x-coordinates.
- The heatmap grid (4 status rows × N month columns) maps directly to CSS Grid with parameterized `grid-template-columns` based on the number of months in the data.
- The color palette is fully defined in the HTML reference: green (#34A853) for shipped, blue (#0078D4) for in-progress, amber (#F4B400) for carryover, red (#EA4335) for blockers. These should be CSS custom properties for maintainability.
- No authentication, authorization, or security hardening is needed — this is a local tool for one user.
- The entire project can be scaffolded and functional in a single development sprint. --- **Goal:** Pixel-perfect reproduction of the HTML reference design, driven by `data.json`. | Task | Estimate | Details | |------|----------|---------| | Scaffold Blazor Server project | 30 min | `dotnet new blazor -n ReportingDashboard --interactivity Server` | | Define `DashboardData` model classes | 1 hr | Mirror the JSON schema above. Use `record` types for immutability | | Create `DashboardDataService` | 30 min | Singleton, reads `data.json` from `wwwroot/`, deserializes with `System.Text.Json` | | Build `Header.razor` component | 1 hr | Title, subtitle, legend icons. Direct port from HTML reference | | Build `Timeline.razor` component | 3 hr | SVG generation with computed milestone positions. Most complex component | | Build `Heatmap.razor` + `HeatmapCell.razor` | 2 hr | CSS Grid layout, color-coded rows, bullet-prefixed items | | Create sample `data.json` | 30 min | Fictional project data for demo/testing | | CSS polish and screenshot verification | 1 hr | Compare browser render against reference PNG at 1920×1080 | | Task | Details | |------|---------| | Add `data.sample.json` with schema documentation | Help future users understand the format | | Startup validation of `data.json` | Friendly error page if JSON is malformed | | CSS custom properties for easy re-theming | Swap color palette per project type | | `FileSystemWatcher` auto-reload | Dashboard refreshes when JSON file is saved | | `?print=true` mode | Hides any dev-only UI, optimizes for screenshot | | Task | Details | |------|---------| | Multi-project support | Dropdown selector, multiple JSON files | | Simple JSON editor form | Blazor form to edit `data.json` without hand-editing | | ADO integration | Pull work item data from Azure DevOps REST API into `data.json` format | | Automated screenshot capture | Playwright script to render and save PNG on schedule |
- **Start by copying the CSS from the HTML reference directly into `app.css`.** The design is already complete — don't redesign it.
- **Use `dotnet watch` during development** for instant feedback on Razor/CSS changes.
- **Create `data.json` first, then build components.** Having real data to render against prevents back-and-forth on the model design.
- **Test screenshots early** — render the page, take a screenshot, overlay it against the reference PNG. Catch pixel drift before building all components.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Framework** | Blazor Server (.NET 8) | `net8.0` | Mandated stack. Use `dotnet new blazor --interactivity Server` template | | **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches the HTML reference exactly. No CSS framework needed | | **SVG Rendering** | Inline SVG in Razor | N/A | Timeline milestones rendered as `<svg>` elements with Razor `@` expressions for coordinates | | **Icons/Shapes** | Hand-coded SVG | N/A | Diamonds (`<polygon>`), circles (`<circle>`), lines (`<line>`) — all in the reference HTML | | **Font** | Segoe UI | System font | No web font loading needed; available on all Windows machines | | **CSS Isolation** | Blazor scoped CSS (`.razor.css`) | Built-in | One scoped CSS file per component to avoid style leakage | **No additional frontend NuGet packages are required.** The design is achievable with pure HTML/CSS/SVG rendered by Razor. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Native, fast, no extra dependency. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` | | **File Watching (optional)** | `FileSystemWatcher` | Built into .NET 8 | Auto-reload dashboard when `data.json` is edited. Low priority but nice UX | | **Configuration** | `IConfiguration` + custom JSON | Built-in | Load `data.json` as a typed options object via `IOptions<DashboardData>` or direct deserialization | | Approach | Technology | Rationale | |----------|-----------|-----------| | **Primary** | `data.json` flat file | Single JSON file in project root. No database. Edited by hand or generated by scripts | | **Schema Validation (optional)** | `JsonSchema.Net` v7.x | MIT license. Validate `data.json` structure at startup to catch typos. Optional but recommended | **Do NOT use a database.** SQLite, LiteDB, or any ORM would be overengineering for a single JSON config file. | Purpose | Technology | Version | Notes | |---------|-----------|---------|-------| | **Unit Tests** | xUnit | 2.7+ | .NET ecosystem standard. Test data model deserialization and coordinate calculations | | **Assertions** | FluentAssertions | 6.12+ | Readable assertions. MIT license | | **UI Testing** | bUnit | 1.25+ | Blazor component testing. Verify rendered HTML structure matches expected output | | **Screenshot Comparison (optional)** | Playwright for .NET | 1.41+ | If automated screenshot regression is desired. Low priority for MVP | | Purpose | Tool | Notes | |---------|------|-------| | **Build** | `dotnet build` | Standard .NET CLI | | **Run** | `dotnet run` or `dotnet watch` | Hot reload during development | | **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Full Blazor tooling support | | **Solution Structure** | `.sln` with single `.csproj` | Keep it flat — one project is sufficient | ---
```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── wwwroot/
│   │   └── data.json              ← Dashboard data (served as static file too)
│   ├── Models/
│   │   └── DashboardData.cs       ← Strongly-typed model for data.json
│   ├── Services/
│   │   └── DashboardDataService.cs ← Reads and deserializes data.json
│   ├── Components/
│   │   ├── Layout/
│   │   │   └── MainLayout.razor
│   │   └── Pages/
│   │       └── Dashboard.razor     ← The single page
│   ├── Components/Shared/
│   │   ├── Header.razor            ← Title, subtitle, legend
│   │   ├── Timeline.razor          ← SVG milestone visualization
│   │   ├── Heatmap.razor           ← Status grid
│   │   └── HeatmapCell.razor       ← Individual cell with work items
│   └── Properties/
│       └── launchSettings.json
```
- **Single project** — no class libraries, no shared projects. This is a one-page app.
- **Component decomposition** mirrors the visual sections: Header → Timeline → Heatmap. Each is independently testable with bUnit.
- **Models folder** keeps the `data.json` contract in one place.
- **Services folder** contains the single data service.
```
data.json → DashboardDataService (singleton) → Dashboard.razor → Child Components
```
- `DashboardDataService` is registered as a **singleton** in `Program.cs`.
- On app startup (or on-demand with caching), it reads `data.json` from disk using `System.Text.Json`.
- `Dashboard.razor` injects the service and passes typed model objects to child components as `[Parameter]` props.
- No API controllers, no HTTP endpoints, no SignalR custom hubs — Blazor Server's built-in circuit handles everything.
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
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonthIndex": 3,
    "rows": [
      {
        "category": "shipped",
        "items": {
          "Jan": ["Item A", "Item B"],
          "Feb": ["Item C"],
          "Mar": [],
          "Apr": ["Item D", "Item E"]
        }
      },
      {
        "category": "in-progress",
        "items": { ... }
      },
      {
        "category": "carryover",
        "items": { ... }
      },
      {
        "category": "blockers",
        "items": { ... }
      }
    ]
  }
}
```
- `currentMonthIndex` drives the highlighted column (amber background in the reference).
- `milestones[].events[].type` maps to SVG shapes: `"checkpoint"` → circle, `"poc"` → yellow diamond, `"production"` → green diamond.
- Category names map directly to CSS class prefixes: `shipped` → `.ship-*`, `in-progress` → `.prog-*`, etc. The timeline SVG should be generated in `Timeline.razor` with computed positions:
```csharp
@code {
    private double GetXPosition(DateOnly date)
    {
        var totalDays = (TimelineEnd.ToDateTime(TimeOnly.MinValue) - TimelineStart.ToDateTime(TimeOnly.MinValue)).TotalDays;
        var dayOffset = (date.ToDateTime(TimeOnly.MinValue) - TimelineStart.ToDateTime(TimeOnly.MinValue)).TotalDays;
        return (dayOffset / totalDays) * SvgWidth;
    }
}
``` This replaces the hard-coded pixel positions in the HTML reference with data-driven calculations.
```css
/* wwwroot/app.css — CSS custom properties */
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
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-current: #FFE4E4;
    --color-accent: #0078D4;
    --font-family: 'Segoe UI', Arial, sans-serif;
}
``` This allows easy re-theming for different projects or teams without touching component CSS. Since the primary output is PowerPoint screenshots:
- Set `<meta name="viewport" content="width=1920">` and fix the body to 1920×1080.
- Use `font-smoothing: antialiased` for crisp text in screenshots.
- Avoid any animations or transitions — they cause artifacts in screenshots.
- Consider adding a `?print=true` query parameter mode that hides browser chrome artifacts and sets a clean white background. ---

### Considerations & Risks

- **None required.** This is a local-only tool for a single user generating screenshots. No login, no roles, no tokens. If future sharing is needed, the simplest addition would be Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (one line in `Program.cs`), but this is out of scope.
- `data.json` contains project metadata that may be sensitive (project names, milestone dates, blocker descriptions). Ensure the file is not committed to public repositories.
- Add `data.json` to `.gitignore` and provide a `data.sample.json` template instead.
- No encryption needed for local file storage. | Aspect | Recommendation | |--------|---------------| | **Runtime** | Local `dotnet run` on developer machine | | **Port** | Default Kestrel on `https://localhost:5001` or `http://localhost:5000` | | **Reverse Proxy** | Not needed — direct Kestrel access | | **Containerization** | Not needed for local use. If desired later, a simple `Dockerfile` with `mcr.microsoft.com/dotnet/aspnet:8.0` base image | | **Process Management** | Manual start/stop. Optionally create a `.bat`/`.ps1` script for one-click launch | **$0.** Everything runs locally. No cloud services, no databases, no subscriptions. --- | Risk | Severity | Mitigation | |------|----------|------------| | **Blazor Server is heavyweight for a static page** | Low | Acceptable trade-off for mandated stack. The SignalR circuit adds ~50KB overhead but works fine locally. Could use Blazor Static SSR (new in .NET 8) to eliminate the circuit entirely | | **SVG timeline rendering edge cases** | Medium | Milestones clustering on the same date may overlap labels. Implement a simple collision detection that offsets overlapping labels vertically | | **data.json schema drift** | Low | Add startup validation with a strongly-typed model. `System.Text.Json` throws clear errors on missing required properties if using `[Required]` attributes | | **Hot reload breaks SVG** | Low | Known .NET 8 issue where hot reload doesn't always re-render SVG. Use `dotnet watch` with `--no-hot-reload` flag if encountered | | **Screenshot consistency** | Medium | Different browsers render SVG slightly differently. Standardize on Edge/Chrome for screenshots. Document the browser + zoom level (100%) in the README | | Decision | Trade-off | Rationale | |----------|-----------|-----------| | No database | Cannot query historical data | Simplicity is the goal. Historical snapshots can be managed by versioning `data.json` files (e.g., `data.2026-04.json`) | | Fixed 1920×1080 | Not usable on mobile/small screens | Explicitly designed for screenshot capture, not interactive use | | No JS charting library | Limited to what SVG + CSS can do | The reference design proves this is sufficient. Adding Chart.js or ApexCharts would introduce JS interop complexity for no visual benefit | | Blazor Server vs Static SSR | Persistent SignalR connection for a read-only page | Blazor Server is the mandated stack. For pure screenshot use, Static SSR rendering mode (`@rendermode InteractiveServer` can be omitted) would eliminate the circuit | .NET 8 introduced **Static Server-Side Rendering (Static SSR)** as a render mode within the Blazor framework. For a read-only dashboard with no interactivity:
- **Static SSR** (`@attribute [StreamRendering]` without `@rendermode InteractiveServer`) renders HTML on the server and sends it without a SignalR circuit. This is lighter and more screenshot-friendly.
- **Interactive Server** maintains a SignalR connection for real-time updates. Only needed if you want auto-refresh when `data.json` changes. **Recommendation:** Start with **Static SSR** for simplicity. Add `@rendermode InteractiveServer` only if you implement features like auto-refresh or interactive tooltips later. --- | # | Question | Who Decides | Impact | |---|----------|-------------|--------| | 1 | **Should the dashboard auto-refresh when `data.json` is modified?** | Product Owner | Determines whether to use Static SSR or Interactive Server mode. Auto-refresh requires a `FileSystemWatcher` + SignalR push | | 2 | **How many months should the heatmap display?** | Stakeholder | The reference shows 4 months (Jan–Apr). If it's always a rolling window, the grid column count is dynamic. If fixed per report, it's simpler | | 3 | **Should multiple project dashboards be supported?** | Product Owner | If yes, need a project selector or multiple `data.json` files. If no, single-file is simplest | | 4 | **What is the `data.json` authoring workflow?** | Engineering Lead | Will users hand-edit JSON? Use a simple form? Auto-generate from ADO queries? This affects whether we need a JSON editor UI | | 5 | **Is the "Now" line always today's date or configurable?** | Product Owner | The reference has a fixed "NOW" marker. Could be `DateTime.Now` or a `currentDate` field in `data.json` for point-in-time screenshots | | 6 | **Should the ADO Backlog link in the header be configurable per project?** | Stakeholder | Minor but affects the data model | | 7 | **Print/export to PDF needed?** | Product Owner | If yes, consider adding a `window.print()` button with `@media print` CSS. Browser print-to-PDF at 1920×1080 works well | ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline and displays a heatmap grid of work items categorized as Shipped, In Progress, Carryover, and Blockers — designed for screenshot capture into PowerPoint decks.

**Primary Recommendation:** Build a minimal Blazor Server app with a single Razor component page, read all data from a local `data.json` file at startup, and render the timeline/heatmap entirely with inline SVG and CSS Grid — no JavaScript charting libraries needed. The HTML design reference already provides the exact CSS patterns; the Blazor implementation should translate these directly into scoped CSS and parameterized Razor markup. Total solution should be under 10 files and deployable with `dotnet run`.

---

## 2. Key Findings

- The existing `OriginalDesignConcept.html` is a self-contained, pixel-perfect design using only CSS Grid, Flexbox, and inline SVG — no JavaScript libraries. This means the Blazor implementation needs **zero JS interop** for rendering.
- The design targets a fixed **1920×1080 viewport** optimized for screenshots, not responsive use. This simplifies layout work significantly — no breakpoints or mobile considerations needed.
- Blazor Server's real-time SignalR connection is overkill for this use case, but it's the mandated stack and still works well for local-only scenarios with negligible latency.
- A `data.json` file is the ideal data source — no database needed. `System.Text.Json` (built into .NET 8) handles deserialization natively with no additional packages.
- The SVG timeline with milestones, diamonds (PoC), circles (checkpoints), and a "NOW" line can be generated entirely in Razor markup using `@foreach` loops and calculated x-coordinates.
- The heatmap grid (4 status rows × N month columns) maps directly to CSS Grid with parameterized `grid-template-columns` based on the number of months in the data.
- The color palette is fully defined in the HTML reference: green (#34A853) for shipped, blue (#0078D4) for in-progress, amber (#F4B400) for carryover, red (#EA4335) for blockers. These should be CSS custom properties for maintainability.
- No authentication, authorization, or security hardening is needed — this is a local tool for one user.
- The entire project can be scaffolded and functional in a single development sprint.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server UI)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Framework** | Blazor Server (.NET 8) | `net8.0` | Mandated stack. Use `dotnet new blazor --interactivity Server` template |
| **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches the HTML reference exactly. No CSS framework needed |
| **SVG Rendering** | Inline SVG in Razor | N/A | Timeline milestones rendered as `<svg>` elements with Razor `@` expressions for coordinates |
| **Icons/Shapes** | Hand-coded SVG | N/A | Diamonds (`<polygon>`), circles (`<circle>`), lines (`<line>`) — all in the reference HTML |
| **Font** | Segoe UI | System font | No web font loading needed; available on all Windows machines |
| **CSS Isolation** | Blazor scoped CSS (`.razor.css`) | Built-in | One scoped CSS file per component to avoid style leakage |

**No additional frontend NuGet packages are required.** The design is achievable with pure HTML/CSS/SVG rendered by Razor.

### Backend (Data Layer)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Native, fast, no extra dependency. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` |
| **File Watching (optional)** | `FileSystemWatcher` | Built into .NET 8 | Auto-reload dashboard when `data.json` is edited. Low priority but nice UX |
| **Configuration** | `IConfiguration` + custom JSON | Built-in | Load `data.json` as a typed options object via `IOptions<DashboardData>` or direct deserialization |

### Data Storage

| Approach | Technology | Rationale |
|----------|-----------|-----------|
| **Primary** | `data.json` flat file | Single JSON file in project root. No database. Edited by hand or generated by scripts |
| **Schema Validation (optional)** | `JsonSchema.Net` v7.x | MIT license. Validate `data.json` structure at startup to catch typos. Optional but recommended |

**Do NOT use a database.** SQLite, LiteDB, or any ORM would be overengineering for a single JSON config file.

### Testing

| Purpose | Technology | Version | Notes |
|---------|-----------|---------|-------|
| **Unit Tests** | xUnit | 2.7+ | .NET ecosystem standard. Test data model deserialization and coordinate calculations |
| **Assertions** | FluentAssertions | 6.12+ | Readable assertions. MIT license |
| **UI Testing** | bUnit | 1.25+ | Blazor component testing. Verify rendered HTML structure matches expected output |
| **Screenshot Comparison (optional)** | Playwright for .NET | 1.41+ | If automated screenshot regression is desired. Low priority for MVP |

### Build & Tooling

| Purpose | Tool | Notes |
|---------|------|-------|
| **Build** | `dotnet build` | Standard .NET CLI |
| **Run** | `dotnet run` or `dotnet watch` | Hot reload during development |
| **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Full Blazor tooling support |
| **Solution Structure** | `.sln` with single `.csproj` | Keep it flat — one project is sufficient |

---

## 4. Architecture Recommendations

### Overall Pattern: Single-Project, Component-Based

```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── wwwroot/
│   │   └── data.json              ← Dashboard data (served as static file too)
│   ├── Models/
│   │   └── DashboardData.cs       ← Strongly-typed model for data.json
│   ├── Services/
│   │   └── DashboardDataService.cs ← Reads and deserializes data.json
│   ├── Components/
│   │   ├── Layout/
│   │   │   └── MainLayout.razor
│   │   └── Pages/
│   │       └── Dashboard.razor     ← The single page
│   ├── Components/Shared/
│   │   ├── Header.razor            ← Title, subtitle, legend
│   │   ├── Timeline.razor          ← SVG milestone visualization
│   │   ├── Heatmap.razor           ← Status grid
│   │   └── HeatmapCell.razor       ← Individual cell with work items
│   └── Properties/
│       └── launchSettings.json
```

**Why this structure:**
- **Single project** — no class libraries, no shared projects. This is a one-page app.
- **Component decomposition** mirrors the visual sections: Header → Timeline → Heatmap. Each is independently testable with bUnit.
- **Models folder** keeps the `data.json` contract in one place.
- **Services folder** contains the single data service.

### Data Flow

```
data.json → DashboardDataService (singleton) → Dashboard.razor → Child Components
```

1. `DashboardDataService` is registered as a **singleton** in `Program.cs`.
2. On app startup (or on-demand with caching), it reads `data.json` from disk using `System.Text.Json`.
3. `Dashboard.razor` injects the service and passes typed model objects to child components as `[Parameter]` props.
4. No API controllers, no HTTP endpoints, no SignalR custom hubs — Blazor Server's built-in circuit handles everything.

### Data Model Design (`data.json`)

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
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonthIndex": 3,
    "rows": [
      {
        "category": "shipped",
        "items": {
          "Jan": ["Item A", "Item B"],
          "Feb": ["Item C"],
          "Mar": [],
          "Apr": ["Item D", "Item E"]
        }
      },
      {
        "category": "in-progress",
        "items": { ... }
      },
      {
        "category": "carryover",
        "items": { ... }
      },
      {
        "category": "blockers",
        "items": { ... }
      }
    ]
  }
}
```

**Key design decisions:**
- `currentMonthIndex` drives the highlighted column (amber background in the reference).
- `milestones[].events[].type` maps to SVG shapes: `"checkpoint"` → circle, `"poc"` → yellow diamond, `"production"` → green diamond.
- Category names map directly to CSS class prefixes: `shipped` → `.ship-*`, `in-progress` → `.prog-*`, etc.

### SVG Timeline Coordinate Calculation

The timeline SVG should be generated in `Timeline.razor` with computed positions:

```csharp
@code {
    private double GetXPosition(DateOnly date)
    {
        var totalDays = (TimelineEnd.ToDateTime(TimeOnly.MinValue) - TimelineStart.ToDateTime(TimeOnly.MinValue)).TotalDays;
        var dayOffset = (date.ToDateTime(TimeOnly.MinValue) - TimelineStart.ToDateTime(TimeOnly.MinValue)).TotalDays;
        return (dayOffset / totalDays) * SvgWidth;
    }
}
```

This replaces the hard-coded pixel positions in the HTML reference with data-driven calculations.

### CSS Strategy

**Recommendation: Use Blazor CSS isolation (`.razor.css` files) plus a single `app.css` for shared variables.**

```css
/* wwwroot/app.css — CSS custom properties */
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
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-current: #FFE4E4;
    --color-accent: #0078D4;
    --font-family: 'Segoe UI', Arial, sans-serif;
}
```

This allows easy re-theming for different projects or teams without touching component CSS.

### Rendering for Screenshot Quality

Since the primary output is PowerPoint screenshots:
- Set `<meta name="viewport" content="width=1920">` and fix the body to 1920×1080.
- Use `font-smoothing: antialiased` for crisp text in screenshots.
- Avoid any animations or transitions — they cause artifacts in screenshots.
- Consider adding a `?print=true` query parameter mode that hides browser chrome artifacts and sets a clean white background.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None required.** This is a local-only tool for a single user generating screenshots. No login, no roles, no tokens.

If future sharing is needed, the simplest addition would be Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (one line in `Program.cs`), but this is out of scope.

### Data Protection

- `data.json` contains project metadata that may be sensitive (project names, milestone dates, blocker descriptions). Ensure the file is not committed to public repositories.
- Add `data.json` to `.gitignore` and provide a `data.sample.json` template instead.
- No encryption needed for local file storage.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Local `dotnet run` on developer machine |
| **Port** | Default Kestrel on `https://localhost:5001` or `http://localhost:5000` |
| **Reverse Proxy** | Not needed — direct Kestrel access |
| **Containerization** | Not needed for local use. If desired later, a simple `Dockerfile` with `mcr.microsoft.com/dotnet/aspnet:8.0` base image |
| **Process Management** | Manual start/stop. Optionally create a `.bat`/`.ps1` script for one-click launch |

### Infrastructure Costs

**$0.** Everything runs locally. No cloud services, no databases, no subscriptions.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **Blazor Server is heavyweight for a static page** | Low | Acceptable trade-off for mandated stack. The SignalR circuit adds ~50KB overhead but works fine locally. Could use Blazor Static SSR (new in .NET 8) to eliminate the circuit entirely |
| **SVG timeline rendering edge cases** | Medium | Milestones clustering on the same date may overlap labels. Implement a simple collision detection that offsets overlapping labels vertically |
| **data.json schema drift** | Low | Add startup validation with a strongly-typed model. `System.Text.Json` throws clear errors on missing required properties if using `[Required]` attributes |
| **Hot reload breaks SVG** | Low | Known .NET 8 issue where hot reload doesn't always re-render SVG. Use `dotnet watch` with `--no-hot-reload` flag if encountered |
| **Screenshot consistency** | Medium | Different browsers render SVG slightly differently. Standardize on Edge/Chrome for screenshots. Document the browser + zoom level (100%) in the README |

### Trade-offs Accepted

| Decision | Trade-off | Rationale |
|----------|-----------|-----------|
| No database | Cannot query historical data | Simplicity is the goal. Historical snapshots can be managed by versioning `data.json` files (e.g., `data.2026-04.json`) |
| Fixed 1920×1080 | Not usable on mobile/small screens | Explicitly designed for screenshot capture, not interactive use |
| No JS charting library | Limited to what SVG + CSS can do | The reference design proves this is sufficient. Adding Chart.js or ApexCharts would introduce JS interop complexity for no visual benefit |
| Blazor Server vs Static SSR | Persistent SignalR connection for a read-only page | Blazor Server is the mandated stack. For pure screenshot use, Static SSR rendering mode (`@rendermode InteractiveServer` can be omitted) would eliminate the circuit |

### Blazor Server vs. Static SSR Consideration

.NET 8 introduced **Static Server-Side Rendering (Static SSR)** as a render mode within the Blazor framework. For a read-only dashboard with no interactivity:

- **Static SSR** (`@attribute [StreamRendering]` without `@rendermode InteractiveServer`) renders HTML on the server and sends it without a SignalR circuit. This is lighter and more screenshot-friendly.
- **Interactive Server** maintains a SignalR connection for real-time updates. Only needed if you want auto-refresh when `data.json` changes.

**Recommendation:** Start with **Static SSR** for simplicity. Add `@rendermode InteractiveServer` only if you implement features like auto-refresh or interactive tooltips later.

---

## 7. Open Questions

| # | Question | Who Decides | Impact |
|---|----------|-------------|--------|
| 1 | **Should the dashboard auto-refresh when `data.json` is modified?** | Product Owner | Determines whether to use Static SSR or Interactive Server mode. Auto-refresh requires a `FileSystemWatcher` + SignalR push |
| 2 | **How many months should the heatmap display?** | Stakeholder | The reference shows 4 months (Jan–Apr). If it's always a rolling window, the grid column count is dynamic. If fixed per report, it's simpler |
| 3 | **Should multiple project dashboards be supported?** | Product Owner | If yes, need a project selector or multiple `data.json` files. If no, single-file is simplest |
| 4 | **What is the `data.json` authoring workflow?** | Engineering Lead | Will users hand-edit JSON? Use a simple form? Auto-generate from ADO queries? This affects whether we need a JSON editor UI |
| 5 | **Is the "Now" line always today's date or configurable?** | Product Owner | The reference has a fixed "NOW" marker. Could be `DateTime.Now` or a `currentDate` field in `data.json` for point-in-time screenshots |
| 6 | **Should the ADO Backlog link in the header be configurable per project?** | Stakeholder | Minor but affects the data model |
| 7 | **Print/export to PDF needed?** | Product Owner | If yes, consider adding a `window.print()` button with `@media print` CSS. Browser print-to-PDF at 1920×1080 works well |

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** Pixel-perfect reproduction of the HTML reference design, driven by `data.json`.

| Task | Estimate | Details |
|------|----------|---------|
| Scaffold Blazor Server project | 30 min | `dotnet new blazor -n ReportingDashboard --interactivity Server` |
| Define `DashboardData` model classes | 1 hr | Mirror the JSON schema above. Use `record` types for immutability |
| Create `DashboardDataService` | 30 min | Singleton, reads `data.json` from `wwwroot/`, deserializes with `System.Text.Json` |
| Build `Header.razor` component | 1 hr | Title, subtitle, legend icons. Direct port from HTML reference |
| Build `Timeline.razor` component | 3 hr | SVG generation with computed milestone positions. Most complex component |
| Build `Heatmap.razor` + `HeatmapCell.razor` | 2 hr | CSS Grid layout, color-coded rows, bullet-prefixed items |
| Create sample `data.json` | 30 min | Fictional project data for demo/testing |
| CSS polish and screenshot verification | 1 hr | Compare browser render against reference PNG at 1920×1080 |

**Total MVP: ~10 hours**

### Phase 2: Polish (1 day, optional)

| Task | Details |
|------|---------|
| Add `data.sample.json` with schema documentation | Help future users understand the format |
| Startup validation of `data.json` | Friendly error page if JSON is malformed |
| CSS custom properties for easy re-theming | Swap color palette per project type |
| `FileSystemWatcher` auto-reload | Dashboard refreshes when JSON file is saved |
| `?print=true` mode | Hides any dev-only UI, optimizes for screenshot |

### Phase 3: Enhancements (future, if needed)

| Task | Details |
|------|---------|
| Multi-project support | Dropdown selector, multiple JSON files |
| Simple JSON editor form | Blazor form to edit `data.json` without hand-editing |
| ADO integration | Pull work item data from Azure DevOps REST API into `data.json` format |
| Automated screenshot capture | Playwright script to render and save PNG on schedule |

### Quick Wins

1. **Start by copying the CSS from the HTML reference directly into `app.css`.** The design is already complete — don't redesign it.
2. **Use `dotnet watch` during development** for instant feedback on Razor/CSS changes.
3. **Create `data.json` first, then build components.** Having real data to render against prevents back-and-forth on the model design.
4. **Test screenshots early** — render the page, take a screenshot, overlay it against the reference PNG. Catch pixel drift before building all components.

### Libraries Summary (NuGet Packages)

| Package | Version | Required | Purpose |
|---------|---------|----------|---------|
| `Microsoft.AspNetCore.Components.Web` | 8.0.x | ✅ Built-in | Blazor Server framework |
| `System.Text.Json` | 8.0.x | ✅ Built-in | JSON deserialization |
| — | — | — | **That's it for MVP** |
| `xUnit` | 2.7+ | Optional | Unit testing |
| `bUnit` | 1.25+ | Optional | Blazor component testing |
| `FluentAssertions` | 6.12+ | Optional | Test readability |
| `JsonSchema.Net` | 7.x | Optional | JSON validation at startup |

**The MVP requires zero additional NuGet packages beyond what ships with the .NET 8 Blazor Server template.** This is the strongest endorsement of the stack choice — everything needed is built-in.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/521829bb83577dff7e64ec22b57478a21b0e4af3/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
