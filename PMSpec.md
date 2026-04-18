# PM Specification: My Project

## Executive Summary

We are building a single-page executive reporting dashboard — a Blazor Server (.NET 8) web application that reads project data from a `data.json` file and renders a 1920x1080-optimized visual report suitable for screenshot capture and insertion into PowerPoint decks. The dashboard displays a horizontal SVG milestone timeline and a color-coded monthly execution heatmap, matching the visual design established in `OriginalDesignConcept.html`. The goal is to give project leads a zero-friction, zero-auth tool for communicating project health, delivery milestones, and execution status to senior leadership.

---

## Business Goals

1. Enable project leads to produce executive-ready status slides in under 5 minutes by editing `data.json` and taking a browser screenshot.
2. Provide clear visual communication of milestone progress (PoC, production releases, checkpoints) across a configurable date range.
3. Communicate month-over-month execution health across four dimensions: Shipped, In Progress, Carryover, and Blockers.
4. Eliminate manual slide-building effort for recurring executive status updates.
5. Ensure the tool is maintainable by non-engineers — data updates require only JSON edits, no code changes.
6. Deliver a zero-cost, zero-infrastructure solution runnable on any developer workstation with `dotnet run`.

---

## User Stories & Acceptance Criteria

---

**Story 1 — View Executive Dashboard**
**As a** project lead, **I want** to open a browser and immediately see the full project status dashboard, **so that** I can take a screenshot for my executive PowerPoint deck.

- [ ] App launches with `dotnet run` and serves at `http://localhost:5000`
- [ ] Page renders at 1920x1080 and requires no scrolling
- [ ] All three sections (header, timeline, heatmap) are visible simultaneously on screen
- [ ] Page loads in under 2 seconds on localhost
- [ ] No login, authentication prompt, or access control is present
- [ ] Visual matches `OriginalDesignConcept.html` — see **Visual Design Specification §Header**

---

**Story 2 — View Project Header**
**As an** executive, **I want** to see the project name, workstream, reporting period, and a link to the ADO backlog, **so that** I understand the context of the report at a glance.

- [ ] Header displays project title as `<h1>` at 24px bold
- [ ] Subtitle line shows workstream name and current month/year at 12px, color `#888`
- [ ] Optional ADO backlog hyperlink renders in `#0078D4` next to the title
- [ ] Header right side shows legend: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (grey circle), Now line (red)
- [ ] Header is separated from the timeline by a 1px `#E0E0E0` bottom border
- [ ] All values are driven from `data.json` `project` block
- [ ] Visual matches `OriginalDesignConcept.html` — see **Visual Design Specification §Header**

---

**Story 3 — View Milestone Timeline**
**As an** executive, **I want** to see a horizontal timeline showing each milestone track with key event markers, **so that** I can assess delivery pacing and upcoming dates at a glance.

- [ ] Timeline section is 196px tall, background `#FAFAFA`, separated from heatmap by a 2px `#E8E8E8` border
- [ ] Left panel (230px wide) lists milestone labels (e.g., M1, M2, M3) with color-coded text and descriptions
- [ ] SVG canvas spans remaining width (~1560px at 1920px viewport), height 185px
- [ ] Month gridlines render as faint vertical lines (`#bbb`, 40% opacity) with month labels at top
- [ ] Each milestone row renders as a horizontal colored line at its designated Y position
- [ ] Checkpoint events render as hollow circles (r=7, white fill, colored stroke)
- [ ] PoC milestone events render as gold diamond polygons (`#F4B400`) with drop shadow
- [ ] Production release events render as green diamond polygons (`#34A853`) with drop shadow
- [ ] "NOW" line renders as a red dashed vertical line (`#EA4335`, stroke-dasharray 5,3) at today's date
- [ ] Date labels appear above or below markers to avoid overlap
- [ ] All milestone data driven from `data.json` `timeline` block
- [ ] X-position of each event calculated from `(date - startDate).TotalDays / totalDays * svgWidth` using `DateOnly` arithmetic
- [ ] Visual matches `OriginalDesignConcept.html` — see **Visual Design Specification §Timeline**

---

**Story 4 — View Monthly Execution Heatmap**
**As an** executive, **I want** to see a color-coded grid showing what was Shipped, In Progress, Carried Over, and Blocked each month, **so that** I can assess execution momentum and risks.

- [ ] Heatmap section fills remaining vertical space below timeline
- [ ] Section title "MONTHLY EXECUTION HEATMAP" renders in uppercase, 14px, `#888`, bold
- [ ] Grid uses CSS Grid: first column 160px (row headers), remaining columns equal-width (one per month)
- [ ] Grid header row: corner cell "STATUS" label; month name columns at 16px bold; current month column highlighted `#FFF0D0` / `#C07700`
- [ ] Four status rows: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Row headers styled per category color scheme (see **Visual Design Specification §Heatmap**)
- [ ] Each cell displays bullet items as `.it` divs with colored dot indicators (`::before` pseudo-element)
- [ ] Current month cells receive a darker tint variant of their row color
- [ ] Empty cells for future months display a muted dash (`-`) in `#AAA`
- [ ] All cell content driven from `data.json` `heatmap` block
- [ ] Visual matches `OriginalDesignConcept.html` — see **Visual Design Specification §Heatmap**

---

**Story 5 — Edit Data Without Code Changes**
**As a** project lead, **I want** to update project data by editing `data.json` and reloading the page, **so that** I can keep the dashboard current without developer assistance.

- [ ] All visible text, dates, and items are sourced exclusively from `data.json`
- [ ] App reads `data.json` on startup (or on each request — implementation choice)
- [ ] If `data.json` is missing or malformed, app logs a descriptive error to console and displays a clear error page (not a blank page or unhandled exception)
- [ ] `data.json` schema is documented in the repository README
- [ ] Changing milestone dates in `data.json` correctly repositions SVG markers on reload

---

**Story 6 — View Fictional Example Data on First Run**
**As a** developer or new user, **I want** the app to ship with a complete fictional project dataset, **so that** I can see the full dashboard immediately without configuring data.

- [ ] Repository includes a fully populated `data.json` with a fictional project (not placeholder "TODO" text)
- [ ] Fictional data covers at least 3 milestone tracks, 4–6 months, and multiple items per heatmap cell
- [ ] Example data exercises all milestone event types: checkpoint, PoC, production release
- [ ] Example data exercises all four heatmap row types with at least one item each

---

## Visual Design Specification

**Design Reference File:** `OriginalDesignConcept.html` (repository: `azurenerd/ReportingDashboard`)

All engineers **must** read `OriginalDesignConcept.html` in full before implementing any UI component. The rendered screenshot at `docs/design-screenshots/OriginalDesignConcept.png` is the canonical visual target.

---

### Overall Layout

- **Canvas:** Fixed `width: 1920px`, `height: 1080px`, `overflow: hidden`
- **Background:** `#FFFFFF`
- **Font:** `'Segoe UI', Arial, sans-serif` (system font, no CDN)
- **Text color:** `#111`
- **Layout direction:** Vertical flex column (`display: flex; flex-direction: column`)
- **Three vertical sections:** Header → Timeline → Heatmap (no scrolling)

---

### §Header

- **Container:** `padding: 12px 44px 10px`, `border-bottom: 1px solid #E0E0E0`, `display: flex`, `justify-content: space-between`, `align-items: center`
- **Title (`h1`):** `font-size: 24px`, `font-weight: 700`
- **ADO Link:** Inline after title, `color: #0078D4`, no underline
- **Subtitle:** `font-size: 12px`, `color: #888`, `margin-top: 2px`
- **Legend (right side):** Flex row, `gap: 22px`, items at 12px; each item is an icon + label pair:
  - PoC Milestone: 12×12px gold (`#F4B400`) rotated square (diamond)
  - Production Release: 12×12px green (`#34A853`) rotated square
  - Checkpoint: 8×8px circle, `background: #999`
  - Now line: 2×14px vertical bar, `background: #EA4335`

---

### §Timeline

- **Container:** `height: 196px`, `background: #FAFAFA`, `padding: 6px 44px 0`, `border-bottom: 2px solid #E8E8E8`, flex row
- **Left label panel:** `width: 230px`, flex-shrink 0, flex column with `justify-content: space-around`, `padding: 16px 12px 16px 0`, `border-right: 1px solid #E0E0E0`
  - Each milestone label: `font-size: 12px`, `font-weight: 600`, category color; description in `font-weight: 400`, `color: #444`
- **SVG canvas:** `width: 1560px`, `height: 185px`, `overflow: visible`
  - **Month gridlines:** Vertical lines, `stroke: #bbb`, `stroke-opacity: 0.4`, `stroke-width: 1`; month labels at top, `fill: #666`, `font-size: 11px`, `font-weight: 600`
  - **NOW line:** `stroke: #EA4335`, `stroke-width: 2`, `stroke-dasharray: 5,3`; "NOW" label `fill: #EA4335`, `font-size: 10px`, `font-weight: 700`
  - **Milestone track lines:** `stroke-width: 3`, colored per milestone
  - **Checkpoint circles (major):** `r=7`, white fill, colored stroke `stroke-width: 2.5`
  - **Checkpoint circles (minor):** `r=4`, `fill: #999` (solid)
  - **PoC diamonds:** `<polygon>` 11px half-size diamond, `fill: #F4B400`, drop-shadow filter
  - **Production diamonds:** `<polygon>` same shape, `fill: #34A853`, drop-shadow filter
  - **Drop-shadow filter:** `feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`
  - **Date labels:** `font-size: 10px`, `fill: #666`, `text-anchor: middle` (or left-aligned for edge cases)

---

### §Heatmap

- **Container:** Flex column, `padding: 10px 44px 10px`, fills remaining height
- **Section title:** `font-size: 14px`, `font-weight: 700`, `color: #888`, `text-transform: uppercase`, `letter-spacing: 0.5px`, `margin-bottom: 8px`
- **Grid:** `display: grid`, `grid-template-columns: 160px repeat(N, 1fr)`, `grid-template-rows: 36px repeat(4, 1fr)`, `border: 1px solid #E0E0E0`
- **Corner cell:** `background: #F5F5F5`, `font-size: 11px`, `font-weight: 700`, `color: #999`, uppercase, `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`
- **Column headers (non-current):** `background: #F5F5F5`, `font-size: 16px`, `font-weight: 700`, centered, `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`
- **Current month column header:** `background: #FFF0D0`, `color: #C07700`
- **Row headers:** `font-size: 11px`, `font-weight: 700`, uppercase, `letter-spacing: 0.7px`, `padding: 0 12px`, `border-right: 2px solid #CCC`, `border-bottom: 1px solid #E0E0E0`
- **Data cells:** `padding: 8px 12px`, `border-right: 1px solid #E0E0E0`, `border-bottom: 1px solid #E0E0E0`, `overflow: hidden`
- **Bullet items (`.it`):** `font-size: 12px`, `color: #333`, `padding: 2px 0 2px 12px`, relative-positioned; `::before` pseudo is 6×6px circle at left

#### Color Schemes by Row

| Row | Header BG | Header Text | Cell BG | Current Month Cell | Dot Color |
|---|---|---|---|---|---|
| Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

---

## UI Interaction Scenarios

**Scenario 1 — Initial page load**
User navigates to `http://localhost:5000`. The dashboard renders immediately with no loading spinner or skeleton state. All three sections (header, timeline, heatmap) are fully populated from `data.json`. The page fits within 1920×1080 with no scrollbars. The NOW line appears at today's calculated x-position.

**Scenario 2 — Reading the header**
User sees the project title in large bold text at top-left, with the ADO backlog link immediately following in blue. Below it, the workstream name and current month/year appear in grey. At top-right, the legend shows four icon-label pairs in a horizontal row for quick reference.

**Scenario 3 — Reading the milestone timeline**
User scans the timeline section. The left panel shows milestone IDs (M1, M2, M3) with color-coded labels. In the SVG area, horizontal colored lines extend across the date range. The user identifies the NOW (red dashed) line and sees which milestones are past vs. upcoming. Diamond markers indicate PoC completions (gold) and production releases (green). Hollow circles mark checkpoints.

**Scenario 4 — Identifying today's position**
User looks for the red dashed vertical "NOW" line in the SVG timeline and the highlighted column in the heatmap. The current month column header is amber (`#FFF0D0` background, `#C07700` text). Current-month cells in each heatmap row have a slightly deeper tint than non-current cells.

**Scenario 5 — Scanning the heatmap for shipped items**
User scans the green "Shipped" row across all month columns. Each cell lists items with green dot bullets. Future months show a muted dash. Current month cells have a deeper green background.

**Scenario 6 — Identifying blockers**
User scans the red "Blockers" row. Any non-empty blocker cell immediately draws the eye due to the red color scheme. Items are listed with red dot bullets. An empty blocker cell for a month means no active blockers that month.

**Scenario 7 — Identifying carryover**
User scans the amber "Carryover" row to see items that slipped from a previous month. Items carried into the current month appear in the deeper amber current-month cell.

**Scenario 8 — Data-driven rendering (varying column count)**
If `data.json` specifies 3 months instead of 6, the heatmap grid adjusts its `grid-template-columns` to `160px repeat(3, 1fr)`. The SVG timeline adjusts month gridlines and scale accordingly.

**Scenario 9 — Empty state (missing heatmap items)**
A month with no items for a given row displays a single `.it` element containing a dash (`-`) in `color: #AAA`. No empty grid cells are left completely blank, preserving visual grid structure.

**Scenario 10 — Error state (invalid data.json)**
If `data.json` is missing or cannot be parsed, the app displays a plain error page with a descriptive message (e.g., "Could not load data.json: file not found") logged to console at `Error` level. No stack trace is exposed to the browser.

**Scenario 11 — Screenshot capture workflow**
User sets browser zoom to 100%, resizes window to 1920px wide (or uses browser devtools device emulation at 1920×1080). User takes a full-page screenshot. The resulting image matches the design reference exactly for paste into PowerPoint.

**Scenario 12 — No hover/interactive states required**
This is a read-only reporting tool. No hover tooltips, click handlers, or interactive state changes are required. The design is static by intent.

---

## Scope

### In Scope

- Single Blazor Server page (`Dashboard.razor`) at `http://localhost:5000`
- `data.json` schema: project metadata, timeline (milestones + events), heatmap (rows + cells)
- SVG milestone timeline rendered from `data.json` milestone data
- Monthly execution heatmap grid rendered from `data.json` heatmap data
- Current month column highlighting (computed from `data.json` `currentColumn` or system clock)
- NOW line position computed from `data.json` `nowDate` or system date
- Fictional example `data.json` covering 3 milestones, 6 months, all four row types
- Startup validation of `data.json` with descriptive error output
- Basic console logging for startup errors via `ILogger<T>`
- xUnit + bUnit smoke tests for data binding correctness
- README documenting `data.json` schema and `dotnet run` instructions
- CSS and layout pixel-perfect at 1920×1080 per `OriginalDesignConcept.html`

### Out of Scope

- Authentication, authorization, or user sessions of any kind
- Multi-project support or project switching UI
- Database (SQLite, SQL Server, EF Core, etc.)
- Cloud hosting, Docker, IIS, Azure App Service
- Responsive/mobile layout
- Real-time data refresh or WebSocket-based updates beyond Blazor Server's default SignalR
- ADO API integration (link is display-only, not live data)
- Export-to-PDF or programmatic screenshot capture
- Editing `data.json` from within the UI
- User preferences, themes, or configuration UI
- Playwright or automated pixel-regression testing
- Accessibility (WCAG) compliance
- Internationalization or localization
- Any telemetry, analytics, or APM tooling

---

## Non-Functional Requirements

- **Performance:** Page first-load completes in ≤ 2 seconds on localhost on a modern developer workstation
- **Resolution:** Layout renders correctly at exactly 1920×1080; screenshot output must be usable in PowerPoint without cropping
- **Startup reliability:** App must log a clear error and serve an error page (not crash silently) if `data.json` is missing or malformed
- **Data integrity:** All displayed values must come from `data.json`; no hardcoded content in Razor markup (except structural placeholder dashes for empty cells)
- **Security:** App binds to `127.0.0.1` only (configured in `appsettings.json`); no network exposure
- **Dependencies:** Zero external CSS frameworks, zero web font CDNs; all assets served locally
- **Maintainability:** Any non-engineer can update project data by editing `data.json` with a text editor, following the README schema documentation
- **Build:** App builds with `dotnet build` without errors or warnings on .NET 8 SDK
- **Test coverage:** bUnit tests cover: data deserialization from `data.json`, heatmap row/cell rendering, milestone count rendering in timeline label panel

---

## Success Metrics

1. `dotnet run` starts the app and serves a fully rendered dashboard at `http://localhost:5000` with no manual configuration beyond having `data.json` in the working directory.
2. A browser screenshot at 1920×1080 is visually indistinguishable from `OriginalDesignConcept.html` rendered in a browser (same layout, color scheme, typography, grid structure).
3. Editing any field in `data.json` and reloading the browser reflects the change on the dashboard.
4. All bUnit tests pass with `dotnet test`.
5. A project lead can produce a PowerPoint-ready screenshot of a new fictional project in under 5 minutes by editing `data.json` and pressing F5.
6. The app starts without errors when `data.json` is valid; produces a clear error message when `data.json` is absent.

---

## Constraints & Assumptions

**Technical Constraints**
- Target runtime: .NET 8 SDK (must be pre-installed on developer workstation)
- Framework: Blazor Server only — no Blazor WebAssembly, no Razor Pages, no MVC
- Font: Segoe UI must be available as a system font (Windows workstation assumed)
- Layout is fixed-width 1920px — not responsive; this is by design for screenshot fidelity
- SVG timeline is hand-coded inline in Razor — no third-party charting library

**Assumptions**
- One dashboard per running instance; one `data.json` per project
- `data.json` is co-located with the running application (working directory)
- `nowDate` in `data.json` may be set manually or omitted to default to `DateTime.Today`
- `currentColumn` in `data.json` may be set manually or omitted to default to the current calendar month
- Timeline date range is defined by `startDate` and `endDate` in `data.json` (not auto-computed)
- Number of heatmap columns equals the number of entries in `data.json` `heatmap.columns` array
- The tool will be used by one person at a time on their own workstation; no concurrency concerns
- The design reference `OriginalDesignConcept.html` is the authoritative visual specification; all agents must read it before writing any UI code or layout
- The fictional example data ships with the repository and is suitable for demonstration without modification