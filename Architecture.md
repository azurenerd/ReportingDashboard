# Architecture

## Overview & Goals

**Project Name:** Executive Reporting Dashboard  
**Technology Stack:** C# .NET 8 with Blazor Server, Entity Framework Core, SQLite, Bootstrap 5.3, Chart.js 4.4+

### Purpose
Build a lightweight, single-page executive reporting dashboard that visualizes project milestones, progress metrics, and work item status. The dashboard reads configuration from a JSON file, persists data to a local SQLite database, and renders clean, screenshot-ready visuals optimized for PowerPoint presentations. The system eliminates manual status report creation by providing real-time, automated metrics aggregation and executive-friendly visualization.

### Core Goals
1. **Reduce reporting overhead:** Automate 80% of status report creation through programmatic dashboard generation and screenshot capture.
2. **Provide single source of truth:** Centralize all project metrics (milestones, progress, work items) in one location, eliminating conflicting reports.
3. **Enable rapid iteration:** Keep architecture simple and focused on visual presentation, enabling quick UI refinement based on stakeholder feedback.
4. **Minimize operational complexity:** Deploy locally with zero cloud dependencies, no authentication infrastructure, and no enterprise security overhead.
5. **Ensure screenshot readiness:** Design all components for direct embedding in executive presentations without redaction or manual formatting.

### Key Constraints
- **No authentication or authorization** required (local, trusted environment).
- **Single project display** at a time (no multi-project rollup in MVP).
- **Read-only work items** in MVP (no edit, create, or delete).
- **Local deployment only** (Windows or Linux with .NET 8 runtime).
- **Exactly four work item statuses:** New, In Progress, Shipped, Carried Over.
- **Exactly four milestone statuses:** Planned, In Progress, Completed, Delayed.

---

## System Components

### 1. **Blazor Server Application (AgentSquad.Runner)**
**Responsibility:** Host interactive web application, manage component lifecycle, coordinate real-time data binding.

**Key Responsibilities:**
- Start HTTP server and initialize Blazor Server runtime on port 5000 (development) or 80/443 (production).
- Load Program.cs configuration: register services (ProjectDataService, ReportingService, MilestoneService) in dependency injection container.
- Initialize ApplicationDbContext, apply EF Core migrations on startup.
- Load and cache data.json in memory via ProjectDataService.
- Render Dashboard.razor and child components with Bootstrap styling.

**Interfaces:**
- **Startup Handler:** `Program.cs` → `app.Services.GetRequiredService<IProjectDataService>().InitializeAsync()`
- **Dependency Injection:** All services registered in `services.AddScoped<IProjectDataService>()`, etc.

**Dependencies:**
- Entity Framework Core 8.0
- Bootstrap 5.3 (via CDN)
- Chart.js 4.4+ (via CDN)
- System.Text.Json (built-in)

**Technology:** Blazor Server (.NET 8)

---

### 2. **Dashboard.razor (Primary Component)**
**Responsibility:** Render the main executive dashboard layout, orchestrate child components, handle user interactions.

**Key Responsibilities:**
- Inject ReportingService, MilestoneService, and ProjectDataService.
- Call ReportingService on component initialization to load metrics (% complete, shipped count, carry-over count).
- Render header section: Project name, start date, target end date.
- Render key metrics cards: completion percentage, shipped items, carried-over items.
- Render milestone timeline via Chart.js (delegated to MilestoneTimeline.razor).
- Render work items table/cards organized by status.
- Handle filtering: by status, by milestone (update work items list on selection).
- Call `StateHasChanged()` after filter changes to re-render.

**Layout Structure:**
```
<div class="container-fluid mt-5 mb-5">
  <!-- Header: Project name, dates, key metrics -->
  <DashboardHeader Project="@project" />
  
  <!-- Key metrics cards -->
  <MetricsRow Metrics="@metrics" />
  
  <!-- Milestone timeline -->
  <MilestoneTimeline Milestones="@milestones" />
  
  <!-- Filtering controls -->
  <FilterControls OnStatusFilterChanged="@HandleStatusFilter" 
                   OnMilestoneFilterChanged="@HandleMilestoneFilter" />
  
  <!-- Work items by status -->
  <WorkItemsGrid WorkItems="@filteredWorkItems" />
</div>
```

**Data Binding:**
- `@project` (Project object): loaded from ReportingService
- `@metrics` (DashboardMetrics object): aggregated values (completion %, shipped, carry-over)
- `@milestones` (List<Milestone>): filtered by date range
- `@filteredWorkItems` (List<WorkItem>): filtered by status and/or milestone selection
- `@selectedStatusFilter` (string): current status filter ("All", "New", "In Progress", "Shipped", "Carried Over")
- `@selectedMilestoneFilter` (Guid?): selected milestone ID or null

**Lifecycle:**
- `OnInitializedAsync()`: Call `ReportingService.GetDashboardMetricsAsync()` to load data.
- User interaction (filter click) → call `HandleStatusFilter()` or `HandleMilestoneFilter()` → update local state → call `StateHasChanged()` → re-render.

**CSS Classes:**
- `container-fluid`: Bootstrap responsive container.
- `mt-5 mb-5`: margin top/bottom for spacing.
- `row`, `col-md-4`: Bootstrap grid for metrics cards.

---

### 3. **MilestoneTimeline.razor (Child Component)**
**Responsibility:** Render horizontal milestone timeline with status indicators and progress bars using Chart.js.

**Key Responsibilities:**
- Accept `Milestones` parameter (List<Milestone>).
- Call Chart.js via JavaScript interop to render horizontal bar chart.
- Display milestone name, scheduled date, and completion percentage.
- Color-code by status: green (Completed), yellow (In Progress), red (Delayed), gray (Planned).
- Render progress bars under each milestone.
- Handle milestone click events (optional: drill down to related work items).

**JavaScript Interop:**
```csharp
[Inject] IJSRuntime JS { get; set; }

private async Task InitializeChart()
{
    await JS.InvokeVoidAsync("initMilestoneChart", canvasId, chartData);
}
```

**Chart.js Configuration:**
- Chart type: `bar` (horizontal bar chart for milestones).
- X-axis: milestone names.
- Y-axis: completion percentage (0-100%).
- Colors by status:
  - Completed: `#28a745` (Bootstrap success/green)
  - In Progress: `#ffc107` (Bootstrap warning/yellow)
  - Delayed: `#dc3545` (Bootstrap danger/red)
  - Planned: `#6c757d` (Bootstrap secondary/gray)

**Data Model (passed to Chart.js):**
```json
{
  "labels": ["Milestone 1", "Milestone 2", ...],
  "datasets": [
    {
      "label": "Completion %",
      "data": [75, 100, 50, 0],
      "backgroundColor": ["#ffc107", "#28a745", "#dc3545", "#6c757d"]
    }
  ]
}
```

---

### 4. **ReportingService (Business Logic)**
**Responsibility:** Aggregate executive metrics from SQLite, compute KPIs, return strongly-typed dashboard data.

**Key Responsibilities:**
- Query ApplicationDbContext for project, milestones, work items.
- Compute metrics:
  - **Completion %:** (items with status "Shipped" + items with status "In Progress" partially) / total items.
  - **Shipped count:** Count work items with status = "Shipped".
  - **Carry-over count:** Count work items with status = "Carried Over".
- Return DashboardMetrics object.
- Cache results in memory; invalidate on explicit refresh.
- Use `.Include()` to avoid N+1 queries.

**Interface:**
```csharp
public interface IReportingService
{
    Task<DashboardMetrics> GetDashboardMetricsAsync(Guid projectId);
    Task<List<Milestone>> GetMilestonesAsync(Guid projectId);
    Task<List<WorkItem>> GetWorkItemsAsync(Guid projectId, string? statusFilter, Guid? milestoneFilter);
}

public class DashboardMetrics
{
    public decimal CompletionPercentage { get; set; }
    public int ShippedCount { get; set; }
    public int CarriedOverCount { get; set; }
}
```

**Implementation Notes:**
- Use EntityFramework Logging to diagnose slow queries.
- Implement query optimization: `.Include(p => p.WorkItems).Include(p => p.Milestones)`.
- Cache DashboardMetrics for 1 minute; refresh on demand.

---

### 5. **ProjectDataService (Data Loading & Caching)**
**Responsibility:** Load data from data.json file, seed SQLite on startup, provide in-memory cache.

**Key Responsibilities:**
- On application startup, read data.json from wwwroot directory via System.Text.Json.
- Deserialize JSON into Project, Milestone, WorkItem objects.
- Validate JSON schema (check required fields, status enum values).
- If validation fails, log error and display user-friendly error message.
- Seed SQLite: check if database is empty; if so, insert Project, Milestones, WorkItems from data.json.
- Maintain in-memory cache of Project (Dictionary<Guid, Project>).
- Provide methods to retrieve cached data synchronously.
- On explicit refresh, reload data.json and update cache + SQLite.

**Interface:**
```csharp
public interface IProjectDataService
{
    Task InitializeAsync();
    Task<Project?> GetProjectAsync(Guid projectId);
    Task RefreshFromJsonAsync(Guid projectId);
}
```

**JSON Deserialization (System.Text.Json):**
```csharp
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var project = JsonSerializer.Deserialize<Project>(jsonContent, options);
```

**Error Handling:**
```csharp
if (project == null || project.Milestones == null || project.WorkItems == null)
{
    _logger.LogError("Invalid data.json: missing required fields");
    throw new InvalidOperationException("data.json is malformed");
}
```

---

### 6. **MilestoneService (Milestone Logic)**
**Responsibility:** Filter milestones by date range, compute completion percentage, handle milestone-work item relationships.

**Key Responsibilities:**
- Accept project ID and optional date range.
- Query ApplicationDbContext for milestones within date range.
- Compute milestone completion percentage: (work items with status "Shipped" or "In Progress") / total work items assigned to milestone.
- Return sorted list by scheduled date.
- Provide method to get work items for a specific milestone.

**Interface:**
```csharp
public interface IMilestoneService
{
    Task<List<Milestone>> GetMilestonesByDateRangeAsync(Guid projectId, DateTime? startDate, DateTime? endDate);
    Task<decimal> GetMilestoneCompletionPercentageAsync(Guid milestoneId);
    Task<List<WorkItem>> GetWorkItemsByMilestoneAsync(Guid milestoneId);
}
```

---

### 7. **ApplicationDbContext (Entity Framework Core)**
**Responsibility:** Manage database connection, define entity mappings, handle migrations.

**Key Responsibilities:**
- Define DbSet<Project>, DbSet<Milestone>, DbSet<WorkItem>.
- Configure SQLite connection string (file path in App_Data directory).
- Define entity relationships: Project → Milestones (1:N), Project → WorkItems (1:N), Milestone → WorkItems (1:N).
- Configure column mappings and constraints (e.g., status enums).
- Apply migrations on startup via `context.Database.Migrate()`.
- Configure lazy loading disabled (explicit `.Include()` required).

**Entity Configuration:**
```csharp
public DbSet<Project> Projects { get; set; }
public DbSet<Milestone> Milestones { get; set; }
public DbSet<WorkItem> WorkItems { get; set; }

protected override void OnConfiguring(DbContextOptionsBuilder options)
{
    options.UseSqlite("Data Source=App_Data/dashboard.db");
    options.UseLazyLoadingProxies(false);
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Define relationships, constraints, indexes
    modelBuilder.Entity<WorkItem>()
        .Property(w => w.Status)
        .HasConversion<string>()
        .HasMaxLength(50);
}
```

---

### 8. **Data Models (Project, Milestone, WorkItem)**
**Responsibility:** Represent domain entities with validation and type safety.

**Project Model:**
```csharp
public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime TargetEndDate { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<WorkItem> WorkItems { get; set; } = new();
}
```

**Milestone Model:**
```csharp
public class Milestone
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; }
    public DateTime ScheduledDate { get; set; }
    public MilestoneStatus Status { get; set; } // Planned, In Progress, Completed, Delayed
    public decimal CompletionPercentage { get; set; } // 0-100
}

public enum MilestoneStatus
{
    Planned,
    InProgress,
    Completed,
    Delayed
}
```

**WorkItem Model:**
```csharp
public class WorkItem
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? MilestoneId { get; set; }
    public string Title { get; set; }
    public WorkItemStatus Status { get; set; } // New, In Progress, Shipped, Carried Over
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? OwnerName { get; set; } // Optional
}

public enum WorkItemStatus
{
    New,
    InProgress,
    Shipped,
    CarriedOver
}
```

---

### 9. **JavaScript Interop (chart-interop.js)**
**Responsibility:** Provide Chart.js initialization and management.

**Key Responsibilities:**
- Expose `initMilestoneChart(canvasId, chartData)` function callable from Blazor.
- Create Chart.js instance with milestone data.
- Handle chart updates and destruction.
- Provide utility functions for color mapping by milestone status.

**Implementation:**
```javascript
window.dashboardCharts = {
    charts: {},
    
    initMilestoneChart: function(canvasId, chartData) {
        const ctx = document.getElementById(canvasId).getContext('2d');
        this.charts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: chartData,
            options: {
                responsive: true,
                indexAxis: 'y',
                scales: {
                    x: { max: 100 }
                }
            }
        });
    },
    
    destroyChart: function(canvasId) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
            delete this.charts[canvasId];
        }
    }
};
```

---

### 10. **Bootstrap Layout (_Layout.cshtml)**
**Responsibility:** Provide HTML structure, load CSS/JS libraries, define global styling.

**Key Responsibilities:**
- Define `<html>`, `<head>`, `<body>` structure.
- Load Bootstrap 5.3 CSS from CDN.
- Load Chart.js from CDN.
- Load Font Awesome 6.4 from CDN.
- Include custom dashboard.css.
- Define `@Body` placeholder for component rendering.
- Set viewport meta tags for responsive design.

**CDN Links:**
- Bootstrap: `https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css`
- Chart.js: `https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js`
- Font Awesome: `https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css`

---

### 11. **dashboard.css (Custom Styling)**
**Responsibility:** Define dashboard-specific styles for screenshot readiness.

**Key Responsibilities:**
- Set professional color palette: success (green), warning (yellow), danger (red), secondary (gray).
- Define metrics card styling: large number, small label.
- Define status badge colors: `.badge-success`, `.badge-warning`, `.badge-danger`.
- Define progress bar styling.
- Ensure readable font sizes (14pt minimum body, 18pt+ headers).
- Set whitespace and grid alignment for executive readability.
- No animations or floating elements (optimize for static screenshots).

**Key Styles:**
```css
.metrics-card {
    background: white;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    padding: 20px;
    text-align: center;
}

.metrics-card .metric-value {
    font-size: 2.5rem;
    font-weight: bold;
    color: #333;
}

.metrics-card .metric-label {
    font-size: 0.875rem;
    color: #999;
    margin-top: 10px;
}

.status-badge-shipped { background-color: #28a745; }
.status-badge-in-progress { background-color: #ffc107; color: black; }
.status-badge-new { background-color: #17a2b8; }
.status-badge-carried-over { background-color: #dc3545; }
```

---

## Component Interactions

### Data Flow: Startup Sequence
```
1. Program.cs: Startup
   ├─ Register services in DI container
   ├─ Configure ApplicationDbContext
   └─ Build and run WebApplication

2. _Layout.cshtml: Render
   ├─ Load Bootstrap, Chart.js, Font Awesome from CDN
   └─ Load dashboard.css

3. Dashboard.razor: Component Initialization
   ├─ Call ReportingService.GetDashboardMetricsAsync()
   ├─ Call MilestoneService.GetMilestonesAsync()
   ├─ Call ReportingService.GetWorkItemsAsync()
   └─ Render child components (MilestoneTimeline, WorkItemsGrid)

4. MilestoneTimeline.razor: Render Chart
   ├─ Accept @Milestones parameter
   ├─ Call JS.InvokeVoidAsync("initMilestoneChart", ...)
   └─ Chart.js renders milestone bar chart

5. ProjectDataService: Background (Initialization)
   ├─ Read data.json from wwwroot/
   ├─ Deserialize and validate
   ├─ Check if SQLite is empty
   └─ If empty, seed with Project, Milestones, WorkItems
```

### Data Flow: User Filters Work Items
```
1. User clicks status filter (e.g., "Shipped")
   └─ Dashboard.razor: OnStatusFilterChanged()

2. Dashboard.razor:
   ├─ Update @selectedStatusFilter = "Shipped"
   ├─ Call ReportingService.GetWorkItemsAsync(projectId, "Shipped", null)
   ├─ Update @filteredWorkItems
   └─ Call StateHasChanged()

3. Blazor re-renders:
   └─ WorkItemsGrid.razor displays filtered items only
```

### Data Flow: Milestone Completion Update
```
1. External trigger (data.json updated, or dashboard refresh triggered)
   └─ Call ProjectDataService.RefreshFromJsonAsync()

2. ProjectDataService:
   ├─ Reload data.json
   ├─ Update SQLite (INSERT/UPDATE/DELETE)
   └─ Invalidate in-memory cache

3. Dashboard.razor:
   ├─ Call ReportingService.GetDashboardMetricsAsync() again
   ├─ Metrics re-calculated from fresh SQLite data
   └─ Call StateHasChanged() to re-render with new values
```

### API-like Interactions (No HTTP, Blazor DI only)
```
Dashboard.razor
├─ Injects IReportingService
├─ Injects IMilestoneService
├─ Injects IProjectDataService

MilestoneTimeline.razor
├─ Injects IJSRuntime (for Chart.js)

ReportingService
├─ Injects ApplicationDbContext
├─ Injects ILogger<ReportingService>

ProjectDataService
├─ Injects IConfiguration (for file paths)
├─ Injects ILogger<ProjectDataService>
```

---

## Data Model

### Entity-Relationship Diagram
```
Project (1)
├─ id (GUID, PK)
├─ name (string)
├─ startDate (DateTime)
├─ targetEndDate (DateTime)
└── (1:N) → Milestone
└── (1:N) → WorkItem

Milestone (N)
├─ id (GUID, PK)
├─ projectId (GUID, FK → Project.id)
├─ name (string)
├─ scheduledDate (DateTime)
├─ status (enum: Planned, InProgress, Completed, Delayed)
├─ completionPercentage (decimal 0-100)
└── (1:N) → WorkItem (optional)

WorkItem (N)
├─ id (GUID, PK)
├─ projectId (GUID, FK → Project.id)
├─ milestoneId (GUID?, FK → Milestone.id, nullable)
├─ title (string)
├─ status (enum: New, InProgress, Shipped, CarriedOver)
├─ createdDate (DateTime)
├─ completedDate (DateTime?, nullable)
└─ ownerName (string?, nullable)
```

### SQLite Schema (EF Core Migrations)
```sql
CREATE TABLE Projects (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    StartDate DATETIME NOT NULL,
    TargetEndDate DATETIME NOT NULL
);

CREATE TABLE Milestones (
    Id TEXT PRIMARY KEY,
    ProjectId TEXT NOT NULL REFERENCES Projects(Id),
    Name TEXT NOT NULL,
    ScheduledDate DATETIME NOT NULL,
    Status TEXT NOT NULL CHECK(Status IN ('Planned', 'InProgress', 'Completed', 'Delayed')),
    CompletionPercentage REAL NOT NULL DEFAULT 0
);

CREATE TABLE WorkItems (
    Id TEXT PRIMARY KEY,
    ProjectId TEXT NOT NULL REFERENCES Projects(Id),
    MilestoneId TEXT REFERENCES Milestones(Id),
    Title TEXT NOT NULL,
    Status TEXT NOT NULL CHECK(Status IN ('New', 'InProgress', 'Shipped', 'CarriedOver')),
    CreatedDate DATETIME NOT NULL,
    CompletedDate DATETIME,
    OwnerName TEXT
);

CREATE INDEX idx_workitems_projectid ON WorkItems(ProjectId);
CREATE INDEX idx_workitems_status ON WorkItems(Status);
CREATE INDEX idx_milestones_projectid ON Milestones(ProjectId);
CREATE INDEX idx_milestones_status ON Milestones(Status);
```

### data.json Schema
```json
{
  "project": {
    "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "name": "Project Alpha",
    "startDate": "2026-01-01T00:00:00Z",
    "targetEndDate": "2026-12-31T00:00:00Z"
  },
  "milestones": [
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "name": "Phase 1: Planning",
      "scheduledDate": "2026-02-28T00:00:00Z",
      "status": "Completed",
      "completionPercentage": 100
    }
  ],
  "workItems": [
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "milestoneId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "title": "Define requirements",
      "status": "Shipped",
      "createdDate": "2026-01-01T00:00:00Z",
      "completedDate": "2026-02-15T00:00:00Z",
      "ownerName": "Alice Smith"
    }
  ]
}
```

### Computed Fields
- **DashboardMetrics.CompletionPercentage:** (WorkItems with Status="Shipped" or Status="InProgress") / Total WorkItems × 100
- **DashboardMetrics.ShippedCount:** COUNT(WorkItems WHERE Status="Shipped")
- **DashboardMetrics.CarriedOverCount:** COUNT(WorkItems WHERE Status="CarriedOver")
- **Milestone.CompletionPercentage:** (WorkItems with MilestoneId AND Status="Shipped") / (WorkItems with MilestoneId) × 100

---

## API Contracts

**Note:** Blazor Server does not expose traditional REST APIs. Instead, components call services directly via dependency injection. However, the following interfaces define method contracts:

### IReportingService
```csharp
public interface IReportingService
{
    /// <summary>
    /// Aggregates executive metrics for a project.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>DashboardMetrics with completion %, shipped count, carry-over count</returns>
    Task<DashboardMetrics> GetDashboardMetricsAsync(Guid projectId);
    
    /// <summary>
    /// Gets all milestones for a project.
    /// </summary>
    Task<List<Milestone>> GetMilestonesAsync(Guid projectId);
    
    /// <summary>
    /// Gets work items with optional filtering by status and/or milestone.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="statusFilter">Null for all, or specific status (e.g., "Shipped")</param>
    /// <param name="milestoneFilter">Null for all, or specific milestone ID</param>
    Task<List<WorkItem>> GetWorkItemsAsync(Guid projectId, string? statusFilter, Guid? milestoneFilter);
}

public class DashboardMetrics
{
    public decimal CompletionPercentage { get; set; } // 0-100
    public int ShippedCount { get; set; }
    public int CarriedOverCount { get; set; }
    public int TotalWorkItems { get; set; }
    public int InProgressCount { get; set; }
    public int NewCount { get; set; }
}
```

### IProjectDataService
```csharp
public interface IProjectDataService
{
    /// <summary>
    /// Initializes the application: loads data.json and seeds SQLite if needed.
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// Retrieves a project from cache.
    /// </summary>
    Task<Project?> GetProjectAsync(Guid projectId);
    
    /// <summary>
    /// Reloads data from data.json and updates SQLite and cache.
    /// </summary>
    Task RefreshFromJsonAsync();
    
    /// <summary>
    /// Returns the currently loaded project (singleton for MVP).
    /// </summary>
    Task<Project?> GetCurrentProjectAsync();
}
```

### IMilestoneService
```csharp
public interface IMilestoneService
{
    /// <summary>
    /// Gets milestones filtered by date range.
    /// </summary>
    Task<List<Milestone>> GetMilestonesByDateRangeAsync(Guid projectId, DateTime? startDate, DateTime? endDate);
    
    /// <summary>
    /// Computes completion percentage for a milestone.
    /// </summary>
    Task<decimal> GetMilestoneCompletionPercentageAsync(Guid milestoneId);
    
    /// <summary>
    /// Gets all work items assigned to a milestone.
    /// </summary>
    Task<List<WorkItem>> GetWorkItemsByMilestoneAsync(Guid milestoneId);
}
```

### Razor Component Parameters (Event Handlers)
```csharp
// In Dashboard.razor
private async Task HandleStatusFilter(string? status)
{
    selectedStatusFilter = status;
    filteredWorkItems = await ReportingService.GetWorkItemsAsync(projectId, status, null);
    StateHasChanged();
}

private async Task HandleMilestoneFilter(Guid? milestoneId)
{
    selectedMilestoneFilter = milestoneId;
    filteredWorkItems = await ReportingService.GetWorkItemsAsync(projectId, null, milestoneId);
    StateHasChanged();
}
```

### Error Handling
- **InvalidOperationException:** Thrown if data.json is malformed or SQLite initialization fails.
- **FileNotFoundException:** Thrown if data.json is missing from wwwroot/.
- **ArgumentException:** Thrown if invalid status or milestone ID is provided.
- **JsonException:** Thrown if JSON deserialization fails (caught and logged in ProjectDataService).

All exceptions are logged via ILogger<T>. User-facing errors are displayed as toast notifications or error messages in the UI.

---

## Infrastructure Requirements

### Hosting Environment
- **Operating System:** Windows 10/11, Windows Server 2019+, or Linux (Ubuntu 20.04+, RHEL 8+, Debian 11+)
- **.NET Runtime:** .NET 8.0 LTS (8.0.203+)
- **RAM:** Minimum 512 MB (typical 1-2 GB for comfortable operation)
- **Disk:** 200 MB free space for .NET SDK + dependencies + SQLite database
- **Network:** Local network or loopback (no internet required for operation; CDN access required for initial load)

### Port Configuration
- **HTTP:** Port 5000 (development, localhost)
- **HTTPS:** Port 5001 (development, localhost)
- **Production:** Port 80 (HTTP) or 443 (HTTPS) via IIS or reverse proxy

### File System Layout
```
C:\Projects\AgentSquad\
├── AgentSquad.Runner.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── App_Data/
│   └── dashboard.db (SQLite, created on startup if missing)
├── Models/
│   ├── Project.cs
│   ├── Milestone.cs
│   ├── WorkItem.cs
│   └── DashboardViewModel.cs
├── Services/
│   ├── IProjectDataService.cs
│   ├── ProjectDataService.cs
│   ├── IReportingService.cs
│   ├── ReportingService.cs
│   ├── IMilestoneService.cs
│   └── MilestoneService.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Migrations/
│       ├── 20260401000000_InitialCreate.cs
│       └── ...
├── Components/
│   ├── Dashboard.razor
│   ├── MilestoneTimeline.razor
│   ├── ProgressCard.razor
│   ├── FilterControls.razor
│   ├── WorkItemsGrid.razor
│   └── _Layout.cshtml
└── wwwroot/
    ├── data.json
    ├── css/
    │   └── dashboard.css
    └── js/
        └── chart-interop.js
```

### Dependencies (NuGet Packages)
```
Microsoft.EntityFrameworkCore 8.0.4
Microsoft.EntityFrameworkCore.Sqlite 8.0.4
Microsoft.EntityFrameworkCore.Tools 8.0.4
System.Text.Json (built-in with .NET 8)
Microsoft.Extensions.Logging (built-in)
Microsoft.Extensions.Configuration (built-in)
Microsoft.Extensions.DependencyInjection (built-in)
```

### Database Initialization
```csharp
// In Program.cs or Startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    
    var projectDataService = scope.ServiceProvider.GetRequiredService<IProjectDataService>();
    await projectDataService.InitializeAsync();
}
```

### Configuration (appsettings.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=App_Data/dashboard.db"
  },
  "Dashboard": {
    "DataJsonPath": "wwwroot/data.json"
  }
}
```

### CI/CD (Optional, Deferred to Phase 3)
- **Build:** `dotnet build -c Release`
- **Publish:** `dotnet publish -c Release -o .\publish`
- **Deployment:** Copy publish folder to server; run via `dotnet AgentSquad.Runner.dll` or Windows Service

### Backup & Recovery
- **Database Backup:** Daily copy of App_Data/dashboard.db to network share.
- **Configuration Backup:** Version control appsettings.json and data.json in Git.
- **Recovery Procedure:** Restore backup .db file, restart application.

### Performance Baselines
- **Application Startup:** <5 seconds
- **Dashboard Load:** <2 seconds
- **Metrics Query:** <100 ms (up to 10K work items)
- **Chart.js Render:** <500 ms (up to 50 milestones)
- **Component Re-render:** <300 ms after filter change

---

## Technology Stack Decisions

### Frontend: Blazor Server (vs. Alternatives)

**Choice:** Blazor Server  
**Rationale:**
- **Code Sharing:** Write C# for both backend and frontend; eliminate context-switching between C# and JavaScript.
- **Type Safety:** Strongly-typed component parameters and event handlers prevent runtime errors.
- **Real-time Binding:** Built-in two-way data binding via `@bind` directive; no need for React/Vue state management.
- **No Build Step:** Blazor Server compiles to IL; no Webpack/Babel required.
- **Server-Side Logic:** Business logic runs on server; no risk of reverse-engineering client-side code.

**Alternatives Rejected:**
- **Blazor WebAssembly:** Requires API endpoints and adds deployment complexity; overkill for single-page dashboard.
- **React/Vue:** JavaScript context-switching, build pipeline overhead, steeper learning curve for .NET team.
- **ASP.NET MVC:** Less interactive; would require full-page refreshes on filter changes.

### Charting: Chart.js (vs. Alternatives)

**Choice:** Chart.js 4.4+  
**Rationale:**
- **Lightweight:** ~10 KB minified; no bloat compared to Syncfusion or Telerik.
- **Clean API:** Simple configuration for bar charts, timelines, and progress visualization.
- **Accessibility:** Version 4.4+ includes ARIA labels and keyboard navigation.
- **Visual Quality:** Produces professional, screenshot-ready charts suitable for PowerPoint.
- **JavaScript Interop:** Integrates cleanly with Blazor via JSRuntime.

**Alternatives Rejected:**
- **Syncfusion Charts:** Enterprise pricing, licensing complexity, unnecessary features for MVP.
- **Telerik Chart:** Similar to Syncfusion; excessive cost and complexity.
- **Canvas Drawing (HTML5):** Manual rendering overhead; Chart.js abstracts this away.
- **SVG:** No interactive tooltips; Chart.js handles interactivity out-of-the-box.

### Database: SQLite (vs. Alternatives)

**Choice:** SQLite 3.45+  
**Rationale:**
- **Zero Configuration:** Single .db file, no server setup required.
- **Local Deployment:** No cloud infrastructure, no credentials, no network latency.
- **ACID Compliance:** Ensures data consistency for financial and milestone data.
- **Performance:** <100 ms queries for up to 100K work items with proper indexing.
- **Simplicity:** Embedded in .NET; no separate database server to manage.

**Alternatives Rejected:**
- **SQL Server:** Over-engineered for single-page dashboard; requires licensing and infrastructure.
- **PostgreSQL:** Overkill; adds operational overhead (server setup, administration).
- **MySQL:** Similar to PostgreSQL; unnecessary complexity for local deployment.
- **NoSQL (MongoDB, Cosmos):** Schemaless design doesn't fit strongly-typed domain model.
- **File-based JSON:** No indexing, slow queries for large datasets.

### ORM: Entity Framework Core 8.0 (vs. Alternatives)

**Choice:** Entity Framework Core 8.0  
**Rationale:**
- **Strongly-Typed:** Query results are C# objects, not dynamic or untyped dictionaries.
- **Automatic Migrations:** Track schema changes via code-first migrations.
- **LINQ Queries:** Expressive, type-safe queries with IntelliSense support.
- **Lazy Loading Control:** Explicitly disable lazy loading to avoid N+1 queries.
- **Built-in Logging:** Diagnose slow queries via EF logging.

**Alternatives Rejected:**
- **Dapper:** Micro-ORM; good for simple queries, but less abstraction for complex relationships.
- **Raw SQL:** String concatenation risk; no type safety.
- **LINQ to SQL:** Deprecated; EF Core is the official successor.

### CSS Framework: Bootstrap 5.3 (vs. Alternatives)

**Choice:** Bootstrap 5.3  
**Rationale:**
- **Responsive Grid:** Mobile-friendly layouts out-of-the-box; executive views scale to mobile devices.
- **Pre-built Components:** Badges, progress bars, modals, alerts reduce custom CSS.
- **Accessibility:** WCAG AA compliant utilities and components.
- **CDN Availability:** No npm install required; instant deployment.
- **Professional Aesthetic:** Clean, modern design suitable for executive presentations.

**Alternatives Rejected:**
- **Tailwind CSS:** Utility-first approach requires more custom styling; overkill for dashboard.
- **Material Design:** Heavier framework; adds unnecessary complexity.
- **Custom CSS:** Time-consuming; Bootstrap provides battle-tested layouts.

### JSON Configuration: System.Text.Json (vs. Alternatives)

**Choice:** System.Text.Json (built-in .NET)  
**Rationale:**
- **Zero Dependencies:** Built-in to .NET 8; no external library required.
- **Performance:** Faster than Newtonsoft.Json for most use cases.
- **Modern API:** Async support, source generation for compile-time optimization.
- **Simplicity:** Straightforward deserialization of data.json to strongly-typed models.

**Alternatives Rejected:**
- **Newtonsoft.Json (Json.NET):** Older, slower; System.Text.Json is the modern standard.
- **Manual Parsing:** Error-prone; lose type safety.

### Testing: xUnit (vs. Alternatives)

**Choice:** xUnit 2.6+  
**Rationale:**
- **.NET Native:** Designed for .NET; excellent tooling in Visual Studio.
- **Fluent Assertions:** Easy-to-read assertion syntax (with Fluent Assertions library).
- **Parallelization:** Tests run in parallel by default; faster feedback loops.
- **Traits:** Organize tests by category; skip tests by trait.

**Alternatives Rejected:**
- **NUnit:** Older; xUnit is newer and preferred by .NET community.
- **MSTest:** Tied to Visual Studio; less flexibility.

---

## Security Considerations

### Authentication & Authorization
- **No Login Required:** Per requirements, dashboard is in a trusted local environment.
- **Session State (Optional):** If future role-based access needed, use Blazor session state:
  ```csharp
  [Inject] protected ProtectedSessionStorage sessionStorage { get; set; }
  
  private async Task SetUserRole(string role)
  {
      await sessionStorage.SetAsync("userRole", role);
  }
  ```
- **Configuration-Based Roles:** Read roles from appsettings.json; no database lookups.

### Data Protection
- **No PII in Dashboard UI:** Project names, milestone titles, and work item summaries are assumed non-sensitive.
- **No Credentials in Code:** Use appsettings.json for configuration, not hardcoded strings.
- **Database Encryption (Optional):** If sensitive financial data is stored:
  - Use SQLCipher for file-level encryption.
  - Store encryption key in environment variable (not Git).
  - Enable file-level OS permissions (restrict read/write to admin).

### Input Validation
- **JSON Schema Validation:** Validate data.json structure on load:
  ```csharp
  if (project?.Milestones == null || project.WorkItems == null)
      throw new InvalidOperationException("Invalid data.json");
  ```
- **Enum Validation:** Ensure status values match defined enums (Planned, InProgress, etc.).
- **Date Validation:** Ensure TargetEndDate > StartDate; warn if milestone date is outside project range.

### XSS Prevention
- **Razor Syntax Escaping:** Blazor automatically HTML-encodes `@` expressions.
  ```razor
  <div>@project.Name</div> <!-- Automatically escaped -->
  ```
- **No innerHTML:** Never use `@Html.Raw()` with user-supplied data.
- **Chart.js Data:** Passed as JSON, not HTML string; no XSS risk.

### CSRF Protection
- **Blazor Anti-Forgery:** Built-in to Blazor Server; tokens are generated per session.
  ```csharp
  [ValidateAntiForgeryToken]
  public async Task UpdateWorkItem(WorkItem item) { ... }
  ```
- **No External Forms:** Dashboard is read-only in MVP; form protection deferred.

### HTTPS & Transport Security
- **Development:** HTTP on localhost:5000 is acceptable for local development.
- **Production (Local Network):** Enable HTTPS:
  ```csharp
  app.UseHsts();
  app.UseHttpsRedirection();
  ```
- **Self-Signed Certificates:** Use for local network:
  ```bash
  dotnet dev-certs https --trust
  ```

### CDN Security
- **Subresource Integrity (SRI):** Verify integrity of third-party scripts:
  ```html
  <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"
      integrity="sha384-..."
      crossorigin="anonymous"></script>
  ```
- **Use HTTPS:** All CDN links must use HTTPS; no mixed content.

### Logging & Monitoring
- **Avoid Logging PII:** Log errors and metrics, not user data.
  ```csharp
  _logger.LogInformation("Dashboard loaded for project {ProjectId}", projectId);
  _logger.LogError("Failed to load data.json: {Exception}", ex.Message);
  ```
- **No Stack Traces in UI:** Return user-friendly error messages; log full exceptions server-side.

### File Access
- **data.json Location:** Store in wwwroot/ (web-accessible); ensure no secrets inside.
- **SQLite Database:** Store in App_Data/ (not web-accessible) or with OS file permissions restricting access.
- **File Validation:** Check file exists before reading; handle FileNotFoundException gracefully.

---

## Scaling Strategy

### Vertical Scaling (Single Machine, More Resources)
**Approach:** Add more RAM and CPU to the host machine.

**Scaling Path:**
1. **Current:** 1 Blazor Server instance, local SQLite, 100s concurrent users, <50 MB memory per user.
2. **Phase 2 (if needed):** Increase server RAM to 4-8 GB; handle 500+ concurrent users.
3. **Bottleneck:** Blazor Server circuit state grows linearly; beyond ~1000 concurrent users, vertical scaling hits limits.

**Mitigation:**
- Implement circuit breakers; disconnect idle users after 30 minutes.
- Cache frequently-accessed metrics in-memory; invalidate every 5 minutes.
- Paginate work items table; load only visible rows (virtual scrolling if needed).

### Horizontal Scaling (Multiple Machines)
**Note:** MVP does not require horizontal scaling. Only consider if >1000 concurrent users.

**Future Architecture (Post-MVP):**
```
Load Balancer
├─ Blazor Server Instance 1 (Port 5000)
├─ Blazor Server Instance 2 (Port 5000)
└─ Blazor Server Instance N (Port 5000)
    ↓
Shared SQL Server Database (Replaces SQLite)
```

**Requirements for Horizontal Scaling:**
- **Distributed Session State:** Use SQL Server or Redis for circuit state (not local memory).
- **Shared Database:** Migrate SQLite to SQL Server; all instances read from same DB.
- **Load Balancer:** NGINX, HAProxy, or Azure Application Gateway.
- **Sticky Sessions:** Ensure user requests route to same Blazor instance (or use distributed state).

### Database Scaling
**Current:** SQLite on local machine, up to 100K work items.

**Phase 2 (if >100K items):**
1. Migrate to SQL Server (install locally or on network).
2. Add indexes on Status, ProjectId, MilestoneId.
3. Implement query caching: cache DashboardMetrics for 5 minutes.
4. Use SQL Server pagination: `OFFSET 0 ROWS FETCH NEXT 50 ROWS ONLY`.

**Phase 3 (if >1M items):**
1. Implement data archiving: move completed milestones to archive table.
2. Use SQL Server read replicas for reporting queries.
3. Implement eventual consistency: read from replica (may lag 5 seconds behind primary).

### Frontend Scaling
**Current:** Single Dashboard.razor component, no complex nesting.

**Scaling Path:**
1. **Virtual Scrolling:** If work items table has >500 rows, implement virtual scrolling (only render visible rows).
2. **Lazy Loading:** Load milestone timeline on demand (not on initial page load).
3. **Code Splitting:** If component count grows, use Blazor lazy loading (`@attribute [RenderModeInteractiveServer(prerender: false)]`).

### Performance Optimization (Per Tier)

| Tier | Metrics | Database | Frontend | Notes |
|------|---------|----------|----------|-------|
| MVP | <10K items, 100s users | SQLite, 1 instance | Single Dashboard.razor | Sufficient for 3-month pilot. |
| Phase 2 | 10-100K items, 500s users | SQLite + caching, 1 instance | Virtual scrolling, pagination | Add metrics caching; monitor memory. |
| Phase 3 | 100K-1M items, 1000s users | SQL Server, multiple instances | Lazy loading, code splitting | Horizontal scale; distributed state. |

---

## Risks & Mitigations

### Technical Risks

#### 1. Blazor Server Memory Leaks (MEDIUM LIKELIHOOD, HIGH IMPACT)
**Risk Description:** Large component trees or excessive state mutations cause memory to grow unbounded. If 500+ users connect, memory usage compounds to gigabytes; server crashes.

**Mitigation:**
- **Avoid Nested Loops:** Don't render 10K work items in a single table; paginate to 50 items per page.
- **Disposal:** Implement `IAsyncDisposable` on services; unsubscribe from events.
  ```csharp
  public class DashboardService : IAsyncDisposable
  {
      async ValueTask IAsyncDisposable.DisposeAsync()
      {
          // Clean up resources
      }
  }
  ```
- **Profiling:** Use Visual Studio Profiler to detect memory leaks monthly.
- **Circuit Timeout:** Set Blazor circuit timeout to 30 minutes; idle users disconnect.

#### 2. JavaScript Interop Fragility for Chart.js (MEDIUM LIKELIHOOD, MEDIUM IMPACT)
**Risk Description:** JavaScript interop breaks after Blazor updates or Chart.js API changes. Timeline chart fails to render; dashboard shows blank canvas.

**Mitigation:**
- **Version Pinning:** Pin Chart.js to specific version (4.4.0); document any breaking changes.
- **Error Handling:**
  ```javascript
  window.dashboardCharts.initMilestoneChart = function(canvasId, data) {
      try {
          // Create chart
      } catch (e) {
          console.error("Chart initialization failed:", e);
          document.getElementById(canvasId).innerHTML = "<p>Chart failed to load</p>";
      }
  };
  ```
- **Testing:** After each Blazor version upgrade, manually test Chart.js rendering.
- **Isolation:** Keep Chart.js in separate MilestoneTimeline.razor component; easy to replace if needed.

#### 3. SQLite Concurrency Issues (LOW LIKELIHOOD, MEDIUM IMPACT)
**Risk Description:** If data.json is updated by external process while dashboard reads, SQLite locks occur. Queries timeout; users see blank dashboard.

**Mitigation:**
- **Write-Ahead Logging (WAL):** Enable SQLite WAL mode (default in EF Core 8+).
  ```sql
  PRAGMA journal_mode=WAL;
  ```
- **Read Timeouts:** Set connection timeout to 5 seconds (fail fast if locked).
  ```csharp
  "Data Source=dashboard.db;Connection Timeout=5;"
  ```
- **Manual Refresh:** Provide "Refresh Dashboard" button; user manually reloads if data is stale.
- **File Watcher (Post-MVP):** Implement FileSystemWatcher to detect data.json changes; auto-refresh dashboard.

#### 4. data.json Format Drift (MEDIUM LIKELIHOOD, MEDIUM IMPACT)
**Risk Description:** Stakeholder edits data.json incorrectly (missing field, wrong enum value). JSON deserializer fails; entire dashboard crashes with cryptic error.

**Mitigation:**
- **JSON Schema Validation:**
  ```csharp
  var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
  var project = JsonSerializer.Deserialize<Project>(json, options);
  if (project?.Name == null || project.Milestones == null)
      throw new InvalidOperationException("Missing required fields in data.json");
  ```
- **Friendly Error Message:** Catch `JsonException` and display:
  ```
  Error loading data.json: Invalid format. Please check:
  - Required fields: project.name, project.milestones, project.workItems
  - Status values must be: Planned, InProgress, Completed, Delayed
  ```
- **Version Control:** Keep data.json in Git with sample valid file; revert corrupted versions.
- **Schema Documentation:** Provide data.json schema and sample in README.md.

### Operational Risks

#### 5. Single Point of Failure: SQLite Database File (LOW LIKELIHOOD, HIGH IMPACT)
**Risk Description:** App_Data/dashboard.db is corrupted or accidentally deleted. Entire dashboard fails; no data recovery.

**Mitigation:**
- **Automated Backups:** Daily backup script (Windows Task Scheduler or Linux cron):
  ```powershell
  Copy-Item "C:\Projects\AgentSquad\App_Data\dashboard.db" "\\backup-server\dashboard-backup\$(Get-Date -Format 'yyyyMMdd').db"
  ```
- **Backup UI:** Add "Download Database Backup" button in admin panel (post-MVP).
- **Recovery Procedure:** If database corrupted, restore latest backup, restart app.
- **Monitoring:** Log database file size daily; alert if grows >500 MB (indicates data bloat).

#### 6. Single Point of Failure: data.json File (MEDIUM LIKELIHOOD, MEDIUM IMPACT)
**Risk Description:** data.json is deleted, corrupted, or has stale data. Stakeholder views incorrect metrics.

**Mitigation:**
- **Version Control:** Keep data.json in Git; easy to restore prior versions.
- **Read-Only Validation:** On load, validate JSON structure; reject if invalid.
- **Notification:** Display timestamp "Last updated: 2026-04-12 15:30 UTC" on dashboard; users know if data is stale.
- **Audit Log (Post-MVP):** Log every data.json update (who, when, what changed).

#### 7. Deployment Failure (LOW LIKELIHOOD, MEDIUM IMPACT)
**Risk Description:** `dotnet publish` or `dotnet run` fails; application won't start. Executives can't access dashboard during critical meeting.

**Mitigation:**
- **Pre-deployment Testing:** Publish to staging environment before production.
  ```bash
  dotnet publish -c Release -o publish-staging
  cd publish-staging && dotnet AgentSquad.Runner.dll
  ```
- **Quick Rollback:** Keep previous version (v1.0, v1.1) in Git tags; revert if needed.
- **Health Check:** Add simple health endpoint (post-MVP):
  ```csharp
  app.MapHealthChecks("/health");
  ```
- **Startup Logs:** Log all startup steps; diagnose failures quickly.

### Skill & Knowledge Risks

#### 8. Blazor Server Learning Curve (MEDIUM LIKELIHOOD, LOW IMPACT)
**Risk Description:** Team unfamiliar with Blazor Server lifecycle (OnInitializedAsync, StateHasChanged, circuits). Inefficient code; hard-to-debug rendering issues.

**Mitigation:**
- **Pair Programming:** Pairing sessions for first 2 weeks; knowledge transfer.
- **Code Review:** Review all Blazor components for common mistakes (e.g., calling StateHasChanged unnecessarily).
- **Documentation:** Create internal "Blazor Server Best Practices" wiki:
  - When to call StateHasChanged() (after non-Blazor state changes)
  - How to avoid N+1 queries in EF Core
  - JavaScript interop patterns for Chart.js
- **Training:** 2-hour Blazor Server fundamentals workshop before development.

#### 9. Entity Framework Core Query Optimization (MEDIUM LIKELIHOOD, LOW IMPACT)
**Risk Description:** Lazy-loaded navigation properties cause N+1 queries. ReportingService queries Milestones but forgets to `.Include(m => m.WorkItems)`. Each milestone query fetches work items separately; 100 milestones = 101 queries. Dashboard becomes slow.

**Mitigation:**
- **Explicit .Include():** Always use explicit eager loading:
  ```csharp
  await _context.Projects
      .Include(p => p.Milestones)
          .ThenInclude(m => m.WorkItems)
      .FirstOrDefaultAsync(p => p.Id == projectId);
  ```
- **Query Profiling:** Use EF Logging to detect slow queries:
  ```csharp
  optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
  ```
- **Performance Tests:** Before release, test queries with 10K+ items; measure query time.
- **Review Checklist:** Code review template includes "N+1 queries?" question.

### Design Risks

#### 10. Screenshot-Ready Design Doesn't Match Executive Expectations (MEDIUM LIKELIHOOD, HIGH IMPACT)
**Risk Description:** MVP dashboard looks good in development but executives find it unprofessional, cluttered, or hard to read in actual presentation. Requires significant rework before sign-off.

**Mitigation:**
- **Early Prototype:** Build Phase 1 MVP in 1 week; get stakeholder sign-off on visual design before Phase 2 engineering.
- **Screenshot Iteration:** Each week, take screenshot of current dashboard; share with stakeholders. Gather feedback early.
- **Design Template:** Reference OriginalDesignConcept.html and ReportingDashboardDesign.png throughout development.
- **Accessibility Testing:** Verify contrast ratios (WCAG AA) before Phase 1 sign-off.
  ```bash
  # Use online tool: https://webaim.org/resources/contrastchecker/
  ```
- **Presentation Simulation:** Screenshot dashboard, embed in PowerPoint, project in conference room. Get real feedback.

#### 11. Performance Degradation with Large Datasets (MEDIUM LIKELIHOOD, MEDIUM IMPACT)
**Risk Description:** Dashboard works fine with 50 milestones and 1K work items. Real project has 200 milestones and 50K items. Chart.js timeline becomes unresponsive; Blazor re-renders take >2 seconds.

**Mitigation:**
- **Load Testing:** Before Phase 2 sign-off, load 100K work items into test database. Measure performance.
- **Pagination:** Implement work items pagination (50 per page); don't render all 100K at once.
- **Aggregation:** For timeline with >50 milestones, aggregate by quarter; drill-down for details (post-MVP).
- **Caching:** Cache DashboardMetrics for 5 minutes; reduce database queries.
- **Lazy Loading:** Load milestone details on-demand, not on page load.

---

## Implementation Timeline

### Phase 1: MVP (Week 1)
**Goal:** Functional, screenshot-ready dashboard reading from data.json.

**Deliverables:**
- Blazor Server project + .sln structure
- Project, Milestone, WorkItem models
- Dashboard.razor with Bootstrap layout
- Key metrics cards (% complete, shipped, carry-over)
- Prototype Chart.js timeline (hardcoded 5 milestones)
- Work items table
- Professional CSS styling

**Exit Criteria:**
- Dashboard loads in <2 seconds
- Metrics display correctly
- Screenshot fits 1920x1080 without scrolling
- Stakeholder visual design sign-off

### Phase 2: Data Persistence (Week 2)
**Goal:** Replace hardcoded data with SQLite backend.

**Deliverables:**
- EF Core migration system
- ApplicationDbContext with Project/Milestone/WorkItem entities
- ProjectDataService (loads data.json, seeds SQLite)
- ReportingService (aggregates metrics)
- SQLite database (App_Data/dashboard.db)
- Dashboard.razor updated to query services

**Exit Criteria:**
- Data persisted to SQLite
- Metrics calculated from real data
- Dashboard loads in <2 seconds with 10K+ items
- No hardcoded sample data

### Phase 3: Polish & Optimization (Week 3)
**Goal:** Refine visuals, add filtering, optimize performance.

**Deliverables:**
- Status and milestone filtering
- Enhanced Chart.js timeline (color-coded, interactive)
- Performance optimizations (.Include(), query caching)
- Unit tests for ReportingService
- Documentation (data.json schema, architecture)
- Production-ready deployment scripts

**Exit Criteria:**
- All acceptance criteria met
- Unit test coverage >80% for business logic
- Performance benchmarks passed
- Stakeholder sign-off

---

## Conclusion

This architecture provides a **minimal-complexity, Blazor Server-based single-page dashboard** optimized for executive reporting and screenshot-ready presentation. By leveraging .NET 8's native strengths (C#, Entity Framework Core, built-in dependency injection) and avoiding unnecessary complexity (no APIs, no multi-project support, no authentication), the team can deliver an MVP in one week and iterate rapidly based on stakeholder feedback.

**Key architectural principles:**
1. **Simplicity First:** Fewer moving parts = faster development, easier debugging.
2. **Screenshot-Optimized:** Every design decision prioritizes static visual presentation.
3. **Strongly-Typed:** C# models prevent runtime errors; LINQ queries are type-safe.
4. **Performance-Aware:** Index SQLite columns, use eager loading (.Include()), cache metrics.
5. **Maintainable:** Clean separation of concerns (services, models, components); clear dependency injection.

The system is designed to scale vertically (more RAM/CPU) up to ~1000 concurrent users. If horizontal scaling is needed post-MVP, migrate SQLite to SQL Server and distribute Blazor Server instances behind a load balancer.

Success metrics: Dashboard loads <2 seconds, displays accurate metrics, and is visually professional enough to screenshot and embed directly in executive presentations without redaction.