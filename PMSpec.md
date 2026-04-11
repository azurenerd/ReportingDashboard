# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page, data-driven executive reporting dashboard that visualizes project milestones, delivery status, and monthly execution progress. The dashboard reads all content from a local `data.json` configuration file and renders a pixel-perfect 1920×1080 view optimized for screenshotting into PowerPoint decks. Built as a minimal Blazor Server (.NET 8) application with zero authentication, no database, and no external dependencies, the tool enables program managers to quickly generate polished project status visuals for leadership reviews.

## Business Goals

1. **Reduce executive reporting prep time** — Eliminate manual PowerPoint slide construction by providing a screenshot-ready dashboard that auto-renders from structured JSON data.
2. **Standardize project visibility** — Provide a consistent, repeatable visual format for communicating milestone progress, shipped work, in-progress items, carryover debt, and blockers across all projects.
3. **Enable rapid status updates** — Allow a PM to update a single `data.json` file and refresh the browser to produce an updated executive view in under 60 seconds.
4. **Maintain simplicity** — Deliver a tool so lightweight it runs via `dotnet run` on any developer machine with zero infrastructure, zero authentication, and zero ongoing maintenance.
5. **Preserve design fidelity** — Match the approved visual design (`OriginalDesignConcept.html`) with pixel-perfect accuracy so screenshots are indistinguishable from a professionally designed slide.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Metadata

**As a** program manager, **I want** to see the project title, organizational context, date, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period this report covers.

**Visual Reference:** `OriginalDesignConcept.html` — `.hdr` section (header bar with title, subtitle, and legend).

**Acceptance Criteria:**
- [ ] Dashboard displays the project title as a bold 24px heading, sourced from `data.json` → `title`
- [ ] An inline hyperlink labeled "→ ADO Backlog" appears next to the title, linking to `data.json` → `backlogUrl`
- [ ] Subtitle line displays organization, workstream, and date from `data.json` → `subtitle` in 12px gray text
- [ ] A legend appears right-aligned in the header showing four marker types: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now marker (red vertical line)
- [ ] All header content renders from `data.json` — no hardcoded text in markup

---

### US-2: View Milestone Timeline

**As an** executive reviewer, **I want** to see a horizontal timeline showing major milestones as diamonds and checkpoints as circles across a multi-month span, **so that** I can quickly understand the project's trajectory and where we are relative to key dates.

**Visual Reference:** `OriginalDesignConcept.html` — `.tl-area` section (timeline area with SVG and track labels).

**Acceptance Criteria:**
- [ ] Timeline renders as an inline SVG, 1560px wide × 185px tall, inside the `.tl-area` container
- [ ] A left-side label panel (230px wide) lists each milestone track with its ID (e.g., "M1"), description, and track color sourced from `data.json` → `tracks`
- [ ] Vertical gridlines appear at each month boundary with abbreviated month labels (Jan, Feb, Mar, etc.)
- [ ] Each track renders as a horizontal colored line spanning the full SVG width
- [ ] Checkpoint markers render as outlined circles on the track line at their date position
- [ ] PoC milestones render as gold (#F4B400) diamond shapes with drop shadow
- [ ] Production milestones render as green (#34A853) diamond shapes with drop shadow
- [ ] Each marker has a date label positioned above or below the track line
- [ ] A red dashed vertical "NOW" line appears at the current date position with a "NOW" label
- [ ] The timeline start/end dates and all marker positions are driven by `data.json` → `timelineStart`, `timelineEnd`, and `tracks[].markers[]`
- [ ] Supports 1–5 tracks without visual overlap

---

### US-3: View Monthly Execution Heatmap

**As an** executive reviewer, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked — organized by month, **so that** I can assess delivery velocity and identify problem areas at a glance.

**Visual Reference:** `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` section (heatmap grid below timeline).

**Acceptance Criteria:**
- [ ] Heatmap renders as a CSS Grid with structure: `160px repeat(N, 1fr)` columns where N = number of months from `data.json` → `months`
- [ ] Column headers display month names; the current month column is highlighted with gold background (#FFF0D0) and amber text (#C07700)
- [ ] Four status rows appear in this order: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each row header displays the category name with an icon, using the category's signature color
- [ ] Each cell lists work items as bullet-pointed entries (colored dot + text) from `data.json` → `categories[].itemsByMonth`
- [ ] Cells in the current month column use a darker/highlighted background variant
- [ ] Empty cells (future months with no items) display a muted dash "–"
- [ ] A section title "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" appears above the grid in uppercase gray text

---

### US-4: Configure Dashboard via JSON File

**As a** program manager, **I want** to define all dashboard content in a single `data.json` file, **so that** I can update the report by editing text without touching any code.

**Acceptance Criteria:**
- [ ] A `data.json` file in `wwwroot/` drives all rendered content: title, subtitle, backlog URL, timeline tracks, milestones, months, categories, and work items
- [ ] A `data.sample.json` file is provided with a complete fictional project ("Project Phoenix") demonstrating all supported fields
- [ ] Modifying `data.json` and refreshing the browser reflects changes immediately (no rebuild required)
- [ ] If `data.json` is missing or malformed, the application displays a clear, human-readable error message — not a stack trace
- [ ] The JSON schema supports 1–6 months, 1–5 timeline tracks, and 1–4 status categories without code changes
- [ ] `data.json` is listed in `.gitignore`; only `data.sample.json` is committed

---

### US-5: Screenshot-Ready Rendering

**As a** program manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, nav bars, or interactive elements, **so that** I can take a full-page browser screenshot and paste it directly into a PowerPoint slide.

**Visual Reference:** `OriginalDesignConcept.html` — `body` element with `width:1920px; height:1080px; overflow:hidden`.

**Acceptance Criteria:**
- [ ] The `<body>` element is constrained to exactly 1920×1080 pixels with `overflow: hidden`
- [ ] No browser scrollbars appear at 100% zoom in Edge/Chrome at 1920×1080 viewport
- [ ] No navigation sidebar, hamburger menu, or Blazor framework UI chrome is visible
- [ ] No JavaScript-driven loading spinners or skeleton screens appear — the page renders as complete static HTML
- [ ] The page is fully rendered on first paint with no layout shift
- [ ] `launchSettings.json` configures `http://localhost:5000` for easy local access

---

### US-6: Run Dashboard Locally with Zero Setup

**As a** developer or PM, **I want** to start the dashboard with a single `dotnet run` command, **so that** I don't need to install databases, configure cloud services, or set up authentication.

**Acceptance Criteria:**
- [ ] Running `dotnet run` from the project directory starts the application and serves the dashboard at `http://localhost:5000`
- [ ] No NuGet packages beyond the .NET 8 SDK are required
- [ ] No database, cloud service, or external API connection is needed
- [ ] No authentication prompt or login screen appears
- [ ] The application runs on any machine with .NET 8 SDK installed (Windows primary, macOS/Linux secondary)
- [ ] `Ctrl+C` cleanly shuts down the server

---

### US-7: Current Month Auto-Detection

**As a** program manager, **I want** the dashboard to automatically highlight the current month's column, **so that** the "now" context is visually obvious without manual configuration.

**Acceptance Criteria:**
- [ ] The current month is determined by comparing `data.json` → `currentMonth` to column headers
- [ ] The matching column header receives the gold highlight style (`.apr-hdr` equivalent)
- [ ] All cells in the current month column receive the darker background variant for their category
- [ ] The SVG timeline "NOW" marker is positioned using `data.json` → `currentDate`

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html`

Engineers must consult this file directly and match its output pixel-for-pixel. The following description supplements the file for documentation purposes.

### Overall Layout

The page uses a vertical flex column (`display: flex; flex-direction: column`) constrained to exactly **1920×1080 pixels** with `overflow: hidden`. Background is pure white (`#FFFFFF`). The font stack is `'Segoe UI', Arial, sans-serif` with base text color `#111`.

The layout has three vertically stacked sections:

1. **Header Bar** (`.hdr`) — ~46px tall, fixed at top
2. **Timeline Area** (`.tl-area`) — exactly 196px tall
3. **Heatmap Grid** (`.hm-wrap`) — fills remaining vertical space (`flex: 1`)

### Section 1: Header Bar

- **Structure:** Flex row, `justify-content: space-between`, padded `12px 44px 10px`
- **Left side:**
  - Project title: `<h1>` at `24px`, `font-weight: 700`, color `#111`
  - Inline link: color `#0078D4`, no underline, text "→ ADO Backlog"
  - Subtitle: `12px`, color `#888`, margin-top `2px`
- **Right side — Legend:** Flex row with `22px` gap containing four items:
  - Gold diamond (12×12px, `#F4B400`, rotated 45°) + "PoC Milestone"
  - Green diamond (12×12px, `#34A853`, rotated 45°) + "Production Release"
  - Gray circle (8×8px, `#999`) + "Checkpoint"
  - Red vertical line (2×14px, `#EA4335`) + "Now (Apr 2026)"
  - All legend text: `12px`
- **Bottom border:** `1px solid #E0E0E0`

### Section 2: Timeline Area

- **Container:** Flex row, height `196px`, background `#FAFAFA`, bottom border `2px solid #E8E8E8`, padded `6px 44px 0`
- **Left Label Panel** (230px wide):
  - Flex column, `justify-content: space-around`, right border `1px solid #E0E0E0`
  - Each track label: `12px`, `font-weight: 600`, track color for ID (e.g., "M1" in `#0078D4`)
  - Description text: `font-weight: 400`, color `#444`
- **SVG Timeline** (fills remaining width):
  - Dimensions: 1560×185px, `overflow: visible`
  - **Month gridlines:** Vertical lines every ~260px, `stroke: #bbb`, `stroke-opacity: 0.4`
  - **Month labels:** `11px`, `font-weight: 600`, color `#666`, positioned 5px right of gridline
  - **Track lines:** Horizontal lines at y=42, y=98, y=154 (for 3 tracks), `stroke-width: 3`, track color
  - **Checkpoint circles:** `r=5–7`, white fill, track-color stroke, `stroke-width: 2.5`
  - **PoC diamonds:** 22px diagonal, fill `#F4B400`, drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`)
  - **Production diamonds:** 22px diagonal, fill `#34A853`, same drop shadow
  - **Small dots:** `r=4`, fill `#999` (minor checkpoints)
  - **NOW marker:** Dashed vertical line, `stroke: #EA4335`, `stroke-width: 2`, `stroke-dasharray: 5,3`; label "NOW" in `10px`, `font-weight: 700`, `#EA4335`
  - **Date labels:** `10px`, color `#666`, `text-anchor: middle`, positioned above or below track line

### Section 3: Heatmap Grid

- **Wrapper** (`.hm-wrap`): Flex column, `flex: 1`, padded `10px 44px 10px`
- **Title:** `14px`, `font-weight: 700`, color `#888`, uppercase, `letter-spacing: 0.5px`, margin-bottom `8px`
- **Grid** (`.hm-grid`): CSS Grid, `grid-template-columns: 160px repeat(4, 1fr)`, `grid-template-rows: 36px repeat(4, 1fr)`, border `1px solid #E0E0E0`

#### Grid Header Row (36px tall)

| Cell | Style |
|------|-------|
| Corner (`.hm-corner`) | `#F5F5F5` bg, `11px` bold uppercase `#999`, right border `1px solid #E0E0E0`, bottom border `2px solid #CCC` |
| Month headers (`.hm-col-hdr`) | `16px` bold, `#F5F5F5` bg, centered, right/bottom borders |
| Current month header (`.apr-hdr`) | `#FFF0D0` bg, `#C07700` text |

#### Data Rows — Color Scheme

| Row | Header BG | Header Text | Cell BG | Current Cell BG | Dot Color |
|-----|-----------|-------------|---------|-----------------|-----------|
| **Shipped** | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| **In Progress** | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| **Carryover** | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| **Blockers** | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers (`.hm-row-hdr`)

- `11px`, bold, uppercase, `letter-spacing: 0.7px`
- Right border `2px solid #CCC`, bottom border `1px solid #E0E0E0`
- Prefixed with category emoji/icon (✅, 🔵, etc.)

#### Data Cells (`.hm-cell`)

- Padding `8px 12px`, right/bottom borders `1px solid #E0E0E0`
- Each item (`.it`): `12px`, color `#333`, `padding: 2px 0 2px 12px`, `line-height: 1.35`
- Colored dot: `6×6px` circle, positioned `left: 0; top: 7px` via `::before` pseudo-element

### Typography Summary

| Element | Size | Weight | Color |
|---------|------|--------|-------|
| Page title | 24px | 700 | #111 |
| Subtitle | 12px | 400 | #888 |
| Legend items | 12px | 400 | #111 |
| Timeline month labels | 11px | 600 | #666 |
| Timeline date labels | 10px | 400 | #666 |
| Track labels | 12px | 600 | track color |
| Track descriptions | 12px | 400 | #444 |
| Heatmap title | 14px | 700 | #888 |
| Column headers | 16px | 700 | inherit |
| Row headers | 11px | 700 | category color |
| Cell items | 12px | 400 | #333 |

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The browser renders the complete dashboard as a single static HTML page with no loading spinner, skeleton screen, or JavaScript hydration delay. All three sections (header, timeline, heatmap) appear simultaneously on first paint. The page fits exactly within a 1920×1080 viewport with no scrollbars.

**Scenario 2: User Views Header and Identifies Project Context**
User sees the project title prominently at top-left (e.g., "Project Phoenix Release Roadmap") with a clickable "→ ADO Backlog" link. The subtitle confirms the organizational context and reporting period. The legend at top-right provides a key for all timeline marker types.

**Scenario 3: User Reads the Milestone Timeline**
User views the timeline area and sees horizontal track lines for each major workstream (M1, M2, M3). The left-side labels identify each track by name and description. Diamond shapes mark PoC and Production milestones with date labels. A red dashed "NOW" line provides temporal orientation. The user can trace each track line to understand past checkpoints and upcoming milestones.

**Scenario 4: User Scans the Heatmap for Delivery Status**
User looks at the heatmap grid and immediately identifies the current month by its gold-highlighted column header. The four colored rows provide a traffic-light summary: green (shipped = good), blue (in progress = active), amber (carryover = needs attention), red (blocked = needs escalation). The user reads specific work items within each cell.

**Scenario 5: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" hyperlink in the header. The browser navigates to the configured Azure DevOps backlog URL (from `data.json` → `backlogUrl`) in the current tab.

**Scenario 6: User Takes a Screenshot for PowerPoint**
User opens the dashboard in Microsoft Edge at 100% zoom with a 1920×1080 viewport. The entire dashboard is visible without scrolling. User takes a full-page screenshot (e.g., via Snipping Tool or `Ctrl+Shift+S`). The resulting image is ready to paste into a 16:9 PowerPoint slide at full resolution.

**Scenario 7: User Updates Project Data**
User opens `wwwroot/data.json` in a text editor, modifies work item text, adds a new milestone marker, or changes the current month. User saves the file and refreshes the browser. The dashboard re-renders with the updated content within 1 second.

**Scenario 8: Empty Future Month Cells**
User views months that have no work items yet (e.g., May, June). Those cells display a muted dash "–" in light gray rather than appearing blank, maintaining grid visual integrity.

**Scenario 9: Missing or Malformed data.json**
User starts the application without a `data.json` file or with invalid JSON. Instead of a stack trace or blank page, the dashboard displays a clear error message such as "Error: Could not load data.json. Please ensure the file exists in wwwroot/ and contains valid JSON."

**Scenario 10: User Hovers Over a Milestone Diamond**
Since this is a static SSR page with no JavaScript interactivity, no hover tooltip appears. This is intentional — the date labels printed adjacent to each milestone marker provide all necessary context without interaction. (Future enhancement: optional CSS-only tooltips could be added without JavaScript.)

## Scope

### In Scope

- Single-page Blazor Server application (.NET 8, Static SSR) rendering the dashboard
- Complete CSS port from `OriginalDesignConcept.html` into `dashboard.css`
- `data.json` configuration file defining all dashboard content
- `data.sample.json` with fictional "Project Phoenix" data demonstrating all features
- Header component with project title, subtitle, backlog link, and legend
- SVG timeline component with configurable tracks, milestones, checkpoints, and NOW marker
- CSS Grid heatmap component with 4 status rows × N month columns
- Current month auto-highlighting based on `data.json` → `currentMonth`
- Date-to-SVG-coordinate mapping for milestone positioning
- Graceful error handling for missing/malformed `data.json`
- Fixed 1920×1080 viewport optimized for screenshots
- `launchSettings.json` for convenient local startup
- `.gitignore` entry for `data.json`

### Out of Scope

- **Authentication / Authorization** — No login, no user accounts, no RBAC
- **Database** — No SQL, no Entity Framework, no SQLite; flat JSON file only
- **Real-time updates** — No SignalR, no WebSocket, no auto-refresh; manual browser refresh only
- **Responsive design** — Fixed 1920×1080 viewport only; no mobile or tablet layouts
- **Multi-project support** — One `data.json` = one project; swap files manually for different projects
- **Print stylesheet** — Optimized for screenshot, not print
- **Dark mode** — White background only, matching the approved design
- **User input / forms** — No editable fields, no data entry, no filters or search
- **CI/CD pipeline** — No automated build/deploy; local `dotnet run` only
- **Containerization** — No Docker, no Kubernetes
- **Cloud deployment** — No Azure, no AWS; local machine only
- **Internationalization** — English only
- **Accessibility (WCAG)** — Not a priority for a screenshot-capture tool; may be revisited if the dashboard becomes user-facing
- **Charting libraries** — No MudBlazor, Radzen, Syncfusion, Chart.js, or D3; hand-coded SVG and CSS only
- **Browser compatibility testing** — Target Edge/Chrome (Chromium) only

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Page load time (first paint) | < 500ms on localhost |
| HTML response size | < 100KB (no JS payload) |
| Server startup time | < 3 seconds from `dotnet run` to listening |
| Data refresh (edit JSON + F5) | < 1 second to re-render |

### Reliability

- Application must start cleanly with `dotnet run` on any machine with .NET 8 SDK
- Malformed `data.json` must produce a clear error message, not crash the process
- Missing `data.json` must produce a clear error message, not a 500 error page

### Maintainability

- Zero external NuGet dependencies beyond the .NET 8 SDK
- All visual styling in a single `dashboard.css` file ported from the original HTML
- All data in a single `data.json` file — no code changes needed for content updates
- Component architecture: Header, Timeline, and HeatmapGrid as separate `.razor` files

### Compatibility

| Environment | Support Level |
|-------------|--------------|
| Windows + Edge/Chrome | Primary — pixel-perfect |
| macOS + Chrome | Secondary — functional, font may differ (Arial fallback) |
| Linux + Chrome | Secondary — functional, font may differ |

### Security

- No authentication required
- No sensitive data processed
- No user input accepted
- No HTTPS required (localhost only)
- `data.json` excluded from source control via `.gitignore` to prevent accidental commit of internal project names

## Success Metrics

1. **Visual Fidelity:** A screenshot of the Blazor dashboard at 1920×1080 is visually indistinguishable from a screenshot of `OriginalDesignConcept.html` when populated with the same data — validated by side-by-side comparison.
2. **Data-Driven Rendering:** Changing any field in `data.json` and refreshing the browser correctly updates the corresponding visual element on the dashboard — validated for all data fields (title, subtitle, tracks, milestones, categories, work items, current month).
3. **Zero-Config Startup:** A new developer can clone the repo, run `dotnet run`, and see the dashboard at `http://localhost:5000` with the sample data — no additional setup steps required.
4. **Time to Update:** A PM can update `data.json` with new monthly status data and produce an updated screenshot in under 2 minutes.
5. **Screenshot Quality:** The dashboard screenshot pastes cleanly into a standard 16:9 PowerPoint slide (13.33" × 7.5" at 144 DPI) without scaling artifacts.
6. **Build Simplicity:** The project compiles with zero warnings and zero external NuGet package restores (SDK-included packages only).

## Constraints & Assumptions

### Technical Constraints

- **Runtime:** .NET 8 SDK (LTS) is required and must be pre-installed on the developer's machine
- **Render mode:** Blazor Static SSR only — no interactive render modes, no SignalR circuit
- **Viewport:** Fixed 1920×1080 pixels; the CSS uses absolute pixel values, not responsive units
- **Font dependency:** Segoe UI is a Windows system font; macOS/Linux users will see Arial fallback, which may cause minor spacing differences
- **SVG rendering:** Timeline is inline SVG generated server-side in Razor; no client-side JavaScript manipulation
- **Grid columns:** The heatmap grid supports up to 6 month columns (beyond 6, column widths become too narrow for readable text at 12px)

### Assumptions

- The primary user is a PM on Windows using Microsoft Edge or Chrome for screenshots
- The dashboard will be used by 1 person at a time on their local machine — no concurrent access concerns
- Data volume is small: ≤5 timeline tracks, ≤6 months, ≤4 status categories, ≤10 items per cell
- The PM is comfortable editing JSON files in a text editor (VS Code, Notepad++, etc.)
- Screenshots will be taken at exactly 100% browser zoom — no DPI scaling adjustments
- The `OriginalDesignConcept.html` design is approved and final; no design iteration is expected
- The project will not evolve into a multi-user or cloud-hosted application; if that need arises, a new spec will be written
- Color palette and category names (Shipped, In Progress, Carryover, Blockers) are fixed for v1; future versions may allow custom categories via `data.json`

### Dependencies

- **.NET 8 SDK** — must be installed; no other tooling required
- **`OriginalDesignConcept.html`** — the canonical visual reference; must be consulted by all engineers before implementation
- **`data.sample.json`** — must ship with realistic fictional data so the dashboard works out of the box