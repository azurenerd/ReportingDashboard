# PM Specification: My Project

## Executive Summary

Create a lightweight, screenshot-optimized web dashboard for executive visibility into project milestones and progress. Built on Blazor Server (.NET 8) with JSON-based configuration, this single-page application enables PowerPoint-ready reporting without enterprise infrastructure, authentication, or database dependencies. Executives will embed dashboard screenshots directly into presentation decks to communicate project status (milestones, shipped/in-progress/carryover items, timeline) with minimal maintenance overhead.

## Business Goals

1. Enable executives to view project status at a glance—milestones, shipped/in-progress/carryover work items—within 3 seconds of page load
2. Support 100% screenshot-based reporting workflow; users take browser screenshots and embed them directly in PowerPoint slides
3. Eliminate dependency on complex BI tools, cloud services, or database infrastructure—single JSON file (data.json) is source of truth
4. Maintain visual fidelity across browser rendering, print preview, and PowerPoint embedding at target resolution (1920x1080)
5. Reduce ongoing maintenance burden to simple data.json file updates; no code changes required for status updates
6. Provide clear, color-coded milestone status indicators (Completed/In Progress/At Risk) and work item categorization for executive clarity

## User Stories & Acceptance Criteria

### Story 1: View Project Timeline with Milestone Status

**As an** executive sponsor  
**I want to** see all project milestones in chronological order with clear status indicators  
**So that** I can quickly assess which milestones are on track, delayed, or completed

**Acceptance Criteria:**
- [ ] Milestone timeline displays all milestones from data.json in chronological order (earliest to latest)
- [ ] Each milestone shows: Name, Target Date, Status (Completed/In Progress/Planned/At Risk)
- [ ] Status indicators use color coding: Green=Completed, Yellow=In Progress, Red=At Risk
- [ ] Days remaining/overdue automatically calculated from target date vs. current date
- [ ] Milestones >= 3 days overdue flagged as "At Risk" in red
- [ ] Milestone description/details visible on hover or expandable section
- [ ] Timeline renders correctly in browser print preview (Chrome DevTools, Firefox Print Preview)

### Story 2: View Work Item Summary by Category

**As an** executive  
**I want to** see counts and list of work items grouped by status (Shipped, In Progress, Carryover)  
**So that** I can assess what has been delivered and what remains at risk

**Acceptance Criteria:**
- [ ] Dashboard displays summary cards showing: # Shipped, # In Progress, # Carryover
- [ ] Cards include count badges with consistent color scheme
- [ ] Summary total counts match data.json workItems array
- [ ] Overall project % complete calculated and displayed prominently
- [ ] Summary updates within 5 seconds of data.json file change
- [ ] Carryover items show original target date and new target date
- [ ] Carryover reason visible (e.g., "Unexpected downstream dependency discovered")
- [ ] Cards render without text overflow or layout shift when printed

### Story 3: Export Dashboard as Screenshot for PowerPoint

**As an** executive communicator  
**I want to** take a browser screenshot of the dashboard and embed it in a PowerPoint slide  
**So that** I can present status to stakeholders without building custom slides

**Acceptance Criteria:**
- [ ] Dashboard renders consistently at 1920x1080 resolution (target display size)
- [ ] Print styles optimize spacing, fonts, and colors for PowerPoint slide embedding
- [ ] Navigation chrome (header, footer, reload button) hidden in print view
- [ ] Milestone timeline and work item tables fit within standard PowerPoint slide bounds (8.5" x 11" equivalent)
- [ ] Text remains readable at typical presentation zoom levels (minimum 11pt font)
- [ ] All status colors and badges render correctly in print preview
- [ ] Export via browser Print > Save as PDF produces pixel-perfect fidelity
- [ ] Screenshot quality tested on Chrome DevTools device emulation (1920x1080)

### Story 4: Load Project Data from JSON Configuration

**As a** dashboard maintainer  
**I want to** update project status by editing a simple data.json file  
**So that** I can quickly reflect changes without modifying code or database

**Acceptance Criteria:**
- [ ] Dashboard loads data from wwwroot/data/data.json on startup
- [ ] JSON schema matches documented structure (ProjectMetadata, Milestones, WorkItems)
- [ ] Missing or invalid JSON fields handled gracefully with error message
- [ ] Dashboard detects data.json changes and reloads within 5 seconds (FileSystemWatcher)
- [ ] All connected browser sessions receive updated data after file change
- [ ] Malformed JSON produces user-friendly error (not stack trace)
- [ ] Data validation ensures required fields present (Name, Date, Status for milestones)
- [ ] Sample data.json provided with fictional project (5-10 milestones, 10-15 work items)

### Story 5: View Detailed Work Item Information

**As an** executive  
**I want to** see work item titles, milestone associations, and completion status  
**So that** I can drill into specific areas of concern or progress

**Acceptance Criteria:**
- [ ] Shipped items table shows: Title, Associated Milestone, Completion Date
- [ ] In Progress items table shows: Title, Associated Milestone, % Complete
- [ ] Carryover items table shows: Title, Original Target Date, New Target Date, Reason
- [ ] Work item count per milestone visible (e.g., "Milestone: Phase 1 (3 items)")
- [ ] No horizontal scrolling required on 1920px wide displays
- [ ] Table rows alternate background color for readability
- [ ] Print layout maintains table structure and alignment

### Story 6: Manual Data Refresh

**As a** dashboard viewer  
**I want to** refresh the dashboard without reloading the page  
**So that** I can see latest data if I know changes were made to data.json

**Acceptance Criteria:**
- [ ] Refresh button visible on dashboard
- [ ] Clicking refresh loads latest data.json without full page reload
- [ ] User feedback (loading spinner or toast notification) during refresh
- [ ] Refresh completes within 2 seconds
- [ ] Dashboard remains responsive during refresh (no freezing)

## Scope

### In Scope

- Blazor Server web application running on Windows (.NET 8 LTS)
- Single-page dashboard displaying milestones and work items from data.json
- JSON-based data loading and validation (System.Text.Json)
- Print-optimized CSS for PowerPoint screenshot export (@media print rules)
- Five reusable sub-components: MilestoneTimeline, ProjectStatusCard, ShippedItemsList, CarryoverIndicator, ProgressIndicator
- FileSystemWatcher for automatic data.json reload detection
- Sample data.json with fictional project (8-10 milestones, 10-15 work items)
- Bootstrap 5.3.x + custom CSS styling
- Basic date calculations (days remaining, overdue detection, UTC to local conversion)
- Manual refresh button for on-demand data reload
- Self-contained Windows .exe deployment (no runtime dependencies)
- Error handling and user-friendly messages for malformed JSON
- DashboardService with LoadDataAsync and ValidateDataAsync methods
- Color-coded status badges and milestone indicators

### Out of Scope

- User authentication, authorization, or role-based access control
- Multi-project support (single dashboard per instance; no dropdown project selector)
- Historical data, audit trails, or versioning of work item changes
- Automated email reports or server-side PDF generation (SelectPdf)
- Multi-timezone support (single timezone assumed; UTC assumed internally)
- Database backend (SQLite, SQL Server, or cloud databases)
- REST API endpoints or external data source integrations
- Dashboard customization UI or layout builder
- Advanced charting (burndown charts, velocity, cumulative flow diagrams)
- Real-time collaboration, commenting, or multi-user editing
- Mobile-responsive layout optimization
- Dark mode or theme switching
- Internationalization or localization (English only)
- Search, filtering, or sorting of work items
- Drag-and-drop timeline editing or status updates
- Push notifications or status change alerts
- Integration with Azure DevOps, Jira, GitHub, or other work tracking systems
- Complex dependency tracking or critical path analysis
- Gantt chart visualization

## Non-Functional Requirements

### Performance

- Dashboard renders within 200ms for target dataset (30-50 work items)
- Initial page load completes in < 3 seconds (including CSS, Bootstrap, and component rendering)
- JSON parsing via System.Text.Json completes in < 100ms for typical data.json
- Hot-reload detection and UI update completes within 5 seconds of data.json file change
- Maximum WebSocket payload per update: 10 KB
- Memory footprint per connected session: < 250 MB
- Support for 30+ concurrent user sessions on single Windows Server 2022 machine
- Dashboard rendering time (80-150ms for Blazor Server with 30 work items)
- Total HTTP payload: ~120 KB initial load + ~10 KB per refresh

### Reliability

- Dashboard gracefully handles missing or malformed data.json (error banner displayed, no crash)
- Automatic WebSocket reconnection on network disconnect (Blazor Server native)
- File read/write errors logged and displayed to user with retry option
- Graceful degradation; dashboard displays stale data during file unavailability (30-second tolerance)
- Data validation prevents loading of incomplete milestone/work item records
- No data loss during concurrent read/write scenarios (single-threaded write assumption honored)

### Scalability

- Single Windows machine deployment (no clustering, load balancing, or cloud scaling)
- Estimated capacity: 50-100 concurrent users on standard Windows Server 2022 hardware (8+ CPU, 16+ GB RAM)
- Dataset up to 100 work items without performance degradation
- No horizontal scalability required per project constraints
- Not designed for high-traffic or public-facing scenarios

### Security (Minimal - Intranet Only)

- No authentication or authorization required (assumes intranet/trusted network access)
- HTTPS enforced in production deployment (IIS or nginx reverse proxy with valid SSL certificate)
- JSON file secured via OS-level ACLs (Windows NTFS permissions); service account read-only access
- No encryption at rest for data.json (non-sensitive project metadata only)
- WebSocket communication encrypted via TLS/SSL in production
- No sensitive data (credentials, API keys, passwords) stored in data.json
- Assume network-level access controls prevent unauthorized access to dashboard URL

### Availability

- No uptime SLA required (internal reporting tool for intranet use)
- Graceful degradation during file access errors or missing data.json
- Manual restart procedure documented (Windows Service via NSSM or Task Scheduler)
- Acceptable downtime for maintenance: no 24/7 availability requirement

### Compatibility

- Supported browsers: Chrome 90+, Edge 90+, Firefox 88+ (WebSocket requirement)
- Target display resolution: 1920x1080 (desktop only; no mobile optimization)
- Print export: Chrome and Edge native print-to-PDF functionality
- Deployment platform: Windows Server 2022+, Windows 10/11 Pro
- No cross-platform deployment or Linux/macOS support required

### Data Integrity

- No concurrent writes to data.json (single-threaded file write assumption)
- Data validation enforced via C# data annotations (Required, format validation)
- Historical snapshots not preserved (only current state displayed)
- ISO 8601 UTC dates required in data.json; local rendering applied in UI
- No automatic carryover detection (manually curated by data maintainer)

## Success Metrics

### Delivery Metrics

- [ ] All 6 user stories accepted and signed off by stakeholder
- [ ] Dashboard prototype reviewed against OriginalDesignConcept.html and ReportingDashboardDesign.png design references
- [ ] Sample data.json with realistic fictional project (Q2 Cloud Migration scenario) created and loading successfully
- [ ] Dashboard renders without console errors or warnings in Chrome/Edge
- [ ] Self-contained .exe builds and runs without external dependencies

### Quality Metrics

- [ ] Print preview (Chrome DevTools) matches reference design screenshot
- [ ] Zero horizontal scrolling at 1920x1080 browser resolution
- [ ] Milestone timeline and work item tables fit within PowerPoint slide bounds (8.5" x 11" equivalent)
- [ ] All text remains readable at 96 DPI print resolution; minimum 11pt font size
- [ ] Page load time measured at < 3 seconds (Lighthouse/DevTools performance audit)
- [ ] JSON parsing time < 100ms for 30-50 work items
- [ ] WebSocket payload < 10 KB per update
- [ ] Memory footprint < 200 MB baseline

### User Acceptance Metrics

- [ ] Dashboard embedded in PowerPoint slide and reviewed by intended executive audience
- [ ] Executive feedback confirms readability and visual clarity for presentation context
- [ ] Stakeholders confirm all required status information visible at a glance (no drilling required)
- [ ] Screenshot quality deemed suitable for executive-level stakeholder distribution
- [ ] Carryover items, milestone status, and progress percentages accurate and complete

### Technical Metrics

- [ ] DashboardService LoadDataAsync and ValidateDataAsync tested with valid and malformed JSON
- [ ] FileSystemWatcher detects data.json changes and reloads within 5 seconds
- [ ] Memory footprint measured < 200 MB baseline during normal operation
- [ ] Rendering time logged < 200ms for 30-50 work items
- [ ] All status color codes render correctly in print preview (Chrome DevTools print simulation)
- [ ] Date calculations (days remaining, overdue detection) verified for edge cases (DST transitions)

### Scope Adherence Metrics

- [ ] No authentication layer implemented
- [ ] No database or external APIs integrated
- [ ] No charting beyond RadzenBlazor timeline/status components
- [ ] Single-project, single-timezone implementation confirmed
- [ ] Zero custom JavaScript interop required
- [ ] Bootstrap 5.3 used for base layout; custom CSS < 500 lines

## Constraints & Assumptions

### Technical Constraints

- Single Windows machine deployment only (no clustering, load balancing, or multi-region support)
- No cloud infrastructure dependencies (Azure, AWS, or hosted services)
- File-based JSON only (no database backend, no API gateways, no message queues)
- Self-contained .NET 8 executable deployment model (all dependencies bundled; ~200 MB publish output)
- Single-timezone support (UTC assumed for data storage; local time for rendering)
- No multi-service or concurrent write support to data.json (single writer assumption)
- Blazor Server only (no Blazor WebAssembly or hybrid rendering modes)
- Windows-only deployment (no Linux/macOS/Docker support)

### Functional Constraints

- No multi-project support (one dashboard instance = one project; no project selector dropdown)
- No user authentication or role-based access (assume intranet network access controls)
- No historical audit trails or snapshot versioning (current state only)
- No concurrent writes to data.json (single maintainer assumption)
- No enterprise security requirements (assume network-level access control)
- No real-time push notifications or status change alerts
- No offline-mode or progressive web app (PWA) functionality

### Design Constraints

- Screenshot-optimized only (print fidelity paramount; mobile responsiveness deferred)
- Simple, clean design inspired by OriginalDesignConcept.html (no heavy animations or custom graphics)
- Bootstrap 5.3 + minimal custom CSS (no Tailwind, Material Design, or third-party UI frameworks except RadzenBlazor)
- No custom JavaScript interop or client-side rendering logic
- No real-time collaboration, commenting, or multi-user conflict resolution
- Print layout fixed for 8.5" x 11" slide dimensions (no responsive print styles per paper size)

### Business & Organizational Assumptions

- Executives or designated maintainers will manually edit data.json to update project status (no automated data pipeline)
- Dashboard will be embedded in PowerPoint slides for executive briefings (not used for live monitoring or dashboards)
- No automated report generation or scheduled email distribution required at MVP stage
- Project metadata (name, description, start/end dates) remains static after initial configuration
- Work items are pre-defined in data.json; no UI for creating, editing, or deleting items
- Carryover reasons and new target dates provided by project manager (no automatic detection or calculation)
- Overall % complete either calculated from work item completion or manually provided in summary object
- No performance benchmarking or comparison against Jira, Azure Boards, or other tools required

### Data & Technology Assumptions

- data.json provided by product owner or designated data maintainer before dashboard deployment
- All milestone dates in data.json are in future or recently completed (no historical validation logic required)
- Work item category assignments (Shipped/In Progress/Carryover) are manually curated and accurate
- ISO 8601 UTC date format enforced in data.json (e.g., "2026-04-30T23:59:59Z")
- Single-threaded file writes to data.json are sufficient (no file locking, advisory locks, or database-like concurrency)
- DateTimeOffset UTC conversion and local rendering handles all timezone concerns
- Bootstrap 5.3 CDN or local package available during build and deployment
- .NET 8 SDK (LTS) available on development machine and build server

### Team & Infrastructure Assumptions

- Development team familiar with C# and .NET (or willing to learn Blazor Server component model)
- Visual Studio 2022 Community/Professional available for development
- Windows Server 2022 or Windows 10/11 Pro available for deployment/testing
- Standard Windows infrastructure (no specialized containers, orchestration, or cloud provisioning required)
- No cross-platform deployment or CI/CD pipeline complexity required
- Target viewers use Chrome/Edge browsers (WebSocket support required; no IE11 compatibility needed)
- Network is secure intranet (no VPN tunneling, external DMZ, or WAF protection required)
- File system permissions manageable via Windows NTFS ACLs (no complex identity/access management system)

---

**Specification Owner:** [PM Name]  
**Date Created:** 2026-04-09  
**Version:** 1.0 (Draft)  
**Status:** Awaiting Stakeholder Review & Sign-Off  
**Next Step:** Schedule design review with executive sponsor to validate scope, accept user stories, and clarify open questions