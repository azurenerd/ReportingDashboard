# Architecture: Executive Project Reporting Dashboard

## Overview & Goals

The Executive Project Reporting Dashboard is a lightweight, single-page Blazor Server application designed to provide real-time visibility into project milestones, progress, and work item status. The system prioritizes simplicity, visual clarity, and ease of use—enabling project managers to generate professional, screenshot-ready reports for executive presentations without manual design work.

**Primary Goals:**
1. Display project status and key metrics in under 3 seconds on page load
2. Provide a clean, printable visualization optimized for PowerPoint screenshots
3. Read all data from a local JSON configuration file with zero external dependencies
4. Deliver a maintainable, testable codebase that can be extended with future integrations (JIRA, Azure DevOps)
5. Support up to 500 work items and 10-20 milestones without perceptible performance degradation

**Success Criteria:**
- All 8 user stories pass acceptance testing in Chrome, Firefox, and Edge
- Page load time ≤ 3 seconds
- Print/screenshot layout renders correctly without content overflow
- JSON deserialization completes in <500ms
- DataService and CalculationService achieve >80% unit test coverage
- Application gracefully handles missing or malformed `data.json`

---

## System Components

### 1. **Dashboard.razor** (Main Page Component)
**Responsibility:** Entry point for the application; orchestrates all child components and manages application-level state.

**Key Features:**
- Loads project data on page initialization via `DataService.LoadProjectDataAsync()`
- Computes KPIs and aggregations via `CalculationService`
- Renders child components: `Header`, `Timeline`, `MetricsPanel`, `StatusBoard`, `Footer`
- Handles data load errors and displays graceful fallback UI

**Data Dependencies:**
- Receives `ProjectData` from `DataService`
- Passes derived data to child components via `@ChildContent` parameters

**Interfaces:**
```csharp
public partial class Dashboard : ComponentBase
{
    [Inject] public IDataService DataService { get; set; }
    [Inject] public ICalculationService CalculationService { get; set; }
    
    private ProjectData projectData;
    private ProjectCalculations calculations;
    private string errorMessage;
    private bool isLoading = true;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            projectData = await DataService.LoadProjectDataAsync();
            calculations = CalculationService.CalculateMetrics(projectData);
        }
        catch (Exception ex)
        {
            errorMessage = $"Unable to load project data: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

**Dependencies:**
- `IDataService` (dependency injection)
- `ICalculationService` (dependency injection)

---

### 2. **Header Component** (Header.razor)
**Responsibility:** Display project metadata (name, status, owner, start/end dates) at the top of the dashboard.

**Key Features:**
- Displays project name as prominent heading
- Shows project status with color-coded visual indicator ("On Track" = green, "At Risk" = amber, "Off Track" = red)
- Renders project owner name
- Formats and displays start and target end dates (e.g., "Jan 1, 2026 - Jun 30, 2026")
- Responsive design optimized for print

**Data Model:**
```csharp
public partial class Header : ComponentBase
{
    [Parameter]
    public ProjectMetadata Project { get; set; }
}
```

**Dependencies:**
- `ProjectMetadata` (passed from parent)

**CSS Classes:**
- `.header` - Container
- `.header__title` - Project name
- `.header__status` - Status badge with color coding
- `.header__owner` - Owner name
- `.header__dates` - Date range

---

### 3. **Timeline Component** (Timeline.razor)
**Responsibility:** Render milestones in a horizontal timeline layout with completion status indicators.

**Key Features:**
- Displays milestones ordered chronologically (by `order` and `dueDate`)
- Shows milestone title and due date
- Visual distinction between completed and upcoming milestones (checkmark icon vs. circle)
- Horizontally scrollable on narrow screens; fits single page for typical milestone counts (5-10)
- Print-friendly styling

**Data Model:**
```csharp
public partial class Timeline : ComponentBase
{
    [Parameter]
    public List<Milestone> Milestones { get; set; }
    
    private List<Milestone> SortedMilestones => 
        Milestones?.OrderBy(m => m.Order).ThenBy(m => m.DueDate).ToList() ?? new List<Milestone>();
}
```

**Child Component:** `MilestoneIndicator`
- Renders individual milestone card
- Parameters: `Milestone`, `IsCompleted`

**Dependencies:**
- `List<Milestone>` (passed from parent)

**CSS Classes:**
- `.timeline` - Container
- `.timeline__item` - Individual milestone
- `.timeline__item--completed` - Modifier for completed milestone
- `.timeline__item--upcoming` - Modifier for upcoming milestone
- `.timeline__icon` - Checkmark or circle icon

---

### 4. **MilestoneIndicator Component** (MilestoneIndicator.razor)
**Responsibility:** Render a single milestone card with visual completion status.

**Key Features:**
- Displays milestone title
- Shows due date formatted as "MMM D, YYYY"
- Shows completion checkmark if completed, circle outline if upcoming
- Color-coded background (green for completed, light gray for upcoming)

**Data Model:**
```csharp
public partial class MilestoneIndicator : ComponentBase
{
    [Parameter]
    public Milestone Milestone { get; set; }
}
```

**Dependencies:**
- `Milestone` (passed from parent)

---

### 5. **MetricsPanel Component** (MetricsPanel.razor)
**Responsibility:** Display key performance indicators (KPIs) in a grid of cards.

**Key Features:**
- Displays 6 KPIs as individual cards: % Complete, Total Story Points, Items Shipped, Items In Progress, Items Carried Over, Velocity Per Sprint
- Each card shows label and numeric value
- Large, readable font sizes suitable for executive presentations
- Grid layout (2-3 columns depending on screen width)
- Print-friendly spacing and colors

**Data Model:**
```csharp
public partial class MetricsPanel : ComponentBase
{
    [Parameter]
    public ProjectCalculations Calculations { get; set; }
}

public class ProjectCalculations
{
    public int PercentageComplete { get; set; }
    public int TotalStoryPoints { get; set; }
    public int ShippedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CarriedOverCount { get; set; }
    public double VelocityPerSprint { get; set; }
}
```

**Child Component:** `MetricCard`
- Parameters: `Label`, `Value`, `Unit` (e.g., "%", "pts")

**Dependencies:**
- `ProjectCalculations` (passed from parent)

**CSS Classes:**
- `.metrics-panel` - Container
- `.metrics-panel__grid` - Grid layout
- `.metric-card` - Individual card
- `.metric-card__label` - Label text
- `.metric-card__value` - Large numeric value

---

### 6. **StatusBoard Component** (StatusBoard.razor)
**Responsibility:** Organize work items into three columns: Shipped, In Progress, Carried Over.

**Key Features:**
- Three-column layout (CSS Grid: `grid-template-columns: repeat(3, 1fr)`)
- Each column displays work items as cards
- Column headers show title and count (e.g., "Shipped (7)")
- Vertically scrollable within each column if many items
- Responsive: stacks to single column on narrow screens (via media query)

**Data Model:**
```csharp
public partial class StatusBoard : ComponentBase
{
    [Parameter]
    public List<WorkItem> WorkItems { get; set; }
    
    private List<WorkItem> ShippedItems => GetItemsByCategory("Shipped");
    private List<WorkItem> InProgressItems => GetItemsByCategory("InProgress");
    private List<WorkItem> CarriedOverItems => GetItemsByCategory("CarriedOver");
    
    private List<WorkItem> GetItemsByCategory(string category) =>
        WorkItems?.Where(w => w.Category == category).ToList() ?? new List<WorkItem>();
}
```

**Child Component:** `StatusColumn`
- Parameters: `ColumnTitle`, `Items`, `ColumnType`

**Dependencies:**
- `List<WorkItem>` (passed from parent)

**CSS Classes:**
- `.status-board` - Container
- `.status-column` - Individual column
- `.status-column__header` - Column title and count
- `.status-column__body` - Scrollable item list

---

### 7. **StatusColumn Component** (StatusColumn.razor)
**Responsibility:** Render a single work item column with header and item list.

**Key Features:**
- Displays column title (e.g., "Shipped")
- Shows count of items in parentheses
- Lists work items as cards (via `StatusCard` child component)
- Vertical scroll if items exceed visible height

**Data Model:**
```csharp
public partial class StatusColumn : ComponentBase
{
    [Parameter]
    public string ColumnTitle { get; set; }
    
    [Parameter]
    public List<WorkItem> Items { get; set; }
}
```

**Child Component:** `StatusCard`

**Dependencies:**
- `String` (column title)
- `List<WorkItem>` (items to display)

---

### 8. **StatusCard Component** (StatusCard.razor)
**Responsibility:** Render a single work item as a compact card with title and story points.

**Key Features:**
- Displays work item title (truncated to prevent excessive wrapping)
- Shows story points as a number with "pts" label (e.g., "8 pts")
- Clean, minimal styling suitable for dense layouts
- Subtle background color to distinguish from surrounding elements

**Data Model:**
```csharp
public partial class StatusCard : ComponentBase
{
    [Parameter]
    public WorkItem WorkItem { get; set; }
}
```

**Dependencies:**
- `WorkItem` (passed from parent)

**CSS Classes:**
- `.status-card` - Container
- `.status-card__title` - Work item title
- `.status-card__points` - Story points badge

---

### 9. **Footer Component** (Footer.razor)
**Responsibility:** Display data freshness information.

**Key Features:**
- Shows "Last Updated" timestamp (current date/time)
- Human-readable format (e.g., "Last updated: April 12, 2026 at 01:19 UTC")
- Hidden in print layout via `@media print`

**Data Model:**
```csharp
public partial class Footer : ComponentBase
{
    private DateTime CurrentDateTime => DateTime.UtcNow;
    
    private string FormattedDateTime =>
        CurrentDateTime.ToString("MMMM dd, yyyy \\a\\t hh:mm UTC");
}
```

**CSS Classes:**
- `.footer` - Container
- `.footer__text` - Text content
- `@media print { .footer { display: none; } }`

---

### 10. **IDataService** (Services/DataService.cs)
**Responsibility:** Load and deserialize JSON data from `wwwroot/data/data.json`.

**Key Features:**
- Async method `LoadProjectDataAsync()` returns `ProjectData`
- Uses `System.Text.Json.JsonSerializer` (no external dependencies)
- Validates JSON structure and provides meaningful error messages
- Handles missing file gracefully
- Caches data after first load (optional optimization)

**Interface Definition:**
```csharp
public interface IDataService
{
    Task<ProjectData> LoadProjectDataAsync();
}

public class DataService : IDataService
{
    private readonly IWebHostEnvironment _env;
    
    public DataService(IWebHostEnvironment env)
    {
        _env = env;
    }
    
    public async Task<ProjectData> LoadProjectDataAsync()
    {
        var dataPath = Path.Combine(_env.WebRootPath, "data", "data.json");
        
        if (!File.Exists(dataPath))
            throw new FileNotFoundException($"Data file not found: {dataPath}");
        
        using var stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read);
        var projectData = await JsonSerializer.DeserializeAsync<ProjectData>(stream);
        
        if (projectData == null)
            throw new InvalidOperationException("Failed to deserialize project data");
        
        return projectData;
    }
}
```

**Error Handling:**
- `FileNotFoundException` if `data.json` not found
- `JsonException` if JSON is malformed
- `InvalidOperationException` if deserialization fails

**Dependencies:**
- `System.Text.Json`
- `IWebHostEnvironment` (injected)

**Dependency Injection (Program.cs):**
```csharp
services.AddScoped<IDataService, DataService>();
```

---

### 11. **ICalculationService** (Services/CalculationService.cs)
**Responsibility:** Compute KPIs and aggregations from project data.

**Key Features:**
- Method `CalculateMetrics(ProjectData)` returns `ProjectCalculations`
- Computes: % complete, total/completed/in-progress story points, item counts, velocity
- Handles edge cases (zero total points, empty lists)
- Pure function; no side effects

**Interface Definition:**
```csharp
public interface ICalculationService
{
    ProjectCalculations CalculateMetrics(ProjectData data);
}

public class CalculationService : ICalculationService
{
    public ProjectCalculations CalculateMetrics(ProjectData data)
    {
        var shippedItems = data.WorkItems.Where(w => w.Category == "Shipped").ToList();
        var inProgressItems = data.WorkItems.Where(w => w.Category == "InProgress").ToList();
        var carriedOverItems = data.WorkItems.Where(w => w.Category == "CarriedOver").ToList();
        
        int totalStoryPoints = data.Metrics.TotalStoryPoints;
        int completedStoryPoints = data.Metrics.CompletedStoryPoints;
        int percentageComplete = totalStoryPoints > 0 
            ? (int)((completedStoryPoints / (double)totalStoryPoints) * 100) 
            : 0;
        
        return new ProjectCalculations
        {
            PercentageComplete = percentageComplete,
            TotalStoryPoints = totalStoryPoints,
            ShippedCount = shippedItems.Count,
            InProgressCount = inProgressItems.Count,
            CarriedOverCount = carriedOverItems.Count,
            VelocityPerSprint = data.Metrics.VelocityPerSprint
        };
    }
}
```

**Dependencies:**
- None (pure logic)

**Dependency Injection (Program.cs):**
```csharp
services.AddScoped<ICalculationService, CalculationService>();
```

---

### 12. **Data Models** (Models/)

**ProjectData.cs:**
```csharp
public class ProjectData
{
    [JsonPropertyName("project")]
    public ProjectMetadata Project { get; set; }
    
    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();
    
    [JsonPropertyName("workItems")]
    public List<WorkItem> WorkItems { get; set; } = new();
    
    [JsonPropertyName("metrics")]
    public ProjectMetrics Metrics { get; set; }
}

public class ProjectMetadata
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; } // "On Track", "At Risk", "Off Track"
    
    [JsonPropertyName("owner")]
    public string Owner { get; set; }
    
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }
    
    [JsonPropertyName("targetEndDate")]
    public DateTime TargetEndDate { get; set; }
}

public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("dueDate")]
    public DateTime DueDate { get; set; }
    
    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }
    
    [JsonPropertyName("order")]
    public int Order { get; set; }
}

public class WorkItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("category")]
    public string Category { get; set; } // "Shipped", "InProgress", "CarriedOver"
    
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("storyPoints")]
    public int StoryPoints { get; set; }
    
    [JsonPropertyName("completedDate")]
    public DateTime? CompletedDate { get; set; }
}

public class ProjectMetrics
{
    [JsonPropertyName("totalStoryPoints")]
    public int TotalStoryPoints { get; set; }
    
    [JsonPropertyName("completedStoryPoints")]
    public int CompletedStoryPoints { get; set; }
    
    [JsonPropertyName("inProgressStoryPoints")]
    public int InProgressStoryPoints { get; set; }
    
    [JsonPropertyName("carriedOverCount")]
    public int CarriedOverCount { get; set; }
    
    [JsonPropertyName("velocityPerSprint")]
    public double VelocityPerSprint { get; set; }
}

public class ProjectCalculations
{
    public int PercentageComplete { get; set; }
    public int TotalStoryPoints { get; set; }
    public int ShippedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CarriedOverCount { get; set; }
    public double VelocityPerSprint { get; set; }
}
```

---

## Component Interactions

### Data Flow (Diagram)

```
Page Load
  ↓
Dashboard.OnInitializedAsync()
  ├─ DataService.LoadProjectDataAsync()
  │   └─ Reads wwwroot/data/data.json
  │       └─ Returns ProjectData
  │
  ├─ CalculationService.CalculateMetrics(ProjectData)
  │   └─ Returns ProjectCalculations (KPIs)
  │
  └─ Renders child components:
      ├─ Header (ProjectMetadata)
      ├─ Timeline (Milestone[])
      │   └─ MilestoneIndicator[] (for each milestone)
      ├─ MetricsPanel (ProjectCalculations)
      │   └─ MetricCard[] (for each KPI)
      ├─ StatusBoard (WorkItem[])
      │   └─ StatusColumn[] (Shipped, InProgress, CarriedOver)
      │       └─ StatusCard[] (for each work item)
      └─ Footer
```

### Component Communication Patterns

1. **Parent-to-Child Data Binding (via `@Parameter`):**
   - `Dashboard` → `Header` (ProjectMetadata)
   - `Dashboard` → `Timeline` (Milestone[])
   - `Dashboard` → `MetricsPanel` (ProjectCalculations)
   - `Dashboard` → `StatusBoard` (WorkItem[])
   - `StatusBoard` → `StatusColumn` (WorkItem[], ColumnTitle)
   - `StatusColumn` → `StatusCard` (WorkItem)
   - `Timeline` → `MilestoneIndicator` (Milestone)
   - `MetricsPanel` → `MetricCard` (Label, Value, Unit)

2. **Service Injection (via Dependency Injection):**
   - `Dashboard` uses `@inject IDataService` and `@inject ICalculationService`
   - Services registered in `Program.cs` with `AddScoped()` lifetime

3. **Cascading Parameters (if needed for future features):**
   - Not currently used; can be added for theme or configuration if required

4. **No Event Communication:**
   - Dashboard is read-only; no events required
   - Future filtering/interactive features would use event callbacks

---

## Data Model

### Entity Relationships

```
ProjectData
├─ ProjectMetadata (1:1)
│   ├─ Name: string
│   ├─ Status: string ("On Track" | "At Risk" | "Off Track")
│   ├─ Owner: string
│   ├─ StartDate: DateTime
│   └─ TargetEndDate: DateTime
│
├─ Milestone[] (1:N)
│   ├─ Id: string (unique)
│   ├─ Title: string
│   ├─ DueDate: DateTime
│   ├─ IsCompleted: bool
│   └─ Order: int (for sorting)
│
├─ WorkItem[] (1:N)
│   ├─ Id: string (unique)
│   ├─ Title: string
│   ├─ Category: string ("Shipped" | "InProgress" | "CarriedOver")
│   ├─ Status: string (informational)
│   ├─ StoryPoints: int
│   └─ CompletedDate: DateTime? (nullable)
│
└─ ProjectMetrics (1:1)
    ├─ TotalStoryPoints: int
    ├─ CompletedStoryPoints: int
    ├─ InProgressStoryPoints: int
    ├─ CarriedOverCount: int
    └─ VelocityPerSprint: double
```

### Storage Format (data.json)

Location: `wwwroot/data/data.json`

**Schema:**
```json
{
  "project": {
    "name": "string",
    "status": "string (On Track | At Risk | Off Track)",
    "owner": "string",
    "startDate": "ISO 8601 date (YYYY-MM-DD)",
    "targetEndDate": "ISO 8601 date (YYYY-MM-DD)"
  },
  "milestones": [
    {
      "id": "string (unique)",
      "title": "string",
      "dueDate": "ISO 8601 date",
      "isCompleted": "boolean",
      "order": "integer (1, 2, 3...)"
    }
  ],
  "workItems": [
    {
      "id": "string (unique)",
      "title": "string",
      "category": "string (Shipped | InProgress | CarriedOver)",
      "status": "string (informational, e.g., 'Done', 'In Progress')",
      "storyPoints": "integer",
      "completedDate": "ISO 8601 date or null"
    }
  ],
  "metrics": {
    "totalStoryPoints": "integer",
    "completedStoryPoints": "integer",
    "inProgressStoryPoints": "integer",
    "carriedOverCount": "integer",
    "velocityPerSprint": "number (double)"
  }
}
```

### Data Constraints

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| Project.Name | string | Yes | Non-empty, ≤100 chars |
| Project.Status | string | Yes | Must be "On Track", "At Risk", or "Off Track" |
| Project.Owner | string | Yes | Non-empty, ≤50 chars |
| Project.StartDate | DateTime | Yes | Valid ISO 8601 date |
| Project.TargetEndDate | DateTime | Yes | Must be ≥ StartDate |
| Milestone.Id | string | Yes | Unique across milestones |
| Milestone.Title | string | Yes | Non-empty, ≤100 chars |
| Milestone.DueDate | DateTime | Yes | Valid ISO 8601 date |
| Milestone.IsCompleted | bool | Yes | true/false |
| Milestone.Order | int | Yes | Positive integer, unique within milestones |
| WorkItem.Id | string | Yes | Unique across work items |
| WorkItem.Title | string | Yes | Non-empty, ≤150 chars |
| WorkItem.Category | string | Yes | "Shipped", "InProgress", or "CarriedOver" |
| WorkItem.Status | string | No | For display/reference only |
| WorkItem.StoryPoints | int | Yes | Non-negative integer |
| WorkItem.CompletedDate | DateTime? | No | Nullable; valid ISO 8601 if provided |
| Metrics.TotalStoryPoints | int | Yes | Non-negative |
| Metrics.CompletedStoryPoints | int | Yes | Non-negative, ≤ TotalStoryPoints |
| Metrics.InProgressStoryPoints | int | Yes | Non-negative |
| Metrics.CarriedOverCount | int | Yes | Non-negative |
| Metrics.VelocityPerSprint | double | Yes | Non-negative |

### No Database Persistence

- All data is read-only from `data.json`
- No changes are persisted back to file
- Optional SQLite database exists in project but is not required for MVP
- Future integration with JIRA/Azure DevOps would replace JSON source

---

## API Contracts

### Services

#### IDataService

**Method: LoadProjectDataAsync()**

**Signature:**
```csharp
Task<ProjectData> LoadProjectDataAsync()
```

**Request:** None (reads from file system)

**Response:**
```csharp
{
  ProjectData data = new ProjectData
  {
    Project = new ProjectMetadata { ... },
    Milestones = new List<Milestone> { ... },
    WorkItems = new List<WorkItem> { ... },
    Metrics = new ProjectMetrics { ... }
  }
}
```

**Error Responses:**
- `FileNotFoundException`: `data.json` not found at `wwwroot/data/data.json`
- `JsonException`: JSON is malformed or invalid
- `InvalidOperationException`: Deserialization produced null object

**HTTP Status Code (if wrapped in API endpoint):** 200 (success) or 500 (error)

---

#### ICalculationService

**Method: CalculateMetrics(ProjectData data)**

**Signature:**
```csharp
ProjectCalculations CalculateMetrics(ProjectData data)
```

**Request:**
```csharp
{
  ProjectData data // Deserialized from JSON
}
```

**Response:**
```csharp
{
  ProjectCalculations calculations = new ProjectCalculations
  {
    PercentageComplete = 45,          // 0-100
    TotalStoryPoints = 100,
    ShippedCount = 7,
    InProgressCount = 5,
    CarriedOverCount = 2,
    VelocityPerSprint = 20.0
  }
}
```

**Error Handling:** None (pure logic; input validation delegated to DataService)

**Calculation Formulas:**
- **PercentageComplete** = (CompletedStoryPoints / TotalStoryPoints) × 100, or 0 if TotalStoryPoints == 0
- **ShippedCount** = Count of WorkItems where Category == "Shipped"
- **InProgressCount** = Count of WorkItems where Category == "InProgress"
- **CarriedOverCount** = Count of WorkItems where Category == "CarriedOver"
- **VelocityPerSprint** = Copied directly from Metrics.VelocityPerSprint (no calculation)

---

### Blazor Component Parameters

#### Dashboard

No parameters (root component)

**State:**
- `ProjectData projectData`
- `ProjectCalculations calculations`
- `string errorMessage`
- `bool isLoading`

---

#### Header

**Parameters:**
```csharp
[Parameter]
public ProjectMetadata Project { get; set; }
```

**Renders:**
- Project name
- Status with color code
- Owner name
- Start and end dates

---

#### Timeline

**Parameters:**
```csharp
[Parameter]
public List<Milestone> Milestones { get; set; }
```

**Renders:**
- Horizontal row of milestones
- Each as `MilestoneIndicator` child component

---

#### MetricsPanel

**Parameters:**
```csharp
[Parameter]
public ProjectCalculations Calculations { get; set; }
```

**Renders:**
- Grid of metric cards
- Each card displays a KPI

---

#### StatusBoard

**Parameters:**
```csharp
[Parameter]
public List<WorkItem> WorkItems { get; set; }
```

**Renders:**
- Three columns (Shipped, InProgress, CarriedOver)
- Each as `StatusColumn` child component

---

#### StatusColumn

**Parameters:**
```csharp
[Parameter]
public string ColumnTitle { get; set; }

[Parameter]
public List<WorkItem> Items { get; set; }
```

**Renders:**
- Column header with title and count
- Grid of `StatusCard` items

---

#### StatusCard

**Parameters:**
```csharp
[Parameter]
public WorkItem WorkItem { get; set; }
```

**Renders:**
- Card with title and story points

---

### Error Handling

**Data Load Failures:**
1. If `data.json` not found: Display message "Unable to load project data. File not found."
2. If JSON malformed: Display message "Unable to load project data. JSON parsing failed."
3. If deserialization fails: Display message "Unable to load project data. Invalid data format."

**User-Facing Error Message Template:**
```html
<div class="error-alert">
  <p>⚠️ Unable to load project data.</p>
  <p>{{ ErrorMessage }}</p>
  <p><a href="javascript:location.reload()">Refresh page</a></p>
</div>
```

---

## Infrastructure Requirements

### Hosting Environment

**Local Deployment:**
- Windows, Linux, or macOS with .NET 8.0 runtime or SDK
- Blazor Server host (ASP.NET Core)
- No cloud services required

**Network:**
- Localhost or internal network only
- No internet connectivity required
- No external API calls or third-party service dependencies

**Resources:**
- CPU: 1+ cores (minimal load)
- RAM: 512MB minimum; 1GB recommended
- Disk: 500MB for .NET SDK + runtime + application files
- No database service required (local file system only)

### File System Layout

```
C:\Git\AgentSquad\                           (Git repository root)
├── src\
│   └── AgentSquad.Runner\                   (Main .sln project)
│       ├── AgentSquad.Runner.csproj
│       ├── Components\
│       │   ├── Dashboard.razor
│       │   ├── Header.razor
│       │   ├── Timeline.razor
│       │   ├── MilestoneIndicator.razor
│       │   ├── MetricsPanel.razor
│       │   ├── StatusBoard.razor
│       │   ├── StatusColumn.razor
│       │   ├── StatusCard.razor
│       │   └── Footer.razor
│       ├── Models\
│       │   ├── ProjectData.cs
│       │   ├── ProjectMetadata.cs
│       │   ├── Milestone.cs
│       │   ├── WorkItem.cs
│       │   ├── ProjectMetrics.cs
│       │   └── ProjectCalculations.cs
│       ├── Services\
│       │   ├── IDataService.cs
│       │   ├── DataService.cs
│       │   ├── ICalculationService.cs
│       │   └── CalculationService.cs
│       ├── wwwroot\
│       │   ├── css\
│       │   │   └── dashboard.css
│       │   ├── data\
│       │   │   └── data.json
│       │   └── index.html
│       ├── Properties\
│       │   └── launchSettings.json
│       ├── Program.cs
│       ├── appsettings.json
│       └── bin\, obj\
├── tests\
│   ├── AgentSquad.Runner.Tests\
│   │   ├── AgentSquad.Runner.Tests.csproj
│   │   ├── Services\
│   │   │   ├── DataServiceTests.cs
│   │   │   └── CalculationServiceTests.cs
│   │   └── bin\, obj\
│   └── ...
└── AgentSquad.sln                           (Solution file)
```

### Static Files & Configuration

**data.json Location:**
- Path: `wwwroot/data/data.json`
- Deployed as part of published application
- Editable by project managers post-deployment
- Must be valid JSON; no server-side validation of schema

**CSS Location:**
- Path: `wwwroot/css/dashboard.css`
- Imported in `index.html` or component-scoped via `@import`
- Optional: Component-scoped CSS files (e.g., `Dashboard.razor.css`)

**index.html:**
- Path: `wwwroot/index.html`
- Standard Blazor Server boilerplate
- Includes `<app>` element and `app.bundle.js` script
- No custom JavaScript required for MVP

### Dependency Versions

- **.NET SDK**: 8.0.x (LTS)
- **Blazor Server**: 8.0.x (built-in)
- **System.Text.Json**: 8.0.x (built-in)
- **xUnit**: 2.6.x (testing)
- **Moq**: 4.20.x (mocking)
- **Entity Framework Core**: 8.0.x (optional, not used for MVP)

### Database (Optional)

- **SQLite**: `agentsquad_azurenerd_ReportingDashboard.db` (pre-existing in project)
- **Purpose**: Optional future integration; not required for MVP
- **Note**: If used, connection string in `appsettings.json`

### CI/CD (Optional)

Not required for MVP. Future considerations:
- GitHub Actions or Azure Pipelines for automated builds
- Deployment to internal IIS or Docker registry
- Automated screenshot testing for regression detection

---

## Technology Stack Decisions

### Why Blazor Server?

**Chosen:** Blazor Server 8.0.x

**Justification:**
- Mandatory per requirements; non-negotiable
- Server-side rendering produces clean HTML suitable for screenshots
- Built-in component reusability eliminates SPA framework complexity (React, Vue, Angular not needed)
- Two-way data binding (`@bind`) simplifies state management
- No JavaScript expertise required for basic functionality
- Integrated with Visual Studio 2022 and .NET tooling

**Alternatives Considered & Rejected:**
- Blazor WASM: Adds complexity; no benefit for local-only dashboard
- ASP.NET Core MVC: More boilerplate for simple single-page app
- PHP/Node.js/Python: Non-standard for .NET 8 stack

---

### Why System.Text.Json?

**Chosen:** System.Text.Json (built-in to .NET 8)

**Justification:**
- Zero external dependencies; reduces deployment complexity
- High performance for small-to-medium JSON files (<5MB)
- Native support in .NET 8; no compatibility issues
- Async deserialization via `DeserializeAsync<T>()`
- Built-in attribute-based property mapping (`[JsonPropertyName]`)

**Alternatives Considered & Rejected:**
- Newtonsoft.Json: More mature, but adds NuGet dependency; overkill for simple schema

---

### Why Vanilla CSS?

**Chosen:** Vanilla CSS (no framework)

**Justification:**
- Full control over layout and print styles
- CSS Grid and Flexbox provide all needed layout primitives
- Minimal file size; no runtime overhead
- BEM naming convention ensures maintainability
- Responsive design via `@media` queries

**Alternatives Considered & Rejected:**
- Bootstrap: Overkill; adds 50KB+ CSS; opinionated design conflicts with custom layout
- Tailwind CSS: Requires build tooling; adds complexity
- Vuetify/Material-UI: Frontend framework dependencies; not applicable to Blazor Server

---

### Why No Database for MVP?

**Chosen:** JSON file storage only

**Justification:**
- Data is read-only; no persistence layer needed
- `data.json` is manually maintained by project managers
- Eliminates Entity Framework Core configuration complexity
- Faster time-to-market; no schema migrations
- File can be generated externally (JIRA, Azure DevOps) in future

**Future Evolution:**
- Phase 2: Optional SQLite for data snapshots/historical trends
- Phase 3: Optional integration with JIRA/Azure DevOps API

---

### Why No Authentication?

**Chosen:** No authentication required

**Justification:**
- Internal-only tool; no sensitive data exposed
- Stakeholders are trusted employees
- Reduces development time and operational complexity
- No multi-user access control required

**Future Evolution (if needed):**
- Simple Windows auth (IIS + Active Directory)
- OAuth 2.0 via enterprise identity provider (Entra ID)

---

### Why xUnit + Moq for Testing?

**Chosen:** xUnit 2.6.x + Moq 4.20.x

**Justification:**
- Industry-standard for .NET testing
- xUnit: Simpler syntax than NUnit; better async/await support
- Moq: Lightweight mocking for dependency injection
- Tight integration with Visual Studio test runner
- Fast test execution; no external service dependencies

**Test Coverage Target:** >80% for DataService and CalculationService

---

## Security Considerations

### Data Protection

**At Rest:**
- `data.json` stored in plain text as static file
- No encryption required; contains mock/fictional project metrics only
- File permissions: Standard web server read access (typically `webroot` directory)
- No personal identifiable information (PII) or sensitive business data

**In Transit:**
- HTTP sufficient for local/internal network deployment
- HTTPS optional; not required per requirements
- Blazor Server WebSocket connection is same-origin (localhost or internal network)

**Authentication & Authorization:**
- No authentication required
- No user sessions or cookies
- No authorization checks; read-only access for all viewers
- No audit trail of data access

### Input Validation

**JSON Deserialization:**
- `System.Text.Json` validates JSON structure on parse
- Missing required fields handled gracefully with `null` values
- Type mismatches raise `JsonException` (caught and displayed to user)
- No code injection vectors (JSON is not executed)

**Component Parameters:**
- Nullable parameters validated in components
- Empty lists handled with `?? new List<T>()`
- Division by zero guarded in CalculationService (`if (totalStoryPoints > 0)`)

### Content Security

- No `<script>` injection vectors (Blazor Server escapes HTML output)
- No inline JavaScript required for MVP
- CSS is static; no dynamic style injection
- URLs hardcoded (no user input in navigation)

### File System Access

- DataService reads only from `wwwroot/data/data.json`
- Path is hardcoded; no user-controlled file path
- File existence check before read; graceful error if missing
- No write access to file system (read-only application)

### Deployment Security

**Local Network:**
- Firewall: Restrict access to authorized subnets/machines
- IIS Binding: Use internal hostname (e.g., `reporting-dashboard.internal`)
- No public internet exposure
- VPN or network segmentation if across multiple physical locations

**Future Cloud Deployment:**
- Add TLS 1.2+ encryption
- Implement Azure AD authentication
- Use Azure Key Vault for connection strings (if database added)
- Enable Azure WAF for DDoS/injection protection

---

## Scaling Strategy

### Current Scale (MVP)

**Design Assumptions:**
- 5-10 concurrent users (project managers, executives)
- 1 project instance per deployment
- Up to 500 work items, 10-20 milestones
- Data refresh: manual (user clicks F5 or refresh button)
- No real-time updates

**Performance Targets:**
- Page load: <3 seconds
- JSON deserialization: <500ms
- Rendering: <1 second
- No perceptible lag with 500 work items

### Scaling Approaches (Post-MVP)

**Horizontal Scaling:**
1. Deploy multiple instances behind a load balancer
   - Requires stateless Blazor Server (no session affinity needed; `data.json` is immutable)
   - Load balancer routes to least-busy instance
   - Each instance loads its own copy of `data.json`

2. Shared `data.json` via network file share (if scaling to many instances)
   - SMB/NFS file share on dedicated file server
   - Each instance reads from shared location
   - Requires network latency mitigation (caching)

**Vertical Scaling:**
1. Increase server resources (CPU, RAM)
   - Handles more concurrent Blazor Server WebSocket connections
   - Allows larger datasets to load into memory

2. Connection pooling (if database added)
   - EF Core connection strings with MaxPoolSize settings
   - Reduces connection overhead for snapshot queries

**Data Volume Scaling:**
1. Pagination of work items (beyond 500)
   ```csharp
   // Example: Load 50 items per "page", fetch on demand
   List<WorkItem> GetWorkItemsForPage(int pageNumber, int pageSize = 50)
   {
       return AllWorkItems.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
   }
   ```

2. Virtual scrolling (Blazor `Virtualize` component)
   ```razor
   <Virtualize Items="@LargeWorkItemList" Context="item">
       <StatusCard WorkItem="@item" />
   </Virtualize>
   ```

3. SQL Server database (for ad-hoc queries, filtering, aggregations)
   - Replace JSON file with EF Core DbContext
   - Implement repository pattern for data access
   - Add indexes on `Category`, `Status`, `DueDate`

**Multi-Project Scaling:**
1. Add project selector dropdown at top of Dashboard
   - Parameter: `@bind-SelectedProjectId`
   - DataService: `LoadProjectDataAsync(string projectId)`
   - Reads from `wwwroot/data/{projectId}/data.json`

2. Backend API for project list
   ```csharp
   [HttpGet("/api/projects")]
   public async Task<List<ProjectSummary>> GetProjects()
   {
       // Return list of available projects with metadata
   }
   ```

### Caching Strategy

**Current (No Cache):**
- DataService reads `data.json` on every Dashboard load
- Acceptable for manual refresh workflow

**Future (Caching Layer):**
```csharp
public class CachedDataService : IDataService
{
    private ProjectData _cache;
    private DateTime _cacheTime;
    private const int CACHE_DURATION_MINUTES = 5;
    
    public async Task<ProjectData> LoadProjectDataAsync()
    {
        if (_cache != null && DateTime.UtcNow - _cacheTime < TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
            return _cache;
        
        _cache = await LoadFromFileAsync();
        _cacheTime = DateTime.UtcNow;
        return _cache;
    }
}
```

### Monitoring & Observability (Post-MVP)

1. Serilog + Application Insights
   - Log data load time, deserialization errors, rendering time
   - Track user session duration and refresh frequency
   - Alert on exceptions or slow page loads

2. Performance metrics
   - Page load time (via browser DevTools / New Relic)
   - JSON file size growth
   - Server memory and CPU usage
   - WebSocket connection count

---

## Risks & Mitigations

### Risk 1: Missing or Corrupted `data.json`

**Impact:** High — Application cannot render; user sees error page

**Likelihood:** Low (file is checked into version control)

**Mitigation:**
1. Validate file existence and JSON schema on startup
2. Display graceful error message with recovery instructions
3. Provide downloadable template `data.json.sample` as fallback
4. Automated tests for file parsing

**Code Example:**
```csharp
public async Task<ProjectData> LoadProjectDataAsync()
{
    var dataPath = Path.Combine(_env.WebRootPath, "data", "data.json");
    
    if (!File.Exists(dataPath))
    {
        throw new FileNotFoundException($"Data file not found. Expected path: {dataPath}");
    }
    
    using var stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read);
    var data = await JsonSerializer.DeserializeAsync<ProjectData>(stream)
        ?? throw new InvalidOperationException("Deserialized data is null");
    
    ValidateProjectData(data); // Schema validation
    return data;
}
```

---

### Risk 2: JSON Schema Evolution

**Impact:** Medium — Old data format incompatible with new code

**Likelihood:** Medium (requirements may change post-MVP)

**Mitigation:**
1. Use nullable types in C# model for backward compatibility
2. Implement versioning in `data.json` (e.g., `"version": "1.0"`)
3. Add schema validation layer with clear error messages
4. Document breaking changes in CHANGELOG.md

**Code Example:**
```csharp
public class ProjectData
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";
    
    [JsonPropertyName("project")]
    public ProjectMetadata Project { get; set; }
    
    // Nullable for future extensibility
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
```

---

### Risk 3: Large Dataset Performance Degradation

**Impact:** Medium — Dashboard becomes sluggish with 1000+ work items

**Likelihood:** Low (MVP assumption is <500 items)

**Mitigation:**
1. Implement virtual scrolling (Blazor `Virtualize` component)
2. Add pagination for large lists
3. Pre-filter items by category server-side before rendering
4. Monitor rendering time in production; set alerts at 2s threshold

**Code Example:**
```razor
<div class="status-column__body">
    <Virtualize Items="@Items" Context="item">
        <StatusCard WorkItem="@item" />
    </Virtualize>
</div>
```

---

### Risk 4: Print Layout Breakage Across Browsers

**Impact:** High (for executives using print feature) — Content overflows, missing elements

**Likelihood:** Medium (CSS print rules are fragile)

**Mitigation:**
1. Test `@media print` in Chrome, Firefox, Edge before release
2. Use CSS page breaks to control pagination
3. Set fixed font sizes and margins (avoid relative units in print)
4. Provide "Download as PDF" button via headless browser API (future enhancement)

**CSS Example:**
```css
@media print {
    body {
        margin: 0;
        padding: 0.5in;
    }
    
    .timeline {
        page-break-inside: avoid;
    }
    
    .status-board {
        display: block; /* Stack columns vertically in print */
    }
    
    .status-column {
        page-break-inside: avoid;
    }
}
```

---

### Risk 5: Blazor Server WebSocket Connection Failure

**Impact:** High (user unable to interact with dashboard)

**Likelihood:** Low (internal network, stable infrastructure)

**Mitigation:**
1. Display reconnection status/error message to user
2. Implement automatic page reload after prolonged disconnection
3. Blazor Server built-in recovery (automatically attempts reconnect)
4. Monitor WebSocket connection stability in production logs

**Code Example (Blazor automatically handles reconnection):**
```html
<!-- app.html or index.html -->
<script src="_framework/blazor.server.js"></script>
<script>
    Blazor.defaultReconnectionHandler._reconnectCallback = () => {
        console.log("Blazor reconnecting...");
    };
</script>
```

---

### Risk 6: Component Rendering Errors

**Impact:** High (child component exception crashes entire page)

**Likelihood:** Low (simple components, limited logic)

**Mitigation:**
1. Add error boundary component (Blazor `ErrorBoundary`)
2. Wrap child components in try/catch blocks
3. Log exceptions to Serilog for debugging
4. Display user-friendly error cards instead of crashing

**Code Example:**
```razor
<ErrorBoundary>
    <ChildContent>
        <Timeline Milestones="@projectData.Milestones" />
    </ChildContent>
    <ErrorContent>
        <div class="error-message">
            Failed to render timeline. Please refresh the page.
        </div>
    </ErrorContent>
</ErrorBoundary>
```

---

### Risk 7: Data Manual Entry Errors

**Impact:** Medium (incorrect KPIs displayed)

**Likelihood:** Medium (project managers edit `data.json` manually)

**Mitigation:**
1. Provide `data.json` schema validation tool (e.g., JSON Schema validator)
2. Include comments in sample `data.json` with field descriptions
3. Add server-side validation of story points (non-negative, totals match)
4. Display warnings in UI if data inconsistencies detected

**Schema Validation (server-side):**
```csharp
private void ValidateProjectData(ProjectData data)
{
    if (data.Metrics.CompletedStoryPoints > data.Metrics.TotalStoryPoints)
        throw new InvalidOperationException("Completed story points cannot exceed total");
    
    if (data.WorkItems.Any(w => w.StoryPoints < 0))
        throw new InvalidOperationException("Story points cannot be negative");
    
    var categoryTotals = data.WorkItems
        .GroupBy(w => w.Category)
        .ToDictionary(g => g.Key, g => g.Sum(w => w.StoryPoints));
    
    var sumOfCategories = categoryTotals.Values.Sum();
    if (sumOfCategories != data.Metrics.TotalStoryPoints)
        Console.WriteLine("⚠️ Warning: Sum of category story points does not match total");
}
```

---

### Risk 8: Browser Compatibility Issues

**Impact:** Medium (dashboard renders incorrectly in unsupported browser)

**Likelihood:** Low (MVP targets latest Chrome, Firefox, Edge)

**Mitigation:**
1. Test in Chrome, Firefox, Edge (latest two versions) before release
2. Use CSS Grid and Flexbox (widely supported; avoid experimental features)
3. Provide clear error message if browser unsupported
4. Monitor browser usage analytics post-deployment

**Browser Version Matrix (Testing Plan):**
| Browser | Version | Test Print | Test Interactivity |
|---------|---------|------------|-------------------|
| Chrome | Latest, Latest-1 | ✓ | ✓ |
| Firefox | Latest, Latest-1 | ✓ | ✓ |
| Edge | Latest, Latest-1 | ✓ | ✓ |

---

### Risk 9: Static File Caching Issues

**Impact:** Low–Medium (users see stale CSS/HTML after updates)

**Likelihood:** Low (local deployment; cache busting not critical)

**Mitigation:**
1. Enable cache busting in `Program.cs` for static files
   ```csharp
   app.UseStaticFiles(new StaticFileOptions
   {
       OnPrepareResponse = ctx =>
       {
           ctx.Context.Response.Headers.Add("Cache-Control", "public, max-age=3600");
       }
   });
   ```
2. Users can force refresh (Ctrl+Shift+R) to clear browser cache
3. Document deployment steps (clear browser cache after updates)

---

### Risk 10: Regulatory/Compliance Gaps (Future)

**Impact:** Medium (if real project data used post-MVP)

**Likelihood:** Low (MVP uses mock data only)

**Mitigation:**
1. Audit if real project data is introduced
2. Implement data retention policies (backup, deletion)
3. Add audit logging of data access (if sensitive data handled)
4. Document data privacy measures in runbook

---

## Implementation Phases & Deliverables

### Phase 1: Foundation (Days 1-3)

**Deliverables:**
- Data models (ProjectData, Milestone, WorkItem, ProjectMetrics)
- DataService with JSON deserialization
- CalculationService with KPI formulas
- Unit tests for DataService (deserialization, error handling)
- Sample `data.json` with 5 milestones, 20 work items

**Testing:**
- xUnit tests for DataService and CalculationService
- Manual test: Verify `data.json` loads and deserializes correctly

**Output:**
- Compilable project with data layer functional
- Can instantiate and calculate metrics from sample data

---

### Phase 2: UI Components (Days 4-7)

**Deliverables:**
- Dashboard.razor (main page)
- Header, Timeline, MilestoneIndicator, MetricsPanel, StatusBoard, StatusColumn, StatusCard, Footer components
- Basic CSS layout using CSS Grid and Flexbox
- Error boundary for component exceptions
- Page renders with sample data; all stories 1-4 functional

**Testing:**
- Manual browser test (Chrome, Firefox, Edge)
- Verify all components render correct data
- Take screenshots to match design reference

**Output:**
- Fully functional dashboard displaying all project metadata, milestones, metrics, and work items
- Meets user stories 1-4, 7, and partially 5 (data loads, displays)

---

### Phase 3: Polish & Printing (Days 8-10)

**Deliverables:**
- CSS refinement (typography, spacing, colors, borders)
- Print styles (@media print) optimized for single/multi-page output
- Responsive design adjustments (tablet/mobile fallback)
- Final sample `data.json` with 5 milestones, 25-30 work items
- Error messages and fallback UI for missing data
- README.md with setup and usage instructions

**Testing:**
- Print test: Export to PDF in all three browsers; verify layout
- Screenshot test: Take full-page screenshots in all browsers
- Verify footer displays correctly
- Accessibility check: Zoom to 200%; confirm readability

**Output:**
- Production-ready dashboard
- Meets all 8 user stories
- Executives can take screenshot in <1 minute and insert into PowerPoint

---

### Phase 4: Testing & Validation (Days 9-10)

**Deliverables:**
- Acceptance testing checklist (all 8 user stories)
- Manual test results in Chrome, Firefox, Edge
- Print/screenshot validation (matches OriginalDesignConcept.html)
- Performance testing (page load time, rendering)
- Error scenario testing (missing data.json, malformed JSON)

**Output:**
- Sign-off from stakeholders on visual design and functionality
- Release notes with installation instructions
- Deployment package (.sln, compiled binaries, data files)

---

## Success Criteria (Summary)

| Criterion | Metric | Target | Status |
|-----------|--------|--------|--------|
| Page Load Time | Seconds | <3s | TBD |
| Data Deserialization | Milliseconds | <500ms | TBD |
| Test Coverage | % of DataService, CalculationService | >80% | TBD |
| User Story Pass Rate | # stories passing acceptance criteria | 8/8 | TBD |
| Browser Compatibility | # browsers passing tests | 3/3 (Chrome, Firefox, Edge) | TBD |
| Print Output Quality | # browsers rendering correctly | 3/3 | TBD |
| Error Handling | # gracefully handled error scenarios | All critical ones | TBD |
| Time to Screenshot | Minutes from startup | <5 min | TBD |

---

## Appendix: File Structure Summary

```
C:\Git\AgentSquad\
├── AgentSquad.sln                           (Solution file)
├── src\
│   └── AgentSquad.Runner\                   (Blazor Server project)
│       ├── Components\                      (Razor components)
│       │   ├── Dashboard.razor
│       │   ├── Header.razor
│       │   ├── Timeline.razor
│       │   ├── MilestoneIndicator.razor
│       │   ├── MetricsPanel.razor
│       │   ├── MetricCard.razor
│       │   ├── StatusBoard.razor
│       │   ├── StatusColumn.razor
│       │   ├── StatusCard.razor
│       │   └── Footer.razor
│       ├── Models\                         (Data entities)
│       │   ├── ProjectData.cs
│       │   ├── ProjectMetadata.cs
│       │   ├── Milestone.cs
│       │   ├── WorkItem.cs
│       │   ├── ProjectMetrics.cs
│       │   └── ProjectCalculations.cs
│       ├── Services\                       (Business logic)
│       │   ├── IDataService.cs
│       │   ├── DataService.cs
│       │   ├── ICalculationService.cs
│       │   └── CalculationService.cs
│       ├── wwwroot\                        (Static files)
│       │   ├── css\
│       │   │   ├── app.css                 (Blazor default)
│       │   │   └── dashboard.css           (Custom styles)
│       │   ├── data\
│       │   │   └── data.json               (Project data)
│       │   └── index.html                  (Entry point)
│       ├── Properties\
│       │   └── launchSettings.json
│       ├── Program.cs                      (Startup & DI)
│       ├── App.razor                       (Root component)
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── AgentSquad.Runner.csproj
└── tests\
    └── AgentSquad.Runner.Tests\
        ├── Services\
        │   ├── DataServiceTests.cs
        │   └── CalculationServiceTests.cs
        ├── AgentSquad.Runner.Tests.csproj
        └── obj\, bin\
```

---

## Conclusion

This architecture provides a straightforward, maintainable foundation for the Executive Project Reporting Dashboard. By leveraging Blazor Server's component model, System.Text.Json for data deserialization, and vanilla CSS for layout, the solution achieves MVP delivery in 2 weeks with minimal external dependencies. The modular component structure and dependency injection setup enable future enhancements (filtering, charting, multi-project support) without architectural refactoring. Error handling and validation ensure the dashboard remains robust even if `data.json` is corrupted or incomplete. Success depends on rigorous testing across target browsers and careful CSS tuning for print output—both of which are achievable within the project timeline.