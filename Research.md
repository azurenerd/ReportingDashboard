# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-14 17:26 UTC_

### Summary

This project is a single-page executive reporting dashboard built with .NET 8 Blazor Server, designed for screenshot capture into PowerPoint decks. It reads project milestone and status data from a local `data.json` file and renders a timeline with milestone diamonds/checkpoints plus a color-coded heatmap grid showing Shipped, In Progress, Carryover, and Blockers by month. The architecture is intentionally minimal: no authentication, no database, no cloud dependencies. The entire application is a single Blazor Server project with a JSON file as the data source, pure CSS for styling (matching the reference design's Grid/Flexbox layout), and inline SVG for the timeline visualization. This can be built and running in under a day. **Primary Recommendation:** Build as a single Blazor Server project with zero external UI libraries. The reference HTML design is already pure HTML/CSS/SVG — translate it directly into Razor components with CSS isolation. Use `System.Text.Json` to deserialize `data.json` at startup. No charting library is needed; the SVG timeline is simple enough to generate directly in Razor markup. ---

### Key Findings

- The reference design (`OriginalDesignConcept.html`) uses only vanilla CSS Grid, Flexbox, and inline SVG — no JavaScript frameworks or charting libraries are needed.
- The dashboard is a read-only, single-page view with no interactivity beyond rendering — Blazor Server's SignalR circuit is overkill but acceptable for local use and keeps the stack unified.
- A `data.json` flat file is the ideal data source for this scope; introducing a database adds complexity with zero benefit for a screenshot-oriented tool.
- The design targets a fixed 1920×1080 viewport — responsive design is unnecessary and would complicate pixel-perfect screenshot capture.
- CSS isolation (`.razor.css` files) in Blazor perfectly maps to the component-per-section architecture the design demands.
- The SVG timeline with milestone markers (diamonds, circles, dashed "NOW" line) is straightforward to generate server-side in Razor without any SVG library.
- The color palette is well-defined in the reference HTML and should be extracted into CSS custom properties for maintainability.
- `System.Text.Json` (built into .NET 8) handles all JSON deserialization needs — no need for Newtonsoft.Json.
- The heatmap grid's `grid-template-columns: 160px repeat(4, 1fr)` pattern maps directly to a Blazor component with dynamic column count based on the months in `data.json`.
- Hot Reload in .NET 8 Blazor Server provides rapid iteration on layout tweaks, critical for matching the design pixel-for-pixel. ---
- `dotnet new blazor -n ReportingDashboard --interactivity Server` (or use `blazorserver` template)
- Create solution file: `dotnet new sln` → `dotnet sln add src/ReportingDashboard`
- Define C# models (`DashboardData`, `Milestone`, `HeatmapCategory`)
- Create `data.json` with fictional project data
- Build `DashboardDataService` (singleton, reads JSON at startup)
- Create `MainLayout.razor` (strip default nav, full-width)
- Translate reference HTML header section → `DashboardHeader.razor` + `.razor.css`
- **Verify:** Dashboard renders header with title, subtitle, and legend matching the reference
- Build `Timeline.razor` with SVG generation
- Implement `GetXPosition()` date-to-pixel mapping
- Render milestone tracks (horizontal lines), markers (diamonds, circles), month grid lines, and "NOW" dashed line
- Style milestone labels sidebar (left panel, 230px wide)
- **Verify:** Timeline matches reference layout — month grid lines align, diamonds render with drop shadows, "NOW" line is positioned correctly
- Build `Heatmap.razor` with CSS Grid layout
- Implement dynamic column headers from `data.json` months
- Build row headers (Shipped, In Progress, Carryover, Blockers) with category-specific colors
- Build `HeatmapCell.razor` for item lists with bullet dots
- Highlight current month column with darker background
- **Verify:** Heatmap matches reference — grid lines, colors, typography, bullet alignment
- Fine-tune spacing, font sizes, and colors against the reference design
- Set fixed 1920×1080 viewport in `app.css`
- Test screenshot capture in Chrome DevTools (Ctrl+Shift+P → "Capture full size screenshot")
- Add file-watching for `data.json` auto-reload during development
- Write 2–3 unit tests for JSON deserialization edge cases
- **Verify:** Screenshot is indistinguishable from the reference design at a glance
- **Immediate value:** A working dashboard with fictional data can be demoed to stakeholders within Day 1 to validate the layout before investing in real data integration.
- **Low effort, high impact:** CSS custom properties make theme changes (e.g., different color scheme per project) a 5-minute edit.
- **File watching:** Adding `FileSystemWatcher` on `data.json` enables a live edit workflow — change the JSON, see the dashboard update without restart.
- ❌ Authentication or authorization
- ❌ Database or ORM
- ❌ REST API endpoints
- ❌ Client-side interactivity (filters, tooltips, animations)
- ❌ Responsive design or mobile layout
- ❌ Automated screenshot pipeline (defer)
- ❌ Multi-user or concurrent access handling
- ❌ Logging infrastructure beyond console output
- ❌ Docker containerization
- ❌ CI/CD pipeline (manual `dotnet run` is the deployment) --- | Package | Version | Required? | Purpose | |---------|---------|-----------|---------| | `Microsoft.AspNetCore.Components` | 8.0.x | Included in SDK | Blazor framework | | `System.Text.Json` | 8.0.x | Included in SDK | JSON deserialization | | — | — | — | — | | `xunit` | 2.7.x | Optional | Unit testing | | `bunit` | 1.25.x | Optional | Component testing | | `FluentAssertions` | 6.12.x | Optional | Test assertions | **Total external dependencies for production: Zero.** Everything needed ships with the .NET 8 SDK. This is the simplest possible dependency footprint.

### Recommended Tools & Technologies

- **Date:** April 14, 2026 **Stack:** C# .NET 8 · Blazor Server · Local Only · .sln Structure --- | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **Framework** | Blazor Server (.NET 8) | `net8.0` | Server-side rendering with Razor components | | **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Matches reference design exactly | | **SVG Rendering** | Inline SVG in Razor | N/A | Timeline milestones, markers, "NOW" line | | **CSS Architecture** | Blazor CSS Isolation (`.razor.css`) | Built-in | Scoped styles per component | | **Design Tokens** | CSS Custom Properties | N/A | Color palette, spacing, typography from reference | | **Font** | Segoe UI (system font) | N/A | Already specified in reference design | **No external UI component library is recommended.** Libraries like MudBlazor, Radzen, or Syncfusion add massive dependency weight for a project that needs exactly one page of custom-styled HTML. The reference design is pure CSS — translating it into a component library's abstractions would be harder, not easier. **No charting library is recommended.** The timeline is a simple SVG with lines, circles, diamonds, and text — roughly 50 lines of SVG markup. Libraries like ApexCharts.Blazor or BlazorChart would impose their own styling and fight the reference design. | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Deserialize `data.json` | | **File Watching** | `IFileProvider` / `PhysicalFileProvider` | Built into .NET 8 | Optional: auto-reload on `data.json` changes | | **DI Registration** | `IServiceCollection` | Built into .NET 8 | Register data service as Singleton | | **Configuration** | `IConfiguration` (appsettings.json) | Built into .NET 8 | App settings (port, file path) | | Layer | Technology | Purpose | |-------|-----------|---------| | **Primary Store** | `data.json` flat file | All dashboard data | | **Schema Validation** | C# POCOs with `System.Text.Json` attributes | Type-safe deserialization | | **Location** | Project root or configurable path via `appsettings.json` | Simple file system access | **No database.** SQLite, LiteDB, or any other store would be over-engineering. The data is small (dozens of items), read-only at runtime, and edited by hand or by a separate process. A JSON file is human-readable, version-controllable, and trivially editable. | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **Unit Testing** | xUnit | 2.7+ | Test data model deserialization | | **Blazor Component Testing** | bUnit | 1.25+ | Test Razor component rendering | | **Assertions** | FluentAssertions | 6.12+ | Readable test assertions | Testing is optional for this scope but included for completeness. The highest-value test is verifying that `data.json` deserializes correctly into the model. | Tool | Version | Purpose | |------|---------|---------| | **.NET SDK** | 8.0.x (latest patch) | Build and run | | **Visual Studio 2022** or **VS Code + C# Dev Kit** | Latest | IDE | | **Hot Reload** | Built into `dotnet watch` | Rapid CSS/layout iteration | | **Browser DevTools** | Any Chromium | Inspect layout, screenshot capture | ---
```
ReportingDashboard.sln
│
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj        # net8.0, Blazor Server
│       ├── Program.cs                        # Minimal hosting, register services
│       ├── appsettings.json                  # Config (data file path, Kestrel port)
│       ├── data.json                         # Dashboard data (milestones, items)
│       │
│       ├── Models/
│       │   ├── DashboardData.cs              # Root model
│       │   ├── Milestone.cs                  # Timeline milestones
│       │   ├── HeatmapCategory.cs            # Shipped/InProgress/Carryover/Blockers
│       │   └── HeatmapItem.cs                # Individual items in cells
│       │
│       ├── Services/
│       │   └── DashboardDataService.cs       # Loads & caches data.json
│       │
│       ├── Components/
│       │   ├── Pages/
│       │   │   └── Dashboard.razor           # Single page (route: "/")
│       │   │   └── Dashboard.razor.css       # Page-level styles
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor          # Minimal shell (no nav)
│       │   │   └── MainLayout.razor.css
│       │   ├── DashboardHeader.razor         # Title, subtitle, legend
│       │   ├── DashboardHeader.razor.css
│       │   ├── Timeline.razor                # SVG timeline with milestones
│       │   ├── Timeline.razor.css
│       │   ├── Heatmap.razor                 # Grid container
│       │   ├── Heatmap.razor.css
│       │   ├── HeatmapCell.razor             # Individual cell with items
│       │   └── HeatmapCell.razor.css
│       │
│       └── wwwroot/
│           └── css/
│               └── app.css                   # Global resets, CSS custom properties
│
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── DataModelTests.cs
```
```
data.json  →  DashboardDataService (Singleton, loaded at startup)
                    ↓
           Dashboard.razor (injects service)
                    ↓
    ┌───────────────┼───────────────┐
    ↓               ↓               ↓
DashboardHeader  Timeline       Heatmap
  (title,        (SVG render     (CSS Grid,
   legend)        milestones)     4 rows × N months)
                                    ↓
                               HeatmapCell × N
                                (items list)
``` **Pattern: Top-down parameter passing.** The `Dashboard.razor` page loads data once and passes it down as `[Parameter]` properties to child components. No cascading values, no state management, no event callbacks needed — this is a pure render pipeline.
```csharp
// Dashboard.razor
@inject DashboardDataService DataService

<DashboardHeader Data="@_data" />
<Timeline Milestones="@_data.Milestones" CurrentDate="@_data.CurrentDate" />
<Heatmap Categories="@_data.Categories" Months="@_data.Months" />

@code {
    private DashboardData _data = default!;
    protected override void OnInitialized() => _data = DataService.GetData();
}
```
```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Engineering Division • Platform Team • April 2026",
  "backlogUrl": "https://dev.azure.com/org/project",
  "currentDate": "2026-04-14",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonthIndex": 3,
  "timelineStart": "2026-01-01",
  "timelineEnd": "2026-06-30",
  "milestones": [
    {
      "id": "M1",
      "label": "Core Platform",
      "description": "API Gateway & Auth Service",
      "color": "#0078D4",
      "markers": [
        { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
        { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "May Prod" }
      ]
    }
  ],
  "categories": [
    {
      "name": "Shipped",
      "key": "shipped",
      "items": {
        "Jan": ["Feature A", "Feature B"],
        "Feb": ["Feature C"],
        "Mar": ["Feature D", "Feature E"],
        "Apr": []
      }
    },
    {
      "name": "In Progress",
      "key": "inProgress",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Feature F", "Feature G"] }
    },
    {
      "name": "Carryover",
      "key": "carryover",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Feature H"] }
    },
    {
      "name": "Blockers",
      "key": "blockers",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Dependency X"] }
    }
  ]
}
``` Define the design system in `app.css`:
```css
:root {
  /* From reference design */
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
  --color-blockers: #EA4335;
  --color-blockers-bg: #FFF5F5;
  --color-blockers-bg-current: #FFE4E4;
  --color-blockers-header: #FEF2F2;
  --color-border: #E0E0E0;
  --color-border-heavy: #CCC;
  --color-text: #111;
  --color-text-muted: #888;
  --color-link: #0078D4;
  --font-family: 'Segoe UI', Arial, sans-serif;
}
``` Each component gets its own `.razor.css` file with scoped styles that mirror the corresponding section from the reference HTML. This provides perfect encapsulation without CSS class name collisions. The timeline is rendered server-side in `Timeline.razor` using Razor markup to generate SVG elements. Key calculations:
```csharp
@code {
    private double GetXPosition(DateTime date)
    {
        var totalDays = (TimelineEnd - TimelineStart).TotalDays;
        var elapsed = (date - TimelineStart).TotalDays;
        return (elapsed / totalDays) * SvgWidth;
    }
}
``` Marker types map to SVG primitives:
- **Checkpoint** → `<circle>` (small, gray fill)
- **PoC Milestone** → `<polygon>` (diamond, gold `#F4B400`)
- **Production Release** → `<polygon>` (diamond, green `#34A853`)
- **NOW line** → `<line>` (dashed, red `#EA4335`)
- **Milestone track** → `<line>` (horizontal, colored per milestone) The reference design targets exactly 1920×1080. For screenshot fidelity:
```css
body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
}
``` This ensures the dashboard renders identically across machines and produces clean screenshots. Do NOT add responsive breakpoints — they would compromise the pixel-perfect layout needed for PowerPoint slides. ---

### Considerations & Risks

- **None.** This is explicitly out of scope. The dashboard runs locally and serves a single user taking screenshots. No login, no roles, no middleware. If this ever needs to be shared on a network, the simplest addition would be a shared secret via query parameter (`?key=abc123`) checked in middleware — but defer this unless the need arises.
- `data.json` contains project status information, not PII or secrets.
- No encryption needed for data at rest or in transit (localhost only).
- If the JSON contains sensitive project names, ensure the file is `.gitignore`'d and not committed to public repos. | Aspect | Recommendation | |--------|---------------| | **Runtime** | Kestrel (built into .NET 8, no IIS needed) | | **Launch** | `dotnet run` from project directory | | **Port** | `https://localhost:5001` or configured in `launchSettings.json` | | **Process** | Run on developer's local machine only | | **Deployment** | None — `dotnet run` or `dotnet watch` is the deployment | **No containers, no reverse proxy, no cloud hosting.** This is a developer tool run locally.
- **Startup time:** Blazor Server on .NET 8 starts in <2 seconds locally.
- **Memory:** Minimal — a single JSON file in memory, no SignalR connections beyond the one browser tab.
- **Logging:** Default .NET logging to console is sufficient. No structured logging or log aggregation needed. --- **Severity:** Low **Description:** For a static read-only page, Blazor Server's SignalR circuit adds unnecessary complexity. A static HTML generator or Blazor Static SSR (new in .NET 8) would be lighter. **Mitigation:** Accept the overhead — it's negligible for local use, and using Blazor Server keeps the door open for future interactivity (filtering, date range selection) without a rewrite. Alternatively, use .NET 8's new Static Server-Side Rendering (Static SSR) mode via `[StreamRendering]` attribute to render once and disconnect SignalR. **Recommendation:** Use `@rendermode InteractiveServer` only if you want live updates when `data.json` changes. Otherwise, Static SSR is perfect for a screenshot tool. **Severity:** Medium **Description:** The timeline SVG positions milestones based on date-to-pixel calculations. If the SVG viewport width doesn't match the container, markers will be misaligned. **Mitigation:** Hard-code the SVG `width` attribute to match the container width (derived from `1920px - padding`). Test with browser DevTools overlay. The reference design uses `width="1560"` for the SVG inside a padded container — replicate this exactly. **Severity:** Low **Description:** As the dashboard evolves, the `data.json` schema will change. Without validation, breaking changes will produce runtime errors. **Mitigation:** Use strongly-typed C# models with `[JsonPropertyName]` attributes. Add a startup validation step that logs clear errors if required fields are missing. Consider adding a `schemaVersion` field to `data.json` for future-proofing. **Severity:** Low **Description:** Blazor CSS isolation works by appending a unique attribute (e.g., `b-abc123`) to elements. Deep child elements in `RenderFragment` content may not receive the attribute, causing style leaks. **Mitigation:** Use `::deep` combinator in `.razor.css` files when styling child component elements. Keep component hierarchies shallow (the design only needs 2 levels). **Description:** By default, `data.json` is read once at startup. Changing the file requires restarting the app. **Alternative:** Register a `FileSystemWatcher` or use `IOptionsMonitor<T>` with a custom JSON configuration source to detect file changes and reload data automatically. This is a nice-to-have for iteration speed. **Recommendation:** Implement file watching — it's ~15 lines of code and dramatically improves the edit-refresh workflow. **Description:** The dashboard is designed for browser screenshots, not CSS print media. **Alternative:** Add a `@media print` stylesheet that forces the 1920×1080 layout. Or use Playwright/Puppeteer for automated screenshot generation. **Recommendation:** Defer automated screenshots. Browser DevTools "Capture full size screenshot" at 1920×1080 is sufficient. ---
- **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or fixed at the current month and 3 prior months? **Recommendation:** Make it data-driven — render as many months as exist in `data.json`.
- **How many milestone tracks in the timeline?** The reference shows 3 (M1, M2, M3). Should there be a maximum? **Recommendation:** Support 1–5 tracks; more than 5 will be visually cramped in the 196px timeline area.
- **Should the "NOW" line be auto-calculated or manual?** The reference hard-codes it. **Recommendation:** Auto-calculate from `DateTime.Now` relative to the timeline range, but allow override via `currentDate` in `data.json` for generating historical snapshots.
- **Will multiple projects/dashboards be needed?** If yes, the app could accept a query parameter (`?project=phoenix`) to load different JSON files. **Recommendation:** Design for this from day one — it's trivial to implement and avoids a rewrite later. Convention: `data.{project}.json` or a `projects/` subfolder.
- **What is the update cadence for `data.json`?** Weekly? Monthly? This affects whether manual editing is acceptable or if an automated pipeline (e.g., ADO work item query export) should populate it. **Recommendation:** Start manual; automate later if the cadence is weekly or faster.
- **Should the ADO Backlog link in the header be functional?** The reference shows it as a hyperlink. **Recommendation:** Yes — include it in `data.json` as `backlogUrl` and render as a clickable link. It won't work in a PowerPoint screenshot, but it's useful when viewing the live dashboard. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Date:** April 14, 2026
**Stack:** C# .NET 8 · Blazor Server · Local Only · .sln Structure

---

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with .NET 8 Blazor Server, designed for screenshot capture into PowerPoint decks. It reads project milestone and status data from a local `data.json` file and renders a timeline with milestone diamonds/checkpoints plus a color-coded heatmap grid showing Shipped, In Progress, Carryover, and Blockers by month. The architecture is intentionally minimal: no authentication, no database, no cloud dependencies. The entire application is a single Blazor Server project with a JSON file as the data source, pure CSS for styling (matching the reference design's Grid/Flexbox layout), and inline SVG for the timeline visualization. This can be built and running in under a day.

**Primary Recommendation:** Build as a single Blazor Server project with zero external UI libraries. The reference HTML design is already pure HTML/CSS/SVG — translate it directly into Razor components with CSS isolation. Use `System.Text.Json` to deserialize `data.json` at startup. No charting library is needed; the SVG timeline is simple enough to generate directly in Razor markup.

---

## 2. Key Findings

- The reference design (`OriginalDesignConcept.html`) uses only vanilla CSS Grid, Flexbox, and inline SVG — no JavaScript frameworks or charting libraries are needed.
- The dashboard is a read-only, single-page view with no interactivity beyond rendering — Blazor Server's SignalR circuit is overkill but acceptable for local use and keeps the stack unified.
- A `data.json` flat file is the ideal data source for this scope; introducing a database adds complexity with zero benefit for a screenshot-oriented tool.
- The design targets a fixed 1920×1080 viewport — responsive design is unnecessary and would complicate pixel-perfect screenshot capture.
- CSS isolation (`.razor.css` files) in Blazor perfectly maps to the component-per-section architecture the design demands.
- The SVG timeline with milestone markers (diamonds, circles, dashed "NOW" line) is straightforward to generate server-side in Razor without any SVG library.
- The color palette is well-defined in the reference HTML and should be extracted into CSS custom properties for maintainability.
- `System.Text.Json` (built into .NET 8) handles all JSON deserialization needs — no need for Newtonsoft.Json.
- The heatmap grid's `grid-template-columns: 160px repeat(4, 1fr)` pattern maps directly to a Blazor component with dynamic column count based on the months in `data.json`.
- Hot Reload in .NET 8 Blazor Server provides rapid iteration on layout tweaks, critical for matching the design pixel-for-pixel.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server UI)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Framework** | Blazor Server (.NET 8) | `net8.0` | Server-side rendering with Razor components |
| **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Matches reference design exactly |
| **SVG Rendering** | Inline SVG in Razor | N/A | Timeline milestones, markers, "NOW" line |
| **CSS Architecture** | Blazor CSS Isolation (`.razor.css`) | Built-in | Scoped styles per component |
| **Design Tokens** | CSS Custom Properties | N/A | Color palette, spacing, typography from reference |
| **Font** | Segoe UI (system font) | N/A | Already specified in reference design |

**No external UI component library is recommended.** Libraries like MudBlazor, Radzen, or Syncfusion add massive dependency weight for a project that needs exactly one page of custom-styled HTML. The reference design is pure CSS — translating it into a component library's abstractions would be harder, not easier.

**No charting library is recommended.** The timeline is a simple SVG with lines, circles, diamonds, and text — roughly 50 lines of SVG markup. Libraries like ApexCharts.Blazor or BlazorChart would impose their own styling and fight the reference design.

### Backend (Data Layer)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Deserialize `data.json` |
| **File Watching** | `IFileProvider` / `PhysicalFileProvider` | Built into .NET 8 | Optional: auto-reload on `data.json` changes |
| **DI Registration** | `IServiceCollection` | Built into .NET 8 | Register data service as Singleton |
| **Configuration** | `IConfiguration` (appsettings.json) | Built into .NET 8 | App settings (port, file path) |

### Data Storage

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Primary Store** | `data.json` flat file | All dashboard data |
| **Schema Validation** | C# POCOs with `System.Text.Json` attributes | Type-safe deserialization |
| **Location** | Project root or configurable path via `appsettings.json` | Simple file system access |

**No database.** SQLite, LiteDB, or any other store would be over-engineering. The data is small (dozens of items), read-only at runtime, and edited by hand or by a separate process. A JSON file is human-readable, version-controllable, and trivially editable.

### Testing

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Unit Testing** | xUnit | 2.7+ | Test data model deserialization |
| **Blazor Component Testing** | bUnit | 1.25+ | Test Razor component rendering |
| **Assertions** | FluentAssertions | 6.12+ | Readable test assertions |

Testing is optional for this scope but included for completeness. The highest-value test is verifying that `data.json` deserializes correctly into the model.

### Development Tooling

| Tool | Version | Purpose |
|------|---------|---------|
| **.NET SDK** | 8.0.x (latest patch) | Build and run |
| **Visual Studio 2022** or **VS Code + C# Dev Kit** | Latest | IDE |
| **Hot Reload** | Built into `dotnet watch` | Rapid CSS/layout iteration |
| **Browser DevTools** | Any Chromium | Inspect layout, screenshot capture |

---

## 4. Architecture Recommendations

### Solution Structure

```
ReportingDashboard.sln
│
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj        # net8.0, Blazor Server
│       ├── Program.cs                        # Minimal hosting, register services
│       ├── appsettings.json                  # Config (data file path, Kestrel port)
│       ├── data.json                         # Dashboard data (milestones, items)
│       │
│       ├── Models/
│       │   ├── DashboardData.cs              # Root model
│       │   ├── Milestone.cs                  # Timeline milestones
│       │   ├── HeatmapCategory.cs            # Shipped/InProgress/Carryover/Blockers
│       │   └── HeatmapItem.cs                # Individual items in cells
│       │
│       ├── Services/
│       │   └── DashboardDataService.cs       # Loads & caches data.json
│       │
│       ├── Components/
│       │   ├── Pages/
│       │   │   └── Dashboard.razor           # Single page (route: "/")
│       │   │   └── Dashboard.razor.css       # Page-level styles
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor          # Minimal shell (no nav)
│       │   │   └── MainLayout.razor.css
│       │   ├── DashboardHeader.razor         # Title, subtitle, legend
│       │   ├── DashboardHeader.razor.css
│       │   ├── Timeline.razor                # SVG timeline with milestones
│       │   ├── Timeline.razor.css
│       │   ├── Heatmap.razor                 # Grid container
│       │   ├── Heatmap.razor.css
│       │   ├── HeatmapCell.razor             # Individual cell with items
│       │   └── HeatmapCell.razor.css
│       │
│       └── wwwroot/
│           └── css/
│               └── app.css                   # Global resets, CSS custom properties
│
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── DataModelTests.cs
```

### Data Flow

```
data.json  →  DashboardDataService (Singleton, loaded at startup)
                    ↓
           Dashboard.razor (injects service)
                    ↓
    ┌───────────────┼───────────────┐
    ↓               ↓               ↓
DashboardHeader  Timeline       Heatmap
  (title,        (SVG render     (CSS Grid,
   legend)        milestones)     4 rows × N months)
                                    ↓
                               HeatmapCell × N
                                (items list)
```

### Component Architecture

**Pattern: Top-down parameter passing.** The `Dashboard.razor` page loads data once and passes it down as `[Parameter]` properties to child components. No cascading values, no state management, no event callbacks needed — this is a pure render pipeline.

```csharp
// Dashboard.razor
@inject DashboardDataService DataService

<DashboardHeader Data="@_data" />
<Timeline Milestones="@_data.Milestones" CurrentDate="@_data.CurrentDate" />
<Heatmap Categories="@_data.Categories" Months="@_data.Months" />

@code {
    private DashboardData _data = default!;
    protected override void OnInitialized() => _data = DataService.GetData();
}
```

### Data Model Design (`data.json`)

```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Engineering Division • Platform Team • April 2026",
  "backlogUrl": "https://dev.azure.com/org/project",
  "currentDate": "2026-04-14",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonthIndex": 3,
  "timelineStart": "2026-01-01",
  "timelineEnd": "2026-06-30",
  "milestones": [
    {
      "id": "M1",
      "label": "Core Platform",
      "description": "API Gateway & Auth Service",
      "color": "#0078D4",
      "markers": [
        { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
        { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "May Prod" }
      ]
    }
  ],
  "categories": [
    {
      "name": "Shipped",
      "key": "shipped",
      "items": {
        "Jan": ["Feature A", "Feature B"],
        "Feb": ["Feature C"],
        "Mar": ["Feature D", "Feature E"],
        "Apr": []
      }
    },
    {
      "name": "In Progress",
      "key": "inProgress",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Feature F", "Feature G"] }
    },
    {
      "name": "Carryover",
      "key": "carryover",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Feature H"] }
    },
    {
      "name": "Blockers",
      "key": "blockers",
      "items": { "Jan": [], "Feb": [], "Mar": [], "Apr": ["Dependency X"] }
    }
  ]
}
```

### CSS Architecture

**Approach: CSS Custom Properties + CSS Isolation**

Define the design system in `app.css`:

```css
:root {
  /* From reference design */
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
  --color-blockers: #EA4335;
  --color-blockers-bg: #FFF5F5;
  --color-blockers-bg-current: #FFE4E4;
  --color-blockers-header: #FEF2F2;
  --color-border: #E0E0E0;
  --color-border-heavy: #CCC;
  --color-text: #111;
  --color-text-muted: #888;
  --color-link: #0078D4;
  --font-family: 'Segoe UI', Arial, sans-serif;
}
```

Each component gets its own `.razor.css` file with scoped styles that mirror the corresponding section from the reference HTML. This provides perfect encapsulation without CSS class name collisions.

### SVG Timeline Generation

The timeline is rendered server-side in `Timeline.razor` using Razor markup to generate SVG elements. Key calculations:

```csharp
@code {
    private double GetXPosition(DateTime date)
    {
        var totalDays = (TimelineEnd - TimelineStart).TotalDays;
        var elapsed = (date - TimelineStart).TotalDays;
        return (elapsed / totalDays) * SvgWidth;
    }
}
```

Marker types map to SVG primitives:
- **Checkpoint** → `<circle>` (small, gray fill)
- **PoC Milestone** → `<polygon>` (diamond, gold `#F4B400`)
- **Production Release** → `<polygon>` (diamond, green `#34A853`)
- **NOW line** → `<line>` (dashed, red `#EA4335`)
- **Milestone track** → `<line>` (horizontal, colored per milestone)

### Fixed Viewport Strategy

The reference design targets exactly 1920×1080. For screenshot fidelity:

```css
body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
}
```

This ensures the dashboard renders identically across machines and produces clean screenshots. Do NOT add responsive breakpoints — they would compromise the pixel-perfect layout needed for PowerPoint slides.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope. The dashboard runs locally and serves a single user taking screenshots. No login, no roles, no middleware.

If this ever needs to be shared on a network, the simplest addition would be a shared secret via query parameter (`?key=abc123`) checked in middleware — but defer this unless the need arises.

### Data Protection

- `data.json` contains project status information, not PII or secrets.
- No encryption needed for data at rest or in transit (localhost only).
- If the JSON contains sensitive project names, ensure the file is `.gitignore`'d and not committed to public repos.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Kestrel (built into .NET 8, no IIS needed) |
| **Launch** | `dotnet run` from project directory |
| **Port** | `https://localhost:5001` or configured in `launchSettings.json` |
| **Process** | Run on developer's local machine only |
| **Deployment** | None — `dotnet run` or `dotnet watch` is the deployment |

**No containers, no reverse proxy, no cloud hosting.** This is a developer tool run locally.

### Operational Concerns

- **Startup time:** Blazor Server on .NET 8 starts in <2 seconds locally.
- **Memory:** Minimal — a single JSON file in memory, no SignalR connections beyond the one browser tab.
- **Logging:** Default .NET logging to console is sufficient. No structured logging or log aggregation needed.

---

## 6. Risks & Trade-offs

### Risk: Blazor Server is Overkill

**Severity:** Low
**Description:** For a static read-only page, Blazor Server's SignalR circuit adds unnecessary complexity. A static HTML generator or Blazor Static SSR (new in .NET 8) would be lighter.
**Mitigation:** Accept the overhead — it's negligible for local use, and using Blazor Server keeps the door open for future interactivity (filtering, date range selection) without a rewrite. Alternatively, use .NET 8's new Static Server-Side Rendering (Static SSR) mode via `[StreamRendering]` attribute to render once and disconnect SignalR.
**Recommendation:** Use `@rendermode InteractiveServer` only if you want live updates when `data.json` changes. Otherwise, Static SSR is perfect for a screenshot tool.

### Risk: SVG Positioning Drift

**Severity:** Medium
**Description:** The timeline SVG positions milestones based on date-to-pixel calculations. If the SVG viewport width doesn't match the container, markers will be misaligned.
**Mitigation:** Hard-code the SVG `width` attribute to match the container width (derived from `1920px - padding`). Test with browser DevTools overlay. The reference design uses `width="1560"` for the SVG inside a padded container — replicate this exactly.

### Risk: JSON Schema Evolution

**Severity:** Low
**Description:** As the dashboard evolves, the `data.json` schema will change. Without validation, breaking changes will produce runtime errors.
**Mitigation:** Use strongly-typed C# models with `[JsonPropertyName]` attributes. Add a startup validation step that logs clear errors if required fields are missing. Consider adding a `schemaVersion` field to `data.json` for future-proofing.

### Risk: CSS Isolation Specificity

**Severity:** Low
**Description:** Blazor CSS isolation works by appending a unique attribute (e.g., `b-abc123`) to elements. Deep child elements in `RenderFragment` content may not receive the attribute, causing style leaks.
**Mitigation:** Use `::deep` combinator in `.razor.css` files when styling child component elements. Keep component hierarchies shallow (the design only needs 2 levels).

### Trade-off: No Hot Data Reload

**Description:** By default, `data.json` is read once at startup. Changing the file requires restarting the app.
**Alternative:** Register a `FileSystemWatcher` or use `IOptionsMonitor<T>` with a custom JSON configuration source to detect file changes and reload data automatically. This is a nice-to-have for iteration speed.
**Recommendation:** Implement file watching — it's ~15 lines of code and dramatically improves the edit-refresh workflow.

### Trade-off: No Export/Print CSS

**Description:** The dashboard is designed for browser screenshots, not CSS print media.
**Alternative:** Add a `@media print` stylesheet that forces the 1920×1080 layout. Or use Playwright/Puppeteer for automated screenshot generation.
**Recommendation:** Defer automated screenshots. Browser DevTools "Capture full size screenshot" at 1920×1080 is sufficient.

---

## 7. Open Questions

1. **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or fixed at the current month and 3 prior months? **Recommendation:** Make it data-driven — render as many months as exist in `data.json`.

2. **How many milestone tracks in the timeline?** The reference shows 3 (M1, M2, M3). Should there be a maximum? **Recommendation:** Support 1–5 tracks; more than 5 will be visually cramped in the 196px timeline area.

3. **Should the "NOW" line be auto-calculated or manual?** The reference hard-codes it. **Recommendation:** Auto-calculate from `DateTime.Now` relative to the timeline range, but allow override via `currentDate` in `data.json` for generating historical snapshots.

4. **Will multiple projects/dashboards be needed?** If yes, the app could accept a query parameter (`?project=phoenix`) to load different JSON files. **Recommendation:** Design for this from day one — it's trivial to implement and avoids a rewrite later. Convention: `data.{project}.json` or a `projects/` subfolder.

5. **What is the update cadence for `data.json`?** Weekly? Monthly? This affects whether manual editing is acceptable or if an automated pipeline (e.g., ADO work item query export) should populate it. **Recommendation:** Start manual; automate later if the cadence is weekly or faster.

6. **Should the ADO Backlog link in the header be functional?** The reference shows it as a hyperlink. **Recommendation:** Yes — include it in `data.json` as `backlogUrl` and render as a clickable link. It won't work in a PowerPoint screenshot, but it's useful when viewing the live dashboard.

---

## 8. Implementation Recommendations

### Phase 1: Scaffold & Static Render (Day 1, ~4 hours)

1. `dotnet new blazor -n ReportingDashboard --interactivity Server` (or use `blazorserver` template)
2. Create solution file: `dotnet new sln` → `dotnet sln add src/ReportingDashboard`
3. Define C# models (`DashboardData`, `Milestone`, `HeatmapCategory`)
4. Create `data.json` with fictional project data
5. Build `DashboardDataService` (singleton, reads JSON at startup)
6. Create `MainLayout.razor` (strip default nav, full-width)
7. Translate reference HTML header section → `DashboardHeader.razor` + `.razor.css`
8. **Verify:** Dashboard renders header with title, subtitle, and legend matching the reference

### Phase 2: Timeline SVG (Day 1, ~3 hours)

1. Build `Timeline.razor` with SVG generation
2. Implement `GetXPosition()` date-to-pixel mapping
3. Render milestone tracks (horizontal lines), markers (diamonds, circles), month grid lines, and "NOW" dashed line
4. Style milestone labels sidebar (left panel, 230px wide)
5. **Verify:** Timeline matches reference layout — month grid lines align, diamonds render with drop shadows, "NOW" line is positioned correctly

### Phase 3: Heatmap Grid (Day 1–2, ~3 hours)

1. Build `Heatmap.razor` with CSS Grid layout
2. Implement dynamic column headers from `data.json` months
3. Build row headers (Shipped, In Progress, Carryover, Blockers) with category-specific colors
4. Build `HeatmapCell.razor` for item lists with bullet dots
5. Highlight current month column with darker background
6. **Verify:** Heatmap matches reference — grid lines, colors, typography, bullet alignment

### Phase 4: Polish & Screenshot Optimization (Day 2, ~2 hours)

1. Fine-tune spacing, font sizes, and colors against the reference design
2. Set fixed 1920×1080 viewport in `app.css`
3. Test screenshot capture in Chrome DevTools (Ctrl+Shift+P → "Capture full size screenshot")
4. Add file-watching for `data.json` auto-reload during development
5. Write 2–3 unit tests for JSON deserialization edge cases
6. **Verify:** Screenshot is indistinguishable from the reference design at a glance

### Quick Wins

- **Immediate value:** A working dashboard with fictional data can be demoed to stakeholders within Day 1 to validate the layout before investing in real data integration.
- **Low effort, high impact:** CSS custom properties make theme changes (e.g., different color scheme per project) a 5-minute edit.
- **File watching:** Adding `FileSystemWatcher` on `data.json` enables a live edit workflow — change the JSON, see the dashboard update without restart.

### What NOT to Build

- ❌ Authentication or authorization
- ❌ Database or ORM
- ❌ REST API endpoints
- ❌ Client-side interactivity (filters, tooltips, animations)
- ❌ Responsive design or mobile layout
- ❌ Automated screenshot pipeline (defer)
- ❌ Multi-user or concurrent access handling
- ❌ Logging infrastructure beyond console output
- ❌ Docker containerization
- ❌ CI/CD pipeline (manual `dotnet run` is the deployment)

---

### Appendix: Key NuGet Packages

| Package | Version | Required? | Purpose |
|---------|---------|-----------|---------|
| `Microsoft.AspNetCore.Components` | 8.0.x | Included in SDK | Blazor framework |
| `System.Text.Json` | 8.0.x | Included in SDK | JSON deserialization |
| — | — | — | — |
| `xunit` | 2.7.x | Optional | Unit testing |
| `bunit` | 1.25.x | Optional | Component testing |
| `FluentAssertions` | 6.12.x | Optional | Test assertions |

**Total external dependencies for production: Zero.** Everything needed ships with the .NET 8 SDK. This is the simplest possible dependency footprint.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/a732eeb1d2cc820b0fd7cfac3a9281161339a7c2/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
