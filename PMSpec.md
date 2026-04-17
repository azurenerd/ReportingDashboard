# PM Specification: Executive Project Reporting Dashboard

**Document Version:** 1.0
**Date:** April 17, 2026
**Author:** Program Management
**Status:** Draft

---

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes a project's milestone timeline, delivery status, and monthly execution heatmap in a fixed 1920×1080 layout optimized for PowerPoint screenshot capture. The dashboard reads all data from a local `data.json` configuration file, renders a pixel-perfect view matching the `OriginalDesignConcept.html` reference design, and requires no authentication, database, or cloud infrastructure — enabling any project lead to generate polished executive status slides with minimal effort.

---

## Business Goals

1. **Eliminate manual slide creation** — Provide a live-rendered dashboard that can be screenshotted directly into PowerPoint decks, replacing hours of manual formatting per reporting cycle.
2. **Standardize executive reporting** — Establish a consistent visual format for communicating project milestones, shipped work, in-progress items, carryover, and blockers across all workstreams.
3. **Maximize clarity for leadership** — Present project health at a glance using a color-coded heatmap and timeline so executives can assess status in under 30 seconds.
4. **Minimize operational overhead** — Deliver a zero-dependency, local-only tool that runs via `dotnet run` with no infrastructure cost, no authentication setup, and no ongoing maintenance burden.
5. **Enable rapid data updates** — Allow project leads to update a single `data.json` file and immediately see the refreshed dashboard, supporting weekly or bi-weekly reporting cadences.

---

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** to see the project title, organizational context, report date, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are viewing.

**Visual Reference:** `OriginalDesignConcept.html` — Header section (`.hdr`)

**Acceptance Criteria:**
- [ ] The page displays the project title as an H1 element (24px, bold, `#111`).
- [ ] A clickable ADO Backlog link appears inline with the title, styled in `#0078D4`.
- [ ] A subtitle line shows the organization, workstream name, and report month (12px, `#888`).
- [ ] All header text is data-driven from `data.json` fields: `title`, `subtitle`, `backlogUrl`.
- [ ] The header is separated from the timeline section by a 1px `#E0E0E0` bottom border.

### US-2: View Milestone Legend

**As an** executive viewer, **I want** to see a legend explaining the milestone marker types (PoC Milestone, Production Release, Checkpoint, Now line), **so that** I can interpret the timeline without external documentation.

**Visual Reference:** `OriginalDesignConcept.html` — Legend area (top-right of `.hdr`)

**Acceptance Criteria:**
- [ ] Four legend items are displayed horizontally in the header's right-aligned area with 22px gaps.
- [ ] PoC Milestone shows a 12×12px amber (`#F4B400`) diamond (rotated 45° square).
- [ ] Production Release shows a 12×12px green (`#34A853`) diamond (rotated 45° square).
- [ ] Checkpoint shows an 8×8px gray (`#999`) circle.
- [ ] "Now" shows a 2×14px red (`#EA4335`) vertical line.
- [ ] All legend labels are 12px Segoe UI.

### US-3: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline with workstream swim lanes showing milestone markers at their scheduled dates and a "NOW" indicator, **so that** I can understand where the project stands relative to its planned milestones.

**Visual Reference:** `OriginalDesignConcept.html` — Timeline area (`.tl-area` and inner SVG)

**Acceptance Criteria:**
- [ ] The timeline section is 196px tall with a `#FAFAFA` background and a 2px `#E8E8E8` bottom border.
- [ ] A left-side panel (230px wide) lists workstream labels (e.g., "M1 — Chatbot & MS Role") with workstream-specific colors.
- [ ] The SVG canvas (1560×185px) renders vertical month gridlines at proportionally spaced intervals.
- [ ] Month labels (Jan, Feb, Mar, etc.) appear at the top of each gridline in 11px bold `#666`.
- [ ] Each workstream renders as a colored horizontal line (3px stroke) at its designated Y position.
- [ ] PoC milestones render as amber (`#F4B400`) diamonds with drop shadow filter.
- [ ] Production milestones render as green (`#34A853`) diamonds with drop shadow filter.
- [ ] Checkpoints render as small circles — larger outlined circles for major checkpoints, smaller filled `#999` circles for minor ones.
- [ ] Each milestone has a date label (10px, `#666`) positioned above or below to avoid overlap.
- [ ] A dashed red (`#EA4335`) vertical "NOW" line appears at the X position corresponding to the current date (or `reportDate` from JSON).
- [ ] "NOW" text label appears at the top of the NOW line in 10px bold red.
- [ ] All milestone positions are calculated proportionally: `x = (date - rangeStart) / (rangeEnd - rangeStart) * 1560`.
- [ ] The number of workstreams and milestones is fully driven by `data.json`.

### US-4: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was Shipped, In Progress, Carried Over, and Blocked for each month, **so that** I can assess execution velocity and identify recurring issues.

**Visual Reference:** `OriginalDesignConcept.html` — Heatmap section (`.hm-wrap`, `.hm-grid`)

**Acceptance Criteria:**
- [ ] The heatmap has a title bar: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" (14px, bold, uppercase, `#888`).
- [ ] The grid uses CSS Grid: `160px repeat(N, 1fr)` where N = number of month columns from `data.json`.
- [ ] The first row contains column headers: a "STATUS" corner cell and month name headers (16px, bold).
- [ ] The current month's column header has a highlighted background (`#FFF0D0`) with amber text (`#C07700`) and a "▸ Now" indicator.
- [ ] Four data rows exist, one per status category:
  - **Shipped** (✓): Row header green text `#1B7A28` on `#E8F5E9`; cells `#F0FBF0`, current month `#D8F2DA`; bullet dots `#34A853`.
  - **In Progress** (◐): Row header blue text `#1565C0` on `#E3F2FD`; cells `#EEF4FE`, current month `#DAE8FB`; bullet dots `#0078D4`.
  - **Carryover** (↻): Row header amber text `#B45309` on `#FFF8E1`; cells `#FFFDE7`, current month `#FFF0B0`; bullet dots `#F4B400`.
  - **Blockers** (✕): Row header red text `#991B1B` on `#FEF2F2`; cells `#FFF5F5`, current month `#FFE4E4`; bullet dots `#EA4335`.
- [ ] Each cell displays a list of work items as 12px text with a 6×6px colored bullet (circle via `::before` pseudo-element).
- [ ] Empty future-month cells display a gray dash "-".
- [ ] All cell content is driven by `data.json` heatmap entries.
- [ ] The heatmap grid fills the remaining vertical space below the timeline (using `flex: 1`).

### US-5: Configure Dashboard via JSON

**As a** project lead, **I want** to edit a single `data.json` file to update all dashboard content (title, workstreams, milestones, heatmap items), **so that** I can refresh the report without modifying any code.

**Acceptance Criteria:**
- [ ] A `data.json` file in `wwwroot/data/` (or project root) contains all dashboard configuration.
- [ ] The JSON schema includes: `title`, `subtitle`, `backlogUrl`, `reportDate`, `workstreams[]`, `months[]`, and `heatmap` with `rows[]` and `cells[]`.
- [ ] The application loads and deserializes `data.json` at startup using `System.Text.Json`.
- [ ] If `data.json` is missing or contains invalid JSON, the application logs a clear error message and displays a user-friendly error state.
- [ ] Changing `data.json` and restarting the application reflects the updated data.

### US-6: Screenshot the Dashboard

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrolling, **so that** I can take a full-page screenshot and paste it directly into a PowerPoint slide without cropping or resizing.

**Acceptance Criteria:**
- [ ] The `<body>` element has fixed dimensions: `width: 1920px; height: 1080px; overflow: hidden`.
- [ ] All content fits within the 1920×1080 viewport with no scrollbars.
- [ ] The page renders identically in Chrome, Edge, and Firefox at 100% zoom on a 1080p display.
- [ ] Using Chrome DevTools device emulation at 1920×1080 produces a clean full-page capture.

### US-7: Run the Dashboard Locally

**As a** developer, **I want** to start the dashboard with a single `dotnet run` command, **so that** I can view it in my browser at `localhost` without any infrastructure setup.

**Acceptance Criteria:**
- [ ] The application starts with `dotnet run` and serves on `https://localhost:5001` (or configured port).
- [ ] No database connection, external API, or authentication is required.
- [ ] The application has zero NuGet dependencies beyond the default Blazor Server template.
- [ ] `dotnet watch` enables hot reload for CSS and Razor component changes during development.

---

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the `ReportingDashboard` repository. Engineers MUST consult this file and the rendered screenshot (`docs/design-screenshots/OriginalDesignConcept.png`) as the authoritative visual target.

![OriginalDesignConcept design reference](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/main/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Page Layout

- **Dimensions:** Fixed 1920×1080px, no scrolling, `overflow: hidden`.
- **Background:** `#FFFFFF`.
- **Font Family:** `'Segoe UI', Arial, sans-serif`.
- **Base Text Color:** `#111`.
- **Layout Mode:** Flexbox column (`display: flex; flex-direction: column`), three stacked sections filling the viewport height.
- **Horizontal Padding:** 44px on left and right for all major sections.

### Section 1: Header (`.hdr`) — ~46px height

| Property | Value |
|----------|-------|
| Padding | `12px 44px 10px` |
| Border | Bottom 1px solid `#E0E0E0` |
| Layout | Flexbox row, `space-between`, vertically centered |
| `flex-shrink` | `0` (fixed height) |

**Left Side:**
- **Title (H1):** 24px, font-weight 700, color `#111`. Contains project name + inline link.
- **ADO Link:** Inline `<a>` in title, color `#0078D4`, no underline.
- **Subtitle:** 12px, color `#888`, margin-top 2px. Format: "Org • Workstream • Month Year".

**Right Side (Legend):**
- Horizontal flex container, 22px gap between items, vertically centered.
- Each item: flex row, 6px gap, 12px font size.
- **PoC Milestone:** 12×12px inline-block, `background: #F4B400`, `transform: rotate(45deg)`.
- **Production Release:** 12×12px inline-block, `background: #34A853`, `transform: rotate(45deg)`.
- **Checkpoint:** 8×8px circle, `background: #999`, `border-radius: 50%`.
- **Now Indicator:** 2×14px vertical bar, `background: #EA4335`.

### Section 2: Timeline Area (`.tl-area`) — 196px height

| Property | Value |
|----------|-------|
| Height | `196px` (fixed, `flex-shrink: 0`) |
| Background | `#FAFAFA` |
| Border | Bottom 2px solid `#E8E8E8` |
| Layout | Flexbox row, `align-items: stretch` |
| Padding | `6px 44px 0` |

**Left Column — Workstream Labels (230px wide):**
- Fixed 230px width, `flex-shrink: 0`.
- Vertical flex column, `justify-content: space-around`.
- Padding: `16px 12px 16px 0`.
- Right border: 1px solid `#E0E0E0`.
- Each workstream label:
  - ID line (e.g., "M1"): 12px, font-weight 600, workstream-specific color.
  - Description line: 12px, font-weight 400, color `#444`.
  - Workstream colors from reference: M1 = `#0078D4`, M2 = `#00897B`, M3 = `#546E7A`.

**Right Column — SVG Canvas (`.tl-svg-box`, flex: 1):**
- `padding-left: 12px; padding-top: 6px`.
- SVG element: `width="1560" height="185"`, `overflow: visible; display: block`.

**SVG Elements:**
- **Month Gridlines:** Vertical lines at 260px intervals (0, 260, 520, 780, 1040, 1300), stroke `#bbb`, opacity 0.4, stroke-width 1.
- **Month Labels:** 11px, font-weight 600, fill `#666`, positioned 5px right of gridline, y=14.
- **NOW Line:** Dashed vertical line, stroke `#EA4335`, stroke-width 2, `stroke-dasharray: 5,3`. Position calculated from current date. "NOW" text: 10px, bold, fill `#EA4335`.
- **Workstream Lines:** Horizontal lines spanning full width (0 to 1560), stroke-width 3, color matches workstream. Y positions: ~42px (M1), ~98px (M2), ~154px (M3), evenly distributed.
- **Start Circles:** 7px radius, white fill, workstream-colored stroke (2.5px width). Date label nearby.
- **PoC Diamonds:** `<polygon>` forming 22px diamond shape, fill `#F4B400`, with drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`).
- **Production Diamonds:** Same shape, fill `#34A853`, same shadow filter.
- **Checkpoint Circles:** Small (4-5px radius), either outlined (stroke `#888`) for major or filled (`#999`) for minor.
- **Milestone Labels:** 10px, fill `#666`, `text-anchor: middle`, positioned above or below the marker to avoid overlap.

### Section 3: Heatmap (`.hm-wrap`) — Fills remaining space

| Property | Value |
|----------|-------|
| Layout | Flexbox column, `flex: 1; min-height: 0` |
| Padding | `10px 44px 10px` |

**Title Bar (`.hm-title`):**
- 14px, font-weight 700, color `#888`, `letter-spacing: 0.5px`, `text-transform: uppercase`.
- Margin-bottom: 8px.
- `flex-shrink: 0`.

**Grid (`.hm-grid`):**
- CSS Grid: `grid-template-columns: 160px repeat(4, 1fr); grid-template-rows: 36px repeat(4, 1fr)`.
- Border: 1px solid `#E0E0E0`.
- `flex: 1; min-height: 0` (fills remaining space).

**Column Headers:**
- **Corner Cell (`.hm-corner`):** Background `#F5F5F5`, 11px bold uppercase `#999`, centered, border-right 1px `#E0E0E0`, border-bottom 2px `#CCC`.
- **Month Headers (`.hm-col-hdr`):** 16px bold, background `#F5F5F5`, centered, border-right 1px `#E0E0E0`, border-bottom 2px `#CCC`.
- **Current Month Header (`.apr-hdr`):** Background `#FFF0D0`, color `#C07700`.

**Row Headers (`.hm-row-hdr`):**
- 11px bold uppercase, `letter-spacing: 0.7px`, padding `0 12px`, border-right 2px `#CCC`, border-bottom 1px `#E0E0E0`.
- Per-row styling:

| Row | CSS Class | Text Color | Background |
|-----|-----------|------------|------------|
| Shipped | `.ship-hdr` | `#1B7A28` | `#E8F5E9` |
| In Progress | `.prog-hdr` | `#1565C0` | `#E3F2FD` |
| Carryover | `.carry-hdr` | `#B45309` | `#FFF8E1` |
| Blockers | `.block-hdr` | `#991B1B` | `#FEF2F2` |

**Data Cells (`.hm-cell`):**
- Padding: `8px 12px`, border-right 1px `#E0E0E0`, border-bottom 1px `#E0E0E0`, `overflow: hidden`.

| Row | Default Background | Current Month Background | Bullet Color |
|-----|--------------------|--------------------------|--------------|
| Shipped | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

**Item Text (`.it`):**
- 12px, color `#333`, `padding: 2px 0 2px 12px`, `line-height: 1.35`, position relative.
- `::before` pseudo-element: 6×6px circle, `border-radius: 50%`, positioned at `left: 0; top: 7px`, color per row (see table above).

### Complete Color Palette Reference

| Token | Hex | Usage |
|-------|-----|-------|
| Page background | `#FFFFFF` | Body |
| Primary text | `#111` | Headings, body |
| Secondary text | `#333` | Heatmap items |
| Muted text | `#888` | Subtitles, section titles |
| Disabled text | `#999` | Corner cell, legend |
| Label text | `#666` | SVG labels, month headers |
| Workstream label text | `#444` | Workstream descriptions |
| Link blue | `#0078D4` | ADO link, in-progress bullets, M1 color |
| Teal | `#00897B` | M2 workstream color |
| Blue-gray | `#546E7A` | M3 workstream color |
| Green (shipped) | `#34A853` | Production diamonds, shipped bullets |
| Dark green text | `#1B7A28` | Shipped row header |
| Green bg light | `#E8F5E9` | Shipped row header bg |
| Green bg cell | `#F0FBF0` | Shipped cell |
| Green bg current | `#D8F2DA` | Shipped current month |
| Blue text | `#1565C0` | In-progress row header |
| Blue bg light | `#E3F2FD` | In-progress row header bg |
| Blue bg cell | `#EEF4FE` | In-progress cell |
| Blue bg current | `#DAE8FB` | In-progress current month |
| Amber (PoC) | `#F4B400` | PoC diamonds, carryover bullets |
| Amber text | `#B45309` | Carryover row header |
| Amber highlight text | `#C07700` | Current month column header |
| Yellow bg light | `#FFF8E1` | Carryover row header bg |
| Yellow bg cell | `#FFFDE7` | Carryover cell |
| Yellow bg current | `#FFF0B0` | Carryover current month |
| Yellow bg header | `#FFF0D0` | Current month column header bg |
| Red (NOW/blockers) | `#EA4335` | NOW line, blocker bullets |
| Dark red text | `#991B1B` | Blocker row header |
| Red bg light | `#FEF2F2` | Blocker row header bg |
| Red bg cell | `#FFF5F5` | Blocker cell |
| Red bg current | `#FFE4E4` | Blocker current month |
| Timeline bg | `#FAFAFA` | Timeline area |
| Grid header bg | `#F5F5F5` | Column/corner headers |
| Border light | `#E0E0E0` | General borders |
| Border medium | `#E8E8E8` | Timeline bottom border |
| Border heavy | `#CCC` | Row/column header separators |
| Gridline | `#BBB` | SVG month gridlines (40% opacity) |

---

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `https://localhost:5001`. The application loads `data.json`, deserializes it, and renders the full dashboard in a single viewport (1920×1080). The header, timeline, and heatmap all appear simultaneously with no loading spinner or progressive render needed (data is local and instantaneous).

**Scenario 2: User Views the Project Header**
User sees the project title (e.g., "Project Phoenix Release Roadmap") with a clickable "→ ADO Backlog" link at the top-left, and the subtitle showing organizational context and report month. The legend at the top-right explains all timeline marker types.

**Scenario 3: User Reads the Milestone Timeline**
User scans the timeline section and sees horizontal colored swim lanes for each workstream (e.g., M1, M2, M3). Diamond markers at specific dates indicate PoC milestones (amber) and Production releases (green). Small circles mark checkpoints. A dashed red vertical "NOW" line shows the current date position, allowing the user to instantly see which milestones are past, current, or upcoming.

**Scenario 4: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser navigates to the configured Azure DevOps backlog URL (from `data.json`'s `backlogUrl` field) in the current tab.

**Scenario 5: User Examines the Heatmap for Current Month Status**
User looks at the heatmap grid and immediately identifies the current month column by its highlighted header (amber background `#FFF0D0` with "▸ Now" indicator). The current month's data cells also have a slightly darker background across all four status rows, drawing the eye to the most relevant column.

**Scenario 6: User Scans Shipped Items Across Months**
User reads the green "Shipped" row left-to-right and sees bulleted lists of completed deliverables per month. Each item has a green dot prefix. The user can quickly count shipped items and see delivery momentum.

**Scenario 7: User Identifies Blockers**
User scans the red "Blockers" row and sees items with red dot prefixes in the affected months. The red color coding draws immediate attention to items requiring executive intervention.

**Scenario 8: User Identifies Carryover Patterns**
User scans the amber "Carryover" row and notices items appearing in consecutive months, indicating work that has slipped from its original schedule. This pattern signals execution risk to the executive viewer.

**Scenario 9: User Takes a Screenshot for PowerPoint**
User presses Win+Shift+S (Windows Snip) or uses Chrome DevTools "Capture full size screenshot" while the browser is set to 1920×1080. The entire dashboard fits in a single capture with no cropping needed. The user pastes the screenshot directly into a PowerPoint slide.

**Scenario 10: Empty Future Month Cells**
User views months that haven't occurred yet (e.g., May, June when the report date is April). These cells display a gray dash "—" character, indicating no data is expected yet rather than suggesting missing data.

**Scenario 11: Data Loading Error**
If `data.json` is missing, malformed, or contains invalid data, the page displays a centered error message: "Unable to load dashboard data. Please check data.json." styled in `#EA4335` on a white background. No partial or broken rendering is shown.

**Scenario 12: Hover Over Heatmap Item (Enhancement)**
User hovers over a bulleted item in the heatmap. A native browser tooltip (via `title` attribute) shows the full item text, useful when text is truncated due to cell overflow. No JavaScript tooltip library is required.

**Scenario 13: Multiple Workstreams Render Dynamically**
User configures `data.json` with 2, 3, or 4 workstreams. The timeline SVG automatically adjusts workstream Y positions to distribute evenly across the 185px height. Workstream labels in the left panel also redistribute via `justify-content: space-around`.

**Scenario 14: Responsive Behavior (Explicitly None)**
User resizes the browser window smaller than 1920×1080. The page does NOT reflow or respond. Content is clipped by `overflow: hidden`. This is by design — the dashboard is a fixed-dimension artifact for screenshots, not a responsive web application.

---

## Scope

### In Scope

- Single-page Blazor Server application rendering the executive dashboard
- Fixed 1920×1080px layout matching `OriginalDesignConcept.html` pixel-for-pixel
- Header component with data-driven title, subtitle, backlog link, and milestone legend
- SVG-based timeline component with data-driven workstream swim lanes, milestone markers (PoC diamonds, production diamonds, checkpoint circles), and "NOW" line
- CSS Grid heatmap component with four status rows (Shipped, In Progress, Carryover, Blockers) and configurable month columns
- Current month highlighting in both the heatmap column header and data cells
- `data.json` configuration file as the sole data source
- C# record-based data model with `System.Text.Json` deserialization
- `DashboardDataService` singleton for loading and caching data
- `dashboard.css` ported from the reference HTML design
- Fictional "Project Phoenix" sample data in `data.json` for demonstration
- Error state rendering when `data.json` is missing or invalid
- `dotnet run` / `dotnet watch` local development workflow
- Binding to `localhost` only (no network exposure)

### Out of Scope

- **Authentication / Authorization** — No login, no roles, no identity provider integration
- **Database** — No SQL Server, SQLite, Entity Framework, or any persistent storage beyond the JSON file
- **Responsive design** — No mobile, tablet, or variable-width layouts
- **Real-time updates** — No SignalR push, no WebSocket live data (despite Blazor Server's capability)
- **Charting libraries** — No Chart.js, Plotly, Radzen Charts, or any third-party visualization package
- **CSS frameworks** — No Bootstrap, Tailwind, MudBlazor, or any UI component library
- **Multi-page navigation** — No routing beyond the single dashboard page; no sidebar or nav menu
- **Azure DevOps integration** — No automated import from ADO work item queries (future enhancement)
- **Automated screenshot capture** — No Playwright or headless browser scripting (future enhancement)
- **Multi-project support** — No project switching or multiple JSON file selection (future enhancement)
- **PDF export / print stylesheet** — No `@media print` rules (future enhancement)
- **JSON schema validation file** — No `data.schema.json` (future enhancement; can be added without code changes)
- **File watcher auto-reload** — No `FileSystemWatcher` on `data.json` (future enhancement)
- **Containerization** — No Dockerfile or Docker Compose
- **CI/CD pipeline** — No GitHub Actions, Azure DevOps Pipeline, or automated builds
- **Unit tests / component tests** — No xUnit, bUnit, or test project (future enhancement)
- **Internationalization / Localization** — English only
- **Accessibility (WCAG)** — Not targeted for this release (screenshot-only consumption model)

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** | < 1 second from `localhost` (first contentful paint) |
| **JSON deserialization** | < 50ms for a `data.json` up to 50KB |
| **Memory usage** | < 100MB runtime footprint |
| **Startup time** | < 3 seconds from `dotnet run` to serving requests |

### Security

| Requirement | Implementation |
|-------------|----------------|
| **Network binding** | `localhost` only — configured in `launchSettings.json` to prevent external access |
| **No authentication** | Explicitly none; no middleware, no cookies, no tokens |
| **No sensitive data** | `data.json` contains project status only, no PII or credentials |
| **HTTPS** | Use default Kestrel HTTPS on localhost with dev certificate |

### Reliability

| Requirement | Target |
|-------------|--------|
| **Availability** | N/A — runs on-demand locally; no uptime SLA |
| **Data integrity** | Read-only from `data.json`; no write operations that could corrupt data |
| **Error handling** | Graceful error display if `data.json` is missing or malformed |

### Scalability

- **Not applicable.** This is a single-user, local-only tool. No concurrent users, no horizontal scaling, no load balancing.

### Compatibility

| Browser | Minimum Version |
|---------|----------------|
| Chrome | 120+ |
| Edge | 120+ |
| Firefox | 120+ |

- Windows 10/11 with .NET 8.0 SDK installed.
- Segoe UI font available (ships with Windows).

### Screenshot Fidelity

| Requirement | Target |
|-------------|--------|
| **Resolution** | Pixel-perfect at 1920×1080 |
| **Consistency** | Identical rendering across Chrome, Edge, Firefox |
| **No artifacts** | No scrollbars, no overflow, no content clipping within the viewport |

---

## Success Metrics

1. **Visual Match:** The rendered dashboard is visually indistinguishable from the `OriginalDesignConcept.html` reference when compared side-by-side at 1920×1080 — verified by screenshot overlay comparison.
2. **Data-Driven Rendering:** Changing any field in `data.json` and restarting the application correctly reflects the change in the rendered output with no code modifications.
3. **Zero-Config Startup:** A new developer can clone the repo, run `dotnet run`, and see the dashboard at `localhost` within 60 seconds with no additional setup steps.
4. **Screenshot Workflow:** A project lead can go from editing `data.json` to having a PowerPoint-ready screenshot in under 2 minutes.
5. **File Count:** The entire application consists of ≤ 10 source files (excluding `bin/`, `obj/`, and `.sln`).
6. **Dependency Count:** Zero NuGet packages beyond the default Blazor Server template (`Microsoft.AspNetCore.App` framework reference only).
7. **Infrastructure Cost:** $0 — runs entirely on the developer's local machine.

---

## Constraints & Assumptions

### Technical Constraints

- **Runtime:** .NET 8.0 LTS SDK must be installed on the developer's machine.
- **Fixed Dimensions:** The page is hardcoded to 1920×1080px. It will not render correctly on monitors with lower resolution unless browser zoom is adjusted or DevTools device emulation is used.
- **Font Dependency:** Segoe UI is a Windows system font. On macOS/Linux, the fallback to Arial may produce slight visual differences. This is acceptable since the primary use case is Windows.
- **Blazor Server Overhead:** The SignalR connection adds ~50KB of JS and a persistent WebSocket. This is negligible for local use but means the page is not a pure static HTML file.
- **No Offline Mode:** The page requires the Kestrel server to be running. It cannot be opened as a standalone HTML file.

### Timeline Assumptions

- **Phase 1 (MVP):** 1–2 developer days to scaffold, build components, port CSS, and create sample data.
- **Phase 2 (Polish):** 1 additional day for JSON schema, auto-NOW-line, and current-month highlighting refinements.
- **Total estimated effort:** 2–3 developer days.

### Dependency Assumptions

- The `OriginalDesignConcept.html` file in the `ReportingDashboard` repository is the canonical and final visual design. Any deviations require PM approval.
- The `data.json` file will be hand-edited by the project lead (not generated by an automated tool) for the MVP phase.
- The dashboard will be used by a single person at a time on their local machine. Multi-user or shared-server scenarios are not planned.
- The number of heatmap months will be 4 (matching the reference design). This is configurable in `data.json` by adding/removing month entries, but the grid layout is optimized for 4 columns.
- The number of workstreams in the timeline will be 3 (matching the reference). Adding more workstreams is supported but may require manual verification of SVG spacing.
- The "NOW" line position will be auto-calculated from the system date at page load time. A `reportDate` field in `data.json` can override this for generating historical snapshots.

### Design Assumptions

- The design will closely follow `OriginalDesignConcept.html` but may include minor improvements for clarity and polish (e.g., better text alignment, refined spacing) as long as the overall layout structure, color scheme, and visual hierarchy remain consistent.
- Emoji or Unicode symbols (✓, ◐, ↻, ✕) used in row headers are acceptable alternatives to the original design's text-only labels, provided they render cleanly in screenshot output.