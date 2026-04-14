# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, delivery status, and blockers in a format optimized for PowerPoint screenshot capture. The dashboard reads all data from a local `data.json` configuration file and renders a timeline with milestone markers plus a color-coded heatmap grid showing Shipped, In Progress, Carryover, and Blockers by month—giving executives instant visibility into project health without requiring them to navigate ADO or other tools.

## Business Goals

1. **Reduce executive reporting prep time by 80%** — Replace manual PowerPoint slide construction with a single screenshot from a live, data-driven dashboard that renders at exactly 1920×1080.
2. **Provide a single-glance project health view** — Combine milestone timeline progress and monthly execution status (shipped, in-progress, carryover, blockers) into one unified page that answers "how is the project going?" in under 5 seconds.
3. **Enable rapid data updates without code changes** — All project data lives in a human-editable `data.json` file, allowing PMs to update milestones and status items in seconds and immediately see the refreshed dashboard.
4. **Maintain zero operational overhead** — No authentication, no database, no cloud deployment, no CI/CD pipeline. The dashboard runs locally via `dotnet run` and requires zero infrastructure.
5. **Support multiple project reporting** — Allow different `data.json` files (one per project) so the same dashboard can generate screenshots for multiple executive review decks.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project manager, **I want** to see the project title, organizational context, current month, and a link to the ADO backlog at the top of the dashboard, **so that** every screenshot I capture is self-documenting and executives know exactly which project and time period they are looking at.

**Visual Reference:** Header section (`.hdr`) of `OriginalDesignConcept.html`

- [ ] Dashboard displays the project title in bold 24px font on the left side of the header
- [ ] An "→ ADO Backlog" hyperlink appears inline with the title, styled in `#0078D4` blue
- [ ] A subtitle line below the title shows the organization, workstream, and current month (e.g., "Trusted Platform · Privacy Automation Workstream · April 2026") in 12px muted gray (`#888`)
- [ ] A legend appears on the right side of the header with four marker types: PoC Milestone (gold diamond `#F4B400`), Production Release (green diamond `#34A853`), Checkpoint (gray circle `#999`), and Now line (red vertical bar `#EA4335`)
- [ ] All header text and values are driven by `data.json` fields (`title`, `subtitle`, `backlogUrl`)
- [ ] The header is separated from the timeline below by a 1px `#E0E0E0` bottom border

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing key project milestones across multiple workstreams with clear visual markers for checkpoints, PoC dates, and production releases, **so that** I can quickly understand the project schedule and where we are relative to plan.

**Visual Reference:** Timeline area (`.tl-area`) and SVG section of `OriginalDesignConcept.html`

- [ ] A left sidebar (230px wide) lists milestone track labels (e.g., "M1 — Chatbot & MS Role", "M2 — PDS & Data Inventory") with each track's ID in bold and description in regular weight, color-coded per milestone
- [ ] The SVG timeline area (1560px wide, 185px tall) renders month gridlines (Jan through Jun) as faint vertical lines with month labels at the top
- [ ] Each milestone track renders as a colored horizontal line spanning the full SVG width
- [ ] Checkpoint markers render as open circles (white fill, colored stroke) at the appropriate date position
- [ ] PoC milestone markers render as gold (`#F4B400`) diamond shapes with a drop shadow
- [ ] Production release markers render as green (`#34A853`) diamond shapes with a drop shadow
- [ ] A red dashed vertical "NOW" line (`#EA4335`, stroke-dasharray 5,3) appears at the current date position with a "NOW" label
- [ ] Date labels appear above or below each marker (e.g., "Jan 12", "Mar 26 PoC")
- [ ] The timeline supports 1–5 milestone tracks, driven by the `milestones` array in `data.json`
- [ ] The timeline background is `#FAFAFA` with a 2px `#E8E8E8` bottom border separating it from the heatmap

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked — organized by month — **so that** I can assess execution velocity and identify problem areas at a glance.

**Visual Reference:** Heatmap section (`.hm-wrap`, `.hm-grid`) of `OriginalDesignConcept.html`

- [ ] A section title "MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS" appears above the grid in 14px uppercase muted gray with 0.5px letter spacing
- [ ] The grid uses CSS Grid with columns: 160px (row headers) + N equal-width columns (one per month in `data.json`)
- [ ] Column headers show month names in 16px bold; the current month column is highlighted with a gold background (`#FFF0D0`) and amber text (`#C07700`)
- [ ] A "Status" corner cell appears at top-left in 11px uppercase gray
- [ ] Four status rows render in order: ✅ Shipped (green), 🔄 In Progress (blue), ⏳ Carryover (amber), 🚫 Blockers (red)
- [ ] Each row header uses category-specific colors (text + background) as defined in the design
- [ ] Data cells show individual items as bulleted lists with 6px colored dot indicators matching the row category
- [ ] The current month's data cells use a darker background shade than other months
- [ ] Empty cells display a muted dash ("—") character
- [ ] Item text is 12px, color `#333`, with `line-height: 1.35` and `padding: 2px 0 2px 12px`
- [ ] The grid has a 1px `#E0E0E0` outer border, 1px cell borders, and 2px heavy borders (`#CCC`) between header and data rows/columns
- [ ] The heatmap fills all remaining vertical space below the timeline

### US-4: Configure Dashboard via JSON

**As a** project manager, **I want** to edit a single `data.json` file to update all dashboard content (title, milestones, heatmap items), **so that** I can refresh the dashboard without touching any code.

- [ ] All displayed data is sourced from `data.json` — no hardcoded content exists in the UI
- [ ] The JSON schema includes: `title`, `subtitle`, `backlogUrl`, `currentDate`, `months`, `currentMonthIndex`, `timelineStart`, `timelineEnd`, `milestones[]`, and `categories[]`
- [ ] Each milestone object contains: `id`, `label`, `description`, `color`, and `markers[]` (each with `date`, `type`, `label`)
- [ ] Each category object contains: `name`, `key`, and `items{}` (a dictionary of month → string array)
- [ ] Invalid or missing JSON fields produce a clear error message at startup, not a blank page
- [ ] The dashboard auto-reloads when `data.json` is modified (via `FileSystemWatcher`), without requiring an app restart

### US-5: Capture Pixel-Perfect Screenshots

**As a** project manager, **I want** the dashboard to render at exactly 1920×1080 with no scrollbars, dynamic elements, or layout shifts, **so that** my browser screenshot captures are clean and ready to paste directly into PowerPoint.

- [ ] The page body is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] No scrollbars appear at any viewport size
- [ ] All content fits within the 1920×1080 frame without clipping (for up to 5 milestone tracks and 6 months with up to 5 items per heatmap cell)
- [ ] No loading spinners, skeleton screens, or progressive rendering — the page renders fully on first paint
- [ ] The dashboard renders identically in Chrome, Edge, and any Chromium-based browser
- [ ] A screenshot captured via Chrome DevTools "Capture full size screenshot" at 1920×1080 matches the `OriginalDesignConcept.html` reference design

### US-6: Support Multiple Projects

**As a** project manager managing multiple workstreams, **I want** to load different project data files via a URL query parameter, **so that** I can generate separate dashboard screenshots for each project without switching config files.

- [ ] Navigating to `/?project=phoenix` loads `data.phoenix.json` (or `projects/phoenix.json`)
- [ ] Navigating to `/` with no parameter loads the default `data.json`
- [ ] If the requested project file does not exist, a clear error message is displayed
- [ ] Each project's dashboard is fully independent — different titles, milestones, categories, and months

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository. All UI implementations MUST match this file's visual output. See also the rendered screenshot: `docs/design-screenshots/OriginalDesignConcept.png`.

### Overall Page Layout

- **Viewport:** Fixed 1920×1080px, no responsive breakpoints, `overflow: hidden`
- **Background:** `#FFFFFF`
- **Font Family:** `'Segoe UI', Arial, sans-serif`
- **Base Text Color:** `#111`
- **Layout:** Vertical flex column (`display: flex; flex-direction: column`) with three sections stacked top-to-bottom:
  1. Header (flex-shrink: 0, ~50px)
  2. Timeline Area (flex-shrink: 0, fixed 196px height)
  3. Heatmap (flex: 1, fills remaining space)

### Section 1: Header (`.hdr`)

- **Padding:** `12px 44px 10px`
- **Layout:** Flexbox row, `align-items: center; justify-content: space-between`
- **Border:** 1px solid `#E0E0E0` bottom
- **Left Side:**
  - Title: `<h1>` at 24px, font-weight 700, containing project name + "→ ADO Backlog" link in `#0078D4`
  - Subtitle: 12px, color `#888`, margin-top 2px
- **Right Side (Legend):** Flexbox row with 22px gap, 12px font size
  - PoC Milestone: 12×12px square rotated 45° (diamond), background `#F4B400`
  - Production Release: 12×12px diamond, background `#34A853`
  - Checkpoint: 8×8px circle, background `#999`
  - Now line: 2×14px vertical bar, background `#EA4335`

### Section 2: Timeline Area (`.tl-area`)

- **Height:** 196px fixed
- **Background:** `#FAFAFA`
- **Padding:** `6px 44px 0`
- **Border:** 2px solid `#E8E8E8` bottom
- **Layout:** Flexbox row, `align-items: stretch`
- **Left Sidebar (Milestone Labels):**
  - Width: 230px, flex-shrink: 0
  - Border-right: 1px solid `#E0E0E0`
  - Padding: `16px 12px 16px 0`
  - Layout: Flexbox column, `justify-content: space-around`
  - Each label: 12px, font-weight 600, line-height 1.4
    - Track ID (e.g., "M1") in milestone color (e.g., `#0078D4`)
    - Description in font-weight 400, color `#444`
  - Milestone track colors from reference: M1 = `#0078D4`, M2 = `#00897B`, M3 = `#546E7A`
- **Right SVG Area (`.tl-svg-box`):**
  - Flex: 1, padding-left: 12px, padding-top: 6px
  - SVG: width="1560", height="185", `overflow: visible`
  - **Month Gridlines:** Vertical lines at equal intervals (every 260px for 6 months), stroke `#bbb` opacity 0.4, width 1
  - **Month Labels:** 11px, font-weight 600, fill `#666`, positioned 5px right of each gridline at y=14
  - **NOW Line:** Vertical dashed line, stroke `#EA4335`, width 2, dasharray "5,3"; "NOW" label in 10px bold red
  - **Milestone Tracks:** Horizontal lines spanning full SVG width, stroke width 3, color per milestone
  - **Track vertical positions:** Evenly spaced (approximately y=42, y=98, y=154 for 3 tracks)
  - **Markers:**
    - Checkpoint: `<circle>` r=5–7, fill white, stroke colored, stroke-width 2.5
    - PoC Diamond: `<polygon>` forming an 11px diamond, fill `#F4B400`, with drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`)
    - Production Diamond: Same shape, fill `#34A853`, same shadow
    - Small checkpoint dots: `<circle>` r=4, fill `#999`
  - **Marker Labels:** 10px, fill `#666`, text-anchor middle, positioned above or below markers

### Section 3: Heatmap (`.hm-wrap`)

- **Padding:** `10px 44px 10px`
- **Layout:** Flexbox column, `flex: 1; min-height: 0`
- **Title Bar (`.hm-title`):**
  - Font: 14px, weight 700, color `#888`
  - Letter-spacing: 0.5px, text-transform: uppercase
  - Margin-bottom: 8px
  - Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"
- **Grid (`.hm-grid`):**
  - Layout: CSS Grid
  - Columns: `160px repeat(N, 1fr)` where N = number of months
  - Rows: `36px repeat(4, 1fr)`
  - Outer border: 1px solid `#E0E0E0`
  - **Corner Cell:** Background `#F5F5F5`, 11px bold uppercase `#999`, text "STATUS"
  - **Column Headers:** 16px bold, background `#F5F5F5`, centered; border-bottom 2px `#CCC`
    - Current month: background `#FFF0D0`, color `#C07700`, text includes "→ Now"
  - **Row Headers:** 11px bold uppercase, letter-spacing 0.7px, padding `0 12px`, border-right 2px `#CCC`

#### Heatmap Color System (exact hex values from `OriginalDesignConcept.html`)

| Category | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Bullet Dot |
|----------|---------------|-----------------|---------|----------------------|------------|
| **Shipped** | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| **In Progress** | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| **Carryover** | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| **Blockers** | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

- **Data Cells:** Padding `8px 12px`, border-right 1px `#E0E0E0`, border-bottom 1px `#E0E0E0`
- **Item Text:** 12px, color `#333`, padding `2px 0 2px 12px`, line-height 1.35
- **Bullet Dots:** 6×6px circles via CSS `::before` pseudo-element, positioned absolutely at left:0, top:7px

### Component Hierarchy

```
MainLayout.razor (no nav, full-width shell)
└── Dashboard.razor (single page, route "/")
    ├── DashboardHeader.razor (title, subtitle, legend)
    ├── Timeline.razor (SVG timeline + milestone sidebar)
    └── Heatmap.razor (CSS Grid container)
        └── HeatmapCell.razor × N (individual item list cells)
```

### CSS Custom Properties (defined in `app.css :root`)

```
--color-shipped: #34A853          --color-shipped-bg: #F0FBF0
--color-shipped-bg-current: #D8F2DA   --color-shipped-header: #E8F5E9
--color-progress: #0078D4         --color-progress-bg: #EEF4FE
--color-progress-bg-current: #DAE8FB  --color-progress-header: #E3F2FD
--color-carryover: #F4B400        --color-carryover-bg: #FFFDE7
--color-carryover-bg-current: #FFF0B0 --color-carryover-header: #FFF8E1
--color-blockers: #EA4335         --color-blockers-bg: #FFF5F5
--color-blockers-bg-current: #FFE4E4  --color-blockers-header: #FEF2F2
--color-border: #E0E0E0           --color-border-heavy: #CCC
--color-text: #111                --color-text-muted: #888
--color-link: #0078D4
--font-family: 'Segoe UI', Arial, sans-serif
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Full Dashboard Render**
User navigates to `https://localhost:5001/`. The dashboard reads `data.json` and renders the complete page in a single server-side pass. The header, timeline, and heatmap all appear simultaneously with no loading spinner, skeleton screen, or progressive rendering. The page fits exactly within 1920×1080 with no scrollbars.

**Scenario 2: User Views the Header and Identifies the Project**
User sees the project title ("Project Phoenix Release Roadmap") in bold at the top left, with a blue "→ ADO Backlog" link inline. Below the title, a subtitle line shows the organization, workstream, and current month. On the right, four legend items explain the marker types used in the timeline. The user immediately understands which project and time period the dashboard represents.

**Scenario 3: User Reads the Milestone Timeline**
User looks at the timeline section and sees 1–5 horizontal colored lines representing milestone workstreams. Each track has a label in the left sidebar (e.g., "M1 — Core Platform"). Along each track, markers indicate checkpoint dates (open circles), PoC milestones (gold diamonds), and production releases (green diamonds). A red dashed "NOW" vertical line shows today's date. The user traces each track left-to-right to understand past achievements and upcoming targets.

**Scenario 4: User Hovers Over a Milestone Diamond**
Since this is a static, screenshot-optimized dashboard, no tooltip or hover state is implemented. All relevant information (date, label, type) is rendered as persistent text labels adjacent to each marker. This ensures no information is lost when the page is captured as a screenshot.

**Scenario 5: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" hyperlink in the header. The browser navigates to the Azure DevOps backlog URL specified in `data.json`. This link is functional in the live dashboard but will not be interactive in a PowerPoint screenshot (acceptable trade-off).

**Scenario 6: User Examines the Heatmap for Current Month Status**
User looks at the heatmap grid and immediately identifies the current month column by its highlighted gold header (`#FFF0D0` background, `#C07700` text with "→ Now"). The current month's data cells also use a darker shade across all four rows. The user scans the four rows top-to-bottom: green (Shipped) shows completed items, blue (In Progress) shows active work, amber (Carryover) shows items slipped from last month, and red (Blockers) shows impediments.

**Scenario 7: User Scans Heatmap for Trends Across Months**
User reads the heatmap left-to-right across columns to identify trends. A healthy project shows increasing items in the Shipped row and decreasing items in Carryover/Blockers rows over time. The color coding makes it possible to detect patterns without reading individual item text.

**Scenario 8: Data-Driven Rendering — Variable Month Count**
The heatmap renders as many month columns as specified in `data.json`. If the PM configures 6 months (Jan–Jun), the grid expands to `160px repeat(6, 1fr)`. If only 3 months are configured, the grid shows 3 columns. The layout adjusts automatically; the 160px row header width remains constant.

**Scenario 9: Data-Driven Rendering — Variable Milestone Count**
The timeline renders 1–5 milestone tracks based on the `milestones` array in `data.json`. Track vertical positions are calculated by dividing the 185px SVG height evenly. With 1 track, the line appears centered; with 5 tracks, they are evenly spaced at ~37px intervals.

**Scenario 10: Empty State — No Items in a Heatmap Cell**
When a heatmap cell has no items for a given month/category combination, the cell displays a single muted dash ("—") in `#AAA` color instead of being blank. This prevents the grid from looking broken and clearly communicates "nothing to report" for that intersection.

**Scenario 11: Error State — Missing or Invalid data.json**
If `data.json` is missing, malformed, or missing required fields, the dashboard displays a clear error message at startup in the console log (e.g., "Error: data.json not found at path X" or "Error: 'title' field is required in data.json"). The page renders a friendly error state rather than a blank page or unhandled exception.

**Scenario 12: Hot Reload — PM Edits data.json While Dashboard is Running**
The PM modifies `data.json` in a text editor (e.g., adds a new shipped item for April). The `FileSystemWatcher` detects the file change and the `DashboardDataService` reloads the data. On the next browser refresh (or via Blazor Server's SignalR push), the dashboard reflects the updated data without restarting the application.

**Scenario 13: Multi-Project — Loading a Different Project**
User navigates to `/?project=atlas`. The application looks for `data.atlas.json` (or `projects/atlas.json`) and renders that project's dashboard. The title, milestones, and heatmap all reflect the Atlas project data. Navigating back to `/` loads the default `data.json`.

**Scenario 14: Screenshot Capture Workflow**
User opens Chrome DevTools (F12), presses Ctrl+Shift+P, types "Capture full size screenshot", and presses Enter. Chrome captures the 1920×1080 dashboard as a PNG. The resulting image matches the reference design and is ready to paste into PowerPoint with no cropping or resizing needed.

**Scenario 15: No Responsive Behavior**
The page does not adapt to different screen sizes. If viewed on a smaller monitor, horizontal scrollbars appear in the browser. This is intentional — the dashboard is designed for a specific resolution and any responsive adaptation would compromise screenshot fidelity.

## Scope

### In Scope

- Single-page Blazor Server dashboard at route `/`
- Header component with project title, ADO backlog link, subtitle, and legend
- SVG timeline component with 1–5 milestone tracks, checkpoint/PoC/production markers, month gridlines, and "NOW" line
- CSS Grid heatmap component with 4 status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- Color-coded cells matching the exact palette from `OriginalDesignConcept.html`
- Current month highlighting in both column header and data cells
- `data.json` flat-file data source with strongly-typed C# model deserialization
- `FileSystemWatcher`-based hot reload of `data.json`
- Multi-project support via `?project=` query parameter
- Fixed 1920×1080 viewport for screenshot optimization
- CSS custom properties for design token management
- Fictional sample data for a "Project Phoenix" demo
- Unit tests for JSON deserialization validation
- Solution structure with `src/` and `tests/` directories

### Out of Scope

- Authentication or authorization of any kind
- Database or ORM (SQLite, LiteDB, EF Core, etc.)
- REST API endpoints
- Client-side interactivity (tooltips, hover effects, filters, animations, click handlers beyond the ADO link)
- Responsive design or mobile layout
- Automated screenshot pipeline (Playwright, Puppeteer)
- Multi-user or concurrent access handling
- Structured logging or log aggregation (beyond console)
- Docker containerization
- CI/CD pipeline
- Print stylesheet (`@media print`)
- Dark mode or theme switching
- Data editing UI — `data.json` is edited manually in a text editor
- ADO work item integration or automated data pipeline
- Export to PDF or image from within the app
- Accessibility compliance (WCAG) — this is a screenshot tool, not a user-facing application
- Internationalization or localization

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Application startup | < 2 seconds (cold start, local) |
| Page render (first paint) | < 500ms after navigation |
| `data.json` reload after file change | < 1 second |
| Memory footprint | < 100 MB RSS |
| JSON file size support | Up to 500 KB (covers hundreds of items) |

### Security

- **Authentication:** None required. The application runs on `localhost` only.
- **Data Classification:** Project status data only — no PII, no secrets, no credentials.
- **Network Exposure:** Kestrel binds to `localhost` by default. No external network access.
- **Sensitive Data:** If `data.json` contains sensitive project names, the file should be added to `.gitignore`.

### Reliability

- **Availability Target:** N/A — this is a local developer tool, not a service.
- **Error Handling:** Malformed `data.json` must produce a clear console error and render a user-friendly error message, not an unhandled exception or blank page.
- **Graceful Degradation:** If a milestone has zero markers, the track line still renders. If a category has zero items for all months, the row still renders with dashes.

### Compatibility

- **Browser Support:** Any Chromium-based browser (Chrome, Edge). Firefox and Safari are not required.
- **OS:** Windows 10/11 (developer's machine). macOS/Linux would work via .NET 8 cross-platform support but is not a testing target.
- **Resolution:** Fixed 1920×1080. No other resolutions are supported or tested.

### Maintainability

- **External Production Dependencies:** Zero. All functionality uses built-in .NET 8 SDK libraries.
- **Component Architecture:** Each visual section maps to exactly one Razor component with its own `.razor.css` file for style isolation.
- **Design Tokens:** All colors, fonts, and spacing values are defined as CSS custom properties in a single `app.css` file.

## Success Metrics

1. **Visual Fidelity:** A Chrome DevTools screenshot of the running dashboard is visually indistinguishable from the `OriginalDesignConcept.html` reference at normal viewing distance (verified by side-by-side comparison).
2. **Data-Driven Rendering:** Changing any value in `data.json` and refreshing the browser correctly reflects the change in the UI — verified for: title, subtitle, milestone labels, milestone markers, heatmap items, current month highlighting.
3. **Zero-Code Update Workflow:** A non-developer PM can update `data.json` in Notepad, save the file, and see the dashboard update within 2 seconds — without restarting the application or editing any code.
4. **Screenshot Readiness:** A full-page screenshot at 1920×1080 contains no scrollbars, no loading indicators, no clipped content, and no layout artifacts — ready to paste into PowerPoint.
5. **Build Success:** `dotnet build` completes with zero errors and zero warnings. `dotnet test` passes all unit tests for JSON deserialization.
6. **Startup Simplicity:** A new developer can clone the repo, run `dotnet run`, open `localhost:5001`, and see the dashboard with sample data — no configuration, no database setup, no environment variables.
7. **Multi-Project Verified:** Navigating to `/?project=test` loads a different JSON file and renders a distinct dashboard, confirming multi-project support works end-to-end.

## Constraints & Assumptions

### Technical Constraints

- **Framework:** .NET 8 Blazor Server — already selected based on team expertise and existing solution structure. No framework change is permitted.
- **Zero External UI Libraries:** No MudBlazor, Radzen, Syncfusion, or other component libraries. The reference design is pure CSS and must be translated directly.
- **Zero Charting Libraries:** The SVG timeline is hand-built in Razor markup. No ApexCharts, Chart.js, or similar.
- **Fixed Resolution:** 1920×1080 only. Responsive design is explicitly prohibited as it would compromise screenshot fidelity.
- **System Font:** Segoe UI (ships with Windows). No web font downloads.
- **SVG Width:** Hard-coded to 1560px to match the reference design's container width (1920px minus 44px padding on each side minus 230px sidebar minus 12px gap).

### Timeline Assumptions

- **Phase 1 (Scaffold + Header):** ~4 hours
- **Phase 2 (Timeline SVG):** ~3 hours
- **Phase 3 (Heatmap Grid):** ~3 hours
- **Phase 4 (Polish + Testing):** ~2 hours
- **Total Estimate:** 1–2 developer-days to reach screenshot-ready quality
- **Immediate Demo:** A working dashboard with fictional data can be shown to stakeholders at end of Day 1

### Dependency Assumptions

- Developer machine has .NET 8 SDK installed (8.0.x latest patch)
- Developer has a Chromium-based browser for viewing and screenshotting
- No network access is required at runtime — all data is local
- The `OriginalDesignConcept.html` reference file in the ReportingDashboard repository is the authoritative design spec; any discrepancy between the HTML and the PNG screenshot, the HTML source wins
- `data.json` is maintained manually by the PM; no automated data pipeline exists or is planned for v1
- The dashboard will be used by 1 person at a time on localhost — no concurrency concerns

### Data Assumptions

- Heatmap will display 4–6 months of data (configurable in `data.json`)
- Timeline will display 1–5 milestone tracks (more than 5 would be visually cramped in 196px)
- Each heatmap cell will contain 0–8 items (more would overflow the cell at the default font size)
- The "NOW" date defaults to `DateTime.Now` but can be overridden via `currentDate` in `data.json` for generating historical/future snapshots
- Month names in `data.json` use 3-letter abbreviations (Jan, Feb, Mar, etc.)

### Open Decisions

1. **Month count policy:** Render exactly the months listed in `data.json` (data-driven). Recommended: support 4–6 months.
2. **Multi-project file convention:** Use `data.{project}.json` in the project root or `projects/{project}.json` subfolder. To be decided during implementation.
3. **Data update cadence:** Assumed monthly. If weekly, consider adding an ADO query export script in a future iteration.