# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page, screenshot-optimized executive reporting dashboard that visualizes project milestones on a timeline and displays monthly execution status in a color-coded heatmap grid. The dashboard reads all data from a local `data.json` file, requires zero authentication or cloud infrastructure, and is designed to be captured at 1920×1080 resolution for direct embedding into PowerPoint decks for executive stakeholders.

## Business Goals

1. **Eliminate manual slide creation** — Provide a live, data-driven dashboard that can be screenshotted directly into executive PowerPoint decks, replacing manually maintained status slides.
2. **Increase project visibility** — Give executives an at-a-glance view of what has shipped, what is in progress, what carried over from prior months, and what is blocked.
3. **Standardize status reporting** — Establish a repeatable, template-driven format for project milestone and execution reporting across teams.
4. **Minimize operational overhead** — Deliver a zero-dependency, local-only tool that any team member can run with `dotnet run` and update by editing a single JSON file.
5. **Enable rapid iteration** — Support hot-reload of data so that status updates appear in real time without restarting the application, enabling last-minute updates before executive reviews.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** program manager, **I want** to see the project title, team/workstream context, current date, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately understand what project and time period they are looking at.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` element.

- [ ] The header displays the project title in bold 24px text.
- [ ] A subtitle line shows the team name, workstream, and reporting month (e.g., "Trusted Platform · Privacy Automation Workstream · April 2026").
- [ ] An optional hyperlink labeled "→ ADO Backlog" appears next to the title, linking to the URL specified in `data.json`.
- [ ] A legend row displays four indicators: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now line (red vertical bar).
- [ ] All header content is driven by `data.json` fields: `title`, `subtitle`, `backlogUrl`.

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major project milestones across multiple workstreams, **so that** I can understand the sequencing and current progress of the project's big rocks.

**Visual Reference:** Timeline area of `OriginalDesignConcept.html` — `.tl-area` and inline `<svg>` element.

- [ ] The timeline renders as an inline SVG spanning the width of the page (approximately 1560px).
- [ ] Vertical grid lines mark the start of each month, labeled with abbreviated month names (Jan, Feb, Mar, etc.).
- [ ] Each milestone track renders as a colored horizontal line at a distinct Y position.
- [ ] A left sidebar (230px wide) lists track identifiers (e.g., "M1", "M2") and labels.
- [ ] Checkpoint milestones appear as small circles on the track line.
- [ ] PoC milestones appear as gold (`#F4B400`) diamond shapes with drop shadow.
- [ ] Production milestones appear as green (`#34A853`) diamond shapes with drop shadow.
- [ ] A dashed red (`#EA4335`) vertical "NOW" line is rendered at the position corresponding to the current date (auto-calculated, with optional override in JSON).
- [ ] Date labels appear above or below each milestone marker.
- [ ] The timeline supports 1–6 tracks as defined in `data.json`.

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing shipped items, in-progress work, carryover items, and blockers organized by month, **so that** I can quickly assess execution health and identify problem areas.

**Visual Reference:** Heatmap section of `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` elements.

- [ ] The heatmap renders as a CSS Grid with a fixed 160px row-header column and dynamically sized month columns.
- [ ] A header row shows month names, with the current month highlighted in amber/gold (`#FFF0D0` background, `#C07700` text).
- [ ] Four status rows are displayed: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red).
- [ ] Each cell lists individual work items as bulleted entries with a small colored dot matching the row category.
- [ ] Cells for the current month use a deeper shade of their category color to draw visual emphasis.
- [ ] Empty cells for future months display a dash (`-`) in muted gray.
- [ ] The number of month columns is dynamic, driven by the `heatmap.months` array in `data.json`.

### US-4: Load Dashboard Data from JSON

**As a** program manager, **I want** the dashboard to read all its data from a single `data.json` file, **so that** I can update project status by editing a simple text file without touching code.

- [ ] The application reads `data.json` from the project root on startup.
- [ ] The JSON schema supports: project metadata (title, subtitle, backlogUrl), timeline configuration (date range, tracks, milestones), and heatmap data (months, categories, items).
- [ ] Invalid or missing `data.json` displays a clear error message in the browser.
- [ ] The application ships with example fictional project data pre-populated in `data.json`.

### US-5: Auto-Reload on Data Change

**As a** program manager, **I want** the dashboard to automatically refresh when I save changes to `data.json`, **so that** I can iterate on the status content without restarting the app or refreshing the browser.

- [ ] A `FileSystemWatcher` monitors `data.json` for changes.
- [ ] When a change is detected, the data service reloads the file and notifies connected Blazor components.
- [ ] The UI re-renders within 2 seconds of a file save.
- [ ] Manual browser refresh (F5) also reloads the latest data as a fallback.

### US-6: Specify Alternate Data File via CLI

**As a** program manager managing multiple projects, **I want** to specify a different JSON data file via command-line argument, **so that** I can maintain separate dashboards for different projects.

- [ ] The application accepts `--data <filepath>` as a command-line argument (e.g., `dotnet run -- --data ./project-alpha.json`).
- [ ] If no `--data` argument is provided, the application defaults to `data.json` in the project root.
- [ ] If the specified file does not exist, the application displays a clear error message.

### US-7: Screenshot-Ready Rendering

**As a** program manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, **so that** I can take a clean full-page screenshot for my PowerPoint slides.

**Visual Reference:** `body` style in `OriginalDesignConcept.html` — `width:1920px;height:1080px;overflow:hidden`.

- [ ] The page body is fixed at 1920×1080 pixels with `overflow: hidden`.
- [ ] All content fits within the viewport without scrolling.
- [ ] The layout uses Segoe UI font, which is pre-installed on all target Windows machines.
- [ ] No browser chrome, scrollbars, or interactive controls appear that would degrade screenshot quality.

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository and `OriginalDesignConcept.png` rendered screenshot. Engineers MUST consult both files and match the visual output.

### Overall Layout

The page is a single 1920×1080px fixed-size viewport with `overflow: hidden`, using a vertical flex column layout (`display: flex; flex-direction: column`). The three major sections stack vertically:

1. **Header** (~50px) — fixed height, flex-shrink: 0
2. **Timeline Area** (196px) — fixed height, flex-shrink: 0
3. **Heatmap Area** (remaining space, ~834px) — flex: 1

### Section 1: Header (`.hdr`)

- **Layout:** Flexbox row, `justify-content: space-between`, `align-items: center`
- **Padding:** `12px 44px 10px`
- **Border:** Bottom `1px solid #E0E0E0`
- **Left side:**
  - Title: `<h1>` at `font-size: 24px`, `font-weight: 700`, `color: #111`
  - Backlog link: Inline `<a>` in title, `color: #0078D4`, no underline
  - Subtitle: `font-size: 12px`, `color: #888`, `margin-top: 2px`
- **Right side (Legend):**
  - Four legend items in a flex row with `gap: 22px`
  - PoC Milestone: 12×12px square rotated 45° (`transform: rotate(45deg)`), `background: #F4B400`
  - Production Release: 12×12px square rotated 45°, `background: #34A853`
  - Checkpoint: 8×8px circle, `background: #999`
  - Now indicator: 2×14px vertical bar, `background: #EA4335`
  - Label text: `font-size: 12px`, `color: #111`

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox row, `align-items: stretch`
- **Dimensions:** Height `196px`, `padding: 6px 44px 0`
- **Background:** `#FAFAFA`
- **Border:** Bottom `2px solid #E8E8E8`

#### Track Label Sidebar (left)
- **Width:** 230px, `flex-shrink: 0`
- **Layout:** Flex column, `justify-content: space-around`
- **Padding:** `16px 12px 16px 0`
- **Border:** Right `1px solid #E0E0E0`
- Each track label:
  - ID (e.g., "M1"): `font-size: 12px`, `font-weight: 600`, color matches track color
  - Description: `font-weight: 400`, `color: #444`
  - Track colors: M1 = `#0078D4`, M2 = `#00897B`, M3 = `#546E7A` (data-driven)

#### SVG Timeline (right, `.tl-svg-box`)
- **Layout:** `flex: 1`, `padding-left: 12px`, `padding-top: 6px`
- **SVG Dimensions:** `width="1560" height="185"`, `overflow: visible`
- **Month grid lines:** Vertical `<line>` elements at equal intervals (260px apart for 6 months), `stroke: #bbb`, `stroke-opacity: 0.4`
- **Month labels:** `<text>` at `font-size: 11`, `font-weight: 600`, `fill: #666`, font-family: Segoe UI
- **Track lines:** Horizontal `<line>` spanning full width, `stroke-width: 3`, color per track
  - Track Y positions: evenly spaced (approximately Y=42, Y=98, Y=154 for 3 tracks)
- **Checkpoint markers:** `<circle>` with `r="5-7"`, white fill, colored stroke matching track, `stroke-width: 2.5`
- **PoC milestone markers:** `<polygon>` diamond shape (11px radius), `fill: #F4B400`, with drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`)
- **Production milestone markers:** `<polygon>` diamond shape (11px radius), `fill: #34A853`, same drop shadow
- **Small checkpoint dots:** `<circle>` with `r="4"`, `fill: #999`
- **NOW line:** Vertical dashed `<line>`, `stroke: #EA4335`, `stroke-width: 2`, `stroke-dasharray: 5,3`
- **NOW label:** `<text>` "NOW", `fill: #EA4335`, `font-size: 10`, `font-weight: 700`
- **Date labels:** `<text>` near markers, `font-size: 10`, `fill: #666`, `text-anchor: middle`

### Section 3: Heatmap Area (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1`, `min-height: 0`
- **Padding:** `10px 44px 10px`

#### Heatmap Title
- `font-size: 14px`, `font-weight: 700`, `color: #888`
- `letter-spacing: 0.5px`, `text-transform: uppercase`
- `margin-bottom: 8px`
- Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"

#### Heatmap Grid (`.hm-grid`)
- **Layout:** CSS Grid
- **Columns:** `160px repeat(N, 1fr)` where N = number of months
- **Rows:** `36px repeat(4, 1fr)` (header row + 4 category rows)
- **Border:** `1px solid #E0E0E0`

#### Header Row
- **Corner cell (`.hm-corner`):** `background: #F5F5F5`, text "STATUS", `font-size: 11px`, `font-weight: 700`, `color: #999`, `text-transform: uppercase`, `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`
- **Month header cells (`.hm-col-hdr`):** `font-size: 16px`, `font-weight: 700`, `background: #F5F5F5`, `border-right: 1px solid #E0E0E0`, `border-bottom: 2px solid #CCC`, centered text
- **Current month header (`.apr-hdr`):** `background: #FFF0D0`, `color: #C07700`

#### Category Rows — Color Scheme

| Category | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Dot Color |
|----------|--------------|----------------|---------|----------------------|-----------|
| ✅ Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| 🔵 In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| 🟡 Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| 🔴 Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers (`.hm-row-hdr`)
- `font-size: 11px`, `font-weight: 700`, `text-transform: uppercase`, `letter-spacing: 0.7px`
- `padding: 0 12px`, `border-right: 2px solid #CCC`, `border-bottom: 1px solid #E0E0E0`

#### Data Cells (`.hm-cell`)
- `padding: 8px 12px`, `border-right: 1px solid #E0E0E0`, `border-bottom: 1px solid #E0E0E0`, `overflow: hidden`
- Each item (`.it`): `font-size: 12px`, `color: #333`, `padding: 2px 0 2px 12px`, `line-height: 1.35`
- Colored dot: `::before` pseudo-element, `width: 6px`, `height: 6px`, `border-radius: 50%`, positioned at `left: 0`, `top: 7px`

### Typography
- **Primary font:** `'Segoe UI', Arial, sans-serif`
- **Title:** 24px bold
- **Subtitle:** 12px regular, `#888`
- **Month headers:** 16px bold
- **Row headers:** 11px bold uppercase
- **Cell items:** 12px regular, `#333`
- **Timeline labels:** 10-11px, `#666`
- **Legend text:** 12px regular

### Global Styles
- `* { margin: 0; padding: 0; box-sizing: border-box; }`
- `body { width: 1920px; height: 1080px; overflow: hidden; background: #FFFFFF; }`
- `a { color: #0078D4; text-decoration: none; }`

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `localhost:5000`. The dashboard loads and renders the full page within 1 second. The header shows the project title, subtitle, and legend. The timeline shows all milestone tracks with markers. The heatmap shows all monthly execution data. The NOW line appears at the correct position for today's date. No loading spinner or skeleton screen is needed — the page renders server-side in a single pass.

**Scenario 2: User Views the Milestone Timeline**
User looks at the timeline area and sees horizontal colored track lines spanning January through June. Diamond markers indicate PoC and Production milestones at their respective dates. Small circles mark checkpoints. A red dashed vertical line labeled "NOW" indicates the current date. The user can visually trace each workstream's progress relative to the current date.

**Scenario 3: User Reads the Heatmap Grid**
User scans the heatmap from left to right. The current month column (e.g., April) is visually highlighted with a warmer background and amber header. The user reads the Shipped row to see what was delivered, the In Progress row for active work, the Carryover row for slipped items, and the Blockers row for impediments. Each item has a colored dot matching its category.

**Scenario 4: User Hovers Over a Heatmap Item (Phase 2 Enhancement)**
User hovers over an item in the heatmap cell. A subtle tooltip appears showing the full item name (useful if text was truncated due to cell width). The tooltip disappears when the mouse moves away. This interaction is optional for MVP and does not affect screenshot output.

**Scenario 5: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" hyperlink in the header. The browser navigates to the Azure DevOps backlog URL specified in `data.json`. This link is functional when viewing live but renders as static blue text in screenshots.

**Scenario 6: User Updates `data.json` and Sees Live Refresh**
User edits `data.json` in a text editor (e.g., VS Code), adds a new item to the "Shipped" category for April, and saves the file. Within 2 seconds, the dashboard automatically re-renders with the new item visible in the heatmap. No browser refresh is needed.

**Scenario 7: User Takes a Screenshot for PowerPoint**
User opens the dashboard in Chrome, ensures the browser window is sized to 1920×1080 (or uses DevTools device emulation), and takes a screenshot using the OS screenshot tool or browser capture. The resulting image is a clean, professional-looking status report with no browser chrome, scrollbars, or interactive artifacts.

**Scenario 8: Dashboard Loads with Missing or Invalid `data.json`**
The `data.json` file is missing, empty, or contains invalid JSON. The dashboard displays a centered error message: "Unable to load dashboard data. Please check that data.json exists and contains valid JSON." The error is styled simply with the same Segoe UI font on a white background.

**Scenario 9: Dashboard Renders with Empty Heatmap Data**
The `data.json` file is valid but some heatmap categories have no items for certain months. Those cells display a muted dash (`-`) in `color: #AAA` to indicate no data, rather than appearing blank.

**Scenario 10: User Runs Dashboard for a Different Project**
User runs `dotnet run -- --data ./project-beta.json` from the command line. The dashboard loads the alternate JSON file and renders that project's data instead. The header, timeline, and heatmap all reflect the data from the specified file.

**Scenario 11: NOW Line Position on Different Dates**
When the dashboard is viewed on different dates, the red NOW line automatically repositions based on the system clock. If `data.json` includes an optional `nowDate` override field, that date is used instead, enabling the user to generate snapshots as of a specific historical date.

**Scenario 12: SignalR Circuit Reconnection**
User leaves the dashboard open in a browser tab for an extended period. The Blazor Server SignalR circuit disconnects due to inactivity. The page displays a subtle reconnection overlay. Upon reconnecting, the dashboard re-renders with the latest `data.json` content.

## Scope

### In Scope

- Single-page Blazor Server application targeting .NET 8
- Header component with project title, subtitle, backlog link, and legend
- Inline SVG milestone timeline with configurable tracks, milestones, and checkpoints
- Auto-calculated NOW line with optional date override in JSON
- Color-coded heatmap grid with four status categories (Shipped, In Progress, Carryover, Blockers) × N months
- All data driven from a single `data.json` flat file
- `FileSystemWatcher` for automatic data reload on file change
- CLI `--data` argument for specifying alternate data files
- Fixed 1920×1080 layout optimized for screenshot capture
- CSS ported from `OriginalDesignConcept.html` reference design
- Example fictional project data pre-populated in `data.json`
- Graceful error display for missing or invalid JSON
- "Last Updated" timestamp in a footer area (Phase 2 polish)
- Subtle hover tooltips on heatmap items (Phase 2 polish)

### Out of Scope

- **Authentication and authorization** — No login, no user accounts, no RBAC
- **Database or persistent storage** — No SQL, no NoSQL, no cloud storage
- **Multi-user or collaborative features** — Single-user, single-browser tool
- **Responsive/mobile design** — Fixed 1920×1080 only; no tablet or phone layouts
- **CSS frameworks** — No Bootstrap, Tailwind, MudBlazor, or Radzen
- **JavaScript charting libraries** — No Chart.js, D3, or ApexCharts
- **Cloud deployment** — No Azure App Service, no Docker, no CI/CD pipeline
- **Automated screenshot capture** — Playwright automation deferred to Phase 2
- **Multi-project navigation** — Tabbed or dropdown project switcher deferred to Phase 3
- **Historical snapshot browsing** — Month-over-month navigation deferred to Phase 3
- **PDF export** — Deferred to Phase 3
- **Print-friendly CSS** — Deferred to Phase 3
- **Internationalization / localization** — English only
- **Accessibility (WCAG)** — Not a priority for this screenshot-oriented tool
- **Unit tests / integration tests** — Optional, deferred to Phase 2

## Non-Functional Requirements

### Performance
- **Initial page load:** The dashboard must render completely within 1 second on localhost (no network latency).
- **Data reload latency:** After `data.json` is saved, the UI must re-render within 2 seconds.
- **JSON file size:** The application must handle `data.json` files up to 1 MB without degradation (far exceeds expected use of ~5-20 KB).

### Reliability
- **FileSystemWatcher fallback:** If the file watcher misses an event, manual browser refresh (F5) must always load the latest data.
- **SignalR resilience:** The Blazor Server circuit should have an extended `DisconnectedCircuitRetentionPeriod` (e.g., 1 hour) and display a reconnection overlay when disconnected.
- **Error handling:** Malformed JSON must not crash the application; instead, display a user-friendly error message.

### Security
- **No authentication required.** The application binds to `localhost` only (loopback interface).
- **No sensitive data.** The `data.json` file contains project status text, not PII or credentials.
- **No HTTPS required** for local development (optional via `dotnet dev-certs https --trust`).
- **OS-level file permissions** (NTFS ACLs) are the only access control mechanism needed.

### Scalability
- **Not applicable.** This is a single-user, single-machine tool. No concurrent users, no horizontal scaling.
- **Timeline tracks:** The SVG timeline must support 1–10 tracks without layout degradation.
- **Heatmap columns:** The grid must support 1–12 month columns without overflow.

### Maintainability
- **Zero external NuGet dependencies** for the MVP. All functionality uses .NET 8 built-in libraries.
- **Self-contained deployment option:** `dotnet publish --self-contained -r win-x64` produces a single distributable folder with no runtime prerequisite.

### Screenshot Fidelity
- **Target resolution:** 1920×1080 pixels exactly.
- **Target font:** Segoe UI (pre-installed on Windows 10/11).
- **DPI:** Standard 96 DPI (100% scaling). Document the required browser zoom level for high-DPI displays.

## Success Metrics

| Metric | Target | Measurement Method |
|--------|--------|--------------------|
| **Screenshot parity** | Dashboard screenshot at 1920×1080 visually matches the `OriginalDesignConcept.png` reference in layout, colors, and typography | Side-by-side visual comparison by PM |
| **Data-driven rendering** | 100% of displayed text, milestones, and heatmap items are sourced from `data.json` (no hardcoded content) | Edit `data.json`, verify all changes appear |
| **Time to update status** | A PM can update `data.json` and capture a new screenshot in under 2 minutes | Timed walkthrough |
| **Zero-config startup** | `dotnet run` from the project directory launches the dashboard with no additional setup | Fresh clone + run test |
| **Error resilience** | Invalid JSON displays a friendly error; missing file displays a friendly error; neither crashes the app | Manual fault injection testing |
| **Multi-project support** | `dotnet run -- --data ./other.json` loads alternate data correctly | Test with 2+ JSON files |
| **Build success** | `dotnet build` completes with zero warnings and zero errors | CI or manual build |

## Constraints & Assumptions

### Technical Constraints

- **Runtime:** .NET 8 SDK must be installed on the development and execution machine (or use self-contained publish).
- **Browser:** The dashboard is designed for Chromium-based browsers (Chrome, Edge) at 1920×1080. Firefox and Safari are not tested or supported.
- **Operating System:** Windows 10/11 is the primary target (Segoe UI font dependency). macOS/Linux may work but are not guaranteed to produce identical screenshots.
- **No JavaScript:** The application must not require custom JavaScript. All rendering is done via Blazor Server-side components and inline SVG.
- **No CSS frameworks:** Pure CSS Grid + Flexbox only. No Bootstrap, Tailwind, or component libraries.
- **No database:** Data persistence is exclusively via the `data.json` flat file.

### Timeline Assumptions

- **Phase 1 (MVP):** 1–2 days — Functional dashboard matching the design reference, data-driven from JSON.
- **Phase 2 (Polish):** 1 day — FileSystemWatcher live reload, CLI data file argument, hover tooltips, footer timestamp, NOW line animation.
- **Phase 3 (Future):** Deferred indefinitely — Multi-project tabs, historical snapshots, PDF export, Playwright automation.

### Dependency Assumptions

- The `OriginalDesignConcept.html` file in the ReportingDashboard repository is the canonical visual reference and will not change during development.
- The `data.json` schema defined in this specification is final for Phase 1. Schema changes require a spec update.
- The target user (program manager) is comfortable editing JSON files in a text editor.
- Screenshots will be taken manually using OS-level tools (Snipping Tool, Win+Shift+S) or browser DevTools capture. Automated screenshot tooling is out of scope for MVP.
- The fictional example data in the shipped `data.json` should be realistic enough to serve as a template for real project data.