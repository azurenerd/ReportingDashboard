# Architecture

## Overview & Goals

AgentSquad Executive Dashboard is a single-page, local-only Blazor Server application that enables program managers to generate presentation-ready screenshots of project status for PowerPoint presentations. Built on .NET 8 with zero infrastructure overhead, the dashboard loads project data from a JSON configuration file and displays:

- **Timeline Visualization**: Horizontal bar chart showing 4-5 milestones chronologically sorted with status indicators (Completed/In Progress/At Risk/Upcoming)
- **Status Summary**: Four distinct cards displaying work item counts (Shipped/In Progress/Carried Over/At Risk) with progress bars and color coding
- **Executive Readiness**: Light-theme optimized for projector display at 1920×1080 resolution with font sizes ≥18pt visible from 10+ feet

**Architecture Goals**:
1. Zero external dependencies beyond standard NuGet packages
2. Single developer deployment via `dotnet run` with no configuration
3. Sub-2-second dashboard load time (startup to fully rendered)
4. Graceful error handling for missing or malformed data.json
5. Screenshot-ready output for direct PowerPoint embedding
6. Reusable component library for future project dashboards

---

## System Components

### 1. Dashboard.razor (Orchestrator Page)

**Responsibility**: Main application entry point; orchestrates data loading and component composition. Manages application-level state (ProjectData) and passes to child components via cascading parameters.

**Scope**:
- Route: `/` (root path)
- Loads ProjectData from DataService on initialization
- Displays loading indicator during data fetch
- Shows user-friendly error messages on failure
- Passes ProjectData to all child components via CascadingValue

**Key Methods**:
- `OnInitializedAsync()`: Async initialization; calls DataService.LoadProjectDataAsync()
- `LoadProjectDataAsync()`: Wraps service call with error handling

**Data Ownership**:
- `projectData`: ProjectData instance (loaded at startup, immutable)
- `isLoading`: bool (true until data loads or error occurs)
- `errorMessage`: string (populated if FileNotFoundException, JsonException, or InvalidOperationException occurs)

**Dependencies**:
- DataService (injected, Scoped): JSON file loading and caching
- MilestoneTimeline component: Child component
- StatusSummary component: Child component
- MudBlazor: Container and grid layout

---

### 2. MilestoneTimeline.razor (Timeline Visualization)

**Responsibility**: Render horizontal bar chart showing milestones with status indicators. Display milestones chronologically left-to-right sorted by TargetDate.

**Scope**:
- Receives ProjectData via [CascadingParameter]
- Reads Milestone collection (4-10 items expected)
- Renders Chart.js horizontal bar chart (via CChart.js)
- Color-codes bars by status: Green (Completed), Blue (In Progress), Orange (At Risk), Gray (Upcoming)
- No scrolling; fits within 1920×1080 viewport

**Key Logic**:
- Sort milestones by TargetDate ascending
- Map milestone statuses to CSS hex colors
- Build ChartJs configuration with horizontal (y) axis
- Responsive height (default 300px, configurable)

**Data Ownership**:
- `chartConfig`: ChartConfiguration built from ProjectData.Milestones
- `chartReference`: ChartJs component reference (for lifecycle)

**Dependencies**:
- ProjectData (via CascadingParameter)
- CChart.js NuGet package (Chart.js C# wrapper)
- MudBlazor (styling and container)

---

### 3. StatusSummary.razor (Status Container)

**Responsibility**: Container component arranging four StatusCard instances horizontally. Calculates overall project metrics (total work, percentages) and distributes to child cards.

**Scope**:
- Receives ProjectData via [CascadingParameter]
- Reads WorkItemAggregation counts (Shipped, InProgress, CarriedOver, AtRisk, TotalCapacity)
- Renders responsive grid (xs=12, sm=6, md=3 for 1920×1080)
- Calculates individual and overall percentages
- Optional: Display overall progress bar (configurable)

**Key Calculations**:
```
totalShipped = WorkItems.Shipped
totalInProgress = WorkItems.InProgress
totalCarriedOver = WorkItems.CarriedOver
totalAtRisk = WorkItems.AtRisk
grandTotal = sum of above
percentage = (count / grandTotal) * 100 for each status
```

**Data Ownership**:
- Calculated metrics (totalShipped, totalInProgress, etc.)
- Layout state (Grid configuration)
- Reads-only from ProjectData.WorkItems (no mutations)

**Dependencies**:
- ProjectData (via CascadingParameter)
- StatusCard component (reused 4x)
- MudBlazor Grid and Item components

---

### 4. StatusCard.razor (Reusable Status Widget)

**Responsibility**: Display single status metric with label, count, progress bar, and color indicator. Fully parameterized and reusable. Optimized for large text (18pt+ numbers) for projector visibility.

**Scope**:
- Pure presentation component (no business logic)
- Receives all state via [Parameter] attributes
- Renders: MudCard → Label (h6, 16pt) → Count (h3, 36pt) → LinearProgress → Percentage (14pt)
- Minimum height 180px; box-shadow for elevation
- Four color options: Success (Green), Info (Blue), Warning (Orange), Error (Red)

**Parameters**:
- `Label`: string (e.g., "Shipped", "In Progress")
- `Count`: int (0-999)
- `PercentComplete`: double (0.0-100.0)
- `Color`: Color enum (MudBlazor)
- `CustomClass`: string (optional CSS override)

**Data Ownership**:
- None (stateless; all data via parameters)

**Dependencies**:
- MudBlazor: Card, Text, LinearProgress components

---

### 5. DataService.cs (Business Logic Service)

**Responsibility**: Load JSON configuration file from disk, deserialize to strongly-typed ProjectData model, handle file I/O errors gracefully, and cache loaded data in memory.

**Scope**:
- Reads wwwroot/data/data.json
- Uses System.Text.Json for deserialization (PropertyNameCaseInsensitive: true)
- Caches for 300 seconds (5 minutes)
- Validates schema and throws InvalidOperationException on mismatch
- Catches FileNotFoundException, JsonException, and general Exception

**Public Interface**:
- `LoadProjectDataAsync(): Task<ProjectData>` – Load or return cached data
- `ClearCache(): void` – Force reload from disk on next call
- `GetCacheAgeSeconds(): int` – Return current cache age

**Error Handling**:
- FileNotFoundException: "Data file not found at wwwroot/data/data.json. Please ensure the file exists and is readable."
- JsonException: "Data file contains invalid JSON. Please check syntax at wwwroot/data/data.json."
- InvalidOperationException: "Data structure does not match expected schema. Ensure all required fields are present."

**Data Ownership**:
- `_cachedProjectData`: ProjectData instance (immutable after deserialization)
- `_lastLoadTime`: DateTime (for cache expiration)

**Dependencies**:
- System.Text.Json (built-in)
- System.IO (built-in)
- ProjectData model

**Service Lifetime**: Scoped (one per Blazor Server circuit; not Singleton to allow cache clearing)

---

### 6. Models / Data Contracts

#### ProjectData.cs
```
- projectName: string (required, max 256 chars)
- period: string (required, e.g., "April - June 2026")
- description: string (optional project summary)
- startDate: DateTime (ISO 8601)
- endDate: DateTime (ISO 8601, must be >= startDate)
- milestones: List<Milestone> (4-10 items required)
- workItems: WorkItemAggregation (exactly one)
- version: string (defaults to "1.0"; validated for "1.x" compatibility)
```

**Validation**:
- ProjectName: Required, non-empty
- Milestones.Count: Between 4 and 10 inclusive
- WorkItems.Shipped + InProgress + CarriedOver + AtRisk <= TotalCapacity
- All numeric fields: Non-negative integers
- StartDate <= EndDate

---

#### Milestone.cs
```
- id: string (UUID format, unique)
- name: string (required, max 128 chars)
- targetDate: DateTime (ISO 8601)
- completionDate: DateTime? (null if not completed)
- status: MilestoneStatus enum
  - Upcoming: 0
  - InProgress: 1
  - AtRisk: 2
  - Completed: 3
- description: string (optional, max 512 chars)
- owners: List<string> (optional, max 5 entries)
```

---

#### WorkItemAggregation.cs
```
- shipped: int (>= 0)
- inProgress: int (>= 0)
- carriedOver: int (>= 0)
- atRisk: int (>= 0)
- totalCapacity: int (>= sum of above)
```

---

### 7. Program.cs (Application Configuration)

**Responsibility**: Bootstrap Blazor Server application, register services, configure MudBlazor theme and logging.

**Service Registration**:
- `AddRazorPages()`: Razor page support
- `AddServerSideBlazor()`: Blazor Server hosting
- `AddMudServices()`: MudBlazor Material Design components
- `AddScoped<DataService>()`: JSON data service (scoped lifetime)

**Middleware Configuration**:
- Exception handling (production)
- HSTS (production)
- Static file serving (wwwroot)
- Blazor hub mapping
- Fallback to _Host.cshtml

**Logging Configuration**:
- Default level: Information (production), Debug (development)
- Microsoft: Warning (suppress noise)
- AgentSquad.Dashboard: Information (custom component logs)

---

## Component Interactions

### Primary Use Case: Application Startup and Dashboard Render

**Flow**:
1. **User navigates to http://localhost:5000**
   - Browser requests `/` route
   - ASP.NET Core routes to Dashboard.razor page component

2. **Dashboard.razor initializes**
   - `OnInitializedAsync()` fires automatically
   - Injects `DataService` from DI container
   - Calls `await dataService.LoadProjectDataAsync()`
   - Sets `isLoading = true` until completion

3. **DataService loads JSON file**
   - Checks in-memory cache (if < 300 seconds old, return cached copy)
   - If cache miss: Reads `wwwroot/data/data.json` via `File.ReadAllTextAsync()`
   - Deserializes JSON to `ProjectData` via `JsonSerializer.Deserialize<ProjectData>()`
   - Validates schema (throws InvalidOperationException on mismatch)
   - Caches result and returns to caller

4. **Dashboard receives ProjectData**
   - Sets `isLoading = false`
   - Calls `StateHasChanged()` to trigger re-render
   - Renders CascadingValue<ProjectData> wrapping child components

5. **Child components receive data via cascading parameter**
   - **MilestoneTimeline**: Receives ProjectData, reads Milestones collection, builds ChartJs config, renders horizontal bar chart
   - **StatusSummary**: Receives ProjectData, reads WorkItems, calculates totals and percentages, renders 4 StatusCard instances
   - **StatusCard** (4x): Receives Label, Count, PercentComplete, Color parameters; renders card with progress bar

6. **Browser receives rendered HTML**
   - Single-page HTML document with:
     - MudBlazor theme CSS (Material Design light theme)
     - Custom app.css overrides
     - Dashboard layout (MudContainer + MudGrid)
     - MilestoneTimeline Chart.js canvas
     - 4 StatusCard components with MudBlazor cards
     - Blazor signalR circuit JavaScript for server-client communication

7. **User takes screenshot**
   - Entire dashboard visible at 1920×1080 resolution
   - Light theme optimized for projector display
   - All text, numbers, and colors clear and executive-ready
   - User copies image to PowerPoint slide

---

### Secondary Flow: Data Refresh (Manual)

**Option A: Browser Refresh (F5)**
- User edits data.json file
- User presses F5 in browser
- Blazor circuit reconnects
- Dashboard.razor OnInitializedAsync() fires again
- DataService.ClearCache() is called implicitly (cache age > 300 seconds)
- DataService loads updated data.json from disk
- Components re-render with new data

**Option B: Refresh Button (Phase 2 Enhancement)**
- User clicks [Refresh] button on dashboard
- EventCallback fires in Dashboard.razor
- Calls `dataService.ClearCache()`
- Calls `await dataService.LoadProjectDataAsync()` again
- ProjectData property updates
- StateHasChanged() triggers re-render
- All child components receive updated ProjectData via cascading
- MilestoneTimeline rebuilds ChartJs configuration
- StatusCard components recalculate percentages
- UI updates instantly

---

### Component Dependency Graph

```
Program.cs
  ├─ Services
  │   └─ DataService (Scoped)
  │       └─ System.Text.Json
  │       └─ System.IO
  │
  └─ Pages
      └─ Dashboard.razor
          ├─ DataService (injected)
          ├─ MilestoneTimeline (child)
          │   ├─ ProjectData (cascading parameter)
          │   ├─ CChart.js library
          │   └─ MudBlazor (styling)
          │
          └─ StatusSummary (child)
              ├─ ProjectData (cascading parameter)
              ├─ StatusCard (child × 4)
              │   ├─ MudBlazor.Card
              │   ├─ MudBlazor.Text
              │   └─ MudBlazor.LinearProgress
              │
              └─ ProgressBar (child, optional)
                  ├─ ProjectData (cascading parameter)
                  └─ MudBlazor.LinearProgress
```

---

## Data Model

### Entity Definitions

#### ProjectData (Root Aggregate)

**Purpose**: Represents entire dashboard state from data.json file. Immutable after deserialization.

**Fields**:
| Field | Type | Required | Constraints | Example |
|-------|------|----------|-------------|---------|
| projectName | string | Yes | Non-empty, ≤256 chars | "Q2 Product Launch" |
| period | string | Yes | Non-empty | "April - June 2026" |
| description | string | No | ≤512 chars | "Deliver core product features" |
| startDate | DateTime | Yes | ISO 8601, ≤ endDate | "2026-04-01T00:00:00Z" |
| endDate | DateTime | Yes | ISO 8601, ≥ startDate | "2026-06-30T23:59:59Z" |
| milestones | List<Milestone> | Yes | 4-10 items, sorted by TargetDate | [...] |
| workItems | WorkItemAggregation | Yes | Sum ≤ TotalCapacity | {...} |
| version | string | No | Defaults to "1.0" | "1.0" |

---

#### Milestone

**Purpose**: Represents single project milestone with temporal and status information.

**Fields**:
| Field | Type | Required | Constraints | Example |
|-------|------|----------|-------------|---------|
| id | string | Yes | UUID format, unique | "milestone-design-001" |
| name | string | Yes | Non-empty, ≤128 chars | "Design Complete" |
| targetDate | DateTime | Yes | ISO 8601 | "2026-04-30T00:00:00Z" |
| completionDate | DateTime? | No | ISO 8601, ≤ today | "2026-04-28T00:00:00Z" |
| status | MilestoneStatus | Yes | Enum: Upcoming, InProgress, AtRisk, Completed | "completed" |
| description | string | No | ≤512 chars | "Complete design phase" |
| owners | List<string> | No | ≤5 entries, max 64 chars each | ["Design Lead"] |

**MilestoneStatus Enum**:
```
Upcoming (0)    – Not started, target future
InProgress (1)  – Currently being worked on
AtRisk (2)      – May not meet target date
Completed (3)   – Finished and delivered
```

**Visualization Colors**:
- Upcoming: #9E9E9E (Gray)
- InProgress: #2196F3 (Blue)
- AtRisk: #FF9800 (Orange)
- Completed: #4CAF50 (Green)

---

#### WorkItemAggregation

**Purpose**: Pre-aggregated counts of work items by status. No individual item list (aggregated for dashboard simplicity).

**Fields**:
| Field | Type | Required | Constraints | Example |
|-------|------|----------|-------------|---------|
| shipped | int | Yes | ≥0, ≤ totalCapacity | 12 |
| inProgress | int | Yes | ≥0, ≤ totalCapacity | 5 |
| carriedOver | int | Yes | ≥0, ≤ totalCapacity | 3 |
| atRisk | int | Yes | ≥0, ≤ totalCapacity | 2 |
| totalCapacity | int | Yes | ≥ (sum of above) | 25 |

**Validation Rule**: `shipped + inProgress + carriedOver + atRisk ≤ totalCapacity`

---

### Entity Relationships

```
ProjectData (1) ──────── (Many) Milestone
    │
    │ Contains exactly one
    │
    └────────────────────── WorkItemAggregation (1)
```

**Constraints**:
- One ProjectData per data.json file
- 4 to 10 Milestones per ProjectData (enforced)
- Exactly one WorkItemAggregation per ProjectData
- Milestone IDs unique within ProjectData
- No circular references or orphaned entities
- All dates in ISO 8601 UTC timezone

---

### Storage Strategy: File-Based JSON

**Location**: `wwwroot/data/data.json`

**File Format**:
- Encoding: UTF-8
- Line endings: CRLF (Windows standard) or LF (both supported)
- Structure: Single JSON object with ProjectData schema
- Size limit: 10 MB (enforced by DataService validation)

**Example data.json**:
```json
{
  "projectName": "Q2 Product Launch",
  "period": "April - June 2026",
  "description": "Deliver core product features for Q2 milestone",
  "startDate": "2026-04-01T00:00:00Z",
  "endDate": "2026-06-30T23:59:59Z",
  "version": "1.0",
  "milestones": [
    {
      "id": "milestone-design-001",
      "name": "Design Complete",
      "targetDate": "2026-04-30T00:00:00Z",
      "completionDate": "2026-04-28T00:00:00Z",
      "status": "completed",
      "description": "Complete design phase for all features",
      "owners": ["Design Lead"]
    },
    {
      "id": "milestone-dev-001",
      "name": "Development Phase 1",
      "targetDate": "2026-05-31T00:00:00Z",
      "completionDate": null,
      "status": "in-progress",
      "description": "Core backend API development",
      "owners": ["Engineering Lead", "Backend Team"]
    },
    {
      "id": "milestone-qa-001",
      "name": "QA & Testing",
      "targetDate": "2026-06-15T00:00:00Z",
      "completionDate": null,
      "status": "upcoming",
      "description": "Quality assurance and regression testing",
      "owners": ["QA Lead"]
    },
    {
      "id": "milestone-release-001",
      "name": "Release to Production",
      "targetDate": "2026-06-30T00:00:00Z",
      "completionDate": null,
      "status": "at-risk",
      "description": "Production deployment and cutover",
      "owners": ["Ops Lead"]
    }
  ],
  "workItems": {
    "shipped": 12,
    "inProgress": 5,
    "carriedOver": 3,
    "atRisk": 2,
    "totalCapacity": 25
  }
}
```

**Refresh Behavior**:
- No automatic polling or real-time updates (MVP)
- User manually edits data.json file with text editor or programmatic sync
- Browser refresh (F5) or manual refresh button clears cache and reloads
- DataService caches for 300 seconds between requests
- No distributed caching or multi-machine sync (Phase 2 enhancement)

**Backup Strategy**:
- User responsibility (Windows File History, cloud sync, or source control)
- Recommended: Store data.json in Git for version history

---

### Calculated Fields (Derived, Not Stored)

Computed at runtime by StatusSummary and child components:

```csharp
public static int GetTotalWork(ProjectData data)
    => data.WorkItems.Shipped 
       + data.WorkItems.InProgress 
       + data.WorkItems.CarriedOver 
       + data.WorkItems.AtRisk;

public static double GetCompletionPercent(ProjectData data)
{
    int total = data.GetTotalWork();
    return total > 0 ? (double)data.WorkItems.Shipped / total * 100 : 0;
}

public static int GetUncommittedWork(ProjectData data)
    => data.WorkItems.TotalCapacity - data.GetTotalWork();

public static int GetOnTimeCount(ProjectData data)
    => data.Milestones.Count(m => m.Status == MilestoneStatus.Completed);

public static int GetAtRiskCount(ProjectData data)
    => data.Milestones.Count(m => m.Status == MilestoneStatus.AtRisk);
```

---

## API Contracts

### Service Interfaces

#### IDataService

**Purpose**: Define contract for loading and caching project data from JSON file.

**Public Methods**:

```csharp
public interface IDataService
{
    /// <summary>
    /// Load project data from wwwroot/data/data.json.
    /// Returns cached result if cache is less than 300 seconds old.
    /// </summary>
    /// <returns>ProjectData object deserialized from JSON</returns>
    /// <exception cref="FileNotFoundException">If data.json does not exist</exception>
    /// <exception cref="JsonException">If JSON is malformed</exception>
    /// <exception cref="InvalidOperationException">If schema validation fails</exception>
    Task<ProjectData> LoadProjectDataAsync();

    /// <summary>
    /// Clear in-memory cache to force fresh read from disk on next LoadProjectDataAsync() call.
    /// Used for manual refresh (Phase 2) or testing.
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Get current cache age in seconds (0 if no active cache).
    /// </summary>
    int GetCacheAgeSeconds();
}
```

**Implementation**: `DataService.cs` (Scoped lifetime)

---

### Component Parameters

#### StatusCard Parameters

```csharp
public class StatusCardParameters
{
    /// <summary>
    /// Display label (e.g., "Shipped", "In Progress").
    /// Displayed as h6 (16pt font).
    /// </summary>
    [Parameter]
    public string Label { get; set; }

    /// <summary>
    /// Item count (0-999).
    /// Displayed as h3 (36pt font) for projector visibility.
    /// </summary>
    [Parameter]
    public int Count { get; set; }

    /// <summary>
    /// Progress bar percentage (0.0-100.0).
    /// Used for LinearProgress value.
    /// </summary>
    [Parameter]
    public double PercentComplete { get; set; }

    /// <summary>
    /// MudBlazor Color enum for progress bar and border.
    /// Values: Success (green), Info (blue), Warning (orange), Error (red).
    /// </summary>
    [Parameter]
    public Color Color { get; set; } = Color.Primary;

    /// <summary>
    /// Optional CSS class for custom styling.
    /// </summary>
    [Parameter]
    public string CustomClass { get; set; }
}
```

---

#### MilestoneTimeline Parameters

```csharp
public class MilestoneTimelineParameters
{
    /// <summary>
    /// ProjectData cascading parameter containing Milestone collection.
    /// </summary>
    [CascadingParameter]
    public ProjectData ProjectData { get; set; }

    /// <summary>
    /// Chart height in pixels (default: 300).
    /// </summary>
    [Parameter]
    public int ChartHeightPixels { get; set; } = 300;

    /// <summary>
    /// Show completed milestones in timeline (default: true).
    /// If false, filters Completed status from display.
    /// </summary>
    [Parameter]
    public bool ShowCompleted { get; set; } = true;
}
```

---

#### StatusSummary Parameters

```csharp
public class StatusSummaryParameters
{
    /// <summary>
    /// ProjectData cascading parameter containing WorkItems.
    /// </summary>
    [CascadingParameter]
    public ProjectData ProjectData { get; set; }

    /// <summary>
    /// Show overall progress bar below status cards (default: true).
    /// </summary>
    [Parameter]
    public bool ShowProgressBar { get; set; } = true;
}
```

---

### Error Response Contracts

#### DataServiceException

```csharp
public class DataServiceException : Exception
{
    public DataServiceException(string message) 
        : base(message) { }

    public DataServiceException(string message, Exception innerException) 
        : base(message, innerException) { }

    public ErrorCode ErrorCode { get; set; }
}

public enum ErrorCode
{
    FileNotFound = 1,
    JsonMalformed = 2,
    DeserializationFailed = 3,
    ValidationFailed = 4,
    UnknownError = 5
}
```

**Error Messages** (user-facing, displayed in ErrorDisplay component):

| Scenario | Message |
|----------|---------|
| File missing | "Data file not found at wwwroot/data/data.json. Please ensure the file exists and is readable." |
| JSON syntax error | "Data file contains invalid JSON. Please check syntax at wwwroot/data/data.json. Common issues: missing commas, unclosed quotes, trailing commas." |
| Schema mismatch | "Data structure does not match expected schema. Ensure all required fields are present: projectName, period, milestones (array with 4-10 items), workItems (object)." |
| Validation failure | "Data validation failed: {specific constraint violated}" |
| Unknown error | "An unexpected error occurred while loading project data. Please refresh and try again." |

---

#### ErrorDisplay Component

```csharp
public class ErrorDisplayParameters
{
    /// <summary>
    /// Error message to display to user.
    /// </summary>
    [Parameter]
    public string Message { get; set; }

    /// <summary>
    /// Error severity: Error (red), Warning (orange), Info (blue).
    /// Default: Error.
    /// </summary>
    [Parameter]
    public Severity Severity { get; set; } = Severity.Error;

    /// <summary>
    /// Show refresh button (default: true).
    /// </summary>
    [Parameter]
    public bool ShowRefreshButton { get; set; } = true;

    /// <summary>
    /// Callback fired when user clicks refresh button.
    /// </summary>
    [Parameter]
    public EventCallback OnRefreshClicked { get; set; }
}
```

---

### Request/Response Shapes

#### Dashboard Load Request

**Route**: `/`

**Triggers**: User navigates to root URL or page first loads

**Flow**:
- No explicit request payload (GET request)
- Dashboard.razor OnInitializedAsync() implicitly requests data from DataService
- DataService.LoadProjectDataAsync() returns ProjectData or throws exception

---

#### DataService Response

**Success Response**:
```
Type: ProjectData
Fields:
  - projectName: string
  - period: string
  - description: string
  - startDate: DateTime
  - endDate: DateTime
  - milestones: Milestone[]
  - workItems: {
      shipped: int,
      inProgress: int,
      carriedOver: int,
      atRisk: int,
      totalCapacity: int
    }
  - version: string
```

**Error Response** (thrown as exception):
```
Type: InvalidOperationException or FileNotFoundException
Message: User-friendly error description (see Error Messages table above)
```

---

#### Component Render Contract

**MilestoneTimeline renders**:
```
Canvas element with Chart.js configuration:
{
  Type: "bar"
  Labels: ["Design Complete", "Development Phase 1", "QA & Testing", "Release to Production"]
  Datasets: [{
    Label: "Milestones",
    Data: [100, 100, 100, 100],
    BackgroundColor: ["#4CAF50", "#2196F3", "#9E9E9E", "#FF9800"],
    BorderRadius: 4
  }]
  Options: {
    IndexAxis: "y",                    // Horizontal bars
    Responsive: true,
    Maintainaspect: true,
    Plugins: {
      Legend: { Display: false },
      Tooltip: { Enabled: true }
    }
  }
}
```

**StatusSummary renders**:
```
MudGrid (Spacing=2):
  [0] StatusCard(Label="Shipped", Count=12, PercentComplete=48, Color=Success)
  [1] StatusCard(Label="In Progress", Count=5, PercentComplete=20, Color=Info)
  [2] StatusCard(Label="Carried Over", Count=3, PercentComplete=12, Color=Warning)
  [3] StatusCard(Label="At Risk", Count=2, PercentComplete=8, Color=Error)
```

**StatusCard renders**:
```
MudCard (height=180px min, shadow=0 2px 4px):
  MudText (h6, 16pt): "Shipped"
  MudText (h3, 36pt): "12"
  MudLinearProgress (value=48%, color=Green)
  MudText (caption, 14pt): "48%"
```

---

## Infrastructure Requirements

### Hosting & Deployment

#### Development Environment

**Specifications**:
```
OS:        Windows 10/11 or Windows Server 2019+
Runtime:   .NET 8.0 SDK (latest patch) [e.g., 8.0.1]
IDE:       Visual Studio 2022 Community/Professional
Browser:   Chrome 90+ or Edge 90+
Ports:     5000 (HTTP), 5001 (HTTPS)
Protocol:  HTTP (localhost), HTTPS (localhost)
```

**Startup Command**:
```bash
dotnet run --launch-profile https
```

**Output**: Application listens on https://localhost:5001 and http://localhost:5000

**Developer Requirements**:
- .NET 8.0 SDK installed globally
- Visual Studio 2022 or VS Code with C# extension
- Admin privileges for first-time HTTPS certificate generation

---

#### Production Deployment: Single Machine

**Specifications**:
```
OS:              Windows 10/11 or Windows Server 2016+
Runtime:         .NET 8.0 Runtime (publish self-contained, no SDK required)
Executable:      AgentSquad.Dashboard.exe (published artifact)
Location:        C:\dashboards\agentsquad\ (or user-specified path)
Port:            5000 (HTTP)
Protocol:        HTTP (internal network, HTTPS not required)
Process Model:   Direct executable or batch file wrapper
Auto-start:      Optional (via scheduled task or Windows Service in Phase 2)
```

**Deployment Command**:
```bash
dotnet publish -c Release --self-contained -r win-x64 -o C:\dashboards\agentsquad
```

**Publish Artifact Structure**:
```
C:\dashboards\agentsquad\
  ├─ AgentSquad.Dashboard.exe
  ├─ AgentSquad.Dashboard.dll
  ├─ appsettings.json
  ├─ wwwroot/
  │   ├─ data/
  │   │   └─ data.json (user edits this)
  │   ├─ css/
  │   │   └─ app.css
  │   └─ index.html
  ├─ .NET runtime files (200+ MB for self-contained)
  └─ ... (other assemblies)
```

**Startup**:
```bash
C:\dashboards\agentsquad\AgentSquad.Dashboard.exe
```

**Access**: http://localhost:5000 or http://<machine-ip>:5000 from internal network

---

#### Production Deployment: Shared IIS Server

**Specifications**:
```
OS:                   Windows Server 2016+ with IIS 7.5+
.NET 8 Hosting Bundle: Latest version installed on server
App Pool:             .NET CLR version = "No Managed Code"
App Pool Identity:    ApplicationPoolIdentity (default) or custom service account
Bindings:             HTTP port 80 or 8080 (internal network only)
SSL/TLS:              Not required (internal deployment)
Process Model:        Integrated (default for Blazor Server)
Session Mode:         In-process (default Blazor Server)
```

**IIS Configuration**:

```xml
<configuration>
  <system.applicationHost>
    <applicationPool name="AgentSquad.Dashboard" managedRuntimeVersion="v4.0" managedPipelineMode="Integrated">
      <processModel identityType="ApplicationPoolIdentity" />
      <recycling>
        <periodicRestart privateMemory="104857600" />
      </recycling>
    </applicationPool>
    
    <sites>
      <site name="AgentSquad.Dashboard" id="2">
        <application path="/" applicationPool="AgentSquad.Dashboard">
          <virtualDirectory path="/" physicalPath="C:\dashboards\agentsquad\publish" />
        </application>
        <bindings>
          <binding protocol="http" bindingInformation="*:8080:*" />
        </bindings>
      </site>
    </sites>
  </system.applicationHost>
</configuration>
```

**Pre-deployment**:
1. Install .NET 8 Hosting Bundle: https://dotnet.microsoft.com/download/dotnet/8.0
2. Run: `dotnet publish -c Release -o C:\dashboards\agentsquad\publish`
3. Set folder permissions: IIS_IUSRS group needs Read + Execute
4. Create IIS app pool and site (manual or via PowerShell script)
5. Verify: Navigate to http://localhost:8080

---

### Storage Requirements

#### Local File Storage

**data.json**:
```
Location:  wwwroot/data/data.json
Size:      < 10 MB (enforced by DataService validation)
Encoding:  UTF-8
Ownership: User running the application
Access:    Read-only at runtime (safe to edit via external editor while app running due to Blazor circuit isolation)
Backup:    User responsibility (Windows File History, cloud sync like OneDrive, or Git)
Retention: Single file (no archival; use Git for version history)
```

**Application Directory**:
```
Total Disk Space: 1 GB minimum (for .NET runtime + application + data)

Development:
  C:\workspace\AgentSquad\
    ├─ AgentSquad.Dashboard.csproj
    ├─ Program.cs
    ├─ Pages/
    ├─ Components/
    ├─ Models/
    ├─ Services/
    ├─ wwwroot/
    │   └─ data/
    │       └─ data.json
    └─ bin/obj/ (build artifacts)

Production:
  C:\dashboards\agentsquad\
    ├─ AgentSquad.Dashboard.exe
    ├─ AgentSquad.Dashboard.dll
    ├─ wwwroot/
    │   └─ data/
    │       └─ data.json
    └─ .NET runtime (200+ MB self-contained)
```

---

### Networking

#### Local Machine (Development)

```
Localhost IPv4:  127.0.0.1:5000 (HTTP)
Localhost IPv6:  [::1]:5000
HTTPS:           127.0.0.1:5001 (development certificate)
External Network: Not accessible
Firewall:        No rules needed
VPN:             Not applicable
```

---

#### Shared Server (Internal Network)

```
Network Access:     Internal network only (no internet-facing)
Port:               8080 (HTTP) or configurable
Firewall:           Open port 8080 to internal network range (e.g., 192.168.x.x/16)
DNS:                Optional (access via server IP or hostname from internal DNS)
HTTPS:              Not required (internal network)
Proxy/Gateway:      Not needed
CDN:                Not applicable
Load Balancer:      Not applicable for MVP (single instance)
External Access:    Explicitly blocked (not supported)
```

---

### Database & Persistence

**No Database Required**:
```
Data Source:        Single JSON file (wwwroot/data/data.json)
SQL Server:         Not needed
PostgreSQL:         Not needed
SQLite:             Not used (unnecessary complexity for static dashboard)
ORM (EF Core):      Not used (no relational mapping needed)
Migration Scripts:  Not applicable
Schema Versioning:  Manual via "version" field in data.json
Transactions:       Not needed (read-only dashboard)
Connection Pools:   Not needed
```

**Why JSON Instead of Database**:
- MVP scope: Single file, pre-aggregated data
- Local-only deployment: No multi-machine sync required
- Zero infrastructure: User edits JSON directly with text editor
- Schema simplicity: No migrations or ORM overhead
- Scalability for MVP: File-based adequate for <100 MB data

---

### Logging & Monitoring

#### Application Logging

**Framework**: Microsoft.Extensions.Logging (built-in)

**Providers**:
- Console (development)
- EventLog (Windows Event Viewer, production)

**Configuration** (appsettings.json):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "AgentSquad.Dashboard": "Information"
    }
  }
}
```

**Log Events**:
- App startup: "Dashboard application started on https://localhost:5001"
- Data load success: "Loaded project data from wwwroot/data/data.json (150 ms)"
- Data load error: "Failed to load data: FileNotFoundException at wwwroot/data/data.json"
- Cache hit: "Returning cached ProjectData (age: 45 seconds)"
- Blazor circuit: "Blazor circuit established for client {id}"

**Retention**: No persistent log storage in MVP (console output only)

---

#### Health Checks

**MVP Approach** (Manual Validation):
- User opens browser to http://localhost:5000
- Dashboard loads without errors
- All components render correctly
- Verify <2 seconds startup time

**Phase 2 Enhancement** (Optional):
```csharp
// Add health check endpoint
app.MapHealthChecks("/health");

// In Program.cs:
builder.Services.AddHealthChecks()
    .AddCheck("data_file", () =>
    {
        if (File.Exists("wwwroot/data/data.json"))
            return HealthCheckResult.Healthy();
        return HealthCheckResult.Unhealthy("data.json missing");
    });
```

**No Formal Monitoring in MVP**:
- No uptime monitoring (internal tool)
- No alerting system
- No metrics collection
- No APM (Application Performance Monitoring)

---

### Performance Requirements

#### Load Time SLA

**Target**: <2 seconds from app start to fully rendered dashboard

**Breakdown**:
```
.NET startup:           <500 ms (Kestrel host, assembly loading)
JSON file read:         <50 ms (wwwroot/data/data.json via File.ReadAllTextAsync)
JSON deserialization:   <100 ms (System.Text.Json for typical 1 MB file)
Blazor component tree:  <1000 ms (Dashboard + MilestoneTimeline + StatusSummary + 4× StatusCard)
Chart.js rendering:     <300 ms (Canvas initialization and bar chart)
Total:                  ~1.95 seconds (theoretical maximum)
```

**Measurement**:
- Use `Stopwatch` in DataService and Dashboard.razor
- Log timing to console/EventLog
- Alert if any phase exceeds budget

---

#### Memory Usage

**Target**: <100 MB resident memory

**Breakdown**:
```
Blazor Server runtime:  ~40-50 MB (ASP.NET Core + assemblies)
MudBlazor CSS/JS:       ~5-10 MB (Material Design libraries)
ProjectData cache:      ~5-10 MB (for 10 MB JSON file, plus overhead)
Margin:                 ~30-40 MB (browser internals, signalR, buffer)
Total:                  ~85-100 MB
```

**Monitoring**:
- Task Manager: Verify w3wp.exe (IIS) or dotnet.exe process
- PerfView or Windows Performance Toolkit (optional, Phase 2)

---

#### JSON Parsing Performance

**Target**: <100 ms for files up to 10 MB

**Measurements**:
- System.Text.Json: ~50-80 ms for typical 1 MB JSON
- Scaling: Linear growth with file size
- If >100 ms: Consider lazy-loading (Phase 2)

---

### Monitoring & Observability

#### MVP (No Formal Monitoring)

```
Manual Checks:  User periodically tests dashboard loads correctly
Uptime:         Not monitored (internal development tool)
Alerting:       Not configured
Metrics:        None collected
APM:            Not used
SLA:            None defined (internal tool, best-effort)
```

#### Phase 2 Enhancements (Optional)

```
Application Insights:
  - Dependency tracking (file I/O, deserialization)
  - Request latency (dashboard load time)
  - Custom events (data load start/end, cache hits)
  - Exception tracking (JSON parsing errors)

Logging:
  - Persistent log storage (Azure Log Analytics or on-premises ELK)
  - Structured logging (Serilog optional)
  - Log aggregation and search

Monitoring:
  - Server resource monitoring (CPU, RAM, disk)
  - Availability monitoring (synthetic transaction every 5 minutes)
  - Performance baselines and alerting
```

---

### CI/CD Pipeline

#### Development (MVP)

```
No CI/CD required for MVP
Build:        Manual: dotnet build
Test:         Manual: dotnet test (optional, 2-3 unit tests)
Publish:      Manual: dotnet publish -c Release
Deploy:       Manual copy of publish folder to target machine
```

#### Phase 2 (Optional GitHub Actions)

**Trigger**: Push to main branch

**Pipeline Steps**:
1. Checkout code
2. Setup .NET 8 SDK
3. Restore dependencies: `dotnet restore`
4. Build: `dotnet build -c Release`
5. Test: `dotnet test` (xUnit tests)
6. Publish: `dotnet publish -c Release -o ./publish`
7. Create artifact: ZIP of publish folder
8. Upload to GitHub Artifacts for download

**GitHub Actions Workflow** (future):
```yaml
name: Build & Publish

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'
      
      - run: dotnet restore
      
      - run: dotnet build -c Release --no-restore
      
      - run: dotnet test -c Release --no-build --no-restore
      
      - run: dotnet publish -c Release -o ./publish
      
      - uses: actions/upload-artifact@v3
        with:
          name: dashboard-release
          path: ./publish/
          retention-days: 30

  deploy:  # Optional: automated deployment to test server
    needs: build
    runs-on: windows-latest
    if: success()
    
    steps:
      - uses: actions/download-artifact@v3
        with:
          name: dashboard-release
          path: C:\dashboards\agentsquad
      
      - run: Restart-WebAppPool -Name "AgentSquad.Dashboard"
        shell: powershell
```

---

### Infrastructure as Code (Phase 2)

#### IIS Deployment Script

```powershell
# provision-iis.ps1
param(
    [string]$SiteName = "AgentSquad.Dashboard",
    [string]$PhysicalPath = "C:\dashboards\agentsquad",
    [int]$Port = 8080,
    [string]$AppPoolIdentity = "ApplicationPoolIdentity"
)

# Check IIS installed
if (-not (Get-WindowsFeature IIS-WebServer).Installed) {
    Write-Host "Installing IIS..."
    Install-WindowsFeature -Name IIS-WebServer, IIS-WebServerRole -IncludeManagementTools
}

# Create app pool
Write-Host "Creating app pool '$SiteName'..."
New-WebAppPool -Name $SiteName -Force
Set-ItemProperty -Path "IIS:\AppPools\$SiteName" -Name "managedRuntimeVersion" -Value ""

# Create site
Write-Host "Creating IIS site '$SiteName' at $PhysicalPath..."
New-Website -Name $SiteName `
    -PhysicalPath $PhysicalPath `
    -HostHeader "" `
    -Port $Port `
    -AppPool $SiteName `
    -Force

# Set folder permissions
Write-Host "Setting folder permissions..."
$acl = Get-Acl $PhysicalPath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "IIS_IUSRS",
    "ReadAndExecute",
    "ContainerInherit,ObjectInherit",
    "None",
    "Allow"
)
$acl.AddAccessRule($rule)
Set-Acl -Path $PhysicalPath -AclObject $acl

Write-Host "✓ IIS Site '$SiteName' created successfully"
Write-Host "✓ Access site at http://localhost:$Port"
```

**Usage**:
```powershell
. .\provision-iis.ps1 -SiteName "AgentSquad.Dashboard" -PhysicalPath "C:\dashboards\agentsquad" -Port 8080
```

---

### Configuration & Environment Variables

#### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "DataFilePath": "wwwroot/data/data.json",
  "CacheExpirationSeconds": 300,
  "MaxJsonFileSizeMB": 10
}
```

#### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug"
    }
  },
  "DetailedErrors": true
}
```

#### Environment Variables (Optional, Phase 2)

```
ASPNETCORE_ENVIRONMENT    = Production | Development
ASPNETCORE_URLS           = http://0.0.0.0:5000
DATA_FILE_PATH            = /custom/path/to/data.json
CACHE_EXPIRATION_SECONDS  = 300
```

**Usage**:
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://0.0.0.0:8080"
dotnet run
```

---

## Technology Stack Decisions

### Recommended Stack (Mandatory per Requirements)

| Layer | Technology | Version | Purpose | Decision |
|-------|-----------|---------|---------|----------|
| Runtime | .NET | 8.0.x | Blazor Server host, async/await | Mandatory |
| UI Framework | Blazor Server | 8.0.x | Component model, server-side rendering | Mandatory |
| Component Library | MudBlazor | 6.10.0+ | Material Design, responsive grid, light/dark theme | Chosen (vs. Radzen, Ant Design) |
| JSON | System.Text.Json | 8.0.x (built-in) | Data deserialization, 40% faster than alternatives | Chosen (vs. Newtonsoft.Json) |
| Charting | CChart.js | 1.1.0+ | Horizontal bar chart (timeline), Blazor-native wrapper | Chosen (vs. ApexCharts, custom HTML) |
| Testing | xUnit | 2.7.0 | Unit tests for DataService | Chosen for MVP (optional) |
| Mocking | Moq | 4.20.0 | Dependency injection mocking | Chosen for tests |
| Component Testing | bUnit | 1.27.1 | Blazor component testing | Chosen for MVP (optional) |
| IDE | Visual Studio | 2022 Community+ | Development environment | Standard |
| Build System | MSBuild | (built-in) | Project compilation | Standard |
| Package Manager | NuGet | (built-in) | Dependency management | Standard |

---

### Rejected Alternatives

| Technology | Reason Rejected |
|------------|-----------------|
| Newtonsoft.Json (JSON.NET) | Legacy (deprecated by Microsoft), 40% slower, 500 KB dependency, unnecessary features (BSON, XML) |
| Blazor WASM | Adds complexity (separate build pipeline, no server-side rendering), larger payload, unsuitable for local-only app |
| ApexCharts.Blazor | 200 KB footprint (vs. 40 KB Chart.js), steeper learning curve, overkill for simple dashboard |
| Custom HTML/CSS Timeline | 6+ hours implementation, manual styling, accessibility debt, testing burden |
| Bootstrap 5 | Unnecessary 150 KB CSS (MudBlazor already included), adds framework overhead |
| Tailwind CSS | Requires build pipeline, utility-first model conflicts with MudBlazor Material Design |
| Fluxor (Blazor Redux) | Complex state management not needed for read-only dashboard, 20+ hours overhead |
| Docker | Setup overhead, Windows Docker not lightweight, no multi-machine deployment required |
| Azure Cloud Services | Explicitly rejected per requirements (local-only, zero cloud services), unnecessary cost ($10-50/mo) |
| SQL Server / PostgreSQL | No database needed, JSON file sufficient for aggregated data, eliminates infrastructure |
| Entity Framework Core | No relational mapping required, adds ORM overhead not justified for static data |

---

### Dependency Versions (Pinned)

**NuGet Packages** (in .csproj):
```xml
<ItemGroup>
  <PackageReference Include="MudBlazor" Version="6.10.0" />
  <!-- System.Text.Json is built-in to .NET 8 -->
  <!-- CChart.js integrated via NuGet; exact version TBD -->
</ItemGroup>

<ItemGroup Condition="'$(Configuration)' == 'Debug'">
  <PackageReference Include="xunit" Version="2.7.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
  <PackageReference Include="Moq" Version="4.20.0" />
  <PackageReference Include="bunit" Version="1.27.1" />
</ItemGroup>
```

**No Floating Versions**: All versions pinned explicitly to avoid breaking changes on `dotnet restore`

**LTS Support Timeline**:
- .NET 8: Support until November 2026 (3-year LTS)
- MudBlazor 6.10.0: Active maintenance, >2K GitHub stars
- All dependencies: MIT/Apache 2.0 licensing, no commercial restrictions

---

## Security Considerations

### Authentication & Authorization

**Status**: Not Required for MVP

**Rationale**:
- Internal-only application (no external users)
- No sensitive data in fictional project data
- Local network deployment only
- OS file system ACLs sufficient for access control
- Zero user login complexity

**Phase 2 Decision Point** (if expanding to multi-user shared server):
```csharp
// Add ASP.NET Core Identity
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
builder.Services.AddAuthorization();

// Add login page and user management
// Integrate with Active Directory (optional)
```

**File-Level Access Control** (implement now):
```
data.json NTFS Permissions:
  Owner:               Administrator or designated data curator
  Read:                Application service account + authorized team members
  Modify/Write:        Data curator only
  Delete:              Administrator only
  Full Control:        Administrator only
```

---

### Data Protection

**Encryption**: Not Required

**Rationale**:
- No sensitive data (fictional project metrics only)
- Local-only deployment (no network transmission)
- No PII or confidential information stored
- Windows file system ACLs provide adequate OS-level protection
- No compliance requirements (internal tool)

**Future Enhancement** (Phase 2+, if storing production data):
```csharp
// Implement AES-256-GCM encryption
using System.Security.Cryptography;

public class DataEncryption
{
    public static byte[] Encrypt(string plaintext, byte[] key)
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.Mode = CipherMode.GCM;
            
            var encryptor = aes.CreateEncryptor(key, aes.IV);
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(Encoding.UTF8.GetBytes(plaintext));
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }
    }
}
```

---

### Input Validation

**JSON Schema Validation** (implement MVP):
```csharp
// DataService.cs
public async Task<ProjectData> LoadProjectDataAsync()
{
    // ... load and deserialize JSON ...
    
    // Validate schema before returning
    ValidateProjectData(projectData);
    return projectData;
}

private void ValidateProjectData(ProjectData data)
{
    // Required fields
    if (string.IsNullOrWhiteSpace(data.ProjectName))
        throw new InvalidOperationException("ProjectName is required and non-empty");
    
    // Collection constraints
    if (data.Milestones == null || data.Milestones.Count < 4 || data.Milestones.Count > 10)
        throw new InvalidOperationException("Milestones must contain 4-10 items");
    
    // Numeric constraints
    int totalWork = data.WorkItems.Shipped + data.WorkItems.InProgress + 
                    data.WorkItems.CarriedOver + data.WorkItems.AtRisk;
    if (totalWork > data.WorkItems.TotalCapacity)
        throw new InvalidOperationException("Total work items exceed capacity");
    
    if (data.WorkItems.Shipped < 0 || data.WorkItems.InProgress < 0 ||
        data.WorkItems.CarriedOver < 0 || data.WorkItems.AtRisk < 0)
        throw new InvalidOperationException("Work item counts cannot be negative");
    
    // Enum validation
    var validStatuses = Enum.GetValues(typeof(MilestoneStatus));
    foreach (var milestone in data.Milestones)
    {
        if (!Enum.IsDefined(typeof(MilestoneStatus), milestone.Status))
            throw new InvalidOperationException($"Invalid milestone status: {milestone.Status}");
    }
    
    // Date range validation
    if (data.StartDate > data.EndDate)
        throw new InvalidOperationException("StartDate must be <= EndDate");
}
```

**Field Constraints** (enforce via attributes):
```csharp
public class Milestone
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; }
    
    [Required]
    public DateTime TargetDate { get; set; }
    
    [MaxLength(5)]
    public List<string> Owners { get; set; } = new();
}

public class WorkItemAggregation
{
    [Range(0, int.MaxValue)]
    public int Shipped { get; set; }
    
    [Range(0, int.MaxValue)]
    public int InProgress { get; set; }
}
```

---

### Injection Prevention

**SQL Injection**: Not Applicable
- No SQL database, no SQL queries, no injection risk

**Script Injection (XSS)**: Minimal Risk
- Razorcomponents escape HTML by default
- All data read from local JSON file (no user input via web forms)
- MudBlazor sanitizes output

**Deserialization Attacks**: Mitigated
- Use strongly-typed models (ProjectData, Milestone, WorkItemAggregation)
- System.Text.Json is safer than alternatives (no code execution)
- Validate schema immediately after deserialization

**Command Injection**: Not Applicable
- No shell execution, no Process.Start calls
- File I/O only via managed System.IO APIs

**No Authentication Bypass Risk**: Not Applicable
- No authentication layer in MVP

---

### Transport Security

**Development**:
- HTTP (localhost:5000) and HTTPS (localhost:5001) both supported
- Development certificate auto-generated by .NET SDK

**Production (Internal Network)**:
- HTTP only (no HTTPS required for internal network)
- No external internet-facing endpoints
- Firewall blocks external access to port 8080

**Future (Phase 2, if internet-exposed)**:
```
Add HTTPS via:
  - Self-signed certificate (development)
  - Let's Encrypt certificate (production via certbot)
  - Corporate certificate (enterprise)
```

---

## Scaling Strategy

### Horizontal Scaling: Not Applicable to MVP

**Decision**: Scale vertically first; horizontal scaling deferred to Phase 2

**Why Not Horizontal**:
- Blazor Server maintains per-circuit state (difficult to distribute across machines)
- Dashboard is read-only (no transaction contention or complex state coordination)
- MVP scope: Single instance adequate for <50 simultaneous users
- Would require: Session state store (Redis/AppFabric), distributed cache, load balancer

**Phase 2 Option (if expanding to multiple teams)**:

```
Option A: Independent Deployments (Recommended)
  - Each team/project gets its own dashboard instance
  - Own data.json file per instance
  - No shared database
  - No distributed state coordination
  - Simplest to operate

Option B: Shared Server with Session Affinity
  - Deploy on Windows Server with IIS
  - Use sticky sessions (affinity) to keep user circuits on same machine
  - Add Redis or AppFabric cache layer
  - Implement database-backed session state
  - More complex but enables shared infrastructure
```

---

### Vertical Scaling: Recommended Path

**Increase Per-Machine Capacity**:
```
CPU:           Add cores (Blazor Server scales linearly with CPU count)
RAM:           Increase to 16+ GB
                - Baseline: ~50 MB (Blazor Server + MudBlazor)
                - Per circuit: ~10-20 MB (ProjectData cache + signalR state)
                - At 100 circuits: ~1-2 GB total
Disk I/O:      SSD for data.json (improves file read performance)
```

**Bottleneck Analysis**:
```
Rank | Bottleneck              | Impact | Mitigation
-----|-------------------------|--------|--------------------
1    | JSON file load (>10MB)  | High   | Lazy-load milestones (Phase 2)
2    | Chart.js rendering      | Low    | Defer animations, simplify DOM
3    | Component rerendering   | Low    | Use @key directive, reduce updates
4    | Blazor signalR circuit  | Low    | Connection pooling (built-in)
5    | Database                | None   | No database required
6    | Network latency         | None   | Local network, sub-millisecond
```

**Maximum Practical Capacity** (single Windows Server):
```
Simultaneous Users:     50-100 circuits (depending on RAM and CPU)
Data File Size:         Up to 100 MB (acceptable but slow, <2 sec budget)
JSON Parsing Time:      <100 ms for 10 MB file (System.Text.Json optimized)
Response Time:          <2 seconds dashboard load (measured end-to-end)
Memory per Circuit:     ~10-20 MB (ProjectData cache + signalR)
Total Sustainable Load: 50-100 users × 20 MB = 1-2 GB RAM required
```

---

### Caching Strategy

**Current** (MVP):
```csharp
// DataService.cs (5-minute cache TTL)
private ProjectData _cachedProjectData;
private DateTime _lastLoadTime;

public async Task<ProjectData> LoadProjectDataAsync()
{
    // Return cached if < 300 seconds old
    if (_cachedProjectData != null && 
        (DateTime.UtcNow - _lastLoadTime).TotalSeconds < 300)
    {
        return _cachedProjectData;
    }
    
    // Otherwise reload from disk
    var json = await File.ReadAllTextAsync(DataFilePath);
    _cachedProjectData = JsonSerializer.Deserialize<ProjectData>(json);
    _lastLoadTime = DateTime.UtcNow;
    return _cachedProjectData;
}

public void ClearCache()
{
    _cachedProjectData = null;
    _lastLoadTime = DateTime.MinValue;
}
```

**Phase 2 Enhancement** (if data updates frequently):
```csharp
// In-memory caching with file watcher
public class DataServiceWithWatcher : IDataService
{
    private readonly IMemoryCache _cache;
    private readonly FileSystemWatcher _watcher;
    
    public DataServiceWithWatcher()
    {
        _cache = new MemoryCache(new MemoryCacheOptions 
        { 
            SizeLimit = 50_000_000 // 50 MB max cache
        });
        
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(DataFilePath))
        {
            Filter = "data.json",
            NotifyFilter = NotifyFilters.LastWrite
        };
        
        _watcher.Changed += (s, e) => 
        {
            _cache.Remove("projectData");
            Logger.LogInformation("Cache invalidated due to file change");
        };
        
        _watcher.EnableRaisingEvents = true;
    }
    
    public async Task<ProjectData> LoadProjectDataAsync()
    {
        if (_cache.TryGetValue("projectData", out ProjectData cached))
            return cached;
        
        // Load from disk
        var json = await File.ReadAllTextAsync(DataFilePath);
        var data = JsonSerializer.Deserialize<ProjectData>(json);
        
        // Cache with 10 MB size limit
        _cache.Set("projectData", data, new MemoryCacheEntryOptions 
        { 
            Size = json.Length,
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        
        return data;
    }
}
```

**Cache Invalidation Strategies**:
- **Time-based**: 5-minute TTL (MVP)
- **Event-based**: File system watcher (Phase 2)
- **Manual**: ClearCache() method (Phase 2 refresh button)
- **Distributed**: Not needed (single machine MVP)

---

### Load Balancing

**Not Applicable for MVP**:
- Single machine, single instance
- No need for load balancer

**Phase 2** (if scaling to multiple servers):
```
Windows Network Load Balancing (NLB)
  - Prerequisites:
    • Identical data.json on all servers (via shared network path \\fileserver\dashboards\data.json)
    • Sticky sessions (circuit affinity) to keep user requests on same server
    • Time sync across servers (NTP)
  
  - Configuration:
    • NLB cluster name: agentsquad-dashboard
    • Port: 8080 (HTTP)
    • Affinity: Single (keep requests from single IP to same host)
    • Heartbeat: 5-second intervals
  
  - Monitoring:
    • Health check endpoint: GET /health (returns 200 if data.json accessible)
    • Remove unhealthy servers from rotation automatically
```

---

## Risks & Mitigations

### Critical Risks

| Risk | Probability | Impact | Mitigation | Timeline | Owner |
|------|-------------|--------|-----------|----------|-------|
| **JSON schema breaks parsing** | High | Medium | Version schema field in data.json + unit tests (2-3 tests DataService) | MVP | Dev |
| **data.json file missing** | Medium | High | Graceful error message: "Data file not found at wwwroot/data/data.json" | MVP | Dev |
| **MudBlazor breaks on .NET 9** | Low | High | Monitor MudBlazor GitHub releases monthly; maintain test baseline | Continuous | DevOps |
| **Large JSON files (>50 MB) slow startup** | Low | Medium | Implement lazy-loading or pagination in Phase 2 if needed | Phase 2 | Dev |
| **Component rerendering perf degrades** | Low | Low | Benchmark using Chrome DevTools; use @key directive sparingly | Continuous | Dev |

---

### Technical Risk Mitigations

#### 1. JSON Schema Versioning

**Implement**: Version field in data.json + compatibility check

```json
{
  "version": "1.0",
  "schemaVersion": "1.0",
  "projectName": "Q2 Launch"
}
```

```csharp
// Validate version compatibility
if (!data.Version.StartsWith("1."))
    throw new InvalidOperationException(
        $"Incompatible schema version. Expected 1.x, got {data.Version}");
```

**Benefit**: Allow future schema changes (e.g., 2.0) without breaking existing dashboards

---

#### 2. Error Handling & Recovery

**Implement**: Graceful degradation with actionable user messages

```csharp
try
{
    projectData = await dataService.LoadProjectDataAsync();
}
catch (FileNotFoundException ex)
{
    errorMessage = "Data file not found. Create 'wwwroot/data/data.json' with sample project data.";
    isLoading = false;
    logger.LogError(ex, "data.json missing at {path}", DataFilePath);
}
catch (JsonException ex)
{
    errorMessage = "JSON syntax error. Check data.json for: missing commas, unclosed quotes, invalid enums.";
    isLoading = false;
    logger.LogError(ex, "JSON parsing failed");
}
catch (InvalidOperationException ex)
{
    errorMessage = $"Data validation failed: {ex.Message}";
    isLoading = false;
    logger.LogError(ex, "Schema validation failed");
}
```

**Display to User**:
```
[Error Alert]
Data file is malformed JSON. Please check syntax at wwwroot/data/data.json.
Common issues: missing commas, unclosed quotes, trailing commas.

[Refresh Button]
```

---

#### 3. Dependency Monitoring

**Process**:
- **NuGet**: Check for updates weekly (`dotnet outdated` or NuGet Package Manager)
- **MudBlazor**: Monitor GitHub releases monthly (subscribe to notifications)
- **.NET 8**: Receive official LTS support until November 2026 (check Microsoft release notes)
- **CChart.js**: Monitor npm registry for breaking changes

**Testing Before Upgrade**:
```bash
# Create feature branch
git checkout -b upgrade/mudb lazor-6.11.0

# Update package
dotnet add package MudBlazor --version 6.11.0

# Run tests
dotnet test

# Manual testing
dotnet run

# If successful, merge to main
git merge --ff-only upgrade/mudblazor-6.11.0
```

---

#### 4. Performance Benchmarking

**Measure Load Time**:
```csharp
// DataService.cs
public async Task<ProjectData> LoadProjectDataAsync()
{
    var sw = Stopwatch.StartNew();
    
    try
    {
        var json = await File.ReadAllTextAsync(DataFilePath);
        logger.LogInformation("File read: {ms}ms", sw.ElapsedMilliseconds);
        
        sw.Restart();
        var data = JsonSerializer.Deserialize<ProjectData>(json);
        logger.LogInformation("Deserialization: {ms}ms", sw.ElapsedMilliseconds);
        
        if (sw.ElapsedMilliseconds > 100)
            logger.LogWarning("Slow deserialization: {ms}ms", sw.ElapsedMilliseconds);
        
        return data;
    }
    finally
    {
        logger.LogInformation("Total load time: {ms}ms", sw.ElapsedMilliseconds);
    }
}
```

**Monitor Dashboard Load**:
```csharp
// Dashboard.razor
protected override async Task OnInitializedAsync()
{
    var sw = Stopwatch.StartNew();
    
    try
    {
        projectData = await dataService.LoadProjectDataAsync();
        logger.LogInformation("Dashboard loaded: {ms}ms", sw.ElapsedMilliseconds);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Dashboard load failed");
    }
}
```

**Alert Thresholds**:
- JSON parsing >100 ms: Warning
- Dashboard load >2 seconds: Error (investigate)
- Component render >1 second: Optimization candidate

---

### Dependency Risks

**NuGet Package Risk Assessment**:

| Package | Version | Status | Risk Level | Notes |
|---------|---------|--------|-----------|-------|
| MudBlazor | 6.10.0 | Actively maintained | Low | 2K+ GitHub stars, stable release, compatible with .NET 8 |
| System.Text.Json | 8.0.x | Built-in | None | Microsoft-supported, no external dependency risk |
| CChart.js | 1.1.0 | Stable | Low | Chart.js (upstream) widely used in production, stable C# wrapper |

**Mitigation Actions**:
1. **Pin versions** in .csproj (no floating versions like `6.10.*`)
2. **Monitor releases** monthly (GitHub notifications)
3. **Test upgrades** before merging (feature branch → run tests → manual testing → merge)
4. **Use LTS releases** (.NET 8 support until Nov 2026)
5. **Document breaking changes** if upgrading (update CHANGELOG.md)

**Future Risk** (Phase 2+):
```
If multi-team deployment needed:
  - Consider Blazor Server limitations on horizontal scaling
  - Evaluate Blazor WASM as alternative (if no server-side logic needed)
  - Plan for session state store (Redis, AppFabric, or SQL Server)
  - Monitor .NET 9 compatibility (May 2026 release) and upgrade timeline
```

---

### Organizational Risks

| Risk | Mitigation | Owner | Timeline |
|------|-----------|-------|----------|
| **Data curator unavailable** | Document data.json schema; train backup person; store in source control | PM | Week 1 |
| **Knowledge silos** | Code review on all changes; README documentation; onboarding guide | Tech Lead | MVP + 2 weeks |
| **Budget constraints for Phase 2** | Use free/open-source tools: GitHub Actions (CI/CD), Windows Server IIS (hosting) | PM | Phase 2 planning |
| **Stakeholder expectation creep** | Document Phase 1 vs Phase 2 boundary; maintain feature backlog; review scope monthly | PM | Ongoing |
| **Production data leaks** | Use fictional data only; enforce file ACLs; no sensitive PII in data.json | Security | MVP |

---

### Summary: Risk Priority

**High Priority (MVP)**:
1. JSON schema validation + error messages
2. File system access validation (ACLs on data.json)
3. Unit tests (2-3 for DataService JSON parsing)
4. Performance benchmarking (measure <2 sec load time)

**Medium Priority (Phase 1+)**:
1. Dependency monitoring process (monthly check for updates)
2. Component performance profiling (Chrome DevTools, measure paint times)
3. Backup strategy documentation (data.json version control in Git)
4. Training for data curator (how to edit data.json schema)

**Low Priority (Phase 2+)**:
1. Lazy-loading for large files (>50 MB)
2. Multi-server session state (Redis, AppFabric)
3. Horizontal scaling architecture (Windows NLB)
4. Advanced caching strategies (distributed cache, invalidation events)

---

## Appendix: MVP Delivery Timeline

### Day 1: Project Setup & Component Structure
- Create Blazor Server project: `dotnet new blazorserver -n AgentSquad.Dashboard`
- Add MudBlazor: `dotnet add package MudBlazor`
- Create folder structure (Pages, Components, Services, Models)
- Stub out component hierarchy (Dashboard → MilestoneTimeline, StatusSummary)
- **Deliverable**: Project compiles without errors

### Day 2: Models & DataService
- Create ProjectData, Milestone, WorkItemAggregation models
- Implement DataService with JSON loading and caching
- Create sample data.json in wwwroot/data/
- Implement error handling and validation
- **Deliverable**: DataService loads data.json successfully; unit tests passing

### Day 3: Component Implementation
- Implement Dashboard.razor orchestration and cascading parameters
- Implement MilestoneTimeline with ChartJs
- Implement StatusSummary and 4× StatusCard components
- Wire up all component interactions
- **Deliverable**: All components render with sample data

### Day 4: Styling & Refinement
- Apply MudBlazor light theme
- Custom CSS for font sizes (18pt+ for projector visibility)
- Responsive grid layout (1920×1080 optimization)
- Manual visual regression testing (screenshot baseline)
- **Deliverable**: Dashboard screenshot ready for PowerPoint

### Day 5: Testing & Documentation
- Write 2-3 unit tests for DataService
- Document data.json schema and editing instructions
- Create README with deployment steps
- Final stakeholder review and sign-off
- **Deliverable**: MVP ready for production deployment

---

## Conclusion

This architecture provides a simple, maintainable, and scalable foundation for an executive dashboard that eliminates manual status report creation. By leveraging .NET 8, Blazor Server, and pre-aggregated JSON data, the system achieves:

✓ Zero infrastructure overhead  
✓ Single-file deployment  
✓ Sub-2-second load time  
✓ Screenshot-ready output  
✓ Graceful error handling  
✓ Reusable component library for future projects  

The design prioritizes MVP simplicity with clear paths for Phase 2 enhancements (data refresh button, multi-project support, horizontal scaling) without requiring architectural rework.