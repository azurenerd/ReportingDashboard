# PM Specification: My Project

## Executive Summary

We are building a **single-page executive reporting dashboard** that visualizes a project's health, milestones, and monthly execution status in a format optimized for screenshotting into PowerPoint decks. The page reads a local `data.json` file and renders a fictional project ("Privacy Automation Release Roadmap") using a header, a milestone Gantt timeline with swimlanes, and a monthly execution heatmap covering Shipped, In Progress, Carryover, and Blockers. The product's primary value is giving executives a clear, one-glance view of "what shipped, what's in flight, what slipped, and what's next" without any enterprise overhead.

## Business Goals

1. **Accelerate executive communication** — produce a screenshot-ready status visual in under 2 minutes whenever leadership asks "where are we?"
2. **Create a single source of truth** for project status that lives in a version-controllable `data.json` file editable by the project owner without engineering help.
3. **Standardize the visual language** of project reporting (PoC diamond, Prod diamond, Checkpoint circle, NOW line, Shipped/InProgress/Carryover/Blocker rows) so execs learn the format once and apply it across projects.
4. **Eliminate tooling friction** — no auth, no cloud, no database, no build pipeline; a developer can `dotnet run` locally and immediately screenshot.
5. **Preserve pixel-perfect design fidelity** to `OriginalDesignConcept.html` so the output slides cleanly into existing PowerPoint deck templates at 1920×1080.
6. **Enable rapid last-minute edits** — editing `data.json` and refreshing the browser updates the dashboard with no rebuild required.
7. **Demonstrate the pattern** on a fictional but realistic project ("Privacy Automation Release Roadmap") so new adopters see the full visual vocabulary on first run.

## User Stories & Acceptance Criteria

### Story 1 — Launch the dashboard locally
**As a** project lead, **I want** to start the dashboard with a single `dotnet run` command, **so that** I can produce an executive-ready visual without any deployment effort.
- [ ] Running `dotnet run` from the solution root starts Kestrel on `http://localhost:5000`.
- [ ] The app binds to `127.0.0.1` only (not reachable from the LAN).
- [ ] No HTTPS, no auth, no login page — navigating to `/` renders the dashboard immediately.
- [ ] Startup time under 5 seconds on a typical dev laptop.
- [ ] If `data.json` is missing or malformed, a clear error page identifies the file and line.

### Story 2 — View the full project header
**As an** executive viewer, **I want** to see the project title, subtitle/org context, a backlog link, and a milestone legend at the top of the page, **so that** I immediately know what project I'm looking at and what the symbols mean.
- [ ] Header matches the **Header section** of `OriginalDesignConcept.html`.
- [ ] Title rendered at 24px / weight 700 (e.g., "Privacy Automation Release Roadmap").
- [ ] Subtitle rendered at 12px / `#888` (e.g., "Trusted Platform · Privacy Automation Workstream · April 2026").
- [ ] Backlog link rendered in `#0078D4` immediately after the title; href comes from `data.json`.
- [ ] Legend on the right shows: yellow diamond = PoC Milestone, green diamond = Production Release, gray circle = Checkpoint, red vertical bar = Now.
- [ ] Header has a 1px `#E0E0E0` bottom border.

### Story 3 — View the milestone timeline (Gantt)
**As an** executive viewer, **I want** to see each workstream as a horizontal swimlane with milestone glyphs plotted on a shared month axis, **so that** I can understand sequencing and slippage at a glance.
- [ ] Timeline matches the **Timeline/Gantt section** of `OriginalDesignConcept.html`.
- [ ] Left 230px column lists swimlanes (M1/M2/M3) with an ID line (colored, 12px/600) and a label line (`#444`, 12px/400).
- [ ] Right SVG area (width 1560, height 185) renders month gridlines and labels ("Jan"…"Jun") at `#666` 11px/600.
- [ ] Each swimlane renders as a horizontal line in its lane color (M1 `#0078D4`, M2 `#00897B`, M3 `#546E7A`).
- [ ] PoC milestones render as `#F4B400` diamonds; Prod as `#34A853` diamonds; Checkpoints as white-filled circles with the swimlane's stroke color (major) or solid `#999` filled circles (minor).
- [ ] A red dashed vertical line at `#EA4335` stroke-width 2 with label "NOW" marks `nowDate` from `data.json`.
- [ ] Milestone x-coordinates are computed linearly from `startDate`→`endDate`, not hardcoded.
- [ ] Each milestone has a small text label (date + type) positioned above or below the glyph at `#666` 10px.

### Story 4 — View the monthly execution heatmap
**As an** executive viewer, **I want** to see a four-row × N-month grid of Shipped / In Progress / Carryover / Blockers items, **so that** I can see what happened each month and where the project stands this month.
- [ ] Heatmap matches the **Heatmap grid** section of `OriginalDesignConcept.html`.
- [ ] Grid uses `grid-template-columns:160px repeat(N,1fr)` where N = `heatmap.months.length` (default 4).
- [ ] Five rows: header row (36px) + 4 equal data rows.
- [ ] Column header cells at 16px/700 on `#F5F5F5`; current month uses `apr-hdr` style (`#FFF0D0` bg, `#C07700` text).
- [ ] Row headers: "● Shipped" (`#1B7A28` on `#E8F5E9`), "▶ In Progress" (`#1565C0` on `#E3F2FD`), "● Carryover" (`#B45309` on `#FFF8E1`), "● Blockers" (`#991B1B` on `#FEF2F2`).
- [ ] Data cells use row-themed tints; current-month column uses the darker `.apr` variant.
- [ ] Each item line uses a 6px colored bullet (`::before`) in the row's accent color and 12px `#333` text.
- [ ] Heatmap title "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" at 14px/700 `#888`, uppercase, `.5px` letter-spacing.

### Story 5 — Drive all content from `data.json`
**As a** project lead, **I want** to edit a single JSON file to change the project name, dates, swimlanes, milestones, and heatmap items, **so that** I can update the dashboard without any code changes.
- [ ] All on-screen text (except legend labels) originates from `data.json`.
- [ ] Schema includes `project`, `timeline` (with `startDate`, `endDate`, `nowDate`, `swimlanes[]`), and `heatmap` (with `months[]`, `currentMonthIndex`, `rows.{shipped,inProgress,carryover,blockers}`).
- [ ] Saving `data.json` while the app is running causes the rendered page to reflect the change on next browser refresh (hot-reload via `IOptionsMonitor<T>`).
- [ ] A ship-ready sample `data.json` is included that exercises all 4 heatmap rows and all 3 milestone types.

### Story 6 — Screenshot for PowerPoint
**As a** project lead, **I want** the page to render exactly at 1920×1080 with no scrollbars, **so that** a window screenshot drops cleanly into a slide.
- [ ] `body` is fixed to 1920×1080, `overflow:hidden`.
- [ ] Font stack `'Segoe UI', Arial, sans-serif`.
- [ ] At a 1920×1080 browser viewport, nothing is clipped and no scrollbars appear.
- [ ] Visual diff against `OriginalDesignConcept.png` shows no structural drift (spot-check of spacing, colors, fonts).

### Story 7 — Graceful failure on bad data
**As a** project lead, **I want** clear feedback when `data.json` is broken, **so that** I don't ship a half-rendered screenshot.
- [ ] Missing `data.json` → error page naming the missing file.
- [ ] Malformed JSON → error page showing parser message.
- [ ] Milestone dates outside `[startDate, endDate]` render clipped to the edge and emit a console warning.

## Visual Design Specification

All visuals MUST match `OriginalDesignConcept.html` and the rendered `OriginalDesignConcept.png` at 1920×1080. The page is a single fixed-size document composed of three stacked sections inside a flex-column body.

### Global
- **Viewport:** `body { width:1920px; height:1080px; overflow:hidden; background:#FFFFFF; color:#111; display:flex; flex-direction:column; }`
- **Font:** `'Segoe UI', Arial, sans-serif` throughout.
- **Link color:** `#0078D4`, no underline.
- **Reset:** `* { margin:0; padding:0; box-sizing:border-box; }`

### Section 1 — Header (`.hdr`)
- Padding `12px 44px 10px`, 1px `#E0E0E0` bottom border, `display:flex`, `justify-content:space-between`, `align-items:center`, `flex-shrink:0`.
- **Left block:**
  - `<h1>` at 24px/700 containing the project title followed by an inline backlog link (`#0078D4`).
  - `.sub` subtitle at 12px `#888`, `margin-top:2px`.
- **Right block (legend):** flex row, `gap:22px`, `align-items:center`. Each legend item is a flex row with `gap:6px` and 12px text:
  - Yellow `#F4B400` 12×12px square rotated 45° → "PoC Milestone"
  - Green `#34A853` 12×12px square rotated 45° → "Production Release"
  - Gray `#999` 8×8px circle → "Checkpoint"
  - Red `#EA4335` 2×14px bar → "Now (Apr 2026)"

### Section 2 — Timeline area (`.tl-area`)
- `display:flex; align-items:stretch; padding:6px 44px 0; height:196px; border-bottom:2px solid #E8E8E8; background:#FAFAFA; flex-shrink:0;`.
- **Left swimlane legend:** 230px wide, `display:flex; flex-direction:column; justify-content:space-around; padding:16px 12px 16px 0; border-right:1px solid #E0E0E0;`. Each swimlane entry: 12px/600 colored ID ("M1" `#0078D4`, "M2" `#00897B`, "M3" `#546E7A`) over a 12px/400 `#444` description.
- **SVG canvas:** `flex:1; padding-left:12px; padding-top:6px;`. SVG is `width=1560 height=185 overflow:visible`.
  - Month gridlines: vertical 1px `#bbb` @ 0.4 opacity at every month boundary, with month label at 11px/600 `#666` Segoe UI offset +5px right.
  - NOW line: 2px `#EA4335` `stroke-dasharray:5,3` at `DateToX(nowDate)`; "NOW" text label 10px/700 `#EA4335` at x+4.
  - Swimlane rows at y = 42, 98, 154. Horizontal 3px line in the swimlane color (`#0078D4`, `#00897B`, `#546E7A`) across the full 1560 width.
  - **PoC glyph:** 22×22 diamond (rotated square) fill `#F4B400`, with `feDropShadow` filter.
  - **Prod glyph:** 22×22 diamond fill `#34A853`, same shadow.
  - **Major checkpoint:** white-filled circle r=7 with stroke in the swimlane color, stroke-width 2.5.
  - **Minor checkpoint:** solid `#999` circle r=4 (or r=5 with white fill + `#888` stroke 2.5 for named checkpoints).
  - Milestone text labels: 10px `#666` Segoe UI, `text-anchor:middle`, placed above (y-16) or below (y+24) each glyph to avoid overlap.

### Section 3 — Heatmap (`.hm-wrap`)
- `flex:1; display:flex; flex-direction:column; padding:10px 44px 10px;`.
- **Title bar:** `.hm-title` 14px/700 `#888` uppercase `.5px` letter-spacing, `margin-bottom:8px`.
- **Grid:** `display:grid; grid-template-columns:160px repeat(4,1fr); grid-template-rows:36px repeat(4,1fr); border:1px solid #E0E0E0;`.
- **Header row cells:**
  - `.hm-corner` (top-left): `#F5F5F5` bg, 11px/700 `#999` uppercase, right+bottom borders.
  - `.hm-col-hdr` (month headers): 16px/700 on `#F5F5F5`; current-month variant `.apr-hdr` uses `#FFF0D0` bg / `#C07700` text.
  - All header cells have `border-right:1px solid #E0E0E0; border-bottom:2px solid #CCC`.
- **Row headers (`.hm-row-hdr`):** `padding:0 12px`, 11px/700 uppercase, `.7px` letter-spacing, `border-right:2px solid #CCC`.
  - Shipped: `#1B7A28` on `#E8F5E9` (`.ship-hdr`)
  - In Progress: `#1565C0` on `#E3F2FD` (`.prog-hdr`)
  - Carryover: `#B45309` on `#FFF8E1` (`.carry-hdr`)
  - Blockers: `#991B1B` on `#FEF2F2` (`.block-hdr`)
- **Data cells (`.hm-cell`):** `padding:8px 12px; overflow:hidden; border-right/bottom:1px solid #E0E0E0`. Per-row background tints + darker current-month variant:
  - Shipped: `#F0FBF0` / `#D8F2DA` (apr); bullet `#34A853`.
  - In Progress: `#EEF4FE` / `#DAE8FB` (apr); bullet `#0078D4`.
  - Carryover: `#FFFDE7` / `#FFF0B0` (apr); bullet `#F4B400`.
  - Blockers: `#FFF5F5` / `#FFE4E4` (apr); bullet `#EA4335`.
- **Item line (`.it`):** 12px `#333`, `padding:2px 0 2px 12px`, `line-height:1.35`; `::before` 6×6px circular bullet absolutely positioned `left:0; top:7px`.

### Component Hierarchy
```
Dashboard.razor
├── Header.razor         (title, subtitle, backlog link, legend)
├── TimelineSvg.razor    (swimlane legend column + inline SVG)
│     └── MilestoneLayout helpers (DateToX)
└── HeatmapGrid.razor
      └── HeatmapCell.razor (× 4 rows × N months)
```

## UI Interaction Scenarios

**Scenario 1 — Initial page load:** User navigates to `http://localhost:5000/`. The header, timeline, and heatmap render in a single server-side pass. No spinners, no progressive loading. Total time to first contentful paint < 500ms on localhost.

**Scenario 2 — Screenshot capture:** User sizes the browser to exactly 1920×1080 (or uses DevTools device emulation), hits `Win+Shift+S` or browser print-to-PDF, and captures the whole viewport. No scrollbars appear; no element is clipped.

**Scenario 3 — Data-driven header rendering:** `data.json` contains `project.title = "Privacy Automation Release Roadmap"`, `project.subtitle = "Trusted Platform · Privacy Automation Workstream · April 2026"`, `project.backlogUrl = "https://ado.../backlog"`. The header reflects all three; the backlog link is clickable and opens the URL.

**Scenario 4 — Timeline date mapping:** `data.json` sets `timeline.startDate=2026-01-01`, `endDate=2026-06-30`, `nowDate=2026-04-19`. The M1 PoC milestone at `2026-03-26` is rendered at x ≈ 745 of the 1560-wide SVG; the NOW line appears at x ≈ 823. Month labels Jan/Feb/Mar/Apr/May/Jun appear at 0/260/520/780/1040/1300.

**Scenario 5 — Current-month highlighting:** `heatmap.currentMonthIndex=3` (April). The April column header adopts the `.apr-hdr` warm amber styling; all four data cells in that column use the darker `.apr` row-tint. Other columns remain in their base tint.

**Scenario 6 — Multi-item heatmap cell:** A cell receives `["API v2 shipped", "Telemetry dashboard live"]`. Both items render as stacked bullet lines inside the cell; text wraps at the cell width, overflow hidden clips anything past the bottom.

**Scenario 7 — Empty cell:** A heatmap cell receives `[]`. The cell renders a single dim dash (`—`) at `#AAA` per the reference CSS, preserving grid alignment.

**Scenario 8 — Swimlane with mixed milestones:** M1 has a Jan 12 checkpoint, a Mar 26 PoC diamond, and an Apr 15 Prod diamond. All three render on the same horizontal line, with labels positioned alternately above/below to avoid collision. Diamonds are drawn with the drop-shadow filter; the checkpoint is a white-filled ring stroked in `#0078D4`.

**Scenario 9 — Hot reload on `data.json` save:** User edits `data.json` (e.g., adds a new Shipped item), saves the file. On the next browser refresh (F5), the new item appears. No app restart required. (Live push via SignalR is *not* guaranteed in MVP.)

**Scenario 10 — Malformed `data.json`:** User introduces a syntax error. Browser shows a developer error page naming `data.json` and the parser error; the dashboard does not render partial state.

**Scenario 11 — Missing `data.json`:** App starts but `data.json` is not present. Startup fails fast with a logged error and an error page stating "data.json not found next to executable."

**Scenario 12 — Hover/click (non-interactive):** The page has no interactive behaviors. Hover states are not required; click on the backlog link is the only navigation. This is intentional — the product is a screenshot, not an app.

**Scenario 13 — Narrow viewport:** If the user opens the page below 1920px wide, content is clipped to the right (by design). No responsive layout is attempted. README instructs users to screenshot at 1920×1080.

**Scenario 14 — Milestone out of range:** A milestone with `date` before `startDate` is clamped to x=0 and a console warning is logged. A milestone after `endDate` is clamped to x=1560 and warned. The page still renders.

**Scenario 15 — Variable swimlane count:** `data.json` defines 2 swimlanes instead of 3. The left legend and SVG auto-distribute vertical space (y positions recalculated as `42 + i * ((185-42)/max(1,N-1))`), preserving visual balance.

## Scope

### In Scope
- Single-page Blazor Server (.NET 8) web app, one `.sln`.
- Renders at fixed 1920×1080 to match `OriginalDesignConcept.html`.
- Reads a local `data.json` describing project metadata, timeline swimlanes/milestones, and monthly heatmap rows.
- Header with title, subtitle, backlog link, and legend.
- Timeline section with left swimlane legend and right inline-SVG Gantt (month gridlines, NOW line, swimlane lines, PoC/Prod diamonds, Checkpoint circles, per-milestone labels).
- Heatmap section with 4 rows (Shipped, In Progress, Carryover, Blockers) × N months (default 4), with current-month column highlight and themed row colors.
- Sample `data.json` for a fictional "Privacy Automation Release Roadmap" exercising all glyph and row types.
- Hot-reload of `data.json` via `IOptionsMonitor<T>` (refresh browser to see changes).
- Minimal automated tests: render-smoke, malformed-JSON handling, `DateToX` math unit test.
- README with run/screenshot instructions.

### Out of Scope
- Authentication, authorization, user accounts, session state.
- HTTPS, CSP headers, enterprise security review.
- Database, EF Core, SQLite, or any persistent store beyond `data.json`.
- Docker, Kubernetes, Azure App Service, any cloud hosting.
- CI/CD beyond an optional `dotnet build` + `dotnet test` GitHub Action.
- Responsive/mobile/tablet layouts; only 1920×1080 is supported.
- Accessibility (WCAG) compliance targets.
- Third-party charting libraries (Chart.js, Plotly, ApexCharts, LiveCharts2).
- Third-party component libraries (MudBlazor, Radzen, Blazorise).
- JavaScript/JS-interop code.
- Interactive filtering, drill-down, tooltips, or drag-to-edit.
- Multi-project selector or project switcher.
- Historical snapshots or time-travel of past dashboard states.
- Automated PNG export (Playwright) — deferred to a later phase.
- Print-specific CSS — deferred.
- Real PII or customer data in `data.json`.

## Non-Functional Requirements

- **Performance:** Time to first contentful paint ≤ 500ms on localhost; full render (including SVG) ≤ 1s. Cold start ≤ 5s.
- **Fidelity:** Visual diff vs. `OriginalDesignConcept.png` shows no structural drift in layout, colors (exact hex match), or typography (Segoe UI, correct sizes/weights).
- **Security:** Kestrel bound to `127.0.0.1` only; no inbound ports opened to the LAN. No HTTPS, no auth required. `data.json` must contain only fictional/non-sensitive data (documented in README).
- **Reliability:** No SLA. Single-user local tool. Malformed `data.json` produces a clear error, not a crash loop.
- **Scalability:** Not applicable. Single user, single page, single project.
- **Portability:** Runs on Windows 10/11 with .NET 8 SDK installed. macOS/Linux supported functionally; screenshot fidelity targeted on Windows (Segoe UI availability).
- **Maintainability:** Total code footprint ≤ 900 lines. No JS. All styling via CSS copied verbatim from `OriginalDesignConcept.html`.
- **Observability:** Console logging via default `ILogger`. No telemetry, no App Insights.
- **Cost:** $0. No cloud, no licenses.

## Success Metrics

1. **Pixel parity:** Side-by-side comparison of the rendered page vs. `OriginalDesignConcept.png` at 1920×1080 passes a human review with no structural differences.
2. **Time-to-screenshot:** A user who has cloned the repo and installed .NET 8 can go from `git clone` to a pasted PowerPoint screenshot in ≤ 5 minutes.
3. **Data editability:** A non-developer can modify `data.json` (change title, add a milestone, add a heatmap item) and see the change in the browser after refresh, without engineering intervention.
4. **Zero-JS:** Final build emits no custom JavaScript (only Blazor framework JS is present).
5. **Tests pass:** `dotnet test` runs green on the three targeted tests (render-smoke, bad-JSON, `DateToX` math).
6. **Sample coverage:** Shipped `data.json` exercises all 3 milestone types (PoC/Prod/Checkpoint) and all 4 heatmap rows (Shipped/InProgress/Carryover/Blockers) so first-run users see the full visual vocabulary.
7. **Adoption indicator:** At least one executive deck uses a screenshot of this dashboard within 2 weeks of MVP delivery.

## Constraints & Assumptions

### Technical Constraints
- **Stack mandate:** C# .NET 8 + Blazor Server, single `.sln`. Static SSR rendering is preferred; Interactive Server is acceptable.
- **Local-only:** App must run via `dotnet run` on the author's machine. No cloud, no container.
- **No JavaScript:** All rendering via Razor + inline SVG + CSS.
- **No charting library:** Timeline is hand-authored SVG; heatmap is CSS Grid.
- **No component library:** No MudBlazor / Radzen / Blazorise.
- **Fixed viewport:** 1920×1080 only. No responsive breakpoints.
- **Font:** Segoe UI with Arial / sans-serif fallback; Segoe UI availability assumed on Windows screenshot hosts.

### Timeline Assumptions
- **MVP target:** 1–2 engineering days to reach a screenshot-ready page.
- **Phase 2 (polish — parameterized months, extra milestone types, `FileSystemWatcher` fallback):** additional 0.5–1 day, if needed.
- **Phase 3+ (Playwright export, multi-project selector):** deferred.

### Dependency Assumptions
- Developer has .NET 8 SDK installed.
- Developer has a modern Chromium-based browser for screenshotting at 1920×1080.
- `OriginalDesignConcept.html` and `OriginalDesignConcept.png` are available as the canonical visual references; implementations must consult them before coding.
- `data.json` is the sole content source; all on-screen strings (except static legend labels) come from it.
- No network connectivity is required at runtime.
- The project is a demo/reporting tool, not a shipping product; accessibility, i18n, and multi-tenant concerns are explicitly deferred.

### Open Items Resolved for MVP (defaults)
- **Render mode:** Static SSR (recommended). Flip to Interactive Server only if requested.
- **Month count:** Configurable via `data.json` (`heatmap.months`), default 4.
- **Milestone types:** `poc`, `prod`, `checkpoint` only for MVP.
- **Swimlane count:** Variable, driven by `data.json`; default 3.
- **Heatmap items:** Plain labels, no counts/metrics.
- **Backlog link:** Configurable URL per project (`project.backlogUrl`); omit rendering if empty.
- **Publish target:** `dotnet run` on author's laptop; self-contained `.exe` deferred.
- **Multiple projects:** Single-project only; each project gets its own `data.json`.