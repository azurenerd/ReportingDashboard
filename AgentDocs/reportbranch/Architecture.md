# Architecture

## Overview & Goals

The ReportingDashboard is a full-stack 3D animated web application serving as a futuristic "project command center" for executive presentations. It visualizes project management data across seven interactive dashboard sections using WebGL-powered 3D scenes with glassmorphism styling, bloom effects, and cinematic animations.

**Architecture Style:** Client-server monolith with a stateless REST API backend and a rich WebGL frontend. The frontend employs a hybrid rendering model — pure Three.js geometry for 3D visualizations (hierarchy graph, risk radar, timeline) and HTML overlays for text-heavy panels (metrics, activity feed, detail panel) — composed within a single React Three Fiber canvas.

**Primary Goals:**
1. Deliver all 7 dashboard sections as independently composable R3F/React components fed by 7 REST endpoints
2. Maintain strict separation between 3D scene logic (`scene/`), HTML overlay components (`components/`), data fetching (`api.ts`), and shared state (`store.ts`)
3. Enable zero-config local execution via `npm install && npm run dev`
4. Sustain ≥60 FPS on dedicated GPU and ≥30 FPS on integrated GPU with bloom post-processing active
5. Provide a documented single-module migration path (`api.ts`) from mock data to live APIs

---

## System Components

### 1. Express API Server

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Serve 7 read-only REST endpoints returning mock JSON data |
| **Technology** | Node.js 22 LTS, Express 5.x |
| **Interface** | HTTP GET on `localhost:3001/api/*` |
| **Dependencies** | `cors` (cross-origin for Vite dev server), `nodemon` (dev reload) |
| **Data** | In-memory JavaScript objects exported from `server/data/mockData.js` |
| **Statelessness** | No sessions, no database, no writes. Every request returns deterministic JSON. |

**Internal Structure:**
```
server/
├── index.js                 # Express app bootstrap, CORS config, route mounting
├── routes/
│   └── project.js           # All 7 GET route handlers
└── data/
    └── mockData.js          # Centralized mock data objects (project summary,
                             #   items, sprint metrics, risks, team activity,
                             #   roadmap, item details)
```

### 2. React Application Shell

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Bootstrap React, mount R3F Canvas, manage global layout, orchestrate data loading |
| **Technology** | React 19.x, Vite 6.x, TypeScript 5.7+ (strict mode) |
| **Interface** | Browser DOM at `localhost:5173` |
| **Dependencies** | Zustand store, `api.ts` module |
| **Key Files** | `main.tsx` (entry), `App.tsx` (layout + Canvas), `store.ts`, `api.ts` |

**`App.tsx` Layout Responsibility:**
- Renders full-viewport `<Canvas>` from R3F with `dpr={[1, 2]}` and `gl={{ antialias: true, alpha: false }}`
- Positions a fixed HTML layer outside the Canvas for the Detail Panel (avoids z-fighting)
- Manages the loading → fly-in → interactive state machine

### 3. Zustand State Store (`store.ts`)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Single source of truth for all shared application state |
| **Technology** | Zustand 5.x |
| **Interface** | Hook-based selectors (`useDashboardStore(s => s.selectedNode)`) |

**Store Shape:**
```typescript
interface DashboardStore {
  // Data (populated on mount)
  projectSummary: ProjectSummary | null;
  projectItems: ProjectItem[];
  sprintMetrics: SprintMetrics | null;
  risks: Risk[];
  teamActivity: ActivityEvent[];
  roadmap: RoadmapMilestone[];
  
  // UI State
  isLoading: boolean;
  loadError: string | null;
  selectedNodeId: string | null;
  detailPanelData: ReportDetail | null;
  isDetailPanelOpen: boolean;
  isFlyInComplete: boolean;
  
  // Camera
  cameraTarget: { position: [number, number, number]; lookAt: [number, number, number] } | null;
  
  // Actions
  fetchAllData: () => Promise<void>;
  selectNode: (id: string) => Promise<void>;
  clearSelection: () => void;
  setFlyInComplete: () => void;
}
```

**Selector Pattern:** Components subscribe to the narrowest slice they need. Example: `HierarchyScene` subscribes to `projectItems` and `selectedNodeId` only. This prevents unnecessary re-renders — critical for 60 FPS.

### 4. API Service Module (`api.ts`)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Centralized HTTP client — the **single replacement point** for migrating to real APIs |
| **Technology** | Native `fetch` API |
| **Interface** | Exported async functions: `fetchProjectSummary()`, `fetchProjectItems()`, etc. |
| **Dependencies** | None (zero-dependency module) |

```typescript
const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:3001';

// Migration: change BASE_URL and add Authorization header here
async function apiFetch<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`);
  if (!res.ok) throw new Error(`API ${path}: ${res.status}`);
  return res.json();
}

export const fetchProjectSummary = () => apiFetch<ProjectSummary>('/api/project-summary');
export const fetchProjectItems = () => apiFetch<ProjectItem[]>('/api/project-items');
export const fetchSprintMetrics = () => apiFetch<SprintMetrics>('/api/sprint-metrics');
export const fetchRisks = () => apiFetch<Risk[]>('/api/risks');
export const fetchTeamActivity = () => apiFetch<ActivityEvent[]>('/api/team-activity');
export const fetchRoadmap = () => apiFetch<RoadmapMilestone[]>('/api/roadmap');
export const fetchReportDetail = (id: string) => apiFetch<ReportDetail>(`/api/report/${id}`);
```

### 5. R3F 3D Scene Components (`scene/`)

Each scene component is a self-contained R3F component rendered inside the `<Canvas>`. They read data from the Zustand store and render Three.js geometry.

#### 5a. `DashboardLayout.tsx`
- **Responsibility:** Positions all 3D sections in world space using `<group position={[x,y,z]}>` wrappers
- **Layout Strategy:** Sections are arranged on a virtual "floor plane" — Hierarchy center-left, Radar center-right, Timeline along the bottom. Positions are constants in a `LAYOUT_CONFIG` object for easy tuning.

#### 5b. `HierarchyScene.tsx` (US-2)
- **Responsibility:** Render the Epic → Feature → Story/Task node graph as 3D spheres with animated connection lines
- **Layout Algorithm:** Pre-computed tree layout (3 tiers radiating outward from center). Epics at Y=0, Features at Y=-2, Stories at Y=-4. Angular distribution within each tier.
- **Geometry:** `<Sphere>` (drei) with `<meshStandardMaterial emissive={statusColor} emissiveIntensity={hovered ? 1.5 : 0.3} />`. Size: Epic=0.8, Feature=0.5, Story=0.3.
- **Connections:** `<Line>` (drei) between parent-child nodes with animated dash offset via `useFrame`.
- **Interactions:** `onPointerOver` → set emissive intensity + scale tween (useFrame). `onClick` → dispatch `selectNode(id)` + set `cameraTarget` to node position.
- **Animation Boundary:** Hover glow and floating bob use `useFrame`. Click-to-focus camera tween uses GSAP (targets the R3F camera ref).

#### 5c. `RiskRadar.tsx` (US-4)
- **Responsibility:** Render risks as orbiting nodes grouped by severity on concentric rings
- **Layout:** 4 concentric ring radii (Critical=1, High=2, Medium=3, Low=4). Risk nodes orbit their ring with `useFrame` angle increment.
- **Visuals:** Critical/High nodes use red/orange emissive materials with higher bloom contribution. Ring paths rendered as `<Ring>` geometry with wireframe.
- **Interactions:** `onClick` dispatches `selectNode(risk.id)`.

#### 5d. `Timeline3D.tsx` (US-6)
- **Responsibility:** Render horizontal 3D timeline with milestone markers
- **Layout:** Linear X-axis path from `x=-8` to `x=8`. Milestones as vertical pillars or diamond shapes at computed X positions based on date.
- **Visuals:** Completed milestones are solid green, active pulse blue, upcoming dim gray. The timeline "rail" is a `<Tube>` geometry with emissive glow.
- **Interactions:** Hover shows milestone label (drei `<Html>` tooltip). Click opens detail panel.

#### 5e. `ParticleField.tsx` (US-8, US-9)
- **Responsibility:** Ambient particle background for visual depth
- **Implementation:** `<Points>` (drei) with 500-1000 particles in a bounding box. Slow drift animation via `useFrame` modifying `positions` buffer attribute.
- **Performance:** Use `<PointMaterial>` with `size={0.02}` and low opacity. Particle count is a tunable constant (`PARTICLE_COUNT`) for performance presets.

#### 5f. `PostEffects.tsx` (US-9)
- **Responsibility:** Post-processing pipeline for bloom and glow
- **Implementation:** `@react-three/postprocessing` `<EffectComposer>` with `<Bloom luminanceThreshold={0.4} intensity={1.2} radius={0.8} />` and `<Vignette eskil={false} offset={0.1} darkness={0.5} />`
- **Critical Constraint:** Bloom `luminanceThreshold` must be set high enough (≥0.4) so that glassmorphism HTML overlays don't bloom. Only emissive 3D materials (nodes, connections, timeline rail) should exceed the threshold.

#### 5g. `CameraController.tsx` (US-8, US-9)
- **Responsibility:** Manages cinematic fly-in on load + click-to-focus transitions + orbit controls
- **Animation Boundary:** This is the **only** component where GSAP directly controls R3F camera properties.
- **Fly-in Sequence:** GSAP timeline: `camera.position` from `[0, 20, 40]` → `[0, 5, 15]` over 3.5s with `power2.inOut` ease. On complete: enable OrbitControls, dispatch `setFlyInComplete()`.
- **Click-to-Focus:** When `cameraTarget` changes in store, GSAP tweens camera position to `target.position` over 1s. OrbitControls target updates simultaneously.
- **Orbit Controls:** drei `<OrbitControls>` with `enableDamping`, `dampingFactor={0.05}`, `minDistance={5}`, `maxDistance={30}`. Disabled during fly-in and focus transitions.

### 6. HTML Overlay Components (`components/`)

These are standard React components rendered **outside** the R3F Canvas as a fixed-position overlay layer. They use Tailwind CSS for glassmorphism styling.

#### 6a. `ProjectOverview.tsx` (US-1)
- **Responsibility:** Top-left overlay card showing project health KPIs
- **Data Source:** `store.projectSummary`
- **Animations:** GSAP-powered animated counters (completion %, health score, days remaining). SVG progress ring for health score with stroke-dasharray animation.
- **Styling:** Glassmorphism card: `backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl`

#### 6b. `SprintMetrics.tsx` (US-3)
- **Responsibility:** Bottom-left overlay with burndown and velocity charts
- **Data Source:** `store.sprintMetrics`
- **Charts:** Recharts `<AreaChart>` for burndown, `<BarChart>` for velocity. Dark theme with neon accent colors (`#00f0ff`, `#a855f7`).
- **Styling:** Same glassmorphism card pattern. Charts rendered with transparent background to blend with the dark theme.

#### 6c. `TeamActivity.tsx` (US-5)
- **Responsibility:** Right-side scrollable feed of recent activity events
- **Data Source:** `store.teamActivity`
- **Animations:** New entries slide in from right with CSS `@keyframes` or GSAP stagger. Pulse dot on each entry uses CSS animation.
- **Styling:** Glassmorphism card with `max-height` and `overflow-y: auto` with custom scrollbar styling.

#### 6d. `DetailPanel.tsx` (US-7)
- **Responsibility:** Slide-in panel showing full item details when a node/card is clicked
- **Data Source:** `store.detailPanelData` (fetched on demand from `/api/report/:id`)
- **Trigger:** Opens when `store.isDetailPanelOpen === true`
- **Animation:** GSAP slide from `translateX(100%)` to `translateX(0)` over 0.4s. Close reverses.
- **Styling:** Full-height right-side panel, glassmorphism, wider than other cards (~400px).

#### 6e. `LoadingState.tsx` (US-8)
- **Responsibility:** Full-screen loading overlay shown while data fetches
- **Visuals:** Centered pulsing logo/spinner with "Initializing Command Center..." text. Fades out when data is loaded and fly-in begins.

### 7. Shared Types (`types/`)

```
client/src/types/
├── project.ts        # ProjectSummary, ProjectItem, ItemStatus, ItemType
├── sprint.ts         # SprintMetrics, BurndownPoint, VelocitySprint
├── risk.ts           # Risk, RiskSeverity
├── activity.ts       # ActivityEvent, TeamMember, ActivityType
├── roadmap.ts        # RoadmapMilestone, MilestoneStatus
└── report.ts         # ReportDetail (union of all detail fields)
```

Types are client-only (TypeScript interfaces). The server uses plain JavaScript but its mock data conforms to these shapes by convention.

---

## Component Interactions

### Data Flow: Load Sequence

```
Browser Load
    │
    ├─► LoadingState renders (full-screen overlay)
    │
    ├─► store.fetchAllData() fires
    │       │
    │       ├─► Promise.all([
    │       │     fetchProjectSummary(),
    │       │     fetchProjectItems(),
    │       │     fetchSprintMetrics(),
    │       │     fetchRisks(),
    │       │     fetchTeamActivity(),
    │       │     fetchRoadmap()
    │       │   ])
    │       │       │
    │       │       ▼
    │       │   Express :3001 → 6 GET handlers → return mock JSON
    │       │
    │       ├─► Store updated: isLoading=false, all data populated
    │       │
    │       └─► On error: store.loadError set, LoadingState shows error message
    │
    ├─► LoadingState fades out
    │
    ├─► CameraController begins GSAP fly-in (3.5s)
    │       │
    │       ├─► ParticleField visible throughout
    │       ├─► Dashboard sections fade in as camera arrives
    │       └─► On complete: OrbitControls enabled, isFlyInComplete=true
    │
    └─► Interactive state: user can orbit, hover, click
```

### Data Flow: Node Selection

```
User clicks 3D node (HierarchyScene/RiskRadar/Timeline)
    │
    ├─► Component calls store.selectNode(id)
    │       │
    │       ├─► store.selectedNodeId = id
    │       ├─► store.isDetailPanelOpen = true
    │       ├─► store.cameraTarget = node's 3D position
    │       └─► await fetchReportDetail(id) → store.detailPanelData
    │
    ├─► CameraController detects cameraTarget change
    │       └─► GSAP tweens camera to target position (1s)
    │
    ├─► HierarchyScene detects selectedNodeId change
    │       └─► Selected node gets highlight ring / scale-up
    │
    └─► DetailPanel detects isDetailPanelOpen=true
            └─► GSAP slides panel in from right
```

### Animation System Boundaries

This is a **critical architectural boundary** to prevent GSAP and R3F `useFrame` from conflicting:

| Animation Type | System | Reason |
|---------------|--------|--------|
| Camera fly-in | **GSAP** | Timeline sequencing, easing, precise duration control |
| Camera click-to-focus | **GSAP** | Smooth position tween with callback on complete |
| Node hover glow | **useFrame** | Per-frame emissive intensity lerp, tightly coupled to R3F render loop |
| Node floating bob | **useFrame** | Continuous sinusoidal Y offset, needs frame-level precision |
| Particle drift | **useFrame** | Buffer geometry position updates every frame |
| Orbital risk rotation | **useFrame** | Continuous angle increment per frame |
| Timeline rail glow pulse | **useFrame** | Shader uniform update per frame |
| Animated counters | **GSAP** | DOM text content tween, runs outside Canvas |
| Progress ring | **GSAP** | SVG stroke-dashoffset tween, DOM element |
| Detail panel slide | **GSAP** | DOM translateX tween |
| Activity feed entries | **CSS** | `@keyframes` for pulse dot; GSAP stagger for entry appearance |
| Section fade-in on load | **GSAP** | Opacity tween on HTML overlay containers, sequenced with fly-in |

**Rule:** GSAP never touches Three.js object properties inside the R3F render loop except the camera (via ref). `useFrame` never touches DOM elements. This prevents frame-rate issues from competing animation systems.

### Communication Patterns

- **Store → Components:** Zustand selector hooks (reactive, minimal re-renders)
- **Components → Store:** Direct action dispatch (`store.selectNode(id)`)
- **Store → API:** `api.ts` async functions called from store actions
- **3D ↔ HTML:** No direct communication. Both read from the same Zustand store. The store is the mediator.
- **CameraController ↔ OrbitControls:** CameraController disables OrbitControls during GSAP tweens via a ref flag (`isAnimating`), re-enables on tween complete.

---

## Data Model

### Entities

#### ProjectSummary
```typescript
interface ProjectSummary {
  id: string;
  name: string;
  status: 'On Track' | 'At Risk' | 'Off Track';
  completionPercentage: number;        // 0-100
  deliveryConfidence: 'High' | 'Medium' | 'Low';
  currentSprint: string;               // e.g., "Sprint 14"
  sprintDaysRemaining: number;
  healthScore: number;                  // 0-100
  totalEpics: number;
  totalFeatures: number;
  totalStories: number;
  totalBugs: number;
}
```

#### ProjectItem
```typescript
type ItemStatus = 'Done' | 'InProgress' | 'Blocked' | 'NotStarted' | 'AtRisk';
type ItemType = 'Epic' | 'Feature' | 'Story' | 'Task' | 'Bug';

interface ProjectItem {
  id: string;
  title: string;
  type: ItemType;
  status: ItemStatus;
  parentId: string | null;             // null for Epics
  assignee: string;
  priority: 'Critical' | 'High' | 'Medium' | 'Low';
  storyPoints: number;
  remainingWork: number;               // hours
  tags: string[];
}
```

#### SprintMetrics
```typescript
interface BurndownPoint {
  day: number;                         // sprint day 1-10
  ideal: number;                       // ideal remaining points
  actual: number;                      // actual remaining points
}

interface VelocitySprint {
  sprint: string;                      // "Sprint 11", "Sprint 12", etc.
  planned: number;
  completed: number;
}

interface SprintMetrics {
  currentSprint: string;
  velocity: number;                    // average points per sprint
  plannedPoints: number;
  completedPoints: number;
  burndown: BurndownPoint[];
  velocityHistory: VelocitySprint[];   // last 5 sprints
  openBugs: number;
  blockers: number;
  carryoverItems: number;
}
```

#### Risk
```typescript
type RiskSeverity = 'Critical' | 'High' | 'Medium' | 'Low';

interface Risk {
  id: string;
  title: string;
  description: string;
  severity: RiskSeverity;
  owner: string;
  status: 'Open' | 'Mitigated' | 'Closed';
  impact: string;
  mitigation: string;
  dateIdentified: string;              // ISO 8601
}
```

#### ActivityEvent
```typescript
type ActivityType = 'PR_Merged' | 'PR_Opened' | 'Task_Completed' | 'Bug_Fixed'
                  | 'Comment' | 'Deploy' | 'Review' | 'Sprint_Update';

interface TeamMember {
  id: string;
  name: string;
  role: string;
  avatar: string;                      // initials or emoji
}

interface ActivityEvent {
  id: string;
  type: ActivityType;
  description: string;
  member: TeamMember;
  timestamp: string;                   // ISO 8601
  relatedItemId: string | null;
}
```

#### RoadmapMilestone
```typescript
type MilestoneStatus = 'Completed' | 'Active' | 'Upcoming';
type MilestoneType = 'Release' | 'Milestone' | 'SprintBoundary';

interface RoadmapMilestone {
  id: string;
  title: string;
  type: MilestoneType;
  status: MilestoneStatus;
  date: string;                        // ISO 8601
  description: string;
  deliverables: string[];
}
```

#### ReportDetail
```typescript
interface ReportDetail {
  id: string;
  title: string;
  type: string;                        // item type or "Risk"
  description: string;
  owner: string;
  status: string;
  priority: string;
  estimate: number | null;             // story points or null
  remainingWork: number | null;        // hours or null
  dependencies: string[];
  recentActivity: {
    date: string;
    action: string;
    actor: string;
  }[];
  tags: string[];
  createdDate: string;
  updatedDate: string;
}
```

### Relationships

```
ProjectSummary (1) ──── aggregates ────► ProjectItem (many)
ProjectItem (parent) ──── parentId ────► ProjectItem (children)
  Epic (0 parents) → Feature (parentId=Epic.id) → Story/Task/Bug (parentId=Feature.id)
ActivityEvent (many) ──── member ────► TeamMember (1)
ActivityEvent (many) ──── relatedItemId ────► ProjectItem (0..1)
ReportDetail ──── fetched by id ────► ProjectItem.id | Risk.id
```

### Storage

All data lives in `server/data/mockData.js` as exported JavaScript objects. No database. No file I/O at runtime. Data is loaded into memory when the Express server starts and served directly from module scope.

**Mock Data Volume Requirements:**
| Entity | Minimum Count |
|--------|--------------|
| ProjectSummary | 1 |
| Epics | 4 |
| Features | 12 |
| Stories/Tasks/Bugs | 40+ |
| Risks | 8 |
| Team Members | 10 |
| Activity Events | 20 |
| Roadmap Milestones | 6 |

---

## API Contracts

All endpoints return `application/json`. All are `GET` only. No request body, no query parameters (except `:id`).

### `GET /api/project-summary`

**Response 200:**
```json
{
  "id": "proj-001",
  "name": "Project Atlas",
  "status": "At Risk",
  "completionPercentage": 67,
  "deliveryConfidence": "Medium",
  "currentSprint": "Sprint 14",
  "sprintDaysRemaining": 4,
  "healthScore": 72,
  "totalEpics": 4,
  "totalFeatures": 12,
  "totalStories": 34,
  "totalBugs": 8
}
```

### `GET /api/project-items`

**Response 200:** Array of `ProjectItem`. Flat list; hierarchy is reconstructed client-side via `parentId`.
```json
[
  {
    "id": "epic-001",
    "title": "User Authentication Platform",
    "type": "Epic",
    "status": "InProgress",
    "parentId": null,
    "assignee": "Sarah Chen",
    "priority": "High",
    "storyPoints": 89,
    "remainingWork": 32,
    "tags": ["security", "core"]
  }
]
```

### `GET /api/sprint-metrics`

**Response 200:** Single `SprintMetrics` object with nested arrays for burndown and velocity history.

### `GET /api/risks`

**Response 200:** Array of `Risk` (minimum 8 items).

### `GET /api/team-activity`

**Response 200:** Array of `ActivityEvent` (minimum 20 items), sorted by timestamp descending.

### `GET /api/roadmap`

**Response 200:** Array of `RoadmapMilestone` (minimum 6 items), sorted by date ascending.

### `GET /api/report/:id`

**Response 200:** Single `ReportDetail` object for the given item or risk ID.

**Response 404:**
```json
{
  "error": "Not Found",
  "message": "No item found with id: <id>"
}
```

### Error Handling Pattern

All endpoints follow the same error structure:
```json
{
  "error": "<HTTP Status Text>",
  "message": "<Human-readable description>"
}
```

The Express server uses a global error handler middleware that catches thrown errors and returns consistent JSON. The server never returns HTML error pages.

**Client-side error handling:** The `apiFetch` wrapper in `api.ts` throws on non-2xx responses. The Zustand `fetchAllData` action catches errors and sets `store.loadError`. The `LoadingState` component renders the error message with a retry button.

---

## Infrastructure Requirements

### Local Development (Primary Target)

| Requirement | Detail |
|-------------|--------|
| **Runtime** | Node.js 22 LTS |
| **Browser** | Modern Chromium (Chrome/Edge) with WebGL 2.0 + hardware acceleration |
| **OS** | Windows, macOS, or Linux |
| **GPU** | Integrated GPU minimum (WebGL 2.0 at 1080p); dedicated GPU recommended |
| **Ports** | 5173 (Vite dev server), 3001 (Express API) |
| **Network** | Localhost only; no external network required |
| **Storage** | ~200MB for `node_modules`; no runtime storage |

### Zero-Config Setup

Root `package.json` scripts:
```json
{
  "scripts": {
    "dev": "concurrently \"npm run dev:server\" \"npm run dev:client\"",
    "dev:server": "cd server && nodemon index.js",
    "dev:client": "cd client && vite",
    "build": "cd client && vite build",
    "install:all": "npm install && cd server && npm install && cd ../client && npm install"
  }
}
```

`npm install` at root triggers `install:all` via a `postinstall` script, which installs dependencies in both `server/` and `client/` subdirectories.

### Optional Deployment (Not In Scope, Documented for Future)

| Tier | Frontend | Backend | Estimated Cost |
|------|----------|---------|---------------|
| **Azure Static Web Apps + App Service B1** | Static build deployed to SWA | Express on App Service | ~$15/month |
| **Azure Container Apps (single container)** | Vite build served by Express static middleware | Same container | ~$10/month |
| **Local only** | `vite dev` | `node server/index.js` | $0 |

### CI/CD

Out of scope per spec. No pipelines, no Dockerfiles. The README documents manual build steps:
```bash
cd client && npm run build    # Produces dist/
cd server && node index.js    # Serve API + optionally serve dist/ as static
```

---

## Technology Stack Decisions

### Frontend Stack

| Choice | Alternatives Considered | Justification |
|--------|------------------------|---------------|
| **React 19 + TypeScript** | Vanilla JS, Svelte | Spec mandates component-based architecture; React has the richest Three.js ecosystem (R3F, drei) |
| **React Three Fiber 9** | Vanilla Three.js | 7 interactive sections with shared state require declarative scene composition. R3F handles raycasting events, HTML↔WebGL bridging, and React state integration out of the box. Adds ~30KB gzipped. |
| **@react-three/drei 10** | Custom helpers | OrbitControls, Html overlays, Text, Line, Points — all needed. Writing these from scratch would add 2+ days. |
| **@react-three/postprocessing 3** | Manual EffectComposer | Declarative bloom/vignette pipeline that integrates with R3F's render loop without manual pass management. |
| **GSAP 3.12** | Framer Motion, anime.js | Only animation library that can tween Three.js camera properties via refs AND sequence DOM animations in a single timeline. Framer Motion cannot animate Three.js objects. GSAP's timeline API is essential for the cinematic fly-in sequence. |
| **Zustand 5** | Redux, Jotai, Context API | Minimal boilerplate, no providers needed inside R3F Canvas (Context doesn't bridge into R3F's reconciler). Zustand works across both React DOM and R3F contexts. |
| **Recharts 2.15** | Chart.js, D3, Nivo | React-native API, renders inside HTML overlays naturally, supports dark theme with minimal config. D3 requires imperative DOM manipulation; Chart.js needs a wrapper. |
| **Tailwind CSS 4** | styled-components, CSS modules | Utility-first approach is fastest for glassmorphism patterns (`backdrop-blur-xl`, `bg-white/5`). No runtime CSS-in-JS overhead — important for 60 FPS. |
| **Vite 6** | Webpack, Parcel | Spec-mandated. Fastest HMR, native TypeScript support, Vitest integration. |

### Backend Stack

| Choice | Alternatives Considered | Justification |
|--------|------------------------|---------------|
| **Express 5** | Fastify, Koa, Hono | Spec-mandated. For 7 static GET endpoints, any framework works. Express has the largest ecosystem if future middleware is needed. |
| **In-memory mock data** | SQLite, JSON files on disk | Simplest possible data layer. No file I/O, no query language, no ORM. Data is a JavaScript module imported at startup. |
| **cors middleware** | Manual headers | Standard solution for Vite dev server (port 5173) → Express (port 3001) cross-origin requests. |
| **nodemon** | ts-node-dev, tsx | Server is plain JavaScript (no TypeScript compilation needed). Nodemon watches for file changes and restarts. |

### Tooling Decisions

| Choice | Justification |
|--------|---------------|
| **concurrently** | Single `npm run dev` starts both servers. Simpler than Docker Compose for local dev. |
| **TypeScript strict mode** | Prevents `any` leaks in a complex 3D codebase with Vector3 tuples, nested data models, and event handlers. |
| **Vitest** (if tests added) | Vite-native test runner; shares the same config and transform pipeline. |

---

## Security Considerations

### Current Scope (Mock Data, Local Only)

| Area | Status | Detail |
|------|--------|--------|
| **Authentication** | Not required | No login, no tokens, no sessions. All endpoints are public. |
| **Authorization** | Not required | Single role: anonymous viewer. |
| **Data protection** | Not required | All data is mock. No PII, no secrets. |
| **HTTPS** | Not required | Localhost HTTP is acceptable for development/demo. |
| **Input validation** | Minimal | Only `/api/report/:id` accepts user input (the `id` parameter). Validate that `id` is alphanumeric. |
| **CORS** | Permissive | `cors({ origin: 'http://localhost:5173' })` — restrict to Vite dev server origin only. |
| **Dependencies** | Standard | Run `npm audit` before demo. No known vulnerable packages in the recommended stack. |

### XSS Prevention

HTML overlay components render data from the API. Even with mock data, establish the pattern:
- React's JSX auto-escapes string interpolation (safe by default)
- Never use `dangerouslySetInnerHTML` with API data
- drei's `<Html>` component renders into a React portal — same JSX escaping applies

### Future Migration Security Checklist

When replacing mock data with real APIs:
1. Add `helmet` middleware to Express for security headers (CSP, X-Frame-Options, etc.)
2. Add authentication middleware (e.g., `passport-azure-ad` for Entra ID bearer tokens)
3. Add `Authorization: Bearer <token>` header in `api.ts` `apiFetch` function
4. Enforce HTTPS in production
5. Validate and sanitize all API response data rendered in HTML overlays
6. Add rate limiting middleware to Express

---

## Scaling Strategy

### Current Scope: No Scaling Required

This is a single-user local demo application. There is no multi-user access, no database, no deployment infrastructure.

### Performance Scaling (Rendering Budget)

The primary "scaling" concern is maintaining frame rate as scene complexity increases:

| Lever | Low Preset | High Preset |
|-------|-----------|-------------|
| Particle count | 200 | 1000 |
| Bloom enabled | No | Yes |
| Bloom intensity | — | 1.2 |
| Node geometry detail (segments) | 8 | 32 |
| Anti-aliasing | Off | MSAA 4x |
| Shadow maps | Off | Off (not needed for neon aesthetic) |
| Max simultaneous animations | 3 | Unlimited |

**Implementation:** A `QUALITY_PRESET` constant in a config file. Components read this value and adjust geometry detail, particle count, and post-processing accordingly. Default: `'high'`. Toggle to `'low'` for integrated GPU presentations.

**Geometry Budget:**
- 60 hierarchy nodes × 32-segment spheres = ~60K triangles
- 8 risk nodes + 4 orbital rings = ~5K triangles
- 6 timeline markers + rail tube = ~3K triangles
- 1000 particles (point sprites) = 1K draw calls (instanced)
- **Total: ~70K triangles** — well within budget for any GPU supporting WebGL 2.0

### Data Scaling (Future)

If migrating to real APIs with larger datasets:
- Implement pagination on `/api/project-items` (currently returns flat list)
- Add client-side virtualization for the activity feed (only render visible entries)
- Consider Level-of-Detail (LOD) for hierarchy: collapse deep sub-trees into aggregate nodes when zoomed out
- Add `loading` states per section rather than a single global fetch

### Deployment Scaling (Future)

If deployed beyond local:
- Frontend: Static files on CDN (Azure Static Web Apps) — scales infinitely for read-only content
- Backend: Single App Service instance is sufficient for mock data. For real APIs with external calls, add caching (Redis) and horizontal scaling behind Azure Front Door.

---

## Risks & Mitigations

### High Risk

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|------------|
| 1 | **WebGL performance on integrated GPU** — Bloom post-processing + 60 nodes + 1000 particles drops below 30 FPS on Intel UHD | High | Demo looks janky on exec's laptop | Implement `QUALITY_PRESET` config (high/low). Low preset disables bloom, reduces particles to 200, lowers geometry detail. **Test on target hardware in Phase 1.** |
| 2 | **Scope creep from visual polish** — 9 mandatory animation types × 7 sections = 63 animation touchpoints. Easy to spend days on micro-adjustments. | High | Timeline overrun | Phase delivery strictly: Phase 2 = functional with placeholder visuals. Phase 3 = polish. Define "good enough" for each animation before starting Phase 3. |
| 3 | **GSAP + R3F animation conflicts** — Two animation systems fighting over the same Three.js objects cause jitter or frozen frames | Medium | Visual glitches during camera transitions | Enforce strict animation boundary: GSAP owns camera + DOM only. `useFrame` owns all 3D object animations. Document this boundary in code comments and this architecture doc. CameraController disables OrbitControls during GSAP tweens. |

### Medium Risk

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|------------|
| 4 | **R3F `<Html>` overlay z-fighting** — Glassmorphism panels clip behind 3D objects or receive bloom incorrectly | Medium | Visual artifacts, unreadable text | Use drei `<Html>` with `zIndexRange={[0, 0]}` and `style={{ pointerEvents: 'auto' }}`. Alternatively, render HTML panels as a fixed DOM overlay **outside** the Canvas entirely (recommended for DetailPanel and SprintMetrics). Prototype in Phase 1. |
| 5 | **Bloom bleeds into HTML overlays** — UnrealBloomPass affects the entire framebuffer, making glassmorphism panels glow | Medium | Text becomes unreadable | Set `luminanceThreshold` ≥ 0.4 so only emissive 3D materials bloom. Use selective bloom via `@react-three/postprocessing` `Selection` API if global threshold is insufficient. Test in Phase 1 prototype. |
| 6 | **3D hierarchy layout overlaps** — 56+ nodes with parent-child connections become a visual tangle | Medium | Hierarchy section is unreadable | Use pre-computed tier layout (not force-directed) with fixed radial positions per tier. Limit visible labels to hovered/selected nodes. Provide zoom-to-tier interaction. |

### Low Risk

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|------------|
| 7 | **Existing .NET project conflict** — `src/ReportingDashboard/` confuses developers about which is the canonical implementation | Low | Developer confusion, wrong project modified | Place the Node.js dashboard in `src/reporting-dashboard-web/` (distinct directory). Add a note in the root README. Defer .NET disposition to stakeholder decision. |
| 8 | **Mock data doesn't resemble real API shapes** — Future migration requires significant refactoring | Low | Migration cost increases | Design mock data types to mirror Azure DevOps work item fields (id, title, state, assignedTo, effort, etc.). Document field mappings in README. |
| 9 | **WebGL context loss during 30-minute demo** — Browser reclaims GPU resources | Low | Dashboard goes blank | R3F handles context restoration automatically. Add an `onContextLost` handler that shows a "Reconnecting..." overlay and calls `renderer.forceContextRestore()`. |

### Architectural Decision Records (ADRs)

| Decision | Status | Rationale |
|----------|--------|-----------|
| Render DetailPanel and SprintMetrics as fixed DOM overlays outside Canvas, not inside `<Html>` | **Recommended** | Avoids z-fighting and bloom interference. These panels are text-heavy and benefit from standard CSS rendering. Only use `<Html>` for small tooltips positioned near 3D objects. |
| Pre-computed tree layout for hierarchy, not force-directed simulation | **Recommended** | Force-directed (d3-force-3d) produces unpredictable layouts and requires simulation settling time. Pre-computed tiers are deterministic, instant, and easier to position the camera for. |
| Server uses plain JavaScript, not TypeScript | **Recommended** | The server is 3 files totaling ~200 lines. TypeScript compilation adds complexity (ts-node or build step) with negligible benefit. Type safety is enforced by convention — mock data must match client TypeScript interfaces. |
| Use `import.meta.env.VITE_API_BASE_URL` for API base URL | **Decided** | Enables pointing the client at a different backend (e.g., real API server) without code changes. Default fallback to `http://localhost:3001`. |