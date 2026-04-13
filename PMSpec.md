# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones on a timeline and displays a color-coded heatmap of work item statuses (Shipped, In Progress, Carryover, Blockers) by month. The dashboard reads all data from a local `data.json` configuration file, runs locally via Blazor Server (.NET 8), and is purpose-built to produce polished 1920×1080 screenshots for executive PowerPoint decks—requiring no authentication, no cloud infrastructure, and no enterprise security.

## Business Goals

1. **Provide executive-ready project visibility** — Deliver a single-page view that communicates project health, milestone progress, and monthly execution status at a glance.
2. **Eliminate manual slide creation** — Enable the project lead to screenshot the dashboard directly into PowerPoint decks, replacing hours of manual slide formatting each reporting cycle.
3. **Standardize status reporting** — Establish a reusable, data-driven template that can be repopulated for any project by editing a single JSON file.
4. **Minimize operational overhead** — Require zero cloud resources, zero authentication, and zero ongoing maintenance; the dashboard runs locally and costs $0.
5. **Accelerate reporting cadence** — Reduce time-to-report from hours to minutes by changing `data.json` and refreshing the browser.

## User Stories & Acceptance Criteria

### US-1: View Project Header

**As an** executive viewer, **I want** to see the project title, organizational context, date, and a link to the ADO backlog at the top of the page, **so that** I immediately understand which project and time period I'm looking at.

**Visual Reference:** Header section (`.hdr`) of `OriginalDesignConcept.html`

- [ ] Page displays project title in 24px bold font (e.g., "Privacy Automation Release Roadmap")
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, styled in `#0078D4`
- [ ] Subtitle displays organization, workstream, and current month in 12px gray text (`#888`)
- [ ] All text values are driven by `data.json` (`project.title`, `project.subtitle`, `project.backlogUrl`)
- [ ] Header is separated from the timeline by a 1px `#E0E0E0` bottom border

### US-2: View Milestone Legend

**As an** executive viewer, **I want** to see a legend explaining the milestone symbols, **so that** I can interpret the timeline without asking for clarification.

**Visual Reference:** Legend area (top-right of `.hdr`) of `OriginalDesignConcept.html`

- [ ] Legend displays four items in a horizontal row: PoC Milestone (gold diamond `#F4B400`), Production Release (green diamond `#34A853`), Checkpoint (gray circle `#999`), Now marker (red vertical bar `#EA4335`)
- [ ] Each legend item shows the symbol followed by a 12px label
- [ ] Legend items are spaced with a 22px gap
- [ ] Legend is right-aligned within the header bar

### US-3: View Milestone Timeline

**As an** executive viewer, **I want** to see a horizontal timeline showing major milestones across multiple workstreams, **so that** I can understand the project schedule and current position at a glance.

**Visual Reference:** Timeline area (`.tl-area`) of `OriginalDesignConcept.html`

- [ ] Timeline area has a light gray background (`#FAFAFA`) and is exactly 196px tall
- [ ] Left sidebar (230px wide) lists each timeline track with its ID (e.g., "M1"), label, and description, color-coded per track
- [ ] Main SVG area renders horizontal track lines spanning the full timeline date range
- [ ] Month gridlines appear as vertical lines with month labels (Jan–Jun) at the top
- [ ] Checkpoints render as open circles with colored stroke on the track line
- [ ] PoC milestones render as gold diamond polygons (`#F4B400`) with drop shadow
- [ ] Production releases render as green diamond polygons (`#34A853`) with drop shadow
- [ ] A dashed red "NOW" line (`#EA4335`, `stroke-dasharray: 5,3`) marks the current date position with a "NOW" label
- [ ] Each milestone has a date label (e.g., "Mar 26 PoC") positioned above or below the track line
- [ ] Milestone X-positions are calculated proportionally from `data.json` date values, not hardcoded
- [ ] All tracks, milestones, and dates are driven by `data.json` (`timeline.tracks`, `timeline.startDate`, `timeline.endDate`)

### US-4: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked for each month, **so that** I can assess execution velocity and identify risks.

**Visual Reference:** Heatmap section (`.hm-wrap`, `.hm-grid`) of `OriginalDesignConcept.html`

- [ ] Section title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in 14px uppercase gray text
- [ ] Grid uses CSS Grid with columns: 160px (row header) + N equal-width month columns
- [ ] Header row (36px) shows month names; the current/highlight month has a gold background (`#FFF0D0`) with amber text (`#C07700`)
- [ ] Four data rows correspond to the four status categories, each color-coded:
  - **Shipped** — row header: green text `#1B7A28` on `#E8F5E9`; cells: `#F0FBF0`; highlight month: `#D8F2DA`; bullet: `#34A853`
  - **In Progress** — row header: blue text `#1565C0` on `#E3F2FD`; cells: `#EEF4FE`; highlight month: `#DAE8FB`; bullet: `#0078D4`
  - **Carryover** — row header: amber text `#B45309` on `#FFF8E1`; cells: `#FFFDE7`; highlight month: `#FFF0B0`; bullet: `#F4B400`
  - **Blockers** — row header: red text `#991B1B` on `#FEF2F2`; cells: `#FFF5F5`; highlight month: `#FFE4E4`; bullet: `#EA4335`
- [ ] Each cell lists work items as bulleted lines (6px colored circle + 12px text)
- [ ] Empty cells for future months display a gray dash "—"
- [ ] The highlight month column is determined by `data.json` (`heatmap.highlightMonth`), not by system date
- [ ] All category names, items, and months are driven by `data.json` (`heatmap.categories`, `heatmap.months`)

### US-5: Configure Dashboard via JSON

**As a** project lead, **I want** to update a single `data.json` file to change all dashboard content, **so that** I can repurpose the dashboard for any project without editing code.

- [ ] A `data.json` file in `wwwroot/` contains all dashboard data: project info, timeline tracks, milestones, heatmap categories and items
- [ ] Changing `data.json` and refreshing the browser updates the dashboard immediately
- [ ] Invalid or missing JSON fields degrade gracefully (empty sections, not crashes)
- [ ] The JSON schema supports 1–6 timeline tracks, 1–12 months, and 0–N items per heatmap cell
- [ ] A sample `data.json` with fictional project data is included out of the box

### US-6: Capture Screenshot for PowerPoint

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, **so that** I can take a full-page screenshot that fits perfectly into a 16:9 PowerPoint slide.

- [ ] The page renders at a fixed 1920×1080 viewport with `overflow: hidden`
- [ ] No scrollbars appear at 1920×1080 resolution
- [ ] All content fits within the viewport without clipping
- [ ] The page renders correctly in Chrome and Edge (Chromium-based browsers)
- [ ] Screenshot via browser DevTools ("Capture full size screenshot") produces a clean image

## Visual Design Specification

**Design Reference File:** `OriginalDesignConcept.html` (located in the ReportingDashboard repository)

**Design Screenshot:** See `OriginalDesignConcept.png` rendered at 1920×1080.

Engineers MUST consult both the HTML source and the screenshot. The implementation must be pixel-accurate to this design.

### Overall Page Layout

- **Viewport:** Fixed 1920×1080px, no scrolling, white background (`#FFFFFF`)
- **Font Family:** `'Segoe UI', Arial, sans-serif`
- **Base Text Color:** `#111`
- **Link Color:** `#0078D4`, no underline
- **Layout:** Vertical flex column (`display: flex; flex-direction: column`) with three stacked sections:
  1. Header (auto height, ~50px)
  2. Timeline (fixed 196px height)
  3. Heatmap (flex: 1, fills remaining space)
- **Horizontal Padding:** 44px left/right on all sections

### Section 1: Header (`.hdr`)

- **Layout:** Flexbox row, `align-items: center`, `justify-content: space-between`
- **Padding:** `12px 44px 10px`
- **Bottom Border:** 1px solid `#E0E0E0`
- **Left side:**
  - Title: `<h1>`, 24px, font-weight 700
  - Inline link: `<a>` in `#0078D4`
  - Subtitle: 12px, color `#888`, margin-top 2px
- **Right side (Legend):**
  - Horizontal row with 22px gap
  - Four legend items, each with a symbol + 12px label:
    - PoC Milestone: 12×12px gold (`#F4B400`) square rotated 45° (diamond)
    - Production Release: 12×12px green (`#34A853`) square rotated 45° (diamond)
    - Checkpoint: 8×8px gray (`#999`) circle
    - Now marker: 2×14px red (`#EA4335`) vertical bar

### Section 2: Timeline (`.tl-area`)

- **Layout:** Flexbox row, `align-items: stretch`
- **Height:** Fixed 196px
- **Background:** `#FAFAFA`
- **Bottom Border:** 2px solid `#E8E8E8`

**Left Sidebar (Track Labels):**
- Width: 230px, flex-shrink: 0
- Vertical flex column, `justify-content: space-around`
- Right border: 1px solid `#E0E0E0`
- Each track label: 12px font-weight 600, track color for ID, `#444` for description
- Track colors from design: M1 = `#0078D4`, M2 = `#00897B`, M3 = `#546E7A`

**Right Area (SVG Timeline):**
- Flex: 1, padding-left 12px, padding-top 6px
- Contains a single `<svg>` element, width fills container, height 185px
- **Month Gridlines:** Vertical lines at proportional positions, stroke `#bbb` opacity 0.4
- **Month Labels:** 11px, font-weight 600, fill `#666`, positioned at top of each gridline
- **Track Lines:** Horizontal lines spanning full width, stroke = track color, stroke-width 3
- **Track vertical spacing:** ~56px between tracks (y=42, y=98, y=154 in the design)
- **Milestone Markers:**
  - Checkpoint: `<circle>` r=5–7, white fill, colored stroke, stroke-width 2.5
  - PoC: `<polygon>` diamond (11px radius), fill `#F4B400`, with drop shadow filter
  - Production: `<polygon>` diamond (11px radius), fill `#34A853`, with drop shadow filter
  - Small checkpoint dots: `<circle>` r=4, fill `#999`
- **NOW Line:** `<line>` full height, stroke `#EA4335`, stroke-width 2, `stroke-dasharray="5,3"`; label "NOW" in 10px bold red
- **Drop Shadow Filter:** `<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>`
- **Date Labels:** 10px, fill `#666`, `text-anchor: middle`, positioned above or below the milestone marker

### Section 3: Heatmap (`.hm-wrap`)

- **Layout:** Flex column, flex: 1, min-height: 0
- **Padding:** `10px 44px 10px`

**Title Row:**
- 14px, font-weight 700, color `#888`, letter-spacing 0.5px, uppercase
- Margin-bottom 8px

**Grid (`.hm-grid`):**
- CSS Grid: `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months
- `grid-template-rows: 36px repeat(4, 1fr)`
- Border: 1px solid `#E0E0E0`
- Flex: 1 (fills remaining vertical space)

**Corner Cell (`.hm-corner`):**
- Background `#F5F5F5`, 11px bold uppercase, color `#999`
- Right border: 1px solid `#E0E0E0`, bottom border: 2px solid `#CCC`
- Text: "STATUS"

**Column Headers (`.hm-col-hdr`):**
- 16px bold, centered, background `#F5F5F5`
- Borders: right 1px `#E0E0E0`, bottom 2px `#CCC`
- **Highlight month** (`.apr-hdr`): background `#FFF0D0`, color `#C07700`

**Row Headers (`.hm-row-hdr`):**
- 11px bold uppercase, letter-spacing 0.7px, padding 0 12px
- Right border: 2px solid `#CCC`, bottom border: 1px solid `#E0E0E0`
- Per-category styling:
  - Shipped: color `#1B7A28`, bg `#E8F5E9`, emoji ✅
  - In Progress: color `#1565C0`, bg `#E3F2FD`, emoji 🔵
  - Carryover: color `#B45309`, bg `#FFF8E1`, emoji 🟡
  - Blockers: color `#991B1B`, bg `#FEF2F2`, emoji 🔴

**Data Cells (`.hm-cell`):**
- Padding: `8px 12px`, borders: right 1px `#E0E0E0`, bottom 1px `#E0E0E0`
- Per-category background colors (normal / highlight):
  - Shipped: `#F0FBF0` / `#D8F2DA`
  - In Progress: `#EEF4FE` / `#DAE8FB`
  - Carryover: `#FFFDE7` / `#FFF0B0`
  - Blockers: `#FFF5F5` / `#FFE4E4`
- **Item bullets (`.it`):** 12px, color `#333`, padding-left 12px, line-height 1.35
- **Bullet dot:** CSS `::before` pseudo-element, 6×6px circle, positioned absolutely left:0 top:7px, color matches category

### Color Token Summary

| Token | Hex | Usage |
|-------|-----|-------|
| Shipped green | `#34A853` | Bullet, production diamond |
| Shipped bg | `#F0FBF0` | Cell background |
| Shipped highlight | `#D8F2DA` | Current month cell |
| Shipped header bg | `#E8F5E9` | Row header |
| Shipped header text | `#1B7A28` | Row header text |
| Progress blue | `#0078D4` | Bullet, links, M1 track |
| Progress bg | `#EEF4FE` | Cell background |
| Progress highlight | `#DAE8FB` | Current month cell |
| Progress header bg | `#E3F2FD` | Row header |
| Progress header text | `#1565C0` | Row header text |
| Carryover amber | `#F4B400` | Bullet, PoC diamond |
| Carryover bg | `#FFFDE7` | Cell background |
| Carryover highlight | `#FFF0B0` | Current month cell |
| Carryover header bg | `#FFF8E1` | Row header |
| Carryover header text | `#B45309` | Row header text |
| Blockers red | `#EA4335` | Bullet, NOW line |
| Blockers bg | `#FFF5F5` | Cell background |
| Blockers highlight | `#FFE4E4` | Current month cell |
| Blockers header bg | `#FEF2F2` | Row header |
| Blockers header text | `#991B1B` | Row header text |
| Highlight month header bg | `#FFF0D0` | Column header |
| Highlight month header text | `#C07700` | Column header text |
| M2 track color | `#00897B` | Timeline track |
| M3 track color | `#546E7A` | Timeline track |
| Grid border | `#E0E0E0` | Cell borders |
| Header separator | `#CCC` | Thick borders |

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The dashboard loads and renders the complete page—header, timeline, and heatmap—in a single viewport at 1920×1080. No loading spinner is needed; the page renders server-side in a single HTTP response. All data is populated from `data.json`.

**Scenario 2: User Views the Header**
User sees the project title ("Privacy Automation Release Roadmap") with a clickable ADO backlog link in blue. The subtitle shows the organization, workstream, and reporting month. The legend on the right explains the four symbol types used in the timeline.

**Scenario 3: User Reads the Timeline**
User scans the timeline from left to right. Three horizontal tracks (M1, M2, M3) show colored lines with milestones plotted at their date positions. The dashed red "NOW" line indicates the current date. The user can visually compare which milestones are past vs. upcoming.

**Scenario 4: User Hovers Over a Milestone Diamond**
The milestone diamond and its date label are visible at default state. No hover tooltip is implemented in v1 (the dashboard is designed for screenshots, not interactive use). The date label adjacent to the diamond provides all context needed.

**Scenario 5: User Scans the Heatmap**
User looks at the heatmap grid. The current month column is visually highlighted with a warm gold header and slightly darker cell backgrounds. The user reads down each column to see what was shipped, what's in progress, what carried over, and what's blocked. Colored bullet dots reinforce the category through visual pattern.

**Scenario 6: User Identifies Blockers**
User looks at the bottom row (Blockers, red-themed). Red bullet dots next to item names draw immediate attention. If no blockers exist for a month, the cell shows a gray dash "—".

**Scenario 7: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser navigates to the Azure DevOps backlog URL specified in `data.json`. The link opens in the same tab (matching the original design's `<a>` behavior).

**Scenario 8: User Takes a Screenshot**
User opens Chrome DevTools (Ctrl+Shift+I), runs "Capture full size screenshot" from the command palette. The resulting PNG is exactly 1920×1080 and contains the full dashboard with no scrollbars, clipping, or artifacts. User pastes the image into a PowerPoint slide.

**Scenario 9: User Updates Dashboard Data**
User opens `wwwroot/data.json` in a text editor, changes item names, adds a new milestone, or updates the highlight month. User saves the file and refreshes the browser. The dashboard reflects all changes immediately.

**Scenario 10: Empty Data State**
User provides a `data.json` with an empty items array for a heatmap category in a given month. The cell renders a gray dash "—" instead of bullet items. The page does not crash or show errors.

**Scenario 11: Error State — Missing data.json**
If `data.json` is missing or contains invalid JSON, the page displays a simple centered error message: "Unable to load dashboard data. Please check data.json." No stack traces or technical details are shown.

**Scenario 12: Responsive Behavior**
The page is NOT responsive. At viewports smaller than 1920×1080, content is clipped (`overflow: hidden`). This is by design—the dashboard targets a fixed screenshot resolution only.

## Scope

### In Scope

- Single-page Blazor Server dashboard matching `OriginalDesignConcept.html` visual design
- Header with project title, subtitle, ADO backlog link, and milestone legend
- SVG-based milestone timeline with multiple tracks, date-positioned milestones, and "NOW" marker
- CSS Grid heatmap with four status categories (Shipped, In Progress, Carryover, Blockers) × N months
- Highlight styling for the current reporting month
- Data-driven rendering from a single `data.json` configuration file
- Sample `data.json` with fictional project data included out of the box
- Fixed 1920×1080 viewport optimized for screenshot capture
- Local-only execution via `dotnet run` (no deployment)
- Graceful handling of empty cells and missing optional data

### Out of Scope

- **Authentication & authorization** — No login, no roles, no tokens
- **Database** — No SQL, no Entity Framework, no data persistence beyond the JSON file
- **Cloud deployment** — No Azure, no Docker, no CI/CD pipeline
- **Responsive design** — No mobile, tablet, or variable-width support
- **Interactive features** — No tooltips, hover effects, drag-and-drop, or filtering
- **Real-time updates** — No WebSocket push, no SignalR hub, no auto-refresh
- **Multi-project support** — One dashboard per `data.json`; no project selector
- **Data entry UI** — Users edit `data.json` directly in a text editor
- **Export functionality** — No PDF export, no image export; users screenshot manually
- **Print stylesheet** — Out of scope for v1
- **Accessibility (WCAG compliance)** — Not required for an internal screenshot tool
- **Internationalization** — English only
- **Playwright screenshot automation** — Nice-to-have for future; not required for v1
- **Unit or integration tests** — Not required for a <500-line app with no business logic

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Page load time (initial) | < 1 second on localhost |
| Time to full render | < 500ms (server-side rendered, no client JS) |
| JSON deserialization | < 50ms for a data.json under 50KB |
| Memory footprint | < 100MB (Blazor Server baseline) |

### Compatibility

| Requirement | Target |
|-------------|--------|
| Browser | Chrome 120+ or Edge 120+ (Chromium-based) |
| Operating System | Windows 10/11 (Segoe UI font dependency) |
| .NET SDK | .NET 8.0 LTS |
| Resolution | 1920×1080 fixed viewport |

### Reliability

- The application must not crash on malformed `data.json`; display an error message instead
- The application must render correctly with 0–6 timeline tracks and 0–12 heatmap months
- No JavaScript errors in the browser console during normal operation

### Security

- No security requirements. The application runs locally with no network exposure beyond localhost.
- `data.json` contains non-sensitive project status data. No encryption needed.

### Maintainability

- CSS ported from `OriginalDesignConcept.html` must use CSS custom properties for color tokens
- Data model classes must use C# records with XML doc comments
- Solution must build with zero warnings on `dotnet build`

## Success Metrics

| # | Metric | Target | How to Verify |
|---|--------|--------|---------------|
| 1 | **Visual fidelity** | Dashboard screenshot is visually indistinguishable from `OriginalDesignConcept.png` at 1920×1080 | Side-by-side comparison of screenshot vs. design reference |
| 2 | **Data-driven rendering** | Changing any value in `data.json` and refreshing the browser updates the dashboard correctly | Modify 5+ fields in data.json, verify each renders |
| 3 | **Time to update** | Project lead can update dashboard content in < 5 minutes | Time a data.json edit + browser refresh cycle |
| 4 | **Zero-config setup** | New user can run the dashboard with `dotnet run` and no additional setup | Fresh clone → `dotnet run` → browser opens → dashboard renders |
| 5 | **Screenshot quality** | Full-page screenshot fits a 16:9 PowerPoint slide with no cropping needed | Insert screenshot into PowerPoint, verify no blank space or overflow |
| 6 | **Build cleanliness** | Solution builds with zero errors and zero warnings | `dotnet build` output |

## Constraints & Assumptions

### Technical Constraints

- **Must use Blazor Server (.NET 8)** — The parent repository (`AgentSquad`) is a .NET ecosystem; consistency is required
- **Must match `OriginalDesignConcept.html` design** — This is the approved executive visual; deviations require explicit approval
- **Fixed 1920×1080 resolution** — The dashboard is a screenshot target, not a responsive web application
- **No external JavaScript dependencies** — All rendering must be achievable with Blazor's native HTML/CSS/SVG support
- **No NuGet packages beyond the default Blazor template** — `System.Text.Json` (built-in) is the only serialization dependency
- **Windows-only runtime** — Segoe UI font is required and only available on Windows

### Assumptions

- The project lead has .NET 8 SDK installed on their development machine
- The project lead is comfortable editing JSON files in a text editor
- Chrome or Edge is available for viewing and screenshotting the dashboard
- `data.json` will be manually maintained (no automated data pipeline feeds into it)
- The dashboard will be used by 1 person at a time on localhost; no concurrent access concerns
- The number of heatmap items per cell will not exceed ~8 (to fit within the cell at 12px font size without overflow)
- Timeline tracks are limited to 3–5 to maintain visual clarity in the 196px-tall timeline area
- The heatmap displays 4–6 months to fit within the grid without horizontal compression
- The "NOW" marker date is auto-calculated from `DateTime.Now` at render time (not from data.json)
- Color scheme is hardcoded in CSS; data.json controls content only, not visual styling