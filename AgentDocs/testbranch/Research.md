# Research

_No research has been documented yet._

## Research technology stack for ReportingDashboard

_Researched on 2026-05-01 06:06 UTC_

### Summary

The ReportingDashboard is a local-only tool that renders a composite release roadmap visualization — a Gantt timeline with milestone markers plus a color-coded heatmap grid — driven by Azure DevOps work-item data. The design reference is ~70% CSS Grid/Flexbox and ~30% SVG, which means **SVG + DOM + D3.js is the correct frontend rendering approach; Canvas and Phaser.js are categorically wrong for this UI**. The backend should be an ASP.NET Core 8 Minimal API serving three REST endpoints plus static files, with SQLite via EF Core 8 for local storage and on-demand ADO REST API sync for data ingestion. Total dependency count is deliberately minimal: 6 NuGet packages, 6 npm packages, zero cloud services, zero infrastructure cost. ---

### Key Findings

- **Canvas and Phaser.js are rejected.** The design uses CSS Grid with `::before` pseudo-elements, rich text with mixed font weights, and SVG `<polygon>` elements with drop-shadow filters. Canvas cannot render CSS pseudo-elements or native text wrapping; Phaser adds 1.2MB of unused game-engine code. SVG + DOM is the only approach that matches the design without reimplementation.
- **D3.js v7.9 is the right visualization library.** The Gantt timeline needs `scaleTime()` for date-to-pixel mapping, data joins for declarative SVG updates, and built-in transitions. Chart.js has no Gantt chart type and renders to Canvas. D3 adds only ~17KB gzipped when tree-shaken.
- **The heatmap grid requires zero charting library.** It is a CSS Grid layout with styled `<div>` elements — pure DOM + CSS. The design reference CSS can be copied directly into `dashboard.css`.
- **Blazor is rejected.** It eliminates TypeScript (violating the "TypeScript preferred" constraint), requires JS interop to call D3, and cannot render SVG `<polygon>` with `filter` attributes natively.
- **SQLite is the optimal local database.** Zero-installation, single-file, relational queries with GROUP BY for heatmap aggregation, and EF Core migrations for schema evolution. LiteDB lacks joins/aggregations; SQL Server LocalDB requires a 280MB install.
- **ADO sync uses WIQL + batch REST API.** One WIQL call returns work-item IDs; batch calls (200 per request) fetch details. 500 items = 4 API calls total. The dashboard functions offline after initial sync.
- **The "no cloud services" constraint is satisfied.** The ADO REST API call is a data pull (like `git fetch`), not a cloud service dependency. No data is stored in the cloud, no cloud compute runs, and there is no cloud billing.
- **Security implementation takes ~1 hour.** .NET User Secrets for PAT storage, DPAPI for at-rest encryption, localhost-only Kestrel binding, and `.gitignore` protection cover all threat vectors. --- **Goal:** Pixel-perfect static dashboard running on `localhost` with hardcoded sample data. | Task | Days | Output | |---|---|---| | Scaffold .sln with Api + Client projects | 0.5 | Solution builds and runs | | Port `OriginalDesignConcept.html` CSS → `dashboard.css` | 0.5 | Exact colors, grid, fonts | | Implement `header.ts` (title, subtitle, legend) | 0.5 | Header matches design | | Implement `heatmap.ts` (CSS Grid with sample items) | 1 | Heatmap grid matches design | | Implement `timeline.ts` with D3.js (Gantt SVG with sample milestones) | 2 | Timeline matches design | | Wire `main.ts` to render from `sample-data.json` | 0.5 | Full dashboard renders from static JSON | | Configure Vite proxy + MSBuild targets | 0.5 | `dotnet run` serves working dashboard | **Deliverable:** Team and stakeholders can see the exact design rendering in a browser. Visual feedback loop starts immediately. | Task | Days | Output | |---|---|---| | Define EF Core entities and `DashboardDbContext` | 0.5 | Schema defined | | Create initial migration; auto-migrate on startup | 0.5 | DB auto-creates | | Implement `GET /api/roadmap` endpoint | 1 | Returns JSON from SQLite | | Implement `GET /api/workitems` drill-down endpoint | 0.5 | Filtered cell data | | Seed database from `sample-data.json` on first run | 0.5 | Dashboard renders from DB | | Wire frontend `fetchRoadmap()` to API | 0.5 | Frontend reads from live API | | Add Swagger UI | 0.5 | Interactive API testing at `/swagger` | **Deliverable:** Dashboard renders from a real database. API is testable via Swagger. | Task | Days | Output | |---|---|---| | Implement `AdoSyncService` (WIQL + batch fetch) | 2 | Pulls live data from ADO | | Implement state mapping logic (configurable) | 1 | ADO states → dashboard categories | | Add `POST /api/sync` endpoint | 0.5 | One-click sync from UI | | Add "Sync" button to frontend header | 0.5 | User triggers sync; sees loading state | | Implement `CredentialStore` (DPAPI) | 0.5 | PAT encrypted at rest | | Add Serilog file logging | 0.5 | Diagnostic logs in `%LOCALAPPDATA%` | **Deliverable:** Dashboard shows live ADO data. One button to refresh. | Task | Days | Output | |---|---|---| | Add drill-down panel (click heatmap cell → show items with ADO links) | 1.5 | Interactive heatmap | | Dynamic "NOW" line based on `new Date()` | 0.5 | Line moves with current date | | Error handling and loading states | 0.5 | Graceful failure UX | | Backend + frontend unit tests | 1.5 | xUnit + Vitest test suites | | Self-contained single-file publish + README | 0.5 | Distributable EXE | | CI pipeline (GitHub Actions) | 0.5 | Automated build + test | **Deliverable:** Production-ready local tool. Distributable as a single EXE.
- **Copy the design CSS on Day 1.** The `OriginalDesignConcept.html` CSS is already production-quality. Extract it to `dashboard.css` and the team sees visual progress immediately — before any TypeScript is written.
- **Swagger UI for free API testing.** `Swashbuckle` provides an interactive API explorer at `/swagger` with 3 lines of setup. Invaluable during Phase 2–3 development.
- **`dotnet watch` + Vite HMR for <2s feedback.** Both tools support hot-reload. C# changes restart the API in ~200ms; TypeScript/CSS changes appear instantly via Vite HMR.
- **Sample data JSON as the Phase 1 contract.** Define `sample-data.json` matching the exact design content. This becomes the TypeScript interface definition, the API response shape, and the test fixture — all from one file. | Prototype | Why First | Risk Mitigated | |---|---|---| | **D3 timeline with milestone diamonds** | Most complex rendering component. If D3 can't match the design's drop-shadow diamonds and dashed NOW line, we need to know in Week 1, not Week 4. | D3 learning curve; SVG filter compatibility | | **ADO WIQL query** | Verify that ADO returns sufficient fields to categorize items into the four heatmap rows. If iteration paths don't map cleanly to months, the entire data model changes. | ADO data shape assumptions | | **Vite → wwwroot MSBuild copy** | Ensure the build pipeline works end-to-end before building features on top of it. A broken build integration in Week 3 would block everything. | Build toolchain integration |
- [ ] All existing tests pass (`dotnet test` + `npm test`)
- [ ] No TypeScript errors (`npx tsc --noEmit`)
- [ ] Dashboard renders at 1920×1080 matching design reference
- [ ] `dotnet run` from repo root starts a working dashboard
- [ ] README updated with any new setup steps ---
```typescript
// src/models/colors.ts
export const COLORS = {
  shipped: {
    bg: '#F0FBF0',
    bgActive: '#D8F2DA',
    dot: '#34A853',
    header: '#E8F5E9',
    text: '#1B7A28',
    border: '#CCC',
  },
  inProgress: {
    bg: '#EEF4FE',
    bgActive: '#DAE8FB',
    dot: '#0078D4',
    header: '#E3F2FD',
    text: '#1565C0',
    border: '#CCC',
  },
  carryover: {
    bg: '#FFFDE7',
    bgActive: '#FFF0B0',
    dot: '#F4B400',
    header: '#FFF8E1',
    text: '#B45309',
    border: '#CCC',
  },
  blocked: {
    bg: '#FFF5F5',
    bgActive: '#FFE4E4',
    dot: '#EA4335',
    header: '#FEF2F2',
    text: '#991B1B',
    border: '#CCC',
  },
  milestone: {
    poc: '#F4B400',
    production: '#34A853',
    checkpoint: '#999',
  },
  ui: {
    nowLine: '#EA4335',
    link: '#0078D4',
    gridBorder: '#E0E0E0',
    headerBg: '#F5F5F5',
    timelineBg: '#FAFAFA',
    bodyText: '#111',
    subtitleText: '#888',
    itemText: '#333',
    currentMonthHeaderBg: '#FFF0D0',
    currentMonthHeaderText: '#C07700',
  },
} as const;

export type StatusCategory = keyof typeof COLORS & ('shipped' | 'inProgress' | 'carryover' | 'blocked');
```
```typescript
// src/models/types.ts
export interface Workstream {
  id: string;           // "M1", "M2", "M3"
  name: string;         // "Chatbot & MS Role"
  color: string;        // "#0078D4"
  sortOrder: number;
}

export interface Milestone {
  id: string;
  workstreamId: string;
  name: string;         // "Mar 26 PoC"
  date: string;         // ISO 8601: "2026-03-26"
  type: 'PoC' | 'Production' | 'Checkpoint';
}

export interface WorkItem {
  id: string;           // ADO work item ID
  title: string;
  status: 'Shipped' | 'InProgress' | 'Carryover' | 'Blocked';
  month: string;        // "Jan", "Feb", "Mar", "Apr"
  workstreamId: string;
  adoUrl: string;
}

export interface RoadmapData {
  workstreams: Workstream[];
  milestones: Milestone[];
  workItems: WorkItem[];
  months: MonthColumn[];
  dateRange: { start: string; end: string };
  lastSyncUtc: string | null;
}

export interface MonthColumn {
  name: string;         // "Jan", "Feb", etc.
  isCurrent: boolean;   // true for the month matching today
}
```
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.*
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.*
dotnet add package Microsoft.AspNetCore.SpaProxy --version 8.0.*
dotnet add package Serilog.AspNetCore --version 8.0.*
dotnet add package Serilog.Sinks.File --version 5.0.*
dotnet add package Swashbuckle.AspNetCore --version 6.5.*
dotnet add package System.Security.Cryptography.ProtectedData --version 8.0.*
```
```bash
npm init -y
npm install d3
npm install -D typescript vite @types/d3 vitest @vitest/coverage-v8 jsdom
```
```bash
dotnet add package xunit --version 2.8.*
dotnet add package xunit.runner.visualstudio --version 2.8.*
dotnet add package Microsoft.NET.Test.Sdk --version 17.*
dotnet add package FluentAssertions --version 6.12.*
dotnet add package NSubstitute --version 5.1.*
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.*
```

### Recommended Tools & Technologies

- | Component | Technology | Version | License | Notes | |---|---|---|---|---| | Language | **TypeScript** | ~5.4+ | Apache-2.0 | `strict: true`, `target: ES2022` (no polyfills needed on Windows corporate browsers) | | Build tool | **Vite** | ~5.x | MIT | Dev server with HMR on :5173; production builds to `dist/` | | Visualization (timeline) | **D3.js** | ~7.9 | ISC | Selective imports: `d3-scale`, `d3-selection`, `d3-time`, `d3-time-format`, `d3-axis` | | D3 types | **@types/d3** | ~7.4 | MIT | Full TypeScript type definitions | | Layout (heatmap) | **CSS Grid + Flexbox** | Native | — | Ported directly from design reference; `grid-template-columns: 160px repeat(4,1fr)` | | Framework | **None (Vanilla TS)** | — | — | Single-view dashboard; React/Vue/Angular are unnecessary overhead | | CSS | **Plain CSS** | — | — | Custom Grid layout from design; Tailwind/Bootstrap would fight it | | Font | **Segoe UI** | System | — | Windows system font; zero web-font loading | | Component | Technology | Version | License | Notes | |---|---|---|---|---| | Runtime | **.NET 8.0 LTS** | 8.0.x | MIT | Support through November 2026 | | Web framework | **ASP.NET Core Minimal API** | in-box | MIT | 3 endpoints; ~400 lines total backend code | | JSON serialization | **System.Text.Json** | in-box | MIT | Source generators for AOT-friendly serialization | | HTTP client | **IHttpClientFactory** | in-box | MIT | Pooled connections for ADO REST API calls | | Logging | **Serilog.AspNetCore** | ~8.0.x | Apache-2.0 | Structured logging with file sink for local diagnostics | | Logging sink | **Serilog.Sinks.File** | ~5.0.x | Apache-2.0 | Rotating log files in `%LOCALAPPDATA%` | | OpenAPI | **Swashbuckle.AspNetCore** | ~6.5.x | MIT | Swagger UI at `/swagger` for dev-time API testing | | SPA proxy | **Microsoft.AspNetCore.SpaProxy** | 8.0.x | MIT | Proxies to Vite dev server during development | | Config | **Microsoft.Extensions.Configuration** | in-box | MIT | `appsettings.json` + User Secrets for PAT | | Component | Technology | Version | License | Notes | |---|---|---|---|---| | Engine | **SQLite** | bundled | Public domain | Zero-install; single `.db` file in `%LOCALAPPDATA%` | | ORM | **Microsoft.EntityFrameworkCore.Sqlite** | 8.0.x | MIT | LINQ queries, migrations, strongly-typed entities | | Migrations | **Microsoft.EntityFrameworkCore.Design** | 8.0.x | MIT | `dotnet ef migrations add` for schema evolution | | PAT encryption | **System.Security.Cryptography.ProtectedData** | 8.0.0 | MIT | DPAPI; CurrentUser scope; Windows-only | | Layer | Tool | Version | Notes | |---|---|---|---| | Backend unit/integration | **xUnit** | ~2.8.x | `[Fact]`/`[Theory]`; `WebApplicationFactory` for API integration tests | | Backend assertions | **FluentAssertions** | ~6.12.x | Readable `Should().Be()` syntax | | Backend mocking | **NSubstitute** | ~5.1.x | Interface mocking for service isolation | | HTTP mocking | **MockHttpMessageHandler** | — | Mock ADO REST API responses in `AdoSyncService` tests | | Frontend unit | **Vitest** | ~1.6.x | Vite-native; `jsdom` environment for DOM testing | | Frontend coverage | **@vitest/coverage-v8** | ~1.6.x | V8 coverage provider with `lcov` output | | Visual regression (Phase 2) | **Playwright** | ~1.x | Screenshot diffing against design reference at 1920×1080 | | Tool | Notes | |---|---| | **GitHub Actions** | Two parallel jobs: `backend` (dotnet build/test) + `frontend` (npm ci/lint/test/build). ~2 min total. | | **Self-contained publish** | `dotnet publish -r win-x64 --self-contained -p:PublishSingleFile=true` → ~30MB EXE | ---
```
ReportingDashboard.sln
├── src/
│   ├── ReportingDashboard.Api/           # ASP.NET Core 8 Minimal API
│   │   ├── Program.cs                    # Entry, DI, middleware, endpoint mapping
│   │   ├── Features/
│   │   │   ├── Roadmap/                  # GET /api/roadmap → full dashboard payload
│   │   │   │   ├── RoadmapEndpoint.cs
│   │   │   │   └── RoadmapDto.cs
│   │   │   ├── WorkItems/               # GET /api/workitems?status=X&month=Y
│   │   │   │   ├── WorkItemsEndpoint.cs
│   │   │   │   └── WorkItemDto.cs
│   │   │   └── Sync/                    # POST /api/sync → triggers ADO pull
│   │   │       ├── SyncEndpoint.cs
│   │   │       └── SyncResult.cs
│   │   ├── Data/
│   │   │   ├── DashboardDbContext.cs
│   │   │   ├── Entities/                # Workstream, Milestone, WorkItem
│   │   │   └── Migrations/
│   │   ├── Services/
│   │   │   ├── AdoSyncService.cs        # WIQL + batch fetch + state mapping
│   │   │   └── CredentialStore.cs       # DPAPI-based PAT storage
│   │   └── wwwroot/                     # Vite build output (auto-copied)
│   │
│   └── ReportingDashboard.Client/        # TypeScript + Vite
│       ├── package.json
│       ├── tsconfig.json
│       ├── vite.config.ts
│       ├── index.html
│       └── src/
│           ├── main.ts                   # Entry: fetch data, render components
│           ├── api/
│           │   └── roadmapApi.ts         # Typed fetch wrapper for /api/roadmap
│           ├── components/
│           │   ├── header.ts             # DOM: title, subtitle, legend
│           │   ├── timeline.ts           # D3.js: SVG Gantt with milestones
│           │   ├── heatmap.ts            # DOM: CSS Grid with status rows
│           │   └── drilldown.ts          # DOM: modal/panel for cell click
│           ├── models/
│           │   └── types.ts              # Workstream, Milestone, WorkItem interfaces
│           └── styles/
│               └── dashboard.css         # Ported from OriginalDesignConcept.html
│
├── tests/
│   ├── ReportingDashboard.Api.Tests/     # xUnit
│   └── ReportingDashboard.Client.Tests/  # Vitest (separate from Client/src)
│
└── ReportingDashboard.sln
```
```
Azure DevOps REST API
        │
        │  (WIQL + batch, authenticated via PAT)
        ▼
  AdoSyncService ───► SQLite DB (dashboard.db)
                          │
                          │  (EF Core LINQ queries)
                          ▼
                    Minimal API (/api/roadmap)
                          │
                          │  (JSON over localhost HTTP)
                          ▼
                    Browser (TypeScript)
                      ├── header.ts    → DOM + Flexbox
                      ├── timeline.ts  → D3.js + SVG
                      └── heatmap.ts   → DOM + CSS Grid
```
```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Workstream  │     │  Milestone   │     │   WorkItem   │
├──────────────┤     ├──────────────┤     ├──────────────┤
│ Id (PK)      │◄────│ WorkstreamId │     │ Id (PK)      │
│ Name         │     │ Id (PK)      │     │ Title        │
│ Color        │     │ Name         │     │ Status       │  ← Shipped|InProgress|
│ SortOrder    │     │ Date         │     │ Month        │     Carryover|Blocked
└──────────────┘     │ Type         │     │ WorkstreamId │──►┐
                     └──────────────┘     │ AdoUrl       │   │
                                          │ LastSyncedUtc│   │
                                          └──────────────┘   │
                                                             ▼
                                                       Workstream
```
- `WorkItems(Status, Month)` — composite index for heatmap GROUP BY query
- `Milestones(WorkstreamId)` — lookup milestones per workstream lane | Endpoint | Method | Purpose | Response Size | |---|---|---|---| | `/api/roadmap` | GET | Full dashboard state (workstreams, milestones, work items, last sync timestamp) | <50KB JSON | | `/api/workitems?status=X&month=Y` | GET | Drill-down: items for a specific heatmap cell | <5KB JSON | | `/api/sync` | POST | Trigger ADO data pull; returns item count and timestamp | <1KB JSON | **Design decision:** One monolithic GET for the dashboard. The entire payload is <50KB — one round-trip is faster than 6 parallel fetches and simpler to cache. | Layer | Mechanism | TTL | Invalidation | |---|---|---|---| | Backend | `IMemoryCache` on `/api/roadmap` response | 60 seconds | Cleared on POST `/api/sync` | | Frontend | Single fetch on page load; manual refresh button | N/A | User-triggered | | Static assets | `Cache-Control: immutable` via Vite hashed filenames | Indefinite | New build = new hash | | SQLite | No caching needed | N/A | <1ms queries at 6K rows |
```typescript
// main.ts — application entry point
import { fetchRoadmap } from '@/api/roadmapApi';
import { renderHeader } from '@/components/header';
import { renderTimeline } from '@/components/timeline';
import { renderHeatmap } from '@/components/heatmap';

async function init() {
  const data = await fetchRoadmap();

  renderHeader(document.getElementById('header')!, data);
  renderTimeline(
    document.querySelector('#timeline svg')! as SVGSVGElement,
    data.workstreams,
    data.milestones,
    [new Date(data.dateRange.start), new Date(data.dateRange.end)]
  );
  renderHeatmap(document.getElementById('heatmap')!, data);
}

init();
``` Each component is a pure function: `(container, data) → void`. No framework, no state management, no virtual DOM. The dashboard is one view with one data fetch. ---

### Considerations & Risks

- **Dashboard itself:** No authentication. Runs on `localhost:5000`, accessible only to the local user.
- **ADO API access:** Personal Access Token (PAT) with `Work Items (Read)` scope. | Layer | Mechanism | When | |---|---|---| | **Development** | .NET User Secrets (`dotnet user-secrets set "Ado:Pat" "..."`) | Always; PAT stored outside repo in `%APPDATA%\Microsoft\UserSecrets\` | | **Runtime** | Environment variable `REPORTINGDASHBOARD_ADO__PAT` | Alternative to user-secrets; injected at process start | | **At-rest** | DPAPI encryption via `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser` | For distributed EXE; encrypted file in `%LOCALAPPDATA%\ReportingDashboard\cred.dat` |
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // binds 127.0.0.1 + ::1 only
});
``` Even with a misconfigured firewall, Kestrel rejects non-localhost connections.
- SQLite database stored in `%LOCALAPPDATA%\ReportingDashboard\` (user-scoped ACLs on Windows)
- Database contains work-item titles and IDs — no PII, no credentials
- **SQLCipher encryption deferred** — add if security review requires it (`SQLitePCLRaw.bundle_e_sqlcipher` NuGet)
```gitignore
*.db
*.db-wal
*.db-shm
cred.dat
appsettings.*.json
!appsettings.json
!appsettings.Development.json
``` | Scenario | Command | Output | |---|---|---| | **Developer** | `dotnet run` + `npm run dev` | API on :5000, Vite HMR on :5173 | | **Distribution** | `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` | Single ~30MB EXE | | **Desktop app (Phase 2)** | WPF/WinForms shell with embedded WebView2 | Native window; no browser required | **$0.** Zero cloud services, zero hosting, zero database servers, zero licenses. ADO REST API calls are free under existing ADO licensing. --- | Risk | Severity | Probability | Mitigation | |---|---|---|---| | **D3.js learning curve** | Medium | Medium | Team only needs 5 D3 modules (scale, selection, time, time-format, axis). Provide working `timeline.ts` example in Phase 1 spike. | | **Vite + MSBuild integration fragility** | Medium | Low | Use the `Microsoft.AspNetCore.SpaProxy` pattern from the official `dotnet new react` template. Well-tested by the .NET team. | | **ADO work-item state mapping ambiguity** | Medium | High | "Carryover" has no standard ADO state. Clarify with stakeholders before Phase 3. Default to iteration-based detection with tag override. | | **ADO API rate limiting** | Low | Low | On-demand sync only; batch calls (200 items/request). 500 items = 4 requests, well under the 200 req/min limit. | | **SQLite concurrency during sync** | Low | Low | WAL mode; single-user app; sync writes are sequential. | | Decision | What We Gain | What We Give Up | Revisit Trigger | |---|---|---|---| | Vanilla TypeScript (no React/Vue) | Minimal bundle; no framework learning; direct DOM control | Component lifecycle; state management; large ecosystem | App grows beyond 3 views | | SVG over Canvas | CSS styling; accessibility; DOM events; design fidelity | Performance ceiling at >10K elements | Dashboard visualizes >5,000 simultaneous SVG nodes | | SQLite over JSON file | SQL queries; migrations; relational integrity | 10 minutes extra setup | Never — SQLite is strictly superior for structured data | | Single `/api/roadmap` endpoint | One round-trip; simple caching; simple frontend | No partial updates; full payload every time | Payload exceeds 1MB | | D3.js over custom SVG code | Date scales; data joins; transitions; tested math | 17KB bundle; learning curve | Team already has SVG rendering expertise and prefers raw DOM | | Skill | Current Team Level | Risk | Remediation | |---|---|---|---| | C# / ASP.NET Core | High | None | — | | TypeScript | Medium-High | Low | Strict tsconfig catches issues at compile time | | D3.js | Low-Medium | Medium | Provide annotated `timeline.ts` reference implementation; limit to 5 modules | | SVG | Medium | Low | Design reference already contains the exact SVG structure to replicate | | EF Core + SQLite | High | None | — | | Vite | Low | Low | Zero-config for TypeScript; only `vite.config.ts` proxy needs understanding | --- These require stakeholder decisions before implementation begins. Ordered by blocking priority: | # | Question | Blocks | Proposed Default | |---|---|---|---| | 1 | **How are ADO work items categorized into Shipped/InProgress/Carryover/Blocked?** By state field? By tag? By iteration? | Phase 3 (ADO sync) | Hybrid: state-based for Shipped/InProgress; iteration-based for Carryover (past iteration + not closed); tag-based for Blocked (`[blocked]` tag) | | 2 | **What ADO project and area path should the sync query?** Single team or cross-team? | Phase 3 (ADO sync) | Configurable in `appsettings.json`: `Ado:AreaPath` = `"One\\Privacy Automation"` | | 3 | **Should the dashboard support historical snapshots?** (View January's status as it was in January) | Schema design | No — show current state only. Add snapshot table later if requested. | | 4 | **Is on-demand sync sufficient, or should the dashboard auto-refresh?** | Frontend design | On-demand via "Sync" button. Add optional timer (configurable interval) in Phase 4 if requested. | | 5 | **Will this ever be deployed as a shared web app (not local-only)?** | Architecture | No — design for local-only. If shared deployment is needed later, add authentication and switch to a hosted SQLite or PostgreSQL. | | 6 | **Should the tool include a "Export as PNG" button?** | Frontend scope | No — use browser screenshot (Ctrl+Shift+S). The 1920×1080 fixed layout is already screenshot-optimized. Add html2canvas export in Phase 4 if requested. | | 7 | **How many months should the heatmap display?** The design shows 4 (Jan–Apr). | UI layout | Configurable: default 4 months centered on current month. CSS Grid columns adjust dynamically: `160px repeat(N, 1fr)`. | ---

### Detailed Analysis

# Deep-Dive Analysis: ReportingDashboard Sub-Questions

---

## Sub-Question 1: Optimal Frontend Rendering Approach

**Question:** What is the optimal frontend rendering approach (HTML5 Canvas vs. SVG vs. Phaser.js) for the specific UI components in the design — timeline/Gantt, heatmap grid, and milestone diamonds?

### Key Findings

The design reference (`OriginalDesignConcept.html`) contains three distinct rendering zones, each with different technical requirements:

| Zone | Element Count | Interactivity | Text Rendering | CSS Integration |
|---|---|---|---|---|
| **Header + Legend** | ~15 DOM nodes | Click links | Rich text with mixed fonts/sizes | Flexbox layout, inline styles |
| **Timeline/Gantt SVG** | ~60 SVG elements | Hover tooltips, click milestones | SVG `<text>` with anchor/alignment | Embedded in Flexbox container |
| **Heatmap Grid** | ~80-120 DOM nodes | Click cells for drill-down | HTML text with `::before` pseudo-elements | **CSS Grid** with `160px repeat(4,1fr)` |

**Critical observation:** The heatmap grid uses CSS `::before` pseudo-elements for colored bullet dots. This is a CSS-only feature — Canvas and Phaser cannot render pseudo-elements. Replicating this in Canvas would require manual circle drawing at computed positions for every work item.

### Technology Evaluation

#### HTML5 Canvas 2D

| Aspect | Assessment |
|---|---|
| **Text rendering** | `ctx.fillText()` has no line wrapping, no rich formatting, no sub-pixel anti-aliasing on all platforms. The design has 12px text with mixed weights — Canvas text would look noticeably worse than DOM text on Windows. |
| **Hit testing** | No built-in event delegation. Every clickable element (milestone diamond, heatmap cell, ADO link) requires manual `isPointInPath()` checks or a spatial index. For ~200 interactive elements, this is significant boilerplate. |
| **CSS Grid layout** | Impossible. The heatmap's `grid-template-columns: 160px repeat(4,1fr)` would need to be reimplemented as manual coordinate math. Every window resize requires full recalculation. |
| **Accessibility** | Canvas is a single opaque bitmap to screen readers. WCAG 2.1 AA compliance requires a parallel hidden DOM — doubling the implementation effort. |
| **Retina/HiDPI** | Requires manual `devicePixelRatio` scaling. SVG and DOM handle this automatically. |
| **Performance** | Canvas excels at >10,000 elements (particle systems, data plots with millions of points). At ~200 elements, this advantage is irrelevant — DOM/SVG rendering is <1ms. |

**Verdict: Reject.** Canvas is the wrong tool for a layout-driven, text-heavy, interactive dashboard with <500 elements.

#### Phaser.js 3.80+

| Aspect | Assessment |
|---|---|
| **Bundle size** | `phaser` npm package is **1.2MB minified** (4.1MB unminified). The entire ReportingDashboard UI logic will be <50KB. Phaser would be 96% of the frontend bundle. |
| **Rendering model** | Phaser renders to a WebGL canvas (or Canvas 2D fallback). Same limitations as Canvas above, plus the overhead of a scene graph designed for game objects (Sprites, Tweens, Physics bodies). |
| **Layout engine** | None. Phaser has no CSS Grid, no Flexbox, no text wrapping. Positioning is absolute pixel coordinates only. |
| **Learning curve** | Phaser's API is designed around game loops (`preload`, `create`, `update`), sprite sheets, and arcade physics. None of these concepts map to a reporting dashboard. The team would be learning a game engine to render a table. |
| **Community examples** | Zero dashboard/reporting examples exist in the Phaser ecosystem. Every tutorial, plugin, and community answer assumes game development. |
| **Appropriate use cases** | 2D games, interactive simulations, sprite-based animations. |

**Verdict: Reject.** Phaser is a game engine. Using it for a CSS Grid dashboard is like using Unreal Engine to render a spreadsheet. The abstraction mismatch would cost weeks of development time and produce an inferior result.

#### SVG + DOM + CSS (Recommended)

| Aspect | Assessment |
|---|---|
| **Design fidelity** | The existing design *already uses SVG* for the timeline and *already uses CSS Grid* for the heatmap. Porting to TypeScript means parameterizing the existing markup — not rewriting the rendering approach. |
| **Text rendering** | Native browser text rendering — sub-pixel anti-aliasing, `font-family: 'Segoe UI'`, CSS `font-weight`, `letter-spacing`, `text-transform`. Identical to the design reference. |
| **Interactivity** | Standard DOM events (`click`, `mouseenter`, `mouseleave`) on SVG elements and HTML divs. No hit-testing math needed. |
| **CSS Grid** | The heatmap's `grid-template-columns: 160px repeat(4,1fr)` works natively. Responsive behavior is free. |
| **Accessibility** | SVG elements support `role`, `aria-label`, `tabindex`. Screen readers can navigate the timeline. HTML heatmap cells are inherently accessible. |
| **Performance** | At ~200-300 total DOM nodes, rendering is <2ms. Re-rendering the entire dashboard on data change (via D3 data joins or manual DOM updates) takes <5ms. No virtualization needed. |
| **Developer tooling** | Browser DevTools inspect individual SVG elements and CSS Grid cells. Canvas debugging requires specialized tools. |

**Verdict: Strongly recommended.** This is not a close decision. The design was built with SVG + CSS, the requirements (text, layout, interactivity, accessibility) all favor DOM-based rendering, and the element count is orders of magnitude below where Canvas becomes beneficial.

### Concrete Recommendation

**Use a hybrid approach: DOM + CSS for layout, SVG for the timeline, D3.js for data binding.**

```
┌─────────────────────────────────────────────┐
│  HEADER (DOM + CSS Flexbox)                 │  ← header.ts: creates divs
├─────────────┬───────────────────────────────┤
│  Workstream │  TIMELINE (SVG via D3.js)     │  ← timeline.ts: D3 scales + SVG
│  Labels     │  <svg> with lines, diamonds,  │
│  (DOM)      │  circles, text                │
├─────────────┴───────────────────────────────┤
│  HEATMAP (DOM + CSS Grid)                   │  ← heatmap.ts: creates grid divs
│  grid-template-columns: 160px repeat(4,1fr) │
│  Each cell contains styled <div> items      │
└─────────────────────────────────────────────┘
```

The TypeScript component boundaries map 1:1 to the design's visual sections. No abstraction mismatch.

---

## Sub-Question 2: C# .NET 8 Backend Architecture

**Question:** What C# .NET 8 backend architecture (ASP.NET Core Minimal API, Razor Pages, Blazor) best serves a local-only dashboard with real-time data refresh?

### Key Findings

The backend has three responsibilities:
1. **Serve static files** (the Vite-built frontend)
2. **Expose REST endpoints** (roadmap data, work items, sync trigger)
3. **Sync data from ADO** (HTTP calls to ADO REST API, write to SQLite)

This is a narrow API surface — 3 endpoints, no authentication, no middleware complexity. The architecture choice should minimize ceremony.

### Technology Evaluation

#### ASP.NET Core Minimal API (Recommended)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DashboardDbContext>();
builder.Services.AddHttpClient<AdoSyncService>();

var app = builder.Build();
app.UseStaticFiles(); // serves wwwroot/

app.MapGet("/api/roadmap", async (DashboardDbContext db) =>
    Results.Ok(await db.GetRoadmapAsync()));

app.MapPost("/api/sync", async (AdoSyncService svc) =>
    Results.Ok(await svc.SyncAsync()));

app.Run();
```

| Aspect | Assessment |
|---|---|
| **Lines of code for full API** | ~50 lines in `Program.cs` + ~100 lines per feature handler. Total backend: ~400 lines. |
| **Startup time** | <200ms. Critical for a local tool that users start/stop frequently. |
| **Static file serving** | `UseStaticFiles()` serves the Vite build output from `wwwroot/`. Zero config. |
| **JSON serialization** | `System.Text.Json` source generators for AOT-friendly, zero-allocation serialization. |
| **Dependency injection** | Full `IServiceCollection` — register `DbContext`, `HttpClient`, services. |
| **OpenAPI/Swagger** | `builder.Services.AddEndpointsApiExplorer()` + Swashbuckle for dev-time API docs. |
| **Version** | .NET 8.0 LTS. `Microsoft.AspNetCore.App` metapackage includes everything needed. |

#### Blazor Server / Blazor WebAssembly

| Aspect | Assessment |
|---|---|
| **Blazor Server** | Maintains a SignalR circuit per browser tab. Adds WebSocket complexity for zero benefit — the dashboard is read-only with manual refresh. |
| **Blazor WASM** | Compiles C# to WebAssembly. The frontend runs in the browser, which sounds appealing — but D3.js (our visualization library) is JavaScript. Calling D3 from Blazor WASM requires JS interop (`IJSRuntime.InvokeAsync`), which is slower, harder to debug, and loses TypeScript type safety. |
| **Blazor rendering** | Blazor's `RenderTreeBuilder` does not support SVG `<polygon>` elements with `filter` attributes (used for milestone diamond drop shadows in the design). Workaround: raw `MarkupString` — which defeats the purpose of using Blazor's component model. |
| **TypeScript** | The spec says "TypeScript preferred." Blazor eliminates TypeScript from the frontend — the team loses type safety on the rendering code, which is the most complex part of the application. |

**Verdict: Reject Blazor.** It fights against D3.js, TypeScript, and SVG — the three technologies best suited for this dashboard's rendering.

#### Razor Pages

| Aspect | Assessment |
|---|---|
| **Model** | Server-rendered HTML with page models. Each page is a `.cshtml` file with a `PageModel` class. |
| **Fit** | Razor Pages excels at multi-page, form-driven apps (CRUD, admin panels). This dashboard is a single-page visualization with no forms. |
| **JavaScript integration** | Razor Pages can include `<script>` tags, but there's no built-in TypeScript compilation pipeline. The team would need to bolt on Vite/esbuild anyway. |
| **Advantage over Minimal API** | None for this project. Razor Pages adds a `Pages/` folder convention and `PageModel` base class overhead without benefit. |

**Verdict: Reject.** Not wrong, but unnecessarily ceremonious for a single-page dashboard API.

### Concrete Recommendation

**ASP.NET Core 8 Minimal API with the following configuration:**

```csharp
// Program.cs — complete backend setup
var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddDbContext<DashboardDbContext>(opt =>
    opt.UseSqlite($"Data Source={GetDbPath()}"));
builder.Services.AddHttpClient<AdoSyncService>();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();  // serves index.html for /
app.UseStaticFiles();   // serves wwwroot/*

// Endpoints
app.MapRoadmapEndpoints();   // GET /api/roadmap
app.MapWorkItemEndpoints();  // GET /api/workitems
app.MapSyncEndpoints();      // POST /api/sync

// Ensure DB exists
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<DashboardDbContext>();
await db.Database.MigrateAsync();

app.Run();
```

**Key design decisions:**
- `AddMemoryCache()` for response caching (60s TTL, invalidated on sync)
- `AddHttpClient<AdoSyncService>()` for resilient, pooled HTTP connections to ADO
- `Database.MigrateAsync()` on startup — the database auto-creates and migrates. Zero manual setup for users.
- Kestrel binds to `localhost:5000` by default. Configure in `launchSettings.json`.

### SPA Development Proxy

For hot-reload during development, add the Vite SPA proxy:

```csharp
// Only in Development — proxy unknown routes to Vite dev server
if (app.Environment.IsDevelopment())
{
    app.UseSpa(spa =>
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    });
}
```

Package: `Microsoft.AspNetCore.SpaProxy` 8.0.x (NuGet). This enables:
- `dotnet watch run` serves the API on :5000
- `npm run dev` (Vite) serves the frontend on :5173 with HMR
- API calls from the frontend proxy through ASP.NET Core → no CORS issues

---

## Sub-Question 3: Local Data Storage Engine

**Question:** What local data storage engine (SQLite, LiteDB, embedded SQL Server) best fits structured roadmap/backlog data with query flexibility?

### Key Findings

The data model has three clear entities with relational integrity needs:

```
Workstream (1) ──► (N) Milestone
Workstream (1) ──► (N) WorkItem
WorkItem has: Status (enum), Month (temporal), Title, AdoUrl
```

Typical data volumes:
- 3-10 workstreams
- 10-50 milestones
- 50-500 work items per month
- 6-12 months visible → 300-6,000 work items total

Query patterns:
- "All work items grouped by Status and Month" (heatmap)
- "All milestones for workstream M1 ordered by date" (timeline)
- "Work items where Status = 'Blocked' and Month = 'Apr'" (drill-down)

### Technology Evaluation

#### SQLite via EF Core 8 (Recommended)

| Aspect | Detail |
|---|---|
| **Package** | `Microsoft.EntityFrameworkCore.Sqlite` 8.0.11 |
| **Underlying provider** | `Microsoft.Data.Sqlite` 8.0.11 (wraps `e_sqlite3` native library) |
| **Database file** | Single `.db` file. Portable, copyable, backupable. |
| **Schema migrations** | `dotnet ef migrations add InitialCreate` → versioned schema evolution |
| **Query capability** | Full SQL via LINQ. Aggregations, joins, ordering, filtering — all needed for dashboard queries. |
| **Performance** | At 6,000 rows, SELECT with WHERE + GROUP BY returns in <1ms. No index tuning needed. |
| **Concurrency** | WAL (Write-Ahead Logging) mode supports concurrent reads during a write. For a single-user local app, this is more than sufficient. |
| **Max practical size** | SQLite handles databases up to 281 TB. Our data will never exceed 10MB. |
| **Tooling** | DB Browser for SQLite (free GUI), `sqlite3` CLI, VS Code extensions. Rich ecosystem for inspection and debugging. |

**EF Core 8 + SQLite specific capabilities:**
```csharp
// Strongly-typed LINQ query for heatmap data
var heatmapData = await db.WorkItems
    .GroupBy(wi => new { wi.Status, wi.Month })
    .Select(g => new HeatmapCell
    {
        Status = g.Key.Status,
        Month = g.Key.Month,
        Items = g.Select(wi => new WorkItemDto
        {
            Id = wi.Id,
            Title = wi.Title,
            AdoUrl = wi.AdoUrl
        }).ToList()
    })
    .ToListAsync();
```

This generates efficient SQL and returns the exact shape the frontend needs.

#### LiteDB 5.0.21

| Aspect | Detail |
|---|---|
| **Model** | Document database (NoSQL). Stores BSON documents in a single file. |
| **Package** | `LiteDB` 5.0.21 (NuGet) |
| **Query** | LINQ-like syntax but limited: no GROUP BY, no JOIN, no aggregations. |
| **Schema** | Schema-less. No migrations. Sounds appealing but means no compile-time validation of data shapes. |
| **Relational queries** | The heatmap needs "group work items by Status × Month." LiteDB would require loading all items into memory and grouping in C# — fine at 6K rows but architecturally wrong. |
| **Community** | 8.3K GitHub stars, but development has slowed significantly. Last major release was 2021. Maintenance mode. |

**Verdict: Reject.** The data is inherently relational (workstreams → milestones, workstreams → work items). LiteDB's lack of joins and aggregations means the application code would compensate for what the database should handle natively.

#### SQL Server Express LocalDB

| Aspect | Detail |
|---|---|
| **Package** | `Microsoft.EntityFrameworkCore.SqlServer` 8.0.x |
| **Installation** | Requires SQL Server Express LocalDB installed on the machine. ~280MB install. |
| **Startup** | LocalDB instances start on first connection (~2-5 seconds cold start). Noticeable delay. |
| **Features** | Full SQL Server feature set — stored procedures, full-text search, JSON support. |
| **Overkill factor** | High. The dashboard queries are simple SELECTs with GROUP BY. SQL Server's advanced features (columnstore indexes, CLR types, Service Broker) are irrelevant. |
| **Distribution** | Users must have LocalDB installed. SQLite requires zero installation — it's bundled in the NuGet package. |

**Verdict: Reject.** Unnecessary dependency and installation burden for a simple local tool.

#### JSON File (Simplest Option)

| Aspect | Detail |
|---|---|
| **Implementation** | Read/write `dashboard.json` with `System.Text.Json`. |
| **Queries** | LINQ-to-Objects on deserialized collections. |
| **Concurrency** | File locking required for read-during-write safety. |
| **Migrations** | Manual versioning. If the data shape changes, write migration code by hand. |
| **Appropriate for** | Config files, small settings, <100 items. |

**Verdict: Reject for primary storage. Use as seed data format.** A JSON file works well for initial sample data or for importing/exporting snapshots, but not as the runtime database. EF Core migrations provide schema evolution that JSON files cannot.

### Concrete Recommendation

**SQLite via `Microsoft.EntityFrameworkCore.Sqlite` 8.0.x**

**Database location:**
```csharp
static string GetDbPath() =>
    Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ReportingDashboard",
        "dashboard.db");
```

**Connection string in `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "Dashboard": "Data Source=%LOCALAPPDATA%/ReportingDashboard/dashboard.db"
  }
}
```

**SQLite pragmas for optimal local performance:**
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder options)
{
    options.UseSqlite(connectionString, sqliteOpts =>
    {
        sqliteOpts.CommandTimeout(10);
    });
}

// In DashboardDbContext.OnModelCreating or via a migration
// Enable WAL mode and reasonable cache size
// Execute: PRAGMA journal_mode=WAL; PRAGMA cache_size=-8000; (8MB cache)
```

**Entity configuration:**
```csharp
public class DashboardDbContext : DbContext
{
    public DbSet<Workstream> Workstreams => Set<Workstream>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkItem>()
            .HasIndex(wi => new { wi.Status, wi.Month }); // heatmap query index

        modelBuilder.Entity<Milestone>()
            .HasIndex(m => m.WorkstreamId);
    }
}
```

The composite index on `(Status, Month)` ensures the heatmap query is a single index scan — though at 6K rows, even a full table scan is <1ms. The index is future-proofing for larger datasets.

---

## Sub-Question 4: Frontend TypeScript Project Integration with .sln

**Question:** How should the frontend TypeScript project be structured, built, and integrated within the .sln to enable a single `dotnet run` developer experience?

### Key Findings

The challenge: a .sln file expects .csproj projects. The TypeScript frontend uses npm/Vite. These two build systems must coexist with three requirements:
1. `dotnet run` works out of the box (dev experience)
2. `dotnet publish` produces a complete deployable artifact (distribution)
3. `npm run dev` provides HMR during frontend development (inner loop)

### Approach: Dual-Project with MSBuild Integration

The TypeScript project lives as a sibling directory to the API project, **not inside it**:

```
src/
├── ReportingDashboard.Api/
│   ├── ReportingDashboard.Api.csproj    ← references Client build output
│   ├── Program.cs
│   └── wwwroot/                         ← Vite build output copied here
│
└── ReportingDashboard.Client/
    ├── package.json
    ├── package-lock.json
    ├── tsconfig.json
    ├── vite.config.ts
    ├── index.html
    └── src/
        ├── main.ts
        └── ...
```

**Why not inside the API project?** Mixing `node_modules/` (300MB+) with `bin/`/`obj/` creates confusion. Separate directories have clean boundaries and independent `.gitignore` entries.

### MSBuild Integration (.csproj Targets)

```xml
<!-- ReportingDashboard.Api.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <SpaRoot>..\ReportingDashboard.Client\</SpaRoot>
    <SpaProxyServerUrl>http://localhost:5173</SpaProxyServerUrl>
    <SpaProxyLaunchCommand>npm run dev</SpaProxyLaunchCommand>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.SpaProxy" Version="8.0.*" />
  </ItemGroup>

  <!-- Restore npm packages when building in Release or CI -->
  <Target Name="NpmInstall" BeforeTargets="Build"
          Condition="!Exists('$(SpaRoot)node_modules')">
    <Exec Command="npm ci --prefer-offline" WorkingDirectory="$(SpaRoot)" />
  </Target>

  <!-- Build the frontend during Publish (not Debug builds — use HMR instead) -->
  <Target Name="PublishClient" BeforeTargets="Build"
          Condition="'$(Configuration)' == 'Release'">
    <Exec Command="npm ci --prefer-offline" WorkingDirectory="$(SpaRoot)" />
    <Exec Command="npm run build" WorkingDirectory="$(SpaRoot)" />
  </Target>

  <!-- Copy Vite output to wwwroot for Publish -->
  <Target Name="CopyClientDist" AfterTargets="PublishClient"
          Condition="'$(Configuration)' == 'Release'">
    <ItemGroup>
      <DistFiles Include="$(SpaRoot)dist\**\*" />
    </ItemGroup>
    <Copy SourceFiles="@(DistFiles)"
          DestinationFolder="wwwroot\%(RecursiveDir)" />
  </Target>
</Project>
```

### Vite Configuration

```typescript
// vite.config.ts
import { defineConfig } from 'vite';
import path from 'path';

export default defineConfig({
  root: '.',
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    sourcemap: true,
  },
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, 'src'),
    },
  },
});
```

**Key: the Vite dev server proxies `/api/*` to ASP.NET Core.** This means:
- Frontend devs run `npm run dev` and access `localhost:5173`
- API calls go to `localhost:5173/api/roadmap` → proxied to `localhost:5000/api/roadmap`
- No CORS configuration needed
- HMR works for CSS and TypeScript changes

### Developer Workflows

| Scenario | Commands | What Happens |
|---|---|---|
| **Full-stack dev** | Terminal 1: `dotnet watch run`<br>Terminal 2: `cd Client && npm run dev` | Backend on :5000, frontend on :5173 with HMR. Changes to either side are instant. |
| **Backend-only dev** | `dotnet run` | Serves pre-built static files from `wwwroot/`. No HMR. |
| **Build for distribution** | `dotnet publish -c Release -r win-x64 --self-contained` | Runs `npm ci && npm run build`, copies dist → wwwroot, publishes single-file EXE. |
| **CI pipeline** | `dotnet build -c Release && dotnet test` | MSBuild targets handle npm install + build automatically. |

### TypeScript Configuration

```json
// tsconfig.json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "ESNext",
    "moduleResolution": "bundler",
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "isolatedModules": true,
    "esModuleInterop": true,
    "lib": ["ES2022", "DOM", "DOM.Iterable"],
    "skipLibCheck": true,
    "paths": {
      "@/*": ["./src/*"]
    },
    "outDir": "./dist",
    "sourceMap": true
  },
  "include": ["src/**/*.ts"],
  "exclude": ["node_modules", "dist"]
}
```

**`target: ES2022`** — modern browsers on Windows corporate devices support ES2022 natively. No need for Babel or polyfills. This produces smaller, faster output.

### .sln File Structure

```
ReportingDashboard.sln
├── src\ReportingDashboard.Api\ReportingDashboard.Api.csproj
├── tests\ReportingDashboard.Api.Tests\ReportingDashboard.Api.Tests.csproj
└── (Solution Items)
    ├── .gitignore
    ├── README.md
    └── Directory.Build.props
```

The TypeScript project is **not** in the .sln (it's not a .csproj). It's a dependency of the API project, triggered by MSBuild targets. This is the standard pattern for ASP.NET Core + SPA projects (same pattern used by the `dotnet new react` and `dotnet new angular` templates).

---

## Sub-Question 5: Charting/Visualization Libraries

**Question:** What charting/visualization libraries (D3.js, Chart.js, custom SVG) can replicate the exact heatmap-grid and Gantt-timeline from the design reference?

### Key Findings

The design has two distinct visualization types:

**Type A: Gantt Timeline (SVG)**
- Horizontal workstream lanes with colored lines
- Milestone markers: diamonds (`<polygon>`) for PoC/Production, circles for checkpoints
- Date labels with `text-anchor: middle`
- Drop shadow filter on diamonds (`<filter><feDropShadow>`)
- Dashed "NOW" line with label
- Month grid lines

**Type B: Heatmap Grid (CSS Grid)**
- 5-column × 5-row grid (header + 4 status categories)
- Each cell contains a list of work items with colored bullet dots
- Row-specific color theming (green/blue/yellow/red)
- Current-month column highlighted with different background
- Column headers with month names

### Library Evaluation for Type A (Gantt Timeline)

#### D3.js v7.9 (Recommended)

| Aspect | Detail |
|---|---|
| **Package** | `d3` 7.9.0, `@types/d3` 7.4.3 |
| **Bundle impact** | Tree-shakeable. Import only needed modules: `d3-scale` (4KB), `d3-selection` (6KB), `d3-axis` (2KB), `d3-time` (3KB), `d3-time-format` (2KB). Total: ~17KB gzipped. |
| **Scale for time axis** | `d3.scaleTime()` maps `Date` objects to pixel positions. Handles month boundaries, uneven months, and date math correctly. |
| **SVG generation** | `d3.select('svg').selectAll('line').data(months).join('line')` — declarative data binding to SVG elements. |
| **Milestone diamonds** | D3's `d3.symbol().type(d3.symbolDiamond)` generates diamond paths. Or use raw `<polygon>` like the design reference — D3 doesn't force its abstractions. |
| **Transitions** | Built-in: `selection.transition().duration(300).attr('cx', newX)` for animated updates when data changes. |
| **TypeScript support** | `@types/d3` provides full type definitions. `d3.scaleTime<number, number>()` is generic and type-safe. |

**D3 code for the timeline (sketch):**

```typescript
import * as d3 from 'd3';
import type { Milestone, Workstream } from '@/models';

export function renderTimeline(
  container: SVGSVGElement,
  workstreams: Workstream[],
  milestones: Milestone[],
  dateRange: [Date, Date]
) {
  const width = 1560;
  const x = d3.scaleTime()
    .domain(dateRange)
    .range([0, width]);

  // Month grid lines
  const months = d3.timeMonth.range(dateRange[0], dateRange[1]);
  d3.select(container)
    .selectAll('line.month-grid')
    .data(months)
    .join('line')
    .attr('x1', d => x(d))
    .attr('x2', d => x(d))
    .attr('y1', 0)
    .attr('y2', 185)
    .attr('stroke', '#bbb')
    .attr('stroke-opacity', 0.4);

  // NOW line
  const now = new Date();
  d3.select(container)
    .selectAll('line.now-line')
    .data([now])
    .join('line')
    .attr('x1', d => x(d))
    .attr('x2', d => x(d))
    .attr('y1', 0)
    .attr('y2', 185)
    .attr('stroke', '#EA4335')
    .attr('stroke-width', 2)
    .attr('stroke-dasharray', '5,3');

  // Milestone diamonds
  d3.select(container)
    .selectAll('polygon.milestone')
    .data(milestones.filter(m => m.type === 'PoC' || m.type === 'Production'))
    .join('polygon')
    .attr('points', d => {
      const cx = x(new Date(d.date));
      const cy = workstreamY(d.workstreamId);
      const r = 11;
      return `${cx},${cy-r} ${cx+r},${cy} ${cx},${cy+r} ${cx-r},${cy}`;
    })
    .attr('fill', d => d.type === 'PoC' ? '#F4B400' : '#34A853')
    .attr('filter', 'url(#sh)');
}
```

This directly mirrors the SVG structure in the design reference — same elements, same attributes, but data-driven.

#### Chart.js 4.4.x (Rejected for Timeline)

| Aspect | Detail |
|---|---|
| **Chart types** | Bar, Line, Pie, Doughnut, Radar, Scatter, Bubble. No Gantt. No timeline. |
| **Plugin: chartjs-plugin-annotation** | Can draw lines and boxes on a chart. Cannot render diamond polygons with drop shadows. |
| **Custom rendering** | Chart.js exposes a Canvas 2D context for custom drawing. But this is Canvas — same limitations as Sub-Question 1. |
| **Gantt plugins** | No official Gantt chart type. Third-party plugins like `chartjs-chart-gantt` have <100 GitHub stars and are unmaintained. |

**Verdict: Reject for timeline.** Chart.js is designed for statistical charts, not timeline visualizations with custom marker shapes.

#### Chart.js 4.4.x (Rejected for Heatmap Too)

The heatmap grid is **not a chart** — it's a CSS Grid layout with styled divs. No charting library should be used for this. The heatmap is pure DOM + CSS.

#### Custom SVG Without D3 (Viable but Slower to Build)

| Aspect | Detail |
|---|---|
| **Approach** | Manually create SVG elements via `document.createElementNS()` |
| **Scale calculations** | Write date-to-pixel mapping functions by hand |
| **Data binding** | Manual DOM diffing (create, update, remove elements) |

**Verdict: Viable for a simple static render, but D3's data joins (`selectAll().data().join()`) save significant code for updates, transitions, and edge cases.** The ~17KB bundle cost of D3 modules pays for itself in development velocity.

### Concrete Recommendation

**Timeline: D3.js v7.9 with selective module imports.**

```typescript
// Import only what's needed — tree-shaking eliminates the rest
import { select, selectAll } from 'd3-selection';
import { scaleTime, scaleLinear } from 'd3-scale';
import { timeMonth, timeDay } from 'd3-time';
import { timeFormat } from 'd3-time-format';
import { axisBottom } from 'd3-axis';
```

**Heatmap: Pure TypeScript DOM manipulation. No library.**

```typescript
export function renderHeatmap(
  container: HTMLElement,
  data: HeatmapData
) {
  const grid = document.createElement('div');
  grid.className = 'hm-grid';

  // Headers
  grid.appendChild(createCornerCell());
  data.months.forEach(m => grid.appendChild(createColHeader(m)));

  // Rows
  for (const status of ['Shipped', 'InProgress', 'Carryover', 'Blocked'] as const) {
    grid.appendChild(createRowHeader(status));
    data.months.forEach(month => {
      const items = data.items.filter(i => i.status === status && i.month === month.name);
      grid.appendChild(createDataCell(status, month, items));
    });
  }

  container.appendChild(grid);
}
```

The heatmap CSS is already written in the design reference — just copy it to `dashboard.css` and the TypeScript creates the DOM nodes that the CSS Grid styles.

---

## Sub-Question 6: ADO Data Ingestion Strategy

**Question:** What data ingestion strategy should be used to pull work-item data from ADO backlogs into local storage without cloud service dependencies?

### Key Findings

Azure DevOps provides two APIs for work item data:

| API | URL Pattern | Authentication | Rate Limits |
|---|---|---|---|
| **REST API v7.1** | `https://dev.azure.com/{org}/{project}/_apis/wit/wiql` | PAT (Personal Access Token) or OAuth | 200 requests/min per user |
| **OData Analytics** | `https://analytics.dev.azure.com/{org}/{project}/_odata/v4.0-preview/WorkItems` | PAT | More generous; designed for bulk reads |

### Recommended: WIQL + Batch REST API

**Step 1: Execute a WIQL query to get matching work item IDs**

```http
POST https://dev.azure.com/{org}/{project}/_apis/wit/wiql?api-version=7.1
Authorization: Basic {base64(:PAT)}
Content-Type: application/json

{
  "query": "SELECT [System.Id] FROM WorkItems WHERE [System.AreaPath] UNDER 'ProjectName\\Privacy Automation' AND [System.ChangedDate] >= @Today - 180 ORDER BY [System.ChangedDate] DESC"
}
```

Response: array of work item IDs (fast — returns only IDs, not full items).

**Step 2: Batch-fetch work item details (up to 200 per call)**

```http
POST https://dev.azure.com/{org}/{project}/_apis/wit/workitemsbatch?api-version=7.1
Authorization: Basic {base64(:PAT)}
Content-Type: application/json

{
  "ids": [1234, 1235, 1236, ...],
  "fields": ["System.Id", "System.Title", "System.State", "System.IterationPath",
             "System.Tags", "System.CreatedDate", "System.ChangedDate"]
}
```

This minimizes API calls. 500 work items = 1 WIQL call + 3 batch calls = 4 total requests.

### C# Implementation

```csharp
public class AdoSyncService
{
    private readonly HttpClient _http;
    private readonly DashboardDbContext _db;
    private readonly ILogger<AdoSyncService> _logger;

    public AdoSyncService(HttpClient http, DashboardDbContext db, ILogger<AdoSyncService> logger)
    {
        _http = http;
        _db = db;
        _logger = logger;

        _http.BaseAddress = new Uri("https://dev.azure.com/{org}/{project}/");
        // PAT is configured via IHttpClientFactory named client + appsettings
    }

    public async Task<SyncResult> SyncAsync(CancellationToken ct = default)
    {
        // 1. Run WIQL to get IDs
        var ids = await ExecuteWiqlAsync(ct);

        // 2. Batch-fetch in chunks of 200
        var workItems = new List<AdoWorkItem>();
        foreach (var chunk in ids.Chunk(200))
        {
            var batch = await FetchWorkItemBatchAsync(chunk, ct);
            workItems.AddRange(batch);
        }

        // 3. Map ADO states → dashboard categories
        var mapped = workItems.Select(MapToWorkItem).ToList();

        // 4. Upsert into SQLite
        await UpsertWorkItemsAsync(mapped, ct);

        return new SyncResult(workItems.Count, DateTime.UtcNow);
    }

    private WorkItem MapToWorkItem(AdoWorkItem ado)
    {
        var status = ado.State switch
        {
            "Closed" or "Resolved" or "Done" => "Shipped",
            "Active" or "Committed" => "InProgress",
            _ when ado.Tags.Contains("blocked", StringComparison.OrdinalIgnoreCase) => "Blocked",
            _ when ado.Tags.Contains("carryover", StringComparison.OrdinalIgnoreCase) => "Carryover",
            _ => "InProgress"
        };

        var month = ado.IterationPath.Split('\\').LastOrDefault() ?? "Unknown";

        return new WorkItem
        {
            Id = ado.Id.ToString(),
            Title = ado.Title,
            Status = status,
            Month = month,
            AdoUrl = $"https://dev.azure.com/{org}/{project}/_workitems/edit/{ado.Id}",
            LastSyncedUtc = DateTime.UtcNow.ToString("O")
        };
    }
}
```

### State Mapping Strategy

This is a critical design decision. The design has four categories:

| Dashboard Status | ADO State Mapping | Fallback |
|---|---|---|
| **Shipped** | State = "Closed", "Resolved", "Done" | — |
| **In Progress** | State = "Active", "Committed", "In Progress" | Default for unrecognized states |
| **Carryover** | Tag contains "carryover" OR iteration < current iteration AND state ≠ Closed | Requires iteration comparison logic |
| **Blocked** | Tag contains "blocked" OR has linked blocker work item | Check tags first, then linked items |

**Open question for stakeholders:** The carryover detection needs clarification. Options:
- **Tag-based:** Team manually tags items as "carryover." Simple but requires discipline.
- **Iteration-based:** Items in past iterations that aren't closed are automatically carryover. Automated but may miscategorize paused items.
- **Hybrid:** Use iteration-based as default, allow tag override. Recommended.

### PAT Storage

```csharp
// appsettings.json (template — PAT value comes from user-secrets)
{
  "Ado": {
    "Organization": "msazure",
    "Project": "One",
    "AreaPath": "One\\Privacy Automation",
    "Pat": "" // DO NOT commit. Use: dotnet user-secrets set "Ado:Pat" "your-pat-here"
  }
}
```

```csharp
// Registration in Program.cs
builder.Services.AddHttpClient<AdoSyncService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var org = config["Ado:Organization"];
    var project = config["Ado:Project"];
    client.BaseAddress = new Uri($"https://dev.azure.com/{org}/{project}/_apis/");

    var pat = config["Ado:Pat"];
    var authBytes = Encoding.ASCII.GetBytes($":{pat}");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
});
```

### "No Cloud Services" Constraint

The spec says "no cloud services." The ADO REST API is an external HTTP call, not a cloud service dependency in the architectural sense. The dashboard:
- Stores no data in the cloud
- Runs no cloud-hosted compute
- Has no cloud billing
- Functions offline after initial sync (reads from local SQLite)

The ADO API call is analogous to `git fetch` — it's a data pull from an existing corporate tool, not a cloud service integration.

---

## Sub-Question 7: Cross-Stack Testing Strategy

**Question:** What testing strategy spans both the C# backend and TypeScript frontend within a single CI pipeline?

### Key Findings

The project has four testable layers:

| Layer | Technology | Test Type | Tools |
|---|---|---|---|
| Data access | EF Core + SQLite | Integration | xUnit + in-memory SQLite |
| API endpoints | Minimal API | Integration | xUnit + WebApplicationFactory |
| ADO sync service | HttpClient | Unit (mocked HTTP) | xUnit + `MockHttpMessageHandler` |
| Frontend rendering | TypeScript + D3 | Unit | Vitest + jsdom |
| Visual regression | Full page | E2E (deferred) | Playwright (Phase 2) |

### Backend Testing: xUnit 2.8+

**Packages:**

| Package | Version | Purpose |
|---|---|---|
| `xunit` | 2.8.x | Test framework |
| `xunit.runner.visualstudio` | 2.8.x | VS Test Explorer integration |
| `Microsoft.NET.Test.Sdk` | 17.x | Test host |
| `FluentAssertions` | 6.12.x | Readable assertions |
| `NSubstitute` | 5.1.x | Mocking |
| `Microsoft.AspNetCore.Mvc.Testing` | 8.0.x | `WebApplicationFactory` |

**API integration test example:**

```csharp
public class RoadmapEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RoadmapEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace SQLite with in-memory for test isolation
                services.RemoveAll<DbContextOptions<DashboardDbContext>>();
                services.AddDbContext<DashboardDbContext>(opt =>
                    opt.UseInMemoryDatabase("TestDb"));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetRoadmap_ReturnsWorkstreamsAndMilestones()
    {
        var response = await _client.GetAsync("/api/roadmap");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var roadmap = await response.Content.ReadFromJsonAsync<RoadmapDto>();
        roadmap.Should().NotBeNull();
        roadmap!.Workstreams.Should().NotBeEmpty();
    }
}
```

**ADO sync service unit test (mocked HTTP):**

```csharp
[Fact]
public async Task SyncAsync_MapsClosedItemsToShipped()
{
    // Arrange
    var handler = new MockHttpMessageHandler();
    handler.When("*/_apis/wit/wiql*")
        .Respond("application/json", """{"workItems":[{"id":1}]}""");
    handler.When("*/_apis/wit/workitemsbatch*")
        .Respond("application/json", """{"value":[{"id":1,"fields":{"System.Title":"Test","System.State":"Closed","System.Tags":""}}]}""");

    var httpClient = handler.ToHttpClient();
    var svc = new AdoSyncService(httpClient, _db, _logger);

    // Act
    await svc.SyncAsync();

    // Assert
    var item = await _db.WorkItems.FindAsync("1");
    item!.Status.Should().Be("Shipped");
}
```

### Frontend Testing: Vitest 1.x

**Packages:** (in `package.json` devDependencies)

```json
{
  "devDependencies": {
    "vitest": "^1.6.0",
    "@vitest/coverage-v8": "^1.6.0",
    "jsdom": "^24.1.0"
  }
}
```

**Vitest config:**

```typescript
// vitest.config.ts
import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    environment: 'jsdom',  // provides document, window for DOM tests
    globals: true,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov'],
      include: ['src/**/*.ts'],
      exclude: ['src/**/*.d.ts'],
    },
  },
});
```

**Frontend unit test examples:**

```typescript
// timeline.test.ts
import { describe, it, expect, beforeEach } from 'vitest';
import { renderTimeline } from '@/components/timeline';

describe('renderTimeline', () => {
  let svg: SVGSVGElement;

  beforeEach(() => {
    document.body.innerHTML = '<svg id="timeline" width="1560" height="185"></svg>';
    svg = document.getElementById('timeline') as unknown as SVGSVGElement;
  });

  it('renders month grid lines', () => {
    renderTimeline(svg, mockWorkstreams, mockMilestones, [
      new Date('2026-01-01'),
      new Date('2026-07-01'),
    ]);

    const lines = svg.querySelectorAll('line.month-grid');
    expect(lines.length).toBe(6); // Jan–Jun
  });

  it('renders NOW line at current date position', () => {
    renderTimeline(svg, mockWorkstreams, mockMilestones, [
      new Date('2026-01-01'),
      new Date('2026-07-01'),
    ]);

    const nowLine = svg.querySelector('line.now-line');
    expect(nowLine).not.toBeNull();
    expect(nowLine?.getAttribute('stroke')).toBe('#EA4335');
  });

  it('renders diamond polygons for PoC milestones', () => {
    renderTimeline(svg, mockWorkstreams, [
      { id: 'm1', workstreamId: 'M1', name: 'PoC', date: '2026-03-26', type: 'PoC' },
    ], [new Date('2026-01-01'), new Date('2026-07-01')]);

    const diamonds = svg.querySelectorAll('polygon.milestone');
    expect(diamonds.length).toBe(1);
    expect(diamonds[0].getAttribute('fill')).toBe('#F4B400');
  });
});
```

### CI Pipeline: GitHub Actions

```yaml
# .github/workflows/ci.yml
name: CI
on: [push, pull_request]

jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet test --no-build -c Release --verbosity normal

  frontend:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: src/ReportingDashboard.Client
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: src/ReportingDashboard.Client/package-lock.json
      - run: npm ci
      - run: npm run lint
      - run: npm run test -- --reporter=verbose
      - run: npm run build  # ensures production build doesn't break
```

**Backend and frontend jobs run in parallel.** Total CI time: ~2 minutes (whichever job is slower).

### Visual Regression Testing (Phase 2)

When the dashboard is stable, add Playwright screenshot tests:

```typescript
// tests/visual/dashboard.spec.ts
import { test, expect } from '@playwright/test';

test('dashboard matches design reference', async ({ page }) => {
  await page.goto('http://localhost:5000');
  await page.waitForSelector('.hm-grid');

  // Compare against golden screenshot
  await expect(page).toHaveScreenshot('dashboard-1920x1080.png', {
    maxDiffPixelRatio: 0.01,  // allow 1% pixel diff for anti-aliasing
  });
});
```

This catches visual regressions by comparing against the design reference screenshot. Defer until Phase 4 — it requires a stable visual baseline.

---

## Sub-Question 8: Security Model for Local Tool

**Question:** What security model is appropriate for a local-only tool that may handle internal roadmap data and ADO credentials?

### Key Findings

The security surface is small but non-trivial:

| Asset | Sensitivity | Threat |
|---|---|---|
| **ADO PAT** | High — grants read/write access to ADO | Accidental commit to source control; leaked via logs |
| **SQLite database** | Medium — internal roadmap titles, work item IDs | Another user on shared machine could read the file |
| **HTTP API** | Low — runs on localhost only | Malicious local process could query the API |
| **Frontend assets** | None — static HTML/JS/CSS | No secrets in frontend code |

### PAT Protection

**Layer 1: .NET User Secrets (Development)**

```bash
cd src/ReportingDashboard.Api
dotnet user-secrets init
dotnet user-secrets set "Ado:Pat" "ghp_xxxxxxxxxxxxxxxxxxxx"
```

Stores the PAT in `%APPDATA%\Microsoft\UserSecrets\{guid}\secrets.json` — outside the repo directory. Cannot be accidentally committed.

**Layer 2: Environment Variable (Production/Distribution)**

```bash
set REPORTINGDASHBOARD_ADO__PAT=ghp_xxxxxxxxxxxxxxxxxxxx
dotnet run
```

.NET configuration binds `Ado:Pat` from environment variable `REPORTINGDASHBOARD_ADO__PAT` automatically (double underscore = section separator).

**Layer 3: DPAPI Encryption (Windows)**

For a polished distribution, encrypt the PAT at rest:

```csharp
using System.Security.Cryptography;

public static class CredentialStore
{
    private static readonly string CredPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ReportingDashboard", "cred.dat");

    public static void StorePat(string pat)
    {
        var plainBytes = Encoding.UTF8.GetBytes(pat);
        var encrypted = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        Directory.CreateDirectory(Path.GetDirectoryName(CredPath)!);
        File.WriteAllBytes(CredPath, encrypted);
    }

    public static string? LoadPat()
    {
        if (!File.Exists(CredPath)) return null;
        var encrypted = File.ReadAllBytes(CredPath);
        var plainBytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
```

`DataProtectionScope.CurrentUser` means only the Windows user who encrypted it can decrypt it. Other users on the same machine cannot read the PAT.

**Package:** `System.Security.Cryptography.ProtectedData` 8.0.0 (NuGet — not in-box for .NET 8).

### Database Protection

**Default: User-scoped file location**

```
%LOCALAPPDATA%\ReportingDashboard\dashboard.db
```

On Windows, `%LOCALAPPDATA%` is `C:\Users\{username}\AppData\Local` — accessible only to the current user (ACL-protected by default).

**Optional: SQLite encryption**

If the team wants database-at-rest encryption:

```csharp
// Connection string with password (uses SQLCipher under the hood)
"Data Source=dashboard.db;Password=my-secret-key"
```

Package: `Microsoft.Data.Sqlite` 8.0.x supports the `Password` connection string parameter when paired with `SQLitePCLRaw.bundle_e_sqlcipher` (NuGet). This encrypts the entire database file with AES-256-CBC.

**Recommendation: Defer encryption.** The database contains work item titles and IDs — not credentials, PII, or financial data. File-system ACLs provide adequate protection for an internal tool. Add SQLCipher if a security review requires it.

### Localhost Binding

```csharp
// Program.cs — ensure Kestrel only binds to localhost
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // IPv4 127.0.0.1 + IPv6 ::1 only
});
```

This prevents the dashboard from being accessible to other machines on the network. Even if the user's firewall is misconfigured, Kestrel rejects non-localhost connections.

### Git Protection

```gitignore
# .gitignore additions for security
*.db
*.db-wal
*.db-shm
cred.dat
appsettings.*.json
!appsettings.json
!appsettings.Development.json
```

**Pre-commit hook (optional but recommended):**

```bash
#!/bin/sh
# .git/hooks/pre-commit — block PAT commits
if git diff --cached --diff-filter=ACM | grep -qiE '(pat|token|password|secret)\s*[:=]\s*["'"'"'][A-Za-z0-9+/=]{20,}'; then
  echo "ERROR: Potential secret detected in staged changes. Aborting commit."
  exit 1
fi
```

### Security Summary

| Layer | Protection | Implementation Cost |
|---|---|---|
| PAT in user-secrets | Keeps PAT out of repo | 5 minutes (built-in) |
| PAT in env var | Runtime injection | 0 (built-in) |
| DPAPI encryption | At-rest PAT protection | 30 minutes |
| Localhost binding | Network isolation | 2 lines of code |
| .gitignore | Prevents accidental commits | 5 minutes |
| SQLCipher | Database encryption | Deferred |

**Total security implementation effort: ~1 hour.** All using .NET built-in or well-established libraries. No custom crypto, no third-party auth services, no cloud key vaults.

---

## Summary: Technology Decision Matrix

| Sub-Question | Decision | Confidence | Key Package |
|---|---|---|---|
| 1. Frontend rendering | SVG + CSS Grid + DOM (not Canvas/Phaser) | **Very High** | Native browser APIs |
| 2. Backend architecture | ASP.NET Core 8 Minimal API | **Very High** | `Microsoft.AspNetCore.App` 8.0 |
| 3. Data storage | SQLite via EF Core 8 | **High** | `Microsoft.EntityFrameworkCore.Sqlite` 8.0.x |
| 4. .sln integration | Vite + MSBuild targets + SPA proxy | **High** | `Microsoft.AspNetCore.SpaProxy` 8.0.x |
| 5. Visualization | D3.js for timeline; pure CSS for heatmap | **Very High** | `d3` 7.9.x, `@types/d3` 7.4.x |
| 6. ADO ingestion | WIQL + batch REST API via HttpClient | **High** | `System.Net.Http` (in-box) |
| 7. Testing | xUnit + WebApplicationFactory + Vitest + jsdom | **Very High** | See packages above |
| 8. Security | User-secrets + DPAPI + localhost binding | **High** | `System.Security.Cryptography.ProtectedData` 8.0.x |

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/13e558a1eadaf4c29841c3e2e2a72132ef362199/AgentDocs/testbranch/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
