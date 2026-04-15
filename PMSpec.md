# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes a project's milestone timeline and monthly execution status (Shipped, In Progress, Carryover, Blockers) in a clean, screenshot-friendly format optimized for 1920×1080 PowerPoint slides. The dashboard reads all data from a local `data.json` configuration file, runs locally via `dotnet run` with zero cloud dependencies, and is built with Blazor Server (.NET 8) using vanilla CSS and inline SVG to faithfully reproduce the approved `OriginalDesignConcept.html` design.

## Business Goals

1. **Provide executive visibility into project health** — Deliver a single-glance view of what has shipped, what is in progress, what carried over, and what is blocked, organized by month.
2. **Eliminate manual slide creation** — Replace hand-built PowerPoint status slides with a data-driven dashboard that can be screenshotted directly into decks, saving 30–60 minutes per reporting cycle.
3. **Enable rapid data updates** — Allow a PM or engineering lead to update project status by editing a single `data.json` file with no code changes, deployments, or database migrations.
4. **Maintain zero operational cost** — Run entirely on the user's local machine with no cloud services, licenses, hosting fees, or authentication infrastructure.
5. **Ensure pixel-perfect screenshot fidelity** — Produce consistent 1920×1080 captures across Edge and Chrome that look professional in executive presentations.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** to see the project title, organizational context, current month, and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are looking at.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` element.

- [ ] Dashboard displays the project title from `data.json` as an H1 heading (24px, bold, `#111`).
- [ ] A clickable "→ ADO Backlog" hyperlink appears inline with the title, styled in `#0078D4`.
- [ ] Subtitle line shows organization, workstream, and current month (12px, `#888`).
- [ ] A legend appears on the right side of the header with four items: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line).
- [ ] All text values are driven by `data.json` fields (`title`, `subtitle`, `backlogUrl`).

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing major project milestones with their dates and status markers, **so that** I can understand the project's trajectory and upcoming deadlines at a glance.

**Visual Reference:** Timeline section of `OriginalDesignConcept.html` — `.tl-area` element and inline `<svg>`.

- [ ] Timeline displays as a horizontal SVG (1560×185px) with vertical month gridlines.
- [ ] Each milestone renders as a colored horizontal swim lane with its label (e.g., "M1") and sublabel (e.g., "API Gateway & Auth") on the left sidebar (230px wide).
- [ ] Checkpoint markers render as outlined circles on the swim lane.
- [ ] PoC milestones render as gold (`#F4B400`) diamond shapes with drop shadow.
- [ ] Production releases render as green (`#34A853`) diamond shapes with drop shadow.
- [ ] A red dashed vertical "NOW" line (`#EA4335`) is positioned based on the current system date relative to the timeline date range.
- [ ] Date labels appear above or below each marker, centered on the marker's X position.
- [ ] The timeline supports 1–5 milestone swim lanes from `data.json`.
- [ ] Month gridlines and labels span the full `timelineStartDate` to `timelineEndDate` range.

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was shipped, what is in progress, what carried over, and what is blocked for each month, **so that** I can assess execution velocity and identify risks.

**Visual Reference:** Heatmap section of `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` elements.

- [ ] Heatmap renders as a CSS Grid with a "Status" corner cell, month column headers, and four status rows.
- [ ] The current month column is visually highlighted with a warm background (`#FFF0D0`) and amber text (`#C07700`) in the header, plus a "◀ Now" indicator.
- [ ] Shipped row uses green tones: header `#E8F5E9`/`#1B7A28`, cells `#F0FBF0`, current month `#D8F2DA`, dot `#34A853`.
- [ ] In Progress row uses blue tones: header `#E3F2FD`/`#1565C0`, cells `#EEF4FE`, current month `#DAE8FB`, dot `#0078D4`.
- [ ] Carryover row uses amber tones: header `#FFF8E1`/`#B45309`, cells `#FFFDE7`, current month `#FFF0B0`, dot `#F4B400`.
- [ ] Blockers row uses red tones: header `#FEF2F2`/`#991B1B`, cells `#FFF5F5`, current month `#FFE4E4`, dot `#EA4335`.
- [ ] Each item within a cell displays with a colored bullet (6px circle) and 12px text.
- [ ] Empty cells display a muted dash character.
- [ ] All items are populated from `data.json` `itemsByMonth` arrays.
- [ ] The number of month columns is driven by the `months` array in `data.json`.

### US-4: Configure Dashboard via JSON

**As a** project lead, **I want** to edit a single `data.json` file to change all dashboard content — title, milestones, status items, and date ranges — **so that** I can reuse the dashboard for different projects or reporting periods without touching code.

- [ ] The application reads `data.json` from a configurable file path (default: `./data.json` relative to content root).
- [ ] The file path is overridable via `appsettings.json` (`DataFilePath` key).
- [ ] All displayed text, dates, milestones, and status items are sourced exclusively from `data.json`.
- [ ] Changing `data.json` and refreshing the browser reflects the new data immediately.
- [ ] Invalid or missing `data.json` displays a clear error message instead of a crash.
- [ ] The JSON schema uses `camelCase` property naming and is deserializable with `System.Text.Json`.

### US-5: Capture Screenshot for PowerPoint

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars or overflow, **so that** I can take a browser screenshot and paste it directly into a PowerPoint slide without cropping or resizing.

- [ ] The page body is fixed at `width: 1920px; height: 1080px; overflow: hidden`.
- [ ] No horizontal or vertical scrollbars appear when viewed at 1920×1080 in browser Device Mode.
- [ ] The layout renders identically in Microsoft Edge and Google Chrome.
- [ ] All fonts use `Segoe UI` (system font, no web font loading required).
- [ ] The screenshot matches the visual quality of the `OriginalDesignConcept.html` reference.

### US-6: Run Dashboard Locally

**As a** developer, **I want** to start the dashboard with a single `dotnet run` command and view it at `localhost:5000`, **so that** there is zero setup friction.

- [ ] `dotnet run` starts the application and binds to `http://localhost:5000`.
- [ ] No database, Docker, cloud service, or authentication setup is required.
- [ ] The project has zero NuGet dependencies beyond the implicit `Microsoft.AspNetCore.App` framework reference.
- [ ] `dotnet watch run` provides hot reload for `.razor` and `.css` file changes.
- [ ] The application can be published as a self-contained single-file executable via `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true`.

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository. See also the rendered screenshot: `docs/design-screenshots/OriginalDesignConcept.png`. All implementations MUST match these visuals exactly.

### Overall Page Layout

- **Viewport:** Fixed 1920×1080 pixels, `overflow: hidden`, white background (`#FFFFFF`).
- **Layout Model:** Flexbox column (`display: flex; flex-direction: column`) for the three stacked sections.
- **Font:** `'Segoe UI', Arial, sans-serif` — system font, no loading required.
- **Base text color:** `#111`.
- **Link color:** `#0078D4`, no underline.

### Section 1: Header Bar (`.hdr`)

- **Height:** Auto (approximately 50px based on content).
- **Padding:** `12px 44px 10px`.
- **Border:** 1px solid `#E0E0E0` bottom.
- **Layout:** Flexbox row, `align-items: center`, `justify-content: space-between`.
- **Left side:**
  - H1: 24px, font-weight 700, color `#111`. Contains project title text and an inline anchor ("→ ADO Backlog") in `#0078D4`.
  - Subtitle div: 12px, color `#888`, margin-top 2px. Contains org path and current month.
- **Right side — Legend:** Flexbox row with 22px gap, four inline items each at 12px font size:
  - PoC Milestone: 12×12px square rotated 45° (`transform: rotate(45deg)`), fill `#F4B400`.
  - Production Release: 12×12px square rotated 45°, fill `#34A853`.
  - Checkpoint: 8×8px circle, fill `#999`.
  - Now indicator: 2×14px vertical bar, fill `#EA4335`, labeled "Now (Apr 2026)".

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed 196px.
- **Background:** `#FAFAFA`.
- **Padding:** `6px 44px 0`.
- **Border:** 2px solid `#E8E8E8` bottom.
- **Layout:** Flexbox row, `align-items: stretch`.

#### Timeline Left Sidebar (230px wide)

- **Width:** 230px, flex-shrink 0.
- **Border:** 1px solid `#E0E0E0` right.
- **Padding:** `16px 12px 16px 0`.
- **Layout:** Flexbox column, `justify-content: space-around`.
- **Each milestone label:** 12px, font-weight 600, line-height 1.4.
  - Label (e.g., "M1") in the milestone's color (e.g., `#0078D4`, `#00897B`, `#546E7A`).
  - Sublabel in font-weight 400, color `#444`.

#### Timeline SVG Area (`.tl-svg-box`)

- **Flex:** 1 (fills remaining width).
- **Padding:** left 12px, top 6px.
- **SVG Dimensions:** 1560×185 pixels, `overflow: visible`.

##### SVG Elements

- **Month gridlines:** Vertical lines at equal intervals across 1560px width. Stroke `#bbb`, opacity 0.4, width 1px. Month label text at `x+5, y=14`, fill `#666`, 11px, font-weight 600, font-family `Segoe UI, Arial`.
- **"NOW" indicator:** Vertical dashed line (`stroke-dasharray: 5,3`), stroke `#EA4335`, width 2px, full height. "NOW" text label at `x+4, y=14`, fill `#EA4335`, 10px, font-weight 700.
- **Milestone swim lanes:** Horizontal lines at Y positions spaced ~56px apart (y=42, y=98, y=154 for 3 milestones). Stroke color matches milestone color, width 3px, spanning full 1560px width.
- **Checkpoint markers:** Circle, radius 5–7px, white fill, colored stroke (milestone color or `#888`), stroke-width 2.5. Small gray dots (`r=4`, fill `#999`) for minor checkpoints.
- **PoC milestone markers:** Diamond polygon (11px radius), fill `#F4B400`, with drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`).
- **Production release markers:** Diamond polygon (11px radius), fill `#34A853`, same drop shadow filter.
- **Date labels:** 10px, fill `#666`, `text-anchor: middle`, font-family `Segoe UI, Arial`. Positioned above (y - 16px) or below (y + 24px) the swim lane to avoid overlap.

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Flex:** 1 (fills remaining vertical space, ~838px).
- **Padding:** `10px 44px 10px`.
- **Layout:** Flexbox column.

#### Heatmap Title

- **Text:** "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers".
- **Style:** 14px, font-weight 700, color `#888`, letter-spacing 0.5px, uppercase, margin-bottom 8px.

#### Heatmap Grid (`.hm-grid`)

- **Layout:** CSS Grid.
- **Columns:** `160px repeat(N, 1fr)` where N = number of months (default 4).
- **Rows:** `36px repeat(4, 1fr)` — 36px header row, then 4 equal data rows.
- **Border:** 1px solid `#E0E0E0` outer.

##### Header Row

- **Corner cell (`.hm-corner`):** Background `#F5F5F5`, 11px bold uppercase text "STATUS" in `#999`, border-right 1px `#E0E0E0`, border-bottom 2px `#CCC`.
- **Month headers (`.hm-col-hdr`):** Background `#F5F5F5`, 16px bold, centered text. Border-right 1px `#E0E0E0`, border-bottom 2px `#CCC`.
- **Current month header (`.apr-hdr`):** Background `#FFF0D0`, text color `#C07700`, with "◀ Now" indicator.

##### Status Rows — Color System

Each row has a **row header** (160px) and **N data cells** (1 per month):

| Row | Header BG | Header Text | Cell BG | Current Month BG | Bullet Color | Emoji |
|-----|-----------|-------------|---------|-------------------|--------------|-------|
| Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` | ✅ |
| In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` | 🔵 |
| Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` | 🟡 |
| Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` | 🔴 |

- **Row header (`.hm-row-hdr`):** 11px bold uppercase, letter-spacing 0.7px, border-right 2px `#CCC`, border-bottom 1px `#E0E0E0`.
- **Data cells (`.hm-cell`):** Padding `8px 12px`, border-right 1px `#E0E0E0`, border-bottom 1px `#E0E0E0`, `overflow: hidden`.
- **Items (`.it`):** 12px, color `#333`, padding `2px 0 2px 12px`, line-height 1.35. Pseudo-element `::before` creates a 6px colored circle bullet at `left: 0, top: 7px`.

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Dashboard Renders with Full Data**
User navigates to `http://localhost:5000`. The dashboard loads and displays the header with project title, subtitle, and legend; the milestone timeline SVG with all markers and the "NOW" line positioned at today's date; and the full heatmap grid populated with status items from `data.json`. The entire page fits within 1920×1080 with no scrollbars.

**Scenario 2: User Views Milestone Timeline**
User looks at the timeline area and sees colored horizontal swim lanes for each milestone (e.g., M1 in blue, M2 in teal, M3 in gray-blue). Diamond markers indicate PoC and Production milestones with date labels. A red dashed "NOW" line shows the current date position. The left sidebar lists milestone labels and sublabels.

**Scenario 3: User Reads the Heatmap to Assess Project Health**
User scans the heatmap from top to bottom. The Shipped row (green) shows completed items by month. The In Progress row (blue) shows active work. The Carryover row (amber) highlights items that slipped from previous months. The Blockers row (red) calls out impediments. The current month column is highlighted with a warm amber header background to draw attention.

**Scenario 4: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. The browser navigates to the URL specified in `data.json` (`backlogUrl`). This link is functional in the browser but inert when the page is captured as a screenshot for PowerPoint.

**Scenario 5: User Takes a Screenshot for PowerPoint**
User opens browser DevTools, sets viewport to 1920×1080 Device Mode, and captures a full-page screenshot. The resulting image is a clean, professional dashboard with no browser chrome, scrollbars, or UI artifacts. The image is pasted directly into a PowerPoint slide at full resolution.

**Scenario 6: User Updates `data.json` and Refreshes**
User edits `data.json` to add a new shipped item, changes the title, or adds a milestone marker. User refreshes the browser. The dashboard re-renders with the updated data. No restart of the application is required.

**Scenario 7: Empty Month Cells**
A month column has no items for a given status row (e.g., no blockers in January). The cell displays a muted dash character (`-` in color `#AAA`) instead of being blank, maintaining visual grid integrity.

**Scenario 8: `data.json` File is Missing or Malformed**
The application cannot find `data.json` at the configured path, or the file contains invalid JSON. Instead of crashing or showing a blank page, the dashboard displays a centered error message: "Unable to load dashboard data. Please check that data.json exists and contains valid JSON." The error is also logged to the console.

**Scenario 9: Many Items Overflow a Heatmap Cell**
A status cell contains more items than can fit vertically (e.g., 8+ items in a single month). The cell's `overflow: hidden` CSS truncates the content visually. Items are rendered in order from `data.json`; the most important items should be listed first by the data author.

**Scenario 10: Single Milestone vs. Multiple Milestones**
When `data.json` contains only 1 milestone, the timeline renders a single swim lane centered vertically. When 3–5 milestones are present, swim lanes are evenly spaced within the 185px SVG height. The left sidebar adjusts with `justify-content: space-around`.

**Scenario 11: Hover Over ADO Backlog Link**
User hovers over the "→ ADO Backlog" link. The link text shows the default browser hover cursor (pointer). No tooltip or special hover effect is required — this is a simple hyperlink.

**Scenario 12: Browser Resize Behavior**
User resizes the browser window smaller than 1920×1080. The page does not reflow or respond — it remains fixed at 1920×1080 and the browser shows scrollbars as needed. This is intentional; the dashboard is a screenshot tool, not a responsive web app.

## Scope

### In Scope

- Single-page Blazor Server dashboard matching `OriginalDesignConcept.html` visual design
- Header section with configurable title, subtitle, backlog URL, and legend
- SVG milestone timeline with configurable swim lanes, markers (checkpoint/PoC/production), date labels, and auto-calculated "NOW" line
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) and configurable month columns
- `data.json` file as the sole data source for all dashboard content
- C# record-based data models for JSON deserialization
- `DashboardDataService` to read and deserialize `data.json`
- `dashboard.css` stylesheet ported from the original HTML design with CSS custom properties
- Blazor component decomposition: `Header.razor`, `TimelineSvg.razor`, `HeatmapGrid.razor`, `HeatmapRow.razor`
- Configurable `data.json` file path via `appsettings.json`
- Error handling for missing or malformed `data.json`
- Fictional sample data for "Project Phoenix" demonstrating all features
- xUnit + bUnit unit tests for `DashboardDataService` and key components
- `README.md` documenting setup, usage, data schema, and screenshot capture workflow
- Self-contained single-file executable publish support

### Out of Scope

- **Authentication and authorization** — No login, no RBAC, no Azure AD integration
- **Database or ORM** — No SQLite, EF Core, or any persistent storage beyond `data.json`
- **Real-time data sync** — No live connection to ADO, Jira, or any project management tool
- **Responsive/mobile layout** — Fixed 1920×1080 only; no breakpoints or media queries
- **Dark mode or theme switching** — Single light theme matching the design reference
- **Multi-project support** — Single `data.json` file; no project picker or routing
- **Automated screenshot capture** — No Playwright or Puppeteer integration
- **Deployment infrastructure** — No Docker, no Azure App Service, no CI/CD pipeline
- **Internationalization (i18n)** — English only
- **Accessibility (a11y)** — Not a priority for a screenshot-capture tool; may be added later
- **Print stylesheet** — Not needed; screenshots are the distribution mechanism
- **JavaScript interop** — All rendering is server-side Razor and CSS; no JS libraries
- **CSS framework or component library** — No Bootstrap, Tailwind, MudBlazor, or Radzen

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Page load time (cold start) | < 2 seconds on localhost |
| Page load time (warm) | < 500ms on localhost |
| `data.json` deserialization | < 50ms for files up to 100KB |
| Memory footprint | < 100MB working set |

### Security

- No authentication required — local-only tool.
- No PII or secrets stored in `data.json`.
- HTTP only on `localhost:5000` — no HTTPS certificate management needed.
- If sensitive project names are a concern, `data.json` should be added to `.gitignore` and distributed separately.

### Reliability

- The application must start successfully with `dotnet run` on any machine with .NET 8 SDK installed.
- The application must handle missing/malformed `data.json` gracefully with a user-visible error message (no unhandled exceptions or blank pages).
- The application must render consistently across Microsoft Edge and Google Chrome at 1920×1080.

### Scalability

- Not applicable — single-user, local-only tool.
- `data.json` is expected to contain < 200 items total across all status categories.

### Maintainability

- Zero external NuGet dependencies in the production project.
- CSS custom properties for all color tokens to enable easy palette changes.
- Component decomposition to keep each `.razor` file under 100 lines.
- All data-driven — no hardcoded project content in Razor templates.

## Success Metrics

| # | Metric | Target | Measurement |
|---|--------|--------|-------------|
| 1 | **Visual fidelity** | Dashboard screenshot is indistinguishable from `OriginalDesignConcept.html` reference at 1920×1080 | Side-by-side comparison in Edge and Chrome |
| 2 | **Data-driven rendering** | 100% of displayed text comes from `data.json` — zero hardcoded project content | Code review of `.razor` files |
| 3 | **Setup friction** | New user can run dashboard in < 2 minutes with only `dotnet run` | Timed walkthrough by a developer unfamiliar with the project |
| 4 | **Update cycle time** | Editing `data.json` and refreshing browser takes < 30 seconds to see changes | Timed task |
| 5 | **Screenshot quality** | Exported 1920×1080 screenshot is presentation-ready with no artifacts | Paste into PowerPoint and verify on projector/screen share |
| 6 | **Test coverage** | `DashboardDataService` has unit tests covering: valid data, missing file, malformed JSON, empty arrays | `dotnet test` passes with all scenarios covered |
| 7 | **Zero-dependency build** | `dotnet build` succeeds with no NuGet restore beyond framework references | Build log verification |

## Constraints & Assumptions

### Technical Constraints

- **Runtime:** .NET 8 SDK required on the developer's machine.
- **Framework:** Blazor Server (chosen for hot reload and .NET developer experience; SignalR overhead accepted as negligible for local use).
- **Viewport:** Fixed 1920×1080 — the layout is not responsive and must not reflow.
- **Font:** Segoe UI system font — available on Windows by default; other OS users may see Arial fallback.
- **Browser support:** Microsoft Edge and Google Chrome only (latest stable versions).
- **SVG coordinate system:** Timeline SVG is 1560×185px; all marker positions are computed as normalized values (0.0–1.0) multiplied by SVG width at render time.
- **CSS Grid:** Heatmap column count must match the `months` array length in `data.json`.

### Timeline Assumptions

- **Phase 1 (MVP):** ~4 hours — Solution structure, data model, CSS port, single-page dashboard.
- **Phase 2 (Refactor):** ~2 hours — Component extraction, data-driven month/timeline span, CSS custom properties.
- **Phase 3 (Polish):** ~2 hours — Unit tests, edge case data, README, publish configuration.
- **Total estimated effort:** ~8 hours for a single developer.

### Dependency Assumptions

- The `OriginalDesignConcept.html` file in the ReportingDashboard repository is the approved and final visual design. Any design changes require PM sign-off before implementation.
- The `data.json` schema defined in this specification is stable. Schema changes require corresponding updates to the C# data model records.
- The dashboard will be used by 1–3 people on their local machines. There is no plan for shared hosting or multi-user access.
- The user will manually capture screenshots using browser DevTools Device Mode (1920×1080). Automated screenshot tooling (Playwright) may be considered in a future iteration.

### Open Decisions (Defaults Applied)

| # | Decision | Default | Change Requires |
|---|----------|---------|-----------------|
| 1 | Number of heatmap month columns | Configurable via `data.json` `months` array (default: 4) | `data.json` edit |
| 2 | Timeline date range | Driven by `timelineStartDate` / `timelineEndDate` in `data.json` | `data.json` edit |
| 3 | "NOW" line positioning | Auto-calculated from `DateTime.Now` relative to timeline range | Code change to override |
| 4 | Multi-project support | Single `data.json` file | Future PM spec if needed |
| 5 | ADO Backlog link behavior | Functional hyperlink from `backlogUrl` in `data.json` | `data.json` edit |