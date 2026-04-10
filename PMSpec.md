# PM Specification: My Project

## Executive Summary

Build a simple, screenshot-optimized one-page Blazor Server 8.0 dashboard for executives to visualize project milestones, progress, and status at a glance. The dashboard reads project data from a JSON configuration file, displays health status, timeline visualization, and metrics breakdown (shipped/in-progress/carryover), with automatic data refresh via FileSystemWatcher and print CSS for clean PowerPoint exports. Deploy as standalone application in <3 weeks with zero authentication, external dependencies, or cloud infrastructure.

## Business Goals

1. Enable executives to quickly assess project health status (On Track, At Risk, Off Track) at a glance
2. Provide visual timeline of major milestones with completion tracking
3. Display metrics breakdown: shipped items, in-progress work, carryover items from previous periods
4. Enable zero-friction screenshot export to PowerPoint decks without manual cleanup
5. Eliminate external dependencies and authentication complexity (local file-based data only)
6. Deliver MVP within 3 weeks with minimal infrastructure overhead

## User Stories & Acceptance Criteria

**US-1: View Executive Dashboard**

As an executive, I want to see a single-page dashboard with project status, milestones, and metrics, so that I can quickly assess project health and share progress with stakeholders.

Acceptance Criteria:
- [ ] Dashboard loads without authentication or login prompts
- [ ] Displays project name, current status, and last-updated timestamp
- [ ] Fully renders on Windows desktop at 1920x1080 and 1366x768 resolutions
- [ ] Metrics cards clearly show shipped/in-progress/carryover item counts
- [ ] Timeline chart displays all milestones with completion status indicators
- [ ] Page loads completely in under 2 seconds on standard hardware

**US-2: View Milestone Timeline**

As an executive, I want to see a visual timeline of project milestones with due dates, so that I understand when major deliverables are expected.

Acceptance Criteria:
- [ ] ChartJs line chart displays milestones on a continuous date axis
- [ ] Completed milestones marked visually (checkmark, different color, or filled marker)
- [ ] Incomplete milestones clearly distinguished from completed ones
- [ ] Month and year labels visible on timeline axis
- [ ] Chart updates when data.json changes (no application restart required)
- [ ] Timeline renders without console errors in browser developer tools

**US-3: Export Dashboard as Screenshot**

As an executive, I want to capture clean screenshots for PowerPoint without navigation clutter, so that I can include dashboard visuals in executive presentations.

Acceptance Criteria:
- [ ] Print-to-PDF produces clean output with no navigation menu visible
- [ ] Content area maximized with no empty margins or UI artifacts
- [ ] Metrics cards and timeline render consistently across zoom levels (100%, 125%, 150%)
- [ ] No interactive elements (buttons, dropdowns, hover states) visible in printed view
- [ ] Screenshot quality consistent across Chrome, Edge, and Firefox browsers
- [ ] Printed output maintains readability at standard PowerPoint display sizes

**US-4: Update Dashboard Data**

As a project manager, I want to update project metrics by editing a JSON file, so that executives always see current project status without application changes.

Acceptance Criteria:
- [ ] Edit data.json file to update shipped items, in-progress items, and carryover items
- [ ] Edit milestone names, due dates, and completion status in data.json
- [ ] Dashboard automatically reloads within 500ms of file save
- [ ] Malformed JSON shows fallback data with no application crash
- [ ] Supports at least 50 milestone entries and 100+ metric items per project
- [ ] File format documented with example data.json for reference

## Scope

### In Scope
- Standalone Blazor Server 8.0 application (Windows .NET 8 environment)
- JSON file-based data source (data.json in application directory)
- Milestone timeline visualization using ChartJs.Blazor 3.4.0
- Metrics cards displaying shipped, in-progress, and carryover item counts
- Project status summary with health indicator (On Track / At Risk / Off Track)
- Bootstrap 5.3.x responsive styling and grid system
- Print CSS media queries for screenshot optimization
- FileSystemWatcher event-driven monitoring for live data.json reload
- Debounced reload logic (500ms) to prevent cascade updates
- Example fictional project data demonstrating all dashboard features
- Component-based architecture (Dashboard, MetricsCard, MilestoneTimeline, StatusSummary)
- Last-updated timestamp display and data refresh indicator
- Fallback error handling for malformed JSON (displays default/empty state)

### Out of Scope
- Multi-user authentication, login screens, or role-based access control
- Database, SQL Server, SQLite, or cloud infrastructure
- Real-time team collaboration or live data feed integration
- Mobile responsiveness or tablet/phone optimization (desktop-optimized only)
- Historical data archival, trend analysis, or performance comparisons
- API integrations, webhook handlers, or external data source connections
- Email distribution, automated reporting, or scheduled exports
- Advanced charting features (drill-down interactivity, 3D visualization, animations)
- Embedded components in AgentSquad.Runner (standalone for MVP; Q2+ integration potential)
- Enterprise security controls (encryption, audit logging, compliance frameworks)
- Custom UI builder or drag-and-drop dashboard customization

## Non-Functional Requirements

### Performance
- Dashboard page load time: <2 seconds (cold start)
- Data reload on file change: <500ms latency
- Chart rendering: <1 second from data load to display
- JSON parsing overhead: <50ms for typical <50KB files
- Memory footprint: Blazor Server app <150MB RAM at idle
- No database queries; all data loaded into memory at startup

### Scalability
- Support up to 50 milestone entries per project
- Support up to 100+ metric items (shipped/in-progress/carryover combined)
- JSON file size limit: <50KB for optimal performance
- Single-user or small-team usage (<5 concurrent browser sessions)
- No concurrent multi-user data editing (sequential file updates only)
- No external load balancing or scaling requirements

### Reliability
- Application recovers from malformed JSON without crashing (fallback defaults)
- FileSystemWatcher monitoring with polling fallback for network shares (UNC paths)
- File lock retry logic: 3 attempts with 100ms backoff for concurrent writes
- Graceful error handling for missing data.json (loads with empty/default data)
- Browser WebSocket reconnection handled natively by Blazor Server
- Application restart not required for data updates (event-driven refresh)

### Security
- No authentication or authorization layer required (internal-only tool assumption)
- File-level ACLs applied if data.json stored on network share (Windows NTFS)
- No sensitive personal data, credentials, or proprietary algorithms in JSON
- No SQL injection risks (JSON parsing only, no database queries)
- No cross-site scripting (XSS) vulnerabilities (server-side rendering)
- data.json excluded from Git version control via .gitignore; example data.json.template provided

### Compatibility
- Target environment: Windows 10/11 desktop
- Supported browsers: Chrome 90+, Edge 90+, Firefox 88+ (modern standards)
- .NET 8 SDK required for development and deployment
- No legacy browser support (IE11 not supported)
- Kestrel self-hosted or IIS deployment both supported
- No macOS or Linux deployment support (Windows-only FileSystemWatcher dependency)

## Success Metrics

- ✓ Standalone Blazor Server app created with `dotnet new blazorserver` and compiles without errors
- ✓ data.json loads and parses successfully on application startup
- ✓ Metric cards display with Bootstrap grid and responsive layout (verified at 1920x1080 and 1366x768)
- ✓ MilestoneTimeline component renders with ChartJs date axis and milestone markers
- ✓ FileSystemWatcher detects data.json file changes; UI refreshes within 500ms
- ✓ Print CSS produces clean screenshot output with no navigation clutter
- ✓ Example project data (fictional project) demonstrates all dashboard features
- ✓ Zero external API calls; fully local execution verified in network-disconnected environment
- ✓ MVP deployment complete within 3 weeks of project start
- ✓ No console errors or browser developer tool warnings during normal operation
- ✓ StatusSummary component displays health indicator (On Track, At Risk, Off Track) correctly
- ✓ DataService properly handles file lock contention during rapid data.json updates

## Constraints & Assumptions

### Technical Constraints
- Standalone application, not embedded in AgentSquad.Runner for MVP phase
- Local file-based data source only; no external database or data warehouse
- Client-side ChartJs rendering (no server-side chart image generation)
- Windows-only deployment; relies on System.IO.FileSystemWatcher (not cross-platform)
- Single data.json file as source of truth (no distributed data sync)
- Bootstrap 5.3.x CSS framework (responsive grid limited to desktop viewport widths)
- Maximum JSON file size: 50KB for acceptable performance
- No JavaScript interop needed (pure Blazor Server components)

### Timeline Assumptions
- Project delivery within 3 weeks (MVP scope only)
- Week 1: Blazor scaffold, DataService implementation, static layout
- Week 2: ChartJs integration, metric cards, Bootstrap grid
- Week 3: Print CSS refinement, screenshot testing, documentation
- AgentSquad.Runner integration deferred to Q2 (post-MVP)

### Dependency Assumptions
- .NET 8 SDK available on developer machine and deployment target
- Visual Studio 2022 Community (free) or VS Code with OmniSharp available
- Windows 10/11 environment with modern browser (Chrome, Edge, or Firefox)
- Administrator or developer role on Windows machine (no enterprise restrictions on file system access)
- data.json manually edited by project manager (no concurrent editing tools required)
- No active directory integration, SSO, or centralized identity management required

### Stakeholder Assumptions
- Executives have access to modern Windows desktops with current browsers
- Screenshots will be manually captured for PowerPoint decks (no automated distribution workflow)
- data.json updated on a weekly or manual basis (not real-time team updates)
- Dashboard displays current project snapshot only (not historical trends or forecasts)
- No requirement for mobile/tablet access (desktop PowerPoint viewing context only)
- Milestones and metrics are project-specific and relatively static per reporting cycle

### Open Questions for Stakeholder
- What is the expected data refresh frequency? (daily, weekly, manual on-demand?)
- Should dashboard archive historical project data for comparison, or always display current state only?
- Will data.json be deployed on local machine only, or on a network share (UNC path)?
- Is AgentSquad.Runner integration planned post-MVP, or is standalone deployment permanent?
- Are there other executives or stakeholders who need simultaneous dashboard access beyond single-user?
- Will export functionality expand to include PDF generation or email distribution in future phases?