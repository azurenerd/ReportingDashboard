# PM Specification: Executive Reporting Dashboard

**Document Version:** 1.0
**Date:** April 17, 2026
**Author:** Program Management
**Status:** Draft
**Design Reference:** `OriginalDesignConcept.html` (ReportingDashboard repo)

---

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes a project's milestone timeline and monthly execution status (Shipped, In Progress, Carryover, Blockers) in a format optimized for 1920×1080 PowerPoint screenshot capture. The dashboard reads all data from a local `data.json` configuration file and runs entirely on localhost via a .NET 8 Blazor Server application—requiring zero authentication, zero cloud infrastructure, and zero external dependencies. The primary consumer is a program manager who needs a polished, at-a-glance project status view to paste into executive slide decks.

---

## Business Goals

1. **Reduce executive reporting preparation time by 75%.** Replace manual PowerPoint slide construction with an auto-rendered, data-driven dashboard that produces screenshot-ready visuals in seconds.
2. **Provide a single, consistent visual format for project status.** Standardize how milestone progress and monthly execution are communicated to executives across presentations.
3. **Enable rapid iteration on project data.** Allow the PM to edit a single JSON file and immediately see updated visuals without rebuilding, redeploying, or involving engineering.
4. **Maintain pixel-perfect screenshot fidelity.** Ensure the rendered dashboard at 1920×1080 matches the approved design exactly, producing presentation-quality output every time.
5. **Keep total cost of ownership at $0.** No cloud services, no licenses, no subscriptions—runs entirely on a developer's local machine with the .NET 8 SDK.

---

## User Stories & Acceptance Criteria

### US-1: View Project Header and Metadata

**As a** program manager, **I want** to see the project title, organizational context (subtitle), a link to the ADO backlog, and the current reporting date at the top of the dashboard, **so that** executives immediately know which project and time period this report covers.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` element.

**Acceptance Criteria:**
- [ ] The project title renders at 24px bold with an inline clickable link (styled `#0078D4`) to the ADO backlog URL from `data.json`.
- [ ] The subtitle renders at 12px in `#888` below the title, showing org hierarchy and current month/year.
- [ ] A legend appears right-aligned in the header showing four symbols: PoC Milestone (amber diamond), Production Release (green diamond), Checkpoint (gray circle), and Now line (red vertical bar).
- [ ] All header data is sourced from `data.json` `header` object.
- [ ] The header has a 1px `#E0E0E0` bottom border separating it from the timeline section.

---

### US-2: View Milestone Timeline

**As a** program manager, **I want** to see a horizontal timeline with multiple tracks showing project milestones (kickoff, PoC, production) plotted by date, **so that** executives can instantly understand the project's schedule and where we are relative to key dates.

**Visual Reference:** Timeline area of `OriginalDesignConcept.html` — `.tl-area` element with inline SVG.

**Acceptance Criteria:**
- [ ] The timeline section renders at a fixed height of 196px with a `#FAFAFA` background.
- [ ] A left sidebar (230px wide) lists each track's label (e.g., "M1") and description (e.g., "API Gateway & Auth"), color-coded per track.
- [ ] The SVG area (remaining width, ~1560px) renders vertical month grid lines with month abbreviation labels (Jan–Jun).
- [ ] Each track renders as a colored horizontal line at a distinct Y offset.
- [ ] Checkpoint milestones render as open circles (white fill, colored stroke, radius 7px).
- [ ] PoC milestones render as amber (`#F4B400`) diamond shapes with drop shadow.
- [ ] Production milestones render as green (`#34A853`) diamond shapes with drop shadow.
- [ ] A dashed red (`#EA4335`) vertical "NOW" line renders at the current date's calculated X position.
- [ ] Date labels appear above or below each milestone marker.
- [ ] Milestone X positions are calculated dynamically: `(date - startDate) / (endDate - startDate) * svgWidth`.
- [ ] All timeline data is sourced from `data.json` `timeline` object.
- [ ] The section has a 2px `#E8E8E8` bottom border.

---

### US-3: View Monthly Execution Heatmap

**As a** program manager, **I want** to see a color-coded grid showing what was Shipped, In Progress, Carried Over, and Blocked for each month, **so that** executives can quickly assess execution health and spot trends.

**Visual Reference:** Heatmap section of `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` elements.

**Acceptance Criteria:**
- [ ] A section title "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" renders in uppercase, 14px bold, `#888`.
- [ ] The heatmap renders as a CSS Grid with columns: 160px (row header) + N equal-width data columns (one per month).
- [ ] Grid rows: 36px header row + 4 data rows (one per status category).
- [ ] Column headers show month names at 16px bold. The highlighted month (from `data.json`) gets a gold background (`#FFF0D0`) with `#C07700` text.
- [ ] Row headers are uppercase, 11px bold, color-coded per category: Shipped (`#1B7A28` on `#E8F5E9`), In Progress (`#1565C0` on `#E3F2FD`), Carryover (`#B45309` on `#FFF8E1`), Blockers (`#991B1B` on `#FEF2F2`).
- [ ] Each data cell contains a list of work item names, each prefixed with a 6px colored dot matching the row's category color.
- [ ] Cells in the highlighted month column have a deeper background tint (e.g., Shipped: `#D8F2DA` instead of `#F0FBF0`).
- [ ] Empty cells display a muted dash (`—`) in `#AAA`.
- [ ] All heatmap data is sourced from `data.json` `heatmap` object.
- [ ] The heatmap fills remaining vertical space below the timeline (flex: 1).

---

### US-4: Load Dashboard Data from JSON File

**As a** program manager, **I want** the dashboard to read all of its data from a single `data.json` file, **so that** I can update project status by editing one file without touching any code.

**Acceptance Criteria:**
- [ ] The application reads `data.json` from `wwwroot/data/data.json` at startup.
- [ ] The JSON schema matches the defined C# record models (`DashboardData`, `HeaderInfo`, `TimelineTrack`, `Milestone`, `HeatmapData`, `HeatmapRow`).
- [ ] A `schemaVersion` field is present in the JSON and validated on load.
- [ ] Changes to `data.json` are reflected on page refresh without rebuilding the application.
- [ ] The JSON file includes fictional example data for a "Project Phoenix" with 3 timeline tracks, 4 months of heatmap data, and realistic work item names.

---

### US-5: Capture Screenshot-Ready Output

**As a** program manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, **so that** I can take a full-page browser screenshot and paste it directly into a PowerPoint slide.

**Acceptance Criteria:**
- [ ] The `<body>` element is styled to `width: 1920px; height: 1080px; overflow: hidden`.
- [ ] All content fits within the viewport without scrolling.
- [ ] The Segoe UI font renders correctly on Windows.
- [ ] No browser chrome, scrollbars, or Blazor framework UI (e.g., reconnection overlay) appears in the viewport.
- [ ] The rendered output visually matches the `OriginalDesignConcept.html` reference when viewed at 100% zoom in Chrome.

---

### US-6: Run the Dashboard Locally

**As a** program manager, **I want** to start the dashboard with a single command (`dotnet run` or `dotnet watch`), **so that** I don't need to configure servers, databases, or cloud services.

**Acceptance Criteria:**
- [ ] `dotnet run` starts the application and opens on `http://localhost:5000` or `https://localhost:5001`.
- [ ] No external dependencies (databases, APIs, cloud services) are required.
- [ ] `dotnet watch` enables hot reload for CSS and Razor changes during development.
- [ ] Zero NuGet packages beyond what ships with the .NET 8 SDK are required.

---

### US-7: Handle Missing or Malformed Data Gracefully

**As a** program manager, **I want** the dashboard to show a clear error message if `data.json` is missing or contains invalid data, **so that** I know exactly what to fix rather than seeing a blank or broken page.

**Acceptance Criteria:**
- [ ] If `data.json` is not found, the page displays a centered error message: "data.json not found. Place your data file at wwwroot/data/data.json."
- [ ] If `data.json` contains invalid JSON or schema mismatches, the page displays the deserialization error message in a user-friendly format.
- [ ] The error page uses the same Segoe UI font and a simple, clean layout.
- [ ] The application does not crash or show a stack trace in the browser.

---

### US-8: Support Multiple Data Files

**As a** program manager, **I want** to switch between different project data files using a query parameter (e.g., `?data=project-b.json`), **so that** I can generate dashboards for multiple projects without renaming files.

**Acceptance Criteria:**
- [ ] The dashboard accepts an optional `?data=<filename>` query parameter.
- [ ] If provided, the application loads `wwwroot/data/<filename>` instead of the default `data.json`.
- [ ] If the specified file does not exist, the error handling from US-7 applies.
- [ ] File paths are sanitized to prevent directory traversal (e.g., `../` is rejected).
- [ ] If no query parameter is provided, `data.json` is loaded by default.

---

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` in the ReportingDashboard repository. Engineers MUST consult this file and the rendered screenshot (`OriginalDesignConcept.png`) for pixel-level implementation guidance.

![OriginalDesignConcept design reference](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/main/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Layout

- **Viewport:** Fixed 1920×1080 pixels, no scroll, white (`#FFFFFF`) background.
- **Font:** `'Segoe UI', Arial, sans-serif` — system font, no web font loading.
- **Layout Direction:** Vertical flex column (`display: flex; flex-direction: column`).
- **Three main sections stacked vertically:**
  1. **Header** (flex-shrink: 0, ~50px)
  2. **Timeline Area** (flex-shrink: 0, fixed 196px height)
  3. **Heatmap Area** (flex: 1, fills remaining ~834px)

### Section 1: Header Bar (`.hdr`)

- **Padding:** 12px top, 44px left/right, 10px bottom.
- **Border:** 1px solid `#E0E0E0` bottom.
- **Layout:** Flexbox, `align-items: center`, `justify-content: space-between`.
- **Left side:**
  - `<h1>` — Project title, 24px, font-weight 700, color `#111`.
  - Inline `<a>` link to ADO backlog, color `#0078D4`, no underline.
  - `.sub` — Subtitle line, 12px, color `#888`, margin-top 2px. Format: "Org · Workstream · Month Year".
- **Right side — Legend:**
  - Horizontal flex row with 22px gap.
  - Each legend item: 12px font, flex row with 6px gap.
  - **PoC Milestone:** 12×12px square, background `#F4B400`, rotated 45° (diamond).
  - **Production Release:** 12×12px square, background `#34A853`, rotated 45° (diamond).
  - **Checkpoint:** 8×8px circle, background `#999`.
  - **Now line:** 2×14px rectangle, background `#EA4335`.

### Section 2: Timeline Area (`.tl-area`)

- **Height:** 196px fixed.
- **Background:** `#FAFAFA`.
- **Border:** 2px solid `#E8E8E8` bottom.
- **Padding:** 6px top, 44px left/right.
- **Layout:** Flexbox, `align-items: stretch`.

#### 2a: Track Label Sidebar (left, 230px)

- **Width:** 230px, flex-shrink 0.
- **Border:** 1px solid `#E0E0E0` right.
- **Padding:** 16px top/bottom, 12px right.
- **Layout:** Flex column, `justify-content: space-around`.
- **Each track label:**
  - Track ID (e.g., "M1") — 12px, font-weight 600, color = track color.
  - Description (e.g., "API Gateway & Auth") — 12px, font-weight 400, color `#444`.
  - Track colors from data: e.g., `#0078D4` (blue), `#00897B` (teal), `#546E7A` (blue-gray).

#### 2b: SVG Timeline Chart (right, flex: 1)

- **SVG dimensions:** width 1560, height 185, `overflow: visible`.
- **Month grid lines:** Vertical lines at equal intervals (~260px apart for 6 months), stroke `#bbb`, opacity 0.4.
- **Month labels:** `<text>` at top of each grid line, 11px, font-weight 600, color `#666`.
- **Track lines:** Horizontal `<line>` elements at Y offsets (~42, ~98, ~154 for 3 tracks), stroke = track color, stroke-width 3.
- **Milestone markers:**
  - **Checkpoint:** `<circle>` r=5–7, fill white, stroke = track color or `#888`, stroke-width 2.5.
  - **PoC:** `<polygon>` diamond (11px radius), fill `#F4B400`, with drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`).
  - **Production:** `<polygon>` diamond (11px radius), fill `#34A853`, same drop shadow.
  - **Small checkpoint dots:** `<circle>` r=4, fill `#999` (no stroke).
- **Date labels:** `<text>` 10px, color `#666`, `text-anchor: middle`, positioned above or below markers.
- **NOW line:** `<line>` full height, stroke `#EA4335`, stroke-width 2, `stroke-dasharray="5,3"`. `<text>` "NOW" in 10px bold `#EA4335`.

### Section 3: Heatmap Area (`.hm-wrap`)

- **Padding:** 10px top/bottom, 44px left/right.
- **Layout:** Flex column, flex: 1.

#### 3a: Heatmap Title (`.hm-title`)

- **Text:** "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"
- **Style:** 14px, font-weight 700, color `#888`, uppercase, letter-spacing 0.5px, margin-bottom 8px.

#### 3b: Heatmap Grid (`.hm-grid`)

- **Layout:** CSS Grid.
- **Columns:** `160px repeat(N, 1fr)` where N = number of months.
- **Rows:** `36px repeat(4, 1fr)`.
- **Border:** 1px solid `#E0E0E0` outer.

**Corner cell (`.hm-corner`):**
- Background `#F5F5F5`, 11px bold uppercase, color `#999`, text "STATUS".
- Border-right 1px `#E0E0E0`, border-bottom 2px `#CCC`.

**Column headers (`.hm-col-hdr`):**
- 16px bold, centered, background `#F5F5F5`.
- Border-right 1px `#E0E0E0`, border-bottom 2px `#CCC`.
- **Highlighted month:** background `#FFF0D0`, color `#C07700`.

**Row headers (`.hm-row-hdr`):**
- 11px bold uppercase, letter-spacing 0.7px, padding 0 12px.
- Border-right 2px `#CCC`, border-bottom 1px `#E0E0E0`.
- Color-coded per category (see color palette below).

**Data cells (`.hm-cell`):**
- Padding 8px 12px.
- Border-right 1px `#E0E0E0`, border-bottom 1px `#E0E0E0`.
- Each item (`.it`): 12px, color `#333`, padding-left 12px, line-height 1.35.
- 6×6px colored dot (`::before` pseudo-element) positioned left of each item text.

### Color Palette

| Category | Row Header Text | Row Header BG | Cell BG | Cell Highlight BG | Dot Color |
|----------|----------------|---------------|---------|-------------------|-----------|
| **Shipped** | `#1B7A28` | `#E8F5E9` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| **In Progress** | `#1565C0` | `#E3F2FD` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| **Carryover** | `#B45309` | `#FFF8E1` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| **Blockers** | `#991B1B` | `#FEF2F2` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

| Element | Color |
|---------|-------|
| Background | `#FFFFFF` |
| Primary text | `#111` |
| Secondary text | `#888` |
| Muted text | `#999` |
| Dark text | `#333` |
| Link | `#0078D4` |
| Border light | `#E0E0E0` |
| Border heavy | `#CCC` |
| Subtle background | `#FAFAFA` |
| Header background | `#F5F5F5` |
| NOW line | `#EA4335` |
| PoC milestone | `#F4B400` |
| Prod milestone | `#34A853` |
| Highlight column BG | `#FFF0D0` |
| Highlight column text | `#C07700` |

---

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Dashboard Renders Successfully**
The user navigates to `http://localhost:5000`. The Blazor Server app loads `data.json`, deserializes it, and renders the full dashboard. The header, timeline, and heatmap all appear simultaneously within 2 seconds. The viewport is exactly 1920×1080 with no scrollbars. The "NOW" line is positioned at today's date on the timeline.

**Scenario 2: User Views the Header and Identifies the Project**
The user sees the project title ("Project Phoenix Release Roadmap") in bold at the top-left, with a blue hyperlink to the ADO backlog. Below it, the subtitle shows "Engineering Platform · Core Infrastructure Workstream · April 2026." The legend on the right shows four symbol types. No interaction is required—this is a read-only view.

**Scenario 3: User Reads the Milestone Timeline**
The user looks at the timeline section. Three horizontal tracks are visible, each color-coded (blue, teal, blue-gray). The left sidebar labels each track (M1: "API Gateway & Auth", M2: "PDS & Data Inventory", M3: "Auto Review DFD"). Along each track, milestone markers appear at their date positions: open circles for checkpoints, amber diamonds for PoC milestones, green diamonds for production releases. Date labels appear near each marker. A dashed red vertical line labeled "NOW" indicates the current date.

**Scenario 4: User Hovers Over a Milestone Diamond and Sees a Tooltip (Phase 3)**
The user hovers over an amber PoC diamond on the M1 track. A lightweight tooltip appears showing: "Mar 26 PoC — API Gateway & Auth — Proof of Concept milestone." The tooltip disappears when the user moves the cursor away. *(Note: This is a Phase 3 enhancement. For MVP, no hover interaction exists.)*

**Scenario 5: User Scans the Monthly Execution Heatmap**
The user looks at the heatmap grid below the timeline. Four rows (Shipped, In Progress, Carryover, Blockers) are color-coded. Four month columns (January–April) show work items as bulleted text. The "April" column is highlighted with a warm gold background, indicating the current reporting month. The user can quickly see that 2 items shipped in April, 2 are in progress, 1 carried over, and 1 blocker exists.

**Scenario 6: User Clicks the ADO Backlog Link**
The user clicks the blue "→ ADO Backlog" link in the header. The browser navigates to the URL specified in `data.json` `header.backlogLink` (e.g., `https://dev.azure.com/org/project/_backlogs`). The link opens in the same tab (standard anchor behavior).

**Scenario 7: Heatmap Renders with Empty Cells**
The `data.json` heatmap has an empty array `[]` for the Carryover row in January. The dashboard renders that cell with a muted dash (`—`) in `#AAA` instead of blank space, maintaining grid visual consistency.

**Scenario 8: Data File is Missing — Error State**
The user deletes or renames `data.json` and refreshes the page. Instead of a blank page or stack trace, the dashboard displays a centered, clean error message: "data.json not found. Place your data file at wwwroot/data/data.json." The error page uses the same Segoe UI font on a white background.

**Scenario 9: Data File Has Invalid JSON — Error State**
The user introduces a syntax error in `data.json` (e.g., trailing comma). On page refresh, the dashboard displays a friendly error: "Failed to load dashboard data: [specific JSON parse error]." The message includes enough detail to locate the issue.

**Scenario 10: User Switches Projects via Query Parameter**
The user navigates to `http://localhost:5000?data=project-b.json`. The app loads `wwwroot/data/project-b.json` instead of the default. A different project's data renders. Navigating to `http://localhost:5000` (no parameter) reverts to the default `data.json`.

**Scenario 11: User Takes a Screenshot for PowerPoint**
The user opens Chrome, navigates to the dashboard at 1920×1080 (using DevTools device toolbar or a maximized window on a 1080p display). They press Ctrl+Shift+S or use a screenshot tool. The captured image is a clean, presentation-ready visual with no browser chrome, no scrollbars, and pixel-perfect rendering matching the design reference.

**Scenario 12: User Edits data.json and Refreshes**
The user opens `wwwroot/data/data.json` in a text editor, changes the title from "Project Phoenix" to "Project Mercury," saves the file, and refreshes the browser. The dashboard immediately shows "Project Mercury Release Roadmap" in the header. No rebuild or restart is needed.

**Scenario 13: Timeline Renders with Milestones Outside Visible Range**
The user adds a milestone dated December 2025 (before the timeline's January start). The dashboard clamps the milestone's X position to the left edge of the SVG (x=0) rather than rendering it off-screen or causing layout breakage.

---

## Scope

### In Scope

- Single-page Blazor Server web application (.NET 8)
- Header section with project title, subtitle, ADO backlog link, and legend
- SVG timeline visualization with multiple tracks, milestone markers (checkpoint, PoC, production), month grid lines, and "NOW" indicator
- CSS Grid heatmap with 4 status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- Current month column highlighting
- Data loading from local `data.json` file via `System.Text.Json`
- Fictional example data for a "Project Phoenix" project
- Strongly-typed C# record data models with `required` properties
- `DashboardDataService` singleton for data loading and caching
- CSS custom properties for the full color palette
- Fixed 1920×1080 viewport for screenshot capture
- Graceful error handling for missing or malformed `data.json`
- Query parameter support for switching between data files (`?data=filename.json`)
- `dotnet watch` hot reload support for development
- `@media print` CSS rules for browser printing
- Segoe UI system font (no web font loading)
- Schema versioning via `schemaVersion` field in `data.json`

### Out of Scope

- ❌ Authentication or authorization of any kind
- ❌ Database (SQLite, SQL Server, Cosmos, or otherwise)
- ❌ REST API or GraphQL endpoints
- ❌ Admin panel or data entry UI for editing `data.json`
- ❌ Multi-user or concurrent access support
- ❌ Real-time data updates or WebSocket push
- ❌ Docker containerization
- ❌ CI/CD pipeline
- ❌ Logging infrastructure (beyond console output for debugging)
- ❌ Unit tests or integration tests for MVP
- ❌ Responsive/mobile layout (fixed 1920×1080 only)
- ❌ Dark mode (deferred to Phase 3)
- ❌ Playwright screenshot automation (deferred to Phase 3)
- ❌ Tooltip hover interactions (deferred to Phase 3)
- ❌ PDF export
- ❌ Integration with ADO APIs or any external data source
- ❌ CSS frameworks (Bootstrap, Tailwind)
- ❌ JavaScript charting libraries (Chart.js, Plotly, Radzen)
- ❌ Reverse proxy, IIS, or Docker hosting

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** | < 2 seconds from `dotnet run` to fully rendered dashboard |
| **Data file size** | Support `data.json` up to 500KB without degradation |
| **SVG rendering** | Timeline renders with up to 10 tracks × 20 milestones each without jank |
| **Heatmap rendering** | Support up to 8 month columns × 4 rows × 10 items per cell |

### Security

- **No authentication required.** The application binds to `localhost` only.
- **File path sanitization:** The `?data=` query parameter MUST reject paths containing `..`, `/`, or `\` to prevent directory traversal.
- **No sensitive data.** The dashboard displays project names, dates, and work item titles only.
- **No external network calls.** The application makes zero outbound HTTP requests.

### Reliability

- **Graceful degradation:** Missing or malformed `data.json` produces a clear error page, not a crash.
- **Startup validation:** C# `required` record properties catch schema errors at deserialization time with actionable error messages.
- **No persistent state:** The application is stateless. Restarting (`dotnet run`) restores full functionality.

### Compatibility

- **Browser:** Chrome (latest) at 1920×1080 viewport. Edge and Firefox are secondary targets but not tested.
- **OS:** Windows 10/11 with .NET 8 SDK installed.
- **Font:** Segoe UI (pre-installed on all Windows machines).

### Maintainability

- **Total file count:** Under 15 files in the project.
- **External NuGet packages:** Zero.
- **Single `dashboard.css` file:** All styles in one file, using CSS custom properties for theming.
- **Component composition:** Dashboard page composed of 3 Razor sub-components (Header, Timeline, Heatmap).

---

## Success Metrics

| # | Metric | Target | How to Measure |
|---|--------|--------|----------------|
| 1 | **Screenshot fidelity** | Rendered dashboard at 1920×1080 is visually indistinguishable from the `OriginalDesignConcept.html` reference | Side-by-side comparison in image viewer |
| 2 | **Data-to-screenshot time** | < 30 seconds from editing `data.json` to capturing a screenshot | Manual timing: save file → refresh browser → capture |
| 3 | **Zero-config startup** | `dotnet run` with no prior setup produces a working dashboard | Fresh clone → `dotnet run` → browser opens successfully |
| 4 | **Data file editability** | A non-engineer PM can correctly modify `data.json` to update project data | PM successfully updates title, adds a milestone, and adds a heatmap item without assistance |
| 5 | **Error handling coverage** | 100% of error scenarios (missing file, bad JSON, bad schema) produce user-friendly messages | Test each error scenario manually |
| 6 | **Build success** | `dotnet build` completes with zero errors and zero warnings | CI or local build output |
| 7 | **External dependency count** | Zero external NuGet packages | Inspect `.csproj` file |

---

## Constraints & Assumptions

### Technical Constraints

1. **Must use .NET 8 and Blazor Server.** The existing repository and team skillset are C#/.NET. No alternative frameworks (React, Vue, plain HTML) are permitted.
2. **Fixed 1920×1080 viewport.** The dashboard is designed for screenshot capture at this exact resolution. No responsive or mobile layout is required.
3. **Segoe UI font only.** No web fonts, no Google Fonts, no font CDN. Segoe UI ships with Windows.
4. **No JavaScript.** The entire UI must be implementable in Razor + CSS + inline SVG. No JS interop, no npm packages.
5. **Localhost only.** The application binds to `localhost`. No network exposure, no SSL certificate management beyond Kestrel defaults.
6. **Single data source.** All dashboard data comes from `data.json`. No database, no API, no environment variables for data.

### Timeline Assumptions

1. **Phase 1 (MVP):** 1–2 developer days. Pixel-perfect reproduction of the design, data-driven from `data.json`.
2. **Phase 2 (Polish):** 1 developer day. Dynamic date calculations, error handling, hot reload verification.
3. **Phase 3 (Nice-to-haves):** Optional, 0.5–1 day each. Playwright automation, tooltips, dark mode, print CSS.

### Dependency Assumptions

1. **.NET 8 SDK** is installed on the developer's machine.
2. **Chrome browser** is available for screenshot capture and viewport testing.
3. **Windows OS** is the target platform (for Segoe UI font availability).
4. **The `OriginalDesignConcept.html` file** in the ReportingDashboard repository is the canonical design reference and will not change during implementation.
5. **The PM will maintain `data.json` manually.** No automated data pipeline or ADO integration is expected.
6. **The fictional "Project Phoenix" example data** is sufficient for initial demonstration; real project data will be substituted by the PM post-delivery.

### Open Decisions

| # | Question | Recommended Answer | Status |
|---|----------|--------------------|--------|
| 1 | Should the timeline date range auto-calculate from data or be configurable? | Configurable via `startDate`/`endDate` in `data.json` | **Decided** |
| 2 | Should heatmap column count be dynamic? | Yes, driven by `columns` array in `data.json`, max 8 | **Decided** |
| 3 | Should the highlighted month be auto-detected or explicit? | Configurable via `highlightColumn` in `data.json`, default to current month if omitted | **Decided** |
| 4 | Should multiple data files be supported? | Yes, via `?data=filename.json` query parameter | **Decided** |
| 5 | Should print CSS be included in MVP? | Yes, as a quick win (`@media print` rules) | **Decided** |