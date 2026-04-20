# PM Specification: My Project

## Executive Summary

"My Project" is a single-page, screenshot-optimized executive reporting dashboard that renders a fixed 1920×1080 view of a project's milestone timeline and monthly execution status (Shipped / In Progress / Carryover / Blockers). Built as a minimal local-only Blazor Server (.NET 8) app driven entirely by a hand-edited `data.json` file, it exists so a PM can take a clean screenshot and paste it directly into an executive PowerPoint deck — no BI tool, no auth, no cloud, no interactivity.

## Business Goals

1. **Give executives one-glance visibility** into project status (what shipped, what's in flight, what slipped, what's blocked) without reading status emails.
2. **Eliminate manual PowerPoint chart authoring** for the PM by producing a deck-ready visual artifact from structured data.
3. **Make status updates trivial to maintain** — editing a single JSON file must be the only "admin UI."
4. **Preserve visual fidelity for screenshots** — the rendered page must match the canonical `OriginalDesignConcept.html` / `OriginalDesignConcept.png` at 1920×1080 pixel-for-pixel.
5. **Keep implementation cost and complexity minimal** — deliver v1 in ≤4 engineer-days using only the mandated stack (C# .NET 8 + Blazor Server, local-only, `.sln` structure).
6. **Enable stakeholder self-service** — a PM (non-developer) should be able to edit `data.json`, refresh the browser, and re-screenshot without a developer in the loop.
7. **Establish a reusable reporting template** that can be re-skinned for additional projects by swapping `data.json`.

## User Stories & Acceptance Criteria

### Story 1: View project header at a glance
**As an** executive reviewing a status deck, **I want** the dashboard to display the project title, subtitle/workstream, current reporting month, and a link to the backing ADO backlog, **so that** I immediately understand what project and time period I'm looking at.

*Visual reference: Header section (`.hdr`) of `OriginalDesignConcept.html` — top band, 24px bold title with inline backlog link, 12px grey subtitle.*

Acceptance criteria:
- [ ] Top band renders project title at 24px bold, weight 700, color `#111`.
- [ ] Inline "→ ADO Backlog" link in `#0078D4` immediately to the right of the title.
- [ ] Subtitle line below title: 12px, `#888`, format "`<Org> · <Workstream> · <Month Year>`".
- [ ] Title, subtitle, and backlog URL are read from `data.json` → `project.{title,subtitle,backlogUrl}`.
- [ ] Header has a 1px `#E0E0E0` bottom border and 12px 44px padding.

### Story 2: Understand legend symbols
**As an** executive, **I want** a visible legend explaining each symbol (PoC Milestone diamond, Production Release diamond, Checkpoint dot, NOW line), **so that** I can interpret the timeline without a separate key.

*Visual reference: Right-aligned legend in the `.hdr` section.*

Acceptance criteria:
- [ ] Legend row is flex-aligned right in the header with 22px gap between items.
- [ ] PoC Milestone: 12×12px amber (`#F4B400`) diamond (rotated square) + label "PoC Milestone".
- [ ] Production Release: 12×12px green (`#34A853`) diamond + label "Production Release".
- [ ] Checkpoint: 8×8px grey (`#999`) circle + label "Checkpoint".
- [ ] Now: 2×14px red (`#EA4335`) vertical bar + label "Now (<Current Month Year>)" with the current month computed server-side.

### Story 3: See milestone timeline across months
**As an** executive, **I want** a horizontal Gantt-style timeline showing all major milestones (big rocks) across swim lanes with month gridlines and a "NOW" indicator, **so that** I can see what's already delivered, what's imminent, and what's ahead.

*Visual reference: `.tl-area` section (196px tall) of `OriginalDesignConcept.html` — left 230px label column + right SVG timeline (1560×185).*

Acceptance criteria:
- [ ] Timeline area is 196px tall with background `#FAFAFA` and bottom border 2px `#E8E8E8`.
- [ ] Left column (230px) lists each swim lane as `<LaneId><br><LaneLabel>` at 12px, color-coded per lane (e.g., M1 `#0078D4`, M2 `#00897B`, M3 `#546E7A`).
- [ ] SVG timeline (1560×185) renders one horizontal line per lane at the lane's brand color, 3px stroke.
- [ ] Month gridlines rendered as faint vertical lines at `#bbb` 0.4 opacity with "Jan", "Feb", … labels at `#666` 11px.
- [ ] Milestones render as: amber diamond for `type=poc`, green diamond for `type=prod`, circle (stroked white / filled grey) for `type=checkpoint`.
- [ ] Each milestone has a date/label caption above or below, 10px `#666`.
- [ ] Dashed red vertical "NOW" line with label "NOW" at the x-coordinate `(Today - start) / (end - start) × 1560`, computed server-side.
- [ ] All milestone dates, lane definitions, and timeline start/end dates are sourced from `data.json` → `timeline.*`.

### Story 4: See monthly execution heatmap
**As an** executive, **I want** a 4-row × 4-column grid of Shipped / In Progress / Carryover / Blockers items across the last four months with the current month highlighted, **so that** I can scan execution momentum and slippage at a glance.

*Visual reference: `.hm-wrap` / `.hm-grid` section of `OriginalDesignConcept.html` — CSS Grid `160px repeat(4,1fr)` × `36px repeat(4,1fr)`.*

Acceptance criteria:
- [ ] Heatmap occupies the remaining vertical space below the timeline (flex:1).
- [ ] Section title "Monthly Execution Heatmap · Shipped • In Progress • Carryover • Blockers" at 14px uppercase `#888`, letter-spacing 0.5px.
- [ ] Grid: `grid-template-columns:160px repeat(4,1fr)`; `grid-template-rows:36px repeat(4,1fr)`; outer border `1px solid #E0E0E0`.
- [ ] Column headers show month names at 16px bold on `#F5F5F5`; the current month header uses `#FFF0D0` background and `#C07700` text.
- [ ] Row headers are 11px uppercase bold, letter-spacing 0.7px, colored per category (see Visual Design Specification).
- [ ] Shipped row: `#E8F5E9` header, `#F0FBF0` cells, `#D8F2DA` for current month, `#34A853` bullet dots, `#1B7A28` header text.
- [ ] In Progress row: `#E3F2FD` header, `#EEF4FE` cells, `#DAE8FB` current, `#0078D4` dots, `#1565C0` header text.
- [ ] Carryover row: `#FFF8E1` header, `#FFFDE7` cells, `#FFF0B0` current, `#F4B400` dots, `#B45309` header text.
- [ ] Blockers row: `#FEF2F2` header, `#FFF5F5` cells, `#FFE4E4` current, `#EA4335` dots, `#991B1B` header text.
- [ ] Each item in a cell renders as 12px `#333`, preceded by a 6×6px colored dot at left.
- [ ] All cell content sourced from `data.json` → `heatmap.rows[].cells[][]`.
- [ ] Empty cells render a single `-` glyph in `#AAA`.

### Story 5: Edit data without rebuilding
**As a** PM maintaining the dashboard, **I want** to edit `data.json` and see changes reflected in the browser on refresh (no rebuild), **so that** I can iterate quickly before executive reviews.

Acceptance criteria:
- [ ] `data.json` lives at a well-known path (`wwwroot/data.json`) and is documented in README.
- [ ] Changes to `data.json` are detected by `FileSystemWatcher` (debounced ~250ms) and picked up on next browser refresh without restarting the process.
- [ ] Schema is documented in README with an annotated example.
- [ ] A sample fictional-project `data.json` ships in the repo.

### Story 6: Graceful handling of invalid data
**As a** PM who just made a typo in `data.json`, **I want** to see a clear, human-readable error banner at the top of the page, **so that** I can fix the file without reading a stack trace.

Acceptance criteria:
- [ ] If `data.json` is missing, malformed JSON, or fails schema validation, the page renders a red banner at top with: file path, line/column if available, and the parse/validation error message.
- [ ] The rest of the page degrades gracefully (empty timeline/heatmap placeholders) rather than throwing a YSOD.
- [ ] A recoverable error does not crash the host process.

### Story 7: Screenshot-ready rendering
**As a** PM building an executive deck, **I want** the rendered page to be exactly 1920×1080, with no scrollbars, loading spinners, or interactive chrome, **so that** a browser screenshot drops cleanly into a 16:9 PowerPoint slide.

*Visual reference: `body` rule in `OriginalDesignConcept.html` — `width:1920px;height:1080px;overflow:hidden`.*

Acceptance criteria:
- [ ] `<body>` has `width:1920px; height:1080px; overflow:hidden`.
- [ ] No Blazor circuit loading indicator, SignalR reconnect UI, or dev error overlay is visible at `http://localhost:5080/`.
- [ ] First HTTP GET returns fully rendered HTML (static SSR) — no flicker.
- [ ] Font stack: `'Segoe UI', -apple-system, 'Helvetica Neue', Arial, sans-serif`.
- [ ] README documents the recommended screenshot workflow (browser at 1920+ width, F12 → device toolbar → 1920×1080, or use Windows Snipping Tool).

### Story 8: Run locally with one command
**As a** developer or PM, **I want** to clone the repo and run `dotnet run`, **so that** I can open `http://localhost:5080` and see the dashboard immediately.

Acceptance criteria:
- [ ] `dotnet run` from `src/ReportingDashboard.Web/` starts Kestrel on `http://localhost:5080` only (no HTTPS, no external binding).
- [ ] README has a ≤5-line "Getting Started" section.
- [ ] Sample `data.json` renders a fully populated dashboard on first run.
- [ ] Optional: self-contained single-file publish (`dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true`) produces a double-clickable `.exe`.

## Visual Design Specification

All visuals are canonicalized by `OriginalDesignConcept.html` (source) and `OriginalDesignConcept.png` (rendered reference). Engineers MUST open and read `OriginalDesignConcept.html` before writing any markup and MUST visually diff their output against `OriginalDesignConcept.png` at 1920×1080.

### Global Layout

- **Viewport:** `<body>` is `width:1920px; height:1080px; overflow:hidden; background:#FFFFFF; color:#111;`.
- **Font stack:** `'Segoe UI', -apple-system, 'Helvetica Neue', Arial, sans-serif`. Windows is the canonical screenshot OS.
- **Page structure (top to bottom, flex column):**
  1. Header band (`.hdr`) — auto height, ~48px.
  2. Timeline area (`.tl-area`) — fixed 196px.
  3. Heatmap wrapper (`.hm-wrap`) — flex:1, fills remaining height (~836px).
- **Reset:** `*{margin:0;padding:0;box-sizing:border-box;}`.

### Color Palette (exact hex codes)

| Role | Hex |
|---|---|
| Page background | `#FFFFFF` |
| Primary text | `#111` |
| Subtitle / muted | `#888` |
| Muted-2 / borders heavy | `#999` |
| Light divider | `#E0E0E0` |
| Heavy divider | `#CCC` |
| Panel light | `#FAFAFA` |
| Panel medium | `#F5F5F5` |
| Microsoft blue (M1, progress) | `#0078D4` |
| Teal (M2) | `#00897B` |
| Slate (M3) | `#546E7A` |
| Current-month header bg | `#FFF0D0` |
| Current-month header fg | `#C07700` |
| Shipped dot / milestone prod | `#34A853` |
| Shipped header text | `#1B7A28` |
| Shipped header bg | `#E8F5E9` |
| Shipped cell bg | `#F0FBF0` |
| Shipped cell current bg | `#D8F2DA` |
| Progress header text | `#1565C0` |
| Progress header bg | `#E3F2FD` |
| Progress cell bg | `#EEF4FE` |
| Progress cell current bg | `#DAE8FB` |
| Carryover dot / PoC milestone | `#F4B400` |
| Carryover header text | `#B45309` |
| Carryover header bg | `#FFF8E1` |
| Carryover cell bg | `#FFFDE7` |
| Carryover cell current bg | `#FFF0B0` |
| Blocker dot / NOW line | `#EA4335` |
| Blocker header text | `#991B1B` |
| Blocker header bg | `#FEF2F2` |
| Blocker cell bg | `#FFF5F5` |
| Blocker cell current bg | `#FFE4E4` |
| Empty-cell placeholder | `#AAA` |
| Cell item body text | `#333` |
| Timeline gridline | `#bbb` @ 0.4 opacity |
| Timeline label grey | `#666` |

### Typography

| Element | Size | Weight | Color | Transform |
|---|---|---|---|---|
| Header title (`h1`) | 24px | 700 | `#111` | — |
| Header subtitle (`.sub`) | 12px | 400 | `#888` | — |
| Legend text | 12px | 400 | `#111` | — |
| Timeline month labels | 11px | 600 | `#666` | — |
| Timeline milestone captions | 10px | 400 | `#666` | — |
| Timeline NOW label | 10px | 700 | `#EA4335` | — |
| Lane label (big) | 12px | 600 | brand color | — |
| Lane sub-label | 12px | 400 | `#444` | — |
| Heatmap section title | 14px | 700 | `#888` | uppercase, letter-spacing 0.5px |
| Heatmap column header | 16px | 700 | `#111` (or `#C07700` current) | — |
| Heatmap row header | 11px | 700 | category color | uppercase, letter-spacing 0.7px |
| Heatmap corner cell | 11px | 700 | `#999` | uppercase |
| Heatmap cell item | 12px | 400 | `#333` | — |

### Section 1 — Header (`.hdr`)

- `padding:12px 44px 10px;` `border-bottom:1px solid #E0E0E0;` `display:flex; align-items:center; justify-content:space-between; flex-shrink:0;`
- **Left cluster:** `<h1>` containing project title + inline `<a>` backlog link (with "→" arrow glyph). Below: `.sub` — "`<Org> · <Workstream> · <Month Year>`".
- **Right cluster (legend):** flex row, `gap:22px`, items vertically centered:
  - Amber 12×12 diamond (`rotate(45deg)`) + "PoC Milestone"
  - Green 12×12 diamond + "Production Release"
  - Grey 8×8 circle + "Checkpoint"
  - Red 2×14 vertical bar + "Now (<Month Year>)"

### Section 2 — Timeline (`.tl-area`)

- `display:flex; align-items:stretch; padding:6px 44px 0; flex-shrink:0; height:196px; border-bottom:2px solid #E8E8E8; background:#FAFAFA;`
- **Lane-label column (left, 230px fixed):** flex column, `justify-content:space-around`, right-bordered `1px solid #E0E0E0`. Each lane row: `<LaneId>` 12px weight 600 in lane color, `<br>`, `<LaneLabel>` 12px weight 400 `#444`.
- **SVG canvas (right, 1560×185):**
  - Month gridlines: `<line stroke="#bbb" stroke-opacity="0.4" stroke-width="1">` at evenly spaced x-coords from timeline start to end; `<text fill="#666" font-size="11" font-weight="600">` month abbreviation at top.
  - NOW indicator: `<line stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3">` full height, plus `<text>` "NOW" 10px bold.
  - Per lane: horizontal `<line stroke="<laneColor>" stroke-width="3">` at the lane's y-coordinate.
  - Milestone markers (at computed x for each `milestone.date`):
    - `type=poc` → amber diamond `polygon fill="#F4B400"` with drop shadow filter `#sh`.
    - `type=prod` → green diamond `polygon fill="#34A853"` with drop shadow.
    - `type=checkpoint` → stroked white circle `r=5..7` OR filled grey `r=4` dot.
  - Each milestone has a 10px `#666` caption (date + short label) offset above or below to avoid overlap.
- **SVG filter:** `<filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter>`.

### Section 3 — Heatmap (`.hm-wrap` + `.hm-grid`)

- Wrapper: `flex:1; min-height:0; display:flex; flex-direction:column; padding:10px 44px 10px;`
- Title bar (`.hm-title`): "Monthly Execution Heatmap · Shipped • In Progress • Carryover • Blockers" at the typography spec above, `margin-bottom:8px; flex-shrink:0;`.
- Grid (`.hm-grid`): `flex:1; min-height:0; display:grid; grid-template-columns:160px repeat(4,1fr); grid-template-rows:36px repeat(4,1fr); border:1px solid #E0E0E0;`
- **Row 0 (headers):**
  - Corner cell `.hm-corner`: "Status" on `#F5F5F5`, `#999` text, right+bottom borders.
  - Four `.hm-col-hdr` month cells on `#F5F5F5`; current month cell adds `.apr-hdr` modifier (`#FFF0D0` bg, `#C07700` text).
- **Rows 1-4 (data):** For each of Shipped / In Progress / Carryover / Blockers:
  - Row header: `.hm-row-hdr.<cat>-hdr` — icon + uppercase category label, padded `0 12px`, right-bordered `2px solid #CCC`, color/bg per palette above.
  - Four data cells: `.hm-cell.<cat>-cell`; current-month cell adds `.apr` modifier for the darker tint. `padding:8px 12px`; `overflow:hidden`.
  - Each item inside a cell is `.it` — 12px `#333`, `padding:2px 0 2px 12px`, `line-height:1.35`, with a `::before` pseudo-element that is a 6×6px `border-radius:50%` category-colored dot positioned at `left:0; top:7px;`.
  - Empty cells render a single `.it` with `color:#AAA` and content `-`.

### Component Hierarchy

```
Dashboard.razor (@page "/")
├── DashboardHeader.razor       — project title, backlog link, subtitle, legend
├── TimelineSvg.razor           — 230px lane-label column + 1560×185 SVG
└── Heatmap.razor               — .hm-title + .hm-grid (CSS Grid, 5×5)
```

### Reference Files

- `docs/OriginalDesignConcept.html` — canonical markup/CSS source.
- `docs/design-screenshots/OriginalDesignConcept.png` — canonical rendered reference at 1920×1080.
- Engineers must read the HTML source **before** writing any code; CSS should be ported verbatim into `Dashboard.razor.css` (with `.apr` renamed to `.current` for clarity).

## UI Interaction Scenarios

**Scenario 1 — Initial page load (happy path):** User navigates to `http://localhost:5080/`. Server reads `data.json`, binds to `DashboardData`, and returns fully rendered HTML on the first byte. User sees the full 1920×1080 dashboard immediately with no spinner, no flicker, and no scrollbars.

**Scenario 2 — Viewing the header:** User's eye lands on the top band. They read the project title ("Privacy Automation Release Roadmap") at 24px bold, the inline "→ ADO Backlog" link in Microsoft blue, and the subtitle ("Trusted Platform · Privacy Automation Workstream · April 2026") in muted grey directly below.

**Scenario 3 — Interpreting the legend:** User glances to the right edge of the header and sees four legend items side-by-side (amber diamond = PoC, green diamond = Production, grey circle = Checkpoint, red bar = Now). No hover or click required — the legend is always visible.

**Scenario 4 — Reading the milestone timeline:** User scans the Gantt-style SVG below the header. They see three horizontal swim lanes (M1 blue, M2 teal, M3 slate), labeled on the left. Each lane has diamonds and circles at the dates of its milestones, with 10px captions (e.g., "Mar 26 PoC", "Apr Prod (TBD)"). A dashed red "NOW" vertical line marks today's position across all lanes.

**Scenario 5 — Data-driven NOW line:** When the server renders the page on April 19, 2026, the NOW line appears at x ≈ `(Apr 19 − Jan 1) / (Jun 30 − Jan 1) × 1560 ≈ 974`. One month later on May 19, the same code re-renders with x ≈ 1234 — no code change needed.

**Scenario 6 — Current month highlighting:** The heatmap column corresponding to `DateTime.Today.Month` gets the `.current` modifier class, giving its header an amber-tinted background (`#FFF0D0`) and its cells a slightly darker tint of their category color (e.g., `#D8F2DA` for shipped). This draws the executive's eye to the most recent month's deliverables.

**Scenario 7 — Reading a cell:** User scans the In Progress row for the April column and sees four items, each prefixed by a small blue dot, listing deliverables currently underway. Items are plain text, 12px, non-interactive.

**Scenario 8 — Empty cell:** The Shipped row for May (a future month) contains no items in `data.json`. The cell renders a single grey `-` character centered vertically, signaling "nothing yet" without leaving visual whitespace that looks like a rendering bug.

**Scenario 9 — Cell with many items (overflow policy):** A month contains 7 items but the cell only fits 4 comfortably. The first 4 render normally; the 5th-7th are collapsed into a single `.it` reading "`+3 more`" in the same style. The cap `N` is configurable in `data.json` (default: 4).

**Scenario 10 — PM edits data.json and refreshes:** PM opens `wwwroot/data.json` in VS Code, adds a new milestone to M1, and saves. `FileSystemWatcher` detects the change within ~250ms and invalidates the cached `DashboardData`. PM hits F5 in the browser; the new milestone diamond is now rendered on the M1 swim lane. No rebuild, no restart.

**Scenario 11 — Malformed JSON (error state):** PM accidentally removes a closing brace. On next browser refresh, the page renders a red banner across the top: "⚠ Failed to load data.json: Unexpected end of JSON input at line 42, column 3." The header, timeline, and heatmap below degrade to empty placeholders. The process does not crash.

**Scenario 12 — Missing file (error state):** If `data.json` does not exist, the banner reads "⚠ data.json not found at <path>. See README for schema."

**Scenario 13 — Screenshot workflow:** PM opens the dashboard in Edge on a 1920-wide external monitor (or uses DevTools device toolbar at 1920×1080), presses Win+Shift+S, captures the full viewport, and pastes directly into a 16:9 PowerPoint slide. No cropping or post-processing required.

**Scenario 14 — Laptop screen fallback:** PM is on a 1440-wide laptop. The body remains fixed at 1920px and horizontally overflows; PM uses browser zoom to 75% to fit the full design on-screen for review. Screenshotting at reduced zoom is explicitly NOT supported for deck use — canonical screenshots require a 1920-wide window.

**Scenario 15 — No interactivity (by design):** User hovers over a milestone diamond. Nothing happens (no tooltip). User clicks a cell. Nothing happens (no drill-down). This is intentional — the page is a screenshot artifact, not an application. The only interactive element is the "→ ADO Backlog" link in the header.

**Scenario 16 — Second project, same template:** To produce a dashboard for a different project, the PM copies the folder, replaces `data.json`, and runs `dotnet run`. No code changes.

## Scope

### In Scope

- Single-page Blazor Server (.NET 8) app with static SSR rendering at `http://localhost:5080/`.
- Fixed 1920×1080 viewport; pixel-faithful port of `OriginalDesignConcept.html` CSS.
- Three rendered sections: header + legend, milestone timeline (SVG), monthly execution heatmap (CSS Grid).
- Strongly-typed POCO models (`DashboardData`, `TimelineLane`, `Milestone`, `HeatmapRow`, `HeatmapItem`) bound from `wwwroot/data.json` via `System.Text.Json`.
- `DashboardDataService` singleton with `FileSystemWatcher`-based hot-reload (debounced) and `IMemoryCache` invalidation.
- Server-side computation of NOW line x-coordinate, month gridlines, and current-month column flag (never hardcoded).
- JSON schema validation with a friendly in-page red banner on parse/validation failure.
- Sample fictional-project `data.json` committed to the repo as the canonical example.
- README covering: editing `data.json`, running locally (`dotnet run`), optional self-contained publish, and screenshot workflow.
- Unit tests (xUnit) for timeline coordinate math, NOW-line positioning, and overflow-truncation logic.
- bUnit test confirming `Dashboard.razor` renders without exception against the sample `data.json`.
- Solution structure: `ReportingDashboard.sln` + `src/ReportingDashboard.Web/` + `tests/ReportingDashboard.Web.Tests/`.
- Heatmap per-cell item overflow: truncate at N items with "+K more" (N default 4, configurable in JSON).
- Optional: `dotnet publish` single-file win-x64 self-contained executable.

### Out of Scope

- Authentication, authorization, SSO, RBAC, identity of any kind.
- HTTPS / TLS certificates (localhost HTTP only).
- Cloud hosting, containers (Docker / Kubernetes), Azure App Service, App Insights, OpenTelemetry.
- Any database, ORM (EF Core, Dapper), or persistent data store beyond the `data.json` file.
- Component libraries (MudBlazor, Radzen, FluentUI Blazor) or charting libraries (ChartJs.Blazor, Plotly.Blazor, ApexCharts).
- Blazor interactive render modes (`InteractiveServer`, `InteractiveWebAssembly`) and any SignalR-backed interactivity.
- Responsive / mobile design, adaptive breakpoints, touch interactions.
- Tooltips, modals, drill-downs, filtering, sorting, search, pagination.
- Multi-project/multi-tenant support (one app instance = one `data.json` = one project).
- In-app editing of `data.json` (no admin UI, no forms).
- Export-to-PDF button, built-in screenshot generation, headless-Chrome rendering.
- ADO/Jira integration or any data-import pipeline.
- Internationalization, localization, RTL support.
- Accessibility beyond baseline semantic HTML (explicit color-blind-safe palette not required for v1; flagged as open question).
- Theming beyond what's expressible via swapping color fields in `data.json`.

## Non-Functional Requirements

**Performance**
- Time-to-first-byte for `GET /` ≤ 200ms on a developer laptop.
- Full page render (HTML + CSS, no JS bundle beyond Blazor framework defaults for static SSR) ≤ 150KB total payload.
- Server-side render of `Dashboard.razor` ≤ 10ms for a `data.json` up to 50KB.
- `data.json` hot-reload debounce ≤ 500ms from file save to cache invalidation.

**Security**
- Kestrel bound to `http://localhost:5080` only (loopback); not network-reachable by default.
- No authentication, no authorization (explicit product decision; documented in README).
- No secrets in source or in `data.json` (plaintext executive summary content only).
- No outbound network calls at runtime.
- Dependency hygiene: only first-party Microsoft packages + pinned `FluentAssertions 6.12.x` (avoid v8 commercial license) and `bUnit 1.33.x`.

**Scalability**
- Single-user, single-process, local-only. No horizontal scaling required.
- `data.json` expected size: <50KB; design target: up to 200KB without degradation.
- Heatmap item count: up to 20 items per cell before truncation kicks in.

**Reliability**
- Malformed `data.json` MUST NOT crash the host — render an error banner and continue serving.
- Missing `data.json` MUST produce a clear "file not found" banner, not a YSOD.
- No uptime SLA (local tool, started on demand).
- Zero external dependencies at runtime (no DB, no API, no CDN) — reliability ≈ reliability of `dotnet run`.

**Maintainability**
- Total Razor + C# LOC target: <1,000 lines for v1.
- `dotnet format` and `Microsoft.CodeAnalysis.NetAnalyzers` clean.
- ≥70% line coverage on coordinate-math and data-binding logic.
- README enables a non-author developer to run the app and edit `data.json` within 5 minutes.

**Observability**
- Console logging only (`Microsoft.Extensions.Logging.Console`). Log: startup, `data.json` load/reload events, parse errors.

**Portability**
- Canonical render target: Windows 10/11 + Edge/Chrome at 1920×1080.
- Must build and run on macOS/Linux for development, but screenshot fidelity is only guaranteed on Windows (Segoe UI font).

## Success Metrics

1. **Pixel parity** — rendered page at 1920×1080 is visually indistinguishable from `OriginalDesignConcept.png` by side-by-side inspection (all section positions, colors, font sizes match).
2. **Edit-to-render loop time** — PM can edit `data.json`, refresh browser, and see the change in ≤10 seconds (manual stopwatch).
3. **Time-to-first-dashboard** — a new developer clones the repo and sees a rendered dashboard at `http://localhost:5080/` in ≤5 minutes from `git clone`.
4. **Deck adoption** — PM uses ≥1 screenshot of the rendered dashboard in an executive review deck within 2 weeks of v1 ship.
5. **Zero-regression rendering** — 100% of xUnit + bUnit tests pass in CI on every PR.
6. **Payload budget** — full-page payload remains <150KB across all v1 commits (enforced by a manual check or simple test).
7. **Error-banner coverage** — 100% of manually injected malformed-JSON cases render a banner instead of crashing (verified by a documented manual test plan).
8. **Implementation velocity** — v1 shipped in ≤4 engineer-days from Phase 0 start.

## Constraints & Assumptions

**Mandatory technical constraints**
- Language / runtime: **C# .NET 8 LTS** (8.0.11+). No other runtimes.
- UI framework: **Blazor Server** with **Static SSR** render mode only. `InteractiveServer`/`InteractiveWebAssembly` are forbidden.
- Solution layout: **`.sln` with `src/` and `tests/`** folders.
- Deployment target: **local workstation only** (`dotnet run`). No cloud, no container.
- Storage: **single `data.json` file**. No database, no ORM.
- UI library: **none** — raw Razor + hand-authored CSS.
- Authentication: **none**.

**Design constraints**
- Canonical design source: `OriginalDesignConcept.html` + `OriginalDesignConcept.png`. All engineers MUST read the HTML source before writing code.
- Fixed 1920×1080 viewport; responsive design explicitly forbidden.
- Font: `'Segoe UI', -apple-system, 'Helvetica Neue', Arial, sans-serif`. Windows is the canonical screenshot OS.
- CSS Grid `grid-template-columns:160px repeat(4,1fr)` is load-bearing and must be preserved.
- Color palette is fixed as enumerated in the Visual Design Specification.

**Timeline assumptions**
- ~4 engineer-days total: Phase 0 scaffold (0.5d) → Phase 1 static port (1d) → Phase 2 data binding (1d) → Phase 3 hot-reload + error banner (0.5d) → Phase 4 tests (0.5d) → buffer (0.5d).
- Design is locked; no iteration on visual spec during implementation.
- Sample `data.json` can be authored in parallel with Phase 1 by the PM.

**Dependency assumptions**
- .NET 8 SDK is installed on the developer's and PM's machine.
- A modern Chromium browser (Edge/Chrome) is available for rendering and screenshotting.
- Segoe UI is available on the screenshot machine (standard on Windows).
- `wwwroot/data.json` is the sole source of truth; no upstream data system integration is assumed for v1.
- PMs are comfortable editing JSON manually (or using VS Code with schema validation).

**Scope assumptions**
- One project per app instance. Multi-project routing (`/projects/{slug}`) is not assumed.
- Executive audience accepts the current red/yellow/green palette (colorblind-safety flagged as open question, not a v1 blocker).
- "NOW" is defined as server-local `DateTime.Today` — acceptable for a single-user local tool.
- Heatmap shows 4 months; timeline shows 6 months. Both default values match the reference design; both are configurable via `data.json` but default behavior is not changed in v1.
- No PII in `data.json`. If that assumption breaks in future, security model must be revisited.