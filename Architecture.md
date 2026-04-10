# Architecture

## Overview & Goals

AgentSquad.Reporting is a lightweight, single-page executive dashboard built with C# .NET 8 and Blazor Server for local-only deployment. The architecture prioritizes simplicity, screenshot-optimized rendering, and minimal operational overhead by eliminating authentication, database requirements, and cloud infrastructure dependencies.

**Primary Goals:**
- Simplify executive visibility into project status via unified dashboard
- Enable rapid PowerPoint screenshot capture for executive presentations
- Reduce operational overhead (no auth, no database, no cloud)
- Standardize milestone communication with consistent visual format
- Minimize time-to-deployment (<5 minutes: copy + run)

**Architecture Principles:**
- Read-only data consumption (no write operations to data.json)
- Single-writer, multi-reader file access pattern
- Event-driven refresh with fallback polling
- Server-side rendering (SSR) for minimal WebSocket overhead
- Pixel-perfect rendering for 1920x1080 PowerPoint screenshots

---

## System Components

### DashboardDataService (Singleton Service)
**Responsibility:** Load, validate, cache, and refresh project data from JSON file. Monitor file changes. Notify subscribers of data updates.

**Public Interface:**
```csharp
public interface IDashboardDataService
{
    DashboardData CurrentData { get; }
    bool IsValid { get; }
    string? LastErrorMessage { get; }
    event EventHandler? DataRefreshed;
    Task InitializeAsync();
    Task RefreshAsync();
    Task<bool> IsDataValidAsync();
}
```

**Dependencies:**
- `IWebHostEnvironment` (injected) — determines wwwroot path
- `ILogger<DashboardDataService>` — optional logging
- `System.Text.Json` — JSON deserialization
- `System.IO.FileSystemWatcher` — file change detection
- `System.Timers.Timer` — polling fallback

**Data Owned:**
- `_currentData: DashboardData` — last valid loaded data (in-memory cache)
- `_previousData: DashboardData` — prior state for resilience on parse failure
- `_dataPath: string` — full path to data.json
- `_isValid: bool` — validation state flag

**Behavior:**
- Singleton lifetime (registered in DI container)
- Load JSON on construction via `InitializeAsync()`
- FileSystemWatcher detects changes <100ms
- Polling timer fires every 30s as fallback
- `RefreshAsync()` re-reads file, re-parses JSON, validates schema, updates cache
- On parse failure: log error, retain previous data, set error flag, raise `DataRefreshed` event
- Implements `IAsyncDisposable` for cleanup

---

### Dashboard.razor (Main Page)
**Responsibility:** Render complete dashboard layout, coordinate child components, subscribe to data refresh events.

**Public Interface:**
```csharp
@page "/"
@rendermode static
@inherits LayoutComponentBase
@implements IAsyncDisposable
```

**Dependencies:**
- `IDashboardDataService` (injected)
- `StatusCards.razor` (child component)
- `MilestoneTimeline.razor` (child component)
- `StatusChart.razor` (child component)

**Data Owned:**
- Reference to `IDashboardDataService.CurrentData` (read-only, cascaded to children)
- Subscription to `DataRefreshed` event

**Responsibilities:**
- Render Bootstrap 12-column grid layout
- Display project header (name, ID, last updated timestamp)
- Render three status card columns (completed/in-progress/carried)
- Render milestone timeline section
- Render status metrics bar chart
- Subscribe to `DataRefreshed` event → call `StateHasChanged()` for full re-render
- Apply print-media CSS for PowerPoint screenshot optimization

---

### StatusCards.razor (UI Component)
**Responsibility:** Render three status metric cards displaying item counts and item lists.

**Parameters:**
```csharp
[Parameter] public DashboardData Data { get; set; } = null!;
[Parameter] public EventCallback OnItemClicked { get; set; }
```

**Dependencies:**
- `DashboardData` parameter (from Dashboard.razor)
- Bootstrap CSS classes (success/warning/danger)

**Responsibilities:**
- Iterate `Data.Completed`, `Data.InProgress`, `Data.CarriedOver`
- Render card headers with status name and item count
- Render item lists (title, description, owner, date)
- Apply color coding via Bootstrap classes
- Limit visible items to 5-20 per card; enable scrolling if >20
- Sort: Completed DESC (newest first), InProgress ASC (oldest first), CarriedOver ASC (oldest first)

---

### MilestoneTimeline.razor (UI Component)
**Responsibility:** Render milestone Gantt chart as custom SVG.

**Parameters:**
```csharp
[Parameter] public List<Milestone>? Milestones { get; set; }
[Parameter] public int Height { get; set; } = 300;
[Parameter] public string ThemeColor { get; set; } = "dark";
```

**Dependencies:**
- `List<Milestone>` parameter
- HTML5 SVG (no JavaScript required)
- Custom CSS for SVG styling

**Responsibilities:**
- Calculate SVG coordinates (x=timeline axis, y=milestone rows)
- Render each milestone as horizontal bar (SVG `<rect>`)
- Color code by status: Completed (green), On Track (blue), At Risk (yellow), Delayed (red)
- Label with milestone name and target date (e.g., "Apr 15, 2024")
- Support 10+ milestones without layout overflow
- Maintain aspect ratio at 1920x1080 and 1280x720 resolutions
- Pixel-perfect rendering for PowerPoint screenshots

---

### StatusChart.razor (UI Component)
**Responsibility:** Render horizontal bar chart showing status item counts via ChartJS.

**Parameters:**
```csharp
[Parameter] public DashboardData Data { get; set; } = null!;
[Parameter] public bool AnimationEnabled { get; set; } = false;
```

**Dependencies:**
- `CurrieTechnologies.Razor.ChartJS` NuGet (v4.1.0+)
- `DashboardData` parameter
- JavaScript interop for chart initialization

**Data Owned:**
- `chart: Chart` instance (holds rendered chart state)
- `chartCanvas: ElementReference` (DOM reference)

**Responsibilities:**
- Extract counts: `Data.Completed.Count`, `Data.InProgress.Count`, `Data.CarriedOver.Count`
- Initialize ChartJS horizontal bar chart (type: "bar", indexAxis: "y")
- Set colors: green (completed), orange/yellow (in-progress), red (carried)
- Render chart on component init (`OnInitializedAsync()`)
- Update chart on data change (`OnParametersSetAsync()`)
- Disable animations (static rendering for screenshots)
- Display count labels on bars

---

### Data Models
**DashboardData.cs** — Root aggregate
```csharp
public class DashboardData
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<StatusItem> Completed { get; set; } = new();
    public List<StatusItem> InProgress { get; set; } = new();
    public List<StatusItem> CarriedOver { get; set; } = new();
    
    public int CompletedCount => Completed?.Count ?? 0;
    public int InProgressCount => InProgress?.Count ?? 0;
    public int CarriedOverCount => CarriedOver?.Count ?? 0;
    public int TotalCount => CompletedCount + InProgressCount + CarriedOverCount;
}
```

**Milestone.cs**
```csharp
public class Milestone : IComparable<Milestone>
{
    public string Name { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public bool Completed { get; set; }
    public string Status { get; set; } = "On Track"; // "On Track", "At Risk", "Delayed", "Completed"
    public int CompareTo(Milestone? other) => TargetDate.CompareTo(other?.TargetDate);
}
```

**StatusItem.cs**
```csharp
public class StatusItem : IComparable<StatusItem>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime AddedDate { get; set; }
    public string Owner { get; set; } = string.Empty;
    public int CompareTo(StatusItem? other) => AddedDate.CompareTo(other?.AddedDate);
}
```

---

## Component Interactions

### Data Flow: App Startup (UC1)
1. ASP.NET Core host starts → DI container injects `IDashboardDataService` singleton
2. `DashboardDataService.InitializeAsync()` called
3. Load `data.json` from wwwroot → parse via `System.Text.Json.JsonSerializer.Deserialize<DashboardData>()`
4. Validate against `data.schema.json` schema (required fields check)
5. If valid: cache in `_currentData`; if invalid: log error, retain empty `_previousData`, set `IsValid=false`
6. Initialize FileSystemWatcher (watches wwwroot/data.json for LastWrite changes)
7. Start polling timer (fires RefreshAsync every 30s)
8. Kestrel web server listens on localhost:7001
9. Browser requests `/` → Dashboard.razor renders (SSR static mode)
10. Dashboard injects `IDashboardDataService`, reads `CurrentData`, passes to child components
11. Child components render (StatusCards, MilestoneTimeline, StatusChart)

### Data Flow: Page View (UC2)
1. Browser loads `https://localhost:7001`
2. Dashboard.razor rendered server-side (static, no WebSocket)
3. Project header displays: ProjectName, ProjectId, LastUpdated timestamp
4. StatusCards renders: Completed (green), InProgress (yellow), CarriedOver (red) with counts and item lists
5. MilestoneTimeline renders: SVG with 10+ milestone bars, status colors, date labels
6. StatusChart renders: ChartJS horizontal bar chart
7. All HTML returned to browser (no interactive state needed)

### Data Flow: JSON File Change (UC3)
1. Project manager edits `wwwroot/data.json`
2. FileSystemWatcher fires `Changed` event (<100ms)
3. `DashboardDataService.RefreshAsync()` triggered
4. Re-read `data.json` from disk
5. Re-parse JSON → validate schema
6. If valid: update `_currentData`, set `IsValid=true`; if invalid: log error, retain `_previousData`, set `IsValid=false`
7. Raise `DataRefreshed` event
8. Dashboard.razor subscribed to `DataRefreshed` → calls `StateHasChanged()`
9. Dashboard re-renders children with updated `CurrentData`
10. Browser receives updated HTML via WebSocket (Blazor Server interop)
11. UI updates with new data
12. If 30s polling timer also fires: de-duplicates refresh (hash comparison prevents redundant parse)

### Data Flow: JSON Parse Error (UC4)
1. File changed event → `RefreshAsync()` called
2. `JsonSerializer.Deserialize()` throws `JsonException` (malformed JSON)
3. Catch exception: log error with file path and exception details
4. Retain `_previousData` in cache
5. Set `IsValid=false`, `LastErrorMessage="Invalid JSON format"`
6. Raise `DataRefreshed` event (fire even on error)
7. Dashboard re-renders: display **alert warning**: "Data file error at HH:MM. Displaying cached data."
8. Dashboard still shows previous valid data (zero data loss)
9. Project manager fixes JSON file → FileSystemWatcher triggers refresh
10. If new JSON valid: `_currentData` updates, `IsValid=true`, alert dismissed

### Component Interaction Diagram
```
Browser Request (/)
    ↓
Dashboard.razor (static SSR)
    ├→ Injects IDashboardDataService
    ├→ Reads CurrentData
    ├→ Renders child components
    │
    ├→ StatusCards.razor
    │   └→ Displays Completed/InProgress/CarriedOver lists (green/yellow/red)
    │
    ├→ MilestoneTimeline.razor
    │   └→ Renders SVG Gantt with milestone bars
    │
    └→ StatusChart.razor
        └→ Renders ChartJS horizontal bar chart

DashboardDataService (Singleton)
    ├→ Loads data.json at startup
    ├→ FileSystemWatcher monitors file changes
    ├→ Polling timer (30s fallback)
    ├→ Caches CurrentData in memory
    ├→ Fires DataRefreshed event on refresh
    └→ Handles JSON parse errors (retains previous data)

Data Flow on File Change:
FileSystemWatcher → RefreshAsync() → Parse JSON → Validate → Update cache → DataRefreshed event → Dashboard.StateHasChanged() → Re-render children → Browser HTML update
```

---

## Data Model

### Storage Strategy
- **Location**: `wwwroot/data.json` (embedded in published app)
- **Format**: UTF-8 JSON with ISO 8601 UTC timestamps
- **Size Limit**: <1MB (supports ~1000 total items)
- **File Permissions**: Read-only in production; writable by project manager during edits
- **Backup**: Manual file copy; no automatic versioning
- **Concurrency**: Single-writer, multi-reader safe via OS-level file locks

### JSON Schema (`data.schema.json`)
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["projectName", "projectId", "lastUpdated", "milestones", "completed", "inProgress", "carriedOver"],
  "properties": {
    "projectName": { "type": "string", "minLength": 1, "maxLength": 200 },
    "projectId": { "type": "string", "pattern": "^[A-Z0-9\\-]+$", "minLength": 1, "maxLength": 50 },
    "lastUpdated": { "type": "string", "format": "date-time" },
    "milestones": {
      "type": "array",
      "maxItems": 20,
      "items": {
        "required": ["name", "targetDate", "completed", "status"],
        "properties": {
          "name": { "type": "string", "minLength": 1, "maxLength": 100 },
          "targetDate": { "type": "string", "format": "date-time" },
          "completed": { "type": "boolean" },
          "status": { "enum": ["On Track", "At Risk", "Delayed", "Completed"] }
        }
      }
    },
    "completed": {
      "type": "array",
      "maxItems": 500,
      "items": {
        "required": ["title", "description", "addedDate", "owner"],
        "properties": {
          "title": { "type": "string", "minLength": 1, "maxLength": 200 },
          "description": { "type": "string", "maxLength": 500 },
          "addedDate": { "type": "string", "format": "date-time" },
          "owner": { "type": "string", "minLength": 1, "maxLength": 100 }
        }
      }
    },
    "inProgress": {
      "type": "array",
      "maxItems": 500,
      "items": {
        "required": ["title", "description", "addedDate", "owner"],
        "properties": {
          "title": { "type": "string", "minLength": 1, "maxLength": 200 },
          "description": { "type": "string", "maxLength": 500 },
          "addedDate": { "type": "string", "format": "date-time" },
          "owner": { "type": "string", "minLength": 1, "maxLength": 100 }
        }
      }
    },
    "carriedOver": {
      "type": "array",
      "maxItems": 500,
      "items": {
        "required": ["title", "description", "addedDate", "owner"],
        "properties": {
          "title": { "type": "string", "minLength": 1, "maxLength": 200 },
          "description": { "type": "string", "maxLength": 500 },
          "addedDate": { "type": "string", "format": "date-time" },
          "owner": { "type": "string", "minLength": 1, "maxLength": 100 }
        }
      }
    }
  }
}
```

### Data Validation Rules
| Field | Constraint | Purpose |
|-------|-----------|---------|
| `projectName` | 1-200 chars, non-empty | Prevent DOS via oversized input |
| `projectId` | Alphanumeric + hyphens, 1-50 chars | Block special chars (SQLi, XSS prep) |
| `lastUpdated` | ISO 8601 UTC (format: YYYY-MM-DDTHH:MM:SSZ) | Ensure consistent timestamp parsing |
| `milestones[*]` | Max 20 items, all required fields present | Prevent timeline rendering errors |
| `completed[*]` | Max 500 items, title + owner required | Limit card rendering overhead |
| `inProgress[*]` | Max 500 items, title + owner required | Limit card rendering overhead |
| `carriedOver[*]` | Max 500 items, title + owner required | Limit card rendering overhead |

### Entity Relationships
```
DashboardData (1)
  ├─ 1..* Milestones (ordered by TargetDate)
  ├─ 0..500 StatusItems (Completed, ordered by AddedDate DESC)
  ├─ 0..500 StatusItems (InProgress, ordered by AddedDate ASC)
  └─ 0..500 StatusItems (CarriedOver, ordered by AddedDate ASC)
```

---

## API Contracts

### IDashboardDataService Interface
```csharp
public interface IDashboardDataService
{
    /// <summary>Get current dashboard data (cached in memory).</summary>
    DashboardData CurrentData { get; }

    /// <summary>Flag indicating if last load succeeded.</summary>
    bool IsValid { get; }

    /// <summary>Error message from last failed load (null if valid).</summary>
    string? LastErrorMessage { get; }

    /// <summary>Fired when data refreshes from file.</summary>
    event EventHandler? DataRefreshed;

    /// <summary>Load data.json and initialize file watcher + polling timer.</summary>
    Task InitializeAsync();

    /// <summary>Re-read and parse data.json; raises DataRefreshed event.</summary>
    Task RefreshAsync();

    /// <summary>Validate current data against schema; returns true if valid.</summary>
    Task<bool> IsDataValidAsync();
}
```

### Component Parameter Contracts
**Dashboard.razor** — No input parameters; injected `IDashboardDataService`
```csharp
[Inject] IDashboardDataService DataService { get; set; } = null!;
private void OnDataRefreshed(object? sender, EventArgs e) { StateHasChanged(); }
```

**StatusCards.razor**
```csharp
[Parameter] public DashboardData Data { get; set; } = null!;
[Parameter] public EventCallback OnItemClicked { get; set; }
```

**MilestoneTimeline.razor**
```csharp
[Parameter] public List<Milestone>? Milestones { get; set; }
[Parameter] public int Height { get; set; } = 300;
[Parameter] public string ThemeColor { get; set; } = "dark";
```

**StatusChart.razor**
```csharp
[Parameter] public DashboardData Data { get; set; } = null!;
[Parameter] public bool AnimationEnabled { get; set; } = false;
```

### Error Response Models
**ValidationError**
```csharp
public class ValidationError
{
    public string Field { get; set; } // e.g., "projectName"
    public string Message { get; set; } // e.g., "Required field missing"
    public string? Constraint { get; set; } // e.g., "minLength"
}
```

**ServiceError**
```csharp
public class ServiceError
{
    public int Code { get; set; } // 1=FileNotFound, 2=InvalidJson, 3=ValidationFailed
    public string Message { get; set; }
    public string? StackTrace { get; set; } // Debug only
    public List<ValidationError>? Errors { get; set; }
}
```

### Logging Contracts
**Log Level Strategy**
| Level | Event | Action |
|-------|-------|--------|
| DEBUG | File watcher event fired; JSON parsed | Trace only (development) |
| INFO | App startup; data loaded; refresh completed | Log to console/file |
| WARN | JSON validation failed; file lock detected | Alert project manager |
| ERROR | JSON parse error; file not found; exception | Log with stack trace; retain data |

**Sample Log Events**
```
[INFO] DashboardDataService initialized. DataPath: wwwroot/data.json
[INFO] File change detected: data.json (LastWrite)
[INFO] Data refreshed successfully. Items: Completed=15, InProgress=8, CarriedOver=3
[WARN] JSON validation failed: missing field 'projectId'
[ERROR] Failed to parse data.json: Unexpected character 'x' at line 5, col 12
[ERROR] File watcher error: Access denied to wwwroot/data.json
[INFO] Polling timer triggered; no file changes detected (hash unchanged)
```

---

## Infrastructure Requirements

### Hosting Environment

#### Development
- **OS**: Windows 10+, macOS 11+, or Linux (Ubuntu 20.04+)
- **Runtime**: .NET 8.0.0+ SDK
- **IDE**: Visual Studio 2022 v17.4+ or VS Code + C# Dev Kit
- **Port**: localhost:5001 (HTTPS), localhost:5000 (HTTP, default)
- **Launch**: `dotnet run`

#### Production
- **OS**: Windows Server 2019+ or Linux (Ubuntu 20.04 LTS+)
- **Runtime**: .NET 8.0.0+ (self-contained .exe or framework-dependent)
- **Port**: 7001 (configurable via `appsettings.json`)
- **User**: Local admin or service account with read/write access to wwwroot
- **Launch**: Copy published folder + execute `AgentSquad.Reporting.exe` (Windows) or `./AgentSquad.Reporting` (Linux)

### Networking

#### Connectivity
- **Local Machine Only** (default): App listens on `127.0.0.1:7001`
- **Internal Network**: Bind to machine IP or hostname (e.g., `192.168.1.100:7001`)
- **Firewall Rules**: Allow inbound TCP 7001 from trusted subnet only
- **No Cloud Exposure**: No CDN, no load balancer, no public IP

#### Protocol Configuration
**HTTP (Default)**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://localhost:7001" }
    }
  }
}
```

**HTTPS (Optional)**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:7001",
        "Certificate": { "Path": "/path/to/cert.pfx", "Password": "password" }
      }
    }
  }
}
```

### Storage & File System

#### Directory Layout
```
C:\AgentSquad\Reporting\
├── AgentSquad.Reporting.exe
├── appsettings.json
├── appsettings.Production.json
├── wwwroot/
│   ├── data.json (read-write; ~100KB typical)
│   ├── data.schema.json (read-only; 3KB)
│   ├── index.html
│   ├── css/
│   │   ├── app.css (~50KB)
│   │   └── bootstrap.min.css (~30KB, optional if self-hosting CDN)
│   └── js/ (minimal; ChartJS interop only)
└── logs/ (optional)
    └── dashboard-20240410.log
```

#### Data File Handling
- **Path Resolution**: `Path.Combine(env.WebRootPath, "data.json")`
- **Encoding**: UTF-8 with BOM
- **Line Endings**: LF or CRLF (auto-handled by System.Text.Json)
- **Max Size**: <1MB; soft limit ~500 status items per category
- **Permissions**: Owner=Read/Write, Others=Read (or Read-only post-setup)
- **Backup**: Manual via script; recommend hourly/daily snapshots to network share

#### Disk Space Requirements
| Component | Size |
|-----------|------|
| .NET 8 Runtime (self-contained) | ~200MB |
| Application binaries | ~50MB |
| Static files (CSS, JS) | ~5MB |
| data.json (1000 items max) | ~500KB |
| **Total** | **~255MB** |

### Resource Allocation

#### CPU
- **Typical**: Single-core sufficient
- **Peak**: File parsing at refresh (<100ms for 1000 items)
- **Idle**: <5% utilization
- **Recommendation**: 1-2 CPU cores minimum

#### Memory
- **Application**: 200-300MB baseline
- **Data Cache**: 20-50MB (1000 items)
- **File Watcher**: <5MB
- **ChartJS Rendering**: 50-100MB (transient)
- **Total**: 300-450MB typical; 500MB recommended

#### I/O
- **Data File Read**: <50ms (sequential read, 100KB file)
- **JSON Deserialization**: <100ms (1000 items)
- **Polling Interval**: 30 seconds (minimal disk impact)
- **File Watcher**: Event-driven (<1ms latency)

### CI/CD Pipeline

#### Build Workflow (`.github/workflows/build.yml`)
```yaml
name: Build & Test
on: [push, pull_request]
jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build -c Release
      - run: dotnet test --logger "trx"
      - run: dotnet publish -c Release -r win-x64 --self-contained
      - uses: actions/upload-artifact@v3
        with:
          name: AgentSquad.Reporting-win-x64
          path: AgentSquad.Reporting/bin/Release/net8.0/win-x64/publish/
```

#### Deployment Checklist
- [ ] Code review approved
- [ ] Unit tests passing (>80% coverage on DashboardDataService)
- [ ] Screenshot tests at 1920x1080 passing (Chrome, Firefox, Edge)
- [ ] No unhandled exceptions in logs
- [ ] Publish: `dotnet publish -c Release -r win-x64 --self-contained`
- [ ] Copy `publish/` folder to deployment location
- [ ] Execute `AgentSquad.Reporting.exe`
- [ ] Verify browser opens to `https://localhost:7001`
- [ ] Verify all dashboard elements visible (header, cards, timeline, chart)

### Monitoring & Logging

#### Log Output Destinations
- **Development**: Console (stdout)
- **Production**: Console + optional file (`logs/dashboard-YYYY-MM-DD.log`)

#### Metrics to Monitor
| Metric | Target | Alert Threshold |
|--------|--------|-----------------|
| Page Load Time | <1000ms | >2000ms |
| File Refresh Latency | <2000ms | >5000ms |
| Memory Usage | 300-450MB | >800MB |
| Error Rate | 0% | >0% |
| Data Validity | 100% | <100% |

#### Error Alerting
- Log errors to console + file
- Display warning banner in UI if `IsValid == false`
- Retain last-known-good data in memory cache
- No automatic email/Slack alerts (out of scope for MVP)

### Deployment Scenarios

#### Scenario A: Single Machine (Developer/Manager)
- Deploy self-contained .exe to `C:\AgentSquad\Reporting\`
- Create Windows Scheduled Task to restart app at midnight (optional)
- Project manager edits `data.json` via Notepad; dashboard auto-refreshes

#### Scenario B: Internal Network (Team of Executives)
- Deploy to shared folder: `\\fileserver\AgentSquad\Reporting`
- Install as Windows Service via `sc create` or NSSM
- Configure DNS: `reporting.internal.company.net` → server IP
- Executives bookmark `https://reporting.internal.company.net:7001`
- Project manager edits `data.json` via network share; all viewers see updates within 2s

#### Scenario C: Linux (Future)
```bash
dotnet publish -c Release -r linux-x64 --self-contained
# Or containerized:
docker build -t agentsquad-reporting:1.0 .
docker run -d -p 7001:7001 -v /data:/app/wwwroot agentsquad-reporting:1.0
```

### Disaster Recovery
- **RTO (Recovery Time Objective)**: <5 minutes (redeploy .exe + restore data.json)
- **RPO (Recovery Point Objective)**: Last backup of data.json (hourly recommended)
- **Backup Strategy**: Copy wwwroot/data.json to network share via scheduled script
- **Retention Policy**: Keep 30 days of daily snapshots

---

## Technology Stack Decisions

### Core Framework: Blazor Server (.NET 8)
**Choice**: Blazor Server (not WebAssembly, not ASP.NET Core MVC)

**Rationale**:
- Built-in Kestrel web server (no external dependencies)
- Server-side rendering (SSR) eliminates WebSocket overhead
- Strongly-typed C# models match JSON deserialization
- Component-based architecture (StatusCards, MilestoneTimeline, StatusChart)
- Built-in DI container for singleton DashboardDataService

**Trade-off**: Blazor Server WebSocket latency acceptable for read-only dashboard with <1s initial load target.

### JSON Deserialization: System.Text.Json
**Choice**: `System.Text.Json` (built-in .NET 8)

**Rationale**:
- Zero external dependencies
- Faster than Newtonsoft.Json
- AOT-friendly (future Blazor AOT support)
- Native UTC datetime parsing
- Strongly-typed DTOs (no dynamic objects)

**Configuration**:
```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter() }
};
var data = JsonSerializer.Deserialize<DashboardData>(json, options);
```

### File Monitoring: FileSystemWatcher + Polling
**Choice**: Hybrid strategy (FileSystemWatcher + 30-second polling)

**Rationale**:
- FileSystemWatcher detects file changes <100ms (fast user feedback)
- Polling fallback every 30s handles missed events, file locks, platform quirks
- 30s latency acceptable for executive dashboard (not real-time)
- No external dependencies (both built-in System.IO)

**Alternative Considered**: Hash-based change detection (compare file hash to avoid redundant parsing)

### Charting: ChartJS + Custom SVG
**Choice**: 
- ChartJS v4.1.0+ (status bar chart)
- Custom SVG (milestone Gantt timeline)

**Rationale**:
- ChartJS: Lightweight, clean defaults, widely adopted in Blazor projects
- SVG: Zero JavaScript overhead, screenshot-perfect rendering, no interop latency
- Combined approach minimizes Blazor Server WebSocket overhead

**Trade-off**: Manual SVG rendering for timeline (no D3.js complexity); if visualizations become complex, migrate to Plotly or D3.

### CSS Framework: Bootstrap 5.3 CDN
**Choice**: Bootstrap 5.3.0+ via CDN (with fallback option)

**Rationale**:
- Professional default styling, responsive grid (12-column)
- No build tool complexity (CDN via `<link>` tag)
- Rapid prototyping for MVP
- Custom print CSS overlays for PowerPoint optimization

**Fallback Strategy** (if CDN unavailable):
- Self-host Bootstrap CSS in `wwwroot/css/`
- Use Subresource Integrity (SRI) hash for security
- JavaScript fallback to load local CSS if CDN fails

### Server-Side Rendering: Blazor SSR (Static Mode)
**Choice**: Static SSR with `@rendermode static`

**Rationale**:
- Dashboard is read-only (no user interactions, buttons, forms)
- Eliminates WebSocket overhead (initial load <1s)
- Minimal JavaScript interop (only ChartJS deferred)
- Streaming rendering for progressive enhancement

**Rendering Order**:
1. Project header (name, ID, timestamp) — immediate
2. Status cards (completed/in-progress/carried) — stream
3. Milestone timeline (SVG) — stream
4. Status chart (ChartJS) — deferred load (lowest priority)

### Dependency Management
| Package | Version | Rationale | Pin Strategy |
|---------|---------|-----------|--------------|
| `CurrieTechnologies.Razor.ChartJS` | 4.1.0+ | ChartJS interop; actively maintained | Pin exact (v4.1.0); test on updates |
| `Bootstrap` | 5.3.0+ | CSS framework; CDN or self-hosted | CDN default; self-host fallback |
| `.NET` | 8.0.0+ | LTS release (support until Nov 2026) | Enforce latest 8.0.x patch in CI/CD |
| `xUnit` | 2.6.0+ | Unit testing (optional, post-MVP) | Pin major version (v2.x) |
| `Moq` | 4.18.0+ | Mocking for tests (optional) | Pin major version (v4.x) |

---

## Security Considerations

### Authentication & Authorization
**Current Stance**: None required (out of scope per PM spec)

**Rationale**:
- Local-only deployment (localhost or internal network)
- Read-only dashboard; no data modification
- Assumes trusted network and trusted users
- No PII or sensitive data stored (project names, owner names only)

**If Future Auth Needed (Post-MVP)**:
- **Windows Integrated Auth** (simplest for Windows Server): `builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);`
- **HTTP Basic Auth** (Linux/cross-platform): Requires HTTPS; single username/password in `appsettings.json` (not suitable for multiple users)

### Input Validation & Sanitization

#### JSON Schema Validation
```csharp
public async Task<bool> IsDataValidAsync()
{
    var schemaPath = Path.Combine(env.WebRootPath, "data.schema.json");
    var schema = JsonSchema.FromFile(schemaPath);
    var result = schema.Validate(_currentData);
    return result.IsValid;
}
```

#### Field-Level Validation
| Field | Validation | Purpose |
|-------|-----------|---------|
| `projectName` | Max 200 chars; non-empty | Prevent DOS |
| `projectId` | Alphanumeric + hyphens only | Block special chars (SQLi/XSS prep) |
| `owner` | Max 100 chars; HTML-encode on render | Prevent XSS |
| `description` | Max 500 chars; HTML-encode on render | Prevent XSS |

#### Data Sanitization Implementation
```csharp
// In Blazor component
@((MarkupString)HtmlEncode(item.Description))

private string HtmlEncode(string? text)
{
    if (string.IsNullOrEmpty(text)) return string.Empty;
    return System.Net.WebUtility.HtmlEncode(text);
}
```

### Data Protection at Rest
**Current Stance**: No encryption required

**Rationale**:
- JSON file stored on local/internal machine only
- No PII or sensitive credentials in file
- File permissions controlled via OS (NTFS/ext4)

**If Encryption Needed (Future)**:
- DPAPI (Data Protection API) for Windows
- File-level encryption: BitLocker (Windows) or dm-crypt (Linux)

### Data Protection in Transit
**Current Stance**: HTTP acceptable for localhost; HTTPS optional for internal network

**Configuration**:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://localhost:7001" },
      "Https": {
        "Url": "https://localhost:7001",
        "Certificate": { "Path": "/path/to/cert.pfx", "Password": "password" }
      }
    }
  }
}
```

### Cross-Site Request Forgery (CSRF)
**Not Applicable**: Dashboard is read-only (no POST/PUT/DELETE operations)

### Dependency Security
| Package | Risk | Mitigation |
|---------|------|-----------|
| `CurrieTechnologies.Razor.ChartJS` | Unmaintained library | Pin version; monitor advisories |
| `Bootstrap 5.3` (CDN) | Availability; man-in-the-middle | Self-host CSS; use SRI hash |
| `.NET 8.0` | Runtime vulnerabilities | Follow Microsoft patches monthly |

**Self-Hosted Bootstrap with SRI**:
```html
<link 
  rel="stylesheet" 
  href="/css/bootstrap.min.css"
  integrity="sha384-1BmE4kWBq78iYhFldwKuhfstqYDgo8AwQAZAs6WjsDSO1o1I7K8zEiN6LqxaZnL"
  crossorigin="anonymous">
```

---

## Scaling Strategy

### Vertical Scaling (Add Resources to Single Machine)

#### Phase 1: Current Capacity
- **CPU**: 2 cores; handles <1000 total status items
- **Memory**: 500MB; supports 10 concurrent viewers
- **Disk**: 500MB free space
- **Bottleneck**: JSON deserialization time, dashboard re-render latency

#### Phase 2: Increase Resources
```
Current          →  Scaled
1000 items       →  2000 items (add 500MB RAM, 1 CPU core)
10 viewers       →  25 viewers (no change; read-only scales linearly)
30s refresh      →  15s refresh (increase CPU; reduce polling latency)
```

#### Phase 3: Implement Caching Layer
```csharp
public class CachedDashboardDataService : IDashboardDataService
{
    private DashboardData? _cache;
    private DateTime _cacheExpiry;
    private const int CACHE_TTL_SECONDS = 60;

    public DashboardData CurrentData
    {
        get
        {
            if (_cache != null && DateTime.UtcNow < _cacheExpiry)
                return _cache; // Return cached copy
            
            _ = RefreshAsync();
            return _cache ?? new DashboardData();
        }
    }
}
```

### Horizontal Scaling (Multiple Machines)

#### Architecture: Read-Only Replica Pattern
```
Shared Network Drive (NFS/SMB)
  └─ data.json (single source of truth)
     ├─ reporting-vm1: AgentSquad.Reporting (reads data.json)
     ├─ reporting-vm2: AgentSquad.Reporting (reads data.json)
     └─ reporting-vm3: AgentSquad.Reporting (reads data.json)

Load Balancer (HAProxy / Windows NLB)
  └─ Distributes requests to vm1, vm2, vm3 (round-robin, sticky sessions off)
```

#### Mitigation: Stagger Polling Timers
- vm1: polling at 0s offset
- vm2: polling at 10s offset
- vm3: polling at 20s offset
- Prevents concurrent file lock contention

### Bottleneck Mitigation

#### Bottleneck 1: File I/O
| Scenario | Cause | Mitigation |
|----------|-------|-----------|
| Slow disk | Network drive latency | Local SSD; pre-cache on startup |
| File lock | Concurrent writes | Use OS-level file locking; queue refresh events |
| Large file (>1MB) | 1000+ items | Migrate to database (SQL Server/Cosmos) |

#### Bottleneck 2: JSON Deserialization
| Scenario | Cause | Mitigation |
|----------|-------|-----------|
| Parse lag (<100ms) | 1000 items | Use System.Text.Json (already chosen) |
| DateTime parsing | Timezone conversions | Pre-parse in DashboardDataService; cache |
| Memory spike | Duplicate deserialization | Singleton service (no duplication) |

#### Bottleneck 3: Browser Rendering
| Scenario | Cause | Mitigation |
|----------|-------|-----------|
| SVG timeline slow | 100+ milestones | Paginate milestones; lazy-load SVG |
| ChartJS lag | Large datasets | Aggregate data server-side; limit bars |
| Network latency | File download | Enable Gzip compression (built-in) |

### Performance Optimizations (Priority Order)
1. **Enable Gzip Compression** (30-50% reduction, free)
   ```csharp
   builder.Services.AddResponseCompression(options =>
   {
       options.Providers.Add<GzipCompressionProvider>();
   });
   ```

2. **Use Bootstrap Minified** (already done)
   ```html
   <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
   ```

3. **Enable Browser Caching** (static files, 1 year)
   ```csharp
   app.UseStaticFiles(new StaticFileOptions
   {
       OnPrepareResponse = ctx =>
       {
           ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
       }
   });
   ```

4. **Lazy-Load ChartJS** (defer until after DOM ready)
   ```html
   <script defer src="ChartJS"></script>
   ```

---

## Risks & Mitigations

### Risk Matrix

| Risk | Severity | Probability | Impact | Mitigation |
|------|----------|------------|--------|-----------|
| **FileSystemWatcher misses rapid file changes** | Low | Medium | Data stale <30s | Polling fallback (current); hash-based detection |
| **JSON file corruption (malformed JSON)** | Medium | Low | Dashboard blank; cached data retained | Schema validation + error logging; UI alert banner |
| **data.json file not found** | High | Low | 404 error; app crash | Check file exists on startup; handle FileNotFoundException |
| **Out-of-memory (1000+ items)** | Medium | Low | App crash; process killed by OS | Monitor memory; migrate to database when >500 items |
| **Blazor Server WebSocket timeout** | Low | Low | Browser disconnects; page blank | Built-in Blazor handling; user refreshes browser |
| **Bootstrap CDN unavailable** | Medium | Very Low | No styling; unstyled page | Self-host Bootstrap; CDN fallback via JavaScript |
| **ChartJS library incompatibility** | Low | Low | Chart doesn't render | Pin version 4.1.0; test on updates; fallback to SVG bars |
| **Timezone parsing errors** | Low | Medium | Wrong dates displayed | Store all dates UTC; parse in browser local tz |
| **Concurrent writes to data.json** | High | Low | Partial file read; corruption | Single-writer assumption; document clearly; warn users |
| **Security: Unauthorized access** | High | Very Low | Exec data exposed on network | Local-only deployment; firewall rules; trusted network |
| **Dependency on .NET 8 runtime** | Medium | Low | Can't run on older .NET | Use .NET 8 LTS; self-contained .exe avoids dependency |
| **Scope creep (export, multi-project)** | Medium | Medium | Timeline slippage | Deny out-of-scope features; document for v2 |

### Technical Risks: Deep Dives

#### Risk: FileSystemWatcher Unreliability
**Severity**: Low | **Probability**: Medium (on fast file writes)

**Root Cause**: Windows NTFS batches multiple file changes; FileSystemWatcher may fire once for rapid edits.

**Impact**: Dashboard stale <30s if file written 3+ times/minute (acceptable for executive dashboard).

**Mitigation**:
```csharp
// Current: polling every 30s
private void InitializePollingFallback()
{
    _pollTimer = new Timer(30000); // 30 seconds
    _pollTimer.Elapsed += async (s, e) => await RefreshAsync();
}

// Alternative: hash-based change detection
private string _lastFileHash = string.Empty;

public async Task RefreshAsync()
{
    try
    {
        var currentHash = ComputeHash(await File.ReadAllBytesAsync(_dataPath));
        if (currentHash == _lastFileHash) return; // No change
        
        _lastFileHash = currentHash;
        var json = await File.ReadAllTextAsync(_dataPath);
        _currentData = JsonSerializer.Deserialize<DashboardData>(json);
        DataRefreshed?.Invoke(this, EventArgs.Empty);
    }
    catch (Exception ex)
    {
        Logger.LogError("Refresh failed: {Error}", ex.Message);
    }
}

private string ComputeHash(byte[] data)
{
    using (var sha = System.Security.Cryptography.SHA256.Create())
    {
        return Convert.ToBase64String(sha.ComputeHash(data));
    }
}
```

#### Risk: JSON Deserialization Failures
**Severity**: Medium | **Probability**: Low (if PM edits JSON manually)

**Root Cause**: Malformed JSON (missing quotes, invalid dates, trailing commas).

**Impact**: `JsonException` thrown; dashboard blank (without mitigation); user must fix JSON and restart app.

**Mitigation** (current architecture):
```csharp
public async Task RefreshAsync()
{
    try
    {
        var json = await File.ReadAllTextAsync(_dataPath);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json);
        
        if (!await IsDataValidAsync())
            throw new InvalidOperationException("Schema validation failed");
        
        _currentData = deserialized;
        _isValid = true;
        _lastErrorMessage = null;
    }
    catch (JsonException ex)
    {
        _isValid = false;
        _lastErrorMessage = $"Invalid JSON: {ex.Message} at line {ex.LineNumber}";
        Logger.LogError("JSON parse failed: {Error}", ex.Message);
    }
    finally
    {
        DataRefreshed?.Invoke(this, EventArgs.Empty); // Fire even on error
    }
}
```

**UI Alert Banner** (Dashboard.razor):
```html
@if (DataService.IsValid == false)
{
    <div class="alert alert-warning alert-dismissible fade show" role="alert">
        <strong>Data Error:</strong> @DataService.LastErrorMessage
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>
}
```

#### Risk: Out-of-Memory (Data Bloat)
**Severity**: Medium | **Probability**: Low (1000+ items threshold)

**Root Cause**: 1000 status items = ~500KB JSON; parsed object graph = ~5-10MB memory; multiple refresh cycles without GC.

**Impact**: App consumes 500MB → 1GB; VM runs out of RAM; process killed by OS; dashboard unavailable.

**Mitigation** (MVP):
```csharp
// Monitor memory on startup
var memory = GC.GetTotalMemory(false);
if (memory > 800_000_000) // 800MB
{
    Logger.LogWarning("High memory usage: {MemoryMB}MB", memory / 1_000_000);
}

// Optional: explicit GC after refresh
public async Task RefreshAsync()
{
    // ... refresh logic ...
    GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
}
```

**Scaling Mitigation** (Future):
- Migrate JSON → SQL Server/SQLite when >500 items
- Implement pagination (show 50 items/page)
- Archive old data to separate file

#### Risk: Concurrent Writes to data.json
**Severity**: High | **Probability**: Low (single-writer assumption)

**Root Cause**: Project manager edits JSON while app reads; partial file lock.

**Impact**: Corrupted data, `JsonException`, dashboard stale/blank, potential data loss.

**Mitigation**:
1. **Enforce Single-Writer Discipline**
   - Document: "Only one user edits data.json at a time"
   - Use file-level access control (NTFS permissions)
   - Or: Version control (Git) with merge conflict resolution

2. **File Locking** (OS-level)
   ```csharp
   private bool IsFileLocked(string filePath)
   {
       try
       {
           using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
           {
               return false; // Not locked
           }
       }
       catch (IOException)
       {
           return true; // Locked by another process
       }
   }

   // In refresh: wait for file to be available
   while (IsFileLocked(_dataPath))
   {
       await Task.Delay(100); // Retry every 100ms
   }
   ```

3. **Atomic Writes** (PM-side)
   - Write to `data.json.tmp`
   - Atomic `File.Move(data.json.tmp, data.json)` (OS-level)
   - Prevents partial reads

### Dependency Risks

#### Risk: Bootstrap 5.3 CDN Unavailable
**Severity**: Medium | **Probability**: Very Low (CDN 99.9% uptime)

**Root Cause**: CDN outage, ISP blocks domain, restricted environment.

**Impact**: No CSS loaded; page renders unstyled (ugly but functional); dashboard still shows data.

**Mitigation**:
1. **Self-Host Bootstrap** (best)
   ```bash
   wget https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css
   ```
   
   ```html
   <link rel="stylesheet" href="/css/bootstrap.min.css">
   ```

2. **CDN Fallback** (JavaScript)
   ```html
   <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
   <script>
     if (!document.querySelector('link[href*="bootstrap"]').sheet) {
       var link = document.createElement('link');
       link.rel = 'stylesheet';
       link.href = '/css/bootstrap.min.css';
       document.head.appendChild(link);
     }
   </script>
   ```

#### Risk: ChartJS Library Incompatibility
**Severity**: Low | **Probability**: Low (actively maintained library)

**Root Cause**: Library outdated, major .NET upgrade (9.0), ChartJS v5 breaking changes.

**Impact**: Chart doesn't render, JavaScript errors, status metrics bar chart missing.

**Mitigation**:
1. **Pin NuGet Version**
   ```xml
   <PackageReference Include="CurrieTechnologies.Razor.ChartJS" Version="4.1.0" />
   ```

2. **Monitor Security Advisories**
   ```bash
   dotnet list package --vulnerable
   ```

3. **Fallback to HTML/CSS Chart**
   ```html
   <!-- If ChartJS fails, fall back to Bootstrap progress bars -->
   <div class="progress">
     <div class="progress-bar bg-success" style="width: 60%">Completed (15)</div>
   </div>
   ```

#### Risk: .NET 8 LTS Support Ending
**Severity**: Medium | **Probability**: Very Low (LTS = 3 years support until Nov 2026)

**Root Cause**: .NET 8.0 LTS support ends; security patches no longer available.

**Impact**: Unpatched vulnerabilities, compliance/audit issues.

**Mitigation**:
- Plan upgrade to .NET 9.0 / 10.0 LTS (Nov 2025)
- Security patching: Update .NET 8.0 monthly
- CI/CD pin: Enforce minimum .NET 8.0.15+ (latest patch)

```yaml
# .github/workflows/build.yml
- uses: actions/setup-dotnet@v3
  with:
    dotnet-version: '8.0.x' # Always latest 8.0.z
```

### Operational Risks

#### Risk: Timezone/Date Parsing Errors
**Severity**: Low | **Probability**: Medium (if dates manually edited)

**Root Cause**: Non-ISO 8601 dates (e.g., "April 10, 2024"), timezone ambiguity.

**Impact**: Wrong milestone dates displayed (off by hours/days), incorrect timeline position.

**Mitigation** (current):
```csharp
// Enforce ISO 8601 UTC in schema
"lastUpdated": {
  "type": "string",
  "format": "date-time",
  "pattern": "^\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}Z$"
}

// In C# deserialization
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter() }
};
var data = JsonSerializer.Deserialize<DashboardData>(json, options);

// Dates auto-convert to UTC; browser renders in local tz
var localDate = data.LastUpdated.ToLocalTime();
```

---

## Appendix: Summary Matrix

| Aspect | Detail |
|--------|--------|
| **Technology Stack** | C# .NET 8 Blazor Server, System.Text.Json, ChartJS 4.1.0+, Bootstrap 5.3 CDN |
| **Data Storage** | `data.json` in wwwroot; <1MB; JSON schema validation |
| **Hosting** | Kestrel on localhost:7001; self-contained .exe (255MB total) |
| **Performance Target** | <1s initial load; <2s file refresh; <500ms re-render |
| **Scalability Limit** | ~1000 total status items before database migration |
| **Memory Required** | 500MB recommended (300-450MB typical) |
| **CPU Required** | 1-2 cores minimum |
| **Deployment Time** | <5 minutes (copy + execute) |
| **Security** | No auth; local-only; read-only; trusted network |
| **Test Coverage** | >80% on DashboardDataService (xUnit + Moq) |
| **CI/CD** | GitHub Actions (build, test, publish) |
| **Monitoring** | Console + file logging; no external alerts |
| **Backup** | Manual daily snapshots to network share |
| **Disaster Recovery** | <5 min RTO; hourly RPO |
| **Documentation** | README (deployment, JSON schema, troubleshooting) |