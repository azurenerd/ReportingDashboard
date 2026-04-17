# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-17 09:45 UTC_

### Summary

This project is a single-page executive reporting dashboard that visualizes project milestones, shipped features, in-progress work, carryover items, and blockers in a screenshot-friendly layout optimized for PowerPoint decks. The design is already defined via an HTML reference template (`OriginalDesignConcept.html`) featuring an SVG timeline with milestone diamonds/circles and a color-coded heatmap grid. **Primary Recommendation:** Build this as a minimal Blazor Server application with zero authentication, reading all data from a local `data.json` file. Use raw CSS (ported from the reference HTML) and inline SVG rendered via Blazor components — no JavaScript charting libraries needed. The entire app should be 5–8 Razor components, one JSON model, and one service class. Total implementation effort: 1–2 days for an experienced .NET developer. The simplicity of this project is its greatest asset. Resist the temptation to over-engineer. The reference HTML is already an excellent design — the Blazor version should be a near-direct port with improved data-binding, not a redesign. ---

### Key Findings

- The reference design uses only CSS Grid, Flexbox, and inline SVG — no JavaScript charting library is needed. Blazor can render all of this natively.
- A `data.json` flat file is the ideal data source for this use case. No database is necessary or beneficial — the data changes infrequently and is manually curated.
- Blazor Server's real-time SignalR connection is irrelevant here (no interactivity needed), but it's the mandated stack and works fine for local use. The app will function as a simple server-rendered page.
- The design targets a fixed 1920×1080 viewport for screenshot capture. This simplifies CSS — no complex responsive breakpoints needed, though a basic fluid layout is a low-cost improvement.
- The SVG timeline is the most complex visual element. It should be generated programmatically from milestone data using a dedicated Blazor component that calculates x-positions from date ranges.
- The heatmap grid maps directly to CSS Grid with 5 columns (`160px repeat(4, 1fr)`) and 5 rows (header + 4 status categories). Each cell contains a list of work items with colored bullet indicators.
- The color palette is fully defined in the reference: green (#34A853) for shipped, blue (#0078D4) for in-progress, amber (#F4B400) for carryover, red (#EA4335) for blockers.
- System.Text.Json (built into .NET 8) is sufficient for JSON deserialization — no third-party JSON library needed.
- The app should be runnable via `dotnet run` with zero configuration. No Docker, no IIS, no reverse proxy. --- **Goal:** Get the reference design rendering pixel-perfect in Blazor with hardcoded data.
- `dotnet new blazor -n ReportingDashboard --interactivity Server` (creates .NET 8 Blazor project)
- Delete the default Counter/Weather pages
- Create `Dashboard.razor` as the single page at route `/`
- Copy the reference HTML structure into Razor markup
- Port the reference CSS into `wwwroot/css/dashboard.css`
- Hardcode all data values inline (same as the reference HTML)
- **Verify:** Visual comparison with reference screenshot at 1920×1080 **Goal:** Replace hardcoded values with data from `data.json`.
- Define C# record model classes matching the JSON schema
- Create a sample `data.json` with fictional project data
- Build `DashboardDataService` to read and deserialize the file
- Register service in DI container
- Inject service into `Dashboard.razor` and bind data to markup
- Extract sub-components: `Header.razor`, `Timeline.razor`, `HeatmapGrid.razor`
- **Verify:** Change values in `data.json`, restart app, confirm UI updates **Goal:** Make the SVG timeline fully dynamic.
- Implement date-to-pixel calculation in `Timeline.razor`
- Render month grid lines from date range
- Render milestone tracks with correct shapes per type
- Render "Now" indicator line
- **Verify:** Add/remove milestones in JSON, verify SVG updates correctly **Goal:** Final visual polish for executive-quality screenshots.
- Fine-tune spacing, font sizes, colors to match reference exactly
- Add CSS custom properties for easy color theming
- Hide Blazor framework UI elements (reconnect modal, error UI)
- Add `@rendermode` static SSR if SignalR overhead is noticeable
- Test screenshot workflow: `dotnet run` → open browser → capture at 1920×1080
- Write a brief README with usage instructions
- **Verify:** Side-by-side comparison with reference design. Confirm screenshot quality.
- **Fictional project data:** Create a compelling fake project ("Project Aurora — AI-Powered Customer Analytics Platform") with realistic milestones spanning 6 months. This makes demos and screenshots look professional.
- **CSS custom properties for theming:** 10 minutes of work, enables instant rebranding for different teams/projects.
- **Static SSR mode:** Add `@rendermode` static to eliminate the SignalR connection. The page loads faster and has no reconnection UI to worry about.
- ❌ No authentication
- ❌ No database
- ❌ No API endpoints
- ❌ No JavaScript interop
- ❌ No third-party UI component library
- ❌ No Docker containerization
- ❌ No CI/CD pipeline (local tool, not deployed)
- ❌ No real-time updates
- ❌ No export/print functionality
- ❌ No responsive mobile layout (target is 1920×1080 screenshots) --- The `.csproj` file should be minimal:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
``` **Zero NuGet packages required.** Everything needed (Blazor, System.Text.Json, Kestrel) ships with the .NET 8 SDK. For testing only:
```xml
<!-- ReportingDashboard.Tests.csproj -->
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
<PackageReference Include="xunit" Version="2.7.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
<PackageReference Include="bunit" Version="1.25.3" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```
```json
{
  "title": "Project Aurora Release Roadmap",
  "subtitle": "AI Platform · Customer Analytics Workstream · April 2026",
  "backlogLink": "https://dev.azure.com/org/project/_backlogs",
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "currentDate": "2026-04-17",
    "tracks": [
      {
        "id": "M1",
        "label": "ML Pipeline & Training",
        "color": "#0078D4",
        "events": [
          { "date": "2026-01-15", "label": "Kickoff", "type": "Checkpoint" },
          { "date": "2026-03-20", "label": "Mar 20 PoC", "type": "PoC" },
          { "date": "2026-05-01", "label": "May Prod", "type": "Production" }
        ]
      },
      {
        "id": "M2",
        "label": "Data Ingestion & ETL",
        "color": "#00897B",
        "events": [
          { "date": "2025-12-19", "label": "Dec 19", "type": "Checkpoint" },
          { "date": "2026-02-11", "label": "Feb 11", "type": "Checkpoint" },
          { "date": "2026-03-15", "label": "Mar PoC", "type": "PoC" },
          { "date": "2026-05-15", "label": "May Prod", "type": "Production" }
        ]
      },
      {
        "id": "M3",
        "label": "Dashboard & Reporting UI",
        "color": "#546E7A",
        "events": [
          { "date": "2026-02-01", "label": "Design Complete", "type": "Checkpoint" },
          { "date": "2026-04-10", "label": "Apr PoC", "type": "PoC" },
          { "date": "2026-06-01", "label": "Jun Prod", "type": "Production" }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "highlightColumn": "Apr",
    "shipped": {
      "items": {
        "Jan": ["Data schema v1", "Auth service"],
        "Feb": ["ETL pipeline alpha", "Logging framework"],
        "Mar": ["ML model v1", "API gateway", "Monitoring alerts"],
        "Apr": ["Customer segmentation", "Export API"]
      }
    },
    "inProgress": {
      "items": {
        "Jan": ["Pipeline design"],
        "Feb": ["Model training infra"],
        "Mar": ["Dashboard wireframes"],
        "Apr": ["Real-time scoring", "Dashboard build", "Load testing"]
      }
    },
    "carryover": {
      "items": {
        "Jan": [],
        "Feb": ["Data quality checks"],
        "Mar": ["SSO integration"],
        "Apr": ["SSO integration", "Perf optimization"]
      }
    },
    "blockers": {
      "items": {
        "Jan": [],
        "Feb": [],
        "Mar": ["GPU quota approval"],
        "Apr": ["Vendor contract delay"]
      }
    }
  }
}
``` | Project | Relevance | Notes | |---------|-----------|-------| | **Blazor Dashboard samples (Microsoft)** | Template reference | Microsoft's official Blazor dashboard samples show layout patterns but use Syncfusion/Telerik controls | | **Blazor.Diagrams** | SVG rendering in Blazor | Shows how to do complex SVG in Blazor components; useful reference for timeline | | **BlazorGantt** (community) | Gantt chart in Blazor | Overly complex for this use case but validates the approach of SVG-in-Blazor | | **Excubo.Blazor.Canvas** | Canvas rendering | Alternative to SVG; not recommended (SVG is simpler for this design) | **Bottom line:** No existing library does exactly what the reference design needs. The design is simple enough to build from scratch in Blazor, and that approach gives full pixel-level control — which is critical for screenshot-quality output.

### Recommended Tools & Technologies

- **Project:** Executive Project Reporting Dashboard **Date:** April 17, 2026 **Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure --- | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **UI Framework** | Blazor Server (built-in) | .NET 8.0 | Mandated stack. Use `@rendermode InteractiveServer` or static SSR | | **CSS Approach** | Scoped CSS + global stylesheet | N/A | Port the reference HTML's CSS directly. Use Blazor CSS isolation (`Component.razor.css`) for component styles | | **SVG Rendering** | Raw SVG markup in Razor | N/A | No library needed. The reference HTML already has the SVG template — port it to a parameterized Razor component | | **Icons** | None needed | N/A | The design uses only CSS shapes (rotated squares for diamonds, circles) — no icon library required | | **Fonts** | Segoe UI (system font) | N/A | Already available on Windows. Fallback: `'Segoe UI', Arial, sans-serif` per the reference CSS | **Why no charting library?** The timeline is a simple SVG with lines, circles, diamonds, and text. Libraries like Chart.js, ApexCharts, or Radzen Charts would add complexity without benefit. The reference design's SVG is ~40 lines — a Blazor component can generate this dynamically with less code than configuring a charting library.
- **MudBlazor / Radzen / Syncfusion**: These UI component libraries are excellent for form-heavy CRUD apps but are overkill here. They'd impose their own design system, making it harder to match the reference design pixel-for-pixel. Raw HTML/CSS gives full control.
- **Blazorise**: Same reasoning. The design is custom, not based on Bootstrap or Material. | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **JSON Deserialization** | System.Text.Json | Built into .NET 8 | Use `JsonSerializer.Deserialize<T>()` with source generators for AOT compatibility | | **File I/O** | System.IO.File | Built into .NET 8 | `File.ReadAllTextAsync("data.json")` — that's it | | **Configuration** | Standard `appsettings.json` | Built into .NET 8 | For the data file path only. Or just hardcode `data.json` in the project root | | **DI Registration** | Built-in DI container | .NET 8 | Register a singleton `DashboardDataService` | | Component | Recommendation | Notes | |-----------|---------------|-------| | **Primary Store** | `data.json` flat file | JSON file in the project directory. Manually edited or generated by a script. No database. | | **Schema** | Strongly-typed C# record classes | Define `DashboardData`, `Milestone`, `WorkItem`, `TimelineConfig` records | | **Hot Reload** | `FileSystemWatcher` (optional) | If desired, watch `data.json` for changes and refresh the UI automatically via SignalR. Low priority. | **Why no database?** The data is:
- Small (dozens of items, not thousands)
- Read-only at runtime
- Manually curated before each executive presentation
- Versioned alongside the project (JSON in source control) A SQLite database would add migrations, an ORM, and connection management for zero benefit. A JSON file is inspectable, diffable, and editable in any text editor. | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Unit Testing** | xUnit | 2.7+ | .NET ecosystem standard | | **Blazor Component Testing** | bUnit | 1.25+ | Test Razor components in isolation. Verify SVG output, grid rendering | | **Assertions** | FluentAssertions | 6.12+ | Readable test assertions (`result.Should().HaveCount(4)`) | | **Snapshot Testing** | Verify | 23.0+ | Optional: snapshot-test rendered HTML output to catch visual regressions | | Component | Recommendation | Notes | |-----------|---------------|-------| | **SDK** | .NET 8.0 SDK (latest patch) | Currently 8.0.404 | | **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Hot reload support for Blazor | | **Linting** | dotnet format (built-in) | `dotnet format` for code style | | **Build** | `dotnet build` | Standard MSBuild | | **Run** | `dotnet run` or `dotnet watch` | `dotnet watch` for live reload during development | --- This is a read-only, single-page app. Use the simplest architecture that works:
```
data.json
    ↓ (read on startup + optional file watch)
DashboardDataService (singleton)
    ↓ (injected into components)
Razor Components (render HTML/SVG/CSS)
    ↓ (served to browser)
Browser (static render, screenshot-ready)
``` **Do NOT use:** CQRS, MediatR, Repository Pattern, Clean Architecture layers, or any pattern designed for complex domain logic. This app has no domain logic — it reads JSON and renders HTML.
```
ReportingDashboard.sln
│
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── data.json                    ← Dashboard data
│       ├── Models/
│       │   └── DashboardData.cs         ← Record types for JSON shape
│       ├── Services/
│       │   └── DashboardDataService.cs  ← Reads and caches data.json
│       ├── Components/
│       │   ├── App.razor                ← Root component
│       │   ├── Layout/
│       │   │   └── MainLayout.razor
│       │   └── Pages/
│       │       └── Dashboard.razor      ← The single page
│       ├── Components/Dashboard/
│       │   ├── Header.razor             ← Title, subtitle, legend
│       │   ├── Timeline.razor           ← SVG milestone timeline
│       │   ├── HeatmapGrid.razor        ← CSS Grid status matrix
│       │   ├── HeatmapRowHeader.razor   ← Row label (Shipped, In Progress, etc.)
│       │   └── HeatmapCell.razor        ← Individual cell with work items
│       └── wwwroot/
│           └── css/
│               └── dashboard.css        ← Global styles ported from reference
│
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── ...
```
```csharp
// Models/DashboardData.cs
public record DashboardData
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string BacklogLink { get; init; }
    public required TimelineConfig Timeline { get; init; }
    public required HeatmapConfig Heatmap { get; init; }
}

public record TimelineConfig
{
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required DateOnly CurrentDate { get; init; }
    public required List<MilestoneTrack> Tracks { get; init; }
}

public record MilestoneTrack
{
    public required string Id { get; init; }          // "M1", "M2", "M3"
    public required string Label { get; init; }       // "Chatbot & MS Role"
    public required string Color { get; init; }       // "#0078D4"
    public required List<MilestoneEvent> Events { get; init; }
}

public record MilestoneEvent
{
    public required DateOnly Date { get; init; }
    public required string Label { get; init; }
    public required MilestoneType Type { get; init; } // Checkpoint, PoC, Production
}

public enum MilestoneType { Checkpoint, PoC, Production }

public record HeatmapConfig
{
    public required List<string> Columns { get; init; }       // ["Jan", "Feb", "Mar", "Apr"]
    public required string HighlightColumn { get; init; }     // "Apr" (current month)
    public required HeatmapCategory Shipped { get; init; }
    public required HeatmapCategory InProgress { get; init; }
    public required HeatmapCategory Carryover { get; init; }
    public required HeatmapCategory Blockers { get; init; }
}

public record HeatmapCategory
{
    public required Dictionary<string, List<string>> Items { get; init; }
    // Key = column name ("Jan", "Feb", etc.), Value = list of item labels
}
``` The SVG timeline is the most complex component. Key implementation details:
- **Date-to-X mapping:** Calculate pixel position from dates using linear interpolation:
   ```csharp
   private double DateToX(DateOnly date)
   {
       double totalDays = (EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
       double dayOffset = (date.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
       return (dayOffset / totalDays) * SvgWidth;
   }
   ```
- **Track Y positions:** Each milestone track gets a fixed Y band (e.g., track 1 at y=42, track 2 at y=98, track 3 at y=154).
- **Milestone shapes:**
- **Checkpoint** → `<circle>` (small, filled gray)
- **PoC** → `<polygon>` (diamond, filled gold #F4B400)
- **Production** → `<polygon>` (diamond, filled green #34A853)
- **"Now" line** → `<line>` (dashed red vertical line at current date's X position)
- **Month grid lines:** Render `<line>` and `<text>` for each month boundary. Port the reference CSS nearly verbatim. Key decisions:
- **Use a single global `dashboard.css`** rather than scoped CSS per component. The design is cohesive and tightly coupled — splitting CSS across components adds complexity without benefit for a 1-page app.
- **Fixed viewport approach:** Set `body { width: 1920px; height: 1080px; overflow: hidden; }` for screenshot mode. Optionally add a CSS class toggle for fluid layout during development.
- **CSS Grid for heatmap:** `grid-template-columns: 160px repeat(4, 1fr)` exactly as in the reference.
- **Color tokens:** Define CSS custom properties for the 4 status colors to enable easy theming:
  ```css
  :root {
      --color-shipped: #34A853;
      --color-progress: #0078D4;
      --color-carryover: #F4B400;
      --color-blockers: #EA4335;
  }
  ``` ---

### Considerations & Risks

- **None.** This is explicitly a no-auth, local-only tool. The app will:
- Bind to `localhost` only (Kestrel default for development)
- Have no login page, no user management, no roles
- Be accessed only by the person running it on their machine If sharing becomes needed later, the simplest upgrade path is adding Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (one line in `Program.cs`).
- **No sensitive data in `data.json`:** Project names, milestone labels, and status items are non-confidential.
- **No encryption needed:** Local file, local server, local browser.
- **Source control:** The `data.json` can be committed to the repo. If project names are sensitive, add it to `.gitignore` and distribute separately. | Aspect | Recommendation | |--------|---------------| | **Runtime** | Kestrel (built into .NET 8) | | **Binding** | `http://localhost:5000` (default) | | **Launch** | `dotnet run` from project directory | | **Distribution** | Clone repo + `dotnet run`, or `dotnet publish` for a self-contained exe | | **Self-contained publish** | `dotnet publish -c Release -r win-x64 --self-contained` produces a single folder that runs anywhere on Windows without .NET SDK | **$0.** This runs on the developer's local machine. No cloud services, no hosting, no licenses. --- **Severity:** Low **Impact:** Slightly more complex than a pure static HTML file. Requires .NET SDK installed. **Mitigation:** This is the mandated stack, and the overhead is minimal. The benefit is strongly-typed data binding from `data.json` — no manual HTML editing when data changes. The trade-off is worth it. **Alternative within stack:** Blazor Static SSR (new in .NET 8) renders the page server-side without SignalR. Add `@rendermode` to use static rendering, which eliminates the WebSocket connection. This is ideal for a screenshot-oriented page. **Severity:** Medium **Impact:** The SVG timeline has multiple overlapping elements (tracks, diamonds, circles, text labels, dashed lines). Getting positions and layering right requires careful math.
- Port the reference SVG coordinates first as hardcoded values, verify visual match.
- Then parameterize with date-to-pixel calculations.
- Use `<defs>` and `<filter>` for drop shadows exactly as the reference does. **Severity:** Medium **Impact:** The page must look identical in screenshots as it does in the browser. Blazor Server adds a reconnection UI overlay and a `<script>` tag that could affect rendering.
- Remove or hide the Blazor reconnection UI via CSS: `.components-reconnect-modal { display: none; }`
- Use Chrome DevTools device emulation at 1920×1080 for consistent screenshots
- Consider adding a `?screenshot=true` query param that hides any interactive elements **Severity:** Low **Impact:** As the dashboard evolves, `data.json` schema may need new fields. **Mitigation:** Use nullable properties with defaults in C# records. New fields are optional; old JSON files still deserialize correctly. The page loads `data.json` once at startup (or on first request). If you edit the JSON while the app is running, you won't see changes until restart. **Acceptable for this use case:** You edit JSON → restart app → take screenshot. Cycle time is ~2 seconds. **Optional enhancement:** Add `FileSystemWatcher` to auto-reload, but this adds complexity for minimal gain. The current design is one page. If future requests add multiple project dashboards or comparison views, the architecture supports it (add more Razor pages, extend data.json with an array of projects), but it's not designed for it. ---
- **How many months should the heatmap display?** The reference shows 4 columns (Jan–Apr). Should this be configurable in `data.json`, or always show the last N months? **Recommendation:** Make it data-driven — the `Columns` array in `data.json` controls what months appear.
- **How many milestone tracks in the timeline?** The reference shows 3 (M1, M2, M3). Should the SVG dynamically scale if there are 5 or 6 tracks? **Recommendation:** Support up to 5 tracks with automatic Y-spacing. Beyond that, the 196px timeline area becomes cramped.
- **Should the "Now" line auto-calculate from system date or be specified in `data.json`?** **Recommendation:** Default to `DateTime.Now` but allow override in JSON for generating historical snapshots.
- **Print/export support?** Should the page include a "Save as PDF" or "Copy to clipboard" button, or will the user always use browser screenshot tools? **Recommendation:** No export button. The user will use Windows Snipping Tool or browser screenshot. Keep the UI chrome-free for clean screenshots.
- **Will multiple projects need separate dashboard pages?** If so, the data model should include a project identifier and the app should support routing (`/dashboard/{projectId}`). **Recommendation:** Defer until needed. Start with one `data.json` = one project.
- **What is the reference screenshot `C:/Pics/ReportingDashboardDesign.png`?** This file needs to be reviewed to identify any design differences from `OriginalDesignConcept.html`. If it's a modified version, those modifications should be cataloged before implementation. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Project Reporting Dashboard
**Date:** April 17, 2026
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure

---

## 1. Executive Summary

This project is a single-page executive reporting dashboard that visualizes project milestones, shipped features, in-progress work, carryover items, and blockers in a screenshot-friendly layout optimized for PowerPoint decks. The design is already defined via an HTML reference template (`OriginalDesignConcept.html`) featuring an SVG timeline with milestone diamonds/circles and a color-coded heatmap grid.

**Primary Recommendation:** Build this as a minimal Blazor Server application with zero authentication, reading all data from a local `data.json` file. Use raw CSS (ported from the reference HTML) and inline SVG rendered via Blazor components — no JavaScript charting libraries needed. The entire app should be 5–8 Razor components, one JSON model, and one service class. Total implementation effort: 1–2 days for an experienced .NET developer.

The simplicity of this project is its greatest asset. Resist the temptation to over-engineer. The reference HTML is already an excellent design — the Blazor version should be a near-direct port with improved data-binding, not a redesign.

---

## 2. Key Findings

- The reference design uses only CSS Grid, Flexbox, and inline SVG — no JavaScript charting library is needed. Blazor can render all of this natively.
- A `data.json` flat file is the ideal data source for this use case. No database is necessary or beneficial — the data changes infrequently and is manually curated.
- Blazor Server's real-time SignalR connection is irrelevant here (no interactivity needed), but it's the mandated stack and works fine for local use. The app will function as a simple server-rendered page.
- The design targets a fixed 1920×1080 viewport for screenshot capture. This simplifies CSS — no complex responsive breakpoints needed, though a basic fluid layout is a low-cost improvement.
- The SVG timeline is the most complex visual element. It should be generated programmatically from milestone data using a dedicated Blazor component that calculates x-positions from date ranges.
- The heatmap grid maps directly to CSS Grid with 5 columns (`160px repeat(4, 1fr)`) and 5 rows (header + 4 status categories). Each cell contains a list of work items with colored bullet indicators.
- The color palette is fully defined in the reference: green (#34A853) for shipped, blue (#0078D4) for in-progress, amber (#F4B400) for carryover, red (#EA4335) for blockers.
- System.Text.Json (built into .NET 8) is sufficient for JSON deserialization — no third-party JSON library needed.
- The app should be runnable via `dotnet run` with zero configuration. No Docker, no IIS, no reverse proxy.

---

## 3. Recommended Technology Stack

### Frontend (UI Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 | Mandated stack. Use `@rendermode InteractiveServer` or static SSR |
| **CSS Approach** | Scoped CSS + global stylesheet | N/A | Port the reference HTML's CSS directly. Use Blazor CSS isolation (`Component.razor.css`) for component styles |
| **SVG Rendering** | Raw SVG markup in Razor | N/A | No library needed. The reference HTML already has the SVG template — port it to a parameterized Razor component |
| **Icons** | None needed | N/A | The design uses only CSS shapes (rotated squares for diamonds, circles) — no icon library required |
| **Fonts** | Segoe UI (system font) | N/A | Already available on Windows. Fallback: `'Segoe UI', Arial, sans-serif` per the reference CSS |

**Why no charting library?** The timeline is a simple SVG with lines, circles, diamonds, and text. Libraries like Chart.js, ApexCharts, or Radzen Charts would add complexity without benefit. The reference design's SVG is ~40 lines — a Blazor component can generate this dynamically with less code than configuring a charting library.

**Alternative considered and rejected:**
- **MudBlazor / Radzen / Syncfusion**: These UI component libraries are excellent for form-heavy CRUD apps but are overkill here. They'd impose their own design system, making it harder to match the reference design pixel-for-pixel. Raw HTML/CSS gives full control.
- **Blazorise**: Same reasoning. The design is custom, not based on Bootstrap or Material.

### Backend (Data Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **JSON Deserialization** | System.Text.Json | Built into .NET 8 | Use `JsonSerializer.Deserialize<T>()` with source generators for AOT compatibility |
| **File I/O** | System.IO.File | Built into .NET 8 | `File.ReadAllTextAsync("data.json")` — that's it |
| **Configuration** | Standard `appsettings.json` | Built into .NET 8 | For the data file path only. Or just hardcode `data.json` in the project root |
| **DI Registration** | Built-in DI container | .NET 8 | Register a singleton `DashboardDataService` |

### Data Storage

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **Primary Store** | `data.json` flat file | JSON file in the project directory. Manually edited or generated by a script. No database. |
| **Schema** | Strongly-typed C# record classes | Define `DashboardData`, `Milestone`, `WorkItem`, `TimelineConfig` records |
| **Hot Reload** | `FileSystemWatcher` (optional) | If desired, watch `data.json` for changes and refresh the UI automatically via SignalR. Low priority. |

**Why no database?** The data is:
1. Small (dozens of items, not thousands)
2. Read-only at runtime
3. Manually curated before each executive presentation
4. Versioned alongside the project (JSON in source control)

A SQLite database would add migrations, an ORM, and connection management for zero benefit. A JSON file is inspectable, diffable, and editable in any text editor.

### Testing

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | 2.7+ | .NET ecosystem standard |
| **Blazor Component Testing** | bUnit | 1.25+ | Test Razor components in isolation. Verify SVG output, grid rendering |
| **Assertions** | FluentAssertions | 6.12+ | Readable test assertions (`result.Should().HaveCount(4)`) |
| **Snapshot Testing** | Verify | 23.0+ | Optional: snapshot-test rendered HTML output to catch visual regressions |

### Build & Tooling

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **SDK** | .NET 8.0 SDK (latest patch) | Currently 8.0.404 |
| **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Hot reload support for Blazor |
| **Linting** | dotnet format (built-in) | `dotnet format` for code style |
| **Build** | `dotnet build` | Standard MSBuild |
| **Run** | `dotnet run` or `dotnet watch` | `dotnet watch` for live reload during development |

---

## 4. Architecture Recommendations

### Overall Pattern: Minimal Vertical Slice

This is a read-only, single-page app. Use the simplest architecture that works:

```
data.json
    ↓ (read on startup + optional file watch)
DashboardDataService (singleton)
    ↓ (injected into components)
Razor Components (render HTML/SVG/CSS)
    ↓ (served to browser)
Browser (static render, screenshot-ready)
```

**Do NOT use:** CQRS, MediatR, Repository Pattern, Clean Architecture layers, or any pattern designed for complex domain logic. This app has no domain logic — it reads JSON and renders HTML.

### Solution Structure

```
ReportingDashboard.sln
│
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── data.json                    ← Dashboard data
│       ├── Models/
│       │   └── DashboardData.cs         ← Record types for JSON shape
│       ├── Services/
│       │   └── DashboardDataService.cs  ← Reads and caches data.json
│       ├── Components/
│       │   ├── App.razor                ← Root component
│       │   ├── Layout/
│       │   │   └── MainLayout.razor
│       │   └── Pages/
│       │       └── Dashboard.razor      ← The single page
│       ├── Components/Dashboard/
│       │   ├── Header.razor             ← Title, subtitle, legend
│       │   ├── Timeline.razor           ← SVG milestone timeline
│       │   ├── HeatmapGrid.razor        ← CSS Grid status matrix
│       │   ├── HeatmapRowHeader.razor   ← Row label (Shipped, In Progress, etc.)
│       │   └── HeatmapCell.razor        ← Individual cell with work items
│       └── wwwroot/
│           └── css/
│               └── dashboard.css        ← Global styles ported from reference
│
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── ...
```

### Data Model Design

```csharp
// Models/DashboardData.cs
public record DashboardData
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string BacklogLink { get; init; }
    public required TimelineConfig Timeline { get; init; }
    public required HeatmapConfig Heatmap { get; init; }
}

public record TimelineConfig
{
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required DateOnly CurrentDate { get; init; }
    public required List<MilestoneTrack> Tracks { get; init; }
}

public record MilestoneTrack
{
    public required string Id { get; init; }          // "M1", "M2", "M3"
    public required string Label { get; init; }       // "Chatbot & MS Role"
    public required string Color { get; init; }       // "#0078D4"
    public required List<MilestoneEvent> Events { get; init; }
}

public record MilestoneEvent
{
    public required DateOnly Date { get; init; }
    public required string Label { get; init; }
    public required MilestoneType Type { get; init; } // Checkpoint, PoC, Production
}

public enum MilestoneType { Checkpoint, PoC, Production }

public record HeatmapConfig
{
    public required List<string> Columns { get; init; }       // ["Jan", "Feb", "Mar", "Apr"]
    public required string HighlightColumn { get; init; }     // "Apr" (current month)
    public required HeatmapCategory Shipped { get; init; }
    public required HeatmapCategory InProgress { get; init; }
    public required HeatmapCategory Carryover { get; init; }
    public required HeatmapCategory Blockers { get; init; }
}

public record HeatmapCategory
{
    public required Dictionary<string, List<string>> Items { get; init; }
    // Key = column name ("Jan", "Feb", etc.), Value = list of item labels
}
```

### SVG Timeline Rendering Strategy

The SVG timeline is the most complex component. Key implementation details:

1. **Date-to-X mapping:** Calculate pixel position from dates using linear interpolation:
   ```csharp
   private double DateToX(DateOnly date)
   {
       double totalDays = (EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
       double dayOffset = (date.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
       return (dayOffset / totalDays) * SvgWidth;
   }
   ```

2. **Track Y positions:** Each milestone track gets a fixed Y band (e.g., track 1 at y=42, track 2 at y=98, track 3 at y=154).

3. **Milestone shapes:**
   - **Checkpoint** → `<circle>` (small, filled gray)
   - **PoC** → `<polygon>` (diamond, filled gold #F4B400)
   - **Production** → `<polygon>` (diamond, filled green #34A853)
   - **"Now" line** → `<line>` (dashed red vertical line at current date's X position)

4. **Month grid lines:** Render `<line>` and `<text>` for each month boundary.

### CSS Strategy

Port the reference CSS nearly verbatim. Key decisions:

- **Use a single global `dashboard.css`** rather than scoped CSS per component. The design is cohesive and tightly coupled — splitting CSS across components adds complexity without benefit for a 1-page app.
- **Fixed viewport approach:** Set `body { width: 1920px; height: 1080px; overflow: hidden; }` for screenshot mode. Optionally add a CSS class toggle for fluid layout during development.
- **CSS Grid for heatmap:** `grid-template-columns: 160px repeat(4, 1fr)` exactly as in the reference.
- **Color tokens:** Define CSS custom properties for the 4 status colors to enable easy theming:
  ```css
  :root {
      --color-shipped: #34A853;
      --color-progress: #0078D4;
      --color-carryover: #F4B400;
      --color-blockers: #EA4335;
  }
  ```

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly a no-auth, local-only tool. The app will:
- Bind to `localhost` only (Kestrel default for development)
- Have no login page, no user management, no roles
- Be accessed only by the person running it on their machine

If sharing becomes needed later, the simplest upgrade path is adding Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (one line in `Program.cs`).

### Data Protection

- **No sensitive data in `data.json`:** Project names, milestone labels, and status items are non-confidential.
- **No encryption needed:** Local file, local server, local browser.
- **Source control:** The `data.json` can be committed to the repo. If project names are sensitive, add it to `.gitignore` and distribute separately.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Kestrel (built into .NET 8) |
| **Binding** | `http://localhost:5000` (default) |
| **Launch** | `dotnet run` from project directory |
| **Distribution** | Clone repo + `dotnet run`, or `dotnet publish` for a self-contained exe |
| **Self-contained publish** | `dotnet publish -c Release -r win-x64 --self-contained` produces a single folder that runs anywhere on Windows without .NET SDK |

### Infrastructure Costs

**$0.** This runs on the developer's local machine. No cloud services, no hosting, no licenses.

---

## 6. Risks & Trade-offs

### Risk: Blazor Server Is Overkill for a Static Page

**Severity:** Low
**Impact:** Slightly more complex than a pure static HTML file. Requires .NET SDK installed.
**Mitigation:** This is the mandated stack, and the overhead is minimal. The benefit is strongly-typed data binding from `data.json` — no manual HTML editing when data changes. The trade-off is worth it.

**Alternative within stack:** Blazor Static SSR (new in .NET 8) renders the page server-side without SignalR. Add `@rendermode` to use static rendering, which eliminates the WebSocket connection. This is ideal for a screenshot-oriented page.

### Risk: SVG Complexity in Timeline Component

**Severity:** Medium
**Impact:** The SVG timeline has multiple overlapping elements (tracks, diamonds, circles, text labels, dashed lines). Getting positions and layering right requires careful math.
**Mitigation:**
1. Port the reference SVG coordinates first as hardcoded values, verify visual match.
2. Then parameterize with date-to-pixel calculations.
3. Use `<defs>` and `<filter>` for drop shadows exactly as the reference does.

### Risk: Screenshot Fidelity

**Severity:** Medium
**Impact:** The page must look identical in screenshots as it does in the browser. Blazor Server adds a reconnection UI overlay and a `<script>` tag that could affect rendering.
**Mitigation:**
- Remove or hide the Blazor reconnection UI via CSS: `.components-reconnect-modal { display: none; }`
- Use Chrome DevTools device emulation at 1920×1080 for consistent screenshots
- Consider adding a `?screenshot=true` query param that hides any interactive elements

### Risk: Data File Schema Evolution

**Severity:** Low
**Impact:** As the dashboard evolves, `data.json` schema may need new fields.
**Mitigation:** Use nullable properties with defaults in C# records. New fields are optional; old JSON files still deserialize correctly.

### Trade-off: No Hot Data Refresh

The page loads `data.json` once at startup (or on first request). If you edit the JSON while the app is running, you won't see changes until restart.
**Acceptable for this use case:** You edit JSON → restart app → take screenshot. Cycle time is ~2 seconds.
**Optional enhancement:** Add `FileSystemWatcher` to auto-reload, but this adds complexity for minimal gain.

### Trade-off: Single Page Only

The current design is one page. If future requests add multiple project dashboards or comparison views, the architecture supports it (add more Razor pages, extend data.json with an array of projects), but it's not designed for it.

---

## 7. Open Questions

1. **How many months should the heatmap display?** The reference shows 4 columns (Jan–Apr). Should this be configurable in `data.json`, or always show the last N months? **Recommendation:** Make it data-driven — the `Columns` array in `data.json` controls what months appear.

2. **How many milestone tracks in the timeline?** The reference shows 3 (M1, M2, M3). Should the SVG dynamically scale if there are 5 or 6 tracks? **Recommendation:** Support up to 5 tracks with automatic Y-spacing. Beyond that, the 196px timeline area becomes cramped.

3. **Should the "Now" line auto-calculate from system date or be specified in `data.json`?** **Recommendation:** Default to `DateTime.Now` but allow override in JSON for generating historical snapshots.

4. **Print/export support?** Should the page include a "Save as PDF" or "Copy to clipboard" button, or will the user always use browser screenshot tools? **Recommendation:** No export button. The user will use Windows Snipping Tool or browser screenshot. Keep the UI chrome-free for clean screenshots.

5. **Will multiple projects need separate dashboard pages?** If so, the data model should include a project identifier and the app should support routing (`/dashboard/{projectId}`). **Recommendation:** Defer until needed. Start with one `data.json` = one project.

6. **What is the reference screenshot `C:/Pics/ReportingDashboardDesign.png`?** This file needs to be reviewed to identify any design differences from `OriginalDesignConcept.html`. If it's a modified version, those modifications should be cataloged before implementation.

---

## 8. Implementation Recommendations

### Phase 1: Static Port (Day 1, Morning)

**Goal:** Get the reference design rendering pixel-perfect in Blazor with hardcoded data.

1. `dotnet new blazor -n ReportingDashboard --interactivity Server` (creates .NET 8 Blazor project)
2. Delete the default Counter/Weather pages
3. Create `Dashboard.razor` as the single page at route `/`
4. Copy the reference HTML structure into Razor markup
5. Port the reference CSS into `wwwroot/css/dashboard.css`
6. Hardcode all data values inline (same as the reference HTML)
7. **Verify:** Visual comparison with reference screenshot at 1920×1080

### Phase 2: Data Binding (Day 1, Afternoon)

**Goal:** Replace hardcoded values with data from `data.json`.

1. Define C# record model classes matching the JSON schema
2. Create a sample `data.json` with fictional project data
3. Build `DashboardDataService` to read and deserialize the file
4. Register service in DI container
5. Inject service into `Dashboard.razor` and bind data to markup
6. Extract sub-components: `Header.razor`, `Timeline.razor`, `HeatmapGrid.razor`
7. **Verify:** Change values in `data.json`, restart app, confirm UI updates

### Phase 3: Timeline Component (Day 2, Morning)

**Goal:** Make the SVG timeline fully dynamic.

1. Implement date-to-pixel calculation in `Timeline.razor`
2. Render month grid lines from date range
3. Render milestone tracks with correct shapes per type
4. Render "Now" indicator line
5. **Verify:** Add/remove milestones in JSON, verify SVG updates correctly

### Phase 4: Polish & Screenshot Optimization (Day 2, Afternoon)

**Goal:** Final visual polish for executive-quality screenshots.

1. Fine-tune spacing, font sizes, colors to match reference exactly
2. Add CSS custom properties for easy color theming
3. Hide Blazor framework UI elements (reconnect modal, error UI)
4. Add `@rendermode` static SSR if SignalR overhead is noticeable
5. Test screenshot workflow: `dotnet run` → open browser → capture at 1920×1080
6. Write a brief README with usage instructions
7. **Verify:** Side-by-side comparison with reference design. Confirm screenshot quality.

### Quick Wins

- **Fictional project data:** Create a compelling fake project ("Project Aurora — AI-Powered Customer Analytics Platform") with realistic milestones spanning 6 months. This makes demos and screenshots look professional.
- **CSS custom properties for theming:** 10 minutes of work, enables instant rebranding for different teams/projects.
- **Static SSR mode:** Add `@rendermode` static to eliminate the SignalR connection. The page loads faster and has no reconnection UI to worry about.

### What NOT to Build

- ❌ No authentication
- ❌ No database
- ❌ No API endpoints
- ❌ No JavaScript interop
- ❌ No third-party UI component library
- ❌ No Docker containerization
- ❌ No CI/CD pipeline (local tool, not deployed)
- ❌ No real-time updates
- ❌ No export/print functionality
- ❌ No responsive mobile layout (target is 1920×1080 screenshots)

---

## Appendix A: Package References

The `.csproj` file should be minimal:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**Zero NuGet packages required.** Everything needed (Blazor, System.Text.Json, Kestrel) ships with the .NET 8 SDK.

For testing only:

```xml
<!-- ReportingDashboard.Tests.csproj -->
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
<PackageReference Include="xunit" Version="2.7.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
<PackageReference Include="bunit" Version="1.25.3" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

## Appendix B: Sample `data.json` Structure

```json
{
  "title": "Project Aurora Release Roadmap",
  "subtitle": "AI Platform · Customer Analytics Workstream · April 2026",
  "backlogLink": "https://dev.azure.com/org/project/_backlogs",
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "currentDate": "2026-04-17",
    "tracks": [
      {
        "id": "M1",
        "label": "ML Pipeline & Training",
        "color": "#0078D4",
        "events": [
          { "date": "2026-01-15", "label": "Kickoff", "type": "Checkpoint" },
          { "date": "2026-03-20", "label": "Mar 20 PoC", "type": "PoC" },
          { "date": "2026-05-01", "label": "May Prod", "type": "Production" }
        ]
      },
      {
        "id": "M2",
        "label": "Data Ingestion & ETL",
        "color": "#00897B",
        "events": [
          { "date": "2025-12-19", "label": "Dec 19", "type": "Checkpoint" },
          { "date": "2026-02-11", "label": "Feb 11", "type": "Checkpoint" },
          { "date": "2026-03-15", "label": "Mar PoC", "type": "PoC" },
          { "date": "2026-05-15", "label": "May Prod", "type": "Production" }
        ]
      },
      {
        "id": "M3",
        "label": "Dashboard & Reporting UI",
        "color": "#546E7A",
        "events": [
          { "date": "2026-02-01", "label": "Design Complete", "type": "Checkpoint" },
          { "date": "2026-04-10", "label": "Apr PoC", "type": "PoC" },
          { "date": "2026-06-01", "label": "Jun Prod", "type": "Production" }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "highlightColumn": "Apr",
    "shipped": {
      "items": {
        "Jan": ["Data schema v1", "Auth service"],
        "Feb": ["ETL pipeline alpha", "Logging framework"],
        "Mar": ["ML model v1", "API gateway", "Monitoring alerts"],
        "Apr": ["Customer segmentation", "Export API"]
      }
    },
    "inProgress": {
      "items": {
        "Jan": ["Pipeline design"],
        "Feb": ["Model training infra"],
        "Mar": ["Dashboard wireframes"],
        "Apr": ["Real-time scoring", "Dashboard build", "Load testing"]
      }
    },
    "carryover": {
      "items": {
        "Jan": [],
        "Feb": ["Data quality checks"],
        "Mar": ["SSO integration"],
        "Apr": ["SSO integration", "Perf optimization"]
      }
    },
    "blockers": {
      "items": {
        "Jan": [],
        "Feb": [],
        "Mar": ["GPU quota approval"],
        "Apr": ["Vendor contract delay"]
      }
    }
  }
}
```

## Appendix C: Comparable Open-Source Projects

| Project | Relevance | Notes |
|---------|-----------|-------|
| **Blazor Dashboard samples (Microsoft)** | Template reference | Microsoft's official Blazor dashboard samples show layout patterns but use Syncfusion/Telerik controls |
| **Blazor.Diagrams** | SVG rendering in Blazor | Shows how to do complex SVG in Blazor components; useful reference for timeline |
| **BlazorGantt** (community) | Gantt chart in Blazor | Overly complex for this use case but validates the approach of SVG-in-Blazor |
| **Excubo.Blazor.Canvas** | Canvas rendering | Alternative to SVG; not recommended (SVG is simpler for this design) |

**Bottom line:** No existing library does exactly what the reference design needs. The design is simple enough to build from scratch in Blazor, and that approach gives full pixel-level control — which is critical for screenshot-quality output.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/6ad8e65a18f3f93735ec5d1a69cb81ca4778bd5f/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
