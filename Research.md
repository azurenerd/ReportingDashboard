# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-13 22:20 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work item statuses (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The architecture is intentionally minimal—no authentication, no database, no enterprise security—optimized for generating polished screenshots for executive PowerPoint decks. **Primary Recommendation:** Build a single Blazor Server project with zero external JavaScript dependencies. Use inline SVG for the timeline, CSS Grid for the heatmap, and `System.Text.Json` for config deserialization. The entire solution can be one `.razor` page with a backing model class and a JSON service. Target delivery: a working dashboard in a single sprint. ---

### Key Findings

- The original HTML design is a static 1920×1080 fixed-layout page using CSS Grid, Flexbox, and inline SVG—all of which translate directly to Blazor Server `.razor` components with zero JavaScript interop needed.
- Blazor Server in .NET 8 supports static SSR (`@rendermode` directives), which is ideal for a read-only dashboard that just renders data from a JSON file—no WebSocket overhead needed if using static SSR.
- No charting library is necessary. The timeline uses simple SVG primitives (lines, circles, polygons, text) that are trivially rendered in Razor markup with `@foreach` loops over milestone data.
- The heatmap grid is pure CSS Grid with colored cells—no library needed, just CSS classes matching the original design's color tokens.
- `System.Text.Json` (built into .NET 8) is the only dependency needed for JSON deserialization. No NuGet packages are required for the MVP.
- The 1920×1080 fixed viewport design means no responsive design work is needed—the page is purpose-built for screenshot capture.
- Blazor's component model maps cleanly to the visual sections: Header, Timeline, Heatmap, each as a separate `.razor` component. ---
- `dotnet new blazor -n ReportingDashboard --interactivity Server`
- Create `data.json` with sample data matching the fictional project
- Create `DashboardDataService` to read and deserialize JSON
- Build `Dashboard.razor` as a single page with inline HTML/CSS matching the original design
- Port the exact CSS from `OriginalDesignConcept.html`
- Verify at `https://localhost:5001` — screenshot should match the design reference
- Extract `DashboardHeader.razor`, `TimelineSection.razor`, `HeatmapSection.razor`
- Add CSS isolation (`.razor.css` files)
- Add XML doc comments to the data model
- Add Playwright-based screenshot automation script
- Add a `--watch` mode to auto-refresh when `data.json` changes (use `FileSystemWatcher`)
- Add a simple print stylesheet
- **Copy the original CSS verbatim** from `OriginalDesignConcept.html` into your Blazor app. Modify only what's needed to support data binding. This eliminates CSS debugging.
- **Start with a single `Dashboard.razor` file** containing everything. Extract components only if the file exceeds ~300 lines.
- **Use `wwwroot/data.json`** so it's served as a static file AND readable by the service. No path gymnastics.
- **Hot reload** via `dotnet watch run` during development—instant CSS and Razor changes.
```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project",
    "currentMonth": "Apr"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "m1",
        "label": "M1",
        "description": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
          { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
          { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "highlightMonth": "Apr",
    "categories": [
      {
        "name": "Shipped",
        "cssClass": "ship",
        "items": {
          "Jan": ["DSR v2 Rollout", "Privacy Portal GA"],
          "Feb": ["DSAR Automation", "Consent API v3"],
          "Mar": ["DPO Dashboard", "Audit Log Export"],
          "Apr": ["Retention Policies", "Data Lineage MVP"]
        }
      }
    ]
  }
}
```
```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs                          # DI registration, minimal config
    ├── Services/
    │   └── DashboardDataService.cs         # Reads & caches data.json
    ├── Models/
    │   └── DashboardData.cs                # Records for JSON deserialization
    ├── Components/
    │   ├── App.razor
    │   ├── Layout/
    │   │   └── MainLayout.razor
    │   └── Pages/
    │       └── Dashboard.razor             # The single dashboard page
    │       └── Dashboard.razor.css         # Scoped CSS (ported from original)
    └── wwwroot/
        ├── css/
        │   └── app.css                     # Global reset + custom properties
        └── data.json                       # Dashboard configuration data
```
- **Do not use `@rendermode InteractiveServer` unless interactivity is needed.** Static SSR is faster and avoids WebSocket connections for a read-only page.
- **The SVG width should be calculated from the container**, not hardcoded to 1560px. Use `@code` to compute milestone X positions based on date math.
- **The heatmap "highlight" column** (current month with darker background) should be driven by `data.json`'s `highlightMonth` field, not `DateTime.Now`.
- **CSS Grid `grid-template-rows: 36px repeat(4, 1fr)`** for the heatmap means the header row is fixed-height and data rows stretch equally—preserve this from the original design.
- **Drop shadow filter on SVG diamonds**: Include the `<defs><filter id="sh">` exactly as in the original HTML. Blazor renders SVG natively; no special handling needed.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Use static SSR mode (`@rendermode` not set, or `InteractiveServer` only if needed) | | **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches original design exactly; no CSS framework needed | | **SVG Rendering** | Inline SVG in Razor | N/A | Timeline milestones rendered via `<svg>` elements in `.razor` files | | **Fonts** | Segoe UI (system font) | N/A | Already available on Windows; matches design spec | | **Icons/Shapes** | CSS transforms + SVG polygons | N/A | Diamond shapes via `transform: rotate(45deg)`, circles via SVG | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Zero-dependency; use `JsonSerializer.Deserialize<T>()` | | **File Reading** | `IFileProvider` / `File.ReadAllTextAsync` | Built into .NET 8 | Read `data.json` from `wwwroot/` or project root | | **DI Service** | Custom `DashboardDataService` | N/A | Singleton service registered in `Program.cs` | | **Configuration** | `data.json` (custom) | N/A | Not `appsettings.json`—separate file for dashboard data | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Project Type** | Blazor Web App (Server) | .NET 8 | `dotnet new blazor --interactivity Server` | | **Solution Structure** | Single `.sln` with one `.csproj` | N/A | No need for multi-project; this is intentionally simple | | **Build Tool** | `dotnet` CLI | 8.0.x | `dotnet build`, `dotnet run` | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Tests** | xUnit | 2.7+ | Only if testing JSON deserialization logic | | **Component Tests** | bUnit | 1.25+ | Only if testing Razor component rendering | | **Screenshot Validation** | Playwright for .NET | 1.41+ | Optional; automate screenshot capture at 1920×1080 | | Library | Why Not | |---------|---------| | MudBlazor / Radzen / Syncfusion | Overkill; the design is custom CSS, not a component library layout | | Chart.js / ApexCharts / Plotly | Timeline is simple SVG; heatmap is CSS Grid with colored divs | | Entity Framework | No database; data comes from a JSON file | | Bootstrap / Tailwind | Fixed 1920×1080 layout with custom CSS; frameworks add unnecessary weight | | SignalR (explicit) | Built into Blazor Server already; no custom hub needed | | Any authentication library | Explicitly out of scope | ---
```
data.json ──→ DashboardDataService ──→ Razor Components ──→ HTML/CSS/SVG
                (Singleton, loaded once)     (Header, Timeline, Heatmap)
``` This is a **read-only rendering pipeline**. No state management, no user input handling, no CRUD operations. Map directly to the visual design sections:
```
App.razor
└── MainLayout.razor
    └── Dashboard.razor (single page, route: "/")
        ├── DashboardHeader.razor
        │   ├── Title, subtitle, date
        │   └── Legend (milestone symbols)
        ├── TimelineSection.razor
        │   ├── TimelineLabels.razor (M1, M2, M3 sidebar)
        │   └── TimelineSvg.razor (SVG with month lines, milestones, "NOW" marker)
        └── HeatmapSection.razor
            ├── HeatmapHeader.razor (month column headers)
            └── HeatmapRow.razor × 4 (Shipped, In Progress, Carryover, Blockers)
                └── HeatmapCell.razor (individual cells with item bullets)
```
```csharp
public record DashboardData
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineTrack> Tracks { get; init; }
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
    public DateTime Date { get; init; }
    public string TrackId { get; init; }        // Which timeline track
}

public record TimelineTrack
{
    public string Id { get; init; }
    public string Label { get; init; }          // "M1"
    public string Description { get; init; }    // "Chatbot & MS Role"
    public string Color { get; init; }          // "#0078D4"
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public record HeatmapData
{
    public List<string> Months { get; init; }               // ["Jan", "Feb", "Mar", "Apr"]
    public string HighlightMonth { get; init; }             // "Apr" (current month)
    public List<HeatmapCategory> Categories { get; init; }  // Shipped, In Progress, etc.
}

public record HeatmapCategory
{
    public string Name { get; init; }           // "Shipped"
    public string CssClass { get; init; }       // "ship"
    public Dictionary<string, List<string>> Items { get; init; } // month → items
}
``` **Directly port the original HTML design's CSS** into a scoped CSS file (`Dashboard.razor.css`) or a single `dashboard.css` in `wwwroot/css/`. Key decisions:
- **Fixed viewport**: `width: 1920px; height: 1080px; overflow: hidden;` on the body/root element. This is not a responsive app—it's a screenshot target.
- **CSS Grid for heatmap**: `grid-template-columns: 160px repeat(4, 1fr)` exactly as in the original design.
- **Color tokens as CSS custom properties**:
```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-highlight: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-highlight: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-highlight: #FFF0B0;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-highlight: #FFE4E4;
}
```
- **No CSS preprocessor needed**. Plain CSS with custom properties is sufficient. The timeline SVG should be generated in Razor using data-driven positioning:
```csharp
@code {
    private double GetXPosition(DateTime date)
    {
        var totalDays = (EndDate - StartDate).TotalDays;
        var dayOffset = (date - StartDate).TotalDays;
        return (dayOffset / totalDays) * SvgWidth;
    }
}
``` SVG elements to render per milestone type:
- **Checkpoint**: `<circle>` with white fill, colored stroke
- **PoC Milestone**: `<polygon>` (diamond) with `#F4B400` fill + drop shadow filter
- **Production Release**: `<polygon>` (diamond) with `#34A853` fill + drop shadow filter
- **"NOW" line**: `<line>` with `stroke="#EA4335"` and `stroke-dasharray="5,3"`
- `Program.cs` registers `DashboardDataService` as a singleton
- Service reads and deserializes `data.json` on first access (lazy-loaded, cached)
- `Dashboard.razor` injects the service, calls `GetDashboardData()`
- Child components receive data via `[Parameter]` attributes
- No interactivity needed—pure server-side rendering ---

### Considerations & Risks

- **None.** Explicitly out of scope per requirements. The app runs locally—no login, no roles, no tokens. If ever needed later: add `Microsoft.AspNetCore.Authentication.Negotiate` for Windows auth (one line in `Program.cs`).
- `data.json` contains project status data, not secrets. No encryption needed.
- If sensitive project names are a concern, keep `data.json` out of source control via `.gitignore`. | Aspect | Recommendation | |--------|---------------| | **Runtime** | Local `dotnet run` on developer's machine | | **Port** | Default `https://localhost:5001` or `http://localhost:5000` | | **Deployment** | Not deployed anywhere; run locally, screenshot, close | | **Alternative** | `dotnet publish -c Release` → run the exe directly | | **Screenshot Tool** | Browser DevTools (Ctrl+Shift+P → "Capture full size screenshot") or Playwright automation | **$0.** This runs on a developer's laptop. No cloud resources, no hosting, no licenses (all tools are free/included with .NET SDK). --- | Risk | Severity | Mitigation | |------|----------|------------| | **SVG rendering differences across browsers** | Low | Target Chrome/Edge only for screenshots; both use Chromium | | **Blazor Server overhead for a static page** | Low | Use static SSR mode; or consider publishing as a static HTML file via `dotnet publish` | | **JSON schema changes breaking deserialization** | Low | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and nullable properties | | **Segoe UI font not available on non-Windows** | Low | App is Windows-only per requirements; Segoe UI is preinstalled | | Decision | Trade-off | Rationale | |----------|-----------|-----------| | No component library (MudBlazor, etc.) | Must write custom CSS | Design is pixel-specific; libraries would fight the custom layout | | No database | Must edit JSON file manually | Simplicity; a JSON file is the easiest "config" format for one-off updates | | Fixed 1920×1080 viewport | Not usable on mobile/small screens | Purpose-built for screenshots; responsive design adds unnecessary complexity | | Single project, no layers | No separation of concerns | Appropriate for a <500 line app; over-engineering would slow delivery | | No JavaScript interop | No client-side interactivity | Dashboard is read-only; server-side rendering is sufficient | Not applicable. This is a single-user local tool. If it ever needs to serve multiple users, the migration path is:
- Deploy to IIS/Kestrel on an internal server
- Add Windows Authentication (one line)
- Add multiple `data.json` files with a project selector dropdown --- | # | Question | Recommended Default | Needs Input From | |---|----------|-------------------|------------------| | 1 | How many months should the heatmap display? | 4 months (matching original design) | Product Owner | | 2 | How many timeline tracks maximum? | 3-5 tracks | Product Owner | | 3 | Should the "NOW" marker auto-calculate from system date or be specified in data.json? | Auto-calculate from `DateTime.Now` | Developer preference | | 4 | Should the data.json include the color scheme, or should colors be hardcoded per category? | Hardcoded in CSS; data.json only has content | Design decision | | 5 | Is Playwright screenshot automation desired, or is manual screenshot sufficient? | Manual for now; automate if used frequently | End user | | 6 | Should the backlog URL (ADO link) in the header be clickable, or just display text? | Clickable `<a>` tag matching original design | End user | | 7 | What date range should the timeline span? | Configurable in data.json (startDate, endDate) | Product Owner | ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work item statuses (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The architecture is intentionally minimal—no authentication, no database, no enterprise security—optimized for generating polished screenshots for executive PowerPoint decks.

**Primary Recommendation:** Build a single Blazor Server project with zero external JavaScript dependencies. Use inline SVG for the timeline, CSS Grid for the heatmap, and `System.Text.Json` for config deserialization. The entire solution can be one `.razor` page with a backing model class and a JSON service. Target delivery: a working dashboard in a single sprint.

---

## 2. Key Findings

- The original HTML design is a static 1920×1080 fixed-layout page using CSS Grid, Flexbox, and inline SVG—all of which translate directly to Blazor Server `.razor` components with zero JavaScript interop needed.
- Blazor Server in .NET 8 supports static SSR (`@rendermode` directives), which is ideal for a read-only dashboard that just renders data from a JSON file—no WebSocket overhead needed if using static SSR.
- No charting library is necessary. The timeline uses simple SVG primitives (lines, circles, polygons, text) that are trivially rendered in Razor markup with `@foreach` loops over milestone data.
- The heatmap grid is pure CSS Grid with colored cells—no library needed, just CSS classes matching the original design's color tokens.
- `System.Text.Json` (built into .NET 8) is the only dependency needed for JSON deserialization. No NuGet packages are required for the MVP.
- The 1920×1080 fixed viewport design means no responsive design work is needed—the page is purpose-built for screenshot capture.
- Blazor's component model maps cleanly to the visual sections: Header, Timeline, Heatmap, each as a separate `.razor` component.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Use static SSR mode (`@rendermode` not set, or `InteractiveServer` only if needed) |
| **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches original design exactly; no CSS framework needed |
| **SVG Rendering** | Inline SVG in Razor | N/A | Timeline milestones rendered via `<svg>` elements in `.razor` files |
| **Fonts** | Segoe UI (system font) | N/A | Already available on Windows; matches design spec |
| **Icons/Shapes** | CSS transforms + SVG polygons | N/A | Diamond shapes via `transform: rotate(45deg)`, circles via SVG |

### Backend / Data Layer

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Zero-dependency; use `JsonSerializer.Deserialize<T>()` |
| **File Reading** | `IFileProvider` / `File.ReadAllTextAsync` | Built into .NET 8 | Read `data.json` from `wwwroot/` or project root |
| **DI Service** | Custom `DashboardDataService` | N/A | Singleton service registered in `Program.cs` |
| **Configuration** | `data.json` (custom) | N/A | Not `appsettings.json`—separate file for dashboard data |

### Project Structure

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Project Type** | Blazor Web App (Server) | .NET 8 | `dotnet new blazor --interactivity Server` |
| **Solution Structure** | Single `.sln` with one `.csproj` | N/A | No need for multi-project; this is intentionally simple |
| **Build Tool** | `dotnet` CLI | 8.0.x | `dotnet build`, `dotnet run` |

### Testing (Optional/Lightweight)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Tests** | xUnit | 2.7+ | Only if testing JSON deserialization logic |
| **Component Tests** | bUnit | 1.25+ | Only if testing Razor component rendering |
| **Screenshot Validation** | Playwright for .NET | 1.41+ | Optional; automate screenshot capture at 1920×1080 |

### Libraries NOT Needed

| Library | Why Not |
|---------|---------|
| MudBlazor / Radzen / Syncfusion | Overkill; the design is custom CSS, not a component library layout |
| Chart.js / ApexCharts / Plotly | Timeline is simple SVG; heatmap is CSS Grid with colored divs |
| Entity Framework | No database; data comes from a JSON file |
| Bootstrap / Tailwind | Fixed 1920×1080 layout with custom CSS; frameworks add unnecessary weight |
| SignalR (explicit) | Built into Blazor Server already; no custom hub needed |
| Any authentication library | Explicitly out of scope |

---

## 4. Architecture Recommendations

### 4.1 Overall Pattern: Single-Page Read-Only Dashboard

```
data.json ──→ DashboardDataService ──→ Razor Components ──→ HTML/CSS/SVG
                (Singleton, loaded once)     (Header, Timeline, Heatmap)
```

This is a **read-only rendering pipeline**. No state management, no user input handling, no CRUD operations.

### 4.2 Component Architecture

Map directly to the visual design sections:

```
App.razor
└── MainLayout.razor
    └── Dashboard.razor (single page, route: "/")
        ├── DashboardHeader.razor
        │   ├── Title, subtitle, date
        │   └── Legend (milestone symbols)
        ├── TimelineSection.razor
        │   ├── TimelineLabels.razor (M1, M2, M3 sidebar)
        │   └── TimelineSvg.razor (SVG with month lines, milestones, "NOW" marker)
        └── HeatmapSection.razor
            ├── HeatmapHeader.razor (month column headers)
            └── HeatmapRow.razor × 4 (Shipped, In Progress, Carryover, Blockers)
                └── HeatmapCell.razor (individual cells with item bullets)
```

### 4.3 Data Model

```csharp
public record DashboardData
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineTrack> Tracks { get; init; }
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
    public DateTime Date { get; init; }
    public string TrackId { get; init; }        // Which timeline track
}

public record TimelineTrack
{
    public string Id { get; init; }
    public string Label { get; init; }          // "M1"
    public string Description { get; init; }    // "Chatbot & MS Role"
    public string Color { get; init; }          // "#0078D4"
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public record HeatmapData
{
    public List<string> Months { get; init; }               // ["Jan", "Feb", "Mar", "Apr"]
    public string HighlightMonth { get; init; }             // "Apr" (current month)
    public List<HeatmapCategory> Categories { get; init; }  // Shipped, In Progress, etc.
}

public record HeatmapCategory
{
    public string Name { get; init; }           // "Shipped"
    public string CssClass { get; init; }       // "ship"
    public Dictionary<string, List<string>> Items { get; init; } // month → items
}
```

### 4.4 CSS Strategy

**Directly port the original HTML design's CSS** into a scoped CSS file (`Dashboard.razor.css`) or a single `dashboard.css` in `wwwroot/css/`. Key decisions:

- **Fixed viewport**: `width: 1920px; height: 1080px; overflow: hidden;` on the body/root element. This is not a responsive app—it's a screenshot target.
- **CSS Grid for heatmap**: `grid-template-columns: 160px repeat(4, 1fr)` exactly as in the original design.
- **Color tokens as CSS custom properties**:

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-highlight: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-highlight: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-highlight: #FFF0B0;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-highlight: #FFE4E4;
}
```

- **No CSS preprocessor needed**. Plain CSS with custom properties is sufficient.

### 4.5 SVG Timeline Rendering

The timeline SVG should be generated in Razor using data-driven positioning:

```csharp
@code {
    private double GetXPosition(DateTime date)
    {
        var totalDays = (EndDate - StartDate).TotalDays;
        var dayOffset = (date - StartDate).TotalDays;
        return (dayOffset / totalDays) * SvgWidth;
    }
}
```

SVG elements to render per milestone type:
- **Checkpoint**: `<circle>` with white fill, colored stroke
- **PoC Milestone**: `<polygon>` (diamond) with `#F4B400` fill + drop shadow filter
- **Production Release**: `<polygon>` (diamond) with `#34A853` fill + drop shadow filter
- **"NOW" line**: `<line>` with `stroke="#EA4335"` and `stroke-dasharray="5,3"`

### 4.6 Data Flow

1. `Program.cs` registers `DashboardDataService` as a singleton
2. Service reads and deserializes `data.json` on first access (lazy-loaded, cached)
3. `Dashboard.razor` injects the service, calls `GetDashboardData()`
4. Child components receive data via `[Parameter]` attributes
5. No interactivity needed—pure server-side rendering

---

## 5. Security & Infrastructure

### 5.1 Authentication & Authorization

**None.** Explicitly out of scope per requirements. The app runs locally—no login, no roles, no tokens.

If ever needed later: add `Microsoft.AspNetCore.Authentication.Negotiate` for Windows auth (one line in `Program.cs`).

### 5.2 Data Protection

- `data.json` contains project status data, not secrets. No encryption needed.
- If sensitive project names are a concern, keep `data.json` out of source control via `.gitignore`.

### 5.3 Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Local `dotnet run` on developer's machine |
| **Port** | Default `https://localhost:5001` or `http://localhost:5000` |
| **Deployment** | Not deployed anywhere; run locally, screenshot, close |
| **Alternative** | `dotnet publish -c Release` → run the exe directly |
| **Screenshot Tool** | Browser DevTools (Ctrl+Shift+P → "Capture full size screenshot") or Playwright automation |

### 5.4 Infrastructure Costs

**$0.** This runs on a developer's laptop. No cloud resources, no hosting, no licenses (all tools are free/included with .NET SDK).

---

## 6. Risks & Trade-offs

### 6.1 Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **SVG rendering differences across browsers** | Low | Target Chrome/Edge only for screenshots; both use Chromium |
| **Blazor Server overhead for a static page** | Low | Use static SSR mode; or consider publishing as a static HTML file via `dotnet publish` |
| **JSON schema changes breaking deserialization** | Low | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and nullable properties |
| **Segoe UI font not available on non-Windows** | Low | App is Windows-only per requirements; Segoe UI is preinstalled |

### 6.2 Trade-offs Made

| Decision | Trade-off | Rationale |
|----------|-----------|-----------|
| No component library (MudBlazor, etc.) | Must write custom CSS | Design is pixel-specific; libraries would fight the custom layout |
| No database | Must edit JSON file manually | Simplicity; a JSON file is the easiest "config" format for one-off updates |
| Fixed 1920×1080 viewport | Not usable on mobile/small screens | Purpose-built for screenshots; responsive design adds unnecessary complexity |
| Single project, no layers | No separation of concerns | Appropriate for a <500 line app; over-engineering would slow delivery |
| No JavaScript interop | No client-side interactivity | Dashboard is read-only; server-side rendering is sufficient |

### 6.3 Scalability Considerations

Not applicable. This is a single-user local tool. If it ever needs to serve multiple users, the migration path is:
1. Deploy to IIS/Kestrel on an internal server
2. Add Windows Authentication (one line)
3. Add multiple `data.json` files with a project selector dropdown

---

## 7. Open Questions

| # | Question | Recommended Default | Needs Input From |
|---|----------|-------------------|------------------|
| 1 | How many months should the heatmap display? | 4 months (matching original design) | Product Owner |
| 2 | How many timeline tracks maximum? | 3-5 tracks | Product Owner |
| 3 | Should the "NOW" marker auto-calculate from system date or be specified in data.json? | Auto-calculate from `DateTime.Now` | Developer preference |
| 4 | Should the data.json include the color scheme, or should colors be hardcoded per category? | Hardcoded in CSS; data.json only has content | Design decision |
| 5 | Is Playwright screenshot automation desired, or is manual screenshot sufficient? | Manual for now; automate if used frequently | End user |
| 6 | Should the backlog URL (ADO link) in the header be clickable, or just display text? | Clickable `<a>` tag matching original design | End user |
| 7 | What date range should the timeline span? | Configurable in data.json (startDate, endDate) | Product Owner |

---

## 8. Implementation Recommendations

### 8.1 Phasing

**Phase 1 — Working Dashboard (1-2 days)**
1. `dotnet new blazor -n ReportingDashboard --interactivity Server`
2. Create `data.json` with sample data matching the fictional project
3. Create `DashboardDataService` to read and deserialize JSON
4. Build `Dashboard.razor` as a single page with inline HTML/CSS matching the original design
5. Port the exact CSS from `OriginalDesignConcept.html`
6. Verify at `https://localhost:5001` — screenshot should match the design reference

**Phase 2 — Component Extraction (optional, 1 day)**
1. Extract `DashboardHeader.razor`, `TimelineSection.razor`, `HeatmapSection.razor`
2. Add CSS isolation (`.razor.css` files)
3. Add XML doc comments to the data model

**Phase 3 — Polish (optional, 1 day)**
1. Add Playwright-based screenshot automation script
2. Add a `--watch` mode to auto-refresh when `data.json` changes (use `FileSystemWatcher`)
3. Add a simple print stylesheet

### 8.2 Quick Wins

1. **Copy the original CSS verbatim** from `OriginalDesignConcept.html` into your Blazor app. Modify only what's needed to support data binding. This eliminates CSS debugging.
2. **Start with a single `Dashboard.razor` file** containing everything. Extract components only if the file exceeds ~300 lines.
3. **Use `wwwroot/data.json`** so it's served as a static file AND readable by the service. No path gymnastics.
4. **Hot reload** via `dotnet watch run` during development—instant CSS and Razor changes.

### 8.3 Sample `data.json` Structure

```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project",
    "currentMonth": "Apr"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "m1",
        "label": "M1",
        "description": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
          { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
          { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "highlightMonth": "Apr",
    "categories": [
      {
        "name": "Shipped",
        "cssClass": "ship",
        "items": {
          "Jan": ["DSR v2 Rollout", "Privacy Portal GA"],
          "Feb": ["DSAR Automation", "Consent API v3"],
          "Mar": ["DPO Dashboard", "Audit Log Export"],
          "Apr": ["Retention Policies", "Data Lineage MVP"]
        }
      }
    ]
  }
}
```

### 8.4 Key File Inventory (Final State)

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs                          # DI registration, minimal config
    ├── Services/
    │   └── DashboardDataService.cs         # Reads & caches data.json
    ├── Models/
    │   └── DashboardData.cs                # Records for JSON deserialization
    ├── Components/
    │   ├── App.razor
    │   ├── Layout/
    │   │   └── MainLayout.razor
    │   └── Pages/
    │       └── Dashboard.razor             # The single dashboard page
    │       └── Dashboard.razor.css         # Scoped CSS (ported from original)
    └── wwwroot/
        ├── css/
        │   └── app.css                     # Global reset + custom properties
        └── data.json                       # Dashboard configuration data
```

### 8.5 Critical Implementation Notes

1. **Do not use `@rendermode InteractiveServer` unless interactivity is needed.** Static SSR is faster and avoids WebSocket connections for a read-only page.
2. **The SVG width should be calculated from the container**, not hardcoded to 1560px. Use `@code` to compute milestone X positions based on date math.
3. **The heatmap "highlight" column** (current month with darker background) should be driven by `data.json`'s `highlightMonth` field, not `DateTime.Now`.
4. **CSS Grid `grid-template-rows: 36px repeat(4, 1fr)`** for the heatmap means the header row is fixed-height and data rows stretch equally—preserve this from the original design.
5. **Drop shadow filter on SVG diamonds**: Include the `<defs><filter id="sh">` exactly as in the original HTML. Blazor renders SVG natively; no special handling needed.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/0b31074c7c6e0b8f66a9fb9aba861bcd8b213225/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
