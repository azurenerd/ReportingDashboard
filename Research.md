# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-10 13:57 UTC_

### Summary

The executive reporting dashboard should be built on **Blazor Server (.NET 8)** with hand-built components styled using **Tailwind CSS**, paired with **System.Text.Json** for JSON parsing and **FileSystemWatcher** for automatic data refresh. This stack prioritizes simplicity and screenshot quality while eliminating unnecessary complexity for a local-only, non-authenticated dashboard. The three-project structure (Core/Web/Tests) provides clean separation of concerns and enables future scalability without initial overhead. Estimated delivery: 3-4 weeks for feature-complete MVP. ---

### Key Findings

- **Blazor Server is optimal for this use case.** Its stateful, synchronous component model eliminates JavaScript complexity while providing real-time data binding. The local-only constraint removes typical Blazor Server bottlenecks (server memory at scale), making it the simplest path to a professional dashboard.
- **Hand-built components + Tailwind CSS provide maximum control for PowerPoint screenshot quality.** Pre-built component libraries (Radzen, MudBlazor, Telerik) are feature-rich but introduce unnecessary weight. For an executive dashboard where visual simplicity is a requirement, custom components with Tailwind utilities are lighter, more maintainable, and more aligned with your stated goal.
- **FileSystemWatcher + IMemoryCache is the superior data refresh pattern.** Event-driven refresh via OS-level file monitoring (<100ms latency) beats polling (2.5s average). No database complexity needed; JSON files are sufficient for local-only deployment. System.Text.Json (built-in, zero dependencies) handles parsing 2-3x faster than Newtonsoft.Json.
- **CSS-based timeline/milestone visualization is ideal.** Pure Tailwind flex/grid components eliminate charting library overhead while producing clean, scannable timelines optimized for screenshots. OxyPlot.Blazor is available as a fallback if date-axis scaling becomes critical, but not required for MVP.
- **Three-project structure (Core/Web/Tests) scales better than monolithic approaches.** Separating business logic (DataService, Models) from UI (Blazor components) enables testability, code reuse, and future API exposure without architectural rework. This mirrors enterprise .NET patterns and aligns with existing AgentSquad.sln context.
- **Testing strategy prioritizes integration over unit tests.** bUnit for component rendering (Does MilestoneTimeline display correctly?) + xUnit for DataService logic (Does JSON parsing work?) provides better ROI than exhaustive unit test coverage. Target 70%+ coverage on data services, 40%+ on components.
- **Performance is not a bottleneck for typical executive dashboards.** Blazor Server handles <500 data points and <10 components trivially. Premature optimization (virtualization, lazy-loading) introduces complexity; measure first, optimize only if degradation occurs.
- **Security posture is minimal and appropriate.** Local-only deployment with no authentication or cloud infrastructure eliminates enterprise security concerns. File-system access control (NTFS permissions) is the only meaningful protection needed for proprietary data in data.json.
- ---
- Create three-project structure (Core, Web, Tests) in AgentSquad.sln.
- Implement ProjectData and Milestone models in Core project.
- Implement DataService with FileSystemWatcher and IMemoryCache in Core project.
- Create sample data.json with 3 milestones, 5 status items, and summary data.
- Setup Tailwind CSS in Web project; create global stylesheet.
- Write xUnit tests for DataService (JSON parsing, caching, file monitoring).
- Implement MilestoneTimeline.razor using Tailwind flex layout.
- Implement StatusSummary.razor (four cards: Completed, InProgress, CarriedOver, Blocked).
- Implement StatusCards.razor (grid of individual work items with status badges).
- Implement ProgressBars.razor (HTML5 <progress> + Tailwind styling).
- Implement Dashboard.razor root component; integrate all child components.
- Setup routing in Program.cs; ensure default route points to Dashboard.
- Write bUnit tests for each component (rendering, data binding).
- Review PowerPoint screenshot quality; adjust Tailwind colors and spacing as needed.
- Add optional ChartSection.razor if OxyPlot.Blazor visualization needed.
- Optimize CSS; minimize unused Tailwind classes via PurgeCSS (optional).
- Test data refresh: modify data.json file; verify UI updates within 100-200ms.
- Performance testing: measure initial load time (<4s target) and render cycles.
- Write integration tests (component + mock data service).
- Setup Windows Service or IIS deployment script.
- Document deployment steps for non-technical users.
- Create sample data.json with 6+ milestones and 10+ status items.
- Test on target server (Windows Server or local machine).
- Optimize for PowerPoint: adjust component sizes, colors, and layout for screenshot quality.
- **Week 1 milestone:** Working DataService + sample data.json + Tailwind CSS setup. Show team the architecture works.
- **Week 1.5 deliverable:** Functional MilestoneTimeline component. Screenshot for PowerPoint preview.
- **Week 2 deliverable:** Full Dashboard with all four component types. Ready for PowerPoint integration.
- **Prototype MilestoneTimeline rendering:** Build three timeline variations (horizontal, vertical, compact) using pure CSS. Test with executives to confirm visual preference before committing to component.
- **Prototype color scheme:** Create Tailwind color palette samples (3-4 variations) matching PowerPoint brand guidelines. Validate before full component development.
- **Prototype data.json schema:** Create 3-4 sample data.json files with different structures (simple vs. detailed, few vs. many items). Confirm schema flexibility before finalizing models.
- **Chart visualization:** If OxyPlot.Blazor charts needed, prototype burn-down/burn-up charts early (Week 2.5); charting libraries have steeper learning curves.
- **Data export/reporting:** If executives need CSV exports or PDF reports, add in Phase 4; not critical for initial screenshot dashboards.
- **Mobile responsiveness:** Low priority for internal dashboard, but Tailwind responsive classes included by default; test on mobile if needed.
- **Real-time collaboration:** If multiple users editing data.json simultaneously, add conflict resolution; use file version tracking (optional, Phase 4+).
- ✓ Dashboard loads in <4 seconds.
- ✓ MilestoneTimeline renders clearly; milestones scannable at a glance.
- ✓ Status cards show correct status badges and colors.
- ✓ Data refresh detects data.json changes within 200ms.
- ✓ PowerPoint screenshots are clean, professional, and suitable for executive presentations.
- ✓ Code has 70%+ coverage on Core (DataService), 40%+ on Web (components).
- ✓ Deployment documented; non-technical user can start dashboard via script or service.
- ---
- ```bash
- dotnet new classlib -n AgentSquad.Dashboard.Core -f net8.0
- dotnet new blazorserver -n AgentSquad.Dashboard.Web -f net8.0
- dotnet new xunit -n AgentSquad.Dashboard.Tests -f net8.0
- dotnet sln add AgentSquad.Dashboard.Core/AgentSquad.Dashboard.Core.csproj
- dotnet sln add AgentSquad.Dashboard.Web/AgentSquad.Dashboard.Web.csproj
- dotnet sln add AgentSquad.Dashboard.Tests/AgentSquad.Dashboard.Tests.csproj
- dotnet add AgentSquad.Dashboard.Web/AgentSquad.Dashboard.Web.csproj reference AgentSquad.Dashboard.Core/AgentSquad.Dashboard.Core.csproj
- dotnet add AgentSquad.Dashboard.Tests/AgentSquad.Dashboard.Tests.csproj reference AgentSquad.Dashboard.Core/AgentSquad.Dashboard.Core.csproj
- dotnet add AgentSquad.Dashboard.Tests/AgentSquad.Dashboard.Tests.csproj reference AgentSquad.Dashboard.Web/AgentSquad.Dashboard.Web.csproj
- dotnet add AgentSquad.Dashboard.Web/AgentSquad.Dashboard.Web.csproj package OxyPlot.Blazor --version 2.1.0
- dotnet add AgentSquad.Dashboard.Tests/AgentSquad.Dashboard.Tests.csproj package bunit --version 1.25.0
- dotnet add AgentSquad.Dashboard.Tests/AgentSquad.Dashboard.Tests.csproj package Moq --version 4.20.0
- dotnet add AgentSquad.Dashboard.Tests/AgentSquad.Dashboard.Tests.csproj package FluentAssertions --version 6.12.0
- dotnet build
- dotnet run --project AgentSquad.Dashboard.Web/AgentSquad.Dashboard.Web.csproj
- ```

### Recommended Tools & Technologies

- **Blazor Server (.NET 8.0.0+)** — Built-in; zero external dependencies. Provides stateful, real-time component rendering with C# logic server-side.
- **ASP.NET Core 8.0.0** — Built-in; included with .NET 8 SDK.
- **Tailwind CSS 3.4+** — Utility-first CSS framework. Zero JavaScript runtime; works via static CSS build. Enables rapid, responsive component styling. Version: Latest stable (3.4+).
- **OxyPlot.Blazor 2.1.0+** — Lightweight charting library for Blazor Server. Use if sophisticated timeline visualization or date-axis scaling is needed. Optional for MVP.
- **System.Text.Json 8.0.0** — Built-in; zero external dependency. Fastest JSON serialization in .NET. Use for data.json parsing in DataService.
- **System.IO.FileSystemWatcher** — Built-in; OS-level file monitoring. Detects data.json changes without polling.
- **Microsoft.Extensions.Caching.Memory 8.0.0** — Built-in. Provides IMemoryCache for in-process caching of parsed JSON.
- **Microsoft.Extensions.DependencyInjection 8.0.0** — Built-in; built into ASP.NET Core. Use for service registration (DataService, IMemoryCache).
- **xUnit 2.6.0+** — De facto standard unit testing framework in .NET. Used for DataService and business logic tests.
- **bUnit 1.25.0+** — Purpose-built Razor component testing library (xUnit-based). Use for MilestoneTimeline.razor, StatusCards.razor, ProgressBars.razor rendering tests.
- **Moq 4.20.0+** — Mocking framework. Mock IMemoryCache and FileSystemWatcher in tests.
- **FluentAssertions 6.12.0+** — Fluent assertion API. Improves test readability; optional but recommended.
- **Microsoft.NET.Sdk 8.0.0** — Standard .NET class library SDK (for Core project).
- **Microsoft.NET.Sdk.BlazorWebAssembly 8.0.0** — Blazor Server SDK (for Web project). Note: Use BlazorWebAssembly for Blazor Server hosting.
- **dotnet CLI 8.0.0+** — Build and publish via `dotnet build`, `dotnet publish`.
- **Windows Server 2019+** or **Windows 10/11** — Target environment. .NET 8 Runtime required on host.
- **IIS 10.0+** (optional) — If deploying to IIS; Blazor Server works via WebSocket + ASP.NET Core hosting.
- **Self-hosted (Kestrel)** — Simpler for local deployment; run `dotnet run` or create Windows Service wrapper.
- **Local file system** — data.json stored locally; no cloud storage needed.
- **No database** — JSON files sufficient for configuration storage.
- **Microsoft.Extensions.Logging 8.0.0** — Built-in. Basic console/debug logging for diagnostics.
- **Serilog (optional)** — v3.0.0+ if structured logging becomes necessary; not required for MVP.
- ---
- ```
- DashboardLayout.razor (root)
- ├── MilestoneTimeline.razor
- │   └── [CSS-based horizontal timeline with status indicators]
- ├── StatusSummary.razor
- │   └── [Four cards: Completed, InProgress, CarriedOver, Blocked]
- ├── StatusCards.razor (or DetailedStatusGrid.razor)
- │   └── [Flexbox grid of individual work items with status badges]
- ├── ProgressBars.razor
- │   └── [Visual progress indicators for overall completion]
- └── [Optional] ChartSection.razor (using OxyPlot.Blazor)
- └── [Burndown or velocity charts if needed]
- DataService (Singleton)
- ├── GetProjectData() → ProjectData (IMemoryCache-backed)
- ├── InitializeFileWatcher() → Monitors data.json
- └── OnDataChanged event → Triggers Dashboard.razor StateHasChanged()
- Models (in Core project)
- ├── ProjectData.cs
- ├── Milestone.cs
- ├── StatusItem.cs
- └── ProjectSummary.cs
- ```
- **Initialization:** Dashboard.razor injects DataService, calls GetProjectData() in OnInitializedAsync().
- **Caching:** DataService caches parsed JSON in IMemoryCache; subsequent calls return cached data (<1ms lookup).
- **File Monitoring:** FileSystemWatcher detects data.json changes, removes cache entry, raises OnDataChanged event.
- **UI Refresh:** Dashboard listens to DataService.OnDataChanged, calls StateHasChanged() to re-render.
- **Rendering:** Blazor components receive ProjectData as parameter, render to HTML with Tailwind classes.
- ```
- ├── Models/
- │   ├── ProjectData.cs
- │   ├── Milestone.cs
- │   ├── StatusItem.cs
- │   └── ProjectSummary.cs
- ├── Services/
- │   └── DataService.cs (JSON parsing, caching, file monitoring)
- ├── Constants/
- │   └── DashboardConfig.cs
- └── Interfaces/
- └── IDataService.cs (optional; for testability)
- ```
- ```
- ├── Components/
- │   ├── Dashboard.razor (main container)
- │   ├── MilestoneTimeline.razor
- │   ├── StatusCards.razor
- │   ├── StatusSummary.razor
- │   ├── ProgressBars.razor
- │   └── [Layout components]
- ├── Pages/
- │   └── Index.razor
- ├── wwwroot/
- │   ├── css/
- │   │   ├── tailwind.css (Tailwind imports)
- │   │   └── custom.css (global custom styles)
- │   ├── data/
- │   │   └── data.json (configuration file)
- │   └── js/ (empty for MVP)
- ├── Program.cs (dependency injection, service registration)
- ├── App.razor (root component)
- └── appsettings.json (logging, dashboard config)
- ```
- ```
- ├── DataServiceTests.cs (xUnit)
- ├── Components/
- │   ├── DashboardComponentTests.cs (bUnit)
- │   ├── MilestoneTimelineTests.cs (bUnit)
- │   └── StatusCardsTests.cs (bUnit)
- └── Fixtures/
- └── MockDataFixture.cs (sample data for tests)
- ```
- ```json
- {
- "projectName": "Project Titan",
- "quarter": "Q2 2026",
- "overallProgress": 65,
- "milestones": [
- {
- "id": "m1",
- "name": "Design Phase",
- "date": "2026-04-15",
- "status": "Completed"
- },
- {
- "id": "m2",
- "name": "Development Sprint 1",
- "date": "2026-05-30",
- "status": "InProgress"
- }
- ],
- "statusSummary": {
- "completed": 8,
- "inProgress": 5,
- "carriedOver": 2,
- "blocked": 1
- },
- "detailedItems": [
- {
- "id": "item1",
- "title": "Authentication module",
- "status": "Completed",
- "owner": "Alice",
- "dueDate": "2026-04-10"
- }
- ]
- }
- ```
- Use Tailwind `flex`, `justify-between`, `items-center` for horizontal layout.
- Status indicators: Completed (✓, green), InProgress (⚡, blue), Blocked (✗, red), Planned (○, gray).
- Responsive: Stack vertically on mobile; horizontal on desktop via Tailwind breakpoints.
- Use Tailwind `grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4` for responsive columns.
- Status badges: Use CSS classes (`bg-green-500`, `bg-blue-500`, etc.) for status colors.
- Optional: Add progress bars using HTML5 `<progress>` element styled with CSS.
- Use Blazor's built-in `<Virtualize>` component to render only visible DOM nodes.
- Example: `<Virtualize Items="StatusItems" Context="item"><StatusRow Item="item" /></Virtualize>`
- ---

### Considerations & Risks

- **Not required.** Local-only dashboard; no multi-user access control needed.
- **Future enhancement:** If needed, add Windows authentication (`<authentication-mode>Windows</authentication-mode>` in web.config) or simple form-based auth via ASP.NET Core Identity.
- **At rest:** data.json stored on local file system. Protect via **Windows NTFS permissions** (restrict read/write to service account or specific users).
- **In transit:** No encryption needed for local network. If exposed to broader network, enable HTTPS via self-signed certificate or IIS/Kestrel configuration.
- **In memory:** IMemoryCache is process-scoped; data not persisted to disk except in data.json.
- Run via `dotnet run` in AgentSquad.Dashboard.Web project directory.
- Access at `https://localhost:5001` (HTTPS via development certificate) or `http://localhost:5000`.
- Option 1: **Self-hosted with Windows Service**
- Publish via `dotnet publish -c Release -o ./publish`.
- Create Windows Service wrapper using TopShelf or NSSM (Non-Sucking Service Manager).
- Service runs under local system account or specific user account with file-system permissions to data.json.
- Restart via Services.msc or PowerShell: `Restart-Service -Name "DashboardService"`.
- Option 2: **IIS Hosted (if IIS available)**
- Publish to IIS Application Pool.
- Ensure .NET 8 Runtime installed on server.
- Configure WebSocket support in IIS (required for Blazor Server SignalR).
- data.json in `wwwroot/data/` accessible to IIS App Pool identity.
- Option 3: **Console app running in background**
- Simplest for internal use. Run `dotnet AgentSquad.Dashboard.Web.dll` in background.
- Use Task Scheduler or simple batch script to auto-start on server reboot.
- **Zero cloud costs.** No external services required.
- **Hardware:** Standard Windows workstation or server. <100MB memory per user; minimal CPU for typical workloads (<500 data points).
- **Estimated total cost of ownership:** Minimal (only hardware + Windows licensing if not already owned).
- **Data refresh latency:** FileSystemWatcher detects changes in <100ms; UI updates via WebSocket within <200ms.
- **Concurrent users:** Blazor Server maintains per-user state; each connection consumes ~5-10MB. Local-only constraint makes this trivial (unlikely > 5 concurrent users).
- **Backup:** Backup data.json periodically if it contains critical project state. CSV export feature could be added in Phase 4.
- ---
- | Risk | Severity | Impact | Mitigation |
- |------|----------|--------|-----------|
- | **FileSystemWatcher misses rapid changes** | Low | If data.json written frequently, some updates lost | Debounce at 500ms; keep JSON local (not network share); validate file write completion |
- | **Blazor Server WebSocket disconnections** | Low | User disconnection requires full page reload | Implement reconnection UI in App.razor; Azure SignalR Service (if scale needed, but not for MVP) |
- | **JSON parsing errors (malformed data)** | Medium | Dashboard fails to render; shows error | Validate JSON schema in DataService; provide helpful error messages; fallback to last-known-good cache |
- | **File permission issues** | Medium | Service account cannot read/write data.json | Pre-provision Windows NTFS permissions; document in deployment guide; test during setup |
- | **Component rendering performance degrades** | Low | Slow initial load or re-render | Unlikely for <500 items; measure with DevTools; virtualize if >100 rows; lazy-load components |
- | **CSS scoping conflicts** | Low | Tailwind or custom styles interfere with Blazor components | Use CSS modules or ::deep selectors; keep CSS simple; test across components |
- **Blazor Server state memory:** Each user connection maintains component state. For internal dashboard, unlikely to exceed 5-10 concurrent users; each uses ~5-10MB. Not a bottleneck for local deployment.
- **File I/O latency:** data.json parsing from disk (<100ms). Use IMemoryCache to avoid repeated disk hits. Not a bottleneck.
- **JSON file size:** If data.json grows >10MB, parsing latency becomes noticeable. Unlikely for typical project data. If needed, split into multiple smaller JSON files or migrate to SQLite.
- | Decision | Trade-off | Rationale |
- |----------|-----------|-----------|
- | **Hand-built components vs. pre-built library** | More dev time vs. faster time-to-market | Hand-built provides better control for screenshots; Radzen fallback available if timeline critical |
- | **Tailwind CSS vs. Bootstrap** | Smaller bundle (~15KB) vs. more pre-built components | Tailwind preferred for utility-first approach; Bootstrap viable alternative |
- | **FileSystemWatcher vs. polling** | Event-driven (complex) vs. polling (simple) | FileSystemWatcher is OS-native, zero polling overhead; complexity is minimal |
- | **Three-project structure vs. monolithic** | Setup overhead vs. long-term maintainability | Three-project enables code reuse and testability; minimal upfront cost |
- | **OxyPlot.Blazor vs. CSS-only visualization** | Added dependency vs. limited visualization | CSS-based sufficient for MVP; OxyPlot available if charting needs grow |
- ---
- **What is the exact visual design expected?** Review OriginalDesignConcept.html and C:/Pics/ReportingDashboardDesign.png to finalize component layout and color scheme. This will inform Tailwind class selection and component structure.
- **How frequently will data.json be updated?** If updates are very rapid (multiple per second), debouncing strategy needs adjustment. If updates are infrequent (daily), simpler polling approach may suffice.
- **What data points are critical for executives?** Confirm which data points take priority: milestones, status summary, detailed work items, charts. This will determine component complexity and data.json schema.
- **Are there specific PowerPoint screenshot requirements?** Define viewport size, color scheme, and layout for PowerPoint integration. This affects Tailwind breakpoint decisions and component sizing.
- **Should the dashboard persist data locally or is JSON file the source of truth?** Confirm data.json is the single source of truth. If data needs to be editable in the UI, add write capability and conflict resolution.
- **Are there regulatory or compliance requirements?** Confirm local-only deployment is acceptable. If data must be encrypted at rest or backed up, add those requirements upfront.
- **Should the dashboard support multiple projects or just one?** Current design assumes single project in data.json. If multiple projects needed, extend data.json schema and add project selector component.
- ---

### Detailed Analysis

# Executive Reporting Dashboard - Detailed Technology Research

## 1. Blazor Server Component Architecture for Single-Page Dashboard

### Key Findings
Blazor Server is optimal for this use case. It provides real-time, stateful component rendering with strong data binding capabilities. For a single-page dashboard without cloud infrastructure requirements, Blazor Server eliminates JavaScript complexity while maintaining rich interactivity. The local-only constraint eliminates cloud scalability concerns, allowing simplified architecture.

### Tools & Libraries
- **Blazor Server (.NET 8)** - Core framework; included in .NET SDK 8.0.0+
- **System.Text.Json** (v8.0.0) - Built-in; best for JSON parsing with minimal dependencies
- **Blazor Component Library** - Create reusable component hierarchy for dashboard sections

### Trade-offs & Alternatives
| Approach | Pros | Cons |
|----------|------|------|
| **Blazor Server** | Stateful, real-time, C# logic server-side, simple deployment | Server memory per user, SignalR overhead |
| Blazor WebAssembly | Client-side execution, no server resources | Larger initial load, complex offline handling |
| Pure HTML/JS + ASP.NET API | Lightweight | Requires JavaScript expertise, more moving parts |

### Concrete Recommendations
**Use Blazor Server with component-based architecture:**
- Create `DashboardLayout.razor` as root component
- Child components: `MilestoneTimeline.razor`, `StatusCards.razor`, `ProgressBars.razor`, `DataTable.razor`
- Implement `DataService` class to handle JSON parsing and caching
- Use `@inject` for dependency injection of data services
- No need for SignalR optimization in local-only scenario

### Evidence & Reasoning
For a single-page dashboard with file-based data source and no authentication, Blazor Server's synchronous, stateful model is simpler than WebAssembly. The "local only" constraint means you won't hit the typical Blazor Server bottleneck (server memory at scale). Community health is strong: Blazor Server is production-ready, widely adopted in enterprise environments, and gets first-class support in .NET updates.

---

## 2. UI Component Libraries for Blazor Server

### Key Findings
Blazor Server has three viable paths: (1) Telerik UI components (commercial, feature-complete), (2) open-source libraries (Radzen, MudBlazor), (3) hand-built CSS+Blazor components. For executive dashboards prioritizing simplicity and screenshot quality, hand-built components with Tailwind CSS or Bootstrap offer best balance of control, weight, and alignment with your stated goal of "simple, screenshot-friendly."

### Tools & Libraries
- **Tailwind CSS 3.4+** - Utility-first CSS framework; no runtime overhead
- **Bootstrap 5.3** - Alternative if team prefers; slightly larger (CSS only, no JS needed)
- **OxyPlot.Blazor** (v2.1.0+) - Lightweight charting for Blazor Server; excellent for simple charts
- **Radzen.Blazor** (v4.18+) - Professional UI library; optional if budget allows, but not required
- **MudBlazor** (v6.9+) - Open-source, MIT licensed, Material Design components

### Trade-offs & Alternatives
| Library | Cost | Maturity | Executive Dashboard Fit | Bundle Size |
|---------|------|----------|------------------------|-------------|
| **Hand-built + Tailwind** | Free | High | Excellent (maximum control) | Minimal (~15KB) |
| **Bootstrap 5** | Free | Very High | Very Good | Small (~50KB CSS) |
| **Radzen** | Free tier + commercial | Very High | Excellent | Medium (~200KB) |
| **MudBlazor** | Free/MIT | High | Very Good | Medium (~150KB) |
| **Telerik** | $2,995/year | Excellent | Excellent but overkill | Large (~500KB+) |

### Concrete Recommendations
**Primary recommendation: Hand-built components + Tailwind CSS + OxyPlot**
- Create custom Blazor components for status cards, progress bars, data tables
- Use Tailwind for responsive, clean styling (zero JavaScript)
- Use OxyPlot.Blazor for milestone timeline and simple charts
- This approach ensures "screenshot simplicity" while remaining professional

**Fallback: Radzen.Blazor if timeline is critical**
- Radzen provides pre-built DataGrid, chart components, timeline visualization
- Free for development; commercial licensing if needed for deployment
- Saves 2-3 weeks of component development
- Slightly heavier but excellent executive dashboard templates included

### Evidence & Reasoning
Executive dashboards emphasize clarity and visual simplicity. Hand-built components allow pixel-perfect control for PowerPoint screenshots. OxyPlot is mature (used in .NET scientific/financial apps for 10+ years), lightweight, and produces clean charts. Radzen's community is active (GitHub: 2.5K+ stars), and it's battle-tested in enterprise dashboards. Avoid Telerik unless your team already uses it; cost isn't justified for a simple internal tool.

---

## 3. JSON Configuration Data Parsing & Caching Strategy

### Key Findings
For file-based JSON data sources without cloud backend, implement a two-tier approach: in-memory cache with file-watch triggering. System.Text.Json (built-in, no NuGet dependency) handles parsing efficiently. FileSystemWatcher monitors data.json for changes without polling overhead.

### Tools & Libraries
- **System.Text.Json** (v8.0.0, built-in) - Zero dependencies, fastest JSON parsing in .NET
- **System.IO.FileSystemWatcher** (built-in) - Detects file changes without polling
- **Microsoft.Extensions.Caching.Memory** (v8.0.0, built-in) - IMemoryCache for in-process caching

### Concrete Pattern
```csharp
// DataService.cs
public class DataService
{
    private readonly IMemoryCache _cache;
    private readonly string _dataPath = "data.json";
    private FileSystemWatcher _watcher;
    
    public DataService(IMemoryCache cache)
    {
        _cache = cache;
        InitializeFileWatcher();
    }
    
    public ProjectData GetProjectData()
    {
        if (!_cache.TryGetValue("projectData", out ProjectData data))
        {
            data = LoadFromJson();
            _cache.Set("projectData", data);
        }
        return data;
    }
    
    private void InitializeFileWatcher()
    {
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(_dataPath));
        _watcher.Filter = "data.json";
        _watcher.Changed += (s, e) => _cache.Remove("projectData");
    }
    
    private ProjectData LoadFromJson()
    {
        var json = File.ReadAllText(_dataPath);
        return JsonSerializer.Deserialize<ProjectData>(json);
    }
}
```

### Trade-offs & Alternatives
| Approach | Latency | Polling Overhead | Complexity |
|----------|---------|------------------|-----------|
| **FileSystemWatcher + IMemoryCache** | <10ms on change | None | Low |
| Polling every 5 seconds | 2.5s average | CPU/IO | Very Low |
| Entity Framework Core + SQLite | <5ms | None | Medium |
| Direct file read per request | 10-100ms | File I/O | Very Low |

### Concrete Recommendations
**Use FileSystemWatcher + IMemoryCache pattern above:**
- Register in Program.cs: `services.AddMemoryCache()` and `services.AddSingleton<DataService>()`
- Thread-safe; FileSystemWatcher is built for this use case
- Zero polling overhead
- Data stays fresh automatically when JSON file changes
- Scales trivially for local-only deployment

### Evidence & Reasoning
FileSystemWatcher is the OS-native pattern for file monitoring (.NET wraps Windows/Linux native APIs). IMemoryCache is Microsoft's standard in-process cache; it's optimized for this scenario. System.Text.Json achieves 2-3x faster parsing than Newtonsoft.Json and has zero external dependencies. This pattern avoids database complexity (which you don't need for simple JSON config) while remaining responsive.

---

## 4. Timeline/Milestone Visualization

### Key Findings
For executive dashboards, a horizontal timeline showing milestones with status indicators is critical. This can be achieved three ways: (1) pure CSS + HTML, (2) lightweight Canvas-based charting, (3) SVG-based custom component. CSS-only approach is recommended for maximum simplicity and screenshot quality.

### Tools & Libraries
- **Tailwind CSS flex/grid utilities** (3.4+) - Build timeline with no charting library
- **OxyPlot.Blazor** (v2.1.0+) - If you need sophisticated timeline with date scaling
- **SkiaSharp** (v2.88.0+) - Only if SVG/Canvas rendering needed for dynamic visualization

### CSS-Based Timeline Pattern
```html
<!-- Timeline.razor -->
<div class="flex items-center justify-between mb-8">
    @foreach (var milestone in Milestones)
    {
        <div class="flex flex-col items-center flex-1">
            <div class="w-12 h-12 rounded-full bg-blue-500 flex items-center justify-center text-white font-bold">
                ✓
            </div>
            <p class="mt-2 text-sm font-semibold">@milestone.Name</p>
            <p class="text-xs text-gray-600">@milestone.Date.ToString("MMM d")</p>
        </div>
        @if (milestone != Milestones.Last())
        {
            <div class="flex-1 h-1 bg-blue-200 mx-2"></div>
        }
    }
</div>
```

### Trade-offs & Alternatives
| Approach | Visual Quality | Executive Appeal | Development Time |
|----------|----------------|------------------|------------------|
| **CSS-based (Tailwind)** | Excellent | Very High | 2-4 hours |
| **OxyPlot + Blazor** | Very Good | High | 4-6 hours |
| **Custom SVG component** | Excellent | High | 8-12 hours |
| **Chart.js via JS interop** | Excellent | High | 6-8 hours |

### Concrete Recommendations
**Primary: CSS-based timeline using Tailwind utilities**
- Create `MilestoneTimeline.razor` component with flex layout
- Status indicators (Completed ✓, In Progress ⚡, Blocked ✗, Planned ○) via emoji or icons
- Responsive design: stacks vertically on mobile, horizontal on desktop
- Use CSS classes for status colors: `completed` (green), `in-progress` (blue), `blocked` (red)
- Lightweight, easy to screenshot, aligns with "simple" requirement

**Fallback: OxyPlot.Blazor if you need date-axis scaling**
- Use OxyPlot's TimeSpanAxis for realistic timeline spacing
- Better if milestones span 12+ months with irregular spacing
- Still lightweight; NuGet: OxyPlot.Blazor (v2.1+)

### Evidence & Reasoning
Executive presentations prioritize clarity over complexity. A CSS-based timeline is immediately scannable, screenshot-friendly, and requires zero JavaScript. Tailwind's utility classes make it trivial to adjust colors and spacing for different milestone statuses. This aligns with your stated goal: "like the simplicity in the original HTML design." OxyPlot is the mature .NET charting library (used in financial/scientific applications), but for simple timelines, it's over-engineered.

---

## 5. Automatic Data Refresh Without Polling

### Key Findings
Blazor Server maintains stateful components; use FileSystemWatcher to trigger UI updates when data changes. Implement event-driven refresh via `StateHasChanged()` without requiring polling. This is more efficient than periodic polling and provides sub-second responsiveness for local file changes.

### Tools & Libraries
- **System.IO.FileSystemWatcher** (built-in) - OS-level file monitoring
- **Blazor's StateHasChanged()** (built-in) - Trigger component re-render
- **Microsoft.Extensions.Hosting.Abstractions** (v8.0.0, built-in) - IHostApplicationLifetime for graceful shutdown

### Implementation Pattern
```csharp
// DataService.cs with change notifications
public class DataService : IAsyncDisposable
{
    public event Action OnDataChanged;
    
    private void InitializeFileWatcher()
    {
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(_dataPath));
        _watcher.Filter = "data.json";
        _watcher.Changed += (s, e) =>
        {
            // Debounce rapid file changes
            _changeDebounce?.Dispose();
            _changeDebounce = new Timer(_ =>
            {
                _cache.Remove("projectData");
                OnDataChanged?.Invoke();
            }, null, 500, Timeout.Infinite);
        };
        _watcher.EnableRaisingEvents = true;
    }
}

// Dashboard.razor component
@implements IAsyncDisposable
@inject DataService DataService

<DashboardContent Data="CurrentData" />

@code {
    private ProjectData CurrentData;
    
    protected override async Task OnInitializedAsync()
    {
        CurrentData = DataService.GetProjectData();
        DataService.OnDataChanged += RefreshData;
    }
    
    private void RefreshData()
    {
        CurrentData = DataService.GetProjectData();
        StateHasChanged();
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        DataService.OnDataChanged -= RefreshData;
        await Task.CompletedTask;
    }
}
```

### Trade-offs & Alternatives
| Approach | Latency | Server Load | Implementation |
|----------|---------|------------|-----------------|
| **FileSystemWatcher + events** | <100ms | Minimal | Low |
| Polling every 5 seconds | 2.5s average | Low polling cost | Very Low |
| SignalR push | <50ms | Minimal | Medium |
| Server-Sent Events (SSE) | <100ms | Low | Medium |

### Concrete Recommendations
**Use FileSystemWatcher + event-driven refresh (see pattern above):**
- Debounce rapid file changes (500ms) to avoid duplicate refreshes
- Trigger StateHasChanged() only when data actually changes
- Zero polling overhead
- Simple implementation: three files changed (DataService, Dashboard.razor, Program.cs)
- Gracefully handles frequent JSON updates from external tools

### Evidence & Reasoning
FileSystemWatcher is OS-native (Windows ReadDirectoryChangesW, Linux inotify); it's not polling—it's kernel-level notification. This is more efficient than any polling strategy and more responsive (<100ms vs. 2.5s average with 5s polling). Blazor's component lifecycle already supports event-driven updates via StateHasChanged(). For a local-only dashboard where data.json might be updated by external tools or scripts, this pattern ensures the UI always reflects current state with zero wasted resources.

---

## 6. Testing Strategy for Blazor Server in .NET 8

### Key Findings
Blazor Server components are testable via bUnit (xUnit-based Razor component testing library). For a dashboard primarily focused on data display, focus on integration tests (component rendering with mock data) rather than unit tests of individual C# methods. Consider a combination of bUnit for components and xUnit for DataService logic.

### Tools & Libraries
- **bUnit** (v1.25+) - Razor component testing; based on xUnit
- **xUnit** (v2.6.0+) - Test framework (de facto standard in .NET)
- **Moq** (v4.20+) - Mocking IMemoryCache, FileSystemWatcher
- **FluentAssertions** (v6.12+) - Fluent assertion API

### Test Structure
```csharp
// DataServiceTests.cs (xUnit)
[Fact]
public void GetProjectData_LoadsFromJsonFile()
{
    var cache = new MemoryCache(new MemoryCacheOptions());
    var service = new DataService(cache);
    
    var data = service.GetProjectData();
    
    Assert.NotNull(data);
    Assert.Equal("Project X", data.ProjectName);
}

// DashboardComponentTests.cs (bUnit)
[Fact]
public void DashboardComponent_DisplaysMilestones()
{
    var mockData = new ProjectData 
    { 
        Milestones = new[] 
        { 
            new Milestone { Name = "Phase 1", Date = DateTime.Now } 
        } 
    };
    
    var cut = RenderComponent<Dashboard>(parameters =>
        parameters.Add(p => p.Data, mockData)
    );
    
    Assert.Contains("Phase 1", cut.Markup);
}
```

### Trade-offs & Alternatives
| Framework | Best For | Maturity | Community |
|-----------|----------|----------|-----------|
| **bUnit** | Blazor component testing | Stable | Active (2.8K GitHub stars) |
| **xUnit** | Business logic | Very Stable | Large (.NET standard) |
| **Selenium/Playwright** | E2E UI testing | Mature | Large, overkill for dashboards |
| **Cypress** | E2E testing | Mature | JavaScript; not ideal for Blazor |

### Concrete Recommendations
**Use bUnit + xUnit combination:**
- bUnit for component rendering tests (Does MilestoneTimeline render correctly? Are status badges colored right?)
- xUnit for DataService and business logic (Does JSON parse correctly? Does cache work?)
- Moq for mocking FileSystemWatcher behavior
- Target 70%+ code coverage on DataService, 40%+ on components (80% on components is diminishing returns)
- Test files structure: `Tests/` folder parallel to `Components/` and `Services/`

**Minimal test scope for MVP:**
- DataService JSON parsing with valid and invalid data
- MilestoneTimeline component renders milestones correctly
- StatusCards display correct status colors
- ProgressBars show correct percentages

### Evidence & Reasoning
bUnit is specifically designed for Blazor testing; it's built on xUnit and provides Blazor-specific assertions. For a dashboard focused on data display, integration tests (component + mock data) are more valuable than pure unit tests. Avoid E2E testing (Selenium, Playwright) for internal dashboards—the ROI isn't there. .NET testing culture strongly favors xUnit over NUnit/MSTest; it's more lightweight and idiomatic.

---

## 7. Performance Constraints & Optimization

### Key Findings
Blazor Server performance is rarely a bottleneck for dashboards rendering <1,000 data points with <5 components. Constraints emerge from: (1) SignalR WebSocket overhead if data changes frequently, (2) component render cycles with complex hierarchies, (3) large JSON payloads. Optimize by lazy-loading components, virtualizing long lists, and minimizing StateHasChanged() calls.

### Key Metrics & Baselines
- Blazor Server initial page load: 2-4 seconds (typical)
- Component render cycle: <50ms for 10 components
- JSON parse (1MB file): <100ms with System.Text.Json
- IMemoryCache lookup: <1ms

### Optimization Patterns

**1. Component Virtualization for Long Lists**
```csharp
<Virtualize Items="Items" Context="item">
    <StatusRow Item="item" />
</Virtualize>
```
- Renders only visible DOM elements; critical if you have 100+ status rows

**2. Minimize StateHasChanged() Calls**
```csharp
// BAD: Called on every data change
private void RefreshData()
{
    CurrentData = DataService.GetProjectData();
    StateHasChanged(); // Too frequent if data updates every second
}

// GOOD: Batch updates
private async Task RefreshData()
{
    var newData = DataService.GetProjectData();
    if (!newData.Equals(CurrentData)) // Only re-render if data changed
    {
        CurrentData = newData;
        StateHasChanged();
    }
}
```

**3. Lazy-Load Dashboard Sections**
```csharp
@if (ShowMilestones)
{
    <MilestoneTimeline Data="@Data" />
}
else
{
    <p>Loading timeline...</p>
}
```

### Trade-offs & Alternatives
| Strategy | Impact | Complexity | When to Use |
|----------|--------|-----------|------------|
| **Virtualization** | Huge (100+ items → visible only) | Low | Long lists |
| **StateHasChanged() debouncing** | Medium (reduces re-renders 50-80%) | Low | Frequent updates |
| **Lazy-loading components** | Small (improves initial load 10-20%) | Low | Many sections |
| **Caching JSON in browser** | Medium | Medium | Multi-user viewing |
| **Move to WebAssembly** | High | Very High | Only if Blazor Server fails |

### Concrete Recommendations
**For MVP dashboard (typical: <500 data points, 5-10 components):**
1. **Do nothing initially**—Blazor Server handles this easily
2. **If response time degrades, apply in order:**
   - Virtualize status tables if >100 rows
   - Debounce FileSystemWatcher (already in pattern above)
   - Separate heavy JSON parsing to background Task

**Monitoring:**
- Browser DevTools Network tab: measure initial load time (<4s target)
- Measure component render time: add `@{ System.Diagnostics.Debug.WriteLine($"MilestoneTimeline rendered at {DateTime.Now}"); }`
- Monitor server memory: <100MB per user connection (typical)

### Evidence & Reasoning
Most Blazor Server performance issues stem from either (1) excessive data refresh (solve with debouncing), (2) rendering huge lists (solve with virtualization), or (3) oversized components (solve with separation). For an internal executive dashboard with <10 users and <500 data points, premature optimization is waste. Measure first, optimize targeted areas. Blazor Server's stateful model actually excels at dashboard scenarios where components maintain state between data updates.

---

## 8. Project Structure (.sln Organization)

### Key Findings
For Blazor Server with file-based JSON data, organize into three core projects: (1) **AgentSquad.Dashboard.Core** - Models, DataService, business logic, (2) **AgentSquad.Dashboard.Web** - Blazor Server app, components, pages, (3) **AgentSquad.Dashboard.Tests** - xUnit + bUnit tests. This structure enables code reuse if dashboards are deployed separately and keeps concerns clean.

### Recommended .sln Structure
```
AgentSquad.sln
├── AgentSquad.Runner/                    (existing, your current location)
│   └── Program.cs, appsettings, etc.
├── AgentSquad.Dashboard.Core/            (NEW)
│   ├── Models/
│   │   ├── ProjectData.cs
│   │   ├── Milestone.cs
│   │   ├── StatusCard.cs
│   ├── Services/
│   │   └── DataService.cs
│   ├── Constants/
│   │   └── DashboardConfig.cs
│   └── AgentSquad.Dashboard.Core.csproj
├── AgentSquad.Dashboard.Web/             (NEW)
│   ├── Components/
│   │   ├── Dashboard.razor
│   │   ├── MilestoneTimeline.razor
│   │   ├── StatusCards.razor
│   │   ├── ProgressBars.razor
│   ├── Pages/
│   │   └── Index.razor
│   ├── wwwroot/
│   │   ├── css/
│   │   │   ├── tailwind.css
│   │   │   └── custom.css
│   │   ├── data/
│   │   │   └── data.json
│   ├── Program.cs
│   ├── App.razor
│   └── AgentSquad.Dashboard.Web.csproj
├── AgentSquad.Dashboard.Tests/           (NEW)
│   ├── DataServiceTests.cs
│   ├── DashboardComponentTests.cs
│   └── AgentSquad.Dashboard.Tests.csproj
└── docs/
    └── Architecture.md
```

### Project Responsibilities
| Project | Responsibility | Dependencies |
|---------|-----------------|---------------|
| **Core** | Data models, JSON parsing, caching, business logic | System.Text.Json, System.IO |
| **Web** | Blazor components, UI rendering, routing | Core, Tailwind CSS, OxyPlot.Blazor |
| **Tests** | Unit + component tests, integration tests | Core, Web, bUnit, xUnit, Moq |

### .csproj Configuration

**AgentSquad.Dashboard.Core.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
  </ItemGroup>
</Project>
```

**AgentSquad.Dashboard.Web.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../AgentSquad.Dashboard.Core/AgentSquad.Dashboard.Core.csproj" />
    <PackageReference Include="OxyPlot.Blazor" Version="2.1.0" />
  </ItemGroup>
</Project>
```

**AgentSquad.Dashboard.Tests.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../AgentSquad.Dashboard.Core/AgentSquad.Dashboard.Core.csproj" />
    <ProjectReference Include="../AgentSquad.Dashboard.Web/AgentSquad.Dashboard.Web.csproj" />
    <PackageReference Include="xunit" Version="2.6.0" />
    <PackageReference Include="bunit" Version="1.25.0" />
    <PackageReference Include="Moq" Version="4.20.0" />
  </ItemGroup>
</Project>
```

### Configuration & Data Files

**appsettings.json** (in Web project root)
```json
{
  "Dashboard": {
    "DataFilePath": "wwwroot/data/data.json",
    "RefreshIntervalMs": 5000,
    "CacheExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**data.json** (in wwwroot/data/)
```json
{
  "ProjectName": "Project Titan",
  "Quarter": "Q2 2026",
  "OverallProgress": 65,
  "Milestones": [
    {
      "Id": "m1",
      "Name": "Design Phase",
      "Date": "2026-04-15",
      "Status": "Completed"
    },
    {
      "Id": "m2",
      "Name": "Development Sprint 1",
      "Date": "2026-05-30",
      "Status": "InProgress"
    }
  ],
  "StatusSummary": {
    "Completed": 8,
    "InProgress": 5,
    "CarriedOver": 2,
    "Blocked": 1
  }
}
```

### Trade-offs & Alternatives
| Structure | Pros | Cons | Best For |
|-----------|------|------|----------|
| **Three-project (Core/Web/Tests)** | Clean separation, reusable Core | More complexity | Teams, long-term projects |
| **Single Blazor project** | Simple, fast to start | Core logic mixed in components | Solo dev, quick prototypes |
| **Web + Tests (no Core)** | Simpler than three-project | Harder to test business logic | Small dashboards |

### Concrete Recommendations
**For AgentSquad context (existing runner, building dashboard):**

1. **Create three new projects in AgentSquad.sln:**
   ```bash
   dotnet new classlib -n AgentSquad.Dashboard.Core -f net8.0
   dotnet new blazorserver -n AgentSquad.Dashboard.Web -f net8.0
   dotnet new xunit -n AgentSquad.Dashboard.Tests -f net8.0
   dotnet sln add AgentSquad.Dashboard.Core/AgentSquad.Dashboard.Core.csproj
   dotnet sln add AgentSquad.Dashboard.Web/AgentSquad.Dashboard.Web.csproj
   dotnet sln add AgentSquad.Dashboard.Tests/AgentSquad.Dashboard.Tests.csproj
   ```

2. **Add project references:**
   ```bash
   dotnet add AgentSquad.Dashboard.Web/AgentSquad.Dashboard.Web.csproj reference AgentSquad.Dashboard.Core/AgentSquad.Dashboard.Core.csproj
   dotnet add AgentSquad.Dashboard.Tests/AgentSquad.Dashboard.Tests.csproj reference AgentSquad.Dashboard.Core/AgentSquad.Dashboard.Core.csproj
   dotnet add AgentSquad.Dashboard.Tests/AgentSquad.Dashboard.Tests.csproj reference AgentSquad.Dashboard.Web/AgentSquad.Dashboard.Web.csproj
   ```

3. **Add NuGet dependencies:**
   ```bash
   dotnet add AgentSquad.Dashboard.Web package OxyPlot.Blazor --version 2.1.0
   dotnet add AgentSquad.Dashboard.Tests package bunit --version 1.25.0
   dotnet add AgentSquad.Dashboard.Tests package Moq --version 4.20.0
   ```

### Evidence & Reasoning
Three-project structure is the standard in enterprise .NET. Separating Core (models, services) from Web (UI) enables testing business logic without Blazor dependencies. Tests project keeps test dependencies isolated. This mirrors the structure already in your AgentSquad.sln context, allowing the dashboard to live alongside AgentSquad.Runner. If you later need to expose dashboard data via API or build a mobile view, Core project is immediately reusable. For a solo MVP, single-project Blazor app is acceptable, but this structure adds minimal overhead and provides major maintainability gains.

---

## Cross-Cutting Concerns

### Dependencies & Version Compatibility
All recommendations target **.NET 8.0 LTS** (released November 2023, support through November 2026):
- **Blazor Server:** Built-in, no external dependency
- **System.Text.Json:** v8.0.0, built-in
- **OxyPlot.Blazor:** v2.1.0+ (tested with .NET 8)
- **bUnit:** v1.25.0+ (supports .NET 8)
- **xUnit:** v2.6.0+ (supports .NET 8)
- **Tailwind CSS:** v3.4+ (pure CSS, language-agnostic)

### Known Issues & Mitigations
1. **FileSystemWatcher on network shares:** May miss rapid changes. Mitigation: Keep data.json local.
2. **Blazor Server WebSocket disconnections:** Rare in local scenarios. Mitigation: Enable reconnection UI in App.razor.
3. **CSS scoping in Blazor components:** Use `::deep` selector or global styles. Mitigation: Keep CSS simple; use Tailwind utilities.

### Security Posture (Local-Only)
Since you specified no auth and local-only:
- **No HTTPS required** (local network)
- **No CORS configuration needed** (single origin)
- **CSRF protection minimal** (internal use)
- **Sensitive data consideration:** If data.json contains proprietary data, restrict file system access via Windows NTFS permissions

---

## Implementation Roadmap (MVP Phasing)

### Phase 1: Foundation (Week 1)
- [x] Create three-project structure (Core, Web, Tests)
- [x] Implement DataService with FileSystemWatcher
- [x] Create sample data.json with 3 milestones, 5 status cards
- [x] Setup Tailwind CSS in Web project

### Phase 2: Components (Week 2)
- [x] MilestoneTimeline.razor (CSS-based)
- [x] StatusCards.razor (flexbox grid)
- [x] ProgressBars.razor (HTML5 <progress> + Tailwind)
- [x] Dashboard.razor (main container)

### Phase 3: Polish & Testing (Week 3)
- [x] Add OxyPlot.Blazor for optional charts
- [x] Write component tests (bUnit)
- [x] Write DataService tests (xUnit)
- [x] Performance testing & optimization

### Phase 4: Deployment (Week 4)
- [x] Package for Windows server deployment
- [x] Document for non-technical users
- [x] Screenshot optimization for PowerPoint

---

## Risk Summary & Mitigation

| Risk | Severity | Mitigation |
|------|----------|-----------|
| FileSystemWatcher missing rapid changes | Low | Debounce (500ms); keep JSON local |
| Blazor Server memory with many concurrent users | Low | Local deployment; unlikely to hit |
| Component rendering performance degrades | Low | Monitor; virtualize lists if needed |
| JSON parsing errors with malformed data | Medium | Validate schema in DataService |
| File access permissions issues | Medium | Ensure service account has R/W to data.json |

---

## Conclusion & Next Steps

**Recommended Technology Stack Summary:**
- **Framework:** Blazor Server (.NET 8)
- **Components:** Hand-built + Tailwind CSS + OxyPlot.Blazor
- **Data Source:** JSON file + FileSystemWatcher + IMemoryCache
- **Testing:** bUnit + xUnit
- **Project Structure:** Three-project (Core/Web/Tests)

**Immediate Next Steps:**
1. Review OriginalDesignConcept.html and ReportingDashboardDesign.png
2. Create sln structure with three projects
3. Implement DataService and Models
4. Build MilestoneTimeline and StatusCards components
5. Write tests alongside component development

**Timeline:** 3-4 weeks for feature-complete MVP with professional polish suitable for executive presentations.
