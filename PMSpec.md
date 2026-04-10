# PM Specification: My Project

## Executive Summary

A single-page executive reporting dashboard built with Blazor Server and Bootstrap 5 that displays project milestones, task progress, and status. Reads data from JSON configuration file. Designed for screenshot export to PowerPoint decks; no authentication or enterprise security required.

## Business Goals

1. Provide executives real-time visibility into project health (milestones, shipped, in-progress, carried-over work)
2. Enable screenshot-ready reporting for PowerPoint presentations
3. Simplify manual status updates with JSON-based data source (no database)
4. Deliver MVP in 1 week using existing C# .NET 8 skills

## User Stories & Acceptance Criteria

**As an Executive**, I want to view project milestones on a timeline at the top of the page, so that I can see critical project dates and progress.
- [ ] Timeline displays 5+ milestones with target dates
- [ ] Color-coded status (on-track, at-risk, completed)
- [ ] Responsive layout supports desktop and tablet views

**As a Project Manager**, I want to see tasks organized by status (Completed, In Progress, Carried Over), so that I can report work distribution accurately.
- [ ] Three-column task board displays all categories
- [ ] Task count visible per section
- [ ] Clean card design suitable for screenshots

**As a Dashboard Maintainer**, I want to update project data via data.json, so that I can reflect current status without code changes.
- [ ] data.json loads without errors on startup
- [ ] Malformed JSON shows error message (not crash)
- [ ] Changes reflect on page refresh

**As an Executive**, I want KPI summary cards (on-time %, shipped count, in-progress count), so that I understand project health at a glance.
- [ ] Summary cards appear above task board
- [ ] Percentages calculated from task data
- [ ] Cards use color indicators (green/yellow/red)

## Scope

### In Scope
- Single-page Blazor Server dashboard
- Data loaded from wwwroot/data/data.json
- Bootstrap 5 responsive UI
- Chart.js milestone timeline (JSInterop)
- Three task status columns (Completed, In Progress, Carried Over)
- KPI cards (on-time %, shipped, in-progress)
- Fake project data (5 milestones, 12+ tasks)
- Local Kestrel deployment
- Print-to-PDF screenshot workflow

### Out of Scope
- Authentication/authorization
- Database or persistent storage (file-only)
- Admin UI for editing data.json
- Multi-project support
- Historical archival
- Email notifications
- Mobile app
- HTTPS/TLS
- User account management

## Non-Functional Requirements

- **Performance:** Page loads in <2 seconds on local network
- **Uptime:** 99% availability (local server)
- **Scalability:** Single project, max 20 tasks, 10 milestones
- **Security:** None (trusted internal environment)
- **Compatibility:** Chrome, Edge, Firefox (desktop)
- **Deployment:** .NET 8 Runtime required; Kestrel HTTP only

## Success Metrics

- Dashboard renders without errors
- All user stories accepted and demo-ready
- Screenshots export cleanly to PDF (no broken layouts)
- Data loads from data.json in <1 second
- Milestone timeline interactive (Chart.js renders)
- Task board reflects all statuses clearly
- KPI calculations accurate

## Constraints & Assumptions

**Technical Constraints:**
- JSON file-based data source (no database)
- No external npm/build pipeline
- Razor Server components only (no SPA framework)
- Chart.js loaded via CDN

**Assumptions:**
- Executives access dashboard on desktop/laptop only
- Data updated manually before screenshots taken
- .NET 8 Runtime available on deployment server
- Bootstrap 5 CSS sufficient (no custom styling)
- No concurrent user load (single-user focus)
- Internet access available for CDN libraries (Chart.js, Bootstrap)

**Dependencies:**
- Original DesignConcept.html template as design reference
- ReportingDashboardDesign.png mockup approval