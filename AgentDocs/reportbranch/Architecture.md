# Architecture

## Overview & Goals

The ReportingDashboard is a self-contained, full-stack web application that renders project management data as an immersive 3D "command center" experience. It consists of two processes—a **React/Three.js frontend** served by Vite and a **Node.js/Express backend** serving mock JSON data—launched together via a single `npm run dev` command.

**Architecture style:** Client-server monorepo with REST API. No database, no authentication, no external dependencies.

**Primary goals the architecture must satisfy:**

1. **Visual impact** — Cinematic 3D scene with bloom, particles, glassmorphism, and animated camera transitions at ≥30 fps on integrated GPUs.
2. **Zero-friction setup** — Clone → `npm install && npm run dev` → running app in under 2 minutes.
3. **Future extensibility** — Mock data layer can be swapped for real APIs (Azure DevOps, Jira) without touching visualization components.
4. **Performance** — <100 WebGL draw calls/frame, <2 MB gzipped bundle, <50 ms API response times.

---

## System Components

### 1. Express Backend (`server/`)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Serve 7 REST endpoints returning typed mock JSON data |
| **Runtime** | Node.js 22 LTS, executed via `tsx` (no compile step) |
| **Port** | 3001 (configurable via `PORT` env var) |
| **Dependencies** | `express@5.1.x`, `cors@2.8.x`, `tsx@4.19.x` |
| **Data source** | In-memory TypeScript mock data factory (`server/data/mockData.ts`) |
| **Interfaces** | HTTP REST JSON API consumed by the frontend |

**Internal structure:**

```
server/
├── index.ts                  # Express app bootstrap, CORS, error middleware
├── routes/
│   ├── projectRoutes.ts      # /api/project-summary, /api/project-items
│   ├── sprintRoutes.ts       # /api/sprint-metrics
│   ├── riskRoutes.ts         # /api/risks
│   ├── teamRoutes.ts         # /api/team-activity
│   ├── roadmapRoutes.ts      # /api/roadmap
│   └── reportRoutes.ts       # /api/report/:id
├── data/
│   ├── mockData.ts           # Master factory: generateMockData() → AllData
│   ├── types.ts              # Shared TypeScript interfaces (also exported to client)
│   └── seed.ts               # Deterministic seed utilities
└── middleware/
    └── errorHandler.ts       # Centralized error → JSON response
```

### 2. React Frontend (`client/`)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render 3D scene, 2D overlays, charts, and interactive UI |
| **Build tool** | Vite 6.3.x with HMR |
| **Port** | 5173 (Vite default) |
| **API access** | Vite dev proxy forwards `/api/*` → `http://localhost:3001` |
| **Key libraries** | React 19, Three.js 0.175, R3F 9.1, Chart.js 4.5, Tailwind 4.1, Framer Motion 12, GSAP 3.12 |

**Internal structure:**

```
client/
├── index.html
├── vite.config.ts             # Proxy config, Tailwind plugin, build optimization
├── tailwind.config.ts
├── tsconfig.json
└── src/
    ├── main.tsx               # React root mount
    ├── App.tsx                # Layout orchestrator: Canvas + HTML overlay
    ├── api/
    │   └── client.ts          # Typed fetch wrapper with error handling
    ├── hooks/
    │   ├── useProjectData.ts  # Fetches & caches all dashboard data
    │   ├── useAnimatedValue.ts # GSAP-powered animated counter hook
    │   └── useCameraFocus.ts  # Camera fly-to-target hook
    ├── store/
    │   └── dashboardStore.ts  # Lightweight React context for shared state
    ├── scene/                 # R3F 3D components (one per file)
    │   ├── SceneSetup.tsx     # Lights, fog, environment map
    │   ├── CameraController.tsx # Fly-in animation + click-to-focus
    │   ├── ParticleBackground.tsx # Instanced mesh particle field
    │   ├── ProjectHierarchy.tsx   # 3D force-directed node graph
    │   ├── RiskRadar.tsx      # Orbital risk visualization
    │   ├── TimelinePath.tsx   # TubeGeometry roadmap
    │   └── PostProcessing.tsx # Selective bloom via layers
    ├── components/            # HTML overlay components
    │   ├── DashboardCards.tsx  # Glassmorphism project overview cards
    │   ├── DetailPanel.tsx    # Slide-in detail panel
    │   ├── SprintCharts.tsx   # Chart.js burndown & velocity
    │   ├── ActivityFeed.tsx   # Scrollable team activity
    │   ├── LoadingScreen.tsx  # Initial load state
    │   └── WebGLFallback.tsx  # No-WebGL2 error message
    ├── types/
    │   └── index.ts           # Frontend type definitions (mirrors server types)
    └── styles/
        └── globals.css        # Tailwind directives, glassmorphism utilities, fonts
```

### 3. Root Workspace (`/`)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Monorepo orchestration, shared config, documentation |
| **Key files** | `package.json` (workspace scripts), `tsconfig.json` (base config), `README.md` |
| **Dev command** | `npm run dev` → `concurrently "npm run dev:server" "npm run dev:client"` |

```
/
├── package.json               # Root: scripts, devDependencies (concurrently, prettier, eslint)
├── tsconfig.json              # Base TS config extended by client/ and server/
├── .prettierrc
├── eslint.config.js
├── README.md
├── server/                    # Backend (see above)
└── client/                    # Frontend (see above)
```

### 4. Shared Type System

TypeScript interfaces for all data entities are defined in `server/data/types.ts` and re-exported. The client imports these types directly via a TypeScript path alias (`@shared/types`), ensuring compile-time contract enforcement between backend responses and frontend consumers. No code-generation step is needed since both sides run TypeScript.

---

## Component Interactions

### Runtime Data Flow

```
┌─────────────────────────────────────────────────────────┐
│                      Browser                            │
│                                                         │
│  ┌─────────────┐    ┌──────────────┐    ┌───────────┐  │
│  │  App.tsx     │───▶│ useProject   │───▶│ api/      │  │
│  │  (Layout)    │    │ Data.ts      │    │ client.ts │  │
│  └──────┬───┬──┘    └──────┬───────┘    └─────┬─────┘  │
│         │   │              │                  │         │
│    ┌────▼┐ ┌▼─────┐  ┌────▼─────┐     fetch(/api/*)   │
│    │R3F  │ │HTML   │  │Dashboard │            │         │
│    │Canvas│ │Overlay│  │Store     │            │         │
│    │     │ │       │  │(Context) │            │         │
│    └─────┘ └───────┘  └──────────┘            │         │
└───────────────────────────────────────────────┼─────────┘
                                                │
                              Vite proxy (/api/* → :3001)
                                                │
┌───────────────────────────────────────────────▼─────────┐
│                Express Backend (:3001)                   │
│                                                         │
│  Router ──▶ Route Handler ──▶ mockData (in-memory)     │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### Startup Sequence

1. `npm run dev` invokes `concurrently`:
   - **Server**: `tsx server/index.ts` → Express listens on `:3001`
   - **Client**: `vite dev` → Vite serves on `:5173` with proxy to `:3001`
2. Browser opens `http://localhost:5173`
3. `index.html` loads → React mounts → `<Canvas>` initializes WebGL context
4. `CameraController` plays GSAP fly-in animation (2-4 sec)
5. `useProjectData` hook fires parallel `fetch()` calls to all 6 list endpoints
6. Data arrives → React state updates → 3D scene and HTML overlays render
7. User clicks a node → `dashboardStore` updates `selectedItemId` → `DetailPanel` slides in → `fetch(/api/report/:id)` loads detail

### Inter-Component Communication

| From | To | Mechanism |
|------|----|-----------|
| HTML overlay click | 3D camera | `dashboardStore.focusTarget` → `CameraController` reads via context and animates |
| 3D node click | Detail panel | R3F `onClick` handler → `dashboardStore.selectedItemId` → `DetailPanel` reacts |
| API client | All consumers | `useProjectData` hook returns `{ data, loading, error }` tuple |
| Performance monitor | Particle system | `drei/PerformanceMonitor` → reduces particle count dynamically |

---

## Data Model

### Entity Types

```typescript
// Shared between server and client

type ItemStatus = 'done' | 'in-progress' | 'blocked' | 'not-started' | 'at-risk';
type ItemType = 'epic' | 'feature' | 'story' | 'task' | 'bug';
type RiskSeverity = 'critical' | 'high' | 'medium' | 'low';
type ActivityEventType = 'pr-completed' | 'task-completed' | 'comment' | 'deployment' | 'review';

interface ProjectSummary {
  id: string;
  name: string;
  status: string;
  currentSprint: string;
  completionPercent: number;        // 0-100
  deliveryConfidence: number;       // 0-100
  daysRemaining: number;
  healthScore: number;              // 0-100
  healthColor: 'green' | 'yellow' | 'red';
  totalEpics: number;
  totalFeatures: number;
  totalStories: number;
}

interface ProjectItem {
  id: string;
  type: ItemType;
  title: string;
  description: string;
  status: ItemStatus;
  parentId: string | null;          // null for epics
  owner: string;
  priority: 'critical' | 'high' | 'medium' | 'low';
  storyPoints: number;
  remainingWork: number;            // hours
  dependencies: string[];           // item IDs
  recentActivity: ActivityEvent[];  // last 3 events
}

interface SprintMetrics {
  sprintName: string;
  sprintNumber: number;
  startDate: string;                // ISO 8601
  endDate: string;
  velocity: {
    sprints: string[];              // sprint labels
    planned: number[];
    completed: number[];
  };
  burndown: {
    days: string[];                 // day labels
    ideal: number[];
    actual: number[];
  };
  openBugs: number;
  blockers: number;
  carryoverItems: number;
}

interface Risk {
  id: string;
  title: string;
  description: string;
  severity: RiskSeverity;
  owner: string;
  status: 'open' | 'mitigated' | 'closed';
  category: string;
  impactArea: string;
  mitigationPlan: string;
}

interface TeamActivity {
  events: ActivityEvent[];
  teamMembers: TeamMember[];
}

interface ActivityEvent {
  id: string;
  type: ActivityEventType;
  actor: string;
  actorAvatar: string;              // initials or color
  description: string;
  timestamp: string;                // ISO 8601
  relatedItemId: string | null;
}

interface TeamMember {
  id: string;
  name: string;
  role: string;
  avatar: string;
}

interface RoadmapMilestone {
  id: string;
  title: string;
  date: string;                     // ISO 8601
  phase: 'completed' | 'active' | 'upcoming';
  type: 'release' | 'milestone' | 'sprint-boundary';
  description: string;
}

interface Roadmap {
  milestones: RoadmapMilestone[];
  sprintBoundaries: { date: string; label: string }[];
}

interface ReportDetail {
  id: string;
  type: ItemType | 'risk';
  title: string;
  description: string;
  owner: string;
  status: string;
  priority: string;
  estimate: number | null;
  remainingWork: number | null;
  dependencies: { id: string; title: string; status: string }[];
  recentActivity: ActivityEvent[];
  metadata: Record<string, string>; // flexible key-value for extra fields
}
```

### Entity Relationships

```
ProjectSummary (1)
    │
    ├── Epic (4+)
    │     └── Feature (3+ per epic)
    │           └── Story/Task (3+ per feature)
    │
    ├── Risk (8+)
    │
    ├── RoadmapMilestone (6+)
    │
    └── ActivityEvent (20+)
          └── references TeamMember (10+)
          └── optionally references ProjectItem
```

### Minimum Data Volumes

| Entity | Minimum Count |
|--------|--------------|
| Epics | 4 |
| Features | 12 (3 per epic) |
| Stories/Tasks | 40+ |
| Risks | 8 |
| Team Members | 10 |
| Activity Events | 20 |
| Roadmap Milestones | 6 |

### Storage

All data is generated by `server/data/mockData.ts` at server start and held in memory. The factory uses deterministic logic (no random seeds) so the same data is returned on every restart. Data is structured as typed arrays and a lookup map:

```typescript
interface AllMockData {
  projectSummary: ProjectSummary;
  projectItems: ProjectItem[];
  sprintMetrics: SprintMetrics;
  risks: Risk[];
  teamActivity: TeamActivity;
  roadmap: Roadmap;
  itemIndex: Map<string, ReportDetail>;  // universal ID → detail lookup
}
```

---

## API Contracts

All endpoints are prefixed with `/api`, return `Content-Type: application/json`, and follow consistent error shapes.

### Standard Error Response

```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Item with id 'xyz' not found"
  }
}
```

### Endpoints

#### `GET /api/project-summary`

Returns high-level project health indicators.

**Response** `200 OK`:
```json
{
  "id": "proj-001",
  "name": "Project Phoenix",
  "status": "In Progress",
  "currentSprint": "Sprint 14",
  "completionPercent": 67,
  "deliveryConfidence": 78,
  "daysRemaining": 8,
  "healthScore": 72,
  "healthColor": "yellow",
  "totalEpics": 4,
  "totalFeatures": 12,
  "totalStories": 48
}
```

#### `GET /api/project-items`

Returns the full hierarchy of epics → features → stories/tasks for the 3D node graph.

**Response** `200 OK`:
```json
{
  "items": [
    {
      "id": "epic-001",
      "type": "epic",
      "title": "User Authentication Platform",
      "status": "in-progress",
      "parentId": null,
      "owner": "Sarah Chen",
      "storyPoints": 89,
      "children": ["feat-001", "feat-002", "feat-003"]
    }
  ]
}
```

The response is a flat array. Parent-child relationships are expressed via `parentId`. The frontend builds the tree client-side for the force-directed layout.

#### `GET /api/sprint-metrics`

Returns velocity history, burndown data, and blocker counts.

**Response** `200 OK`:
```json
{
  "sprintName": "Sprint 14",
  "sprintNumber": 14,
  "startDate": "2026-04-20T00:00:00Z",
  "endDate": "2026-05-03T00:00:00Z",
  "velocity": {
    "sprints": ["S10", "S11", "S12", "S13", "S14"],
    "planned": [34, 40, 38, 42, 40],
    "completed": [31, 38, 35, 40, 28]
  },
  "burndown": {
    "days": ["Day 1", "Day 2", "Day 3", "Day 4", "Day 5", "Day 6", "Day 7", "Day 8", "Day 9", "Day 10"],
    "ideal": [40, 36, 32, 28, 24, 20, 16, 12, 8, 0],
    "actual": [40, 38, 35, 33, 29, 25, 22, null, null, null]
  },
  "openBugs": 7,
  "blockers": 3,
  "carryoverItems": 2
}
```

#### `GET /api/risks`

Returns all project risks and blockers for the radar visualization.

**Response** `200 OK`:
```json
{
  "risks": [
    {
      "id": "risk-001",
      "title": "Third-party API deprecation",
      "description": "Payment provider v2 API sunset in Q3",
      "severity": "critical",
      "owner": "Mike Torres",
      "status": "open",
      "category": "Technical",
      "impactArea": "Revenue Pipeline",
      "mitigationPlan": "Migration to v3 API scheduled for Sprint 16"
    }
  ]
}
```

#### `GET /api/team-activity`

Returns recent team events and team member list.

**Response** `200 OK`:
```json
{
  "events": [
    {
      "id": "evt-001",
      "type": "pr-completed",
      "actor": "Sarah Chen",
      "actorAvatar": "SC",
      "description": "Merged PR #247: Implement OAuth2 refresh flow",
      "timestamp": "2026-04-30T14:32:00Z",
      "relatedItemId": "story-012"
    }
  ],
  "teamMembers": [
    {
      "id": "tm-001",
      "name": "Sarah Chen",
      "role": "Senior Engineer",
      "avatar": "SC"
    }
  ]
}
```

#### `GET /api/roadmap`

Returns milestones and sprint boundaries for the timeline view.

**Response** `200 OK`:
```json
{
  "milestones": [
    {
      "id": "ms-001",
      "title": "Alpha Release",
      "date": "2026-02-15T00:00:00Z",
      "phase": "completed",
      "type": "release",
      "description": "Internal alpha with core features"
    }
  ],
  "sprintBoundaries": [
    { "date": "2026-04-06T00:00:00Z", "label": "Sprint 13" },
    { "date": "2026-04-20T00:00:00Z", "label": "Sprint 14" }
  ]
}
```

#### `GET /api/report/:id`

Returns detailed information for any entity (epic, feature, story, risk) by universal ID.

**Response** `200 OK`:
```json
{
  "id": "feat-003",
  "type": "feature",
  "title": "SSO Integration",
  "description": "Implement SAML-based SSO for enterprise customers",
  "owner": "Alex Kim",
  "status": "in-progress",
  "priority": "high",
  "estimate": 21,
  "remainingWork": 12,
  "dependencies": [
    { "id": "feat-001", "title": "Auth Core", "status": "done" }
  ],
  "recentActivity": [
    {
      "id": "evt-044",
      "type": "task-completed",
      "actor": "Alex Kim",
      "actorAvatar": "AK",
      "description": "Completed SAML metadata parser",
      "timestamp": "2026-04-29T09:15:00Z",
      "relatedItemId": "story-031"
    }
  ],
  "metadata": {
    "sprint": "Sprint 14",
    "epicTitle": "User Authentication Platform"
  }
}
```

**Response** `404 Not Found` (unknown ID):
```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Item with id 'xyz-999' not found"
  }
}
```

### CORS Configuration

In development, the Vite proxy handles cross-origin. The Express server also configures `cors()` as a fallback:

```typescript
app.use(cors({ origin: 'http://localhost:5173' }));
```

---

## Infrastructure Requirements

### Development Environment

| Requirement | Specification |
|-------------|--------------|
| **Node.js** | 22 LTS |
| **GPU** | WebGL2-capable (discrete GPU recommended; recent integrated acceptable) |
| **Browser** | Chrome or Edge latest |
| **OS** | Windows, macOS, or Linux |
| **Disk** | ~500 MB (node_modules) |
| **RAM** | 4 GB minimum |

### Dev Server Architecture

```
npm run dev
    │
    ├── concurrently
    │     ├── tsx server/index.ts          → Express on :3001
    │     └── vite dev                     → Vite on :5173
    │           └── proxy: /api/* → :3001
    │
    └── Browser → http://localhost:5173
```

### Production Build (Optional)

```
npm run build
    │
    ├── vite build → client/dist/    (static assets)
    └── Express serves:
          ├── /api/*     → JSON endpoints
          └── /*         → client/dist/index.html (SPA fallback)
```

### Hosting (If Deployed)

| Tier | Frontend | Backend | Cost |
|------|----------|---------|------|
| **Free** | Vercel / GitHub Pages | Render free tier | $0 |
| **Small** | Azure Static Web Apps | Azure App Service B1 | ~$15/mo |

### CI/CD

Not required for MVP. If added later:

- **GitHub Actions**: `npm ci && npm run lint && npm run build`
- **No test suite** required per spec, but the pipeline should validate that the build succeeds and linting passes.

### Containerization (Optional)

Single-stage Dockerfile:

```dockerfile
FROM node:22-alpine
WORKDIR /app
COPY package*.json ./
COPY server/ ./server/
COPY client/ ./client/
RUN npm ci
RUN cd client && npx vite build
EXPOSE 3001
CMD ["npx", "tsx", "server/index.ts"]
```

Express serves static assets from `client/dist/` in production mode.

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **UI Framework** | React 19 + TypeScript | Required by spec. Mature ecosystem, strong typing, excellent R3F integration. |
| **3D Rendering** | Three.js 0.175 via React Three Fiber 9.1 | Spec mandates Three.js. R3F wraps it declaratively, preventing imperative spaghetti at 7+ scene components. Underlying Three.js remains accessible via refs. |
| **3D Utilities** | @react-three/drei 10.x | 200+ helper components (Float, OrbitControls, Html, Text, PerformanceMonitor). Eliminates boilerplate. |
| **Post-processing** | @react-three/postprocessing 3.x | Selective bloom via Three.js layers. Only glowing objects are bloomed, preventing UI washout. |
| **2D Charts** | Chart.js 4.5 + react-chartjs-2 5.3 | Spec recommends Chart.js. Lightweight, well-documented, renders in HTML overlay (not WebGL). |
| **Animation** | GSAP 3.12 | Camera fly-in, animated counters, timeline sequencing. Superior easing and timeline API. **License note:** Free for non-commercial use; verify if used in customer-facing contexts. Fallback: `@react-spring/three` (MIT). |
| **2D Animation** | Framer Motion 12 | Panel slide-in/out, hover effects, list entry transitions. Declarative API integrates naturally with React. |
| **Styling** | Tailwind CSS 4.1 | Rapid glassmorphism prototyping (`backdrop-blur-xl bg-white/10`). Dark mode built-in. |
| **Typography** | @fontsource/inter | Clean, professional, variable-weight font. Self-hosted, no external CDN. |
| **Backend** | Express 5.1 on Node.js 22 | Required by spec. Minimal, well-understood, zero-config for 7 route handlers. |
| **TS Execution** | tsx 4.19 | Runs TypeScript directly in Node without a build step. Faster dev iteration. |
| **Build Tool** | Vite 6.3 | Fast HMR, native ESM, built-in proxy, optimized production builds. |
| **Dev Orchestration** | concurrently 9.x | Single `npm run dev` launches both servers. Simple and proven. |
| **3D Layout** | d3-force-3d | Force-directed graph layout for the project hierarchy. Computes positions; R3F renders them. |
| **Linting** | ESLint 9 (flat config) + Prettier 3.5 | Consistent code quality. Flat config is the modern ESLint standard. |
| **Dev Tuning** | leva 0.10 | GUI panel for adjusting bloom intensity, particle count, camera speed during development. Stripped from production. |

---

## Security Considerations

### Current State (Mock Data Only)

- **No authentication or authorization** — The application serves static mock data with no user accounts, sessions, or tokens.
- **No sensitive data** — All data is fabricated. No PII, credentials, or proprietary information in the codebase.
- **CORS** — Configured to allow only `http://localhost:5173` in development.
- **Input validation** — The `:id` parameter in `/api/report/:id` is validated as a string and looked up in a Map. Unknown IDs return 404. No SQL injection risk (no database).
- **Dependencies** — All npm packages are from well-known, actively maintained open-source projects. Pin exact versions in `package-lock.json`.

### Future State (Real Data Integration)

When mock data is replaced with real APIs, the following must be added:

| Concern | Requirement |
|---------|-------------|
| **Authentication** | API key or OAuth2 bearer token on all `/api/*` endpoints |
| **HTTPS** | TLS termination required; redirect HTTP → HTTPS |
| **CORS** | Lock down to specific production origins |
| **Input validation** | Sanitize `:id` parameter (alphanumeric + hyphens only) |
| **Rate limiting** | Express rate-limit middleware to prevent abuse |
| **Secrets management** | API keys for Azure DevOps / Jira stored in environment variables, never in code |
| **CSP headers** | Content-Security-Policy to restrict script sources |

### WebGL Security

- Three.js WebGL context runs in the browser sandbox. No server-side GPU access.
- No user-uploaded content is rendered in the 3D scene.
- Shader code is hardcoded (no dynamic GLSL evaluation).

---

## Scaling Strategy

### Current Scale (Demo Application)

This application is designed for **single-user, local execution**. No scaling is required for MVP.

- **Data**: <100 entities held in memory (~50 KB). No pagination needed.
- **Concurrent users**: 1 (local development).
- **Compute**: Single Express process handles all requests.

### Scaling Path (If Productized)

| Dimension | Approach |
|-----------|----------|
| **Multiple concurrent viewers** | Express is stateless; deploy behind a load balancer. Each instance loads mock data independently. |
| **Real data with large projects** | Add pagination to `/api/project-items` (skip/take query params). Implement virtual scrolling in the activity feed. Aggregate hierarchy nodes beyond a depth threshold. |
| **3D performance at scale** | `drei/PerformanceMonitor` auto-degrades: reduce particle count, disable bloom, lower geometry detail when FPS drops below threshold. Use LOD (Level of Detail) for distant nodes. |
| **Bundle size** | Code-split 3D scene components with `React.lazy()`. Load Chart.js only when the Sprint Metrics section scrolls into view. Tree-shake unused drei utilities. |
| **API latency (real backends)** | Add a caching layer (in-memory or Redis) between Express and external APIs. Cache TTL of 60 seconds is acceptable for dashboard data. |
| **Global distribution** | Frontend: CDN-hosted static assets. Backend: Deploy to multiple regions or use edge functions for API caching. |

### Performance Budget Enforcement

| Metric | Budget | Enforcement |
|--------|--------|-------------|
| Draw calls/frame | <100 | Use instanced meshes; profile with `gl.getExtension('WEBGL_debug_renderer_info')` |
| Particles | 2000 (discrete GPU), 500 (integrated) | `PerformanceMonitor` auto-adjusts |
| Bundle size (gzipped) | <2 MB | Vite build analyzer; fail CI if exceeded |
| API response time | <50 ms | Express middleware logs response times; mock data guarantees this |

---

## Risks & Mitigations

### High Severity

| # | Risk | Impact | Likelihood | Mitigation |
|---|------|--------|------------|------------|
| 1 | **WebGL performance on integrated GPUs** | Dashboard stutters during executive demos, undermining the core value proposition | High | Use `drei/PerformanceMonitor` to auto-degrade: reduce particle count to 500, disable bloom, simplify geometry. Profile early on Intel Iris hardware. Cap draw calls at 100 via instanced meshes. |
| 2 | **3D hierarchy layout complexity** | Force-directed layout with 56+ nodes in 3D space may produce overlapping or unreadable results | High | Use `d3-force-3d` with tuned charge/link forces. Prototype this component first in isolation. Fall back to deterministic tree layout (concentric circles per hierarchy level) if force layout is unreliable. |
| 3 | **Scope creep on visual polish** | "Premium demo quality" is subjective; team spends unlimited time tweaking effects instead of completing features | Medium | Define a visual baseline with reference screenshots in Phase 1. Treat visual polish as a separate Phase 3 task with a timebox. Use leva GUI for rapid parameter tuning. |

### Medium Severity

| # | Risk | Impact | Likelihood | Mitigation |
|---|------|--------|------------|------------|
| 4 | **GSAP license ambiguity** | If the dashboard is shown to paying customers, GSAP's "no-charge" license may not apply | Medium | Evaluate `@react-spring/three` (MIT) as a drop-in for camera animations. GSAP is technically superior but Spring is license-safe. Make this decision in Phase 1. |
| 5 | **Bloom + HTML overlay interference** | Bloom post-processing can wash out glassmorphism overlays if not layer-isolated | Medium | Use Three.js layers: assign bloom-eligible objects to layer 1, render bloom pass only on that layer. Test this interaction in a prototype before building all sections. |
| 6 | **Three.js version pinning** | Three.js has frequent releases with breaking shader/material changes | Low | Pin exact version (`0.175.0`). Do not use `^` range. Update deliberately with a dedicated PR. |
| 7 | **Cross-browser WebGL differences** | Safari and Firefox may render shaders differently than Chrome | Medium | Use standardized GLSL (no Chrome-specific extensions). Test on Firefox and Safari in Phase 3. Gracefully degrade effects that don't render correctly. |

### Low Severity

| # | Risk | Impact | Likelihood | Mitigation |
|---|------|--------|------------|------------|
| 8 | **R3F learning curve** | Developers unfamiliar with R3F may struggle with the declarative 3D model | Low | Provide team with R3F tutorial links. The drei library eliminates most boilerplate. Raw Three.js is always accessible via refs as escape hatch. |
| 9 | **Mock data staleness** | If the demo is reused months later, dates in mock data look outdated | Low | Use relative date generation in the mock data factory (e.g., "today minus 5 days") so data always looks current. |