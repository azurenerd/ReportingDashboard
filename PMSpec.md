# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, shipped deliverables, in-progress work, carryover items, and blockers in a clean, screenshot-optimized layout. The dashboard reads all data from a local `data.json` configuration file, renders a pixel-perfect 1920×1080 view matching the `OriginalDesignConcept.html` design template, and is intended to be captured as screenshots for PowerPoint executive presentations. This is a zero-dependency, local-only Blazor Server application with no authentication, no database, and no cloud infrastructure.

## Business Goals

1. **Provide at-a-glance project visibility for executives** — Deliver a single-page view that communicates project health, milestone progress, and execution status without requiring executives to navigate ADO boards or dashboards.
2. **Eliminate manual PowerPoint chart creation** — Replace the time-consuming process of building status slides by hand with a screenshot-ready webpage that can be captured directly into executive decks.
3. **Standardize project reporting format** — Establish a reusable, data-driven template that any project lead can populate by editing a JSON file, ensuring consistent reporting across teams.
4. **Minimize setup and maintenance burden** — Deliver a solution that requires zero infrastructure, zero accounts, and zero ongoing operational cost — just `dotnet run` and a browser.
5. **Enable rapid report updates** — Allow project leads to update the dashboard content by editing a single `data.json` file and refreshing the browser, with no build or deployment step required.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** the dashboard to display the project title, organizational context, report date, and a link to the ADO backlog, **so that** executives immediately understand which project and time period the report covers.

**Acceptance Criteria:**
- [ ] The header displays the project title in bold 24px font at the top-left
- [ ] A subtitle line shows the organizational hierarchy and report month (e.g., "Engineering Division · Platform Modernization · April 2026")
- [ ] A clickable link labeled "→ ADO Backlog" appears inline with the title, linking to the URL specified in `data.json`
- [ ] All header values are driven by `data.json` — no hardcoded text
- [ ] **Visual reference:** Header section of `OriginalDesignConcept.html` — white background, `#E0E0E0` bottom border, 44px horizontal padding

### US-2: View Milestone Timeline Legend

**As an** executive, **I want** a legend in the header area that explains the meaning of each timeline symbol (PoC diamond, Production diamond, Checkpoint circle, Now line), **so that** I can interpret the timeline without asking for clarification.

**Acceptance Criteria:**
- [ ] Four legend items appear right-aligned in the header: PoC Milestone (gold diamond `#F4B400`), Production Release (green diamond `#34A853`), Checkpoint (gray circle `#999`), Now line (red vertical bar `#EA4335`)
- [ ] Legend items are 12px font with inline shape indicators matching the SVG symbols
- [ ] Legend is driven by static rendering (not data-driven) as these are universal symbols
- [ ] **Visual reference:** Top-right of header in `OriginalDesignConcept.html`

### US-3: View Milestone Timeline with Track Labels

**As an** executive, **I want** to see a horizontal timeline showing major workstreams as labeled tracks with milestone markers at key dates, **so that** I can understand the project's roadmap and where each workstream stands relative to the current date.

**Acceptance Criteria:**
- [ ] A left-side panel (230px wide) lists each timeline track with its ID (e.g., "M1"), name, and description, color-coded per track
- [ ] The main timeline area renders an SVG (approximately 1560×185px) with horizontal lines for each track
- [ ] Milestones render as: diamonds for PoC (`#F4B400`) and Production (`#34A853`), open circles for checkpoints (track-colored stroke), small filled circles (`#999`) for minor checkpoints
- [ ] Each milestone has a date label positioned above or below the marker
- [ ] A vertical dashed red line (`#EA4335`, stroke-dasharray `5,3`) marks the current date with a "NOW" label
- [ ] Month labels (Jan–Jun) appear along the top with vertical gridlines (`#BBB`, 0.4 opacity)
- [ ] Milestone X positions are calculated by linear interpolation of date within the configured date range
- [ ] The timeline section has a `#FAFAFA` background and a `2px #E8E8E8` bottom border
- [ ] All tracks and milestones are driven by `data.json` `timelineTracks` array
- [ ] **Visual reference:** `.tl-area` section of `OriginalDesignConcept.html`

### US-4: View Monthly Execution Heatmap

**As an** executive, **I want** a color-coded grid showing what was Shipped, what is In Progress, what carried over from the previous month, and what is Blocked — organized by month columns, **so that** I can quickly assess execution health and identify problem areas.

**Acceptance Criteria:**
- [ ] A section title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase, 14px bold, `#888` color
- [ ] The grid uses CSS Grid with columns: 160px row-header + N equal-width month columns (default 4)
- [ ] A header row shows month names in 16px bold; the current month column is highlighted with `#FFF0D0` background and `#C07700` text with a "← Now" indicator
- [ ] Four data rows appear with category-specific color schemes:
  - **Shipped:** Header `#1B7A28` on `#E8F5E9`; cells `#F0FBF0` (highlight: `#D8F2DA`); bullet `#34A853`
  - **In Progress:** Header `#1565C0` on `#E3F2FD`; cells `#EEF4FE` (highlight: `#DAE8FB`); bullet `#0078D4`
  - **Carryover:** Header `#B45309` on `#FFF8E1`; cells `#FFFDE7` (highlight: `#FFF0B0`); bullet `#F4B400`
  - **Blockers:** Header `#991B1B` on `#FEF2F2`; cells `#FFF5F5` (highlight: `#FFE4E4`); bullet `#EA4335`
- [ ] Each cell contains work items as 12px text with a 6px colored circle bullet (`::before` pseudo-element)
- [ ] Empty cells display a gray dash "-"
- [ ] Row headers are uppercase, 11px bold with 0.7px letter-spacing
- [ ] All heatmap data (columns, highlight index, rows, items) is driven by `data.json` `heatmap` object
- [ ] **Visual reference:** `.hm-wrap` and `.hm-grid` sections of `OriginalDesignConcept.html`

### US-5: Configure Dashboard via JSON File

**As a** project lead, **I want** to define all dashboard content in a single `data.json` file, **so that** I can update the report by editing one file without touching code.

**Acceptance Criteria:**
- [ ] The application reads `data.json` from a configurable path (default: project root `Data/` folder)
- [ ] The JSON schema supports: `header` (title, subtitle, backlogLink, reportDate), `timelineTracks` (array of tracks with milestones), and `heatmap` (columns, highlightColumnIndex, rows with cellItems)
- [ ] If `data.json` is missing or malformed, the dashboard displays a clear error message instead of crashing
- [ ] A `data.example.json` is provided with fictional "Project Phoenix" sample data
- [ ] Refreshing the browser (`F5`) re-reads and re-renders from the current `data.json` contents

### US-6: Capture Dashboard as Screenshot

**As a** project lead, **I want** the dashboard to render cleanly at exactly 1920×1080 with no scrollbars, spinners, or framework UI chrome, **so that** I can take a full-page browser screenshot and paste it directly into a PowerPoint slide.

**Acceptance Criteria:**
- [ ] The page renders at a fixed 1920×1080 viewport with `overflow: hidden`
- [ ] No scrollbars appear at 1920×1080
- [ ] No Blazor reconnection banners, loading indicators, or error overlays are visible during normal operation
- [ ] The SignalR error UI (`#blazor-error-ui`) is suppressed or hidden
- [ ] The page is fully rendered on first paint — no progressive loading or skeleton screens
- [ ] Screenshot via Chrome DevTools "Capture full size screenshot" produces a clean 1920×1080 PNG

## Visual Design Specification

> **Canonical design reference:** `OriginalDesignConcept.html` from the `azurenerd/ReportingDashboard` repository. Engineers MUST consult this file and the rendered screenshot (`docs/design-screenshots/OriginalDesignConcept.png`) for pixel-level fidelity.

### Overall Page Layout

- **Dimensions:** Fixed 1920×1080px, `overflow: hidden`
- **Background:** `#FFFFFF`
- **Font family:** `'Segoe UI', Arial, sans-serif`
- **Base text color:** `#111`
- **Layout model:** Flexbox column (`display: flex; flex-direction: column`) — header, timeline, and heatmap stack vertically and fill the viewport exactly

### Section 1: Header Bar (`.hdr`)

- **Height:** Auto (approximately 50px)
- **Padding:** `12px 44px 10px`
- **Border:** Bottom `1px solid #E0E0E0`
- **Layout:** Flexbox row, `justify-content: space-between; align-items: center`
- **Left side:**
  - `<h1>` — 24px, font-weight 700, contains project title + inline `<a>` link in `#0078D4`
  - Subtitle `<div class="sub">` — 12px, `#888`, margin-top 2px
- **Right side (Legend):**
  - Flexbox row with 22px gap
  - Four legend items, each 12px with inline shape:
    - PoC: 12×12px square rotated 45° (`transform: rotate(45deg)`), `#F4B400`
    - Production: 12×12px rotated square, `#34A853`
    - Checkpoint: 8×8px circle, `#999`
    - Now: 2×14px vertical bar, `#EA4335`

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed 196px
- **Background:** `#FAFAFA`
- **Padding:** `6px 44px 0`
- **Border:** Bottom `2px solid #E8E8E8`
- **Layout:** Flexbox row, `align-items: stretch`

#### Track Label Panel (left side)

- **Width:** 230px, `flex-shrink: 0`
- **Border:** Right `1px solid #E0E0E0`
- **Padding:** `16px 12px 16px 0`
- **Layout:** Flexbox column, `justify-content: space-around`
- **Track labels:** 12px, font-weight 600, line-height 1.4
  - Track ID (e.g., "M1") in track color (`#0078D4`, `#00897B`, `#546E7A`)
  - Description in font-weight 400, `#444`

#### SVG Timeline (right side, `.tl-svg-box`)

- **SVG dimensions:** Width 1560px, height 185px, `overflow: visible`
- **Month gridlines:** Vertical lines at x=0, 260, 520, 780, 1040, 1300 — stroke `#BBB`, opacity 0.4, width 1
- **Month labels:** 11px, font-weight 600, `#666`, positioned 5px right of gridline, y=14
- **"NOW" line:** Vertical dashed line at calculated X position — `#EA4335`, stroke-width 2, `stroke-dasharray: 5,3`; label 10px bold `#EA4335`
- **Track lines:** Horizontal full-width lines — stroke = track color, stroke-width 3
  - Track 1 at y≈42, Track 2 at y≈98, Track 3 at y≈154 (evenly spaced)
- **Milestone markers:**
  - **Checkpoint (open circle):** `<circle>` r=5–7, fill white, stroke = track color or `#888`, stroke-width 2.5
  - **Minor checkpoint (filled dot):** `<circle>` r=4, fill `#999`
  - **PoC (diamond):** `<polygon>` 4 points forming diamond, fill `#F4B400`, with drop shadow filter
  - **Production (diamond):** Same shape, fill `#34A853`, with drop shadow filter
- **Milestone labels:** 10px, `#666`, text-anchor middle, positioned above (y-16) or below (y+24) the track line
- **Drop shadow filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Layout:** Flexbox column, `flex: 1` (fills remaining viewport height)
- **Padding:** `10px 44px 10px`

#### Section Title (`.hm-title`)

- **Font:** 14px, font-weight 700, `#888`, letter-spacing 0.5px, uppercase
- **Margin-bottom:** 8px

#### Grid (`.hm-grid`)

- **Layout:** CSS Grid
  - Columns: `160px repeat(N, 1fr)` where N = number of months (default 4)
  - Rows: `36px repeat(4, 1fr)` — 36px header row + 4 equal data rows
- **Border:** `1px solid #E0E0E0`

#### Column Headers

- **Corner cell (`.hm-corner`):** `#F5F5F5` bg, 11px bold uppercase `#999`, centered, border-right `1px solid #E0E0E0`, border-bottom `2px solid #CCC`
- **Month headers (`.hm-col-hdr`):** `#F5F5F5` bg, 16px bold, centered, same borders
- **Highlighted month (`.apr-hdr`):** `#FFF0D0` bg, `#C07700` text

#### Row Headers (`.hm-row-hdr`)

- **Font:** 11px bold uppercase, letter-spacing 0.7px
- **Padding:** `0 12px`
- **Border:** Right `2px solid #CCC`, bottom `1px solid #E0E0E0`
- **Per-category styles:**

| Category | Text Color | Background |
|----------|-----------|------------|
| Shipped | `#1B7A28` | `#E8F5E9` |
| In Progress | `#1565C0` | `#E3F2FD` |
| Carryover | `#B45309` | `#FFF8E1` |
| Blockers | `#991B1B` | `#FEF2F2` |

#### Data Cells (`.hm-cell`)

- **Padding:** `8px 12px`
- **Borders:** Right `1px solid #E0E0E0`, bottom `1px solid #E0E0E0`
- **Overflow:** Hidden
- **Items (`.it`):** 12px, `#333`, padding `2px 0 2px 12px`, line-height 1.35
- **Bullet:** `::before` pseudo-element — 6×6px circle, positioned absolute left:0, top:7px

| Category | Cell BG | Highlighted Cell BG | Bullet Color |
|----------|---------|-------------------|-------------|
| Shipped | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

### Color Palette Summary

| Token | Hex | Usage |
|-------|-----|-------|
| White | `#FFFFFF` | Page background |
| Near-black | `#111` | Base text |
| Dark gray | `#333` | Cell item text |
| Medium gray | `#444` | Track descriptions |
| Gray | `#666` | Milestone labels, month labels |
| Light gray | `#888` | Subtitle, section titles |
| Muted gray | `#999` | Corner header text, checkpoints |
| Border gray | `#E0E0E0` | Cell borders, section dividers |
| Heavy border | `#CCC` | Row/column header separator |
| Light bg | `#FAFAFA` | Timeline background |
| Header bg | `#F5F5F5` | Grid header cells |
| Microsoft blue | `#0078D4` | Links, In Progress bullet, track M1 |
| Teal | `#00897B` | Track M2 |
| Blue-gray | `#546E7A` | Track M3 |
| Green (shipped) | `#34A853` | Production milestones, shipped bullets |
| Gold (PoC) | `#F4B400` | PoC milestones, carryover bullets |
| Red (blocker) | `#EA4335` | NOW line, blocker bullets |
| Highlight gold | `#FFF0D0` | Current month header bg |
| Highlight gold text | `#C07700` | Current month header text |

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Dashboard Renders Fully**
The user navigates to `http://localhost:5000`. The Blazor Server application reads `data.json`, deserializes it, and renders the complete dashboard in a single paint. The page displays the header with project title, subtitle, and legend; the timeline with all track lines, milestone markers, month gridlines, and the NOW indicator; and the heatmap grid with all four category rows populated. No loading spinners, skeleton screens, or progressive rendering are visible. The entire page fits within 1920×1080 with no scrollbars.

**Scenario 2: User Views the Project Header**
The user sees the project title (e.g., "Project Phoenix Release Roadmap") in 24px bold at the top-left, with a blue "→ ADO Backlog" hyperlink inline. Below the title, a subtitle shows the organizational context and report month. On the right side of the header, four legend items explain the timeline symbols. The header is visually separated from the timeline by a thin gray border.

**Scenario 3: User Reads the Milestone Timeline**
The user scans the timeline from left to right. Three horizontal track lines span the full width, each in a distinct color. Vertical month gridlines provide temporal context (Jan through Jun). Diamond markers indicate PoC (gold) and Production (green) milestones. Open and filled circles mark checkpoints. A dashed red vertical "NOW" line shows the current date. Date labels below or above each marker identify the specific milestone date and name. The left panel labels each track (M1, M2, M3) with its workstream name.

**Scenario 4: User Clicks the ADO Backlog Link**
The user clicks the "→ ADO Backlog" link in the header. A new browser tab opens navigating to the URL specified in `data.json`'s `header.backlogLink` field. The dashboard remains open and unchanged in the original tab.

**Scenario 5: User Scans the Heatmap for Execution Status**
The user looks at the heatmap grid. The current month column (e.g., April) is visually highlighted with a warm gold background in the header and slightly saturated cell backgrounds. The user reads top-to-bottom: green Shipped row shows completed items, blue In Progress row shows active work, amber Carryover row shows items that slipped from previous months, and red Blockers row shows items that are stuck. Each item has a colored bullet matching its category.

**Scenario 6: User Identifies Blocked Items**
The user focuses on the Blockers row. The red-tinted cells make it immediately obvious which months have blockers. Each blocked item is listed with a red bullet. If a month has no blockers, a gray dash is shown. The user can note these items for discussion in the executive review.

**Scenario 7: User Compares Current Month to Previous Months**
The user visually compares the highlighted "Now" column against prior months. The highlight column's warmer background tones draw the eye. The user can see if Shipped items are increasing month-over-month and if Carryover/Blockers are decreasing — key health indicators.

**Scenario 8: User Takes a Screenshot for PowerPoint**
The user opens Chrome/Edge DevTools, sets the viewport to 1920×1080 via the Device Toolbar, and uses `Ctrl+Shift+P → "Capture full size screenshot"`. The resulting PNG is a clean, complete rendering of the dashboard with no browser chrome, no scrollbars, and no Blazor framework UI elements. The user pastes this directly into a PowerPoint slide.

**Scenario 9: User Updates Report Data**
The user opens `data.json` in a text editor, modifies item text (e.g., moves an item from "In Progress" to "Shipped"), saves the file, and refreshes the browser with `F5`. The dashboard re-reads the JSON and renders the updated content immediately.

**Scenario 10: Empty State — No Items in a Cell**
A heatmap cell has no items for a given category/month combination (e.g., no Blockers in January). The cell displays a single gray dash character "-" in `#AAA` color, maintaining the grid's visual structure.

**Scenario 11: Error State — Missing or Malformed data.json**
The user starts the application without a `data.json` file or with invalid JSON. Instead of crashing or showing a Blazor error screen, the dashboard displays a single centered error message on a white background: "Unable to load dashboard data. Please check that data.json exists and contains valid JSON." The specific file path and error detail (e.g., "Unexpected character at line 15") are included below the message.

**Scenario 12: Error State — SignalR Disconnection**
If the Blazor Server SignalR connection drops (e.g., the user's machine sleeps and wakes), the default Blazor reconnection UI is suppressed. No overlay or banner appears. The user simply refreshes with `F5` to reconnect and re-render. For static SSR mode (recommended), this scenario does not apply.

## Scope

### In Scope

- Single-page Blazor Server (.NET 8) application rendering a project reporting dashboard
- Header section with configurable title, subtitle, backlog link, and report date
- SVG milestone timeline with configurable tracks, milestone types (PoC, Production, Checkpoint), and date-to-pixel positioning
- "NOW" indicator line calculated from the report date or system date
- Monthly execution heatmap grid with four status rows: Shipped, In Progress, Carryover, Blockers
- Color-coded cells with highlighted current-month column
- All data driven by a single `data.json` configuration file
- C# data model classes (`DashboardData.cs`) matching the JSON schema
- `DashboardDataService` singleton for reading and caching JSON data
- Sample `data.example.json` with fictional "Project Phoenix" data
- Fixed 1920×1080 layout optimized for screenshot capture
- Scoped CSS matching the `OriginalDesignConcept.html` design exactly
- Graceful error handling for missing or malformed `data.json`
- Suppression of Blazor framework UI chrome (reconnection banners, error overlays)
- Local-only hosting on `http://localhost:5000` via Kestrel bound to `127.0.0.1`
- Legend in header explaining timeline symbol meanings

### Out of Scope

- **Authentication & authorization** — No login, no tokens, no RBAC
- **Database** — No SQL, no SQLite, no Entity Framework
- **Cloud deployment** — No Azure, no Docker, no CI/CD pipeline
- **HTTPS** — HTTP only; no certificate management
- **Responsive design** — Fixed 1920×1080 only; no mobile, tablet, or small-screen support
- **Dark mode** — Light theme only (defer to post-MVP)
- **Multi-project support** — Single `data.json` per instance (URL-parameter-based switching is a future enhancement)
- **Auto-refresh / live reload** — No `FileSystemWatcher`; manual `F5` to reload
- **Data editing UI** — No forms; edit `data.json` directly in a text editor
- **Export to PNG/PDF** — Manual browser screenshot; no in-app export (Playwright automation is optional post-MVP)
- **Historical data / trend tracking** — No month-over-month comparison; each report is a snapshot
- **Print stylesheet** — Not included in MVP
- **Accessibility (WCAG)** — Not a requirement for this internal screenshot tool
- **Internationalization / localization** — English only
- **Unit tests, component tests, integration tests** — Optional post-MVP; not required for initial delivery
- **API endpoints** — No REST API, no GraphQL; the Blazor component reads the service directly

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** | < 1 second on localhost (first meaningful paint) |
| **JSON deserialization** | < 50ms for a `data.json` file up to 100KB |
| **SVG render** | < 100ms for up to 10 tracks × 20 milestones |
| **Memory usage** | < 100MB RSS for the running application |

### Security

| Requirement | Implementation |
|-------------|---------------|
| **Network binding** | Kestrel listens on `127.0.0.1:5000` only — not accessible on LAN or internet |
| **No authentication** | Intentionally omitted; local-only access |
| **Sensitive data** | `data.json` with real project data must NOT be committed to source control; `.gitignore` it and provide `data.example.json` |
| **No external dependencies at runtime** | No CDN links, no external API calls, no telemetry |

### Reliability

| Requirement | Target |
|-------------|--------|
| **Graceful degradation** | Missing/malformed `data.json` shows a clear error message, not a crash |
| **No runtime errors in screenshot** | The rendered page must never show Blazor error boundaries or SignalR reconnection UI |
| **Browser support** | Microsoft Edge and Google Chrome (Chromium-based); Firefox is not a target |

### Maintainability

| Requirement | Target |
|-------------|--------|
| **Total file count** | ≤ 10 application files (excluding `bin/`, `obj/`, `Properties/`) |
| **Total lines of code** | ≤ 500 lines across all `.cs` and `.razor` files |
| **Zero external NuGet packages** | MVP uses only what ships with the Blazor Server template |

## Success Metrics

1. **Visual fidelity:** A side-by-side comparison of a dashboard screenshot and the `OriginalDesignConcept.png` reference shows no visible layout differences at 1920×1080 — same spacing, colors, fonts, and grid proportions.
2. **Data-driven rendering:** Changing any value in `data.json` (title, milestone dates, heatmap items) and refreshing the browser produces a correctly updated dashboard with no code changes.
3. **Screenshot readiness:** A full-page screenshot captured via Chrome DevTools at 1920×1080 produces a clean PNG with no scrollbars, loading indicators, error banners, or browser chrome.
4. **Zero-install experience:** A user with .NET 8 SDK installed can clone the repo, run `dotnet run`, open `http://localhost:5000`, and see a fully rendered dashboard within 60 seconds.
5. **Error resilience:** Deleting `data.json` and refreshing the browser shows a user-friendly error message — not a crash, stack trace, or blank page.
6. **File simplicity:** The entire application consists of ≤ 10 source files and zero external NuGet dependencies beyond the default Blazor Server template.

## Constraints & Assumptions

### Technical Constraints

- **Framework:** .NET 8 LTS with Blazor Server (or Blazor Static SSR for the dashboard page)
- **Fixed viewport:** 1920×1080px — no responsive breakpoints
- **Font dependency:** Segoe UI must be available on the host OS (Windows); fallback to `-apple-system, Arial, sans-serif` for other platforms
- **Browser:** Chromium-based browsers only (Edge, Chrome) for screenshot fidelity
- **No JavaScript interop:** All rendering is done via Razor markup and inline SVG — no JS files
- **Local-only:** Kestrel bound to `127.0.0.1`; no network-accessible deployment

### Timeline Assumptions

- **MVP delivery:** 1–2 developer days for a fully functional, screenshot-ready dashboard
- **Polish phase:** 1 additional day for CSS fine-tuning, edge-case testing, and optional enhancements
- **No phased rollout:** This is a single-delivery project — no alpha/beta/GA stages

### Dependency Assumptions

- The user has .NET 8 SDK installed on their Windows development machine
- The user has access to Chrome or Edge for screenshot capture
- The `OriginalDesignConcept.html` file in the `azurenerd/ReportingDashboard` repository is the authoritative design reference and will not change during development
- The `data.json` schema defined in this specification is final for MVP; schema changes require a spec update
- No approval workflow is needed for dashboard content — the project lead owns their `data.json`

### Open Decisions (Require Product Owner Input)

| # | Decision | Default Assumption | Impact if Changed |
|---|----------|--------------------|-------------------|
| 1 | Number of heatmap month columns | 4 (driven by `data.json`) | CSS grid adjusts dynamically; low impact |
| 2 | Timeline date range | Jan–Jun 2026 (derived from milestone dates) | Date-to-pixel math adjusts; low impact |
| 3 | "NOW" line source | System date (`DateOnly.FromDateTime(DateTime.Now)`) | If specified in JSON, add one field; trivial |
| 4 | Multiple report files | Not supported in MVP | Adds URL routing; medium impact |