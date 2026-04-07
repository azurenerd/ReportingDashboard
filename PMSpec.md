# PM Specification: My Project

## Executive Summary

Build a single-page executive dashboard using Blazor Server that displays real-time project health, milestones, and work item progress by reading from a JSON configuration file. The dashboard provides executives visibility into shipped items, in-progress work, carried-over tasks, and a timeline of major project milestones—replacing manual status report compilation and enabling data-driven decision-making.

## Business Goals

1. Provide executives with real-time, single-page visibility into project health (milestones, progress, blockers)
2. Enable data-driven decision-making on project status without manual compilation
3. Reduce time spent gathering and formatting status updates by 80%
4. Deploy and achieve adoption within executive stakeholder group within 6 weeks
5. Create a reusable dashboard pattern for future project reporting

## User Stories & Acceptance Criteria

### User Story 1: View Project Overview
**As an** executive sponsor  
**I want to** see high-level project status at a glance  
**So that** I can quickly assess project health in under 60 seconds

**Acceptance Criteria:**
- [ ] Dashboard loads in <2 seconds on local network
- [ ] Three status cards display (Shipped / In-Progress / Carried-Over) with item counts
- [ ] Milestone timeline visible at top showing dates, status, and critical milestones
- [ ] Responsive on desktop (1920x1080 minimum) and tablet (1024x768)
- [ ] No loading spinners visible after 2 seconds

### User Story 2: Track Work Item Progress
**As a** project manager  
**I want to** drill into detailed work items by status, milestone, and assignee  
**So that** I can identify bottlenecks and re-prioritize work

**Acceptance Criteria:**
- [ ] Grid displays all work items with title, status, target date, % complete, assignee
- [ ] Filter panel allows filtering by status, milestone, date range (minimum 3 independent filters)
- [ ] Items sortable by due date, % complete, and status
- [ ] Grid handles 500+ items without perceptible lag or freezing
- [ ] Column headers are sticky on scroll for large datasets

### User Story 3: Monitor Real-Time Updates
**As a** dashboard viewer  
**I want to** see status updates automatically when data.json changes  
**So that** multiple viewers always see current project state

**Acceptance Criteria:**
- [ ] FileSystemWatcher detects data.json changes within 500ms
- [ ] Connected viewers receive updates without page refresh
- [ ] No console errors during rapid successive updates (10+ per minute)
- [ ] Graceful handling of concurrent viewer connections (10+ simultaneous)
- [ ] Update debouncing prevents excessive re-renders (minimum 500ms between updates)

### User Story 4: Maintain Data Integrity
**As a** system administrator  
**I want to** ensure dashboard loads correct data from data.json  
**So that** executives make decisions based on accurate information

**Acceptance Criteria:**
- [ ] Dashboard validates JSON structure on load and logs validation results
- [ ] Handles missing or malformed fields gracefully with user-facing error messages
- [ ] File change debounce prevents update storms (minimum 500ms between re-parses)
- [ ] Dashboard continues functioning if data.json is temporarily unavailable
- [ ] Audit logs track all data loads and errors for compliance review

### User Story 5: Access Dashboard Securely
**As an** executive stakeholder  
**I want to** access the dashboard only if I have proper authorization  
**So that** sensitive project data is protected

**Acceptance Criteria:**
- [ ] Dashboard requires authentication via Windows Auth (corporate network) or role-based session
- [ ] HTTPS is enforced on all connections (including local deployment)
- [ ] Users cannot access dashboard without valid credentials
- [ ] Session timeout occurs after 30 minutes of inactivity

## Scope

### In Scope
- Single-page dashboard displaying milestones, status cards, progress grid, filters
- JSON file-based data loading (data.json) with FileSystemWatcher for real-time file monitoring
- Basic charting: bar chart by status; timeline/Gantt using SyncFusion (Blazor Suite v24.1) or Radzen Blazor alternative
- Authentication: Windows Authentication (corporate network) or role-based authorization
- Responsive UI for desktop (1920x1080+) and tablet (1024x768+) viewports
- Docker containerization for single-image local deployment to Windows Server or Linux host
- Component unit tests using bUnit with minimum 70% code coverage for critical features
- Serilog structured logging to local file with daily rollover (30-day retention)
- Health check endpoint for basic uptime monitoring

### Out of Scope
- In-app data editing or write-back functionality to data.json
- Integration with external project management tools (Jira, Azure DevOps, Microsoft Project)
- Historical trend analysis, burndown charts, or velocity reports
- Email notifications, Slack alerts, or push notifications
- Mobile app, PWA (Progressive Web App), or cross-platform native clients
- Cloud deployment (AWS, Azure, GCP) or distributed systems
- Advanced analytics, machine learning, or predictive insights
- Role-based access control beyond basic binary authorization (admin/viewer)
- Multi-tenant or multi-project dashboard aggregation
- API endpoint for programmatic dashboard access

## Non-Functional Requirements

| Category | Requirement | Target | Notes |
|----------|-------------|--------|-------|
| **Performance** | Dashboard initial load time | <2 seconds on local network | Measured from cold start; after warming up browser cache |
| **Performance** | Grid rendering (500+ items) | Virtualization to render visible rows only; no >500ms lag on scroll | Use Radzen/SyncFusion built-in virtualization |
| **Performance** | Real-time update latency | File change detected and rendered to all viewers within 500ms | FileSystemWatcher debounced to 500ms minimum |
| **Performance** | Time-to-interactive (TTI) | <3 seconds | User can interact with filters and grid before this threshold |
| **Availability** | Uptime SLA (business hours) | 99.5% | Single point of failure acceptable for MVP; health check endpoint required |
| **Availability** | Mean time to recovery (MTTR) | <15 minutes | Manual restart of Docker container acceptable |
| **Scalability** | Concurrent viewers | Support 10+ simultaneous connections without degradation | SignalR connection pooling configured; monitor active connections |
| **Scalability** | Data size limit | Up to 500 work items; 100KB JSON file max (MVP) | Archive or migrate to SQLite if >100MB |
| **Security** | Authentication method | Windows Auth (corporate) OR role-based session auth | No OAuth for local-only deployment |
| **Security** | Transport security | HTTPS mandatory (self-signed certificates acceptable locally) | Kestrel defaults to HTTPS; enforce via middleware |
| **Security** | Data at rest | OS-level filesystem permissions on data.json | No encryption required for local-only; version control via Git |
| **Security** | Secrets management | appsettings.Development.json (excluded from Git) | No hardcoded credentials in source code |
| **Compliance** | Audit logging | All data loads, errors, and user access logged | Serilog structured logs; retain 30 days |
| **Compliance** | Data backup | data.json versioned in Git; SQLite backup (if used) daily | Git history provides immutable audit trail |
| **Usability** | Learning curve | First-time user can understand dashboard without training | Intuitive layout; clear labeling; <1 day for non-technical users |
| **Usability** | Accessibility | WCAG 2.1 Level AA compliance | Keyboard navigation, color contrast, alt text for charts |
| **Maintainability** | Code documentation | All public methods and components documented via XML comments | Minimum coverage: 80% of public surface area |
| **Maintainability** | Test coverage | Minimum 70% code coverage for component logic | bUnit + xUnit; exclude trivial getters/setters |
| **Maintainability** | Logging verbosity | Structured logging at INFO level in production; DEBUG in development | Use Serilog property enrichment for context |

## Success Metrics

- [ ] Dashboard deployed to staging environment and accessible via HTTPS within 4 weeks
- [ ] Proof-of-concept timeline/Gantt visualization reviewed and approved by 2+ executive stakeholders
- [ ] 3+ executive stakeholders actively using dashboard by end of week 5
- [ ] Dashboard loads in <2 seconds measured on production hardware under typical network conditions
- [ ] Zero critical bugs (severity P0/P1) in first 2 weeks post-launch
- [ ] Stakeholder satisfaction survey score ≥8/10 on usability (minimum 2 respondents)
- [ ] Time spent manually compiling status reports reduced by 80% (baseline measurement required)
- [ ] 100% uptime during first 2-week production monitoring window (business hours only)
- [ ] All unit tests passing; code coverage ≥70% for component logic
- [ ] No security vulnerabilities identified in HTTPS, authentication, or data handling

## Constraints & Assumptions

### Technical Constraints
- **Framework**: Blazor Server with .NET 8.0 runtime (mandatory)
- **Deployment**: Local-only (corporate network or on-premises server); no cloud infrastructure
- **Host options**: Windows Server (IIS or Kestrel) or Linux server (Docker + Kestrel)
- **Charting library**: SyncFusion Blazor Suite v24.1 ($2,395/dev/year license cost) OR Radzen Blazor (free, limited timeline support)
- **Single point of failure**: MVP does not require HA, backup servers, or load balancing
- **Data persistence**: JSON file-based (no database required); optional SQLite only if historical queries needed
- **Concurrent connections**: SignalR default supports 100 concurrent; sufficient for executive use case

### Timeline Assumptions
- **Development timeline**: 4–6 weeks to MVP (depends on charting library choice and timeline complexity)
- **Research & design**: Week 1
- **Core dashboard build**: Weeks 2–3
- **Charting integration**: Week 4 (SyncFusion) or Weeks 4–5 (custom timeline)
- **Testing & hardening**: Week 5
- **Stakeholder review & UAT**: Week 6
- **Contingency**: 1–2 weeks for unforeseen technical blockers

### Data & Dependency Assumptions
- **data.json format**: Provided by user; assumed to follow the specified schema (project, milestones, workItems structure)
- **Data updates**: Manual JSON file edits OR external ETL pipeline (not specified; impacts architecture)
- **Data frequency**: Assume updates no more than once per hour (affects debouncing strategy)
- **Data volume**: <500 work items in MVP scope; JSON file <100KB
- **Stakeholder availability**: 1–2 executive stakeholders available for weekly UAT/feedback sessions

### Operational Assumptions
- **Ownership**: One primary data owner responsible for JSON file updates and maintenance
- **Network**: Corporate network assumed stable; no WANs, mobile networks, or unreliable connectivity
- **Hosting**: IT/DevOps team available to deploy Docker container and manage HTTPS certificates
- **Monitoring**: Basic health check sufficient; no enterprise APM (Application Performance Monitoring) required
- **Compliance**: No regulatory requirements (HIPAA, SOC 2, GDPR) beyond basic access controls and audit logging

### Feature Assumptions
- **Timeline scope**: Display milestones only; individual work items appear in grid (not on timeline)
- **Drill-down**: Clicking work item in grid shows details; no separate detail modal required for MVP
- **Filtering**: Additive filters (AND logic); no complex saved filter profiles
- **Export**: Not required for MVP (no Excel, PDF, or CSV export)
- **Notifications**: Not required for MVP; executives check dashboard manually
- **Integration**: No Jira, Azure DevOps, or external tool sync for MVP

---

**Document Version**: 1.0  
**Last Updated**: 2026-04-07  
**Owner**: Product Manager  
**Status**: Ready for Architecture & Engineering Handoff