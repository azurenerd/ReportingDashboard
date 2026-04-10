# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, shipped work, in-progress items, carryover, and blockers in a screenshot-friendly format optimized for PowerPoint decks. The dashboard reads all data from a local `data.json` configuration file and renders a pixel-precise 1920×1080 view using Blazor Server (.NET 8), CSS Grid, Flexbox, and inline SVG—requiring zero third-party dependencies, no authentication, and no cloud infrastructure.

## Business Goals

1. **Reduce executive reporting prep time** — Eliminate manual slide creation by providing a live, always-current dashboard that can be screenshotted directly into PowerPoint.
2. **Standardize project visibility** — Provide a consistent, repeatable visual format for communicating project status across milestones, shipped deliverables, active work, carryover, and blockers.
3. **Enable self-service updates** — Allow project leads to update a single `data.json` file and immediately see the dashboard reflect changes, with no developer intervention required.
4. **Zero operational cost** — Run entirely on a developer's local machine with `dotnet run`, requiring no cloud hosting, licensing, or ongoing infrastructure spend.
5. **Maintain visual fidelity** — Produce a fixed 1920×1080 pixel layout that captures cleanly in browser screenshots without cropping, scaling artifacts, or responsive reflow.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Metadata

**As an** executive viewer, **I want** to see the project title, subtitle (team/workstream context), date, and a link to the ADO backlog at the top of the dashboard, **so that** I immediately know which project I'm looking at and can navigate to the backlog for details.

**Visual Reference:** Header section (`.hdr`) in `OriginalDesignConcept.html`

- [ ] The header displays the project title in 24px bold font.
- [ ] The subtitle appears below the title in 12px gray text showing team, workstream, and current month.
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, styled in Microsoft blue (`#0078D4`).
- [ ] All text values are driven by `data.json` fields: `title`, `subtitle`, `backlogLink`.
- [ ] The header is separated from content below by a 1px `#E0E0E0` border.

### US-2: View Timeline Legend

**As an** executive viewer, **I want** to see a legend in the header explaining the meaning of each timeline symbol (PoC milestone, production release, checkpoint, NOW line), **so that** I can correctly interpret the timeline without external documentation.

**Visual Reference:** Legend area (right side of `.hdr`) in `OriginalDesignConcept.html`

- [ ] The legend displays four items in a horizontal row: PoC Milestone (gold diamond `#F4B400`), Production Release (green diamond `#34A853`), Checkpoint (gray circle `#999`), and Now line (red vertical bar `#EA4335`).
- [ ] Each legend item has a 12px font label next to its symbol.
- [ ] Legend items are spaced with a 22px gap.
- [ ] Legend symbols match the exact shapes rendered in the SVG timeline.

### US-3: View Milestone Timeline

**As an** executive viewer, **I want** to see a horizontal timeline showing milestone tracks (big rocks) with PoC diamonds, production release diamonds, checkpoint circles, and a red "NOW" marker, **so that** I can assess schedule progress at a glance.

**Visual Reference:** Timeline area (`.tl-area` and SVG) in `OriginalDesignConcept.html`

- [ ] The timeline section has a fixed height of 196px with a `#FAFAFA` background.
- [ ] A left sidebar (230px wide) lists milestone track names (e.g., M1, M2, M3) with labels, each in a distinct color.
- [ ] The SVG area spans the remaining width and renders month columns (Jan–Jun) with vertical grid lines.
- [ ] Each track is a horizontal line at a distinct Y position, colored per the track's designated color.
- [ ] PoC milestones render as gold (`#F4B400`) diamond shapes with drop shadows.
- [ ] Production releases render as green (`#34A853`) diamond shapes with drop shadows.
- [ ] Checkpoints render as small gray circles (outlined or filled).
- [ ] A red dashed vertical line (`#EA4335`, stroke-dasharray `5,3`) marks the current date ("NOW"), with a bold red "NOW" label.
- [ ] Milestone labels (date text) appear above or below the marker, positioned via `text-anchor: middle`.
- [ ] All timeline data is driven by `data.json` → `timeline.tracks[]` and `timeline.nowDate`.
- [ ] The number of tracks is dynamic; SVG height adjusts to accommodate all tracks with ~56px vertical spacing per track.

### US-4: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a color-coded grid of work items organized by status (Shipped, In Progress, Carryover, Blockers) and by month, **so that** I can quickly assess execution health and identify problem areas.

**Visual Reference:** Heatmap grid (`.hm-wrap`, `.hm-grid`) in `OriginalDesignConcept.html`

- [ ] The heatmap has a section title "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in 14px bold uppercase gray text.
- [ ] The grid uses CSS Grid with columns: `160px repeat(N, 1fr)` where N is the number of displayed months (default 4).
- [ ] The first row contains column headers for each month, with the current month highlighted in gold (`#FFF0D0` background, `#C07700` text).
- [ ] The first column contains row headers for each status category, styled with category-specific background colors.
- [ ] Each data cell contains bulleted work items (6px colored dot + 12px text, `#333`).
- [ ] **Shipped row**: green theme — header (`#E8F5E9`, text `#1B7A28`), cells (`#F0FBF0`), current month cell (`#D8F2DA`), dot (`#34A853`).
- [ ] **In Progress row**: blue theme — header (`#E3F2FD`, text `#1565C0`), cells (`#EEF4FE`), current month cell (`#DAE8FB`), dot (`#0078D4`).
- [ ] **Carryover row**: amber theme — header (`#FFF8E1`, text `#B45309`), cells (`#FFFDE7`), current month cell (`#FFF0B0`), dot (`#F4B400`).
- [ ] **Blockers row**: red theme — header (`#FEF2F2`, text `#991B1B`), cells (`#FFF5F5`), current month cell (`#FFE4E4`), dot (`#EA4335`).
- [ ] Future month cells with no items display a gray dash (`#AAA`).
- [ ] All heatmap data is driven by `data.json` → `heatmap` object with category keys and month arrays.

### US-5: Configure Dashboard via JSON

**As a** project lead, **I want** to edit a single `data.json` file to update all dashboard content (title, milestones, work items, months), **so that** I can maintain the dashboard without touching code.

- [ ] The application reads `wwwroot/data.json` (or a configurable path) at startup.
- [ ] The JSON schema supports: `title`, `subtitle`, `backlogLink`, `currentMonth`, `months[]`, `timeline` (with tracks and milestones), and `heatmap` (with status categories and monthly item arrays).
- [ ] If `data.json` is missing or malformed, the application logs a clear error message to the console and displays a user-friendly error on the page.
- [ ] Changing `data.json` and restarting the application reflects the updated data.
- [ ] A sample `data.json` with fictional project data is included as part of the deliverable.

### US-6: Screenshot-Ready Layout

**As an** executive viewer, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, nav chrome, or extraneous UI, **so that** I can take a full-page browser screenshot and paste it directly into a PowerPoint slide.

**Visual Reference:** `body` style in `OriginalDesignConcept.html` — `width:1920px; height:1080px; overflow:hidden`

- [ ] The `<body>` is constrained to `1920px × 1080px` with `overflow: hidden`.
- [ ] No Blazor default navigation sidebar, top bar, or footer is rendered.
- [ ] No scrollbars appear at any content volume that fits the design's capacity.
- [ ] The page uses `font-family: 'Segoe UI', Arial, sans-serif` throughout.
- [ ] Background is white (`#FFFFFF`), base text color is `#111`.

### US-7: Run Dashboard Locally

**As a** project lead, **I want** to launch the dashboard with `dotnet run` and view it at `http://localhost:5000`, **so that** I don't need any cloud setup, Docker, or special configuration.

- [ ] The application binds to `http://localhost:5000` only (no external network exposure).
- [ ] No authentication or HTTPS is required.
- [ ] The application starts and renders the dashboard within 5 seconds of `dotnet run`.
- [ ] The only prerequisite is .NET 8 SDK installed on the machine.

## Visual Design Specification

**Reference File:** `OriginalDesignConcept.html` (located in the ReportingDashboard repository)
**Reference Image:** `C:/Pics/ReportingDashboardDesign.png`

### Overall Layout

The dashboard is a **single-page, fixed-viewport layout** at exactly **1920×1080 pixels**. The page uses a vertical flex column (`display: flex; flex-direction: column`) with three stacked sections that fill the entire viewport without scrolling:

1. **Header** (~46px) — Title bar with project metadata and legend
2. **Timeline** (196px fixed) — Horizontal milestone Gantt chart
3. **Heatmap** (fills remaining space) — Monthly execution status grid

### Section 1: Header (`.hdr`)

- **Layout:** Flexbox row, `justify-content: space-between`, `align-items: center`
- **Padding:** `12px 44px 10px`
- **Border:** Bottom 1px solid `#E0E0E0`
- **Left side:**
  - **Title:** `<h1>` at 24px, font-weight 700, color `#111`. Includes an inline `<a>` link in `#0078D4` (no underline) pointing to the ADO backlog.
  - **Subtitle:** `<div class="sub">` at 12px, color `#888`, margin-top 2px. Shows team · workstream · month.
- **Right side (Legend):**
  - Flexbox row with 22px gap.
  - Four items, each a `<span>` with a symbol + 12px label:
    - **PoC Milestone:** 12×12px square rotated 45° (`transform: rotate(45deg)`), background `#F4B400`
    - **Production Release:** 12×12px square rotated 45°, background `#34A853`
    - **Checkpoint:** 8×8px circle (`border-radius: 50%`), background `#999`
    - **Now line:** 2×14px vertical bar, background `#EA4335`, label "Now (Apr 2026)"

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox row, `align-items: stretch`
- **Dimensions:** Height 196px, padding `6px 44px 0`
- **Background:** `#FAFAFA`
- **Border:** Bottom 2px solid `#E8E8E8`

#### Track Labels Sidebar (Left, 230px)

- Fixed width 230px, flex-shrink 0
- Flexbox column, `justify-content: space-around`, padding `16px 12px 16px 0`
- Right border: 1px solid `#E0E0E0`
- Each track label:
  - Track ID (e.g., "M1") in 12px, font-weight 600, colored per track (e.g., `#0078D4`, `#00897B`, `#546E7A`)
  - Track name (e.g., "Chatbot & MS Role") in 12px, font-weight 400, color `#444`

#### SVG Timeline (Right, flex: 1)

- Container: `.tl-svg-box`, flex 1, padding-left 12px, padding-top 6px
- SVG element: width 1560px, height 185px, `overflow: visible`
- **Month grid lines:** Vertical lines at 260px intervals (0, 260, 520, 780, 1040, 1300), stroke `#bbb` opacity 0.4
- **Month labels:** Text at (x+5, y=14), fill `#666`, 11px, font-weight 600
- **NOW line:** Dashed vertical line at calculated x-position, stroke `#EA4335`, stroke-width 2, `stroke-dasharray: 5,3`. Label "NOW" in 10px bold red.
- **Track lines:** Horizontal lines spanning full width at Y positions (42, 98, 154 for 3 tracks), each colored per track, stroke-width 3.
- **Milestone markers:**
  - **Checkpoint (circle):** `<circle>` with white fill, colored stroke (2.5px), radius 5–7px
  - **PoC (diamond):** `<polygon>` with 4 points forming a diamond, fill `#F4B400`, with drop shadow filter (`feDropShadow dx=0, dy=1, stdDeviation=1.5, flood-opacity=0.3`)
  - **Production (diamond):** Same shape as PoC, fill `#34A853`, with drop shadow
  - **Small checkpoint (dot):** `<circle>` radius 4, fill `#999`
- **Milestone labels:** `<text>` at 10px, fill `#666`, `text-anchor: middle`, positioned above or below the marker (alternating to avoid overlap)
- **X-position calculation:** Linear interpolation from date to pixel: `x = ((date - startDate) / (endDate - startDate)) * 1560`

### Section 3: Heatmap (`.hm-wrap`)

- **Layout:** Flexbox column, flex 1, min-height 0
- **Padding:** `10px 44px 10px`

#### Heatmap Title (`.hm-title`)

- 14px, font-weight 700, color `#888`, letter-spacing 0.5px, text-transform uppercase
- Content: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"
- Margin-bottom 8px

#### Heatmap Grid (`.hm-grid`)

- **CSS Grid:** `grid-template-columns: 160px repeat(4, 1fr)`; `grid-template-rows: 36px repeat(4, 1fr)`
- **Border:** 1px solid `#E0E0E0`
- **Flex:** 1 (fills remaining vertical space), min-height 0

##### Corner Cell (`.hm-corner`)

- Background `#F5F5F5`, 11px bold uppercase, color `#999`
- Text: "STATUS"
- Borders: right 1px solid `#E0E0E0`, bottom 2px solid `#CCC`

##### Column Headers (`.hm-col-hdr`)

- Background `#F5F5F5`, 16px bold, centered
- Borders: right 1px solid `#E0E0E0`, bottom 2px solid `#CCC`
- **Current month highlight** (`.apr-hdr`): background `#FFF0D0`, color `#C07700`

##### Row Headers (`.hm-row-hdr`)

- 11px bold uppercase, letter-spacing 0.7px, padding `0 12px`
- Border: right 2px solid `#CCC`, bottom 1px solid `#E0E0E0`
- Per-category styling:
  - **✓ Shipped** (`.ship-hdr`): color `#1B7A28`, background `#E8F5E9`
  - **→ In Progress** (`.prog-hdr`): color `#1565C0`, background `#E3F2FD`
  - **↻ Carryover** (`.carry-hdr`): color `#B45309`, background `#FFF8E1`
  - **✕ Blockers** (`.block-hdr`): color `#991B1B`, background `#FEF2F2`

##### Data Cells (`.hm-cell`)

- Padding `8px 12px`, borders: right 1px `#E0E0E0`, bottom 1px `#E0E0E0`, overflow hidden
- Work items (`.it`): 12px, color `#333`, padding `2px 0 2px 12px`, line-height 1.35
- Bullet dot (`.it::before`): 6×6px circle, positioned absolute at left 0, top 7px
- **Color theming per row:**

| Row | Cell BG | Current Month Cell BG | Dot Color |
|-----|---------|----------------------|-----------|
| Shipped | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

- **Empty/future cells:** Display a single dash "–" in color `#AAA`

### Typography

- **Font family:** `'Segoe UI', Arial, sans-serif` (system font, native on Windows)
- **Base text color:** `#111`
- **Title:** 24px / 700 weight
- **Subtitle:** 12px / 400 weight / `#888`
- **Heatmap section title:** 14px / 700 weight / `#888` / uppercase
- **Column headers:** 16px / 700 weight
- **Row headers:** 11px / 700 weight / uppercase / 0.7px letter-spacing
- **Work items:** 12px / 400 weight / `#333`
- **Timeline labels:** 10–11px / 600 weight / `#666`
- **Legend:** 12px / 400 weight

### Color Palette Summary

| Purpose | Hex Code |
|---------|----------|
| Page background | `#FFFFFF` |
| Base text | `#111` |
| Microsoft Blue (links, In Progress) | `#0078D4` |
| Subtle text / subtitle | `#888` |
| Timeline area background | `#FAFAFA` |
| Grid background / header cells | `#F5F5F5` |
| Muted label text | `#999` |
| Current month header BG | `#FFF0D0` |
| Current month header text | `#C07700` |
| Work item text | `#333` |
| Shipped green (header text) | `#1B7A28` |
| Shipped green (header BG) | `#E8F5E9` |
| Shipped green (dot / diamond) | `#34A853` |
| In Progress blue (header text) | `#1565C0` |
| In Progress blue (header BG) | `#E3F2FD` |
| Carryover amber (header text) | `#B45309` |
| Carryover amber (header BG) | `#FFF8E1` |
| Carryover amber (dot / diamond) | `#F4B400` |
| Blockers red (header text) | `#991B1B` |
| Blockers red (header BG) | `#FEF2F2` |
| Blockers red (dot / diamond) | `#EA4335` |
| NOW line | `#EA4335` |
| Border standard | `#E0E0E0` |
| Border heavy | `#CCC` |
| Timeline area border | `#E8E8E8` |
| Track M1 | `#0078D4` |
| Track M2 | `#00897B` |
| Track M3 | `#546E7A` |

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The dashboard renders the full 1920×1080 layout in under 2 seconds. The header, timeline, and heatmap are all visible without scrolling. Data is loaded from `data.json` and all sections are populated. If `data.json` is missing, a centered error message appears instead of the dashboard.

**Scenario 2: User Views the Header**
User sees the project title (e.g., "Privacy Automation Release Roadmap") with a blue "→ ADO Backlog" link to the right of the title. Below the title, the subtitle shows the team, workstream, and current month. On the far right of the header, the legend displays four symbols with labels explaining timeline markers.

**Scenario 3: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. A new browser tab opens to the configured ADO backlog URL from `data.json`. The dashboard remains open in the original tab.

**Scenario 4: User Reads the Milestone Timeline**
User looks at the timeline section and sees horizontal track lines (one per milestone/big rock), each in a distinct color. Diamond shapes mark PoC and production milestones; circles mark checkpoints. A red dashed "NOW" line shows the current date relative to the timeline. The user can identify which milestones are past, current, and upcoming.

**Scenario 5: User Identifies the Current Date on the Timeline**
User spots the red dashed vertical "NOW" line and its bold red label. All milestones to the left of the NOW line are past; those to the right are upcoming. This provides instant schedule context.

**Scenario 6: User Hovers Over a Milestone Diamond**
User hovers over a gold PoC diamond on the timeline. A browser-native tooltip (via SVG `<title>` element) displays the milestone label and date (e.g., "Mar 26 PoC"). No custom tooltip framework is required.

**Scenario 7: User Scans the Heatmap for Shipped Items**
User looks at the green "✓ Shipped" row. Each month column shows bullet-pointed items that were delivered. The current month column is visually highlighted with a deeper green shade. The user quickly counts shipped items per month.

**Scenario 8: User Identifies Blockers**
User looks at the red "✕ Blockers" row. Red-highlighted cells with red dots draw attention to blocked items. The current month cell uses a deeper red (`#FFE4E4`) to emphasize urgency. Empty blocker cells in future months show a gray dash.

**Scenario 9: User Identifies Carryover Risk**
User looks at the amber "↻ Carryover" row to understand which items slipped from previous months. Items appearing in the current month's carryover cell indicate incomplete work that was carried forward.

**Scenario 10: User Compares Months in the Heatmap**
User scans across a single status row (e.g., In Progress) and compares item counts per month. The current month column header is highlighted with a gold background (`#FFF0D0`) and "→ Now" label, making it instantly identifiable.

**Scenario 11: User Takes a Screenshot for PowerPoint**
User opens the dashboard in Edge or Chrome, uses the browser's full-page screenshot tool (`Ctrl+Shift+S` in Edge), captures the 1920×1080 page, and pastes it into a PowerPoint slide. The fixed layout ensures no cropping or scaling is needed.

**Scenario 12: User Updates Dashboard Data**
Project lead edits `data.json` to add a new shipped item to April, restarts the application (`dotnet run`), and refreshes the browser. The heatmap now shows the new item in the Shipped/April cell.

**Scenario 13: Malformed JSON Error State**
User accidentally introduces a syntax error in `data.json`. On next application start, the console logs a clear error message (e.g., "Failed to parse data.json: Unexpected character at line 42"). The browser displays a simple error message: "Dashboard data could not be loaded. Check data.json for errors."

**Scenario 14: Empty Heatmap Cells**
A month in the future has no items in any category. Each cell for that month displays a gray dash ("–" in `#AAA`), indicating no data rather than a rendering error.

**Scenario 15: Multiple Timeline Tracks**
A project has 5 milestone tracks instead of 3. The timeline section dynamically increases the vertical spacing and SVG height to accommodate all tracks. Track labels in the left sidebar stack vertically with equal spacing.

## Scope

### In Scope

- Single-page Blazor Server dashboard rendering at 1920×1080 fixed viewport
- Header component with configurable title, subtitle, backlog link, and legend
- Horizontal SVG timeline with multiple milestone tracks, PoC/production diamonds, checkpoint circles, and a dynamic "NOW" marker
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- Color-coded cells and row headers per the original design's color palette
- `data.json` file as the single data source for all dashboard content
- Strongly-typed C# models for JSON deserialization using `System.Text.Json`
- `DashboardDataService` singleton that reads and deserializes `data.json` at startup
- Custom `DashboardLayout.razor` with no nav sidebar, no footer, no default Blazor chrome
- CSS file (`dashboard.css`) ported from `OriginalDesignConcept.html` styles
- Sample `data.json` with fictional project data for demonstration
- Localhost-only hosting on `http://localhost:5000`
- Error handling for missing or malformed `data.json`
- Browser-native tooltip on SVG milestone markers (via `<title>` element)

### Out of Scope

- **Authentication & authorization** — No login, no roles, no identity provider integration
- **HTTPS / TLS** — Local HTTP only
- **Cloud deployment** — No Azure, AWS, Docker, or CI/CD pipeline
- **Database backend** — No SQL, no Entity Framework, no SQLite (JSON file only)
- **Real-time updates** — No live reload, no WebSocket push on file change (restart required)
- **Responsive design** — Fixed 1920×1080; no mobile/tablet support
- **PDF export** — No server-side PDF generation
- **Automated screenshot endpoint** — No Playwright or headless browser integration
- **Multi-project routing** — Single project per instance (no `/project/{name}` routing)
- **Historical snapshots** — No versioned data files or month-over-month comparison
- **Data editing UI** — No in-browser form for editing `data.json`; manual file editing only
- **Accessibility (WCAG)** — Not targeted for this MVP (screenshot-only use case)
- **Internationalization** — English only, no localization
- **Third-party charting libraries** — No MudBlazor, Radzen, Chart.js, or similar

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Application startup time | < 5 seconds from `dotnet run` to serving first request |
| Page load time (first paint) | < 2 seconds on localhost |
| JSON parsing time | < 100ms for a `data.json` up to 50 KB |
| SVG rendering | Smooth at up to 10 timeline tracks and 200 heatmap items total |

### Reliability

- The application must start without error when a valid `data.json` is present.
- The application must display a clear error message (not a stack trace) when `data.json` is missing or invalid.
- The application must not crash or hang when `data.json` contains empty arrays or null fields.

### Security

- Bind to `localhost` only (`http://localhost:5000`) — no external network exposure.
- No user input is accepted via the browser; all data comes from a trusted local file.
- No CORS headers needed (single origin).
- No secrets or credentials in `data.json` or application configuration.

### Compatibility

- **Target browser:** Microsoft Edge or Google Chrome (latest stable version)
- **Target OS:** Windows 10/11 with .NET 8 SDK installed
- **Font dependency:** Segoe UI (pre-installed on Windows)

### Maintainability

- All visual styles in a single `dashboard.css` file.
- All data in a single `data.json` file.
- Component-based architecture (Header, Timeline, Heatmap, HeatmapRow, HeatmapCell) for clear separation of concerns.
- Strongly-typed C# models prevent silent data binding errors.

## Success Metrics

1. **Visual fidelity:** The rendered dashboard is visually indistinguishable from the reference design (`OriginalDesignConcept.html` and `ReportingDashboardDesign.png`) when viewed side-by-side in a browser at 1920×1080.
2. **Screenshot quality:** A full-page browser screenshot at 1920×1080 pastes cleanly into a standard 16:9 PowerPoint slide without cropping, whitespace, or scaling artifacts.
3. **Data-driven rendering:** Changing any value in `data.json` (title, milestone dates, work items, months) and restarting the app results in the dashboard correctly reflecting the updated data.
4. **Zero-dependency deployment:** The application runs successfully with only the .NET 8 SDK installed — no additional NuGet packages, npm packages, or external services required.
5. **Startup simplicity:** A new user can clone the repository, run `dotnet run`, open `http://localhost:5000`, and see the dashboard in under 60 seconds.
6. **Error resilience:** Malformed `data.json` produces a human-readable error message in both the console and the browser — never an unhandled exception page.
7. **Reporting cycle time:** A project lead can update `data.json`, restart the app, take a screenshot, and paste it into PowerPoint in under 3 minutes.

## Constraints & Assumptions

### Technical Constraints

- **Framework:** Must use Blazor Server on .NET 8 LTS per the existing repository's technology stack.
- **No third-party UI packages:** The dashboard must use only built-in .NET 8 libraries (System.Text.Json, Kestrel, Blazor Server). No MudBlazor, Radzen, or charting libraries.
- **Fixed viewport:** Layout must be exactly 1920×1080 pixels. No responsive breakpoints.
- **SVG for timeline:** The timeline must use inline SVG (no `<canvas>`, no charting library). This matches the original design and ensures screenshot fidelity.
- **CSS Grid for heatmap:** The heatmap must use CSS Grid with the column pattern from the original design (`160px repeat(N, 1fr)`).
- **Segoe UI font:** The application assumes Segoe UI is available (Windows only). No web font fallback is planned.

### Timeline Assumptions

- **Phase 1 (MVP):** 1–2 days — Pixel-accurate reproduction of the original design as a data-driven Blazor app.
- **Phase 2 (Polish):** 1 day — Improved spacing, hover tooltips, JSON validation, refined typography.
- **Total estimated effort:** 2–3 developer days.

### Dependency Assumptions

- The developer machine has .NET 8 SDK installed.
- The developer machine is running Windows 10 or 11 (for Segoe UI font availability).
- The developer has access to Edge or Chrome for viewing and screenshotting.
- The `data.json` file will be manually edited by the project lead (no automated data pipeline).
- The dashboard serves a single project at a time; multi-project support is a future enhancement.

### Data Assumptions

- The heatmap will display 4–6 months of data (configurable via `data.json` `months` array).
- The timeline will have 1–10 milestone tracks (dynamic SVG height calculation required).
- Each heatmap cell will contain 0–8 work items (beyond 8, items may be clipped by cell overflow).
- The `data.json` file will not exceed 50 KB in size.
- Work item text will be concise (under 60 characters per item) to fit within heatmap cells.