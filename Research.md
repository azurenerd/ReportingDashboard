# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-09 20:26 UTC_

### Summary

The Executive Dashboard project requires a lightweight, local-only reporting interface for showcasing project milestones and progress to C-suite stakeholders. Using C# .NET 8 with Blazor Server is optimal for this use case: it eliminates JavaScript complexity, leverages built-in file I/O and JSON parsing, and delivers production-ready components in 2 weeks. The technology stack is intentionally minimal—no external charting libraries, no cloud dependencies, no enterprise overhead—prioritizing simplicity and screenshot-friendly output. Recommended approach: JSON file-based data store, Bootstrap 5.3 for styling, custom SVG components for timeline visualization, and stateless page refresh for data updates.

### Key Findings

- **Simplicity wins for executive dashboards**: Custom charting libraries (Syncfusion, Chart.js) add unnecessary complexity; SVG rendering provides superior control and screenshot compatibility at zero external dependency cost.
- **JSON file-based storage is optimal over databases**: Project refresh cycles (monthly) and single-dashboard scope eliminate relational complexity. File-based approach is simpler to edit, version, and transfer than SQLite or server databases.
- **System.Text.Json (built-in) eliminates external dependency**: Newtonsoft.Json adds no value for dashboard data volumes; native System.Text.Json with source generators matches or exceeds performance.
- **Bootstrap 5.3 provides professional aesthetics immediately**: Executive-grade polish is non-negotiable; custom CSS risks amateurish appearance. Bootstrap zero-configuration approach saves 3-5 days of design work.
- **Blazor Server eliminates JavaScript framework complexity**: No SPA overhead needed; server-side rendering simplifies data binding, state management, and screenshot consistency.
- **File watcher optional for MVP**: Manual page refresh acceptable for monthly update cycles; hot-reload (FileSystemWatcher) deferred to Phase 2 without impacting core functionality.
- **No authentication needed for local-only deployment**: Single-user, internal tool requires no auth layer initially; Windows/SSO auth trivial to add if future intranet deployment occurs.
- **10-day delivery achievable with focused MVP scope**: Day-by-day breakdown validates realistic timeline: prototype (Days 1-2), JSON schema validation (Days 3-4), component builds (Days 5-6), styling/screenshots (Days 7-8), deployment (Days 9-10).
- Load data.json on app startup
- Cache in memory for lifetime of application session
- Manual browser refresh loads fresh data
- Add `FileSystemWatcher` to monitor data.json for changes
- Trigger `IMemoryCache.Remove("project_data")` when file changes
- Next page request reloads data without app restart
- Top section: Project header (name, owner, status, target date)
- Second section: Timeline of milestones (full width)
- Third section: 3-column work state grid (Shipped | In Progress | Carried Over)
- Bottom section: Health metrics (KPI counters)
- ```css
- /* Override Bootstrap defaults for executive polish */
- .dashboard-header {
- background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
- color: white;
- padding: 2rem;
- border-radius: 8px;
- }
- .milestone-bar {
- height: 24px;
- background: #e0e0e0;
- border-radius: 4px;
- position: relative;
- }
- .milestone-bar.completed {
- background: #4caf50;
- }
- .milestone-bar.in-progress {
- background: #2196f3;
- }
- .work-item-card {
- border-left: 4px solid #667eea;
- margin-bottom: 0.5rem;
- }
- ```
- All data modeled as JSON; no migrations, no SQL, no ORM. If future expansion requires 50+ projects or historical tracking, transition to SQLite (trivial migration: deserialize JSON into SQLite schema, use Dapper for queries).
- Implement Windows Authentication (IIS integrated auth) or Azure AD via ASP.NET Core Identity
- Add `[Authorize]` attributes to Dashboard.razor
- No authorization logic needed (everyone sees same data)
- | Phase | Days | Deliverable | Owner |
- |-------|------|-------------|-------|
- | **Design & setup** | 1-2 | Project structure, design assets reviewed, data schema validated | Full team |
- | **Data & models** | 3-4 | data.json with realistic sample data, C# model classes, DashboardDataService | Backend dev |
- | **Component scaffolding** | 5 | Dashboard.razor, TimelineComponent, WorkStateComponent, MetricsComponent (empty) | Frontend dev |
- | **Timeline rendering** | 6 | SVG-based milestone bars with status colors; responsive layout | Frontend dev |
- | **Work state grid** | 7 | 3-column layout (Shipped/In Progress/Carried Over); item cards | Frontend dev |
- | **Styling & polish** | 8 | Bootstrap integration, custom CSS, color scheme, executive-grade appearance | Frontend dev + design |
- | **Testing & screenshots** | 9 | Manual testing, browser screenshot capture, PowerPoint-ready images | QA / Frontend dev |
- | **Deployment & docs** | 10 | Local IIS setup, README.md, deployment guide, JSON schema documentation | DevOps / Backend dev |
- Static HTML mockup from design.png (no Blazor)
- Screenshot for stakeholder sign-off
- Validates visual direction before component building
- Basic Blazor page with hardcoded sample data
- Renders all 3 sections (Timeline, WorkState, Metrics)
- No JSON loading yet; prove interactivity works
- Full dashboard with sample data.json
- Professional appearance ready for PowerPoint
- Screenshot set captured for executive review
- **Timeline Rendering Prototype** (1-2 hours, before Day 6)
- Build simple SVG milestone bars in isolation
- Validate visual readability (milestone names, dates, status colors)
- Test responsiveness; ensure full-width fits executive monitor aspect ratios
- Iterate on spacing, font sizes, color contrast
- **Color Scheme Prototype** (2-4 hours, before Day 8)
- Bootstrap defaults vs. branded colors
- Create 2-3 design variants in Bootstrap
- Screenshot each; present to stakeholder for approval
- Finalize CSS before end of Day 7 to avoid late changes
- **Data Schema Validation** (1-2 hours, before Day 4)
- Write 3-5 sample data.json files for different project scenarios
- Deserialize with System.Text.Json; validate schema matches C# models
- Test edge cases: missing fields, future dates, 0% complete milestones
- Finalize schema before component builds to avoid refactoring
- **Phase 2 (Week 3-4)**: FileSystemWatcher hot-reload; no app restart on data.json change
- **Phase 2 (Week 3-4)**: Optional Bunit component tests if team needs snapshot regression testing
- **Phase 3 (Month 2)**: Multi-project selector; project list dropdown
- **Phase 3 (Month 2)**: Monthly snapshot archive for historical trending
- **Phase 4 (Month 3)**: Real-time alerts via SignalR + email integration (if approved)
- **Phase 4 (Month 3)**: Mobile-responsive design (if iPad/tablet presentation required)
- **Show design files** (OriginalDesignConcept.html, ReportingDashboardDesign.png)
- Get visual approval; identify must-haves vs. nice-to-haves
- **Review data schema** (JSON structure)
- Validate fields; confirm project lifecycle (1 month, 1 quarter, rolling?)
- Identify which metrics are critical (% complete, risk flags, burn rate?)
- **Align on refresh cadence**
- Manual file edit, automated daily, or on-demand trigger?
- Impacts deployment and file watching strategy
- **Confirm deployment environment**
- Local Windows machine, shared intranet server, or isolated for screenshots only?
- Affects hosting and HTTPS decisions
- **Establish acceptance criteria**
- What does "ready for executive presentation" look like?
- Color palette, metric precision, layout preferences?
- **Assign data ownership**
- Who maintains data.json going forward?
- Version control, archival, update process?
- **Backend dev**: 2 days for service layer, models, JSON loading (~8 hours)
- **Frontend dev**: 6 days for components, styling, polish (~24 hours)
- **Full-stack pair**: 4 days for integrated build and testing (~16 hours)
- **Team size**: Recommend 1 backend + 1 frontend + 1 design resource for 10-day timeline
- [ ] Dashboard.razor loads data.json successfully on page load
- [ ] All three sections render without JavaScript errors (Timeline, WorkState, Metrics)
- [ ] Bootstrap styling applied; professional appearance validated by design review
- [ ] Sample data.json provided with realistic project data
- [ ] README.md documents JSON schema and data maintenance process
- [ ] Deployment instructions for local IIS or Kestrel included
- [ ] Screenshots captured at 1920x1080 resolution; PowerPoint-ready
- [ ] Code reviewed; no TODOs or technical debt in MVP commit
- [ ] Git repository clean; no uncommitted changes or secrets in code
- **Week 2 retrospective**: Collect stakeholder feedback on layout, metrics, colors
- **Week 3**: Address feedback; plan Phase 2 hot-reload feature if data maintenance becomes burden
- **Month 2**: Expand to multi-project view if executive demand grows beyond initial scope
- **Month 3+**: Historical trending, alerts, drill-down only if stakeholder demand justifies scope increase
- ---
- ```json
- {
- "project": {
- "name": "Project Skylight",
- "description": "Q2 platform modernization initiative",
- "status": "OnTrack",
- "owner": "VP Engineering",
- "targetDate": "2026-06-30"
- },
- "milestones": [
- {
- "id": "m1",
- "name": "Core API Migration",
- "targetDate": "2026-05-15",
- "completedDate": null,
- "status": "InProgress",
- "percentComplete": 75
- },
- {
- "id": "m2",
- "name": "Database Refactoring",
- "targetDate": "2026-05-30",
- "completedDate": null,
- "status": "NotStarted",
- "percentComplete": 0
- },
- {
- "id": "m3",
- "name": "Performance Optimization",
- "targetDate": "2026-06-15",
- "completedDate": null,
- "status": "NotStarted",
- "percentComplete": 0
- },
- {
- "id": "m4",
- "name": "Launch Production",
- "targetDate": "2026-06-30",
- "completedDate": null,
- "status": "NotStarted",
- "percentComplete": 0
- }
- ],
- "workItems": [
- {
- "id": "w1",
- "title": "REST API v2 endpoints",
- "status": "Shipped",
- "completedDate": "2026-04-05",
- "milestone": "m1"
- },
- {
- "id": "w2",
- "title": "Authentication layer refactor",
- "status": "Shipped",
- "completedDate": "2026-04-10",
- "milestone": "m1"
- },
- {
- "id": "w3",
- "title": "GraphQL gateway",
- "status": "InProgress",
- "completedDate": null,
- "milestone": "m1"
- },
- {
- "id": "w4",
- "title": "Data migration scripts",
- "status": "InProgress",
- "completedDate": null,
- "milestone": "m1"
- },
- {
- "id": "w5",
- "title": "Cache layer implementation",
- "status": "InProgress",
- "completedDate": null,
- "milestone": "m1"
- },
- {
- "id": "w6",
- "title": "Load testing framework",
- "status": "CarriedOver",
- "completedDate": null,
- "milestone": "m1"
- },
- {
- "id": "w7",
- "title": "Documentation updates",
- "status": "CarriedOver",
- "completedDate": null,
- "milestone": "m1"
- }
- ],
- "summary": {
- "shipped": 2,
- "inProgress": 3,
- "carriedOver": 2,
- "totalPlanned": 7
- }
- }
- ```
- ---
- The Executive Dashboard project is a low-risk, high-value MVP achievable in 10 days using C# .NET 8 Blazor Server with zero external charting dependencies. The technology stack is intentionally minimal, leveraging built-in .NET capabilities (System.Text.Json, IMemoryCache, FileSystemWatcher) and Bootstrap for professional styling. Success criteria are clear: deliver a screenshot-friendly, stateless dashboard for executive presentation, with sample data.json and deployment documentation.

### Recommended Tools & Technologies

- | Component | Technology | Version | Rationale |
- |-----------|-----------|---------|-----------|
- | UI Framework | Blazor Server (ASP.NET Core) | 8.0.0+ | Server-side rendering, single codebase, SignalR ready |
- | CSS Framework | Bootstrap | 5.3.0+ | Professional styling, zero-config, responsive grid |
- | Visualization | Custom SVG + C# | N/A (built-in) | No external dependency, screenshot-friendly, full control |
- | Icons (optional) | Bootstrap Icons | 1.11.0+ | Lightweight, Bootstrap-integrated |
- | Component | Technology | Version | Rationale |
- |-----------|-----------|---------|-----------|
- | JSON Parsing | System.Text.Json | 8.0.0+ (built-in) | Native to .NET 8, source generator support, zero external dependency |
- | File I/O | System.IO | 8.0.0+ (built-in) | Read data.json from wwwroot or local path |
- | Data Caching | In-Memory (IMemoryCache) | 8.0.0+ (built-in) | Load on startup, optional FileSystemWatcher for hot-reload |
- | File Monitoring (optional) | System.IO.FileSystemWatcher | 8.0.0+ (built-in) | Phase 2: detect data.json changes without app restart |
- | Component | Technology | Rationale |
- |-----------|-----------|-----------|
- | Configuration | data.json (JSON file) | File-based, easy to edit manually, no schema migrations, simple to version control |
- | Location | wwwroot/data/data.json or App_Data/ | Static asset or protected directory depending on deployment |
- | Component | Technology | Version | Rationale |
- |-----------|-----------|---------|-----------|
- | Unit Testing | xUnit | 2.6.3+ | .NET standard, excellent for service layer tests |
- | Component Testing | Bunit | 1.27.0+ | Specialized for Blazor component testing; optional for MVP |
- | Build | dotnet CLI | 8.0.0+ | Built-in, no additional tools needed |
- | Component | Technology | Rationale |
- |-----------|-----------|-----------|
- | Local Development | IIS Express / Kestrel | Built-in to Visual Studio, F5 debugging with hot reload |
- | Production (local) | IIS or self-hosted Kestrel | Windows/.NET native; reverse proxy optional |
- | Containerization | Not recommended for MVP | Pure .NET approach; Docker (mcr.microsoft.com/dotnet/aspnet:8.0) available if future intranet deployment |
- | HTTPS | Self-signed cert (dev) / valid cert (prod) | Built-in to Blazor Server development certificates |
- | Component | Technology | Rationale |
- |-----------|-----------|-----------|
- | Source Control | Git | Assumed (project context) |
- | CI/CD (future) | GitHub Actions or Azure Pipelines | Deferred to Phase 2; not required for MVP |
- ```
- AgentSquad.sln
- ├── AgentSquad.Runner (Blazor Server project)
- │   ├── Pages/
- │   │   └── Dashboard.razor (main dashboard page)
- │   ├── Components/ (reusable Blazor components)
- │   │   ├── TimelineComponent.razor
- │   │   ├── WorkStateComponent.razor
- │   │   └── MetricsComponent.razor
- │   ├── Services/
- │   │   └── DashboardDataService.cs (loads and caches data.json)
- │   ├── Models/ (C# data models)
- │   │   ├── Project.cs
- │   │   ├── Milestone.cs
- │   │   ├── WorkItem.cs
- │   │   └── ProjectStatus.cs (enum)
- │   ├── wwwroot/
- │   │   ├── css/
- │   │   │   └── dashboard.css (custom overrides to Bootstrap)
- │   │   ├── data/
- │   │   │   └── data.json (project configuration data)
- │   │   ├── lib/ (optional: bootstrap CSS via NuGet or CDN)
- │   │   └── img/ (logos, icons if needed)
- │   ├── Program.cs (DI registration, Blazor Server configuration)
- │   └── AgentSquad.Runner.csproj
- └── AgentSquad.Tests (xUnit + Bunit tests, optional for MVP)
- └── DashboardDataServiceTests.cs
- ```
- Logging (use built-in `ILogger<T>` for now)
- Validation (manual for small dataset; FluentValidation deferred to Phase 2)
- Dependency Injection (use built-in ASP.NET Core DI container)
- ORM (file-based, no database)
- Authentication (not needed for MVP)
- ```
- data.json (file)
- ↓
- DashboardDataService (reads on startup, caches in memory)
- ↓
- Dashboard.razor (injects service, passes data to child components)
- ├── TimelineComponent (renders milestones as SVG)
- ├── WorkStateComponent (renders shipped/in-progress/carried-over sections)
- └── MetricsComponent (renders KPI counters)
- ```
- Injects `DashboardDataService`
- Calls `GetProjectData()` in `OnInitializedAsync()`
- Renders child components, passing `Project` model
- Receives `List<Milestone>` as parameter
- Generates SVG `<svg>` markup in C# (no Chart.js)
- Renders each milestone as horizontal bar with status indicator
- Spans full page width for executive visibility
- Receives `List<WorkItem>` as parameter
- Renders 3-column grid: Shipped | In Progress | Carried Over
- Uses Bootstrap card components for consistent styling
- Shows item count and percentage of total
- Renders key numbers: % complete, items shipped, at-risk count
- Color-coded status (green/yellow/red based on ProjectStatus enum)
- Minimal interactivity; snapshot-focused
- ```csharp
- public class Project
- {
- public string Name { get; set; }
- public string Description { get; set; }
- public ProjectStatus Status { get; set; } // OnTrack, AtRisk, Delayed
- public DateTime TargetDate { get; set; }
- public string Owner { get; set; }
- public List<Milestone> Milestones { get; set; }
- public List<WorkItem> WorkItems { get; set; }
- public ProjectSummary Summary { get; set; }
- }
- public class Milestone
- {
- public string Id { get; set; }
- public string Name { get; set; }
- public DateTime TargetDate { get; set; }
- public DateTime? CompletedDate { get; set; }
- public MilestoneStatus Status { get; set; } // NotStarted, InProgress, Completed, Blocked
- public int PercentComplete { get; set; }
- }
- public class WorkItem
- {
- public string Id { get; set; }
- public string Title { get; set; }
- public WorkItemStatus Status { get; set; } // Shipped, InProgress, CarriedOver
- public DateTime CompletedDate { get; set; }
- public string MilestoneId { get; set; } // links to Milestone
- }
- public class ProjectSummary
- {
- public int Shipped { get; set; }
- public int InProgress { get; set; }
- public int CarriedOver { get; set; }
- public int TotalPlanned { get; set; }
- }
- public enum ProjectStatus { OnTrack, AtRisk, Delayed }
- public enum MilestoneStatus { NotStarted, InProgress, Completed, Blocked }
- public enum WorkItemStatus { Shipped, InProgress, CarriedOver }
- ```
- ```csharp
- public class DashboardDataService
- {
- private readonly ILogger<DashboardDataService> _logger;
- private readonly IMemoryCache _cache;
- private Project _cachedProject;
- public DashboardDataService(ILogger<DashboardDataService> logger, IMemoryCache cache)
- {
- _logger = logger;
- _cache = cache;
- }
- public async Task<Project> GetProjectDataAsync()
- {
- // Return cached data if available
- if (_cache.TryGetValue("project_data", out Project cachedProject))
- return cachedProject;
- // Load data.json from file
- var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot/data/data.json");
- var json = await File.ReadAllTextAsync(jsonPath);
- var project = JsonSerializer.Deserialize<Project>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
- // Cache for lifetime of app (or configurable expiry)
- _cache.Set("project_data", project);
- return project;
- }
- }
- ```
- Register in `Program.cs`:
- ```csharp
- builder.Services.AddScoped<DashboardDataService>();
- builder.Services.AddMemoryCache();
- ```

### Considerations & Risks

- wwwroot/data/data.json accessible to app identity only (IIS app pool user on Windows)
- No encryption needed for local deployment
- Standard NTFS permissions sufficient
- Encrypt data.json at rest using System.Security.Cryptography.DataProtectionScope.CurrentUser
- Manage encryption keys securely (Azure Key Vault if cloud-hosted; local key store if on-premise)
- **Runtime**: IIS Express (built into Visual Studio)
- **Debug**: F5 launches with Blazor hot reload
- **Port**: Default https://localhost:5001
- **Option 1 (Recommended for MVP)**: Self-hosted Kestrel on Windows machine
- Simple: `dotnet run --configuration Release` in PowerShell
- No IIS overhead
- Accessible via http://localhost:5000 or machine IP if on network
- **Option 2 (Enterprise)**: IIS application pool
- Install ASP.NET Core Hosting Bundle (v8.0)
- Create IIS website binding to Kestrel reverse proxy
- Windows service to auto-restart on failure
- **MVP (local)**: $0
- **Future (if intranet)**: $0-50/month for small Windows VM or on-premise hardware
- **If cloud (not recommended, explicitly out of scope)**: ~$15-50/month for Azure App Service B1
- **Local development**: Built-in self-signed cert (generated by `dotnet dev-certs https --trust`)
- **Production on intranet**: Self-signed cert acceptable for internal use
- **If internet-facing**: Use Let's Encrypt (free, via certbot or manual renewal scripts)
- **Firewall**: Open port 5000/5001 only to required network segment (e.g., executive subnet)
- **No public internet exposure required**
- **Data in transit**: HTTPS on local network (self-signed) or HTTP if completely isolated
- | Risk | Severity | Probability | Mitigation |
- |------|----------|-------------|-----------|
- | JSON schema changes break deserialization | Medium | Low | Version schema; add `[JsonPropertyName]` attributes for backward compatibility; document breaking changes |
- | File I/O permission errors in production | Medium | Low | Test with app identity (IIS user); use exception handling in DashboardDataService; log failures |
- | SVG rendering performance with 1000+ milestones | Low | Very low | Generate static SVG string vs. DOM; use `@((MarkupString)svgHtml)` in Blazor |
- | Blazor Server SignalR disconnection on network hiccup | Low | Very low | Not required for MVP; manual page refresh acceptable for monthly data updates |
- | CSS framework bloat affecting page load | Very low | Very low | Bootstrap 5.3 minified ~25KB gzipped; negligible for internal tool |
- | Data.json accidentally committed with sensitive information | Medium | Low | Add data.json to .gitignore; use sample-data.json as template; code review before deployment |
- **100+ milestones**: SVG generation remains O(n) linear; up to ~1000 items render in <200ms (no optimization needed)
- **50+ projects**: Current file-based approach breaks; recommend migrating to SQLite with single-file database
- **Real-time multi-user updates**: Requires SignalR broadcast; Blazor Server supports this natively via `StateHasChanged()` in SignalR hub method
- | Decision | Status | Rationale |
- |----------|--------|-----------|
- | Hot-reload (FileSystemWatcher) | Defer to Phase 2 | Manual refresh acceptable for monthly updates; adds complexity without MVP value |
- | Mobile responsiveness | Defer to Phase 2 | Screenshots only (desktop); can add CSS media queries later if mobile reporting needed |
- | Multi-project view | Defer to Phase 3 | MVP single-project dashboard; project selector straightforward to add |
- | Drill-down/detail views | Defer to Phase 4+ | Executive summary only; operational details out of scope |
- | Historical trending | Defer to Phase 3 | First version: current month only; month-over-month requires data archive |
- | Audit logging | Defer to Phase 2 | Not required for internal-only tool; add if compliance demands arise |
- | Notification/alerts | Defer to Phase 2 | Static snapshot sufficient for MVP; real-time alerts require SignalR + email integration |
- | **DECIDE UPFRONT**: JSON schema structure | Decide now | Validate with stakeholders before component builds; avoid schema thrashing |
- | **DECIDE UPFRONT**: Styling/color scheme | Decide now | Design needs to be approved for executive presentation; delays in Phase 2 if uncertain |
- | **DECIDE UPFRONT**: Data refresh cadence | Decide now | Impacts file watching strategy and deployment approach |
- **Who maintains data.json?** (Developer, Product Manager, Project Office?)
- **Refresh cadence**: Daily, weekly, monthly, or on-demand?
- **Version control**: Commit data.json to Git, or external storage?
- **Data validation**: Should app validate schema, or fail silently on errors?
- **Which metrics matter most** to executives? (% complete, velocity, burn rate, risk score?)
- **Project phases**: Does project tracking span 1 month, 1 quarter, or rolling 90 days?
- **Success criteria**: What defines "on-track" vs. "at-risk"? (Days late, % of milestones missed, budget variance?)
- **Stakeholder distribution**: Email static HTML, or link to live dashboard URL?
- **Color scheme**: Use Bootstrap defaults, or company branding?
- **Milestone visibility**: Show all milestones, or only "big rocks" (top 5-10)?
- **Work item detail**: Show task titles, or just counts?
- **Executive-friendly language**: Use technical terms (backlog, sprint) or business terms (committed, shipped)?
- **Hosting preference**: Local machine, shared network folder, or intranet IIS server?
- **Update mechanism**: Manual file edit trigger, scheduled task, or API integration?
- **Backup/archival**: Store monthly snapshots for historical comparison?
- **Access control**: If multi-user: Which roles see which projects?

### Detailed Analysis

# Executive Dashboard Project - Technical Research

## 1. Domain & Market Research

### Core Domain Concepts
- **Milestone tracking**: Discrete project delivery points with target dates and completion status
- **Work state classification**: Shipped (completed), In Progress (active), Carried Over (delayed/backlog)
- **Executive visibility**: High-level project health without operational detail
- **Timeline visualization**: Gantt-style or sequential milestone representation

### Target Users & Workflows
- C-suite executives (CFO, COO, VP Product) reviewing 5-15 minute status updates
- Monthly or bi-weekly review cycles before board/stakeholder meetings
- Screenshot capture for PowerPoint presentations and email distribution
- No drill-down capability needed—aggregate view only

### Competitor/Reference Products
- JIRA Portfolio dashboards (overcomplicated, enterprise-focused)
- Azure DevOps project dashboards (heavy, requires integration)
- Asana/Monday.com executive reports (cloud-based, overkill for local deployment)
- Custom dashboards in Tableau/Power BI (expensive licensing)
- **Relevant pattern**: Simplicity wins. Microsoft internally uses simple one-page HTML dashboards for exactly this use case.

### Regulatory/Compliance
- None applicable for internal, local-only reporting tool
- Data remains on-premise; no PII/sensitive data handling required at this stage

---

## 2. Technology Stack Evaluation for C# .NET 8 Blazor Server

### Recommended Primary Stack

**Blazor Server (ASP.NET Core 8.0.0+)**
- **Strengths**: Server-side rendering, single .NET codebase, real-time updates via SignalR, no JavaScript framework complexity
- **Maturity**: Stable, production-ready (released Nov 2023)
- **Community**: Strong, well-documented, backed by Microsoft
- **Why for this project**: Dashboard doesn't need SPA interactivity; server-side rendering is simpler and faster to build for a one-page executive report

**System.Text.Json (built-in to .NET 8)**
- **Strengths**: Native, performant, no external dependency, source generator support
- **Alternative rejected**: Newtonsoft.Json v13.0.3 adds unnecessary dependency (though compatible)
- **Recommendation**: Use System.Text.Json exclusively. Performance difference negligible for dashboard data volumes (<1MB JSON typical).

**File-based Data Store**
- **Approach**: Read data.json from wwwroot or local file path
- **Library**: System.IO.File + System.Text.Json
- **Why not SQLite**: Overkill; JSON file is simpler, requires no migrations, easier to edit/version
- **Why not cloud database**: Explicitly out of scope

### CSS & Styling Approaches

**Option 1: Bootstrap 5.3.0 (Recommended)**
- **Strengths**: Professional, zero-configuration look, responsive, minimal CSS customization needed
- **Maturity**: Stable, widely used in .NET projects
- **Why for this project**: Executive dashboards benefit from familiar, polished aesthetics. Bootstrap provides this immediately.
- **Integration**: Add via wwwroot/lib or CDN reference in layout

**Option 2: Tailwind CSS (Rejected)**
- **Why rejected**: Requires build tooling (PostCSS) outside pure .NET workflow; adds complexity for a simple dashboard

**Option 3: Custom CSS**
- **Why rejected**: Screenshot-focused UI benefits from proven design patterns; custom CSS risks amateurish appearance

### Charting & Visualization

**Option 1: SVG + Inline C# Rendering (Recommended)**
- **Approach**: Generate timeline and progress bars directly as SVG markup in Blazor components
- **Strengths**: No external dependency, full control, lightweight, PDF/screenshot friendly
- **Trade-off**: Minimal interactivity (not needed for exec dashboard)
- **Recommendation**: Custom component classes to render SVG based on data model

**Option 2: Chart.js via JavaScript Interop (Considered)**
- **Library**: Chart.js 4.4.0 + BlazorChartjs wrapper
- **Why rejected**: Adds JavaScript complexity; SVG is simpler for non-interactive dashboard
- **Use case**: If real-time drill-down becomes requirement later

**Option 3: Syncfusion Blazor Components (Rejected)**
- **Cost**: $995-$1,995/developer/year licensing
- **Complexity**: Overkill for simple milestone timeline
- **Why rejected**: Project explicitly avoids enterprise overhead

---

## 3. Architecture Patterns & Design

### Recommended Architecture: Component-Driven Service Pattern

```
DashboardPage (Blazor component)
├── TimelineComponent (renders milestone SVG)
├── WorkStateGridComponent (shipped/in-progress/carried-over sections)
└── MetricsComponent (project health indicators)

Services/
├── DashboardDataService (reads data.json, caches in memory)
├── FileWatcherService (optional: monitors data.json for changes)

Models/
├── Project
├── Milestone
├── WorkItem
└── ProjectStatus (enum: OnTrack, AtRisk, Delayed)
```

### Data Storage Strategy
- **Primary**: data.json file in wwwroot/data/
- **In-memory cache**: Load on app startup, refresh on file change
- **Schema**: Flat, no relational complexity (see Section 4 for schema details)

### Scalability & Performance Considerations
- **Current scope**: Single dashboard, <10MB JSON file, renders in <100ms
- **Bottleneck prevention**: Use object pooling for SVG generation if rendering >1000 items; not anticipated
- **Caching strategy**: Load data.json once on app startup; optionally watch file for changes using System.IO.FileSystemWatcher

### API Design
- Not applicable; local file-based system, no HTTP API needed
- If future requirement: Create minimal REST endpoints (GET /api/project/{id}) returning JSON; no CRUD needed initially

---

## 4. Libraries, Frameworks & Dependencies

### Core Dependencies
| Package | Version | Purpose | Notes |
|---------|---------|---------|-------|
| ASP.NET Core Blazor Server | 8.0.0+ | UI framework | Included in .NET 8 SDK |
| System.Text.Json | 8.0.0+ | JSON parsing | Built-in, use source generators for models |
| Bootstrap | 5.3.0+ | CSS framework | Via wwwroot/lib or CDN |
| System.IO.FileSystemWatcher | 8.0.0+ | File monitoring (optional) | Built-in for hot-reload |

### Testing Framework (Optional, Recommended)
- **xUnit 2.6.3+** with Bunit 1.27.0+ for Blazor component testing
- **Why**: Standard in .NET ecosystem, excellent for component snapshot testing
- **Scope for MVP**: Test data loading and JSON parsing; skip component render tests initially

### CI/CD (Out of scope for initial build, but planned)
- **GitHub Actions** (built-in to repo if GitHub-hosted)
- **Alternative**: Azure Pipelines (if already in use)

### No external dependencies recommended for:
- Logging (use built-in ILogger for now)
- Validation (manual for this small dataset)
- DI Container (use built-in ASP.NET Core DI)

### Licensing Notes
- All recommended packages: MIT, Apache 2.0, or equivalent permissive licenses
- No GPL dependencies introduced
- Syncfusion explicitly rejected due to licensing cost

---

## 5. Security & Infrastructure

### Authentication & Authorization
- **Recommendation**: NONE at this stage
- **Reasoning**: Internal-only tool, local deployment, no multi-user access needed
- **Future consideration**: If shared/deployed to intranet, add minimal Windows/SSO auth via [Authorize] attributes

### Data Protection
- **Approach**: No encryption needed for internal-only, non-sensitive data
- **File permissions**: Restrict wwwroot/data/data.json to app identity only (standard IIS/Windows permissions)
- **Future**: If sensitive project data added, consider encrypted JSON with key management

### Hosting & Deployment Strategy
- **Local**: IIS Express (dev), or self-hosted Kestrel behind reverse proxy (production)
- **Containerization**: Not recommended for initial MVP; pure Windows/.NET approach
- **Alternative if needed**: Docker container with official mcr.microsoft.com/dotnet/aspnet:8.0 base image
- **Infrastructure cost**: $0 (local/on-premise). If future Azure hosting: ~$15-50/month for App Service

### HTTPS Consideration
- **For local/intranet**: Use self-signed cert (built into Blazor Server)
- **For internet-facing**: Acquire valid cert (Let's Encrypt free option)

---

## 6. Risks, Trade-offs & Open Questions

### Technical Risks
| Risk | Severity | Mitigation |
|------|----------|-----------|
| Scaling to 100+ milestones/items | Low | Pre-render static HTML snapshot; SVG generation is O(n) but fast up to 10k items |
| JSON schema changes breaking app | Medium | Version schema with migration logic in DashboardDataService; document format |
| File system permissions blocking data.json reads | Low | Use IIS app pool identity; test locally first |
| Blazor Server SignalR connection drops | Low | Not needed for MVP; stateless page refresh acceptable |

### Scalability Bottlenecks
- **None foreseen** for stated use case (single dashboard, refresh monthly)
- **Future bottleneck** if expanding to 50+ projects: Consider database instead of JSON

### Skills Gaps
- **Assumption**: Team knows C#/.NET basics
- **Learning curve**: Blazor component model is ~1-2 days for full-stack devs
- **Mitigation**: Start with simpler component-per-section approach; refactor later if needed

### Decisions Deferred vs. Upfront
| Decision | Defer | Reason |
|----------|-------|--------|
| Real-time updates (SignalR) | ✓ Defer | Not needed; manual refresh OK |
| Multi-user access control | ✓ Defer | Single-user tool initially |
| Mobile responsiveness | ✓ Defer | Screenshots only (desktop); can add CSS media queries later |
| Drill-down/detail views | ✓ Defer | Executive summary only |
| Historical trending | ✓ Defer | First version: current month only |

### Open Questions for Stakeholders
1. **Refresh frequency**: Daily, weekly, or on-demand via file edit?
2. **Data ownership**: Who maintains data.json? (Dev team, PM, other?)
3. **Export format**: Static PNG/PDF screenshots, or interactive HTML files?
4. **Color/branding**: Use company brand colors or Bootstrap defaults?
5. **Metrics**: Which KPIs matter most? (% complete, on-time delivery, burn rate?)

---

## 7. Implementation Recommendations

### MVP Scope (Week 1-2)
1. **Day 1-2**: Create Blazor project, scaffold components (Timeline, WorkState, Metrics)
2. **Day 3-4**: Define JSON schema; create sample data.json
3. **Day 5**: Build Timeline component (SVG milestone rendering)
4. **Day 6**: Build WorkState grid (shipped/in-progress/carried-over sections)
5. **Day 7-8**: Polish styling, test screenshots, iterate on design
6. **Day 9-10**: Code review, documentation, deployment to local IIS

### Quick Wins (Demonstrate value early)
- **Day 2 output**: Static prototype (HTML + inline CSS) from design.png
- **Day 5 output**: Clickable Blazor prototype reading hardcoded data
- **Day 8 output**: Screenshot-ready dashboard with sample data.json

### Areas Requiring Prototyping
- **Timeline rendering approach**: Build quick SVG prototype to validate visual hierarchy and readability
- **Color scheme/responsive layout**: Prototype Bootstrap grid before full component implementation
- **Data schema**: Validate JSON structure with sample data before finalizing service layer

### Phasing Beyond MVP
- **Phase 2** (if approved): Add file watcher for hot-reload; no app restart on data.json change
- **Phase 3** (future): Multi-project view; project selector dropdown
- **Phase 4** (future): Historical trending; month-over-month comparison

### Recommended File Structure
```
AgentSquad.sln
├── AgentSquad.Runner/
│   ├── Pages/
│   │   └── Dashboard.razor (main dashboard page)
│   ├── Components/
│   │   ├── TimelineComponent.razor
│   │   ├── WorkStateComponent.razor
│   │   └── MetricsComponent.razor
│   ├── Services/
│   │   └── DashboardDataService.cs
│   ├── Models/
│   │   ├── Project.cs
│   │   ├── Milestone.cs
│   │   └── WorkItem.cs
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── dashboard.css (custom overrides)
│   │   ├── data/
│   │   │   └── data.json (project data)
│   │   └── lib/
│   │       └── bootstrap/ (CSS framework)
│   └── Program.cs (register DashboardDataService)
└── AgentSquad.Tests/
    └── DashboardDataServiceTests.cs
```

### Development Tools & Workflow
- **IDE**: Visual Studio 2022 (17.8+) or VS Code with C# Dev Kit
- **Local debugging**: F5 launches Blazor Server with hot reload via dotnet watch
- **Browser DevTools**: Chrome F12 for styling; Blazor DevTools extension for component inspection

---

## JSON Schema Specification

### Recommended data.json Structure
```json
{
  "project": {
    "name": "Project Codename",
    "description": "Brief executive summary",
    "status": "OnTrack",
    "owner": "VP Product",
    "targetDate": "2026-06-30"
  },
  "milestones": [
    {
      "id": "m1",
      "name": "Big Rock #1: Core Platform",
      "targetDate": "2026-05-15",
      "completedDate": null,
      "status": "InProgress",
      "percentComplete": 75
    }
  ],
  "workItems": [
    {
      "id": "w1",
      "title": "Feature A",
      "status": "Shipped",
      "completedDate": "2026-04-05",
      "milestone": "m1"
    }
  ],
  "summary": {
    "shipped": 12,
    "inProgress": 5,
    "carriedOver": 2,
    "totalPlanned": 19
  }
}
```

**Rationale**: Flat structure, no nesting complexity; easy to edit manually or generate from scripts; parses instantly with System.Text.Json source generators.

---

## Summary & Next Steps

**Immediate actions**:
1. Review design files (OriginalDesignConcept.html, ReportingDashboardDesign.png) to finalize layout requirements
2. Validate JSON schema with stakeholder (data ownership, refresh cadence)
3. Create initial .sln structure and Blazor project
4. Begin Day 1-2 prototype work (static HTML proof of concept)

**Stack confirmed**: C# .NET 8 Blazor Server, System.Text.Json, Bootstrap 5.3, custom SVG components. No external charting libraries; no enterprise dependencies; ship in 10 days.
