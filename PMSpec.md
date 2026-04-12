# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, shipping status, and monthly progress in a format optimized for PowerPoint screenshot capture at 1920×1080 resolution. The dashboard reads all display data from a local `data.json` configuration file, renders a timeline with milestone markers and a color-coded heatmap grid, and requires zero authentication, zero cloud services, and zero database infrastructure — making it an intentionally minimal, locally-hosted tool built with C# .NET 8 and Blazor Server.

## Business Goals

1. **Provide executives with a single-glance project status view** that communicates shipped work, in-progress items, carryover debt, and blockers across a rolling monthly window.
2. **Eliminate manual PowerPoint slide creation** by producing a pixel-perfect, screenshot-ready dashboard that can be captured directly into executive presentation decks.
3. **Enable non-developers to update project status** by editing a plain `data.json` file without touching HTML, CSS, or code.
4. **Reduce reporting cycle time** from hours of manual slide formatting to minutes of JSON editing and a browser screenshot.
5. **Establish a reusable reporting template** that can be cloned and adapted for any project by swapping the `data.json` file.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Metadata

**As an** executive viewer, **I want** to see the project name, workstream, reporting period, and a link to the ADO backlog at the top of the dashboard, **so that** I immediately know which project and time period I'm looking at.

**Visual Reference:** Header section (`.hdr`) of `OriginalDesignConcept.html`

- [ ] Project name displays as a bold 24px heading (e.g., "Privacy Automation Release Roadmap")
- [ ] An "→ ADO Backlog" hyperlink appears inline after the project name, styled in `#0078D4`
- [ ] Subtitle line shows workstream name, bullet separator, and report month (e.g., "Trusted Platform · Privacy Automation Workstream · April 2026") in 12px gray (`#888`)
- [ ] A legend appears right-aligned in the header showing four indicators: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now line (red vertical bar)
- [ ] All values are driven from `data.json` fields: `projectName`, `workstream`, `reportMonth`, `backlogUrl`

### US-2: View Milestone Timeline

**As an** executive viewer, **I want** to see a horizontal timeline showing milestone swim lanes with date markers, **so that** I can understand the project's chronological plan and current position at a glance.

**Visual Reference:** Timeline area (`.tl-area`) and inline SVG of `OriginalDesignConcept.html`

- [ ] A left sidebar (230px wide) lists milestone IDs and labels (e.g., "M1 — Chatbot & MS Role"), each colored per their milestone color from `data.json`
- [ ] Each milestone renders as a horizontal line spanning the full timeline width, colored per milestone
- [ ] Month grid lines appear at evenly spaced intervals with month abbreviation labels (Jan, Feb, Mar, etc.)
- [ ] Checkpoint events render as open circles (white fill, colored stroke) on the milestone line
- [ ] PoC milestones render as gold (`#F4B400`) diamond shapes with drop shadow
- [ ] Production releases render as green (`#34A853`) diamond shapes with drop shadow
- [ ] Each event has a text label positioned above or below the marker showing date and description
- [ ] A dashed red (`#EA4335`) vertical "NOW" line indicates the current date position
- [ ] The NOW date is read from `data.json` field `timeline.nowDate`
- [ ] The timeline date range is driven by `timeline.startDate` and `timeline.endDate`
- [ ] Timeline area has a fixed height of 196px with `#FAFAFA` background

### US-3: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a color-coded grid showing shipped items, in-progress work, carryover, and blockers organized by month, **so that** I can assess execution health and identify problem areas.

**Visual Reference:** Heatmap grid (`.hm-wrap`, `.hm-grid`) of `OriginalDesignConcept.html`

- [ ] A section title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase gray 14px text
- [ ] The grid uses CSS Grid with columns: 160px status label + 4 equal-width month columns
- [ ] The grid has a header row (36px) with month names; the current month column has a highlighted gold background (`#FFF0D0`) with amber text (`#C07700`) and "← Now" indicator
- [ ] **Shipped row** (green theme): header `#E8F5E9` with `#1B7A28` text; cells `#F0FBF0`, current month cell `#D8F2DA`; bullet dots `#34A853`
- [ ] **In Progress row** (blue theme): header `#E3F2FD` with `#1565C0` text; cells `#EEF4FE`, current month cell `#DAE8FB`; bullet dots `#0078D4`
- [ ] **Carryover row** (amber theme): header `#FFF8E1` with `#B45309` text; cells `#FFFDE7`, current month cell `#FFF0B0`; bullet dots `#F4B400`
- [ ] **Blockers row** (red theme): header `#FEF2F2` with `#991B1B` text; cells `#FFF5F5`, current month cell `#FFE4E4`; bullet dots `#EA4335`
- [ ] Each cell lists individual work items as 12px text with a colored dot prefix, driven from `data.json` heatmap arrays
- [ ] Empty cells display a gray dash placeholder
- [ ] The heatmap grid fills the remaining vertical space below the timeline

### US-4: Configure Dashboard via JSON File

**As a** project manager (non-developer), **I want** to update the dashboard content by editing a `data.json` file, **so that** I can refresh the reporting view without modifying any code.

- [ ] A `data.json` file in `wwwroot/` contains all display data: project metadata, timeline milestones, and heatmap items
- [ ] Changing `data.json` and refreshing the browser reflects the new data
- [ ] The JSON schema supports: `projectName`, `workstream`, `reportMonth`, `backlogUrl`, `timeline` (with `startDate`, `endDate`, `nowDate`, `milestones[]`), and `heatmap` (with `columns[]`, `currentColumn`, `shipped`, `inProgress`, `carryover`, `blockers` as month-keyed dictionaries)
- [ ] Malformed JSON displays a user-friendly error message rather than a blank page or crash
- [ ] Missing optional fields (e.g., empty blocker arrays) render gracefully with placeholder dashes

### US-5: Capture Screenshot-Ready Output

**As an** executive viewer, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, **so that** I can take a browser screenshot and paste it directly into a PowerPoint slide.

**Visual Reference:** `body` CSS in `OriginalDesignConcept.html` — `width:1920px; height:1080px; overflow:hidden`

- [ ] The page body is fixed at 1920px × 1080px with `overflow: hidden`
- [ ] No scrollbars appear at 100% zoom on a 1920×1080 display
- [ ] The layout uses `flex-direction: column` to stack header → timeline → heatmap vertically
- [ ] All content fits within the fixed viewport without truncation under normal data volumes (up to 5 items per heatmap cell)
- [ ] The font family is `'Segoe UI', Arial, sans-serif` for consistent rendering on Windows

### US-6: Run Dashboard Locally with Minimal Setup

**As a** developer or project manager, **I want** to launch the dashboard with a single `dotnet run` command, **so that** I don't need to configure cloud services, databases, or authentication.

- [ ] The application starts with `dotnet run` or `dotnet watch` from the project directory
- [ ] The dashboard is accessible at `http://localhost:5000` (or configured port)
- [ ] No authentication prompts or login screens appear
- [ ] No external service dependencies (no database, no cloud APIs, no CDN)
- [ ] The application runs on .NET 8 LTS with zero third-party NuGet packages beyond the default Blazor Server template

## Visual Design Specification

All UI implementation MUST match the design defined in **`OriginalDesignConcept.html`** and its rendered screenshot at **`docs/design-screenshots/OriginalDesignConcept.png`**. The design is a fixed 1920×1080 single-page layout with three vertically stacked sections.

### Overall Page Layout

- **Viewport:** Fixed `1920px × 1080px`, `overflow: hidden`, white (`#FFFFFF`) background
- **Font:** `'Segoe UI', Arial, sans-serif`, base text color `#111`
- **Layout Model:** `display: flex; flex-direction: column` on `<body>`
- **Horizontal Padding:** 44px on left and right for all major sections
- **Three Sections (top to bottom):**
  1. Header bar (~50px)
  2. Timeline area (fixed 196px height)
  3. Heatmap grid (fills remaining ~834px)

### Section 1: Header Bar (`.hdr`)

- **Layout:** Flexbox, `align-items: center`, `justify-content: space-between`
- **Padding:** `12px 44px 10px`
- **Bottom border:** `1px solid #E0E0E0`
- **Left side:**
  - Project name: `<h1>`, 24px, font-weight 700, color `#111`
  - Inline link: `<a>` styled in `#0078D4`, no underline, text "→ ADO Backlog"
  - Subtitle: 12px, color `#888`, margin-top 2px — format: "Workstream · Report Month"
- **Right side — Legend row:** Flexbox with 22px gaps, 12px font-size
  - PoC Milestone: 12×12px gold (`#F4B400`) square rotated 45° (diamond shape)
  - Production Release: 12×12px green (`#34A853`) square rotated 45°
  - Checkpoint: 8×8px gray (`#999`) circle
  - Now indicator: 2×14px red (`#EA4335`) vertical bar, label "Now (Apr 2026)"

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox row, `align-items: stretch`
- **Height:** Fixed 196px
- **Background:** `#FAFAFA`
- **Bottom border:** `2px solid #E8E8E8`
- **Left Sidebar (230px):**
  - Fixed width, flex-shrink 0
  - Contains milestone labels stacked vertically with `justify-content: space-around`
  - Each label: 12px font-weight 600, milestone ID in milestone color (e.g., `#0078D4` for M1), description in `#444` font-weight 400
  - Right border: `1px solid #E0E0E0`
- **SVG Timeline Panel (flex: 1):**
  - SVG element: width 1560px, height 185px, `overflow: visible`
  - **Month Grid Lines:** Vertical lines at 260px intervals (0, 260, 520, 780, 1040, 1300), stroke `#bbb` at 0.4 opacity
  - **Month Labels:** 11px, font-weight 600, color `#666`, positioned 5px right of each grid line at y=14
  - **"NOW" Line:** Dashed vertical line (`stroke-dasharray: 5,3`), 2px width, color `#EA4335`, with "NOW" label in 10px bold red at top
  - **Milestone Swim Lanes:** Horizontal lines at y=42, y=98, y=154 (evenly spaced for 3 milestones), stroke-width 3, colored per milestone
  - **Checkpoint Markers:** Open circles (white fill, colored stroke, stroke-width 2.5), radius 5–7px
  - **PoC Diamond Markers:** `<polygon>` forming 22px diamond, fill `#F4B400`, with drop-shadow filter
  - **Production Diamond Markers:** `<polygon>` forming 22px diamond, fill `#34A853`, with drop-shadow filter
  - **Event Labels:** 10px text, color `#666`, `text-anchor: middle`, positioned above or below markers
  - **Drop Shadow Filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3">`

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Layout:** Flexbox column, `flex: 1` (fills remaining space)
- **Padding:** `10px 44px 10px`
- **Section Title (`.hm-title`):** 14px, font-weight 700, color `#888`, letter-spacing 0.5px, uppercase, margin-bottom 8px
- **Grid (`.hm-grid`):**
  - `display: grid`
  - `grid-template-columns: 160px repeat(4, 1fr)`
  - `grid-template-rows: 36px repeat(4, 1fr)`
  - `border: 1px solid #E0E0E0`
  - `flex: 1; min-height: 0`

#### Grid Header Row (36px)

| Cell | Background | Font | Border |
|------|-----------|------|--------|
| Corner (`.hm-corner`) | `#F5F5F5` | 11px bold `#999` uppercase | right: `1px solid #E0E0E0`, bottom: `2px solid #CCC` |
| Month headers (`.hm-col-hdr`) | `#F5F5F5` | 16px bold | right: `1px solid #E0E0E0`, bottom: `2px solid #CCC` |
| Current month (`.apr-hdr`) | `#FFF0D0` | 16px bold `#C07700` | Same as above |

#### Grid Data Rows (4 rows, equal height)

| Row | Header BG | Header Text | Cell BG | Current Month Cell BG | Dot Color |
|-----|-----------|-------------|---------|----------------------|-----------|
| ✓ Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| → In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| ↻ Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| ✕ Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Cell Item Styling (`.hm-cell .it`)

- Font: 12px, color `#333`, line-height 1.35
- Padding: `2px 0 2px 12px` (left indent for dot)
- Dot: `::before` pseudo-element, 6×6px circle, positioned absolutely at left:0, top:7px, colored per row

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The browser renders the full dashboard in a single viewport at 1920×1080. The header, timeline, and heatmap all appear simultaneously with no loading spinner or progressive rendering. All data is populated from `data.json`.

**Scenario 2: User Views Project Header**
User sees the project name "Privacy Automation Release Roadmap" in bold at top-left, with a blue "→ ADO Backlog" link beside it. Below it, a gray subtitle shows the workstream and month. On the right, a legend row shows four marker types (PoC diamond, Production diamond, Checkpoint circle, Now line).

**Scenario 3: User Reads Milestone Timeline**
User looks at the timeline area and sees three horizontal swim lanes labeled M1, M2, M3 on the left sidebar. Each lane has colored markers at specific dates. A dashed red "NOW" vertical line bisects the timeline at the current date, providing immediate visual context of where the project stands chronologically.

**Scenario 4: User Identifies a PoC Milestone**
User spots a gold diamond on the M1 swim lane labeled "Mar 26 PoC". The diamond shape and gold color match the legend's "PoC Milestone" indicator. The label text below the diamond confirms the date and type.

**Scenario 5: User Identifies a Production Release**
User spots a green diamond on the M2 swim lane labeled "Mar 30 Prod". The green color and diamond shape match the legend's "Production Release" indicator.

**Scenario 6: User Scans the Heatmap for Current Month Status**
User's eye is drawn to the "April ← Now" column header, which has a distinct gold background (`#FFF0D0`) differentiating it from other months. The user scans down the April column to see shipped items (green), in-progress items (blue), carryover items (amber), and blockers (red).

**Scenario 7: User Identifies a Blocker**
User scans the Blockers row (red-themed) and sees "Waiting on legal review for PII handling" in the April column. The red dot and red-tinted cell background immediately signal this is a blocking issue requiring attention.

**Scenario 8: User Identifies Carryover Debt**
User notices the Carryover row shows "E2E test suite (from Mar)" in the April column, indicating work that slipped from the previous month. The amber coloring and "(from Mar)" suffix make the carryover origin clear.

**Scenario 9: User Takes a Screenshot for PowerPoint**
User presses the screenshot hotkey (Win+Shift+S or similar). The entire dashboard fits within a single 1920×1080 capture with no scrollbars, cut-off content, or browser chrome within the content area. The screenshot pastes cleanly into a 16:9 PowerPoint slide.

**Scenario 10: User Updates Dashboard Data**
User opens `data.json` in a text editor, changes a blocker item text, saves the file, and refreshes the browser. The dashboard re-renders with the updated blocker text in the appropriate cell.

**Scenario 11: Empty Heatmap Cells**
For months with no items in a category (e.g., no blockers in January), the cell displays a single gray dash "—" rather than appearing blank, maintaining visual consistency across the grid.

**Scenario 12: Malformed JSON Error State**
User accidentally introduces a syntax error in `data.json` and refreshes. Instead of a blank page or unhandled exception, the dashboard displays a clear error message indicating the JSON could not be parsed, along with guidance to check the file format.

**Scenario 13: Hover Over ADO Backlog Link**
User hovers over the "→ ADO Backlog" link in the header. The cursor changes to a pointer, indicating it is clickable. Clicking opens the backlog URL from `data.json` (though this is primarily decorative for screenshot purposes).

**Scenario 14: Timeline with Pre-Range Dates**
A milestone event has a date before the timeline's `startDate` (e.g., "Dec 19" on a Jan–Jun timeline). The marker renders at the left edge (x=0) of the timeline with its label visible, rather than being clipped or hidden.

## Scope

### In Scope

- Single-page Blazor Server dashboard matching the `OriginalDesignConcept.html` design
- Header bar with project metadata, backlog link, and milestone legend
- SVG-based horizontal timeline with milestone swim lanes, date markers (checkpoint circles, PoC diamonds, production diamonds), and a "NOW" indicator line
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) × configurable month columns
- Color-coded cells and row headers matching the exact hex values from the design
- `data.json` file as the sole data source for all dashboard content
- Fixed 1920×1080 viewport layout optimized for screenshot capture
- `DashboardDataService` for JSON deserialization using `System.Text.Json`
- POCO data models (`DashboardConfig`, `Milestone`, `TimelineEvent`, `HeatmapData`)
- Blazor CSS isolation (`Dashboard.razor.css`) with scoped styles ported from the original HTML
- Local Kestrel hosting via `dotnet run` on `http://localhost:5000`
- Graceful handling of missing/empty heatmap cells
- Basic error display for malformed `data.json`
- Sample `data.json` with fictional project data for demonstration

### Out of Scope

- **Authentication and authorization** — no login, no user accounts, no role-based access
- **Database or persistent storage** — no SQL, no Entity Framework, no cloud storage
- **Cloud deployment** — no Azure App Service, no containers, no CI/CD pipeline
- **HTTPS / TLS** — localhost HTTP is sufficient
- **Multi-project support** — single `data.json` at a time; no project switcher UI
- **Real-time data refresh** — no `FileSystemWatcher` or auto-polling; manual browser refresh required
- **Responsive / mobile layout** — fixed 1920×1080 only; no breakpoints
- **Print-to-PDF** — no `@media print` stylesheet (screenshots are the delivery mechanism)
- **Interactive filtering or drill-down** — read-only display; no click-to-filter, no modals
- **Charting libraries** — no Chart.js, ApexCharts, Plotly, or similar; SVG is hand-rendered
- **Component libraries** — no MudBlazor, Radzen, Syncfusion, or similar
- **Tooltip hover states on heatmap cells** — items are displayed inline; no hover tooltips
- **Accessibility (WCAG) compliance** — optimized for screenshot capture, not screen reader usage
- **Internationalization / localization** — English only, single locale
- **Unit or integration test suite** — testing is optional and not required for MVP delivery

## Non-Functional Requirements

### Performance

- **Page load time:** Dashboard must render fully within 2 seconds of browser navigation on localhost
- **JSON parsing:** `data.json` files up to 50KB must deserialize in under 100ms
- **Memory footprint:** Application should consume less than 100MB RAM at steady state

### Reliability

- **Graceful degradation:** Null or missing fields in `data.json` must not cause unhandled exceptions; use null-coalescing defaults throughout Razor markup
- **Error messaging:** JSON parse failures must display a user-readable error, not a stack trace
- **Browser refresh recovery:** A simple F5 refresh must fully recover the dashboard state

### Compatibility

- **Target browser:** Microsoft Edge (latest) on Windows — optimized for Segoe UI font rendering
- **Secondary browser support:** Google Chrome (latest) — layout must be equivalent
- **Runtime:** .NET 8 LTS (supported through November 2026)
- **Operating system:** Windows 10/11 (primary), macOS/Linux (untested but functional via .NET cross-platform)

### Visual Fidelity

- **Resolution:** Fixed 1920×1080px layout; no responsive breakpoints
- **Font rendering:** Segoe UI system font must be used; no web font downloads
- **Color accuracy:** All hex color values must match the design specification exactly (see Visual Design Specification section)
- **Screenshot readiness:** No scrollbars, no browser-injected UI, no overflow at 100% zoom on a 1920×1080 display

### Maintainability

- **File count:** Application should comprise ~6 core files (Program.cs, DashboardData.cs, DashboardDataService.cs, Dashboard.razor, Dashboard.razor.css, data.json) plus standard Blazor scaffolding (App.razor, Routes.razor)
- **Zero external NuGet packages:** All functionality uses .NET 8 built-in libraries
- **Hot reload:** `dotnet watch` must support live editing of Razor and CSS files

## Success Metrics

| # | Metric | Target | Measurement |
|---|--------|--------|-------------|
| 1 | Visual fidelity | Dashboard screenshot overlays on `OriginalDesignConcept.png` with <5% pixel deviation | Manual overlay comparison in PowerPoint |
| 2 | Data-driven rendering | All text, dates, colors, and items on the dashboard are sourced from `data.json` with zero hardcoded content | Code review: no string literals in Razor markup that should come from data |
| 3 | JSON update cycle time | Editing `data.json` and refreshing the browser produces updated dashboard in <10 seconds end-to-end | Manual timing test |
| 4 | Setup complexity | New user can go from `git clone` to running dashboard in ≤3 commands (`dotnet restore`, `dotnet run`, open browser) | README walkthrough test |
| 5 | File count | Core application files ≤ 8 (excluding `obj/`, `bin/`, `.csproj`) | File system count |
| 6 | External dependencies | Zero third-party NuGet packages | `.csproj` inspection |
| 7 | Screenshot quality | Dashboard fits 16:9 PowerPoint slide at full bleed with no clipping | Paste screenshot into 1920×1080 slide |

## Constraints & Assumptions

### Technical Constraints

- **Runtime:** Must use .NET 8 and Blazor Server as specified by the existing repository's technology stack
- **No JavaScript:** The original design is JS-free; the Blazor implementation should avoid custom JavaScript interop
- **Fixed viewport:** The 1920×1080 fixed layout is a hard constraint driven by the PowerPoint screenshot use case — responsive design is explicitly excluded
- **System fonts only:** Segoe UI must render natively; no web font loading or CDN dependencies
- **Local-only hosting:** The application runs on `localhost` via Kestrel; no network exposure required

### Assumptions

- **Single user:** Only one person views the dashboard at a time; no concurrent access concerns
- **Manual refresh:** Users will manually refresh the browser after editing `data.json`; auto-refresh is not expected
- **Heatmap cell capacity:** Each heatmap cell will contain at most 5 items; overflow behavior is not required for MVP
- **Timeline range:** The timeline spans a 6-month configurable window; ranges shorter or longer than 6 months are not tested but should function
- **"NOW" date source:** The "NOW" line position is driven by the `nowDate` field in `data.json`, not by system clock — this allows screenshots to be taken with a controlled date for consistency
- **Browser zoom:** Users will view the dashboard at 100% browser zoom; behavior at other zoom levels is undefined
- **Data integrity:** The `data.json` file is authored by a trusted user; no adversarial input validation is required
- **ADO Backlog link:** The backlog URL link is functional but primarily decorative — its main purpose is to appear in screenshots
- **Milestone count:** The timeline supports 2–5 milestone swim lanes; layouts with more than 5 may require vertical spacing adjustments that are out of scope for MVP