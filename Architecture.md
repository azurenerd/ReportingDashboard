# Architecture

## Overview & Goals

**My Project** is a lightweight executive reporting dashboard built on C# .NET 8 with Blazor Server that visualizes project milestones, progress, and status from a JSON configuration file. The system transforms project data into an executive-ready, screenshot-optimized view suitable for PowerPoint presentations without requiring developer involvement or cloud infrastructure.

**Architectural Goals:**
1. **Simplicity First** - Single-page Blazor Server application with minimal external dependencies
2. **Local-Only Deployment** - Self-contained Windows executable requiring no .NET SDK or cloud access
3. **Real-Time Iteration** - File-watched data.json enables rapid executive deck updates without app restart
4. **Professional Visualization** - Screenshot-optimized dashboard rendering clean timelines and status summaries
5. **Zero Friction** - Read-only, no authentication, no multi-user complexity; double-click executable to launch
6. **Maintainability** - Clear component separation, dependency injection, and testability (>80% code coverage)

The architecture prioritizes **executive accessibility over scalability**. This is fundamentally a single-user, local-only tool; architectural decisions favor ease-of-use and minimal deployment complexity over horizontal scaling or multi-tenant patterns.

---

## System Components

### Core Services

#### DataProvider (Singleton Service)
**Responsibility:** Load, validate, cache, and refresh dashboard data from data.json; manage file watching.

**Public Interface:**
```csharp
public class DataProvider
{
    public Task<DashboardData> GetDashboardDataAsync();
    public Task RefreshDataAsync();
    public event EventHandler DataChanged;
    public void StartFileWatching();
    public void StopFileWatching();
}
```

**Dependencies:**
- IHostEnvironment (app root path resolution)
- IMemoryCache (in-memory caching with 5-min TTL)
- DashboardDataValidator (schema validation)
- ILogger<DataProvider> (error logging)
- FileSystemWatcher (file monitoring with 500ms debounce)

**Data Managed:**
- Cached DashboardData object
- File path to data.json (resolved via ContentRootPath)
- File watcher subscription state

**Behavior:**
- Load data.json asynchronously via File.ReadAllTextAsync (non-blocking)
- Deserialize JSON using System.Text.Json (6-10x faster than Newtonsoft)
- Validate against schema using DashboardDataValidator
- Cache in IMemoryCache (5-minute TTL)
- Invalidate cache on data.json file change (debounced 500ms)
- Raise DataChanged event on successful update
- Fallback to cached data if validation fails; display error message to user
- Log all exceptions to ./logs/error.log

---

#### DashboardDataValidator (Service)
**Responsibility:** Validate data.json structure; provide clear error messages for schema violations.

**Public Interface:**
```csharp
public class DashboardDataValidator
{
    public ValidationResult Validate(string jsonString);
    public ValidationResult Validate(DashboardData data);
    public string GetSchemaDefinition();
}
```

**Dependencies:**
- JsonSchema.NET v6.x (schema validation library)

**Data Managed:**
- JSON schema definition (embedded resource or constant)

**Validation Rules:**
- `projectName`: Required, non-empty, 1-100 chars
- `projectOwner`: Required, non-empty, 1-100 chars
- `reportingPeriod`: Required, format "Q# YYYY" or "Month YYYY"
- `timeline`: Required array of Milestone objects (min 1)
  - `name`: Required, 1-200 chars
  - `dueDate`: Required, ISO8601 datetime
  - `status`: Required, enum (NotStarted|InProgress|Shipped)
  - `completedDate`: Optional datetime (required if status=Shipped)
- `items`: Required array of ProjectItem objects (min 1)
  - `id`: Required, UUID format
  - `title`: Required, 1-300 chars
  - `category`: Required, enum (Shipped|InProgress|CarriedOver)
  - `dueDate`: Required, ISO8601 datetime
  - `priority`: Required, enum (Low|Medium|High)
  - `owner`: Required, 1-100 chars
  - `description`: Optional, 0-500 chars

**Error Handling:**
- Validation failure includes field path and constraint violated
- Human-readable messages ("'projectName' field is required")
- Technical details logged (line numbers, stack trace)

---

### UI Components

#### DashboardLayout.razor (Root Container)
**Responsibility:** Root application container; load data.json on startup; manage file watching; cascade data to child sections; orchestrate refresh UI.

**Public Interface:**
- `OnInitializedAsync()` - Load data on app startup
- `RefreshDataAsync()` - Manual refresh button handler
- `DisplayError(string message)` - Show validation/error banners

**Parameters:**
- None (root component)

**Cascading Parameters Provided:**
- DashboardData (to all children)

**Dependencies:**
- DataProvider (load/refresh data)
- IHostEnvironment (file paths)
- ILogger<DashboardLayout> (error logging)

**Data Managed:**
- DashboardData (cascading to children)
- IsLoading (bool flag during async load)
- ErrorMessage (string, displays error banner if set)
- FileSystemWatcher subscription

**Renders:**
- Header: Project name + reporting period
- Error banner (if data invalid or load fails)
- ProgressCardsSection (child)
- TimelineSection (child)
- ItemGridSection (child)
- MetricsFooter (child)
- Manual refresh button
- Loading spinner during data fetch

**Lifecycle:**
1. OnInitializedAsync: Call DataProvider.GetDashboardDataAsync()
2. Subscribe to DataProvider.DataChanged event
3. On event: Call RefreshDataAsync() to reload data
4. Handle exceptions: Display error, log to file, retain cached data if available
5. OnDispose: Unsubscribe from DataChanged event

---

#### ProgressCardsSection.razor
**Responsibility:** Display 3 summary cards (Shipped, In Progress, Carried Over) with counts and status indicators.

**Parameters:**
- [CascadingParameter] DashboardData Data

**Dependencies:**
- DashboardData (cascading)
- StatusBadge.razor (shared component)

**Data Managed:**
- None (read-only; aggregates Summary from DashboardData)

**Renders:**
- 3 cards, each with:
  - Color-coded badge (green=Shipped, yellow=InProgress, gray=CarriedOver)
  - Count (e.g., "5 Shipped")
  - Percentage of total items
  - Large, readable font (16px+) for accessibility
  - High contrast for WCAG AA compliance

**Behavior:**
- Calculate SummaryMetrics from Items.GroupBy(i => i.Category).Count()
- Detect overall health status: OverallHealthStatus = (OverdueCount > 0) ? "AtRisk" : "OnTrack"
- Cards update immediately when Data cascading parameter changes

---

#### TimelineSection.razor
**Responsibility:** Render milestone timeline as color-coded Gantt chart; highlight overdue milestones.

**Parameters:**
- [CascadingParameter] DashboardData Data

**Dependencies:**
- DashboardData (cascading)
- ApexCharts.Razor v2.5.x
- StatusBadge.razor (shared component)

**Data Managed:**
- None (read-only; displays Timeline from DashboardData)

**Renders:**
- ApexCharts Gantt chart with:
  - X-axis: Date range (earliest DueDate to latest DueDate + 30 days)
  - Y-axis: Milestone names
  - Bars color-coded by status:
    - Green: Shipped (status=Shipped)
    - Yellow: In Progress (status=InProgress)
    - Red: At Risk (status=NotStarted AND DueDate < Today)
    - Gray: Not Started (status=NotStarted AND DueDate >= Today)
  - Data labels: Milestone name, due date
  - Legend: Status indicators
  - SVG output (screenshot-safe, no hover effects)

**CSS:**
- Sticky header (remains visible during scroll)
- Responsive: Full width on desktop, scroll horizontally on mobile
- Print-optimized (@media print): No interactive elements

**Behavior:**
- Calculate overdue milestones: Where(m => m.DueDate < DateTime.Today && m.Status != MilestoneStatus.Shipped)
- Map Status enum to color via helper method
- Re-render when Data parameter changes

---

#### ItemGridSection.razor
**Responsibility:** Display filterable, sortable table of project items with category and priority indicators.

**Parameters:**
- [CascadingParameter] DashboardData Data

**Dependencies:**
- DashboardData (cascading)
- MudBlazor DataGrid v6.x
- StatusBadge.razor (shared component)

**Data Managed:**
- SelectedCategory (ItemCategory? nullable; null = all items)
- SortBy (string; default "Priority")
- SortDirection (SortDirection enum; Ascending/Descending)
- Preserve filter state in URL query string (?category=InProgress&sort=DueDate)

**Renders:**
- Filter buttons: All, Shipped, In Progress, Carried Over
- MudBlazor DataGrid with columns:
  - Title (clickable column header to sort)
  - Category (color-coded badge)
  - Owner (text)
  - Due Date (formatted "MMM dd, yyyy")
  - Priority (High/Medium/Low with icon)
- Row highlighting: Red background if item.IsOverdue (DueDate < Today && Category != Shipped)
- Pagination: 50 items per page (configurable)
- Sort indicators: Arrow (↑/↓) on active sort column

**Behavior:**
- Filter: Items.Where(i => _selectedCategory == null || i.Category == _selectedCategory)
- Sort: Items.OrderByDescending(i => i.Priority) (default); toggle on column click
- Debounce filter updates (200ms delay) to prevent excessive re-renders
- Update URL on filter change: `_navigationManager.NavigateTo($"?category={category}")`
- Parse URL on component init to restore filter state (bookmarking support)

---

#### MetricsFooter.razor
**Responsibility:** Display dashboard summary statistics: total item counts, health status, reporting period info.

**Parameters:**
- [CascadingParameter] DashboardData Data

**Dependencies:**
- DashboardData (cascading)

**Data Managed:**
- None (read-only; aggregates Summary)

**Renders:**
- Footer bar with:
  - "Total Items: X" (sum of all items)
  - "Shipped: X | In Progress: X | Carried Over: X"
  - "Overall Status: [OnTrack|AtRisk]" (visual indicator)
  - Reporting Period: "Q2 2026" or "April 2026"
  - Last Updated: Timestamp of data.json read

---

#### StatusBadge.razor (Shared Component)
**Responsibility:** Render color-coded status indicator badge.

**Parameters:**
- Status (MilestoneStatus | ItemCategory enum)
- Label (optional string override; defaults to Status.ToString())
- Size (Small|Medium|Large; default Medium)

**Dependencies:**
- Bootstrap CSS classes

**Data Managed:**
- None (stateless)

**Renders:**
- Inline badge with background color:
  - Green (#28a745): Shipped
  - Yellow (#ffc107): In Progress
  - Gray (#6c757d): Carried Over, Not Started
  - Red (#dc3545): At Risk (if date overdue)
- Font: 12-14px, white text, 4-8px rounded corners
- Optional icon (check, clock, warning) based on status

---

## Component Interactions

### Data Flow Diagram

```
File System (data.json)
       ↓
FileSystemWatcher (detects changes, 500ms debounce)
       ↓
DataProvider.RefreshDataAsync()
       ↓
DashboardDataValidator.Validate()
       ↓
(Success) → IMemoryCache (5-min TTL)
       ↓
DashboardLayout (re-fetches from cache)
       ↓
[CascadingParameter] DashboardData
       ├── ProgressCardsSection
       ├── TimelineSection
       ├── ItemGridSection
       └── MetricsFooter
       
(Failure) → Display error banner; retain cached data
       ↓
Error log written to ./logs/error.log
```

### Primary Use Cases

#### Use Case 1: Initial Application Load
1. DashboardLayout.OnInitializedAsync() triggered
2. Call DataProvider.GetDashboardDataAsync()
3. DataProvider checks IMemoryCache ("dashboard_data" key)
   - If cache hit: return cached DashboardData; skip file I/O
   - If cache miss: proceed to step 4
4. Read data.json from disk: File.ReadAllTextAsync(Path.Combine(ContentRootPath, "data.json"))
5. Deserialize JSON: JsonSerializer.Deserialize<DashboardData>(json)
6. Validate schema: DashboardDataValidator.Validate(data)
   - If invalid: throw ValidationException with field path
7. Cache in IMemoryCache with 5-min sliding expiration
8. Return DashboardData to DashboardLayout
9. Set IsLoading = false
10. DashboardLayout cascade DashboardData to children
11. Children components receive cascading parameter; render:
    - TimelineSection renders ApexCharts Gantt
    - ProgressCardsSection renders 3 summary cards
    - ItemGridSection renders MudBlazor DataGrid
    - MetricsFooter renders footer bar

**Timeline:** ~500-1000ms (file I/O + JSON parse + validation)

---

#### Use Case 2: File Update Trigger (Auto-Refresh)
1. PM edits data.json in text editor; saves file
2. FileSystemWatcher detects file change event (FileSystemEventArgs)
3. Debounce handler: Cancel previous timer; start 500ms timer
4. After 500ms (if no new events): Trigger DataProvider.RefreshDataAsync()
5. RefreshDataAsync invalidates IMemoryCache ("dashboard_data")
6. Call DataProvider.GetDashboardDataAsync() (proceeds to file I/O, same as Use Case 1)
7. On success: DataProvider raises DataChanged event
8. DashboardLayout subscribes to DataChanged; StateHasChanged() triggers re-render
9. Cascading parameter updates; all children re-render
10. If validation fails: Display error message; retain cached version

**Timeline:** ~1-2 seconds (file watch debounce + file I/O + re-render)

---

#### Use Case 3: Filter Items by Category
1. User clicks "In Progress" button in ItemGridSection
2. OnCategoryFilterChanged(ItemCategory.InProgress) handler triggered
3. Update SelectedCategory state variable
4. Update URL query string: _navigationManager.NavigateTo("?category=InProgress")
5. Compute filtered items: FilteredItems = Items.Where(i => i.Category == SelectedCategory).ToList()
6. MudBlazor DataGrid re-renders filtered rows
7. User can click same button again to toggle filter off (SelectedCategory = null)

**Timeline:** <100ms (client-side filtering, no server round-trip)

---

#### Use Case 4: Sort Items by Column
1. User clicks "Due Date" column header in ItemGridSection
2. OnSortColumnClicked("DueDate") handler triggered
3. If SortBy == "DueDate": Toggle SortDirection (Ascending ↔ Descending)
4. If SortBy != "DueDate": Update SortBy = "DueDate"; reset SortDirection = Ascending
5. Compute sorted items: 
   - If SortDirection == Ascending: Items.OrderBy(i => i.DueDate).ToList()
   - Else: Items.OrderByDescending(i => i.DueDate).ToList()
6. MudBlazor DataGrid re-renders sorted rows with sort indicator (↑/↓)

**Timeline:** <100ms (client-side sorting)

---

#### Use Case 5: Screenshot for PowerPoint
1. User views dashboard in browser (e.g., Chrome, Edge)
2. Press Print or Ctrl+P to open print dialog
3. Select "Save as PDF" or screenshot tool
4. CSS media query @media print activates:
   - Hide filter buttons (.no-print { display: none; })
   - Remove interactive elements
   - Fixed font sizes and colors for PDF
5. Capture screenshot: Dashboard renders cleanly with:
   - Project name + reporting period at top
   - Summary cards in row
   - Gantt timeline (sticky header visible)
   - Item table (first 50 rows visible without scrolling)
   - Color-coded status indicators
   - Professional spacing and typography
6. Embed screenshot in PowerPoint slide

**Design Considerations:**
- No hover-dependent elements (screenshot-safe)
- Minimum 16px font size (accessibility + readability)
- High contrast colors (WCAG AA compliant)
- Bootstrap responsive grid (fits single page)

---

#### Use Case 6: Manual Refresh Button
1. User clicks "Refresh" button in DashboardLayout header
2. OnRefreshClicked() handler triggered
3. Set IsLoading = true (show spinner)
4. Call DataProvider.RefreshDataAsync()
5. Invalidate cache; re-load data.json; validate
6. Set IsLoading = false (hide spinner)
7. On error: Display error banner; log to file

**Timeline:** ~500-1000ms

---

## Data Model

### Entities & Relationships

#### DashboardData (Root Aggregate)
Root entity containing all dashboard information for a single reporting period.

```csharp
public class DashboardData
{
    public string ProjectName { get; set; }                // Required, 1-100 chars
    public string ProjectOwner { get; set; }               // Required, 1-100 chars
    public string ReportingPeriod { get; set; }            // Required, format "Q# YYYY" or "Month YYYY"
    public List<Milestone> Timeline { get; set; }          // Required, min 1 item
    public List<ProjectItem> Items { get; set; }           // Required, min 1 item
    public SummaryMetrics Summary { get; set; }            // Calculated, not stored
}
```

**Relationships:**
- DashboardData.Timeline → List<Milestone> (composition; 0..* milestones per dashboard)
- DashboardData.Items → List<ProjectItem> (composition; 0..* items per dashboard)
- DashboardData.Summary → SummaryMetrics (aggregation; calculated from items, not persisted)

---

#### Milestone
Represents a project milestone with timeline and completion status.

```csharp
public class Milestone
{
    public string Id { get; set; }                         // UUID, auto-generated
    public string Name { get; set; }                       // Required, 1-200 chars
    public DateTime DueDate { get; set; }                  // Required, ISO8601
    public MilestoneStatus Status { get; set; }           // Enum: NotStarted (0), InProgress (1), Shipped (2)
    public DateTime? CompletedDate { get; set; }          // Nullable; required if Status == Shipped
}
```

**Computed Properties:**
- `IsOverdue` → (DueDate < DateTime.Today && Status != MilestoneStatus.Shipped)
- `DaysUntilDue` → (DueDate - DateTime.Today).TotalDays
- `StatusColor` → Green (Shipped), Yellow (InProgress), Red (Overdue), Gray (NotStarted)

---

#### ProjectItem
Represents a project deliverable or work item with ownership and priority.

```csharp
public class ProjectItem
{
    public string Id { get; set; }                         // UUID, auto-generated
    public string Title { get; set; }                      // Required, 1-300 chars
    public ItemCategory Category { get; set; }            // Enum: Shipped (0), InProgress (1), CarriedOver (2)
    public DateTime DueDate { get; set; }                  // Required, ISO8601
    public Priority Priority { get; set; }                // Enum: Low (0), Medium (1), High (2)
    public string Owner { get; set; }                      // Required, 1-100 chars
    public string Description { get; set; }               // Optional, 0-500 chars
}
```

**Computed Properties:**
- `IsOverdue` → (DueDate < DateTime.Today && Category != ItemCategory.Shipped)
- `StatusColor` → Green (Shipped), Yellow (InProgress), Gray (CarriedOver)
- `PriorityIcon` → ↑ (High), → (Medium), ↓ (Low)

---

#### SummaryMetrics (Calculated)
Aggregated metrics derived from Items; not stored in data.json.

```csharp
public class SummaryMetrics
{
    public int TotalShipped { get; set; }                  // Count where Category == Shipped
    public int TotalInProgress { get; set; }               // Count where Category == InProgress
    public int TotalCarriedOver { get; set; }              // Count where Category == CarriedOver
    public int TotalItems { get; set; }                    // Sum of above 3
    public int OverdueCount { get; set; }                  // Count where IsOverdue == true
    public HealthStatus OverallHealthStatus { get; set; } // OnTrack (if OverdueCount == 0), AtRisk (if > 0)
    public DateTime LastUpdated { get; set; }              // Timestamp of data.json read
}
```

**Calculation:**
```csharp
public static SummaryMetrics Calculate(List<ProjectItem> items)
{
    var shipped = items.Count(i => i.Category == ItemCategory.Shipped);
    var inProgress = items.Count(i => i.Category == ItemCategory.InProgress);
    var carriedOver = items.Count(i => i.Category == ItemCategory.CarriedOver);
    var overdue = items.Count(i => i.IsOverdue);
    
    return new SummaryMetrics
    {
        TotalShipped = shipped,
        TotalInProgress = inProgress,
        TotalCarriedOver = carriedOver,
        TotalItems = shipped + inProgress + carriedOver,
        OverdueCount = overdue,
        OverallHealthStatus = overdue > 0 ? HealthStatus.AtRisk : HealthStatus.OnTrack,
        LastUpdated = DateTime.UtcNow
    };
}
```

---

#### Enumerations

**MilestoneStatus**
```csharp
public enum MilestoneStatus
{
    NotStarted = 0,    // Milestone not started; no work begun
    InProgress = 1,    // Work ongoing; on track to complete
    Shipped = 2        // Milestone completed; delivery date met or exceeded
}
```

**ItemCategory**
```csharp
public enum ItemCategory
{
    Shipped = 0,       // Deliverable completed and shipped
    InProgress = 1,    // Work ongoing; expected completion on schedule
    CarriedOver = 2    // Deferred from previous reporting period; not yet shipped
}
```

**Priority**
```csharp
public enum Priority
{
    Low = 0,           // Nice-to-have; can slip if needed
    Medium = 1,        // Important; strive to ship on schedule
    High = 2           // Critical; ship at all costs; blocks other work
}
```

**HealthStatus**
```csharp
public enum HealthStatus
{
    OnTrack,           // No overdue items; project progressing normally
    AtRisk             // 1+ overdue items; project at risk of further delays
}
```

---

### Storage

**Format:** JSON (text-based)

**Location:** Local file system
- Path: `{IHostEnvironment.ContentRootPath}/data.json`
- Typically: `C:\path\to\MyProject.exe/../data.json`

**Caching Strategy:**
- IMemoryCache with 5-minute sliding expiration
- Cache invalidation on file change (FileSystemWatcher)
- Manual invalidation on error (fallback to previous version)

**Versioning & Backup:**
- Git version control (developers commit data.json updates)
- Manual backups recommended (PM team responsibility)
- No automated snapshots (out of scope)

**Size Limits:**
- Typical: <5MB (supports up to 10,000 items + milestones)
- Performance degrades >10,000 items (migration to SQLite recommended in Phase 2)

---

### data.json Schema

**Complete Example:**
```json
{
  "projectName": "Q2 2026 Product Release",
  "projectOwner": "John Executive",
  "reportingPeriod": "Q2 2026",
  "timeline": [
    {
      "id": "m001",
      "name": "Design Phase Complete",
      "dueDate": "2026-04-30T23:59:59Z",
      "status": "Shipped",
      "completedDate": "2026-04-28T17:00:00Z"
    },
    {
      "id": "m002",
      "name": "MVP Development",
      "dueDate": "2026-06-15T23:59:59Z",
      "status": "InProgress",
      "completedDate": null
    },
    {
      "id": "m003",
      "name": "Beta Launch",
      "dueDate": "2026-07-31T23:59:59Z",
      "status": "NotStarted",
      "completedDate": null
    }
  ],
  "items": [
    {
      "id": "item-001",
      "title": "User Authentication Module",
      "category": "Shipped",
      "dueDate": "2026-04-20T23:59:59Z",
      "priority": "High",
      "owner": "Alice Smith",
      "description": "OAuth 2.0 + JWT token management"
    },
    {
      "id": "item-002",
      "title": "Payment Gateway Integration",
      "category": "InProgress",
      "dueDate": "2026-05-15T23:59:59Z",
      "priority": "High",
      "owner": "Bob Johnson",
      "description": "Stripe + PayPal integration"
    },
    {
      "id": "item-003",
      "title": "Mobile Optimization",
      "category": "CarriedOver",
      "dueDate": "2026-04-30T23:59:59Z",
      "priority": "Medium",
      "owner": "Carol Lee",
      "description": "Responsive CSS for iOS + Android browsers"
    }
  ]
}
```

---

## API Contracts

### Service Interfaces

#### IDataProvider
```csharp
public interface IDataProvider
{
    /// <summary>
    /// Load dashboard data from cache or file; validate schema.
    /// </summary>
    /// <returns>DashboardData if successful; throws ValidationException if invalid.</returns>
    /// <exception cref="FileNotFoundException">data.json not found.</exception>
    /// <exception cref="JsonException">JSON parsing error.</exception>
    /// <exception cref="ValidationException">Schema validation failed.</exception>
    Task<DashboardData> GetDashboardDataAsync();
    
    /// <summary>
    /// Invalidate cache; reload data from file.
    /// </summary>
    Task RefreshDataAsync();
    
    /// <summary>
    /// Event raised when data successfully reloaded.
    /// Subscribers: DashboardLayout listens and triggers re-render.
    /// </summary>
    event EventHandler DataChanged;
    
    /// <summary>
    /// Start FileSystemWatcher; monitor data.json changes.
    /// Call from DashboardLayout.OnInitialized.
    /// </summary>
    void StartFileWatching();
    
    /// <summary>
    /// Stop FileSystemWatcher; cleanup resources.
    /// Call from DashboardLayout.OnDispose.
    /// </summary>
    void StopFileWatching();
}
```

---

#### IDataValidator
```csharp
public interface IDataValidator
{
    /// <summary>
    /// Validate JSON string against schema.
    /// </summary>
    /// <returns>ValidationResult with IsValid flag and error details.</returns>
    ValidationResult Validate(string jsonString);
    
    /// <summary>
    /// Validate deserialized DashboardData object against schema.
    /// </summary>
    ValidationResult Validate(DashboardData data);
    
    /// <summary>
    /// Return JSON Schema definition (for debugging).
    /// </summary>
    string GetSchemaDefinition();
}
```

---

#### Blazor Component Cascading Parameters
```csharp
public partial class TimelineSection : ComponentBase
{
    /// <summary>
    /// Dashboard data cascade from parent (DashboardLayout).
    /// Triggers component re-render when DashboardData reference changes.
    /// </summary>
    [CascadingParameter]
    public DashboardData Data { get; set; }
}
```

---

### Error Handling

**Exception Types**

1. **FileNotFoundException**
   - When: data.json file not found at ContentRootPath
   - Message: "data.json not found at {path}. Ensure file is in app root directory."
   - Action: Display error banner; provide manual refresh button
   - Fallback: Retain cached data if available

2. **JsonException**
   - When: JSON parsing fails (malformed JSON)
   - Message: "Invalid JSON format. Error at line {lineNumber}, character {charPosition}: {details}"
   - Action: Display error with line number; log full stack trace
   - Fallback: Retain cached data

3. **ValidationException**
   - When: Schema validation fails (missing required field, wrong type, etc.)
   - Message: "Schema validation failed: 'projectName' field is required. Ensure data.json matches expected schema."
   - Action: Display error; highlight problem field
   - Fallback: Retain cached data

4. **UnauthorizedAccessException**
   - When: File permission denied (read access blocked)
   - Message: "Permission denied reading data.json. Ensure app has read access to file."
   - Action: Display error; log to error.log
   - Fallback: Retain cached data

---

**Error Response Format (UI)**
```
┌─────────────────────────────────────────┐
│ ⚠ Error Loading Dashboard Data          │
│                                         │
│ Schema validation failed:               │
│ 'projectName' field is required.        │
│ Ensure all required fields are present. │
│                                         │
│ [Manual Refresh] [Learn More]           │
└─────────────────────────────────────────┘
```

---

**Error Logging (File)**
```
[2026-04-07 19:51:02.145 UTC] ERROR | DataProvider | FileNotFoundException
File Path: C:\MyProject\data.json
Message: data.json not found at {path}
Stack Trace: ...
Cached Data Retained: Yes (Last loaded: 2026-04-07 19:45:00)

[2026-04-07 19:52:15.832 UTC] ERROR | DashboardDataValidator | ValidationException
Schema Validation Failed: 
- Field: timeline[0].dueDate | Error: Invalid date format. Expected ISO8601.
- Field: items[2].priority | Error: Invalid enum value 'Critical'. Expected High|Medium|Low.
Validation Rules: See schema at path...
```

---

### Blazor Component Event Handlers

**DashboardLayout**
- `OnInitializedAsync()` - Load data; start file watching
- `OnRefreshClicked()` - Manual refresh handler
- `OnFileChanged(object sender, FileSystemEventArgs e)` - File watch event
- `OnDispose()` - Cleanup file watcher; unsubscribe events

**ItemGridSection**
- `OnCategoryFilterChanged(ItemCategory? category)` - Filter button clicked
- `OnSortColumnClicked(string columnName)` - Column header clicked
- `OnPageChanged(int pageNumber)` - Pagination
- `OnPageSizeChanged(int pageSize)` - Items per page

---

## Infrastructure Requirements

### Hosting & Deployment

**Environment:** Windows 10+ Local Machine or Network Share

**Execution Model:**
- Single self-contained executable (.exe)
- No .NET Runtime prerequisite
- No installer or configuration wizard
- Double-click to launch; opens browser to https://localhost:5001

**Network & Ports:**
- **Protocol:** HTTPS (TLS 1.2+)
- **Port:** 5001 (default; configurable)
- **Host:** localhost (127.0.0.1)
- **Network Requirements:** None (local-only; no outbound connections)
- **Firewall:** localhost exempt from Windows Firewall; no rules required

**Multi-Instance Behavior:**
- Each .exe instance runs independently
- No port conflicts (OS assigns different ports if port 5001 unavailable)
- No shared state; multiple users can run same executable simultaneously
- Each instance has own IMemoryCache

---

### Storage

**Local File System**

**data.json**
- **Path:** `{IHostEnvironment.ContentRootPath}/data.json`
- **Example:** `C:\Users\Executive\Desktop\MyProject.exe/../data.json`
- **Format:** UTF-8 JSON text
- **Size:** Typical <5MB (supports ~10,000 items)
- **Permissions:** Read-only recommended (prevents accidental overwrites)
- **Lifecycle:** Created manually by PM; versioned in Git; updated externally

**Logs**
- **Path:** `{IHostEnvironment.ContentRootPath}/logs/error.log`
- **Format:** Text (ISO8601 timestamp + log level + message)
- **Size:** Rolling daily (max 10 files; ~50MB total)
- **Retention:** 10 days (oldest files deleted automatically)
- **Permissions:** Read/write by app; readable by admin

**Temporary Files**
- **Cache:** IMemoryCache (in-memory; no disk writes)
- **Session State:** Blazor SignalR (in-memory; no persistence across restarts)

---

### System Requirements

**Minimum Hardware**
- CPU: 2+ cores (minimal; 1 core sufficient for dashboard)
- RAM: 500MB available (typical app footprint <300MB)
- Disk: 200MB free (executable + data.json)
- Display: 1366x768 minimum (responsive design adapts to smaller screens)

**Supported Operating Systems**
- Windows 10 (22H2 or later)
- Windows 11
- Windows Server 2019+ (for network share deployment)

**Browser Compatibility**
- Chromium-based: Chrome, Edge, Brave (recommended)
- Firefox 90+ (supported)
- Safari 14+ (supported)
- IE 11: Not supported (.NET 8 requires modern browser)

---

### Self-Contained Executable

**Build Command**
```bash
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
```

**Output**
- Executable: `publish/MyProject.exe` (~180MB)
- Runtime files: `publish/*.dll` (included in executable package)
- No external dependencies required

**Size Breakdown**
- .NET 8 Runtime: ~130MB (includes CoreCLR, CoreFX, GC)
- Application Code: ~2MB
- Dependencies (MudBlazor, ApexCharts, etc.): ~48MB
- Total: ~180MB

**Optional Optimization**
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishTrimmed=true -o ./publish
```
- Removes unused code; reduces size to ~150MB
- Trade-off: Slightly longer startup time; minimal runtime overhead

---

### CI/CD Pipeline

**Build**
```bash
dotnet build MyProject.sln -c Debug
```
- Output: `bin/Debug/net8.0/` (dev build for testing)

**Test**
```bash
dotnet test MyProject.Tests/MyProject.Tests.csproj -c Debug
```
- Framework: xUnit 2.x
- Target: >80% code coverage (DataProvider, validation, component rendering)
- Execution: Parallel (xUnit default)

**Publish (Release)**
```bash
dotnet publish MyProject.sln -c Release -r win-x64 --self-contained -o ./publish
```
- Output: `publish/MyProject.exe`
- Artifacts: Copy `publish/` to distribution location

**Deployment**
- Manual: PM downloads `MyProject.exe` + `data.json`; runs locally or shares via network
- CI/CD Integration: GitHub Actions or Azure Pipelines (optional)
  - Trigger: Git commit to main branch
  - Build: `dotnet build -c Release`
  - Test: `dotnet test -c Release`
  - Publish: `dotnet publish -c Release -r win-x64 --self-contained`
  - Artifact: Upload `publish/` to GitHub Releases

---

### Monitoring & Logging

**Log Level (Production)**
- Level: Error (only failures logged)
- Console: Disabled (execs have no console access)
- File: `./logs/error.log` (local file)

**Log Format**
```
[2026-04-07T19:51:02.145Z] ERROR | DataProvider | FileNotFoundException: data.json not found at C:\path
[2026-04-07T19:52:15.832Z] ERROR | FileSystemWatcher | UnauthorizedAccessException: Network path unavailable
[2026-04-07T19:53:22.501Z] ERROR | DashboardLayout | ValidationException: Schema validation failed
```

**Log Retention**
- Rolling daily: New file each day (error.log.YYYY-MM-DD)
- Max files: 10 (oldest deleted)
- Max total size: 50MB

**Diagnostics**
- Windows Event Viewer: Application errors (if app crashes)
- Task Manager: Memory usage, CPU; verify app process running
- No Application Performance Monitoring (APM) required (local-only)

---

### Disaster Recovery

**Data Backup**
- **Responsibility:** PM team (not automated)
- **Strategy:** Git version control + manual export to CSV/archive
- **Frequency:** Weekly recommended
- **Recovery:** Restore from Git history; re-run app with previous data.json

**Executable Backup**
- **Responsibility:** Distribution team
- **Strategy:** Publish releases to GitHub Releases or internal file share
- **Frequency:** Each production release (milestone or quarterly)
- **Recovery:** Download previous release .exe if current version fails

---

## Technology Stack Decisions

### Core Framework

**Choice: Blazor Server (.NET 8)**

**Rationale:**
- Server-side rendering eliminates JavaScript complexity for dashboard logic
- Two-way data binding simplifies state synchronization between components
- Native C# enables code reuse (models, validation, business logic)
- Built-in Dependency Injection integrates with .NET ecosystem
- WebSocket-based SignalR provides real-time updates (file watch events)
- Excellent for read-heavy, low-latency dashboards

**Alternatives Evaluated:**
1. **Blazor WASM (Client-Side)** - Eliminated: Bundle size (~3-5MB), offline-first complexity unneeded
2. **ASP.NET MVC + JavaScript** - Eliminated: More boilerplate; manual data binding; slower iteration
3. **Electron (Node.js)** - Eliminated: Not .NET; unsupported stack per requirements

---

### JSON Serialization

**Choice: System.Text.Json (Built-in .NET 8)**

**Rationale:**
- Zero external dependencies (faster startup, smaller executable)
- 6-10x faster than Newtonsoft.Json (measured benchmark)
- Native .NET 8 optimization; actively maintained by Microsoft
- Async-friendly APIs for non-blocking I/O
- SourceGenerator support enables compile-time serialization (Phase 2+)

**Alternatives Evaluated:**
1. **Newtonsoft.Json (Json.NET)** - Eliminated: Slower; legacy; unnecessary overhead
2. **Protobuf** - Eliminated: Binary format; PM team edits JSON, not protobuf
3. **YAML** - Eliminated: PM comfort with JSON format; no added benefit

---

### Schema Validation

**Choice: JsonSchema.NET v6.x**

**Rationale:**
- Lightweight validation against JSON Schema Draft-7 standard
- Declarative schema (separate from code); easy to maintain
- Human-readable error messages with field paths
- Catches structural errors early (missing fields, wrong types)
- No third-party dependencies beyond JsonSchema.NET

**Alternatives Evaluated:**
1. **Manual Validation (if/throw pattern)** - Eliminated: Error-prone; brittle; hard to maintain at scale
2. **Fluent Validation (library)** - Eliminated: Overkill for simple JSON; JSON Schema more appropriate
3. **Runtime Type Checking** - Eliminated: Catches errors too late (after deserialization)

---

### In-Memory Caching

**Choice: IMemoryCache (.NET Built-in)**

**Rationale:**
- Integrated with .NET Dependency Injection; no setup required
- Thread-safe; handles concurrent access automatically
- Configurable expiration (sliding TTL, absolute expiration)
- Sufficient for single-user, local-only dashboard
- No external cache server needed (Redis eliminated)

**Caching Strategy:**
- TTL: 5 minutes (sliding expiration on cache hit)
- Cache key: "dashboard_data" (single key; simple scheme)
- Invalidation: FileSystemWatcher triggers cache eviction on file change
- Fallback: If cache miss and file read fails, retain previous cached version

---

### File Monitoring

**Choice: FileSystemWatcher (.NET Built-in)**

**Rationale:**
- Native .NET API; no external dependencies
- Event-driven (responds to file changes without polling)
- Configurable debounce; prevents false-positive triggers
- Works on local paths and network shares
- Proven reliability in production systems

**Implementation:**
```csharp
_watcher = new FileSystemWatcher(Path.GetDirectoryName(dataPath))
{
    Filter = "data.json",
    NotifyFilter = NotifyFilters.LastWrite
};
_watcher.Changed += (s, e) => {
    // Debounce 500ms
    // Validate file before reloading
    // Re-fetch data; raise DataChanged event
};
_watcher.EnableRaisingEvents = true;
```

**Alternatives Evaluated:**
1. **Polling (Timer)** - Eliminated: Less efficient; more CPU overhead
2. **Gapless File Hash Monitoring** - Eliminated: Unnecessary complexity
3. **File.SystemWatcher from Microsoft.Extensions.FileProviders** - Viable alternative; decided on built-in for simplicity

---

### UI Components & Styling

**Choice: Bootstrap 5.3.x + MudBlazor 6.x + Radzen Blazor 5.x**

**Rationale:**
- **Bootstrap 5.3.x**: Industry-standard responsive grid; excellent print/screenshot support; ships with Blazor template
- **MudBlazor 6.x**: Material Design components; excellent DataGrid with built-in sort/filter; modern UX
- **Radzen Blazor 5.x**: Dashboard layout components; optional (lightweight import)

**Styling Strategy:**
- Base: Bootstrap 5.3.x (grid, typography, responsive breakpoints)
- Components: MudBlazor DataGrid, status badges, cards
- Customization: dashboard.css (brand colors, spacing, print optimizations)
- Icons: FontAwesome 6.x (status icons, priority indicators)

**Print/Screenshot CSS:**
```css
@media print {
  .no-print { display: none; }
  .dashboard-container { page-break-inside: avoid; }
  body { background: white; font-size: 12px; }
}
```

**Alternatives Evaluated:**
1. **Tailwind CSS** - Viable; smaller final bundle (~8KB vs 30KB); decided on Bootstrap for ecosystem and Blazor integration
2. **Custom CSS** - Eliminated: Maintenance burden; no design system
3. **Semantic UI** - Eliminated: Unmaintained

---

### Charting Library

**Choice: ApexCharts.Razor v2.5.x**

**Rationale:**
- SVG-based rendering produces pixel-perfect timelines for PowerPoint screenshots
- Gantt chart support (horizontal bars, date ranges, status colors)
- Interactive legend; zoom/pan capabilities
- Lightweight bundle; no heavy dependencies (Plotly is heavier)
- Active community; regular updates

**Gantt Configuration:**
```csharp
var options = new ApexCharts.ApexChartOptions<TimelineData>()
{
    Chart = new Chart { Type = ChartType.Bar },
    PlotOptions = new PlotOptions
    {
        Bar = new PlotOptionsBar { Horizontal = true }
    },
    Xaxis = new XAxis { Type = XAxisType.Datetime }
};
```

**Alternatives Evaluated:**
1. **Plotly.NET** - Viable; heavier bundle; overkill for simple timelines
2. **D3.js Custom Rendering** - Eliminated: High dev cost; unnecessary complexity
3. **HTML Table with CSS** - Eliminated: Not a visualization; poor executive UX

---

### Testing Framework

**Choice: xUnit 2.x + Bunit 1.x + Moq 4.x + FluentAssertions 6.x**

**Rationale:**
- **xUnit 2.x**: Lightweight, parallel-safe, .NET standard
- **Bunit 1.x**: Blazor-native component testing; render components in isolation
- **Moq 4.x**: Dependency mocking (IHostEnvironment, FileSystemWatcher); simple API
- **FluentAssertions 6.x**: Readable assertions; excellent error messages

**Test Coverage Target:** >80% (DataProvider, validation, component rendering)

**Example Test:**
```csharp
[Fact]
public async Task DataProvider_LoadsValidJson_ReturnsDashboardData()
{
    var provider = new DataProvider(_env.Object, _cache, _validator, _logger);
    var data = await provider.GetDashboardDataAsync();
    
    data.Should().NotBeNull();
    data.ProjectName.Should().Be("Test Project");
    data.Items.Should().HaveCount(3);
}
```

**Alternatives Evaluated:**
1. **NUnit** - Viable; no advantage over xUnit
2. **Selenium/Playwright E2E** - Eliminated: Over-engineered for local dashboards; Bunit sufficient for component testing
3. **No Testing** - Rejected: Unacceptable risk for executive-facing tool

---

### Logging

**Choice: ILogger (Built-in .NET Dependency Injection)**

**Rationale:**
- Integrated with .NET 8 DI container; no additional configuration
- Flexible provider model (console, file, cloud) without code changes
- Structured logging support (key-value pairs)
- Minimal performance overhead

**Configuration (appsettings.json):**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Error",
      "Microsoft": "Error"
    },
    "File": {
      "Path": "./logs/error.log",
      "IncludeScopes": true,
      "MaxFileSize": 10485760,
      "MaxRetainedFiles": 10
    }
  }
}
```

---

### Deployment & Distribution

**Choice: Self-Contained Executable (dotnet publish -r win-x64 --self-contained)**

**Rationale:**
- Single .exe file; no .NET runtime prerequisite
- Drop-and-run deployment; execs double-click to launch
- Portable (local disk, USB drive, network share)
- Size (~180MB) acceptable for local distribution

**Build Command:**
```bash
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
```

**Distribution:**
1. ZIP publish folder (optional compression)
2. Place MyProject.exe + data.json in same directory
3. Share via email, network share, or GitHub Releases
4. Exec runs: `MyProject.exe`

---

## Security Considerations

### Authentication & Authorization

**Status: Not Implemented**

**Rationale:**
- Scope explicitly "local-only, single-user, read-only"
- No multi-user access; no role-based permissions needed
- No external API access; no credential management required
- Authentication layer would add complexity without benefit

**Future Consideration (Phase 2+):**
- If multi-exec access becomes requirement, add Windows Auth (integrated security)
- No external identity provider (AAD, Okta) for local-only deployment

---

### Data Protection

**In-Transit:** No transmission.
- All processing local; no network calls
- HTTPS localhost (self-signed cert; browser trust required once per machine)

**At-Rest:** Plaintext JSON.
- data.json stored as readable text on local disk
- **Recommendation:** Set file permissions to read-only (prevents accidental overwrites)
- **Future Enhancement:** If network share deployment, enable Windows BitLocker or NTFS encryption

**Logging:** No sensitive data.
- Error logs contain file paths, exception messages, validation errors
- No PII, credentials, or sensitive business logic logged
- Log files (./logs/error.log) readable by app owner only

**Backup:** Git version control.
- Repository contributors authenticated via GitHub/GitLab
- Commits reviewed before merge to main
- No unencrypted secrets in commits

---

### Input Validation

**JSON Schema Validation (Mandatory)**

**Enforced Constraints:**
- `projectName`: 1-100 chars, non-null, UTF-8 string
- `timeline`: Array with min 1 milestone
- `items`: Array with min 1 item
- `dueDate`: ISO8601 datetime format (strict parsing)
- `status`: Enum only (NotStarted|InProgress|Shipped)
- `priority`: Enum only (Low|Medium|High)

**Error Handling:**
- Invalid JSON: Display "Invalid JSON format" + line number
- Schema violation: Display "Field X is required" or "Invalid enum value Y"
- Type mismatch: Display "Expected type Z; got type W"
- No stack traces displayed to user (logged to file)

**Injection Prevention:**
- HTML/JavaScript injection: MudBlazor DataGrid automatically escapes content
- SQL injection: N/A (no database; JSON only)
- File path traversal: ContentRootPath restricts access to app root only

**File Access Security:**
- data.json path validated: Path.Combine(ContentRootPath, "data.json")
- No user-provided file paths accepted
- No directory traversal (no "../" navigation)

---

### Secret Management

**No Secrets Required**

- No API keys, tokens, or credentials in code
- No hardcoded connection strings
- No authentication secrets
- No telemetry endpoints

**Future Consideration:**
- If Phase 2 adds cloud features, use appsettings.json + user secrets (built-in .NET feature)

---

### File Permissions

**Recommended Configuration:**

| File | Owner | Permissions | Rationale |
|------|-------|-----------|-----------|
| MyProject.exe | Everyone | Read/Execute | Execs can run; not modify |
| data.json | PM Team | Read-Only | Prevent accidental overwrites |
| logs/error.log | App User | Read/Write | App writes errors; admin reads |

**Windows NTFS:**
```
icacls "C:\path\data.json" /inheritance:r /grant:r "Users:R"
```

---

## Scaling Strategy

### Single-User, Local-Only Constraint

This dashboard is fundamentally designed for single-user, single-machine deployment. **Horizontal scaling (multiple machines) and vertical scaling (multi-instance load balancing) are out of scope.**

---

### Performance Bottlenecks & Mitigations

#### Bottleneck 1: File I/O (data.json Read)
**Issue:** Large JSON files (>5MB) block Blazor Server threads during async file read.

**Mitigation:**
- Async File.ReadAllTextAsync (non-blocking; frees thread pool)
- IMemoryCache with 5-min sliding expiration (95%+ cache hit rate expected)
- FileSystemWatcher invalidates cache on file change (prevents stale reads)

**Performance Target:** <1000ms end-to-end (file read + parse + cache) on typical PC

**Scaling Limit:** Supports up to 10,000 items before noticeable latency

**Future Action:** If >10,000 items, migrate to SQLite backend (Phase 2+)

---

#### Bottleneck 2: JSON Deserialization
**Issue:** System.Text.Json deserialization slower on large JSON objects (100MB+ files).

**Mitigation:**
- Cache deserialized DashboardData object (bypass parsing on cache hit)
- Compile-time SourceGenerator support in .NET 8 (faster serialization, Phase 2+)
- Profile deserialization time in Phase 2; optimize if >500ms

**Performance Target:** <500ms deserialization for 10,000-item JSON

**Scaling Limit:** Typical project <5MB JSON = <500ms parse

---

#### Bottleneck 3: Client-Side Filtering (ItemGridSection)
**Issue:** Filtering 10,000 items via LINQ on every keypress is CPU-bound client-side.

**Mitigation:**
- Debounce filter updates (200ms delay between keystrokes)
- MudBlazor DataGrid efficiently handles 1,000+ rows with virtualization
- Implement pagination (show 50 items/page) if >1,000 items

**Performance Target:** <100ms filter latency (all filtering client-side)

**Scaling Limit:** MudBlazor DataGrid performant up to ~5,000 rows

**Future Action:** Server-side filtering if >10,000 items (Phase 2+)

---

#### Bottleneck 4: Gantt Chart Rendering (ApexCharts)
**Issue:** SVG rendering slows for >200 milestones; browser paint time increases.

**Mitigation:**
- Aggregate minor milestones (hide low-priority ones; show on demand)
- Limit timeline to top 50 active milestones; detail view for drill-down
- Use ApexCharts lazy-loading options if available

**Performance Target:** <2sec render time for 200 milestones

**Scaling Limit:** Acceptable performance up to 200 milestones per dashboard

**Future Action:** Timeline pagination or aggregation (Phase 2)

---

#### Bottleneck 5: Blazor Server SignalR (UI Updates)
**Issue:** Large data cascades trigger expensive DOM diffs on every parameter change.

**Mitigation:**
- Immutable data objects (DashboardData not mutated in-place; reference equality detection)
- Override ComponentBase.ShouldRender() to prevent redundant renders
- Cascading parameters optimized for shallow comparison (Blazor built-in)

**Performance Target:** <100ms re-render latency on data change

**Scaling Limit:** Supports <5MB DashboardData cascade

---

### Caching Strategy

**IMemoryCache (5-min Sliding Expiration)**
- **Cache Key:** "dashboard_data" (single key; simple scheme)
- **Hit Rate Expectation:** >95% (auto-refresh on file change)
- **Eviction:** Automatic on TTL expiration or manual via FileSystemWatcher
- **Size:** In-memory; no disk writes; limited by available RAM

**Behavioral Caching:**
- First access: Cache miss; file I/O + parse + validate (~1000ms)
- Subsequent accesses (within 5 min): Cache hit; return instantly (<10ms)
- File change: Invalidate cache; re-fetch on next access (~1000ms)

**Future Enhancements (Phase 2+):**
- Distributed cache if multi-machine deployment (Redis)
- Search index caching for milestone/item lookups

---

### Load Balancing

**Not Applicable.** Single-user, single-machine deployment. No load balancer needed.

---

### Database Scaling Path (Future)

**If data volume exceeds 10,000 items or real-time sync becomes requirement:**

**Phase 2+ Migration Path:**
1. Replace data.json file I/O with SQLite backend
2. Add Entity Framework Core data access layer
3. Implement query pagination (offset/limit on server)
4. Retain Blazor Server UI; same component hierarchy

**Code Impact:** Minimal
- Swap DataProvider.LoadFromFileAsync()  DataProvider.LoadFromDatabaseAsync()
- All other components unchanged (consume DashboardData model identically)

**Example Migration:**
```csharp
// Phase 1 (current)
public async Task<DashboardData> GetDashboardDataAsync()
{
    var json = await File.ReadAllTextAsync(path);
    return JsonSerializer.Deserialize<DashboardData>(json);
}

// Phase 2 (future)
public async Task<DashboardData> GetDashboardDataAsync()
{
    using var db = new DashboardContext();
    return await db.DashboardData.FirstOrDefaultAsync();
}
```

---

## Risks & Mitigations

### Critical Risks (Severity: High)

#### Risk 1: data.json Corruption
**Scenario:** PM edits data.json in text editor; accidentally deletes required field or mangles JSON syntax. App fails to load; execs blocked from reporting.

**Impact:** 
- Dashboard unavailable; reporting deadline missed
- Potential data loss (corrupted file overwrites valid backup)

**Mitigations:**
1. **Schema validation catches malformed JSON** - JsonSchema.NET rejects invalid structure; error message tells PM what's wrong
2. **Fallback to cached version** - If validation fails, retain last-known-good data; display error but app remains functional
3. **Error message logged + displayed** - User-friendly message shown to PM; technical details logged to file for troubleshooting
4. **Git version control** - Commits tracked; PM can revert to previous version via Git history
5. **Read-only file permissions** - Set data.json to read-only to prevent accidental overwrites

**Monitoring:** Alert PM if data.json validation fails 2+ times

---

#### Risk 2: FileSystemWatcher Failure
**Scenario:** FileSystemWatcher exception occurs (network path unavailable, permission denied). Auto-refresh stops; PM updates data.json but dashboard doesn't reflect changes until app restart.

**Impact:**
- PM thinks changes applied; execs see stale data
- Reporting inconsistency; lost trust in dashboard
- Workaround requires app restart (friction for non-technical execs)

**Mitigations:**
1. **Manual refresh button** - Always provide UI button to force data reload; doesn't depend on file watcher
2. **Exception logging** - Log all FileSystemWatcher exceptions; track reliability metrics
3. **Fallback polling** - If watcher fails, activate 10-sec polling timer (fallback mechanism, Phase 2+)
4. **User notification** - Display banner "File watch unavailable; use manual refresh" if exceptions detected
5. **Graceful error handling** - Catch FileSystemWatcher exceptions; don't crash app

**Code Example:**
```csharp
try
{
    _watcher = new FileSystemWatcher(dirPath) { ... };
    _watcher.EnableRaisingEvents = true;
}
catch (Exception ex)
{
    _logger.LogError("FileSystemWatcher failed: {ex}", ex);
    _hasWatcherError = true;
    // Manual refresh button remains available
}
```

---

#### Risk 3: Self-Contained .exe Size (180MB)
**Scenario:** Network latency when PMs share .exe via file share or email. Large download discourages adoption; execs receive file slowly.

**Impact:**
- Adoption friction; execs prefer workarounds (manual PowerPoint slides)
- Support burden (questions about download speed, file corruption)

**Mitigations:**
1. **Pre-compress ZIP** - Reduce to ~70MB via ZIP compression; document expected file size
2. **.NET 8 trimming** - Optional `--self-contained -p:PublishTrimmed=true` reduces to ~150MB (trade-off: longer startup)
3. **Document size upfront** - README clearly states "180MB download"
4. **Provide download link** - Host on GitHub Releases or internal file share (one-time download per machine)
5. **Local copy recommendation** - Encourage PMs to copy .exe locally after first download (speeds subsequent launches)

**Performance Target:** <2sec startup time on typical PC (even with 180MB executable)

---

### Operational Risks (Severity: Medium)

#### Risk 4: Blazor Server Session Timeout (30 min)
**Scenario:** Exec opens dashboard; steps away for 35+ minutes. Blazor Server session times out; browser shows "Connection Lost" banner.

**Impact:**
- Exec must refresh page or restart app
- Dashboard not suitable for long-running presentations (workaround: keep browser active)

**Mitigations:**
1. **Increase SignalR timeout** - Set in Program.cs: `options.ClientTimeoutInterval = TimeSpan.FromMinutes(60)`
2. **Reconnection banner** - Display "Connection Lost" + "Retry" button; auto-retry every 5 sec
3. **Document expected behavior** - README notes "Dashboard suitable for 1-hour executive reviews; refresh if session timeout"
4. **Stateless architecture** - No session state lost; refresh page re-loads data from cache

**Code Example:**
```csharp
builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(60);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});
```

---

#### Risk 5: data.json Edited While App Running
**Scenario:** PM saves data.json to network share while app has file locked. FileSystemWatcher detects change; app attempts to reload; file read fails due to lock.

**Impact:**
- Temporary reload failure; error displayed; cached data retained
- PM not aware of reload failure; thinks changes applied

**Mitigations:**
1. **Debounce file change events** - 500ms delay ensures file lock released before reload attempt
2. **Handle file-in-use exceptions** - Catch IOException; display "File locked; try again in 5 seconds"
3. **Retry mechanism** - Automatically retry reload 3 times with exponential backoff
4. **User notification** - If reload fails, display "Couldn't reload data; ensure data.json isn't open in editor"
5. **Test network scenarios** - Phase 2 validation: edit data.json on network share; verify reload succeeds

---

#### Risk 6: Multiple Instances of .exe on Same Machine
**Scenario:** Exec accidentally launches MyProject.exe twice; two browsers open with same dashboard.

**Impact:**
- Port conflict (both trying to bind :5001) or first instance owns port
- Second instance fails to start; user confusion

**Mitigations:**
1. **OS handles port assignment** - If port 5001 unavailable, OS assigns random high port (automatic)
2. **Auto-open browser URL** - First instance opens browser; second instance shows error (acceptable)
3. **Document expected behavior** - README notes "Each .exe instance runs independently"
4. **Mutex prevention (optional)** - Phase 2+: Single-instance check (prevent duplicate launches)

---

### Dependency Risks (Severity: Low-Medium)

| Dependency | Version | Risk | Severity | Mitigation |
|------------|---------|------|----------|-----------|
| **.NET 8 Runtime** | 8.0+ | LTS support ends Nov 2026; breaking changes in .NET 9 | Low | Lock to .NET 8; plan migration to .NET 9 LTS by Nov 2026 |
| **Blazor Server** | Shipped with .NET 8 | SignalR session timeout bugs; WebSocket stability | Low | Test long sessions (>1 hour); verify reconnection behavior |
| **ApexCharts.Razor** | 2.5.x | Community library; maintenance uncertainty; newer versions may break | Medium | Monitor GitHub issues quarterly; pin version; plan custom SVG fallback if abandoned |
| **MudBlazor** | 6.x | Heavy CSS bundle; slower DataGrid for 10k+ rows | Low | Benchmark filtering performance; consider lightweight alternative if needed |
| **Radzen Blazor** | 5.x | Dependency bloat; optional (can omit if not used) | Low | Use only DashboardLayout; omit unused components; audit bundle size |
| **JsonSchema.NET** | 6.x | Schema validation performance; edge cases on complex schemas | Low | Profile validation time; cache compiled schema object; <500ms target |

**Monitoring Strategy:**
- Monthly: Check NuGet package updates; review release notes for breaking changes
- Quarterly: Test new versions in staging environment before production upgrade
- Post-launch: Set up GitHub Dependabot alerts for security updates

---

### Technical Risks (Severity: Medium)

#### Risk 7: FileSystemWatcher False-Positive Triggers
**Scenario:** File locked by text editor or network delay causes multiple FileSystemWatcher events in rapid succession. Cache invalidated multiple times; excessive re-fetches cause CPU spike.

**Impact:**
- Unnecessary file reads; CPU/disk I/O spike
- Potential for race conditions (cache invalidated between read/parse)

**Mitigations:**
1. **500ms debounce** - Collect events; fire reload only after quiet period
2. **Validate file before reload** - Catch JsonException; don't invalidate cache if parse fails
3. **Fallback to cached version** - If reload fails, retain previous data; user not affected
4. **Logging & metrics** - Track reload frequency; alert if >10 reloads/min (indicates network issue)

**Code Example:**
```csharp
private CancellationTokenSource _debounceToken;
private void OnFileChanged(object sender, FileSystemEventArgs e)
{
    _debounceToken?.Cancel();
    _debounceToken = new CancellationTokenSource();
    
    _ = Task.Delay(500, _debounceToken.Token).ContinueWith(async _ =>
    {
        if (!_debounceToken.Token.IsCancellationRequested)
            await RefreshDataAsync();
    });
}
```

---

#### Risk 8: JSON Deserialization Performance
**Scenario:** Very large data.json (>10MB) takes >2 seconds to deserialize. Dashboard loading spin 2+ seconds; execs perceive app as slow.

**Impact:**
- Poor user experience; low adoption
- Execs prefer manual reporting over slow dashboard

**Mitigations:**
1. **Profile in Phase 2** - Benchmark deserialization time with 10,000-item JSON; optimize if >500ms
2. **Implement pagination** - Don't load all items at once; fetch page-by-page from database (Phase 2+)
3. **Compile-time optimization** - Use System.Text.Json SourceGenerator in .NET 8 (auto-generated serialization code)
4. **Async loading** - Use File.ReadAllTextAsync; don't block Blazor Server threads

**Performance Target:** <1sec total load time (file I/O + parse + validate + cache)

---

#### Risk 9: Blazor Component Re-render Cascades
**Scenario:** DashboardLayout re-fetches data; all child components re-render unnecessarily. Blazor diffs large DashboardData object; performance degrades with 10,000+ items.

**Impact:**
- Re-render latency increases (1-2sec); dashboard feels sluggish
- Browser paint time increases; UI freezes momentarily

**Mitigations:**
1. **Override ShouldRender()** - Prevent redundant renders via shallow comparison
2. **Immutable data** - Never mutate DashboardData in-place; always create new instance
3. **Component memoization** - Cache child component output if data unchanged
4. **Lazy rendering** - Virtualize long lists; render visible items only

**Code Example:**
```csharp
public partial class TimelineSection : ComponentBase
{
    private DashboardData _lastData;
    
    [CascadingParameter]
    public DashboardData Data { get; set; }
    
    public override bool ShouldRender()
    {
        return Data != _lastData; // Shallow comparison sufficient
    }
    
    protected override void OnParametersSet()
    {
        _lastData = Data;
    }
}
```

---

#### Risk 10: Memory Leak in FileSystemWatcher
**Scenario:** App runs for 24+ hours; IMemoryCache and FileSystemWatcher not properly disposed. Memory usage grows unbounded; app crashes with OutOfMemoryException.

**Impact:**
- App unavailable; execs can't access dashboard
- Data loss if unsaved changes in-flight

**Mitigations:**
1. **Dispose FileSystemWatcher** - Implement IAsyncDisposable; clean up on app shutdown
2. **IMemoryCache TTL** - Automatic eviction on 5-min expiration (no manual cleanup needed)
3. **Memory monitoring** - Windows Task Manager shows memory usage; set alert if >500MB
4. **Graceful shutdown** - Blazor Server handles process exit; cleanup on IDisposable

**Code Example:**
```csharp
public void Dispose()
{
    _watcher?.Dispose();
    _cache?.Remove("dashboard_data");
}
```

---

### User Experience Risks (Severity: Low-Medium)

| Risk | Mitigation |
|------|-----------|
| **Screenshot renders poorly for PowerPoint** | Test screenshot export in Phase 1; verify responsive CSS; validate color contrast (WCAG AA); print preview before embedding |
| **data.json update not visible after 2 seconds** | Document expected latency (file watch + Blazor re-render ~1-2 sec); provide manual refresh button; display "Loading..." indicator |
| **Execs close browser; lose dashboard** | Stateless app; no session persistence needed (re-open executable to restart); document in README |
| **Error message unclear** | Use user-friendly messages ("'projectName' field is required") not stack traces; log full details to ./logs/error.log |
| **File permissions prevent app from reading data.json** | Document read-only recommendation; guide exec through Windows file permissions setup (README) |
| **Gantt chart text unreadable on small display** | Responsive design scales font; test on 1366x768 minimum resolution; provide zoom/pan controls (ApexCharts built-in) |

---

### Business Risks (Severity: Low)

| Risk | Mitigation |
|------|-----------|
| **PM team doesn't understand JSON schema; creates invalid data.json** | Provide example data.json template; schema documentation in README; validation error messages guide correction |
| **Execs forget to backup data.json before edits** | Recommend Git-based versioning; provide Git-to-CSV export script (Phase 2); document backup process in README |
| **Dashboard becomes dependency; no owner post-launch** | Define SLA in README: weekly check-in; log all errors; assign admin; establish change control process |
| **Feature requests accumulate (PDF export, Jira sync, etc.)** | Document scope explicitly; defer to Phase 2; establish change control process; communicate "MVP scope" clearly |
| **Execs expect real-time data sync from external sources** | Clarify "local-only, file-based" in Phase 1 kickoff; discuss integration requirements with stakeholders early |

---

## Conclusion

**My Project** is a well-scoped, technically sound executive dashboard built on proven .NET 8 technologies. The architecture prioritizes **simplicity and accessibility** over feature completeness, enabling executives to generate professional reporting dashboards with a double-click.

**Key Architectural Strengths:**
1. **Local-only deployment** eliminates cloud infrastructure complexity
2. **File-based data model** enables rapid iteration without developer intervention
3. **Blazor Server + MudBlazor + ApexCharts** provide modern UI with minimal boilerplate
4. **Self-contained executable** removes .NET runtime prerequisite
5. **Comprehensive error handling & logging** ensures reliability for non-technical users

**Risk Mitigation:** All critical risks (data corruption, file watch failure, executable size) have concrete mitigations; monitoring strategy defined for Phase 2+.

**Scaling Path:** Architecture supports migration to SQLite backend if data volume exceeds 10,000 items; component hierarchy unchanged.

**Success Criteria:** Executive adoption of dashboard for 2+ reporting cycles without critical issues.