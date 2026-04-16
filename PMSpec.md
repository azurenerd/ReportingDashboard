# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard as a Blazor Server (.NET 8) web application that visualizes project milestones on an SVG timeline and displays work item status in a color-coded heatmap grid. The dashboard reads all data from a local `data.json` configuration file, requires no authentication or cloud infrastructure, and is optimized for pixel-perfect 1920×1080 screenshots to be embedded in PowerPoint decks for executive audiences. The design is based on the `OriginalDesignConcept.html` template from the ReportingDashboard repository and will be improved for maximum executive readability.

## Business Goals

1. **Provide at-a-glance project visibility for executives** — Deliver a single-page view that communicates project health, shipped items, in-progress work, carryover, and blockers without requiring navigation or interaction.
2. **Eliminate manual slide creation overhead** — Enable the project lead to take a browser screenshot of the dashboard and paste it directly into a PowerPoint deck, replacing hours of manual chart and table creation each reporting cycle.
3. **Enable self-service data updates** — Allow any team member to update project status by editing a simple `data.json` text file, with no database, admin UI, or developer intervention required.
4. **Deliver a reusable reporting template** — Create a dashboard that can be repurposed for any project by swapping out the `data.json` file, making this a lightweight tool applicable across teams and workstreams.
5. **Maintain zero operational cost** — The application runs entirely on a developer's local workstation with no cloud dependencies, hosting fees, or license requirements.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** to see the project title, organizational context, current reporting period, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know what project they are looking at and can drill into the backlog if needed.

**Visual Reference:** Header section (`.hdr`) of `OriginalDesignConcept.html`

- [ ] The page displays the project title in bold 24px font at the top-left
- [ ] A subtitle line shows the organization, workstream, and current month (e.g., "Trusted Platform · Privacy Automation Workstream · April 2026")
- [ ] An ADO Backlog hyperlink is rendered next to the title in the brand blue color (`#0078D4`)
- [ ] A legend appears at the top-right showing icons for: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line)
- [ ] All header content is driven from `data.json` metadata fields

### US-2: View Milestone Timeline

**As an** executive reviewer, **I want** to see a horizontal timeline showing major milestones across multiple project tracks, **so that** I can understand the project's key dates and progress at a glance.

**Visual Reference:** Timeline area (`.tl-area` and inline SVG) of `OriginalDesignConcept.html`

- [ ] The timeline displays horizontal track lines, one per project track (e.g., M1, M2, M3), each in its designated track color
- [ ] Month columns are evenly spaced with vertical gridlines and month labels (e.g., Jan, Feb, Mar, Apr, May, Jun)
- [ ] PoC milestones render as gold diamond shapes (`#F4B400`) with drop shadows
- [ ] Production release milestones render as green diamond shapes (`#34A853`) with drop shadows
- [ ] Checkpoints render as small gray circles (`#999`)
- [ ] Start events render as open circles with the track's color stroke
- [ ] Each milestone has a date label positioned above or below the marker
- [ ] A red dashed vertical "NOW" line (`#EA4335`) indicates the current date position
- [ ] Track names are listed in a left sidebar panel (230px wide) with track IDs and descriptions
- [ ] Milestone positions are calculated dynamically from dates in `data.json`, not hardcoded
- [ ] Adding or removing milestones in `data.json` correctly updates the timeline without code changes

### US-3: View Monthly Execution Heatmap

**As an** executive reviewer, **I want** to see a grid showing what was shipped, what is in progress, what carried over, and what is blocked — organized by month, **so that** I can assess execution velocity and identify risks.

**Visual Reference:** Heatmap grid (`.hm-wrap`, `.hm-grid`) of `OriginalDesignConcept.html`

- [ ] The heatmap has a section title "Monthly Execution Heatmap" in uppercase gray text
- [ ] Column headers display month names; the current month column is highlighted with a gold background (`#FFF0D0`) and gold text (`#C07700`) with a "◀ Now" indicator
- [ ] Four status rows are displayed: ✅ Shipped (green), 🔵 In Progress (blue), 🟡 Carryover (amber), 🔴 Blockers (red)
- [ ] Each cell contains a list of work item names, each prefixed by a colored dot matching the row's status color
- [ ] Row headers use uppercase bold text with semantic background tints matching the status color
- [ ] Cells for the current month have a deeper background tint than other months
- [ ] Empty cells (future months with no data) display a muted dash character
- [ ] The number of month columns is dynamic, driven by the months array in `data.json`
- [ ] Work items are rendered by filtering `data.json` items by status and month

### US-4: Load Dashboard Data from JSON

**As a** project lead, **I want** the dashboard to read all display data from a single `data.json` file, **so that** I can update project status without modifying code or recompiling the application.

- [ ] A `data.json` file exists in the project's `wwwroot/` directory
- [ ] The file contains sections for: metadata (title, subtitle, months, backlog URL), tracks, milestones, and work items
- [ ] The application deserializes `data.json` on startup using `System.Text.Json`
- [ ] Changing values in `data.json` and restarting the app reflects the updated data on the dashboard
- [ ] If `data.json` is missing or malformed, the application displays a clear error message instead of crashing silently
- [ ] A documented sample `data.json` with fictional project data is included in the repository

### US-5: Capture Screenshot for PowerPoint

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with clean typography and no scrollbars, **so that** I can take a full-page screenshot and paste it directly into a PowerPoint slide with no cropping or resizing.

- [ ] The dashboard content fits within a 1920×1080 viewport without scrollbars
- [ ] The `Segoe UI` font is used throughout (available on all Windows machines)
- [ ] All text is crisp and legible at 100% zoom in Microsoft Edge
- [ ] The page has a white background (`#FFFFFF`) with no visual artifacts at the edges
- [ ] Colors are vibrant enough to remain distinguishable when projected or printed in a slide deck

### US-6: Run the Dashboard Locally

**As a** developer, **I want** to start the dashboard with a single `dotnet run` command, **so that** there is no complex setup, deployment, or infrastructure required.

- [ ] The project builds and runs with `dotnet run` from the project directory
- [ ] The dashboard is accessible at `https://localhost:5001` or `http://localhost:5000`
- [ ] No external NuGet packages beyond the .NET 8 SDK are required
- [ ] No database setup, migration, or seed script is needed
- [ ] `dotnet watch` enables hot reload for CSS and Razor changes during development

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository. Engineers MUST consult this file and the rendered screenshot (`OriginalDesignConcept.png`) for pixel-level accuracy.

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/main/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Page Layout

- **Viewport:** Fixed 1920×1080px, no scrolling, white background (`#FFFFFF`)
- **Font:** `'Segoe UI', Arial, sans-serif` throughout; base text color `#111`
- **Layout model:** Vertical flex column (`display: flex; flex-direction: column`) with three stacked sections:
  1. Header bar (flex-shrink: 0, ~48px height)
  2. Timeline area (flex-shrink: 0, 196px height)
  3. Heatmap area (flex: 1, fills remaining space)
- **Horizontal padding:** 44px on left and right for all three sections

### Section 1: Header Bar (`.hdr`)

- **Layout:** Flexbox row, `justify-content: space-between; align-items: center`
- **Padding:** `12px 44px 10px`
- **Border:** 1px solid `#E0E0E0` on bottom
- **Left side:**
  - Title: `<h1>` at `font-size: 24px; font-weight: 700` in `#111`
  - ADO link inline with title: `color: #0078D4; text-decoration: none`
  - Subtitle: `font-size: 12px; color: #888; margin-top: 2px`
- **Right side (Legend):**
  - Horizontal flex row with `gap: 22px`
  - Each legend item: `font-size: 12px` with an inline shape indicator:
    - PoC Milestone: 12×12px square rotated 45° (`transform: rotate(45deg)`), fill `#F4B400`
    - Production Release: 12×12px square rotated 45°, fill `#34A853`
    - Checkpoint: 8×8px circle, fill `#999`
    - Now indicator: 2×14px rectangle, fill `#EA4335`

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox row, `align-items: stretch`
- **Dimensions:** Height 196px, `flex-shrink: 0`
- **Background:** `#FAFAFA`
- **Border:** 2px solid `#E8E8E8` on bottom
- **Padding:** `6px 44px 0`

#### Track Label Sidebar (left)

- **Width:** 230px, `flex-shrink: 0`
- **Layout:** Flex column, `justify-content: space-around`
- **Padding:** `16px 12px 16px 0`
- **Border:** 1px solid `#E0E0E0` on right
- **Each track label:**
  - Track ID (e.g., "M1"): `font-size: 12px; font-weight: 600` in the track's color
  - Track name (e.g., "Chatbot & MS Role"): `font-weight: 400; color: #444` on a new line
  - Track colors from design: M1=`#0078D4`, M2=`#00897B`, M3=`#546E7A`

#### SVG Timeline Panel (right, `.tl-svg-box`)

- **Layout:** `flex: 1; padding-left: 12px; padding-top: 6px`
- **SVG dimensions:** Width 1560px, height 185px, `overflow: visible`
- **Month gridlines:** Vertical lines at equal intervals (260px apart for 6 months), stroke `#bbb` at 0.4 opacity
- **Month labels:** `font-size: 11px; font-weight: 600; fill: #666` positioned 5px right of each gridline, y=14
- **Track lines:** Horizontal lines spanning full SVG width, stroke=track color, `stroke-width: 3`
  - Track 1 (M1) at y=42
  - Track 2 (M2) at y=98
  - Track 3 (M3) at y=154
- **Milestone markers (by type):**
  - `"start"`: Open circle, `r=7`, white fill, colored stroke matching track, `stroke-width: 2.5`
  - `"checkpoint"`: Small filled circle, `r=4–5`, fill `#999` or white fill with gray stroke
  - `"poc"`: Diamond polygon (11px radius), fill `#F4B400`, with drop-shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`)
  - `"production"`: Diamond polygon (11px radius), fill `#34A853`, with same drop-shadow filter
- **Milestone labels:** `font-size: 10px; fill: #666; text-anchor: middle`, positioned above or below the marker
- **"NOW" line:** Vertical dashed line at the current date x-position, stroke `#EA4335`, `stroke-width: 2`, `stroke-dasharray: 5,3`. Label "NOW" in `font-size: 10px; font-weight: 700; fill: #EA4335`

### Section 3: Heatmap Area (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1; min-height: 0`
- **Padding:** `10px 44px 10px`

#### Heatmap Title

- `font-size: 14px; font-weight: 700; color: #888; letter-spacing: 0.5px; text-transform: uppercase; margin-bottom: 8px`
- Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"

#### Heatmap Grid (`.hm-grid`)

- **Layout:** CSS Grid
- **Columns:** `160px repeat(N, 1fr)` where N = number of months from data
- **Rows:** `36px repeat(4, 1fr)` — one header row (36px) + four data rows (equal-fill)
- **Border:** 1px solid `#E0E0E0` on the outer grid

##### Header Row

| Cell | Style |
|------|-------|
| Corner cell (`.hm-corner`) | Background `#F5F5F5`, text "STATUS" in `font-size: 11px; font-weight: 700; color: #999; text-transform: uppercase`, border-right 1px `#E0E0E0`, border-bottom 2px `#CCC` |
| Month headers (`.hm-col-hdr`) | `font-size: 16px; font-weight: 700; background: #F5F5F5`, centered text, border-right 1px `#E0E0E0`, border-bottom 2px `#CCC` |
| Current month header (`.apr-hdr`) | Override background `#FFF0D0`, text color `#C07700`, includes "◀ Now" indicator |

##### Data Rows — Color Specification

| Row | Header Class | Header BG | Header Text Color | Cell BG | Current Month Cell BG | Dot Color |
|-----|-------------|-----------|-------------------|---------|----------------------|-----------|
| ✅ Shipped | `.ship-hdr` | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| 🔵 In Progress | `.prog-hdr` | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| 🟡 Carryover | `.carry-hdr` | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| 🔴 Blockers | `.block-hdr` | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

##### Row Headers (`.hm-row-hdr`)

- `font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.7px`
- Padding: `0 12px`
- Border-right: 2px solid `#CCC`
- Border-bottom: 1px solid `#E0E0E0`

##### Data Cells (`.hm-cell`)

- Padding: `8px 12px`
- Border-right: 1px solid `#E0E0E0`
- Border-bottom: 1px solid `#E0E0E0`
- Each work item (`.it`): `font-size: 12px; color: #333; padding: 2px 0 2px 12px; line-height: 1.35`
- Colored dot: 6×6px circle via `::before` pseudo-element, positioned absolutely at `left: 0; top: 7px`
- Empty cells: Display `"-"` in `color: #AAA`

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Full Dashboard Render**
The user navigates to `https://localhost:5001` in Microsoft Edge. The browser loads the Blazor Server application, which reads `data.json` from disk, deserializes it, and renders the complete dashboard within a single viewport (1920×1080). The header, timeline, and heatmap all appear simultaneously with no loading spinner or progressive rendering. The "NOW" line on the timeline is positioned at the current system date (or the override date from `data.json`).

**Scenario 2: User Views the Header and Identifies the Project**
The user sees the project title (e.g., "Project Atlas Release Roadmap") in bold at the top-left, with the organizational subtitle below it. The legend at the top-right explains the four milestone marker types. The user clicks the "→ ADO Backlog" link and is taken to the Azure DevOps backlog in a new browser tab.

**Scenario 3: User Reads the Milestone Timeline**
The user looks at the timeline section and sees three horizontal track lines, each labeled on the left (M1, M2, M3) with colored IDs and descriptive names. Diamond markers indicate PoC and Production milestones with date labels. Small circles mark checkpoints. The red dashed "NOW" line shows the current position in time, allowing the user to see which milestones are past, current, and upcoming.

**Scenario 4: User Hovers Over a Milestone Diamond**
The milestone diamond has a subtle drop shadow that provides depth. No tooltip or popup is displayed — this is a static, screenshot-optimized view. The date label next to the diamond provides all necessary context. *(No interactive hover behavior is implemented by design.)*

**Scenario 5: User Scans the Heatmap for Current Month Status**
The user looks at the heatmap and immediately identifies the current month column by its gold-highlighted header (`#FFF0D0`). The deeper-tinted cells in that column draw the eye. The user reads the Shipped row to see recent deliverables, scans In Progress for active work, checks Carryover for items that slipped, and reviews Blockers for risks requiring escalation.

**Scenario 6: User Identifies a Blocked Item**
In the Blockers row (red), the user sees a work item name prefixed by a red dot. The item appears in the current month column, indicating an active blocker. The user notes this for discussion in the executive review meeting.

**Scenario 7: User Takes a Screenshot for PowerPoint**
The user presses `Win+Shift+S` (Windows Snipping Tool) or uses Edge's built-in screenshot feature to capture the full 1920×1080 viewport. The resulting image has clean edges, no scrollbars, and no browser chrome. The user pastes it into a PowerPoint slide at full-bleed size.

**Scenario 8: Data-Driven Rendering — Project Lead Adds a New Work Item**
The project lead opens `data.json` in a text editor, adds a new entry to the `items` array (e.g., `{"name": "API Gateway v2", "status": "in-progress", "month": "Apr"}`), saves the file, restarts the app with `dotnet run`, and refreshes the browser. The new item appears in the In Progress row under the April column.

**Scenario 9: Data-Driven Rendering — New Milestone Added**
The project lead adds a new milestone to the `milestones` array in `data.json` with a date and type. After restarting, the timeline SVG recalculates positions and renders the new marker at the correct x-coordinate proportional to the date.

**Scenario 10: Data-Driven Rendering — Month Columns Adjusted**
The project lead changes the `months` array in `data.json` from `["Jan","Feb","Mar","Apr","May","Jun"]` to `["Mar","Apr","May","Jun","Jul","Aug"]` to shift the reporting window. After restart, both the timeline and heatmap re-render with the new month range.

**Scenario 11: Empty State — No Work Items for a Month**
A month column has no work items for a given status row. The cell displays a muted dash (`-`) in `#AAA` color, indicating no data rather than a rendering error.

**Scenario 12: Error State — Missing or Malformed data.json**
The user deletes or corrupts `data.json` and runs the application. Instead of an unhandled exception page, the dashboard displays a clear error message: "Unable to load dashboard data. Please check that data.json exists and contains valid JSON." The error message includes the expected file path.

**Scenario 13: Error State — Empty data.json**
The `data.json` file exists but contains empty arrays (no tracks, no milestones, no items). The dashboard renders the header with metadata, an empty timeline area (track lines with no markers), and a heatmap grid with all cells showing dashes. No crash occurs.

**Scenario 14: Fixed Viewport — No Responsive Behavior**
The user resizes the browser window to a smaller size. The dashboard does not reflow or adapt — it is designed for exactly 1920×1080. Content may be clipped or scrollbars may appear at smaller sizes. This is acceptable; the user is instructed to use the dashboard at full viewport size for screenshot capture.

## Scope

### In Scope

- Single-page Blazor Server (.NET 8) web application
- Header section with project title, subtitle, ADO backlog link, and milestone legend
- SVG milestone timeline with multiple tracks, dynamic date-based positioning, and "NOW" indicator
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- Data loading from a local `data.json` file using `System.Text.Json`
- C# record-based data models for metadata, tracks, milestones, and work items
- Singleton `DashboardDataService` registered in DI for JSON loading and caching
- `TimelineCalculator` helper for date-to-pixel coordinate mapping
- Fictional sample project data ("Project Atlas" or similar) pre-loaded in `data.json`
- CSS ported from `OriginalDesignConcept.html` with refinements for executive polish
- Fixed 1920×1080 viewport optimized for screenshot capture
- Documentation of the `data.json` schema with inline comments in the sample file
- Graceful error handling for missing or malformed `data.json`

### Out of Scope

- ❌ Authentication or authorization (no login, no roles, no middleware)
- ❌ Database of any kind (no SQLite, LiteDB, EF Core, or other persistence)
- ❌ API endpoints (no REST controllers, no minimal API routes)
- ❌ Client-side JavaScript or interactivity beyond page load
- ❌ Third-party charting libraries (no Radzen Charts, ApexCharts, Syncfusion)
- ❌ Third-party CSS frameworks (no Bootstrap, Tailwind, MudBlazor)
- ❌ Docker containerization
- ❌ CI/CD pipeline
- ❌ Configuration UI or admin panel (edit `data.json` directly)
- ❌ Export, print, or PDF generation (use browser/OS screenshot tools)
- ❌ Responsive or mobile layout (fixed 1920×1080 only)
- ❌ Real-time data refresh or file watchers (manual restart after `data.json` edits)
- ❌ ADO or external system data import pipeline
- ❌ Multi-page navigation or routing beyond the single dashboard page
- ❌ User preferences, themes, or dark mode
- ❌ Automated testing infrastructure (optional; not required for MVP)

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Page load time (first meaningful paint) | < 2 seconds on localhost |
| `data.json` deserialization time | < 100ms for files up to 50KB |
| SVG timeline render | < 500ms for up to 50 milestones across 10 tracks |
| Memory footprint | < 100MB RSS (Blazor Server baseline + cached data) |

### Security

- No authentication or authorization required (local-only application)
- `data.json` contains non-sensitive project status information; no encryption needed
- No PII, credentials, or secrets stored in any project file
- If deployed to a shared server in the future, `data.json` should be moved out of `wwwroot/` to prevent public HTTP access

### Reliability

- The application must start successfully with a valid `data.json` file on every invocation of `dotnet run`
- Missing or malformed `data.json` must produce a user-friendly error message, not an unhandled exception
- The application must render correctly in Microsoft Edge (Chromium) on Windows 10/11

### Compatibility

- Target runtime: .NET 8 SDK on Windows 10/11
- Target browser: Microsoft Edge (Chromium), latest stable version
- Target resolution: 1920×1080 at 100% DPI scaling
- Font dependency: `Segoe UI` (pre-installed on all Windows machines)

### Maintainability

- Zero external NuGet dependencies (all functionality from .NET 8 SDK)
- Total solution should be under 10 files (excluding `bin/`, `obj/`, `Properties/`)
- `data.json` schema must be self-documenting with descriptive field names

## Success Metrics

1. **Visual fidelity:** The rendered dashboard matches the `OriginalDesignConcept.html` design with improvements, verified by side-by-side visual comparison at 1920×1080 in Microsoft Edge.
2. **Screenshot quality:** A full-viewport screenshot of the dashboard can be pasted into a PowerPoint slide and is legible, professional, and indistinguishable from a hand-designed graphic.
3. **Data-driven rendering:** Modifying any value in `data.json` (adding/removing milestones, changing work item status, adjusting months) correctly updates the dashboard after an application restart — no code changes required.
4. **Zero-setup deployment:** A new developer can clone the repository and run `dotnet run` to see the dashboard with no additional setup, database migrations, package installs, or configuration.
5. **Sub-10-file solution:** The entire application (excluding build artifacts) consists of fewer than 10 source files, demonstrating the simplicity goal.
6. **Error resilience:** The application handles missing, empty, and malformed `data.json` gracefully with user-friendly error messages in all three cases.

## Constraints & Assumptions

### Technical Constraints

- **Technology stack:** Must use C# .NET 8 with Blazor Server — this is a mandated stack, not a choice.
- **No external dependencies:** Zero NuGet packages beyond what ships with the .NET 8 SDK. `System.Text.Json` and `Microsoft.AspNetCore.Components` are SDK-included.
- **Fixed viewport:** Designed exclusively for 1920×1080; responsive behavior is explicitly not required.
- **Windows-only font:** `Segoe UI` is the design font and is only guaranteed available on Windows. The dashboard is not intended for macOS or Linux use.
- **Local execution only:** The application runs on `localhost` via Kestrel. No cloud hosting, no public URL, no TLS certificate management.

### Timeline Assumptions

- **Phase 1 (Day 1):** Scaffold Blazor project, port static HTML/CSS from design, verify visual fidelity.
- **Phase 2 (Day 1–2):** Implement data models, JSON loading service, and data-driven rendering.
- **Phase 3 (Day 2):** Build dynamic SVG timeline with `TimelineCalculator` helper.
- **Phase 4 (Day 2–3):** Create fictional project data, polish visuals, test screenshot workflow.
- **Total estimated effort:** 2–3 days for a developer familiar with Blazor and CSS.

### Dependency Assumptions

- The developer has .NET 8 SDK installed on their Windows workstation.
- Microsoft Edge (Chromium) is available for viewing and screenshotting the dashboard.
- The `OriginalDesignConcept.html` file in the ReportingDashboard repository is the canonical design reference and has been reviewed by all engineers before implementation begins.
- The `data.json` schema is stable for MVP; no runtime schema evolution or migration is needed.
- The "NOW" line auto-calculates from `DateTime.Now` by default, with an optional `currentDate` override field in `data.json` for generating historical or future-dated snapshots.
- The heatmap month columns and timeline month range are both driven by the same `months` array in `data.json`, ensuring consistency between the two sections.