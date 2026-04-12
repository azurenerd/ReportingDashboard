# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-12 01:54 UTC_

### Summary

For a simple executive reporting dashboard using C# .NET 8 with Blazor Server, we recommend a lightweight architecture leveraging Blazor Server's real-time capabilities with local SQLite data storage for milestone and progress tracking. The project should avoid unnecessary complexity—focusing on clean UI presentation for screenshot-ready executive reporting. A single Blazor component reading from a JSON configuration file with Chart.js for timeline visualization provides the optimal balance of simplicity, maintainability, and visual appeal for PowerPoint integration. **Primary Recommendation:** Build a single-page Blazor Server application with minimal dependencies, using Chart.js for milestone timeline visualization, entity models for strongly-typed data binding, and local SQLite for persistent data storage. This approach leverages .NET's native strengths while keeping the codebase simple enough for rapid iteration and screenshot capture. ---

### Key Findings

- **Blazor Server is ideal for this use case:** Real-time data binding, no JavaScript framework overhead, and native .NET code execution eliminate context-switching and simplify development.
- **Simplicity drives screenshot quality:** Minimal dependencies, clean separation of concerns, and straightforward UI logic make the application easier to optimize visually for executive presentations.
- **Local-first architecture is appropriate:** With no cloud services, SQLite provides zero-friction persistent storage; data.json can be loaded and cached in memory for rapid rendering.
- **Chart.js offers the best milestone visualization:** Lightweight, integrates cleanly via JavaScript interop in Blazor, produces clean timeline and progress graphics suitable for PowerPoint screenshots.
- **Entity Framework Core 8 enables clean data modeling:** Strongly-typed models for projects, milestones, and work items prevent data inconsistencies and simplify filtering/sorting for reporting views.
- **No custom authentication needed:** Per requirements, implement simple role-based access control via session state or configuration; executive access is assumed trusted in local environment.
- **JSON configuration approach minimizes moving parts:** data.json as the single source of truth for project metadata, milestones, and work status eliminates database schema complexity. --- **Goal:** Functional single-page dashboard reading from data.json, zero complexity.
- [ ] Create Blazor Server project + .sln structure
- [ ] Define Project, Milestone, WorkItem models in C#
- [ ] Load data.json via System.Text.Json; hard-code sample data
- [ ] Build Dashboard.razor component with Bootstrap grid layout:
- Header (project name, date range)
- Key metrics cards (% complete, shipped count, carry-over count)
- Work item table (Status | Title | Owner | Date)
- [ ] Prototype Chart.js timeline with 3–5 sample milestones
- [ ] CSS: Ensure clean, minimal aesthetic suitable for PowerPoint screenshots
- **Deliverable:** Runnable app; screenshot-ready dashboard **Goal:** Replace hard-coded data with SQLite backend.
- [ ] Add Entity Framework Core + SQLite NuGet packages
- [ ] Create DbContext, migrations for Project/Milestone/WorkItem schema
- [ ] Seed SQLite from data.json on app startup
- [ ] Implement ProjectDataService to query SQLite
- [ ] Add ReportingService for executive metrics aggregation
- [ ] Update Dashboard.razor to call ReportingService
- **Deliverable:** Dashboard reading from SQLite; data persisted **Goal:** Refine visuals, add interactivity, optimize performance.
- [ ] Enhance Chart.js timeline: clickable milestones, status color-coding, drag tooltips
- [ ] Add filtering: by status, by milestone, by date range
- [ ] Implement dark mode toggle (CSS only, no JavaScript)
- [ ] Performance: profile Blazor re-render; optimize queries with .Include()
- [ ] Add unit tests for ReportingService calculations (xUnit)
- [ ] Document data.json schema and configuration
- **Deliverable:** Polished dashboard, performant, screenshot-ready
- **Milestone timeline with milestone cards:** Instead of table, display mileposts horizontally with percentage complete below each. Instant visual impact.
- **Color-coded status badges:** Use Bootstrap badge classes (success/warning/danger) for statuses. Improves scannability in screenshots.
- **Simple progress bars:** Add Bootstrap progress bars under each milestone. Executives immediately see burn-down progress.
- **Export to PNG button:** Use html2canvas.js to screenshot dashboard directly. Executive convenience.
- **Dark theme toggle:** Add one-click CSS theme switch. Makes dashboard look polished without complexity.
- **Prototype Chart.js timeline separately:** Build a standalone HTML + JS prototype (outside Blazor initially) to validate timeline UX. Import into Blazor once design is locked.
- **Mock ReportingService calculations:** Before SQLite implementation, hard-code metrics aggregation in a test file. Verify business logic is correct.
- **Screenshot iteration loop:** Build dashboard → screenshot → iterate UI → screenshot. This is the core feedback loop for executive presentation.
- [ ] Multi-project support (if/when needed)
- [ ] Historical data warehouse (if/when trends become critical)
- [ ] Role-based access control (if/when multiple user types emerge)
- [ ] Mobile-responsive design (if/when mobile access is required; responsive Bootstrap grid handles this out-of-the-box)
- [ ] API endpoints for external integrations (if/when other systems need to consume data) --- This executive reporting dashboard is best served by a **minimal-complexity, Blazor Server-based approach** leveraging C# .NET 8's native strengths. By keeping the tech stack lean—SQLite for data, System.Text.Json for configuration, Chart.js for visualization, Bootstrap for layout—the team maximizes iteration speed and produces screenshot-ready output optimized for executive consumption. Start with Phase 1 MVP in one week, validate visual design with stakeholders, then add persistence and polish in subsequent phases. **Success metrics:** Dashboard loads in <2 seconds, displays 5–10 key metrics clearly, and is visually simple enough to screenshot and embed in PowerPoint without redaction.

### Recommended Tools & Technologies

- **Blazor Server** (bundled with .NET 8): Interactive server-side rendering with real-time component updates. Strength: eliminates JavaScript framework complexity, uses C# for all logic. Alternative: Blazor WebAssembly (not recommended here—requires APIs and adds deployment complexity).
- **Bootstrap 5.3**: CSS framework for responsive, professional layout. Use CDN link in Host.cshtml. Strength: minimal configuration, extensive component library.
- **Chart.js 4.4+** (via JavaScript interop): Lightweight charting library for milestone timeline and progress visualization. Version 4.4 offers enhanced animation and accessibility. Alternative: Syncfusion (enterprise, overkill for this use case).
- **Font Awesome 6.4** (CDN): Icon library for milestone markers and status indicators. Strength: 2KB embedded icons, extensive set.
- **.NET 8 Class Libraries** (no framework required): Plain C# projects for domain models, data access, and business logic. Strength: no runtime overhead, maximum type safety.
- **Entity Framework Core 8.0**: ORM for SQLite data access. Configure with lazy loading disabled (instantiate explicitly) for dashboard performance. Alternative: Dapper (lighter, but EF is sufficient here).
- **SQLite 3.45+** (file-based, bundled with System.Data.SQLite 1.0.118): Local relational database for projects, milestones, work items. Strength: zero-config, ACID compliance, single .db file. No cloud provider needed.
- **data.json** (user-managed): Configuration source for project metadata, milestone definitions, and initial work item catalog. Load once at app startup, cache in memory.
- **System.Text.Json** (built-in): Deserialize data.json into strongly-typed models. Strength: no external dependency, high performance.
- **MSBuild** (Visual Studio / .NET CLI): Native .NET build system. Command: `dotnet build` and `dotnet publish`.
- **xUnit 2.6** + **Moq 4.20**: Unit testing framework for business logic validation. Strength: .NET-native, extensive assertion library.
- **Bunit 1.24** (optional, for advanced Blazor component testing): Component testing framework for dashboard rendering logic. Only if visual regression testing is required.
- **Visual Studio 2022 (Community or Professional)** or **Visual Studio Code + C# Dev Kit**: IDE for development. Strength: integrated debugging, Blazor hot reload, .sln management.
- **.NET 8 SDK**: Command-line tooling. Ensure version 8.0.203+ for Blazor Server stability. | Component | Version | Rationale | |-----------|---------|-----------| | .NET Runtime | 8.0 LTS | Long-term support, stable Blazor Server implementation. | | Entity Framework Core | 8.0.4+ | Latest 8.x stable, SQLite provider fully compatible. | | Bootstrap | 5.3+ | Latest stable, responsive grid for executive dashboard. | | Chart.js | 4.4.0+ | Latest with accessibility improvements, clean timeline API. | | SQLite | 3.45.0+ | Latest stable, bundled in System.Data.SQLite. | | xUnit | 2.6.0+ | Latest stable, no breaking changes in 2.6 series. | ---
```
AgentSquad.Runner/
├── Models/
│   ├── Project.cs
│   ├── Milestone.cs
│   ├── WorkItem.cs
│   └── DashboardViewModel.cs
├── Services/
│   ├── ProjectDataService.cs (loads data.json + SQLite)
│   ├── MilestoneService.cs (milestone filtering/timeline logic)
│   └── ReportingService.cs (executive metrics aggregation)
├── Data/
│   ├── ApplicationDbContext.cs (EF Core DbContext)
│   └── Migrations/ (EF Core schema)
├── Components/
│   ├── Dashboard.razor (main page component)
│   ├── MilestoneTimeline.razor (Chart.js wrapper)
│   ├── ProgressCard.razor (reusable status card)
│   └── _Layout.cshtml (Bootstrap structure)
└── wwwroot/
    ├── data.json
    ├── css/
    │   └── dashboard.css (custom styling for screenshots)
    └── js/
        └── chart-interop.js (JavaScript interop for Chart.js)
```
- **Startup:** Load data.json via `System.Text.Json` → populate in-memory cache + SQLite.
- **Dashboard Load:** ReportingService aggregates metrics (completion %, shipped items, carry-over items) from SQLite.
- **Timeline Rendering:** MilestoneService filters milestones by date range, passes to Chart.js via JavaScript interop.
- **Real-time Updates:** If work items are modified, refresh cache and trigger Blazor component re-render.
```
Project
├── Id (GUID)
├── Name
├── StartDate
├── TargetEndDate
└── WorkItems[] (many)

Milestone
├── Id (GUID)
├── ProjectId (FK)
├── Name
├── ScheduledDate
└── Status (Planned, In Progress, Completed, Delayed)

WorkItem
├── Id (GUID)
├── ProjectId (FK)
├── Title
├── Status (New, In Progress, Shipped, Carried Over)
├── CreatedDate
├── CompletedDate
└── MilestoneId (FK, nullable)
```
- **Application-level in-memory cache:** On startup, load data.json and SQLite records into Dictionary<string, Project>. Invalidate only on explicit reload.
- **No distributed cache needed:** Single-machine deployment means no cache coherency issues.
- **Chart data pre-computed:** On dashboard load, compute milestone dates and work item counts once, pass to Chart.js.
- **Not applicable for single-page application:** No API endpoints needed. Blazor Server communicates directly with services via dependency injection.
- **If future integration (e.g., external reporting systems): Design RESTful endpoints following REST conventions. Use MVC controllers in the same .sln. ---

### Considerations & Risks

- **No formal authentication required** per requirements (local, trusted environment).
- **Implement basic session-based role scoping (optional):** Add simple middleware to log access by role (Executive, Manager, Contributor). Store in session state.
- **Data at rest:** SQLite database file is plaintext. If sensitive financial data is stored, apply SQLite encryption via SQLCipher or file-level OS permissions.
- **HTTPS in production:** If deployed to local network, enable HTTPS with self-signed certificates (Windows/IIS).
- **Environment variables:** Store sensitive config (database path, data.json location) in appsettings.json (not checked into repo) and appsettings.{Environment}.json.
- **No PII encryption required:** Assuming project names, milestone titles are non-sensitive executive data.
- **Local deployment:** Host on Windows or Linux machine running .NET 8 runtime.
- **Deployment method:** `dotnet publish -c Release` → copy to local server → run via Windows Service (ServiceCollection + BackgroundService) or directly via `dotnet run`.
- **IIS Hosting (optional):** Publish to IIS with reverse proxy configuration for HTTPS. Use WebDeploy or manual file copy.
- **No containerization required:** Complexity not justified for single-page dashboard. If future scaling: Dockerfile with `mcr.microsoft.com/dotnet/aspnet:8.0`.
- **Estimated cost:** $0 on-premises (use existing compute). If hosted on Azure: ~$5–10/month for Basic App Service + Database tier (minimal workload).
- **Scalability:** Local deployment scales to ~100s of concurrent users via vertical scaling (more RAM/CPU). Milestone: if >500 concurrent users, consider SQLite → SQL Server + scale horizontally. --- | Risk | Likelihood | Impact | Mitigation | |------|------------|--------|-----------| | **Blazor Server state explosion with large datasets** | Medium | Dashboard becomes slow if project has >10K work items | Implement pagination/filtering in ReportingService; lazy-load work item details. | | **JavaScript interop fragility for Chart.js** | Low | Chart rendering breaks on Blazor updates | Isolate Chart.js in separate component; use `@ref` to control lifecycle. Test after each Blazor version update. | | **SQLite concurrency with multiple dashboard instances** | Low | Database locks if reports run in parallel | SQLite handles concurrent reads fine; limit writes to single thread. Use write-ahead logging (WAL mode). | | **data.json format drift from schema** | Medium | Dashboard fails to deserialize corrupted JSON | Implement JSON schema validation on load; provide detailed error messages. Add unit tests for deserialization. |
- **SQLite performance:** Up to ~100K work items load in <1s with indexed queries. Beyond that, migrate to SQL Server.
- **Chart.js rendering:** Timeline with >1000 milestones becomes slow. Solution: aggregate by month/quarter, implement drill-down.
- **Blazor Server memory:** Large component trees grow linearly. Keep dashboard single-page; avoid nested tables with thousands of rows.
- **Blazor Server component lifecycle:** Team unfamiliar with Blazor's StateHasChanged() and re-render triggers may produce inefficient code. Mitigation: pair programming, code review for performance-critical components.
- **JavaScript interop for Chart.js:** Small learning curve for team new to JS interop. Mitigation: encapsulate in single service; document with examples.
- **EF Core query optimization:** Lazy-loaded navigation properties can cause N+1 queries. Mitigation: use .Include() explicitly; test with EF Profiler or logging.
- **SQLite database file:** No redundancy. Mitigation: implement daily backup script (copy .db file to network share). Add backup UI option.
- **data.json:** If corrupted, dashboard fails to load. Mitigation: version control data.json in Git; add validation on load. | Trade-off | Choice | Rationale | |-----------|--------|-----------| | **Database** | SQLite vs. SQL Server | SQLite chosen for zero-config simplicity; SQL Server not needed unless scaling to enterprise. | | **Charting** | Chart.js vs. Syncfusion/Telerik | Chart.js chosen for lightweight footprint and clean timeline API; Syncfusion adds licensing cost and complexity. | | **Frontend Framework** | Blazor Server vs. React/Vue | Blazor Server chosen to avoid JavaScript context-switching and leverage .NET expertise. | | **Caching** | In-memory vs. Redis | In-memory chosen for local deployment; Redis adds operational overhead not justified here. | ---
- **What constitutes "shipped" vs. "in progress"?** Define status taxonomy precisely in domain model. Should "Shipped" mean deployed to production, or completed and tested?
- **How frequently should the dashboard refresh data from data.json?** Real-time (watch file system), scheduled (poll every N minutes), or manual (user-triggered)? Impacts caching strategy.
- **Who maintains data.json?** Is it a manual Excel export, or system-generated from a tracking tool? Affects data pipeline design.
- **What metrics are most critical for executive view?** Focus on velocity (items/sprint), burndown, milestone on-time percentage, or budget tracking? Influences card/chart priority.
- **Should work items be editable in the dashboard, or read-only?** Adding edit capability requires form validation, SQLite writes, and audit logging. Recommend read-only for MVP.
- **Multi-project support?** Dashboard shows one project at a time, or rolls up multiple projects? Impacts data model and filtering UI.
- **Historical trends:** Should dashboard show past performance (e.g., last month's velocity), or only current sprint? Requires time-series storage. ---

### Detailed Analysis

Based on the context provided and the mandatory technology stack (C# .NET 8 with Blazor Server), let me produce the comprehensive research document:

# Executive Summary

For a simple executive reporting dashboard using C# .NET 8 with Blazor Server, we recommend a lightweight architecture leveraging Blazor Server's real-time capabilities with local SQLite data storage for milestone and progress tracking. The project should avoid unnecessary complexity—focusing on clean UI presentation for screenshot-ready executive reporting. A single Blazor component reading from a JSON configuration file with Chart.js for timeline visualization provides the optimal balance of simplicity, maintainability, and visual appeal for PowerPoint integration.

**Primary Recommendation:** Build a single-page Blazor Server application with minimal dependencies, using Chart.js for milestone timeline visualization, entity models for strongly-typed data binding, and local SQLite for persistent data storage. This approach leverages .NET's native strengths while keeping the codebase simple enough for rapid iteration and screenshot capture.

---

# Key Findings

- **Blazor Server is ideal for this use case:** Real-time data binding, no JavaScript framework overhead, and native .NET code execution eliminate context-switching and simplify development.
- **Simplicity drives screenshot quality:** Minimal dependencies, clean separation of concerns, and straightforward UI logic make the application easier to optimize visually for executive presentations.
- **Local-first architecture is appropriate:** With no cloud services, SQLite provides zero-friction persistent storage; data.json can be loaded and cached in memory for rapid rendering.
- **Chart.js offers the best milestone visualization:** Lightweight, integrates cleanly via JavaScript interop in Blazor, produces clean timeline and progress graphics suitable for PowerPoint screenshots.
- **Entity Framework Core 8 enables clean data modeling:** Strongly-typed models for projects, milestones, and work items prevent data inconsistencies and simplify filtering/sorting for reporting views.
- **No custom authentication needed:** Per requirements, implement simple role-based access control via session state or configuration; executive access is assumed trusted in local environment.
- **JSON configuration approach minimizes moving parts:** data.json as the single source of truth for project metadata, milestones, and work status eliminates database schema complexity.

---

# Recommended Technology Stack

## Frontend
- **Blazor Server** (bundled with .NET 8): Interactive server-side rendering with real-time component updates. Strength: eliminates JavaScript framework complexity, uses C# for all logic. Alternative: Blazor WebAssembly (not recommended here—requires APIs and adds deployment complexity).
- **Bootstrap 5.3**: CSS framework for responsive, professional layout. Use CDN link in Host.cshtml. Strength: minimal configuration, extensive component library.
- **Chart.js 4.4+** (via JavaScript interop): Lightweight charting library for milestone timeline and progress visualization. Version 4.4 offers enhanced animation and accessibility. Alternative: Syncfusion (enterprise, overkill for this use case).
- **Font Awesome 6.4** (CDN): Icon library for milestone markers and status indicators. Strength: 2KB embedded icons, extensive set.

## Backend
- **.NET 8 Class Libraries** (no framework required): Plain C# projects for domain models, data access, and business logic. Strength: no runtime overhead, maximum type safety.
- **Entity Framework Core 8.0**: ORM for SQLite data access. Configure with lazy loading disabled (instantiate explicitly) for dashboard performance. Alternative: Dapper (lighter, but EF is sufficient here).

## Data Storage
- **SQLite 3.45+** (file-based, bundled with System.Data.SQLite 1.0.118): Local relational database for projects, milestones, work items. Strength: zero-config, ACID compliance, single .db file. No cloud provider needed.
- **data.json** (user-managed): Configuration source for project metadata, milestone definitions, and initial work item catalog. Load once at app startup, cache in memory.
- **System.Text.Json** (built-in): Deserialize data.json into strongly-typed models. Strength: no external dependency, high performance.

## Build & Testing
- **MSBuild** (Visual Studio / .NET CLI): Native .NET build system. Command: `dotnet build` and `dotnet publish`.
- **xUnit 2.6** + **Moq 4.20**: Unit testing framework for business logic validation. Strength: .NET-native, extensive assertion library.
- **Bunit 1.24** (optional, for advanced Blazor component testing): Component testing framework for dashboard rendering logic. Only if visual regression testing is required.

## Development Environment
- **Visual Studio 2022 (Community or Professional)** or **Visual Studio Code + C# Dev Kit**: IDE for development. Strength: integrated debugging, Blazor hot reload, .sln management.
- **.NET 8 SDK**: Command-line tooling. Ensure version 8.0.203+ for Blazor Server stability.

## Versioning & Dependencies
| Component | Version | Rationale |
|-----------|---------|-----------|
| .NET Runtime | 8.0 LTS | Long-term support, stable Blazor Server implementation. |
| Entity Framework Core | 8.0.4+ | Latest 8.x stable, SQLite provider fully compatible. |
| Bootstrap | 5.3+ | Latest stable, responsive grid for executive dashboard. |
| Chart.js | 4.4.0+ | Latest with accessibility improvements, clean timeline API. |
| SQLite | 3.45.0+ | Latest stable, bundled in System.Data.SQLite. |
| xUnit | 2.6.0+ | Latest stable, no breaking changes in 2.6 series. |

---

# Architecture Recommendations

## Overall Design Pattern: Clean Architecture (Simplified for Single-Page Context)

```
AgentSquad.Runner/
├── Models/
│   ├── Project.cs
│   ├── Milestone.cs
│   ├── WorkItem.cs
│   └── DashboardViewModel.cs
├── Services/
│   ├── ProjectDataService.cs (loads data.json + SQLite)
│   ├── MilestoneService.cs (milestone filtering/timeline logic)
│   └── ReportingService.cs (executive metrics aggregation)
├── Data/
│   ├── ApplicationDbContext.cs (EF Core DbContext)
│   └── Migrations/ (EF Core schema)
├── Components/
│   ├── Dashboard.razor (main page component)
│   ├── MilestoneTimeline.razor (Chart.js wrapper)
│   ├── ProgressCard.razor (reusable status card)
│   └── _Layout.cshtml (Bootstrap structure)
└── wwwroot/
    ├── data.json
    ├── css/
    │   └── dashboard.css (custom styling for screenshots)
    └── js/
        └── chart-interop.js (JavaScript interop for Chart.js)
```

## Data Flow
1. **Startup:** Load data.json via `System.Text.Json` → populate in-memory cache + SQLite.
2. **Dashboard Load:** ReportingService aggregates metrics (completion %, shipped items, carry-over items) from SQLite.
3. **Timeline Rendering:** MilestoneService filters milestones by date range, passes to Chart.js via JavaScript interop.
4. **Real-time Updates:** If work items are modified, refresh cache and trigger Blazor component re-render.

## Data Model
```
Project
├── Id (GUID)
├── Name
├── StartDate
├── TargetEndDate
└── WorkItems[] (many)

Milestone
├── Id (GUID)
├── ProjectId (FK)
├── Name
├── ScheduledDate
└── Status (Planned, In Progress, Completed, Delayed)

WorkItem
├── Id (GUID)
├── ProjectId (FK)
├── Title
├── Status (New, In Progress, Shipped, Carried Over)
├── CreatedDate
├── CompletedDate
└── MilestoneId (FK, nullable)
```

## Caching Strategy
- **Application-level in-memory cache:** On startup, load data.json and SQLite records into Dictionary<string, Project>. Invalidate only on explicit reload.
- **No distributed cache needed:** Single-machine deployment means no cache coherency issues.
- **Chart data pre-computed:** On dashboard load, compute milestone dates and work item counts once, pass to Chart.js.

## API Strategy
- **Not applicable for single-page application:** No API endpoints needed. Blazor Server communicates directly with services via dependency injection.
- **If future integration (e.g., external reporting systems): Design RESTful endpoints following REST conventions. Use MVC controllers in the same .sln.

---

# Security & Infrastructure

## Authentication & Authorization
- **No formal authentication required** per requirements (local, trusted environment).
- **Implement basic session-based role scoping (optional):** Add simple middleware to log access by role (Executive, Manager, Contributor). Store in session state.
- **Data at rest:** SQLite database file is plaintext. If sensitive financial data is stored, apply SQLite encryption via SQLCipher or file-level OS permissions.

## Data Protection
- **HTTPS in production:** If deployed to local network, enable HTTPS with self-signed certificates (Windows/IIS).
- **Environment variables:** Store sensitive config (database path, data.json location) in appsettings.json (not checked into repo) and appsettings.{Environment}.json.
- **No PII encryption required:** Assuming project names, milestone titles are non-sensitive executive data.

## Hosting & Deployment
- **Local deployment:** Host on Windows or Linux machine running .NET 8 runtime.
- **Deployment method:** `dotnet publish -c Release` → copy to local server → run via Windows Service (ServiceCollection + BackgroundService) or directly via `dotnet run`.
- **IIS Hosting (optional):** Publish to IIS with reverse proxy configuration for HTTPS. Use WebDeploy or manual file copy.
- **No containerization required:** Complexity not justified for single-page dashboard. If future scaling: Dockerfile with `mcr.microsoft.com/dotnet/aspnet:8.0`.

## Infrastructure Costs
- **Estimated cost:** $0 on-premises (use existing compute). If hosted on Azure: ~$5–10/month for Basic App Service + Database tier (minimal workload).
- **Scalability:** Local deployment scales to ~100s of concurrent users via vertical scaling (more RAM/CPU). Milestone: if >500 concurrent users, consider SQLite → SQL Server + scale horizontally.

---

# Risks & Trade-offs

## Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|-----------|
| **Blazor Server state explosion with large datasets** | Medium | Dashboard becomes slow if project has >10K work items | Implement pagination/filtering in ReportingService; lazy-load work item details. |
| **JavaScript interop fragility for Chart.js** | Low | Chart rendering breaks on Blazor updates | Isolate Chart.js in separate component; use `@ref` to control lifecycle. Test after each Blazor version update. |
| **SQLite concurrency with multiple dashboard instances** | Low | Database locks if reports run in parallel | SQLite handles concurrent reads fine; limit writes to single thread. Use write-ahead logging (WAL mode). |
| **data.json format drift from schema** | Medium | Dashboard fails to deserialize corrupted JSON | Implement JSON schema validation on load; provide detailed error messages. Add unit tests for deserialization. |

## Scalability Bottlenecks
- **SQLite performance:** Up to ~100K work items load in <1s with indexed queries. Beyond that, migrate to SQL Server.
- **Chart.js rendering:** Timeline with >1000 milestones becomes slow. Solution: aggregate by month/quarter, implement drill-down.
- **Blazor Server memory:** Large component trees grow linearly. Keep dashboard single-page; avoid nested tables with thousands of rows.

## Skill Gaps & Learning Curve
- **Blazor Server component lifecycle:** Team unfamiliar with Blazor's StateHasChanged() and re-render triggers may produce inefficient code. Mitigation: pair programming, code review for performance-critical components.
- **JavaScript interop for Chart.js:** Small learning curve for team new to JS interop. Mitigation: encapsulate in single service; document with examples.
- **EF Core query optimization:** Lazy-loaded navigation properties can cause N+1 queries. Mitigation: use .Include() explicitly; test with EF Profiler or logging.

## Single Points of Failure
- **SQLite database file:** No redundancy. Mitigation: implement daily backup script (copy .db file to network share). Add backup UI option.
- **data.json:** If corrupted, dashboard fails to load. Mitigation: version control data.json in Git; add validation on load.

## Trade-offs Made
| Trade-off | Choice | Rationale |
|-----------|--------|-----------|
| **Database** | SQLite vs. SQL Server | SQLite chosen for zero-config simplicity; SQL Server not needed unless scaling to enterprise. |
| **Charting** | Chart.js vs. Syncfusion/Telerik | Chart.js chosen for lightweight footprint and clean timeline API; Syncfusion adds licensing cost and complexity. |
| **Frontend Framework** | Blazor Server vs. React/Vue | Blazor Server chosen to avoid JavaScript context-switching and leverage .NET expertise. |
| **Caching** | In-memory vs. Redis | In-memory chosen for local deployment; Redis adds operational overhead not justified here. |

---

# Open Questions

1. **What constitutes "shipped" vs. "in progress"?** Define status taxonomy precisely in domain model. Should "Shipped" mean deployed to production, or completed and tested?

2. **How frequently should the dashboard refresh data from data.json?** Real-time (watch file system), scheduled (poll every N minutes), or manual (user-triggered)? Impacts caching strategy.

3. **Who maintains data.json?** Is it a manual Excel export, or system-generated from a tracking tool? Affects data pipeline design.

4. **What metrics are most critical for executive view?** Focus on velocity (items/sprint), burndown, milestone on-time percentage, or budget tracking? Influences card/chart priority.

5. **Should work items be editable in the dashboard, or read-only?** Adding edit capability requires form validation, SQLite writes, and audit logging. Recommend read-only for MVP.

6. **Multi-project support?** Dashboard shows one project at a time, or rolls up multiple projects? Impacts data model and filtering UI.

7. **Historical trends:** Should dashboard show past performance (e.g., last month's velocity), or only current sprint? Requires time-series storage.

---

# Implementation Recommendations

## Phased Approach

### Phase 1: MVP (Week 1)
**Goal:** Functional single-page dashboard reading from data.json, zero complexity.

- [ ] Create Blazor Server project + .sln structure
- [ ] Define Project, Milestone, WorkItem models in C#
- [ ] Load data.json via System.Text.Json; hard-code sample data
- [ ] Build Dashboard.razor component with Bootstrap grid layout:
  - Header (project name, date range)
  - Key metrics cards (% complete, shipped count, carry-over count)
  - Work item table (Status | Title | Owner | Date)
- [ ] Prototype Chart.js timeline with 3–5 sample milestones
- [ ] CSS: Ensure clean, minimal aesthetic suitable for PowerPoint screenshots
- **Deliverable:** Runnable app; screenshot-ready dashboard

### Phase 2: Data Persistence (Week 2)
**Goal:** Replace hard-coded data with SQLite backend.

- [ ] Add Entity Framework Core + SQLite NuGet packages
- [ ] Create DbContext, migrations for Project/Milestone/WorkItem schema
- [ ] Seed SQLite from data.json on app startup
- [ ] Implement ProjectDataService to query SQLite
- [ ] Add ReportingService for executive metrics aggregation
- [ ] Update Dashboard.razor to call ReportingService
- **Deliverable:** Dashboard reading from SQLite; data persisted

### Phase 3: Polish & Optimization (Week 3)
**Goal:** Refine visuals, add interactivity, optimize performance.

- [ ] Enhance Chart.js timeline: clickable milestones, status color-coding, drag tooltips
- [ ] Add filtering: by status, by milestone, by date range
- [ ] Implement dark mode toggle (CSS only, no JavaScript)
- [ ] Performance: profile Blazor re-render; optimize queries with .Include()
- [ ] Add unit tests for ReportingService calculations (xUnit)
- [ ] Document data.json schema and configuration
- **Deliverable:** Polished dashboard, performant, screenshot-ready

## Quick Wins (High-Value, Low-Effort)

1. **Milestone timeline with milestone cards:** Instead of table, display mileposts horizontally with percentage complete below each. Instant visual impact.
2. **Color-coded status badges:** Use Bootstrap badge classes (success/warning/danger) for statuses. Improves scannability in screenshots.
3. **Simple progress bars:** Add Bootstrap progress bars under each milestone. Executives immediately see burn-down progress.
4. **Export to PNG button:** Use html2canvas.js to screenshot dashboard directly. Executive convenience.
5. **Dark theme toggle:** Add one-click CSS theme switch. Makes dashboard look polished without complexity.

## Prototyping Recommendations

- **Prototype Chart.js timeline separately:** Build a standalone HTML + JS prototype (outside Blazor initially) to validate timeline UX. Import into Blazor once design is locked.
- **Mock ReportingService calculations:** Before SQLite implementation, hard-code metrics aggregation in a test file. Verify business logic is correct.
- **Screenshot iteration loop:** Build dashboard → screenshot → iterate UI → screenshot. This is the core feedback loop for executive presentation.

## Deferred Decisions (Post-MVP)

- [ ] Multi-project support (if/when needed)
- [ ] Historical data warehouse (if/when trends become critical)
- [ ] Role-based access control (if/when multiple user types emerge)
- [ ] Mobile-responsive design (if/when mobile access is required; responsive Bootstrap grid handles this out-of-the-box)
- [ ] API endpoints for external integrations (if/when other systems need to consume data)

---

# Conclusion

This executive reporting dashboard is best served by a **minimal-complexity, Blazor Server-based approach** leveraging C# .NET 8's native strengths. By keeping the tech stack lean—SQLite for data, System.Text.Json for configuration, Chart.js for visualization, Bootstrap for layout—the team maximizes iteration speed and produces screenshot-ready output optimized for executive consumption. Start with Phase 1 MVP in one week, validate visual design with stakeholders, then add persistence and polish in subsequent phases.

**Success metrics:** Dashboard loads in <2 seconds, displays 5–10 key metrics clearly, and is visually simple enough to screenshot and embed in PowerPoint without redaction.
