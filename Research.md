# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-17 03:36 UTC_

### Summary

This project is a **single-page executive reporting dashboard** that visualizes project milestones, progress status, and monthly heatmaps. The design reference (`OriginalDesignConcept.html`) defines a fixed 1920×1080 layout optimized for PowerPoint screenshot capture, featuring an SVG timeline with milestone diamonds/circles, and a CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) across monthly columns. **Primary Recommendation:** Build this as a minimal Blazor Server application with zero authentication, reading all data from a local `data.json` file deserialized via `System.Text.Json`. Use raw Blazor components with inline SVG for the timeline and CSS Grid for the heatmap — no charting library needed. The design is simple enough that third-party UI component suites would add unnecessary complexity. Target a single `dotnet run` experience with no database, no Docker, and no external dependencies beyond the .NET 8 SDK. --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.Components` | 8.0.x (SDK-included) | Blazor framework | ✅ Included in SDK | | `System.Text.Json` | 8.0.x (SDK-included) | JSON deserialization | ✅ Included in SDK | | No additional packages | — | — | — | This is intentional. The project's scope does not justify any third-party packages. Everything needed — Blazor components, JSON parsing, file I/O, Kestrel hosting — ships with the .NET 8 SDK. The HTML reference defines these key CSS patterns that must be preserved: | Pattern | CSS Property | Value | Used For | |---------|-------------|-------|----------| | Fixed viewport | `body { width: 1920px; height: 1080px; overflow: hidden; }` | Fixed dimensions | Screenshot-ready layout | | Vertical flex | `body { display: flex; flex-direction: column; }` | Top-to-bottom sections | Page structure | | Timeline area | `.tl-area { height: 196px; }` | Fixed height | Consistent SVG canvas | | Heatmap grid | `grid-template-columns: 160px repeat(4, 1fr)` | Row headers + data columns | Status matrix | | Status row colors | `.ship-cell { background: #F0FBF0; }` etc. | Category-specific backgrounds | Visual grouping | | Current month highlight | `.apr { background: #D8F2DA; }` etc. | Darker shade for current month | Temporal emphasis | | Bullet indicators | `.it::before { width: 6px; height: 6px; border-radius: 50%; }` | Colored dots | Item markers | | Milestone diamonds | `<polygon points="..." fill="#F4B400">` | SVG rotated squares | PoC milestones | | Drop shadows | `<filter id="sh"><feDropShadow .../>` | SVG filter | Milestone emphasis | **Recommendation:** Start with the exact CSS from the HTML reference in a single `dashboard.css` file. Only refactor into `.razor.css` component-scoped files if the single file becomes unwieldy (unlikely at this scale).
```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "currentMonth": "Apr"
  },
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
      },
      {
        "id": "M2",
        "label": "PDS & Data Inventory",
        "color": "#00897B",
        "milestones": [
          { "date": "2025-12-19", "type": "checkpoint", "label": "Dec 19" },
          { "date": "2026-02-11", "type": "checkpoint", "label": "Feb 11" },
          { "date": "2026-03-15", "type": "poc", "label": "Mar 15 PoC" }
        ]
      }
    ]
  },
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "shipped": {
    "Jan": ["Graph API v2 Launch", "DSAR Portal Redesign"],
    "Feb": ["PDS Connector", "Consent SDK 3.0"],
    "Mar": ["Auto-classification ML model", "DFD Generator v1"],
    "Apr": ["MS Role Integration"]
  },
  "inProgress": {
    "Jan": [],
    "Feb": ["Data Inventory Scanner"],
    "Mar": ["Chatbot NLU Training"],
    "Apr": ["Review Automation", "PDS Dashboard"]
  },
  "carryover": {
    "Jan": [],
    "Feb": [],
    "Mar": ["Legacy API Migration"],
    "Apr": ["Consent Propagation"]
  },
  "blockers": {
    "Jan": [],
    "Feb": [],
    "Mar": [],
    "Apr": ["Dependency on Platform Team SDK release"]
  }
}
```

### Key Findings

- The original HTML design is entirely achievable with **pure CSS Grid + Flexbox + inline SVG** — no JavaScript charting library is required.
- Blazor Server is well-suited here because the app is local-only, so the SignalR latency concern is irrelevant (localhost round-trip is sub-millisecond).
- A `data.json` file is the correct data strategy — no database is warranted for this scope. `System.Text.Json` (built into .NET 8) handles deserialization with zero additional packages.
- The 1920×1080 fixed-width design means **responsive design is not needed** — the page is purpose-built for screenshot capture at a specific resolution.
- SVG generation for the timeline/Gantt milestones should be done **server-side in Razor markup**, not via a JS charting library, to keep the stack pure C#/Blazor.
- The color palette, font (Segoe UI), and layout metrics are fully specified in the HTML reference and can be directly translated to Blazor component CSS.
- `IFileSystemWatcher` or simple polling can enable **live reload of `data.json`** so the user can edit data and see changes without restarting the app.
- The entire solution can be a **single Blazor Server project** — no need for separate class libraries, API projects, or shared projects given the scope.
- Hot Reload (`dotnet watch`) works well with Blazor Server in .NET 8, enabling rapid iteration on layout and styling. --- **Goal:** Pixel-perfect reproduction of `OriginalDesignConcept.html` as a Blazor Server app with hardcoded data.
- `dotnet new blazor -n ReportingDashboard --interactivity Server`
- Delete all scaffolded pages except the root layout.
- Create `Dashboard.razor` as the single page at route `/`.
- Copy all CSS from the HTML reference into `wwwroot/css/dashboard.css`.
- Port the HTML structure into Razor components: `Header.razor`, `TimelineSection.razor`, `HeatmapGrid.razor`.
- Hardcode all data inline (copy text content from the HTML reference).
- **Validate:** Screenshot at 1920×1080 and compare side-by-side with the design reference. **Deliverable:** A running Blazor app that produces an identical screenshot to the HTML reference. **Goal:** Replace hardcoded data with `data.json`.
- Define `DashboardData` record types matching the JSON schema.
- Create a sample `data.json` with fictional project data.
- Build `DashboardDataService` to read and deserialize the file.
- Update Razor components to bind to the data model.
- Parameterize SVG coordinates (milestone positions calculated from dates).
- Parameterize CSS Grid columns (month count from data).
- **Validate:** Change `data.json`, restart, verify the dashboard updates. **Deliverable:** A data-driven dashboard that renders any project from a JSON file. **Goal:** Quality-of-life improvements.
- Add `FileSystemWatcher` for live reload of `data.json` (no restart needed).
- Add a print-friendly CSS `@media print` stylesheet.
- Add graceful error handling if `data.json` is missing or malformed (show a helpful error message, not a stack trace).
- Add a `/screenshot` route variant with zero margin/padding for cleaner captures.
- Create 2–3 sample `data.json` files for different fictional projects to demonstrate flexibility.
- **Immediate:** The HTML reference can be dropped directly into `wwwroot/` as a static file and served alongside Blazor, giving an instant working baseline to compare against.
- **Immediate:** `dotnet watch` gives sub-second CSS iteration — no build step for style tweaks.
- **High value:** The SVG timeline is the most complex visual element. Port it first, then the heatmap grid (which is straightforward CSS Grid).
- ❌ User authentication or role-based access
- ❌ A database or ORM layer
- ❌ An admin UI for editing data
- ❌ Responsive/mobile layouts
- ❌ Dark mode
- ❌ API controllers or REST endpoints
- ❌ Docker containerization
- ❌ CI/CD pipelines
- ❌ Logging infrastructure (beyond `Console.WriteLine` for debugging)
- ❌ Multiple pages or navigation ---

### Recommended Tools & Technologies

- **Project:** Executive Project Reporting Dashboard **Date:** April 17, 2026 **Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure --- | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Framework** | Blazor Server (.NET 8) | `net8.0` | Built-in, no additional packages. Use `@rendermode InteractiveServer`. | | **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Directly port the grid from the HTML reference: `grid-template-columns: 160px repeat(4, 1fr)`. No CSS framework needed. | | **Timeline/Gantt** | Inline SVG in Razor | N/A | Generate `<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` elements directly in `.razor` files with C# `@foreach` loops over milestone data. | | **Icons** | None required | — | The design uses simple CSS shapes (rotated squares for diamonds, circles for checkpoints). | | **CSS Isolation** | Blazor CSS Isolation (`.razor.css`) | Built-in | Scope styles per component to avoid conflicts. | | **Font** | Segoe UI (system font) | N/A | Available on all Windows machines. Fallback: `Arial, sans-serif`. | **Why no charting library?** Libraries like Radzen Charts, MudBlazor Charts, or Blazorise Charts are designed for dynamic data visualization (bar charts, line charts, pie charts). The timeline in this design is a **custom Gantt-style SVG** with positioned diamonds, circles, and horizontal lines — none of these libraries support this layout natively. Hand-crafted SVG in Razor is simpler, more maintainable, and pixel-perfect to the design. | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Use `JsonSerializer.Deserialize<DashboardData>()`. Source generators available for AOT but unnecessary here. | | **File Reading** | `System.IO.File.ReadAllTextAsync()` | Built-in | Read `data.json` from the app's content root. | | **File Watching** | `FileSystemWatcher` | Built-in (`System.IO`) | Optional: watch `data.json` for changes, trigger UI refresh via `InvokeAsync(StateHasChanged)`. | | **Data Models** | C# Records | Built-in | Use `record` types for immutable data models. Clean, concise, built-in equality. | | **Configuration** | `appsettings.json` for app config, `data.json` for dashboard data | Built-in | Separate concerns: app settings vs. display data. | **None.** A database is architectural overkill for this project. The `data.json` file serves as both the data store and the configuration. The user manually edits this file (or it's generated by another tool) and the dashboard renders it. This is the correct choice for a screenshot-oriented reporting tool. | Component | Recommendation | Notes | |-----------|---------------|-------| | **Runtime** | .NET 8 SDK | `dotnet run` from command line. | | **Hosting** | Kestrel (built-in) | Blazor Server's default. Runs on `https://localhost:5001` or configured port. | | **Deployment** | `dotnet publish` → xcopy deploy | Single folder output. No containers, no IIS needed for local use. | | **Hot Reload** | `dotnet watch run` | Reflects CSS and Razor changes without restart. | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Unit Testing** | xUnit | `2.7.0+` | .NET ecosystem standard. Test data model deserialization and SVG coordinate calculations. | | **Component Testing** | bUnit | `1.25+` | Blazor-specific component testing. Verify rendered HTML structure matches expected grid/SVG output. | | **Snapshot Testing** | Verify | `23.0+` | Optional: snapshot test the rendered HTML to catch visual regressions. | > **Honest assessment:** For a project this simple, you may skip formal testing entirely. The primary validation is visual — does the screenshot look right? Manual verification against the design reference is likely sufficient for MVP.
```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj        # Blazor Server, net8.0
│       ├── Program.cs                        # Minimal hosting setup
│       ├── Components/
│       │   ├── App.razor                     # Root component
│       │   ├── Pages/
│       │   │   └── Dashboard.razor           # Single page, route "/"
│       │   ├── Layout/
│       │   │   └── MainLayout.razor          # Minimal layout (no nav)
│       │   └── Shared/
│       │       ├── TimelineSection.razor      # SVG timeline + milestones
│       │       ├── HeatmapGrid.razor          # CSS Grid status matrix
│       │       ├── HeatmapRow.razor           # Single status row
│       │       └── Header.razor               # Title, subtitle, legend
│       ├── Models/
│       │   └── DashboardData.cs              # Record types for data.json
│       ├── Services/
│       │   └── DashboardDataService.cs       # Reads/watches data.json
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css             # Global styles ported from HTML reference
│       │   └── data.json                     # Dashboard data file
│       └── Properties/
│           └── launchSettings.json
└── tests/                                     # Optional
    └── ReportingDashboard.Tests/
        └── ReportingDashboard.Tests.csproj
``` --- This project does not warrant MVVM, Clean Architecture, CQRS, or any layered pattern. Use the simplest architecture that works:
- **`DashboardDataService`** — A singleton service registered in DI that reads `data.json`, deserializes it, and exposes the typed model. Optionally watches the file for changes.
- **Razor Components** — Directly inject `DashboardDataService`, read the model, and render HTML/SVG. No ViewModels, no mediators, no repositories.
```csharp
// Program.cs — entire setup
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```
```csharp
public record DashboardData(
    ProjectInfo Project,
    List<Milestone> Milestones,
    List<MonthColumn> Months,
    StatusSection Shipped,
    StatusSection InProgress,
    StatusSection Carryover,
    StatusSection Blockers
);

public record ProjectInfo(string Title, string Subtitle, string BacklogUrl, string CurrentMonth);
public record Milestone(string Id, string Label, string Date, string Type, string TrackId, double PositionPercent);
public record MonthColumn(string Name, bool IsCurrentMonth);
public record StatusSection(List<MonthItems> MonthData);
public record MonthItems(string Month, List<string> Items);
``` The original HTML hardcodes SVG coordinates. For a data-driven approach:
- Define the SVG viewBox as `0 0 1560 185` (matching the original).
- Each milestone track (M1, M2, M3) maps to a fixed Y coordinate (42, 98, 154).
- Each milestone's X coordinate is calculated from its date relative to the timeline range: `x = (date - startDate) / (endDate - startDate) * viewBoxWidth`.
- Render month gridlines at evenly spaced X intervals.
- The "NOW" line is calculated from today's date. This logic lives in a simple C# helper method, not a service — it's pure coordinate math. Directly port the CSS from the HTML reference:
```css
.hm-grid {
    display: grid;
    grid-template-columns: 160px repeat(4, 1fr);  /* row header + N months */
    grid-template-rows: 36px repeat(4, 1fr);       /* column header + 4 status rows */
}
``` The number of month columns should be driven by `data.json` — use inline `grid-template-columns` style computed in Blazor:
```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Data.Months.Count, 1fr);">
```
```
data.json (on disk)
    ↓  File.ReadAllTextAsync
DashboardDataService (singleton, DI)
    ↓  Inject
Dashboard.razor (page)
    ↓  Pass props
Header.razor ← ProjectInfo
TimelineSection.razor ← Milestones[], Months[]
HeatmapGrid.razor ← Shipped, InProgress, Carryover, Blockers
``` No API controllers, no HTTP calls, no SignalR custom hubs. Blazor Server's built-in circuit handles everything. ---

### Considerations & Risks

- **None.** This is explicitly out of scope. The app runs on localhost for a single user generating screenshots. Do not add authentication middleware, Identity, or any auth packages. If this ever needs to be shared on a network, the simplest addition would be a reverse proxy with basic auth — but defer this entirely.
- `data.json` contains project status information, not PII or secrets. No encryption needed.
- If sensitive project names are a concern, the file lives on the user's local machine only. | Scenario | Approach | |----------|----------| | **Development** | `dotnet watch run` — hot reload, auto-browser-open. | | **"Production" (local)** | `dotnet run --configuration Release` or `dotnet publish -c Release -o ./publish` then run the executable directly. | | **Sharing with team** | Copy the publish folder to a shared drive. Recipient runs the `.exe`. Kestrel binds to localhost by default. | **$0.** Everything runs locally. No cloud, no containers, no CI/CD needed for this scope. The page is designed at 1920×1080 for direct screenshot capture. Recommended workflow:
- Open `https://localhost:5001` in Edge/Chrome.
- Press `F11` for fullscreen.
- Use `Win+Shift+S` (Snipping Tool) or browser DevTools device emulation at 1920×1080.
- Paste into PowerPoint. **Pro tip:** Add a `@media print` CSS block that hides browser chrome, or create a dedicated `/screenshot` route with zero padding for pixel-perfect captures. --- | Risk | Likelihood | Impact | Mitigation | |------|-----------|--------|------------| | **Blazor Server is overkill for a static page** | Medium | Low | Accepted trade-off. The project mandates Blazor Server. The overhead is minimal — a single SignalR connection for one user. Alternative would be a static HTML generator, but that's outside the mandated stack. | | **SVG coordinate math produces misaligned elements** | Medium | Medium | Port exact pixel values from the HTML reference first, then parameterize. Test at 1920×1080 in Chrome. | | **data.json schema changes break the UI silently** | Low | Medium | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and add null checks in Razor. Optionally validate on load and show a clear error banner. | | **Hot Reload doesn't catch CSS changes in .razor.css** | Low | Low | Known .NET 8 limitation for some CSS isolation scenarios. Workaround: use a global `dashboard.css` file in `wwwroot/css/` which always hot-reloads. | | **Font rendering differs across machines** | Low | Low | Segoe UI is pre-installed on all Windows 10/11 machines. For non-Windows, fall back to system sans-serif. Screenshots will always be taken on Windows. |
- **No database** → Cannot query historical data or trend over time. Accepted: the user edits `data.json` monthly.
- **No auth** → Anyone on the local network could access if Kestrel binds to `0.0.0.0`. Mitigation: default binding is `localhost` only.
- **No component library (MudBlazor, Radzen, etc.)** → Must hand-code all CSS. Accepted: the design is fixed and specific. A component library would fight the custom layout rather than help.
- **Fixed 1920×1080 layout** → Not usable on mobile or smaller screens. Accepted: this is a screenshot tool, not a responsive web app.
- **Blazor familiarity:** Any C# developer can work in Blazor. The Razor syntax is straightforward.
- **SVG knowledge:** The timeline requires understanding SVG coordinate systems, `<line>`, `<polygon>`, `<circle>`, and `<text>` elements. This is the steepest learning curve, but the HTML reference provides a complete example to port from.
- **CSS Grid:** Modern and well-documented. The HTML reference provides the exact grid definition. --- | # | Question | Recommendation | Stakeholder | |---|----------|---------------|-------------| | 1 | **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable via `data.json`? | Yes — make the month list dynamic in `data.json`. The CSS Grid column count adapts automatically. | Product Owner | | 2 | **Should the timeline scale to a fixed range (e.g., Jan–Jun) or auto-scale to the data?** | Fixed range defined in `data.json` (e.g., `timelineStart: "2026-01-01"`, `timelineEnd: "2026-06-30"`). Auto-scaling adds complexity for minimal benefit. | Product Owner | | 3 | **Should `data.json` be editable through the UI, or only via text editor?** | Text editor only for MVP. A JSON editor UI would triple the scope. | Project Lead | | 4 | **Multiple projects or single project per instance?** | Single project per `data.json`. For multiple projects, run multiple instances or switch JSON files. | Product Owner | | 5 | **Should the "NOW" line auto-calculate from system date, or be set in `data.json`?** | Auto-calculate from `DateTime.Now` by default, with an optional override in `data.json` for generating screenshots for specific dates. | Engineer | | 6 | **Will the ADO Backlog link in the header be functional, or just display text?** | Make it a real `<a href>` — the URL comes from `data.json`. Zero implementation cost. | Product Owner | | 7 | **Dark mode or light mode only?** The design reference is light mode. | Light mode only. Dark mode doubles CSS effort for a screenshot tool. | Product Owner | ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Project Reporting Dashboard
**Date:** April 17, 2026
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure

---

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** that visualizes project milestones, progress status, and monthly heatmaps. The design reference (`OriginalDesignConcept.html`) defines a fixed 1920×1080 layout optimized for PowerPoint screenshot capture, featuring an SVG timeline with milestone diamonds/circles, and a CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) across monthly columns.

**Primary Recommendation:** Build this as a minimal Blazor Server application with zero authentication, reading all data from a local `data.json` file deserialized via `System.Text.Json`. Use raw Blazor components with inline SVG for the timeline and CSS Grid for the heatmap — no charting library needed. The design is simple enough that third-party UI component suites would add unnecessary complexity. Target a single `dotnet run` experience with no database, no Docker, and no external dependencies beyond the .NET 8 SDK.

---

## 2. Key Findings

- The original HTML design is entirely achievable with **pure CSS Grid + Flexbox + inline SVG** — no JavaScript charting library is required.
- Blazor Server is well-suited here because the app is local-only, so the SignalR latency concern is irrelevant (localhost round-trip is sub-millisecond).
- A `data.json` file is the correct data strategy — no database is warranted for this scope. `System.Text.Json` (built into .NET 8) handles deserialization with zero additional packages.
- The 1920×1080 fixed-width design means **responsive design is not needed** — the page is purpose-built for screenshot capture at a specific resolution.
- SVG generation for the timeline/Gantt milestones should be done **server-side in Razor markup**, not via a JS charting library, to keep the stack pure C#/Blazor.
- The color palette, font (Segoe UI), and layout metrics are fully specified in the HTML reference and can be directly translated to Blazor component CSS.
- `IFileSystemWatcher` or simple polling can enable **live reload of `data.json`** so the user can edit data and see changes without restarting the app.
- The entire solution can be a **single Blazor Server project** — no need for separate class libraries, API projects, or shared projects given the scope.
- Hot Reload (`dotnet watch`) works well with Blazor Server in .NET 8, enabling rapid iteration on layout and styling.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server UI)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Framework** | Blazor Server (.NET 8) | `net8.0` | Built-in, no additional packages. Use `@rendermode InteractiveServer`. |
| **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Directly port the grid from the HTML reference: `grid-template-columns: 160px repeat(4, 1fr)`. No CSS framework needed. |
| **Timeline/Gantt** | Inline SVG in Razor | N/A | Generate `<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` elements directly in `.razor` files with C# `@foreach` loops over milestone data. |
| **Icons** | None required | — | The design uses simple CSS shapes (rotated squares for diamonds, circles for checkpoints). |
| **CSS Isolation** | Blazor CSS Isolation (`.razor.css`) | Built-in | Scope styles per component to avoid conflicts. |
| **Font** | Segoe UI (system font) | N/A | Available on all Windows machines. Fallback: `Arial, sans-serif`. |

**Why no charting library?** Libraries like Radzen Charts, MudBlazor Charts, or Blazorise Charts are designed for dynamic data visualization (bar charts, line charts, pie charts). The timeline in this design is a **custom Gantt-style SVG** with positioned diamonds, circles, and horizontal lines — none of these libraries support this layout natively. Hand-crafted SVG in Razor is simpler, more maintainable, and pixel-perfect to the design.

### Backend (Data Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Use `JsonSerializer.Deserialize<DashboardData>()`. Source generators available for AOT but unnecessary here. |
| **File Reading** | `System.IO.File.ReadAllTextAsync()` | Built-in | Read `data.json` from the app's content root. |
| **File Watching** | `FileSystemWatcher` | Built-in (`System.IO`) | Optional: watch `data.json` for changes, trigger UI refresh via `InvokeAsync(StateHasChanged)`. |
| **Data Models** | C# Records | Built-in | Use `record` types for immutable data models. Clean, concise, built-in equality. |
| **Configuration** | `appsettings.json` for app config, `data.json` for dashboard data | Built-in | Separate concerns: app settings vs. display data. |

### Database

**None.** A database is architectural overkill for this project. The `data.json` file serves as both the data store and the configuration. The user manually edits this file (or it's generated by another tool) and the dashboard renders it. This is the correct choice for a screenshot-oriented reporting tool.

### Infrastructure

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **Runtime** | .NET 8 SDK | `dotnet run` from command line. |
| **Hosting** | Kestrel (built-in) | Blazor Server's default. Runs on `https://localhost:5001` or configured port. |
| **Deployment** | `dotnet publish` → xcopy deploy | Single folder output. No containers, no IIS needed for local use. |
| **Hot Reload** | `dotnet watch run` | Reflects CSS and Razor changes without restart. |

### Testing

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | `2.7.0+` | .NET ecosystem standard. Test data model deserialization and SVG coordinate calculations. |
| **Component Testing** | bUnit | `1.25+` | Blazor-specific component testing. Verify rendered HTML structure matches expected grid/SVG output. |
| **Snapshot Testing** | Verify | `23.0+` | Optional: snapshot test the rendered HTML to catch visual regressions. |

> **Honest assessment:** For a project this simple, you may skip formal testing entirely. The primary validation is visual — does the screenshot look right? Manual verification against the design reference is likely sufficient for MVP.

### Project Structure

```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj        # Blazor Server, net8.0
│       ├── Program.cs                        # Minimal hosting setup
│       ├── Components/
│       │   ├── App.razor                     # Root component
│       │   ├── Pages/
│       │   │   └── Dashboard.razor           # Single page, route "/"
│       │   ├── Layout/
│       │   │   └── MainLayout.razor          # Minimal layout (no nav)
│       │   └── Shared/
│       │       ├── TimelineSection.razor      # SVG timeline + milestones
│       │       ├── HeatmapGrid.razor          # CSS Grid status matrix
│       │       ├── HeatmapRow.razor           # Single status row
│       │       └── Header.razor               # Title, subtitle, legend
│       ├── Models/
│       │   └── DashboardData.cs              # Record types for data.json
│       ├── Services/
│       │   └── DashboardDataService.cs       # Reads/watches data.json
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css             # Global styles ported from HTML reference
│       │   └── data.json                     # Dashboard data file
│       └── Properties/
│           └── launchSettings.json
└── tests/                                     # Optional
    └── ReportingDashboard.Tests/
        └── ReportingDashboard.Tests.csproj
```

---

## 4. Architecture Recommendations

### Pattern: Simple Service + Razor Components

This project does not warrant MVVM, Clean Architecture, CQRS, or any layered pattern. Use the simplest architecture that works:

1. **`DashboardDataService`** — A singleton service registered in DI that reads `data.json`, deserializes it, and exposes the typed model. Optionally watches the file for changes.
2. **Razor Components** — Directly inject `DashboardDataService`, read the model, and render HTML/SVG. No ViewModels, no mediators, no repositories.

```csharp
// Program.cs — entire setup
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

### Data Model Design (for `data.json`)

```csharp
public record DashboardData(
    ProjectInfo Project,
    List<Milestone> Milestones,
    List<MonthColumn> Months,
    StatusSection Shipped,
    StatusSection InProgress,
    StatusSection Carryover,
    StatusSection Blockers
);

public record ProjectInfo(string Title, string Subtitle, string BacklogUrl, string CurrentMonth);
public record Milestone(string Id, string Label, string Date, string Type, string TrackId, double PositionPercent);
public record MonthColumn(string Name, bool IsCurrentMonth);
public record StatusSection(List<MonthItems> MonthData);
public record MonthItems(string Month, List<string> Items);
```

### SVG Timeline Strategy

The original HTML hardcodes SVG coordinates. For a data-driven approach:

1. Define the SVG viewBox as `0 0 1560 185` (matching the original).
2. Each milestone track (M1, M2, M3) maps to a fixed Y coordinate (42, 98, 154).
3. Each milestone's X coordinate is calculated from its date relative to the timeline range: `x = (date - startDate) / (endDate - startDate) * viewBoxWidth`.
4. Render month gridlines at evenly spaced X intervals.
5. The "NOW" line is calculated from today's date.

This logic lives in a simple C# helper method, not a service — it's pure coordinate math.

### CSS Grid Heatmap Strategy

Directly port the CSS from the HTML reference:

```css
.hm-grid {
    display: grid;
    grid-template-columns: 160px repeat(4, 1fr);  /* row header + N months */
    grid-template-rows: 36px repeat(4, 1fr);       /* column header + 4 status rows */
}
```

The number of month columns should be driven by `data.json` — use inline `grid-template-columns` style computed in Blazor:

```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Data.Months.Count, 1fr);">
```

### Data Flow

```
data.json (on disk)
    ↓  File.ReadAllTextAsync
DashboardDataService (singleton, DI)
    ↓  Inject
Dashboard.razor (page)
    ↓  Pass props
Header.razor ← ProjectInfo
TimelineSection.razor ← Milestones[], Months[]
HeatmapGrid.razor ← Shipped, InProgress, Carryover, Blockers
```

No API controllers, no HTTP calls, no SignalR custom hubs. Blazor Server's built-in circuit handles everything.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope. The app runs on localhost for a single user generating screenshots. Do not add authentication middleware, Identity, or any auth packages.

If this ever needs to be shared on a network, the simplest addition would be a reverse proxy with basic auth — but defer this entirely.

### Data Protection

- `data.json` contains project status information, not PII or secrets. No encryption needed.
- If sensitive project names are a concern, the file lives on the user's local machine only.

### Hosting & Deployment

| Scenario | Approach |
|----------|----------|
| **Development** | `dotnet watch run` — hot reload, auto-browser-open. |
| **"Production" (local)** | `dotnet run --configuration Release` or `dotnet publish -c Release -o ./publish` then run the executable directly. |
| **Sharing with team** | Copy the publish folder to a shared drive. Recipient runs the `.exe`. Kestrel binds to localhost by default. |

### Infrastructure Costs

**$0.** Everything runs locally. No cloud, no containers, no CI/CD needed for this scope.

### Screenshot Workflow

The page is designed at 1920×1080 for direct screenshot capture. Recommended workflow:
1. Open `https://localhost:5001` in Edge/Chrome.
2. Press `F11` for fullscreen.
3. Use `Win+Shift+S` (Snipping Tool) or browser DevTools device emulation at 1920×1080.
4. Paste into PowerPoint.

**Pro tip:** Add a `@media print` CSS block that hides browser chrome, or create a dedicated `/screenshot` route with zero padding for pixel-perfect captures.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| **Blazor Server is overkill for a static page** | Medium | Low | Accepted trade-off. The project mandates Blazor Server. The overhead is minimal — a single SignalR connection for one user. Alternative would be a static HTML generator, but that's outside the mandated stack. |
| **SVG coordinate math produces misaligned elements** | Medium | Medium | Port exact pixel values from the HTML reference first, then parameterize. Test at 1920×1080 in Chrome. |
| **data.json schema changes break the UI silently** | Low | Medium | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and add null checks in Razor. Optionally validate on load and show a clear error banner. |
| **Hot Reload doesn't catch CSS changes in .razor.css** | Low | Low | Known .NET 8 limitation for some CSS isolation scenarios. Workaround: use a global `dashboard.css` file in `wwwroot/css/` which always hot-reloads. |
| **Font rendering differs across machines** | Low | Low | Segoe UI is pre-installed on all Windows 10/11 machines. For non-Windows, fall back to system sans-serif. Screenshots will always be taken on Windows. |

### Trade-offs Made

1. **No database** → Cannot query historical data or trend over time. Accepted: the user edits `data.json` monthly.
2. **No auth** → Anyone on the local network could access if Kestrel binds to `0.0.0.0`. Mitigation: default binding is `localhost` only.
3. **No component library (MudBlazor, Radzen, etc.)** → Must hand-code all CSS. Accepted: the design is fixed and specific. A component library would fight the custom layout rather than help.
4. **Fixed 1920×1080 layout** → Not usable on mobile or smaller screens. Accepted: this is a screenshot tool, not a responsive web app.

### Skills Considerations

- **Blazor familiarity:** Any C# developer can work in Blazor. The Razor syntax is straightforward.
- **SVG knowledge:** The timeline requires understanding SVG coordinate systems, `<line>`, `<polygon>`, `<circle>`, and `<text>` elements. This is the steepest learning curve, but the HTML reference provides a complete example to port from.
- **CSS Grid:** Modern and well-documented. The HTML reference provides the exact grid definition.

---

## 7. Open Questions

| # | Question | Recommendation | Stakeholder |
|---|----------|---------------|-------------|
| 1 | **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable via `data.json`? | Yes — make the month list dynamic in `data.json`. The CSS Grid column count adapts automatically. | Product Owner |
| 2 | **Should the timeline scale to a fixed range (e.g., Jan–Jun) or auto-scale to the data?** | Fixed range defined in `data.json` (e.g., `timelineStart: "2026-01-01"`, `timelineEnd: "2026-06-30"`). Auto-scaling adds complexity for minimal benefit. | Product Owner |
| 3 | **Should `data.json` be editable through the UI, or only via text editor?** | Text editor only for MVP. A JSON editor UI would triple the scope. | Project Lead |
| 4 | **Multiple projects or single project per instance?** | Single project per `data.json`. For multiple projects, run multiple instances or switch JSON files. | Product Owner |
| 5 | **Should the "NOW" line auto-calculate from system date, or be set in `data.json`?** | Auto-calculate from `DateTime.Now` by default, with an optional override in `data.json` for generating screenshots for specific dates. | Engineer |
| 6 | **Will the ADO Backlog link in the header be functional, or just display text?** | Make it a real `<a href>` — the URL comes from `data.json`. Zero implementation cost. | Product Owner |
| 7 | **Dark mode or light mode only?** The design reference is light mode. | Light mode only. Dark mode doubles CSS effort for a screenshot tool. | Product Owner |

---

## 8. Implementation Recommendations

### Phase 1: Static Port (Day 1) — MVP

**Goal:** Pixel-perfect reproduction of `OriginalDesignConcept.html` as a Blazor Server app with hardcoded data.

1. `dotnet new blazor -n ReportingDashboard --interactivity Server`
2. Delete all scaffolded pages except the root layout.
3. Create `Dashboard.razor` as the single page at route `/`.
4. Copy all CSS from the HTML reference into `wwwroot/css/dashboard.css`.
5. Port the HTML structure into Razor components: `Header.razor`, `TimelineSection.razor`, `HeatmapGrid.razor`.
6. Hardcode all data inline (copy text content from the HTML reference).
7. **Validate:** Screenshot at 1920×1080 and compare side-by-side with the design reference.

**Deliverable:** A running Blazor app that produces an identical screenshot to the HTML reference.

### Phase 2: Data-Driven (Day 2)

**Goal:** Replace hardcoded data with `data.json`.

1. Define `DashboardData` record types matching the JSON schema.
2. Create a sample `data.json` with fictional project data.
3. Build `DashboardDataService` to read and deserialize the file.
4. Update Razor components to bind to the data model.
5. Parameterize SVG coordinates (milestone positions calculated from dates).
6. Parameterize CSS Grid columns (month count from data).
7. **Validate:** Change `data.json`, restart, verify the dashboard updates.

**Deliverable:** A data-driven dashboard that renders any project from a JSON file.

### Phase 3: Polish & QoL (Day 3, optional)

**Goal:** Quality-of-life improvements.

1. Add `FileSystemWatcher` for live reload of `data.json` (no restart needed).
2. Add a print-friendly CSS `@media print` stylesheet.
3. Add graceful error handling if `data.json` is missing or malformed (show a helpful error message, not a stack trace).
4. Add a `/screenshot` route variant with zero margin/padding for cleaner captures.
5. Create 2–3 sample `data.json` files for different fictional projects to demonstrate flexibility.

### Quick Wins

- **Immediate:** The HTML reference can be dropped directly into `wwwroot/` as a static file and served alongside Blazor, giving an instant working baseline to compare against.
- **Immediate:** `dotnet watch` gives sub-second CSS iteration — no build step for style tweaks.
- **High value:** The SVG timeline is the most complex visual element. Port it first, then the heatmap grid (which is straightforward CSS Grid).

### What NOT to Build

- ❌ User authentication or role-based access
- ❌ A database or ORM layer
- ❌ An admin UI for editing data
- ❌ Responsive/mobile layouts
- ❌ Dark mode
- ❌ API controllers or REST endpoints
- ❌ Docker containerization
- ❌ CI/CD pipelines
- ❌ Logging infrastructure (beyond `Console.WriteLine` for debugging)
- ❌ Multiple pages or navigation

---

## Appendix A: NuGet Package Summary

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.Components` | 8.0.x (SDK-included) | Blazor framework | ✅ Included in SDK |
| `System.Text.Json` | 8.0.x (SDK-included) | JSON deserialization | ✅ Included in SDK |
| No additional packages | — | — | — |

**Total external NuGet dependencies: 0**

This is intentional. The project's scope does not justify any third-party packages. Everything needed — Blazor components, JSON parsing, file I/O, Kestrel hosting — ships with the .NET 8 SDK.

## Appendix B: CSS Architecture Notes

The HTML reference defines these key CSS patterns that must be preserved:

| Pattern | CSS Property | Value | Used For |
|---------|-------------|-------|----------|
| Fixed viewport | `body { width: 1920px; height: 1080px; overflow: hidden; }` | Fixed dimensions | Screenshot-ready layout |
| Vertical flex | `body { display: flex; flex-direction: column; }` | Top-to-bottom sections | Page structure |
| Timeline area | `.tl-area { height: 196px; }` | Fixed height | Consistent SVG canvas |
| Heatmap grid | `grid-template-columns: 160px repeat(4, 1fr)` | Row headers + data columns | Status matrix |
| Status row colors | `.ship-cell { background: #F0FBF0; }` etc. | Category-specific backgrounds | Visual grouping |
| Current month highlight | `.apr { background: #D8F2DA; }` etc. | Darker shade for current month | Temporal emphasis |
| Bullet indicators | `.it::before { width: 6px; height: 6px; border-radius: 50%; }` | Colored dots | Item markers |
| Milestone diamonds | `<polygon points="..." fill="#F4B400">` | SVG rotated squares | PoC milestones |
| Drop shadows | `<filter id="sh"><feDropShadow .../>` | SVG filter | Milestone emphasis |

**Recommendation:** Start with the exact CSS from the HTML reference in a single `dashboard.css` file. Only refactor into `.razor.css` component-scoped files if the single file becomes unwieldy (unlikely at this scale).

## Appendix C: Sample `data.json` Schema

```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "currentMonth": "Apr"
  },
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
      },
      {
        "id": "M2",
        "label": "PDS & Data Inventory",
        "color": "#00897B",
        "milestones": [
          { "date": "2025-12-19", "type": "checkpoint", "label": "Dec 19" },
          { "date": "2026-02-11", "type": "checkpoint", "label": "Feb 11" },
          { "date": "2026-03-15", "type": "poc", "label": "Mar 15 PoC" }
        ]
      }
    ]
  },
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "shipped": {
    "Jan": ["Graph API v2 Launch", "DSAR Portal Redesign"],
    "Feb": ["PDS Connector", "Consent SDK 3.0"],
    "Mar": ["Auto-classification ML model", "DFD Generator v1"],
    "Apr": ["MS Role Integration"]
  },
  "inProgress": {
    "Jan": [],
    "Feb": ["Data Inventory Scanner"],
    "Mar": ["Chatbot NLU Training"],
    "Apr": ["Review Automation", "PDS Dashboard"]
  },
  "carryover": {
    "Jan": [],
    "Feb": [],
    "Mar": ["Legacy API Migration"],
    "Apr": ["Consent Propagation"]
  },
  "blockers": {
    "Jan": [],
    "Feb": [],
    "Mar": [],
    "Apr": ["Dependency on Platform Team SDK release"]
  }
}
```

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/bd2a0064ad27527a192055a8fa6edcf704f1ccd5/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
