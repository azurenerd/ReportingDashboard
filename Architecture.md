# Architecture

## Overview & Goals

This document describes the system architecture for the Executive Reporting Dashboard, a lightweight, screenshot-optimized web dashboard built on Blazor Server (.NET 8) for executive visibility into project milestones and progress.

**Architectural Goals:**

1. Enable <3 second dashboard loads with <200ms rendering for 30-50 work items
2. Support 100% screenshot-based PowerPoint reporting without complex BI tools or databases
3. Maintain pixel-perfect visual fidelity at 1920x1080 resolution for print/presentation embedding
4. Minimize operational overhead via JSON-based configuration (data.json) with zero code changes for status updates
5. Eliminate external dependencies: no cloud services, no authentication, no database required
6. Support 50-100 concurrent intranet users on a single Windows machine

The architecture prioritizes simplicity and reliability over extensibility, deferring horizontal scaling, advanced state management, and multi-project support to future versions.

---

## System Components

### 1. DashboardService (Core Service Layer)

**Responsibility:** Load, validate, and monitor data.json file system changes.

**Public Interface:**
```csharp
public interface IDashboardService
{
    Task<DashboardData> LoadDataAsync(string filePath);
    Task<bool> ValidateDataAsync(DashboardData data);
    IAsyncEnumerable<DashboardData> WatchDataAsync(string filePath);
}
```

**Dependencies:** None (isolation layer)

**Data Ownership:**
- No persistent state; acts as stateless gateway to file system
- Returns fully hydrated `DashboardData` object

**Responsibilities:**
- Read UTF-8 JSON from `wwwroot/data/data.json`
- Parse with System.Text.Json (PropertyNameCaseInsensitive=true)
- Validate against DataAnnotations and custom rules
- Monitor file changes via FileSystemWatcher
- Handle errors gracefully with user-friendly messages
- Log all operations and errors to ILogger

### 2. DashboardPage.razor (Parent Component)

**Responsibility:** Orchestrate state lifecycle, manage data refresh, coordinate child components.

**Public Interface:**
```csharp
[Parameter] public string DataFilePath { get; set; } = "wwwroot/data/data.json";
[CascadingParameter] public DashboardData CurrentData { get; set; }
public async Task RefreshDataAsync() { }
```

**Dependencies:**
- IDashboardService (injected, scoped lifetime)
- ILogger<DashboardPage>

**Data Ownership:**
- Holds current `DashboardData` in private component field
- Manages loading state and error messages
- Cascades data to all child components

**Responsibilities:**
- Load dashboard data on initialization (`OnInitializedAsync`)
- Provide manual refresh button with user feedback (loading spinner)
- Cascade `DashboardData` to child components via cascading parameters
- Display error banner if JSON is invalid or missing
- Call `StateHasChanged()` after data updates to trigger re-renders
- Measure and log page load time for performance monitoring

### 3. ProjectStatusCard.razor (Child Component)

**Responsibility:** Display summary metrics (counts, % complete, project name/dates).

**Public Interface:**
```csharp
[CascadingParameter] public DashboardData Data { get; set; }
[CascadingParameter] public ProjectMetadata Project { get; set; }
[CascadingParameter] public DashboardSummary Summary { get; set; }
```

**Dependencies:** None (read-only display)

**Data Ownership:** None; read-only projection of parent data

**Responsibilities:**
- Display shipped/in-progress/carryover count badges
- Show overall % complete as primary metric
- Render project name, start date, end date
- Color-code status metrics (Green=shipped, Yellow=in-progress, Red=carryover)
- Render without overflow at 1920px width
- Hide non-essential elements in print view

### 4. MilestoneTimeline.razor (Child Component)

**Responsibility:** Render chronological milestone timeline with calculated status indicators.

**Public Interface:**
```csharp
[CascadingParameter] public IEnumerable<Milestone> Milestones { get; set; }
private List<MilestoneViewModel> ProcessedMilestones { get; set; }
```

**Dependencies:** MilestoneCalculations utility class

**Data Ownership:**
- Transforms Milestone objects to MilestoneViewModel (adds calculated fields)
- Computes: DaysRemaining, IsOverdue, StatusLabel, CSS classes
- Sorts milestones chronologically (earliest to latest)

**Responsibilities:**
- Sort milestones by Date ascending
- Calculate days remaining: `(Date - DateTime.UtcNow).TotalDays`
- Flag milestones ≥3 days overdue as "At Risk" (red)
- Render status badges with color coding: Green=Completed, Yellow=InProgress, Red=AtRisk
- Display milestone name, date, status, description on hover
- Support expandable details section
- Ensure all content fits within 8.5" × 11" print bounds

### 5. ShippedItemsList.razor (Child Component)

**Responsibility:** Tabular display of completed work items.

**Public Interface:**
```csharp
[CascadingParameter] public IEnumerable<WorkItem> ShippedItems { get; set; }
[CascadingParameter] public Dictionary<string, Milestone> MilestoneMap { get; set; }
```

**Dependencies:** None

**Data Ownership:** None (read-only filtering and projection)

**Responsibilities:**
- Filter work items by Category == Shipped
- Map MilestoneId to Milestone.Name via lookup dictionary
- Render table: Title | Milestone | Completed Date
- Alternate row background colors for readability
- Support no horizontal scrolling at 1920px width
- Maintain table structure in print preview

### 6. InProgressItemsList.razor (Child Component)

**Responsibility:** Tabular display of in-progress work items.

**Public Interface:**
```csharp
[CascadingParameter] public IEnumerable<WorkItem> InProgressItems { get; set; }
[CascadingParameter] public Dictionary<string, Milestone> MilestoneMap { get; set; }
```

**Dependencies:** None

**Data Ownership:** None (read-only filtering)

**Responsibilities:**
- Filter work items by Category == InProgress
- Map MilestoneId to Milestone.Name
- Render table: Title | Milestone | % Complete
- Visualize progress bars per item (0-100 range)
- Alternate row colors; no horizontal scrolling
- Support print layout maintenance

### 7. CarryoverIndicator.razor (Child Component)

**Responsibility:** Highlight and display slipped items with risk context.

**Public Interface:**
```csharp
[CascadingParameter] public IEnumerable<WorkItem> CarryoverItems { get; set; }
```

**Dependencies:** None

**Data Ownership:** None (read-only display)

**Responsibilities:**
- Filter work items by Category == Carryover
- Render red warning banner styling (executive attention)
- Display table: Title | Original Target | New Target | Reason
- Show carryover reason for transparency
- Highlight risk: show delta between original and new target dates
- Print styling for visibility

### 8. ProgressIndicator.razor (Child Component)

**Responsibility:** Overall project progress visualization and timeline countdown.

**Public Interface:**
```csharp
[CascadingParameter] public DashboardSummary Summary { get; set; }
[CascadingParameter] public ProjectMetadata Project { get; set; }
```

**Dependencies:** None

**Data Ownership:** None (computed projections only)

**Responsibilities:**
- Display overall % complete via horizontal progress bar
- Calculate days to project end date: `(Project.EndDate - DateTime.UtcNow).TotalDays`
- Render project end date in local time
- Color-code status: Green if on track, Red if at risk (% complete vs. elapsed time)
- Support print layout without overflow

---

## Component Interactions

### Primary Use Case: Load & Display Dashboard

**Sequence:**

1. **User navigates to dashboard URL**
   - Browser requests `index.html`
   - Blazor Server initializes WebSocket connection
   - DashboardPage component mounts

2. **DashboardPage.OnInitializedAsync() executes**
   - Calls `IDashboardService.LoadDataAsync("wwwroot/data/data.json")`
   - Sets `isLoading = true`
   - Catches `DashboardLoadException` and displays error banner if JSON invalid

3. **DashboardService.LoadDataAsync() executes**
   - Reads file: `File.ReadAllTextAsync(filePath, Encoding.UTF8)`
   - Parses JSON: `JsonSerializer.Deserialize<DashboardData>(json, options)`
   - Validates: `Validator.TryValidateObject(data, context, results, validateAllProperties: true)`
   - Returns `DashboardData` object or throws exception
   - Logs operation status to ILogger

4. **DashboardPage receives data**
   - Stores in `currentData` field
   - Sets `isLoading = false`
   - Calls `StateHasChanged()` to trigger render

5. **Blazor renders component tree**
   - DashboardPage calls `base.BuildRenderTree(builder)` with cascading parameters
   - Each child component receives data via `[CascadingParameter] public DashboardData Data`
   - Child components independently render their data subsets:
     - `ProjectStatusCard`: displays summary metrics
     - `MilestoneTimeline`: renders milestones with calculated status
     - `ShippedItemsList`, `InProgressItemsList`, `CarryoverIndicator`: filter and display work items
     - `ProgressIndicator`: shows overall progress

6. **Browser displays dashboard**
   - HTML sent over WebSocket to client
   - Client renders in browser window
   - Page load time measured and logged

### Secondary Use Case: Manual Refresh

**Sequence:**

1. **User clicks "Refresh" button**
   - Button click event triggers `RefreshDataAsync()`

2. **RefreshDataAsync() executes**
   - Sets `isLoading = true`
   - Calls `IDashboardService.LoadDataAsync(DataFilePath)` again
   - Updates `currentData` field
   - Sets `isLoading = false`
   - Calls `InvokeAsync(StateHasChanged)` to re-render on UI thread

3. **WebSocket delta sync**
   - Blazor Server calculates render tree diff
   - Sends only changed HTML segments over WebSocket (~10 KB max payload)
   - Client updates DOM incrementally

4. **Dashboard re-renders**
   - Child components re-evaluate with new data
   - Users see updated milestone status, work item counts, etc.
   - Toast notification confirms refresh complete

### Tertiary Use Case: Auto-Reload on File Change (v2)

**Sequence (Future Enhancement):**

1. **FileSystemWatcher detects data.json change**
   - `FileSystemWatcher.Changed` event fires
   - Logs file modification with timestamp

2. **Service notifies parent component**
   - Calls `parent.InvokeAsync(() => RefreshDataAsync())`
   - Ensures UI thread safety

3. **All connected clients receive update**
   - Blazor Server broadcasts via WebSocket
   - Dashboard auto-refreshes without user action

---

## Data Model

### Entity Definitions

#### ProjectMetadata
```csharp
public class ProjectMetadata
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(255, MinimumLength = 1)]
    public string Name { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Range(0, 100)]
    public int? OverallStatusPercentComplete { get; set; }
}
```

**Purpose:** Stores project-level metadata (name, timeline, description, overall status).

**Validation:** Required fields enforced; dates must be valid ISO 8601 UTC.

#### Milestone
```csharp
public class Milestone
{
    [Required]
    [RegularExpression(@"^[a-z0-9_-]{1,50}$")]
    public string Id { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Name { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [EnumDataType(typeof(MilestoneStatus))]
    public MilestoneStatus Status { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Computed]
    public int DaysRemaining { get; set; }

    [Computed]
    public bool IsOverdue { get; set; }

    [Computed]
    public string StatusLabel { get; set; }
}

public enum MilestoneStatus
{
    Planned = 0,
    InProgress = 1,
    Completed = 2,
    AtRisk = 3
}
```

**Purpose:** Represents project milestones with target dates and status.

**Validation:** Id alphanumeric, Date required, Status enum enforced.

**Computed Fields:** Calculated by MilestoneTimeline component (not stored in JSON).

#### WorkItem
```csharp
public class WorkItem
{
    [Required]
    [RegularExpression(@"^[a-z0-9_-]{1,50}$")]
    public string Id { get; set; }

    [Required]
    [StringLength(512, MinimumLength = 1)]
    public string Title { get; set; }

    [Required]
    [EnumDataType(typeof(WorkItemCategory))]
    public WorkItemCategory Category { get; set; }

    [StringLength(50)]
    public string MilestoneId { get; set; }

    [Range(0, 100)]
    public int? PercentComplete { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? OriginalTarget { get; set; }

    public DateTime? NewTarget { get; set; }

    [StringLength(500)]
    public string CarryoverReason { get; set; }

    public string AssignedTo { get; set; }

    [StringLength(1000)]
    public string Notes { get; set; }
}

public enum WorkItemCategory
{
    Shipped = 0,
    InProgress = 1,
    Carryover = 2
}
```

**Purpose:** Represents individual work items with category, milestone association, and completion metadata.

**Validation:** Title required, Category enum enforced, PercentComplete 0-100, dates optional.

**Relationships:** MilestoneId references Milestone.Id (client-side join in components).

#### DashboardSummary
```csharp
public class DashboardSummary
{
    [Range(0, 10000)]
    public int ShippedCount { get; set; }

    [Range(0, 10000)]
    public int InProgressCount { get; set; }

    [Range(0, 10000)]
    public int CarryoverCount { get; set; }

    [Range(0, 100)]
    public int OverallPercentComplete { get; set; }

    public DateTime LastUpdated { get; set; }
}
```

**Purpose:** Aggregated summary metrics for dashboard display.

**Calculation:** Can be pre-calculated in data.json or derived from work items at load time.

#### Root DashboardData
```csharp
public class DashboardData
{
    [Required]
    public ProjectMetadata Project { get; set; } = new();

    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public List<Milestone> Milestones { get; set; } = new();

    [Required]
    [MinLength(1)]
    [MaxLength(500)]
    public List<WorkItem> WorkItems { get; set; } = new();

    public DashboardSummary Summary { get; set; } = new();

    [Computed]
    public DateTime DataLoadedAt { get; set; } = DateTime.UtcNow;
}
```

**Purpose:** Root container for entire dashboard data.

**Validation:** Project and collections required; size limits enforced.

### Storage Strategy

**Location:** `wwwroot/data/data.json` (Windows-local file system)

**Format:** UTF-8 JSON, camelCase property names, ISO 8601 UTC dates

**Persistence Model:**
- Single-threaded writes via file system (no database)
- No concurrent write protection (single maintainer assumption)
- Manual backups: daily `robocopy` to `D:\backups\dashboard`
- 30-day rolling retention

**Sample data.json:**
```json
{
  "project": {
    "name": "Q2 Cloud Migration",
    "startDate": "2026-01-01T00:00:00Z",
    "endDate": "2026-06-30T23:59:59Z",
    "description": "Migrate legacy systems to cloud",
    "overallStatusPercentComplete": 62
  },
  "milestones": [
    {
      "id": "m1",
      "name": "Phase 1: Infrastructure",
      "date": "2026-02-28T23:59:59Z",
      "status": "completed",
      "description": "Provision cloud resources"
    }
  ],
  "workItems": [
    {
      "id": "wi1",
      "title": "Database migration",
      "category": "shipped",
      "milestoneId": "m1",
      "completedDate": "2026-02-15T18:00:00Z"
    }
  ],
  "summary": {
    "shippedCount": 8,
    "inProgressCount": 5,
    "carryoverCount": 2,
    "overallPercentComplete": 62,
    "lastUpdated": "2026-04-09T21:12:00Z"
  }
}
```

---

## API Contracts

### IDashboardService Interface

**Lifetime:** Scoped (one instance per HTTP request; shared across components in request)

**Methods:**

#### LoadDataAsync(string filePath): Task<DashboardData>
- **Purpose:** Load and validate dashboard data from JSON file synchronously
- **Parameters:** `filePath` = full path to data.json
- **Returns:** Fully deserialized and validated `DashboardData` object
- **Exceptions:**
  - `FileNotFoundException` if file missing
  - `JsonException` if JSON malformed (includes line number, byte position)
  - `InvalidOperationException` if JSON parses but validation fails
  - `DashboardLoadException` (custom) for all load errors
- **Logging:** Info on success, Error on failure with full exception
- **Performance:** <100ms for 30-50 item dataset

#### ValidateDataAsync(DashboardData data): Task<bool>
- **Purpose:** Validate data against DataAnnotations and custom rules
- **Parameters:** `data` = DashboardData object to validate
- **Returns:** true if valid, false if validation fails
- **Side Effects:** Logs validation errors via ILogger
- **Custom Rules:**
  - Milestones sorted by Date
  - WorkItem.MilestoneId references existing Milestone.Id
  - No duplicate WorkItem.Id or Milestone.Id values

#### WatchDataAsync(string filePath): IAsyncEnumerable<DashboardData>
- **Purpose:** Monitor data.json for changes and yield updated data (v2)
- **Parameters:** `filePath` = path to monitor
- **Returns:** Async enumerable that yields on file changes
- **Implementation:** FileSystemWatcher on file LastWrite event
- **Behavior:** Infinite loop yielding data updates; callers must break the enumeration
- **Performance:** Detects changes within 100ms

### Exception Hierarchy

```csharp
public class DashboardLoadException : Exception
{
    public DashboardLoadException(string message) : base(message) { }
    public DashboardLoadException(string message, Exception innerException) 
        : base(message, innerException) { }
}

public class DashboardValidationException : Exception
{
    public List<string> Errors { get; set; } = new();
    public DashboardValidationException(string message, List<string> errors) 
        : base(message) { Errors = errors; }
}
```

### Blazor Component Cascading Parameters

**DashboardPage cascades to all children:**
```
[CascadingParameter] public DashboardData Data
[CascadingParameter] public ProjectMetadata Project
[CascadingParameter] public List<Milestone> Milestones
[CascadingParameter] public List<WorkItem> WorkItems
[CascadingParameter] public DashboardSummary Summary
[CascadingParameter] public Dictionary<string, Milestone> MilestoneMap
```

**Child components receive (filtered examples):**
```
// MilestoneTimeline.razor
[CascadingParameter] public IEnumerable<Milestone> Milestones

// ShippedItemsList.razor
[CascadingParameter] public IEnumerable<WorkItem> WorkItems (filtered by Category==Shipped)
[CascadingParameter] public Dictionary<string, Milestone> MilestoneMap
```

### Error Handling Contract

**Dashboard displays user-friendly error banner if:**
- data.json missing: "Dashboard data file not found. Please contact administrator."
- JSON malformed: "Invalid JSON format in data.json at line {LineNumber}, position {Position}."
- Validation failed: "Dashboard data validation failed. Check required fields and data types."
- File read error: "Unable to read dashboard data. Please try refreshing."

**All errors logged to ILogger with full exception details (never shown to user).**

---

## Infrastructure Requirements

### Hosting Environment

**Target Platforms:**
- Windows Server 2022+ (production)
- Windows 10/11 Pro (development/small deployments)
- No Linux, macOS, or Docker support required

**Runtime:**
- .NET 8 LTS (November 2026 EOL)
- Included in self-contained executable
- No external runtime installation required

**Self-Contained Deployment:**
```bash
dotnet publish -c Release -r win-x64 --self-contained
# Output: ~200 MB single .exe with all dependencies bundled
```

### Process Management

**Option A: Windows Service (Recommended for production)**
```batch
nssm install AgentSquadDashboard "C:\dashboards\AgentSquadDashboard.exe"
nssm set AgentSquadDashboard AppDirectory "C:\dashboards"
nssm set AgentSquadDashboard AppStdout "C:\dashboards\logs\stdout.log"
nssm set AgentSquadDashboard AppStderr "C:\dashboards\logs\stderr.log"
nssm set AgentSquadDashboard Start SERVICE_AUTO_START
nssm start AgentSquadDashboard
```

**Option B: Task Scheduler (Scheduled restarts)**
```xml
<Task>
  <Triggers><BootTrigger /></Triggers>
  <Actions>
    <Exec>
      <Command>C:\dashboards\AgentSquadDashboard.exe</Command>
      <Arguments>--urls=https://localhost:5001</Arguments>
    </Exec>
  </Actions>
</Task>
```

### Networking

**Development:**
- HTTP: `http://localhost:5000`

**Production:**
- HTTPS: `https://dashboard.internal:443` (via IIS/nginx reverse proxy)
- TLS 1.3 minimum
- Valid SSL certificate (self-signed acceptable for intranet)

**Reverse Proxy Configuration (IIS):**
```xml
<system.webServer>
  <rewrite>
    <rules>
      <rule name="ReverseProxyToBlazor">
        <match url="(.*)" />
        <action type="Rewrite" url="http://localhost:5000/{R:1}" />
      </rule>
    </rules>
  </rewrite>
  <httpProxy>
    <enabled>true</enabled>
    <socket closeConnection="false" />
  </httpProxy>
</system.webServer>
```

**WebSocket Configuration:**
- Enable WebSocket protocol in IIS Application Pool
- Keep-alive interval: 5 seconds (default)
- Message timeout: 30 seconds

### Storage & Directories

**Directory Structure:**
```
C:\dashboards\
├── AgentSquadDashboard.exe
├── appsettings.json
├── appsettings.Production.json
├── wwwroot\
│   ├── css\
│   │   ├── bootstrap.min.css
│   │   └── app.css
│   ├── data\
│   │   └── data.json
│   ├── js\
│   └── index.html
├── logs\
│   ├── app-.log (daily rolling)
│   ├── stdout.log
│   └── stderr.log
└── backup\
    ├── data_2026-04-09.json
    ├── data_2026-04-08.json
    └── ...
```

**File Permissions (Windows NTFS ACLs):**
```
C:\dashboards\
  Full Control: SYSTEM, Administrators, [ServiceAccount]
  Modify: [DashboardUser]
  Read-Only: [Everyone on intranet via AD group]

C:\dashboards\wwwroot\data\data.json
  Full Control: [DataMaintainer]
  Read-Only: [DashboardService]

C:\dashboards\logs\
  Full Control: SYSTEM, [ServiceAccount]
  Read: Administrators
```

**Backup Strategy:**
```batch
REM Daily backup (runs at 2 AM)
robocopy C:\dashboards\wwwroot\data D:\backups\dashboard /MON:1 /XO
REM Retention: 30 days rolling (delete files older than 30 days)
```

### Configuration Files

**appsettings.json (Development):**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "Dashboard": {
    "DataFilePath": "wwwroot/data/data.json",
    "FileWatcherEnabled": false,
    "CacheDurationSeconds": 300,
    "MaxConcurrentSessions": 50
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

**appsettings.Production.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "Dashboard": {
    "DataFilePath": "C:/dashboards/wwwroot/data/data.json",
    "FileWatcherEnabled": true,
    "CacheDurationSeconds": 600,
    "MaxConcurrentSessions": 100
  },
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:443",
        "Certificate": {
          "Path": "C:/certs/dashboard.pfx",
          "Password": "${CERT_PASSWORD}"
        }
      }
    }
  }
}
```

### Logging & Monitoring

**Serilog Configuration:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();
```

**Key Metrics to Monitor:**
- Page load time (target: <3 seconds)
- JSON parse time (target: <100 ms)
- Active WebSocket connections
- Memory baseline (<200 MB)
- File access errors
- Validation failures

**Health Check Endpoint (Optional v2):**
```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString()
            })
        });
    }
});
```

### CI/CD Pipeline

**Build Script (build.ps1):**
```powershell
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

dotnet clean
dotnet build -c $Configuration
dotnet test --no-build -c $Configuration
dotnet publish -c $Configuration -r $Runtime --self-contained -o ./publish
```

**Deployment Script (deploy.ps1):**
```powershell
param(
    [string]$DeployPath = "C:\dashboards",
    [string]$BackupPath = "D:\backups\dashboard"
)

Copy-Item "$DeployPath\wwwroot\data\data.json" "$BackupPath\data_$(Get-Date -Format 'yyyy-MM-dd').json"
nssm stop AgentSquadDashboard
Copy-Item "publish\*" $DeployPath -Recurse -Force
nssm start AgentSquadDashboard
Write-Host "Deployment complete"
```

---

## Technology Stack Decisions

### Blazor Server 8.0+

**Justification:**
- Server-side rendering eliminates client-side JavaScript complexity
- WebSocket-based state management ideal for intranet dashboards
- Native C# component model simplifies data binding
- No build tooling required (pure .NET compilation)
- Real-time interactivity without explicit API layer

**Alternatives Rejected:**
- Blazor WebAssembly: Requires client-side bundle, slower initial load
- ASP.NET MVC: Lacks interactive component model, increases page complexity
- SPA frameworks (React, Vue): Require JavaScript, DevOps overhead

### System.Text.Json

**Justification:**
- Native to .NET 8 (zero external dependencies)
- 2-3x faster parsing than Newtonsoft.Json
- Compile-time source generation support
- PropertyNameCaseInsensitive = true handles data.json camelCase convention
- Sufficient for simple JSON structure

**Alternatives Rejected:**
- Newtonsoft.Json: Adds 1.5 MB, slower, legacy support
- Jil: Fewer features, less community adoption

### RadzenBlazor 4.x

**Justification:**
- Polished, executive-grade UI components (Timeline, Chart, Badge, Card)
- Print-optimized rendering (critical for PowerPoint screenshots)
- Responsive design at 1920x1080 without custom CSS burden
- Telerik backing ensures long-term maintenance
- Active community (>1M NuGet downloads)

**Alternatives Rejected:**
- OxyPlot: Limited Blazor integration, primarily WPF/WinForms
- Chart.js via interop: JavaScript overhead, screenshot timing issues
- Custom SVG: Zero dependencies but high development effort (~40 hours)

### Bootstrap 5.3.x

**Justification:**
- Industry-standard responsive framework
- Grid system enables clean layout without custom CSS
- Active maintenance (updated March 2024)
- Massive community adoption and documentation
- Zero .NET-specific dependencies

**Alternatives Rejected:**
- Tailwind CSS: JIT compilation overhead, utility-first complexity
- Material Design: Visual inconsistency with RadzenBlazor
- Custom CSS only: Increased development effort, higher defect risk

### FileSystemWatcher (System.IO)

**Justification:**
- Native Windows API, highly reliable
- Minimal performance overhead
- No external dependencies
- Suitable for single-server deployment

**Alternatives Rejected:**
- Polling: Higher CPU usage, less responsive
- Database backend: Introduces infrastructure dependency
- API webhooks: Requires external service

### .NET 8 LTS

**Justification:**
- Long-term support until November 2026
- Performance improvements over .NET 7
- Native JSON source generation support
- Latest C# language features (records, pattern matching)

**Migration Path:**
- Plan upgrade to .NET 9/10 LTS by Q3 2026

---

## Security Considerations

### Authentication & Authorization

**MVP Approach:** No built-in authentication

- Assume intranet-only access (network-level controls sufficient)
- Windows domain AD groups restrict dashboard URL access
- No role-based authorization required at application level

**Future Enhancement (v2):** Windows Authentication
```csharp
// In Program.cs
builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);

// In launchSettings.json
"windowsAuthentication": true,
"anonymousAuthentication": false
```

### Data Protection at Rest

**data.json Security:**
- Windows NTFS ACLs: DataMaintainer has write, service account has read-only
- No encryption at rest (project metadata is non-sensitive)
- Regular backups to secure network share
- Access auditing via Windows Event Viewer

**Sensitive Data Policy:**
- **Never store** passwords, API keys, or PII in data.json
- Exception handling: Log errors without exposing file paths to users

### Data Protection in Transit

**WebSocket Encryption:**
- Enforce HTTPS in production (TLS 1.3 minimum)
- Certificate: Valid SSL cert via reverse proxy (IIS/nginx)
- Self-signed certs acceptable for intranet
- Disable HTTP in appsettings.Production.json

**WebSocket Configuration:**
```csharp
app.UseHttpsRedirection();
app.MapBlazorHub(); // Requires HTTPS in production
```

### Logging & Audit

**Log File Protection:**
- Location: `C:\dashboards\logs\`
- Permissions: Administrators + ServiceAccount read/write only
- Retention: 30 days rolling (auto-delete via RollingInterval.Day)
- Redaction: Never log sensitive data (implement `[SensitiveData]` attribute on fields)

**Sensitive Logging Example:**
```csharp
[SensitiveData]
public string CarryoverReason { get; set; } // Example: if contains PII

// In logging
_logger.LogInformation("Processed work item {Id} with reason {Reason}", 
    item.Id, 
    "[REDACTED]"); // Never log sensitive fields
```

### Input Validation

**JSON Schema Validation (via DataAnnotations):**
```csharp
[Required, StringLength(255, MinimumLength = 1)]
public string Name { get; set; }

[Range(0, 100)]
public int? PercentComplete { get; set; }

[RegularExpression(@"^[a-z0-9_-]{1,50}$")]
public string Id { get; set; }
```

**Output Encoding (XSS Prevention):**
- Blazor Server auto-escapes HTML by default
- Use `@Html.Raw()` **only** for trusted RadzenBlazor components
- Never render user-supplied HTML
- Validate all milestone/work item descriptions against allowed characters

**File Path Traversal Prevention:**
```csharp
var fullPath = Path.GetFullPath(filePath);
var basePath = Path.GetFullPath("wwwroot/data");
if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
    throw new UnauthorizedAccessException("Invalid file path");
```

### Threat Model

| Threat | Likelihood | Impact | Mitigation |
|--------|-----------|--------|-----------|
| Unauthorized access to dashboard | Low | Moderate | Network-level access control (AD groups), HTTPS |
| Malformed JSON crashes app | Medium | High | Strict validation, error banner, graceful degradation |
| File tampering (data.json) | Low | High | NTFS ACLs, backups, audit logging |
| WebSocket eavesdropping | Low | Moderate | HTTPS/TLS 1.3 enforcement |
| Timezone calculation errors | Medium | Moderate | UTC storage, `DateTime.UtcNow` only |
| Large JSON DoS | Low | Low | File size limit (5 MB), parse timeout (10s) |

---

## Scaling Strategy

### Single-Machine Architecture (Current MVP)

**Capacity Targets:**
- Concurrent WebSocket connections: 50-100 users
- Memory per connection: ~1-2 MB
- Dashboard dataset: <500 work items
- JSON file size: <5 MB
- Rendering time: <200 ms

**Hardware Assumptions:**
- Windows Server 2022 with 8+ CPU cores
- 16+ GB RAM
- SSD for data.json reads
- Gigabit network connection

### Bottleneck Analysis & Mitigations

| Bottleneck | Symptom | Mitigation | Timeline |
|-----------|---------|-----------|----------|
| **File I/O on data.json** | High disk latency during refresh | Implement in-memory cache (300s TTL) | MVP |
| **WebSocket payload size** | Slow state updates | Compress payloads, delta sync only | v1.1 |
| **JSON parsing CPU** | Spike during large file loads | Pre-parse + cache (5-10 min) | v1.1 |
| **Memory exhaustion** | Browser crashes >100 concurrent users | Horizontal scaling with Redis | v2 |
| **Milestone calculations** | Render delay with 100+ milestones | Virtual scrolling in UI table | v1.1 |

### In-Memory Caching (MVP)

**Implementation:**
```csharp
public class CachedDashboardService : IDashboardService
{
    private DashboardData _cache;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private const int CacheTtlSeconds = 300;

    public async Task<DashboardData> LoadDataAsync(string filePath)
    {
        if (_cache != null && DateTime.UtcNow < _cacheExpiry)
            return _cache;

        var data = await _baseService.LoadDataAsync(filePath);
        _cache = data;
        _cacheExpiry = DateTime.UtcNow.AddSeconds(CacheTtlSeconds);
        return data;
    }
}

// In Program.cs
builder.Services.AddSingleton<IDashboardService>(
    new CachedDashboardService(new DashboardService(logger)));
```

**Cache Hit Ratio:** Estimated 90%+ if file changes infrequent

### Horizontal Scaling Path (v2 - Future)

**If exceeding 100 concurrent users:**

1. **Deploy Multiple App Servers**
   - Load balancer: Azure Load Balancer or nginx
   - Sticky sessions: WebSocket affinity required
   - Auto-scaling: Monitor CPU/memory, add servers if >70%

2. **Shared Cache Tier**
   - Redis cluster for distributed caching
   - Keep single data.json source of truth
   - Invalidate cache on file change (broadcast via SignalR)

3. **Data Layer Upgrade**
   - Migrate from file-based JSON to SQLite
   - Advisory file locks for concurrent write safety
   - Enable audit table for historical snapshots (future reporting)

4. **Performance Optimization**
   - Virtual scrolling for work item tables (>200 items)
   - Lazy loading of milestone details
   - Gzip compression on HTTP responses

---

## Risks & Mitigations

### Technical Risks

#### 1. File Contention on data.json

**Risk Level:** Medium

**Impact:** Stale or corrupt data if multiple writers attempt concurrent updates

**Probability:** Medium (assumes single maintainer, but automation could violate assumption)

**Mitigation:**
- **MVP:** Document single-threaded write assumption in operational runbook
- **Monitoring:** Log all file read/write operations with timestamps
- **Future:** Implement advisory file locks via `FileStream.Lock()` if bottleneck emerges
- **Long-term:** Migrate to SQLite with proper concurrency control

**Detection:** Alert if multiple write events detected within 5 seconds

#### 2. Malformed JSON Crashes Application

**Risk Level:** High

**Impact:** Entire dashboard offline, executives unable to view status

**Probability:** Medium (manual file editing by non-technical user)

**Mitigation:**
- **MVP:** Strict DataAnnotations validation + custom validators
- **UI:** Error banner displays validation errors to user (non-stack-trace)
- **Logging:** Full error details logged for operator debugging
- **Graceful Degradation:** Cache last-known-good data for 30 minutes if load fails
- **Validation:** Unit tests with 50+ edge cases (empty strings, special chars, invalid dates)

**Example Error Message:**
```
"Invalid dashboard data. Please check:
- Required field 'name' in project metadata
- ISO 8601 date format (YYYY-MM-DDTHH:MM:SSZ)
- Milestone 'id' must be alphanumeric
Contact administrator if problem persists."
```

#### 3. WebSocket Disconnection

**Risk Level:** Low

**Impact:** User sees stale data briefly; dashboard becomes unresponsive

**Probability:** Low (intranet assumption, stable network)

**Mitigation:**
- **Native:** Blazor Server auto-reconnects within 3-5 seconds
- **UI:** Display reconnection status to user
- **Fallback:** Manual refresh button always available
- **Timeout:** Graceful degradation after 30-second outage

#### 4. JSON File Missing

**Risk Level:** Medium

**Impact:** Dashboard fails to load on application start

**Probability:** Low (backup strategy, but deployment errors possible)

**Mitigation:**
- **Startup Check:** Verify file existence before Blazor initialization
- **Error Message:** "Dashboard data file not found at {path}. Please contact administrator."
- **Initialization:** Create sample data.json on first startup if missing (optional)

#### 5. Memory Leak in FileSystemWatcher

**Risk Level:** Medium

**Impact:** Gradual memory bloat over weeks, eventual server restart required

**Probability:** Low (properly disposed in using block)

**Mitigation:**
- **Code Review:** Ensure `using` statement wraps FileSystemWatcher
- **Monitoring:** Track memory baseline weekly; alert if growth >50 MB/week
- **Test:** Run memory profiler for 24-hour continuous operation
- **Workaround:** Scheduled restart via Task Scheduler every Sunday 2 AM

#### 6. Timezone Calculation Errors

**Risk Level:** Medium

**Impact:** Executives see incorrect milestone status (off by 1-2 days)

**Probability:** Medium (DST transitions, system clock drift)

**Mitigation:**
- **Storage:** All dates stored in UTC (ISO 8601) in data.json
- **Calculation:** Use `DateTime.UtcNow` **only** (never `DateTime.Now`)
- **Testing:** Verify calculations at DST transition dates (2026-03-08, 2026-11-01)
- **Logging:** Log all date comparisons for audit trail
- **Display:** Show timezone context in UI (e.g., "Eastern Time")

#### 7. Certificate Expiration

**Risk Level:** Low

**Impact:** HTTPS fails, dashboard unreachable in production

**Probability:** Low (but 100% if not managed)

**Mitigation:**
- **Tracking:** Calendar reminder 90 days before expiry
- **Automation:** Script to check cert expiry and alert 30 days out
- **Renewal:** Self-signed certs (intranet only) don't expire; CA-signed certs require annual renewal
- **Redundancy:** Keep 2-week buffer (replace 2 weeks before expiry)

#### 8. Log File Disk Full

**Risk Level:** Low

**Impact:** Application silently stops logging; silent failure invisible to operators

**Probability:** Low (30-day retention, typical dashboard log volume ~5-10 MB/month)

**Mitigation:**
- **Monitoring:** Alert if `C:\dashboards\logs\` exceeds 80% capacity
- **Rotation:** RollingInterval.Day automatic cleanup (retainedFileCountLimit: 30)
- **Threshold:** Set disk space alert at <10% free

### Dependency Risks

#### RadzenBlazor 4.x Commercial Licensing

**Risk Level:** Medium (long-term)

**Impact:** Future licensing cost or forced migration

**Probability:** Low (Telerik provides free tier for development; commercial models typically stable)

**Mitigation:**
- **Contract:** Negotiate 3-year support agreement with Telerik
- **Fallback:** Document migration path to custom SVG rendering (~8 weeks effort)
- **Monitoring:** Subscribe to Telerik release notes; track breaking changes
- **Alternative:** Evaluate OxyPlot or lightweight charting before licensing becomes blocking

#### .NET 8 LTS EOL (November 2026)

**Risk Level:** Low

**Impact:** Security patches stop; long-term maintenance burden

**Probability:** Certain (by design)

**Mitigation:**
- **Timeline:** Plan migration to .NET 9 LTS (November 2027) by Q3 2026
- **Effort:** Minimal (backward-compatible, no API changes expected)
- **Testing:** Run smoke tests on .NET 9 RC before November 2026

#### Bootstrap 5.3.x CVE in CSS Parser

**Risk Level:** Low (for this application)

**Impact:** Potential XSS if user content rendered (not applicable here)

**Probability:** Low (executive metadata only)

**Mitigation:**
- **Monitoring:** Subscribe to Bootstrap security advisories
- **Update Cadence:** Review quarterly; apply critical patches within 7 days
- **Validation:** Scan dependencies with `dotnet list package --vulnerable`

#### System.Text.Json Limited Customization

**Risk Level:** Low

**Impact:** May require workarounds for complex serialization

**Probability:** Low (JSON structure is simple)

**Mitigation:**
- **Acceptance:** System.Text.Json feature parity sufficient for this use case
- **Fallback:** Migrate to Newtonsoft.Json if needed (performance trade-off, +1.5 MB)

### Mitigation Execution Plan

**Before MVP Release (Week 1-2):**
- [ ] Validate JSON with 50+ edge cases (empty strings, special chars, out-of-range dates, null fields)
- [ ] Test FileSystemWatcher reliability (kill process, network delay, rapid file writes)
- [ ] Performance audit: measure <3s load time, <200ms render time, <100MB memory
- [ ] Security audit: verify HTTPS, ACL permissions, log redaction
- [ ] Disaster recovery test: restore from backup, verify data integrity

**Post-Release (Ongoing):**
- [ ] Week 1-4: Monitor error logs daily for validation failures, file access errors
- [ ] Week 4-8: Measure memory baseline; alert if growth >50 MB/week
- [ ] Monthly: Review NuGet security advisories; apply critical patches
- [ ] Monthly: Monitor WebSocket connections; alert if >5 disconnections/hour
- [ ] Quarterly: Capacity planning; track peak concurrent users
- [ ] Quarterly: Upgrade dependencies if minor versions available
- [ ] Annual: DST transition testing; verify date calculations
- [ ] Annual: Certificate renewal check; plan renewal if CA-signed

---

## Operational Runbook

### Deployment

**Step 1: Build**
```powershell
cd C:\Git\AgentSquad\src\AgentSquad.Runner
dotnet clean
dotnet build -c Release
dotnet test --no-build -c Release
dotnet publish -c Release -r win-x64 --self-contained -o C:\publish
```

**Step 2: Backup Current Data**
```powershell
Copy-Item "C:\dashboards\wwwroot\data\data.json" "D:\backups\dashboard\data_$(Get-Date -Format 'yyyy-MM-dd_HHmmss').json"
```

**Step 3: Stop Service**
```batch
nssm stop AgentSquadDashboard
```

**Step 4: Deploy**
```powershell
Copy-Item "C:\publish\*" "C:\dashboards" -Recurse -Force -Exclude @("data.json", "logs", "backup")
```

**Step 5: Start Service**
```batch
nssm start AgentSquadDashboard
```

**Step 6: Verify**
```powershell
# Check service status
nssm status AgentSquadDashboard

# Verify HTTPS connectivity
curl https://dashboard.internal:443 -SkipCertificateCheck

# Check logs for errors
Get-Content "C:\dashboards\logs\app-*.log" -Tail 50
```

### Troubleshooting

**Dashboard fails to load**
1. Check `C:\dashboards\logs\app-*.log` for errors
2. Verify `C:\dashboards\wwwroot\data\data.json` exists and is valid JSON
3. Test JSON validity: `C:\dashboards\AgentSquadDashboard.exe --validate`
4. Check file permissions: `icacls C:\dashboards\wwwroot\data`
5. Restart service: `nssm restart AgentSquadDashboard`

**WebSocket connection fails**
1. Verify HTTPS enabled in production
2. Check certificate validity: `certutil -dump C:\certs\dashboard.crt`
3. Test WebSocket: `wscat -c wss://dashboard.internal:443/blazor`
4. Verify reverse proxy configuration (IIS WebSocket module enabled)

**Performance degradation**
1. Check memory usage: `Get-Process AgentSquadDashboard | Select-Object Name, WorkingSet`
2. Monitor CPU: `perfmon` + add "% Processor Time" counter
3. Check active connections: `netstat -an | find "ESTABLISHED"`
4. Analyze logs for JSON parsing time
5. If memory >300 MB, restart service

### Backup & Recovery

**Automated Daily Backup:**
```batch
REM Task Scheduler: Daily at 2:00 AM
robocopy C:\dashboards\wwwroot\data D:\backups\dashboard /MON:1 /XO /R:3
```

**Manual Restore:**
```powershell
# List backups
Get-ChildItem D:\backups\dashboard

# Restore from date
Copy-Item "D:\backups\dashboard\data_2026-04-08.json" "C:\dashboards\wwwroot\data\data.json"

# Verify
(Get-Content C:\dashboards\wwwroot\data\data.json | ConvertFrom-Json).project.name
```

**Disaster Recovery Test (Monthly):**
1. Stop service
2. Delete current data.json
3. Restore from backup
4. Start service
5. Verify dashboard displays correct data
6. Document results in log