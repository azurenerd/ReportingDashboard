# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones on a timeline and displays a monthly execution heatmap of work items categorized as Shipped, In Progress, Carryover, and Blockers. The dashboard reads all data from a local `data.json` configuration file, renders a pixel-perfect 1920×1080 view optimized for screenshot capture into PowerPoint decks, and requires zero authentication, zero cloud dependencies, and zero additional NuGet packages beyond the .NET 8 Blazor Server template.

## Business Goals

1. **Reduce executive reporting preparation time** — Enable project leads to generate polished, consistent project status visuals by editing a single JSON file rather than manually building PowerPoint slides from scratch.
2. **Standardize project visibility** — Provide a repeatable, uniform format for communicating project health (shipped, in-progress, carryover, blockers) across all workstreams.
3. **Maximize screenshot fidelity** — Deliver a fixed 1920×1080 layout that produces clean, presentation-ready screenshots with no browser chrome artifacts, animations, or responsive layout jitter.
4. **Minimize operational overhead** — Zero infrastructure cost, zero authentication, zero database — a single `dotnet run` command on a developer machine produces the dashboard.
5. **Enable rapid data updates** — Allow a project lead to update milestones and heatmap items by editing `data.json` and refreshing the browser, with no rebuild or redeployment required.

## User Stories & Acceptance Criteria

### US-1: View Project Header
**As a** project lead, **I want** to see the project title, subtitle (team/workstream/date context), and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are viewing.

**Visual Reference:** Header section of `OriginalDesignConcept.html` (`.hdr` element)

- [ ] The header displays the `title` field from `data.json` in 24px bold text.
- [ ] The `subtitle` field renders below the title in 12px gray (#888) text.
- [ ] The `backlogUrl` field renders as a clickable hyperlink styled in accent blue (#0078D4) next to the title.
- [ ] The header spans the full 1920px width with 44px horizontal padding.
- [ ] A 1px solid #E0E0E0 border separates the header from the timeline below.

### US-2: View Milestone Legend
**As an** executive viewer, **I want** to see a legend explaining the milestone icon types (PoC Milestone, Production Release, Checkpoint, Now line), **so that** I can interpret the timeline without additional explanation.

**Visual Reference:** Legend area in the header-right of `OriginalDesignConcept.html`

- [ ] Four legend items display horizontally in the top-right of the header with 22px gaps.
- [ ] PoC Milestone: 12×12px amber (#F4B400) diamond (rotated 45°).
- [ ] Production Release: 12×12px green (#34A853) diamond (rotated 45°).
- [ ] Checkpoint: 8×8px gray (#999) circle.
- [ ] Now line: 2×14px red (#EA4335) vertical bar.
- [ ] All labels render in 12px Segoe UI.

### US-3: View Milestone Timeline
**As a** project lead, **I want** to see a horizontal timeline with milestone swim lanes showing key events (checkpoints, PoC milestones, production releases) positioned by date, **so that** executives can see the project schedule at a glance.

**Visual Reference:** Timeline area (`.tl-area`) of `OriginalDesignConcept.html`

- [ ] The timeline area has a #FAFAFA background, is 196px tall, and spans the full width with 44px horizontal padding.
- [ ] A 230px-wide left sidebar lists milestone IDs and labels, one per swim lane, vertically distributed.
- [ ] Each milestone renders as a colored horizontal line spanning the full SVG width (1560px usable).
- [ ] Vertical month gridlines appear at computed positions for each month in the `timelineStartMonth` to `timelineEndMonth` range, with month labels at top.
- [ ] Checkpoint events render as open circles (white fill, colored stroke) on their milestone line.
- [ ] PoC events render as amber (#F4B400) diamond shapes with drop shadow.
- [ ] Production events render as green (#34A853) diamond shapes with drop shadow.
- [ ] Each event has a text label (date + description) positioned above or below the event marker.
- [ ] A dashed red (#EA4335) vertical "NOW" line appears at the x-position corresponding to `currentDate` from `data.json`.
- [ ] The "NOW" label renders in bold red 10px text.
- [ ] Event x-positions are calculated proportionally: `(dayOffset / totalDays) * svgWidth`.

### US-4: View Monthly Execution Heatmap
**As an** executive, **I want** to see a color-coded grid showing what was Shipped, In Progress, Carried Over, and Blocked for each month, **so that** I can quickly assess project execution health and trends.

**Visual Reference:** Heatmap section (`.hm-wrap`, `.hm-grid`) of `OriginalDesignConcept.html`

- [ ] The heatmap fills the remaining vertical space below the timeline.
- [ ] A section title reads "MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS" in 14px bold uppercase gray (#888) with 0.5px letter spacing.
- [ ] The grid uses CSS Grid with columns: `160px repeat(N, 1fr)` where N = number of months in `heatmap.months`.
- [ ] The first row contains column headers: a "STATUS" corner cell and one cell per month.
- [ ] The current month column header has an amber highlight (#FFF0D0 background, #C07700 text) with a "→ Now" indicator.
- [ ] Four data rows render in order: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red).
- [ ] Each row header cell uses uppercase bold 11px text with category-specific background and text colors.
- [ ] Each data cell lists work items as 12px text with a 6×6px colored bullet (circle) to the left.
- [ ] Cells in the current month column have a deeper-tinted background to visually highlight the present.
- [ ] Empty cells display a gray dash "—" placeholder.

### US-5: Configure Dashboard via JSON
**As a** project lead, **I want** to define all dashboard content (title, milestones, heatmap items) in a single `data.json` file, **so that** I can update the dashboard without modifying any code.

- [ ] The dashboard reads `data.json` from the `wwwroot/` directory at application startup.
- [ ] The JSON schema supports: `title`, `subtitle`, `backlogUrl`, `currentDate`, `timelineStartMonth`, `timelineEndMonth`, `milestones[]`, and `heatmap{}`.
- [ ] Changing `data.json` and refreshing the browser reflects the updated data (no rebuild needed).
- [ ] A `data.sample.json` file is provided as a template with fictional project data.
- [ ] If `data.json` is malformed or missing required fields, the app displays a friendly error message rather than a stack trace.

### US-6: Capture Screenshot-Ready Output
**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, animations, or browser chrome artifacts, **so that** I can take a clean screenshot for my PowerPoint deck.

- [ ] The `<body>` element is fixed at `width: 1920px; height: 1080px; overflow: hidden`.
- [ ] No CSS animations or transitions are present anywhere in the page.
- [ ] Font rendering uses `-webkit-font-smoothing: antialiased` for crisp text.
- [ ] The page is fully rendered on initial load with no loading spinners or skeleton screens.
- [ ] The viewport meta tag is set to `width=1920`.

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository. Engineers MUST consult this file and the rendered screenshot (`docs/design-screenshots/OriginalDesignConcept.png`) as the pixel-perfect target.

### Overall Page Layout

- **Dimensions:** Fixed 1920×1080px, no scroll, white (#FFFFFF) background.
- **Font:** `'Segoe UI', Arial, sans-serif` — system font, no web font loading.
- **Layout Model:** Vertical flex column (`display: flex; flex-direction: column`) with three stacked sections:
  1. Header (flex-shrink: 0, ~50px)
  2. Timeline (flex-shrink: 0, 196px)
  3. Heatmap (flex: 1, fills remaining ~834px)

### Section 1: Header (`.hdr`)

| Property | Value |
|----------|-------|
| Padding | `12px 44px 10px` |
| Border | Bottom: `1px solid #E0E0E0` |
| Layout | `display: flex; align-items: center; justify-content: space-between` |
| Title | `font-size: 24px; font-weight: 700; color: #111` |
| ADO Link | `color: #0078D4; text-decoration: none` (inline with title) |
| Subtitle | `font-size: 12px; color: #888; margin-top: 2px` |
| Legend (right) | Horizontal flex with `gap: 22px` |

**Legend Icons (rendered as inline `<span>` elements):**
- PoC Milestone: `12×12px; background: #F4B400; transform: rotate(45deg)` (creates diamond)
- Production Release: `12×12px; background: #34A853; transform: rotate(45deg)`
- Checkpoint: `8×8px; border-radius: 50%; background: #999`
- Now indicator: `2×14px; background: #EA4335` (vertical bar)

### Section 2: Timeline Area (`.tl-area`)

| Property | Value |
|----------|-------|
| Background | `#FAFAFA` |
| Height | `196px` (fixed) |
| Padding | `6px 44px 0` |
| Border | Bottom: `2px solid #E8E8E8` |
| Layout | `display: flex; align-items: stretch` |

**Left Sidebar (milestone labels):**
- Width: `230px` (flex-shrink: 0)
- Border: Right `1px solid #E0E0E0`
- Padding: `16px 12px 16px 0`
- Each milestone: `font-size: 12px; font-weight: 600; line-height: 1.4`
- Milestone ID color matches the milestone's line color
- Milestone label: `font-weight: 400; color: #444`

**SVG Timeline (`.tl-svg-box`):**
- Flex: 1 (fills remaining width)
- SVG viewbox: `width="1560" height="185"`
- Month gridlines: Vertical lines at equal intervals, `stroke: #bbb; stroke-opacity: 0.4`
- Month labels: `fill: #666; font-size: 11px; font-weight: 600`
- Milestone lines: Horizontal, `stroke-width: 3`, color from data
- Checkpoint circles: `r="5-7"; fill: white; stroke: [milestone-color]; stroke-width: 2.5`
- Small dot checkpoints: `r="4"; fill: #999` (no stroke)
- PoC diamonds: `<polygon>` rotated square, `fill: #F4B400; filter: drop-shadow`
- Production diamonds: `<polygon>` rotated square, `fill: #34A853; filter: drop-shadow`
- NOW line: `stroke: #EA4335; stroke-width: 2; stroke-dasharray: 5,3` (full height)
- NOW label: `fill: #EA4335; font-size: 10px; font-weight: 700`
- Event labels: `fill: #666; font-size: 10px; text-anchor: middle`
- Drop shadow filter: `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3">`

### Section 3: Heatmap (`.hm-wrap`)

| Property | Value |
|----------|-------|
| Padding | `10px 44px 10px` |
| Layout | `display: flex; flex-direction: column; flex: 1; min-height: 0` |

**Section Title (`.hm-title`):**
- `font-size: 14px; font-weight: 700; color: #888; letter-spacing: 0.5px; text-transform: uppercase`

**Grid (`.hm-grid`):**
- `display: grid; border: 1px solid #E0E0E0`
- Columns: `160px repeat(N, 1fr)` (N = month count)
- Rows: `36px repeat(4, 1fr)`

**Column Header Row:**

| Cell Type | Background | Font | Border |
|-----------|-----------|------|--------|
| Corner (`.hm-corner`) | `#F5F5F5` | `11px bold uppercase #999` | Right: `1px solid #E0E0E0`; Bottom: `2px solid #CCC` |
| Month Header (`.hm-col-hdr`) | `#F5F5F5` | `16px bold` | Right: `1px solid #E0E0E0`; Bottom: `2px solid #CCC` |
| Current Month Header | `#FFF0D0` | `16px bold #C07700` | Same borders |

**Data Row Color Scheme:**

| Category | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Bullet Color |
|----------|--------------|-----------------|---------|----------------------|-------------|
| Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

**Row Headers (`.hm-row-hdr`):**
- `font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.7px`
- `border-right: 2px solid #CCC; border-bottom: 1px solid #E0E0E0`
- Include emoji prefix: ✅ Shipped, 🔧 In Progress, 🔁 Carryover, 🚫 Blockers

**Data Cells (`.hm-cell`):**
- `padding: 8px 12px; border-right: 1px solid #E0E0E0; border-bottom: 1px solid #E0E0E0`
- Items (`.it`): `font-size: 12px; color: #333; padding: 2px 0 2px 12px; line-height: 1.35`
- Bullet: `6×6px circle` via `::before` pseudo-element, `position: absolute; left: 0; top: 7px`

### CSS Custom Properties (for theming)

```
--color-shipped: #34A853        --color-shipped-bg: #F0FBF0        --color-shipped-bg-current: #D8F2DA
--color-progress: #0078D4       --color-progress-bg: #EEF4FE       --color-progress-bg-current: #DAE8FB
--color-carryover: #F4B400      --color-carryover-bg: #FFFDE7      --color-carryover-bg-current: #FFF0B0
--color-blockers: #EA4335       --color-blockers-bg: #FFF5F5        --color-blockers-bg-current: #FFE4E4
--color-accent: #0078D4         --font-family: 'Segoe UI', Arial, sans-serif
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The dashboard renders immediately as a single full-screen (1920×1080) page with all three sections visible: header, timeline, and heatmap. All data is loaded from `data.json` on the server side — there is no loading spinner, skeleton screen, or progressive rendering. The page is screenshot-ready on first paint.

**Scenario 2: User Views the Header**
User sees the project title ("Privacy Automation Release Roadmap") in bold 24px text with a blue "→ ADO Backlog" hyperlink. Below it, a subtitle shows the team, workstream, and reporting month. On the right side, a horizontal legend displays four icon types with labels. No interaction is required — this is a read-only information banner.

**Scenario 3: User Clicks the ADO Backlog Link**
User clicks the blue "→ ADO Backlog" link in the header. A new browser tab opens navigating to the URL specified in `data.json`'s `backlogUrl` field. The dashboard remains unchanged.

**Scenario 4: User Reads the Milestone Timeline**
User scans the timeline section from left to right. Three horizontal swim lanes represent the project's major milestones (M1, M2, M3). Each lane has its own color. Events are plotted as circles (checkpoints) and diamonds (PoC/production) at their calendar positions. A dashed red vertical "NOW" line indicates the current date, providing an at-a-glance view of what's completed vs. upcoming.

**Scenario 5: User Hovers Over a Milestone Diamond**
User hovers the mouse over a yellow PoC diamond on the timeline. The browser's default cursor changes to a pointer (if styled) but no tooltip or popup appears — this is a static, screenshot-optimized view. Event labels are always visible as text rendered directly in the SVG.

**Scenario 6: User Reads the Heatmap Grid**
User scans the heatmap from top to bottom. The "Shipped" row (green) shows completed items per month. The "In Progress" row (blue) shows active work. The "Carryover" row (amber) shows items that slipped from the prior month. The "Blockers" row (red) shows impediments. The current month column is visually highlighted with a deeper background tint and an amber column header.

**Scenario 7: User Identifies the Current Month**
User sees that the "April → Now" column header is styled with an amber (#FFF0D0) background and dark amber (#C07700) text, immediately distinguishing it from past and future months. All four data cells in that column have a deeper background tint.

**Scenario 8: Heatmap Cell with No Items (Empty State)**
A month/category combination with no items displays a single gray dash "—" in the cell rather than being left blank. This prevents visual ambiguity about whether data is missing vs. there are genuinely no items.

**Scenario 9: User Updates data.json and Refreshes**
User edits `data.json` to add a new shipped item to April, saves the file, and refreshes the browser (F5). The dashboard re-reads `data.json` from disk and renders the updated heatmap with the new item visible in the Shipped/April cell.

**Scenario 10: Malformed data.json (Error State)**
User accidentally introduces a JSON syntax error in `data.json`. On page load, instead of a stack trace or blank page, the dashboard displays a centered error message: "Unable to load dashboard data. Please check data.json for syntax errors." with the specific deserialization error message in smaller gray text below.

**Scenario 11: Missing data.json (Error State)**
User deletes or renames `data.json`. On page load, the dashboard displays a friendly message: "data.json not found. Please place a valid data.json file in the wwwroot/ directory. See data.sample.json for the expected format."

**Scenario 12: User Takes a Screenshot**
User presses `Win+Shift+S` (or uses any screenshot tool) and captures the full browser viewport. Because the page is exactly 1920×1080 with no scrollbars and no animations, the screenshot is clean and ready to paste directly into a PowerPoint slide at full HD resolution.

**Scenario 13: Timeline with Overlapping Event Labels**
Two milestone events fall on dates close enough that their text labels would overlap. The rendering engine offsets the second label vertically (above vs. below the milestone line) to prevent text collision.

## Scope

### In Scope

- Single-page Blazor Server (.NET 8) web application running locally
- Header component with configurable title, subtitle, backlog URL, and icon legend
- SVG timeline component with data-driven milestone swim lanes, event markers (checkpoint circles, PoC diamonds, production diamonds), month gridlines, and a "NOW" line
- CSS Grid heatmap component with four status rows (Shipped, In Progress, Carryover, Blockers) and configurable month columns
- Current-month visual highlighting (amber column header, deeper cell tint)
- All content driven by a single `data.json` file in `wwwroot/`
- `data.sample.json` template with fictional project data included in the repository
- Fixed 1920×1080 layout optimized for screenshot capture
- Friendly error display for malformed or missing `data.json`
- CSS custom properties for easy color re-theming
- `dotnet run` deployment — no build scripts, containers, or cloud infrastructure

### Out of Scope

- **Authentication/authorization** — No login, no roles, no tokens
- **Database** — No SQLite, no LiteDB, no Entity Framework
- **Responsive/mobile design** — Fixed viewport only
- **Multi-project support** — Single `data.json`, single dashboard view
- **JSON editor UI** — Users edit `data.json` by hand or external tooling
- **ADO integration** — No live Azure DevOps API calls
- **Auto-refresh/file watching** — Manual browser refresh only (Phase 1)
- **PDF export or print stylesheet** — Screenshot capture is the output method
- **Automated screenshot capture** — No Playwright or headless browser scripting
- **Tooltips or interactive hover states** — Static, read-only rendering
- **Internationalization/localization** — English only
- **Accessibility (WCAG compliance)** — Not required for internal screenshot tool
- **CI/CD pipeline** — Not needed for a local utility
- **Unit or integration tests** — Optional for Phase 2; not required for MVP delivery

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Time to first meaningful paint** | < 500ms on localhost |
| **Full page render** | < 1 second from browser navigation to screenshot-ready state |
| **data.json load + deserialize** | < 100ms for files up to 100KB |
| **Memory footprint** | < 150MB total process (Blazor Server + Kestrel) |

### Security

| Requirement | Specification |
|-------------|--------------|
| **Authentication** | None — local-only access |
| **Network exposure** | Bind to `localhost` only (`http://localhost:5000`) |
| **Data sensitivity** | `data.json` excluded from version control via `.gitignore` |
| **HTTPS** | Optional — HTTP is acceptable for local use |

### Reliability

| Requirement | Specification |
|-------------|--------------|
| **Availability** | On-demand — user starts/stops the app manually |
| **Error handling** | Graceful degradation with user-friendly error messages for JSON errors |
| **Data durability** | `data.json` is the user's responsibility to back up |

### Scalability

Not applicable. This is a single-user, single-machine tool. No concurrent user support, horizontal scaling, or load balancing is needed.

### Browser Compatibility

| Browser | Support Level |
|---------|--------------|
| Microsoft Edge (Chromium) | Primary — standardize screenshots on this browser |
| Google Chrome | Supported |
| Firefox, Safari | Not tested, not supported |

### Screenshot Quality

- Render at 100% browser zoom, no DPI scaling
- Segoe UI system font — no FOUT (flash of unstyled text)
- No sub-pixel anti-aliasing differences between runs
- SVG elements render with drop shadow filter for milestone diamonds

## Success Metrics

| # | Metric | Target | Measurement |
|---|--------|--------|-------------|
| 1 | **Screenshot match** | Dashboard screenshot overlaid on reference PNG (`OriginalDesignConcept.png`) shows < 5% pixel deviation | Visual comparison at 1920×1080 |
| 2 | **Data update turnaround** | Project lead can edit `data.json` and capture an updated screenshot in < 2 minutes | Timed walkthrough |
| 3 | **Setup time** | New user can clone the repo, run `dotnet run`, and see the dashboard in < 5 minutes | Timed walkthrough with README |
| 4 | **File count** | Total solution is under 15 files (excluding `bin/`, `obj/`, `.gitignore`) | `find` command |
| 5 | **Zero external dependencies** | MVP builds and runs with only the default .NET 8 SDK — no additional NuGet packages | `dotnet restore` output |
| 6 | **Render time** | Page loads in under 1 second on localhost | Browser DevTools performance tab |
| 7 | **Executive readability** | At least 3 out of 5 test viewers can identify project status (on track / at risk / blocked) within 10 seconds of viewing the screenshot | Informal feedback |

## Constraints & Assumptions

### Technical Constraints

1. **Technology stack is mandated:** .NET 8 with Blazor Server. No alternative frontend frameworks (React, Angular, Vue) or backend frameworks (Node.js, Python).
2. **Fixed viewport:** 1920×1080px. The design is not responsive and does not support other resolutions.
3. **No JavaScript:** The entire UI must be rendered via Blazor Razor components with inline SVG and CSS. Zero JS interop for the MVP.
4. **System font dependency:** Segoe UI is required and is only natively available on Windows. macOS/Linux users would see Arial fallback.
5. **Static SSR preferred:** Use Blazor Static Server-Side Rendering (no `@rendermode InteractiveServer`) to avoid unnecessary SignalR circuit overhead for a read-only page.
6. **Single project, single page:** The architecture supports exactly one dashboard view from one `data.json` file.

### Timeline Assumptions

1. MVP delivery is estimated at **10 hours of engineering effort** (1–2 calendar days).
2. Phase 2 polish (file watcher, validation, sample data) adds **1 additional day**.
3. The HTML design reference (`OriginalDesignConcept.html`) is considered **final** — no design iteration is expected.
4. CSS from the reference HTML can be ported directly; no design system or component library alignment is needed.

### Dependency Assumptions

1. The development machine has **.NET 8 SDK** installed (8.0.x).
2. The development machine runs **Windows 10/11** (for Segoe UI font availability).
3. **Microsoft Edge or Google Chrome** is available for viewing and screenshotting.
4. The `OriginalDesignConcept.html` file in the ReportingDashboard repository is the **authoritative design source**. The rendered screenshot (`OriginalDesignConcept.png`) serves as the pixel-match target.
5. Users are comfortable **editing JSON by hand** (or have external tooling) — no in-app editor is provided.
6. `data.json` will be manually maintained — there is no live data feed, API integration, or database sync.
7. The dashboard is accessed **only from localhost** — no network deployment, no shared hosting.

### Data Assumptions

1. The heatmap will display **4–6 months** of data (configurable via `heatmap.months` array).
2. Each heatmap cell will contain **0–8 work items** — beyond 8 items, text may overflow the cell and be clipped.
3. The timeline will display **3–5 milestones** — beyond 5, vertical spacing in the SVG may become cramped.
4. The `currentDate` field in `data.json` is **user-controlled** (not derived from system clock), enabling point-in-time screenshots for any reporting date.