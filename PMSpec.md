# PM Specification: Executive Reporting Dashboard

**Document Version:** 1.0
**Date:** April 14, 2026
**Author:** Program Management
**Status:** Draft for Review

---

## Executive Summary

We are building a single-page, screenshot-optimized executive reporting dashboard that visualizes a project's milestone timeline and monthly execution status (Shipped, In Progress, Carryover, Blockers) driven entirely by a local `data.json` configuration file. The dashboard replicates and improves upon the existing `OriginalDesignConcept.html` design reference, rendering at a fixed 1920×1080 resolution so that screenshots can be directly embedded into PowerPoint decks for executive stakeholders. The solution is intentionally minimal — a single Blazor Server page with no authentication, no database, and no external dependencies — optimized for rapid iteration and visual fidelity.

---

## Business Goals

1. **Provide executive-grade project visibility** — Deliver a single-page view that communicates project health, milestone progress, and work item status at a glance, suitable for director/VP-level audiences.
2. **Enable rapid deck preparation** — Allow the user to update a JSON file, refresh the browser, and screenshot a pixel-perfect dashboard in under 60 seconds, eliminating manual PowerPoint chart creation.
3. **Maintain zero operational overhead** — The tool runs locally on the developer's workstation with no cloud hosting, no licensing fees, no authentication, and no infrastructure to maintain.
4. **Support multiple project snapshots** — Enable the user to maintain multiple `data.json` files (e.g., per-sprint, per-quarter, per-audience) and switch between them via a query parameter, supporting different executive presentations from a single tool.
5. **Achieve pixel-perfect design fidelity** — Match the visual design established in `OriginalDesignConcept.html` while improving readability, with the output indistinguishable from a professionally designed slide.

---

## User Stories & Acceptance Criteria

### US-1: View Project Dashboard at a Glance

**As a** program manager, **I want** to open a single browser page and immediately see my project's title, milestone timeline, and monthly execution heatmap, **so that** I can assess project health in under 5 seconds.

**Visual Reference:** Header section + Timeline area + Heatmap grid from `OriginalDesignConcept.html`

**Acceptance Criteria:**
- [ ] Navigating to `https://localhost:<port>/` renders the full dashboard within 2 seconds
- [ ] The page displays at exactly 1920×1080 pixels with no scrollbars
- [ ] The header shows the project title, subtitle (team/workstream/date), and a hyperlink to the ADO backlog
- [ ] The milestone timeline section appears below the header with labeled tracks and SVG event markers
- [ ] The heatmap grid appears below the timeline with color-coded rows for Shipped, In Progress, Carryover, and Blockers
- [ ] All data is sourced from `wwwroot/data.json` — no hardcoded values in the page

### US-2: Configure Dashboard via JSON

**As a** program manager, **I want** to edit a `data.json` file to define my project's title, milestones, timeline range, and monthly work items, **so that** I can update the dashboard without touching any code.

**Acceptance Criteria:**
- [ ] A `data.json` file in `wwwroot/` defines all dashboard content: title, subtitle, backlog URL, current date, months, milestone tracks, and heatmap rows
- [ ] Changing `data.json` and restarting the app (or using hot reload) reflects the updated data on the dashboard
- [ ] The JSON schema supports variable numbers of milestone tracks (1–5), variable months in the heatmap (2–6), and variable work items per cell (0–10)
- [ ] Missing or empty fields in `data.json` render gracefully (empty cells, no crash)
- [ ] Malformed JSON displays a user-friendly error banner instead of a stack trace

### US-3: View Milestone Timeline with Event Markers

**As an** executive viewer, **I want** to see a horizontal timeline with milestone tracks, checkpoint circles, PoC diamonds, production release diamonds, and a "NOW" indicator, **so that** I can understand where each workstream stands relative to the current date.

**Visual Reference:** Timeline/SVG area from `OriginalDesignConcept.html` — tracks at Y=42, Y=98, Y=154; diamonds (11px rotated squares) with drop shadows; dashed red "NOW" line

**Acceptance Criteria:**
- [ ] Each milestone track renders as a colored horizontal line spanning the full SVG width (1560px)
- [ ] Checkpoint events render as open circles (white fill, colored stroke) on their track line
- [ ] PoC milestone events render as gold diamonds (`#F4B400`) with a drop shadow filter
- [ ] Production release events render as green diamonds (`#34A853`) with a drop shadow filter
- [ ] A dashed red vertical line (`#EA4335`, stroke-dasharray `5,3`) marks the current date ("NOW")
- [ ] "NOW" text label appears at the top of the dashed line in red bold font
- [ ] Each event has a date label positioned above or below the track line (alternating to avoid overlap)
- [ ] A left sidebar (230px wide) lists milestone track names (e.g., "M1", "M2") with descriptions
- [ ] Month grid lines appear at evenly spaced intervals with month abbreviation labels (Jan–Jun)
- [ ] Date-to-X position mapping uses linear interpolation across the configured timeline range

### US-4: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked — organized by month, **so that** I can quickly assess execution velocity and identify risks.

**Visual Reference:** Heatmap grid section from `OriginalDesignConcept.html` — CSS Grid with `160px repeat(N, 1fr)` columns

**Acceptance Criteria:**
- [ ] The heatmap renders as a CSS Grid with a "Status" label column (160px) and one column per month
- [ ] Column headers display month names; the current month column is highlighted with a gold background (`#FFF0D0`) and amber text (`#C07700`)
- [ ] Four status rows render in order: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Row headers display the status label in uppercase with an emoji prefix (✅, 🔵, 🟡, 🔴) and a colored background matching the row theme
- [ ] Each data cell lists work items as bullet-pointed text entries with a small colored dot (6px circle) as the bullet
- [ ] Current month cells have a slightly darker background variant than non-current month cells
- [ ] Empty cells display a dash ("—") in muted gray text
- [ ] The heatmap fills the remaining vertical space below the timeline (flex: 1)

### US-5: Take Screenshots for PowerPoint

**As a** program manager, **I want** the dashboard to render at exactly 1920×1080 with no browser chrome artifacts, **so that** I can take a clean screenshot and paste it directly into a PowerPoint slide.

**Acceptance Criteria:**
- [ ] The `<body>` element has `width: 1920px; height: 1080px; overflow: hidden`
- [ ] No scrollbars, tooltips, or interactive UI elements appear on the static page
- [ ] Font rendering uses Segoe UI (system font on Windows) — no web font loading delays
- [ ] All SVG elements render without anti-aliasing artifacts at 1920×1080
- [ ] A `@media print` CSS rule hides browser chrome for Ctrl+P → Save as PDF workflow
- [ ] On screens smaller than 1920px, a CSS `transform: scale()` auto-fits the dashboard to the viewport width

### US-6: Switch Between Data Files

**As a** program manager, **I want** to load different JSON data files via a URL query parameter (e.g., `?data=q3-review.json`), **so that** I can maintain multiple dashboard versions for different meetings without renaming files.

**Acceptance Criteria:**
- [ ] Navigating to `/?data=q3-review.json` loads `wwwroot/q3-review.json` instead of the default `data.json`
- [ ] If the query parameter is omitted, `data.json` is loaded by default
- [ ] If the specified file does not exist, a friendly error banner is displayed
- [ ] Path traversal attacks are prevented (only files in `wwwroot/` are accessible)

### US-7: View Legend for Timeline Markers

**As an** executive viewer, **I want** to see a legend in the header that explains what each shape and color on the timeline means, **so that** I can interpret the dashboard without a verbal walkthrough.

**Visual Reference:** Legend area in the header-right of `OriginalDesignConcept.html`

**Acceptance Criteria:**
- [ ] The header displays four legend items aligned right: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), Now indicator (red vertical line)
- [ ] Legend icons match the exact shapes used in the timeline SVG
- [ ] Legend text is 12px Segoe UI, consistent with the design reference

---

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the `ReportingDashboard` repository
**Rendered Screenshot:** `docs/design-screenshots/OriginalDesignConcept.png`

Engineers MUST consult both the HTML source and the rendered screenshot. The implementation must be pixel-comparable to the screenshot at 1920×1080.

### Overall Page Layout

- **Canvas:** Fixed `1920px × 1080px`, white background (`#FFFFFF`), `overflow: hidden`
- **Layout Direction:** Vertical flex column (`display: flex; flex-direction: column`)
- **Font Stack:** `'Segoe UI', Arial, sans-serif` — system font, no web font loading
- **Base Text Color:** `#111`
- **Link Color:** `#0078D4` (no underline)

The page is divided into three vertical sections stacked top-to-bottom:

```
┌─────────────────────────────────────────────────────┐
│  HEADER (flex-shrink: 0, ~48px)                     │
├─────────────────────────────────────────────────────┤
│  TIMELINE AREA (flex-shrink: 0, height: 196px)      │
│  ┌──────────┬──────────────────────────────────────┐│
│  │ Sidebar  │  SVG Timeline (flex: 1)              ││
│  │ (230px)  │                                      ││
│  └──────────┴──────────────────────────────────────┘│
├─────────────────────────────────────────────────────┤
│  HEATMAP (flex: 1, fills remaining space)           │
│  ┌───────┬────────┬────────┬────────┬────────┐     │
│  │Status │ Month1 │ Month2 │ Month3 │ Month4 │     │
│  ├───────┼────────┼────────┼────────┼────────┤     │
│  │Shipped│  ...   │  ...   │  ...   │  ...   │     │
│  ├───────┼────────┼────────┼────────┼────────┤     │
│  │In Prog│  ...   │  ...   │  ...   │  ...   │     │
│  ├───────┼────────┼────────┼────────┼────────┤     │
│  │Carry  │  ...   │  ...   │  ...   │  ...   │     │
│  ├───────┼────────┼────────┼────────┼────────┤     │
│  │Block  │  ...   │  ...   │  ...   │  ...   │     │
│  └───────┴────────┴────────┴────────┴────────┘     │
└─────────────────────────────────────────────────────┘
```

### Section 1: Header Bar (`.hdr`)

- **Layout:** Flexbox row, `align-items: center`, `justify-content: space-between`
- **Padding:** `12px 44px 10px`
- **Border:** Bottom `1px solid #E0E0E0`
- **Left side:**
  - **Title (`h1`):** 24px, font-weight 700, e.g., "Privacy Automation Release Roadmap" followed by a linked "→ ADO Backlog" in `#0078D4`
  - **Subtitle (`.sub`):** 12px, color `#888`, margin-top 2px, e.g., "Trusted Platform · Privacy Automation Workstream · April 2026"
- **Right side — Legend:**
  - Horizontal flex row with `gap: 22px`
  - Four legend items, each as a flex row with `gap: 6px`, `font-size: 12px`:
    1. Gold diamond (12×12px `<span>`, `background: #F4B400`, `transform: rotate(45deg)`) + "PoC Milestone"
    2. Green diamond (12×12px, `background: #34A853`, `transform: rotate(45deg)`) + "Production Release"
    3. Gray circle (8×8px, `border-radius: 50%`, `background: #999`) + "Checkpoint"
    4. Red line (2×14px, `background: #EA4335`) + "Now (Apr 2026)"

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox row, `align-items: stretch`
- **Padding:** `6px 44px 0`
- **Height:** Fixed `196px`
- **Background:** `#FAFAFA`
- **Border:** Bottom `2px solid #E8E8E8`

#### Timeline Sidebar (left column)
- **Width:** `230px`, flex-shrink 0
- **Layout:** Flex column, `justify-content: space-around`
- **Padding:** `16px 12px 16px 0`
- **Border:** Right `1px solid #E0E0E0`
- **Content:** One entry per milestone track:
  - Track name (e.g., "M1") in bold 12px, track-specific color (e.g., `#0078D4`)
  - Description (e.g., "Chatbot & MS Role") in regular 12px, color `#444`
  - Track colors from design: M1 = `#0078D4`, M2 = `#00897B`, M3 = `#546E7A`

#### Timeline SVG (right column, `.tl-svg-box`)
- **Layout:** `flex: 1`, `padding-left: 12px`, `padding-top: 6px`
- **SVG Dimensions:** `width="1560" height="185"`, `overflow: visible`
- **SVG Filter Definition:** Drop shadow filter `id="sh"` — `feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"`

**Month Grid Lines:**
- Vertical lines at evenly spaced X positions (0, 260, 520, 780, 1040, 1300 for Jan–Jun)
- Line style: `stroke="#bbb"`, `stroke-opacity="0.4"`, `stroke-width="1"`
- Month labels: Positioned at X+5, Y=14, `fill="#666"`, `font-size="11"`, `font-weight="600"`

**"NOW" Indicator:**
- Dashed vertical red line at the X position corresponding to the current date
- `stroke="#EA4335"`, `stroke-width="2"`, `stroke-dasharray="5,3"`
- "NOW" text label: `fill="#EA4335"`, `font-size="10"`, `font-weight="700"`, positioned at top of line

**Milestone Track Lines:**
- Horizontal lines spanning full SVG width (x1=0, x2=1560)
- Y positions evenly distributed (Y=42, Y=98, Y=154 for 3 tracks)
- `stroke-width="3"`, stroke color per track (e.g., `#0078D4`, `#00897B`, `#546E7A`)

**Event Markers on Tracks:**
- **Checkpoint:** Open circle — `r="5-7"`, `fill="white"`, `stroke="{trackColor}"`, `stroke-width="2.5"`
- **PoC Milestone:** Diamond (rotated square polygon) — `fill="#F4B400"`, `filter="url(#sh)"`, points offset ±11px from center
- **Production Release:** Diamond — `fill="#34A853"`, `filter="url(#sh)"`, same geometry as PoC
- **Small checkpoint dots:** `r="4"`, `fill="#999"` (solid, no stroke)
- **Date Labels:** `text-anchor="middle"`, `fill="#666"`, `font-size="10"`, positioned above (Y-16) or below (Y+24) the track line

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1`, `min-height: 0`
- **Padding:** `10px 44px 10px`

#### Heatmap Title (`.hm-title`)
- `font-size: 14px`, `font-weight: 700`, `color: #888`
- `letter-spacing: 0.5px`, `text-transform: uppercase`
- `margin-bottom: 8px`
- Content: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"

#### Grid Structure (`.hm-grid`)
- **CSS Grid:** `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months
- **Grid Rows:** `grid-template-rows: 36px repeat(4, 1fr)`
- **Border:** `1px solid #E0E0E0`
- **Flex:** `flex: 1; min-height: 0` to fill remaining space

#### Header Row
- **Corner Cell (`.hm-corner`):** `background: #F5F5F5`, centered text "STATUS" in 11px bold uppercase `#999`, `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`
- **Month Column Headers (`.hm-col-hdr`):** `background: #F5F5F5`, centered month name in 16px bold, `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`
- **Current Month Highlight (`.apr-hdr`):** `background: #FFF0D0`, `color: #C07700`, appended with " ← Now"

#### Data Rows — Color Scheme

| Row | Header Class | Header BG | Header Text | Cell Class | Cell BG | Highlight BG | Bullet Color |
|-----|-------------|-----------|-------------|------------|---------|--------------|--------------|
| Shipped | `.ship-hdr` | `#E8F5E9` | `#1B7A28` | `.ship-cell` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `.prog-hdr` | `#E3F2FD` | `#1565C0` | `.prog-cell` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `.carry-hdr` | `#FFF8E1` | `#B45309` | `.carry-cell` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `.block-hdr` | `#FEF2F2` | `#991B1B` | `.block-cell` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers (`.hm-row-hdr`)
- `font-size: 11px`, `font-weight: 700`, `text-transform: uppercase`, `letter-spacing: 0.7px`
- `padding: 0 12px`, `border-right: 2px solid #CCC`, `border-bottom: 1px solid #E0E0E0`
- Prefixed with emoji: ✅ Shipped, 🔵 In Progress, 🟡 Carryover, 🔴 Blockers

#### Data Cells (`.hm-cell`)
- `padding: 8px 12px`, `border-right: 1px solid #E0E0E0`, `border-bottom: 1px solid #E0E0E0`, `overflow: hidden`
- Work items rendered as `<div class="it">` elements:
  - `font-size: 12px`, `color: #333`, `padding: 2px 0 2px 12px`, `line-height: 1.35`
  - `::before` pseudo-element: 6×6px colored circle, `position: absolute`, `left: 0`, `top: 7px`
- Current month cells get the highlight background class (e.g., `.ship-cell.apr` → `background: #D8F2DA`)

### CSS Custom Properties (Color Tokens)

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-highlight: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-highlight: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-highlight: #FFF0B0;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-highlight: #FFE4E4;
}
```

---

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Full Dashboard Render**
The user navigates to `https://localhost:5001/`. The Blazor Server app reads `wwwroot/data.json`, deserializes it into the `DashboardData` model, and renders the complete dashboard. The header shows the project title with a clickable ADO backlog link, the legend appears on the right. The timeline SVG draws all milestone tracks with event markers. The heatmap grid fills the remaining space with color-coded status rows. The entire render completes within 2 seconds. The page is static — no loading spinners, no progressive rendering.

**Scenario 2: User Views the Milestone Timeline**
The user looks at the timeline area and sees 3 horizontal colored track lines (M1 in blue, M2 in teal, M3 in gray-blue). Each track has event markers: open circles for checkpoints, gold diamonds for PoC milestones, green diamonds for production releases. A dashed red vertical line labeled "NOW" shows the current date position. The sidebar on the left labels each track (e.g., "M1 — Chatbot & MS Role"). Date labels appear above or below each event marker to avoid visual collision.

**Scenario 3: User Reads the Heatmap Grid**
The user scans the heatmap from left to right. The "Status" column labels each row. The current month column (e.g., "April") is highlighted with a warm gold header (`#FFF0D0`) and "← Now" suffix. The Shipped row (green) shows completed deliverables. The In Progress row (blue) shows active work. The Carryover row (amber) shows items slipping from the previous month. The Blockers row (red) shows impediments. Each cell contains bullet-pointed work item names with colored dots matching the row theme.

**Scenario 4: User Hovers Over a Heatmap Cell (Enhancement)**
When the user hovers over a heatmap data cell, a subtle background color shift (5% darker) provides visual feedback. This is a CSS-only enhancement (`transition: background-color 0.15s`) that does not require JavaScript. The hover effect is optional and does not appear in screenshots.

**Scenario 5: User Clicks the ADO Backlog Link**
The user clicks the "→ ADO Backlog" hyperlink in the header. The browser navigates to the Azure DevOps backlog URL specified in `data.json`. This is the only interactive element on the page.

**Scenario 6: User Switches Data Files via Query Parameter**
The user navigates to `/?data=q4-planning.json`. The app loads `wwwroot/q4-planning.json` instead of the default `data.json`. The dashboard renders with the alternate project data. If the file does not exist, an error banner appears at the top of the page: "Could not load data file: q4-planning.json" on a light red background.

**Scenario 7: Heatmap Cell Has No Items (Empty State)**
A month/status combination with no work items renders a single muted dash ("—") in `color: #AAA` within the cell. The cell retains its colored background. No blank/collapsed cells.

**Scenario 8: data.json Contains Malformed JSON (Error State)**
The user saves a `data.json` with a syntax error (e.g., trailing comma). On page load, the data service catches the `JsonException` and the page renders a centered error banner: "Error loading dashboard data: [error message]. Please check data.json." The banner uses a light red background (`#FEF2F2`) with red text (`#991B1B`). No stack trace is exposed.

**Scenario 9: Dashboard Viewed on a Laptop Screen (Responsive Behavior)**
The user opens the dashboard on a 1366×768 laptop screen. A CSS media query detects `max-width: 1920px` and applies `transform: scale(calc(100vw / 1920)); transform-origin: top left;` to the `<body>`. The entire dashboard scales down proportionally, maintaining visual fidelity. The user can then zoom in or take a scaled screenshot. For pixel-perfect 1920×1080 screenshots, the user should set their browser window to that resolution.

**Scenario 10: User Edits data.json and Hot-Reloads**
The user runs `dotnet watch`, edits `data.json` to add a new work item to the April "In Progress" cell, and saves the file. The Blazor hot-reload detects the change and refreshes the page. The new item appears in the heatmap within 1–2 seconds. No manual browser refresh is needed.

**Scenario 11: User Takes a Screenshot for PowerPoint**
The user opens the dashboard at 1920×1080 (full-screen on a 1080p monitor or using browser DevTools device mode). They use Windows Snipping Tool (Win+Shift+S) to capture the browser viewport. The captured image is a clean, presentation-ready dashboard with no scrollbars, browser chrome, or interactive artifacts. They paste it directly into a PowerPoint slide.

---

## Scope

### In Scope

- Single-page Blazor Server application rendering at 1920×1080
- Header bar with project title, subtitle, ADO backlog link, and timeline legend
- SVG milestone timeline with configurable tracks, checkpoints, PoC diamonds, production diamonds, and "NOW" indicator
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) and configurable month columns
- Color-coded row theming matching the `OriginalDesignConcept.html` design (green/blue/amber/red)
- Current month highlight in the heatmap (gold header, darker cell backgrounds)
- `data.json` configuration file driving all dashboard content
- Fictional example data for a "Project Phoenix" scenario pre-loaded as the default `data.json`
- Query parameter support (`?data=filename.json`) for switching between data files
- CSS auto-scaling for non-1920px viewports via `transform: scale()`
- `@media print` CSS for clean PDF export
- Error handling for malformed or missing `data.json` (friendly error banner)
- Graceful handling of empty heatmap cells (dash placeholder)
- Hot-reload support via `dotnet watch` for rapid iteration
- C# record-based data model (`DashboardData`, `MilestoneTrack`, `MilestoneEvent`, `HeatmapData`, `HeatmapRow`)
- `DashboardDataService` singleton for JSON loading and caching
- Scoped CSS via `Dashboard.razor.css` matching the original design's color palette and layout
- Subtle hover effect on heatmap cells (CSS-only, non-essential)
- Item count badges in row headers (e.g., "SHIPPED (6)")

### Out of Scope

- **Authentication or authorization** — The app runs on localhost for a single user; no login, no RBAC
- **Database or Entity Framework** — Data is a flat JSON file, not a relational store
- **REST API endpoints** — No API layer; the Blazor page reads the file directly
- **Admin UI for editing data** — Users edit `data.json` in any text editor
- **Real-time updates or SignalR push** — Data changes require a page refresh or hot-reload
- **Docker containerization** — Local-only tool; no container orchestration needed
- **CI/CD pipeline** — No automated build/deploy; `dotnet run` is the deployment
- **Automated testing** — Manual visual verification is the primary validation method; optional xUnit smoke test for JSON deserialization only
- **Client-side JavaScript or npm packages** — Pure server-rendered HTML/CSS/SVG
- **Third-party Blazor component libraries** (MudBlazor, Radzen, Syncfusion) — Custom CSS matches the design better
- **Third-party charting libraries** (ApexCharts, ChartJs.Blazor) — The timeline is ~15 SVG elements; a library would be overkill
- **Multi-project views** — Each dashboard instance shows one project; switch via `data.json`
- **Responsive mobile layout** — Fixed 1920×1080 canvas; mobile viewing is not a use case
- **Data generation from ADO/Jira** — The app consumes JSON; data generation is a separate concern
- **Theming UI** — Color tokens are in CSS custom properties; theming is done by editing CSS, not through a UI
- **Internationalization/localization** — English only
- **Accessibility (WCAG compliance)** — The output is for screenshots, not interactive use by screen readers

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Page load time (cold start, localhost) | < 3 seconds |
| Page load time (warm, hot-reload) | < 1 second |
| `data.json` deserialization | < 50ms for files up to 100KB |
| Memory footprint | < 100MB (Blazor Server + Kestrel) |
| SVG render time (browser) | < 200ms for up to 5 tracks × 20 events |

### Security

- **Localhost only:** The app listens on `127.0.0.1` by default. No remote access.
- **No authentication required:** Single-user local tool.
- **No PII stored:** `data.json` contains project names and milestone labels only.
- **Path traversal prevention:** The `?data=` query parameter must resolve to a file within `wwwroot/` only. Reject any path containing `..`, `/`, or `\` beyond the filename.
- **No cookies, sessions, or user tracking.**

### Reliability

- The app must not crash on malformed `data.json` — display an error banner instead.
- The app must handle missing optional fields in `data.json` with sensible defaults (empty arrays, placeholder text).
- `dotnet watch` hot-reload must work reliably for both `.razor` and `.json` file changes.

### Maintainability

- Total codebase should be under 500 lines (models + service + page + CSS).
- Zero external NuGet dependencies beyond the implicit `Microsoft.AspNetCore.App` framework reference.
- `data.json` schema should be self-documenting with clear property names.

### Compatibility

- **Target browser:** Microsoft Edge or Google Chrome on Windows (latest stable).
- **Target resolution:** 1920×1080 (primary), with CSS scaling for other resolutions.
- **Target OS:** Windows 10/11 with .NET 8 SDK installed.
- **Font dependency:** Segoe UI (pre-installed on Windows).

---

## Success Metrics

| # | Metric | Target | How to Measure |
|---|--------|--------|----------------|
| 1 | **Design fidelity** | Dashboard screenshot is visually indistinguishable from `OriginalDesignConcept.png` when overlaid at 50% opacity | Manual overlay comparison in image editor |
| 2 | **Data-to-screenshot time** | User can edit `data.json` and capture a screenshot in < 60 seconds | Timed manual test |
| 3 | **JSON schema completeness** | `data.json` can express all visual elements shown in the design (title, subtitle, N milestone tracks, N months, 4 status rows with M items each) | Schema validation against design reference |
| 4 | **Error resilience** | Malformed `data.json` produces a readable error banner, not a crash or stack trace | Test with 5 malformed JSON variants |
| 5 | **Zero-config startup** | `dotnet run` from the project directory launches the dashboard with sample data, no additional setup | Fresh clone test |
| 6 | **Codebase simplicity** | Total lines of code (excluding generated files) < 500 | `cloc` analysis |
| 7 | **Zero external dependencies** | No NuGet packages beyond the framework reference | Inspect `.csproj` |

---

## Constraints & Assumptions

### Technical Constraints

1. **Technology stack is fixed:** Blazor Server on .NET 8.0. This decision has been made based on the research phase and is not open for debate.
2. **No JavaScript:** All rendering is server-side via Blazor. No client-side JS frameworks, no npm, no bundlers.
3. **No external NuGet packages:** `System.Text.Json` (built-in) is the only serialization library. No MudBlazor, no Radzen, no charting libraries.
4. **Fixed 1920×1080 canvas:** The page is designed as a screenshot canvas, not a responsive web application. The CSS `transform: scale()` fallback is for convenience only.
5. **Single-page architecture:** One `.razor` page at route `/`. No navigation, no routing beyond the root.
6. **Local filesystem dependency:** `data.json` is read from `wwwroot/` via `System.IO`. No HTTP calls, no cloud storage.
7. **Windows-first:** Segoe UI font dependency assumes Windows. On macOS/Linux, the font falls back to Arial (acceptable but not pixel-identical).

### Timeline Assumptions

1. **Implementation effort:** 2–3 days for a single developer, based on the research phase estimate.
2. **Phase 1 (scaffold + static layout):** Day 1 — project setup, CSS porting, hardcoded HTML.
3. **Phase 2 (data model + JSON loading):** Day 1–2 — model definition, service, `data.json` creation.
4. **Phase 3 (dynamic rendering):** Day 2 — Blazor data binding, SVG generation, heatmap grid.
5. **Phase 4 (polish):** Day 2–3 — hover effects, item counts, error handling, auto-scaling, final QA.

### Dependency Assumptions

1. **.NET 8 SDK** is installed on the developer's workstation.
2. **Visual Studio 2022 or VS Code** with C# extension is available for development.
3. The `OriginalDesignConcept.html` file in the `ReportingDashboard` repo is the authoritative design reference and will not change during implementation.
4. The `C:/Pics/ReportingDashboardDesign.png` file provides supplementary visual guidance but the HTML file takes precedence for exact CSS values.
5. The user will manually take screenshots (Windows Snipping Tool or browser DevTools) — no automated screenshot pipeline is needed for v1.
6. The fictional "Project Phoenix" sample data will be created during implementation to demonstrate all dashboard features.

### Data Schema Assumptions

1. The `data.json` schema defined in the research phase (title, subtitle, backlogUrl, currentDate, months, currentMonthIndex, milestoneTracks, heatmap) is accepted as-is.
2. The heatmap supports exactly four status categories: Shipped, In Progress, Carryover, Blockers (matching the design).
3. The timeline supports 1–5 milestone tracks with unlimited events per track.
4. The heatmap supports 2–6 month columns (configurable via the `months` array in `data.json`).
5. Date values in `data.json` use ISO 8601 format (`YYYY-MM-DD`), deserialized as `DateOnly` in C#.

### Open Questions (Deferred to Implementation)

1. Should the timeline date range be explicit in `data.json` or auto-computed from milestone dates? **Default assumption:** Explicit `timelineStart` and `timelineEnd` fields, with fallback to auto-compute if omitted.
2. Should the heatmap month count match the timeline month count? **Default assumption:** They are independent — the timeline can show 6 months while the heatmap shows 4.
3. Will automated screenshot capture (e.g., Playwright) be needed later? **Default assumption:** Not for v1. Can be added as a future enhancement.