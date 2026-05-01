# Architecture

**Project:** ReportingDashboard — Privacy Automation Release Roadmap
**Last Updated:** May 1, 2026
**Stack:** React 19 · TypeScript 5.7 · Node.js 22 · Azure Static Web Apps

---

## Overview & Goals

The ReportingDashboard is a single-page, read-only React application that renders the Privacy Automation Release Roadmap at a fixed 1920×1080 resolution. It combines an SVG Gantt-style timeline (three workstream tracks with milestone markers) and a CSS Grid heatmap (four status rows × four month columns) into one screen, sourced live from Azure DevOps via a Node.js Azure Function proxy.

**Architecture Goals:**

1. **Pixel fidelity** — Match `OriginalDesignConcept.png` within ≤0.1% Playwright screenshot diff.
2. **Live data** — Surface ADO backlog state with ≤5-minute staleness, zero manual effort.
3. **Simplicity** — No database, no SSR, no routing. A thin API proxy + a single-page SPA.
4. **Security** — Microsoft Entra ID authentication; OBO token flow to ADO; no secrets in client code.
5. **Low cost** — Azure Static Web Apps free tier; Azure Functions consumption plan. Target $0–$5/month.

---

## System Components

### 1. Frontend SPA (`/src`)

| Attribute | Detail |
|---|---|
| **Responsibility** | Render the dashboard UI; manage auth session; fetch and cache data; display loading/error states |
| **Runtime** | Browser (Edge/Chrome, 1920×1080) |
| **Framework** | React 19.1 + TypeScript 5.7, built with Vite 6 |
| **Key Libraries** | `@azure/msal-react` (auth), `@tanstack/react-query` (data fetching), `d3-scale`/`d3-time` (SVG math), `date-fns` (date arithmetic) |
| **Output** | Static assets (HTML, JS, CSS) deployed to Azure Static Web Apps CDN |

### 2. API Proxy (`/api`)

| Attribute | Detail |
|---|---|
| **Responsibility** | Authenticate inbound bearer tokens; execute WIQL queries against ADO; transform raw work items into `RoadmapData`; apply server-side in-memory cache (5-min TTL) |
| **Runtime** | Node.js 22 LTS on Azure Functions (consumption plan, bundled with SWA) |
| **Key Libraries** | `azure-devops-node-api` (ADO client), `@azure/identity` (OBO token exchange), `zod` (response validation) |
| **Endpoints** | Single endpoint: `GET /api/roadmap` |

### 3. Azure DevOps (External)

| Attribute | Detail |
|---|---|
| **Responsibility** | Source of truth for work items, milestones, and status |
| **Interface** | REST API + WIQL query language |
| **Owned by** | PM team (backlog structure, area paths, iteration paths, milestone tags) |

### 4. Microsoft Entra ID (External)

| Attribute | Detail |
|---|---|
| **Responsibility** | Identity provider; issues OAuth 2.0 tokens for SPA and API |
| **Interface** | MSAL redirect/silent flows (SPA); OBO token exchange (API) |

### 5. Azure Application Insights (External)

| Attribute | Detail |
|---|---|
| **Responsibility** | Telemetry collection — page load times, API latency, error rates, unique user counts |
| **Interface** | JavaScript SDK (frontend), Node.js SDK (API) |

---

## Component Interactions

### Data Flow

```
┌──────────┐     HTTPS/Bearer      ┌──────────────────┐      OBO Token      ┌─────────────┐
│  Browser  │ ──────────────────▶  │  Azure Function   │ ──────────────────▶ │  Azure DevOps│
│  (React)  │ ◀────────────────── │  GET /api/roadmap  │ ◀────────────────── │  REST API    │
└──────────┘    RoadmapData JSON   └──────────────────┘    WIQL Results      └─────────────┘
      │                                    │
      │  MSAL silent/redirect              │  @azure/identity OBO
      ▼                                    ▼
┌──────────────┐                  ┌──────────────────┐
│  Entra ID    │                  │  Entra ID        │
│  (login)     │                  │  (token exchange) │
└──────────────┘                  └──────────────────┘
```

### Sequence: Authenticated Page Load

1. User navigates to dashboard URL.
2. MSAL checks for cached token. If missing → redirect to Entra ID login → redirect back.
3. MSAL acquires access token silently (scope: custom API).
4. React Query calls `GET /api/roadmap` with `Authorization: Bearer <token>`.
5. Azure Function validates token, performs OBO exchange for ADO scope (`499b84ac-1321-427f-aa17-267ca6975798/.default`).
6. Azure Function checks in-memory cache. If fresh → return cached. Else → execute WIQL query against ADO, transform response, cache, return.
7. React Query stores `RoadmapData` with 5-min `staleTime`.
8. Components render Header, Timeline SVG, and Heatmap Grid.
9. On window re-focus after >5 min, React Query refetches in background.

### Sequence: API Failure

1. `GET /api/roadmap` returns 5xx or times out.
2. React Query checks for stale cached data in memory.
3. If stale data exists → render dashboard + yellow warning banner with timestamp + Retry button.
4. If no cached data → render full-page error state with Retry button.
5. Additionally, on first successful load, `RoadmapData` is serialized to `localStorage` as cold-start fallback. On total failure with empty React Query cache, read from `localStorage`.

---

## Data Model

### Core Types

```typescript
// ── Canonical API response shape ──

interface RoadmapData {
  milestones: Milestone[];
  statusRows: StatusRow[];
  months: MonthColumn[];
  metadata: ResponseMetadata;
}

interface ResponseMetadata {
  fetchedAt: string;          // ISO 8601 timestamp
  adoQueryId: string;         // WIQL query identifier for traceability
  cacheHit: boolean;          // whether server-side cache was used
}

interface Milestone {
  id: string;                 // ADO work item ID or manual identifier
  label: string;              // e.g. "Mar 26 PoC"
  track: 'M1' | 'M2' | 'M3';
  trackLabel: string;         // e.g. "Chatbot & MS Role"
  trackColor: string;         // e.g. "#0078D4"
  date: string;               // ISO 8601 date
  type: 'poc' | 'production' | 'checkpoint';
}

interface MonthColumn {
  key: string;                // e.g. "2026-01"
  label: string;              // e.g. "Jan"
  startDate: string;          // first day of month, ISO
  isCurrent: boolean;         // true if this is the current month
}

interface StatusRow {
  category: StatusCategory;
  items: Record<string, WorkItem[]>;  // keyed by MonthColumn.key
}

type StatusCategory = 'shipped' | 'inProgress' | 'carryover' | 'blockers';

interface WorkItem {
  id: number;                 // ADO work item ID
  title: string;              // display title (truncated to 80 chars)
  adoUrl: string;             // direct link to ADO work item
}
```

### Status Category Color Config

```typescript
interface StatusColorConfig {
  headerBg: string;
  headerText: string;
  cellBg: string;
  cellBgCurrent: string;
  accent: string;             // bullet color
}

const STATUS_COLORS: Record<StatusCategory, StatusColorConfig> = {
  shipped:     { headerBg: '#E8F5E9', headerText: '#1B7A28', cellBg: '#F0FBF0', cellBgCurrent: '#D8F2DA', accent: '#34A853' },
  inProgress:  { headerBg: '#E3F2FD', headerText: '#1565C0', cellBg: '#EEF4FE', cellBgCurrent: '#DAE8FB', accent: '#0078D4' },
  carryover:   { headerBg: '#FFF8E1', headerText: '#B45309', cellBg: '#FFFDE7', cellBgCurrent: '#FFF0B0', accent: '#F4B400' },
  blockers:    { headerBg: '#FEF2F2', headerText: '#991B1B', cellBg: '#FFF5F5', cellBgCurrent: '#FFE4E4', accent: '#EA4335' },
} as const;
```

### Storage

| Store | Purpose | TTL | Persistence |
|---|---|---|---|
| React Query in-memory cache | Primary client cache | 5 minutes (`staleTime`) | Browser session only |
| `localStorage` key `roadmap-cache` | Cold-start fallback | None (overwritten on each success) | Survives tab close |
| Azure Function in-memory `Map` | Server-side ADO cache | 5 minutes | Function instance lifetime |

**No database.** All data is fetched live from ADO. The two caching layers (client + server) reduce ADO API load.

---

## API Contracts

### `GET /api/roadmap`

**Authentication:** Required. `Authorization: Bearer <Entra ID access token>`.

**Query Parameters:** None for MVP.

**Success Response (200):**

```json
{
  "milestones": [
    {
      "id": "ms-m1-poc",
      "label": "Mar 26 PoC",
      "track": "M1",
      "trackLabel": "Chatbot & MS Role",
      "trackColor": "#0078D4",
      "date": "2026-03-26",
      "type": "poc"
    }
  ],
  "statusRows": [
    {
      "category": "shipped",
      "items": {
        "2026-01": [
          { "id": 12345, "title": "Privacy chatbot v1 deployed", "adoUrl": "https://dev.azure.com/..." }
        ],
        "2026-02": [],
        "2026-03": [],
        "2026-04": []
      }
    }
  ],
  "months": [
    { "key": "2026-01", "label": "Jan", "startDate": "2026-01-01", "isCurrent": false },
    { "key": "2026-02", "label": "Feb", "startDate": "2026-02-01", "isCurrent": false },
    { "key": "2026-03", "label": "Mar", "startDate": "2026-03-01", "isCurrent": false },
    { "key": "2026-04", "label": "Apr", "startDate": "2026-04-01", "isCurrent": true }
  ],
  "metadata": {
    "fetchedAt": "2026-04-15T10:30:00Z",
    "adoQueryId": "roadmap-main-v1",
    "cacheHit": false
  }
}
```

**Error Responses:**

| Status | Body | Condition |
|---|---|---|
| 401 | `{ "error": "Unauthorized", "message": "Invalid or expired token" }` | Missing/invalid bearer token |
| 502 | `{ "error": "ADO Unavailable", "message": "Azure DevOps API returned error", "retryAfter": 30 }` | ADO API failure |
| 429 | `{ "error": "Rate Limited", "message": "Too many requests", "retryAfter": 60 }` | ADO rate limit hit |
| 500 | `{ "error": "Internal Error", "message": "Unexpected server error" }` | Unhandled exception |

**Zod Validation:** The API validates the ADO WIQL response with Zod before transformation. Malformed ADO data returns 502 with a descriptive message rather than passing garbage to the client.

### WIQL Query (Internal)

The Azure Function executes two WIQL queries against ADO:

**Query 1 — Heatmap Work Items:**
```sql
SELECT [System.Id], [System.Title], [System.State], [System.IterationPath], [System.Tags]
FROM WorkItems
WHERE [System.AreaPath] UNDER '{AREA_PATH}'
  AND [System.IterationPath] UNDER '{ITERATION_ROOT}'
  AND [System.WorkItemType] IN ('User Story', 'Bug', 'Task')
ORDER BY [System.ChangedDate] DESC
```

**Query 2 — Milestones:**
```sql
SELECT [System.Id], [System.Title], [Microsoft.VSTS.Scheduling.TargetDate], [System.Tags]
FROM WorkItems
WHERE [System.AreaPath] UNDER '{AREA_PATH}'
  AND [System.Tags] CONTAINS 'milestone'
ORDER BY [Microsoft.VSTS.Scheduling.TargetDate] ASC
```

The transformation layer maps ADO states to status categories:
- `Closed` / `Done` → `shipped`
- `Active` / `In Progress` → `inProgress`
- Items appearing in iteration N but not closed, then appearing again in iteration N+1 → `carryover`
- Items tagged `blocked` or with `System.State = 'Blocked'` → `blockers`

Month assignment uses the work item's iteration path end date.

---

## UI Component Architecture

### Component Tree

```
<MsalProvider instance={msalInstance}>
  <QueryClientProvider client={queryClient}>
    <App>
      <AuthGuard>                          ← redirects unauthenticated users
        <DashboardPage>
          <Header />                       ← Section 1: title, subtitle, legend
          <TimelineArea>                   ← Section 2: 196px fixed height
            <TimelineSidebar />            ← 230px left panel
            <TimelineSVG />                ← flex:1 SVG canvas
          </TimelineArea>
          <HeatmapSection>                 ← Section 3: flex:1 fills remaining
            <HeatmapTitle />
            <HeatmapGrid>
              <CornerCell />
              <ColumnHeader />             ← ×4, current month highlighted
              <StatusRow>                  ← ×4 (shipped, inProgress, carryover, blockers)
                <RowHeader />
                <DataCell />               ← ×4 months per row
              </StatusRow>
            </HeatmapGrid>
          </HeatmapSection>
          <StatusBar />                    ← "Last updated" + warning banner
        </DashboardPage>
        <LoadingSkeleton />                ← shown during initial fetch
        <ErrorState />                     ← shown on total failure
      </AuthGuard>
    </App>
  </QueryClientProvider>
</MsalProvider>
```

### Visual Section → Component Mapping

| Design Section | Component | CSS Layout | Data Bindings | Interactions |
|---|---|---|---|---|
| **Header bar** (`.hdr`) | `<Header>` | Flexbox, `justify-content: space-between`, padding `12px 44px 10px` | `currentMonth` for subtitle; static title text | ADO Backlog link → `window.open(adoUrl, '_blank')` |
| **Legend** (right side of header) | `<Legend>` (child of Header) | Flexbox row, `gap: 22px` | `currentMonth` for "Now (Mon YYYY)" label | None (static) |
| **Timeline left sidebar** | `<TimelineSidebar>` | Flexbox column, `width: 230px`, `justify-content: space-around` | `milestones` filtered to unique tracks → track labels/colors | None |
| **Timeline SVG canvas** | `<TimelineSVG>` | SVG `width="1560" height="185"`, parent has `flex:1`, `padding-left:12px` | `milestones[]` → marker positions via `d3.scaleTime`; `currentDate` → NOW line x-position | Phase 2: hover tooltips on diamonds |
| **Month gridlines** (inside SVG) | Rendered inline in `<TimelineSVG>` | SVG `<line>` + `<text>` at computed x positions | `months[]` → x positions via `d3.scaleTime(dateRange, [0, 1560])` | None |
| **Track lines** (inside SVG) | Rendered inline in `<TimelineSVG>` | SVG `<line>` at y=42, 98, 154 | Track config array (color, y-position) | None |
| **Milestone markers** (inside SVG) | `<MilestoneMarker>` sub-component | SVG `<polygon>` (diamonds) or `<circle>` (checkpoints) with `<filter>` drop shadow | Each `Milestone` → x from date scale, y from track, shape from type | Phase 2: tooltip on hover |
| **NOW line** (inside SVG) | Rendered inline in `<TimelineSVG>` | SVG `<line>` stroke `#EA4335`, `stroke-dasharray="5,3"`, width 2 | `new Date()` → x via scale | None |
| **Heatmap title** (`.hm-title`) | `<HeatmapTitle>` | Block, 14px bold uppercase, `margin-bottom: 8px` | Static text | None |
| **Heatmap grid** (`.hm-grid`) | `<HeatmapGrid>` | CSS Grid: `grid-template-columns: 160px repeat(4, 1fr)`, `grid-template-rows: 36px repeat(4, 1fr)`, `flex:1` | `statusRows[]`, `months[]` | None |
| **Corner cell** (`.hm-corner`) | `<CornerCell>` | Grid position [1,1], flexbox centered | Static "STATUS" text | None |
| **Column headers** (`.hm-col-hdr`) | `<ColumnHeader month={m}>` | Grid row 1, flexbox centered, 16px bold | `month.label`, `month.isCurrent` → apply `.apr-hdr` highlight class | None |
| **Row headers** (`.hm-row-hdr`) | `<RowHeader category={c}>` | Grid column 1, 11px bold uppercase | `STATUS_COLORS[category]` → background/text color | None |
| **Data cells** (`.hm-cell`) | `<DataCell items={items} category={c} isCurrent={bool}>` | Padding `8px 12px`, `overflow: hidden` | `items[]` → bulleted list; empty → dash in `#AAA`; `isCurrent` → darker bg | None (Phase 2: "+N more" overflow) |
| **Work item bullet** (`.it`) | Rendered inline in `<DataCell>` | `position: relative`, `padding-left: 12px`, `::before` pseudo-element 6×6px circle | `item.title`, `STATUS_COLORS[category].accent` → bullet color | None |
| **Status bar** | `<StatusBar>` | Absolute bottom or flex footer, small text | `metadata.fetchedAt` → "Last updated: ..." | Retry button on error |
| **Warning banner** | `<WarningBanner>` | Fixed top overlay, yellow background | Shown when serving stale data after API failure | Dismiss / Retry |
| **Loading skeleton** | `<LoadingSkeleton>` | Matches 3-section layout with pulsing placeholders | Shown while `isLoading` from React Query | None |
| **Error state** | `<ErrorState>` | Centered full-page message | Shown when no data available at all | Retry button calls `refetch()` |

### File Structure

```
src/
├── main.tsx                     # Entry point, MSAL + QueryClient providers
├── App.tsx                      # AuthGuard + DashboardPage routing
├── auth/
│   ├── msalConfig.ts            # MSAL configuration (clientId, tenantId, scopes)
│   └── AuthGuard.tsx            # Redirects unauthenticated users
├── api/
│   └── useRoadmapData.ts        # React Query hook: GET /api/roadmap
├── components/
│   ├── Header/
│   │   ├── Header.tsx
│   │   ├── Header.module.css
│   │   └── Legend.tsx
│   ├── Timeline/
│   │   ├── TimelineArea.tsx
│   │   ├── TimelineArea.module.css
│   │   ├── TimelineSidebar.tsx
│   │   ├── TimelineSVG.tsx
│   │   └── MilestoneMarker.tsx
│   ├── Heatmap/
│   │   ├── HeatmapSection.tsx
│   │   ├── HeatmapSection.module.css
│   │   ├── HeatmapGrid.tsx
│   │   ├── ColumnHeader.tsx
│   │   ├── RowHeader.tsx
│   │   ├── CornerCell.tsx
│   │   └── DataCell.tsx
│   ├── StatusBar.tsx
│   ├── WarningBanner.tsx
│   ├── LoadingSkeleton.tsx
│   └── ErrorState.tsx
├── config/
│   ├── colors.ts                # STATUS_COLORS, track colors, CSS custom property map
│   ├── tracks.ts                # M1/M2/M3 track definitions (label, color, y-position)
│   └── constants.ts             # ADO backlog URL, staleTime, etc.
├── types/
│   └── roadmap.ts               # All TypeScript interfaces
├── utils/
│   ├── dateScale.ts             # d3.scaleTime factory for timeline x-axis
│   └── formatDate.ts            # date-fns helpers
├── styles/
│   ├── global.css               # CSS reset, custom properties, body 1920×1080
│   └── tokens.css               # :root color tokens from design spec
└── __tests__/
    ├── components/              # Vitest + React Testing Library unit tests
    └── visual/                  # Playwright screenshot tests
```

### CSS Strategy

- **`styles/tokens.css`** — All 30+ CSS custom properties from the design spec (color tokens).
- **`styles/global.css`** — Reset, body fixed at 1920×1080, font-family, flex column layout.
- **CSS Modules** — One `.module.css` per component directory. Scoped class names, no runtime cost.
- **No Tailwind, no CSS-in-JS.** The design has a fixed layout with ~30 unique style rules.

---

## Infrastructure Requirements

### Hosting

| Resource | Service | Tier | Notes |
|---|---|---|---|
| Frontend SPA | Azure Static Web Apps | Free | Global CDN, custom domain, auto-deploy from GitHub |
| API Functions | Azure Functions (bundled with SWA) | Consumption | Node.js 22, pay-per-execution |
| Monitoring | Azure Application Insights | Free (5 GB/mo) | Frontend JS SDK + API Node.js SDK |

### Networking

- All traffic over HTTPS (enforced by SWA).
- No VNet required — ADO API is publicly accessible with bearer auth.
- CDN caching for static assets (HTML/JS/CSS); API responses are not CDN-cached.

### CI/CD Pipeline

**GitHub Actions workflow:** `.github/workflows/ci.yml`

```yaml
triggers: push to main, pull_request

jobs:
  lint-typecheck:
    - npm ci
    - npm run lint          # ESLint flat config
    - npm run typecheck     # tsc --noEmit

  unit-test:
    - npm run test          # Vitest

  visual-regression:
    - npx playwright install --with-deps chromium
    - npm run build
    - npx playwright test   # Screenshot comparison at 1920×1080
    - Upload diff artifacts on failure

  deploy:
    needs: [lint-typecheck, unit-test, visual-regression]
    if: github.ref == 'refs/heads/main'
    - Azure/static-web-apps-deploy@v1
```

### Configuration Files

| File | Purpose |
|---|---|
| `staticwebapp.config.json` | Auth rules (require login), CSP headers, API route mapping, fallback to `index.html` |
| `api/local.settings.json` | Local dev: ADO org URL, project, area path (gitignored) |
| `api/host.json` | Azure Functions host config |
| `.env.production` | MSAL clientId, tenantId (non-secret, committed) |

### `staticwebapp.config.json`

```json
{
  "routes": [
    { "route": "/api/*", "allowedRoles": ["authenticated"] },
    { "route": "/*", "allowedRoles": ["authenticated"] }
  ],
  "responseOverrides": {
    "401": { "redirect": "/.auth/login/aad" }
  },
  "globalHeaders": {
    "Content-Security-Policy": "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; connect-src 'self' https://dev.azure.com https://login.microsoftonline.com; frame-ancestors 'none'",
    "X-Frame-Options": "DENY",
    "X-Content-Type-Options": "nosniff"
  }
}
```

---

## Technology Stack Decisions

| Layer | Choice | Version | Justification |
|---|---|---|---|
| **UI Framework** | React | 19.1.x | Mandatory stack. Functional components + hooks only. |
| **Language** | TypeScript | 5.7.x | Mandatory stack. Strict mode. |
| **Build** | Vite | 6.x | Fast HMR, native TS support, tree-shaking. CRA is deprecated. |
| **SVG Math** | D3 (`d3-scale`, `d3-time`) | 7.9.x | Date-to-pixel mapping for timeline. React renders the SVG; D3 only computes coordinates. Charting libraries (Recharts, Nivo) fight the custom diamond/circle design. |
| **CSS** | CSS Modules + vanilla CSS | — | Fixed layout, ~30 rules. No runtime cost. Tailwind/CSS-in-JS add complexity with no benefit. |
| **Data Fetching** | TanStack Query | 5.x | Built-in caching, staleTime, refetchOnWindowFocus, loading/error states. |
| **Date Utils** | date-fns | 4.x | Tree-shakeable, immutable. Month arithmetic for heatmap columns. |
| **Auth (client)** | `@azure/msal-react` + `@azure/msal-browser` | 2.x / 4.x | Microsoft standard for Entra ID SPAs. |
| **Runtime (API)** | Node.js | 22 LTS | Mandatory stack. LTS through April 2027. |
| **ADO Client** | `azure-devops-node-api` | 14.x | Official typed SDK. WIQL + work item batch fetch. |
| **Auth (API)** | `@azure/identity` | latest | OBO token exchange for ADO scope. |
| **Validation** | Zod | 3.x | Runtime schema validation of ADO responses. Catches malformed data before it reaches the client. |
| **Unit Tests** | Vitest | 3.x | Vite-native, Jest-compatible. |
| **Component Tests** | React Testing Library | 16.x | DOM-based, avoids implementation coupling. |
| **Visual Regression** | Playwright | 1.50.x | Pixel-comparison at 1920×1080. Critical success metric (≤0.1% diff). |
| **Lint** | ESLint + `@typescript-eslint` | 9.x / 8.x | Flat config format. |
| **Format** | Prettier | 3.x | Zero-config consistency. |

**Rejected alternatives:** Next.js (unnecessary SSR complexity for authed SPA), Redux/Zustand (single read-only data fetch doesn't need global state), Recharts/Nivo (can't produce custom diamond markers and drop shadows to spec), Tailwind (overkill for fixed layout), axios (native fetch in Node 22 suffices).

---

## Security Considerations

### Authentication Flow

1. **SPA → Entra ID:** MSAL `loginRedirect` with scope for the custom API. Silent token renewal on subsequent visits.
2. **SPA → API:** Bearer token in `Authorization` header on every `/api/roadmap` call.
3. **API → Entra ID:** Validate inbound token (audience, issuer, expiry). Exchange for ADO-scoped token via OBO flow.
4. **API → ADO:** Call ADO REST API with OBO token. User's own ADO permissions apply — no escalation.

### Authorization

- Binary: authenticated Microsoft Entra ID user = full access. No RBAC needed for MVP.
- SWA `staticwebapp.config.json` enforces `allowedRoles: ["authenticated"]` on all routes.

### Data Protection

- **In transit:** HTTPS everywhere (SWA enforced).
- **At rest:** No data persisted. `localStorage` cache contains only work item titles and IDs (no PII beyond assignee names already visible in ADO).
- **Secrets:** ADO PAT / app credentials stored in Azure Function Application Settings (encrypted at rest), never in client code or source control.

### Content Security Policy

Configured via `staticwebapp.config.json` (see Infrastructure section). Restricts `script-src` to `'self'`, blocks framing, limits `connect-src` to SWA origin + ADO + Entra ID endpoints.

### Input Validation

- API has no user-supplied query parameters in MVP — the WIQL queries are hardcoded constants.
- ADO API responses are validated with Zod schemas before transformation. Invalid data → 502 error, not silent corruption.

---

## Scaling Strategy

### Target Scale

< 500 concurrent users. This is an internal dashboard for ~50 regular users with occasional spikes during leadership reviews.

### Frontend Scaling

- Azure Static Web Apps serves static assets from global CDN — scales to any number of concurrent users automatically.
- No server-side rendering; the browser does all rendering work.

### API Scaling

- Azure Functions consumption plan auto-scales based on request volume.
- **Server-side cache (in-memory, 5-min TTL):** Deduplicates ADO API calls across concurrent users hitting the same Function instance.
- **Client-side cache (React Query, 5-min staleTime):** Each browser caches its own response, reducing API calls.
- **Net effect:** Even with 500 users, ADO sees at most ~1 request per 5 minutes per active Function instance (typically 1–3 instances).

### ADO API Rate Limits

- ADO rate limits are per-user (OBO flow). Each user's token counts against their own quota.
- If throttled (429), the API returns `retryAfter` to the client. React Query retries with exponential backoff.
- **Escape hatch:** If OBO-per-user causes aggregate throttling, switch to a service principal with a single ADO PAT and increase server-side cache TTL to 10 minutes.

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| 1 | **ADO API rate limits** hit under concurrent load | Medium | High — dashboard unresponsive | Dual caching (client 5-min + server 5-min). OBO spreads load across user quotas. Fallback: service principal + longer TTL. |
| 2 | **Pixel fidelity drift** from design spec | Medium | High — stakeholder rejection | Playwright screenshot tests in CI with ≤0.1% diff threshold. Run on every PR. |
| 3 | **WIQL query returns wrong data** due to ADO backlog restructuring | Medium | Medium — incorrect dashboard | Version WIQL queries as named constants. Document expected area path/iteration path structure. Alert on Zod validation failures. |
| 4 | **MSAL token acquisition fails** silently | Low | High — blank page | Implement `InteractionRequiredAuthError` handler → fallback to redirect. Show explicit error state, not blank page. |
| 5 | **ADO API unavailable** (outage) | Low | High — no data | Serve stale React Query cache + warning banner. `localStorage` fallback for cold starts. "Last updated" timestamp for transparency. |
| 6 | **SVG rendering differences** across Edge/Chrome | Low | Medium — visual inconsistencies | Playwright tests run on Chromium. Manual spot-check on Edge during QA. SVG features used (polygon, feDropShadow) are well-supported. |
| 7 | **Bundle size exceeds 200 KB** | Low | Low — slower load | Tree-shake D3 (import only `d3-scale`, `d3-time`). Monitor with `vite-plugin-visualizer`. date-fns is tree-shakeable by default. |
| 8 | **Entra ID app registration delayed** by identity team | Medium | High — blocks Phase 2 | Decouple: Phase 1 uses hardcoded data with no auth. App registration is only needed for Phase 2. Escalate early if blocked. |
| 9 | **Carryover detection logic is ambiguous** | Medium | Medium — misclassified items | Define carryover as: item existed in iteration N, was not Closed/Done at iteration end, and appears in iteration N+1. Document the rule. PM validates with manual audit. |
| 10 | **Function cold starts** exceed 2s target | Low | Low — slow first load after idle | Node.js 22 Functions cold start is typically <1s. If problematic, enable "always ready" instances ($0 on free tier not available — accept the trade-off). |