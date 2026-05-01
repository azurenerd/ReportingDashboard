# Research

_No research has been documented yet._

## Research technology stack for ReportingDashboard

_Researched on 2026-05-01 10:01 UTC_

### Summary

The ReportingDashboard is a polished, 3D animated project reporting dashboard designed as a futuristic "project command center" for executive presentations. Based on the feature specification, the prescribed stack is **Node.js/Express** (backend) with **Three.js + Vite** (frontend), serving mock JSON data with no database or authentication. The existing repository contains a .NET 8 Blazor scaffold at `src/ReportingDashboard/`, which will need to be replaced or run alongside the spec-mandated Node.js stack. This research evaluates the best libraries, patterns, and implementation strategies within the Node.js/Three.js ecosystem to deliver a visually impressive, interactive 3D dashboard. **Primary Recommendation:** Build a monorepo with a Vite + TypeScript + React Three Fiber frontend and a lightweight Express backend serving mock JSON. Use GSAP for animations, `@react-three/drei` for 3D helpers, and Zustand for state management. Target a 3-phase delivery: static data + scene → interactivity → polish/animations. ---

### Key Findings

- The spec mandates **7 REST API endpoints** (`/api/project-summary`, `/api/project-items`, `/api/sprint-metrics`, `/api/risks`, `/api/team-activity`, `/api/roadmap`, `/api/report/:id`) — all serving mock JSON with no database.
- **Three.js** is the preferred 3D engine; React Three Fiber (R3F) wraps it idiomatically for React and is the most productive path for component-based 3D UIs.
- The dashboard requires **7 distinct sections** (Project Overview, 3D Hierarchy, Sprint Metrics, Risk Radar, Team Activity, Timeline, Detail Panel) — each with specific animation and interaction requirements.
- **Mock data volume** is modest (1 project, 4 epics, 12 features, 40+ tasks, 8 risks, 10 team members, 20 activity events, 6 milestones) — in-memory JSON is sufficient.
- The visual style is a **dark-mode, glassmorphism, neon-accent** aesthetic with bloom/glow, particle backgrounds, and floating 3D panels — this requires post-processing (UnrealBloomPass) and custom shaders.
- **No authentication, no database, no external services** — the entire app must run locally via `npm install && npm run dev`.
- The existing .NET 8 project in the repo (`src/ReportingDashboard/`) does not match the spec's prescribed stack and should be treated as a prototype/placeholder.
- Camera fly-in, click-to-focus, hover glow, animated counters, and orbit controls are **mandatory interactions**, not nice-to-haves. ---
- Set up monorepo with Vite + React + TypeScript (client) and Express (server)
- Implement all 7 mock API endpoints with realistic data meeting volume requirements
- Create shared TypeScript types for all data models
- Basic R3F canvas with OrbitControls and dark background
- Render Project Overview as HTML overlay with fetched data
- **Deliverable:** Working full-stack app with data flowing from Express → React
- Build the 3D Project Hierarchy (force-directed or tree layout with `d3-force-3d`)
- Implement Risk Radar as animated orbital visualization
- Build 3D Timeline with milestone nodes
- Add click-to-select and hover-glow interactions
- Implement Detail Panel (slides in on node click)
- Sprint Metrics section with Recharts burndown/velocity
- Team Activity feed with animated pulses
- **Deliverable:** All 7 sections functional with basic interactions
- Camera fly-in sequence on load (GSAP timeline)
- Bloom/glow post-processing via `@react-three/postprocessing`
- Glassmorphism CSS for all panels (backdrop-filter, border glow)
- Particle background field
- Animated counters and progress rings
- Hover depth effects and neon accent lighting
- Smooth panel open/close transitions
- Performance profiling and quality presets
- **Deliverable:** Executive-ready polished demo
- **Particle background + dark theme** (30 min) — immediately sets the visual tone
- **Project Overview card with animated counters** (1 hr) — proves data flow end-to-end
- **Basic 3D hierarchy with colored nodes** (2 hrs) — the "wow factor" centerpiece
- **HTML overlay positioning in 3D space:** Build a quick proof-of-concept with 2–3 glassmorphism panels floating in a Three.js scene. Verify z-ordering, mouse events pass through correctly, and text remains crisp.
- **Bloom + transparency interaction:** Test that bloom post-processing doesn't blow out glassmorphism panels or make text unreadable.
- **Performance on integrated GPU:** Run a stress test with 60 3D nodes + particles + bloom on the weakest target hardware before committing to high particle counts.

### Recommended Tools & Technologies

- | Layer | Library | Version | Purpose | |-------|---------|---------|---------| | Build tool | **Vite** | 6.x | Fast HMR, TypeScript support, spec-mandated | | Language | **TypeScript** | 5.7+ | Type safety for complex 3D scene graph | | UI Framework | **React** | 19.x | Component model for dashboard sections | | 3D Engine | **Three.js** | r175+ | WebGL rendering, spec-mandated | | React 3D Binding | **@react-three/fiber** | 9.x | Declarative Three.js in React | | 3D Helpers | **@react-three/drei** | 10.x | OrbitControls, Text, Html overlays, Environment | | Post-processing | **@react-three/postprocessing** | 3.x | Bloom, glow, vignette effects | | Animation | **GSAP** | 3.12+ | Camera fly-in, counter animations, smooth transitions | | State Management | **Zustand** | 5.x | Lightweight, minimal boilerplate for shared state | | 2D Charts (sprint metrics) | **recharts** | 2.15+ | React-native charting for burndown/velocity (rendered in Html overlays) | | CSS | **Tailwind CSS** | 4.x | Utility-first, fast glassmorphism/dark-mode styling | | HTTP Client | **fetch API** (native) | — | No extra dependency needed for 7 endpoints | **Why React Three Fiber over vanilla Three.js:** The dashboard has 7+ interactive sections with shared state (selected node, active panel). R3F's component model makes it natural to compose 3D scenes with React state, handle click/hover events declaratively, and mix HTML overlays (detail panels, metrics cards) with 3D content. Vanilla Three.js would require manual DOM↔WebGL bridging that R3F handles out of the box. **Why GSAP over Framer Motion for 3D:** GSAP provides timeline-based sequencing for camera fly-ins and can tween Three.js object properties directly. Framer Motion is great for DOM but doesn't natively animate Three.js objects. | Layer | Library | Version | Purpose | |-------|---------|---------|---------| | Runtime | **Node.js** | 22 LTS | Spec-mandated | | Framework | **Express** | 5.x | Spec-mandated, REST API serving | | CORS | **cors** | 2.8+ | Cross-origin for Vite dev server | | Dev runner | **nodemon** | 3.x | Auto-reload during development | | Data | **JSON files / in-memory** | — | No database per spec | | Tool | Version | Purpose | |------|---------|---------| | **concurrently** | 9.x | Run client + server with single `npm run dev` | | **ESLint** | 9.x | Linting with flat config | | **Prettier** | 3.x | Code formatting | | **TypeScript** | 5.7+ | Shared types between client/server | | Tool | Version | Purpose | |------|---------|---------| | **Vitest** | 3.x | Unit tests, Vite-native | | **Playwright** | 1.50+ | E2E / visual regression for 3D renders | | **MSW (Mock Service Worker)** | 2.x | API mocking in tests | ---
```
┌─────────────────────────────────────────────────┐
│                  Browser (Desktop)                │
│                                                   │
│  ┌───────────┐  ┌──────────┐  ┌───────────────┐ │
│  │ React App │──│ Zustand   │──│ API Service   │ │
│  │ (Vite)    │  │ Store     │  │ (fetch)       │ │
│  └─────┬─────┘  └──────────┘  └───────┬───────┘ │
│        │                               │         │
│  ┌─────▼─────────────────────┐         │         │
│  │ R3F Canvas                │         │         │
│  │  ├─ HierarchyScene       │         │         │
│  │  ├─ RiskRadarScene       │         │         │
│  │  ├─ TimelineScene        │         │         │
│  │  ├─ ParticleBackground   │         │         │
│  │  └─ PostProcessing       │         │         │
│  └───────────────────────────┘         │         │
│                                        │         │
│  ┌─────────────────────────────┐       │         │
│  │ HTML Overlay Panels         │       │         │
│  │  ├─ ProjectOverview        │       │         │
│  │  ├─ SprintMetrics          │       │         │
│  │  ├─ TeamActivity           │       │         │
│  │  └─ DetailPanel            │       │         │
│  └─────────────────────────────┘       │         │
└────────────────────────────────────────┼─────────┘
                                         │
                              HTTP (localhost:3001)
                                         │
┌────────────────────────────────────────▼─────────┐
│              Express Server (Node.js)             │
│  ┌──────────┐  ┌────────────────────────┐        │
│  │ Routes   │──│ Mock Data (JSON files) │        │
│  │ 7 GETs   │  │ /server/data/          │        │
│  └──────────┘  └────────────────────────┘        │
└──────────────────────────────────────────────────┘
```
- **Hybrid 3D + HTML Rendering:** Use R3F's `<Html>` component from drei to render glassmorphism panels as HTML overlays positioned in 3D space. This gives CSS styling for cards while maintaining 3D depth. The 3D hierarchy, risk radar, and timeline are pure Three.js geometry; metrics cards and text-heavy panels are HTML overlays.
- **Scene Composition Pattern:** Each dashboard section (Hierarchy, Radar, Timeline) is an independent R3F component with its own sub-scene. A `DashboardLayout` component positions them in 3D space. Camera targets are predefined positions that GSAP tweens to on navigation.
- **State Flow:** Zustand store holds: `selectedNode`, `activePanel`, `dashboardData` (fetched from API), `cameraTarget`, `isLoading`. Components subscribe to slices they need. No prop drilling.
- **Data Fetching Pattern:** A single `useEffect` on mount fetches all 6 list endpoints in parallel (`Promise.all`). Detail panel fetches `/api/report/:id` on demand when a node is clicked. Loading states render skeleton/shimmer animations.
- **Mock Data Replacement Strategy:** All API calls go through a single `api.ts` module. To replace mock data with real APIs later, only this module needs updating — swap base URLs and add authentication headers. Document this in the README.
```
/server
  index.js              # Express app, CORS, routes
  routes/
    project.js          # All 7 GET endpoints
  data/
    mockData.js          # Exported mock JSON objects

/client
  index.html
  package.json
  vite.config.ts
  src/
    main.tsx             # React entry point
    App.tsx              # Layout + R3F Canvas
    api.ts               # All fetch calls (single replacement point)
    store.ts             # Zustand store
    scene/
      HierarchyScene.tsx  # 3D node graph
      RiskRadar.tsx       # Animated radar visualization
      Timeline3D.tsx      # 3D horizontal timeline
      ParticleField.tsx   # Background particles
      PostEffects.tsx     # Bloom, glow
    components/
      ProjectOverview.tsx
      SprintMetrics.tsx
      TeamActivity.tsx
      DetailPanel.tsx
      LoadingState.tsx
    styles/
      globals.css         # Tailwind + glassmorphism utilities

/package.json            # Root: concurrently script
/README.md
``` ---

### Considerations & Risks

- **None required.** The spec explicitly states no authentication. Mock data only.
- **Future-proofing note:** When replacing mock data with real APIs, add an auth middleware layer in Express (e.g., `passport.js` with Azure AD/Entra ID bearer tokens) and a React auth context wrapping the app.
- No sensitive data — all mock. No encryption needed.
- When transitioning to real data: enforce HTTPS, add CSP headers, sanitize any user-generated content rendered in HTML overlays to prevent XSS. | Tier | Option | Cost Estimate | |------|--------|---------------| | **Demo/Small** | Azure Static Web Apps (frontend) + Azure App Service B1 (backend) | ~$15/month | | **Demo/Small** | Single Azure Container App (both) | ~$10/month | | **Zero-cost** | Local only per spec | $0 | For executive demo purposes, **Azure Static Web Apps** with a linked API backend is the simplest deployment. The frontend builds to static files; the Express backend can run as a linked API or a separate App Service.
- **WebGL Compatibility:** Ensure target demo machines have hardware-accelerated WebGL. Test on the actual presentation hardware.
- **Performance Budget:** Three.js scenes with bloom post-processing, 60+ 3D nodes, and particle systems can strain integrated GPUs. Profile on target hardware; reduce particle count or disable bloom as fallback. --- | Risk | Impact | Mitigation | |------|--------|------------| | **WebGL performance on target hardware** | Demo fails visually on exec laptop with integrated GPU | Profile early; implement quality presets (low/high); reduce particles and disable bloom on low-end | | **3D interaction complexity** | Click/hover on 3D objects is harder than DOM — raycasting edge cases | Use R3F's built-in event system (`onPointerOver`, `onClick`) which handles raycasting automatically | | **Scope creep from visual polish** | 9 mandatory animation types + 7 sections = large surface area | Phase delivery; get functionality working first, add polish in Phase 3 | | Risk | Impact | Mitigation | |------|--------|------------| | **R3F + HTML overlay z-fighting** | Glassmorphism panels may clip or layer incorrectly with 3D content | Use drei's `<Html>` with `zIndexRange` and `occlude` props; test with actual layouts early | | **GSAP + R3F animation conflicts** | Two animation systems fighting over the same objects | Use GSAP only for camera and non-R3F DOM; use R3F's `useFrame` for per-frame 3D animations | | **Existing .NET project confusion** | Repo already has `src/ReportingDashboard/` as .NET 8 | Decide upfront: replace it entirely, or keep it and add a `/src/ReportingDashboard-web/` directory |
- **React Three Fiber vs. vanilla Three.js:** R3F adds ~30KB gzipped but saves significant development time for interactive, stateful 3D UIs. Worth it for this project's complexity.
- **TypeScript vs. JavaScript:** TypeScript adds build complexity but is essential for a 7-section dashboard with shared 3D types (Vector3, node hierarchies). Strong recommendation to use TypeScript.
- **Recharts for 2D charts vs. all-3D:** The spec allows Chart.js or D3. Recharts integrates naturally with React and renders inside HTML overlays. Pure 3D charts are visually impressive but 3× the development effort for burndown/velocity charts that need axis labels and legends. ---
- **Existing .NET project disposition:** Should `src/ReportingDashboard/` (the .NET 8 Blazor app) be replaced, kept as a separate implementation, or removed? The spec mandates Node.js/Express.
- **Presentation hardware:** What GPU/browser will be used for the executive demo? This determines the performance budget for particles, bloom, and geometry count.
- **Real API transition scope:** The spec mentions documenting how to replace mock data — is there a known target system (Azure DevOps, Jira, etc.) that shapes the mock data schema?
- **Monorepo vs. standalone:** Should the ReportingDashboard live inside the AgentSquad monorepo (current location) or be extracted to its own repository? The Node.js stack is orthogonal to the .NET solution.
- **Accessibility requirements:** The spec doesn't mention accessibility. Should the dashboard meet WCAG guidelines, or is it purely a visual demo?
- **Browser support:** Desktop only per spec — but which browsers? Chrome-only is simplest for WebGL2; Firefox and Edge add testing surface. ---

### Detailed Analysis

Now I have the full picture. The spec calls for Node.js/Express + Three.js, but the existing repo has a .NET 8 project. The spec explicitly defines the tech stack. Let me produce the research document.

# ReportingDashboard — Technology Stack Research

## Executive Summary

The ReportingDashboard is a polished, 3D animated project reporting dashboard designed as a futuristic "project command center" for executive presentations. Based on the feature specification, the prescribed stack is **Node.js/Express** (backend) with **Three.js + Vite** (frontend), serving mock JSON data with no database or authentication. The existing repository contains a .NET 8 Blazor scaffold at `src/ReportingDashboard/`, which will need to be replaced or run alongside the spec-mandated Node.js stack. This research evaluates the best libraries, patterns, and implementation strategies within the Node.js/Three.js ecosystem to deliver a visually impressive, interactive 3D dashboard.

**Primary Recommendation:** Build a monorepo with a Vite + TypeScript + React Three Fiber frontend and a lightweight Express backend serving mock JSON. Use GSAP for animations, `@react-three/drei` for 3D helpers, and Zustand for state management. Target a 3-phase delivery: static data + scene → interactivity → polish/animations.

---

## Key Findings

- The spec mandates **7 REST API endpoints** (`/api/project-summary`, `/api/project-items`, `/api/sprint-metrics`, `/api/risks`, `/api/team-activity`, `/api/roadmap`, `/api/report/:id`) — all serving mock JSON with no database.
- **Three.js** is the preferred 3D engine; React Three Fiber (R3F) wraps it idiomatically for React and is the most productive path for component-based 3D UIs.
- The dashboard requires **7 distinct sections** (Project Overview, 3D Hierarchy, Sprint Metrics, Risk Radar, Team Activity, Timeline, Detail Panel) — each with specific animation and interaction requirements.
- **Mock data volume** is modest (1 project, 4 epics, 12 features, 40+ tasks, 8 risks, 10 team members, 20 activity events, 6 milestones) — in-memory JSON is sufficient.
- The visual style is a **dark-mode, glassmorphism, neon-accent** aesthetic with bloom/glow, particle backgrounds, and floating 3D panels — this requires post-processing (UnrealBloomPass) and custom shaders.
- **No authentication, no database, no external services** — the entire app must run locally via `npm install && npm run dev`.
- The existing .NET 8 project in the repo (`src/ReportingDashboard/`) does not match the spec's prescribed stack and should be treated as a prototype/placeholder.
- Camera fly-in, click-to-focus, hover glow, animated counters, and orbit controls are **mandatory interactions**, not nice-to-haves.

---

## Recommended Technology Stack

### Frontend

| Layer | Library | Version | Purpose |
|-------|---------|---------|---------|
| Build tool | **Vite** | 6.x | Fast HMR, TypeScript support, spec-mandated |
| Language | **TypeScript** | 5.7+ | Type safety for complex 3D scene graph |
| UI Framework | **React** | 19.x | Component model for dashboard sections |
| 3D Engine | **Three.js** | r175+ | WebGL rendering, spec-mandated |
| React 3D Binding | **@react-three/fiber** | 9.x | Declarative Three.js in React |
| 3D Helpers | **@react-three/drei** | 10.x | OrbitControls, Text, Html overlays, Environment |
| Post-processing | **@react-three/postprocessing** | 3.x | Bloom, glow, vignette effects |
| Animation | **GSAP** | 3.12+ | Camera fly-in, counter animations, smooth transitions |
| State Management | **Zustand** | 5.x | Lightweight, minimal boilerplate for shared state |
| 2D Charts (sprint metrics) | **recharts** | 2.15+ | React-native charting for burndown/velocity (rendered in Html overlays) |
| CSS | **Tailwind CSS** | 4.x | Utility-first, fast glassmorphism/dark-mode styling |
| HTTP Client | **fetch API** (native) | — | No extra dependency needed for 7 endpoints |

**Why React Three Fiber over vanilla Three.js:** The dashboard has 7+ interactive sections with shared state (selected node, active panel). R3F's component model makes it natural to compose 3D scenes with React state, handle click/hover events declaratively, and mix HTML overlays (detail panels, metrics cards) with 3D content. Vanilla Three.js would require manual DOM↔WebGL bridging that R3F handles out of the box.

**Why GSAP over Framer Motion for 3D:** GSAP provides timeline-based sequencing for camera fly-ins and can tween Three.js object properties directly. Framer Motion is great for DOM but doesn't natively animate Three.js objects.

### Backend

| Layer | Library | Version | Purpose |
|-------|---------|---------|---------|
| Runtime | **Node.js** | 22 LTS | Spec-mandated |
| Framework | **Express** | 5.x | Spec-mandated, REST API serving |
| CORS | **cors** | 2.8+ | Cross-origin for Vite dev server |
| Dev runner | **nodemon** | 3.x | Auto-reload during development |
| Data | **JSON files / in-memory** | — | No database per spec |

### Tooling & DX

| Tool | Version | Purpose |
|------|---------|---------|
| **concurrently** | 9.x | Run client + server with single `npm run dev` |
| **ESLint** | 9.x | Linting with flat config |
| **Prettier** | 3.x | Code formatting |
| **TypeScript** | 5.7+ | Shared types between client/server |

### Testing

| Tool | Version | Purpose |
|------|---------|---------|
| **Vitest** | 3.x | Unit tests, Vite-native |
| **Playwright** | 1.50+ | E2E / visual regression for 3D renders |
| **MSW (Mock Service Worker)** | 2.x | API mocking in tests |

---

## Architecture Recommendations

### Overall Architecture

```
┌─────────────────────────────────────────────────┐
│                  Browser (Desktop)                │
│                                                   │
│  ┌───────────┐  ┌──────────┐  ┌───────────────┐ │
│  │ React App │──│ Zustand   │──│ API Service   │ │
│  │ (Vite)    │  │ Store     │  │ (fetch)       │ │
│  └─────┬─────┘  └──────────┘  └───────┬───────┘ │
│        │                               │         │
│  ┌─────▼─────────────────────┐         │         │
│  │ R3F Canvas                │         │         │
│  │  ├─ HierarchyScene       │         │         │
│  │  ├─ RiskRadarScene       │         │         │
│  │  ├─ TimelineScene        │         │         │
│  │  ├─ ParticleBackground   │         │         │
│  │  └─ PostProcessing       │         │         │
│  └───────────────────────────┘         │         │
│                                        │         │
│  ┌─────────────────────────────┐       │         │
│  │ HTML Overlay Panels         │       │         │
│  │  ├─ ProjectOverview        │       │         │
│  │  ├─ SprintMetrics          │       │         │
│  │  ├─ TeamActivity           │       │         │
│  │  └─ DetailPanel            │       │         │
│  └─────────────────────────────┘       │         │
└────────────────────────────────────────┼─────────┘
                                         │
                              HTTP (localhost:3001)
                                         │
┌────────────────────────────────────────▼─────────┐
│              Express Server (Node.js)             │
│  ┌──────────┐  ┌────────────────────────┐        │
│  │ Routes   │──│ Mock Data (JSON files) │        │
│  │ 7 GETs   │  │ /server/data/          │        │
│  └──────────┘  └────────────────────────┘        │
└──────────────────────────────────────────────────┘
```

### Key Architectural Decisions

1. **Hybrid 3D + HTML Rendering:** Use R3F's `<Html>` component from drei to render glassmorphism panels as HTML overlays positioned in 3D space. This gives CSS styling for cards while maintaining 3D depth. The 3D hierarchy, risk radar, and timeline are pure Three.js geometry; metrics cards and text-heavy panels are HTML overlays.

2. **Scene Composition Pattern:** Each dashboard section (Hierarchy, Radar, Timeline) is an independent R3F component with its own sub-scene. A `DashboardLayout` component positions them in 3D space. Camera targets are predefined positions that GSAP tweens to on navigation.

3. **State Flow:** Zustand store holds: `selectedNode`, `activePanel`, `dashboardData` (fetched from API), `cameraTarget`, `isLoading`. Components subscribe to slices they need. No prop drilling.

4. **Data Fetching Pattern:** A single `useEffect` on mount fetches all 6 list endpoints in parallel (`Promise.all`). Detail panel fetches `/api/report/:id` on demand when a node is clicked. Loading states render skeleton/shimmer animations.

5. **Mock Data Replacement Strategy:** All API calls go through a single `api.ts` module. To replace mock data with real APIs later, only this module needs updating — swap base URLs and add authentication headers. Document this in the README.

### Folder Structure (Spec-Aligned)

```
/server
  index.js              # Express app, CORS, routes
  routes/
    project.js          # All 7 GET endpoints
  data/
    mockData.js          # Exported mock JSON objects

/client
  index.html
  package.json
  vite.config.ts
  src/
    main.tsx             # React entry point
    App.tsx              # Layout + R3F Canvas
    api.ts               # All fetch calls (single replacement point)
    store.ts             # Zustand store
    scene/
      HierarchyScene.tsx  # 3D node graph
      RiskRadar.tsx       # Animated radar visualization
      Timeline3D.tsx      # 3D horizontal timeline
      ParticleField.tsx   # Background particles
      PostEffects.tsx     # Bloom, glow
    components/
      ProjectOverview.tsx
      SprintMetrics.tsx
      TeamActivity.tsx
      DetailPanel.tsx
      LoadingState.tsx
    styles/
      globals.css         # Tailwind + glassmorphism utilities

/package.json            # Root: concurrently script
/README.md
```

---

## Security & Infrastructure

### Authentication & Authorization
- **None required.** The spec explicitly states no authentication. Mock data only.
- **Future-proofing note:** When replacing mock data with real APIs, add an auth middleware layer in Express (e.g., `passport.js` with Azure AD/Entra ID bearer tokens) and a React auth context wrapping the app.

### Data Protection
- No sensitive data — all mock. No encryption needed.
- When transitioning to real data: enforce HTTPS, add CSP headers, sanitize any user-generated content rendered in HTML overlays to prevent XSS.

### Hosting & Deployment (If Needed Beyond Local)

| Tier | Option | Cost Estimate |
|------|--------|---------------|
| **Demo/Small** | Azure Static Web Apps (frontend) + Azure App Service B1 (backend) | ~$15/month |
| **Demo/Small** | Single Azure Container App (both) | ~$10/month |
| **Zero-cost** | Local only per spec | $0 |

For executive demo purposes, **Azure Static Web Apps** with a linked API backend is the simplest deployment. The frontend builds to static files; the Express backend can run as a linked API or a separate App Service.

### Operational Concerns
- **WebGL Compatibility:** Ensure target demo machines have hardware-accelerated WebGL. Test on the actual presentation hardware.
- **Performance Budget:** Three.js scenes with bloom post-processing, 60+ 3D nodes, and particle systems can strain integrated GPUs. Profile on target hardware; reduce particle count or disable bloom as fallback.

---

## Risks & Trade-offs

### High Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| **WebGL performance on target hardware** | Demo fails visually on exec laptop with integrated GPU | Profile early; implement quality presets (low/high); reduce particles and disable bloom on low-end |
| **3D interaction complexity** | Click/hover on 3D objects is harder than DOM — raycasting edge cases | Use R3F's built-in event system (`onPointerOver`, `onClick`) which handles raycasting automatically |
| **Scope creep from visual polish** | 9 mandatory animation types + 7 sections = large surface area | Phase delivery; get functionality working first, add polish in Phase 3 |

### Medium Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| **R3F + HTML overlay z-fighting** | Glassmorphism panels may clip or layer incorrectly with 3D content | Use drei's `<Html>` with `zIndexRange` and `occlude` props; test with actual layouts early |
| **GSAP + R3F animation conflicts** | Two animation systems fighting over the same objects | Use GSAP only for camera and non-R3F DOM; use R3F's `useFrame` for per-frame 3D animations |
| **Existing .NET project confusion** | Repo already has `src/ReportingDashboard/` as .NET 8 | Decide upfront: replace it entirely, or keep it and add a `/src/ReportingDashboard-web/` directory |

### Trade-offs

1. **React Three Fiber vs. vanilla Three.js:** R3F adds ~30KB gzipped but saves significant development time for interactive, stateful 3D UIs. Worth it for this project's complexity.
2. **TypeScript vs. JavaScript:** TypeScript adds build complexity but is essential for a 7-section dashboard with shared 3D types (Vector3, node hierarchies). Strong recommendation to use TypeScript.
3. **Recharts for 2D charts vs. all-3D:** The spec allows Chart.js or D3. Recharts integrates naturally with React and renders inside HTML overlays. Pure 3D charts are visually impressive but 3× the development effort for burndown/velocity charts that need axis labels and legends.

---

## Open Questions

1. **Existing .NET project disposition:** Should `src/ReportingDashboard/` (the .NET 8 Blazor app) be replaced, kept as a separate implementation, or removed? The spec mandates Node.js/Express.
2. **Presentation hardware:** What GPU/browser will be used for the executive demo? This determines the performance budget for particles, bloom, and geometry count.
3. **Real API transition scope:** The spec mentions documenting how to replace mock data — is there a known target system (Azure DevOps, Jira, etc.) that shapes the mock data schema?
4. **Monorepo vs. standalone:** Should the ReportingDashboard live inside the AgentSquad monorepo (current location) or be extracted to its own repository? The Node.js stack is orthogonal to the .NET solution.
5. **Accessibility requirements:** The spec doesn't mention accessibility. Should the dashboard meet WCAG guidelines, or is it purely a visual demo?
6. **Browser support:** Desktop only per spec — but which browsers? Chrome-only is simplest for WebGL2; Firefox and Edge add testing surface.

---

## Implementation Recommendations

### Phase 1 — Foundation & Data (Days 1–3)
- Set up monorepo with Vite + React + TypeScript (client) and Express (server)
- Implement all 7 mock API endpoints with realistic data meeting volume requirements
- Create shared TypeScript types for all data models
- Basic R3F canvas with OrbitControls and dark background
- Render Project Overview as HTML overlay with fetched data
- **Deliverable:** Working full-stack app with data flowing from Express → React

### Phase 2 — 3D Scenes & Interactivity (Days 4–8)
- Build the 3D Project Hierarchy (force-directed or tree layout with `d3-force-3d`)
- Implement Risk Radar as animated orbital visualization
- Build 3D Timeline with milestone nodes
- Add click-to-select and hover-glow interactions
- Implement Detail Panel (slides in on node click)
- Sprint Metrics section with Recharts burndown/velocity
- Team Activity feed with animated pulses
- **Deliverable:** All 7 sections functional with basic interactions

### Phase 3 — Visual Polish & Animation (Days 9–12)
- Camera fly-in sequence on load (GSAP timeline)
- Bloom/glow post-processing via `@react-three/postprocessing`
- Glassmorphism CSS for all panels (backdrop-filter, border glow)
- Particle background field
- Animated counters and progress rings
- Hover depth effects and neon accent lighting
- Smooth panel open/close transitions
- Performance profiling and quality presets
- **Deliverable:** Executive-ready polished demo

### Quick Wins (Demonstrate Value Early)
1. **Particle background + dark theme** (30 min) — immediately sets the visual tone
2. **Project Overview card with animated counters** (1 hr) — proves data flow end-to-end
3. **Basic 3D hierarchy with colored nodes** (2 hrs) — the "wow factor" centerpiece

### Prototype Before Committing
- **HTML overlay positioning in 3D space:** Build a quick proof-of-concept with 2–3 glassmorphism panels floating in a Three.js scene. Verify z-ordering, mouse events pass through correctly, and text remains crisp.
- **Bloom + transparency interaction:** Test that bloom post-processing doesn't blow out glassmorphism panels or make text unreadable.
- **Performance on integrated GPU:** Run a stress test with 60 3D nodes + particles + bloom on the weakest target hardware before committing to high particle counts.
