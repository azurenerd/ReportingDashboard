# PM Specification: My Project

## Executive Summary

"My Project" is a local, single-page executive status dashboard that renders a project's milestones, monthly progress, and blockers in a screenshot-ready 1920x1080 view modeled on `OriginalDesignConcept.html`. It is a lightweight Blazor Server (.NET 8) application that reads all content from a hand-edited `data.json` file, enabling a project lead to update the dashboard in seconds and paste fresh screenshots into PowerPoint decks for executive reviews. The goal is maximum visual fidelity to the reference design with the minimum possible engineering surface area — no auth, no database, no cloud.

## Business Goals

1. **Shorten executive status prep time** from hours of manual slide editing to under five minutes per update cycle.
2. **Standardize how projects are reported to executives** across the Trusted Platform org by using one visual template (milestones timeline + monthly heatmap of Shipped / In Progress / Carryover / Blockers).
3. **Deliver screenshot-perfect fidelity** to the approved `OriginalDesignConcept.html` visual so executive decks look polished and consistent.
4. **Decouple content from presentation** — the project lead maintains a simple `data.json`; designers and engineers own the layout once.
5. **Eliminate operational burden**: zero cloud cost, zero auth setup, zero production support footprint. Runs locally via `dotnet run`.
6. **Seed a reusable reporting template** that can later be extended to additional projects by dropping in a new `data.json`.
7. **Provide a fictional sample project** ("Project Aurora — Customer Portal Revamp" or similar) on first run so the author has a working example to copy.

## User Stories & Acceptance Criteria

### Story 1 — Executive consumes the dashboard screenshot
**As an** executive reviewing a program,
**I want** a single-page visual showing project milestones and the current month's shipped, in-progress, carryover, and blocker items,
**so that** I can assess project health in under 30 seconds without reading long status emails.

Acceptance Criteria:
- [ ] A single 1920x1080 page renders all four status categories plus a timeline without scrolling.
- [ ] Milestones for PoC, Production, and Checkpoint are visually distinct per the legend in `OriginalDesignConcept.html` (header section).
- [ ] The current month is visually highlighted in the heatmap (amber `.apr-hdr` column header + per-row `.apr` cell shading).
- [ ] A "NOW" marker appears on the timeline at today's date (red dashed vertical line per the timeline SVG).

### Story 2 — Project lead authors the content via `data.json`
**As a** project lead,
**I want** to edit a single JSON file to update all dashboard content,
**so that** I do not need to touch code, HTML, or a CMS to refresh my status view.

Acceptance Criteria:
- [ ] All visible text (title, subtitle, backlog URL, track labels, milestone labels, heatmap items) is sourced from `data.json`.
- [ ] The schema matches the structure documented in the research findings (title, subtitle, backlogUrl, currentDate, timeline, heatmap).
- [ ] Saving `data.json` triggers a live reload within 1 second (via `FileSystemWatcher`); no app restart required.
- [ ] Malformed JSON does not crash the app; a friendly error banner surfaces line/column of the parse failure and the last-known-good render is retained.

### Story 3 — Project lead captures a screenshot for PowerPoint
**As a** project lead,
**I want** to capture a clean 1920x1080 PNG of the dashboard,
**so that** I can paste it directly into my executive PowerPoint deck.

Acceptance Criteria:
- [ ] The page renders at exactly 1920x1080 with `overflow:hidden`; no scrollbars appear in screenshots.
- [ ] No Blazor reconnect modal, dev banner, or browser chrome appears in screenshots (hidden via CSS on `#components-reconnect-modal`).
- [ ] A `tools/screenshot.ps1` script (Playwright-based) produces `docs/screenshots/{yyyy-MM-dd}.png` in one command.
- [ ] Fonts render as Segoe UI on Windows; fallback stack is `'Segoe UI', 'Selawik', Arial, sans-serif`.

### Story 4 — Project lead views the milestone timeline
**As a** project lead,
**I want** to see each workstream (M1, M2, M3) as its own horizontal track with correctly positioned milestone diamonds and checkpoints,
**so that** executives immediately understand sequencing and upcoming big rocks.

Acceptance Criteria (references `OriginalDesignConcept.html` → **Timeline Area** `.tl-area` + `<svg>`):
- [ ] Each track renders as a colored horizontal line (`M1 #0078D4`, `M2 #00897B`, `M3 #546E7A`) across the full SVG width.
- [ ] PoC milestones render as amber (`#F4B400`) rotated-square diamonds; Production as green (`#34A853`) diamonds; Checkpoints as open or filled circles (`#999`).
- [ ] Milestone x-position is computed from `date` against `timeline.rangeStart`/`rangeEnd`; month gridlines appear at 260px intervals.
- [ ] Track labels (M1/M2/M3 plus description) appear in the 230px left gutter of the timeline area.
- [ ] A dashed red "NOW" line renders at today's date with a `NOW` label.

### Story 5 — Project lead views the monthly heatmap
**As a** project lead,
**I want** a 4-row × 4-column heatmap showing Shipped, In Progress, Carryover, and Blockers per month,
**so that** executives see a month-over-month trend at a glance.

Acceptance Criteria (references `OriginalDesignConcept.html` → **Heatmap** `.hm-wrap` / `.hm-grid`):
- [ ] Grid uses `grid-template-columns:160px repeat(4,1fr)` and `grid-template-rows:36px repeat(4,1fr)` per the reference CSS.
- [ ] Row color coding matches: Shipped (greens — `#E8F5E9`/`#F0FBF0`/`#D8F2DA`, bullet `#34A853`), In Progress (blues — `#E3F2FD`/`#EEF4FE`/`#DAE8FB`, bullet `#0078D4`), Carryover (ambers — `#FFF8E1`/`#FFFDE7`/`#FFF0B0`, bullet `#F4B400`), Blockers (reds — `#FEF2F2`/`#FFF5F5`/`#FFE4E4`, bullet `#EA4335`).
- [ ] The current month column header uses the `.apr-hdr` treatment (`#FFF0D0` background, `#C07700` text).
- [ ] Each cell renders items as 12px `#333` text with a 6px colored bullet positioned via `::before`.
- [ ] Empty cells display a single dimmed dash (`#AAA`).

### Story 6 — Project lead handles an empty/new project
**As a** project lead starting a new project,
**I want** the dashboard to gracefully render when some heatmap cells or milestone tracks are empty,
**so that** I can begin reporting from day one without cosmetic defects.

Acceptance Criteria:
- [ ] Empty heatmap cells render the dashed placeholder as in the reference.
- [ ] Tracks with zero milestones render only the horizontal line (no crash, no orphan labels).
- [ ] Sample `data.json` (fictional project) ships with the app so the dashboard is non-empty on first run.

### Story 7 — Developer prevents visual drift
**As an** engineer maintaining the dashboard,
**I want** automated tests that fail when the rendered HTML or SVG drifts from the approved design,
**so that** future changes do not silently break the screenshot fidelity.

Acceptance Criteria:
- [ ] xUnit tests cover `TimelineLayout.ComputeX` for month-start, month-end, mid-month, and leap-year Feb inputs.
- [ ] bUnit snapshot tests exist for `Heatmap.razor` and `TimelineSvg.razor` against a golden model.
- [ ] CI runs `dotnet build` and `dotnet test` on every PR.

### Story 8 — Project lead updates branding colors (future-ready)
**As a** project lead,
**I want** the timeline track colors and primary accent color to be expressible in `data.json`,
**so that** different workstreams can have distinct visual identities without code changes.

Acceptance Criteria:
- [ ] Each `tracks[].color` in `data.json` drives both the track line stroke and the left-gutter label color.
- [ ] Legend colors remain fixed (PoC amber, Production green, Checkpoint gray, NOW red) per the design file.

## Visual Design Specification

**Canonical reference:** `OriginalDesignConcept.html` (in `ReportingDashboard` repo). All engineers must open and read this file — plus the rendered screenshot `docs/design-screenshots/OriginalDesignConcept.png` — before writing code. The design is to be matched **pixel-for-pixel** at 1920x1080.

### Page Canvas
- **Viewport:** `body { width: 1920px; height: 1080px; overflow: hidden; background: #FFFFFF; }`.
- **Font stack:** `'Segoe UI', 'Selawik', Arial, sans-serif`; base text color `#111`.
- **Top-level layout:** `body` is a vertical flexbox containing, in order: Header (`.hdr`), Timeline Area (`.tl-area`), Heatmap Wrap (`.hm-wrap`).
- **Global link color:** `#0078D4`, no underline.

### Section 1 — Header (`.hdr`)
- Padding: `12px 44px 10px`; bottom border `1px solid #E0E0E0`; flex row, space-between, vertically centered; `flex-shrink:0`.
- **Left block:**
  - `h1` — 24px, weight 700 — project title (e.g., "Privacy Automation Release Roadmap") followed by an inline link "→ ADO Backlog" in `#0078D4`.
  - `.sub` — 12px, color `#888`, margin-top 2px — subtitle (e.g., "Trusted Platform · Privacy Automation Workstream · April 2026").
- **Right block — Legend (inline flex, 22px gap, 12px text):**
  - `PoC Milestone` — 12×12px `#F4B400` square rotated 45°.
  - `Production Release` — 12×12px `#34A853` square rotated 45°.
  - `Checkpoint` — 8×8px `#999` circle.
  - `Now (Apr 2026)` — 2×14px `#EA4335` vertical bar.

### Section 2 — Timeline Area (`.tl-area`)
- Flex row, `align-items:stretch`, padding `6px 44px 0`, fixed height `196px`, bottom border `2px solid #E8E8E8`, background `#FAFAFA`.
- **Left gutter (230px wide):** flex column, `justify-content:space-around`, padding `16px 12px 16px 0`, right border `1px solid #E0E0E0`. Contains one label per track:
  - `M1` — color `#0078D4`, 12px weight 600; sub-label (e.g., "Chatbot & MS Role") weight 400, color `#444`.
  - `M2` — color `#00897B`.
  - `M3` — color `#546E7A`.
- **Right SVG box (`.tl-svg-box`, `flex:1`):** contains a 1560×185 inline SVG with:
  - **Month gridlines** every 260px (Jan, Feb, Mar, Apr, May, Jun), `#bbb` @ 40% opacity, 1px; month label at top-left of each gridline (11px, weight 600, `#666`).
  - **Track baselines** — one per track at `y = 42, 98, 154` (one track every 56px), stroke 3px in the track's color.
  - **Milestones** — diamonds (`polygon`) 22×22 centered on (x,y) for PoC (`#F4B400`) and Production (`#34A853`) kinds, with a `feDropShadow` filter; date/label text centered above or below (10px, `#666`).
  - **Checkpoints** — `circle` 7px (open, white fill + track-color stroke) or 4-5px (filled `#999`).
  - **NOW line** — dashed `#EA4335`, 2px, `stroke-dasharray:5,3`, at x = today's date position; `NOW` label at top (10px, weight 700, `#EA4335`).

### Section 3 — Heatmap Wrap (`.hm-wrap`)
- Flex column, `flex:1 min-height:0`, padding `10px 44px 10px`.
- **Title (`.hm-title`):** 14px, weight 700, color `#888`, uppercase, letter-spacing 0.5px, margin-bottom 8px. Text: `Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers`.
- **Grid (`.hm-grid`):**
  - `grid-template-columns: 160px repeat(4, 1fr);`
  - `grid-template-rows: 36px repeat(4, 1fr);`
  - Outer border `1px solid #E0E0E0`.
- **Header row (row 1):**
  - `.hm-corner` (col 1) — background `#F5F5F5`, 11px weight 700 `#999`, uppercase, text `"Status"`, right border `1px solid #E0E0E0`, bottom border `2px solid #CCC`.
  - `.hm-col-hdr` (cols 2–5) — 16px weight 700, background `#F5F5F5`. The **current month** uses `.apr-hdr` modifier: background `#FFF0D0`, text `#C07700`.
- **Data rows (rows 2–5):**
  - Each row begins with `.hm-row-hdr` (160px wide, padding `0 12px`, 11px weight 700 uppercase, letter-spacing 0.7px, right border `2px solid #CCC`, bottom border `1px solid #E0E0E0`).
  - Data cells (`.hm-cell`) — padding `8px 12px`, right/bottom borders `1px solid #E0E0E0`, `overflow:hidden`.
  - Items (`.it`) — 12px `#333`, padding `2px 0 2px 12px`, line-height 1.35, with a 6px colored circular bullet at `left:0 top:7px` via `::before`.

**Row color matrix:**

| Row | Row-header class | Header bg/text | Cell bg | Current-month bg (`.apr`) | Bullet color |
|---|---|---|---|---|---|
| Shipped | `.ship-hdr` | `#E8F5E9` / `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `.prog-hdr` | `#E3F2FD` / `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `.carry-hdr` | `#FFF8E1` / `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `.block-hdr` | `#FEF2F2` / `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

### Component Hierarchy
```
Index.razor
├── DashboardHeader.razor        (Section 1 — title, subtitle, legend)
├── TimelineSvg.razor            (Section 2 — left gutter + inline SVG)
│     └── (uses TimelineLayout.ComputeX for milestone positions)
└── Heatmap.razor                (Section 3 — CSS Grid, 4×4 + headers)
```

## UI Interaction Scenarios

**Scenario 1 — Initial page load (happy path):** User navigates to `http://localhost:5000/`. Within 500 ms the full 1920x1080 dashboard is visible: header with title + "→ ADO Backlog" link + legend; timeline area with M1/M2/M3 tracks, milestone diamonds, checkpoints, and a red dashed NOW line at today's date; heatmap with 4 rows × 4 month columns, the current month column highlighted amber, items bulleted in category color. No loading spinner, no empty frames.

**Scenario 2 — Project lead edits `data.json` and saves:** The `FileSystemWatcher` fires, `DashboardDataService.Reload()` re-parses the file, raises `OnChanged`, and `Index.razor` calls `StateHasChanged()`. Within ~1 second (after 250 ms debounce) the browser DOM diff-updates in place — new milestones slide to new positions, heatmap cells repopulate, current-month highlight shifts if `currentMonthIndex` changed. No page refresh, no flicker of the whole page.

**Scenario 3 — Project lead introduces malformed JSON:** The watcher fires, parse throws `JsonException`. Service keeps the last-known-good model in memory and sets an `ErrorState` with line/column + message. `Index.razor` renders a small top-anchored banner ("data.json parse error at line 42, column 7: Expected ',' but got ']'") while the previously valid dashboard continues to render underneath. When the file is fixed and re-saved, the banner disappears.

**Scenario 4 — Current-month highlight shifts month-over-month:** Author changes `heatmap.currentMonthIndex` from 2 to 3. On reload, the amber `.apr-hdr` treatment moves from March to April column; the `.apr` cell-shade modifier moves correspondingly in all four rows. No code changes required.

**Scenario 5 — Milestone falls exactly on a month boundary:** `TimelineLayout.ComputeX` returns an x-coordinate aligned with the month gridline (e.g., Apr 1 → x = 780). The milestone diamond centers precisely on the gridline; the date label remains legible and does not collide with the month label at the top of the column.

**Scenario 6 — Empty heatmap cell:** A cell with no items renders a single dashed placeholder (`—` in `#AAA`) so the grid row retains a consistent visual weight rather than collapsing.

**Scenario 7 — Track with zero milestones:** The track still renders its horizontal baseline (e.g., `M3` gray line) and its left-gutter label; no diamonds or circles are drawn. The layout does not shift.

**Scenario 8 — Overflow item in a heatmap cell:** If the author puts too many items in one cell, `overflow:hidden` on `.hm-cell` clips the extra items. No ellipsis is added (acceptable for screenshots; author is expected to keep cells terse). A guidance note in the README recommends ≤ 4 items per cell for April-column rows and ≤ 2 for others.

**Scenario 9 — Hover on milestone diamond (basic, non-essential):** Native browser tooltip shows the milestone label from the SVG `<title>` child element. No custom tooltip component is required because the artifact is a screenshot.

**Scenario 10 — Click on "→ ADO Backlog" link:** Opens `backlogUrl` from `data.json` in the same tab (or `_blank` if we choose). This is a convenience for the author, not an executive feature.

**Scenario 11 — Blazor Server reconnect event:** If the SignalR circuit drops and reconnects, the built-in `#components-reconnect-modal` is hidden via CSS (`display:none`) so that it never appears in screenshots. On reconnect, the page re-renders from server state with no visible transition.

**Scenario 12 — Screenshot capture (`tools/screenshot.ps1`):** Script launches headless Chromium at exactly 1920x1080, navigates to `http://localhost:5000/`, waits for `networkidle`, then calls `page.screenshot()` saving to `docs/screenshots/{yyyy-MM-dd}.png`. Exit code is 0 on success.

**Scenario 13 — Non-1920 window (author runs the app and just opens it in a browser):** Page still renders at fixed 1920×1080 (viewport-sized, not responsive). If the user's window is smaller, horizontal/vertical scroll occurs on the browser viewport but the layout itself does not reflow. README instructs users to set browser zoom to 100% and window width ≥ 1920px for screenshotting.

**Scenario 14 — File missing at startup:** If `data.json` is absent, the app boots with a banner: "`data.json` not found at `<path>`. Create one based on `data.sample.json`." A sample file is shipped alongside the app.

**Scenario 15 — Multiple rapid saves (debounce):** `FileSystemWatcher` often fires twice for a single save on Windows. A 250 ms debounce in `DashboardDataService` coalesces events into one reload.

## Scope

### In Scope
- Single-page Blazor Server (.NET 8) web app rendered at `http://localhost:5000/`.
- `data.json` as the only content source, hot-reloaded via `FileSystemWatcher` (debounced).
- Three visual sections matching `OriginalDesignConcept.html`: Header, multi-track SVG Timeline, 4×4 Heatmap.
- Strongly-typed POCO model (`DashboardModel`, `MilestoneTrack`, `Milestone`, `HeatmapItem`) via `System.Text.Json`.
- Pure-C# `TimelineLayout.ComputeX` helper with unit tests for date→pixel math.
- CSS ported verbatim from the reference `<style>` block into `wwwroot/css/site.css`.
- Friendly JSON parse-error banner with line/column, preserving the last-known-good render.
- Fictional sample `data.json` (e.g., "Project Aurora — Customer Portal Revamp") shipped with the app.
- CSS rule hiding `#components-reconnect-modal` so screenshots are clean.
- xUnit + bUnit + FluentAssertions test project with snapshot tests for `Heatmap` and `TimelineSvg`.
- Optional Playwright-based `tools/screenshot.ps1` to capture 1920×1080 PNGs.
- README documenting prerequisites, `dotnet run`, and screenshot workflow.

### Out of Scope
- Authentication, authorization, identity, SSO, or any user login.
- HTTPS, certificate provisioning, production hosting, Docker, Kubernetes, IIS, App Service.
- Database, ORM (Entity Framework), migrations, or any persistence beyond `data.json`.
- In-app editing UI, forms, CRUD endpoints, admin console.
- Multi-project selection / dropdown loader (single `data.json` per deployment).
- PDF export, email digests, scheduled reports, webhook integrations.
- Real-time data ingestion from ADO, Jira, GitHub, or any external system.
- Mobile or responsive layouts; tablet/phone rendering; dark mode; theming beyond track colors.
- Component libraries (MudBlazor, Radzen, Blazorise), charting libraries (ChartJs.Blazor, Plotly), or JS frameworks (HTMX, Alpine).
- Telemetry, App Insights, OpenTelemetry, analytics.
- Internationalization / localization; the app is English-only.
- Accessibility beyond semantic HTML defaults (this is a screenshot tool, not an interactive product).
- Historical snapshot browsing (`/history/YYYY-MM.json`) — deferred.

## Non-Functional Requirements

**Performance**
- First-paint of `/` within **500 ms** on the authoring machine (localhost, warm process).
- `data.json` hot-reload completes and UI updates within **1 second** of file save (250 ms debounce + parse + render).
- Page must render fully within **3 seconds** in a headless Playwright session (for screenshot automation).

**Visual fidelity**
- The rendered page must be visually indistinguishable from `OriginalDesignConcept.html` at 1920×1080 to a reviewing executive (per-pixel parity not required; layout, colors, typography, and spacing must match exactly).
- All hex colors, font sizes, paddings, and grid/flex rules from the reference `<style>` block must be reproduced verbatim in `site.css`.

**Security**
- No authentication by design. The app binds only to `http://localhost:5000` (never `0.0.0.0`) and is intended for single-user local use.
- No secrets, PII, or confidential data stored by the app; `data.json` contents are the author's responsibility.
- HTTPS redirect middleware disabled to avoid certificate prompts locally.
- Default Blazor antiforgery protections remain enabled (no forms, but defaults stay on).

**Scalability**
- Single user, single process. No scale-out requirements. SignalR runs in-process with default capacity.

**Reliability**
- Malformed `data.json` must never crash the app; the process must keep serving the last-known-good render and show a parse-error banner.
- App must survive `FileSystemWatcher` double-fire events on Windows via debouncing.

**Maintainability**
- Zero external service dependencies. `dotnet build` + `dotnet test` must succeed offline.
- Code formatted with `dotnet format`; `.editorconfig` present; Microsoft.CodeAnalysis.NetAnalyzers enabled.

**Observability**
- Console logging at `Information` level; structured messages for data reload events and parse failures. No cloud telemetry.

**Portability**
- Target Windows 10/11 with .NET 8 SDK installed. Segoe UI assumed present. Font fallback stack `'Segoe UI', 'Selawik', Arial, sans-serif` documented for non-Windows experimentation.

## Success Metrics

1. **Design parity sign-off:** Side-by-side comparison of the rendered app screenshot and `OriginalDesignConcept.png` is approved by the project lead and the executive sponsor in Phase 1.
2. **Time-to-update:** Project lead can update `data.json`, visually verify, and capture a PowerPoint-ready PNG in **≤ 5 minutes** end-to-end.
3. **Zero-crash reliability:** 30 consecutive edits to `data.json` (including intentionally malformed payloads) result in zero process crashes and always produce either a valid render or a parse-error banner.
4. **Test coverage:** `TimelineLayout.ComputeX` has ≥ 90% branch coverage; bUnit snapshot tests exist for both `Heatmap` and `TimelineSvg`; CI green on every PR.
5. **Screenshot automation:** `tools/screenshot.ps1` produces a 1920×1080 PNG in under 10 seconds on the authoring machine.
6. **Adoption:** At least one executive deck per month uses a screenshot from this tool within the first 90 days post-launch.
7. **Footprint:** Total source code ≤ 1,500 lines (excluding tests) and published self-contained exe ≤ 100 MB.

## Constraints & Assumptions

**Technical constraints**
- Stack is fixed: Blazor Server on .NET 8 (LTS through Nov 2026), Kestrel, System.Text.Json, vanilla CSS + inline SVG. No component or charting libraries permitted (enforced by ADR `docs/adr/0001-no-component-libraries.md`).
- Rendering viewport is fixed at 1920×1080; the app is not responsive.
- Windows-only authoring/screenshotting target; Segoe UI is assumed available.
- Local-only binding (`localhost:5000`); no network exposure.
- No database; all state lives in `data.json` plus in-memory last-known-good.

**Timeline assumptions**
- Phase 0 (scaffolding) — ½ day.
- Phase 1 (static render pixel-match) — 1 day; serves as the exec sign-off milestone.
- Phase 2 (JSON loader + hot reload) — ½ day.
- Phase 3 (robustness, tests, reconnect-modal suppression) — ½ day.
- Phase 4 (Playwright screenshot script) — ½ day, optional.
- Total: ~3 engineer-days to production-ready.

**Dependency assumptions**
- The authoring machine has .NET 8 SDK installed (or runs the self-contained published exe).
- `OriginalDesignConcept.html` in the ReportingDashboard repo is the canonical design source of truth; any future visual change requires updating both that file and this spec.
- Project lead is responsible for the accuracy of `data.json` contents (items, dates, track labels). The app does not validate business correctness — only JSON schema conformance.
- No external API integrations are assumed; milestones and heatmap items are hand-authored.
- The fictional sample project ("Project Aurora — Customer Portal Revamp" or equivalent) will be authored during Phase 2 and ships with the repo as the default `data.json` so the app is demonstrable out of the box.
- All implementing agents/engineers will read `OriginalDesignConcept.html` and view `docs/design-screenshots/OriginalDesignConcept.png` **before** writing any code or creating an implementation plan, as explicitly mandated by the project owner.