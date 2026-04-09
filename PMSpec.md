# PM Specification: My Project

## Executive Summary

We are building a simple, single-page executive dashboard that visualizes project milestones and progress status in a screenshot-ready format for PowerPoint presentations. The dashboard will load project data from a JSON configuration file and display four status categories (shipped, in-progress, carried-over, at-risk) alongside a timeline visualization of major milestones. Built on .NET 8 with Blazor Server and MudBlazor, this local-only application requires zero infrastructure and eliminates manual status report creation.

## Business Goals

1. Enable program managers to generate presentation-ready screenshots of project status without manual compilation
2. Provide executives with a single-page view of project health across four key work-item categories
3. Deliver a lightweight, zero-configuration dashboard deployable to any Windows machine with .NET 8
4. Eliminate infrastructure complexity by using local JSON files as the data source
5. Create a reusable executive reporting template for future projects

## User Stories & Acceptance Criteria

### US-1: Executive Views Project Timeline
**As a** project manager  
**I want to** see a horizontal timeline of major milestones at the top of the dashboard  
**So that** I can communicate project phases and key deliverables to executives in presentations

**Acceptance Criteria:**
- [ ] Timeline displays 4-5 key milestones with target dates
- [ ] Each milestone shows status: Completed, In Progress, At Risk, or Upcoming
- [ ] Milestones are sorted chronologically left-to-right
- [ ] Visualization fits within 1920x1080 viewport without horizontal scrolling
- [ ] Timeline uses a clear visual style (horizontal bar chart) matching OriginalDesignConcept.html

### US-2: Executive Assesses Work Status
**As a** C-level executive  
**I want to** see four status cards showing counts of shipped, in-progress, carried-over, and at-risk items  
**So that** I can quickly assess overall project health and progress

**Acceptance Criteria:**
- [ ] Four distinct cards display accurate work item counts from data.json
- [ ] Each card includes label, count number, and progress bar (0-100%)
- [ ] Color coding applied: Green (shipped), Blue (in-progress), Orange (carried-over), Red (at-risk)
- [ ] Cards are horizontally arranged and readable from 10+ feet distance on a projector
- [ ] Font sizes meet minimum 18pt for projector visibility

### US-3: Executive Generates Screenshot for Presentation
**As a** program manager  
**I want to** take a screenshot of the dashboard in light theme  
**So that** I can embed it directly in PowerPoint slides without additional formatting

**Acceptance Criteria:**
- [ ] Entire dashboard fits within single 1920x1080 screenshot with no scrolling required
- [ ] Light theme is optimized for projector display and has high contrast
- [ ] All text, numbers, colors, and layout elements are clearly visible and readable
- [ ] Dashboard design matches or improves upon OriginalDesignConcept.html
- [ ] Screenshot quality is suitable for executive presentation without post-processing

## Scope

### In Scope
- Single-page Blazor Server application with Dashboard.razor as main page
- Four status cards displaying shipped, in-progress, carried-over, and at-risk item counts
- Horizontal milestone timeline visualization at top of dashboard
- JSON data loading from data.json file on application startup
- Light theme optimized for PowerPoint presentations and projector display
- Responsive layout tested at 1920x1080 resolution
- Sample data.json with fictional project data
- MudBlazor component styling for professional appearance
- StatusCard component (reusable 4x for different statuses)
- MilestoneTimeline component for chronological milestone display
- DataService for JSON file loading and parsing
- Project Models (ProjectData, Milestone, WorkItem classes)

### Out of Scope
- Multi-user support or concurrent dashboard access
- User authentication or authorization (no login required)
- Real-time data refresh via WebSockets or polling
- Dark theme toggle (Phase 2 enhancement)
- PDF or Excel export capabilities (screenshots sufficient)
- Custom color theming per project
- Role-based access control (RBAC)
- Cloud deployment or Azure integration
- Database integration or persistence layer
- Mobile-responsive design (<1024px viewports)
- Print stylesheets or print optimization
- Data validation or error recovery UI
- Dashboard state persistence across sessions
- Multi-project support or project selector

## Non-Functional Requirements

| Requirement | Target | Rationale |
|-------------|--------|-----------|
| **Load Time** | <2 seconds from app start to fully rendered dashboard | Executives expect instant visual feedback |
| **JSON Parsing** | <100ms for files up to 10MB | Should not block UI during data loading |
| **Memory Usage** | <100MB resident memory footprint | Local machine resource efficiency |
| **Browser Compatibility** | Chrome 90+, Edge 90+ on Windows 10+ | Standard corporate browser support |
| **Viewport Resolution** | Optimized for 1920x1080; responsive to 1280x720 | PowerPoint slide standard dimensions |
| **Color Contrast** | WCAG AA minimum for light theme | Projector readability at distance |
| **Font Readability** | Minimum 16pt body text, 18pt+ for status numbers | Visible from 10+ feet |
| **Error Handling** | Graceful degradation if data.json missing or malformed | User-friendly error messages |
| **Zero Configuration** | `dotnet run` launches without config file setup | Lowest friction deployment |
| **Security Model** | No authentication; relies on OS file system ACLs | Internal-only application, no sensitive data |
| **Encryption** | Not required | Local-only deployment, fictional data |

## Success Metrics

- ✓ Dashboard loads and renders all 4 status cards with correct counts from sample data.json
- ✓ Milestone timeline displays 4-5 milestones in chronological order with visual bar representation
- ✓ Full dashboard screenshot (1920x1080, 100% zoom) matches or improves upon OriginalDesignConcept.html design
- ✓ Entire dashboard visible without vertical or horizontal scrolling at 1920x1080 resolution
- ✓ Application starts successfully via `dotnet run` without configuration or environment setup
- ✓ Light theme optimized for projector display and high-contrast PowerPoint integration
- ✓ All text, numbers, and visual elements are clear and executive-ready in screenshots
- ✓ Stakeholder sign-off: "Ready for PowerPoint presentation"
- ✓ Code compiles without warnings or errors
- ✓ Unit tests pass for DataService JSON parsing (2-3 tests minimum)

## Constraints & Assumptions

**Technical Constraints:**
- No external database; data.json file is the single source of truth
- Local-only deployment (no cloud services, no Azure, no multi-machine sync)
- Single project per dashboard instance (not multi-project dashboard)
- No user authentication or authorization required
- No real-time data synchronization or cross-user collaboration
- Build and deploy must work on Windows 10+ with .NET 8 SDK installed
- JSON files limited to 10MB for reasonable startup performance
- Browser-based UI only; no desktop client or mobile app

**Timeline Assumptions:**
- MVP deliverable in 3-5 developer days
- Design lockdown occurs before development begins (OriginalDesignConcept.html is final)
- Stakeholder review and approval after functional prototype (Day 2)
- No iterative design cycles; one build-and-review phase

**Dependency Assumptions:**
- Development team has .NET 8.0 SDK installed locally
- .NET 8 Hosting Bundle available for shared server deployment (if needed post-MVP)
- MudBlazor 6.10.0+ remains compatible with .NET 8 throughout project
- System.Text.Json (built-in) provides sufficient JSON parsing capability
- CChart.js library supports Blazor component integration without custom JavaScript
- Windows file system ACLs provide adequate security for fictional project data

**Data & Usage Assumptions:**
- Executives will manually edit data.json or sync from external project management tool
- Dashboard used for monthly/quarterly status reviews, not daily real-time monitoring
- Fictional project data is acceptable for MVP; no sensitive or production data required
- Screenshots taken at 1920x1080 in 100% browser zoom for consistency
- Presentation context is PowerPoint slides with standard 16:9 aspect ratio
- Users will not store production or confidential information in data.json
- No archival or historical data retention needed (single snapshot per review)

**Architecture Assumptions:**
- MudBlazor Material Design components provide 80%+ of UI design needs without custom CSS
- Cascading parameters are sufficient for read-only data passing (no complex state management)
- Single Blazor Server project is appropriate for MVP scope
- Eager loading of JSON at startup is acceptable (lazy loading deferred to Phase 2)
- Light theme is primary; dark theme toggle is Phase 2 enhancement