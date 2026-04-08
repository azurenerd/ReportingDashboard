# Architecture

## Overview & Goals

**System Purpose:** Executive reporting dashboard providing real-time visibility into project milestones, task status, and progress metrics via a single-page Blazor Server application that reads from JSON configuration and outputs screenshot-ready visualizations for PowerPoint integration.

**Primary Goals:**
- Display project health metrics and milestone timeline on a clean, responsive dashboard
- Enable screenshot capture without loss of fidelity for executive briefings
- Operate entirely on local infrastructure with zero external cloud dependencies
- Minimize deployment complexity through file-based data storage (data.json)
- Support manual data updates via JSON editing without code redeployment

**Design Principles:**
- Single-page, uncluttered layout optimized for visual consumption
- Server-side rendering for consistent rendering across refresh cycles
- No client-side complexity; all business logic in C# backend
- Responsive Bootstrap grid supporting desktop screens 1024px+
- Deterministic rendering for screenshot consistency

---

## System Components

### 1. **Dashboard.razor (Main Page Container)**
**Responsibility:** Orchestrate page layout, manage overall state, coordinate child component rendering.

**Interfaces:**
- Input: None (root page)
- Output: HTML rendered dashboard with three sections (timeline, status cards, metrics)

**Dependencies:** 
- ProjectDataService (for data retrieval)
- MilestoneTimeline.razor
- StatusCard.razor
- ProgressMetrics.razor
- ErrorBoundary.razor (error handling)

**Data Flow:**
```csharp
- OnInitializedAsync(): Load ProjectData via ProjectDataService
- StateHasChanged(): Trigger re-render on data updates
- Pass ProjectData and Milestones to child components via parameters
```

**Key Logic:**
- Deserialize JSON on component initialization
- Handle JSON parsing errors gracefully with user-friendly messages
- Manage component visibility/state across page sections

---

### 2. **MilestoneTimeline.razor (Milestone Visualization)**
**Responsibility:** Render visual timeline of project milestones with completion status indicators.

**Interfaces:**
- Input: `List<Milestone> Milestones`, `DateTime ProjectStartDate`, `DateTime ProjectEndDate`
- Output: Full-width timeline section with visual status indicators

**Parameters:**
```csharp
[Parameter] public List<Milestone> Milestones { get; set; }
[Parameter] public int ProjectDurationDays { get; set; }
```

**Dependencies:**
- Chart.js via JavaScript Interop (optional; can use custom SVG)
- Bootstrap grid utilities

**Rendering Logic:**
- Map milestone dates to horizontal timeline axis
- Use color coding: Green (completed), Blue (in-progress), Gray (pending)
- Display milestone name, target date, completion status
- Responsive: stack vertically on screens <1024px (via Bootstrap d-md-block)

**Data Requirements:**
```csharp
class Milestone {
    string Name { get; set; }
    DateTime TargetDate { get; set; }
    MilestoneStatus Status { get; set; } // Completed, InProgress, Pending
    int CompletionPercentage { get; set; }
}
```

---

### 3. **StatusCard.razor (Task Status Breakdown)**
**Responsibility:** Display count of tasks in each status category with expandable task lists.

**Interfaces:**
- Input: `TaskStatusSummary StatusSummary`, `List<Task> Tasks`
- Output: Three cards (Shipped, In-Progress, Carried-Over) with counts and task lists

**Parameters:**
```csharp
[Parameter] public string StatusCategory { get; set; } // "Shipped", "InProgress", "CarriedOver"
[Parameter] public int TaskCount { get; set; }
[Parameter] public List<Task> Tasks { get; set; }
[Parameter] public string CardColor { get; set; } // CSS class
```

**Card Layout:**
- Header: Status category name + task count (large, bold)
- Color stripe: Green (shipped), Blue (in-progress), Orange (carried-over)
- Expandable task list: Bootstrap collapse component
- Task item: Checkbox icon + task name + assigned owner

**Data Requirements:**
```csharp
class Task {
    string Name { get; set; }
    TaskStatus Status { get; set; } // Shipped, InProgress, CarriedOver
    string AssignedTo { get; set; }
    DateTime DueDate { get; set; }
}

class TaskStatusSummary {
    int ShippedCount { get; set; }
    int InProgressCount { get; set; }
    int CarriedOverCount { get; set; }
}
```

---

### 4. **ProgressMetrics.razor (Burn-down & Completion Charts)**
**Responsibility:** Visualize project completion percentage and burn-down trajectory.

**Interfaces:**
- Input: `ProjectMetrics Metrics`, `List<DateTime> DatesVector`, `List<int> BurndownVector`
- Output: Chart section with completion % and optional burn-down chart

**Parameters:**
```csharp
[Parameter] public int CompletionPercentage { get; set; }
[Parameter] public int TotalTasks { get; set; }
[Parameter] public int CompletedTasks { get; set; }
[Parameter] public double BurndownRate { get; set; } // tasks/day
```

**Rendering:**
- Progress bar: Bootstrap progress component (large, 12pt text)
- Completion %: Large numeric display (24pt font, centered)
- Optional Chart.js burn-down line chart below progress bar
- No animations (static for screenshot consistency)

**Data Requirements:**
```csharp
class ProjectMetrics {
    int CompletionPercentage { get; set; }
    int TotalTasks { get; set; }
    int CompletedTasks { get; set; }
    DateTime ProjectStartDate { get; set; }
    DateTime ProjectEndDate { get; set; }
    double EstimatedBurndownRate { get; set; }
}
```

---

### 5. **ProjectDataService.cs (Data Access Layer)**
**Responsibility:** Load, parse, validate JSON data; expose in-memory cache; handle data refresh.

**Public Methods:**
```csharp
public async Task<ProjectData> LoadProjectDataAsync(string jsonFilePath)
public void RefreshData()
public ProjectData GetCachedData()
public bool ValidateJsonSchema(string json)
```

**Implementation:**
- Read data.json from wwwroot directory via System.IO
- Deserialize using System.Text.Json
- Cache in-memory as `ProjectData` object
- Return user-friendly error message on JSON parse failure
- Support manual refresh (called by Dashboard.razor on user action)

**Error Handling:**
```csharp
try {
    var json = await File.ReadAllTextAsync(jsonPath);
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var data = JsonSerializer.Deserialize<ProjectData>(json, options);
    if (data == null) throw new InvalidOperationException("JSON deserialization resulted in null");
    _cachedData = data;
    return data;
} catch (JsonException ex) {
    throw new DataLoadException($"Invalid JSON format: {ex.Message}");
} catch (FileNotFoundException) {
    throw new DataLoadException("data.json not found in wwwroot directory");
}
```

**In-Memory Cache:**
```csharp
private ProjectData _cachedData;
private DateTime _lastLoadTime;
```

---

### 6. **ErrorBoundary.razor (Error Handling)**
**Responsibility:** Catch and display component rendering errors gracefully.

**Behavior:**
- Wrap Dashboard.razor in Blazor ErrorBoundary
- Display user-friendly error messages (no stack traces)
- Show "Refresh" button to retry data load
- Log errors to browser console (dev mode only)

---

## Component Interactions

### Data Flow (On Page Load)
```
Browser Request → Program.cs routing → Index.razor → Dashboard.razor
                                                          ↓
                                        OnInitializedAsync() calls
                                        ProjectDataService.LoadProjectDataAsync()
                                                          ↓
                                        Reads data.json → JSON deserialization
                                                          ↓
                                        Returns ProjectData object
                                                          ↓
                                        Dashboard passes to child components:
                                          - MilestoneTimeline ← Milestones[]
                                          - StatusCard ← TaskStatusSummary, Tasks[]
                                          - ProgressMetrics ← ProjectMetrics
                                                          ↓
                                        Each component renders HTML
                                                          ↓
                                        Browser displays dashboard
```

### Data Refresh Flow (User Manually Refreshes Browser)
```
User edits data.json → Browser F5/Ctrl+R → Page reload
                                              ↓
                                      Dashboard.OnInitializedAsync() re-runs
                                              ↓
                                      ProjectDataService loads updated JSON
                                              ↓
                                      All child components re-render with new data
```

### Error Flow (Malformed JSON)
```
ProjectDataService.LoadProjectDataAsync() catches JsonException
                                              ↓
                                      Throws DataLoadException
                                              ↓
                                      ErrorBoundary catches exception
                                              ↓
                                      Displays error message to user:
                                      "Error loading project data: [reason]"
                                              ↓
                                      User sees "Refresh" button
```

### Chart Rendering (Chart.js Interop)
```
ProgressMetrics.razor renders progress bar (HTML)
                            ↓
                    Chart.js data passed via @ref to JavaScript
                            ↓
                    JavaScript InteropService creates Chart instance
                            ↓
                    Chart renders in <canvas> element
                            ↓
                    No animations (rendering: { preserveAspectRatio: 'xMidYMid' })
```

---

## Data Model

### **ProjectData (Root Entity)**
```csharp
public class ProjectData {
    public ProjectInfo Project { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<Task> Tasks { get; set; }
    public ProjectMetrics Metrics { get; set; }
}
```

### **ProjectInfo**
```csharp
public class ProjectInfo {
    public string Name { get; set; }              // e.g., "Q2 Mobile App Release"
    public string Description { get; set; }       // e.g., "iOS & Android app v2.0"
    public DateTime StartDate { get; set; }       // Project kickoff
    public DateTime EndDate { get; set; }         // Target completion
    public string Status { get; set; }            // "OnTrack", "AtRisk", "Delayed"
    public string Sponsor { get; set; }           // Executive sponsor name
    public string ProjectManager { get; set; }    // PM name
}
```

### **Milestone**
```csharp
public class Milestone {
    public string Id { get; set; }                // Unique ID, e.g., "m1"
    public string Name { get; set; }              // e.g., "Design Review Complete"
    public DateTime TargetDate { get; set; }
    public DateTime? ActualDate { get; set; }     // null if not completed
    public MilestoneStatus Status { get; set; }  // Completed, InProgress, Pending
    public int CompletionPercentage { get; set; } // 0-100
}

public enum MilestoneStatus {
    Completed = 0,
    InProgress = 1,
    Pending = 2
}
```

### **Task**
```csharp
public class Task {
    public string Id { get; set; }                // Unique ID, e.g., "t1"
    public string Name { get; set; }              // e.g., "API authentication"
    public TaskStatus Status { get; set; }       // Shipped, InProgress, CarriedOver
    public string AssignedTo { get; set; }       // Owner name
    public DateTime DueDate { get; set; }
    public int EstimatedDays { get; set; }       // For burn-down calc
    public string RelatedMilestone { get; set; } // Milestone.Id reference
}

public enum TaskStatus {
    Shipped = 0,
    InProgress = 1,
    CarriedOver = 2
}
```

### **ProjectMetrics**
```csharp
public class ProjectMetrics {
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CarriedOverTasks { get; set; }
    public int CompletionPercentage { get; set; } // (CompletedTasks / TotalTasks) * 100
    public double EstimatedBurndownRate { get; set; } // tasks/day
    public DateTime ProjectStartDate { get; set; }
    public DateTime ProjectEndDate { get; set; }
    public int DaysRemaining { get; set; }        // (ProjectEndDate - Now).Days
}
```

### **Storage Format: data.json**
```json
{
  "project": {
    "name": "Q2 Mobile App Release",
    "description": "iOS and Android mobile app version 2.0 with new payment integration",
    "startDate": "2026-04-01",
    "endDate": "2026-06-30",
    "status": "OnTrack",
    "sponsor": "VP of Product",
    "projectManager": "Jane Smith"
  },
  "milestones": [
    {
      "id": "m1",
      "name": "Design Review Complete",
      "targetDate": "2026-04-15",
      "actualDate": "2026-04-12",
      "status": "Completed",
      "completionPercentage": 100
    },
    {
      "id": "m2",
      "name": "Development Sprint 1 Done",
      "targetDate": "2026-05-01",
      "actualDate": null,
      "status": "InProgress",
      "completionPercentage": 65
    },
    {
      "id": "m3",
      "name": "QA Testing Complete",
      "targetDate": "2026-06-01",
      "actualDate": null,
      "status": "Pending",
      "completionPercentage": 0
    }
  ],
  "tasks": [
    {
      "id": "t1",
      "name": "API Authentication Module",
      "status": "Shipped",
      "assignedTo": "John Doe",
      "dueDate": "2026-04-20",
      "estimatedDays": 5,
      "relatedMilestone": "m1"
    },
    {
      "id": "t2",
      "name": "Payment Integration",
      "status": "InProgress",
      "assignedTo": "Alice Brown",
      "dueDate": "2026-05-10",
      "estimatedDays": 8,
      "relatedMilestone": "m2"
    },
    {
      "id": "t3",
      "name": "iOS Push Notifications",
      "status": "CarriedOver",
      "assignedTo": "Bob Wilson",
      "dueDate": "2026-05-15",
      "estimatedDays": 6,
      "relatedMilestone": "m2"
    }
  ],
  "metrics": {
    "totalTasks": 10,
    "completedTasks": 3,
    "inProgressTasks": 5,
    "carriedOverTasks": 2,
    "estimatedBurndownRate": 1.2
  }
}
```

### **Stored Locations:**
- **data.json:** `/wwwroot/data/data.json` (accessible to ProjectDataService via File.ReadAllTextAsync)
- **No database:** File-based storage only, no SQLite or external DB

---

## API Contracts

### **Endpoint 1: GET /api/project-data (Optional - for async refresh)**
**Purpose:** Fetch current project data without page reload (supports manual refresh button).

**Request:**
```
GET /api/project-data HTTP/1.1
Host: localhost:5000
```

**Response (200 OK):**
```json
{
  "project": { ... },
  "milestones": [ ... ],
  "tasks": [ ... ],
  "metrics": { ... }
}
```

**Response (400 Bad Request - Malformed JSON):**
```json
{
  "error": "Invalid JSON in data.json",
  "message": "Unexpected token '}' at line 5, column 10"
}
```

**Response (404 Not Found):**
```json
{
  "error": "data.json not found",
  "message": "File /wwwroot/data/data.json does not exist"
}
```

**Implementation:**
```csharp
[ApiController]
[Route("api")]
public class ProjectDataController : ControllerBase {
    private readonly ProjectDataService _service;
    
    [HttpGet("project-data")]
    public async Task<IActionResult> GetProjectData() {
        try {
            var data = await _service.LoadProjectDataAsync("wwwroot/data/data.json");
            return Ok(data);
        } catch (DataLoadException ex) {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

**Note:** This endpoint is optional. Dashboard can also trigger data refresh via simple browser F5.

### **No Other Endpoints**
- No authentication endpoints (no login/logout)
- No data mutation endpoints (POST/PUT/DELETE)
- No multi-project endpoints (single project only)
- Users edit data.json manually via text editor

---

## Infrastructure Requirements

### **Runtime Environment**
- **OS:** Windows 10/11 or Linux (Ubuntu 20.04+)
- **.NET SDK:** .NET 8 LTS
- **Browser:** Chrome, Edge, Safari (Desktop, 1024px+ minimum)
- **Network:** Localhost only, no internet required

### **Disk Storage**
- **Application size:** ~50-100 MB (Blazor Server runtime)
- **data.json:** <1 MB (typical for 3-4 milestones, 8-10 tasks)
- **No external database required**

### **Memory Requirements**
- **RAM:** 512 MB minimum (2 GB recommended)
- **Blazor Server session state:** ~10-20 MB per active browser session
- **No multi-user caching needed (single project, single-user tool)**

### **Deployment**

**Development:**
```bash
dotnet run --urls "https://localhost:5001;http://localhost:5000"
```

**Production (Standalone Exe):**
```bash
dotnet publish -c Release -r win-x64 --self-contained
# Output: bin/Release/net8.0/win-x64/publish/AgentSquad.Runner.exe
```

**Linux Deployment:**
```bash
dotnet publish -c Release -r linux-x64 --self-contained
# Run: ./AgentSquad.Runner
```

### **File Structure**
```
AgentSquad.Runner/
├── Pages/
│   └── Index.razor (Dashboard.razor embedded)
├── Components/
│   ├── MilestoneTimeline.razor
│   ├── StatusCard.razor
│   ├── ProgressMetrics.razor
│   └── ErrorBoundary.razor
├── Services/
│   └── ProjectDataService.cs
├── Data/
│   └── Models.cs (ProjectData, Milestone, Task, etc.)
├── wwwroot/
│   ├── data/
│   │   └── data.json
│   ├── css/
│   │   ├── bootstrap.min.css (or CDN)
│   │   └── app.css (custom styling)
│   └── js/
│       └── chart.js (via <script> tag or npm)
├── appsettings.json
├── Program.cs
├── _Imports.razor
└── AgentSquad.Runner.csproj
```

### **No CI/CD Pipeline Required**
- Local development only
- No automated deployment to cloud
- Manual testing via browser
- Optional: GitHub Actions for local testing (not required)

---

## Technology Stack Decisions

### **Blazor Server (C# .NET 8)**
**Justification:**
- ✅ Server-side rendering ensures consistent output across page refreshes
- ✅ Single language (C#) end-to-end eliminates JavaScript complexity
- ✅ Built-in routing, component model, data binding
- ✅ Simplified deployment without Node.js/npm complexity
- ✅ No WebAssembly bundle size overhead
- ❌ Requires .NET 8 runtime on client (acceptable for internal tools)

### **Bootstrap 5.x**
**Justification:**
- ✅ Responsive grid system (1024px+ breakpoints)
- ✅ Pre-built components (cards, progress bars, grids)
- ✅ No custom CSS required for MVP
- ✅ Professional appearance suitable for executives
- ✅ CDN available (no npm dependency if not required)

### **Chart.js v4.x (via JavaScript Interop)**
**Justification:**
- ✅ Lightweight (~30 KB minified)
- ✅ No animation overhead (static rendering for screenshots)
- ✅ Supports bar charts, line charts, progress indicators
- ✅ Works with Blazor JavaScript Interop
- ✅ No .NET charting library needed (simplifies deployment)

**Alternative:** Use custom SVG rendering if Chart.js causes compatibility issues.

### **System.Text.Json (Built-in .NET)**
**Justification:**
- ✅ Zero external dependencies (included in .NET 8)
- ✅ High performance for small JSON documents
- ✅ Case-insensitive property matching
- ✅ No NuGet packages needed
- ✅ Standard serialization across .NET ecosystem

### **File System I/O (System.IO)**
**Justification:**
- ✅ No database overhead
- ✅ Single data.json file stored in wwwroot
- ✅ OS file permissions provide basic access control
- ✅ Manual editing via text editor (no admin UI needed)
- ✅ No migration scripts, schema management

### **Why NOT: Entity Framework, SQLite, Dapper**
- ❌ Complexity beyond requirements (single-file data storage)
- ❌ Migration overhead for small reporting tool
- ❌ Schema management not needed
- ❌ Added deployment dependencies

### **Why NOT: React, Vue, Angular**
- ❌ Requires Node.js, npm, build toolchain
- ❌ JavaScript ecosystem complexity
- ❌ Cross-language debugging
- ❌ Violates "C# .NET 8 only" mandate

### **Why NOT: Cloud (Azure, AWS, GCP)**
- ❌ Violates "local only" requirement
- ❌ Added complexity, cost, maintenance
- ❌ Executive requirement is screenshot capture, not cloud hosting
- ❌ Network dependencies introduce failure modes

---

## Security Considerations

### **Authentication & Authorization**
- **No login system required:** Dashboard is internal tool, not public-facing
- **OS-level file permissions:** Restrict access to data.json via Windows ACLs or Linux chmod
- **Example (Windows):**
  ```
  icacls "C:\path\to\wwwroot\data\data.json" /grant "DOMAIN\Users":R /remove "Users"
  ```
- **Example (Linux):**
  ```bash
  chmod 600 /path/to/wwwroot/data/data.json  # Read-only for owner
  ```

### **Data Protection**
- **No sensitive data in JSON:** Use fictional project names, anonymized task owners
- **If sensitive metrics needed:** Encrypt data.json using Windows DPAPI or Linux OpenSSL
  ```csharp
  // Optional encryption in ProjectDataService
  using System.Security.Cryptography;
  // Decrypt before deserialization
  ```
- **File backup:** Keep data.json under version control or regular backups

### **Input Validation**
- **JSON schema validation:** Validate all deserialized objects
  ```csharp
  if (data?.Project == null || data.Milestones == null || data.Tasks == null) {
      throw new DataLoadException("Missing required fields in JSON");
  }
  ```
- **Date validation:** Ensure milestones and tasks have valid dates
  ```csharp
  if (milestone.TargetDate > DateTime.MaxValue || milestone.TargetDate < DateTime.MinValue) {
      throw new DataLoadException($"Invalid date in milestone '{milestone.Name}'");
  }
  ```

### **Error Handling (No Information Leakage)**
- ❌ **Don't expose:** Full stack traces, file paths, internal exceptions
- ✅ **Do expose:** User-friendly error messages ("Invalid JSON format")
- **Example ErrorBoundary output:**
  ```
  Error loading project data. Please check data.json format and refresh.
  ```

### **HTTPS/TLS**
- **Development:** HTTP localhost (no TLS needed)
- **Production (if internal network):** Configure HTTPS in Program.cs
  ```csharp
  app.UseHttpsRedirection(); // Optional for internal deployment
  ```

### **No Secrets in Code**
- ❌ No hardcoded API keys, passwords, connection strings
- ✅ All configuration via appsettings.json or environment variables
- ✅ data.json contains only project metadata (no credentials)

---

## Scaling Strategy

### **Current Design (Single Project, Single User)**
- Supports 1 active browser session
- data.json with 3-5 milestones, 8-15 tasks
- Load time: <2 seconds
- Memory: ~20-50 MB per session

### **Phase 2: Multiple Projects**
- Refactor data.json to array of projects:
  ```json
  {
    "projects": [
      { "name": "Q2 Mobile App", "milestones": [...], "tasks": [...] },
      { "name": "Backend API Rewrite", "milestones": [...], "tasks": [...] }
    ]
  }
  ```
- Add project selector dropdown to Dashboard
- Route: `/dashboard?projectId=project1`

### **Phase 3: Historical Metrics**
- Add `metrics_history.json` with weekly snapshots
- Track completion % trend over time
- Add trend chart to ProgressMetrics component
- Example structure:
  ```json
  {
    "snapshots": [
      { "date": "2026-04-01", "completionPercentage": 10 },
      { "date": "2026-04-08", "completionPercentage": 25 }
    ]
  }
  ```

### **Phase 4: Multi-User Collaboration**
- Add lightweight user sessions (cookie-based, no auth DB)
- Track who viewed dashboard and when
- Optional: notification system for data.json changes
- **Note:** Still no external cloud services

### **Performance Optimization (If Needed)**
- **Caching:** Cache deserialized ProjectData in memory, invalidate on user action
- **Lazy loading:** Don't load full task lists initially; paginate or collapse sections
- **Chart optimization:** Use requestAnimationFrame for smooth scrolling (if animations added)
- **Current bottleneck:** File I/O for data.json; acceptable for <1MB files

### **Deployment Scaling**
- **Single server:** Current setup scales to 10-50 concurrent browser sessions per machine
- **Multiple machines:** Run separate instances, each with own data.json copy
- **Shared file server:** Mount shared SMB/NFS drive for data.json (introduces network dependency)
- **Database option:** Migrate to SQLite for >100 concurrent users (Phase 3+)

---

## Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|-----------|
| **JSON file corruption** | Dashboard won't load; users see error | Medium | ① Regular backups of data.json; ② JSON schema validation on load; ③ Diff-friendly JSON formatting (easy to spot changes) |
| **Stale data in browser cache** | Users see old metrics after data.json edit | High | ① Educate users to press Ctrl+Shift+R (hard refresh); ② Add "Refresh Data" button in Dashboard; ③ Disable HTTP caching for data.json |
| **Chart.js version incompatibility** | Chart doesn't render; visual gaps | Low | ① Lock Chart.js version in script tag; ② Test on target browsers weekly; ③ Have fallback SVG rendering option |
| **Blazor Server session timeout** | Users lose state after 30 minutes inactivity | Medium | ① Set reasonable session timeout (60 min); ② Add "Session expired" warning; ③ Inform users to press F5 to reconnect |
| **Scaling to 100+ tasks** | Page load time exceeds 2s; scroll lag | Low | ① Implement pagination/lazy loading; ② Split tasks into sections; ③ Migrate to lighter charting library |
| **Cross-browser rendering differences** | Screenshots look different on Safari vs. Chrome | Low | ① Test on Chrome, Edge, Safari; ② Use CSS Grid instead of Flexbox if needed; ③ Embed fonts (avoid system font rendering differences) |
| **data.json not found at startup** | FileNotFoundException; error page | Medium | ① Create sample data.json during first deployment; ② Validate file path in appsettings.json; ③ Clear error message: "Create data.json in wwwroot/data/" |
| **Large data.json (>5MB)** | Deserialization lag; memory spike | Very Low | ① For now: not a concern (MVP is <1MB); ② Future: stream JSON parsing or split into multiple files |
| **User edits data.json while dashboard is open** | Old cached data shown; inconsistency | Medium | ① Implement optional auto-refresh every 30 seconds; ② Add manual "Sync" button; ③ Show "Last loaded: HH:MM" timestamp |
| **No audit trail of data changes** | Can't track who changed what in data.json | Low | ① Store data.json in Git repo with commits; ② Use GitHub/ADO for version history; ③ Not needed for MVP (fiction project) |

### **Top 3 Priority Mitigations**
1. **JSON Validation:** Fail-fast with clear error messages; don't render broken UI
2. **Browser Cache:** Ensure users can force-refresh and see latest data
3. **File Path:** Document exact location of data.json in README

---

## Implementation Roadmap

### **Week 1 (MVP)**
- Day 1: Blazor Server project scaffold, Program.cs setup
- Day 2: ProjectDataService, data.json parsing, sample data
- Day 3: Dashboard.razor layout, Bootstrap grid
- Day 4: MilestoneTimeline, StatusCard components
- Day 5: ProgressMetrics chart, error handling, testing
- **Deliverable:** Working dashboard with sample data, screenshot-ready

### **Week 2 (Polish)**
- Day 1: Performance optimization, caching strategy
- Day 2: Responsive testing (1024px, 1366px, 1920px screens)
- Day 3: README.md, data.json schema documentation
- Day 4: Manual testing, screenshot validation for PowerPoint
- Day 5: Bug fixes, deployment to production server
- **Deliverable:** Production-ready dashboard, documented for maintenance

### **Future Phases**
- Multi-project support, historical metrics, export to PDF/HTML, team collaboration features

---

## Documentation Requirements

### **README.md**
- Setup instructions (`dotnet run`)
- data.json schema reference
- Component diagram
- Screenshot examples
- Troubleshooting (common errors)

### **data.json.sample**
- Complete example with 3 milestones, 10 tasks
- Realistic project (e.g., "Q2 Mobile App Release")
- All optional fields explained

### **DEPLOYMENT.md**
- Production build steps
- Windows/Linux deployment
- Firewall rules (if applicable)
- Performance tuning tips

---

**End of Architecture Document**