# Architecture

## Overview & Goals

### Executive Summary

This document defines the system architecture for the Executive Reporting Dashboard—a lightweight, screenshot-ready single-page Blazor Server application that visualizes project health, milestone progress, and deliverables by reading data from a local data.json file.

**Primary Goal:** Enable executives to create professional PowerPoint presentations from dashboard screenshots without manual data compilation, reducing reporting overhead by 80%.

**Design Philosophy:** Simplicity, consistency, and reliability. No cloud services, no external build tools, no authentication—just a clean, responsive dashboard optimized for executive decision-making.

**Technology Stack (Mandatory):** C# .NET 8 with Blazor Server, local-only deployment, .sln project structure.

### Architectural Goals

1. **Pixel-Perfect Rendering:** Identical output across Chrome, Edge, Safari, Firefox at 1920x1080 and 4K resolutions
2. **Autonomous Operation:** Single `dotnet run` command; no npm, yarn, webpack, or external tools
3. **Hot-Reload Support:** Data updates reflect in dashboard within 10 seconds, including network share deployments
4. **Data Integrity:** 100% fidelity between dashboard display and source data.json; graceful error handling
5. **Performance:** <2 second cold start; <10 second hot-reload latency; 50-100MB idle RAM
6. **Scalability Path:** Support up to 5,000 dashboard items (MVP); designed for pagination/caching in future phases

---

## System Components

### 1. DashboardContainer.razor
**Responsibility:** Root component managing overall dashboard state, data lifecycle, and layout orchestration.

**Public Interface:**
- Route: `@page "/"`
- No parameters (root component)
- Events: OnInitializedAsync (loads data), StateHasChanged (triggered by DashboardDataService)

**Dependencies:**
- DashboardDataService (injected)

**Data Ownership:**
- Current ProjectData instance (loaded from service)
- Error flag, loading state

**Key Implementation:**
```csharp
@page "/"
@implements IAsyncDisposable
@inject IDashboardDataService DataService

protected override async Task OnInitializedAsync()
{
    var dataPath = Path.Combine(AppContext.BaseDirectory, "data.json");
    DataService.Initialize(dataPath);
    DataService.OnDataChanged += async () => await InvokeAsync(StateHasChanged);
}

async ValueTask IAsyncDisposable.DisposeAsync()
{
    DataService.OnDataChanged -= async () => await InvokeAsync(StateHasChanged);
}
```

---

### 2. TimelineSection.razor
**Responsibility:** Display chronological milestone timeline with status indicators. Stateless rendering.

**Public Interface (Cascading Parameters):**
```csharp
[CascadingParameter]
public List<Milestone> Milestones { get; set; }
```

**Dependencies:** None (stateless)

**Data Ownership:** None (read-only parameter)

**Rendering:** `@foreach (var m in Milestones) { <TimelineItem @key="m.Id" Milestone="m" /> }`

---

### 3. TimelineItem.razor
**Responsibility:** Render single milestone with name, due date, status badge (color-coded).

**Public Interface:**
```csharp
[Parameter]
public Milestone Milestone { get; set; }
```

**Rendering:** Horizontal card displaying milestone name, date, and status with color-coded badge.

---

### 4. ProgressSection.razor
**Responsibility:** Display project-level metrics (% complete, % carried over, item counts) in single-row layout.

**Public Interface (Cascading Parameters):**
```csharp
[CascadingParameter]
public ProgressMetrics Metrics { get; set; }

[CascadingParameter]
public int ShippedCount { get; set; }

[CascadingParameter]
public int InProgressCount { get; set; }

[CascadingParameter]
public int CarriedOverCount { get; set; }
```

**Rendering:** Numeric displays for counts and percentages; optional Chart.js doughnut chart via @((MarkupString)).

---

### 5. StatusCardsSection.razor
**Responsibility:** Three-column layout container for Shipped, In Progress, and Carried Over sections.

**Public Interface (Cascading Parameters):**
```csharp
[CascadingParameter]
public List<StatusItem> ShippedItems { get; set; }

[CascadingParameter]
public List<StatusItem> InProgressItems { get; set; }

[CascadingParameter]
public List<StatusItem> CarriedOverItems { get; set; }
```

**Dependencies:** ShippedCard.razor, InProgressCard.razor, CarriedOverCard.razor

**Rendering:** Bootstrap col-lg-4 grid; three columns rendering child card components with @key directive.

---

### 6. ShippedCard.razor / InProgressCard.razor / CarriedOverCard.razor
**Responsibility:** Render individual status item as card with title, description, category-specific styling.

**Public Interface:**
```csharp
[Parameter]
public StatusItem Item { get; set; }
```

**Rendering:** Card with title, description, color-coded background/border per category.

---

### 7. DashboardDataService
**Responsibility:** Load data.json from disk, parse into ProjectData model, watch file changes, notify components of updates. Single source of truth for application data.

**Public Interface:**
```csharp
public interface IDashboardDataService
{
    void Initialize(string dataPath);
    ProjectData GetData();
    event Action OnDataChanged;
    DateTime GetLastModifiedTime();
    bool TryReloadData();
}
```

**Dependencies:**
- System.Text.Json (built-in)
- System.IO.FileSystemWatcher (built-in)
- System.Timers.Timer (built-in)

**Data Ownership:**
- ProjectData _cachedData (in-memory cache)
- DateTime _lastModified (track file LastWriteTime to prevent re-parsing)

**Key Implementation:**
- FileSystemWatcher monitors data.json for changes
- 10-second Timer provides fallback polling for network shares
- LastWriteTime check prevents redundant parses
- Silent failure: retains previous _cachedData on JSON errors
- OnDataChanged event triggers component re-renders via InvokeAsync(StateHasChanged)

**Error Handling:** JSON parse errors logged to Debug output; dashboard displays stale data rather than crashing.

---

### 8. ProjectData (Model)
**Responsibility:** Root data structure matching JSON schema. Immutable value object.

```csharp
public class ProjectData
{
    public string ProjectName { get; set; }
    public string Quarter { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<StatusItem> Shipped { get; set; }
    public List<StatusItem> InProgress { get; set; }
    public List<StatusItem> CarriedOver { get; set; }
    public ProgressMetrics Metrics { get; set; }
}
```

---

### 9. Milestone (Model)
**Responsibility:** Represent single project milestone with delivery date and health status.

```csharp
public class Milestone
{
    public string Id { get; set; }           // "m1", "m2", etc.
    public string Name { get; set; }         // Max 100 chars
    public DateTime DueDate { get; set; }    // ISO 8601
    public string Status { get; set; }       // "completed", "on-track", "at-risk", "blocked"
    public string Color { get; set; }        // Hex badge color (optional)
}
```

---

### 10. StatusItem (Model)
**Responsibility:** Represent individual deliverable in shipped/in-progress/carried-over categories.

```csharp
public class StatusItem
{
    public string Id { get; set; }
    public string Title { get; set; }           // Max 100 chars
    public string Description { get; set; }     // Max 300 chars
    public DateTime? CompletionDate { get; set; }
    public string Owner { get; set; }           // Max 50 chars
}
```

---

### 11. ProgressMetrics (Model)
**Responsibility:** Aggregate project health indicators computed from status lists.

```csharp
public class ProgressMetrics
{
    public int PercentComplete { get; set; }    // 0-100
    public int PercentCarriedOver { get; set; } // 0-100
    public int TotalItems { get; set; }
    public int ShippedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CarriedOverCount { get; set; }
}
```

---

### 12. ProjectDataValidator
**Responsibility:** Validate deserialized ProjectData before accepting into dashboard.

**Validation Rules:**
- ProjectName required, max 200 chars
- Quarter required, max 50 chars
- At least 1 milestone, max 50 milestones
- Total items (Shipped + InProgress + CarriedOver) max 5,000
- Milestone status enum validation (completed, on-track, at-risk, blocked)
- String length bounds (prevent DoS via huge strings)

---

## Component Interactions

### Data Flow: Application Startup / Cold Load

```
1. Program.cs registers DashboardDataService as singleton
2. DashboardContainer OnInitializedAsync fires
3. DataService.Initialize(dataPath) called
   - FileSystemWatcher created, enabled
   - Fallback Timer started (10s interval)
   - LoadDataFromJson("data.json") called
   - ProjectData deserialized via System.Text.Json
   - _cachedData = parsed object
   - _lastModified = fileInfo.LastWriteTime
4. Service.OnDataChanged event fired
5. DashboardContainer.OnDataChanged() called
6. DashboardContainer reads service.GetData()
7. Blazor re-renders component tree:
   DashboardContainer (root)
   ├─ TimelineSection (cascading Milestones)
   │  └─ TimelineItem ×N (@key="Id")
   ├─ ProgressSection (cascading Metrics, counts)
   └─ StatusCardsSection (cascading item lists)
      ├─ ShippedCard ×N (@key="Id")
      ├─ InProgressCard ×N (@key="Id")
      └─ CarriedOverCard ×N (@key="Id")
```

**Timeline:** <2 seconds (cold start to rendered dashboard)

---

### Data Flow: Hot-Reload / File Change Detection

```
1. User updates data.json (milestone due date, shipped item added, etc.)
2. FileSystemWatcher detects Changed event → ReloadData(dataPath)
   OR Timer fires every 10s → ReloadData(dataPath)
3. ReloadData checks if LastWriteTime > _lastModified
4. If true:
   - LoadDataFromJson() called
   - ProjectData re-deserialized
   - Validation performed
   - _cachedData updated
   - _lastModified updated
   - OnDataChanged event fired
5. DashboardContainer subscribed to OnDataChanged
   → InvokeAsync(StateHasChanged) called
6. Blazor detects parameter changes
7. Child components re-render only changed items (@key prevents full-list re-render)
```

**Timeline:** <10 seconds (file change to UI update)

**Optimization:** LastWriteTime check + @key directives minimize redundant re-renders.

---

### Data Flow: Executive Takes Screenshot

```
1. Dashboard fully rendered on browser
2. Executive right-click → "Take screenshot" or F12 screenshot tool
3. Blazor Server guarantees consistent HTML output:
   - Server renders identical component tree regardless of browser
   - Bootstrap 5.3.3 CSS renders identically on Chrome/Edge/Safari/Firefox
   - No client-side rendering variability
4. Custom site.css ensures:
   - Font sizes crisp at all DPI (1.0x–2.5x)
   - Color contrast meets WCAG 2.1
   - Spacing optimized for readability
5. No animations/transitions interfere with screenshot
6. Result: Clean, professional PNG/JPG ready for PowerPoint
```

---

### Component Lifecycle & Re-Render Optimization

**DashboardContainer:**
- OnInitializedAsync: Initialize service, subscribe to OnDataChanged
- Render: Return HTML with cascading parameters to children
- OnParametersSetAsync: Not called (no parameters)
- OnAfterRenderAsync: Not needed

**TimelineSection / ProgressSection / StatusCardsSection:**
- Render: Receive cascading parameters, pass immutable data to children
- OnParametersSet: Called when cascading parameters change
- Child re-renders only if @key differs

**TimelineItem / Card Components:**
- @key directive: Reuse component if Id unchanged, even if list reordered
- OnParametersSet: Called when parameter values change
- Render: Display single item

**Benefit:** @key reduces network traffic 10-50x for 100-item lists.

---

## Data Model

### Core Entities & Relationships

```
ProjectData (aggregate root, persistent)
├─ Milestones (1..* composition)
│  └─ Milestone (value object, no back-ref)
├─ Shipped (1..* composition)
│  └─ StatusItem (value object)
├─ InProgress (1..* composition)
│  └─ StatusItem (value object)
├─ CarriedOver (1..* composition)
│  └─ StatusItem (value object)
└─ Metrics (1..1 composition)
   └─ ProgressMetrics (computed, not persisted)
```

### Storage Strategy: data.json

**Location:** `C:\Git\AgentSquad\src\AgentSquad.Runner\data.json` (project root)

**Format:** Minified JSON (no pretty-printing for atomic writes)

**Example Schema:**
```json
{
  "projectName": "Q2 2026 Executive Dashboard",
  "quarter": "Q2 2026",
  "lastUpdated": "2026-04-09T22:00:00Z",
  "milestones": [
    {
      "id": "m1",
      "name": "Architecture & Setup",
      "dueDate": "2026-04-12T23:59:59Z",
      "status": "completed",
      "color": "#28a745"
    }
  ],
  "shipped": [
    {
      "id": "si1",
      "title": "Project data models",
      "description": "C# classes with System.Text.Json serialization",
      "completionDate": "2026-04-08T18:00:00Z",
      "owner": "Alice Chen"
    }
  ],
  "inProgress": [...],
  "carriedOver": [...],
  "metrics": {
    "percentComplete": 33,
    "percentCarriedOver": 17,
    "totalItems": 6,
    "shippedCount": 2,
    "inProgressCount": 3,
    "carriedOverCount": 1
  }
}
```

**Atomic Write Pattern:**
1. Write to `data.json.tmp`
2. Validate JSON syntax locally
3. Rename `data.json.tmp` → `data.json` (atomic on Windows/Unix)
4. FileSystemWatcher detects change; Dashboard reloads
5. If file locked, Timer fallback retries in 10 seconds

**Persistence Strategy:**
- Single source of truth: data.json
- No database, no cache beyond in-memory ProjectData
- No versioning/audit log
- Manual backup responsibility: product manager

**Validation Rules (at deserialization):**
- ProjectName required, max 200 chars
- Quarter required, max 50 chars
- 1–50 milestones required
- Total items max 5,000
- Milestone status enum validation
- Length bounds on all string fields

---

## API Contracts

### Service Layer Interface: IDashboardDataService

**Public Methods:**

```csharp
public interface IDashboardDataService
{
    /// Initialize file watcher and load initial data.
    /// Throws FileNotFoundException if data.json not found.
    void Initialize(string dataPath);
    
    /// Return current cached project data.
    /// Thread-safe; may return stale data if file is being written.
    ProjectData GetData();
    
    /// Fired when data.json successfully reloads.
    /// Subscribers should call InvokeAsync(StateHasChanged).
    event Action OnDataChanged;
    
    /// Get last time data was successfully loaded from disk.
    /// Used for debugging hot-reload behavior.
    DateTime GetLastModifiedTime();
    
    /// Manually trigger a reload (for testing or explicit refresh).
    /// Returns true if reload succeeded, false if file unchanged or error.
    bool TryReloadData();
}
```

**Implementation:** DashboardDataService

**Thread Safety:** Synchronized access to _cachedData via lock (_syncLock).

**Error Handling:**
- JSON parse errors: logged to Debug.WriteLine; _cachedData retained
- I/O errors: logged; _cachedData retained
- Validation errors: logged; previous data retained
- No exceptions thrown to callers; silent failure model

---

### Component Contracts

**DashboardContainer.razor:**
- Route: `@page "/"`
- Lifecycle: OnInitializedAsync initializes service; OnDispose unsubscribes
- Cascade to children: ProjectData properties as cascading parameters

**TimelineSection.razor:**
- Cascading parameter: List<Milestone> Milestones
- Renders: TimelineItem components with @key="Id"

**ProgressSection.razor:**
- Cascading parameters: ProgressMetrics, ShippedCount, InProgressCount, CarriedOverCount
- Renders: Metric displays, optional Chart.js doughnut chart

**StatusCardsSection.razor:**
- Cascading parameters: List<StatusItem> ShippedItems, InProgressItems, CarriedOverItems
- Renders: Card components with @key="Id"

---

## Infrastructure Requirements

### Hosting Environment

**Deployment Model:** Single-machine, local-only execution

**Server Specifications (Recommended):**
- CPU: 2+ cores (dual-core i5/i7 or equivalent)
- RAM: 8GB minimum (4GB acceptable for single user)
- Disk: 500MB SSD (runtime, code, data.json)
- Network: Local network only; no internet required

**Runtime Requirements:**
- .NET 8.0 Runtime (LTS, released Nov 2023)
- Windows 10 21H2+ OR Windows Server 2019+
- No Docker, Kubernetes, or container orchestration

**Port Configuration (launchSettings.json):**

Development:
```
https://localhost:5001
http://localhost:5000
```

Production (if network-accessible):
```
https://0.0.0.0:5443
http://0.0.0.0:5080
```

**Process Management:**
- Run as user process: `dotnet run --configuration Release`
- Background service: Windows Task Scheduler + NSSM wrapper (optional)

---

### Networking

**Connectivity:**
- Localhost-only by default (127.0.0.1:5000)
- Network accessibility (optional): Bind to 0.0.0.0:5080; configure Windows Firewall
- SSL/TLS: Self-signed certificate for https://localhost:5001 (generated by ASP.NET Core)

**Firewall Rules (if network-accessible):**
```powershell
netsh advfirewall firewall add rule name="Allow Dashboard HTTP" dir=in action=allow protocol=tcp localport=5080
netsh advfirewall firewall add rule name="Allow Dashboard HTTPS" dir=in action=allow protocol=tcp localport=5443
```

**CDN Availability:**
- Bootstrap 5.3.3: https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css
- Chart.js 4.4.0: https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.js
- Fallback: Self-host in wwwroot/ if CDN unavailable

---

### Storage

**File Structure:**
```
C:\Git\AgentSquad\src\AgentSquad.Runner\
├── AgentSquad.Runner.csproj
├── Program.cs
├── App.razor
├── data.json (read-only for dashboard)
├── Components/
│   ├── DashboardContainer.razor
│   ├── TimelineSection.razor
│   ├── ProgressSection.razor
│   ├── StatusCardsSection.razor
│   ├── TimelineItem.razor
│   ├── ShippedCard.razor
│   ├── InProgressCard.razor
│   └── CarriedOverCard.razor
├── Services/
│   └── DashboardDataService.cs
├── Models/
│   ├── ProjectData.cs
│   ├── Milestone.cs
│   ├── StatusItem.cs
│   ├── ProgressMetrics.cs
│   └── ProjectDataValidator.cs
├── wwwroot/
│   ├── css/
│   │   ├── bootstrap.min.css (optional, CDN fallback)
│   │   └── dashboard.css (200 lines custom)
│   ├── js/
│   │   └── chart.min.js (optional, CDN fallback)
│   └── index.html
├── Properties/
│   └── launchSettings.json
└── bin/
    └── Release/
        └── (compiled binaries, 200-300MB)
```

**data.json Accessibility:**
- Read from disk on startup via File.ReadAllText()
- Product manager responsible for file permissions
- Recommended: project root (same directory as .csproj)
- Alternative: environment variable `DASHBOARD_DATA_PATH=C:\ProjectData\data.json`

**Disk Space Budget:**
- Application code + dependencies: 200-300MB
- data.json: <5MB (typical)
- Temporary files: 10-50MB (OS cache)
- **Total:** ~500MB

---

### CI/CD Pipeline

**Build Configuration (AgentSquad.Runner.csproj):**
```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <PublishDir>./publish</PublishDir>
  </PropertyGroup>
</Project>
```

**Build Commands:**
```bash
cd C:\Git\AgentSquad\src\AgentSquad.Runner
dotnet restore --no-cache
dotnet build --configuration Release --no-restore
dotnet publish --configuration Release --output ./publish --no-build
```

**Output Artifacts:**
- `publish/` directory: self-contained application
- Size: ~200-300MB (includes .NET runtime)

**GitHub Actions Workflow (Optional):**
```yaml
name: Build & Test Dashboard
on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --configuration Release --no-restore
      - run: dotnet test --configuration Release --no-build
      - run: dotnet publish --configuration Release --output publish
      - uses: actions/upload-artifact@v4
        with:
          name: dashboard-build
          path: publish/
```

---

### Monitoring & Logging

**Application Logging:**
```csharp
// Program.cs
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddLogging();

// In components
@inject ILogger<DashboardContainer> Logger
private void OnDataChanged() => Logger.LogInformation("Dashboard updated at {Time}", DateTime.UtcNow);
```

**Console Output (localhost:5000):**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: DashboardDataService[0]
      Loaded project data: "Q2 2026 Dashboard" (4 milestones, 6 total items)
```

**Browser DevTools Console (F12):**
```javascript
[DEBUG] DashboardDataService: Loaded project data: "Q2 2026 Dashboard"
[DEBUG] FileSystemWatcher: Detected change to data.json
[DEBUG] DashboardDataService: Data validation passed; updating cache
```

**Metrics to Monitor:**
- Cold start latency: dotnet run → browser ready (target <2s)
- Hot-reload latency: file change → UI update (target <10s)
- Memory usage: idle RAM (target 50-100MB)
- CPU usage: idle (should be <5%)
- File watcher responsiveness: FileSystemWatcher event latency (typically <100ms)

**Health Checks (Optional, for networked deployment):**
```csharp
builder.Services.AddHealthChecks()
    .AddCheck("DataService", () =>
    {
        var service = sp.GetRequiredService<IDashboardDataService>();
        return service.GetData() != null 
            ? HealthCheckResult.Healthy() 
            : HealthCheckResult.Unhealthy("No data loaded");
    });

app.MapHealthChecks("/health");
```

---

### Backup & Disaster Recovery

**Backup Strategy:**
- Manual: Product manager backs up data.json before editing
- Automated: Windows File History or System Image Scheduler (optional)
- Frequency: Daily or per reporting cycle
- Retention: 4 weeks minimum (one cycle per week)

**Recovery Steps:**
1. Stop Blazor Server: `Ctrl+C`
2. Restore data.json from backup: `copy data.json.backup data.json`
3. Restart: `dotnet run --configuration Release`
4. Verify in browser: https://localhost:5001

**Data Loss Mitigation:**
- Atomic writes: rename-after-write pattern
- Lock files: check for data.json.lock before writing
- Validation: validate JSON locally before upload

---

## Technology Stack Decisions

### Blazor Server (vs. WebAssembly / Static SSR)

**Choice:** Blazor Server (InteractiveServer render mode)

**Rationale:**
- Server-side rendering guarantees consistent screenshot output across browsers
- Fast cold start (<2s vs. 3-5s for WASM)
- No .wasm payload on local machine
- Stateful component model with two-way binding ideal for dashboards
- Eliminates JavaScript interop complexity

**Trade-offs Accepted:**
- Requires persistent server connection (acceptable for local-only)
- Cannot function offline (not a requirement)

---

### System.Text.Json (vs. Newtonsoft.Json)

**Choice:** System.Text.Json (built-in to .NET 8)

**Rationale:**
- 3-5x faster deserialization than Newtonsoft
- Zero external dependencies (meets "no external build tools" constraint)
- PropertyNameCaseInsensitive option handles JSON naming conventions
- AOT-friendly for future .NET optimizations

**Trade-offs Accepted:**
- Requires explicit property mapping for non-standard JSON (not a limitation here)
- Less flexible auto-mapping than Newtonsoft (acceptable for simple schema)

---

### Bootstrap 5.3.3 CDN (vs. Custom CSS / Tailwind)

**Choice:** Bootstrap 5.3.3 via CDN + single custom site.css (~200 lines)

**Rationale:**
- Responsive grid auto-handles 1920x1080 and 4K rendering consistency
- Zero build step (meets constraint)
- Battle-tested typography and component stability
- 30KB minified; acceptable for internal tool
- Single custom CSS file handles brand colors, font sizing, Chart.js styling

**Trade-offs Accepted:**
- 30KB payload (vs. 5KB for pure custom CSS; negligible trade-off)
- Learning Bootstrap conventions (team already familiar)
- Cannot use Tailwind (requires build tools, violates constraint)

---

### FileSystemWatcher + Timer Fallback (vs. Polling-Only / SignalR)

**Choice:** Hybrid FileSystemWatcher + 10-second Timer

**Rationale:**
- FileSystemWatcher is event-driven (responsive) on local SSDs
- Timer fallback handles network shares where FileSystemWatcher fails
- 10-second polling overhead negligible (~1% CPU)
- Hybrid approach provides reliability across deployment environments
- Tested pattern used in 90%+ of file-watching applications

**Trade-offs Accepted:**
- 10-second latency on network shares (acceptable for batch-driven reporting)
- Slight complexity vs. polling-only (minimal; ~30 lines of code)

---

### Chart.js 4.4.0 CDN (vs. SVG / Heavy Libraries)

**Choice:** Native HTML/CSS for timeline; Chart.js for progress metrics (optional)

**Rationale:**
- Timeline: HTML divs with CSS positioning render cleanly for screenshots (no artifacts)
- Progress metrics: Chart.js 11KB minified; broad community; zero security CVEs
- Avoids heavy libraries (ApexCharts, Plotly, D3.js add 100KB+ bloat)
- Optional Chart.js allows MVP to launch without charts

**Trade-offs Accepted:**
- Custom SVG rendering requires more code (benefits justify effort)
- Light chart library suitable for simple gauges (Chart.js sufficient)

---

### Single Project Structure (vs. Multi-Project)

**Choice:** Single AgentSquad.Runner.csproj with Components/, Services/, Models/ subdirectories

**Rationale:**
- Fastest builds and single deployment unit
- Aligns with dotnet new blazorserver template
- Matches .NET conventions
- Eliminates MSBuild orchestration complexity
- Sufficient for single-dashboard MVP

**Trade-offs Accepted:**
- Cannot independently version components (acceptable; treat as single service)
- Scale to multi-project only if dashboard becomes shared library (future phase)

---

## Security Considerations

### Authentication & Authorization

**Current Model (MVP):**
- No authentication required (internal tool, trusted users only)
- No authorization boundaries (single project, no multi-tenant)
- Local network only (no internet exposure)

**Justification:**
- PM spec explicitly states "no authentication/RBAC required"
- Read-only dashboard (no write APIs to protect)
- Executives and product managers are trusted internal stakeholders

**Future Enhancement (Post-MVP, if dashboard shared via network):**
```csharp
// Add simple API key authentication (header-based, not OAuth)
public class ApiKeyMiddleware
{
    public async Task InvokeAsync(HttpContext context, IConfiguration config)
    {
        var apiKey = config["Dashboard:ApiKey"];
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var key) || key != apiKey)
        {
            context.Response.StatusCode = 401;
            return;
        }
        await _next(context);
    }
}
```

---

### Input Validation

**Validation Rules (at deserialization):**
- ProjectName required, max 200 chars (prevent DoS via huge strings)
- Quarter required, max 50 chars
- 1-50 milestones (prevent array DoS)
- Total items max 5,000
- Milestone status enum validation (prevent invalid codes)
- All string fields bounded (prevent memory exhaustion)

**Implementation:** ProjectDataValidator class; explicit ProjectDataValidator.Validate(data) before accepting data.

**Silent Failure:** If validation fails, previous _cachedData retained; no crash.

---

### Data Protection

**No PII Handling:**
- data.json contains project metadata only (milestone names, deliverable titles)
- No customer data, passwords, API keys, or secrets
- File permissions: standard Windows ACLs
- No encryption at rest (project data not sensitive; isolated network)

**File I/O Security:**
```csharp
// Atomic write pattern (client-side responsibility)
private void SafeWriteDataJson(string filePath, string jsonContent)
{
    // 1. Write to .tmp
    string tmpPath = filePath + ".tmp";
    File.WriteAllText(tmpPath, jsonContent);
    
    // 2. Validate JSON before rename
    try { JsonSerializer.Deserialize<ProjectData>(jsonContent); }
    catch { File.Delete(tmpPath); throw; }
    
    // 3. Atomic rename
    File.Delete(filePath);
    File.Move(tmpPath, filePath);
}
```

---

### OWASP Compliance (Local-Only Context)

| Risk | Mitigation | Priority |
|------|-----------|----------|
| Injection | Input validation + System.Text.Json case-insensitive parsing | High |
| Broken Authentication | Not applicable (no auth) | N/A |
| Sensitive Data Exposure | No PII; local network; HTTPS optional | Low |
| XML External Entities | Not applicable (JSON-only) | N/A |
| Broken Access Control | Single-user/trusted-team model | Low |
| Security Misconfiguration | Hardcoded to localhost; no secrets in code | Medium |
| XSS | Blazor Server server-side rendering; no user HTML | Low |
| Insecure Deserialization | System.Text.Json AOT-safe; no BinaryFormatter | Low |
| Known Vulnerabilities | Dependency scanning in CI/CD (`dotnet list package --vulnerable`) | Medium |
| Insufficient Logging | Debug output to browser console; optional Serilog | Low |

---

## Scaling Strategy

### Current Constraints (MVP)

**Hard Limits:**
- 1 Blazor Server instance (no clustering)
- ~1,000 concurrent Blazor connections (single machine limit)
- 5,000 total dashboard items (no pagination)
- 5MB JSON file size (before >500ms parse latency)

**Target Use Case:**
- 1-5 executives viewing dashboard per day
- 10 data.json updates per day (batch-driven)
- Local SSD deployment

---

### Scaling Dimension 1: Data Size (5K → 50K Items)

**Solution:** Pagination + Lazy Loading

```csharp
public class PaginatedStatusItems
{
    public List<StatusItem> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage => (PageNumber * PageSize) < TotalCount;
}

public class DashboardDataService
{
    public PaginatedStatusItems GetShippedItems(int pageNumber = 1, int pageSize = 20)
    {
        var pageItems = _cachedData.Shipped
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        return new PaginatedStatusItems { Items = pageItems, ... };
    }
}
```

**Component Implementation:** Add previous/next buttons; bind to PageNumber state.

---

### Scaling Dimension 2: File Size (5MB → 100MB)

**Solution:** Split data.json into Per-Quarter Files

```
data-q2-2026.json
data-q3-2026.json
data-q4-2026.json
```

**Implementation:**
```csharp
public void Initialize(string dataPath)
{
    var quarter = Environment.GetEnvironmentVariable("DASHBOARD_QUARTER") ?? "Q2_2026";
    var actualPath = Path.Combine(
        Path.GetDirectoryName(dataPath),
        $"data-{quarter.ToLower()}.json"
    );
    LoadDataFromJson(actualPath);
}
```

---

### Scaling Dimension 3: Concurrent Users (5 → 100+)

**For 5-10 users (current MVP):**
- Single Blazor Server instance sufficient
- In-memory ProjectData caching in DashboardDataService

**For 100+ users (future phase):**
- Add distributed cache (SQL Server State Store, Redis)

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Configuration.GetConnectionString("Redis");
});

// Cache for 30 seconds
var cacheKey = "dashboard:projectdata";
var cached = _cache.Get<ProjectData>(cacheKey);
if (cached == null)
{
    LoadDataFromJson(dataPath);
    _cache.Set(cacheKey, _cachedData, TimeSpan.FromSeconds(30));
}
```

---

### Scaling Dimension 4: Hot-Reload Latency (10s → Sub-second)

**Current:** FileSystemWatcher + 10s Timer polling

**Optimization (if latency becomes issue):**
```csharp
// Reduce interval from 10s to 5s
_fallbackTimer = new System.Timers.Timer(5000) { AutoReset = true };

// Add debouncing to prevent reload storms
private DateTime _lastReloadAttempt = DateTime.MinValue;
private void ReloadData(string path)
{
    var now = DateTime.UtcNow;
    if ((now - _lastReloadAttempt).TotalMilliseconds < 2000)
        return; // Skip if attempted <2s ago
    
    _lastReloadAttempt = now;
    
    var fileInfo = new FileInfo(path);
    if (fileInfo.LastWriteTime > _lastModified)
        LoadDataFromJson(path);
}
```

---

### Caching Strategy

**Cache Levels:**
1. **In-Memory (DashboardDataService):** ProjectData _cachedData (expires on file change)
2. **Browser (optional):** Blazor circuit state (automatic; survives F5 within circuit lifetime)
3. **HTTP (optional):** Response headers if exposed via web
   ```csharp
   response.Headers.CacheControl = "public, max-age=60";
   ```

---

## Risks & Mitigations

### Risk 1: FileSystemWatcher Unreliability on Network Shares

**Probability:** High | **Impact:** Medium | **Priority:** P0

**Details:** FileSystemWatcher fails on network shares and WSL; data updates won't trigger hot-reload.

**Mitigation:**
- Hybrid approach: FileSystemWatcher + 10-second Timer fallback
- **Action:** Test on actual network share deployment before production
- **Fallback:** If FileSystemWatcher fails, Timer polling guarantees reload within 10 seconds
- **Cost:** Minimal (timer overhead ~1% CPU)

**Test Plan:** Week 2 deliverable—test FileSystemWatcher and Timer on actual deployment machine (local SSD vs. network share).

---

### Risk 2: JSON Deserialization Crashes on Schema Mismatch

**Probability:** Medium | **Impact:** High | **Priority:** P0

**Details:** If data.json schema changes or is malformed, dashboard crashes and displays blank page.

**Mitigation:**
- ProjectDataValidator with explicit validation before accepting data
- Silent failure: retain previous _cachedData if new JSON invalid
- Debug output to browser console (F12) for troubleshooting
- Unit tests verify validator catches missing/invalid fields

**Test Plan:** Unit tests for ProjectDataValidator; manual schema mismatch testing.

---

### Risk 3: Screenshot Rendering Inconsistency Across Monitors/DPI

**Probability:** Low | **Impact:** High | **Priority:** P1

**Details:** Executives see different layouts on 1920x1080, 4K, and laptop screens; PowerPoint looks misaligned.

**Mitigation:**
- Blazor Server (not WASM) guarantees identical server-side rendering across browsers
- Bootstrap 5.3.3 via CDN provides consistent typography and grid
- No animations/transitions (static HTML only)
- CSS calibrated for 1.0x–2.5x DPI scaling
- Screenshot on target monitor at Week 1 end; collect executive feedback

**Test Plan:** Week 1 deliverable—screenshot on 1920x1080 and 4K; get executive sign-off on visual design.

---

### Risk 4: Memory Leak in FileSystemWatcher / Timer

**Probability:** Low | **Impact:** Medium | **Priority:** P1

**Details:** Dashboard gradually consumes RAM over days; performance degrades.

**Mitigation:**
- Implement IDisposable; properly clean up FileSystemWatcher and Timer
- Unsubscribe OnDataChanged event in component Dispose
- Monitor idle RAM consumption (target: 50–100MB)

```csharp
public class DashboardDataService : IDisposable
{
    public void Dispose() { _watcher?.Dispose(); _fallbackTimer?.Dispose(); }
}

@implements IAsyncDisposable
async ValueTask IAsyncDisposable.DisposeAsync()
{
    DataService.OnDataChanged -= OnDataChanged;
}
```

---

### Risk 5: CDN Downtime (Bootstrap / Chart.js)

**Probability:** Low | **Impact:** Medium | **Priority:** P1

**Details:** Bootstrap or Chart.js CDN unavailable; dashboard CSS/charts don't load.

**Mitigation:**
- Self-host Bootstrap 5.3.3 and Chart.js 4.4.0 in wwwroot/
- Browser cache stores CSS/JS; single CDN miss tolerable for internal tool
- Graceful degradation: dashboard remains functional without Chart.js

```html
<!-- index.html: fallback to local -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet">
<link href="/css/bootstrap.min.css" rel="stylesheet" onerror="this.style.display='none'">
```

---

### Risk 6: .NET 8 Runtime EOL

**Probability:** Very Low | **Impact:** Medium | **Priority:** P2

**Details:** .NET 8 LTS support ends 2026-11; security patches stop.

**Mitigation:**
- .NET 9 (Nov 2024) and .NET 10 (Nov 2025) backward-compatible
- Timeline: MVP targets Q2 2026; upgrade to .NET 9 LTS before EOL if dashboard still in use
- Risk acceptance: For internal tool used 1-2 years, EOL is acceptable cost

---

### Risk 7: data.json File Corruption

**Probability:** Low | **Impact:** High | **Priority:** P1

**Details:** data.json corrupted or partially written; dashboard displays no data.

**Mitigation:**
- Atomic writes: product manager uses rename-after-write pattern
- Backups: manual backup before each edit (`copy data.json data.json.backup`)
- Validation before publish: product manager validates JSON locally
- Recovery: restore from backup in <5 minutes

---

### Risk 8: Data Out of Sync with Actual Project State

**Probability:** Medium | **Impact:** High | **Priority:** P1

**Details:** data.json stale; executives make decisions on outdated information.

**Mitigation:**
- Process: product manager updates data.json at start of each reporting cycle (weekly, not continuous)
- Ownership: assign single DRI (directly responsible individual) to own data.json
- Verification: executive signs off on data accuracy before using screenshots
- Automation (post-MVP): API sync from Jira/Azure DevOps

---

### Risk 9: Executive Team Rejects Visual Design / Color Scheme

**Probability:** Medium | **Impact:** Medium | **Priority:** P2

**Details:** Dashboard colors/layout don't match executive preferences; rework CSS.

**Mitigation:**
- Early feedback: prototype hardcoded dashboard at Week 1 end; collect stakeholder feedback
- Design lock: CSS finalized by end of Week 1; no late-stage changes allowed
- Reference: OriginalDesignConcept.html and C:/Pics/ReportingDashboardDesign.png guide design
- Approval gate: executive team signs off on design before Phase 2 (data integration)

**Test Plan:** Week 1 deliverable—screenshot + executive approval on visual design.

---

### Risk 10: FileSystemWatcher Misses Rapid File Changes

**Probability:** Low | **Impact:** Low | **Priority:** P3

**Details:** Multiple rapid data.json writes; FileSystemWatcher misses some events; dashboard update delayed.

**Mitigation:**
- LastWriteTime check: service verifies file timestamp before re-parsing (avoids duplicate parses)
- Change debouncing: wait 2 seconds before attempting another reload (prevents reload storms)
- Polling fallback: 10-second timer guarantees eventual consistency
- Acceptance: batch-driven reporting (10 updates/day) doesn't require sub-second latency

---

### Risk Priority Matrix

| Risk | Probability | Impact | Effort | Priority | Status |
|------|-------------|--------|--------|----------|--------|
| FileSystemWatcher fails | High | Medium | Low | **P0** | Mitigated (hybrid Timer) |
| JSON schema mismatch | Medium | High | Low | **P0** | Mitigated (validation) |
| Screenshot inconsistency | Low | High | Medium | **P1** | Mitigated (Blazor Server) |
| Memory leak | Low | Medium | Low | **P1** | Mitigated (IDisposable) |
| CDN downtime | Low | Medium | Low | **P1** | Mitigated (self-host) |
| .NET 8 EOL | Very Low | Medium | Medium | **P2** | Accept risk |
| data.json corruption | Low | High | Low | **P1** | Mitigated (atomic writes) |
| Data out of sync | Medium | High | High | **P1** | Mitigated (process) |
| Design rejection | Medium | Medium | High | **P2** | Mitigated (early feedback) |
| Rapid file changes | Low | Low | Low | **P3** | Mitigated (debouncing) |

---

## Deployment Checklist

- [ ] .NET 8.0 SDK and Runtime installed on target machine
- [ ] Git repository cloned or source files copied
- [ ] `dotnet restore` completed (downloads NuGet packages)
- [ ] `dotnet build --configuration Release` succeeds (no compilation errors)
- [ ] data.json exists in project root with valid JSON
- [ ] `dotnet run --configuration Release` starts without errors
- [ ] Browser loads https://localhost:5001 without certificate warnings
- [ ] Dashboard displays hardcoded sample data (timeline, metrics, cards)
- [ ] FileSystemWatcher functional: edit data.json, confirm reload within 10 seconds
- [ ] Screenshot on target monitor/projector looks clean and professional
- [ ] Executive stakeholders approve visual design before production

---

## Performance Targets

| Metric | Target | Threshold | Action if Exceeded |
|--------|--------|-----------|-------------------|
| Cold Start Latency | <2 seconds | >3 seconds | Profile startup; check disk I/O |
| Hot-Reload Latency | <10 seconds | >15 seconds | Reduce polling interval; optimize JSON size |
| Memory Usage (Idle) | 50-100MB | >200MB | Check for memory leaks in service |
| Memory Usage (Peak) | 200MB | >500MB | Implement pagination for large datasets |
| CPU Usage (Idle) | <5% | >10% | Reduce timer polling frequency |
| JSON File Size | <5MB | >10MB | Split into multiple files or archive old data |
| Rendering Latency | <500ms | >1 second | Use @key directives; profile component tree |
| Network Latency (CDN) | <200ms | >500ms | Self-host CDN files locally if needed |

---

## Summary

This architecture delivers a lightweight, screenshot-ready executive dashboard optimized for simplicity, consistency, and reliability. It leverages Blazor Server for server-side rendering consistency, System.Text.Json for fast JSON parsing, FileSystemWatcher + Timer fallback for robust hot-reload, and Bootstrap 5.3.3 for responsive design across monitor sizes.

The system is designed to scale horizontally (pagination, pagination) and vertically (caching, file splitting) as requirements grow. All major risks are identified and mitigated with concrete strategies. The architecture prioritizes MVP delivery (Week 1-4) while laying groundwork for future enhancements (multi-project support, API integration, historical tracking).