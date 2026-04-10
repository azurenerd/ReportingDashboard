# Architecture

## Overview & Goals

This document describes the system architecture for an executive project dashboard built with C# .NET 8 and Blazor Server. The dashboard is designed as a lightweight, screenshot-optimized tool for executive visibility into project status with minimal operational complexity.

**Core Goals:**
- **Simplicity:** Local-only deployment, no cloud dependencies, no authentication required
- **Clarity:** Pre-computed KPIs displayed with clean Material Design UI optimized for PowerPoint screenshots
- **Speed:** <2-second startup, <100ms refresh time for manual data updates
- **Maintainability:** Layered architecture with testable business logic, 80% unit test coverage
- **Deployability:** Single-folder publish artifact (~50-60MB), 5-minute deployment via file copy + service start
- **Scalability:** Single machine with <10 concurrent users supported; horizontal scaling possible if demand increases

**Non-Functional Requirements:**
- Load time: <2 seconds startup (includes JSON parse + KPI aggregation)
- Availability: Single machine; process-dependent (auto-restart via Windows Service optional)
- Concurrency: <10 simultaneous users (internal tool)
- Security: OS-level file permissions on data.json; no HTTPS required (internal network only)
- Persistence: Manual JSON edits reflected on dashboard after F5 refresh
- Export: Browser screenshots sufficient (no PDF/PowerPoint export required for MVP)

---

## System Components

### 1. DataRepository
**Responsibility:** File I/O abstraction for data.json persistence. Stateless wrapper around file system operations.

**Public Interface:**
```csharp
public interface IDataRepository
{
    Task<ProjectData> LoadAsync();
    Task<ProjectData> ReloadAsync();
    Task SaveAsync(ProjectData data);
    Task<FileInfo> GetFileInfoAsync();
}
```

**Dependencies:**
- System.IO (file operations)
- System.Text.Json (deserialization)

**Data Owned:** None (stateless)

**Responsibilities:**
- Read data.json from disk
- Deserialize JSON to ProjectData POCO
- Handle file not found, JSON syntax errors
- Return raw deserialized data (no aggregation)

---

### 2. ProjectDataService
**Responsibility:** Business logic layer. Aggregates raw project data into KPI-computed view models. Caches computed state for fast access.

**Public Interface:**
```csharp
public interface IProjectDataService
{
    Task InitializeAsync();
    DashboardViewModel GetDashboard();
    Task RefreshAsync();
    ProjectData GetRawProjectData();
}
```

**Dependencies:**
- IDataRepository (data loading)
- System.Linq (LINQ to Objects aggregation)

**Data Owned:**
- `_cachedViewModel: DashboardViewModel` (in-memory cache, updated on refresh)
- `_rawData: ProjectData` (raw deserialized state)

**Key Methods:**
- `InitializeAsync()` - Called on app startup; loads data, aggregates KPIs, caches ViewModel
- `GetDashboard()` - Returns cached ViewModel (synchronous, no I/O)
- `RefreshAsync()` - Reloads from disk, re-aggregates, updates cache; called on user refresh request
- `AggregateToViewModel(ProjectData)` - Calculates all KPIs: completion %, task counts, status rollups
- `DetermineProjectStatus(milestones, tasks)` - Logic: if any milestone overdue by >5 days = "At Risk"; if any blocked task = "Blocked"; else "On Track"
- `CalculateMilestoneProgress(milestone, tasks)` - Computes milestone completion % based on dependent task completion

---

### 3. DashboardPage.razor
**Responsibility:** UI rendering layer. Pure presentation, no business logic. Binds to cached ViewModel and handles user interactions (refresh button).

**Dependencies:**
- IProjectDataService (injected, for GetDashboard())
- MudBlazor components (MudContainer, MudGrid, MudCard, MudButton, etc.)
- StatusCard.razor, TimelineChart.razor components

**Data Owned:** None (stateless; UI state managed by Blazor framework)

**Lifecycle:**
1. OnInitializedAsync() - Calls DataService.InitializeAsync(), renders with cached ViewModel
2. User clicks Refresh button - Calls DataService.RefreshAsync(), StateHasChanged() triggers re-render
3. Component reads from DataService.GetDashboard() on each render

**Layout:**
- MudContainer (responsive width) containing:
  - MudGrid with StatusCard components (Completed, In Progress, Carried Over, Blocked counts)
  - Milestone timeline (via TimelineChart component)
  - Refresh button (triggers manual data reload)

---

### 4. StatusCard.razor
**Responsibility:** Reusable presentation component for displaying metric counts with color coding.

**Parameters:**
```csharp
[Parameter] public string Title { get; set; }
[Parameter] public int Value { get; set; }
[Parameter] public string ColorClass { get; set; }
[Parameter] public string IconName { get; set; }
```

**Dependencies:**
- MudBlazor (MudCard, MudText, MudIcon, MudStack)

**Renders:** MudCard with title, numeric value, color-coded background, optional icon

---

### 5. TimelineChart.razor
**Responsibility:** Renders milestone timeline using custom HTML/CSS horizontal bars (no external charting library).

**Parameters:**
```csharp
[Parameter] public MilestoneDisplayModel[] Milestones { get; set; }
[Parameter] public EventCallback<string> OnMilestoneClick { get; set; }
```

**Dependencies:** MudBlazor (MudStack, MudText)

**Implementation:**
- Custom CSS horizontal bars positioned by `left: {percentComplete}%`
- Displays milestone name, due date, percent complete
- Color-coded: green = completed, blue = in progress, orange = at risk, gray = planned

---

### 6. ProjectData (POCO Models)
**Responsibility:** Raw deserialized JSON representation. No business logic, pure data containers.

**Models:**
```csharp
public class ProjectData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public TaskItem[] Tasks { get; set; }
    public Milestone[] Milestones { get; set; }
    public string Owner { get; set; }
    public int Version { get; set; }
}

public class TaskItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string AssignedTo { get; set; }
    public int EstimatedDays { get; set; }
    public int ActualDays { get; set; }
    public string[] Tags { get; set; }
}

public class Milestone
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? AchievedDate { get; set; }
    public int[] DependentTaskIds { get; set; }
    public MilestoneStatus Status { get; set; }
    public int PercentComplete { get; set; }
}
```

---

### 7. DashboardViewModel (View Model)
**Responsibility:** Pre-computed presentation model with aggregated KPIs. Consumed by Blazor components.

```csharp
public class DashboardViewModel
{
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string Status { get; set; } // "On Track", "At Risk", "Blocked"
    public int CompletionPercentage { get; set; }
    public DateTime LastRefreshed { get; set; }
    public int TotalTaskCount { get; set; }
    public int CompletedCount { get; set; }
    public int InProgressCount { get; set; }
    public int PlannedCount { get; set; }
    public int BlockedCount { get; set; }
    public int CarriedOverCount { get; set; }
    public MilestoneDisplayModel[] Milestones { get; set; }
    public int OnTrackMilestoneCount { get; set; }
    public int AtRiskMilestoneCount { get; set; }
    public int CompletedMilestoneCount { get; set; }
    public int TasksOverdue { get; set; }
    public int MilestonesOverdue { get; set; }
    public double AverageTaskCompletionDays { get; set; }
    public double ScheduleVariance { get; set; }
}

public class MilestoneDisplayModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? AchievedDate { get; set; }
    public int PercentComplete { get; set; }
    public string Status { get; set; }
    public int DaysUntilDue { get; set; }
    public bool IsOverdue { get; set; }
}
```

---

## Component Interactions

### Data Flow: Application Startup

```
1. Blazor app starts
2. DashboardPage.OnInitializedAsync() executes
3. → Calls IProjectDataService.InitializeAsync()
4.   → Calls IDataRepository.LoadAsync()
5.     → Reads Data/data.json from disk
6.     → System.Text.Json deserializes to ProjectData POCO
7.   → Calls ProjectDataService.AggregateToViewModel(projectData)
8.     → Calculates completion %, task counts, status
9.     → Caches DashboardViewModel in-memory
10. ← Returns to DashboardPage
11. DashboardPage.Render() executes
12. → Calls DataService.GetDashboard() (synchronous, cached)
13. → Passes ViewModel to StatusCard, TimelineChart components
14. ← Browser displays dashboard (<2 seconds total)
```

### Data Flow: Manual Refresh

```
1. User clicks "Refresh Data" button
2. DashboardPage.RefreshData() event handler executes
3. → Calls IProjectDataService.RefreshAsync()
4.   → Calls IDataRepository.ReloadAsync()
5.     → Re-reads Data/data.json from disk
6.     → System.Text.Json deserializes to ProjectData
7.   → Calls ProjectDataService.AggregateToViewModel()
8.     → Re-calculates all KPIs
9.     → Updates cached DashboardViewModel
10. ← Returns to DashboardPage
11. Calls StateHasChanged() (Blazor framework triggers re-render)
12. DashboardPage.Render() executes
13. → Reads updated ViewModel from DataService.GetDashboard()
14. → StatusCard, TimelineChart components receive new parameters
15. ← Browser displays updated metrics
```

### Dependency Injection

Configured in Program.cs:
```csharp
services.AddSingleton<IDataRepository, DataRepository>();
services.AddSingleton<IProjectDataService, ProjectDataService>();
services.AddMudBlazor();
```

**Singleton scope justification:** Both services cached for session lifetime; data only reloads on explicit user action (Refresh button). No background polling or auto-refresh.

---

## Data Model

### Storage Strategy

**Primary Data:** `Data/data.json` (relative to application root)
- Single-file JSON loaded into memory at startup
- No schema migrations; version field allows backward compatibility
- Committed to Git for version control and audit trail

**Archival Strategy (For Month-Over-Month Trending):**
- Monthly snapshots: `Data/data-2026-03.json`, `Data/data-2026-04.json`, etc.
- Stored alongside current data.json
- Git-tracked for historical comparison
- Not loaded by default; requires separate load endpoint if trending features added

**File Permissions:**
- Data.json: Read-only for dashboard process; manual edit by PM only
- Location: Published folder or network share
- Access control: OS-level file permissions (Windows NTFS ACLs)

### Sample data.json Structure

```json
{
  "id": "project-alpha",
  "name": "Project Alpha",
  "description": "Q1 2026 Executive Dashboard Initiative",
  "startDate": "2026-01-15T00:00:00Z",
  "endDate": null,
  "status": "Executing",
  "owner": "Jane Smith",
  "version": 1,
  "tasks": [
    {
      "id": 1,
      "name": "Architecture Design",
      "description": "Design system layered architecture",
      "status": "Completed",
      "createdDate": "2026-01-01T00:00:00Z",
      "dueDate": "2026-02-01T00:00:00Z",
      "completedDate": "2026-01-31T00:00:00Z",
      "assignedTo": "John Doe",
      "estimatedDays": 10,
      "actualDays": 9,
      "tags": ["design", "core"]
    }
  ],
  "milestones": [
    {
      "id": "m1",
      "name": "Requirements Approved",
      "description": "All stakeholder sign-off complete",
      "dueDate": "2026-02-01T00:00:00Z",
      "achievedDate": "2026-02-01T00:00:00Z",
      "dependentTaskIds": [1],
      "status": "Completed",
      "percentComplete": 100
    }
  ]
}
```

### Enumerations

**ProjectStatus:** Planning (0), Executing (1), Closed (2)

**TaskStatus:** Planned (0), InProgress (1), Completed (2), Blocked (3), CarriedOver (4)

**MilestoneStatus:** Planned (0), InProgress (1), Completed (2), AtRisk (3)

---

## API Contracts

### Service Interfaces

#### IDataRepository
```csharp
public interface IDataRepository
{
    /// <summary>Loads project data from data.json file.</summary>
    /// <exception cref="FileNotFoundException">data.json not found</exception>
    /// <exception cref="JsonException">Deserialization failed; malformed JSON</exception>
    Task<ProjectData> LoadAsync();

    /// <summary>Reloads project data from disk (for manual refresh).</summary>
    Task<ProjectData> ReloadAsync();

    /// <summary>Saves updated project data to disk (future use).</summary>
    Task SaveAsync(ProjectData data);

    /// <summary>Gets file metadata (size, last modified).</summary>
    Task<FileInfo> GetFileInfoAsync();
}
```

#### IProjectDataService
```csharp
public interface IProjectDataService
{
    /// <summary>Initializes service on app startup; loads and aggregates data.</summary>
    /// <exception cref="DashboardException">Data load or validation error</exception>
    Task InitializeAsync();

    /// <summary>Returns cached dashboard view model (synchronous, no I/O).</summary>
    DashboardViewModel GetDashboard();

    /// <summary>Reloads data from disk and recomputes all KPIs.</summary>
    /// <exception cref="DashboardException">Data reload or aggregation error</exception>
    Task RefreshAsync();

    /// <summary>Returns raw project data (for debugging only).</summary>
    ProjectData GetRawProjectData();
}
```

### Error Handling

**Exception Hierarchy:**
```csharp
public class DashboardException : Exception
{
    public string ErrorCode { get; set; }
    public string UserMessage { get; set; }
}

public class DataLoadException : DashboardException
{
    // Thrown when data.json cannot be read or parsed
}

public class DataValidationException : DashboardException
{
    // Thrown when deserialized data violates business rules
}
```

**Error Boundary in DashboardPage.razor:**
```razor
@if (ErrorState.HasError)
{
    <MudAlert Severity="Severity.Error" ShowCloseIcon="true">
        <strong>Dashboard Error:</strong> @ErrorState.ErrorMessage
        <MudButton Variant="Variant.Text" OnClick="HandleRetry">Retry</MudButton>
    </MudAlert>
}
```

---

## Infrastructure Requirements

### Hosting

**Kestrel (Recommended for MVP)**
- Built-in .NET 8 web server, zero configuration
- Port: 5000 (HTTP), 5001 (HTTPS, optional)
- Startup: `dotnet AgentSquad.Runner.dll` or `AgentSquad.Runner.exe`
- Memory: ~150MB base + 2KB per WebSocket connection
- Max concurrent: ~500 connections (unlikely for internal tool)
- Ideal for: Development, single-machine local deployments

**appsettings.json (Kestrel):**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {"Url": "http://localhost:5000"}
    },
    "Limits": {
      "KeepAliveTimeout": 90,
      "MaxConcurrentConnections": 100,
      "MaxConcurrentUpgradedConnections": 100
    }
  }
}
```

**IIS (Alternative for Enterprise)**
- Requires .NET 8 Hosting Bundle installed on Windows Server 2016+
- App Pool: .NET Core Integrated Pipeline
- Identity: ApplicationPoolIdentity with read-only access to data.json
- Recycling: Default (20-minute idle timeout)
- Advantages: Centralized management, SSL bindings, IIS monitoring
- When to use: >10 concurrent users or centralized server deployment

### Networking

**Local Deployment (Single Machine):**
- No external network access required
- Firewall: Allow inbound on port 5000 from localhost only
- HTTPS: Not required (no sensitive data in transit, internal use)

**Internal Network Deployment:**
- Server listens on 0.0.0.0:5000 (all network interfaces)
- Client navigates to `http://{server-ip}:5000`
- HTTPS: Not required (trusted internal network)
- DNS: Optional (use IP address directly)

**Network Configuration (appsettings.Production.json):**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {"Url": "http://0.0.0.0:5000"}
    }
  }
}
```

### Storage

**Data File Location:** `Data/data.json` (relative to app root)
- Size: <10MB typical
- Permissions: Read-only for dashboard process; manual edit by PM
- Backup: Version-controlled in Git (no separate backup)
- Archival: Copy to `data-{YYYY-MM}.json` monthly

**Published Artifact:**
- Size: ~50-60MB (self-contained, includes .NET 8 runtime)
- Generate: `dotnet publish -c Release --self-contained -o ./publish`
- Contents: Binaries, wwwroot (CSS/JS), data folder, runtime libraries

**Directory Structure (Published):**
```
publish/
├── AgentSquad.Runner.exe
├── AgentSquad.Runner.deps.json
├── AgentSquad.Runner.runtimeconfig.json
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── mudblazor/
├── Data/
│   ├── data.json (PM updates manually)
│   └── data-2026-03.json (archive)
└── (runtime libraries)
```

### Deployment Steps

1. **Publish:** `dotnet publish -c Release --self-contained -o ./publish`
2. **Copy:** Transfer `./publish` folder to target machine
3. **Configure:** Edit `Data/data.json` with project-specific content
4. **Start:** Run `.\AgentSquad.Runner.exe` or `dotnet AgentSquad.Runner.dll`
5. **Verify:** Navigate to `http://localhost:5000`
6. **Test:** Click Refresh button, verify dashboard updates

### Optional: Windows Service Deployment (TopShelf)

**Why:** Auto-restart on crash, no manual process management, startup with OS

**Setup:**
1. Add NuGet: `dotnet add package TopShelf`
2. Modify Program.cs to wrap Kestrel host with TopShelf
3. Install: `.\AgentSquad.Runner.exe install`
4. Start: `net start AgentSquad.Runner` or Services.msc
5. Manage: Services.msc, sc start/stop, or HTTP health check

### Monitoring & Observability

**Health Check Endpoint (Optional):**
```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var response = new
        {
            status = report.Status.ToString(),
            lastRefresh = _projectDataService.GetDashboard().LastRefreshed,
            memoryMB = GC.GetTotalMemory(false) / 1_048_576
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});
```

**Logging (Optional for MVP):**
```csharp
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    // Optionally add: config.AddFile("logs/app.log");
});
```

**Metrics to Monitor:**
- Startup time (target <2 seconds)
- JSON parse duration
- In-memory ViewModel size
- Data file size (warn if >100MB)

---

## Technology Stack Decisions

### Core Framework: .NET 8.0 with Blazor Server

**Rationale:**
- Mandatory per project specification
- Stateful WebSocket connection per user enables instant UI updates on refresh
- Unified C# codebase (server + client logic)
- Kestrel deployment requires zero infrastructure setup
- Out-of-the-box dependency injection, logging, configuration

**Alternatives Considered:**
- Blazor WASM: Would require ~500KB IL download; stateless complexity; offline capability unnecessary
- ASP.NET MVC: Requires additional JavaScript framework; not modern enough for executive UI expectations

---

### UI Framework: MudBlazor 6.10.0

**Rationale:**
- Material Design 3 provides instantly professional appearance credible to executives
- 80+ pre-built components: cards, grids, buttons, alerts (reduces custom CSS)
- MIT licensed, no vendor lock-in
- Screenshot quality: Material Design grids/cards render crisply in print-to-PDF
- Active maintenance: 2-3 releases/month
- Learning curve: 2-3 days vs. 1-2 weeks for competitors
- No commercial licensing costs

**Alternatives Considered:**
- Radzen Blazor: Overkill; includes Gantt/Timeline (unnecessary interactivity); steeper API learning curve
- Telerik Blazor: $2000+/year licensing; unnecessary for local internal tool
- Bootstrap DIY: Requires CSS expertise; screenshot consistency depends on manual styling

---

### Data Persistence: JSON + System.Text.Json

**Rationale:**
- Zero external dependencies (System.Text.Json built into .NET 8)
- Version-controllable; historical snapshots via `data-{YYYY-MM}.json`
- No schema migrations needed
- Acceptable performance for <10MB datasets
- Load-time: ~100-500ms typical (not on critical path; only on F5)
- Simple backup/recovery (commit to Git)

**Alternatives Considered:**
- SQLite: Query capability, but added complexity; 2MB footprint; unnecessary for <10MB data with manual updates
- SQL Server: Enterprise overkill; licensing costs; network dependency
- In-memory only: Loses data on restart; no audit trail

---

### Charting: Custom CSS Timeline + Chart.JS

**Rationale:**
- **Timelines:** Custom HTML/CSS bars (0KB, pixel-perfect screenshots, no interactivity overhead)
- **Progress Charts:** Chart.JS via ChartJS.Blazor (80KB gzipped, good screenshot quality, simple bar charts)
- Executives read screenshots; interactive features unused
- CSS rendering 10x faster than JavaScript charting

**Alternatives Considered:**
- SVG (custom): Pixel-perfect control; 4-hour build time; complex layout calculation
- Plotly.NET: Professional-grade; F#/C# interop adds cognitive load
- Radzen Timeline: Tied to Radzen framework (already rejected)

---

### Testing: xUnit + Moq (Service Layer Only)

**Rationale:**
- xUnit: Microsoft-preferred for .NET 8; lightweight; good async support
- Moq: Industry standard mocking; easy to mock file system
- Focus: 80% coverage of ProjectDataService (KPI aggregation logic)
- Skip: Blazor component tests (MudBlazor tested upstream; low ROI)
- Test Investment: 5-10 days; protects against executive-visible bugs

**Alternatives Considered:**
- bUnit: Component testing; unnecessary for thin presentational wrappers
- NUnit: Equally valid; xUnit preferred by Microsoft
- Selenium/Playwright: E2E testing; overkill for single-page dashboard
- No tests: Risky; KPI bugs go unnoticed in production screenshots

---

### Deployment: Kestrel Standalone (Optional TopShelf)

**Rationale:**
- No infrastructure setup; single-folder publish
- Zero cost; no server lease or cloud bill
- Auto-restart via TopShelf Windows Service (optional, 1-day effort)
- Update process: Copy folder + restart (30-second downtime acceptable)

**Alternatives Considered:**
- IIS: Standard but overkill for <10 users; requires server OS
- Docker: Over-engineers for single-machine deployment
- Azure App Service: Violates "local only" mandate
- GitHub Pages: Loses Blazor interactivity

---

## Security Considerations

### Authentication & Authorization

**Current MVP:** No authentication required. Internal tool with trusted users (PM + executives).

**If Future Hardening Needed:**
- **Windows Authentication:** Leverage OS identity on domain-joined machines
- **ASP.NET Core Identity:** Fallback for non-domain environments (3-5 days effort)
- **LDAP/Active Directory:** Centralized access control if >20 users

### Data Protection

**Data at Rest:**
- data.json stored unencrypted (trusted local environment)
- OS file permissions restrict access to dashboard service account only
  ```powershell
  icacls "C:\publish\Data\data.json" /grant "NT SERVICE\AgentSquad.Runner:(F)" /remove "Users"
  ```
- Git: Commit sample data.json only; .gitignore real production data

**Data in Transit:**
- HTTP (ws://) sufficient for localhost (no HTTPS MVP)
- Internal network: No encryption required (trusted LAN)
- If external access later: Add certificate via IIS or Kestrel TLS

**Data Exposure:**
- data.json never serialized to client (server-side only)
- Browser receives only pre-computed ViewModel (aggregated KPIs)
- WebSocket messages encrypted by OS/browser transport layer

### Input Validation

**JSON Deserialization:**
```csharp
try
{
    var json = await File.ReadAllTextAsync(_dataPath);
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    return JsonSerializer.Deserialize<ProjectData>(json, options)
        ?? throw new DataValidationException("data.json is empty");
}
catch (JsonException ex)
{
    throw new DataValidationException($"JSON parse error: {ex.Message}", ex);
}
```

**Business Logic Validation:**
- ProjectData.Name: Required, non-empty
- Tasks: At least 1 task required
- Milestones: At least 1 milestone required
- Task statuses: Must match enum (Planned, InProgress, Completed, Blocked, CarriedOver)
- Dates: StartDate < EndDate (if EndDate provided)

### Risk Mitigation

| Risk | Mitigation |
|------|-----------|
| Unauthorized data.json access | OS file permissions + audit logging |
| XSS via JSON | Input validation + HTML-escape rendering (MudBlazor default) |
| DoS from large JSON | File size warn threshold (>100MB); deserialize timeout (5s) |
| Concurrent edits | Document PM workflow (edit locally, test, deploy); file lock detection |

---

## Scaling Strategy

### Current Bottlenecks

| Bottleneck | Limit | Trigger | Mitigation |
|-----------|-------|---------|-----------|
| JSON File Size | 10MB | >500 projects | Archive to monthly snapshots; SQLite if trending required |
| In-Memory Cache | ~100MB | >500 projects in memory | Monitor GC.GetTotalMemory(); lazy-load per project |
| WebSocket Connections | ~500 concurrent | >10 users | IIS connection pooling; Blazor WASM fallback |
| Startup Time | <2s target | Large JSON | SQLite indexed queries; lazy-load dashboard sections |

### Vertical Scaling (Single Machine)

**If current machine saturates:**
1. Increase available RAM (up to host limits)
2. Upgrade CPU (minor benefit; JSON deserialization not CPU-bound)
3. Move to SSD if disk I/O is bottleneck (unlikely for small JSON)

**No code changes required.** Singleton service caches ViewModel; scaling up is transparent.

### Horizontal Scaling (If >20 Users)

**Not recommended for MVP.** Requires 2-3 weeks engineering:
- Load balancer (HAProxy, Azure LB)
- Shared data.json via SMB network share (adds latency)
- Session affinity (Blazor Server WebSocket must stay on single instance)

**Only justified if:**
- >20 concurrent users confirmed
- Executives across multiple offices need simultaneous access
- Single-machine deployment infeasible

### Caching Strategy

**Current:** Singleton service with in-memory ViewModel cache.

**Enhancement (Future):** Implement sliding expiration cache:
```csharp
private DateTime _cacheExpiry = DateTime.MinValue;

public DashboardViewModel GetDashboard()
{
    if (DateTime.UtcNow > _cacheExpiry)
    {
        _logger.LogWarning("Cache expired; auto-refreshing");
        RefreshAsync().Wait();
    }
    return _cache;
}
```

---

## Risks & Mitigations

### Technical Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| **data.json Corruption** | Low | Critical: Dashboard offline | Version-control backups; retry logic with exponential backoff; PM workflow docs (edit locally, test, deploy) |
| **JSON Deserialization Exception** | Medium | Major: Dashboard fails to load | Comprehensive validation unit tests; error boundary page; fallback to cached ViewModel if available |
| **Slow Startup Time** | Low | Minor: Delayed screenshot | Monitor startup metrics; add progress indicator; async component initialization; pre-compute on build |
| **WebSocket Disconnect** | Low | Major: Loss of interactivity | Auto-reconnect on disconnect; "Reconnecting..." banner; queue refresh requests |
| **Data.json File Lock** | Low | Major: Refresh hangs | Detect file lock; retry with exponential backoff (100ms, 200ms, 400ms); user notification |

### Dependency Risks

| Dependency | Version | Risk | Mitigation |
|------------|---------|------|-----------|
| **MudBlazor** | 6.10.0 | Breaking changes in 7.0 | Pin version in .csproj; test major upgrades in staging; maintain upgrade roadmap |
| **.NET 8.0** | LTS (support until Nov 2026) | EOL; Win OS incompatibility | Document minimum OS (Windows 10 21H2+); test self-contained publish on target |
| **System.Text.Json** | Built-in | Performance regression in updates | Benchmark deserialization; switch to Newtonsoft.Json if perf degrades |
| **MudBlazor CSS** | 6.10.0 | Material Design updates break screenshots | Screenshot regression tests; freeze version for production data |

### Operational Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| **PM Edits data.json While Dashboard Runs** | Medium | Data inconsistency; stale metrics | Document PM workflow: edit locally → test → deploy; file lock detection |
| **Service Crash Without Restart** | Medium | Dashboard offline | TopShelf Windows Service (auto-restart); health check endpoint; Task Scheduler auto-restart |
| **Data.json Becomes Stale** | High | Executives cite outdated numbers | Add "Last Updated" timestamp; educate PM to refresh before screenshots; version-control snapshots |
| **Network Latency on Internal Network** | Low | Slow refresh | Monitor WebSocket latency; optimize ViewModel serialization; local cache retention |

### Security Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| **Unauthorized data.json Access** | Low | Data exfiltration | OS file permissions (NTFS ACLs); restrict to dashboard service account; audit file access |
| **XSS via JSON Fields** | Low | Malicious code execution | Input validation on Name/Description fields; HTML-escape all rendered text (MudBlazor default) |
| **DoS from Large JSON** | Very Low | Service unavailable | File size limits (warn if >100MB); deserialize timeout (5s); rate-limit refresh (1 per 5s) |
| **Unencrypted Local Data** | Very Low | Data at rest exposure | Acceptable for trusted local environment; add HTTPS if deployed to external network |

### Immediate Actions (Before MVP Deployment)

- [ ] Add comprehensive JSON validation in DataRepository
- [ ] Implement error boundary on DashboardPage (graceful degradation)
- [ ] Document PM workflow for data.json edits
- [ ] Configure OS file permissions (read-only for dashboard service account)
- [ ] Test Windows Service wrapper (TopShelf) auto-restart mechanism
- [ ] Create sample data.json with schema documentation

### Short-Term Actions (Post-MVP)

- [ ] Add health check endpoint `/health` for monitoring
- [ ] Implement startup performance logging (JSON size, parse duration)
- [ ] Create screenshot regression test (baseline folder + CI comparison)
- [ ] Add auto-reconnect logic for WebSocket disconnects
- [ ] Monitor in-memory ViewModel size; alert if >100MB

### Long-Term Actions (Scaling)

- [ ] Monitor memory usage; plan SQLite migration at 5MB+ data
- [ ] Implement project-level filtering if >100 projects
- [ ] Add Serilog structured logging for audit trail (when audit required)
- [ ] Design load-balanced deployment (2-3 Kestrel instances) if >20 users confirmed

---

## Implementation Timeline

**Phase 1 (Week 1):** Core Infrastructure
- Set up Blazor Server project with MudBlazor
- Define data.json schema + populate sample data
- Implement ProjectDataService + DataRepository
- Write unit tests for KPI aggregation (target 80% coverage)
- **Deliverable:** Service layer testable and working

**Phase 2 (Week 2):** Dashboard UI
- Implement DashboardPage.razor with MudBlazor Grid layout
- Create StatusCard.razor component
- Implement TimelineChart.razor with custom CSS
- Test refresh functionality
- **Deliverable:** Full dashboard with screenshot for designer review

**Phase 3 (Week 3):** Polish + Deployment
- Refine colors and typography for PowerPoint screenshots
- Test responsive layout (multiple screen sizes)
- Publish release build + test standalone Kestrel
- Optional: Wrap in TopShelf Windows Service
- Create deployment documentation + README
- **Deliverable:** Packaged executable ready for production

---

## Success Criteria

✓ Dashboard loads in <2 seconds (startup + KPI aggregation)
✓ Screenshots print crisply to PDF for PowerPoint (1920x1080+, zoom 100%)
✓ KPI calculations verified by unit tests (80%+ coverage)
✓ Deployment requires <5 minutes (copy folder + run executable)
✓ Manual data.json edits reflected on dashboard after F5 refresh
✓ Zero cloud dependencies; all data stays on-premises
✓ Refresh button provides manual data reload with visual feedback
✓ No build or infrastructure expertise required from PM; can update data.json directly