# Architecture

## Overview & Goals

The Executive Project Dashboard is a single-page Blazor Server web application that enables executives to visualize project milestones, progress, and work status at a glance. The dashboard reads all data from a JSON configuration file (data.json) and renders as a screenshot-optimized view designed for PowerPoint embedding.

**Key Architectural Goals:**
- Enable executives to assess project health via deterministic, reproducible screenshots
- Provide visibility into shipped features, in-progress work, and carryover items
- Simplify project status reporting by eliminating manual PowerPoint slide updates
- Deliver zero cloud dependencies and zero installation burden on stakeholder machines
- Establish a scalable foundation for future multi-project dashboards (Phase 2)

**Core Principles:**
- Single Responsibility: Each component has one reason to change
- Local-First: All processing occurs on the local machine; no cloud exposure
- Deterministic Rendering: Server-side rendering ensures consistent output across machines and browsers
- Minimal Dependencies: Only use built-in .NET 8 libraries and MudBlazor 6.10.2
- File-Based Configuration: JSON storage (data.json) enables stakeholder updates without code changes

---

## System Components

### Service Layer

#### 1. DataConfigurationService
**Responsibility:** Load, deserialize, and validate project data from wwwroot/data.json.

**Public Interface:**
```csharp
public class DataConfigurationService
{
    public async Task<ProjectData> LoadConfigurationAsync(string filePath);
    public ValidationResult ValidateProjectData(ProjectData data);
    public bool FileExists(string filePath);
}
```

**Key Responsibilities:**
- Read data.json asynchronously from file system
- Deserialize JSON to strongly-typed ProjectData using System.Text.Json
- Validate schema against FluentValidation rules at startup
- Throw typed exceptions (FileNotFoundException, JsonException, ConfigurationException) with clear error messages

**Dependencies:**
- System.Text.Json (built-in)
- FluentValidation 11.9.x
- System.IO.File (built-in)

**Data Owned:** None (stateless service)

---

#### 2. ProjectDashboardState
**Responsibility:** Hold in-memory application state, manage project data lifecycle, and coordinate refresh operations.

**Public Interface:**
```csharp
public class ProjectDashboardState : IAsyncDisposable
{
    public ProjectData? CurrentProject { get; private set; }
    public bool IsLoaded { get; private set; }
    public string? LoadError { get; private set; }
    
    public async Task InitializeAsync();
    public async Task RefreshDataAsync();
    public void SetRefreshInterval(TimeSpan interval);
    public event Action? OnStateChanged;
}
```

**Key Responsibilities:**
- Load data.json on initialization via DataConfigurationService
- Handle missing/malformed files gracefully with clear error messages
- Implement optional 5-minute refresh timer for external data updates
- Notify child components of state changes via OnStateChanged event
- Manage timer disposal on component cleanup

**Dependencies:**
- DataConfigurationService (injected)
- System.Timers.Timer
- ProjectData model

**Data Owned:**
- CurrentProject (ProjectData instance)
- IsLoaded state flag
- LoadError message
- Refresh timer state

**Registration:** Scoped lifetime in DI container (one instance per Blazor Server connection)

---

### Component Layer

#### 3. Dashboard.razor (Parent Component)
**Responsibility:** Orchestrate single-page layout, load project state, and cascade data to child components.

**Public Parameters:** None (root component)

**Injected Services:**
- ProjectDashboardState (scoped service)

**Key Responsibilities:**
- Call DashboardState.InitializeAsync() in OnInitializedAsync()
- Set optional 5-minute refresh timer
- Display error UI if LoadError is not null
- Cascade ProjectDashboardState to all child components
- Structure fixed-width (1200px) single-page layout
- Subscribe/unsubscribe to OnStateChanged for re-rendering

**Child Components:**
- MilestoneTimeline
- ProgressBoard
- MetricsPanel

---

#### 4. MilestoneTimeline.razor
**Responsibility:** Display horizontal timeline visualization of milestones with dates, status badges, and completion percentages.

**Cascading Parameters:**
- ProjectData Data

**Key Responsibilities:**
- Render 5-10 milestones horizontally using MudBlazor.MudTimeline
- Display milestone name, target date, status badge (Completed/In Progress/Not Started), and completion % progress bar
- Color-code status: green (Completed), yellow (In Progress), gray (Not Started)
- Ensure all milestones fit single viewport without pagination
- Apply CSS print queries to preserve layout on Ctrl+P export

**Dependencies:**
- MudBlazor.MudTimeline
- MudBlazor.MudCard
- MudBlazor.MudProgressLinear

---

#### 5. ProgressBoard.razor
**Responsibility:** Display three-column progress layout (Shipped/In Progress/Carried Over) with item counts.

**Cascading Parameters:**
- ProjectData Data

**Key Responsibilities:**
- Create three-column MudBlazor.MudGrid layout
- Pass Shipped/InProgress/CarriedOver items to ProgressColumn component
- Display column header with count totals (e.g., "Shipped: 30")
- Ensure 20-30 work items render without scrolling (fixed layout, ~33% width per column)
- Apply CSS for consistent column alignment

**Child Components:**
- ProgressColumn (×3)

---

#### 6. ProgressColumn.razor
**Responsibility:** Render single progress column with title, item list, and count badge.

**Parameters:**
- string ColumnTitle ("Shipped" | "In Progress" | "Carried Over")
- List<ProgressItem> Items
- string StatusColor ("success" | "warning" | "error")

**Key Responsibilities:**
- Render MudCard with header containing title and count badge
- Display each work item: title, assignee, and (for carried-over) blocking reason
- Show empty-state message if Items.Count == 0
- Fit items within fixed column width without horizontal overflow

**Dependencies:**
- MudBlazor.MudCard
- MudBlazor.MudChip

---

#### 7. MetricsPanel.razor
**Responsibility:** Display executive summary KPIs (counts and percentages).

**Cascading Parameters:**
- ProjectData Data

**Key Responsibilities:**
- Display four metric cards: Total Items, Shipped, In Progress, Carried Over
- Calculate and display percentages auto-derived from MetricsData properties
- Format numbers with clear labels and visual hierarchy (large bold numbers, smaller labels)
- Use MudBlazor.MudGrid for responsive card layout

**Dependencies:**
- MudBlazor.MudGrid
- MudBlazor.MudCard
- MudBlazor.MudText

---

### Model Layer

#### 8. ProjectData (Root Aggregate)
```csharp
public class ProjectData
{
    [Required]
    public ProjectInfo Project { get; set; }
    
    [Required]
    public List<Milestone> Milestones { get; set; } = new();
    
    [Required]
    public ProgressData Progress { get; set; }
    
    [Required]
    public MetricsData Metrics { get; set; }
}
```

**Responsibility:** Container for all project configuration; strongly-typed representation of data.json structure.

---

#### 9. ProjectInfo
```csharp
public class ProjectInfo
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Project owner is required")]
    [StringLength(100, MinimumLength = 1)]
    public string Owner { get; set; }
    
    [Required(ErrorMessage = "Project status is required")]
    [RegularExpression(@"^(On Track|At Risk|Blocked)$")]
    public string Status { get; set; }
    
    [Required]
    public DateTime LastUpdated { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
}
```

**Responsibility:** Represent project metadata (name, owner, status, timestamp, description).

---

#### 10. Milestone
```csharp
public class Milestone
{
    [Required(ErrorMessage = "Milestone ID is required")]
    [StringLength(50, MinimumLength = 1)]
    public string Id { get; set; }
    
    [Required(ErrorMessage = "Milestone name is required")]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; }
    
    [Required]
    public DateTime TargetDate { get; set; }
    
    [Required(ErrorMessage = "Milestone status is required")]
    [RegularExpression(@"^(Completed|In Progress|Not Started)$")]
    public string Status { get; set; }
    
    [Required]
    [Range(0, 100)]
    public int PercentComplete { get; set; }
}
```

**Responsibility:** Represent single milestone with target date and completion status (0-100%).

---

#### 11. ProgressData
```csharp
public class ProgressData
{
    [Required]
    public List<ProgressItem> Shipped { get; set; } = new();
    
    [Required]
    public List<ProgressItem> InProgress { get; set; } = new();
    
    [Required]
    public List<ProgressItem> CarriedOver { get; set; } = new();
}
```

**Responsibility:** Organize work items by status columns (shipped, in-progress, carried-over).

---

#### 12. ProgressItem
```csharp
public class ProgressItem
{
    [Required(ErrorMessage = "Item ID is required")]
    [StringLength(50, MinimumLength = 1)]
    public string Id { get; set; }
    
    [Required(ErrorMessage = "Item title is required")]
    [StringLength(300, MinimumLength = 1)]
    public string Title { get; set; }
    
    [StringLength(100)]
    public string? Assignee { get; set; }
    
    [StringLength(300)]
    public string? Reason { get; set; } // For carried-over items only
}
```

**Responsibility:** Represent single work item (shipped/in-progress/carried-over).

---

#### 13. MetricsData
```csharp
public class MetricsData
{
    [Required]
    [Range(0, 1000)]
    public int TotalItems { get; set; }
    
    [Required]
    [Range(0, 1000)]
    public int ShippedCount { get; set; }
    
    [Required]
    [Range(0, 1000)]
    public int InProgressCount { get; set; }
    
    [Required]
    [Range(0, 1000)]
    public int CarriedOverCount { get; set; }
    
    public decimal ShippedPercentage => 
        TotalItems > 0 ? Math.Round((ShippedCount * 100m) / TotalItems, 2) : 0;
    
    public decimal InProgressPercentage => 
        TotalItems > 0 ? Math.Round((InProgressCount * 100m) / TotalItems, 2) : 0;
    
    public decimal CarriedOverPercentage => 
        TotalItems > 0 ? Math.Round((CarriedOverCount * 100m) / TotalItems, 2) : 0;
}
```

**Responsibility:** Store aggregate KPI counts and provide computed percentage properties.

---

## Component Interactions

### Primary Use Case: Dashboard Load

```
1. Application Startup
   └─ Program.cs DI Setup
      ├─ Register MudBlazor services
      ├─ Register DataConfigurationService
      ├─ Register ProjectDashboardState (Scoped)
      └─ Configure Kestrel HTTPS on localhost:5001

2. Dashboard.razor OnInitializedAsync()
   └─ Inject ProjectDashboardState
      └─ Call State.InitializeAsync()
         ├─ DataConfigurationService.LoadConfigurationAsync("wwwroot/data.json")
         │  ├─ Read file from disk
         │  ├─ System.Text.Json.Deserialize<ProjectData>()
         │  └─ Return deserialized object
         ├─ Validate via FluentValidation
         │  ├─ Check required fields
         │  ├─ Validate field lengths and ranges
         │  └─ Ensure 5-10 milestones, <50 total work items
         ├─ If validation fails:
         │  ├─ Catch exception
         │  ├─ Set State.LoadError = detailed error message
         │  └─ Return
         └─ If success:
            ├─ Set State.CurrentProject = ProjectData
            ├─ Set State.IsLoaded = true
            ├─ Invoke State.OnStateChanged event
            └─ Start optional 5-minute refresh timer

3. Dashboard.razor Render
   ├─ If State.LoadError != null:
   │  └─ Display error UI (red alert box with actionable message)
   ├─ Else if State.IsLoaded == false:
   │  └─ Display loading spinner
   └─ Else (success):
      ├─ Cascade State to all children
      └─ Render fixed-width (1200px) layout:
         ├─ Header section (Project name, status, last updated)
         ├─ MilestoneTimeline component
         │  └─ Render milestones[] via MudTimeline
         ├─ ProgressBoard component
         │  └─ Render three ProgressColumn components
         │     └─ Render ProgressItem[] with title, assignee, reason
         └─ MetricsPanel component
            └─ Display metrics + computed percentages

4. Browser Renders Single-Page View
   ├─ Apply fixed-width CSS (1200px)
   ├─ Timeline at top (horizontal, no wrapping)
   ├─ Progress board in middle (three columns, ~33% width each)
   ├─ Metrics at bottom
   └─ All visible in single viewport (no scrolling)

5. Screenshot Workflow
   ├─ User presses Ctrl+P (Print to PDF)
   ├─ Browser applies @media print CSS queries
   ├─ Hide chrome, set fixed widths, optimize whitespace
   ├─ Generate single-page PDF
   └─ User pastes screenshot into PowerPoint
```

### Secondary Use Case: Data Refresh During Presentation

```
1. Optional 5-Minute Timer (if enabled)
   └─ Timer event fires
      └─ Call State.RefreshDataAsync()
         ├─ DataConfigurationService.LoadConfigurationAsync()
         ├─ Validate new ProjectData
         └─ If success:
            ├─ Update State.CurrentProject = new data
            ├─ Invoke State.OnStateChanged event
            └─ Child components re-render with new data
```

### Data Flow: Component Dependencies

```
Program.cs
├─ Blazor Framework (MudBlazor, System.Text.Json, FluentValidation)
├─ DataConfigurationService
│  ├─ System.Text.Json
│  ├─ FluentValidation
│  └─ System.IO.File
├─ ProjectDashboardState (Scoped)
│  ├─ DataConfigurationService (injected)
│  ├─ System.Timers.Timer
│  └─ ProjectData model
└─ Dashboard.razor (root)
   ├─ ProjectDashboardState (injected + cascaded)
   ├─ MilestoneTimeline.razor
   │  ├─ ProjectData (cascaded)
   │  └─ MudBlazor.MudTimeline
   ├─ ProgressBoard.razor
   │  ├─ ProjectData (cascaded)
   │  ├─ ProgressColumn.razor (×3)
   │  │  ├─ ProgressItem[] (parameter)
   │  │  └─ MudBlazor components
   │  └─ MudBlazor.MudGrid
   └─ MetricsPanel.razor
      ├─ ProjectData (cascaded)
      └─ MudBlazor components
```

---

## Data Model

### Storage Strategy

**File Location:** `wwwroot/data.json` (relative to application root)

**File Format:** UTF-8 encoded JSON

**Serialization:** System.Text.Json with default settings

**Reload Mechanism:**
- Initial load: `OnInitializedAsync()` in Dashboard.razor
- Optional refresh: 5-minute timer via `System.Timers.Timer`
- Manual refresh: User-triggered via button (Phase 2 enhancement)

### Example data.json Structure

```json
{
  "project": {
    "name": "Project Phoenix",
    "owner": "John Smith",
    "status": "On Track",
    "lastUpdated": "2026-04-09T18:00:00Z",
    "description": "Executive dashboard for Q2 2026 roadmap"
  },
  "milestones": [
    {
      "id": "m1",
      "name": "Phase 1: Infrastructure Setup",
      "targetDate": "2026-05-15",
      "status": "Completed",
      "percentComplete": 100
    },
    {
      "id": "m2",
      "name": "Phase 2: Core Features",
      "targetDate": "2026-06-30",
      "status": "In Progress",
      "percentComplete": 65
    },
    {
      "id": "m3",
      "name": "Phase 3: QA & Launch",
      "targetDate": "2026-07-31",
      "status": "Not Started",
      "percentComplete": 0
    }
  ],
  "progress": {
    "shipped": [
      { "id": "i1", "title": "User authentication", "assignee": "Alice Johnson" },
      { "id": "i2", "title": "Dashboard layout", "assignee": "Bob Smith" }
    ],
    "inProgress": [
      { "id": "i3", "title": "ProgressBoard implementation", "assignee": "David Brown" }
    ],
    "carriedOver": [
      { "id": "i4", "title": "Mobile responsiveness", "reason": "Blocked on design review" }
    ]
  },
  "metrics": {
    "totalItems": 50,
    "shippedCount": 30,
    "inProgressCount": 15,
    "carriedOverCount": 5
  }
}
```

### Data Relationships

```
ProjectData (root aggregate)
├─ ProjectInfo (1:1, composition)
├─ Milestones[] (1:many, composition)
│  └─ Milestone (5-10 items, enforced via UI)
├─ ProgressData (1:1, composition)
│  ├─ Shipped[] (0-many ProgressItem)
│  ├─ InProgress[] (0-many ProgressItem)
│  └─ CarriedOver[] (0-many ProgressItem)
│     └─ Total <50 items, enforced via UI
└─ MetricsData (1:1, composition)
   └─ Auto-computed from Progress counts
```

### Cardinality Constraints

- **Milestones:** 5-10 items (enforced via FluentValidation at startup)
- **ProgressItems:** <50 total across all columns (enforced via FluentValidation)
- **Status Enums:** Strictly-typed validation ("Completed" | "In Progress" | "Not Started")
- **PercentComplete:** Range 0-100 (enforced via [Range] attribute)

---

## API Contracts

### Service Interfaces

#### IDataConfigurationService

```csharp
public interface IDataConfigurationService
{
    /// <summary>
    /// Load and deserialize project data from JSON file.
    /// Throws FileNotFoundException if file doesn't exist.
    /// Throws JsonException if JSON is malformed.
    /// Throws ConfigurationException if schema validation fails.
    /// </summary>
    Task<ProjectData> LoadConfigurationAsync(string filePath);
    
    /// <summary>
    /// Validate project data schema against FluentValidation rules.
    /// Returns ValidationResult with all violations.
    /// </summary>
    ValidationResult ValidateProjectData(ProjectData data);
    
    /// <summary>
    /// Check if file exists at specified path.
    /// </summary>
    bool FileExists(string filePath);
}
```

#### IProjectDashboardState

```csharp
public interface IProjectDashboardState
{
    ProjectData? CurrentProject { get; }
    bool IsLoaded { get; }
    string? LoadError { get; }
    
    Task InitializeAsync();
    Task RefreshDataAsync();
    void SetRefreshInterval(TimeSpan interval);
    event Action? OnStateChanged;
}
```

### Component Parameter Contracts

#### Dashboard.razor
- No public parameters (root component)
- Injected: ProjectDashboardState
- Cascades: ProjectDashboardState to children

#### MilestoneTimeline.razor
- Cascading Parameter: ProjectData Data

#### ProgressBoard.razor
- Cascading Parameter: ProjectData Data
- Child Component: ProgressColumn (×3)

#### ProgressColumn.razor
- Parameter: string ColumnTitle
- Parameter: List<ProgressItem> Items
- Parameter: string StatusColor

#### MetricsPanel.razor
- Cascading Parameter: ProjectData Data

### Exception Types

```csharp
public class ConfigurationException : Exception
{
    public List<string> ValidationErrors { get; set; }
    public ConfigurationException(string message, List<string> errors = null)
        : base(message)
    {
        ValidationErrors = errors ?? new();
    }
}
```

### Error Response Format

When error occurs during initialization:
```
State.LoadError = "{ErrorType}: {DetailedMessage}"

Examples:
- "FileNotFoundException: data.json not found at wwwroot/data.json. Ensure file exists in application root."
- "JsonException: Invalid JSON format at line 5, column 12: Unexpected token '}'"
- "ConfigurationException: Validation failed. Errors: [Project name is required, Milestone percentComplete must be 0-100]"
```

---

## Infrastructure Requirements

### Hosting

**Web Server:** ASP.NET Core Kestrel (built-in, no IIS required)

**Kestrel Configuration (Program.cs):**
```csharp
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(5001, listenOptions =>
    {
        listenOptions.UseHttps(); // Dev certificate
    });
});

app.Run("https://localhost:5001");
```

**HTTPS Certificate:**
- Development: Self-signed certificate (automatic via `dotnet dev-certs https`)
- Production (local): Windows certificate store or self-signed

**Port:** 5001 (HTTPS only)

**Startup Time Target:** <2 seconds on modern hardware (Core i7, 16GB RAM)

### Networking

**Listening Address:** `https://localhost:5001`

**Protocol:** HTTPS only (Blazor Server default)

**Firewall:** Not required (local machine only)

**Network Isolation:** Single machine (no network exposure planned for MVP)

**Browser Connectivity:**
- Launch browser automatically on startup via `Process.Start()`
- Target: Chrome 124+, Edge 124+
- Not supported: Internet Explorer, Safari (Windows)

### Storage

**Directory Structure:**
```
[AppRoot]/
├─ wwwroot/
│  ├─ data.json                       (Project configuration, read-only)
│  ├─ index.html
│  ├─ css/
│  │  └─ dashboard.css                (Print media queries)
│  └─ js/
│     └─ app.js
├─ bin/Release/net8.0-windows/
│  └─ publish/                        (Self-contained .exe output)
└─ app.exe                            (Executable)
```

**File Permissions:**
- wwwroot/data.json: Read-only (prevent accidental overwrites)
- wwwroot/css/: Read-only
- Application directory: Execute permission

**Storage Sizes:**
- Self-contained .exe: ~150-200MB (includes .NET 8 runtime)
- wwwroot/: ~5MB (CSS, HTML, data.json)
- Runtime memory at startup: ~200-300MB

**Data Persistence:**
- In-memory state: Lost on application restart
- data.json: Persists on disk (stakeholder updates via external tool)

### Dependencies

**NuGet Packages:**
```xml
<ItemGroup>
    <PackageReference Include="MudBlazor" Version="6.10.2" />
    <PackageReference Include="MudBlazor.Extensions" Version="5.4.0" />
    <PackageReference Include="FluentValidation" Version="11.9.2" />
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
</ItemGroup>

<ItemGroup Condition="'$(Configuration)' == 'Debug'">
    <PackageReference Include="xUnit" Version="2.7.0" />
    <PackageReference Include="Moq" Version="4.20.70" />
</ItemGroup>
```

**System Requirements:**
- .NET 8 SDK (development only)
- Visual Studio 2022 Community or VS Code + C# Dev Kit
- Windows 10 or Windows 11 (target deployment)

**No External Dependencies:**
- No cloud SDKs (AWS, Azure)
- No databases
- No packages requiring native DLLs
- Docker optional for deployment (not required for MVP)

### CI/CD Pipeline

**Build Script:**
```powershell
# Restore NuGet packages
dotnet restore

# Run unit tests
dotnet test

# Build for development
dotnet build -c Debug

# Publish as self-contained executable
dotnet publish -c Release --self-contained -r win-x64 -o ./publish
```

**Build Output:**
```
publish/
├─ AgentSquad.Runner.exe             (Executable)
├─ wwwroot/                          (Static files bundled)
│  ├─ data.json
│  ├─ index.html
│  └─ css/dashboard.css
└─ [.NET 8 runtime files]            (~150MB total)
```

**Deployment Process:**
1. Build self-contained .exe via `dotnet publish`
2. Package as ZIP archive: `AgentSquad.Runner-v1.0.zip`
3. Distribute to stakeholders
4. Stakeholders extract ZIP
5. Double-click .exe to launch (browser opens to https://localhost:5001)

### Monitoring & Logging

**Logging Configuration (appsettings.json):**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "System": "Warning",
      "Microsoft": "Warning",
      "AgentSquad.Runner": "Information"
    }
  }
}
```

**Log Output Targets:**
- Console (Kestrel startup messages)
- Optional: File logging via Serilog (future enhancement)

**Error Tracking:**
- ProjectDashboardState.LoadError captures startup failures
- Displayed in Dashboard.razor error UI
- No remote error reporting (local-only)

**Health Checks:**
- Application startup verification
- data.json file existence check
- Schema validation at startup

**Performance Metrics:**
- Startup time: Measured via Stopwatch (target <2s)
- Render time: Blazor built-in performance timeline (target <1s)
- Memory usage: Monitor via Task Manager (target <500MB)

### Testing Infrastructure

**Unit Testing Framework:** xUnit 2.7.0

**Mocking Library:** Moq 4.20.0

**Test Scope:**
- DataConfigurationService (load, deserialize, validate)
- ProjectDashboardState (initialize, refresh, error handling)
- MetricsData computed properties (percentage calculations)
- FluentValidation rules (schema validation)

**No UI Testing Required:**
- Blazor components are rendered server-side (deterministic)
- Manual browser testing sufficient for screenshot validation
- Optional: Playwright for automated screenshot testing (Phase 2)

**Test Execution:**
```powershell
dotnet test
```

### Development Environment Setup

**Prerequisites:**
- Windows 10 or Windows 11
- .NET 8 SDK
- Visual Studio 2022 Community or VS Code + C# Dev Kit

**Setup:**
```powershell
git clone <repo-url>
cd AgentSquad.Runner
dotnet restore
dotnet run
# Browser opens to https://localhost:5001
```

---

## Technology Stack Decisions

### Decision 1: State Management Pattern

**Selected:** Blazor Scoped Service (ProjectDashboardState) + In-Memory

**Justification:**
- Single-user local dashboard: No concurrent request handling needed
- Scoped lifetime: One state instance per Blazor Server connection
- No external dependencies (Redis, database)
- Performance: <1ms overhead
- Acceptable limitation: State lost on restart (short sessions only)

**Alternative Rejected:** Distributed cache (Redis) - unnecessary complexity
**Alternative Rejected:** Database (SQLite) - premature optimization; migration path exists for Phase 2

---

### Decision 2: Data Storage & Configuration

**Selected:** System.Text.Json + wwwroot/data.json

**Justification:**
- Aligns with PM requirement "read from JSON"
- Zero runtime dependencies (built-in to .NET 8)
- ~15% faster deserialization than Newtonsoft.Json
- Human-editable, version-control friendly
- Supports optional 5-minute file refresh timer
- Schema validated via FluentValidation at startup

**Alternative Rejected:** SQLite - adds ~50KB file + ORM complexity
**Alternative Rejected:** In-memory only - no persistence; poor stakeholder confidence

---

### Decision 3: UI Component Library

**Selected:** MudBlazor 6.10.2

**Justification:**
- Pre-built MudTimeline component ideal for milestone visualization
- Pre-built Cards/Grids for progress columns
- MIT licensed, active maintenance (40+ contributors)
- 3K+ GitHub stars, 1.5M NuGet downloads/week
- ~50-100ms render overhead acceptable (<2% total)
- Excellent documentation and community support

**Alternative Rejected:** Radzen - server-side dependencies complicate local deployment
**Alternative Rejected:** Custom Bootstrap - highest development cost, no pre-built components

---

### Decision 4: Deployment Model

**Selected:** Self-Contained Windows x64 .exe

**Justification:**
- Zero dependencies on stakeholder machine
- Single file distribution (single .exe)
- Startup <2 seconds on modern hardware
- ~150-200MB size acceptable for modern storage
- No installation required

**Command:** `dotnet publish -c Release --self-contained -r win-x64`

**Alternative Rejected:** Framework-dependent - requires .NET 8 pre-installed (installation burden)
**Alternative Rejected:** Docker - adds complexity; unnecessary for local deployment
**Alternative Rejected:** IIS - infrastructure overhead, Windows admin required

---

### Decision 5: Single-Page Rendering & Export

**Selected:** HTML/CSS + Browser Print-to-PDF

**Justification:**
- Zero dependencies
- CSS @media print queries enable print optimization
- Deterministic output (server-side rendering)
- No pagination (single-page constraint ensures consistency)
- Browser print-to-PDF sufficient for manual PowerPoint workflow

**Alternative Rejected:** Backend PDF generation (PdfSharp) - adds library, unnecessary
**Alternative Rejected:** Headless browser (Playwright) - useful for CI/CD; overkill for manual screenshots

---

### Decision 6: Data Validation

**Selected:** FluentValidation 11.9.x

**Justification:**
- Expressive, chainable validation rules
- Excellent error messages (display to users)
- FOSS, actively maintained
- Validates complex nested structures (milestones, progress items)
- Clear, actionable error messages at startup

**Alternative Rejected:** Data Annotations - limited expressiveness, verbose
**Alternative Rejected:** Manual validation - error-prone, low maintainability

---

## Security Considerations

### Authentication & Authorization

**Status:** NOT REQUIRED for MVP

- Single-user local dashboard (no multi-user access control)
- Authorized stakeholders only (trusted network assumption)
- No RBAC needed for current scope
- HTTPS enforced by default (Blazor Server dev certificate on localhost:5001)

**Future Phase 2 Consideration:**
```csharp
// If distributed across org network, add Windows authentication:
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();
builder.Services.AddAuthorization();
```

### Data Protection

**In Transit:**
- HTTPS enforced: All traffic encrypted via TLS 1.2+ (Blazor Server default)
- Local machine only: No data leaves machine
- Self-signed certificate sufficient for localhost

**At Rest:**
- data.json stored as plain-text JSON (semi-public project metadata only)
- No sensitive credentials, PII, or secrets allowed
- File permissions: Read-only access recommended
- Backup: Version control only (no automatic backup)

**Memory State:**
- In-memory ProjectData cached in ProjectDashboardState (scoped, per-user)
- State lost on application restart (acceptable for local)
- No persistence layer = no database exposure

### Input Validation

**Data Source Validation via FluentValidation:**

```csharp
public class ProjectDataValidator : AbstractValidator<ProjectData>
{
    public ProjectDataValidator()
    {
        RuleFor(x => x.Project).NotNull().SetValidator(new ProjectInfoValidator());
        RuleFor(x => x.Milestones)
            .NotNull()
            .Must(m => m.Count >= 5 && m.Count <= 10)
            .WithMessage("Must have 5-10 milestones");
        RuleFor(x => x.Progress).NotNull().SetValidator(new ProgressDataValidator());
        RuleFor(x => x.Metrics).NotNull().SetValidator(new MetricsDataValidator());
    }
}

public class ProgressDataValidator : AbstractValidator<ProgressData>
{
    public ProgressDataValidator()
    {
        RuleFor(x => x.Shipped.Count + x.InProgress.Count + x.CarriedOver.Count)
            .LessThanOrEqualTo(50)
            .WithMessage("Total work items must be <50");
    }
}
```

**JSON Deserialization Validation:**
- System.Text.Json enforces type safety (no dynamic objects)
- Required fields validated at deserialization
- Enum validation: Status must match exact values
- Range validation: PercentComplete 0-100, TotalItems <1000

**Render Context Security:**
- Blazor components receive cascaded parameters only (no query string manipulation)
- No user input in HTML (all data from validated JSON)
- No SQL injection (no database layer)
- No XSS: MudBlazor components auto-escape HTML

### Configuration Security

**Secrets Management:**
- NO secrets in data.json (policy: metadata only)
- NO API keys, passwords, or tokens in source code
- NO connection strings (local file-based storage)

**File Access Control:**
- Read-only recommendation for wwwroot/data.json
- Windows file permissions: Restrict write to project owner only
- Configuration files: No sensitive data

---

## Scaling Strategy

### Horizontal Scaling (NOT APPLICABLE for MVP)

**Why Not Needed:**
- Single project per dashboard = 1 data.json per instance
- Single user (local machine) = no concurrent request handling
- Fixed workload: <50 items, <10 milestones = constant render time

**Future Phase 2 Options:**
- **Option A:** Deploy independent .exe per project (simplest)
- **Option B:** Migrate to SQLite + multi-project selector (add project ID query param)
- **Option C:** Add lightweight load balancer (Nginx) for shared server deployment

### Vertical Scaling (CURRENT APPROACH)

**Current Constraints:**
- Single thread: Kestrel handles 1-10 concurrent connections (overkill for single-user)
- Memory: <500MB at startup (acceptable on stakeholder machines)
- CPU: Single core utilization (render time <1s)
- Disk: 150-200MB .exe + 5MB wwwroot

**Performance Optimization Points (if needed):**

1. **Large Item Count Mitigation (if >50 items):**
   ```csharp
   // Optional: Lazy-load items in ProgressColumn
   [Parameter] public List<ProgressItem> Items { get; set; }
   private List<ProgressItem> VisibleItems;
   private int PageSize = 20;
   
   protected override void OnParametersSet()
   {
       VisibleItems = Items.Take(PageSize).ToList();
   }
   
   private void LoadMore()
   {
       VisibleItems = Items.Take(VisibleItems.Count + PageSize).ToList();
   }
   ```

2. **Timeline Rendering (if >10 milestones):**
   ```csharp
   @if (milestone.Status == "Completed" && milestone.TargetDate < DateTime.Now.AddMonths(-3))
   {
       <!-- Collapsed view -->
   }
   else
   {
       <!-- Full view -->
   }
   ```

3. **Data Refresh Interval:**
   ```csharp
   // Default 5 minutes, disable if data rarely changes
   DashboardState.SetRefreshInterval(TimeSpan.FromMinutes(5));
   DashboardState.SetRefreshInterval(TimeSpan.Zero); // Disable
   ```

### Caching Strategy

**In-Memory Caching:**
- ProjectDashboardState holds single ProjectData instance (scoped per connection)
- Metrics computed on-demand from Progress items (no separate cache)
- No distributed cache needed (single machine)

**Optional: Browser-Side Caching (Phase 2):**
```csharp
// Cache data.json in localStorage (future enhancement)
public async Task LoadFromCacheOrNetwork()
{
    var cached = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "projectData");
    if (!string.IsNullOrEmpty(cached))
    {
        CurrentProject = JsonSerializer.Deserialize<ProjectData>(cached);
    }
    else
    {
        await RefreshDataAsync();
        await JSRuntime.InvokeVoidAsync("localStorage.setItem", "projectData", jsonString);
    }
}
```

### Load Balancing

**NOT APPLICABLE for MVP:**
- Single machine: No load distribution
- Single Kestrel instance: Handles all requests
- No shared state: Each stakeholder runs independent .exe

**Future Multi-Machine Deployment (Phase 2+):**
```
Scenario: Shared network server
Option: Deploy to Windows IIS
  - Application pool per project
  - Disable session affinity (stateless Blazor)
  - External session store (Redis) if state needed
  - Front-load with Nginx/HAProxy for SSL termination
```

---

## Risks & Mitigations

### Risk Assessment Matrix

| Risk | Likelihood | Impact | Priority | Mitigation |
|------|-----------|--------|----------|-----------|
| JSON schema change requires recompilation | MEDIUM | MEDIUM | **P0** | Document schema in README, strict validation, strongly-typed models, clear error messages |
| Screenshot inconsistency across browsers/OS | LOW | MEDIUM | **P0** | Lock CSS to 1200px, test on Chrome/Edge 124+, Windows 10/11 at 1920x1080 & 1440x900, use @media print |
| data.json corruption or deletion | LOW | MEDIUM | **P1** | File existence check at startup, fallback empty state, warning message, version control backup |
| Large item counts (100+) degrade performance | LOW | LOW | **P1** | Enforce <50 items validation, implement pagination if exceeded, document limitation |
| Blazor Server hub timeout during presentations | LOW | LOW | **P2** | Default timeout 30s idle, extend via CircuitOptions if needed, keep window active |
| .exe file large (~150-200MB) | LOW | LOW | **P2** | Accept size, recommend framework-dependent if network distribution problematic, compress as ZIP |
| Single-user local state lost on restart | HIGH | LOW | **P2** | Acceptable for MVP, document in README, add SQLite persistence if Phase 2 needed |
| No concurrent write safety (data.json) | LOW | MEDIUM | **P2** | Single-user local only, read-only recommended, implement version control lock if multi-reader |
| Dependency on MudBlazor (external) | LOW | LOW | **P3** | MIT licensed, active maintenance (40+ contributors), large community, upgrade path available |
| FluentValidation vulnerability | LOW | MEDIUM | **P3** | Monitor NuGet advisories, pin version ≥11.9.2, upgrade quarterly, no remote execution risk |

### Technical Risk Mitigations

#### Risk 1: JSON Schema Changes Require Recompilation

**Concrete Mitigation Steps:**

1. Document schema in README with examples
2. Version data.json (add "version": "1.0" field)
3. Create schema change log
4. Implement validator with clear error messages:
   ```csharp
   public class ProjectDataValidator : AbstractValidator<ProjectData>
   {
       private const int SupportedVersion = 1;
       
       public ProjectDataValidator()
       {
           RuleFor(x => x.SchemaVersion)
               .Equal(SupportedVersion)
               .WithMessage($"Unsupported schema version. Expected {SupportedVersion}");
       }
   }
   ```
5. Create migration guide for stakeholders
6. Test with sample data.json files before distribution

---

#### Risk 2: Screenshot Inconsistency Across Browsers/OS

**Concrete Mitigation Steps:**

1. Lock layout to fixed width (1200px):
   ```css
   .dashboard-container {
       max-width: 1200px;
       margin: 0 auto;
       padding: 20px;
   }
   
   @media print {
       body { margin: 0; padding: 10px; }
       .dashboard-container { max-width: 100%; }
       nav, footer { display: none; }
       .timeline { page-break-inside: avoid; }
       .progress-board { page-break-inside: avoid; }
   }
   ```

2. Use CSS @media print queries (standard feature)
3. Test on target OS/browser combinations
4. Create test matrix:

| OS | Browser | Resolution | Print Status |
|----|---------|-----------|----------|
| Windows 10 | Chrome 124 | 1920x1080 | ✓ Pass |
| Windows 10 | Edge 124 | 1440x900 | ✓ Pass |
| Windows 11 | Chrome 124 | 1920x1080 | ✓ Pass |
| Windows 11 | Edge 124 | 1440x900 | ✓ Pass |

5. Document in README: "Tested on Chrome 124+, Edge 124+, Windows 10/11"

---

#### Risk 3: Data Corruption or Accidental Deletion

**Concrete Mitigation Steps:**

1. Implement file existence check at startup:
   ```csharp
   public async Task InitializeAsync()
   {
       try
       {
           if (!_configService.FileExists(DataFilePath))
           {
               LoadError = $"Critical Error: data.json not found at {DataFilePath}\n\n" +
                   "Recovery steps:\n" +
                   "1. Restore data.json from backup\n" +
                   "2. Ensure file exists in wwwroot/ folder\n" +
                   "3. Restart application";
               return;
           }
           
           CurrentProject = await _configService.LoadConfigurationAsync(DataFilePath);
           IsLoaded = true;
       }
       catch (Exception ex)
       {
           LoadError = $"Error: {ex.Message}";
           IsLoaded = false;
       }
   }
   ```

2. Recommend read-only file permissions
3. Version control backup (commit data.json to Git)
4. Document backup process in README

---

#### Risk 4: Large Item Counts Degrade Performance

**Concrete Mitigation Steps:**

1. Enforce <50 items in validation:
   ```csharp
   RuleFor(x => x.Milestones)
       .Must(m => m.Count <= 10)
       .WithMessage("Maximum 10 milestones allowed");
   
   RuleFor(x => x.Shipped.Count + x.InProgress.Count + x.CarriedOver.Count)
       .LessThanOrEqualTo(50)
       .WithMessage("Maximum 50 total work items allowed");
   ```

2. Display clear validation error if limit exceeded
3. Document in README: "Executive-level summary: max 50 items"
4. Offer pagination as Phase 2 enhancement

---

### Dependency Risk Mitigations

**NuGet Package Dependencies:**

1. **MudBlazor 6.10.2**
   - Mitigation: MIT licensed, 3K+ GitHub stars, 1.5M downloads/week
   - Risk: Breaking change in new version
   - Strategy: Pin to 6.10.x, test before upgrading

2. **FluentValidation 11.9.x**
   - Mitigation: Open-source, widely used, security monitored
   - Risk: Vulnerability in library
   - Strategy: Monitor NuGet security advisories, upgrade quarterly

3. **System.Text.Json (built-in)**
   - No risk: Part of .NET Framework, Microsoft-maintained

---

### Operational Risk Mitigations

**Performance Monitoring:**
```csharp
private Stopwatch _stopwatch = Stopwatch.StartNew();

protected override async Task OnInitializedAsync()
{
    await DashboardState.InitializeAsync();
    var startupMs = _stopwatch.ElapsedMilliseconds;
    Console.WriteLine($"Startup time: {startupMs}ms (target: <2000ms)");
    
    if (startupMs > 2000)
        Console.WriteLine("⚠️ WARNING: Startup exceeds 2s target");
}

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        var renderMs = _stopwatch.ElapsedMilliseconds - startupMs;
        Console.WriteLine($"Render time: {renderMs}ms (target: <1000ms)");
    }
}
```

**Error Logging:**
```csharp
public class DataConfigurationService
{
    private readonly ILogger<DataConfigurationService> _logger;
    
    public async Task<ProjectData> LoadConfigurationAsync(string filePath)
    {
        try
        {
            _logger.LogInformation($"Loading data.json from {filePath}");
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError($"File not found: {filePath}");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"JSON parse error: {ex.Message}");
            throw;
        }
    }
}
```

---

## Summary: Highest-Impact Risk Controls

| Control | Effort | Impact | Priority |
|---------|--------|--------|----------|
| Fixed-width CSS layout + print media queries | LOW | HIGH | **P0** |
| Schema validation with clear error messages | MEDIUM | HIGH | **P0** |
| File existence check at startup | LOW | MEDIUM | **P1** |
| Documentation (README with schema, limits) | LOW | MEDIUM | **P1** |
| Test matrix (browsers, OS, resolutions) | MEDIUM | HIGH | **P1** |
| Version control backup (data.json in Git) | LOW | MEDIUM | **P2** |
| Performance monitoring via Stopwatch | LOW | LOW | **P2** |
| Item count validation (<50 total) | LOW | LOW | **P2** |