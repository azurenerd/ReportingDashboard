# Research

_No research has been documented yet._

## Research technology stack for ReportingDashboard

_Researched on 2026-05-01 08:25 UTC_

### Summary

The ReportingDashboard is an internal reporting tool that visualizes a **Privacy Automation Release Roadmap** — combining a Gantt-style timeline with milestone markers and a heatmap grid showing work item status (Shipped, In Progress, Carryover, Blockers) across monthly columns. The target resolution is 1920×1080, designed for executive stakeholders and program managers. **Primary recommendation:** Build a lightweight, statically-generated React SPA using **Vite + React 19 + TypeScript 5.7**, with **D3.js** for the SVG timeline and **CSS Grid** for the heatmap. Data should be sourced from **Azure DevOps REST API** via a thin Node.js backend (Express). Deploy as an **Azure Static Web App** with an Azure Functions API layer. This is a read-heavy, low-write dashboard — keep the architecture simple and avoid over-engineering with heavy state management or databases. ---

### Key Findings

- The design is a fixed-layout, data-dense dashboard with two primary visualizations: an SVG timeline/Gantt and a CSS Grid heatmap — neither requires a heavyweight charting library.
- The color palette, typography (Segoe UI), and grid structure are precisely defined in the HTML reference; pixel-fidelity to this spec is the primary UI goal.
- Data source is Azure DevOps (ADO) backlog items, making the ADO REST API the natural data layer — no custom database is needed for MVP.
- The dashboard is read-only and internal-facing, dramatically simplifying auth (AAD/Entra ID), security, and scalability concerns.
- The 5-column CSS Grid layout (`160px repeat(4, 1fr)`) with 4 status rows maps directly to a component architecture: `<HeatmapGrid>`, `<TimelineArea>`, `<Header>`.
- SVG milestone shapes (diamonds for PoC/Production, circles for checkpoints) with drop shadows require either hand-crafted SVG or D3 — a charting library like Recharts would fight this custom design more than help.
- The "Now" line (dashed red vertical rule at current date) and dynamic month positioning require date-math integration into the SVG coordinate system.
- At internal-tool scale (<100 concurrent users), infrastructure costs are negligible — Azure Static Web Apps free tier is sufficient. --- **Goal:** Pixel-perfect dashboard with hardcoded data matching the design reference.
- Scaffold Vite + React + TypeScript project.
- Implement `<Header>`, `<TimelineArea>`, `<HeatmapGrid>` components.
- Use hardcoded JSON matching the design's content.
- Set up Playwright visual regression test comparing against `OriginalDesignConcept.png`.
- Deploy to Azure Static Web Apps (no auth yet).
- **Deliverable:** A screenshot-identical static dashboard. **Goal:** Connect to Azure DevOps API.
- Build Node.js Azure Function to proxy ADO WIQL queries.
- Define TypeScript types for `RoadmapData` and transform ADO responses.
- Integrate React Query for data fetching with 5-min cache.
- Add MSAL authentication (Entra ID).
- **Deliverable:** Live dashboard showing real ADO backlog data. **Goal:** Production-ready internal tool.
- Add error states, loading skeletons, "last updated" timestamp.
- Configure CSP headers, Application Insights.
- Set up GitHub Actions CI: lint, type-check, unit tests, Playwright visual regression.
- Document WIQL query structure and how to add new milestones/status items.
- **Deliverable:** Production deployment with CI/CD pipeline.
- **Playwright screenshot test** — set this up in Phase 1. It becomes the source of truth for visual correctness and catches regressions immediately.
- **CSS custom properties for colors** — makes future theme adjustments (dark mode, different workstreams) trivial.
- **`staticwebapp.config.json` with auth rules** — one file locks down the entire app to authenticated users.
- **ADO WIQL queries** — prototype the exact queries in ADO's web query editor before writing any code. Validate that the data shape supports the dashboard's needs.
- **SVG timeline date-to-pixel mapping** — build a standalone CodeSandbox with D3 scales to validate month gridline spacing and milestone positioning before integrating into the full app.

### Recommended Tools & Technologies

- **Date:** May 1, 2026 **Project:** Privacy Automation Release Roadmap — Reporting Dashboard **Stack Constraint:** React · TypeScript · Node.js (decided, non-negotiable) --- | Layer | Library | Version | Rationale | |---|---|---|---| | **Framework** | React | 19.1.x | Stable, team standard. Use functional components + hooks exclusively. | | **Language** | TypeScript | 5.7.x | Strict mode enabled. Use `satisfies` operator for type-safe config objects. | | **Build Tool** | Vite | 6.x | Fast HMR, native TS/TSX support, superior DX over CRA (which is deprecated). | | **SVG/Timeline** | D3.js (`d3-scale`, `d3-shape`, `d3-time`) | 7.9.x | Use only the scale/time modules — not the full D3 DOM manipulation. Render via React JSX, use D3 only for math. | | **CSS Approach** | CSS Modules + vanilla CSS | — | The design uses CSS Grid and Flexbox extensively. CSS Modules provide scoping without runtime cost. No need for Tailwind or CSS-in-JS for a fixed-layout dashboard. | | **State Management** | React Context + `useReducer` | built-in | Dashboard is read-only with a single data fetch. No need for Redux, Zustand, or Jotai. | | **Data Fetching** | TanStack Query (React Query) | 5.x | Handles caching, refetching, loading/error states for ADO API calls. | | **Date Handling** | date-fns | 4.x | Tree-shakeable, immutable. Needed for month arithmetic, timeline positioning. | | **Routing** | None (single page) | — | Dashboard is a single view. If multi-page needed later, add React Router 7.x. |
- **Recharts / Nivo / Victory** — These charting libraries impose their own visual language. The design requires custom SVG shapes (rotated diamonds, dashed lines, drop shadows) that are easier to build directly in JSX + D3 scales than to coerce from a charting library.
- **Tailwind CSS** — Adds build complexity and a learning curve for a team that may not use it. The design has ~30 unique style rules; CSS Modules are sufficient.
- **styled-components / Emotion** — Runtime CSS-in-JS adds bundle weight for no benefit here. | Layer | Library | Version | Rationale | |---|---|---|---| | **Runtime** | Node.js | 22 LTS | Long-term support through April 2027. | | **API Framework** | Express | 5.x (or Azure Functions HTTP triggers) | Thin proxy layer to ADO API. Express if self-hosted; Azure Functions if serverless. | | **ADO Client** | `azure-devops-node-api` | 14.x | Official Microsoft SDK for Azure DevOps REST API. Typed. | | **Validation** | Zod | 3.x | Runtime schema validation for API responses from ADO. | | **HTTP Client** | `undici` (built-in Node 22 fetch) | built-in | Native fetch in Node 22. No need for axios. | **Recommendation: None for MVP.** The dashboard reads from Azure DevOps in real-time (or with a short TTL cache). If performance or offline requirements emerge:
- **Phase 2 option:** Azure Cosmos DB (serverless tier) or Azure Table Storage for caching ADO query results with a 5-minute TTL.
- **Phase 2 option:** SQLite via `better-sqlite3` if a local dev cache is needed. | Concern | Tool | Notes | |---|---|---| | **Hosting (frontend)** | Azure Static Web Apps | Free tier. Auto-deploys from GitHub. Built-in staging environments. | | **Hosting (API)** | Azure Functions (Node.js 22) | Bundled with Static Web Apps. Consumption plan = pay-per-execution. | | **CI/CD** | GitHub Actions | `.github/workflows/azure-static-web-apps.yml` — auto-generated by Azure. | | **CDN** | Azure Front Door (built into SWA) | Global edge caching, HTTPS. | | **Monitoring** | Azure Application Insights | Free tier sufficient. Track API latency, errors, page load. | | Layer | Tool | Version | Notes | |---|---|---|---| | **Unit Tests** | Vitest | 3.x | Vite-native, Jest-compatible API. | | **Component Tests** | React Testing Library | 16.x | DOM-based testing, avoids implementation details. | | **E2E / Visual Regression** | Playwright | 1.50.x | Pixel-comparison screenshots against the 1920×1080 reference. Critical for this project. | | **Linting** | ESLint + `@typescript-eslint` | 9.x + 8.x | Flat config format. | | **Formatting** | Prettier | 3.x | Opinionated, zero-config. | ---
```
<App>
  <Header>                    — Title, subtitle, legend icons
  <TimelineArea>
    <TimelineSidebar>         — M1/M2/M3 milestone labels
    <TimelineSVG>             — SVG: month gridlines, track lines, milestone shapes, "NOW" marker
  <HeatmapSection>
    <HeatmapTitle>            — "DELIVERY STATUS" label
    <HeatmapGrid>             — CSS Grid container
      <CornerCell>
      <ColumnHeader month />  — Jan, Feb, Mar, Apr
      <RowHeader status />    — SHIPPED, IN PROGRESS, CARRYOVER, BLOCKERS
      <DataCell items[] status month /> — Work items with colored bullets
```
```
ADO REST API  →  Node.js API (proxy + transform)  →  React Query cache  →  Components
```
- **Node.js API layer** calls ADO with a WIQL query, transforms raw work items into a typed `RoadmapData` shape (milestones[], statusRows[]).
- **React Query** caches with a 5-minute `staleTime`. Dashboard auto-refreshes on window focus.
- **Components** receive typed props — no raw API shapes leak into the UI layer.
```typescript
interface RoadmapData {
  milestones: Milestone[];
  statusRows: StatusRow[];
  currentDate: string; // ISO date for "NOW" line
  months: MonthColumn[];
}

interface Milestone {
  id: string;
  label: string;
  track: 'M1' | 'M2' | 'M3';
  date: string;
  type: 'poc' | 'production' | 'checkpoint';
}

interface StatusRow {
  category: 'shipped' | 'inProgress' | 'carryover' | 'blockers';
  items: Record<string, WorkItem[]>; // keyed by month
}

interface WorkItem {
  id: number;
  title: string;
  adoUrl: string;
}
``` The heatmap maps directly to CSS Grid:
```css
.heatmap-grid {
  display: grid;
  grid-template-columns: 160px repeat(4, 1fr);
  grid-template-rows: 36px repeat(4, 1fr);
}
``` Each status category (Shipped, In Progress, Carryover, Blockers) gets a CSS class with its background/text colors matching the design spec exactly. Use CSS custom properties for the color tokens:
```css
:root {
  --color-shipped-bg: #F0FBF0;
  --color-shipped-accent: #34A853;
  --color-shipped-header-bg: #E8F5E9;
  --color-progress-bg: #EEF4FE;
  --color-progress-accent: #0078D4;
  /* ... etc */
}
```
- Use `d3-time` and `d3-scale` to create a `scaleTime` mapping date range → x-pixel coordinates.
- Render all SVG elements as React JSX (`<line>`, `<circle>`, `<polygon>`, `<text>`), not via D3 DOM manipulation.
- Milestone diamonds: `<polygon points="..." />` with a `transform` or calculated points from the scale.
- Drop shadows: SVG `<filter>` with `<feDropShadow>` (already defined in the reference HTML).
- "NOW" line: `<line>` at `scale(new Date())` with `stroke-dasharray="5,3"`. ---

### Considerations & Risks

- **Microsoft Entra ID (AAD)** via `@azure/msal-react` (v2.x) and `@azure/msal-browser` (v4.x).
- Use the **MSAL React wrapper** with `MsalProvider` at app root.
- Acquire tokens silently for ADO API scope (`499b84ac-1321-427f-aa17-267ca6975798/.default`).
- The Node.js API layer validates the bearer token using `@azure/identity` and forwards an on-behalf-of token to ADO.
- Read-only dashboard — no write operations. Authorization is binary: authenticated Microsoft employee = access.
- If team-scoping is needed later, use AAD security groups.
- All data originates from Azure DevOps — no PII beyond employee names on work items.
- HTTPS enforced by Azure Static Web Apps.
- No data stored at rest in MVP (all fetched live from ADO).
- Content Security Policy headers configured in `staticwebapp.config.json`.
```
GitHub repo  →  GitHub Actions  →  Azure Static Web Apps
                                     ├── Frontend (global CDN)
                                     └── /api/* → Azure Functions (Node.js)
                                                    └── Azure DevOps REST API
``` | Scale | Frontend | API | Monitoring | Total/month | |---|---|---|---|---| | **Small** (<50 users, <10K req/mo) | Free (SWA free tier) | Free (Functions consumption, 1M free req) | Free (App Insights 5GB) | **$0** | | **Medium** (<500 users, <100K req/mo) | Free | ~$5 | Free | **~$5** | This is an internal dashboard — costs are negligible. --- | Risk | Likelihood | Impact | Mitigation | |---|---|---|---| | **ADO API rate limits** | Medium | High — dashboard becomes unresponsive | Cache aggressively with React Query (5-min staleTime). Implement server-side caching in Azure Functions. | | **ADO WIQL query complexity** | Medium | Medium — wrong data displayed | Define and test WIQL queries early. Version them as constants in the codebase. | | **SVG rendering inconsistencies** | Low | Medium — visual mismatch across browsers | Playwright visual regression tests against the reference screenshot at 1920×1080. | | **Pixel-fidelity to design spec** | Medium | High — stakeholder rejection | Use Playwright screenshot comparison in CI. Set a tight diff threshold (0.1%). | | **MSAL token acquisition failures** | Low | High — app unusable | Implement silent token fallback → redirect flow. Handle `InteractionRequiredAuthError`. |
- **D3 scales + raw SVG over charting library** — More initial code, but pixel-perfect control over the custom design. A charting library would fight the spec.
- **No database** — Simpler architecture, but every page load hits ADO. Acceptable for <500 users with 5-min caching.
- **CSS Modules over Tailwind** — Less "utility-first" flexibility, but the design is fixed-layout with few responsive breakpoints. CSS Modules are sufficient and avoid toolchain complexity.
- **No SSR/Next.js** — This is an internal SPA behind auth. SSR adds complexity with no SEO or first-paint benefit for authenticated users.
- **Single point of failure:** Azure DevOps API availability. If ADO is down, the dashboard shows stale or no data.
- **Mitigation:** Display "last updated" timestamp. Cache last successful response in localStorage as a fallback. ---
- **ADO Query Definition:** Which ADO project, area path, and iteration path should the WIQL query target? Who maintains the backlog structure?
- **Refresh Frequency:** Should the dashboard auto-refresh on an interval, or only on manual reload / window focus?
- **Month Range:** Is the 4-column month window always "current month ± context," or is it configurable? The design shows Jan–Apr but the timeline shows Jan–Jun.
- **Multi-Workstream Support:** Will this dashboard serve only "Privacy Automation," or should it be parameterized for multiple workstreams?
- **Milestone Definitions:** Are M1/M2/M3 milestones manually curated, or derived from ADO data (e.g., tagged work items)?
- **Access Control:** Is any role-based access needed (e.g., some users see only certain rows), or is it all-or-nothing?
- **Export/Share:** Do stakeholders need PDF/PNG export of the dashboard for slide decks?
- **Mobile Support:** The design targets 1920×1080. Is any responsive/tablet support needed, or is desktop-only acceptable? ---

### Detailed Analysis

# Research: Technology Stack for ReportingDashboard

**Date:** May 1, 2026
**Project:** Privacy Automation Release Roadmap — Reporting Dashboard
**Stack Constraint:** React · TypeScript · Node.js (decided, non-negotiable)

---

## 1. Executive Summary

The ReportingDashboard is an internal reporting tool that visualizes a **Privacy Automation Release Roadmap** — combining a Gantt-style timeline with milestone markers and a heatmap grid showing work item status (Shipped, In Progress, Carryover, Blockers) across monthly columns. The target resolution is 1920×1080, designed for executive stakeholders and program managers.

**Primary recommendation:** Build a lightweight, statically-generated React SPA using **Vite + React 19 + TypeScript 5.7**, with **D3.js** for the SVG timeline and **CSS Grid** for the heatmap. Data should be sourced from **Azure DevOps REST API** via a thin Node.js backend (Express). Deploy as an **Azure Static Web App** with an Azure Functions API layer. This is a read-heavy, low-write dashboard — keep the architecture simple and avoid over-engineering with heavy state management or databases.

---

## 2. Key Findings

- The design is a fixed-layout, data-dense dashboard with two primary visualizations: an SVG timeline/Gantt and a CSS Grid heatmap — neither requires a heavyweight charting library.
- The color palette, typography (Segoe UI), and grid structure are precisely defined in the HTML reference; pixel-fidelity to this spec is the primary UI goal.
- Data source is Azure DevOps (ADO) backlog items, making the ADO REST API the natural data layer — no custom database is needed for MVP.
- The dashboard is read-only and internal-facing, dramatically simplifying auth (AAD/Entra ID), security, and scalability concerns.
- The 5-column CSS Grid layout (`160px repeat(4, 1fr)`) with 4 status rows maps directly to a component architecture: `<HeatmapGrid>`, `<TimelineArea>`, `<Header>`.
- SVG milestone shapes (diamonds for PoC/Production, circles for checkpoints) with drop shadows require either hand-crafted SVG or D3 — a charting library like Recharts would fight this custom design more than help.
- The "Now" line (dashed red vertical rule at current date) and dynamic month positioning require date-math integration into the SVG coordinate system.
- At internal-tool scale (<100 concurrent users), infrastructure costs are negligible — Azure Static Web Apps free tier is sufficient.

---

## 3. Recommended Technology Stack

### Frontend

| Layer | Library | Version | Rationale |
|---|---|---|---|
| **Framework** | React | 19.1.x | Stable, team standard. Use functional components + hooks exclusively. |
| **Language** | TypeScript | 5.7.x | Strict mode enabled. Use `satisfies` operator for type-safe config objects. |
| **Build Tool** | Vite | 6.x | Fast HMR, native TS/TSX support, superior DX over CRA (which is deprecated). |
| **SVG/Timeline** | D3.js (`d3-scale`, `d3-shape`, `d3-time`) | 7.9.x | Use only the scale/time modules — not the full D3 DOM manipulation. Render via React JSX, use D3 only for math. |
| **CSS Approach** | CSS Modules + vanilla CSS | — | The design uses CSS Grid and Flexbox extensively. CSS Modules provide scoping without runtime cost. No need for Tailwind or CSS-in-JS for a fixed-layout dashboard. |
| **State Management** | React Context + `useReducer` | built-in | Dashboard is read-only with a single data fetch. No need for Redux, Zustand, or Jotai. |
| **Data Fetching** | TanStack Query (React Query) | 5.x | Handles caching, refetching, loading/error states for ADO API calls. |
| **Date Handling** | date-fns | 4.x | Tree-shakeable, immutable. Needed for month arithmetic, timeline positioning. |
| **Routing** | None (single page) | — | Dashboard is a single view. If multi-page needed later, add React Router 7.x. |

**Alternatives considered and rejected:**
- **Recharts / Nivo / Victory** — These charting libraries impose their own visual language. The design requires custom SVG shapes (rotated diamonds, dashed lines, drop shadows) that are easier to build directly in JSX + D3 scales than to coerce from a charting library.
- **Tailwind CSS** — Adds build complexity and a learning curve for a team that may not use it. The design has ~30 unique style rules; CSS Modules are sufficient.
- **styled-components / Emotion** — Runtime CSS-in-JS adds bundle weight for no benefit here.

### Backend

| Layer | Library | Version | Rationale |
|---|---|---|---|
| **Runtime** | Node.js | 22 LTS | Long-term support through April 2027. |
| **API Framework** | Express | 5.x (or Azure Functions HTTP triggers) | Thin proxy layer to ADO API. Express if self-hosted; Azure Functions if serverless. |
| **ADO Client** | `azure-devops-node-api` | 14.x | Official Microsoft SDK for Azure DevOps REST API. Typed. |
| **Validation** | Zod | 3.x | Runtime schema validation for API responses from ADO. |
| **HTTP Client** | `undici` (built-in Node 22 fetch) | built-in | Native fetch in Node 22. No need for axios. |

### Database

**Recommendation: None for MVP.** The dashboard reads from Azure DevOps in real-time (or with a short TTL cache). If performance or offline requirements emerge:
- **Phase 2 option:** Azure Cosmos DB (serverless tier) or Azure Table Storage for caching ADO query results with a 5-minute TTL.
- **Phase 2 option:** SQLite via `better-sqlite3` if a local dev cache is needed.

### Infrastructure

| Concern | Tool | Notes |
|---|---|---|
| **Hosting (frontend)** | Azure Static Web Apps | Free tier. Auto-deploys from GitHub. Built-in staging environments. |
| **Hosting (API)** | Azure Functions (Node.js 22) | Bundled with Static Web Apps. Consumption plan = pay-per-execution. |
| **CI/CD** | GitHub Actions | `.github/workflows/azure-static-web-apps.yml` — auto-generated by Azure. |
| **CDN** | Azure Front Door (built into SWA) | Global edge caching, HTTPS. |
| **Monitoring** | Azure Application Insights | Free tier sufficient. Track API latency, errors, page load. |

### Testing

| Layer | Tool | Version | Notes |
|---|---|---|---|
| **Unit Tests** | Vitest | 3.x | Vite-native, Jest-compatible API. |
| **Component Tests** | React Testing Library | 16.x | DOM-based testing, avoids implementation details. |
| **E2E / Visual Regression** | Playwright | 1.50.x | Pixel-comparison screenshots against the 1920×1080 reference. Critical for this project. |
| **Linting** | ESLint + `@typescript-eslint` | 9.x + 8.x | Flat config format. |
| **Formatting** | Prettier | 3.x | Opinionated, zero-config. |

---

## 4. Architecture Recommendations

### Component Architecture

```
<App>
  <Header>                    — Title, subtitle, legend icons
  <TimelineArea>
    <TimelineSidebar>         — M1/M2/M3 milestone labels
    <TimelineSVG>             — SVG: month gridlines, track lines, milestone shapes, "NOW" marker
  <HeatmapSection>
    <HeatmapTitle>            — "DELIVERY STATUS" label
    <HeatmapGrid>             — CSS Grid container
      <CornerCell>
      <ColumnHeader month />  — Jan, Feb, Mar, Apr
      <RowHeader status />    — SHIPPED, IN PROGRESS, CARRYOVER, BLOCKERS
      <DataCell items[] status month /> — Work items with colored bullets
```

### Data Flow

```
ADO REST API  →  Node.js API (proxy + transform)  →  React Query cache  →  Components
```

1. **Node.js API layer** calls ADO with a WIQL query, transforms raw work items into a typed `RoadmapData` shape (milestones[], statusRows[]).
2. **React Query** caches with a 5-minute `staleTime`. Dashboard auto-refreshes on window focus.
3. **Components** receive typed props — no raw API shapes leak into the UI layer.

### Key TypeScript Types

```typescript
interface RoadmapData {
  milestones: Milestone[];
  statusRows: StatusRow[];
  currentDate: string; // ISO date for "NOW" line
  months: MonthColumn[];
}

interface Milestone {
  id: string;
  label: string;
  track: 'M1' | 'M2' | 'M3';
  date: string;
  type: 'poc' | 'production' | 'checkpoint';
}

interface StatusRow {
  category: 'shipped' | 'inProgress' | 'carryover' | 'blockers';
  items: Record<string, WorkItem[]>; // keyed by month
}

interface WorkItem {
  id: number;
  title: string;
  adoUrl: string;
}
```

### CSS Grid Strategy

The heatmap maps directly to CSS Grid:
```css
.heatmap-grid {
  display: grid;
  grid-template-columns: 160px repeat(4, 1fr);
  grid-template-rows: 36px repeat(4, 1fr);
}
```

Each status category (Shipped, In Progress, Carryover, Blockers) gets a CSS class with its background/text colors matching the design spec exactly. Use CSS custom properties for the color tokens:

```css
:root {
  --color-shipped-bg: #F0FBF0;
  --color-shipped-accent: #34A853;
  --color-shipped-header-bg: #E8F5E9;
  --color-progress-bg: #EEF4FE;
  --color-progress-accent: #0078D4;
  /* ... etc */
}
```

### SVG Timeline Strategy

- Use `d3-time` and `d3-scale` to create a `scaleTime` mapping date range → x-pixel coordinates.
- Render all SVG elements as React JSX (`<line>`, `<circle>`, `<polygon>`, `<text>`), not via D3 DOM manipulation.
- Milestone diamonds: `<polygon points="..." />` with a `transform` or calculated points from the scale.
- Drop shadows: SVG `<filter>` with `<feDropShadow>` (already defined in the reference HTML).
- "NOW" line: `<line>` at `scale(new Date())` with `stroke-dasharray="5,3"`.

---

## 5. Security & Infrastructure

### Authentication

- **Microsoft Entra ID (AAD)** via `@azure/msal-react` (v2.x) and `@azure/msal-browser` (v4.x).
- Use the **MSAL React wrapper** with `MsalProvider` at app root.
- Acquire tokens silently for ADO API scope (`499b84ac-1321-427f-aa17-267ca6975798/.default`).
- The Node.js API layer validates the bearer token using `@azure/identity` and forwards an on-behalf-of token to ADO.

### Authorization

- Read-only dashboard — no write operations. Authorization is binary: authenticated Microsoft employee = access.
- If team-scoping is needed later, use AAD security groups.

### Data Protection

- All data originates from Azure DevOps — no PII beyond employee names on work items.
- HTTPS enforced by Azure Static Web Apps.
- No data stored at rest in MVP (all fetched live from ADO).
- Content Security Policy headers configured in `staticwebapp.config.json`.

### Deployment Architecture

```
GitHub repo  →  GitHub Actions  →  Azure Static Web Apps
                                     ├── Frontend (global CDN)
                                     └── /api/* → Azure Functions (Node.js)
                                                    └── Azure DevOps REST API
```

### Cost Estimate

| Scale | Frontend | API | Monitoring | Total/month |
|---|---|---|---|---|
| **Small** (<50 users, <10K req/mo) | Free (SWA free tier) | Free (Functions consumption, 1M free req) | Free (App Insights 5GB) | **$0** |
| **Medium** (<500 users, <100K req/mo) | Free | ~$5 | Free | **~$5** |

This is an internal dashboard — costs are negligible.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| **ADO API rate limits** | Medium | High — dashboard becomes unresponsive | Cache aggressively with React Query (5-min staleTime). Implement server-side caching in Azure Functions. |
| **ADO WIQL query complexity** | Medium | Medium — wrong data displayed | Define and test WIQL queries early. Version them as constants in the codebase. |
| **SVG rendering inconsistencies** | Low | Medium — visual mismatch across browsers | Playwright visual regression tests against the reference screenshot at 1920×1080. |
| **Pixel-fidelity to design spec** | Medium | High — stakeholder rejection | Use Playwright screenshot comparison in CI. Set a tight diff threshold (0.1%). |
| **MSAL token acquisition failures** | Low | High — app unusable | Implement silent token fallback → redirect flow. Handle `InteractionRequiredAuthError`. |

### Trade-offs Made

1. **D3 scales + raw SVG over charting library** — More initial code, but pixel-perfect control over the custom design. A charting library would fight the spec.
2. **No database** — Simpler architecture, but every page load hits ADO. Acceptable for <500 users with 5-min caching.
3. **CSS Modules over Tailwind** — Less "utility-first" flexibility, but the design is fixed-layout with few responsive breakpoints. CSS Modules are sufficient and avoid toolchain complexity.
4. **No SSR/Next.js** — This is an internal SPA behind auth. SSR adds complexity with no SEO or first-paint benefit for authenticated users.

### Bottlenecks

- **Single point of failure:** Azure DevOps API availability. If ADO is down, the dashboard shows stale or no data.
- **Mitigation:** Display "last updated" timestamp. Cache last successful response in localStorage as a fallback.

---

## 7. Open Questions

1. **ADO Query Definition:** Which ADO project, area path, and iteration path should the WIQL query target? Who maintains the backlog structure?
2. **Refresh Frequency:** Should the dashboard auto-refresh on an interval, or only on manual reload / window focus?
3. **Month Range:** Is the 4-column month window always "current month ± context," or is it configurable? The design shows Jan–Apr but the timeline shows Jan–Jun.
4. **Multi-Workstream Support:** Will this dashboard serve only "Privacy Automation," or should it be parameterized for multiple workstreams?
5. **Milestone Definitions:** Are M1/M2/M3 milestones manually curated, or derived from ADO data (e.g., tagged work items)?
6. **Access Control:** Is any role-based access needed (e.g., some users see only certain rows), or is it all-or-nothing?
7. **Export/Share:** Do stakeholders need PDF/PNG export of the dashboard for slide decks?
8. **Mobile Support:** The design targets 1920×1080. Is any responsive/tablet support needed, or is desktop-only acceptable?

---

## 8. Implementation Recommendations

### Phase 1: Static MVP (Week 1–2)

**Goal:** Pixel-perfect dashboard with hardcoded data matching the design reference.

- Scaffold Vite + React + TypeScript project.
- Implement `<Header>`, `<TimelineArea>`, `<HeatmapGrid>` components.
- Use hardcoded JSON matching the design's content.
- Set up Playwright visual regression test comparing against `OriginalDesignConcept.png`.
- Deploy to Azure Static Web Apps (no auth yet).
- **Deliverable:** A screenshot-identical static dashboard.

### Phase 2: Live Data (Week 3–4)

**Goal:** Connect to Azure DevOps API.

- Build Node.js Azure Function to proxy ADO WIQL queries.
- Define TypeScript types for `RoadmapData` and transform ADO responses.
- Integrate React Query for data fetching with 5-min cache.
- Add MSAL authentication (Entra ID).
- **Deliverable:** Live dashboard showing real ADO backlog data.

### Phase 3: Polish & CI (Week 5)

**Goal:** Production-ready internal tool.

- Add error states, loading skeletons, "last updated" timestamp.
- Configure CSP headers, Application Insights.
- Set up GitHub Actions CI: lint, type-check, unit tests, Playwright visual regression.
- Document WIQL query structure and how to add new milestones/status items.
- **Deliverable:** Production deployment with CI/CD pipeline.

### Quick Wins

- **Playwright screenshot test** — set this up in Phase 1. It becomes the source of truth for visual correctness and catches regressions immediately.
- **CSS custom properties for colors** — makes future theme adjustments (dark mode, different workstreams) trivial.
- **`staticwebapp.config.json` with auth rules** — one file locks down the entire app to authenticated users.

### Prototyping Recommended

- **ADO WIQL queries** — prototype the exact queries in ADO's web query editor before writing any code. Validate that the data shape supports the dashboard's needs.
- **SVG timeline date-to-pixel mapping** — build a standalone CodeSandbox with D3 scales to validate month gridline spacing and milestone positioning before integrating into the full app.

## Visual Design References

The following design reference files were found in the repository. These MUST be used as the canonical visual specification when building UI components.

### `OriginalDesignConcept.html`

**Type:** HTML Design Template

**Layout Structure:**
- **Header section** with title, subtitle, and legend
- **Timeline/Gantt section** with SVG milestone visualization
- **Heatmap grid** — status rows × month columns, color-coded by category
  - Shipped row (green tones)
  - In Progress row (blue tones)
  - Carryover row (yellow/amber tones)
  - Blockers row (red tones)

**Key CSS Patterns:**
- Uses CSS Grid layout
- Uses Flexbox layout
- Color palette: #FFFFFF, #111, #0078D4, #888, #FAFAFA, #F5F5F5, #999, #FFF0D0, #C07700, #333, #1B7A28, #E8F5E9, #F0FBF0, #D8F2DA, #34A853
- Font: Segoe UI
- Grid columns: `160px repeat(4,1fr)`
- Designed for 1920×1080 screenshot resolution

<details><summary>Full HTML Source</summary>

```html
<!DOCTYPE html><html lang="en"><head><meta charset="UTF-8">
<style>
*{margin:0;padding:0;box-sizing:border-box;}
body{width:1920px;height:1080px;overflow:hidden;background:#FFFFFF;
     font-family:'Segoe UI',Arial,sans-serif;color:#111;display:flex;flex-direction:column;}
a{color:#0078D4;text-decoration:none;}
.hdr{padding:12px 44px 10px;border-bottom:1px solid #E0E0E0;display:flex;
      align-items:center;justify-content:space-between;flex-shrink:0;}
.hdr h1{font-size:24px;font-weight:700;}
.sub{font-size:12px;color:#888;margin-top:2px;}
.tl-area{display:flex;align-items:stretch;padding:6px 44px 0;flex-shrink:0;height:196px;
          border-bottom:2px solid #E8E8E8;background:#FAFAFA;}
.tl-svg-box{flex:1;padding-left:12px;padding-top:6px;}
/* heatmap */
.hm-wrap{flex:1;min-height:0;display:flex;flex-direction:column;padding:10px 44px 10px;}
.hm-title{font-size:14px;font-weight:700;color:#888;letter-spacing:.5px;text-transform:uppercase;margin-bottom:8px;flex-shrink:0;}
.hm-grid{flex:1;min-height:0;display:grid;
          grid-template-columns:160px repeat(4,1fr);
          grid-template-rows:36px repeat(4,1fr);
          border:1px solid #E0E0E0;}
/* header cells */
.hm-corner{background:#F5F5F5;display:flex;align-items:center;justify-content:center;
            font-size:11px;font-weight:700;color:#999;text-transform:uppercase;
            border-right:1px solid #E0E0E0;border-bottom:2px solid #CCC;}
.hm-col-hdr{display:flex;align-items:center;justify-content:center;
             font-size:16px;font-weight:700;background:#F5F5F5;
             border-right:1px solid #E0E0E0;border-bottom:2px solid #CCC;}
.hm-col-hdr.apr-hdr{background:#FFF0D0;color:#C07700;}
/* row header */
.hm-row-hdr{display:flex;align-items:center;padding:0 12px;
             font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.7px;
             border-right:2px solid #CCC;border-bottom:1px solid #E0E0E0;}
/* data cells */
.hm-cell{padding:8px 12px;border-right:1px solid #E0E0E0;border-bottom:1px solid #E0E0E0;overflow:hidden;}
.hm-cell .it{font-size:12px;color:#333;padding:2px 0 2px 12px;position:relative;line-height:1.35;}
.hm-cell .it::before{content:'';position:absolute;left:0;top:7px;width:6px;height:6px;border-radius:50%;}
/* row colors */
.ship-hdr{color:#1B7A28;background:#E8F5E9;border-right:2px solid #CCC;}
.ship-cell{background:#F0FBF0;} .ship-cell.apr{background:#D8F2DA;}
.ship-cell .it::before{background:#34A853;}
.prog-hdr{color:#1565C0;background:#E3F2FD;border-right:2px solid #CCC;}
.prog-cell{background:#EEF4FE;} .prog-cell.apr{background:#DAE8FB;}
.prog-cell .it::before{background:#0078D4;}
.carry-hdr{color:#B45309;background:#FFF8E1;border-right:2px solid #CCC;}
.carry-cell{background:#FFFDE7;} .carry-cell.apr{background:#FFF0B0;}
.carry-cell .it::before{background:#F4B400;}
.block-hdr{color:#991B1B;background:#FEF2F2;border-right:2px solid #CCC;}
.block-cell{background:#FFF5F5;} .block-cell.apr{background:#FFE4E4;}
.block-cell .it::before{background:#EA4335;}
</style></head><body>
<div class="hdr">
  <div>
    <h1>Privacy Automation Release Roadmap <a href="#">⧉ ADO Backlog</a></h1>
    <div class="sub">Trusted Platform · Privacy Automation Workstream · April 2026</div>
  </div>
  
<div style="display:flex;gap:22px;align-items:center;">
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:12px;height:12px;background:#F4B400;transform:rotate(45deg);display:inline-block;flex-shrink:0;"></span>PoC Milestone
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:12px;height:12px;background:#34A853;transform:rotate(45deg);display:inline-block;flex-shrink:0;"></span>Production Release
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:8px;height:8px;border-radius:50%;background:#999;display:inline-block;flex-shrink:0;"></span>Checkpoint
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:2px;height:14px;background:#EA4335;display:inline-block;flex-shrink:0;"></span>Now (Apr 2026)
  </span>
</div>
</div>
<div class="tl-area">
  
<div style="width:230px;flex-shrink:0;display:flex;flex-direction:column;
            justify-content:space-around;padding:16px 12px 16px 0;
            border-right:1px solid #E0E0E0;">
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#0078D4;">
    M1<br/><span style="font-weight:400;color:#444;">Chatbot &amp; MS Role</span></div>
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#00897B;">
    M2<br/><span style="font-weight:400;color:#444;">PDS &amp; Data Inventory</span></div>
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#546E7A;">
    M3<br/><span style="font-weight:400;color:#444;">Auto Review DFD</span></div>
</div>
  <div class="tl-svg-box"><svg xmlns="http://www.w3.org/2000/svg" width="1560" height="185" style="overflow:visible;display:block">
<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>
<line x1="0" y1="0" x2="0" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="5" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Jan</text>
<line x1="260" y1="0" x2="260" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="265" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Feb</text>
<line x1="520" y1="0" x2="520" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="525" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Mar</text>
<line x1="780" y1="0" x2="780" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="785" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Apr</text>
<line x1="1040" y1="0" x2="1040" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="1045" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">May</text>
<line x1="1300" y1="0" x2="1300" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="1305" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Jun</text>
<line x1="823" y1="0" x2="823" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
<text x="827" y="14" fill="#EA4335" font-size="10" font-weight="700" font-family="Segoe UI,Arial">NOW</text>
<line x1="0" y1="42" x2="1560" y2="42" stroke="#0078D4" stroke-width="3"/>
<circle cx="104" cy="42" r="7" fill="white" stroke="#0078D4" stroke-width="2.5"/>
<text x="104" y="26" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Jan 12</text>
<polygon points="745,31 756,42 745,53 734,42" fill="#F4B400" filter="url(#sh)"/><text x="745" y="66" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Mar 26 PoC</text>
<polygon points="1040,31 1051,42 1040,53 1029,42" fill="#34A853" filter="url(#sh)"/><text x="1040" y="18" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Apr Prod (TBD)</text>
<line x1="0" y1="98" x2="1560" y2="98" stroke="#00897B" stroke-width="3"/>
<circle cx="0" cy="98" r="7" fill="white" stroke="#00897B" stroke-width="2.5"/>
<text x="10" y="82" fill="#666" font-size="10" font-family="Segoe UI,Arial">Dec 19</text>
<circle cx="355" cy="98" r="5" fill="white" stroke="#888" stroke-width="2.5"/>
<text x="355" y="82" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Feb 11</text>
<circle cx="546" cy="98" r="4" fill="#999"/>
<circle cx="607" cy="98" r="4" fill="#999"/>
<circle cx="650" cy="98" r="4" fill="#999"/>
<circle cx="667" cy="98" r="4" fill="#999"/>
<polygon points="693,87 704,98 693,109 682,98" fill="#F4B400" filter="url(#sh)"/><text x="693" y="74" text-anchor="middle" fill="#666" font-size="10" font-family="Seg
<!-- truncated -->
```
</details>


## Design Visual Previews

The following screenshots were rendered from the HTML design reference files. Engineers MUST match these visuals exactly.

### OriginalDesignConcept.html

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/0d78f5db4ce30301ffc0a90e9a3fbe7f8df5ee3d/AgentDocs/testbranch/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
