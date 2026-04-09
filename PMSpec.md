# PM Specification: My Project

## Executive Summary

Build a single-page Blazor Server dashboard that enables executives to visualize project milestones, progress, and work status at a glance. The dashboard reads data from a JSON configuration file and renders as a screenshot-optimized view designed for PowerPoint embedding—no cloud dependencies, no authentication, minimal complexity. Delivery in 4-6 weeks using MudBlazor 6.10.2 and self-contained .NET 8 deployment.

## Business Goals

1. Enable executives to assess project health via a single, deterministic screenshot
2. Provide visibility into shipped features, in-progress work, and carryover items
3. Simplify project status reporting by eliminating manual PowerPoint slide updates
4. Deliver a tool with zero cloud dependencies and zero installation burden on stakeholder machines
5. Establish a foundation for future multi-project dashboards (Phase 2)

## User Stories & Acceptance Criteria

### US-1: View Project Milestone Timeline
**As an** executive, **I want** to see all project milestones on a horizontal timeline at the top of the dashboard, **so that** I understand key delivery dates and completion progress.

**Acceptance Criteria:**
- [ ] Timeline displays 5-10 major milestones with target dates and status
- [ ] Each milestone shows completion percentage (0-100%)
- [ ] Status indicators display: Completed, In Progress, Not Started
- [ ] Timeline renders on first dashboard load without pagination
- [ ] Print-to-PDF via Ctrl+P preserves timeline layout without wrapping

### US-2: Track Work Item Status Across Three Columns
**As a** project manager, **I want** to see work distributed across Shipped, In Progress, and Carried Over columns, **so that** stakeholders understand what was delivered, what's active, and what slipped.

**Acceptance Criteria:**
- [ ] Dashboard displays three distinct columns: Shipped, In Progress, Carried Over
- [ ] Each work item shows: title, assignee, and (for carried over) reason for slip
- [ ] Column counts auto-calculate and display as totals (e.g., "Shipped: 30")
- [ ] 20-30 work items render without scrolling (fixed-width 1200px layout)
- [ ] Carried over items clearly indicate blocking reasons

### US-3: Load Dashboard Data from JSON Configuration
**As a** project owner, **I want** to maintain project data in a human-editable data.json file, **so that** I can update progress without code changes.

**Acceptance Criteria:**
- [ ] Dashboard reads wwwroot/data.json on application startup
- [ ] JSON schema validates against required fields; clear error messages on failure
- [ ] Malformed JSON prevents dashboard render with actionable error text
- [ ] Optional 5-minute timer-based refresh reloads data.json from disk
- [ ] No database, no cloud API calls, zero external dependencies

### US-4: Display Executive Summary Metrics
**As an** executive, **I want** to see KPI counts (total items, shipped %, in progress %, carryover %), **so that** I quickly gauge project health.

**Acceptance Criteria:**
- [ ] Metrics panel displays: Total Items, Shipped Count, In Progress Count, Carried Over Count
- [ ] Percentages auto-calculate from progress data (e.g., Shipped % = shippedCount / totalItems * 100)
- [ ] No manual data entry required; all metrics derived from work items
- [ ] Metrics update on dashboard load or refresh

### US-5: Export Dashboard for PowerPoint
**As a** presenter, **I want** to take a screenshot and paste it into my deck, **so that** stakeholders see a professional project status view.

**Acceptance Criteria:**
- [ ] Dashboard renders as single page (no pagination, entire view fits in 1200x1600px viewport)
- [ ] CSS print media queries optimize for export (hide chrome, set fixed widths)
- [ ] Screenshot identical across Chrome 124+ and Edge 124+ browsers
- [ ] Layout consistent on Windows 10/11 at 1920x1080 and 1440x900 resolutions
- [ ] Print-to-PDF tested and verified for PowerPoint embedding

## Scope

### In Scope
- Single-page Blazor Server dashboard (server-side rendering, no client SPA complexity)
- MudBlazor 6.10.2 UI components (Card, Grid, Timeline, Button, Dialog)
- MudTimeline component for milestone horizontal timeline visualization
- Three-column ProgressBoard layout (Shipped/In Progress/Carried Over)
- MetricsPanel with KPI summary (counts and percentages)
- ProjectDashboardState service for in-memory data management
- System.Text.Json configuration loading from wwwroot/data.json
- FluentValidation 11.9.x for JSON schema validation at startup
- Fixed-width 1200px layout for consistent slide alignment
- CSS media print queries for PowerPoint export optimization
- Self-contained Windows x64 .exe deployment (single executable + wwwroot bundled)
- Example data.json file with fictional project
- README documentation (setup instructions, data.json schema, deployment guide)

### Out of Scope
- User authentication or role-based access control (RBAC)
- Cloud deployment (AWS, Azure, or any hosted service)
- Multi-project support (single project per dashboard; multi-project deferred to Phase 2)
- Automated screenshot capture or headless browser integration
- Database or persistent storage (JSON file only for MVP)
- Real-time data integrations (Jira, Azure DevOps, GitHub Projects)
- Programmatic PDF/Word export API (browser print-to-PDF sufficient)
- Historical data tracking, trending, or month-over-month analysis
- Mobile-responsive design (desktop Windows-only)
- Dark mode, theming, or customizable color schemes
- Concurrent data editing or multi-user scenarios

## Non-Functional Requirements

| Category | Requirement | Target | Rationale |
|---|---|---|---|
| **Performance** | Application startup time | <2 seconds on modern hardware | Users expect instant feedback |
| **Performance** | Dashboard initial render | <1 second | Preserve presentation flow |
| **Security** | Data transmission | Local machine only | No cloud or external API exposure |
| **Security** | HTTPS enforcement | Default dev certificate (localhost:5001) | Blazor Server default; sufficient for local use |
| **Security** | Credentials in data.json | None; semi-public metadata only | No sensitive PII or secrets stored |
| **Reliability** | Missing data.json handling | Startup check + in-memory fallback | Graceful error messages instead of crash |
| **Reliability** | JSON schema validation | Startup validation via FluentValidation | Prevent silent data corruption |
| **Scalability** | Work item limit | <50 items per dashboard | Executive-level summary; not detailed tracking |
| **Scalability** | Milestone limit | 5-10 items | Prevents timeline clutter |
| **Deployability** | Installation complexity | Zero; standalone .exe, no .NET runtime pre-required | Stakeholder machines clean of dependencies |
| **Deployability** | Package size | ~150-200MB (self-contained .exe) | Acceptable for modern storage; frame-dependent option available if needed |
| **Screenshot Reproducibility** | Cross-browser consistency | Identical on Chrome 124+, Edge 124+ | CSS fixed-width layout, no JavaScript rendering variations |
| **Screenshot Reproducibility** | Cross-OS consistency | Identical on Windows 10/11 | Tested at 1920x1080 and 1440x900 resolutions |
| **Print Quality** | Print-to-PDF output | Preserves all elements without pagination | CSS media queries optimize whitespace and layout |

## Success Metrics

- ✓ Dashboard renders identically at 1920x1080 and 1440x900 on Windows 10/11
- ✓ Print-to-PDF (Ctrl+P) generates single-page output suitable for PowerPoint
- ✓ data.json validation catches schema violations with clear error messages at startup
- ✓ Self-contained .exe executes on clean Windows machine (no .NET 8 pre-installed)
- ✓ All MudBlazor components (Timeline, Grid, Card, Button) render without console errors
- ✓ Milestone timeline displays all 5-10 milestones with dates, status, and completion %
- ✓ Progress board accurately tallies and displays shipped/in-progress/carryover counts
- ✓ Metrics panel auto-calculates percentages: Shipped %, In Progress %, Carryover %
- ✓ Application startup completes in <2 seconds
- ✓ Dashboard initial render completes in <1 second
- ✓ README documents: setup instructions, data.json schema, deployment steps
- ✓ Stakeholder testing confirms successful screenshot-to-PowerPoint workflow
- ✓ Example data.json with fictional project (e.g., "Project Phoenix") included and functional

## Constraints & Assumptions

### Technical Constraints
- **Windows-Only**: Self-contained deployment targets Windows x64 exclusively; no Linux/macOS support
- **Single data.json**: File-based storage; no concurrent write safety or multi-user locking
- **Fixed Data Schema**: Structural changes to JSON require code recompilation (not schema-driven)
- **No Cloud**: All processing occurs on local machine; no cloud APIs or external data sources
- **Executable Size**: ~150-200MB bundled with .NET 8 runtime; acceptable for modern storage
- **Browser Requirement**: Chrome 124+, Edge 124+; no legacy IE or older versions
- **In-Memory State**: Application state lost on restart; no persistence between sessions

### Timeline Assumptions
- **4-6 week delivery**: Based on Blazor Server ecosystem maturity and MudBlazor feature completeness
- **Iterative development**: Week-by-week milestones with stakeholder demos
- **No external integrations**: MVP assumes manual data.json updates (no Jira/Azure DevOps connectors)
- **Single developer/team**: Assumes C# and Blazor familiarity; learning curve for non-.NET teams adds 1-2 weeks

### Dependency Assumptions
- **Visual Studio 2022 Community**: Available and pre-configured (or VS Code + C# Dev Kit)
- **.NET 8 SDK**: Downloaded and installed locally for development
- **MudBlazor 6.10.2**: Latest stable version available on NuGet (as of research date 2026-04-09)
- **FluentValidation 11.9.x**: Compatible with .NET 8, available on NuGet
- **Stakeholder Windows machines**: Windows 10 or Windows 11; modern hardware (post-2018)
- **No enterprise infrastructure**: No AD/LDAP, no group policy restrictions, local admin access for deployment

### Business Assumptions
- **Single project per dashboard**: One data.json = one project view; no multi-project selector
- **Manual data.json maintenance**: Project owner edits JSON directly or via external tool; no web UI for data entry
- **Screenshots only**: PowerPoint export via manual browser screenshot (Ctrl+PrintScreen); no programmatic export
- **Point-in-time visibility**: Dashboard shows current state; no historical tracking or trend analysis
- **Authorized stakeholders only**: No public sharing; assumes trusted network or local-only access
- **No sensitive data**: Project metadata treated as semi-public; no PII, credentials, or compliance-sensitive fields