# PM Specification: My Project

## Executive Summary

Build a lightweight, single-page executive reporting dashboard in C# .NET 8 Blazor Server that displays project milestones, progress, and task status. The solution reads data from a JSON configuration file, requires no authentication or cloud infrastructure, and prioritizes simplicity and screenshot-ready visuals for PowerPoint decks.

## Business Goals

1. Provide executives real-time visibility into project health, milestone progress, and task status
2. Enable quick screenshot capture from the dashboard for integration into PowerPoint executive briefings
3. Reduce manual status reporting overhead by centralizing project metrics in a single dashboard
4. Create a lightweight, self-contained reporting tool deployable locally without cloud dependencies or IT infrastructure
5. Establish a reusable template for executive project reporting across multiple initiatives

## User Stories & Acceptance Criteria

**User Story 1: View Project Milestones Timeline**

As an executive, I want to see a visual timeline of major project milestones at the top of the dashboard, so that I can quickly assess which milestones are on-track and identify upcoming deliverables.

- [ ] Timeline displays milestone name, target date, and completion status
- [ ] Timeline is full-width at top of page and visually prominent
- [ ] Completed milestones are clearly differentiated from pending/in-progress milestones
- [ ] Timeline is responsive and readable on desktop and laptop screens (1024px+)
- [ ] Font size is ≥12pt for readability in PowerPoint screenshots

**User Story 2: Monitor Task Status Breakdown**

As an executive, I want to see counts of shipped, in-progress, and carried-over tasks organized in visual cards, so that I can quickly assess project momentum and identify where work is stalled.

- [ ] Three status cards display: Shipped, In-Progress, Carried-Over with task counts
- [ ] Cards use color coding: green (shipped), blue (in-progress), orange (carried-over)
- [ ] Each card shows count and task list
- [ ] Cards automatically update when data.json is refreshed
- [ ] Cards are laid out in responsive grid below timeline section

**User Story 3: View Progress Metrics**

As an executive, I want to see project progress visualizations including burn-down and completion percentage, so that I can assess overall project velocity and remaining work.

- [ ] Progress chart displays current project completion percentage
- [ ] Visual representation is clear and legible for slide screenshots
- [ ] Chart updates when data.json is reloaded
- [ ] No animations that interfere with static screenshot capture
- [ ] Responsive to multiple screen sizes

**User Story 4: Load and Display Project Data from JSON**

As an admin, I want to update project status by editing data.json and refreshing the browser, so that I can quickly communicate status changes without requiring code deployment.

- [ ] Dashboard loads data.json on startup
- [ ] Malformed JSON displays user-friendly error message
- [ ] Dashboard automatically refreshes when browser is refreshed after data.json edit
- [ ] No database backend required
- [ ] Sample data.json provided with fictional project example

## Scope

### In Scope

- Single-project executive dashboard (no multi-project switcher)
- JSON-based data model (data.json) for all project metrics and status
- Blazor Server components: Dashboard.razor, MilestoneTimeline.razor, StatusCard.razor, ProgressMetrics.razor
- Bootstrap 5 responsive grid layout
- Chart.js visualization for milestones and progress metrics
- Local file-based execution (no cloud services)
- Sample fake project data with 3-4 milestones and 8-10 tasks
- Error handling for corrupted JSON files
- README documentation with data schema and deployment instructions
- No authentication or authorization system

### Out of Scope

- Multi-user support or role-based access control
- Authentication, login, or session management
- Historical trend tracking or archived snapshots
- PDF export or email distribution functionality
- Database backend (file-based storage only)
- Cloud hosting or deployment infrastructure
- Admin UI for data editing (manual JSON editing only)
- Multi-project dashboard switcher
- Real-time data synchronization across users
- Mobile app or mobile-responsive design
- Data backup or recovery features
- Automated data refresh from external sources

## Non-Functional Requirements

| Category | Requirement |
|----------|-------------|
| **Performance** | Dashboard loads in <2 seconds; charts render in <500ms |
| **Compatibility** | Chrome, Edge, Safari on Windows/Mac; minimum screen width 1024px |
| **Reliability** | Graceful error handling for corrupted JSON; no unhandled exceptions; validates JSON on load |
| **Security** | No authentication system; file access controlled via OS-level permissions on data.json |
| **Simplicity** | Minimal external dependencies; single-page layout; no complex navigation flows |
| **Accessibility** | Font size ≥12pt; WCAG AA contrast compliance; clear visual hierarchy |
| **Screenshot Quality** | No animations that interfere with static screenshot capture; consistent rendering across refreshes |
| **Scalability** | Current design supports 1 project; future refactoring can support multiple projects |

## Success Metrics

✓ Dashboard loads without errors and displays all sample project data correctly  
✓ All four user stories fully implemented and verified through acceptance criteria  
✓ Screenshots captured from dashboard copy cleanly into PowerPoint without quality loss  
✓ JSON data changes reflected in UI within one browser refresh cycle  
✓ Charts render correctly on desktop screens (1024px to 1920px width)  
✓ Milestone timeline visually distinguishes completed vs. pending milestones  
✓ No authentication prompts or cloud service dependencies in deployment  
✓ sample data.json provided with fictional project (e.g., "Q2 Mobile App Release")  
✓ README documentation includes data.json schema, component structure, and local deployment steps  
✓ Code builds and runs successfully with `dotnet run` on Windows and Linux  

## Constraints & Assumptions

**Technical Constraints:**
- Must execute locally on Windows or Linux (no cloud infrastructure)
- No external authentication systems or third-party identity providers
- Single-page dashboard only (no multi-page navigation)
- File system-based data storage (data.json in project directory or wwwroot)
- Blazor Server architecture (no standalone WebAssembly)

**Timeline Assumptions:**
- MVP delivery target: 1-2 weeks
- Team has C# .NET 8 development environment configured
- Visual design can reference OriginalDesignConcept.html (simplified, not replicated exactly)

**Dependency Assumptions:**
- .NET 8 SDK available on deployment machine
- Bootstrap 5.x CDN accessible or bundled with application
- Chart.js v4.x available via JavaScript Interop or CDN
- Users can manually edit JSON and refresh browser (no automation required)
- data.json remains on local machine for entire application lifecycle

**Scope Assumptions:**
- One active project per dashboard instance
- No need for real-time updates across multiple simultaneous users
- PowerPoint presentation is primary consumption pattern (not live dashboard monitoring)
- Executive audience accesses via desktop/laptop browser only
- Project uses consistent milestone and task naming conventions