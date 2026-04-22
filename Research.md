# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-22 09:28 UTC_

### Summary

This project is a single-page executive reporting dashboard that visualizes project milestones, progress status, and monthly heatmap data. The application will be built with **Blazor Server on .NET 8**, load data from a **local JSON configuration file**, and render a pixel-perfect dashboard designed for screenshot capture at 1920×1080 for PowerPoint decks. **Primary recommendation:** Keep the architecture radically simple — a single Blazor Server project with no database, no authentication, no API layer. Data lives in a `dashboard-data.json` file. The UI is built with pure CSS Grid/Flexbox and inline SVG (no charting library needed). The entire solution should be < 15 files and buildable in a single sprint. --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Blazor Server runtime | Yes (implicit) | | `System.Text.Json` | 8.0.x | JSON deserialization | Yes (built-in) | | `xunit` | 2.7.x | Unit testing | For tests project | | `bunit` | 1.25.x | Blazor component testing | For tests project | | `FluentAssertions` | 6.12.x | Readable test assertions | For tests project | **Total third-party runtime dependencies: 0.** Everything needed is built into .NET 8. --- *This research document provides the engineering team with everything needed to begin implementation immediately. The architecture is intentionally minimal — the simplicity of a single JSON file driving a single Blazor page is the correct design for a screenshot-oriented executive dashboard.*

### Key Findings

- The original HTML design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`), **Flexbox**, and **inline SVG** — all of which Blazor Server renders natively without any JavaScript interop or third-party libraries.
- **No charting library is needed.** The timeline/Gantt section is a simple SVG with lines, circles, diamonds, and text — easily generated from data in a Blazor component. Adding a charting library would be over-engineering.
- **JSON is the optimal data format** for the configuration file: human-editable, natively supported via `System.Text.Json` in .NET 8, no extra dependencies, and structured enough to represent milestones, status categories, and heatmap items.
- The dashboard is **read-only and stateless** — no forms, no user input, no persistence. This eliminates the need for state management patterns, database layers, or API controllers.
- Blazor Server's **SignalR connection** is irrelevant for this use case (no interactivity needed), but the framework still provides the best developer experience for a .NET team rendering server-side HTML with C# models.
- The design targets a **fixed 1920×1080 viewport** for screenshot capture, which simplifies CSS — no responsive breakpoints needed, though a `@media` fallback for development on smaller screens is recommended.
- **Hot reload** in .NET 8 Blazor Server works well for iterating on CSS and component layout, which will be the primary development activity.
- The `Segoe UI` font specified in the design is pre-installed on Windows (the target environment), so no web font loading is required.
- **File watching** for `dashboard-data.json` changes can be implemented with `FileSystemWatcher` to enable live-reload of data without restarting the app. --- **Goal:** Render the dashboard from a JSON file, matching the original design.
- **Scaffold the project** — `dotnet new blazorserver -n ReportingDashboard --framework net8.0`
- **Define C# models** — `DashboardConfig`, `ProjectInfo`, `Milestone`, `StatusRow`, etc.
- **Create sample `dashboard-data.json`** with fictional project data matching the original design's structure
- **Build `DashboardDataService`** — singleton that reads and deserializes JSON on startup
- **Build `Dashboard.razor` page** — single page, inject data service
- **Build `Header.razor`** — title, subtitle, legend icons (translate directly from HTML)
- **Build `Heatmap.razor` + `HeatmapCell.razor`** — CSS Grid layout, color-coded rows
- **Build `Timeline.razor`** — SVG generation from milestone data
- **Global CSS** — body sizing, font, resets in `app.css`
- **FileSystemWatcher** — auto-reload data when JSON changes (no app restart)
- **Error handling** — friendly message if JSON is malformed or missing
- **JSON Schema file** — enable autocomplete in VS Code for data editing
- **Print/screenshot CSS** — `@media print` styles for clean output
- **Browser zoom hint** — small dev-mode banner suggesting 100% zoom for accurate screenshots
- **Query parameter routing** — `?project=my-project` loads `my-project.json`
- **Blazor Static SSR** — remove SignalR for pure HTML rendering
- **Dark mode** — alternate CSS variables for dark-background PowerPoint slides
- **PDF export** — server-side HTML-to-PDF via `Playwright` or `PuppeteerSharp` (but screenshotting is simpler) | Quick Win | Effort | Impact | |-----------|--------|--------| | Copy-paste original CSS into Blazor scoped CSS files | 30 min | Instant visual parity | | `dotnet watch` for hot reload during CSS tweaking | 0 min (built-in) | Fast iteration | | JSON sample data matching original design | 30 min | Validates data model immediately | | CSS custom properties for color themes | 15 min | Easy to adjust palette later | **File name:** `dashboard-data.json` **Location:** `wwwroot/data/dashboard-data.json` (or configurable path)
```json
{
  "$schema": "dashboard-schema.json",
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "currentDate": "2026-04-10"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "name": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
          { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
          { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
        ]
      },
      {
        "id": "M2",
        "name": "PDS & Data Inventory",
        "color": "#00897B",
        "milestones": [
          { "date": "2025-12-19", "label": "Dec 19", "type": "checkpoint" },
          { "date": "2026-02-11", "label": "Feb 11", "type": "checkpoint" },
          { "date": "2026-03-15", "label": "Mar 15 PoC", "type": "poc" },
          { "date": "2026-04-18", "label": "Apr Prod", "type": "production" }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "highlightMonth": "Apr",
    "rows": [
      {
        "category": "Shipped",
        "items": {
          "Jan": ["CELA Chatbot v1", "Role assignment API"],
          "Feb": ["PDS connector", "Data classification rules"],
          "Mar": ["Auto-review pipeline", "DFD generator v1"],
          "Apr": ["Compliance dashboard", "Audit trail export"]
        }
      },
      {
        "category": "In Progress",
        "items": {
          "Jan": ["MS Role integration"],
          "Feb": ["Inventory scanner"],
          "Mar": ["Review automation"],
          "Apr": ["E2E testing", "Perf optimization", "Docs update"]
        }
      },
      {
        "category": "Carryover",
        "items": {
          "Jan": [],
          "Feb": ["Auth token refresh fix"],
          "Mar": ["Legacy data migration"],
          "Apr": ["Cross-team API contract"]
        }
      },
      {
        "category": "Blockers",
        "items": {
          "Jan": [],
          "Feb": [],
          "Mar": ["Upstream service outage"],
          "Apr": ["Dependency on Platform v3 release"]
        }
      }
    ]
  }
}
```

### Recommended Tools & Technologies

- **Project:** Executive Reporting Dashboard **Date:** April 22, 2026 **Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure --- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (.NET 8) | 8.0.x | Built-in, no additional package | | **CSS Layout** | CSS Grid + Flexbox | Native | Matches original design exactly | | **SVG Rendering** | Inline SVG via Razor | Native | Timeline milestones, diamonds, circles | | **CSS Isolation** | Blazor scoped CSS (`.razor.css`) | Built-in | Per-component styles, no conflicts | | **Icons/Shapes** | Pure CSS + inline SVG | N/A | Diamond shapes via `transform: rotate(45deg)`, circles via `border-radius` | **No third-party UI/CSS frameworks recommended.** Bootstrap, MudBlazor, Radzen, etc. would add unnecessary weight and fight against the custom design. The original HTML/CSS is self-contained and should be translated directly into Blazor components with scoped CSS. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built-in (.NET 8) | Native, fast, no extra dependency | | **File Watching** | `System.IO.FileSystemWatcher` | Built-in (.NET 8) | Auto-reload on data file changes | | **Configuration** | `dashboard-data.json` (custom file) | N/A | Not `appsettings.json` — separate concern | | **DI Registration** | `IServiceCollection` | Built-in | Singleton service for data provider | | Layer | Technology | Notes | |-------|-----------|-------| | **Primary Store** | `dashboard-data.json` flat file | Human-editable, version-controllable | | **Format** | JSON with `$schema` reference | Enables IDE autocomplete when editing | | **Location** | Project root or `wwwroot/data/` | Configurable via `appsettings.json` path | **No database.** SQLite, LiteDB, and other embedded databases are unnecessary for a read-only dashboard with a single data file. A flat JSON file is the simplest, most portable, and most editable option. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Tests** | xUnit | 2.7.x | .NET ecosystem standard | | **Assertions** | FluentAssertions | 6.12.x | Readable test assertions | | **Blazor Component Tests** | bUnit | 1.25.x+ | Renders Blazor components in-memory | | **JSON Validation** | Manual test fixtures | N/A | Validate deserialization of sample data | | Tool | Version | Notes | |------|---------|-------| | **.NET SDK** | 8.0.x | `dotnet new blazorserver` template | | **IDE** | Visual Studio 2022 or VS Code + C# Dev Kit | Both fully support .NET 8 Blazor | | **Hot Reload** | Built-in `dotnet watch` | CSS + Razor changes without restart | ---
```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Models/
│       │   └── DashboardData.cs          # POCO models for JSON deserialization
│       ├── Services/
│       │   └── DashboardDataService.cs    # Loads & caches JSON, watches for changes
│       ├── Components/
│       │   ├── Pages/
│       │   │   └── Dashboard.razor        # Single page — the entire app
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor       # Minimal layout wrapper
│       │   │   └── MainLayout.razor.css
│       │   ├── Header.razor               # Title, subtitle, legend
│       │   ├── Header.razor.css
│       │   ├── Timeline.razor             # SVG milestone Gantt chart
│       │   ├── Timeline.razor.css
│       │   ├── Heatmap.razor              # Status grid (Shipped/InProgress/Carryover/Blockers)
│       │   ├── Heatmap.razor.css
│       │   └── HeatmapCell.razor          # Individual cell with item bullets
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── app.css                # Global resets, font, body sizing
│       │   └── data/
│       │       └── dashboard-data.json    # Sample data file
│       └── appsettings.json               # Only: data file path config
├── tests/
│   └── ReportingDashboard.Tests/
│       └── ReportingDashboard.Tests.csproj
└── docs/
    └── design-screenshots/
```
```
dashboard-data.json
        │
        ▼
DashboardDataService (Singleton, FileSystemWatcher)
        │
        ▼
Dashboard.razor (Page) ──► @inject IDashboardDataService
        │
        ├── Header.razor     [CascadingParameter or direct pass]
        ├── Timeline.razor   [Parameter: List<Milestone>]
        └── Heatmap.razor    [Parameter: HeatmapData]
                ├── HeatmapCell.razor × N
```
```csharp
public record DashboardConfig
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineTrack> TimelineTracks { get; init; }
    public HeatmapData Heatmap { get; init; }
}

public record ProjectInfo
{
    public string Title { get; init; }          // "Privacy Automation Release Roadmap"
    public string Subtitle { get; init; }       // "Trusted Platform · Privacy Automation..."
    public string BacklogUrl { get; init; }     // ADO link
    public string CurrentMonth { get; init; }   // "April 2026"
}

public record Milestone
{
    public string Label { get; init; }          // "Mar 26 PoC"
    public string Type { get; init; }           // "poc" | "production" | "checkpoint"
    public DateOnly Date { get; init; }
}

public record TimelineTrack
{
    public string Id { get; init; }             // "M1"
    public string Name { get; init; }           // "Chatbot & MS Role"
    public string Color { get; init; }          // "#0078D4"
    public List<Milestone> Milestones { get; init; }
}

public record HeatmapData
{
    public List<string> Months { get; init; }   // ["Jan", "Feb", "Mar", "Apr"]
    public string HighlightMonth { get; init; } // "Apr"
    public List<StatusRow> Rows { get; init; }
}

public record StatusRow
{
    public string Category { get; init; }       // "Shipped" | "In Progress" | "Carryover" | "Blockers"
    public Dictionary<string, List<string>> ItemsByMonth { get; init; }
}
``` **Approach:** Blazor scoped CSS (`.razor.css` files) for component-specific styles, plus one global `app.css` for resets and body sizing. Key CSS decisions from the original design:
- **Body:** Fixed `width: 1920px; height: 1080px; overflow: hidden` for screenshot fidelity
- **Grid:** `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months (data-driven)
- **Color system:** Define CSS custom properties in `app.css` for the four category color families:
  ```css
  :root {
    --shipped-bg: #F0FBF0;       --shipped-highlight: #D8F2DA;
    --shipped-accent: #34A853;   --shipped-header: #E8F5E9;
    --progress-bg: #EEF4FE;     --progress-highlight: #DAE8FB;
    --progress-accent: #0078D4; --progress-header: #E3F2FD;
    --carry-bg: #FFFDE7;        --carry-highlight: #FFF0B0;
    --carry-accent: #F4B400;    --carry-header: #FFF8E1;
    --block-bg: #FFF5F5;        --block-highlight: #FFE4E4;
    --block-accent: #EA4335;    --block-header: #FEF2F2;
  }
  ``` The timeline SVG should be **generated dynamically in Razor** based on milestone data, not hardcoded. Key calculations:
```csharp
// In Timeline.razor
@code {
    private double GetXPosition(DateOnly date)
    {
        var totalDays = (EndDate.ToDateTime(default) - StartDate.ToDateTime(default)).TotalDays;
        var offset = (date.ToDateTime(default) - StartDate.ToDateTime(default)).TotalDays;
        return (offset / totalDays) * SvgWidth;
    }
}
``` SVG elements needed:
- **Month gridlines:** Vertical `<line>` elements with `<text>` labels
- **"NOW" indicator:** Dashed red vertical line at current date position
- **Track lines:** Horizontal colored `<line>` per timeline track
- **Milestones:** `<polygon>` for diamonds (PoC/Production), `<circle>` for checkpoints
- **Labels:** `<text>` elements positioned relative to milestone markers
- **Drop shadow:** `<filter>` with `<feDropShadow>` for diamond markers ---

### Considerations & Risks

- **None.** This is explicitly a no-auth, local-only application. The dashboard is read-only and displays non-sensitive project status data. If future needs arise, the simplest addition would be Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (one line in `Program.cs`), but this is not recommended for the initial build.
- The `dashboard-data.json` file is the only data store. It contains project names and status descriptions — no PII, no credentials.
- No encryption needed. No HTTPS certificate needed for local development (though `dotnet dev-certs` provides one by default). | Aspect | Recommendation | |--------|---------------| | **Runtime** | `dotnet run` or `dotnet watch` during development | | **Production local** | `dotnet publish -c Release` → run the self-contained executable | | **Port** | Default `https://localhost:5001` or configure in `launchSettings.json` | | **Process management** | Windows Task Scheduler or run manually before taking screenshots | | **No containerization** | Docker is overkill for a local single-user app | | **No CI/CD** | Manual build; optionally a `build.ps1` script | **$0.** This runs entirely on the developer's local machine. No cloud services, no hosting, no recurring costs. --- | Risk | Likelihood | Impact | Mitigation | |------|-----------|--------|------------| | **Blazor Server SignalR overhead** for a static dashboard | Low | Low | The connection is idle; negligible resource use for a single user. Could switch to Blazor Static SSR (new in .NET 8) if desired. | | **JSON schema drift** — data file structure diverges from C# models | Medium | Medium | Add a JSON Schema (`$schema`) file and validate on load. Log clear error messages for missing fields. | | **SVG rendering inconsistencies** across browsers | Low | Medium | Test in Edge/Chrome (primary screenshot targets). SVG 1.1 features used are universally supported. | | **FileSystemWatcher reliability** on Windows | Low | Low | Known to occasionally miss events. Implement a polling fallback (check file timestamp every 5 seconds). | | **Over-engineering temptation** | High | Medium | Resist adding databases, APIs, auth, or component libraries. The simplicity IS the feature. | | Decision | What We Gain | What We Give Up | |----------|-------------|----------------| | JSON file instead of database | Zero-dependency data, human-editable, version-controllable | No querying, no multi-user writes, no history | | No charting library | Zero JS dependencies, pixel-perfect control, smaller bundle | Must hand-code SVG layout math | | Blazor Server instead of static HTML | C# data binding, component reuse, hot reload | SignalR connection overhead (negligible for local use) | | Fixed 1920×1080 layout | Screenshot-perfect output every time | Awkward on smaller dev screens (mitigate with browser zoom) | | No auth | Zero configuration, instant startup | Anyone on the machine can view (acceptable for local-only) | .NET 8 introduced **Blazor Static Server-Side Rendering** (no SignalR, pure HTML response). For a read-only dashboard, this is technically the ideal render mode. To use it:
```csharp
// In Dashboard.razor
@attribute [StreamRendering(false)]
@rendermode @(new ServerRenderMode(prerender: true))
``` This eliminates the WebSocket connection entirely. **Recommended** if the team is comfortable with .NET 8's new render mode system. ---
- **How many months should the heatmap display?** The original design shows 4 months (Jan–Apr). Should this be configurable in the JSON, or always show a rolling window?
- **Should the "NOW" line auto-calculate from system date or be specified in the JSON?** Auto-calculation is simpler but means the dashboard looks different each day. A fixed date in the JSON ensures screenshot consistency.
- **Multiple projects?** Is there ever a need to switch between different JSON files for different projects, or is this truly a single-project tool? (If multiple: add a dropdown or query parameter like `?project=privacy-automation`.)
- **Color-blind accessibility?** The original design relies heavily on red/green/yellow/blue color coding. Should we add patterns, icons, or labels for accessibility? (Low priority if this is screenshot-only for specific executives.)
- **Data update workflow?** Who edits the JSON file? Is it manually edited in VS Code, or should there be a simple form/editor? (Recommendation: keep it manual JSON editing for V1.)
- **Timeline date range?** Should the timeline always show 6 months (Jan–Jun as in the design), or should the range be data-driven from the earliest to latest milestone? ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Reporting Dashboard
**Date:** April 22, 2026
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure

---

## 1. Executive Summary

This project is a single-page executive reporting dashboard that visualizes project milestones, progress status, and monthly heatmap data. The application will be built with **Blazor Server on .NET 8**, load data from a **local JSON configuration file**, and render a pixel-perfect dashboard designed for screenshot capture at 1920×1080 for PowerPoint decks.

**Primary recommendation:** Keep the architecture radically simple — a single Blazor Server project with no database, no authentication, no API layer. Data lives in a `dashboard-data.json` file. The UI is built with pure CSS Grid/Flexbox and inline SVG (no charting library needed). The entire solution should be < 15 files and buildable in a single sprint.

---

## 2. Key Findings

- The original HTML design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`), **Flexbox**, and **inline SVG** — all of which Blazor Server renders natively without any JavaScript interop or third-party libraries.
- **No charting library is needed.** The timeline/Gantt section is a simple SVG with lines, circles, diamonds, and text — easily generated from data in a Blazor component. Adding a charting library would be over-engineering.
- **JSON is the optimal data format** for the configuration file: human-editable, natively supported via `System.Text.Json` in .NET 8, no extra dependencies, and structured enough to represent milestones, status categories, and heatmap items.
- The dashboard is **read-only and stateless** — no forms, no user input, no persistence. This eliminates the need for state management patterns, database layers, or API controllers.
- Blazor Server's **SignalR connection** is irrelevant for this use case (no interactivity needed), but the framework still provides the best developer experience for a .NET team rendering server-side HTML with C# models.
- The design targets a **fixed 1920×1080 viewport** for screenshot capture, which simplifies CSS — no responsive breakpoints needed, though a `@media` fallback for development on smaller screens is recommended.
- **Hot reload** in .NET 8 Blazor Server works well for iterating on CSS and component layout, which will be the primary development activity.
- The `Segoe UI` font specified in the design is pre-installed on Windows (the target environment), so no web font loading is required.
- **File watching** for `dashboard-data.json` changes can be implemented with `FileSystemWatcher` to enable live-reload of data without restarting the app.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components + CSS)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (.NET 8) | 8.0.x | Built-in, no additional package |
| **CSS Layout** | CSS Grid + Flexbox | Native | Matches original design exactly |
| **SVG Rendering** | Inline SVG via Razor | Native | Timeline milestones, diamonds, circles |
| **CSS Isolation** | Blazor scoped CSS (`.razor.css`) | Built-in | Per-component styles, no conflicts |
| **Icons/Shapes** | Pure CSS + inline SVG | N/A | Diamond shapes via `transform: rotate(45deg)`, circles via `border-radius` |

**No third-party UI/CSS frameworks recommended.** Bootstrap, MudBlazor, Radzen, etc. would add unnecessary weight and fight against the custom design. The original HTML/CSS is self-contained and should be translated directly into Blazor components with scoped CSS.

### Backend (Data Loading)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built-in (.NET 8) | Native, fast, no extra dependency |
| **File Watching** | `System.IO.FileSystemWatcher` | Built-in (.NET 8) | Auto-reload on data file changes |
| **Configuration** | `dashboard-data.json` (custom file) | N/A | Not `appsettings.json` — separate concern |
| **DI Registration** | `IServiceCollection` | Built-in | Singleton service for data provider |

### Data Storage

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Primary Store** | `dashboard-data.json` flat file | Human-editable, version-controllable |
| **Format** | JSON with `$schema` reference | Enables IDE autocomplete when editing |
| **Location** | Project root or `wwwroot/data/` | Configurable via `appsettings.json` path |

**No database.** SQLite, LiteDB, and other embedded databases are unnecessary for a read-only dashboard with a single data file. A flat JSON file is the simplest, most portable, and most editable option.

### Testing

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Tests** | xUnit | 2.7.x | .NET ecosystem standard |
| **Assertions** | FluentAssertions | 6.12.x | Readable test assertions |
| **Blazor Component Tests** | bUnit | 1.25.x+ | Renders Blazor components in-memory |
| **JSON Validation** | Manual test fixtures | N/A | Validate deserialization of sample data |

### Build & Tooling

| Tool | Version | Notes |
|------|---------|-------|
| **.NET SDK** | 8.0.x | `dotnet new blazorserver` template |
| **IDE** | Visual Studio 2022 or VS Code + C# Dev Kit | Both fully support .NET 8 Blazor |
| **Hot Reload** | Built-in `dotnet watch` | CSS + Razor changes without restart |

---

## 4. Architecture Recommendations

### Overall Pattern: **Single-Project Minimal Architecture**

```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Models/
│       │   └── DashboardData.cs          # POCO models for JSON deserialization
│       ├── Services/
│       │   └── DashboardDataService.cs    # Loads & caches JSON, watches for changes
│       ├── Components/
│       │   ├── Pages/
│       │   │   └── Dashboard.razor        # Single page — the entire app
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor       # Minimal layout wrapper
│       │   │   └── MainLayout.razor.css
│       │   ├── Header.razor               # Title, subtitle, legend
│       │   ├── Header.razor.css
│       │   ├── Timeline.razor             # SVG milestone Gantt chart
│       │   ├── Timeline.razor.css
│       │   ├── Heatmap.razor              # Status grid (Shipped/InProgress/Carryover/Blockers)
│       │   ├── Heatmap.razor.css
│       │   └── HeatmapCell.razor          # Individual cell with item bullets
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── app.css                # Global resets, font, body sizing
│       │   └── data/
│       │       └── dashboard-data.json    # Sample data file
│       └── appsettings.json               # Only: data file path config
├── tests/
│   └── ReportingDashboard.Tests/
│       └── ReportingDashboard.Tests.csproj
└── docs/
    └── design-screenshots/
```

### Data Flow

```
dashboard-data.json
        │
        ▼
DashboardDataService (Singleton, FileSystemWatcher)
        │
        ▼
Dashboard.razor (Page) ──► @inject IDashboardDataService
        │
        ├── Header.razor     [CascadingParameter or direct pass]
        ├── Timeline.razor   [Parameter: List<Milestone>]
        └── Heatmap.razor    [Parameter: HeatmapData]
                ├── HeatmapCell.razor × N
```

### Data Model (JSON → C# POCOs)

```csharp
public record DashboardConfig
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineTrack> TimelineTracks { get; init; }
    public HeatmapData Heatmap { get; init; }
}

public record ProjectInfo
{
    public string Title { get; init; }          // "Privacy Automation Release Roadmap"
    public string Subtitle { get; init; }       // "Trusted Platform · Privacy Automation..."
    public string BacklogUrl { get; init; }     // ADO link
    public string CurrentMonth { get; init; }   // "April 2026"
}

public record Milestone
{
    public string Label { get; init; }          // "Mar 26 PoC"
    public string Type { get; init; }           // "poc" | "production" | "checkpoint"
    public DateOnly Date { get; init; }
}

public record TimelineTrack
{
    public string Id { get; init; }             // "M1"
    public string Name { get; init; }           // "Chatbot & MS Role"
    public string Color { get; init; }          // "#0078D4"
    public List<Milestone> Milestones { get; init; }
}

public record HeatmapData
{
    public List<string> Months { get; init; }   // ["Jan", "Feb", "Mar", "Apr"]
    public string HighlightMonth { get; init; } // "Apr"
    public List<StatusRow> Rows { get; init; }
}

public record StatusRow
{
    public string Category { get; init; }       // "Shipped" | "In Progress" | "Carryover" | "Blockers"
    public Dictionary<string, List<string>> ItemsByMonth { get; init; }
}
```

### CSS Architecture

**Approach:** Blazor scoped CSS (`.razor.css` files) for component-specific styles, plus one global `app.css` for resets and body sizing.

Key CSS decisions from the original design:
- **Body:** Fixed `width: 1920px; height: 1080px; overflow: hidden` for screenshot fidelity
- **Grid:** `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months (data-driven)
- **Color system:** Define CSS custom properties in `app.css` for the four category color families:
  ```css
  :root {
    --shipped-bg: #F0FBF0;       --shipped-highlight: #D8F2DA;
    --shipped-accent: #34A853;   --shipped-header: #E8F5E9;
    --progress-bg: #EEF4FE;     --progress-highlight: #DAE8FB;
    --progress-accent: #0078D4; --progress-header: #E3F2FD;
    --carry-bg: #FFFDE7;        --carry-highlight: #FFF0B0;
    --carry-accent: #F4B400;    --carry-header: #FFF8E1;
    --block-bg: #FFF5F5;        --block-highlight: #FFE4E4;
    --block-accent: #EA4335;    --block-header: #FEF2F2;
  }
  ```

### SVG Timeline Generation

The timeline SVG should be **generated dynamically in Razor** based on milestone data, not hardcoded. Key calculations:

```csharp
// In Timeline.razor
@code {
    private double GetXPosition(DateOnly date)
    {
        var totalDays = (EndDate.ToDateTime(default) - StartDate.ToDateTime(default)).TotalDays;
        var offset = (date.ToDateTime(default) - StartDate.ToDateTime(default)).TotalDays;
        return (offset / totalDays) * SvgWidth;
    }
}
```

SVG elements needed:
- **Month gridlines:** Vertical `<line>` elements with `<text>` labels
- **"NOW" indicator:** Dashed red vertical line at current date position
- **Track lines:** Horizontal colored `<line>` per timeline track
- **Milestones:** `<polygon>` for diamonds (PoC/Production), `<circle>` for checkpoints
- **Labels:** `<text>` elements positioned relative to milestone markers
- **Drop shadow:** `<filter>` with `<feDropShadow>` for diamond markers

---

## 5. Security & Infrastructure

### Authentication & Authorization
**None.** This is explicitly a no-auth, local-only application. The dashboard is read-only and displays non-sensitive project status data. If future needs arise, the simplest addition would be Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (one line in `Program.cs`), but this is not recommended for the initial build.

### Data Protection
- The `dashboard-data.json` file is the only data store. It contains project names and status descriptions — no PII, no credentials.
- No encryption needed. No HTTPS certificate needed for local development (though `dotnet dev-certs` provides one by default).

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | `dotnet run` or `dotnet watch` during development |
| **Production local** | `dotnet publish -c Release` → run the self-contained executable |
| **Port** | Default `https://localhost:5001` or configure in `launchSettings.json` |
| **Process management** | Windows Task Scheduler or run manually before taking screenshots |
| **No containerization** | Docker is overkill for a local single-user app |
| **No CI/CD** | Manual build; optionally a `build.ps1` script |

### Infrastructure Costs
**$0.** This runs entirely on the developer's local machine. No cloud services, no hosting, no recurring costs.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| **Blazor Server SignalR overhead** for a static dashboard | Low | Low | The connection is idle; negligible resource use for a single user. Could switch to Blazor Static SSR (new in .NET 8) if desired. |
| **JSON schema drift** — data file structure diverges from C# models | Medium | Medium | Add a JSON Schema (`$schema`) file and validate on load. Log clear error messages for missing fields. |
| **SVG rendering inconsistencies** across browsers | Low | Medium | Test in Edge/Chrome (primary screenshot targets). SVG 1.1 features used are universally supported. |
| **FileSystemWatcher reliability** on Windows | Low | Low | Known to occasionally miss events. Implement a polling fallback (check file timestamp every 5 seconds). |
| **Over-engineering temptation** | High | Medium | Resist adding databases, APIs, auth, or component libraries. The simplicity IS the feature. |

### Trade-offs Made

| Decision | What We Gain | What We Give Up |
|----------|-------------|----------------|
| JSON file instead of database | Zero-dependency data, human-editable, version-controllable | No querying, no multi-user writes, no history |
| No charting library | Zero JS dependencies, pixel-perfect control, smaller bundle | Must hand-code SVG layout math |
| Blazor Server instead of static HTML | C# data binding, component reuse, hot reload | SignalR connection overhead (negligible for local use) |
| Fixed 1920×1080 layout | Screenshot-perfect output every time | Awkward on smaller dev screens (mitigate with browser zoom) |
| No auth | Zero configuration, instant startup | Anyone on the machine can view (acceptable for local-only) |

### Blazor Static SSR Alternative
.NET 8 introduced **Blazor Static Server-Side Rendering** (no SignalR, pure HTML response). For a read-only dashboard, this is technically the ideal render mode. To use it:
```csharp
// In Dashboard.razor
@attribute [StreamRendering(false)]
@rendermode @(new ServerRenderMode(prerender: true))
```
This eliminates the WebSocket connection entirely. **Recommended** if the team is comfortable with .NET 8's new render mode system.

---

## 7. Open Questions

1. **How many months should the heatmap display?** The original design shows 4 months (Jan–Apr). Should this be configurable in the JSON, or always show a rolling window?

2. **Should the "NOW" line auto-calculate from system date or be specified in the JSON?** Auto-calculation is simpler but means the dashboard looks different each day. A fixed date in the JSON ensures screenshot consistency.

3. **Multiple projects?** Is there ever a need to switch between different JSON files for different projects, or is this truly a single-project tool? (If multiple: add a dropdown or query parameter like `?project=privacy-automation`.)

4. **Color-blind accessibility?** The original design relies heavily on red/green/yellow/blue color coding. Should we add patterns, icons, or labels for accessibility? (Low priority if this is screenshot-only for specific executives.)

5. **Data update workflow?** Who edits the JSON file? Is it manually edited in VS Code, or should there be a simple form/editor? (Recommendation: keep it manual JSON editing for V1.)

6. **Timeline date range?** Should the timeline always show 6 months (Jan–Jun as in the design), or should the range be data-driven from the earliest to latest milestone?

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** Render the dashboard from a JSON file, matching the original design.

1. **Scaffold the project** — `dotnet new blazorserver -n ReportingDashboard --framework net8.0`
2. **Define C# models** — `DashboardConfig`, `ProjectInfo`, `Milestone`, `StatusRow`, etc.
3. **Create sample `dashboard-data.json`** with fictional project data matching the original design's structure
4. **Build `DashboardDataService`** — singleton that reads and deserializes JSON on startup
5. **Build `Dashboard.razor` page** — single page, inject data service
6. **Build `Header.razor`** — title, subtitle, legend icons (translate directly from HTML)
7. **Build `Heatmap.razor` + `HeatmapCell.razor`** — CSS Grid layout, color-coded rows
8. **Build `Timeline.razor`** — SVG generation from milestone data
9. **Global CSS** — body sizing, font, resets in `app.css`

### Phase 2: Polish (1 day)

1. **FileSystemWatcher** — auto-reload data when JSON changes (no app restart)
2. **Error handling** — friendly message if JSON is malformed or missing
3. **JSON Schema file** — enable autocomplete in VS Code for data editing
4. **Print/screenshot CSS** — `@media print` styles for clean output
5. **Browser zoom hint** — small dev-mode banner suggesting 100% zoom for accurate screenshots

### Phase 3: Optional Enhancements (future)

- **Query parameter routing** — `?project=my-project` loads `my-project.json`
- **Blazor Static SSR** — remove SignalR for pure HTML rendering
- **Dark mode** — alternate CSS variables for dark-background PowerPoint slides
- **PDF export** — server-side HTML-to-PDF via `Playwright` or `PuppeteerSharp` (but screenshotting is simpler)

### Quick Wins

| Quick Win | Effort | Impact |
|-----------|--------|--------|
| Copy-paste original CSS into Blazor scoped CSS files | 30 min | Instant visual parity |
| `dotnet watch` for hot reload during CSS tweaking | 0 min (built-in) | Fast iteration |
| JSON sample data matching original design | 30 min | Validates data model immediately |
| CSS custom properties for color themes | 15 min | Easy to adjust palette later |

### Recommended JSON Data File Format

**File name:** `dashboard-data.json`
**Location:** `wwwroot/data/dashboard-data.json` (or configurable path)

```json
{
  "$schema": "dashboard-schema.json",
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "currentDate": "2026-04-10"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "name": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
          { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
          { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
        ]
      },
      {
        "id": "M2",
        "name": "PDS & Data Inventory",
        "color": "#00897B",
        "milestones": [
          { "date": "2025-12-19", "label": "Dec 19", "type": "checkpoint" },
          { "date": "2026-02-11", "label": "Feb 11", "type": "checkpoint" },
          { "date": "2026-03-15", "label": "Mar 15 PoC", "type": "poc" },
          { "date": "2026-04-18", "label": "Apr Prod", "type": "production" }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "highlightMonth": "Apr",
    "rows": [
      {
        "category": "Shipped",
        "items": {
          "Jan": ["CELA Chatbot v1", "Role assignment API"],
          "Feb": ["PDS connector", "Data classification rules"],
          "Mar": ["Auto-review pipeline", "DFD generator v1"],
          "Apr": ["Compliance dashboard", "Audit trail export"]
        }
      },
      {
        "category": "In Progress",
        "items": {
          "Jan": ["MS Role integration"],
          "Feb": ["Inventory scanner"],
          "Mar": ["Review automation"],
          "Apr": ["E2E testing", "Perf optimization", "Docs update"]
        }
      },
      {
        "category": "Carryover",
        "items": {
          "Jan": [],
          "Feb": ["Auth token refresh fix"],
          "Mar": ["Legacy data migration"],
          "Apr": ["Cross-team API contract"]
        }
      },
      {
        "category": "Blockers",
        "items": {
          "Jan": [],
          "Feb": [],
          "Mar": ["Upstream service outage"],
          "Apr": ["Dependency on Platform v3 release"]
        }
      }
    ]
  }
}
```

### NuGet Packages Summary

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Blazor Server runtime | Yes (implicit) |
| `System.Text.Json` | 8.0.x | JSON deserialization | Yes (built-in) |
| `xunit` | 2.7.x | Unit testing | For tests project |
| `bunit` | 1.25.x | Blazor component testing | For tests project |
| `FluentAssertions` | 6.12.x | Readable test assertions | For tests project |

**Total third-party runtime dependencies: 0.** Everything needed is built into .NET 8.

---

*This research document provides the engineering team with everything needed to begin implementation immediately. The architecture is intentionally minimal — the simplicity of a single JSON file driving a single Blazor page is the correct design for a screenshot-oriented executive dashboard.*

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/fe3fc76172977fed76511f41ca8bddc4769300f8/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
