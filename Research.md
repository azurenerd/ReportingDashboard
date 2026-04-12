# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-12 08:21 UTC_

### Summary

The project requires a simple, screenshot-optimized reporting dashboard for executive visibility into project milestones and progress. Given the mandatory C# .NET 8 with Blazor Server constraint, the recommended approach is a Blazor Server application that reads from a local JSON configuration file and renders an interactive yet print-friendly dashboard matching the OriginalDesignConcept.html design. This avoids unnecessary complexity while leveraging Blazor's component model for maintainability. The dashboard is designed for simplicity—no authentication, no cloud services, and minimal infrastructure requirements. The solution can be packaged as a self-contained Windows executable, making it trivial to distribute and run on any developer or executive machine.

### Key Findings

- **Blazor Server is ideal for this use case**: Interactive HTML rendering, C# code-behind, and built-in component reusability without the complexity of Blazor WebAssembly (WASM). Simpler deployment, no client-side compilation required.
- **JSON configuration approach is correct**: Eliminates database dependency, enables version control of data, and supports simple file-based updates without needing a SQL backend.
- **Design is CSS Grid/Flexbox compatible**: The OriginalDesignConcept.html uses modern CSS exclusively—no complex charting libraries needed. SVG timeline can be rendered with C# string interpolation or lightweight Razor components.
- **Print/screenshot optimization is achievable**: Blazor Server can generate static HTML that renders identically in browsers and print contexts. No special reporting framework (Crystal Reports, SSRS) needed.
- **Local-only architecture simplifies deployment**: No cloud infrastructure, no authentication layer, no distributed caching—just a single .sln project that compiles to a Windows Service or console app. Executable can be run directly or embedded in Windows Scheduler for automated report generation.
- **Team familiarity assumed**: C# .NET 8 and Blazor are primary stack—no new language or framework learning curve. **Goal**: Functional dashboard reading from `data.json`, matching OriginalDesignConcept.html design.
- **Create Blazor Server project**:
- Use `dotnet new blazorserver -n ReportingDashboard` (or create via Visual Studio).
- Target `net8.0`.
- Remove default Counter, Weather components; keep only MainLayout.
- **Define data models**:
- Implement `ReportData`, `Milestone`, `StatusRow` classes based on schema above.
- Add validation and defaults (e.g., empty items list if not provided).
- **Create `data.json`**:
- Hand-craft sample data for fictional project (e.g., "AgentSquad Privacy Module Release").
- 3–4 milestones, 4 status rows (Shipped, In Progress, Carryover, Blockers), 4–6 months of timeline.
- Place in `wwwroot/data/`.
- **Implement `ReportService`**:
- Load JSON on startup using `System.Text.Json`.
- Register as singleton in `Program.cs`.
- **Build `Dashboard.razor` component**:
- Compose `Header`, `Timeline`, `Heatmap` child components.
- Pass `ReportData` as cascading parameter.
- **Implement `Header.razor`**:
- Title, subtitle, ADO backlog link.
- Legend (4 icons: PoC, Prod, Checkpoint, Now).
- **Implement `Timeline.razor`**:
- Generate SVG for milestone lines, checkpoints, markers.
- Use C# to calculate x-coordinates based on dates.
- **Implement `Heatmap.razor` and `HeatmapCell.razor`**:
- Grid layout with CSS Grid.
- 4 status rows × ~6 month columns.
- Color classes from `StatusRow.CssClass` (e.g., `ship-cell`, `prog-cell`).
- **Style with `app.css`**:
- Copy CSS from OriginalDesignConcept.html.
- Enhance for print-friendliness (`@media print`).
- **Test & validate**:
- Manual browser test (F5 in Visual Studio).
- Verify layout at 1920x1080.
- Test print preview (Ctrl+P in browser).
- Screenshot for comparison with original design. **Goal**: Production-ready `.exe`, tested on target machines, documented for end-users.
- **Responsive design** (if needed):
- Add media queries for mobile/tablet (or document that desktop-only).
- **Unit tests**:
- Test `ReportService.LoadReportData()` with sample JSON.
- Test date-to-SVG-coordinate calculations in `Timeline`.
- Test empty/null data handling in `Heatmap`.
- **Build single-file `.exe`**:
- Update `.csproj` with publishing properties.
- Run `dotnet publish -c Release --self-contained -p:PublishSingleFile=true`.
- Output: `bin/Release/net8.0/publish/ReportingDashboard.exe`.
- **Create user guide**:
- How to run the `.exe`.
- How to edit `data.json` (schema, field descriptions).
- How to take screenshots for PowerPoint.
- **Deploy & test on target machine**:
- Copy `.exe` and `.json` to Windows machine (no admin required).
- Run `.exe`, verify dashboard loads in browser (localhost:5000 or similar).
- **File watching**: Auto-refresh dashboard when `data.json` changes (use `FileSystemWatcher`).
- **Multi-project support**: Load multiple `data.json` files, allow user to switch projects via dropdown.
- **Drill-down details**: Click milestone to show task list, dependencies, risk items.
- **PDF export**: Use Selenium or Puppeteer-sharp to automate screenshots, generate PDF reports.
- **Dark mode toggle**: CSS variable overrides for light/dark theme.
- **Real-time sync**: If data source moves to Azure DevOps API, add scheduled polling and auto-reload.
- **Analytics**: Track report views (e.g., log timestamp + user IP if security layer is added).
- **Hardcode sample data in component**: Skip `data.json` initially; inline ReportData in `Dashboard.razor` `@code` block. Validate layout before wiring file loading.
- **Use CSS Grid template from OriginalDesignConcept.html**: Copy styles directly to avoid layout bugs.
- **Generate SVG as static string**: For first iteration, hardcode SVG string in `Timeline.razor` based on sample milestones. Replace with dynamic generation after validating visual output.
- **Test print output early**: Take browser print preview screenshot; compare pixel-for-pixel with OriginalDesignConcept.html screenshot. Iterate CSS until pixel-perfect.
- [ ] Dashboard loads in browser without errors.
- [ ] Visual layout matches OriginalDesignConcept.html at 1920x1080.
- [ ] Print preview (Ctrl+P) renders correctly on A4/Letter paper.
- [ ] Screenshot can be pasted into PowerPoint slide without re-sizing.
- [ ] `data.json` can be edited, page reloads to reflect changes (manual F5 acceptable for MVP).
- [ ] Single `.exe` runs on Windows 10+ without dependencies.
- [ ] Unit tests pass (100% coverage of `ReportService` and `Timeline` calculations). --- **Document Version**: 1.0 **Created**: 2026-04-12 **Stack**: C# .NET 8 with Blazor Server **Target Deployment**: Windows local/intranet **Estimated Effort**: 2–3 weeks (MVP to production-ready `.exe`)

### Recommended Tools & Technologies

- **Blazor Server (ASP.NET Core 8.0)**: Interactive components, C# code-behind, hot reload support during development. No WASM complexity needed; server-side rendering is simpler and performant for a single-user or small team scenario.
- Version: 8.0.x (latest patch)
- Package: `Microsoft.AspNetCore.Components.Web`, `Microsoft.AspNetCore.Components.Server`
- Rationale: Built-in to .NET 8, mature, excellent for rapid development. Avoids need for separate frontend framework.
- **CSS (native, no framework)**: Use CSS Grid and Flexbox as demonstrated in OriginalDesignConcept.html. No Bootstrap, Tailwind, or Material UI needed—custom CSS is cleaner for a single-page layout and smaller payload.
- Approach: Single `app.css` file with class-based styling for heatmap, timeline, and header sections.
- **SVG rendering via C# Razor**: Generate SVG inline using Razor syntax for timeline milestones, checkpoints, and "now" indicator. No JavaScript charting library (Chart.js, D3, Plotly) required.
- Pattern: Use `MarkupString` in Blazor to render HTML/SVG from C# string builders.
- **ASP.NET Core 8.0 (Minimal APIs or MVC Controller)**: Trivial HTTP endpoints if interactivity is needed beyond page load. For a static dashboard reading JSON once on load, Minimal APIs are overkill—just use Blazor `@code` blocks for component logic.
- Version: 8.0.x
- Package: `Microsoft.AspNetCore.App`
- **System.Text.Json**: Native JSON deserialization in .NET 8. Minimal configuration, zero external dependencies.
- Version: Built-in to .NET 8 (8.0.x)
- Classes to define: `ReportData`, `Milestone`, `ProgressItem` (matching data.json schema)
- Rationale: No third-party library needed. Fast, memory-efficient.
- **JSON configuration file (data.json)**: Plain JSON file in application root or `wwwroot/` directory.
- Schema (inferred from OriginalDesignConcept.html):
    ```json
    {
      "reportTitle": "Privacy Automation Release Roadmap",
      "subtitle": "Trusted Platform – Privacy Automation Workstream – April 2026",
      "adoBacklogUrl": "#",
      "milestones": [
        {
          "id": "M1",
          "title": "Chatbot & MS Role",
          "color": "#0078D4",
          "startDate": "2026-01-12",
          "checkpoints": [...],
          "pocMilestone": { "date": "2026-03-26", ... },
          "productionRelease": { "date": "2026-05-01", ... }
        }
      ],
      "statusRows": [
        {
          "category": "Shipped",
          "cssClass": "ship-cell",
          "items": [
            { "month": "Jan", "value": "API v1 Released", ... },
            { "month": "Feb", "value": null, ... }
          ]
        },
        { "category": "In Progress", ... },
        { "category": "Carryover", ... },
        { "category": "Blockers", ... }
      ]
    }
    ```
- File location: `wwwroot/data/data.json` or appsettings.json (less preferred; JSON files allow easy editing without recompilation)
- File is loaded once on application startup or on-demand in Blazor component lifecycle.
- **File-based JSON**: No database. Read `data.json` on startup into memory. If real-time updates needed, watch file system with `FileSystemWatcher` and reload on change (optional feature, not MVP).
- Package (optional): `System.IO.FileSystemWatcher` (built-in to .NET Core)
- **appsettings.json**: Store non-sensitive configuration (e.g., data file path, title templates, color palette overrides).
- Version: Standard .NET 8 configuration provider.
- **xUnit**: Industry-standard unit testing framework for .NET. Test data loading, milestone calculations, CSV/JSON serialization.
- Package: `xunit`, `xunit.runner.visualstudio`
- Version: 2.6.x (latest stable)
- **Moq**: Mocking library for isolating component tests from file I/O and date/time.
- Package: `Moq`
- Version: 4.20.x
- **Blazor Unit Test Support**: `bunit` for testing Blazor components in isolation.
- Package: `bunit`, `bunit.web`
- Version: 1.26.x (latest stable for .NET 8)
- Use case: Test heatmap grid rendering, milestone timelines, null/empty data handling.
- **FluentAssertions**: Readable test assertions.
- Package: `FluentAssertions`
- Version: 6.12.x
- **.NET 8 SDK**: Required. Visual Studio 2022 (v17.9+) or Visual Studio Code with C# Dev Kit.
- Target Framework: `net8.0`
- **Project File (.csproj)**: Standard .NET 8 structure.
- Output type: `Exe` (Windows console app) or `WinExe` (hidden console). For distribution, consider `SelfContained=true` and `PublishSingleFile=true` to create a single .exe.
- **No CI/CD**: Project is local-only. Manual builds via Visual Studio or `dotnet publish -c Release` command line. If automation desired, GitHub Actions or Azure Pipelines can be used locally (build step only, no deployment to cloud).
- **No containerization required**: Windows executable is simpler to distribute and run than Docker for small team dashboards.
- **Serilog** (optional for future logging): If dashboard is run as a service or background job for report generation.
- Package: `Serilog`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`
- Version: 3.1.x
- MVP: Use standard `System.Diagnostics.Debug` and `Console.WriteLine()`.
- **Performance profiling**: Built-in to Visual Studio 2022. No external tools needed for a single-page application. **Model-View-Component (MVC-light)**: Use Blazor components as the V, C# models as the M, and component `@code` blocks for logic (similar to controller logic). No separate business logic layer needed for this simple dashboard.
```
AgentSquad.Runner/
├── Components/
│   ├── Dashboard.razor              (root dashboard component)
│   ├── Heatmap.razor                (status grid)
│   ├── Timeline.razor               (milestones SVG)
│   ├── Header.razor                 (title, subtitle, legend)
│   └── Shared/
│       └── MainLayout.razor
├── Models/
│   ├── ReportData.cs                (JSON schema classes)
│   ├── Milestone.cs
│   ├── ProgressItem.cs
│   └── StatusRow.cs
├── Services/
│   ├── ReportService.cs             (loads JSON, exposes data)
│   └── DesignConstants.cs           (colors, grid config, font settings)
├── wwwroot/
│   ├── data/
│   │   └── data.json                (dashboard data file)
│   ├── css/
│   │   ├── app.css                  (main styles matching OriginalDesignConcept.html)
│   │   └── heatmap.css              (heatmap grid styles)
│   ├── index.html                   (Blazor host page)
│   └── favicon.ico
├── Program.cs                       (Blazor Server startup config)
├── appsettings.json
└── AgentSquad.Runner.csproj
```
- **On startup**: `Program.cs` registers `ReportService` as scoped/singleton dependency.
- **On page load**: `Dashboard.razor` `OnInitializedAsync()` calls `ReportService.LoadReportDataAsync()`.
- **Service loads JSON**: `File.ReadAllTextAsync("wwwroot/data/data.json")` → `JsonSerializer.Deserialize<ReportData>()`.
- **Components render**: `Dashboard` composes `Header`, `Timeline`, and `Heatmap` as child components, passing `ReportData` as parameter.
- **CSS Grid renders**: `Heatmap.razor` iterates rows and columns from `ReportData.StatusRows`, outputting `<div>` grid items with class-based color coding.
- **SVG timeline renders**: `Timeline.razor` builds SVG string via C# for milestones, checkpoints, and "now" line.
```
Dashboard.razor
├── Header.razor (title, subtitle, legend)
├── Timeline.razor (SVG milestones, dates, PoC/Prod markers)
└── Heatmap.razor (grid: row headers + month columns + cells)
    └── HeatmapCell.razor (individual item cells with color class)
```
- **Single-file CSS**: `wwwroot/css/app.css` contains all styles from OriginalDesignConcept.html plus any enhancements.
- **BEM naming convention** (optional but recommended): `.hm-grid__cell`, `.hm-cell--shipped`, `.hm-cell--blocked` for clarity and maintainability.
- **CSS custom properties (variables)**: Define color palette in `:root` for easy theming.
  ```css
  :root {
    --color-shipped: #1B7A28;
    --color-shipped-bg: #E8F5E9;
    --color-progress: #1565C0;
    --color-progress-bg: #E3F2FD;
    --color-carryover: #B45309;
    --color-carryover-bg: #FFF8E1;
    --color-blocker: #991B1B;
    --color-blocker-bg: #FEF2F2;
  }
  ```
- **Print-friendly**: Include `@media print` rules to ensure layout is preserved in browser print dialog or screenshot. Set margins to 0, hide unnecessary UI elements, and ensure colors print correctly.
```csharp
public class ReportData
{
    public string ReportTitle { get; set; }
    public string Subtitle { get; set; }
    public string AdoBacklogUrl { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<StatusRow> StatusRows { get; set; } // Shipped, In Progress, Carryover, Blockers
}

public class Milestone
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Color { get; set; }
    public DateTime StartDate { get; set; }
    public List<Checkpoint> Checkpoints { get; set; }
    public MilestoneMarker PocMilestone { get; set; }
    public MilestoneMarker ProductionRelease { get; set; }
}

public class Checkpoint
{
    public DateTime Date { get; set; }
    public string Label { get; set; }
}

public class MilestoneMarker
{
    public DateTime Date { get; set; }
    public string Label { get; set; }
    public string Color { get; set; } // e.g., #F4B400 for PoC, #34A853 for Prod
}

public class StatusRow
{
    public string Category { get; set; } // "Shipped", "In Progress", "Carryover", "Blockers"
    public string CssClass { get; set; }
    public string HeaderColor { get; set; }
    public List<StatusItem> Items { get; set; }
}

public class StatusItem
{
    public string Month { get; set; }
    public string Value { get; set; } // null if cell is empty
}
``` Use C# to build SVG string dynamically based on `Milestone` data:
```csharp
private string GenerateTimelineSvg()
{
    var svgBuilder = new StringBuilder();
    svgBuilder.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1560\" height=\"185\">");
    
    foreach (var milestone in reportData.Milestones)
    {
        // Add month gridlines
        // Add milestone line
        // Add checkpoint circles
        // Add PoC and Prod markers (diamonds)
    }
    
    svgBuilder.Append("</svg>");
    return svgBuilder.ToString();
}
``` Then in `Timeline.razor`:
```razor
@((MarkupString)GenerateTimelineSvg())
```

### Considerations & Risks

- **Not applicable**: Dashboard is read-only, internal-use only, no user authentication required. If future versions need access control (e.g., restricted to certain teams), implement Windows Authentication via `[Authorize]` attributes and Active Directory integration—no OAuth/external IdP needed for local scenarios.
- **No sensitive data in data.json**: Dashboard displays project milestones and status only. No passwords, API keys, or PII.
- **File system security**: Place `data.json` in a restricted folder with read-only permissions if the application runs as a service or scheduled task. Standard Windows NTFS ACLs are sufficient.
- **HTTPS not required**: Local-only dashboard. HTTP is acceptable for localhost or intranet. If exposed beyond local network, enable HTTPS via self-signed certificate or organizational CA.
- **Windows executable**: Build as self-contained single-file `.exe` using project property `<PublishSingleFile>true</PublishSingleFile>` and `<SelfContained>true</SelfContained>`.
- Size: ~100-150 MB (includes .NET runtime).
- Distribution: Copy `.exe` to any Windows machine, run directly. No installation required.
- **Alternative: Windows Service**: If automated report generation needed (e.g., daily snapshots), host Blazor app in a Windows Service using `TopShelf` or .NET Worker Service template.
- Package: `Topshelf` (optional)
- Version: 4.3.x
- **Alternative: Scheduled task**: Host as console app, execute via Windows Task Scheduler to generate HTML snapshot on schedule.
- **IIS hosting (optional)**: For broader team access, deploy to IIS as an ASP.NET Core application (requires `.NET Runtime Hosting Bundle` on server). Not recommended for MVP—keep it simple with standalone `.exe`.
- **Infrastructure costs**: Zero. No cloud services, no database, no CDN. Only cost is developer time and local disk space (<1 GB).
- **File-based data not scalable to high concurrency**: If 100+ users simultaneously read `data.json`, file I/O becomes bottleneck. Mitigation: Cache in memory after first load. For MVP, acceptable risk (single user or small team).
- **No data versioning or audit trail**: Changing `data.json` overwrites history. Mitigation: Use Git to version data.json; commit changes with descriptive messages. Or implement simple file backup on load.
- **Date calculations depend on system clock**: SVG timeline positions based on `DateTime` objects. If system date/time is incorrect, timeline will be misaligned. Mitigation: Use `DateTimeOffset` for timezone awareness; validate dates in tests.
- **CSS Grid browser compatibility**: Layout depends on CSS Grid support. Affects IE 11 (not supported) but fine for modern browsers (Edge, Chrome, Firefox). Mitigation: Target modern browsers only; no polyfills for IE.
- **Blazor Server requires WebSocket connection**: If running on restricted network or behind proxy, WebSocket may be blocked. Mitigation: Test on target network early; fall back to long-polling if needed (Blazor supports this natively).
- **Single-threaded file access**: `FileSystemWatcher` may miss rapid updates to `data.json`. Not a concern for manual edits; only relevant if another process writes to file simultaneously.
- **Browser tab limit**: Each browser tab opens a separate WebSocket. If dashboard is left open for weeks, memory may accumulate. Mitigation: Add auto-refresh or cache-busting query parameter to force reload.
- **SVG rendering performance**: If timeline extends beyond 12 months or milestones exceed 10, SVG string concatenation may slow. Mitigation: Use StringBuilder and benchmark; consider lazy-rendering off-screen months.
- **Team familiarity**: Assumes C# and Blazor knowledge. Learning curve is minimal if team has .NET experience.
- **CSS Grid knowledge**: Required for customizing heatmap layout. Not difficult; MDN documentation is excellent.
- **SVG coordinate math**: Generating timeline SVG requires understanding x/y positioning. Encapsulate in helper methods to reduce complexity. | Decision | Upside | Downside | Mitigation | |----------|--------|----------|------------| | **JSON file vs. database** | Simple, version control friendly, zero infrastructure | No ACID guarantees, manual locking for concurrent writes | Lock file during writes, or accept data loss risk for MVP | | **Blazor Server vs. Blazor WASM** | Simpler deployment, real-time updates, less client-side code | Requires .NET runtime on server | Fine for local/intranet; not web-scale | | **Custom CSS vs. framework** | Smaller payload, pixel-perfect design match | Manual responsive design, no pre-built components | Use CSS Grid/Flexbox; test on target resolutions | | **SVG via C# vs. charting library** | No external dependencies, lightweight, matches design | Manual coordinate math, harder to animate or add interactivity | Use hardcoded SVG template for MVP; refactor later if needed | | **No authentication** | Zero complexity, fast login | Anyone with URL/app can access. Security relies on network isolation. | Keep on intranet only; add auth layer if exposed |
- **How often will `data.json` be updated?** Manual once-a-week edit, automated import from ADO, or real-time sync? Answer determines file watching and caching strategy.
- **How many months of timeline history should be displayed?** Current design shows Jan–Jun. Does scope expand to 12+ months? Impacts SVG size and UI layout.
- **How many projects/dashboards?** Single dashboard for one project, or multi-tenancy with URL-based project selection (e.g., `/dashboard/privacy-automation`)? Affects `data.json` schema and file organization.
- **Print/screenshot resolution**: Current design is 1920x1080. Should be responsive to smaller screens (1024x768, 1366x768)? Or is fixed-size acceptable for PowerPoint use?
- **Interactivity level**: Static read-only view, or should users drill down (click milestone to see details), export to PDF, or edit data in-browser?
- **Color accessibility**: Should contrast ratios meet WCAG AA or AAA standards? Current design uses vibrant colors; may fail accessibility checks.
- **Browser support**: Only modern Chrome/Edge/Firefox, or legacy IE 11? (Blazor Server doesn't support IE 11.)
- **Automated report generation**: Should dashboard be run manually, scheduled daily/weekly, or exposed as a web service for on-demand access?

### Detailed Analysis

# Research: Executive Reporting Dashboard - Technology Stack & Architecture

## Executive Summary

The project requires a simple, screenshot-optimized reporting dashboard for executive visibility into project milestones and progress. Given the mandatory C# .NET 8 with Blazor Server constraint, the recommended approach is a Blazor Server application that reads from a local JSON configuration file and renders an interactive yet print-friendly dashboard matching the OriginalDesignConcept.html design. This avoids unnecessary complexity while leveraging Blazor's component model for maintainability. The dashboard is designed for simplicity—no authentication, no cloud services, and minimal infrastructure requirements. The solution can be packaged as a self-contained Windows executable, making it trivial to distribute and run on any developer or executive machine.

## Key Findings

- **Blazor Server is ideal for this use case**: Interactive HTML rendering, C# code-behind, and built-in component reusability without the complexity of Blazor WebAssembly (WASM). Simpler deployment, no client-side compilation required.
- **JSON configuration approach is correct**: Eliminates database dependency, enables version control of data, and supports simple file-based updates without needing a SQL backend.
- **Design is CSS Grid/Flexbox compatible**: The OriginalDesignConcept.html uses modern CSS exclusively—no complex charting libraries needed. SVG timeline can be rendered with C# string interpolation or lightweight Razor components.
- **Print/screenshot optimization is achievable**: Blazor Server can generate static HTML that renders identically in browsers and print contexts. No special reporting framework (Crystal Reports, SSRS) needed.
- **Local-only architecture simplifies deployment**: No cloud infrastructure, no authentication layer, no distributed caching—just a single .sln project that compiles to a Windows Service or console app. Executable can be run directly or embedded in Windows Scheduler for automated report generation.
- **Team familiarity assumed**: C# .NET 8 and Blazor are primary stack—no new language or framework learning curve.

## Recommended Technology Stack

### Frontend Layer
- **Blazor Server (ASP.NET Core 8.0)**: Interactive components, C# code-behind, hot reload support during development. No WASM complexity needed; server-side rendering is simpler and performant for a single-user or small team scenario.
  - Version: 8.0.x (latest patch)
  - Package: `Microsoft.AspNetCore.Components.Web`, `Microsoft.AspNetCore.Components.Server`
  - Rationale: Built-in to .NET 8, mature, excellent for rapid development. Avoids need for separate frontend framework.

- **CSS (native, no framework)**: Use CSS Grid and Flexbox as demonstrated in OriginalDesignConcept.html. No Bootstrap, Tailwind, or Material UI needed—custom CSS is cleaner for a single-page layout and smaller payload.
  - Approach: Single `app.css` file with class-based styling for heatmap, timeline, and header sections.

- **SVG rendering via C# Razor**: Generate SVG inline using Razor syntax for timeline milestones, checkpoints, and "now" indicator. No JavaScript charting library (Chart.js, D3, Plotly) required.
  - Pattern: Use `MarkupString` in Blazor to render HTML/SVG from C# string builders.

### Backend / Business Logic Layer
- **ASP.NET Core 8.0 (Minimal APIs or MVC Controller)**: Trivial HTTP endpoints if interactivity is needed beyond page load. For a static dashboard reading JSON once on load, Minimal APIs are overkill—just use Blazor `@code` blocks for component logic.
  - Version: 8.0.x
  - Package: `Microsoft.AspNetCore.App`

- **System.Text.Json**: Native JSON deserialization in .NET 8. Minimal configuration, zero external dependencies.
  - Version: Built-in to .NET 8 (8.0.x)
  - Classes to define: `ReportData`, `Milestone`, `ProgressItem` (matching data.json schema)
  - Rationale: No third-party library needed. Fast, memory-efficient.

- **JSON configuration file (data.json)**: Plain JSON file in application root or `wwwroot/` directory.
  - Schema (inferred from OriginalDesignConcept.html):
    ```json
    {
      "reportTitle": "Privacy Automation Release Roadmap",
      "subtitle": "Trusted Platform – Privacy Automation Workstream – April 2026",
      "adoBacklogUrl": "#",
      "milestones": [
        {
          "id": "M1",
          "title": "Chatbot & MS Role",
          "color": "#0078D4",
          "startDate": "2026-01-12",
          "checkpoints": [...],
          "pocMilestone": { "date": "2026-03-26", ... },
          "productionRelease": { "date": "2026-05-01", ... }
        }
      ],
      "statusRows": [
        {
          "category": "Shipped",
          "cssClass": "ship-cell",
          "items": [
            { "month": "Jan", "value": "API v1 Released", ... },
            { "month": "Feb", "value": null, ... }
          ]
        },
        { "category": "In Progress", ... },
        { "category": "Carryover", ... },
        { "category": "Blockers", ... }
      ]
    }
    ```
  - File location: `wwwroot/data/data.json` or appsettings.json (less preferred; JSON files allow easy editing without recompilation)
  - File is loaded once on application startup or on-demand in Blazor component lifecycle.

### Data Persistence & Configuration
- **File-based JSON**: No database. Read `data.json` on startup into memory. If real-time updates needed, watch file system with `FileSystemWatcher` and reload on change (optional feature, not MVP).
  - Package (optional): `System.IO.FileSystemWatcher` (built-in to .NET Core)

- **appsettings.json**: Store non-sensitive configuration (e.g., data file path, title templates, color palette overrides).
  - Version: Standard .NET 8 configuration provider.

### Testing & Quality Assurance
- **xUnit**: Industry-standard unit testing framework for .NET. Test data loading, milestone calculations, CSV/JSON serialization.
  - Package: `xunit`, `xunit.runner.visualstudio`
  - Version: 2.6.x (latest stable)

- **Moq**: Mocking library for isolating component tests from file I/O and date/time.
  - Package: `Moq`
  - Version: 4.20.x

- **Blazor Unit Test Support**: `bunit` for testing Blazor components in isolation.
  - Package: `bunit`, `bunit.web`
  - Version: 1.26.x (latest stable for .NET 8)
  - Use case: Test heatmap grid rendering, milestone timelines, null/empty data handling.

- **FluentAssertions**: Readable test assertions.
  - Package: `FluentAssertions`
  - Version: 6.12.x

### Build & Deployment
- **.NET 8 SDK**: Required. Visual Studio 2022 (v17.9+) or Visual Studio Code with C# Dev Kit.
  - Target Framework: `net8.0`

- **Project File (.csproj)**: Standard .NET 8 structure.
  - Output type: `Exe` (Windows console app) or `WinExe` (hidden console). For distribution, consider `SelfContained=true` and `PublishSingleFile=true` to create a single .exe.

- **No CI/CD**: Project is local-only. Manual builds via Visual Studio or `dotnet publish -c Release` command line. If automation desired, GitHub Actions or Azure Pipelines can be used locally (build step only, no deployment to cloud).

- **No containerization required**: Windows executable is simpler to distribute and run than Docker for small team dashboards.

### Monitoring & Observability (Optional)
- **Serilog** (optional for future logging): If dashboard is run as a service or background job for report generation.
  - Package: `Serilog`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`
  - Version: 3.1.x
  - MVP: Use standard `System.Diagnostics.Debug` and `Console.WriteLine()`.

- **Performance profiling**: Built-in to Visual Studio 2022. No external tools needed for a single-page application.

## Architecture Recommendations

### Overall Design Pattern
**Model-View-Component (MVC-light)**: Use Blazor components as the V, C# models as the M, and component `@code` blocks for logic (similar to controller logic). No separate business logic layer needed for this simple dashboard.

### Project Structure (Single .sln)
```
AgentSquad.Runner/
├── Components/
│   ├── Dashboard.razor              (root dashboard component)
│   ├── Heatmap.razor                (status grid)
│   ├── Timeline.razor               (milestones SVG)
│   ├── Header.razor                 (title, subtitle, legend)
│   └── Shared/
│       └── MainLayout.razor
├── Models/
│   ├── ReportData.cs                (JSON schema classes)
│   ├── Milestone.cs
│   ├── ProgressItem.cs
│   └── StatusRow.cs
├── Services/
│   ├── ReportService.cs             (loads JSON, exposes data)
│   └── DesignConstants.cs           (colors, grid config, font settings)
├── wwwroot/
│   ├── data/
│   │   └── data.json                (dashboard data file)
│   ├── css/
│   │   ├── app.css                  (main styles matching OriginalDesignConcept.html)
│   │   └── heatmap.css              (heatmap grid styles)
│   ├── index.html                   (Blazor host page)
│   └── favicon.ico
├── Program.cs                       (Blazor Server startup config)
├── appsettings.json
└── AgentSquad.Runner.csproj
```

### Data Flow
1. **On startup**: `Program.cs` registers `ReportService` as scoped/singleton dependency.
2. **On page load**: `Dashboard.razor` `OnInitializedAsync()` calls `ReportService.LoadReportDataAsync()`.
3. **Service loads JSON**: `File.ReadAllTextAsync("wwwroot/data/data.json")` → `JsonSerializer.Deserialize<ReportData>()`.
4. **Components render**: `Dashboard` composes `Header`, `Timeline`, and `Heatmap` as child components, passing `ReportData` as parameter.
5. **CSS Grid renders**: `Heatmap.razor` iterates rows and columns from `ReportData.StatusRows`, outputting `<div>` grid items with class-based color coding.
6. **SVG timeline renders**: `Timeline.razor` builds SVG string via C# for milestones, checkpoints, and "now" line.

### Component Hierarchy
```
Dashboard.razor
├── Header.razor (title, subtitle, legend)
├── Timeline.razor (SVG milestones, dates, PoC/Prod markers)
└── Heatmap.razor (grid: row headers + month columns + cells)
    └── HeatmapCell.razor (individual item cells with color class)
```

### Styling Approach
- **Single-file CSS**: `wwwroot/css/app.css` contains all styles from OriginalDesignConcept.html plus any enhancements.
- **BEM naming convention** (optional but recommended): `.hm-grid__cell`, `.hm-cell--shipped`, `.hm-cell--blocked` for clarity and maintainability.
- **CSS custom properties (variables)**: Define color palette in `:root` for easy theming.
  ```css
  :root {
    --color-shipped: #1B7A28;
    --color-shipped-bg: #E8F5E9;
    --color-progress: #1565C0;
    --color-progress-bg: #E3F2FD;
    --color-carryover: #B45309;
    --color-carryover-bg: #FFF8E1;
    --color-blocker: #991B1B;
    --color-blocker-bg: #FEF2F2;
  }
  ```
- **Print-friendly**: Include `@media print` rules to ensure layout is preserved in browser print dialog or screenshot. Set margins to 0, hide unnecessary UI elements, and ensure colors print correctly.

### Data Model Design
```csharp
public class ReportData
{
    public string ReportTitle { get; set; }
    public string Subtitle { get; set; }
    public string AdoBacklogUrl { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<StatusRow> StatusRows { get; set; } // Shipped, In Progress, Carryover, Blockers
}

public class Milestone
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Color { get; set; }
    public DateTime StartDate { get; set; }
    public List<Checkpoint> Checkpoints { get; set; }
    public MilestoneMarker PocMilestone { get; set; }
    public MilestoneMarker ProductionRelease { get; set; }
}

public class Checkpoint
{
    public DateTime Date { get; set; }
    public string Label { get; set; }
}

public class MilestoneMarker
{
    public DateTime Date { get; set; }
    public string Label { get; set; }
    public string Color { get; set; } // e.g., #F4B400 for PoC, #34A853 for Prod
}

public class StatusRow
{
    public string Category { get; set; } // "Shipped", "In Progress", "Carryover", "Blockers"
    public string CssClass { get; set; }
    public string HeaderColor { get; set; }
    public List<StatusItem> Items { get; set; }
}

public class StatusItem
{
    public string Month { get; set; }
    public string Value { get; set; } // null if cell is empty
}
```

### Timeline SVG Generation
Use C# to build SVG string dynamically based on `Milestone` data:
```csharp
private string GenerateTimelineSvg()
{
    var svgBuilder = new StringBuilder();
    svgBuilder.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1560\" height=\"185\">");
    
    foreach (var milestone in reportData.Milestones)
    {
        // Add month gridlines
        // Add milestone line
        // Add checkpoint circles
        // Add PoC and Prod markers (diamonds)
    }
    
    svgBuilder.Append("</svg>");
    return svgBuilder.ToString();
}
```

Then in `Timeline.razor`:
```razor
@((MarkupString)GenerateTimelineSvg())
```

## Security & Infrastructure

### Authentication & Authorization
**Not applicable**: Dashboard is read-only, internal-use only, no user authentication required. If future versions need access control (e.g., restricted to certain teams), implement Windows Authentication via `[Authorize]` attributes and Active Directory integration—no OAuth/external IdP needed for local scenarios.

### Data Protection
- **No sensitive data in data.json**: Dashboard displays project milestones and status only. No passwords, API keys, or PII.
- **File system security**: Place `data.json` in a restricted folder with read-only permissions if the application runs as a service or scheduled task. Standard Windows NTFS ACLs are sufficient.
- **HTTPS not required**: Local-only dashboard. HTTP is acceptable for localhost or intranet. If exposed beyond local network, enable HTTPS via self-signed certificate or organizational CA.

### Hosting & Deployment
- **Windows executable**: Build as self-contained single-file `.exe` using project property `<PublishSingleFile>true</PublishSingleFile>` and `<SelfContained>true</SelfContained>`.
  - Size: ~100-150 MB (includes .NET runtime).
  - Distribution: Copy `.exe` to any Windows machine, run directly. No installation required.

- **Alternative: Windows Service**: If automated report generation needed (e.g., daily snapshots), host Blazor app in a Windows Service using `TopShelf` or .NET Worker Service template.
  - Package: `Topshelf` (optional)
  - Version: 4.3.x

- **Alternative: Scheduled task**: Host as console app, execute via Windows Task Scheduler to generate HTML snapshot on schedule.

- **IIS hosting (optional)**: For broader team access, deploy to IIS as an ASP.NET Core application (requires `.NET Runtime Hosting Bundle` on server). Not recommended for MVP—keep it simple with standalone `.exe`.

- **Infrastructure costs**: Zero. No cloud services, no database, no CDN. Only cost is developer time and local disk space (<1 GB).

## Risks & Trade-offs

### Technical Risks
1. **File-based data not scalable to high concurrency**: If 100+ users simultaneously read `data.json`, file I/O becomes bottleneck. Mitigation: Cache in memory after first load. For MVP, acceptable risk (single user or small team).

2. **No data versioning or audit trail**: Changing `data.json` overwrites history. Mitigation: Use Git to version data.json; commit changes with descriptive messages. Or implement simple file backup on load.

3. **Date calculations depend on system clock**: SVG timeline positions based on `DateTime` objects. If system date/time is incorrect, timeline will be misaligned. Mitigation: Use `DateTimeOffset` for timezone awareness; validate dates in tests.

4. **CSS Grid browser compatibility**: Layout depends on CSS Grid support. Affects IE 11 (not supported) but fine for modern browsers (Edge, Chrome, Firefox). Mitigation: Target modern browsers only; no polyfills for IE.

5. **Blazor Server requires WebSocket connection**: If running on restricted network or behind proxy, WebSocket may be blocked. Mitigation: Test on target network early; fall back to long-polling if needed (Blazor supports this natively).

### Scalability Bottlenecks
- **Single-threaded file access**: `FileSystemWatcher` may miss rapid updates to `data.json`. Not a concern for manual edits; only relevant if another process writes to file simultaneously.
- **Browser tab limit**: Each browser tab opens a separate WebSocket. If dashboard is left open for weeks, memory may accumulate. Mitigation: Add auto-refresh or cache-busting query parameter to force reload.
- **SVG rendering performance**: If timeline extends beyond 12 months or milestones exceed 10, SVG string concatenation may slow. Mitigation: Use StringBuilder and benchmark; consider lazy-rendering off-screen months.

### Skills & Learning Curve
- **Team familiarity**: Assumes C# and Blazor knowledge. Learning curve is minimal if team has .NET experience.
- **CSS Grid knowledge**: Required for customizing heatmap layout. Not difficult; MDN documentation is excellent.
- **SVG coordinate math**: Generating timeline SVG requires understanding x/y positioning. Encapsulate in helper methods to reduce complexity.

### Trade-offs
| Decision | Upside | Downside | Mitigation |
|----------|--------|----------|------------|
| **JSON file vs. database** | Simple, version control friendly, zero infrastructure | No ACID guarantees, manual locking for concurrent writes | Lock file during writes, or accept data loss risk for MVP |
| **Blazor Server vs. Blazor WASM** | Simpler deployment, real-time updates, less client-side code | Requires .NET runtime on server | Fine for local/intranet; not web-scale |
| **Custom CSS vs. framework** | Smaller payload, pixel-perfect design match | Manual responsive design, no pre-built components | Use CSS Grid/Flexbox; test on target resolutions |
| **SVG via C# vs. charting library** | No external dependencies, lightweight, matches design | Manual coordinate math, harder to animate or add interactivity | Use hardcoded SVG template for MVP; refactor later if needed |
| **No authentication** | Zero complexity, fast login | Anyone with URL/app can access. Security relies on network isolation. | Keep on intranet only; add auth layer if exposed |

## Open Questions

1. **How often will `data.json` be updated?** Manual once-a-week edit, automated import from ADO, or real-time sync? Answer determines file watching and caching strategy.

2. **How many months of timeline history should be displayed?** Current design shows Jan–Jun. Does scope expand to 12+ months? Impacts SVG size and UI layout.

3. **How many projects/dashboards?** Single dashboard for one project, or multi-tenancy with URL-based project selection (e.g., `/dashboard/privacy-automation`)? Affects `data.json` schema and file organization.

4. **Print/screenshot resolution**: Current design is 1920x1080. Should be responsive to smaller screens (1024x768, 1366x768)? Or is fixed-size acceptable for PowerPoint use?

5. **Interactivity level**: Static read-only view, or should users drill down (click milestone to see details), export to PDF, or edit data in-browser?

6. **Color accessibility**: Should contrast ratios meet WCAG AA or AAA standards? Current design uses vibrant colors; may fail accessibility checks.

7. **Browser support**: Only modern Chrome/Edge/Firefox, or legacy IE 11? (Blazor Server doesn't support IE 11.)

8. **Automated report generation**: Should dashboard be run manually, scheduled daily/weekly, or exposed as a web service for on-demand access?

## Implementation Recommendations

### Phase 1: MVP (Week 1–2)
**Goal**: Functional dashboard reading from `data.json`, matching OriginalDesignConcept.html design.

1. **Create Blazor Server project**:
   - Use `dotnet new blazorserver -n ReportingDashboard` (or create via Visual Studio).
   - Target `net8.0`.
   - Remove default Counter, Weather components; keep only MainLayout.

2. **Define data models**:
   - Implement `ReportData`, `Milestone`, `StatusRow` classes based on schema above.
   - Add validation and defaults (e.g., empty items list if not provided).

3. **Create `data.json`**:
   - Hand-craft sample data for fictional project (e.g., "AgentSquad Privacy Module Release").
   - 3–4 milestones, 4 status rows (Shipped, In Progress, Carryover, Blockers), 4–6 months of timeline.
   - Place in `wwwroot/data/`.

4. **Implement `ReportService`**:
   - Load JSON on startup using `System.Text.Json`.
   - Register as singleton in `Program.cs`.

5. **Build `Dashboard.razor` component**:
   - Compose `Header`, `Timeline`, `Heatmap` child components.
   - Pass `ReportData` as cascading parameter.

6. **Implement `Header.razor`**:
   - Title, subtitle, ADO backlog link.
   - Legend (4 icons: PoC, Prod, Checkpoint, Now).

7. **Implement `Timeline.razor`**:
   - Generate SVG for milestone lines, checkpoints, markers.
   - Use C# to calculate x-coordinates based on dates.

8. **Implement `Heatmap.razor` and `HeatmapCell.razor`**:
   - Grid layout with CSS Grid.
   - 4 status rows × ~6 month columns.
   - Color classes from `StatusRow.CssClass` (e.g., `ship-cell`, `prog-cell`).

9. **Style with `app.css`**:
   - Copy CSS from OriginalDesignConcept.html.
   - Enhance for print-friendliness (`@media print`).

10. **Test & validate**:
    - Manual browser test (F5 in Visual Studio).
    - Verify layout at 1920x1080.
    - Test print preview (Ctrl+P in browser).
    - Screenshot for comparison with original design.

### Phase 2: Polish & Deployment (Week 3)
**Goal**: Production-ready `.exe`, tested on target machines, documented for end-users.

1. **Responsive design** (if needed):
   - Add media queries for mobile/tablet (or document that desktop-only).

2. **Unit tests**:
   - Test `ReportService.LoadReportData()` with sample JSON.
   - Test date-to-SVG-coordinate calculations in `Timeline`.
   - Test empty/null data handling in `Heatmap`.

3. **Build single-file `.exe`**:
   - Update `.csproj` with publishing properties.
   - Run `dotnet publish -c Release --self-contained -p:PublishSingleFile=true`.
   - Output: `bin/Release/net8.0/publish/ReportingDashboard.exe`.

4. **Create user guide**:
   - How to run the `.exe`.
   - How to edit `data.json` (schema, field descriptions).
   - How to take screenshots for PowerPoint.

5. **Deploy & test on target machine**:
   - Copy `.exe` and `.json` to Windows machine (no admin required).
   - Run `.exe`, verify dashboard loads in browser (localhost:5000 or similar).

### Phase 3: Future Enhancements (Post-MVP)
- **File watching**: Auto-refresh dashboard when `data.json` changes (use `FileSystemWatcher`).
- **Multi-project support**: Load multiple `data.json` files, allow user to switch projects via dropdown.
- **Drill-down details**: Click milestone to show task list, dependencies, risk items.
- **PDF export**: Use Selenium or Puppeteer-sharp to automate screenshots, generate PDF reports.
- **Dark mode toggle**: CSS variable overrides for light/dark theme.
- **Real-time sync**: If data source moves to Azure DevOps API, add scheduled polling and auto-reload.
- **Analytics**: Track report views (e.g., log timestamp + user IP if security layer is added).

### Quick Wins for Early Validation
1. **Hardcode sample data in component**: Skip `data.json` initially; inline ReportData in `Dashboard.razor` `@code` block. Validate layout before wiring file loading.
2. **Use CSS Grid template from OriginalDesignConcept.html**: Copy styles directly to avoid layout bugs.
3. **Generate SVG as static string**: For first iteration, hardcode SVG string in `Timeline.razor` based on sample milestones. Replace with dynamic generation after validating visual output.
4. **Test print output early**: Take browser print preview screenshot; compare pixel-for-pixel with OriginalDesignConcept.html screenshot. Iterate CSS until pixel-perfect.

### Success Criteria
- [ ] Dashboard loads in browser without errors.
- [ ] Visual layout matches OriginalDesignConcept.html at 1920x1080.
- [ ] Print preview (Ctrl+P) renders correctly on A4/Letter paper.
- [ ] Screenshot can be pasted into PowerPoint slide without re-sizing.
- [ ] `data.json` can be edited, page reloads to reflect changes (manual F5 acceptable for MVP).
- [ ] Single `.exe` runs on Windows 10+ without dependencies.
- [ ] Unit tests pass (100% coverage of `ReportService` and `Timeline` calculations).

---

**Document Version**: 1.0  
**Created**: 2026-04-12  
**Stack**: C# .NET 8 with Blazor Server  
**Target Deployment**: Windows local/intranet  
**Estimated Effort**: 2–3 weeks (MVP to production-ready `.exe`)

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/7cef72562b505c4f51f8825488b386e0d4161e91/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
