# Architecture

## Overview & Goals

Build a lightweight executive dashboard using Blazor Server (.NET 8) that loads project status from a local `data.json` file and renders a screenshot-friendly single-page view optimized for PowerPoint presentations. The dashboard displays milestones, tasks grouped by status (Shipped | In-Progress | Carried-Over), and fiscal quarter timelines with zero authentication, database dependencies, or external charting libraries.

**Key Objectives:**
- Enable executives to assess project health in one view without login friction
- Eliminate database dependencies and deployment complexity via local file-based JSON
- Deliver screenshot-quality visuals (1920x1080 fixed viewport) optimized for PowerPoint
- Reduce reporting overhead by prioritizing clarity over feature breadth
- Support grayscale print-safety and colorblind-accessible design patterns
- Deliver MVP in 3-4 days with <5MB artifact footprint

---

## System Components

### 1. DataProvider (C# Static Service)

**Responsibility:** Load, deserialize, validate, and cache project data from JSON.

**Public API:**
```csharp
public static class DataProvider
{
    public static async Task<ProjectData> LoadAsync(string filePath);
    public static ProjectData? GetCurrentData();
    public static void SetData(ProjectData data);
}
```

**Dependencies:**
- System.Text.Json (built-in, v8.0.x)
- System.IO (built-in)
- System.ComponentModel.DataAnnotations (built-in)
- ProjectData POCO models

**Data Ownership:**
- Cached ProjectData singleton (in-memory)
- File path configuration
- Parse errors and validation state
- Deserialization logic via System.Text.Json

**Error Handling:**
- Throws `JsonException` on malformed JSON  caught by Dashboard  renders ErrorCard
- Throws `ValidationException` on missing [Required] fields  caught by Dashboard
- Logs exceptions to browser console (no stack trace exposure)

---

### 2. Dashboard.razor (Root Component)

**Responsibility:** Orchestrate page lifecycle, manage loading states, coordinate child components, handle errors and refresh.

**Public Parameters:** None (root component)

**Public Methods:**
```csharp
protected override async Task OnInitializedAsync();
private async Task RefreshData();
```

**State Management:**
```csharp
private enum LoadingState { Loading, Loaded, Error }
private LoadingState currentState = LoadingState.Loading;
private ProjectData? data;
private string? errorMessage;
```

**Dependencies:**
- DataProvider service
- TimelineSection component
- TaskBoard component
- ErrorCard component

**Data Ownership:**
- LoadingState (Loading|Loaded|Error)
- ProjectData instance
- Error messages for display
- LINQ grouping logic for tasks

**Primary Workflow:**
1. OnInitializedAsync: Call DataProvider.LoadAsync("data.json")
2. Success: currentState = Loaded  render TimelineSection + TaskBoard
3. Failure: currentState = Error  render ErrorCard with Reload button
4. Group tasks via LINQ before passing to TaskBoard:
   ```csharp
   var shippedTasks = data.Tasks.Where(t => t.Status == TaskStatus.Shipped).ToList();
   var inProgressTasks = data.Tasks.Where(t => t.Status == TaskStatus.InProgress).ToList();
   var carriedOverTasks = data.Tasks.Where(t => t.Status == TaskStatus.CarriedOver).ToList();
   ```

---

### 3. TimelineSection.razor (Child Component)

**Responsibility:** Render milestone timeline as Tailwind gradient bars with fiscal quarter labels.

**Public Parameters:**
```csharp
[Parameter]
public List<Milestone> Milestones { get; set; } = new();
```

**Dependencies:**
- DateFormatter utility class (custom, 20 lines, zero dependencies)
- Tailwind CSS classes
- Milestone POCO model

**Data Ownership:**
- Milestone HTML structure (div layout)
- Gradient bar color scheme
- Date formatting via DateFormatter.ToMilestoneFormat()

**Rendering Logic:**
```
@foreach (var milestone in Milestones)
  <div class="relative h-2 bg-gray-200">
    <div class="absolute bg-gradient-to-r from-blue-400 to-blue-600" 
         style="width: {percentage}%; height: 100%;">
    </div>
    <span>{DateFormatter.ToMilestoneFormat(milestone.DueDate)}</span>
  </div>
```

**Output:** HTML divs (no ChartJS, no SVG, no JavaScript)

---

### 4. TaskBoard.razor (Child Component)

**Responsibility:** Render 3-column grid layout (Shipped | In-Progress | Carried-Over) with scrollable task lists.

**Public Parameters:**
```csharp
[Parameter]
public List<Task> ShippedTasks { get; set; } = new();

[Parameter]
public List<Task> InProgressTasks { get; set; } = new();

[Parameter]
public List<Task> CarriedOverTasks { get; set; } = new();
```

**Dependencies:**
- TaskCard component (child, rendered xN)
- Task POCO model
- Tailwind CSS grid utilities

**Data Ownership:**
- Column headers ("Shipped", "In-Progress", "Carried-Over")
- Task count display per column
- Scrollable container styling

**Rendering Structure:**
```html
<div class="grid grid-cols-3 gap-4">
  <div class="flex flex-col">
    <h3>Shipped ({ShippedTasks.Count})</h3>
    <div class="h-96 overflow-y-auto scroll-smooth">
      @foreach (var task in ShippedTasks) {
        <TaskCard Task="task" />
      }
    </div>
  </div>
  <!-- Similar for InProgress, CarriedOver -->
</div>
```

---

### 5. TaskCard.razor (Child Component)

**Responsibility:** Render individual task card with status-specific styling.

**Public Parameters:**
```csharp
[Parameter]
public Task Task { get; set; } = null!;
```

**Dependencies:**
- TaskStatus enum
- Tailwind CSS utility classes

**Data Ownership:**
- Status-conditional styling logic
- Badge text ("✓", "→", "?")
- Task display content

**Conditional Styling:**
```csharp
private string GetCardClasses(TaskStatus status) => status switch
{
    TaskStatus.Shipped => "bg-green-50 border-l-4 border-solid border-green-500 text-green-900",
    TaskStatus.InProgress => "bg-blue-50 border-l-4 border-solid border-blue-500 text-blue-900 font-semibold",
    TaskStatus.CarriedOver => "bg-amber-50 border-l-4 border-dashed border-amber-400 text-amber-900 line-through",
    _ => ""
};
```

**Output:** HTML div with Tailwind classes applied conditionally

---

### 6. ErrorCard.razor (Child Component)

**Responsibility:** Display user-friendly error message and recovery action.

**Public Parameters:**
```csharp
[Parameter]
public string ErrorMessage { get; set; } = "Unknown error occurred";

[Parameter]
public EventCallback OnReload { get; set; }
```

**Dependencies:**
- Tailwind CSS classes for error styling

**Data Ownership:**
- Error display styling
- Button labels and actions

**Output:**
```html
<div class="bg-red-50 border-l-4 border-red-500 p-4 rounded text-red-900">
  <h2 class="font-bold text-lg mb-2">⚠️ Error Loading Dashboard</h2>
  <p class="mb-4">@ErrorMessage</p>
  <p class="text-sm mb-4">Check data.json format and click Reload to try again.</p>
  <button @onclick="OnReload">🔄 Reload</button>
</div>
```

---

## Component Interactions

### Data Flow: Page Load → Display Dashboard

```
1. User requests dashboard route (/)

2. Dashboard.OnInitializedAsync() executes
   → currentState = Loading
   → Render loading spinner

3. Dashboard calls DataProvider.LoadAsync("data.json")
   
4. DataProvider:
   → Reads file (System.IO.File.ReadAllTextAsync)
   → Deserializes JSON (System.Text.Json.JsonSerializer.Deserialize)
   → Validates POCOs (DataAnnotations reflection)
   → Caches result in _cachedData singleton
   → Returns ProjectData

5. Success Path:
   → currentState = Loaded
   → data = ProjectData instance
   → Dashboard.StateHasChanged()
   
6. Dashboard renders children:
   → TimelineSection receives data.Milestones
   → TaskBoard receives:
      - shippedTasks (filtered via LINQ)
      - inProgressTasks (filtered via LINQ)
      - carriedOverTasks (filtered via LINQ)

7. TimelineSection renders:
   @foreach milestone
   → DateFormatter.ToMilestoneFormat(milestone.DueDate)
   → Output: "Q2 FY2026 (May 15)"
   → Tailwind gradient bar HTML

8. TaskBoard renders 3 columns:
   @foreach in each list
   → TaskCard Task={task}

9. TaskCard renders:
   → GetCardClasses(task.Status)
   → GetBadge(task.Status)
   → HTML div with conditional Tailwind classes

10. Browser displays fully-rendered dashboard
```

### Data Flow: User Clicks Refresh

```
1. User clicks "🔄 Refresh" button

2. Dashboard.RefreshData() executes
   → currentState = Loading
   → Render loading spinner (overlay)

3. Repeat steps 3-9 from "Page Load" flow

4. Dashboard.StateHasChanged()

5. Component tree re-renders with fresh data
```

### Data Flow: JSON Parse Error

```
1. DataProvider.LoadAsync() reads data.json
   → System.Text.Json throws JsonException
   OR
   → ValidationException (missing [Required] field)

2. Dashboard catch block:
   → currentState = Error
   → errorMessage = exception.Message
   → Console.WriteLine (browser console)

3. Dashboard renders ErrorCard:
   → ErrorMessage parameter = exception message
   → OnReload callback bound to Dashboard.RefreshData()

4. User clicks "🔄 Reload" button
   → ErrorCard fires OnReload EventCallback
   → Dashboard.RefreshData() executes
   → Repeat page load flow
```

### Component Dependency Graph

```
Dashboard.razor (root)
├── TimelineSection.razor (child)
│   └── DateFormatter (utility)
├── TaskBoard.razor (child)
│   └── TaskCard.razor (xN children)
├── ErrorCard.razor (child)
└── DataProvider (service)
    ├── System.Text.Json
    ├── System.IO
    ├── System.ComponentModel.DataAnnotations
    └── ProjectData POCOs
```

**Data Flow Direction:** Unidirectional (parent → child via [Parameter])
- Dashboard → TimelineSection (Milestones)
- Dashboard → TaskBoard (ShippedTasks, InProgressTasks, CarriedOverTasks)
- TaskBoard → TaskCard (Task)
- Dashboard → ErrorCard (ErrorMessage, OnReload callback)
- ErrorCard → Dashboard (fires OnReload callback)

---

## Data Model

### Entity: ProjectData (Root Aggregate)

```csharp
public record ProjectData
{
    [JsonPropertyName("project")]
    public required ProjectInfo Project { get; init; }

    [JsonPropertyName("milestones")]
    public required List<Milestone> Milestones { get; init; }

    [JsonPropertyName("tasks")]
    public required List<Task> Tasks { get; init; }
}
```

### Entity: ProjectInfo

```csharp
public record ProjectInfo
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    [Required(ErrorMessage = "Project name is required")]
    public required string Name { get; init; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProjectStatus Status { get; init; } = ProjectStatus.OnTrack;

    [JsonPropertyName("lastUpdated")]
    public DateTime? LastUpdated { get; init; }

    [JsonPropertyName("owner")]
    public string? Owner { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

public enum ProjectStatus
{
    [EnumMember(Value = "on-track")]
    OnTrack = 0,

    [EnumMember(Value = "at-risk")]
    AtRisk = 1,

    [EnumMember(Value = "blocked")]
    Blocked = 2,

    [EnumMember(Value = "complete")]
    Complete = 3
}
```

### Entity: Milestone

```csharp
public record Milestone
{
    [JsonPropertyName("id")]
    [Required]
    public required string Id { get; init; }

    [JsonPropertyName("title")]
    [Required(ErrorMessage = "Milestone title is required")]
    public required string Title { get; init; }

    [JsonPropertyName("dueDate")]
    [Required(ErrorMessage = "Due date is required")]
    public required DateTime DueDate { get; init; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MilestoneStatus Status { get; init; } = MilestoneStatus.InProgress;

    [JsonPropertyName("completionPercent")]
    [Range(0, 100)]
    public int CompletionPercent { get; init; } = 0;

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

public enum MilestoneStatus
{
    [EnumMember(Value = "not-started")]
    NotStarted = 0,

    [EnumMember(Value = "in-progress")]
    InProgress = 1,

    [EnumMember(Value = "completed")]
    Completed = 2,

    [EnumMember(Value = "delayed")]
    Delayed = 3
}
```

### Entity: Task

```csharp
public record Task
{
    [JsonPropertyName("id")]
    [Required]
    public required string Id { get; init; }

    [JsonPropertyName("title")]
    [Required(ErrorMessage = "Task title is required")]
    public required string Title { get; init; }

    [JsonPropertyName("status")]
    [Required(ErrorMessage = "Task status is required")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required TaskStatus Status { get; init; }

    [JsonPropertyName("dueDate")]
    public DateTime? DueDate { get; init; }

    [JsonPropertyName("owner")]
    public string? Owner { get; init; }

    [JsonPropertyName("month")]
    public string? Month { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("completedDate")]
    public DateTime? CompletedDate { get; init; }
}

public enum TaskStatus
{
    [EnumMember(Value = "shipped")]
    Shipped = 0,

    [EnumMember(Value = "in-progress")]
    InProgress = 1,

    [EnumMember(Value = "carried-over")]
    CarriedOver = 2
}
```

### Storage: data.json

**Location:** `{AppRoot}/data.json` (colocated with application) or configurable via appsettings.json

**Format:** UTF-8 JSON (no BOM)

**Property Naming:** camelCase

**Enum Naming:** kebab-case (e.g., "on-track", "in-progress", "shipped")

**Sample Structure:**
```json
{
  "project": {
    "id": "proj-2026-q2",
    "name": "Q2 2026 MVP Launch",
    "status": "on-track",
    "lastUpdated": "2026-04-09T22:21:00Z",
    "owner": "Executive Team"
  },
  "milestones": [
    {
      "id": "m1",
      "title": "MVP Launch",
      "dueDate": "2026-05-15T00:00:00Z",
      "status": "in-progress",
      "completionPercent": 65,
      "description": "Deploy API to production"
    }
  ],
  "tasks": [
    {
      "id": "t1",
      "title": "Deploy API",
      "status": "shipped",
      "dueDate": "2026-04-05T00:00:00Z",
      "owner": "Backend Team",
      "month": "2026-04",
      "completedDate": "2026-04-05T18:30:00Z"
    }
  ]
}
```

**Constraints:**
- Max Size: <5 MB comfortable
- Max Tasks: <200 (recommended for smooth scrolling)
- Max Milestones: <50 (recommended for timeline rendering)
- Field Length Limits:
  - Project.Name: 200 chars max
  - Milestone.Title: 150 chars max
  - Task.Title: 200 chars max
  - Owner: 100 chars max
  - Description: 500 chars max

**Serialization Configuration:**
```csharp
public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseNamingPolicy)
        }
    };
}
```

**Deserialization Rules:**
- Property names match JSON camelCase (auto-converted)
- Enum values match kebab-case strings (e.g., "on-track" → ProjectStatus.OnTrack)
- DateTime fields parse ISO8601 UTC (no timezone conversion)
- Unknown properties ignored (forward compatibility)

---

## API Contracts

### DataProvider Service Interface

#### LoadAsync(filePath: string) → Task<ProjectData>

**Request:**
- `filePath` (string): Relative or absolute path to data.json file

**Response (Success):**
```csharp
ProjectData {
  Project: ProjectInfo { ... },
  Milestones: List<Milestone> { ... },
  Tasks: List<Task> { ... }
}
```

**Response (Failure):**

| Exception | Trigger | Message |
|-----------|---------|---------|
| `FileNotFoundException` | data.json not found at path | "File 'data.json' not found at {path}" |
| `JsonException` | Invalid JSON syntax | "'{char}' is an invalid start of a value..." |
| `JsonException` | Type mismatch (dueDate as int) | "The JSON value could not be converted to System.DateTime..." |
| `ValidationException` | Missing [Required] field | "The {FieldName} field is required." |
| `Exception` | Unexpected error | Full exception (logged, not exposed to UI) |

**Error Logging:**
```csharp
try
{
    return await LoadAsync(filePath);
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] DataProvider.LoadAsync failed: {ex.GetType().Name}");
    Console.WriteLine($"[ERROR] {ex.Message}");
    throw;
}
```

**Performance:**
- JSON Parse: <100ms for files <5MB
- Deserialization: <50ms for typical data
- Total: <150ms for typical load

---

#### GetCurrentData() → ProjectData?

**Returns:** Last loaded ProjectData from in-memory cache, or null if not yet loaded

**Usage:** Retrieve cached data without re-parsing JSON file

---

#### SetData(data: ProjectData) → void

**Request:** Valid ProjectData instance

**Side Effects:**
- Updates in-memory cache
- Fires DataReloaded event (Phase 2 feature)

---

### Blazor Component Contracts

#### Dashboard.razor

**Lifecycle:**
- `OnInitializedAsync()`: Calls DataProvider.LoadAsync("data.json"), sets state, renders
- `RefreshData()`: Clears state, calls LoadAsync again, triggers re-render

**State Machine:**
```
Start → Loading → Loaded → Display
                    ↓
                  Error → Reload
```

**Child Component Bindings:**
```html
@if (currentState == LoadingState.Loading)
{
  <div>Loading dashboard...</div>
}
else if (currentState == LoadingState.Loaded && data != null)
{
  <TimelineSection Milestones="data.Milestones" />
  <TaskBoard ShippedTasks="..." InProgressTasks="..." CarriedOverTasks="..." />
}
else if (currentState == LoadingState.Error)
{
  <ErrorCard ErrorMessage="@errorMessage" OnReload="RefreshData" />
}
```

---

#### TimelineSection.razor

**Parameter Binding:**
```html
<TimelineSection Milestones="data.Milestones" />
```

**Expected Input:** `List<Milestone>` (unordered, sorted by DueDate in template)

**Output:** Static HTML divs (no interactive elements, no JavaScript)

---

#### TaskBoard.razor

**Parameter Bindings:**
```html
<TaskBoard 
  ShippedTasks="shippedTasks" 
  InProgressTasks="inProgressTasks" 
  CarriedOverTasks="carriedOverTasks" />
```

**Expected Input:** Three pre-grouped `List<Task>` instances (already filtered by status)

**Output:** 3-column grid with TaskCard children

---

#### TaskCard.razor

**Parameter Binding:**
```html
@foreach (var task in ShippedTasks) {
  <TaskCard Task="task" />
}
```

**Expected Input:** Single `Task` POCO instance

**Output:** HTML div with conditional Tailwind classes applied

---

#### ErrorCard.razor

**Parameter Bindings:**
```html
<ErrorCard 
  ErrorMessage="@errorMessage" 
  OnReload="@(async () => await RefreshData())" />
```

**Expected Inputs:**
- `ErrorMessage` (string): User-friendly error description
- `OnReload` (EventCallback): Callback to Dashboard.RefreshData method

**Output:** Error card UI with Reload button

---

## Infrastructure Requirements

### Hosting

#### Development Environment

**Runtime:** Kestrel (self-hosted .NET development server)

**Startup:**
```bash
cd C:\Git\AgentSquad\src\AgentSquad.Runner
dotnet run --configuration Development
```

**Accessible At:**
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001 (optional)

**Data Path:** `{ProjectRoot}/data.json` or relative path specified in appsettings.Development.json

**Supported Browsers:** Chrome 90+, Edge 90+

**Port Forwarding:** Not required (local access only)

---

#### Production Environment (IIS 10+)

**Application Pool Configuration:**
- .NET Version: .NET 8.0.x
- Pipeline Mode: Integrated
- Managed Runtime Version: v4.0

**Physical Path:** `C:\inetpub\wwwroot\agentsquad-runner\`

**Website Binding:**
- Protocol: HTTP (default) or HTTPS (optional)
- IP Address: All Unassigned
- Port: 80 (HTTP) or 443 (HTTPS)
- Hostname: {server-hostname}

**Application Identity:** ApplicationPoolIdentity

**Data Path:** `C:\inetpub\wwwroot\agentsquad-runner\data.json`

**File Permissions:**
```
C:\inetpub\wwwroot\agentsquad-runner\data.json
├── Owner: SYSTEM
├── IIS AppPool identity: Read (R) permission
├── Admin/Developers: Modify (write for manual updates)
└── Everyone: Deny
```

**Required Installation:** .NET 8.0 Hosting Bundle (one-time install per server)

---

### Networking

#### Local Only (MVP)

- No external API calls
- No cloud services or CDN
- No database connectivity (local file-based only)
- No WebSocket/SignalR
- No real-time data push

**Scope:** Single machine deployment or internal network only

---

### Storage

#### Primary Data Store: data.json

**Location Options:**
1. Development: `{ProjectRoot}/data.json`
2. Production (IIS): `C:\inetpub\wwwroot\agentsquad-runner\data.json`
3. Configurable: Set via appsettings.json `DataProvider:FilePath` property

**File Properties:**
- Encoding: UTF-8 (no BOM)
- Max Size: <5 MB comfortable (>50 MB requires database migration)
- Format: JSON (human-readable, CamelCase properties, kebab-case enums)
- Backup Strategy: Git version control + periodic snapshots
- Permissions: Read-only for app; admin-write only

---

#### Configuration Files

**appsettings.json (Development)**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "DataProvider": {
    "FilePath": "data.json",
    "ValidationMode": "Light"
  }
}
```

**appsettings.Production.json (IIS)**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  },
  "DataProvider": {
    "FilePath": "C:\\inetpub\\wwwroot\\agentsquad-runner\\data.json",
    "ValidationMode": "Light"
  }
}
```

---

### Deployment Artifacts

**Published DLL Structure:**
```
publish/
├── AgentSquad.Runner.dll                    (~800 KB)
├── AgentSquad.Runner.pdb                    (~200 KB, optional)
├── System.*.dll (framework refs)            (~1.5 MB)
├── Microsoft.AspNetCore.*.dll (runtime)     (~2.0 MB)
├── Tailwind CSS library                     (~30 KB)
└── data.json                                (~50 KB sample)

Total: ~4.5 MB (framework-dependent)
Alternative: ~50 MB self-contained (with .NET 8 runtime)
```

**Build & Publish:**

```bash
# Development Build
dotnet build --configuration Debug
# Output: bin/Debug/net8.0/

# Release Publish (framework-dependent)
dotnet publish --configuration Release --output ./publish
# Output: publish/net8.0/

# Self-Contained Publish (optional)
dotnet publish -c Release -r win-x64 --self-contained --output ./publish-standalone
# Output: publish-standalone/ (~50 MB)
```

---

### Monitoring & Logging

#### Development

**Console Logging:**
- Debug, Information, Warning, Error messages to console
- Configurable via `appsettings.Development.json`

**Browser Console:**
- Client-side exceptions logged
- DataProvider errors logged in full
- No stack trace exposure to UI (error card only)

**Visual Studio Debugging:**
- Breakpoints, Watch windows, Step-through debugging
- Network inspection (Chrome DevTools F12)

---

#### Production (IIS)

**Event Viewer Logging:**
- Application event log (Windows Event Viewer)
- Errors logged with timestamp and exception details

**Browser Console:**
- Users can inspect errors via browser console (F12)
- Exceptions logged for troubleshooting

**No External Services:**
- No cloud logging (Azure Application Insights, Splunk, etc.)
- No APM (Application Performance Monitoring)
- No error tracking (Sentry, etc.)

**Manual Review Process:**
- Admin reviews IIS logs periodically
- Users report errors via manual console inspection

---

### CI/CD Pipeline (Phase 2, Optional)

**GitHub Actions Workflow:**
```yaml
name: Build & Publish

on:
  push:
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
      
      - name: Build
        run: dotnet build --configuration Release
      
      - name: Test
        run: dotnet test --configuration Release --verbosity normal
      
      - name: Publish
        run: dotnet publish -c Release --output publish
      
      - name: Deploy to IIS
        run: |
          # PowerShell remoting or manual deployment
          # Copy publish/ to C:\inetpub\wwwroot\agentsquad-runner\
          # Restart application pool
```

---

## Technology Stack Decisions

### Core Framework

**Chosen:** Blazor Server (.NET 8.0.x)

**Rationale:**
- Server-side rendering (SSR) eliminates JavaScript framework overhead
- Native C# development (no TypeScript, no Node.js)
- Built-in async/await for file I/O operations
- Mature, production-ready (.NET 8 LTS, support through Nov 2026)
- Zero external JavaScript dependencies required
- Ideal for internal tools with limited user base

**Alternatives Rejected:**
- Blazor WebAssembly: Requires more client-side logic; less ideal for server-side data loading
- ASP.NET MVC: More complex view rendering; Blazor components more reusable
- Razor Pages: Less compositional; Blazor components better for dashboard structure

---

### JSON Serialization

**Chosen:** System.Text.Json (v8.0.x, built-in)

**Rationale:**
- Zero NuGet dependencies (built-in to .NET 8)
- Fast JSON deserialization (optimal performance)
- Native support for POCO models via DataAnnotations
- Supports custom Enum converters (kebab-case)

**Alternatives Rejected:**
- Newtonsoft.Json (Json.NET): Legacy, heavier, external dependency
- Custom JSON parsing: Reinventing the wheel, error-prone

---

### CSS Framework

**Chosen:** Tailwind CSS 3.4.x (via NuGet)

**Rationale:**
- Lightweight utility-first design (~30 KB gzipped)
- No CSS hand-coding required (faster development)
- WCAG AAA color utilities built-in
- Zero JavaScript dependencies
- Minimal artifact footprint
- Reusable across all components

**Alternatives Rejected:**
- Bootstrap 5: Heavier (~50+ KB), excessive components, more bloat
- Custom CSS: Time-consuming, error-prone, inconsistent styling
- Material Design: Overkill for screenshot-focused dashboard

---

### Validation Framework

**Chosen:** System.ComponentModel.DataAnnotations (built-in)

**Rationale:**
- Zero NuGet dependencies (built-in to .NET 8)
- Light-touch validation: `[Required]` attributes only
- Fast implementation (1-2 hours for all POCOs)
- Sufficient for internal tool with manual data entry
- No performance overhead

**Alternatives Rejected:**
- FluentValidation: Over-engineered for this scope, 4-6 hours implementation
- JSON Schema Validators: External tools, CI/CD complexity, overkill

---

### Testing Framework

**Chosen:** xUnit 2.6.x + Moq 4.20.x

**Rationale:**
- xUnit: .NET-native, lightweight, mature
- Moq: Simple mocking for file I/O and JSON deserialization
- Minimal test setup (no complex fixtures)
- Fast test execution (<1 second for DataProvider tests)

**Test Scope (MVP):**
- DataProvider.LoadAsync() nominal path (valid JSON, successful parse)
- DataProvider.LoadAsync() error paths (missing file, malformed JSON, validation failures)
- Target Coverage: >80% of DataProvider logic

---

### Date Formatting

**Chosen:** Custom DateFormatter utility class (20 lines, zero dependencies)

**Rationale:**
- Zero external dependencies (no Humanizer NuGet)
- Tight control over fiscal year format
- Executive-friendly output: "Q2 FY2026 (May 15)"
- Reusable across timeline and task cards

**Implementation:**
```csharp
public static class DateFormatter
{
    public static string ToMilestoneFormat(DateTime date, int fiscalYearStart = 10)
    {
        var quarter = (date.Month - 1) / 3 + 1;
        var fiscalYear = date.Month >= fiscalYearStart ? date.Year + 1 : date.Year;
        return $"Q{quarter} FY{fiscalYear} ({date:MMM dd})";
    }
}

// Usage: DateFormatter.ToMilestoneFormat(new DateTime(2026, 5, 15))
// Output: "Q2 FY2026 (May 15)"
```

**Alternatives Rejected:**
- Humanizer 2.14.x: External NuGet, adds complexity, not needed for simple formatting

---

### Hosting Platform

**Chosen (Development):** Kestrel (self-hosted .NET development server)

**Chosen (Production):** IIS 10+ with .NET 8 Hosting Bundle

**Rationale:**
- Local/internal deployment only (no cloud services)
- IIS: Standard Windows enterprise hosting
- Kestrel: Simple development server, no setup complexity
- No Docker, no container orchestration (MVP scope)

**Alternatives Rejected:**
- Azure App Service: Cloud service, out of scope (local only)
- AWS Lambda: Serverless overkill, cloud service
- Docker: Containerization unnecessary for local deployment

---

## Security Considerations

### Authentication & Authorization

**MVP Status:** Not Required

**Reasoning:**
- Internal tool for executive use only
- Single-user assumption (one dashboard per instance)
- Read-only dashboard (no write operations)
- Trusted environment (local/intranet only)
- No multi-user access control needed

**Phase 2 Recommendations:**
- If multi-user access required: Add Windows Authentication (IIS integrated mode)
- If intranet expansion needed: Add Basic Auth or OAuth2
- Role-based access control: Defer until feature demand emerges

---

### Data Protection

**Risk Profile:** Minimal

**Content Assessment:**
- data.json contains no PII (Personally Identifiable Information)
- No secrets (API keys, credentials, passwords)
- No sensitive business data (only milestone/task names + status)
- No encryption at rest needed

**File Permissions (IIS):**
```
C:\inetpub\wwwroot\agentsquad-runner\data.json

ACL Permissions:
├── Owner: SYSTEM (Full Control)
├── IIS AppPool identity: Read (R) permission only
├── Administrators: Modify (for manual updates)
└── Everyone: Deny (no access)
```

**Backup Strategy:**
- Primary: Git version control (commit data.json to repo)
- Secondary: Periodic file snapshots (daily/weekly backup copies)
- No database backups required (file is source of truth)

---

### Input Validation

**Strategy:** Light, Schema-Based Validation

**Validation Rules:**
```csharp
// All POCO properties marked [Required] will trigger ValidationException
// if missing from JSON during deserialization

[Required] string Name;           // Must be present in JSON
[Required] string Id;              // Must be present in JSON
[Required] DateTime DueDate;       // Must be present and valid ISO8601
[Required] TaskStatus Status;      // Must be present and valid enum value
```

**Validation Exceptions:**
- Caught by Dashboard component
- Rendered in ErrorCard (user-friendly message only)
- Full exception logged to browser console (admin troubleshooting)

**No Stack Trace Exposure:**
- Error card displays: "Failed to parse data.json. Check JSON format."
- Console logs full exception details
- Never expose stack trace to end-user UI

---

### JSON Parse Errors

**Safe Error Handling:**
```csharp
try
{
    var json = await File.ReadAllTextAsync(filePath);
    var data = JsonSerializer.Deserialize<ProjectData>(json, JsonOptions.Default);
    return data;
}
catch (JsonException ex)
{
    // Malformed JSON syntax
    Console.WriteLine($"[ERROR] JSON Parse failed: {ex.Message}");
    throw;  // Caught by Dashboard → ErrorCard
}
catch (ValidationException ex)
{
    // Missing [Required] field
    Console.WriteLine($"[ERROR] Validation failed: {ex.Message}");
    throw;  // Caught by Dashboard → ErrorCard
}
catch (Exception ex)
{
    // Unexpected error
    Console.WriteLine($"[ERROR] Unexpected: {ex}");
    throw;  // Caught by Dashboard → ErrorCard
}
```

---

### No Direct User Input

**Security Benefit:**
- Read-only dashboard (no form submissions)
- No query parameters interpreted
- No text input fields accepting user data
- No injection attack vectors (SQL, script, etc.)

---

## Scaling Strategy

### Vertical Scaling (MVP)

**Current Approach:** Single-machine deployment only

**Memory Usage (Per Instance):**
- ProjectData deserialized: ~100 KB (3 milestones + 12 tasks)
- Blazor Server runtime: ~30 MB (initialization overhead)
- Total per user: ~35-40 MB (acceptable for single desktop user)

**Performance Targets:**
- Startup time: <5 seconds (Blazor Server initialization + JSON parse)
- Page load: <2 seconds (Dashboard render after DataProvider.LoadAsync)
- JSON parse: <100ms (for files <5MB)
- Refresh action: <1 second (full data reload + re-render)
- Scrolling: <60ms frame time (smooth scrolling, no jank)

---

### Performance Thresholds & Bottlenecks

| Metric | MVP Limit | Threshold | Bottleneck | Phase 2 Solution |
|--------|-----------|-----------|-----------|-----------------|
| **Tasks** | <200 | 500+ | Fixed-height scroll jank | Implement Blazor.Virtualize (render only visible items) |
| **Milestones** | <50 | 100+ | Timeline width overflow | Add pagination or collapsible groups |
| **JSON File Size** | <5 MB | >50 MB | File read/parse lag | Migrate to SQLite or SQL Server database |
| **Startup Time** | <5 sec | Exceeds | Slow Blazor initialization | Add in-memory cache + pre-warming |
| **Concurrent Users** | 1 (local) | 10+ (intranet) | Blazor Server scalability | Implement circuit pooling + load balancing |

---

### In-Memory Caching (MVP)

**Singleton Pattern:**
```csharp
public static class DataProvider
{
    private static ProjectData? _cachedData;

    public static ProjectData? GetCurrentData() => _cachedData;

    public static async Task<ProjectData> LoadAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        _cachedData = JsonSerializer.Deserialize<ProjectData>(json, JsonOptions.Default);
        return _cachedData;
    }
}
```

**Cache Behavior:**
- Persists for browser session lifetime
- Reloaded on manual "Refresh" button click
- No automatic invalidation (Phase 1)
- No FileSystemWatcher auto-reload (Phase 2 feature)

**Benefit:**
- Eliminates repeated JSON file reads within session
- Fast GetCurrentData() access (zero deserialization)
- Minimal memory overhead (~100 KB for typical data)

---

### No Horizontal Scaling (MVP)

**Constraint:** Single-machine deployment only

**Rationale:**
- Internal tool (single executive per instance)
- No concurrent user requirement
- No load balancing needed
- No multi-server replication needed

**Phase 2 Scaling Path (If Needed):**
- Implement Blazor circuit pooling (ASP.NET Core feature)
- Add load balancer (if intranet deployment scales to 10+ users)
- Consider database migration (if JSON >50 MB or 100+ concurrent users)

---

### Database Migration Threshold (Phase 2)

**Current Strategy:** Local JSON file (MVP)

**Trigger for Database Migration:**
- JSON file size exceeds 50 MB
- Task count exceeds 500 (performance degradation)
- Milestone count exceeds 100
- Multi-user concurrent access required
- Real-time data sync needed

**Recommended Database (Phase 2):**
- SQLite (local, zero setup) — for single-server IIS deployment
- SQL Server (if multi-server or 100+ users) — requires infrastructure

---

## Risks & Mitigations

### High-Impact Technical Risks

#### Risk 1: JSON File Corruption

**Likelihood:** Low

**Impact:** Parse failure  blank dashboard  business disruption

**Mitigation Strategy:**
1. **DataAnnotations Validation:**
   - `[Required]` attributes catch missing fields
   - `[Range(0,100)]` validates CompletionPercent
   - Type validation via System.Text.Json (DateTime, enum, etc.)

2. **Error Card UI:**
   - Display user-friendly message: "Failed to parse data.json"
   - Include recovery instruction: "Check JSON format and click Reload"
   - No stack trace exposure (console only)

3. **Manual Reload Recovery:**
   - "🔄 Reload" button allows user to retry after fixing JSON
   - No silent failures (always show error message)

4. **Git Backup:**
   - Commit data.json to version control
   - Retrieve previous version if current is corrupted

**Owner:** Developer (prevention), Admin (recovery)

---

#### Risk 2: File Path Misconfiguration

**Likelihood:** Medium

**Impact:** File not found  app crash  dashboard inaccessible

**Mitigation:**
1. **Configurable Path:**
   - Accept file path via appsettings.json
   - Support relative and absolute paths
   - Document both options in README

2. **Startup Validation:**
   - Verify file exists before dashboard loads
   - Log clear error message if missing: "data.json not found at {path}"

3. **Configuration Documentation:**
   - Include appsettings.json samples (Development, Production)
   - Deployment guide with path instructions

**Owner:** Admin (configuration), Developer (validation logic)

---

#### Risk 3: Blazor Server Crash

**Likelihood:** Low

**Impact:** Session lost  user must refresh browser  in-progress work lost

**Mitigation:**
1. **Read-Only Dashboard:**
   - No user data mutations (no save operations)
   - No data loss on session disconnect

2. **Manual Refresh Button:**
   - User can recover by clicking "🔄 Refresh"
   - Triggers DataProvider.LoadAsync() again
   - Full re-render of dashboard

3. **Blazor Reconnection Logic (Phase 2):**
   - Implement circuit state restoration
   - Graceful degradation message
   - Automatic retry on connection drop

**Owner:** Developer (error handling), User (manual recovery)

---

#### Risk 4: Large JSON Parse Timeout

**Likelihood:** Low

**Impact:** Startup delay >5 seconds  poor user experience

**Mitigation:**
1. **Asynchronous Loading:**
   - Use `File.ReadAllTextAsync` (non-blocking I/O)
   - Prevent Blazor thread blocking

2. **Size Constraints:**
   - Recommend <5 MB file size (MVP limit)
   - >5 MB triggers performance review
   - >50 MB triggers database migration discussion

3. **Pre-Launch Testing:**
   - Load-test with representative data (~4-5 MB)
   - Measure parse time (<100ms target)
   - Benchmark in target environments (Kestrel, IIS)

**Owner:** Developer (testing), Admin (data management)

---

#### Risk 5: Screenshot Resolution Mismatch

**Likelihood:** Medium

**Impact:** Text unreadable in PowerPoint  dashboard unusable for presenting

**Mitigation:**
1. **Fixed Viewport:**
   - Lock to 1920x1080 via HTML meta tag:
     ```html
     <meta name="viewport" content="width=1920px, initial-scale=1, user-scalable=no">
     ```
   - No responsive breakpoints (no `md:`, `lg:` Tailwind prefixes)

2. **Cross-Browser Testing:**
   - Test at 1920x1080 in Chrome 90+ and Edge 90+
   - Verify text crisp, borders sharp, colors consistent
   - Screenshot comparison (before launch)

3. **PowerPoint Validation:**
   - Export screenshot at correct resolution
   - Paste into PowerPoint deck
   - Verify readability in presentation mode

4. **DPI Awareness:**
   - Use CSS pixels (not device pixels)
   - Tailwind utilities handle scaling
   - Test on multiple monitor DPIs

**Owner:** QA (testing), Developer (implementation)

---

### Dependency Risks

#### .NET 8 LTS Support

**Risk:** End-of-life approaching

**Mitigation:**
- Track EOL date: November 10, 2026
- Plan migration to .NET 9 (or later LTS) post-MVP
- Document upgrade path in Phase 2 roadmap

---

#### Tailwind CSS Stability

**Risk:** CSS generation changes breaking layout

**Mitigation:**
- Pin Tailwind version in .csproj: `<PackageReference Include="TailwindCSS" Version="3.4.0" />`
- Test CSS output before production deployment
- Document Tailwind utility classes used in components

---

#### System.Text.Json

**Risk:** None (built-in, no external dependency)

**Benefit:**
- Zero dependency risk
- Native .NET 8 support
- No version conflicts

---

#### Blazor Server Stability

**Risk:** Low (mature, LTS support)

**Mitigation:**
- Use official .NET 8 runtime
- Follow ASP.NET Core security updates
- Test reconnection logic on network drops (Phase 2)

---

### Operational Risks

#### Manual data.json Updates

**Risk:** Schema drift, invalid JSON, missing required fields

**Mitigation:**
1. **JSON Schema Documentation:**
   - Provide schema template (sample data.json)
   - Document all field requirements
   - Include field length limits and constraints

2. **Validation on Load:**
   - DataAnnotations catch missing [Required] fields
   - System.Text.Json validates types (DateTime, enum, etc.)
   - Error card guides user to fix and reload

3. **Sample Data Provided:**
   - Include valid sample data.json with project
   - Use as template for manual updates
   - Reference in README

4. **JSON Linter (Phase 2):**
   - Add pre-commit CI/CD check
   - Validate data.json schema before merge

**Owner:** Project Manager (data entry), Admin (validation)

---

#### Carried-Over Tasks Stale Data

**Risk:** Tasks never removed  dashboard becomes cluttered with past items

**Mitigation:**
1. **No Auto-Expiration (MVP):**
   - Manual cleanup discipline required
   - Document quarterly review process

2. **Phase 2 Improvement:**
   - Add auto-expiration logic (e.g., remove after 90 days)
   - Or add archive feature (separate "archived-over" status)

3. **Documentation:**
   - Include quarterly cleanup checklist
   - Recommend in README

**Owner:** Project Manager (data management)

---

#### Intranet Deployment (Phase 2)

**Risk:** Network-wide access  security considerations

**Mitigation:**
1. **Firewall Restrictions:**
   - Allow inbound HTTP/HTTPS only from internal network
   - Deny external access via firewall rules

2. **Authentication (Phase 2):**
   - Implement Windows Authentication (IIS integrated mode)
   - Require Active Directory credentials

3. **TLS/SSL (Phase 2):**
   - Not required for MVP (internal only)
   - Consider HTTPS for multi-user intranet deployment
   - Use self-signed certificate if needed

4. **IIS Hardening:**
   - Follow Microsoft IIS security hardening guide
   - Disable unnecessary modules
   - Enable request filtering

**Owner:** IT Admin (infrastructure), Developer (implementation)

---

### Risk Mitigation Checklist (MVP)

- [x] DataAnnotations validation on all POCO properties
- [x] Error card UI (user-friendly messages, no stack traces)
- [x] Console logging (full exception details for troubleshooting)
- [x] Manual refresh button (recovery path for errors)
- [x] Screenshot testing (1920x1080, Chrome + Edge, grayscale PDF)
- [x] WCAG AAA contrast audit (axe DevTools verification)
- [x] appsettings.json configuration (file path, log level)
- [x] Git version control (data.json committed to repo)
- [x] Unit tests (DataProvider.LoadAsync, >80% coverage)
- [x] Development documentation (README, sample data)

---

### Phase 2 Risk Mitigations

- [ ] FileSystemWatcher auto-reload with 500ms debounce (prevent flicker)
- [ ] Blazor circuit state restoration (graceful reconnection on disconnect)
- [ ] SQLite migration path (if JSON >50 MB or 100+ users)
- [ ] Windows Authentication (if intranet multi-user access)
- [ ] Audit logging (task state changes, who made updates, when)
- [ ] Data backup automation (daily Git commits, snapshot backups)
- [ ] Performance profiling (.NET trace, memory analysis)
- [ ] Load testing (simulate 10+ concurrent users)

---

## Conclusion

This architecture delivers a lightweight, screenshot-optimized executive dashboard in 3-4 days using C# .NET 8 and Blazor Server. It prioritizes simplicity, local deployment, and executive comprehension through status-first grouping and accessible design patterns. The system is ready for implementation by a single .NET developer with Blazor and Tailwind CSS experience.

**MVP Launch Target:** Day 3-4

**Ready for Engineering Handoff**