# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, delivery status, and monthly execution progress. The dashboard reads all data from a local `data.json` configuration file and renders a pixel-perfect 1920×1080 view optimized for screenshots that will be pasted directly into PowerPoint decks for executive stakeholders. Built with .NET 8 Blazor Server, it requires zero authentication, zero external dependencies, and zero cloud infrastructure.

## Business Goals

1. **Reduce executive reporting preparation time** — Replace manual PowerPoint slide construction with a live, data-driven dashboard that can be screenshotted in seconds whenever data changes.
2. **Standardize project status communication** — Provide a consistent visual format (timeline + heatmap) that executives can instantly parse across different projects and workstreams.
3. **Enable self-service status updates** — Allow project managers to update a single `data.json` file (no code changes) to reflect current milestones, shipped items, in-progress work, carryovers, and blockers.
4. **Eliminate dependency on design tools** — Remove the need for Visio, Lucidchart, or manual PowerPoint diagram editing by auto-generating timeline and heatmap visuals from structured data.
5. **Achieve zero-cost, zero-ops deployment** — Run entirely on a developer's local machine via `dotnet run` with no cloud hosting, licensing, or infrastructure costs.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Metadata

**As a** project manager, **I want** to see the project title, subtitle (org/workstream/date), and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are viewing.

*Visual Reference: Header section (`.hdr`) in `OriginalDesignConcept.html`*

- [ ] The page displays the `title` from `data.json` as a bold 24px heading.
- [ ] The `subtitle` renders below the title in 12px gray text.
- [ ] The `backlogUrl` renders as a clickable blue link (`#0078D4`) adjacent to the title.
- [ ] A legend bar appears on the right side of the header showing four marker types: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line).

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major milestones across multiple workstream tracks, **so that** I can understand the project's progression from start through planned completion at a glance.

*Visual Reference: Timeline area (`.tl-area`) and SVG section in `OriginalDesignConcept.html`*

- [ ] The left panel (230px wide) lists each track with its identifier (e.g., "M1"), name, and color-coded label.
- [ ] The SVG area renders a horizontal timeline spanning the `startDate` to `endDate` from `data.json`.
- [ ] Vertical month grid lines appear at each month boundary with month abbreviation labels (Jan, Feb, Mar, etc.).
- [ ] Each track renders as a colored horizontal line at a distinct Y position.
- [ ] Checkpoint milestones render as open circles (white fill, colored stroke) on their track line.
- [ ] PoC milestones render as gold (`#F4B400`) diamond shapes with a drop shadow.
- [ ] Production milestones render as green (`#34A853`) diamond shapes with a drop shadow.
- [ ] Each milestone displays a date label positioned above or below the marker to avoid overlap.
- [ ] A dashed red (`#EA4335`) vertical "NOW" line appears at the position corresponding to the current date (or `nowDate` from JSON if specified).
- [ ] The "NOW" label appears in bold red text adjacent to the line.

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what is blocked — organized by month, **so that** I can quickly assess execution health and identify problem areas.

*Visual Reference: Heatmap section (`.hm-wrap`, `.hm-grid`) in `OriginalDesignConcept.html`*

- [ ] A section title "Monthly Execution Heatmap" appears in uppercase gray text above the grid.
- [ ] The grid has a header row with month names; the current month column is highlighted with a warm background (`#FFF0D0`) and amber text (`#C07700`).
- [ ] Four status rows appear: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red).
- [ ] Each row header displays the status name in uppercase with the corresponding status color and icon.
- [ ] Each cell contains bullet-pointed work items with a small colored dot prefix matching the row's status color.
- [ ] Cells in the current month column have a slightly deeper background tint than other months.
- [ ] Empty cells (future months with no data) display a gray dash character.
- [ ] The grid fills the remaining vertical space below the timeline, ensuring the full page fits within 1080px height.

### US-4: Configure Dashboard via JSON

**As a** project manager, **I want** to edit a single `data.json` file to update all dashboard content (title, milestones, status items), **so that** I can refresh the reporting view without touching any code.

- [ ] The application reads `data.json` from the project root directory on startup.
- [ ] Changes to `data.json` are reflected after restarting the application (or optionally via hot-reload with `FileSystemWatcher`).
- [ ] The JSON schema supports: `title`, `subtitle`, `backlogUrl`, `timeline` (with tracks and milestones), and `heatmap` (with months, categories, and items).
- [ ] If `data.json` is missing or malformed, the application displays a clear error message instead of crashing.
- [ ] The `nowDate` field is optional; if omitted, the system uses today's date.
- [ ] The `currentMonth` field is optional; if omitted, the system auto-detects the current month for highlighting.

### US-5: Screenshot-Ready Rendering

**As a** project manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, popups, or overlays, **so that** I can take a browser screenshot and paste it directly into a PowerPoint slide.

- [ ] The page body is fixed at `width: 1920px; height: 1080px; overflow: hidden`.
- [ ] No Blazor reconnection modal or error overlay is visible (hidden via CSS).
- [ ] No scrollbars appear at the target resolution.
- [ ] All text is rendered in Segoe UI (system font on Windows) at sizes readable when projected (minimum 10px for labels, 12px for body text, 16px+ for headers).
- [ ] The white background (`#FFFFFF`) ensures clean paste into slides without transparency artifacts.

### US-6: Run Dashboard Locally

**As a** developer, **I want** to start the dashboard with a single `dotnet run` command, **so that** there is zero deployment overhead.

- [ ] The application starts on `http://localhost:5000` (or a configured port) without requiring HTTPS certificates.
- [ ] No authentication or login is required.
- [ ] No database connection or migration step is needed.
- [ ] No external API calls are made at startup.
- [ ] The dashboard page loads within 2 seconds of navigating to the root URL.

### US-7: View Dashboard with Fictional Demo Data

**As a** stakeholder evaluating this tool, **I want** the default `data.json` to contain realistic fictional project data, **so that** I can immediately see a compelling example without configuring anything.

- [ ] The shipped `data.json` contains a fictional project (e.g., "Project Phoenix — Cloud Migration Platform").
- [ ] The timeline includes 3–5 tracks with varied milestone types spanning a 6-month window.
- [ ] The heatmap includes 4 months of data with 2–4 items per category per month.
- [ ] Shipped items use past-tense action phrases (e.g., "Deployed API gateway to staging").
- [ ] Blocker items include realistic impediments (e.g., "Waiting on security review approval").

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` located in the ReportingDashboard repository. Engineers MUST open this file in a browser and match the rendered output pixel-for-pixel. Additionally, consult `C:/Pics/ReportingDashboardDesign.png` for the target visual appearance.

### Page-Level Layout

- **Viewport:** Fixed 1920px × 1080px, no scrolling, white (`#FFFFFF`) background.
- **Font Family:** `'Segoe UI', Arial, sans-serif` — system font, no web font imports.
- **Base Text Color:** `#111` (near-black).
- **Link Color:** `#0078D4` (Microsoft blue), no underline.
- **Layout Direction:** Vertical flex column (`display: flex; flex-direction: column`).
- **Three major sections stack vertically:** Header → Timeline → Heatmap.

### Section 1: Header (`.hdr`)

- **Padding:** 12px top, 44px horizontal, 10px bottom.
- **Border:** 1px solid `#E0E0E0` on the bottom edge.
- **Layout:** Flexbox, `align-items: center; justify-content: space-between`.
- **Left side:**
  - Title: `<h1>` at 24px, font-weight 700.
  - Subtitle: 12px, color `#888`, margin-top 2px.
- **Right side — Legend bar:**
  - Horizontal flex with 22px gap.
  - Each legend item: 12px text with an inline shape indicator.
  - PoC Milestone: 12×12px gold (`#F4B400`) square rotated 45° to form a diamond.
  - Production Release: 12×12px green (`#34A853`) square rotated 45°.
  - Checkpoint: 8×8px gray (`#999`) circle.
  - Now indicator: 2×14px red (`#EA4335`) vertical bar.

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed 196px. Flex-shrink 0. Background `#FAFAFA`.
- **Border:** 2px solid `#E8E8E8` on the bottom.
- **Padding:** 6px top, 44px horizontal.
- **Layout:** Horizontal flex with two children.

#### Timeline Left Panel (Track Labels)

- **Width:** 230px, flex-shrink 0.
- **Border:** 1px solid `#E0E0E0` on the right.
- **Content:** Vertically spaced track labels, each showing:
  - Track ID (e.g., "M1") in 12px, font-weight 600, colored to match the track.
  - Track name in 12px, font-weight 400, color `#444`.
- **Track Colors (from reference):**
  - Track 1: `#0078D4` (blue)
  - Track 2: `#00897B` (teal)
  - Track 3: `#546E7A` (blue-gray)

#### Timeline SVG Area (`.tl-svg-box`)

- **Flex:** 1 (fills remaining width). Padding-left 12px, padding-top 6px.
- **SVG Dimensions:** Width 1560px, height 185px, `overflow: visible`.
- **Month Grid Lines:** Vertical lines at each month boundary, stroke `#bbb` at 0.4 opacity, 1px width.
- **Month Labels:** 11px, font-weight 600, color `#666`, positioned 5px right of grid line, y=14.
- **Track Lines:** Horizontal lines spanning full width, 3px stroke in track color.
- **Track Y Positions:** Evenly spaced (approximately y=42, y=98, y=154 for 3 tracks).
- **Milestone Markers:**
  - **Checkpoint (circle):** White fill, track-colored stroke (2.5px), radius 5–7px.
  - **PoC (diamond):** Gold fill (`#F4B400`), SVG `<polygon>` with 4 points forming a diamond, drop shadow filter.
  - **Production (diamond):** Green fill (`#34A853`), same diamond shape, drop shadow filter.
  - **Small checkpoint (dot):** Solid `#999` fill, radius 4px, no stroke.
- **Drop Shadow Filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`.
- **Milestone Labels:** 10px, color `#666`, `text-anchor: middle`, positioned above or below the marker.
- **NOW Line:** Dashed vertical line (`stroke-dasharray: 5,3`), color `#EA4335`, 2px width, spans full SVG height. "NOW" label in 10px bold red.

### Section 3: Heatmap (`.hm-wrap`)

- **Flex:** 1 (fills all remaining vertical space). Padding 10px horizontal 44px.
- **Section Title (`.hm-title`):** 14px, font-weight 700, color `#888`, uppercase, letter-spacing 0.5px, margin-bottom 8px.

#### Heatmap Grid (`.hm-grid`)

- **Layout:** CSS Grid.
- **Columns:** `160px repeat(4, 1fr)` — one label column + four month columns.
- **Rows:** `36px repeat(4, 1fr)` — one header row + four status rows.
- **Border:** 1px solid `#E0E0E0` on the outer grid.

#### Grid Header Row

- **Corner Cell (`.hm-corner`):** Background `#F5F5F5`, 11px bold uppercase `#999` text ("STATUS"), borders right 1px `#E0E0E0` and bottom 2px `#CCC`.
- **Month Header Cells (`.hm-col-hdr`):** 16px bold, centered, background `#F5F5F5`, border-right 1px `#E0E0E0`, border-bottom 2px `#CCC`.
- **Current Month Header (`.apr-hdr`):** Background `#FFF0D0`, text color `#C07700`, with "▸ Now" suffix.

#### Status Row Definitions

| Status | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Dot Color |
|--------|--------------|-----------------|---------|----------------------|-----------|
| Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers (`.hm-row-hdr`)

- 11px bold uppercase, letter-spacing 0.7px, padding 0 12px.
- Border-right 2px solid `#CCC`, border-bottom 1px `#E0E0E0`.
- Includes an emoji/icon prefix (✅ Shipped, 🔵 In Progress, etc.).

#### Data Cells (`.hm-cell`)

- Padding 8px 12px. Border-right 1px `#E0E0E0`, border-bottom 1px `#E0E0E0`.
- **Item Bullets (`.it`):** 12px, color `#333`, padding-left 12px, line-height 1.35.
- **Colored Dot (`.it::before`):** 6×6px circle, absolutely positioned left 0, top 7px, filled with the row's status color.

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The dashboard renders the full page in under 2 seconds. The header displays the project title, subtitle, and legend. The timeline shows all tracks with milestone markers. The heatmap grid displays all status categories with work items. No loading spinner, no progressive rendering — the full page appears at once.

**Scenario 2: User Views the Header**
User sees the project title in bold at the top-left. A blue hyperlink labeled with an arrow icon and "ADO Backlog" text appears next to the title, linking to the Azure DevOps backlog URL. The subtitle shows the organization, workstream, and reporting month. On the right, four legend items explain the marker shapes used in the timeline.

**Scenario 3: User Reads the Timeline**
User looks at the timeline section. On the left, track labels (M1, M2, M3) identify each workstream with its color. On the right, horizontal colored lines represent each track's progression. Diamond markers at key dates indicate PoC and Production milestones. Small circles indicate checkpoints. A dashed red vertical "NOW" line shows the current date position. Date labels next to each marker provide exact dates.

**Scenario 4: User Identifies the Current Month in the Heatmap**
User scans the heatmap column headers. The current month column (e.g., "April") stands out with a warm gold background (`#FFF0D0`) and amber text, plus a "▸ Now" indicator. All cells in that column have a deeper tint than adjacent months.

**Scenario 5: User Assesses Execution Health**
User scans the four heatmap rows from top to bottom. Green (Shipped) rows show completed deliverables. Blue (In Progress) shows active work. Amber (Carryover) highlights items that slipped from prior months. Red (Blockers) surfaces impediments. The user can immediately see if the red/amber rows are growing over time, indicating project health issues.

**Scenario 6: User Sees an Empty Future Month**
For months beyond the current month (e.g., May, June), cells that have no data display a single gray dash ("–") to indicate intentional emptiness rather than a data loading error.

**Scenario 7: User Takes a Screenshot**
User sets their browser to 1920×1080 resolution. The entire dashboard fits within the viewport with no scrollbars. The user presses `Ctrl+Shift+S` (or uses DevTools screenshot) and receives a clean 1920×1080 image ready for PowerPoint insertion. No Blazor overlays, reconnection modals, or browser chrome appear in the captured area.

**Scenario 8: Dashboard Loads with Missing or Malformed JSON**
If `data.json` is missing or contains invalid JSON, the page displays a centered error message (e.g., "Unable to load dashboard data. Please check data.json.") instead of a blank page or crash. The error text is styled simply — no stack trace, no technical jargon.

**Scenario 9: User Hovers Over a Milestone Diamond**
When the user hovers over a milestone diamond on the timeline, the browser's default SVG tooltip (via `<title>` element) shows the milestone date, label, and type. No custom tooltip library is needed.

**Scenario 10: User Updates data.json and Reloads**
User edits `data.json` to change a milestone date or add a new shipped item. User restarts the app (`dotnet run`) or refreshes the browser (if hot-reload via `FileSystemWatcher` is enabled). The dashboard reflects the updated data immediately.

## Scope

### In Scope

- Single-page Blazor Server dashboard matching the `OriginalDesignConcept.html` design.
- Header component with project title, subtitle, backlog link, and marker legend.
- SVG timeline component with tracks, milestones (checkpoint/PoC/production), month grid lines, and NOW indicator.
- CSS Grid heatmap component with four status rows (Shipped, In Progress, Carryover, Blockers) across four month columns.
- Current-month column highlighting with distinct background colors.
- Data-driven rendering from a local `data.json` file.
- Fictional demo data for a realistic-looking example project ("Project Phoenix").
- C# model classes (`DashboardData`, `Milestone`, `StatusCategory`, etc.) matching the JSON schema.
- Singleton data service for reading and deserializing `data.json`.
- Custom `dashboard.css` ported from the reference HTML.
- Minimal `DashboardLayout.razor` with no navigation sidebar or menu.
- CSS rule to hide Blazor's `.components-reconnect-modal`.
- Fixed 1920×1080 viewport for screenshot readiness.
- Error handling for missing/malformed `data.json`.
- SVG `<title>` elements on milestones for basic hover tooltips.

### Out of Scope

- **Authentication and authorization** — No login, no roles, no Identity framework.
- **Database or persistent storage** — No Entity Framework, no SQLite, no SQL Server.
- **Cloud deployment** — No Azure App Service, no Docker, no CI/CD pipeline.
- **HTTPS / TLS** — Local HTTP only.
- **Responsive / mobile design** — Fixed 1920×1080, Windows desktop browser only.
- **Multi-project views** — One project per page; switch by swapping `data.json`.
- **Real-time collaboration** — Single-user, local-only.
- **Print stylesheet** — Screenshots replace printing.
- **Accessibility (WCAG compliance)** — The primary output is screenshots; screen reader optimization is not required.
- **Internationalization / localization** — English only.
- **Animated transitions or loading states** — Static render for screenshot fidelity.
- **Third-party UI component libraries** (MudBlazor, Radzen, Bootstrap, Tailwind).
- **Third-party charting libraries** (Chart.js, Plotly, ApexCharts).
- **Auto-refresh / live-reload of data** — Nice-to-have but not required for MVP. Manual restart is acceptable.

## Non-Functional Requirements

### Performance

- **Page load time:** Dashboard must fully render within 2 seconds of browser navigation on localhost.
- **Server-side render time:** The Razor component tree must render in <50ms (expected <10ms for ~100 DOM elements).
- **Startup time:** `dotnet run` must reach "listening on port" within 5 seconds.

### Reliability

- **Graceful degradation:** If `data.json` is missing or malformed, display a user-friendly error message. Do not throw unhandled exceptions to the browser.
- **No external dependencies at runtime:** The app must function with no internet connection.

### Portability

- **Target OS:** Windows 10/11 (developer workstation).
- **Target Browser:** Microsoft Edge or Google Chrome (latest stable).
- **Runtime:** .NET 8 LTS SDK installed on the machine. Optionally publish as self-contained for machines without the SDK.

### Security

- **No authentication required** — Local-only tool.
- **No sensitive data exposure** — `data.json` lives in the project root (not `wwwroot`), so it is not served via HTTP.
- **XSS protection** — Blazor's default HTML encoding for rendered strings. Use `MarkupString` only where required for SVG rendering, and only with developer-controlled data.

### Maintainability

- **Zero external NuGet packages** — Only built-in .NET 8 libraries.
- **Component-per-section architecture** — Each visual section (Header, Timeline, HeatmapGrid) is an independent `.razor` component.
- **CSS class names match the reference HTML** — Enables side-by-side visual comparison during development.

## Success Metrics

| Metric | Target | Measurement Method |
|--------|--------|--------------------|
| Visual fidelity | Dashboard screenshot is indistinguishable from `OriginalDesignConcept.html` rendered in a browser | Side-by-side comparison at 1920×1080 |
| Time to update data | < 2 minutes to edit `data.json` and see changes | Timed test: change a milestone date, restart, verify |
| Screenshot readiness | Zero overlays, scrollbars, or artifacts in captured image | Take screenshot in Edge at 1920×1080, paste into PowerPoint |
| Startup simplicity | Single command (`dotnet run`) launches the dashboard | Verify from a clean terminal |
| Data flexibility | New projects can be visualized by replacing `data.json` only | Create a second JSON file for a different fictional project, swap, verify rendering |
| Code simplicity | Total solution is one `.csproj` with zero external NuGet dependencies | Inspect `ReportingDashboard.csproj` for `<PackageReference>` elements |
| Development time | Complete implementation within 8–12 hours | Track elapsed time from project creation to final screenshot |

## Constraints & Assumptions

### Constraints

- **Technology stack is predetermined:** .NET 8 Blazor Server. No alternative frameworks (React, Angular, static HTML) will be considered.
- **No budget for tools or services:** $0 infrastructure cost. No paid UI libraries, no cloud hosting.
- **Fixed viewport:** 1920×1080 pixels. The design is not responsive and will not adapt to other screen sizes.
- **Windows-only:** Segoe UI font dependency and PowerPoint workflow assume Windows 10/11.
- **Single data source:** All content comes from one `data.json` file. No API integrations, no database queries.

### Assumptions

- The developer has .NET 8 SDK (8.0.x) installed on their Windows machine.
- The developer has access to Microsoft Edge or Google Chrome for viewing and screenshotting.
- The dashboard will serve one project at a time. Multiple projects require separate `data.json` files and separate runs or a file-switching mechanism.
- The `OriginalDesignConcept.html` file in the repository is the authoritative visual specification. Any discrepancy between this spec document and the HTML file should be resolved in favor of the HTML file.
- The `C:/Pics/ReportingDashboardDesign.png` image represents the stakeholder's desired final appearance and should be consulted alongside the HTML reference.
- 3–5 timeline tracks is the typical use case. If more than 5 tracks are needed, the timeline area height may need to increase, potentially requiring the heatmap to scroll or the overall layout to exceed 1080px.
- The heatmap displays exactly 4 months. If a different number of months is needed, the grid column template must be adjusted in CSS and the JSON schema extended.
- `data.json` is edited by technical users (PMs or developers) who are comfortable with JSON syntax.
- The "NOW" line position defaults to the system date (`DateTime.Now`) but can be overridden via the `nowDate` field in `data.json` for demo or historical reporting purposes.