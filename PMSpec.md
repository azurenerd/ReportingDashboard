# PM Specification: My Project

## Executive Summary

This project delivers a simple, single-page Executive Reporting Dashboard built with Blazor Server (.NET 8) that visualizes project milestones, progress, and status for executive stakeholders. The dashboard reads data from a JSON file (data.json), enables real-time auto-refresh via FileSystemWatcher, and produces clean, PowerPoint-ready screenshots. Deployed as a self-contained .exe requiring zero IT involvement, this tool reduces manual status reporting overhead by allowing executives to edit metrics directly in their preferred text editors.

## Business Goals

1. **Simplify executive visibility** - Create a single-page reporting tool that displays project milestones, progress, and status at a glance without requiring technical knowledge
2. **Enable fast PowerPoint deck creation** - Support native browser screenshots for executive presentations without manual redesign or design rework
3. **Minimize friction for non-technical executives** - Deliver one-click installation (.exe) with no IT involvement, .NET runtime installation, or authentication requirements
4. **Reduce manual status reporting overhead** - Implement JSON-driven data model allowing executives to edit metrics directly in VS Code or Notepad without code deployment

## User Stories & Acceptance Criteria

### US-001: View Project Milestones & Timeline
**As a** Project Executive  
**I want to** see a visual timeline of project milestones with target dates and completion status  
**So that** I can track progress toward key deliverables at a glance

- [ ] Milestone timeline displays as horizontal bar chart (Chart.js) with 3+ milestones visible without scrolling
- [ ] Each milestone shows: name, target date, current status (on-track/at-risk/delayed/completed), progress percentage
- [ ] Chart renders cleanly in print preview and screenshots at 1080p and 1440p resolutions
- [ ] Color-coded status: green (on-track), yellow (at-risk), red (delayed), gray (completed)

### US-002: View Project Status Snapshot
**As a** Project Executive  
**I want to** see what was shipped, what's in progress, and what was carried over from last period  
**So that** I understand current delivery status and team momentum

- [ ] Three-column layout displays: Shipped | In Progress | Carried Over items
- [ ] Each column renders as Bootstrap cards with item names and descriptions
- [ ] Layout remains readable without horizontal scrolling on 1080p+ displays
- [ ] Print preview renders all items without unwanted page breaks between columns

### US-003: Edit Project Data Without Developer Help
**As a** Project Executive  
**I want to** edit milestones and status items directly in data.json without developer intervention  
**So that** I can maintain current metrics without IT dependencies

- [ ] JSON schema is simple and documented; no complex nesting or required dependencies
- [ ] JSON file validates on app startup; validation errors display in red banner at top
- [ ] Schema supports: project name, reporting period, 5+ milestones, 10+ status items
- [ ] File edits trigger app refresh within 1 second (FileSystemWatcher + 500ms debounce)

### US-004: Auto-Refresh on Data Changes
**As a** Project Executive  
**I want to** see the dashboard update automatically when I edit data.json  
**So that** I can iterate quickly on metrics without restarting the app

- [ ] File watcher monitors data.json for changes (Last-Write event triggers)
- [ ] UI refreshes within 1 second of file save
- [ ] "Data refreshed at HH:mm:ss" timestamp displays below dashboard header
- [ ] Multiple rapid edits (3-4 within 1 second) debounce correctly; no duplicate refreshes

### US-005: Print/Screenshot for PowerPoint Decks
**As a** Project Executive  
**I want to** print or screenshot the dashboard for insertion into PowerPoint slides  
**So that** I can create executive presentations without manual redesign

- [ ] Native browser print (Ctrl+P) produces clean, professional output; no overlapping text or elements
- [ ] Print CSS hides navigation, footer, and refresh indicator
- [ ] Milestone timeline and status boxes remain on-page without unwanted breaks; all text readable
- [ ] Color scheme prints correctly; chart rendering exact in print preview

### US-006: Install & Launch With Single Click
**As a** Project Executive  
**I want to** download a single .exe file, double-click it, and immediately see the dashboard  
**So that** I don't need IT support or .NET installation

- [ ] Self-contained .exe bundles .NET 8 runtime; zero external dependencies
- [ ] Startup launches default browser to http://localhost:5000 within 5 seconds
- [ ] Dashboard renders and becomes interactive within 3 seconds of app launch
- [ ] File size <150MB (acceptable for intranet distribution over typical networks)

### US-007: View Project Metadata & KPIs
**As a** Project Executive  
**I want to** see project name, reporting period, and key performance indicators (on-time delivery %, team capacity %)  
**So that** I have context for milestone and status information

- [ ] Header section displays project name and reporting period prominently
- [ ] KPI metrics display in responsive grid format (readable at 1080p/1440p)
- [ ] KPI values update automatically when data.json is edited
- [ ] Layout matches screenshot-friendly design for PowerPoint insertion

## Scope

### In Scope

- Single-page Blazor Server application (.NET 8.0+)
- JSON data file (data.json) with flat, CEO-editable schema
- Bootstrap 5.3 styling via CDN (no build pipeline required)
- Chart.js 4.4.x milestone timeline (horizontal bar chart via CDN)
- FileSystemWatcher for real-time file monitoring with 500ms debounce
- Three-column status snapshot layout (Shipped | In Progress | Carried Over)
- Print CSS for screenshot quality at 1080p and 1440p resolutions
- Self-contained win-x64 .exe deployment (single-file, no .NET runtime install required)
- Unit + component tests (Tier 1 & 2: 80% data layer coverage, 70% component coverage)
- README.md with setup instructions, data schema documentation, print/screenshot workflow
- Sample data.json with realistic fictional project (3+ milestones, 2-3 items per status column)
- Error handling for malformed JSON (red banner with helpful message)
- Data refresh timestamp indicator ("Data refreshed at HH:mm:ss")
- Status color coding (on-track = green, at-risk = yellow, delayed = red, completed = gray)

### Out of Scope

- Authentication, user accounts, or role-based access control
- Multi-user sync, cloud backup, or network file sharing
- PDF export functionality or wkhtmltopdf integration
- MSI installer packaging (Wix Toolset) for Group Policy deployment
- Visual regression testing (Playwright automation)
- Database (SQLite or otherwise) - data remains JSON-only in MVP
- Enterprise security (encryption at rest, encryption in transit, audit logs)
- macOS/Linux deployment (win-x64 only in MVP)
- Pagination or lazy-loading (MVP assumes <100 milestones/status items)
- Persistent data versioning, Git integration, or backup automation
- Cloud infrastructure (Azure, AWS, or other cloud providers)
- Load testing, stress testing, or multi-user concurrency
- Mobile responsiveness or touch-optimized UI
- Dark mode, theme customization, or white-label support
- API integrations (Jira, Azure DevOps, Slack, etc.)
- Scheduled reporting, email notifications, or data exports
- Analytics, usage tracking, or telemetry collection

## Non-Functional Requirements

### Performance
- **App startup (cold):** <5 seconds from .exe double-click to browser window open
- **App startup (warm):** <2 seconds from .exe launch to interactive dashboard
- **JSON load & parse:** <500ms for data.json files up to 50MB
- **File watcher debounce:** 500ms (prevents cascade updates from rapid file writes)
- **UI refresh latency:** ≤1 second from file change detection to render completion
- **Memory footprint:** ~150MB (Blazor Server + .NET 8 runtime combined)
- **CPU idle:** <1% when no data changes occurring
- **Browser responsiveness:** UI interactive within 3 seconds of startup

### Reliability
- **Data integrity:** File writes atomic; app does not corrupt or lose data.json contents
- **Error handling:** Malformed JSON displays user-friendly error message (not blank screen)
- **Graceful degradation:** Unhandled exceptions logged to browser console; app remains responsive
- **Uptime:** Localhost app available 24/7 with no network dependencies
- **Crash recovery:** No data loss on app restart; data.json retains all previous state
- **File watcher stability:** FileSystemWatcher debounce prevents race conditions on rapid file writes

### Usability
- **Setup time:** Non-technical executive completes download, launch, and first data.json edit within 5 minutes
- **Learning curve:** Zero onboarding required; JSON schema self-evident from sample data and documentation
- **Accessibility:** WCAG 2.1 AA color contrast for all text; readable at all tested resolutions
- **Error clarity:** Validation error messages provide specific guidance (e.g., "Invalid status value: must be one of on-track, at-risk, delayed, completed")
- **Navigation simplicity:** Single-page layout; no multi-step workflows or complex menus

### Portability
- **Target OS:** Windows 10+ (64-bit, win-x64 architecture)
- **Browser support:** Chrome, Edge, Firefox (via embedded Kestrel server)
- **Print quality:** Clean, readable output at 96dpi, 150dpi, and 300dpi print resolutions
- **Screenshot quality:** Consistent rendering at 1080p (1920×1080) and 1440p (2560×1440) viewport sizes
- **Offline capability:** App works fully offline (localhost only; no cloud or network dependencies)

### Maintainability
- **Code structure:** Clear separation of concerns (Models/, Services/, Components/, Pages/)
- **Configuration:** data.json path, port, and project settings configurable via appsettings.json
- **Logging:** Info/Warning/Error level messages logged to browser console (and optionally to file)
- **Testing coverage:** Minimum 70% code coverage; test-driven development for data validation layer
- **Documentation:** Inline code comments explaining FileSystemWatcher debounce, cascading parameters, and data flow

## Success Metrics

### Functional Delivery
- ✓ Dashboard renders all required components: Project Metadata, Milestone Timeline, Status Snapshot, Refresh Indicator
- ✓ FileSystemWatcher successfully monitors data.json; UI updates <1 second after file save
- ✓ JSON schema documented and validated; sample data.json provided with realistic fictional project
- ✓ Print preview (Ctrl+P) renders cleanly at 1080p and 1440p without text overflow or unwanted page breaks
- ✓ Screenshot captures suitable for PowerPoint insertion (no artifacts, readable text, professional appearance)

### Deployment & Distribution
- ✓ Self-contained .exe is <150MB; launches successfully with no IT intervention
- ✓ Embedded Kestrel opens default browser to http://localhost:5000 automatically on startup
- ✓ Verified user workflow: Download .exe → Double-click → Edit data.json → App auto-refreshes
- ✓ Tested startup latency: cold start <5s, warm start <2s, UI interactive within 3s

### Quality & Testing
- ✓ Unit tests pass: JSON validation, POCO deserialization, status enum validation (80% data layer coverage)
- ✓ Component tests pass: MilestoneTimeline, StatusSnapshot, ProjectMetadata rendering (70% component coverage)
- ✓ Manual testing: Print preview clean, screenshot visually suitable for PowerPoint decks
- ✓ Error handling verified: Malformed JSON displays helpful error message instead of blank screen
- ✓ File watcher stability confirmed: Multiple rapid edits (3-4 within 1 second) debounce without duplicate refreshes

### Documentation
- ✓ README.md documents: setup instructions, data schema reference, print/screenshot workflow, system requirements
- ✓ Sample data.json includes 3+ milestones with realistic details and 2-3 status items per column
- ✓ Inline code comments explain FileSystemWatcher debounce mechanism, cascading parameters, JSON validation flow
- ✓ Data schema documentation specifies required vs. optional fields, valid enum values, and example JSON structure

### Executive Validation
- ✓ Executive stakeholder reviews dashboard mock-up; confirms visual clarity and information hierarchy
- ✓ Screenshot inserted into PowerPoint deck; confirmed suitable for executive presentation without modification
- ✓ Non-technical executive successfully edits data.json (using provided sample) and observes auto-refresh within 1 second

## Constraints & Assumptions

### Technical Constraints
- **Single-machine, localhost-only architecture:** No network routing, no cloud services, no multi-user synchronization
- **Local file system only:** data.json must reside in local directory on same machine; no remote file access or network drive support
- **Windows-only (MVP):** win-x64 deployment target; macOS and Linux support deferred to Phase 2
- **Simple JSON schema:** No relational queries, no transactions, no built-in versioning or rollback
- **No authentication layer:** App assumes single user or trusted environment with physical machine access controls
- **No enterprise security:** No encryption at rest, no encryption in transit (localhost only), no audit logging
- **CDN dependency:** Bootstrap 5.3 and Chart.js loaded from public CDNs (requires internet access for initial load)
- **Single-file JSON limitation:** Data.json cannot exceed ~50MB without performance degradation in deserialization and DOM rendering

### Business Constraints
- **MVP timeline:** 4-6 weeks for single developer (Week 1: setup/schema, Week 2: services/watcher, Week 3: components, Week 4: styling, Week 5: tests, Week 6: build/QA)
- **No external service dependencies:** All functionality must be self-contained or built-in to .NET 8 (System.Text.Json, FileSystemWatcher)
- **Deployment target:** Non-technical executives on Windows 10+ machines; no IT deployment pipeline
- **Screenshot-first design:** All UI decisions prioritized for clean, professional PowerPoint screenshots at 1080p and 1440p

### Assumptions

#### User Assumptions
- Executives have Windows 10+ machines available and can download/run executable files
- Executives are comfortable editing JSON in VS Code, Notepad, or similar text editor
- Executives understand basic project terminology (milestones, status, progress, KPIs)
- No regulatory or compliance requirements (PCI-DSS, HIPAA) apply to project metrics data
- Non-executives will not have access to or need to edit data.json

#### Technical Assumptions
- .NET 8 SDK is available on development machine for building and testing
- C# developers have basic async/await proficiency; Blazor Server experience not required (1-2 week learning curve acceptable)
- System.Text.Json built-in library is sufficient for JSON validation (no external schema validation library needed)
- FileSystemWatcher behaves reliably on target OS (Windows NTFS); no network drive or cloud file syncing scenarios
- Bootstrap 5.3 and Chart.js CDNs are accessible (no offline-first requirement; assumes corporate network allows CDN access)
- Browser print preview (Ctrl+P) is sufficient for screenshot creation; no native export button required

#### Data Assumptions
- Milestones and status items will not exceed 100 total items (no pagination required)
- data.json file size will stay <50MB (single-year project history or less)
- Data edits are infrequent enough that 500ms debounce is responsive for user feedback
- Print/screenshot uses native browser tools exclusively; no PDF generation or advanced export tooling needed
- All executives use Chrome, Edge, or Firefox browsers (no legacy IE support required)

#### Infrastructure Assumptions
- IT will not restrict localhost port access on corporate machines
- IT will not block access to Bootstrap and Chart.js CDNs (jsDelivr or equivalent)
- Machines have 1GB+ RAM available (Blazor Server + .NET 8 runtime ~150MB)
- Network latency for CDN access does not exceed typical corporate firewall speeds (CDN resources cached after first load)
- data.json location is writable by executives running the app

---

**Document Version:** 1.0  
**Last Updated:** 2026-04-09  
**Status:** Ready for Development