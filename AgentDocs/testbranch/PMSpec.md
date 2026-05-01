# PM Specification: ReportingDashboard

## Executive Summary

The ReportingDashboard is a local-only, zero-cost developer tool that renders a composite release roadmap visualization—combining a Gantt-style timeline with milestone markers and a color-coded monthly execution heatmap—driven by Azure DevOps work-item data. It eliminates the ~30-minute manual process of assembling status reports from ADO queries and spreadsheets, replacing it with a one-click sync and a pixel-perfect 1920×1080 screenshot-ready dashboard that runs entirely on `localhost` with no cloud services, no hosting infrastructure, and no operational cost.

---

## Business Goals

1. **Eliminate manual status report assembly** — Reduce roadmap generation time from ~30 minutes of ADO querying and spreadsheet formatting to <2 minutes (one-click sync + browser screenshot).
2. **Provide a single-view roadmap for leadership** — One browser screenshot at 1920×1080 captures the full team status: workstream milestones, shipped items, in-progress work, carryover, and blockers.
3. **Enable data-driven standup conversations** — Heatmap cells link directly to ADO work items; drill-down shows item-level detail with clickable ADO URLs without leaving the dashboard.
4. **Zero operational cost** — No cloud hosting, no database servers, no licenses, no recurring fees. The tool runs locally on any Windows machine with .NET 8.
5. **Distributable as a single artifact** — Team members can run the dashboard from a single self-contained EXE (<40MB) without installing Node.js or cloning the repository.

---

## User Stories & Acceptance Criteria

### Epic 1: Static Dashboard Rendering (Phase 1)

**US-1.1** — **As an engineering lead**, I want to see the dashboard title, team context, and a visual legend, so that I immediately understand what the visualization represents.

*Visual Reference: Section A (Header Bar) of `OriginalDesignConcept.html`*

- [ ] Header displays "Privacy Automation Release Roadmap" in 24px bold `#111`
- [ ] "→ ADO Backlog" link appears next to the title in `#0078D4` and opens the configured ADO URL in a new tab
- [ ] Subtitle shows "Trusted Platform · Privacy Automation Workstream · {Current Month} {Year}" in 12px `#888`
- [ ] Legend renders four items matching the design: PoC diamond (gold `#F4B400`), Production diamond (green `#34A853`), Checkpoint circle (gray `#999`), Now line (red `#EA4335`)
- [ ] Header layout matches the design reference within 2px tolerance at 1920×1080

---

**US-1.2** — **As a program manager**, I want to see workstream timelines with milestone diamonds positioned by date, so that I can track progress against key dates.

*Visual Reference: Section B (Timeline/Gantt Area) of `OriginalDesignConcept.html`*

- [ ] Left panel (230px) displays workstream labels (M1, M2, M3) with names and correct colors (M1: `#0078D4`, M2: `#00897B`, M3: `#546E7A`)
- [ ] SVG renders horizontal workstream lane lines in the correct colors with 3px stroke width
- [ ] PoC milestones render as gold (`#F4B400`) diamond polygons with `feDropShadow` filter
- [ ] Production milestones render as green (`#34A853`) diamond polygons with `feDropShadow` filter
- [ ] Checkpoints render as circles (outlined white fill with colored stroke for major; solid `#999` for minor)
- [ ] Date labels appear near each milestone marker in 10px `#666`
- [ ] Month grid lines divide the timeline into equal monthly intervals with labels in 11px semi-bold `#666`
- [ ] D3 `scaleTime()` correctly maps dates to pixel positions within the 1560px SVG width
- [ ] Timeline renders from sample JSON data without an API call

---

**US-1.3** — **As a team member**, I want a visual indicator of today's date on the timeline, so that I can see where we are relative to upcoming milestones.

*Visual Reference: NOW line in Section B of `OriginalDesignConcept.html`*

- [ ] A dashed vertical line appears at the x-position corresponding to today's date
- [ ] Line uses stroke `#EA4335`, width 2px, dash pattern `5,3`
- [ ] "NOW" label appears at the top of the line in 10px bold `#EA4335`
- [ ] Position recalculates on each page load based on `new Date()`

---

**US-1.4** — **As an engineering lead**, I want to see work items categorized into a Shipped/InProgress/Carryover/Blocked × Month grid, so that I can quickly assess team execution status.

*Visual Reference: Section C (Heatmap Grid) of `OriginalDesignConcept.html`*

- [ ] CSS Grid renders with `grid-template-columns: 160px repeat(N, 1fr)` where N = number of displayed months (default 4)
- [ ] Column headers display month names in 16px bold; current month highlighted with `#FFF0D0` background and `#C07700` text with "← Now" indicator
- [ ] Four status rows render with correct row header colors, text, and backgrounds per the color token table
- [ ] Each work item in a cell displays with a 6×6px colored `::before` pseudo-element dot and 12px `#333` title text
- [ ] Cells for the current month use the `bgActive` color variant
- [ ] Empty future-month cells display a dash in `#AAA`
- [ ] Grid renders from sample JSON data without an API call

---

**US-1.5** — **As a developer**, I want the dashboard to render completely from a `sample-data.json` file, so that I can develop and test UI components without a backend.

- [ ] `sample-data.json` contains workstreams, milestones, work items, and month definitions matching the design reference content
- [ ] `main.ts` loads `sample-data.json` and calls `renderHeader()`, `renderTimeline()`, `renderHeatmap()`
- [ ] The full dashboard matches `OriginalDesignConcept.html` when rendered at 1920×1080
- [ ] `npm run dev` serves the dashboard with Vite HMR on port 5173

---

### Epic 2: Backend API & Database (Phase 2)

**US-2.1** — **As a frontend developer**, I want `GET /api/roadmap` to return all dashboard data in a single JSON payload, so that the frontend can render with one HTTP call.

- [ ] Endpoint returns `RoadmapData` JSON with `workstreams`, `milestones`, `workItems`, `months`, `dateRange`, and `lastSyncUtc`
- [ ] Response is <50KB for typical data volumes (500 work items)
- [ ] Response is cached in `IMemoryCache` with 60-second TTL, invalidated on `POST /api/sync`
- [ ] Endpoint returns HTTP 200 with empty arrays (not 500) when the database is empty

---

**US-2.2** — **As a user**, I want `GET /api/workitems?status=X&month=Y` to return filtered work items for a specific heatmap cell, so that the drill-down panel can load quickly.

- [ ] Endpoint accepts `status` (Shipped|InProgress|Carryover|Blocked) and `month` (Jan|Feb|…) query parameters
- [ ] Returns an array of `WorkItemDto` objects with `id`, `title`, `status`, `month`, `adoUrl`
- [ ] Returns HTTP 400 with a descriptive message for invalid parameter values
- [ ] Response is <5KB for typical cell contents

---

**US-2.3** — **As a user running the tool for the first time**, I want the database to be created automatically, so that I don't need to run manual setup commands.

- [ ] `Program.cs` calls `db.Database.MigrateAsync()` on startup
- [ ] SQLite database file is created at `%LOCALAPPDATA%\ReportingDashboard\dashboard.db`
- [ ] Directory is created if it doesn't exist
- [ ] Schema includes `Workstreams`, `Milestones`, and `WorkItems` tables with correct columns and indexes
- [ ] WAL journal mode is enabled for concurrent read safety

---

**US-2.4** — **As a user**, I want the dashboard to display sample data immediately after installation, so that I can verify the tool works before configuring ADO sync.

- [ ] If the database has zero workstreams after migration, seed from `sample-data.json`
- [ ] Seeded data matches the design reference (3 workstreams, representative milestones and work items)
- [ ] Seeding is idempotent — running the app again does not duplicate records

---

**US-2.5** — **As a backend developer**, I want Swagger UI at `/swagger`, so that I can test API endpoints interactively during development.

- [ ] Swagger UI is accessible at `http://localhost:5000/swagger` when `ASPNETCORE_ENVIRONMENT=Development`
- [ ] All three endpoints are documented with request/response schemas
- [ ] Swagger UI is NOT served in Release/Production configuration

---

### Epic 3: ADO Data Sync (Phase 3)

**US-3.1** — **As an engineering lead**, I want to pull current work-item data from our ADO backlog into the dashboard, so that the heatmap reflects real team status.

- [ ] `AdoSyncService` executes a WIQL query scoped to the configured area path
- [ ] Work item IDs are batch-fetched in chunks of 200 per API call
- [ ] Fetched fields include: System.Id, System.Title, System.State, System.IterationPath, System.Tags, System.ChangedDate
- [ ] Items are upserted into SQLite — existing items updated, new items inserted
- [ ] `POST /api/sync` returns a `SyncResult` with `itemCount` and `syncedAtUtc`

---

**US-3.2** — **As a PM**, I want ADO work-item states to be automatically categorized into Shipped/InProgress/Carryover/Blocked, so that the heatmap is populated correctly.

- [ ] State mapping: Closed/Resolved/Done → Shipped; Active/Committed → InProgress
- [ ] Items tagged `[blocked]` (case-insensitive) are categorized as Blocked regardless of state
- [ ] Items tagged `[carryover]` or in a past iteration while not closed are categorized as Carryover
- [ ] State mapping is configurable via `appsettings.json` for teams with custom ADO states
- [ ] Unrecognized states default to InProgress

---

**US-3.3** — **As a user**, I want a "Sync" button in the dashboard header, so that I can refresh data from ADO with one click.

*Visual Reference: Header area of `OriginalDesignConcept.html` — button added to header right side*

- [ ] A "Sync" button appears in the header bar (right side, near the legend)
- [ ] Clicking the button sends `POST /api/sync`
- [ ] A loading indicator ("Syncing…") is visible during the operation
- [ ] On success, the dashboard re-renders with updated data and shows "Synced {N} items at {time}"
- [ ] On failure, an error message is displayed without clearing existing dashboard data

---

**US-3.4** — **As a developer**, I want the ADO Personal Access Token to be stored securely and never committed to source control.

- [ ] PAT is read from .NET User Secrets (`Ado:Pat`) during development
- [ ] PAT can alternatively be provided via environment variable `REPORTINGDASHBOARD_ADO__PAT`
- [ ] `CredentialStore` class supports DPAPI encryption for distributed EXE scenario
- [ ] `.gitignore` includes `cred.dat` and `appsettings.*.json` (except base configs)
- [ ] PAT is never logged (Serilog destructure policy excludes it)

---

### Epic 4: Interactivity & Polish (Phase 4)

**US-4.1** — **As an engineering lead**, I want to click a heatmap cell and see the individual work items with links to ADO, so that I can quickly navigate to items that need attention.

*Visual Reference: Heatmap grid cells in Section C of `OriginalDesignConcept.html` — drill-down panel overlays or slides in*

- [ ] Clicking any heatmap data cell opens a drill-down panel
- [ ] Panel displays a list of work items with title and a clickable ADO URL (opens in new tab)
- [ ] Panel can be dismissed by clicking outside it, pressing Escape, or clicking a close button
- [ ] Panel title shows the cell context (e.g., "In Progress — April: 12 items")

---

**US-4.2** — **As a user**, I want clear feedback when data is loading or when an error occurs, so that I'm never looking at a blank screen wondering what happened.

- [ ] Initial page load shows a loading indicator while fetching `/api/roadmap`
- [ ] If the API is unreachable, a friendly error message is displayed
- [ ] If ADO sync fails, the specific error is shown and existing data remains visible
- [ ] No unhandled JavaScript exceptions appear in the browser console during normal operation

---

**US-4.3** — **As a PM**, I want to distribute the dashboard as a single EXE file, so that team members can run it without cloning the repo or installing Node.js.

- [ ] `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` produces a single EXE
- [ ] The EXE includes the Vite-built frontend assets in wwwroot
- [ ] Running the EXE starts the dashboard on `localhost:5000`
- [ ] EXE size is <40MB
- [ ] README documents the publish command and first-run instructions

---

**US-4.4** — **As a developer**, I want automated tests covering the API endpoints, sync service, and frontend rendering components, so that regressions are caught before merge.

- [ ] xUnit tests cover: `GET /api/roadmap` returns valid data, `GET /api/workitems` filters correctly, `POST /api/sync` handles success and failure
- [ ] `AdoSyncService` tests use mocked HTTP to verify state mapping (Closed→Shipped, tagged→Blocked, etc.)
- [ ] Vitest tests cover: `renderTimeline` creates correct SVG elements, `renderHeatmap` creates correct grid structure, NOW line uses current date
- [ ] `dotnet test` and `npm test` both pass in CI
- [ ] Code coverage: >70% line coverage for both backend and frontend

---

**US-4.5** — **As a developer**, I want a CI pipeline that builds and tests both backend and frontend on every push, so that broken code is caught before merge.

- [ ] GitHub Actions workflow runs on push and pull_request
- [ ] Backend job: `dotnet restore` → `dotnet build` → `dotnet test`
- [ ] Frontend job: `npm ci` → `npm run lint` → `npm test` → `npm run build`
- [ ] Jobs run in parallel; total CI time <3 minutes
- [ ] Pipeline fails if any test fails or build errors occur

---

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` rendered at 1920×1080. Engineers MUST match this design exactly.

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/13e558a1eadaf4c29841c3e2e2a72132ef362199/AgentDocs/testbranch/design-screenshots/OriginalDesignConcept.png)

### Global Styles

- **Viewport:** Fixed `1920px × 1080px`, `overflow: hidden`
- **Background:** `#FFFFFF`
- **Font:** `'Segoe UI', Arial, sans-serif` (Windows system font, zero web-font loading)
- **Base text color:** `#111`
- **Link color:** `#0078D4`, no underline
- **Box model:** `box-sizing: border-box` on all elements

### Section A: Header Bar

- **Layout:** `display: flex; align-items: center; justify-content: space-between`
- **Padding:** `12px 44px 10px`
- **Bottom border:** `1px solid #E0E0E0`
- **Flex-shrink:** 0 (fixed height, does not compress)

**Left content:**
- Title: `<h1>` at `font-size: 24px; font-weight: 700` containing "Privacy Automation Release Roadmap" followed by a link "→ ADO Backlog" in `#0078D4`
- Subtitle: `<div class="sub">` at `font-size: 12px; color: #888; margin-top: 2px` showing "Trusted Platform · Privacy Automation Workstream · April 2026"

**Right content (Legend):**
- Container: `display: flex; gap: 22px; align-items: center`
- Each legend item: `display: flex; align-items: center; gap: 6px; font-size: 12px`
  - **PoC Milestone:** 12×12px square, `background: #F4B400; transform: rotate(45deg)`, inline-block
  - **Production Release:** 12×12px square, `background: #34A853; transform: rotate(45deg)`, inline-block
  - **Checkpoint:** 8×8px circle, `border-radius: 50%; background: #999`
  - **Now indicator:** 2×14px rectangle, `background: #EA4335`, label text "Now (Apr 2026)"

### Section B: Timeline / Gantt Area

- **Layout:** `display: flex; align-items: stretch`
- **Dimensions:** `height: 196px; flex-shrink: 0`
- **Padding:** `6px 44px 0`
- **Background:** `#FAFAFA`
- **Bottom border:** `2px solid #E8E8E8`

**Left panel (Workstream Labels):**
- **Width:** 230px, `flex-shrink: 0`
- **Layout:** `display: flex; flex-direction: column; justify-content: space-around`
- **Padding:** `16px 12px 16px 0`
- **Right border:** `1px solid #E0E0E0`
- Each workstream label:
  - ID ("M1", "M2", "M3"): `font-size: 12px; font-weight: 600` in workstream color
  - Name: `font-weight: 400; color: #444` on next line via `<br/>`
  - M1 color: `#0078D4`, M2 color: `#00897B`, M3 color: `#546E7A`

**Right panel (SVG Timeline):**
- **Container:** `flex: 1; padding-left: 12px; padding-top: 6px`
- **SVG:** `width="1560" height="185"`, `overflow: visible`

**SVG Elements:**
- **Drop shadow filter:** `<filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter>`
- **Month grid lines:** Vertical `<line>` at equal 260px intervals (Jan=0, Feb=260, Mar=520, Apr=780, May=1040, Jun=1300), `stroke="#bbb" stroke-opacity="0.4" stroke-width="1"`
- **Month labels:** `<text>` at x+5 from each grid line, `y="14"`, `fill="#666" font-size="11" font-weight="600"`
- **Workstream lanes:** Horizontal `<line>` spanning full width (x1=0, x2=1560), `stroke-width="3"`:
  - M1 at `y="42"`, stroke `#0078D4`
  - M2 at `y="98"`, stroke `#00897B`
  - M3 at `y="154"`, stroke `#546E7A`
- **Milestone diamonds:** `<polygon>` with four points forming a diamond (radius ~11px from center), `filter="url(#sh)"`:
  - PoC: `fill="#F4B400"`
  - Production: `fill="#34A853"`
- **Checkpoint circles:**
  - Major: `<circle>` with `r="5-7"`, `fill="white"`, colored `stroke` (matching workstream or `#888`), `stroke-width="2.5"`
  - Minor: `<circle>` with `r="4"`, `fill="#999"` (solid, no stroke)
- **Date labels:** `<text>` near markers, `font-size="10"`, `fill="#666"`, `text-anchor="middle"`
- **NOW line:** `<line>` at current date x-position, full height, `stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"`; label `<text>` "NOW" at top, `fill="#EA4335" font-size="10" font-weight="700"`

### Section C: Monthly Execution Heatmap

- **Container:** `flex: 1; min-height: 0; display: flex; flex-direction: column; padding: 10px 44px 10px`

**Section title:**
- `font-size: 14px; font-weight: 700; color: #888; letter-spacing: 0.5px; text-transform: uppercase; margin-bottom: 8px`
- Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"

**Grid layout:**
- `display: grid`
- `grid-template-columns: 160px repeat(4, 1fr)`
- `grid-template-rows: 36px repeat(4, 1fr)`
- `border: 1px solid #E0E0E0`
- `flex: 1; min-height: 0`

**Corner cell (row 1, col 1):**
- `background: #F5F5F5`
- Text "STATUS": `font-size: 11px; font-weight: 700; color: #999; text-transform: uppercase`
- `border-right: 1px solid #E0E0E0; border-bottom: 2px solid #CCC`
- Content centered with flexbox

**Column headers (row 1, cols 2-5):**
- `font-size: 16px; font-weight: 700; background: #F5F5F5`
- Centered content via flexbox
- `border-right: 1px solid #E0E0E0; border-bottom: 2px solid #CCC`
- **Current month override:** `background: #FFF0D0; color: #C07700` (class `apr-hdr`)

**Row headers (col 1, rows 2-5):**
- `font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.7px`
- `padding: 0 12px; border-right: 2px solid #CCC; border-bottom: 1px solid #E0E0E0`
- Row-specific theming:

| Row | Text Color | Background | CSS Class |
|---|---|---|---|
| ✅ Shipped | `#1B7A28` | `#E8F5E9` | `ship-hdr` |
| 🔵 In Progress | `#1565C0` | `#E3F2FD` | `prog-hdr` |
| 🟡 Carryover | `#B45309` | `#FFF8E1` | `carry-hdr` |
| 🔴 Blockers | `#991B1B` | `#FEF2F2` | `block-hdr` |

**Data cells (cols 2-5, rows 2-5):**
- `padding: 8px 12px; border-right: 1px solid #E0E0E0; border-bottom: 1px solid #E0E0E0; overflow: hidden`
- Row-specific cell backgrounds:

| Row | Default Background | Current-Month Background | Dot Color | CSS Classes |
|---|---|---|---|---|
| Shipped | `#F0FBF0` | `#D8F2DA` | `#34A853` | `ship-cell`, `ship-cell.apr` |
| In Progress | `#EEF4FE` | `#DAE8FB` | `#0078D4` | `prog-cell`, `prog-cell.apr` |
| Carryover | `#FFFDE7` | `#FFF0B0` | `#F4B400` | `carry-cell`, `carry-cell.apr` |
| Blocked | `#FFF5F5` | `#FFE4E4` | `#EA4335` | `block-cell`, `block-cell.apr` |

**Work item elements within cells:**
- `<div class="it">` with `font-size: 12px; color: #333; padding: 2px 0 2px 12px; position: relative; line-height: 1.35`
- `::before` pseudo-element: `content: ''; position: absolute; left: 0; top: 7px; width: 6px; height: 6px; border-radius: 50%` with row-specific `background` color

### Color Token Reference

All colors MUST be referenced via the `COLORS` constant in `src/models/colors.ts`:

```
shipped:    bg=#F0FBF0  bgActive=#D8F2DA  dot=#34A853  header=#E8F5E9  text=#1B7A28
inProgress: bg=#EEF4FE  bgActive=#DAE8FB  dot=#0078D4  header=#E3F2FD  text=#1565C0
carryover:  bg=#FFFDE7  bgActive=#FFF0B0  dot=#F4B400  header=#FFF8E1  text=#B45309
blocked:    bg=#FFF5F5  bgActive=#FFE4E4  dot=#EA4335  header=#FEF2F2  text=#991B1B
milestone:  poc=#F4B400  production=#34A853  checkpoint=#999
ui:         nowLine=#EA4335  link=#0078D4  gridBorder=#E0E0E0  headerBg=#F5F5F5
            timelineBg=#FAFAFA  bodyText=#111  subtitleText=#888  itemText=#333
            currentMonthHeaderBg=#FFF0D0  currentMonthHeaderText=#C07700
```

---

## UI Interaction Scenarios

**Scenario 1: Initial page load with data**
User navigates to `http://localhost:5000`. The page fetches `GET /api/roadmap` and renders all three sections: header with title/subtitle/legend, timeline with workstream lanes and positioned milestones, and heatmap grid with work items grouped by status × month. The NOW dashed line appears at today's date. The current month column header is highlighted in gold (`#FFF0D0`). Total time from navigation to fully rendered: <1 second.

**Scenario 2: Initial page load with empty database (first run)**
User runs the tool for the first time. The database auto-creates and seeds with sample data. The dashboard renders with representative sample workstreams, milestones, and work items matching the design reference. No error or blank state appears.

**Scenario 3: User hovers over a milestone diamond on the timeline**
When the user moves the cursor over a diamond `<polygon>` element, a tooltip appears showing the milestone name, date, and type (e.g., "Mar 26 — PoC Milestone"). The diamond's drop shadow (`feDropShadow` filter) makes it visually distinct from the lane line beneath it.

**Scenario 4: User clicks a heatmap cell to drill down**
User clicks on any data cell (e.g., "In Progress" × "April"). A drill-down panel slides in or appears as an overlay. The panel title shows "In Progress — April: 12 items". Each item displays its title and a clickable ADO URL that opens in a new browser tab. The panel is dismissed by clicking outside it, pressing Escape, or clicking a close button.

**Scenario 5: User triggers an ADO sync via the Sync button**
User clicks the "Sync" button in the header. A loading indicator ("Syncing…") appears. The backend executes WIQL + batch fetch against ADO, maps states, and upserts into SQLite. On completion, the frontend re-fetches `/api/roadmap`, re-renders all components, and shows a success message: "Synced 342 items at 2:15 PM".

**Scenario 6: ADO sync fails due to expired PAT**
The `POST /api/sync` returns a 401. The frontend displays: "Sync failed: Invalid or expired PAT. Run `dotnet user-secrets set \"Ado:Pat\" \"<your-pat>\"` and restart." The existing dashboard data remains fully visible — the error does not blank the screen.

**Scenario 7: ADO sync fails due to network error**
The `POST /api/sync` times out or returns a network error. The frontend displays: "Sync failed: Could not reach Azure DevOps. Check your network connection." Existing data remains visible.

**Scenario 8: User clicks the "→ ADO Backlog" link in the header**
The link opens the configured ADO backlog URL (`https://dev.azure.com/{org}/{project}/_backlogs`) in a new browser tab, navigating directly to the full ADO board.

**Scenario 9: The NOW line reflects the current date**
Each time the dashboard renders (page load or post-sync re-render), the NOW line position is calculated from `new Date()` mapped through D3's `scaleTime()`. Refreshing the page on a different day moves the line to the correct position.

**Scenario 10: User views at 1920×1080 for screenshot capture**
The body has `width: 1920px; height: 1080px; overflow: hidden`. The user uses Edge's Ctrl+Shift+S screenshot tool or similar to capture a pixel-perfect roadmap image for slide decks or emails. All content fits within the viewport with no scrolling required.

**Scenario 11: API is unreachable on page load**
The frontend's fetch to `/api/roadmap` fails. Instead of a blank page, the dashboard displays a centered error message: "Could not connect to the dashboard API. Ensure the backend is running on localhost:5000." A retry button is available.

**Scenario 12: Heatmap cell with no items (empty future month)**
Future-month cells that have no work items display a single dash "–" in `#AAA` color, indicating no data rather than a rendering error.

---

## Scope

### In Scope

- Gantt timeline with D3.js: workstream lanes, milestone diamonds (PoC/Production), checkpoint circles, NOW line, month grid lines, date labels
- Heatmap grid with CSS Grid: 4 status rows (Shipped, In Progress, Carryover, Blocked) × N month columns (default 4), color-coded per design, work items with bullet dots
- REST API: `GET /api/roadmap` (full payload), `GET /api/workitems?status=X&month=Y` (drill-down), `POST /api/sync` (ADO pull)
- Local SQLite database via EF Core 8 with auto-migration and sample data seeding
- ADO integration via WIQL + batch REST API with configurable state mapping
- Drill-down panel: click heatmap cell → view work items with ADO links
- Security: PAT storage via User Secrets / env var / DPAPI, localhost-only Kestrel binding, `.gitignore` protection
- Self-contained single-file publish for Windows x64
- Unit tests: xUnit (backend) + Vitest (frontend)
- CI pipeline: GitHub Actions with parallel backend + frontend jobs
- Swagger UI for development-time API testing
- Serilog structured logging with file sink

### Out of Scope

- **Authentication / authorization** — local-only tool; localhost binding is sufficient
- **Multi-user / shared web deployment** — designed for single-user local use
- **Historical snapshots / time-travel** — current state only; no "view as of January"
- **Auto-refresh / WebSocket push** — on-demand sync via button is sufficient
- **Export to PNG/PDF** — layout is screenshot-optimized; use browser screenshot tools
- **Mobile / responsive layout** — fixed 1920×1080 viewport for screenshot fidelity
- **Dark mode** — not in the design reference
- **Desktop app wrapper (WebView2)** — deferred; browser-based is sufficient
- **Cross-platform (macOS/Linux)** — DPAPI and Segoe UI are Windows-specific
- **ADO write-back** — dashboard is read-only; edits happen in ADO
- **Multiple ADO projects / organizations** — single configurable area path
- **React/Vue/Angular framework** — vanilla TypeScript + D3.js is the chosen approach
- **Canvas or Phaser.js rendering** — rejected; SVG + DOM is the correct approach for this UI

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|---|---|
| Page load (cold start to interactive) | <1 second |
| `GET /api/roadmap` response time | <50ms at p99 |
| ADO sync (500 work items) | <10 seconds |
| Frontend full re-render | <50ms |
| Backend startup | <500ms |
| Vite HMR update | <500ms |

### Security

| Control | Implementation |
|---|---|
| Network isolation | Kestrel `ListenLocalhost(5000)` — rejects non-localhost connections |
| PAT storage (dev) | .NET User Secrets in `%APPDATA%\Microsoft\UserSecrets\` |
| PAT storage (dist) | DPAPI `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser` |
| PAT via env var | `REPORTINGDASHBOARD_ADO__PAT` |
| No secrets in logs | Serilog destructure policy excludes `Ado:Pat` |
| Source control | `.gitignore` excludes `*.db`, `cred.dat`, sensitive `appsettings.*.json` |
| ADO PAT scope | `Work Items (Read)` only — no write, no code access |
| Database location | `%LOCALAPPDATA%\ReportingDashboard\` — Windows user-scoped ACLs |
| Database encryption | Deferred (SQLCipher); add if security review requires it |

### Reliability

| Requirement | Implementation |
|---|---|
| Offline operation | Dashboard renders from local SQLite; ADO unavailability does not break UI |
| Graceful sync failure | Error message displayed; existing data remains intact |
| Database recovery | Delete corrupted `.db` file and re-sync; WAL mode prevents corruption |
| Empty state handling | Dashboard renders placeholder content when no data exists |

### Scalability (By Design Constraints)

| Dimension | Limit | Notes |
|---|---|---|
| Concurrent users | 1 | Single-user local tool |
| Work items | ~10,000 | SQLite and CSS Grid handle this comfortably |
| Workstreams | ~20 | Limited by 185px SVG timeline height |
| Months displayed | 4–12 | CSS Grid columns adjust dynamically |

---

## Success Metrics

### Phase 1: Static Dashboard
- [ ] `npm run dev` renders the full dashboard from sample JSON matching `OriginalDesignConcept.html` at 1920×1080
- [ ] All three visual sections match the design reference within 2px tolerance
- [ ] No TypeScript compilation errors (`npx tsc --noEmit`)
- [ ] Vite production build succeeds (`npm run build`)

### Phase 2: API + Database
- [ ] `dotnet run` starts the API on localhost:5000 and serves the dashboard
- [ ] `GET /api/roadmap` returns seeded data; dashboard renders from API
- [ ] Swagger UI accessible at `/swagger` in development
- [ ] Database auto-creates on first run with no manual steps

### Phase 3: ADO Sync
- [ ] `POST /api/sync` pulls live work items from the configured ADO area path
- [ ] Work items correctly categorized into Shipped/InProgress/Carryover/Blocked
- [ ] Sync button triggers refresh and shows success/failure feedback
- [ ] PAT never appears in repo or logs

### Phase 4: Polish & Distribution
- [ ] Drill-down panel works for all heatmap cells with ADO links
- [ ] Error and loading states implemented for all failure modes
- [ ] `dotnet test` passes with >70% backend line coverage
- [ ] `npm test` passes with >70% frontend line coverage
- [ ] GitHub Actions CI passes on push
- [ ] `dotnet publish` produces a working single-file EXE <40MB
- [ ] README documents prerequisites, build, PAT setup, and usage

### Business Success (Post-Launch)
| Metric | Target |
|---|---|
| Time to generate roadmap screenshot | <2 minutes (vs. ~30 min baseline) |
| Team adoption | 100% of team leads within 2 weeks |
| Data accuracy | Heatmap matches ADO state within 1 sync cycle |
| Stakeholder acceptance | Dashboard screenshot replaces manual report |

---

## Constraints & Assumptions

### Constraints

| # | Constraint |
|---|---|
| C1 | **Windows-only** — DPAPI and Segoe UI are Windows-specific; macOS/Linux support requires replacing both |
| C2 | **Local-only execution** — no shared hosting; each user runs their own instance |
| C3 | **No cloud services** — zero Azure provisioning; ADO REST API pull is the only external call |
| C4 | **Zero operational cost** — no licenses, hosting fees, or database servers |
| C5 | **.NET 8.0 LTS** — must use .NET 8 (supported through November 2026) |
| C6 | **TypeScript for frontend** — vanilla TS + D3.js; no React/Vue/Angular |
| C7 | **1920×1080 fixed viewport** — optimized for screenshot capture; not responsive |
| C8 | **ADO PAT authentication** — no OAuth flow; user must manually configure a PAT |

### Assumptions

| # | Assumption | Risk if Wrong | Validation |
|---|---|---|---|
| A1 | ADO work items have `System.State`, `System.IterationPath`, and `System.Tags` populated | State mapping fails | Validate with WIQL query in Phase 3 spike |
| A2 | "Carryover" can be detected via tag or iteration heuristic | Items miscategorized | **Requires stakeholder decision (OQ-1) before Phase 3** |
| A3 | ~500 work items is typical sync volume | Rate limiting if >2,000 | Monitor sync duration; batch size is adjustable |
| A4 | Team uses Windows with .NET 8 SDK installed | Tool doesn't run | Self-contained publish bundles runtime |
| A5 | Segoe UI available on all target machines | Font rendering differs | Arial specified as CSS fallback |
| A6 | 4 months is the default heatmap width | Layout doesn't fit needs | Configurable in `appsettings.json` |
| A7 | Single-user, single-instance usage | Data corruption | SQLite WAL mode; documented constraint |
| A8 | On-demand sync is sufficient | Data goes stale | Add auto-refresh timer if requested |

### Open Questions Requiring Stakeholder Decision

| # | Question | Blocks | Proposed Default |
|---|---|---|---|
| OQ-1 | How are ADO items categorized into the four status rows? | Phase 3 | Hybrid: state-based + tag-based + iteration-based |
| OQ-2 | What ADO org/project/area path to sync? | Phase 3 | Configurable in `appsettings.json` |
| OQ-3 | Support historical snapshots? | Phase 2 schema | No — current state only |
| OQ-4 | How many months in the heatmap? | Phase 1 layout | Configurable, default 4 |
| OQ-5 | On-demand vs. auto-refresh? | Frontend | On-demand; add timer later if needed |
| OQ-6 | Will this ever be shared/hosted? | Architecture | No — local only |
| OQ-7 | Include "Export as PNG"? | Frontend | No — use browser screenshot |