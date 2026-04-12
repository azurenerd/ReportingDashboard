# PM Specification: My Project

## Executive Summary
My Project is a lightweight, single-page executive reporting dashboard designed to provide real-time visibility into project milestones, progress, and work item status. Built on C# .NET 8 with Blazor Server, the dashboard reads project data from a JSON configuration file and presents a clean, printable visualization suitable for PowerPoint presentations and executive briefings. The solution prioritizes simplicity, visual clarity, and ease of use—enabling project managers to communicate project health, shipped features, in-progress work, and carried-over items at a glance.

## Business Goals
1. Provide executives with a single, authoritative view of project status and milestones in under 30 seconds of page load
2. Enable project managers to generate professional, screenshot-ready reports for executive presentations without manual PowerPoint design work
3. Deliver a working prototype within 2 weeks that is immediately usable for reporting and requires minimal ongoing maintenance
4. Create a lightweight, self-contained dashboard with no external dependencies, cloud services, or multi-tier infrastructure
5. Establish a simple data model (JSON) that can be manually maintained or integrated with future project tracking systems (JIRA, Azure DevOps) as needed

## User Stories & Acceptance Criteria

### Story 1: Display Project Header with Status
**As an executive**, I want to see the project name, current status, owner, and planned dates at the top of the dashboard, **so that** I immediately understand the project context and current health.

**Acceptance Criteria:**
- [ ] Project name is displayed prominently at the top of the page
- [ ] Project status ("On Track", "At Risk", "Off Track") is shown with a visual indicator (color-coded or icon)
- [ ] Project owner name is displayed
- [ ] Start date and target end date are shown in a clear, readable format (e.g., "Jan 1, 2026 – Jun 30, 2026")
- [ ] Header layout matches the OriginalDesignConcept.html design and references ReportingDashboardDesign.png visual style
- [ ] The header is readable when printed or screenshotted for PowerPoint

### Story 2: Display Project Milestones Timeline
**As a project manager**, I want to see all major milestones laid out horizontally in a timeline, **so that** executives can see the project roadmap and understand which milestones are completed versus upcoming.

**Acceptance Criteria:**
- [ ] Milestones are displayed in a horizontal timeline, ordered chronologically
- [ ] Each milestone shows the title and due date
- [ ] Completed milestones are visually distinguished from upcoming milestones (e.g., checkmark, filled circle, or grayed-out appearance)
- [ ] Timeline is scrollable if it exceeds the page width, or fits on a single page for typical 5–10 milestones
- [ ] Timeline section is screenshot-friendly and prints cleanly
- [ ] Milestone data is read from `data.json` and matches the Milestone data model (id, title, dueDate, isCompleted, order)

### Story 3: Display Work Items by Category (Shipped, In Progress, Carried Over)
**As an executive**, I want to see work items organized into three columns—Shipped, In Progress, and Carried Over—**so that** I can quickly assess what was delivered, what is actively being worked on, and what slipped from the previous period.

**Acceptance Criteria:**
- [ ] Three columns are displayed side-by-side: "Shipped", "In Progress", and "Carried Over"
- [ ] Each column displays work items as cards with title, story points, and status
- [ ] Work items are grouped by the `category` field in `data.json` (Shipped, InProgress, CarriedOver)
- [ ] Each card shows story points as a number (e.g., "8 pts")
- [ ] Card titles are concise and do not wrap excessively
- [ ] A count of items in each column is displayed (e.g., "Shipped (7)")
- [ ] Layout is responsive and prints cleanly for typical work item counts (10–30 items per category)
- [ ] Visual styling matches the ReportingDashboardDesign.png design reference

### Story 4: Display Key Metrics Panel (KPIs)
**As an executive**, I want to see a summary of key metrics—percentage complete, total story points, shipped count, in-progress count, and velocity—**so that** I can gauge overall project health and progress at a glance.

**Acceptance Criteria:**
- [ ] Metrics panel displays at least the following KPIs:
  - Percentage of work completed (completedStoryPoints / totalStoryPoints × 100)
  - Total story points (from metrics.totalStoryPoints)
  - Number of items shipped (count of items in Shipped category)
  - Number of items in progress (count of items in InProgress category)
  - Number of items carried over (metrics.carriedOverCount or count of CarriedOver category)
  - Velocity per sprint (from metrics.velocityPerSprint)
- [ ] Each metric is displayed as a card or panel with a label and value
- [ ] Metrics are computed from `data.json` on page load
- [ ] Panel is positioned prominently (e.g., below the header, above work items)
- [ ] Panel is printable and suitable for PowerPoint screenshots

### Story 5: Load Data from JSON Configuration File
**As a developer**, I want the dashboard to load all project data from a `data.json` file stored in the `wwwroot/data/` directory, **so that** project managers can update the dashboard by simply editing the JSON file without code changes.

**Acceptance Criteria:**
- [ ] Dashboard reads `data.json` from `wwwroot/data/data.json` on page initialization
- [ ] Deserialization uses `System.Text.Json` (no external dependencies)
- [ ] `data.json` schema matches the defined data model (ProjectData, ProjectMetadata, Milestone, WorkItem, ProjectMetrics)
- [ ] The example `data.json` file contains a sample project with 5 milestones and 20+ work items across the three categories
- [ ] If `data.json` is missing or malformed, the page displays a graceful error message (e.g., "Unable to load project data. Please check data.json.")
- [ ] Data is loaded asynchronously and does not block page rendering

### Story 6: Provide a Screenshot-Ready, Printable Layout
**As a project manager**, I want the dashboard layout to be optimized for taking screenshots and printing to PDF/PowerPoint, **so that** I can quickly insert professional-looking reports into executive presentations.

**Acceptance Criteria:**
- [ ] The layout uses CSS Grid and Flexbox for clean, aligned spacing
- [ ] Default print styles (@media print) remove unnecessary UI elements (e.g., margins, buttons)
- [ ] Content is optimized for a standard letter-size page (8.5" × 11") or widescreen monitor (16:9)
- [ ] When printed or screenshotted, all content fits within a single page or spans logically across pages
- [ ] Font sizes and colors are chosen for readability and professional appearance
- [ ] Timeline does not break or overflow unexpectedly when printed
- [ ] Work item cards remain visible and readable in both screen and print views
- [ ] Tested in Chrome, Firefox, and Edge browsers

### Story 7: Display Footer with Last Updated Date
**As an executive**, I want to see when the dashboard data was last updated, **so that** I can verify the freshness of the information presented.

**Acceptance Criteria:**
- [ ] Footer is displayed at the bottom of the page
- [ ] Last updated date/time is shown (either the current date or a date/time embedded in `data.json`)
- [ ] Format is clear and human-readable (e.g., "Last updated: April 12, 2026 at 01:19 UTC")
- [ ] Footer is optional in print layout (can be hidden via @media print)

### Story 8: Provide Sample data.json with Mock Project Data
**As a developer**, I want the project to include a working example `data.json` file with realistic fictional project data, **so that** the dashboard can be demonstrated immediately without requiring external data source setup.

**Acceptance Criteria:**
- [ ] `data.json` is stored in `wwwroot/data/data.json`
- [ ] Contains a complete, valid example with:
  - Project metadata (name, status, owner, dates)
  - 5 milestones (mix of completed and upcoming)
  - 20–30 work items (distributed across Shipped, InProgress, CarriedOver categories)
  - Metrics (totalStoryPoints, completedStoryPoints, inProgressStoryPoints, carriedOverCount, velocityPerSprint)
- [ ] All data is realistic and suitable for a fictional but believable project
- [ ] JSON is valid and matches the required schema
- [ ] File can be edited manually by project managers to test updates

## Scope

### In Scope
- Single-page Blazor Server application (no multi-page routing)
- Read-only dashboard display (no editing or persistence of changes)
- Data loading from local JSON file (`data.json`)
- Display of project metadata (name, status, owner, dates)
- Horizontal milestone timeline with completion status
- Work items organized into three categories (Shipped, In Progress, Carried Over)
- Key metrics panel (KPIs: % complete, story points, counts, velocity)
- CSS-based print and screenshot optimization
- Error handling for missing or malformed `data.json`
- Footer with last updated timestamp
- Sample `data.json` file with fictional project data
- Responsive layout using CSS Grid and Flexbox
- No authentication or authorization
- No real-time updates or WebSocket polling
- Basic unit tests for DataService (JSON deserialization)
- Manual browser testing and screenshot validation

### Out of Scope
- User authentication or role-based access control
- Multi-project dashboard (single project per deployment)
- Data editing or write-back functionality
- Integration with external systems (JIRA, Azure DevOps, Salesforce, etc.)—*may be added post-MVP*
- Real-time data updates or automatic refresh
- Dark mode or themeable color schemes (use default light theme)
- Mobile app or native application
- Database storage (JSON is the single source of truth)
- Advanced charting or visualizations (burndown charts, velocity trends)—*may be added post-MVP*
- Filtering, sorting, or search within the dashboard
- Multi-language localization
- Accessibility audit (WCAG compliance)—*nice-to-have, not required*
- Cloud deployment or CI/CD pipeline setup
- Historical data trending or snapshots
- Notifications or alerts

## Non-Functional Requirements

### Performance
- Page must load and render within 3 seconds on standard hardware (no artificial delays)
- JSON deserialization must complete in <500ms for files <5MB
- Dashboard must handle up to 500 work items without perceptible lag
- No external API calls or network dependencies (all data is local)

### Reliability & Availability
- Dashboard must remain available as long as the Blazor Server process is running (no single point of failure within the application layer)
- Graceful error handling if `data.json` is missing, corrupted, or invalid (display user-friendly error message, do not crash)
- Data load failure must not crash the application; fallback to empty or sample state

### Usability
- Page must render correctly in Chrome, Firefox, Edge (latest two versions)
- Print layout must work correctly in all major browsers
- All text must be legible without zooming at 100% browser zoom
- Screenshots must capture the full dashboard in a single image or easily stitchable set of images

### Security
- No authentication or user session required
- No personal or sensitive data is stored; use mock/fictional data only
- JSON file is served as static content; no server-side data encryption required for non-sensitive mock data
- HTTPS is not required for local deployments (HTTP is acceptable)
- No external service calls or third-party API authentication

### Maintainability
- Code is written in C# following Microsoft naming conventions
- Components are modular and reusable (Timeline, StatusCard, MetricsPanel, etc.)
- `DataService` is injectable for testability
- All configuration is externalized to `data.json` (no hardcoded values)
- Minimal external dependencies (prefer built-in .NET 8 libraries)

## Success Metrics

1. **Time to First Screenshot**: Ability to generate a professional, printable screenshot of the dashboard within 5 minutes of application startup (project manager metric)
2. **Data Update Cycle**: Project manager can update `data.json` and refresh the page in <2 minutes without code changes (operability metric)
3. **Visual Parity with Design**: Dashboard layout matches or exceeds the ReportingDashboardDesign.png design reference as judged by project stakeholder review (quality metric)
4. **Page Load Time**: Dashboard page loads and renders in <3 seconds on typical hardware (performance metric)
5. **Test Coverage**: DataService and CalculationService have >80% unit test coverage (code quality metric)
6. **Manual Acceptance Testing**: All 8 user stories pass acceptance criteria testing in at least 2 of the 3 major browsers (Chrome, Firefox, Edge)
7. **Print/Screenshot Quality**: Dashboard prints to PDF and appears in PowerPoint screenshots without layout breakage or content overflow (usability metric)
8. **Error Resilience**: Application handles missing `data.json`, malformed JSON, and partial data gracefully without crashing (reliability metric)

## Constraints & Assumptions

### Technical Constraints
- **Technology Stack**: Must use C# .NET 8 with Blazor Server (non-negotiable per project requirements)
- **Data Source**: Must read from local JSON file; no database or external API integration required for MVP
- **Deployment**: Local Windows/Linux/macOS environment only; no cloud services (AWS, Azure, GCP)
- **No External Authentication**: Cannot require Entra ID, OAuth, or any authentication provider
- **Browser Support**: Must work in Chrome, Firefox, and Edge (latest versions); no legacy browser support required
- **File Storage**: `data.json` must be stored in `wwwroot/data/` directory (part of Blazor Server static files)
- **No JavaScript Frameworks**: Minimal JavaScript; leverage Blazor Server for rendering (no React, Vue, Angular)

### Assumptions
- **Data Stability**: `data.json` structure will remain stable for at least 3 months post-launch; significant schema changes will be handled as a future enhancement
- **File Size**: `data.json` will remain <5MB (no performance concerns for larger files)
- **Project Count**: Initial deployment supports a single project per dashboard instance; multi-project support is post-MVP
- **User Count**: Expected usage is 5–10 internal project managers and executives viewing the dashboard asynchronously; no concurrent session limits required
- **Offline Requirement**: Dashboard does not need to function offline; always-online assumption for Blazor Server connection
- **Data Freshness**: Data updates are manual and asynchronous (no real-time sync required); project manager refreshes dashboard when data is updated
- **HTML Design Reference**: OriginalDesignConcept.html and ReportingDashboardDesign.png are available and will be reviewed before implementation begins
- **Timeline Scope**: Project timeline is assumed to be 3–12 months; no multi-year roadmap complexity
- **Work Item Volume**: Dashboard will display 20–50 work items initially; scaling to 500+ items is a post-MVP optimization

### Timeline Assumptions
- **MVP Delivery**: 2 weeks (10 business days)
- **Phase 1 (Days 1–5)**: Data model, DataService, basic dashboard layout, sample data
- **Phase 2 (Days 6–10)**: Timeline component, metrics panel, CSS refinement, print testing
- **Post-MVP**: Chart integration, filtering, dark mode, and external data source integration as future enhancements

### Dependency Assumptions
- **Development Environment**: Visual Studio 2022 or .NET CLI 8.0 is available
- **Design Files Access**: OriginalDesignConcept.html and ReportingDashboardDesign.png (C:/Pics/) are accessible to the development team before implementation
- **No External API Dependencies**: No JIRA, Azure DevOps, or other project tracking system integration required for MVP (data is purely JSON-based)
- **No Database**: SQLite database file exists in the project but is not required for MVP; JSON is the primary data source

### Business Assumptions
- **Executive Buy-In**: Executives have approved the MVP feature set and will not request major scope additions during development
- **Data Source Management**: A project manager or team lead will manually maintain `data.json` or integrate a data pipeline post-MVP
- **Screenshot Usage**: Dashboard is intended for quarterly or monthly executive reporting; not a real-time monitoring tool
- **Licensing**: All dependencies (Blazor Server, .NET 8, xUnit, Moq, Chart.js) use permissive open-source licenses compatible with the project's usage