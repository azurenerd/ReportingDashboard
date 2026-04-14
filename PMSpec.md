# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones on a timeline and displays a color-coded heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers). The dashboard reads all data from a local `data.json` configuration file, runs as a lightweight Blazor Server application on localhost, and is designed to produce pixel-perfect 1920×1080 screenshots for inclusion in PowerPoint decks to executive stakeholders.

## Business Goals

1. **Provide at-a-glance project visibility** — Executives can see the full state of a project (milestones, shipped work, in-progress items, blockers) in a single static view without navigating multiple tools or reports.
2. **Enable PowerPoint-ready screenshots** — The dashboard renders at exactly 1920×1080 with a clean, professional design so that a browser screenshot can be pasted directly into an executive slide deck with zero post-processing.
3. **Minimize operational overhead** — The tool runs locally with no authentication, no cloud infrastructure, no database, and no external dependencies. Total cost of ownership is $0.
4. **Support rapid data updates** — A program manager can edit a single `data.json` file to update milestones, status items, and metadata, then immediately see the updated dashboard without redeploying or restarting the application.
5. **Establish a reusable template** — The dashboard should work for any project by changing only the `data.json` file, enabling the same tool to be used across multiple workstreams and teams.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Metadata

**As a** program manager, **I want** to see the project title, subtitle (team/workstream/date), and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are viewing.

**Visual Reference:** Header section (`.hdr`) in `OriginalDesignConcept.html`

- [ ] The page displays a bold project title (24px, font-weight 700) in the top-left
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, styled in `#0078D4`
- [ ] A subtitle line below the title shows team name, workstream, and report month (12px, `#888`)
- [ ] A legend appears in the top-right with four icon+label pairs: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), Now marker (red vertical line)
- [ ] All header data is driven from `data.json` fields (`title`, `subtitle`, `backlogUrl`)

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major milestones across multiple workstreams, **so that** I can understand the project's key dates and current progress at a glance.

**Visual Reference:** Timeline area (`.tl-area` and inline SVG) in `OriginalDesignConcept.html`

- [ ] The timeline section is 196px tall with a `#FAFAFA` background, separated from the header by a border
- [ ] A left sidebar (230px wide) lists workstream labels (e.g., "M1 — Chatbot & MS Role") with color-coded identifiers
- [ ] The SVG area (flex:1, ~1560px wide) renders one horizontal line per workstream, colored per workstream
- [ ] Milestones are rendered as: gold diamond (`#F4B400`) for PoC, green diamond (`#34A853`) for Production, open circle with stroke for Start, small gray circle (`#999`) for Checkpoint
- [ ] Each milestone has a date label (10px, `#666`) positioned above or below the marker to avoid overlap
- [ ] A vertical dashed red line (`#EA4335`, stroke-dasharray 5,3) marks the current date ("NOW") with a bold red label
- [ ] Month gridlines appear as light vertical lines with month abbreviation labels (11px, `#666`)
- [ ] Milestone X-positions are calculated from dates: `x = (date - startDate).TotalDays / (endDate - startDate).TotalDays * svgWidth`
- [ ] All workstreams and milestones are driven from `data.json`

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a grid of work items organized by status (Shipped, In Progress, Carryover, Blockers) and month, **so that** I can quickly assess execution health and identify problem areas.

**Visual Reference:** Heatmap grid (`.hm-wrap`, `.hm-grid`) in `OriginalDesignConcept.html`

- [ ] The heatmap section title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" (14px, uppercase, `#888`)
- [ ] The grid uses CSS Grid with columns `160px repeat(N,1fr)` where N = number of month columns from `data.json`
- [ ] The first row is a header row (36px) with a corner cell ("STATUS") and month column headers (16px, bold)
- [ ] The current month column header has an amber highlight (`#FFF0D0`, text `#C07700`) with a "← Now" indicator
- [ ] Four status rows are displayed: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each row header uses the status color scheme with uppercase text, an emoji prefix, and a 2px right border
- [ ] Data cells contain bulleted items (12px, `#333`) with a 6px colored circle bullet (matching row color) via CSS `::before`
- [ ] Current-month data cells have a darker tint (e.g., Shipped: `#D8F2DA` instead of `#F0FBF0`)
- [ ] Empty cells display a gray dash ("—")
- [ ] All heatmap items are driven from `data.json`

### US-4: Load Dashboard Data from JSON

**As a** program manager, **I want** the dashboard to read all its data from a single `data.json` file, **so that** I can update project status by editing one file without touching code.

- [ ] The application reads `wwwroot/data.json` on startup
- [ ] The JSON is deserialized into strongly-typed C# models (`DashboardData`, `Workstream`, `Milestone`, `StatusCategory`, `MonthItems`)
- [ ] If `data.json` is missing or contains invalid JSON, the application displays a user-friendly error message (not a stack trace)
- [ ] A `schemaVersion` field in the JSON is validated; mismatches produce a clear error
- [ ] Sample `data.json` ships with the project containing fictional "Project Atlas" data

### US-5: Live-Reload on Data Change

**As a** program manager, **I want** the dashboard to automatically refresh when I save changes to `data.json`, **so that** I can iterate on the report content without manually refreshing the browser.

- [ ] A `FileSystemWatcher` monitors `data.json` for changes
- [ ] Changes are debounced (500ms) to handle editors that perform atomic saves
- [ ] The dashboard re-renders via Blazor's `StateHasChanged()` mechanism within 1 second of the file save
- [ ] If the updated JSON is malformed, the previous valid data continues to display and an error banner appears

### US-6: Auto-Calculate Current Date Indicators

**As a** program manager, **I want** the "NOW" line on the timeline and the current-month column highlight to be auto-calculated from today's date, **so that** I don't need to manually update date markers each month.

- [ ] The red dashed "NOW" line X-position is calculated from `DateOnly.FromDateTime(DateTime.Today)`
- [ ] The current-month column in the heatmap is auto-detected and highlighted with the amber style
- [ ] Both auto-calculations can be overridden by explicit values in `data.json` (for generating historical snapshots)

### US-7: Screenshot-Ready Rendering

**As a** program manager, **I want** the page to render at exactly 1920×1080 with no scrollbars and no dynamic elements, **so that** I can capture a full-page screenshot that drops directly into a PowerPoint slide.

- [ ] The `<body>` is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] No scrollbars appear when all data fits within the viewport
- [ ] No loading spinners, skeleton screens, or animation are visible after initial render
- [ ] The page renders identically in Microsoft Edge on Windows (the reference browser)
- [ ] The README documents the Edge screenshot shortcut (`Ctrl+Shift+S → Capture full page`)

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` (located in the ReportingDashboard repository) and `C:/Pics/ReportingDashboardDesign.png`. Engineers MUST open and review these files before writing any code.

### Overall Page Layout

- **Viewport:** Fixed 1920×1080px, no scrolling, white (`#FFFFFF`) background
- **Font Family:** `'Segoe UI', Arial, sans-serif` — system font, no web font loading
- **Base Text Color:** `#111`
- **Layout Model:** Flexbox column (`display: flex; flex-direction: column`) — the page is three stacked horizontal bands
- **Side Padding:** 44px left/right on all three sections

### Section 1: Header Bar (`.hdr`)

- **Height:** Auto (approximately 50px)
- **Padding:** `12px 44px 10px`
- **Border:** `1px solid #E0E0E0` bottom
- **Layout:** Flexbox row, `space-between` alignment
- **Left Side:**
  - Title: `<h1>` at 24px, font-weight 700, color `#111`
  - Inline link: color `#0078D4`, no underline, preceded by "→" arrow character
  - Subtitle: 12px, color `#888`, margin-top 2px
- **Right Side (Legend):**
  - Four items in a horizontal flex row with 22px gap
  - Each item: 12px text with an inline icon to the left (6px gap)
  - PoC Milestone icon: 12×12px square, `#F4B400`, rotated 45° (diamond shape)
  - Production Release icon: 12×12px square, `#34A853`, rotated 45° (diamond shape)
  - Checkpoint icon: 8×8px circle, `#999`, border-radius 50%
  - Now marker icon: 2×14px rectangle, `#EA4335`

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed 196px
- **Background:** `#FAFAFA`
- **Border:** `2px solid #E8E8E8` bottom
- **Padding:** `6px 44px 0`
- **Layout:** Flexbox row, `align-items: stretch`

#### Workstream Label Sidebar

- **Width:** 230px, flex-shrink 0
- **Border:** `1px solid #E0E0E0` right
- **Padding:** `16px 12px 16px 0`
- **Layout:** Flexbox column, `justify-content: space-around`
- **Each Label:**
  - Workstream ID (e.g., "M1") in 12px, font-weight 600, color = workstream color
  - Workstream name on next line in 12px, font-weight 400, color `#444`
  - Workstream colors from design: M1 = `#0078D4` (blue), M2 = `#00897B` (teal), M3 = `#546E7A` (blue-gray)

#### SVG Timeline Canvas

- **Dimensions:** Width ~1560px (flex: 1), Height 185px
- **Padding:** `padding-left: 12px; padding-top: 6px`
- **Month Gridlines:** Vertical lines at evenly spaced intervals, stroke `#bbb` opacity 0.4, width 1px
- **Month Labels:** 11px, font-weight 600, color `#666`, positioned 5px right of each gridline at y=14
- **NOW Line:** Vertical dashed line, stroke `#EA4335`, width 2px, `stroke-dasharray: 5,3`, full height; label "NOW" in 10px bold `#EA4335` at top
- **Workstream Lines:** Horizontal lines spanning full SVG width, stroke = workstream color, width 3px; each workstream occupies a distinct Y-lane (y=42, y=98, y=154 in the reference)
- **Milestone Markers:**
  - **Start:** Open circle, radius 7px, white fill, stroke = workstream color, stroke-width 2.5
  - **Checkpoint:** Small filled circle, radius 4–5px, fill `#999` or open with `#888` stroke
  - **PoC:** Diamond (polygon rotated square), fill `#F4B400`, with drop-shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`)
  - **Production:** Diamond, fill `#34A853`, same drop-shadow filter
- **Milestone Labels:** 10px, color `#666`, `text-anchor: middle`, positioned above or below the marker (alternating to avoid overlap)

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1` (fills remaining viewport height)
- **Padding:** `10px 44px 10px`

#### Section Title

- **Text:** "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"
- **Style:** 14px, font-weight 700, color `#888`, uppercase, letter-spacing 0.5px, margin-bottom 8px

#### Grid Structure

- **CSS Grid:** `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months
- **Rows:** `grid-template-rows: 36px repeat(4, 1fr)`
- **Border:** `1px solid #E0E0E0` around entire grid

#### Header Row (36px)

| Cell | Style |
|------|-------|
| Corner ("STATUS") | Background `#F5F5F5`, 11px bold uppercase `#999`, center-aligned, right border `1px solid #E0E0E0`, bottom border `2px solid #CCC` |
| Month Headers | Background `#F5F5F5`, 16px bold, center-aligned, right border `1px solid #E0E0E0`, bottom border `2px solid #CCC` |
| Current Month Header | Background `#FFF0D0`, text color `#C07700`, with "← Now" indicator |

#### Status Rows (4 rows, each `1fr` height)

| Status | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Bullet Color |
|--------|--------------|-----------------|---------|----------------------|--------------|
| ✅ Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| 🔵 In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| 🟡 Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| 🔴 Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers

- 11px, font-weight 700, uppercase, letter-spacing 0.7px
- Padding: `0 12px`
- Right border: `2px solid #CCC`
- Bottom border: `1px solid #E0E0E0`

#### Data Cells

- Padding: `8px 12px`
- Right border: `1px solid #E0E0E0`, bottom border: `1px solid #E0E0E0`
- Items: 12px, color `#333`, line-height 1.35, padding `2px 0 2px 12px`
- Bullet: 6×6px circle via CSS `::before` pseudo-element, positioned `left: 0; top: 7px`, color matches row status
- Empty cells: single item showing "—" in color `#AAA`

### Complete Color Palette

| Token | Hex | Usage |
|-------|-----|-------|
| Background | `#FFFFFF` | Page background |
| Text Primary | `#111` | Body text |
| Text Secondary | `#888` | Subtitles, section titles |
| Text Tertiary | `#666` | Timeline labels |
| Text Item | `#333` | Heatmap cell items |
| Text Muted | `#AAA` | Empty cell dashes |
| Link | `#0078D4` | Hyperlinks |
| Border Light | `#E0E0E0` | Cell borders, section dividers |
| Border Medium | `#CCC` | Header row bottom borders, row header right borders |
| Border Heavy | `#E8E8E8` | Timeline section bottom |
| Surface | `#FAFAFA` | Timeline background |
| Surface Header | `#F5F5F5` | Grid header cells |
| Accent Now | `#FFF0D0` | Current month header background |
| Accent Now Text | `#C07700` | Current month header text |
| Green (Shipped) | `#34A853` | Shipped bullet, production milestone |
| Green Header BG | `#E8F5E9` | Shipped row header |
| Green Header Text | `#1B7A28` | Shipped row header text |
| Green Cell | `#F0FBF0` | Shipped cell background |
| Green Cell Active | `#D8F2DA` | Shipped current-month cell |
| Blue (In Progress) | `#0078D4` | In Progress bullet, M1 workstream |
| Blue Header BG | `#E3F2FD` | In Progress row header |
| Blue Header Text | `#1565C0` | In Progress row header text |
| Blue Cell | `#EEF4FE` | In Progress cell background |
| Blue Cell Active | `#DAE8FB` | In Progress current-month cell |
| Amber (Carryover) | `#F4B400` | Carryover bullet, PoC milestone |
| Amber Header BG | `#FFF8E1` | Carryover row header |
| Amber Header Text | `#B45309` | Carryover row header text |
| Amber Cell | `#FFFDE7` | Carryover cell background |
| Amber Cell Active | `#FFF0B0` | Carryover current-month cell |
| Red (Blockers) | `#EA4335` | Blockers bullet, NOW line |
| Red Header BG | `#FEF2F2` | Blockers row header |
| Red Header Text | `#991B1B` | Blockers row header text |
| Red Cell | `#FFF5F5` | Blockers cell background |
| Red Cell Active | `#FFE4E4` | Blockers current-month cell |
| Teal (M2) | `#00897B` | M2 workstream color |
| Blue-Gray (M3) | `#546E7A` | M3 workstream color |
| Checkpoint | `#999` | Checkpoint marker fill |

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5050`. The Blazor Server app loads `data.json`, renders the full dashboard in a single server round-trip, and displays the complete page at 1920×1080 with no loading spinner or skeleton screen. The header, timeline, and heatmap are all visible without scrolling.

**Scenario 2: User Views the Project Header**
User sees the project title in bold (e.g., "Project Atlas Release Roadmap") with a blue "→ ADO Backlog" link to the right. Below the title, a subtitle shows the team, workstream, and report month. In the top-right corner, a legend displays four milestone type icons with labels. All text is driven from `data.json`.

**Scenario 3: User Reads the Milestone Timeline**
User looks at the middle band of the page and sees a timeline spanning 6 months (Jan–Jun). Three horizontal colored lines represent workstreams (M1 blue, M2 teal, M3 blue-gray). Diamond markers indicate PoC (gold) and Production (green) milestones. Open circles mark start dates. Small gray dots mark checkpoints. A red dashed vertical "NOW" line shows today's position. Date labels appear above or below each marker.

**Scenario 4: User Hovers Over a Milestone Diamond**
User hovers over a gold PoC diamond on the timeline. The browser's default cursor changes to a pointer (if a tooltip or link is configured). In the MVP, no tooltip appears — this is a static screenshot-oriented view. (Phase 2 enhancement: add `<title>` SVG elements for native browser tooltips showing milestone details.)

**Scenario 5: User Scans the Heatmap for Blockers**
User's eye is drawn to the bottom row of the heatmap, which has a red-tinted background. Each cell in the Blockers row lists blocking items with red bullet points. The current month's column is highlighted with a darker red tint (`#FFE4E4`), drawing attention to active blockers. Empty future-month cells show a gray dash.

**Scenario 6: User Identifies Current Month at a Glance**
The current month column header has an amber background (`#FFF0D0`) with the text in `#C07700` and a "← Now" label. Every data cell in that column has a slightly darker background tint than adjacent months. Combined with the red "NOW" timeline marker, the user instantly knows where "today" falls.

**Scenario 7: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser navigates to the URL specified in `data.json` field `backlogUrl`. This opens in the same tab (standard `<a>` behavior). No JavaScript is involved.

**Scenario 8: User Edits data.json and Dashboard Updates**
User opens `data.json` in a text editor, changes a work item from "In Progress" to "Shipped," and saves the file. Within 1 second, the `FileSystemWatcher` detects the change, the `DataService` reloads and re-parses the JSON, and the Blazor circuit pushes the re-rendered page to the browser. The item moves from the blue In Progress row to the green Shipped row without a manual browser refresh.

**Scenario 9: User Saves Malformed JSON**
User accidentally introduces a syntax error in `data.json` (e.g., a missing comma). The `FileSystemWatcher` fires, `DataService` attempts to parse the file and catches the `JsonException`. The dashboard continues displaying the last valid data. A subtle error banner appears at the top of the page indicating "data.json has errors — showing last valid data." The banner disappears once the user fixes the JSON and saves again.

**Scenario 10: data.json File Is Missing on Startup**
User deletes or misplaces `data.json` before starting the application. Instead of a blank page or unhandled exception, the dashboard renders a centered error message: "No data.json found. Place a valid data.json file in the wwwroot/ directory and restart." The page uses the same font and color scheme as the dashboard for visual consistency.

**Scenario 11: User Takes a Screenshot for PowerPoint**
User opens the dashboard in Microsoft Edge at 1920×1080 resolution. They press `Ctrl+Shift+S` and select "Capture full page." The resulting PNG image is exactly 1920×1080 with no scrollbars, tooltips, or browser chrome. The user pastes this image directly into a PowerPoint slide at full-bleed size.

**Scenario 12: Dashboard Renders with Empty Heatmap Months**
The `data.json` defines month columns for May and June but includes no work items for those months. The heatmap renders those cells with a single gray dash ("—") in `#AAA`. The grid structure remains intact with consistent column widths. No cells collapse or show "undefined."

## Scope

### In Scope

- Single-page Blazor Server application rendering at 1920×1080
- Header section with configurable title, subtitle, backlog link, and milestone legend
- SVG timeline with multiple workstreams, milestone markers (start, checkpoint, PoC, production), date labels, month gridlines, and auto-positioned "NOW" line
- CSS Grid heatmap with 4 status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- Current-month auto-highlighting (column header and cell tints)
- All data driven from a single `data.json` file with strongly-typed C# models
- `FileSystemWatcher`-based live reload with 500ms debounce
- Graceful error handling for missing or malformed `data.json`
- Sample `data.json` with fictional "Project Atlas" data
- CSS ported directly from `OriginalDesignConcept.html` with CSS custom properties for colors
- `schemaVersion` validation in the JSON
- README with setup instructions, data.json schema documentation, and screenshot workflow
- Unit tests for JSON deserialization and date-to-pixel calculation logic
- Component tests (bUnit) for Dashboard.razor rendering

### Out of Scope

- **Authentication and authorization** — No login, no roles, no tokens, no OAuth
- **Database or persistent storage** — No SQL, no SQLite, no Entity Framework
- **Cloud deployment** — No Azure, no Docker, no CI/CD pipelines
- **Responsive/mobile layout** — Fixed 1920×1080 only; no media queries for smaller screens
- **Multi-user concurrency** — Single user on localhost only
- **In-browser data editing UI** — Data is edited via `data.json` file (Phase 3 enhancement)
- **Multi-project switching** — One `data.json` = one project per instance (Phase 3 enhancement)
- **PDF/image export from the app** — User captures screenshots manually via browser tools
- **Dark mode or theme switching** — Single light theme matching the design reference (CSS variables enable future theming)
- **Animations, transitions, or interactive charts** — Static rendering only
- **Accessibility (WCAG) compliance** — Not required for a single-user screenshot tool, though semantic HTML will be used where practical
- **Internationalization / localization** — English only
- **Telemetry, logging, or monitoring** — No Application Insights, no structured logging beyond console output
- **API endpoints** — No REST/GraphQL API; data is file-based

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Application startup time | < 2 seconds from `dotnet run` to first request served |
| Page load time (first meaningful paint) | < 500ms on localhost |
| Live reload latency (file save → re-render) | < 1 second |
| Memory footprint | < 100MB working set |

### Security

| Requirement | Implementation |
|-------------|---------------|
| Network binding | `localhost` only — `app.Urls.Add("http://localhost:5050")` |
| No secrets in data | `data.json` contains project status text, not credentials or PII |
| File system access | Read-only access to `wwwroot/data.json`; no write operations |
| No external network calls | Zero outbound HTTP requests; fully offline-capable |

### Reliability

| Requirement | Implementation |
|-------------|---------------|
| Malformed JSON handling | Last-known-good data displayed; error banner shown |
| Missing file handling | Clear error page with instructions |
| FileSystemWatcher failures | Fallback to manual browser refresh (`F5`) |
| SignalR circuit drop | Blazor reconnection UI shown automatically (built-in) |

### Compatibility

| Requirement | Target |
|-------------|--------|
| Primary browser | Microsoft Edge (Chromium) on Windows |
| Secondary browser | Google Chrome (expected visual parity within 1-2px) |
| .NET SDK | .NET 8.0 LTS (any patch version) |
| Operating System | Windows 10/11 (primary); macOS/Linux supported but not tested |

### Maintainability

- Zero external NuGet dependencies for the main project
- Total codebase: ~5 files beyond scaffolding (~500 lines of application code)
- CSS: ~150 lines, ported directly from the HTML reference
- No custom JavaScript; no JS interop

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Visual fidelity** | Dashboard screenshot is indistinguishable from `OriginalDesignConcept.html` when rendered side-by-side at 1920×1080 in Edge | Manual pixel comparison by PM |
| 2 | **Data-driven rendering** | Changing any value in `data.json` is reflected on the dashboard within 1 second without code changes | Edit 5 different fields; verify each renders correctly |
| 3 | **Time to first screenshot** | A new user can clone the repo, run `dotnet run`, and capture a screenshot in < 5 minutes | Timed walkthrough by a team member unfamiliar with the project |
| 4 | **Implementation effort** | Phase 1 MVP completed within 4–8 hours of engineering time | Time tracking |
| 5 | **Zero runtime errors** | No unhandled exceptions during normal operation (load, display, live-reload) | Run for 1 hour with periodic `data.json` edits |
| 6 | **Test coverage** | All C# model deserialization paths and date-to-pixel calculations covered by unit tests | xUnit test suite passes with 100% model coverage |
| 7 | **PowerPoint readiness** | Screenshot pastes into a 16:9 PowerPoint slide at full width with no cropping or resizing needed | Insert screenshot into a standard widescreen slide template |

## Constraints & Assumptions

### Technical Constraints

1. **Fixed viewport** — The page MUST render at exactly 1920×1080px. This is not negotiable; it is the core requirement for screenshot capture.
2. **No external dependencies** — The main project (`ReportingDashboard.csproj`) must have zero NuGet package references beyond the .NET 8 SDK. All functionality uses built-in .NET 8 libraries.
3. **Blazor Server** — The technology choice is Blazor Server (not Blazor WebAssembly, not Razor Pages, not MVC). This is chosen for hot-reload developer experience and future interactivity potential.
4. **Single JSON file** — All dashboard data lives in one `data.json` file. No database, no API, no environment variables for data.
5. **Localhost only** — The app binds to `http://localhost:5050`. It is not designed to be exposed on a network.
6. **System font** — Segoe UI is used as the primary font. This is a Windows system font; no web font loading is permitted (it would delay rendering and risk FOUT in screenshots).
7. **No JavaScript** — The entire dashboard renders via Blazor Server-side rendering and pure CSS. Zero JavaScript files, zero JS interop calls.

### Timeline Assumptions

- Phase 1 (MVP) is estimated at 4–8 hours of engineering effort
- Phase 2 (Polish) is estimated at 2–4 hours, scheduled at engineering's discretion
- Phase 3 (Future Enhancements) is not scheduled and will be prioritized based on user feedback

### Dependency Assumptions

- Engineers have .NET 8.0 SDK installed locally
- Engineers have access to the `OriginalDesignConcept.html` file in the ReportingDashboard repository and the `C:/Pics/ReportingDashboardDesign.png` reference image
- Microsoft Edge (Chromium) is available on the development machine for visual testing
- The `data.json` schema is defined once and validated via C# `required` record properties; no external JSON Schema file is needed

### Design Assumptions

- The `OriginalDesignConcept.html` is the authoritative visual specification. Any ambiguity in this PM spec should be resolved by consulting the HTML file directly.
- The heatmap displays a configurable number of month columns (defined in `data.json`), defaulting to 4 based on the reference design.
- The timeline span (start month to end month) is defined in `data.json` and milestone positions are calculated relative to that span.
- The "NOW" line defaults to auto-calculation from the system date but can be overridden in `data.json` for historical snapshot generation.
- Work items in the heatmap are simple text strings. No rich formatting, links, or nested structures within items.

### Open Decisions (Defaulted for MVP)

| Decision | Default for MVP | Can Revisit In |
|----------|----------------|----------------|
| Number of heatmap months | Configurable via `data.json` (default 4) | Phase 2 |
| Timeline span calculation | Manual start/end dates in `data.json` | Phase 2 |
| Multi-project support | One project per app instance | Phase 3 |
| Screenshot browser | Microsoft Edge (documented in README) | — |
| NOW line positioning | Auto from system date, overridable in JSON | Phase 1 |
| Data editing method | Hand-edit `data.json` | Phase 3 |