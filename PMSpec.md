# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, timelines, and monthly execution status (shipped, in-progress, carryover, blockers) in a format optimized for 1920×1080 screenshot capture and PowerPoint presentation. The dashboard reads all data from a local `data.json` configuration file, requires zero authentication or infrastructure, and is built as a minimal Blazor Server application on .NET 8. This tool enables project leads to produce polished, consistent executive status reports by simply editing a JSON file and taking a browser screenshot.

## Business Goals

1. **Reduce executive report preparation time** from hours of manual PowerPoint slide building to minutes of JSON editing plus a screenshot capture.
2. **Standardize project status reporting** across teams by providing a reusable, data-driven template that enforces consistent visual formatting and categorization (Shipped, In Progress, Carryover, Blockers).
3. **Improve milestone visibility** by rendering an SVG timeline with date-positioned markers (checkpoints, PoC milestones, production releases) and a "NOW" indicator, giving executives an immediate sense of project trajectory.
4. **Eliminate tooling overhead** by delivering a zero-dependency, local-only application that any team member can run with `dotnet run` — no cloud accounts, no database, no authentication setup.
5. **Enable non-developers to update project data** by using a human-readable JSON file as the sole data source, allowing PMs and leads to maintain report content without modifying code.

## User Stories & Acceptance Criteria

### US-1: View Project Dashboard at a Glance

**As a** project lead, **I want** to open a single browser page and immediately see the full project status — header, milestone timeline, and monthly heatmap — **so that** I can assess project health in under 10 seconds.

**Acceptance Criteria:**
- [ ] The dashboard renders as a single page at exactly 1920×1080 pixels with no scrolling required
- [ ] The page loads in under 2 seconds on localhost
- [ ] All three visual sections are visible simultaneously: header, timeline, and heatmap
- [ ] **Visual Reference:** Matches `OriginalDesignConcept.html` overall layout — header bar at top, timeline area in middle, heatmap grid filling remaining space

### US-2: Configure Dashboard via JSON

**As a** project lead, **I want** to edit a `data.json` file to change the project title, subtitle, milestones, and heatmap items, **so that** I can reuse this dashboard for any project without modifying code.

**Acceptance Criteria:**
- [ ] All displayed text (title, subtitle, backlog URL, milestone labels, heatmap items) is sourced from `data.json`
- [ ] Changing `data.json` and refreshing the browser reflects updates immediately
- [ ] The JSON schema supports: title, subtitle, backlogUrl, currentDate, timeline date range, milestones with events, and four heatmap categories with per-month item lists
- [ ] Invalid JSON produces a clear error message on the page (not a stack trace)

### US-3: View Milestone Timeline with Date-Positioned Markers

**As an** executive viewer, **I want** to see a horizontal timeline with labeled month columns, milestone swim lanes, and date-accurate markers (checkpoints, PoC diamonds, production diamonds, "NOW" line), **so that** I understand the project's temporal progression and upcoming deliverables.

**Acceptance Criteria:**
- [ ] The timeline renders as an inline SVG within the timeline area section
- [ ] Month grid lines are evenly spaced and labeled (e.g., Jan, Feb, Mar, Apr, May, Jun)
- [ ] Each milestone appears as a horizontal colored bar with its own swim lane
- [ ] Milestone labels appear in a left-side panel (230px wide) with milestone ID and description
- [ ] Checkpoint events render as open circles on the milestone bar
- [ ] PoC milestones render as gold (#F4B400) diamond shapes with drop shadow
- [ ] Production milestones render as green (#34A853) diamond shapes with drop shadow
- [ ] A red dashed vertical "NOW" line (#EA4335) appears at the correct date position
- [ ] Event labels (date text) appear above or below the marker without overlapping
- [ ] **Visual Reference:** Matches the timeline section of `OriginalDesignConcept.html` — SVG area with three milestone swim lanes (M1, M2, M3), month gridlines Jan–Jun, diamond markers, and NOW line

### US-4: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a color-coded grid showing what was Shipped, In Progress, Carried Over, and Blocked for each month, **so that** I can quickly assess execution velocity and identify problem areas.

**Acceptance Criteria:**
- [ ] The heatmap renders as a CSS Grid with a "Status" column header and N month column headers
- [ ] Four category rows are displayed: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each cell contains bullet-pointed items with a colored dot matching the category
- [ ] The current month column is visually highlighted with a slightly darker background and a distinct column header style (gold background #FFF0D0, text color #C07700)
- [ ] Empty cells display a muted dash character
- [ ] The heatmap section title reads "Monthly Execution Heatmap" in uppercase with secondary text color
- [ ] **Visual Reference:** Matches the heatmap grid section of `OriginalDesignConcept.html` — 160px row header column + repeat(N, 1fr) data columns, color-coded rows, current month highlighting

### US-5: Click Through to ADO Backlog

**As a** project lead, **I want** the header to include a clickable link to the Azure DevOps backlog, **so that** viewers of the live dashboard (not screenshot) can navigate to the source of truth for work items.

**Acceptance Criteria:**
- [ ] The header displays a "→ ADO Backlog" link next to the title
- [ ] The link URL is sourced from `backlogUrl` in `data.json`
- [ ] The link opens in the default browser behavior (same tab)
- [ ] Link color is #0078D4 with no underline, matching the reference design
- [ ] **Visual Reference:** Matches the header section of `OriginalDesignConcept.html` — link appears inline after the H1 title

### US-6: Auto-Reload on Data Changes

**As a** project lead, **I want** the dashboard to automatically refresh when I save changes to `data.json`, **so that** I can iterate on report content without manually refreshing the browser.

**Acceptance Criteria:**
- [ ] A `FileSystemWatcher` monitors `data.json` for changes
- [ ] When the file is modified, the dashboard re-reads the JSON and triggers a Blazor re-render
- [ ] Multiple rapid file system events (e.g., from VS Code save) are debounced to a single reload
- [ ] The page does not flicker or lose scroll position during reload

### US-7: Capture Pixel-Perfect Screenshot

**As a** project lead, **I want** the dashboard to render identically to the reference design at 1920×1080, **so that** my PowerPoint screenshots are clean, professional, and consistent across reporting periods.

**Acceptance Criteria:**
- [ ] Body element is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] Font family is Segoe UI (system font), matching the reference design
- [ ] All colors, spacing, borders, and typography match `OriginalDesignConcept.html` within 2px tolerance
- [ ] Chrome DevTools device emulation at 1920×1080 produces a screenshot indistinguishable from the reference
- [ ] No Blazor framework UI (error boundaries, reconnect dialogs) is visible in normal operation

### US-8: View Dashboard with Fictional Sample Data

**As a** developer or evaluator, **I want** the project to ship with a complete `data.json` containing realistic fictional project data, **so that** I can see the dashboard working immediately after cloning and running the project.

**Acceptance Criteria:**
- [ ] A `data.json` file is included with fictional but realistic project data
- [ ] The sample data populates all four heatmap categories with 2–4 items per cell
- [ ] The sample data includes 3 milestones with varied event types (checkpoint, PoC, production)
- [ ] The sample data covers a 6-month timeline range
- [ ] Running `dotnet run` and opening the browser shows a fully populated dashboard with no configuration needed

## Visual Design Specification

**Reference File:** `OriginalDesignConcept.html` from the ReportingDashboard repository. Engineers MUST consult this file and the rendered screenshot (`docs/design-screenshots/OriginalDesignConcept.png`) as the canonical visual specification.

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/main/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Page Layout

- **Dimensions:** Fixed 1920×1080 pixels, no scrolling, `overflow: hidden`
- **Background:** White (#FFFFFF)
- **Font:** `'Segoe UI', Arial, sans-serif`
- **Primary text color:** #111
- **Layout:** Vertical flex column (`display: flex; flex-direction: column`) with three stacked sections:
  1. **Header Bar** (flex-shrink: 0, ~50px)
  2. **Timeline Area** (flex-shrink: 0, fixed height 196px)
  3. **Heatmap Area** (flex: 1, fills remaining vertical space)

### Section 1: Header Bar (`.hdr`)

- **Padding:** 12px 44px 10px
- **Border:** 1px solid #E0E0E0 on bottom
- **Layout:** Flexbox, `align-items: center; justify-content: space-between`
- **Left side:**
  - **Title:** H1, 24px, font-weight 700, e.g., "Privacy Automation Release Roadmap"
  - **Backlog link:** Inline `<a>` tag, color #0078D4, no underline, text "→ ADO Backlog"
  - **Subtitle:** 12px, color #888, margin-top 2px, e.g., "Trusted Platform · Privacy Automation Workstream · April 2026"
- **Right side — Legend:** Horizontal flex row, gap 22px, containing four legend items:
  - **PoC Milestone:** 12×12px gold (#F4B400) square rotated 45° (diamond shape) + "PoC Milestone" label at 12px
  - **Production Release:** 12×12px green (#34A853) square rotated 45° + "Production Release" label at 12px
  - **Checkpoint:** 8×8px circle, background #999 + "Checkpoint" label at 12px
  - **Now indicator:** 2×14px red (#EA4335) vertical bar + "Now (Apr 2026)" label at 12px

### Section 2: Timeline Area (`.tl-area`)

- **Height:** 196px fixed
- **Background:** #FAFAFA
- **Border:** 2px solid #E8E8E8 on bottom
- **Padding:** 6px 44px 0
- **Layout:** Horizontal flex (`display: flex; align-items: stretch`)

#### Left Panel — Milestone Labels (230px wide)

- **Width:** 230px, flex-shrink 0
- **Border:** 1px solid #E0E0E0 on right
- **Padding:** 16px 12px 16px 0
- **Layout:** Flex column, `justify-content: space-around`
- **Each milestone label:**
  - Font-size 12px, font-weight 600, line-height 1.4
  - Milestone ID (e.g., "M1") in the milestone's theme color
  - Description (e.g., "Chatbot & MS Role") in font-weight 400, color #444
  - Milestone colors: M1=#0078D4, M2=#00897B, M3=#546E7A (from reference)

#### Right Panel — SVG Timeline (`.tl-svg-box`, flex: 1)

- **SVG Dimensions:** 1560×185 pixels, `overflow: visible`
- **Padding:** left 12px, top 6px

**SVG Elements:**

| Element | Specification |
|---------|--------------|
| **Month grid lines** | Vertical `<line>` from y=0 to y=185, stroke #bbb, opacity 0.4, stroke-width 1. Evenly spaced at ~260px intervals for 6 months. |
| **Month labels** | `<text>` at x=gridLine+5, y=14, fill #666, font-size 11px, font-weight 600, Segoe UI. Text: "Jan", "Feb", "Mar", etc. |
| **NOW line** | Vertical dashed `<line>` at the current date's x-position, stroke #EA4335, stroke-width 2, stroke-dasharray "5,3". Label: `<text>` "NOW" in #EA4335, font-size 10px, font-weight 700. |
| **Milestone bars** | Horizontal `<line>` spanning full SVG width (x1=0 to x2=1560), stroke-width 3, stroke color matching milestone theme. Each bar on its own y-coordinate swim lane (y=42, y=98, y=154 for 3 milestones). |
| **Checkpoint circles** | `<circle>` r=4–7, fill white, stroke matching milestone color or #888, stroke-width 2.5. Date label as `<text>` above/below. |
| **PoC diamond** | `<polygon>` forming a diamond (4 points, ±11px offset), fill #F4B400, with drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`). |
| **Production diamond** | `<polygon>` same diamond shape, fill #34A853, same drop shadow filter. |
| **Small checkpoint dots** | `<circle>` r=4, fill #999 (no stroke), for minor checkpoints without labels. |
| **Event labels** | `<text>` text-anchor "middle", fill #666, font-size 10px, Segoe UI. Positioned above (y - 16px) or below (y + 24px) the marker to avoid overlap. |

**Date-to-Pixel Mapping:**
- Define a date range via `startDate` and `endDate` in `data.json`
- Formula: `x = ((date - startDate).TotalDays / (endDate - startDate).TotalDays) * svgWidth`
- `svgWidth` = 1560px

### Section 3: Heatmap Area (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1; min-height: 0`
- **Padding:** 10px 44px 10px

#### Heatmap Title

- Font-size 14px, font-weight 700, color #888
- Letter-spacing 0.5px, text-transform uppercase
- Margin-bottom 8px
- Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"

#### Heatmap Grid (`.hm-grid`)

- **Layout:** CSS Grid
- **Grid columns:** `160px repeat(N, 1fr)` where N = number of months (default 4)
- **Grid rows:** `36px repeat(4, 1fr)` — 36px header row + 4 equal data rows
- **Border:** 1px solid #E0E0E0

**Header Row:**

| Cell | Style |
|------|-------|
| **Corner cell** (`.hm-corner`) | Background #F5F5F5, text "STATUS" in 11px bold uppercase #999, border-right 1px #E0E0E0, border-bottom 2px #CCC |
| **Month column headers** (`.hm-col-hdr`) | Background #F5F5F5, font-size 16px bold, centered, border-right 1px #E0E0E0, border-bottom 2px #CCC |
| **Current month header** (`.apr-hdr`) | Background #FFF0D0, text color #C07700 — visually distinct to highlight "now" |

**Data Rows — Color Scheme per Category:**

| Category | Row Header Text Color | Row Header BG | Cell BG | Current Month Cell BG | Bullet Dot Color |
|----------|----------------------|---------------|---------|----------------------|-----------------|
| **Shipped** | #1B7A28 | #E8F5E9 | #F0FBF0 | #D8F2DA | #34A853 |
| **In Progress** | #1565C0 | #E3F2FD | #EEF4FE | #DAE8FB | #0078D4 |
| **Carryover** | #B45309 | #FFF8E1 | #FFFDE7 | #FFF0B0 | #F4B400 |
| **Blockers** | #991B1B | #FEF2F2 | #FFF5F5 | #FFE4E4 | #EA4335 |

**Row Headers** (`.hm-row-hdr`):
- 11px bold uppercase, letter-spacing 0.7px
- Padding 0 12px, border-right 2px solid #CCC, border-bottom 1px #E0E0E0
- Includes emoji prefix: ✅ Shipped, 🔵 In Progress, 🔁 Carryover, 🚫 Blockers

**Data Cells** (`.hm-cell`):
- Padding 8px 12px, border-right 1px #E0E0E0, border-bottom 1px #E0E0E0, overflow hidden
- Each item (`.it`): font-size 12px, color #333, padding 2px 0 2px 12px, line-height 1.35
- Bullet dot via `::before` pseudo-element: 6×6px circle, positioned absolute at left=0, top=7px, background color matching category
- Empty cells: single item with text "–" in color #AAA

### Color System (CSS Custom Properties)

```css
:root {
  --color-shipped: #34A853;
  --color-shipped-bg: #F0FBF0;
  --color-shipped-bg-current: #D8F2DA;
  --color-shipped-header: #E8F5E9;
  --color-shipped-text: #1B7A28;
  --color-progress: #0078D4;
  --color-progress-bg: #EEF4FE;
  --color-progress-bg-current: #DAE8FB;
  --color-progress-header: #E3F2FD;
  --color-progress-text: #1565C0;
  --color-carryover: #F4B400;
  --color-carryover-bg: #FFFDE7;
  --color-carryover-bg-current: #FFF0B0;
  --color-carryover-header: #FFF8E1;
  --color-carryover-text: #B45309;
  --color-blocker: #EA4335;
  --color-blocker-bg: #FFF5F5;
  --color-blocker-bg-current: #FFE4E4;
  --color-blocker-header: #FEF2F2;
  --color-blocker-text: #991B1B;
  --color-poc-milestone: #F4B400;
  --color-prod-milestone: #34A853;
  --color-now-line: #EA4335;
  --color-link: #0078D4;
  --color-border: #E0E0E0;
  --color-border-heavy: #CCC;
  --color-text-primary: #111;
  --color-text-secondary: #888;
  --color-text-muted: #999;
  --color-text-body: #333;
  --color-bg-timeline: #FAFAFA;
  --color-bg-header-cell: #F5F5F5;
  --color-current-month-header: #FFF0D0;
  --color-current-month-text: #C07700;
}
```

### Component Hierarchy

```
Dashboard.razor (single page)
├── DashboardHeader.razor
│   ├── Title + backlog link
│   ├── Subtitle
│   └── Legend (inline SVG icons)
├── TimelineSection.razor
│   ├── Left panel: milestone labels
│   └── Right panel: inline <svg> with calculated positions
│       ├── Month grid lines
│       ├── NOW line
│       ├── Milestone bars (per swim lane)
│       ├── Checkpoint circles
│       ├── PoC diamonds
│       └── Production diamonds
└── HeatmapGrid.razor
    ├── Corner cell ("Status")
    ├── Month column headers (with current month highlight)
    ├── Row headers (Shipped, In Progress, Carryover, Blockers)
    └── HeatmapCell.razor × (4 categories × N months)
        └── Bullet items with colored dots
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Full Dashboard Render**
User navigates to `http://localhost:5000`. The page renders in under 2 seconds showing the complete dashboard: header with project title "Privacy Automation Release Roadmap", subtitle, and legend; the SVG timeline with three milestone swim lanes, month gridlines from Jan–Jun, diamond markers at key dates, and a red dashed "NOW" line at the current date; and the heatmap grid with four color-coded rows and month columns filled with bullet items from `data.json`.

**Scenario 2: User Views Header and Identifies Project Context**
User sees the header bar at the top of the page. The project title is in bold 24px text with a blue "→ ADO Backlog" link. Below it, the subtitle shows the organizational context and current month. On the right, the legend shows four icons: gold diamond (PoC Milestone), green diamond (Production Release), gray circle (Checkpoint), and red vertical bar (Now indicator).

**Scenario 3: User Reads the Timeline to Understand Project Trajectory**
User looks at the timeline section. The left panel shows three labeled milestones (M1, M2, M3) with their descriptions. The right SVG area shows horizontal bars for each milestone with markers at specific dates. The user identifies that M2 has already shipped (production diamond before the NOW line) while M1 and M3 have future production milestones. The NOW line clearly separates past from future.

**Scenario 4: User Scans the Heatmap to Assess Monthly Execution**
User scans the heatmap grid. The current month column (April) is visually highlighted with a golden header and slightly darker cell backgrounds. The user quickly sees that the "Shipped" row has 2 items in April, "In Progress" has 3 items, "Carryover" has 1 item from the previous month, and "Blockers" has 1 item in red. Empty future-month cells show a muted dash.

**Scenario 5: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser navigates to the Azure DevOps backlog URL specified in `data.json`. This only works in the live browser view, not in screenshots.

**Scenario 6: User Edits data.json and Sees Updated Dashboard**
User opens `data.json` in VS Code, adds a new item to the "Shipped" category for April, and saves the file. The `FileSystemWatcher` detects the change, the Blazor app re-reads the JSON, and the dashboard re-renders within 1–2 seconds. The new item appears in the Shipped/April cell with a green bullet dot.

**Scenario 7: User Captures Screenshot for PowerPoint**
User opens Chrome DevTools, enables device emulation at 1920×1080, and uses "Capture screenshot" to save a pixel-perfect PNG of the dashboard. The screenshot shows the entire dashboard with no browser chrome, no scrollbars, and no Blazor framework UI. The user pastes the image directly into a PowerPoint slide.

**Scenario 8: Dashboard Renders with Empty Heatmap Cells**
When a category has no items for a given month (e.g., no blockers in May), the corresponding cell displays a single muted dash character ("–") in color #AAA rather than being completely empty. This preserves grid structure and visual consistency.

**Scenario 9: Dashboard Handles Invalid or Missing data.json**
If `data.json` is missing, malformed, or fails schema validation, the page displays a centered error message in plain text (e.g., "Error loading dashboard data: [description]") instead of the dashboard. No stack trace or Blazor error boundary is shown to the user.

**Scenario 10: Timeline Adjusts to Date Range from Configuration**
User changes the `startDate` and `endDate` fields in `data.json` from a 6-month range to a 4-month range. After refresh, the SVG timeline redraws with fewer month columns, and all milestone markers are repositioned according to the new date-to-pixel mapping. The "NOW" line moves to reflect the current date within the new range.

**Scenario 11: Milestone with Label Above vs. Below**
When two event labels on the same milestone swim lane would overlap, the rendering alternates label placement — one above the bar (y - 16px), the next below (y + 24px) — to maintain readability. In the reference design, "Mar 26 PoC" appears below the M1 bar while "Apr Prod (TBD)" appears above.

**Scenario 12: Dashboard with Variable Number of Months**
User configures `data.json` with 3 months instead of 4. The heatmap grid dynamically adjusts: `grid-template-columns` becomes `160px repeat(3, 1fr)`, and only 3 month column headers render. The current month highlighting applies to whichever month matches `currentMonthIndex`.

## Scope

### In Scope

- Single-page Blazor Server dashboard matching the `OriginalDesignConcept.html` reference design
- `data.json` configuration file as the sole data source for all displayed content
- SVG timeline rendering with date-to-pixel coordinate mapping for milestones, checkpoints, PoC diamonds, production diamonds, and "NOW" line
- CSS Grid heatmap with four color-coded status rows (Shipped, In Progress, Carryover, Blockers) and N month columns
- Current month visual highlighting in both heatmap column header and cell backgrounds
- Milestone label panel on the left side of the timeline
- Header with project title, subtitle, backlog URL link, and legend
- `FileSystemWatcher` for auto-reload when `data.json` changes
- Sample `data.json` with realistic fictional project data for immediate out-of-box demo
- Fixed 1920×1080 viewport targeting for screenshot fidelity
- CSS custom properties for the full color palette
- Strongly-typed C# model classes for JSON deserialization with `System.Text.Json`
- Error handling for missing or invalid `data.json` with user-friendly error display
- `dotnet watch` support for development hot reload

### Out of Scope

- **Authentication / authorization** — no login, no user accounts, no role-based access
- **Database or Entity Framework** — no SQL, no SQLite, no ORM
- **REST API endpoints** — no backend API; the dashboard is a single rendered page
- **Responsive / mobile layout** — fixed 1920×1080 only; no breakpoints
- **Dark mode** — single light theme matching the reference design
- **Export to PDF/PNG** — use browser screenshot functionality instead
- **Historical data tracking** — each report is a point-in-time snapshot; history lives in PowerPoint
- **Multi-user collaboration** — single local user workflow
- **CI/CD pipeline** — local tool; no automated deployment
- **Docker container** — run from source with `dotnet run`
- **Any NuGet packages beyond default Blazor Server template** — zero additional dependencies
- **JavaScript interop or charting libraries** — pure Blazor + inline SVG + CSS Grid
- **Printing / print stylesheet** — screenshot-based workflow only
- **Localization / internationalization** — English only
- **Accessibility (WCAG compliance)** — output is screenshots, not an interactive web application for end users
- **Telemetry or analytics** — no tracking of any kind

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** (localhost, cold start) | < 3 seconds |
| **Page load time** (localhost, warm) | < 1 second |
| **Re-render after data.json change** | < 2 seconds |
| **Memory footprint** | < 100 MB |
| **`data.json` read + deserialize** | < 50 ms for files up to 100 KB |

### Security

- **No authentication or authorization required.** Application runs on localhost only.
- **No sensitive data** stored or transmitted. The `data.json` contains project status information equivalent to what appears in executive PowerPoint slides.
- **No PII** is collected, stored, or processed.
- **No encryption** required for data at rest or in transit.
- **No network exposure** — Kestrel binds to localhost only. No external ports are opened.

### Reliability

- **Availability target:** N/A — local tool run on-demand, not a service.
- **Graceful degradation:** If `data.json` is invalid, display a clear error message rather than crashing.
- **FileSystemWatcher resilience:** Debounce multiple rapid file events; recover gracefully if the file is temporarily locked during save.

### Scalability

- **Not applicable.** Single user, single machine, single page. The data volume is trivially small (< 100 items across all categories).

### Compatibility

- **Target browser:** Google Chrome (latest stable) at 1920×1080 device emulation for screenshots.
- **Backup browser:** Microsoft Edge (Chromium-based).
- **Runtime:** .NET 8 SDK (8.0.400+).
- **OS:** Windows 10/11 (primary), macOS/Linux (should work but not explicitly tested).

### Maintainability

- **Total file count:** Under 15 files.
- **No external dependencies** beyond the default Blazor Server template.
- **CSS matches reference design verbatim** — changes to the design are made by editing CSS, not by learning a component library API.

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Time from clone to running dashboard** | < 5 minutes | Developer runs `git clone`, `dotnet run`, opens browser |
| 2 | **Time from JSON edit to updated screenshot** | < 2 minutes | Edit `data.json`, save, screenshot |
| 3 | **Visual fidelity to reference design** | > 95% match | Side-by-side comparison of screenshot vs. `OriginalDesignConcept.png` at 1920×1080 |
| 4 | **Zero runtime errors** with sample data | 0 errors | `dotnet run` + page load with included `data.json` produces no console errors |
| 5 | **All four heatmap categories populated** | 4/4 categories render correctly | Visual inspection of Shipped, In Progress, Carryover, Blockers rows |
| 6 | **All milestone types render correctly** | Checkpoint, PoC, Production, NOW line all visible | Visual inspection of timeline SVG |
| 7 | **Non-developer can update report** | JSON edit only, no code changes | PM edits `data.json` in Notepad, saves, and dashboard updates |
| 8 | **Screenshot is PowerPoint-ready** | 1920×1080 PNG with no artifacts | Paste into PowerPoint at full-slide size with no resizing needed |

## Constraints & Assumptions

### Technical Constraints

1. **Framework:** Must be built with Blazor Server on .NET 8 (`net8.0`) per existing team stack and tooling.
2. **No additional NuGet packages:** The solution must use only what ships with the default `dotnet new blazorserver` template — `Microsoft.AspNetCore.App` framework reference and `System.Text.Json`.
3. **Fixed viewport:** The page is designed for exactly 1920×1080 pixels. No responsive layout is required or desired — the output is screenshots, not an interactive web application.
4. **No JavaScript:** All rendering (SVG timeline, CSS Grid heatmap) must be achievable with pure Blazor server-side rendering. No JS interop, no charting libraries.
5. **Single project structure:** One `.sln` with one `.csproj` — no microservices, no class libraries, no separate test project in the initial delivery.
6. **System font only:** Segoe UI (Windows system font) — no web fonts to download.

### Timeline Assumptions

1. **Phase 1 (Core MVP):** 1–2 days — static dashboard matching reference design with hardcoded sample data.
2. **Phase 2 (Dynamic Data):** 0.5–1 day — all elements driven by `data.json` with date-to-pixel math.
3. **Phase 3 (Polish):** 0.5 day — component extraction, FileSystemWatcher, CSS custom properties, unit tests.
4. **Total estimated effort:** 2–3.5 developer-days.

### Dependency Assumptions

1. **.NET 8 SDK** is installed on the developer's machine (version 8.0.400+).
2. **Google Chrome** (latest stable) is available for screenshot capture.
3. **The reference design file** (`OriginalDesignConcept.html`) and screenshot (`OriginalDesignConcept.png`) are available in the ReportingDashboard repository and remain the canonical visual spec.
4. **No external services** are required — the app runs entirely on localhost with no network dependencies.
5. **The `data.json` schema** will stabilize during Phase 1 and remain backward-compatible going forward. Schema changes in future iterations must not break existing `data.json` files.
6. **One project per dashboard instance.** To report on a different project, the user swaps the `data.json` file — no multi-project support within a single JSON file.
7. **The user is comfortable editing JSON** in any text editor. A JSON Schema file will be provided for editor autocompletion, but no graphical editor is built.