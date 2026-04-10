# PM Specification: My Project

## Executive Summary
My Project is a lightweight, single-page Blazor Server web application designed to provide executives with real-time visibility into project health, milestones, and progress. The dashboard reads project data from a JSON configuration file, displays work items categorized by status (Shipped, In Progress, Carried Over), renders a timeline of key milestones, and enables screenshot extraction for PowerPoint presentations. No authentication, enterprise complexity, or external integrations required—built for simplicity and visual clarity.

## Business Goals

1. Provide executives with a single-page snapshot of project health at a glance (<30 second review time).
2. Enable project managers to update project data by editing a JSON file without restarting the application.
3. Support PowerPoint screenshot extraction with consistent, professional visual presentation across browsers.
4. Maintain absolute simplicity: no authentication, no enterprise security overhead, no third-party dependencies beyond essential charting.
5. Display project progress through work item categorization (Shipped, In Progress, Carried Over) and milestone timeline visualization.
6. Support auto-refresh capabilities when project data file changes, without requiring manual browser refresh.

## User Stories & Acceptance Criteria

### US-1: Executive Views Project Status Dashboard
**As an** executive sponsor  
**I want to** see project milestones, shipped/in-progress/carried-over work items, and timelines on a single page  
**So that** I can quickly assess project health and communicate status to stakeholders.

**Acceptance Criteria:**
- [ ] Dashboard displays 4 primary sections: (1) Milestone timeline at top, (2) Project summary card with metadata, (3) Status bar chart (Shipped/InProgress/CarriedOver counts), (4) Work item list grouped by status.
- [ ] Milestone timeline displays key milestones with dates in chronological order using custom SVG rendering (no third-party Gantt library).
- [ ] Status chart renders work item counts per category using ChartJS.Blazor bar or pie chart.
- [ ] Work items are grouped and labeled by status: Shipped, In Progress, Carried Over.
- [ ] Page loads in under 2 seconds on local network.
- [ ] Layout is fixed-width (1200px), light theme, no responsive mobile design.

### US-2: Dashboard Reads Data from JSON File
**As a** project manager  
**I want to** update project data by editing data.json without restarting the application  
**So that** I can maintain the dashboard without technical dependencies.

**Acceptance Criteria:**
- [ ] Dashboard reads `data.json` from application root on startup.
- [ ] JSON schema supports: `projectName`, `description`, `milestones[]` (name, date, status), `workItems[]` (title, status, assignee).
- [ ] Data model enforces 3 work item statuses: Shipped, InProgress, CarriedOver.
- [ ] FileSystemWatcher detects file changes; page re-renders within 1 second of file write.
- [ ] Missing or malformed data.json displays user-friendly error message without application crash.
- [ ] Parsing errors are logged with sufficient detail for debugging.

### US-3: Dashboard Auto-Updates on File Change
**As a** project manager  
**I want to** modify data.json and see changes reflected immediately on the dashboard  
**So that** stakeholders viewing the dashboard always see current data without manual refresh.

**Acceptance Criteria:**
- [ ] FileSystemWatcher monitors data.json with 500ms debounce to handle atomic writes.
- [ ] On file change event, Blazor component calls StateHasChanged() triggering immediate re-render.
- [ ] Re-render preserves application state; no data loss or visual flickering.
- [ ] Debouncing logic prevents duplicate parsing from multiple file write events.

### US-4: Screenshots Are Production-Ready for PowerPoint
**As an** executive  
**I want to** screenshot the dashboard and directly embed it in PowerPoint without cropping or editing  
**So that** my presentations look professional and require minimal overhead.

**Acceptance Criteria:**
- [ ] Page optimized for 1920x1080 desktop resolution; all content fits in single viewport without scrolling.
- [ ] Light theme with white/light gray background, dark text, blue/neutral accent colors.
- [ ] Typography is professional and readable: sans-serif font (Segoe UI or system default), 14-16px body text.
- [ ] No horizontal scrollbars; no overflow elements.
- [ ] Screenshot appearance is consistent across Chrome and Edge browsers (no rendering differences).

## Scope

### In Scope
- Single-page Blazor Server web application (.NET 8).
- JSON-based data loading (`data.json` in application root).
- Real-time file watching via FileSystemWatcher with 500ms debounce.
- Static HTML timeline component (SVG-based milestone visualization).
- Status bar chart (ChartJS.Blazor wrapper for Chart.js).
- Work item list grouped by status (Shipped, In Progress, Carried Over).
- Sample/fake data for fictional project demonstrating all features.
- Light theme CSS with fixed-width 1200px layout, professional typography.
- MudBlazor UI components (Card, Grid, Container) for layout scaffolding.
- Unit tests for DashboardDataService (JSON parsing, error handling, file watch logic).
- Local deployment via `dotnet run` or Visual Studio debugger.
- README.md documenting data.json schema, deployment steps, usage.

### Out of Scope
- Authentication, authorization, user roles, multi-user access control.
- Database backend (data.json is sole source of truth).
- Real-time data sync from external APIs or project management tools.
- Mobile/tablet responsive design or adaptive layouts.
- Multi-user concurrent editing of data.json with conflict resolution.
- Historical data trending, archival, or time-series comparisons.
- Cloud deployment (Azure App Service, AWS EC2, GCP).
- Automated screenshot generation (PuppeteerSharp integration).
- Multi-dashboard support with parameterized routing (`/dashboard?project=X`).
- Dark mode, theme switching, or custom color schemes.
- PDF/Excel export functionality.
- Email notifications, alerts, or webhooks.
- Health check endpoints or deployment monitoring infrastructure.
- HTTPS/TLS certificates or advanced network security.

## Non-Functional Requirements

| Requirement | Target |
|-------------|--------|
| **Page Load Time** | <2 seconds on local network (1920x1080 viewport). |
| **Chart Render Time** | <1 second (ChartJS.Blazor initialization and rendering). |
| **File Watch Latency** | Page re-renders within 1 second of data.json write completion. |
| **Concurrent Users** | Support <20 simultaneous Blazor Server sessions on typical developer hardware (2+ cores, 8GB RAM). |
| **Availability** | 24/7 uptime on local machine; no SLA required for simple internal use. |
| **Reliability** | Zero data loss (data persists in data.json); app restart reloads data automatically. |
| **Security** | No authentication required; internal deployment only. IP whitelisting optional for network-exposed instances. data.json contains non-sensitive metadata only. |
| **Scalability** | Single-machine deployment; not designed for distributed scaling. |
| **Maintainability** | Code organized by responsibility (Models, Services, Components); no external state management libraries (Redux, MediatR, Fluxor); xUnit unit tests cover critical paths. |
| **Browser Support** | Latest versions of Chrome, Edge, Firefox; no IE11 support required. |
| **Accessibility** | Not required; executive-focused internal tool. |
| **Localization** | English only. |

## Success Metrics

Project is **complete** and **successful** when:

1. ✓ Dashboard page renders without JavaScript errors; all Blazor components (MilestoneTimeline, StatusChart, WorkItemList) load and display.
2. ✓ data.json is parsed correctly on application startup; work items and milestones appear in dashboard with correct counts.
3. ✓ FileSystemWatcher correctly detects changes to data.json; page re-renders with new data within 1 second of file write.
4. ✓ Status chart displays accurate work item counts grouped by Shipped, In Progress, and Carried Over categories.
5. ✓ Milestone timeline displays all milestones in chronological order with accurate dates and status indicators.
6. ✓ Layout is fixed-width 1200px with light theme; no horizontal scrollbars on 1920x1080 resolution.
7. ✓ Screenshots captured via F12 browser tools are clean, professional, and PowerPoint-ready without cropping.
8. ✓ Unit test suite passes (xUnit tests cover DashboardDataService JSON parsing, file watch debouncing, error scenarios).
9. ✓ Sample data.json contains realistic fictional project with 5+ milestones, 10+ work items across all statuses.
10. ✓ README.md documents data.json schema, deployment steps (`dotnet run`), how to edit data, screenshot instructions.

## Constraints & Assumptions

### Technical Constraints
- **Framework**: Must use .NET 8 Blazor Server (per technology stack research).
- **UI Library**: Must use MudBlazor v7.0+ for components (free, MIT licensed, no enterprise licensing costs).
- **JSON Parsing**: Must use System.Text.Json (built-in to .NET 8; no Newtonsoft.Json dependency).
- **Data Storage**: data.json only; no relational database, no cloud storage, no external APIs.
- **Charting**: ChartJS.Blazor v4.1.2 for status chart; custom SVG for timeline (no third-party Gantt libraries).
- **File Watching**: System.IO.FileSystemWatcher (built-in); debounce logic required for atomic file writes.
- **Deployment**: Local machine only via `dotnet run` or Visual Studio; no cloud hosting.
- **Layout**: Fixed-width 1200px; no responsive mobile design.
- **Testing**: xUnit 2.6+ and Moq 4.20+ for unit tests.

### Assumptions
- **Design Assets**: OriginalDesignConcept.html and C:/Pics/ReportingDashboardDesign.png are accessible and will be reviewed before development begins to finalize layout and styling.
- **Data Update Frequency**: data.json is updated manually or by external tooling (not real-time API ingestion). Updates occur infrequently (daily or weekly, not sub-second).
- **Single Operator**: One user/operator maintains data.json; no concurrent editing conflicts or access control needed.
- **Non-Sensitive Data**: Project metadata (milestones, work items, statuses) is non-sensitive internal information; no encryption or PII protection required.
- **Local Network Only**: Dashboard deployed on local machine or LAN server; no internet exposure or HTTPS required.
- **Blazor Familiarity**: Team has basic Blazor Server knowledge or is willing to invest 2-3 days in ramp-up.
- **Development Environment**: Visual Studio 2022 (17.8+) or VS Code + OmniSharp; .NET 8 SDK installed.