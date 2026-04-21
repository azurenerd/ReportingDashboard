# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones on a timeline and categorizes work items into Shipped, In Progress, Carryover, and Blockers via a color-coded heatmap grid. The dashboard reads all data from a local `data.json` file and is designed to be screenshot-captured at 1920×1080 for inclusion in PowerPoint decks to executive leadership. Built with .NET 8 Blazor Static SSR, it requires zero authentication, zero cloud dependencies, and zero external databases — just `dotnet run`, open a browser, and take a screenshot.

## Business Goals

1. **Provide executive visibility into project health** — Deliver a single-page visual summary that communicates shipped work, active work, carryover items, and blockers at a glance, eliminating the need for executives to dig through backlogs or attend lengthy status meetings.
2. **Enable screenshot-ready reporting** — Produce a pixel-perfect 1920×1080 page that can be directly captured and pasted into PowerPoint slide decks with no additional formatting or cropping required.
3. **Minimize reporting overhead** — Allow a program manager to update a single `data.json` file and immediately see the refreshed dashboard, reducing weekly reporting effort to under 15 minutes.
4. **Visualize milestone progress on a timeline** — Show major project milestones (PoC, Production Release, Checkpoints) positioned on a horizontal timeline so executives can see what has been achieved, what is upcoming, and where the project stands relative to "now."
5. **Support reuse across projects** — Enable the same dashboard application to render reports for different projects by swapping out the `data.json` file, making it a reusable tool rather than a one-off page.

## User Stories & Acceptance Criteria

### US-1: View Dashboard at a Glance

**As a** program manager, **I want** to open a single URL in my browser and immediately see the full project status dashboard, **so that** I can quickly assess project health without navigating multiple pages.

**Acceptance Criteria:**
- [ ] Navigating to `http://localhost:5000` renders the complete dashboard on a single page
- [ ] The page renders fully within 2 seconds on localhost
- [ ] The page fits within a 1920×1080 viewport with no scrollbars
- [ ] **Visual ref:** The full page layout matches `OriginalDesignConcept.html` — header at top, timeline in middle, heatmap grid filling remaining space

### US-2: See Project Title, Subtitle, and Backlog Link

**As an** executive viewer, **I want** to see the project name, workstream context, and a link to the full backlog at the top of the dashboard, **so that** I know which project I'm looking at and can drill into details if needed.

**Acceptance Criteria:**
- [ ] The header displays the project title in bold 24px font (from `data.json` → `title`)
- [ ] The subtitle displays below the title in 12px gray text (from `data.json` → `subtitle`)
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, linking to the URL in `data.json` → `backlogUrl`
- [ ] **Visual ref:** `.hdr` section of `OriginalDesignConcept.html` — left-aligned title with right-aligned legend

### US-3: Understand the Legend

**As an** executive viewer, **I want** to see a legend explaining the milestone marker types (PoC, Production Release, Checkpoint, Now line), **so that** I can correctly interpret the timeline visualization.

**Acceptance Criteria:**
- [ ] The legend appears in the header bar, right-aligned
- [ ] Four legend items are displayed: gold diamond for "PoC Milestone," green diamond for "Production Release," gray circle for "Checkpoint," and red vertical line for "Now"
- [ ] Legend items use 12px font with inline icon+label layout
- [ ] **Visual ref:** Right side of `.hdr` div in `OriginalDesignConcept.html`

### US-4: View Milestone Timeline with Tracks

**As a** program manager, **I want** to see horizontal timeline tracks for each major workstream with milestones positioned by date, **so that** I can communicate project progress against time to executives.

**Acceptance Criteria:**
- [ ] Each track in `data.json` → `timeline.tracks[]` renders as a horizontal colored line spanning the full timeline width
- [ ] Track labels (e.g., "M1 — Chatbot & MS Role") appear in a fixed-width left sidebar (230px)
- [ ] Milestones render at the correct x-position computed from their date relative to `timeline.startDate` and `timeline.endDate`
- [ ] Milestone types render with correct shapes: `checkpoint` = open circle, `poc` = gold diamond with shadow, `production` = green diamond with shadow
- [ ] Each milestone has a text label positioned above or below the track line
- [ ] Month gridlines appear as light vertical lines with month abbreviation labels at the top
- [ ] **Visual ref:** `.tl-area` section and `<svg>` element in `OriginalDesignConcept.html`

### US-5: See the "Now" Indicator on the Timeline

**As an** executive viewer, **I want** to see a vertical red dashed line on the timeline indicating the current date, **so that** I can immediately understand where "today" falls relative to milestones.

**Acceptance Criteria:**
- [ ] A red (`#EA4335`) dashed vertical line spans the full height of the SVG timeline
- [ ] The line's x-position is computed from `data.json` → `currentDate` using the same date-to-pixel formula as milestones
- [ ] A bold "NOW" label appears at the top of the line in red 10px font
- [ ] **Visual ref:** The dashed red line in the SVG section of `OriginalDesignConcept.html`

### US-6: View the Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a grid showing work items organized by status (Shipped, In Progress, Carryover, Blockers) across monthly columns, **so that** I can quickly see what was delivered, what's active, what slipped, and what's stuck.

**Acceptance Criteria:**
- [ ] The heatmap title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase gray text
- [ ] Column headers display month names from `data.json` → `heatmap.months[]`
- [ ] The current month column (matching `heatmap.currentMonth`) has an amber/gold highlight background (`#FFF0D0`) with "→ Now" appended to the header text
- [ ] Four status rows render: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each cell lists work items as bullet-pointed text entries from `data.json` → `heatmap.categories[].items[month]`
- [ ] Empty cells show a gray dash "—"
- [ ] **Visual ref:** `.hm-wrap` and `.hm-grid` sections of `OriginalDesignConcept.html`

### US-7: Distinguish Status Rows by Color

**As an** executive viewer, **I want** each heatmap row to be color-coded by status category, **so that** I can instantly identify shipped vs. blocked items by visual scanning.

**Acceptance Criteria:**
- [ ] Shipped row: header background `#E8F5E9`, text `#1B7A28`, cell background `#F0FBF0`, current month cell `#D8F2DA`, bullet dot `#34A853`
- [ ] In Progress row: header background `#E3F2FD`, text `#1565C0`, cell background `#EEF4FE`, current month cell `#DAE8FB`, bullet dot `#0078D4`
- [ ] Carryover row: header background `#FFF8E1`, text `#B45309`, cell background `#FFFDE7`, current month cell `#FFF0B0`, bullet dot `#F4B400`
- [ ] Blockers row: header background `#FEF2F2`, text `#991B1B`, cell background `#FFF5F5`, current month cell `#FFE4E4`, bullet dot `#EA4335`
- [ ] **Visual ref:** Row-specific CSS classes (`.ship-*`, `.prog-*`, `.carry-*`, `.block-*`) in `OriginalDesignConcept.html`

### US-8: Load Dashboard Data from JSON File

**As a** program manager, **I want** all dashboard content to be driven by a single `data.json` file, **so that** I can update the report by editing one file without touching code.

**Acceptance Criteria:**
- [ ] The application reads `data.json` from a configurable path (default: `./data.json`, overridable via `appsettings.json`)
- [ ] All displayed text (title, subtitle, backlog URL, milestone labels, work items, month names) comes from `data.json`
- [ ] If `data.json` is missing or malformed, the application displays a clear error message at startup (not a blank page)
- [ ] A `data.sample.json` is included in the repository as a schema reference with fictional example data

### US-9: Take a Screenshot for PowerPoint

**As a** program manager, **I want** the dashboard to render cleanly at exactly 1920×1080 pixels with no browser chrome artifacts, **so that** I can take a full-page screenshot and paste it directly into a PowerPoint slide.

**Acceptance Criteria:**
- [ ] The page body is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] No scrollbars appear when viewed at 100% zoom in a 1920×1080 browser window
- [ ] The page uses Segoe UI font (Windows system font) — no web font loading delays
- [ ] All content renders without layout shifts or async loading indicators
- [ ] A `@media print` stylesheet is included that hides browser-added headers/footers

### US-10: Summary Counts in Row Headers

**As an** executive viewer, **I want** to see a count of items in each status row header (e.g., "SHIPPED (7)"), **so that** I can quickly gauge volume without counting individual entries.

**Acceptance Criteria:**
- [ ] Each row header in the heatmap displays the category name followed by the total item count across all months in parentheses
- [ ] The count updates dynamically based on `data.json` content
- [ ] **Visual ref:** Enhancement over `OriginalDesignConcept.html` — adds count badge to `.hm-row-hdr` elements

### US-11: Milestone Tooltips on Hover

**As a** program manager previewing the dashboard on-screen, **I want** to hover over a milestone marker and see a tooltip with additional detail, **so that** I can verify data accuracy before taking a screenshot.

**Acceptance Criteria:**
- [ ] Hovering over any milestone marker (diamond or circle) in the SVG timeline shows a CSS tooltip
- [ ] The tooltip displays the milestone's full label, date, and type (e.g., "Mar 26 — PoC Milestone")
- [ ] Tooltips are CSS-only (no JavaScript) and do not appear in screenshots
- [ ] Tooltips disappear when the mouse moves away

## Visual Design Specification

> **Canonical design reference:** `OriginalDesignConcept.html` from the `ReportingDashboard` repository, rendered at 1920×1080. See also: `docs/design-screenshots/OriginalDesignConcept.png` for the rendered screenshot. Engineers MUST consult these files and match the visual output pixel-for-pixel before applying any enhancements.

### Overall Page Layout

- **Dimensions:** Fixed `1920px` wide × `1080px` tall, `overflow: hidden`
- **Background:** `#FFFFFF`
- **Font:** `'Segoe UI', Arial, sans-serif` — Windows system font, no loading required
- **Base text color:** `#111`
- **Layout model:** Vertical flex column (`display: flex; flex-direction: column`) with three stacked sections:
  1. Header (flex-shrink: 0, ~50px)
  2. Timeline area (flex-shrink: 0, fixed height: 196px)
  3. Heatmap grid (flex: 1, fills remaining space ~834px)

### Section 1: Header Bar (`.hdr`)

- **Padding:** `12px 44px 10px`
- **Border:** `1px solid #E0E0E0` bottom
- **Layout:** Flexbox, `align-items: center; justify-content: space-between`
- **Left side:**
  - Title: `<h1>` at `24px`, `font-weight: 700`, color `#111`
  - Inline backlog link: `<a>` in `#0078D4`, no underline, preceded by "→" arrow
  - Subtitle: `12px`, `color: #888`, `margin-top: 2px`
- **Right side (Legend):**
  - Horizontal flex with `gap: 22px`
  - Four legend items, each: inline-flex with `gap: 6px`, `font-size: 12px`
    - PoC Milestone: `12×12px` square, `background: #F4B400`, `transform: rotate(45deg)` (renders as diamond)
    - Production Release: `12×12px` square, `background: #34A853`, `transform: rotate(45deg)` (diamond)
    - Checkpoint: `8×8px` circle, `background: #999`, `border-radius: 50%`
    - Now indicator: `2×14px` rectangle, `background: #EA4335`

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed `196px`
- **Background:** `#FAFAFA`
- **Padding:** `6px 44px 0`
- **Border:** `2px solid #E8E8E8` bottom
- **Layout:** Flexbox, `align-items: stretch`

**Left sidebar (track labels):**
- **Width:** `230px`, flex-shrink: 0
- **Border:** `1px solid #E0E0E0` right
- **Padding:** `16px 12px 16px 0`
- **Layout:** flex-column, `justify-content: space-around`
- Each track label: `12px`, `font-weight: 600`, track-colored ID (e.g., "M1" in `#0078D4`), with description in `font-weight: 400`, `color: #444`

**Right area (SVG timeline, `.tl-svg-box`):**
- **Flex:** 1 (fills remaining width)
- **Padding:** `padding-left: 12px; padding-top: 6px`
- **SVG canvas:** `width="1560" height="185"`, `overflow: visible`

**SVG elements:**
- **Month gridlines:** Vertical `<line>` elements at 260px intervals, `stroke: #bbb`, `stroke-opacity: 0.4`
- **Month labels:** `<text>` at top, `fill: #666`, `font-size: 11`, `font-weight: 600`
- **Track lines:** Horizontal `<line>` spanning full width, `stroke-width: 3`, colored per track
- **Checkpoint markers:** `<circle>` with `r="5-7"`, `fill: white`, `stroke` matching track color, `stroke-width: 2.5`
- **PoC milestones:** `<polygon>` diamond (11px radius), `fill: #F4B400`, with drop shadow filter (`feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"`)
- **Production milestones:** `<polygon>` diamond, `fill: #34A853`, same shadow filter
- **Small checkpoints:** `<circle>` with `r="4"`, `fill: #999` (no stroke)
- **Milestone labels:** `<text>`, `fill: #666`, `font-size: 10`, `text-anchor: middle`, positioned above or below the track line
- **"NOW" line:** Vertical dashed `<line>`, `stroke: #EA4335`, `stroke-width: 2`, `stroke-dasharray: 5,3`; label "NOW" in `fill: #EA4335`, `font-size: 10`, `font-weight: 700`

### Section 3: Heatmap Grid (`.hm-wrap` + `.hm-grid`)

**Wrapper (`.hm-wrap`):**
- **Flex:** 1 (fills remaining vertical space)
- **Padding:** `10px 44px 10px`
- **Layout:** flex-column

**Title (`.hm-title`):**
- `font-size: 14px`, `font-weight: 700`, `color: #888`
- `letter-spacing: 0.5px`, `text-transform: uppercase`
- `margin-bottom: 8px`

**Grid (`.hm-grid`):**
- **CSS Grid:** `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months (4 in original design)
- **Grid rows:** `36px` header row + `repeat(4, 1fr)` for data rows
- **Border:** `1px solid #E0E0E0`

**Corner cell (`.hm-corner`):**
- `background: #F5F5F5`, `font-size: 11px`, `font-weight: 700`, `color: #999`, uppercase
- `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`

**Column headers (`.hm-col-hdr`):**
- `font-size: 16px`, `font-weight: 700`, `background: #F5F5F5`
- `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`
- **Current month highlight (`.apr-hdr`):** `background: #FFF0D0`, `color: #C07700`

**Row headers (`.hm-row-hdr`):**
- `font-size: 11px`, `font-weight: 700`, uppercase, `letter-spacing: 0.7px`
- `border-right: 2px solid #CCC`, `border-bottom: 1px solid #E0E0E0`
- Color per category (see color table below)

**Data cells (`.hm-cell`):**
- `padding: 8px 12px`, `border-right: 1px solid #E0E0E0`, `border-bottom: 1px solid #E0E0E0`
- Work items (`.it`): `font-size: 12px`, `color: #333`, `line-height: 1.35`, `padding: 2px 0 2px 12px`
- Bullet dot: `6×6px` circle via `::before` pseudo-element, positioned absolutely at `left: 0, top: 7px`

**Color matrix (exact hex codes):**

| Category | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Bullet Dot |
|----------|--------------|----------------|---------|----------------------|------------|
| Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

### Component-to-Design Mapping

```
Dashboard.razor (full page, route: "/")
├── DashboardHeader.razor         → .hdr div (title + legend)
├── TimelineSection.razor         → .tl-area div
│   ├── Track labels (inline)     → 230px left sidebar
│   └── SVG timeline (inline)     → <svg width="1560" height="185">
└── HeatmapGrid.razor             → .hm-wrap + .hm-grid
    ├── Column headers (inline)   → .hm-col-hdr cells
    └── @foreach category:
        ├── Row header             → .hm-row-hdr
        └── @foreach month:
            └── HeatmapCell.razor  → .hm-cell with .it items
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The page renders the complete dashboard in under 2 seconds: header with project title and legend, timeline with milestone markers, and heatmap grid with color-coded work items. All data is loaded from `data.json` on the server side — there are no loading spinners or progressive rendering. The page appears fully formed.

**Scenario 2: User Views the Header and Identifies the Project**
User sees the project title in large bold text at the top-left (e.g., "Project Phoenix – Cloud Migration Roadmap"). The subtitle below it shows the organizational context and date. On the right side of the header, the legend shows four marker types. The user immediately understands what project this reports on and how to read the visual elements.

**Scenario 3: User Reads the Timeline to Assess Milestone Progress**
User looks at the timeline section and sees 3 horizontal track lines, each representing a major workstream. Each track has markers (circles and diamonds) positioned at specific dates. The red dashed "NOW" line shows the current date. The user can see which milestones are to the left of "NOW" (completed) and which are to the right (upcoming). Track labels on the left identify each workstream (e.g., "M1 — Chatbot & MS Role").

**Scenario 4: User Hovers Over a Milestone Diamond and Sees a Tooltip**
User moves the mouse cursor over a gold diamond marker on the timeline. A CSS tooltip appears near the marker showing: "Mar 26 — PoC Milestone." The tooltip has a white background with a subtle border and shadow. When the user moves the mouse away, the tooltip disappears. This tooltip does not appear in screenshots (CSS `:hover` only).

**Scenario 5: User Scans the Heatmap to Find Blockers**
User looks at the bottom row of the heatmap grid (red-tinted "Blockers" row). The current month column is highlighted with a darker background. Each cell contains bullet-pointed text items describing specific blockers. The user counts the items and sees the total in the row header (e.g., "BLOCKERS (3)").

**Scenario 6: User Identifies the Current Month at a Glance**
User notices one column header in the heatmap has an amber/gold background (`#FFF0D0`) with "→ Now" text. All cells in that column have a slightly darker shade than other columns. This immediately draws the eye to the most relevant month.

**Scenario 7: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser navigates to the Azure DevOps backlog URL specified in `data.json`. No API integration is involved — it is a standard `<a href>` link.

**Scenario 8: User Takes a Screenshot**
User opens Chrome DevTools, sets the viewport to 1920×1080, and uses "Capture full size screenshot." The resulting PNG is a pixel-perfect copy of the dashboard with no browser chrome, scrollbars, or loading artifacts. The user pastes this directly into a PowerPoint slide.

**Scenario 9: User Updates data.json and Refreshes**
User edits `data.json` in a text editor to add a new shipped item under April. User refreshes the browser page. The heatmap now shows the new item in the Shipped/April cell, and the row header count increments by one. No server restart is required (using `dotnet watch` for hot reload).

**Scenario 10: Empty Heatmap Cell**
A month has no items for a given category (e.g., no items shipped in June). The cell displays a single gray dash "—" in `color: #AAA` to indicate intentional emptiness rather than a rendering error.

**Scenario 11: data.json Is Missing or Malformed**
The application starts but `data.json` is not found at the configured path. Instead of rendering a blank page, the dashboard displays a centered error message: "Error: Could not load data.json — file not found at [path]. Please create a data.json file. See data.sample.json for the expected schema." If the JSON is malformed, the error includes the deserialization exception message.

**Scenario 12: Multiple Timeline Tracks**
The `data.json` file defines 4 timeline tracks instead of 3. The SVG height auto-adjusts to accommodate the additional track. Track spacing is computed as `svgHeight / (trackCount + 1)` to maintain even vertical distribution. The left sidebar renders 4 track labels with equal spacing.

## Scope

### In Scope

- Single-page Blazor Static SSR web application targeting .NET 8
- Header component with project title, subtitle, backlog link, and legend
- SVG-based horizontal timeline with date-computed milestone positioning
- Support for three milestone types: checkpoint (circle), PoC (gold diamond), production (green diamond)
- Computed "NOW" vertical line based on `currentDate` from data file
- CSS Grid heatmap with dynamic month columns and four status rows (Shipped, In Progress, Carryover, Blockers)
- Full color coding per the design specification (green/blue/amber/red row themes)
- Current month column highlighting with amber header and darker cell backgrounds
- Summary item counts in row headers (e.g., "SHIPPED (7)")
- CSS-only tooltips on milestone hover
- All data driven from a single `data.json` file
- `data.sample.json` with fictional "Project Phoenix" example data and schema documentation
- Configurable data file path via `appsettings.json`
- Strongly-typed C# models with `System.Text.Json` deserialization
- Startup error handling for missing/malformed `data.json`
- Fixed 1920×1080 layout optimized for screenshot capture
- `@media print` stylesheet for clean printing
- Unit tests for date-to-pixel coordinate calculation (`DateToX`)
- Unit tests for JSON deserialization service
- `.gitignore` entry for `data.json` (to prevent committing real project data)

### Out of Scope

- **Authentication and authorization** — No login, no RBAC, no Azure AD integration
- **Database storage** — No SQL Server, SQLite, Cosmos DB, or any persistence beyond `data.json`
- **API endpoints** — No REST or GraphQL APIs; data is file-based only
- **Real-time updates** — No SignalR push, no WebSocket, no auto-refresh (user manually refreshes)
- **Responsive/mobile layout** — Fixed 1920×1080 only; no mobile breakpoints
- **Dark mode** — Light theme only (future enhancement)
- **Automated screenshot generation** — No Playwright integration (future enhancement, Phase 4)
- **Multi-project switching UI** — No dropdown or navigation between projects (future enhancement)
- **ADO/Jira API integration** — The backlog link is a static `<a href>`, not a live data connection
- **FileSystemWatcher auto-reload** — Not in initial scope; user refreshes manually
- **Docker containerization** — Local `dotnet run` only
- **CI/CD pipeline** — No automated build/deploy
- **Accessibility (WCAG AA)** — Color-blind patterns are a future enhancement; initial version uses color only
- **Internationalization/localization** — English only, no RTL support
- **Data editing UI** — Users edit `data.json` in a text editor, not through the web interface

## Non-Functional Requirements

### Performance
- **Page load time:** < 2 seconds from navigation to fully rendered page on localhost
- **Data file size:** Support `data.json` files up to 1 MB without degradation
- **Memory footprint:** < 100 MB RAM for the running application
- **Startup time:** Application ready to serve requests within 3 seconds of `dotnet run`

### Rendering Fidelity
- **Resolution:** Page MUST render at exactly 1920×1080 with no scrollbars at 100% browser zoom
- **Font rendering:** Segoe UI system font with fallback chain `'Segoe UI', -apple-system, 'Helvetica Neue', Arial, sans-serif`
- **SVG precision:** All coordinate calculations use `double` precision; rounding to 1 decimal place at render time only
- **Browser compatibility:** Chrome 120+ and Edge 120+ on Windows (primary screenshot targets)

### Security
- **No authentication required** — Application is local-only, single-user
- **No network calls** — The application makes zero outbound HTTP requests (no telemetry, no CDN fonts, no analytics)
- **Data protection:** `data.json` is excluded from version control via `.gitignore`; `data.sample.json` contains only fictional data

### Reliability
- **Error handling:** Application must not crash or show a blank page if `data.json` is missing or malformed; display a human-readable error message instead
- **Graceful degradation:** If a timeline track has zero milestones, render the track line with no markers (not an error)
- **Idempotent rendering:** Same `data.json` input always produces identical visual output (deterministic SVG coordinates)

### Maintainability
- **Component isolation:** Each visual section (header, timeline, heatmap) is a separate Blazor component with scoped CSS
- **Zero external dependencies:** No NuGet packages beyond the default Blazor template
- **Schema as contract:** `data.json` schema is defined by C# POCO models; changes to the schema require model updates

## Success Metrics

1. **Visual fidelity:** A side-by-side comparison of a screenshot from the running application and `OriginalDesignConcept.png` shows no visible layout differences at 1920×1080 (validated by manual review).
2. **Data-driven rendering:** Changing any value in `data.json` and refreshing the browser correctly reflects the change in the rendered dashboard — verified for: title text, milestone dates, work item lists, and month columns.
3. **Screenshot workflow:** A program manager can complete the full workflow — edit `data.json`, run the app, take a screenshot, paste into PowerPoint — in under 5 minutes.
4. **Error resilience:** Deleting `data.json` and navigating to the dashboard shows a helpful error message (not a crash or blank page).
5. **Build simplicity:** A new developer can clone the repo and run `dotnet run` with zero additional setup steps (no database migrations, no npm install, no environment variables).
6. **Test coverage:** Unit tests pass for `DateToX()` coordinate calculation (minimum 5 test cases covering start date, end date, mid-range, and edge cases) and `JsonFileDataService` deserialization (valid file, missing file, malformed JSON).

## Constraints & Assumptions

### Technical Constraints
- **Runtime:** .NET 8 LTS (SDK 8.0.x) — must be installed on the developer's machine
- **Rendering mode:** Blazor Static SSR (`--interactivity None`) — no SignalR, no WebAssembly
- **Fixed resolution:** 1920×1080 pixel layout is non-negotiable; the deliverable is a screenshot, not a responsive web app
- **No external CDN:** All resources (CSS, fonts) must be local; the page must render fully offline
- **Windows only:** Segoe UI font dependency assumes Windows; no macOS/Linux testing required
- **CSS from design file:** Initial CSS must be copied verbatim from `OriginalDesignConcept.html`; visual improvements may only be applied after baseline fidelity is confirmed

### Timeline Assumptions
- **Phase 1 (Static replica):** 1 day — port HTML design to Blazor with hardcoded data
- **Phase 2 (Data-driven):** 1 day — implement `data.json` loading and dynamic rendering
- **Phase 3 (Enhancements):** 1 day — add tooltips, summary counts, print stylesheet
- Total estimated effort: **3 working days**

### Dependency Assumptions
- The `OriginalDesignConcept.html` file in the `ReportingDashboard` repository is the authoritative design reference and will not change during development
- The `data.json` schema (defined in research findings) is approved and will not undergo breaking changes during Phase 1–2
- The developer has Chrome or Edge installed for screenshot capture
- The developer has a display or virtual display capable of 1920×1080 rendering

### Data Assumptions
- `data.json` will contain 3–5 timeline tracks with 2–8 milestones each
- The heatmap will display 4–6 month columns
- Each status category will have 0–10 work items per month
- `currentDate` in `data.json` is manually set by the user (not auto-detected from system clock) to ensure reproducible screenshots
- Work item text is short (under 60 characters) and does not need truncation or wrapping logic