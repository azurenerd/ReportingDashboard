# PM Specification: My Project

## Executive Summary

We are building a simple, single-page executive reporting dashboard that reads project milestone and progress data from a JSON configuration file and renders it in a clean, screenshot-ready format optimized for PowerPoint presentations. This lightweight Blazor Server application will enable executives to gain rapid visibility into project status without authentication, enterprise complexity, or cloud infrastructure, replacing static slide decks with a living reporting interface.

## Business Goals

1. Enable executives to gain rapid, at-a-glance visibility into project milestones, progress, and status
2. Replace static PowerPoint slide updates with a living, screenshot-ready reporting dashboard
3. Minimize IT overhead through simple, local-only deployment with zero authentication and enterprise complexity
4. Provide a reusable template for future project dashboards
5. Support rapid screenshot capture for executive presentations without manual data aggregation

## User Stories & Acceptance Criteria

**As an Executive, I want to view project status and milestones on a single page, so that I can quickly assess project health without opening multiple documents or reports.**

- [ ] Dashboard displays project name, current status, and milestone timeline on initial page load
- [ ] Page loads in <2 seconds
- [ ] Layout is readable on standard desktop monitors without horizontal scrolling
- [ ] All key information is visible above the fold

**As an Executive, I want to see progress metrics (completed, in-progress, carried-over work items) at a glance, so that I understand what was shipped, what's in flight, and what slipped.**

- [ ] Progress section displays item counts by status category
- [ ] Completion percentage is visible and accurate
- [ ] Status indicators are color-coded (e.g., green=done, yellow=in-progress, orange=carried-over)
- [ ] Metrics update when data is refreshed

**As an Executive, I want a clear timeline of project milestones with dates and status, so that I understand critical deadlines and big-rock delivery dates.**

- [ ] Timeline displays all major milestones with dates and completion status
- [ ] Milestones are sorted chronologically
- [ ] Large rocks/critical milestones are visually prioritized
- [ ] Status is indicated visually (on-track, at-risk, complete, delayed)
- [ ] Timeline spans the full project duration without gaps

**As an Administrator, I want to refresh the dashboard with updated data without restarting the server, so that I can push changes quickly without downtime.**

- [ ] Manual "Refresh" button triggers data reload
- [ ] Refresh re-reads data.json and updates all metrics
- [ ] Refresh completes in <1 second
- [ ] No server restart is required
- [ ] User receives visual confirmation of successful refresh

**As an Executive, I want to capture high-quality screenshots for PowerPoint presentations, so that I can share project status in decks without manual transcription.**

- [ ] Dashboard layout is optimized for standard 1920x1080 screenshot resolution
- [ ] All text is legible at screen capture quality
- [ ] Status indicators and charts render clearly in screenshots
- [ ] No content overflows or is cut off during capture
- [ ] Layout remains clean and professional when embedded in slides

## Scope

### In Scope
- Single-page Blazor Server web application (.NET 8)
- JSON configuration file (data.json) for project milestones and progress data
- Razor components matching OriginalDesignConcept.html design template
- Chart.js timeline visualization for milestone tracking
- Bootstrap 5.3 responsive layout
- Manual refresh button to reload data.json
- Print-friendly and screenshot-optimized CSS
- Sample data for 1-3 fictional projects
- Local-only deployment (Windows IIS or Windows Service)
- Basic file-based error logging via Windows Event Viewer

### Out of Scope
- User authentication or role-based access control
- Multi-user access control or audit logging
- Database backend (file-based JSON only; no SQL Server, relational database, or ORM)
- Cloud hosting or cloud services (Azure, AWS, etc.)
- Historical trend analysis, snapshots, or time-series data
- Auto-refresh on file change (FileSystemWatcher polling deferred to Phase 2)
- Export to Excel, PDF, or PowerPoint XML (manual screenshot only)
- Mobile-responsive design (desktop/tablet only)
- Burndown charts, velocity metrics, or advanced data visualizations
- Windows Authentication or corporate directory integration
- Real-time collaboration or multi-user editing
- Automated CI/CD pipeline integration for data updates

## Non-Functional Requirements

**Performance**
- Initial page load time: <2 seconds
- Data refresh time: <1 second
- Chart.js rendering: <500ms for up to 50 milestones

**Availability & Reliability**
- Target uptime: 99% (internal tool)
- Single Blazor Server instance supports ~100 concurrent users
- Graceful handling of network disconnects with reconnection logic

**Security**
- No user authentication or authorization required
- No sensitive data stored or transmitted
- data.json permissions restricted to application identity user (optional)
- No encryption required; project metadata only
- Deployable only on corporate/internal networks (no public internet exposure)

**Scalability**
- Single-instance deployment; no load balancing required
- JSON file I/O must not block UI updates
- Support dashboard updates for 1-10 concurrent projects

**Usability**
- Zero training required; intuitive at-a-glance layout
- All status information must be understandable within 5 seconds of page load
- Color contrast meets WCAG AA accessibility standards

**Maintainability**
- Modular Razor components with reusable progress cards and timeline elements
- Single source of truth: data.json (no duplicate data)
- Minimal external NuGet dependencies (prefer .NET 8 built-ins)

## Success Metrics

- [ ] Dashboard loads successfully on first page request with zero console errors
- [ ] All milestones from data.json render correctly on the timeline visualization
- [ ] Progress metrics (completed, in-progress, carried-over counts) match data.json source values
- [ ] Manual refresh button successfully reloads data.json and updates UI within 1 second
- [ ] Screenshots captured at 1920x1080 resolution are clear, legible, and suitable for PowerPoint embedding
- [ ] Administrator can update data.json and refresh without code changes or server restart
- [ ] Layout matches OriginalDesignConcept.html design template in structure and visual hierarchy
- [ ] Executive stakeholder confirms dashboard is simpler and more actionable than static slide decks
- [ ] Dashboard runs on Windows Server with IIS/.NET 8 runtime with zero deployment errors

## Constraints & Assumptions

**Technical Constraints**
- Local-only deployment; no internet or cloud services
- Windows Server / IIS hosting environment required
- No external database; file-based JSON only
- No sensitive data; project metadata only
- Minimal external dependencies; prioritize .NET 8 built-in libraries

**Timeline Assumptions**
- 3-week implementation timeline (1 week scaffold/components, 1 week integration/testing, 1 week optimization/deployment)
- OriginalDesignConcept.html is structurally sound and replicable in Razor components
- Team has C# / .NET 8 development experience

**Data & Usage Assumptions**
- Executives will manually request dashboard refreshes (no auto-polling in MVP)
- JSON file updates are infrequent and non-concurrent
- Screenshot capture is manual (no automated export service)
- Dashboard displays single project at a time (multi-project support deferred to Phase 2)
- No historical archiving or trend tracking required for MVP

**Deployment Assumptions**
- .NET 8 runtime is pre-installed on target Windows Server
- IIS is available and configured for ASP.NET Core hosting
- Administrator has file system access to deploy and update data.json
- No corporate proxy or firewall restrictions prevent Blazor WebSocket communication