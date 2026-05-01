# Architecture

## Overview & Goals

The ReportingDashboard is a full-stack local web application that renders project management data as an immersive 3D command center. It composites a full-viewport React Three Fiber WebGL canvas (particles, bloom, force-directed graphs, orbital visualizations, 3D timelines) with CSS-positioned glassmorphism 2D overlay panels (project overview, sprint metrics, team activity, detail drill-down). An Express 4 backend serves mock JSON from in-memory objects across 7 REST endpoints. The entire system runs locally with `npm install && npm run dev`—zero infrastructure, zero auth, zero external dependencies.

**Architectural Goals:**

1. **Separation of 3D and 2D rendering concerns** — The R3F Canvas owns the WebGL scene graph; HTML/CSS overlay panels own text-heavy UI. They communicate through React Context, never through the Three.js scene.
2. **Single camera controller pattern** — One `CameraController` component manages all camera state (fly-in, click-to-focus, section transitions). No component directly mutates the camera.
3. **API-first mock data** — All data flows through HTTP endpoints, even though it's mock. This makes the future swap to real APIs a backend-only change with zero frontend modifications.
4. **Adaptive visual quality** — The 3D pipeline degrades gracefully (particle count, post-processing passes, shadow resolution) based on measured frame rate via `<PerformanceMonitor>`.
5. **Developer ergonomics** — Single `npm run dev` starts both servers; Leva debug panel exposes all visual tuning parameters at runtime; HMR works for both 2D and 3D components.

---

## System Components

### 1. Express API Server (`server/`)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Serve mock project data as JSON over REST; apply security middleware |
| **Runtime** | Node.js 22 LTS, Express 4.21.x |
| **Port** | 3001 |
| **Dependencies** | `express`, `cors`, `helmet` |
| **Data source** | In-memory TypeScript objects imported from `server/data/mockData.ts` |
| **Interfaces** | 7 GET endpoints (see API Contracts) |

**Internal structure:**

```
server/
├── index.ts              # Express app bootstrap, middleware stack, route mounting
├── middleware/
│   ├── cors.ts           # CORS config (origin whitelist from env or localhost:5173)
│   ├── helmet.ts         # CSP headers with WebGL-compatible policy
│   └── errorHandler.ts   # Global error handler returning { error, message } JSON
├── routes/
│   ├── projectSummary.ts # GET /api/project-summary
│   ├── projectItems.ts   # GET /api/project-items
│   ├── sprintMetrics.ts  # GET /api/sprint-metrics
│   ├── risks.ts          # GET /api/risks
│   ├── teamActivity.ts   # GET /api/team-activity
│   ├── roadmap.ts        # GET /api/roadmap
│   └── report.ts         # GET /api/report/:id (with param validation)
├── data/
│   └── mockData.ts       # Single file: all typed mock data objects
└── types/
    └── index.ts          # Shared TypeScript interfaces (duplicated to client/src/types/)
```

Each route module exports a single `Router` instance. Route handlers are synchronous (no async needed for in-memory data) but use `try/catch` for safety. The `report.ts` route validates the `:id` param against a UUID regex before lookup.

### 2. React Frontend Application (`client/`)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the full dashboard UI: 3D scene + 2D overlay panels |
| **Runtime** | Browser (Chrome 120+, Edge 120+, Firefox 120+, Safari 17+) |
| **Build** | Vite 6.x, TypeScript 5.5.x |
| **Port** | 5173 (Vite dev server) |
| **Key deps** | React 18.3, @react-three/fiber 9.x, @react-three/drei 10.x, @react-three/postprocessing 3.x, swr 4.x, chart.js 4.4, gsap 3.12, d3 7.9 |

The frontend is decomposed into four layers:

#### 2a. Scene Layer (`client/src/scene/`)

Lives inside the R3F `<Canvas>` element. All components here are Three.js scene graph nodes.

| Component | Responsibility |
|-----------|---------------|
| `MainCanvas.tsx` | Root `<Canvas>` with `gl` config (antialias, alpha, powerPreference), `<Suspense>` boundary, performance monitor, and scene children |
| `CameraController.tsx` | Sole owner of camera position/target. Accepts `mode` prop: `'fly-in'`, `'orbit'`, `'focus'`, `'section'`. Uses `@react-spring/three` for interpolation. Exposes `focusOn(position, target)` via ref |
| `ParticleField.tsx` | `<Points>` or `<Stars>` (drei) with configurable count (default 1000, capped via Leva). Continuous drift animation in `useFrame` |
| `PostProcessingStack.tsx` | `<EffectComposer>` wrapping `<Bloom>` (intensity, threshold, radius configurable via Leva), `<Vignette>`, optional `<ChromaticAberration>` |
| `HierarchyGraph.tsx` | 3D force-directed graph using `three-forcegraph`. Receives `nodes[]` and `links[]` from SWR hook. Maps node type to geometry size, status to color. Emits `onNodeClick(id)` |
| `RiskRadar.tsx` | Concentric orbit rings rendered as `<Line>` (drei). Risk nodes orbit at radii mapped to severity. Uses `useFrame` for continuous rotation. Emits `onRiskClick(id)` |
| `Timeline3D.tsx` | Horizontal tube/rail with milestone markers as `<Sphere>` geometries. Sprint boundaries as vertical `<Line>` segments. Labels via `three-spritetext`. Emits `onMilestoneClick(id)` |
| `SceneLighting.tsx` | Ambient + directional + point lights. Soft shadows enabled. Parameters exposed via Leva |

#### 2b. Overlay Layer (`client/src/components/`)

Positioned as CSS `position: fixed` over the Canvas. Uses `pointer-events: none` on the container, `pointer-events: auto` on interactive elements.

| Component | Responsibility |
|-----------|---------------|
| `ProjectOverview.tsx` | Glassmorphism card displaying 7 project summary fields. Contains `<ProgressRing>` (SVG, GSAP-animated) and `<AnimatedCounter>` sub-components |
| `SprintMetrics.tsx` | Glassmorphism card with Chart.js burndown (line), velocity (bar), and planned-vs-completed (bar) charts via `react-chartjs-2`. Animated counters for bug/blocker/carryover counts |
| `TeamActivity.tsx` | Scrollable feed of activity events. GSAP `stagger` animation on mount. Each item shows avatar, name, action, timestamp, target |
| `DetailPanel.tsx` | Right-side slide-out panel (GSAP `x` tween, 300ms). Displays 9 fields for selected item. Triggered by `DashboardContext.selectedItemId`. Fetches from `/api/report/:id` via SWR |
| `LoadingScreen.tsx` | Full-viewport overlay shown during initial Three.js shader compilation. Fades out when `<Canvas>` fires `onCreated` |
| `ErrorBoundary.tsx` | Catches WebGL context loss and React render errors. Shows fallback UI per section |
| `WebGLCheck.tsx` | Checks for WebGL 2.0 support on mount. Renders error message if unavailable |

#### 2c. Shared UI (`client/src/components/ui/`)

| Component | Responsibility |
|-----------|---------------|
| `GlassCard.tsx` | Reusable glassmorphism container: `backdrop-filter: blur(16px)`, translucent bg, subtle border, hover glow via CSS transition |
| `AnimatedCounter.tsx` | GSAP `to()` tween from 0 to target value on mount. Configurable duration, format (integer/percent/decimal) |
| `ProgressRing.tsx` | SVG circle with `stroke-dashoffset` animated via GSAP from 0% to target. Color-coded by threshold |
| `StatusBadge.tsx` | Color-coded pill for status values (Done=green, In Progress=blue, Blocked=red, Not Started=gray, At Risk=orange) |

#### 2d. Data & State Layer (`client/src/api/`, `client/src/context/`)

| Module | Responsibility |
|--------|---------------|
| `api/client.ts` | SWR hook wrappers: `useProjectSummary()`, `useProjectItems()`, `useSprintMetrics()`, `useRisks()`, `useTeamActivity()`, `useRoadmap()`, `useReportDetail(id)`. Each returns `{ data, error, isLoading }` |
| `api/fetcher.ts` | Shared `fetcher` function for SWR: `fetch(url).then(r => r.json())` with error handling |
| `context/DashboardContext.tsx` | React Context + `useReducer`. State shape: `{ selectedItemId: string | null, activeSection: SectionId, detailPanelOpen: boolean, qualityLevel: 'low' | 'medium' | 'high' }` |
| `context/DashboardProvider.tsx` | Provider component wrapping `<App>`. Dispatches: `SELECT_ITEM`, `CLOSE_DETAIL`, `SET_SECTION`, `SET_QUALITY` |

### 3. Root Orchestration (`package.json`)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Monorepo root. Single `npm install` installs both client + server deps. Single `npm run dev` starts both |
| **Tool** | `concurrently` 9.x |
| **Scripts** | `dev`: `concurrently "npm run dev:server" "npm run dev:client"`, `dev:server`: `cd server && nodemon index.ts`, `dev:client`: `cd client && vite` |
| **Install strategy** | npm workspaces (`"workspaces": ["client", "server"]`) or root-level `postinstall` script running `cd client && npm install && cd ../server && npm install` |

### 4. Shared Types (`types/`)

TypeScript interfaces shared between server and client. Duplicated in both `server/types/` and `client/src/types/` (simple copy, no build-time sharing needed for this project size). Alternatively, a `shared/` workspace package.

---

## Component Interactions

### Scene Graph Architecture

```
<Canvas>                              ← R3F root, owns WebGLRenderer
  <CameraController />                ← sole camera owner, reads DashboardContext
  <SceneLighting />                   ← ambient + directional + point lights
  <ParticleField />                   ← background particles (Points geometry)
  <Suspense fallback={null}>
    <HierarchyGraph                   ← force-directed graph (three-forcegraph)
      data={projectItems}
      onNodeClick={dispatch SELECT_ITEM}
    />
    <RiskRadar                        ← orbital risk visualization
      data={risks}
      onRiskClick={dispatch SELECT_ITEM}
    />
    <Timeline3D                       ← horizontal milestone rail
      data={roadmap}
      onMilestoneClick={dispatch SELECT_ITEM}
    />
  </Suspense>
  <PostProcessingStack />             ← Bloom, Vignette (EffectComposer)
</Canvas>
```

### 3D/2D Compositing Strategy

```
┌─────────────────────────────────────────────┐
│  z-index: 0   <Canvas> (position: fixed,    │
│               top:0, left:0, 100vw×100vh)   │
│               WebGL renders here             │
├─────────────────────────────────────────────┤
│  z-index: 10  <OverlayContainer>            │
│               pointer-events: none           │
│  ┌──────────────────────┐ ┌──────────────┐  │
│  │ ProjectOverview      │ │ SprintMetrics│  │
│  │ pointer-events: auto │ │ ptr-events:  │  │
│  │ position: fixed      │ │ auto         │  │
│  │ top: 20px left: 20px │ │ top:20 right │  │
│  └──────────────────────┘ └──────────────┘  │
│  ┌──────────────────────┐                    │
│  │ TeamActivity         │                    │
│  │ bottom-left          │                    │
│  └──────────────────────┘                    │
├─────────────────────────────────────────────┤
│  z-index: 20  <DetailPanel>                  │
│               position: fixed, right: 0      │
│               width: 400px, height: 100vh    │
│               transform: translateX(100%)     │
│               GSAP slides in on SELECT_ITEM   │
├─────────────────────────────────────────────┤
│  z-index: 50  <LoadingScreen>                │
│               Full viewport, fades out        │
└─────────────────────────────────────────────┘
```

**Key rule:** The `<Canvas>` and 2D overlays never share a React tree inside the Canvas. They communicate exclusively through `DashboardContext`. When a 3D node is clicked, the `onClick` handler inside the Canvas dispatches `SELECT_ITEM` to the context. The `DetailPanel` (outside Canvas) reads `selectedItemId` from context and fetches `/api/report/:id`.

### Camera Controller Pattern

`CameraController` is the **single owner** of `camera.position` and the orbit target. No other component writes to the camera. It operates in modes:

| Mode | Trigger | Behavior |
|------|---------|----------|
| `fly-in` | App mount | Animates from `[0, 50, 100]` to default position `[0, 10, 30]` over 3s using `@react-spring/three` |
| `orbit` | Default after fly-in | `<OrbitControls>` (drei) enabled with damping. User can rotate/zoom |
| `focus` | `SELECT_ITEM` dispatch (3D node click) | Spring-animates camera to position the clicked node at center-screen. Orbit controls temporarily disabled |
| `section` | `SET_SECTION` dispatch | Predefined camera positions per section. Spring transition between them |

The controller exposes an imperative `focusOn(worldPos: Vector3)` method via `useImperativeHandle` for programmatic focus requests.

### Data Flow Sequence

```
1. Browser loads → React mounts <App>
2. <DashboardProvider> initializes context (selectedItemId=null, activeSection='overview')
3. <LoadingScreen> renders at z-index 50 (opaque)
4. <Canvas onCreated={() => setLoaded(true)}> starts WebGL init + shader compilation
5. SWR hooks fire in parallel:
   useProjectSummary()  → GET /api/project-summary  → Express → mockData.projectSummary
   useProjectItems()    → GET /api/project-items     → Express → mockData.projectItems
   useSprintMetrics()   → GET /api/sprint-metrics    → Express → mockData.sprintMetrics
   useRisks()           → GET /api/risks             → Express → mockData.risks
   useTeamActivity()    → GET /api/team-activity     → Express → mockData.teamActivity
   useRoadmap()         → GET /api/roadmap           → Express → mockData.roadmap
6. Canvas created → CameraController enters 'fly-in' mode → 3s animation
7. LoadingScreen fades out (GSAP opacity tween, 500ms)
8. 2D panels render with data → AnimatedCounters and ProgressRings GSAP-animate from 0
9. 3D scene populates: HierarchyGraph builds force layout, RiskRadar starts orbiting
10. User clicks 3D node:
    → HierarchyGraph.onNodeClick(id) → dispatch({ type: 'SELECT_ITEM', id })
    → CameraController reads selectedItemId, enters 'focus' mode, animates to node
    → DetailPanel reads selectedItemId, calls useReportDetail(id)
    → GET /api/report/:id → Express validates :id → returns item detail
    → DetailPanel slides in (GSAP translateX 0, 300ms)
11. User clicks close / clicks outside → dispatch CLOSE_DETAIL → DetailPanel slides out
    → CameraController returns to 'orbit' mode
```

### SWR Hook → Component Mapping

| SWR Hook | Consumer Component(s) | Refresh Strategy |
|----------|----------------------|-----------------|
| `useProjectSummary()` | `ProjectOverview` | `revalidateOnFocus: false` (mock data is static) |
| `useProjectItems()` | `HierarchyGraph` | Same |
| `useSprintMetrics()` | `SprintMetrics` | Same |
| `useRisks()` | `RiskRadar` | Same |
| `useTeamActivity()` | `TeamActivity` | Same |
| `useRoadmap()` | `Timeline3D` | Same |
| `useReportDetail(id)` | `DetailPanel` | Fetches on `id` change; `dedupingInterval: 60000` |

SWR is configured globally with:
```typescript
<SWRConfig value={{
  fetcher: (url: string) => fetch(`http://localhost:3001${url}`).then(r => {
    if (!r.ok) throw new Error(`API error: ${r.status}`);
    return r.json();
  }),
  revalidateOnFocus: false,
  revalidateOnReconnect: false,
  shouldRetryOnError: false,
}}>
```

---

## Data Model

### Core Entities

```typescript
// ──── Project Summary ────
interface ProjectSummary {
  id: string;                    // UUID
  name: string;                  // "Project Phoenix"
  status: ProjectStatus;         // 'on-track' | 'at-risk' | 'off-track'
  completionPercentage: number;  // 0-100
  deliveryConfidence: number;    // 0-100
  currentSprint: string;         // "Sprint 14"
  daysRemaining: number;         // 23
  healthScore: number;           // 0-100
  healthThresholds: {
    green: number;               // >= 75
    yellow: number;              // >= 50
    red: number;                 // < 50
  };
}

// ──── Project Items (Hierarchy) ────
interface ProjectItem {
  id: string;                    // UUID
  type: 'epic' | 'feature' | 'story' | 'task' | 'bug';
  title: string;
  description: string;
  status: ItemStatus;            // 'done' | 'in-progress' | 'blocked' | 'not-started' | 'at-risk'
  priority: 'critical' | 'high' | 'medium' | 'low';
  owner: string;                 // Team member name
  parentId: string | null;       // null for epics
  estimate: number;              // Story points
  remainingWork: number;         // Story points
  dependencies: string[];        // Array of item IDs
  recentActivity: ActivityEvent[];
}

// For three-forcegraph consumption:
interface GraphData {
  nodes: ProjectItem[];          // Flat array; type determines visual size
  links: { source: string; target: string }[];  // parentId → id relationships
}

// ──── Sprint Metrics ────
interface SprintMetrics {
  currentSprint: string;
  velocity: SprintVelocity[];    // Last 4+ sprints
  burndown: BurndownPoint[];     // Daily data points for current sprint
  plannedVsCompleted: {
    planned: number;
    completed: number;
    carryover: number;
  };
  openBugs: number;
  blockers: number;
  carryoverItems: number;
}

interface SprintVelocity {
  sprint: string;
  committed: number;
  completed: number;
}

interface BurndownPoint {
  day: number;
  ideal: number;
  actual: number;
}

// ──── Risks ────
interface Risk {
  id: string;
  title: string;
  description: string;
  severity: 'critical' | 'high' | 'medium' | 'low';
  status: 'open' | 'mitigated' | 'closed';
  owner: string;
  category: string;              // "Technical", "Schedule", "Resource", "External"
  mitigation: string;
  impactedItems: string[];       // Item IDs
}

// ──── Team Activity ────
interface ActivityEvent {
  id: string;
  type: 'pr-completed' | 'task-completed' | 'comment' | 'deployment' | 'review';
  actor: TeamMember;
  action: string;                // "merged PR #142"
  target: string;                // Item title or PR name
  targetId: string;              // Item ID for drill-down
  timestamp: string;             // ISO 8601
}

interface TeamMember {
  id: string;
  name: string;
  avatar: string;                // URL or initials-based generated avatar
  role: string;                  // "Senior Engineer", "PM", etc.
}

// ──── Roadmap ────
interface Roadmap {
  milestones: Milestone[];
  sprints: SprintBoundary[];
}

interface Milestone {
  id: string;
  name: string;
  date: string;                  // ISO 8601 date
  type: 'release' | 'milestone' | 'checkpoint';
  status: 'completed' | 'active' | 'upcoming';
  description: string;
  relatedItems: string[];        // Item IDs
}

interface SprintBoundary {
  name: string;
  startDate: string;
  endDate: string;
}

// ──── Report Detail (composite) ────
interface ReportDetail {
  id: string;
  title: string;
  description: string;
  owner: string;
  status: ItemStatus;
  priority: string;
  estimate: number;
  remainingWork: number;
  dependencies: { id: string; title: string; status: ItemStatus }[];
  recentActivity: ActivityEvent[];
}
```

### Entity Relationships

```
ProjectSummary (1) ←── aggregates ──→ ProjectItem (many)
ProjectItem (1) ←── parentId ──→ ProjectItem (many)     // hierarchy
ProjectItem (1) ←── dependencies ──→ ProjectItem (many)  // cross-links
Risk (1) ←── impactedItems ──→ ProjectItem (many)
ActivityEvent (1) ←── targetId ──→ ProjectItem (1)
ActivityEvent (1) ←── actor ──→ TeamMember (1)
Milestone (1) ←── relatedItems ──→ ProjectItem (many)
```

### Mock Data Volume

| Entity | Count | Source |
|--------|-------|--------|
| ProjectSummary | 1 | `mockData.projectSummary` |
| Epic | 4 | `mockData.projectItems` (type='epic') |
| Feature | 12 | `mockData.projectItems` (type='feature') |
| Story/Task/Bug | 40+ | `mockData.projectItems` (type='story'|'task'|'bug') |
| Risk | 8 | `mockData.risks` |
| TeamMember | 10 | `mockData.teamMembers` |
| ActivityEvent | 20 | `mockData.teamActivity` |
| Milestone | 6 | `mockData.roadmap.milestones` |
| SprintVelocity | 4 | `mockData.sprintMetrics.velocity` |

### Storage

**None.** All data lives in `server/data/mockData.ts` as exported TypeScript constants. No database, no file I/O, no persistence. Express routes import objects and return them directly (or filtered by ID for `/api/report/:id`).

---

## API Contracts

All endpoints return `Content-Type: application/json`. Base URL: `http://localhost:3001`.

### GET /api/project-summary

**Response 200:**
```json
{
  "id": "proj-001",
  "name": "Project Phoenix",
  "status": "on-track",
  "completionPercentage": 67,
  "deliveryConfidence": 82,
  "currentSprint": "Sprint 14",
  "daysRemaining": 23,
  "healthScore": 78,
  "healthThresholds": { "green": 75, "yellow": 50, "red": 0 }
}
```

### GET /api/project-items

**Response 200:**
```json
{
  "nodes": [
    {
      "id": "epic-001",
      "type": "epic",
      "title": "User Authentication Platform",
      "description": "...",
      "status": "in-progress",
      "priority": "critical",
      "owner": "Sarah Chen",
      "parentId": null,
      "estimate": 89,
      "remainingWork": 21,
      "dependencies": [],
      "recentActivity": []
    }
    // ... 55+ more items
  ],
  "links": [
    { "source": "epic-001", "target": "feat-001" },
    { "source": "feat-001", "target": "story-001" }
    // ... parent→child relationships
  ]
}
```

### GET /api/sprint-metrics

**Response 200:**
```json
{
  "currentSprint": "Sprint 14",
  "velocity": [
    { "sprint": "Sprint 11", "committed": 34, "completed": 29 },
    { "sprint": "Sprint 12", "committed": 38, "completed": 35 },
    { "sprint": "Sprint 13", "committed": 36, "completed": 33 },
    { "sprint": "Sprint 14", "committed": 40, "completed": 22 }
  ],
  "burndown": [
    { "day": 1, "ideal": 40, "actual": 40 },
    { "day": 2, "ideal": 36, "actual": 38 }
    // ... daily points
  ],
  "plannedVsCompleted": { "planned": 40, "completed": 22, "carryover": 4 },
  "openBugs": 7,
  "blockers": 3,
  "carryoverItems": 4
}
```

### GET /api/risks

**Response 200:**
```json
{
  "risks": [
    {
      "id": "risk-001",
      "title": "Third-party API rate limiting",
      "description": "Payment provider may throttle during peak load testing",
      "severity": "critical",
      "status": "open",
      "owner": "Marcus Johnson",
      "category": "Technical",
      "mitigation": "Implement circuit breaker pattern and request queuing",
      "impactedItems": ["feat-003", "story-012"]
    }
    // ... 7 more risks
  ]
}
```

### GET /api/team-activity

**Response 200:**
```json
{
  "events": [
    {
      "id": "evt-001",
      "type": "pr-completed",
      "actor": { "id": "tm-001", "name": "Sarah Chen", "avatar": "SC", "role": "Senior Engineer" },
      "action": "merged PR #142: Add OAuth2 refresh token flow",
      "target": "OAuth Integration",
      "targetId": "story-005",
      "timestamp": "2026-04-30T16:42:00Z"
    }
    // ... 19 more events
  ],
  "teamMembers": [
    { "id": "tm-001", "name": "Sarah Chen", "avatar": "SC", "role": "Senior Engineer" }
    // ... 9 more
  ]
}
```

### GET /api/roadmap

**Response 200:**
```json
{
  "milestones": [
    {
      "id": "ms-001",
      "name": "Alpha Release",
      "date": "2026-03-15",
      "type": "release",
      "status": "completed",
      "description": "Internal alpha with core authentication flow",
      "relatedItems": ["epic-001"]
    }
    // ... 5 more milestones
  ],
  "sprints": [
    { "name": "Sprint 12", "startDate": "2026-03-25", "endDate": "2026-04-07" }
    // ...
  ]
}
```

### GET /api/report/:id

**Request:** `:id` must match `/^[a-z]+-\d{3}$/` (e.g., `epic-001`, `story-042`, `risk-001`).

**Response 200:**
```json
{
  "id": "story-005",
  "title": "Implement OAuth2 refresh token flow",
  "description": "Handle token expiry and silent refresh without user interruption",
  "owner": "Sarah Chen",
  "status": "in-progress",
  "priority": "high",
  "estimate": 5,
  "remainingWork": 2,
  "dependencies": [
    { "id": "story-003", "title": "OAuth2 provider setup", "status": "done" }
  ],
  "recentActivity": [
    {
      "id": "evt-001",
      "type": "pr-completed",
      "actor": { "id": "tm-001", "name": "Sarah Chen", "avatar": "SC", "role": "Senior Engineer" },
      "action": "merged PR #142",
      "target": "OAuth Integration",
      "targetId": "story-005",
      "timestamp": "2026-04-30T16:42:00Z"
    }
  ]
}
```

**Response 400 (invalid ID format):**
```json
{ "error": "INVALID_ID", "message": "ID must match format: type-NNN" }
```

**Response 404 (not found):**
```json
{ "error": "NOT_FOUND", "message": "Item with id 'story-999' not found" }
```

### Error Handling Convention

All endpoints return errors as:
```json
{
  "error": "ERROR_CODE",
  "message": "Human-readable description"
}
```

HTTP status codes: `200` (success), `400` (bad request/validation), `404` (not found), `500` (unexpected server error caught by global handler).

---

## Infrastructure Requirements

### Local Development (Primary — Spec Default)

| Requirement | Detail |
|-------------|--------|
| **Node.js** | 22 LTS (required) |
| **npm** | 10.x (ships with Node 22) |
| **Browser** | Chrome/Edge/Firefox 120+ or Safari 17+ with WebGL 2.0 |
| **GPU** | Integrated graphics sufficient at `low` quality; discrete GPU recommended for `high` |
| **Ports** | 5173 (Vite), 3001 (Express) — both localhost only |
| **Network** | None required after `npm install` |
| **Disk** | ~300MB (`node_modules` with Three.js + R3F deps) |

### Startup Sequence

```bash
git clone <repo>
cd reporting-dashboard
npm install          # installs root + client + server workspaces
npm run dev          # concurrently: vite (5173) + nodemon express (3001)
# Open http://localhost:5173
```

### Optional: Docker

```dockerfile
FROM node:22-alpine AS build
WORKDIR /app
COPY . .
RUN npm ci --workspaces
RUN cd client && npm run build

FROM node:22-alpine
WORKDIR /app
COPY --from=build /app/server ./server
COPY --from=build /app/client/dist ./client/dist
COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/package.json .
ENV NODE_ENV=production
EXPOSE 3001
CMD ["node", "server/index.js"]
```

In production mode, Express serves the built Vite output from `client/dist/` as static files, eliminating the need for two processes.

### CI/CD

Out of scope per spec. If added later:
- **Lint:** `npm run lint` (ESLint flat config)
- **Type check:** `npm run typecheck` (tsc --noEmit for both client and server)
- **Test:** `npm run test` (Vitest)
- **Build:** `npm run build` (Vite production build)

---

## Technology Stack Decisions

| Category | Choice | Version | Justification |
|----------|--------|---------|---------------|
| **Runtime** | Node.js | 22 LTS | Spec-mandated |
| **HTTP Server** | Express | 4.21.x | Spec-mandated. Express 5 available but ecosystem middleware still catching up |
| **UI Framework** | React | 18.3.x | Component model essential for 7 dashboard sections + state management. Hooks-based architecture |
| **Language** | TypeScript | 5.5.x | Type safety across 10+ data interfaces, 3D prop types, API response shapes. Non-negotiable at this complexity |
| **Bundler** | Vite | 6.x | Spec-mandated. Sub-second HMR critical for visual iteration |
| **3D Engine** | Three.js | r170+ | Spec-mandated |
| **3D React Bridge** | @react-three/fiber | 9.x | Eliminates imperative Three.js boilerplate. Declarative scene graph matches React mental model. ~30KB bundle cost justified by 500+ lines saved |
| **3D Helpers** | @react-three/drei | 10.x | Pre-built OrbitControls, Float, Stars, Text, Preload. Avoids reimplementing common patterns |
| **Post-Processing** | @react-three/postprocessing | 3.x | Declarative Bloom, Vignette via pmndrs EffectComposer. Cleaner than raw Three.js EffectComposer |
| **Force Graph** | three-forcegraph | 1.77+ | Purpose-built 3D force-directed layout. Avoids building d3-force-3d integration from scratch (highest-risk feature) |
| **3D Labels** | three-spritetext | 1.9+ | Billboard text labels without geometry overhead. Essential for node labels in hierarchy graph |
| **Data Fetching** | SWR | 4.x | Caching, loading/error states, deduplication out of the box. Correct pattern for future real-API swap |
| **2D Charts** | Chart.js + react-chartjs-2 | 4.4 + 5.2 | Lightweight (60KB gzip), excellent animation support. Covers burndown + velocity + comparative charts |
| **Custom Viz** | D3.js | 7.9.x | Used only for Risk Radar orbit SVG. Not used for standard charts (Chart.js is simpler) |
| **DOM Animation** | GSAP | 3.12.x | Superior timeline control for choreographed sequences (fly-in → counters → rings). Free license sufficient for internal use |
| **3D Animation** | @react-spring/three | 9.7.x | Physics-based spring interpolation for camera transitions and node hover effects inside R3F |
| **Security** | helmet | 8.x | CSP headers with WebGL-compatible policy. Zero cost to add, establishes pattern for future deployment |
| **CORS** | cors | 2.8.x | Required for cross-origin requests between Vite (5173) and Express (3001) |
| **Dev Reload** | nodemon | 3.1.x | Auto-restarts Express on server file changes |
| **Concurrent Runner** | concurrently | 9.x | Single `npm run dev` starts both Vite + Express |
| **Debug GUI** | Leva | 0.10.x | Runtime sliders for bloom intensity, particle count, glow colors. Invaluable for visual tuning during development |

### Decisions NOT Made (Intentionally Excluded)

| Technology | Reason for exclusion |
|------------|---------------------|
| Redux / Zustand | Overkill. App is read-heavy with minimal state mutations. React Context + useReducer sufficient |
| React Router | No multi-page routing needed. Navigation is camera-based section transitions |
| Framer Motion | Overlaps with GSAP. GSAP chosen for superior timeline sequencing |
| Socket.io / WebSockets | No real-time data. All mock data is static |
| Database (any) | Spec explicitly prohibits persistence |
| Tailwind CSS | Glassmorphism requires custom CSS properties that Tailwind abstracts poorly. Raw CSS + theme tokens is cleaner |

---

## Security Considerations

### Authentication & Authorization

**None.** The spec explicitly prohibits authentication. No login, sessions, tokens, cookies, or user identity. The application is a local-only demo tool.

### Transport Security

- Local development runs over `http://localhost` — no TLS required.
- If deployed behind a reverse proxy in future, Express should be configured with `trust proxy` and the proxy should terminate TLS.

### Input Validation

- **`GET /api/report/:id`**: The `:id` parameter is validated against a regex pattern (`/^[a-z]+-\d{3}$/`) before lookup. This prevents:
  - Path traversal (e.g., `../../etc/passwd`)
  - NoSQL injection (if data source changes to MongoDB later)
  - Arbitrary string processing
- All other endpoints are parameterless GETs returning static data.

### HTTP Security Headers (helmet)

```typescript
app.use(helmet({
  contentSecurityPolicy: {
    directives: {
      defaultSrc: ["'self'"],
      scriptSrc: ["'self'", "'unsafe-eval'"],  // Three.js shader compilation requires eval
      styleSrc: ["'self'", "'unsafe-inline'"],  // Glassmorphism inline styles
      imgSrc: ["'self'", "data:", "blob:"],     // Three.js textures
      connectSrc: ["'self'", "http://localhost:3001"],
      workerSrc: ["'self'", "blob:"],           // Three.js web workers
    }
  },
  crossOriginEmbedderPolicy: false,  // Required for SharedArrayBuffer if used by Three.js
}));
```

### CORS Configuration

```typescript
app.use(cors({
  origin: process.env.CORS_ORIGIN || 'http://localhost:5173',
  methods: ['GET'],
  optionsSuccessStatus: 200,
}));
```

Configurable via `CORS_ORIGIN` environment variable for future deployment scenarios.

### Secrets Management

Zero secrets in the codebase. No API keys, tokens, connection strings, or credentials. The `.env` file (if present) contains only non-sensitive configuration like `PORT=3001` and `CORS_ORIGIN`.

---

## Scaling Strategy

### Current Scope: No Scaling Required

This is a single-user, local-only demo application. It serves static mock data from memory. There is nothing to scale.

### Future Scaling Path (If Deployed)

If the application is later deployed for remote access:

**Frontend:**
- Vite production build outputs static files → serve from any CDN (Azure Static Web Apps, Vercel, S3+CloudFront)
- Bundle splitting: Three.js + R3F are the heaviest chunks (~800KB gzipped). Vite's dynamic import splits them from the initial bundle
- The 3D scene is client-rendered; the server has zero rendering load

**Backend:**
- Express serves JSON from memory → sub-millisecond response times → a single Node.js process handles thousands of concurrent users
- If real API integration is added later, introduce caching (Redis or in-memory TTL cache) to avoid hammering upstream APIs
- Stateless design: no sessions, no server-side state → horizontal scaling via load balancer is trivial

**Performance Scaling (Client-Side):**

The adaptive quality system is the primary scaling mechanism:

```typescript
<PerformanceMonitor
  onDecline={() => dispatch({ type: 'SET_QUALITY', level: 'low' })}
  onIncline={() => dispatch({ type: 'SET_QUALITY', level: 'high' })}
>
  <ParticleField count={qualitySettings[qualityLevel].particles} />
  <PostProcessingStack enabled={qualitySettings[qualityLevel].bloom} />
</PerformanceMonitor>
```

| Setting | Particles | Bloom | Shadows | Antialias |
|---------|-----------|-------|---------|-----------|
| High | 1000 | Full (intensity 1.5) | Soft | MSAA 4x |
| Medium | 500 | Reduced (intensity 0.8) | Basic | MSAA 2x |
| Low | 200 | Disabled | Disabled | None |

---

## Risks & Mitigations

### Critical Risks

| # | Risk | Impact | Likelihood | Mitigation |
|---|------|--------|------------|------------|
| 1 | **3D Force Graph complexity** — `three-forcegraph` may not handle 56+ nodes with acceptable click interaction, label rendering, and camera animation | Blocks US-2 entirely; 60%+ of dev time consumed | High | **Prototype first.** Build isolated 5-node demo in Phase 1. If `three-forcegraph` fails: fall back to custom `d3-force-3d` layout with R3F `<InstancedMesh>` for nodes. Budget 1 day for fallback. |
| 2 | **WebGL performance on integrated GPUs** — Bloom + 1000 particles + 56 animated graph nodes + shadows = potential sub-30fps | Unusable on Surface Pro, MacBook Air, and similar devices | Medium | `<PerformanceMonitor>` auto-degrades quality. Cap particles at 200 on low. Disable bloom entirely below 24fps. Use `InstancedMesh` for graph nodes (1 draw call vs 56). |
| 3 | **Visual polish scope creep** — "Executive demo ready" is subjective; team could iterate on aesthetics indefinitely | Schedule overrun of 50%+ | High | Define 3 reference screenshots before coding (Dribbble "dark dashboard" as inspiration). Time-box Phase 5 to 3 days max. Stakeholder review at end of Phase 4 to agree on "done." |

### High Risks

| # | Risk | Impact | Likelihood | Mitigation |
|---|------|--------|------------|------------|
| 4 | **Bloom + glassmorphism compositing** — CSS `backdrop-filter` on elements over a bloomed WebGL canvas may render differently across browsers (blur applies to the composited canvas output, not the scene) | Visual artifacts on Safari/Firefox | Medium | Test cross-browser in Phase 1. If `backdrop-filter` fails over Canvas: use solid semi-transparent backgrounds without blur as fallback. Both look acceptable with the neon glow theme. |
| 5 | **Three.js shader compilation stall** — First render freezes for 1-3s while GPU compiles shaders | Bad first impression; appears broken | High | drei's `<Preload all />` pre-compiles shaders during loading screen. Show animated loading spinner at z-index 50 until `Canvas.onCreated` fires. |
| 6 | **GSAP + R3F animation conflict** — GSAP animating DOM elements while R3F animates 3D objects on the same frame can cause jank | Stuttering during camera fly-in + counter animation | Low | Keep GSAP on DOM (2D overlays) and `@react-spring/three` on 3D scene. Never mix. Sequence them: fly-in completes → then counters animate (GSAP timeline `delay`). |

### Medium Risks

| # | Risk | Mitigation |
|---|------|------------|
| 7 | **R3F/drei version pinning** — Breaking changes between minor versions | Pin exact versions in `package.json`. Use `npm ci` not `npm install` in Dockerfile. |
| 8 | **Bundle size exceeding 2MB gzipped** — Three.js + R3F + Chart.js + D3 + GSAP is heavy | Tree-shake D3 (import only `d3-selection`, `d3-scale`, `d3-shape`). Lazy-load Chart.js panels. Vite's rollup optimizer handles the rest. Monitor with `npx vite-bundle-visualizer`. |
| 9 | **SWR cache stale after mockData changes** — Developers edit mock data but see old values | Set `dedupingInterval: 0` in development. SWR fetches fresh on every mount during dev. |