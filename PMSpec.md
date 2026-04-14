# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, delivery status, and monthly execution progress in a format optimized for 1920×1080 PowerPoint screenshots. The dashboard reads all data from a local `data.json` configuration file and renders a pixel-perfect view using Blazor Server (.NET 8), requiring zero authentication, zero cloud infrastructure, and zero external dependencies. This tool gives program managers a fast, repeatable way to produce polished roadmap visuals for leadership reviews.

## Business Goals

1. **Reduce executive reporting prep time by 75%** — Eliminate manual PowerPoint slide construction by generating a screenshot-ready dashboard directly from structured data.
2. **Ensure visual consistency across reporting cycles** — A single HTML/CSS template guarantees uniform formatting, colors, and layout every month, regardless of who prepares the deck.
3. **Enable rapid data updates** — Editing a single `data.json` file and refreshing the browser produces an updated dashboard in under 10 seconds, compared to 30–60 minutes of manual slide editing.
4. **Provide at-a-glance project health visibility** — Executives can see shipped items, in-progress work, carryover debt, and blockers in one view without navigating multiple tools (ADO, Jira, etc.).
5. **Maintain zero operational overhead** — No servers to maintain, no licenses to procure, no databases to back up. Runs entirely on a developer's local machine.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Metadata

**As a** program manager, **I want** to see the project title, organizational context (subtitle), and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and workstream this report covers.

*Visual Reference: Header section of `OriginalDesignConcept.html` — `.hdr` element*

- [ ] Dashboard displays a bold project title (24px, font-weight 700) left-aligned in the header bar.
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, colored `#0078D4`.
- [ ] A subtitle line appears below the title showing organization, workstream, and current month (12px, color `#888`).
- [ ] A legend appears right-aligned in the header showing icons for: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line).
- [ ] All header content is driven by `data.json` fields: `title`, `subtitle`, `backlogUrl`.

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major milestone tracks with dated markers, **so that** I can understand the project's planned trajectory and current position at a glance.

*Visual Reference: Timeline area of `OriginalDesignConcept.html` — `.tl-area` and SVG elements*

- [ ] The timeline section appears below the header with a light gray background (`#FAFAFA`) and a 196px fixed height.
- [ ] A left sidebar (230px wide) lists milestone track labels (e.g., M1, M2, M3) with track name and description.
- [ ] Each track renders as a horizontal colored line spanning the full SVG width (1560px).
- [ ] Milestone markers render at the correct date position: diamonds for PoC (gold `#F4B400`) and Production (green `#34A853`), circles for Checkpoints (gray `#999` or outlined with track color).
- [ ] A dashed red vertical line (`#EA4335`, stroke-dasharray `5,3`) marks the current date with a "NOW" label.
- [ ] Month labels (Jan, Feb, Mar, etc.) appear at the top of the SVG with vertical gridlines.
- [ ] Each milestone marker has a date label positioned above or below the track line.
- [ ] The number of tracks, milestone positions, and date range are all driven by `data.json`.

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked — organized by month, **so that** I can assess execution velocity and identify problem areas.

*Visual Reference: Heatmap grid of `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` elements*

- [ ] The heatmap fills the remaining vertical space below the timeline.
- [ ] A section title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase, 14px, color `#888`.
- [ ] The grid uses CSS Grid with columns: `160px repeat(N, 1fr)` where N = number of months.
- [ ] Column headers show month names (16px, bold); the current month column is highlighted with a gold background (`#FFF0D0`, text color `#C07700`) and labeled with "← Now".
- [ ] Four status rows appear: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red).
- [ ] Row headers use uppercase text with category-specific background colors and left-side emoji indicators (✅, 🔵, 🟡, 🔴 or equivalent).
- [ ] Each cell lists work items as bullet points (12px, `#333`) with a small colored circle prefix matching the row's status color.
- [ ] Current-month cells use a slightly darker background tint than other months in the same row.
- [ ] Empty future-month cells display a dash (`-`) in gray (`#AAA`).
- [ ] All heatmap data is driven by `data.json` `statusRows` object.

### US-4: Configure Dashboard via JSON

**As a** program manager, **I want** to update a single `data.json` file to change all dashboard content (title, milestones, status items), **so that** I can prepare reports for different months or projects without touching any code.

- [ ] A `data.json` file in the project root contains all configurable dashboard data.
- [ ] Changing `data.json` and refreshing the browser reflects the new data within 2 seconds.
- [ ] A `data.template.json` file is provided with documented schema, example values, and comments explaining each field.
- [ ] The JSON schema supports: project title, subtitle, backlog URL, current date, month list, milestone tracks (with id, label, color, and milestones array), and status rows (shipped, inProgress, carryover, blockers — each as a dictionary of month → item list).
- [ ] Malformed JSON displays a friendly error message on the page instead of a crash or blank screen.

### US-5: Capture Screenshot for PowerPoint

**As a** program manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, **so that** I can take a browser screenshot and paste it directly into a 16:9 PowerPoint slide without resizing or cropping.

- [ ] The page body is fixed at `width: 1920px; height: 1080px; overflow: hidden`.
- [ ] All content fits within the viewport without scrolling.
- [ ] The page renders identically in Chromium-based browsers (Edge, Chrome).
- [ ] Using Chrome DevTools "Capture full size screenshot" produces a clean 1920×1080 image.

### US-6: Run Dashboard Locally with Zero Setup

**As a** developer, **I want** to start the dashboard with a single `dotnet run` command and no external dependencies, **so that** I can get a working dashboard in under 60 seconds on any Windows machine with the .NET 8 SDK installed.

- [ ] `dotnet run` starts the application and opens on `http://localhost:5000` (or `https://localhost:5001`).
- [ ] No NuGet packages beyond the default Blazor Server template are required.
- [ ] No database, Docker, or cloud service is needed.
- [ ] The project builds with zero warnings on .NET 8.0.x LTS.

### US-7: Support Variable Number of Months

**As a** program manager, **I want** the heatmap to support a configurable number of month columns (e.g., 4, 6, or 12), **so that** I can create quarterly, half-year, or annual views depending on the audience.

*Visual Reference: Heatmap grid columns in `OriginalDesignConcept.html` — `grid-template-columns` pattern*

- [ ] The number of month columns is determined by the `months` array in `data.json`.
- [ ] The CSS Grid adjusts dynamically: `grid-template-columns: 160px repeat(@months.Count, 1fr)`.
- [ ] The SVG timeline width and date-to-pixel mapping adjusts based on the date range spanned by the months.
- [ ] The layout remains visually balanced with 3 to 8 month columns.

### US-8: Support Variable Number of Milestone Tracks

**As a** program manager, **I want** to define 1 to 5 milestone tracks in `data.json`, **so that** I can represent projects with different numbers of major workstreams.

*Visual Reference: Timeline track sidebar and SVG track lines in `OriginalDesignConcept.html`*

- [ ] The `tracks` array in `data.json` defines 1–5 tracks, each with a unique id, label, color, and milestones list.
- [ ] The SVG height and track spacing adjust dynamically based on the number of tracks.
- [ ] The left sidebar in the timeline area lists all track labels vertically, evenly spaced.
- [ ] Each track line is rendered with its configured color.

## Visual Design Specification

> **Canonical Reference:** `OriginalDesignConcept.html` in the ReportingDashboard repository. Engineers MUST consult this file and the rendered screenshot (`docs/design-screenshots/OriginalDesignConcept.png`) for pixel-level guidance.

### Overall Page Layout

- **Dimensions:** Fixed 1920×1080 pixels, no scrolling (`overflow: hidden`).
- **Background:** `#FFFFFF` (pure white).
- **Font Family:** `'Segoe UI', Arial, sans-serif`.
- **Base Text Color:** `#111`.
- **Layout Direction:** Vertical flex column (`display: flex; flex-direction: column`).
- **Three major vertical sections** stacked top-to-bottom:
  1. **Header** (~46px height, fixed)
  2. **Timeline Area** (196px height, fixed)
  3. **Heatmap Grid** (fills remaining ~838px, flex-grow)

### Section 1: Header (`.hdr`)

- **Padding:** `12px 44px 10px`.
- **Border:** Bottom border `1px solid #E0E0E0`.
- **Layout:** Flexbox row, `align-items: center; justify-content: space-between`.
- **Left side:**
  - **Title:** `<h1>` at 24px, font-weight 700. Contains project name followed by a link ("→ ADO Backlog") in `#0078D4`.
  - **Subtitle:** 12px, color `#888`, margin-top 2px. Shows org hierarchy and date context.
- **Right side — Legend:**
  - Horizontal flex row with 22px gap.
  - Four legend items, each 12px font:
    - **PoC Milestone:** 12×12px gold (`#F4B400`) square rotated 45° (diamond shape).
    - **Production Release:** 12×12px green (`#34A853`) square rotated 45° (diamond shape).
    - **Checkpoint:** 8×8px gray (`#999`) circle.
    - **Now indicator:** 2×14px red (`#EA4335`) vertical bar.

### Section 2: Timeline Area (`.tl-area`)

- **Height:** 196px, fixed (`flex-shrink: 0`).
- **Background:** `#FAFAFA`.
- **Border:** Bottom `2px solid #E8E8E8`.
- **Padding:** `6px 44px 0`.
- **Layout:** Flexbox row.
- **Left Sidebar** (230px wide, `flex-shrink: 0`):
  - Vertical flex column with `justify-content: space-around`.
  - Padding: `16px 12px 16px 0`.
  - Right border: `1px solid #E0E0E0`.
  - Each track label: 12px font-weight 600, line-height 1.4.
    - Track ID (e.g., "M1") in the track's assigned color.
    - Track name in font-weight 400, color `#444`.
  - Example track colors: M1 = `#0078D4`, M2 = `#00897B`, M3 = `#546E7A`.
- **SVG Timeline** (`.tl-svg-box`, `flex: 1`):
  - Padding: `left 12px, top 6px`.
  - SVG dimensions: `width="1560" height="185"`, `overflow: visible`.
  - **Month gridlines:** Vertical lines (`stroke: #bbb, opacity 0.4, width 1`) at equal intervals (~260px apart for 6 months).
  - **Month labels:** 11px, font-weight 600, color `#666`, positioned 5px right of each gridline, y=14.
  - **NOW line:** Dashed vertical red line (`stroke: #EA4335, width 2, dasharray 5,3`) at the current date's X position. "NOW" label in 10px bold red.
  - **Track lines:** Horizontal lines spanning the full SVG width (`stroke-width: 3`) in each track's color. Y positions evenly distributed (e.g., y=42, y=98, y=154 for 3 tracks).
  - **Checkpoint markers:** Circles (`r=5–7`) with white fill and track-color stroke (`stroke-width: 2.5`), or small solid gray dots (`r=4, fill: #999`).
  - **PoC milestone markers:** Diamond shapes via `<polygon>` with 4 points (±11px from center), fill `#F4B400`, with drop shadow filter.
  - **Production milestone markers:** Same diamond shape, fill `#34A853`, with drop shadow filter.
  - **Date labels:** 10px, color `#666`, `text-anchor: middle`, positioned above or below the track line (alternating to avoid overlap).
  - **Drop shadow filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`.

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Padding:** `10px 44px 10px`.
- **Layout:** Flex column, `flex: 1; min-height: 0`.
- **Section Title** (`.hm-title`):
  - 14px, font-weight 700, color `#888`, letter-spacing 0.5px, uppercase.
  - Margin-bottom 8px.
  - Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers".
- **Grid** (`.hm-grid`):
  - CSS Grid: `grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr)`.
  - Border: `1px solid #E0E0E0`.
  - `flex: 1; min-height: 0`.

#### Grid Header Row (36px)

| Cell | Style |
|------|-------|
| **Corner cell** (`.hm-corner`) | Background `#F5F5F5`, 11px bold uppercase `#999`, text "STATUS", centered, right border `1px solid #E0E0E0`, bottom border `2px solid #CCC` |
| **Month headers** (`.hm-col-hdr`) | Background `#F5F5F5`, 16px bold, centered, right border `1px solid #E0E0E0`, bottom border `2px solid #CCC` |
| **Current month header** (`.apr-hdr`) | Background `#FFF0D0`, text color `#C07700`, with "← Now" indicator |

#### Status Row Colors

| Row | Header BG | Header Text | Cell BG | Current Month Cell BG | Bullet Color |
|-----|-----------|-------------|---------|----------------------|--------------|
| **Shipped** | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| **In Progress** | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| **Carryover** | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| **Blockers** | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers (`.hm-row-hdr`)

- 11px, bold, uppercase, letter-spacing 0.7px.
- Padding: `0 12px`.
- Right border: `2px solid #CCC`.
- Bottom border: `1px solid #E0E0E0`.
- Prefixed with status emoji/icon (✅, 🔵, ⚠, 🔴).

#### Data Cells (`.hm-cell`)

- Padding: `8px 12px`.
- Borders: right `1px solid #E0E0E0`, bottom `1px solid #E0E0E0`.
- **Items** (`.it`): 12px, color `#333`, padding `2px 0 2px 12px`, line-height 1.35.
- **Bullet indicator:** `::before` pseudo-element — 6×6px circle, positioned absolute, left 0, top 7px, filled with the row's status color.
- **Empty cells:** Display "—" in `#AAA`.

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The dashboard renders the complete 1920×1080 view in under 2 seconds. The header shows the project title, subtitle, and legend. The timeline displays all milestone tracks with markers. The heatmap grid shows all status rows populated with data from `data.json`. The current month column is visually highlighted.

**Scenario 2: User Identifies Current Project Position**
User looks at the timeline and immediately sees the red dashed "NOW" line indicating today's date. They can compare milestone markers (diamonds) to the left of NOW (completed) versus to the right (upcoming) to gauge progress.

**Scenario 3: User Scans Execution Health**
User looks at the heatmap grid and scans left-to-right across the "Shipped" row to see delivery velocity. They then scan the "Blockers" row — if the current month cell has items listed in red, it signals active risk.

**Scenario 4: User Hovers Over ADO Backlog Link**
User hovers over the "→ ADO Backlog" link in the header. The cursor changes to a pointer (standard `<a>` behavior). Clicking opens the configured backlog URL from `data.json` in a new tab.

**Scenario 5: User Compares Months**
User visually compares the "Shipped" row across months. The current month column stands out due to its gold-tinted header (`#FFF0D0`) and slightly darker cell backgrounds, making it easy to distinguish from historical months.

**Scenario 6: User Captures Screenshot**
User opens Chrome DevTools (`Ctrl+Shift+I`), runs "Capture full size screenshot" from the command palette. The resulting PNG is exactly 1920×1080 and can be pasted directly into a 16:9 PowerPoint slide with no cropping needed.

**Scenario 7: User Updates Data for Next Month**
User opens `data.json` in a text editor, moves current "In Progress" items to "Shipped", adds new items to "In Progress", updates the `currentDate` field, and saves. They refresh the browser and see the updated dashboard immediately.

**Scenario 8: User Provides Malformed JSON**
User accidentally introduces a syntax error in `data.json` (e.g., trailing comma). Instead of a blank page or crash, the dashboard displays a centered error message: "Error loading dashboard data: [specific JSON parse error]. Please check data.json." styled in red on a white background.

**Scenario 9: User Views Dashboard with Empty Status Cells**
For future months where no items exist yet, the heatmap cells display a gray dash ("—") instead of blank space, making it clear these are intentionally empty rather than broken.

**Scenario 10: User Views Dashboard with Minimal Tracks**
User configures `data.json` with only 1 milestone track. The timeline SVG adjusts its height and the sidebar shows a single track label. The layout remains visually balanced without excessive whitespace.

**Scenario 11: User Views Milestone Marker Details**
User sees a gold diamond on a track line labeled "Mar 20 PoC". The date label is positioned near the diamond (above or below the track line) in 10px gray text, providing context without cluttering the timeline.

## Scope

### In Scope

- Single-page Blazor Server (.NET 8) application rendering a project roadmap dashboard
- Fixed 1920×1080 layout optimized for screenshot capture
- Header section with project title, subtitle, backlog link, and milestone legend
- SVG timeline with configurable milestone tracks (1–5 tracks), date markers (Checkpoint, PoC, Production), month gridlines, and a "NOW" indicator
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- Current month visual highlighting (gold header, darker cell backgrounds)
- All content driven by a single `data.json` file
- `data.template.json` with documented schema and example data
- Graceful error display for malformed JSON
- Local-only execution via `dotnet run` (Kestrel)
- Dynamic grid columns based on number of months in `data.json`
- Dynamic SVG positioning based on date range and milestone dates
- Fictional "Project Phoenix" example data pre-loaded for demo purposes
- CSS custom properties (variables) for easy color theming
- README with setup instructions and screenshot workflow

### Out of Scope

- **Authentication and authorization** — No login, no roles, no tokens.
- **Database or persistent storage** — No SQL, no SQLite, no Entity Framework. Data lives in `data.json`.
- **Real-time updates or WebSocket interactivity** — Page is static; refresh to update.
- **Responsive or mobile layout** — Fixed 1920×1080 only. No breakpoints.
- **Multi-page navigation or routing** — Single page, no tabs, no drill-down views.
- **Export/PDF generation** — Manual screenshot only. No Playwright, no PDF endpoint.
- **Multi-project support** — One `data.json` = one project. No project switcher.
- **Cloud deployment** — No Azure, no Docker, no CI/CD pipeline.
- **Accessibility compliance (WCAG)** — Optimized for visual screenshot capture, not screen readers.
- **Cross-browser testing** — Chromium-only (Edge/Chrome).
- **Animations or transitions** — Static render only.
- **Editing UI** — No in-browser editing of data. Edit `data.json` directly.
- **Historical data or versioning** — No time-series storage or diff views.
- **Internationalization (i18n)** — English only.
- **Dark mode** — Light theme only, matching the reference design.

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** | < 2 seconds from `dotnet run` to fully rendered dashboard |
| **Data refresh** | < 1 second from browser refresh to updated content |
| **JSON parse time** | < 100ms for `data.json` files up to 50KB |
| **Build time** | < 10 seconds (`dotnet build`) |
| **Startup time** | < 3 seconds (`dotnet run` to listening) |

### Rendering Fidelity

- Dashboard MUST render pixel-perfect at 1920×1080 in Chromium browsers (Chrome 120+, Edge 120+).
- All content MUST fit within viewport with no scrollbars.
- Screenshot output via Chrome DevTools MUST produce a clean image suitable for direct PowerPoint insertion.
- Font rendering MUST use Segoe UI on Windows (primary target platform).

### Reliability

- Application MUST start successfully with a valid `data.json`.
- Application MUST display a user-friendly error message if `data.json` is missing or malformed (not crash, not blank page).
- Application MUST handle nullable/missing fields in `data.json` gracefully (render what's available, skip what's missing).

### Security

- No authentication required.
- No sensitive data in transit (local-only access).
- `data.json` SHOULD be listed in `.gitignore` if it contains real project data.
- `data.template.json` with fictional data SHOULD be committed to the repository.

### Maintainability

- Codebase MUST have fewer than 15 files total (excluding `bin/`, `obj/`).
- Zero third-party NuGet dependencies beyond the default Blazor Server template.
- Solution MUST build with zero warnings on .NET 8.0.x LTS.
- Code MUST follow standard C# conventions and Blazor component patterns.

## Success Metrics

| # | Metric | Target | Measurement |
|---|--------|--------|-------------|
| 1 | **Time to first working dashboard** | ≤ 60 seconds from `git clone` | Run `dotnet run`, open browser, see fully rendered dashboard |
| 2 | **Time to update report data** | ≤ 2 minutes | Edit `data.json`, refresh browser, verify changes |
| 3 | **Screenshot quality** | 1920×1080 PNG suitable for executive PowerPoint | Visual inspection — no clipping, no scrollbars, no artifacts |
| 4 | **Visual fidelity to reference design** | ≥ 95% match to `OriginalDesignConcept.html` | Side-by-side comparison of screenshot vs. reference |
| 5 | **Zero runtime dependencies** | 0 external services required | Runs with only .NET 8 SDK installed |
| 6 | **Build cleanliness** | 0 warnings, 0 errors | `dotnet build` output |
| 7 | **File count** | ≤ 15 source files | `find . -name '*.cs' -o -name '*.razor' -o -name '*.css' -o -name '*.json' | wc -l` |
| 8 | **Data flexibility** | Supports 3–8 month columns and 1–5 milestone tracks | Tested with multiple `data.json` configurations |

## Constraints & Assumptions

### Technical Constraints

- **Runtime:** .NET 8.0.x LTS (must not use .NET 9 preview features).
- **Template:** Blazor Server project (`dotnet new blazor --interactivity Server`).
- **Browser target:** Chromium-based browsers only (Chrome, Edge). No Firefox/Safari testing required.
- **Resolution:** Fixed 1920×1080. No responsive design.
- **OS:** Primary development and execution on Windows (Segoe UI font availability assumed).
- **No JavaScript frameworks:** All rendering via Blazor Razor components and inline SVG. No React, Vue, Chart.js, D3, etc.
- **No CSS frameworks:** No Bootstrap, Tailwind, or Material. All CSS ported from the reference HTML.
- **Data format:** Single `data.json` file using `System.Text.Json` deserialization. No YAML, no TOML, no XML.

### Timeline Assumptions

- **Phase 1 (Static Port):** 1–2 hours — Pixel-perfect static replica of reference design in Blazor.
- **Phase 2 (Data-Driven):** 1–2 hours — Wire up `data.json` to replace hardcoded content.
- **Phase 3 (Polish):** 1 hour — Dynamic SVG positioning, error handling, template file, README.
- **Total estimated effort:** 3–5 hours for a developer familiar with Blazor.

### Dependency Assumptions

- The .NET 8 SDK is already installed on the development machine.
- The developer has access to the `OriginalDesignConcept.html` reference file in the ReportingDashboard repository.
- A Chromium browser is available for rendering verification and screenshot capture.
- No approval gates, security reviews, or compliance checks are required for this local tool.

### Data Assumptions

- `data.json` will be manually authored and maintained by the program manager.
- Data volume is small: typically 3–8 months, 1–5 tracks, and 0–10 items per status cell.
- The JSON schema will remain stable; breaking changes are acceptable during initial development since there is only one consumer.
- The `currentDate` field in `data.json` controls the "NOW" indicator position; it is NOT auto-detected from system time (allowing preparation of future-dated reports).

### Operational Assumptions

- The dashboard will be used by 1–3 people (program managers on the team).
- It will be run locally on demand (before leadership meetings), not continuously.
- The primary output artifact is a PNG screenshot, not the running application itself.
- No monitoring, alerting, or logging infrastructure is needed.