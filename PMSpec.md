# PM Specification: My Project

## Executive Summary

Build a lightweight executive dashboard using Blazor Server (.NET 8) that loads project status from a local `data.json` file and renders a screenshot-friendly single-page view. The dashboard displays milestones, tasks grouped by status (Shipped | In-Progress | Carried-Over), and fiscal quarter timelines—optimized for PowerPoint deck integration with zero authentication, database dependencies, or external charting libraries. Deliver MVP in 3-4 days with <5MB artifact footprint.

## Business Goals

1. Enable executives to assess project health (milestones, shipped work, in-progress tasks, slipped items) in a single view without login friction
2. Eliminate database dependencies and deployment complexity via local file-based JSON configuration
3. Deliver screenshot-quality visuals (1920x1080 desktop viewport) optimized for static PowerPoint presentations
4. Reduce status reporting overhead by removing interactive charting frameworks and focusing on clarity over feature breadth
5. Support grayscale print-safety and colorblind-accessible design patterns for inclusive executive communication
6. Achieve MVP launch within 3-4 days with minimal .NET ecosystem dependencies (Tailwind CSS only)

## User Stories & Acceptance Criteria

### US-1: Load & Display Project Dashboard from JSON

**As an** executive  
**I want to** view project status on a single-page dashboard  
**So that** I can assess milestones, shipped work, in-progress tasks, and carried-over items in one view

**Acceptance Criteria:**
- [ ] Dashboard loads data from local `data.json` file via System.Text.Json deserialization
- [ ] Loading state displays while parsing JSON
- [ ] DataAnnotations validation catches missing required fields (name, status, milestones, tasks)
- [ ] Error card renders with readable message if JSON parse fails (not stack trace)
- [ ] Manual "Refresh" button triggers DataProvider.LoadAsync() to reload data
- [ ] Screenshot at 1920x1080 renders crisp text, borders, and colors without scaling artifacts
- [ ] No responsive breakpoints or mobile media queries applied

### US-2: Visualize Milestone Timeline with Fiscal Quarters

**As an** executive  
**I want to** see upcoming milestones as a horizontal timeline with fiscal quarter labels  
**So that** I can understand project delivery cadence and major deliverables at a glance

**Acceptance Criteria:**
- [ ] Timeline renders 3-15 milestones via Tailwind CSS divs (no ChartJS, no SVG)
- [ ] Each milestone displays as a gradient bar with due date formatted as "Q2 FY2026 (May 15)"
- [ ] Fiscal year start month defaults to October (US FY convention)
- [ ] Timeline section appears at top of dashboard above task board
- [ ] Milestones render identically across Chrome and Edge browsers
- [ ] Grayscale PDF export preserves all visual information (no color-only signals)
- [ ] Timeline fits within 1920px viewport without horizontal scrolling

### US-3: Display Tasks in Status-First 3-Column Layout

**As an** executive  
**I want to** see all tasks grouped by status (Shipped | In-Progress | Carried-Over)  
**So that** I can quickly assess what's delivered, what's active, and what's slipped

**Acceptance Criteria:**
- [ ] TaskBoard renders 3 fixed-width columns via `grid grid-cols-3 gap-4`
- [ ] Each column displays task count in header and scrollable card list
- [ ] Scrollable area uses `h-96 overflow-y-auto scroll-smooth` with smooth scrolling
- [ ] TaskCard.razor renders individual tasks with status-specific styling applied conditionally
- [ ] Tasks grouped via LINQ on Dashboard.razor render (no JSON restructuring required)
- [ ] Column headers labeled: "Shipped", "In-Progress", "Carried-Over"
- [ ] All 12 sample tasks display without pagination buttons

### US-4: Apply Layered Visual Distinction for Task Status

**As an** executive  
**I want to** distinguish task status via color + border + typography + text badge  
**So that** I can scan status even in grayscale print and colorblind-accessible modes

**Acceptance Criteria:**
- [ ] **Shipped tasks**: `bg-green-50` + `border-l-4 border-solid border-green-500` + `text-green-900` + ✓ checkmark
- [ ] **In-Progress tasks**: `bg-blue-50` + `border-l-4 border-solid border-blue-500` + `text-blue-900 font-semibold` + ⏱ badge
- [ ] **Carried-Over tasks**: `bg-amber-50` + `border-l-4 border-dashed border-amber-400` + `text-amber-900 line-through` + ? badge
- [ ] All color combinations meet WCAG AAA contrast ratio (18:1 minimum) verified with axe DevTools
- [ ] Grayscale PDF export preserves visual distinction via border patterns + strikethrough (no color alone)
- [ ] Strikethrough applies to carried-over task titles to signal past-due status

### US-5: Handle Data Load Failures Gracefully

**As an** executive  
**I want to** see clear error messaging if data.json is missing or malformed  
**So that** I can take corrective action without confusion

**Acceptance Criteria:**
- [ ] Parse errors render error card with human-readable message (not JSON stack trace)
- [ ] Error card includes "Reload" button to trigger manual refresh
- [ ] Exception details logged to browser console for troubleshooting
- [ ] Dashboard renders empty state if data.json not found (no 500 error or blank white page)
- [ ] Error card provides instruction: "Check data.json format and click Reload"
- [ ] Loading state displays for <5 seconds before timeout

## Scope

### In Scope

- Single project dashboard displaying one project's metrics at a time
- Local file-based JSON configuration (`data.json` colocated with application)
- Manual refresh button (not auto-reload on file change)
- Status-first task grouping via LINQ (Shipped → In-Progress → Carried-Over)
- Tailwind CSS timeline visualization (gradient bars, no ChartJS or interactive charts)
- Desktop-only viewport targeting (1920x1080 fixed, no mobile responsiveness)
- Light DataAnnotations validation (required field checks only, no FluentValidation)
- Custom DateFormatter class with fiscal year support (Q1-Q4 FY labels)
- Three task status categories: Shipped, In-Progress, Carried-Over
- Error handling with fallback UI (friendly error card instead of exception page)
- Unit tests for DataProvider.LoadAsync() (xUnit + Moq)
- Styling to match OriginalDesignConcept.html visual language via Tailwind utilities
- Sample data.json with 3 milestones and 12 tasks (4 shipped, 4 in-progress, 4 carried-over)
- Grayscale print-safe design (color + pattern + text, not color alone)

### Out of Scope

- Database integration or API connectivity (Phase 2+)
- Multi-project navigation or project switcher tabs (Phase 2+)
- Auto-reload via FileSystemWatcher or polling (Phase 2+)
- Heroicons or Font Awesome icon library (Phase 1 uses text badges; icons Phase 2+)
- CSV/Excel export functionality (Phase 2+)
- Real-time data push via WebSocket or SignalR (Phase 2+)
- Authentication, login, or role-based access control
- Drill-down interactivity (clickable milestones, expandable task details)
- Mobile responsiveness or tablet layout variants
- Customizable color schemes per team or organization (Phase 2+)
- Fiscal year configuration in UI (Phase 1: hardcoded October; Phase 2: JSON config)
- Historical version tracking, audit logs, or task history
- Task filtering, sorting, or search functionality
- Owner badges or team assignments on task cards
- Real-time collaboration features
- Cloud deployment (Azure App Service, AWS Lambda, etc.)
- CI/CD pipelines or automated testing in this MVP

## Non-Functional Requirements

### Performance

- **Startup Time**: <5 seconds (Blazor Server initialization + JSON parse)
- **Page Load**: Dashboard renders within 2 seconds of DataProvider.LoadAsync() completion
- **JSON Parse**: <100ms for files <5MB
- **Refresh Action**: Complete data reload and re-render within 1 second
- **Scrolling**: Smooth scrolling with <60ms frame time (no jank on task cards)

### Browser Compatibility & Rendering

- **Target Browsers**: Chrome 90+, Edge 90+ (desktop only)
- **Screenshot Consistency**: Identical visual rendering across Chrome and Edge at 1920x1080
- **Viewport**: Fixed 1920x1080 desktop resolution (no responsive breakpoints)
- **Print Quality**: Grayscale PDF export preserves all status distinctions (color + border + pattern + text)

### Accessibility

- **WCAG Compliance**: WCAG AAA contrast ratios (18:1 minimum) for all text + background combinations
- **Colorblindness Support**: Status visibility via layered signals (color + border pattern + strikethrough + text badge), not color alone
- **Grayscale Safety**: Dashboard functional in black-and-white print mode
- **Font Readability**: Minimum font size 14px for body text, 16px+ for headers

### Data Validation & Error Handling

- **Validation Type**: Light (DataAnnotations only—`[Required]` attribute on POCOs)
- **Parse Failures**: Render error card instead of blank page or exception stack
- **Silent Failures**: None allowed; all errors logged to browser console
- **Data Loss Prevention**: No data mutations without explicit save action (MVP: read-only dashboard)

### Artifact & Deployment

- **Artifact Size**: <5MB total (Blazor Server runtime + .NET 8 framework + application DLLs)
- **No External Dependencies**: Tailwind CSS 3.4.x only; no node_modules, no npm footprint
- **Hosting**: Local IIS 10+ or Kestrel (self-hosted); no cloud services
- **File Permissions**: data.json must be readable by IIS/Kestrel application identity

### Scalability Limits

- **Task Capacity**: Recommended <200 tasks for smooth scrolling; >500 requires Blazor.Virtualize (Phase 2)
- **Milestone Capacity**: Recommended <50 milestones; >100 requires pagination (Phase 2)
- **JSON File Size**: <5MB comfortable; >50MB requires database migration (Phase 2+)

### Reliability & Availability

- **No SLA Required**: Internal dashboard, no uptime commitment
- **Connection Drops**: Blazor Server reconnection logic handles brief disconnects; graceful degradation on longer outages
- **Data Backups**: data.json is source of truth; recommend version control (Git) or periodic snapshots
- **Crash Recovery**: Manual refresh button provides recovery path if dashboard becomes unresponsive

## Success Metrics

| Metric | Definition | Target | Owner |
|--------|-----------|--------|-------|
| **US-1 Completion** | Dashboard loads sample data.json without errors, displays on first page load | 100% pass | Dev |
| **US-2 Completion** | Timeline renders 3 milestones with "Q2 FY2026 (May 15)" format in 1920px viewport | 100% pass | Dev |
| **US-3 Completion** | TaskBoard displays 12 tasks in 3 columns (4 shipped, 4 in-progress, 4 carried-over) | 100% pass | Dev |
| **US-4 Completion** | TaskCard styling matches spec (green/blue/amber + borders + strikethrough) with WCAG AAA contrast | 100% pass | QA |
| **US-5 Completion** | Malformed JSON renders error card with "Reload" button (not blank or 500 error) | 100% pass | Dev |
| **Screenshot Quality** | 1920x1080 render identical in Chrome/Edge; text crisp, borders sharp, colors consistent | Pass | QA |
| **Grayscale Print Safety** | PDF export preserves Shipped/In-Progress/Carried-Over distinction without color | Pass | QA |
| **Accessibility Audit** | axe DevTools browser extension reports zero contrast violations | 0 issues | QA |
| **Code Coverage** | DataProvider.LoadAsync() unit tests cover nominal + error paths | >80% | Dev |
| **Data Load Success** | DataProvider parses sample data.json without exceptions | 100% | Dev |
| **Artifact Size** | Published DLL footprint <5MB | <5MB | Dev |
| **PowerPoint Integration** | Dashboard screenshot pastes into PowerPoint deck with readable text | Pass | PM |
| **MVP Delivery Timeline** | All 5 user stories completed, tested, and ready for demo | Day 3-4 | PM |
| **Executive Sign-Off** | Status visibility matches cognitive model (Shipped first, then In-Progress, then Carried-Over) | Qualitative sign-off | Exec |

## Constraints & Assumptions

### Technical Constraints

- **Technology Stack**: Blazor Server (.NET 8.0.x), Tailwind CSS 3.4.x via NuGet, System.Text.Json (no external charting libraries like ChartJS or SyncFusion)
- **Validation Framework**: Light DataAnnotations only; no FluentValidation, JSON Schema validators, or enterprise validation
- **Data Source**: Local `data.json` file only (no REST API, no database, no cloud storage)
- **Refresh Model**: Manual button only (no FileSystemWatcher auto-reload, no polling, no real-time updates in MVP)
- **Timeline Visualization**: Tailwind CSS HTML divs only (no SVG generation, no Canvas rendering, no interactive charts)
- **Hosting**: Local IIS 10+ or Kestrel self-hosted (no Azure App Service, no AWS Lambda, no containerization in MVP)
- **Icon Library**: Text badges only in Phase 1 (no Heroicons, no Font Awesome in MVP)

### Timeline Assumptions

- **MVP Delivery**: 3-4 days (21 hours of development + QA)
- **Phase 2 Deferral**: Auto-reload, icons, CSV export, multi-project support pushed post-launch
- **Sprint Duration**: Single continuous sprint, no multi-week iterations

### Data & Schema Assumptions

- **data.json Stability**: Schema remains stable during Phase 1 (no versioning, migrations, or backward compatibility in MVP)
- **Task Count**: <200 tasks per project (representative of single-project scope)
- **Milestone Count**: <50 milestones per project
- **File Size**: JSON files <5MB comfortable
- **Fiscal Year Start**: October (US FY convention); configurable post-MVP
- **Manual Updates**: data.json updated manually by project manager (no upstream ETL integration, no Jira/Azure DevOps sync in Phase 1)
- **Carried-Over Logic**: No auto-expiration (quarterly manual cleanup discipline required)

### Organizational & Team Assumptions

- **Executive Consumption Model**: Executives access dashboard via static screenshot (not live browser session)
- **Developer Skillset**: .NET developer familiar with Blazor Server, C# POCOs, and Tailwind CSS
- **PowerPoint Integration**: Screenshots exported for PowerPoint decks at 1920x1080 resolution
- **Feedback Loop**: Executive review and sign-off available Day 3-4 for demo
- **No Mobile Usage**: Executives do not consume dashboard on phones or tablets
- **Internal Use Only**: No multi-tenant architecture, no SaaS deployment, no external stakeholders

### Design Assumptions

- **OriginalDesignConcept.html Replication**: Design language from HTML template replicated via Tailwind utilities (no CSS hand-coding, no design system overhaul)
- **Color Scheme Fixed**: Green/blue/amber colors fixed; no per-team customization in Phase 1
- **Single Project Dashboard**: One project per dashboard instance (no multi-project switcher in Phase 1)
- **Screenshot-First Priority**: All design decisions weighted toward screenshot clarity over interactive features
- **No Real-Time Requirement**: Manual refresh sufficient for daily standup snapshots (no live updates during presentations)