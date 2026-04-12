# PM Specification: My Project

## Executive Summary

My Project is a lightweight, screenshot-optimized executive reporting dashboard built as a Blazor Server web application. It provides single-page visibility into project health by displaying shipped items, in-progress work, carryover items, and blockers across a 4-month timeline, with an interactive SVG milestone tracker. The dashboard reads all data from a simple JSON configuration file (data.json) and requires no authentication, database, or external services, enabling executives to quickly assess project status and export dashboard views to PowerPoint presentations.

## Business Goals

1. **Enable real-time executive visibility** into project health by displaying consolidated status across four status categories (shipped, in progress, carryover, blockers) in a single, scannable view.
2. **Eliminate manual status reporting** by allowing project managers to update data.json and refresh the browser to reflect current project state without modifying code.
3. **Support screenshot-ready design** optimized for PowerPoint presentations; all visuals must be deterministic, fully reproducible, and pixel-perfect across different machines.
4. **Provide timeline-based milestone tracking** showing key delivery milestones (PoC, production releases, checkpoints) aligned to project calendar months.
5. **Maintain operational simplicity** with zero infrastructure complexity—no databases, no authentication, no caching layers, no external service dependencies.

## User Stories & Acceptance Criteria

### Story 1: Executive Views Project Dashboard on Initial Load
**As an** executive stakeholder, **I want** to view the complete project status dashboard immediately upon opening the application, **so that** I can quickly assess project health without any additional navigation or configuration.

**Acceptance Criteria:**
- [ ] Dashboard loads in under 2 seconds on first page load
- [ ] Header displays project name ("My Project" or configured name from data.json)
- [ ] Header displays subtitle with workstream/organization context (from data.json "description" field)
- [ ] Legend is visible in top-right corner showing: PoC Milestone (yellow diamond), Production Release (green diamond), Checkpoint (gray circle), Now marker (red vertical line)
- [ ] Four status rows are visible below timeline: Shipped, In Progress, Carryover, Blockers
- [ ] Heatmap displays 4-month columns with data populated from data.json
- [ ] All UI elements render in exact layout matching OriginalDesignConcept.html (see Visual Design Specification section)
- [ ] No console errors or warnings visible in browser dev tools

### Story 2: Executive Sees Milestone Timeline with Delivery Dates
**As an** executive, **I want** to see a horizontal timeline with milestone markers positioned by date, **so that** I can understand when key deliverables are expected and track milestone progress visually.

**Acceptance Criteria:**
- [ ] Timeline renders as SVG with horizontal lines for each major milestone (M1, M2, M3, etc.)
- [ ] Milestones are positioned along 6-month calendar grid (Jan, Feb, Mar, Apr, May, Jun)
- [ ] Current date ("Now") is marked with red vertical dashed line at correct position
- [ ] PoC milestones appear as yellow diamonds with label and date below timeline
- [ ] Production release milestones appear as green diamonds with label and date
- [ ] Checkpoint milestones appear as gray circles with label and date
- [ ] All dates are aligned to month grid lines programmatically via NodaTime calculations
- [ ] Hovering over a milestone shape displays tooltip with milestone label and exact date (ISO format)

### Story 3: Executive Reviews Status Heatmap Grid
**As an** executive, **I want** to see a color-coded grid showing how many items were shipped, are in progress, carried over, or blocked in each month, **so that** I can identify months with high risk, velocity, or bottlenecks.

**Acceptance Criteria:**
- [ ] Heatmap displays 4-month columns (current month + 3 future months)
- [ ] First column header shows "March", second "April ? Now" (highlighting current month), third "May", fourth "June"
- [ ] Four status rows render: Shipped (green), In Progress (blue), Carryover (yellow), Blockers (red)
- [ ] Shipped row background: light green (#F0FBF0), darker green (#D8F2DA) for current month
- [ ] In Progress row background: light blue (#EEF4FE), darker blue (#DAE8FB) for current month
- [ ] Carryover row background: light yellow (#FFFDE7), darker yellow (#FFF0B0) for current month
- [ ] Blockers row background: light red (#FFF5F5), darker red (#FFE4E4) for current month
- [ ] Each cell displays 2-4 item names, each with a colored dot indicator matching row type
- [ ] Row headers show status type in uppercase (SHIPPED, IN PROGRESS, CARRYOVER, BLOCKERS)
- [ ] Items are populated from data.json arrays (shipped[], inProgress[], carryover[], blockers[])
- [ ] Empty cells display a dash ("—") instead of blank
- [ ] Grid borders are 1px light gray (#E0E0E0), with heavier borders (#CCC) separating sections

### Story 4: Project Manager Updates Dashboard via data.json
**As a** project manager, **I want** to edit the data.json file to update shipped items, in-progress work, carryover, and blockers, **so that** I can keep the dashboard current without modifying code or redeploying.

**Acceptance Criteria:**
- [ ] data.json schema supports: projectName, description, quarters[], milestones[]
- [ ] quarters[] array contains objects with: month (string), year (number), shipped[], inProgress[], carryover[], blockers[]
- [ ] milestones[] array contains objects with: id, label, date (ISO 8601), type ("poc"|"release"|"checkpoint")
- [ ] Application reads data.json on startup via DashboardDataService
- [ ] Changes to data.json are reflected in dashboard after browser refresh (F5 or Ctrl+R)
- [ ] Invalid JSON in data.json displays graceful error message (not blank screen)
- [ ] data.json path is configurable via appsettings.json (default: wwwroot/data/data.json)

### Story 5: Executive Prints/Screenshots Dashboard for PowerPoint
**As an** executive, **I want** to print or screenshot the dashboard and embed it in PowerPoint presentations, **so that** I can share project status in executive reviews and steering committee meetings.

**Acceptance Criteria:**
- [ ] Dashboard layout is optimized for 1920x1080 resolution (exactly matching OriginalDesignConcept.html dimensions)
- [ ] All text is readable in printed output with no overlapping or truncation
- [ ] Heatmap grid prints on a single page in landscape orientation
- [ ] Colors and styling are preserved in both browser print preview and PDF export
- [ ] No dynamic elements (animations, tooltips) interfere with screenshot capture
- [ ] Print stylesheet ensures no UI chrome (browser tabs, scrollbars) is visible in screenshots
- [ ] Screenshots remain pixel-perfect consistent across different machines/browsers

### Story 6: Dashboard Displays "Last Updated" Timestamp
**As an** executive, **I want** to see when the dashboard data was last updated, **so that** I know the staleness of the information I'm reviewing.

**Acceptance Criteria:**
- [ ] "Last Updated" text appears in header or footer with exact timestamp
- [ ] Timestamp is automatically calculated from data.json file's modification time
- [ ] Format is human-readable (e.g., "Last Updated: Apr 12, 2026 at 2:51 AM UTC")
- [ ] Timestamp updates automatically when data.json is modified and page is refreshed

## Visual Design Specification

The dashboard visual design is defined in **OriginalDesignConcept.html** and follows the exact layout, color scheme, and typography specified in that file. The following section describes all visual elements that engineers must implement.

### Overall Layout & Canvas
- **Canvas size**: 1920px width × 1080px height (desktop landscape, optimized for PowerPoint screenshots)
- **Background**: Solid white (#FFFFFF)
- **Font family**: Segoe UI, Arial (fallback)
- **Text color**: Dark gray (#111) for body text, lighter gray (#888) for secondary/metadata
- **Layout structure**: Flexbox column layout with three main sections: header, timeline, heatmap

### Header Section (OriginalDesignConcept.html `.hdr` class)
- **Height**: 60px (flex-shrink: 0, fixed)
- **Padding**: 12px 44px (top/bottom 12px, left/right 44px)
- **Border**: Bottom border 1px solid light gray (#E0E0E0)
- **Layout**: Flexbox row with space-between alignment
- **Left side**: Contains project title and subtitle
  - **Title** (h1): Font size 24px, font-weight 700, color #111
  - **Title link**: "? ADO Backlog" hyperlink in blue (#0078D4), no underline
  - **Subtitle** (.sub): Font size 12px, color #888, margin-top 2px
  - **Subtitle text**: "[Organization] • [Workstream] • [Month Year]" format from data.json "description" field
- **Right side**: Legend showing milestone and marker types
  - **Legend items**: Four inline flex items with 22px gap between them
  - **PoC Milestone**: Yellow (#F4B400) 12×12px diamond (45° rotated square), followed by text "PoC Milestone"
  - **Production Release**: Green (#34A853) 12×12px diamond, followed by text "Production Release"
  - **Checkpoint**: Gray (#999) 8×8px circle, followed by text "Checkpoint"
  - **Now marker**: Red (#EA4335) 2px wide × 14px tall vertical line, followed by text "Now ([Month] [Year])"
  - **Legend font**: 12px, color #111

### Timeline Section (OriginalDesignConcept.html `.tl-area` class)
- **Height**: 196px (flex-shrink: 0, fixed)
- **Padding**: 6px 44px 0 (top 6px, left/right 44px)
- **Border-bottom**: 2px solid light gray (#E8E8E8)
- **Background**: Light gray (#FAFAFA)
- **Layout**: Flexbox row with two sub-sections
  
**Left milestone label box** (230px fixed width):
- **Padding**: 16px 12px 16px 0
- **Border-right**: 1px solid light gray (#E0E0E0)
- **Content**: Vertical list of 3 milestone labels (M1, M2, M3, etc.)
  - Each label: Font size 12px, font-weight 600, color varies by milestone line (M1: #0078D4 blue, M2: #00897B teal, M3: #546E7A gray)
  - Below label: Secondary text in font-weight 400, color #444, describing milestone name (e.g., "Chatbot & MS Role")

**Right SVG timeline box** (.tl-svg-box):
- **Flex**: 1 (fills remaining width)
- **SVG dimensions**: 1560px width × 185px height
- **Overflow**: visible
- **Content**: SVG-rendered timeline with the following elements:

  **Month vertical gridlines** (light gray backdrop):
  - Vertical lines at x-positions: 0 (Jan), 260 (Feb), 520 (Mar), 780 (Apr), 1040 (May), 1300 (Jun)
  - Stroke: #bbb, opacity 0.4, width 1px
  - Month labels positioned above each line: Font size 11px, font-weight 600, color #666

  **"Now" marker**:
  - Red vertical dashed line at position x=823 (early April)
  - Stroke: #EA4335, width 2px, stroke-dasharray "5,3"
  - Label "NOW" above line: Font size 10px, font-weight 700, color #EA4335

  **Milestone timelines** (horizontal colored lines for each major milestone M1, M2, M3):
  - Each milestone displays as a horizontal line spanning Jan–Jun at different y-positions
  - M1 timeline: Stroke #0078D4 (blue), width 3px, positioned at y=42
  - M2 timeline: Stroke #00897B (teal), width 3px, positioned at y=98
  - M3 timeline: Stroke #546E7A (gray), width 3px, positioned at y=154
  
  **Milestone start circles** (major milestones):
  - Styled as large circles (r=7) with white fill and colored stroke (matches timeline color)
  - Positioned at milestone start dates
  
  **Milestone checkpoints** (progress markers):
  - Styled as smaller circles (r=4-5) with gray fill and stroke (#999)
  - Positioned at checkpoint dates between major milestones

  **Milestone endpoints** (PoC and Production releases):
  - Styled as diamonds (4-point polygons)
  - PoC milestones: Yellow (#F4B400) fill with drop-shadow filter
  - Production release milestones: Green (#34A853) fill with drop-shadow filter
  - Positioned at exact end dates from milestones[] array
  - Date labels positioned below/above diamond: Font size 10px, color #666

### Heatmap Section (OriginalDesignConcept.html `.hm-wrap` class)
- **Flex**: 1 (fills remaining vertical space)
- **Padding**: 10px 44px
- **Display**: Flex column
- **Min-height**: 0 (allows proper overflow handling)

**Heatmap title** (.hm-title):
- **Font-size**: 14px, font-weight 700
- **Color**: #888
- **Letter-spacing**: 0.5px
- **Text-transform**: UPPERCASE
- **Text content**: "MONTHLY EXECUTION HEATMAP — SHIPPED ✓ IN PROGRESS ? CARRYOVER ↻ BLOCKERS ✖"
- **Margin-bottom**: 8px

**Heatmap grid** (.hm-grid):
- **Display**: CSS Grid
- **Grid-template-columns**: 160px repeat(4, 1fr) [5 columns total: status label + 4 months]
- **Grid-template-rows**: 36px repeat(4, 1fr) [5 rows total: month headers + 4 status rows]
- **Border**: 1px solid light gray (#E0E0E0)
- **Flex**: 1, min-height 0

**Grid corner cell** (.hm-corner):
- **Grid position**: Top-left (row 1, column 1)
- **Background**: Light gray (#F5F5F5)
- **Display**: Flex, centered content
- **Font-size**: 11px, font-weight 700
- **Color**: #999
- **Text-transform**: UPPERCASE
- **Text content**: "Status"
- **Border-right**: 1px solid #E0E0E0
- **Border-bottom**: 2px solid #CCC

**Month header cells** (.hm-col-hdr):
- **Grid position**: Row 1, columns 2–5
- **Background**: Light gray (#F5F5F5) [default]
- **Font-size**: 16px, font-weight 700
- **Color**: #111
- **Display**: Flex, centered content
- **Border-right**: 1px solid #E0E0E0
- **Border-bottom**: 2px solid #CCC
- **Current month styling** (.apr-hdr): Background light yellow (#FFF0D0), text color dark orange (#C07700)
- **Text content**: Month names from data.json quarters[] array (e.g., "March", "April ? Now", "May", "June")
- **Note**: Current month column includes "? Now" suffix and "apr-hdr" styling

**Status row headers** (.hm-row-hdr):
- **Grid position**: Rows 2–5, column 1
- **Display**: Flex, align-items center
- **Padding**: 0 12px
- **Font-size**: 11px, font-weight 700
- **Text-transform**: UPPERCASE
- **Letter-spacing**: 0.7px
- **Border-right**: 2px solid #CCC
- **Border-bottom**: 1px solid #E0E0E0
- **Color and background vary by status type**:

  **Shipped row** (.ship-hdr):
  - Color: Dark green (#1B7A28)
  - Background: Light green (#E8F5E9)
  - Text: "✓ Shipped"

  **In Progress row** (.prog-hdr):
  - Color: Dark blue (#1565C0)
  - Background: Light blue (#E3F2FD)
  - Text: "? In Progress"

  **Carryover row** (.carry-hdr):
  - Color: Dark orange (#B45309)
  - Background: Light yellow (#FFF8E1)
  - Text: "↻ Carryover"

  **Blockers row** (.block-hdr):
  - Color: Dark red (#991B1B)
  - Background: Light pink (#FEF2F2)
  - Text: "✖ Blockers"

**Status data cells** (.hm-cell):
- **Grid position**: Rows 2–5, columns 2–5
- **Padding**: 8px 12px
- **Border-right**: 1px solid #E0E0E0
- **Border-bottom**: 1px solid #E0E0E0
- **Overflow**: hidden
- **Background color varies by status type and month**:

  **Shipped cells**: Background #F0FBF0 [default], #D8F2DA [current month]
  **In Progress cells**: Background #EEF4FE [default], #DAE8FB [current month]
  **Carryover cells**: Background #FFFDE7 [default], #FFF0B0 [current month]
  **Blockers cells**: Background #FFF5F5 [default], #FFE4E4 [current month]

**Status item text** (.it):
- **Font-size**: 12px
- **Color**: #333
- **Padding**: 2px 0 2px 12px
- **Position**: relative
- **Line-height**: 1.35
- **Content**: Item names from data.json (e.g., "Chatbot MVP", "Role Templates")
- **Dot indicator** (.it::before):
  - **Content**: Empty, displays as colored circle
  - **Position**: absolute, left 0, top 7px
  - **Dimensions**: 6px × 6px, border-radius 50%
  - **Color**: Matches status type:
    - Shipped: Green (#34A853)
    - In Progress: Blue (#0078D4)
    - Carryover: Yellow/gold (#F4B400)
    - Blockers: Red (#EA4335)

**Empty cell indicator**:
- **Text**: "—" (en-dash)
- **Color**: Light gray (#AAA)
- **Used when**: No items in that status row for that month

### Color Palette (Complete)
| Use Case | Color | Hex |
|----------|-------|-----|
| Shipped header background | Light green | #E8F5E9 |
| Shipped cell background | Light green | #F0FBF0 |
| Shipped cell background (current month) | Medium green | #D8F2DA |
| Shipped indicator dot | Dark green | #34A853 |
| In Progress header background | Light blue | #E3F2FD |
| In Progress cell background | Light blue | #EEF4FE |
| In Progress cell background (current month) | Medium blue | #DAE8FB |
| In Progress indicator dot | Blue | #0078D4 |
| Carryover header background | Light yellow | #FFF8E1 |
| Carryover cell background | Light yellow | #FFFDE7 |
| Carryover cell background (current month) | Medium yellow | #FFF0B0 |
| Carryover indicator dot | Gold | #F4B400 |
| Blockers header background | Light red | #FEF2F2 |
| Blockers cell background | Light red | #FFF5F5 |
| Blockers cell background (current month) | Medium red | #FFE4E4 |
| Blockers indicator dot | Red | #EA4335 |
| M1 timeline line | Blue | #0078D4 |
| M2 timeline line | Teal | #00897B |
| M3 timeline line | Gray | #546E7A |
| PoC milestone diamond | Yellow | #F4B400 |
| Production release diamond | Green | #34A853 |
| Checkpoint circle | Gray | #999 |
| Now marker line | Red | #EA4335 |
| Primary text | Dark gray | #111 |
| Secondary text | Medium gray | #888 |
| Tertiary text | Light gray | #666 |
| Body text | Dark gray | #333 |
| Link text | Blue | #0078D4 |
| Border color | Light gray | #E0E0E0 |
| Heavy border color | Gray | #CCC |
| Header/section background | Very light gray | #F5F5F5 |
| Page background | White | #FFFFFF |
| Timeline area background | Very light gray | #FAFAFA |
| Current month header background | Light yellow/orange | #FFF0D0 |
| Current month header text | Dark orange | #C07700 |

## UI Interaction Scenarios

### Scenario 1: User Opens Dashboard and Sees Complete Project Status
**When** a user navigates to the dashboard URL and page load completes, **then** the dashboard displays all three sections (header, timeline, heatmap) with data fully populated and formatted according to the Visual Design Specification. **Expected**: Header shows project name and subtitle, legend displays all four milestone types, timeline SVG renders all milestones and the current date marker, heatmap grid displays all status rows with data. No loading spinner or placeholder state is visible after 2-second timeout.

### Scenario 2: User Reads Project Title and Status Context
**When** a user views the header section, **then** they immediately understand: (a) the project name, (b) the organization/workstream that owns it, (c) the current month/year. **Expected**: Title reads "My Project", subtitle reads "[Organization] • [Workstream] • April 2026" or equivalent from data.json.

### Scenario 3: User Identifies Current Timeline Position with "Now" Marker
**When** a user looks at the timeline SVG, **then** they instantly recognize the current date by a prominent red vertical dashed line labeled "NOW". **Expected**: The red line appears at the correct x-position corresponding to today's date, positioned between month gridlines. The line is visually distinct from milestone timelines (different color, dashed style, thinner stroke).

### Scenario 4: User Scans Milestone Timeline for Delivery Dates
**When** a user views the timeline section, **then** they see three primary milestone lines (M1, M2, M3) spanning the Jan–Jun timeline, each with a color-coded horizontal line, labeled start/end dates, and milestone type markers (circles, diamonds). **Expected**: Left sidebar lists M1, M2, M3 with short descriptions; SVG timeline shows corresponding lines with start/end dates; PoC milestones appear as yellow diamonds, production releases as green diamonds, checkpoints as gray circles. Hovering over any shape reveals a tooltip with label and exact date.

### Scenario 5: User Compares Execution Health Across Months
**When** a user examines the heatmap grid, **then** they can quickly identify which months had the highest shipped velocity (green rows, item counts), which have heavy in-progress work (blue rows), which have carryover risk (yellow rows), and which have blockers (red rows). **Expected**: Each cell displays 2–4 item names with colored dot indicators; current month (April) has a darker background shade to visually distinguish it; empty cells show a dash ("—") rather than blank space; row headers clearly label each status type.

### Scenario 6: User Hovers Over a Heatmap Cell to See Details
**When** a user hovers over any heatmap cell containing items, **then** a tooltip appears showing the full list of items in that cell. **Expected**: Tooltip displays item names as a bulleted list, background is semi-transparent dark gray, tooltip disappears when mouse leaves cell.

### Scenario 7: User Identifies Current Month in Grid
**When** a user scans the heatmap column headers, **then** they immediately recognize the current month by a distinct background color and a "? Now" label suffix. **Expected**: Current month column (April) has light yellow/orange background (#FFF0D0) with text "April ? Now" in dark orange (#C07700); all other month columns have default light gray background (#F5F5F5) with black text.

### Scenario 8: User Sees Milestone Checkpoints and Progress Markers
**When** a user examines the timeline, **then** between major milestone markers (start and end), they see smaller checkpoint circles indicating progress gates, verification points, or intermediate delivery dates. **Expected**: Checkpoint circles are gray (#999), smaller than start/end markers (r=4-5 pixels), positioned at exact dates from milestones[] array with checkpoint type.

### Scenario 9: User Screenshots Dashboard for PowerPoint Export
**When** a user presses Print Screen or uses browser Print → Save as PDF, **then** the entire dashboard renders as a single static image with all colors, text, and layouts preserved exactly as displayed on screen. **Expected**: No animations, no hover states, no scrollbars visible; layout remains 1920×1080 aspect ratio; all text is readable; grid lines and cell borders are crisp; PDF export prints on a single landscape page without truncation.

### Scenario 10: User Refreshes Page After data.json Update
**When** a project manager updates data.json (adds new items, changes months, updates milestone dates) and the user refreshes the browser, **then** the dashboard immediately displays updated data without any delay or confirmation dialog. **Expected**: Page reloads in under 2 seconds; new items appear in their respective cells; timeline SVG recalculates milestone positions based on updated dates; heatmap rows update to match new quarters array.

### Scenario 11: User Views "Last Updated" Timestamp
**When** a user views the dashboard header or footer, **then** they see a timestamp indicating when data.json was last modified. **Expected**: Timestamp format is "Last Updated: Apr 12, 2026 at 2:51 AM UTC"; timestamp is automatically calculated from file system; timestamp updates after each data.json modification and page refresh.

### Scenario 12: User Experiences Missing or Invalid data.json
**When** data.json is missing, malformed JSON, or invalid schema, **then** the dashboard displays a user-friendly error message instead of a blank screen. **Expected**: Error message reads "Unable to load project data. Please check that data.json exists and contains valid JSON." or similar; error does not expose stack traces or technical details; an example valid data.json schema is provided in documentation.

### Scenario 13: User Views Heatmap on Small Screen (Responsive Behavior)
**When** a user views the dashboard on a tablet or narrow desktop window, **then** the layout adapts by: reducing font sizes, collapsing the legend to a single-column list, or hiding non-essential details. **Expected**: Heatmap remains readable; column widths scale proportionally; text does not overlap; scrolling is smooth and does not break layout. (Note: MVP may assume 1920×1080 desktop resolution; responsive design is a future enhancement.)

### Scenario 14: User Exports Dashboard as High-Resolution PNG
**When** a user uses automated screenshot tools (e.g., Playwright, Selenium) to capture the dashboard as PNG, **then** the resulting image matches a known baseline PNG with pixel-perfect accuracy. **Expected**: Rendering is deterministic (same output every time); colors match exact hex values; font rendering is consistent; no timing-dependent elements (animations, lazy-load images) introduce variability.

### Scenario 15: User Clicks on Milestone Diamond or Checkpoint Circle
**When** a user clicks on a milestone marker in the timeline, **then** a modal or side panel appears showing detailed information about that milestone (label, date, associated deliverables, status, owner). **Expected**: Modal includes: milestone name, date, type (PoC/release/checkpoint), linked ADO work items (if configured), and any notes from data.json. Clicking outside modal closes it. (Note: This is a stretch goal; MVP assumes timeline is read-only.)

## Scope

### In Scope
- **Single-page dashboard** displaying project health across four status categories (shipped, in progress, carryover, blockers)
- **JSON-driven data model**: Read all project data from data.json file; no database queries or external APIs
- **SVG timeline visualization**: Render milestone markers (PoC, production, checkpoint) at date-aligned positions with current date "Now" marker
- **CSS Grid heatmap**: Display status items in 4-month × 4-status grid with color-coded cells and dot indicators
- **Exact design compliance**: Replicate OriginalDesignConcept.html layout, colors, typography, and spacing precisely
- **Screenshot optimization**: Ensure deterministic rendering at 1920×1080 resolution suitable for PowerPoint exports
- **Date-driven positioning**: Use NodaTime or DateTime to calculate milestone and month-column positions based on dates in data.json
- **No authentication**: Dashboard is publicly accessible on local network; no login required
- **No database**: All data persists in data.json file only; no database server, no cache layer
- **Stateless application**: Dashboard renders identically on every load; no session state, no user preferences stored
- **Browser compatibility**: Support latest versions of Chrome, Edge, Firefox on Windows/Mac/Linux
- **Unit tests**: Write tests for JSON deserialization, date calculations, and heatmap rendering logic
- **Documentation**: Provide data.json schema, deployment instructions, and screenshot capture workflow
- **Print stylesheet**: Ensure dashboard prints cleanly on single landscape page

### Out of Scope
- **Authentication & authorization**: No login, no user roles, no permission checks
- **Multi-project support**: Dashboard displays one project at a time (no project selector dropdown in MVP)
- **Database integration**: No SQL database, no Entity Framework, no connection strings
- **Real-time sync**: No WebSocket push notifications; user must refresh browser to see data.json updates
- **Mobile-responsive design**: Dashboard is optimized for 1920×1080 desktop only (responsive design deferred to post-MVP)
- **Dark mode**: Only light theme in MVP; dark mode CSS variant is a future enhancement
- **Drill-down interactivity**: Clicking heatmap cells to see detailed task lists is out of scope; timeline remains read-only
- **ADO/Jira integration**: No API calls to Azure DevOps or Jira; milestone links are static labels only
- **Multi-language support**: Dashboard is English-only; i18n localization deferred
- **Advanced charting**: No burn-down charts, velocity trends, or predictive analytics; heatmap and timeline only
- **Export formats**: No CSV export, no JSON export; screenshots/PowerPoint are the only export method
- **Data validation**: No schema validation in UI; invalid data.json results in error message, not data correction
- **Auto-refresh**: Dashboard does not poll data.json automatically; manual F5 refresh required
- **Custom color palettes**: Color scheme is fixed per OriginalDesignConcept.html (no theme switcher)
- **Performance optimization for >12 months**: Heatmap assumes 4-month display window; pagination or scrolling for longer timelines deferred

## Non-Functional Requirements

### Performance
- **Initial page load**: Dashboard must render completely within 2 seconds on a standard corporate network (10 Mbps minimum)
- **Data loading**: data.json file (< 10 KB) must deserialize and render in < 500ms
- **Browser memory**: Application memory footprint must remain under 50 MB during typical usage
- **No external dependencies**: All rendering (SVG, CSS Grid, fonts) must complete in the browser without waiting for external resources (CDN, APIs)
- **Deterministic rendering**: Same output on every page load; no random layout shifts or animations

### Reliability
- **99.9% uptime**: Application must remain available for local network access (assumes Kestrel/IIS Express on stable Windows machine)
- **Error recovery**: If data.json is missing or malformed, display graceful error message and offer troubleshooting steps
- **Data integrity**: JSON deserialization must validate required fields (projectName, quarters, milestones); missing fields trigger error, not silent fallback
- **No console errors**: Browser dev tools must show no JavaScript errors, warnings, or unhandled exceptions on normal usage

### Scalability
- **Data model**: Schema must support up to 12 months of timeline history and future planning, with up to 20 items per status category per month
- **No server-side state**: Blazor Server must not accumulate session data; each refresh should be independent
- **Concurrent users**: Application must support 10+ concurrent users on same network without performance degradation (stateless design guarantees this)

### Security
- **No secrets in code**: API keys, connection strings, or credentials must not appear in source code
- **No network encryption required**: Assumes internal/trusted network only; HTTPS is optional for MVP on localhost
- **data.json visibility**: File is human-readable and unencrypted; assume no sensitive/confidential data stored in project status strings
- **No SQL injection**: Application uses built-in JSON deserialization; no string concatenation or dynamic SQL
- **CSRF protection**: Not applicable (no form submissions, no state-changing operations)

### Usability
- **Scannable layout**: Executive must identify current month, velocity, and blockers in < 10 seconds of viewing
- **Color accessibility**: Heatmap colors must be distinguishable for colorblind users (use distinct hues: green/blue/yellow/red, not red/green only)
- **Font readability**: Body text (12px Segoe UI) must be readable on standard 24" monitors at typical viewing distance (> 2 feet)
- **Consistent styling**: All UI elements must match OriginalDesignConcept.html design spec exactly; no ad-hoc styling or CSS drift

### Maintainability
- **Code comments**: C# business logic should be commented only where non-obvious; CSS should be minimal and self-explanatory
- **Service isolation**: DashboardDataService, DateCalculationService, VisualizationService should be independently testable
- **No magic numbers**: All colors, sizes, and offsets should be extracted to constants or configuration
- **Documentation**: data.json schema, Blazor component props, and service interfaces should be well-documented with XML doc comments

### Deployment
- **Self-contained publish**: Application should publish as a single .exe file (dotnet publish -c Release -r win-x64 --self-contained)
- **No external dependencies**: No need to install .NET runtime on target machine if using self-contained publish
- **Configuration via appsettings**: Environment-specific settings (data file path, port) must be configurable without code changes
- **Local network deployment**: Application should start and listen on all interfaces (0.0.0.0) by default for internal network access

## Success Metrics

1. **Dashboard renders correctly**: Visual layout matches OriginalDesignConcept.html pixel-for-pixel; all four status rows display with correct colors and formatting
2. **Data loading works**: data.json schema is correctly parsed; sample data with 4 quarters and 3 milestones renders without errors
3. **Timeline is accurate**: Milestone markers align to correct dates on SVG timeline; current date "Now" marker is positioned correctly; all milestone types (PoC, release, checkpoint) are visually distinct
4. **Heatmap displays properly**: Heatmap grid renders 4-month columns and 4 status rows; current month has darker background; items are populated from data.json with correct dot indicators; empty cells show dashes
5. **Screenshots are reproducible**: Taking screenshots of the same dashboard at different times produces pixel-identical images (no animations, no timing-dependent rendering)
6. **PowerPoint workflow is smooth**: Executive can screenshot dashboard (Print Screen or browser print→PNG), paste into PowerPoint, and have a professional-looking status slide in < 5 minutes
7. **Data updates are simple**: Project manager can edit data.json (add/remove items, change dates, update months) and see changes reflected after browser refresh with no code modification
8. **Error handling is graceful**: Invalid data.json or missing files result in user-friendly error message, not blank screen or exception details
9. **Documentation is complete**: All stakeholders understand data.json schema, how to deploy locally, and how to capture screenshots
10. **Unit tests pass**: Data deserialization tests, date calculation tests, and heatmap rendering tests all pass; code coverage > 80% for business logic

## Constraints & Assumptions

### Technical Constraints
- **Windows development environment**: Primary development and deployment platform is Windows 10/11 with .NET 8 SDK installed
- **No Linux requirement**: MVP assumes deployment on Windows only; Docker/Linux support deferred to post-MVP
- **Blazor Server limitation**: Server must run in same network environment as users; Blazor Server is not optimized for high-latency or unreliable networks
- **SVG rendering**: Timeline uses SVG with fixed coordinates calculated at render time; no vector graphics library or D3.js
- **CSS Grid support**: Assumes modern browsers (Chrome 57+, Edge 16+, Firefox 52+) with full CSS Grid support; no IE11 compatibility
- **No JavaScript frameworks**: Application uses Blazor/C# only; no Vue.js, React, or Angular (keeps dependencies minimal)
- **data.json file location**: File must reside in wwwroot/data/ directory or configurable path in appsettings.json; no relative path traversal allowed

### Assumptions
- **Team has .NET 8 expertise**: Development and maintenance is performed by C# developers familiar with Blazor, POCO models, and System.Text.Json
- **Project timeline is < 12 months**: Heatmap display is optimized for 4–6 months; longer timelines require pagination (post-MVP feature)
- **data.json is updated manually**: No automated data sync from external systems (ADO, Jira, etc.); project manager manually edits JSON
- **Executives have network access**: Dashboard is served on internal network only; no public internet exposure required
- **No mobile usage**: Dashboard is not accessed from mobile phones or tablets in MVP; responsive design is deferred
- **Data is not sensitive**: Project status strings (item names, status labels) do not contain confidential information; file is stored unencrypted
- **Browser history is not relevant**: Users do not navigate back/forward; each page load fetches latest data.json
- **Data.json updates are infrequent**: No need for real-time push notifications; users manually refresh to see changes (F5)
- **Current date is system date**: "Now" marker uses server system time; assumes accurate system clock on deployment machine
- **Color palette is sufficient**: Four distinct status colors (green/blue/yellow/red) do not need customization; color scheme is fixed
- **Print output is adequate**: Executive can screenshot and embed in PowerPoint; no need for formal PDF report generation or scheduling
- **No audit trail needed**: No requirement to track who modified data.json when; version control (Git) is assumed to be the audit source
- **Schema stability**: data.json schema is finalized before development; post-launch changes to schema will require data migration
- **Single deployment location**: Application is deployed once on a shared network folder or single machine; no need for multiple instances or load balancing

### Timeline Assumptions
- **MVP timeline**: 2 weeks from start to functional dashboard with sample data
- **Week 1**: Blazor project setup, CSS/HTML template porting, data model design, sample data creation
- **Week 2**: Component rendering, data loading, testing, documentation
- **Stretch goals**: Print stylesheet, "last updated" timestamp, dark mode (only if time permits)
- **Post-MVP enhancements**: Mobile responsiveness, multi-project support, drill-down interactivity, ADO integration (future releases)

### Dependency Assumptions
- **.NET 8 LTS** is available and will remain supported for duration of project
- **NodaTime 4.1+** is a stable, compatible dependency for date calculations
- **Visual Studio 2022 or Rider** is the development IDE
- **Windows Kestrel** or **IIS Express** is the web server for local development and deployment
- **Git** is used for version control; code is stored in a private repository

---

**Document Version**: 1.0  
**Last Updated**: April 12, 2026  
**Author**: Product Management  
**Status**: Ready for Architecture & Development