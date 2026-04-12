# PM Specification: My Project

## Executive Summary

My Project is a simple, single-page executive reporting dashboard that visualizes project milestones and progress across four categorical statuses (Shipped, In Progress, Carryover, Blockers) organized by monthly time periods. Built on Blazor Server (.NET 8.0) with data sourced from a local JSON configuration file, the dashboard provides a clean, screenshot-friendly visual overview of project health suitable for executive decks and stakeholder presentations. The system requires no authentication, cloud infrastructure, or complex enterprise features—just straightforward visualization of what's shipping, what's in progress, what carried over from last month, and what's blocking.

## Business Goals

1. **Provide single-source-of-truth project visibility**: Enable executives to see project status at a glance without drilling into multiple systems or reports.

2. **Enable easy executive communication**: Support creation of professional, clean screenshots for PowerPoint decks and stakeholder meetings without manual image editing.

3. **Reduce status reporting overhead**: Eliminate manual dashboard updates by reading data from a simple, version-controllable JSON configuration file.

4. **Visualize milestone progress and blockers**: Display key project milestones (PoC gates, production releases) alongside monthly execution status (shipped items, in-progress work, carryover, and blockers).

5. **Support rapid iteration**: Use a lightweight technology stack (no database, no auth, no external dependencies) to enable quick updates to data and design without deployment friction.

## User Stories & Acceptance Criteria

### Story 1: Executive Views Project Dashboard

**As a** project executive or stakeholder,  
**I want** to view the complete project status dashboard on a single page,  
**so that** I can quickly assess overall project health and milestone progress without needing to navigate multiple reports.

**Acceptance Criteria:**
- [ ] Dashboard loads on page open and displays all required sections (header, timeline, heatmap, legend) without requiring user interaction
- [ ] Page displays cleanly at 1920×1080 resolution (standard executive presentation size)
- [ ] Title and subtitle are prominently displayed at the top with project context (see Visual Design Specification section 1)
- [ ] All four status categories (Shipped, In Progress, Carryover, Blockers) are visible and clearly labeled
- [ ] Timeline section displays milestone indicators with dates
- [ ] Data renders from data.json without errors
- [ ] No loading spinners or delay is visible; page renders instantly

### Story 2: Executive Understands Milestone Timeline

**As a** project stakeholder,  
**I want** to see a horizontal timeline showing key project milestones (PoC gates, production releases) with dates and status indicators,  
**so that** I can understand the project's major delivery gates and expected completion dates.

**Acceptance Criteria:**
- [ ] Timeline displays horizontally with month dividers (January, February, March, April, etc.) as shown in design (see Visual Design Specification section 2)
- [ ] Milestones are represented as diamond-shaped markers with color coding: yellow (#F4B400) for PoC milestones, green (#34A853) for production releases
- [ ] Start and end dates of major project tracks are shown as checkpoints (small circles) on timeline
- [ ] "NOW" indicator (red dashed line) shows current month position on timeline
- [ ] Milestone names (M1, M2, M3, etc.) and descriptions appear in a legend panel to the left of the timeline
- [ ] Legend shows milestone ID, name, and scope (e.g., "M1: Chatbot & MS Role")
- [ ] Timeline scales to accommodate 6-12 month date ranges without horizontal scrolling

### Story 3: Executive Tracks Monthly Execution Status

**As a** project manager,  
**I want** to see a grid displaying shipped items, in-progress work, carryover items, and blockers organized by month,  
**so that** I can quickly identify what was completed, what's active, and what risks need addressing.

**Acceptance Criteria:**
- [ ] Heatmap grid displays 4 status rows (Shipped, In Progress, Carryover, Blockers) and 4+ month columns
- [ ] Grid header row shows month names (March, April, May, June) with "April (Now)" highlighted in yellow background
- [ ] Each status row has a color-coded left header: green for Shipped, blue for In Progress, yellow for Carryover, red for Blockers (see Visual Design Specification section 3)
- [ ] Data cells display item names as bulleted lists with colored dot indicators matching the row color
- [ ] April column (current month) has a slightly darker background shade to distinguish it from past/future months
- [ ] Grid uses CSS Grid layout with fixed left column (160px) and proportional month columns (see Visual Design Specification section 3.2)
- [ ] Empty cells display "-" in light gray to indicate no items in that status/month combination

### Story 4: Executive Understands Status Legend

**As a** project stakeholder,  
**I want** to see a legend explaining the visual markers and colors used in the dashboard,  
**so that** I can correctly interpret milestones, statuses, and timeline indicators without ambiguity.

**Acceptance Criteria:**
- [ ] Legend displays in the top-right corner of the header section (see Visual Design Specification section 1.3)
- [ ] Legend shows 4 symbol types with labels:
  - [ ] Yellow rotated square: "PoC Milestone"
  - [ ] Green rotated square: "Production Release"
  - [ ] Gray circle: "Checkpoint"
  - [ ] Red vertical line: "Now (Apr 2026)"
- [ ] Legend items use same colors and symbols as timeline
- [ ] Legend text is legible (font size 12px, readable against background)
- [ ] Legend is vertically aligned with header content

### Story 5: Executive Takes Screenshot for Presentation

**As a** project lead,  
**I want** to take a clean screenshot of the dashboard suitable for PowerPoint decks,  
**so that** I can use the visual in executive presentations without additional editing or annotation.

**Acceptance Criteria:**
- [ ] Dashboard renders with no scrollbars, overflow content, or layout shifts
- [ ] Page fits within 1920×1080 viewport without horizontal or vertical scrolling
- [ ] All text is clearly legible at standard presentation zoom levels (100-150%)
- [ ] Colors are distinct and accessible (sufficient contrast ratio ≥4.5:1 for text)
- [ ] Browser print-to-PDF function produces layout-accurate output
- [ ] Optional: "Print" button exists on dashboard to trigger browser print dialog (not required for MVP)

### Story 6: Project Manager Updates Dashboard Data

**As a** project manager or product owner,  
**I want** to update the dashboard with new status data by editing a simple JSON file,  
**so that** I can refresh the dashboard without requiring a developer to rebuild or redeploy the application.

**Acceptance Criteria:**
- [ ] data.json file exists in the application root directory with human-readable JSON structure
- [ ] data.json contains sections for: projectName, projectContext, timePeriods, milestones, statusCategories
- [ ] Each status category object contains: name, color (hex code), backgroundColor (hex code), items list
- [ ] Each item in a status category contains: name (string), periods (array of strings/booleans indicating presence in each month)
- [ ] Editing data.json and restarting the application reflects new data on dashboard
- [ ] data.json template includes inline comments explaining the schema
- [ ] Invalid JSON causes graceful error display rather than app crash

## Visual Design Specification

### Section 1: Header & Legend (Top Section, Full Width)

**Layout & Structure:**
- Horizontal flexbox container spanning full viewport width (1920px)
- Left-aligned title section with right-aligned legend
- Height: approximately 50-60px including padding
- Background: solid white (#FFFFFF)
- Border-bottom: 1px solid gray (#E0E0E0)
- Padding: 12px 44px (top/bottom 12px, left/right 44px)

**Title Section (Left):**
- H1 heading: "Privacy Automation Release Roadmap" (or project name from data.json)
- Font: Segoe UI, 24px, font-weight 700 (bold)
- Color: dark gray (#111)
- Optional link text: "→ ADO Backlog" (color: #0078D4, underline on hover)
- Subtitle line below heading: "Trusted Platform · Privacy Automation Workstream · April 2026"
- Subtitle font: 12px, color: medium gray (#888), margin-top: 2px

**Legend Section (Right):**
- Flexbox row with gap: 22px between legend items
- 4 legend items, each displaying a symbol + label:
  1. Yellow rotated square (12×12px, transform: rotate(45deg), background: #F4B400) + text "PoC Milestone"
  2. Green rotated square (12×12px, transform: rotate(45deg), background: #34A853) + text "Production Release"
  3. Gray circle (8×8px, border-radius: 50%, background: #999) + text "Checkpoint"
  4. Red vertical line (2px wide, 14px tall, background: #EA4335) + text "Now (Apr 2026)"
- Each legend item: font-size 12px, flex display, centered vertically
- Symbol-to-label gap: 6px

### Section 2: Timeline Section (Milestone Gantt Chart)

**Layout & Structure:**
- Horizontal flexbox container below header
- Height: 196px (flex-shrink: 0)
- Padding: 6px 44px 0
- Border-bottom: 2px solid light gray (#E8E8E8)
- Background: light off-white (#FAFAFA)
- Contains two sub-sections: milestone legend panel (left) + SVG timeline (right)

**Milestone Legend Panel (Left):**
- Width: 230px (flex-shrink: 0)
- Flex column layout, space-around distribution
- Padding: 16px 12px 16px 0
- Border-right: 1px solid #E0E0E0
- 3 milestone entries (M1, M2, M3), each:
  - Milestone ID (bold, font-size 12px, font-weight 600, color varies per milestone)
  - M1: color #0078D4 (blue)
  - M2: color #00897B (teal)
  - M3: color #546E7A (gray-blue)
  - Milestone name below ID (font-size 12px, font-weight 400, color #444)
  - Line-height: 1.4

**SVG Timeline (Right):**
- Width: variable (flex: 1)
- Height: 185px
- SVG viewBox: 1560×185
- Rendering method: inline SVG generated by Blazor component (MarkupString)
- Content (from OriginalDesignConcept.html reference):
  - **Month dividers**: vertical light gray lines at 260px intervals (Jan, Feb, Mar, Apr, May, Jun)
  - **Month labels**: text "Jan", "Feb", "Mar", etc. positioned above dividers, font-size 11px, font-weight 600, fill #666
  - **"NOW" indicator**: red dashed vertical line (stroke #EA4335, stroke-width 2, stroke-dasharray "5,3") with "NOW" label (font-size 10px, font-weight 700, fill #EA4335)
  - **Milestone tracks**: 3 horizontal lines, one per milestone:
    - M1 track: stroke #0078D4, stroke-width 3, y=42
    - M2 track: stroke #00897B, stroke-width 3, y=98
    - M3 track: stroke #546E7A, stroke-width 3, y=154
  - **Checkpoint markers**: circles on tracks
    - Large circle (r=7): white fill, colored stroke (milestone color), stroke-width 2.5 (indicates start point)
    - Medium circle (r=5): white fill, gray stroke, stroke-width 2.5 (indicates checkpoint)
    - Small filled circles (r=4): gray fill #999 (indicates checkpoint without stroke)
  - **Milestone markers**: rotated diamond/square polygons on tracks
    - Yellow diamond (#F4B400) for PoC milestones with shadow filter
    - Green diamond (#34A853) for production releases with shadow filter
    - Polygon points: (x, y-11), (x+11, y), (x, y+11), (x-11, y) to form rotated square
  - **Date labels**: text positioned above/below markers, font-size 10px, fill #666

### Section 3: Heatmap Grid Section (Status & Execution)

**Layout & Structure:**
- Flex column container below timeline
- Flex: 1 (expand to fill remaining viewport space)
- Padding: 10px 44px 10px
- Minimum height: 0 to prevent flex overflow

**Heatmap Title:**
- Font-size: 14px, font-weight: 700, color: #888
- Text-transform: uppercase
- Letter-spacing: 0.5px
- Margin-bottom: 8px
- Flex-shrink: 0
- Label: "Monthly Execution Heatmap - Shipped · In Progress · Carryover · Blockers"

**CSS Grid Layout:**
- Display: CSS Grid
- Grid-template-columns: 160px repeat(4, 1fr) — left column fixed at 160px, 4 month columns proportionally distributed
- Grid-template-rows: 36px repeat(4, 1fr) — header row 36px, 4 status rows proportionally distributed
- Border: 1px solid #E0E0E0
- Flex: 1 (expand to fill container), min-height: 0

**Header Row (Grid Row 1):**
- **Corner cell** (Grid position [0,0]):
  - Background: #F5F5F5
  - Text: "Status" (uppercase, font-size 11px, font-weight 700, color #999)
  - Display: flex, centered
  - Border-right: 1px solid #E0E0E0
  - Border-bottom: 2px solid #CCC
- **Column headers** (Grid positions [0,1-4]):
  - One cell per month: "March", "April", "May", "June"
  - Font-size: 16px, font-weight: 700
  - Background: #F5F5F5 (or #FFF0D0 with color #C07700 for April = "Now")
  - Display: flex, centered
  - Border-right: 1px solid #E0E0E0
  - Border-bottom: 2px solid #CCC

**Status Rows (Grid Rows 2-5):**
- 4 rows for status categories: Shipped, In Progress, Carryover, Blockers
- Each row has 5 cells: row header + 4 month data cells

**Row Header Cells (Grid Column 0):**
- **Shipped row**:
  - Text: "✓ Shipped"
  - Color: #1B7A28 (dark green)
  - Background: #E8F5E9 (light green)
  - Border-right: 2px solid #CCC
  - Border-bottom: 1px solid #E0E0E0
- **In Progress row**:
  - Text: "→ In Progress"
  - Color: #1565C0 (dark blue)
  - Background: #E3F2FD (light blue)
  - Border-right: 2px solid #CCC
  - Border-bottom: 1px solid #E0E0E0
- **Carryover row**:
  - Text: "⟲ Carryover"
  - Color: #B45309 (dark orange/amber)
  - Background: #FFF8E1 (light yellow)
  - Border-right: 2px solid #CCC
  - Border-bottom: 1px solid #E0E0E0
- **Blockers row**:
  - Text: "⚠ Blockers"
  - Color: #991B1B (dark red)
  - Background: #FEF2F2 (light red)
  - Border-right: 2px solid #CCC
  - Border-bottom: 1px solid #E0E0E0
- All row headers:
  - Font-size: 11px, font-weight: 700, text-transform: uppercase, letter-spacing: 0.7px
  - Display: flex, padding: 0 12px, align-items: center
  - Font family: Segoe UI

**Data Cells (Grid positions [1-4, 1-4]):**
- Each cell displays items for that status/month combination
- Item format: colored dot (inline marker) + item name
- Display items as list, each on separate line
- Cell padding: 8px 12px
- Cell borders: right 1px #E0E0E0, bottom 1px #E0E0E0

**Cell Content (Items):**
- Font-size: 12px, line-height: 1.35
- Item class: `.it`
- Each item has:
  - `::before` pseudo-element: 6px diameter circle (border-radius: 50%)
  - Color matches status row (green #34A853 for Shipped, blue #0078D4 for In Progress, yellow #F4B400 for Carryover, red #EA4335 for Blockers)
  - Circle positioned left: 0, top: 7px
  - Text padding: 2px 0 2px 12px
  - Text color: #333
- Empty cells show "-" with color #AAA

**Cell Background Colors by Status & Month:**
- **Shipped cells**: background #F0FBF0 (default), #D8F2DA (April/current month)
- **In Progress cells**: background #EEF4FE (default), #DAE8FB (April/current month)
- **Carryover cells**: background #FFFDE7 (default), #FFF0B0 (April/current month)
- **Blockers cells**: background #FFF5F5 (default), #FFE4E4 (April/current month)

**Grid Dimensions:**
- Fixed width: 1920px (no horizontal scroll)
- Flexible height: expands to fill viewport remainder
- All text remains within cell bounds; overflow hidden
- Responsive: columns maintain proportional width across months

## UI Interaction Scenarios

**Scenario 1: User Lands on Dashboard**
The user opens the web application in a browser and lands on the dashboard home page. The page loads instantly (no loading spinner), and all sections are visible: header with title and legend (top), timeline with milestones (middle), and heatmap grid with status data (bottom-half). No user interaction is required; the dashboard displays a read-only project overview.

**Scenario 2: User Scans Header & Understands Context**
The user reads the dashboard header (left side) and learns the project name ("Privacy Automation Release Roadmap"), project context ("Trusted Platform · Privacy Automation Workstream · April 2026"), and available link to backlog. They scan the legend (right side) to understand symbols: yellow diamond = PoC Milestone, green diamond = Production Release, gray circle = Checkpoint, red line = Now marker. This takes <10 seconds.

**Scenario 3: User Views Milestone Timeline**
The user's eye moves to the timeline section and immediately sees 3 milestone tracks (M1, M2, M3) with horizontal colored lines. They identify key dates: M1 started Jan 12, has a PoC gate at Mar 26, and production release in Apr (TBD). They see M2 started Dec 19, has checkpoints in Feb and Mar, and production in late Mar. They see M3 started Jan 5 and has a PoC gate in late Mar. The red "NOW" line indicates current position (April 2026).

**Scenario 4: User Scans Monthly Execution Heatmap**
The user shifts focus to the heatmap grid and reads status by month. For March (past month), they see items shipped, in progress, and carried over. For April (current month, highlighted yellow header), they see what's being shipped now and what's in progress this month. For May and June (future months), they see carryover items and blockers. Row colors (green, blue, yellow, red) make status category instantly recognizable.

**Scenario 5: User Identifies Blockers & Risks**
The user scans the red "Blockers" row and identifies blocking issues in April (current month). They see which blocking issues are new vs. carried over from March. This 20-second scan reveals risks needing executive attention.

**Scenario 6: User Prepares Screenshot for Executive Presentation**
The user opens a screenshot tool (macOS Cmd+Shift+4, Windows Snip & Sketch, or Chrome DevTools) and captures the dashboard. The entire page fits within the browser viewport (1920×1080), so the screenshot includes header, timeline, and heatmap without scrolling. Colors are distinct, text is legible, and the visual is presentation-ready. User pastes screenshot directly into PowerPoint without additional editing.

**Scenario 7: User Interacts with Timeline (Future Enhancement)**
The user hovers over a milestone diamond marker on the timeline and sees a tooltip showing milestone name, type (PoC/Production), and target date. Hovering over a checkpoint circle shows the checkpoint name and date. This behavior is a future enhancement (not in MVP) but designed to support.

**Scenario 8: User Clicks on Heatmap Cell (Future Enhancement)**
The user clicks on a heatmap cell containing "Feature A" and sees a modal dialog with additional details: epic owner, description, dependencies, risks. This drill-down behavior is deferred post-launch but the design should not preclude it.

**Scenario 9: Application Loads from Corrupted data.json**
The developer accidentally corrupts data.json with invalid JSON syntax. The user accesses the dashboard and sees an error message: "Error loading project data: Invalid JSON in data.json. Please check file syntax." The page does not crash; the error is displayed in a visible alert box. The developer corrects the JSON and restarts the application.

**Scenario 10: Application Loads with Minimal Example Data**
A new developer clones the repository and runs `dotnet watch run` locally. The dashboard loads with example data (fictional project, 4 months, all status categories populated). This proves the system works end-to-end without requiring manual data entry.

## Scope

### In Scope

- **Single-page dashboard** displaying project milestones and monthly execution status
- **Milestone timeline visualization** with SVG rendering (month dividers, milestone markers, checkpoints, "NOW" indicator)
- **Status heatmap grid** with CSS Grid layout (4 status rows × 4-6 month columns, color-coded cells with item lists)
- **Header section** with project title, subtitle, context, and legend
- **Data loading from local JSON file** (data.json) on application startup
- **Blazor Server application** running on .NET 8.0 with no external dependencies (System.Text.Json built-in)
- **Responsive layout** optimized for 1920×1080 viewport (executive presentation size)
- **Browser print support** (standard browser print-to-PDF workflow)
- **Example/sample data** included for fictional project (Privacy Automation Release Roadmap or similar)
- **Read-only dashboard** with no data editing UI (data changes via direct JSON file edits only)
- **Error handling** for malformed JSON files (graceful error display)
- **Documentation** including README and data.json schema comments

### Out of Scope

- **Multi-project support** (single project hardcoded per dashboard instance; multi-project support deferred post-launch)
- **Real-time data updates** (data refreshes only on application restart; file watcher auto-reload deferred)
- **Authentication & authorization** (internal tool, no user login or role-based access control)
- **Cloud deployment** (local/on-premises hosting only; cloud infrastructure deferred)
- **Interactive features** (no click drill-down, no hover tooltips, no interactive brushing; designed to not preclude future enhancements)
- **Mobile/responsive design** (tablet/mobile support deferred; desktop-only 1920×1080 target)
- **Database or persistent storage** beyond local JSON file
- **Export to formats** beyond browser print-to-PDF (PNG export, Excel export deferred)
- **Historical tracking or version control** (no archival or comparison views)
- **Theme customization** (light mode only; dark mode deferred)
- **Accessibility** beyond basic WCAG 2.1 AA compliance (no screen reader testing, no keyboard navigation beyond standard browser controls)
- **Automated testing** (unit tests optional, integration tests optional)
- **CI/CD pipeline** (manual testing and deployment only)

## Non-Functional Requirements

### Performance

- **Page load time**: < 1 second (including JSON parsing and render)
- **Time-to-interactive**: < 2 seconds
- **SVG rendering**: Timeline with 50-60 month range must render without layout jank
- **CSS Grid render**: Heatmap with 4-5 rows × 4-6 columns must render instantly
- **Memory footprint**: Application should consume < 200 MB RAM on startup
- **Data file size**: data.json should remain < 500 KB (typical size ~50 KB for 4 months × 20 items)

### Reliability

- **Uptime**: Not applicable (local deployment); when running, dashboard should not crash
- **Error recovery**: If data.json is missing or corrupt, app displays friendly error message and continues running (does not crash)
- **Data integrity**: No data loss if application is restarted (all data read from data.json on startup)

### Accessibility

- **Text contrast**: All text must have contrast ratio ≥ 4.5:1 against background (WCAG 2.1 AA minimum)
- **Color not sole indicator**: Status information conveyed through both color and text label (not color alone)
- **Keyboard navigation**: Blazor Server provides browser-default keyboard support; no additional keyboard shortcuts required for MVP
- **Screen reader support**: Deferred post-launch (not required for executive presentation dashboard)

### Security

- **Data encryption**: Not required (fictional example data only; if sensitive, use OS-level file permissions)
- **HTTPS**: Not required for local deployment; HTTP on localhost:5000 acceptable
- **Input validation**: data.json is server-side only; no user input to validate
- **Dependency vulnerabilities**: Zero external NuGet dependencies (only .NET 8.0 framework built-ins)

### Scalability

- **Concurrent users**: Single-user focus (developer or presenter); Blazor Server supports <100 concurrent connections without load balancing (not required for MVP)
- **Data growth**: Heatmap grid should support up to 50 items per status category and 12 months without performance degradation
- **Timeline granularity**: Support 1-12 month ranges without layout overflow

### Browser Compatibility

- **Minimum browsers**: Chrome 100+, Edge 100+, Firefox 100+, Safari 15+ (modern browsers only)
- **Mobile/tablet**: Not required for MVP (desktop-only)
- **Print support**: CSS must remain valid when printed to PDF via browser print dialog

## Success Metrics

1. **Deployment**: Application successfully builds and runs with `dotnet run` on developer machine within 1 minute
2. **Data loading**: data.json with example data (4 months, 4 statuses, 15+ items) loads and renders without errors on page load
3. **Visual match**: Dashboard pixel-accurate screenshot matches OriginalDesignConcept.html design template (colors, fonts, spacing, grid layout)
4. **Milestone timeline**: SVG timeline renders all 3+ milestones with correct date positioning, colored markers, and "NOW" indicator visible
5. **Heatmap grid**: CSS Grid displays correct number of rows (4) and columns (4-6), with correct color coding and item text content
6. **Screenshot export**: Full dashboard screenshot taken via browser tool/print fits within 1920×1080 without scrolling and is presentation-ready
7. **Error handling**: Malformed data.json produces readable error message on dashboard (app does not crash)
8. **Documentation**: README includes instructions for updating data.json and example schema
9. **Execution time**: First user should be able to clone repo, run app, and see working dashboard within 5 minutes
10. **Stakeholder feedback**: One executive user confirms dashboard is suitable for PowerPoint presentation without modification

## Constraints & Assumptions

### Technical Constraints

- **Technology locked**: C# / Blazor Server / .NET 8.0 (as determined by research findings)
- **No external dependencies**: Zero NuGet packages beyond .NET 8.0 framework (System.Text.Json only)
- **File-based data**: Single data.json configuration file; no database allowed
- **Local deployment**: No cloud infrastructure (AWS, Azure, GCP); on-premises or developer machine only
- **Single project per instance**: One application instance = one project dashboard; multi-project support requires separate instances or code changes

### Timeline Assumptions

- **Development time**: 1-2 weeks for MVP based on research findings
- **Data update frequency**: Monthly or quarterly (manual data.json edits, no automated sync)
- **Release target**: ASAP; no specific date dependency

### Business Assumptions

- **Audience**: Internal executives and stakeholders only (no external customer access)
- **Data sensitivity**: Fictional project data for demonstration; if real project data used, apply OS-level file permissions
- **Executive intent**: Dashboard used for presentations and internal communication, not operational tooling
- **Change process**: Product manager updates data.json in version control; deployment via standard application restart
- **Success definition**: Executive feedback confirms dashboard is suitable for PowerPoint use; no production SLA required

### Design Assumptions

- **Viewport**: 1920×1080 desktop resolution (standard presentation size); responsive design deferred
- **Data structure**: Up to 6 months, 4 status categories, 20-30 items total; larger datasets may require pagination (deferred)
- **Interaction model**: Read-only dashboard; no user-editable forms in UI
- **Visual fidelity**: CSS Grid + inline SVG match OriginalDesignConcept.html design; no custom charting libraries needed

### Team & Skills Assumptions

- **Developer expertise**: Team member(s) familiar with C#, ASP.NET Core, Blazor basics
- **Designer involvement**: Product manager or designer manages visual specification; developers implement per spec
- **Testing approach**: Manual testing only; automated unit/integration tests optional
- **DevOps**: Developers handle local builds and manual deployment to target machine