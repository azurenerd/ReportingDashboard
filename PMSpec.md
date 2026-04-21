# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, shipping status, in-progress work, carryover items, and blockers. The dashboard reads all data from a local `data.json` configuration file and renders a pixel-perfect view optimized for 1920×1080 screenshots destined for PowerPoint executive decks. Built as a minimal Blazor Server (.NET 8) application with zero authentication, no database, and no external dependencies, this tool replaces the manual process of editing static HTML files each reporting period.

## Business Goals

1. **Eliminate manual HTML editing** — Provide a data-driven dashboard where updating project status requires editing a single JSON file instead of modifying HTML markup, reducing update time from 30+ minutes to under 5 minutes.
2. **Deliver screenshot-ready executive views** — Render a polished, presentation-quality single-page dashboard at exactly 1920×1080 resolution that can be directly captured and pasted into PowerPoint decks with no post-processing.
3. **Provide at-a-glance project health visibility** — Surface the four critical dimensions executives care about (Shipped, In Progress, Carryover, Blockers) in a color-coded heatmap grid alongside a milestone timeline, enabling a 30-second comprehension of project status.
4. **Maintain zero operational overhead** — Require no cloud hosting, no database, no authentication, and no ongoing maintenance — run locally on demand, capture a screenshot, and close.
5. **Enable rapid iteration on reporting content** — Support hot-reload during development so the user can edit `data.json`, save, and immediately see the updated dashboard without restarting the application.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** program manager, **I want** to see the project title, organizational context, reporting period, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are looking at.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` element.

- [ ] Dashboard displays the project title in bold 24px font on the left side of the header
- [ ] A clickable ADO backlog link appears inline with the title (blue `#0078D4`, no underline)
- [ ] A subtitle line displays the organization, workstream, and current month in 12px gray (`#888`) text
- [ ] A legend appears on the right side of the header showing four indicators: PoC Milestone (amber diamond), Production Release (green diamond), Checkpoint (gray circle), and Now line (red vertical bar)
- [ ] All header data is driven from the `project` section of `data.json`

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major project milestones across multiple workstreams, **so that** I can understand the project's trajectory and key delivery dates at a glance.

**Visual Reference:** Timeline area of `OriginalDesignConcept.html` — `.tl-area` element with SVG rendering.

- [ ] The timeline section appears below the header with a light gray (`#FAFAFA`) background and a fixed height of 196px
- [ ] A left-side label column (230px wide) lists each workstream with its ID (e.g., "M1") and name, color-coded per track
- [ ] Each workstream renders as a horizontal line spanning the full timeline width, colored per track definition in `data.json`
- [ ] PoC milestones render as amber (`#F4B400`) diamond shapes with drop shadow
- [ ] Production milestones render as green (`#34A853`) diamond shapes with drop shadow
- [ ] Checkpoints render as small gray circles (filled `#999` or outlined with track color)
- [ ] Start points render as outlined circles in the track color
- [ ] Each milestone displays a date label (e.g., "Mar 26 PoC") positioned above or below the shape
- [ ] A vertical dashed red (`#EA4335`) "NOW" line is positioned at the current date (auto-calculated or overridden via JSON)
- [ ] Month labels (Jan–Jun) appear at the top of the SVG with vertical gridlines at each month boundary
- [ ] The number of tracks, milestones per track, and date range are all driven by `data.json`

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked for each month, **so that** I can assess execution health and identify trends.

**Visual Reference:** Heatmap grid of `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` elements.

- [ ] A section title "Monthly Execution Heatmap" appears in uppercase gray (`#888`) 14px bold text
- [ ] The grid uses CSS Grid with columns: 160px label + N equal-width month columns (default 4)
- [ ] Column headers display month names in 16px bold text; the current month column has an amber highlight (`#FFF0D0`, text `#C07700`) with a "◀ Now" indicator
- [ ] Four status rows are displayed: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each row header uses uppercase 11px bold text with the category color and a matching background tint
- [ ] Data cells display individual items as 12px text with a 6px colored bullet indicator (using CSS `::before` pseudo-element)
- [ ] Current-month cells have a deeper background tint than other months (e.g., shipped: `#D8F2DA` vs. `#F0FBF0`)
- [ ] Empty future cells display a gray dash "-"
- [ ] All heatmap data (months, categories, items per cell) is driven from the `heatmap` section of `data.json`

### US-4: Configure Dashboard via JSON

**As a** program manager, **I want** to update all dashboard content by editing a single `data.json` file, **so that** I can quickly refresh the dashboard for each reporting cycle without touching code.

- [ ] A `data.json` file in the `Data/` directory contains all dashboard content: project info, timeline tracks, milestones, and heatmap data
- [ ] The application reads and deserializes `data.json` at startup using `System.Text.Json`
- [ ] Changes to `data.json` are reflected after restarting the application (or automatically via file watcher in Phase 2)
- [ ] If `data.json` is missing, the application displays a friendly error message instead of crashing
- [ ] If `data.json` contains invalid JSON or missing required fields, a descriptive error message is shown

### US-5: Capture Dashboard Screenshot

**As a** program manager, **I want** the dashboard to render cleanly at exactly 1920×1080 pixels with no scrollbars, **so that** I can take a full-page screenshot and paste it directly into my PowerPoint deck.

- [ ] The page body is set to exactly `width: 1920px; height: 1080px; overflow: hidden`
- [ ] All content fits within the viewport without scrolling
- [ ] The page uses Segoe UI system font (no web font loading delays)
- [ ] The page renders identically in Edge Chromium at 100% zoom
- [ ] No Blazor framework UI chrome (nav menus, sidebars, loading spinners) is visible

### US-6: Run Dashboard Locally

**As a** developer, **I want** to start the dashboard with a single `dotnet run` command and access it at `http://localhost:5000`, **so that** there is zero setup friction.

- [ ] Running `dotnet run` starts Kestrel on `http://localhost:5000` (HTTP only)
- [ ] No authentication prompt or login page appears
- [ ] The dashboard page loads as the root URL (`/`)
- [ ] The application requires zero NuGet packages beyond the default Blazor Server template
- [ ] The port is configurable via `appsettings.json`

### US-7: Load Sample Data for Demo

**As a** new user, **I want** the project to ship with a realistic sample `data.json` for a fictional project, **so that** I can see the dashboard working immediately and understand the data format by example.

- [ ] A sample `data.json` is included with fictional but realistic project data (e.g., "Project Phoenix — Cloud Migration Platform")
- [ ] The sample includes 3 timeline tracks with varied milestone types (PoC, production, checkpoints)
- [ ] The sample includes 4 months of heatmap data with items in all four status categories
- [ ] The sample renders a visually complete and balanced dashboard matching the reference design

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the `ReportingDashboard` repository and `C:/Pics/ReportingDashboardDesign.png`. Engineers MUST consult these files and match the rendered output pixel-for-pixel. The rendered screenshot is available at:

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/69d83848dd419051b7080124d605bf1da5c3e51e/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Page Layout

- **Viewport:** Fixed `1920px × 1080px`, `overflow: hidden`, white (`#FFFFFF`) background
- **Font:** `'Segoe UI', Arial, sans-serif`, base color `#111`
- **Layout Mode:** Vertical flex column (`display: flex; flex-direction: column`) — the page is divided into three stacked horizontal bands:
  1. **Header band** — flex-shrink: 0, auto-height (~50px)
  2. **Timeline band** — flex-shrink: 0, fixed height 196px
  3. **Heatmap band** — flex: 1 (fills remaining vertical space, ~834px)
- **Horizontal padding:** 44px on left and right for all three bands

### Section 1: Header (`.hdr`)

- **Layout:** `display: flex; align-items: center; justify-content: space-between`
- **Padding:** `12px 44px 10px`
- **Border:** Bottom `1px solid #E0E0E0`
- **Left group:**
  - **Title:** `<h1>` at `font-size: 24px; font-weight: 700` containing project name and an inline `<a>` link (color `#0078D4`, no underline) to the ADO backlog
  - **Subtitle:** `<div class="sub">` at `font-size: 12px; color: #888; margin-top: 2px` showing org · workstream · month
- **Right group (Legend):** Horizontal flex row with `gap: 22px`, each item at `font-size: 12px`:
  - PoC Milestone: 12×12px amber (`#F4B400`) square rotated 45° (diamond)
  - Production Release: 12×12px green (`#34A853`) square rotated 45° (diamond)
  - Checkpoint: 8×8px gray (`#999`) circle
  - Now indicator: 2×14px red (`#EA4335`) vertical bar

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** `display: flex; align-items: stretch`
- **Padding:** `6px 44px 0`
- **Height:** Fixed 196px
- **Background:** `#FAFAFA`
- **Border:** Bottom `2px solid #E8E8E8`
- **Left label column:** 230px wide, `flex-shrink: 0`, with `border-right: 1px solid #E0E0E0`
  - Contains track labels vertically distributed with `justify-content: space-around`
  - Each label: Track ID (e.g., "M1") in `font-size: 12px; font-weight: 600` with track color, followed by `<br/>` and track name in `font-weight: 400; color: #444`
  - Track colors from reference: M1 = `#0078D4` (blue), M2 = `#00897B` (teal), M3 = `#546E7A` (blue-gray)
- **Right SVG area** (`.tl-svg-box`): `flex: 1; padding-left: 12px; padding-top: 6px`
  - SVG dimensions: `width="1560" height="185"`, `overflow: visible`
  - **Month gridlines:** Vertical lines at equal intervals (260px apart for 6-month span), `stroke: #bbb; stroke-opacity: 0.4; stroke-width: 1`
  - **Month labels:** `font-size: 11; font-weight: 600; fill: #666` positioned 5px right of each gridline at y=14
  - **NOW line:** Dashed vertical line at calculated x-position, `stroke: #EA4335; stroke-width: 2; stroke-dasharray: 5,3` with "NOW" label in `font-size: 10; font-weight: 700; fill: #EA4335`
  - **Track lines:** Horizontal `<line>` elements at y=42, y=98, y=154 (evenly spaced for 3 tracks), `stroke-width: 3`, color per track
  - **Start circles:** `r="7"`, `fill: white`, `stroke: [track-color]; stroke-width: 2.5`
  - **Checkpoint circles:** Small (`r="4-5"`), either filled `#999` or outlined with `stroke: #888`
  - **PoC diamonds:** `<polygon>` with 4 points forming an 11px diamond, `fill: #F4B400`, with drop shadow filter (`feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"`)
  - **Production diamonds:** Same shape, `fill: #34A853`, with drop shadow
  - **Date labels:** `font-size: 10; fill: #666; text-anchor: middle`, positioned above (y - 16px) or below (y + 24px) the milestone shape

### Section 3: Heatmap Grid (`.hm-wrap` + `.hm-grid`)

- **Wrapper:** `flex: 1; min-height: 0; display: flex; flex-direction: column; padding: 10px 44px 10px`
- **Title:** `font-size: 14px; font-weight: 700; color: #888; letter-spacing: 0.5px; text-transform: uppercase; margin-bottom: 8px` — displays "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"
- **Grid:** `display: grid; border: 1px solid #E0E0E0`
  - **Columns:** `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months (default 4)
  - **Rows:** `grid-template-rows: 36px repeat(4, 1fr)` — header row + 4 category rows

#### Grid Header Row

| Cell | Style |
|------|-------|
| **Corner cell** (`.hm-corner`) | `background: #F5F5F5; font-size: 11px; font-weight: 700; color: #999; text-transform: uppercase; border-right: 1px solid #E0E0E0; border-bottom: 2px solid #CCC` — displays "STATUS" |
| **Month header** (`.hm-col-hdr`) | `font-size: 16px; font-weight: 700; background: #F5F5F5; border-right: 1px solid #E0E0E0; border-bottom: 2px solid #CCC` — displays month name |
| **Current month header** (`.hm-col-hdr.apr-hdr`) | Override: `background: #FFF0D0; color: #C07700` — displays month name + "◀ Now" |

#### Grid Category Rows

Each row has a consistent color scheme applied to the row header and all data cells:

| Category | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Bullet Color |
|----------|--------------|----------------|---------|----------------------|-------------|
| **✅ Shipped** | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| **🔵 In Progress** | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| **🟡 Carryover** | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| **🔴 Blockers** | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

- **Row header** (`.hm-row-hdr`): `font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.7px; border-right: 2px solid #CCC; border-bottom: 1px solid #E0E0E0; padding: 0 12px`
- **Data cells** (`.hm-cell`): `padding: 8px 12px; border-right: 1px solid #E0E0E0; border-bottom: 1px solid #E0E0E0; overflow: hidden`
- **Items** (`.hm-cell .it`): `font-size: 12px; color: #333; padding: 2px 0 2px 12px; line-height: 1.35; position: relative`
- **Bullet indicators** (`.it::before`): `position: absolute; left: 0; top: 7px; width: 6px; height: 6px; border-radius: 50%` with background color matching the category

### CSS Custom Properties (Recommended Extraction)

```css
:root {
    --shipped-primary: #34A853;    --shipped-bg: #F0FBF0;
    --shipped-bg-current: #D8F2DA; --shipped-header-bg: #E8F5E9;  --shipped-text: #1B7A28;
    --progress-primary: #0078D4;   --progress-bg: #EEF4FE;
    --progress-bg-current: #DAE8FB;--progress-header-bg: #E3F2FD; --progress-text: #1565C0;
    --carryover-primary: #F4B400;  --carryover-bg: #FFFDE7;
    --carryover-bg-current: #FFF0B0;--carryover-header-bg: #FFF8E1;--carryover-text: #B45309;
    --blocker-primary: #EA4335;    --blocker-bg: #FFF5F5;
    --blocker-bg-current: #FFE4E4; --blocker-header-bg: #FEF2F2;  --blocker-text: #991B1B;
    --page-width: 1920px;  --page-height: 1080px;
    --side-padding: 44px;  --grid-label-width: 160px;
}
```

### Component Hierarchy

```
App.razor
└── MainLayout.razor (no nav, no sidebar — full-page single column)
    └── Dashboard.razor (the single route "/")
        ├── Header.razor        → .hdr section
        ├── Timeline.razor      → .tl-area section (includes SVG generation)
        └── HeatmapGrid.razor   → .hm-wrap section
            └── HeatmapCell.razor (×N) → individual .hm-cell elements
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Dashboard Renders with Project Data**
User navigates to `http://localhost:5000`. The browser renders the full dashboard within 1 second. The header shows the project title, subtitle, and legend. The timeline shows all workstream tracks with positioned milestones. The heatmap grid shows all months and status categories populated from `data.json`. The "NOW" line is positioned at today's date. No loading spinner, skeleton, or progressive rendering is visible — the page appears fully formed.

**Scenario 2: User Views the Header and Identifies the Project**
User looks at the top of the page and sees the project name in bold (e.g., "Project Phoenix Release Roadmap"), a clickable "→ ADO Backlog" link in blue, the organizational context line, and the legend explaining the four visual indicators (PoC diamond, Production diamond, Checkpoint circle, Now line).

**Scenario 3: User Reads the Milestone Timeline**
User looks at the timeline band and sees horizontal colored lines for each workstream (e.g., M1 in blue, M2 in teal, M3 in blue-gray). Diamond shapes mark PoC and Production milestones with date labels. Small circles mark checkpoints. A red dashed vertical "NOW" line shows where today falls relative to all milestones, giving an instant sense of progress.

**Scenario 4: User Scans the Heatmap for Current Month Status**
User's eye is drawn to the current-month column, which has a distinctly highlighted header (amber background, "◀ Now" indicator). The current-month cells across all four rows have a deeper background tint, making them visually prominent. The user quickly reads bullet items to understand what shipped this month, what's in progress, what carried over, and what's blocked.

**Scenario 5: User Compares Months for Trend Analysis**
User scans left-to-right across the heatmap rows to observe trends. For example, they notice the "Shipped" row has increasing items from March to April, the "Carryover" row is decreasing, and the "Blockers" row has a new item in April. The consistent color coding makes cross-month comparison intuitive.

**Scenario 6: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. A new browser tab opens navigating to the Azure DevOps backlog URL specified in `data.json`. The dashboard remains unchanged in the original tab.

**Scenario 7: User Captures a Screenshot for PowerPoint**
User opens the dashboard in Edge Chromium at 100% zoom with the window sized to 1920×1080. They use Windows Snipping Tool (or Win+Shift+S) to capture the full page. The resulting image is clean, with no scrollbars, no browser chrome captured within the viewport, and all text is crisp at the target resolution. The screenshot is pasted directly into a PowerPoint slide.

**Scenario 8: User Updates `data.json` and Refreshes**
User edits `data.json` to add a new shipped item for April, saves the file, and refreshes the browser (or the page auto-refreshes via FileSystemWatcher in Phase 2). The heatmap now shows the new item in the April/Shipped cell with the correct green bullet indicator.

**Scenario 9: Empty State — Future Month Cells**
User views a month column (e.g., May or June) that has no data yet. Instead of empty white cells, each cell displays a single gray dash "—" indicating intentional emptiness rather than a rendering error.

**Scenario 10: Error State — Missing `data.json`**
User starts the application without a `data.json` file in the `Data/` directory. Instead of a crash or white screen, the dashboard displays a centered, friendly error message: "Dashboard data not found. Please ensure data.json exists in the Data/ directory." styled with a gray border and informational icon.

**Scenario 11: Error State — Malformed JSON**
User accidentally introduces a syntax error in `data.json` (e.g., trailing comma, missing bracket). The dashboard displays a descriptive error: "Error reading data.json: [specific parse error message]. Please fix the JSON syntax and refresh." The error message includes enough detail for the user to locate and fix the problem.

**Scenario 12: Data-Driven Rendering — Variable Track Count**
User configures `data.json` with 2 timeline tracks instead of 3. The timeline section renders only 2 horizontal lines with appropriate vertical spacing. The left label column shows only 2 track labels. No empty third row or visual artifact appears.

**Scenario 13: Data-Driven Rendering — Variable Month Count**
User configures `data.json` with 6 months instead of 4 in the heatmap. The CSS Grid adjusts to `160px repeat(6, 1fr)`, producing 6 equal-width month columns. Header cells and data cells render for all 6 months. The current-month highlighting applies to the correct column.

## Scope

### In Scope

- Single-page Blazor Server (.NET 8) web application rendering a project status dashboard
- Data-driven rendering from a local `data.json` configuration file
- Header component with project title, subtitle, backlog link, and visual legend
- SVG-based milestone timeline with multiple workstreams, milestone types (PoC, Production, Checkpoint), and a "NOW" indicator
- CSS Grid-based heatmap with 4 status categories (Shipped, In Progress, Carryover, Blockers) × N month columns
- Color-coded row theming matching the reference design exactly (greens, blues, ambers, reds)
- Current-month visual highlighting in both the timeline ("NOW" line) and heatmap (amber header, deeper cell tints)
- Fixed 1920×1080 viewport optimized for screenshot capture
- Sample `data.json` with fictional but realistic project data
- Friendly error display for missing or malformed `data.json`
- CSS custom properties for the full color palette
- Localhost-only HTTP hosting via Kestrel on a configurable port
- Hot reload support via `dotnet watch` during development
- Auto-reload on `data.json` changes via `FileSystemWatcher` (Phase 2)

### Out of Scope

- **Authentication / authorization** — No login, no cookies, no tokens, no role-based access
- **Database** — No SQLite, LiteDB, SQL Server, or any persistence layer
- **REST API / Web API controllers** — No endpoints; this is not a service
- **Docker containerization** — Not needed for a local-only tool
- **CI/CD pipeline** — Manual `dotnet run` / `dotnet publish` only
- **Third-party CSS framework** — No Bootstrap, Tailwind, MudBlazor, or Radzen
- **JavaScript charting library** — No Chart.js, ApexCharts, or D3; SVG is hand-rendered
- **Responsive / mobile design** — Fixed 1920×1080 only; no breakpoints, no media queries (except optional print)
- **Multi-user / concurrent access** — Single user on localhost
- **Cloud hosting** — No Azure, AWS, or any remote deployment
- **Real-time data feeds** — Data comes from a static JSON file, not a live API
- **Unit test project** — Optional; not required for MVP
- **Multiple project support** — Design for one project; multi-project is a future enhancement
- **Automated screenshot tooling** — Puppeteer-Sharp integration is a "nice-to-have," not in scope for MVP
- **Animations / transitions** — Static render only for screenshot fidelity

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** | < 1 second from navigation to fully rendered content (localhost, no network latency) |
| **`data.json` deserialization** | < 50ms for a file up to 100KB |
| **SVG render time** | < 100ms for up to 10 timeline tracks with 20 milestones each |
| **Memory footprint** | < 100MB RSS for the running application |
| **Startup time** | < 3 seconds from `dotnet run` to serving HTTP requests |

### Security

| Requirement | Specification |
|------------|---------------|
| **Network exposure** | Bind to `localhost` only (`127.0.0.1`); not accessible from other machines |
| **Protocol** | HTTP only; no HTTPS required (no sensitive data, no public network) |
| **Data classification** | Project names and status descriptions only — no PII, no credentials, no secrets |
| **Input validation** | Validate `data.json` structure on load; reject and display errors for malformed input |
| **No secrets in code** | No API keys, connection strings, or credentials anywhere in the codebase |

### Reliability

| Requirement | Specification |
|------------|---------------|
| **Availability** | Run on demand; no uptime SLA (start when needed, stop when done) |
| **Crash recovery** | If the app crashes, `dotnet run` restarts it; no persistent state to corrupt |
| **Graceful degradation** | Missing/invalid `data.json` produces a helpful error page, not a white screen or stack trace |

### Compatibility

| Requirement | Specification |
|------------|---------------|
| **Primary browser** | Microsoft Edge Chromium (latest stable) at 100% zoom, 1920×1080 viewport |
| **Secondary browser** | Google Chrome (latest stable) — should render identically |
| **Operating system** | Windows 10/11 with .NET 8 SDK installed |
| **Font dependency** | Segoe UI (pre-installed on all Windows systems) |

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Screenshot fidelity** | Dashboard screenshot at 1920×1080 is visually indistinguishable from the reference design `OriginalDesignConcept.html` when rendered in the same browser | Side-by-side visual comparison |
| 2 | **Data update turnaround** | User can update `data.json` and capture a new screenshot in under 5 minutes | Timed user test |
| 3 | **Zero-config startup** | `dotnet run` produces a working dashboard with sample data with no additional setup steps | First-run test on a clean machine with .NET 8 SDK |
| 4 | **JSON schema clarity** | A new user can understand and modify `data.json` by reading the sample file, with no documentation | User comprehension test |
| 5 | **External dependencies** | Zero NuGet packages beyond the default Blazor Server template | `dotnet list package` output |
| 6 | **File count** | Complete solution in ≤ 15 files (including `data.json`, CSS, Razor components, models, service) | File count in project directory |
| 7 | **Error resilience** | Missing or malformed `data.json` produces a user-friendly error message, never a stack trace or white screen | Negative testing with missing/corrupt files |

## Constraints & Assumptions

### Technical Constraints

- **Technology stack is fixed:** Blazor Server on .NET 8 — this is a mandated stack choice, not negotiable
- **No external NuGet packages:** The core application must use only framework-included libraries (`System.Text.Json`, `Microsoft.AspNetCore.Components`, etc.)
- **Fixed viewport:** The page is designed for exactly 1920×1080 pixels — no responsive breakpoints, no mobile support
- **Single data source:** All content comes from one `data.json` file — no API calls, no database queries
- **Localhost only:** The application binds to `127.0.0.1` and is not intended for network deployment
- **System font only:** Segoe UI must be available (Windows-only assumption); no web fonts to avoid loading delays that could affect screenshot timing

### Timeline Assumptions

- **Phase 1 (MVP):** 1–2 days — Scaffold project, implement all components, port CSS, create sample data, verify screenshot output
- **Phase 2 (Polish):** 1 day — Error handling, FileSystemWatcher auto-reload, print stylesheet
- **Phase 3 (Optional):** As time permits — Puppeteer-Sharp screenshots, multi-project support, date auto-advance
- **Total estimated effort:** 3 days for a complete, polished deliverable

### Dependency Assumptions

- **.NET 8 SDK** (`8.0.400+`) is installed on the development machine
- **Windows 10/11** is the operating system (for Segoe UI font availability)
- **Edge Chromium** (latest stable) is available for screenshot capture
- **No network access required** at runtime — the application is fully offline-capable
- The reference design files (`OriginalDesignConcept.html` and `C:/Pics/ReportingDashboardDesign.png`) are available to engineers during development for visual comparison
- The `data.json` schema is stable after MVP — breaking schema changes would require coordinated updates to both the JSON file and C# model classes

### Design Assumptions

- The reference design's 4-month heatmap and 3-track timeline are representative but not fixed — the implementation must support a variable number of months (driven by `data.json`) and variable number of tracks
- The "NOW" line position defaults to `DateTime.Now` but can be overridden in `data.json` for retrospective reporting or future planning views
- Item text in heatmap cells is plain text only — no rich formatting, no links, no icons within cell items
- The ADO backlog URL in the header is the only interactive element; all other content is read-only and static