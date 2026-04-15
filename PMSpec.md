# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones on a timeline and displays work item status in a color-coded heatmap grid. The dashboard reads all data from a local `data.json` file, runs as a lightweight Blazor Server application on localhost, and is optimized for taking pixel-perfect 1920×1080 screenshots to embed in PowerPoint decks for executive stakeholders. The design follows and improves upon the reference layout in `OriginalDesignConcept.html`.

## Business Goals

1. **Provide executives with at-a-glance project visibility** — A single screenshot should communicate project health, milestone progress, shipped items, in-progress work, carryover debt, and blockers without requiring any explanation.
2. **Eliminate manual slide creation** — Replace the current process of manually building status slides in PowerPoint with a data-driven dashboard that produces screenshot-ready visuals in seconds.
3. **Enable rapid status updates** — Allow project managers to update a single `data.json` file and immediately see the refreshed dashboard, reducing reporting overhead from hours to minutes.
4. **Maintain simplicity and portability** — The tool must run locally with zero cloud dependencies, zero authentication, and zero infrastructure cost, so any team member can use it without IT involvement.
5. **Standardize executive reporting format** — Establish a consistent, professional visual format for project status reporting across teams and workstreams.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project manager, **I want** to see the project title, organizational context, reporting period, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are viewing.

**Visual Reference:** Header section (`.hdr`) in `OriginalDesignConcept.html`

- [ ] The header displays the project title in bold 24px font
- [ ] A subtitle line shows the organization, workstream, and current month (e.g., "Trusted Platform · Privacy Automation Workstream · April 2026")
- [ ] A clickable link to the ADO backlog appears next to the title, styled in Microsoft blue (`#0078D4`)
- [ ] A legend on the right side of the header displays four icon types: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line)
- [ ] All text and layout values are driven from `data.json`

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major project milestones across multiple workstreams, **so that** I can understand the project roadmap and current progress at a glance.

**Visual Reference:** Timeline area (`.tl-area` and inline SVG) in `OriginalDesignConcept.html`

- [ ] The timeline displays N workstream tracks as horizontal lines, each with a unique color
- [ ] A left-side label panel (230px wide) lists each track with its ID (e.g., "M1") and description
- [ ] Month dividers appear as light vertical lines with month labels (Jan–Jun or as configured)
- [ ] A dashed red vertical "NOW" line indicates the current date position
- [ ] PoC milestones render as gold (`#F4B400`) diamond shapes with drop shadows
- [ ] Production milestones render as green (`#34A853`) diamond shapes with drop shadows
- [ ] Checkpoints render as small gray circles (filled `#999` or outlined)
- [ ] Each milestone has a date label positioned above or below the track line to avoid collisions
- [ ] Milestone positions are calculated dynamically from dates in `data.json` using time-to-pixel mapping
- [ ] The timeline supports a configurable date range (start and end dates from `data.json`)

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a grid showing work items organized by status (Shipped, In Progress, Carryover, Blockers) and by month, **so that** I can quickly assess execution velocity and identify problem areas.

**Visual Reference:** Heatmap grid (`.hm-wrap`, `.hm-grid`) in `OriginalDesignConcept.html`

- [ ] The heatmap displays as a CSS Grid with a "Status" column header and N month columns
- [ ] The current month column is visually highlighted with a warm background (`#FFF0D0`) and amber text (`#C07700`)
- [ ] Four status rows appear: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each cell lists work items as bulleted text entries with colored dot indicators matching the row theme
- [ ] Empty cells for future months display a gray dash ("–")
- [ ] Row headers use uppercase text, bold weight, and the row's theme color
- [ ] All items, month names, and the highlighted column are driven from `data.json`
- [ ] A section title "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" appears above the grid

### US-4: Load Dashboard Data from JSON

**As a** project manager, **I want** the dashboard to read all its data from a `data.json` configuration file, **so that** I can update project status by editing a single file without touching any code.

- [ ] The application reads `data.json` from the `wwwroot` directory (or a configurable path in `appsettings.json`)
- [ ] The JSON file contains project metadata, timeline tracks, milestones, and heatmap data
- [ ] The application deserializes the JSON into strongly-typed C# record models using `System.Text.Json`
- [ ] If `data.json` is missing or malformed, the dashboard displays a clear error message instead of crashing
- [ ] Changes to `data.json` are reflected on the next page load (no application restart required during development with `dotnet watch`)

### US-5: Screenshot-Ready Layout

**As a** project manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, **so that** I can capture a full-page browser screenshot and paste it directly into a PowerPoint slide.

- [ ] The `<body>` element is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] All three sections (header, timeline, heatmap) fit within the 1080px vertical space without scrolling
- [ ] The layout uses the Segoe UI system font for consistency with Microsoft corporate branding
- [ ] No browser chrome, scrollbars, or UI artifacts appear in the screenshot area
- [ ] The visual output matches the reference design in `OriginalDesignConcept.html` with refinements for improved spacing, shadows, and typography

### US-6: Run Dashboard Locally

**As a** developer, **I want** to run the dashboard with a single `dotnet run` command, **so that** I can view and iterate on the dashboard without any setup beyond the .NET 8 SDK.

- [ ] The application starts on `http://localhost:5000` (or a configured port) with no additional dependencies
- [ ] No database, cloud service, or authentication is required
- [ ] `dotnet watch` enables hot reload for CSS and Razor component changes during development
- [ ] The application can be published as a self-contained executable via `dotnet publish -c Release -r win-x64 --self-contained`

### US-7: Use Example Fictional Data

**As a** new user, **I want** the repository to include a `data.json` file pre-populated with fictional project data, **so that** I can see a working dashboard immediately and understand the expected data format.

- [ ] The included `data.json` contains a fictional project with 3 timeline tracks, 6+ milestones, and heatmap data across 4 months
- [ ] The example data demonstrates all milestone types (PoC, Production, Checkpoint)
- [ ] The example data includes items in all four heatmap categories (Shipped, In Progress, Carryover, Blockers)
- [ ] The example data is realistic enough to serve as a template for real projects

## Visual Design Specification

> **Canonical Design Reference:** `OriginalDesignConcept.html` in the ReportingDashboard repository. Engineers MUST consult this file and the rendered screenshot (`docs/design-screenshots/OriginalDesignConcept.png`) before writing any code.

### Overall Page Layout

- **Viewport:** Fixed `1920px × 1080px`, `overflow: hidden`, white background (`#FFFFFF`)
- **Font Family:** `'Segoe UI', Arial, sans-serif`
- **Base Text Color:** `#111`
- **Layout Model:** Vertical flex column (`display: flex; flex-direction: column`) filling the full viewport
- **Three stacked sections:** Header → Timeline → Heatmap (heatmap uses `flex: 1` to fill remaining space)

### Section 1: Header (`.hdr`)

- **Height:** Auto (content-driven, approximately 50px)
- **Padding:** `12px 44px 10px`
- **Border:** `1px solid #E0E0E0` bottom
- **Layout:** Flexbox row, `justify-content: space-between; align-items: center`
- **Left side:**
  - Title: `<h1>` at `24px`, `font-weight: 700`, color `#111`
  - Backlog link: Inline `<a>` styled in `#0078D4`, no underline
  - Subtitle: `12px`, color `#888`, `margin-top: 2px`
- **Right side — Legend row:**
  - Flexbox row with `gap: 22px`
  - Each legend item: `12px` font, flex row with `gap: 6px`
  - PoC Milestone icon: `12px × 12px` square, `background: #F4B400`, `transform: rotate(45deg)` (renders as diamond)
  - Production Release icon: `12px × 12px` square, `background: #34A853`, `transform: rotate(45deg)`
  - Checkpoint icon: `8px × 8px` circle, `background: #999`
  - Now indicator: `2px × 14px` vertical bar, `background: #EA4335`

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed `196px`
- **Background:** `#FAFAFA`
- **Padding:** `6px 44px 0`
- **Border:** `2px solid #E8E8E8` bottom
- **Layout:** Flexbox row, `align-items: stretch`

#### Track Label Panel (left side)

- **Width:** Fixed `230px`, `flex-shrink: 0`
- **Border:** `1px solid #E0E0E0` right
- **Padding:** `16px 12px 16px 0`
- **Layout:** Flex column, `justify-content: space-around`
- **Track labels:**
  - Track ID (e.g., "M1"): `12px`, `font-weight: 600`, track color (M1: `#0078D4`, M2: `#00897B`, M3: `#546E7A`)
  - Track description: `font-weight: 400`, color `#444`

#### SVG Timeline (right side, `.tl-svg-box`)

- **Dimensions:** `width: 1560px`, `height: 185px`, `overflow: visible`
- **Padding:** `padding-left: 12px; padding-top: 6px`
- **Month grid lines:** Vertical `<line>` elements at equal intervals (260px apart), stroke `#bbb` at `0.4` opacity
- **Month labels:** `<text>` at y=14, `11px`, `font-weight: 600`, fill `#666`
- **"NOW" line:** Dashed vertical `<line>`, stroke `#EA4335`, `stroke-width: 2`, `stroke-dasharray: 5,3`, with "NOW" label in `10px` bold red
- **Track lines:** Horizontal `<line>` spanning full width, `stroke-width: 3`, using track color
  - Track 1 at `y=42` (color `#0078D4`)
  - Track 2 at `y=98` (color `#00897B`)
  - Track 3 at `y=154` (color `#546E7A`)
- **Milestone shapes:**
  - **PoC diamond:** `<polygon>` forming a diamond (11px radius), fill `#F4B400`, with drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`)
  - **Production diamond:** Same shape, fill `#34A853`, same shadow
  - **Checkpoint (large):** `<circle>` r=7, white fill, track-color stroke at `2.5px` width
  - **Checkpoint (small):** `<circle>` r=4–5, fill `#999` or white with `#888` stroke
- **Date labels:** `<text>` at `10px`, fill `#666`, `text-anchor: middle`, positioned above or below the track line

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Flex:** `flex: 1; min-height: 0` (fills remaining viewport)
- **Padding:** `10px 44px 10px`
- **Section title (`.hm-title`):** `14px`, `font-weight: 700`, color `#888`, `letter-spacing: 0.5px`, uppercase, `margin-bottom: 8px`

#### Grid Structure (`.hm-grid`)

- **Display:** `grid`
- **Columns:** `grid-template-columns: 160px repeat(4, 1fr)`
- **Rows:** `grid-template-rows: 36px repeat(4, 1fr)`
- **Border:** `1px solid #E0E0E0`

#### Column Headers

- **Corner cell (`.hm-corner`):** Background `#F5F5F5`, `11px` bold uppercase, color `#999`, `border-bottom: 2px solid #CCC`
- **Month headers (`.hm-col-hdr`):** Background `#F5F5F5`, `16px` bold, `border-bottom: 2px solid #CCC`
- **Current month highlight (`.apr-hdr`):** Background `#FFF0D0`, color `#C07700`

#### Row Themes (4 categories)

| Category | Row Header Class | Header BG | Header Text | Cell BG | Highlight Cell BG | Bullet Color |
|----------|-----------------|-----------|-------------|---------|-------------------|--------------|
| Shipped | `.ship-hdr` | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `.prog-hdr` | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `.carry-hdr` | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `.block-hdr` | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers (`.hm-row-hdr`)

- `11px`, bold, uppercase, `letter-spacing: 0.7px`
- `border-right: 2px solid #CCC`
- Emoji prefix per row: ✅ Shipped, 🔵 In Progress, 🔁 Carryover, 🚫 Blockers

#### Data Cells (`.hm-cell`)

- **Padding:** `8px 12px`
- **Borders:** `1px solid #E0E0E0` right and bottom
- **Work items (`.it`):** `12px`, color `#333`, `padding: 2px 0 2px 12px`, `line-height: 1.35`
- **Bullet dot:** `::before` pseudo-element, `6px × 6px` circle, positioned at `left: 0; top: 7px`, color matches row theme
- **Empty future cells:** Display `–` in `color: #AAA`

### Complete Color Palette

| Token | Hex | Usage |
|-------|-----|-------|
| White | `#FFFFFF` | Page background |
| Near-black | `#111` | Base text |
| Microsoft Blue | `#0078D4` | Links, In Progress bullets, Track 1 |
| Teal | `#00897B` | Track 2 |
| Blue-gray | `#546E7A` | Track 3 |
| Gray-light | `#888` | Subtitles, section titles |
| Gray-medium | `#999` | Checkpoints, corner cell text |
| Gray-dark | `#666` | SVG labels, month labels |
| Body text | `#333` | Heatmap item text |
| Track label secondary | `#444` | Track description text |
| Border light | `#E0E0E0` | Cell borders, header bottom |
| Border heavy | `#CCC` | Row header right border, column header bottom |
| Timeline BG | `#FAFAFA` | Timeline area background |
| Header BG | `#F5F5F5` | Column headers, corner cell |
| Highlight month BG | `#FFF0D0` | Current month column header |
| Highlight month text | `#C07700` | Current month column header text |
| Gold/Amber | `#F4B400` | PoC diamonds, Carryover bullets |
| Green | `#34A853` | Production diamonds, Shipped bullets |
| Red | `#EA4335` | NOW line, Blocker bullets |
| Shipped row header BG | `#E8F5E9` | |
| Shipped row header text | `#1B7A28` | |
| Shipped cell BG | `#F0FBF0` | |
| Shipped highlight cell BG | `#D8F2DA` | |
| In Progress row header BG | `#E3F2FD` | |
| In Progress row header text | `#1565C0` | |
| In Progress cell BG | `#EEF4FE` | |
| In Progress highlight cell BG | `#DAE8FB` | |
| Carryover row header BG | `#FFF8E1` | |
| Carryover row header text | `#B45309` | |
| Carryover cell BG | `#FFFDE7` | |
| Carryover highlight cell BG | `#FFF0B0` | |
| Blockers row header BG | `#FEF2F2` | |
| Blockers row header text | `#991B1B` | |
| Blockers cell BG | `#FFF5F5` | |
| Blockers highlight cell BG | `#FFE4E4` | |

### Typography Scale

| Element | Size | Weight | Transform | Spacing |
|---------|------|--------|-----------|---------|
| Page title | 24px | 700 | None | Normal |
| Subtitle | 12px | 400 | None | Normal |
| Legend items | 12px | 400 | None | Normal |
| Track ID | 12px | 600 | None | Normal |
| Track description | 12px | 400 | None | Normal |
| SVG month labels | 11px | 600 | None | Normal |
| SVG date labels | 10px | 400 | None | Normal |
| SVG "NOW" label | 10px | 700 | None | Normal |
| Heatmap section title | 14px | 700 | Uppercase | 0.5px |
| Column headers | 16px | 700 | None | Normal |
| Corner cell | 11px | 700 | Uppercase | Normal |
| Row headers | 11px | 700 | Uppercase | 0.7px |
| Cell items | 12px | 400 | None | Normal |

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Full Dashboard Render**
The user navigates to `http://localhost:5000`. The application reads `data.json`, deserializes it into the data model, and renders the complete dashboard within a single 1920×1080 viewport. The header, timeline, and heatmap all appear simultaneously with no loading spinner or progressive rendering. The page is screenshot-ready on first paint.

**Scenario 2: User Views the Project Header**
The user sees the project title in large bold text (e.g., "Privacy Automation Release Roadmap") with a blue hyperlink to the ADO backlog. Below it, a subtitle shows the organizational hierarchy and current month. On the right side of the header, a legend row displays four icon types with labels, allowing the user to decode the timeline symbols.

**Scenario 3: User Reads the Milestone Timeline**
The user looks at the timeline area and sees 3 horizontal track lines in distinct colors. Each track is labeled on the left (e.g., "M1 — Chatbot & MS Role"). Along each track, diamond shapes mark PoC and Production milestones, and circles mark checkpoints. A dashed red vertical line labeled "NOW" indicates the current date. The user can visually compare where "NOW" falls relative to upcoming milestones to assess schedule risk.

**Scenario 4: User Hovers Over a Milestone Diamond**
The milestone diamond displays a subtle drop shadow (already rendered via SVG filter). Date labels are positioned near each milestone. In this initial version, there is no interactive tooltip — the label is always visible. *(Future enhancement: Add hover tooltips with milestone details.)*

**Scenario 5: User Scans the Heatmap for Current Month Status**
The user's eye is drawn to the highlighted column (current month) which has a warm amber header background (`#FFF0D0`). Each row in the highlighted column shows the current month's items with a slightly deeper background tint than adjacent months. The user scans vertically: green (shipped) items at top, blue (in progress) in the middle, amber (carryover) below, and red (blockers) at the bottom.

**Scenario 6: User Identifies Blockers**
The user scans the Blockers row (red-tinted) across all months. Items in this row stand out with red bullet dots and a light red cell background. The user can quickly count blockers per month and see if the trend is improving or worsening.

**Scenario 7: User Captures a Screenshot for PowerPoint**
The user opens Chrome DevTools (Ctrl+Shift+P → "Capture full size screenshot") or uses a screenshot tool. The fixed 1920×1080 layout produces a pixel-perfect image that matches PowerPoint's standard widescreen slide dimensions. No cropping or resizing is needed.

**Scenario 8: User Updates Project Data**
The project manager opens `data.json` in a text editor, adds a new work item to the "In Progress" category for the current month, and saves the file. If running with `dotnet watch`, the dashboard refreshes automatically. Otherwise, the user refreshes the browser to see the updated data.

**Scenario 9: User Clicks the ADO Backlog Link**
The user clicks the backlog link in the header. The browser opens the configured URL (from `data.json`) in a new tab, navigating to the Azure DevOps backlog for the project.

**Scenario 10: Empty State — No Items in a Heatmap Cell**
When a heatmap cell has no work items (e.g., future months), it displays a single gray dash ("–") in `color: #AAA` to indicate emptiness rather than leaving the cell blank.

**Scenario 11: Error State — Missing or Malformed data.json**
If `data.json` is missing or contains invalid JSON, the dashboard displays a centered error message (e.g., "Unable to load dashboard data. Please check data.json.") instead of rendering a broken layout or throwing an unhandled exception.

**Scenario 12: Error State — Missing Required Fields in data.json**
If `data.json` exists but is missing required fields (e.g., no `project.title` or no `heatmap.rows`), the dashboard renders with placeholder text (e.g., "[No title]") or omits the section gracefully, rather than crashing.

**Scenario 13: Timeline with Overlapping Milestones**
When two milestones on the same track have dates close enough that their labels would overlap, the rendering engine offsets one label above the track line and one below, preventing text collision.

**Scenario 14: Responsive Behavior — Not Applicable**
The dashboard is explicitly designed for a fixed 1920×1080 viewport. On smaller screens, the user will see the top-left portion of the dashboard with browser scrollbars. No responsive layout adjustments are made. This is by design.

## Scope

### In Scope

- Single-page Blazor Server dashboard matching the `OriginalDesignConcept.html` design
- Header component with project title, subtitle, backlog link, and milestone legend
- SVG timeline component with dynamic date-to-pixel mapping, multiple tracks, and milestone rendering
- CSS Grid heatmap component with 4 status rows × N month columns, color-coded by category
- `data.json` file as the sole data source, read at runtime via `System.Text.Json`
- C# record-based data model (`DashboardData`, `ProjectInfo`, `TimelineTrack`, `Milestone`, `HeatmapData`, `HeatmapRow`)
- `DashboardDataService` singleton for JSON loading and caching
- Fixed 1920×1080 viewport optimized for browser screenshots
- Pre-populated `data.json` with fictional example project data
- `dashboard.css` ported directly from the HTML design reference
- Visual polish improvements over the original design (better spacing, subtle shadows, cleaner typography)
- Basic SVG label collision avoidance for overlapping milestones
- Unit tests for model deserialization and date-to-pixel calculations
- Component tests (bUnit) for Blazor component rendering

### Out of Scope

- **Authentication / Authorization** — No login, no roles, no tokens, no middleware
- **Database** — No SQLite, no SQL Server, no LiteDB; `data.json` is the only data store
- **REST API / GraphQL** — No API endpoints; the dashboard is the only surface
- **Multi-project support** — One `data.json` = one project. No project switching UI
- **Responsive / mobile layout** — The design is fixed at 1920×1080; no breakpoints
- **Real-time data sync** — No ADO integration, no live data feeds, no webhooks
- **PDF export / print button** — Screenshots are the export mechanism (future enhancement)
- **Interactive tooltips or click-through** — The dashboard is read-only and static (no drill-down)
- **Dark mode / theme switching** — Single light theme matching the design reference
- **Containerization (Docker)** — Unnecessary for a local-only application
- **CI/CD pipeline** — Not needed for this scope; manual `dotnet run` or `dotnet publish`
- **Accessibility (WCAG)** — The primary output is a screenshot, not an interactive web app consumed by screen readers
- **Localization / i18n** — English only
- **Historical data / versioning** — Data history is managed via Git commits of `data.json`, not by the application
- **Third-party UI libraries** (MudBlazor, Radzen, etc.) — Raw HTML/CSS only
- **Third-party charting libraries** — Hand-crafted SVG only

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Initial page load (cold start) | < 2 seconds including .NET startup |
| Page render after data load | < 100ms |
| `data.json` deserialization | < 50ms for files up to 100KB |
| SVG rendering (50+ elements) | < 50ms |
| Memory usage (idle) | < 100MB |

### Security

- No authentication or authorization required
- No PII or secrets stored in `data.json`
- Application binds to `localhost` only by default (not `0.0.0.0`)
- No external network calls — fully offline-capable after `dotnet restore`

### Reliability

- The application must handle missing or malformed `data.json` gracefully (error message, not crash)
- The application must handle missing optional fields in `data.json` without throwing exceptions
- No uptime SLA — this is a local development tool, not a production service

### Maintainability

- Single-project solution with < 10 files
- Zero third-party NuGet dependencies in the main project
- CSS copied directly from the design reference for 1:1 traceability
- C# record types serve as both the data schema and documentation

### Compatibility

- .NET 8.0 SDK required
- Tested on Windows (primary) — expected to work on macOS/Linux without modification
- Browser: Chrome (primary, for DevTools screenshot), Edge, Firefox
- Output resolution: 1920×1080 (16:9 widescreen, matches PowerPoint default)

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Screenshot fidelity** | Dashboard screenshot is visually indistinguishable from `OriginalDesignConcept.html` reference at 1920×1080 | Side-by-side visual comparison |
| 2 | **Data-driven rendering** | 100% of displayed text, milestones, and heatmap items are sourced from `data.json` (zero hardcoded content) | Code review — no string literals in Razor components |
| 3 | **Setup time** | A new user can clone the repo, run `dotnet run`, and see the dashboard in < 2 minutes | Timed walkthrough with a fresh machine |
| 4 | **Update cycle time** | A project manager can edit `data.json` and see updated dashboard in < 30 seconds | Timed test with `dotnet watch` |
| 5 | **Build success** | `dotnet build` completes with zero errors and zero warnings | CI build output |
| 6 | **Test pass rate** | All unit and component tests pass | `dotnet test` output — 100% pass rate |
| 7 | **Stakeholder approval** | The dashboard screenshot is accepted by at least one executive stakeholder as "ready for use in status decks" | Stakeholder sign-off |

## Constraints & Assumptions

### Technical Constraints

- **Runtime:** .NET 8.0 SDK must be installed on the developer's machine
- **Browser:** A modern Chromium-based browser is required for pixel-perfect screenshots (Chrome DevTools "Capture full size screenshot")
- **Resolution:** The layout is hardcoded to 1920×1080; it will not adapt to other resolutions
- **Font:** Segoe UI must be available as a system font (standard on Windows; may need installation on macOS/Linux)
- **No JavaScript:** The dashboard uses Blazor Server-side rendering only; no custom JavaScript is permitted
- **Single project:** The solution must remain a single `.csproj` (plus test project) — no microservices, no class libraries

### Timeline Assumptions

- **Phase 1 (Static Layout):** 1 day — Port HTML/CSS into Blazor components with hardcoded data
- **Phase 2 (Data-Driven):** 1–2 days — Build data models, JSON loading, and dynamic rendering
- **Phase 3 (Polish):** 1 day — Improve typography, spacing, shadows, and add label collision avoidance
- **Total estimated effort:** 3–4 developer-days

### Dependency Assumptions

- The `OriginalDesignConcept.html` design file in the ReportingDashboard repository is the canonical design reference and will not change during implementation
- The `data.json` schema defined in this spec (via C# record types) is final; schema changes after implementation begins will require rework
- The application will only be used by the project manager who runs it locally — no multi-user or network sharing scenarios
- PowerPoint slides use the standard 16:9 widescreen format (13.33" × 7.5"), which maps to 1920×1080 pixel screenshots at 144 DPI
- The fictional example data in `data.json` does not need to represent any real project — it serves only to demonstrate the dashboard's capabilities

### Open Questions (Pending Stakeholder Input)

| # | Question | Default Assumption |
|---|----------|--------------------|
| 1 | How many months should the heatmap display? | 4 months (configurable via `data.json`) |
| 2 | How many timeline tracks are supported? | N tracks (dynamic from `data.json`), defaulting to 3 in the example |
| 3 | Should the "NOW" line use the system date or a date from `data.json`? | System date (`DateTime.Now`), with optional override in `data.json` |
| 4 | Should the heatmap column count match the timeline date range? | No — they are independent; heatmap columns are explicitly listed in `data.json` |
| 5 | Should the backlog link open in a new tab? | Yes (`target="_blank"`) |
| 6 | Should `@media print` styles be included? | No — deferred to a future enhancement |