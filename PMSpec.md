# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, delivery status, and monthly execution progress as a pixel-perfect 1920×1080 webpage optimized for PowerPoint screenshot capture. The dashboard reads all content from a `data.json` configuration file, rendering a timeline with milestone markers and a color-coded heatmap grid showing Shipped, In Progress, Carryover, and Blocker items across months—enabling any project lead to produce polished executive status slides in seconds without design tools.

## Business Goals

1. **Reduce executive reporting prep time by 90%** — Replace manual PowerPoint slide construction with a single screenshot of a live, data-driven dashboard page.
2. **Standardize project status communication** — Provide a consistent visual format (timeline + heatmap) so all projects report status in the same structure, improving cross-project readability for leadership.
3. **Enable self-service updates** — Allow any project lead to update a single JSON file and instantly produce a presentation-ready status view without engineering support or design skills.
4. **Achieve pixel-perfect screenshot fidelity** — Render at exactly 1920×1080 so screenshots paste directly into 16:9 PowerPoint decks without cropping, scaling, or quality loss.
5. **Minimize operational overhead** — Zero infrastructure cost, zero authentication, zero cloud dependencies. The tool runs locally on a developer's machine with `dotnet run`.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** to see the project title, organizational context, and current reporting period at the top of the dashboard, **so that** executives immediately understand which project and timeframe they are viewing.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` element.

- [ ] Dashboard displays the project title as a bold 24px heading (e.g., "Project Phoenix Release Roadmap")
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, styled in `#0078D4`
- [ ] Subtitle displays organization path and current month (e.g., "Engineering Platform · Core Infrastructure · April 2026") in 12px gray (`#888`) text
- [ ] A legend appears on the right side of the header showing four marker types: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line)
- [ ] All header content is read from `data.json` — no hardcoded project names

### US-2: View Milestone Timeline

**As an** executive viewer, **I want** to see a horizontal timeline showing project milestones across multiple workstreams, **so that** I can understand the overall project schedule, what has been completed, and what is coming next.

**Visual Reference:** Timeline area of `OriginalDesignConcept.html` — `.tl-area` element and inline SVG.

- [ ] Timeline renders as an SVG spanning the full width (~1560px) within a 196px-tall section with `#FAFAFA` background
- [ ] Left sidebar (230px wide) lists track labels (e.g., "M1 — API Gateway & Auth") with each track in its designated color
- [ ] Each track renders as a horizontal colored line with milestones positioned by date
- [ ] Checkpoints render as open circles with colored stroke; PoC milestones render as gold (`#F4B400`) diamonds with drop shadow; Production milestones render as green (`#34A853`) diamonds with drop shadow
- [ ] Each milestone displays a date label positioned above or below the marker
- [ ] A vertical dashed red (`#EA4335`) "NOW" line indicates the current date position with a "NOW" text label
- [ ] Month boundaries (Jan–Jun) appear as light vertical grid lines at equal intervals with month labels at the top
- [ ] The number of tracks and milestones is dynamic, driven by `data.json`
- [ ] Timeline supports 1–5 tracks without layout breakage

### US-3: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a color-coded grid showing what was shipped, what is in progress, what carried over, and what is blocked for each month, **so that** I can quickly assess project health and delivery velocity.

**Visual Reference:** Heatmap section of `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` elements.

- [ ] Heatmap renders as a CSS Grid with a "Status" column header and 4 month columns
- [ ] The current month column is visually highlighted with a gold-tinted header (`#FFF0D0`, text `#C07700`) and darker cell backgrounds
- [ ] Four status rows are displayed: ✓ Shipped (green), → In Progress (blue), ↻ Carryover (amber), ✕ Blockers (red)
- [ ] Each row header uses uppercase text with the category's accent color and tinted background
- [ ] Each cell lists work items as bullet-pointed entries (6px colored dot + 12px text)
- [ ] Empty cells for future months display a dash ("—") in light gray
- [ ] Heatmap fills the remaining vertical space below the timeline, stretching to fit 1080px total page height
- [ ] All item text and category assignments are read from `data.json`

### US-4: Load Dashboard Data from JSON

**As a** project lead, **I want** the dashboard to read all its content from a `data.json` file, **so that** I can update project status by editing a single file without touching any code.

- [ ] Application reads `data.json` from the `wwwroot/` directory at startup
- [ ] JSON structure includes sections for `project` (title, subtitle, backlog URL), `timeline` (tracks, milestones, date range), and `heatmap` (columns, items per category per month)
- [ ] Changing `data.json` and restarting the app (or using hot reload) reflects the updated data in the rendered dashboard
- [ ] A `data.sample.json` file is provided in the repository with documented field descriptions and example fictional data
- [ ] If `data.json` is malformed or missing required fields, the app logs a clear error message at startup rather than rendering a broken page

### US-5: Capture Screenshot for PowerPoint

**As a** project lead, **I want** the dashboard page to render at exactly 1920×1080 pixels with no scrolling, **so that** I can take a browser screenshot or use an automated tool and paste it directly into a 16:9 PowerPoint slide.

- [ ] The `<body>` element is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] All content fits within the viewport without scrollbars at 100% zoom in Chrome/Edge
- [ ] No dynamic elements (animations, loading spinners) interfere with static screenshot capture
- [ ] The page renders fully on first paint — no delayed JavaScript rendering or progressive loading

### US-6: Run Dashboard Locally

**As a** developer, **I want** to start the dashboard with a single command (`dotnet run` or `dotnet watch`), **so that** I can view and iterate on the dashboard without any setup beyond the .NET 8 SDK.

- [ ] `dotnet run` starts the application on `localhost:5000` (or configurable port)
- [ ] `dotnet watch` enables hot reload for CSS and Razor markup changes
- [ ] No external dependencies (databases, APIs, cloud services) are required
- [ ] Application starts in under 3 seconds on a standard development machine
- [ ] README includes clear setup instructions: prerequisites, clone, run, and screenshot workflow

## Visual Design Specification

**Canonical Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository and `C:/Pics/ReportingDashboardDesign.png`. Engineers MUST consult both files and match the rendered output exactly.

**Rendered Preview:**

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/62dc7dd0b787b86b86fa5b07f88beeba4e7b7498/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Page Layout

- **Viewport:** Fixed `1920px × 1080px`, `overflow: hidden`, white (`#FFFFFF`) background
- **Font:** `'Segoe UI', Arial, sans-serif`, base color `#111`
- **Layout Direction:** Vertical flex column (`display: flex; flex-direction: column`)
- **Horizontal Padding:** 44px on left and right (consistent across header, timeline, and heatmap sections)
- **Three Major Sections** stacked vertically:
  1. **Header** — fixed height (~46px), flex-shrink: 0
  2. **Timeline Area** — fixed height (196px), flex-shrink: 0
  3. **Heatmap Area** — fills remaining space (`flex: 1; min-height: 0`)

### Section 1: Header (`.hdr`)

- **Layout:** Flexbox row, `align-items: center`, `justify-content: space-between`
- **Padding:** `12px 44px 10px`
- **Border:** 1px solid `#E0E0E0` bottom
- **Left Side:**
  - Title: `<h1>` at 24px, font-weight 700, containing project name + inline link
  - Link: `#0078D4`, no text-decoration, preceded by "→" arrow character
  - Subtitle: 12px, color `#888`, margin-top 2px — shows org path and month
- **Right Side — Legend:** Horizontal flex row with 22px gap, 12px font size
  - PoC Milestone: 12×12px square rotated 45° (`transform: rotate(45deg)`), fill `#F4B400`
  - Production Release: 12×12px square rotated 45°, fill `#34A853`
  - Checkpoint: 8×8px circle, fill `#999`
  - Now indicator: 2×14px vertical bar, fill `#EA4335`, label "Now (Apr 2026)"

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox row, `align-items: stretch`
- **Dimensions:** Height 196px, padding `6px 44px 0`
- **Background:** `#FAFAFA`
- **Border:** 2px solid `#E8E8E8` bottom

#### Track Labels Sidebar (left, 230px wide)

- **Layout:** Flex column, `justify-content: space-around`
- **Padding:** `16px 12px 16px 0`
- **Border:** 1px solid `#E0E0E0` right
- **Each Track Label:**
  - Track ID (e.g., "M1") in 12px, font-weight 600, track color
  - Description on next line in font-weight 400, color `#444`
  - Track colors from data: Track 1 `#0078D4`, Track 2 `#00897B`, Track 3 `#546E7A`

#### SVG Timeline (right, flex: 1)

- **Container:** `.tl-svg-box`, padding-left 12px, padding-top 6px
- **SVG Dimensions:** width 1560px, height 185px, `overflow: visible`
- **Month Grid Lines:** Vertical lines at x = 0, 260, 520, 780, 1040, 1300 — stroke `#bbb` at 0.4 opacity
- **Month Labels:** 11px, font-weight 600, fill `#666`, positioned 5px right of each grid line at y=14
- **"NOW" Line:** Dashed vertical line (`stroke-dasharray: 5,3`), stroke `#EA4335`, stroke-width 2. Position calculated as: `x = ((currentDate - startDate) / (endDate - startDate)) * 1560`. Label "NOW" in 10px bold `#EA4335` at y=14.
- **Track Lanes:** Horizontal lines spanning full width at y = 42, 98, 154 (56px spacing). Stroke width 3, color matches track color.
- **Milestone Shapes:**
  - **Checkpoint:** `<circle>` r=5–7, white fill, colored stroke width 2.5. Date label in 10px `#666` positioned above (y - 16).
  - **PoC Diamond:** `<polygon>` forming 22px diamond (11px radius), fill `#F4B400`, with drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`). Label below or above.
  - **Production Diamond:** Same as PoC but fill `#34A853`.
  - **Small Checkpoint Dots:** `<circle>` r=4, solid fill `#999`, no stroke, no label.

### Section 3: Heatmap Area (`.hm-wrap`)

- **Layout:** Flex column, padding `10px 44px 10px`
- **Section Title:** 14px, font-weight 700, color `#888`, uppercase, letter-spacing 0.5px, margin-bottom 8px. Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"

#### Heatmap Grid (`.hm-grid`)

- **Layout:** CSS Grid
- **Columns:** `grid-template-columns: 160px repeat(4, 1fr)` — 1 label column + 4 month columns
- **Rows:** `grid-template-rows: 36px repeat(4, 1fr)` — 1 header row + 4 status rows
- **Border:** 1px solid `#E0E0E0` outer
- **Fills remaining vertical space** via `flex: 1; min-height: 0`

#### Header Row

| Cell | Class | Background | Font | Border |
|------|-------|------------|------|--------|
| Corner ("STATUS") | `.hm-corner` | `#F5F5F5` | 11px bold uppercase `#999` | right 1px `#E0E0E0`, bottom 2px `#CCC` |
| Month columns | `.hm-col-hdr` | `#F5F5F5` | 16px bold | right 1px `#E0E0E0`, bottom 2px `#CCC` |
| Current month | `.hm-col-hdr.apr-hdr` | `#FFF0D0` | 16px bold `#C07700` | same borders |

#### Status Rows — Color Specification

| Row | Header Class | Header BG | Header Text | Cell BG | Cell Highlight (current month) | Dot Color |
|-----|-------------|-----------|-------------|---------|-------------------------------|-----------|
| ✓ Shipped | `.ship-hdr` | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| → In Progress | `.prog-hdr` | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| ↻ Carryover | `.carry-hdr` | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| ✕ Blockers | `.block-hdr` | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Header Cells (`.hm-row-hdr`)

- 11px, font-weight 700, uppercase, letter-spacing 0.7px
- Padding `0 12px`
- Border-right: 2px solid `#CCC`
- Border-bottom: 1px solid `#E0E0E0`

#### Data Cells (`.hm-cell`)

- Padding: `8px 12px`
- Border-right: 1px solid `#E0E0E0`, border-bottom: 1px solid `#E0E0E0`
- Overflow: hidden
- **Each item (`.it`):** 12px, color `#333`, padding `2px 0 2px 12px`, line-height 1.35
- **Bullet dot (`::before`):** 6×6px circle, absolutely positioned left 0, top 7px, color per row category

### CSS Custom Properties (Recommended Improvement)

```css
:root {
    --color-shipped: #34A853;       --color-shipped-bg: #F0FBF0;
    --color-shipped-hdr: #1B7A28;   --color-shipped-hdr-bg: #E8F5E9;
    --color-shipped-highlight: #D8F2DA;
    --color-progress: #0078D4;      --color-progress-bg: #EEF4FE;
    --color-progress-hdr: #1565C0;  --color-progress-hdr-bg: #E3F2FD;
    --color-progress-highlight: #DAE8FB;
    --color-carryover: #F4B400;     --color-carryover-bg: #FFFDE7;
    --color-carryover-hdr: #B45309; --color-carryover-hdr-bg: #FFF8E1;
    --color-carryover-highlight: #FFF0B0;
    --color-blocker: #EA4335;       --color-blocker-bg: #FFF5F5;
    --color-blocker-hdr: #991B1B;   --color-blocker-hdr-bg: #FEF2F2;
    --color-blocker-highlight: #FFE4E4;
}
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Full Dashboard Render**
User navigates to `localhost:5000` in Chrome/Edge. The page renders at 1920×1080 with the header, timeline, and heatmap fully populated from `data.json`. No loading spinner, no progressive rendering — all content appears on first paint. The "NOW" line is positioned at the current date within the timeline.

**Scenario 2: User Views Project Header**
User sees the project title ("Project Phoenix Release Roadmap") in bold at the top-left, with a blue "→ ADO Backlog" link. Below is the subtitle showing org path and month. On the right, a legend shows four marker types. The user can identify the project, reporting period, and visual legend at a glance.

**Scenario 3: User Reads the Milestone Timeline**
User looks at the timeline area and sees 3 horizontal track lanes (M1, M2, M3) with labeled milestones. Completed milestones (checkpoints, PoC diamonds) appear to the left of the red "NOW" line. Future milestones (production diamonds) appear to the right. The user can visually gauge progress: items left of "NOW" are done, items right are upcoming.

**Scenario 4: User Hovers Over a Milestone Diamond**
User hovers over a gold PoC diamond on the timeline. The browser displays the SVG `<text>` label (e.g., "Mar 26 PoC") which is always visible — no tooltip is needed as labels are statically rendered. No hover state change is implemented; the dashboard is optimized for static screenshot capture.

**Scenario 5: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser navigates to the URL specified in `data.json` under `project.backlogUrl`. This is a standard `<a href>` with no SPA routing — it opens in the same tab (or new tab if the user Ctrl+clicks).

**Scenario 6: User Scans the Heatmap for Current Month Status**
User looks at the heatmap and immediately identifies the current month column (April) because it has a gold-tinted header (`#FFF0D0`) and darker cell backgrounds. They scan the four rows top-to-bottom: green Shipped items show completed work, blue In Progress shows active work, amber Carryover shows items that slipped from last month, and red Blockers show impediments.

**Scenario 7: User Compares Month-over-Month Progress**
User reads across the Shipped row from left to right (March → April) to see delivery velocity. They count items in each cell. They compare the Carryover row to see if debt is growing or shrinking. The consistent layout and color coding enables this comparison in under 10 seconds.

**Scenario 8: Data-Driven Rendering — Variable Item Counts**
A project lead updates `data.json` to add 6 items to the April Shipped cell. The dashboard renders all 6 items stacked vertically within the cell. Cell overflow is hidden (`overflow: hidden`) to maintain the fixed 1080px height. Items beyond the visible area are clipped — the data model should be curated to fit.

**Scenario 9: Data-Driven Rendering — Fewer Than 4 Months**
A project lead sets `heatmap.columns` to `["Mar", "Apr"]` (only 2 months). The CSS Grid `repeat(N, 1fr)` adapts, and only 2 month columns render. The grid columns definition must be dynamically generated from the data (not hardcoded at 4).

**Scenario 10: Empty State — Future Month with No Items**
For months that have no items yet (e.g., May, June), the heatmap cell displays a single dash "—" in light gray (`#AAA`). This clearly communicates "no data yet" rather than appearing broken.

**Scenario 11: Error State — Missing or Invalid data.json**
If `data.json` is missing or contains invalid JSON, the application logs a descriptive error to the console (e.g., "Failed to load data.json: file not found at path X"). The browser displays a simple error message rather than an empty or broken page. No stack trace is exposed in the UI.

**Scenario 12: Error State — Missing Required Fields**
If `data.json` is valid JSON but missing a required field (e.g., `project.title` is null), deserialization fails fast with a clear error message naming the missing field. The application does not render a half-populated dashboard.

**Scenario 13: Screenshot Capture Workflow**
User opens the dashboard in Chrome at `localhost:5000`, presses F11 for full-screen mode (or sets browser window to 1920×1080), and takes a screenshot using Snipping Tool, Win+Shift+S, or Playwright automation. The resulting image is exactly 1920×1080 and pastes directly into a 16:9 PowerPoint slide with no resizing needed.

**Scenario 14: Data Update and Refresh**
User edits `data.json` to update April's shipped items. If running with `dotnet watch`, the page hot-reloads automatically. If running with `dotnet run`, the user restarts the app and refreshes the browser. The updated data appears immediately.

**Scenario 15: No Responsive Behavior**
User resizes the browser window to a smaller viewport. The page does NOT reflow or adapt — it remains at 1920×1080 with content clipped or scrollbars appearing. This is by design; the page is built for a fixed screenshot resolution, not responsive viewing.

## Scope

### In Scope

- Single-page Blazor Server (or Blazor SSR) web application rendering at 1920×1080
- Header component with project title, backlog link, subtitle, and milestone legend
- SVG timeline component with N configurable tracks, date-positioned milestones (checkpoint circles, PoC diamonds, production diamonds), month grid lines, and a "NOW" date marker
- CSS Grid heatmap component with 4 status rows (Shipped, In Progress, Carryover, Blockers) × N month columns, with current-month highlighting
- All dashboard content driven by a single `data.json` file using `System.Text.Json` deserialization
- C# data model (records) for `DashboardData`, `ProjectInfo`, `TimelineTrack`, `Milestone`, `HeatmapData`, `HeatmapCategory`
- `DashboardDataService` singleton that loads and provides dashboard data
- `data.sample.json` with fictional "Project Phoenix" example data and field documentation
- CSS matching the reference design's color palette, typography, spacing, and layout exactly
- CSS custom properties for the 16-color palette to enable easy theming
- README with setup instructions, prerequisites, and screenshot workflow
- Support for 1–5 timeline tracks and 2–6 heatmap month columns

### Out of Scope

- **Authentication and authorization** — No login, no RBAC, no tokens, no middleware security
- **Database or persistent storage** — No SQLite, no EF Core, no data history tracking
- **Cloud deployment** — No Azure, no Docker, no CI/CD pipeline, no CDN
- **Responsive design** — Fixed 1920×1080 only; no mobile, tablet, or variable viewport support
- **Interactive features** — No tooltips, click-through drill-downs, filtering, or sorting
- **Real-time updates** — No WebSocket push, no SignalR interactivity, no auto-refresh polling
- **Multi-project support** — One `data.json` = one project. No project selector or multi-file routing
- **Data entry UI** — No forms, no inline editing, no JSON editor. Users edit `data.json` in a text editor
- **Export functionality** — No "Download as PNG" button, no PDF export. Screenshots are manual or via optional Playwright script
- **Accessibility compliance** — Not targeted for WCAG compliance (screenshot-optimized static view)
- **Internationalization** — English only, no localization
- **Automated testing** — bUnit and Playwright tests are optional stretch goals, not required for v1
- **Historical data or trending** — No month-over-month comparison views, no charting of velocity trends
- **Component library integration** — No MudBlazor, Radzen, or Syncfusion. Raw HTML/CSS only

## Non-Functional Requirements

### Performance

| Metric | Target | Rationale |
|--------|--------|-----------|
| **Application startup** | < 3 seconds | Developer experience; `dotnet run` to browser-ready |
| **Page load (first paint)** | < 500ms | All content must appear on first render for screenshot reliability |
| **JSON deserialization** | < 50ms | `data.json` is small (< 50KB); `System.Text.Json` is sufficient |
| **Memory footprint** | < 100MB | Single-user local tool; Blazor Server baseline is ~30MB |

### Reliability

- The application must render correctly 100% of the time given valid `data.json` input
- Invalid JSON must produce a clear error message, not a blank or partially rendered page
- The application must not crash or hang if `data.json` is deleted while running

### Browser Compatibility

- **Primary target:** Microsoft Edge and Google Chrome (latest stable, Chromium-based)
- **Screenshot standardization:** All screenshots must be captured in Chrome or Edge to ensure consistent SVG rendering
- Firefox and Safari are not tested or supported

### Security

- No authentication required
- No user input accepted (read-only data rendering)
- No network calls beyond initial page load from localhost
- `data.json` files containing sensitive project names should be added to `.gitignore`; a `data.sample.json` template must be committed to the repository
- No injection surfaces exist (no forms, no query parameters, no user-generated content)

### Maintainability

- Single `.sln` with single `.csproj` — no multi-project complexity
- Zero external NuGet dependencies beyond the .NET 8 SDK
- CSS custom properties for theming — changing project colors requires editing only `:root` variables
- Data model uses C# records with `init` properties for immutability and clear schema

## Success Metrics

1. **Visual Fidelity:** A screenshot of the running dashboard at 1920×1080 is indistinguishable from the reference design (`OriginalDesignConcept.html`) when placed side-by-side — same layout, colors, typography, and spacing.
2. **Data-Driven Rendering:** Changing `data.json` to a different fictional project (different title, tracks, milestones, heatmap items) produces a correctly rendered dashboard with no code changes.
3. **Setup Simplicity:** A new developer can clone the repo, run `dotnet run`, and see the dashboard in their browser within 60 seconds (assuming .NET 8 SDK is installed).
4. **Screenshot Workflow:** A project lead can update `data.json`, restart the app, and capture a PowerPoint-ready screenshot in under 2 minutes.
5. **Zero Dependencies:** `dotnet restore` downloads zero NuGet packages beyond what the .NET 8 SDK provides. No `node_modules`, no npm, no external CSS frameworks.
6. **Heatmap Readability:** An executive unfamiliar with the project can identify the current month, count shipped items, and spot blockers within 10 seconds of viewing the screenshot.
7. **Timeline Clarity:** The "NOW" line correctly divides past milestones from future milestones, and all milestone labels are legible without overlapping.

## Constraints & Assumptions

### Technical Constraints

- **.NET 8 SDK required** — The project targets .NET 8.0 LTS. Developers must have the SDK installed (version 8.0.x).
- **Windows-primary** — The design uses Segoe UI font, which is a Windows system font. The CSS includes Arial as a fallback, but visual fidelity is guaranteed only on Windows.
- **Fixed resolution** — The page is hardcoded to 1920×1080. It will not adapt to other resolutions. Browser zoom must be at 100%.
- **Chromium browsers only** — SVG drop shadow filters and text positioning are tested on Chrome/Edge only. Firefox/Safari may render differently.
- **No hot-reload for data** — In the base implementation, changing `data.json` requires an app restart. `FileSystemWatcher`-based live reload is a stretch goal.
- **Local-only execution** — The app binds to `localhost`. No remote access, no network deployment.

### Timeline Assumptions

- **Phase 1 (Day 1):** Scaffold project, port reference HTML/CSS as static markup, verify pixel match
- **Phase 2 (Day 1–2):** Implement data model, JSON loading, and data-driven rendering
- **Phase 3 (Day 2):** Implement dynamic SVG timeline with date-to-coordinate mapping
- **Phase 4 (Day 2–3):** Polish, `data.sample.json`, README, optional Playwright automation
- **Total estimated effort:** 2–3 days for a single developer familiar with Blazor and CSS

### Dependency Assumptions

- The .NET 8 SDK is already installed on the developer's machine
- The developer has access to the `OriginalDesignConcept.html` reference file and `C:/Pics/ReportingDashboardDesign.png`
- Chrome or Edge is available for viewing and screenshotting the dashboard
- No external APIs, services, or data sources are required — all data is local in `data.json`
- PowerPoint is available on the developer's machine for the final slide-paste step (not a software dependency, but a workflow assumption)

### Design Assumptions

- The heatmap will display **4 month columns** by default (matching the reference design), but the implementation should support 2–6 columns via `data.json` configuration
- The timeline will display **3 tracks** by default, but the implementation should support 1–5 tracks with dynamic SVG height adjustment
- The "NOW" marker will be **auto-calculated from the system date** for simplicity. If reproducible screenshots are needed, the user can override this by adding an optional `nowDate` field to `data.json`
- Work items in heatmap cells are **plain text strings** — no links, no rich formatting, no icons
- The dashboard displays a **single project** per instance. Multi-project support is out of scope for v1

### Open Design Decisions (To Be Resolved During Implementation)

1. **Dynamic vs. fixed heatmap column count** — The CSS Grid `repeat(N, 1fr)` must be generated dynamically if N varies. Default assumption: support variable N from data.
2. **SVG height scaling** — With more than 3 tracks, the 185px SVG height may be insufficient. Assumption: scale to `tracks.length * 56px + 17px` for the SVG height and adjust `.tl-area` height accordingly.
3. **Milestone label collision** — When milestones are close together on the timeline, labels may overlap. Assumption: alternate label positions (above/below track line) and accept minor overlaps in edge cases — the user curates data to avoid this.
4. **"NOW" date source** — Default to `DateTime.Today`. If `data.json` includes an optional `timeline.nowDate` field, use that instead for reproducible screenshots.