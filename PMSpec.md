# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones on an SVG timeline and displays a color-coded heatmap grid of work items by delivery status (Shipped, In Progress, Carryover, Blockers). The dashboard reads all data from a local `data.json` file, runs locally via `dotnet run` with zero cloud dependencies, and is optimized for pixel-perfect 1920×1080 screenshots to be embedded in PowerPoint decks for executive stakeholders.

## Business Goals

1. **Provide at-a-glance project visibility** — Give executives a single-page view of project health, milestone progress, and delivery status without requiring them to log into ADO, Jira, or any other tool.
2. **Eliminate manual slide creation** — Replace hand-built PowerPoint status slides with a screenshot of a live, data-driven dashboard that can be updated by editing a JSON file.
3. **Reduce status reporting overhead** — Enable project leads to update a single `data.json` file and produce a presentation-ready visual in under 60 seconds.
4. **Standardize reporting format** — Establish a reusable template that can be cloned for any project by swapping out the data file, ensuring consistent executive communication across teams.
5. **Maintain zero infrastructure cost** — Deliver the solution with no servers, no databases, no cloud services, and no ongoing operational burden.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** to see the project title, organizational context (subtitle), and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project they are looking at and can drill into the backlog if needed.

**Visual Reference:** Header section (`.hdr`) of `OriginalDesignConcept.html`

- [ ] The page displays the project title in 24px bold font at the top-left
- [ ] A subtitle line shows the organization, workstream, and current month in 12px gray text
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, styled in `#0078D4` blue
- [ ] A legend appears at the top-right showing icons for: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now line (red vertical bar)
- [ ] All text values are driven from `data.json` (title, subtitle, backlogUrl)

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing milestone tracks with key dates marked as diamonds and circles, **so that** I can understand the project's planned cadence and current position at a glance.

**Visual Reference:** Timeline area (`.tl-area` and inline SVG) of `OriginalDesignConcept.html`

- [ ] The timeline section appears below the header with a light gray (`#FAFAFA`) background
- [ ] A left sidebar (230px wide) lists each milestone track with its ID (e.g., "M1") and description, color-coded per track
- [ ] The SVG area renders horizontal track lines spanning the full date range
- [ ] Month boundaries are marked with vertical grid lines and labeled (e.g., "Jan", "Feb")
- [ ] Checkpoint milestones render as small circles on the track line
- [ ] PoC milestones render as gold (`#F4B400`) diamond shapes with drop shadow
- [ ] Production milestones render as green (`#34A853`) diamond shapes with drop shadow
- [ ] A red dashed vertical "NOW" line (`#EA4335`) marks the current date position
- [ ] Each milestone has a date label positioned above or below to avoid overlap
- [ ] The number of tracks, milestones, and date range are all driven from `data.json`

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a grid showing what was shipped, what is in progress, what carried over, and what is blocked — organized by month, **so that** I can assess delivery velocity and identify risks.

**Visual Reference:** Heatmap section (`.hm-wrap` and `.hm-grid`) of `OriginalDesignConcept.html`

- [ ] The heatmap section title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase gray text
- [ ] The grid has a "Status" corner cell, month column headers, and four status rows
- [ ] The current month column header is highlighted with a gold background (`#FFF0D0`) and "← Now" indicator
- [ ] **Shipped row:** Green header (`#E8F5E9`), cells with `#F0FBF0` background (current month: `#D8F2DA`), green bullet dots (`#34A853`)
- [ ] **In Progress row:** Blue header (`#E3F2FD`), cells with `#EEF4FE` background (current month: `#DAE8FB`), blue bullet dots (`#0078D4`)
- [ ] **Carryover row:** Amber header (`#FFF8E1`), cells with `#FFFDE7` background (current month: `#FFF0B0`), amber bullet dots (`#F4B400`)
- [ ] **Blockers row:** Red header (`#FEF2F2`), cells with `#FFF5F5` background (current month: `#FFE4E4`), red bullet dots (`#EA4335`)
- [ ] Each cell lists work items as bullet-pointed text (12px, `#333`)
- [ ] Empty cells for future months display a gray dash ("—")
- [ ] All items, months, and current-month designation are driven from `data.json`

### US-4: Load Dashboard Data from JSON

**As a** project lead, **I want** the dashboard to read all its content from a `data.json` file, **so that** I can update project status by editing a single file without touching code.

- [ ] A `data.json` file in `wwwroot/data/` contains all dashboard content (title, subtitle, backlog URL, current date, timeline tracks/milestones, heatmap months/rows/items)
- [ ] The application deserializes `data.json` on startup using `System.Text.Json`
- [ ] If `data.json` is missing or malformed, the application logs a clear error message and does not crash
- [ ] Changing `data.json` and restarting the app reflects the updated data on the dashboard

### US-5: Capture Screenshot-Ready Output

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, **so that** I can take a full-page screenshot and paste it directly into a PowerPoint slide.

- [ ] The `<body>` element is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] All content fits within the viewport without scrolling
- [ ] The layout uses Segoe UI as the primary font (with Arial fallback)
- [ ] Colors, spacing, and typography match the `OriginalDesignConcept.html` reference exactly
- [ ] The page renders identically in Microsoft Edge and Google Chrome

### US-6: Run Dashboard Locally

**As a** developer, **I want** to start the dashboard with a single `dotnet run` command, **so that** there is no complex setup, deployment pipeline, or infrastructure dependency.

- [ ] `dotnet run` starts the application and serves it on `http://localhost:5000` (or configurable port)
- [ ] No database, cloud service, or external API is required
- [ ] No third-party NuGet packages beyond the default .NET 8 SDK are required
- [ ] The project builds with `dotnet build` with zero warnings

### US-7: Hot-Reload Data Without Restart

**As a** project lead, **I want** the dashboard to automatically detect changes to `data.json` and refresh, **so that** I can iterate on the data without restarting the application.

- [ ] A `FileSystemWatcher` monitors `data.json` for changes
- [ ] When the file is modified, the service reloads and re-deserializes the data
- [ ] The Blazor page re-renders via `StateHasChanged()` without requiring a browser refresh
- [ ] If the updated file is malformed JSON, the dashboard retains the last valid data and logs a warning

## Visual Design Specification

**Canonical Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository. Engineers MUST consult this file and the rendered screenshot (`OriginalDesignConcept.png`) as the authoritative visual spec. All implementations must match these visuals exactly.

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/main/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Layout

- **Viewport:** Fixed 1920×1080 pixels, no scroll, white (`#FFFFFF`) background
- **Font:** `'Segoe UI', Arial, sans-serif` — base text color `#111`
- **Structure:** Vertical flex column (`display: flex; flex-direction: column`) with three stacked sections:
  1. **Header** (`.hdr`) — fixed height, top of page
  2. **Timeline Area** (`.tl-area`) — fixed 196px height, middle section
  3. **Heatmap Area** (`.hm-wrap`) — flex-grow fills remaining vertical space

### Section 1: Header (`.hdr`)

- **Layout:** Flexbox row, `justify-content: space-between`, `align-items: center`
- **Padding:** `12px 44px 10px`
- **Border:** Bottom `1px solid #E0E0E0`
- **Left side:**
  - Project title: `<h1>` at `24px`, `font-weight: 700`, color `#111`
  - Inline backlog link: `color: #0078D4`, no underline
  - Subtitle: `12px`, `color: #888`, `margin-top: 2px`
- **Right side — Legend:** Horizontal flex with `gap: 22px`, each legend item at `12px` font:
  - PoC Milestone: `12×12px` gold (`#F4B400`) square rotated 45° (diamond)
  - Production Release: `12×12px` green (`#34A853`) square rotated 45° (diamond)
  - Checkpoint: `8×8px` gray (`#999`) circle
  - Now line: `2×14px` red (`#EA4335`) vertical bar

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox row, `height: 196px`, `background: #FAFAFA`, `border-bottom: 2px solid #E8E8E8`
- **Padding:** `6px 44px 0`
- **Left sidebar** (230px wide, `flex-shrink: 0`):
  - Vertical flex column, `justify-content: space-around`
  - Each track label: `12px`, `font-weight: 600`, track color for ID, `#444` for description
  - Right border: `1px solid #E0E0E0`
- **Right SVG area** (`.tl-svg-box`, `flex: 1`):
  - SVG element: `width="1560" height="185"`, `overflow: visible`
  - **Month grid lines:** Vertical lines at equal intervals, `stroke: #bbb`, `stroke-opacity: 0.4`
  - **Month labels:** `11px`, `font-weight: 600`, `fill: #666`, positioned 5px right of grid line
  - **NOW line:** Red dashed vertical (`stroke: #EA4335`, `stroke-width: 2`, `stroke-dasharray: 5,3`), with "NOW" label (`10px`, `font-weight: 700`, `fill: #EA4335`)
  - **Track lines:** Horizontal lines spanning full width, `stroke-width: 3`, colored per track
  - **Checkpoint markers:** `<circle>` with `r="5-7"`, white fill, track-color stroke, `stroke-width: 2.5`
  - **PoC milestones:** `<polygon>` diamond (11px radius), `fill: #F4B400`, with drop shadow filter
  - **Production milestones:** `<polygon>` diamond (11px radius), `fill: #34A853`, with drop shadow filter
  - **Milestone labels:** `10px`, `fill: #666`, `text-anchor: middle`, positioned above or below the track line
  - **Drop shadow filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`

### Section 3: Heatmap Area (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1`, `padding: 10px 44px 10px`
- **Section title** (`.hm-title`): `14px`, `font-weight: 700`, `color: #888`, `letter-spacing: 0.5px`, uppercase

#### Heatmap Grid (`.hm-grid`)

- **Layout:** CSS Grid, `border: 1px solid #E0E0E0`
- **Columns:** `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months
- **Rows:** `grid-template-rows: 36px repeat(4, 1fr)` (header row + 4 status rows)

##### Header Row
- **Corner cell** (`.hm-corner`): `background: #F5F5F5`, `11px`, `font-weight: 700`, `color: #999`, uppercase, `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`
- **Month headers** (`.hm-col-hdr`): `16px`, `font-weight: 700`, `background: #F5F5F5`, centered, `border-bottom: 2px solid #CCC`
- **Current month header** (`.apr-hdr`): `background: #FFF0D0`, `color: #C07700`

##### Status Rows — Color Specifications

| Status | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Bullet Dot Color |
|--------|--------------|-----------------|---------|----------------------|-----------------|
| ✅ Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| 🔵 In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| 🟡 Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| 🔴 Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

##### Row Headers (`.hm-row-hdr`)
- `11px`, `font-weight: 700`, uppercase, `letter-spacing: 0.7px`
- `border-right: 2px solid #CCC`, `border-bottom: 1px solid #E0E0E0`

##### Data Cells (`.hm-cell`)
- `padding: 8px 12px`, `border-right: 1px solid #E0E0E0`, `border-bottom: 1px solid #E0E0E0`, `overflow: hidden`
- **Items** (`.it`): `12px`, `color: #333`, `padding: 2px 0 2px 12px`, `line-height: 1.35`
- **Bullet dot** (`::before` pseudo-element): `6×6px` circle, `position: absolute`, `left: 0`, `top: 7px`, colored per status row

### Component Hierarchy

```
Dashboard.razor (page layout, data injection)
├── Header (title, subtitle, backlog link, legend)
├── Timeline.razor (SVG timeline)
│   ├── Month grid lines and labels
│   ├── Track lines (one per milestone track)
│   ├── Milestone markers (circles, diamonds)
│   └── NOW line
└── Heatmap.razor (CSS Grid)
    ├── Header row (corner + month headers)
    └── Status rows × 4
        ├── Row header (status label)
        └── HeatmapCell.razor × N months
            └── Item bullets (text with colored dot)
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The dashboard renders immediately as a single full-page view at 1920×1080. The header shows the project title, subtitle, and legend. The timeline section displays all milestone tracks with markers positioned by date. The heatmap grid shows all status rows populated with work items. The current month column is highlighted. The red "NOW" line is positioned at the current date on the timeline.

**Scenario 2: User Views Milestone Timeline**
User looks at the timeline section and sees horizontal colored track lines (one per workstream). Each track has markers: small gray circles for checkpoints, gold diamonds for PoC milestones, and green diamonds for production releases. A red dashed vertical "NOW" line bisects the timeline at the current date. Month labels (Jan, Feb, Mar, etc.) appear along the top of the SVG area at each month boundary.

**Scenario 3: User Hovers Over a Milestone Diamond**
User hovers over a gold PoC diamond on the timeline. The browser's native SVG tooltip (via `<title>` element) shows the milestone label and date (e.g., "Mar 26 PoC"). No custom tooltip implementation is required — the SVG `<title>` element provides accessible hover text.

**Scenario 4: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser opens the URL specified in `data.json`'s `backlogUrl` field in a new tab. If no URL is configured, the link is not rendered.

**Scenario 5: User Reads the Heatmap to Assess Project Health**
User scans the heatmap from top to bottom. The Shipped row (green) shows completed items — more items here signals healthy delivery. The In Progress row (blue) shows active work. The Carryover row (amber) shows items that slipped from a prior month — this is a warning signal. The Blockers row (red) shows items that cannot proceed — this requires executive attention. The current month column is visually highlighted with a warmer background tone in each row.

**Scenario 6: User Views an Empty Future Month**
User looks at a future month column (e.g., May, June). Cells in future months display a gray dash ("—") to indicate no items are planned or tracked yet. The cell backgrounds remain in their status-row color but with no bullet items.

**Scenario 7: User Updates data.json and Dashboard Refreshes**
User edits `data.json` in a text editor — adds a new shipped item to April. The `FileSystemWatcher` detects the file change, reloads the data, and triggers a Blazor re-render. The dashboard updates in the browser without requiring a manual refresh or restart.

**Scenario 8: User Provides Malformed data.json**
User accidentally introduces a JSON syntax error in `data.json`. The `FileSystemWatcher` detects the change and attempts to deserialize. Deserialization fails; the service logs a warning message to the console (e.g., "Failed to reload data.json: unexpected token at line 42"). The dashboard continues to display the last successfully loaded data. No crash, no blank page.

**Scenario 9: data.json File Is Missing on Startup**
User starts the application but `data.json` does not exist at the expected path. The service logs an error ("data.json not found at wwwroot/data/data.json"). The dashboard renders with a centered message: "No data file found. Place a data.json file in wwwroot/data/ and restart." No unhandled exception.

**Scenario 10: User Takes a Screenshot**
User opens the dashboard in Edge or Chrome, sets the viewport to 1920×1080 (via DevTools device toolbar or full-screen on a 1080p monitor), and captures a screenshot using the browser's screenshot tool or Windows Snipping Tool. The resulting image is a clean, presentation-ready slide with no browser chrome, scrollbars, or clipped content.

**Scenario 11: Dashboard Renders with Variable Month Count**
The `data.json` file is configured with 3 months instead of 4. The CSS Grid dynamically adjusts: `grid-template-columns: 160px repeat(3, 1fr)`. All cells expand proportionally. The timeline SVG adjusts its date range and month grid lines accordingly. No hardcoded assumptions about "exactly 4 months."

**Scenario 12: Dashboard Renders with Variable Track Count**
The `data.json` file is configured with 2 milestone tracks instead of 3. The timeline sidebar shows 2 track labels. The SVG renders 2 horizontal track lines with proportionally adjusted vertical spacing. The SVG height adapts to the number of tracks.

## Scope

### In Scope

- Single-page Blazor Server application rendering at 1920×1080
- Header section with project title, subtitle, backlog link, and milestone legend
- SVG timeline with configurable milestone tracks, date-positioned markers (checkpoint circles, PoC diamonds, production diamonds), month grid lines, and a "NOW" date indicator
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- Current month highlighting in the heatmap with distinct background colors
- All content driven from a single `data.json` flat file
- `System.Text.Json` deserialization into C# POCO models
- `FileSystemWatcher`-based hot-reload of `data.json`
- Error handling for missing or malformed `data.json`
- Fictional example data for "Project Atlas — Cloud Migration Platform"
- Scoped CSS matching the `OriginalDesignConcept.html` design exactly
- Local-only execution via `dotnet run` on Kestrel

### Out of Scope

- **Authentication & authorization** — No login, no roles, no access control
- **Database** — No SQLite, no LiteDB, no Entity Framework
- **Cloud deployment** — No Azure, no AWS, no Docker containers
- **Responsive design** — Fixed 1920×1080 only; no mobile, no tablet
- **Multi-project support** — One project per `data.json` instance; no project selector UI
- **Real-time collaboration** — Single user, local machine only
- **Data entry UI** — No forms, no editors; data is edited in a text editor
- **Automated screenshot capture** — Manual screenshot via browser tools
- **Print stylesheet** — Not required; screenshots are the distribution method
- **Accessibility compliance (WCAG)** — Nice-to-have but not a requirement for this internal tool
- **Internationalization / localization** — English only
- **CI/CD pipeline** — No build pipeline, no automated deployment
- **Third-party NuGet packages** — No charting libraries, no CSS frameworks, no ORM
- **API endpoints** — No REST API, no GraphQL; the dashboard is the only interface
- **Notification system** — No email, no Teams alerts for status changes

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** | < 1 second from `dotnet run` to fully rendered page (localhost) |
| **Data reload time** | < 500ms from `data.json` file save to re-rendered dashboard |
| **Build time** | < 10 seconds for `dotnet build` |
| **Memory footprint** | < 100 MB RSS for the running application |

### Rendering Fidelity

| Metric | Target |
|--------|--------|
| **Resolution** | Exactly 1920×1080 pixels, no scrollbars |
| **Color accuracy** | All hex color values match the design spec within ±0 tolerance |
| **Font rendering** | Segoe UI at specified sizes; consistent across Edge and Chrome on Windows |
| **SVG precision** | Milestone positions accurate to ±2 pixels of the date-to-coordinate calculation |

### Security

- No authentication required
- No sensitive data (PII, credentials) in `data.json`
- Application binds to `localhost` only by default
- No outbound network calls

### Reliability

- Application must not crash on malformed `data.json` — graceful degradation with last-known-good data
- Application must not crash on missing `data.json` — display a clear error message
- No unhandled exceptions in normal operation

### Maintainability

- Total codebase under 10 files (excluding generated/obj/bin)
- No third-party NuGet dependencies beyond the .NET 8 SDK
- `data.json` schema documented in code via C# POCO model with XML comments

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Screenshot parity** | Dashboard screenshot is visually indistinguishable from the `OriginalDesignConcept.html` reference when placed side-by-side | Manual visual comparison by product owner |
| 2 | **Data update cycle time** | < 2 minutes from "edit data.json" to "screenshot in PowerPoint" | Timed walkthrough |
| 3 | **Setup time for new project** | < 5 minutes to clone repo, edit `data.json` with new project data, and produce first screenshot | Timed walkthrough by a new user |
| 4 | **Zero external dependencies** | Application runs with `dotnet run` and zero NuGet packages beyond the SDK | Verify `.csproj` has no `<PackageReference>` entries |
| 5 | **Build success** | `dotnet build` completes with zero errors and zero warnings | CI or local build output |
| 6 | **Fictional data completeness** | Example `data.json` populates all four heatmap rows and all three timeline tracks with realistic data | Visual inspection of rendered dashboard |
| 7 | **Hot-reload functional** | Editing `data.json` updates the dashboard within 1 second without browser refresh | Manual test |

## Constraints & Assumptions

### Technical Constraints

- **.NET 8 SDK required** — The target machine must have .NET 8.0 LTS installed (or use a self-contained publish)
- **Windows-first** — Segoe UI font is a Windows system font; macOS/Linux users will fall back to Arial (acceptable degradation)
- **Single browser target** — Optimized for Microsoft Edge and Google Chrome; Firefox and Safari are not tested
- **Fixed resolution** — The layout is hardcoded to 1920×1080; it will not adapt to other screen sizes
- **No JavaScript** — The application uses Blazor Server rendering exclusively; no JS interop is required or permitted
- **Blazor Server hosting model** — Requires a persistent SignalR connection; this is acceptable for local single-user use but means the page cannot be served as a static file

### Timeline Assumptions

- **Phase 1 (Static Replica):** 1 day — Port HTML/CSS into Blazor, verify visual match
- **Phase 2 (Data-Driven):** 1–2 days — Build data model, service, and dynamic rendering
- **Phase 3 (Polish):** 1 day — Hot-reload, CSS custom properties, tests
- **Total estimated effort:** 2–3 developer-days

### Dependency Assumptions

- The `OriginalDesignConcept.html` file in the ReportingDashboard repository is the canonical design reference and will not change during implementation
- The `data.json` schema defined in this spec is final; any schema changes require a spec update
- The developer has Visual Studio 2022 or VS Code with C# Dev Kit installed
- The developer has Edge or Chrome available for visual verification
- No approval gates or review processes are required — this is an internal productivity tool

### Data Assumptions

- A single `data.json` file supports one project; multiple projects require separate instances of the application
- The heatmap supports 3–6 months of columns without layout overflow
- The timeline supports 2–5 milestone tracks without SVG height overflow
- Work item descriptions in heatmap cells are short (under 40 characters) to prevent cell overflow
- The `currentDate` field in `data.json` controls the "NOW" line position, enabling reproducible screenshots regardless of the actual system date