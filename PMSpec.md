# PM Specification: Executive Reporting Dashboard

**Document Version:** 1.0
**Date:** April 16, 2026
**Author:** Program Management
**Status:** Draft
**Design Reference:** `OriginalDesignConcept.html` (canonical), `C:/Pics/ReportingDashboardDesign.png` (aspirational)

---

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes a project's milestone timeline, shipped deliverables, in-progress work, carryover items, and blockers in a clean, screenshot-friendly format optimized for 1920×1080 PowerPoint slides. The dashboard reads all data from a local `data.json` configuration file, requires zero authentication or cloud infrastructure, and is designed to be the fastest path from "project status update" to "polished executive slide."

---

## Business Goals

1. **Reduce status reporting friction** — Enable a program manager to produce a polished, executive-ready project status visual in under 5 minutes by editing a single JSON file and taking a browser screenshot.
2. **Standardize project visibility** — Provide a consistent, repeatable visual format for communicating milestone progress, shipped work, active work, carryover, and blockers across all projects presented to leadership.
3. **Eliminate tooling overhead** — Deliver a zero-dependency, local-only tool that requires no licenses, no cloud accounts, no database, and no authentication — just `dotnet run` and a browser.
4. **Enable rapid iteration** — Allow the dashboard data to be updated and re-rendered in seconds via hot-reload, supporting last-minute updates before executive reviews.
5. **Produce pixel-perfect screenshots** — Ensure the rendered page at 1920×1080 is visually clean enough to paste directly into a PowerPoint deck without post-processing.

---

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** program manager, **I want** to see the project title, subtitle (team/workstream/date), and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are looking at.

**Visual Reference:** `OriginalDesignConcept.html` — Header section (`.hdr`)

**Acceptance Criteria:**
- [ ] The page displays a bold project title (24px, weight 700) at the top-left
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, styled in `#0078D4`
- [ ] A subtitle line below the title shows team name, workstream, and current month/year in `#888`, 12px
- [ ] A legend appears at the top-right showing icons for: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line)
- [ ] All text values are driven from `data.json` fields: `title`, `subtitle`, `backlogUrl`

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major milestone streams with their key dates and current progress markers, **so that** I can quickly assess where the project stands relative to planned milestones.

**Visual Reference:** `OriginalDesignConcept.html` — Timeline area (`.tl-area` and inline SVG)

**Acceptance Criteria:**
- [ ] A left sidebar (230px wide) lists each milestone stream with its ID (e.g., "M1") and name, each in its stream-specific color
- [ ] The main timeline area renders an SVG (1560×185px) with horizontal lines per stream
- [ ] Month labels (Jan–Jun) appear along the top with vertical gridlines
- [ ] Checkpoint milestones render as open circles with a border
- [ ] PoC milestones render as gold (`#F4B400`) diamond shapes with a drop shadow
- [ ] Production milestones render as green (`#34A853`) diamond shapes with a drop shadow
- [ ] Each milestone has a date label positioned above or below the marker
- [ ] A dashed red (`#EA4335`) vertical "NOW" line appears at the position corresponding to `currentDate`
- [ ] The number of streams is dynamic — driven by the `milestoneStreams` array in `data.json`
- [ ] Date-to-pixel positioning uses linear interpolation between `timelineStart` and `timelineEnd`

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked — organized by month, **so that** I can assess execution velocity and identify risks at a glance.

**Visual Reference:** `OriginalDesignConcept.html` — Heatmap section (`.hm-wrap`, `.hm-grid`)

**Acceptance Criteria:**
- [ ] A section title "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" appears above the grid in uppercase, 14px, `#888`
- [ ] The grid has a fixed left column (160px) for row category headers and N dynamic columns for months
- [ ] Column headers display month names; the current month column is highlighted with amber background (`#FFF0D0`) and amber text (`#C07700`)
- [ ] Four category rows are displayed: Shipped (green), In Progress (blue), Carryover (yellow/amber), Blockers (red)
- [ ] Each row header uses the category's accent color and background tint with an emoji prefix (✅, 🔵, ⏳, 🔴 or similar)
- [ ] Data cells display item names as bulleted list items with a 6px colored circle bullet
- [ ] Current-month cells use a darker shade of the row's background color
- [ ] Empty cells display a muted dash ("—") in `#AAA`
- [ ] The number of columns is dynamic — driven by the `heatmap.columns` array in `data.json`
- [ ] The grid fills the remaining vertical space of the 1080px viewport

### US-4: Load Dashboard Data from JSON

**As a** program manager, **I want** the dashboard to read all display data from a single `data.json` file, **so that** I can update the dashboard content by editing one file without touching code.

**Acceptance Criteria:**
- [ ] The application reads `data.json` from the `wwwroot/data/` directory on startup
- [ ] The JSON is deserialized into strongly-typed C# records using `System.Text.Json`
- [ ] If `data.json` is missing or malformed, the application displays a clear error message (not a stack trace)
- [ ] A `data.sample.json` file is provided with inline comments documenting every field
- [ ] Changing `data.json` and restarting the app (or using hot-reload) reflects the new data immediately

### US-5: Generate Screenshot-Ready Output

**As a** program manager, **I want** the rendered page to be exactly 1920×1080 pixels with no scrollbars and no extraneous UI chrome, **so that** I can take a browser screenshot and paste it directly into PowerPoint.

**Acceptance Criteria:**
- [ ] The `<body>` element is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] No navigation sidebar, footer, or Blazor default chrome is visible
- [ ] No scrollbars appear at the default zoom level
- [ ] The page renders identically in Chrome and Edge at 100% zoom on a 1920×1080 display
- [ ] The Segoe UI font is used throughout (system font on Windows)

### US-6: Run Dashboard Locally with Zero Configuration

**As a** developer, **I want** to launch the dashboard with a single `dotnet run` command and no external dependencies, **so that** I can get it running on any Windows machine with the .NET 8 SDK.

**Acceptance Criteria:**
- [ ] `dotnet run` starts the application and serves it at `https://localhost:5001` (or configured port)
- [ ] No NuGet packages beyond the default Blazor Server template are required
- [ ] No database, Docker, or external service is needed
- [ ] `dotnet watch` enables hot-reload for CSS and Razor component changes

---

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html`
**Visual Screenshot:** `OriginalDesignConcept.png` (rendered at 1920×1080)

Engineers MUST consult the original HTML file and match the rendered screenshot pixel-for-pixel. The following describes every visual section in detail.

### Overall Layout

- **Viewport:** Fixed `1920px × 1080px`, no scroll, white (`#FFFFFF`) background
- **Font Family:** `'Segoe UI', Arial, sans-serif`
- **Primary Text Color:** `#111`
- **Layout Direction:** Vertical flex column (`display: flex; flex-direction: column`)
- **Horizontal Padding:** `44px` on left and right for all major sections

### Section 1: Header Bar (`.hdr`)

- **Height:** Auto (content-driven, approximately 50px)
- **Layout:** Flexbox row, `align-items: center`, `justify-content: space-between`
- **Bottom Border:** `1px solid #E0E0E0`
- **Padding:** `12px 44px 10px`

**Left Side:**
- **Title:** `<h1>` at `24px`, `font-weight: 700`, color `#111`
- **Backlog Link:** Inline `<a>` within the h1, color `#0078D4`, no underline, prefixed with "→"
- **Subtitle:** `<div class="sub">` at `12px`, color `#888`, `margin-top: 2px`

**Right Side — Legend:**
- Horizontal flex with `gap: 22px`
- Four legend items, each as a flex row with `gap: 6px`, `font-size: 12px`:
  1. **PoC Milestone:** 12×12px square, `background: #F4B400`, rotated 45° (diamond shape)
  2. **Production Release:** 12×12px square, `background: #34A853`, rotated 45° (diamond shape)
  3. **Checkpoint:** 8×8px circle, `background: #999`
  4. **Now Indicator:** 2×14px vertical bar, `background: #EA4335`, label "Now (Apr 2026)"

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed `196px`
- **Background:** `#FAFAFA`
- **Bottom Border:** `2px solid #E8E8E8`
- **Layout:** Flexbox row, `align-items: stretch`
- **Padding:** `6px 44px 0`

**Left Sidebar (Stream Labels):**
- **Width:** `230px`, flex-shrink 0
- **Border Right:** `1px solid #E0E0E0`
- **Padding:** `16px 12px 16px 0`
- **Layout:** Flex column, `justify-content: space-around`
- Each stream label: `12px`, `font-weight: 600`, `line-height: 1.4`
  - Stream ID (e.g., "M1") in stream color (`#0078D4`, `#00897B`, `#546E7A`)
  - Stream name below in `font-weight: 400`, color `#444`

**Right SVG Area (`.tl-svg-box`):**
- **SVG Dimensions:** `width="1560" height="185"`, `overflow: visible`
- **Padding:** `padding-left: 12px; padding-top: 6px`

**SVG Elements:**
- **Month Gridlines:** Vertical lines at equal intervals (260px apart), `stroke: #bbb`, `stroke-opacity: 0.4`
- **Month Labels:** `font-size: 11`, `font-weight: 600`, `fill: #666`, positioned 5px right of gridline at y=14
- **Stream Lines:** Horizontal lines spanning full width, `stroke-width: 3`, color matches stream
  - M1 line at y=42, color `#0078D4`
  - M2 line at y=98, color `#00897B`
  - M3 line at y=154, color `#546E7A`
- **Milestone Markers:**
  - *Checkpoint:* `<circle>` with `fill: white`, `stroke: #888` or stream color, `stroke-width: 2.5`, radius 5–7px
  - *PoC:* `<polygon>` diamond (11px radius), `fill: #F4B400`, with drop shadow filter
  - *Production:* `<polygon>` diamond (11px radius), `fill: #34A853`, with drop shadow filter
  - *Small dots:* `<circle>` radius 4, `fill: #999` (for minor checkpoints)
- **Milestone Labels:** `<text>` at `font-size: 10`, `fill: #666`, `text-anchor: middle`, positioned 16px above or below the marker
- **NOW Line:** Dashed vertical line, `stroke: #EA4335`, `stroke-width: 2`, `stroke-dasharray: 5,3`; label "NOW" in `font-size: 10`, `font-weight: 700`, `fill: #EA4335`
- **Drop Shadow Filter:** `<filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter>`

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1` (fills remaining vertical space)
- **Padding:** `10px 44px 10px`

**Section Title (`.hm-title`):**
- `font-size: 14px`, `font-weight: 700`, `color: #888`
- `letter-spacing: 0.5px`, `text-transform: uppercase`
- `margin-bottom: 8px`

**Grid Container (`.hm-grid`):**
- **CSS Grid:** `grid-template-columns: 160px repeat(N, 1fr)` where N = number of month columns
- **Grid Rows:** `36px` for header row, then `repeat(4, 1fr)` for data rows
- **Border:** `1px solid #E0E0E0`

**Corner Cell (`.hm-corner`):**
- `background: #F5F5F5`, centered text "STATUS" at `11px`, `font-weight: 700`, `color: #999`, uppercase
- `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`

**Column Headers (`.hm-col-hdr`):**
- `font-size: 16px`, `font-weight: 700`, `background: #F5F5F5`
- `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`
- **Current month highlight (`.apr-hdr`):** `background: #FFF0D0`, `color: #C07700`

**Row Headers (`.hm-row-hdr`):**
- `font-size: 11px`, `font-weight: 700`, uppercase, `letter-spacing: 0.7px`
- `border-right: 2px solid #CCC`, `border-bottom: 1px solid #E0E0E0`
- Category-specific colors:

| Category | Header BG | Header Text | Row Class |
|----------|-----------|-------------|-----------|
| Shipped | `#E8F5E9` | `#1B7A28` | `.ship-hdr` |
| In Progress | `#E3F2FD` | `#1565C0` | `.prog-hdr` |
| Carryover | `#FFF8E1` | `#B45309` | `.carry-hdr` |
| Blockers | `#FEF2F2` | `#991B1B` | `.block-hdr` |

**Data Cells (`.hm-cell`):**
- `padding: 8px 12px`, `border-right: 1px solid #E0E0E0`, `border-bottom: 1px solid #E0E0E0`
- Category-specific background colors:

| Category | Default BG | Current Month BG | Bullet Color |
|----------|-----------|------------------|--------------|
| Shipped | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

**Item Text (`.it`):**
- `font-size: 12px`, `color: #333`, `padding: 2px 0 2px 12px`, `line-height: 1.35`
- `::before` pseudo-element: 6×6px circle bullet, positioned `left: 0`, `top: 7px`, colored per category

### Color Palette Reference

| Token | Hex | Usage |
|-------|-----|-------|
| `--shipped-accent` | `#34A853` | Shipped bullet, production diamond |
| `--shipped-bg` | `#F0FBF0` | Shipped cell background |
| `--shipped-bg-current` | `#D8F2DA` | Shipped cell, current month |
| `--shipped-header` | `#E8F5E9` | Shipped row header |
| `--progress-accent` | `#0078D4` | In-progress bullet, links, M1 stream |
| `--progress-bg` | `#EEF4FE` | In-progress cell background |
| `--progress-bg-current` | `#DAE8FB` | In-progress cell, current month |
| `--carryover-accent` | `#F4B400` | Carryover bullet, PoC diamond |
| `--carryover-bg` | `#FFFDE7` | Carryover cell background |
| `--carryover-bg-current` | `#FFF0B0` | Carryover cell, current month |
| `--blocker-accent` | `#EA4335` | Blocker bullet, NOW line |
| `--blocker-bg` | `#FFF5F5` | Blocker cell background |
| `--blocker-bg-current` | `#FFE4E4` | Blocker cell, current month |
| `--header-bg` | `#F5F5F5` | Column/corner header |
| `--current-month-header` | `#FFF0D0` | Current month column header |
| `--text-primary` | `#111` | Body text |
| `--text-secondary` | `#666` | SVG labels, subtle text |
| `--text-muted` | `#888` / `#999` | Subtitle, section titles |
| `--border` | `#E0E0E0` | Grid borders |
| `--border-heavy` | `#CCC` | Row/column header separators |

---

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `https://localhost:5001`. The dashboard renders in under 2 seconds, displaying the full 1920×1080 layout with header, timeline, and heatmap — all populated from `data.json`. No loading spinner is needed for this local-file scenario.

**Scenario 2: User Views the Header**
User sees the project title in bold at the top-left (e.g., "Privacy Automation Release Roadmap") with a blue "→ ADO Backlog" link. Below it, a subtle gray subtitle shows the team, workstream, and current month. On the right, a legend explains the four marker types used in the timeline.

**Scenario 3: User Reads the Milestone Timeline**
User looks at the timeline area and sees three horizontal stream lines (M1, M2, M3) with labeled milestones. The left sidebar identifies each stream by ID and name. A dashed red "NOW" line clearly marks the current date, allowing the user to instantly see which milestones are past, present, and future.

**Scenario 4: User Hovers Over a Milestone Diamond**
User hovers over a gold or green diamond milestone marker on the timeline. A CSS-only tooltip appears showing the milestone date and label (e.g., "Mar 26 — PoC Milestone"). The tooltip disappears when the mouse moves away. *(Phase 2 enhancement — not required for MVP.)*

**Scenario 5: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser navigates to the URL specified in `data.json` field `backlogUrl`, opening in the same tab (or new tab if configured).

**Scenario 6: User Scans the Heatmap Grid**
User looks at the heatmap and immediately identifies the current month column (highlighted in amber). They scan each row: green for shipped items, blue for in-progress, amber for carryover, red for blockers. The bulleted items in each cell provide specific deliverable names.

**Scenario 7: User Identifies an Empty Month**
User sees a month column with no shipped items. The cell displays a muted dash ("—") in light gray (`#AAA`), clearly indicating "nothing here" rather than a rendering bug.

**Scenario 8: User Views a Data-Heavy Month**
User sees a month with many items in a single category. Items are rendered as a compact bulleted list (12px font, 1.35 line-height). If items overflow the cell height, they are clipped via `overflow: hidden` — the fixed 1080px layout does not scroll.

**Scenario 9: User Updates `data.json` and Refreshes**
User edits `data.json` to add a new shipped item for April. They save the file and either restart the app (`dotnet run`) or rely on hot-reload (`dotnet watch`). Refreshing the browser shows the updated data immediately.

**Scenario 10: User Encounters a Malformed `data.json`**
User accidentally introduces a JSON syntax error or removes a required field. On next page load, the dashboard displays a clear, human-readable error message (e.g., "Error loading data.json: Missing required field 'title'") instead of a raw exception stack trace.

**Scenario 11: User Takes a Screenshot**
User presses `Win+Shift+S` (or uses browser DevTools screenshot) to capture the full page at 1920×1080. The captured image has no scrollbars, no browser chrome artifacts, and is ready to paste into PowerPoint.

**Scenario 12: Dynamic Column Count**
User configures `data.json` with 6 months of data instead of 4. The heatmap grid automatically expands to 6 month columns using CSS Grid `repeat(6, 1fr)`, and the current month highlight shifts to the correct column based on `currentColumnIndex`.

**Scenario 13: Dynamic Stream Count**
User adds a 4th milestone stream (M4) to the `milestoneStreams` array. The timeline sidebar adds a 4th label, and the SVG renders a 4th horizontal line with its milestones. Stream vertical spacing adjusts automatically.

---

## Scope

### In Scope

- Single-page Blazor Server application rendering the executive dashboard
- Data-driven rendering from a local `data.json` file
- Header section with project title, subtitle, backlog link, and legend
- SVG milestone timeline with dynamic streams, milestones, and "NOW" indicator
- CSS Grid heatmap with Shipped, In Progress, Carryover, and Blockers rows
- Dynamic column count (months) and row item count driven by JSON data
- Current month visual highlighting (amber column header, darker cell backgrounds)
- Fixed 1920×1080 viewport optimized for PowerPoint screenshots
- CSS ported from `OriginalDesignConcept.html` with CSS custom properties for theming
- `data.sample.json` with documentation of all fields
- Error handling for missing or malformed `data.json`
- Hot-reload support via `dotnet watch`

### Out of Scope

- **Authentication / authorization** — No login, no RBAC, no middleware
- **Database** — No SQLite, SQL Server, CosmosDB, or any persistent store beyond the JSON file
- **REST API endpoints** — No controllers, no API layer
- **Responsive / mobile layout** — Fixed 1920×1080 only
- **Admin panel or CRUD forms** — JSON is edited in a text editor
- **Logging infrastructure** — No structured logging, no Application Insights
- **Docker / containerization** — Not needed for local-only tool
- **CI/CD pipeline** — No GitHub Actions, no Azure DevOps pipelines
- **Multi-user concurrency** — Single user, single browser tab assumed
- **Internationalization / localization** — English only
- **Accessibility (WCAG compliance)** — Not a requirement for screenshot-targeted output
- **Print stylesheet** — Screenshots, not printing
- **Dark mode** — Single light theme matching the design reference
- **Playwright screenshot automation** — Deferred to Phase 3 (optional)

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** | < 2 seconds on localhost (cold start) |
| **Hot-reload refresh** | < 1 second after CSS/Razor change |
| **JSON deserialization** | < 50ms for typical `data.json` (< 50KB) |
| **Memory usage** | < 100MB RSS for the Kestrel process |

### Security

- Application binds to `localhost` only by default (Kestrel default behavior)
- No authentication middleware — explicitly excluded
- `data.json` should have appropriate filesystem permissions if it contains sensitive project data
- No PII is stored or processed
- No secrets or credentials in `data.json`

### Reliability

- Application should start successfully with a valid `data.json` on every launch
- Application should display a clear error message (not crash) if `data.json` is missing or invalid
- No uptime SLA — this is a local developer tool, not a production service

### Compatibility

- **Browsers:** Chrome (latest) and Edge (latest) on Windows
- **OS:** Windows 10/11 with .NET 8 SDK installed
- **Display:** 1920×1080 at 100% scaling
- **Font:** Segoe UI (pre-installed on Windows)

### Maintainability

- Total solution should be under 10 files
- Zero NuGet dependencies beyond default Blazor Server template
- CSS should be under 150 lines
- No JavaScript required

---

## Success Metrics

| # | Metric | Target | Measurement |
|---|--------|--------|-------------|
| 1 | **Visual fidelity** | Dashboard screenshot is visually indistinguishable from `OriginalDesignConcept.png` at 1920×1080 | Side-by-side manual comparison |
| 2 | **Data-driven rendering** | All visible text, milestones, and heatmap items are sourced from `data.json` — zero hardcoded content | Change `data.json` and verify all content updates |
| 3 | **Zero-config launch** | `dotnet run` on a clean machine with .NET 8 SDK starts the dashboard with no errors | Manual verification |
| 4 | **Screenshot quality** | A browser screenshot of the page is usable in a PowerPoint slide without cropping or editing | Paste into PowerPoint and present |
| 5 | **Update turnaround** | A program manager can update `data.json` and produce a new screenshot in under 5 minutes | Timed walkthrough |
| 6 | **File count** | Total solution files (excluding `bin/`, `obj/`, `.vs/`) ≤ 15 | `find` command |
| 7 | **Dependency count** | Zero additional NuGet packages beyond the default template | Inspect `.csproj` |

---

## Constraints & Assumptions

### Technical Constraints

- **Framework:** .NET 8 LTS with Blazor Server — stack decision is final
- **Fixed resolution:** 1920×1080 pixels — no responsive breakpoints
- **No JavaScript:** All rendering via Razor/SVG/CSS — no JS interop
- **No charting libraries:** SVG primitives rendered inline — no Radzen, MudBlazor, or ApexCharts
- **No CSS frameworks:** No Bootstrap, Tailwind, or MUI — custom CSS only
- **Windows-only font:** Segoe UI is a system font on Windows; other OS users would see Arial fallback
- **Local-only hosting:** Kestrel on localhost — no reverse proxy, no IIS, no cloud hosting

### Timeline Assumptions

- **Phase 1 (MVP):** 1–2 days for a pixel-perfect, data-driven dashboard
- **Phase 2 (Polish):** 1 additional day for CSS theming, tooltips, and `FileSystemWatcher` auto-refresh
- **Phase 3 (Automation):** Half day for optional Playwright screenshot script
- Engineers should have the original `OriginalDesignConcept.html` open in a browser alongside the Blazor app during development for visual comparison

### Dependency Assumptions

- Developer machine has .NET 8 SDK (8.0.x LTS) installed
- Developer has Chrome or Edge for viewing and screenshotting
- Developer is comfortable editing JSON in a text editor
- No external APIs, services, or network connectivity required at runtime
- The `data.json` schema is stable — no schema versioning is needed for v1

### Data Assumptions

- The number of milestone streams will typically be 2–5 (design tested with 3)
- The number of heatmap month columns will typically be 4–6
- Individual heatmap cells will have 0–8 items (design tested with 2–3)
- Item names in `data.json` will be short (under 60 characters) to fit within grid cells without overflow
- `currentDate` in `data.json` will be updated manually by the PM before each reporting cycle

### Open Questions (Deferred to Implementation)

1. Should milestone label placement (above vs. below the stream line) be configurable in JSON, or determined by an auto-layout algorithm to avoid overlaps?
2. Should the heatmap support a 5th custom category row, or is the 4-row structure (Shipped/InProgress/Carryover/Blockers) fixed?
3. Should multiple project support (e.g., `?project=alpha` URL parameter) be included in Phase 1, or deferred?