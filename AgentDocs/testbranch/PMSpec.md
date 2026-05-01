# PM Specification: ReportingDashboard

**Document Owner:** Privacy Automation PM Team
**Last Updated:** May 1, 2026
**Status:** Draft
**Design Reference:** `OriginalDesignConcept.html` — rendered at 1920×1080

---

## Executive Summary

The ReportingDashboard is an internal, read-only web application that visualizes the Privacy Automation Release Roadmap for executive stakeholders and program managers. It combines a Gantt-style SVG timeline (with milestone markers across multiple workstreams) and a color-coded heatmap grid (showing work item status by month) into a single 1920×1080 dashboard view, sourced live from Azure DevOps backlog data. The tool eliminates manual slide-deck updates by providing a real-time, always-current view of delivery status across the Trusted Platform · Privacy Automation workstream.

---

## Business Goals

1. **Eliminate manual reporting overhead** — Replace monthly PowerPoint roadmap slides with a live, auto-refreshing dashboard that pulls directly from Azure DevOps, saving ~4 hours/month of PM manual effort.
2. **Increase delivery visibility** — Provide executive stakeholders with an always-current view of what has shipped, what is in progress, what has carried over, and what is blocked, without requiring them to navigate Azure DevOps directly.
3. **Improve milestone tracking accuracy** — Surface PoC and Production Release milestones with their actual dates on a timeline, making schedule slips immediately visible rather than buried in backlog queries.
4. **Standardize status communication** — Establish a single, canonical view of delivery status that all stakeholders reference, reducing conflicting status interpretations across email threads and meetings.
5. **Enable data-driven sprint retrospectives** — The monthly heatmap provides a historical record of carryover and blockers, enabling the team to identify systemic delivery issues over time.

---

## User Stories & Acceptance Criteria

### US-1: View Dashboard at a Glance

**As a** program manager, **I want** to open the dashboard and immediately see the roadmap title, workstream context, current month, and a legend explaining all visual symbols, **so that** I can orient myself without needing any training.

**Visual Reference:** Header section of `OriginalDesignConcept.html`

- [ ] Dashboard displays title "Privacy Automation Release Roadmap" with a link to the ADO Backlog.
- [ ] Subtitle reads "Trusted Platform · Privacy Automation Workstream · [Current Month Year]".
- [ ] Legend shows four icons: gold diamond (PoC Milestone), green diamond (Production Release), gray circle (Checkpoint), red vertical line (Now).
- [ ] Page renders at 1920×1080 without horizontal or vertical scrollbars.

### US-2: View Milestone Timeline

**As an** executive stakeholder, **I want** to see a horizontal timeline with milestone markers for each workstream (M1, M2, M3), **so that** I can quickly assess where each initiative stands relative to the current date.

**Visual Reference:** Timeline area (`.tl-area`) of `OriginalDesignConcept.html`

- [ ] Left sidebar (230px wide) displays M1 ("Chatbot & MS Role"), M2 ("PDS & Data Inventory"), M3 ("Auto Review DFD") with workstream-colored labels.
- [ ] SVG area renders horizontal track lines for each workstream in their designated colors (M1: `#0078D4`, M2: `#00897B`, M3: `#546E7A`).
- [ ] Month gridlines appear at Jan, Feb, Mar, Apr, May, Jun with labeled headers.
- [ ] A dashed red vertical "NOW" line (`#EA4335`, `stroke-dasharray: 5,3`) appears at the current date position.
- [ ] PoC milestones render as gold diamonds (`#F4B400`), Production milestones as green diamonds (`#34A853`), checkpoints as gray circles (`#999`).
- [ ] Each milestone has a date label (e.g., "Mar 26 PoC") positioned above or below the track line.
- [ ] Diamond markers have a drop shadow (`feDropShadow dx=0 dy=1 stdDeviation=1.5 flood-opacity=0.3`).

### US-3: View Monthly Execution Heatmap

**As a** program manager, **I want** to see a grid of work items organized by status (Shipped, In Progress, Carryover, Blockers) and month, **so that** I can identify delivery trends and problem areas at a glance.

**Visual Reference:** Heatmap section (`.hm-wrap`) of `OriginalDesignConcept.html`

- [ ] Section title reads "MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS" in uppercase, 14px, `#888`.
- [ ] Grid displays 5 columns: Status label (160px) + 4 month columns (equal width).
- [ ] Grid displays 5 rows: Column headers (36px) + 4 status rows (equal height, filling remaining space).
- [ ] Current month column header is highlighted with gold background (`#FFF0D0`) and text color `#C07700`, labeled with "◀ Now".
- [ ] Each status row uses its designated color scheme (see Visual Design Specification).
- [ ] Work items appear as bulleted text (6×6px colored circle + 12px text) within their respective cells.
- [ ] Future month cells display a dash ("—") in `#AAA` when no items exist.

### US-4: Navigate to ADO Work Items

**As a** program manager, **I want** to click the "ADO Backlog" link in the header, **so that** I can jump directly to the Azure DevOps backlog for deeper investigation.

- [ ] Header contains a clickable link styled in `#0078D4` that opens the ADO backlog in a new tab.
- [ ] Link text includes a visual indicator (→ arrow or similar).

### US-5: Authenticate with Corporate Credentials

**As a** Microsoft employee, **I want** the dashboard to authenticate me automatically via my Entra ID session, **so that** I don't need separate credentials and unauthorized users cannot access the tool.

- [ ] Unauthenticated users are redirected to Microsoft Entra ID login.
- [ ] Authenticated users see the dashboard without additional prompts.
- [ ] Token is acquired silently on subsequent visits if the session is still valid.
- [ ] Non-Microsoft accounts are denied access.

### US-6: See Fresh Data Without Manual Refresh

**As an** executive stakeholder, **I want** the dashboard data to refresh automatically when I return to the tab, **so that** I always see current information without clicking a refresh button.

- [ ] Data is cached for 5 minutes (`staleTime`).
- [ ] When the browser window regains focus, stale data is automatically re-fetched.
- [ ] A "Last updated: [timestamp]" indicator is visible on the dashboard.

### US-7: Understand Data Unavailability

**As a** user, **I want** to see clear feedback when data cannot be loaded, **so that** I know the issue is with data fetching and not the dashboard itself.

- [ ] A loading skeleton is displayed while data is being fetched.
- [ ] If the ADO API call fails, an error message is displayed with a "Retry" button.
- [ ] If stale cached data exists, it is shown alongside a warning banner ("Showing cached data from [timestamp]. Live data unavailable.").

---

## Visual Design Specification

**Canonical Reference:** `OriginalDesignConcept.html` — see also the rendered screenshot `OriginalDesignConcept.png` at 1920×1080.

### Overall Layout

- **Viewport:** Fixed 1920×1080, no scroll, `overflow: hidden`.
- **Direction:** Vertical flex column (`display: flex; flex-direction: column`).
- **Background:** `#FFFFFF`.
- **Font Family:** `'Segoe UI', Arial, sans-serif`.
- **Base Text Color:** `#111`.

The page is divided into three vertical sections stacked top-to-bottom:

```
┌──────────────────────────────────────────────────┐
│  Header (flex-shrink: 0, ~46px)                  │
├──────────────────────────────────────────────────┤
│  Timeline Area (flex-shrink: 0, 196px)           │
├──────────────────────────────────────────────────┤
│  Heatmap Section (flex: 1, fills remaining)      │
└──────────────────────────────────────────────────┘
```

### Section 1: Header (`.hdr`)

- **Padding:** `12px 44px 10px`
- **Border-bottom:** `1px solid #E0E0E0`
- **Layout:** Flexbox, `align-items: center`, `justify-content: space-between`
- **Left side:**
  - **Title (`h1`):** "Privacy Automation Release Roadmap" — 24px, font-weight 700. Followed by an inline link "→ ADO Backlog" in `#0078D4`, no underline.
  - **Subtitle (`.sub`):** "Trusted Platform · Privacy Automation Workstream · April 2026" — 12px, `#888`, margin-top 2px.
- **Right side — Legend:** Horizontal flex row, 22px gap, containing four legend items at 12px font size:
  - Gold diamond (12×12px square rotated 45°, `#F4B400`) + "PoC Milestone"
  - Green diamond (12×12px square rotated 45°, `#34A853`) + "Production Release"
  - Gray circle (8×8px, `#999`) + "Checkpoint"
  - Red vertical bar (2×14px, `#EA4335`) + "Now (Apr 2026)"

### Section 2: Timeline Area (`.tl-area`)

- **Height:** 196px, `flex-shrink: 0`
- **Background:** `#FAFAFA`
- **Padding:** `6px 44px 0`
- **Border-bottom:** `2px solid #E8E8E8`
- **Layout:** Flexbox, `align-items: stretch`

**Left Sidebar (230px):**
- `flex-shrink: 0`, `border-right: 1px solid #E0E0E0`
- Three milestone labels vertically distributed (`justify-content: space-around`):
  - **M1** — bold 12px, `#0078D4`, subtitle "Chatbot & MS Role" in 12px, `#444`
  - **M2** — bold 12px, `#00897B`, subtitle "PDS & Data Inventory" in 12px, `#444`
  - **M3** — bold 12px, `#546E7A`, subtitle "Auto Review DFD" in 12px, `#444`

**Right SVG Area (`.tl-svg-box`, flex: 1):**
- SVG canvas: 1560×185px, `overflow: visible`
- **Month gridlines:** Vertical lines at x=0 (Jan), 260 (Feb), 520 (Mar), 780 (Apr), 1040 (May), 1300 (Jun) — stroke `#bbb`, opacity 0.4, width 1. Month labels at x+5, y=14, fill `#666`, 11px, font-weight 600.
- **"NOW" line:** Dashed vertical line at computed current-date x-position — stroke `#EA4335`, width 2, `stroke-dasharray: 5,3`. Label "NOW" at x+4, y=14, fill `#EA4335`, 10px, font-weight 700.
- **Track lines:** Three horizontal lines spanning full width:
  - M1 at y=42, stroke `#0078D4`, width 3
  - M2 at y=98, stroke `#00897B`, width 3
  - M3 at y=154, stroke `#546E7A`, width 3
- **Milestone markers on tracks:**
  - **Checkpoint (circle):** `fill: white`, stroke matching track color, stroke-width 2.5, radius 5–7px. Small filled gray circles (r=4, fill `#999`) for minor checkpoints.
  - **PoC diamond:** `<polygon>` forming an 11px diamond, `fill: #F4B400`, with drop shadow filter.
  - **Production diamond:** `<polygon>` forming an 11px diamond, `fill: #34A853`, with drop shadow filter.
  - **Drop shadow filter (`#sh`):** `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`
- **Date labels:** Positioned above or below milestone markers, 10px, fill `#666`, `text-anchor: middle`.

### Section 3: Heatmap Section (`.hm-wrap`)

- **Layout:** Flexbox column, `flex: 1`, `min-height: 0`
- **Padding:** `10px 44px 10px`

**Title (`.hm-title`):**
- Text: "MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS"
- 14px, font-weight 700, `#888`, uppercase, letter-spacing 0.5px, margin-bottom 8px.

**Grid (`.hm-grid`):**
- `display: grid`
- `grid-template-columns: 160px repeat(4, 1fr)`
- `grid-template-rows: 36px repeat(4, 1fr)`
- `border: 1px solid #E0E0E0`
- `flex: 1; min-height: 0`

**Corner Cell (`.hm-corner`):**
- Text: "STATUS" — 11px, font-weight 700, `#999`, uppercase
- Background `#F5F5F5`, border-right `1px solid #E0E0E0`, border-bottom `2px solid #CCC`

**Column Headers (`.hm-col-hdr`):**
- 16px, font-weight 700, background `#F5F5F5`
- Border-right `1px solid #E0E0E0`, border-bottom `2px solid #CCC`
- **Current month highlight (`.apr-hdr`):** background `#FFF0D0`, color `#C07700`, label includes "◀ Now"

**Row Headers (`.hm-row-hdr`):**
- 11px, font-weight 700, uppercase, letter-spacing 0.7px
- Border-right `2px solid #CCC`, border-bottom `1px solid #E0E0E0`
- Color scheme per status:

| Status | Header BG | Header Text | Cell BG | Current Month Cell BG | Bullet Color |
|---|---|---|---|---|---|
| ✅ Shipped | `#E8F5E9` | `#1B7A28` | `#F0FBF0` | `#D8F2DA` | `#34A853` |
| 🔵 In Progress | `#E3F2FD` | `#1565C0` | `#EEF4FE` | `#DAE8FB` | `#0078D4` |
| 🟡 Carryover | `#FFF8E1` | `#B45309` | `#FFFDE7` | `#FFF0B0` | `#F4B400` |
| 🔴 Blockers | `#FEF2F2` | `#991B1B` | `#FFF5F5` | `#FFE4E4` | `#EA4335` |

**Data Cells (`.hm-cell`):**
- Padding `8px 12px`, border-right `1px solid #E0E0E0`, border-bottom `1px solid #E0E0E0`
- Work items (`.it`): 12px, color `#333`, padding `2px 0 2px 12px`, line-height 1.35
- Bullet: 6×6px circle via `::before` pseudo-element, positioned absolute, left 0, top 7px, colored per status row
- Empty cells show "—" in `#AAA`

### CSS Custom Properties (Color Tokens)

```css
:root {
  --color-shipped-bg: #F0FBF0;
  --color-shipped-bg-current: #D8F2DA;
  --color-shipped-accent: #34A853;
  --color-shipped-header-bg: #E8F5E9;
  --color-shipped-header-text: #1B7A28;
  --color-progress-bg: #EEF4FE;
  --color-progress-bg-current: #DAE8FB;
  --color-progress-accent: #0078D4;
  --color-progress-header-bg: #E3F2FD;
  --color-progress-header-text: #1565C0;
  --color-carryover-bg: #FFFDE7;
  --color-carryover-bg-current: #FFF0B0;
  --color-carryover-accent: #F4B400;
  --color-carryover-header-bg: #FFF8E1;
  --color-carryover-header-text: #B45309;
  --color-blockers-bg: #FFF5F5;
  --color-blockers-bg-current: #FFE4E4;
  --color-blockers-accent: #EA4335;
  --color-blockers-header-bg: #FEF2F2;
  --color-blockers-header-text: #991B1B;
  --color-current-month-header-bg: #FFF0D0;
  --color-current-month-header-text: #C07700;
  --color-now-line: #EA4335;
  --color-poc-milestone: #F4B400;
  --color-prod-milestone: #34A853;
  --color-checkpoint: #999;
  --color-link: #0078D4;
  --color-grid-border: #E0E0E0;
  --color-grid-header-bg: #F5F5F5;
  --color-grid-header-border: #CCC;
}
```

---

## UI Interaction Scenarios

**Scenario 1: Initial Page Load (Authenticated User)**
User navigates to the dashboard URL. MSAL detects a valid Entra ID session and acquires a token silently. A loading skeleton matching the three-section layout (header, timeline placeholder, heatmap grid outline) appears for <2 seconds. The dashboard renders with live ADO data. The "Last updated" timestamp in the footer shows the current time.

**Scenario 2: Initial Page Load (Unauthenticated User)**
User navigates to the dashboard URL without a valid session. MSAL redirects the user to the Microsoft Entra ID login page. After successful authentication, the user is redirected back and sees the fully rendered dashboard.

**Scenario 3: User Views the Header and Orients Themselves**
User sees the title "Privacy Automation Release Roadmap" with a "→ ADO Backlog" link on the left, and a four-item legend (PoC Milestone, Production Release, Checkpoint, Now) on the right. The subtitle confirms the workstream and current month.

**Scenario 4: User Reads the Timeline to Assess Schedule**
User scans the timeline left-to-right. The three horizontal track lines (M1 blue, M2 teal, M3 gray-blue) show milestone progression. The dashed red "NOW" line bisects the timeline at the current date. Milestones to the left of NOW are completed or past; milestones to the right are upcoming. Diamond shapes with drop shadows draw the eye to key PoC and Production dates.

**Scenario 5: User Hovers Over a Milestone Diamond and Sees a Tooltip**
User hovers over a gold PoC diamond on the M1 track. A tooltip appears showing "M1: Chatbot & MS Role — PoC — Mar 26, 2026". The tooltip disappears when the cursor moves away. *(Phase 2 enhancement; Phase 1 relies on static date labels.)*

**Scenario 6: User Scans the Heatmap to Find Blockers**
User looks at the bottom row (Blockers, red-tinted). The current month column is visually emphasized with a darker red background (`#FFE4E4`). Items in this cell have red bullet points. The user can quickly count blockers and read their titles.

**Scenario 7: User Identifies Carryover Trends**
User compares the Carryover row across months. Items appearing in multiple consecutive months indicate persistent carryover. The amber color scheme (`#FFFDE7` / `#FFF0B0`) makes these cells visually distinct from Shipped (green) and In Progress (blue).

**Scenario 8: User Clicks the ADO Backlog Link**
User clicks the "→ ADO Backlog" link in the header. A new browser tab opens to the Azure DevOps backlog view for the Privacy Automation workstream.

**Scenario 9: User Returns to Dashboard Tab After 10 Minutes**
User switches back to the dashboard tab. React Query detects the window focus event and checks that data is stale (>5 minutes old). A background refetch occurs. The dashboard updates in place with fresh data. The "Last updated" timestamp changes.

**Scenario 10: ADO API Is Unavailable**
The Azure Function API call to ADO fails (500 or timeout). If cached data exists in memory, it is displayed with a yellow warning banner: "⚠ Showing cached data from [timestamp]. Live data unavailable." A "Retry" button is available. If no cached data exists, a full-page error state is shown: "Unable to load roadmap data. Please try again." with a "Retry" button.

**Scenario 11: Dashboard Renders with No Items in a Status Row**
A status row (e.g., Blockers) has zero items for a given month. The corresponding cell displays "—" in muted gray (`#AAA`), maintaining the grid structure without leaving blank cells.

**Scenario 12: Dashboard Renders with Many Items in a Cell**
A cell contains more work items than can fit in the visible area. The cell has `overflow: hidden`, clipping excess items. *(Phase 2 consideration: add a "+N more" indicator or expand-on-click.)*

**Scenario 13: Current Month Column Highlighting**
The heatmap automatically determines the current month and applies the highlighted column header style (`#FFF0D0` background, `#C07700` text, "◀ Now" label) and darker cell backgrounds for all four status rows in that column.

---

## Scope

### In Scope

- Single-page React SPA rendering the Privacy Automation Release Roadmap at 1920×1080
- SVG-based timeline with three workstream tracks (M1, M2, M3), milestone markers (PoC, Production, Checkpoint), month gridlines, and a dynamic "NOW" line
- CSS Grid heatmap with 4 status rows (Shipped, In Progress, Carryover, Blockers) × 4 month columns, color-coded per the design spec
- Header with title, ADO Backlog link, subtitle, and legend
- Node.js Azure Function API layer proxying WIQL queries to Azure DevOps
- Microsoft Entra ID (AAD) authentication via MSAL
- React Query data caching with 5-minute staleTime and window-focus refetch
- Loading skeleton, error state, and "last updated" timestamp
- Playwright visual regression tests against `OriginalDesignConcept.png` at 1920×1080
- GitHub Actions CI pipeline (lint, type-check, unit tests, visual regression)
- Deployment to Azure Static Web Apps
- Content Security Policy headers via `staticwebapp.config.json`
- CSS custom properties for all color tokens

### Out of Scope

- **Mobile / responsive layout** — Desktop-only (1920×1080). No tablet or phone support.
- **Dark mode** — CSS custom properties enable future support, but it is not included in this release.
- **PDF / PNG export** — No export functionality. Stakeholders can use browser screenshot tools.
- **Multi-workstream parameterization** — Dashboard is hardcoded to Privacy Automation. Other workstreams require a future initiative.
- **Write operations to ADO** — Dashboard is strictly read-only. No creating, editing, or transitioning work items.
- **Role-based access control** — Binary auth only: authenticated Microsoft employee = full access.
- **Custom date range selection** — The month range is derived from data, not user-configurable.
- **Real-time updates (WebSocket / SignalR)** — Polling via React Query on window focus is sufficient.
- **Offline mode / PWA** — No service worker or offline caching.
- **Internationalization (i18n)** — English only.
- **Database or persistent caching layer** — All data fetched live from ADO with in-memory caching only.

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|---|---|
| **Time to Interactive (TTI)** | < 3 seconds on corporate network (after auth) |
| **API response time (ADO proxy)** | < 2 seconds P95 |
| **Largest Contentful Paint (LCP)** | < 2.5 seconds |
| **Bundle size (gzipped)** | < 200 KB (excluding D3 tree-shaken modules) |
| **Data freshness** | ≤ 5 minutes stale |

### Security

- All traffic over HTTPS (enforced by Azure Static Web Apps).
- Microsoft Entra ID authentication required; no anonymous access.
- Bearer token validated server-side in Azure Function before proxying to ADO.
- On-behalf-of (OBO) flow for ADO API calls — user's own permissions apply.
- Content Security Policy headers restricting script sources, frame ancestors, and connect sources.
- No data persisted at rest — all data fetched live from ADO.
- No secrets in client-side code; all ADO credentials managed via Azure Function environment variables.

### Scalability

- Target: < 500 concurrent users.
- Azure Static Web Apps + Azure Functions consumption plan scales automatically.
- React Query client-side caching reduces redundant API calls.
- Server-side caching in Azure Functions (in-memory, 5-min TTL) reduces ADO API load.

### Reliability

- **Availability target:** 99.5% (aligned with Azure Static Web Apps SLA).
- **Graceful degradation:** If ADO API is unavailable, display last cached data with a warning banner.
- **localStorage fallback:** Cache last successful response in localStorage for cold-start resilience.
- **Monitoring:** Azure Application Insights tracks API latency, error rates, and page load performance.

### Browser Support

- Microsoft Edge (latest 2 versions) — primary
- Google Chrome (latest 2 versions) — secondary
- No IE11 support required.

---

## Success Metrics

| Metric | Target | Measurement Method |
|---|---|---|
| **Visual fidelity** | Playwright screenshot diff ≤ 0.1% against `OriginalDesignConcept.png` | CI pipeline visual regression test |
| **Adoption** | ≥ 20 unique authenticated users within 30 days of launch | Application Insights unique user count |
| **Manual reporting hours saved** | ≥ 4 hours/month reduction in PM roadmap slide preparation | PM self-reported time tracking |
| **Data accuracy** | 100% of Shipped/In Progress/Carryover/Blocker items match ADO backlog state | Manual audit comparing dashboard to ADO query results |
| **Page load performance** | TTI < 3s for P90 of page loads | Application Insights performance metrics |
| **Uptime** | ≥ 99.5% availability over any 30-day window | Azure Monitor availability tests |
| **Stakeholder satisfaction** | ≥ 4/5 rating from executive stakeholders in post-launch survey | Survey administered 2 weeks post-launch |

---

## Constraints & Assumptions

### Technical Constraints

- **Stack is non-negotiable:** React + TypeScript + Node.js. No alternatives will be considered.
- **Hosting:** Must deploy to Azure Static Web Apps (organizational standard for internal tools).
- **Authentication:** Must use Microsoft Entra ID (AAD). No other identity providers.
- **Data source:** Azure DevOps REST API is the sole data source. No custom databases for MVP.
- **Resolution:** Dashboard is designed for 1920×1080 only. Responsive behavior is not required.
- **Font:** Segoe UI must be the primary font. It is pre-installed on all corporate Windows machines; web font fallback to Arial for non-Windows clients.

### Timeline Assumptions

- **Phase 1 (Static MVP):** Weeks 1–2. Pixel-perfect static dashboard with hardcoded data.
- **Phase 2 (Live Data):** Weeks 3–4. ADO integration, MSAL auth, React Query caching.
- **Phase 3 (Polish & CI):** Week 5. Error states, monitoring, CI pipeline, documentation.
- **Total timeline:** 5 weeks from kickoff to production deployment.

### Dependency Assumptions

- The ADO backlog for Privacy Automation is well-structured with consistent area paths and iteration paths that can be queried via WIQL.
- The Entra ID app registration (client ID, tenant ID, API permissions) will be provisioned by the identity team within Week 1.
- The Azure Static Web Apps resource will be provisioned in the team's Azure subscription by the infrastructure team within Week 1.
- Milestone definitions (M1, M2, M3) and their associated dates are maintained manually in ADO as tagged work items or a dedicated query. The PM team owns this data.
- The ADO API rate limits (per-user, per-org) are sufficient for <500 users with 5-minute caching. If throttling occurs, server-side caching in Azure Functions will be added.
- Stakeholders accept a desktop-only, 1920×1080 experience. No requests for mobile or tablet support are expected during the initial release.
- The four-status-row model (Shipped, In Progress, Carryover, Blockers) is complete. No additional status categories are needed for MVP.

### Open Questions (Requiring PM Decision)

1. Which ADO project, area path, and iteration path should the WIQL query target?
2. Should the dashboard auto-refresh on a timer interval, or only on window focus?
3. Is the 4-month heatmap window always "current month ± context," or should it be configurable?
4. Are M1/M2/M3 milestones manually curated or derived from ADO tags?
5. Will multi-workstream support be needed in the next 6 months?
6. Do stakeholders need PDF/PNG export for slide decks?