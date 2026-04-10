# PM Specification: My Project

## Executive Summary

My Project is a lightweight, single-page executive dashboard that consolidates project milestone tracking and progress reporting into a screenshot-optimized web application. Built with Blazor Server and powered by JSON configuration files, it enables executives to quickly assess project health, track shipped deliverables, monitor in-progress work, and identify carried-over items—all without authentication, database complexity, or enterprise infrastructure overhead. The dashboard is designed specifically for PowerPoint presentation screenshots and internal network deployment.

## Business Goals

1. **Simplify executive visibility** — Provide a unified view of project status without requiring access to complex project management tools
2. **Enable rapid reporting** — Support one-click screenshot capture for executive presentations and status meetings
3. **Reduce operational overhead** — Eliminate authentication, database, and cloud infrastructure requirements for a local-only reporting tool
4. **Standardize milestone communication** — Create a consistent visual format for communicating project timelines, risks, and progress to leadership
5. **Minimize time-to-deployment** — Deploy locally via copy-and-run model without complex CI/CD or infrastructure setup

## User Stories & Acceptance Criteria

### US-1: View Project Overview
**As an** executive sponsor  
**I want to** see project name, ID, and last updated timestamp at a glance  
**So that** I can verify the dashboard is current and understand which project is being reported

**Acceptance Criteria:**
- [ ] Dashboard displays project name and project ID prominently at top of page
- [ ] Last updated timestamp is visible with readable date/time format
- [ ] Timestamp updates within 2 seconds of `data.json` file change
- [ ] Page loads in <1 second on local machine or internal network
- [ ] Information persists across browser refresh without data loss

---

### US-2: Review Milestone Timeline
**As an** executive  
**I want to** see a visual timeline of major milestones with target dates and current status  
**So that** I understand the project delivery schedule and which milestones are on track, at risk, or delayed

**Acceptance Criteria:**
- [ ] Timeline displays as horizontal bar chart with milestone names and target dates
- [ ] Each milestone shows status: On Track, At Risk, Delayed, or Completed
- [ ] Completed milestones are visually distinct (e.g., checkmark, different color)
- [ ] Timeline renders without clipping or overflow at 1920×1080 resolution
- [ ] Timeline supports minimum 10 concurrent milestones without layout issues
- [ ] Date labels are readable and properly formatted (e.g., "Apr 15, 2024")
- [ ] Pixel-perfect rendering confirmed for PowerPoint screenshots

---

### US-3: Track Completed Items
**As a** project manager  
**I want to** see a categorized list of completed deliverables and work items  
**So that** I can communicate shipped value to executives and stakeholders

**Acceptance Criteria:**
- [ ] Status card displays total count of completed items
- [ ] Each item includes: title, description, owner name, and completion date
- [ ] Card uses green color coding for visual emphasis
- [ ] List displays 5–20 items per category without scrolling (scrollable if exceeds limit)
- [ ] Items are sorted by date (newest first)
- [ ] No limit on number of items that can be stored in data.json

---

### US-4: Track In-Progress Work
**As a** project manager  
**I want to** see active work items with ownership and progress descriptions  
**So that** I can communicate ongoing efforts and dependencies to the team and executives

**Acceptance Criteria:**
- [ ] Status card displays total count of in-progress items
- [ ] Each item includes: title, description (e.g., "60% complete"), owner name, and start date
- [ ] Card uses yellow/orange color coding
- [ ] Items sorted by start date (oldest first, indicating longer-running tasks)
- [ ] List displays without automatic scrolling (scrollable if needed)

---

### US-5: Track Carried-Over Items
**As a** project manager  
**I want to** see items that missed their original target and carried over to current/next period  
**So that** I can explain delays and slippage to executives

**Acceptance Criteria:**
- [ ] Status card displays total count of carried-over items
- [ ] Each item includes: title, description (e.g., "Delayed pending vendor approval"), owner name, and original date
- [ ] Card uses red color coding to indicate risk/slip
- [ ] Items sorted by original date (oldest first)
- [ ] Clear visual distinction from in-progress and completed items

---

### US-6: View Status Metrics Chart
**As an** executive  
**I want to** see a summary bar chart showing item counts across all status categories  
**So that** I can quickly assess overall project momentum and workload distribution

**Acceptance Criteria:**
- [ ] Horizontal bar chart displays three bars: Completed, In Progress, Carried Over
- [ ] Each bar is labeled with the status category and numeric count
- [ ] Bars use consistent color scheme (green/yellow/red) matching status cards
- [ ] Chart renders without animations or transitions (static for screenshot consistency)
- [ ] Chart is responsive and maintains aspect ratio at 1920×1080 and 1280×720 resolutions
- [ ] Chart legend or labels clearly identify each metric

---

### US-7: Auto-Refresh Dashboard on Data Changes
**As a** dashboard operator  
**I want the** page to automatically detect and reload when `data.json` is modified  
**So that** I don't need to manually refresh the browser or restart the application

**Acceptance Criteria:**
- [ ] Dashboard detects file changes to `data.json` within 2 seconds
- [ ] Page automatically re-renders with new data without manual browser refresh
- [ ] Multiple rapid file changes are handled without duplicate updates or flickering
- [ ] Implements file watcher with 30-second polling fallback for reliability
- [ ] No error messages displayed to user during normal file refresh
- [ ] State is retained if user is viewing the page during refresh

---

### US-8: Screenshot-Optimized Rendering
**As an** executive presenter  
**I want to** capture clean, professional screenshots of the dashboard for PowerPoint decks  
**So that** I can include current project status visuals in executive presentations

**Acceptance Criteria:**
- [ ] Dashboard renders cleanly without UI clutter, navigation bars, or distracting elements
- [ ] All text is legible at 1920×1080 resolution with standard browser zoom (100%)
- [ ] Color contrast meets WCAG AA standards for readability in presentations
- [ ] Page layout remains consistent across Chrome, Edge, and Firefox browsers
- [ ] Print-to-image capture produces pixel-perfect output without clipping
- [ ] Fonts render consistently across Windows and Linux machines (system font stack)
- [ ] No animations or auto-playing elements that might cause visual inconsistency in screenshots

---

## Scope

### In Scope
- Single-page Blazor Server web application (.NET 8)
- JSON-based configuration file (`data.json`) loading and deserialization
- Milestone timeline visualization (custom SVG Gantt-style chart)
- Status card components displaying completed, in-progress, and carried-over item counts
- Status metrics bar chart (ChartJS v4.1.0+)
- File system watcher + polling hybrid for automatic data refresh
- Bootstrap 5.3 CDN styling for responsive, professional layout
- Print-friendly CSS and viewport configuration for PowerPoint screenshots
- Sample `data.json` with fictional project example
- Comprehensive deployment and setup documentation
- Unit test coverage for DashboardDataService (xUnit + Moq)
- Data model validation (strongly-typed C# DTOs)
- Self-contained .NET 8 executable deployment option

### Out of Scope
- User authentication, authorization, or login system
- Multi-user concurrent editing or role-based access control
- Relational database persistence (JSON-only for MVP)
- Email notifications, alerts, or automated escalations
- REST API endpoints or external data integrations
- Real-time collaboration or multi-device synchronization
- Historical data versioning or changelog tracking
- Audit logging or compliance/regulatory reporting
- Internationalization (i18n) or multi-language support
- Mobile-optimized responsive design (desktop-first)
- Automated screenshot capture or reporting workflows
- Integration with external project management tools (Jira, Azure DevOps, etc.)
- Advanced charting (3D, drill-down, custom interactivity)

---

## Non-Functional Requirements

| Category | Requirement | Target |
|---|---|---|
| **Performance** | Page initial load time | <1 second on local machine |
| **Performance** | File refresh detection latency | <2 seconds |
| **Performance** | UI state re-render time | <500ms |
| **Availability** | Deployment model | Local-only; no cloud/SaaS dependencies |
| **Availability** | Concurrent viewers supported | 1–10 (single machine or internal network) |
| **Security** | Authentication requirement | None (assumes trusted internal network) |
| **Security** | Encryption at rest | Not required (local file, non-sensitive data) |
| **Security** | Encryption in transit | Not required (local-only, no network exposure) |
| **Security** | Data exposure risk | Minimal (project names and owner names only, no PII) |
| **Scalability** | Maximum status items per category | 50 items without performance degradation |
| **Scalability** | Maximum concurrent milestones | 10 milestones without layout issues |
| **Scalability** | Total JSON file size | <1MB (reasonable for local file) |
| **Reliability** | Malformed JSON handling | Gracefully retain previous data; log error |
| **Reliability** | File watcher edge cases | Implement polling fallback every 30 seconds |
| **Reliability** | Unhandled exceptions | Zero during normal operation; graceful error messages |
| **Reliability** | Data loss risk | Minimal (read-only application, no writes to data.json) |
| **Compatibility** | Browser support | Chrome, Edge, Firefox (latest versions) |
| **Compatibility** | Screen resolution support | 1920×1080 and 1280×720 (16:9 aspect ratio) |
| **Compatibility** | Viewport scaling | 100% zoom (standard browser default) |
| **Usability** | Setup complexity | Copy folder + run executable (< 5 minutes) |
| **Usability** | Training required | None (intuitive dashboard, self-explanatory) |
| **Usability** | Screenshot capture method | Browser print-to-image (single click) |

---

## Success Metrics

**Project is considered complete when:**

- ✅ Dashboard loads in <1 second on local machine (measured via browser DevTools)
- ✅ File change to `data.json` triggers page re-render within 2 seconds
- ✅ PowerPoint screenshots capture dashboard at 1920×1080 without text clipping or layout overflow
- ✅ All components render pixel-perfect across Chrome, Edge, and Firefox browsers
- ✅ Zero unhandled exceptions logged during 1-hour baseline testing session
- ✅ Deployment achieved in <5 minutes: copy published files + execute `AgentSquad.Reporting.exe`
- ✅ Sample `data.json` loads successfully and displays all test data without errors
- ✅ Unit test coverage for `DashboardDataService` reaches ≥80% (statement coverage)
- ✅ JSON schema validation prevents silent failures (malformed JSON logged, previous data retained)
- ✅ Print CSS confirmed working across Chrome and Firefox (test print-to-PDF)
- ✅ Documentation complete: README includes deployment steps, JSON schema, troubleshooting
- ✅ Dashboard displays all required elements: project name, milestones, status cards, metrics chart

---

## Constraints & Assumptions

### Technical Constraints

| Constraint | Rationale | Impact |
|---|---|---|
| **JSON-only data storage** | Simplifies deployment; no database installation | Limited to ~1000 status items before performance degrades; database migration deferred to future release |
| **Local-only deployment** | Eliminates cloud infrastructure requirements | No horizontal scaling; supports 1–10 concurrent viewers maximum |
| **Blazor Server** | Standard .NET 8 web framework | Requires .NET 8 runtime on deployment machine; slower than static HTML but simpler than SPA |
| **File watcher + polling** | Balances reliability with responsiveness | Introduces 0–30 second refresh latency; acceptable for executive dashboard (not real-time) |
| **No authentication layer** | Per project scope (local-only tool) | Assumes only trusted users on internal network can access machine |
| **Bootstrap CDN dependency** | Rapid prototyping without build complexity | Requires internet access for CSS download; consider self-hosting if restricted environment |
| **Custom SVG timeline** | Avoids JavaScript interop overhead for Blazor | Manual rendering; future complex visualizations may require D3.js or Plotly migration |

### Timeline Assumptions

| Assumption | Rationale |
|---|---|
| **Development: 4 weeks for MVP** | 1 developer, full-time; includes design, coding, testing, documentation |
| **Quick wins: +3–5 days optional** | Docker containerization, health check endpoint, HTML export (post-MVP enhancements) |
| **Production hardening: +2–3 days optional** | Logging, error handling, documentation refinement (if scope expands) |
| **No blocking dependencies** | Design files (OriginalDesignConcept.html, ReportingDashboardDesign.png) available at project start |
| **.NET 8 SDK pre-installed** | Deployment assumes Visual Studio 2022 or .NET 8 runtime available on target machine |

### Business & Operational Assumptions

| Assumption | Rationale |
|---|---|
| **Dashboard viewed on trusted internal network** | Justifies no encryption, authentication, or formal security controls |
| **Data updated manually via file edit** | Project manager edits `data.json` directly; no REST API or UI form required for MVP |
| **Executives familiar with web browsers** | No training or complex UI onboarding required; intuitive dashboard design |
| **Single project per deployment** | Each project gets its own dashboard instance; multi-project aggregation deferred to future |
| **Static timeline rendering prioritized** | Interactive Gantt chart features (drag-drop, drill-down) deferred; screenshots require pixel-perfect static layout |
| **PowerPoint screenshot-first design** | Layout optimized for 1920×1080 (16:9); usability secondary to visual presentation quality |
| **JSON schema stable at MVP** | Data model locked after initial release; backward-compatibility maintained for future updates |
| **No historical data retention required** | Dashboard shows current month only; previous months archived separately by project manager |

### Dependencies & Prerequisites

- .NET 8.0.0+ SDK installed on development and deployment machines
- Visual Studio 2022 or VS Code + C# Dev Kit for development
- NuGet package: `CurrieTechnologies.Razor.ChartJS` v4.1.0 or later
- Access to design reference files: `OriginalDesignConcept.html` and `C:/Pics/ReportingDashboardDesign.png`
- Existing `AgentSquad` solution repository in Git
- Modern web browser (Chrome, Edge, or Firefox) on viewing machines
- Internet access for Bootstrap 5.3 CDN (or local CSS copy for restricted networks)