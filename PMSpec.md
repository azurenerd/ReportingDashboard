# PM Specification: Executive Reporting Dashboard

**Document Version:** 1.0
**Date:** April 16, 2026
**Author:** Program Management
**Status:** Draft

---

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes a project's milestone timeline, shipping progress, active work, carryover items, and blockers in a fixed 1920×1080 layout optimized for PowerPoint screenshot capture. The dashboard reads all display data from a local `data.json` file, enabling any team member to update project status by editing JSON and taking a browser screenshot—no database, authentication, or deployment infrastructure required.

---

## Business Goals

1. **Reduce executive reporting prep time by 80%** — Replace manual PowerPoint slide construction with a single JSON edit and browser screenshot workflow.
2. **Standardize project status communication** — Provide a consistent, professional visual format for milestone timelines and monthly execution heatmaps across all executive presentations.
3. **Increase project transparency** — Give executives immediate visual clarity on what shipped, what's in progress, what carried over, and what's blocked, organized by month.
4. **Enable rapid iteration** — Allow the project lead to update status data in under 2 minutes by editing a JSON file, with the dashboard reflecting changes instantly.
5. **Zero infrastructure cost** — Run entirely on a developer's local machine with no cloud services, hosting fees, or license costs beyond the existing .NET SDK.

---

## User Stories & Acceptance Criteria

### US-1: View Project Header and Identity

**As a** project lead, **I want** to see the project title, subtitle (team/workstream/date), and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they are viewing.

**Acceptance Criteria:**
- [ ] The header displays the `title` field from `data.json` as a bold 24px heading
- [ ] The subtitle displays below the title in 12px gray text showing team, workstream, and current month
- [ ] An optional backlog URL renders as a clickable hyperlink (`→ ADO Backlog`) next to the title
- [ ] If `backlogUrl` is null or empty, the link is hidden without layout shift
- [ ] **Visual Reference:** Header section of `OriginalDesignConcept.html` — `.hdr` class

### US-2: View Milestone Timeline Legend

**As an** executive viewer, **I want** to see a legend explaining the timeline symbols (PoC Milestone, Production Release, Checkpoint, Now marker), **so that** I can interpret the timeline without additional explanation.

**Acceptance Criteria:**
- [ ] Legend appears in the top-right of the header bar
- [ ] Four legend items displayed horizontally with 22px gaps: gold diamond (PoC Milestone), green diamond (Production Release), gray circle (Checkpoint), red vertical line (Now)
- [ ] Legend symbols exactly match the SVG shapes used in the timeline
- [ ] **Visual Reference:** Header legend area of `OriginalDesignConcept.html` — right side of `.hdr`

### US-3: View Milestone Timeline with Tracks

**As an** executive viewer, **I want** to see a horizontal timeline with multiple milestone tracks (one per major workstream), each showing checkpoints, PoC milestones, and production releases positioned by date, **so that** I can understand the project's temporal plan at a glance.

**Acceptance Criteria:**
- [ ] The timeline area is 196px tall with a light gray (`#FAFAFA`) background
- [ ] A left sidebar (230px wide) lists each milestone track with its ID (e.g., "M1") and label, color-coded to match the track line
- [ ] Each track renders as a horizontal colored line spanning the full SVG width (1560px)
- [ ] Month gridlines appear as vertical lines with month labels (Jan–Jun) at evenly spaced intervals
- [ ] Checkpoint events render as open circles (white fill, colored stroke) on their track line at the correct date position
- [ ] PoC milestones render as gold (`#F4B400`) diamond shapes with drop shadow
- [ ] Production releases render as green (`#34A853`) diamond shapes with drop shadow
- [ ] Each event has a date label positioned above or below the shape
- [ ] A red dashed vertical "NOW" line appears at the current month position with a "NOW" label
- [ ] All X positions are calculated from event dates relative to the timeline date range
- [ ] **Visual Reference:** Timeline section of `OriginalDesignConcept.html` — `.tl-area` and embedded SVG

### US-4: View Monthly Execution Heatmap

**As an** executive viewer, **I want** to see a grid showing Shipped, In Progress, Carryover, and Blocker items organized by month, **so that** I can quickly assess execution velocity and identify problems.

**Acceptance Criteria:**
- [ ] The heatmap fills the remaining vertical space below the timeline
- [ ] A section title reads "MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS" in uppercase gray text
- [ ] The grid uses CSS Grid: first column 160px (row headers), remaining columns equal width (one per month)
- [ ] Column headers show month names; the current month column is highlighted with gold background (`#FFF0D0`) and text (`#C07700`)
- [ ] Four data rows with distinct color coding:
  - **Shipped** (✓): Header `#1B7A28` on `#E8F5E9`; cells `#F0FBF0` (current month: `#D8F2DA`); bullet `#34A853`
  - **In Progress** (→): Header `#1565C0` on `#E3F2FD`; cells `#EEF4FE` (current month: `#DAE8FB`); bullet `#0078D4`
  - **Carryover** (⟳): Header `#B45309` on `#FFF8E1`; cells `#FFFDE7` (current month: `#FFF0B0`); bullet `#F4B400`
  - **Blockers** (✕): Header `#991B1B` on `#FEF2F2`; cells `#FFF5F5` (current month: `#FFE4E4`); bullet `#EA4335`
- [ ] Each cell lists work items as 12px text with a colored bullet dot (6px circle) to the left
- [ ] Empty future-month cells display a gray dash ("–")
- [ ] **Visual Reference:** Heatmap section of `OriginalDesignConcept.html` — `.hm-wrap`, `.hm-grid`

### US-5: Update Dashboard via JSON File

**As a** project lead, **I want** to edit a `data.json` file to change the project title, milestones, and heatmap items, **so that** I can update the dashboard without touching code.

**Acceptance Criteria:**
- [ ] All rendered content is driven by `data.json` — no hardcoded project data in code
- [ ] The JSON schema supports: `title`, `subtitle`, `backlogUrl`, `timelineMonths`, `currentMonth`, `milestones[]` (with events), and `heatmap` (with rows and items per month)
- [ ] Changing a value in `data.json` and refreshing the browser reflects the update
- [ ] Malformed JSON displays a clear error message instead of a blank page or crash
- [ ] A sample `data.json` with fictional project data ships with the application

### US-6: Capture Screenshot for PowerPoint

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, **so that** I can take a full-page browser screenshot that pastes cleanly into a 16:9 PowerPoint slide.

**Acceptance Criteria:**
- [ ] The root element is fixed at `width: 1920px; height: 1080px; overflow: hidden`
- [ ] All content fits within the viewport with no scrolling required
- [ ] The page renders identically in Chrome and Edge at 100% zoom
- [ ] No browser-injected UI elements (scrollbars, tooltips) appear in the screenshot area
- [ ] **Visual Reference:** Full page layout of `OriginalDesignConcept.html` — `body` styles

---

## Visual Design Specification

**Primary Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository
**Secondary Design Reference:** `C:/Pics/ReportingDashboardDesign.png`
**Rendered Preview:** `docs/design-screenshots/OriginalDesignConcept.png`

### Page Layout (Top-Level)

The page is a **vertical flex column** (`display: flex; flex-direction: column`) at a fixed resolution of **1920×1080 pixels** with `overflow: hidden`. There are three stacked sections:

| Section | Height | Background | CSS Class |
|---------|--------|------------|-----------|
| Header Bar | ~48px (auto) | `#FFFFFF` | `.hdr` |
| Timeline Area | 196px (fixed) | `#FAFAFA` | `.tl-area` |
| Heatmap Area | Remaining (flex: 1) | `#FFFFFF` | `.hm-wrap` |

### Section 1: Header Bar (`.hdr`)

- **Layout:** Flexbox, `align-items: center`, `justify-content: space-between`
- **Padding:** `12px 44px 10px`
- **Border:** Bottom `1px solid #E0E0E0`
- **Left side:**
  - Title: `<h1>` at `font-size: 24px; font-weight: 700; color: #111`
  - Backlog link: Inline `<a>` at `color: #0078D4; text-decoration: none`
  - Subtitle: `<div>` at `font-size: 12px; color: #888; margin-top: 2px`
- **Right side (Legend):**
  - Horizontal flex row with `gap: 22px`
  - Each legend item: flex row with `gap: 6px; font-size: 12px`
  - PoC Milestone: 12×12px square, `background: #F4B400`, rotated 45° (diamond)
  - Production Release: 12×12px square, `background: #34A853`, rotated 45° (diamond)
  - Checkpoint: 8×8px circle, `background: #999`
  - Now marker: 2×14px rectangle, `background: #EA4335`

### Section 2: Timeline Area (`.tl-area`)

- **Layout:** Flexbox horizontal, `align-items: stretch`
- **Height:** 196px fixed, `flex-shrink: 0`
- **Padding:** `6px 44px 0`
- **Border:** Bottom `2px solid #E8E8E8`
- **Background:** `#FAFAFA`

**Left Sidebar (Milestone Labels):**
- Width: 230px, `flex-shrink: 0`
- Flex column, `justify-content: space-around`
- Padding: `16px 12px 16px 0`
- Border right: `1px solid #E0E0E0`
- Each label: `font-size: 12px; font-weight: 600; line-height: 1.4`
- ID (e.g., "M1") in track color; description in `color: #444; font-weight: 400`

**SVG Timeline (`.tl-svg-box`):**
- `flex: 1`, padded `12px left, 6px top`
- SVG viewport: `width="1560" height="185"`, `overflow: visible`
- **Drop shadow filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`
- **Month gridlines:** Vertical lines at 260px intervals, `stroke: #bbb; stroke-opacity: 0.4; stroke-width: 1`
- **Month labels:** `font-size: 11; font-weight: 600; fill: #666; font-family: Segoe UI, Arial`
- **NOW line:** Vertical dashed line, `stroke: #EA4335; stroke-width: 2; stroke-dasharray: 5,3`
- **NOW label:** `font-size: 10; font-weight: 700; fill: #EA4335`
- **Track lines:** Horizontal, `stroke-width: 3`, color from milestone data
- **Checkpoint circles:** `r="5–7"`, white fill, colored stroke, `stroke-width: 2.5`
- **Small dots:** `r="4"`, `fill: #999` (minor checkpoints)
- **PoC diamonds:** `<polygon>` with 4 points forming an 11px diamond, `fill: #F4B400`, with drop shadow
- **Production diamonds:** Same shape, `fill: #34A853`, with drop shadow
- **Event labels:** `font-size: 10; fill: #666; text-anchor: middle; font-family: Segoe UI, Arial`

### Section 3: Heatmap Area (`.hm-wrap`)

- **Layout:** Flex column, `flex: 1; min-height: 0`
- **Padding:** `10px 44px 10px`

**Section Title (`.hm-title`):**
- `font-size: 14px; font-weight: 700; color: #888`
- `letter-spacing: 0.5px; text-transform: uppercase`
- `margin-bottom: 8px; flex-shrink: 0`

**Grid (`.hm-grid`):**
- `flex: 1; min-height: 0`
- CSS Grid: `grid-template-columns: 160px repeat(4, 1fr)`
- `grid-template-rows: 36px repeat(4, 1fr)`
- `border: 1px solid #E0E0E0`

**Corner Cell (`.hm-corner`):**
- `background: #F5F5F5; font-size: 11px; font-weight: 700; color: #999; text-transform: uppercase`
- `border-right: 1px solid #E0E0E0; border-bottom: 2px solid #CCC`

**Column Headers (`.hm-col-hdr`):**
- `font-size: 16px; font-weight: 700; background: #F5F5F5`
- `border-right: 1px solid #E0E0E0; border-bottom: 2px solid #CCC`
- Current month override (`.apr-hdr`): `background: #FFF0D0; color: #C07700`

**Row Headers (`.hm-row-hdr`):**
- `font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.7px`
- `padding: 0 12px; border-right: 2px solid #CCC; border-bottom: 1px solid #E0E0E0`
- Color per category (text color / background):
  - Shipped: `#1B7A28` / `#E8F5E9`
  - In Progress: `#1565C0` / `#E3F2FD`
  - Carryover: `#B45309` / `#FFF8E1`
  - Blockers: `#991B1B` / `#FEF2F2`

**Data Cells (`.hm-cell`):**
- `padding: 8px 12px; border-right: 1px solid #E0E0E0; border-bottom: 1px solid #E0E0E0; overflow: hidden`
- Background per category (normal / current month accent):
  - Shipped: `#F0FBF0` / `#D8F2DA`
  - In Progress: `#EEF4FE` / `#DAE8FB`
  - Carryover: `#FFFDE7` / `#FFF0B0`
  - Blockers: `#FFF5F5` / `#FFE4E4`

**Item Text (`.hm-cell .it`):**
- `font-size: 12px; color: #333; padding: 2px 0 2px 12px; line-height: 1.35`
- `::before` pseudo-element: 6×6px circle, `position: absolute; left: 0; top: 7px`
- Bullet color per category: Shipped `#34A853`, In Progress `#0078D4`, Carryover `#F4B400`, Blockers `#EA4335`

### Typography

| Element | Font | Size | Weight | Color |
|---------|------|------|--------|-------|
| Page title | Segoe UI, Arial, sans-serif | 24px | 700 | `#111` |
| Subtitle | Segoe UI | 12px | 400 | `#888` |
| Legend items | Segoe UI | 12px | 400 | `#111` |
| Milestone labels | Segoe UI | 12px | 600 | Track color |
| SVG month labels | Segoe UI, Arial | 11px | 600 | `#666` |
| SVG event labels | Segoe UI, Arial | 10px | 400 | `#666` |
| Heatmap section title | Segoe UI | 14px | 700 | `#888` |
| Column headers | Segoe UI | 16px | 700 | `#111` (or `#C07700` for current month) |
| Row headers | Segoe UI | 11px | 700 | Category color |
| Cell items | Segoe UI | 12px | 400 | `#333` |

### Complete Color Palette

| Token | Hex | Usage |
|-------|-----|-------|
| Page background | `#FFFFFF` | Body background |
| Primary text | `#111` | Title, default text |
| Secondary text | `#888` | Subtitle, section titles |
| Tertiary text | `#666` | SVG labels |
| Cell text | `#333` | Heatmap items |
| Link blue | `#0078D4` | Hyperlinks, M1 track, In Progress bullet |
| Timeline BG | `#FAFAFA` | Timeline area background |
| Grid header BG | `#F5F5F5` | Column/corner header cells |
| Border light | `#E0E0E0` | Cell borders, header bottom |
| Border medium | `#CCC` | Row/column header separators |
| Border heavy | `#E8E8E8` | Timeline bottom border |
| Current month BG | `#FFF0D0` | Highlighted column header |
| Current month text | `#C07700` | Highlighted column header text |
| Shipped green | `#34A853` | Bullet, production diamond |
| Shipped header | `#1B7A28` / `#E8F5E9` | Row header text/bg |
| Shipped cell | `#F0FBF0` / `#D8F2DA` | Normal/accent cell bg |
| Progress blue | `#0078D4` | Bullet |
| Progress header | `#1565C0` / `#E3F2FD` | Row header text/bg |
| Progress cell | `#EEF4FE` / `#DAE8FB` | Normal/accent cell bg |
| Carryover amber | `#F4B400` | Bullet, PoC diamond |
| Carryover header | `#B45309` / `#FFF8E1` | Row header text/bg |
| Carryover cell | `#FFFDE7` / `#FFF0B0` | Normal/accent cell bg |
| Blocker red | `#EA4335` | Bullet, NOW line |
| Blocker header | `#991B1B` / `#FEF2F2` | Row header text/bg |
| Blocker cell | `#FFF5F5` / `#FFE4E4` | Normal/accent cell bg |
| M2 track | `#00897B` | Teal track color |
| M3 track | `#546E7A` | Blue-gray track color |
| Gridline | `#BBB` (40% opacity) | SVG month gridlines |
| Minor checkpoint | `#999` | Small dot fill |

---

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `https://localhost:5001`. The application reads `data.json`, deserializes it into the data model, and renders the full dashboard within the 1920×1080 viewport. The header, timeline, and heatmap all appear simultaneously with no loading spinner or progressive rendering. All milestone positions are computed from dates; all heatmap cells are populated from JSON data.

**Scenario 2: User Views the Project Header**
User sees the project title in bold 24px text at the top-left (e.g., "Privacy Automation Release Roadmap") with an optional "→ ADO Backlog" hyperlink. Below the title, a subtitle shows the team, workstream, and current reporting month. On the right side, the legend shows four symbol explanations: gold diamond (PoC), green diamond (Production), gray circle (Checkpoint), and red line (Now).

**Scenario 3: User Reads the Milestone Timeline**
User looks at the middle section and sees a left sidebar listing milestone track IDs (M1, M2, M3) with descriptions color-coded by track. To the right, an SVG timeline spans January through June with vertical month gridlines. Each track is a horizontal colored line with events positioned at their calendar dates. A dashed red "NOW" line marks the current date. The user can visually assess which milestones are past, current, or upcoming.

**Scenario 4: User Hovers Over a Milestone Diamond**
The milestone diamonds and event labels are rendered as static SVG elements. In this initial version, no interactive hover tooltips are implemented—the date labels adjacent to each shape provide the necessary context. (Future enhancement: SVG `<title>` elements for native browser tooltips.)

**Scenario 5: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" hyperlink in the header. The browser navigates to the URL specified in `data.json`'s `backlogUrl` field, opening the Azure DevOps backlog in a new context. If `backlogUrl` is null, no link is rendered.

**Scenario 6: User Scans the Monthly Execution Heatmap**
User looks at the bottom section and sees a 5-column grid (Status + 4 months). The current month column is highlighted with a gold header background. Four rows show categorized items: green (Shipped), blue (In Progress), amber (Carryover), red (Blockers). Each item appears as a bulleted text line. The user can quickly count items per category per month to assess velocity trends.

**Scenario 7: Data-Driven Rendering — User Edits `data.json`**
User opens `data.json` in a text editor, adds a new item to the "shipped" category for April, saves the file, and refreshes the browser. The heatmap now shows the new item in the Shipped/April cell with the correct green bullet. No code changes are needed.

**Scenario 8: Empty State — No Items in a Month**
A heatmap cell for a future month with no items displays a single gray dash ("–") in `color: #AAA`. The cell retains its category background color but is otherwise visually quiet, signaling "no data yet" without breaking the grid layout.

**Scenario 9: Error State — Malformed or Missing `data.json`**
If `data.json` is missing, unreadable, or contains invalid JSON, the dashboard displays a centered error message on a white background: "Unable to load dashboard data. Please check data.json." The page does not crash, show a stack trace, or render a partial/broken layout.

**Scenario 10: Error State — Missing Required Fields**
If `data.json` is valid JSON but missing required fields (e.g., no `title` or no `milestones` array), the dashboard renders with sensible defaults: empty string for missing text fields, empty timeline for missing milestones, empty heatmap for missing rows. A console warning logs which fields are missing.

**Scenario 11: Screenshot Capture Workflow**
User opens the dashboard in Chrome/Edge at 1920×1080 with 100% zoom, right-clicks, and selects "Take Screenshot" or uses a screenshot tool. The captured image is exactly 1920×1080 with no scrollbars, no browser chrome in the content area, and all text rendered crisply. The user pastes the image directly into a PowerPoint slide set to 16:9 aspect ratio.

**Scenario 12: Multiple Milestone Tracks**
The timeline supports 1–5 milestone tracks. Track Y-positions are evenly distributed across the 185px SVG height. With 3 tracks (as in the reference design), tracks appear at approximately Y=42, Y=98, and Y=154. Adding a 4th or 5th track recalculates spacing proportionally.

---

## Scope

### In Scope

- Single-page Blazor Server application rendering at 1920×1080 fixed resolution
- Header bar with project title, subtitle, optional ADO backlog link, and legend
- SVG milestone timeline with date-computed positioning for checkpoints, PoC milestones, and production releases
- Monthly execution heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) and color-coded cells
- Current month visual highlighting (gold column header, accent cell backgrounds)
- "NOW" date marker on the timeline as a red dashed vertical line
- All content driven by `data.json` file — no hardcoded project data
- Sample `data.json` with realistic fictional project data
- Strongly-typed C# record models for JSON deserialization
- `DashboardDataService` for reading and caching `data.json`
- CSS ported from `OriginalDesignConcept.html` with CSS custom properties for theming
- Graceful error handling for missing or malformed `data.json`
- Documentation of the screenshot capture procedure

### Out of Scope

- ❌ Authentication or authorization of any kind
- ❌ Database or ORM integration
- ❌ Admin UI or web-based data editing form
- ❌ Responsive or mobile layout
- ❌ REST API endpoints
- ❌ Logging infrastructure beyond default console output
- ❌ CI/CD pipeline or automated builds
- ❌ Docker containerization
- ❌ Unit tests or integration tests (optional; low ROI for this scope)
- ❌ Multi-user concurrency or real-time collaboration
- ❌ Historical dashboard snapshots or versioning
- ❌ Automated screenshot capture tooling
- ❌ Interactive tooltips, click-to-drill-down, or animations
- ❌ Multiple project support or project selector
- ❌ Cloud deployment or hosting
- ❌ Print stylesheet (deferred to polish phase if time permits)

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Page load time (cold start) | < 3 seconds from `dotnet run` to rendered page |
| Page load time (warm) | < 500ms on browser refresh |
| JSON deserialization | < 50ms for a `data.json` up to 100KB |
| SVG rendering | < 200ms for up to 5 tracks with 20 events each |

### Screenshot Fidelity

| Metric | Target |
|--------|--------|
| Viewport dimensions | Exactly 1920×1080px, no scrollbars |
| Browser compatibility | Chrome 120+ and Edge 120+ at 100% zoom |
| Font rendering | Segoe UI system font, no web font loading delay |
| SVG rendering | All shapes (line, circle, polygon, text, feDropShadow) render correctly |

### Security

| Requirement | Approach |
|-------------|----------|
| Authentication | None — local-only tool |
| Data protection | `data.json` should be `.gitignore`'d if it contains sensitive project names |
| Network exposure | Kestrel binds to `localhost` only; not accessible from other machines by default |
| PII handling | No PII expected; no encryption needed |

### Reliability

| Requirement | Target |
|-------------|--------|
| Error handling | Malformed `data.json` shows user-friendly error, not a crash |
| Availability | Runs on-demand via `dotnet run`; no uptime SLA needed |
| Data integrity | Read-only application; cannot corrupt `data.json` |

### Maintainability

| Requirement | Target |
|-------------|--------|
| Total file count | ≤ 10 files (excluding `bin/`, `obj/`, `.sln`) |
| External NuGet dependencies | Zero beyond default Blazor Server template |
| Code complexity | No abstractions beyond Model/Service/Page pattern |

---

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Dashboard renders from JSON** | All visible content driven by `data.json` with zero hardcoded project data | Manual verification: change every field in `data.json` and confirm all changes render |
| 2 | **Screenshot parity** | Dashboard screenshot at 1920×1080 is visually indistinguishable from `OriginalDesignConcept.html` reference | Side-by-side visual comparison of screenshot vs. reference PNG |
| 3 | **Update turnaround time** | Project lead can update `data.json` and capture a new screenshot in under 2 minutes | Timed walkthrough with a non-technical user |
| 4 | **Error resilience** | Missing or malformed `data.json` produces a friendly error message, not a crash | Test with empty file, invalid JSON, and missing required fields |
| 5 | **Zero-dependency simplicity** | Application builds and runs with `dotnet build && dotnet run` with no additional setup | Fresh clone on a machine with only .NET 8 SDK installed |
| 6 | **Executive readability** | An executive unfamiliar with the project can identify shipped items, blockers, and upcoming milestones within 10 seconds of viewing | Informal usability check with 2–3 stakeholders |

---

## Constraints & Assumptions

### Technical Constraints

1. **Runtime:** .NET 8.0 LTS SDK must be installed on the developer's machine
2. **Browser:** Chrome or Edge required for screenshot capture; Firefox/Safari not targeted
3. **Resolution:** The dashboard is designed exclusively for 1920×1080 at 100% zoom; other resolutions will produce layout issues
4. **Font dependency:** Segoe UI must be available on the host OS (pre-installed on Windows; may require installation on macOS/Linux)
5. **Local only:** The application runs on `localhost`; no network deployment is planned or supported
6. **Single page:** The entire application is one Razor component page; no routing beyond the root URL

### Assumptions

1. The project lead is comfortable editing JSON files in a text editor
2. The developer's machine has sufficient resources to run a Blazor Server application (minimal: any machine that can run .NET 8)
3. The `OriginalDesignConcept.html` design file in the ReportingDashboard repository is the authoritative visual specification
4. The secondary design reference (`C:/Pics/ReportingDashboardDesign.png`) should be reviewed by engineers for any additional design requirements not captured in the HTML reference
5. Four heatmap categories (Shipped, In Progress, Carryover, Blockers) are sufficient; no additional categories are needed
6. The timeline spans a fixed 6-month window; this range is configurable via `data.json`
7. Up to 5 milestone tracks are supported; more than 5 would require SVG height adjustments
8. The green/blue/amber/red color scheme is universal across all projects using this dashboard
9. No historical data retention is needed — each `data.json` represents the current state only
10. Implementation timeline is 2–3 days for a single developer