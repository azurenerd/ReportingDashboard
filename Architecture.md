# Architecture

## Overview & Goals

This document describes the complete system architecture for the Executive Reporting Dashboard—a single-page Blazor Server application that renders project milestone and progress data from a JSON configuration file for executive visibility and PowerPoint-ready screenshots.

**Primary Goals:**
1. Enable rapid, at-a-glance visibility into project milestones and progress
2. Replace static PowerPoint decks with a living, screenshot-optimized dashboard
3. Minimize IT overhead through simple, local-only deployment (no cloud, no authentication)
4. Provide a reusable dashboard template for future projects
5. Deliver <2-second page loads and <1-second data refresh

**Key Architectural Principles:**
- **Component-driven UI**: Reusable Razor components for dashboard layout, milestones, progress indicators
- **Single source of truth**: data.json configuration file (file-based, no database)
- **Server-side rendering**: ASP.NET Core 8.0 handles rendering; minimal client-side complexity
- **Local-only deployment**: Windows IIS or Windows Service hosting; zero cloud services
- **Screenshot-optimized design**: CSS media queries and fixed layouts for PowerPoint export
- **Stateless refresh**: Manual admin-triggered data reload without server restart

---

## System Components

### 1. Presentation Layer (Blazor Server UI)

**Responsibility**: Render dashboard layout, display project data, handle user interactions (refresh button, navigation).

**Key Components**:

#### Dashboard.razor (Main Container)
- Root Razor component; loads on page request
- Displays project summary, milestone timeline, and progress metrics
- Manages page-level state and component hierarchy
- Invokes ProjectDataService on initialization
- Provides visual feedback for manual refresh operations

**Dependencies**: ProjectDataService, MilestoneTimeline, ProgressCard, ProjectSummary
**Data Flow**: Initializes → loads project data → renders child components

#### MilestoneTimeline.razor
- Displays project milestones in chronological order (HTML table or Chart.js timeline)
- Shows milestone name, target date, and status (on-track, at-risk, complete, delayed)
- Visually prioritizes large rocks/critical milestones
- Chart.js rendering for client-side interactivity (optional; fallback to HTML table)

**Input**: `List<Milestone>` from ProjectDataService
**Output**: Rendered HTML table or Canvas (Chart.js)
**Styling**: Print-friendly CSS; optimized for 1920x1080 screenshot resolution

#### ProgressCard.razor (Reusable Component)
- Displays progress metrics: completed count, in-progress count, carried-over count
- Shows completion percentage as a progress bar
- Color-coded status indicators (green=done, yellow=in-progress, orange=carried-over)
- Updates reactively when data is refreshed

**Input**: `ProgressMetrics` object (category, count, completion %)
**Output**: Card-style HTML layout with status badges
**Responsive**: Flexbox/Grid for desktop-only layout

#### ProjectSummary.razor
- High-level project overview: name, current status, health indicator
- Key statistics: total milestones, completion %, at-risk count
- Loads immediately above the fold

**Input**: `Project` model with summary fields
**Output**: Summary card HTML
**Styling**: Fixed height; no scrolling required

#### RefreshButton.razor
- Manual refresh control; admin-triggered data reload
- Calls RefreshService to re-read data.json
- Provides visual feedback: spinner during load, success/error message
- Completes within <1 second

**Interaction**: OnClick event → RefreshService.RefreshData() → UI update
**Feedback**: Toast or modal confirmation message

---

### 2. Service Layer (Business Logic)

**Responsibility**: Load and deserialize project data, manage refresh operations, validate data integrity.

#### ProjectDataService.cs
- **Responsibility**: Load and deserialize data.json into typed models
- **Methods**:
  - `Task<Project> LoadProjectDataAsync()` — Read data.json from wwwroot; deserialize into Project model
  - `List<Milestone> GetMilestones()` — Return milestones sorted chronologically
  - `ProgressMetrics GetProgressMetrics()` — Calculate completion %, item counts by status
  - `Project GetCurrentProject()` — Return currently selected project (cached in-memory)
- **Lifetime**: Scoped (injected into Dashboard.razor; single instance per request)
- **Error Handling**: Catch FileNotFoundException, JsonException; log to Windows Event Viewer; return empty data or default values
- **File I/O**: Use System.Text.Json with System.IO.File for synchronous reads (simple for JSON <10MB)

**Data Flow**:
1. Dashboard.razor calls `LoadProjectDataAsync()` on component init
2. ProjectDataService reads wwwroot/data.json via File.ReadAllTextAsync()
3. System.Text.Json deserializes into `Project` object
4. Components bind to properties: Milestones, ProgressMetrics
5. Manual refresh re-invokes LoadProjectDataAsync()

#### RefreshService.cs
- **Responsibility**: Orchestrate data reload; signal UI updates via Blazor SignalR (optional)
- **Methods**:
  - `Task RefreshDataAsync()` — Re-read data.json; deserialize; clear in-memory cache
  - `event Action OnDataRefreshed` — Notify subscribed components of refresh completion
- **Lifetime**: Singleton (shared across all users for consistency)
- **Concurrency**: Use SemaphoreSlim to prevent concurrent file reads; queue refresh requests if needed
- **Performance**: Target <1 second refresh time (file read + JSON deserialization)

**Data Flow**:
1. Admin clicks RefreshButton
2. RefreshButton calls `RefreshService.RefreshDataAsync()`
3. RefreshService acquires lock, re-reads data.json
4. ProjectDataService cache cleared
5. OnDataRefreshed event fires; subscribed components re-render
6. UI confirmation shown to admin

---

### 3. Data Layer (JSON File Storage)

**Responsibility**: Persist project milestone and progress data; serve as single source of truth.

#### data.json (Configuration File)
- **Location**: wwwroot/data.json (deployed with application)
- **Purpose**: Store project names, milestones, progress metrics
- **Update Method**: Manual file edit by administrator; requires manual refresh in dashboard
- **Schema**:

```json
{
  "projects": [
    {
      "id": "project-1",
      "name": "Project Alpha",
      "status": "on-track",
      "summary": "Executive summary of project status",
      "milestones": [
        {
          "id": "m-1",
          "name": "Design Complete",
          "date": "2026-04-15",
          "status": "complete",
          "isLargeRock": true,
          "description": "All design mockups approved"
        },
        {
          "id": "m-2",
          "name": "Development Sprint 1",
          "date": "2026-05-30",
          "status": "on-track",
          "isLargeRock": false,
          "description": "Core features implemented"
        }
      ],
      "progress": {
        "completed": 45,
        "inProgress": 23,
        "carriedOver": 8,
        "total": 76,
        "completionPercentage": 59.2
      }
    }
  ]
}
```

- **Validation**: ProjectDataService validates JSON structure on load; defaults to empty data if invalid
- **Permissions**: Optional file-level ACLs to restrict write access to administrator identity only (Windows NTFS)
- **Backup**: Administrator responsible for backing up data.json externally (not application concern)

---

### 4. Models (Data Contracts)

**C# Classes** (System.Text.Json serializable):

```csharp
public class Project
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string Summary { get; set; }
    public List<Milestone> Milestones { get; set; }
    public ProgressMetrics Progress { get; set; }
}

public class Milestone
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } // "on-track", "at-risk", "complete", "delayed"
    public bool IsLargeRock { get; set; }
    public string Description { get; set; }
}

public class ProgressMetrics
{
    public int Completed { get; set; }
    public int InProgress { get; set; }
    public int CarriedOver { get; set; }
    public int Total { get; set; }
    public decimal CompletionPercentage { get; set; }
}
```

- **Serialization**: System.Text.Json with default options (case-insensitive matching enabled)
- **Immutability**: Consider record types for Models to ensure data integrity post-deserialization

---

## Component Interactions

### Data Flow Diagram (Logical)

```
Admin Page Request
    ↓
[Dashboard.razor] (on-initialize)
    ↓
ProjectDataService.LoadProjectDataAsync()
    ↓
File.ReadAllTextAsync(wwwroot/data.json)
    ↓
System.Text.Json.Deserialize<Project>()
    ↓
Cache Project object in-memory
    ↓
[Dashboard.razor] binds to data
    ├─→ [ProjectSummary.razor] displays name, status
    ├─→ [MilestoneTimeline.razor] renders milestones
    └─→ [ProgressCard.razor] displays metrics
    ↓
Page renders; interactive
    ↓
Admin clicks "Refresh" button
    ↓
[RefreshButton.razor] calls RefreshService.RefreshDataAsync()
    ↓
RefreshService acquires lock, clears cache
    ↓
ProjectDataService re-reads data.json
    ↓
OnDataRefreshed event fires
    ↓
Subscribed components re-render
    ↓
Admin sees updated dashboard
```

### Refresh Sequence

1. **Admin Interaction**: Clicks "Refresh Data" button in RefreshButton.razor
2. **UI Feedback**: Button shows spinner; confirmation toast displayed
3. **Service Call**: RefreshButton invokes `RefreshService.RefreshDataAsync()`
4. **Lock Acquisition**: RefreshService acquires SemaphoreSlim to prevent concurrent reads
5. **Cache Clear**: In-memory Project object cleared from ProjectDataService
6. **File Read**: ProjectDataService re-reads data.json from disk
7. **Deserialization**: System.Text.Json deserializes into new Project object
8. **Event Signal**: RefreshService fires `OnDataRefreshed` event
9. **Component Updates**: Subscribed Razor components react; Dashboard re-renders
10. **Confirmation**: Admin sees "Data refreshed successfully" message with timestamp

### Component Lifecycle

- **Dashboard.razor**: Initializes once on page load; subscribes to RefreshService.OnDataRefreshed for reactive updates
- **ProjectSummary, MilestoneTimeline, ProgressCard**: Rendered from Dashboard state; no independent lifecycle
- **RefreshButton**: Independent; triggers refresh on click; shows UI feedback
- **ProjectDataService**: Scoped lifetime; single instance per request; refreshes on-demand
- **RefreshService**: Singleton lifetime; shared across all concurrent users; thread-safe

---

## Data Model

### Entity Relationship

```
Project (1)
├─ Milestones (many)
│  └─ Milestone properties: id, name, date, status, isLargeRock, description
├─ Progress (1)
│  └─ ProgressMetrics: completed, inProgress, carriedOver, total, completionPercentage
└─ Summary fields: id, name, status, summary
```

### Storage Strategy

- **Format**: JSON (text-based, human-editable)
- **Location**: wwwroot/data.json (co-located with application assets)
- **Deployment**: Included in application package; updated manually by administrator
- **No Database**: Single JSON file eliminates relational database complexity
- **Caching**: In-memory cache (Project object) after deserialization; cleared on refresh
- **Concurrency**: File-level locking via SemaphoreSlim in RefreshService to prevent concurrent reads

### Data Lifecycle

1. **Creation**: Administrator manually creates data.json with project milestones and progress
2. **Deployment**: data.json packaged with application; deployed to wwwroot folder
3. **Runtime Access**: ProjectDataService reads on page load; cached in-memory
4. **Refresh**: Admin clicks button; RefreshService re-reads file; cache updated
5. **Update**: Administrator edits data.json externally (text editor); clicks Refresh in dashboard
6. **Archival**: No automatic versioning; administrator manually backs up data.json as needed

### Data Validation

- **On Load**: ProjectDataService validates JSON structure; returns default/empty data if invalid
- **Schema Compliance**: data.json must conform to Project/Milestone/ProgressMetrics schema
- **Type Safety**: System.Text.Json enforces type matching during deserialization; JsonException caught and logged
- **Null Handling**: Optional properties handled gracefully (default values for missing fields)

---

## API Contracts

### RESTful Endpoints (ASP.NET Core)

#### GET /api/projects/current
- **Purpose**: Retrieve current project data (for API clients; Blazor components use service directly)
- **Response**:
```json
{
  "id": "project-1",
  "name": "Project Alpha",
  "status": "on-track",
  "summary": "...",
  "milestones": [...],
  "progress": {...}
}
```
- **Status Codes**: 200 (success), 500 (file read error)
- **Latency**: <500ms (cached in-memory)

#### POST /api/projects/refresh
- **Purpose**: Trigger data reload from data.json
- **Request Body**: Empty
- **Response**:
```json
{
  "success": true,
  "message": "Data refreshed successfully",
  "timestamp": "2026-04-07T20:47:46Z"
}
```
- **Status Codes**: 200 (success), 500 (file read error, lock timeout)
- **Latency**: <1 second (file I/O + deserialization)

#### GET /api/milestones
- **Purpose**: Retrieve all milestones (optional; for reporting or future features)
- **Response**: Array of Milestone objects (sorted chronologically)
- **Status Codes**: 200 (success), 500 (error)

#### GET /api/progress
- **Purpose**: Retrieve progress metrics only (optional; for dashboard widgets)
- **Response**: ProgressMetrics object
- **Status Codes**: 200 (success), 500 (error)

### Error Handling

- **FileNotFoundException**: Log to Event Viewer; return 500 with message "Data file not found"
- **JsonException**: Log to Event Viewer; return 500 with message "Invalid data format"
- **SemaphoreSlim Timeout**: Log to Event Viewer; return 500 with message "Refresh operation timed out"
- **General Exception**: Log full stack trace; return 500 with generic "Internal server error"

### Content-Type
- **Request**: application/json
- **Response**: application/json
- **Blazor Components**: Use ProjectDataService directly (no HTTP calls); bypasses API layer

---

## Infrastructure Requirements

### Hosting Environment

**Local-Only Deployment**:
- **Option A**: Windows Server + IIS (production-like)
- **Option B**: Windows 10/11 + IIS Express (development)
- **Option C**: Windows Service (alternative to IIS)

**Required Components**:
1. Windows Server 2019 or later (or Windows 10/11 for development)
2. .NET 8 Runtime (ASP.NET Core 8.0)
3. IIS (10.0+) with Application Request Routing (ARR) module
4. HTTP.SYS (kernel mode HTTP listener)

**Application Pool Settings**:
- **Name**: AgentSquadDashboard
- **Runtime Version**: No managed code (.NET 8 is not managed code runtime)
- **.NET CLR Version**: n/a (uses out-of-process hosting model)
- **Pipeline Mode**: Integrated
- **Preload Enabled**: Yes (auto-start on server boot)
- **Idle Timeout**: 0 (never unload)
- **Recycle**: Daily at 2 AM (configurable)

**IIS Site Configuration**:
- **Binding**: http://localhost:80 or custom port (e.g., http://dashboard.internal:8080)
- **Physical Path**: C:\inetpub\Dashboard (or custom path)
- **Authentication**: Anonymous (no Windows Auth required)
- **SSL**: Optional; HTTP only for internal network (no sensitive data)

### Networking

- **Deployment Network**: Internal corporate network only (no public internet exposure)
- **Firewall Rules**: Allow HTTP traffic on port 80 (or custom port) from internal IPs
- **WebSocket**: Blazor Server requires WebSocket support (HTTP/1.1 Upgrade); ensure firewall/proxy allows upgrade requests
- **DNS**: Optional; IP address or hostname resolvable from internal network
- **Load Balancing**: Not required (single-instance deployment); no ARR or NLB needed
- **Proxy**: If deployed behind HTTP proxy, ensure WebSocket upgrade supported

### Storage

- **Application Files**: C:\inetpub\Dashboard\{Program files, wwwroot, etc.}
- **data.json**: C:\inetpub\Dashboard\wwwroot\data.json (read-only from IIS perspective; writable by administrator)
- **Logs**: Windows Event Viewer (Application and Services Logs)
- **Cache**: In-process memory (ProjectDataService cache); cleared on refresh
- **No External Storage**: No shared drives, cloud storage, or databases required

### Monitoring & Logging

**Event Logging**:
- **Source**: Blazor Server application (programmatic logging to Event Viewer)
- **Event Types**: 
  - Information: Page load, data refresh success
  - Warning: Slow load times, cache misses
  - Error: File read failures, JSON parsing errors, refresh timeouts

**Health Check**:
- **Endpoint**: GET / (page load test)
- **Expected**: <2 second response time; HTML dashboard renders
- **Monitoring Tool**: Windows Performance Monitor or external HTTP monitor (optional)

**Performance Metrics** (Windows Performance Monitor):
- CPU utilization (target: <5% idle)
- Memory usage (target: <500MB for single-instance)
- HTTP requests/sec (monitor for unusual spikes)
- Page load times (target: <2 seconds)

### Backup & Disaster Recovery

- **Application Files**: Backed up as part of server image/System State backup
- **data.json**: Administrator responsible for backup (not application-managed)
- **Recovery**: Restore from backup; re-deploy .NET 8 runtime if needed
- **RTO**: <1 hour (manual recovery; no automated failover)
- **RPO**: Administrator-dependent (frequency of data.json backups)

---

## Technology Stack Decisions

### Mandatory Stack: C# .NET 8 with Blazor Server

**Blazor Server (ASP.NET Core 8.0)**
- **Decision**: Full-stack C# application; single language for UI and backend logic
- **Justification**: 
  - Eliminates JavaScript framework learning curve
  - Full .NET ecosystem access (Entity Framework, logging, DI)
  - Server-side rendering eliminates cross-browser compatibility concerns
  - WebSocket-based interactivity suitable for internal dashboard
  - Rapid component development via Razor templates
- **Trade-offs**: 
  - Slightly higher server load than Blazor WebAssembly (all logic runs server-side)
  - Session state per user (memory overhead)
  - Acceptable for internal tool with ~100 concurrent user limit

**Razor Components**
- **Decision**: Use .razor files for all UI components (Dashboard, MilestoneTimeline, ProgressCard, etc.)
- **Justification**: 
  - Built-in to Blazor; no additional framework required
  - Component reusability across multiple pages
  - Strongly-typed C# code-behind; IntelliSense support
  - Hot reload support for development productivity
- **Alternative Rejected**: Angular/React/Vue (requires separate frontend infrastructure; adds complexity)

**System.Text.Json**
- **Decision**: Native JSON deserialization for data.json (no third-party JSON libraries)
- **Justification**: 
  - Built-in to .NET 8; zero external dependency
  - High performance; no reflection overhead
  - Handles simple schemas (Project, Milestone, ProgressMetrics) without complex mapping
  - Minimal learning curve
- **Alternative Rejected**: Newtonsoft.Json (mature but adds external dependency; not needed for simple schema)

**Bootstrap 5.3**
- **Decision**: CSS framework for responsive layout and pre-built components (buttons, cards, grids)
- **Justification**: 
  - Industry standard; well-documented
  - Minimal custom CSS required
  - Print-friendly media queries available
  - Mobile-responsive (fallback; not primary requirement)
- **Alternative Rejected**: Tailwind CSS (more control but requires build toolchain; Bootstrap simpler for MVP)

**Chart.js 4.x**
- **Decision**: Client-side timeline/chart rendering (optional; fallback to HTML table)
- **Justification**: 
  - Lightweight; screenshot-friendly
  - No server-side rendering required
  - Interactivity (zoom, pan, hover) for desktop browsers
  - CDN-based deployment; no npm build required
- **Alternative Rejected**: Plotly.NET (server-side rendering; heavier payload)

**Windows IIS + .NET Runtime**
- **Decision**: Host on local Windows Server with IIS
- **Justification**: 
  - No cloud services per requirements
  - Windows-native deployment; familiar to IT teams
  - IIS proven for ASP.NET Core (out-of-process hosting model)
  - Minimal infrastructure overhead
- **Alternative Rejected**: Docker (adds containerization complexity; Windows-only environment doesn't benefit)

**FileSystemWatcher (.NET Built-in)**
- **Decision**: Optional future enhancement for auto-detecting data.json changes
- **Justification**: 
  - Built-in to .NET; no external dependency
  - Efficient file change notification
  - Can trigger SignalR broadcast to refresh all connected clients
- **Status**: Deferred to Phase 2 (manual refresh sufficient for MVP)

**xUnit + Moq (Testing)**
- **Decision**: Unit testing framework and mocking library for service/component tests
- **Justification**: 
  - Lightweight; .NET standard
  - Integration with Visual Studio Test Explorer
  - Async/await support for Task-based tests
- **Alternative Rejected**: NUnit (heavier; not needed for MVP)

**Visual Studio 2022 Community**
- **Decision**: Development IDE
- **Justification**: 
  - Full Blazor debugging support
  - Built-in .NET 8 toolchain and Razor editor
  - Free Community edition
- **Alternative Rejected**: VS Code (less integrated Blazor debugging; requires Omnisharp extension)

---

## Security Considerations

### Authentication & Authorization

**Decision**: No authentication required (internal tool).
- Dashboard accessible to anyone on internal network via URL
- No user login; no session management
- No role-based access control

**Future Consideration** (Phase 2): Windows Authentication for corporate network integration
- Authenticate via Active Directory
- Restrict dashboard to specific Windows groups

### Data Protection

**Sensitive Data**: None.
- Project metadata only (names, dates, status)
- No financial data, PII, or credentials stored
- data.json contains no encryption requirements

**File Permissions**:
- Optional: Restrict data.json write access to administrator identity user via NTFS ACLs
- Readable by application identity (IIS AppPool user)
- Prevents accidental modification by non-administrators

**Data in Transit**:
- HTTP only (no HTTPS required for internal network)
- Blazor Server uses WebSocket over HTTP (unencrypted)
- No sensitive data transmitted; acceptable risk for internal tool

### Input Validation

**JSON Deserialization**:
- System.Text.Json validates schema on load
- Invalid JSON caught and logged; application returns default data
- Type mismatches handled gracefully (null for missing fields)

**File Path Traversal**:
- data.json hardcoded to wwwroot/data.json; no user-supplied paths
- FileNotFoundException caught and logged (not exposed to UI)

**XSS Prevention**:
- Razor components automatically HTML-encode @variables
- No eval() or dynamic code execution
- No user-supplied HTML injected into components

### Infrastructure Security

**Network Isolation**:
- Deployment restricted to internal corporate network
- Firewall rules prevent public internet access
- No DNS exposure to external networks

**Application Identity**:
- IIS AppPool runs as ApplicationPoolIdentity (least-privileged account)
- No administrator privileges required
- data.json permissions restricted to AppPool identity (read); administrator (read/write)

**Error Logging**:
- Errors logged to Windows Event Viewer (local machine only)
- No sensitive data in logs
- Administrator access required to view Event Viewer logs

---

## Scaling Strategy

### Single-Instance Design

**Current Limit**: Single Blazor Server instance handles ~100 concurrent WebSocket connections.
- Sufficient for internal executive dashboard (low concurrent user count)
- No load balancing required

**Scaling Path** (if needed in future):

1. **Vertical Scaling** (Phase 2):
   - Increase server CPU/memory
   - Raise AppPool connection limit in IIS
   - Expected ceiling: ~500 concurrent users on 4-core, 8GB server

2. **Horizontal Scaling** (Phase 3, if required):
   - Deploy multiple IIS instances behind load balancer (NLB)
   - Implement distributed cache (Redis) for shared session state
   - Use Azure Service Bus or similar for inter-instance signaling
   - **Note**: Contradicts "local-only" requirement; requires architectural change

3. **Performance Optimization** (Phase 2):
   - Add in-memory caching for Project data (currently no cache expiration)
   - Lazy-load milestones if JSON >10MB (paginate timeline)
   - Compress HTTP responses (gzip)
   - Minify CSS/JavaScript

### Data Layer Scaling

**Current Bottleneck**: File I/O (SemaphoreSlim-based serialization prevents concurrent reads).
- For MVP: Sufficient (manual refresh triggered by admin, not high-frequency)
- Future: Consider SQLite local database if refresh frequency increases

**Caching Strategy**:
- In-process cache (Dictionary) with optional TTL
- Refresh clears cache; next page load re-reads file
- No distributed cache needed for single-instance

### UI Responsiveness

**Current Approach**: All rendering server-side (Blazor Server).
- Initial page load: <2 seconds (JSON deserialization + Razor rendering)
- Component updates: <500ms (WebSocket + re-render)
- Chart.js rendering: <500ms (client-side; independent of server)

**Optimization Path**:
- Implement virtual scrolling for large milestone lists (>100 items)
- Lazy-load chart visualization (render on-demand)
- Defer non-critical component renders

---

## Risks & Mitigations

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|-----------|
| **Blazor WebSocket Disconnect** | Medium | Implement automatic reconnection logic; display "Reconnecting..." message; cache page state locally until reconnect |
| **data.json Corruption** | Medium | Implement atomic write (write to temp file, then rename); use file locking (SemaphoreSlim); administrator backup protocol |
| **Large JSON File Performance** | Low | Monitor file size; implement lazy-loading for >100 milestones; consider pagination or SQLite in Phase 2 |
| **Chart.js Rendering Delay** | Low | Profile rendering time; aggregate milestones by quarter if >100 items; consider server-side rendering as fallback |
| **Memory Leak in Blazor** | Low | Monitor AppPool memory over time; implement component disposal patterns; test with long-running dashboard |
| **JSON Deserialization Timeout** | Low | Set timeout on file read operations; handle JsonException explicitly |

### Operational Risks

| Risk | Severity | Mitigation |
|------|----------|-----------|
| **Administrator Forgets to Refresh** | Low | Add timestamp to dashboard ("Last refreshed: 2026-04-07 20:47"); optional auto-refresh timer (UI notification) |
| **Accidental Data.json Deletion** | Medium | Administrator backup protocol; consider read-only flag on production file; version control data.json in repo |
| **IIS Crash/Restart** | Medium | Enable AppPool auto-restart in IIS; health check script (external monitor); Windows Service alternative |
| **Network Outage** | Low | Blazor Server reconnects automatically; no data loss (state preserved server-side for 10+ minutes) |

### Scalability Risks

| Risk | Severity | Mitigation |
|------|----------|-----------|
| **>100 Concurrent Users** | Low | Vertical scale (more CPU/memory); monitor AppPool CPU/memory metrics; plan Phase 2 horizontal scaling if needed |
| **Refresh Lock Contention** | Low | Unlikely at <100 users; add logging to RefreshService to detect contention; optimize file I/O |
| **WebSocket Message Queue Backlog** | Low | Monitor Blazor Server connection pool; limit per-user message throughput if needed |

### Deployment Risks

| Risk | Severity | Mitigation |
|------|----------|-----------|
| **.NET 8 Runtime Not Installed** | High | Pre-deployment checklist; automated installer; provide .NET 8 setup bundle with application |
| **IIS Configuration Incorrect** | Medium | Deploy web.config with correct application pool settings; document IIS setup steps; pre-configure AppPool in setup script |
| **Firewall Blocks WebSocket** | Medium | Validate WebSocket connectivity during deployment; test upgrade request to ws:// endpoint; document firewall rule (HTTP/1.1 Upgrade) |
| **data.json File Permissions** | Low | Document NTFS ACL setup; pre-stage file with correct permissions in installer |

### Data Risks

| Risk | Severity | Mitigation |
|------|----------|-----------|
| **Invalid JSON Prevents Load** | Medium | Validate JSON schema before deployment; provide schema validator in setup; log JsonException with file path |
| **Stale Data Due to Manual Refresh** | Low | Educate administrator; add timestamp to UI; optional auto-refresh timer (future phase) |
| **Milestone Date in Past** | Low | Handle gracefully in MilestoneTimeline (display "Overdue"); sort by date regardless |

---

## Implementation Roadmap

### Phase 1 (MVP, 2-3 weeks)

**Week 1**: Foundation
- Create Blazor Server project (.NET 8)
- Implement Dashboard.razor, ProjectSummary.razor, ProgressCard.razor, MilestoneTimeline.razor
- Create Project, Milestone, ProgressMetrics models
- Implement ProjectDataService (file read, JSON deserialization)

**Week 2**: Integration & Refresh
- Integrate ProjectDataService into Dashboard
- Implement RefreshService with SemaphoreSlim
- Add RefreshButton.razor with UI feedback
- Create sample data.json with 3-5 fictional projects

**Week 3**: Testing & Optimization
- Unit tests for ProjectDataService, RefreshService
- Screenshot testing (1920x1080 resolution)
- CSS optimization for print-friendly layout
- Deployment to IIS; validate page load times

### Phase 2 (Future Enhancements)

- FileSystemWatcher for auto-detect data.json changes
- SignalR broadcast to refresh all connected clients
- Windows Authentication integration
- Historical snapshots and trend analysis
- Export to PDF/PowerPoint XML

### Phase 3 (Advanced)

- Horizontal scaling with load balancer
- Distributed cache (Redis)
- Dashboard customization UI (admin panel to edit projects without file edit)
- Mobile-responsive design

---

**Document Version**: 1.0  
**Last Updated**: 2026-04-07  
**Status**: Approved for Implementation