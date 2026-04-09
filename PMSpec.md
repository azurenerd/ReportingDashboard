# PM Specification: My Project

## Executive Summary
Build a lightweight, screenshot-ready single-page executive dashboard that visualizes project health, milestone progress, and deliverables by reading data from a local data.json file. Using Blazor Server, Bootstrap 5.3.3, and System.Text.Json, the dashboard enables executives to create professional PowerPoint presentations without manual data compilation, reducing reporting overhead by 80%.

## Business Goals
1. Deliver a simple, no-frills dashboard that executives can screenshot for PowerPoint presentations
2. Eliminate manual status compilation workflows by automating data visualization from JSON source
3. Provide real-time project visibility showing what's shipped, in-progress, carried-over, and upcoming milestones
4. Enable non-technical product managers to update dashboard data via data.json without code changes
5. Ensure consistent, pixel-perfect rendering across multiple monitors and DPI settings for professional presentation quality

## User Stories & Acceptance Criteria

**US-1: View Project Milestone Timeline**
- As an executive, I want to see a horizontal timeline of major project milestones so I can understand project critical path and delivery dates
- Acceptance Criteria:
  - [ ] Timeline displays 5-10 milestones in chronological order at the top of the dashboard
  - [ ] Each milestone shows: name, due date, and status (completed/on-track/at-risk/blocked) with color coding
  - [ ] Milestones are visually distinct and easy to scan in <5 seconds
  - [ ] Timeline renders identically on 1920x1080 and 4K monitors without scaling artifacts
  - [ ] Screenshot is clean, with no rendering artifacts or blinking content

**US-2: View Project Progress Metrics**
- As an executive, I want to see overall project completion percentage, counts of shipped/in-progress/carried-over items so I can quickly assess health
- Acceptance Criteria:
  - [ ] Dashboard displays: % complete, % carried over, item counts for each category
  - [ ] Progress metrics update automatically when data.json changes (hot-reload within 10 seconds)
  - [ ] Metrics are displayed both numerically and visually (optional: doughnut chart via Chart.js)
  - [ ] Data accuracy verified: metrics match source data.json 100%

**US-3: View Shipped, In-Progress, and Carried-Over Work**
- As an executive, I want to see lists of completed items, active work, and carryover items so I can understand team capacity and rollover risks
- Acceptance Criteria:
  - [ ] Three-column layout displays "Shipped", "In Progress", and "Carried Over" sections
  - [ ] Each section shows item title and brief description in card format
  - [ ] Cards are color-coded by category for visual distinction
  - [ ] Layout adapts responsively to tablet (col-md-6) and mobile (col-12) views
  - [ ] Cards render cleanly in screenshots without text overlap or truncation

**US-4: Load Project Data from JSON File**
- As a product manager, I want the dashboard to read project data from a local data.json file so I can update milestone/progress data without code changes
- Acceptance Criteria:
  - [ ] Dashboard loads data.json on startup automatically
  - [ ] JSON schema supports: project name, quarter, milestones array (id, name, dueDate, status), shipped/in-progress/carried-over arrays (id, title, description)
  - [ ] Deserialization handles missing optional fields gracefully (no crashes)
  - [ ] File parsing errors log to browser console; dashboard retains previous data
  - [ ] Sample data.json template provided with 2-3 fictional projects for testing

**US-5: Hot-Reload Dashboard on Data Changes**
- As a product manager, I want the dashboard to automatically reload when data.json is modified so I can iterate without restarting
- Acceptance Criteria:
  - [ ] Changes to data.json trigger re-render within 10 seconds
  - [ ] Hot-reload works on local SSD deployments
  - [ ] Hot-reload works on network share deployments (via Timer fallback if FileSystemWatcher fails)
  - [ ] No duplicate renders, flicker, or UI glitches on reload
  - [ ] Polling continues automatically if file watcher encounters permissions issues

**US-6: Screenshot Dashboard for PowerPoint**
- As an executive, I want to take pixel-perfect screenshots of the dashboard for PowerPoint decks so presentations look professional and consistent
- Acceptance Criteria:
  - [ ] Dashboard renders identically across Chrome, Edge, Safari, and Firefox
  - [ ] Text is crisp and legible at all monitor DPI settings (1.0x to 2.5x scaling)
  - [ ] No animations, blinking, or dynamic content that would interfere with screenshots
  - [ ] Color scheme and spacing optimized for readability in presentations
  - [ ] Executive team approves visual design and color scheme before deployment

## Scope

### In Scope
- Single-page Blazor Server dashboard (one route, no navigation menus)
- Local data.json file loading via System.Text.Json deserialization
- FileSystemWatcher + Timer-based hybrid hot-reload mechanism
- Bootstrap 5.3.3 responsive grid layout via CDN (no build tools)
- Custom CSS dashboard styling (200 lines max) for brand colors and spacing
- Chart.js 4.4.0 via CDN for optional progress bar/gauge visualization
- Three-tier component hierarchy: DashboardContainer → TimelineSection / ProgressSection / StatusCardsSection
- Hardcoded sample project data for testing and demo purposes
- Data models: ProjectData, Milestone, StatusItem, ProgressMetrics (C# classes)
- DashboardDataService for JSON parsing and file-watching logic

### Out of Scope
- User authentication, login, or role-based access control
- Multi-user concurrency, permissions, or user profiles
- Historical data tracking, trend analysis, or Q-over-Q comparisons
- Real-time notifications (Slack, email, SMS integrations)
- API integrations with Jira, Azure DevOps, GitHub, or other tools
- Database backend or server-side persistence
- Mobile app, native iOS/Android, or offline PWA mode
- Advanced charting libraries (Plotly, D3.js, ApexCharts)
- Automated email distribution or scheduled report generation
- Encrypted data storage, PII handling, or secrets management
- High-availability deployment (multiple instances, load balancer, failover)
- Advanced performance optimizations (pagination, lazy loading, server-side caching)
- Accessibility beyond basic WCAG 2.1 compliance
- Custom domain setup or HTTPS SSL certificates

## Non-Functional Requirements

| Requirement | Target | Rationale |
|---|---|---|
| **Load Time** | <2 seconds cold start on local SSD | Executives use during live meetings; delays erode trust |
| **Hot-Reload Latency** | <10 seconds from file change to UI update | Acceptable for batch-driven reporting; not real-time |
| **Rendering Consistency** | Identical output across Chrome, Edge, Safari, Firefox | Screenshot fidelity critical for PowerPoint distribution |
| **Availability** | 99% uptime on local deployment | Read-only local app with no external dependencies |
| **Data Accuracy** | 100% fidelity to source data.json | No calculation errors or data transformation bugs |
| **Scalability** | Supports up to 5,000 total dashboard items (milestones + cards) | Larger datasets require pagination (future phase) |
| **Deployment Friction** | Single `dotnet run` command; no npm/yarn/npm build step required | Non-developers should be able to launch dashboard |
| **Security** | No authentication required; no PII in data.json; deployable on isolated network | Read-only internal tool; no cloud exposure or public internet access |
| **Resource Footprint** | 50-100MB RAM at idle; 200-300MB disk for runtime + code | Acceptable for developer workstation or internal server |
| **Responsive Design** | Grid adapts: col-lg-4/col-lg-3 (desktop), col-md-6 (tablet), col-12 (mobile) | Screenshots must be readable on multiple monitor sizes |

## Success Metrics

1. **Executive Adoption:** Executive team uses dashboard screenshots in ≥3 consecutive reporting cycles without requesting manual alternatives
2. **Time Savings:** Reporting data compilation time reduced from 4 hours to <30 minutes per reporting cycle
3. **Screenshot Quality:** Zero complaints from executives about rendering inconsistency, readability, or layout issues across monitors
4. **Data Accuracy:** No discrepancies between dashboard display and source data.json (100% match rate)
5. **Hot-Reload Reliability:** File change detection and re-render works 100% of the time on target deployment environment (test on actual machine before production)
6. **Performance:** Cold start latency ≤2 seconds; hot-reload latency ≤10 seconds; zero UI lag or flicker
7. **Code Quality:** Core DashboardDataService and Models covered by xUnit tests with ≥80% code coverage
8. **Design Approval:** Executive stakeholders approve visual design, color scheme, and information hierarchy before Week 2 completion

## Constraints & Assumptions

### Technical Constraints
- **Single Blazor Server Instance:** No load balancing, clustering, or high-availability failover (single point of failure acceptable for internal tool)
- **Local File Storage Only:** No cloud blob storage, databases, or remote APIs; data.json is the single source of truth
- **No External Build Tools:** Pure .NET 8 CLI tooling only; no npm, yarn, webpack, or Node.js required
- **FileSystemWatcher Platform Limitations:** Windows FileSystemWatcher fails on network shares and WSL; hybrid Timer-based fallback required
- **JSON File Atomic Writes:** Requires rename-after-write pattern to prevent partial JSON reads during file updates

### Business Constraints
- **Fixed Timeline:** Delivery by end of Q2 2026 for executive reporting cycle
- **Single Project Focus:** MVP targets one project only; multi-project support deferred to future phase
- **No Cloud Resources:** Must run on local workstation or isolated internal server (no AWS, Azure, or GCP accounts)
- **Limited Customization Budget:** Design and CSS locked after Week 1 approval; avoid late-stage color/layout changes

### Assumptions
- **Data.json Location:** Accessible local SSD or network share; executives have read permissions
- **Update Frequency:** Data.json modified ≤10 times per day (batch-driven reporting, not streaming)
- **Executive Browser:** Modern Chrome/Edge/Safari with JavaScript enabled; no IE11 or legacy browser support required
- **Development Environment:** Visual Studio 2022 v17.8+ or VS Code with C# DevKit; .NET 8.0.x SDK pre-installed
- **Deployment Environment:** Developer workstation or isolated internal server with .NET 8 runtime; no public internet access required
- **Data Size:** Project data < 5MB (JSON file); < 5,000 total dashboard items
- **Authentication Model:** Dashboard accessed by trusted internal team only; no auth/RBAC required
- **CDN Availability:** jsdelivr CDN reachable from deployment network (Bootstrap and Chart.js CDN links accessible)
- **Component Stability:** Data model and component structure locked after Week 1 design review; minimal schema changes thereafter
- **Screenshot Target:** 1920x1080 monitor as primary baseline; 4K and laptop displays as secondary targets
- **Design Reference:** OriginalDesignConcept.html and C:/Pics/ReportingDashboardDesign.png inform initial mockups; all agents review before building

### Dependencies
- .NET 8.0.x SDK and runtime pre-installed
- Visual Studio 2022 v17.8+ or VS Code with C# DevKit
- Git repository access (AgentSquad.Runner project)
- Bootstrap 5.3.3 CDN and Chart.js 4.4.0 CDN reachable from deployment network
- Sample data.json template agreed upon by product manager before Phase 2