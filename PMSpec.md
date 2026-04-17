# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones on a timeline and displays a monthly execution heatmap of work items by status (Shipped, In Progress, Carryover, Blockers). The dashboard reads all data from a local `data.json` configuration file and renders a pixel-perfect 1920×1080 layout optimized for screenshot capture into PowerPoint executive decks. Built as a minimal .NET 8 Blazor Server application with zero external dependencies, no authentication, and no database.

## Business Goals

1. **Reduce executive reporting prep time** — Enable project leads to generate polished, consistent project status visuals by editing a simple JSON file and taking a browser screenshot, eliminating manual PowerPoint chart building.
2. **Standardize project visibility** — Provide a repeatable visual format that communicates milestone progress, shipped work, active work, carryover debt, and blockers in a single glanceable view.
3. **Ensure screenshot fidelity** — Produce a fixed 1920×1080 rendered page that translates 1:1 into PowerPoint slides without resizing, cropping, or reformatting artifacts.
4. **Minimize operational overhead** — Deliver a zero-infrastructure tool that runs locally via `dotnet run` with no cloud services, databases, authentication, or ongoing maintenance costs.
5. **Enable rapid data updates** — Allow project leads to update project status by editing a human-readable `data.json` file and refreshing the browser, with no code changes required.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** to see the project title, organizational context, current month, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately understand which project and time period they are viewing.

*Visual Reference: Header section (`.hdr`) of `OriginalDesignConcept.html`*

- [ ] Dashboard displays the project title in bold 24px font at the top-left
- [ ] Subtitle shows the organization, workstream, and current month below the title in 12px gray text
- [ ] An optional ADO Backlog hyperlink appears inline with the title
- [ ] A legend appears at the top-right showing icons for: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now marker (red vertical line)
- [ ] All header values are driven from `data.json` — changing the JSON changes the display

### US-2: View Milestone Timeline

**As an** executive reviewer, **I want** to see a horizontal timeline with milestone swimlanes showing key dates, PoC milestones, production releases, and checkpoints, **so that** I can quickly assess where the project stands relative to its planned milestones.

*Visual Reference: Timeline area (`.tl-area` and inline SVG) of `OriginalDesignConcept.html`*

- [ ] Timeline renders as an inline SVG spanning the full width of the content area (~1560px)
- [ ] A left-side label column (230px) lists each milestone by ID (e.g., M1, M2, M3) with a short name and distinct color
- [ ] Each milestone has a horizontal colored swimlane line spanning the full timeline width
- [ ] Checkpoints render as open circles on the swimlane at the correct date position
- [ ] PoC milestones render as gold diamond markers (`#F4B400`) with drop shadow
- [ ] Production releases render as green diamond markers (`#34A853`) with drop shadow
- [ ] Month gridlines appear as light vertical lines with month labels (Jan, Feb, Mar, etc.)
- [ ] A red dashed vertical "NOW" line indicates the current date position
- [ ] Date labels appear above or below each marker showing the date and milestone type
- [ ] All milestone data (dates, types, labels, colors) is driven from `data.json`
- [ ] Adding or removing milestones in `data.json` adds or removes swimlane rows

### US-3: View Monthly Execution Heatmap

**As an** executive reviewer, **I want** to see a grid showing what was Shipped, In Progress, Carried Over, and Blocked for each month, **so that** I can assess execution velocity and identify persistent blockers.

*Visual Reference: Heatmap grid (`.hm-wrap`, `.hm-grid`) of `OriginalDesignConcept.html`*

- [ ] Heatmap renders as a CSS Grid with a status label column (160px) and one column per month
- [ ] A section title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase gray text
- [ ] Four status rows display: ✅ Shipped (green), 🔵 In Progress (blue), 🟡 Carryover (amber), 🔴 Blockers (red)
- [ ] Each cell lists work items as bullet-pointed text entries with a colored dot prefix matching the row status
- [ ] The current month column is visually highlighted with a warmer background tint and a "◀ Now" indicator in the column header
- [ ] Empty cells for future months display a gray dash placeholder
- [ ] Row headers use uppercase text with the status emoji/icon and category name
- [ ] Column headers display month names in bold 16px text
- [ ] All work items and month columns are driven from `data.json`
- [ ] Adding months to `data.json` adds columns; adding items populates cells

### US-4: Configure Dashboard via JSON File

**As a** project lead, **I want** to edit a single `data.json` file to update all dashboard content (title, milestones, work items, months), **so that** I can refresh the dashboard without touching any code.

- [ ] A `data.json` file exists in `wwwroot/data/data.json`
- [ ] The JSON schema supports: title, subtitle, backlogUrl, currentMonth, months array, milestones array (with events), and categories array (with items per month)
- [ ] The application deserializes `data.json` on startup using `System.Text.Json`
- [ ] Invalid or missing JSON fields produce a clear error message in the browser, not a crash
- [ ] A sample `data.json` with fictional project data is included as a starting template
- [ ] The `data.json` file includes a `schemaVersion` field for future compatibility

### US-5: Capture Screenshot for PowerPoint

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrolling, **so that** I can capture a full-page screenshot and paste it directly into a PowerPoint slide without cropping or resizing.

- [ ] The page body is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] All content fits within the viewport without scrollbars at 1920×1080
- [ ] The page includes `<meta name="viewport" content="width=1920">` for consistent rendering
- [ ] Screenshots taken via Windows Snipping Tool (`Win+Shift+S`) or Chrome DevTools full-page capture produce a clean, complete image
- [ ] No Blazor loading spinners, connection indicators, or error banners appear in the captured screenshot

### US-6: Run Dashboard Locally

**As a** developer, **I want** to start the dashboard with a single `dotnet run` command and access it at `http://localhost:5000`, **so that** there is zero deployment or infrastructure setup required.

- [ ] `dotnet run` starts the application without errors
- [ ] The dashboard is accessible at `http://localhost:5000` (or `https://localhost:5001`)
- [ ] No external services, databases, or API keys are required
- [ ] `dotnet watch` enables live reload during development
- [ ] A `README.md` explains how to edit `data.json`, run the app, and take screenshots

## Visual Design Specification

> **Canonical Design Reference:** `OriginalDesignConcept.html` from the `ReportingDashboard` repository. Engineers MUST consult this file and match its visual output exactly. See also: rendered screenshot at `docs/design-screenshots/OriginalDesignConcept.png`.

![OriginalDesignConcept design reference](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/main/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Page Layout

- **Dimensions:** Fixed 1920×1080px, no scrolling, `overflow: hidden`
- **Background:** `#FFFFFF` (white)
- **Font Family:** `'Segoe UI', Arial, sans-serif`
- **Base Text Color:** `#111`
- **Layout Direction:** Vertical flex column (`display: flex; flex-direction: column`)
- **Horizontal Padding:** 44px on left and right for all major sections

The page is divided into three stacked sections from top to bottom:

```
┌──────────────────────────────────────────────────────┐
│  HEADER (fixed height, ~50px)                        │
├──────────────────────────────────────────────────────┤
│  TIMELINE AREA (fixed height, 196px)                 │
├──────────────────────────────────────────────────────┤
│  HEATMAP GRID (flex: 1, fills remaining space)       │
└──────────────────────────────────────────────────────┘
```

### Section 1: Header (`.hdr`)

- **Height:** Auto (approximately 50px)
- **Padding:** `12px 44px 10px`
- **Border:** 1px solid `#E0E0E0` bottom
- **Layout:** Flexbox row, `justify-content: space-between; align-items: center`

**Left Side:**
- **Title (`h1`):** 24px, font-weight 700, color `#111`. Includes inline hyperlink to ADO backlog in `#0078D4` (no underline)
- **Subtitle (`.sub`):** 12px, color `#888`, margin-top 2px. Format: "Organization · Workstream · Month Year"

**Right Side (Legend):**
- Horizontal flex row with 22px gap between items
- Each legend item is a flex row with 6px gap: icon + 12px label text
- **PoC Milestone:** 12×12px square, background `#F4B400`, rotated 45° (diamond shape)
- **Production Release:** 12×12px square, background `#34A853`, rotated 45° (diamond shape)
- **Checkpoint:** 8×8px circle, background `#999`
- **Now Marker:** 2×14px rectangle, background `#EA4335`

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed 196px
- **Background:** `#FAFAFA`
- **Padding:** `6px 44px 0`
- **Border:** 2px solid `#E8E8E8` bottom
- **Layout:** Flexbox row, `align-items: stretch`

**Left Label Column (230px fixed width):**
- Flex column, `justify-content: space-around`
- Padding: `16px 12px 16px 0`
- Border-right: 1px solid `#E0E0E0`
- Each milestone label: 12px font-size, font-weight 600, line-height 1.4
  - ID line (e.g., "M1") in milestone color
  - Name line (e.g., "Chatbot & MS Role") in `#444`, font-weight 400
- **Milestone colors from reference:** M1: `#0078D4` (blue), M2: `#00897B` (teal), M3: `#546E7A` (blue-gray)

**Right SVG Timeline (flex: 1):**
- SVG element: width 1560px, height 185px, `overflow: visible`
- **Month Gridlines:** Vertical lines at 260px intervals (0, 260, 520, 780, 1040, 1300), stroke `#bbb`, opacity 0.4, width 1px
- **Month Labels:** Positioned 5px right of each gridline, y=14, fill `#666`, font-size 11px, font-weight 600
- **"NOW" Line:** Vertical dashed line (stroke `#EA4335`, width 2px, dash pattern "5,3") at the calculated x-position of the current date. "NOW" label in 10px bold red text
- **Swimlane Lines:** Horizontal lines spanning full width at y=42, y=98, y=154 (evenly spaced for 3 milestones), stroke width 3px, color matching milestone
- **Checkpoint Markers:** `<circle>` elements, radius 5-7px, white fill with colored stroke (width 2.5px). Small checkpoints: radius 4px, solid `#999` fill
- **PoC Diamond Markers:** `<polygon>` with 4 points forming a diamond (11px radius), fill `#F4B400`, with drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`)
- **Production Diamond Markers:** Same polygon shape, fill `#34A853`, same drop shadow
- **Date Labels:** 10px text, fill `#666`, `text-anchor: middle`, positioned above or below markers (alternating to avoid overlap)
- **X-axis Mapping Formula:** `x = ((date - timelineStartDate) / (timelineEndDate - timelineStartDate)) * 1560`

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Flex:** 1 (fills all remaining vertical space)
- **Padding:** `10px 44px 10px`
- **Layout:** Flex column

**Section Title (`.hm-title`):**
- Font-size 14px, font-weight 700, color `#888`
- Letter-spacing 0.5px, text-transform uppercase
- Margin-bottom 8px
- Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"

**Grid (`.hm-grid`):**
- CSS Grid: `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months
- CSS Grid: `grid-template-rows: 36px repeat(4, 1fr)`
- Border: 1px solid `#E0E0E0`
- Flex: 1 (fills remaining space within `.hm-wrap`)

**Corner Cell (`.hm-corner`):**
- Background `#F5F5F5`, 11px bold uppercase text, color `#999`
- Text: "STATUS"
- Border-right: 1px solid `#E0E0E0`, border-bottom: 2px solid `#CCC`

**Column Headers (`.hm-col-hdr`):**
- Background `#F5F5F5`, 16px bold text, centered
- Border-right: 1px solid `#E0E0E0`, border-bottom: 2px solid `#CCC`
- **Current month highlight (`.apr-hdr`):** Background `#FFF0D0`, text color `#C07700`, label appended with "◀ Now"

**Row Headers (`.hm-row-hdr`):**
- 11px bold uppercase text, letter-spacing 0.7px
- Padding: `0 12px`
- Border-right: 2px solid `#CCC`, border-bottom: 1px solid `#E0E0E0`

**Row Color Scheme (four status rows):**

| Status | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Bullet Color |
|--------|--------------|----------------|---------|----------------------|-------------|
| ✅ Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| 🔵 In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| 🟡 Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| 🔴 Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

**Data Cells (`.hm-cell`):**
- Padding: `8px 12px`
- Border-right: 1px solid `#E0E0E0`, border-bottom: 1px solid `#E0E0E0`
- Overflow: hidden

**Work Items (`.it`):**
- Font-size 12px, color `#333`, line-height 1.35
- Padding: `2px 0 2px 12px` (indent for bullet)
- `::before` pseudo-element: 6×6px circle at left=0, top=7px, colored per row status
- Empty/future cells: single item with text "–" in color `#AAA`

### CSS Custom Properties (Recommended Enhancement)

```css
:root {
  --color-shipped: #34A853;
  --color-shipped-bg: #F0FBF0;
  --color-shipped-bg-current: #D8F2DA;
  --color-shipped-hdr-bg: #E8F5E9;
  --color-shipped-hdr-text: #1B7A28;
  --color-progress: #0078D4;
  --color-progress-bg: #EEF4FE;
  --color-progress-bg-current: #DAE8FB;
  --color-progress-hdr-bg: #E3F2FD;
  --color-progress-hdr-text: #1565C0;
  --color-carry: #F4B400;
  --color-carry-bg: #FFFDE7;
  --color-carry-bg-current: #FFF0B0;
  --color-carry-hdr-bg: #FFF8E1;
  --color-carry-hdr-text: #B45309;
  --color-block: #EA4335;
  --color-block-bg: #FFF5F5;
  --color-block-bg-current: #FFE4E4;
  --color-block-hdr-bg: #FEF2F2;
  --color-block-hdr-text: #991B1B;
  --color-now: #EA4335;
  --color-current-month-hdr-bg: #FFF0D0;
  --color-current-month-hdr-text: #C07700;
  --color-link: #0078D4;
  --color-border: #E0E0E0;
  --color-border-heavy: #CCC;
}
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The dashboard renders the complete page in a single paint — header, timeline, and heatmap — all populated from `data.json`. No loading spinner, skeleton screen, or progressive rendering is visible. The page appears fully formed within 1 second.

**Scenario 2: User Views the Project Header**
User sees the project title ("Privacy Automation Release Roadmap") in the top-left with a clickable ADO Backlog link. The subtitle shows the organizational context and current month. On the right, a legend explains the four marker types used in the timeline. All text is crisp and readable at 1920×1080.

**Scenario 3: User Reads the Milestone Timeline**
User scans the timeline section and sees three horizontal swimlane lines (M1, M2, M3) with distinct colors. Diamond markers indicate PoC milestones (gold) and Production releases (green). Open circles indicate checkpoints. A red dashed "NOW" line shows the current date position. Date labels next to each marker provide exact dates and milestone descriptions.

**Scenario 4: User Hovers Over a Milestone Diamond**
User hovers over a gold PoC diamond marker on the timeline. The browser's default SVG hover behavior applies (cursor changes to pointer if the element is within an `<a>` tag). No custom tooltip is implemented in v1 — the date label adjacent to the diamond provides the needed context. *(Future enhancement: add SVG `<title>` elements for native browser tooltips.)*

**Scenario 5: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser navigates to the configured Azure DevOps backlog URL (from `data.json`). This link is functional when viewing in-browser but non-functional when captured as a PowerPoint screenshot (expected behavior).

**Scenario 6: User Scans the Heatmap for Current Month**
User's eye is drawn to the highlighted current-month column (warm gold background `#FFF0D0` header, brighter cell backgrounds). The "◀ Now" indicator in the column header confirms this is the active month. The user reads items vertically within the column to understand current execution status.

**Scenario 7: User Identifies Blockers**
User looks at the bottom row (Blockers, red) and scans across months. Red-tinted cells with `#EA4335` bullet dots immediately stand out. If the current month has blockers, the cell uses a more saturated red background (`#FFE4E4`) to draw attention.

**Scenario 8: User Assesses Carryover Trends**
User compares the Carryover row (amber) across months to identify whether unfinished work is accumulating. Items appearing in multiple consecutive month cells indicate persistent carryover that requires attention.

**Scenario 9: Data-Driven Rendering — Months Change**
Project lead adds "May" and "Jun" to the `months` array in `data.json` and adds corresponding items in each category. After restarting the app and refreshing the browser, the heatmap grid expands to 6 columns. The CSS Grid `repeat(N, 1fr)` calculation adjusts automatically. The timeline already shows 6 months of gridlines.

**Scenario 10: Data-Driven Rendering — Milestone Added**
Project lead adds a fourth milestone "M4" to the `milestones` array in `data.json` with its own events. After refresh, the timeline label column shows four entries, the SVG renders a fourth swimlane line, and the vertical spacing adjusts to accommodate the new row (each swimlane y-position recalculated as `height / (milestoneCount + 1) * index`).

**Scenario 11: Empty State — No Items in a Cell**
A heatmap cell for a future month has no items in `data.json`. The cell renders with the appropriate background color and displays a single gray dash "–" in `#AAA` text, indicating no data rather than showing a blank cell.

**Scenario 12: Error State — Missing or Invalid `data.json`**
The `data.json` file is missing or contains invalid JSON. The application displays a user-friendly error message on the dashboard page (e.g., "Unable to load dashboard data. Check that data.json exists and contains valid JSON.") instead of an unhandled exception page. The error message is styled consistently with the dashboard font and padding.

**Scenario 13: Error State — Partial Data**
Some fields in `data.json` are missing (e.g., `backlogUrl` is null). The dashboard renders with sensible defaults — the backlog link is hidden, not broken. Nullable C# POCO properties ensure graceful degradation.

**Scenario 14: Screenshot Capture Workflow**
User opens Chrome, navigates to `http://localhost:5000`, presses `Ctrl+Shift+P` in DevTools, types "Capture full size screenshot," and saves the PNG. The resulting image is exactly 1920×1080 pixels with no Blazor reconnection banners, no scrollbars, and no cut-off content. User pastes the image into PowerPoint and it fills the slide perfectly.

**Scenario 15: Responsive Behavior (Intentionally None)**
User resizes the browser window to less than 1920px wide. The page does NOT reflow. Content is clipped by `overflow: hidden`. This is by design — the dashboard is optimized for a fixed 1920×1080 viewport, not responsive viewing.

## Scope

### In Scope

- Single-page Blazor Server dashboard rendered at fixed 1920×1080px
- Header section with project title, subtitle, ADO backlog link, and marker legend
- SVG milestone timeline with colored swimlane lines, checkpoint circles, PoC diamonds, production diamonds, month gridlines, and "NOW" marker
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) and data-driven month columns
- Current-month column highlighting with distinct background colors
- All data driven from a single `data.json` file using `System.Text.Json` deserialization
- C# POCO data models for dashboard schema
- `DashboardDataService` singleton for reading and caching `data.json`
- Blazor component decomposition: `Dashboard.razor`, `DashboardHeader.razor`, `TimelineSection.razor`, `HeatmapGrid.razor`
- CSS ported from `OriginalDesignConcept.html` with CSS custom properties for theming
- Sample `data.json` with fictional project data
- `README.md` with instructions for editing data, running the app, and capturing screenshots
- Graceful error handling for missing or malformed `data.json`
- Static SSR rendering mode (no SignalR WebSocket for read-only content)

### Out of Scope

- ❌ Authentication or authorization of any kind
- ❌ Database, ORM, or Entity Framework integration
- ❌ REST API or GraphQL endpoints
- ❌ Real-time updates, SignalR push notifications, or WebSocket interactivity
- ❌ Responsive or mobile layout — fixed 1920×1080 only
- ❌ Admin panel or in-browser data editing UI
- ❌ PDF, PNG, or automated export functionality (manual screenshots only)
- ❌ Caching layer beyond in-memory singleton
- ❌ Structured logging framework (console output is sufficient)
- ❌ Docker containerization or Kubernetes deployment
- ❌ CI/CD pipeline or automated builds
- ❌ Azure DevOps API integration (link is static, not live-queried)
- ❌ Multi-project support or URL-based project switching (v1 is single-project)
- ❌ Hot-reload of `data.json` via FileSystemWatcher (restart/refresh is sufficient)
- ❌ Custom tooltips or interactive hover states beyond browser defaults
- ❌ Print-specific CSS media queries
- ❌ Accessibility (WCAG) compliance (screenshot-only output)
- ❌ Internationalization or localization
- ❌ Unit tests or component tests (optional, not required for v1)
- ❌ Third-party NuGet packages (MudBlazor, charting libraries, Newtonsoft.Json)

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** | < 1 second from `dotnet run` to fully rendered page |
| **Data parsing** | < 50ms to deserialize `data.json` (expected < 10KB file) |
| **SVG render** | Timeline SVG renders without visible jank or reflow |
| **Memory footprint** | < 50MB total application memory at runtime |

### Security

| Requirement | Implementation |
|-------------|---------------|
| **Authentication** | None — local-only tool, single user |
| **Data sensitivity** | `data.json` contains project status, not PII. No encryption required |
| **Network exposure** | Kestrel binds to `localhost` only. Not exposed to network |
| **HTTPS** | Optional. HTTP is acceptable for local use |

### Reliability

| Requirement | Target |
|-------------|--------|
| **Availability** | Runs when started; no uptime SLA (local utility) |
| **Error handling** | Malformed `data.json` shows a user-friendly error, not a stack trace |
| **Data integrity** | `data.json` is read-only at runtime; no mutation risk |

### Scalability

Not applicable. This is a single-user, single-machine, read-only utility. The dataset is < 100 items. No scaling considerations apply.

### Compatibility

| Dimension | Requirement |
|-----------|------------|
| **Browser** | Chrome (latest) at 1920×1080. Other browsers are best-effort |
| **OS** | Windows 10/11 (Segoe UI system font required) |
| **.NET Runtime** | .NET 8 SDK |
| **Screenshot Tools** | Windows Snipping Tool, Chrome DevTools full-page capture |

## Success Metrics

1. **Visual Fidelity:** The rendered dashboard is visually indistinguishable from `OriginalDesignConcept.html` when compared side-by-side at 1920×1080 (subjective review by project lead).
2. **Data-Driven Rendering:** Changing any value in `data.json` (title, milestones, work items, months) and refreshing the browser reflects the change accurately in the rendered dashboard.
3. **Screenshot Workflow Completion:** A project lead can go from editing `data.json` to having a 1920×1080 screenshot pasted into PowerPoint in under 2 minutes.
4. **Zero-Setup Run:** A developer can clone the repo, run `dotnet run`, and see the dashboard at `localhost:5000` with no additional configuration, packages, or environment setup.
5. **Total Implementation Time:** The complete dashboard is implemented and functional within 5–8 hours of developer effort.
6. **File Count:** The entire application consists of 8 or fewer source files (excluding `bin/`, `obj/`, and generated files).

## Constraints & Assumptions

### Technical Constraints

- **Framework:** Must use .NET 8 Blazor Server (organizational mandate for the C# ecosystem)
- **No External NuGet Packages:** All rendering is achieved with built-in .NET 8 libraries, raw HTML/CSS/SVG. No MudBlazor, no charting libraries, no Newtonsoft.Json
- **Fixed Viewport:** 1920×1080px, non-negotiable. This matches standard 1080p monitors and PowerPoint slide dimensions
- **System Font:** Segoe UI is assumed available (Windows-only usage). No web font loading
- **Local Execution Only:** The app runs on `localhost` via Kestrel. No cloud hosting, no public URL
- **Static SSR Preferred:** Use .NET 8 Static Server-Side Rendering to avoid unnecessary SignalR WebSocket overhead for read-only content

### Assumptions

- **Single User:** Only one person (the project lead) uses the dashboard at a time. No concurrency considerations
- **Manual Updates:** The project lead manually edits `data.json` in a text editor (e.g., VS Code). No programmatic data pipeline feeds into this file
- **Monthly Cadence:** The dashboard is updated monthly (or as needed) before executive reviews. Hot-reload is unnecessary
- **Windows Environment:** The primary user runs Windows 10/11 with .NET 8 SDK installed and uses Chrome as their browser
- **No Accessibility Requirements:** The dashboard's sole output is screenshots for PowerPoint. Screen readers, keyboard navigation, and WCAG compliance are not required
- **Design Reference is Authoritative:** The `OriginalDesignConcept.html` file and its rendered screenshot define the visual specification. Any ambiguity in this PMSpec is resolved by consulting the HTML source
- **Fictional Sample Data:** The initial `data.json` ships with fictional project data to demonstrate all features. The project lead replaces this with real data for their project
- **No Version Control for Data:** `data.json` may or may not be committed to source control at the project lead's discretion. If project names are sensitive, it should be `.gitignore`'d
- **Browser Screenshot Tooling:** The project lead is comfortable using Chrome DevTools or Windows Snipping Tool. No automated screenshot pipeline is needed

### Dependencies

| Dependency | Version | Purpose | Risk |
|-----------|---------|---------|------|
| .NET 8 SDK | 8.0+ | Build and run the Blazor Server app | Low — stable, LTS release |
| Segoe UI font | System | Primary display font | Low — pre-installed on all Windows machines |
| Chrome browser | Latest | Rendering and screenshot capture | Low — ubiquitous |
| `OriginalDesignConcept.html` | From repo | Visual design reference | Low — committed to repository |

### Timeline

| Phase | Description | Estimated Effort |
|-------|-------------|-----------------|
| Phase 1 | Skeleton + CSS port | 2–3 hours |
| Phase 2 | Data model + JSON binding | 1–2 hours |
| Phase 3 | SVG timeline rendering | 1–2 hours |
| Phase 4 | Polish + screenshot optimization + README | 1 hour |
| **Total** | **End-to-end implementation** | **5–8 hours** |