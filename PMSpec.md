# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard as a Blazor Server (.NET 8) web application that visualizes project milestones, timelines, and monthly execution status (shipped, in-progress, carryover, blockers) in a format optimized for 1920×1080 screenshots destined for PowerPoint decks. The application reads all project data from a local `data.json` configuration file, requires no authentication or cloud infrastructure, and is designed to be run locally via `dotnet run`.

## Business Goals

1. **Provide executives with a single-glance view of project health** — consolidating milestone timelines and monthly execution status into one screen, eliminating the need to navigate multiple tools (ADO, Excel, etc.).
2. **Reduce reporting preparation time** — enable project managers to update a simple JSON file and take a screenshot, replacing manual PowerPoint slide construction.
3. **Standardize project reporting format** — establish a consistent visual template that can be reused across projects and workstreams for executive reviews.
4. **Ensure pixel-perfect screenshot fidelity** — render at a fixed 1920×1080 viewport so that screenshots paste cleanly into 16:9 PowerPoint slides without cropping or scaling artifacts.
5. **Minimize operational overhead** — zero infrastructure cost, zero authentication, zero external dependencies; the entire tool runs locally with `dotnet run`.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Metadata

**As a** project manager, **I want** to see the project title, organization, workstream, current month, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are viewing.

**Visual Reference:** Header section (`.hdr`) in `OriginalDesignConcept.html`

- [ ] The page displays the project title as a bold 24px heading (from `data.json` field `title`).
- [ ] An "→ ADO Backlog" hyperlink appears inline with the title and navigates to the URL specified in `data.json` field `backlogUrl`.
- [ ] A subtitle line displays the organization, workstream, and current month (e.g., "Trusted Platform · Privacy Automation Workstream · April 2026").
- [ ] A legend appears right-aligned in the header showing four icons: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line).
- [ ] The header has a 1px solid `#E0E0E0` bottom border.

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline with milestone markers for each project track, **so that** I can quickly understand the project's key dates and current position relative to plan.

**Visual Reference:** Timeline area (`.tl-area`, SVG section) in `OriginalDesignConcept.html`

- [ ] The timeline section displays below the header with a light gray background (`#FAFAFA`) and a height of approximately 196px.
- [ ] A left sidebar (230px wide) lists each track with its ID (e.g., "M1") and label (e.g., "Chatbot & MS Role"), each in the track's designated color.
- [ ] The main SVG area renders vertical month gridlines with abbreviated month labels (Jan, Feb, Mar, etc.) spanning the configured date range.
- [ ] Each track renders as a colored horizontal line with milestone markers: checkpoints as open circles, PoC milestones as gold (`#F4B400`) diamonds, and production releases as green (`#34A853`) diamonds.
- [ ] A dashed red (`#EA4335`) vertical "NOW" line appears at the position corresponding to the current date (auto-calculated from system date or overridden via `data.json`).
- [ ] Each milestone marker has a text label (date and/or description) positioned above or below the track line to avoid overlap.
- [ ] The timeline section has a 2px solid `#E8E8E8` bottom border.

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a grid showing what was shipped, what is in progress, what carried over, and what is blocked for each month, **so that** I can assess execution velocity and identify risks.

**Visual Reference:** Heatmap section (`.hm-wrap`, `.hm-grid`) in `OriginalDesignConcept.html`

- [ ] The heatmap section fills the remaining vertical space below the timeline.
- [ ] A section title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase, 14px bold, gray (`#888`).
- [ ] The grid uses CSS Grid with columns: 160px row header + N equal-width month columns (default 4 months).
- [ ] The first row is a header row (36px height) with a "Status" corner cell and month name headers.
- [ ] The current month column header is highlighted with a gold background (`#FFF0D0`) and gold text (`#C07700`), with a "← Now" indicator.
- [ ] Four category rows are rendered: Shipped (green), In Progress (blue), Carryover (amber/yellow), Blockers (red).
- [ ] Each row header displays the category name with its emoji icon, styled with the category's background and text colors.
- [ ] Each data cell lists individual work items as 12px text with a colored bullet (6px circle) matching the category color.
- [ ] The current month column has a slightly more saturated background color for each category to draw visual emphasis.
- [ ] Future months with no data display a gray dash ("—").

### US-4: Configure Dashboard via JSON

**As a** project manager, **I want** to edit a single `data.json` file to update all dashboard content (title, milestones, heatmap items), **so that** I can prepare updated reports without modifying code.

- [ ] The application reads `data.json` from the `wwwroot/data/` directory on startup.
- [ ] The JSON schema supports: project metadata (title, org, workstream, month, backlog URL), timeline configuration (date range, tracks with milestones), and heatmap data (four categories × N months of items).
- [ ] If `data.json` is malformed or missing required fields, the application displays a clear error message on the page instead of crashing.
- [ ] Changes to `data.json` are reflected after restarting the application (hot reload via `dotnet watch` is acceptable).

### US-5: Screenshot-Ready Rendering

**As a** project manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrolling, **so that** I can take a browser screenshot and paste it directly into a PowerPoint slide.

- [ ] The `<body>` element is fixed at 1920×1080 with `overflow: hidden`.
- [ ] All content fits within the viewport without scrolling for a typical project with up to 3 timeline tracks and 4 months × 4 categories of heatmap data (up to ~5 items per cell).
- [ ] The page uses the Segoe UI system font (no web font loading required).
- [ ] No default Blazor navigation sidebar, header bar, or "About" link is visible.

### US-6: Run Locally with Zero Configuration

**As a** developer, **I want** to launch the dashboard with `dotnet run` and view it in a browser, **so that** I don't need to configure servers, databases, or authentication.

- [ ] The application starts with `dotnet run` and opens on `http://localhost:5000` or `https://localhost:5001`.
- [ ] No database connection string, API key, or authentication configuration is required.
- [ ] The application includes a sample `data.json` with fictional project data so it renders a complete dashboard out of the box.

## Visual Design Specification

**Design Source File:** `OriginalDesignConcept.html` — Engineers MUST consult this file as the canonical reference for all visual decisions.

### Overall Page Layout

- **Viewport:** Fixed 1920×1080px, no scrolling (`overflow: hidden`).
- **Background:** `#FFFFFF` (white).
- **Font Family:** `'Segoe UI', Arial, sans-serif`.
- **Base Text Color:** `#111`.
- **Layout Direction:** Vertical flex column (`display: flex; flex-direction: column`).
- **Three major sections stack vertically:**
  1. Header (flex-shrink: 0, ~48px)
  2. Timeline Area (flex-shrink: 0, 196px fixed height)
  3. Heatmap Area (flex: 1, fills remaining space)

### Section 1: Header (`.hdr`)

- **Padding:** 12px top, 44px horizontal, 10px bottom.
- **Border:** 1px solid `#E0E0E0` bottom.
- **Layout:** Flexbox, `align-items: center`, `justify-content: space-between`.
- **Left side:**
  - `<h1>` — 24px, font-weight 700. Contains project title followed by a hyperlink ("→ ADO Backlog") in `#0078D4`.
  - Subtitle `<div class="sub">` — 12px, color `#888`, margin-top 2px. Format: "{organization} · {workstream} · {currentMonth}".
- **Right side (Legend):**
  - Four inline legend items with 22px gap, each 12px font-size:
    - **PoC Milestone:** 12×12px gold (`#F4B400`) square rotated 45° (diamond shape).
    - **Production Release:** 12×12px green (`#34A853`) square rotated 45° (diamond shape).
    - **Checkpoint:** 8×8px gray (`#999`) circle.
    - **Now Indicator:** 2×14px red (`#EA4335`) vertical bar.

### Section 2: Timeline Area (`.tl-area`)

- **Height:** 196px fixed.
- **Background:** `#FAFAFA`.
- **Padding:** 6px top, 44px horizontal.
- **Border:** 2px solid `#E8E8E8` bottom.
- **Layout:** Flexbox, `align-items: stretch`.

**Left Sidebar (Track Labels):**
- Width: 230px, flex-shrink 0.
- Border-right: 1px solid `#E0E0E0`.
- Padding: 16px vertical, 12px right.
- Each track label: 12px, font-weight 600, line-height 1.4.
  - Track ID (e.g., "M1") in the track's designated color.
  - Track description below in font-weight 400, color `#444`.
- Track colors from reference: M1 = `#0078D4` (blue), M2 = `#00897B` (teal), M3 = `#546E7A` (blue-gray).

**Right SVG Area (`.tl-svg-box`):**
- Flex: 1, padding-left 12px, padding-top 6px.
- SVG element: width 1560px, height 185px, `overflow: visible`.
- **Month Gridlines:** Vertical lines at equal intervals, stroke `#bbb`, opacity 0.4, stroke-width 1. Month labels at x+5, y=14, fill `#666`, 11px, font-weight 600.
- **Track Lines:** Horizontal lines spanning full width, stroke = track color, stroke-width 3. Each track is vertically spaced (y ≈ 42, 98, 154 for three tracks).
- **Milestone Markers:**
  - *Checkpoint (circle):* `<circle>` with white fill, track-color stroke (2.5px width), radius 5–7px.
  - *PoC Milestone (diamond):* `<polygon>` forming a diamond (4 points), fill `#F4B400`, with drop shadow filter.
  - *Production Release (diamond):* `<polygon>` forming a diamond, fill `#34A853`, with drop shadow filter.
  - *Small checkpoint dots:* `<circle>` radius 4px, fill `#999` (for minor checkpoints).
- **NOW Line:** Dashed vertical line, stroke `#EA4335`, stroke-width 2, dash pattern `5,3`. "NOW" text label in `#EA4335`, 10px, font-weight 700.
- **Milestone Labels:** `<text>` elements, 10px, fill `#666`, `text-anchor: middle`, positioned above or below the track line.
- **Drop Shadow Filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`.

### Section 3: Heatmap (`.hm-wrap`)

- **Flex:** 1 (fills remaining space).
- **Padding:** 10px top/bottom, 44px horizontal.

**Title Bar (`.hm-title`):**
- 14px, font-weight 700, color `#888`, letter-spacing 0.5px, text-transform uppercase, margin-bottom 8px.

**Grid (`.hm-grid`):**
- CSS Grid: `grid-template-columns: 160px repeat(4, 1fr)`.
- `grid-template-rows: 36px repeat(4, 1fr)`.
- Border: 1px solid `#E0E0E0`.

**Corner Cell (`.hm-corner`):**
- Background `#F5F5F5`, 11px bold, color `#999`, uppercase, centered.
- Border-right: 1px solid `#E0E0E0`, border-bottom: 2px solid `#CCC`.

**Column Headers (`.hm-col-hdr`):**
- Background `#F5F5F5`, 16px bold, centered.
- Border-right: 1px solid `#E0E0E0`, border-bottom: 2px solid `#CCC`.
- **Current month highlight (`.apr-hdr`):** Background `#FFF0D0`, color `#C07700`.

**Row Headers (`.hm-row-hdr`):**
- 11px bold, uppercase, letter-spacing 0.7px, padding 0 12px.
- Border-right: 2px solid `#CCC`, border-bottom: 1px solid `#E0E0E0`.

**Category Color Palette:**

| Category | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Bullet Color |
|----------|--------------|-----------------|---------|----------------------|-------------|
| **Shipped** | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| **In Progress** | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| **Carryover** | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| **Blockers** | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

**Data Cells (`.hm-cell`):**
- Padding: 8px 12px, border-right: 1px solid `#E0E0E0`, border-bottom: 1px solid `#E0E0E0`, overflow hidden.
- **Item text (`.it`):** 12px, color `#333`, padding 2px 0 2px 12px, line-height 1.35.
- **Item bullet (`.it::before`):** 6×6px circle, positioned at left:0, top:7px, colored per category.
- **Empty cells:** Display a gray dash `—` in color `#AAA`.

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The full dashboard renders within 2 seconds showing the header, timeline, and heatmap populated from `data.json`. No loading spinner is needed given the simplicity of the data source. The page fits entirely within a 1920×1080 viewport with no scrollbars.

**Scenario 2: User Views Project Header**
User sees the project title ("Project Phoenix Release Roadmap") in bold at the top-left, with a clickable "→ ADO Backlog" link beside it. Below the title, a gray subtitle shows "Contoso Engineering · Platform Modernization · April 2026". On the right side of the header, four legend items explain the timeline marker types.

**Scenario 3: User Reads the Milestone Timeline**
User scans left-to-right across the timeline. Three horizontal track lines are visible, each in a distinct color. The user sees diamond markers at key milestone dates and a dashed red "NOW" vertical line indicating today's date. Date labels appear above or below each marker for context.

**Scenario 4: User Hovers Over a Milestone Diamond**
In the MVP, milestone markers are static SVG elements with no hover tooltip. Future enhancement: add a `<title>` element inside each SVG shape to display a native browser tooltip with milestone details on hover.

**Scenario 5: User Scans the Heatmap for Current Month**
The current month column (e.g., "April") is visually highlighted with a gold/amber header (`#FFF0D0`) and "← Now" indicator. Each category row in the current month column has a slightly more saturated background, drawing the executive's eye to the most relevant data.

**Scenario 6: User Identifies Blockers**
The bottom row of the heatmap is red-themed. If any items appear in the Blockers row for the current month, they are rendered with red bullets on a light red background (`#FFE4E4`), making blocked items immediately visible to executives.

**Scenario 7: User Clicks the ADO Backlog Link**
Clicking "→ ADO Backlog" in the header opens the configured Azure DevOps backlog URL in a new browser tab (or the same tab, depending on target attribute). This is a standard `<a href>` link with no special behavior.

**Scenario 8: Empty Month Cells**
For future months with no data, cells display a gray dash ("—") in `#AAA` text. This prevents empty cells from looking broken and signals that no items are planned yet.

**Scenario 9: Data Error on Load**
If `data.json` is missing, unreadable, or contains invalid JSON, the page displays a centered error message (e.g., "Unable to load dashboard data. Please check data.json.") instead of rendering a broken or empty dashboard. The error is visible and actionable.

**Scenario 10: User Takes a Screenshot**
User presses Win+Shift+S (or uses browser DevTools) to capture the page. The fixed 1920×1080 viewport ensures the screenshot captures the complete dashboard without scrollbars, browser chrome, or overflow. The screenshot is directly usable in a 16:9 PowerPoint slide.

**Scenario 11: User Updates data.json and Refreshes**
User edits `data.json` in VS Code to add a new shipped item for April. With `dotnet watch` running, the user refreshes the browser (or the page auto-reloads) and sees the updated item appear in the Shipped/April cell of the heatmap.

## Scope

### In Scope

- Single-page Blazor Server (.NET 8) web application
- Header component with project title, subtitle, ADO backlog link, and legend
- SVG milestone timeline with up to 3 tracks, multiple milestone types (checkpoint, PoC, production), and a "NOW" indicator
- CSS Grid heatmap with 4 category rows (Shipped, In Progress, Carryover, Blockers) × configurable month columns
- All data driven from a single `data.json` file
- Fixed 1920×1080 viewport optimized for screenshots
- Sample `data.json` with fictional project data included out of the box
- Full CSS port from `OriginalDesignConcept.html` design template
- Color-coded category styling matching the reference design exactly
- Local-only execution via `dotnet run`
- Current month visual highlighting in the heatmap

### Out of Scope

- **Authentication & authorization** — No login, no roles, no middleware
- **Database** — No SQL Server, SQLite, or any persistent storage beyond the JSON file
- **CRUD UI for editing data** — Users edit `data.json` directly in a text editor
- **Responsive/mobile design** — Fixed viewport only; no breakpoints
- **Multi-user access** — Designed for single-user local use
- **CI/CD pipeline** — No build/deploy automation
- **Docker containerization** — Not needed for local use
- **Cloud hosting** — Runs on developer's local machine only
- **JavaScript interop** — All rendering in pure Razor + CSS + inline SVG
- **Third-party component libraries** (MudBlazor, Radzen, etc.)
- **Third-party charting libraries** (Chart.js, ApexCharts, etc.)
- **Print/PDF export** — Deferred to Phase 2
- **Multi-project support** — Deferred to Phase 3
- **Tooltip interactions on timeline milestones** — Future enhancement
- **Automated testing** — Low priority given app simplicity; deferred

## Non-Functional Requirements

### Performance

- **Page Load Time:** The dashboard must render fully within 2 seconds of browser navigation on a local machine (no network latency).
- **JSON Parse Time:** `data.json` deserialization must complete in under 100ms for files up to 50KB.
- **Memory Usage:** Application should consume less than 100MB of RAM during normal operation.

### Reliability

- **Graceful Error Handling:** Malformed or missing `data.json` must produce a user-visible error message, not an unhandled exception or blank page.
- **Startup Resilience:** Application must start successfully even if `data.json` is temporarily locked by another process (retry once after 1 second).

### Compatibility

- **Browser:** Microsoft Edge (Chromium) is the primary target. Chrome and Firefox should render identically given the use of standard CSS Grid/Flexbox/SVG.
- **OS:** Windows 10/11 (Segoe UI is a system font).
- **Resolution:** Optimized for 1920×1080. Other resolutions are not tested or supported.

### Maintainability

- **Component Isolation:** Each visual section (Header, Timeline, Heatmap) is a separate Razor component for readability and maintainability.
- **CSS Organization:** Global styles in `app.css` match the reference HTML class names verbatim for easy cross-referencing.

### Security

- **No authentication required.** The app runs on `localhost` only.
- **No PII or secrets** are stored in `data.json`.
- **No external API calls** are made by the application.

## Success Metrics

1. **Visual Fidelity:** A screenshot of the running dashboard, when placed side-by-side with the reference `OriginalDesignConcept.html` rendered in Edge, is indistinguishable in layout, color, typography, and spacing (validated by PM visual review).
2. **Time to Update:** A project manager can update `data.json` and produce an updated screenshot in under 5 minutes.
3. **Zero-Config Launch:** A new developer can clone the repo, run `dotnet run`, and see a fully rendered dashboard with sample data in under 2 minutes.
4. **Screenshot Quality:** A 1920×1080 screenshot taken from the dashboard pastes into a 16:9 PowerPoint slide without any cropping, scaling, or aspect ratio distortion.
5. **Data Completeness:** All fields in `data.json` are reflected in the rendered dashboard — no data is silently dropped or ignored.
6. **Build Success:** The project compiles with zero warnings and zero additional NuGet packages beyond the default Blazor Server template.

## Constraints & Assumptions

### Technical Constraints

- **Technology Stack:** Blazor Server on .NET 8 LTS is mandated. No alternative frameworks (React, Angular, plain HTML) are acceptable.
- **No External Dependencies:** Zero NuGet packages beyond the default `Microsoft.NET.Sdk.Web` SDK. Zero JavaScript libraries.
- **Fixed Viewport:** The page MUST render at 1920×1080 with `overflow: hidden`. Responsive design is explicitly prohibited to preserve screenshot fidelity.
- **System Font Only:** Segoe UI (Windows system font). No web fonts or font downloads.

### Timeline Assumptions

- **Phase 1 (MVP):** 1–2 development days to deliver a fully functional, screenshot-ready dashboard.
- **Phase 2 (Polish):** 1 optional day for FileSystemWatcher auto-reload, hover transitions, print stylesheet.
- **Phase 3 (Multi-Project):** Future scope, only if multiple projects need dashboard views.

### Dependency Assumptions

- **.NET 8 SDK** is installed on the developer's machine.
- **Microsoft Edge (Chromium)** is the browser used for screenshots.
- The developer has access to a text editor (VS Code recommended) for editing `data.json`.
- The `OriginalDesignConcept.html` file in the repository is the single source of truth for visual design.

### Data Assumptions

- `data.json` will be updated manually by the PM at most weekly (likely monthly).
- A typical project has 2–4 timeline tracks with 3–8 milestones each.
- A typical heatmap has 4 months × 4 categories with 1–6 items per cell.
- The "NOW" line position will be auto-calculated from `DateTime.Now` unless explicitly overridden in `data.json`.
- The heatmap defaults to 4 month columns but the schema should support a configurable number of months.

### Open Questions (Requiring PM Decision)

| # | Question | Default Assumption |
|---|----------|--------------------|
| 1 | How often will `data.json` be updated? | Monthly — manual editing is sufficient |
| 2 | Should the "NOW" line auto-calculate from system date? | Yes — auto-calculate from `DateTime.Now` |
| 3 | How many months should the heatmap show? | 4 months, configurable via `data.json` |
| 4 | Should the dashboard support multiple projects? | No — single project for MVP |
| 5 | What browser for screenshots? | Microsoft Edge (Chromium) |
| 6 | Is the ADO Backlog link functional? | Yes — standard `<a href>` to the configured URL |