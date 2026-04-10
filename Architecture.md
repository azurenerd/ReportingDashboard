# Architecture

## Overview & Goals

**Executive Dashboard** is a lightweight, screenshot-ready web application that displays project milestones, work item status (shipped, in-progress, carried-over), and progress metrics on a single page. Built on .NET 8 Blazor Server with Bootstrap 5.3.0, it loads data from a local JSON configuration file and renders pixel-perfect screenshots for PowerPoint presentations without post-processing.

**Core Goals:**
1. **Enable Executive Visibility**: Display project status, shipped items, active work, carried-over items, and milestone timeline on a single screen with zero scrolling for typical workloads
2. **Minimize Time-to-Screenshot**: Executives capture PowerPoint-ready screenshots in <30 seconds, with zero Photoshop or image editing required
3. **Eliminate Infrastructure Overhead**: Deliver dashboard functionality in 2-3 days with zero cloud services, no CI/CD, no authentication—run locally via `dotnet run`
4. **Ensure Screenshot Consistency**: Render identically across Chrome, Edge, Firefox on Windows/macOS/Linux for reproducible presentation decks
5. **Simplify Data Management**: Executives update dashboard content by editing `data.json` and restarting the application; no database, no API backend

**Architecture Principles:**
- **Immutable data flow**: ProjectDashboard loaded once at startup, cached in memory, passed immutably to components
- **Server-side rendering**: Blazor Server ensures deterministic rendering across all browsers; no client-side variation
- **Single responsibility**: Each component owns exactly one UI section; DashboardPage orchestrates layout
- **Minimal dependencies**: Bootstrap 5.3.0 only external CSS framework; System.Text.Json built-in JSON parsing
- **Local-only deployment**: No cloud services, no database, no authentication; runs on executive's machine via `dotnet run`

---

## System Components

### ProjectDataService (Singleton Service)

**Responsibility:**
- Load data.json from disk at application startup
- Parse and deserialize JSON into immutable ProjectDashboard object
- Cache ProjectDashboard in memory for application lifetime
- Provide data access via dependency injection
- Validate schema and report errors to UI
- Support future hot-reload capability (Phase 2)

**Public Interface:**
```csharp
public interface IProjectDataService
{
    // Initialize data on startup; throws on file not found, JSON error, schema validation failure
    Task InitializeAsync();
    
    // Get cached dashboard object; throws if not initialized
    ProjectDashboard GetDashboard();
    
    // Reload data from disk (Phase 2)
    Task RefreshAsync();
    
    // Status properties
    bool IsInitialized { get; }
    string? LastError { get; }
}
```

**Dependencies:**
- `IWebHostEnvironment` (DI) - locate ContentRootPath
- `ILogger<ProjectDataService>` (DI) - structured logging
- System.Text.Json - JSON deserialization

**Data Ownership:**
- Owns: Singleton ProjectDashboard instance (immutable)
- Immutability: Yes (clients receive read-only reference)

**Configuration (Program.cs):**
```csharp
services.AddSingleton<IProjectDataService, ProjectDataService>();
var dataService = app.Services.GetRequiredService<IProjectDataService>();
await dataService.InitializeAsync(); // Call before app.Run()
```

---

### DashboardPage.razor (Root Component)

**Responsibility:**
- Serve as application root and layout container
- Inject IProjectDataService and retrieve cached dashboard
- Render page structure (header, timeline, grid, footer)
- Expose immutable ProjectDashboard to children via CascadingValue
- Handle initialization errors and display user-friendly messages

**Public Interface:**
```razor
@page "/"
@inject IProjectDataService DataService

<!-- Renders on success: header + 3 child components -->
<!-- Renders on error: alert with friendly message -->
<!-- Renders on loading: spinner -->
```

**Parameters:**
- None (root component)

**Cascades:**
- `ProjectDashboard` (immutable)

**Child Components:**
- TimelinePanel.razor
- StatusGrid.razor
- MetricsFooter.razor

**Error Handling:**
```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        Dashboard = DataService.GetDashboard();
    }
    catch (Exception ex)
    {
        ErrorMessage = DataService.LastError ?? ex.Message;
        // Display alert instead of crashing
    }
}
```

---

### TimelinePanel.razor

**Responsibility:**
- Display milestone timeline horizontally across page width
- Render each milestone with status badge and target date
- Sort milestones chronologically (left to right)
- Apply status colors (green=Completed, blue=OnTrack, orange=AtRisk, red=Delayed)
- Ensure WCAG AA accessibility (color + text differentiation)

**Parameters:**
```csharp
[CascadingParameter] ProjectDashboard? Dashboard { get; set; }
```

**Renders:**
- Flexbox timeline container with milestone items
- Status badge for each milestone
- Milestone name and date

**Styling:**
- `.timeline-container` - flexbox layout, horizontal scrolling if needed
- `.timeline-item` - centered column with marker + content
- `.timeline-marker` - colored circle (12px) for status

---

### StatusGrid.razor

**Responsibility:**
- Render three-column layout (Shipped, InProgress, CarriedOver)
- Distribute work items across columns
- Apply responsive Bootstrap grid (col-md-4 for desktop)
- Pass filtered work-item lists to StatusColumn children

**Parameters:**
```csharp
[CascadingParameter] ProjectDashboard? Dashboard { get; set; }
```

**Child Components:**
- StatusColumn (3 instances: Shipped, InProgress, CarriedOver)

**Layout:**
- Bootstrap row with 3 equal-width columns (col-md-4)
- Gap between columns (g-3)

---

### StatusColumn.razor (Reusable)

**Responsibility:**
- Display single column of work items with consistent styling
- Render each item as a card with title, description, completion date
- Show column header with item count (e.g., "Shipped (8)")
- Sort items in reverse chronological order (most recent first)
- Apply color-coded badge (green/blue/orange)

**Parameters:**
```csharp
[Parameter] public string? Title { get; set; } // "Shipped", "In Progress", "Carried Over"
[Parameter] public List<WorkItem>? Items { get; set; } // Work items for column
[Parameter] public string BadgeClass { get; set; } = "secondary"; // Bootstrap color class
```

**Renders:**
- Column header with badge showing count
- Card list of work items
- Empty state message if no items

**Styling:**
- `.status-column` - light gray background, padding, border-radius
- `.card` - white background, subtle shadow, rounded corners
- `.card-title` - font-weight 600
- Overflow-y auto if >10 items (Phase 1; Phase 2 adds Virtualize wrapper)

**Phase 2 Enhancement (Virtualization):**
```razor
<Virtualize Items="Items.OrderByDescending(i => i.CompletedDate).ToList()" 
           Context="item" 
           OverscanCount="5">
    <!-- Card template -->
</Virtualize>
```

---

### MetricsFooter.razor

**Responsibility:**
- Display aggregate project metrics at dashboard bottom
- Show: Total Planned Items, Completed Count, Completion %, Health Score
- Use large, legible fonts for screenshot visibility
- Apply color coding (green=healthy, orange=at-risk, red=blocked)
- Ensure metrics visible without excessive scrolling

**Parameters:**
```csharp
[CascadingParameter] ProjectDashboard? Dashboard { get; set; }
```

**Renders:**
- Four centered metric boxes
- Large number values (font-size 2.5rem)
- Small uppercase labels
- Dynamic color based on completion % and health score

**Color Logic:**
- Completion %: >= 75% green, >= 50% orange, < 50% red
- Health Score: >= 75% green, >= 50% orange, < 50% red

---

## Component Interactions

### Primary Use Case: Application Startup → Dashboard Display

```
1. APPLICATION STARTUP
   Program.cs registers ProjectDataService as Singleton
   Program.cs calls ProjectDataService.InitializeAsync()
      Reads data/data.json from ContentRootPath
      Deserializes JSON → ProjectDashboard (with Milestones, WorkItems, ProgressMetrics)
      Caches immutable ProjectDashboard in memory
      Logs success or error to console
   Application ready; Blazor server starts listening on https://localhost:7123

2. USER NAVIGATES TO DASHBOARD
   Browser requests https://localhost:7123/
   DashboardPage.razor initializes
      OnInitializedAsync() injects IProjectDataService
      Calls DataService.GetDashboard() → retrieves cached ProjectDashboard
      Sets local Dashboard property
   DashboardPage renders

3. CASCADING DATA FLOW
   DashboardPage wraps Dashboard in <CascadingValue>
   TimelinePanel receives [CascadingParameter] ProjectDashboard
      Renders Dashboard.Milestones sorted by TargetDate
      Applies status colors and badges
   StatusGrid receives [CascadingParameter] ProjectDashboard
      Extracts Dashboard.Shipped, .InProgress, .CarriedOver
      Passes each list to StatusColumn child (3 instances)
         StatusColumn[1] renders Shipped items (green badge)
         StatusColumn[2] renders InProgress items (blue badge)
         StatusColumn[3] renders CarriedOver items (orange badge)
   MetricsFooter receives [CascadingParameter] ProjectDashboard
      Renders Dashboard.Metrics (health score, completion %)
      Calculates color codes based on thresholds

4. BROWSER RENDERS HTML/CSS
   Bootstrap 5.3.0 CSS loads from wwwroot (cached 1 year)
   Custom site.css applies color palette overrides
   TimelinePanel renders flexbox timeline (5-10 milestones horizontal)
   StatusGrid renders Bootstrap row/col-md-4 grid (3 columns side-by-side)
   StatusColumns render cards (white background, subtle shadow, readable typography)
   MetricsFooter renders centered metric boxes with large font sizes
   Total render time: <500ms on local Kestrel

5. SCREENSHOT READY
   Executive captures browser window (1920x1080)
   No post-processing needed
   Pastes directly into PowerPoint
```

### Secondary Use Case: Error Handling (Missing or Malformed data.json)

```
1. STARTUP FAILURE
   ProjectDataService.InitializeAsync() throws FileNotFoundException
   Exception logged: "Data file not found: data/data.json"
   Application continues (does not crash)
   _lastError populated with friendly message

2. DASHBOARD LOADS WITH ERROR STATE
   DashboardPage.OnInitializedAsync() calls DataService.GetDashboard()
   Catches exception (data not initialized)
   Sets ErrorMessage = DataService.LastError
   Renders <div class="alert alert-danger"> instead of dashboard content

3. USER SEES
   "Error Loading Dashboard" heading
   Friendly error text: "Data file not found: data/data.json. Create file with sample data."
   No stack trace or technical jargon
   User can check file system, fix problem, and restart application
```

### Tertiary Use Case: Future Hot-Reload (Phase 2)

```
1. DEVELOPER EDITS data.json
   Saves changes

2. FILE WATCHER DETECTS CHANGE
   FileSystemWatcher.Changed event fires
   ProjectDataService.RefreshAsync() called
   New data.json deserialized into memory
   Cached _dashboard reference updated

3. SIGNALR HUB NOTIFIES BROWSER
   HubConnection sends "DataRefreshed" message
   DashboardPage receives update notification
   StateHasChanged() triggers re-render
   Cascading value updated; all children re-render

4. BROWSER RE-RENDERS
   New milestone/workitem data appears (no page reload)
```

---

## Data Model

### Core Entities

#### ProjectDashboard (Root)
```csharp
public class ProjectDashboard
{
    public string ProjectName { get; set; } // Required: "Project Alpha"
    public string? Description { get; set; } // Optional: "Q2 2026 Roadmap"
    
    public DateTime StartDate { get; set; } // Project start date (ISO 8601)
    public DateTime PlannedCompletion { get; set; } // Target completion (ISO 8601)
    
    public List<Milestone> Milestones { get; set; } = new(); // 5-10 items
    public List<WorkItem> Shipped { get; set; } = new(); // Completed items
    public List<WorkItem> InProgress { get; set; } = new(); // Active items
    public List<WorkItem> CarriedOver { get; set; } = new(); // Backlog/delayed items
    
    public ProgressMetrics Metrics { get; set; } = new(); // Calculated aggregates
}
```

#### Milestone
```csharp
public class Milestone
{
    public string Id { get; set; } // Required, unique: "m001"
    public string Name { get; set; } // Required: "Design Review"
    public string? Description { get; set; } // Optional narrative
    
    public DateTime TargetDate { get; set; } // Target date (ISO 8601)
    public string Status { get; set; } // "Completed" | "OnTrack" | "AtRisk" | "Delayed"
}
```

**Validation Rules:**
- Status must be one of: "Completed", "OnTrack", "AtRisk", "Delayed"
- Id must be non-empty, unique within Milestones array
- TargetDate must be valid ISO 8601 format

#### WorkItem
```csharp
public class WorkItem
{
    public string Id { get; set; } // Required, globally unique: "w001"
    public string Title { get; set; } // Required: "Implement user auth"
    public string? Description { get; set; } // Optional (max 500 chars)
    
    public DateTime CompletedDate { get; set; } // When finished (ISO 8601)
    public string? Owner { get; set; } // Optional: responsible team (Phase 2)
}
```

**Validation Rules:**
- Id must be non-empty, globally unique (across Shipped/InProgress/CarriedOver)
- Title must be 1-200 characters
- Description must be <500 characters
- CompletedDate must be valid ISO 8601 format

#### ProgressMetrics
```csharp
public class ProgressMetrics
{
    public int TotalPlanned { get; set; } // Shipped.Count + InProgress.Count + CarriedOver.Count
    public int Completed { get; set; } // Shipped.Count
    public int InFlight { get; set; } // InProgress.Count + CarriedOver.Count
    public decimal HealthScore { get; set; } // (Completed / TotalPlanned) * 100, range 0-100
}
```

**Calculation (by ProjectDataService on load):**
```csharp
metrics.TotalPlanned = dashboard.Shipped.Count + dashboard.InProgress.Count + dashboard.CarriedOver.Count;
metrics.Completed = dashboard.Shipped.Count;
metrics.InFlight = dashboard.InProgress.Count + dashboard.CarriedOver.Count;
metrics.HealthScore = (metrics.Completed / Math.Max(metrics.TotalPlanned, 1)) * 100m;
```

### Data Relationships

```
ProjectDashboard
├── Milestones[] (0..10 items)
│   ├── Id: string (unique)
│   ├── Name: string
│   ├── TargetDate: DateTime
│   └── Status: string
│
├── Shipped[] (0..50 items)
│   ├── Id: string (globally unique)
│   ├── Title: string
│   ├── CompletedDate: DateTime
│   └── Description: string?
│
├── InProgress[] (0..50 items)
│   └── [same structure as Shipped]
│
├── CarriedOver[] (0..50 items)
│   └── [same structure as Shipped]
│
└── Metrics: ProgressMetrics
    ├── TotalPlanned: int
    ├── Completed: int
    ├── InFlight: int
    └── HealthScore: decimal
```

### Storage Strategy

**Format:** Local JSON file (data.json)
- **Location:** `ContentRootPath/data/data.json`
- **Encoding:** UTF-8
- **Size Limit:** <50KB typical (performance tested)
- **Mutability:** Read-only to application; manual edit by operators via text editor
- **Refresh:** Application restart required (Phase 2: file watcher)

**Sample data.json:**
```json
{
  "projectName": "Project Alpha",
  "description": "Q2 2026 Initiative",
  "startDate": "2026-01-15T00:00:00Z",
  "plannedCompletion": "2026-06-30T00:00:00Z",
  
  "milestones": [
    {
      "id": "m001",
      "name": "Requirements Complete",
      "description": "All requirements signed off",
      "targetDate": "2026-02-28T00:00:00Z",
      "status": "Completed"
    }
  ],
  
  "shipped": [
    {
      "id": "w001",
      "title": "Implement authentication",
      "description": "JWT-based user auth",
      "completedDate": "2026-02-10T00:00:00Z"
    }
  ],
  
  "inProgress": [],
  "carriedOver": []
}
```

**Deserialization (System.Text.Json):**
```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true, // JSON camelCase ↔ C# PascalCase
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
};

using var fs = File.OpenRead(dataPath);
var dashboard = await JsonSerializer.DeserializeAsync<ProjectDashboard>(fs, options);
```

---

## API Contracts

### Blazor Component Interfaces

Since this is Blazor Server (server-side rendering), there are no traditional REST endpoints. Communication occurs via:
- **Cascading Parameters** (parent-to-child immutable data flow)
- **Service Injection** (dependency injection)
- **SignalR Circuits** (WebSocket connection for interactive updates)

#### IProjectDataService

```csharp
public interface IProjectDataService
{
    /// <summary>
    /// Initialize dashboard data from data.json at startup.
    /// Must be called in Program.cs before app.Run().
    /// </summary>
    /// <exception cref="FileNotFoundException">If data.json not found</exception>
    /// <exception cref="JsonException">If JSON malformed or schema invalid</exception>
    /// <exception cref="InvalidOperationException">If deserialized data null</exception>
    Task InitializeAsync();
    
    /// <summary>
    /// Get cached ProjectDashboard (immutable).
    /// Safe to call multiple times; returns same reference until RefreshAsync.
    /// </summary>
    /// <exception cref="InvalidOperationException">If InitializeAsync not called</exception>
    ProjectDashboard GetDashboard();
    
    /// <summary>
    /// Reload data from disk and update cache (Phase 2).
    /// </summary>
    Task RefreshAsync();
    
    /// <summary>
    /// True if data loaded successfully.
    /// </summary>
    bool IsInitialized { get; }
    
    /// <summary>
    /// User-friendly error message from last load attempt. Null if successful.
    /// </summary>
    string? LastError { get; }
}
```

#### Component Parameters

**DashboardPage.razor**
```csharp
@page "/"
// No parameters (root component)
// Injects: IProjectDataService
// Cascades: ProjectDashboard
```

**TimelinePanel.razor**
```csharp
[CascadingParameter] ProjectDashboard? Dashboard { get; set; }
// Renders: Milestone items from Dashboard.Milestones
// No local state
```

**StatusGrid.razor**
```csharp
[CascadingParameter] ProjectDashboard? Dashboard { get; set; }
// Renders: 3x StatusColumn with distributed work items
// No local state
```

**StatusColumn.razor**
```csharp
[Parameter] public string? Title { get; set; } // "Shipped", "In Progress", "Carried Over"
[Parameter] public List<WorkItem>? Items { get; set; } // Work items for column
[Parameter] public string BadgeClass { get; set; } = "secondary"; // Bootstrap color
// Renders: Card list of WorkItems
// No local state
```

**MetricsFooter.razor**
```csharp
[CascadingParameter] ProjectDashboard? Dashboard { get; set; }
// Renders: 4-box metric display
// No local state
```

### Error Response Contract

| Error Type | Source | HTTP Response | UI Display |
|-----------|--------|---------------|-----------|
| FileNotFoundException | ProjectDataService | 200 OK (app continues) | Alert: "Data file not found: data/data.json" |
| JsonException | System.Text.Json | 200 OK (app continues) | Alert: "JSON deserialization failed: [message]" |
| InvalidOperationException | GetDashboard() | 200 OK | Alert: "Data not initialized" |
| General Exception | File I/O, other | 200 OK | Alert: "Unexpected error: [message]" |

**User-Facing Error:**
```html
<div class="alert alert-danger" role="alert">
  <h4 class="alert-heading">Error Loading Dashboard</h4>
  <p>@ErrorMessage</p>
  <!-- No stack trace, no technical details -->
</div>
```

### Data Validation

**ProjectDashboardValidator:**
```csharp
public class ProjectDashboardValidator
{
    public static void Validate(ProjectDashboard? dashboard)
    {
        if (dashboard == null)
            throw new InvalidOperationException("Dashboard data is null");
        
        if (string.IsNullOrWhiteSpace(dashboard.ProjectName))
            throw new JsonException("ProjectName is required");
        
        // Validate milestone statuses
        foreach (var m in dashboard.Milestones ?? new())
        {
            if (!new[] { "Completed", "OnTrack", "AtRisk", "Delayed" }.Contains(m.Status))
                throw new JsonException($"Invalid milestone status: {m.Status}");
        }
        
        // Validate work item IDs are globally unique
        var allIds = new HashSet<string>();
        foreach (var item in (dashboard.Shipped ?? new())
            .Concat(dashboard.InProgress ?? new())
            .Concat(dashboard.CarriedOver ?? new()))
        {
            if (string.IsNullOrWhiteSpace(item.Id))
                throw new JsonException("WorkItem.Id is required");
            if (!allIds.Add(item.Id))
                throw new JsonException($"Duplicate WorkItem.Id: {item.Id}");
        }
    }
}
```

---

## Infrastructure Requirements

### Hosting

**Server:**
- **Runtime:** .NET 8.0 LTS (November 2026 support)
- **Web Server:** Kestrel (built-in ASP.NET Core)
- **Protocol:** HTTPS (default), HTTP (optional)
- **Port:** 7123 (HTTPS), 5123 (HTTP) - configurable
- **Address Binding:** localhost only (127.0.0.1)

**Configuration (launchSettings.json):**
```json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "https://localhost:7123",
      "applicationUrl": "https://localhost:7123;http://localhost:5123",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Certificate:** Self-signed HTTPS certificate (auto-generated by dotnet run)

### Networking

**Connections:**
- Local machine only; no remote access
- Browser-to-Server: SignalR WebSocket (localhost)
- Server-to-File System: Local file I/O

**Port Requirements:**
- 7123 (HTTPS, configurable)
- 5123 (HTTP, configurable)
- No firewall rules needed (localhost only)

### Storage

**Directory Structure:**
```
ProjectRoot/
├── src/AgentSquad.Runner/
│   ├── Components/
│   │   ├── DashboardPage.razor
│   │   ├── TimelinePanel.razor
│   │   ├── StatusGrid.razor
│   │   ├── StatusColumn.razor
│   │   └── MetricsFooter.razor
│   ├── Models/
│   │   ├── ProjectDashboard.cs
│   │   ├── Milestone.cs
│   │   ├── WorkItem.cs
│   │   └── ProgressMetrics.cs
│   ├── Services/
│   │   ├── IProjectDataService.cs
│   │   └── ProjectDataService.cs
│   ├── wwwroot/
│   │   ├── css/
│   │   │   ├── bootstrap.min.css (90KB, cached 1yr)
│   │   │   ├── bootstrap-icons.css (50KB)
│   │   │   └── site.css (<2KB custom overrides)
│   │   └── data/
│   │       └── data.json (sample, <50KB)
│   ├── App.razor
│   ├── Program.cs
│   ├── appsettings.json
│   └── AgentSquad.Runner.csproj
├── tests/
│   ├── AgentSquad.Runner.Tests/
│   │   ├── ProjectDataServiceTests.cs (xUnit)
│   │   ├── TimelinePanelTests.cs (Bunit)
│   │   └── AgentSquad.Runner.Tests.csproj
├── README.md
└── .gitignore
```

**Data Storage:**
- **Location:** `wwwroot/data/data.json`
- **Format:** JSON, UTF-8
- **Size:** <50KB typical
- **Ownership:** Operator (manual edit)
- **Encryption:** None (local filesystem assumed secure)

**Disk Requirements:**
- Source code: ~5MB
- Dependencies: ~200MB
- Build output: ~50MB
- Runtime resident: ~100MB
- **Total:** ~400MB

### Configuration

**appsettings.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Blazor": {
    "CircuitOptions": {
      "DisconnectedCircuitMaxRetained": 100,
      "DisconnectedCircuitRetentionPeriod": "00:03:00",
      "JSInteropDefaultAsyncTimeout": "00:01:00"
    }
  }
}
```

**appsettings.Development.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug"
    }
  }
}
```

### Build and Deployment

**Build:**
```bash
dotnet build
```
- Time: <5 seconds on clean checkout
- Output: /bin/Debug or /bin/Release
- Dependencies: Downloaded from nuget.org (cached locally)

**Run:**
```bash
dotnet run
```
- Starts application on https://localhost:7123
- Logs to console (structured logging)
- Shutdown: Ctrl+C

**Publish (Optional, for non-developer distribution):**
```bash
dotnet publish -c Release -o ./publish
```
- Output: Self-contained executable (~80MB)
- Execution: `./publish/AgentSquad.Runner.exe` (Windows)

### CI/CD

**Not applicable.** This is a local-only tool.
- No automated build pipelines required
- No cloud deployment
- Manual testing via dotnet run
- No artifact storage

### Monitoring and Logging

**Framework:** Microsoft.Extensions.Logging (built-in)

**Log Output:**
- Destination: Console (stdout)
- Format: Structured text (timestamp, level, category, message)
- Retention: Session-only
- Sensitive Data: Never logged (only load events)

**Log Categories:**
```csharp
namespace AgentSquad.Runner.Services
{
    public class ProjectDataService : IProjectDataService
    {
        private readonly ILogger<ProjectDataService> _logger;
        
        public async Task InitializeAsync()
        {
            _logger.LogInformation("Loading dashboard from {Path}", dataPath);
            _logger.LogError(ex, "Failed to load dashboard data");
        }
    }
}
```

**Performance Monitoring:** Manual via browser DevTools
- Page load time: F12 → Network → measure DOMContentLoaded
- Memory: F12 → Memory tab
- Re-render latency: Browser console profiling

---

## Technology Stack Decisions

### Decision 1: Data Loading & Caching Strategy

**Chosen:** One-time startup load into memory (singleton cache)

**Rationale:**
- Meets <500ms page load requirement
- Simplest implementation (zero disk I/O after startup)
- Sufficient for single-session use case
- File watcher deferred to Phase 2

**Implementation:**
```csharp
services.AddSingleton<IProjectDataService, ProjectDataService>();
await dataService.InitializeAsync(); // Call in Program.cs
```

---

### Decision 2: State Management & Data Binding

**Chosen:** Blazor cascading parameters (immutable, parent-to-child)

**Rationale:**
- Read-only data (no updates)
- Immutable ProjectDashboard passed down component tree
- <50 items per category = negligible re-render cost
- No external state management library needed

**Implementation:**
```razor
<CascadingValue Value="Dashboard">
    <TimelinePanel />
    <StatusGrid />
    <MetricsFooter />
</CascadingValue>
```

---

### Decision 3: Component Hierarchy Depth

**Chosen:** 5 focused components (DashboardPage, TimelinePanel, StatusGrid, StatusColumn, MetricsFooter)

**Rationale:**
- Matches UI structure exactly
- Each component <80 lines (maintainable)
- Single responsibility principle
- Testable via Bunit
- Balanced between modularity and overhead

---

### Decision 4: Timeline Visualization

**Chosen:** Custom HTML/CSS layout using Bootstrap Grid

**Rationale:**
- Lightweight (2KB), fully accessible
- Screenshot-stable across browsers
- No external charting library overhead
- HTML semantic structure
- Supports 5-10 milestones fitting horizontally

**Implementation:** Flexbox timeline with status badges and color markers

---

### Decision 5: CSS Framework

**Chosen:** Bootstrap 5.3.0

**Rationale:**
- Screenshot consistency across Chrome, Edge, Firefox
- Built-in WCAG 2.1 AA accessibility
- Zero build step (matches local dotnet run requirement)
- 1500+ Bootstrap Icons integrated
- Executive-grade color palette customizable via site.css

**Alternatives Evaluated & Rejected:**
- Tailwind CSS: Requires PostCSS/Webpack build step
- Vanilla CSS: Highest maintenance burden, inconsistent spacing

---

### Decision 6: JSON Serialization

**Chosen:** System.Text.Json (built-in, .NET 8 source generators)

**Rationale:**
- Built into .NET 8.0 (zero extra NuGet dependency)
- 2-3x faster than Newtonsoft.Json
- Source-generated serializers (compile-time validation)
- Direct model binding to ProjectDashboard

**Alternative Rejected:**
- Newtonsoft.Json: Legacy, slower, maintenance-mode

---

### Decision 7: Testing Strategy

**Chosen:** Unit tests (xUnit) + Component tests (Bunit)

**Rationale:**
- Comprehensive coverage of services and components
- Maintainable test suite for Phase 2/3 iterations
- Standard .NET testing stack
- Establishes regression baseline

**Test Scope:**
- Unit: ProjectDataService, data models, validation
- Component: DashboardPage, TimelinePanel, StatusGrid, MetricsFooter

---

### Decision 8: Local Deployment

**Chosen:** `dotnet run` from source (default); optional publish.exe for non-developers

**Rationale:**
- Meets requirement: "run locally via dotnet run"
- Assumes IT-literate operator with .NET 8 SDK
- Fastest iteration for developers
- Optional publish for non-developer distribution

---

### Decision 9: Screenshot Consistency & Rendering

**Chosen:** Server-side rendering (Blazor Server default)

**Rationale:**
- Guaranteed consistent rendering across browsers
- No client-side variation (deterministic output)
- Meets business goal: "render identically across Chrome, Edge, Firefox"
- Pre-render static components for <500ms load time

---

### Decision 10: Performance Optimization

**Chosen:** Virtualization-ready architecture (Phase 2 implementation)

**Rationale:**
- Supports growth from 50 to 500+ items without architectural change
- Phase 1: Sufficient for <100 items per category
- Phase 2: Add `<Virtualize>` wrapper when needed
- Zero impact if dataset stays small

---

## Security Considerations

### Authentication & Authorization

**Status:** Not applicable

- Single-user local application; no multi-user access control required
- No login, no user roles, no API keys
- Phase 3 consideration: If network deployment needed, implement Windows Integrated Auth or OAuth

### Data Protection

#### At Rest
- **Strategy:** No encryption
- **Rationale:** Local filesystem assumed secure; operator responsible for OS-level file permissions
- **Mitigation:** Document in README that data.json should reside in restricted directory

#### In Transit
- **Strategy:** HTTPS optional (self-signed certificate auto-generated)
- **Rationale:** localhost-only, no network exposure; encryption not required
- **Configuration:** TLS 1.2+ available in appsettings.json if needed

### Input Validation

**JSON Schema Validation:**
```csharp
// System.Text.Json provides type safety
// Malformed JSON throws JsonException
// Required fields validated via PropertyName attributes
```

**Field-Level Validation:**
```csharp
public class ProjectDashboardValidator
{
    public static void Validate(ProjectDashboard? dashboard)
    {
        if (dashboard == null)
            throw new InvalidOperationException("Dashboard data is null");
        
        if (string.IsNullOrWhiteSpace(dashboard.ProjectName))
            throw new JsonException("ProjectName is required");
        
        foreach (var m in dashboard.Milestones ?? new())
        {
            if (!new[] { "Completed", "OnTrack", "AtRisk", "Delayed" }.Contains(m.Status))
                throw new JsonException($"Invalid milestone status: {m.Status}");
        }
        
        // Validate global work item ID uniqueness
        var allIds = new HashSet<string>();
        foreach (var item in (dashboard.Shipped ?? new())
            .Concat(dashboard.InProgress ?? new())
            .Concat(dashboard.CarriedOver ?? new()))
        {
            if (!allIds.Add(item.Id))
                throw new JsonException($"Duplicate WorkItem.Id: {item.Id}");
        }
    }
}
```

### Path Traversal Prevention

**Strategy:** Whitelist file paths; always use `Path.Combine()` with `ContentRootPath`

```csharp
// Safe
var dataPath = Path.Combine(env.ContentRootPath, "data", "data.json");

// Defense in depth: Verify resolved path within root
var resolvedPath = Path.GetFullPath(dataPath);
var resolvedRoot = Path.GetFullPath(env.ContentRootPath);

if (!resolvedPath.StartsWith(resolvedRoot, StringComparison.OrdinalIgnoreCase))
    throw new SecurityException($"Path traversal attempt detected");
```

### Sensitive Data Logging

**Strategy:** No sensitive data in logs; only load events and errors

```csharp
// Good
_logger.LogInformation("Dashboard data loaded from {Path}", dataPath);

// Bad (do not do)
_logger.LogInformation("Dashboard data: {@Dashboard}", dashboard); // Exposes all data
```

---

## Scaling Strategy

### Phase 1 (Current): Vertical Scaling - In-Memory Cache

**Constraints:**
- Single user, local machine only
- <100 work items per category
- <10 milestones
- ~500ms page load time
- <100MB resident memory

**Implementation:**
```csharp
public class ProjectDataService
{
    private ProjectDashboard? _cache; // Singleton in-memory cache
    
    public ProjectDashboard GetDashboard() => _cache!; // O(1) lookup
}
```

**Performance Profile:**
- Load time: 50-150ms (JSON parse + validation)
- Memory footprint: 2-5MB per dashboard
- Subsequent accesses: <1ms
- Suitable for: <500 items total

### Phase 2: Component Virtualization (>100 items/category)

**Implementation:**
```razor
<Virtualize Items="Items.OrderByDescending(i => i.CompletedDate).ToList()" 
           Context="item" 
           OverscanCount="5">
    <div class="card mb-2"><!-- card template --></div>
</Virtualize>
```

**Benefits:**
- Renders only visible DOM nodes (10-15 cards, not 500)
- Memory scales with viewport, not dataset
- Re-render latency <50ms even with 500+ items
- Smooth scrolling (no janky frame drops)

### Phase 3+: Horizontal Scaling (Network Deployment)

**Architecture Changes (if required):**
```
Current (Phase 1):
User → Kestrel (localhost) → data.json

Future (Phase 3):
Users (multiple) → Load Balancer → Multiple Kestrels → Shared Data Store
                     ├─ Windows Auth or OAuth
                     ├─ Circuit limits (SignalR)
                     ├─ Redis cache (distributed)
                     └─ Database (SQLite or SQL Server)
```

**Implementation (Phase 3, not MVP):**
```csharp
// Distributed cache via Redis
public class ProjectDataService
{
    private readonly IDistributedCache _cache;
    
    public async Task InitializeAsync()
    {
        var cached = await _cache.GetStringAsync("dashboard:data");
        if (!string.IsNullOrEmpty(cached))
        {
            _dashboard = JsonSerializer.Deserialize<ProjectDashboard>(cached);
            return;
        }
        
        _dashboard = await LoadFromSourceAsync();
        await _cache.SetStringAsync("dashboard:data", JsonSerializer.Serialize(_dashboard),
            new DistributedCacheEntryOptions { AbsoluteExpiration = TimeSpan.FromMinutes(5) });
    }
}
```

### Caching Strategy

| Layer | Mechanism | TTL | Phase |
|-------|-----------|-----|-------|
| Application | Singleton in-memory | Session lifetime | 1 |
| Static Assets | HTTP Cache-Control | 1 year (31536000s) | 1 |
| Data Refresh | File Watcher + SignalR | Manual + live | 2 |
| Distributed | Redis | 5 minutes | 3 |

**HTTP Static Asset Caching (Program.cs):**
```csharp
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.PhysicalPath;
        if (path?.EndsWith(".css") == true || path?.EndsWith(".woff2") == true)
        {
            ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }
    }
});
```

---

## Risks & Mitigations

### Risk Matrix

| Risk | Likelihood | Impact | Mitigation | Phase |
|------|-----------|--------|-----------|-------|
| JSON schema mismatch (missing fields) | Medium | Medium | ProjectDashboardValidator + typed C# models | 1 |
| File I/O failure at startup | Low | High | Try-catch + user-friendly error message + graceful degradation | 1 |
| Browser rendering inconsistency | Low | High | Bootstrap defaults + cross-browser testing | 1 |
| Performance degradation (>100 items/category) | Low | Medium | Virtualization in Phase 2 + load testing | 2 |
| Blazor circuit memory leak | Very Low | High | Circuit limits (DisconnectedCircuitMaxRetained: 100) | 1 |
| data.json manually corrupted | Low | High | Validation + clear error messaging + sample template | 1 |
| JSON deserialization slowdown | Very Low | Low | System.Text.Json (2-3x faster) | 1 |
| data.json file deleted/moved | Very Low | High | Error state + user corrects | 1 |
| Network deployment without auth | N/A | Critical | Windows Integrated Auth or OAuth | 3 |
| Multi-user concurrency issues | N/A | High | Circuit limits + load balancing | 3 |

---

### Technical Risks

#### Risk: JSON Deserialization Failures

**Scenario:** Operator edits data.json with syntax error; application fails to load

**Mitigation:**
1. Provide well-documented sample data.json with comments
2. Implement ProjectDashboardValidator for schema validation
3. Display user-friendly error message (not stack trace)
4. Log detailed error for debugging

**Implementation:**
```csharp
public async Task InitializeAsync()
{
    try
    {
        using var fs = File.OpenRead(_dataPath);
        _cache = await JsonSerializer.DeserializeAsync<ProjectDashboard>(fs, _options);
        ProjectDashboardValidator.Validate(_cache);
        _logger.LogInformation("✓ Dashboard loaded successfully");
    }
    catch (FileNotFoundException ex)
    {
        _lastError = $"Data file not found: {_dataPath}";
        _logger.LogError(ex, _lastError);
        throw;
    }
    catch (JsonException ex)
    {
        _lastError = $"JSON syntax error: {ex.Message}";
        _logger.LogError(ex, _lastError);
        throw;
    }
}
```

**User-Facing Message:**
```
Error Loading Dashboard
JSON syntax error: Missing comma on line 45. Check data.json for typos.
```

---

#### Risk: Component Re-render Storms

**Scenario:** Parent data change cascades through all children, causing excessive DOM updates

**Mitigation:**
1. Use immutable data (ProjectDashboard never mutated after load)
2. Limit cascade depth (5 components max)
3. Avoid inline event handlers

**Implementation:**
```razor
@* Good: Immutable data, no re-render unless reference changes *@
@code {
    [CascadingParameter] ProjectDashboard? Dashboard { get; set; }
    // No StateHasChanged() needed
}
```

---

#### Risk: Screenshot Inconsistency Across Browsers

**Scenario:** Dashboard renders differently in Chrome vs Firefox; screenshot unusable

**Mitigation:**
1. Use Bootstrap defaults (tested across browsers)
2. Minimize custom CSS
3. Test on target browsers before release

**Testing Checklist:**
- [ ] Windows: Chrome, Edge, Firefox
- [ ] macOS: Chrome, Safari, Firefox
- [ ] Linux: Chrome, Firefox
- [ ] 1920x1080 screenshot shows full dashboard
- [ ] Colors match Bootstrap palette
- [ ] No layout shifts during load

**Custom CSS Restrictions (site.css):**
```css
/* Good: Minimal overrides, no browser hacks */
:root {
    --bs-primary: #0066cc;
    --bs-success: #28a745;
}

/* Bad: Avoid */
.timeline { webkit-transform: scale(1); }
.card { filter: drop-shadow(...); } /* Use Bootstrap shadows instead */
```

---

### Dependency Risks

#### Risk: NuGet Package Vulnerabilities

**Mitigation:**
- Pin exact package versions (no floating semver)
- Run `dotnet list package --vulnerable` before release
- Update dependencies quarterly
- Target .NET 8.0 LTS (support until November 2026)

**Implementation (AgentSquad.Runner.csproj):**
```xml
<ItemGroup>
    <PackageReference Include="System.Text.Json" Version="8.0.1" />
    <PackageReference Include="Microsoft.AspNetCore.Components" Version="8.0.1" />
</ItemGroup>
```

---

#### Risk: Blazor Circuit Memory Leaks

**Scenario:** Unhandled exceptions or repeated navigations leak memory

**Mitigation:**
1. Implement circuit handler to log disconnections
2. Set DisconnectedCircuitMaxRetained = 100
3. Monitor circuit count via diagnostic endpoint

**Implementation (Program.cs):**
```csharp
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => 
    {
        options.DisconnectedCircuitMaxRetained = 100;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    });

#if DEBUG
app.Map("/health", async context =>
{
    var info = new
    {
        memoryUsageMb = GC.GetTotalMemory(false) / (1024 * 1024),
        timestamp = DateTime.UtcNow
    };
    await context.Response.WriteAsJsonAsync(info);
});
#endif
```

---

#### Risk: Large Dataset Performance Degradation

**Scenario:** Operator adds 500+ work items; dashboard becomes slow

**Mitigation:**
1. Monitor baseline performance with 50 items
2. Implement virtualization in Phase 2
3. Add performance warnings in README

**README Performance Section:**
```markdown
## Performance Notes

- Optimized for **<100 work items per category**
- 50 items/category: <500ms load, <50ms re-render
- 200+ items/category: Re-render latency may exceed 200ms
- **Phase 2:** Virtualization supports 500+ items

### Monitor Performance
1. F12 → Network tab
2. Measure DOMContentLoaded time
3. Expected: <500ms on local machine
```

---

### Operator/User Risks

#### Risk: Operator Overwrites data.json Accidentally

**Scenario:** Text editor saves with syntax error; dashboard fails to load

**Mitigation:**
1. Provide sample data.json with comments explaining structure
2. Display friendly error message with recovery steps
3. Recommend version control (Git) or backups

**Sample data.json Template (with comments):**
```json
{
  // Project name (required, non-empty)
  "projectName": "Project Alpha",
  
  // Start date (required, ISO 8601 format)
  "startDate": "2026-01-15T00:00:00Z",
  
  // Milestones (optional, but recommended 5-10)
  "milestones": [
    {
      "id": "m001",
      "name": "Design Review",
      "status": "Completed"
      // Must be one of: Completed, OnTrack, AtRisk, Delayed
    }
    // NOTE: Add comma between items, no trailing comma
  ]
}
```

---

### Dependency Drift Risk

**Scenario:** Months of inactivity; .NET 8 LTS nears end-of-life; libraries age

**Mitigation:**
1. Document dependency sunset dates
2. Plan annual dependency update pass
3. Monitor .NET 8 LTS timeline

**Dependency Tracker (README):**
```
| Dependency | Version | EOL Date | Action |
|------------|---------|----------|--------|
| .NET | 8.0 LTS | Nov 2026 | Plan migration to .NET 9 by Oct 2026 |
| Bootstrap | 5.3.0 | Ongoing | Update quarterly |
| System.Text.Json | Built-in | Nov 2026 | Included with .NET 8 |
```

---

## Summary: Mitigation Priority

**Phase 1 (MVP, High Priority):**
- ✓ Schema validation (ProjectDashboardValidator)
- ✓ Path traversal prevention (Path.Combine + ContentRootPath)
- ✓ Input validation (JSON deserialization + typed models)
- ✓ Error handling (user-friendly messages)
- ✓ Logging (no sensitive data)
- ✓ Component immutability (prevent re-render storms)

**Phase 2 (Medium Priority):**
- Component virtualization (if >100 items/category)
- File watcher + hot-reload
- Cross-browser testing suite
- Performance profiling

**Phase 3+ (Low Priority, Future):**
- Network deployment & authentication
- Load balancing & distributed caching
- Multi-user circuit limits
- Database backend migration

---

## Implementation Roadmap

### Phase 1 (MVP): 2-3 Business Days

Deliverables:
- ProjectDashboard, Milestone, WorkItem, ProgressMetrics data models
- ProjectDataService singleton with startup JSON loading
- 5 core components (DashboardPage, TimelinePanel, StatusGrid, StatusColumn, MetricsFooter)
- Bootstrap 5.3.0 + Bootstrap Icons 1.11.3
- Custom site.css with executive color palette
- Sample data.json with fictional project (5-10 milestones, 30-50 work items)
- xUnit unit tests (ProjectDataService, models)
- Bunit component tests (DashboardPage, TimelinePanel, StatusGrid, MetricsFooter)
- README.md with usage instructions and data.json schema documentation

### Phase 2 (Polish): 1-2 Days

Deliverables:
- File watcher + SignalR HubConnection for live data refresh
- Virtualize wrapper for 100+ items per category
- Screenshot testing suite (automated browser comparisons)
- WCAG 2.1 accessibility audit (axe DevTools, keyboard navigation)
- Performance profiling (Lighthouse, SignalR circuit memory)

### Phase 3 (Future): Out of Scope

Deliverables (if requirement emerges):
- Multi-project dashboard selector
- PDF/image export pipeline
- Database backend (SQLite or SQL Server)
- Network deployment with Windows authentication
- REST API endpoints

---

## Success Criteria

**Delivery Metrics:**
- ✓ Dashboard built and deployed in 2-3 days (MVP)
- ✓ Zero compile-time errors; all unit and integration tests pass
- ✓ Application runs locally with `dotnet run` without external service setup

**Functional Metrics:**
- ✓ All 7 user stories completed and verified
- ✓ Sample data.json loads without deserialization errors
- ✓ Dashboard displays all milestone and work-item data without truncation
- ✓ Metrics footer shows calculated health score and completion % matching JSON

**Non-Functional Metrics:**
- ✓ Page load time <500ms (measured via browser DevTools on local machine)
- ✓ Screenshot identical across Chrome, Edge, Firefox (pixel comparison)
- ✓ All text meets WCAG AA contrast ratio (4.5:1 for normal, 3:1 for large)
- ✓ Keyboard navigation through all elements (Tab, Shift+Tab, Enter)
- ✓ Build time <5 seconds on clean checkout

**Business Metrics:**
- ✓ Executive generates PowerPoint-ready screenshot within 30 seconds of dashboard launch
- ✓ No post-processing or image editing required before pasting into presentation
- ✓ Single-page view accommodates typical review (<50 items per category) without scrolling
- ✓ Dashboard ready for weekly/monthly stakeholder updates with zero manual data entry