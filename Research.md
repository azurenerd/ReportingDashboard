# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-10 14:15 UTC_

### Summary

For a simple, local-only executive reporting dashboard using C# .NET 8 with Blazor Server, the recommended approach is a lightweight Blazor Server application with JSON file-based data, minimal dependencies, and direct server-side rendering. This stack provides rapid development, simplicity for screenshots, and zero infrastructure complexity. Primary recommendation: Blazor Server with System.Text.Json for configuration, Chart.js interop for milestone visualization, and Bootstrap 5 for responsive UI—all proven, stable libraries with strong .NET ecosystem support.

### Key Findings

- Blazor Server is ideal for local reporting dashboards—no build pipeline needed, C# on both client and server, interactive without JavaScript overhead
- JSON-based configuration is sufficient for this scope; no database required (file-based simplicity aligns with design screenshot goal)
- Chart.js via JSInterop provides executive-friendly milestone timelines and progress visualization without heavy dependencies
- Bootstrap 5 (CSS only, no JavaScript required) maintains template design simplicity while ensuring responsive, polished UI
- Team can leverage existing C# skills; minimal JavaScript required; Razor components encapsulate design patterns cleanly
- No authentication/security complexity eliminates session management, HTTPS, and claims infrastructure—pure HTML rendering
- Single .sln structure with one Blazor Server project is the minimal viable path; no microservices or layered architecture needed
- Create Blazor Server project with Bootstrap 5 template
- Build Dashboard.razor layout (header, 3-column grid)
- Create MilestoneTimeline and ProgressBars components
- Create data.json with fake project data (5 milestones, 12 tasks)
- Implement DataService.ReadProjectData() to load and deserialize
- Integrate Chart.js for milestone timeline (via JSInterop)
- Deploy locally and test screenshot export (Print to PDF via browser)
- Use existing Bootstrap template—CSS already polished, no custom styling needed
- Fake data in JSON (no real project integration phase 1)—focus on UI/UX
- Chart.js CDN link (no npm/build required)—simple to integrate
- Data editing UI (admin page to update data.json via form)
- Auto-refresh via SignalR or polling
- Historical archival/project switching
- Unit tests (bUnit)
- Logging and observability
- Create a rough HTML static file matching OriginalDesignConcept.html with fake data first (no Blazor). Once design is approved, convert to Blazor components for interactivity and cleaner code reuse.

### Recommended Tools & Technologies

- **Blazor Server 8.0** – Server-side Razor component framework; provides interactivity without bundling JavaScript frameworks
- **Bootstrap 5.3.x** – CSS-only, responsive grid/utilities; keeps design templating simple and screenshot-friendly
- **Chart.js 4.x** – Via JSInterop for milestone timelines and burndown charts; lightweight, widely adopted in exec dashboards
- **Font Awesome 6.x** – Icon set for milestone badges and status indicators (optional, can use Unicode)
- **.NET 8 (LTS)** – Latest stable, C# 12 language features, performance improvements for Razor compilation
- **System.Text.Json** – Built-in JSON serialization; no external dependency needed for data.json parsing
- **ASP.NET Core 8 (Kestrel)** – Included with Blazor Server; local HTTP server sufficient for screenshot/screenshot use case
- **JSON file (data.json)** – Project milestones, status, burndown data; no database required
- **System.IO.File** – Built-in for reading JSON configuration; no ORM or database driver needed
- **.NET 8 CLI** – dotnet build, dotnet run; no additional build tooling needed
- **xUnit 2.6.x** – Standard .NET testing framework (optional for component testing, not required for MVP)
- **Blazor testing library** – bUnit 1.x for component unit tests (deferred, not MVP priority)
- ```
- MyProject.sln
- ├── MyProject.Web/
- │   ├── Pages/
- │   │   ├── Dashboard.razor (main reporting page)
- │   │   └── Index.razor
- │   ├── Components/
- │   │   ├── MilestoneTimeline.razor
- │   │   ├── ProgressBars.razor
- │   │   └── StatusCards.razor
- │   ├── Data/
- │   │   ├── DataService.cs (reads data.json)
- │   │   └── Models/ (ProjectStatus, Milestone, Task DTOs)
- │   ├── wwwroot/
- │   │   ├── css/bootstrap.min.css
- │   │   ├── js/chart.min.js
- │   │   └── data/
- │   │       └── data.json
- │   ├── App.razor
- │   └── Program.cs
- └── MyProject.Web.Tests/ (optional, deferred)
- ```
- HTTP request to /dashboard
- Blazor Server loads Dashboard.razor component
- DataService reads data.json from wwwroot/data
- Models deserialize via System.Text.Json
- Child components (MilestoneTimeline, ProgressBars, StatusCards) receive data via parameters
- Chart.js renders via JSInterop for interactive milestone timeline
- Pure HTML/Bootstrap renders status cards and progress indicators
- Dashboard (main page component, orchestrates layout)
- MilestoneTimeline (renders Chart.js timeline from milestones array)
- ProjectStatusSummary (KPI cards: on-time %, shipped count, in-progress count)
- TaskBoard (sections: Completed, In Progress, Carried Over; Task cards)

### Considerations & Risks

- Development: `dotnet run` in Visual Studio or CLI
- Staging/Shared: Deploy .NET 8 Runtime + published DLL to Windows/Linux server, run Kestrel on internal port
- No HTTPS, no load balancer, no CDN required for MVP
- data.json editing requires file-based update (no UI editor in MVP)
- Kestrel auto-restart on file change (optional: implement hot reload via directory watcher)
- No logging/monitoring needed for MVP; add Event Log or Console logging if troubleshooting required
- | Risk | Impact | Mitigation |
- |------|--------|-----------|
- | **Single-file data source** | If data.json corrupted, dashboard breaks | Validate JSON on load; provide error message if parse fails |
- | **No version control for data** | Data updates not tracked | Consider CSV or database if audit trail required in future |
- | **Screenshot-dependent workflow** | Stale data if file not updated before screenshot | Embed data refresh timestamp on dashboard; document manual update process |
- | **Razor compilation time** | Slow startup on large codebases | Not a bottleneck for single-page dashboard; acceptable for local use |
- | **Chart.js JSInterop** | Client-side rendering dependency on JavaScript | Chart.js is lightweight and stable; fallback to static SVG/HTML charts if JS fails |
- **Data source in production:** Will data.json be manually edited, or should a simple admin page for editing be added?
- **Refresh strategy:** How often should the dashboard refresh? Auto-reload every N minutes, or manual refresh only?
- **Mobile viewing:** Are executives viewing on tablets/mobile, or desktop only? (Affects Bootstrap grid breakpoints)
- **Archival:** Do historical project dashboards need to be preserved, or is only the current project shown?
- **Multi-project support:** Scope locked to single project, or should there be project selection dropdown?

### Detailed Analysis

# Research.md: Executive Reporting Dashboard Technology Stack

## Executive Summary

For a simple, local-only executive reporting dashboard using C# .NET 8 with Blazor Server, the recommended approach is a lightweight Blazor Server application with JSON file-based data, minimal dependencies, and direct server-side rendering. This stack provides rapid development, simplicity for screenshots, and zero infrastructure complexity. Primary recommendation: Blazor Server with System.Text.Json for configuration, Chart.js interop for milestone visualization, and Bootstrap 5 for responsive UI—all proven, stable libraries with strong .NET ecosystem support.

## Key Findings

- Blazor Server is ideal for local reporting dashboards—no build pipeline needed, C# on both client and server, interactive without JavaScript overhead
- JSON-based configuration is sufficient for this scope; no database required (file-based simplicity aligns with design screenshot goal)
- Chart.js via JSInterop provides executive-friendly milestone timelines and progress visualization without heavy dependencies
- Bootstrap 5 (CSS only, no JavaScript required) maintains template design simplicity while ensuring responsive, polished UI
- Team can leverage existing C# skills; minimal JavaScript required; Razor components encapsulate design patterns cleanly
- No authentication/security complexity eliminates session management, HTTPS, and claims infrastructure—pure HTML rendering
- Single .sln structure with one Blazor Server project is the minimal viable path; no microservices or layered architecture needed

## Recommended Technology Stack

### Frontend Layer
- **Blazor Server 8.0** – Server-side Razor component framework; provides interactivity without bundling JavaScript frameworks
- **Bootstrap 5.3.x** – CSS-only, responsive grid/utilities; keeps design templating simple and screenshot-friendly
- **Chart.js 4.x** – Via JSInterop for milestone timelines and burndown charts; lightweight, widely adopted in exec dashboards
- **Font Awesome 6.x** – Icon set for milestone badges and status indicators (optional, can use Unicode)

### Backend Layer
- **.NET 8 (LTS)** – Latest stable, C# 12 language features, performance improvements for Razor compilation
- **System.Text.Json** – Built-in JSON serialization; no external dependency needed for data.json parsing
- **ASP.NET Core 8 (Kestrel)** – Included with Blazor Server; local HTTP server sufficient for screenshot/screenshot use case

### Data Layer
- **JSON file (data.json)** – Project milestones, status, burndown data; no database required
- **System.IO.File** – Built-in for reading JSON configuration; no ORM or database driver needed

### Development & Testing
- **.NET 8 CLI** – dotnet build, dotnet run; no additional build tooling needed
- **xUnit 2.6.x** – Standard .NET testing framework (optional for component testing, not required for MVP)
- **Blazor testing library** – bUnit 1.x for component unit tests (deferred, not MVP priority)

### Project Structure
```
MyProject.sln
├── MyProject.Web/
│   ├── Pages/
│   │   ├── Dashboard.razor (main reporting page)
│   │   └── Index.razor
│   ├── Components/
│   │   ├── MilestoneTimeline.razor
│   │   ├── ProgressBars.razor
│   │   └── StatusCards.razor
│   ├── Data/
│   │   ├── DataService.cs (reads data.json)
│   │   └── Models/ (ProjectStatus, Milestone, Task DTOs)
│   ├── wwwroot/
│   │   ├── css/bootstrap.min.css
│   │   ├── js/chart.min.js
│   │   └── data/
│   │       └── data.json
│   ├── App.razor
│   └── Program.cs
└── MyProject.Web.Tests/ (optional, deferred)
```

## Architecture Recommendations

**Rendering Strategy:** Server-side Razor components with no JavaScript complexity. Dashboard page renders from Blazor Server component that reads data.json on startup/refresh.

**Data Flow:**
1. HTTP request to /dashboard
2. Blazor Server loads Dashboard.razor component
3. DataService reads data.json from wwwroot/data
4. Models deserialize via System.Text.Json
5. Child components (MilestoneTimeline, ProgressBars, StatusCards) receive data via parameters
6. Chart.js renders via JSInterop for interactive milestone timeline
7. Pure HTML/Bootstrap renders status cards and progress indicators

**State Management:** No shared state complexity needed. Single page loads data once per request; refresh reloads from file.

**Component Hierarchy:**
- Dashboard (main page component, orchestrates layout)
  - MilestoneTimeline (renders Chart.js timeline from milestones array)
  - ProjectStatusSummary (KPI cards: on-time %, shipped count, in-progress count)
  - TaskBoard (sections: Completed, In Progress, Carried Over; Task cards)

**Design Pattern:** Server-side rendering avoids Single Page App complexity. Each section is a discrete Razor component with data binding and parameter passing; no state management framework needed.

## Security & Infrastructure

**Authentication/Authorization:** None required. Assumes trusted local environment or internal network. No claims, roles, or session state.

**Data Protection:** data.json contains only project metadata—no PII, no secrets. No encryption needed.

**Hosting:** Local Kestrel server (`dotnet run` on developer machine or small server). Sufficient for screenshot/executive review.

**Deployment:** 
- Development: `dotnet run` in Visual Studio or CLI
- Staging/Shared: Deploy .NET 8 Runtime + published DLL to Windows/Linux server, run Kestrel on internal port
- No HTTPS, no load balancer, no CDN required for MVP

**Infrastructure Cost:** Zero (local machine or single small server); no cloud services.

**Operational Considerations:**
- data.json editing requires file-based update (no UI editor in MVP)
- Kestrel auto-restart on file change (optional: implement hot reload via directory watcher)
- No logging/monitoring needed for MVP; add Event Log or Console logging if troubleshooting required

## Risks & Trade-offs

| Risk | Impact | Mitigation |
|------|--------|-----------|
| **Single-file data source** | If data.json corrupted, dashboard breaks | Validate JSON on load; provide error message if parse fails |
| **No version control for data** | Data updates not tracked | Consider CSV or database if audit trail required in future |
| **Screenshot-dependent workflow** | Stale data if file not updated before screenshot | Embed data refresh timestamp on dashboard; document manual update process |
| **Razor compilation time** | Slow startup on large codebases | Not a bottleneck for single-page dashboard; acceptable for local use |
| **Chart.js JSInterop** | Client-side rendering dependency on JavaScript | Chart.js is lightweight and stable; fallback to static SVG/HTML charts if JS fails |

**Scalability:** Not a concern for this scope. Single-page, local-first design. If expanded to multi-project dashboards later, consider database (SQLite for local) and lightweight ORM (Dapper or Entity Framework Core).

## Open Questions

1. **Data source in production:** Will data.json be manually edited, or should a simple admin page for editing be added?
2. **Refresh strategy:** How often should the dashboard refresh? Auto-reload every N minutes, or manual refresh only?
3. **Mobile viewing:** Are executives viewing on tablets/mobile, or desktop only? (Affects Bootstrap grid breakpoints)
4. **Archival:** Do historical project dashboards need to be preserved, or is only the current project shown?
5. **Multi-project support:** Scope locked to single project, or should there be project selection dropdown?

## Implementation Recommendations

**MVP Scope (Week 1):**
1. Create Blazor Server project with Bootstrap 5 template
2. Build Dashboard.razor layout (header, 3-column grid)
3. Create MilestoneTimeline and ProgressBars components
4. Create data.json with fake project data (5 milestones, 12 tasks)
5. Implement DataService.ReadProjectData() to load and deserialize
6. Integrate Chart.js for milestone timeline (via JSInterop)
7. Deploy locally and test screenshot export (Print to PDF via browser)

**Quick Wins:**
- Use existing Bootstrap template—CSS already polished, no custom styling needed
- Fake data in JSON (no real project integration phase 1)—focus on UI/UX
- Chart.js CDN link (no npm/build required)—simple to integrate

**Deferred (Phase 2):**
- Data editing UI (admin page to update data.json via form)
- Auto-refresh via SignalR or polling
- Historical archival/project switching
- Unit tests (bUnit)
- Logging and observability

**Prototype Recommendation:**
Create a rough HTML static file matching OriginalDesignConcept.html with fake data first (no Blazor). Once design is approved, convert to Blazor components for interactivity and cleaner code reuse.
