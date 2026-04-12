# PM Specification: Executive Project Reporting Dashboard

## Executive Summary

The Executive Project Reporting Dashboard is a simple, screenshot-optimized web application that provides executives with real-time visibility into a project's milestones, progress, and monthly execution status. Built as a self-contained Blazor Server application running on .NET 8, it reads project data from a JSON configuration file and renders an interactive, print-friendly dashboard matching the OriginalDesignConcept.html design. The solution requires no authentication, no cloud infrastructure, and no database—just a single executable file and a JSON file, making it trivial to distribute and use for PowerPoint presentations and executive briefings.

## Business Goals

1. **Enable executive visibility** into project health through a simple, self-service dashboard that executives can access without IT support or specialized training.
2. **Support rapid visual reporting** by providing a screenshot-ready interface optimized for 1920x1080 resolution, enabling executives to paste dashboard views directly into PowerPoint presentations.
3. **Eliminate manual status report creation** by allowing project managers to maintain a single JSON file that automatically populates the dashboard with current milestone dates, delivery status, and progress metrics.
4. **Minimize operational overhead** by removing infrastructure dependencies (no database, no authentication, no cloud services), allowing the dashboard to run as a standalone Windows executable on any machine.
5. **Standardize project communication** across the organization by providing a consistent, branded visual template that communicates project status through color-coded status rows and timeline markers.

## User Stories & Acceptance Criteria

### Story 1: Executive Views Project Dashboard on Startup
**As an** executive sponsor  
**I want** to open a single executable file and immediately see a complete project status dashboard  
**so that** I can quickly assess project health without waiting for reports or needing IT support.

**Acceptance Criteria:**
- [ ] Application launches from a single `.exe` file with no installation or prerequisite software required
- [ ] Dashboard loads in the default browser (localhost:5000 or similar) within 3 seconds
- [ ] Visual layout matches OriginalDesignConcept.html at 1920x1080 resolution
- [ ] Data is read from `data.json` file in the same directory as the executable
- [ ] Dashboard displays without errors even if browser cache is cleared

**Visual Reference:** Entire dashboard page layout from OriginalDesignConcept.html (header, timeline section, heatmap grid)

---

### Story 2: Executive Sees Project Header with Title and Legend
**As an** executive  
**I want** to see a clear project title, team context, and a legend explaining the status indicators  
**so that** I immediately understand what project I'm viewing and what the visual symbols mean.

**Acceptance Criteria:**
- [ ] Header displays project title in bold 24px font (e.g., "Privacy Automation Release Roadmap")
- [ ] Subtitle shows team/workstream context in smaller gray font (e.g., "Trusted Platform – Privacy Automation Workstream – April 2026")
- [ ] Legend in top-right corner shows four symbols with labels:
  - Yellow diamond = "PoC Milestone"
  - Green diamond = "Production Release"
  - Gray circle = "Checkpoint"
  - Red vertical line = "Now (current date)"
- [ ] ADO Backlog link is clickable and styled as blue hyperlink (#0078D4)
- [ ] Header remains visible when scrolling (sticky) if content exceeds viewport height

**Visual Reference:** Header section of OriginalDesignConcept.html (`.hdr` class with h1, subtitle, legend items)

---

### Story 3: Executive Views Milestone Timeline with Dates
**As a** project manager updating the dashboard  
**I want** to define milestones with color-coded timeline lines, checkpoints, and marker dates  
**so that** executives can see the project's critical path and delivery timeline at a glance.

**Acceptance Criteria:**
- [ ] Timeline section displays horizontal colored lines for each milestone (M1, M2, M3, etc.)
- [ ] Each milestone line is labeled on the left with ID and title (e.g., "M1 – Chatbot & MS Role")
- [ ] Milestone start date is marked with a white-filled circle on the timeline
- [ ] Checkpoints are marked as smaller circles along the milestone line
- [ ] PoC (Proof of Concept) milestone date is marked as a yellow diamond (color: #F4B400)
- [ ] Production release date is marked as a green diamond (color: #34A853)
- [ ] Months are shown as vertical gridlines labeled "Jan", "Feb", "Mar", etc.
- [ ] "Now" line (red dashed, color: #EA4335) shows current date/month position on timeline
- [ ] SVG renders correctly at all zoom levels and in browser print preview
- [ ] Tooltip or label shows exact date when hovering over milestone markers (TBD: implement in Phase 2)

**Visual Reference:** Timeline/Gantt section of OriginalDesignConcept.html (`.tl-area` with SVG containing month gridlines, milestone lines, circles, and diamond markers)

---

### Story 4: Project Manager Views Monthly Execution Heatmap
**As a** project manager  
**I want** to see a grid showing what was Shipped, In Progress, Carried Over, or Blocked each month  
**so that** I can quickly identify execution trends and communicate progress to executives.

**Acceptance Criteria:**
- [ ] Heatmap displays as a grid with 5 columns (Status label + 4 months) and 5 rows (header + 4 status categories)
- [ ] Column headers show month names: "March", "April (Now)", "May", "June" with appropriate highlighting
- [ ] Row headers show status categories: "✓ Shipped", "? In Progress", "⟲ Carryover", "✕ Blockers"
- [ ] Each cell contains a list of 1-4 items (work items shipped, in progress, etc.) for that month/status combination
- [ ] Status rows use color-coded backgrounds:
  - Shipped row: Green (#E8F5E9 background, #34A853 dot indicator)
  - In Progress row: Blue (#E3F2FD background, #0078D4 dot indicator)
  - Carryover row: Amber (#FFF8E1 background, #F4B400 dot indicator)
  - Blockers row: Red (#FEF2F2 background, #EA4335 dot indicator)
- [ ] April (current month) column has highlighted background (#FFF0D0) to indicate "Now"
- [ ] Empty cells display "-" in gray text
- [ ] Each item displays as a bullet point with 6px colored dot before the text
- [ ] Grid borders are subtle (#E0E0E0) for clean appearance
- [ ] Heatmap scrolls horizontally if more than 4 months of data exist

**Visual Reference:** Heatmap section of OriginalDesignConcept.html (`.hm-wrap`, `.hm-grid`, row/cell color classes: `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`)

---

### Story 5: Project Manager Edits data.json and Dashboard Updates
**As a** project manager  
**I want** to edit the `data.json` file with a text editor and have the dashboard reflect changes  
**so that** I can maintain the dashboard without deploying new code or restarting the application.

**Acceptance Criteria:**
- [ ] `data.json` file is located in the same directory as the executable (or in `wwwroot/data/` for web hosting)
- [ ] JSON schema is documented and includes all required fields: `reportTitle`, `subtitle`, `adoBacklogUrl`, `milestones`, `statusRows`
- [ ] Editing `data.json` with Notepad or VS Code (valid JSON syntax) populates the dashboard correctly on next page load
- [ ] JSON validation errors are caught and displayed as friendly error message on the dashboard
- [ ] Milestone dates are parsed as ISO 8601 format (e.g., "2026-03-26")
- [ ] Empty items arrays in status rows are handled gracefully (cells remain empty, no crashes)
- [ ] Special characters in item names (quotes, ampersands) are HTML-escaped to prevent XSS

**Visual Reference:** Data input affects all sections (Header, Timeline, Heatmap)

---

### Story 6: Executive Takes Screenshot for PowerPoint Presentation
**As an** executive  
**I want** to capture a screenshot of the dashboard that prints/renders correctly without scrolling  
**so that** I can paste the image into my PowerPoint deck for board meetings.

**Acceptance Criteria:**
- [ ] Dashboard layout fits exactly within a 1920x1080 viewport (common laptop resolution) without scrolling
- [ ] Print preview (Ctrl+P in browser) shows the entire dashboard on a single page or clear page breaks
- [ ] Screenshot taken with browser developer tools (or screenshot tool) shows exact match to OriginalDesignConcept.html visual
- [ ] Colors remain vibrant and legible when saved as PNG/JPG and embedded in PowerPoint
- [ ] Margins and padding ensure no content is cut off at page edges
- [ ] Print stylesheet ensures no UI chrome (scrollbars, browser tabs) appears in output
- [ ] Responsive breakpoints are not triggered at 1920x1080 (fixed-width layout is acceptable)

**Visual Reference:** Entire OriginalDesignConcept.html design; pixel-perfect visual match required

---

### Story 7: Users Experience Clean, Professional UI Without Distractions
**As an** executive or project manager  
**I want** a simple, uncluttered dashboard with no unnecessary buttons, menus, or chrome  
**so that** the focus remains on the project status and milestones.

**Acceptance Criteria:**
- [ ] No navigation menu, hamburger menu, or collapsible sidebars present
- [ ] No pop-up alerts, notifications, or modal dialogs unless displaying critical errors
- [ ] No form inputs, editable fields, or interactive controls in the UI (data editing is done via JSON file, not UI)
- [ ] No footer, copyright, or attribution text visible (keep space for content)
- [ ] Logo/branding is optional; not required for MVP
- [ ] Page background is white (#FFFFFF) to match design
- [ ] Font stack uses Segoe UI, Arial, sans-serif for system consistency
- [ ] No animations or transitions (static render preferred for simplicity and screenshot consistency)

**Visual Reference:** OriginalDesignConcept.html demonstrates minimal, clean UI design

---

## Visual Design Specification

### Overall Layout Structure

The dashboard is structured as a single-column flexbox layout (1920px width × 1080px height) with four major sections stacked vertically:

1. **Header Section** (height: ~60px, flex-shrink: 0)
2. **Timeline Section** (height: 196px, flex-shrink: 0)
3. **Heatmap Section** (flex: 1, remaining height)
4. **No Footer**

### Color Palette

| Role | Color | Usage |
|------|-------|-------|
| Primary Brand | #0078D4 | Milestone M1 line, header link, progress dots |
| Shipped Status | #1B7A28 (text), #34A853 (dot), #E8F5E9 (bg), #F0FBF0 (cell) | Shipped row header and cells |
| In Progress Status | #1565C0 (text), #0078D4 (dot), #E3F2FD (bg), #EEF4FE (cell) | In Progress row header and cells |
| Carryover Status | #B45309 (text), #F4B400 (dot), #FFF8E1 (bg), #FFFDE7 (cell) | Carryover row header and cells |
| Blocker Status | #991B1B (text), #EA4335 (dot), #FEF2F2 (bg), #FFF5F5 (cell) | Blockers row header and cells |
| Current Month Highlight | #FFF0D0 | April column background (April = "Now") |
| Primary Text | #111 | Body text, default font color |
| Secondary Text | #444 | Milestone description, lighter emphasis |
| Tertiary Text | #888, #999 | Headers, labels, secondary information |
| Light Gray | #FAFAFA, #F5F5F5 | Background fills, header cells |
| Borders | #E0E0E0, #CCC | Grid lines, cell borders |
| White | #FFFFFF | Body background, circle fills on timeline |

### Typography

| Element | Font | Size | Weight | Color | Usage |
|---------|------|------|--------|-------|-------|
| Page Title (H1) | Segoe UI, Arial, sans-serif | 24px | Bold (700) | #111 | "Privacy Automation Release Roadmap" |
| Subtitle | Segoe UI, Arial, sans-serif | 12px | Regular (400) | #888 | Team/workstream context line |
| Heatmap Title | Segoe UI, Arial, sans-serif | 14px | Bold (700) | #888 | "Monthly Execution Heatmap..." label |
| Milestone Label (Timeline) | Segoe UI, Arial, sans-serif | 12px | Bold (600) on ID, Regular (400) on description | #0078D4 (ID), #444 (desc) | "M1 Chatbot & MS Role" |
| Column Header (Heatmap) | Segoe UI, Arial, sans-serif | 16px | Bold (700) | #111 | "March", "April", "May", "June" |
| Row Header (Heatmap) | Segoe UI, Arial, sans-serif | 11px | Bold (700) | Status-dependent | "✓ Shipped", "? In Progress", etc. |
| Cell Content (Heatmap) | Segoe UI, Arial, sans-serif | 12px | Regular (400) | #333 | Work item descriptions |
| Timeline Month Label (SVG) | Segoe UI, Arial, sans-serif | 11px | Bold (600) | #666 | "Jan", "Feb", "Mar", "Apr", etc. |
| Marker Label (SVG) | Segoe UI, Arial, sans-serif | 10px | Regular (400) | #666 | Date labels like "Jan 12", "Mar 26 PoC" |
| Legend Label | Segoe UI, Arial, sans-serif | 12px | Regular (400) | #111 | "PoC Milestone", "Production Release", etc. |

### Header Section Design

**Container:** `.hdr` flexbox with `padding: 12px 44px 10px`, `border-bottom: 1px solid #E0E0E0`

**Left Side (flex: 1):**
- `<h1>` with project title (24px, bold)
- Subtitle `<div>` below title (12px, #888 color)
- Optional: `<a>` link to ADO Backlog styled as blue hyperlink

**Right Side (flex: none):**
- Legend container with flex layout, `gap: 22px`
- Four legend items, each as `<span>` with inline-flex, `gap: 6px`:
  1. PoC Milestone: 12px×12px square rotated 45° (#F4B400) + "PoC Milestone" label
  2. Production Release: 12px×12px square rotated 45° (#34A853) + "Production Release" label
  3. Checkpoint: 8px×8px circle (#999) + "Checkpoint" label
  4. Now (Apr 2026): 2px×14px vertical line (#EA4335) + "Now (Apr 2026)" label

### Timeline Section Design

**Container:** `.tl-area` flex layout, `height: 196px`, `padding: 6px 44px 0`, `border-bottom: 2px solid #E8E8E8`, `background: #FAFAFA`

**Left Panel (width: 230px, flex-shrink: 0):**
- Contains milestone legend (M1, M2, M3 labels)
- Each milestone entry is 12px font with ID in color (e.g., #0078D4 for M1), description in #444 below
- Vertical alignment: space-around

**SVG Pane (.tl-svg-box):**
- SVG dimensions: width=1560px, height=185px
- Contains:
  - **Month Gridlines:** Vertical lines at x=0, 260, 520, 780, 1040, 1300 (260px apart = one month). Color: #bbb, opacity 0.4, stroke-width 1px. Text labels above ("Jan", "Feb", etc.) in 11px, #666.
  - **Now Line:** Vertical dashed red line (#EA4335, stroke-width 2, stroke-dasharray "5,3") at x≈823. Text "NOW" in 10px bold above.
  - **Milestone Lines:** Horizontal lines for each milestone (colored per design), stroke-width 3px
    - M1: #0078D4 at y=42
    - M2: #00897B at y=98
    - M3: #546E7A at y=154
  - **Start Circle:** White-filled circle with colored stroke at start of each milestone line (r=7, stroke-width 2.5)
  - **Checkpoints:** Smaller circles along milestone lines (r=4-5, white fill with stroke, or solid fill depending on state)
  - **PoC Marker:** Yellow diamond (#F4B400) at 12×12px size (rotated square), positioned at x/y for PoC date. Drop shadow filter applied.
  - **Production Marker:** Green diamond (#34A853), same style as PoC marker.
  - **Date Labels:** Text elements below/above markers (10px font, #666, text-anchor="middle")

### Heatmap Section Design

**Container:** `.hm-wrap` flexbox, flex-direction: column, `padding: 10px 44px 10px`, flex: 1 (remaining height)

**Title:** `.hm-title` (14px, bold, #888, uppercase, letter-spacing 0.5px, margin-bottom 8px)

**Grid Container:** `.hm-grid` CSS Grid with:
- **Grid Template:**
  - Columns: `160px repeat(4, 1fr)` (row header, then 4 month columns of equal width)
  - Rows: `36px repeat(4, 1fr)` (header row, then 4 data rows for Shipped, In Progress, Carryover, Blockers)
  - Border: 1px solid #E0E0E0
  - Gap: 0 (borders create spacing)

**Header Row Cells:**
- **Corner Cell (row 1, col 1):** `.hm-corner` — background #F5F5F5, flex layout, center text, font 11px bold #999 uppercase, border-right 1px #E0E0E0, border-bottom 2px #CCC. Text: "Status"
- **Column Header Cells (row 1, col 2-5):** `.hm-col-hdr` — background #F5F5F5, font 16px bold, centered, border-right 1px #E0E0E0, border-bottom 2px #CCC. Text: "March", "April", "May", "June". April column uses `.apr-hdr` variant with background #FFF0D0 and color #C07700.

**Row Header Cells (col 1, row 2-5):**
Each is `.hm-row-hdr` with status-specific class (`.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr`):
- Font: 11px bold uppercase, letter-spacing 0.7px
- Padding: 0 12px
- Border-right: 2px solid #CCC
- Border-bottom: 1px solid #E0E0E0
- Background and text color per status (see Color Palette)

**Data Cells (row 2-5, col 2-5):**
Each cell is `.hm-cell` with status-specific class (`.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`):
- Padding: 8px 12px
- Border-right: 1px solid #E0E0E0
- Border-bottom: 1px solid #E0E0E0
- Overflow: hidden
- Background color per status (see Color Palette)
- April cells add `.apr` modifier for lighter highlight background

**Cell Content:**
- Each item is a `<div class="it">` element (font 12px, color #333, padding 2px 0 2px 12px, line-height 1.35)
- Pseudo-element `::before` adds 6px × 6px colored circle (border-radius 50%) positioned at left (position absolute, left 0, top 7px)
- Circle color matches status indicator color (green for Shipped, blue for In Progress, etc.)
- Empty cells display a single item with "-" and color #AAA

### Responsive & Print Considerations

- **Fixed-width layout:** Designed for 1920×1080. No responsive breakpoints for MVP. Document that mobile/tablet support is out of scope.
- **Print styles:** Add `@media print` CSS rules:
  - Body `margin: 0`, `padding: 0`, `overflow: visible`
  - Hide browser chrome (use `print-color-adjust: exact` to preserve colors)
  - Ensure heatmap and timeline fit on single page without page breaks (or use explicit page breaks if needed)
  - Use landscape orientation recommendation

---

## UI Interaction Scenarios

**Scenario 1: User Opens Dashboard on Startup**
- User double-clicks the `ReportingDashboard.exe` file or runs it from command line
- Application launches and opens the default browser to `http://localhost:5000` or similar port
- Dashboard loads completely within 3 seconds
- User sees the full page layout: header, timeline, and heatmap all visible without scrolling
- All content is rendered with the exact visual styling from OriginalDesignConcept.html

**Scenario 2: User Reviews Project Header and Context**
- User immediately sees the project title ("Privacy Automation Release Roadmap") in prominent 24px font
- Subtitle below provides team/workstream context ("Trusted Platform – Privacy Automation Workstream – April 2026")
- Legend in top-right corner shows four symbols with labels
- User clicks "ADO Backlog" link to navigate to the backlog (URL from data.json)
- All legend symbols are correctly colored and labeled (yellow diamond for PoC, green for Prod, gray circle for checkpoint, red line for now)

**Scenario 3: User Examines Milestone Timeline**
- User looks at the timeline section and sees three horizontal colored lines (one for each milestone)
- M1 line is blue (#0078D4) with label "M1 – Chatbot & MS Role" on the left
- M2 line is teal (#00897B) with label "M2 – PDS & Data Inventory"
- M3 line is gray (#546E7A) with label "M3 – Auto Review DFD"
- Month gridlines are visible (Jan, Feb, Mar, Apr, May, Jun) with vertical separator lines
- A red dashed "NOW" line is positioned at the current month (April)
- User sees start circles on each milestone (M1 starts at Jan 12 with date label)
- User sees checkpoints as smaller circles along M2 (at Feb 11, and additional unnamed checkpoints)
- User sees yellow diamond markers (PoC milestones) on each timeline (M1 at Mar 26, M2 at Mar 20, M3 at Mar 30)
- User sees green diamond markers (Production releases) on M1 (Apr TBD), M2 (Mar 30)
- All markers are drop-shadowed for visual depth

**Scenario 4: User Hovers Over Timeline Marker (Future Enhancement)**
- User moves mouse over a diamond marker (PoC or Prod) on the timeline
- A tooltip appears showing the full date and marker type (e.g., "Mar 26 PoC" or "Mar 30 Production Release")
- Tooltip disappears when user moves mouse away
- *Note: This is a Phase 2 enhancement; MVP has static labels*

**Scenario 5: User Reviews Monthly Execution Heatmap**
- User scrolls down or views the heatmap section showing four status rows (Shipped, In Progress, Carryover, Blockers)
- Four month columns are labeled: March, April (highlighted in orange/amber to indicate "Now"), May, June
- Shipped row (green background) shows items shipped in each month
  - March: "TODO - item shipped", "TODO - item shipped"
  - April: "TODO - item shipped", "TODO - item shipped"
  - May/June: "-" (empty)
  - Each item has a green circle before it (#34A853)
- In Progress row (blue background) shows items actively being worked
  - Similar layout with blue circles (#0078D4)
- Carryover row (amber background) shows items carried over from previous months
  - Yellow circles (#F4B400)
- Blockers row (red background) shows blockers impeding progress
  - Red circles (#EA4335)
- April column has a slightly lighter background (#FFF0D0) to indicate "Now"
- All cells have subtle gray borders (#E0E0E0) for clean grid appearance
- Empty cells display "-" in light gray text

**Scenario 6: Project Manager Edits data.json**
- Project manager opens `data.json` in a text editor (Notepad, VS Code, etc.)
- Updates milestone dates, titles, or status items
- Saves the file
- Without restarting the application, the user refreshes the browser (F5)
- Dashboard immediately reflects the new data
  - Timeline markers move to new dates
  - Heatmap items update with new work items or removed items
  - No errors or reload glitches occur

**Scenario 7: Executive Takes Screenshot for Presentation**
- Executive opens the dashboard and verifies all content is visible at 1920×1080
- Executive uses browser's screenshot tool (Ctrl+Shift+S in Chrome/Edge) or Snagit to capture the full dashboard
- Screenshot is saved as PNG/JPG image file
- Executive opens PowerPoint and pastes the image into a slide
- Image appears with vibrant colors and sharp text (not blurry or distorted)
- No browser chrome (URL bar, tabs) or system elements appear in the screenshot
- Image fits within the slide without requiring resizing (or minimal resize needed)

**Scenario 8: Dashboard Print Preview**
- Executive opens the browser print dialog (Ctrl+P)
- Print preview shows the entire dashboard on a single page (or with clear page breaks if needed)
- Colors are preserved in the preview (no grayscale conversion)
- Margins are minimal to maximize content area
- No unnecessary UI elements or page headers/footers appear
- Executive prints to PDF or physical paper; output matches the on-screen appearance

**Scenario 9: User Accesses Dashboard from Different Machine**
- User copies `ReportingDashboard.exe` and `data.json` to a different Windows 10+ machine
- User double-clicks the `.exe` file (no installation or admin rights required)
- Dashboard launches and displays correctly with the same layout and colors
- Data from `data.json` loads correctly regardless of machine or user

**Scenario 10: Empty or Invalid Data**
- `data.json` is missing or malformed (invalid JSON syntax)
- Application displays a friendly error message on the dashboard (e.g., "Error loading data file. Please check data.json is valid.")
- Application does not crash or show a blank page
- If `statusRows` is empty, heatmap shows no rows but maintains grid structure
- If `milestones` is empty, timeline shows month gridlines but no milestone lines or markers

**Scenario 11: Large Data Set Performance**
- `data.json` contains 10 milestones, 12 months of history, and 50+ work items across all status rows
- Dashboard still loads and renders within 3-5 seconds
- No lag or freezing when scrolling or interacting with the page
- SVG timeline with many markers renders smoothly without frame rate drops

**Scenario 12: Special Characters in Data**
- Work item titles contain special characters: quotes ("), ampersands (&), angle brackets (<>), or emojis
- Dashboard renders the characters safely without XSS vulnerabilities or display corruption
- HTML entities are properly escaped in output

---

## Scope

### In Scope

- **Single-page dashboard application** displaying project status, milestones, and monthly execution metrics
- **Blazor Server** application (C# .NET 8) packaged as self-contained Windows `.exe` file
- **JSON-based configuration** (`data.json`) for dashboard data; no database required
- **Visual design matching** OriginalDesignConcept.html exactly at 1920×1080 resolution
- **Print/screenshot optimization** — layout fits single viewport without scrolling, colors preserved in print preview
- **Four status categories:** Shipped, In Progress, Carryover, Blockers (color-coded grid)
- **Milestone timeline visualization** using SVG with lines, circles, checkpoints, PoC/Prod markers
- **Legend** explaining visual symbols (PoC, Prod, Checkpoint, Now line)
- **Four-month heatmap** (configurable, default: March–June) showing work items per status per month
- **No authentication** — read-only, internal-use dashboard; network security via intranet isolation
- **No database** — all data stored in JSON file, loaded into memory on startup
- **Basic error handling** — friendly error messages if data file is missing or malformed
- **Standalone execution** — single `.exe` file, no installation, no prerequisites (other than Windows OS)
- **Unit tests** for JSON deserialization, date calculations, and data validation
- **User documentation** — README or guide on how to run the executable and edit `data.json`

### Out of Scope

- **User authentication or authorization** — no login, no role-based access control
- **Data editing via UI** — only file-based edits to `data.json` are supported
- **Real-time data sync** from external sources (ADO, Jira, etc.) — data is static until manually updated
- **Multi-user collaboration** — no concurrent editing or version control within the app
- **Mobile responsiveness** — fixed 1920×1080 design; mobile/tablet support is not a requirement
- **Dark mode or theme customization** (MVP) — single light theme only
- **Data persistence in database** — JSON file is the sole data store
- **Cloud deployment** — local/intranet only; no AWS, Azure, GCP cloud hosting
- **Containerization** (Docker) — Windows `.exe` is simpler for this use case
- **Advanced charting or analytics** — only heatmap and timeline grid visualization
- **PDF export functionality** (MVP) — screenshots to PowerPoint are the primary use case
- **Drag-and-drop reordering** or interactive editing of milestones within the dashboard
- **Drill-down or detail view** for individual work items (click milestone to see tasks) — MVP shows summary only
- **Automated data import** from ADO, Jira, or other tools — manual JSON edits only
- **Scheduled report generation** or batch processing (can be added later)
- **WCAG accessibility compliance** (MVP) — focus is on visual design match, not formal a11y audit
- **Internet Explorer 11 or legacy browser support** — modern browsers (Chrome, Edge, Firefox) only
- **Internationalization (i18n) or multi-language support**
- **Search or filtering functionality** within the dashboard
- **Milestone dependency tracking** or critical path analysis
- **Risk register or issue tracking** beyond the "Blockers" status row
- **Team assignment or resource allocation** visualization
- **Budget, cost, or financial tracking**
- **Automated testing beyond unit tests** (no UI/E2E testing framework in MVP)

---

## Non-Functional Requirements

### Performance
- **Page Load Time:** Dashboard must load and render completely within 3 seconds on a modern Windows machine (2+ GHz CPU, SSD, 8+ GB RAM)
- **JSON Deserialization:** `data.json` file up to 500 KB should deserialize in <100 ms
- **SVG Rendering:** Timeline SVG with up to 20 milestones should render without visible delay
- **Memory Usage:** Application should consume <200 MB RAM at rest (idle state)
- **Startup Time:** `.exe` launch to first page display should take <3 seconds (including .NET runtime startup)

### Scalability & Limits
- **Milestone Count:** Support up to 20 milestones without performance degradation
- **Month Timeline:** Support up to 24 months (2 years) of timeline history
- **Work Items:** Support up to 100 work items across all status rows without noticeable slowdown
- **Concurrent Sessions:** Single-user or small team (< 5 simultaneous browser tabs) expected; no high-concurrency optimization required
- **File Size:** `data.json` should not exceed 1 MB

### Reliability & Robustness
- **Uptime:** Application should remain available as long as the process is running (no scheduled maintenance windows)
- **Error Handling:** All exceptions should be caught and logged; dashboard should display a friendly error message instead of crashing
- **Data Validation:** Invalid JSON should be caught with specific error message (not generic "file not found")
- **Date Handling:** Support ISO 8601 date format (YYYY-MM-DD); gracefully handle invalid date strings with error message
- **Null/Empty Data:** Dashboard should handle missing or empty fields without crashes (display "-" or default values)

### Security
- **Data Protection:** No sensitive data (passwords, API keys, PII) should be stored in `data.json`
- **Input Validation:** HTML-escape all user-supplied data (work item names, milestone titles) to prevent XSS vulnerabilities
- **File System Security:** `data.json` should use standard Windows NTFS ACLs for access control (not enforced by app)
- **HTTPS:** Not required for local/intranet dashboard; HTTP localhost is acceptable
- **No Session Management:** Stateless application; each browser tab is independent

### Compatibility
- **Operating System:** Windows 10, Windows 11, Windows Server 2019+
- **Browser:** Modern browsers only (Chrome 90+, Edge 90+, Firefox 88+, Safari 14+ on rare Mac use)
- **Internet Explorer 11:** Explicitly not supported (Blazor Server incompatible with IE11)
- **.NET Runtime:** .NET 8.0 runtime included in self-contained `.exe`; no external .NET installation required by end user
- **Screen Resolution:** Optimized for 1920×1080; lesser resolutions may require scrolling (not a bug, by design)

### Maintainability
- **Code Structure:** Modular Blazor component hierarchy (`Dashboard.razor` → `Header.razor`, `Timeline.razor`, `Heatmap.razor`)
- **Configuration:** All app settings (colors, fonts, grid dimensions) should be centralizable in `DesignConstants.cs` or `appsettings.json`
- **Testing:** Unit tests for business logic (JSON loading, date calculations, validation)
- **Documentation:** Inline code comments for non-obvious logic; user guide for editing `data.json`

### Usability
- **Visual Clarity:** All colors and text must be legible at 1920×1080 resolution on standard monitors
- **Consistency:** Design must match OriginalDesignConcept.html pixel-for-pixel (or with justified improvements)
- **Simplicity:** No hidden features, no settings dialogs, no configuration UI — all customization via `data.json`
- **Accessibility:** Color contrast should be reasonable (not formally WCAG AA/AAA audited, but vibrant colors may fail some checks)
- **Print-Friendly:** Print preview and screenshot capture must produce visually identical output to on-screen

---

## Success Metrics

1. **Visual Design Match:** Dashboard layout and styling must match OriginalDesignConcept.html reference image with 95%+ pixel-for-pixel accuracy (as measured by manual side-by-side comparison)

2. **Functional Completeness:** All four major sections (Header, Timeline, Heatmap, Legend) render correctly with data from `data.json` populated accurately

3. **Performance:**
   - Page load time ≤ 3 seconds on a standard Windows machine
   - SVG timeline renders without visible lag even with 20+ milestones
   - Memory usage ≤ 200 MB at rest

4. **Data Flexibility:** `data.json` schema supports:
   - 3–10 milestones with configurable dates, titles, and colors
   - 4–6 months of timeline history
   - 2–50 work items distributed across 4 status rows
   - Nullable/empty fields handled gracefully

5. **Executable Quality:**
   - Single `.exe` file (≤ 150 MB) that runs without installation on Windows 10+
   - No external dependencies or prerequisites required (except Windows OS)
   - Copies to any directory and runs correctly (no registry entries, no system paths)

6. **Screenshot Optimization:**
   - Entire dashboard fits within 1920×1080 viewport without scrolling
   - Screenshot capture produces clean image with no browser chrome
   - Print preview (Ctrl+P) shows single-page output with preserved colors
   - Image can be directly embedded in PowerPoint with minimal/no resizing

7. **Error Handling:**
   - Missing or malformed `data.json` displays specific error message (not generic crash)
   - Invalid dates, null fields, and empty arrays are handled without exceptions
   - Application remains responsive even with invalid data (shows error page, not hung state)

8. **Testing Coverage:**
   - ≥ 80% unit test coverage of `ReportService` (JSON loading, deserialization)
   - ≥ 70% unit test coverage of `Timeline` calculations (date-to-coordinate math)
   - All tests pass before release

9. **Documentation:**
   - User guide provided explaining how to run `.exe` and edit `data.json`
   - `data.json` schema documented with all required and optional fields
   - README includes screenshots showing before/after of editing data

10. **Deployment:**
    - Build and publish process documented (one-command `dotnet publish`)
    - Tested successful deployment on 2+ Windows machines (not build machine only)
    - `.exe` and `data.json` can be copied together and run from any directory

---

## Constraints & Assumptions

### Technical Constraints

1. **Mandatory C# .NET 8 Stack:** Project must use Blazor Server (ASP.NET Core 8.0) as front-end framework; no Vue, React, or other JavaScript frameworks
2. **Self-Contained Executable:** Must produce a single `.exe` file (no separate runtime installation); use `PublishSingleFile=true` and `SelfContained=true` in `.csproj`
3. **Windows-Only Deployment:** Target Windows 10+ only; Linux/macOS support explicitly out of scope
4. **Local/Intranet Only:** No cloud deployment, no HTTPS required, no authentication needed
5. **JSON-Based Data:** No database (SQL Server, Postgres, etc.); all data persists in `data.json` file
6. **No Build Toolchain for End Users:** End users should not need Visual Studio, .NET SDK, or any build tools; just download `.exe` and run
7. **Fixed-Width Design:** 1920×1080 is the target resolution; responsive design is not required

### Timeline Assumptions

1. **MVP Timeline:** 2–3 weeks to deliver functional, tested, production-ready executable (Phase 1–2 per research findings)
2. **Single Developer/Team:** Assumes 1–2 developers with C# and Blazor experience can complete the work
3. **No External Dependencies:** All required packages (Blazor Server, System.Text.Json) are included in .NET 8; no waiting for third-party library releases
4. **Design Finalized:** OriginalDesignConcept.html design is fixed; no major design changes mid-project

### Data & Process Assumptions

1. **Manual Data Updates:** Project manager edits `data.json` with a text editor; no API integration or automated sync in MVP
2. **Single Project Per Executable:** One `.exe` corresponds to one project dashboard; multi-tenancy is a Phase 3 enhancement
3. **Small User Base:** Expected users are < 10 (e.g., execs and PM team); no load balancing or multi-instance deployment needed
4. **Non-Concurrent Access:** No simultaneous edits to `data.json`; file locking is not implemented (accept data loss risk for MVP)
5. **Data Retention:** `data.json` is the permanent source of truth; no need for historical snapshots or audit logs (Git versioning of JSON file is sufficient)
6. **Quarterly Update Cadence:** `data.json` is expected to be updated monthly or quarterly, not in real-time

### Team & Skill Assumptions

1. **Team has C# experience:** Team is familiar with C# and object-oriented programming
2. **Team has Blazor familiarity:** Team has used or worked with Blazor components, Razor syntax, and component lifecycle (`OnInitializedAsync`, etc.)
3. **CSS Grid knowledge:** Team or designer understands CSS Grid and Flexbox layout
4. **JSON literacy:** End users (project managers) can edit JSON with a text editor and understand basic syntax rules
5. **No specialized training required:** No need for external courses or certifications; standard .NET development practices apply

### Business & Usage Assumptions

1. **Executive-Focused Tool:** Dashboard is intended for C-level (executives, directors, VPs) not for detailed execution tracking
2. **Screenshot-Primary Usage:** Primary use case is capturing images for PowerPoint decks and email briefings, not live dashboard monitoring
3. **Infrequent Access:** Executives check dashboard once weekly or monthly; not a daily mission-critical tool
4. **Non-Sensitive Data:** Dashboard displays only project status milestones, not confidential financial data, security secrets, or PII
5. **Internal Distribution:** Dashboard is intended for internal team only; no external sharing or internet-facing deployment
6. **Browser Access Assumed:** End users have a modern web browser installed (implied by Windows desktop environment)

### Design & UX Assumptions

1. **OriginalDesignConcept.html is Canonical:** The HTML/CSS reference design is the single source of truth for visual appearance; any deviations must be justified and approved
2. **Simplicity Over Features:** Dashboard prioritizes clean, uncluttered UI over advanced features; no navigation menus, settings dialogs, or hidden functionality
3. **Read-Only Interface:** No in-app data editing; all changes come via external JSON file edits
4. **No Animations:** Static rendering preferred for simplicity and screenshot consistency; no CSS transitions or JavaScript animations in MVP
5. **Color Accessibility Not Formalized:** Design uses vibrant colors that may not meet WCAG AA/AAA contrast ratios; MVP focuses on visual match to design, not formal accessibility audit

### Deployment & Operations Assumptions

1. **Windows Admin Rights Not Required:** End users do not need admin rights to download and run `.exe` (file system permissions may restrict write access, but read is sufficient)
2. **Automatic .NET Runtime:** Self-contained `.exe` includes .NET 8 runtime; no separate installation step
3. **No Network Connectivity Required:** Dashboard works offline; no API calls or external data sources in MVP
4. **Standard Windows File System:** `data.json` stored as plain text on local disk or network share; no special file permissions required
5. **Browser Auto-Opens:** `.exe` automatically launches the default browser on startup (standard Blazor Server behavior)
6. **Port 5000 Available:** Assumes localhost:5000 (or next available port) is accessible for browser communication; firewall rules not a concern for local machine

---

# Appendix: Data Schema

## data.json Structure

```json
{
  "reportTitle": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform – Privacy Automation Workstream – April 2026",
  "adoBacklogUrl": "https://dev.azure.com/...",
  "milestones": [
    {
      "id": "M1",
      "title": "Chatbot & MS Role",
      "color": "#0078D4",
      "startDate": "2026-01-12",
      "checkpoints": [
        {
          "date": "2026-02-11",
          "label": "Checkpoint 1"
        }
      ],
      "pocMilestone": {
        "date": "2026-03-26",
        "label": "Mar 26 PoC",
        "color": "#F4B400"
      },
      "productionRelease": {
        "date": "2026-04-15",
        "label": "Apr Prod (TBD)",
        "color": "#34A853"
      }
    }
  ],
  "statusRows": [
    {
      "category": "Shipped",
      "headerCssClass": "ship-hdr",
      "cellCssClass": "ship-cell",
      "headerColor": "#1B7A28",
      "items": [
        {
          "month": "March",
          "value": "API v1 Released"
        },
        {
          "month": "April",
          "value": "UI Dashboard Shipped"
        }
      ]
    },
    {
      "category": "In Progress",
      "headerCssClass": "prog-hdr",
      "cellCssClass": "prog-cell",
      "headerColor": "#1565C0",
      "items": [
        {
          "month": "April",
          "value": "Backend optimization"
        }
      ]
    },
    {
      "category": "Carryover",
      "headerCssClass": "carry-hdr",
      "cellCssClass": "carry-cell",
      "headerColor": "#B45309",
      "items": [
        {
          "month": "May",
          "value": "Mobile app redesign"
        }
      ]
    },
    {
      "category": "Blockers",
      "headerCssClass": "block-hdr",
      "cellCssClass": "block-cell",
      "headerColor": "#991B1B",
      "items": []
    }
  ]
}
```