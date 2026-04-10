# PM Specification: My Project

## Executive Summary

Build a lightweight, screenshot-optimized executive dashboard for project milestone and progress reporting using Blazor Server and JSON data persistence. The dashboard enables PMs to quickly visualize project health (completion %, task status, milestone timeline) and generate PowerPoint-ready screenshots without manual data manipulation. Target: 3-week MVP with zero cloud infrastructure, manual monthly data refresh, <10 concurrent users.

## Business Goals

1. Enable executives to assess project health at a glance during status meetings and presentations
2. Eliminate email/spreadsheet-based status reporting workflow by providing single source of truth dashboard
3. Generate PowerPoint-ready screenshots with crisp typography and professional Material Design aesthetic
4. Support PM-driven monthly data updates with minimal operational overhead (edit JSON, refresh, screenshot)
5. Establish zero-downtime reporting capability for 10+ concurrent internal stakeholders
6. Provide visual indicators for project risk (on-track, at-risk, delayed milestones)

## User Stories & Acceptance Criteria

### US1: View Project Overview Dashboard
**As an** executive  
**I want** to see a single-page project dashboard with completion percentage and project status  
**So that** I can quickly assess overall project health for presentations

**Acceptance Criteria:**
- [ ] Dashboard displays project name, start date, and completion percentage prominently
- [ ] Page loads in <2 seconds on default Kestrel (localhost:5000)
- [ ] Layout responsive across 1920x1080 (primary) and 1366x768 (secondary) resolutions
- [ ] Screenshot quality acceptable for PowerPoint 16:9 (crisp text, clean colors, no rendering artifacts)
- [ ] All metrics derive from data.json file (no hard-coded values except sample data)
- [ ] Overall project status displayed (On Track, At Risk, Blocked)

### US2: View Task Status Breakdown by Category
**As a** PM  
**I want** to see tasks grouped by status categories (Completed, In Progress, Carried Over)  
**So that** I can communicate progress and blockers to stakeholders

**Acceptance Criteria:**
- [ ] Dashboard displays three color-coded status cards: Completed (green), In Progress (blue), Carried Over (gray)
- [ ] Each card shows task count and visual indicator
- [ ] Counts calculated from data.json Tasks array, aggregated by Status field
- [ ] Sum of all status counts equals total task count
- [ ] Cards use MudBlazor components for consistent Material Design appearance

### US3: View Milestone Timeline with Progress
**As an** executive  
**I want** to see key project milestones displayed chronologically with due dates and progress percentage  
**So that** I understand major deliverables and track big rocks

**Acceptance Criteria:**
- [ ] Horizontal milestone timeline displays 3-5 milestones in chronological order
- [ ] Each milestone shows: name, due date, percent complete, status indicator
- [ ] Timeline rendered with CSS (no Chart.JS; ensures crisp screenshot quality)
- [ ] Visual distinction for overdue milestones (red border or warning icon)
- [ ] Milestone status determined by due date vs. current date (On Track / Overdue)
- [ ] Percent complete calculated from associated tasks in data.json

### US4: Refresh Dashboard Data Without Restarting
**As a** PM  
**I want** to refresh dashboard data from data.json without restarting the application  
**So that** I can update project status quickly after editing the data file

**Acceptance Criteria:**
- [ ] "Refresh Data" button visible on dashboard triggers data.json reload
- [ ] Service re-aggregates KPIs within 500ms of refresh click
- [ ] UI updates via Blazor StateHasChanged() without full page refresh
- [ ] Manual F5 browser refresh also loads latest data.json
- [ ] Refresh succeeds even if data.json edited externally while app running
- [ ] No data corruption or partial reads during concurrent edits

### US5: Unit Test KPI Aggregation Logic
**As a** developer  
**I want** automated unit tests for ProjectDataService KPI calculation  
**So that** aggregation bugs are caught before deployment

**Acceptance Criteria:**
- [ ] xUnit test suite with 80%+ code coverage of ProjectDataService
- [ ] Tests validate: completion percentage calculation, task count rollups, status determination
- [ ] Tests mock file I/O (no dependency on actual data.json during test)
- [ ] Tests cover edge cases: zero tasks, all completed, overdue milestones
- [ ] All tests pass before deployment
- [ ] Test results viewable in CI pipeline or local test runner

## Scope

### In Scope

- Single-page Blazor Server application (C# .NET 8.0)
- JSON file-based data persistence using System.Text.Json
- MudBlazor 6.10.0 UI component library with Material Design 3
- Four status cards (Completed, In Progress, Carried Over, total project completion %)
- Milestone timeline rendered with HTML/CSS horizontal bars
- Manual refresh button tied to ProjectDataService.ReloadFromJsonAsync()
- Unit test suite for ProjectDataService (80% code coverage target)
- Kestrel standalone deployment package (~50-60MB)
- Optional Windows Service wrapper using TopShelf 4.3.0
- Sample data.json with 3-5 tasks and 2-3 milestones
- Basic README documentation (data.json schema, deployment steps, refresh workflow)
- ProjectDataService KPI aggregation (completion %, task counts, status rollup)
- DataRepository abstraction for data.json file I/O
- DashboardViewModel pre-computed metrics (no component-level calculation)

### Out of Scope

- User authentication or authorization (internal tool, no login required)
- HTTPS/TLS encryption in transit (localhost or internal network only)
- Multi-project dashboard (single project per instance)
- PDF or PowerPoint export capability (browser screenshots sufficient)
- Historical data comparison views or month-over-month trending
- Real-time auto-polling or file watchers (manual F5 refresh only)
- Advanced charting (Gantt timelines, drill-down analysis, interactive charts)
- Audit logging or change tracking (who edited data and when)
- Mobile-responsive design optimization (desktop/laptop primary)
- Blazor component unit tests via bUnit (presentation-only components, low ROI)
- Relational database backend (JSON file only)
- Cloud deployment (Azure, AWS, etc.; local/on-premises only)
- API integrations or real-time data sources
- Dark mode or user-configurable color schemes (light Material Design only)
- Data validation JSON schema enforcement
- Email notifications or alerts

## Non-Functional Requirements

### Performance

- Dashboard page load time: <2 seconds (startup from cold Kestrel process, localhost)
- JSON deserialization and KPI re-aggregation: <500ms (refresh button click to UI update)
- KPI calculation at app startup: <100ms
- Memory footprint: <100MB (in-memory cache sufficient for ~500 projects)
- Refresh button response time (StateHasChanged() latency): <100ms

### Reliability

- Kestrel process auto-restart on crash (via Windows Service wrapper or Task Scheduler)
- 99% uptime acceptable for internal tool (monthly data refresh cadence, no 24/7 SLA)
- Manual refresh operation idempotent (multiple clicks safe, no data corruption)
- Dashboard gracefully handles missing or malformed data.json (displays error message)
- No data loss on refresh or application restart

### Security

- data.json file remains server-side only (never exposed to browser client)
- OS-level file permissions restrict data.json read/write to dashboard application user account
- No external API calls, third-party services, or cloud dependencies
- Localhost or internal network only (HTTPS not required for MVP; add if deployed externally)
- SQL injection not applicable (JSON parsing, not database queries)
- No secrets, API keys, or credentials in data.json or configuration files

### Scalability

- Single Kestrel instance sufficient for <10 concurrent users (internal stakeholder audience)
- data.json file maximum 10MB (executive dashboards rarely exceed 500 projects)
- No database scaling, connection pooling, or read replicas required
- Stateless service architecture allows horizontal scaling if needed later (not MVP requirement)
- WebSocket connection overhead ~2KB per active user

### Maintainability

- Service layer KPI logic centralized in ProjectDataService.cs (single point of modification)
- Blazor components purely presentational (no business logic, data binding only)
- Layered architecture: Presentation (Pages) → Services → Models → Data Layer
- All aggregation logic unit-tested with 80%+ coverage
- Code follows C# .NET 8 conventions (PascalCase, async/await, dependency injection)

## Success Metrics

- [ ] PM can edit data.json, click "Refresh Data" button, and take screenshot for PowerPoint within 5 minutes
- [ ] Dashboard screenshot prints to PDF with crisp typography and readable colors (no rendering artifacts)
- [ ] Completion percentage KPI calculation verified by unit tests (80%+ code coverage)
- [ ] All user story acceptance criteria met and signed off by stakeholder
- [ ] Deployment package (<60MB) runs on Windows Server or Windows Pro without .NET SDK installed
- [ ] Zero unplanned downtime over 1-month pilot deployment
- [ ] ProjectDataService aggregation logic accurate against manual task count verification
- [ ] Milestone timeline correctly displays overdue status for past-due dates
- [ ] Dashboard responsive layout tested on 1920x1080 and 1366x768 resolutions

## Constraints & Assumptions

### Technical Constraints

- Framework locked to Blazor Server (.NET 8.0 only; WASM and MVC explicitly rejected)
- Deployment target: Windows Server 2016+ or Windows Pro/Home (Kestrel)
- JSON file storage only (no relational database, no cloud blob storage)
- Single machine deployment (no load balancing, no multi-region)
- Minimal custom CSS (MudBlazor theming primary, <100 lines custom CSS)
- No JavaScript interop required (MudBlazor + ChartJS.Blazor only)
- <50 concurrent users (internal stakeholder audience maximum)
- data.json kept <10MB (in-memory caching requirement)

### Timeline & Phasing

- Phase 1 (Week 1): Core infrastructure (Blazor setup, data.json schema, ProjectDataService, unit tests)
- Phase 2 (Week 2): Dashboard UI (DashboardPage.razor, status cards, milestone timeline)
- Phase 3 (Week 3): Polish and deployment (styling, responsive layout, Kestrel/TopShelf packaging)
- MVP delivery target: End of Week 3
- Quick win checkpoint: Phase 1 + 2 complete by end of Week 2 (screenshot-ready)

### Dependency Assumptions

- .NET 8.0 SDK and runtime available on development machine
- .NET 8.0 runtime available on deployment target (Windows Server/Pro)
- NuGet package repository accessible (public NuGet.org)
- PM comfortable editing JSON files in text editor (basic JSON syntax knowledge)
- Executives use modern browsers (Chrome, Edge, Firefox; IE11 not required)
- Git version control in place (standard .sln project structure)
- Visual Studio 2022 (17.8+) or VS Code + OmniSharp available for development

### Data & Operational Assumptions

- Project data updates monthly (not real-time), making manual F5 refresh acceptable
- data.json edited manually by PM (no API ingestion or automated data sources)
- No regulatory or compliance logging required (internal tool, trusted environment)
- Single project instance per dashboard installation (no multi-project filtering)
- Executive stakeholders <10 concurrent users (no connection scaling concerns)
- Windows Service hosting preferred over Docker (operational simplicity)
- No HTTPS required for MVP (localhost or internal network only)