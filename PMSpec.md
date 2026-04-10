# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, shipped features, in-progress work, and carried-over items from a simple `data.json` configuration file. The dashboard is a local-only Blazor Server application optimized for screenshot capture into PowerPoint decks, requiring no authentication, no cloud infrastructure, and no enterprise security—delivering maximum visibility into project health with minimum operational complexity.

## Business Goals

1. **Provide executive-ready project visibility** — Deliver a single-page view that communicates project status, milestone progress, and key metrics at a glance, reducing the time spent compiling status updates from multiple sources.
2. **Streamline PowerPoint reporting workflow** — Produce a screenshot-friendly layout at 1280×900 resolution that pastes cleanly into executive slide decks without manual formatting or resizing.
3. **Eliminate reporting tool complexity** — Replace ad-hoc spreadsheet and slide-based reporting with a repeatable, data-driven dashboard that any project manager can update by editing a single JSON file.
4. **Enable rapid report generation** — Allow a project manager to update `data.json` and capture a fresh screenshot in under 2 minutes, enabling weekly or bi-weekly reporting cadences without significant effort.
5. **Zero infrastructure cost** — Run entirely on a local machine with no cloud subscriptions, hosting fees, or license costs.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Overall Status

**As a** project manager, **I want** to see the project name, executive sponsor, reporting period, report date, and overall health status prominently at the top of the dashboard, **so that** anyone viewing the screenshot immediately knows which project and time period is being reported.

**Acceptance Criteria:**
- [ ] Project name is displayed as the page title/header in large, bold text.
- [ ] Executive sponsor name is displayed beneath the project name.
- [ ] Reporting period (e.g., "March 2026") and report date are clearly visible.
- [ ] Overall project status (e.g., "On Track", "At Risk", "Off Track") is displayed with a color-coded badge (green, yellow, red).
- [ ] All fields are populated from the `project` section of `data.json`.

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline of major project milestones at the top of the dashboard, **so that** I can quickly understand the project's trajectory, what has been completed, what is in progress, and what is upcoming.

**Acceptance Criteria:**
- [ ] A horizontal timeline strip is rendered across the top of the page below the project header.
- [ ] Each milestone is positioned chronologically along the timeline.
- [ ] Milestones display their title and target date.
- [ ] Milestone status is visually distinguished: Completed (filled/green), In Progress (partially filled/blue), Upcoming (hollow/gray), Delayed (red outline).
- [ ] A "Today" marker is shown on the timeline for temporal reference.
- [ ] Milestones are populated from the `milestones` array in `data.json`.
- [ ] The timeline renders cleanly at 1280px width without overflow or scrollbars.

### US-3: View Shipped Items

**As an** executive, **I want** to see a list of features and deliverables that were shipped during the reporting period, **so that** I can understand the team's output and completed commitments.

**Acceptance Criteria:**
- [ ] A "Shipped" section displays all items from the `shipped` array in `data.json`.
- [ ] Each item shows its title, description, category, and priority.
- [ ] Items are visually styled with a "completed" indicator (e.g., green checkmark or green left border).
- [ ] The section is clearly labeled and visually separated from other sections.

### US-4: View In-Progress Items

**As an** executive, **I want** to see what work is currently in progress, including completion percentage, **so that** I can gauge how much active work remains and whether the team is on track.

**Acceptance Criteria:**
- [ ] An "In Progress" section displays all items from the `inProgress` array in `data.json`.
- [ ] Each item shows its title, description, category, and priority.
- [ ] A visual progress indicator (e.g., progress bar or percentage badge) shows `percentComplete` for each item.
- [ ] Items are visually styled with an "active" indicator (e.g., blue left border).

### US-5: View Carried-Over Items

**As an** executive, **I want** to see which items were carried over from the previous reporting period and why, **so that** I can identify systemic blockers and hold teams accountable for delayed commitments.

**Acceptance Criteria:**
- [ ] A "Carried Over" section displays all items from the `carriedOver` array in `data.json`.
- [ ] Each item shows its title, description, category, priority, original target date, and reason for carry-over.
- [ ] Items are visually styled with a "warning" indicator (e.g., amber/orange left border).
- [ ] The reason for carry-over is prominently displayed (not hidden or truncated).

### US-6: View Key Metrics

**As an** executive, **I want** to see key project health metrics (e.g., velocity, bug rate, capacity) with trend indicators, **so that** I can assess project momentum without reading detailed reports.

**Acceptance Criteria:**
- [ ] A metrics section displays all items from the `metrics` array in `data.json`.
- [ ] Each metric shows its label, current value, and trend direction.
- [ ] Trend direction is visualized with an icon or arrow: up (▲ green), down (▼ red or green depending on metric), stable (► gray).
- [ ] Metrics are displayed in a compact row or grid format.

### US-7: Configure Dashboard via JSON

**As a** project manager, **I want** to update the dashboard content by editing a single `data.json` file, **so that** I can generate new reports without modifying any code or using a complex editing interface.

**Acceptance Criteria:**
- [ ] All dashboard content is read exclusively from `data.json` at application startup.
- [ ] The JSON schema matches the documented data model (project, milestones, shipped, inProgress, carriedOver, metrics).
- [ ] Changing `data.json` and restarting the application reflects the updated data.
- [ ] A sample `data.json` with fictional but realistic project data is provided as a template.
- [ ] Invalid or missing JSON fields produce a clear error message or graceful fallback, not a crash.

### US-8: Auto-Refresh on Data Change

**As a** project manager, **I want** the dashboard to automatically refresh when I save changes to `data.json`, **so that** I can iterate on the report content without manually restarting the application.

**Acceptance Criteria:**
- [ ] A `FileSystemWatcher` monitors `data.json` for changes.
- [ ] When the file is saved, the dashboard re-reads the data and updates within 5 seconds.
- [ ] If the watcher misses an event, a fallback polling mechanism (every 5 seconds) catches the change.
- [ ] The page does not require a manual browser refresh to reflect updated data.

### US-9: Screenshot-Optimized Layout

**As a** project manager, **I want** the dashboard to render at a fixed width optimized for PowerPoint screenshots, **so that** I can capture the page and paste it into a slide without cropping, scaling, or reformatting.

**Acceptance Criteria:**
- [ ] The page renders at a fixed width of 1280px (not fluid/responsive).
- [ ] All content fits within a single viewport at 1280×900 resolution without scrolling (for the standard data volume).
- [ ] No animations, transitions, or dynamic resizing that could appear mid-frame during screen capture.
- [ ] All elements have explicit background colors (no transparent backgrounds).
- [ ] `@media print` CSS rules are included for clean browser print-to-PDF output.
- [ ] An optional `?print=true` query parameter hides unnecessary browser-oriented UI elements.

## Scope

### In Scope

- Single-page Blazor Server dashboard application targeting .NET 8.
- Horizontal milestone timeline component with status indicators and "Today" marker.
- Sections for Shipped, In Progress, and Carried Over work items with visual differentiation.
- Key metrics display with trend indicators.
- Project header with name, sponsor, reporting period, date, and color-coded status badge.
- JSON-driven data model (`data.json`) with a documented schema.
- Sample `data.json` file with fictional "Project Phoenix" data.
- `FileSystemWatcher` with polling fallback for auto-refresh on data change.
- Custom CSS optimized for 1280×900 screenshot fidelity.
- `@media print` stylesheet for clean PDF/print output.
- Light theme only, using Segoe UI system font stack for Windows/PowerPoint consistency.
- CSS custom properties for status colors (`--status-shipped`, `--status-in-progress`, `--status-carried-over`, `--status-at-risk`).
- Self-contained single-file publish capability (`dotnet publish` to `.exe`).
- Translation and improvement of the existing `OriginalDesignConcept.html` template into Blazor components.
- `DashboardDataService` singleton with dependency injection.

### Out of Scope

- **Authentication and authorization** — No login, no user roles, no access control.
- **Enterprise security** — No HTTPS enforcement, no CORS, no CSP headers, no audit logging.
- **Multi-user support** — Single user running locally; no concurrency or session management.
- **Cloud hosting or deployment** — No Azure, AWS, Docker, or reverse proxy.
- **Database** — No SQL Server, SQLite, or any persistent storage beyond `data.json`.
- **API layer** — No REST/GraphQL endpoints; components read directly from the service.
- **Dark mode** — Light theme only for PowerPoint embedding consistency.
- **Responsive/mobile layout** — Fixed 1280px width only; not designed for tablets or phones.
- **Real-time data integration** — No connections to Jira, Azure DevOps, GitHub Projects, or any external system.
- **Historical comparison or trending** — Single reporting period only (Phase 3 enhancement).
- **Edit UI for `data.json`** — Hand-edit JSON for MVP (Phase 3 enhancement).
- **Multiple simultaneous project views** — One project per `data.json` instance.
- **Internationalization or localization** — English only.
- **Accessibility (WCAG) compliance** — Best-effort only; not a primary requirement for a screenshot tool.
- **Automated testing** — Optional for MVP; recommended only for JSON parsing layer.
- **CI/CD pipeline** — Local build only.

## Non-Functional Requirements

### Performance

| Metric | Target | Rationale |
|--------|--------|-----------|
| **Page load time** | < 1 second (local) | Dashboard must feel instant when loaded from localhost. |
| **Data refresh latency** | < 5 seconds after `data.json` save | User should see updated data without manual intervention. |
| **JSON deserialization** | < 100ms for files up to 1MB | Supports large project reports without perceptible delay. |
| **Memory footprint** | < 100MB RAM | Lightweight enough to run alongside IDE and browser. |

### Reliability

| Metric | Target | Rationale |
|--------|--------|-----------|
| **Crash recovery** | Graceful error display on malformed JSON | Do not crash on invalid data; show a user-friendly error message with the parsing issue. |
| **FileSystemWatcher fallback** | Polling every 5 seconds if watcher fails | Windows `FileSystemWatcher` is known to occasionally miss events. |
| **Startup validation** | Log warnings for missing/extra JSON fields | Prevent silent data omission. |

### Security

- No authentication or authorization required.
- No encryption of `data.json` (contains non-sensitive project metadata).
- Application binds to `localhost` only by default; not network-accessible.
- If sensitive project names are a concern, `data.json` path should be configurable via `appsettings.json` to avoid placing it in `wwwroot/`.

### Usability

- All dashboard content must be legible when captured as a screenshot at 1280×900 and pasted into a 16:9 PowerPoint slide at 96 DPI.
- Status colors must be distinguishable in both color and shape/text (for black-and-white printing).
- Font size minimum: 12px for body text, 16px for section headers, 24px for project title.

### Maintainability

- Zero external NuGet dependencies for MVP (only built-in .NET 8 libraries).
- Single `.sln` with one `.csproj` — no multi-project solution.
- All data model changes are made by updating C# records and corresponding `data.json` schema.

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Screenshot-to-slide time** | < 2 minutes from `data.json` edit to pasted PowerPoint screenshot | Manual timing by project manager during weekly reporting cycle. |
| 2 | **Visual fidelity** | Screenshot at 1280×900 pastes into PowerPoint with no manual resizing or reformatting needed | Visual inspection: text readable, colors accurate, no cropping required. |
| 3 | **Data update cycle** | `data.json` change reflected in browser within 5 seconds without manual restart | Functional test: edit JSON, observe dashboard auto-refresh. |
| 4 | **Time to first working dashboard** | MVP functional within 2 development days | Development timeline tracking. |
| 5 | **Adoption** | At least 1 executive reporting cycle uses the dashboard for screenshot generation | Confirmation from project manager that the tool was used for an actual executive report. |
| 6 | **Zero-install distribution** | Published as a single `.exe` that runs without .NET SDK installed | Test: copy `.exe` to a clean machine, double-click, verify dashboard loads. |
| 7 | **JSON editability** | A non-developer project manager can update `data.json` and generate a new report without assistance | Usability test: hand the sample `data.json` and instructions to a PM, observe task completion. |

## Constraints & Assumptions

### Technical Constraints

1. **Runtime:** .NET 8 SDK (LTS) must be installed on the development machine. Published single-file executable is self-contained.
2. **Framework:** Blazor Server is the mandated web framework per the existing repository technology stack.
3. **Browser:** Dashboard is viewed in a modern Chromium-based browser (Edge or Chrome) for consistent screenshot rendering. Firefox and Safari are not targeted.
4. **Operating System:** Primary target is Windows (Segoe UI font, `FileSystemWatcher` behavior). macOS/Linux compatibility is not a requirement.
5. **No external dependencies:** MVP must use only built-in .NET 8 libraries (`System.Text.Json`, `System.IO.FileSystemWatcher`). Third-party NuGet packages (e.g., MudBlazor) are only permitted if a specific visual component cannot be reasonably implemented with pure CSS.
6. **Fixed layout:** 1280px fixed-width, non-responsive. This is a deliberate constraint for screenshot consistency, not a limitation to be "fixed" later.
7. **Design fidelity:** The existing `OriginalDesignConcept.html` from the ReportingDashboard repository is the visual baseline. The new dashboard must match its simplicity and improve upon its layout, not diverge into a different design language.

### Timeline Assumptions

1. **Phase 1 (MVP):** 1–2 development days — Core dashboard page, data model, JSON loading, CSS layout, sample data.
2. **Phase 2 (Polish):** 1 development day — Component extraction, file watching, print CSS, status color system, timeline improvements.
3. **Phase 3 (Enhancements):** Optional, future sprints — Multi-project support, edit form, historical comparison.
4. **Total MVP delivery:** 2–3 development days from project kickoff to a screenshot-ready dashboard.

### Dependency Assumptions

1. The `OriginalDesignConcept.html` file exists in the ReportingDashboard repository and is accessible to all developers working on this project. It must be reviewed before any implementation begins.
2. The `ReportingDashboardDesign.png` reference image at `C:/Pics/ReportingDashboardDesign.png` is available and represents the target visual design.
3. The .NET 8 SDK is already installed or can be installed on the developer's machine.
4. The project will be developed and run on Windows with a Chromium-based browser available for viewing and screenshots.
5. No corporate proxy, VPN, or network restriction will interfere with `localhost` binding on port 5000/5001.
6. The `data.json` schema defined in this specification is sufficient for executive reporting needs. If additional fields are required (e.g., risk register, dependency map, team roster), a schema extension will be scoped as a separate work item.
7. One `data.json` file per project is an acceptable workflow. Multi-project reporting (single page showing multiple projects) is not needed for MVP.
8. The end user (project manager) is comfortable editing JSON in a text editor or VS Code. No WYSIWYG editing interface is required for MVP.