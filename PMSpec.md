# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page, screenshot-ready executive reporting dashboard that visualizes project milestones on an SVG timeline and displays monthly execution status in a color-coded heatmap grid. The dashboard reads all data from a local `data.json` configuration file and renders a polished 1920×1080 view designed to be captured and embedded directly into PowerPoint decks for executive stakeholders. Built as a Blazor Server application on .NET 8, the tool prioritizes visual fidelity and simplicity over enterprise features — no authentication, no database, no deployment infrastructure.

## Business Goals

1. **Reduce executive reporting preparation time** — Eliminate manual slide-building by providing a live, data-driven dashboard that can be screenshot-captured in seconds and pasted directly into PowerPoint.
2. **Standardize project status communication** — Provide a consistent visual format (timeline + heatmap) that executives can read at a glance across any project, replacing ad-hoc status slides with varying formats.
3. **Enable rapid status updates** — Allow a project manager to update a single `data.json` file and immediately see the rendered dashboard, with no code changes or redeployment required.
4. **Maintain pixel-perfect screenshot quality** — Render at exactly 1920×1080 with a fixed layout so every screenshot is presentation-ready without cropping, scaling, or post-processing.
5. **Keep the tool dead simple** — Zero operational overhead: no servers to maintain, no databases to migrate, no credentials to manage. Run locally with `dotnet run`, capture screenshot, done.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project manager, **I want** to see the project title, organizational context, current month, and a link to the ADO backlog at the top of the dashboard, **so that** every screenshot is self-documenting and executives know exactly which project and time period they are viewing.

**Visual Reference:** Header section (`.hdr`) of `OriginalDesignConcept.html`

**Acceptance Criteria:**
- [ ] Project title renders at 24px bold in the top-left, matching the header bar layout
- [ ] Subtitle displays organization, workstream, and current month in 12px gray (#888) text below the title
- [ ] ADO Backlog link renders as a clickable blue (#0078D4) hyperlink next to the title
- [ ] Legend icons render in the top-right: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), Now indicator (red vertical line)
- [ ] All header values are driven from `data.json` `header` object — no hardcoded text
- [ ] Header has a 1px solid #E0E0E0 bottom border separating it from the timeline

### US-2: View Milestone Timeline with Track Lines

**As an** executive viewer, **I want** to see a horizontal timeline showing major milestone tracks (M1, M2, M3, etc.) with their key dates visualized as markers on a Gantt-style SVG, **so that** I can immediately understand the project's temporal roadmap and where each workstream stands.

**Visual Reference:** Timeline area (`.tl-area` and inline SVG) of `OriginalDesignConcept.html`

**Acceptance Criteria:**
- [ ] Timeline section renders at 196px height with #FAFAFA background, bordered below by 2px solid #E8E8E8
- [ ] Left sidebar (230px wide) lists track labels (e.g., "M1 — Core API & Auth") with track-colored text and descriptions
- [ ] SVG area (flex: 1) renders horizontal track lines spanning the full width, each colored by track color
- [ ] Vertical month gridlines render at computed positions with month labels (Jan, Feb, Mar, etc.) at the top
- [ ] "NOW" indicator renders as a dashed red (#EA4335) vertical line at the current date position with "NOW" label
- [ ] All positions are computed dynamically from `data.json` date ranges — no hardcoded pixel values
- [ ] Track count, labels, colors, and milestone dates are all driven from `data.json`

### US-3: View Milestone Markers on Timeline

**As an** executive viewer, **I want** to see distinct visual markers for different milestone types (PoC, Production Release, Checkpoint) placed at their correct dates on each track line, **so that** I can identify upcoming deadlines and past achievements at a glance.

**Visual Reference:** SVG milestone markers (diamonds, circles) in `OriginalDesignConcept.html`

**Acceptance Criteria:**
- [ ] PoC milestones render as gold (#F4B400) diamond shapes (rotated square polygon) with drop shadow
- [ ] Production Release milestones render as green (#34A853) diamond shapes with drop shadow
- [ ] Checkpoint milestones render as circles — larger open circles (r=7, white fill with colored stroke) for major checkpoints, smaller filled gray (#999) circles (r=4) for minor checkpoints
- [ ] Each milestone has a text label positioned above or below the marker (alternating to avoid overlap)
- [ ] Labels display the milestone date and optional descriptor (e.g., "Mar 26 PoC")
- [ ] Milestone type, date, label, track assignment, and description are all sourced from `data.json`

### US-4: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a color-coded grid showing what was Shipped, In Progress, Carried Over, and Blocked for each month, **so that** I can assess execution velocity and identify patterns or concerns.

**Visual Reference:** Heatmap grid (`.hm-wrap`, `.hm-grid`) in `OriginalDesignConcept.html`

**Acceptance Criteria:**
- [ ] Section title "Monthly Execution Heatmap" renders in 14px bold uppercase gray (#888) with 0.5px letter-spacing
- [ ] Grid renders with CSS Grid: first column 160px (row headers), remaining columns equal-width (one per month)
- [ ] Header row shows month names in 16px bold, with the current month highlighted in gold background (#FFF0D0) with amber text (#C07700)
- [ ] Corner cell displays "Status" in 11px uppercase bold gray (#999)
- [ ] Four status rows render: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each row header shows a status icon and label in uppercase with the row's theme color
- [ ] Data cells show bullet-point items with 6px colored circle bullets matching the row theme
- [ ] Current-month column cells have a darker highlight background than other cells
- [ ] Empty cells display a gray dash "-"
- [ ] All row categories, month columns, highlight column index, and cell items are driven from `data.json`

### US-5: Configure Dashboard via JSON

**As a** project manager, **I want** to edit a single `data.json` file to change all dashboard content (title, milestones, heatmap items, dates), **so that** I can update the dashboard without touching any code or restarting the application.

**Acceptance Criteria:**
- [ ] Application reads `data.json` from the project root on startup
- [ ] JSON schema supports: `header` (title, subtitle, backlogUrl, currentMonth), `timeline` (startDate, endDate, nowDate, tracks, milestones), `heatmap` (columns, highlightColumnIndex, rows with cells)
- [ ] Changing `data.json` and reloading the page reflects the new data (FileSystemWatcher optional enhancement)
- [ ] Malformed JSON produces a clear error message rather than a blank page or cryptic exception
- [ ] A sample `data.json` with fictional "Project Phoenix" data ships with the project as documentation and starting point

### US-6: Capture Screenshot-Ready Output

**As a** project manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, browser chrome artifacts, or layout shifts, **so that** I can take a full-page screenshot and paste it directly into a PowerPoint slide without any editing.

**Acceptance Criteria:**
- [ ] `<body>` is styled with `width: 1920px; height: 1080px; overflow: hidden`
- [ ] All content fits within the 1920×1080 viewport with no scrollbars
- [ ] Layout uses Segoe UI font family with Arial and sans-serif fallbacks
- [ ] Background is pure white (#FFFFFF)
- [ ] The page renders identically in Edge and Chrome at 1920×1080 device emulation
- [ ] No Blazor framework UI elements (error boundaries, reconnect dialogs) are visible during normal operation

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository. Also reference `C:/Pics/ReportingDashboardDesign.png` for the target visual. Engineers MUST consult the rendered screenshot at `docs/design-screenshots/OriginalDesignConcept.png` and match it pixel-for-pixel.

### Overall Page Layout

- **Dimensions:** Fixed 1920px × 1080px, `overflow: hidden`
- **Background:** #FFFFFF
- **Font:** `'Segoe UI', Arial, sans-serif`, base color #111
- **Layout:** Flexbox column (`display: flex; flex-direction: column`) with three stacked sections:
  1. **Header bar** — fixed height, flex-shrink: 0
  2. **Timeline area** — fixed 196px height, flex-shrink: 0
  3. **Heatmap area** — flex: 1 (fills remaining space)

### Section 1: Header Bar (`.hdr`)

- **Padding:** 12px 44px 10px
- **Border:** 1px solid #E0E0E0 bottom
- **Layout:** Flexbox row, `align-items: center; justify-content: space-between`
- **Left side:**
  - **Title:** `<h1>` at 24px, font-weight 700, color #111
  - **Backlog link:** Inline `<a>` in #0078D4, no text-decoration
  - **Subtitle:** 12px, color #888, margin-top 2px — format: "Org · Workstream · Month Year"
- **Right side (Legend):**
  - Flexbox row with 22px gap
  - Four legend items, each a flex row with 6px gap:
    - PoC Milestone: 12×12px square, background #F4B400, rotated 45° (diamond shape)
    - Production Release: 12×12px square, background #34A853, rotated 45°
    - Checkpoint: 8×8px circle, background #999
    - Now indicator: 2×14px rectangle, background #EA4335
  - Label text: 12px

### Section 2: Timeline Area (`.tl-area`)

- **Height:** 196px, flex-shrink: 0
- **Background:** #FAFAFA
- **Border:** 2px solid #E8E8E8 bottom
- **Padding:** 6px 44px 0
- **Layout:** Flexbox row, `align-items: stretch`

#### Timeline Left Sidebar (Track Labels)
- **Width:** 230px, flex-shrink: 0
- **Border:** 1px solid #E0E0E0 right
- **Padding:** 16px 12px 16px 0
- **Layout:** Flexbox column, `justify-content: space-around`
- **Each track label:**
  - Track ID (e.g., "M1") in 12px, font-weight 600, colored by track color
  - Description in 12px, font-weight 400, color #444
  - Line-height: 1.4

#### Timeline SVG Area (`.tl-svg-box`)
- **Layout:** flex: 1, padding-left 12px, padding-top 6px
- **SVG:** 1560px wide × 185px tall, `overflow: visible`
- **SVG elements:**
  - **Drop shadow filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3">`
  - **Month gridlines:** Vertical `<line>` from y=0 to y=185, stroke #BBB at opacity 0.4, width 1
  - **Month labels:** `<text>` at y=14, fill #666, font-size 11, font-weight 600
  - **NOW line:** Vertical dashed `<line>` (stroke-dasharray 5,3), stroke #EA4335, width 2
  - **NOW label:** `<text>` fill #EA4335, font-size 10, font-weight 700
  - **Track lines:** Horizontal `<line>` from x=0 to x=1560, stroke width 3, colored per track
    - Track 1 (M1): y=42, stroke #0078D4
    - Track 2 (M2): y=98, stroke #00897B
    - Track 3 (M3): y=154, stroke #546E7A
    - Y-positions computed: evenly spaced within 185px height (approximately 42, 98, 154 for 3 tracks)
  - **Checkpoint markers:** `<circle>` — major: r=7 white fill with colored stroke (2.5 width); minor: r=4-5, filled #999 or white with #888 stroke
  - **PoC diamonds:** `<polygon>` with 4 points forming 22px diamond (±11px from center), fill #F4B400, filter drop-shadow
  - **Production diamonds:** `<polygon>` same shape, fill #34A853, filter drop-shadow
  - **Milestone labels:** `<text>` centered on marker, fill #666, font-size 10 — positioned above (y - 16) or below (y + 24) the track line

### Section 3: Heatmap Area (`.hm-wrap`)

- **Layout:** flex: 1, flexbox column
- **Padding:** 10px 44px 10px

#### Heatmap Title
- Font-size 14px, font-weight 700, color #888
- Letter-spacing 0.5px, text-transform uppercase
- Margin-bottom 8px
- Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"

#### Heatmap Grid (`.hm-grid`)
- **Layout:** CSS Grid
- **Columns:** `160px repeat(N, 1fr)` where N = number of months (default 4)
- **Rows:** `36px repeat(4, 1fr)` — 36px header row, 4 equal data rows
- **Border:** 1px solid #E0E0E0

#### Grid Header Row
- **Corner cell (`.hm-corner`):** background #F5F5F5, 11px bold uppercase #999, centered, border-right 1px solid #E0E0E0, border-bottom 2px solid #CCC
- **Column headers (`.hm-col-hdr`):** background #F5F5F5, 16px bold, centered, border-right 1px solid #E0E0E0, border-bottom 2px solid #CCC
- **Highlighted month column header:** background #FFF0D0, color #C07700

#### Data Rows — Color Themes

| Row | Header BG | Header Text | Cell BG | Cell BG (highlight) | Bullet Color |
|-----|-----------|-------------|---------|---------------------|--------------|
| **Shipped** | #E8F5E9 | #1B7A28 | #F0FBF0 | #D8F2DA | #34A853 |
| **In Progress** | #E3F2FD | #1565C0 | #EEF4FE | #DAE8FB | #0078D4 |
| **Carryover** | #FFF8E1 | #B45309 | #FFFDE7 | #FFF0B0 | #F4B400 |
| **Blockers** | #FEF2F2 | #991B1B | #FFF5F5 | #FFE4E4 | #EA4335 |

#### Row Headers (`.hm-row-hdr`)
- 11px bold uppercase, letter-spacing 0.7px
- Padding 0 12px, vertically centered
- Border-right 2px solid #CCC, border-bottom 1px solid #E0E0E0
- Includes status emoji/icon prefix (✅, 🔄, etc.)

#### Data Cells (`.hm-cell`)
- Padding 8px 12px
- Border-right 1px solid #E0E0E0, border-bottom 1px solid #E0E0E0
- Overflow hidden
- **Items (`.it`):** font-size 12px, color #333, padding 2px 0 2px 12px, line-height 1.35
- **Bullet:** `::before` pseudo-element — 6×6px circle, positioned absolute, left 0, top 7px, colored per row theme
- **Empty state:** Single item with text "-" in color #AAA

### Component Hierarchy

```
Dashboard.razor (page at /)
├── DashboardHeader.razor — header bar with title, subtitle, legend
├── TimelineSection.razor — full timeline area
│   ├── Track labels sidebar (inline or sub-component)
│   └── Inline SVG with computed milestone markers
└── HeatmapGrid.razor — heatmap container
    ├── Header row (corner + column headers)
    └── HeatmapRow.razor × 4 (one per status category)
        └── HeatmapCell.razor × N (one per month column)
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Dashboard Renders with Data**
User navigates to `https://localhost:5001`. The page loads and renders the complete dashboard: header with project title and legend, timeline with all milestone markers, and heatmap grid with all status items. All content is sourced from `data.json`. The page fits exactly within 1920×1080 with no scrollbars.

**Scenario 2: User Views the Header and Identifies the Project**
User sees the project title (e.g., "Project Phoenix Release Roadmap") in large bold text at top-left, the organizational subtitle below it, and the legend icons at top-right. The user immediately knows which project, workstream, and month this dashboard represents.

**Scenario 3: User Reads the Timeline to Understand Milestone Progress**
User looks at the timeline area and sees 3 horizontal colored track lines (M1, M2, M3). The left sidebar labels each track with its ID and description. Vertical month gridlines divide the timeline into months. A dashed red "NOW" line indicates the current date. Diamond markers (gold for PoC, green for Production) and circle markers (Checkpoints) are placed at their respective dates along each track line. The user can visually assess which milestones are past, current, and upcoming.

**Scenario 4: User Hovers Over a Milestone Diamond and Sees the Label**
Milestone labels (date and description text) are rendered as static SVG `<text>` elements adjacent to each marker. No hover tooltip is required — all labels are always visible. This ensures screenshots capture the full context without interactive states.

**Scenario 5: User Scans the Heatmap to Assess Monthly Execution**
User looks at the heatmap grid and reads across each row (Shipped, In Progress, Carryover, Blockers) to see which items fall into each category for each month. The current month column is visually highlighted with a warmer background color and gold header. The user can quickly assess: "We shipped 2 items in April, have 2 in progress, 1 carryover, and 2 blockers."

**Scenario 6: User Identifies Blockers by Scanning the Red Row**
User's eye is drawn to the Blockers row (red-tinted cells). They see red bullet-pointed items like "Vendor API access pending" and "Capacity approval." The red color immediately signals urgency. Empty blocker cells in earlier months show a gray dash, confirming no blockers existed then.

**Scenario 7: User Captures a Screenshot for PowerPoint**
User opens browser DevTools, sets device emulation to 1920×1080, navigates to the dashboard, and takes a full-page screenshot. The captured image fits perfectly into a 16:9 PowerPoint slide without cropping, scaling, or editing. All text is crisp and legible.

**Scenario 8: User Updates data.json to Reflect New Status**
User edits `data.json` to add a new "Shipped" item in April and move a "Blocker" to "Shipped." User refreshes the browser (or the FileSystemWatcher triggers a reload). The heatmap immediately reflects the changes — the new item appears in the Shipped row's April cell, and the Blocker row's April cell has one fewer item.

**Scenario 9: Empty Heatmap Cells Display Gracefully**
For months where a status category has no items (e.g., no Blockers in January), the cell displays a single gray dash "—" in #AAA color. The cell does not collapse or distort the grid layout.

**Scenario 10: Malformed data.json Produces a Clear Error**
User accidentally introduces a JSON syntax error in `data.json`. On page load, instead of a blank page or unhandled exception, the dashboard displays a clear error message indicating the JSON could not be parsed, with the file path and error details.

**Scenario 11: Timeline Handles Varying Track Counts**
User configures `data.json` with 2 tracks instead of 3. The timeline SVG adjusts track line y-positions to evenly distribute across the available height. The left sidebar shows only 2 track labels. Layout remains balanced.

**Scenario 12: Heatmap Handles Varying Month Counts**
User configures `data.json` with 6 months instead of 4. The CSS Grid adjusts column count to `160px repeat(6, 1fr)`. All 6 month columns render with proportionally narrower widths. Item text may truncate with overflow hidden if cells become too narrow.

## Scope

### In Scope

- Single-page Blazor Server dashboard rendering at 1920×1080
- Header component with project title, subtitle, backlog link, and milestone legend
- SVG timeline with dynamic track lines, month gridlines, NOW indicator, and milestone markers (diamonds, circles)
- CSS Grid heatmap with 4 status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- Color-coded rows with themed backgrounds, bullet colors, and highlight column for current month
- All dashboard content driven by a single `data.json` file
- Strongly-typed C# data models (records/POCOs) for JSON deserialization
- DataService singleton to load and cache `data.json` via `System.Text.Json`
- Sample `data.json` with fictional "Project Phoenix" data
- Blazor CSS isolation (`.razor.css` files) mirroring the reference HTML's styles
- CSS custom properties for the color palette
- FileSystemWatcher on `data.json` for live reload without restart (nice-to-have)
- Error handling for malformed JSON with user-visible error message
- `dotnet watch` support for hot-reload development workflow

### Out of Scope

- **Authentication / Authorization** — No login, no roles, no identity provider
- **Database** — No SQLite, no SQL Server, no Entity Framework. Data lives in `data.json` only
- **Multi-user support** — This is a single-user local tool
- **Real-time updates / SignalR push** — Page refresh is sufficient for data updates
- **Print-optimized CSS** — No `@media print` stylesheets
- **Responsive / mobile layout** — Fixed 1920×1080 only; not designed for mobile or tablets
- **Interactive filtering, sorting, or drill-down** — The dashboard is a static render, not a BI tool
- **Deployment pipeline** — No CI/CD, no Docker, no Azure hosting
- **Multiple project switching** — One dashboard instance shows one project's `data.json`
- **Accessibility (WCAG compliance)** — This is a screenshot tool, not a public-facing web application
- **Internationalization / localization** — English only
- **Automated testing** — Optional; visual inspection is the primary QA method
- **Third-party UI component libraries** (MudBlazor, Radzen, etc.)
- **Charting libraries** (Chart.js, Plotly, etc.) — SVG is hand-rendered

## Non-Functional Requirements

### Performance
- **Page load time:** Dashboard must render fully within 2 seconds on localhost (no network latency)
- **Data file size:** `data.json` is expected to be < 10KB; no performance optimization needed for larger files
- **Memory footprint:** Application should consume < 100MB RAM during normal operation

### Reliability
- **Uptime:** Not applicable — this is a local dev tool started on demand
- **Error handling:** Malformed `data.json` must produce a visible, descriptive error — never a blank page
- **Graceful degradation:** If `data.json` is missing, display a clear "data.json not found" message with expected file path

### Security
- **Authentication:** None required
- **Data sensitivity:** `data.json` contains non-sensitive project status information only
- **HTTPS:** Default Kestrel HTTPS with dev cert is sufficient; not a hard requirement
- **No PII, no secrets** stored or transmitted

### Compatibility
- **Target browsers:** Microsoft Edge and Google Chrome (latest stable versions)
- **Target OS:** Windows 10/11 (Segoe UI font dependency)
- **Target resolution:** 1920×1080 exactly — no other resolutions need to be supported
- **Framework:** .NET 8.0 LTS

### Maintainability
- **Code complexity:** Flat component architecture. No enterprise patterns (Clean Architecture, CQRS, MediatR)
- **Component count:** ≤ 8 Razor components total
- **Total code size:** Target < 1000 lines of C# and Razor combined (excluding generated files)

## Success Metrics

| # | Metric | Target | How to Measure |
|---|--------|--------|----------------|
| 1 | **Visual fidelity** | Dashboard screenshot is visually indistinguishable from `OriginalDesignConcept.html` rendered at 1920×1080 | Side-by-side comparison of screenshots |
| 2 | **Data-driven rendering** | 100% of displayed content sourced from `data.json` — zero hardcoded project-specific text | Change `data.json` to a completely different project; verify all content updates |
| 3 | **Time to update** | Project manager can update `data.json` and capture a new screenshot in < 60 seconds | Timed workflow test |
| 4 | **Screenshot quality** | Captured image fits a 16:9 PowerPoint slide at 1920×1080 with no cropping or scaling | Insert screenshot into PowerPoint; verify no blank borders or overflow |
| 5 | **Setup simplicity** | New developer can clone repo, run `dotnet run`, and see the dashboard in < 3 minutes | Timed onboarding test with no prior setup |
| 6 | **JSON flexibility** | Dashboard correctly renders with 2–5 timeline tracks and 3–6 heatmap month columns | Test with multiple `data.json` configurations |
| 7 | **Build success** | `dotnet build` completes with zero warnings and zero errors | CI or manual build verification |

## Constraints & Assumptions

### Technical Constraints

- **Framework mandate:** Must be built with Blazor Server on .NET 8.0 — no alternative frontend frameworks
- **No third-party UI libraries:** The design must be implemented with native CSS Grid, Flexbox, and inline SVG only
- **Fixed viewport:** 1920×1080 with `overflow: hidden` — the page is not responsive
- **Windows-only:** Segoe UI font is required; the tool is developed and used on Windows
- **Local-only hosting:** Runs on `localhost` via Kestrel — no remote deployment target
- **Single JSON file:** All data sourced from one `data.json` — no database, no API calls, no configuration merging
- **No NuGet dependencies:** Zero additional packages beyond what ships with the .NET 8 SDK

### Timeline Assumptions

- **Estimated effort:** 1–2 developer-days for a complete, polished implementation
- **Phase 1 (Day 1 AM):** Skeleton layout, header, and heatmap grid with hardcoded data
- **Phase 2 (Day 1 PM):** Data models, `data.json`, DataService, data-driven heatmap rendering
- **Phase 3 (Day 2 AM):** SVG timeline with computed milestone positions
- **Phase 4 (Day 2 PM):** Polish, color tuning, FileSystemWatcher, documentation

### Dependency Assumptions

- **.NET 8 SDK** is installed on the developer's machine
- **Visual Studio 2022** or **VS Code with C# Dev Kit** is available
- **Edge or Chrome** (latest stable) is available for rendering and screenshot capture
- The `OriginalDesignConcept.html` reference file in the ReportingDashboard repo is the canonical design source
- The `C:/Pics/ReportingDashboardDesign.png` file represents the desired visual output and should be consulted alongside the HTML reference — if the two conflict, the PNG takes precedence as the product owner's intent
- The sample `data.json` schema defined in this spec (see Research Findings appendix) is the agreed-upon contract between the data file and the application

### Design Assumptions

- The dashboard displays **one project at a time** — no project selector or multi-project view
- Timeline tracks, milestone counts, and heatmap month columns are **dynamic from `data.json`** (not fixed to 3 tracks / 4 months)
- The heatmap's four status categories (Shipped, In Progress, Carryover, Blockers) are **fixed** — they are not configurable from `data.json`
- The "current month" highlight column is specified via `highlightColumnIndex` in `data.json`, not auto-detected from the system clock
- All milestone labels are **always visible** as static text — no hover-to-reveal tooltips (screenshots cannot capture hover states)