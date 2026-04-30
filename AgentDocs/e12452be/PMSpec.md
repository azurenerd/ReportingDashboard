# PM Specification: Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, shipped deliverables, in-progress work, carryover items, and blockers in a clean, screenshot-friendly format optimized for 1920×1080 PowerPoint slides. The application loads all display data from a checked-in JSON configuration file, requiring zero authentication, zero database, and zero cloud infrastructure—making it trivially simple to update and run locally.

## Business Goals

1. **Provide executives with instant project visibility** — A single-page view that communicates project health, milestone progress, and delivery status at a glance without requiring login or navigation.
2. **Eliminate manual slide creation** — Enable program managers to take a browser screenshot directly into PowerPoint decks, replacing hours of manual chart-building with a live, data-driven view.
3. **Enable non-developer data updates** — Any team member can update the dashboard by editing a single JSON file, with no code changes or deployments required.
4. **Standardize project reporting** — Establish a consistent visual format for communicating project status across the organization, with color-coded categories (shipped, in-progress, carryover, blockers).
5. **Minimize operational overhead** — Zero infrastructure cost, zero authentication complexity, zero external dependencies. Run locally with `dotnet run`.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** program manager, **I want** to see the project title, organizational context, current month, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are viewing.

*Visual Reference: Header section (`.hdr`) from `OriginalDesignConcept.html`*

**Acceptance Criteria:**
- [ ] Dashboard displays project title in bold 24px font
- [ ] Subtitle shows organization path and current month/year
- [ ] ADO Backlog link is rendered as a clickable hyperlink (color `#0078D4`)
- [ ] Legend icons appear on the right side of the header: PoC Milestone (amber diamond), Production Release (green diamond), Checkpoint (gray circle), Now indicator (red vertical line)
- [ ] Header has a bottom border of `1px solid #E0E0E0`

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major milestone streams with diamonds for PoC and production releases, circles for checkpoints, and a "NOW" indicator, **so that** I can understand project trajectory and upcoming deadlines at a glance.

*Visual Reference: Timeline section (`.tl-area` + SVG) from `OriginalDesignConcept.html`*

**Acceptance Criteria:**
- [ ] Timeline renders 1-5 horizontal milestone streams, each with a unique color and label
- [ ] Left sidebar (230px wide) displays milestone stream IDs and labels
- [ ] SVG area renders month grid lines with month abbreviation labels
- [ ] PoC milestones render as amber (`#F4B400`) diamond shapes with drop shadow
- [ ] Production releases render as green (`#34A853`) diamond shapes with drop shadow
- [ ] Checkpoints render as circles with white fill and colored stroke
- [ ] A red dashed vertical "NOW" line (`#EA4335`) indicates the current date position
- [ ] Date labels appear above or below each milestone marker
- [ ] Timeline section has a fixed height of 196px with `#FAFAFA` background

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked for each month, **so that** I can quickly assess execution health and identify problem areas.

*Visual Reference: Heatmap section (`.hm-wrap` + `.hm-grid`) from `OriginalDesignConcept.html`*

**Acceptance Criteria:**
- [ ] Heatmap renders as a CSS Grid with a "Status" column header and N month columns
- [ ] Four category rows display: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each cell lists work items as bullet points with a colored dot indicator matching the row category
- [ ] The current month column is visually highlighted with a distinct background color and header styling (`#FFF0D0` header, `#C07700` text)
- [ ] Empty cells display a dash character in muted color
- [ ] Grid fills remaining vertical space below the timeline
- [ ] Section title "Monthly Execution Heatmap" appears above the grid in uppercase, 14px, `#888`

### US-4: Load Dashboard Data from Configuration File

**As a** program manager, **I want** the dashboard to load all display data from a single JSON configuration file checked into the repository, **so that** I can update project status by editing one file without touching code.

**Acceptance Criteria:**
- [ ] Dashboard reads from `Data/dashboard-data.json` at application startup
- [ ] JSON file supports: title, subtitle, backlog URL, current date, timeline date range, milestone streams, heatmap months, current month, and heatmap row data
- [ ] Application starts successfully with the sample data file
- [ ] Modifying the JSON file and restarting the app reflects changes in the rendered dashboard
- [ ] Invalid JSON produces a clear error message in console output (not a crash with stack trace)
- [ ] JSON supports comments via `//` syntax for inline documentation

### US-5: Screenshot-Optimized Rendering

**As a** program manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrolling required, **so that** I can take a full-page browser screenshot and paste it directly into a PowerPoint slide.

**Acceptance Criteria:**
- [ ] Page renders at fixed 1920×1080 dimensions
- [ ] No scrollbars appear when browser window is set to 1920×1080
- [ ] All content is visible without scrolling
- [ ] Font rendering uses Segoe UI (system font on Windows)
- [ ] No loading spinners, skeleton screens, or progressive rendering artifacts appear in screenshots
- [ ] Page is fully rendered within 2 seconds of navigation

### US-6: Run Dashboard Locally with Zero Configuration

**As a** developer, **I want** to run the dashboard with a single `dotnet run` command and no external dependencies, **so that** I can quickly generate screenshots without setting up databases, cloud services, or authentication.

**Acceptance Criteria:**
- [ ] `dotnet run` starts the application successfully
- [ ] Dashboard is accessible at `http://localhost:5000` or `https://localhost:5001`
- [ ] No database connection required
- [ ] No authentication challenge on page load
- [ ] No external API calls required for rendering
- [ ] Application works offline (after initial build/restore)

## Visual Design Specification

**Reference Files:**
- `OriginalDesignConcept.html` — Canonical HTML/CSS design template
- `AgentDocs/e12452be/ReportingDashboardDesign.png` — Visual design mockup

### Overall Layout

The page uses a fixed `1920×1080px` viewport with `overflow: hidden`. The layout is a vertical flex column with three main sections stacked top-to-bottom:

1. **Header** (~50px) — Fixed height, flex row
2. **Timeline** (196px) — Fixed height, flex row with sidebar + SVG
3. **Heatmap** (remaining space) — Flex-grow, CSS Grid

### Section 1: Header (`.hdr`)

- **Layout:** Flexbox row, `justify-content: space-between`, `align-items: center`
- **Padding:** `12px 44px 10px`
- **Border:** Bottom `1px solid #E0E0E0`
- **Left side:**
  - Title: `<h1>` at `24px`, `font-weight: 700`, color `#111`
  - Inline link: color `#0078D4`, no text-decoration
  - Subtitle: `12px`, color `#888`, `margin-top: 2px`
- **Right side (Legend):**
  - Flex row with `gap: 22px`
  - Each legend item: flex row with `gap: 6px`, `font-size: 12px`
  - PoC Milestone: `12×12px` amber (`#F4B400`) square rotated 45°
  - Production Release: `12×12px` green (`#34A853`) square rotated 45°
  - Checkpoint: `8×8px` gray (`#999`) circle
  - Now indicator: `2×14px` red (`#EA4335`) vertical bar

### Section 2: Timeline (`.tl-area`)

- **Layout:** Flexbox row, `align-items: stretch`
- **Dimensions:** Height `196px`, padding `6px 44px 0`
- **Background:** `#FAFAFA`
- **Border:** Bottom `2px solid #E8E8E8`

**Left Sidebar (230px):**
- Fixed width `230px`, flex column, `justify-content: space-around`
- Padding `16px 12px 16px 0`, right border `1px solid #E0E0E0`
- Each stream label: `12px`, `font-weight: 600`, `line-height: 1.4`
- Stream ID in stream color, description in `color: #444`, `font-weight: 400`

**SVG Timeline Area (`.tl-svg-box`):**
- `flex: 1`, padding `12px left`, `6px top`
- SVG dimensions: full width × 185px height
- **Month grid lines:** Vertical lines at equal intervals, `stroke: #bbb`, `stroke-opacity: 0.4`
- **Month labels:** `font-size: 11`, `font-weight: 600`, `fill: #666`, positioned at top of each column
- **NOW line:** Dashed vertical line, `stroke: #EA4335`, `stroke-width: 2`, `stroke-dasharray: 5,3`; label "NOW" in `10px bold red`
- **Track lines:** Horizontal lines per stream, `stroke-width: 3`, color per stream
- **Checkpoint circles:** `r=5-7`, `fill: white`, `stroke` matching stream color, `stroke-width: 2.5`
- **Small checkpoints:** `r=4`, `fill: #999` (no stroke)
- **PoC diamonds:** Polygon rotated 45°, `fill: #F4B400`, with drop shadow filter
- **Production diamonds:** Polygon rotated 45°, `fill: #34A853`, with drop shadow filter
- **Milestone labels:** `font-size: 10`, `fill: #666`, `text-anchor: middle`, positioned above/below markers

### Section 3: Heatmap (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1`, `min-height: 0`
- **Padding:** `10px 44px 10px`

**Section Title (`.hm-title`):**
- `font-size: 14px`, `font-weight: 700`, `color: #888`
- `letter-spacing: 0.5px`, `text-transform: uppercase`, `margin-bottom: 8px`

**Grid (`.hm-grid`):**
- `display: grid`
- `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months
- `grid-template-rows: 36px repeat(4, 1fr)`
- `border: 1px solid #E0E0E0`

**Corner cell (`.hm-corner`):**
- `background: #F5F5F5`, `font-size: 11px`, `font-weight: 700`, `color: #999`
- `text-transform: uppercase`, centered content
- `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`

**Column headers (`.hm-col-hdr`):**
- `font-size: 16px`, `font-weight: 700`, `background: #F5F5F5`, centered
- `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`
- Current month override: `background: #FFF0D0`, `color: #C07700`

**Row headers (`.hm-row-hdr`):**
- `font-size: 11px`, `font-weight: 700`, `text-transform: uppercase`, `letter-spacing: 0.7px`
- `padding: 0 12px`, `border-right: 2px solid #CCC`, `border-bottom: 1px solid #E0E0E0`
- Per-category colors:
  - Shipped: `color: #1B7A28`, `background: #E8F5E9`
  - In Progress: `color: #1565C0`, `background: #E3F2FD`
  - Carryover: `color: #B45309`, `background: #FFF8E1`
  - Blockers: `color: #991B1B`, `background: #FEF2F2`

**Data cells (`.hm-cell`):**
- `padding: 8px 12px`, `border-right: 1px solid #E0E0E0`, `border-bottom: 1px solid #E0E0E0`
- `overflow: hidden`
- Per-category background colors (default / current month):
  - Shipped: `#F0FBF0` / `#D8F2DA`
  - In Progress: `#EEF4FE` / `#DAE8FB`
  - Carryover: `#FFFDE7` / `#FFF0B0`
  - Blockers: `#FFF5F5` / `#FFE4E4`

**Item bullets (`.it`):**
- `font-size: 12px`, `color: #333`, `padding: 2px 0 2px 12px`, `line-height: 1.35`
- `::before` pseudo-element: `6×6px` circle, absolutely positioned left, colored per category:
  - Shipped: `#34A853`
  - In Progress: `#0078D4`
  - Carryover: `#F4B400`
  - Blockers: `#EA4335`

### Typography

| Element | Size | Weight | Color |
|---------|------|--------|-------|
| Page title | 24px | 700 | `#111` |
| Subtitle | 12px | 400 | `#888` |
| Legend items | 12px | 400 | `#111` |
| Timeline month labels | 11px | 600 | `#666` |
| Timeline milestone labels | 10px | 400 | `#666` |
| Timeline stream labels | 12px | 600 | Stream color |
| Heatmap section title | 14px | 700 | `#888` |
| Column headers | 16px | 700 | `#111` (or `#C07700` for current) |
| Row headers | 11px | 700 | Category color |
| Cell items | 12px | 400 | `#333` |

### Font Stack
`'Segoe UI', Arial, sans-serif` — Segoe UI is pre-installed on Windows (target platform).

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The dashboard renders fully within 2 seconds showing the header, timeline, and heatmap populated from the JSON configuration file. No loading spinner or skeleton screen appears. The page fits exactly in a 1920×1080 viewport with no scrollbars.

**Scenario 2: User Views Header and Identifies Project Context**
User sees the project title ("Privacy Automation Release Roadmap") in bold at the top-left, with a clickable "→ ADO Backlog" link. Below is the subtitle showing org path and month. On the right, a legend explains the four visual markers used in the timeline. The user instantly knows which project, team, and time period this dashboard represents.

**Scenario 3: User Reads the Milestone Timeline**
User looks at the timeline section and sees three horizontal colored lines representing milestone streams (M1, M2, M3). Each stream has labeled markers: diamonds for PoC and production milestones, circles for checkpoints. A red dashed "NOW" line shows the current date position. The user can trace each stream left-to-right to understand past progress and upcoming deadlines.

**Scenario 4: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser navigates to the Azure DevOps backlog URL specified in the JSON configuration file (opens in same tab or new tab per browser default).

**Scenario 5: User Scans the Heatmap for Current Month Status**
User looks at the heatmap and immediately notices the current month column (April) is visually highlighted with a warm amber header background (`#FFF0D0`). The user scans down the April column to see what shipped (green), what's in progress (blue), what carried over (amber), and what's blocked (red).

**Scenario 6: User Identifies Blockers Requiring Attention**
User scans the bottom row (Blockers, red-tinted) and sees items like "Vendor API downtime" and "Compliance team bandwidth" in the current month. The red color and category header draw the eye to problems requiring executive intervention.

**Scenario 7: User Takes a Screenshot for PowerPoint**
User sets their browser window to 1920×1080, navigates to the dashboard, waits for full render, and uses Windows screenshot tools (Win+Shift+S or Print Screen). The captured image is clean, complete, and ready to paste directly into a PowerPoint slide with no cropping needed.

**Scenario 8: Data-Driven Rendering — Variable Month Columns**
The JSON configuration file specifies `heatmapMonths: ["Mar", "Apr", "May", "Jun"]`. The heatmap grid dynamically renders 4 columns. If changed to 6 months, the grid adjusts column count accordingly without code changes.

**Scenario 9: Data-Driven Rendering — Variable Milestone Streams**
The JSON file defines 2 milestone streams instead of 3. The timeline renders only 2 horizontal track lines with their respective markers. The left sidebar shows only 2 stream labels. Vertical spacing adjusts proportionally.

**Scenario 10: Empty State — No Items in a Cell**
A heatmap cell for a month/category combination has no items (empty array in JSON). The cell renders a single dash character (`-`) in muted color (`#AAA`) rather than appearing completely empty.

**Scenario 11: Error State — Invalid JSON Configuration**
The `dashboard-data.json` file contains a syntax error. On application startup, the console displays a clear error message identifying the JSON parse failure with file path and approximate error location. The application does not crash with an unhandled exception.

**Scenario 12: Error State — Missing Configuration File**
The `dashboard-data.json` file is deleted or missing. On startup, the console displays a clear error message stating the file was not found and providing the expected file path. The application logs the error gracefully.

**Scenario 13: Print/PDF via Browser**
User presses Ctrl+P in the browser. The `@media print` CSS rules ensure the page prints cleanly at full width without clipping, background colors are preserved, and no browser chrome or Blazor UI artifacts appear.

## Scope

### In Scope

- Single-page Blazor Server application rendering at 1920×1080
- Header component with title, subtitle, backlog link, and legend
- SVG-based milestone timeline with configurable streams, milestones, and NOW indicator
- CSS Grid heatmap with Shipped, In Progress, Carryover, and Blockers rows
- JSON configuration file (`Data/dashboard-data.json`) as the sole data source
- Sample configuration file with fictional project data
- Dynamic column count based on months defined in JSON
- Current month visual highlighting
- CSS custom properties for the full color palette
- `@media print` styles for clean browser printing
- README with setup and usage instructions
- Clear console error messages for JSON parse failures
- JSON Schema file for IDE validation support

### Out of Scope

- Authentication or authorization of any kind
- Azure DevOps API integration or automatic data population
- Database or persistent storage beyond the JSON file
- Real-time updates, WebSocket push, or auto-refresh
- Historical trend analysis or time-series charts
- Multi-tenant, multi-user, or role-based access
- Docker containerization or CI/CD pipeline
- Server-side PDF generation
- Responsive/mobile layout (fixed 1920×1080 only)
- Edit UI for the JSON configuration
- Multiple project views or routing (single dashboard instance)
- Tooltips, hover effects, or interactive drill-downs
- Animations or transitions
- Dark mode or theme switching
- Accessibility compliance (WCAG) beyond semantic HTML
- Automated testing (bUnit/xUnit are optional, not required for MVP)

## Non-Functional Requirements

### Performance
- **Page load time:** Full render in ≤ 2 seconds from navigation on localhost
- **JSON parsing:** Configuration file deserialized in < 100ms for files up to 50KB
- **No external network calls:** Zero API requests, CDN fetches, or font downloads required after initial page load

### Security
- **No authentication required** — This is a local development tool
- **No sensitive data** — Dashboard displays project status only, no PII or credentials
- **No HTTPS requirement** — HTTP on localhost is acceptable (HTTPS available via default Kestrel config)
- **Git-based access control** — Repository permissions govern who can view/edit data

### Scalability
- **Target users:** 1-5 concurrent viewers (local use or small team)
- **Data volume:** JSON config file supports up to 6 months × 4 categories × 10 items per cell without layout overflow
- **Not designed for:** >10 concurrent connections, public internet access, or high availability

### Reliability
- **Availability target:** Runs when developer starts it; no uptime SLA
- **Graceful degradation:** Application logs clear errors on invalid JSON rather than crashing
- **No data loss risk:** JSON file is checked into Git; no mutable server-side state

### Compatibility
- **Target browser:** Microsoft Edge (Chromium) on Windows 10/11
- **Secondary browsers:** Chrome, Firefox (render correctness not guaranteed for SVG text positioning)
- **Target OS:** Windows 10/11 (Segoe UI font dependency)
- **Runtime:** .NET 8.0 LTS

## Success Metrics

| Metric | Target | Measurement Method |
|--------|--------|--------------------|
| Visual fidelity | ≥95% match to `OriginalDesignConcept.html` reference | Side-by-side screenshot comparison |
| Page load time | ≤ 2 seconds on localhost | Manual timing with browser DevTools |
| Data update workflow | < 5 minutes to update and screenshot | Timed walkthrough: edit JSON → restart → screenshot |
| Zero external dependencies | 0 NuGet packages beyond .NET 8 framework | `dotnet list package` output |
| Setup time for new developer | < 5 minutes from `git clone` to running dashboard | Timed walkthrough on clean machine with .NET 8 SDK |
| Screenshot quality | Directly usable in PowerPoint without editing | Paste screenshot at 1920×1080 into 16:9 slide |
| Configuration flexibility | Change project data without code modifications | Edit JSON, restart, verify render |

## Constraints & Assumptions

### Technical Constraints
- **Stack:** Blazor Server on .NET 8 (LTS) — mandated technology choice
- **Fixed viewport:** 1920×1080 pixels — optimized for screenshot capture, not responsive
- **No JavaScript dependencies:** All rendering via Blazor/Razor and inline SVG; no JS interop or charting libraries
- **Single project per instance:** One JSON file drives one dashboard; multi-project requires separate files/instances
- **Windows-first:** Segoe UI font and Edge browser are the primary targets
- **No hot-reload of data:** Application restart required after JSON edits (acceptable for weekly update cadence)

### Timeline Assumptions
- **Implementation effort:** 2-3 days for an experienced .NET developer
- **Phase 1 (Day 1):** Skeleton + static rendering with hardcoded data
- **Phase 2 (Day 1-2):** Timeline SVG generation
- **Phase 3 (Day 2):** JSON data loading integration
- **Phase 4 (Day 2-3):** Polish, screenshot optimization, README

### Dependency Assumptions
- Developer has .NET 8 SDK installed locally
- Developer has access to a Windows machine with Edge browser for screenshot capture
- Repository access provides implicit authorization to view/edit dashboard data
- Executives will receive screenshots (PNG in PowerPoint), not direct dashboard access
- Data updates happen weekly or bi-weekly aligned with executive review cadence
- The JSON configuration file will remain under 50KB (sufficient for 6 months of data)

### Design Assumptions
- The `OriginalDesignConcept.html` file is the canonical visual specification; all deviations require PM approval
- The heatmap always shows exactly 4 categories (Shipped, In Progress, Carryover, Blockers) — this is not configurable
- Month count is configurable (3-6 months) but category count is fixed at 4
- The timeline and heatmap may show different date ranges (timeline: 6 months, heatmap: 4 months) as independently configured in JSON