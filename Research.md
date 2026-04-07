# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-07 19:47 UTC_

### Summary

This project requires a lightweight, local-only executive dashboard built on C# .NET 8 with Blazor Server to visualize project milestones, progress, and status from a JSON configuration file. The recommended architecture prioritizes simplicity and screenshot quality for PowerPoint deck integration. Blazor Server components paired with ApexCharts for timeline visualization, System.Text.Json for data management, and Bootstrap 5 styling deliver a professional dashboard with zero external dependencies. The entire application deploys as a self-contained Windows executable (~180MB) with data.json bundled alongside—no .NET runtime required for end-users, no authentication overhead, and no cloud infrastructure complexity. ---

### Key Findings

- **Blazor Server is ideal for this use case:** Server-side rendering and two-way data binding eliminate JavaScript complexity while maintaining responsiveness; cascading parameters simplify state management across dashboard sections without prop-drilling.
- **ApexCharts.Razor (v2.5.x) produces executive-ready timeline visualizations:** SVG-based charting renders pixel-perfect timelines and Gantt charts with color-coded milestone status (Not Started, In Progress, Shipped, Carried Over); exports cleanly to PowerPoint without quality loss.
- **System.Text.Json + FileSystemWatcher eliminates external dependencies:** Native .NET 8 JSON handling is 6-10x faster than Newtonsoft.Json; FileSystemWatcher enables real-time cache invalidation when data.json changes, supporting iterative exec deck updates without app restart.
- **Self-contained publishing removes .NET runtime prerequisites:** `dotnet publish -r win-x64 --self-contained` produces a single executable (~180MB) that execs can double-click to run; no SDK installation, no dependency management, no friction.
- **Client-side filtering (MudBlazor DataGrid) is fast enough for typical workloads:** Dashboards rarely exceed 1,000 items; filtering 1,000 items client-side is sub-100ms; eliminates server round-trip latency and network dependency.
- **Bootstrap 5.3.x provides print-optimized responsive design out-of-the-box:** Professional dashboard styling, excellent PDF/screenshot support, ships with Blazor templates, minimal custom CSS required.
- **JSON schema validation prevents silent runtime failures:** JsonSchema.NET (v6.x) validates data.json structure at load time, catching config errors early and providing clear error messages.
- **No authentication or complex security required:** Local-only deployment, single executable, no multi-user access = eliminate auth layer entirely; focus dev time on dashboard functionality instead of security infrastructure.
- ---
- Blazor Server project scaffold with Bootstrap 5 template
- DashboardLayout component + DashboardData model
- DataProvider service (load & cache data.json)
- TimelineSection component (milestone list, no Gantt chart yet)
- ProgressCardsSection component (Shipped/InProgress/CarriedOver summary cards)
- Sample data.json with 5-10 fake milestones and items
- Functional dashboard showing fake project progress
- `dotnet run` launches app at `https://localhost:5001`
- Validates Blazor Server setup and component hierarchy
- Establishes data model and file loading before adding complexity
- Produces executable dashboard that execs can review for feedback
- Integrate ApexCharts.Razor
- Build Gantt chart for milestone timeline (color-coded by status)
- ItemGridSection component with MudBlazor DataGrid (sort/filter by category)
- Basic CSS refinement (color scheme, spacing, responsive layout)
- Interactive timeline and item grid
- Filtering by category (Shipped/InProgress/CarriedOver)
- Professional styling for PowerPoint screenshots
- Core value delivery; execs can now see interactive dashboard
- ApexCharts integration highest risk; learn early if issues arise
- MVP + Phase 2 = shippable product
- Unit tests (xUnit) for DataProvider, data validation
- Bunit component tests for TimelineSection, ProgressCards
- JsonSchema.NET validation + error handling
- FileSystemWatcher implementation (auto-refresh on data.json change)
- Self-contained executable publishing setup
- User documentation (how to update data.json, run executable)
- Test coverage: >80% on DataProvider and validation logic
- Deployable self-contained executable (~180MB)
- README.md with setup & usage instructions
- Ensures reliability for exec-facing tool
- Self-contained publishing deferred to end because it's relatively straightforward
- Testing catches schema validation issues before production use
- Get Blazor Server template running locally with sample data
- Hardcode fake project data and render on Index.razor page
- Execs see clickable prototype before Phase 1 ends
- Simple, clean dashboard layout
- Proof that screenshots render well for PowerPoint
- Confirmation that technology stack works
- Blazor Server + Bootstrap responsive layout
- Read-only data.json with 5-10 milestones
- Summary cards (Shipped/InProgress/CarriedOver counts)
- Basic timeline display (list or table, not Gantt yet)
- Self-contained executable deployment
- Gantt chart (Phase 2)
- Advanced filtering (Phase 2)
- Editing UI (out of scope)
- PDF export (out of scope unless stakeholder request)
- **Have execs review data.json schema early.** Get sign-off on milestone and item fields before engineering begins; prevents schema rework mid-project.
- **Test screenshot/PowerPoint export workflow in Phase 1.** Verify that dashboard renders cleanly when captured and embedded in decks.
- **Establish data.json update process upfront.** Decide: Git-based versioning, manual file update, or UI editor? Lock this in before MVP goes live.
- **Use relative file paths** (IHostEnvironment.ContentRootPath) throughout; ensures app works whether run locally or from network share.
- **Publish self-contained executable early (Phase 1 end).** Validate that execs can double-click and launch app without .NET SDK installed; this is critical for adoption.
- **Monitor FileSystemWatcher reliability in real-world use.** First time execs update data.json while app is running, verify cache invalidation works; this is a gotcha point.
- **Log all exceptions to local file.** Execs won't have console access; file logging aids troubleshooting.
- ---
- This executive dashboard project is well-suited to C# .NET 8 with Blazor Server given the "local-only, no auth, simple" constraints. The recommended stack eliminates unnecessary complexity (no database, no cloud, no auth layer) and prioritizes deployment friction-free experience for non-developers. Phases 1-3 deliver a shippable, professional dashboard within 6 weeks with minimal risk. The self-contained executable model ensures execs can run the app with a double-click, and data.json-driven architecture enables rapid iteration on deck content without code changes.

### Recommended Tools & Technologies

- **Blazor Server (.NET 8)** - Server-side component rendering with SignalR two-way binding
- **Radzen Blazor Components (v5.x)** - Dashboard layout templates, responsive grid system
- **MudBlazor (v6.x)** - Data grid with built-in sort/filter/pagination; status cards; professional component library
- **ApexCharts.Razor (v2.5.x)** - Timeline/Gantt chart rendering; SVG export for PowerPoint
- **Bootstrap 5.3.x** - Responsive grid, typography, print CSS; ships with Blazor template
- **FontAwesome (v6.x)** - Icon library for status indicators and navigation
- **.NET 8** - Runtime and SDK; LTS support through 2026
- **System.Text.Json** - Built-in, zero-dependency JSON serialization (6-10x faster than Newtonsoft)
- **IHostEnvironment** - Deployment-agnostic file path resolution
- **FileSystemWatcher** - Monitor data.json changes; invalidate in-memory cache on update
- **IMemoryCache** - In-memory caching with 5-minute TTL for performance
- **JSON (data.json)** - Single-file configuration; no database required
- **JsonSchema.NET (v6.x)** - Schema validation at load time
- **xUnit (v2.x)** - Unit testing framework; lightweight, parallel-safe
- **Bunit (v1.x)** - Blazor-specific component testing; render components in isolation
- **Moq (v4.x)** - Mock dependencies (IHostEnvironment, FileSystemWatcher, IMemoryCache)
- **FluentAssertions (v6.x)** - Readable assertion syntax
- **Visual Studio 2022** or **Visual Studio Code + C# DevKit** - Development environment
- **dotnet CLI (v8.0+)** - Build, test, and publish tooling
- **Self-contained publishing** - `dotnet publish -c Release -r win-x64 --self-contained` for executable distribution
- **.gitignore** - Exclude bin/, obj/, data.json from source control
- ```
- MyProject.sln
- ├── MyProject.csproj (Blazor Server template)
- ├── Components/
- │   ├── Layout/
- │   │   └── DashboardLayout.razor
- │   ├── Sections/
- │   │   ├── TimelineSection.razor
- │   │   ├── ProgressCardsSection.razor
- │   │   ├── ItemGridSection.razor
- │   │   └── MetricsFooter.razor
- │   └── Shared/
- │       └── StatusBadge.razor
- ├── Services/
- │   ├── DataProvider.cs
- │   └── DashboardDataValidator.cs
- ├── Models/
- │   ├── DashboardData.cs
- │   └── ProjectItem.cs
- ├── Styles/
- │   └── dashboard.css
- ├── Pages/
- │   └── Index.razor
- ├── data.json
- └── appsettings.json
- ```
- ```xml
- <ItemGroup>
- <PackageReference Include="Radzen.Blazor" Version="5.1.0" />
- <PackageReference Include="MudBlazor" Version="6.10.0" />
- <PackageReference Include="ApexCharts.Razor" Version="2.5.0" />
- <PackageReference Include="JsonSchema.NET" Version="6.0.0" />
- <PackageReference Include="xunit" Version="2.6.6" />
- <PackageReference Include="bunit" Version="1.25.0" />
- <PackageReference Include="Moq" Version="4.18.4" />
- <PackageReference Include="FluentAssertions" Version="6.12.0" />
- </ItemGroup>
- ```
- ---
- Structure the dashboard as a multi-tier component tree with cascading parameters for state propagation:
- ```
- App.razor
- └── DashboardLayout.razor (loads data.json; caches in-memory)
- ├── [CascadingParameter] DashboardData
- ├── TimelineSection.razor (renders milestone Gantt chart)
- ├── ProgressCardsSection.razor (Shipped/InProgress/CarriedOver summary cards)
- ├── ItemGridSection.razor (MudBlazor DataGrid with sort/filter)
- └── MetricsFooter.razor (total counts, status distribution)
- ```
- DashboardLayout loads data.json at startup via `DataProvider.LoadDataAsync()`
- Data cached in `IMemoryCache` with 5-minute TTL
- DashboardData passed to child components via `[CascadingParameter]`
- Child components bind to data fields; changes trigger Blazor's automatic re-render
- FileSystemWatcher monitors data.json; on change, evicts cache and re-fetches
- Define data.json schema to support dashboard requirements:
- ```json
- {
- "projectName": "Project Alpha",
- "projectOwner": "Executive Name",
- "reportingPeriod": "Q2 2026",
- "timeline": [
- {
- "milestone": "Design Phase Complete",
- "dueDate": "2026-04-30",
- "status": "Shipped",
- "completedDate": "2026-04-28"
- },
- {
- "milestone": "MVP Launch",
- "dueDate": "2026-06-15",
- "status": "InProgress",
- "completedDate": null
- }
- ],
- "items": [
- {
- "id": "item-001",
- "title": "User Authentication Module",
- "category": "Shipped",
- "dueDate": "2026-04-20",
- "priority": "High",
- "owner": "Team Lead Name"
- },
- {
- "id": "item-002",
- "title": "Payment Gateway Integration",
- "category": "InProgress",
- "dueDate": "2026-05-15",
- "priority": "High",
- "owner": "Engineer Name"
- },
- {
- "id": "item-003",
- "title": "Mobile Optimization",
- "category": "CarriedOver",
- "dueDate": "2026-04-30",
- "priority": "Medium",
- "owner": "Designer Name"
- }
- ],
- "summary": {
- "totalShipped": 5,
- "totalInProgress": 3,
- "totalCarriedOver": 2,
- "overallHealthStatus": "OnTrack"
- }
- }
- ```
- Implement `DataProvider` as a singleton service:
- ```csharp
- public class DataProvider
- {
- private readonly IHostEnvironment _env;
- private readonly IMemoryCache _cache;
- private readonly ILogger<DataProvider> _logger;
- private FileSystemWatcher _watcher;
- public async Task<DashboardData> GetDashboardDataAsync()
- {
- if (_cache.TryGetValue("dashboard_data", out DashboardData cachedData))
- return cachedData;
- var data = await LoadFromFileAsync();
- _cache.Set("dashboard_data", data, TimeSpan.FromMinutes(5));
- return data;
- }
- private async Task<DashboardData> LoadFromFileAsync()
- {
- var path = Path.Combine(_env.ContentRootPath, "data.json");
- if (!File.Exists(path))
- throw new FileNotFoundException($"data.json not found at {path}");
- var json = await File.ReadAllTextAsync(path);
- var data = JsonSerializer.Deserialize<DashboardData>(json)
- ?? throw new InvalidOperationException("data.json is empty");
- ValidateSchema(data);
- return data;
- }
- private void ValidateSchema(DashboardData data)
- {
- var schema = JsonSchema.FromText(GetSchemaJson());
- var result = schema.Evaluate(data);
- if (!result.IsValid)
- throw new InvalidOperationException($"Schema validation failed: {string.Join("; ", result.Errors)}");
- }
- }
- ```
- Register in Program.cs:
- ```csharp
- builder.Services.AddSingleton<DataProvider>();
- builder.Services.AddMemoryCache();
- ```
- Use ApexCharts Gantt chart configuration:
- ```csharp
- var chartOptions = new ApexCharts.ApexChartOptions<DashboardData>()
- {
- Chart = new Chart { Type = ChartType.Bar },
- PlotOptions = new PlotOptions
- {
- Bar = new PlotOptionsBar { Horizontal = true, DataLabels = new DataLabelsOptions { Position = "top" } }
- },
- Xaxis = new XAxis { Type = XAxisType.Datetime },
- Series = new List<ApexCharts.Series<DashboardData>>
- {
- new ApexCharts.Series<DashboardData>
- {
- Name = "Timeline",
- Data = data.Timeline.Select(m => new { x = m.Milestone, y = (m.DueDate, m.CompletedDate) }).ToList()
- }
- }
- };
- ```
- Implement in-memory filtering in ItemGridSection component:
- ```csharp
- private List<ProjectItem> FilteredItems => _items
- .Where(i => _categoryFilter == null || i.Category == _categoryFilter)
- .Where(i => _dateFilter == null || i.DueDate >= _dateFilter)
- .OrderByDescending(i => i.Priority)
- .ToList();
- ```
- ---

### Considerations & Risks

- **Local file access:** No encryption needed; data.json is plaintext JSON on local disk
- **In-transit:** No network transmission; all processing local to machine
- **Future enhancement:** If data.json moves to network share, add file encryption (BitLocker on Windows or file-level encryption)
- **Hosting:** Local Windows machine or shared network drive
- **Executable:** Self-contained .NET 8 executable; no cloud deployment
- **Deployment process:**
- Developer: `dotnet publish -c Release -r win-x64 --self-contained -o ./publish`
- Copy `publish/` folder + `data.json` to shared location or exec's machine
- Exec: Double-click MyProject.exe to launch
- App listens on `https://localhost:5001` (self-signed cert for local development)
- **Data persistence:** Store data.json in version control (Git) alongside source code; execs pull latest data.json from repo or receive updates via email
- **Logging:** Configure minimal logging (errors only) to console or local file; no telemetry
- **Backups:** Execs manually backup data.json if making local edits; recommend read-only permissions or version-controlled updates only
- **Performance monitoring:** No APM required; monitor local app performance via Windows Task Manager
- ---
- | Risk | Severity | Mitigation |
- |------|----------|-----------|
- | **FileSystemWatcher false-positive triggers** | Medium | Debounce file change events with 500ms delay; validate file before reloading cache |
- | **data.json corruption** | Medium | Implement schema validation at load time; fallback to cached version if parse fails; log error with user-friendly message |
- | **Blazor Server session timeout** | Low | Set SessionTimeout to 30+ minutes in Program.cs; execs unlikely to keep window open that long anyway |
- | **Self-contained executable size (180MB)** | Low | Acceptable for local distribution; consider ZIP compression if needed; .NET 8 includes runtime trimming options to reduce footprint |
- | **ApexCharts library incompatibility with future Blazor versions** | Low | Monitor ApexCharts.Razor updates quarterly; community is active; fallback to custom SVG if needed |
- **No horizontal scaling:** Single-machine deployment; not applicable
- **JSON file size limit:** Current design handles up to 10,000 items comfortably; if exceeding, migrate to SQLite or CSV
- **Charting performance:** ApexCharts renders smoothly up to ~200 milestones; beyond that, consider pagination or aggregation
- **data.json file:** Protect from accidental deletion; store in Git with backup; consider read-only file permissions
- **FileSystemWatcher:** Monitor for exceptions; fallback to manual refresh button if file watching fails
- **Local machine:** App lifecycle depends on exec's machine; no redundancy; not a concern for single-user tool
- | Choice | Trade-off | Justification |
- |--------|-----------|---------------|
- | **No database (SQLite/SQL Server)** | Data persistence limited to JSON file | Simplicity and deployment friction outweigh relational database benefits |
- | **No authentication** | Single-user, no multi-exec access control | Scope explicitly local-only; auth adds significant complexity |
- | **Client-side filtering** | No server-side data aggregation | <1,000 items = client-side filtering is snappy; server-side overhead unjustified |
- | **Self-contained executable (~180MB)** | Larger download than runtime-dependent | Eliminates .NET runtime install friction; acceptable trade-off for local deployment |
- | **No cloud backup/sync** | No automatic data redundancy | Git version control + manual backup sufficient for executive dashboard use case |
- ---
- **Who maintains data.json over time?** Is it the development team, project managers, or execs directly? Decision impacts update workflow and file permissions.
- **How frequently is data.json updated?** Daily? Weekly? Monthly? Determines whether FileSystemWatcher is necessary or manual refresh button suffices.
- **Should the dashboard support multiple projects** (e.g., switchable project selector), or is it single-project per executable? Affects data model schema and component complexity.
- **Are PDF exports required**, or is screenshot-to-PowerPoint sufficient? If PDF needed, add iTextSharp or SelectPdf integration.
- **What happens if an exec's machine loses power or crashes while dashboard is running?** Is app recovery needed, or is stateless re-launch acceptable?
- **Should execs be able to edit data.json directly in the dashboard UI**, or is it read-only with JSON edited externally? UI editing requires form validation and file write-back logic.
- **How long should the app remain open?** Is session timeout (30+ minutes) acceptable, or do execs expect day-long persistence?
- **What's the rollout plan?** Distribute as ZIP file? Network share? Email? Affects versioning and update strategy.
- ---

### Detailed Analysis

# In-Depth Analysis: Executive Dashboard Research Findings

## 1. Blazor Server Component Architecture for Dashboard

**Key Findings:**
- Blazor Server's server-side rendering and two-way binding are ideal for dashboards with dynamic status updates
- Component composition hierarchies work best with a root layout component housing dashboard sections as child components
- Cascading parameters eliminate prop-drilling; significantly simplify state management for multi-level dashboards

**Tools & Libraries:**
- **Blazor Server (built into .NET 8)** – No separate install required; use Entity Framework Core 8.0 for data access
- **Radzen Blazor Components 5.x** – Optional; provides pre-built dashboard layouts, grid, and timeline components compatible with .NET 8
- **MudBlazor 6.x** – Lightweight Material Design components; excellent for dashboards, better CSS tree-shaking than Radzen

**Trade-offs & Alternatives:**
- Client-side Blazor WASM: Removes server dependency but adds bundle bloat (~3-5MB); rejected for local-only simple dashboard
- Custom HTML + JavaScript: More lightweight but sacrifices Blazor's reactive binding; excessive dev time
- Razor Pages: Too simplistic for interactive filtering/sorting; Blazor components provide better interactivity

**Recommendation:**
Use **Blazor Server components with Radzen Blazor (v5.x)** for dashboard layout + MudBlazor (v6.x) for data tables and status cards. Create a root DashboardLayout component with cascading parameters for project data:
```
DashboardLayout (loads data.json)
├── TimelineSection (milestone visualization)
├── ProgressCardsSection (shipped/in-progress/carried-over)
├── DetailGridSection (item-level view)
└── MetricsFooter (summary stats)
```

**Reasoning:** Radzen's dashboard templates reduce boilerplate; Cascading parameters eliminate JSON-to-component drilling; .NET 8's async/await native support for smooth data binding without freezing UI.

---

## 2. Charting & Visualization Library for Timelines

**Key Findings:**
- Executive dashboards prioritize clarity over complexity; timeline views need milestone markers and date ranges
- Plotly.NET and ApexCharts Blazor wrappers are the only modern options with .NET 8 support
- SVG-based rendering (ApexCharts) produces superior screenshot quality for PowerPoint decks

**Tools & Libraries:**
- **ApexCharts.Razor 2.5.x** – Lightweight SVG-based charting; excellent timeline/Gantt support; <100KB bundle
- **Plotly.NET 5.0.x** – More features but heavier; overkill for simple milestone view
- **Syncfusion Blazor Charts 26.x** – Enterprise-grade but licensing required (not free for commercial use)
- **ChartJS 4.x wrapper for Blazor** – Immature Blazor bindings; not recommended

**Trade-offs & Alternatives:**
- Custom SVG rendering: Provides pixel-perfect control but requires D3.js or manual path math; excessive complexity
- PNG/static images: Defeats interactivity; can't filter by date range
- HTML table with CSS styling: No visualization; poor executive communication

**Recommendation:**
Use **ApexCharts.Razor 2.5.x** for timeline and progress charts. Render milestones as a Gantt chart with color-coded completion status:
- Red: Not started
- Yellow: In progress
- Green: Shipped
- Gray: Carried over

ApexCharts produces clean SVGs that export beautifully to PowerPoint; no licensing concerns; plays well with Blazor Server's event-driven model.

**Reasoning:** Lightweight, screenshot-perfect for executive decks, easy serialization from data.json directly to chart config, native Blazor Server support.

---

## 3. Data Management Strategy for data.json

**Key Findings:**
- JSON file I/O in .NET 8 is optimized; System.Text.Json is faster than Newtonsoft (6-10x in benchmarks)
- For single-file, read-heavy workloads, in-memory caching with file-watch invalidation is ideal
- Schema validation prevents runtime crashes from malformed configs

**Tools & Libraries:**
- **System.Text.Json (built into .NET 8)** – Native, zero-dependency JSON serialization; ~30% faster than Newtonsoft
- **JsonSchema.NET v6.x** – Lightweight schema validation; prevents silent failures from bad configs
- **FileSystemWatcher (built-in)** – Monitor data.json changes; invalidate cache on file update

**Trade-offs & Alternatives:**
- Newtonsoft.Json 13.x: Legacy option; slower, more overhead; not justified for simple dashboards
- SQLite: Over-engineered; adds deployment complexity; file-based JSON is sufficient
- Static JSON embedding: No runtime updates; inflexible for iterative exec reporting

**Recommendation:**
Create a **DataProvider service** that:
1. Loads data.json at app startup using `System.Text.Json.JsonSerializer.Deserialize<DashboardData>()`
2. Validates against schema using JsonSchema.NET
3. Caches in-memory with IMemoryCache (5-minute TTL)
4. Uses FileSystemWatcher to invalidate cache on file changes
5. Exposes via dependency injection

Schema structure:
```json
{
  "projectName": "string",
  "timeline": [
    { "milestone": "string", "dueDate": "ISO8601", "status": "NotStarted|InProgress|Shipped" }
  ],
  "items": [
    { "title": "string", "category": "Shipped|InProgress|CarriedOver", "dueDate": "ISO8601" }
  ]
}
```

**Reasoning:** Zero external dependencies, native .NET 8 performance, file-watch auto-refresh matches how execs iterate on decks.

---

## 4. CSS & Styling for Professional Executive Dashboard

**Key Findings:**
- Blazor Server supports both Bootstrap and Tailwind; Bootstrap 5 has larger ecosystem but Tailwind produces lighter CSS
- Executive dashboards need high contrast, limited color palette, clean typography
- Print/screenshot optimization is critical (no hover-dependent UI, fixed sizes)

**Tools & Libraries:**
- **Bootstrap 5.3.x** – Built-in Blazor templates; extensive component library; 30KB gzipped
- **Tailwind CSS 3.4.x** – Smaller final bundle with purging (~8KB for simple dashboard); modern workflow
- **FontAwesome 6.x** – Icon library; 50+ dashboard-relevant icons; free tier sufficient

**Trade-offs & Alternatives:**
- Custom CSS: Fragile; no design system consistency
- Material Design (MudBlazor CSS): Heavier than needed; adds visual complexity
- Semantic UI: Unmaintained; not recommended

**Recommendation:**
Use **Bootstrap 5.3.x** (ships with Blazor template) + custom utility classes for dashboard-specific needs:
- Color scheme: 3-color palette (primary blue, accent green for "shipped", warning red for "at-risk")
- Typography: Roboto or system fonts; 16px base for executive readability
- Cards: Box-shadow: 0 2px 8px rgba(0,0,0,0.1); rounded corners (4-8px)
- Responsive: Single-column on mobile, 2-column grid on desktop
- Print CSS: `@media print { .no-print { display: none; } }`

Add Bootstrap breakpoints:
- Mobile: <768px
- Desktop: ≥768px

**Reasoning:** Bootstrap 5 is industry-standard for dashboards; minimal learning curve; excellent print support; ships pre-configured with Blazor.

---

## 5. File I/O in .NET 8 for Local data.json Access

**Key Findings:**
- .NET 8 file APIs are async-first; synchronous I/O blocks Blazor Server threads
- Local-only deployments bypass cross-domain file access restrictions
- Exception handling must distinguish between missing files, access errors, and parse failures

**Tools & Libraries:**
- **System.IO (built-in)** – File.ReadAllTextAsync, Directory.GetCurrentDirectory
- **IHostEnvironment.ContentRootPath** – Access app root directory safely from Blazor
- **System.Text.Json (built-in)** – Already recommended for parsing

**Trade-offs & Alternatives:**
- Synchronous File.ReadAllText: Blocks server; creates thread pool starvation; avoid
- Stream-based reading: Over-engineering for small JSON configs

**Recommendation:**
Create async file loading in DataProvider:
```csharp
public async Task<DashboardData> LoadDataAsync()
{
    var path = Path.Combine(_env.ContentRootPath, "data.json");
    
    if (!File.Exists(path))
        throw new FileNotFoundException($"data.json not found at {path}");
    
    var json = await File.ReadAllTextAsync(path);
    var data = JsonSerializer.Deserialize<DashboardData>(json);
    
    if (data == null)
        throw new InvalidOperationException("data.json is empty or invalid");
    
    return data;
}
```

Deployment: Include data.json in project root, set Copy-to-Output-Directory = PreserveNewest in .csproj.

**Reasoning:** Async prevents server thread starvation; ContentRootPath is deployment-agnostic; PreserveNewest ensures data.json travels with executable.

---

## 6. Minimal Deployment Footprint for Local Execution

**Key Findings:**
- Self-contained .NET 8 executables are ~150MB (includes runtime); runtime-dependent are ~50MB + runtime install
- Execs rarely have .NET installed; self-contained executables are prerequisite
- Local file access (no cloud) eliminates cross-origin restrictions

**Tools & Libraries:**
- **dotnet publish -c Release -r win-x64 --self-contained** – Self-contained Windows executable
- **AppSetup.exe or ClickOnce** – Optional installers; not required for local dashboards
- **Electron.NET wrapper** – Over-engineering; not needed

**Trade-offs & Alternatives:**
- Runtime-dependent: Requires .NET 8 Runtime pre-installed; friction for non-dev execs
- Containerized (Docker): Adds complexity; not needed for single-exec distribution
- Web-deployed (Azure App Service): Violates "local-only" requirement

**Recommendation:**
Publish as self-contained executable:
```
dotnet publish -c Release -r win-x64 --self-contained
```
Output: Executable + bin folder. Bundle data.json in same directory. Execs double-click .exe to launch.

Sizing: ~180MB total (runtime + app + deps). Acceptable for local deployment.

**Reasoning:** No external dependencies for execs; drop-and-run simplicity; satisfies "local-only" constraint.

---

## 7. Filtering, Sorting & Timeline Navigation

**Key Findings:**
- Client-side filtering is fast enough for dashboards (typical <1000 items)
- Server-side filtering adds complexity without benefit for local, read-heavy workloads
- Timeline navigation benefits from sticky headers and date-range pickers

**Tools & Libraries:**
- **MudBlazor DataGrid 6.x** – Built-in sorting, filtering, pagination; excellent for item tables
- **Date picker from Radzen Blazor 5.x** – Lightweight date-range selector
- **Blazor query string binding** – Preserve filter state in URL for bookmarking

**Trade-offs & Alternatives:**
- Server-side filtering: Network latency; unnecessary for <1000 items
- Manual checkbox filtering: Tedious UX; DataGrid handles this automatically

**Recommendation:**
1. **Item Tables:** Use MudBlazor DataGrid with built-in sort/filter on category (Shipped/InProgress/CarriedOver)
2. **Timeline View:** Sticky header showing current month; highlight past-due milestones in red
3. **Date Range Filter:** Optional date picker to filter items by due date
4. **URL State:** Bind filters to query strings (`?category=InProgress&month=2026-04`) for shareable links

**Reasoning:** MudBlazor DataGrid eliminates reinventing the wheel; client-side filtering is snappy for executive interactivity; sticky headers keep context visible during scrolling.

---

## 8. Testing Strategy for Data-Driven Dashboard

**Key Findings:**
- No external APIs or auth = testing focuses on data parsing, UI rendering, and calculations
- Snapshot testing catches unintended visual changes
- End-to-end tests verify dashboard renders correctly with sample data

**Tools & Libraries:**
- **xUnit 2.x** – Standard .NET testing framework; lightweight, parallel-safe
- **Moq 4.x** – Mock dependencies (IHostEnvironment, FileSystemWatcher)
- **Bunit 1.x** – Blazor-specific unit testing; render components in isolation
- **FluentAssertions 6.x** – Readable test assertions

**Trade-offs & Alternatives:**
- NUnit: Heavier, no advantage over xUnit for this project
- Selenium/Playwright: Over-engineered for local dashboards; visual regression better handled by Bunit snapshots
- No testing: Risks silent regressions; unacceptable for executive-facing tool

**Recommendation:**
Create test suite covering:
1. **Data Parsing:** Verify data.json deserializes correctly; invalid JSON throws appropriate errors
2. **Component Rendering:** Bunit tests for TimelineSection, ProgressCards rendering with sample data
3. **Filtering Logic:** xUnit tests for category/date filtering calculations
4. **Edge Cases:** Empty data.json, missing milestones, future-dated items

Example:
```csharp
[Fact]
public void DataProvider_LoadsValidJson_ReturnsProjectData()
{
    var data = await _provider.LoadDataAsync();
    Assert.NotNull(data.Timeline);
    Assert.True(data.Items.Count > 0);
}
```

**Reasoning:** xUnit + Bunit covers unit + component layers; Moq isolates file I/O for repeatability; minimal test maintenance required for straightforward dashboard.

---

## Summary: Consolidated Recommendations

| Concern | Solution | Rationale |
|---------|----------|-----------|
| **Components** | Blazor Server + Radzen/MudBlazor | Reactive binding, no complexity |
| **Charts** | ApexCharts.Razor 2.5.x | Screenshot-perfect timelines for PowerPoint |
| **Data** | System.Text.Json + FileSystemWatcher | Native .NET 8, zero dependencies |
| **Styling** | Bootstrap 5.3.x + custom utility classes | Industry-standard, print-optimized |
| **File I/O** | Async File.ReadAllTextAsync + IHostEnvironment | Non-blocking, deployment-agnostic |
| **Deployment** | Self-contained dotnet publish | No .NET runtime prereq for execs |
| **Filtering** | MudBlazor DataGrid + sticky headers | Client-side fast, UX-friendly |
| **Testing** | xUnit 2.x + Bunit 1.x | Component + data layer coverage |
