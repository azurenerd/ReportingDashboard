# PM Specification: Executive Project Reporting Dashboard

**Document Version:** 1.0
**Date:** April 16, 2026
**Author:** Program Management
**Status:** Draft

---

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes a project's milestones, delivery status, and monthly execution progress. The dashboard reads all data from a local `data.json` configuration file and renders a pixel-exact, 1920×1080 view optimized for direct screenshot capture into PowerPoint decks for executive stakeholders. Built on .NET 8 Blazor Server with static server-side rendering, the solution requires zero authentication, zero external dependencies, and zero cloud infrastructure—just `dotnet run` and a browser.

---

## Business Goals

1. **Reduce executive reporting prep time by 75%** — Replace manual PowerPoint slide construction with an auto-rendered dashboard that can be screenshotted directly, eliminating per-slide layout work.
2. **Provide a single, consistent visual format for project status** — Standardize how milestones, shipped items, in-progress work, carryovers, and blockers are communicated to leadership.
3. **Enable rapid data updates without technical skills** — Allow any PM or lead to update project status by editing a single JSON file, with changes reflected immediately on browser refresh.
4. **Maintain screenshot-quality fidelity at 1920×1080** — Every render must produce a clean, professional visual suitable for embedding in executive presentations without post-processing.
5. **Keep operational cost at $0** — The dashboard runs entirely on the user's local machine with no cloud services, licenses, or hosting fees.

---

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** to see the project title, organizational context, current reporting period, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately understand which project and time period they are reviewing.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` element.

**Acceptance Criteria:**
- [ ] The header displays the project title in bold 24px font weight 700
- [ ] A clickable hyperlink to the ADO backlog appears inline with the title (styled in `#0078D4`)
- [ ] A subtitle line shows the organization, workstream, and current month (12px, `#888`)
- [ ] A legend bar on the right side of the header displays four marker types: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line)
- [ ] All header data is sourced from `data.json` — no hardcoded values

---

### US-2: View Milestone Timeline

**As an** executive reviewer, **I want** to see a horizontal timeline showing major project milestones across multiple workstreams, **so that** I can understand the project's delivery cadence, what has been achieved, and what is upcoming.

**Visual Reference:** Timeline area of `OriginalDesignConcept.html` — `.tl-area` element with inline SVG.

**Acceptance Criteria:**
- [ ] The timeline renders as an inline SVG (1560px wide × 185px tall) within a 196px-tall container
- [ ] A left-side panel (230px wide) lists each workstream/track with its code (e.g., "M1") and description
- [ ] Each track renders as a horizontal colored line spanning the full timeline width
- [ ] Month grid lines appear at calculated positions with month labels (Jan–Jun or as defined in `data.json`)
- [ ] Milestones render as the correct marker type: diamond for PoC (`#F4B400`) and Production (`#34A853`), open circle for Checkpoint, small filled circle for minor checkpoints
- [ ] Each milestone displays a date label positioned above or below the marker
- [ ] A dashed red vertical "NOW" line (`#EA4335`) appears at the current date position
- [ ] The timeline supports 1–5 tracks without layout overflow
- [ ] All milestone dates, labels, and types are sourced from `data.json`

---

### US-3: View Monthly Execution Heatmap

**As an** executive reviewer, **I want** to see a color-coded grid showing what was shipped, what is in progress, what carried over, and what is blocked — organized by month, **so that** I can assess execution health at a glance.

**Visual Reference:** Heatmap section of `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` elements.

**Acceptance Criteria:**
- [ ] The heatmap renders as a CSS Grid with a 160px row-header column and N equal-width month columns
- [ ] A header row shows month names, with the current month highlighted in gold (`#FFF0D0`, text `#C07700`)
- [ ] Four status rows render in order: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each row header displays the status category name with an icon, using the category's accent color
- [ ] Each cell lists work items as bullet-pointed text (12px, `#333`) with a colored dot prefix matching the row theme
- [ ] Current-month cells use a darker highlight background than non-current cells
- [ ] Empty cells for future months display a dash (`-`) in muted gray (`#AAA`)
- [ ] The heatmap fills remaining vertical space below the timeline (flex: 1)
- [ ] All heatmap data — columns, rows, items — is sourced from `data.json`

---

### US-4: Configure Dashboard via JSON

**As a** project lead, **I want** to define all dashboard data in a single `data.json` file, **so that** I can update the dashboard content without modifying any code.

**Acceptance Criteria:**
- [ ] A `data.json` file in the project's `Data/` directory serves as the sole data source
- [ ] The JSON schema supports: project header (title, subtitle, backlog URL, current month), timeline tracks (name, color, milestones), and heatmap data (columns, highlight column, status rows with items)
- [ ] The application reads `data.json` on startup and deserializes it into strongly-typed C# models
- [ ] Sample `data.json` is provided with fictional project data demonstrating all features
- [ ] Changing `data.json` and refreshing the browser reflects updated data (no restart required in dev mode)

---

### US-5: Capture Screenshot-Ready Output

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, spinners, or loading indicators, **so that** I can take a browser screenshot and paste it directly into a PowerPoint slide.

**Acceptance Criteria:**
- [ ] The page body is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] Static SSR renders the complete page as HTML on first request — no WebSocket, no hydration delay
- [ ] No loading spinners, skeleton screens, or progressive rendering artifacts appear
- [ ] The page renders identically in Chrome, Edge, and Firefox at 100% zoom
- [ ] A standard browser screenshot (or Snipping Tool capture) produces a clean 1920×1080 image

---

### US-6: Run Dashboard Locally with Zero Configuration

**As a** developer or PM, **I want** to start the dashboard with a single `dotnet run` command, **so that** I don't need to configure databases, authentication, or cloud services.

**Acceptance Criteria:**
- [ ] `dotnet run` from the project directory starts the dashboard on `http://localhost:5000` (or configured port)
- [ ] No database connection string, API key, or authentication token is required
- [ ] No HTTPS certificate is required
- [ ] The application starts in under 3 seconds and the page is viewable immediately

---

### US-7: Handle Malformed Data Gracefully

**As a** project lead, **I want** the dashboard to display a clear error message if `data.json` is missing or malformed, **so that** I can diagnose and fix the data file without developer assistance.

**Acceptance Criteria:**
- [ ] If `data.json` is missing, the page displays: "Error: data.json not found. Please create a data.json file in the Data/ directory."
- [ ] If `data.json` contains invalid JSON, the page displays the deserialization error with line/column information
- [ ] If required fields are missing, the page displays which fields are absent
- [ ] Error messages render on the dashboard page itself (not a stack trace or generic 500 error)

---

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository, rendered at 1920×1080.

![OriginalDesignConcept design reference](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/main/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Layout

The page uses a **vertical flex column** layout (`display: flex; flex-direction: column`) with three stacked sections that fill exactly 1920×1080 pixels with no scrolling:

| Section | Height | Background | Description |
|---------|--------|------------|-------------|
| **Header** | ~50px (auto) | `#FFFFFF` | Title, subtitle, legend — flex-shrink: 0 |
| **Timeline** | 196px fixed | `#FAFAFA` | Track labels + SVG timeline — flex-shrink: 0 |
| **Heatmap** | Remaining space (flex: 1) | `#FFFFFF` | Status grid fills all remaining vertical space |

### Section 1: Header (`.hdr`)

- **Layout:** Flexbox row, `justify-content: space-between`, `align-items: center`
- **Padding:** `12px 44px 10px`
- **Bottom border:** `1px solid #E0E0E0`
- **Left side:**
  - Project title: `<h1>` at `24px`, `font-weight: 700`, color `#111`
  - Inline ADO backlog link: `color: #0078D4`, no underline
  - Subtitle: `12px`, `color: #888`, `margin-top: 2px`
- **Right side — Legend bar:**
  - Four inline items, `gap: 22px`, each `font-size: 12px`:
    1. **PoC Milestone:** 12×12px square rotated 45° (`transform: rotate(45deg)`), `background: #F4B400`
    2. **Production Release:** 12×12px square rotated 45°, `background: #34A853`
    3. **Checkpoint:** 8×8px circle, `background: #999`
    4. **Now indicator:** 2×14px vertical bar, `background: #EA4335`, label includes current month

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox row, `height: 196px`, `background: #FAFAFA`
- **Padding:** `6px 44px 0`
- **Bottom border:** `2px solid #E8E8E8`

#### Track Label Panel (Left, 230px)
- **Width:** 230px, `flex-shrink: 0`
- **Border-right:** `1px solid #E0E0E0`
- **Layout:** Flex column, `justify-content: space-around`, `padding: 16px 12px 16px 0`
- **Each track label:**
  - Track code (e.g., "M1"): `12px`, `font-weight: 600`, track accent color
  - Description: `font-weight: 400`, `color: #444`
- **Track accent colors:**
  - Track 1: `#0078D4` (blue)
  - Track 2: `#00897B` (teal)
  - Track 3: `#546E7A` (blue-gray)

#### SVG Timeline Panel (Right, flex: 1)
- **SVG dimensions:** `width="1560" height="185"`, `overflow: visible`
- **Month grid lines:** Vertical `<line>` elements at calculated X positions, `stroke: #bbb`, `stroke-opacity: 0.4`
- **Month labels:** `<text>` at `font-size: 11`, `font-weight: 600`, `fill: #666`, positioned 5px right of grid line at y=14
- **Track lines:** Horizontal `<line>` elements spanning full width, `stroke-width: 3`, using track accent colors
  - Track 1 at y=42, `stroke: #0078D4`
  - Track 2 at y=98, `stroke: #00897B`
  - Track 3 at y=154, `stroke: #546E7A`
- **Milestone markers:**
  - **Start checkpoint:** `<circle>` with `r="7"`, `fill: white`, `stroke: [track-color]`, `stroke-width: 2.5`
  - **Minor checkpoint:** `<circle>` with `r="4"` or `r="5"`, `fill: #999` or `fill: white` with `stroke: #888`
  - **PoC Milestone:** `<polygon>` diamond (11px radius), `fill: #F4B400`, with drop shadow filter
  - **Production Release:** `<polygon>` diamond (11px radius), `fill: #34A853`, with drop shadow filter
- **Diamond polygon formula:** For center point (cx, cy) with radius r=11: `points="cx,cy-11 cx+11,cy cx,cy+11 cx-11,cy"`
- **Drop shadow filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`
- **NOW line:** Dashed vertical `<line>`, `stroke: #EA4335`, `stroke-width: 2`, `stroke-dasharray: 5,3`, with "NOW" label in bold 10px red
- **Date labels:** `<text>` at `font-size: 10`, `fill: #666`, `text-anchor: middle`, positioned above or below markers

### Section 3: Heatmap (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1`, `min-height: 0`
- **Padding:** `10px 44px 10px`

#### Heatmap Title
- `14px`, `font-weight: 700`, `color: #888`, `letter-spacing: 0.5px`, `text-transform: uppercase`
- Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"
- `margin-bottom: 8px`

#### Heatmap Grid (`.hm-grid`)
- **CSS Grid:** `grid-template-columns: 160px repeat(N, 1fr)` where N = number of month columns
- **Grid rows:** `grid-template-rows: 36px repeat(4, 1fr)`
- **Border:** `1px solid #E0E0E0`
- **The grid fills all remaining vertical space** (`flex: 1; min-height: 0`)

#### Column Headers
- **Corner cell (`.hm-corner`):** `background: #F5F5F5`, text "STATUS" in `11px`, `font-weight: 700`, `color: #999`, uppercase
- **Month headers (`.hm-col-hdr`):** `16px`, `font-weight: 700`, `background: #F5F5F5`
- **Current month highlight (`.apr-hdr`):** `background: #FFF0D0`, `color: #C07700`
- **Bottom border on all headers:** `2px solid #CCC`

#### Status Rows — Color Themes

| Row | Header Color | Header BG | Cell BG | Highlight Cell BG | Dot Color |
|-----|-------------|-----------|---------|-------------------|-----------|
| **✅ Shipped** | `#1B7A28` | `#E8F5E9` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| **🔵 In Progress** | `#1565C0` | `#E3F2FD` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| **🟡 Carryover** | `#B45309` | `#FFF8E1` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| **🔴 Blockers** | `#991B1B` | `#FEF2F2` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers (`.hm-row-hdr`)
- `11px`, `font-weight: 700`, `text-transform: uppercase`, `letter-spacing: 0.7px`
- `border-right: 2px solid #CCC`
- Background and text color per row theme table above

#### Data Cells (`.hm-cell`)
- `padding: 8px 12px`
- `border-right: 1px solid #E0E0E0`, `border-bottom: 1px solid #E0E0E0`
- **Work items (`.it`):** `12px`, `color: #333`, `padding: 2px 0 2px 12px`, `line-height: 1.35`
- **Bullet dot:** CSS `::before` pseudo-element, `6px × 6px` circle, positioned at `left: 0; top: 7px`, colored per row theme

### Typography

| Element | Size | Weight | Color | Font Family |
|---------|------|--------|-------|-------------|
| Page title | 24px | 700 | `#111` | Segoe UI, Arial, sans-serif |
| Subtitle | 12px | 400 | `#888` | Segoe UI, Arial, sans-serif |
| Legend items | 12px | 400 | `#111` | Segoe UI, Arial, sans-serif |
| Track labels (code) | 12px | 600 | Track accent color | Segoe UI, Arial, sans-serif |
| Track labels (desc) | 12px | 400 | `#444` | Segoe UI, Arial, sans-serif |
| SVG month labels | 11px | 600 | `#666` | Segoe UI, Arial |
| SVG date labels | 10px | 400 | `#666` | Segoe UI, Arial |
| SVG "NOW" label | 10px | 700 | `#EA4335` | Segoe UI, Arial |
| Heatmap section title | 14px | 700 | `#888` | Segoe UI, Arial, sans-serif |
| Column headers | 16px | 700 | `#111` (or `#C07700` for current month) | Segoe UI, Arial, sans-serif |
| Row headers | 11px | 700 | Row accent color | Segoe UI, Arial, sans-serif |
| Cell items | 12px | 400 | `#333` | Segoe UI, Arial, sans-serif |

### Complete Color Palette

| Token | Hex | Usage |
|-------|-----|-------|
| Background | `#FFFFFF` | Page body, header |
| Timeline BG | `#FAFAFA` | Timeline area |
| Grid Header BG | `#F5F5F5` | Column/corner headers |
| Text Primary | `#111` | Title |
| Text Secondary | `#888` | Subtitle, section titles |
| Text Body | `#333` | Cell items |
| Text Muted | `#999` | Corner cell label |
| Text Track Desc | `#444` | Track descriptions |
| Link Blue | `#0078D4` | Hyperlinks, In Progress accent |
| Border Light | `#E0E0E0` | Cell borders, header bottom |
| Border Medium | `#CCC` | Row header right border, header bottom |
| Border Timeline | `#E8E8E8` | Timeline bottom border |
| Grid Line | `#bbb` (40% opacity) | SVG month grid lines |
| Shipped Green | `#34A853` | Shipped dot, production diamond |
| Shipped Header | `#1B7A28` | Shipped row header text |
| Shipped Header BG | `#E8F5E9` | Shipped row header background |
| Shipped Cell | `#F0FBF0` | Shipped cell background |
| Shipped Highlight | `#D8F2DA` | Shipped current-month cell |
| Progress Blue | `#0078D4` | In Progress dot, Track 1 line |
| Progress Header | `#1565C0` | In Progress row header text |
| Progress Header BG | `#E3F2FD` | In Progress row header background |
| Progress Cell | `#EEF4FE` | In Progress cell background |
| Progress Highlight | `#DAE8FB` | In Progress current-month cell |
| Carryover Amber | `#F4B400` | Carryover dot, PoC diamond |
| Carryover Header | `#B45309` | Carryover row header text |
| Carryover Header BG | `#FFF8E1` | Carryover row header background |
| Carryover Cell | `#FFFDE7` | Carryover cell background |
| Carryover Highlight | `#FFF0B0` | Carryover current-month cell |
| Blocker Red | `#EA4335` | Blocker dot, NOW line |
| Blocker Header | `#991B1B` | Blocker row header text |
| Blocker Header BG | `#FEF2F2` | Blocker row header background |
| Blocker Cell | `#FFF5F5` | Blocker cell background |
| Blocker Highlight | `#FFE4E4` | Blocker current-month cell |
| Current Month Header | `#FFF0D0` | Current month column header BG |
| Current Month Text | `#C07700` | Current month column header text |
| Track 2 Teal | `#00897B` | Track 2 line color |
| Track 3 Gray | `#546E7A` | Track 3 line color |

---

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Full Dashboard Render**
The user navigates to `http://localhost:5000`. The server renders the complete dashboard as static HTML in a single response. The user sees the header, timeline, and heatmap fully rendered with no loading spinners, skeleton screens, or progressive hydration. The page is immediately screenshot-ready.

**Scenario 2: User Views Project Header and Identifies the Report Context**
The user looks at the top of the page and sees the project title ("Privacy Automation Release Roadmap"), a clickable "→ ADO Backlog" link, the subtitle showing organization and month, and the legend explaining the four marker types. The user understands which project and time period this report covers.

**Scenario 3: User Reads the Milestone Timeline to Assess Delivery Progress**
The user examines the timeline section. They see track labels on the left (M1, M2, M3) with descriptions. On the right, horizontal colored lines represent each track with milestone markers placed at their calendar dates. A dashed red "NOW" line shows the current date. The user can visually compare what has been delivered (markers left of NOW) vs. what is upcoming (markers right of NOW).

**Scenario 4: User Hovers Over a Milestone Diamond and Sees Its Date Label**
The user positions their cursor over a gold diamond marker on the timeline. The date label (e.g., "Mar 26 PoC") is always visible as static text positioned near the marker — no tooltip or hover interaction is needed. All labels are permanently rendered in the SVG.

**Scenario 5: User Scans the Heatmap to Assess Monthly Execution Health**
The user looks at the heatmap grid. The current month column is visually highlighted with a gold header and darker cell backgrounds. The user scans top-to-bottom: green (shipped) items show completed work, blue (in progress) shows active items, amber (carryover) shows items that slipped from last month, and red (blockers) shows impediments. The relative density of items per cell gives an instant visual signal of execution health.

**Scenario 6: User Identifies the Current Month at a Glance**
The current month column header renders with a distinctive gold background (`#FFF0D0`) and amber text (`#C07700`) with an arrow indicator ("→ Now"). All other month headers use the standard gray. The user's eye is immediately drawn to the current month in both the heatmap and the timeline NOW line.

**Scenario 7: User Clicks the ADO Backlog Link**
The user clicks the "→ ADO Backlog" hyperlink in the header. The browser navigates to the Azure DevOps backlog URL specified in `data.json`. This is the only interactive element on the page.

**Scenario 8: User Takes a Screenshot for PowerPoint**
The user presses Win+Shift+S (Windows Snipping Tool) or uses browser DevTools to capture the page at 1920×1080. The captured image contains the complete dashboard with no scrollbars, no browser chrome artifacts, and no overflow content. The screenshot is pasted directly into a PowerPoint slide.

**Scenario 9: User Updates data.json and Refreshes**
The user opens `data.json` in a text editor, changes a work item description in the heatmap section, saves the file, and presses F5 in the browser. The dashboard re-renders with the updated data. During development with `dotnet watch`, the browser may auto-refresh.

**Scenario 10: Empty State — Future Month Cells**
Months in the future that have no planned items display a single dash ("—") in muted gray (`#AAA`) within each status row cell. The grid structure remains intact and visually consistent.

**Scenario 11: Error State — Missing data.json**
If `data.json` is missing or unreadable, the page renders a centered error message on a white background: "Error: data.json not found. Please create a data.json file in the Data/ directory." styled in `16px`, `color: #EA4335`. No stack trace or technical details are exposed.

**Scenario 12: Error State — Malformed JSON**
If `data.json` contains invalid JSON syntax, the page renders an error message showing the parse error with line and column number: "Error parsing data.json: Unexpected character at line 42, position 15." The user can use this information to fix the file.

**Scenario 13: Data-Driven Column Count**
A project lead configures `data.json` with 6 month columns (Jan–Jun) instead of the default 4. The heatmap grid dynamically adjusts `grid-template-columns` to `160px repeat(6, 1fr)`. All cells render proportionally narrower but maintain readability. The timeline SVG month grid lines also adjust to match.

**Scenario 14: No Responsive Behavior**
The page is fixed at 1920×1080. If the browser window is smaller, the page overflows with scrollbars appearing on the browser (not the page body). The dashboard does not reflow, collapse, or change layout at any viewport size. This is intentional — the output target is a fixed-resolution screenshot, not a responsive web application.

---

## Scope

### In Scope

- Single-page dashboard rendering all data from `data.json`
- Header component with project title, subtitle, backlog link, and legend
- SVG-based milestone timeline with multi-track support (1–5 tracks)
- Color-coded monthly execution heatmap with 4 status rows (Shipped, In Progress, Carryover, Blockers)
- Dynamic column count based on `data.json` configuration
- Current month visual highlighting (gold header, darker cell backgrounds, NOW line)
- Static server-side rendering (no WebSocket, no interactivity beyond link navigation)
- Fixed 1920×1080 viewport for screenshot capture
- Sample `data.json` with fictional project data
- Strongly-typed C# data models with `System.Text.Json` deserialization
- Error handling for missing or malformed `data.json`
- CSS extracted and refined from `OriginalDesignConcept.html` with custom properties
- `README.md` with setup and usage instructions
- Local-only execution via `dotnet run`

### Out of Scope

- **Authentication and authorization** — No login, no user accounts, no role-based access
- **Database integration** — No SQL Server, SQLite, or Entity Framework; JSON file is the sole data source
- **HTTPS / TLS certificates** — Local HTTP only
- **Responsive / mobile layouts** — Fixed 1920×1080 only
- **Real-time updates / WebSocket** — Static SSR; manual browser refresh to see changes
- **Multi-project support** — One dashboard instance = one project; no project selector UI
- **Dark mode** — Single light theme matching the reference design
- **Print CSS / PDF export** — Manual screenshot workflow only
- **Automated screenshot capture (Playwright)** — Optional future enhancement, not in MVP
- **Telemetry, analytics, or logging frameworks** — No Application Insights, Serilog, or equivalent
- **CI/CD pipeline** — Local development only
- **Internationalization / localization** — English only
- **Accessibility (WCAG compliance)** — Optimized for screenshot output, not screen readers
- **UI component libraries** — No MudBlazor, Radzen, Syncfusion, or Bootstrap
- **JavaScript interop** — Pure Razor + CSS + inline SVG
- **API endpoints / Swagger** — No REST API; this is a rendered page only
- **Unit tests / snapshot tests** — Recommended for Phase 2 but not required for MVP

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Application startup time | < 3 seconds from `dotnet run` to page-ready |
| Page render time (server) | < 200ms for full HTML generation |
| Page load time (browser) | < 500ms to fully rendered (no external assets to load) |
| Memory footprint | < 50MB RSS during normal operation |
| JSON deserialization | < 10ms for a typical `data.json` (< 50KB) |

### Reliability

| Metric | Target |
|--------|--------|
| Crash on malformed input | Never — graceful error display on all invalid `data.json` scenarios |
| Browser compatibility | Chrome 120+, Edge 120+, Firefox 120+ |
| Render consistency | Pixel-identical output across supported browsers at 100% zoom |

### Security

- No authentication required or implemented
- No data transmitted to external services
- No cookies, session state, or local storage used
- `data.json` stays on the local filesystem only
- HTTPS disabled by default in `launchSettings.json`

### Maintainability

| Metric | Target |
|--------|--------|
| Total file count | < 15 source files |
| Total lines of code | < 1,000 LOC (excluding generated/config files) |
| External NuGet packages | 0 beyond the .NET 8 SDK |
| JavaScript dependencies | 0 |

### Portability

- Runs on any machine with .NET 8 SDK installed
- No OS-specific dependencies (Windows, macOS, Linux)
- `data.json` is the only file that needs customization per project
- Can be published as a self-contained executable (~65MB) for distribution without SDK

---

## Success Metrics

| # | Metric | Target | Measurement |
|---|--------|--------|-------------|
| 1 | **Dashboard renders correctly from sample data** | 100% visual match to `OriginalDesignConcept.html` reference | Side-by-side screenshot comparison |
| 2 | **data.json-driven rendering** | All visible text, dates, and items sourced from JSON — zero hardcoded content | Change `data.json` values and verify all changes appear on refresh |
| 3 | **Screenshot capture quality** | 1920×1080 screenshot is presentation-ready without post-editing | Paste screenshot into PowerPoint at 100% — no cropping, scaling, or cleanup needed |
| 4 | **Zero-config startup** | New user can clone repo, run `dotnet run`, and see the dashboard | Test on a clean machine with only .NET 8 SDK installed |
| 5 | **Data update cycle time** | < 2 minutes from "edit JSON" to "screenshot in PowerPoint" | Time the end-to-end workflow |
| 6 | **Error resilience** | Dashboard displays user-friendly errors for all invalid `data.json` inputs | Test with: missing file, empty file, invalid JSON syntax, missing required fields |
| 7 | **Complexity budget** | < 15 files, < 1,000 LOC, 0 NuGet packages, 0 JavaScript | Count files and lines in final deliverable |

---

## Constraints & Assumptions

### Technical Constraints

1. **Technology stack is fixed:** .NET 8, Blazor Server, C#, static SSR. No alternative frameworks.
2. **No external UI component libraries:** All visuals must be implemented with pure HTML, CSS, and inline SVG in Razor components.
3. **No JavaScript:** All rendering logic must be server-side C# and Razor markup. No JS interop.
4. **No database:** `data.json` is the sole data source. No Entity Framework, no SQL.
5. **No external network calls:** The application must function with no internet connection.
6. **Fixed viewport:** The page is designed exclusively for 1920×1080 and will not adapt to other resolutions.
7. **Single project scope:** One `data.json` file = one project dashboard. No multi-project selector.

### Assumptions

1. **The user has .NET 8 SDK installed** (version 8.0.300 or later) on their development machine.
2. **The user has a modern browser** (Chrome, Edge, or Firefox, version 120+) available locally.
3. **Segoe UI font is available** on the user's machine (standard on Windows; macOS/Linux will fall back to Arial).
4. **The design reference `OriginalDesignConcept.html` is the canonical visual specification.** Any differences between this file and `C:/Pics/ReportingDashboardDesign.png` should be resolved in favor of the HTML file, with the PNG used for supplementary guidance.
5. **Dashboard data changes infrequently** (weekly or monthly). Real-time data updates are not required.
6. **The user captures screenshots manually** using OS tools (Win+Shift+S, Snipping Tool, or browser DevTools). Automated Playwright-based screenshot capture is a future enhancement.
7. **The heatmap will display 4–6 month columns** in typical use. The grid must support this range without layout degradation.
8. **The timeline will display 1–5 workstream tracks** in typical use. More than 5 may cause vertical overflow in the 196px timeline area.
9. **No PII or sensitive data** is stored in `data.json` — only project names, milestone dates, and work item descriptions.
10. **This dashboard will initially run on the author's machine only.** Distribution to other PMs is a future consideration that may require self-contained publishing.

### Dependencies

| Dependency | Owner | Risk |
|-----------|-------|------|
| .NET 8 SDK | Microsoft | Low — LTS release, widely available |
| `OriginalDesignConcept.html` design file | ReportingDashboard repo | Low — already committed to repo |
| `C:/Pics/ReportingDashboardDesign.png` design file | Local machine | Medium — must be reviewed for design delta before implementation begins |
| Segoe UI system font | OS-level | Low — available on Windows; graceful fallback to Arial |