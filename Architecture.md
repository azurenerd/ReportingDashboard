# Architecture

## Overview & Goals

Executive reporting dashboard built on Blazor Server 8.0 displaying project milestones, task progress, and KPIs. Single-page application serving as screenshot-friendly reporting tool for PowerPoint decks. Data source: JSON configuration file (no database, no cloud services). Deployment: Local Kestrel server on Windows/Linux with .NET 8 Runtime. Success criteria: Renders without errors, data loads in <1 second, all user stories demo-ready, screenshots export cleanly to PDF.

## System Components

### Dashboard.razor (Main Page Component)
**Responsibility:** Orchestrate page layout, load project data, coordinate child components.
**Interfaces:** 
- Input: HttpRequest to /dashboard route
- Output: Rendered HTML with three-column grid layout
**Dependencies:** DataService, MilestoneTimeline, ProjectStatusSummary, TaskBoard components.
**Data:** ProjectStatus object containing milestones array and tasks array.

### DataService.cs (Data Access Layer)
**Responsibility:** Read data.json from wwwroot/data, deserialize via System.Text.Json, validate structure, return strongly-typed models.
**Interfaces:**
- Public method: `Task<ProjectStatus> ReadProjectDataAsync()`
- Returns: ProjectStatus model or throws JsonException if malformed.
**Dependencies:** System.Text.Json, IWebHostEnvironment.
**Data:** Reads from `wwwroot/data/data.json` on application startup and on-demand refresh.

### MilestoneTimeline.razor (Component)
**Responsibility:** Render Chart.js timeline visualization of milestones with status indicators.
**Interfaces:**
- Input parameter: `List<Milestone> Milestones`
- Output: Canvas element with Chart.js rendered chart.
**Dependencies:** Chart.js (CDN), JSInterop for Chart initialization.
**Data:** Milestone[] with properties: Name, TargetDate, Status (OnTrack/AtRisk/Completed).

### ProjectStatusSummary.razor (Component)
**Responsibility:** Calculate and display KPI cards (on-time %, shipped count, in-progress count) from task data.
**Interfaces:**
- Input parameter: `List<Task> Tasks`
- Output: Three Bootstrap cards with color indicators (green/yellow/red).
**Dependencies:** None.
**Data:** Aggregated statistics calculated from task status and completion flags.

### TaskBoard.razor (Component)
**Responsibility:** Render three-column task board (Completed, In Progress, Carried Over) with task cards.
**Interfaces:**
- Input parameter: `List<Task> Tasks`
- Output: Three Bootstrap columns with task cards.
**Dependencies:** None.
**Data:** Tasks grouped by status field.

### Models (Data Transfer Objects)
**ProjectStatus.cs:** Root model containing Milestones array and Tasks array.
**Milestone.cs:** Name (string), TargetDate (DateTime), Status (enum: OnTrack, AtRisk, Completed).
**Task.cs:** Id (string), Title (string), Description (string), Status (enum: Completed, InProgress, CarriedOver), AssignedTo (string), DueDate (DateTime).

## Component Interactions

1. **Initialization:** Browser requests /dashboard → Blazor Server renders Dashboard.razor component.
2. **Data Loading:** Dashboard.razor calls DataService.ReadProjectDataAsync() on component mount.
3. **Deserialization:** DataService reads wwwroot/data/data.json and deserializes to ProjectStatus via System.Text.Json.
4. **Rendering:** Dashboard.razor passes ProjectStatus.Milestones to MilestoneTimeline and ProjectStatus.Tasks to ProjectStatusSummary and TaskBoard.
5. **Chart Rendering:** MilestoneTimeline invokes Chart.js via JSInterop, passing milestone data as JavaScript array.
6. **Page Output:** HTML rendered server-side; pure Bootstrap/Chart.js display; no JavaScript state management required.
7. **Refresh:** Browser F5 refresh reloads component and rereads data.json.

## Data Model

### Storage Format
**File:** `wwwroot/data/data.json`
**Structure:** Single JSON object with two top-level arrays:

```json
{
  "milestones": [
    {
      "id": "m1",
      "name": "Project Kickoff",
      "targetDate": "2026-04-01T00:00:00Z",
      "status": "Completed"
    }
  ],
  "tasks": [
    {
      "id": "t1",
      "title": "Design Phase",
      "description": "Complete design mockups and requirements",
      "status": "Completed",
      "assignedTo": "Alice Johnson",
      "dueDate": "2026-04-05T00:00:00Z"
    }
  ]
}
```

### Entity Relationships
- No foreign keys; Milestones and Tasks are independent collections.
- Task status (Completed/InProgress/CarriedOver) determines board column placement.
- Milestone status (OnTrack/AtRisk/Completed) determines color coding (green/orange/blue).

### Validation Rules
- All milestones must have unique IDs.
- All tasks must have unique IDs.
- Status enums must match allowed values (case-sensitive).
- TargetDate and DueDate must be valid ISO 8601 format.

## API Contracts

### GET /dashboard
**Response:** HTML rendered by Dashboard.razor component.
**Status Codes:** 200 (success), 500 (data.json parse error).
**Error Handling:** If data.json is malformed, DataService throws JsonException caught by Dashboard.razor, rendered as error message: "Failed to load project data. Please verify data.json format."

### Data.json Read Endpoint (Internal)
**Method:** Synchronous file read from wwwroot/data/data.json via System.IO.File.ReadAllTextAsync().
**Response:** ProjectStatus object (C# model).
**Error States:**
- FileNotFoundException: Display "data.json not found."
- JsonException: Display "Invalid JSON format in data.json."

### Chart.js JSInterop Calls
**Function:** `interop.initChart(chartElement, milestoneData)`
**Parameters:** DOM canvas element ID, JSON array of milestones.
**Return:** Null (side effect: renders chart).

## Infrastructure Requirements

### Runtime Environment
- **Operating System:** Windows or Linux with .NET 8 Runtime installed.
- **Port:** Kestrel server listens on http://localhost:5000 (configurable in appsettings.json).
- **Memory:** 256 MB minimum; 512 MB recommended.
- **Disk:** 100 MB for application binaries + wwwroot assets.

### Hosting
**Development:** `dotnet run` from Visual Studio or CLI on developer machine.
**Staging/Production:** Deploy published DLL to server, run `dotnet MyProject.Web.dll --urls "http://0.0.0.0:5000"`.

### File Storage
- **data.json:** Stored in wwwroot/data directory; no backup required (manual updates only).
- **Static Assets:** Bootstrap 5 CSS, Chart.js JavaScript, Font Awesome icons served from wwwroot/css, wwwroot/js.

### Networking
- **Internal Only:** No HTTPS, no load balancer, no CDN.
- **Access:** Dashboard accessible to executives on internal network via browser (Chrome, Edge, Firefox).

### CI/CD
- **Build:** `dotnet build MyProject.sln`
- **Publish:** `dotnet publish -c Release -o ./publish`
- **Deploy:** Copy publish folder to server, run executable.

## Technology Stack Decisions

| Component | Technology | Justification |
|-----------|-----------|---|
| **Web Framework** | Blazor Server 8.0 | No build pipeline, C# on both sides, ideal for internal tools, screenshot-friendly HTML output. |
| **Styling** | Bootstrap 5.3.x | CSS-only (no npm), responsive grid, polished UI, executive-ready appearance. |
| **Charting** | Chart.js 4.x | Lightweight, CDN-deployed, strong executive dashboard adoption, JSInterop integration seamless. |
| **JSON Parsing** | System.Text.Json | Built-in .NET 8, no external dependency, high performance, native async support. |
| **Data Access** | File I/O (System.IO) | JSON file is sufficient for single-project, no database needed, simplified deployment. |
| **Server** | ASP.NET Core 8 Kestrel | Included with Blazor Server, local HTTP sufficient, zero external dependencies. |
| **Runtime** | .NET 8 (LTS) | Long-term support, C# 12 features, performance improvements, team existing expertise. |

## Security Considerations

### Authentication & Authorization
**Not implemented.** Assumes trusted internal network. No user accounts, no roles, no session state required.

### Data Protection
**data.json:** Contains only project metadata (milestone names, task titles, dates). No PII, no credentials, no sensitive data requiring encryption.

### Input Validation
- **JSON Schema Validation:** DataService validates JSON structure before deserialization; malformed JSON rejected with error message displayed to user.
- **Type Safety:** C# models enforce data types; invalid dates/enums caught at deserialization.
- **File Permissions:** data.json must be readable by application process; no write operations required.

### HTTPS & Transport
**Not required.** Local/internal network access only. HTTP sufficient for trusted environment.

## Scaling Strategy

**Current Scope (MVP):** Single project, max 20 tasks, 10 milestones, single-user focus (screenshot-dependent workflow).

**Scaling Limitations:**
- Kestrel single-threaded processing adequate for screenshot use case.
- data.json file size negligible; no performance impact.
- No concurrent user load expected; no session state complexity.

**Future Expansion (Phase 2+):**
- **Multi-project support:** Add project selector dropdown; store multiple projects in separate JSON files or introduce SQLite database.
- **Auto-refresh:** Implement SignalR for real-time dashboard updates or polling via JavaScript.
- **Data archival:** Migrate historical data to database; implement date range filtering.
- **Admin UI:** Add form-based editing for data.json via new Blazor page; replace file-based updates.

**Database Migration Path:** If future phases require persistence, use SQLite (local, no server) with Entity Framework Core; maintain single .sln structure.

## Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|-----------|
| **Corrupted data.json** | Medium | Dashboard fails to load | Validate JSON schema on startup; display error message with parsing details; maintain manual backup of data.json. |
| **Stale data in screenshots** | High | Executives view outdated status | Embed "Last Updated" timestamp on dashboard; document manual data refresh process in README; add refresh button to reload from file. |
| **Chart.js CDN unavailable** | Low | Milestone timeline not rendered | Fall back to static HTML bar chart or table if JavaScript unavailable; test CDN links in staging. |
| **Browser compatibility** | Low | Dashboard renders incorrectly | Test on Chrome, Edge, Firefox; Bootstrap 5 covers 99% of modern browsers; no IE11 support needed. |
| **File system permissions** | Low | Cannot read data.json | Run application with sufficient permissions; document file ACL requirements in deployment guide. |
| **Single point of failure (Kestrel)** | Medium | Dashboard unavailable | No redundancy required for MVP; document manual restart procedure; consider Windows Service wrapper for auto-restart. |
| **Performance degradation** | Low | Page load >2 seconds | Monitor JSON file size; pre-cache deserialized models; use IMemoryCache for ProjectStatus if auto-refresh added. |

**Key Assumption:** All risks assume trusted internal environment. If dashboard exposed to public/untrusted network, add authentication, HTTPS, input sanitization, and rate limiting.

---

## Implementation Roadmap

**Week 1 MVP:**
1. Create Blazor Server project from template.
2. Build Dashboard.razor layout (header, grid).
3. Implement DataService to read/deserialize data.json.
4. Create Milestone, Task, ProjectStatus models.
5. Build MilestoneTimeline, ProjectStatusSummary, TaskBoard components.
6. Integrate Chart.js via JSInterop.
7. Create fake data.json with 5 milestones, 12 tasks.
8. Test locally; verify screenshots export to PDF cleanly.

**Phase 2 (Deferred):**
- Admin page for editing data.json via form.
- Auto-refresh via SignalR or polling.
- Unit tests (bUnit).
- Logging and observability.