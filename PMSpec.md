# PM Specification: Executive Project Reporting Dashboard

**Document Version:** 1.0
**Date:** April 17, 2026
**Author:** Program Management
**Status:** Draft for Review

---

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes a project's milestone timeline, shipped deliverables, in-progress work, carryover items, and blockers in a clean, screenshot-ready layout optimized for PowerPoint presentations. The dashboard reads all data from a local `data.json` configuration file and renders a pixel-perfect view at 1920×1080 resolution using Blazor Server (.NET 8), requiring zero authentication, no database, and no cloud infrastructure — enabling any project lead to generate professional executive status views by simply editing a JSON file and running `dotnet run`.

---

## Business Goals

1. **Reduce executive reporting preparation time** — Eliminate manual PowerPoint slide construction by providing a live, data-driven dashboard that can be screenshotted directly into presentation decks.
2. **Standardize project status communication** — Provide a consistent visual format for reporting milestones, shipped work, in-progress items, carryover, and blockers across all projects.
3. **Enable self-service reporting** — Allow any project lead to generate their own executive dashboard by editing a single JSON file, with no developer assistance required.
4. **Maximize visual clarity for leadership** — Deliver a clean, information-dense single-page view that executives can absorb in under 30 seconds, with color-coded status categories and a timeline showing past and future milestones.
5. **Minimize operational overhead** — Zero infrastructure cost, zero authentication complexity, zero deployment pipeline. The tool runs locally on the user's machine with `dotnet run`.

---

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** to see the project title, organizational context, current month, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are looking at.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — top bar with title, subtitle, and legend.

**Acceptance Criteria:**
- [ ] The header displays the project title in bold 24px font weight 700
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, styled in #0078D4
- [ ] The subtitle line shows organization, workstream, and current month in 12px gray (#888) text
- [ ] A legend appears on the right side of the header showing four indicator types: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now line (red vertical bar)
- [ ] All text values are driven from `data.json` fields: `title`, `subtitle`, `backlogLink`

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing key milestone tracks with markers for checkpoints, PoC milestones, and production releases, **so that** I can quickly understand the project's trajectory and where we are relative to planned dates.

**Visual Reference:** Timeline area (`.tl-area`) of `OriginalDesignConcept.html` — left-side track labels + SVG visualization.

**Acceptance Criteria:**
- [ ] The timeline area is 196px tall with a #FAFAFA background
- [ ] A left sidebar (230px wide) lists each milestone track with its ID (e.g., "M1") in the track color and its label in #444
- [ ] The SVG area renders vertical month grid lines with month labels (Jan–Jun) at the top
- [ ] Each milestone track renders as a horizontal colored line spanning the full SVG width
- [ ] Checkpoint events render as circles (white fill, colored stroke or small gray filled circles)
- [ ] PoC milestones render as gold (#F4B400) diamonds with drop shadow
- [ ] Production releases render as green (#34A853) diamonds with drop shadow
- [ ] A dashed red (#EA4335) vertical "NOW" line appears at the current date position with a "NOW" label
- [ ] Date labels appear above or below each milestone marker
- [ ] The timeline supports 1–5 tracks with automatic Y-spacing
- [ ] All timeline data is driven from `data.json` → `timeline.tracks[]`

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked — organized by month, **so that** I can assess execution health at a glance.

**Visual Reference:** Heatmap grid (`.hm-wrap` / `.hm-grid`) of `OriginalDesignConcept.html`.

**Acceptance Criteria:**
- [ ] The heatmap has a section title "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase 14px gray (#888) text
- [ ] The grid uses CSS Grid with columns: 160px row header + one column per month (1fr each)
- [ ] The grid has 5 rows: header row (36px) + 4 status category rows (equal flex)
- [ ] Column headers display month names in bold 16px text; the current month column is highlighted with gold background (#FFF0D0) and gold text (#C07700)
- [ ] Row headers display status category names in uppercase 11px bold text with category-specific background colors
- [ ] Each data cell contains a bulleted list of work item names in 12px text with a colored dot indicator (6px circle) matching the row's status color
- [ ] Empty cells for future months display a gray dash "—"
- [ ] The current month column cells have a slightly darker tinted background per category
- [ ] All heatmap data is driven from `data.json` → `heatmap`
- [ ] The number of month columns is dynamic based on the `columns` array in `data.json`

### US-4: Configure Dashboard via JSON

**As a** project lead, **I want** to edit a single `data.json` file to update all dashboard content — title, milestones, work items — **so that** I can prepare my executive report without touching any code.

**Acceptance Criteria:**
- [ ] A `data.json` file in the project root controls all displayed content
- [ ] The JSON schema supports: `title`, `subtitle`, `backlogLink`, `timeline` (with tracks and events), and `heatmap` (with columns and four status categories)
- [ ] Changing a value in `data.json` and restarting the app (`dotnet run`) reflects the change in the browser
- [ ] Invalid JSON produces a clear error message (not a blank page or unhandled exception)
- [ ] A sample `data.json` is provided with realistic fictional project data ("Project Aurora")

### US-5: Capture Screenshot-Ready Output

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, browser chrome artifacts, or framework UI overlays, **so that** I can take a clean screenshot for my PowerPoint deck.

**Acceptance Criteria:**
- [ ] The page renders at a fixed 1920×1080 viewport with `overflow: hidden`
- [ ] No Blazor reconnection modal or error UI is visible
- [ ] No scrollbars appear when all content fits within the viewport
- [ ] The rendered output matches the reference design (`OriginalDesignConcept.html`) in a side-by-side comparison
- [ ] The page loads fully in under 2 seconds on localhost

### US-6: Run Dashboard Locally with Zero Configuration

**As a** developer or project lead, **I want** to run the dashboard with a single `dotnet run` command and no additional setup, **so that** I can get started immediately without configuring databases, services, or authentication.

**Acceptance Criteria:**
- [ ] The app starts with `dotnet run` from the project directory
- [ ] The app binds to `http://localhost:5000` (or configured port) by default
- [ ] No database, authentication, or external service is required
- [ ] The only prerequisite is the .NET 8 SDK installed on the machine
- [ ] A README file documents the 3-step workflow: edit `data.json` → `dotnet run` → screenshot

---

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` in the `ReportingDashboard` repository. See also the rendered screenshot: `docs/design-screenshots/OriginalDesignConcept.png`. Engineers MUST match these visuals exactly.

### Overall Layout

The page is a single full-screen view at **1920×1080 pixels** with no scrolling. The layout uses a vertical flex column (`display: flex; flex-direction: column`) with three stacked sections:

```
┌─────────────────────────────────────────────────────┐
│  HEADER (flex-shrink: 0, ~50px)                     │
├─────────────────────────────────────────────────────┤
│  TIMELINE AREA (flex-shrink: 0, height: 196px)      │
│  ┌──────────┬──────────────────────────────────┐    │
│  │ Track    │  SVG Timeline (flex: 1)          │    │
│  │ Labels   │                                  │    │
│  │ (230px)  │                                  │    │
│  └──────────┴──────────────────────────────────┘    │
├─────────────────────────────────────────────────────┤
│  HEATMAP AREA (flex: 1, fills remaining space)      │
│  ┌──────┬───────┬───────┬───────┬───────┐          │
│  │Status│ Month │ Month │ Month │ Month │          │
│  │ Hdr  │  1    │  2    │  3    │  4    │          │
│  ├──────┼───────┼───────┼───────┼───────┤          │
│  │Ship  │       │       │       │       │          │
│  ├──────┼───────┼───────┼───────┼───────┤          │
│  │Prog  │       │       │       │       │          │
│  ├──────┼───────┼───────┼───────┼───────┤          │
│  │Carry │       │       │       │       │          │
│  ├──────┼───────┼───────┼───────┼───────┤          │
│  │Block │       │       │       │       │          │
│  └──────┴───────┴───────┴───────┴───────┘          │
└─────────────────────────────────────────────────────┘
```

### Section 1: Header Bar (`.hdr`)

- **Layout:** Flexbox row, `align-items: center`, `justify-content: space-between`
- **Padding:** `12px 44px 10px`
- **Border:** 1px solid #E0E0E0 bottom
- **Left side:**
  - **Title:** `<h1>` at 24px, font-weight 700, color #111. Contains an inline `<a>` link styled in #0078D4 with no underline for the ADO backlog link
  - **Subtitle:** 12px, color #888, margin-top 2px. Shows org hierarchy and current month
- **Right side — Legend row:** Flexbox with 22px gap, 12px font size. Four legend items:
  - **PoC Milestone:** 12×12px gold (#F4B400) square rotated 45° (diamond shape) + label text
  - **Production Release:** 12×12px green (#34A853) square rotated 45° (diamond shape) + label text
  - **Checkpoint:** 8×8px gray (#999) circle + label text
  - **Now indicator:** 2×14px red (#EA4335) vertical bar + label "Now (Apr 2026)"

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox row, `align-items: stretch`
- **Dimensions:** Height 196px, flex-shrink 0
- **Background:** #FAFAFA
- **Border:** 2px solid #E8E8E8 bottom
- **Padding:** `6px 44px 0`

#### Track Label Sidebar (left)
- **Width:** 230px, flex-shrink 0
- **Border-right:** 1px solid #E0E0E0
- **Padding:** `16px 12px 16px 0`
- **Layout:** Flex column, `justify-content: space-around`
- **Each track label:**
  - Track ID (e.g., "M1") in 12px font-weight 600, colored per track (e.g., #0078D4, #00897B, #546E7A)
  - Track name on next line in font-weight 400, color #444

#### SVG Timeline (right, `.tl-svg-box`)
- **Dimensions:** SVG at 1560×185px, `overflow: visible`
- **SVG `<defs>`:** Drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`)
- **Month grid lines:** Vertical lines at each month boundary, stroke #BBB opacity 0.4, width 1. Month labels at top in 11px font-weight 600, color #666
- **"NOW" line:** Dashed vertical line (stroke #EA4335, width 2, dasharray "5,3") at current date x-position. "NOW" label in 10px bold #EA4335
- **Milestone tracks:** Horizontal lines spanning full width, stroke-width 3, colored per track
  - **Track Y positions:** Evenly spaced (approximately y=42, y=98, y=154 for 3 tracks)
- **Milestone event shapes:**
  - **Checkpoint (start):** Circle, r=7, white fill, track-color stroke width 2.5
  - **Checkpoint (minor):** Circle, r=4–5, filled #999 or white fill with #888 stroke
  - **PoC:** Diamond polygon (11px radius), filled #F4B400, with drop shadow filter
  - **Production:** Diamond polygon (11px radius), filled #34A853, with drop shadow filter
- **Date labels:** 10px, color #666, `text-anchor: middle`, positioned above or below markers

### Section 3: Heatmap Area (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1`, `min-height: 0`
- **Padding:** `10px 44px 10px`

#### Section Title (`.hm-title`)
- **Text:** "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"
- **Style:** 14px, font-weight 700, color #888, uppercase, letter-spacing 0.5px, margin-bottom 8px

#### Grid (`.hm-grid`)
- **Layout:** CSS Grid
- **Columns:** `160px repeat(N, 1fr)` where N = number of month columns (default 4)
- **Rows:** `36px repeat(4, 1fr)`
- **Border:** 1px solid #E0E0E0

##### Header Row
- **Corner cell (`.hm-corner`):** "STATUS" label, 11px bold uppercase #999, background #F5F5F5, border-bottom 2px solid #CCC
- **Month header cells (`.hm-col-hdr`):** Month name in 16px bold, background #F5F5F5, border-bottom 2px solid #CCC
- **Current month header (`.apr-hdr`):** Background #FFF0D0, text color #C07700

##### Status Category Rows

Each row has a consistent color scheme applied to the row header and all data cells:

| Category | Row Header | Header BG | Header Text | Cell BG | Current Month Cell BG | Bullet Color |
|----------|-----------|-----------|-------------|---------|----------------------|-------------|
| **Shipped** | "✅ Shipped" | #E8F5E9 | #1B7A28 | #F0FBF0 | #D8F2DA | #34A853 |
| **In Progress** | "🔄 In Progress" | #E3F2FD | #1565C0 | #EEF4FE | #DAE8FB | #0078D4 |
| **Carryover** | "⏳ Carryover" | #FFF8E1 | #B45309 | #FFFDE7 | #FFF0B0 | #F4B400 |
| **Blockers** | "🚫 Blockers" | #FEF2F2 | #991B1B | #FFF5F5 | #FFE4E4 | #EA4335 |

##### Row Headers (`.hm-row-hdr`)
- 11px bold uppercase, letter-spacing 0.7px
- Border-right: 2px solid #CCC
- Border-bottom: 1px solid #E0E0E0

##### Data Cells (`.hm-cell`)
- Padding: `8px 12px`
- Border-right: 1px solid #E0E0E0, border-bottom: 1px solid #E0E0E0
- Each work item (`.it`): 12px, color #333, padding-left 12px, line-height 1.35
- Colored bullet: 6×6px circle (CSS `::before` pseudo-element), positioned at left:0, top:7px
- Empty cells show a gray dash "—" in color #AAA

### Typography

- **Font family:** `'Segoe UI', Arial, sans-serif` (system font, no web font loading)
- **Title:** 24px, weight 700
- **Month headers:** 16px, weight 700
- **Section title:** 14px, weight 700, uppercase
- **Body/items:** 12px, weight 400
- **Labels/headers:** 11px, weight 700, uppercase
- **SVG labels:** 10–11px, weight 400–700

### Color System (CSS Custom Properties)

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-highlight: #D8F2DA;
    --color-shipped-header-bg: #E8F5E9;
    --color-shipped-header-text: #1B7A28;

    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-highlight: #DAE8FB;
    --color-progress-header-bg: #E3F2FD;
    --color-progress-header-text: #1565C0;

    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-highlight: #FFF0B0;
    --color-carryover-header-bg: #FFF8E1;
    --color-carryover-header-text: #B45309;

    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-highlight: #FFE4E4;
    --color-blockers-header-bg: #FEF2F2;
    --color-blockers-header-text: #991B1B;

    --color-link: #0078D4;
    --color-now-line: #EA4335;
    --color-poc-diamond: #F4B400;
    --color-prod-diamond: #34A853;
    --color-checkpoint: #999;
    --color-highlight-month-bg: #FFF0D0;
    --color-highlight-month-text: #C07700;
}
```

### Component Hierarchy

```
App.razor
└── MainLayout.razor (minimal, no nav chrome)
    └── Dashboard.razor (single page at route "/")
        ├── Header.razor (title, subtitle, legend)
        ├── Timeline.razor (track labels + SVG)
        └── HeatmapGrid.razor (section title + CSS Grid)
            ├── HeatmapRowHeader.razor × 4
            └── HeatmapCell.razor × (4 categories × N months)
```

---

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The dashboard loads and renders all three sections (header, timeline, heatmap) simultaneously from cached `data.json` data. The page displays at 1920×1080 with no scrollbars. The "NOW" line on the timeline is positioned at today's date (or the `currentDate` override from JSON). The current month column in the heatmap is highlighted with a gold header. Total load time is under 2 seconds.

**Scenario 2: User Views Header and Identifies Project Context**
User sees the project title "Project Aurora Release Roadmap" with a blue "→ ADO Backlog" hyperlink. Below it, the subtitle reads "AI Platform · Customer Analytics Workstream · April 2026". On the right, the legend shows four visual indicators (PoC diamond, Production diamond, Checkpoint circle, Now line) so the user can decode the timeline symbols.

**Scenario 3: User Reads the Milestone Timeline**
User scans left to right across the timeline. Three horizontal colored lines represent milestone tracks (M1: ML Pipeline in blue, M2: Data Ingestion in teal, M3: Dashboard UI in gray-blue). Diamond markers show upcoming PoC and Production milestones. Circle markers show completed checkpoints. A dashed red "NOW" vertical line bisects the timeline at the current date, giving an instant sense of progress vs. plan.

**Scenario 4: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. A new browser tab opens to the Azure DevOps backlog URL specified in `data.json`. The dashboard page remains unchanged.

**Scenario 5: User Scans the Heatmap for Execution Health**
User looks at the heatmap grid. Green "Shipped" row shows completed deliverables per month with green bullet indicators. Blue "In Progress" row shows active work. Amber "Carryover" row highlights items that slipped from prior months. Red "Blockers" row shows impediments. The current month (April) column is visually emphasized with a gold-tinted header and slightly darker cell backgrounds per category.

**Scenario 6: User Identifies a Blocker**
User notices a red-highlighted item "Vendor contract delay" in the April Blockers cell. The red bullet and red-tinted cell background draw immediate attention. The user notes this for discussion in the executive meeting.

**Scenario 7: User Identifies Carryover Items**
User sees "SSO integration" appearing in both the March and April Carryover cells, indicating an item that has slipped for two consecutive months. The amber color and repeated appearance signal a pattern requiring attention.

**Scenario 8: User Takes a Screenshot for PowerPoint**
User opens Chrome DevTools, sets device emulation to 1920×1080, and captures a full-page screenshot. The resulting image has no scrollbars, no Blazor reconnection overlay, no browser chrome artifacts, and is ready to paste directly into a PowerPoint slide.

**Scenario 9: Data-Driven Rendering with Varying Track Counts**
Project lead configures `data.json` with 2 milestone tracks instead of 3. The timeline automatically adjusts Y-spacing so the 2 tracks are vertically centered within the 196px timeline area. No empty track lines or orphaned labels appear.

**Scenario 10: Data-Driven Rendering with Varying Month Columns**
Project lead changes the heatmap `columns` array from `["Jan","Feb","Mar","Apr"]` to `["Feb","Mar","Apr","May","Jun"]`. The grid renders 5 month columns instead of 4. Column widths adjust proportionally. The `highlightColumn` value determines which column gets the gold highlight treatment.

**Scenario 11: Empty State — No Items in a Heatmap Cell**
A heatmap cell for a month with no items in a category (e.g., no blockers in January) displays a single gray dash "—" in #AAA color, indicating "nothing to report" rather than leaving the cell blank.

**Scenario 12: Error State — Invalid or Missing data.json**
User starts the app without a `data.json` file or with malformed JSON. The page displays a clear, styled error message (e.g., "Unable to load dashboard data. Please check that data.json exists and contains valid JSON.") rather than a blank page or stack trace.

**Scenario 13: Error State — Missing Required Fields in data.json**
User provides a `data.json` missing the `timeline` object. The app logs a descriptive error and renders a meaningful message indicating which required field is missing, rather than crashing with a NullReferenceException.

**Scenario 14: Hover State — Milestone Diamond (Enhancement)**
User hovers over a PoC or Production milestone diamond on the timeline. The cursor changes to a pointer. A browser-native `<title>` tooltip appears showing the milestone label and date (e.g., "Mar 20 PoC — March 20, 2026"). No JavaScript tooltip library is used.

**Scenario 15: Hover State — Backlog Link**
User hovers over the "→ ADO Backlog" link. The link shows the standard browser underline-on-hover behavior per the `a` tag styling (color #0078D4, no text-decoration by default).

---

## Scope

### In Scope

- Single-page Blazor Server (.NET 8) web application
- Fixed 1920×1080 viewport layout optimized for screenshots
- Header component with project title, subtitle, ADO backlog link, and milestone legend
- SVG-based horizontal milestone timeline with up to 5 configurable tracks
- Programmatic date-to-pixel positioning for all timeline elements
- Three milestone event types: Checkpoint (circle), PoC (gold diamond), Production (green diamond)
- Dashed red "NOW" vertical indicator line on the timeline
- CSS Grid heatmap with 4 status rows (Shipped, In Progress, Carryover, Blockers)
- Data-driven month columns (configurable count via `data.json`)
- Current-month highlight styling in the heatmap
- All content driven from a single `data.json` flat file
- Strongly-typed C# record model classes for JSON deserialization
- `DashboardDataService` singleton for reading and caching data
- Sample `data.json` with fictional "Project Aurora" project data
- CSS custom properties for easy color theming
- Global `dashboard.css` ported from the reference HTML design
- Blazor reconnection modal hidden via CSS
- Basic error handling for missing or malformed `data.json`
- README with usage instructions (edit JSON → run → screenshot)
- Unit tests for data service and model deserialization (xUnit + bUnit)

### Out of Scope

- **Authentication / authorization** — No login, no roles, no identity provider integration
- **Database** — No SQL, NoSQL, or any persistent data store beyond the JSON file
- **API endpoints** — No REST/GraphQL APIs; the app only serves the dashboard page
- **JavaScript interop** — No JS libraries, charting frameworks, or client-side scripting
- **Third-party UI component libraries** — No MudBlazor, Radzen, Syncfusion, or Blazorise
- **Docker containerization** — Local-only tool, no container image
- **CI/CD pipeline** — No automated build/deploy; manual `dotnet run` workflow
- **Real-time updates / hot reload of data** — Requires app restart after JSON changes
- **Export / print functionality** — No PDF export, no "Copy to clipboard" button
- **Responsive / mobile layout** — Fixed 1920×1080 only; no breakpoints for smaller screens
- **Multi-project support** — One `data.json` = one project. No project switching or routing
- **Dark mode / theme switching** — Single light theme matching the reference design
- **Localization / internationalization** — English only
- **Accessibility compliance** — Best-effort semantic HTML, but no formal WCAG audit
- **Telemetry / analytics** — No usage tracking or application insights

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** (localhost, cold start) | < 3 seconds |
| **Page load time** (localhost, warm) | < 1 second |
| **Time to interactive** | < 2 seconds (static render, no JS interaction needed) |
| **JSON deserialization** | < 50ms for files up to 100KB |
| **Memory footprint** | < 100MB working set |

### Reliability

| Metric | Target |
|--------|--------|
| **Uptime** | N/A — local tool, runs on-demand |
| **Error handling** | Graceful degradation with user-visible error messages for missing/invalid data.json |
| **Data integrity** | Read-only access to data.json; no risk of data corruption |

### Security

| Requirement | Implementation |
|-------------|---------------|
| **Network binding** | localhost only (Kestrel development mode default) |
| **Authentication** | None |
| **Data sensitivity** | Non-confidential project metadata only |
| **Secrets management** | No secrets; no API keys, tokens, or credentials |
| **HTTPS** | Not required for localhost-only usage |

### Scalability

- **Not applicable.** This is a single-user, single-machine local tool. There is no need to scale horizontally or vertically.
- **Data volume:** Designed for `data.json` files with up to 5 timeline tracks and up to 12 heatmap month columns. Beyond that, the visual design becomes cramped and would require layout adjustments.

### Compatibility

| Environment | Requirement |
|------------|-------------|
| **Runtime** | .NET 8.0 SDK |
| **OS** | Windows 10/11 (primary), macOS/Linux (should work but not tested) |
| **Browser** | Chrome 120+ or Edge 120+ (screenshot target) |
| **Resolution** | 1920×1080 (optimized), other resolutions may show scrollbars |

### Maintainability

- Single global CSS file (no CSS-in-JS, no preprocessors)
- Strongly-typed C# models prevent runtime deserialization errors
- Zero third-party NuGet dependencies in the main project
- Standard .NET 8 Blazor project structure

---

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|-------------------|
| 1 | **Visual fidelity** | Dashboard output matches `OriginalDesignConcept.html` reference at 95%+ visual similarity | Side-by-side screenshot comparison at 1920×1080 |
| 2 | **Data-driven rendering** | 100% of displayed content sourced from `data.json` (no hardcoded values) | Code review: grep for hardcoded strings in Razor components |
| 3 | **Setup time** | New user goes from clone to running dashboard in < 5 minutes | Timed walkthrough with README instructions |
| 4 | **JSON edit-to-screenshot cycle** | < 30 seconds (edit JSON → restart → screenshot) | Timed workflow test |
| 5 | **Screenshot quality** | Screenshot requires zero manual editing before pasting into PowerPoint | User acceptance: project lead confirms paste-ready output |
| 6 | **Zero runtime errors** | App loads and renders with sample data.json without exceptions | `dotnet run` + browser load with no console errors |
| 7 | **Test coverage** | Data service and model deserialization have unit tests passing | `dotnet test` returns 0 exit code |
| 8 | **Build success** | `dotnet build` completes with zero warnings and zero errors | CI-equivalent: `dotnet build --warnaserrors` |

---

## Constraints & Assumptions

### Technical Constraints

1. **Technology stack is mandated:** The application must be built with C# .NET 8 Blazor Server. No alternative frameworks (React, Angular, plain HTML+JS) are acceptable.
2. **No third-party NuGet packages** for the main project. All functionality must use built-in .NET 8 libraries (Blazor, System.Text.Json, Kestrel, System.IO).
3. **Fixed viewport:** The design targets exactly 1920×1080 pixels. Responsive behavior for other resolutions is explicitly out of scope.
4. **Segoe UI font dependency:** The design relies on the Segoe UI system font, which is available on Windows. On macOS/Linux, the fallback is Arial.
5. **Local-only execution:** The app must run on `localhost` without any cloud, network, or external service dependencies.
6. **Static SSR preferred:** To avoid Blazor SignalR reconnection UI artifacts in screenshots, Static Server-Side Rendering should be used where possible.

### Timeline Assumptions

1. **Implementation effort:** 1–2 developer-days for an experienced .NET/Blazor developer.
2. **Phase 1 (static port):** 0.5 days — Get reference HTML rendering pixel-perfect in Blazor with hardcoded data.
3. **Phase 2 (data binding):** 0.5 days — Replace hardcoded values with `data.json` deserialization and component extraction.
4. **Phase 3 (dynamic timeline):** 0.5 days — Implement date-to-pixel calculations and dynamic SVG rendering.
5. **Phase 4 (polish):** 0.5 days — Visual fine-tuning, error handling, README, and screenshot validation.

### Dependency Assumptions

1. **.NET 8 SDK** is installed on the developer's machine (version 8.0.x, latest patch).
2. **Chrome or Edge** browser is available for viewing and screenshotting the dashboard.
3. **The reference design file** (`OriginalDesignConcept.html`) in the `ReportingDashboard` repository is the authoritative visual specification. Any differences between this file and `C:/Pics/ReportingDashboardDesign.png` should be cataloged and resolved before implementation begins.
4. **The `data.json` schema** defined in this specification (see Appendix B of the research document) is the canonical format. Schema changes require a spec update.
5. **No concurrent users.** The app is accessed by a single user on their local machine. No concurrency, session management, or multi-user considerations apply.
6. **Project leads are comfortable editing JSON.** The target user can open `data.json` in a text editor, modify values, and save the file without developer assistance.

### Open Questions Requiring Resolution

| # | Question | Recommended Default | Decision Owner |
|---|----------|-------------------|----------------|
| 1 | Should the "Now" line use system date or JSON override? | Default to `DateTime.Now`; allow override via `currentDate` in JSON | PM |
| 2 | Maximum number of heatmap month columns? | 6 (beyond this, column widths become too narrow at 1920px) | PM |
| 3 | Maximum number of timeline tracks? | 5 (beyond this, the 196px area is too cramped) | PM |
| 4 | What are the differences between `OriginalDesignConcept.html` and `C:/Pics/ReportingDashboardDesign.png`? | Review both files before implementation; `OriginalDesignConcept.html` is primary | PM + Designer |