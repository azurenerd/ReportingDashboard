# PM Specification: Executive Reporting Dashboard

**Document Version:** 1.0
**Date:** April 22, 2026
**Author:** Program Management
**Status:** Draft
**Design Reference:** `OriginalDesignConcept.html` (ReportingDashboard repo) and `C:/Pics/ReportingDashboardDesign.png`

---

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes a project's milestones, delivery status, and monthly execution heatmap. The dashboard loads all display data from a local JSON configuration file (`dashboard-data.json`) and renders a pixel-perfect 1920×1080 view optimized for screenshot capture and insertion into PowerPoint decks for executive audiences. The application is built on Blazor Server (.NET 8) with zero third-party runtime dependencies, no authentication, and no database — simplicity is the primary design constraint.

---

## Business Goals

1. **Provide executive-ready project visibility** — Deliver a single-page view that communicates project health, milestone progress, shipped work, in-progress items, carryovers, and blockers at a glance.
2. **Enable screenshot-to-PowerPoint workflow** — Produce a clean, presentation-quality 1920×1080 rendering that can be captured via browser screenshot and pasted directly into executive slide decks without modification.
3. **Minimize operational overhead** — Require zero infrastructure (no cloud, no database, no auth). The dashboard runs locally on a developer's machine with `dotnet run` and reads from a single editable JSON file.
4. **Support rapid data updates** — Allow a project manager to update the JSON configuration file in any text editor and see changes reflected immediately (via file watching or app restart) without code changes.
5. **Maintain design fidelity** — Match the visual design established in `OriginalDesignConcept.html` exactly, improving upon it where possible for clarity and readability while preserving the clean, minimal aesthetic.
6. **Keep total solution under 15 files** — Resist feature creep and over-engineering; the simplicity of the tool IS the feature.

---

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project manager, **I want** to see the project title, subtitle (team/workstream/date), and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are viewing.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` element.

**Acceptance Criteria:**
- [ ] The header displays the project title in bold 24px font weight 700
- [ ] The subtitle appears below the title in 12px gray (#888) text
- [ ] An optional backlog URL renders as a clickable link styled in #0078D4
- [ ] All text is data-driven from `dashboard-data.json` → `project` object
- [ ] The header spans the full 1920px width with 44px horizontal padding

---

### US-2: View Milestone Legend

**As an** executive viewer, **I want** to see a legend in the header explaining the milestone icon types (PoC Milestone, Production Release, Checkpoint, Now indicator), **so that** I can interpret the timeline without external explanation.

**Visual Reference:** Header legend area (right side of `.hdr`) in `OriginalDesignConcept.html`.

**Acceptance Criteria:**
- [ ] Four legend items are displayed inline with 22px gap between them
- [ ] PoC Milestone: gold (#F4B400) diamond (12×12px rotated 45°)
- [ ] Production Release: green (#34A853) diamond (12×12px rotated 45°)
- [ ] Checkpoint: gray (#999) circle (8×8px)
- [ ] Now indicator: red (#EA4335) vertical bar (2×14px)
- [ ] Each legend item has a 12px label next to the icon
- [ ] Legend is right-aligned within the header bar

---

### US-3: View Milestone Timeline

**As an** executive viewer, **I want** to see a horizontal timeline showing multiple project tracks with milestone markers positioned by date, **so that** I can understand the project's key dates and progress at a glance.

**Visual Reference:** Timeline area (`.tl-area`) and inline SVG in `OriginalDesignConcept.html`.

**Acceptance Criteria:**
- [ ] The timeline section has a fixed height of 196px with #FAFAFA background
- [ ] A left sidebar (230px wide) lists each track with its ID (e.g., "M1"), name, and color
- [ ] The SVG area renders horizontal colored lines for each track
- [ ] Vertical month gridlines appear with month labels (Jan, Feb, Mar, etc.)
- [ ] A dashed red (#EA4335) vertical "NOW" line appears at the current date position
- [ ] Checkpoint milestones render as open circles with colored strokes
- [ ] PoC milestones render as gold (#F4B400) filled diamonds with drop shadow
- [ ] Production milestones render as green (#34A853) filled diamonds with drop shadow
- [ ] Each milestone has a date label positioned above or below the marker
- [ ] All milestone positions are calculated dynamically from dates in the JSON data
- [ ] The timeline date range (start/end) is configurable via JSON
- [ ] Track count is data-driven (supports 1–5 tracks without layout breaking)

---

### US-4: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a color-coded grid showing what was Shipped, In Progress, Carried Over, and Blocked for each month, **so that** I can assess monthly execution health and identify patterns.

**Visual Reference:** Heatmap section (`.hm-wrap`, `.hm-grid`) in `OriginalDesignConcept.html`.

**Acceptance Criteria:**
- [ ] The heatmap title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase 14px bold gray
- [ ] The grid uses CSS Grid with columns: 160px status label + N equal-width month columns
- [ ] Column headers display month names in 16px bold; the current month is highlighted with gold background (#FFF0D0) and amber text (#C07700)
- [ ] Row headers display category names in 11px bold uppercase with category-specific background colors
- [ ] Each data cell contains bullet-pointed items (6px colored circle + 12px text)
- [ ] Empty cells display a gray dash "—"
- [ ] Row color scheme matches the design exactly:
  - **Shipped:** header #E8F5E9 / cells #F0FBF0 / highlight #D8F2DA / accent #34A853
  - **In Progress:** header #E3F2FD / cells #EEF4FE / highlight #DAE8FB / accent #0078D4
  - **Carryover:** header #FFF8E1 / cells #FFFDE7 / highlight #FFF0B0 / accent #F4B400
  - **Blockers:** header #FEF2F2 / cells #FFF5F5 / highlight #FFE4E4 / accent #EA4335
- [ ] The heatmap fills all remaining vertical space below the timeline
- [ ] Month count is data-driven (supports 3–6 months)
- [ ] Items per cell are data-driven from JSON `heatmap.rows[].items`

---

### US-5: Load Dashboard Data from JSON Configuration File

**As a** project manager, **I want** the dashboard to load all display data from a single `dashboard-data.json` file, **so that** I can update project information by editing a text file without changing code.

**Acceptance Criteria:**
- [ ] The application reads `dashboard-data.json` from a configurable path (default: `wwwroot/data/dashboard-data.json`)
- [ ] The JSON file path is configurable via `appsettings.json`
- [ ] The JSON schema includes: `project` (title, subtitle, backlogUrl, currentDate), `timeline` (startDate, endDate, tracks with milestones), and `heatmap` (months, highlightMonth, rows with items)
- [ ] The application deserializes JSON into strongly-typed C# POCOs using `System.Text.Json`
- [ ] A sample `dashboard-data.json` with fictional project data is included in the repo
- [ ] The sample data demonstrates all features: multiple tracks, all milestone types, all four status categories, empty cells, and highlighted month

---

### US-6: Auto-Reload on Data File Change

**As a** project manager, **I want** the dashboard to automatically reflect changes when I edit and save `dashboard-data.json`, **so that** I can iterate on the data without restarting the application.

**Acceptance Criteria:**
- [ ] A `FileSystemWatcher` monitors the JSON data file for changes
- [ ] When the file changes, the data is re-read and the dashboard re-renders
- [ ] A polling fallback (5-second interval file timestamp check) handles missed `FileSystemWatcher` events
- [ ] The application does not crash if the file is temporarily locked during save

---

### US-7: Graceful Error Handling for Missing or Invalid Data

**As a** project manager, **I want** the dashboard to display a clear, friendly error message if the JSON file is missing or malformed, **so that** I can diagnose and fix the issue without technical support.

**Acceptance Criteria:**
- [ ] If `dashboard-data.json` is not found, display: "Dashboard data file not found. Expected location: [path]"
- [ ] If the JSON is malformed, display: "Error reading dashboard data: [specific error message]"
- [ ] Error messages render in the dashboard area (not a browser error page)
- [ ] The error state is visually clean — suitable for accidental screenshot without embarrassment
- [ ] The application recovers automatically when the file is fixed (via file watcher)

---

### US-8: Screenshot-Optimized Rendering

**As a** project manager, **I want** the dashboard to render at exactly 1920×1080 pixels, **so that** I can take a browser screenshot and paste it into a PowerPoint slide at full resolution without cropping or scaling.

**Acceptance Criteria:**
- [ ] `body` element is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] No scrollbars appear when the browser is at 100% zoom on a 1920×1080 or larger display
- [ ] All content fits within the 1080px vertical boundary
- [ ] The page prints cleanly via `@media print` (no navigation chrome, no Blazor connection indicators)
- [ ] Font rendering uses Segoe UI (pre-installed on Windows)

---

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` in the ReportingDashboard repository, supplemented by `C:/Pics/ReportingDashboardDesign.png`. Engineers MUST open and reference these files before writing any component code.

**Rendered Screenshot Reference:**
![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/fe3fc76172977fed76511f41ca8bddc4769300f8/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Page Layout

- **Viewport:** Fixed 1920×1080px, no scrolling, white (#FFFFFF) background
- **Font:** `'Segoe UI', Arial, sans-serif` — primary text color #111
- **Layout Direction:** Vertical flex column (`display: flex; flex-direction: column`)
- **Three stacked sections:** Header → Timeline → Heatmap (heatmap fills remaining space with `flex: 1`)

### Section 1: Header Bar (`.hdr`)

- **Height:** Auto (approximately 48px based on content)
- **Padding:** 12px top, 10px bottom, 44px left/right
- **Border:** 1px solid #E0E0E0 on bottom
- **Layout:** Flexbox, `justify-content: space-between; align-items: center`
- **Left side:**
  - Project title: `<h1>` at 24px, font-weight 700, color #111
  - Backlog link inline with title: color #0078D4, no underline
  - Subtitle: 12px, color #888, margin-top 2px
- **Right side (Legend):**
  - Horizontal flex with 22px gap
  - Four items, each: inline-flex with 6px gap, 12px font-size
  - PoC icon: 12×12px #F4B400 square rotated 45° (diamond)
  - Production icon: 12×12px #34A853 square rotated 45° (diamond)
  - Checkpoint icon: 8×8px circle, #999 background
  - Now indicator: 2×14px rectangle, #EA4335 background

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed 196px
- **Background:** #FAFAFA
- **Padding:** 6px top, 44px left/right, 0 bottom
- **Border:** 2px solid #E8E8E8 on bottom
- **Layout:** Horizontal flex (`display: flex; align-items: stretch`)
- **Left Panel (Track Labels):**
  - Width: 230px, fixed
  - Flex-direction: column, `justify-content: space-around`
  - Padding: 16px top/bottom, 12px right, 0 left
  - Border-right: 1px solid #E0E0E0
  - Each track label: 12px font, weight 600, track color for ID, #444 for name
  - Format: "M1" (bold, colored) + line break + "Chatbot & MS Role" (normal, #444)
- **Right Panel (SVG Timeline):**
  - Flex: 1 (fills remaining width, approximately 1560px)
  - Padding: 12px left, 6px top
  - SVG element: width 1560, height 185, `overflow: visible`
  - **Month gridlines:** Vertical lines at equal intervals, stroke #BBB opacity 0.4, width 1
  - **Month labels:** 11px, weight 600, fill #666, font-family "Segoe UI, Arial", positioned 5px right of gridline, y=14
  - **"NOW" line:** Dashed vertical line, stroke #EA4335, width 2, `stroke-dasharray: 5,3`; "NOW" label: 10px bold #EA4335
  - **Track lines:** Horizontal colored lines, stroke-width 3, spanning full SVG width
  - **Track vertical spacing:** Evenly distributed (e.g., y=42, y=98, y=154 for 3 tracks)
  - **Checkpoint markers:** `<circle>` r=5–7, fill white, stroke track-color or #888, stroke-width 2.5
  - **PoC diamond markers:** `<polygon>` diamond shape (±11px), fill #F4B400, drop shadow filter
  - **Production diamond markers:** `<polygon>` diamond shape (±11px), fill #34A853, drop shadow filter
  - **Drop shadow filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3">`
  - **Milestone labels:** `<text>` 10px, fill #666, `text-anchor: middle`, positioned above or below marker

### Section 3: Heatmap Area (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1; min-height: 0`
- **Padding:** 10px top/bottom, 44px left/right
- **Title bar:**
  - 14px, weight 700, color #888, letter-spacing 0.5px, uppercase
  - Margin-bottom: 8px
  - Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"
- **Grid (`.hm-grid`):**
  - `display: grid`
  - `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months
  - `grid-template-rows: 36px repeat(4, 1fr)`
  - Border: 1px solid #E0E0E0
  - `flex: 1; min-height: 0` (fills remaining space)

#### Grid Header Row

| Cell | Style |
|------|-------|
| **Corner cell** (`.hm-corner`) | Background #F5F5F5, 11px bold #999 uppercase "STATUS", border-right 1px #E0E0E0, border-bottom 2px #CCC |
| **Month headers** (`.hm-col-hdr`) | Background #F5F5F5, 16px bold, border-right 1px #E0E0E0, border-bottom 2px #CCC |
| **Highlighted month** (`.apr-hdr`) | Background #FFF0D0, color #C07700 |

#### Grid Data Rows — Color Scheme

| Category | Row Header BG | Row Header Text | Cell BG | Highlighted Cell BG | Bullet Accent |
|----------|--------------|-----------------|---------|---------------------|---------------|
| **Shipped** | #E8F5E9 | #1B7A28 | #F0FBF0 | #D8F2DA | #34A853 |
| **In Progress** | #E3F2FD | #1565C0 | #EEF4FE | #DAE8FB | #0078D4 |
| **Carryover** | #FFF8E1 | #B45309 | #FFFDE7 | #FFF0B0 | #F4B400 |
| **Blockers** | #FEF2F2 | #991B1B | #FFF5F5 | #FFE4E4 | #EA4335 |

#### Grid Cell Content

- **Row headers** (`.hm-row-hdr`): 11px bold uppercase, letter-spacing 0.7px, padding 0 12px, border-right 2px solid #CCC, border-bottom 1px solid #E0E0E0. Includes emoji prefix (✅, 🔵, ⚠️, 🔴) matching the original design.
- **Data cells** (`.hm-cell`): Padding 8px 12px, border-right 1px solid #E0E0E0, border-bottom 1px solid #E0E0E0, overflow hidden
- **Item bullets** (`.it`): 12px, color #333, padding 2px 0 2px 12px, line-height 1.35, with a 6×6px colored circle pseudo-element (`::before`) positioned at left:0, top:7px
- **Empty state:** Single item showing "—" in #AAA

### CSS Custom Properties (defined in `app.css :root`)

```css
:root {
  --shipped-bg: #F0FBF0;       --shipped-highlight: #D8F2DA;
  --shipped-accent: #34A853;   --shipped-header-bg: #E8F5E9;   --shipped-header-text: #1B7A28;
  --progress-bg: #EEF4FE;     --progress-highlight: #DAE8FB;
  --progress-accent: #0078D4; --progress-header-bg: #E3F2FD;   --progress-header-text: #1565C0;
  --carry-bg: #FFFDE7;        --carry-highlight: #FFF0B0;
  --carry-accent: #F4B400;    --carry-header-bg: #FFF8E1;      --carry-header-text: #B45309;
  --block-bg: #FFF5F5;        --block-highlight: #FFE4E4;
  --block-accent: #EA4335;    --block-header-bg: #FEF2F2;      --block-header-text: #991B1B;
  --highlight-month-bg: #FFF0D0; --highlight-month-text: #C07700;
  --border-light: #E0E0E0;    --border-medium: #CCC;           --border-heavy: #E8E8E8;
  --text-primary: #111;       --text-secondary: #666;           --text-muted: #888;  --text-body: #333;
  --link-color: #0078D4;      --now-color: #EA4335;
  --bg-page: #FFFFFF;         --bg-timeline: #FAFAFA;           --bg-header-cell: #F5F5F5;
}
```

### Component Hierarchy

```
MainLayout.razor
  └── Dashboard.razor (single page, "/" route)
        ├── Header.razor
        │     ├── Title + Subtitle + Backlog Link (left)
        │     └── Legend Icons (right)
        ├── Timeline.razor
        │     ├── Track Labels Panel (left, 230px)
        │     └── SVG Canvas (right, flex-fill)
        │           ├── Month gridlines + labels
        │           ├── NOW indicator line
        │           ├── Track horizontal lines
        │           └── Milestone markers (circles, diamonds) + labels
        └── Heatmap.razor
              ├── Section Title
              └── CSS Grid
                    ├── Corner cell
                    ├── Month column headers
                    ├── Row headers (Shipped, In Progress, Carryover, Blockers)
                    └── HeatmapCell.razor × (rows × months)
                          └── Bulleted item list or empty dash
```

---

## UI Interaction Scenarios

### Scenario 1: Initial Page Load — Happy Path

The user navigates to `https://localhost:5001`. The application reads `dashboard-data.json`, deserializes it, and renders the full dashboard. The header shows the project title, subtitle, and legend. The timeline shows all tracks with milestone markers positioned by date. The heatmap shows all four status rows across the configured months. The current month column is highlighted in gold. The entire page fits within 1920×1080 without scrollbars.

### Scenario 2: User Views the Timeline and Identifies Current Progress

The user looks at the timeline section. They see horizontal colored lines for each project track (e.g., blue for M1, teal for M2). A dashed red vertical "NOW" line marks today's date. Milestone diamonds (gold for PoC, green for Production) and checkpoint circles are positioned along each track line at their respective dates. Date labels appear above or below each marker. The user can immediately see which milestones are past, current, and upcoming.

### Scenario 3: User Scans the Heatmap for Execution Health

The user looks at the heatmap grid. The "April" column header is highlighted in gold (#FFF0D0) indicating the current month. The user scans the "Shipped" row (green) to see what was delivered, the "In Progress" row (blue) for active work, the "Carryover" row (yellow) for delayed items, and the "Blockers" row (red) for impediments. Each cell lists specific work items as bullet points with colored dots.

### Scenario 4: User Takes a Screenshot for PowerPoint

The user sets their browser to 100% zoom on a 1920×1080 or larger display. They use the browser's screenshot tool (or Snipping Tool / Win+Shift+S) to capture the full page. The resulting image is exactly 1920×1080 pixels, fits perfectly into a standard 16:9 PowerPoint slide, and requires no cropping or resizing.

### Scenario 5: User Clicks the Backlog URL

The user clicks the backlog link in the header (e.g., "→ ADO Backlog"). The link opens in a new browser tab, navigating to the Azure DevOps backlog URL specified in the JSON data. The dashboard remains open in the original tab.

### Scenario 6: User Edits the JSON Data File

The user opens `dashboard-data.json` in VS Code or any text editor. They change the project title, add a new milestone, or update heatmap items. They save the file. The `FileSystemWatcher` detects the change and the dashboard re-reads and re-renders the data within seconds, without the user needing to refresh the browser or restart the application.

### Scenario 7: Empty Heatmap Cells

For a month where a status category has no items (e.g., no Blockers in January), the corresponding cell displays a single gray dash "—" in #AAA color. The cell retains its background color and maintains grid alignment.

### Scenario 8: Missing Data File — Error State

The user starts the application but `dashboard-data.json` does not exist at the expected path. Instead of crashing or showing a browser error, the dashboard renders a centered, cleanly-styled error message: "Dashboard data file not found. Expected location: [full path]." The message uses the same Segoe UI font and is visually clean enough that an accidental screenshot would not be embarrassing.

### Scenario 9: Malformed JSON — Error State

The user accidentally introduces a syntax error in `dashboard-data.json` (e.g., trailing comma, missing bracket). The dashboard displays a clear error: "Error reading dashboard data: [JSON parse error details]." Once the user fixes the JSON and saves, the file watcher re-triggers and the dashboard recovers automatically.

### Scenario 10: Timeline with Variable Track Count

The JSON data defines only 2 tracks instead of the design's 3. The timeline adjusts: track lines are spaced evenly within the 185px SVG height, the left panel shows only 2 track labels, and all milestone markers render correctly. Similarly, if 4–5 tracks are defined, spacing adjusts proportionally.

### Scenario 11: Heatmap with Variable Month Count

The JSON data specifies 3 months instead of 4. The CSS Grid adjusts `grid-template-columns` to `160px repeat(3, 1fr)`. All cells render correctly with wider columns. If 6 months are specified, columns narrow proportionally but remain readable.

### Scenario 12: User Hovers Over a Milestone Diamond

The milestone diamond marker shows a subtle visual emphasis (e.g., slight scale increase or brightness change via CSS filter) to confirm interactivity is not needed — this is a read-only informational display. No tooltip is required for V1 as the date labels are already visible.

### Scenario 13: Browser Print / @media Print

The user presses Ctrl+P. The `@media print` styles hide any Blazor connection indicators or development UI. The dashboard prints as a single clean page matching the screen rendering.

---

## Scope

### In Scope

- Single-page Blazor Server (.NET 8) web application
- Fixed 1920×1080 pixel-perfect layout for screenshot capture
- Header component with project title, subtitle, backlog link, and milestone legend
- Timeline component with SVG-rendered milestone Gantt chart (dynamic positioning from data)
- Heatmap component with CSS Grid layout showing Shipped / In Progress / Carryover / Blockers by month
- Local `dashboard-data.json` file as sole data source
- Strongly-typed C# POCO models for JSON deserialization via `System.Text.Json`
- `DashboardDataService` singleton with `FileSystemWatcher` for auto-reload
- Configurable data file path via `appsettings.json`
- Sample `dashboard-data.json` with fictional project data demonstrating all features
- Global CSS reset and CSS custom properties for the color system
- Scoped `.razor.css` files for component-specific styling
- Graceful error handling for missing or malformed JSON
- `@media print` stylesheet for clean printing
- CSS custom properties for easy color theme adjustment
- Data-driven month count (3–6 months) and track count (1–5 tracks)

### Out of Scope

- **Authentication / authorization** — No login, no user roles, no session management
- **Database** — No SQLite, no SQL Server, no embedded DB of any kind
- **API layer** — No REST endpoints, no GraphQL, no controllers
- **Real-time collaboration** — Single-user, single-machine use only
- **Responsive / mobile layout** — Fixed 1920×1080 only; no tablet or phone breakpoints
- **Dark mode** — Deferred to future enhancement
- **PDF export** — Users will use browser screenshots; no server-side PDF generation
- **Multi-project switching** — V1 supports a single JSON file; query-parameter routing is deferred
- **Data editing UI** — Users edit JSON directly in a text editor; no in-app form
- **Charting libraries** — No Chart.js, D3, Plotly, or similar; SVG is hand-rendered
- **Third-party CSS frameworks** — No Bootstrap, MudBlazor, Tailwind, or similar
- **CI/CD pipeline** — Manual `dotnet build` / `dotnet run`; no GitHub Actions or Azure Pipelines
- **Docker / containerization** — Not needed for a local-only tool
- **Telemetry / analytics** — No Application Insights, no usage tracking
- **Internationalization / localization** — English only
- **Accessibility compliance (WCAG)** — Low priority for screenshot-only executive tool; color-blind patterns deferred
- **JSON Schema validation file** — Nice-to-have for Phase 2, not required for MVP
- **Tooltip interactivity** — Read-only; no hover tooltips on milestones for V1

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** | < 1 second on localhost (cold start) |
| **Data reload time** | < 500ms after JSON file change detected |
| **Memory usage** | < 100 MB RSS (single-user Blazor Server) |
| **SVG render time** | < 200ms for up to 5 tracks × 10 milestones each |

### Reliability

| Metric | Target |
|--------|--------|
| **Uptime** | N/A — runs on-demand locally |
| **Crash recovery** | Application must not crash on malformed JSON; display friendly error and recover on fix |
| **FileSystemWatcher fallback** | Polling every 5 seconds if watcher events are missed |

### Security

| Aspect | Requirement |
|--------|-------------|
| **Authentication** | None required |
| **Data sensitivity** | JSON contains project names and status descriptions only — no PII, no credentials |
| **Network exposure** | Localhost binding only (`https://localhost:5001`) |
| **HTTPS** | Default .NET dev cert is sufficient |

### Compatibility

| Aspect | Requirement |
|--------|-------------|
| **Target browsers** | Microsoft Edge (Chromium) and Google Chrome — latest stable versions |
| **Target OS** | Windows 10/11 (developer machine) |
| **Font dependency** | Segoe UI (pre-installed on Windows) |
| **Screen resolution** | Optimized for 1920×1080; functional at higher resolutions |
| **.NET SDK** | .NET 8.0.x |

### Maintainability

| Aspect | Target |
|--------|--------|
| **Total file count** | < 15 source files (excluding bin/obj) |
| **Third-party runtime dependencies** | Zero |
| **Build command** | `dotnet build` with zero warnings |
| **Run command** | `dotnet run` or `dotnet watch` |

---

## Success Metrics

1. **Visual fidelity:** A screenshot of the running dashboard is visually indistinguishable from the `OriginalDesignConcept.html` reference when viewed side-by-side at 1920×1080.
2. **Data-driven rendering:** Changing values in `dashboard-data.json` (title, milestones, heatmap items, month count) produces correct visual output without code changes.
3. **Screenshot workflow:** A project manager can update the JSON, take a screenshot, and paste it into a PowerPoint slide in under 2 minutes.
4. **Zero-config startup:** A new developer can clone the repo, run `dotnet run`, and see the dashboard in their browser without any setup beyond having the .NET 8 SDK installed.
5. **Error resilience:** Deleting or corrupting the JSON file produces a friendly in-page error message (not a crash or browser error).
6. **Auto-reload:** Editing and saving the JSON file while the app is running causes the dashboard to update within 5 seconds without manual browser refresh or app restart.
7. **Build health:** `dotnet build` completes with zero errors and zero warnings.
8. **File count:** The solution contains fewer than 15 source files in the `src/` directory.

---

## Constraints & Assumptions

### Technical Constraints

1. **Framework:** Must use Blazor Server on .NET 8.0.x — the existing team has C#/.NET expertise and the parent repo (`AgentSquad`) is .NET-based.
2. **No JavaScript:** All rendering must be achievable with Blazor Razor components, CSS, and inline SVG. No JavaScript interop, no npm packages.
3. **No third-party runtime packages:** Only `Microsoft.AspNetCore.App` framework reference. `System.Text.Json` is built-in. Test projects may use xUnit, bUnit, and FluentAssertions.
4. **Fixed viewport:** The page is designed for exactly 1920×1080 pixels. Responsive breakpoints are explicitly excluded.
5. **Windows-only font:** Segoe UI is assumed available. No web font fallback CDN is needed since the app runs on Windows.
6. **Local-only:** The app binds to localhost. No cloud deployment, no public URL.

### Timeline Assumptions

1. **Phase 1 (MVP):** 1–2 days — Project scaffold, data models, JSON loading, all three visual components (Header, Timeline, Heatmap), global CSS.
2. **Phase 2 (Polish):** 1 day — FileSystemWatcher auto-reload, error handling, print CSS, JSON schema.
3. **Phase 3 (Optional):** Future — Multi-project support, dark mode, Blazor Static SSR, PDF export.

### Dependency Assumptions

1. The developer machine has .NET 8 SDK installed.
2. The developer machine runs Windows 10 or 11 with Segoe UI font available.
3. Screenshots will be taken in Edge or Chrome at 100% zoom.
4. The `dashboard-data.json` file will be manually edited by the project manager using a text editor (VS Code recommended).
5. The JSON file structure will remain stable after initial implementation — schema changes require code changes to the C# POCOs.

### Design Assumptions

1. The `OriginalDesignConcept.html` file in the ReportingDashboard repository is the canonical visual reference. Any discrepancies between this spec and the HTML file should be resolved in favor of the HTML file.
2. The design screenshot at `C:/Pics/ReportingDashboardDesign.png` supplements the HTML reference and may show minor refinements to be incorporated.
3. The "NOW" line position is derived from the `currentDate` field in the JSON (not from the system clock) to ensure screenshot consistency.
4. The heatmap `highlightMonth` field in the JSON determines which month column gets the gold highlight — this is not auto-calculated.
5. Track colors in the timeline are specified per-track in the JSON data, not hardcoded.

### Open Decisions (to be resolved by engineering)

1. **Blazor render mode:** Standard Blazor Server (with SignalR) vs. Blazor Static SSR (no SignalR). Engineering should choose based on simplicity. Static SSR is preferred if straightforward to implement.
2. **Milestone label placement:** Labels should alternate above/below track lines to avoid overlap. The exact algorithm (alternating, collision-based, or fixed) is an engineering decision.
3. **SVG width calculation:** Whether to hardcode 1560px (matching the original design) or calculate dynamically based on container width. Hardcoded is acceptable for the fixed 1920px layout.