# PM Specification: Executive Project Reporting Dashboard

**Document Version:** 1.0
**Date:** April 17, 2026
**Author:** Program Management
**Status:** Draft
**Project Codename:** ReportingDashboard

---

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes a project's milestone timeline, delivery status, and execution health — all driven by a `data.json` configuration file. The dashboard is designed to be screenshot-captured at 1920×1080 resolution and pasted directly into PowerPoint decks for executive reviews, replacing manually-maintained slides with a data-driven, always-consistent visual. The solution is a minimal Blazor Server application with zero external dependencies, deployable with `dotnet run` on a developer's local machine.

---

## Business Goals

1. **Eliminate manual slide creation** — Replace hand-crafted PowerPoint status slides with a single, data-driven dashboard that produces pixel-perfect screenshots in seconds.
2. **Provide at-a-glance project visibility** — Give executives a single-page view showing what shipped, what's in progress, what carried over from last month, and what's blocked — organized by month.
3. **Visualize milestone timelines** — Display project "big rocks" on a horizontal timeline with clear markers for checkpoints, PoC milestones, and production releases, plus a "NOW" indicator.
4. **Enable rapid updates** — Allow a project manager to update a single `data.json` file and immediately see an updated dashboard without code changes, database migrations, or redeployments.
5. **Maintain zero operational overhead** — No cloud hosting, no authentication, no database, no CI/CD pipeline. Total infrastructure cost: $0. Total setup time: `dotnet run`.
6. **Match or exceed the visual quality of the original HTML design** — The rendered dashboard must be visually indistinguishable from the `OriginalDesignConcept.html` reference at 1920×1080, with improvements where possible.

---

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project manager preparing a status deck, **I want** to see the project name, workstream, current period, and a link to the ADO backlog at the top of the dashboard, **so that** every screenshot is self-documenting and executives know exactly which project and time period they're looking at.

**Visual Reference:** `OriginalDesignConcept.html` — `.hdr` section (Header bar)

**Acceptance Criteria:**
- [ ] The header displays the project name as an H1 element (24px, bold, #111)
- [ ] An optional ADO Backlog hyperlink appears inline with the project name (colored #0078D4)
- [ ] If `backlogUrl` is empty in `data.json`, the link is not rendered
- [ ] A subtitle line displays the workstream name, organization context, and current period (12px, #888)
- [ ] A legend appears on the right side of the header showing four marker types: PoC Milestone (amber diamond), Production Release (green diamond), Checkpoint (gray circle), and Now line (red vertical bar)
- [ ] All header content is read from `data.json`

---

### US-2: View Milestone Timeline

**As an** executive reviewing the status deck, **I want** to see a horizontal timeline showing project milestones across a multi-month span with clear visual markers, **so that** I can immediately understand the project's trajectory and where we are relative to key dates.

**Visual Reference:** `OriginalDesignConcept.html` — `.tl-area` section (Timeline area with SVG)

**Acceptance Criteria:**
- [ ] The timeline renders as an inline SVG element, 1560px wide × 185px tall
- [ ] Vertical gridlines appear at each month boundary with month abbreviation labels (11px, #666, Segoe UI)
- [ ] A dashed red vertical "NOW" line (#EA4335, stroke-width 2, dash pattern 5,3) appears at the position corresponding to the configured current date
- [ ] The word "NOW" appears at the top of the NOW line in red bold text (10px, #EA4335)
- [ ] Each milestone track renders as a horizontal colored line (stroke-width 3) at a distinct Y position
- [ ] Track labels appear in a left sidebar (230px wide) showing track ID (e.g., "M1") and description
- [ ] Checkpoint events render as circles (r=4–7, white fill with colored stroke, or solid #999 for minor checkpoints)
- [ ] PoC milestone events render as amber (#F4B400) diamond shapes with drop shadow
- [ ] Production release events render as green (#34A853) diamond shapes with drop shadow
- [ ] Date labels appear near each event marker (10px, #666, text-anchor middle)
- [ ] The number of tracks is dynamic — driven by the `tracks` array in `data.json`
- [ ] Adding or removing milestones in `data.json` and restarting the app correctly updates the timeline

---

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what shipped, what's in progress, what carried over, and what's blocked — organized by month, **so that** I can assess execution health at a glance without reading a detailed status report.

**Visual Reference:** `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` sections (Heatmap grid)

**Acceptance Criteria:**
- [ ] A section title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" (14px, #888, uppercase, letter-spacing 0.5px)
- [ ] The grid renders with CSS Grid: first column 160px (row headers), remaining columns equal-width (one per month)
- [ ] The header row shows month names (16px, bold) with the current month highlighted in amber background (#FFF0D0, text #C07700) and suffixed with "◀ Now"
- [ ] Four status rows appear: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each row header displays the category name with an emoji prefix, uppercase, in the category's dark color variant
- [ ] Each data cell displays a list of work items as bullet-pointed text (12px, #333) with a small colored circle (6px) as the bullet
- [ ] Cells in the current month column have a darker background tint than other months
- [ ] Empty cells (future months with no data) display a gray dash "—"
- [ ] The number of months displayed is dynamic — driven by the `months` array in `data.json`
- [ ] All status items are read from the `categories` array in `data.json`

---

### US-4: Configure Dashboard via JSON

**As a** project manager, **I want** to edit a single `data.json` file to update all dashboard content — project name, milestones, status items, months, and current period, **so that** I never need to edit code or HTML to prepare a new status screenshot.

**Acceptance Criteria:**
- [ ] The app reads `data.json` from the `wwwroot/data/` directory at startup
- [ ] The JSON schema supports: `projectName`, `workstream`, `period`, `backlogUrl`, `months`, `currentMonthIndex`, `tracks` (with nested events), and `categories` (with nested month data)
- [ ] Missing optional fields (e.g., empty `backlogUrl`) gracefully degrade without errors
- [ ] Invalid JSON produces a clear error message on the dashboard page, not a crash
- [ ] A schema version field (`schemaVersion: 1`) is included for future migration support
- [ ] Changing `data.json` and restarting the app reflects the new data on the dashboard
- [ ] The example `data.json` ships with fictional "Project Phoenix" data containing 3–4 milestone tracks and 20+ status items across all four categories

---

### US-5: Capture Screenshot for PowerPoint

**As a** project manager, **I want** the dashboard to render cleanly at exactly 1920×1080 pixels with no scrollbars, browser chrome artifacts, or overflow, **so that** I can screenshot the page and paste it directly into a PowerPoint slide at full resolution.

**Acceptance Criteria:**
- [ ] The `<body>` element is styled at exactly `width: 1920px; height: 1080px; overflow: hidden`
- [ ] All content fits within the viewport without scrolling
- [ ] The heatmap grid uses `flex: 1` to fill remaining vertical space after the header and timeline
- [ ] No Blazor framework UI (error boundaries, reconnect dialogs) is visible during normal operation
- [ ] The page renders identically in Chrome, Edge, and Firefox at 1920×1080 viewport
- [ ] A print stylesheet (`@media print`) removes margins for clean PDF export as a stretch goal

---

### US-6: Run Dashboard Locally with Zero Setup

**As a** developer or PM, **I want** to run the dashboard with a single `dotnet run` command and no additional setup, **so that** I don't waste time configuring databases, auth providers, or cloud services.

**Acceptance Criteria:**
- [ ] The project builds and runs with `dotnet run` using only the .NET 8 SDK
- [ ] Zero external NuGet packages are required for the core application
- [ ] The app serves on `http://localhost:5000` (or configurable port)
- [ ] No database connection strings, API keys, or environment variables are required
- [ ] The solution contains fewer than 15 files total

---

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository. Engineers MUST consult this file and the rendered screenshot (`docs/design-screenshots/OriginalDesignConcept.png`) for pixel-level guidance.

![OriginalDesignConcept design reference](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/main/docs/design-screenshots/OriginalDesignConcept.png)

### Overall Page Layout

- **Viewport:** Fixed 1920×1080px, no scroll, white (#FFFFFF) background
- **Layout model:** Flexbox column (`display: flex; flex-direction: column`) for the full page
- **Font family:** `'Segoe UI', Arial, sans-serif` — no web fonts, no font loading
- **Base text color:** #111
- **Three vertical sections** stacked top-to-bottom:
  1. **Header** (flex-shrink: 0, ~50px tall)
  2. **Timeline area** (flex-shrink: 0, fixed 196px tall)
  3. **Heatmap grid** (flex: 1, fills remaining space ~834px)

### Section 1: Header Bar (`.hdr`)

- **Padding:** 12px top, 44px horizontal, 10px bottom
- **Border:** 1px solid #E0E0E0 on bottom
- **Layout:** Flexbox row, space-between alignment
- **Left side:**
  - Project name: `<h1>`, 24px, font-weight 700, color #111
  - ADO Backlog link: inline `<a>`, color #0078D4, no underline, preceded by "→" character
  - Subtitle: `<div class="sub">`, 12px, color #888, margin-top 2px, format: `{Organization} · {Workstream} · {Period}`
- **Right side (Legend):**
  - Flexbox row, gap 22px, vertically centered
  - Four legend items, each 12px font:
    - **PoC Milestone:** 12×12px amber (#F4B400) square rotated 45° (diamond shape)
    - **Production Release:** 12×12px green (#34A853) square rotated 45° (diamond shape)
    - **Checkpoint:** 8×8px circle, background #999
    - **Now indicator:** 2×14px rectangle, background #EA4335

### Section 2: Timeline Area (`.tl-area`)

- **Height:** 196px fixed
- **Background:** #FAFAFA
- **Border:** 2px solid #E8E8E8 on bottom
- **Padding:** 6px top, 44px horizontal
- **Layout:** Flexbox row, align-items stretch
- **Left sidebar** (230px wide, flex-shrink 0):
  - Border-right: 1px solid #E0E0E0
  - Padding: 16px vertical, 12px right
  - Contains track labels, vertically distributed with `justify-content: space-around`
  - Each label: Track ID in bold 12px with track color (e.g., M1 in #0078D4), description below in 12px #444 normal weight
- **SVG area** (`.tl-svg-box`, flex: 1):
  - Padding-left: 12px, padding-top: 6px
  - Contains the `<svg>` element: width 1560px, height 185px, overflow visible
  - **SVG drop shadow filter:** `<filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter>`
  - **Month gridlines:** Vertical lines at each month boundary, stroke #bbb, opacity 0.4, width 1. Month labels at (x+5, y=14), 11px, font-weight 600, #666
  - **NOW line:** Dashed vertical line, stroke #EA4335, width 2, dash-array "5,3". "NOW" text label at top, 10px, bold, #EA4335
  - **Track lines:** Horizontal lines spanning full SVG width, stroke-width 3, color per track:
    - Track 1: #0078D4 (blue) at Y=42
    - Track 2: #00897B (teal) at Y=98
    - Track 3: #546E7A (gray-blue) at Y=154
  - **Event markers by type:**
    - `checkpoint` (major): Circle, r=5–7, white fill, track-colored stroke, stroke-width 2.5
    - `checkpoint` (minor): Circle, r=4, solid fill #999
    - `poc`: Diamond polygon (11px radius), fill #F4B400, with drop shadow filter
    - `production`: Diamond polygon (11px radius), fill #34A853, with drop shadow filter
  - **Date labels:** Near each marker, 10px, #666, text-anchor middle. Positioned above or below the track line to avoid overlap

### Section 3: Heatmap Grid (`.hm-wrap`, `.hm-grid`)

- **Padding:** 10px top/bottom, 44px horizontal
- **Layout:** Flexbox column, flex: 1 (fills remaining page height)
- **Section title** (`.hm-title`): 14px, bold, #888, uppercase, letter-spacing 0.5px, margin-bottom 8px
- **Grid** (`.hm-grid`): CSS Grid, flex: 1
  - **Columns:** `160px repeat(N, 1fr)` where N = number of months
  - **Rows:** `36px repeat(4, 1fr)` — 36px header row, 4 equal data rows
  - **Border:** 1px solid #E0E0E0 on outer edge

#### Grid Header Row

| Cell | Class | Style |
|------|-------|-------|
| Corner cell | `.hm-corner` | Background #F5F5F5, 11px bold uppercase #999, border-right 1px #E0E0E0, border-bottom 2px #CCC. Text: "STATUS" |
| Month headers | `.hm-col-hdr` | Background #F5F5F5, 16px bold, centered, border-right 1px #E0E0E0, border-bottom 2px #CCC |
| Current month header | `.hm-col-hdr.apr-hdr` | Background #FFF0D0, color #C07700, text suffixed with " ◀ Now" |

#### Grid Data Rows — Color Scheme

| Category | Row Header Class | Header BG | Header Text | Cell BG | Current Month Cell BG | Bullet Color |
|----------|-----------------|-----------|-------------|---------|----------------------|--------------|
| **Shipped** | `.ship-hdr` | #E8F5E9 | #1B7A28 | #F0FBF0 | #D8F2DA | #34A853 |
| **In Progress** | `.prog-hdr` | #E3F2FD | #1565C0 | #EEF4FE | #DAE8FB | #0078D4 |
| **Carryover** | `.carry-hdr` | #FFF8E1 | #B45309 | #FFFDE7 | #FFF0B0 | #F4B400 |
| **Blockers** | `.block-hdr` | #FEF2F2 | #991B1B | #FFF5F5 | #FFE4E4 | #EA4335 |

#### Grid Data Cells

- **Class:** `.hm-cell` with category modifier (e.g., `.ship-cell`, `.prog-cell`)
- **Padding:** 8px 12px
- **Borders:** 1px solid #E0E0E0 on right and bottom
- **Overflow:** hidden
- **Items** (`.it`): 12px, #333, line-height 1.35, padding 2px 0 2px 12px (room for bullet)
- **Bullet:** CSS `::before` pseudo-element, 6×6px circle, absolutely positioned at left:0 top:7px, colored per category

### CSS Custom Properties (Recommended)

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
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-carryover-hdr-bg: #FFF8E1;
    --color-carryover-hdr-text: #B45309;
    --color-blocker: #EA4335;
    --color-blocker-bg: #FFF5F5;
    --color-blocker-bg-current: #FFE4E4;
    --color-blocker-hdr-bg: #FEF2F2;
    --color-blocker-hdr-text: #991B1B;
    --color-now-line: #EA4335;
    --color-poc-milestone: #F4B400;
    --color-prod-milestone: #34A853;
    --color-checkpoint: #999;
    --color-link: #0078D4;
    --color-current-month-hdr-bg: #FFF0D0;
    --color-current-month-hdr-text: #C07700;
}
```

### Component-to-CSS Mapping

| Blazor Component | Design CSS Classes | Responsibility |
|------------------|--------------------|----------------|
| `Header.razor` | `.hdr`, `.sub` | Project title, subtitle, legend icons |
| `Timeline.razor` | `.tl-area`, `.tl-svg-box` | Track sidebar + inline SVG with milestones |
| `HeatmapGrid.razor` | `.hm-wrap`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr` | Grid container, header row, section title |
| `HeatmapCell.razor` | `.hm-cell`, `.hm-row-hdr`, `.it`, `.[category]-cell`, `.[category]-hdr` | Individual cell rendering with category styling |

---

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Full Dashboard Render**
The user navigates to `http://localhost:5000`. The app reads `data.json` and renders the complete dashboard in a single page: header with project name and legend at top, milestone timeline in the middle section, and the heatmap grid filling the remaining space. All content is visible without scrolling at 1920×1080.

**Scenario 2: User Views Header and Identifies the Project**
The user sees the project name "Project Phoenix — Cloud Migration Platform" in bold at the top-left, with a clickable "→ ADO Backlog" link next to it. Below the title, a subtitle reads "Cloud Engineering · Migration Workstream · April 2026". On the right, four legend items explain the marker types used in the timeline.

**Scenario 3: User Reads the Milestone Timeline**
The user looks at the timeline section and sees three horizontal track lines spanning January through June. Vertical gridlines mark each month. A dashed red "NOW" line indicates the current date (mid-April). Diamond markers in amber (PoC) and green (Production) appear at milestone dates. Circle markers show completed checkpoints. Date labels appear near each marker.

**Scenario 4: User Scans the Heatmap for Execution Health**
The user's eye is drawn to the heatmap grid below the timeline. The current month column (April) is visually highlighted with a warmer background. The "Shipped" row in green shows completed items. The "In Progress" row in blue shows active work. The "Carryover" row in amber shows items that slipped from last month. The "Blockers" row in red shows items that need escalation. Each cell contains bullet-pointed work item descriptions.

**Scenario 5: User Identifies Blockers Requiring Attention**
The user scans the red "Blockers" row and sees 1–2 items in the current month's cell with red bullet points. The visual weight of the red background immediately draws attention to items needing executive action.

**Scenario 6: User Clicks the ADO Backlog Link**
The user clicks the "→ ADO Backlog" link in the header. The browser navigates to the Azure DevOps backlog URL configured in `data.json`. If no URL is configured, the link is not displayed.

**Scenario 7: User Captures a Screenshot for PowerPoint**
The user opens Chrome DevTools, sets the viewport to 1920×1080 using device mode, and captures a full-page screenshot. The resulting PNG is exactly 1920×1080 pixels with no browser chrome, scrollbars, or overflow artifacts. The user pastes it into a PowerPoint slide at full bleed.

**Scenario 8: Dashboard Renders with Minimal Data**
The user creates a `data.json` with only one month, one track with zero events, and one category with zero items. The dashboard renders gracefully: the timeline shows a single track line with no markers, the heatmap shows one month column with empty cells displaying gray dashes, and no errors appear.

**Scenario 9: Dashboard Renders with Maximum Data**
The user creates a `data.json` with 6 months, 5 tracks with 15+ events, and all four categories with 5+ items per cell. The dashboard renders without overflow — text in cells is clipped cleanly, the timeline distributes markers across the full width, and the grid rows share vertical space equally.

**Scenario 10: User Provides Invalid JSON**
The user accidentally corrupts `data.json` (missing comma, invalid syntax). On app restart, the dashboard page displays a clear, human-readable error message (e.g., "Error reading data.json: Unexpected character at line 12") instead of a blank page or unhandled exception.

**Scenario 11: User Updates data.json and Restarts**
The user edits `data.json` to change the project name, add a new milestone, and move an item from "In Progress" to "Shipped." The user restarts the app with `dotnet run`. The dashboard reflects all changes: new project name in the header, new diamond on the timeline, and the work item now appears in the Shipped row.

**Scenario 12: Hover State on Timeline Milestones (Enhancement)**
When the user hovers over a milestone diamond on the timeline, a browser-native tooltip appears showing the milestone label and date (e.g., "Mar 26 — PoC Complete"). This uses the SVG `<title>` element for zero-JavaScript implementation.

---

## Scope

### In Scope

- Single-page Blazor Server dashboard matching the `OriginalDesignConcept.html` design
- Header component with project name, subtitle, ADO Backlog link, and milestone legend
- Inline SVG timeline with dynamic date-to-pixel mapping, track lines, milestone markers (checkpoint/PoC/production), month gridlines, and NOW line
- CSS Grid heatmap with four status categories (Shipped, In Progress, Carryover, Blockers) × N months
- Current month highlighting with distinct background colors
- `data.json` flat file as the sole data source (placed in `wwwroot/data/`)
- Strongly-typed C# record models for JSON deserialization
- Singleton `ProjectDataService` that reads and caches `data.json` at startup
- Example `data.json` with fictional "Project Phoenix" project data (3–4 tracks, 20+ status items)
- CSS extracted from `OriginalDesignConcept.html` into `app.css` with CSS custom properties
- Fixed 1920×1080 viewport optimized for screenshot capture
- SVG drop shadow filter on milestone diamonds
- Graceful handling of missing/optional JSON fields via default values
- Schema version field in `data.json` for future compatibility
- SVG `<title>` tooltips on milestone markers
- Print stylesheet (`@media print`) for clean PDF export (stretch)

### Out of Scope

- ❌ Authentication or authorization of any kind
- ❌ Database (SQLite, SQL Server, CosmosDB, etc.)
- ❌ Entity Framework or any ORM
- ❌ API controllers or REST endpoints
- ❌ Custom SignalR hubs
- ❌ Docker containerization
- ❌ CI/CD pipeline or GitHub Actions workflows
- ❌ Logging infrastructure beyond `Console.WriteLine`
- ❌ CSS frameworks (Bootstrap, Tailwind, etc.)
- ❌ JavaScript charting libraries (Chart.js, ApexCharts, D3, etc.)
- ❌ Component libraries (MudBlazor, Radzen, Syncfusion, etc.)
- ❌ Responsive/mobile design — fixed 1920×1080 only
- ❌ Real-time data updates or live refresh
- ❌ Multi-project/multi-page navigation
- ❌ User settings or preferences
- ❌ Cloud deployment or Azure hosting
- ❌ Accessibility (WCAG) compliance beyond semantic HTML
- ❌ Internationalization or localization
- ❌ Built-in screenshot capture functionality
- ❌ Data editing UI — JSON is edited manually in a text editor

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** | < 1 second on localhost (first meaningful paint) |
| **JSON parse time** | < 50ms for a `data.json` up to 50KB |
| **SVG render time** | < 200ms for up to 5 tracks × 20 events |
| **Memory usage** | < 100MB RSS (including .NET runtime overhead) |
| **Startup time** | < 3 seconds from `dotnet run` to serving requests |

### Reliability

- The app must not crash on malformed `data.json` — display a clear error message instead
- The app must handle missing optional fields gracefully using C# record default values
- No unhandled exceptions should reach the browser — use Blazor error boundaries

### Security

- **No authentication required** — this is a localhost-only tool
- **No PII or secrets** stored in `data.json`
- **HTTP only** — no HTTPS certificate management needed for localhost
- **No external network calls** — the app is fully self-contained
- If future network sharing is needed, implement `BasicAuthMiddleware` with a single shared password

### Maintainability

- Solution must contain **fewer than 15 files** in the core project
- **Zero external NuGet dependencies** for the core app (only built-in .NET 8 SDK packages)
- All styles in a single `app.css` file — no scattered inline styles except where matching the design requires it
- Strongly-typed data models — no anonymous objects or `dynamic` types

### Visual Fidelity

- Dashboard must be **visually indistinguishable** from `OriginalDesignConcept.html` when rendered at 1920×1080 in Chrome
- All hex colors, font sizes, spacing values, and layout dimensions must match the design reference exactly
- Screenshots captured from the dashboard must be **presentation-ready** for PowerPoint without post-processing

---

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Design fidelity** | ≥95% visual match to `OriginalDesignConcept.html` | Side-by-side comparison at 1920×1080 |
| 2 | **Data-driven rendering** | 100% of visible content sourced from `data.json` | Change every field in JSON, verify all changes appear |
| 3 | **Screenshot readiness** | Screenshot is usable in PowerPoint without editing | Paste into a 16:9 slide; all text is legible, no clipping |
| 4 | **Setup simplicity** | Working dashboard within 60 seconds of cloning repo | Time from `git clone` to browser showing dashboard |
| 5 | **Update cycle time** | < 2 minutes to update data and capture a new screenshot | Time from opening `data.json` to pasting into PowerPoint |
| 6 | **Zero-dependency build** | `dotnet build` succeeds with only .NET 8 SDK installed | Build on a clean machine with no pre-installed NuGet packages |
| 7 | **File count** | ≤ 15 files in core project (excluding obj/bin) | `Get-ChildItem -Recurse -File` count |
| 8 | **Fictional data completeness** | Example `data.json` includes ≥3 tracks and ≥20 status items | Count items in the shipped JSON file |

---

## Constraints & Assumptions

### Technical Constraints

- **Runtime:** .NET 8.0 LTS SDK must be installed on the target machine
- **Framework:** Blazor Server (not Blazor WebAssembly) — simplest hosting model for local use
- **Viewport:** Fixed at 1920×1080px — no responsive breakpoints
- **Browser:** Chrome is the primary target for screenshot capture; Edge and Firefox should render acceptably
- **Font:** Segoe UI must be available on the host machine (standard on Windows; falls back to Arial on other OS)
- **Data format:** JSON only — no YAML, TOML, or XML support
- **SVG:** All timeline rendering via inline SVG in Razor — no JavaScript, no canvas, no external charting libraries
- **CSS:** Single `app.css` file extracted from the design HTML — no preprocessors (SASS/LESS), no CSS-in-JS

### Timeline Assumptions

- **Phase 1** (Scaffold + Static Layout): 1 day
- **Phase 2** (Data Model + JSON Binding): 0.5–1 day
- **Phase 3** (Dynamic SVG Timeline): 1 day
- **Phase 4** (Polish + Example Data): 0.5–1 day
- **Total estimated effort:** 3–4 developer-days

### Dependency Assumptions

- The `OriginalDesignConcept.html` file in the ReportingDashboard repo is the **canonical design reference** and will not change during development
- The `C:/Pics/ReportingDashboardDesign.png` file provides supplementary visual guidance and should be consulted alongside the HTML reference
- The .NET 8 SDK is already installed or can be installed without IT approval
- No external services (Azure, GitHub API, npm registry) are needed at runtime
- The developer has Chrome installed for screenshot capture workflow

### Data Assumptions

- The `data.json` file will be manually edited by a single project manager
- Project data is not sensitive (no PII, no credentials, no ITAR-restricted content)
- The JSON file will remain under 50KB (sufficient for 6 months × 4 categories × 10 items each)
- The `currentMonthIndex` will be explicitly set in JSON rather than auto-detected from the system clock (allows pre-staging dashboards before presentations)
- Month labels are short strings (e.g., "Jan", "Feb") — not full date ranges

### Design Assumptions

- The dashboard is a **read-only** view — no editing, no input forms, no interactivity beyond hyperlinks
- The primary output is a **static screenshot** — animation, transitions, and hover effects are nice-to-have but not required
- The heatmap grid will display **exactly 4 status categories** (Shipped, In Progress, Carryover, Blockers) — this is not configurable
- The number of **months** (columns) and **milestone tracks** (rows in timeline) are variable and driven by data
- All text is in English — no RTL support needed