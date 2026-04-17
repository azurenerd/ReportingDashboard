# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, delivery status, and team progress in a format optimized for 1920×1080 PowerPoint screenshots. The dashboard reads all data from a local `data.json` configuration file and renders a pixel-perfect view using Blazor Server (.NET 8), requiring zero authentication, no database, and no cloud dependencies. This tool enables project leads to generate polished, consistent executive status slides in seconds by simply updating a JSON file and taking a browser screenshot.

## Business Goals

1. **Reduce executive reporting prep time by 80%** — eliminate manual PowerPoint slide formatting by providing a screenshot-ready dashboard that renders consistently every time.
2. **Standardize project status communication** — provide a single visual format that shows shipped items, in-progress work, carryover, and blockers across monthly columns, ensuring every executive review uses the same structure.
3. **Provide milestone timeline visibility** — give executives an at-a-glance view of major project milestones (PoC dates, production releases, checkpoints) plotted on a timeline with a clear "NOW" indicator.
4. **Enable data-driven updates with zero friction** — allow project leads to update a single JSON file to refresh the entire dashboard, with no code changes, no deployments, and no database migrations.
5. **Maintain simplicity for single-user local use** — keep infrastructure cost at $0 and complexity at the absolute minimum required for a local screenshot tool.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** to see the project title, subtitle (team/workstream/date), and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are viewing.

**Visual Reference:** Header section (`.hdr`) of `OriginalDesignConcept.html`

- [ ] Dashboard displays a bold 24px project title at the top-left
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, styled in `#0078D4` blue
- [ ] A subtitle line below the title shows team name, workstream, and current month in 12px gray (`#888`) text
- [ ] A legend appears at the top-right with four items: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line)
- [ ] All header values are driven from `data.json` fields: `title`, `subtitle`, `backlogUrl`

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major project milestones across multiple workstreams, **so that** I can quickly understand when key deliverables are planned and whether the project is on track relative to today.

**Visual Reference:** Timeline area (`.tl-area` and inline SVG) of `OriginalDesignConcept.html`

- [ ] Timeline renders as an SVG with month columns (Jan–Jun) separated by vertical grid lines
- [ ] A left sidebar (230px wide) lists milestone stream labels (e.g., M1: Chatbot & MS Role) with stream-specific colors
- [ ] Each milestone stream renders as a colored horizontal line with events plotted at date-proportional x-positions
- [ ] Checkpoint events render as gray circles (`#999`), PoC milestones as gold diamonds (`#F4B400`), and Production releases as green diamonds (`#34A853`)
- [ ] A red dashed vertical line (`#EA4335`) with "NOW" label marks the current date position
- [ ] Date labels appear above or below each event marker
- [ ] Diamond markers have a subtle drop shadow (`feDropShadow`)
- [ ] The timeline supports 1–5 milestone streams from `data.json`
- [ ] Event x-positions are calculated as: `x = (eventDate - startDate) / (endDate - startDate) * svgWidth`

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked — organized by month, **so that** I can assess project health and team velocity at a glance.

**Visual Reference:** Heatmap grid (`.hm-wrap`, `.hm-grid`) of `OriginalDesignConcept.html`

- [ ] Heatmap renders as a CSS Grid with columns: Status label (160px) + 4 month columns (equal width)
- [ ] A header row shows month names in bold 16px text; the current month column is highlighted with gold background (`#FFF0D0`) and gold text (`#C07700`)
- [ ] Four status rows display: ✅ Shipped (green), 🔵 In Progress (blue), 🟡 Carryover (amber), 🔴 Blockers (red)
- [ ] Each cell contains a bulleted list of status items (12px text with a 6px colored dot prefix)
- [ ] Row headers are uppercase, bold, 11px, with letter-spacing and category-specific background colors
- [ ] Current-month cells use a darker tinted background for visual emphasis
- [ ] Empty cells display a subtle "-" placeholder instead of blank space
- [ ] All items are driven from the `categories[].items` dictionary in `data.json`

### US-4: Load Dashboard Data from JSON

**As a** project lead, **I want** the dashboard to read all its data from a single `data.json` file, **so that** I can update project status by editing one file without touching any code.

- [ ] Application loads `data.json` from the project directory at startup
- [ ] JSON deserialization uses strongly-typed C# models (`DashboardData`, `MilestoneStream`, `TimelineEvent`, `StatusCategory`)
- [ ] If `data.json` is missing, the application logs a clear error message and displays a user-friendly error page
- [ ] If `data.json` has malformed JSON, the application logs the parse error with line/column info
- [ ] If required fields are missing (e.g., `title`, `categories`), the application logs a warning but renders what it can
- [ ] Changes to `data.json` are reflected on the next application restart (or page refresh with `dotnet watch`)

### US-5: Auto-Highlight Current Month

**As a** project lead, **I want** the dashboard to automatically highlight the current month's column in the heatmap, **so that** I don't need to manually update which month is "active" in the JSON file.

**Visual Reference:** `.apr-hdr` and `.apr` classes in `OriginalDesignConcept.html`

- [ ] The system computes the current month from `nowDate` in `data.json` (or system date if not specified)
- [ ] The matching month column header gets the gold highlight treatment (`#FFF0D0` background, `#C07700` text, "→ Now" suffix)
- [ ] All cells in the current month column receive a darker background tint per their category
- [ ] If `nowDate` falls outside the displayed months range, no column is highlighted

### US-6: View Dashboard as a Screenshot-Ready Page

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars or overflow, **so that** I can take a full-page browser screenshot and paste it directly into a PowerPoint slide.

- [ ] The `<body>` element is fixed at 1920px × 1080px with `overflow: hidden`
- [ ] All content fits within the viewport without scrolling
- [ ] The layout uses absolute pixel values and fixed proportions — no responsive breakpoints
- [ ] The page renders identically in Chrome and Edge at 100% zoom
- [ ] Font rendering uses Segoe UI (system font on Windows) — no web font downloads

### US-7: Hover Tooltip on Milestone Markers

**As an** executive viewing the dashboard on screen (not screenshot), **I want** to hover over a milestone diamond and see a tooltip with the milestone name, date, and type, **so that** I can get details without cluttering the visual.

**Visual Reference:** Enhancement over `OriginalDesignConcept.html` timeline section

- [ ] Hovering over a PoC or Production diamond displays a CSS-only tooltip (`:hover::after` pseudo-element)
- [ ] Tooltip shows: milestone label, date, and type (e.g., "Mar 26 — PoC Milestone")
- [ ] Tooltip has a white background, subtle border, and drop shadow
- [ ] Tooltip does not require JavaScript — pure CSS implementation
- [ ] Tooltip does not appear in screenshots (hover state only)

### US-8: Progress Summary Bar

**As an** executive, **I want** to see a thin horizontal progress bar under the header showing the proportional breakdown of shipped vs. in-progress vs. carryover vs. blocked items, **so that** I get an instant health signal before reading the details.

**Visual Reference:** Enhancement over `OriginalDesignConcept.html`, positioned between header and timeline

- [ ] A 6px-tall horizontal bar spans the full page width below the header
- [ ] Segments are colored: green (`#34A853`) for shipped, blue (`#0078D4`) for in-progress, amber (`#F4B400`) for carryover, red (`#EA4335`) for blockers
- [ ] Segment widths are proportional to the total item count across all months per category
- [ ] If all items are shipped, the bar is entirely green — providing a clear "healthy project" signal

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the `ReportingDashboard` repository. Engineers MUST consult this file and the rendered screenshot (`docs/design-screenshots/OriginalDesignConcept.png`) for pixel-accurate implementation.

### Overall Page Layout

- **Viewport:** Fixed 1920px × 1080px, `overflow: hidden`, white background (`#FFFFFF`)
- **Layout Direction:** Vertical flex column (`display: flex; flex-direction: column`)
- **Font Family:** `'Segoe UI', Arial, sans-serif`
- **Base Text Color:** `#111`
- **Three major vertical sections** fill the page top-to-bottom:
  1. Header (flex-shrink: 0, ~48px)
  2. Timeline area (flex-shrink: 0, 196px fixed height)
  3. Heatmap grid (flex: 1, fills remaining space)

### Section 1: Header (`.hdr`)

- **Layout:** Flexbox row, `justify-content: space-between`, `align-items: center`
- **Padding:** `12px 44px 10px`
- **Border:** Bottom `1px solid #E0E0E0`
- **Left side:**
  - Title: `<h1>` at 24px, font-weight 700, containing project name + inline `<a>` link in `#0078D4`
  - Subtitle: 12px, color `#888`, margin-top 2px
- **Right side — Legend bar:** Flexbox row with 22px gap, four legend items:
  - PoC Milestone: 12×12px gold (`#F4B400`) square rotated 45° (diamond shape)
  - Production Release: 12×12px green (`#34A853`) square rotated 45°
  - Checkpoint: 8×8px gray (`#999`) circle
  - Now indicator: 2×14px red (`#EA4335`) vertical bar

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox row, `align-items: stretch`
- **Height:** 196px fixed
- **Background:** `#FAFAFA`
- **Border:** Bottom `2px solid #E8E8E8`
- **Padding:** `6px 44px 0`

#### Left Sidebar (Milestone Labels)
- **Width:** 230px, flex-shrink: 0
- **Border:** Right `1px solid #E0E0E0`
- **Content:** Vertically distributed milestone stream labels (e.g., "M1", "Chatbot & MS Role")
- **Label styling:** 12px, font-weight 600, stream color for ID, `#444` for description
- **Stream colors (from reference):** M1 = `#0078D4`, M2 = `#00897B`, M3 = `#546E7A`

#### SVG Timeline (`.tl-svg-box`)
- **SVG Dimensions:** 1560px × 185px, `overflow: visible`
- **Month grid lines:** Vertical lines at 260px intervals, stroke `#bbb` at 0.4 opacity
- **Month labels:** 11px, font-weight 600, color `#666`, positioned 5px right of grid line
- **NOW line:** Dashed vertical red line (`#EA4335`, stroke-width 2, dasharray `5,3`), with bold "NOW" label
- **Milestone streams:** Horizontal lines (stroke-width 3) at y-positions spaced ~56px apart (y=42, y=98, y=154)
- **Event markers:**
  - Checkpoint: `<circle>` r=5-7, white fill, colored or gray stroke (2.5px)
  - PoC: `<polygon>` diamond shape, fill `#F4B400`, with drop shadow filter
  - Production: `<polygon>` diamond shape, fill `#34A853`, with drop shadow filter
- **Drop shadow filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`
- **Date labels:** 10px, color `#666`, `text-anchor: middle`, positioned above or below markers

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Padding:** `10px 44px 10px`
- **Layout:** Flex column, `flex: 1; min-height: 0`
- **Title bar:** 14px, font-weight 700, color `#888`, uppercase, letter-spacing 0.5px, margin-bottom 8px

#### Grid Structure (`.hm-grid`)
- **Display:** CSS Grid
- **Columns:** `160px repeat(4, 1fr)` — status label + 4 month columns
- **Rows:** `36px repeat(4, 1fr)` — header row + 4 status rows
- **Border:** `1px solid #E0E0E0`

#### Column Headers
- **Corner cell (`.hm-corner`):** Background `#F5F5F5`, 11px bold uppercase `#999`, border-right `1px solid #E0E0E0`, border-bottom `2px solid #CCC`
- **Month headers (`.hm-col-hdr`):** Background `#F5F5F5`, 16px bold, centered, border-right `1px solid #E0E0E0`, border-bottom `2px solid #CCC`
- **Current month header (`.apr-hdr`):** Background `#FFF0D0`, color `#C07700`

#### Status Rows — Color Specifications

| Status | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Bullet Dot |
|--------|--------------|-----------------|---------|----------------------|------------|
| Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers (`.hm-row-hdr`)
- **Font:** 11px, bold, uppercase, letter-spacing 0.7px
- **Padding:** `0 12px`
- **Border:** Right `2px solid #CCC`, bottom `1px solid #E0E0E0`

#### Data Cells (`.hm-cell`)
- **Padding:** `8px 12px`
- **Border:** Right `1px solid #E0E0E0`, bottom `1px solid #E0E0E0`
- **Items (`.it`):** 12px, color `#333`, padding `2px 0 2px 12px`, line-height 1.35
- **Bullet dots:** `::before` pseudo-element, 6×6px circle, positioned at left:0, top:7px, colored per status category

### CSS Custom Properties (Color Tokens)

All colors MUST be defined as CSS custom properties in `app.css` `:root` for maintainability:

```
--color-shipped: #34A853          --color-shipped-bg: #F0FBF0
--color-shipped-bg-current: #D8F2DA   --color-shipped-header: #E8F5E9
--color-progress: #0078D4         --color-progress-bg: #EEF4FE
--color-progress-bg-current: #DAE8FB  --color-progress-header: #E3F2FD
--color-carryover: #F4B400        --color-carryover-bg: #FFFDE7
--color-carryover-bg-current: #FFF0B0 --color-carryover-header: #FFF8E1
--color-blocker: #EA4335          --color-blocker-bg: #FFF5F5
--color-blocker-bg-current: #FFE4E4   --color-blocker-header: #FEF2F2
--color-border: #E0E0E0           --color-text-primary: #111
--color-text-secondary: #888      --color-link: #0078D4
```

### Component Hierarchy

```
Dashboard.razor (page)
├── DashboardHeader.razor          → .hdr section
├── ProgressSummaryBar.razor       → 6px colored bar (enhancement)
├── TimelineSection.razor          → .tl-area section
│   ├── Milestone labels sidebar   → left 230px panel
│   └── SVG timeline               → generated MarkupString
└── HeatmapGrid.razor             → .hm-wrap section
    └── HeatmapRow.razor × 4      → one per status category
        └── HeatmapCell.razor × 4 → one per month column
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Full Dashboard Render**
User navigates to `http://localhost:5000`. The application loads `data.json`, deserializes it into the data model, and renders the complete dashboard: header with title/subtitle/legend, progress summary bar, milestone timeline SVG with all streams and events plotted, and the 4×4 heatmap grid with all status items. The current month column is auto-highlighted. Total render time is under 500ms.

**Scenario 2: User Views Milestone Timeline**
User looks at the timeline section and sees three horizontal colored lines (M1, M2, M3) with milestone markers at date-proportional positions. A red dashed "NOW" line indicates the current date. Diamonds (PoC in gold, Production in green) and circles (checkpoints in gray) appear along each stream with date labels. The left sidebar labels each stream with its ID and description.

**Scenario 3: User Hovers Over a Milestone Diamond**
User moves the mouse over a gold PoC diamond on the M2 timeline. A CSS tooltip appears showing "Mar 20 — PoC Milestone" with a white background, `#E0E0E0` border, and subtle shadow. The tooltip disappears when the mouse leaves the diamond. No JavaScript executes.

**Scenario 4: User Reads the Heatmap for Current Month**
User looks at the heatmap grid and immediately identifies the current month column by its gold-highlighted header (`#FFF0D0` background, `#C07700` text). Each cell in that column has a slightly darker tint than non-current months. The user scans shipped items (green dots), in-progress items (blue dots), carryover items (amber dots), and blockers (red dots).

**Scenario 5: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser opens the URL specified in `data.json.backlogUrl` in a new tab. The dashboard remains unchanged.

**Scenario 6: Data-Driven Rendering — Variable Item Counts**
The JSON file has 5 shipped items in March but only 1 in January. The heatmap cells render all items with proper bullet formatting. Cells auto-size within their grid row, and overflow is hidden to maintain the fixed 1080px height.

**Scenario 7: Empty State — No Items for a Month/Category**
The JSON file has no blocker items for January. The Blockers row's January cell displays a subtle gray dash ("-") in muted text (`#AAA`) instead of blank space, so executives know data isn't missing — there are genuinely no blockers.

**Scenario 8: Error State — Missing data.json**
User starts the application but `data.json` does not exist in the expected directory. The application logs: `ERROR: data.json not found at [path]. Please create a data.json file.` The browser displays a simple error message: "Dashboard data not found. Please ensure data.json exists in the application directory."

**Scenario 9: Error State — Malformed JSON**
User edits `data.json` and introduces a syntax error (e.g., trailing comma). On next startup, the application logs the JSON parse error with line/column information. The browser displays: "Failed to load dashboard data. Check application logs for details."

**Scenario 10: User Reads the Progress Summary Bar**
User glances at the thin colored bar between the header and timeline. The bar shows ~40% green (shipped), ~30% blue (in-progress), ~20% amber (carryover), ~10% red (blockers), giving an immediate visual health signal before reading any details.

**Scenario 11: User Takes a Screenshot**
User presses F12, sets the browser viewport to 1920×1080, and captures a screenshot. The resulting image is pixel-perfect with no scrollbars, no browser chrome artifacts, and no hover states visible. The image pastes cleanly into a 16:9 PowerPoint slide.

**Scenario 12: User Updates data.json and Refreshes**
User edits `data.json` to add a new shipped item for April. If running with `dotnet watch`, the application hot-reloads and the browser reflects the change on next page load. If running with `dotnet run`, the user restarts the application and refreshes the browser.

## Scope

### In Scope

- Single-page Blazor Server dashboard matching `OriginalDesignConcept.html` layout
- JSON-driven data model (`data.json`) with strongly-typed C# deserialization
- Header section with title, subtitle, backlog link, and legend
- SVG milestone timeline with 1–5 streams, checkpoint/PoC/production markers, and NOW line
- Monthly execution heatmap with 4 status rows (Shipped, In Progress, Carryover, Blockers) × 4 month columns
- Auto-detection and highlighting of current month column
- CSS-only hover tooltips on milestone diamonds
- Progress summary bar showing proportional status breakdown
- Empty state handling (dash placeholder for empty cells)
- Startup validation with clear error logging for missing/malformed JSON
- CSS custom properties for all color tokens
- Fixed 1920×1080 layout optimized for screenshot capture
- Fictional sample data for demonstration ("Project Phoenix" or similar)
- `dotnet watch` hot-reload support during development

### Out of Scope

- ❌ Authentication or authorization of any kind
- ❌ Database (SQLite, SQL Server, CosmosDB, or otherwise)
- ❌ REST/GraphQL API endpoints
- ❌ Admin UI for editing data
- ❌ Responsive or mobile layout
- ❌ Dark mode
- ❌ Logging infrastructure (Serilog, Application Insights)
- ❌ Docker containerization
- ❌ CI/CD pipeline
- ❌ Multi-user support or concurrent access
- ❌ Real-time data refresh or WebSocket push updates
- ❌ Azure DevOps API integration (future enhancement)
- ❌ Playwright screenshot automation (future enhancement)
- ❌ Print/PDF stylesheet (future enhancement)
- ❌ Multi-project support or project switcher (future enhancement)
- ❌ Third-party component libraries (MudBlazor, Radzen, etc.)
- ❌ JavaScript interop of any kind
- ❌ Unit tests or integration tests (MVP; add in Phase 2 if warranted)

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Page load time (cold start) | < 2 seconds from `dotnet run` to rendered page |
| Page render time (warm) | < 500ms from browser refresh to full paint |
| JSON deserialization | < 50ms for files up to 100KB |
| Memory footprint | < 100MB RSS for the running application |

### Security

- Application binds exclusively to `http://localhost:5000` — not accessible from network
- No PII or credentials stored in `data.json`
- No external network calls — fully air-gapped operation
- `data.json` should be added to `.gitignore` if it contains sensitive project names

### Reliability

- Application must start and render correctly with any valid `data.json` conforming to the schema
- Application must fail gracefully with clear error messages for invalid JSON
- No crash on empty arrays, missing optional fields, or extra unknown fields in JSON

### Compatibility

- **Primary:** Microsoft Edge (Chromium) and Google Chrome, latest stable
- **Secondary:** Firefox latest stable
- **Not supported:** Safari, mobile browsers, IE
- **Viewport:** Fixed 1920×1080 only — no responsive behavior required

### Maintainability

- All colors defined as CSS custom properties — no hardcoded hex values in component CSS
- Component structure maps 1:1 to visual sections for easy modification
- JSON schema is self-documenting with clear field names

## Success Metrics

1. **Visual Fidelity:** The rendered dashboard is visually indistinguishable from `OriginalDesignConcept.html` when compared side-by-side at 1920×1080 (header, timeline, and heatmap sections all match layout, colors, and typography).
2. **Screenshot Quality:** A browser screenshot at 1920×1080 can be pasted into a 16:9 PowerPoint slide with no cropping, scrollbars, or artifacts visible.
3. **Data Flexibility:** Changing any value in `data.json` (title, items, dates, milestone streams) and restarting the application reflects the change correctly on the dashboard.
4. **Time to Update:** A project lead can update `data.json` with new monthly status data and capture a fresh screenshot in under 5 minutes.
5. **Zero External Dependencies:** The application runs with `dotnet run` and zero NuGet packages beyond the default Blazor Server template. No npm, no Node.js, no external services.
6. **Delivery Timeline:** MVP (pixel-perfect dashboard with sample data) delivered within 1 day. Polish enhancements (tooltips, summary bar, auto-highlight) delivered within 2 days total.

## Constraints & Assumptions

### Technical Constraints

- **Framework:** Must use C# .NET 8 Blazor Server as specified by the project stack
- **No JavaScript:** All interactivity (tooltips, hover states) must be CSS-only — no JS interop
- **Fixed Viewport:** Layout is 1920×1080 pixels, non-responsive — this is by design for screenshot consistency
- **System Font:** Segoe UI only — no web font loading (assumes Windows environment)
- **Localhost Only:** Application runs on `http://localhost:5000` — no network exposure

### Timeline Assumptions

- **Day 1:** Core dashboard rendering — header, timeline SVG, heatmap grid, JSON loading, sample data
- **Day 2:** Polish — auto-highlight current month, tooltips, progress bar, empty states, validation
- **Single developer** working full-time on implementation
- **No design iteration cycles** — the reference HTML is the final approved design

### Dependency Assumptions

- .NET 8 SDK is installed on the developer's machine
- Developer has access to the `ReportingDashboard` GitHub repository for the reference HTML file
- Windows environment with Segoe UI font available
- Edge or Chrome browser available for screenshot capture
- No external APIs, services, or databases are needed at any point

### Data Assumptions

- `data.json` will be manually maintained by the project lead (weekly or monthly updates)
- The JSON schema is stable — the four status categories (Shipped, In Progress, Carryover, Blockers) and monthly column structure will not change for MVP
- Maximum 5 milestone streams, maximum 4 displayed months, maximum ~10 items per cell (content overflow is hidden)
- `data.json` file size will remain under 100KB
- The fictional sample data ("Project Phoenix" or similar) will ship with the application as a working example