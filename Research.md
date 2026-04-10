# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-10 07:50 UTC_

### Summary

The Executive Dashboard project requires a lightweight, screenshot-friendly web application for reporting project milestones and progress to stakeholders. Leveraging C# .NET 8 with Blazor Server, the recommended approach is a hybrid rendering model combining static HTML generation from JSON configuration with reactive Blazor components for file watching. This architecture prioritizes simplicity, maintainability, and visual fidelity for PowerPoint extraction over enterprise-grade complexity. The stack emphasizes built-in .NET libraries and free, actively maintained open-source packages (MudBlazor, ChartJS.Blazor, System.Text.Json) to minimize dependencies and licensing overhead. Create Blazor Server project with MudBlazor integration. Implement DashboardDataService (load data.json, basic error handling, no file watching yet). Build Dashboard.razor page with semantic HTML layout (no charts, static HTML only). Create sample data.json with fictional project data. Add basic CSS for fixed-width layout (1200px), light theme, typography. Write unit tests for DashboardDataService (JSON parsing, error cases).

### Key Findings

- Blazor Server's server-side execution eliminates cross-origin concerns and enables direct filesystem access to data.json without permission overhead.
- MudBlazor v7.0+ (MIT licensed) is the appropriate UI component library, offering professional dashboard composition without commercial licensing constraints.
- System.Text.Json (native to .NET 8) paired with FileSystemWatcher provides efficient JSON parsing and real-time file monitoring without external dependencies.
- Static HTML rendering wrapped in a Blazor component delivers the simplicity required for screenshot-based PowerPoint workflows while maintaining reactivity on data file changes.
- Custom SVG/CSS timeline is superior to third-party Gantt libraries for milestone visualization, providing maximum control over aesthetics and minimal overhead.
- Singleton service pattern with component-level state management is sufficient; external state libraries (Redux, MediatR, Fluxor) add unjustified complexity.
- Fixed-width layout optimized for 1920x1080 desktop resolution with light theme is the correct approach; responsive mobile design is unnecessary overhead.
- File-based data updates (via data.json writes) require debounced FileSystemWatcher logic to prevent race conditions from atomic file writes.
- Run `dotnet new blazorserver` scaffold to start project.
- Copy MudBlazor components from official Blazor server template.
- Use Razor @for loops to render milestone/work item lists from data.json.
- Integrate ChartJS.Blazor for status bar chart (Shipped/InProgress/CarriedOver).
- Implement custom SVG timeline component for milestones.
- Add FileSystemWatcher to DashboardDataService with 500ms debounce.
- Connect FileSystemWatcher events to Blazor component re-rendering.
- Test file watching with manual data.json edits.
- Review design against OriginalDesignConcept.html and ReportingDashboardDesign.png.
- Fine-tune CSS (colors, spacing, fonts) for executive aesthetics.
- Test screenshot capture (browser F12, compare across Chrome/Edge).
- Implement error boundary component for JSON parse failures.
- Add health check endpoint (`/health`) for deployment monitoring.
- Document deployment steps (copy data.json, configure appsettings.json, run app).
- Multi-dashboard support (separate data.json per project).
- Historical trending (archive previous data.json snapshots, compare over time).
- Automated screenshot generation (PuppeteerSharp integration).
- Database backend (replace data.json with relational schema).
- Deployment to cloud (Azure App Service, AWS EC2).
- Authentication (OAuth2, SAML).

### Recommended Tools & Technologies

- **Blazor Server** (ASP.NET Core 8.0) - Server-side rendering framework, built into .NET 8.
- **MudBlazor v7.0+** - Blazor component library (MIT licensed). Provides Cards, Containers, Grids, Icons, Typography.
- **ChartJS.Blazor v4.1.2** - JavaScript interop wrapper for Chart.js. Enables bar/pie/line charts for status visualization.
- **System.Text.Json** (built-in to .NET 8) - JSON serialization/deserialization. Fast, zero-dependency alternative to Newtonsoft.Json.
- **.NET 8 Runtime** - Built-in System.IO, System.IO.FileSystemWatcher, System.Collections.Concurrent namespaces.
- **ASP.NET Core 8.0 Dependency Injection** - Built-in service registration and lifecycle management.
- **IOptions<T> pattern** (Microsoft.Extensions.Configuration) - Configuration management for app settings.
- **data.json** (local filesystem) - Source of truth for dashboard data. Plain JSON file, schema-validated via C# models.
- **No relational database required** - Dashboard is read-only, file-based updates only.
- **xUnit 2.6+** - Unit testing framework for C#.
- **Moq 4.20+** - Mocking library for isolating DashboardDataService tests.
- **No additional linting/formatting required** - Use Visual Studio's built-in Roslyn analyzers.
- **Local deployment only** - No cloud services, CDN, or containerization required.
- **Windows, Linux, macOS compatibility** - .NET 8 is cross-platform; use Path.Combine() for platform-agnostic paths.
- **Visual Studio 2022 (17.8+)** or **VS Code + OmniSharp** - IDE with Blazor debugging support.
- **git** - Version control for .sln project structure.
- Use a **hybrid static-dynamic rendering model**:
- **Static generation layer**: DashboardDataService parses data.json into strongly-typed models (Project, Milestone[], WorkItem[]).
- **HTML generation**: Helper methods convert models to semantic HTML (no JSX, pure C# string builders or Razor templates).
- **Blazor component wrapper**: Dashboard.razor wraps generated HTML, watches data.json via FileSystemWatcher, triggers StateHasChanged() on file change.
- **Interactive charts**: ChartJS.Blazor components for progress/status charts (bar, pie). Custom SVG for timeline.
- ```
- MyProject.sln
- ├── MyProject.Web/                    # Blazor Server project
- │   ├── Pages/
- │   │   ├── Index.razor               # Home page
- │   │   └── Dashboard.razor            # Executive dashboard page
- │   ├── Models/
- │   │   ├── DashboardData.cs           # Root DTO
- │   │   ├── Milestone.cs               # Milestone record
- │   │   └── WorkItem.cs                # Work item (Shipped/InProgress/CarriedOver)
- │   ├── Services/
- │   │   └── DashboardDataService.cs    # Loads, watches, caches data.json
- │   ├── Components/
- │   │   ├── MilestoneTimeline.razor    # SVG timeline renderer
- │   │   └── StatusChart.razor          # ChartJS.Blazor wrapper
- │   ├── wwwroot/
- │   │   ├── css/
- │   │   │   └── dashboard.css          # Layout, typography, theme
- │   │   └── js/
- │   │       └── chartjs-interop.js     # ChartJS setup (minimal)
- │   ├── appsettings.json               # Logging, data.json path config
- │   ├── Program.cs                     # Dependency injection, host config
- │   └── data.json                      # Dashboard data (copied to bin/ at build)
- └── MyProject.Tests/                   # xUnit test project
- └── Services/
- └── DashboardDataServiceTests.cs
- ```
- **Application startup**: DashboardDataService reads data.json from AppContext.BaseDirectory.
- **Parsing**: System.Text.Json deserializes JSON into strongly-typed models; validation errors logged.
- **Caching**: Parsed models stored in private fields; public properties expose read-only views.
- **File watching**: FileSystemWatcher monitors data.json; 500ms debounce on change event.
- **Component refresh**: On file change, DashboardDataService raises OnDataChanged event.
- **Blazor re-render**: Dashboard.razor subscribes to event, calls StateHasChanged() to re-render.
- **HTML generation**: Razor component generates semantic HTML from cached models.
- **Chart rendering**: ChartJS.Blazor components call JavaScript interop to draw Chart.js charts.
- **Singleton DashboardDataService**: Single instance, loaded once at startup, reused across all requests.
- **Component state via @code blocks**: Dashboard.razor holds reference to DashboardDataService, binds to public properties.
- **No external state library**: Blazor Server's server-side lifecycle eliminates distributed state concerns.
- **Event-driven updates**: DataChanged event allows multiple components to react without tight coupling.
- **JSON parse errors**: Catch JsonException in DashboardDataService, log error, return empty/fallback models.
- **File not found**: Check File.Exists() before reading; provide clear error message in UI.
- **File lock on write**: FileSystemWatcher may trigger multiple events during atomic writes; debounce to 500ms.
- **Malformed data.json**: Validate schema against C# model; reject invalid entries with informative logging.

### Considerations & Risks

- **None required** - Dashboard is read-only, no sensitive operations. Internal deployment only.
- **Optional: IP whitelisting** - If deployed to a network, restrict access via firewall or reverse proxy.
- **No encryption required** - data.json contains non-sensitive project metadata (milestones, statuses).
- **File permissions**: Set data.json read-only (644 on Linux, restrict Write ACL on Windows) to prevent accidental corruption.
- **Local deployment only**: Run Blazor Server app on workstation or local LAN server.
- **Hosting option 1**: Developer machine via `dotnet run` or Visual Studio debugging.
- **Hosting option 2**: Windows/Linux server running `dotnet MyProject.Web.dll` (requires .NET 8 runtime).
- **Hosting option 3**: Docker container (optional, adds complexity): Create Dockerfile with `FROM mcr.microsoft.com/dotnet/aspnet:8.0`, copy compiled app + data.json.
- **No cloud services**: No Azure, AWS, or GCP resources required.
- **No HTTPS required** (local only): Use HTTP for simplicity. If internet-exposed, enable HTTPS via self-signed cert.
- **Data.json updates**: Place in app working directory or configure path in appsettings.json. Ensure write permissions for data.json files.
- **Logs**: Use Serilog (optional) or built-in ILogger for diagnostic logging. Output to console or file.
- **Monitoring**: None required for simple dashboard. Optional: Add health check endpoint `/health` via ASP.NET Core Health Checks.
- **FileSystemWatcher race conditions**: Atomic file writes may trigger multiple change events. **Mitigation**: Implement 500ms debounce, track last-parsed file hash to avoid redundant parsing.
- **Large data.json files**: Parsing multi-MB JSON repeatedly on each file change could lag. **Mitigation**: Benchmark with realistic data sizes; if >10MB, consider splitting into multiple files or adding incremental update logic.
- **Chart.js interop complexity**: JavaScript interop adds latency on chart re-renders. **Mitigation**: Use ChartJS.Blazor for charts; avoid custom JavaScript where possible. Benchmark interactive performance.
- **Screenshot consistency**: Blazor Server rendering is deterministic, but browser caching/CSS media queries may affect screenshots. **Mitigation**: Use fixed viewport (1200px), light theme, no dark mode, test screenshots in multiple browsers.
- **Blazor Server session loss**: If app restarts, state is lost; user must refresh. **Mitigation**: Data reloads from data.json on next refresh—no data loss, but interruption. Acceptable for internal use.
- **Single-machine deployment**: Not designed for multi-server scaling. If needed, future versions could use database instead of data.json.
- **Real-time update latency**: FileSystemWatcher + Blazor re-render is 1-2 second cycle. Acceptable for executive dashboards (not sub-second).
- **Concurrent users**: Blazor Server sessions consume server memory (25-50MB per user). Safe for <20 simultaneous users on typical hardware.
- **Blazor novices**: 2-3 day ramp-up to understand component lifecycle, interop, and State management.
- **ChartJS.Blazor**: 1 day to configure charts; mostly wrapper around JavaScript library.
- **FileSystemWatcher**: Subtle timing issues (event order, debouncing) may surprise developers unfamiliar with filesystem events.
- | Decision | Chosen Approach | Alternative | Trade-off |
- |----------|---|---|---|
- | **UI Framework** | MudBlazor | Bootstrap 5 | MudBlazor is opinionated, faster to build; Bootstrap is more flexible but requires more CSS. |
- | **JSON Library** | System.Text.Json | Newtonsoft.Json | System.Text.Json is faster, zero-dep; Json.NET is more flexible. |
- | **Charting** | ChartJS.Blazor | OxyPlot | ChartJS is interactive, web-native; OxyPlot is C# native but static. |
- | **Timeline** | Custom SVG | Syncfusion Gantt | Custom is lightweight, simple; Syncfusion is full-featured but overkill. |
- | **Rendering** | Hybrid static-dynamic | Fully dynamic Blazor | Hybrid is simpler, faster, screenshot-friendly; dynamic allows sub-second reactivity. |
- | **State management** | Singleton service | MediatR/Fluxor | Singleton is simple, idiomatic; MediatR adds ceremony for complexity. |
- **Data.json location and ownership**: Who maintains data.json? Is it exported from another system (e.g., project management tool) or manually edited? What's the update frequency?
- **Screenshot automation**: Will screenshots be manually taken (F12, crop) or automated via PuppeteerSharp? Manual is simpler; automation adds complexity but scales to multiple dashboards.
- **Multi-dashboard support**: Is this a one-time dashboard or a template for multiple projects? If multi-project, consider data.json parameter (e.g., `/dashboard?project=ProjectA`) to reuse same page.
- **Design specification**: Need access to OriginalDesignConcept.html and C:/Pics/ReportingDashboardDesign.png to finalize layout and styling. Placeholder HTML/CSS will be created until design is reviewed.
- **Deployment platform**: Windows, Linux, or macOS? Are specific .NET 8 runtime constraints (e.g., self-contained vs. runtime-dependent) required?
- **Real-time vs. manual refresh**: Should dashboard auto-refresh when data.json changes (current recommendation) or require manual browser refresh? Auto-refresh is more seamless but adds FileSystemWatcher complexity.
- **Historical data**: Should dashboard track historical milestones/status over time, or only current state? Current design assumes snapshot-only.
- **Mobile access**: Is tablet/mobile viewing needed? Current recommendation assumes desktop-only for PowerPoint screenshots.

### Detailed Analysis

# Executive Dashboard - Technology Stack Research

## Sub-Question 1: Blazor Component Library for Dashboard UI

**Key Findings:**
Blazor Server ecosystem offers limited native component libraries compared to web frameworks. Syncfusion and Telerik dominate but are commercial. For an internal, simple dashboard, MudBlazor (free, MIT licensed) is the strongest choice. Bootstrap 5 integration is viable but requires manual component composition.

**Tools & Libraries:**
- **MudBlazor v7.0+** - MIT licensed, actively maintained, 2.5K GitHub stars, strong community. Native Blazor components, no JavaScript dependency required. Released June 2024, stable.
- **Telerik UI for Blazor** - Commercial ($2,495/year/developer), mature, production-grade. Overkill for simple dashboard.
- **Syncfusion Blazor** - Commercial, comprehensive, overkill.
- **Bootstrap 5 + C# classes** - Free, lightweight, requires manual styling.

**Trade-offs:**
MudBlazor: Free, no licensing overhead, learning curve (~2 days), smaller ecosystem than Bootstrap. Bootstrap: Lower barrier to entry, more documentation, but requires more custom C# logic for component state. Telerik/Syncfusion: Over-engineered for this use case, licensing costs.

**Recommendation:**
Use **MudBlazor v7.0+**. Justification: Free tier eliminates licensing friction, Blazor-native components match your stack exactly, sufficient for dashboard UI (cards, grids, icons), active maintenance, and community support. The library has been stable since v6.0 (Nov 2022) and fits the "simple" requirement without overhead.

**Evidence:**
MudBlazor is used by Microsoft internal teams and has integration examples specifically for Blazor Server. It ships with CSS-in-JS, eliminating separate stylesheet management.

---

## Sub-Question 2: data.json Loading & Management in Blazor Server

**Key Findings:**
Blazor Server runs on the server-side; loading JSON from filesystem is straightforward using System.IO and System.Text.Json (built into .NET 8). File watching for real-time updates requires FileSystemWatcher. No external API needed.

**Tools & Libraries:**
- **System.Text.Json** (built-in, .NET 8) - Fast, low-memory, no dependencies. v8.0.0 shipped with .NET 8.
- **Newtonsoft.Json (Json.NET)** - More flexible, better for complex scenarios. v13.0.3 (latest).
- **FileSystemWatcher** (System.IO.FileSystemWatcher, built-in) - Watches filesystem changes, allows reactive updates.
- **ConcurrentDictionary<string, T>** (System.Collections.Concurrent) - Thread-safe data caching.

**Trade-offs:**
System.Text.Json: Fast, native to .NET 8, minimal dependencies. Less flexible for edge cases. Json.NET: Slower, extra dependency, more flexible for complex serialization. FileSystemWatcher: Platform-dependent quirks (Windows/Linux differ), requires error handling for file locks during writes.

**Recommendation:**
Use **System.Text.Json for parsing** + **FileSystemWatcher for file monitoring**. Create a singleton service that caches parsed JSON and notifies Blazor components via StateHasChanged() when file changes. Example pattern: `DashboardDataService : IDisposable` (implements file watcher, exposes event for changes).

**Evidence:**
Microsoft recommends System.Text.Json as the default for .NET 8+ performance-critical applications. FileSystemWatcher is the standard approach in .NET for local file monitoring. This avoids external dependencies and keeps the stack minimal.

---

## Sub-Question 3: Charting Library for Milestones & Progress

**Key Findings:**
Blazor lacks a native charting library. Chart.js (via ChartJS.Blazor v4.1.2) is the most mature wrapper. OxyPlot is C# native but has a steeper learning curve. For timelines specifically, custom HTML/CSS or Gantt chart library may be more suitable.

**Tools & Libraries:**
- **ChartJS.Blazor v4.1.2** - JavaScript wrapper for Chart.js, MIT licensed, 800+ GitHub stars, stable since v4.0 (Jan 2022). Includes bar, line, pie, radar charts.
- **OxyPlot v2.1.2** - C# native, open-source, MIT licensed, but primarily static charts, less interactivity.
- **Syncfusion Blazor Charts** - Commercial, includes Gantt for timelines.
- **Custom SVG/HTML timeline** - Lightweight, full control, no dependencies.

**Trade-offs:**
ChartJS.Blazor: Requires JavaScript interop, mature ecosystem, interactive. OxyPlot: Pure C#, static output (PNG), less web-friendly. Syncfusion: Full-featured Gantt but commercial licensing. Custom SVG: Maximum simplicity, minimal overhead, but requires custom timeline logic.

**Recommendation:**
Use **ChartJS.Blazor v4.1.2 for progress/status charts** (bar, pie) + **custom SVG/HTML timeline for milestones**. ChartJS provides professional bar/pie charts for "Shipped/In Progress/Carried Over" categories. For milestones, hand-coded HTML timeline (div + CSS grid) is lighter and gives more control over screenshot aesthetics.

**Evidence:**
ChartJS.Blazor is the de-facto standard in Blazor community for charting. Syncfusion Gantt is bloated for a simple milestone line. Custom SVG timeline is common in executive dashboards (simplicity, clean visuals) and aligns with your PowerPoint screenshot goal.

---

## Sub-Question 4: Dashboard Architecture - Component vs. Standalone Project

**Key Findings:**
Adding to existing .sln structure as a Razor page or component is faster than new project. If existing Blazor Server app exists, integrate as a route. If not, create lightweight Blazor Server project in .sln.

**Tools & Libraries:**
- Existing Blazor Server app in .sln: Add `Pages/Dashboard.razor`
- New Blazor Server project: `dotnet new blazorserver -n Dashboard` in .sln folder
- Shared models: `Shared/DashboardModel.cs` (DTO for JSON structure)

**Trade-offs:**
Single Razor page in existing app: Faster, no new project overhead, simpler deployment. Standalone project: Cleaner separation, but adds complexity (inter-project communication, separate hosting). Given "simple" requirement, single-page approach wins.

**Recommendation:**
If Blazor Server already exists in .sln, **add Dashboard.razor as a routable page**. If building from scratch, **create minimal Blazor Server project** with single `/dashboard` route. Keep it single-page, single-responsibility. Store data models in a `Shared/` folder if multi-project.

**Evidence:**
Razor pages scale to moderate complexity without overhead. Dashboard is read-only (data.json source), so routing simplicity outweighs modularity concerns. Aligns with .sln structure you specified.

---

## Sub-Question 5: State Management Pattern

**Key Findings:**
Blazor Server runs server-side; component state is automatically managed in memory. For a simple dashboard, no external state library (Redux, Akavache) is needed. Component-level state via `@code` blocks is sufficient.

**Tools & Libraries:**
- **Component-level @code blocks** (built-in) - Sufficient for simple data binding.
- **Singleton service + cascading parameters** - For shared state across components.
- **MediatR** (NuGet v12.2.0) - Command/query pattern, optional for complex workflows.
- **Fluxor** (Nuget v5.9.2) - Redux-like state management, overkill for this use case.

**Trade-offs:**
@code blocks: Simple, no boilerplate, sufficient for dashboard. Singleton service: Slightly more structure, cacheable. MediatR: Adds ceremony, useful for larger systems. Fluxor: Over-engineered, complexity unjustified.

**Recommendation:**
Use **singleton service pattern**: Create `DashboardDataService` that loads/watches data.json, exposes public properties, fires events on changes. Inject into Dashboard.razor component, bind to component properties. No external state library needed. Example: `services.AddSingleton<DashboardDataService>()` in Startup.

**Evidence:**
Blazor Server component lifecycle and built-in dependency injection handle state adequately for read-only dashboards. Singleton pattern is idiomatic in ASP.NET Core and avoids dependencies. Team can reason about state without learning Redux patterns.

---

## Sub-Question 6: Page Structure for Screenshots & PowerPoint

**Key Findings:**
Executive dashboards prioritize visual clarity and printability. Use fixed-width layout (1200-1400px), light/white background, professional typography. Avoid responsive mobile design; optimize for desktop screenshot (1920x1080 or 1366x768 common presentation resolutions).

**Tools & Libraries:**
- **MudBlazor Container + Grid** - Provides layout scaffolding, responsive.
- **CSS Grid + Flexbox** (built-in, no library) - Maximum control over layout, no dependencies.
- **Bootstrap 5 Grid** - If not using MudBlazor, alternative layout system.
- **PuppeteerSharp v13.2.0** (optional) - For automated screenshot generation from Blazor.

**Trade-offs:**
MudBlazor layout: Opinionated, consistent, theme support. CSS Grid: Full control, lightweight, steeper learning curve. Bootstrap: Familiar, more docs, extra dependency. PuppeteerSharp: Enables programmatic screenshots, adds Node.js dependency, may be overkill for manual PowerPoint extraction.

**Recommendation:**
Use **MudBlazor layout primitives** (Container, Grid, Card) for structure. Set fixed Container width (~1200px). Design for 1920x1080 resolution. Use light theme, sans-serif fonts (e.g., Segoe UI), ample whitespace. Do NOT optimize for mobile. **Skip PuppeteerSharp**; manual browser screenshot (F12, capture) is simpler and faster for executive use.

**Evidence:**
Fixed-width executive dashboards are industry standard (Tableau, Power BI dashboards). Blazor Server renders server-side, so screenshots are stable across browser refreshes. Whitespace and typography matter more than responsiveness for printed/PowerPoint visuals.

---

## Sub-Question 7: File System Access for data.json

**Key Findings:**
Blazor Server app has full filesystem access (runs on .NET server). No permissions overhead compared to web app. Place data.json in app root or `wwwroot/` folder. FileSystemWatcher monitors for changes without polling.

**Tools & Libraries:**
- **System.IO.File** (built-in) - Read JSON file.
- **System.IO.FileSystemWatcher** (built-in) - Watch for file changes.
- **Path.Combine()** (built-in) - Platform-agnostic path construction.

**Trade-offs:**
Direct file I/O: Simple, no dependencies, fast. FileSystemWatcher: Platform quirks (Windows Create/Change events differ from Linux). No cloud/HTTP overhead, minimal latency.

**Recommendation:**
Place `data.json` in app root or `bin/` folder. Create `DashboardDataService` that:
1. Reads JSON on app startup via `File.ReadAllText()`
2. Watches via FileSystemWatcher with debounce (500ms) to handle file lock issues
3. Re-parses on change, raises event for component refresh

Use `Path.Combine(AppContext.BaseDirectory, "data.json")` for platform-agnostic path resolution.

**Evidence:**
Local file I/O in .NET is robust and well-documented. Debouncing FileSystemWatcher prevents race conditions when files are written atomically. This approach adds zero infrastructure complexity and is ideal for local-only deployments.

---

## Sub-Question 8: Static vs. Dynamic Rendering

**Key Findings:**
Static rendering (generate HTML from JSON at startup) is simpler for screenshots and avoids component lifecycle complexity. Dynamic Blazor components allow reactivity but add unnecessary overhead for a read-only dashboard updated by file writes.

**Tools & Libraries:**
- **Razor HTML generation** (built-in) - Server-side HTML, no JavaScript interop.
- **Blazor components with @code** - Interactive, StateHasChanged() driven.

**Trade-offs:**
Static HTML: Simple, fast, perfect for screenshots, no interactivity. Dynamic Blazor: Reactive, handles real-time updates elegantly, but overkill if data updates are infrequent (e.g., once per day). Hybrid: Render static markup with Blazor wrapper for file watch events.

**Recommendation:**
Use **hybrid approach**: Render dashboard as static HTML fragments generated from data.json, wrapped in a lightweight Blazor component. The component watches data.json and calls StateHasChanged() to re-render on file change. This gives simplicity of static markup (clean screenshots) + reactivity of Blazor. No ChartJS.Blazor interop complexity.

**Evidence:**
Executive dashboards rarely require sub-second reactivity. File-based updates (data.json writes) are infrequent. Static HTML renders instantly and photographs cleanly for PowerPoint. Blazor wrapper adds no overhead and enables refresh on file watch events. This aligns with your "simple" requirement.

---

## Cross-Cutting Recommendations

- **Data Model**: Create `Models/DashboardData.cs` with structure: `Project`, `Milestones[]`, `WorkItems[]` (with Status enum: Shipped/InProgress/CarriedOver).
- **Dependency Injection**: Register `DashboardDataService` as singleton in `Startup.ConfigureServices()`.
- **Error Handling**: Wrap JSON parsing in try-catch; log parse errors, show fallback UI.
- **Testing**: Unit test `DashboardDataService` with mock `data.json` files; use xUnit + Moq (both NuGet).
- **Deployment**: Copy `data.json` to app output directory via `.csproj` build step: `<ItemGroup><None Include="data.json" CopyToOutputDirectory="PreserveNewest" /></ItemGroup>`
