# Research

_No research has been documented yet._

## Research technology stack for ReportingDashboard

_Researched on 2026-05-01 11:45 UTC_

### Summary

The ReportingDashboard is a **3D animated project management visualization** built as a full-stack web application targeting executive demos and engineering managers. The spec mandates **Node.js/Express** backend serving mock JSON data, and a **Three.js + WebGL** frontend with a dark futuristic aesthetic. No database, no authentication, no external services—purely a self-contained demo application runnable via `npm install && npm run dev`. **Primary recommendation:** Use **TypeScript + React + Vite** for the frontend (spec explicitly permits this), **Three.js via React Three Fiber** for 3D rendering, **Chart.js** for 2D charts, and **Express** for the backend API. This combination maximizes developer productivity, type safety, and component reuse while meeting every spec requirement. ---

### Key Findings

- The spec is highly prescriptive: Node.js/Express backend, Three.js frontend, mock data only, no database, no auth—the stack is essentially locked in.
- **React Three Fiber (R3F)** is the most productive way to build complex Three.js scenes with React—it wraps Three.js declaratively and has a mature ecosystem (drei, postprocessing).
- The biggest technical risk is **WebGL performance** with 60+ interactive 3D nodes, particle systems, bloom/glow effects, and smooth camera animations running simultaneously.
- **Glassmorphism + bloom + particles** together are GPU-intensive; careful render pipeline management (selective bloom, instanced meshes, LOD) is critical.
- The mock data volume is modest (< 100 entities)—in-memory JSON is perfectly adequate and simplifies the architecture.
- A **monorepo with Vite** for the frontend and a separate Express server is the cleanest structure, using `concurrently` to launch both via a single `npm run dev`.
- **No competitors exist** in the exact niche (3D animated project dashboards)—closest comparisons are Jira dashboards, Azure DevOps dashboards, and Linear's UI, but none use WebGL 3D visualization. --- **Goal**: Running app with all 7 API endpoints, basic 3D scene, and 2-3 dashboard sections.
- **Scaffold project**: Vite + React + TypeScript frontend, Express + TypeScript backend, concurrently for dev script.
- **Build mock data factory**: Typed interfaces for all entities (Project, Epic, Feature, Story, Risk, etc.). Generate minimum required data volumes.
- **Implement all 7 API endpoints**: Simple route handlers returning mock data. This unblocks all frontend work.
- **Basic Three.js scene**: Canvas, lights, camera, orbit controls, particle background. Confirm 60fps baseline.
- **Project Overview section**: Glassmorphism cards with health score, completion %, sprint info. Animated counters.
- **Sprint Metrics section**: Chart.js burndown and velocity charts in HTML overlay.
- Particle background + bloom gives instant "wow factor" with ~50 lines of R3F code
- Animated counters (0 → 87%) using gsap or framer-motion are visually impressive and trivial to implement
- Glassmorphism cards via Tailwind (`backdrop-blur-xl bg-white/10 border border-white/20`) take minutes
- **3D Project Hierarchy**: Force-directed layout with d3-force-3d, render as instanced spheres with connecting lines. Color-code by status. Click-to-focus camera movement.
- **Health Rings**: Animated torus geometries with shader-based progress fill.
- **Bar Towers**: Instanced box geometries for progress visualization.
- **Risk Radar**: Orbital animated nodes grouped by severity.
- **Timeline/Roadmap**: TubeGeometry path with milestone markers.
- **Team Activity Feed**: Scrollable HTML panel with animated entry transitions.
- **Detail Panel**: Slide-in panel on click, populated from `/api/report/:id`.
- **Camera fly-in**: GSAP timeline for initial load animation.
- **Cross-browser testing**: Chrome, Edge, Firefox, Safari.
- **Performance optimization**: Profile, optimize draw calls, add quality auto-degradation.
- **README & documentation**: Setup instructions, architecture notes, mock data replacement guide. Before committing to full implementation, prototype these in isolation:
- **3D hierarchy graph**: This is the most complex visual element. Build a standalone R3F scene with 50 nodes, force layout, click interaction, and camera animation. Validate performance and usability.
- **Selective bloom**: Test bloom + glassmorphism HTML overlay coexistence. Bloom can interfere with HTML overlays if not configured correctly (render order, layer masking).
- **GSAP + R3F integration**: Confirm GSAP can smoothly animate R3F camera refs without fighting React's render cycle.

### Recommended Tools & Technologies

- | Library | Version | Purpose | License | |---------|---------|---------|---------| | **React** | 19.1.x | UI framework | MIT | | **TypeScript** | 5.8.x | Type safety | Apache-2.0 | | **Vite** | 6.3.x | Build tool, dev server, HMR | MIT | | Library | Version | Purpose | Notes | |---------|---------|---------|-------| | **Three.js** | 0.175.x | WebGL rendering engine | **Required by spec** | | **@react-three/fiber** | 9.1.x | React renderer for Three.js | Declarative scene graph | | **@react-three/drei** | 10.x | Helper components (Text, Float, OrbitControls, Html) | 200+ utilities | | **@react-three/postprocessing** | 3.x | Bloom, glow, depth-of-field | Uses pmndrs/postprocessing under the hood | | **Chart.js** | 4.5.x | 2D charts (burndown, velocity) | MIT; use via `react-chartjs-2` v5 | | **react-chartjs-2** | 5.3.x | React wrapper for Chart.js | MIT | | **gsap** | 3.12.x | Camera fly-in, counter animations, timeline transitions | Free for non-commercial; Business license if monetized | | **leva** | 0.10.x | Dev-mode GUI for tuning 3D params | MIT; strip from prod | | Library | Version | Purpose | |---------|---------|---------| | **Tailwind CSS** | 4.1.x | Utility-first CSS, dark mode, glassmorphism | | **Framer Motion** | 12.x | Panel open/close, hover glow, 2D transitions | | **@fontsource/inter** | 5.x | Clean professional typography | | Library | Version | Purpose | |---------|---------|---------| | **Node.js** | 22 LTS | Runtime (**required by spec**) | | **Express** | 5.1.x | HTTP server (**required by spec**) | | **cors** | 2.8.x | Cross-origin for dev (Vite proxy alternative) | | **tsx** | 4.19.x | TypeScript execution for Node without build step | | Tool | Version | Purpose | |------|---------|---------| | **ESLint** | 9.x | Linting (flat config) | | **Prettier** | 3.5.x | Formatting | | **concurrently** | 9.x | Run frontend + backend with one command | | **Vitest** | 3.1.x | Unit tests (Vite-native) | | **Playwright** | 1.52.x | Visual regression / E2E if needed | | Tool | Purpose | |------|---------| | **GitHub Actions** | CI pipeline | | **Docker** | Optional containerized deployment | | **Azure Static Web Apps** or **Vercel** | Frontend hosting (if deployed beyond local) | | **Azure App Service** or **Render** | Backend hosting (if deployed) | ---
```
┌─────────────────────────────────────┐
│          Vite Dev Server            │
│  React + R3F + Chart.js Frontend    │
│  (port 5173)                        │
│         │                           │
│    fetch(/api/*)                    │
│         │  (Vite proxy in dev)      │
└─────────┼───────────────────────────┘
          ▼
┌─────────────────────────────────────┐
│       Express Backend (port 3001)   │
│                                     │
│  GET /api/project-summary           │
│  GET /api/project-items             │
│  GET /api/sprint-metrics            │
│  GET /api/risks                     │
│  GET /api/team-activity             │
│  GET /api/roadmap                   │
│  GET /api/report/:id                │
│                                     │
│  ┌─────────────┐                    │
│  │ mockData.ts │ ← static JSON     │
│  └─────────────┘                    │
└─────────────────────────────────────┘
```
- App loads → Three.js scene initializes → camera fly-in animation plays
- React components mount → `useEffect` hooks fetch from `/api/*` endpoints via a shared `api.ts` client
- Data populates React state → R3F components render 3D scene graph reactively
- User interactions (hover, click) update React state → camera animates to focus → detail panel slides in
```
<Canvas>
  <SceneSetup />          ← lights, fog, environment
  <CameraController />    ← fly-in, click-to-focus via gsap
  <ParticleBackground />  ← instanced mesh, 2000 particles
  <ProjectHierarchy />    ← force-directed 3D node graph
  <HealthRings />         ← animated torus geometries
  <BarTowers />           ← instanced box geometries
  <TimelinePath />        ← TubeGeometry + milestones
  <RiskRadar />           ← orbital animated nodes
  <EffectComposer>
    <Bloom />             ← selective bloom via layers
  </EffectComposer>
</Canvas>
<HtmlOverlay>
  <DashboardCards />      ← glassmorphism 2D panels
  <DetailPanel />         ← slide-in on click
  <SprintCharts />        ← Chart.js burndown/velocity
  <ActivityFeed />        ← scrollable feed
</HtmlOverlay>
```
- **React Three Fiber over raw Three.js**: The spec has 7+ dashboard sections each with complex 3D elements. R3F's declarative model and React lifecycle integration prevents the "imperative spaghetti" that raw Three.js apps become at this scale. The underlying Three.js is still directly accessible when needed.
- **Hybrid 2D+3D rendering**: Use Three.js for the immersive 3D elements (hierarchy graph, health rings, bar towers, timeline, radar). Use HTML overlays + Chart.js for data-dense 2D elements (burndown charts, activity feed, detail panels). This avoids rendering text in WebGL (poor quality) and leverages CSS for glassmorphism.
- **Selective bloom via Three.js layers**: Assign glowing objects to layer 1, render bloom pass only on that layer. This avoids blooming the entire scene (which washes out text and UI).
- **Instanced meshes for particles and nodes**: With 40+ story/task nodes and 2000+ particles, instancing is mandatory for 60fps performance.
- **GSAP for camera animation**: Three.js camera transitions need smooth easing and sequencing. GSAP's timeline API is purpose-built for this and integrates cleanly with R3F via refs. **No database required** (per spec). Structure:
```
/server/data/
  projects.json
  epics.json
  features.json
  stories.json
  risks.json
  team.json
  activity.json
  roadmap.json
``` Each file is loaded once at server start into memory. The `/api/report/:id` endpoint does a simple lookup across all entity arrays by ID.
```
/
├── server/
│   ├── index.ts
│   ├── routes/
│   │   ├── projectRoutes.ts
│   │   ├── sprintRoutes.ts
│   │   ├── riskRoutes.ts
│   │   ├── teamRoutes.ts
│   │   └── reportRoutes.ts
│   └── data/
│       └── mockData.ts          ← typed mock data factory
├── client/
│   ├── index.html
│   ├── vite.config.ts
│   ├── src/
│   │   ├── main.tsx
│   │   ├── App.tsx
│   │   ├── api/
│   │   │   └── client.ts        ← fetch wrapper with error handling
│   │   ├── hooks/
│   │   │   └── useProjectData.ts
│   │   ├── scene/
│   │   │   ├── SceneSetup.tsx
│   │   │   ├── CameraController.tsx
│   │   │   ├── ParticleBackground.tsx
│   │   │   ├── ProjectHierarchy.tsx
│   │   │   ├── HealthRings.tsx
│   │   │   ├── BarTowers.tsx
│   │   │   ├── TimelinePath.tsx
│   │   │   └── RiskRadar.tsx
│   │   ├── components/
│   │   │   ├── DashboardCards.tsx
│   │   │   ├── DetailPanel.tsx
│   │   │   ├── SprintCharts.tsx
│   │   │   └── ActivityFeed.tsx
│   │   ├── styles/
│   │   │   └── globals.css
│   │   └── types/
│   │       └── index.ts
├── package.json                  ← root workspace scripts
├── README.md
└── tsconfig.json
``` ---

### Considerations & Risks

- **None required.** The spec explicitly states no authentication and no external services. The app serves mock data only.
- No real user data is involved—mock data only.
- If this is ever extended to real data sources, add: API key auth, HTTPS enforcement, CORS lockdown, and input validation on `:id` params. | Tier | Frontend | Backend | Est. Monthly Cost | |------|----------|---------|-------------------| | **Free/Dev** | Vercel free tier or GitHub Pages (static build) | Render free tier or Azure App Service F1 | $0 | | **Small** (< 100 users) | Azure Static Web Apps Standard | Azure App Service B1 | ~$15/mo | | **Medium** (demo events) | Azure Static Web Apps + CDN | Azure App Service S1 | ~$70/mo |
- **Local-first** (primary use case): `npm run dev` starts both Vite and Express via `concurrently`.
- **Static export option**: `vite build` produces static assets. Express serves them in production mode. Single-process deployment.
- **Containerized**: Single Dockerfile—Express serves built Vite assets and API endpoints. Deploy anywhere (Azure Container Apps, Render, Fly.io).
- **WebGL compatibility**: Test on Chrome, Edge, Firefox. Safari WebGL2 support is adequate but may have shader quirks. Include a fallback message for browsers without WebGL2.
- **GPU requirements**: Bloom + particles + 60fps needs a discrete GPU or recent integrated GPU. Document minimum specs in README. --- | Risk | Impact | Mitigation | |------|--------|------------| | **WebGL performance on low-end hardware** | Dashboard stutters or crashes during exec demos | Profile early with Chrome DevTools Performance tab. Use instanced meshes, LOD, and `<PerformanceMonitor>` from drei to auto-degrade quality. Cap particles at 1000 on low-end. | | **3D node graph layout complexity** | The hierarchy view (4 epics → 12 features → 40+ items) needs a force-directed or tree layout in 3D space | Use `d3-force-3d` for layout computation, feed positions into R3F. Prototype this section first. | | **Scope creep in visual polish** | "Premium demo quality" is subjective—easy to spend unlimited time on effects | Define a visual baseline in Phase 1, polish in Phase 2. Use reference screenshots as acceptance criteria. | | Risk | Impact | Mitigation | |------|--------|------------| | **GSAP licensing** | Free for open-source, but "Business" license needed for commercial products. If this dashboard is shown to customers, clarify license status. | Evaluate if `@react-spring/three` (MIT) can replace GSAP for camera work. GSAP is better but Spring is license-safe. | | **Three.js breaking changes** | Three.js has frequent releases with occasional breaking changes to materials/shaders | Pin to exact version (e.g., `0.175.0`). Update deliberately. | | **Browser-specific WebGL bugs** | Safari and Firefox handle GLSL shaders differently than Chrome | Test cross-browser in CI with Playwright. Use standardized shader code. |
- **React Three Fiber vs. raw Three.js**: R3F adds ~30KB bundle overhead and a learning curve for devs unfamiliar with it. Trade-off is worth it for the declarative model at this scene complexity.
- **Tailwind CSS vs. CSS Modules**: Tailwind is faster for prototyping glassmorphism utilities but adds a build dependency. Acceptable for this project scope.
- **Mock data in JSON files vs. in-memory factory**: JSON files are easier to edit; factory functions generate more realistic randomized data. **Recommendation**: Use a typed factory function (`generateMockData()`) that produces consistent data on each server start (seeded random), with an option to export to JSON. ---
- **Target browsers & hardware**: Is this strictly Chrome on modern laptops? Or must it run on conference room displays, iPads, or older hardware? This directly impacts visual effects budget.
- **"Replace mock data with real APIs" scope**: The spec mentions documenting how to swap mock data for real APIs. Should the API client layer be designed with adapter patterns for Azure DevOps / Jira integration, or is a simple "change the URL" note sufficient?
- **Deployment target**: Is this local-only, or will it be hosted for ongoing access? This affects whether to invest in Docker, CI/CD, and hosting infrastructure.
- **Interaction depth**: The spec mentions click-to-focus and detail panels. Should there be drill-down navigation (e.g., click Epic → zoom into its Features → click Feature → see Stories)? Or is one level of detail sufficient?
- **Accessibility requirements**: The dark futuristic aesthetic with neon colors may have contrast ratio issues. Are WCAG compliance targets required, or is this purely a visual demo?
- **Performance budget**: What is the minimum acceptable frame rate? 60fps on all machines, or 30fps acceptable on integrated GPUs?
- **Team familiarity with Three.js/R3F**: If the team has no Three.js experience, the 3D hierarchy view and custom shaders will require significant ramp-up time (2-3 weeks). ---

### Detailed Analysis

# Research: Technology Stack for ReportingDashboard

## Executive Summary

The ReportingDashboard is a **3D animated project management visualization** built as a full-stack web application targeting executive demos and engineering managers. The spec mandates **Node.js/Express** backend serving mock JSON data, and a **Three.js + WebGL** frontend with a dark futuristic aesthetic. No database, no authentication, no external services—purely a self-contained demo application runnable via `npm install && npm run dev`.

**Primary recommendation:** Use **TypeScript + React + Vite** for the frontend (spec explicitly permits this), **Three.js via React Three Fiber** for 3D rendering, **Chart.js** for 2D charts, and **Express** for the backend API. This combination maximizes developer productivity, type safety, and component reuse while meeting every spec requirement.

---

## Key Findings

- The spec is highly prescriptive: Node.js/Express backend, Three.js frontend, mock data only, no database, no auth—the stack is essentially locked in.
- **React Three Fiber (R3F)** is the most productive way to build complex Three.js scenes with React—it wraps Three.js declaratively and has a mature ecosystem (drei, postprocessing).
- The biggest technical risk is **WebGL performance** with 60+ interactive 3D nodes, particle systems, bloom/glow effects, and smooth camera animations running simultaneously.
- **Glassmorphism + bloom + particles** together are GPU-intensive; careful render pipeline management (selective bloom, instanced meshes, LOD) is critical.
- The mock data volume is modest (< 100 entities)—in-memory JSON is perfectly adequate and simplifies the architecture.
- A **monorepo with Vite** for the frontend and a separate Express server is the cleanest structure, using `concurrently` to launch both via a single `npm run dev`.
- **No competitors exist** in the exact niche (3D animated project dashboards)—closest comparisons are Jira dashboards, Azure DevOps dashboards, and Linear's UI, but none use WebGL 3D visualization.

---

## Recommended Technology Stack

### Frontend – Core

| Library | Version | Purpose | License |
|---------|---------|---------|---------|
| **React** | 19.1.x | UI framework | MIT |
| **TypeScript** | 5.8.x | Type safety | Apache-2.0 |
| **Vite** | 6.3.x | Build tool, dev server, HMR | MIT |

### Frontend – 3D / Visualization

| Library | Version | Purpose | Notes |
|---------|---------|---------|-------|
| **Three.js** | 0.175.x | WebGL rendering engine | **Required by spec** |
| **@react-three/fiber** | 9.1.x | React renderer for Three.js | Declarative scene graph |
| **@react-three/drei** | 10.x | Helper components (Text, Float, OrbitControls, Html) | 200+ utilities |
| **@react-three/postprocessing** | 3.x | Bloom, glow, depth-of-field | Uses pmndrs/postprocessing under the hood |
| **Chart.js** | 4.5.x | 2D charts (burndown, velocity) | MIT; use via `react-chartjs-2` v5 |
| **react-chartjs-2** | 5.3.x | React wrapper for Chart.js | MIT |
| **gsap** | 3.12.x | Camera fly-in, counter animations, timeline transitions | Free for non-commercial; Business license if monetized |
| **leva** | 0.10.x | Dev-mode GUI for tuning 3D params | MIT; strip from prod |

### Frontend – UI / Styling

| Library | Version | Purpose |
|---------|---------|---------|
| **Tailwind CSS** | 4.1.x | Utility-first CSS, dark mode, glassmorphism | 
| **Framer Motion** | 12.x | Panel open/close, hover glow, 2D transitions |
| **@fontsource/inter** | 5.x | Clean professional typography |

### Backend

| Library | Version | Purpose |
|---------|---------|---------|
| **Node.js** | 22 LTS | Runtime (**required by spec**) |
| **Express** | 5.1.x | HTTP server (**required by spec**) |
| **cors** | 2.8.x | Cross-origin for dev (Vite proxy alternative) |
| **tsx** | 4.19.x | TypeScript execution for Node without build step |

### Dev Tooling

| Tool | Version | Purpose |
|------|---------|---------|
| **ESLint** | 9.x | Linting (flat config) |
| **Prettier** | 3.5.x | Formatting |
| **concurrently** | 9.x | Run frontend + backend with one command |
| **Vitest** | 3.1.x | Unit tests (Vite-native) |
| **Playwright** | 1.52.x | Visual regression / E2E if needed |

### Infrastructure / CI

| Tool | Purpose |
|------|---------|
| **GitHub Actions** | CI pipeline |
| **Docker** | Optional containerized deployment |
| **Azure Static Web Apps** or **Vercel** | Frontend hosting (if deployed beyond local) |
| **Azure App Service** or **Render** | Backend hosting (if deployed) |

---

## Architecture Recommendations

### Overall Pattern: **Client-Server with REST API**

```
┌─────────────────────────────────────┐
│          Vite Dev Server            │
│  React + R3F + Chart.js Frontend    │
│  (port 5173)                        │
│         │                           │
│    fetch(/api/*)                    │
│         │  (Vite proxy in dev)      │
└─────────┼───────────────────────────┘
          ▼
┌─────────────────────────────────────┐
│       Express Backend (port 3001)   │
│                                     │
│  GET /api/project-summary           │
│  GET /api/project-items             │
│  GET /api/sprint-metrics            │
│  GET /api/risks                     │
│  GET /api/team-activity             │
│  GET /api/roadmap                   │
│  GET /api/report/:id                │
│                                     │
│  ┌─────────────┐                    │
│  │ mockData.ts │ ← static JSON     │
│  └─────────────┘                    │
└─────────────────────────────────────┘
```

### Data Flow

1. App loads → Three.js scene initializes → camera fly-in animation plays
2. React components mount → `useEffect` hooks fetch from `/api/*` endpoints via a shared `api.ts` client
3. Data populates React state → R3F components render 3D scene graph reactively
4. User interactions (hover, click) update React state → camera animates to focus → detail panel slides in

### 3D Scene Architecture

```
<Canvas>
  <SceneSetup />          ← lights, fog, environment
  <CameraController />    ← fly-in, click-to-focus via gsap
  <ParticleBackground />  ← instanced mesh, 2000 particles
  <ProjectHierarchy />    ← force-directed 3D node graph
  <HealthRings />         ← animated torus geometries
  <BarTowers />           ← instanced box geometries
  <TimelinePath />        ← TubeGeometry + milestones
  <RiskRadar />           ← orbital animated nodes
  <EffectComposer>
    <Bloom />             ← selective bloom via layers
  </EffectComposer>
</Canvas>
<HtmlOverlay>
  <DashboardCards />      ← glassmorphism 2D panels
  <DetailPanel />         ← slide-in on click
  <SprintCharts />        ← Chart.js burndown/velocity
  <ActivityFeed />        ← scrollable feed
</HtmlOverlay>
```

### Key Design Decisions

1. **React Three Fiber over raw Three.js**: The spec has 7+ dashboard sections each with complex 3D elements. R3F's declarative model and React lifecycle integration prevents the "imperative spaghetti" that raw Three.js apps become at this scale. The underlying Three.js is still directly accessible when needed.

2. **Hybrid 2D+3D rendering**: Use Three.js for the immersive 3D elements (hierarchy graph, health rings, bar towers, timeline, radar). Use HTML overlays + Chart.js for data-dense 2D elements (burndown charts, activity feed, detail panels). This avoids rendering text in WebGL (poor quality) and leverages CSS for glassmorphism.

3. **Selective bloom via Three.js layers**: Assign glowing objects to layer 1, render bloom pass only on that layer. This avoids blooming the entire scene (which washes out text and UI).

4. **Instanced meshes for particles and nodes**: With 40+ story/task nodes and 2000+ particles, instancing is mandatory for 60fps performance.

5. **GSAP for camera animation**: Three.js camera transitions need smooth easing and sequencing. GSAP's timeline API is purpose-built for this and integrates cleanly with R3F via refs.

### Data Storage Strategy

**No database required** (per spec). Structure:

```
/server/data/
  projects.json
  epics.json
  features.json
  stories.json
  risks.json
  team.json
  activity.json
  roadmap.json
```

Each file is loaded once at server start into memory. The `/api/report/:id` endpoint does a simple lookup across all entity arrays by ID.

### Folder Structure (Recommended)

```
/
├── server/
│   ├── index.ts
│   ├── routes/
│   │   ├── projectRoutes.ts
│   │   ├── sprintRoutes.ts
│   │   ├── riskRoutes.ts
│   │   ├── teamRoutes.ts
│   │   └── reportRoutes.ts
│   └── data/
│       └── mockData.ts          ← typed mock data factory
├── client/
│   ├── index.html
│   ├── vite.config.ts
│   ├── src/
│   │   ├── main.tsx
│   │   ├── App.tsx
│   │   ├── api/
│   │   │   └── client.ts        ← fetch wrapper with error handling
│   │   ├── hooks/
│   │   │   └── useProjectData.ts
│   │   ├── scene/
│   │   │   ├── SceneSetup.tsx
│   │   │   ├── CameraController.tsx
│   │   │   ├── ParticleBackground.tsx
│   │   │   ├── ProjectHierarchy.tsx
│   │   │   ├── HealthRings.tsx
│   │   │   ├── BarTowers.tsx
│   │   │   ├── TimelinePath.tsx
│   │   │   └── RiskRadar.tsx
│   │   ├── components/
│   │   │   ├── DashboardCards.tsx
│   │   │   ├── DetailPanel.tsx
│   │   │   ├── SprintCharts.tsx
│   │   │   └── ActivityFeed.tsx
│   │   ├── styles/
│   │   │   └── globals.css
│   │   └── types/
│   │       └── index.ts
├── package.json                  ← root workspace scripts
├── README.md
└── tsconfig.json
```

---

## Security & Infrastructure

### Authentication & Authorization

**None required.** The spec explicitly states no authentication and no external services. The app serves mock data only.

### Data Protection

- No real user data is involved—mock data only.
- If this is ever extended to real data sources, add: API key auth, HTTPS enforcement, CORS lockdown, and input validation on `:id` params.

### Hosting & Deployment (If Deployed Beyond Local)

| Tier | Frontend | Backend | Est. Monthly Cost |
|------|----------|---------|-------------------|
| **Free/Dev** | Vercel free tier or GitHub Pages (static build) | Render free tier or Azure App Service F1 | $0 |
| **Small** (< 100 users) | Azure Static Web Apps Standard | Azure App Service B1 | ~$15/mo |
| **Medium** (demo events) | Azure Static Web Apps + CDN | Azure App Service S1 | ~$70/mo |

### Deployment Strategy

1. **Local-first** (primary use case): `npm run dev` starts both Vite and Express via `concurrently`.
2. **Static export option**: `vite build` produces static assets. Express serves them in production mode. Single-process deployment.
3. **Containerized**: Single Dockerfile—Express serves built Vite assets and API endpoints. Deploy anywhere (Azure Container Apps, Render, Fly.io).

### Operational Concerns

- **WebGL compatibility**: Test on Chrome, Edge, Firefox. Safari WebGL2 support is adequate but may have shader quirks. Include a fallback message for browsers without WebGL2.
- **GPU requirements**: Bloom + particles + 60fps needs a discrete GPU or recent integrated GPU. Document minimum specs in README.

---

## Risks & Trade-offs

### High Risk

| Risk | Impact | Mitigation |
|------|--------|------------|
| **WebGL performance on low-end hardware** | Dashboard stutters or crashes during exec demos | Profile early with Chrome DevTools Performance tab. Use instanced meshes, LOD, and `<PerformanceMonitor>` from drei to auto-degrade quality. Cap particles at 1000 on low-end. |
| **3D node graph layout complexity** | The hierarchy view (4 epics → 12 features → 40+ items) needs a force-directed or tree layout in 3D space | Use `d3-force-3d` for layout computation, feed positions into R3F. Prototype this section first. |
| **Scope creep in visual polish** | "Premium demo quality" is subjective—easy to spend unlimited time on effects | Define a visual baseline in Phase 1, polish in Phase 2. Use reference screenshots as acceptance criteria. |

### Medium Risk

| Risk | Impact | Mitigation |
|------|--------|------------|
| **GSAP licensing** | Free for open-source, but "Business" license needed for commercial products. If this dashboard is shown to customers, clarify license status. | Evaluate if `@react-spring/three` (MIT) can replace GSAP for camera work. GSAP is better but Spring is license-safe. |
| **Three.js breaking changes** | Three.js has frequent releases with occasional breaking changes to materials/shaders | Pin to exact version (e.g., `0.175.0`). Update deliberately. |
| **Browser-specific WebGL bugs** | Safari and Firefox handle GLSL shaders differently than Chrome | Test cross-browser in CI with Playwright. Use standardized shader code. |

### Trade-offs Accepted

- **React Three Fiber vs. raw Three.js**: R3F adds ~30KB bundle overhead and a learning curve for devs unfamiliar with it. Trade-off is worth it for the declarative model at this scene complexity.
- **Tailwind CSS vs. CSS Modules**: Tailwind is faster for prototyping glassmorphism utilities but adds a build dependency. Acceptable for this project scope.
- **Mock data in JSON files vs. in-memory factory**: JSON files are easier to edit; factory functions generate more realistic randomized data. **Recommendation**: Use a typed factory function (`generateMockData()`) that produces consistent data on each server start (seeded random), with an option to export to JSON.

---

## Open Questions

1. **Target browsers & hardware**: Is this strictly Chrome on modern laptops? Or must it run on conference room displays, iPads, or older hardware? This directly impacts visual effects budget.

2. **"Replace mock data with real APIs" scope**: The spec mentions documenting how to swap mock data for real APIs. Should the API client layer be designed with adapter patterns for Azure DevOps / Jira integration, or is a simple "change the URL" note sufficient?

3. **Deployment target**: Is this local-only, or will it be hosted for ongoing access? This affects whether to invest in Docker, CI/CD, and hosting infrastructure.

4. **Interaction depth**: The spec mentions click-to-focus and detail panels. Should there be drill-down navigation (e.g., click Epic → zoom into its Features → click Feature → see Stories)? Or is one level of detail sufficient?

5. **Accessibility requirements**: The dark futuristic aesthetic with neon colors may have contrast ratio issues. Are WCAG compliance targets required, or is this purely a visual demo?

6. **Performance budget**: What is the minimum acceptable frame rate? 60fps on all machines, or 30fps acceptable on integrated GPUs?

7. **Team familiarity with Three.js/R3F**: If the team has no Three.js experience, the 3D hierarchy view and custom shaders will require significant ramp-up time (2-3 weeks).

---

## Implementation Recommendations

### Phase 1: Foundation & MVP (Week 1-2)

**Goal**: Running app with all 7 API endpoints, basic 3D scene, and 2-3 dashboard sections.

1. **Scaffold project**: Vite + React + TypeScript frontend, Express + TypeScript backend, concurrently for dev script.
2. **Build mock data factory**: Typed interfaces for all entities (Project, Epic, Feature, Story, Risk, etc.). Generate minimum required data volumes.
3. **Implement all 7 API endpoints**: Simple route handlers returning mock data. This unblocks all frontend work.
4. **Basic Three.js scene**: Canvas, lights, camera, orbit controls, particle background. Confirm 60fps baseline.
5. **Project Overview section**: Glassmorphism cards with health score, completion %, sprint info. Animated counters.
6. **Sprint Metrics section**: Chart.js burndown and velocity charts in HTML overlay.

**Quick wins in Phase 1:**
- Particle background + bloom gives instant "wow factor" with ~50 lines of R3F code
- Animated counters (0 → 87%) using gsap or framer-motion are visually impressive and trivial to implement
- Glassmorphism cards via Tailwind (`backdrop-blur-xl bg-white/10 border border-white/20`) take minutes

### Phase 2: Core 3D Visualizations (Week 2-3)

7. **3D Project Hierarchy**: Force-directed layout with d3-force-3d, render as instanced spheres with connecting lines. Color-code by status. Click-to-focus camera movement.
8. **Health Rings**: Animated torus geometries with shader-based progress fill.
9. **Bar Towers**: Instanced box geometries for progress visualization.
10. **Risk Radar**: Orbital animated nodes grouped by severity.

### Phase 3: Remaining Sections & Polish (Week 3-4)

11. **Timeline/Roadmap**: TubeGeometry path with milestone markers.
12. **Team Activity Feed**: Scrollable HTML panel with animated entry transitions.
13. **Detail Panel**: Slide-in panel on click, populated from `/api/report/:id`.
14. **Camera fly-in**: GSAP timeline for initial load animation.
15. **Cross-browser testing**: Chrome, Edge, Firefox, Safari.
16. **Performance optimization**: Profile, optimize draw calls, add quality auto-degradation.
17. **README & documentation**: Setup instructions, architecture notes, mock data replacement guide.

### Prototype-First Recommendations

Before committing to full implementation, prototype these in isolation:

- **3D hierarchy graph**: This is the most complex visual element. Build a standalone R3F scene with 50 nodes, force layout, click interaction, and camera animation. Validate performance and usability.
- **Selective bloom**: Test bloom + glassmorphism HTML overlay coexistence. Bloom can interfere with HTML overlays if not configured correctly (render order, layer masking).
- **GSAP + R3F integration**: Confirm GSAP can smoothly animate R3F camera refs without fighting React's render cycle.
