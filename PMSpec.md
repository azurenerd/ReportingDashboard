# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, shipping status, and monthly progress in a format optimized for PowerPoint screenshot capture at 1920×1080. The dashboard reads all data from a local `data.json` configuration file and is built with Blazor Server (.NET 8), requiring no authentication, no database, and no cloud services. The primary use case is: edit JSON → run locally → screenshot → paste into executive slide deck.

## Business Goals

1. **Reduce executive reporting prep time by 75%** — Replace manual PowerPoint slide construction with a single JSON edit and browser screenshot workflow.
2. **Provide a consistent, professional visual format** — Standardize how project status is communicated to leadership using a repeatable template derived from the `OriginalDesignConcept.html` design.
3. **Enable real-time status visibility** — Allow project leads to update `data.json` and immediately see the rendered dashboard without redeployment or complex tooling.
4. **Eliminate tooling friction** — Deliver a zero-dependency local tool that runs with a single `dotnet run` command, requiring no accounts, licenses, or infrastructure.
5. **Support multi-project reuse** — Structure the data schema so any team can fork the project, edit `data.json` with their own milestones and status items, and produce their own executive dashboard.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Metadata

**As an** executive viewer, **I want** to see the project name, organizational context, current reporting period, and a link to the backlog at the top of the dashboard, **so that** I immediately know what project I'm looking at and where to find more detail.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` div.

- [ ] Dashboard displays the project title from `data.json` `title` field in bold 24px font
- [ ] Subtitle line shows organizational hierarchy and current month from `data.json` `subtitle` field
- [ ] Backlog link renders as a clickable hyperlink sourced from `data.json` `backlogLink` field
- [ ] Legend icons appear to the right of the header: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), Now indicator (red vertical line)
- [ ] Header is visually separated from the timeline area by a 1px solid `#E0E0E0` bottom border

### US-2: View Milestone Timeline

**As an** executive viewer, **I want** to see a horizontal timeline showing each major milestone's progress across months with markers for checkpoints, PoC dates, and production releases, **so that** I can quickly understand the project's trajectory and key delivery dates.

**Visual Reference:** Timeline area of `OriginalDesignConcept.html` — `.tl-area` div and inline SVG.

- [ ] Each milestone from `data.json` `milestones` array renders as a horizontal colored line spanning the full timeline width
- [ ] Milestone labels (e.g., "M1", "M2") and descriptions appear in a fixed-width left sidebar (230px)
- [ ] Checkpoint events render as open circles with a border matching the milestone color
- [ ] PoC events render as gold (`#F4B400`) diamond shapes with drop shadow
- [ ] Production events render as green (`#34A853`) diamond shapes with drop shadow
- [ ] Each event displays a date label positioned near the marker
- [ ] A vertical dashed red (`#EA4335`) line labeled "NOW" appears at the position corresponding to the current date
- [ ] Month labels (Jan, Feb, Mar, etc.) appear at evenly spaced vertical grid lines
- [ ] Timeline SVG renders at 1560px width within the `.tl-svg-box` container
- [ ] Timeline area has a fixed height of 196px with `#FAFAFA` background

### US-3: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked for each month, **so that** I can assess execution velocity and identify problem areas at a glance.

**Visual Reference:** Heatmap section of `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` divs.

- [ ] Heatmap renders as a CSS Grid with row header column (160px) and one column per month from `data.json` `months` array
- [ ] Header row displays month names; the current month column (matching `data.json` `currentMonth`) is highlighted with gold background (`#FFF0D0`) and amber text (`#C07700`)
- [ ] **Shipped row** (green): header `#E8F5E9` bg / `#1B7A28` text, cells `#F0FBF0` bg, current month `#D8F2DA` bg, bullet dots `#34A853`
- [ ] **In Progress row** (blue): header `#E3F2FD` bg / `#1565C0` text, cells `#EEF4FE` bg, current month `#DAE8FB` bg, bullet dots `#0078D4`
- [ ] **Carryover row** (amber): header `#FFF8E1` bg / `#B45309` text, cells `#FFFDE7` bg, current month `#FFF0B0` bg, bullet dots `#F4B400`
- [ ] **Blockers row** (red): header `#FEF2F2` bg / `#991B1B` text, cells `#FFF5F5` bg, current month `#FFE4E4` bg, bullet dots `#EA4335`
- [ ] Each cell lists work items from `data.json` `heatmapRows[].items[month]` as 12px text with colored dot prefix
- [ ] Empty months display a dash (`-`) in muted gray (`#AAA`)
- [ ] Section title "Monthly Execution Heatmap" appears above the grid in uppercase 14px bold gray (`#888`)

### US-4: Configure Dashboard Content via JSON

**As a** project lead, **I want** to edit a single `data.json` file to update all dashboard content (title, milestones, status items), **so that** I can refresh the dashboard without touching any code.

- [ ] All displayed text, dates, colors, and status items are sourced from `wwwroot/data/data.json`
- [ ] The JSON schema matches the `DashboardConfig` C# model (title, subtitle, backlogLink, currentMonth, months, milestones, heatmapRows)
- [ ] Changing `data.json` and restarting the app (or using `dotnet watch`) reflects the new data
- [ ] Malformed JSON produces a clear error message in the browser rather than a blank page or crash
- [ ] Missing optional fields degrade gracefully (e.g., empty milestones array shows no timeline markers)

### US-5: Capture Screenshot-Ready Output

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrolling, **so that** I can take a full-page browser screenshot and paste it directly into a PowerPoint slide.

- [ ] Page body is fixed at 1920px width and 1080px height with `overflow: hidden`
- [ ] No horizontal or vertical scrollbars appear at 100% browser zoom
- [ ] The layout fills the viewport completely — header at top, timeline in middle, heatmap fills remaining space
- [ ] Font rendering uses Segoe UI (Windows system font) with Arial fallback
- [ ] All colors, spacing, and proportions match the `OriginalDesignConcept.html` reference at pixel level

### US-6: Run Dashboard Locally with One Command

**As a** developer, **I want** to start the dashboard with `dotnet run` and view it in my browser, **so that** there is zero setup friction.

- [ ] `dotnet run` starts the Kestrel server and serves the dashboard on `https://localhost:5001` (or configured port)
- [ ] No NuGet package restore failures — the project uses only built-in .NET 8 packages
- [ ] `dotnet watch run` enables live reload during development
- [ ] The default route (`/`) renders the dashboard page directly (no navigation required)
- [ ] No sidebar, nav menu, or default Blazor template chrome appears

## Visual Design Specification

**Reference File:** `OriginalDesignConcept.html` and rendered screenshot at `docs/design-screenshots/OriginalDesignConcept.png`

### Overall Page Layout

The page is a single fixed-viewport layout at exactly **1920×1080 pixels** using a vertical flex column (`display: flex; flex-direction: column`). Background is pure white (`#FFFFFF`). The page is divided into three stacked sections that fill the entire viewport with no scrolling:

| Section | Height | Background |
|---------|--------|------------|
| Header | ~50px (auto, flex-shrink: 0) | White `#FFFFFF` |
| Timeline | 196px (fixed, flex-shrink: 0) | Light gray `#FAFAFA` |
| Heatmap | Remaining space (flex: 1) | White `#FFFFFF` |

### Section 1: Header Bar (`.hdr`)

- **Layout:** Horizontal flex, `align-items: center`, `justify-content: space-between`
- **Padding:** `12px 44px 10px`
- **Border:** Bottom `1px solid #E0E0E0`
- **Left side:**
  - Title: `<h1>` at 24px, font-weight 700, color `#111`. Contains project name and inline hyperlink to backlog (`color: #0078D4`, no underline)
  - Subtitle: 12px, color `#888`, margin-top 2px
- **Right side — Legend row:** Horizontal flex with 22px gap, 12px font size
  - PoC Milestone: 12×12px gold (`#F4B400`) square rotated 45° (diamond shape)
  - Production Release: 12×12px green (`#34A853`) square rotated 45° (diamond shape)
  - Checkpoint: 8×8px gray (`#999`) circle
  - Now indicator: 2×14px red (`#EA4335`) vertical bar + text "Now (Apr 2026)"

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Horizontal flex, `align-items: stretch`
- **Dimensions:** Full width, 196px height, `flex-shrink: 0`
- **Background:** `#FAFAFA`
- **Border:** Bottom `2px solid #E8E8E8`
- **Padding:** `6px 44px 0`

**Left Sidebar (Milestone Labels):**
- Width: 230px, flex-shrink: 0
- Vertical flex with `justify-content: space-around`
- Padding: `16px 12px 16px 0`
- Right border: `1px solid #E0E0E0`
- Each milestone: label in bold 12px colored text (milestone color), description in 12px `#444`

**Right Area (SVG Timeline — `.tl-svg-box`):**
- `flex: 1`, padding-left 12px, padding-top 6px
- Contains inline `<svg>` at width 1560, height 185, `overflow: visible`
- **Month grid lines:** Vertical lines at 260px intervals, stroke `#bbb` opacity 0.4
- **Month labels:** 11px, font-weight 600, color `#666`, positioned 5px right of each grid line
- **"NOW" line:** Vertical dashed line (`stroke-dasharray: 5,3`), stroke `#EA4335`, width 2. Position calculated from current date. Label "NOW" in 10px bold red.
- **Milestone tracks:** Horizontal `<line>` elements spanning full width, stroke = milestone color, stroke-width 3. Vertical spacing ~56px apart (y positions: 42, 98, 154)
- **Checkpoint markers:** `<circle>` with r=5-7, white fill, colored stroke (milestone color or `#888`), stroke-width 2.5
- **PoC diamonds:** `<polygon>` with 4 points forming 22×22px diamond, fill `#F4B400`, drop shadow filter
- **Production diamonds:** `<polygon>` same shape, fill `#34A853`, drop shadow filter
- **Event labels:** `<text>` at 10px, `text-anchor: middle`, fill `#666`, positioned above or below markers
- **Drop shadow filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3">`

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Layout:** Vertical flex column, `flex: 1`, `min-height: 0`
- **Padding:** `10px 44px 10px`

**Section Title (`.hm-title`):**
- Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"
- 14px, font-weight 700, color `#888`, letter-spacing 0.5px, uppercase, margin-bottom 8px

**Grid (`.hm-grid`):**
- CSS Grid: `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months
- `grid-template-rows: 36px repeat(4, 1fr)`
- Outer border: `1px solid #E0E0E0`
- `flex: 1`, `min-height: 0` (fills remaining viewport)

**Corner Cell (`.hm-corner`):**
- Background `#F5F5F5`, 11px bold uppercase, color `#999`
- Right border `1px solid #E0E0E0`, bottom border `2px solid #CCC`

**Column Headers (`.hm-col-hdr`):**
- 16px bold, background `#F5F5F5`, centered
- Borders: right `1px solid #E0E0E0`, bottom `2px solid #CCC`
- **Current month highlight (`.apr-hdr`):** background `#FFF0D0`, color `#C07700`

**Row Headers (`.hm-row-hdr`):**
- 11px bold uppercase, letter-spacing 0.7px
- Right border `2px solid #CCC`, bottom `1px solid #E0E0E0`
- Row-specific colors listed in US-3 acceptance criteria

**Data Cells (`.hm-cell`):**
- Padding `8px 12px`, overflow hidden
- Borders: right `1px solid #E0E0E0`, bottom `1px solid #E0E0E0`
- Items: 12px, color `#333`, padding `2px 0 2px 12px`, line-height 1.35
- Colored dot: 6×6px circle via `::before` pseudo-element, positioned `left: 0, top: 7px`
- Current month cells use the darker background variant per row category

### Typography

| Element | Size | Weight | Color |
|---------|------|--------|-------|
| Page title | 24px | 700 | `#111` |
| Subtitle | 12px | 400 | `#888` |
| Legend items | 12px | 400 | `#111` |
| Milestone labels | 12px | 600 | Milestone color |
| Timeline month labels | 11px | 600 | `#666` |
| Timeline event labels | 10px | 400 | `#666` |
| Heatmap section title | 14px | 700 | `#888` |
| Column headers | 16px | 700 | `#111` (or `#C07700` for current) |
| Row headers | 11px | 700 | Row-specific color |
| Cell items | 12px | 400 | `#333` |

### Color Palette

| Token | Hex | Usage |
|-------|-----|-------|
| Page background | `#FFFFFF` | Body |
| Primary text | `#111` | Headings |
| Secondary text | `#888` | Subtitles, section titles |
| Cell text | `#333` | Heatmap items |
| Link blue | `#0078D4` | Hyperlinks, In Progress dots |
| Timeline bg | `#FAFAFA` | Timeline area |
| Grid border | `#E0E0E0` | Cell borders |
| Header border | `#CCC` | Column/row header borders |
| Shipped green | `#34A853` | Dots, production diamonds |
| Shipped header bg | `#E8F5E9` | Row header |
| Shipped cell bg | `#F0FBF0` / `#D8F2DA` (current) | Data cells |
| Progress blue | `#0078D4` | Dots |
| Progress header bg | `#E3F2FD` | Row header |
| Progress cell bg | `#EEF4FE` / `#DAE8FB` (current) | Data cells |
| Carryover amber | `#F4B400` | Dots, PoC diamonds |
| Carryover header bg | `#FFF8E1` | Row header |
| Carryover cell bg | `#FFFDE7` / `#FFF0B0` (current) | Data cells |
| Blocker red | `#EA4335` | Dots, NOW line |
| Blocker header bg | `#FEF2F2` | Row header |
| Blocker cell bg | `#FFF5F5` / `#FFE4E4` (current) | Data cells |
| Current month header | `#FFF0D0` bg / `#C07700` text | Column highlight |

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `https://localhost:5001/`. The browser renders the full dashboard at 1920×1080. The header appears with project title, subtitle, backlog link, and legend. The timeline section shows all milestones with their events plotted at calculated positions. The heatmap grid fills the remaining space with all four status rows populated from `data.json`. The current month column is highlighted in gold. The "NOW" dashed red line appears at today's date position on the timeline.

**Scenario 2: User Views Milestone Timeline**
User looks at the timeline section and sees horizontal colored lines for each milestone (M1, M2, etc.) with labeled markers. Diamond shapes indicate PoC (gold) and Production (green) milestones. Open circles indicate checkpoints. The "NOW" line provides temporal context. Month grid lines help orient dates visually.

**Scenario 3: User Scans Heatmap for Current Month Status**
User's eye is drawn to the highlighted current-month column (gold header). They scan downward through Shipped (green), In Progress (blue), Carryover (amber), and Blockers (red) to get a complete picture of this month's execution status. Each item has a colored bullet dot matching its category.

**Scenario 4: User Hovers Over Backlog Link**
User hovers over the "→ ADO Backlog" link in the header. The cursor changes to a pointer (standard `<a>` behavior). Clicking navigates to the ADO backlog URL defined in `data.json`.

**Scenario 5: User Takes Screenshot for PowerPoint**
User presses Ctrl+Shift+S (Edge) or uses Snipping Tool to capture the full browser viewport. The fixed 1920×1080 layout ensures the entire dashboard fits in one screenshot with no clipping, scrollbars, or truncation. The screenshot is directly pasteable into a 16:9 PowerPoint slide.

**Scenario 6: Empty Data Scenario**
User provides a `data.json` with an empty `milestones` array and empty `heatmapRows[].items`. The header still renders with title and subtitle. The timeline area shows month grid lines but no milestone tracks. The heatmap grid shows column and row headers with dash (`-`) placeholders in all cells.

**Scenario 7: Malformed JSON Error State**
User saves invalid JSON to `data.json` and restarts the app. Instead of a blank page or unhandled exception, the browser displays a clear error message indicating the JSON parsing failure with the specific error detail.

**Scenario 8: Data Update Workflow**
User edits `data.json` to add a new shipped item to April. They restart the app (or `dotnet watch` triggers automatic reload). Refreshing the browser shows the updated item in the Shipped/April cell with the green bullet dot.

**Scenario 9: User Views Timeline with Dates Spanning Before Visible Range**
A milestone event has a date before the timeline's start month (e.g., "Dec 19" when January is the first month). The marker renders at or near the left edge of the SVG, and the label remains readable.

**Scenario 10: Multi-Milestone Vertical Layout**
User views a dashboard with 3 milestones. The timeline shows 3 horizontal lines evenly spaced vertically (~56px apart). Labels in the left sidebar align vertically with their corresponding timeline tracks.

## Scope

### In Scope

- Single-page Blazor Server dashboard matching `OriginalDesignConcept.html` visual design
- Header section with project title, subtitle, backlog link, and legend
- SVG timeline rendering with dynamically calculated milestone positions from `data.json`
- Color-coded heatmap grid with Shipped, In Progress, Carryover, and Blockers rows
- Current month column highlighting
- "NOW" vertical indicator line calculated from `DateTime.Today`
- `data.json` file as the sole data source with defined schema
- C# model classes (`DashboardConfig`, `Milestone`, `MilestoneEvent`, `HeatmapRow`)
- `DashboardDataService` for JSON deserialization
- Empty layout (no Blazor default nav/sidebar)
- Fixed 1920×1080 viewport for screenshot capture
- Fictional "Project Phoenix" sample data in `data.json`
- JSON validation with user-friendly error messages on malformed input
- Local-only hosting via Kestrel on localhost

### Out of Scope

- **Authentication/Authorization** — No login, no roles, no identity provider
- **Database** — No SQL, no Entity Framework, no data persistence beyond `data.json`
- **Responsive design** — Fixed viewport only; no mobile or tablet support
- **Export/Print buttons** — Manual browser screenshot is the workflow
- **Multi-project routing** — Single `data.json`, single dashboard (future enhancement)
- **Real-time collaboration** — Single user, local tool
- **Cloud deployment** — No Azure, no Docker, no CI/CD pipeline
- **Third-party UI libraries** — No MudBlazor, Radzen, Bootstrap, Chart.js, or similar
- **Dark mode/theming** — Single light theme matching the design reference
- **Historical data or versioning** — No dated snapshots or date picker
- **Telemetry or monitoring** — Console logging only
- **File watcher auto-refresh** — `dotnet watch` handles development; no runtime `data.json` hot-reload in v1
- **Automated testing** — No unit or integration tests for v1 (manual visual verification)
- **Accessibility (WCAG)** — Not a requirement for an internal screenshot tool
- **Internationalization** — English only

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Page load time (localhost) | < 1 second from `dotnet run` ready to full render |
| `data.json` deserialization | < 50ms for files up to 100KB |
| SVG timeline render | < 100ms for up to 10 milestones with 20 events each |
| Memory footprint | < 100MB for the running Kestrel process |

### Visual Fidelity

| Metric | Target |
|--------|--------|
| Viewport match | Exactly 1920×1080px, no scrollbars at 100% zoom |
| Design parity | Pixel-level match to `OriginalDesignConcept.html` at same viewport |
| Font rendering | Segoe UI on Windows; Arial fallback on Mac/Linux |
| Browser support | Chrome 120+ and Edge 120+ (screenshot-capture browsers) |

### Reliability

| Metric | Target |
|--------|--------|
| Startup success rate | 100% with valid `data.json` |
| Error handling | Graceful error page for malformed JSON — never a blank screen |
| Data schema validation | Fail-fast with descriptive message on missing required fields |

### Security

- No authentication or authorization required
- No network egress — all rendering is local
- HTTPS via Kestrel's default development certificate
- No user-generated content — all data is from trusted local `data.json`

### Maintainability

- Zero third-party NuGet dependencies beyond .NET 8 SDK built-ins
- Single `.razor` page — no component fragmentation
- CSS ported directly from the HTML reference for traceability
- `data.json` schema documented via C# model classes

## Success Metrics

| # | Metric | Target | Measurement |
|---|--------|--------|-------------|
| 1 | **Feature completeness** | All 3 visual sections render (header, timeline, heatmap) | Manual visual comparison against `OriginalDesignConcept.png` |
| 2 | **Data-driven rendering** | 100% of displayed content sourced from `data.json` | Change every field in JSON and verify UI updates |
| 3 | **Screenshot fidelity** | Full dashboard captured in one 1920×1080 screenshot with no clipping | Browser screenshot at 100% zoom |
| 4 | **Zero-friction startup** | `dotnet run` → browser → dashboard visible in < 10 seconds | Timed from command to rendered page |
| 5 | **JSON editability** | Non-developer can edit `data.json` to update dashboard content | Have a PM edit the file and verify the result |
| 6 | **Visual design match** | Dashboard is indistinguishable from `OriginalDesignConcept.html` at 1920×1080 | Side-by-side comparison at pixel level |
| 7 | **Error resilience** | Malformed JSON shows a helpful error, not a crash | Test with invalid JSON, empty file, and missing fields |

## Constraints & Assumptions

### Technical Constraints

- **Framework:** Must be Blazor Server on .NET 8 LTS (matches existing `AgentSquad` repository structure and team skillset)
- **No third-party UI packages:** Design is simple enough for native CSS + SVG; adding libraries increases bundle size and maintenance burden
- **Fixed viewport:** 1920×1080 only — the page is a screenshot tool, not a web application
- **Single file data source:** `data.json` is the entire data layer — no database, no API calls
- **Windows-primary:** Segoe UI font dependency; the tool is used on Windows machines for PowerPoint workflows
- **SVG rendering:** Timeline must use inline SVG (not Canvas or charting library) to match the reference design and enable Razor-based dynamic rendering

### Timeline Assumptions

- **MVP delivery:** 1–2 days of focused development (approximately 7 hours of coding)
- **Single developer:** One engineer builds the entire solution
- **No external dependencies:** No waiting on API access, design approvals, or infrastructure provisioning
- **Design is final:** The `OriginalDesignConcept.html` is the approved visual specification — no design iteration expected

### Dependency Assumptions

- Developer has .NET 8 SDK installed (`dotnet --version` ≥ 8.0.0)
- Developer has access to the `ReportingDashboard` GitHub repository for the HTML design reference
- Browser available for testing: Chrome or Edge, version 120+
- Windows machine with Segoe UI font installed (standard on all Windows 10/11 installations)

### Data Assumptions

- `data.json` will contain fewer than 10 milestones and fewer than 50 total events
- Heatmap will display 4–6 months (configurable via `months` array)
- Each heatmap cell will contain 0–8 items (design accommodates up to ~8 lines per cell at current row height)
- Dates in milestone events span a 6–12 month range
- The `currentMonth` value in `data.json` is a 3-letter abbreviation matching one entry in the `months` array