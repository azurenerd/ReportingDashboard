# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-10 06:52 UTC_

### Summary

A simple executive dashboard for project milestone and progress reporting requires a lightweight, screenshot-optimized approach. Build a standalone Blazor Server application (`AgentSquad.Reporting`) that loads project data from JSON configuration files, displays status metrics via ChartJS, renders milestone timelines with custom SVG, and refreshes via FileSystemWatcher. The architecture prioritizes simplicity (no authentication, database, or cloud services) and visual clarity for PowerPoint screenshots. This approach minimizes complexity while meeting all functional requirements and fits the mandatory C# .NET 8 tech stack. --- ```xml <ItemGroup> <PackageReference Include="CurrieTechnologies.Razor.ChartJS" Version="4.1.0" /> <!-- All other packages are built into .NET 8 --> </ItemGroup> ``` ---

### Key Findings

- JSON file-based configuration with System.Text.Json eliminates database overhead and aligns with "local-only" requirements; FileSystemWatcher + polling hybrid enables seamless data refresh without app restart.
- ChartJS (v4.1.0+) handles status bar charts efficiently, while custom SVG Gantt timelines provide screenshot-perfect rendering without JavaScript interop overhead that burdens Blazor Server.
- Standalone Blazor Server project (`AgentSquad.Reporting`) maintains architectural separation from AgentSquad.Runner worker process; Blazor Server includes built-in Kestrel server eliminating deployment complexity.
- Bootstrap 5.3 CDN styling provides professional appearance and responsive layout without build tool complexity; custom print CSS ensures pixel-perfect PowerPoint screenshots at 1920x1080 resolution.
- Singleton DashboardDataService with event-based refresh keeps state management minimal and components decoupled; read-only dashboard requires no complex state machines or caching strategies.
- .NET 8 Server-Side Rendering (SSR) with streaming reduces WebSocket overhead compared to full interactive Blazor, critical for dashboard use case where interactivity is minimal.
- No authentication, authorization, or encryption required per project scope; dashboard is local-only administrative tool intended for internal executive viewing only.
- Minimal risk profile: architecture is straightforward, dependency count is low (3-4 external packages), and technologies are mature with large community support across .NET ecosystem.
- ---
- Create `AgentSquad.Reporting` Blazor Server project (.NET 8)
- Register `DashboardDataService` in DI container
- Implement JSON deserialization (System.Text.Json)
- Create Dashboard.razor main page with Bootstrap grid layout
- Build StatusCards.razor component (completed/in-progress/carried counts)
- Integrate ChartJS via `CurrieTechnologies.Razor.ChartJS`
- Add status bar chart (horizontal bars for counts)
- Style with Bootstrap 5.3
- Build MilestoneTimeline.razor with custom SVG Gantt rendering
- Implement FileSystemWatcher + polling hybrid in DashboardDataService
- Add event-based refresh triggering `StateHasChanged()`
- Sample `data.json` with fictional project
- Add print-friendly CSS and PowerPoint screenshot styling
- Test on Chrome, Edge, Firefox
- Screenshot at 1920x1080 and validate PowerPoint compatibility
- Document JSON schema, deployment steps
- Unit tests for DashboardDataService (xUnit + Moq)
- **Pre-rendered example dashboard** - Static HTML/CSS mockup before Blazor to validate design with stakeholders
- **Docker containerization** - Single Dockerfile for easy deployment: `docker build -t agentsquad-reporting . && docker run -p 7001:7001`
- **Health check endpoint** - `/health` endpoint returning current data file status (useful for monitoring)
- **Export to HTML** - Add "Download as HTML" button for archival before meeting
- **SVG Timeline Prototype** - Before integrating into Blazor, prototype SVG Gantt rendering in static HTML (1-2 hours, validate rendering)
- **JSON Schema Validation** - Create JSON Schema file (`data.schema.json`) and validate on load; catch config errors early
- **Browser Screenshot Tool** - Prototype automated screenshot capture via headless Chrome (Puppeteer.NET v4.0+) if frequent reports needed
- [ ] Error handling for malformed JSON (non-blocking)
- [ ] Logging of file watcher events (to console or local log file)
- [ ] CSS print styles tested across browsers
- [ ] Sample `data.json` with realistic project data
- [ ] README with deployment instructions
- [ ] Unit test coverage for DashboardDataService (>80%)
- [ ] Documentation of data.json schema
- [ ] Performance baseline (page load <1s, refresh <500ms)
- **MVP (all phases)**: 4 weeks, 1 developer
- **Quick wins**: +3-5 days (containerization, health check, export)
- **Production hardening**: +2-3 days (logging, error handling, documentation)
- **Total to production-ready**: ~6 weeks, 1 developer
- Dashboard loads in <1 second on local machine
- File change detected and rendered within 2 seconds
- PowerPoint screenshots capture at 1920x1080 without clipping
- Zero unhandled exceptions during normal operation
- Deployment requires <5 minutes (copy + run)

### Recommended Tools & Technologies

- **Blazor Server** (.NET 8.0.0+) - Server-side .NET component framework with WebSocket interop
- **Bootstrap 5.3.0+** (CDN) - CSS framework for responsive grid and professional styling; `https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css`
- **ChartJS v4.1.0+** via `CurrieTechnologies.Razor.ChartJS` (NuGet: v4.1.0) - Lightweight bar charts for status metrics
- **Custom SVG** (no dependencies) - Gantt timeline for milestones using HTML5 `<svg>` elements
- **.NET 8.0.0+** - LTS release, stable, widely supported
- **ASP.NET Core Kestrel** (built-in) - Web server, no additional packages needed
- **System.Text.Json** (built-in .NET 8) - JSON deserialization, zero external dependencies
- **System.IO.FileSystemWatcher** (built-in) - File change detection for JSON config reload
- **System.Timers.Timer** (built-in) - Polling fallback for robust file refresh
- **JSON flat-file config** - `data.json` in app root or wwwroot directory
- **Strongly-typed C# DTOs** - Deserialized from JSON via System.Text.Json
- **xUnit** (NuGet: v2.6.0+) - Unit testing framework, preferred in .NET ecosystem
- **Moq** (NuGet: v4.18.0+) - Mocking library for DashboardDataService tests
- **GitHub Actions** (existing) - No additional CI/CD tools needed; basic YAML workflows sufficient
- **Visual Studio 2022** or **VS Code + C# Dev Kit** - Development environment
- **dotnet CLI** (v8.0+) - Project management and build
- **SQL Server Compact** (optional future) - If persistence beyond JSON becomes needed; avoid initially
- ```
- AgentSquad/
- ├── AgentSquad.Runner/          (existing worker service)
- ├── AgentSquad.Reporting/       (new Blazor Server app)
- │   ├── Pages/
- │   │   ├── Dashboard.razor     (main page)
- │   │   ├── Index.razor         (welcome/redirect)
- │   ├── Components/
- │   │   ├── StatusCards.razor   (completed/in-progress/carried)
- │   │   ├── MilestoneTimeline.razor  (SVG Gantt)
- │   │   ├── StatusChart.razor   (ChartJS bar chart)
- │   ├── Services/
- │   │   ├── IDashboardDataService.cs
- │   │   ├── DashboardDataService.cs
- │   ├── Models/
- │   │   ├── DashboardData.cs
- │   │   ├── Milestone.cs
- │   │   ├── StatusItem.cs
- │   ├── wwwroot/
- │   │   ├── data.json           (configuration file)
- │   │   ├── css/app.css         (custom styles + print CSS)
- │   ├── Program.cs              (DI + Kestrel config)
- ├── AgentSquad.sln
- ```
- ```csharp
- public class DashboardData
- {
- public string ProjectName { get; set; }
- public string ProjectId { get; set; }
- public DateTime LastUpdated { get; set; }
- public List<Milestone> Milestones { get; set; } = new();
- public List<StatusItem> Completed { get; set; } = new();
- public List<StatusItem> InProgress { get; set; } = new();
- public List<StatusItem> CarriedOver { get; set; } = new();
- }
- public class Milestone
- {
- public string Name { get; set; }
- public DateTime TargetDate { get; set; }
- public bool Completed { get; set; }
- public string Status { get; set; } // "On Track", "At Risk", "Delayed"
- }
- public class StatusItem
- {
- public string Title { get; set; }
- public string Description { get; set; }
- public DateTime AddedDate { get; set; }
- public string Owner { get; set; }
- }
- ```
- ```json
- {
- "projectName": "Project X - Q2 Delivery",
- "projectId": "PRJ-2024-001",
- "lastUpdated": "2024-04-10T18:30:00Z",
- "milestones": [
- {
- "name": "Requirements Review",
- "targetDate": "2024-04-15T00:00:00Z",
- "completed": true,
- "status": "Completed"
- },
- {
- "name": "Development Phase 1",
- "targetDate": "2024-05-30T00:00:00Z",
- "completed": false,
- "status": "On Track"
- }
- ],
- "completed": [
- {
- "title": "Architecture Document",
- "description": "Approved by stakeholders",
- "addedDate": "2024-04-08T00:00:00Z",
- "owner": "John Smith"
- }
- ],
- "inProgress": [
- {
- "title": "API Development",
- "description": "Core endpoints - 60% complete",
- "addedDate": "2024-03-15T00:00:00Z",
- "owner": "Jane Doe"
- }
- ],
- "carriedOver": [
- {
- "title": "Database Migration Script",
- "description": "Delayed pending vendor approval",
- "addedDate": "2024-03-01T00:00:00Z",
- "owner": "Bob Johnson"
- }
- ]
- }
- ```
- ```csharp
- public interface IDashboardDataService
- {
- DashboardData CurrentData { get; }
- event EventHandler? DataRefreshed;
- Task RefreshAsync();
- Task<bool> IsDataValidAsync();
- }
- public class DashboardDataService : IDashboardDataService, IAsyncDisposable
- {
- private readonly string _dataPath;
- private DashboardData? _currentData;
- private FileSystemWatcher? _watcher;
- private System.Timers.Timer? _pollTimer;
- public event EventHandler? DataRefreshed;
- public DashboardDataService(IWebHostEnvironment env)
- {
- _dataPath = Path.Combine(env.WebRootPath, "data.json");
- _ = InitializeAsync();
- }
- private async Task InitializeAsync()
- {
- await RefreshAsync();
- InitializeFileWatcher();
- InitializePollingFallback();
- }
- public async Task RefreshAsync()
- {
- try
- {
- var json = await File.ReadAllTextAsync(_dataPath);
- _currentData = JsonSerializer.Deserialize<DashboardData>(json);
- DataRefreshed?.Invoke(this, EventArgs.Empty);
- }
- catch (Exception ex)
- {
- // Log error; retain previous data
- }
- }
- private void InitializeFileWatcher()
- {
- _watcher = new FileSystemWatcher(Path.GetDirectoryName(_dataPath), "data.json");
- _watcher.NotifyFilter = NotifyFilters.LastWrite;
- _watcher.Changed += async (s, e) => await RefreshAsync();
- _watcher.EnableRaisingEvents = true;
- }
- private void InitializePollingFallback()
- {
- _pollTimer = new System.Timers.Timer(30000); // 30 seconds
- _pollTimer.Elapsed += async (s, e) => await RefreshAsync();
- _pollTimer.AutoReset = true;
- _pollTimer.Start();
- }
- public DashboardData CurrentData => _currentData ?? new DashboardData();
- public async ValueTask DisposeAsync()
- {
- _watcher?.Dispose();
- _pollTimer?.Dispose();
- }
- }
- ```
- **Startup**: `Program.cs` registers `DashboardDataService` as singleton; service loads JSON and initializes file watcher + polling timer
- **Page Load**: Dashboard.razor injects `IDashboardDataService`, reads `CurrentData`
- **Component Render**: Status cards, ChartJS bar chart, SVG timeline render from data
- **File Change**: FileSystemWatcher detects `data.json` change, calls `RefreshAsync()`
- **Data Update**: Service deserializes new JSON, raises `DataRefreshed` event
- **UI Refresh**: Components subscribed to event call `StateHasChanged()`
- **Initial Page**: Server-side render (SSR) entire dashboard on first request
- **Data Refresh**: Blazor's `@implements IAsyncDisposable` pattern with component-level refresh subscription
- **Milestone Timeline**: Render as inline SVG with no JavaScript dependencies
- **Status Chart**: ChartJS via JavaScript interop; defer loading until page interactive
- ---

### Considerations & Risks

- **None required** - Dashboard is local-only administrative tool; assume trusted network only
- **Future enhancement** (if needed): Add basic HTTP Basic Auth via middleware (no external packages)
- **At rest**: JSON file stored unencrypted on disk (acceptable for local, non-sensitive use case)
- **In transit**: No encryption needed (local only, no network exposure)
- **No personally identifiable information** stored (project names, owner names only; not sensitive)
- **Target Environment**: Windows/Linux local machine, internal network only
- **Deployment**: Simple copy of published artifacts to local folder or network share
- **Port**: Default HTTP (80) or HTTPS (443) on localhost or internal IP
- **Firewall**: No ingress required; accessed via browser on same machine or internal network
- **No cloud services**: All data, hosting, and compute remain on-premise
- **Storage**: 100KB-1MB (JSON files, minimal)
- **Memory**: ~200-500MB (Blazor Server + ChartJS in-memory)
- **CPU**: Single-core sufficient (read-only dashboard, no heavy processing)
- **Backup**: Backup `data.json` files as part of project documentation backups
- Publish as self-contained executable: `dotnet publish -c Release -r win-x64 --self-contained`
- Execute: `AgentSquad.Reporting.exe`
- Browser opens to `https://localhost:7001`
- Deploy to IIS on Windows Server or Kestrel on Linux
- Configure hostname: `reporting.internal.company.net`
- All team members access via internal DNS
- ---
- | Risk | Severity | Mitigation |
- |------|----------|-----------|
- | **FileSystemWatcher missing rapid file changes** | Low | Implement 30-second polling fallback; acceptable for executive dashboard (not real-time) |
- | **JSON deserialization failures** | Low | Validate JSON schema on load; log errors; retain previous data if parse fails |
- | **Blazor Server WebSocket latency on state refresh** | Low | Minimize interactivity; use SSR; dashboard is read-only, few state changes |
- | **ChartJS library compatibility** | Very Low | `CurrieTechnologies.Razor.ChartJS` is actively maintained; v4.1.0+ tested with .NET 8 |
- | **Date/time timezone issues** | Low | Store all dates as UTC in JSON; render in local browser timezone via JavaScript interop |
- | **CSS print rendering inconsistency across browsers** | Low | Test on Chrome + Firefox; PowerPoint screenshot via browser print-to-image is standard |
- | Decision | Trade-off |
- |----------|-----------|
- | **JSON over database** | No SQL overhead, but scaling beyond ~500 status items becomes unwieldy; defer database until needed |
- | **Singleton service** | Minimal memory, but file watcher can miss changes if file locked during write; polling mitigates |
- | **Custom SVG timeline** | Zero JavaScript overhead, but manual D3/Plotly migration if complex visualizations needed later |
- | **Blazor Server vs. WebAssembly** | Server-side rendering is slower for user input, but dashboard is read-only; no SPA overhead |
- | **Bootstrap CDN** | Depends on CDN availability; consider self-hosting CSS if internet access restricted |
- **Low**: Blazor Server fundamentals, Bootstrap, SVG
- **Team should be familiar with**: C# async/await, dependency injection, file I/O
- **Learning required**: FileSystemWatcher pattern, ChartJS JavaScript interop (< 1 day)
- **Database persistence** - Start with JSON; migrate to SQL/Cosmos if data grows beyond ~1000 items
- **Multi-user concurrency** - JSON file locking sufficient for now; add pessimistic locking if simultaneous edits needed
- **Audit logging** - Not required for MVP; add changelog tracking if compliance needed later
- **Internationalization (i18n)** - English-only for MVP; implement resource files if multi-language needed
- ---
- **Data File Location**: Should `data.json` be embedded in app package, loaded from network share, or located in AppData folder for persistence across updates?
- **Screenshot Resolution**: Should dashboard optimize for 1920x1080 (16:9 widescreen PowerPoint) or support multiple resolutions? What DPI/scaling assumptions?
- **Milestone Timeline Granularity**: Should timeline support sub-milestone items (e.g., Phase 1a, 1b, 1c) or flat list only? What's the maximum number of concurrent milestones?
- **Status Item Counts**: Are 5-10 items per status category typical, or could this grow to 50+ items? Affects scrolling/pagination design.
- **Color Coding Standards**: Should status colors (green/yellow/red for On Track/At Risk/Delayed) be configurable in data.json, or hard-coded in application?
- **Historical Data Tracking**: Should dashboard display previous month's carried-over items (read-only history) or only current month? What's the retention policy?
- **Edit Capability**: Is this read-only for all users, or should project owner be able to edit data.json through UI (adds complexity)?
- **Shared Data**: If multiple projects need dashboards, should each have separate `data-project1.json` file, or single aggregated file with project filter?
- ---

### Detailed Analysis

# Executive Dashboard Research & Analysis

## 1. JSON Data Binding & Configuration

**Key Findings:**
Blazor Server supports strong-typed JSON deserialization via `System.Text.Json` (built-in .NET 8) or Newtonsoft.Json. For dashboard use case, file-based JSON configuration is simplest pattern.

**Tools & Libraries:**
- `System.Text.Json` (.NET 8 built-in) - Zero dependencies, fast, AOT-friendly
- `JsonSerializerOptions` with custom converters for date/timeline serialization
- `FileSystemWatcher` for file change detection (System.IO built-in)

**Trade-offs:**
- System.Text.Json lacks some advanced features Newtonsoft has (custom settings on properties) but is faster and built-in
- File watching adds latency vs. polling; FileSystemWatcher can miss rapid changes
- No database overhead (matches "local only" requirement)

**Concrete Recommendation:**
Use `System.Text.Json` with strongly-typed DTOs (Data Transfer Objects). Implement simple singleton service that loads JSON on app startup and provides file-watch refresh capability via IFileSystemWatcher.

```csharp
public class DashboardData 
{
    public string ProjectName { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<StatusItem> Completed { get; set; }
    public List<StatusItem> InProgress { get; set; }
    public List<StatusItem> Carried { get; set; }
}
```

**Evidence:** System.Text.Json is official .NET recommendation post-3.1; FileSystemWatcher widely used in local tooling (Visual Studio, dotnet watch).

---

## 2. Charting & Timeline Visualization

**Key Findings:**
For Blazor Server dashboards, JavaScript interop charting libraries are standard. Executive reporting favors clean, minimal visual clutter over fancy animations.

**Tools & Libraries:**
- **ChartJS** via Blazor wrapper (NuGet: `CurrieTechnologies.Razor.ChartJS` v4.1.0+) - Lightweight, clean, widely adopted
- **Plotly Blazor** (NuGet v2.0+) - More advanced but heavier; overkill for timelines
- **Custom SVG rendering** - No dependencies, full control, ideal for simple Gantt/timeline

**Trade-offs:**
- ChartJS: Simple bar/line charts, great visual defaults; limited custom interactivity
- Plotly: Rich interactivity and 3D support; large JS bundle, slower rendering
- SVG: Zero JS dependencies, screenshot-perfect, low performance overhead; requires manual implementation

**Concrete Recommendation:**
Use **ChartJS for status bars/progress metrics** (completed/in-progress/carried counts as bar chart) and **custom SVG for milestone timeline** (horizontal Gantt-style bars with date labels). This avoids bloated JavaScript while keeping rendering clean for PowerPoint screenshots.

**Evidence:** ChartJS dominates small dashboard projects in .NET ecosystem. SVG approach reduces JavaScript interop overhead—critical for Blazor Server which is slower than SPA. Executive dashboards prioritize clarity over interactivity (per context: "screenshots for PowerPoint").

---

## 3. Component Architecture

**Key Findings:**
Context suggests integrating into existing `AgentSquad.Runner` project. Blazor Server supports both standalone and modular patterns within single .sln.

**Trade-offs:**
- **Standalone Blazor Server app**: Clean separation, easier to maintain independently
- **New component in Runner**: Reuses existing infrastructure (logging, DI), simpler deployment
- **Runner appears to be a worker service**—may lack built-in web server

**Concrete Recommendation:**
**Create new Blazor Server project** (`AgentSquad.Reporting`) within same .sln, reference shared libraries from Runner. Blazor Server projects include built-in Kestrel server, fully self-contained. If Runner needs to serve reports, use it as data provider and have separate Reporting app.

**Evidence:** AgentSquad.Runner.csproj is worker/console-based. Blazor Server requires web host (Program.cs with builder.AddRazorComponents()). Keeping separate maintains SoC.

---

## 4. CSS Framework & Styling

**Key Findings:**
Executive dashboards require professional appearance with minimal bloat. Bootstrap vs. custom CSS trade-off is real.

**Tools & Libraries:**
- **Bootstrap 5.3.0+** (CSS only, CDN or NuGet): Rapid styling, responsive grids, professional defaults
- **Tailwind CSS v3.3+**: Utility-first, smaller final bundle if PurgeCSS configured
- **Custom CSS**: Full control, minimal download, requires design discipline

**Trade-offs:**
- Bootstrap: Fast to prototype, ~30KB gzipped; adds CSS class overhead
- Tailwind: Smaller final CSS if properly configured (~15KB), requires build step
- Custom: Smallest footprint (~5KB), but design must be pixel-perfect for PowerPoint

**Concrete Recommendation:**
**Bootstrap 5.3 CDN for MVP** (rapidness > optimization). Once design stabilizes, consider Tailwind with PurgeCSS if performance critical. Executive dashboards rarely need cutting-edge CSS—Bootstrap's professional defaults are sufficient.

**Evidence:** Bootstrap 5 widely adopted in Blazor projects. CDN approach avoids build tool complexity. Responsive grid system handles multiple screen sizes without custom work.

---

## 5. Real-Time Data Refresh

**Key Findings:**
Dashboard is read-only report; updates only when config file changes. Options: file watching, polling, manual refresh.

**Tools & Libraries:**
- `System.IO.FileSystemWatcher` (built-in): Detects file changes, triggers reload
- `System.Timers.Timer` (polling): Polls file on interval, simpler fallback
- **Blazor's `StateHasChanged()`**: Re-render after data update

**Trade-offs:**
- FileSystemWatcher: Fast but can miss rapid changes, platform-specific quirks
- Polling: Reliable, predictable, adds CPU overhead (minimal for JSON file)
- No refresh: Requires app restart; unacceptable for executive use

**Concrete Recommendation:**
Implement **hybrid: FileSystemWatcher + fallback polling every 30 seconds**. On file change detected, reload JSON and call `StateHasChanged()` to re-render Blazor component. This avoids app restart while handling edge cases.

```csharp
private void OnDataFileChanged()
{
    _dashboardData = JsonSerializer.Deserialize<DashboardData>(File.ReadAllText(_dataPath));
    InvokeAsync(() => StateHasChanged());
}
```

**Evidence:** FileSystemWatcher is standard .NET practice for local file tools. 30s polling is imperceptible to end-user for executive dashboard use case.

---

## 6. Performance & Rendering Optimization

**Key Findings:**
Blazor Server runs on WebSocket connection; every state change triggers round-trip. Single-page dashboard can optimize this significantly.

**Strategies:**
- **Server-side rendering (SSR)** (Blazor 8+): Render on first load, minimal JavaScript interop
- **Component isolation** (`@rendermode InteractiveServer`): Only re-render changed subtrees
- **Streaming rendering**: Load milestones/status sections progressively
- **Caching**: Cache JSON in memory with TTL, avoid repeated disk reads

**Trade-offs:**
- SSR: Fastest initial load, no interactivity; good for reports
- InteractiveServer: Full Blazor interactivity, but slower per change
- Streaming: Better UX for large pages, adds complexity

**Concrete Recommendation:**
**Use .NET 8 SSR (Static Server-Side Rendering) with streaming**. Load dashboard data once on app startup, serve static HTML for initial request, stream Gantt timeline last. This minimizes Blazor Server overhead while keeping page responsive.

```html
@page "/dashboard"
@rendermode InteractiveServer

<div>@Dashboard.ProjectName</div>
{@await foreach (var milestone in Dashboard.Milestones) {
    <Milestone Data="milestone" />
}}
```

**Evidence:** Blazor SSR in .NET 8 was specifically designed for report-style pages. Reduces latency compared to full interactive mode.

---

## 7. State Management Pattern

**Key Findings:**
Dashboard is read-only, single-source-of-truth is the JSON file. Minimal state needed.

**Patterns:**
- **Singleton service**: Load once, provide to all components. Good for static data.
- **Scoped service**: New instance per connection. Unnecessary for dashboards.
- **Component-local state**: Pass as parameters only. Simple but fragile.

**Concrete Recommendation:**
**Singleton `IDashboardDataService`** injected via DI. Load JSON once at app startup, expose via property. Components subscribe to refresh event on file change. No complex state machine needed.

```csharp
public interface IDashboardDataService
{
    DashboardData CurrentData { get; }
    event EventHandler DataRefreshed;
    void Refresh();
}
```

**Evidence:** Dashboard is write-free, read-only access pattern. Singleton avoids repeated deserialization. Event-based refresh keeps components decoupled.

---

## 8. Screenshot-Friendly Output

**Key Findings:**
Executive screenshots for PowerPoint require pixel-perfect rendering, consistent fonts, print-optimized styling.

**Considerations:**
- **Viewport**: Fixed 1920x1080 or responsive with print styles
- **Print CSS**: Hide unnecessary elements, set exact sizes, black text on white
- **Font rendering**: System fonts (`-apple-system, BlinkMacSystemFont, 'Segoe UI'`) render consistently
- **Color contrast**: WCAG AA for screenshots (7:1 ratio preferred for executives)

**Concrete Recommendation:**
Add `@media print` CSS:
```css
@media print {
  body { font-size: 11pt; color: black; margin: 0; }
  .no-print { display: none; }
  .dashboard { page-break-inside: avoid; }
}
```

Use viewport `<meta name="viewport" content="width=1920, height=1080">` or responsive grid. Font: Bootstrap's default stack (Segoe UI on Windows, excellent compatibility). Test screenshots at 1920x1080 and 1280x720.

**Evidence:** Executive PowerPoint decks commonly use 16:9 aspect ratio (1920x1080 or 1280x720). Fixed viewport ensures consistent rendering across machines. Print CSS prevents UI clutter in screenshots.

---

## Integration Summary

**Recommended Tech Stack:**
- **Framework**: Blazor Server (.NET 8)
- **JSON**: System.Text.Json + FileSystemWatcher
- **Charts**: ChartJS (status) + custom SVG (timeline)
- **Styling**: Bootstrap 5.3 CDN
- **State**: Singleton DashboardDataService
- **Rendering**: SSR with streaming
- **Architecture**: Separate `AgentSquad.Reporting` project in existing .sln

**MVP Timeline:**
- Week 1: JSON schema, DashboardDataService, basic Bootstrap layout
- Week 2: Status cards component, ChartJS integration
- Week 3: SVG Gantt timeline, file watcher refresh
- Week 4: Polish, screenshot testing, documentation

**Critical Success Factors:**
1. Simplicity—no auth, no database, no cloud
2. Screenshot-ready—pixel-perfect rendering
3. Zero interactivity—read-only dashboard
4. Local-only—no network dependencies
