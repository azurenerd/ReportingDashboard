# PM Specification: My Project

## Executive Summary

This project delivers a lightweight, screenshot-friendly executive dashboard for reporting on project milestones, execution status, and progress. The dashboard reads project metadata from a `data.json` configuration file and renders a timeline-based view with monthly status heatmap visualization using C# .NET 8 and Blazor Server. Built for simplicity and visual clarity, the dashboard requires no authentication or enterprise security infrastructure and is optimized for executive PowerPoint deck creation through clean, professional reporting visuals.

## Business Goals

1. **Provide Real-Time Executive Visibility**: Enable executives to understand project status at a glance through a single-page dashboard showing what was shipped, what is in progress, what carried over, and blocking issues.
2. **Enable Screenshot-Ready Reporting**: Create a dashboard that produces publication-quality visuals suitable for direct use in PowerPoint presentations without additional design work.
3. **Minimize Setup & Maintenance Overhead**: Build a simple, locally-deployed solution with no authentication, cloud infrastructure, or complex data pipelines—executives should be able to view the dashboard with zero friction.
4. **Support Quick Data Updates**: Enable project managers to update dashboard data by editing a JSON configuration file without requiring code deployment or database administration.
5. **Establish Consistent Status Reporting**: Create a standardized visual language for project health (shipped, in progress, carryover, blockers) that can be reused across multiple projects and time periods.

## User Stories & Acceptance Criteria

### Story 1: View Project Header & Summary
**As an** executive  
**I want to** see the project name, current timeframe, and team context immediately when I open the dashboard  
**so that** I can instantly understand which project and organization this report covers.

**Acceptance Criteria:**
- [ ] Dashboard displays project title prominently at top (referencing Visual Design Specification: Header section)
- [ ] Subtitle shows organization, workstream, and current month/year
- [ ] Optional: ADO Backlog link is clickable and opens external link in new tab
- [ ] Header spans full viewport width with 1920px layout
- [ ] Text styling matches design mock: 24px bold heading, 12px gray subtitle
- [ ] No loading state on header (loads synchronously with page)

### Story 2: View Milestone Timeline
**As an** executive  
**I want to** see a timeline spanning 6 months with key milestones marked as diamonds or circles  
**so that** I can understand the major delivery dates and dependencies in the project roadmap.

**Acceptance Criteria:**
- [ ] Timeline displays horizontal bars for 3+ milestone tracks (e.g., M1, M2, M3)
- [ ] Month labels appear above timeline: Jan, Feb, Mar, Apr, May, Jun (or configurable range)
- [ ] "Now" marker appears as red dashed line at current date (from data.json `nowMarker`)
- [ ] Milestone markers rendered as:
  - [ ] Diamond shape (45° rotated square) for PoC milestones (yellow/amber, #F4B400)
  - [ ] Diamond shape for Production releases (green, #34A853)
  - [ ] Circle for checkpoints (gray outline, #999)
  - [ ] Line segment for current date (red, #EA4335)
- [ ] Each milestone displays label below timeline with date and type (e.g., "Mar 26 PoC")
- [ ] Milestones are sized and positioned proportionally by date (pixel-per-day calculation)
- [ ] Timeline is SVG-rendered or equivalent vector format (not raster image)
- [ ] Tooltip on hover (Scenario 1 in UI Interaction Scenarios)

### Story 3: View Monthly Status Heatmap
**As an** executive  
**I want to** see a grid showing project status (Shipped, In Progress, Carryover, Blockers) across 4 months  
**so that** I can quickly assess the momentum and identify bottlenecks.

**Acceptance Criteria:**
- [ ] Heatmap displays as CSS Grid with 5 columns (Status row label + 4 months) and 5 rows (header + 4 status categories)
- [ ] Column headers show month names (Mar, Apr, May, Jun) with "April - Now" highlighted in amber background
- [ ] Row headers show status categories: ✓ Shipped, ● In Progress, ⚠ Carryover, ✗ Blockers
- [ ] Each cell contains 0-3 bullet-listed items with single-line text labels
- [ ] Status rows are color-coded:
  - [ ] Shipped: Green backgrounds (#E8F5E9 header, #F0FBF0 cells, #D8F2DA for "Now" month) with green bullets (#34A853)
  - [ ] In Progress: Blue backgrounds (#E3F2FD header, #EEF4FE cells, #DAE8FB for "Now" month) with blue bullets (#0078D4)
  - [ ] Carryover: Amber backgrounds (#FFF8E1 header, #FFFDE7 cells, #FFF0B0 for "Now" month) with amber bullets (#F4B400)
  - [ ] Blockers: Red backgrounds (#FEF2F2 header, #FFF5F5 cells, #FFE4E4 for "Now" month) with red bullets (#EA4335)
- [ ] Item bullets appear as 6px filled circles positioned left of text
- [ ] Grid borders: light gray (#E0E0E0) between cells; darker borders (#CCC) between row headers and data
- [ ] Heatmap title above grid: "Monthly Execution Heatmap - Shipped ✓ In Progress ● Carryover ⚠ Blockers ✗" (uppercase, gray, 14px)
- [ ] Items render from data.json statusRows array dynamically

### Story 4: View Legend & Status Indicators
**As an** executive  
**I want to** see a legend explaining all visual markers and colors  
**so that** I can interpret the dashboard without prior training.

**Acceptance Criteria:**
- [ ] Legend displays in header right section with 4 entries:
  - [ ] PoC Milestone (amber diamond, #F4B400)
  - [ ] Production Release (green diamond, #34A853)
  - [ ] Checkpoint (gray circle, #999)
  - [ ] Now (red vertical line, #EA4335)
- [ ] Each legend entry shows marker icon + label text (12px font)
- [ ] Legend items laid out horizontally with 22px gap between items
- [ ] Icons match timeline and heatmap styling exactly

### Story 5: Load Data from JSON Configuration
**As a** project manager  
**I want to** update the dashboard by editing data.json file  
**so that** I can refresh status reports without redeploying code.

**Acceptance Criteria:**
- [ ] Application reads `data.json` from file system (hardcoded path or configurable via appsettings.json)
- [ ] Data structure supports:
  - [ ] `project` object: name, subtitle, adoBacklogUrl
  - [ ] `milestones` array: id, title, color, date (ISO 8601), type (checkpoint/poc/release)
  - [ ] `statusRows` array: label, category (shipped/inprogress/carryover/blockers), items (month, value)
  - [ ] `nowMarker` string: ISO 8601 date for current date line
- [ ] Application deserializes JSON into strongly-typed C# POCO models
- [ ] If data.json is missing or malformed, display user-friendly error message (not stack trace)
- [ ] Page loads within 2 seconds from cold start
- [ ] Manual refresh button reloads data.json without page reload

### Story 6: Render Dashboard at Fixed 1920x1080 Viewport
**As an** executive taking screenshots  
**I want to** see the dashboard at exactly 1920x1080 resolution  
**so that** screenshots fit cleanly into PowerPoint slides without scaling artifacts.

**Acceptance Criteria:**
- [ ] Body element styled with fixed `width: 1920px; height: 1080px; overflow: hidden`
- [ ] All layout using Flexbox or CSS Grid (no scroll bars)
- [ ] Header area: fixed height ~60px (padding 12px top/bottom), spans full width
- [ ] Timeline area: fixed height ~196px, spans full width
- [ ] Heatmap area: fills remaining vertical space (flex: 1)
- [ ] Content does not overflow viewport
- [ ] Font sizes and spacing match design mock pixel-for-pixel
- [ ] Chrome DevTools shows no scrollbars at 1920x1080

### Story 7: Responsive Print Stylesheet for PowerPoint Screenshots
**As an** executive exporting screenshots  
**I want to** hide UI chrome (buttons, refresh controls) when capturing screenshots  
**so that** the visual focus is on the dashboard content alone.

**Acceptance Criteria:**
- [ ] Page includes optional print stylesheet with @media print rules
- [ ] Print mode hides any refresh buttons, navigation, or non-content UI elements
- [ ] Print mode preserves all color styling and layout
- [ ] Manual screenshot (Win+Shift+S, Chrome DevTools) captures clean content

## Visual Design Specification

### Design Reference File
**Primary Source**: `OriginalDesignConcept.html` (Design Visual Reference section above)

The dashboard is a single-page executive report rendered at 1920×1080 pixel fixed size. The layout uses modern CSS Grid and Flexbox to organize hierarchical information from top to bottom: header context, timeline milestones, and monthly status heatmap.

### Layout Structure & Viewport

**Overall Container:**
- Body: 1920px wide × 1080px tall, fixed size, no scroll
- Background: #FFFFFF (white)
- Font family: Segoe UI, Arial (system sans-serif fallback)
- Text color: #111 (near-black)
- Overflow: hidden (no scroll bars)
- Display: Flexbox column (flex-direction: column)

**Three-Section Layout (top to bottom):**
1. Header section (flex-shrink: 0, ~60px)
2. Timeline section (flex-shrink: 0, 196px)
3. Heatmap section (flex: 1, fills remaining space)

### Header Section

**Container:**
- Padding: 12px 44px (top/bottom 12px, left/right 44px)
- Border-bottom: 1px solid #E0E0E0
- Display: Flex with space-between alignment
- Left side: Title + subtitle
- Right side: Legend items

**Title:**
- Font size: 24px, font-weight: 700 (bold)
- Color: #111
- Optional link styling: color #0078D4, no underline
- Spacing: Subtitle appears 2px below title

**Subtitle:**
- Font size: 12px
- Color: #888 (light gray)
- Format: "[Organization] · [Workstream] · [Current Month Year]"

**Legend (Right Side):**
- Display: Flex with 22px gap between items
- Each legend item: Flex with 6px gap between icon and label
- Label font: 12px, color #111
- Icons:
  - PoC Milestone: 12×12px diamond (rotated 45°), background #F4B400, flex-shrink: 0
  - Production Release: 12×12px diamond (rotated 45°), background #34A853, flex-shrink: 0
  - Checkpoint: 8×8px circle (border-radius: 50%), background #999, flex-shrink: 0
  - Now: 2px wide × 14px tall line, background #EA4335, flex-shrink: 0

### Timeline Section

**Container:**
- Padding: 6px 44px 0
- Height: 196px
- Display: Flex with align-items: stretch
- Border-bottom: 2px solid #E8E8E8
- Background: #FAFAFA (off-white)

**Left Sidebar (Milestone Labels):**
- Width: 230px, flex-shrink: 0
- Display: Flex column with space-around
- Padding: 16px 12px 16px 0
- Border-right: 1px solid #E0E0E0
- For each milestone (M1, M2, M3, etc.):
  - Label ID (e.g., "M1"): 12px font-weight: 600, color: milestone-specific (e.g., #0078D4 for M1)
  - Milestone title (e.g., "Chatbot & MS Role"): 12px font-weight: 400, color: #444, displayed as secondary line
  - Line-height: 1.4

**SVG Timeline Box:**
- Flex: 1, padding-left: 12px, padding-top: 6px
- Contains SVG element with width: 1560px, height: 185px
- SVG overflow: visible (allows text labels to extend beyond bounds)

**SVG Content:**
- Background: transparent
- Vertical month dividers: Thin gray lines (#bbb) at 260px intervals (Jan, Feb, Mar, Apr, May, Jun)
- Month labels: 11px font-weight: 600, color: #666, positioned above first divider at y=14px
- Horizontal timeline bars for each milestone track:
  - Stroke width: 3px
  - Stroke colors: vary by milestone (e.g., #0078D4 for M1)
  - Y-position: 42px for M1, 98px for M2, 154px for M3, etc.
- Milestone markers (dates along each timeline):
  - Checkpoint: Circle marker (r=7 for current, r=5 for historical, r=4 for checkpoint)
  - PoC/Release: Diamond (rotated 45° square, 10×10px)
  - Fill colors: White with colored stroke for open markers; filled for completed
  - Drop shadow filter (dx=0, dy=1, stdDeviation=1.5, flood-opacity=0.3) applied to diamond markers
- Date labels: 10px font, color: #666, positioned above/below markers (y-offset varies)
- Now marker: Red dashed line (#EA4335) with "NOW" label (10px, font-weight: 700, color: #EA4335)

**Color Mapping for Milestone Bars:**
- M1 (Chatbot & MS Role): #0078D4 (blue)
- M2 (PDS & Data Inventory): #00897B (teal)
- M3 (Auto Review DFD): #546E7A (slate gray)
- (Additional milestones: derived from data.json color property)

### Heatmap Section

**Container:**
- Flex: 1 (fills remaining vertical space)
- Min-height: 0 (allows flex child to shrink below content size)
- Display: Flex column
- Padding: 10px 44px 10px
- Overflow: auto (if content exceeds height, scrollable)

**Title:**
- Font size: 14px, font-weight: 700
- Color: #888
- Letter-spacing: 0.5px
- Text-transform: uppercase
- Margin-bottom: 8px
- Content: "Monthly Execution Heatmap - Shipped ✓ In Progress ● Carryover ⚠ Blockers ✗" (or similar)

**Heatmap Grid:**
- Display: CSS Grid
- Grid-template-columns: 160px repeat(4, 1fr) — first column is row labels, next 4 are month columns
- Grid-template-rows: 36px repeat(4, 1fr) — first row is month headers, next 4 are status rows
- Flex: 1, min-height: 0
- Border: 1px solid #E0E0E0 (outer border only; internal cells have individual borders)

**Grid Cells:**

*Corner Cell (top-left, Status header):*
- Background: #F5F5F5
- Display: Flex with center alignment
- Font size: 11px, font-weight: 700
- Color: #999
- Text-transform: uppercase
- Border-right: 1px solid #E0E0E0
- Border-bottom: 2px solid #CCC

*Column Headers (Month names: Mar, Apr, May, Jun):*
- Background: #F5F5F5
- Display: Flex with center alignment
- Font size: 16px, font-weight: 700
- Color: #111
- Border-right: 1px solid #E0E0E0
- Border-bottom: 2px solid #CCC
- Special highlight for current month (April): Background #FFF0D0, color: #C07700

*Row Headers (Status category labels):*
- Min-width: ~140px (auto-width from content)
- Display: Flex with left alignment
- Padding: 0 12px
- Font size: 11px, font-weight: 700
- Text-transform: uppercase
- Letter-spacing: 0.7px
- Border-right: 2px solid #CCC
- Border-bottom: 1px solid #E0E0E0
- Background color & text color vary by row:
  - **Shipped** (.ship-hdr): text #1B7A28 (dark green), bg #E8F5E9 (light green)
  - **In Progress** (.prog-hdr): text #1565C0 (dark blue), bg #E3F2FD (light blue)
  - **Carryover** (.carry-hdr): text #B45309 (dark amber), bg #FFF8E1 (light amber)
  - **Blockers** (.block-hdr): text #991B1B (dark red), bg #FEF2F2 (light red/pink)

*Data Cells:*
- Padding: 8px 12px
- Border-right: 1px solid #E0E0E0
- Border-bottom: 1px solid #E0E0E0
- Overflow: hidden (truncate text if needed)
- Last cell in each row: border-right: none
- Background color varies by row & month:
  - Shipped: #F0FBF0 normally, #D8F2DA for current month (April)
  - In Progress: #EEF4FE normally, #DAE8FB for current month
  - Carryover: #FFFDE7 normally, #FFF0B0 for current month
  - Blockers: #FFF5F5 normally, #FFE4E4 for current month

*Cell Items (text entries within cells):*
- Font size: 12px
- Color: #333
- Padding: 2px 0 2px 12px
- Position: relative (for ::before bullet positioning)
- Line-height: 1.35
- Multiple items stacked vertically
- Bullet marker (::before pseudo-element):
  - Content: '' (empty)
  - Position: absolute, left: 0, top: 7px
  - Width: 6px, height: 6px
  - Border-radius: 50% (circle)
  - Background color varies by row:
    - Shipped: #34A853 (green)
    - In Progress: #0078D4 (blue)
    - Carryover: #F4B400 (amber)
    - Blockers: #EA4335 (red)
- Empty cells (no data): display "-" with color: #AAA

### Typography

**Font Family:**
- Primary: Segoe UI (Microsoft sans-serif)
- Fallback: Arial, system sans-serif

**Font Sizes & Weights:**
- Heading (h1): 24px, font-weight: 700
- Subtitle: 12px, font-weight: 400
- Legend labels: 12px, font-weight: 400
- Heatmap title: 14px, font-weight: 700
- Column headers (months): 16px, font-weight: 700
- Row headers (status): 11px, font-weight: 700
- Cell items: 12px, font-weight: 400
- SVG text (month labels, dates): 11px / 10px, varying weights

**Line Height:**
- Default: 1.35 (cell items)
- Labels: 1.4 (milestone titles)

### Color Palette

**Neutral Colors:**
- White (#FFFFFF): Background
- Off-white (#FAFAFA): Timeline section background
- Light gray (#F5F5F5): Header cells
- Medium gray (#E0E0E0): Cell borders
- Dark gray (#CCC): Row/column borders
- Text gray (#888, #999, #444, #333, #111): Typography

**Status Colors:**
- **Green (Shipped)**: Text #1B7A28, Header bg #E8F5E9, Cell bg #F0FBF0, Highlight bg #D8F2DA, Bullet #34A853
- **Blue (In Progress)**: Text #1565C0, Header bg #E3F2FD, Cell bg #EEF4FE, Highlight bg #DAE8FB, Bullet #0078D4
- **Amber (Carryover)**: Text #B45309, Header bg #FFF8E1, Cell bg #FFFDE7, Highlight bg #FFF0B0, Bullet #F4B400
- **Red (Blockers)**: Text #991B1B, Header bg #FEF2F2, Cell bg #FFF5F5, Highlight bg #FFE4E4, Bullet #EA4335

**Milestone Colors (from data.json, examples):**
- M1: #0078D4 (blue)
- M2: #00897B (teal)
- M3: #546E7A (slate)
- PoC/Release marker fill: #F4B400 (amber) or #34A853 (green)
- Checkpoint marker outline: #999 (gray)
- Now marker: #EA4335 (red)

## UI Interaction Scenarios

### Scenario 1: Initial Page Load
**Description:** User opens the dashboard in a web browser.

**Expected Behavior:**
- Page loads and displays complete dashboard within 2 seconds
- Header appears immediately with project title, subtitle, and legend
- Timeline renders with all milestone bars and markers visible
- Heatmap grid displays with all status rows and month columns populated
- No error messages or loading spinners visible
- All colors, fonts, and spacing match design specification exactly
- Browser viewport is 1920×1080 (or equivalent); no horizontal/vertical scroll bars appear

---

### Scenario 2: Milestone Hover Tooltip
**Description:** User hovers mouse over a milestone marker (diamond or circle) in the timeline.

**Expected Behavior:**
- Tooltip appears near cursor showing:
  - Milestone title (e.g., "Chatbot & MS Role")
  - Milestone date (e.g., "Jan 12, 2026")
  - Milestone type/status (e.g., "PoC", "Checkpoint", "Production Release")
- Tooltip background: light gray or translucent dark background with white text
- Tooltip appears/disappears smoothly (fade-in/out animation optional)
- Milestone marker receives subtle hover state (e.g., slight shadow increase or scale)

---

### Scenario 3: Month Column Highlight
**Description:** User views the heatmap and identifies the current month column.

**Expected Behavior:**
- April column (current month, "Now") stands out visually from other months
- Column header background: #FFF0D0 (amber/yellow highlight)
- Column header text color: #C07700 (darker amber)
- All data cells in April column have slightly darker backgrounds matching their status color
  - Shipped April cells: #D8F2DA (darker green)
  - In Progress April cells: #DAE8FB (darker blue)
  - Carryover April cells: #FFF0B0 (darker amber)
  - Blockers April cells: #FFE4E4 (darker red)
- Other months (Mar, May, Jun) have standard lighter backgrounds
- This visual distinction makes the "current month" column immediately obvious

---

### Scenario 4: Status Row Identification
**Description:** User scans the heatmap to understand status categories.

**Expected Behavior:**
- Each status row (Shipped, In Progress, Carryover, Blockers) has distinct color coding:
  - **Shipped row**: Green header (#E8F5E9 bg, #1B7A28 text), light green cells, green bullet points
  - **In Progress row**: Blue header (#E3F2FD bg, #1565C0 text), light blue cells, blue bullet points
  - **Carryover row**: Amber header (#FFF8E1 bg, #B45309 text), light amber cells, amber bullet points
  - **Blockers row**: Red header (#FEF2F2 bg, #991B1B text), light red cells, red bullet points
- Status row header labels are left-aligned and clearly visible (e.g., "✓ SHIPPED", "● IN PROGRESS", "⚠ CARRYOVER", "✗ BLOCKERS")
- Bullet point markers (::before pseudo-elements) appear as 6px colored circles before each item text
- Color consistency across header, cell backgrounds, and bullet points enables fast visual scanning

---

### Scenario 5: Timeline Now Marker
**Description:** User views the timeline to understand current project timeline position.

**Expected Behavior:**
- A red dashed vertical line appears at the current date position (from data.json `nowMarker` field, e.g., "2026-04-12")
- Line spans full height of SVG timeline box
- Line color: #EA4335 (red)
- Line style: stroke-dasharray="5,3" (dashed pattern)
- Line width: 2px
- Label "NOW" appears above the line in 10px font, color #EA4335, font-weight: 700
- Milestones and dates to the left of the line are in the past; those to the right are future
- This provides clear visual context for timeline position

---

### Scenario 6: Data Cell with Multiple Items
**Description:** User views a heatmap cell containing multiple status items.

**Expected Behavior:**
- Cell displays 2-3 items stacked vertically
- Each item appears on a separate line with 1.35 line-height spacing
- Each item has a colored bullet point (6px circle, ::before element) positioned at left
- Item text: 12px font, color #333, left-padded 12px to align with bullet
- Cell padding: 8px 12px (provides spacing around all items)
- Text truncation: If item text exceeds cell width, it wraps or truncates with ellipsis (overflow: hidden)
- Example rendered cell:
  ```
  ● Feature A
  ● Integration Work
  ● Testing Phase
  ```

---

### Scenario 7: Empty Cell Display
**Description:** User views a heatmap cell with no status items for that month.

**Expected Behavior:**
- Cell displays a single "-" character (dash)
- Dash styling: 12px font, color: #AAA (light gray, de-emphasized)
- Cell background color still matches status row (e.g., light green for Shipped row)
- Cell maintains consistent padding and border styling
- Visual treatment signals "no activity this month" without looking broken or blank

---

### Scenario 8: Legend Reference
**Description:** User needs to understand what visual symbols mean.

**Expected Behavior:**
- Legend appears in header right section, aligned horizontally
- Four legend entries, each with icon + label:
  1. Amber diamond (12×12px, #F4B400) + "PoC Milestone" text
  2. Green diamond (12×12px, #34A853) + "Production Release" text
  3. Gray circle (8×8px, #999) + "Checkpoint" text
  4. Red line (2px×14px, #EA4335) + "Now (Apr 2026)" text (date from data.json)
- Items spaced 22px apart horizontally
- Icons and labels vertically centered
- All fonts 12px, color #111
- Icons are flex-shrink: 0 (do not compress)

---

### Scenario 9: ADO Backlog Link
**Description:** User clicks on the "? ADO Backlog" link in the header.

**Expected Behavior:**
- Link text appears next to project title in header (optional, if adoBacklogUrl is provided in data.json)
- Link color: #0078D4 (blue), no underline
- Hovering over link: text underline appears, cursor changes to pointer
- Clicking link: Opens ADO Backlog URL in new browser tab/window (target="_blank")
- Link does not trigger page refresh or navigation away from dashboard

---

### Scenario 10: Print/Screenshot Mode (Optional)
**Description:** User captures screenshot of dashboard for PowerPoint presentation.

**Expected Behavior (if @media print CSS is implemented):**
- Any refresh buttons, settings controls, or "Export" options are hidden via @media print rules
- Dashboard content (header, timeline, heatmap) remains fully visible and colored
- Layout and typography remain unchanged
- Background remains white (#FFFFFF)
- When printed or captured to image, resulting artifact is clean and presentation-ready
- Manual screenshot tools (Win+Shift+S, Chrome DevTools) capture full 1920×1080 viewport without scroll

---

### Scenario 11: Responsive Behavior on Different Resolutions (Not Required for MVP)
**Description:** User views dashboard on non-standard display resolution (e.g., 1440×900, 2560×1440).

**Expected Behavior:**
- **Recommended (not required):** Dashboard viewport adjusts proportionally but maintains aspect ratio and visual hierarchy
- **Or (simpler MVP):** Dashboard remains fixed at 1920×1080; smaller displays may require horizontal scroll
- **Stretch goal:** Responsive CSS media queries could provide optimized layouts for mobile/tablet, but this is out of scope for initial release

---

### Scenario 12: Data Load Error
**Description:** Application attempts to load data.json but file is missing or malformed.

**Expected Behavior:**
- Dashboard displays a user-friendly error message instead of stack trace
- Message: "Unable to load dashboard data. Please check that data.json is available and properly formatted."
- Error message appears in center of viewport with standard color (#EA4335 red or #888 gray)
- Application does not crash or display JavaScript console errors to user
- Manual refresh button (if implemented) allows user to retry after fixing data file

---

### Scenario 13: Long Item Text Truncation
**Description:** User views a cell with item text longer than cell width.

**Expected Behavior:**
- Cell has overflow: hidden property
- Text wraps to multiple lines if cell height allows, or truncates if not
- No text extends beyond cell border
- Cell maintains consistent padding and alignment
- Tooltips (optional): Hover over truncated text to see full item name

---

### Scenario 14: Manual Data Refresh
**Description:** User clicks "Refresh" button to reload data.json without reloading page.

**Expected Behavior:**
- Refresh button located in header or below heatmap (exact placement from UI design)
- Clicking button: Application reloads data.json from file system
- Dashboard re-renders with new data
- Existing viewport position maintained (no scroll jump)
- Button shows brief loading indicator (optional spinner or text change)
- Upon completion, data is refreshed and page returns to normal state

---

### Scenario 15: Milestone Timeline Spacing & Alignment
**Description:** User views timeline and verifies dates are positioned correctly.

**Expected Behavior:**
- Timeline SVG width: 1560px, accommodating 6 months (260px per month)
- Vertical month divider lines spaced 260px apart
- Month labels centered above dividers (text-anchor="middle")
- Milestone bars render as straight lines spanning from start to end date
- Milestone markers (diamonds, circles) positioned along bars at correct date
- Checkpoint circles: r=7 for current, r=5 for historical, r=4 for intermediate
- Diamond markers (PoC/Release): 10×10px, centered on data point, with drop shadow filter
- All dates scale proportionally (pixel-per-day calculation from data.json)

## Scope

### In Scope

- Single-page executive dashboard at fixed 1920×1080 resolution
- Read project metadata and status from local `data.json` JSON configuration file
- Render project header with title, subtitle, ADO Backlog link (optional), and legend
- Render 6-month timeline with milestone bars, markers (diamonds, circles, lines), and date labels
- Render monthly status heatmap with 4 status categories (Shipped, In Progress, Carryover, Blockers) × 4 month columns
- Color-coded status rows matching design specification (green, blue, amber, red)
- Dynamic data binding: all timeline and heatmap content sourced from data.json
- Support for 3+ milestone tracks in timeline
- Support for 3+ status items per heatmap cell
- "Now" marker on timeline at current date (from data.json `nowMarker`)
- Highlight current month column in heatmap with distinct coloring
- Basic error handling for missing or malformed data.json (user-friendly error message)
- Manual refresh button to reload data.json
- Blazor Server application running on localhost:5001 (or configurable port)
- Local deployment (Windows Service, Console App, or Docker container)
- Responsive print stylesheet (optional, for clean PowerPoint screenshots)
- HTTPS/HTTP support for local network access

### Out of Scope

- Cloud infrastructure (Azure, AWS, GCP) – local deployment only
- Authentication or authorization (no login, no role-based access)
- Enterprise security hardening (encryption, audit logging, etc.)
- Database (SQL Server, PostgreSQL, etc.) – JSON file only
- Real-time data synchronization or WebSocket push updates
- Mobile-responsive design (fixed 1920×1080 desktop layout)
- Horizontal scaling (single-instance deployment)
- Advanced charting libraries (Chart.js, OxyPlot SVG generation is optional; simple SVG acceptable)
- PDF/Excel export functionality
- Drill-down or interactivity beyond hover tooltips
- Dark mode theme toggle (light mode only)
- Internationalization (English only)
- API endpoints or integration with external systems
- Azure DevOps API integration (manual data.json updates only)
- Automated screenshot or PowerPoint generation
- Historical data tracking or versioning of data.json
- Multi-project dashboard (single project per instance)
- Role-based filtering or personalization
- Administrative UI for editing data (JSON file editing only)
- Search or filter functionality across status items
- Sorting or reordering of status rows/columns by user

## Non-Functional Requirements

### Performance

- **Page Load Time:** Dashboard renders completely within 2 seconds from cold start
- **Data Load:** data.json read and deserialized in <500ms
- **Re-render:** Manual refresh completes in <1 second
- **Memory Footprint:** Application uses <500MB RAM on idle, <1GB under typical usage
- **Browser Compatibility:** Works on Chrome 90+, Edge 90+, Firefox 88+ (modern browsers)
- **Client-Side JavaScript:** Minimal JavaScript; server-side rendering via Blazor Server

### Reliability & Availability

- **Uptime:** Application runs continuously on local deployment without crash
- **Error Handling:** Graceful handling of missing/malformed data.json; no unhandled exceptions shown to user
- **Data Integrity:** No data loss or corruption from file I/O operations
- **Concurrency:** Single-user deployment; no concurrent data access contention

### Security (Minimal Requirements)

- **No Authentication:** Dashboard accessible without login (localhost-only initially)
- **No Sensitive Data:** data.json contains only project status and timeline info, no credentials or secrets
- **Output Encoding:** Blazor automatically HTML-encodes all text; no XSS vulnerability
- **File Permissions:** Application runs with read access to data.json; no write access needed initially
- **HTTPS:** Optional for localhost development; use self-signed certificate if needed
- **Network Isolation:** Accessible only on local machine or corporate LAN (no internet exposure)

### Scalability

- **Concurrent Users:** Designed for 1-5 simultaneous users (internal dashboard)
- **Data Size:** Supports up to 12+ months of timeline and 10+ status items per cell
- **Scaling Path:** If usage exceeds 10 concurrent users, consider migrating to Blazor WebAssembly + API backend

### Usability & Accessibility

- **No Training Required:** Dashboard is self-documenting via legend and visual hierarchy
- **Screenshot-Friendly:** Layout optimized for 1920×1080 capture without manual cropping
- **Print-Optimized:** @media print CSS hides non-essential UI elements for clean hardcopy
- **Color Contrast:** Text colors meet WCAG AA contrast ratios (verified via design colors)
- **Keyboard Navigation:** Basic browser keyboard support (Tab, Enter) sufficient; advanced a11y not required

### Maintainability

- **Code Structure:** C# .NET 8 with clear separation of concerns (Models, Services, Components)
- **CSS Organization:** Global styles + scoped component styles; no CSS conflicts
- **Data Schema:** Documented JSON structure with example data.json provided
- **Build Process:** Simple `dotnet build` and `dotnet run` commands
- **Documentation:** README with setup instructions, data.json schema, and configuration guide

## Success Metrics

1. **Visual Fidelity:** Dashboard rendered output pixel-matches design mock (1920×1080) verified by side-by-side comparison
2. **Data Binding:** All timeline and heatmap content dynamically renders from data.json without hardcoding
3. **Load Performance:** Cold start to full render completes in <2 seconds on target hardware (measured via Chrome DevTools)
4. **Error Handling:** Missing/malformed data.json displays user-friendly error message; no stack traces leaked to UI
5. **Refresh Functionality:** Manual refresh button reloads data.json and updates dashboard within 1 second
6. **Screenshot Quality:** PowerPoint screenshot of dashboard requires zero additional cropping or scaling; fits standard 16:9 slide
7. **Local Deployment:** Application runs successfully as Windows Service or Docker container accessible on localhost:5001
8. **Code Coverage:** Unit tests cover data deserialization, color mapping, and timeline calculation logic (>80% coverage)
9. **Team Adoption:** Executives and project managers successfully use dashboard within 5 minutes of opening (no training)
10. **Browser Compatibility:** Dashboard renders correctly on Chrome, Edge, and Firefox (verified cross-browser testing)

## Constraints & Assumptions

### Constraints

- **Fixed Resolution:** Dashboard optimized for 1920×1080; scaling to other resolutions is out of scope
- **Local Deployment Only:** No cloud infrastructure or globally distributed hosting; internal network only
- **Single Data Source:** All data must come from data.json file; no live API integration
- **No Database:** File-based data storage; no SQL Server or PostgreSQL dependency
- **No Authentication:** Dashboard is open; no login or access control
- **No Real-Time Updates:** Data is static per page load; no WebSocket or server-push updates
- **English Language Only:** No internationalization or multi-language support
- **Single Project:** Each dashboard instance covers one project; no multi-project view

### Assumptions

- **Data.json Location:** File is located at `./wwwroot/data/data.json` (relative to application root) or path specified in appsettings.json
- **Date Format:** All dates in data.json are ISO 8601 format (YYYY-MM-DD)
- **Milestone Dates:** At least 3 milestones defined in data.json; at most 12 (6-month timeline)
- **Status Items:** Each status row contains 0-3 items per month; items are single-line text (<100 characters)
- **Color Values:** Milestone colors in data.json are valid hex codes (#RRGGBB format)
- **Timezone:** "Now" marker uses UTC or local machine timezone (to be confirmed with stakeholder)
- **Browser Environment:** Users access dashboard via modern web browser (Chrome, Edge, Firefox); no IE11 support
- **Network Access:** If deployed on corporate LAN, network is stable and responsive (<100ms latency)
- **File System:** Application process has read access to data.json; file is not locked by another process
- **Hardware:** Target deployment machine has at least 2GB RAM and modern CPU (2020+)
- **Stakeholder Availability:** Executives/PMs available for 2-3 feedback cycles during development
- **Design Stability:** Visual design from OriginalDesignConcept.html is final; no major redesigns post-kickoff

### Dependencies

- **.NET 8 SDK:** Required for building and running Blazor Server application
- **Visual Studio 2022** or **.NET CLI:** Required for development and deployment
- **Modern Web Browser:** Chrome, Edge, or Firefox for viewing dashboard
- **File System Access:** Application machine must have read access to data.json file
- **Network Connectivity:** If accessing from LAN, network must be available; localhost-only deployment has no external dependency

### Risk Mitigations

| Risk | Mitigation |
|------|-----------|
| data.json file missing or corrupted | Implement graceful error handling; display user-friendly message; cache previous data if available |
| Milestone dates not in chronological order | Validate and sort dates on load; log warnings for data quality issues |
| Very long status item text overflows cell | Set overflow: hidden and line-height limits; add optional tooltip on hover for full text |
| Timeline with many milestones becomes crowded | Limit to 12 months max; implement optional zoom/scroll if needed in future |
| Blazor WebSocket connection drops | Auto-reconnect logic; display "Reconnecting..." banner; manual refresh button |
| SVG rendering performance degrades | Simplify SVG (no excessive filters); pre-render static elements; profile with DevTools |
| Color contrast accessibility issues | Use design palette verified for WCAG AA; test with accessibility tools |
| Deployment to Windows Service fails | Provide step-by-step PowerShell scripts; document firewall/permissions requirements |

---

**Document Version:** 1.0  
**Date:** 2026-04-12  
**Author:** Program Manager  
**Status:** Ready for Development  
**Technology Stack:** C# .NET 8 Blazor Server, JSON-based Configuration, Local Deployment