# PM Specification: My Project

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, progress, and work item status using a simple, screenshot-ready design optimized for PowerPoint presentations. The dashboard reads from a JSON configuration file (data.json), displays key metrics, milestone timelines, and work item status at a glance, enabling executives to quickly assess project health without authentication, complex UI, or enterprise security overhead. This tool will replace manual slide creation and provide a consistent, real-time visual source of truth for project status updates.

## Business Goals

1. **Reduce status report creation time** by 80% through automated dashboard generation (from 2 hours of manual PowerPoint creation to ~15 minutes of screenshot capture).
2. **Improve executive visibility** into project health with a standardized dashboard showing milestones, progress, shipped items, and carried-over work.
3. **Enable quick iteration** of visual design by keeping the application simple enough to screenshot and embed directly into executive decks without redaction.
4. **Establish a single source of truth** for project data by centralizing metrics from data.json into one lightweight dashboard, eliminating conflicting status reports.
5. **Minimize operational overhead** by deploying locally with no cloud dependencies, no authentication infrastructure, and no enterprise security burden.

## User Stories & Acceptance Criteria

### **Story 1: Executive views project status at a glance**
**As an** executive,  
**I want** to see a single-page dashboard with project name, date range, and key metrics cards,  
**so that** I can quickly assess project health in <30 seconds without drilling into details.

**Acceptance Criteria:**
- [ ] Dashboard header displays project name, start date, and target end date
- [ ] Key metrics cards display: (a) % complete, (b) number of shipped items, (c) number of carried-over items
- [ ] Page loads in <2 seconds on typical corporate network
- [ ] Design matches the simplicity and aesthetic of OriginalDesignConcept.html
- [ ] All metrics are readable in a 1920x1080 screenshot suitable for PowerPoint embedding

### **Story 2: Executive views milestone timeline**
**As an** executive,  
**I want** to see a visual timeline of project milestones across the top of the dashboard with status indicators,  
**so that** I can understand the project schedule and identify at-risk milestones at a glance.

**Acceptance Criteria:**
- [ ] Timeline displays 5–10 major milestones horizontally across the top section
- [ ] Each milestone shows: name, scheduled date, and completion percentage (or status: Planned/In Progress/Completed/Delayed)
- [ ] Milestones are color-coded by status (green=completed, yellow=in progress, red=delayed, gray=planned)
- [ ] Timeline adapts to fit page width without horizontal scrolling
- [ ] Design incorporates Chart.js or similar lightweight charting for clean visual presentation

### **Story 3: Executive views work item breakdown by status**
**As an** executive,  
**I want** to see a table or card layout listing work items grouped or filtered by status (New, In Progress, Shipped, Carried Over),  
**so that** I can understand what work is complete, active, and deferred.

**Acceptance Criteria:**
- [ ] Dashboard displays work items organized by status (can be separate tables, cards, or filtered view)
- [ ] Each work item row shows: title, status badge, owner name (if applicable), and date (created/completed/planned)
- [ ] Status badges use Bootstrap color classes (success/warning/danger) for visual clarity
- [ ] At least 10–15 work items are visible without excessive scrolling (use pagination if >30 items)
- [ ] Work items are read-only (no edit capability in MVP)

### **Story 4: Dashboard reads data from JSON configuration**
**As a** developer,  
**I want** the dashboard to load project, milestone, and work item data from a data.json file,  
**so that** stakeholders can update project status by editing JSON instead of relying on code changes.

**Acceptance Criteria:**
- [ ] data.json file located in wwwroot/ is loaded on application startup
- [ ] JSON schema includes objects for Project, Milestone, and WorkItem with required fields (name, dates, status, etc.)
- [ ] If data.json is malformed, dashboard displays a clear error message and logs to console
- [ ] Sample data.json includes a fictional project with 5+ milestones and 15+ work items
- [ ] Data is cached in memory after load; refresh dashboard to reload updated JSON

### **Story 5: Executive takes screenshot for PowerPoint without redaction**
**As an** executive,  
**I want** to take a clean screenshot of the dashboard and paste it directly into a PowerPoint presentation,  
**so that** I don't need to redact, resize, or manually format the image.

**Acceptance Criteria:**
- [ ] Dashboard fits in a standard 1920x1080 viewport (16:9 aspect ratio)
- [ ] No sensitive credentials, API keys, or internal URLs are visible
- [ ] Color scheme is professional and accessible (WCAG AA contrast minimum)
- [ ] No floating overlays, modals, or banners obscure the main content
- [ ] Font sizes are readable at typical presentation viewing distance (14pt minimum for body text)
- [ ] Optional: "Export to PNG" button uses html2canvas to generate a downloadable screenshot

### **Story 6: Dashboard persists data to local database**
**As a** developer,  
**I want** work item and milestone data to be persisted in a local SQLite database,  
**so that** the dashboard can query and filter data efficiently without reloading JSON on every request.

**Acceptance Criteria:**
- [ ] ApplicationDbContext (EF Core) manages Project, Milestone, and WorkItem entities
- [ ] SQLite database is seeded from data.json on application startup (first run)
- [ ] ReportingService queries SQLite to compute metrics (% complete, shipped count, carried-over count)
- [ ] Data access uses Entity Framework Core with explicit .Include() to avoid N+1 queries
- [ ] Database file is stored in a local directory (e.g., App_Data/dashboard.db)

### **Story 7: Dashboard supports basic filtering by status and milestone**
**As an** executive,  
**I want** to filter work items by status or milestone,  
**so that** I can focus on specific areas of interest (e.g., "show me only carried-over items").

**Acceptance Criteria:**
- [ ] Dashboard includes simple dropdown or button filters for: All, In Progress, Shipped, Carried Over
- [ ] Clicking a filter updates the work item list without page refresh
- [ ] Milestone filter allows selection of a single milestone to show related work items
- [ ] Filter state persists in URL query string (optional, for shareable links)
- [ ] No API calls required; filtering happens on the Blazor Server component

### **Story 8: Dashboard is styled for screenshot clarity**
**As a** designer,  
**I want** the dashboard to use clean typography, appropriate whitespace, and color-coded status indicators,  
**so that** screenshots are visually appealing and suitable for executive presentations.

**Acceptance Criteria:**
- [ ] Dashboard uses Bootstrap 5.3 grid layout for responsive design
- [ ] Color palette includes: success (green), warning (yellow), danger (red), info (blue), and neutral (gray)
- [ ] Status badges and progress bars use Bootstrap utility classes for consistent styling
- [ ] Metrics cards use clear hierarchy: large metric number, small label below
- [ ] Optional dark mode toggle is available via CSS (no JavaScript required)
- [ ] All text is left-aligned and left-to-right (LTR) for US/English presentations

## Scope

### In Scope

- Single-page Blazor Server application displaying one project's status at a time
- Dashboard header with project name, date range, and key metrics cards (% complete, shipped count, carried-over count)
- Horizontal milestone timeline with status color-coding and completion percentage
- Work item table/cards organized by status (New, In Progress, Shipped, Carried Over)
- Load project data from data.json file on application startup
- Seed SQLite database from data.json (first run)
- ReportingService to aggregate executive metrics from SQLite
- Bootstrap 5.3 layout for responsive, professional presentation
- Chart.js integration for milestone timeline visualization
- Read-only work item display (no edit, create, or delete capability)
- Basic filtering by status and milestone
- Simple CSS styling for screenshot clarity (no dark mode toggle in MVP, defer to Phase 3)
- Sample fictional project data (data.json) with 5+ milestones and 15+ work items
- Deployment to Windows or Linux using `dotnet publish`
- No authentication, authorization, or user roles (local, trusted environment)

### Out of Scope

- Multi-project support or project selector (show one project only; multi-project is post-MVP)
- Historical data, trend analysis, or burndown charts over time
- Edit, create, or delete capabilities for work items or milestones
- User authentication, login, or role-based access control
- API endpoints or integrations with external systems (e.g., Azure DevOps, Jira)
- Mobile-responsive design optimizations (Bootstrap responsive grid is sufficient)
- Cloud deployment or hosted infrastructure (local deployment only)
- Real-time notifications or WebSocket updates
- Audit logging or change history tracking
- Export to PDF, Excel, or other formats (PNG screenshot via html2canvas is optional, post-MVP)
- Performance tuning for >10K work items or >1000 milestones (scale up post-MVP if needed)
- Dark mode toggle (defer to Phase 3)
- Accessibility (WCAG) compliance beyond basic color contrast (defer to Phase 3)
- Unit tests, integration tests, or CI/CD pipeline (add post-MVP)
- Documentation beyond inline code comments

## Non-Functional Requirements

### Performance
- Dashboard page must load in <2 seconds on a typical corporate network (5 Mbps latency, <100ms).
- Chart.js milestone timeline must render in <500ms for up to 50 milestones.
- SQLite queries must execute in <100ms for up to 10K work items (with indexed columns).
- Blazor Server component re-render must complete in <300ms after filter change.
- No memory leaks; Blazor component memory footprint must remain <50 MB for typical data size.

### Reliability
- Dashboard must remain available during application lifetime (no scheduled downtime required).
- SQLite database file must be readable and writable from the application directory without elevation.
- If data.json is corrupted, dashboard must display a user-friendly error message (not a crash).
- Milestones and work items must be accurate within 1 minute of data.json or SQLite update.

### Scalability
- Dashboard must support up to 100K work items in SQLite without significant performance degradation (>2s load time).
- Milestone timeline can display up to 50 milestones; beyond that, implement aggregation or drill-down (post-MVP).
- Blazor Server session memory must remain <100 MB per concurrent user; scale to 100s of concurrent users via vertical scaling.

### Usability
- Dashboard layout must be discoverable in <30 seconds (no training required).
- All interactive elements (filters, milestones) must respond to clicks within <200ms.
- Color choices must meet WCAG AA contrast requirements (4.5:1 for small text, 3:1 for large text) to ensure readability in screenshots.
- Font sizes must be ≥14pt for body text, ≥18pt for headers, readable in 1920x1080 presentation view.

### Security
- No credentials, API keys, API URLs, or sensitive project names will be visible in dashboard UI.
- data.json file must be stored in version control with only non-sensitive data (project names, dates, public milestone names).
- SQLite database file must be located in a protected directory (not world-readable) if sensitive financial data is stored.
- Application must run with least privilege (no elevated permissions required).
- All external JavaScript libraries (Chart.js, Bootstrap, Font Awesome) must be sourced from CDN with integrity checks (SRI hashes).

### Maintainability
- Codebase must be organized following Clean Architecture principles (Models, Services, Data, Components).
- All services must be registered in Blazor dependency injection container for testability.
- No hardcoded configuration; all settings must be in appsettings.json or environment variables.
- Code comments are minimal (only clarify non-obvious logic); intention is clear from naming.

## Success Metrics

1. **Deployment Success:** Application builds and runs with `dotnet run` and `dotnet publish` without errors. ✓
2. **Dashboard Load Time:** Homepage loads in <2 seconds on corporate network. ✓
3. **Data Completeness:** All project data from data.json is displayed accurately (100% of milestones and work items visible without errors). ✓
4. **Screenshot Quality:** Single screenshot fits in 1920x1080 viewport, is readable, and is suitable for embedding in PowerPoint without redaction. ✓
5. **Metrics Accuracy:** Key metrics cards (% complete, shipped count, carried-over count) match manual calculation of data.json. ✓
6. **Filtering:** Status and milestone filters work correctly without page refresh; filtered results are accurate. ✓
7. **No Security Issues:** No credentials, API keys, or sensitive URLs visible in UI, HTML, or browser console. ✓
8. **Stakeholder Sign-Off:** Executive stakeholder confirms dashboard design and data presentation meet their expectations for PowerPoint use. ✓

## Constraints & Assumptions

### Technical Constraints
- **Technology Stack:** Must use C# .NET 8 with Blazor Server (no React, Vue, or other JavaScript frameworks).
- **Database:** Must use SQLite (no cloud databases or SQL Server required; local file-based only).
- **Frontend Framework:** Must use Bootstrap 5.3 for layout; Chart.js 4.4+ for milestone visualization (no Syncfusion or commercial charting libraries).
- **Deployment:** Local deployment to Windows or Linux with .NET 8 runtime; no Docker, Kubernetes, or cloud infrastructure.
- **No Authentication:** No user login, roles, or permissions; local trusted environment assumed.

### Timeline & Resource Assumptions
- **Development Timeline:** 3 weeks (Phase 1: MVP in 1 week, Phase 2: Data persistence in 1 week, Phase 3: Polish in 1 week).
- **Team:** 1 full-time developer with .NET/Blazor experience; no dedicated designer required (follow OriginalDesignConcept.html aesthetic).
- **Stakeholder Availability:** Executive stakeholder available for sign-off and feedback by end of Phase 1 (MVP).
- **Design Template:** OriginalDesignConcept.html and ReportingDashboardDesign.png files are available in ReportingDashboard repo and C:/Pics/ directory.

### Data & Project Assumptions
- **data.json Format:** Single JSON file contains all project, milestone, and work item data; no multi-file configuration.
- **Data Frequency:** Project data is updated manually (by stakeholder edit of data.json); no real-time sync required from external systems.
- **Sample Data:** Fictional project with 5+ milestones, 15+ work items, and sample statuses is provided in data.json (not real company data).
- **Work Item Status Values:** Exactly four status values are supported: New, In Progress, Shipped, Carried Over. No custom statuses.
- **Milestone Status Values:** Planned, In Progress, Completed, Delayed. No custom milestone statuses.
- **Single Project:** Dashboard displays one project at a time; no multi-project rollup required (post-MVP).

### Operational Assumptions
- **Local Network Deployment:** Dashboard is deployed on a local machine or intranet server; no internet access required.
- **User Trust:** Users accessing the dashboard are trusted executives; no encryption or access control required.
- **Data Backup:** Data is backed up via external script or manual file copy; no built-in backup required (post-MVP).
- **Maintenance:** Application is maintained by internal team; no SLA or uptime guarantee required.

### Design & UX Assumptions
- **Simplicity First:** Dashboard prioritizes visual clarity over interactive features; fewer clicks, more immediate insight.
- **Screenshot-Ready:** All design decisions optimize for static screenshot capture; no animations, floating elements, or scrollable modals.
- **Executive Audience:** Content is tailored for C-level executives with limited time; no drill-down or detailed reports required (focus on summary metrics).
- **PowerPoint Format:** Design is optimized for 16:9 aspect ratio (1920x1080) to match standard presentation displays.