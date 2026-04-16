# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones on an SVG timeline and displays work item status in a color-coded heatmap grid, all driven by a local `data.json` configuration file. The dashboard is designed to be screenshot at 1920×1080 for direct insertion into PowerPoint decks, providing executives with an at-a-glance view of what has shipped, what is in progress, what carried over, and what is blocked — without requiring any login, cloud service, or complex tooling.

## Business Goals

1. **Provide executive-ready project visibility** — Deliver a single-page view that communicates project health, milestone progress, and work item status to senior leadership in under 10 seconds of viewing.
2. **Eliminate manual slide creation** — Replace hand-built PowerPoint status slides with a data-driven dashboard that produces pixel-perfect 1920×1080 screenshots ready for executive decks.
3. **Enable rapid status updates** — Allow the project manager to update a single `data.json` file and refresh the browser to produce an updated dashboard, reducing status reporting time from hours to minutes.
4. **Maintain zero operational overhead** — Run entirely on the developer's local machine with no cloud dependencies, no authentication, no database, and $0 infrastructure cost.
5. **Standardize project reporting format** — Establish a reusable template that can be cloned for multiple projects by swapping out `data.json` files, creating consistency across executive communications.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project manager, **I want** to see the project title, organizational context, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project they are looking at and can drill into details if needed.

**Visual Reference:** Header section (`.hdr`) from `OriginalDesignConcept.html`

- [ ] The header displays the project title in 24px bold font from `data.json → project.title`
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, pointing to `data.json → project.backlogUrl`
- [ ] The subtitle line displays the org/workstream/date context from `data.json → project.subtitle` in 12px gray text
- [ ] A legend appears on the right side of the header showing four marker types: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line)

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major project milestones across a 6-month window, **so that** I can understand the project's trajectory and where we currently stand relative to key dates.

**Visual Reference:** Timeline area (`.tl-area` and inline SVG) from `OriginalDesignConcept.html`

- [ ] The left panel (230px wide) lists each milestone ID and label, color-coded to match their timeline swim lane
- [ ] The SVG area (1560px wide, 185px tall) renders one horizontal line per milestone from `data.json → milestones[]`
- [ ] Month grid lines and labels (Jan–Jun) appear at evenly spaced intervals across the top of the SVG
- [ ] Checkpoint events render as open circles on the milestone line
- [ ] PoC events render as gold (#F4B400) diamond shapes with a drop shadow
- [ ] Production events render as green (#34A853) diamond shapes with a drop shadow
- [ ] Each event displays a date label above or below the marker
- [ ] A red dashed vertical "NOW" line is positioned based on `data.json → project.currentDate`, not the system clock
- [ ] The "NOW" label appears in red bold text at the top of the line

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what shipped, what's in progress, what carried over, and what's blocked — organized by month, **so that** I can quickly assess execution health and identify problem areas.

**Visual Reference:** Heatmap grid (`.hm-wrap`, `.hm-grid`) from `OriginalDesignConcept.html`

- [ ] The grid title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase gray text
- [ ] The grid has a 160px row header column and 4 month data columns from `data.json → months[]`
- [ ] A header row displays month names; the current month column (matching `data.json → project.currentMonth`) has a highlighted gold background (#FFF0D0) with amber text (#C07700) and "← Now" annotation
- [ ] The **Shipped** row uses green tones: header (#E8F5E9, text #1B7A28), cells (#F0FBF0, current month #D8F2DA), bullet dots (#34A853)
- [ ] The **In Progress** row uses blue tones: header (#E3F2FD, text #1565C0), cells (#EEF4FE, current month #DAE8FB), bullet dots (#0078D4)
- [ ] The **Carryover** row uses amber tones: header (#FFF8E1, text #B45309), cells (#FFFDE7, current month #FFF0B0), bullet dots (#F4B400)
- [ ] The **Blockers** row uses red tones: header (#FEF2F2, text #991B1B), cells (#FFF5F5, current month #FFE4E4), bullet dots (#EA4335)
- [ ] Each work item displays as a 12px text line with a 6px colored circle bullet (via `::before` pseudo-element)
- [ ] Empty month cells display a gray dash "—" placeholder
- [ ] Items are populated from `data.json → shipped`, `inProgress`, `carryover`, `blockers` dictionaries keyed by month name

### US-4: Update Dashboard Data via JSON

**As a** project manager, **I want** to edit a single `data.json` file and refresh my browser to see the updated dashboard, **so that** I can quickly produce updated status screenshots without touching any code.

- [ ] All dashboard content is driven by `wwwroot/data.json` — no hardcoded data in components
- [ ] The `DashboardDataService` reads and deserializes `data.json` at application startup
- [ ] Changing `data.json` and refreshing the browser reflects the updated data (restart acceptable for v1)
- [ ] The JSON structure matches the documented schema (project, months, timelineRange, milestones, shipped, inProgress, carryover, blockers)
- [ ] Invalid or missing JSON fields degrade gracefully (null checks, empty fallbacks) rather than crashing the page

### US-5: Capture Screenshot for PowerPoint

**As a** project manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars or browser chrome artifacts, **so that** I can take a full-page screenshot and paste it directly into my PowerPoint deck.

- [ ] The `<body>` element is fixed at `width: 1920px; height: 1080px` with `overflow: hidden`
- [ ] No scrollbars appear at 1920×1080 viewport
- [ ] The page renders correctly when using Edge/Chrome DevTools device toolbar at 1920×1080
- [ ] The "Capture full size screenshot" DevTools command produces a clean PNG matching the design
- [ ] No Blazor template boilerplate is visible (no nav sidebar, no counter page, no Bootstrap styles)

### US-6: Run Dashboard Locally with Zero Configuration

**As a** developer, **I want** to run `dotnet run` (or `dotnet watch`) and have the dashboard immediately available at `localhost`, **so that** there is no setup friction or deployment complexity.

- [ ] The solution is a single `.sln` with a single `.csproj`
- [ ] `dotnet run` starts the application and serves the dashboard at the root URL (`/`)
- [ ] No database connection string, API key, or environment variable is required
- [ ] No authentication prompt or login page appears
- [ ] Zero additional NuGet packages beyond the default Blazor Server template are required

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository. See also the rendered screenshot: `docs/design-screenshots/OriginalDesignConcept.png`. All implementations MUST match these visuals exactly.

### Overall Page Layout

- **Viewport:** Fixed 1920×1080 pixels, no scroll, white (#FFFFFF) background
- **Layout direction:** Vertical flex column (`display: flex; flex-direction: column`)
- **Font family:** `'Segoe UI', Arial, sans-serif`
- **Base text color:** #111
- **Link color:** #0078D4, no underline
- **Horizontal page padding:** 44px on left and right (applied via padding on `.hdr`, `.tl-area`, `.hm-wrap`)

### Section 1: Header Bar (`.hdr`)

- **Height:** Auto (content-driven), with `padding: 12px 44px 10px`
- **Layout:** Flexbox row, `align-items: center`, `justify-content: space-between`
- **Bottom border:** 1px solid #E0E0E0
- **Left side:**
  - **Title:** `<h1>` at 24px, font-weight 700. Contains project title text followed by an inline `<a>` link reading "→ ADO Backlog" in #0078D4
  - **Subtitle:** `<div class="sub">` at 12px, color #888, margin-top 2px. Displays org · workstream · date
- **Right side — Legend:** Horizontal flex row with 22px gap, containing 4 legend items:
  1. **PoC Milestone:** 12×12px square rotated 45° (`transform: rotate(45deg)`), fill #F4B400, label "PoC Milestone"
  2. **Production Release:** 12×12px square rotated 45°, fill #34A853, label "Production Release"
  3. **Checkpoint:** 8×8px circle, fill #999, label "Checkpoint"
  4. **Now indicator:** 2×14px vertical bar, fill #EA4335, label "Now (Apr 2026)" (month from data)
  - All legend labels are 12px regular weight

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed 196px, `flex-shrink: 0`
- **Background:** #FAFAFA
- **Bottom border:** 2px solid #E8E8E8
- **Layout:** Flexbox row, `align-items: stretch`, padding `6px 44px 0`

#### Left Panel — Milestone Labels

- **Width:** 230px, flex-shrink 0
- **Right border:** 1px solid #E0E0E0
- **Padding:** 16px 12px 16px 0
- **Layout:** Flex column, `justify-content: space-around`
- **Each milestone label:**
  - Line 1: Milestone ID (e.g., "M1") in 12px, font-weight 600, colored to match milestone (M1: #0078D4, M2: #00897B, M3: #546E7A)
  - Line 2: `<br/>` then milestone description in font-weight 400, color #444

#### Right Panel — SVG Timeline (`.tl-svg-box`)

- **Flex:** 1 (fills remaining width)
- **Padding:** left 12px, top 6px
- **SVG dimensions:** 1560×185px, `overflow: visible`

**SVG Elements:**

- **Drop shadow filter:** `<filter id="sh">` with `feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`
- **Month grid lines:** Vertical lines at x = 0, 260, 520, 780, 1040, 1300 — stroke #BBB, opacity 0.4, width 1
- **Month labels:** Text at x+5 of each grid line, y=14, fill #666, 11px, font-weight 600
- **"NOW" line:** Vertical dashed line (stroke #EA4335, width 2, dasharray 5,3) at x-position calculated from `project.currentDate` via linear interpolation across the date range. "NOW" label in 10px bold #EA4335 at top
- **Milestone swim lanes:** 3 horizontal lines at y = 42, 98, 154
  - Each line: `x1=0 x2=1560`, stroke = milestone color, stroke-width = 3
- **Event markers on each lane:**
  - **Checkpoint:** Circle, r=5–7, fill white, stroke #888 or milestone color, stroke-width 2.5
  - **PoC:** Diamond polygon (11px radius rotated square), fill #F4B400, with drop shadow filter
  - **Production:** Diamond polygon (11px radius), fill #34A853, with drop shadow filter
  - **Small dots:** Circle r=4, fill #999 (for minor checkpoints)
- **Event labels:** 10px text, fill #666, text-anchor middle, positioned above (y-16) or below (y+24) the marker

**Date-to-X Coordinate Formula:**
```
x = (dayOffset / totalDays) × 1560
where dayOffset = eventDate - startDate, totalDays = endDate - startDate
```
Timeline range is configured in `data.json → timelineRange` (default: Jan 1 – Jun 30, 2026).

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Flex:** 1 (fills all remaining vertical space)
- **Padding:** 10px 44px 10px
- **Layout:** Flex column

#### Grid Title (`.hm-title`)

- Font: 14px, font-weight 700, color #888
- Letter-spacing: 0.5px, text-transform uppercase
- Margin-bottom: 8px
- Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"

#### Grid Layout (`.hm-grid`)

- **CSS Grid:** `grid-template-columns: 160px repeat(4, 1fr)` — 5 columns total
- **Grid rows:** `grid-template-rows: 36px repeat(4, 1fr)` — 1 header row + 4 data rows
- **Outer border:** 1px solid #E0E0E0

##### Header Row

| Cell | Class | Background | Text | Border |
|------|-------|-----------|------|--------|
| Corner (row 1, col 1) | `.hm-corner` | #F5F5F5 | "STATUS" in 11px bold #999 uppercase | right: 1px #E0E0E0, bottom: 2px #CCC |
| Month columns (row 1, cols 2–5) | `.hm-col-hdr` | #F5F5F5 (default) | Month name in 16px bold | right: 1px #E0E0E0, bottom: 2px #CCC |
| Current month column header | `.hm-col-hdr.apr-hdr` | #FFF0D0 | Month name + " ← Now" in #C07700 | Same borders |

##### Data Rows — Color Scheme

| Row | Header Class | Header BG | Header Text Color | Cell Class | Cell BG | Current Month Cell BG | Bullet Color |
|-----|-------------|-----------|-------------------|-----------|---------|----------------------|-------------|
| Shipped | `.ship-hdr` | #E8F5E9 | #1B7A28 | `.ship-cell` | #F0FBF0 | #D8F2DA (`.apr`) | #34A853 |
| In Progress | `.prog-hdr` | #E3F2FD | #1565C0 | `.prog-cell` | #EEF4FE | #DAE8FB (`.apr`) | #0078D4 |
| Carryover | `.carry-hdr` | #FFF8E1 | #B45309 | `.carry-cell` | #FFFDE7 | #FFF0B0 (`.apr`) | #F4B400 |
| Blockers | `.block-hdr` | #FEF2F2 | #991B1B | `.block-cell` | #FFF5F5 | #FFE4E4 (`.apr`) | #EA4335 |

##### Row Headers

- 11px bold uppercase, letter-spacing 0.7px
- Prefixed with emoji: ✅ Shipped, 🔄 In Progress, ⏩ Carryover, 🚫 Blockers
- Right border: 2px solid #CCC
- Bottom border: 1px solid #E0E0E0

##### Data Cells

- Padding: 8px 12px
- Right border: 1px solid #E0E0E0 (last column: none)
- Bottom border: 1px solid #E0E0E0
- Overflow: hidden

##### Work Item Text (`.it`)

- Font: 12px, color #333
- Padding: 2px 0 2px 12px (left padding for bullet)
- Line-height: 1.35
- `::before` pseudo-element: 6×6px circle, positioned absolute at left=0 top=7px, colored per row status

### Component Hierarchy

```
Dashboard.razor (page, route "/")
├── Header.razor
│   ├── Title + backlog link
│   ├── Subtitle
│   └── Legend (4 marker type indicators)
├── Timeline.razor
│   ├── Milestone label panel (left, 230px)
│   └── SVG timeline (right, 1560px)
│       ├── Month grid lines + labels
│       ├── NOW line (dashed red)
│       └── Per-milestone: swim lane line + event markers + labels
└── HeatmapGrid.razor
    ├── Grid title
    └── CSS Grid
        ├── Header row (corner + month columns)
        └── 4 status rows (Shipped, In Progress, Carryover, Blockers)
            ├── Row header (color-coded)
            └── 4 month cells (with work item bullets)
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000` (or 5001). The dashboard renders immediately as a single full-page view at 1920×1080. The header shows the project title, subtitle, and legend. The timeline shows all milestones with the "NOW" line positioned at the date specified in `data.json`. The heatmap grid shows all four status rows populated with work items from the JSON. No loading spinner, no skeleton screen — the page renders complete on first paint via server-side rendering.

**Scenario 2: User Views the Header and Identifies the Project**
The user sees the project title in large bold text (e.g., "Project Phoenix — Cloud Migration Platform") with a blue "→ ADO Backlog" link. Below that, the subtitle provides organizational context (e.g., "Enterprise Engineering · Platform Modernization Workstream · April 2026"). On the right, four legend items explain the timeline marker types.

**Scenario 3: User Reads the Milestone Timeline**
The user looks at the timeline area and sees three horizontal swim lanes, each colored differently. The left panel labels each lane (M1: "API Gateway & Auth", M2: "Data Pipeline & ETL", M3: "Dashboard & Reporting"). Diamond markers indicate PoC milestones (gold) and Production releases (green). Circle markers indicate checkpoints. A red dashed "NOW" line crosses all lanes vertically, showing the current date position. Date labels appear near each marker.

**Scenario 4: User Hovers Over a Milestone Diamond**
The user hovers over a gold or green diamond marker on the timeline. A CSS tooltip appears showing the full milestone event details: milestone name, event date, and event type (e.g., "M1 — API Gateway & Auth: Mar 20, 2026 — PoC Milestone"). The tooltip has a white background with a subtle shadow, positioned above the marker.

**Scenario 5: User Clicks the ADO Backlog Link**
The user clicks the "→ ADO Backlog" link in the header. A new browser tab opens navigating to the URL specified in `data.json → project.backlogUrl`. The dashboard remains open in the original tab.

**Scenario 6: User Scans the Heatmap for Current Month Status**
The user's eye is drawn to the highlighted current month column (gold background, amber header text with "← Now" annotation). They can see at a glance: what shipped this month (green row), what's actively being worked on (blue row), what carried over from last month (amber row), and what's blocked (red row). Each item has a small colored bullet dot for visual scanning.

**Scenario 7: User Identifies Blockers**
The user looks at the bottom row of the heatmap (red-tinted "Blockers" row). The current month cell has a deeper red background (#FFE4E4) drawing attention. Each blocker item is listed with a red (#EA4335) bullet dot. The user can count blockers and note their descriptions for follow-up.

**Scenario 8: Empty Month Cells**
For months where a status category has no items (e.g., no blockers in January), the cell displays a single gray dash "—" character, indicating intentional emptiness rather than a loading or rendering error.

**Scenario 9: User Takes a Screenshot**
The user opens Edge DevTools (F12), toggles the device toolbar (Ctrl+Shift+M), sets the viewport to 1920×1080, then runs "Capture full size screenshot" (Ctrl+Shift+P → search). The resulting PNG is a clean, chrome-free image of the dashboard that can be directly pasted into a PowerPoint slide.

**Scenario 10: Data Update Workflow**
The project manager opens `wwwroot/data.json` in a text editor, adds a new item to the "shipped" → "Apr" array (e.g., "API rate limiter v3"), saves the file, and refreshes the browser. The heatmap now shows the new item in the Shipped row under April with a green bullet dot.

**Scenario 11: Error State — Malformed JSON**
If `data.json` contains a syntax error or is missing required fields, the dashboard renders a minimal error message (e.g., "Unable to load dashboard data. Please check data.json for errors.") on a white background rather than showing a Blazor unhandled exception page.

**Scenario 12: Error State — Missing data.json**
If `data.json` does not exist at the expected path, the dashboard displays a friendly message: "No data.json found. Please create wwwroot/data.json with your project data." No stack trace is exposed.

## Scope

### In Scope

- Single-page Blazor Server dashboard rendering at 1920×1080
- Header component with project title, subtitle, ADO backlog link, and milestone legend
- SVG timeline component with dynamically positioned milestone markers (checkpoints, PoC diamonds, production diamonds) and a "NOW" line
- Heatmap grid component with 4 status rows (Shipped, In Progress, Carryover, Blockers) × 4 configurable month columns
- Current month visual highlighting (gold background on header and darker cell backgrounds)
- All data driven from a single `data.json` file — no hardcoded content
- Fictional "Project Phoenix" sample data pre-populated in `data.json`
- CSS extracted verbatim from `OriginalDesignConcept.html` into `dashboard.css`
- Removal of all default Blazor template boilerplate (nav, counter, weather, Bootstrap)
- Minimal `Program.cs` with DI registration for `DashboardDataService`
- Hover tooltips on milestone markers showing date and description
- Null-safe rendering with graceful fallbacks for missing data

### Out of Scope

- **Authentication and authorization** — No login, no RBAC, no identity provider integration
- **Responsive/mobile design** — Fixed 1920×1080 only; no breakpoints or fluid layout
- **Real-time updates** — No SignalR push, no WebSocket data refresh, no auto-polling
- **Database integration** — No Entity Framework, no SQLite, no SQL Server
- **Admin UI for editing data** — JSON is edited manually in a text editor
- **Multi-project routing** — v1 supports a single `data.json` only (multi-project is a future enhancement)
- **Dark mode** — Not in v1; potential future enhancement via query parameter
- **Print stylesheet** — Not in v1
- **Automated testing** — Optional; not required for MVP
- **CI/CD pipeline** — Local development only
- **Containerization / Docker** — Not needed; runs via `dotnet run`
- **Cloud deployment** — No Azure, no AWS, no hosting service
- **JavaScript interop** — Zero JS; pure server-rendered HTML/CSS/SVG
- **CSS frameworks** — No Bootstrap, Tailwind, or third-party CSS
- **Component libraries** — No MudBlazor, Radzen, Syncfusion, or Telerik
- **API endpoints** — No REST/GraphQL API exposed
- **Internationalization / localization** — English only
- **Accessibility (WCAG)** — Not a priority for a screenshot-oriented tool; semantic HTML is used where natural but no formal accessibility audit
- **FileSystemWatcher auto-reload** — Deferred to Phase 2

## Non-Functional Requirements

### Performance

| Metric | Target | Rationale |
|--------|--------|-----------|
| **Initial page load** | < 500ms (localhost) | Single user, local-only; Blazor Server SSR renders immediately |
| **JSON deserialization** | < 50ms | `data.json` is < 10KB; `System.Text.Json` is native and fast |
| **SVG render time** | < 100ms | Maximum ~20 SVG elements; trivial for any modern browser |
| **Screenshot capture** | < 2 seconds | Using browser DevTools built-in capture |

### Security

- **No authentication required** — local-only tool, single user
- **No PII or secrets** in `data.json` — only project status text and URLs
- **No network exposure** — binds to `localhost` only; no external-facing ports
- **No HTTPS certificate required** — `http://localhost:5000` is acceptable for local use
- **Antiforgery middleware** included by default (Blazor Server requirement) but no forms to protect

### Reliability

- **Availability target:** N/A — runs on-demand locally, not a production service
- **Error handling:** Malformed `data.json` must not crash the application; display a user-friendly error message instead
- **Data durability:** `data.json` is a flat file on disk; no backup strategy required (user manages their own files)

### Scalability

- **Not applicable** — single user, single machine, single page
- **Maximum data volume:** ~50 work items across all status categories and months; grid layout accommodates this without scrolling within the 1080px height

### Maintainability

- **Single `.sln`, single `.csproj`** — no multi-project complexity
- **Zero external NuGet dependencies** — only the default Blazor Server template packages
- **CSS in one file** — `dashboard.css` contains all styles; no CSS-in-JS, no CSS modules, no Sass
- **Data model matches JSON 1:1** — `System.Text.Json` deserialization with no transforms or adapters

## Success Metrics

| # | Metric | Target | Measurement |
|---|--------|--------|-------------|
| 1 | **Dashboard renders from `data.json`** | All sections (header, timeline, heatmap) populated correctly from JSON data | Visual inspection: compare rendered page against `OriginalDesignConcept.html` design |
| 2 | **Pixel-perfect at 1920×1080** | Screenshot matches design reference with no scrollbars, overflow, or layout shifts | DevTools screenshot at 1920×1080 compared side-by-side with `OriginalDesignConcept.png` |
| 3 | **Zero-config startup** | `dotnet run` → browser → full dashboard in < 30 seconds | Manual test from clean clone |
| 4 | **Data update turnaround** | Edit JSON → refresh → updated dashboard in < 10 seconds | Manual test: add an item, save, refresh |
| 5 | **No boilerplate visible** | No Blazor default nav, sidebar, counter, weather, or Bootstrap elements appear | Visual inspection of rendered page |
| 6 | **Timeline accuracy** | Milestone markers positioned within ±5px of expected date-to-coordinate mapping | Compare marker x-positions against manual calculation using the `DateToX` formula |
| 7 | **Current month highlighting** | The current month column is visually distinct (gold header, darker cell backgrounds) | Visual inspection: current month column stands out from adjacent months |
| 8 | **Graceful error handling** | Malformed `data.json` shows a friendly error, not a stack trace | Test: introduce a JSON syntax error, load the page |

## Constraints & Assumptions

### Technical Constraints

1. **Technology stack:** Blazor Server on .NET 8 — mandated; no alternative frameworks (React, Vue, plain HTML generators) may be used
2. **No external component libraries:** MudBlazor, Radzen, Syncfusion, Telerik, and similar are explicitly prohibited to avoid theming conflicts and unpredictable screenshot output
3. **No charting libraries:** The SVG timeline is hand-crafted using SVG primitives (line, circle, polygon, text) — no Chart.js, Highcharts, or D3.js
4. **No database:** All data comes from `data.json`; no ORM, no connection strings, no migrations
5. **No JavaScript:** The entire page must render server-side with zero JS interop
6. **Fixed viewport:** The page is designed for exactly 1920×1080; responsive behavior is not required and must not be added
7. **CSS verbatim from design:** Styles must be extracted from `OriginalDesignConcept.html` without "modernization" or framework replacement
8. **Windows host:** Primary development and runtime environment is Windows (Segoe UI font assumed available)

### Timeline Assumptions

1. **MVP delivery:** 1–2 hours of implementation time for a developer familiar with Blazor
2. **Phase 2 polish (optional):** Additional 30 minutes for FileSystemWatcher, tooltips, and print CSS
3. **Phase 3 multi-project (optional):** Additional 30 minutes for route-based JSON file selection

### Dependency Assumptions

1. **.NET 8 SDK** is installed on the development machine (`dotnet --version` returns 8.0.x)
2. **Edge or Chrome** is available for viewing and screenshotting the dashboard
3. **No network connectivity** is required at runtime — the app runs entirely offline
4. **`data.json` is manually maintained** by the project manager in a text editor — no automated data pipeline feeds into it
5. **Single user** — the dashboard is not shared via a URL; only the project manager views it locally and shares screenshots
6. **The design reference** (`OriginalDesignConcept.html` and `OriginalDesignConcept.png`) from the ReportingDashboard repository is the canonical visual specification; all discrepancies between the spec text and the HTML/PNG should be resolved in favor of the visual design

### Open Questions (Defaulted for v1)

| Question | Default Decision for v1 |
|----------|------------------------|
| Timeline date range configurable? | Yes — driven by `data.json → timelineRange.startDate/endDate` |
| Number of milestones fixed at 3? | No — render dynamically from `data.json → milestones[]` array (3 is the typical case) |
| Heatmap month columns configurable? | Yes — driven by `data.json → months[]` array (typically 4) |
| ADO Backlog link in JSON? | Yes — `data.json → project.backlogUrl` |
| Multiple project support? | Deferred to Phase 3 |
| Who edits `data.json`? | Project manager, manually in a text editor |
| Colors hardcoded or configurable? | Milestone colors configurable in JSON (`milestones[].color`); heatmap row colors hardcoded in CSS per the design |