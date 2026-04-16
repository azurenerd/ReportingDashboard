# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page, screenshot-optimized executive reporting dashboard that visualizes project milestones on a timeline and displays monthly work item status in a color-coded heatmap grid. The dashboard reads all data from a local `data.json` file, runs entirely on localhost with zero cloud dependencies, and is designed to be captured as 1920×1080 screenshots for PowerPoint decks to executive leadership. Built with Blazor Server on .NET 8, it requires no authentication, no database, and no external JavaScript libraries.

## Business Goals

1. **Provide at-a-glance project visibility** — Give executives a single-page view that communicates project health, milestone progress, and work item status without requiring them to log into ADO or any other tool.
2. **Eliminate manual slide creation** — Replace hand-built PowerPoint status slides with browser screenshots of a live, data-driven dashboard that can be updated by editing a single JSON file.
3. **Reduce reporting cycle time** — Enable the project lead to update `data.json` and capture a fresh screenshot in under 2 minutes, compared to 30+ minutes of manual slide formatting.
4. **Standardize executive reporting format** — Establish a reusable template (timeline + heatmap) that can be applied to any project by swapping out the data file.
5. **Keep operational cost at zero** — Run entirely on the developer's local workstation with no cloud infrastructure, licensing, or subscription costs.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** project lead, **I want** to see the project title, subtitle (org/workstream/date), and a link to the ADO backlog at the top of the dashboard, **so that** executives immediately know which project and time period they're viewing.

**Visual Reference:** Header section (`.hdr`) in `OriginalDesignConcept.html`

- [ ] Page displays the project title in 24px bold text, left-aligned
- [ ] Subtitle appears below the title in 12px gray text showing org, workstream, and current month/year
- [ ] An optional hyperlink to the ADO backlog appears inline with the title
- [ ] A milestone legend appears right-aligned in the header showing icons for PoC Milestone (amber diamond), Production Release (green diamond), Checkpoint (gray circle), and Now indicator (red vertical line)
- [ ] All text and layout values are driven from `data.json`

### US-2: View Milestone Timeline

**As an** executive, **I want** to see a horizontal timeline showing milestone streams with labeled markers for checkpoints, PoC dates, and production releases, **so that** I can understand the project's key dates and current position at a glance.

**Visual Reference:** Timeline area (`.tl-area` and inline SVG) in `OriginalDesignConcept.html`

- [ ] Left sidebar (230px) displays milestone stream labels (e.g., "M1 — Chatbot & MS Role") with stream-specific colors
- [ ] SVG timeline (1560×185px) renders month gridlines with labels (Jan–Jun or as defined in data)
- [ ] A red dashed vertical "NOW" line appears at the position corresponding to `currentDate`
- [ ] Each milestone stream renders as a colored horizontal line spanning the full timeline width
- [ ] Checkpoints render as open circles (white fill, colored stroke)
- [ ] PoC milestones render as amber (#F4B400) diamonds with drop shadow
- [ ] Production releases render as green (#34A853) diamonds with drop shadow
- [ ] Each milestone marker has a date label positioned above or below the marker
- [ ] Timeline supports 1–5 milestone streams as defined in `data.json`
- [ ] X-position calculation: `X = (date - timelineStart).TotalDays / (timelineEnd - timelineStart).TotalDays × 1560`

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a grid of work items organized by status (Shipped, In Progress, Carryover, Blockers) and month, **so that** I can quickly assess execution velocity and identify problem areas.

**Visual Reference:** Heatmap grid (`.hm-wrap`, `.hm-grid`) in `OriginalDesignConcept.html`

- [ ] Section title reads "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" in uppercase gray text
- [ ] Grid uses CSS Grid with columns: 160px status label + N equal-width month columns
- [ ] Header row displays month names; the current month column is highlighted with amber background (#FFF0D0) and amber text (#C07700)
- [ ] Four status rows render in order: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red)
- [ ] Each row header has a colored background matching its status category
- [ ] Each cell lists work items as bullet-pointed entries with a 6px colored dot preceding each item
- [ ] Current-month cells have a slightly darker background tint than other months
- [ ] Empty cells display a gray dash ("—")
- [ ] All items and month assignments are driven from `data.json`

### US-4: Load Dashboard Data from JSON File

**As a** project lead, **I want** the dashboard to read all display data from a `data.json` file, **so that** I can update the report content by editing a single file without touching code.

- [ ] Application reads `data.json` from a configurable file path at startup
- [ ] All dashboard content (title, subtitle, URL, dates, milestones, heatmap items) is deserialized from JSON
- [ ] Malformed JSON produces a clear, user-friendly error message in the browser (not a stack trace)
- [ ] Missing required fields produce specific validation error messages identifying the missing field
- [ ] Sample `data.json` ships with fictional "Project Phoenix" data demonstrating all features

### US-5: Live-Reload on Data File Change

**As a** project lead, **I want** the dashboard to automatically refresh when I save changes to `data.json`, **so that** I can iterate on report content without restarting the application.

- [ ] `FileSystemWatcher` monitors `data.json` for changes
- [ ] Dashboard re-renders within 2 seconds of a file save
- [ ] No manual browser refresh is required (Blazor Server push via SignalR)
- [ ] File watching is resilient to rapid successive saves (debounce of ~500ms)

### US-6: Screenshot-Optimized Rendering

**As a** project lead, **I want** the dashboard to render at exactly 1920×1080 with no scrollbars or browser chrome artifacts, **so that** I can take a clean full-page screenshot for my PowerPoint deck.

**Visual Reference:** `body { width: 1920px; height: 1080px; overflow: hidden; }` in `OriginalDesignConcept.html`

- [ ] Page renders at a fixed 1920×1080 viewport with `overflow: hidden`
- [ ] No scrollbars appear when the browser window is sized to 1920×1080 or larger
- [ ] All content fits within the viewport without clipping
- [ ] Optional `?screenshot=true` query parameter hides any non-essential UI elements (if any exist in future iterations)

## Visual Design Specification

**Canonical Design Reference:** `OriginalDesignConcept.html` from the ReportingDashboard repository. Engineers MUST consult both this HTML file and the rendered screenshot (`OriginalDesignConcept.png`) before implementation. Additionally, review `C:/Pics/ReportingDashboardDesign.png` for any supplementary design intent and improve upon the base design where appropriate.

### Overall Page Layout

- **Dimensions:** Fixed 1920px × 1080px, `overflow: hidden`
- **Background:** `#FFFFFF` (pure white)
- **Font Family:** `'Segoe UI', Arial, sans-serif`
- **Base Text Color:** `#111`
- **Layout:** Flexbox column (`display: flex; flex-direction: column`) — three stacked sections fill the viewport vertically

### Section 1: Header Bar (`.hdr`)

- **Height:** Auto (content-driven, ~48px)
- **Padding:** `12px 44px 10px`
- **Border:** Bottom `1px solid #E0E0E0`
- **Layout:** Flexbox row, `justify-content: space-between; align-items: center`
- **Left side:**
  - Title: `<h1>` at `font-size: 24px; font-weight: 700` with optional `<a>` link in `#0078D4`
  - Subtitle: `font-size: 12px; color: #888; margin-top: 2px`
- **Right side — Legend:** Flexbox row with `gap: 22px`, each legend item is `font-size: 12px` with an inline icon:
  - PoC Milestone: 12×12px amber (`#F4B400`) square rotated 45° (diamond shape)
  - Production Release: 12×12px green (`#34A853`) square rotated 45°
  - Checkpoint: 8×8px gray (`#999`) circle
  - Now indicator: 2×14px red (`#EA4335`) vertical bar

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed `196px`
- **Padding:** `6px 44px 0`
- **Background:** `#FAFAFA`
- **Border:** Bottom `2px solid #E8E8E8`
- **Layout:** Flexbox row, `align-items: stretch`
- **Left sidebar:** 230px fixed width, `border-right: 1px solid #E0E0E0`, contains milestone stream labels:
  - Each label: `font-size: 12px; font-weight: 600; line-height: 1.4`
  - Stream ID (e.g., "M1") in the stream's color; description in `color: #444; font-weight: 400`
  - Vertically distributed with `justify-content: space-around`
- **Right side — SVG canvas (`.tl-svg-box`):** `flex: 1; padding-left: 12px; padding-top: 6px`
  - SVG element: `width="1560" height="185"`
  - **Month gridlines:** Vertical lines at 260px intervals, `stroke: #bbb; stroke-opacity: 0.4`
  - **Month labels:** `font-size: 11; font-weight: 600; fill: #666` positioned 5px right of each gridline
  - **NOW line:** `stroke: #EA4335; stroke-width: 2; stroke-dasharray: 5,3` with "NOW" label in `font-size: 10; font-weight: 700; fill: #EA4335`
  - **Milestone stream tracks:** Horizontal lines at Y positions spaced ~56px apart (Y=42, Y=98, Y=154), `stroke-width: 3`, colored per stream
  - **Checkpoint markers:** `<circle>` with `r="5-7"`, white fill, colored stroke, `stroke-width: 2.5`
  - **PoC diamonds:** `<polygon>` (rotated square, 11px radius), `fill: #F4B400`, with drop shadow filter (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`)
  - **Production diamonds:** Same shape, `fill: #34A853`, same shadow
  - **Small checkpoint dots:** `<circle>` with `r="4"`, `fill: #999` (no stroke)
  - **Date labels:** `font-size: 10; fill: #666; text-anchor: middle`, positioned above or below markers

### Section 3: Heatmap Grid (`.hm-wrap`)

- **Flex:** `flex: 1; min-height: 0` (fills remaining vertical space)
- **Padding:** `10px 44px 10px`
- **Section title (`.hm-title`):** `font-size: 14px; font-weight: 700; color: #888; letter-spacing: 0.5px; text-transform: uppercase; margin-bottom: 8px`
- **Grid (`.hm-grid`):**
  - `display: grid`
  - `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months
  - `grid-template-rows: 36px repeat(4, 1fr)`
  - `border: 1px solid #E0E0E0`
  - `flex: 1; min-height: 0`

#### Grid Header Row

| Cell | Class | Background | Text Style | Borders |
|------|-------|-----------|-----------|---------|
| Corner | `.hm-corner` | `#F5F5F5` | `11px bold #999 uppercase` | Right: `1px #E0E0E0`, Bottom: `2px #CCC` |
| Month headers | `.hm-col-hdr` | `#F5F5F5` | `16px bold` | Right: `1px #E0E0E0`, Bottom: `2px #CCC` |
| Current month | `.hm-col-hdr.apr-hdr` | `#FFF0D0` | `color: #C07700` | Same as above |

#### Status Row Color Scheme

| Status | Row Header Class | Header BG | Header Text | Cell BG | Current Month Cell BG | Bullet Dot Color |
|--------|-----------------|-----------|-------------|---------|----------------------|-----------------|
| Shipped | `.ship-hdr` | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| In Progress | `.prog-hdr` | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| Carryover | `.carry-hdr` | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| Blockers | `.block-hdr` | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers

- `font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.7px`
- `padding: 0 12px; border-right: 2px solid #CCC; border-bottom: 1px solid #E0E0E0`
- Includes an emoji prefix (✅ Shipped, 🔵 In Progress, etc.) per the reference

#### Data Cells

- `padding: 8px 12px; border-right: 1px solid #E0E0E0; border-bottom: 1px solid #E0E0E0; overflow: hidden`
- Each work item (`.it`): `font-size: 12px; color: #333; padding: 2px 0 2px 12px; line-height: 1.35`
- Bullet dot: `::before` pseudo-element, `6px × 6px circle`, `position: absolute; left: 0; top: 7px`, colored per status row

### CSS Custom Properties (Enhancement over Reference)

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-blocker: #EA4335;
    --color-blocker-bg: #FFF5F5;
    --color-blocker-bg-current: #FFE4E4;
    --color-now-line: #EA4335;
    --color-current-month-header: #FFF0D0;
}
```

## UI Interaction Scenarios

**Scenario 1: Initial Page Load — Dashboard Renders with Full Data**
User navigates to `http://localhost:5050`. The page loads and renders all three sections (header, timeline, heatmap) populated from `data.json` within 1 second. The NOW line on the timeline is positioned at the date matching `currentDate` in the data file. The current month column in the heatmap is highlighted with amber styling. No loading spinner or skeleton screen is needed — this is a server-rendered page.

**Scenario 2: User Views the Header and Identifies the Project**
User sees the project title ("Project Phoenix Release Roadmap") in large bold text at top-left, the organizational subtitle below it, and the ADO backlog link. On the right side of the header, the legend icons explain the milestone marker types. The user immediately understands which project, team, and time period the report covers.

**Scenario 3: User Reads the Milestone Timeline**
User scans left-to-right across the timeline. The left sidebar labels identify each milestone stream (M1, M2, M3). Horizontal colored lines span the timeline for each stream. Diamond markers (amber for PoC, green for Production) and circle markers (checkpoints) are positioned at their respective dates. The red dashed NOW line clearly separates past from future. Date labels on each marker provide exact dates.

**Scenario 4: User Hovers Over a Milestone Diamond**
In this version, no tooltip or hover interaction is implemented. The milestone label text (e.g., "Mar 26 PoC") is always visible below or above the diamond marker. This is intentional — the dashboard is optimized for static screenshots, not interactivity.

**Scenario 5: User Scans the Heatmap for Current Month Status**
User's eye is drawn to the highlighted current month column (amber header, slightly darker cell backgrounds). They scan down the column to see: what shipped this month, what's in progress, what carried over from last month, and what's blocked. Each item has a colored bullet dot matching its status row.

**Scenario 6: User Compares Status Across Months**
User reads left-to-right across a status row (e.g., "Shipped") to see the progression of completed items over time. Earlier months may show shipped items while future months show gray dashes. This gives a visual sense of delivery velocity.

**Scenario 7: User Identifies Blockers**
User looks at the red "Blockers" row. Red-tinted cells with red bullet dots immediately stand out. The current month's blocker cell is highlighted in a deeper red (#FFE4E4). If there are no blockers, the cell shows a gray dash.

**Scenario 8: Empty State — Category Has No Items for a Month**
When a status category has no items for a given month, the cell displays a single gray dash ("—") in `color: #AAA`. The cell retains its background color. No "No data" message or empty-state illustration is shown.

**Scenario 9: Data File is Missing or Malformed**
If `data.json` is not found at the expected path, the page displays a centered error message: "Configuration Error: data.json not found at [path]. Please create the file and restart." If the JSON is malformed, the error message identifies the deserialization issue (e.g., "Invalid JSON: missing required field 'title'"). No stack trace is shown.

**Scenario 10: User Edits data.json and Dashboard Live-Reloads**
User opens `data.json` in a text editor, changes a work item name, and saves. Within 2 seconds, the Blazor Server push updates the browser — the dashboard re-renders with the new data. No manual refresh is needed. If the user introduces a JSON syntax error, the dashboard shows the validation error message (Scenario 9) until the file is corrected.

**Scenario 11: User Takes a Screenshot at 1920×1080**
User maximizes the browser window on a 1920×1080 display (or sets the viewport to that size via DevTools). The entire dashboard fits within the viewport with no scrollbars. User presses `Win+Shift+S` or uses a screenshot tool. The captured image is clean, with no browser chrome, scrollbars, or clipped content.

**Scenario 12: User Clicks the ADO Backlog Link**
User clicks the backlog URL link in the header. A new browser tab opens navigating to the configured ADO backlog URL. The dashboard remains in the original tab. This is the only clickable interactive element on the page.

**Scenario 13: Timeline Renders with Variable Number of Streams**
When `data.json` contains 1 stream, one horizontal track renders centered vertically. With 2 streams, tracks are spaced evenly. With 3 streams (reference design), tracks render at Y=42, Y=98, Y=154. With 4–5 streams, Y spacing adjusts proportionally within the 185px SVG height.

**Scenario 14: Heatmap Renders with Variable Number of Months**
When `data.json` specifies 4 months, the grid renders `160px repeat(4, 1fr)`. With 6 months, it renders `160px repeat(6, 1fr)` — cells are narrower but still readable. The CSS grid adjusts automatically; no code change is needed.

## Scope

### In Scope

- Single-page Blazor Server application rendering at 1920×1080
- Header section with project title, subtitle, backlog link, and milestone legend
- SVG-based milestone timeline with configurable streams, markers, and NOW indicator
- CSS Grid heatmap with 4 status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- Current month highlighting in both timeline (NOW line) and heatmap (amber column)
- All data driven from a single `data.json` flat file
- `FileSystemWatcher`-based live reload when `data.json` is modified
- Fictional "Project Phoenix" sample data for demonstration
- Startup validation of `data.json` with user-friendly error messages
- CSS ported from `OriginalDesignConcept.html` with minor enhancements (CSS custom properties, dynamic grid columns)
- Local-only execution on `http://localhost:5050`
- `dotnet watch` support for development hot reload

### Out of Scope

- **Authentication & authorization** — No login, no identity provider, no role-based access
- **Database** — No SQLite, SQL Server, or any persistence beyond the JSON file
- **API layer** — No REST/GraphQL endpoints; data is read directly from file
- **Responsive design** — Fixed 1920×1080 layout only; no mobile or tablet support
- **Dark mode** — Single light theme matching the reference design
- **Multi-project support** — One project per `data.json` file; no project selector UI
- **Print/PDF export** — Screenshots are the delivery mechanism; no Playwright or headless capture
- **Interactive elements** — No tooltips, hover effects, drill-downs, filtering, or sorting (beyond the backlog link)
- **JSON editor UI** — Project lead edits `data.json` directly in a text editor
- **CI/CD pipeline** — No GitHub Actions, Azure DevOps pipelines, or automated deployment
- **Containerization** — No Docker; runs via `dotnet run`
- **Cloud hosting** — No Azure App Service, no deployment to any remote server
- **Third-party component libraries** — No MudBlazor, Radzen, Bootstrap, Tailwind, or JS charting libraries
- **Accessibility (WCAG) compliance** — Not required for a local screenshot tool; can be addressed in future phases
- **Internationalization/localization** — English only

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Initial page load (cold start) | < 3 seconds including Blazor Server SignalR connection |
| Page render after `data.json` change | < 2 seconds from file save to visual update |
| `data.json` deserialization | < 100ms for files up to 50KB |
| SVG render time | < 500ms for up to 5 milestone streams with 20 milestones total |
| Memory footprint | < 100MB RAM for the running application |

### Reliability

- Application should start without errors when a valid `data.json` is present
- Application should display a clear error (not crash) when `data.json` is missing or invalid
- `FileSystemWatcher` should recover gracefully if the file is temporarily locked during save operations
- Application should handle `data.json` with 0 items in any category without errors

### Security

- **No authentication required** — Localhost-only access
- **No PII in data.json** — Only project names, dates, and work item titles
- **No HTTPS required** — HTTP on localhost is acceptable
- **No secrets** — No API keys, connection strings, or credentials anywhere in the project

### Maintainability

- Total file count: 8–12 source files maximum
- Zero external NuGet package dependencies for the main project
- CSS ported from the proven HTML reference design, not written from scratch
- C# data models should be self-documenting with clear property names

### Compatibility

- **Primary browser:** Microsoft Edge or Google Chrome (latest stable)
- **Screenshot resolution:** 1920×1080 pixels
- **Operating system:** Windows 10/11 (Segoe UI font dependency)
- **.NET SDK:** 8.0.x (LTS)

## Success Metrics

1. **Visual fidelity** — Dashboard screenshot at 1920×1080 is visually indistinguishable from the `OriginalDesignConcept.html` reference when compared side-by-side, with improvements in typography and subtle shadows.
2. **Data-driven rendering** — Changing any value in `data.json` (title, dates, items, months) produces a corresponding change in the rendered dashboard within 2 seconds, with no code modifications.
3. **Screenshot readiness** — The full dashboard fits within a single 1920×1080 screenshot with no scrollbars, clipped content, or browser artifacts.
4. **Time to update** — A project lead can update `data.json` and capture a new screenshot in under 2 minutes.
5. **Zero-config startup** — Running `dotnet run` with a valid `data.json` in place produces a working dashboard at `http://localhost:5050` with no additional setup steps.
6. **Sample data quality** — The fictional "Project Phoenix" data tells a coherent project story with realistic milestones, shipped items, in-progress work, carryovers, and at least one blocker.
7. **Error resilience** — Missing or malformed `data.json` produces a human-readable error message, not a crash or stack trace.
8. **Build simplicity** — `dotnet build` succeeds with zero warnings on a clean clone of the repository with only the .NET 8 SDK installed.

## Constraints & Assumptions

### Technical Constraints

- **Fixed viewport:** 1920×1080 is a hard requirement driven by the PowerPoint screenshot use case. Responsive design is explicitly excluded.
- **.NET 8 SDK required:** The developer's machine must have .NET 8.0.x SDK installed. No fallback to older runtimes.
- **Windows-only font dependency:** The design specifies Segoe UI, which is a Windows system font. Running on macOS/Linux would require a font substitution fallback (Arial is specified as backup).
- **Single-page constraint:** The entire dashboard must render on one page. No routing, no navigation, no multi-page architecture.
- **No JavaScript:** All rendering is done via Blazor Server-side C# and CSS. No `<script>` tags, no JS interop, no npm dependencies.
- **JSON schema is the contract:** The `data.json` schema defined in this specification is the interface between the data author and the rendering engine. Breaking changes to the schema require a spec revision.

### Timeline Assumptions

- **Phase 1 (MVP):** 1–2 developer-days to reach a pixel-perfect, data-driven dashboard
- **Phase 2 (Polish):** 0.5–1 additional day for FileSystemWatcher, validation, and design refinements
- **Single developer:** This project is scoped for one engineer working in a focused sprint

### Dependency Assumptions

- The `OriginalDesignConcept.html` file in the ReportingDashboard repository is the authoritative design reference and will not change during implementation
- The `C:/Pics/ReportingDashboardDesign.png` file provides supplementary design intent; any conflicts with the HTML reference should be resolved in favor of the PNG (as it represents the owner's latest vision)
- The .NET 8 SDK is already installed on the development machine
- The developer has access to Microsoft Edge or Chrome for screenshot capture
- No external API integrations are needed — all data is manually curated in `data.json`

### Data Assumptions

- `data.json` will contain fewer than 100 work items total across all categories and months
- The timeline will span 3–12 months maximum
- There will be 1–5 milestone streams maximum
- The heatmap will display 3–6 month columns (optimized for 4)
- Work item descriptions are short (under 60 characters) to fit within heatmap cells without truncation
- The project lead is comfortable editing JSON in a text editor (VS Code, Notepad++, etc.)