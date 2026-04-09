# Architecture

## Overview & Goals

### Purpose

The Executive Reporting Dashboard is a single-page, locally-hosted Blazor Server application (.NET 8) that visualizes project milestones, progress, and status for executive stakeholders. The dashboard reads data from a JSON file (data.json), enables real-time auto-refresh via FileSystemWatcher, and produces clean, PowerPoint-ready screenshots. Deployed as a self-contained .exe requiring zero IT involvement, this tool reduces manual status reporting overhead by allowing executives to edit metrics directly in their preferred text editors.

### Architecture Type

Layered monolithic application (no microservices, no cloud dependencies). Four distinct layers:
- **Presentation Layer:** Blazor Server components + Chart.js + Bootstrap
- **Service Layer:** Data loading, file watching, validation
- **Data Layer:** System.Text.Json deserialization, POCO models
- **Infrastructure Layer:** FileSystemWatcher, Kestrel web server

### Core Design Principles

1. **Simplicity over features** – Single-page layout, flat JSON schema, no database, no authentication
2. **Zero external dependencies** – System.Text.Json, FileSystemWatcher, Bootstrap CDN
3. **CEO-first design** – Edit data.json directly in text editors; auto-refresh on save
4. **Screenshot-optimized** – Print CSS, viewport constraints, color-coded status for PowerPoint
5. **One-click deployment** – Self-contained .exe, no IT setup, no .NET runtime pre-install
6. **Event-driven architecture** – FileSystemWatcher triggers UI refresh, no polling overhead
7. **Fail-fast validation** – Schema errors displayed in red banner, specific guidance for fix
8. **Desktop-first** – Embedded Kestrel, localhost only, no cloud dependencies
9. **Test-driven for data** – 80% unit test coverage for JSON validation, 70% component tests
10. **Transparent operations** – Logged refresh timestamps, error messages, console debugging

### Goals Alignment

**Business Goals:**
- ✓ Simplify executive visibility: Single-page dashboard displays all metrics at a glance
- ✓ Enable fast PowerPoint deck creation: Native browser screenshots for presentations
- ✓ Minimize friction for non-technical executives: One-click .exe installation
- ✓ Reduce manual status reporting overhead: Executives edit JSON directly

**Technical Goals:**
- ✓ Build with mandatory tech stack: C# .NET 8 with Blazor Server, local-only
- ✓ No cloud services: All functionality self-contained on local machine
- ✓ Zero external dependencies: Only built-in .NET libraries in critical path
- ✓ Cold start <5 seconds: Embedded Kestrel + minimal initialization
- ✓ Data refresh <1 second: FileSystemWatcher + debounce
- ✓ File size <150MB: Self-contained .exe acceptable for corporate intranet

---

## System Components

### Layer 1: Data Models (Immutable POCOs)

#### ProjectReport
**Responsibility:** Root aggregate representing complete project reporting snapshot. Single source of truth.

**Ownership:**
- `ProjectName`: string (non-empty, max 100 chars)
- `ReportingPeriod`: string (ISO 8601 or human-readable, required)
- `Milestones`: Milestone[] (1 to 100+ milestones, required)
- `StatusSnapshot`: StatusSnapshot (nested object, required)
- `Kpis`: Dictionary<string, int> (key-value KPI metrics, 0-100%, optional)

**Storage:** JSON file (data.json), UTF-8, <50MB max

**Used By:** DataLoaderService, Index.razor, DataValidator

#### Milestone
**Responsibility:** Individual project milestone with timeline and progress tracking.

**Ownership:**
- `Id`: string (unique within array, max 50 chars)
- `Name`: string (title, max 100 chars)
- `TargetDate`: string (ISO 8601 format YYYY-MM-DD)
- `Status`: string (enum: "on-track", "at-risk", "delayed", "completed")
- `Progress`: int (percentage 0-100)
- `Description`: string (optional narrative, max 500 chars)

**Constraints:**
- Id unique within Milestones array
- TargetDate valid ISO 8601
- Progress 0-100
- Status exact match (case-sensitive lowercase)

**Used By:** MilestoneTimeline component, DataValidator

#### StatusSnapshot
**Responsibility:** Three-bucket categorization of delivery status.

**Ownership:**
- `Shipped`: string[] (completed deliverables, can be empty)
- `InProgress`: string[] (active work items, can be empty)
- `CarriedOver`: string[] (deferred items from prior period, can be empty)

**Constraints:**
- Arrays must not be null (can be empty)
- Each string item: non-empty, max 200 chars, no newlines

**Used By:** StatusSnapshot component, DataValidator

---

### Layer 2: Services (Business Logic & I/O)

#### DataLoaderService
**Responsibility:** Load, deserialize, and parse JSON file into ProjectReport POCO. Single responsibility: JSON I/O and deserialization. No file watching, no validation.

**Lifetime:** Scoped (per HTTP request in Blazor Server context)

**Public Interface:**
```csharp
public interface IDataLoaderService
{
    Task<ProjectReport> LoadAsync(string dataPath = null);
    string GetConfiguredDataPath();
}
```

**Implementation Details:**
- Resolve dataPath from parameter, config, or default "./data.json"
- Check file exists; throw FileNotFoundException if not
- Read file content asynchronously
- Deserialize via System.Text.Json with PropertyNameCaseInsensitive
- Return ProjectReport or throw JsonException if malformed
- Log all operations at INFO level

**Dependencies:**
- IConfiguration (read appsettings.json)
- System.Text.Json (JSON deserialization)
- System.IO (file I/O)
- ILogger (logging)

**Error Handling:**
- FileNotFoundException: Data file not found at path
- JsonException: JSON parse error (propagate to caller)
- IOException: File access denied or in-use

#### DataWatcherService
**Responsibility:** Monitor data.json file for changes. Debounce rapid file writes. Emit events to trigger UI refresh. Pure file monitoring, no data parsing or validation.

**Lifetime:** Singleton (one monitor per app lifetime, shared across all components)

**Public Interface:**
```csharp
public interface IDataWatcherService : IDisposable, IAsyncDisposable
{
    event Func<Task> OnDataChanged;
    void Start(string dataPath = null);
    void Stop();
    DateTime LastRefreshTime { get; }
    string LastRefreshTimeFormatted { get; }
}
```

**Implementation Details:**
- Initialize FileSystemWatcher on data.json directory
- Subscribe to Changed event
- Apply 500ms debounce timer (collapse rapid writes into single event)
- Fire OnDataChanged event on main Blazor Server thread
- Update LastRefreshTime on successful refresh
- Graceful degradation: Log warnings on FSW errors, do not crash

**Dependencies:**
- System.IO.FileSystemWatching (FileSystemWatcher)
- System.Timers or System.Threading.Timer (debounce logic)
- IConfiguration (read dataPath and debounce interval)
- ILogger (log monitoring events)

**Data Owned:**
- FileSystemWatcher instance (enabled/disabled)
- Debounce timer state
- LastRefreshTime timestamp

**Error Handling:**
- Graceful degradation: If FSW fails, log WARNING; app continues
- Do NOT crash or throw on file monitor errors
- No retry logic (FSW self-recovers on next file write)

**Thread Safety:**
- OnDataChanged event fired on main Blazor Server thread (thread-safe)
- System.Timers.Timer is thread-safe
- File reads are non-blocking

#### DataValidator
**Responsibility:** Validate ProjectReport schema. Check required fields, enum values, data types, ranges. Immutable validation logic.

**Lifetime:** Singleton (stateless)

**Public Interface:**
```csharp
public interface IDataValidator
{
    ValidationResult Validate(ProjectReport report);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
}

public class ValidationError
{
    public string Field { get; set; }  // "projectName", "milestones[0].status", etc.
    public string Message { get; set; } // User-friendly error message
}
```

**Validation Rules:**
- projectName: required, non-empty, max 100 chars
- reportingPeriod: required, non-empty, max 50 chars
- milestones: required array, min length 1
- For each milestone:
  - id: required, unique within array, max 50 chars
  - name: required, non-empty, max 100 chars
  - targetDate: required, valid ISO 8601 (YYYY-MM-DD)
  - status: required, one of [on-track, at-risk, delayed, completed]
  - progress: required, integer 0-100
  - description: optional, max 500 chars
- statusSnapshot: required object
  - shipped, inProgress, carriedOver: arrays (can be empty)
- kpis: optional dictionary of metric names to percentages (0-100)

**Implementation Details:**
- Create ValidationResult with IsValid=true initially
- Check each field, accumulate errors (do not fail-fast)
- Return ValidationResult with IsValid flag and Errors collection
- Log warnings if validation fails

**Dependencies:**
- System.Globalization (date parsing)
- ILogger (logging)

**Error Handling:**
- Do NOT throw exceptions; return ValidationResult with IsValid=false
- Accumulate all errors in single pass
- Each ValidationError specifies field path and user-friendly message

---

### Layer 3: Blazor Components (UI & State Management)

#### Index.razor
**Responsibility:** Root orchestration component. Initialize app, subscribe to file watcher, manage parent state, coordinate child components. Central hub for data flow.

**Ownership:**
- ProjectReport state (current loaded data)
- ValidationResult state (current validation errors)
- LastRefreshTime (from DataWatcherService)
- isLoading state (busy during JSON load)

**Lifecycle:**
- OnInitializedAsync:
  1. Subscribe to DataWatcherService.OnDataChanged event
  2. Load initial data via DataLoaderService.LoadAsync()
  3. Validate via DataValidator.Validate()
  4. Start DataWatcherService.Start()
  5. Set state variables
  6. Call StateHasChanged()

- OnDataChanged event handler (fired by file watcher):
  1. Set isLoading = true
  2. Reload JSON via DataLoaderService.LoadAsync()
  3. Validate schema
  4. Update component state (projectReport, validationResult, lastRefreshTime)
  5. Call StateHasChanged()  triggers re-render
  6. Set isLoading = false

**Render:**
- If loading: Show spinner
- If validation failed: Red banner with error messages
- If valid:
  - Render ProjectMetadata component (cascading: projectName, period, kpis)
  - Render MilestoneTimeline component (cascading: milestones[])
  - Render StatusSnapshot component (cascading: shipped[], inProgress[], carriedOver[])
  - Render refresh timestamp: "Data refreshed at HH:mm:ss"

**Cascading Parameters Provided:**
- ProjectReport CurrentReport
- string ProjectName
- string ReportingPeriod
- Dictionary<string, int> Kpis
- Milestone[] Milestones
- StatusSnapshot CurrentStatus
- DateTime LastRefreshTime
- bool HasErrors

**Dependencies:**
- DataWatcherService (singleton): Subscribe to OnDataChanged, access LastRefreshTime
- DataLoaderService (scoped): LoadAsync() method
- DataValidator (singleton): Validate() method
- ILogger: Log data loads, validation errors

**Error Handling:**
- Try/catch DataLoaderService exceptions
- Try/catch DataValidator validation failures
- Display red error banner with specific messages
- Allow user to edit data.json and retry

#### MilestoneTimeline.razor
**Responsibility:** Render milestone timeline as horizontal bar chart via Chart.js. Pure presentation, no business logic.

**Cascading Parameters (Inbound):**
- Milestone[] Milestones
- DateTime LastRefreshTime (optional)

**Ownership:**
- Canvas HTML element (id="milestoneChart")
- Chart.js instance reference (in JavaScript)
- Milestone array rendering logic

**Lifecycle:**
- OnAfterRenderAsync:
  1. If first render OR Milestones array changed:
     - Call IJSRuntime.InvokeAsync("initMilestoneChart", milestones)
     - JS initializes Chart.js on canvas
     - Pass milestone data (name, progress, status, targetDate)
     - JS applies color coding per status
  2. If re-render and Milestones unchanged: No-op

**Render:**
```html
<div class="timeline-container">
  <canvas id="milestoneChart" style="max-height: 400px;"></canvas>
</div>
```

**Chart.js Configuration:**
- Type: 'bar'
- IndexAxis: 'y' (horizontal bars)
- Data labels: milestone name, progress %, status
- Color scheme per status:
  - on-track: #28a745 (green)
  - at-risk: #ffc107 (yellow)
  - delayed: #dc3545 (red)
  - completed: #6c757d (gray)
- Responsive: true, maintainAspectRatio: false

**Dependencies:**
- Chart.js (CDN, via IJSRuntime)
- IJSRuntime: JS interop
- Milestone model (POCO)

**Print Behavior:**
- CSS: `print-color-adjust: exact` (force color in print preview)
- No page breaks (parent container handles with `page-break-inside: avoid`)

#### StatusSnapshot.razor
**Responsibility:** Render three-column status layout (Shipped | In Progress | Carried Over). Pure presentation.

**Cascading Parameters (Inbound):**
- StatusSnapshot CurrentStatus

**Ownership:**
- Three Bootstrap column divs (col-lg-4 each)
- Status item list rendering logic

**Render:**
```html
<div class="row">
  <div class="col-lg-4">
    <h5>Shipped</h5>
    <div class="list-group">
      @foreach (var item in CurrentStatus.Shipped)
        <div class="list-group-item">@item</div>
    </div>
  </div>
  <!-- Repeat for InProgress, CarriedOver -->
</div>
```

**Dependencies:**
- StatusSnapshot model (POCO)
- Bootstrap CSS (from CDN via App.razor)

**Styling:**
- Bootstrap cards or list-group items
- Responsive grid (col-12 on mobile, col-lg-4 on desktop)
- No wrap/overflow (constrained width)

#### ProjectMetadata.razor
**Responsibility:** Render project header (name, period) and KPI metrics grid. Pure presentation.

**Cascading Parameters (Inbound):**
- string ProjectName
- string ReportingPeriod
- Dictionary<string, int> Kpis

**Ownership:**
- Header section (project name, reporting period)
- KPI metrics grid (responsive 2-4 column layout)

**Render:**
```html
<header class="dashboard-header">
  <h1>@ProjectName</h1>
  <p class="text-muted">@ReportingPeriod</p>
</header>
<div class="kpi-grid row">
  @foreach (var kpi in Kpis)
    <div class="col-md-6 col-lg-3">
      <div class="kpi-card">
        <strong>@kpi.Key</strong>
        <p class="kpi-value">@kpi.Value%</p>
      </div>
    </div>
</div>
```

**Dependencies:**
- ProjectReport model (cascading parameters)
- Bootstrap CSS grid utilities

#### App.razor
**Responsibility:** Root layout. Import CSS/JS (CDN links), configure HeadOutlet and error boundary. Infrastructure only.

**Public Interface:**
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <!-- Bootstrap 5.3 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    
    <!-- Chart.js 4.4.x -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    
    <!-- Custom stylesheets -->
    <link href="app.css" rel="stylesheet">
    <link href="print.css" rel="stylesheet" media="print">
    
    <!-- Chart initialization JS -->
    <script src="chart-init.js"></script>
    
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.server.js"></script>
</body>
</html>
```

**Dependencies:**
- Bootstrap CDN
- Chart.js CDN
- Custom CSS files
- Blazor.Server.js runtime

---

### Layer 4: Infrastructure & Configuration

#### Program.cs
**Responsibility:** Application startup, service registration, Kestrel configuration. Bootstrap the app.

**Service Registration:**
```csharp
// Singletons (app-lifetime)
builder.Services.AddSingleton<DataWatcherService>();
builder.Services.AddSingleton<DataValidator>();

// Scoped (per HTTP request)
builder.Services.AddScoped<DataLoaderService>();

// Built-in services
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddLogging();
```

**Kestrel Configuration:**
- Bind to http://127.0.0.1:5000 (localhost only)
- Port 5000
- No external routing, no TLS

**Auto-Launch Browser:**
- On startup, open default browser to http://localhost:5000

#### appsettings.json
**Purpose:** Application configuration (data path, port, logging levels). Non-code source of truth.

**Structure:**
```json
{
  "AppSettings": {
    "DataPath": "data.json",
    "Port": 5000,
    "DebounceIntervalMs": 500,
    "ProjectName": "Default Project"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Configuration Options:**

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| AppSettings:DataPath | string | data.json | Path to JSON file (relative or absolute) |
| AppSettings:Port | int | 5000 | Kestrel port (localhost only) |
| AppSettings:DebounceIntervalMs | int | 500 | FileSystemWatcher debounce (ms) |
| AppSettings:ProjectName | string | Default Project | Display name in dashboard |
| Logging:LogLevel:Default | string | Information | Log level (Debug, Information, Warning, Error) |

---

## Component Interactions

### Use Case 1: Initial App Load

```
User double-clicks ReportingDashboard.exe
   ↓
Kestrel embedded server starts (localhost:5000)
   ↓
Default browser opens http://localhost:5000
   ↓
Index.razor OnInitializedAsync() called
   │
   ├─→ DataWatcherService.Start(dataPath)
   │   └─→ FileSystemWatcher initialized on data.json directory
   │
   ├─→ DataLoaderService.LoadAsync()
   │   ├─→ Read data.json from disk
   │   └─→ System.Text.Json deserialization → ProjectReport POCO
   │
   ├─→ DataValidator.Validate(projectReport)
   │   ├─→ Check required fields
   │   ├─→ Check milestone enum values
   │   └─→ Return ValidationResult { IsValid, Errors[] }
   │
   └─→ If valid:
       ├─→ Set componentState = { projectReport, validationResult }
       └─→ StateHasChanged() → re-render
       
   └─→ If invalid:
       ├─→ Set componentState = { validationResult with errors }
       └─→ StateHasChanged() → render error banner

Blazor renders Index.razor
   │
   ├─→ If errors: Render red error banner with validation messages
   │
   └─→ If valid:
       ├─→ Render ProjectMetadata (cascading: projectName, period, kpis)
       ├─→ Render MilestoneTimeline (cascading: milestones[])
       │   └─→ OnAfterRenderAsync invokes IJSRuntime.initMilestoneChart()
       │       └─→ JavaScript renders Chart.js horizontal bar chart
       ├─→ Render StatusSnapshot (cascading: shipped[], inProgress[], carriedOver[])
       └─→ Render timestamp: "Data refreshed at HH:mm:ss"

Dashboard visible to executive (3-5 seconds cold start)
```

### Use Case 2: Executive Edits data.json

```
Executive opens data.json in VS Code
   ↓
Executive edits milestone: "progress": 75 → "progress": 85
   ↓
Executive saves file (Ctrl+S)
   ↓
FileSystemWatcher detects LastWrite change event
   ↓
DataWatcherService OnChanged handler fires
   ├─→ Cancel existing debounce timer
   └─→ Start new 500ms debounce timer

(If another save within 500ms: Reset timer)

500ms elapses with no new changes
   ↓
DataWatcherService fires OnDataChanged event
   ↓
Index.razor.OnDataChanged handler fires (async)
   ├─→ Set isLoading = true
   ├─→ StateHasChanged() → show loading spinner
   │
   ├─→ DataLoaderService.LoadAsync()
   │   ├─→ Read updated data.json from disk
   │   └─→ Deserialize to ProjectReport { milestones[0].progress = 85 }
   │
   ├─→ DataValidator.Validate(projectReport)
   │   └─→ Return ValidationResult { IsValid = true }
   │
   ├─→ Update component state
   │   ├─→ projectReport = new instance
   │   ├─→ validationResult = new valid result
   │   ├─→ lastRefreshTime = DateTime.Now
   │   └─→ isLoading = false
   │
   └─→ StateHasChanged() → re-render

Blazor re-renders Index.razor
   │
   ├─→ MilestoneTimeline receives updated milestones[] cascading parameter
   │   ├─→ Detects array change (progress changed)
   │   └─→ OnAfterRenderAsync calls IJSRuntime to reinitialize Chart.js
   │       └─→ JavaScript updates canvas: milestone progress bar now 85%
   │
   ├─→ StatusSnapshot receives updated statusSnapshot (if changed)
   │
   └─→ Refresh timestamp updates: "Data refreshed at 14:32:45"

Executive sees updated dashboard within 1 second
```

### Use Case 3: Malformed JSON Edit

```
Executive edits data.json with syntax error
   ↓
Executive saves file
   ↓
FileSystemWatcher → debounce 500ms → OnDataChanged fires
   ↓
Index.razor.OnDataChanged fires
   │
   └─→ DataLoaderService.LoadAsync()
       ├─→ Read data.json (contains syntax error)
       ├─→ System.Text.Json.Deserialize() throws JsonException
       └─→ Catch exception in Index.razor handler

Create ValidationResult { IsValid = false, Errors = [...] }
   ↓
Update component state = { validationResult with errors }
   ↓
StateHasChanged() → re-render
   ↓
Blazor renders Index.razor
   │
   └─→ Detect validationResult.IsValid == false
       │
       └─→ Render red error banner:
           "ERROR: Invalid JSON"
           "Details: Unexpected end of JSON input near line 45"
           "Action: Fix data.json and save again"

Executive sees error banner with guidance
   ↓
Executive fixes JSON syntax, saves again
   ↓
FileSystemWatcher → debounce 500ms → OnDataChanged fires
   ↓
Index.razor.OnDataChanged fires
   │
   ├─→ DataLoaderService.LoadAsync() succeeds (JSON now valid)
   ├─→ DataValidator.Validate() succeeds
   └─→ Update state with valid data, StateHasChanged()

Dashboard displays updated data (error banner gone)
```

### Use Case 4: Print / Screenshot Workflow

```
Executive views dashboard, wants to insert into PowerPoint
   ↓
Executive presses Ctrl+P (Print Preview)
   ↓
Browser enters print media context
   ↓
print.css rules apply:
   ├─→ Hide nav, footer, refresh indicator (display: none)
   ├─→ Set body width=8.5in, height=11in, margin=0.5in
   ├─→ Set milestones, status cards: page-break-inside: avoid
   └─→ Canvas (Chart.js): print-color-adjust: exact → force colors

Browser renders dashboard in print layout
   ├─→ MilestoneTimeline canvas renders with exact colors (green/yellow/red)
   ├─→ StatusSnapshot three columns stay together (no wrapping)
   └─→ ProjectMetadata header stays on first page

Executive sees clean, professional print preview
   ↓
Executive either:
   a) Saves as PDF (Ctrl+P → Save as PDF) → Insert PDF into PowerPoint
   b) Takes screenshot (Cmd+Shift+P → Screenshot) → Crop and insert PNG into PowerPoint

PowerPoint slide has professional dashboard visual
```

### Component Communication Matrix

| From | To | Method | Purpose |
|------|----|---------|----|
| Index.razor | DataWatcherService | Subscribe to OnDataChanged event | Trigger reload on file change |
| Index.razor | DataLoaderService | Call LoadAsync() | Load JSON from disk |
| Index.razor | DataValidator | Call Validate(projectReport) | Validate loaded data |
| Index.razor | MilestoneTimeline | Cascading parameter: Milestone[] | Pass milestone data |
| Index.razor | StatusSnapshot | Cascading parameter: StatusSnapshot | Pass status data |
| Index.razor | ProjectMetadata | Cascading parameters: ProjectName, Period, Kpis | Pass metadata |
| MilestoneTimeline | IJSRuntime | InvokeAsync("initMilestoneChart", milestones) | Initialize Chart.js |
| MilestoneTimeline | Chart.js (JS) | Canvas reference + milestone array | Render chart |
| Program.cs | DataWatcherService | Register as Singleton | Create once, share across requests |
| Program.cs | DataLoaderService | Register as Scoped | New instance per HTTP context |
| Program.cs | DataValidator | Register as Singleton | Stateless validation logic |

---

## Data Model

### Entity Definitions & Storage

#### ProjectReport (Root Aggregate)

**Responsibility:** Complete project reporting snapshot. Single source of truth.

**Storage:** JSON file (data.json), UTF-8 encoded

**Fields:**
```json
{
  "projectName": "Project Horizon",
  "reportingPeriod": "2026-Q2",
  "milestones": [ ... ],
  "statusSnapshot": { ... },
  "kpis": {
    "onTimeDelivery": 85,
    "teamCapacity": 92
  }
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| projectName | string | Yes | Non-empty, max 100 chars |
| reportingPeriod | string | Yes | Non-empty, max 50 chars (ISO 8601 or human-readable) |
| milestones | Milestone[] | Yes | Min length 1, sorted by targetDate recommended |
| statusSnapshot | StatusSnapshot | Yes | Nested object |
| kpis | Dictionary<string, int> | No | Key: string, Value: 0-100 percentage |

#### Milestone (Value Object)

**Responsibility:** Individual project milestone with timeline and progress tracking.

**Storage:** Nested in ProjectReport.Milestones array

**Fields:**
```json
{
  "id": "m1",
  "name": "Platform Launch",
  "targetDate": "2026-05-15",
  "status": "on-track",
  "progress": 75,
  "description": "Core platform infrastructure and APIs"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| id | string | Yes | Unique within array, max 50 chars, no spaces, lowercase |
| name | string | Yes | Non-empty, max 100 chars |
| targetDate | string | Yes | ISO 8601 format (YYYY-MM-DD) |
| status | string | Yes | Enum: "on-track", "at-risk", "delayed", "completed" (lowercase) |
| progress | int | Yes | Integer 0-100 |
| description | string | No | Max 500 chars |

#### StatusSnapshot (Value Object)

**Responsibility:** Three-bucket categorization of delivery status.

**Storage:** Nested in ProjectReport.StatusSnapshot

**Fields:**
```json
{
  "shipped": ["Feature X", "Integration Y"],
  "inProgress": ["Feature Z", "Performance optimization"],
  "carriedOver": ["Deferred item A"]
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| shipped | string[] | Yes | Can be empty, each item max 200 chars |
| inProgress | string[] | Yes | Can be empty, each item max 200 chars |
| carriedOver | string[] | Yes | Can be empty, each item max 200 chars |

### Data Relationships & Constraints

**Hierarchy:**
```
ProjectReport (root)
├── Milestones[] (1 to many)
│   ├── id (unique within array)
│   ├── name
│   ├── targetDate
│   ├── status
│   ├── progress
│   └── description (optional)
├── StatusSnapshot (1 to 1)
│   ├── shipped[]
│   ├── inProgress[]
│   └── carriedOver[]
└── Kpis (Dictionary, 0 to many)
    ├── key: string
    └── value: int (0-100)
```

**Referential Integrity:**
- Milestone.id must be unique within Milestones[] array
- No cross-references between entities (flat, non-relational by design)
- No foreign keys

### Storage Strategy

**File System Storage:**
- **File Path:** ./data.json (configurable via appsettings.json)
- **File Format:** UTF-8 JSON text
- **File Size Limit:** <50MB (recommended)
- **Filename Convention:** 
  - Primary: data.json
  - Multi-project (Phase 2): data-{projectName}.json (optional)

**Atomic Writes:**
- File writes are atomic at OS level (NTFS, Windows)
- App does not perform in-place edits (executives edit directly in text editor)
- No concurrent write protection needed (single-user assumption)

**Permissions:**
- Recommend IT set NTFS ACLs on data.json directory
- Restrict read/write to authorized executives only
- Prevent non-executive access

### Validation Rules (Data Contracts)

**Required Fields:**
- projectName: string, non-empty, max 100 chars
- reportingPeriod: string, non-empty, max 50 chars
- milestones: array, min length 1
- statusSnapshot: object (required)
  - shipped: array (can be empty)
  - inProgress: array (can be empty)
  - carriedOver: array (can be empty)

**Milestone Validation:**
- id: string, non-empty, unique within array, max 50 chars
- name: string, non-empty, max 100 chars
- targetDate: string, valid ISO 8601 (YYYY-MM-DD)
- status: string, one of ["on-track", "at-risk", "delayed", "completed"]
- progress: integer, 0 ≤ progress ≤ 100
- description: string, optional, max 500 chars

**KPI Validation:**
- Each key: string, alphanumeric + spaces, max 50 chars
- Each value: integer, 0 ≤ value ≤ 100

**Error Messages (User-Facing):**
```
"Project name is required and cannot be empty."
"Reporting period is required."
"At least one milestone is required."
"Milestone '{id}': Invalid target date format. Use YYYY-MM-DD (e.g., 2026-05-15)."
"Milestone '{id}': Invalid status '{value}'. Must be one of: on-track, at-risk, delayed, completed."
"Milestone '{id}': Progress must be a number between 0 and 100."
"KPI '{key}': Value must be a number between 0 and 100."
```

---

## API Contracts

### Service Interfaces (Blazor Server, No HTTP REST)

Since this is a Blazor Server app (server-side rendered), there are no HTTP REST endpoints. "API Contracts" define Blazor service interfaces and component communication contracts.

#### DataLoaderService Interface

**Responsibility:** Load and deserialize JSON file to ProjectReport.

**Public Methods:**

```csharp
public interface IDataLoaderService
{
    /// <summary>
    /// Load and deserialize data.json file.
    /// </summary>
    /// <param name="dataPath">Optional file path; defaults to appsettings:AppSettings:DataPath</param>
    /// <returns>Deserialized ProjectReport object</returns>
    /// <exception cref="FileNotFoundException">If data.json not found at path</exception>
    /// <exception cref="JsonException">If JSON is malformed or invalid</exception>
    /// <exception cref="IOException">If file cannot be read</exception>
    Task<ProjectReport> LoadAsync(string dataPath = null);

    /// <summary>
    /// Get configured data path from appsettings.json.
    /// </summary>
    /// <returns>Data file path (relative or absolute)</returns>
    string GetConfiguredDataPath();
}
```

**Error Handling:**
- FileNotFoundException: Propagate to caller (Index.razor catches)
- JsonException: Propagate to caller (caught, displayed as error banner)
- IOException: Propagate (rare; file access denied)
- Logs all operations at INFO level

#### DataWatcherService Interface

**Responsibility:** Monitor data.json file and emit events on change.

**Public Interface:**

```csharp
public interface IDataWatcherService : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Event: Fired asynchronously after debounce period when file change detected.
    /// Handler: Func<Task> (async event)
    /// Fired on: Main Blazor Server thread (thread-safe)
    /// </summary>
    event Func<Task> OnDataChanged;

    /// <summary>
    /// Initialize file watcher for specified data path.
    /// </summary>
    /// <param name="dataPath">Path to monitor (e.g., "./data.json")</param>
    /// <remarks>Does not throw; logs errors internally. Graceful degradation if FSW fails.</remarks>
    void Start(string dataPath = null);

    /// <summary>
    /// Stop file watcher and clean up resources.
    /// </summary>
    void Stop();

    /// <summary>
    /// Get timestamp of last successful refresh.
    /// </summary>
    DateTime LastRefreshTime { get; }

    /// <summary>
    /// Get formatted timestamp for UI display (HH:mm:ss).
    /// </summary>
    string LastRefreshTimeFormatted { get; }
}
```

**Event Contract:**
- `OnDataChanged` is `Func<Task>` (async event)
- Fired on main Blazor Server thread (thread-safe)
- Fired after 500ms debounce period with no additional file changes

#### DataValidator Interface

**Responsibility:** Validate ProjectReport schema and return validation result.

**Public Interface:**

```csharp
public interface IDataValidator
{
    /// <summary>
    /// Validate ProjectReport against schema rules.
    /// </summary>
    /// <param name="report">ProjectReport to validate (can be null)</param>
    /// <returns>ValidationResult with IsValid flag and errors collection</returns>
    /// <remarks>Does NOT throw exceptions; returns result object with errors.</remarks>
    ValidationResult Validate(ProjectReport report);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
}

public class ValidationError
{
    public string Field { get; set; }  // "projectName", "milestones[0].status", etc.
    public string Message { get; set; } // User-friendly error message
}
```

**Error Handling Contract:**
- Does NOT throw exceptions
- Returns ValidationResult with empty Errors list if valid
- Returns ValidationResult with populated Errors list if invalid
- Each ValidationError specifies field path and user-friendly message

### Blazor Component Contracts

#### Index.razor Cascading Parameters (Outbound)

```csharp
[CascadingParameter] public ProjectReport CurrentReport { get; set; }
[CascadingParameter] public string ProjectName { get; set; }
[CascadingParameter] public string ReportingPeriod { get; set; }
[CascadingParameter] public Dictionary<string, int> Kpis { get; set; }
[CascadingParameter] public Milestone[] Milestones { get; set; }
[CascadingParameter] public StatusSnapshot CurrentStatus { get; set; }
[CascadingParameter] public DateTime LastRefreshTime { get; set; }
[CascadingParameter] public bool HasErrors { get; set; }
```

#### MilestoneTimeline.razor Cascading Parameters (Inbound)

```csharp
[CascadingParameter] public Milestone[] Milestones { get; set; }
[CascadingParameter(Name = "LastRefreshTime")] public DateTime LastRefreshTime { get; set; }
```

#### JavaScript Interop Contract (Chart.js)

```csharp
// Blazor → JavaScript
await JSRuntime.InvokeAsync<object>("initMilestoneChart", new
{
    milestones = milestones,
    containerId = "milestoneChart",
    colorScheme = new
    {
        onTrack = "#28a745",
        atRisk = "#ffc107",
        delayed = "#dc3545",
        completed = "#6c757d"
    }
});

// JavaScript function signature
window.initMilestoneChart = function(config) {
    // config.milestones: Milestone[]
    // config.containerId: string
    // config.colorScheme: { onTrack, atRisk, delayed, completed }
    
    // Initialize Chart.js horizontal bar chart
    const ctx = document.getElementById(config.containerId).getContext('2d');
    const chart = new Chart(ctx, {
        type: 'bar',
        data: { ... },
        options: { indexAxis: 'y', ... }
    });
}
```

---

## Infrastructure Requirements

### Hosting

**Target Environment:**
- **OS:** Windows 10+ (64-bit, x86-64)
- **Runtime:** .NET 8.0 LTS (bundled in self-contained .exe)
- **Web Server:** Kestrel (embedded, localhost only)
- **Port:** 5000 (http://127.0.0.1:5000)
- **Browser:** Chrome, Edge, Firefox (latest stable)

**Hardware Requirements (Minimum):**
- CPU: 1 GHz or faster (x64 processor)
- RAM: 512 MB minimum, 1 GB recommended
- Disk: 300 MB free (for .exe + sample data.json)
- Network: None required (localhost only); optional CDN access

**Deployment Model:**
- **Target:** Self-contained single-file executable (win-x64)
- **Build Command:** `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true`
- **Output:** Single .exe file (~120-150MB)
- **Distribution:** File share, intranet download, email attachment
- **Installation:** Double-click .exe; app launches Kestrel and opens default browser

---

### Networking

**Local Networking:**
- **Binding:** http://127.0.0.1:5000 (localhost only, non-routable)
- **External Access:** None required
- **Proxies:** None required
- **TLS/HTTPS:** Not implemented (localhost, no encryption requirement)
- **Firewall:** No firewall rules needed

**CDN Dependencies:**
- **Bootstrap 5.3:** https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css
- **Chart.js 4.4.x:** https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js
- **Blazor.Server.js:** Built-in (served from app /framework/blazor.server.js)

**Corporate Network Assumptions:**
- IT allows outbound HTTPS to jsDelivr CDN
- Fallback: If CDN inaccessible, app still loads with minimal styling (graceful degradation)

---

### Storage

**Data Storage:**
- **Location:** Local file system (same machine as .exe)
- **File:** data.json in app working directory (configurable)
- **Filesystem:** NTFS (Windows standard)
- **Permissions:** Recommend NTFS ACLs (restrict to authorized users)
- **Backup:** User-managed (executives backup data.json manually or via folder sync)
- **Encryption:** Not implemented (internal corporate metrics, no PII)

**Configuration Storage:**
- **Location:** appsettings.json (bundled in .exe)
- **Overrides:** appsettings.Development.json (dev environment)
- **Environment Variables:** Can override via environment (optional)

**Log Storage:**
- **Destination:** Browser console (F12 → Console tab)
- **Optional File Logging:** Not implemented in MVP (Phase 2 feature)
- **Log Levels:** INFO, WARNING, ERROR
- **Retention:** In-memory only (cleared on app restart)

---

### CI/CD Pipeline

**Build Process (Local Development):**

```bash
# 1. Build solution
dotnet build -c Debug

# 2. Run tests
dotnet test ReportingDashboard.Tests -c Debug

# 3. Publish self-contained .exe
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish

# 4. Output: ./publish/ReportingDashboard.exe (~120-150MB)
```

**GitHub Actions Workflow (Optional):**

```yaml
name: Build & Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET 8
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build -c Release --no-restore
    - name: Run tests
      run: dotnet test ReportingDashboard.Tests -c Release --no-build --logger "console;verbosity=normal"
    - name: Publish self-contained .exe
      run: dotnet publish ReportingDashboard -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: ReportingDashboard-exe
        path: ./publish/ReportingDashboard.exe
        retention-days: 30
```

**Manual Release Process:**

1. Developer builds self-contained .exe on local machine
2. Test .exe on clean Windows 10+ VM
3. Upload .exe to shared intranet folder or GitHub Releases
4. Email download link to executive stakeholders
5. Executives run .exe (no installation needed)

---

### Monitoring & Observability

**Application Logging:**
- **Framework:** Built-in Microsoft.Extensions.Logging
- **Levels:** Debug, Information, Warning, Error, Critical
- **Production Level:** Information (minimal overhead)
- **Output:** Browser console (F12)

**Key Events to Log:**

```csharp
// DataLoaderService
_logger.LogInformation($"Loading data from: {dataPath}");
_logger.LogInformation($"Successfully loaded {report.Milestones.Length} milestones");
_logger.LogError($"JSON deserialization failed: {ex.Message}");

// DataWatcherService
_logger.LogInformation($"DataWatcher started monitoring: {dataPath}");
_logger.LogInformation($"Data refresh triggered at {DateTime.Now:HH:mm:ss}");
_logger.LogWarning($"Failed to start DataWatcher: {ex.Message}");

// DataValidator
_logger.LogWarning($"Validation failed with {result.Errors.Count} errors");

// Index.razor
_logger.LogError($"Failed to load data: {ex.Message}");
_logger.LogInformation("Dashboard initialized successfully");
```

**Performance Monitoring (Manual):**
- **Cold Start Time:** <5 seconds (from .exe launch to dashboard visible)
- **Warm Start Time:** <2 seconds (from .exe launch with cached data)
- **JSON Parse Time:** <500ms (for typical data.json <5MB)
- **UI Refresh Latency:** <1 second (from file change to dashboard update)

**Memory Profiling:**
- Use Windows Task Manager → Details tab
- Monitor ReportingDashboard.exe memory usage
- Expected: ~150MB baseline (Blazor + .NET 8 runtime)
- Alert threshold: >300MB (potential memory leak)

**Health Checks (Manual):**
- Verify FileSystemWatcher is active (confirm refresh after data.json edit)
- Verify error messages display correctly (edit data.json with invalid JSON)
- Verify print CSS works (Ctrl+P in browser, check print preview)
- Verify screenshots are clean (take screenshot at 1080p and 1440p)

---

### Deployment Checklist

**Pre-Release:**
- [ ] Unit tests pass (80% data layer coverage)
- [ ] Component tests pass (70% component coverage)
- [ ] Manual testing complete (workflow, error handling, print/screenshot)
- [ ] README.md finalized with setup instructions
- [ ] Sample data.json included in .exe bundle
- [ ] Code review complete

**Release Process:**
- [ ] Build self-contained .exe
- [ ] Test .exe on clean Windows 10+ machine (verify cold start <5s)
- [ ] Verify file size <150MB
- [ ] Sign .exe with code certificate (optional, Phase 2)
- [ ] Upload .exe to shared intranet or GitHub Releases
- [ ] Document download link and instructions
- [ ] Send launch email to executive stakeholders

**Post-Release Support:**
- [ ] Monitor first week for issues (browser console errors)
- [ ] Collect feedback on data.json schema
- [ ] Track refresh latency feedback
- [ ] Plan Phase 2 enhancements

---

### System Configuration (appsettings.json)

**Development:**
```json
{
  "AppSettings": {
    "DataPath": "data.json",
    "Port": 5000,
    "DebounceIntervalMs": 500,
    "ProjectName": "Development Dashboard"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

**Production:**
```json
{
  "AppSettings": {
    "DataPath": "data.json",
    "Port": 5000,
    "DebounceIntervalMs": 500,
    "ProjectName": "Executive Dashboard"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## Technology Stack Decisions

### Runtime & Framework

**Decision: C# .NET 8.0 LTS (Mandatory)**

| Technology | Version | Justification |
|-----------|---------|---------------|
| .NET | 8.0 LTS | Mandatory; mature Blazor Server support, 3-year support window |
| Blazor Server | 8.0 | Built-in two-way data binding, server-side component lifecycle |
| Kestrel | Built-in | Lightweight, localhost-only, embedded, no IIS required |

**Rationale:**
- Mandatory technology stack per project requirements
- Blazor Server provides reactive component model with StateHasChanged()
- Built-in two-way data binding via @bind directive
- FileSystemWatcher integration (System.IO)
- System.Text.Json for JSON serialization (zero external dependency)

---

### Data & Serialization

| Technology | Version | Justification |
|-----------|---------|---------------|
| System.Text.Json | Built-in (.NET 8) | Zero external dependency, fast, source generators for validation |
| File System | NTFS (Windows) | Standard Windows file storage, atomic writes, ACL support |
| JSON File (data.json) | N/A | CEO-editable in VS Code/Notepad, no database overhead |

**Rationale:**
- Built-in to .NET 8; no external dependency
- Faster than Newtonsoft.Json; compile-time validation via source generators
- Flat JSON schema allows non-technical executives to edit directly
- NTFS permissions provide basic access control

---

### Frontend & UI

| Technology | Version | Justification |
|-----------|---------|---------------|
| Bootstrap | 5.3.x (CDN) | Professional defaults, print-friendly, zero build pipeline |
| Chart.js | 4.4.x (CDN) | Lightweight (35KB), screenshot-optimized, industry-standard |
| HTML5 `<progress>` | Built-in | Simple progress bars, no library needed |

**Rationale:**
- Bootstrap CDN eliminates Node.js build pipeline (aligns with "simple deployment")
- Chart.js is industry-standard (Grafana, DataDog use it)
- Horizontal bar chart naturally represents milestone progress
- Print CSS built-in; no external PDF generation needed

---

### File Monitoring

| Technology | Version | Justification |
|-----------|---------|---------------|
| FileSystemWatcher | Built-in (System.IO) | Native .NET, zero overhead, debounce 500ms prevents cascade updates |
| System.Timers.Timer | Built-in | Thread-safe debounce logic, no external library |

**Rationale:**
- FileSystemWatcher is native .NET; zero external dependency
- Event-driven (no polling CPU overhead)
- 500ms debounce prevents multiple LastWrite events on single file save
- Timer-based debounce is thread-safe and simple

---

### Testing

| Technology | Version | Justification |
|-----------|---------|---------------|
| xUnit | 2.7.x | .NET standard, well-documented, JSON validation tests |
| bUnit | 1.28.x | Blazor-specific, test components in isolation with mocked services |
| FluentAssertions | 6.x | Readable assertion syntax, easier debugging |
| Moq | 4.x | Lightweight, standard .NET mocking library |

**Rationale:**
- xUnit is .NET standard for unit testing
- bUnit enables Blazor component testing without running full app
- FluentAssertions improve readability vs. traditional Assert statements
- Moq allows service mocking in component tests

**Coverage Targets:**
- Data layer (DataLoaderService, DataValidator): ≥80%
- Components (MilestoneTimeline, StatusSnapshot, ProjectMetadata): ≥70%

**Phase 2 Enhancement (Not MVP):**
- Playwright 1.45.x: Visual regression testing (screenshot automation)

---

### Deployment

| Technology | Version | Justification |
|-----------|---------|---------------|
| Self-contained .exe | win-x64 | Single file (~120-150MB), no .NET runtime install required, one-click launch |
| Embedded Kestrel | Built-in | Localhost-only, no firewall complexity, no IIS required |

**Build Command:**
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

**Output:** Single .exe file containing .NET 8 runtime + app binaries + wwwroot

**Distribution:** File share, intranet download, email attachment

---

### Configuration Management

| Technology | Version | Justification |
|-----------|---------|---------------|
| appsettings.json | .NET 8 standard | Project name, data file path, port configuration |
| IConfiguration | Built-in | Read config from JSON, environment variables |

**Supported Keys:**
- AppSettings:DataPath (default: "data.json")
- AppSettings:Port (default: 5000)
- AppSettings:DebounceIntervalMs (default: 500)
- AppSettings:ProjectName (default: "Default Project")
- Logging:LogLevel:Default (default: "Information")

---

### Logging

| Technology | Version | Justification |
|-----------|---------|---------------|
| Microsoft.Extensions.Logging | Built-in | Standard .NET logging framework |
| Browser Console | Built-in | F12 → Console tab for debugging |

**Log Levels (Production):**
- Information: Normal operation events
- Warning: Unexpected but recoverable situations
- Error: Error conditions that need attention

**No File Logging (MVP):** Logging only to browser console. Phase 2 can add file logging if needed.

---

## Security Considerations

### Authentication & Authorization

**Status:** Not implemented (not required)

**Justification:**
- Single-user, single-machine app (localhost only)
- No remote network access
- No multi-user collaboration
- Physical machine access controls sufficient
- Executives on trusted corporate machines

**Decision:** Zero authentication overhead. App assumes user has OS login access.

---

### Data at Rest

**Current Approach:** No encryption

**Data Sensitivity:** Low
- Internal corporate metrics (milestones, KPIs, status)
- No PII, no financial data, no trade secrets
- No regulatory requirements (HIPAA, PCI-DSS, SOX)

**Mitigation Strategy:**
- Recommend IT set NTFS ACLs on data.json directory
  - Restrict read/write to authorized executives
  - Prevent non-executive access to shared machine
- Document in README: "IT should secure data.json with NTFS permissions"
- No app-level encryption (would require password, contradicts "zero friction")
- Phase 2: Consider encryption if data sensitivity increases

---

### Data in Transit

**Status:** Not applicable (zero network communication)

**Implementation:**
- Kestrel binds to 127.0.0.1:5000 (non-routable, localhost only)
- No external APIs called
- No cloud services
- No HTTPS needed (no encryption requirement for local traffic)

**Guarantee:** All data stays on local machine. No network exposure.

---

### Input Validation & Sanitization

**JSON Deserialization Error Handling:**
- Try/catch JsonException in DataLoaderService
- Display user-friendly error banner (not stack trace)
- Log error details to browser console (F12) for debugging
- Allow user to fix JSON and retry

**Schema Validation:**
- Validate all required fields (projectName, milestones, statusSnapshot)
- Validate enum values (status in [on-track, at-risk, delayed, completed])
- Validate data types (progress 0-100, dates ISO 8601)
- Validate non-empty collections (at least 1 milestone)
- Return ValidationResult with specific error messages per field
- Display red error banner if validation fails

**String Length Limits:**
- projectName: max 100 chars
- milestone name: max 100 chars
- milestone description: max 500 chars
- status items: max 200 chars
- Enforce in DataValidator; reject if exceeded

**Special Character Handling:**
- JSON strings sanitized by System.Text.Json (no injection risk)
- No HTML rendering of user input (Blazor component parameters are encoded)
- No eval() or dynamic code execution anywhere

**File Path Validation:**
- Resolve relative paths safely (no path traversal)
- Use Path.GetFullPath() + validation

---

### Error Message Disclosure

**Risk:** Stack traces expose internal paths, assembly names, sensitive logic

**Mitigation:**
- Never expose stack traces to UI (log internally only)
- Display generic error messages to executives
  - ✗ "System.IO.FileNotFoundException: C:\Users\admin\Desktop\data.json not found"
  - ✓ "Data file not found. Check the file location and try again."
- Log detailed errors to browser console (F12) for developer debugging
- All exceptions caught in Index.razor and converted to user-friendly messages

---

### Dependency Security

**Current Risk:** Zero external NuGet dependencies in critical path

**Analysis:**
- System.Text.Json: Built-in to .NET 8 (Microsoft-signed)
- FileSystemWatcher: Built-in (System.IO)
- Bootstrap 5.3: CDN-hosted, third-party, low risk (CSS only)
- Chart.js: CDN-hosted, third-party, low risk (charting library)
- Testing: xUnit, bUnit, Moq (development-only, not in deployed .exe)

**Mitigation:**
- No external NuGet dependencies in production code
- Reduces attack surface (no supply-chain risk from npm/NuGet compromises)
- Phase 2: If adding features, evaluate each dependency carefully

---

### Code Signing (Phase 2)

**Current:** Unsigned .exe (Windows SmartScreen may warn on first run)

**Phase 2 Enhancement:**
- Purchase code signing certificate
- Sign .exe before distribution
- Eliminates SmartScreen warnings
- Builds user confidence

---

## Scaling Strategy

### Current Scope (MVP)

**Single-Machine, Single-User Constraints:**
- No horizontal scaling (one machine per executive)
- No load balancing (single Kestrel instance)
- No multi-user concurrency (no locking needed)
- No database clustering (JSON file only)

**Vertical Scaling (Abundant Headroom):**
- Memory: ~150MB (Blazor + .NET 8 runtime) << 1GB available
- CPU: <1% idle (no background workers)
- Disk: ~130MB app + data.json << 300MB available

---

### Performance Bottlenecks & Mitigations

**Bottleneck 1: JSON Deserialization**

- **Current:** System.Text.Json with PropertyNameCaseInsensitive
- **Scalability Limit:** >500ms for large files (>20MB)
- **Mitigation:**
  - MVP: Acceptable for typical data.json (<5MB)
  - Monitor: Log JSON load time at startup and each refresh
  - Phase 2: If >500ms, implement source generators for compile-time validation

**Bottleneck 2: Chart.js Rendering**

- **Current:** All milestones loaded into Chart.js on canvas
- **Scalability Limit:** >20 milestones may cause 1-2 second render lag
- **Mitigation:**
  - MVP: Assume <20 milestones (typical project)
  - Monitor: Log Chart.js initialization time in browser console
  - Phase 2: If >20 milestones, implement pagination or server-side chart generation

**Bottleneck 3: DOM Rendering**

- **Current:** All status items rendered in HTML list
- **Scalability Limit:** >100 total items (shipped + in-progress + carried-over)
- **Mitigation:**
  - MVP: Assume <50 items per bucket
  - Phase 2: If >100 items, implement virtual scrolling or pagination

---

### Caching Strategy

**In-Memory Caching (Minimal):**
- ProjectReport cached in Index.razor component state
- Reloaded on every file change (not stale)
- No explicit caching layer needed (single-user, small data)

**Browser Caching (CDN):**
- Bootstrap 5.3 CSS cached by browser (first load only)
- Chart.js JS cached by browser (first load only)
- Fallback: If CDN unavailable, app loads with basic styling (graceful degradation)

**No Server-Side Caching:**
- Single-machine app, no clustering
- No cache invalidation complexity

---

### Phase 2 Scaling Enhancements

**If data.json >50MB:**
- Migrate to SQLite local database
- Use EF Core for queries and filtering
- Maintain JSON export for backward compatibility

**If executives >50:**
- Create MSI installer for Group Policy deployment
- Enable corporate IT to push updates

**If milestones >100:**
- Implement pagination (10 per page)
- Or: Collapsible sections (Active, Completed, Archived)

**If status items >100:**
- Implement virtual scrolling
- Load visible items only

---

## Risks & Mitigations

### Risk 1: FileSystemWatcher Misses Rapid File Writes

**Severity:** Medium | **Probability:** Medium | **Impact:** Data inconsistency (UI out-of-sync)

**Root Cause:**
- NTFS generates multiple LastWrite events on single file save
- Without debounce, cascade updates flood UI with stale refreshes

**Mitigation:**
- ✓ Implement 500ms debounce timer (collapses rapid events into single refresh)
- ✓ Log all file changes to browser console: "File changed at 14:32:45.123"
- ✓ Display refresh timestamp in UI: "Data refreshed at 14:32:45"
- Monitor: Watch browser console for duplicate "File changed" messages

**Fallback (if FSW fails):**
- Log warning: "FileSystemWatcher unavailable; app continues"
- User must manually refresh (Ctrl+R) if data.json edited
- App remains responsive (graceful degradation)

---

### Risk 2: JSON Deserialization Crash

**Severity:** High | **Probability:** Low | **Impact:** Blank screen, lost dashboard visibility

**Root Cause:**
- Malformed JSON (syntax error, missing fields, wrong types)
- Unhandled JsonException propagates, UI crashes

**Mitigation:**
- ✓ Try/catch JsonException in DataLoaderService
- ✓ Catch and wrap in Index.razor.OnDataChanged handler
- ✓ Display red error banner with specific guidance
  - Example: "Error in milestones[2].targetDate: invalid ISO 8601 format"
- ✓ Allow user to fix data.json and retry
- ✓ Log error details to browser console for troubleshooting

---

### Risk 3: CDN Unavailable (Bootstrap/Chart.js)

**Severity:** Low | **Probability:** Low | **Impact:** Degraded styling, missing chart

**Root Cause:**
- jsDelivr CDN down or corporate firewall blocks CDN access
- CSS and JS fail to load from remote

**Mitigation:**
- ✓ Fallback: App still loads and functions (Blazor components render)
- ✓ Baseline styling: Bootstrap provides defaults, milestones still visible
- ✓ Chart.js unavailable: Canvas renders empty, data displays in HTML
- ✓ Log warning to console: "CDN resources failed to load; using fallback"

---

### Risk 4: data.json File Locked (In-Use by Editor)

**Severity:** Medium | **Probability:** Low | **Impact:** Stale data, refresh fails silently

**Root Cause:**
- Executive editing data.json in VS Code
- FileSystemWatcher triggers, app tries to read while file locked
- IOException: "The process cannot access the file..."

**Mitigation:**
- ✓ Catch IOException in DataLoaderService
- ✓ Log warning: "File is locked; skipping refresh"
- ✓ Keep last-known-good data in memory (don't blank screen)
- ✓ Display warning in UI (optional): "File in use; refresh pending"
- ✓ Auto-retry on next file change (when file is released)

---

### Risk 5: Timestamp Misalignment (Stale "Data Refreshed" Indicator)

**Severity:** Low | **Probability:** Low | **Impact:** Executive confusion (trust erosion)

**Root Cause:**
- Refresh timestamp not updated if data.json change undetected
- Executive edits data.json, timestamp says "refreshed 5 minutes ago"

**Mitigation:**
- ✓ Use file LastWriteTime as cache buster (timestamp = file write time)
- ✓ Display refresh timestamp as "Data refreshed at HH:mm:ss" (always current)
- ✓ Log all file changes to console: "File changed 14:32:45, UI refreshed 14:32:45"
- ✓ Manual refresh button: Allow executive to manually trigger refresh

---

### Risk 6: Kestrel Web Server Crashes

**Severity:** Low | **Probability:** Very Low | **Impact:** App becomes unresponsive

**Root Cause:**
- Unhandled exception in Blazor component
- Stack overflow or out-of-memory in JSON parsing
- Kestrel crashes silently

**Mitigation:**
- ✓ Add ErrorBoundary component in App.razor
- ✓ Catch unhandled exceptions in component lifecycle
- ✓ Display error message instead of blank screen
- ✓ Allow user to refresh (F5) to recover
- ✓ Log all exceptions to browser console

---

### Risk 7: Windows .NET 8 Runtime Not Available

**Severity:** Medium | **Probability:** Low | **Impact:** App won't launch (compatibility issue)

**Root Cause:**
- Target OS: Windows 10+
- Executive on Windows 7 or 8 (unsupported)
- Self-contained .exe bundles .NET 8 (requires modern Windows)

**Mitigation:**
- ✓ Document system requirements: "Windows 10+ (64-bit)"
- ✓ Test on Windows 10 baseline
- ✓ Phase 2 Fallback: Offer framework-dependent .exe if IT pre-installs .NET 8 Runtime
- ✓ Communicate: Email setup instructions clarifying OS requirement

---

### Risk 8: Large JSON File Performance Degradation

**Severity:** Low | **Probability:** Low | **Impact:** 1-2 second UI lag during refresh

**Root Cause:**
- data.json >20MB: Deserialization takes 1-2 seconds
- DOM rendering >100 milestones: Browser layout recalculation slow

**Mitigation:**
- ✓ Monitor JSON parse time: Log in DataLoaderService
- ✓ If >1 second observed, alert executive: "Data file approaching size limit"
- ✓ Phase 2: Implement SQLite migration guide (data.json → database)
- ✓ Phase 2: Implement pagination (display 10 milestones per page)

---

### Risk Summary Matrix

| Risk | Severity | Probability | Mitigation | Effort |
|------|----------|-------------|-----------|--------|
| FSW misses writes | Medium | Medium | 500ms debounce + logging | 2 hours |
| JSON crash | High | Low | Try/catch + error banner | 2 hours |
| CDN unavailable | Low | Low | Graceful degradation | Included |
| File locked | Medium | Low | Catch IOException + retry | 1 hour |
| Stale timestamp | Low | Low | File LastWriteTime + logging | 1 hour |
| Kestrel crash | Low | Very Low | ErrorBoundary component | 1 hour |
| Old Windows | Medium | Low | Document requirements | 30 min |
| Large file lag | Low | Low | Monitor + Phase 2 migration | 30 min |

---

## Summary

This architecture delivers a simple, locally-hosted Executive Reporting Dashboard optimized for non-technical executives. By leveraging .NET 8's built-in capabilities and lightweight libraries, the design avoids unnecessary complexity while ensuring professional, PowerPoint-ready visuals. The single-page layout, auto-refresh via FileSystemWatcher, and transparent error handling build executive confidence. Deployment as a self-contained .exe eliminates IT friction. All identified risks have concrete mitigations. The MVP timeline is 4-6 weeks. Phase 2 enhancements (SQLite migration, MSI installer, visual regression testing) scale the system for larger deployments without compromising MVP delivery.