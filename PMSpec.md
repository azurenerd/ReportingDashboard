# PM Specification: Executive Reporting Dashboard

## Executive Summary

We are building a single-page executive reporting dashboard that visualizes project milestones, shipping status, and monthly execution progress in a format optimized for 1920×1080 PowerPoint screenshot capture. The application is a local-only Blazor Server (.NET 8) web page that reads all data from a `data.json` configuration file, rendering a pixel-perfect SVG timeline and color-coded heatmap grid with zero authentication, zero cloud dependencies, and zero external libraries. This tool enables program managers to produce polished, consistent roadmap visuals for executive presentations by simply editing a JSON file and taking a browser screenshot.

## Business Goals

1. **Reduce executive slide preparation time by 80%** — Replace manual PowerPoint diagram creation with an automated, data-driven rendering pipeline where updating a JSON file instantly produces a presentation-ready visual.
2. **Ensure visual consistency across reporting cycles** — Every monthly update uses the same layout, typography, and color system, eliminating ad-hoc formatting differences between slides.
3. **Provide at-a-glance project health visibility** — Executives can see shipped items, in-progress work, carryover debt, and blockers in a single 1920×1080 view without scrolling or clicking.
4. **Enable zero-infrastructure operation** — The tool runs locally on a developer's Windows machine with `dotnet run`; no servers to provision, no credentials to manage, no recurring costs.
5. **Support rapid iteration on project data** — Hot reload via `dotnet watch` allows PMs to edit `data.json` and see changes reflected in the browser within seconds.

## User Stories & Acceptance Criteria

### US-1: View Project Header and Context

**As a** program manager, **I want** to see the project title, organizational subtitle, backlog link, and a legend of milestone marker types at the top of the dashboard, **so that** anyone viewing the screenshot immediately knows which project, team, and time period the report covers.

**Visual Reference:** Header section (`.hdr`) in `OriginalDesignConcept.html`

**Acceptance Criteria:**
- [ ] The header displays the project title in 24px bold font, left-aligned.
- [ ] A clickable "→ ADO Backlog" link appears inline with the title, styled in `#0078D4`.
- [ ] The subtitle (organization, workstream, month) appears below the title in 12px `#888` text.
- [ ] A legend on the right side of the header shows four marker types: PoC Milestone (gold diamond), Production Release (green diamond), Checkpoint (gray circle), and Now line (red vertical bar).
- [ ] All header content is driven by `data.json` fields: `project.title`, `project.subtitle`, `project.backlogUrl`.
- [ ] The header has a 1px `#E0E0E0` bottom border separating it from the timeline section.

### US-2: View Milestone Timeline with Track Lanes

**As an** executive, **I want** to see a horizontal timeline showing milestone tracks (M1, M2, M3, etc.) with markers for checkpoints, PoC milestones, and production releases, **so that** I can understand the project's temporal arc and where we are relative to key dates.

**Visual Reference:** Timeline area (`.tl-area` and SVG section) in `OriginalDesignConcept.html`

**Acceptance Criteria:**
- [ ] The timeline section renders below the header with a `#FAFAFA` background and fixed height of 196px.
- [ ] A left sidebar (230px wide) lists milestone track labels (e.g., "M1 — Chatbot & MS Role") with color-coded track identifiers.
- [ ] The SVG area (remaining width, ~1560px) renders vertical month divider lines with month labels (Jan–Jun).
- [ ] Each track renders as a horizontal colored line at a distinct Y position (e.g., M1 at y=42, M2 at y=98, M3 at y=154).
- [ ] Checkpoint milestones render as open circles (white fill, colored stroke) with date labels.
- [ ] PoC milestones render as gold (`#F4B400`) diamond shapes with drop-shadow and date labels.
- [ ] Production milestones render as green (`#34A853`) diamond shapes with drop-shadow and date labels.
- [ ] A vertical red dashed line (`#EA4335`, stroke-dasharray "5,3") marks the current date ("NOW") with a bold red label.
- [ ] All milestone positions, dates, types, and labels are driven by `data.json` fields under `tracks[].milestones[]`.
- [ ] The timeline supports a configurable number of tracks (minimum 1, tested up to 5).

### US-3: View Monthly Execution Heatmap

**As an** executive, **I want** to see a color-coded grid showing what was shipped, what's in progress, what carried over, and what's blocked — organized by month — **so that** I can assess execution velocity and identify problem areas at a glance.

**Visual Reference:** Heatmap section (`.hm-wrap`, `.hm-grid`) in `OriginalDesignConcept.html`

**Acceptance Criteria:**
- [ ] The heatmap section fills all remaining vertical space below the timeline.
- [ ] A section title "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" appears in 14px uppercase `#888` text.
- [ ] The grid uses CSS Grid with columns: 160px (status label) + N equal-width month columns.
- [ ] The header row displays month names in 16px bold text; the current month column has a gold highlight (`#FFF0D0` background, `#C07700` text) with a "← Now" indicator.
- [ ] Four status rows render in order: Shipped (green), In Progress (blue), Carryover (amber), Blockers (red).
- [ ] Each row header displays the status name with an emoji prefix (✅, 🔵, 🟡, 🔴) in uppercase, color-matched to its category.
- [ ] Data cells display individual work items as 12px text lines with a colored dot bullet (6px circle) matching the row's status color.
- [ ] The current month's column cells use a darker background variant for visual emphasis.
- [ ] Future months with no items display a dash ("–") in muted text.
- [ ] All heatmap data is driven by `data.json` fields under `heatmap.categories[].items`.

### US-4: Configure Dashboard Data via JSON

**As a** program manager, **I want** to edit a single `data.json` file to update all dashboard content — project metadata, milestones, and heatmap items — **so that** I can refresh the report for each monthly cycle without touching any code.

**Acceptance Criteria:**
- [ ] A `data.json` file exists in the project's `Data/` directory with a documented schema.
- [ ] The JSON schema includes sections for: `project` (title, subtitle, backlogUrl, currentMonth), `timeline` (months, nowPosition), `tracks` (array of milestone tracks with milestones), and `heatmap` (months, categories with items).
- [ ] The application reads `data.json` on startup and deserializes it into strongly-typed C# record models.
- [ ] Invalid or malformed JSON produces a clear error message in the browser rather than a blank page or crash.
- [ ] A sample `data.json` is provided with realistic fictional project data demonstrating all supported features.

### US-5: Capture Pixel-Perfect Screenshots

**As a** program manager, **I want** the dashboard to render at exactly 1920×1080 pixels with no scrollbars, **so that** I can take a browser screenshot and paste it directly into a PowerPoint slide without cropping or resizing.

**Acceptance Criteria:**
- [ ] The `<body>` element has fixed dimensions: `width: 1920px; height: 1080px; overflow: hidden`.
- [ ] All content fits within 1920×1080 without overflow or scrollbars.
- [ ] The page renders identically in Microsoft Edge and Google Chrome at 100% zoom.
- [ ] No Blazor framework chrome, error banners, or reconnection UI is visible on the page.
- [ ] The Segoe UI system font renders correctly on Windows machines.

### US-6: Run Dashboard Locally with Minimal Setup

**As a** developer, **I want** to start the dashboard with a single `dotnet run` command and no additional setup, **so that** I can get it running in under 60 seconds on any Windows machine with .NET 8 SDK installed.

**Acceptance Criteria:**
- [ ] The project builds with zero additional NuGet packages beyond the default Blazor Server template.
- [ ] `dotnet run` starts the application and serves it at `http://localhost:5000`.
- [ ] `dotnet watch` enables hot reload for `.razor` and `.css` file changes.
- [ ] No HTTPS certificate is required (project created with `--no-https`).
- [ ] A `README.md` documents: prerequisites, how to run, how to edit `data.json`, and how to take screenshots.

### US-7: View Dashboard with Fictional Sample Data

**As an** engineer validating the implementation, **I want** the dashboard to ship with a complete fictional project dataset, **so that** I can verify all visual elements render correctly before substituting real project data.

**Acceptance Criteria:**
- [ ] The sample `data.json` contains a fictional project with a descriptive title and organizational context.
- [ ] At least 3 milestone tracks are defined with varied milestone types (checkpoint, PoC, production).
- [ ] All 4 heatmap categories (Shipped, In Progress, Carryover, Blockers) contain items across at least 3 months.
- [ ] The "current month" is set and visually highlighted in both the timeline and heatmap.
- [ ] The rendered dashboard visually matches the reference design in `OriginalDesignConcept.html`.

## Visual Design Specification

**Design Source File:** `OriginalDesignConcept.html`

### Page Container

- **Dimensions:** Fixed 1920px × 1080px, `overflow: hidden`
- **Background:** `#FFFFFF`
- **Font Family:** `'Segoe UI', Arial, sans-serif`
- **Base Text Color:** `#111`
- **Layout:** Flexbox column (`display: flex; flex-direction: column`), three stacked sections filling the full height

### Section 1: Header (`.hdr`)

- **Padding:** 12px top, 44px horizontal, 10px bottom
- **Border:** 1px solid `#E0E0E0` bottom
- **Layout:** Flexbox row, space-between alignment
- **Left side:**
  - **Title (H1):** 24px, font-weight 700, contains project name + inline link
  - **Link style:** `#0078D4`, no underline, text "→ ADO Backlog"
  - **Subtitle (`.sub`):** 12px, `#888`, margin-top 2px; format: "Org · Workstream · Month Year"
- **Right side (Legend):**
  - Horizontal flex row, 22px gap between items
  - Each legend item: 12px text with inline icon
  - PoC Milestone: 12×12px square rotated 45° (`transform: rotate(45deg)`), fill `#F4B400`
  - Production Release: 12×12px square rotated 45°, fill `#34A853`
  - Checkpoint: 8×8px circle, fill `#999`
  - Now indicator: 2×14px vertical bar, fill `#EA4335`, label "Now (Mon Year)"

### Section 2: Timeline Area (`.tl-area`)

- **Height:** Fixed 196px, `flex-shrink: 0`
- **Background:** `#FAFAFA`
- **Border:** 2px solid `#E8E8E8` bottom
- **Padding:** 6px top, 44px horizontal
- **Layout:** Flexbox row

#### Timeline Labels Sidebar (left)
- **Width:** 230px, flex-shrink 0
- **Border:** 1px solid `#E0E0E0` right
- **Padding:** 16px vertical, 12px right
- **Layout:** Flexbox column, `justify-content: space-around`
- **Each track label:**
  - Track ID (e.g., "M1"): 12px, font-weight 600, track color
  - Track name: 12px, font-weight 400, `#444`
  - Track colors: M1 = `#0078D4`, M2 = `#00897B`, M3 = `#546E7A`

#### SVG Timeline (right, `.tl-svg-box`)
- **SVG dimensions:** width 1560, height 185, `overflow: visible`
- **Drop-shadow filter:** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`
- **Month dividers:** Vertical lines at 260px intervals (0, 260, 520, 780, 1040, 1300), stroke `#bbb` opacity 0.4
- **Month labels:** 11px, font-weight 600, `#666`, positioned 5px right of divider line at y=14
- **NOW line:** Vertical dashed line (`stroke-dasharray: 5,3`), `#EA4335`, stroke-width 2; label "NOW" in 10px bold `#EA4335`
- **Track lines:** Horizontal lines spanning full width (x1=0 to x2=1560), stroke-width 3, color = track color
  - M1 at y=42, M2 at y=98, M3 at y=154
- **Checkpoint markers:** Circle, radius 7, white fill, track-color stroke (stroke-width 2.5); date label in 10px `#666`
- **Small checkpoints:** Circle, radius 4–5, filled `#999` or white with `#888` stroke
- **PoC diamond:** Polygon (diamond shape, ±11px), fill `#F4B400`, drop-shadow filter; date + "PoC" label
- **Production diamond:** Polygon (diamond shape, ±11px), fill `#34A853`, drop-shadow filter; date + "Prod" label
- **Label positioning:** Above or below the track line (alternating to avoid overlap), 10px font, `#666`, text-anchor middle

### Section 3: Heatmap (`.hm-wrap`)

- **Padding:** 10px top/bottom, 44px horizontal
- **Layout:** Flexbox column, `flex: 1; min-height: 0` (fills remaining space)

#### Heatmap Title (`.hm-title`)
- 14px, font-weight 700, `#888`, letter-spacing 0.5px, uppercase
- Margin-bottom 8px
- Text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers"

#### Heatmap Grid (`.hm-grid`)
- **CSS Grid:** `grid-template-columns: 160px repeat(4, 1fr)`, `grid-template-rows: 36px repeat(4, 1fr)`
- **Border:** 1px solid `#E0E0E0`
- **Layout:** 5 columns × 5 rows (1 header row + 4 data rows)

#### Column Headers
- **Corner cell (`.hm-corner`):** `#F5F5F5` background, 11px bold uppercase `#999`, "STATUS" label, border-bottom 2px solid `#CCC`
- **Month headers (`.hm-col-hdr`):** `#F5F5F5` background, 16px bold, border-bottom 2px solid `#CCC`
- **Current month header (`.apr-hdr`):** `#FFF0D0` background, `#C07700` text, with "← Now" indicator

#### Row Color Scheme

| Status | Row Header BG | Row Header Text | Cell BG | Current Month Cell BG | Dot Color |
|--------|--------------|-----------------|---------|----------------------|-----------|
| ✅ Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| 🔵 In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| 🟡 Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| 🔴 Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

#### Row Headers (`.hm-row-hdr`)
- 11px, font-weight 700, uppercase, letter-spacing 0.7px
- Padding: 0 12px, border-right 2px solid `#CCC`, border-bottom 1px solid `#E0E0E0`

#### Data Cells (`.hm-cell`)
- Padding: 8px 12px, border-right 1px solid `#E0E0E0`, border-bottom 1px solid `#E0E0E0`
- `overflow: hidden`

#### Cell Items (`.it`)
- 12px, `#333`, line-height 1.35
- Padding: 2px 0 2px 12px (left indent for bullet)
- **Bullet dot (::before pseudo-element):** 6×6px circle, positioned absolute at left=0, top=7px, color matches row status

## UI Interaction Scenarios

**Scenario 1: Initial Page Load**
User navigates to `http://localhost:5000`. The dashboard renders the complete page in a single server-side pass. The header, timeline SVG, and heatmap grid all appear simultaneously with data populated from `data.json`. No loading spinner, no progressive rendering, no layout shift.

**Scenario 2: User Views Project Header**
User sees the project title "Project Name → ADO Backlog" with the organizational subtitle below it. On the right side, they see the legend with four marker types. The header provides immediate context for the entire report.

**Scenario 3: User Scans the Milestone Timeline**
User looks at the timeline area and sees colored horizontal track lines spanning January through June. Diamond markers indicate PoC and Production milestones at specific dates. A red dashed "NOW" line shows the current position in time. The user can visually assess whether milestones are ahead of or behind schedule relative to NOW.

**Scenario 4: User Identifies Current Month in Heatmap**
User looks at the heatmap and immediately identifies the current month column by its gold-highlighted header (`#FFF0D0` background, `#C07700` text) and darker cell backgrounds in each row. This draws the eye to "what's happening right now."

**Scenario 5: User Assesses Shipped Items**
User scans the green "Shipped" row from left to right, seeing green-dotted item names in each month's cell. Months with many items signal high delivery velocity; empty cells with a dash indicate no deliveries that month.

**Scenario 6: User Identifies Blockers**
User looks at the red "Blockers" row and sees red-dotted item names. Any non-empty blocker cell in the current or future months signals risk requiring executive attention.

**Scenario 7: User Spots Carryover Debt**
User compares the amber "Carryover" row across months. Items appearing in the carryover row indicate work that slipped from its original target month. Repeated items across multiple months signal chronic delays.

**Scenario 8: User Hovers Over a Milestone Diamond (Enhancement)**
User hovers over a PoC or Production diamond on the timeline. An optional CSS tooltip appears showing the milestone date, label, and track name. This is a subtle enhancement over the base design — not required for MVP but improves screenshot annotations.

**Scenario 9: User Takes a Screenshot for PowerPoint**
User sets their browser to 1920×1080 (via DevTools device mode or window sizing). The entire dashboard fits in the viewport with no scrollbars. User presses Ctrl+Shift+S (or uses Snipping Tool) to capture the full page and pastes it into a PowerPoint slide at native resolution.

**Scenario 10: Data-Driven Rendering with Varying Track Counts**
When `data.json` contains 2 tracks instead of 3, the timeline SVG adjusts the vertical spacing of track lines and the sidebar shows only 2 labels. The layout remains balanced within the 196px timeline height.

**Scenario 11: Empty Heatmap Cells**
When a heatmap category has no items for a given month (e.g., no items shipped in June), the cell renders a muted dash ("–") in `#AAA` text rather than appearing blank, signaling intentional emptiness rather than a rendering bug.

**Scenario 12: Malformed JSON Error State**
When `data.json` contains invalid JSON or missing required fields, the page renders a centered error message describing the problem (e.g., "Error loading data.json: Missing required field 'project.title'") instead of a blank white page or Blazor error boundary.

**Scenario 13: Page Load with No Interactivity**
After the page renders, there are no SignalR reconnection banners, no interactive buttons, no form elements. The page is purely static HTML/CSS/SVG — what renders on first load is exactly what the screenshot captures.

## Scope

### In Scope

- Single-page Blazor Server application targeting .NET 8 LTS
- Fixed 1920×1080 pixel layout optimized for PowerPoint screenshots
- Header section with project title, subtitle, backlog link, and milestone legend
- SVG timeline with configurable milestone tracks, checkpoint/PoC/production markers, and NOW line
- CSS Grid heatmap with four status rows (Shipped, In Progress, Carryover, Blockers) × N month columns
- Color-coded cells with current-month visual emphasis
- `data.json` file as the single data source with a documented schema
- Strongly-typed C# record models for JSON deserialization
- `DashboardDataService` singleton service for data loading
- CSS custom properties for theming/color palette
- Sample fictional project data demonstrating all features
- `README.md` with setup, usage, and screenshot workflow instructions
- Hot reload support via `dotnet watch`
- Error handling for malformed `data.json`

### Out of Scope

- **Authentication / Authorization** — No login, no user roles, no access control
- **HTTPS** — Localhost only, no TLS certificate
- **Database** — No SQL, no Entity Framework, no persistent storage
- **Multi-user access** — Single developer runs locally; no concurrency concerns
- **Responsive design** — Fixed 1920×1080 only; mobile/tablet layouts are not supported
- **Client-side interactivity** — No JavaScript, no button handlers, no form submissions, no real-time updates
- **API endpoints** — No REST/GraphQL APIs; the app serves one HTML page
- **Deployment / CI/CD** — No Docker, no Azure, no GitHub Actions pipeline
- **Multi-project support** — One dashboard instance serves one `data.json`; switching projects requires editing the file
- **Data entry UI** — No web forms for editing project data; `data.json` is edited manually in a text editor
- **Historical data / versioning** — No tracking of changes over time; each `data.json` is a point-in-time snapshot
- **Charting libraries** — No Chart.js, ApexCharts, MudBlazor, or any third-party UI component library
- **Unit tests** — Deferred to a future iteration; visual validation is manual via screenshot comparison
- **Print stylesheet** — Not needed; screenshots are the capture mechanism

## Non-Functional Requirements

### Performance

| Metric | Target | Rationale |
|--------|--------|-----------|
| Page load time | < 500ms (localhost) | Instant feedback during data editing |
| Time to first meaningful paint | < 300ms | Static SSR renders complete HTML in one pass |
| `data.json` file size limit | Up to 100KB | Supports ~500 heatmap items; well within memory |
| Hot reload cycle | < 2 seconds | `dotnet watch` default for `.razor`/`.css` changes |

### Reliability

- The application must not crash on startup if `data.json` is missing; instead display a clear error.
- The application must gracefully handle missing optional fields in `data.json` (e.g., a track with zero milestones, a category with no items for a month).
- The SVG timeline must not overflow or clip milestone labels for positions between 0.0 and 1.0.

### Compatibility

- **Primary browsers:** Microsoft Edge (latest), Google Chrome (latest) on Windows 10/11
- **OS:** Windows 10 or later (Segoe UI font dependency)
- **SDK:** .NET 8.0.x (any patch version)
- **Resolution:** Validated at exactly 1920×1080 pixels, 100% browser zoom

### Security

- No authentication required (localhost-only, single-user tool)
- No sensitive data stored (project names and milestones are organizational, not PII)
- No network calls beyond localhost Kestrel server
- No CORS configuration needed
- JSON input should be validated to prevent malformed data from crashing the renderer

### Maintainability

- All colors defined as CSS custom properties in `:root` for single-point-of-change theming
- Component hierarchy mirrors the visual hierarchy (header → timeline → heatmap)
- C# records provide immutable, self-documenting data models
- Zero third-party dependencies minimize upgrade burden

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Screenshot fidelity** | Dashboard renders at 1920×1080 with no scrollbars, no clipping, no Blazor chrome visible | Manual browser screenshot comparison against `OriginalDesignConcept.html` |
| 2 | **Data update cycle time** | PM can edit `data.json` and see updated dashboard in < 5 seconds | Timed test: edit JSON → save → verify browser reflects change (hot reload) |
| 3 | **Setup time** | New user can clone repo, run `dotnet run`, and see dashboard in < 60 seconds | Timed walkthrough following `README.md` instructions |
| 4 | **Visual match to reference design** | ≥ 95% visual parity with `OriginalDesignConcept.html` across header, timeline, and heatmap sections | Side-by-side pixel comparison in browser DevTools |
| 5 | **Zero runtime dependencies** | Project builds and runs with no NuGet packages beyond the default Blazor Server template | Verify `.csproj` contains no `<PackageReference>` elements |
| 6 | **JSON schema coverage** | Sample `data.json` exercises all supported features: 3+ tracks, all 4 heatmap categories, current month highlighting, all 3 milestone types | Manual review of rendered output against JSON content |

## Constraints & Assumptions

### Technical Constraints

- **Target framework:** .NET 8.0 LTS — no .NET 9 preview features
- **Render mode:** Blazor Static SSR (no SignalR circuit) — pages render once on the server and ship as pure HTML/CSS/SVG
- **Fixed resolution:** 1920×1080 pixels — the layout is not responsive and will not adapt to other screen sizes
- **No JavaScript:** All rendering is server-side Blazor + CSS + inline SVG; no JS interop
- **System font dependency:** Segoe UI is assumed available on the host OS (Windows); Arial is the CSS fallback
- **Single data source:** One `data.json` file per running instance; no runtime data switching
- **Local only:** The app binds to `localhost:5000` with no external network access

### Assumptions

- The user has .NET 8 SDK installed on a Windows 10/11 machine
- The user has access to Microsoft Edge or Google Chrome for viewing the dashboard
- The user is comfortable editing JSON files in a text editor (VS Code, Notepad++, etc.)
- The dashboard will be used by 1 person at a time on their local machine
- Project data changes monthly or less frequently (not real-time)
- The maximum number of milestone tracks is 5 (SVG height is fixed at 185px; more tracks would require layout adjustment)
- The maximum number of month columns in the heatmap is 6 (matching the Jan–Jun span in the reference design; more months would compress column widths)
- Each heatmap cell will contain at most 8–10 items; cells with more items may overflow and clip (acceptable for screenshot use)
- The ADO Backlog link in the header is cosmetic for screenshots; it does not need to resolve to a real URL
- Color-blind accessibility is not a requirement for this version (the existing color palette uses distinct hue families: green, blue, amber, red)

### Dependencies

- **.NET 8.0 SDK** — must be pre-installed on the developer's machine
- **`OriginalDesignConcept.html`** — serves as the canonical visual specification; all CSS class names and layout patterns are derived from this file
- **Segoe UI font** — ships with Windows; no web font download required
- **Browser DevTools** — required for setting viewport to 1920×1080 for screenshot capture (no programmatic screenshot tool is in scope)