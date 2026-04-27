# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, delivery status, and monthly execution health in a screenshot-friendly format (1920×1080). The application loads all display data from a local `dashboard-data.json` configuration file, requires zero authentication or cloud dependencies, and is designed to be captured as images for PowerPoint decks presented to executive leadership. Built with C# .NET 8 Blazor Server, the dashboard replicates and improves upon the proven layout defined in `OriginalDesignConcept.html`: a milestone timeline header with SVG markers, and a color-coded heatmap grid showing Shipped, In Progress, Carryover, and Blockers by month.

## Business Goals

1. **Reduce executive reporting prep time by 75%** — Replace manual PowerPoint slide construction with a single dashboard page that can be screenshotted directly, eliminating the need to hand-draw timelines and status grids in slide editors.
2. **Provide a single, consistent visual format for project status** — Standardize how project milestones, shipped items, active work, carryover, and blockers are communicated to leadership across reporting cycles.
3. **Enable rapid data updates without developer involvement** — Allow the project manager to update a JSON file in a text editor and immediately see the refreshed dashboard, with no code changes or app restarts required.
4. **Deliver pixel-perfect screenshots at 1920×1080** — Ensure every dashboard render produces a clean, professional image suitable for direct insertion into executive presentation decks without post-processing.
5. **Minimize total cost of ownership** — Ship a zero-dependency, zero-infrastructure application that runs locally via `dotnet run` with no cloud services, databases, authentication systems, or external API calls.

## User Stories & Acceptance Criteria

### US-1: View Project Header Information

**As a** project manager preparing an executive deck, **I want** to see the project title, organizational context subtitle, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and workstream they are looking at.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` class.

- [ ] The header displays the project title in bold 24px font weight 700
- [ ] A subtitle line shows the organization, workstream, and current month in 12px gray (#888) text
- [ ] The backlog URL renders as a clickable hyperlink in Microsoft blue (#0078D4)
- [ ] A legend appears on the right side of the header showing four marker types: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and NOW line (red vertical bar)
- [ ] All text and layout values are loaded from `dashboard-data.json`, not hardcoded

---

### US-2: View Milestone Timeline

**As an** executive reviewing project status, **I want** to see a horizontal timeline with milestone markers for each major workstream, **so that** I can quickly understand the project's planned trajectory, key delivery dates, and where we stand relative to "now."

**Visual Reference:** Timeline/Gantt section of `OriginalDesignConcept.html` — `.tl-area` and inline SVG.

- [ ] The timeline section renders below the header with a light gray (#FAFAFA) background and a fixed height of 196px
- [ ] A left sidebar (230px wide) lists each milestone track with its ID (e.g., "M1"), name, and a color-coded label
- [ ] The SVG area (flex: 1) renders month gridlines as vertical lines with month labels (Jan–Jun or as configured)
- [ ] Each track renders as a horizontal colored line spanning the full SVG width
- [ ] Checkpoint markers render as circles (open or filled) at their date positions
- [ ] PoC milestones render as gold (#F4B400) diamond shapes with drop shadows
- [ ] Production milestones render as green (#34A853) diamond shapes with drop shadows
- [ ] A dashed red (#EA4335) vertical "NOW" line indicates the current date position
- [ ] Date labels appear above or below each marker in 10px gray text
- [ ] X-positions are calculated proportionally based on each marker's date relative to the timeline's start and end dates
- [ ] All timeline data (tracks, markers, dates) is loaded from `dashboard-data.json`

---

### US-3: View Monthly Execution Heatmap

**As an** executive reviewing project health, **I want** to see a color-coded grid showing what was shipped, what is in progress, what carried over from last month, and what is blocked — organized by month, **so that** I can assess delivery velocity and identify risks at a glance.

**Visual Reference:** Heatmap grid section of `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` classes.

- [ ] The heatmap section fills the remaining vertical space below the timeline
- [ ] A section title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase 14px gray (#888) text
- [ ] The grid uses CSS Grid with columns: `160px repeat(N, 1fr)` where N is the number of months
- [ ] The first row contains column headers: a "Status" corner cell and month name cells (16px bold)
- [ ] The current month's column header is highlighted with a golden background (#FFF0D0) and amber text (#C07700), with a "◀ Now" indicator
- [ ] Four data rows exist, one per status category: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each row header cell uses the category's dark color on a light tinted background with uppercase 11px bold text
- [ ] Each data cell lists work items as bullet-pointed lines with a 6px colored circle prefix
- [ ] The current month's data cells use a slightly darker background tint than other months
- [ ] Empty future-month cells display a gray dash ("—")
- [ ] All heatmap data is loaded from `dashboard-data.json`

---

### US-4: Load Dashboard Data from JSON File

**As a** project manager, **I want** the dashboard to read all its display data from a single `dashboard-data.json` file in the `wwwroot/data/` folder, **so that** I can update project status by editing a text file without touching code.

- [ ] The application reads `wwwroot/data/dashboard-data.json` at startup
- [ ] The JSON file contains sections for: `project` (title, subtitle, backlogUrl, currentMonth), `timeline` (tracks, markers, date range), and `heatmap` (months, categories with items)
- [ ] A `dashboard-data.sample.json` file ships with the repo containing realistic fictional project data ("Contoso Platform Modernization" or similar)
- [ ] The active `dashboard-data.json` is listed in `.gitignore` to prevent committing real project data
- [ ] If the JSON file is missing or malformed, the dashboard displays a friendly error message instead of crashing
- [ ] JSON deserialization uses case-insensitive property matching and nullable fields with sensible defaults

---

### US-5: Live-Reload Data Without Restart

**As a** project manager iterating on dashboard content, **I want** changes to `dashboard-data.json` to appear on the dashboard without restarting the application, **so that** I can refine the display in real-time before taking my screenshot.

- [ ] The application uses `PhysicalFileProvider` with `IChangeToken` to watch `dashboard-data.json` for changes
- [ ] When the file is saved, the dashboard re-reads and re-renders within 2 seconds
- [ ] If a file save introduces invalid JSON, the previous valid data continues to display and an error indicator appears
- [ ] No browser refresh is required (Blazor Server's SignalR connection pushes the update)

---

### US-6: Take a Consistent Screenshot

**As a** project manager, **I want** the dashboard to render at exactly 1920×1080 pixels every time, **so that** my screenshots are consistent and fit cleanly into my 16:9 PowerPoint slides.

- [ ] The `<body>` element has `width: 1920px; height: 1080px; overflow: hidden` set in CSS
- [ ] A `<meta name="viewport" content="width=1920">` tag locks the viewport width
- [ ] The layout does not scroll — all content fits within the 1080px height
- [ ] The design renders identically in Microsoft Edge and Google Chrome at 1920×1080

---

### US-7: Run the Dashboard Locally with One Command

**As a** developer or project manager, **I want** to start the dashboard with a single `dotnet run` command, **so that** I can view it immediately at `http://localhost:5000` without any setup, configuration, or external dependencies.

- [ ] `dotnet run` from the project directory starts Kestrel and serves the dashboard
- [ ] The browser opens automatically to the dashboard page (configured in `launchSettings.json`)
- [ ] No NuGet packages beyond the default .NET 8 Blazor Server template are required
- [ ] No database, no Docker, no cloud service, and no API keys are needed

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` and `docs/design-screenshots/OriginalDesignConcept.png` from the ReportingDashboard repository. All UI implementations MUST match these visuals. Engineers should open the HTML file in a browser at 1920×1080 as their pixel-reference.

### Overall Page Layout

- **Viewport:** Fixed 1920×1080 pixels, no scrolling, white (#FFFFFF) background
- **Layout Model:** Vertical flex column (`display: flex; flex-direction: column`)
- **Font Family:** `'Segoe UI', Arial, sans-serif` — system font, no web font loading
- **Base Text Color:** #111
- **Link Color:** #0078D4 (Microsoft blue), no underline

### Section 1: Header Bar (`.hdr`)

- **Height:** Auto (content-driven), approximately 50px
- **Padding:** `12px 44px 10px`
- **Border:** 1px solid #E0E0E0 on bottom
- **Layout:** Flexbox row, `align-items: center`, `justify-content: space-between`
- **Left side:**
  - Project title: `<h1>` at 24px, font-weight 700, color #111
  - Backlog link: inline `<a>` in #0078D4 blue, preceded by "▸" arrow character
  - Subtitle: 12px, color #888, margin-top 2px
- **Right side (Legend):**
  - Horizontal flex row with 22px gap
  - Four legend items, each as a flex row with 6px gap, 12px font size:
    - PoC Milestone: 12×12px square rotated 45° (`transform: rotate(45deg)`), fill #F4B400
    - Production Release: 12×12px square rotated 45°, fill #34A853
    - Checkpoint: 8×8px circle (`border-radius: 50%`), fill #999
    - NOW line: 2×14px vertical bar, fill #EA4335

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed 196px
- **Background:** #FAFAFA
- **Padding:** `6px 44px 0`
- **Border:** 2px solid #E8E8E8 on bottom
- **Layout:** Flexbox row, `align-items: stretch`

#### Timeline Left Sidebar

- **Width:** 230px, flex-shrink 0
- **Padding:** `16px 12px 16px 0`
- **Border:** 1px solid #E0E0E0 on right
- **Content:** Vertically distributed track labels, each containing:
  - Track ID (e.g., "M1") in 12px, font-weight 600, track color
  - Track name in 12px, font-weight 400, color #444
  - Track colors from reference: M1=#0078D4, M2=#00897B, M3=#546E7A

#### Timeline SVG Area (`.tl-svg-box`)

- **Flex:** 1 (fills remaining width)
- **Padding:** `12px left, 6px top`
- **SVG Dimensions:** 1560×185 pixels
- **SVG Elements:**
  - **Month gridlines:** Vertical lines at equal intervals (260px apart for 6 months), stroke #BBB at 0.4 opacity, 1px width
  - **Month labels:** 11px, font-weight 600, color #666, positioned 5px right of each gridline at y=14
  - **NOW line:** Dashed vertical line (`stroke-dasharray: 5,3`), stroke #EA4335, 2px width; "NOW" label in 10px bold #EA4335
  - **Track lines:** Horizontal lines spanning full width, 3px stroke, color matches track
  - **Checkpoint circles:** Open circles (white fill, colored stroke, 2.5px stroke-width, r=5–7) or small filled circles (r=4, fill #999)
  - **PoC diamonds:** SVG `<polygon>` forming a diamond (11px radius), fill #F4B400, with drop-shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`)
  - **Production diamonds:** Same shape, fill #34A853, same drop-shadow
  - **Date labels:** 10px, color #666, `text-anchor: middle`, positioned above or below markers

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Flex:** 1, fills all remaining vertical space
- **Padding:** `10px 44px 10px`
- **Layout:** Flex column

#### Heatmap Title (`.hm-title`)

- **Font:** 14px, font-weight 700, color #888
- **Style:** Uppercase, letter-spacing 0.5px
- **Margin:** 8px bottom

#### Heatmap Grid (`.hm-grid`)

- **Layout:** CSS Grid
- **Columns:** `160px repeat(N, 1fr)` where N = number of months (4 in reference)
- **Rows:** `36px repeat(4, 1fr)` — 36px header row + 4 equal data rows
- **Border:** 1px solid #E0E0E0

#### Grid Header Row

| Cell | Background | Font | Border |
|------|-----------|------|--------|
| Corner (`.hm-corner`) | #F5F5F5 | 11px bold uppercase #999 "STATUS" | Right: 1px #E0E0E0, Bottom: 2px #CCC |
| Month headers (`.hm-col-hdr`) | #F5F5F5 | 16px bold | Right: 1px #E0E0E0, Bottom: 2px #CCC |
| Current month header (`.hm-col-hdr.apr-hdr`) | #FFF0D0 | 16px bold #C07700 | Same borders |

#### Grid Data Rows — Color Specifications

| Category | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Bullet Color |
|----------|--------------|----------------|---------|----------------------|-------------|
| **Shipped** | #E8F5E9 | #1B7A28 | #F0FBF0 | #D8F2DA | #34A853 |
| **In Progress** | #E3F2FD | #1565C0 | #EEF4FE | #DAE8FB | #0078D4 |
| **Carryover** | #FFF8E1 | #B45309 | #FFFDE7 | #FFF0B0 | #F4B400 |
| **Blockers** | #FEF2F2 | #991B1B | #FFF5F5 | #FFE4E4 | #EA4335 |

#### Data Cell Styling (`.hm-cell .it`)

- **Font:** 12px, color #333, line-height 1.35
- **Padding:** `2px 0 2px 12px` (relative positioning)
- **Bullet:** 6×6px circle via `::before` pseudo-element, positioned `left: 0, top: 7px`, colored per row category
- **Empty cells:** Single item with text "—" in color #AAA

### Component Hierarchy

```
App.razor
└── MainLayout.razor (no nav, no sidebar — bare shell)
    └── Dashboard.razor (single page, route "/")
        ├── Header.razor (title, subtitle, legend)
        ├── Timeline.razor (left sidebar + SVG area)
        └── Heatmap.razor (title + grid)
            └── HeatmapRow.razor × 4 (Shipped, InProgress, Carryover, Blockers)
```

### CSS Custom Properties (defined in `app.css :root`)

```
--color-shipped: #34A853          --color-shipped-bg: #F0FBF0
--color-shipped-bg-current: #D8F2DA   --color-shipped-header-bg: #E8F5E9
--color-progress: #0078D4         --color-progress-bg: #EEF4FE
--color-progress-bg-current: #DAE8FB  --color-progress-header-bg: #E3F2FD
--color-carryover: #F4B400        --color-carryover-bg: #FFFDE7
--color-carryover-bg-current: #FFF0B0 --color-carryover-header-bg: #FFF8E1
--color-blockers: #EA4335         --color-blockers-bg: #FFF5F5
--color-blockers-bg-current: #FFE4E4  --color-blockers-header-bg: #FEF2F2
--color-now-line: #EA4335         --color-poc-diamond: #F4B400
--color-prod-diamond: #34A853
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Dashboard Renders from JSON**
The user navigates to `http://localhost:5000`. The Blazor Server app loads `dashboard-data.json`, deserializes it, and renders the full dashboard: header with project title and legend, timeline SVG with all milestone tracks and markers, and the heatmap grid with all four status rows populated. The entire page fits within 1920×1080 with no scrollbars. Total load time is under 1 second.

**Scenario 2: User Views the Header and Identifies the Project**
The user sees the project title in large bold text at the top-left (e.g., "Contoso Platform Modernization Release Roadmap"). Below it, a subtitle shows the organization and current month. On the right, a legend explains the four marker types used in the timeline. The user clicks the "▸ ADO Backlog" link, which opens the Azure DevOps backlog URL from the JSON in a new browser tab.

**Scenario 3: User Reads the Milestone Timeline**
The user scans the timeline section. Three horizontal tracks are visible, each labeled on the left (M1, M2, M3) with a name and color. Along each track, the user sees positioned markers: open circles for project start checkpoints, small filled dots for intermediate checkpoints, gold diamonds for PoC milestones, and green diamonds for production releases. Each marker has a date label. A dashed red vertical line labeled "NOW" indicates the current date, allowing the user to see which milestones are past and which are upcoming.

**Scenario 4: User Hovers Over a Milestone Diamond**
The user hovers their mouse over a gold PoC diamond on the timeline. The browser displays the SVG element's native tooltip (if a `<title>` child element is present in the SVG polygon). The diamond has a subtle drop-shadow that distinguishes it from flat elements, making it visually prominent without additional interaction.

**Scenario 5: User Assesses Monthly Delivery Health via Heatmap**
The user looks at the heatmap grid. The current month column is highlighted with a golden header background. The user reads the "Shipped" row to see what was delivered, the "In Progress" row to see active work, the "Carryover" row to identify slipped items from prior months, and the "Blockers" row to note impediments. Each item has a colored bullet matching its category. Future months show gray dashes indicating no data yet.

**Scenario 6: User Compares Month-Over-Month Progress**
The user scans across the Shipped row left to right, observing that the number of items increases each month, indicating acceleration. They notice two items in last month's Carryover row that now appear in this month's Shipped row, confirming those items were resolved. The color coding makes cross-row scanning fast.

**Scenario 7: User Updates the JSON File and Sees Live Changes**
The user opens `dashboard-data.json` in VS Code, adds a new item to the "inProgress" category for the current month, and saves the file. Within 2 seconds, the Blazor Server detects the file change via `PhysicalFileProvider`, reloads the data, and pushes the updated UI to the browser via SignalR. The new item appears in the In Progress row without a browser refresh.

**Scenario 8: User Saves Invalid JSON**
The user accidentally introduces a syntax error in `dashboard-data.json` (e.g., a missing comma). The file watcher detects the change and attempts deserialization, which fails. The dashboard continues displaying the last known valid data and shows a subtle error indicator (e.g., a small red banner at the bottom: "⚠ Data file error — showing last valid data"). The user fixes the JSON and saves again; the dashboard updates normally.

**Scenario 9: JSON File Is Missing on Startup**
The application starts but `dashboard-data.json` does not exist in `wwwroot/data/`. Instead of a crash or blank page, the dashboard renders a centered, friendly message: "No dashboard data found. Place a dashboard-data.json file in wwwroot/data/ to get started. See dashboard-data.sample.json for the expected format." The sample file path is displayed as a helpful hint.

**Scenario 10: User Takes a Screenshot for PowerPoint**
The user opens Microsoft Edge, navigates to `http://localhost:5000`, and presses F12 to open DevTools. They enable Device Emulation at 1920×1080 resolution. The dashboard renders pixel-perfectly at that resolution. The user takes a screenshot (Ctrl+Shift+S in Edge or Snipping Tool), which produces a clean 1920×1080 image ready to paste into a PowerPoint slide with no cropping needed.

**Scenario 11: Dashboard Renders with Zero Milestones**
The JSON file contains an empty `tracks` array in the timeline section. The timeline area still renders with month gridlines and the NOW line, but no track lines or markers are shown. The left sidebar displays no track labels. The heatmap renders normally since it is independent of the timeline data.

**Scenario 12: Dashboard Renders with Empty Heatmap Categories**
The JSON file has all four heatmap categories with empty item arrays for every month. The grid renders with the correct headers and row labels, but every data cell shows a gray dash ("—"). This allows the user to set up the structure before data is available.

**Scenario 13: Heatmap Has Variable Month Counts**
The JSON file defines 6 months instead of 4. The CSS Grid auto-adjusts: `grid-template-columns: 160px repeat(6, 1fr)`. Column headers display all 6 months. The grid still fills the available width because columns use `1fr` units. The current month column is highlighted based on `currentMonthIndex`.

## Scope

### In Scope

- Single-page Blazor Server dashboard matching the `OriginalDesignConcept.html` reference design
- Header component with project title, subtitle, backlog link, and marker legend
- SVG-based milestone timeline with configurable tracks, markers (checkpoint, PoC, production), and a "NOW" date indicator
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) and N month columns
- JSON configuration file (`dashboard-data.json`) as the sole data source
- Sample data file (`dashboard-data.sample.json`) with realistic fictional project data
- File-watch auto-reload when JSON is updated (no app restart required)
- Fixed 1920×1080 viewport optimized for screenshot capture
- Graceful error handling for missing or malformed JSON files
- CSS custom properties for color theming
- `launchSettings.json` for local development configuration
- README.md with setup instructions, JSON schema documentation, and screenshot workflow
- `.gitignore` entry for `dashboard-data.json` (real data stays local)

### Out of Scope

- **Authentication / Authorization** — No login, no role-based access, no Windows Integrated Auth
- **Database** — No SQLite, no SQL Server, no Entity Framework
- **Admin / Editor UI** — No forms for editing data; users edit JSON directly in a text editor
- **Responsive design** — Fixed 1920×1080 only; no mobile, tablet, or variable-width layouts
- **Dark mode** — Single light theme only
- **Print stylesheet** — Not needed; screenshots are the distribution mechanism
- **REST API endpoints** — No controllers, no Swagger, no external API surface
- **Logging / Telemetry** — No Application Insights, no Serilog, no structured logging
- **Docker container** — Local `dotnet run` only
- **CI/CD pipeline** — No GitHub Actions, no Azure DevOps pipelines
- **Multi-project support** — One JSON file, one dashboard; no project switcher
- **JavaScript interop** — Pure Blazor/CSS/SVG; no JS libraries
- **UI component libraries** — No MudBlazor, Radzen, Syncfusion, or similar
- **Charting libraries** — No Chart.js, no Plotly; hand-crafted SVG only
- **Internationalization / Localization** — English only
- **Accessibility (WCAG compliance)** — Not a primary requirement for this internal screenshot tool; semantic HTML is used where practical

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Cold start time** | Dashboard visible in browser within 3 seconds of `dotnet run` |
| **Page render time** | Full dashboard renders in < 500ms after initial Blazor circuit connection |
| **JSON reload time** | File change detected and UI updated within 2 seconds of file save |
| **Memory usage** | < 100 MB RAM at runtime (Blazor Server + Kestrel + one SignalR circuit) |
| **Build time** | `dotnet build` completes in < 10 seconds |

### Security

- **Threat model:** Single-user local application on a developer workstation. No network exposure beyond `localhost`.
- **Data classification:** Non-sensitive — the JSON contains the same project status information shared in executive meetings and PowerPoint decks.
- **Network:** Kestrel binds to `localhost` only. No external network listeners.
- **Data at rest:** Plain-text JSON. No encryption required.
- **Recommendation:** Add `dashboard-data.json` to `.gitignore` to avoid accidentally committing project-specific data to the repository.

### Reliability

- **Availability target:** N/A — the app runs on-demand when the user needs a screenshot. No uptime SLA.
- **Error recovery:** If JSON is malformed, display last valid data with an error indicator. If JSON is missing, display a helpful setup message. The app must never crash or show a stack trace to the user.
- **Browser compatibility:** Tested on Microsoft Edge (Chromium) and Google Chrome. No Safari, Firefox, or mobile browser support required.

### Scalability

- **Not applicable.** This is a single-user, single-machine, local-only application. It will never serve concurrent users or handle variable load.

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Visual fidelity** | Dashboard output matches `OriginalDesignConcept.png` reference at ≥95% pixel accuracy when compared side-by-side at 1920×1080 | Manual visual comparison by PM |
| 2 | **Data-driven rendering** | All text, dates, items, and colors in the dashboard are sourced from `dashboard-data.json` with zero hardcoded project data | Code review confirming no hardcoded strings in Razor components |
| 3 | **Setup simplicity** | A new developer can clone the repo and see the dashboard in browser within 5 minutes using only `dotnet run` | Timed walkthrough with README instructions |
| 4 | **JSON edit-to-render cycle** | Editing and saving the JSON file results in a visible dashboard update within 2 seconds, no restart | Stopwatch test during file-watch demo |
| 5 | **Screenshot quality** | A browser screenshot at 1920×1080 is directly usable in a PowerPoint slide with no cropping, resizing, or cleanup | PM inserts screenshot into actual executive deck |
| 6 | **Codebase simplicity** | Total project contains ≤ 15 source files and zero additional NuGet packages beyond the Blazor Server template | File count and `.csproj` inspection |
| 7 | **Error resilience** | Missing or malformed JSON produces a user-friendly message, never a stack trace or blank page | Manual test with deleted and corrupted JSON files |

## Constraints & Assumptions

### Technical Constraints

- **Technology stack:** C# .NET 8 Blazor Server is mandatory. No alternative frameworks (React, Angular, plain HTML) will be considered.
- **Target runtime:** .NET 8 LTS SDK must be installed on the development machine. Minimum version: 8.0.x.
- **Operating system:** Windows (Segoe UI font dependency). Mac/Linux are not tested or supported.
- **Browser:** Microsoft Edge (Chromium) is the primary target. Chrome is a secondary target. No other browsers are supported.
- **Viewport:** Fixed 1920×1080. The layout will not adapt to other resolutions.
- **Data format:** JSON only. No YAML, TOML, XML, or database alternatives.
- **No external services:** The application must function entirely offline with no network calls beyond `localhost`.

### Timeline Assumptions

- **Phase 1 (Core MVP):** 1 day — Scaffold project, implement data models, build all three visual sections (Header, Timeline, Heatmap), create sample data file, verify visual fidelity.
- **Phase 2 (Polish):** 1 day — Add file-watch reload, error handling, CSS custom properties, basic tests, README, and `launchSettings.json`.
- **Total estimated effort:** 2 developer-days for a .NET developer familiar with Blazor and CSS.

### Dependency Assumptions

- The developer has .NET 8 SDK installed locally.
- The developer has access to the `OriginalDesignConcept.html` reference file and the `OriginalDesignConcept.png` screenshot for visual comparison.
- The developer has Microsoft Edge or Google Chrome for testing and screenshot capture.
- No external APIs, cloud services, or third-party accounts are needed.
- The `dashboard-data.sample.json` file will use a fictional project name (e.g., "Contoso Platform Modernization") to avoid exposing internal project data in the repository.

### Design Assumptions

- The heatmap month count is data-driven (determined by the `months` array in JSON), not fixed at 4.
- The "NOW" line position is auto-calculated from `DateTime.Now` relative to the timeline date range, with an optional `nowOverride` field in JSON for screenshot consistency.
- The number of timeline tracks is data-driven (determined by the `tracks` array in JSON), not fixed at 3.
- The CSS Grid column widths automatically adjust when the month count changes (using `1fr` units).
- Work items in heatmap cells have no character limit enforced in the UI, but long text will be clipped by the cell's `overflow: hidden` CSS property.