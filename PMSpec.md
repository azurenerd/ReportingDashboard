# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, timelines, and monthly execution status (Shipped, In Progress, Carryover, Blockers) in a clean, screenshot-ready format optimized for 1920×1080 resolution. The dashboard reads all data from a local `data.json` configuration file and renders a pixel-perfect view using Blazor Server (.NET 8), inline SVG, and CSS Grid—designed to be captured and pasted directly into PowerPoint decks for executive stakeholders.

## Business Goals

1. **Provide at-a-glance project visibility** — Deliver a single-page view that executives can consume in under 30 seconds to understand project health, shipped items, blockers, and upcoming milestones.
2. **Eliminate manual slide creation** — Replace hand-crafted PowerPoint status slides with a screenshot of a live, data-driven dashboard, reducing preparation time from hours to minutes.
3. **Standardize project reporting** — Establish a reusable template that any project team can adopt by editing a JSON file, ensuring consistent formatting across executive presentations.
4. **Enable rapid data updates** — Allow project managers to update status by editing a single `data.json` file and refreshing the browser, with zero deployment or build steps required.
5. **Keep operational overhead near zero** — No authentication, no database, no cloud infrastructure. A local `dotnet run` command produces the dashboard.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As an** executive viewer, **I want** to see the project title, organizational context, date, and a link to the backlog at the top of the dashboard, **so that** I immediately know which project and time period I am looking at.

**Visual Reference:** Header section (`.hdr`) in `OriginalDesignConcept.html`

- [ ] Page displays project title in bold 24px font at top-left
- [ ] Subtitle shows organization, workstream, and current month/year
- [ ] Backlog link is rendered as a clickable hyperlink in Microsoft blue (#0078D4)
- [ ] Legend appears at top-right showing icons for PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line)
- [ ] All header content is data-driven from `data.json`

### US-2: View Milestone Timeline

**As an** executive viewer, **I want** to see a horizontal timeline showing milestone tracks with dated markers, **so that** I can understand the project schedule and where we are relative to key dates.

**Visual Reference:** Timeline area (`.tl-area` and SVG) in `OriginalDesignConcept.html`

- [ ] Timeline renders as an inline SVG with viewBox `0 0 1560 185`
- [ ] Left sidebar (230px) lists milestone track labels (e.g., M1, M2, M3) with color-coded IDs and descriptions
- [ ] Each milestone track renders as a colored horizontal line spanning the full timeline width
- [ ] Month dividers appear as vertical lines with month labels (Jan–Jun or as defined in data)
- [ ] Checkpoint markers render as open circles on the track line
- [ ] PoC milestones render as gold (#F4B400) diamond shapes with drop shadow
- [ ] Production releases render as green (#34A853) diamond shapes with drop shadow
- [ ] A red dashed vertical line labeled "NOW" indicates the current date position
- [ ] Date labels appear above or below each marker
- [ ] X-positions of all markers are calculated proportionally from milestone dates relative to the timeline date range
- [ ] Timeline date range (`startDate`, `endDate`) is defined in `data.json`

### US-3: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a color-coded grid showing what was shipped, what is in progress, what carried over, and what is blocked—organized by month, **so that** I can assess execution health at a glance.

**Visual Reference:** Heatmap grid (`.hm-wrap`, `.hm-grid`) in `OriginalDesignConcept.html`

- [ ] Heatmap renders as a CSS Grid with columns: Status label (160px) + 4 time period columns (equal width)
- [ ] Header row displays month names; the current month column is highlighted with gold background (#FFF0D0) and amber text (#C07700)
- [ ] Four status rows are rendered: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each cell lists work items as bullet-pointed text entries with a colored dot prefix matching the row category
- [ ] Current-month cells use a slightly darker background shade than other months
- [ ] Empty future cells display a dash placeholder
- [ ] All items and month labels are data-driven from `data.json`
- [ ] Section title "Monthly Execution Heatmap" appears above the grid in uppercase, gray, with letter spacing

### US-4: Load Dashboard Data from JSON

**As a** project manager, **I want** the dashboard to read all display data from a `data.json` file in the `wwwroot` folder, **so that** I can update project status by editing a single file without touching code.

- [ ] Application reads `wwwroot/data.json` on page load
- [ ] JSON is deserialized into strongly-typed C# record models using `System.Text.Json`
- [ ] All rendered content (title, subtitle, milestones, heatmap items, time periods) comes from the JSON data
- [ ] Changing `data.json` and refreshing the browser reflects updated data
- [ ] JSON parsing uses case-insensitive property matching

### US-5: Handle Malformed or Missing Data Gracefully

**As a** project manager, **I want** the dashboard to display a clear error message if `data.json` is missing or malformed, **so that** I can diagnose and fix data issues without seeing a broken page.

- [ ] If `data.json` is missing, the page displays a centered error message: "Dashboard data not found. Please ensure data.json exists in the wwwroot folder."
- [ ] If JSON parsing fails, the page displays: "Error reading dashboard data: [error detail]"
- [ ] Error messages are styled consistently with the dashboard theme (Segoe UI, muted colors)
- [ ] The application does not crash or show a stack trace to the user

### US-6: Screenshot-Ready Rendering at 1920×1080

**As an** executive viewer, **I want** the dashboard to render cleanly at exactly 1920×1080 pixels with no scrolling, **so that** I can take a full-page screenshot and paste it directly into a PowerPoint slide.

**Visual Reference:** `body` style in `OriginalDesignConcept.html` — `width:1920px;height:1080px;overflow:hidden`

- [ ] Page body is fixed at 1920px wide and 1080px tall with `overflow: hidden`
- [ ] All content fits within the viewport without scrolling
- [ ] The heatmap section flexes to fill remaining vertical space after header and timeline
- [ ] No browser chrome, scrollbars, or interactive UI elements appear in the rendered output
- [ ] Font rendering uses Segoe UI as primary, Arial as fallback

### US-7: Run the Dashboard Locally

**As a** project manager, **I want** to start the dashboard with a single `dotnet run` command, **so that** I can view the dashboard without any infrastructure setup.

- [ ] `dotnet run` starts the application and serves the dashboard on `localhost`
- [ ] No external services, databases, or API keys are required
- [ ] Default port is configurable via `launchSettings.json`
- [ ] Console output confirms the URL where the dashboard is accessible

### US-8: Publish as Self-Contained Executable

**As a** project manager, **I want** to publish the dashboard as a single `.exe` file, **so that** I can share it with colleagues who don't have .NET installed.

- [ ] `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true` produces a single executable
- [ ] The executable runs on any Windows machine without .NET SDK or runtime installed
- [ ] The `data.json` file is accessible for editing alongside the published executable

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` — Engineers MUST consult this file and match its visual output exactly, with improvements only where explicitly noted.

### Overall Layout

- **Viewport:** Fixed 1920px × 1080px, `overflow: hidden`, white (#FFFFFF) background
- **Font Stack:** `'Segoe UI', Arial, sans-serif`, base text color #111
- **Layout Model:** Vertical flex column (`display: flex; flex-direction: column`) with three stacked sections:
  1. Header (fixed height, ~50px)
  2. Timeline area (fixed height, 196px)
  3. Heatmap area (flex: 1, fills remaining space)
- **Horizontal padding:** 44px on both sides for all sections

### Section 1: Header (`.hdr`)

- **Layout:** Flex row, `align-items: center`, `justify-content: space-between`
- **Left side:**
  - Title: `<h1>` at 24px, font-weight 700, color #111
  - Backlog link: inline `<a>` in Microsoft blue #0078D4, no underline
  - Subtitle: 12px, color #888, margin-top 2px
- **Right side (Legend):**
  - Four legend items in a flex row with 22px gap
  - PoC Milestone: 12×12px gold (#F4B400) square rotated 45° (diamond)
  - Production Release: 12×12px green (#34A853) square rotated 45° (diamond)
  - Checkpoint: 8×8px circle, background #999
  - Now indicator: 2×14px red (#EA4335) vertical bar
  - All labels: 12px font size
- **Bottom border:** 1px solid #E0E0E0

### Section 2: Timeline Area (`.tl-area`)

- **Container:** Flex row, height 196px, background #FAFAFA, bottom border 2px solid #E8E8E8, top padding 6px
- **Left sidebar (Milestone Legend):**
  - Width: 230px, flex-shrink 0
  - Vertical flex column with `justify-content: space-around`, padding 16px 12px 16px 0
  - Right border: 1px solid #E0E0E0
  - Each track label:
    - Track ID (e.g., "M1") in 12px, font-weight 600, track color (M1: #0078D4, M2: #00897B, M3: #546E7A)
    - Description on second line in font-weight 400, color #444
- **SVG Timeline (`.tl-svg-box`):**
  - Flex: 1, padding-left 12px, padding-top 6px
  - SVG dimensions: width 1560, height 185, `overflow: visible`
  - **Month grid lines:** Vertical lines at evenly spaced intervals, stroke #bbb, opacity 0.4, width 1
  - **Month labels:** 11px, font-weight 600, color #666, positioned 5px right of each grid line at y=14
  - **NOW line:** Vertical dashed line (stroke #EA4335, width 2, dasharray 5,3), label "NOW" in 10px bold red
  - **Track lines:** Horizontal lines spanning full width, stroke-width 3, colored per track
  - **Markers:**
    - *Checkpoint (circle):* White fill, colored stroke matching track, stroke-width 2.5, radius 5–7px
    - *Small checkpoint (dot):* Filled #999, radius 4px
    - *PoC diamond:* `<polygon>` with 4 points forming 22px diamond, fill #F4B400, drop shadow filter
    - *Production diamond:* Same shape, fill #34A853, drop shadow filter
  - **Date labels:** 10px, color #666, `text-anchor: middle`, positioned above or below marker
  - **Drop shadow filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Container:** Flex: 1 (fills remaining height), flex-direction column, padding 10px 44px
- **Title:** 14px, font-weight 700, color #888, uppercase, letter-spacing 0.5px, margin-bottom 8px
  - Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"
- **Grid (`.hm-grid`):**
  - CSS Grid: `grid-template-columns: 160px repeat(4, 1fr)`
  - `grid-template-rows: 36px repeat(4, 1fr)`
  - Border: 1px solid #E0E0E0
  - Flex: 1 (fills remaining vertical space)

#### Grid Header Row (36px height)

| Cell | Style |
|------|-------|
| Corner (`.hm-corner`) | Background #F5F5F5, 11px bold uppercase #999, border-right 1px #E0E0E0, border-bottom 2px #CCC |
| Month headers (`.hm-col-hdr`) | 16px bold, background #F5F5F5, centered, border-right 1px #E0E0E0, border-bottom 2px #CCC |
| Current month (`.apr-hdr`) | Background #FFF0D0, text color #C07700, append "◀ Now" label |

#### Grid Data Rows — Color Scheme

| Row | Header BG | Header Text | Cell BG | Current Month BG | Dot Color |
|-----|-----------|-------------|---------|-------------------|-----------|
| **Shipped** | #E8F5E9 | #1B7A28 | #F0FBF0 | #D8F2DA | #34A853 |
| **In Progress** | #E3F2FD | #1565C0 | #EEF4FE | #DAE8FB | #0078D4 |
| **Carryover** | #FFF8E1 | #B45309 | #FFFDE7 | #FFF0B0 | #F4B400 |
| **Blockers** | #FEF2F2 | #991B1B | #FFF5F5 | #FFE4E4 | #EA4335 |

#### Grid Data Cells

- Padding: 8px 12px
- Border-right: 1px solid #E0E0E0, border-bottom: 1px solid #E0E0E0
- **Work items (`.it`):** 12px, color #333, padding 2px 0 2px 12px, line-height 1.35
- **Item dot (`.it::before`):** 6×6px circle, positioned absolute left:0 top:7px, colored per row category
- **Row headers (`.hm-row-hdr`):** 11px bold uppercase, letter-spacing 0.7px, border-right 2px solid #CCC, includes emoji prefix (✅ Shipped, 🔵 In Progress, etc.)

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Dashboard Renders with Project Data**
User navigates to `localhost:{port}`. The application reads `data.json`, deserializes it, and renders the complete dashboard within the 1920×1080 viewport. The header displays the project title and current month. The timeline shows all milestone tracks with markers positioned by date. The heatmap grid shows all four status rows populated with items from the JSON data. The current month column is highlighted. The "NOW" line is positioned based on the current date.

**Scenario 2: User Views the Header and Identifies the Project**
User looks at the top of the page and sees the project name in bold, the organizational context and date below it, and a clickable backlog link. On the right, the legend explains the four marker types used in the timeline. This takes under 5 seconds to parse.

**Scenario 3: User Scans the Milestone Timeline for Schedule Status**
User looks at the timeline section. Horizontal colored lines represent milestone tracks. Diamond markers indicate PoC (gold) and Production (green) milestones with date labels. The red dashed "NOW" line shows the current date position. The user can immediately see which milestones are past, current, or future relative to today.

**Scenario 4: User Reads the Heatmap to Assess Execution Health**
User scans the heatmap grid from top to bottom. The "Shipped" row (green) shows completed work. "In Progress" (blue) shows active items. "Carryover" (amber) flags items that slipped from a prior month. "Blockers" (red) highlights impediments. The current month column is visually emphasized with a gold header and darker cell backgrounds.

**Scenario 5: User Identifies Blockers Requiring Attention**
User focuses on the red "Blockers" row. Each cell lists blocking items with red dot indicators. The current-month blockers cell uses a more saturated red background (#FFE4E4) drawing the eye. The user notes specific blockers to discuss in the executive review.

**Scenario 6: User Takes a Screenshot for PowerPoint**
User ensures the browser window is at 1920×1080 (using DevTools device toolbar or window sizing). The entire dashboard fits within the viewport with no scrolling. User presses Win+Shift+S, captures the full page, and pastes into a PowerPoint slide. The screenshot is crisp with readable 12px text and clear color differentiation.

**Scenario 7: Project Manager Updates Status Data**
Project manager opens `wwwroot/data.json` in a text editor. They add a new item to the "Shipped" section for the current month, move a completed "In Progress" item to "Shipped," and add a newly discovered blocker. They save the file, refresh the browser, and the dashboard reflects all changes immediately.

**Scenario 8: Dashboard Loads with Missing data.json**
User starts the application but `data.json` does not exist in `wwwroot/`. Instead of a blank or broken page, a centered error message appears: "Dashboard data not found. Please ensure data.json exists in the wwwroot folder." The message uses the dashboard's Segoe UI font and a muted color scheme.

**Scenario 9: Dashboard Loads with Malformed JSON**
User has a syntax error in `data.json` (e.g., trailing comma). The page displays: "Error reading dashboard data: [specific parse error]" with enough detail to locate and fix the issue. No unhandled exception page is shown.

**Scenario 10: Empty Heatmap Cells for Future Months**
The data for May and June contains no items. Those cells render with a dash "—" placeholder in muted gray (#AAA), maintaining grid structure and visual consistency. No blank cells or layout collapse occurs.

**Scenario 11: NOW Line Auto-Positions Based on Current Date**
The dashboard calculates the current date and positions the red dashed "NOW" line proportionally within the timeline's date range. If today is April 11 and the timeline spans Jan 1–Jun 30, the line appears at approximately 55% of the SVG width. The position updates each time the page is loaded.

## Scope

### In Scope

- Single-page Blazor Server application rendering a project reporting dashboard
- Header section with project title, subtitle, backlog link, and legend
- SVG-based horizontal milestone timeline with multiple tracks, date markers, and "NOW" indicator
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) across four time period columns
- Data loading from a local `data.json` file with strongly-typed C# record models
- Sample `data.json` with fictional project data demonstrating all features
- Fixed 1920×1080 pixel-perfect layout optimized for screenshots
- Error handling for missing or malformed JSON data
- `dotnet publish` support for self-contained single-file executable
- README documenting the `data.json` schema and usage instructions

### Out of Scope

- **Authentication and authorization** — No login, no user roles, no access control
- **Database or API backend** — All data comes from a flat JSON file
- **Real-time data feeds** — No live connections to ADO, Jira, or other project tools
- **Multi-user editing** — No concurrent access handling or locking
- **Dark mode** — Deferred; single light theme matching Microsoft executive presentation style
- **Print/PDF export** — Browser screenshot is the primary capture method
- **Responsive/mobile layout** — Fixed 1920×1080 only; not designed for smaller screens
- **Interactive features** — No hover tooltips, click handlers, drill-downs, or animations
- **Historical data comparison** — Use Git versioning of `data.json` for history
- **Multi-project switching** — One dashboard instance per project; no routing between projects
- **Automated screenshot generation** — Playwright integration deferred to a future phase
- **Data editing UI** — No built-in form for editing `data.json`; hand-edit the file
- **HTTPS/TLS** — Localhost HTTP is sufficient for this local-only tool
- **Charting libraries** — No Radzen, MudBlazor, ApexCharts, or D3; raw SVG and CSS only

## Non-Functional Requirements

### Performance

- **Page load time:** Dashboard must render fully within 2 seconds of browser navigation on localhost
- **Data file size:** Support `data.json` files up to 500 KB without degradation
- **Memory footprint:** Application should consume less than 100 MB RAM during operation

### Rendering Fidelity

- **Target browser:** Microsoft Edge or Google Chrome (latest stable version)
- **Resolution:** Pixel-perfect rendering at 1920×1080; all text must be crisp and legible at 12px
- **Font rendering:** Segoe UI must be used; fallback to Arial on non-Windows systems
- **SVG consistency:** Drop shadows and text alignment must match across Edge and Chrome

### Reliability

- **Startup:** Application must start successfully with `dotnet run` within 5 seconds
- **Error resilience:** Malformed JSON must not crash the application; graceful error display required
- **No external dependencies:** Dashboard must function with no internet connection

### Maintainability

- **Code simplicity:** No unnecessary abstraction layers (no repository pattern, no mediator, no interfaces for single implementations)
- **Single component:** Dashboard logic should reside in a single `.razor` file with `@code` block
- **CSS isolation:** Styles scoped via Blazor CSS isolation (`Dashboard.razor.css`)

### Security

- **Attack surface:** Minimal; no authentication, no user input processing, no API endpoints
- **Input validation:** JSON deserialization only; no user-supplied input from the browser
- **Network exposure:** Localhost binding by default; no external network access required

## Success Metrics

1. **Visual Fidelity:** A screenshot of the rendered dashboard at 1920×1080 is visually indistinguishable from the `OriginalDesignConcept.html` reference when opened side-by-side, with improvements in polish and data-driven rendering.
2. **Data-Driven Rendering:** Changing any value in `data.json` (title, milestones, heatmap items) and refreshing the browser reflects the change accurately within 2 seconds.
3. **Zero-Config Startup:** A new user can clone the repository, run `dotnet run`, and see the dashboard in their browser with no additional setup steps.
4. **Screenshot Quality:** Text at 12px font size is legible when the screenshot is pasted into a standard 16:9 PowerPoint slide (1920×1080 or scaled equivalent).
5. **Self-Contained Distribution:** `dotnet publish` produces a single executable that runs on a clean Windows machine without .NET installed, and serves the dashboard with the bundled `data.json`.
6. **Error Handling:** Missing or malformed `data.json` produces a user-friendly error message, not a crash or stack trace.
7. **Development Velocity:** The fictional sample `data.json` demonstrates all dashboard features (3+ milestone tracks, 4 months, 5+ items per status row) and serves as documentation for the data schema.

## Constraints & Assumptions

### Technical Constraints

- **Runtime:** .NET 8 SDK (LTS) is required for development; self-contained publish removes this requirement for end users
- **Framework:** Blazor Server — chosen to align with the existing AgentSquad repository's .NET ecosystem
- **No JavaScript:** The dashboard must be implementable with zero JS interop; Blazor markup, SVG, and CSS only
- **No NuGet packages for UI:** No charting or component libraries; all visualization is hand-built with SVG and CSS Grid
- **Windows-first:** Segoe UI font dependency assumes Windows; macOS/Linux users may see Arial fallback
- **Fixed viewport:** 1920×1080 is a hard constraint driven by the PowerPoint screenshot workflow

### Timeline Assumptions

- **Phase 1 (MVP):** 1–2 days — Scaffold project, data model, sample JSON, dashboard component with header/timeline/heatmap, CSS porting
- **Phase 2 (Polish):** 1 day — Auto-calculated NOW position, data validation, publish profile, README documentation
- **Phase 3 (Optional):** As needed — Playwright screenshots, multi-project configs, edit form, dark mode

### Dependency Assumptions

- The `OriginalDesignConcept.html` file is the canonical visual reference and will not change during implementation
- The `data.json` schema is stable; any additions are backward-compatible (new fields are optional with defaults)
- The dashboard is used by a single user at a time on localhost; no concurrent access scenarios
- Executives consume the dashboard as static screenshots, not as a live interactive tool
- Project teams are comfortable editing JSON files by hand for data updates

### Data Assumptions

- Each dashboard instance tracks one project
- The heatmap displays exactly four time periods (months) and four status rows
- Milestone tracks are limited to a reasonable number (3–5) to fit within 185px SVG height
- The timeline date range is explicitly defined in `data.json` (not auto-calculated) for viewport control
- Work items in heatmap cells are short text strings (under 60 characters) to avoid cell overflow