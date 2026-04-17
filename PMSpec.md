# PM Specification: Executive Project Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, delivery status, and monthly execution progress as a fixed 1920×1080 webpage optimized for PowerPoint screenshot capture. The dashboard reads all data from a local `data.json` configuration file and renders an SVG timeline with milestone markers alongside a color-coded heatmap grid showing Shipped, In Progress, Carryover, and Blocker items by month — enabling project leads to produce polished executive status slides with zero manual formatting effort.

## Business Goals

1. **Eliminate manual slide creation** — Reduce the time spent building executive status update slides from hours of PowerPoint formatting to a single screenshot of a live, data-driven dashboard.
2. **Standardize project reporting** — Provide a consistent, repeatable visual format for communicating project health across milestones, shipped work, active work, carryover, and blockers.
3. **Enable rapid status updates** — Allow project leads to update a single JSON file and immediately capture a presentation-ready screenshot, enabling weekly or monthly reporting cadences with minimal overhead.
4. **Improve executive visibility** — Give leadership a clear, at-a-glance view of where a project stands across its major workstreams, what has been delivered, what is in flight, and what is blocked.
5. **Zero infrastructure cost** — Deliver a localhost-only tool with no cloud dependencies, no authentication, and no ongoing operational burden.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** to see the project title, workstream subtitle, current month, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are looking at.

*Visual Reference: Header section (`.hdr`) of `OriginalDesignConcept.html`*

- [ ] The project title is displayed in 24px bold font at the top-left
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, opening the URL from `data.json`
- [ ] The subtitle line shows the workstream name and current month in 12px gray text
- [ ] A legend appears at the top-right with four items: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now line (red vertical bar)
- [ ] All text and links are populated from `data.json`

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major project workstreams with milestone markers at key dates, **so that** I can understand the planned delivery cadence and where we are relative to today.

*Visual Reference: Timeline area (`.tl-area` and SVG) of `OriginalDesignConcept.html`*

- [ ] Each workstream (track) is rendered as a horizontal line spanning the full timeline date range
- [ ] Track labels (e.g., "M1 — Chatbot & MS Role") appear in a fixed-width left sidebar (230px)
- [ ] Checkpoints render as open circles with a border color matching the track
- [ ] PoC milestones render as gold (`#F4B400`) diamond shapes with drop shadow
- [ ] Production releases render as green (`#34A853`) diamond shapes with drop shadow
- [ ] Each milestone has a date label positioned above or below the marker
- [ ] Vertical month gridlines appear at evenly spaced intervals with month labels at the top
- [ ] A red dashed "NOW" line (`#EA4335`) appears at the position corresponding to the current date (or override date from `data.json`)
- [ ] The SVG viewBox is `0 0 1560 185`, matching the design reference
- [ ] Milestone X positions are calculated from their date relative to the timeline start/end range
- [ ] The number of tracks and milestones per track is driven entirely by `data.json`

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was shipped, what is in progress, what carried over, and what is blocked — organized by month, **so that** I can assess execution health at a glance.

*Visual Reference: Heatmap grid (`.hm-wrap`, `.hm-grid`) of `OriginalDesignConcept.html`*

- [ ] A section title "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" appears above the grid
- [ ] The grid has a row-header column (160px) and one data column per month from `data.json`
- [ ] Column headers display month names; the current month column is highlighted with a gold background (`#FFF0D0`) and gold text (`#C07700`)
- [ ] Four status rows are rendered: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each row header uses the category's accent color and uppercase text
- [ ] Data cells for the current month use a darker shade than other months
- [ ] Each item within a cell displays as a 12px text line with a 6px colored dot bullet (matching the row's status color)
- [ ] Empty cells display a gray dash ("—")
- [ ] The grid fills the remaining vertical space below the timeline
- [ ] The number of month columns adapts dynamically based on `data.json`

### US-4: Configure Dashboard via JSON

**As a** project lead, **I want** to edit a `data.json` file to change all dashboard content — project info, timeline tracks, milestones, and monthly status items — **so that** I can reuse the same dashboard for different projects or reporting periods without touching code.

- [ ] A `data.json` file in the application root defines all dashboard data
- [ ] The JSON schema includes: `project` (title, subtitle, backlogUrl, currentMonth), `timeline` (startDate, endDate, tracks with milestones), `months` array, and four status sections (`shipped`, `inProgress`, `carryover`, `blockers`) keyed by month
- [ ] Changing `data.json` and restarting the app reflects the new data
- [ ] The app starts successfully with the provided sample `data.json`
- [ ] Invalid or missing `data.json` displays a user-friendly error message, not a stack trace

### US-5: Capture Screenshot for PowerPoint

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, browser chrome artifacts, or overflow, **so that** I can take a clean screenshot and paste it directly into a PowerPoint slide.

- [ ] The page body is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] No scrollbars appear at 1920×1080 resolution
- [ ] The layout does not reflow or wrap at this resolution
- [ ] The page renders correctly in Chrome and Edge at 100% zoom in fullscreen (F11)
- [ ] Screenshots captured via Win+Shift+S or DevTools device emulation produce a clean, presentation-ready image

### US-6: Run Dashboard Locally

**As a** developer, **I want** to start the dashboard with a single `dotnet run` command and access it at `https://localhost:5001`, **so that** there is zero setup friction.

- [ ] `dotnet run` starts the application without errors
- [ ] The dashboard is accessible at the configured localhost URL
- [ ] No database, Docker, or external service is required
- [ ] `dotnet watch run` enables hot reload for CSS and Razor changes during development
- [ ] No authentication prompt or login page appears

### US-7: Live Reload on Data Change (Phase 3 — Optional)

**As a** project lead, **I want** the dashboard to automatically refresh when I save changes to `data.json`, **so that** I can iterate on status content without restarting the app.

- [ ] A `FileSystemWatcher` monitors `data.json` for changes
- [ ] When the file is saved, the dashboard re-reads and re-renders within 2 seconds
- [ ] No manual browser refresh is required
- [ ] File watch errors (e.g., file locked) are handled gracefully

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the `ReportingDashboard` repository. Engineers MUST consult this file and the rendered screenshot (`docs/design-screenshots/OriginalDesignConcept.png`) as the authoritative visual specification. All UI implementations must match these visuals exactly.

![OriginalDesignConcept design reference](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/main/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Page Layout

- **Viewport:** Fixed `1920px × 1080px`, no scrolling (`overflow: hidden`)
- **Background:** `#FFFFFF` (white)
- **Font Family:** `'Segoe UI', Arial, sans-serif`
- **Base Text Color:** `#111`
- **Layout Model:** Vertical flexbox (`flex-direction: column`), three stacked sections filling the viewport height:
  1. **Header** (flex-shrink: 0) — approximately 50px tall
  2. **Timeline Area** (flex-shrink: 0) — fixed `196px` height
  3. **Heatmap Area** (flex: 1) — fills remaining vertical space

### Section 1: Header (`.hdr`)

- **Padding:** `12px 44px 10px`
- **Border:** `1px solid #E0E0E0` bottom
- **Layout:** Flexbox row, `align-items: center`, `justify-content: space-between`
- **Left Side:**
  - **Title** (`h1`): `24px`, `font-weight: 700`, color `#111`
  - **Backlog Link** (`a`): Inline with title, color `#0078D4`, no underline, preceded by "→"
  - **Subtitle** (`.sub`): `12px`, color `#888`, `margin-top: 2px`
- **Right Side — Legend:** Flexbox row, `gap: 22px`
  - **PoC Milestone:** `12×12px` square rotated 45° (`transform: rotate(45deg)`), fill `#F4B400`
  - **Production Release:** `12×12px` square rotated 45°, fill `#34A853`
  - **Checkpoint:** `8×8px` circle, fill `#999`
  - **Now Line:** `2×14px` vertical bar, fill `#EA4335`
  - Each legend item: `12px` font, flexbox row with `6px` gap

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed `196px`
- **Background:** `#FAFAFA`
- **Padding:** `6px 44px 0`
- **Border:** `2px solid #E8E8E8` bottom
- **Layout:** Flexbox row, `align-items: stretch`

#### Left Sidebar (Track Labels)
- **Width:** `230px`, flex-shrink: 0
- **Border:** `1px solid #E0E0E0` right
- **Padding:** `16px 12px 16px 0`
- **Layout:** Flexbox column, `justify-content: space-around`
- **Each Track Label:**
  - Track ID (e.g., "M1"): `12px`, `font-weight: 600`, color matches track (e.g., `#0078D4`, `#00897B`, `#546E7A`)
  - Track description: `12px`, `font-weight: 400`, color `#444`

#### Right SVG Canvas (`.tl-svg-box`)
- **Flex:** 1 (fills remaining width)
- **Padding:** `left: 12px`, `top: 6px`
- **SVG Dimensions:** `width="1560" height="185"`, `overflow: visible`

#### SVG Elements

| Element | Appearance | Details |
|---------|-----------|---------|
| **Month gridlines** | Vertical lines at 260px intervals | `stroke: #bbb`, `stroke-opacity: 0.4`, `stroke-width: 1` |
| **Month labels** | Top of SVG, offset 5px right of gridline | `fill: #666`, `font-size: 11`, `font-weight: 600` |
| **Track lines** | Horizontal lines at Y=42, Y=98, Y=154 | `stroke-width: 3`, color matches track |
| **Checkpoint markers** | Open circle on track line | `r="5-7"`, `fill: white`, `stroke` matches track or `#888`, `stroke-width: 2.5` |
| **PoC milestone** | Diamond (rotated square polygon) | `fill: #F4B400`, SVG `<filter>` drop shadow (`dx=0, dy=1, stdDeviation=1.5, flood-opacity=0.3`) |
| **Production milestone** | Diamond (rotated square polygon) | `fill: #34A853`, same drop shadow filter |
| **Milestone labels** | Text above or below marker | `fill: #666`, `font-size: 10`, `text-anchor: middle` |
| **"NOW" line** | Dashed vertical line | `stroke: #EA4335`, `stroke-width: 2`, `stroke-dasharray: 5,3` |
| **"NOW" label** | Text right of line | `fill: #EA4335`, `font-size: 10`, `font-weight: 700` |
| **Small progress dots** | Filled circles along a track | `r="4"`, `fill: #999` (represent minor checkpoints) |

### Section 3: Heatmap Area (`.hm-wrap`)

- **Flex:** 1 (fills remaining space)
- **Padding:** `10px 44px 10px`
- **Layout:** Flexbox column

#### Heatmap Title (`.hm-title`)
- `font-size: 14px`, `font-weight: 700`, `color: #888`
- `letter-spacing: 0.5px`, `text-transform: uppercase`
- `margin-bottom: 8px`

#### Heatmap Grid (`.hm-grid`)
- **Layout:** CSS Grid
- **Columns:** `160px repeat(N, 1fr)` where N = number of months from `data.json`
- **Rows:** `36px repeat(4, 1fr)` — one header row + four status rows
- **Border:** `1px solid #E0E0E0`

#### Grid Header Row

| Cell | Style |
|------|-------|
| **Corner cell** (`.hm-corner`) | `background: #F5F5F5`, `font-size: 11px`, `font-weight: 700`, `color: #999`, uppercase, `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC` |
| **Month column headers** (`.hm-col-hdr`) | `font-size: 16px`, `font-weight: 700`, `background: #F5F5F5`, centered, `border-bottom: 2px solid #CCC` |
| **Current month header** (`.apr-hdr`) | `background: #FFF0D0`, `color: #C07700` |

#### Status Rows — Color Scheme

| Status | Row Header BG | Row Header Text | Cell BG (default) | Cell BG (current month) | Bullet Color |
|--------|--------------|-----------------|-------------------|------------------------|-------------|
| **Shipped** | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| **In Progress** | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| **Carryover** | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| **Blockers** | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers (`.hm-row-hdr`)
- `font-size: 11px`, `font-weight: 700`, `text-transform: uppercase`, `letter-spacing: 0.7px`
- `padding: 0 12px`
- `border-right: 2px solid #CCC`, `border-bottom: 1px solid #E0E0E0`
- Include status emoji: ✅ Shipped, 🔵 In Progress, ⏳ Carryover, 🔴 Blockers

#### Data Cells (`.hm-cell`)
- `padding: 8px 12px`
- `border-right: 1px solid #E0E0E0`, `border-bottom: 1px solid #E0E0E0`
- `overflow: hidden`

#### Item Text (`.it`)
- `font-size: 12px`, `color: #333`, `padding: 2px 0 2px 12px`, `line-height: 1.35`
- `position: relative` (for the pseudo-element bullet)
- **Bullet** (`::before`): `width: 6px`, `height: 6px`, `border-radius: 50%`, positioned `left: 0`, `top: 7px`, color per row status

### Component Hierarchy

```
App.razor
└── MainLayout.razor (no navigation, no sidebar)
    └── Dashboard.razor (route "/")
        ├── Header.razor
        │   ├── Title + Backlog Link
        │   ├── Subtitle
        │   └── Legend (PoC, Production, Checkpoint, Now)
        ├── TimelineSection.razor
        │   ├── Track Labels Sidebar
        │   └── SVG Canvas
        │       ├── Month Gridlines + Labels
        │       ├── Track Lines (per workstream)
        │       ├── Milestone Markers (checkpoint/poc/production)
        │       ├── Milestone Labels
        │       └── "NOW" Line + Label
        └── HeatmapGrid.razor
            ├── Section Title
            ├── Corner Cell + Month Column Headers
            └── HeatmapRow.razor × 4
                ├── Row Header (status label)
                └── Data Cells × N months
                    └── Item Lines with colored bullets
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `https://localhost:5001`. The dashboard renders immediately as a single full-page view at 1920×1080. The header shows the project title with backlog link, the timeline shows all workstream tracks with milestone markers, and the heatmap displays the current month's execution status. The "NOW" line on the timeline is positioned at today's date. No loading spinner, no skeleton screen — the page renders server-side in a single paint.

**Scenario 2: User Views the Header and Legend**
User looks at the top of the page. The left side shows the project name (e.g., "Privacy Automation Release Roadmap") with a blue "→ ADO Backlog" hyperlink and a gray subtitle showing the workstream and current month. The right side shows a legend with four items: a gold diamond for PoC milestones, a green diamond for production releases, a gray circle for checkpoints, and a red vertical bar for the "Now" indicator.

**Scenario 3: User Reads the Milestone Timeline**
User looks at the timeline section below the header. Three horizontal track lines span from January to June. Each track is labeled on the left sidebar (M1, M2, M3 with descriptions). Diamond markers at key dates indicate PoC and Production milestones. Open circles indicate checkpoints. A red dashed vertical line labeled "NOW" shows the current date position. Date labels appear above or below each milestone marker.

**Scenario 4: User Hovers Over a Milestone Diamond**
No hover interaction is implemented. The dashboard is a static visual — tooltips and hover states are out of scope. The milestone's date label is always visible as static text near the marker.

**Scenario 5: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser navigates to the URL specified in `data.json` `project.backlogUrl` (e.g., an Azure DevOps backlog page). The link opens in the same tab (standard `<a href>` behavior).

**Scenario 6: User Scans the Heatmap for Current Month Status**
User looks at the heatmap grid. The current month column (e.g., "April") is visually highlighted with a gold header background (`#FFF0D0`) and darker cell backgrounds in each row. The user can quickly see what shipped this month (green row), what is actively in progress (blue row), what carried over from last month (amber row), and what is blocked (red row). Each item has a colored bullet dot.

**Scenario 7: Data-Driven Rendering with Different Content**
User edits `data.json` to change the project title, add a new milestone track, or update the month list from 4 to 6 months. After restarting the app (or with live reload enabled), the dashboard renders the updated data: the header reflects the new title, the timeline adds the new track line with its milestones, and the heatmap grid expands to 6 month columns. No code changes required.

**Scenario 8: Heatmap Month with No Items (Empty State)**
User configures `data.json` with an empty array for a status category in a given month (e.g., `"blockers": { "Jan": [] }`). The corresponding heatmap cell displays a single gray dash "—" instead of item bullets, indicating no items for that category/month combination.

**Scenario 9: Missing or Malformed `data.json` (Error State)**
User deletes `data.json` or introduces a JSON syntax error. The dashboard displays a user-friendly error banner (e.g., "Unable to load dashboard data. Please check that data.json exists and contains valid JSON.") instead of a raw stack trace or blank page.

**Scenario 10: Screenshot Capture Workflow**
User opens the dashboard in Chrome/Edge, presses F11 for fullscreen, and uses Win+Shift+S to capture the screen. The captured image is exactly the dashboard content at 1920×1080 with no scrollbars, browser chrome, or overflow artifacts. User pastes directly into PowerPoint.

**Scenario 11: Non-Standard Screen Resolution (No Responsive Behavior)**
User opens the dashboard on a 1366×768 laptop screen. The page does not reflow or adapt — it renders at 1920×1080 with overflow hidden. The user must zoom out or use DevTools device emulation to see the full layout. Responsive design is explicitly not supported.

## Scope

### In Scope

- Single-page Blazor Server dashboard at route `/`
- Fixed 1920×1080 layout matching `OriginalDesignConcept.html`
- Header component with project title, backlog link, subtitle, and legend
- SVG timeline with configurable workstream tracks and milestone markers (checkpoint, PoC, production)
- Auto-calculated "NOW" line based on system date, with optional override in `data.json`
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) and dynamic month columns
- Current month column highlighting with distinct background colors
- All data driven from a single `data.json` file
- C# record-based data models for JSON deserialization via `System.Text.Json`
- `DashboardDataService` singleton for reading and caching `data.json`
- Sample `data.json` with fictional project data for demonstration
- Graceful error handling for missing/malformed `data.json`
- Global `dashboard.css` stylesheet ported from the HTML design reference
- `dotnet run` / `dotnet watch run` development experience with zero external dependencies
- **Optional (Phase 3):** `FileSystemWatcher` for live reload of `data.json`, `@media print` styles, `/screenshot` route variant, 2–3 sample data files

### Out of Scope

- User authentication or authorization (no Identity, no OAuth, no login)
- Database or ORM layer (no Entity Framework, no SQLite, no SQL Server)
- Admin UI or in-browser JSON editor for `data.json`
- Responsive or mobile layouts (fixed 1920×1080 only)
- Dark mode or theme switching
- API controllers, REST endpoints, or GraphQL
- Docker containerization or Kubernetes deployment
- CI/CD pipelines (GitHub Actions, Azure Pipelines)
- Logging infrastructure beyond basic console output
- Multiple pages, navigation menus, or routing beyond `/`
- Third-party component libraries (MudBlazor, Radzen, Blazorise)
- Third-party charting libraries (Chart.js, Radzen Charts)
- Tooltip or hover interactions on milestone markers
- Historical data trending or time-series analysis
- Multi-project views or project switching within the same instance
- Email or notification integrations
- Export to PDF or PowerPoint (screenshots are the intended workflow)

## Non-Functional Requirements

### Performance

| Metric | Target | Rationale |
|--------|--------|-----------|
| **Initial page load** | < 500ms on localhost | Single-page Blazor Server render with no external API calls |
| **data.json parse time** | < 50ms for files up to 100KB | `System.Text.Json` is highly optimized; typical data files will be 2–5KB |
| **SVG render time** | < 100ms for up to 10 tracks × 20 milestones | Server-side Razor generation, no client-side JS |
| **Memory usage** | < 100MB RSS | Minimal app with a single SignalR connection |
| **Live reload (Phase 3)** | < 2 seconds from file save to UI update | `FileSystemWatcher` + `StateHasChanged` |

### Security

- **Authentication:** None. Explicitly excluded.
- **Network binding:** Kestrel binds to `localhost` only by default. The app is not accessible from other machines unless explicitly reconfigured.
- **Data sensitivity:** `data.json` contains project status text (no PII, no secrets, no credentials). No encryption required.
- **Input validation:** The app only reads a local file — no user-provided input, no injection surface.

### Scalability

- **Not applicable.** This is a single-user, localhost-only tool. It will never need to scale beyond one concurrent user.

### Reliability

- **Availability target:** Best-effort. If the app crashes, the user restarts it. No SLA, no health checks, no monitoring.
- **Error handling:** Malformed `data.json` must produce a readable error message. All other failures may surface as default Blazor error pages.

### Compatibility

- **Browsers:** Chrome 120+ and Edge 120+ on Windows 10/11 (screenshot workflow requires these)
- **OS:** Windows 10/11 (Segoe UI font dependency; the primary user runs Windows)
- **SDK:** .NET 8 SDK (latest stable)
- **Resolution:** 1920×1080 only — no other resolutions are tested or supported

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|-------------------|
| 1 | **Visual fidelity** | Dashboard screenshot is indistinguishable from the `OriginalDesignConcept.html` reference at 1920×1080 | Side-by-side visual comparison |
| 2 | **Time to first screenshot** | < 5 minutes from `git clone` to a captured screenshot pasted in PowerPoint | Manual timing of: `dotnet run` → open browser → F11 → Win+Shift+S → paste |
| 3 | **Data change turnaround** | < 2 minutes to update `data.json` and capture a new screenshot | Manual timing of: edit JSON → restart (or live reload) → capture |
| 4 | **Zero external dependencies** | 0 NuGet packages beyond the .NET 8 SDK | Inspect `.csproj` file |
| 5 | **Single-command startup** | App runs with `dotnet run` — no prior setup steps | Verify on a clean machine with only .NET 8 SDK installed |
| 6 | **Error resilience** | Missing/malformed `data.json` shows a helpful message, not a crash | Test by deleting or corrupting the file |
| 7 | **Stakeholder approval** | Project lead confirms the dashboard output is suitable for executive slide decks | Stakeholder review of captured screenshot |

## Constraints & Assumptions

### Technical Constraints

- **Framework:** Blazor Server on .NET 8 — mandated by the existing project stack and team skillset.
- **No external packages:** The solution must use only SDK-included libraries (`System.Text.Json`, `System.IO`, ASP.NET Core Blazor). No third-party NuGet dependencies.
- **Fixed resolution:** The layout is locked to 1920×1080. Responsive behavior will not be implemented.
- **SVG for timeline:** The milestone timeline must be rendered as inline SVG in Razor (server-side), not via a JavaScript charting library. This keeps the stack pure C#/Blazor.
- **Single project file:** One `data.json` = one project dashboard. Multi-project support requires separate instances or file swapping.
- **Windows-only screenshots:** The primary screenshot workflow assumes Windows (Segoe UI font, Win+Shift+S snipping tool, Edge/Chrome on Windows).

### Timeline Assumptions

- **Phase 1 (Static Port):** 1 day — Port the HTML design reference to Blazor with hardcoded data. Achieve visual parity.
- **Phase 2 (Data-Driven):** 1 day — Replace hardcoded content with `data.json` deserialization. Parameterize SVG coordinates and grid columns.
- **Phase 3 (Polish):** 1 day, optional — Add `FileSystemWatcher` live reload, `@media print`, error handling, sample data files.
- **Total estimated effort:** 2–3 days for a developer familiar with Blazor.

### Dependency Assumptions

- **.NET 8 SDK** is installed on the developer's machine.
- The developer has access to the `ReportingDashboard` GitHub repository containing `OriginalDesignConcept.html`.
- The developer has access to the design screenshot at `C:/Pics/ReportingDashboardDesign.png` for visual reference.
- **Segoe UI** font is available on the target machine (standard on Windows 10/11).
- No external services, APIs, or databases are required at any point.
- The project lead will provide or approve the `data.json` content for their specific project; the engineering team provides fictional sample data for development and demonstration.

### Design Assumptions

- The `OriginalDesignConcept.html` is the **canonical design specification**. Any ambiguity in this PM spec is resolved by consulting the HTML source and its rendered screenshot.
- The heatmap displays **exactly four status categories** (Shipped, In Progress, Carryover, Blockers) — this is fixed and not configurable.
- The number of months (heatmap columns) and the number of timeline tracks are configurable via `data.json`.
- The "NOW" line defaults to the current system date but can be overridden in `data.json` for generating screenshots dated in the past or future.
- Milestone marker types are limited to three: `checkpoint` (circle), `poc` (gold diamond), `production` (green diamond). No other marker types are supported.