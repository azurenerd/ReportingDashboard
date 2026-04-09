# PM Specification: My Project

## Executive Summary

My Project is a lightweight, screenshot-ready executive dashboard that displays project milestones, work item status (shipped, in-progress, carried-over), and progress metrics on a single page. Built on .NET 8 Blazor Server with Bootstrap 5.3.0, it loads data from a local JSON configuration file and renders pixel-perfect screenshots for PowerPoint presentations without post-processing. No authentication, no cloud dependencies, no enterprise overhead—just a simple local web application that runs via `dotnet run` on executive machines.

## Business Goals

1. **Enable Executive Visibility**: Display project status, shipped items, active work, carried-over items, and milestone timeline on a single screen with zero scrolling for typical workloads.
2. **Minimize Time-to-Screenshot**: Executives must capture PowerPoint-ready screenshots in <30 seconds, with zero Photoshop or image editing required.
3. **Eliminate Infrastructure Overhead**: Deliver dashboard functionality in 2-3 days with zero cloud services, no CI/CD, no authentication—run locally via `dotnet run`.
4. **Ensure Screenshot Consistency**: Render identically across Chrome, Edge, Firefox on Windows/macOS/Linux for reproducible presentation decks.
5. **Simplify Data Management**: Executives update dashboard content by editing `data.json` and restarting the application; no database, no API backend.

## User Stories & Acceptance Criteria

### US-1: Load Project Dashboard Data

**As an** executive,  
**I want to** launch the dashboard and immediately see my project status,  
**So that** I can generate screenshots for presentations without waiting for external APIs.

**Acceptance Criteria:**
- [ ] Dashboard loads within 500ms on local machine (measured via browser DevTools)
- [ ] Data loads from `data.json` file at application startup
- [ ] If `data.json` is missing or malformed, dashboard displays user-friendly error message (not stack trace)
- [ ] Same JSON structure supports multiple projects (extensible for Phase 2)
- [ ] No external API calls or cloud dependencies

---

### US-2: View Milestone Timeline

**As an** executive,  
**I want to** see a visual timeline of major project milestones at the top of the dashboard,  
**So that** I can understand what's coming and assess schedule risk.

**Acceptance Criteria:**
- [ ] Timeline displays 5-10 milestones horizontally across page width
- [ ] Each milestone shows: name, target date, and status badge (Completed, On Track, At Risk, Delayed)
- [ ] Status colors are distinct: green (completed), blue (on track), orange (at risk), red (delayed)
- [ ] Timeline fits on single screen viewport without horizontal scrolling
- [ ] Milestones render in chronological order, left to right
- [ ] Implementation uses HTML/CSS (no external charting library)

---

### US-3: View Work Item Status Grid

**As an** executive,  
**I want to** see three columns: "Shipped" (completed), "In Progress" (active), and "Carried Over" (delayed/backlog),  
**So that** I can assess momentum and identify blockers at a glance.

**Acceptance Criteria:**
- [ ] Three columns render side-by-side on desktop viewport
- [ ] Each column displays work items as cards with: title, description (optional), completion date
- [ ] Column header shows item count (e.g., "Shipped (8)")
- [ ] All columns visible simultaneously; no horizontal scrolling for <50 items per column
- [ ] Work items render in reverse chronological order (most recent first)
- [ ] Cards use consistent styling: white background, subtle shadow, readable typography

---

### US-4: View Progress Metrics Footer

**As an** executive,  
**I want to** see summary metrics at the bottom of the dashboard (total planned, completed %, health score),  
**So that** I can quickly reference overall project health.

**Acceptance Criteria:**
- [ ] Footer displays: Total Planned Items, Completed Count, Completion %, Health Score (0-100)
- [ ] Health Score calculated as: (Completed / Total) * 100, displayed as percentage
- [ ] Metrics use large, legible font sizes for visibility in screenshots
- [ ] Footer visible within single viewport without excessive scrolling
- [ ] Colors match milestone status palette (green for healthy, orange for at risk, red for blocked)

---

### US-5: Load Data from JSON Configuration

**As a** dashboard operator,  
**I want to** supply project data via a `data.json` file,  
**So that** I can update dashboard content without recompiling code.

**Acceptance Criteria:**
- [ ] Application looks for `data.json` in `ContentRootPath/data/` directory
- [ ] JSON schema maps exactly to C# models: ProjectDashboard, Milestone, WorkItem, ProgressMetrics
- [ ] Deserialization uses System.Text.Json with source-generated serializers
- [ ] Extra JSON fields are silently ignored (forward compatibility)
- [ ] Missing required fields trigger detailed error log and empty dashboard display
- [ ] Sample data.json provided with 5-10 fictional milestones and 40-50 work items

---

### US-6: Generate Presentation Screenshots

**As an** executive,  
**I want to** take native browser screenshots and paste them directly into PowerPoint,  
**So that** I don't spend time in Photoshop adjusting layouts or colors.

**Acceptance Criteria:**
- [ ] Dashboard renders identically on Chrome, Edge, Firefox (latest versions)
- [ ] Screenshot at 1920x1080 resolution shows entire dashboard without cropping
- [ ] No unstyled content flashes during render; Bootstrap CSS loaded before first paint
- [ ] Colors are print-friendly and meet WCAG AA contrast ratios
- [ ] No animated elements cause flicker or render delays during screenshot

---

### US-7: Local Deployment

**As a** developer,  
**I want to** run the dashboard locally with `dotnet run` and access it via `https://localhost:7xxx`,  
**So that** no IT infrastructure or container setup is required.

**Acceptance Criteria:**
- [ ] Application builds with `dotnet build` in <5 seconds on clean checkout
- [ ] Application runs with `dotnet run` and listens on default HTTPS port (7xxx)
- [ ] No external service dependencies (no Docker, no cloud services, no databases)
- [ ] All required dependencies available via NuGet
- [ ] Application exits gracefully on Ctrl+C

---

## Scope

### In Scope

- Single-page executive dashboard with milestone timeline, status grid (shipped/in-progress/carried-over), progress metrics
- Data loading from local `data.json` file at application startup
- Responsive HTML/CSS layout using Bootstrap 5.3.0 and Bootstrap Icons 1.11.3
- Sample `data.json` with fictional project (10 milestones, 40-50 work items)
- Local Kestrel HTTP server (no authentication, no authorization)
- Desktop browser support: Chrome, Edge, Firefox latest versions
- Manual testing and screenshot verification
- C# data models: ProjectDashboard, Milestone, WorkItem, ProgressMetrics
- ProjectDataService singleton pattern for JSON loading
- 5 Blazor components: DashboardPage, TimelinePanel, StatusGrid, StatusColumn, MetricsFooter
- Unit tests (xUnit) and component tests (Bunit)
- README.md with usage instructions and data.json schema documentation

### Out of Scope

- Multi-user access control or authentication
- Real-time data synchronization from external APIs
- Database backend (file-based JSON only)
- Mobile-responsive design optimization
- PDF/image export functionality
- File watcher for hot-reload (deferred to Phase 2)
- Advanced charting or visualizations beyond HTML/CSS
- Email distribution or notification system
- Audit logging or compliance features
- Containerization (Docker) or cloud deployment
- Performance testing with >100 work items per category
- Accessibility testing beyond manual WCAG 2.1 review
- Data persistence or save functionality
- Multi-project dashboard selector (deferred to Phase 3)
- Custom fonts or advanced typography

---

## Non-Functional Requirements

### Performance

- **Page Load Time**: <500ms on local machine (measured via browser DevTools)
- **Time to Interactive**: <1 second (dashboard fully interactive after load)
- **Re-render Latency**: <50ms on status change
- **Page Size**: <500KB uncompressed; <150KB gzip-compressed
- **Memory Usage**: <100MB resident memory (single-user local execution)

### Accessibility

- **WCAG 2.1 Level AA** compliance for color contrast, keyboard navigation, screen reader compatibility
- All work item cards and timeline elements keyboard-accessible (tabindex, focus indicators)
- Color not sole differentiator; milestone status uses color + text badge
- Alt text for icon-only status indicators
- No animated elements that flash >3 times per second

### Security

- **Zero encryption at rest**: data.json stored unencrypted on local filesystem (local-only assumption)
- **Zero encryption in transit**: HTTPS optional; HTTP acceptable for localhost-only deployment
- **Path validation**: All file I/O uses `Path.Combine()` with ContentRootPath; no user-supplied paths
- **Input validation**: JSON deserialization rejects malformed data
- **Logging**: No sensitive data in logs; only load success/failure events
- **Process isolation**: Application runs under user executing `dotnet run`

### Reliability

- **Uptime**: Not applicable (single-user local tool)
- **Data Integrity**: JSON deserialization validates schema; malformed data fails fast with descriptive error
- **Graceful Degradation**: Missing data.json displays empty dashboard with error message
- **Error Handling**: Try-catch around file I/O; log errors to console; display user-friendly error pages

### Scalability

- **Dataset Size**: Tested and performant for <100 work items per status category
- **Component Hierarchy**: 4-6 Blazor components; scales to 10+ without re-render latency
- **Concurrent Users**: Not applicable (single-user local execution)
- **Future Extensions**: If shared via network (Phase 3), implement Windows auth and circuit limits

### Testability

- **Unit Tests**: C# models and ProjectDataService logic covered by xUnit
- **Component Tests**: Blazor components tested in isolation via Bunit
- **Integration Tests**: End-to-end test loading sample data.json and rendering full dashboard
- **Manual Testing**: Screenshot comparison across browsers/OS before delivery

---

## Success Metrics

### Delivery Metrics

- ✓ Dashboard built and deployed in ≤3 days (MVP)
- ✓ Zero compile-time errors; all unit and integration tests pass
- ✓ Application runs locally with `dotnet run` without external service setup

### Functional Metrics

- ✓ All 7 user stories completed and verified
- ✓ Sample data.json loads without deserialization errors
- ✓ Dashboard displays all milestone and work-item data without truncation
- ✓ Metrics footer shows calculated health score and completion % matching JSON data

### Non-Functional Metrics

- ✓ Page load time <500ms (measured via browser DevTools on local machine)
- ✓ Screenshot identical across Chrome, Edge, Firefox (pixel comparison)
- ✓ All text meets WCAG AA contrast ratio (4.5:1 for normal text, 3:1 for large text)
- ✓ Keyboard navigation through all elements without mouse (Tab, Shift+Tab, Enter)
- ✓ Build time <5 seconds on clean checkout

### Business Metrics

- ✓ Executive can generate PowerPoint-ready screenshot within 30 seconds of dashboard launch
- ✓ No post-processing or image editing required before pasting into presentation
- ✓ Single-page view accommodates typical review (<50 items per category) without scrolling
- ✓ Dashboard ready for weekly/monthly stakeholder updates with zero manual data entry

---

## Constraints & Assumptions

### Technical Constraints

- **Platform**: Windows, macOS, or Linux with .NET 8.0 SDK installed
- **Framework**: C# with Blazor Server (no JavaScript framework modifications)
- **Browser Support**: Chrome/Edge/Firefox latest versions
- **Port**: Default HTTPS (7xxx) or HTTP (5xxx)
- **File System**: data.json located in `ContentRootPath/data/` directory
- **Network**: Local machine only; no remote API endpoints
- **Build System**: `dotnet` CLI only; no npm, Webpack, or external build tools
- **Runtime**: .NET 8.0 LTS (support until November 2026)

### Business Assumptions

- **Single User**: Dashboard runs on executive's machine; no multi-user concurrency
- **Static Data Session**: One-time load at startup; manual refresh via app restart (hot-reload deferred to Phase 2)
- **Executable Deliverable**: Executive receives compiled .exe or `dotnet run` instructions
- **Screenshot as Primary Use Case**: Not an interactive web application; primary value is pixel-perfect screenshots
- **Data Ownership**: Executive owns data.json file; application is read-only consumer

### Design Assumptions

- **Typical Dataset**: 5-10 milestones, 30-50 work items total (<100 items per category)
- **Viewport**: Single-page view on 1920x1080 desktop monitor (no mobile optimization)
- **Color Scheme**: Bootstrap default palette with executive-grade accent colors (#0066cc, #28a745, #ffc107)
- **Typography**: System font stack (Bootstrap defaults); no custom webfonts
- **Icons**: Bootstrap Icons library (1500+ SVG icons)
- **Accessibility**: WCAG 2.1 Level AA sufficient (no Level AAA required)

### Operational Assumptions

- **Developer**: IT-literate operator can run `dotnet run` and provide sample data.json
- **No DevOps**: No CI/CD pipeline, no automated deployments
- **No Training**: Dashboard UI is self-explanatory
- **Ephemeral Data**: No audit trail or historical tracking
- **Local Execution**: Executive runs on their local machine
- **Manual Refresh**: Executive restarts app to load updated data.json

### Risk Mitigations

| Risk | Mitigation |
|------|-----------|
| JSON schema mismatch during deserialization | Provide sample data.json; document schema in code comments; use typed C# models |
| Browser screenshot inconsistency across OS/versions | Test on Windows, macOS, Linux; use Bootstrap defaults; avoid custom CSS |
| Performance degradation with large datasets | Virtualization deferred to Phase 2; load-test with 100+ items before Phase 2 |
| File I/O failure at startup | Implement try-catch; display error message; provide default empty dashboard |
| Blazor circuit memory leaks | Monitor via browser DevTools; configure DisconnectedCircuitMaxRetained = 100 |

---

## Implementation Architecture

### Component Hierarchy

```
DashboardPage.razor (root, loads ProjectDataService, renders layout)
├── TimelinePanel.razor (displays milestones horizontally)
├── StatusGrid.razor (3-column wrapper, responsive grid)
│   ├── StatusColumn.razor (shipped items)
│   ├── StatusColumn.razor (in-progress items)
│   └── StatusColumn.razor (carried-over items)
└── MetricsFooter.razor (displays health score, completion %, summary)
```

### Data Models

```csharp
public class ProjectDashboard
{
    public string ProjectName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedCompletion { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<WorkItem> Shipped { get; set; }
    public List<WorkItem> InProgress { get; set; }
    public List<WorkItem> CarriedOver { get; set; }
    public ProgressMetrics Metrics { get; set; }
}

public class Milestone
{
    public string Name { get; set; }
    public DateTime TargetDate { get; set; }
    public string Status { get; set; } // "Completed", "OnTrack", "AtRisk", "Delayed"
}

public class WorkItem
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class ProgressMetrics
{
    public int TotalPlanned { get; set; }
    public int Completed { get; set; }
    public int InFlight { get; set; }
    public decimal HealthScore { get; set; }
}
```

### Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | Blazor Server | .NET 8.0.0+ |
| UI Framework | Bootstrap | 5.3.0 |
| Icons | Bootstrap Icons | 1.11.3 |
| JSON Parsing | System.Text.Json | 8.0.0 (built-in) |
| State Management | Cascading Parameters | Built-in |
| Testing | xUnit + Bunit | 8.0.0+ |
| Deployment | Kestrel | Built-in |

---

## Timeline & Deliverables

### Phase 1 (MVP): 2-3 Business Days

- ✓ Visual Studio project configured with .NET 8.0 Blazor Server template
- ✓ C# data models (ProjectDashboard, Milestone, WorkItem, ProgressMetrics)
- ✓ ProjectDataService singleton with startup JSON loading
- ✓ 5 Blazor components (DashboardPage, TimelinePanel, StatusGrid, StatusColumn, MetricsFooter)
- ✓ Bootstrap 5.3.0 stylesheet and Bootstrap Icons integration
- ✓ Custom site.css with executive color palette overrides
- ✓ Sample data.json with fictional project
- ✓ Unit tests (xUnit) and component tests (Bunit)
- ✓ README.md with usage instructions and schema documentation

### Phase 2 (Polish): Optional, Deferred

- File watcher + SignalR hot-reload for live data refresh
- Virtualization for 100+ work items per category
- Screenshot testing suite (automated browser comparisons)
- Accessibility audit (WCAG 2.1, axe DevTools)
- Performance profiling (Lighthouse, SignalR memory)

### Phase 3 (Future): Out of Scope

- Multi-project dashboard selector
- PDF/image export functionality
- Database backend (SQLite or SQL Server)
- Network deployment and Windows authentication