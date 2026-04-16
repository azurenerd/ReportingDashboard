# PM Specification: Executive Reporting Dashboard

**Document Version:** 1.0
**Date:** April 16, 2026
**Author:** Program Management
**Status:** Draft
**Stack:** C# .NET 8 / Blazor Server / Local-only

---

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones on a timeline and displays monthly execution status (Shipped, In Progress, Carryover, Blockers) in a color-coded heatmap grid. The dashboard reads all data from a local `data.json` file, requires no authentication or cloud infrastructure, and is optimized for screenshot capture at 1920×1080 resolution for inclusion in PowerPoint decks to executive stakeholders. The design is a direct, data-driven translation of the `OriginalDesignConcept.html` reference template into a Blazor Server application.

---

## Business Goals

1. **Provide executive-ready project visibility** — Deliver a single-page view that communicates project health, milestone progress, and monthly execution status at a glance, suitable for leadership review.
2. **Enable screenshot-friendly reporting** — Produce a pixel-perfect 1920×1080 layout that can be captured and pasted directly into PowerPoint decks without additional formatting or editing.
3. **Minimize operational overhead** — Eliminate the need for databases, authentication, cloud hosting, or third-party dependencies. The entire system runs locally with `dotnet run` and reads from a single JSON file.
4. **Support reuse across projects** — Allow the same dashboard application to report on any project by swapping the `data.json` configuration file, enabling consistent executive reporting across the organization.
5. **Reduce reporting preparation time** — Replace manual slide creation with an automated, data-driven view that updates instantly when `data.json` is edited, cutting reporting prep from hours to minutes.

---

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As an** executive viewer, **I want** to see the project title, organizational context, and a link to the backlog at the top of the page, **so that** I immediately know which project I'm reviewing and can drill into details if needed.

**Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` div with title, subtitle, and legend.

**Acceptance Criteria:**
- [ ] The header displays the project title from `data.json` `title` field in bold 24px font
- [ ] The subtitle (team/workstream/date) renders below the title in 12px gray (#888) text from `data.json` `subtitle` field
- [ ] A clickable "→ ADO Backlog" link appears next to the title, using the URL from `data.json` `backlogUrl` field
- [ ] A legend appears on the right side of the header showing four marker types: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), Now marker (red vertical line)
- [ ] The header is separated from the timeline by a 1px solid #E0E0E0 bottom border

---

### US-2: View Milestone Timeline

**As an** executive viewer, **I want** to see a horizontal timeline showing key milestones (checkpoints, PoC dates, production releases) for each major workstream, **so that** I can understand the project schedule and where we are relative to plan.

**Visual Reference:** Timeline area of `OriginalDesignConcept.html` — `.tl-area` div containing left milestone labels and right SVG timeline.

**Acceptance Criteria:**
- [ ] A left panel (230px wide) displays milestone IDs (M1, M2, M3…) with labels, each color-coded to match their timeline row color from `data.json` `milestones[].color`
- [ ] An SVG timeline renders horizontal lines for each milestone row spanning the configured date range (`timelineStartMonth` to `timelineEndMonth`)
- [ ] Vertical gridlines with month labels (Jan, Feb, Mar…) divide the timeline into monthly segments
- [ ] Checkpoint events render as open circles with a colored stroke matching the milestone color
- [ ] PoC milestone events render as gold (#F4B400) diamond shapes with a drop shadow
- [ ] Production release events render as green (#34A853) diamond shapes with a drop shadow
- [ ] Each event displays a date label positioned above or below the marker to avoid overlap
- [ ] A red dashed vertical "NOW" line (#EA4335) appears at the X position corresponding to `data.json` `currentDate`, with a "NOW" label
- [ ] The timeline section has a fixed height of 196px with #FAFAFA background and a 2px #E8E8E8 bottom border
- [ ] Events with dates outside the visible range still render at the boundary (e.g., a Dec event appears at x=0)

---

### US-3: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked for each month, **so that** I can assess execution velocity and identify risks.

**Visual Reference:** Heatmap grid of `OriginalDesignConcept.html` — `.hm-wrap` and `.hm-grid` divs.

**Acceptance Criteria:**
- [ ] A section title "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" appears above the grid in uppercase 14px bold gray (#888) text
- [ ] The grid displays a header row with "Status" in the corner cell and month names across columns
- [ ] The current month column header is highlighted with amber background (#FFF0D0) and amber text (#C07700), with "◀ Now" appended
- [ ] Four status rows render in order: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each row header displays the category name with an emoji prefix (✅, 🔧, 🔁, 🚫) in uppercase, using the category's accent color
- [ ] Each data cell lists work items as bulleted entries with a 6px colored dot matching the row's accent color
- [ ] Current-month cells use a darker background tint than other months (e.g., `.ship-cell.apr` uses #D8F2DA vs #F0FBF0)
- [ ] Empty cells for future months display a gray dash "-"
- [ ] The grid dynamically adjusts its column count based on the `heatmapMonths` array length in `data.json`
- [ ] The grid fills the remaining vertical space below the timeline (using `flex: 1`)

---

### US-4: Load Dashboard Data from JSON

**As a** report author, **I want** to configure all dashboard content (title, milestones, heatmap items) in a single `data.json` file, **so that** I can update the report by editing one file without touching code.

**Acceptance Criteria:**
- [ ] The application reads `data.json` from the `wwwroot/data/` directory on startup
- [ ] All displayed text, dates, colors, URLs, and work items are sourced from `data.json` — no hardcoded content
- [ ] If `data.json` is missing or malformed, the application logs a clear error message and displays a user-friendly error state
- [ ] Changes to `data.json` are reflected after restarting the application (or refreshing the page with `dotnet watch`)
- [ ] The JSON schema supports: `title`, `subtitle`, `backlogUrl`, `currentDate`, `timelineStartMonth`, `timelineEndMonth`, `milestones[]`, `heatmapMonths[]`, `currentMonth`, `categories[]`

---

### US-5: Capture Dashboard as Screenshot

**As a** report author, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, **so that** I can capture a clean full-page screenshot for my PowerPoint presentation.

**Acceptance Criteria:**
- [ ] The page body is fixed at 1920px wide and 1080px tall with `overflow: hidden`
- [ ] All content fits within the viewport without scrolling when using up to 4 heatmap months and 3 milestone rows
- [ ] The page renders identically in Microsoft Edge and Google Chrome at 1920×1080 device emulation
- [ ] No Blazor framework UI elements (error boundaries, reconnect dialogs) are visible during normal operation
- [ ] Font rendering uses Segoe UI (system font on Windows) with Arial as fallback

---

### US-6: Reuse Dashboard for Different Projects

**As a** report author managing multiple projects, **I want** to swap `data.json` files to report on different projects using the same dashboard, **so that** I maintain a consistent reporting format across my portfolio.

**Acceptance Criteria:**
- [ ] Replacing `data.json` with a different project's data file and refreshing renders the new project's dashboard
- [ ] The dashboard adapts to different numbers of milestones (1–5 rows) without layout breakage
- [ ] The dashboard adapts to different numbers of heatmap months (2–6 columns) without layout breakage
- [ ] Milestone colors, category names, and all text are driven by `data.json` — no project-specific hardcoding
- [ ] The fictional "Phoenix Platform" example `data.json` ships with the project as a reference template

---

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` and `docs/design-screenshots/OriginalDesignConcept.png` from the ReportingDashboard repository. Engineers MUST consult these files and match the visual output exactly.

### Overall Page Layout

- **Dimensions:** Fixed 1920px × 1080px, `overflow: hidden`
- **Background:** #FFFFFF
- **Font Family:** `'Segoe UI', Arial, sans-serif`
- **Base Text Color:** #111
- **Layout Model:** Flexbox column (`display: flex; flex-direction: column`)
- **Three stacked sections:** Header → Timeline → Heatmap Grid (heatmap fills remaining space via `flex: 1`)

### Section 1: Header Bar (`.hdr`)

- **Padding:** 12px 44px 10px
- **Border Bottom:** 1px solid #E0E0E0
- **Layout:** Flexbox row, `align-items: center`, `justify-content: space-between`
- **Left Side:**
  - **Title (h1):** 24px, font-weight 700, color #111. Includes inline anchor link "→ ADO Backlog" in #0078D4 with no underline
  - **Subtitle (`.sub`):** 12px, color #888, margin-top 2px. Format: "Team · Workstream · Month Year"
- **Right Side — Legend Row:**
  - Flexbox row, gap 22px, font-size 12px
  - **PoC Milestone:** 12×12px gold (#F4B400) square rotated 45° (diamond) + label text
  - **Production Release:** 12×12px green (#34A853) square rotated 45° (diamond) + label text
  - **Checkpoint:** 8×8px circle, background #999 + label text
  - **Now Marker:** 2×14px rectangle, background #EA4335 + label "Now (Apr 2026)"

### Section 2: Timeline Area (`.tl-area`)

- **Height:** 196px fixed, flex-shrink 0
- **Background:** #FAFAFA
- **Border Bottom:** 2px solid #E8E8E8
- **Padding:** 6px 44px 0
- **Layout:** Flexbox row, `align-items: stretch`

#### Left Panel — Milestone Labels

- **Width:** 230px, flex-shrink 0
- **Border Right:** 1px solid #E0E0E0
- **Padding:** 16px 12px 16px 0
- **Layout:** Flexbox column, `justify-content: space-around`
- **Each label:** 12px font, font-weight 600 for milestone ID (e.g., "M1"), color matches milestone `color` field. Below: font-weight 400, color #444 for milestone description

#### Right Panel — SVG Timeline (`.tl-svg-box`)

- **Flex:** 1 (fills remaining width, approximately 1560px)
- **Padding:** left 12px, top 6px
- **SVG Viewport:** width 1560, height 185, `overflow: visible`

**SVG Elements:**
- **Month Gridlines:** Vertical lines at equal intervals (260px apart for 6 months), stroke #BBB, opacity 0.4, width 1. Month labels at top: 11px, font-weight 600, color #666
- **NOW Line:** Vertical dashed line (`stroke-dasharray: 5,3`), stroke #EA4335, width 2. "NOW" label: 10px, font-weight 700, color #EA4335
- **Milestone Rows:** Horizontal lines spanning full width, stroke = milestone color, width 3. Spaced at Y offsets: row 0 = y:42, row 1 = y:98, row 2 = y:154 (56px spacing)
- **Checkpoint Markers:** `<circle>` with r=5–7, fill white, stroke = milestone color or #888, stroke-width 2.5
- **PoC Diamonds:** `<polygon>` forming diamond shape (11px radius), fill #F4B400, with drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`)
- **Production Diamonds:** Same shape as PoC, fill #34A853, same drop shadow
- **Date Labels:** `<text>` elements, 10px, color #666, `text-anchor: middle`, positioned above (y - 16) or below (y + 24) markers to avoid overlap

**Coordinate Calculation:**
```
X = ((date - timelineStart).Days / (timelineEnd - timelineStart).Days) × svgWidth
```

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Flex:** 1 (fills remaining vertical space)
- **Padding:** 10px 44px 10px
- **Layout:** Flexbox column

#### Section Title (`.hm-title`)

- Font-size 14px, font-weight 700, color #888
- Letter-spacing 0.5px, text-transform uppercase
- Margin-bottom 8px

#### Grid Container (`.hm-grid`)

- **Layout:** CSS Grid
- **Columns:** `160px repeat(N, 1fr)` where N = number of heatmap months
- **Rows:** `36px repeat(4, 1fr)` — 36px header row + 4 equal data rows
- **Border:** 1px solid #E0E0E0

#### Grid Header Row

| Cell | Class | Background | Font | Border |
|------|-------|-----------|------|--------|
| Corner ("Status") | `.hm-corner` | #F5F5F5 | 11px bold uppercase #999 | right: 1px #E0E0E0, bottom: 2px #CCC |
| Month Column Headers | `.hm-col-hdr` | #F5F5F5 | 16px bold | right: 1px #E0E0E0, bottom: 2px #CCC |
| Current Month Header | `.hm-col-hdr.apr-hdr` | #FFF0D0 | 16px bold #C07700 | same borders |

#### Grid Data Rows — Color System

| Category | Row Header Class | Header BG | Header Text | Cell BG | Current Month Cell BG | Bullet Color |
|----------|-----------------|-----------|-------------|---------|----------------------|-------------|
| **Shipped** | `.ship-hdr` | #E8F5E9 | #1B7A28 | #F0FBF0 | #D8F2DA | #34A853 |
| **In Progress** | `.prog-hdr` | #E3F2FD | #1565C0 | #EEF4FE | #DAE8FB | #0078D4 |
| **Carryover** | `.carry-hdr` | #FFF8E1 | #B45309 | #FFFDE7 | #FFF0B0 | #F4B400 |
| **Blockers** | `.block-hdr` | #FEF2F2 | #991B1B | #FFF5F5 | #FFE4E4 | #EA4335 |

#### Row Headers (`.hm-row-hdr`)

- 11px font, bold, uppercase, letter-spacing 0.7px
- Padding: 0 12px
- Border-right: 2px solid #CCC
- Border-bottom: 1px solid #E0E0E0
- Emoji prefix + category name (e.g., "✅ Shipped")

#### Data Cells (`.hm-cell`)

- Padding: 8px 12px
- Border-right: 1px solid #E0E0E0
- Border-bottom: 1px solid #E0E0E0
- `overflow: hidden`

#### Work Item Entries (`.hm-cell .it`)

- Font-size 12px, color #333
- Padding: 2px 0 2px 12px
- Line-height: 1.35
- **Bullet dot:** `::before` pseudo-element — 6×6px circle, positioned absolute at left:0 top:7px, background color matches category accent

---

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Full Dashboard Render**
User navigates to `https://localhost:5001`. The Blazor Server app loads `data.json`, deserializes it into the data model, and renders the complete dashboard: header with title/subtitle/legend, timeline with milestone rows and markers, and heatmap grid with all status rows populated. The page fits entirely within 1920×1080 with no scrollbars. Total load time is under 2 seconds.

**Scenario 2: User Views the Header and Identifies the Project**
User sees the project title "Phoenix Platform Release Roadmap" in bold at top-left, with the subtitle "Engineering Excellence · Phoenix Workstream · April 2026" below. A "→ ADO Backlog" link appears inline with the title. On the right, the legend shows four marker types (PoC diamond, Production diamond, Checkpoint circle, NOW line) with labels.

**Scenario 3: User Reads the Milestone Timeline**
User scans the timeline area. Three horizontal colored lines represent M1 (API Gateway & Auth, blue), M2 (Data Pipeline v2, teal), and M3 (Dashboard & Reporting, gray-blue). Diamond and circle markers appear at key dates along each line. A red dashed "NOW" vertical line shows the current date position. Month gridlines (Jan–Jun) provide temporal context. Labels next to each marker show the date and event type.

**Scenario 4: User Hovers Over a Milestone Diamond and Sees a Tooltip**
*(Phase 2 enhancement)* User hovers over a gold PoC diamond on the M2 timeline row. A tooltip appears showing "Mar 20 PoC — Data Pipeline v2" with the full date. Moving the mouse away dismisses the tooltip. This is a subtle, non-blocking interaction that does not appear in screenshots.

**Scenario 5: User Examines the Heatmap to Assess April Execution**
User looks at the heatmap grid. The "April" column header is highlighted in amber (#FFF0D0), indicating it is the current month. In the Shipped row (green), they see "Gateway GA release" and "SSO integration." In the In Progress row (blue), they see three items. In Carryover (amber), one lingering item. In Blockers (red), two items flagged. The current-month cells have slightly darker backgrounds than past months.

**Scenario 6: User Identifies a Blocker Requiring Attention**
User scans the Blockers row (red) across all months. March shows "Vendor SDK delay." April shows the same item persisting plus "Infra quota approval." The red color and separated row make blockers immediately visible. The user takes a screenshot to discuss in the next executive review.

**Scenario 7: User Captures a Screenshot for PowerPoint**
User opens Chrome DevTools, sets device emulation to 1920×1080, and captures a full-page screenshot. The resulting PNG is a clean, complete representation of the dashboard with no scrollbars, no browser chrome, and no Blazor framework artifacts. The image pastes directly into a PowerPoint slide at full resolution.

**Scenario 8: Data-Driven Rendering — Fewer Milestones**
A different project has only 2 milestones instead of 3. The user edits `data.json` to include only M1 and M2. On refresh, the timeline renders 2 horizontal rows instead of 3, with appropriate Y spacing. The left label panel shows 2 entries. The layout remains balanced and screenshot-ready.

**Scenario 9: Data-Driven Rendering — More Heatmap Months**
User changes `heatmapMonths` from `["Jan","Feb","Mar","Apr"]` to `["Jan","Feb","Mar","Apr","May","Jun"]`. On refresh, the CSS Grid adds two more columns. Column widths adjust proportionally via `repeat(6, 1fr)`. The current-month highlight moves to the correct column based on `currentMonth`.

**Scenario 10: Empty State — No Items in a Heatmap Cell**
For a future month (e.g., May in the Shipped row), `data.json` has no items. The cell renders a gray dash "—" instead of bullet items, maintaining visual consistency and avoiding empty white space.

**Scenario 11: Error State — Missing or Invalid data.json**
If `data.json` is missing, the `DashboardDataService` logs a descriptive error. The dashboard page displays a centered message: "Unable to load dashboard data. Please ensure data.json exists in wwwroot/data/ and contains valid JSON." No unhandled exception or Blazor error boundary is exposed.

**Scenario 12: Error State — Malformed JSON Field**
If a milestone event has an invalid date format (e.g., "not-a-date"), the service logs a warning for that specific event and skips it. The rest of the dashboard renders normally. No crash or blank page.

**Scenario 13: User Clicks the ADO Backlog Link**
User clicks "→ ADO Backlog" in the header. The browser navigates to the URL specified in `data.json` `backlogUrl` field (e.g., `https://dev.azure.com/contoso/phoenix/_backlogs`). This is a standard anchor tag — no special handling is needed.

**Scenario 14: User Refreshes After Editing data.json (with dotnet watch)**
User is running `dotnet watch`. They edit `data.json` to add a new shipped item in April. They refresh the browser. The dashboard re-reads `data.json` and displays the updated item in the Shipped/April cell. No restart is required.

---

## Scope

### In Scope

- Single-page Blazor Server dashboard matching `OriginalDesignConcept.html` visual design
- Header component with data-driven title, subtitle, backlog link, and legend
- SVG timeline component with milestone rows, date markers (checkpoint circles, PoC diamonds, production diamonds), month gridlines, and "NOW" line
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- Current-month column highlighting with amber accent
- All content driven by a single `data.json` configuration file
- C# record-based data models for JSON deserialization
- `DashboardDataService` as a singleton service reading from `wwwroot/data/data.json`
- Fictional "Phoenix Platform" example data in `data.json`
- Fixed 1920×1080 layout optimized for screenshot capture
- CSS directly adapted from the reference HTML design
- Error handling for missing or malformed `data.json`
- Project scaffolding: `.sln` file, folder structure, stripped default Blazor template

### Out of Scope

- **Authentication/Authorization** — No login, no identity, no cookie auth, no Windows auth
- **Database** — No SQL, SQLite, LiteDB, or any persistent data store beyond `data.json`
- **Responsive design** — The page is fixed at 1920×1080; no mobile or tablet layouts
- **Real-time updates** — No SignalR push, no auto-refresh, no WebSocket data feeds
- **Multi-user support** — Single user running locally; no concurrency concerns
- **Built-in screenshot/export** — Users capture screenshots via browser tools or OS snipping tools
- **Dark mode** — Deferred to Phase 2
- **Multi-project file selector** — Deferred to Phase 2; MVP supports one `data.json` at a time
- **Hover tooltips on timeline** — Deferred to Phase 2
- **Print CSS** — Deferred to Phase 2
- **Animated NOW line pulse** — Deferred to Phase 2
- **CI/CD pipeline** — Not needed for a local-only tool
- **Docker containerization** — Not needed; runs directly via `dotnet run`
- **Cloud deployment** (Azure App Service, etc.) — Explicitly excluded
- **Charting libraries** (BlazorApexCharts, Radzen Charts, etc.) — SVG is hand-coded
- **Unit or component tests** — Optional; not required for MVP delivery
- **Accessibility compliance (WCAG)** — Not a requirement for a screenshot-capture tool
- **Internationalization/Localization** — English only

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| **Page load time** | < 2 seconds from cold start on localhost |
| **data.json parse time** | < 100ms for files up to 50KB |
| **SVG render time** | < 500ms for up to 5 milestone rows with 10 events each |
| **Memory usage** | < 100MB resident (including .NET runtime) |

### Visual Fidelity

| Metric | Target |
|--------|--------|
| **Screenshot resolution** | Exactly 1920×1080 pixels, no scrollbars |
| **Design match** | Pixel-accurate reproduction of `OriginalDesignConcept.html` at 1:1 comparison |
| **Font rendering** | Segoe UI on Windows; Arial fallback on other systems |
| **Browser compatibility** | Microsoft Edge 120+ and Google Chrome 120+ (Chromium-based) |

### Security

| Requirement | Approach |
|-------------|----------|
| **Authentication** | None. Localhost-only, single-user tool |
| **Data sensitivity** | `data.json` contains project metadata only — no PII, no credentials |
| **HTTPS** | Default Kestrel HTTPS on localhost (self-signed cert) |
| **Input validation** | JSON deserialization with `System.Text.Json`; no user-supplied input from the browser |

### Reliability

| Metric | Target |
|--------|--------|
| **Uptime** | N/A — runs on-demand via `dotnet run` |
| **Graceful degradation** | Missing/malformed `data.json` shows an error message, not a crash |
| **Data loss risk** | Zero — the app is read-only; `data.json` is the source of truth |

### Scalability

Not applicable. This is a single-user, local-only, read-only dashboard. The practical limit is the number of work items that fit within 1920×1080 without overflow — approximately 4–6 items per heatmap cell and 3–5 milestone rows.

---

## Success Metrics

| # | Metric | Target | How to Measure |
|---|--------|--------|---------------|
| 1 | **Visual fidelity** | Dashboard screenshot is indistinguishable from `OriginalDesignConcept.html` reference at 1920×1080 | Side-by-side comparison in image viewer; no visible differences in layout, color, or typography |
| 2 | **Data-driven rendering** | 100% of displayed content sourced from `data.json` | Change every field in `data.json` and verify all changes reflected in the UI after refresh |
| 3 | **Zero-config startup** | Running `dotnet run` serves a working dashboard with no additional setup | Fresh clone → `dotnet run` → browser opens → dashboard visible with example data |
| 4 | **Screenshot workflow** | Full-page screenshot captured in < 30 seconds | Time from "open DevTools" to "PNG saved" using Chrome device emulation |
| 5 | **Project reusability** | Dashboard renders correctly with 3 different `data.json` files varying in milestone count (1–5) and month count (2–6) | Swap files and verify layout adapts without breakage |
| 6 | **Error resilience** | Missing `data.json` shows friendly error; malformed dates are skipped gracefully | Delete `data.json` and verify error page; insert invalid date and verify partial render |
| 7 | **Build success** | `dotnet build` completes with zero warnings and zero errors | CI-equivalent local build check |

---

## Constraints & Assumptions

### Technical Constraints

1. **Stack is mandated:** Blazor Server on .NET 8 LTS. No alternative frameworks (React, Angular, plain HTML) are permitted.
2. **Fixed viewport:** 1920×1080 pixels. The layout is not responsive and must not require scrolling.
3. **No external NuGet packages** for production. The entire app runs on the .NET 8 SDK built-in libraries (`System.Text.Json`, `Microsoft.AspNetCore.Components`).
4. **No JavaScript.** All rendering is done via Razor components, CSS, and inline SVG. No JS interop.
5. **Windows-first:** Segoe UI font availability is assumed. The primary development and screenshot capture environment is Windows with Edge or Chrome.
6. **Single CSS file:** Use `wwwroot/css/dashboard.css` adapted verbatim from `OriginalDesignConcept.html`. Do not use Blazor CSS isolation.
7. **No database:** `data.json` in `wwwroot/data/` is the sole data source.

### Timeline Assumptions

8. **MVP delivery:** 4–6 hours of development effort for a senior .NET developer familiar with Blazor.
9. **Phase 2 (enhancements):** 2–3 additional hours if multi-project selector, tooltips, and print CSS are approved.
10. **No design iteration needed:** `OriginalDesignConcept.html` is the approved design. The Blazor app is a direct translation, not a redesign.

### Dependency Assumptions

11. **.NET 8 SDK** is installed on the developer's machine (version 8.0.x LTS).
12. **Visual Studio 2022 17.9+** or **VS Code with C# Dev Kit** is available for development.
13. **Chromium-based browser** (Edge or Chrome) is available for screenshot capture at 1920×1080 device emulation.
14. **The reference file `OriginalDesignConcept.html`** in the ReportingDashboard repository is complete and represents the approved design.
15. **`data.json` schema is stable.** The schema defined in this spec (title, subtitle, backlogUrl, currentDate, timelineStartMonth, timelineEndMonth, milestones[], heatmapMonths[], currentMonth, categories[]) will not change during MVP development.

### Business Assumptions

16. **Single user:** Only the report author runs the dashboard locally. No shared hosting or multi-tenant requirements.
17. **Infrequent updates:** `data.json` is edited manually (weekly or monthly) before executive reviews. Real-time data feeds are not needed.
18. **English only:** All UI text and data content is in English.
19. **No compliance requirements:** No SOC2, GDPR, HIPAA, or FedRAMP considerations apply to this local-only tool.

---

## Appendix: data.json Schema Reference

The following JSON schema defines the contract between the `data.json` configuration file and the Blazor dashboard application. All fields are required unless noted.

```json
{
  "title": "string — Dashboard title displayed in header h1",
  "subtitle": "string — Organizational context line below title",
  "backlogUrl": "string — URL for the ADO Backlog link",
  "currentDate": "string (YYYY-MM-DD) — Date used to position the NOW line",
  "timelineStartMonth": "string (YYYY-MM) — First month on the timeline X axis",
  "timelineEndMonth": "string (YYYY-MM) — Last month on the timeline X axis",
  "milestones": [
    {
      "id": "string — Short identifier (e.g., M1)",
      "label": "string — Descriptive label for the milestone",
      "color": "string — Hex color code for the timeline row",
      "events": [
        {
          "date": "string (YYYY-MM-DD) — Event date",
          "type": "string — One of: checkpoint, poc, production",
          "label": "string — Display label next to the marker"
        }
      ]
    }
  ],
  "heatmapMonths": ["string — Short month names for column headers"],
  "currentMonth": "string — Month name to highlight as current",
  "categories": [
    {
      "name": "string — Category display name (Shipped, In Progress, etc.)",
      "cssClass": "string — CSS class prefix (ship, prog, carry, block)",
      "items": {
        "<monthName>": ["string — Work item descriptions"]
      }
    }
  ]
}
```