# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-11 17:10 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. It reads project milestone and status data from a local `data.json` file and renders a pixel-perfect dashboard matching the provided HTML design concept — featuring a timeline/Gantt SVG visualization, a color-coded heatmap grid, and a clean header with legend. The output is optimized for screenshotting into PowerPoint decks. **Primary Recommendation:** Build this as a minimal Blazor Server app with zero authentication, no database, and no external services. Use `System.Text.Json` to deserialize `data.json` at startup, render the timeline with inline SVG in Razor components, and implement the heatmap grid with pure CSS Grid. No charting library is needed — the design is simple enough to replicate with raw SVG and CSS, which gives pixel-perfect control for screenshot fidelity. Target a single `.sln` with one project, deployable via `dotnet run` on any developer machine. ---

### Key Findings

- The original HTML design is entirely self-contained: CSS Grid, Flexbox, inline SVG, and the Segoe UI font family. No JavaScript frameworks or charting libraries are used. This is an advantage — the Blazor port can match it exactly.
- The design targets a fixed `1920x1080` viewport (screenshot resolution), eliminating the need for responsive design patterns. The CSS can use fixed pixel dimensions.
- The heatmap grid uses a `160px repeat(4,1fr)` column layout with 4 status rows (Shipped, In Progress, Carryover, Blockers), each color-coded with distinct palettes. This maps directly to a Blazor `@foreach` loop over data categories.
- The timeline is a horizontal SVG with month gridlines, milestone diamonds, checkpoint circles, and a "NOW" marker. This is ~50 lines of SVG and does not warrant a charting library.
- Data volume is trivially small (a few dozen work items, 3-5 milestones, 4 months of columns). No caching, pagination, or performance optimization is needed.
- Blazor Server's SignalR circuit is irrelevant here since there's no interactivity beyond page load. Static Server-Side Rendering (SSR) in .NET 8 is the ideal render mode.
- No authentication, authorization, or data protection is required per the project brief. This dramatically simplifies the architecture. ---
- `dotnet new blazor -n ReportingDashboard.Web --interactivity None`
- Create the `.sln` file and project structure
- Port the original HTML's CSS into `dashboard.css`
- Create a static `Dashboard.razor` page that renders the header and heatmap grid with hardcoded HTML (proving the CSS works in Blazor)
- Define C# record types for `DashboardData`, `TimelineTrack`, `MilestoneMarker`, `HeatmapCategory`
- Create a sample `data.json` with fictional project data
- Implement `DashboardDataService` to read and deserialize the JSON
- Replace hardcoded HTML with `@foreach` loops over the data model
- Implement `Timeline.razor` with SVG generation from milestone data
- Implement `HeatmapGrid.razor` with CSS Grid cells from category data
- Add the "NOW" marker based on `DateTime.Today`
- Verify screenshot fidelity at 1920x1080
- Fine-tune spacing, colors, and font sizes to match original design
- Create `data.sample.json` with documentation comments
- Add `.gitignore` entry for `data.json`
- **Start from the original CSS** — don't rewrite it. Copy the `<style>` block from `OriginalDesignConcept.html` directly into `dashboard.css`. This guarantees visual parity.
- **Use `dotnet new blazor --interactivity None`** — this creates a static SSR Blazor app with zero JavaScript. Perfect for this use case.
- **Use `record` types** — immutable, concise, perfect for read-only data models. Built into C# 12 / .NET 8.
- **Use CSS custom properties** for the color palette — makes it trivial to create variants for different projects.
```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Engineering Platform • Phoenix Workstream • April 2026",
  "backlogUrl": "https://dev.azure.com/org/project",
  "timelineStart": "2026-01-01",
  "timelineEnd": "2026-06-30",
  "currentDate": "2026-04-11",
  "tracks": [
    {
      "id": "m1",
      "label": "M1",
      "description": "API Gateway & Auth",
      "color": "#0078D4",
      "markers": [
        { "date": "2026-01-15", "type": "checkpoint", "label": "Jan 15" },
        { "date": "2026-03-20", "type": "poc", "label": "Mar 20 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "May Prod" }
      ]
    }
  ],
  "months": ["January", "February", "March", "April"],
  "currentMonth": "April",
  "categories": [
    {
      "name": "Shipped",
      "type": "shipped",
      "itemsByMonth": {
        "January": ["API v1 endpoint", "Auth middleware"],
        "February": ["Rate limiting", "Logging pipeline"],
        "March": ["Dashboard MVP", "CI/CD pipeline"],
        "April": ["Load testing", "Docs site"]
      }
    }
  ]
}
```
- **SVG coordinate calculation**: For the timeline, map dates to X positions using:
   ```csharp
   double GetX(DateOnly date, DateOnly start, DateOnly end, double svgWidth)
       => (date.DayNumber - start.DayNumber) / (double)(end.DayNumber - start.DayNumber) * svgWidth;
   ```
- **Current month highlighting**: Compare each column header to `currentMonth` in `data.json` to apply the `.apr` CSS class (renamed to `.current-month` for clarity).
- **Category-to-CSS mapping**: Use a simple dictionary or switch expression:
   ```csharp
   string GetCellClass(string type, bool isCurrent) => type switch
   {
       "shipped" => isCurrent ? "ship-cell apr" : "ship-cell",
       "progress" => isCurrent ? "prog-cell apr" : "prog-cell",
       "carryover" => isCurrent ? "carry-cell apr" : "carry-cell",
       "blocker" => isCurrent ? "block-cell apr" : "block-cell",
       _ => "hm-cell"
   };
   ```
- **No `_Imports.razor` complexity**: Keep it simple — one layout, one page, a few child components. No routing beyond the default `/` route.
- **Launch profile for screenshots**: Configure `launchSettings.json` to open Chrome/Edge at the exact viewport size:
   ```json
   {
     "profiles": {
       "Dashboard": {
         "commandName": "Project",
         "launchBrowser": true,
         "applicationUrl": "http://localhost:5000"
       }
     }
   }
   ``` --- | Phase | Time | |-------|------| | Project scaffolding | 15 min | | CSS port from original HTML | 30 min | | Data model + JSON | 30 min | | Blazor components (header, timeline, heatmap) | 1.5 hr | | Polish and screenshot verification | 30 min | | **Total** | **~3 hours** | This is a deliberately simple project. The biggest risk is over-engineering it. Resist the urge to add component libraries, databases, or interactivity. The original HTML design is already excellent — the Blazor app's job is to make it data-driven.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | Runtime | .NET 8 SDK | 8.0.x (LTS) | Long-term support through Nov 2026 | | Web Framework | Blazor Server (Static SSR) | ASP.NET Core 8.0 | Use `@rendermode` static for zero SignalR overhead | | Template Engine | Razor Components (.razor) | Built-in | Component-per-section architecture | | Component | Technology | Version | Notes | |-----------|-----------|---------|-------| | Data Format | JSON file (`data.json`) | N/A | Flat file, no database | | Deserialization | `System.Text.Json` | 8.0.x (built-in) | Native, fast, no extra dependency | | Data Service | Singleton `IDataService` | N/A | Reads and caches JSON at startup | | Component | Technology | Notes | |-----------|-----------|-------| | Layout | CSS Grid + Flexbox | Matches original design exactly | | Timeline Visualization | Inline SVG in Razor | No charting library needed | | Heatmap Grid | CSS Grid with color-coded cells | Direct port of original CSS | | Font | Segoe UI (system font) | Available on Windows; fallback to Arial | | Icons/Shapes | SVG primitives (`<polygon>`, `<circle>`, `<line>`) | Diamonds, circles, gridlines | | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.Components` | 8.0.x | Blazor framework (included in SDK) | Included | | `System.Text.Json` | 8.0.x | JSON deserialization (included in SDK) | Included | **No additional NuGet packages are needed.** This project requires zero external dependencies beyond the .NET 8 SDK. This is intentional — the design is simple enough that adding charting libraries (Radzen, MudBlazor, Syncfusion) would add complexity without value. | Technology | Reason | |-----------|--------| | MudBlazor / Radzen / Syncfusion | Overkill for a static dashboard with no user interaction. Adds 500KB+ of CSS/JS and component abstraction that fights the pixel-perfect design. | | Blazor WASM | Unnecessary client-side overhead. Server-side static rendering is faster for this use case. | | Entity Framework / SQLite | No database needed. JSON file is sufficient for this data volume. | | SignalR interactivity | No real-time updates needed. Static SSR eliminates the SignalR circuit entirely. | | Any JavaScript charting library (Chart.js, D3) | The SVG timeline is simple enough to render server-side in Razor. Adding JS interop adds complexity for zero benefit. | | Tool | Version | Purpose | |------|---------|---------| | `xunit` | 2.7.x | Unit testing the data model and JSON deserialization | | `bunit` | 1.25.x | Blazor component testing (optional, low priority) | | `Microsoft.NET.Test.Sdk` | 17.9.x | Test runner | Testing is low priority given the project's simplicity, but if added, xUnit is the standard choice for .NET 8. ---
```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard.Web/
│       ├── ReportingDashboard.Web.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css          # All styles from original HTML
│       │   └── data.json                   # Project data file
│       ├── Models/
│       │   ├── DashboardData.cs            # Root model
│       │   ├── Milestone.cs                # Timeline milestones
│       │   ├── HeatmapCategory.cs          # Shipped/InProgress/Carryover/Blocker
│       │   └── WorkItem.cs                 # Individual items in cells
│       ├── Services/
│       │   └── DashboardDataService.cs     # Reads and deserializes data.json
│       ├── Components/
│       │   ├── App.razor                   # Root component
│       │   ├── Layout/
│       │   │   └── DashboardLayout.razor   # Main layout (no nav, single page)
│       │   └── Pages/
│       │       └── Dashboard.razor         # The single page
│       └── Components/
│           ├── Header.razor                # Title, subtitle, legend
│           ├── Timeline.razor              # SVG timeline with milestones
│           ├── HeatmapGrid.razor           # CSS Grid heatmap
│           ├── HeatmapRowHeader.razor      # Category row header
│           └── HeatmapCell.razor           # Individual cell with items
``` Use **Static Server-Side Rendering (SSR)** — .NET 8's default Blazor Server mode with no interactivity:
```csharp
// No @rendermode directive needed — static SSR is the default
// No SignalR circuit, no WebSocket connection, pure HTML response
``` This means the page loads as plain HTML with CSS. No JavaScript is sent to the client. This is ideal for screenshot capture.
```
data.json (wwwroot/) 
    → DashboardDataService (singleton, reads on first request)
        → Dashboard.razor (injects service, passes data to child components)
            → Header.razor (title, subtitle, date, legend)
            → Timeline.razor (renders SVG from milestone data)
            → HeatmapGrid.razor (renders CSS Grid from category data)
```
```csharp
public record DashboardData
{
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string CurrentDate { get; init; }      // "April 2026"
    public string BacklogUrl { get; init; }
    public List<TimelineTrack> Tracks { get; init; }
    public List<string> Months { get; init; }      // Column headers
    public List<HeatmapCategory> Categories { get; init; }
}

public record TimelineTrack
{
    public string Label { get; init; }             // "M1", "M2", "M3"
    public string Description { get; init; }       // "Chatbot & MS Role"
    public string Color { get; init; }             // "#0078D4"
    public List<MilestoneMarker> Markers { get; init; }
}

public record MilestoneMarker
{
    public string Date { get; init; }              // "2026-01-12"
    public string Type { get; init; }              // "checkpoint" | "poc" | "production"
    public string Label { get; init; }             // "Jan 12", "Mar 26 PoC"
}

public record HeatmapCategory
{
    public string Name { get; init; }              // "Shipped", "In Progress", etc.
    public string Type { get; init; }              // "shipped" | "progress" | "carryover" | "blocker"
    public Dictionary<string, List<string>> ItemsByMonth { get; init; }
}
``` Port the original HTML's `<style>` block directly into `dashboard.css` with minimal changes:
- Keep the fixed `1920x1080` viewport constraint on `body`
- Keep the exact color palette, font sizes, and grid definitions
- Use CSS custom properties (variables) for the color palette to enable easy theming if a different project uses different category colors
- Keep all positioning pixel-perfect — the goal is screenshot parity
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
}
``` The timeline is rendered as an inline `<svg>` element within `Timeline.razor`. Key implementation details:
- **Month gridlines**: Calculated by dividing SVG width by number of months
- **Milestone positions**: Convert date to X-coordinate using linear interpolation between start and end dates
- **"NOW" marker**: Red dashed vertical line at today's date position
- **Marker shapes**: `<polygon>` for diamonds (PoC/Production), `<circle>` for checkpoints
- **Track lines**: Horizontal `<line>` elements, one per milestone track
```razor
@* Example: rendering a diamond milestone marker *@
<polygon points="@($"{x},{y-11} {x+11},{y} {x},{y+11} {x-11},{y}")"
         fill="@marker.Color" filter="url(#sh)" />
``` ---

### Considerations & Risks

- **Not applicable.** Per the project brief:
- No authentication or authorization
- No user accounts or role-based access
- No sensitive data (project status is not PII)
- No HTTPS requirement (local only)
- No CSRF/XSS concerns (no user input, no forms) The only "security" consideration: ensure `data.json` is not accidentally committed with real internal project names if the repo is public. Add it to `.gitignore` and provide a `data.sample.json` instead. | Aspect | Recommendation | |--------|---------------| | Hosting | `dotnet run` on local machine | | Port | `http://localhost:5000` (default Kestrel) | | Deployment | No deployment needed — run locally, screenshot, close | | Containerization | Not needed | | CI/CD | Not needed | | Monitoring | Not needed |
```bash
cd src/ReportingDashboard.Web
dotnet run
# Open http://localhost:5000 in browser
# Take screenshot at 1920x1080
# Ctrl+C to stop
``` For even simpler operation, add a `launchSettings.json` that opens the browser automatically. --- | Risk | Impact | Mitigation | |------|--------|------------| | Segoe UI not available on macOS/Linux | Font rendering differs slightly | Fallback to Arial in CSS; primary use is Windows | | SVG timeline date math | Off-by-one in milestone positioning | Use `DateOnly` and simple linear interpolation; visually verify | | CSS Grid browser differences | Slight layout shifts | Target Chrome/Edge for screenshots; both use same Blink engine | | data.json schema drift | Dashboard breaks silently | Validate JSON against model on load; throw clear error | | Concern | Why It's Not a Risk | |---------|-------------------| | Scalability | Single user, single page, trivial data | | Performance | Static HTML render, sub-100ms response | | Security | No auth, no user input, local only | | Data consistency | Read-only from flat file | | Browser compatibility | Screenshots taken in one known browser | | Decision | Trade-off | Justification | |----------|-----------|---------------| | No charting library | Must hand-code SVG | Full control over pixel-perfect output; design is simple enough | | Static SSR only | No real-time updates | Updates happen by editing `data.json` and refreshing; no need for live data | | No database | Must edit JSON manually | Data volume is tiny; JSON is human-readable and version-controllable | | Fixed 1920x1080 | Not responsive | Explicitly designed for screenshot capture at this resolution | | No component library | Must write all CSS manually | Original HTML already has all the CSS; it's a direct port, not a design exercise | --- | # | Question | Recommended Default | Needs Input? | |---|----------|-------------------|-------------| | 1 | How many months should the heatmap show? | 4 (matching original design: Jan–Apr) | No — make configurable in `data.json` | | 2 | Should the "current month" highlighting be automatic or configured? | Automatic based on system date | No | | 3 | Should the timeline span be configurable? | Yes, via `startDate`/`endDate` in `data.json` | No | | 4 | Will multiple projects use this dashboard? | Design for one project per `data.json`; swap files for different projects | Maybe — if yes, add a project selector dropdown later | | 5 | Should the dashboard auto-refresh? | No — manual refresh after editing `data.json` | No | | 6 | What browser will be used for screenshots? | Microsoft Edge (Chromium) at 100% zoom, 1920x1080 | Confirm with user | | 7 | Should `data.json` live in `wwwroot/` or a configurable path? | `wwwroot/` for simplicity; override via `appsettings.json` if needed | No | ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. It reads project milestone and status data from a local `data.json` file and renders a pixel-perfect dashboard matching the provided HTML design concept — featuring a timeline/Gantt SVG visualization, a color-coded heatmap grid, and a clean header with legend. The output is optimized for screenshotting into PowerPoint decks.

**Primary Recommendation:** Build this as a minimal Blazor Server app with zero authentication, no database, and no external services. Use `System.Text.Json` to deserialize `data.json` at startup, render the timeline with inline SVG in Razor components, and implement the heatmap grid with pure CSS Grid. No charting library is needed — the design is simple enough to replicate with raw SVG and CSS, which gives pixel-perfect control for screenshot fidelity. Target a single `.sln` with one project, deployable via `dotnet run` on any developer machine.

---

## 2. Key Findings

- The original HTML design is entirely self-contained: CSS Grid, Flexbox, inline SVG, and the Segoe UI font family. No JavaScript frameworks or charting libraries are used. This is an advantage — the Blazor port can match it exactly.
- The design targets a fixed `1920x1080` viewport (screenshot resolution), eliminating the need for responsive design patterns. The CSS can use fixed pixel dimensions.
- The heatmap grid uses a `160px repeat(4,1fr)` column layout with 4 status rows (Shipped, In Progress, Carryover, Blockers), each color-coded with distinct palettes. This maps directly to a Blazor `@foreach` loop over data categories.
- The timeline is a horizontal SVG with month gridlines, milestone diamonds, checkpoint circles, and a "NOW" marker. This is ~50 lines of SVG and does not warrant a charting library.
- Data volume is trivially small (a few dozen work items, 3-5 milestones, 4 months of columns). No caching, pagination, or performance optimization is needed.
- Blazor Server's SignalR circuit is irrelevant here since there's no interactivity beyond page load. Static Server-Side Rendering (SSR) in .NET 8 is the ideal render mode.
- No authentication, authorization, or data protection is required per the project brief. This dramatically simplifies the architecture.

---

## 3. Recommended Technology Stack

### Runtime & Framework

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| Runtime | .NET 8 SDK | 8.0.x (LTS) | Long-term support through Nov 2026 |
| Web Framework | Blazor Server (Static SSR) | ASP.NET Core 8.0 | Use `@rendermode` static for zero SignalR overhead |
| Template Engine | Razor Components (.razor) | Built-in | Component-per-section architecture |

### Data Layer

| Component | Technology | Version | Notes |
|-----------|-----------|---------|-------|
| Data Format | JSON file (`data.json`) | N/A | Flat file, no database |
| Deserialization | `System.Text.Json` | 8.0.x (built-in) | Native, fast, no extra dependency |
| Data Service | Singleton `IDataService` | N/A | Reads and caches JSON at startup |

### Frontend / UI

| Component | Technology | Notes |
|-----------|-----------|-------|
| Layout | CSS Grid + Flexbox | Matches original design exactly |
| Timeline Visualization | Inline SVG in Razor | No charting library needed |
| Heatmap Grid | CSS Grid with color-coded cells | Direct port of original CSS |
| Font | Segoe UI (system font) | Available on Windows; fallback to Arial |
| Icons/Shapes | SVG primitives (`<polygon>`, `<circle>`, `<line>`) | Diamonds, circles, gridlines |

### Libraries & NuGet Packages

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.Components` | 8.0.x | Blazor framework (included in SDK) | Included |
| `System.Text.Json` | 8.0.x | JSON deserialization (included in SDK) | Included |

**No additional NuGet packages are needed.** This project requires zero external dependencies beyond the .NET 8 SDK. This is intentional — the design is simple enough that adding charting libraries (Radzen, MudBlazor, Syncfusion) would add complexity without value.

### Explicitly NOT Recommended

| Technology | Reason |
|-----------|--------|
| MudBlazor / Radzen / Syncfusion | Overkill for a static dashboard with no user interaction. Adds 500KB+ of CSS/JS and component abstraction that fights the pixel-perfect design. |
| Blazor WASM | Unnecessary client-side overhead. Server-side static rendering is faster for this use case. |
| Entity Framework / SQLite | No database needed. JSON file is sufficient for this data volume. |
| SignalR interactivity | No real-time updates needed. Static SSR eliminates the SignalR circuit entirely. |
| Any JavaScript charting library (Chart.js, D3) | The SVG timeline is simple enough to render server-side in Razor. Adding JS interop adds complexity for zero benefit. |

### Testing

| Tool | Version | Purpose |
|------|---------|---------|
| `xunit` | 2.7.x | Unit testing the data model and JSON deserialization |
| `bunit` | 1.25.x | Blazor component testing (optional, low priority) |
| `Microsoft.NET.Test.Sdk` | 17.9.x | Test runner |

Testing is low priority given the project's simplicity, but if added, xUnit is the standard choice for .NET 8.

---

## 4. Architecture Recommendations

### Project Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard.Web/
│       ├── ReportingDashboard.Web.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css          # All styles from original HTML
│       │   └── data.json                   # Project data file
│       ├── Models/
│       │   ├── DashboardData.cs            # Root model
│       │   ├── Milestone.cs                # Timeline milestones
│       │   ├── HeatmapCategory.cs          # Shipped/InProgress/Carryover/Blocker
│       │   └── WorkItem.cs                 # Individual items in cells
│       ├── Services/
│       │   └── DashboardDataService.cs     # Reads and deserializes data.json
│       ├── Components/
│       │   ├── App.razor                   # Root component
│       │   ├── Layout/
│       │   │   └── DashboardLayout.razor   # Main layout (no nav, single page)
│       │   └── Pages/
│       │       └── Dashboard.razor         # The single page
│       └── Components/
│           ├── Header.razor                # Title, subtitle, legend
│           ├── Timeline.razor              # SVG timeline with milestones
│           ├── HeatmapGrid.razor           # CSS Grid heatmap
│           ├── HeatmapRowHeader.razor      # Category row header
│           └── HeatmapCell.razor           # Individual cell with items
```

### Render Mode

Use **Static Server-Side Rendering (SSR)** — .NET 8's default Blazor Server mode with no interactivity:

```csharp
// No @rendermode directive needed — static SSR is the default
// No SignalR circuit, no WebSocket connection, pure HTML response
```

This means the page loads as plain HTML with CSS. No JavaScript is sent to the client. This is ideal for screenshot capture.

### Data Flow

```
data.json (wwwroot/) 
    → DashboardDataService (singleton, reads on first request)
        → Dashboard.razor (injects service, passes data to child components)
            → Header.razor (title, subtitle, date, legend)
            → Timeline.razor (renders SVG from milestone data)
            → HeatmapGrid.razor (renders CSS Grid from category data)
```

### Data Model Design

```csharp
public record DashboardData
{
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string CurrentDate { get; init; }      // "April 2026"
    public string BacklogUrl { get; init; }
    public List<TimelineTrack> Tracks { get; init; }
    public List<string> Months { get; init; }      // Column headers
    public List<HeatmapCategory> Categories { get; init; }
}

public record TimelineTrack
{
    public string Label { get; init; }             // "M1", "M2", "M3"
    public string Description { get; init; }       // "Chatbot & MS Role"
    public string Color { get; init; }             // "#0078D4"
    public List<MilestoneMarker> Markers { get; init; }
}

public record MilestoneMarker
{
    public string Date { get; init; }              // "2026-01-12"
    public string Type { get; init; }              // "checkpoint" | "poc" | "production"
    public string Label { get; init; }             // "Jan 12", "Mar 26 PoC"
}

public record HeatmapCategory
{
    public string Name { get; init; }              // "Shipped", "In Progress", etc.
    public string Type { get; init; }              // "shipped" | "progress" | "carryover" | "blocker"
    public Dictionary<string, List<string>> ItemsByMonth { get; init; }
}
```

### CSS Architecture

Port the original HTML's `<style>` block directly into `dashboard.css` with minimal changes:

- Keep the fixed `1920x1080` viewport constraint on `body`
- Keep the exact color palette, font sizes, and grid definitions
- Use CSS custom properties (variables) for the color palette to enable easy theming if a different project uses different category colors
- Keep all positioning pixel-perfect — the goal is screenshot parity

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
}
```

### SVG Timeline Rendering

The timeline is rendered as an inline `<svg>` element within `Timeline.razor`. Key implementation details:

1. **Month gridlines**: Calculated by dividing SVG width by number of months
2. **Milestone positions**: Convert date to X-coordinate using linear interpolation between start and end dates
3. **"NOW" marker**: Red dashed vertical line at today's date position
4. **Marker shapes**: `<polygon>` for diamonds (PoC/Production), `<circle>` for checkpoints
5. **Track lines**: Horizontal `<line>` elements, one per milestone track

```razor
@* Example: rendering a diamond milestone marker *@
<polygon points="@($"{x},{y-11} {x+11},{y} {x},{y+11} {x-11},{y}")"
         fill="@marker.Color" filter="url(#sh)" />
```

---

## 5. Security & Infrastructure

### Security

**Not applicable.** Per the project brief:
- No authentication or authorization
- No user accounts or role-based access
- No sensitive data (project status is not PII)
- No HTTPS requirement (local only)
- No CSRF/XSS concerns (no user input, no forms)

The only "security" consideration: ensure `data.json` is not accidentally committed with real internal project names if the repo is public. Add it to `.gitignore` and provide a `data.sample.json` instead.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| Hosting | `dotnet run` on local machine |
| Port | `http://localhost:5000` (default Kestrel) |
| Deployment | No deployment needed — run locally, screenshot, close |
| Containerization | Not needed |
| CI/CD | Not needed |
| Monitoring | Not needed |

### Running the Dashboard

```bash
cd src/ReportingDashboard.Web
dotnet run
# Open http://localhost:5000 in browser
# Take screenshot at 1920x1080
# Ctrl+C to stop
```

For even simpler operation, add a `launchSettings.json` that opens the browser automatically.

---

## 6. Risks & Trade-offs

### Low Risks (Manageable)

| Risk | Impact | Mitigation |
|------|--------|------------|
| Segoe UI not available on macOS/Linux | Font rendering differs slightly | Fallback to Arial in CSS; primary use is Windows |
| SVG timeline date math | Off-by-one in milestone positioning | Use `DateOnly` and simple linear interpolation; visually verify |
| CSS Grid browser differences | Slight layout shifts | Target Chrome/Edge for screenshots; both use same Blink engine |
| data.json schema drift | Dashboard breaks silently | Validate JSON against model on load; throw clear error |

### Non-Risks (By Design)

| Concern | Why It's Not a Risk |
|---------|-------------------|
| Scalability | Single user, single page, trivial data |
| Performance | Static HTML render, sub-100ms response |
| Security | No auth, no user input, local only |
| Data consistency | Read-only from flat file |
| Browser compatibility | Screenshots taken in one known browser |

### Trade-offs Made

| Decision | Trade-off | Justification |
|----------|-----------|---------------|
| No charting library | Must hand-code SVG | Full control over pixel-perfect output; design is simple enough |
| Static SSR only | No real-time updates | Updates happen by editing `data.json` and refreshing; no need for live data |
| No database | Must edit JSON manually | Data volume is tiny; JSON is human-readable and version-controllable |
| Fixed 1920x1080 | Not responsive | Explicitly designed for screenshot capture at this resolution |
| No component library | Must write all CSS manually | Original HTML already has all the CSS; it's a direct port, not a design exercise |

---

## 7. Open Questions

| # | Question | Recommended Default | Needs Input? |
|---|----------|-------------------|-------------|
| 1 | How many months should the heatmap show? | 4 (matching original design: Jan–Apr) | No — make configurable in `data.json` |
| 2 | Should the "current month" highlighting be automatic or configured? | Automatic based on system date | No |
| 3 | Should the timeline span be configurable? | Yes, via `startDate`/`endDate` in `data.json` | No |
| 4 | Will multiple projects use this dashboard? | Design for one project per `data.json`; swap files for different projects | Maybe — if yes, add a project selector dropdown later |
| 5 | Should the dashboard auto-refresh? | No — manual refresh after editing `data.json` | No |
| 6 | What browser will be used for screenshots? | Microsoft Edge (Chromium) at 100% zoom, 1920x1080 | Confirm with user |
| 7 | Should `data.json` live in `wwwroot/` or a configurable path? | `wwwroot/` for simplicity; override via `appsettings.json` if needed | No |

---

## 8. Implementation Recommendations

### Phasing

**Phase 1: Skeleton (1-2 hours)**
1. `dotnet new blazor -n ReportingDashboard.Web --interactivity None`
2. Create the `.sln` file and project structure
3. Port the original HTML's CSS into `dashboard.css`
4. Create a static `Dashboard.razor` page that renders the header and heatmap grid with hardcoded HTML (proving the CSS works in Blazor)

**Phase 2: Data Model (30 minutes)**
1. Define C# record types for `DashboardData`, `TimelineTrack`, `MilestoneMarker`, `HeatmapCategory`
2. Create a sample `data.json` with fictional project data
3. Implement `DashboardDataService` to read and deserialize the JSON

**Phase 3: Dynamic Rendering (1-2 hours)**
1. Replace hardcoded HTML with `@foreach` loops over the data model
2. Implement `Timeline.razor` with SVG generation from milestone data
3. Implement `HeatmapGrid.razor` with CSS Grid cells from category data
4. Add the "NOW" marker based on `DateTime.Today`

**Phase 4: Polish (30 minutes)**
1. Verify screenshot fidelity at 1920x1080
2. Fine-tune spacing, colors, and font sizes to match original design
3. Create `data.sample.json` with documentation comments
4. Add `.gitignore` entry for `data.json`

### Quick Wins

1. **Start from the original CSS** — don't rewrite it. Copy the `<style>` block from `OriginalDesignConcept.html` directly into `dashboard.css`. This guarantees visual parity.
2. **Use `dotnet new blazor --interactivity None`** — this creates a static SSR Blazor app with zero JavaScript. Perfect for this use case.
3. **Use `record` types** — immutable, concise, perfect for read-only data models. Built into C# 12 / .NET 8.
4. **Use CSS custom properties** for the color palette — makes it trivial to create variants for different projects.

### Sample `data.json` Structure

```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Engineering Platform • Phoenix Workstream • April 2026",
  "backlogUrl": "https://dev.azure.com/org/project",
  "timelineStart": "2026-01-01",
  "timelineEnd": "2026-06-30",
  "currentDate": "2026-04-11",
  "tracks": [
    {
      "id": "m1",
      "label": "M1",
      "description": "API Gateway & Auth",
      "color": "#0078D4",
      "markers": [
        { "date": "2026-01-15", "type": "checkpoint", "label": "Jan 15" },
        { "date": "2026-03-20", "type": "poc", "label": "Mar 20 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "May Prod" }
      ]
    }
  ],
  "months": ["January", "February", "March", "April"],
  "currentMonth": "April",
  "categories": [
    {
      "name": "Shipped",
      "type": "shipped",
      "itemsByMonth": {
        "January": ["API v1 endpoint", "Auth middleware"],
        "February": ["Rate limiting", "Logging pipeline"],
        "March": ["Dashboard MVP", "CI/CD pipeline"],
        "April": ["Load testing", "Docs site"]
      }
    }
  ]
}
```

### Key Implementation Notes

1. **SVG coordinate calculation**: For the timeline, map dates to X positions using:
   ```csharp
   double GetX(DateOnly date, DateOnly start, DateOnly end, double svgWidth)
       => (date.DayNumber - start.DayNumber) / (double)(end.DayNumber - start.DayNumber) * svgWidth;
   ```

2. **Current month highlighting**: Compare each column header to `currentMonth` in `data.json` to apply the `.apr` CSS class (renamed to `.current-month` for clarity).

3. **Category-to-CSS mapping**: Use a simple dictionary or switch expression:
   ```csharp
   string GetCellClass(string type, bool isCurrent) => type switch
   {
       "shipped" => isCurrent ? "ship-cell apr" : "ship-cell",
       "progress" => isCurrent ? "prog-cell apr" : "prog-cell",
       "carryover" => isCurrent ? "carry-cell apr" : "carry-cell",
       "blocker" => isCurrent ? "block-cell apr" : "block-cell",
       _ => "hm-cell"
   };
   ```

4. **No `_Imports.razor` complexity**: Keep it simple — one layout, one page, a few child components. No routing beyond the default `/` route.

5. **Launch profile for screenshots**: Configure `launchSettings.json` to open Chrome/Edge at the exact viewport size:
   ```json
   {
     "profiles": {
       "Dashboard": {
         "commandName": "Project",
         "launchBrowser": true,
         "applicationUrl": "http://localhost:5000"
       }
     }
   }
   ```

---

### Total Estimated Effort

| Phase | Time |
|-------|------|
| Project scaffolding | 15 min |
| CSS port from original HTML | 30 min |
| Data model + JSON | 30 min |
| Blazor components (header, timeline, heatmap) | 1.5 hr |
| Polish and screenshot verification | 30 min |
| **Total** | **~3 hours** |

This is a deliberately simple project. The biggest risk is over-engineering it. Resist the urge to add component libraries, databases, or interactivity. The original HTML design is already excellent — the Blazor app's job is to make it data-driven.

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
