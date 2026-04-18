# PM Specification: My Project

## Executive Summary

We are building a single-page executive reporting dashboard — a Blazor Server (.NET 8) web application that reads project data from a `data.json` file and renders a clean, 1920×1080 screenshot-optimized view of project milestones, delivery status, and monthly execution health. The dashboard is modeled on `OriginalDesignConcept.html` from the `azurenerd/ReportingDashboard` repository and is designed solely to produce high-fidelity slides for PowerPoint executive decks. There is no authentication, no cloud dependency, and no database — just a local Kestrel server and a JSON config file.

---

## Business Goals

1. Provide executives with a single, scannable view of project health: what shipped, what is in progress, what carried over, and what is blocked.
2. Eliminate manual PowerPoint chart construction by producing screenshot-ready 1920×1080 visuals driven entirely by a JSON data file.
3. Enable PMs to update the dashboard in under 5 minutes by editing only `data.json` — no code changes required.
4. Faithfully reproduce and improve upon the `OriginalDesignConcept.html` visual design so that screenshots are indistinguishable from a professionally designed slide.
5. Reduce time-to-briefing for executive status updates from hours to minutes.

---

## User Stories & Acceptance Criteria

---

**Story 1:** **As a PM**, I want to update `data.json` with new project data so that the dashboard reflects the latest status without any code changes.

- [ ] `data.json` is the sole data source; no other files need editing to update content.
- [ ] The app reads and deserializes `data.json` at startup using `System.Text.Json` with `PropertyNameCaseInsensitive = true`.
- [ ] All fields — project title, subtitle, milestones, heatmap rows, and month list — are driven by `data.json`.
- [ ] If `data.json` is malformed, the app logs a clear error and shows a graceful error state rather than crashing.
- [ ] A sample `data.json` with fictional project data ships with the repository.

---

**Story 2:** **As a PM**, I want to navigate to `http://localhost:5000` in a browser and take a full-page screenshot so that I can paste it into a PowerPoint deck.

- [ ] The app starts with `dotnet run` and is accessible at `http://localhost:5000`.
- [ ] The rendered page is exactly 1920×1080 pixels with `overflow: hidden` so no scrollbar or clipping artifacts appear in screenshots.
- [ ] The page renders fully on first load without requiring user interaction.
- [ ] No login screen, splash screen, or loading spinner blocks the view.

---

**Story 3:** **As an executive**, I want to see a project header with the project name, workstream, and reporting period so that I can immediately identify the context of the report.
*(References: `OriginalDesignConcept.html` → `.hdr` section)*

- [ ] Header displays: project title (h1, 24px, bold), subtitle line (12px, `#888`) with team and month/year.
- [ ] Header includes an optional hyperlink (e.g., ADO Backlog URL) styled in `#0078D4`.
- [ ] Header right side shows a legend with four symbols: PoC Milestone (yellow diamond), Production Release (green diamond), Checkpoint (gray circle), Now line (red vertical bar).
- [ ] All header content is sourced from `data.json`.

---

**Story 4:** **As an executive**, I want to see a milestone timeline at the top of the dashboard so that I can understand the project's big-rock schedule at a glance.
*(References: `OriginalDesignConcept.html` → `.tl-area` section)*

- [ ] Timeline section renders as an SVG within a `196px`-tall strip below the header.
- [ ] Each milestone track (big rock) is represented as a horizontal colored line.
- [ ] Milestone track labels (e.g., "M1 – Feature Name") appear in a 230px-wide left panel, color-coded per track.
- [ ] SVG X-positions for milestones are calculated dynamically: `(date - startDate).TotalDays / totalDays * svgWidth`.
- [ ] Diamond markers (rotated squares, 12×12px) indicate PoC milestones (yellow `#F4B400`) and Production Release (green `#34A853`).
- [ ] Circle markers indicate checkpoints (gray `#999`, radius 4–7px depending on type).
- [ ] A vertical red dashed line (`#EA4335`, stroke-dasharray 5,3) marks the current date with "NOW" label.
- [ ] Month column gridlines and labels (Jan–Jun or as configured) render at correct proportional positions.
- [ ] All milestone data is sourced from `data.json`.

---

**Story 5:** **As an executive**, I want to see a monthly execution heatmap below the timeline so that I can quickly assess delivery health by category and month.
*(References: `OriginalDesignConcept.html` → `.hm-wrap` and `.hm-grid` sections)*

- [ ] Heatmap renders as a CSS Grid: `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months.
- [ ] Four status rows render in order: ✅ Shipped (green), 🔄 In Progress (blue), ⏩ Carryover (amber), 🔴 Blockers (red).
- [ ] Each cell contains bullet items (colored dot + text, 12px, `#333`).
- [ ] The current month column is highlighted with a warmer background (amber tint `#FFF0D0` header, `#FFF0B0` / `#DAE8FB` etc. cells) per row type.
- [ ] Empty future months show a single dash item in muted gray (`#AAA`).
- [ ] All heatmap data is sourced from `data.json`.
- [ ] Heatmap section title reads "Monthly Execution Heatmap – Shipped · In Progress · Carryover · Blockers" in uppercase, 14px, `#888`.

---

**Story 6:** **As a PM**, I want the current month column to be automatically highlighted based on system date so that I don't have to manually flag it in `data.json`.

- [ ] The app compares `DateTime.Now` month/year to each column month to determine the "current" column.
- [ ] Current month header cell receives the `apr-hdr` style class (or equivalent dynamic class).
- [ ] Current month data cells receive the darker-tint variant for each row type.

---

## Visual Design Specification

> **Canonical design file:** `OriginalDesignConcept.html` in the `azurenerd/ReportingDashboard` repository. All engineers MUST read this file before writing any layout or styling code.

### Canvas
- **Dimensions:** `width: 1920px; height: 1080px; overflow: hidden`
- **Background:** `#FFFFFF`
- **Font family:** `'Segoe UI', Arial, sans-serif`
- **Base text color:** `#111`

### Component Hierarchy
```
<body> (flex column)
  ├── .hdr          (header bar)
  ├── .tl-area      (timeline strip)
  │     ├── left label panel (230px wide)
  │     └── .tl-svg-box (inline SVG)
  └── .hm-wrap      (heatmap container, flex:1)
        ├── .hm-title
        └── .hm-grid (CSS Grid)
```

### Section 1: Header (`.hdr`)
- **Layout:** Flexbox, `justify-content: space-between`, `align-items: center`
- **Padding:** `12px 44px 10px`
- **Border:** `border-bottom: 1px solid #E0E0E0`
- **Title:** `font-size: 24px; font-weight: 700`
- **Subtitle:** `font-size: 12px; color: #888; margin-top: 2px`
- **Link color:** `#0078D4`
- **Legend (right side):** Flex row, `gap: 22px`
  - PoC: 12×12px yellow `#F4B400` rotated 45° square
  - Prod: 12×12px green `#34A853` rotated 45° square
  - Checkpoint: 8×8px gray `#999` circle
  - Now: 2×14px red `#EA4335` vertical bar

### Section 2: Timeline Strip (`.tl-area`)
- **Layout:** Flexbox, `height: 196px; flex-shrink: 0`
- **Background:** `#FAFAFA`
- **Border:** `border-bottom: 2px solid #E8E8E8`
- **Padding:** `6px 44px 0`
- **Left label panel:** `width: 230px; border-right: 1px solid #E0E0E0`
  - Labels: `font-size: 12px; font-weight: 600; line-height: 1.4`
  - Track colors (examples): M1 `#0078D4`, M2 `#00897B`, M3 `#546E7A`
- **SVG canvas:** `width: 1560px; height: 185px; overflow: visible`
  - Month gridlines: `stroke: #bbb; stroke-opacity: 0.4; stroke-width: 1`
  - Month labels: `font-size: 11px; font-weight: 600; fill: #666`
  - Track lines: `stroke-width: 3`, colored per track
  - NOW line: `stroke: #EA4335; stroke-width: 2; stroke-dasharray: 5,3`
  - Drop shadow filter on diamond markers: `feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`

### Section 3: Heatmap (`.hm-wrap`)
- **Layout:** `flex: 1; min-height: 0; flex-direction: column`
- **Padding:** `10px 44px 10px`
- **Title:** `font-size: 14px; font-weight: 700; color: #888; text-transform: uppercase; letter-spacing: 0.5px`
- **Grid (`.hm-grid`):** `display: grid; grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr); border: 1px solid #E0E0E0`

#### Grid Header Row
- Corner cell: `background: #F5F5F5; font-size: 11px; font-weight: 700; color: #999; text-transform: uppercase; border-right: 1px solid #E0E0E0; border-bottom: 2px solid #CCC`
- Column headers: `font-size: 16px; font-weight: 700; background: #F5F5F5; border-right: 1px solid #E0E0E0; border-bottom: 2px solid #CCC`
- Current month header: `background: #FFF0D0; color: #C07700`

#### Row Headers
- `font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.7px; border-right: 2px solid #CCC; border-bottom: 1px solid #E0E0E0; padding: 0 12px`

#### Row Color Palette
| Row | Header bg | Header text | Cell bg | Current month cell | Dot color |
|---|---|---|---|---|---|
| Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Data Cells
- `padding: 8px 12px; overflow: hidden`
- Item: `font-size: 12px; color: #333; padding: 2px 0 2px 12px; position: relative; line-height: 1.35`
- Dot: `position: absolute; left: 0; top: 7px; width: 6px; height: 6px; border-radius: 50%`

---

## UI Interaction Scenarios

**Scenario 1: Initial page load**
User navigates to `http://localhost:5000`. The page renders immediately at 1920×1080 with no loading spinner. All three sections (header, timeline, heatmap) are fully populated with data from `data.json`. The current month column in the heatmap is highlighted amber.

**Scenario 2: User views the header**
User reads the project title, subtitle (team + reporting month), and ADO Backlog link. The legend in the top-right shows four symbols identifying PoC milestones, Production Releases, Checkpoints, and the Now indicator.

**Scenario 3: User views the milestone timeline**
User scans the horizontal SVG timeline. Each "big rock" track is a colored horizontal line with its label on the left. Diamond markers (yellow = PoC, green = Prod) appear at milestone dates. Circle markers indicate intermediate checkpoints. A red dashed vertical line clearly marks today's date.

**Scenario 4: User reads milestone date labels**
Small date labels appear above or below diamond/circle markers (e.g., "Mar 26 PoC", "Apr Prod (TBD)"). Labels are 10px, `fill: #666`, positioned to avoid overlap where possible.

**Scenario 5: User scans the heatmap for delivery status**
User reads down the four rows (Shipped, In Progress, Carryover, Blockers) for each month. Each cell contains colored bullet items. The current month column is visually distinct due to the amber tint.

**Scenario 6: User identifies the current month**
The current month column header shows an amber background (`#FFF0D0`) and amber text (`#C07700`). All four cells in that column also have their respective darker-tint backgrounds, making the column clearly stand out from past and future months.

**Scenario 7: User views a future month column**
Future month cells contain a single dash item in muted gray (`color: #AAA`) indicating no data planned yet — preventing confusion with empty state vs. intentionally blank.

**Scenario 8: Empty state — data.json has no heatmap items for a category**
If a row category has zero items for a month (not a future placeholder), the cell renders an empty state dash item to maintain visual grid integrity.

**Scenario 9: Error state — data.json missing or malformed**
The app renders a simple centered error message: "Unable to load dashboard data. Please check data.json." The error is styled with the same font/background but no attempt is made to render partial data.

**Scenario 10: PM updates data.json and restarts**
PM edits `data.json`, restarts with `dotnet run`, navigates to `http://localhost:5000`, and immediately sees updated content. No browser cache clearing required (Blazor Server renders server-side).

**Scenario 11: Screenshot capture**
User opens browser at 1920×1080 viewport, navigates to the dashboard, uses browser full-page screenshot (or OS snipping tool at 1920×1080). The result is a clean, print-quality image with no scrollbars, no browser chrome artifacts, and no clipped content.

**Responsive behavior note:** The design is intentionally fixed at 1920×1080. On smaller monitors, horizontal scrolling is expected and acceptable. No responsive breakpoints are required.

---

## Scope

### In Scope
- Single Blazor Server (.NET 8) project: `ReportingDashboard.csproj`
- `Dashboard.razor` — one page, no routing
- `data.json` schema supporting: project metadata, milestone tracks (name, color, start date, milestones), heatmap months, and heatmap rows (Shipped / In Progress / Carryover / Blockers) with per-month items
- `DashboardDataService` — singleton, loads and caches `data.json` at startup
- SVG timeline rendered inline in Razor with dynamic X-position calculation via `TimelineCalculator` helper
- CSS Grid heatmap with scoped CSS (`Dashboard.razor.css`)
- Auto-detection of current month column from `DateTime.Now`
- Fictional sample `data.json` for a made-up software project
- Graceful error rendering if `data.json` is missing or invalid
- `dotnet watch` hot reload support
- bUnit + xUnit unit tests for `TimelineCalculator` and `DashboardDataService`

### Out of Scope
- Authentication or authorization of any kind
- HTTPS / TLS
- Multi-project switcher or routing between projects
- Database (SQL, SQLite, etc.)
- Hot reload of `data.json` without app restart (file watcher)
- Responsive / mobile layout
- Export-to-PDF or server-side screenshot generation
- User-editable UI (no forms, no inline editing)
- Cloud deployment (Azure, AWS, etc.)
- Email or notification integrations
- Third-party charting libraries (MudBlazor, Radzen, Chart.js, etc.)
- Internationalization / localization
- Accessibility compliance (WCAG)

---

## Non-Functional Requirements

- **Performance:** Page must fully render in under 2 seconds on localhost on any modern Windows machine.
- **Resolution fidelity:** Layout must render exactly at 1920×1080 with no overflow or clipping of content areas.
- **Data load time:** `data.json` must be parsed and injected into the component within 500ms of app start.
- **Startup time:** `dotnet run` to first browser render must complete in under 10 seconds on a developer workstation.
- **Reliability:** No unhandled exceptions on valid `data.json`; all deserialization uses `PropertyNameCaseInsensitive = true` for resilience.
- **Security:** No auth, no secrets, no sensitive data. App binds to `http://localhost:5000` only.
- **Maintainability:** All colors defined as named CSS variables or constants; no magic color strings scattered across components.
- **Portability:** App must run on any Windows machine with .NET 8 SDK installed via `dotnet run` — no additional tooling required.
- **Scalability:** Not applicable — single-user local tool.

---

## Success Metrics

- [ ] `dotnet run` starts the app and `http://localhost:5000` renders the full dashboard in under 2 seconds.
- [ ] A PM can update project data (title, milestones, heatmap items) by editing only `data.json` — no code changes.
- [ ] A 1920×1080 browser screenshot of the dashboard is presentation-ready for a PowerPoint deck without post-processing.
- [ ] The visual output matches `OriginalDesignConcept.html` pixel-faithfully in layout, color palette, and typography.
- [ ] `TimelineCalculator` unit tests pass with 100% coverage of date-range edge cases.
- [ ] The app handles a malformed `data.json` gracefully with a readable error message.
- [ ] The current month column is automatically highlighted correctly based on system date.

---

## Constraints & Assumptions

### Technical Constraints
- Target framework: `.NET 8` (`net8.0`); Blazor Server only — no Blazor WebAssembly.
- No third-party UI component libraries; raw Razor + CSS only.
- Font: Segoe UI is a system font on Windows; no web font CDN dependency.
- `data.json` is the sole data source; no database, no API calls.
- Fixed canvas: 1920×1080; responsive design is explicitly out of scope.
- `System.Text.Json` (built-in) only; no Newtonsoft.Json dependency.

### Timeline Assumptions
- Phase 1 MVP (heatmap + static timeline): 2–3 developer days.
- Phase 2 polish (dynamic SVG timeline, current-month auto-detection): 1 developer day.
- Total estimated effort: 3–4 developer days.

### Dependency Assumptions
- Developer machine has .NET 8 SDK installed.
- `OriginalDesignConcept.html` is available in the `azurenerd/ReportingDashboard` GitHub repository and is read by engineers before any layout work begins.
- The fictional sample `data.json` covers a 4–6 month window (Jan–Jun) to exercise all heatmap states.
- "Current month" is determined by `DateTime.Now` on the server; no timezone configuration is needed for a local tool.
- The number of heatmap months is driven by `data.json` (dynamic N columns), not hardcoded to 4.
- Multiple project configurations are not required; `data.json` represents one project at a time.