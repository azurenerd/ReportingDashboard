# Architecture

## Overview & Goals

This document defines the complete system architecture for an **Executive Project Reporting Dashboard**, a single-page Blazor Server application that visualizes project milestones, work item status, and project health metrics from a JSON configuration file.

### Primary Goals
- Deliver a zero-authentication, screenshot-optimized executive dashboard
- Consolidate project data from a simple JSON file into a professional, real-time view
- Ensure sub-1-second page load times and clean PowerPoint-ready visuals
- Provide immediate deployment as a self-contained Windows executable
- Support 1024x768 minimum, optimized for 1920x1080 presentation resolution

### Architecture Principles
1. **Simplicity First**: Minimize dependencies; use .NET 8 built-ins exclusively
2. **Local-Only**: No cloud, no multi-user concerns, no external APIs
3. **Performance**: Read data once, cache in memory, render efficiently
4. **Presentation-Ready**: CSS optimized for print/screenshot capture
5. **Single Responsibility**: Each component handles one dashboard section

---

## System Components

### 1. **Blazor Server Host (Program.cs & App.razor)**
**Responsibility**: ASP.NET Core application entry point and root component  
**Interfaces**:
- HTTP listener on `localhost:5000` or `5001`
- Static file serving from `wwwroot/`

**Dependencies**:
- ASP.NET Core 8.0 framework
- Blazor Server middleware

**Key Behaviors**:
- Initialize DI container with services
- Register DataProvider and IDataCache services
- Load and validate data.json on startup
- Display error overlay if data.json is invalid/missing

---

### 2. **DashboardLayout.razor**
**Responsibility**: Main layout component hosting all dashboard sections  
**Interfaces**:
- Razor component accepting dashboard data model
- CSS grid layout: Header → Timeline → Metrics → Work Items

**Dependencies**:
- IDataProvider (injected)
- MilestoneTimeline.razor
- ProjectMetrics.razor
- WorkItemSummary.razor

**Key Behaviors**:
- Load project data on component initialization
- Pass data to child components via parameters
- Display loading spinner during data fetch
- Display error message if data load fails
- Render print/screenshot-optimized layout

**Data Properties**:
```csharp
public Project ProjectData { get; set; }
public bool IsLoading { get; set; }
public string ErrorMessage { get; set; }
```

---

### 3. **MilestoneTimeline.razor**
**Responsibility**: Horizontal timeline visualization of project milestones  
**Interfaces**:
- Parameter: `List<Milestone> Milestones`
- Renders SVG or HTML canvas for timeline

**Dependencies**:
- Chart.js library (via CDN in wwwroot/index.html)
- Milestone model

**Key Behaviors**:
- Render milestones horizontally with status indicators (completed, in-progress, at-risk, future)
- Color-code by status: green (completed), blue (in-progress), red (at-risk), gray (future)
- Display milestone name and target date
- Responsive sizing: scales 1024-1920px width

**Data Shape**:
```csharp
public class Milestone
{
    public string Name { get; set; }
    public DateTime TargetDate { get; set; }
    public MilestoneStatus Status { get; set; } // Completed, InProgress, AtRisk, Future
    public string Description { get; set; }
}
```

---

### 4. **ProjectMetrics.razor**
**Responsibility**: Display KPIs and project health indicators  
**Interfaces**:
- Parameter: `ProjectMetrics Metrics`
- CSS Grid layout for 2-4 metric cards

**Dependencies**:
- ProjectMetrics model

**Key Behaviors**:
- Display overall completion percentage (circular progress or progress bar)
- Show on-time vs. at-risk status (color-coded badge)
- Display velocity indicator (items completed this month)
- Display milestone count and target count
- High-contrast, executive-friendly styling

**Data Shape**:
```csharp
public class ProjectMetrics
{
    public int CompletionPercentage { get; set; }
    public HealthStatus HealthStatus { get; set; } // OnTrack, AtRisk, Blocked
    public int VelocityThisMonth { get; set; }
    public int TotalMilestones { get; set; }
    public int CompletedMilestones { get; set; }
}
```

---

### 5. **WorkItemSummary.razor**
**Responsibility**: Display work items grouped by status  
**Interfaces**:
- Parameter: `List<WorkItem> WorkItems`
- Three-column layout: "Shipped This Month" | "In Progress" | "Carried Over"

**Dependencies**:
- WorkItem model

**Key Behaviors**:
- Group work items by status property
- Display item count per column
- List item titles and brief descriptions
- Truncate long descriptions with ellipsis
- Responsive column widths

**Data Shape**:
```csharp
public class WorkItem
{
    public string Title { get; set; }
    public string Description { get; set; }
    public WorkItemStatus Status { get; set; } // Shipped, InProgress, CarriedOver
    public string AssignedTo { get; set; }
}
```

---

### 6. **DataProvider.cs (Service Layer)**
**Responsibility**: Read, parse, validate, and cache JSON configuration file  
**Interfaces**:
```csharp
public interface IDataProvider
{
    Task<Project> LoadProjectDataAsync();
    void InvalidateCache();
}

public class DataProvider : IDataProvider
{
    private readonly IDataCache _cache;
    private readonly ILogger<DataProvider> _logger;
    private const string DATA_FILE_PATH = "wwwroot/data.json";

    public async Task<Project> LoadProjectDataAsync()
    {
        // Check cache first
        var cached = await _cache.GetAsync<Project>("project_data");
        if (cached != null) return cached;

        // Read and parse JSON
        var json = await File.ReadAllTextAsync(DATA_FILE_PATH);
        var project = JsonSerializer.Deserialize<Project>(json);

        // Validate structure
        ValidateProjectData(project);

        // Cache result
        await _cache.SetAsync("project_data", project, TimeSpan.FromHours(1));

        return project;
    }

    public void InvalidateCache() => _cache.Remove("project_data");

    private void ValidateProjectData(Project project)
    {
        if (project == null) throw new InvalidOperationException("Project data is null");
        if (string.IsNullOrEmpty(project.Name)) throw new InvalidOperationException("Project name is required");
        if (project.Milestones == null || project.Milestones.Count == 0)
            throw new InvalidOperationException("At least one milestone is required");
    }
}
```

**Dependencies**:
- System.Text.Json (built-in)
- IDataCache (in-memory cache service)
- ILogger (ASP.NET Core logging)

**Key Behaviors**:
- Read UTF-8 encoded JSON file from wwwroot/data.json
- Parse into strongly-typed Project object
- Validate required fields and structure
- Cache in-memory for 1 hour (or configurable TTL)
- Log errors for debugging
- Throw descriptive exceptions on parse failures

---

### 7. **IDataCache Service**
**Responsibility**: In-memory caching layer  
**Interfaces**:
```csharp
public interface IDataCache
{
    Task<T> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
}
```

**Implementation**: Wrap `IMemoryCache` from `Microsoft.Extensions.Caching.Memory`

**Key Behaviors**:
- Store parsed Project object with optional TTL
- Return null if key not found or expired
- Thread-safe operations
- Optional SQLite backing for persistence (Phase 2)

---

### 8. **Models (Project.cs, Milestone.cs, WorkItem.cs)**
**Responsibility**: Data contracts for JSON deserialization  
**Enums**:
```csharp
public enum MilestoneStatus { Completed, InProgress, AtRisk, Future }
public enum WorkItemStatus { Shipped, InProgress, CarriedOver }
public enum HealthStatus { OnTrack, AtRisk, Blocked }
```

**Key Classes**:
```csharp
public class Project
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime TargetEndDate { get; set; }
    public int CompletionPercentage { get; set; }
    public HealthStatus HealthStatus { get; set; }
    public int VelocityThisMonth { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<WorkItem> WorkItems { get; set; }
}
```

---

### 9. **CSS Framework & Styling (dashboard.css)**
**Responsibility**: Print/screenshot-optimized layout and visual design  
**Stack**:
- Pico CSS 2.x for minimalist, clean base styles
- Custom CSS Grid layout for dashboard sections
- CSS media queries for print optimization
- Color palette: Professional executive theme (navy, white, accent colors)

**Key Features**:
- Responsive grid: 1-column mobile → 4-column desktop
- High-contrast text for readability
- Print media query hides navigation, optimizes spacing
- Screenshot-friendly margins and padding
- No external fonts (system fonts only)
- Optimized for 1280x720 and 1920x1080

---

### 10. **Static Assets (wwwroot/)**
**Responsibility**: Client-side files served as static content  
**Directory Structure**:
```
wwwroot/
├── data.json          (Project configuration file)
├── css/
│   ├── dashboard.css  (Custom styles)
│   └── pico.css       (Pico CSS framework CDN link)
├── js/
│   └── chart.js       (Timeline visualization library CDN link)
└── index.html         (Blazor bootstrapper)
```

**data.json Schema**:
```json
{
  "name": "Project Codename",
  "description": "Brief project description",
  "startDate": "2024-01-01",
  "targetEndDate": "2024-12-31",
  "completionPercentage": 45,
  "healthStatus": "OnTrack",
  "velocityThisMonth": 12,
  "milestones": [
    {
      "name": "Phase 1 Launch",
      "targetDate": "2024-03-31",
      "status": "Completed",
      "description": "Core feature rollout"
    }
  ],
  "workItems": [
    {
      "title": "API Integration",
      "description": "Connect to external data source",
      "status": "InProgress",
      "assignedTo": "Team A"
    }
  ]
}
```

---

## Component Interactions

### Data Flow
```
User opens browser → localhost:5000
↓
App.razor initializes → DashboardLayout.razor
↓
DashboardLayout calls IDataProvider.LoadProjectDataAsync()
↓
DataProvider.cs reads wwwroot/data.json
↓
DataProvider validates JSON structure
↓
DataProvider caches result in IMemoryCache
↓
DashboardLayout receives Project model
↓
DashboardLayout renders child components:
  ├─ MilestoneTimeline.razor (Milestone[])
  ├─ ProjectMetrics.razor (ProjectMetrics)
  └─ WorkItemSummary.razor (WorkItem[])
↓
Browser renders HTML + CSS
↓
User sees dashboard (< 1 second)
```

### Component Communication Patterns
1. **Parent → Child**: Data passed via `[Parameter]` properties
2. **Service Injection**: Components inject `IDataProvider` and `IDataCache`
3. **Event Callbacks**: Error handling via `EventCallback` from DashboardLayout to App.razor
4. **Lifecycle**: `OnInitializedAsync()` in DashboardLayout triggers data load

### Error Handling Flow
```
DashboardLayout.OnInitializedAsync()
↓
TRY: IDataProvider.LoadProjectDataAsync()
├─ File not found → Catch → Set ErrorMessage
├─ JSON parse error → Catch → Set ErrorMessage
├─ Validation failure → Catch → Set ErrorMessage
└─ Success → Set ProjectData, IsLoading = false
↓
Render error overlay or dashboard based on ErrorMessage
```

---

## Data Model

### Entity Relationships
```
Project (root)
├── Milestones[] (ordered by TargetDate)
│   └── Status (enum: Completed, InProgress, AtRisk, Future)
└── WorkItems[] (grouped by Status)
    └── Status (enum: Shipped, InProgress, CarriedOver)
```

### Storage Format
- **Primary Storage**: `wwwroot/data.json` (UTF-8 text file)
- **Runtime Storage**: IMemoryCache (in-process)
- **Optional Secondary**: SQLite database for optional persistence (not MVP)

### JSON Structure Validation
```csharp
// Required fields (non-null)
- Project.Name (string)
- Project.Milestones (array, >= 1 item)
- Project.WorkItems (array, >= 0 items)
- Milestone.Name (string)
- Milestone.TargetDate (ISO 8601 string)
- WorkItem.Title (string)
- WorkItem.Status (enum: Shipped|InProgress|CarriedOver)

// Optional fields (nullable)
- Project.Description (string)
- WorkItem.Description (string)
- Milestone.Description (string)
```

### Data Refresh Strategy
- **Default**: Load on application startup, cache for 1 hour
- **Manual Invalidation**: Button in debug/admin section to reload data.json
- **No Real-Time Sync**: Changes to data.json require manual cache invalidation + page refresh

---

## API Contracts

### HTTP Endpoints (Blazor Server)
```
GET /                       → Render Blazor root component (App.razor)
GET /css/dashboard.css      → Serve custom styles
GET /data.json              → Serve static JSON file (optional explicit endpoint)
GET /_framework/*           → Blazor .NET runtime files
```

### Service Interfaces

#### IDataProvider
```csharp
Task<Project> LoadProjectDataAsync();
// Throws: FileNotFoundException, JsonException, InvalidOperationException
```

#### IDataCache
```csharp
Task<T> GetAsync<T>(string key) where T : class;
Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
void Remove(string key);
```

### Response Models
```csharp
// Success Response
{
  "name": "Project X",
  "completionPercentage": 45,
  "healthStatus": "OnTrack",
  "milestones": [...],
  "workItems": [...]
}

// Error Response (displayed in ErrorMessage component)
{
  "error": "data.json not found",
  "details": "File 'wwwroot/data.json' does not exist. Please create it with valid project data."
}
```

### Error Handling
| Error | HTTP Code | Response |
|-------|-----------|----------|
| data.json not found | 500 | Error overlay: "Configuration file missing" |
| Invalid JSON syntax | 500 | Error overlay: "Invalid JSON format" |
| Missing required field | 500 | Error overlay: "Invalid project structure" |
| Successful load | 200 | Render dashboard |

---

## Infrastructure Requirements

### Hardware
- **Development**: Windows 10/11 with .NET 8 SDK (8GB RAM, 2GB disk)
- **Runtime**: Windows 7+, macOS 10.15+, or Linux (x64) with .NET 8 runtime or self-contained executable
- **Display**: Minimum 1024x768 resolution, target 1920x1080

### Networking
- **Localhost Only**: Application binds to `127.0.0.1:5000` or `5001`
- **No External Calls**: No outbound HTTP/HTTPS (except CDN for Chart.js/Feather Icons)
- **No Firewall Rules**: Operates entirely within user's local network stack

### Storage
- **Application Files**: ~50MB (Blazor Server runtime) + ~100KB (static assets)
- **Configuration**: data.json (~10-100KB typical)
- **Runtime Cache**: In-memory only (~1-10MB for typical project data)
- **No Database**: SQLite optional for Phase 2 persistence

### Deployment
- **Development**: `dotnet run` from command line
- **Production**: Self-contained .exe executable (~100-150MB) or framework-dependent deployment
- **Distribution**: Single .exe file or .zip with executable + wwwroot/data.json

### CI/CD (Optional)
- **Build**: `dotnet build` → `dotnet publish -c Release`
- **Test**: `dotnet test` (xUnit, Bunit)
- **Artifact**: Self-contained executable for Windows

### Ports
- **Primary**: `localhost:5000` (HTTP)
- **Fallback**: `localhost:5001` (HTTPS, self-signed cert)
- **Configuration**: `launchSettings.json` specifies port

---

## Technology Stack Decisions

### Blazor Server (vs. Blazor WebAssembly)
**Decision**: Blazor Server  
**Justification**:
- No build complexity (C# compiles server-side, sends HTML to browser)
- Real-time updates via SignalR if needed in future phases
- Simpler debugging and C# interoperability
- Smaller client-side payload
- Local deployment doesn't require static hosting

### JSON File (vs. Database)
**Decision**: JSON file (wwwroot/data.json)  
**Justification**:
- Zero dependencies (no database setup)
- Simple manual editing by project managers
- Easy version control and change tracking
- Sufficient for single-user, read-only dashboard
- SQLite available if caching/querying complexity increases

### Pico CSS (vs. Bootstrap)
**Decision**: Pico CSS 2.x  
**Justification**:
- Minimal, elegant design → clean screenshots
- ~10KB minified (vs. 150KB for Bootstrap)
- Print-optimized out of box
- No JavaScript required
- Professional, executive appearance

### Chart.js (vs. SVG/Canvas)
**Decision**: Chart.js 4.4 via CDN  
**Justification**:
- Handles responsive milestone timeline rendering
- Professional visualization with minimal code
- Lightweight library (~50KB)
- Works seamlessly with Blazor Server
- SVG fallback for simple components

### System.Text.Json (vs. Newtonsoft.Json)
**Decision**: System.Text.Json (built-in)  
**Justification**:
- Built into .NET 8 runtime
- Zero external dependencies
- Sufficient for schema validation
- High performance for small-to-medium JSON files
- No NuGet package required

### IMemoryCache (vs. Redis)
**Decision**: IMemoryCache (built-in)  
**Justification**:
- Single-user local application
- No distributed caching required
- Built into ASP.NET Core DI
- Sub-millisecond cache hits
- No external service dependencies

### Logging & Diagnostics
**Decision**: Built-in ILogger  
**Justification**:
- No external logging framework needed
- Integrated with ASP.NET Core configuration
- Sufficient for local debugging
- Can be enhanced in Phase 2 if required

---

## Security Considerations

### Authentication
**Requirement**: None (per PM specification)  
**Rationale**: Local, single-user dashboard; no multi-user access control needed

### Authorization
**Requirement**: None (per PM specification)  
**Rationale**: All data visible to single user; no role-based access

### Data Protection
- **Data at Rest**: data.json stored in plaintext on local filesystem
- **Data in Transit**: No external API calls (CDN assets only)
- **Sensitive Data**: No passwords, tokens, or PII in scope

### Input Validation
```csharp
// Required in DataProvider.ValidateProjectData()
1. Project object is not null
2. Project.Name is non-empty string
3. Milestones array contains >= 1 item
4. All enum values (Status, HealthStatus) are valid
5. DateTime fields are valid ISO 8601 strings
6. CompletionPercentage is 0-100 range
```

### Error Handling
- Catch JSON parse exceptions → Display user-friendly error
- Catch file not found → Display "Configuration file missing" message
- Log all errors to console/debug output
- Never expose stack traces to UI

### HTTPS/TLS
- **Development**: HTTP only (localhost:5000)
- **Production Option**: HTTPS on localhost:5001 with self-signed certificate (generated by Blazor)
- **Requirement**: No HTTPS needed per scope (local-only)

### CORS
**Requirement**: None (no cross-origin requests)  
**Rationale**: All resources served from same origin

---

## Scaling Strategy

### Single-User Baseline
- Current architecture assumes single local user
- No multi-user concurrency or session management required

### Potential Phase 2 Scaling
1. **Network Sharing** (1-10 users on same network):
   - Bind to `0.0.0.0:5000` instead of localhost
   - Add authentication (simple username/password)
   - Implement session-based caching per user

2. **Large JSON Files** (1000+ milestones/items):
   - Implement lazy loading in components
   - Paginate work items list (20 items per page)
   - Add database layer (SQLite or SQL Server) with Entity Framework

3. **Real-Time Updates**:
   - Use SignalR for live dashboard refresh
   - File watcher on data.json for change detection
   - Broadcast updates to connected clients

4. **Performance Optimization**:
   - Implement virtual scrolling for large work item lists
   - Add CDN caching for static assets
   - Compress Blazor .NET runtime files (gzip/brotli)

### Current Limits
- **JSON File Size**: < 5MB (in-memory parse)
- **Milestone Count**: < 100 (timeline rendering stays responsive)
- **Work Item Count**: < 500 (scrollable list)
- **Concurrent Users**: 1 (local application)

---

## Risks & Mitigations

### Risk 1: JSON File Corruption
**Impact**: Dashboard fails to load; users unable to access project data  
**Probability**: Low (manual editing)  
**Mitigation**:
- Implement file validation in DataProvider
- Display error message with remediation steps
- Provide JSON schema documentation
- Include example data.json with deployment

### Risk 2: Chart.js CDN Unavailability
**Impact**: Timeline visualization fails to render  
**Probability**: Very Low (major CDN)  
**Mitigation**:
- Include Chart.js library locally as fallback
- Implement SVG-based timeline fallback
- Graceful degradation: display table if Chart.js unavailable

### Risk 3: Large JSON Parse Performance
**Impact**: Dashboard load time > 1 second  
**Probability**: Low (typical project data < 100KB)  
**Mitigation**:
- Implement streaming JSON parser for large files
- Add loading indicator during parse
- Cache parsed data in-memory (already planned)
- Lazy-load work items if count > 100

### Risk 4: Screenshot Rendering Inconsistency
**Impact**: Screenshot looks different in Chrome vs. Edge  
**Probability**: Medium (browser rendering differences)  
**Mitigation**:
- Test and validate in both Chrome and Edge
- Use CSS Normalize or Reset framework
- Avoid browser-specific CSS features
- Document screenshot best practices

### Risk 5: .NET 8 Runtime Not Installed
**Impact**: Self-contained .exe fails to start  
**Probability**: Low (self-contained mitigates)  
**Mitigation**:
- Deploy as self-contained executable (not framework-dependent)
- Include .NET 8 runtime in .exe package (~150MB)
- Provide Windows installer (.msi) option
- Document system requirements in README

### Risk 6: Data Loss (data.json overwritten)
**Impact**: Historical project data lost  
**Probability**: Medium (manual editing)  
**Mitigation**:
- Implement automatic daily backups
- Version control data.json in Git
- Provide restore mechanism from backup
- Document manual backup procedure

### Risk 7: Milestone Timeline Rendering at Edge Resolutions
**Impact**: Timeline cut off or misaligned at 1024x768  
**Probability**: Low (responsive CSS planned)  
**Mitigation**:
- Test at minimum 1024x768 resolution
- Implement horizontal scrolling fallback
- Use CSS media queries for responsive breakpoints
- Document minimum supported resolution

### Risk 8: Memory Leak in Long-Running Session
**Impact**: Application gradually consumes more memory; eventual crash  
**Probability**: Low (stateless components, built-in GC)  
**Mitigation**:
- Implement IAsyncDisposable in components
- Monitor memory usage during development
- Add cache expiration (1-hour TTL)
- Recommend periodic browser refresh for long sessions

---

## Deployment & Operations

### Development Environment Setup
```bash
# Prerequisites
1. Install .NET 8 SDK
2. Clone repository
3. Create wwwroot/data.json (copy from example)
4. dotnet run
5. Open browser → https://localhost:5001
```

### Production Deployment
```bash
# Build self-contained executable
dotnet publish -c Release -r win-x64 --self-contained

# Output location
bin\Release\net8.0\win-x64\publish\AgentSquad.Runner.exe

# Distribute
1. Place .exe and wwwroot/data.json in target folder
2. User double-clicks .exe
3. Browser opens to localhost:5000
```

### Configuration Management
- **Development**: launchSettings.json specifies localhost:5000
- **Production**: appsettings.json specifies port 5000
- **Data Source**: wwwroot/data.json (packaged with executable)
- **Logging**: Console output or file logging (optional)

### Monitoring & Troubleshooting
- **Application Logs**: Console output during development
- **JSON Validation**: Diagnostic message if data.json invalid
- **Browser Dev Tools**: F12 to debug Blazor components
- **Port Conflicts**: If 5000 taken, automatically fallback to 5001

---

## Summary

This architecture provides a **lightweight, dependency-free executive dashboard** built on C# .NET 8 and Blazor Server. The design prioritizes:

1. **Simplicity**: JSON file + in-memory caching, zero external services
2. **Performance**: Sub-1-second load times via efficient component rendering
3. **Presentation Quality**: Print/screenshot-optimized CSS for PowerPoint integration
4. **Local Deployment**: Self-contained executable, no cloud or database required
5. **Maintainability**: Strongly-typed models, clean service layer, responsive component hierarchy

**MVP Delivery Estimate**: 1-2 weeks  
**Technology Stack**: C# .NET 8, Blazor Server, Pico CSS, Chart.js, System.Text.Json  
**Deployment**: Self-contained Windows .exe + data.json configuration file