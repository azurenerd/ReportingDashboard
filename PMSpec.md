# PM Specification: My Project

## Executive Summary

My Project is a lightweight executive reporting dashboard built on C# .NET 8 with Blazor Server that visualizes project milestones, progress, and status from a JSON configuration file. The dashboard transforms project data into an executive-ready, screenshot-optimized view that displays shipped deliverables, in-progress work, carried-over items, and milestone timelines. It prioritizes simplicity, local deployment, and professional visualization suitable for PowerPoint executive decks—enabling project leaders to communicate project health and progress without friction.

## Business Goals

1. **Reduce reporting friction.** Enable project leaders to generate professional executive dashboards with a single click, eliminating manual slide creation and spreadsheet-based tracking.
2. **Increase visibility into project health.** Provide executives with a single authoritative view of project status (Shipped/InProgress/CarriedOver) and milestone progress in real time.
3. **Enable rapid deck iteration.** Support quick updates to dashboard data (via JSON) without code changes or developer involvement, allowing project leaders to iterate on executive communications.
4. **Ensure executive accessibility.** Deploy as a self-contained Windows executable that executives can run locally without .NET SDK installation, cloud access, or IT setup.
5. **Maintain simplicity for screenshots.** Design dashboard UI for clean, professional screenshot capture with no interactive elements that render poorly in static images.
6. **Support zero-friction adoption.** Eliminate authentication, multi-user complexity, cloud infrastructure, and security overhead—the dashboard is local-only, file-based, and immediately usable.

## User Stories & Acceptance Criteria

### User Story 1: View Project Milestone Timeline
**As a project leader,** I want to see all project milestones displayed in chronological order with status indicators, so that I can quickly assess what's on track, at risk, or completed.

**Acceptance Criteria:**
- [ ] Milestones render as a horizontal timeline or Gantt chart
- [ ] Each milestone displays: name, due date, and current status (Not Started/In Progress/Shipped)
- [ ] Milestones completed in the past are marked green (Shipped); upcoming are yellow (In Progress) or gray (Not Started)
- [ ] Past-due milestones highlight in red
- [ ] Timeline is sticky (remains visible when scrolling item list below)
- [ ] Timeline renders cleanly in screenshot (no hover-dependent elements, fixed font sizes)

---

### User Story 2: View Progress Summary Cards
**As an executive,** I want to see summary counts of Shipped, In Progress, and Carried Over deliverables, so that I can assess overall project momentum at a glance.

**Acceptance Criteria:**
- [ ] Dashboard displays 3 summary cards above the timeline: Shipped, In Progress, Carried Over
- [ ] Each card shows count and visual indicator (green/yellow/gray)
- [ ] Summary cards update when data.json is reloaded
- [ ] Cards render legibly in PowerPoint screenshots (minimum 16px font, high contrast)

---

### User Story 3: Filter and Sort Project Items
**As a project lead,** I want to filter items by category (Shipped/In Progress/Carried Over) and sort by priority/due date, so that I can focus on specific status groups or upcoming work.

**Acceptance Criteria:**
- [ ] Item table includes filter buttons for each category
- [ ] Clicking a category button filters displayed items to that category
- [ ] "All" filter button shows all items
- [ ] Table sorts by priority (High/Medium/Low) by default; clicking column headers resorts
- [ ] Clicking same header twice reverses sort order
- [ ] Filter state persists in URL query string for bookmarking

---

### User Story 4: View Item Details
**As an executive,** I want to see detailed information for each project item (title, owner, due date, priority), so that I can understand who is responsible and when work is due.

**Acceptance Criteria:**
- [ ] Item table displays columns: Title, Category, Owner, Due Date, Priority
- [ ] Rows are readable and sortable
- [ ] Due dates display in readable format (e.g., "Apr 15, 2026")
- [ ] Overdue items are highlighted (red background or icon)
- [ ] Table is responsive (single column on mobile, multi-column on desktop)

---

### User Story 5: Capture Dashboard Screenshot for PowerPoint
**As a project leader,** I want to screenshot the dashboard and embed it in a PowerPoint deck, so that I can share project status with executives without manual slide creation.

**Acceptance Criteria:**
- [ ] Dashboard renders cleanly when screenshotted (no broken layouts, proper spacing)
- [ ] Colors and text remain legible in grayscale (for printing)
- [ ] Interactive elements (buttons, filters) do not interfere with static screenshot
- [ ] Dashboard fits on a single screen (no scrolling required for overview)
- [ ] Project name and reporting period are visible at top of dashboard

---

### User Story 6: Update Project Data Without Restarting
**As a project manager,** I want to update project data by editing data.json and see changes reflected immediately in the dashboard without restarting the app, so that I can iterate quickly on reporting.

**Acceptance Criteria:**
- [ ] Dashboard monitors data.json for changes
- [ ] When data.json is updated and saved, dashboard automatically reloads data within 2 seconds
- [ ] UI reflects new data (new milestones, updated statuses, new items)
- [ ] If data.json contains errors, dashboard displays error message and retains previous valid data
- [ ] File system watcher handles rapid successive edits gracefully (debounced)

---

### User Story 7: Launch Dashboard Locally Without Prerequisites
**As an executive,** I want to double-click an executable to launch the dashboard without installing .NET, configuring anything, or using a command line, so that I can run it immediately.

**Acceptance Criteria:**
- [ ] Dashboard is distributed as a single .exe file (self-contained)
- [ ] .exe includes all required runtime and dependencies
- [ ] Double-clicking .exe launches dashboard in default browser at https://localhost:5001
- [ ] No .NET SDK or runtime installation required
- [ ] data.json is bundled alongside .exe (or placed in same directory manually)

---

### User Story 8: Share Dashboard with Colleagues
**As a project leader,** I want to share the dashboard executable and data.json with colleagues via email or file share, so that everyone can view project status independently.

**Acceptance Criteria:**
- [ ] Dashboard reads data.json from local file system (no cloud dependency)
- [ ] Executable + data.json can be copied to network share or USB drive and run from any location
- [ ] No hardcoded file paths; app finds data.json relative to executable
- [ ] Multiple instances can run simultaneously on different machines with same/different data.json files

---

### User Story 9: Validate Project Data on Load
**As a system,** I want to validate data.json structure at startup and provide clear error messages if schema is invalid, so that data entry errors surface immediately and prevent silent failures.

**Acceptance Criteria:**
- [ ] On startup, dashboard validates data.json against defined schema
- [ ] If data.json is missing required fields (e.g., projectName, timeline, items), dashboard shows error message
- [ ] If data.json is malformed JSON, dashboard shows "Invalid JSON" error with line number (if possible)
- [ ] Error messages are user-friendly (not stack traces)
- [ ] Dashboard retains last valid data if current load fails; error is displayed but app remains functional

---

### User Story 10: Track Project Progress Across Multiple Reporting Periods
**As an executive,** I want to update data.json with new reporting periods (e.g., monthly progress snapshots), so that I can track how project status evolves over time without losing historical context.

**Acceptance Criteria:**
- [ ] data.json supports reportingPeriod field (e.g., "Q2 2026", "April 2026")
- [ ] Dashboard displays current reporting period at top
- [ ] Multiple data.json files (one per period) can be maintained in Git or shared folder
- [ ] Project leader can swap data.json to view different periods
- [ ] Summary metrics (total shipped/in-progress/carried-over) are attributable to specific reporting period

---

## Scope

### In Scope
- Single-page Blazor Server dashboard displaying project milestones and items
- Gantt-style timeline visualization with color-coded status (Shipped/In Progress/Carried Over/Not Started)
- Summary cards showing counts by category
- Item grid with sort/filter by category, priority, and due date
- Read-only data model (no editing UI; data.json edited externally)
- Local file I/O (data.json loaded from local disk)
- Auto-refresh on data.json file change (FileSystemWatcher)
- Self-contained Windows executable deployment (~180MB)
- Bootstrap 5 responsive layout (mobile/desktop support)
- Print CSS optimization for screenshot capture
- Schema validation for data.json (JsonSchema.NET)
- Error handling and user-friendly error messages
- Basic logging to local file (errors only)
- Unit tests for DataProvider and core business logic
- Component tests for UI rendering (Bunit)

### Out of Scope
- Multi-user access or authentication/authorization
- Cloud deployment or web hosting
- Real-time data synchronization or WebSocket updates
- Database backend (SQLite, SQL Server, etc.)
- Editing UI for project data (inline forms, modal editors)
- Advanced charting (Plotly, D3.js) beyond ApexCharts Gantt
- PDF export or advanced report generation
- Mobile-native app (Xamarin, React Native)
- Jira/Azure DevOps integration or API connectors
- Notification system or alerting
- Role-based dashboard customization
- Dashboard theme switching or dark mode
- Multi-language localization
- Horizontal scaling or load balancing
- Docker containerization (Windows executable is primary distribution)
- Automated data refresh from external sources
- Gantt chart editing or drag-and-drop rescheduling
- Dashboard API for third-party integrations

---

## Non-Functional Requirements

### Performance
- **Data load time:** Dashboard loads and renders within 2 seconds on first launch
- **Filter/sort response:** Category filtering and sorting complete within 100ms (client-side)
- **File monitor latency:** Dashboard detects data.json changes and reloads within 2 seconds
- **Supported data scale:** Dashboard handles up to 1,000 milestones and 10,000 items without perceptible lag
- **Memory footprint:** Application footprint <500MB RAM during typical use

### Security
- **Local-only deployment:** No cloud transmission; all data and processing remains on user's machine
- **No authentication:** Zero auth layer; assumes single trusted user per machine
- **File permissions:** data.json should be readable by app; write permissions optional (manual edits recommended)
- **Logging:** Error logs written to local file; no telemetry or external reporting

### Accessibility
- **WCAG AA compliance:** Dashboard meets WCAG 2.1 AA standards (color contrast, keyboard navigation, screen reader support)
- **Responsive layout:** Dashboard adapts to viewport sizes (mobile: single-column; desktop: multi-column)
- **Print-friendly:** Dashboard renders cleanly in screenshots and PDF print preview without page breaks

### Reliability
- **Error recovery:** Invalid data.json doesn't crash app; error message shown; previous valid data retained
- **Session timeout:** Blazor Server session remains active for 30+ minutes (configurable)
- **Graceful degradation:** If file watcher fails, manual refresh button allows data reload
- **File I/O reliability:** Async I/O prevents UI freezing during data.json read

### Scalability
- **Single-user local app:** Designed for one user per executable instance; horizontal scaling not required
- **Data volume:** Designed for projects with <1,000 milestones; limit enforced in documentation
- **Future migration path:** If data volume exceeds 10,000 items, migrate to SQLite backend (architecture supports this)

### Deployment & Distribution
- **Self-contained executable:** Single .exe file includes .NET 8 runtime and all dependencies
- **Zero installation:** No installer, no registry keys, no prerequisites
- **Portability:** Executable + data.json can run from USB drive, network share, or local disk
- **Size constraint:** Total package <200MB (acceptable for local distribution)

---

## Success Metrics

### Phase 1 Completion (MVP)
- [ ] Blazor Server app launches with `dotnet run` on developer machine
- [ ] TimelineSection renders milestone list from hardcoded sample data
- [ ] ProgressCardsSection displays Shipped/InProgress/CarriedOver counts
- [ ] Summary stats match expected counts (3 shipped, 2 in progress, 1 carried over)
- [ ] Screenshot of dashboard looks professional and fits on single screen
- [ ] No console errors; app runs for 30+ minutes without crashes

### Phase 2 Completion (MVP + Interactive Features)
- [ ] data.json is loaded from file at startup (not hardcoded)
- [ ] Schema validation passes for valid data.json; rejects invalid data with error message
- [ ] MudBlazor DataGrid renders items with sort/filter by category
- [ ] Filtering by category works (clicks update UI within 100ms)
- [ ] ApexCharts Gantt chart renders milestones with color-coded status
- [ ] FileSystemWatcher auto-refreshes dashboard when data.json changes
- [ ] Test coverage: >80% for DataProvider and core business logic
- [ ] 10+ passing xUnit tests for data loading and validation
- [ ] 5+ passing Bunit component tests for UI rendering

### Phase 3 Completion (Polish & Deployment)
- [ ] Self-contained executable published successfully: `dotnet publish -c Release -r win-x64 --self-contained`
- [ ] Executable runs on Windows 10+ machine with no .NET SDK installed
- [ ] Executable + data.json can be copied to network share and run from another machine
- [ ] Bootstrap responsive layout works on mobile viewport (320px) and desktop (1920px)
- [ ] Print/screenshot CSS verified: dashboard renders legibly in grayscale
- [ ] Error handling verified: invalid data.json displays user-friendly error message
- [ ] FileSystemWatcher verified: editing data.json while app runs triggers auto-refresh
- [ ] README.md documents: how to run executable, update data.json, interpret dashboard
- [ ] All Phase 1 + Phase 2 tests passing
- [ ] Code review completed; no critical bugs identified

### Adoption Metrics (Post-Launch)
- [ ] Executive dashboard used to generate 2+ executive reporting decks within first month
- [ ] Project leader rates ease-of-use: 4+/5 (Likert scale)
- [ ] Zero data corruption incidents (valid data.json updates tracked in Git)
- [ ] Dashboard accessed/updated at least weekly by project stakeholders

---

## Constraints & Assumptions

### Technical Constraints
- **Windows-only deployment:** Self-contained executable targets win-x64; macOS/Linux out of scope
- **.NET 8 LTS requirement:** Application requires .NET 8 runtime (LTS support through 2026)
- **Local file I/O:** All data read from local disk; no network-based data sources
- **Single-machine execution:** No clustering, load balancing, or horizontal scaling
- **Blazor Server limitation:** SignalR session timeout at 30+ minutes; expected user behavior

### Timeline Assumptions
- **Phase 1 duration:** 1-2 weeks (basic Blazor app + sample data rendering)
- **Phase 2 duration:** 2-3 weeks (data loading, filtering, charting integration)
- **Phase 3 duration:** 1-2 weeks (testing, deployment, documentation)
- **Total project timeline:** 4-7 weeks for full MVP + production deployment

### Data & Content Assumptions
- **data.json ownership:** Project manager or PM team maintains data.json accuracy; app assumes input data is valid
- **Data volume:** Project has <1,000 milestones and <10,000 items (typical project scope)
- **Update frequency:** data.json updated weekly or monthly (not real-time; manual edits assumed)
- **Historical data:** No requirement to archive or version-control historical snapshots; Git backups sufficient

### Dependency Assumptions
- **No external APIs:** Dashboard does not fetch data from Jira, Azure DevOps, or cloud services
- **No database:** SQLite or SQL Server not required; file-based JSON is sufficient
- **No identity provider:** No AAD, Okta, or other identity service; single-user local app
- **No third-party libraries beyond approved stack:** Radzen Blazor, MudBlazor, ApexCharts.Razor are approved; others require architecture review

### User & Stakeholder Assumptions
- **Executive accessibility:** Execs have Windows 10+ machine; can double-click .exe without IT support
- **PM ownership:** Project manager responsible for maintaining data.json; not shared editing workflow
- **Non-technical users:** Execs do not edit code; data.json edited in text editor or simple form (Phase 3+)
- **Adoption motivation:** Dashboard solves real reporting pain; executives will adopt if ease-of-use is high

### Business Assumptions
- **Budget:** No infrastructure cost (self-contained executable); dev time allocated for 4-7 weeks
- **Maintenance:** Post-launch support <5 hours/month (file watcher reliability, edge cases)
- **Scope lock:** Requirements in this spec are final; out-of-scope items deferred to future phases
- **Success definition:** If execs use dashboard for 2+ reporting cycles without critical issues, project is successful

---