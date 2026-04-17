# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, delivery status, and monthly progress as a pixel-perfect 1920×1080 webpage optimized for PowerPoint screenshot capture. The dashboard reads all data from a local `data.json` configuration file, renders an SVG milestone timeline and a color-coded heatmap grid using Blazor Server (.NET 8), and requires zero authentication, no database, and no external dependencies beyond the .NET framework.

## Business Goals

1. **Eliminate manual slide creation** — Provide a living, data-driven dashboard that can be screenshotted directly into executive PowerPoint decks, replacing hand-drawn status slides that take hours to maintain.
2. **Standardize project reporting** — Establish a repeatable visual format for communicating project health (shipped, in progress, carryover, blockers) across workstreams and leadership reviews.
3. **Maximize simplicity** — Deliver a tool that any program manager can operate by editing a single JSON file in a text editor, with no training, no accounts, and no infrastructure requirements.
4. **Provide at-a-glance milestone visibility** — Give executives a timeline view of major project milestones (PoC, Production Release, Checkpoints) with a clear "NOW" indicator showing where the project stands relative to its plan.
5. **Enable monthly cadence reporting** — Support a rolling window of months in the heatmap so the dashboard can be updated and re-screenshotted each reporting cycle with minimal effort.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** program manager, **I want** to see the project title, organizational context, backlog link, and a legend of visual symbols when I open the dashboard, **so that** anyone viewing the screenshot immediately understands what project this is and how to read the visualization.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — top bar with title, subtitle, and legend icons.

- [ ] The page displays the project title from `data.json` as a bold 24px heading.
- [ ] A clickable backlog URL appears next to the title (rendered as a hyperlink).
- [ ] The subtitle line shows the organizational path and current month (e.g., "Trusted Platform · Privacy Automation Workstream · April 2026").
- [ ] A legend strip on the right side of the header shows four symbol definitions: PoC Milestone (amber diamond), Production Release (green diamond), Checkpoint (gray circle), and Now line (red vertical bar).
- [ ] All text, colors, and spacing match the header section of `OriginalDesignConcept.html`.

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing milestone tracks with date markers, diamond milestones, checkpoint circles, and a "NOW" indicator, **so that** I can instantly understand where each workstream stands relative to its planned milestones.

**Visual Reference:** Timeline/Gantt area of `OriginalDesignConcept.html` — the `.tl-area` section with SVG rendering.

- [ ] The timeline renders as an inline SVG, 1560px wide × 185px tall.
- [ ] Month boundaries appear as light vertical gridlines with month labels (Jan, Feb, Mar, etc.).
- [ ] Each milestone track from `data.json` renders as a horizontal colored line at a distinct Y-position.
- [ ] A left sidebar (230px wide) lists milestone IDs and labels (e.g., "M1 — Chatbot & MS Role") in their track color.
- [ ] Checkpoint events render as open circles with a stroke matching the track color.
- [ ] PoC events render as amber (`#F4B400`) diamond shapes with a drop shadow.
- [ ] Production Release events render as green (`#34A853`) diamond shapes with a drop shadow.
- [ ] A vertical dashed red line (`#EA4335`) appears at the current date position, labeled "NOW".
- [ ] Date labels appear near each event marker (e.g., "Mar 26 PoC").
- [ ] The timeline date range, milestones, and events are all driven by `data.json` — nothing is hardcoded.
- [ ] The date-to-pixel mapping formula is: `xPos = (date - startDate).TotalDays / (endDate - startDate).TotalDays * svgWidth`.

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was Shipped, In Progress, Carried Over, and Blocked for each month, **so that** I can assess project execution health at a glance.

**Visual Reference:** Heatmap grid section of `OriginalDesignConcept.html` — the `.hm-wrap` and `.hm-grid` area.

- [ ] The heatmap renders as a CSS Grid with a 160px row-header column and N equal-width month columns (driven by `data.json`).
- [ ] A header row displays month names; the current month column is highlighted with an amber background (`#FFF0D0`) and dark amber text (`#C07700`), with an arrow indicator ("◀ Now").
- [ ] Four status rows appear in this order: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red).
- [ ] Each row header uses the category's color scheme: distinct background, bold uppercase label, colored text.
- [ ] Each data cell lists work items as bulleted text (6px colored circle + 12–13px item text).
- [ ] Current-month cells use a slightly darker background variant (e.g., `#D8F2DA` instead of `#F0FBF0` for Shipped).
- [ ] Empty cells display a light dash character ("-") in muted text (`#AAA`).
- [ ] The number of months and items per cell are entirely data-driven from `data.json`.

### US-4: Configure Dashboard via JSON

**As a** program manager, **I want** to edit a single `data.json` file to change all dashboard content (title, milestones, months, work items), **so that** I can update the dashboard for each reporting cycle without touching any code.

- [ ] A `data.json` file in the project root defines all dashboard content.
- [ ] The JSON schema includes: `title`, `subtitle`, `backlogUrl`, `currentDate`, `months`, `milestones` (with events), `timelineStartDate`, `timelineEndDate`, and `categories` (with items per month).
- [ ] The application reads `data.json` on startup and deserializes it into a strongly-typed C# model.
- [ ] If `data.json` is missing, the dashboard displays a clear, user-friendly error message — not a stack trace.
- [ ] If `data.json` has malformed JSON or missing required fields, the dashboard displays a descriptive validation error.
- [ ] A `data.sample.json` file is provided as a well-commented template with fictional project data.

### US-5: Capture Pixel-Perfect Screenshots

**As a** program manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, loading spinners, or visual artifacts, **so that** I can take a browser screenshot and paste it directly into my PowerPoint deck.

- [ ] The page body is fixed at `width: 1920px; height: 1080px; overflow: hidden`.
- [ ] All content fits within the viewport without scrolling.
- [ ] No loading spinners, skeleton screens, or flash of unstyled content appear after initial render.
- [ ] The page uses the Segoe UI system font (with Arial fallback) — no web font loading delays.
- [ ] A screenshot taken via Chrome DevTools "Capture full size screenshot" at 1920×1080 matches the design reference.

### US-6: Run Dashboard Locally

**As a** developer or program manager, **I want** to run the dashboard on my local machine with a single command, **so that** I can view and screenshot it without any server infrastructure.

- [ ] `dotnet run` starts the application on `http://localhost:5000`.
- [ ] No HTTPS configuration is required.
- [ ] No database setup is required.
- [ ] No authentication prompts appear.
- [ ] `dotnet watch run` enables hot-reload for iterating on `data.json` and Razor/CSS changes.

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository. See also the rendered screenshot at `docs/design-screenshots/OriginalDesignConcept.png`. Engineers MUST match these visuals exactly, then apply the incremental improvements noted below.

### Overall Page Layout

- **Viewport:** Fixed 1920px × 1080px, white (`#FFFFFF`) background, `overflow: hidden`.
- **Font:** `'Segoe UI', Arial, sans-serif`, primary text color `#111`.
- **Layout Direction:** Vertical flex column (`display: flex; flex-direction: column`).
- **Horizontal Padding:** 44px on left and right for all major sections.
- **Three stacked sections** fill the viewport top-to-bottom:
  1. Header (flex-shrink: 0, ~50px)
  2. Timeline Area (flex-shrink: 0, fixed 196px height)
  3. Heatmap Area (flex: 1, fills remaining space)

### Section 1: Header Bar (`.hdr`)

- **Layout:** Flexbox row, `justify-content: space-between`, `align-items: center`.
- **Padding:** 12px top, 10px bottom, 44px left/right.
- **Bottom Border:** 1px solid `#E0E0E0`.
- **Left Side:**
  - Project title: `<h1>`, 24px, font-weight 700, color `#111`.
  - Inline backlog link: `<a>`, color `#0078D4`, no underline.
  - Subtitle: 12px, color `#888`, margin-top 2px. Format: "Org · Workstream · Month Year".
- **Right Side — Legend Strip:**
  - Four inline items in a flex row with 22px gap.
  - Each item: 12px text + icon shape.
  - PoC Milestone: 12×12px square, `background: #F4B400`, `transform: rotate(45deg)`.
  - Production Release: 12×12px square, `background: #34A853`, `transform: rotate(45deg)`.
  - Checkpoint: 8×8px circle, `background: #999`.
  - Now indicator: 2×14px bar, `background: #EA4335`.

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox row, `align-items: stretch`, 196px fixed height.
- **Background:** `#FAFAFA`.
- **Bottom Border:** 2px solid `#E8E8E8`.

#### Timeline Left Sidebar (230px)

- Fixed 230px width, flex-shrink 0.
- Right border: 1px solid `#E0E0E0`.
- Contains milestone labels stacked vertically with `justify-content: space-around`.
- Each label: Milestone ID (e.g., "M1") in 12px font-weight 600, colored by track color. Description below in font-weight 400, color `#444`.
- Track colors from the design: M1 = `#0078D4`, M2 = `#00897B`, M3 = `#546E7A`. (Data-driven from `data.json`.)

#### Timeline SVG Area (`.tl-svg-box`)

- Flex: 1, padding-left 12px, padding-top 6px.
- SVG element: 1560px wide × 185px tall, `overflow: visible`.
- **Month Gridlines:** Vertical lines at computed X positions, stroke `#bbb` opacity 0.4. Month labels at x+5, y=14, fill `#666`, 11px font-weight 600.
- **Milestone Tracks:** Horizontal lines spanning the full SVG width at evenly spaced Y positions (e.g., y=42, y=98, y=154 for 3 tracks). Stroke width 3, color matching the milestone's track color.
- **Event Markers on Tracks:**
  - **Checkpoint:** `<circle>` r=5–7, fill white, stroke matching track color, stroke-width 2.5.
  - **PoC Milestone:** `<polygon>` diamond shape (11px radius), fill `#F4B400`, with drop-shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`).
  - **Production Release:** `<polygon>` diamond shape (11px radius), fill `#34A853`, same drop-shadow filter.
  - **Small Checkpoint Dots:** `<circle>` r=4, fill `#999` (for minor checkpoints without labels).
- **Date Labels:** `<text>` elements positioned near markers, 10px, fill `#666`, `text-anchor: middle`.
- **"NOW" Line:** Vertical dashed line at the current date's X position. Stroke `#EA4335`, stroke-width 2, `stroke-dasharray: 5,3`. "NOW" label in 10px bold red at the top.

### Section 3: Heatmap Area (`.hm-wrap`)

- **Layout:** Flex column, flex: 1, padding 10px 44px.
- **Section Title:** 14px, font-weight 700, color `#888`, uppercase, letter-spacing 0.5px. Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers".

#### Heatmap Grid (`.hm-grid`)

- **CSS Grid:** `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months from `data.json`.
- **Grid Rows:** `36px` header row + `repeat(4, 1fr)` for the four status categories.
- **Outer Border:** 1px solid `#E0E0E0`.

##### Header Row

- **Corner Cell (`.hm-corner`):** Background `#F5F5F5`, centered text "STATUS" in 11px bold uppercase `#999`. Right border 1px `#E0E0E0`, bottom border 2px `#CCC`.
- **Month Header Cells (`.hm-col-hdr`):** Background `#F5F5F5`, centered month name in 16px bold. Right border 1px `#E0E0E0`, bottom border 2px `#CCC`.
- **Current Month Header (`.hm-col-hdr.current`):** Background `#FFF0D0`, text color `#C07700`. Append " ◀ Now" to the month label.

##### Status Rows (repeated for each category)

Each row consists of a **Row Header** and N **Data Cells**:

| Category | Row Header Color | Row Header BG | Cell BG | Current Month Cell BG | Bullet Color |
|----------|-----------------|---------------|---------|----------------------|--------------|
| Shipped | `#1B7A28` | `#E8F5E9` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#1565C0` | `#E3F2FD` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#B45309` | `#FFF8E1` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#991B1B` | `#FEF2F2` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

- **Row Header (`.hm-row-hdr`):** 11px bold uppercase, letter-spacing 0.7px. Includes emoji prefix (✅, 🔵, ⚠️, 🔴). Right border 2px `#CCC`, bottom border 1px `#E0E0E0`.
- **Data Cell (`.hm-cell`):** Padding 8px 12px. Each work item is a `<div class="it">` with 12–13px text, color `#333`, left-padding 12px for the bullet. The bullet is a 6×6px circle via `::before` pseudo-element, colored per the category.
- **Empty Cell:** Displays a single dash "-" in `#AAA`.
- **Last column cells** have no right border.

### Incremental Improvements Over Original Design

1. **Item text size:** Increase from 12px to 13px for better readability on projected screens.
2. **Fade-in animation:** CSS-only `@keyframes fadeIn` on page load (opacity 0→1 over 300ms).
3. **Hover tooltips:** Add `title` attributes to truncated work item text for native browser tooltip on hover.
4. **Dynamic month count:** Grid columns adapt to the number of months in `data.json` — no hardcoded 4-column limit.
5. **Dynamic current-month highlighting:** Derived from `currentDate` in `data.json`, not hardcoded to "Apr".

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Full Dashboard Render**
User navigates to `http://localhost:5000`. The page renders the complete dashboard in a single frame at 1920×1080: header with project title and legend, timeline with milestone tracks and "NOW" line, and heatmap grid with all status rows populated from `data.json`. A subtle CSS fade-in animation (300ms) plays. No loading spinner appears. The page is immediately ready for screenshot capture.

**Scenario 2: User Views the Header and Identifies the Project**
User sees the project title in bold at the top-left (e.g., "Project Phoenix — Cloud Migration Platform"). Next to the title, a blue hyperlink labeled "→ ADO Backlog" links to the Azure DevOps backlog. Below the title, the subtitle reads the org path and current month. On the right, the legend shows four icon definitions. The user immediately understands the project context and how to read the timeline and heatmap symbols.

**Scenario 3: User Reads the Milestone Timeline**
User looks at the timeline section below the header. Three horizontal colored lines represent milestone tracks (M1, M2, M3). Labels on the left sidebar identify each track (e.g., "M1 — Chatbot & MS Role" in blue). Along each track line, circles mark checkpoints and diamonds mark PoC (amber) and Production (green) milestones, each with a date label. A red dashed vertical line labeled "NOW" shows today's position. The user can see which milestones are past, current, and upcoming.

**Scenario 4: User Hovers Over a Milestone Diamond and Sees a Tooltip**
User moves their mouse over an amber PoC diamond on the timeline. The browser displays a native tooltip (via the `title` attribute) showing the milestone label and date (e.g., "Mar 26 PoC — Chatbot & MS Role"). This provides additional context without cluttering the visual.

**Scenario 5: User Scans the Heatmap for Current-Month Status**
User looks at the heatmap grid and immediately notices the highlighted column — the current month has an amber header (`#FFF0D0`) with "Apr ◀ Now" label. The cells in this column have slightly darker backgrounds than other months. The user scans down the highlighted column to see what shipped this month, what's in progress, what carried over, and what's blocked.

**Scenario 6: User Reads Work Items in a Heatmap Cell**
User focuses on the "In Progress" row under the "Apr" column. The cell contains two bulleted items (e.g., "• API Gateway Migration" and "• Auth Service Refactor"), each with a blue dot. The user reads the items to understand current work. If an item name is long and truncated by the cell width, hovering shows the full text in a browser tooltip.

**Scenario 7: User Views an Empty Heatmap Cell**
User looks at the "Blockers" row under the "Jan" column. No blockers existed in January, so the cell displays a single light-gray dash ("-") in `#AAA` text. This clearly communicates "nothing here" without leaving the cell confusingly blank.

**Scenario 8: User Clicks the Backlog Link**
User clicks the "→ ADO Backlog" hyperlink in the header. The browser navigates to the Azure DevOps backlog URL specified in `data.json`. This is the only interactive navigation on the page.

**Scenario 9: User Takes a Screenshot for PowerPoint**
User opens Chrome DevTools (`Ctrl+Shift+P` → "Capture full size screenshot") or uses Windows Snipping Tool. The page renders at exactly 1920×1080 with no scrollbars, no Blazor loading indicators, and no interactive hover states visible. The screenshot is clean and presentation-ready.

**Scenario 10: Data-Driven Rendering — Adding a New Month**
User edits `data.json` to add "May" to the `months` array and adds May items to each category. After restarting the app (or using hot-reload), the heatmap grid automatically expands to 5 month columns. The CSS Grid adapts via `repeat(5, 1fr)`. No code changes are needed.

**Scenario 11: Data-Driven Rendering — Adding a New Milestone Track**
User adds a fourth milestone ("M4") to the `milestones` array in `data.json` with its own events. The timeline renders a fourth horizontal track line below M3. The left sidebar shows four labels. Track Y-positions adjust automatically to distribute evenly within the 185px SVG height.

**Scenario 12: Error State — Missing data.json**
User starts the application without a `data.json` file present. Instead of a stack trace or blank page, the dashboard displays a centered, user-friendly error message: "Dashboard data not found. Please create a data.json file in the application directory. See data.sample.json for a template." The message uses the same Segoe UI font and clean styling as the rest of the application.

**Scenario 13: Error State — Malformed JSON**
User introduces a syntax error in `data.json` (e.g., a trailing comma). On next load, the dashboard displays a clear error: "Error reading data.json: Invalid JSON at line 42. Please fix the syntax and restart." No raw exception details are shown.

**Scenario 14: No Responsive Behavior — Fixed Viewport**
User opens the dashboard on a laptop with a 1366×768 screen. Horizontal and vertical scrollbars appear. This is expected and acceptable — the dashboard is designed for 1920×1080 screenshot capture, not interactive browsing. The user can zoom out (`Ctrl+minus`) or use a larger monitor for editing convenience.

## Scope

### In Scope

- Single-page Blazor Server application targeting .NET 8
- Header component with project title, subtitle, backlog link, and legend
- SVG milestone timeline with data-driven tracks, event markers, and "NOW" line
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- All content driven by a single `data.json` configuration file
- Strongly-typed C# data model (`DashboardData.cs`) with `System.Text.Json` deserialization
- Singleton `DashboardDataService` to read and expose the data
- `dashboard.css` ported from `OriginalDesignConcept.html` with exact color preservation
- Fixed 1920×1080 viewport optimized for screenshot capture
- Current-month column highlighting derived from `currentDate` in JSON
- Empty cell handling (display "-" in muted text)
- Hover tooltips via `title` attributes for truncated item names
- Graceful error display for missing or malformed `data.json`
- `data.sample.json` template file with fictional project data and comments
- CSS fade-in animation on page load
- `dotnet watch run` support for hot-reload during editing
- Localhost-only hosting on `http://localhost:5000`

### Out of Scope

- ❌ Authentication or authorization of any kind
- ❌ Database (SQLite, SQL Server, CosmosDB, or otherwise)
- ❌ REST API or GraphQL endpoints
- ❌ Admin panel or data editing UI within the app
- ❌ Responsive or mobile layout
- ❌ Dark mode or theme switching
- ❌ Export to PDF, Excel, or image from within the app
- ❌ Real-time data updates or SignalR push notifications
- ❌ Multi-project support (one `data.json` = one project = one page)
- ❌ CI/CD pipeline or automated deployment
- ❌ HTTPS or TLS certificate management
- ❌ Integration with Azure DevOps, Jira, or any external API
- ❌ User tracking, analytics, or telemetry
- ❌ Containerization or Docker support
- ❌ Third-party UI component libraries (MudBlazor, Radzen, etc.)
- ❌ JavaScript interop or client-side charting libraries

## Non-Functional Requirements

### Performance

| Metric | Target | Rationale |
|--------|--------|-----------|
| **Time to first meaningful paint** | < 500ms | Single user, localhost, no external dependencies. Page must appear instantly. |
| **Full render completion** | < 1 second | All SVG elements and heatmap cells must be visible within 1 second of navigation. |
| **data.json parse time** | < 50ms | File is expected to be < 50KB. `System.Text.Json` handles this trivially. |
| **Memory footprint** | < 100MB | Blazor Server baseline + one deserialized JSON object. No concern for local use. |

### Reliability

- The application must start and render correctly on the first attempt after `dotnet run`.
- If `data.json` is invalid, the application must not crash — it must display an error page.
- The application must function fully offline (no CDN fonts, no external CSS, no API calls).

### Security

- No authentication or authorization required.
- The application binds to `localhost` only — no external network exposure.
- `data.json` contains project status metadata only — no secrets, credentials, or PII.
- No HTTPS required for local-only use.

### Maintainability

- Zero external NuGet dependencies beyond the .NET 8 framework.
- Total CSS should remain under 200 lines.
- Total Razor components should remain under 8 files.
- Any developer with .NET experience should be able to understand and modify the project within 30 minutes.

### Compatibility

- **Target Runtime:** .NET 8 LTS (supported through November 2026).
- **Target Browser:** Latest Chromium-based browser (Edge or Chrome) for screenshot capture.
- **Target OS:** Windows 10/11 (Segoe UI font availability assumed).
- **Target Resolution:** 1920×1080 fixed viewport.

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Screenshot fidelity** | A browser screenshot at 1920×1080 matches the `OriginalDesignConcept.html` design within acceptable visual tolerance (layout, colors, typography, spacing). | Side-by-side visual comparison of screenshot vs. design reference. |
| 2 | **Data-driven rendering** | Changing any value in `data.json` (title, months, milestones, items) and reloading the page reflects the change correctly with no code modifications. | Edit `data.json`, reload, verify visual output. |
| 3 | **Setup simplicity** | A new developer can clone the repo, run `dotnet run`, and see the dashboard in under 2 minutes. | Timed walkthrough with a team member unfamiliar with the project. |
| 4 | **Error resilience** | Missing or malformed `data.json` produces a readable error message, not a crash or stack trace. | Test with deleted file, empty file, and malformed JSON. |
| 5 | **Executive adoption** | The PM uses dashboard screenshots in at least one executive presentation within the first reporting cycle. | PM confirms usage in their next leadership review. |
| 6 | **Update cycle time** | Updating the dashboard for a new month (editing `data.json` + taking a new screenshot) takes under 15 minutes. | PM self-reports time spent during second and subsequent update cycles. |

## Constraints & Assumptions

### Technical Constraints

- **Framework:** Must use Blazor Server on .NET 8 LTS — this is the team's standard stack and matches the existing `AgentSquad.Runner` project in the repository.
- **No JavaScript:** All rendering must be achievable with Razor markup, C#, inline SVG, and CSS. No JS interop or client-side libraries.
- **No External Dependencies:** Zero NuGet packages beyond the implicit `Microsoft.AspNetCore.App` framework reference. `System.Text.Json` is built-in and sufficient.
- **Fixed Resolution:** The page is designed for 1920×1080 only. No responsive breakpoints. No mobile support.
- **Single Data Source:** All dashboard content comes from one `data.json` file. No database, no API, no environment variables for content.
- **Localhost Only:** The application runs on `http://localhost:5000`. No public hosting, no reverse proxy, no load balancer.

### Timeline Assumptions

- **Phase 1 (Heatmap MVP):** 1–2 days of development effort.
- **Phase 2 (Timeline):** 1 day of additional development effort.
- **Phase 3 (Polish):** 0.5–1 day for incremental improvements.
- **Total estimated effort:** 2.5–4 days for a single developer.

### Dependency Assumptions

- The developer has .NET 8 SDK installed (8.0.x latest patch).
- The developer has access to a Chromium-based browser (Edge or Chrome) for viewing and screenshotting.
- The developer is running Windows 10/11 (for Segoe UI font availability). macOS/Linux developers would see Arial fallback, which is acceptable for development but not for final screenshots.
- The `OriginalDesignConcept.html` file in the ReportingDashboard repository is the authoritative design reference and will not change during development.

### Data Assumptions

- The `data.json` file will typically contain 3–6 months of data and 2–5 milestone tracks.
- Each heatmap cell will contain 0–8 work items. More than 8 items per cell may cause overflow; this is acceptable in v1 (items will be clipped by `overflow: hidden`).
- The program manager will manually edit `data.json` in a text editor (VS Code, Notepad++, etc.) approximately once per month.
- Work item names will be short phrases (under 60 characters). Longer names may be truncated visually but accessible via hover tooltip.

### Design Assumptions

- The `OriginalDesignConcept.html` represents the approved visual direction. The implementation should match it closely, with only the documented incremental improvements (font size bump, fade-in, dynamic months).
- The color palette defined in the design reference is final and must be preserved exactly — no reinterpretation or "close enough" substitutions.
- The heatmap's four-category structure (Shipped, In Progress, Carryover, Blockers) is fixed for v1. Adding or removing categories would require code changes.