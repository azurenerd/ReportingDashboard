# Architecture

## Overview & Goals

**Project**: Executive Dashboard  
**Technology Stack**: C# .NET 8 Blazor Server, local deployment only, .sln project structure  
**Primary Purpose**: Provide executives with real-time visibility into project health, milestones, and progress through a lightweight, screenshot-friendly web application  

**Core Objectives**:
1. Single-page snapshot of project health (<30 second review time)
2. Real-time data updates via JSON file editing (no app restart required)
3. PowerPoint-ready screenshots with consistent, professional visual presentation
4. Absolute simplicity: no authentication, no enterprise complexity, minimal dependencies
5. Support <20 concurrent users on typical developer hardware (2+ cores, 8GB RAM)
6. Local deployment only; no cloud services or external integrations

**Architectural Philosophy**:
- Hybrid static-dynamic rendering: static HTML for screenshot fidelity + Blazor reactivity for file-watch updates
- Zero external infrastructure: single-machine deployment, no database, file-based data source
- Built-in .NET libraries only: leverage System.Text.Json, FileSystemWatcher, ILogger
- Single operator assumption: no multi-user conflict management or distributed locks

---

## System Components

### 1. DashboardDataService (Singleton Service)

**Responsibility**: Single source of truth for all dashboard data. Loads JSON, parses, caches, monitors file changes, notifies subscribers.

**Public Interface**:
```csharp
public class DashboardDataService : IDisposable {
    public event Action OnDataChanged;
    
    public DashboardData GetCurrentData();
    public Project GetProject();
    public IReadOnlyList<Milestone> GetMilestones();
    public IReadOnlyList<WorkItem> GetWorkItems();
    public IReadOnlyList<WorkItem> GetWorkItemsByStatus(WorkItemStatus status);
    public (int Shipped, int InProgress, int CarriedOver) GetStatusCounts();
    public string GetLastError();
    public bool HasData { get; }
}
```

**Key Responsibilities**:
- Load data.json from disk on application startup
- Parse JSON into strongly-typed DashboardData model using System.Text.Json
- Cache parsed data in-memory with last-known-good fallback
- Monitor data.json for changes via FileSystemWatcher with 500ms debounce
- Raise OnDataChanged event when file modifications detected
- Handle parsing errors gracefully with detailed logging
- Provide read-only access to cached models via public methods

**Dependencies**:
- ILogger<DashboardDataService> (Microsoft.Extensions.Logging)
- IOptions<DashboardOptions> (Microsoft.Extensions.Configuration)
- System.IO.FileSystemWatcher (built-in)
- System.Text.Json (built-in)

**Data Ownership**:
- Owns parsed DashboardData model (in-memory singleton cache)
- Owns FileSystemWatcher instance
- Owns error state (last error message, error code, timestamp)
- Tracks file modification hash to prevent duplicate parsing

**Lifecycle**:
- Constructor: Loads data.json, initializes FileSystemWatcher on background thread
- OnInitialized: Called by Blazor components that subscribe to OnDataChanged
- OnDispose: Cleanup FileSystemWatcher, cancel pending timers

---

### 2. DashboardData Model (DTO)

**Responsibility**: Strongly-typed representation of data.json schema. Encapsulates all dashboard entities and validation rules.

**Core Entities**:

**DashboardData (Root)**:
- Project Project (required)
- List<Milestone> Milestones (required, default: empty list)
- List<WorkItem> WorkItems (required, default: empty list)

**Project**:
- string Name (required, max 256 chars)
- string Description (optional, max 1024 chars)

**Milestone**:
- string Name (required, max 256 chars)
- DateTime Date (required, ISO 8601 format)
- string Status (required, enum: "Completed", "On Track", "At Risk")

**WorkItem**:
- string Title (required, max 512 chars)
- WorkItemStatus Status (required, enum: Shipped, InProgress, CarriedOver)
- string Assignee (optional, max 256 chars)

**WorkItemStatus Enum**:
- Shipped = 0
- InProgress = 1
- CarriedOver = 2

**Dependencies**: None (pure data model, no external dependencies)

**Data Ownership**:
- Encapsulates JSON structure and validation via DataAnnotations ([Required], [StringLength], [RegularExpression])
- No behavior logic; read-only properties
- Immutable after deserialization (no setters in production code)

---

### 3. Dashboard.razor (Page Component)

**Responsibility**: Top-level page orchestrator. Renders UI structure, composes child components, subscribes to data changes.

**Route**: `/` and `/dashboard`

**Public Interface**:
```csharp
[Route("/")]
[Route("/dashboard")]
public partial class Dashboard : ComponentBase {
    [Inject] DashboardDataService DataService { get; set; }
    
    protected override async Task OnInitializedAsync();
    private void OnDataChanged();
}
```

**Key Responsibilities**:
- Inject DashboardDataService as dependency
- Subscribe to DashboardDataService.OnDataChanged event in OnInitializedAsync()
- Call StateHasChanged() when data changes (triggers Blazor re-render)
- Render page sections in fixed vertical layout:
  1. Error boundary (if data.json parse failed)
  2. Project summary card (Project.Name, Project.Description)
  3. Milestone timeline component (pass GetMilestones())
  4. Status chart component (pass GetStatusCounts())
  5. Work item list component (pass GetWorkItemsByStatus())
- Unsubscribe from OnDataChanged event on dispose
- Render fixed-width (1200px) container using MudBlazor

**Dependencies**:
- DashboardDataService (injected singleton)
- MilestoneTimeline.razor (child component)
- StatusChart.razor (child component)
- WorkItemList.razor (child component)
- MudBlazor.Components: Container, Card, Grid, Alert, Typography

**Data Ownership**: None (stateless orchestrator; all data managed by DashboardDataService)

**Lifecycle**:
- OnInitializedAsync(): Subscribe to OnDataChanged, initial render
- OnDataChanged(): Call StateHasChanged() to trigger re-render
- Dispose(): Unsubscribe from OnDataChanged event

---

### 4. MilestoneTimeline.razor (Child Component)

**Responsibility**: Render milestones chronologically as custom HTML/SVG timeline. No dependency on data loading.

**Public Interface**:
```csharp
public partial class MilestoneTimeline : ComponentBase {
    [Parameter]
    public IReadOnlyList<Milestone> Milestones { get; set; }
}
```

**Key Responsibilities**:
- Receive milestones via [Parameter]
- Sort milestones by Date ascending
- Render HTML timeline container (1200px fixed width)
- For each milestone: render badge with name, formatted date (MM/DD/YYYY), status icon (SVG circle)
- Color-code status: Green (Completed), Yellow (On Track), Red (At Risk)
- Use CSS Grid or Flexbox for horizontal/vertical layout
- Support empty milestones list (display "No milestones scheduled")

**Dependencies**:
- MudBlazor (optional: Icon, Tooltip components)
- System.Linq (ordering)

**Data Ownership**: None (receives immutable list via parameter)

**Rendering**:
- Custom SVG circles for status indicators (no third-party Gantt library)
- Pure HTML markup for maximum screenshot control
- No JavaScript interop required

---

### 5. StatusChart.razor (Child Component)

**Responsibility**: Render bar/pie chart of work item status counts. Manage ChartJS.Blazor interop.

**Public Interface**:
```csharp
public partial class StatusChart : ComponentBase {
    [Parameter]
    public (int Shipped, int InProgress, int CarriedOver) StatusCounts { get; set; }
    
    [Inject] IJSRuntime JSRuntime { get; set; }
    
    protected override async Task OnParametersSetAsync();
}
```

**Key Responsibilities**:
- Receive status counts via [Parameter]: (Shipped, InProgress, CarriedOver)
- Detect changes to StatusCounts in OnParametersSetAsync()
- If changed: Call InitializeChart() to update Chart.js instance
- Render bar chart (default) or pie chart (optional)
- Display categories: Shipped, In Progress, Carried Over
- Display corresponding work item counts on Y-axis
- Integrate ChartJS.Blazor v4.1.2 for JavaScript interop

**Dependencies**:
- ChartJS.Blazor v4.1.2 (NuGet)
- IJSRuntime (Microsoft.AspNetCore.Components.Web)
- Chart.js library (loaded via CDN or bundled)

**Data Ownership**: None (receives immutable tuple via parameter)

**JavaScript Interop**:
- InitializeChart() method calls JavaScript to create/update Chart.js instance
- No complex custom JavaScript; ChartJS.Blazor wrapper handles interop

---

### 6. WorkItemList.razor (Child Component)

**Responsibility**: Render work items grouped by status in three sections. No dependency on data loading.

**Public Interface**:
```csharp
public partial class WorkItemList : ComponentBase {
    [Parameter]
    public IReadOnlyList<WorkItem> Shipped { get; set; }
    
    [Parameter]
    public IReadOnlyList<WorkItem> InProgress { get; set; }
    
    [Parameter]
    public IReadOnlyList<WorkItem> CarriedOver { get; set; }
}
```

**Key Responsibilities**:
- Receive three grouped work item lists via [Parameter]
- Render three sections (Shipped, In Progress, Carried Over)
- Each section: MudBlazor Card with color-coded header
- Display work items as semantic HTML list (<ul>)
- Each item: Title, Assignee (two-column or stacked layout)
- Color-code headers: Green (Shipped), Blue (InProgress), Orange (CarriedOver)
- Support empty lists (display "No items in this status")

**Dependencies**:
- MudBlazor (Card, Grid, Typography)

**Data Ownership**: None (receives immutable lists via parameters)

---

### 7. DashboardOptions (Configuration Model)

**Responsibility**: Encapsulate configuration settings for appsettings.json binding.

**Public Interface**:
```csharp
public class DashboardOptions {
    public string DataJsonPath { get; set; } = "data.json";
    public int FileWatchDebounceMs { get; set; } = 500;
}
```

**Configuration Properties**:
- DataJsonPath: Path to data.json file (default: "data.json" in app root, configurable via appsettings)
- FileWatchDebounceMs: Milliseconds to debounce FileSystemWatcher events (default: 500)

**Dependencies**: None

**Lifecycle**: Loaded once at application startup via IOptions<T> pattern from appsettings.json

---

### 8. Program.cs (Startup Configuration)

**Responsibility**: Configure dependency injection, register services, set up Blazor Server.

**Key Registrations**:
```csharp
// Singleton: loaded once at startup, reused across all requests
builder.Services.AddSingleton<DashboardDataService>();

// Configuration binding
builder.Services.Configure<DashboardOptions>(
    builder.Configuration.GetSection("Dashboard"));

// Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
```

**Dependencies**: ASP.NET Core 8.0 built-in DI container

---

## Component Interactions

### Use Case 1: Dashboard Page Loads (Startup)

**Sequence**:
1. Browser requests `/dashboard`
2. ASP.NET Core routes to Dashboard.razor
3. Blazor calls Dashboard.OnInitializedAsync()
4. Dashboard injects DashboardDataService from DI container
5. DashboardDataService.ctor() called (first request only):
   - Loads data.json from disk via File.ReadAllText()
   - Parses JSON into DashboardData model using System.Text.Json
   - Validates model against DataAnnotations constraints
   - Initializes FileSystemWatcher on data.json path
   - Caches DashboardData in-memory
6. Dashboard.OnInitializedAsync() subscribes: `DataService.OnDataChanged += OnDataChanged`
7. Dashboard.razor renders markup:
   - Calls DataService.GetProject() → renders Project Summary Card (MudBlazor)
   - Calls DataService.GetMilestones() → passes to MilestoneTimeline component
   - Calls DataService.GetStatusCounts() → passes to StatusChart component
   - Calls DataService.GetWorkItemsByStatus() → groups, passes to WorkItemList
8. Child components OnParametersSet() called:
   - MilestoneTimeline: Renders SVG timeline
   - StatusChart: Initializes Chart.js via JavaScript interop
   - WorkItemList: Renders three grouped sections
9. **Result**: Executive sees complete dashboard within <2 seconds

---

### Use Case 2: data.json File Changed Externally

**Sequence**:
1. User/external tool modifies data.json on disk (e.g., adds work item, updates milestone)
2. FileSystemWatcher detects file write event
3. DashboardDataService debounce timer (500ms) starts
   - Timer waits for atomic write completion (handles multiple events from single write)
4. Timer fires → DashboardDataService calls ReloadData():
   - Computes file hash to detect stale reads
   - Re-reads data.json from disk via File.ReadAllText()
   - Re-parses JSON into new DashboardData model
   - Validates model against constraints
   - If parse succeeds:
     - Replaces _cachedData with new model
     - Updates _lastLoadedHash
     - Raises OnDataChanged event
   - If parse fails:
     - Logs error with JsonException details
     - Keeps _cachedData from previous load (last-known-good)
     - Sets _lastError for UI display
     - Raises OnDataChanged event with error flag
5. Dashboard.OnDataChanged() handler triggered:
   - Calls StateHasChanged()
6. Blazor Server re-renders Dashboard.razor markup:
   - Calls DataService.GetProject() → renders fresh data
   - Calls DataService.GetMilestones() → passes to MilestoneTimeline
   - Calls DataService.GetStatusCounts() → passes to StatusChart
   - Calls DataService.GetWorkItemsByStatus() → passes to WorkItemList
7. Child components OnParametersSet() called again:
   - MilestoneTimeline: Re-renders with new milestones
   - StatusChart: JavaScript interop updates Chart.js data
   - WorkItemList: Re-renders with new work items
8. **Result**: Dashboard reflects new data within 1 second of file write

---

### Use Case 3: User Views Work Items by Status

**Sequence**:
1. Dashboard.razor calls DataService.GetWorkItemsByStatus(WorkItemStatus.Shipped)
2. DashboardDataService filters cached WorkItems by Status property:
   ```csharp
   return _cachedData.WorkItems
       .Where(w => w.Status == status)
       .ToList()
       .AsReadOnly();
   ```
3. Returns filtered IReadOnlyList<WorkItem> to WorkItemList component
4. WorkItemList.razor receives via [Parameter] and renders `<ul>` with filtered items
5. **Result**: Work items displayed grouped by color-coded sections

---

### Use Case 4: Screenshot Capture for PowerPoint

**Sequence**:
1. Executive views dashboard on Chrome/Edge browser (1920x1080 viewport)
2. Presses F12 (opens DevTools)
3. Uses DevTools screenshot tool (Ctrl+Shift+S or right-click > Capture)
4. Selects Dashboard.razor root div (1200px fixed-width container)
5. Captures viewport screenshot
6. **Result**: Clean PNG with:
   - No horizontal scrollbars
   - Fixed 1200px layout centered
   - Light theme (white background, dark text, blue accents)
   - Professional typography (Segoe UI, 14-16px body)
   - Consistent appearance across Chrome and Edge
   - Ready to paste directly into PowerPoint

---

## Data Model

### Entity Relationships

```
DashboardData (root)
├── Project (1:1)
│   ├── Name (string, required)
│   └── Description (string, optional)
├── Milestones (1:many)
│   ├── Name (string, required)
│   ├── Date (DateTime, required, ISO 8601)
│   └── Status (string, enum: Completed|On Track|At Risk)
└── WorkItems (1:many)
    ├── Title (string, required)
    ├── Status (enum: Shipped|InProgress|CarriedOver)
    └── Assignee (string, optional)
```

### JSON Schema (data.json)

```json
{
  "project": {
    "name": "string (required, max 256 chars)",
    "description": "string (optional, max 1024 chars)"
  },
  "milestones": [
    {
      "name": "string (required, max 256 chars)",
      "date": "ISO 8601 datetime (required)",
      "status": "Completed|On Track|At Risk (required)"
    }
  ],
  "workItems": [
    {
      "title": "string (required, max 512 chars)",
      "status": "Shipped|InProgress|CarriedOver (required)",
      "assignee": "string (optional, max 256 chars)"
    }
  ]
}
```

### Storage Strategy

**Source**: `data.json` file in application root or configured path

**Format**: UTF-8 JSON with optional indentation

**Parsing**:
- System.Text.Json with JsonSerializerOptions:
  - PropertyNameCaseInsensitive: true (allow "ProjectName" or "project")
  - AllowTrailingCommas: false (strict validation)
  - MaxDepth: 64 (prevent DOS attacks)

**Validation**:
- Post-deserialization via System.ComponentModel.DataAnnotations attributes
- Project.Name: [Required], [StringLength(256)]
- Milestone.Name: [Required], [StringLength(256)]
- Milestone.Status: [RegularExpression(@"^(Completed|On Track|At Risk)$")]
- WorkItem.Title: [Required], [StringLength(512)]
- WorkItem.Status: enum enforcement (only 3 valid values)
- WorkItem.Assignee: [StringLength(256)]

**Caching**:
- In-memory DashboardData instance in DashboardDataService singleton
- Replaced on file change (FileSystemWatcher trigger)
- Last-known-good fallback on parse error

**Persistence**:
- File-based only; no database backend
- No automatic backups; external tool responsibility
- File permissions: Read-only (644 Linux, read ACL Windows) recommended

### Data Validation Rules

| Field | Rule | Consequence |
|---|---|---|
| Project.Name | Required, max 256 chars | Parse fails; UI shows error |
| Project.Description | Optional, max 1024 chars | Ignored if missing |
| Milestone.Name | Required, max 256 chars | Parse fails; UI shows error |
| Milestone.Date | Valid ISO 8601 datetime | Parse fails if invalid format |
| Milestone.Status | Enum (Completed\|On Track\|At Risk) | Parse fails if invalid value |
| WorkItem.Title | Required, max 512 chars | Parse fails; UI shows error |
| WorkItem.Status | Enum (Shipped\|InProgress\|CarriedOver) | Parse fails if invalid value |
| WorkItem.Assignee | Optional, max 256 chars | Ignored if missing |
| Milestones list | Empty allowed | Dashboard shows "No milestones" |
| WorkItems list | Empty allowed | Dashboard shows "0 Shipped, 0 In Progress, 0 Carried Over" |
| File encoding | UTF-8 only | Parse fails if non-UTF-8 |

---

## API Contracts

### Service Interfaces

**IDashboardDataService (Primary Interface)**:
```csharp
public interface IDashboardDataService {
    // Event fired when data.json changes
    event Action OnDataChanged;
    
    // Get methods (return cached data)
    DashboardData GetCurrentData();
    Project GetProject();
    IReadOnlyList<Milestone> GetMilestones();
    IReadOnlyList<WorkItem> GetWorkItems();
    IReadOnlyList<WorkItem> GetWorkItemsByStatus(WorkItemStatus status);
    (int Shipped, int InProgress, int CarriedOver) GetStatusCounts();
    
    // Error state
    string GetLastError();
    bool HasData { get; }
}
```

**Method Signatures**:
- `GetCurrentData()`: Returns entire cached DashboardData or null if not loaded
- `GetProject()`: Returns Project from cache, null if not loaded
- `GetMilestones()`: Returns IReadOnlyList sorted by Date ascending, empty if not loaded
- `GetWorkItems()`: Returns IReadOnlyList, empty if not loaded
- `GetWorkItemsByStatus(WorkItemStatus status)`: Returns filtered IReadOnlyList
- `GetStatusCounts()`: Returns tuple (Shipped count, InProgress count, CarriedOver count)
- `GetLastError()`: Returns error message string or null if no error
- `HasData` property: Returns true if data successfully loaded, false if parse failed

### HTTP Endpoints

**Status**: Not required for MVP. Dashboard is single-page Blazor Server (no REST API).

**Future additions** (out-of-scope):
- `GET /api/dashboard/data` → DashboardData
- `GET /api/dashboard/status-counts` → (int, int, int)
- `GET /health` → HealthCheck response

### Error Handling

**Error Codes**:
- `FILE_NOT_FOUND`: data.json missing from configured path
- `JSON_PARSE_ERROR`: JsonException during deserialization (malformed JSON)
- `VALIDATION_ERROR`: Parsed data fails schema validation (e.g., invalid Status enum)
- `IO_ERROR`: IOException reading file (permissions, locked file)
- `UNKNOWN_ERROR`: Unexpected exception

**UI Error Boundary** (Dashboard.razor):
```csharp
@if (!DataService.HasData && !string.IsNullOrEmpty(DataService.GetLastError())) {
    <MudAlert Severity="Severity.Error">
        <strong>Error Loading Dashboard</strong>
        <p>@DataService.GetLastError()</p>
        <p style="font-size: 12px; color: #666;">Check data.json format and try again.</p>
    </MudAlert>
}
```

**Logging** (ILogger<DashboardDataService>):
- INFO: "DashboardDataService initialized, data.json loaded successfully"
- INFO: "data.json change detected, re-parsing..."
- WARN: "data.json validation failed: [field] [reason]"
- ERROR: "Failed to parse data.json: [JsonException message]"
- ERROR: "IOException reading data.json: [IOException message]"

---

## Infrastructure Requirements

### Hosting

**Development Environment**:
- `dotnet run` on developer machine
- Visual Studio 2022 (17.8+) or VS Code + OmniSharp
- .NET 8 SDK required
- HTTP localhost:5000 (default Blazor Server port)
- No HTTPS required for local development

**Production Environment** (Local Machine or LAN):
- .NET 8 Runtime (not SDK) required
- Published binary: `dotnet MyProject.Web.dll`
- Default HTTP port: 5000
- Configurable via appsettings.json Kestrel:Endpoints:Http:Url
- Single instance; no load balancing
- Run as background process via screen, nohup, Windows Task Scheduler, or systemd

### Networking

**Local Only**:
- No internet exposure
- No HTTPS/TLS required (local network assumed trusted)
- Optional: Firewall whitelist if LAN-exposed
  - Restrict port 5000 to trusted subnets only
  - Windows: Windows Defender Firewall rules
  - Linux: iptables or ufw rules
- No DNS, CDN, or reverse proxy needed

### Storage

**data.json**:
- Location: Application root (`bin/Debug/net8.0/data.json` after build)
- Alternative: Configured path via appsettings.json `Dashboard:DataJsonPath`
- Expected size: <1 MB (10+ milestones, 20+ work items typical)
- Maximum supported: <10 MB (5MB safe threshold; test before deploying larger)
- Access: Read-only by app (via FileSystemWatcher), write by external tool
- Permissions: Read-only recommended
  - Linux: `chmod 644 data.json`
  - Windows: Set read-only attribute via File Properties or icacls
- Backup: External tool responsibility; not handled by app

**Logs** (Optional):
- Destination: Console (default, captured by process manager) or file
- Format: Built-in ILogger (plain text) or Serilog (structured JSON)
- Retention: No automatic rollover; manual cleanup acceptable
- Location: Configurable via appsettings.json Logging:File:Path

### Deployment

**Manual Deployment Steps**:
1. `dotnet publish -c Release -o ./publish`
2. Copy `publish/` folder to target machine
3. Copy `data.json` to publish folder root
4. Run: `dotnet MyProject.Web.dll` or create task wrapper

**Project File (.csproj) Configuration**:
```xml
<ItemGroup>
    <None Include="data.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

**appsettings.json** (Kestrel Configuration):
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  },
  "Dashboard": {
    "DataJsonPath": "data.json",
    "FileWatchDebounceMs": 500
  },
  "Logging": {
    "LogLevel": { "Default": "Information" }
  }
}
```

### CI/CD Pipeline

**Build**:
- Tool: Visual Studio or `dotnet build`
- Trigger: On commit to main/develop (optional GitHub Actions workflow)
- Steps:
  1. Restore NuGet packages: `dotnet restore`
  2. Build: `dotnet build -c Release`
  3. Run unit tests: `dotnet test`
  4. Publish: `dotnet publish -c Release -o ./publish`
- Artifacts: MyProject.Web.dll, MyProject.Web.pdb, data.json (sample)

**Test**:
- Framework: xUnit 2.6+, Moq 4.20+
- Coverage: DashboardDataService unit tests (JSON parsing, file watch, error scenarios)
- Command: `dotnet test --verbosity=normal`

**Release**:
- No automated deployment (manual `dotnet publish` + copy)
- Version: Semantic versioning in .csproj (`<Version>1.0.0</Version>`)
- Changelog: Document breaking changes in README.md

### Monitoring

**Not Required** (internal tool, no SLA). Optional additions:
- Console logging to track data.json changes
- Error logging level in appsettings.json
- Manual spot-check after data.json edits
- No health check endpoint, metrics, or alerting required

### Scalability Limits

**Current Design Constraints**:
- Single Blazor Server instance (no multi-server clustering)
- Max ~20 concurrent users
- Blazor Server memory: ~30-50 MB per session
- data.json: <10 MB (5MB safe threshold)
- FileSystemWatcher: Single file only

**Vertical Scaling Path**:
- **2-5 users**: Developer machine (2+ cores, 4GB RAM)
- **5-20 users**: Standard server (4+ cores, 8GB RAM)
- **20+ users**: Not supported; requires database backend + multi-server redesign

### Dependencies (NuGet Packages)

**Required**:
- MudBlazor v7.0+ (MIT)
- ChartJS.Blazor v4.1.2 (MIT)
- System.Text.Json (built-in .NET 8)

**Testing**:
- xUnit v2.6.0+ (Apache 2.0)
- Moq v4.20.0+ (BSD 3-Clause)
- Microsoft.NET.Test.Sdk (MIT)

**NOT Included** (by design):
- No authentication (OAuth2, JWT, SAML)
- No database (EF Core, Dapper, SQLite)
- No state management (MediatR, Fluxor)
- No cloud SDKs (Azure, AWS, GCP)

---

## Technology Stack Decisions

### Decision 1: UI Component Framework → MudBlazor v7.0+

**Chosen**: MudBlazor v7.0+ (Free, MIT licensed, Blazor-native)

**Alternatives**: Bootstrap 5 (lighter, more flexible) | Telerik/Syncfusion (commercial, full-featured)

**Justification**:
- Blazor-native components match .NET 8 stack exactly
- Free tier eliminates licensing friction
- Sufficient for dashboard UI (cards, grids, icons, typography)
- Active maintenance and strong community support
- CSS-in-JS eliminates separate stylesheet overhead

**Trade-offs**:
- MudBlazor: Opinionated, 2-day learning curve, smaller ecosystem than Bootstrap
- Bootstrap: More flexible, but requires custom C# styling logic
- Telerik/Syncfusion: Over-engineered for simple dashboard; licensing costs

---

### Decision 2: Data Storage → File-based (data.json) + FileSystemWatcher

**Chosen**: data.json + System.IO.FileSystemWatcher

**Alternatives**: SQLite database (adds persistence layer) | External API/Cloud (violates local-only constraint)

**Justification**:
- Aligns with PM spec requirement ("update by editing JSON file without restart")
- Zero infrastructure overhead
- Leverages built-in .NET System.IO.FileSystemWatcher
- 500ms debounce handles atomic file writes

**Trade-offs**:
- data.json: Simple, human-readable, but file-locking requires retry logic
- SQLite: More structured, but adds complexity for read-only snapshot dashboard
- External: Breaks local-only deployment requirement

---

### Decision 3: JSON Parsing → System.Text.Json

**Chosen**: System.Text.Json (Built-in .NET 8, fast, zero dependencies)

**Alternatives**: Newtonsoft.Json/Json.NET (more flexible, external dependency)

**Justification**:
- Native to .NET 8; no external dependency
- Superior performance (10ms parse for typical 1MB file)
- Minimal memory footprint
- Sufficient for straightforward JSON schema

**Trade-offs**:
- System.Text.Json: Slightly less flexible, but faster
- Json.NET: Slower, adds NuGet dependency, more edge-case support

---

### Decision 4: Status Chart → ChartJS.Blazor v4.1.2

**Chosen**: ChartJS.Blazor v4.1.2 (JavaScript interop wrapper for Chart.js)

**Alternatives**: OxyPlot v2.1.2 (C# native, static) | Custom Canvas/SVG (lightweight, full control)

**Justification**:
- Professional bar/pie charts for status visualization
- Well-supported in Blazor ecosystem (800+ GitHub stars)
- Interactive charts suitable for executive dashboards
- Minimal configuration required

**Trade-offs**:
- ChartJS.Blazor: Requires JavaScript interop, but web-native
- OxyPlot: Pure C#, but static PNG output less suitable for web
- Custom SVG: Lightweight, but requires custom rendering logic

---

### Decision 5: Milestone Timeline → Custom HTML/CSS/SVG

**Chosen**: Custom HTML/CSS/SVG (Lightweight, maximum control)

**Alternatives**: Syncfusion Gantt (full-featured, commercial) | Third-party Gantt library (bloat, licensing)

**Justification**:
- Milestones are static, chronological list only
- Custom SVG provides maximum control over visual design for screenshots
- Zero external licensing overhead
- Minimal code (20-30 lines HTML/CSS)

**Trade-offs**:
- Custom SVG: Requires custom layout logic, but lightweight
- Gantt library: Full-featured, but overkill and commercial

---

### Decision 6: State Management → Singleton Service + Component-Level @code

**Chosen**: Singleton service (DashboardDataService) + component-level @code blocks

**Alternatives**: Component-level @code only (simpler) | MediatR/Fluxor (more structure)

**Justification**:
- Singleton service loads/watches data.json once, caches across requests
- Event-driven updates (OnDataChanged) allow reactive components without tight coupling
- Balances simplicity with reusability
- Enables US-3 requirement (auto-refresh on file change)

**Trade-offs**:
- Singleton + @code: Slightly more structure, but scalable
- @code only: Minimal boilerplate, but redundant data loading
- MediatR/Fluxor: Complex patterns, unnecessary for simple dashboard

---

### Decision 7: Rendering Model → Hybrid Static-Dynamic

**Chosen**: Hybrid: Static HTML wrapped in Blazor component

**Alternatives**: Pure static HTML (simplest, no reactivity) | Pure dynamic Blazor (fully reactive, component complexity)

**Justification**:
- Static HTML renders instantly, photographs cleanly for PowerPoint
- Blazor wrapper enables file-watch reactivity (OnDataChanged → StateHasChanged)
- Combines simplicity with US-3 requirement (<1 second file-change-to-render)

**Trade-offs**:
- Hybrid: Best of both worlds
- Pure static: No real-time updates without manual refresh
- Pure dynamic: Component lifecycle complexity, potential rendering delays

---

### Decision 8: Page Layout → Fixed-Width 1200px, Desktop-Only

**Chosen**: Fixed-width 1200px, optimized for 1920x1080 desktop resolution

**Alternatives**: Responsive layout (mobile-first) | Fluid width (100% viewport)

**Justification**:
- Directly supports US-4 requirement (PowerPoint-ready screenshots, consistent appearance)
- No horizontal scrollbars on 1920x1080 viewport
- Eliminates responsive CSS overhead
- Aligns with "executive-focused internal tool" use case

**Trade-offs**:
- Fixed 1200px: Predictable screenshots, professional appearance
- Responsive: Adaptable, but adds CSS complexity, inconsistent screenshots
- Fluid: Flexible, but layout shifts with viewport size

---

### Decision 9: Project Structure → Layered .sln (Three Projects)

**Chosen**: MyProject.Web | MyProject.Core | MyProject.Tests

**Alternatives**: Single project (simplest) | Monolithic (fast prototype)

**Justification**:
- Separates concerns: UI (Web), business logic (Core), testing (Tests)
- Enables reuse of Models/Services if dashboard expands
- Supports requirement to "unit test DashboardDataService"
- Scales from prototype to maintenance without refactoring

**Trade-offs**:
- Three projects: More structure, but adds build complexity
- Single project: Simpler initially, but harder to scale
- Monolithic: Fastest prototyping, harder to maintain

---

### Decision 10: Deployment → Standalone .NET 8 Runtime

**Chosen**: Published binary (`dotnet MyProject.Web.dll`)

**Alternatives**: `dotnet run` (dev only) | Docker container (portable, adds complexity) | Windows Service (background process)

**Justification**:
- Requires only .NET 8 runtime (not SDK)
- Portable to any Windows/Linux/macOS machine
- Simpler than Docker for local deployment
- Supports both dev (`dotnet run`) and production (`published binary`) workflows

**Trade-offs**:
- Standalone: Lightweight, portable
- dotnet run: Requires .NET 8 SDK
- Docker: Maximum portability, adds Dockerfile complexity
- Windows Service: Persistent background process, Windows-specific

---

### Decision 11: Error Handling → Try-Catch + User-Friendly Fallback UI

**Chosen**: Try-catch in DashboardDataService + error boundary in Dashboard.razor

**Alternatives**: Strict schema validation (fails loudly) | Silent degradation (hides problems)

**Justification**:
- Catches JsonException and IOException
- Logs detailed error for debugging
- Displays clear message to user ("data.json is invalid; check format")
- Supports US-2 acceptance criteria (missing/malformed data displays friendly error)
- Falls back to last-known-good cached data or empty dashboard

**Trade-offs**:
- Try-catch + fallback: User-friendly, informative
- Strict validation: Forces correct data.json, but stops app
- Silent degradation: App stays running, but users don't know why data is missing

---

### Decision 12: Testing Strategy → xUnit Unit Tests (DashboardDataService)

**Chosen**: xUnit 2.6+ + Moq 4.20+ (unit tests for DashboardDataService only)

**Alternatives**: No tests (fastest initially, high risk) | Full integration tests (comprehensive, slow CI/CD)

**Justification**:
- Covers critical paths: JSON parsing (happy + malformed), file watch debouncing, error scenarios
- Moq library mocks FileSystemWatcher events
- Standard .NET testing stack
- Balances coverage with simplicity (MVP scope)

**Trade-offs**:
- Unit tests: Covers critical paths, xUnit standard
- No tests: Fast initial dev, high regression risk
- Integration tests: Comprehensive, but requires fixtures, slow CI/CD

---

## Security Considerations

### Authentication & Authorization

**Status**: NOT REQUIRED (Internal-Only Tool)

**Design Decision**:
- No login required
- No role-based access control
- All users see all data (executives, project managers)
- Single operator maintains data.json

**Rationale**: PM spec explicitly excludes authentication/authorization; internal deployment only; non-sensitive project metadata

**Optional Hardening** (if exposed to untrusted network):
- Restrict port 5000 via firewall to trusted subnets only
- Deploy behind HTTP reverse proxy (nginx) with basic auth (out-of-scope)
- Never expose to internet without HTTPS + authentication

---

### Data Protection

**At Rest** (data.json):
- No encryption required (non-sensitive metadata: milestone names, work item titles, assignees)
- Set file permissions: Read-only (644 Linux, read ACL Windows)
- No PII, secrets, or credentials in data.json
- No password hashing or token storage

**In Transit**:
- No HTTPS required (local network only)
- No external API calls; all data stays local
- Blazor Server SignalR over HTTP acceptable for trusted LAN

**In Memory**:
- DashboardDataService caches DashboardData in-memory (unencrypted)
- Acceptable: internal tool, trusted operators, non-sensitive data
- Memory cleared on app restart

---

### Input Validation

**data.json Parsing**:
```csharp
var options = new JsonSerializerOptions {
    PropertyNameCaseInsensitive = true,
    AllowTrailingCommas = false,
    MaxDepth = 64  // Prevent DOS attacks
};

var data = JsonSerializer.Deserialize<DashboardData>(json, options);

// Post-deserialization validation
if (data?.Project == null)
    throw new ValidationException("Project is required");
if (data.Milestones?.Any(m => string.IsNullOrEmpty(m.Name)) == true)
    throw new ValidationException("Milestone name required");
if (data.WorkItems?.Any(w => !Enum.IsDefined(w.Status)) == true)
    throw new ValidationException("Invalid WorkItem status");
```

**Field Length Enforcement**:
- Project.Name: Max 256 chars (prevent UI rendering overflow)
- Milestone.Name: Max 256 chars
- WorkItem.Title: Max 512 chars
- Assignee: Max 256 chars

**Type Enforcement**:
- WorkItemStatus enum (only 3 values: Shipped, InProgress, CarriedOver)
- Milestone.Date must be valid DateTime
- No SQL/JavaScript injection possible (JSON file, not dynamic queries)

---

### File System Security

**FileSystemWatcher Race Condition Prevention**:
```csharp
private string _lastLoadedHash;

private void ReloadData() {
    var fileHash = HashFile(_dataJsonPath);
    if (fileHash == _lastLoadedHash) {
        _logger.LogDebug("File hash unchanged; skipping reload");
        return;
    }
    
    var newData = LoadFromJson(File.ReadAllText(_dataJsonPath));
    _cachedData = newData;
    _lastLoadedHash = fileHash;
    OnDataChanged?.Invoke();
}
```

**Mitigation**:
- 500ms debounce prevents symlink DOS attacks (repeated writes)
- Hash check avoids redundant parsing

**Path Traversal Prevention**:
- data.json path resolved via `Path.Combine(AppContext.BaseDirectory, "data.json")`
- No user-supplied path parameters
- Prevents `../../../etc/passwd` attacks

---

## Scaling Strategy

### Vertical Scaling (Single Machine)

**Current Design**: Single Blazor Server instance, no horizontal scaling

**Scaling Limits**:

| Metric | Limit | Mitigation |
|---|---|---|
| Concurrent Users | ~20 (Blazor ~30-50MB per session) | Upgrade hardware; add RAM/CPU |
| data.json Size | <10 MB (5MB safe) | Split files if exceeds; implement pagination |
| Chart.js Rendering | <1000 data points | Pre-aggregate by category |
| FileSystemWatcher Events | Single file, 500ms debounce | No changes needed |

**Vertical Scaling Path**:
- **2-5 users**: Developer machine (2+ cores, 4GB RAM)
- **5-20 users**: Standard server (4+ cores, 8GB RAM)
- **20+ users**: Not supported; database backend + multi-server required

### Caching Strategy

**Current**: In-memory singleton cache (no distributed cache)

**Cache Lifecycle**:
- On startup: DashboardDataService loads data.json
- Cached in-memory: _cachedData field
- Cache invalidation: FileSystemWatcher detects change → reload from disk
- No TTL or background refresh (data updates infrequent: daily/weekly)

**Cache Efficiency**:
- Single load per file change (debounced)
- No redundant parsing (hash check)
- Read-only access prevents stale writes
- No cache coherence issues (single instance)

**No Caching Required For**:
- UI rendering (Blazor Server handles)
- Chart.js (re-rendered on data change only)
- Milestone/work item lists (<1000 items, negligible overhead)

---

### Load Balancing

**Not Applicable**: Single-machine deployment only

**If Future Multi-Server Scaling Needed**:
- Cannot use file-based data.json (no shared filesystem without distributed locks)
- Redesign: Replace data.json with SQLite/PostgreSQL database
- Session affinity in load balancer (each user → same Blazor instance)
- Add Redis for distributed caching (>50 users)

---

### Bottleneck Analysis & Mitigation

| Bottleneck | Current Status | Mitigation |
|---|---|---|
| FileSystemWatcher latency | 500ms debounce + <100ms parse + <100ms render = ~700ms total (<1s acceptable) | None needed; acceptable for infrequent updates |
| JSON parsing time | ~10ms for typical 1MB file | Benchmark with real data; acceptable up to 5MB |
| Chart.js interop | ~200-500ms JavaScript init | Cache Chart.js instance; update data only, no re-init |
| Blazor Server memory | ~50MB per session | Monitor with dotnet trace; limit to <20 concurrent |
| Network latency | Minimal (local LAN) | No CDN or optimization needed |

---

## Risks & Mitigations

### Risk Priority Matrix

| Risk | Impact | Likelihood | Priority | Mitigation Effort |
|---|---|---|---|---|
| **FileSystemWatcher race condition** | High (duplicate parse, stale cache) | High (atomic writes trigger multiple events) | **CRITICAL** | Low (hash check + 500ms debounce) |
| **Malformed data.json crash** | Medium (dashboard shows error) | Medium (user editing error) | **HIGH** | Low (try-catch + fallback UI) |
| **File lock contention** | Low (file locked briefly) | Medium (concurrent write) | Medium | Low (retry logic) |
| **Large data.json slowness** | Medium (UI lag) | Low (realistic <1MB) | Medium | Medium (split files or pagination) |
| **ChartJS.Blazor incompatibility** | Medium (chart breaks) | Low (stable v4.1.2) | Medium | Low (version pin) |
| **MudBlazor breaking change** | Low (layout only) | Low (v7.0 stable) | Low | Low (CSS override) |
| **.NET 8 runtime deprecation** | Low (long runway) | Very Low (LTS until Nov 2026) | Low | High (future migration) |
| **data.json accidental deletion** | Medium (no data) | Low (file permissions help) | Medium | Low (read-only perms) |
| **User editing errors** | Medium (validation error) | Medium (manual editing) | **HIGH** | Low (docs + validation) |
| **Concurrent edit corruption** | Medium (parse error) | Low (single operator assumption) | Medium | Medium (file locking or DB) |

---

### Technical Risk: FileSystemWatcher Race Condition (CRITICAL)

**Scenario**: Atomic file write triggers multiple FileSystemWatcher events (Windows behavior); data parsed twice.

**Impact**: High (potential stale cache, wasted resources)

**Mitigation**:
```csharp
// 500ms debounce + hash check prevents duplicate parsing
private string _lastLoadedHash;
private void ReloadData() {
    var fileHash = HashFile(_dataJsonPath);
    if (fileHash == _lastLoadedHash) {
        _logger.LogDebug("File hash unchanged; skipping reload");
        return;
    }
    // Proceed with reload
}
```

**Implementation**: Hash-based duplicate detection + 500ms debounce timer

---

### Technical Risk: Malformed data.json Crash (HIGH)

**Scenario**: User edits data.json with invalid JSON (missing comma, syntax error).

**Impact**: Medium (dashboard shows error, not a crash)

**Mitigation**:
```csharp
public bool TryLoadData(out string errorMessage) {
    try {
        var data = JsonSerializer.Deserialize<DashboardData>(json, options);
        _cachedData = data;
        _lastError = null;
        OnDataChanged?.Invoke();
        errorMessage = null;
        return true;
    } catch (JsonException ex) {
        _lastError = $"JSON syntax error in data.json: {ex.Message}";
        _logger.LogError(_lastError);
        errorMessage = _lastError;
        return false;  // Return last-known-good data
    }
}
```

**UI Error Boundary** (Dashboard.razor):
```csharp
@if (!DataService.HasData) {
    <MudAlert Severity="Severity.Error">
        <strong>Error Loading Dashboard</strong>
        <p>@DataService.GetLastError()</p>
    </MudAlert>
}
```

**Implementation**: Try-catch + user-friendly error message + fallback to last-known-good data

---

### Technical Risk: File Lock Contention (MEDIUM)

**Scenario**: External tool locks data.json during write while app tries to read.

**Impact**: Low (brief file lock, FileSystemWatcher event fires after write completes)

**Mitigation**:
```csharp
private string ReadFileWithRetry(string path, int maxRetries = 3) {
    for (int i = 0; i < maxRetries; i++) {
        try {
            return File.ReadAllText(path, Encoding.UTF8);
        } catch (IOException ex) when (i < maxRetries - 1) {
            _logger.LogWarning($"File locked; retrying in 100ms ({i + 1}/{maxRetries})");
            Thread.Sleep(100);
        }
    }
    throw new IOException($"Could not read {path} after {maxRetries} retries");
}
```

**Implementation**: Exponential backoff retry logic (3 retries, 100ms between attempts)

---

### Technical Risk: Large data.json Performance Degradation (MEDIUM)

**Scenario**: data.json grows to 10MB+ (many milestones/work items).

**Impact**: Medium (JSON parsing takes 100ms+, UI update lag)

**Mitigation**:
- **Option A** (Recommended): Benchmark with realistic data (5MB safe threshold; test before deploying)
- **Option B**: Split into multiple files (data.json for current, data.archive.json for history)
- **Option C**: Implement incremental parsing (only re-parse changed sections)

**Implementation**: Start with Option A; optimize if bottleneck proven

---

### Dependency Risk: ChartJS.Blazor Version Incompatibility (MEDIUM)

**Scenario**: ChartJS.Blazor v4.1.2 has breaking change in v5.0; JavaScript interop breaks.

**Impact**: Medium (chart doesn't render; dashboard still functional without chart)

**Mitigation**:
- Pin NuGet version in .csproj: `<PackageReference Include="ChartJS.Blazor" Version="4.1.2" />`
- Test upgrade path before adopting v5.0
- Fallback: Render static bar chart via HTML/CSS if ChartJS unavailable

**Implementation**: Version pinning + upgrade testing before deployment

---

### Dependency Risk: MudBlazor CSS/JavaScript Breaking Change (LOW)

**Scenario**: MudBlazor v8.0 changes Card layout; breaks 1200px fixed-width design.

**Impact**: Low (layout only; functionality preserved)

**Mitigation**:
- Pin MudBlazor version: `<PackageReference Include="MudBlazor" Version="7.0.0" />`
- Test layout on version upgrade
- Custom CSS overrides pinned components (force Container max-width: 1200px)

**Implementation**: Version pinning + CSS overrides

---

### Dependency Risk: .NET 8 Runtime Deprecation (LOW)

**Scenario**: .NET 8 LTS support ends (Nov 2026); migration to .NET 9+ required.

**Impact**: Low (long runway; not immediate concern)

**Mitigation**:
- Plan migration when LTS support approaches end-of-life
- Use no obsolete APIs (standard LINQ, async/await)
- Document breaking changes in README.md if .NET version bump required

**Implementation**: Future maintenance task; track .NET LTS roadmap

---

### Operational Risk: data.json Accidental Deletion (MEDIUM)

**Scenario**: User accidentally deletes data.json file.

**Impact**: Medium (dashboard shows "No data loaded"; restart reloads if backup exists)

**Mitigation**:
- Set file permissions: Read-only (644 Linux, read-only ACL Windows)
- Document backup strategy in README.md (external tool responsibility)
- App gracefully handles FileNotFound exception; shows error UI

**Implementation**: File permissions + documentation

---

### Operational Risk: Incorrect data.json Edits (HIGH)

**Scenario**: Project manager manually edits data.json; introduces invalid dates or status values.

**Impact**: Medium (validation error; dashboard shows error message)

**Mitigation**:
- Provide data.json schema documentation with examples
- Include sample data.json with realistic fictional data
- Log validation errors with clear messages
  - Example: "WorkItem[5] has invalid status 'In-Progress' (use 'InProgress')"
- Implement JSON schema validation via external tool (optional, out-of-scope)

**Implementation**: Documentation + validation logging

---

### Operational Risk: Concurrent data.json Edits (MEDIUM)

**Scenario**: Project manager edits data.json while another user views dashboard; file becomes corrupted due to concurrent write.

**Impact**: Medium (parse error; last-known-good cached data shown)

**Mitigation**:
- Assumption: Single operator maintains data.json (per PM spec)
- If multi-user needed: Add file-based lock (check data.json.lock exists before write)
- Better solution: Migrate to database with transaction support (out-of-scope for MVP)

**Implementation**: Document single-operator assumption; plan DB migration if needed

---

### Implementation Priorities

**Must Implement** (MVP):
1. Hash-based duplicate parsing prevention (Risk 1: FileSystemWatcher race condition)
2. Try-catch + fallback UI for parse errors (Risk 2: Malformed data.json crash)
3. Version pinning for critical NuGet packages (Risk 5-6: ChartJS/MudBlazor incompatibility)
4. Clear error messages in UI (Risk 9: User editing errors)
5. File permission recommendations in README (Risk 8: data.json deletion)

**Nice-to-Have** (Future):
1. File read retry logic (Risk 3: File lock contention)
2. File-locking mechanism for concurrent edits (Risk 10: Concurrent edit corruption)
3. JSON schema validation tooling (Risk 9: User editing errors)
4. Data.json size benchmarking (Risk 4: Performance degradation)

**Out-of-Scope** (MVP):
1. Database migration (Risk 10 full solution; requires redesign)
2. Multi-server clustering (beyond single-machine design)
3. Automated screenshot generation (not needed; manual F12 sufficient)