# Research

_No research has been documented yet._

## Research technology stack for ReportingDashboard

_Researched on 2026-05-01 10:49 UTC_

### Summary

The ReportingDashboard is a full-stack web application that serves as a futuristic 3D "project command center" for visualizing project management data during executive demos and engineering leadership reviews. The spec mandates **Node.js + Express** backend, **Three.js** for WebGL 3D rendering, optional **React + TypeScript** frontend with **Vite** bundler, and **mock JSON data only** (no database, no auth, no external services). This research evaluates the best libraries, patterns, and tooling within this mandated stack to deliver a premium, animated, interactive dashboard with glassmorphism aesthetics, 3D node hierarchies, animated charts, and smooth camera controls—all runnable locally with `npm install && npm run dev`. **Primary Recommendation:** Use **React 18 + TypeScript + Vite** for the frontend (structured component model pays off at this complexity), **React Three Fiber** as the React-idiomatic Three.js wrapper, **Express 4** for the REST API, and **GSAP** for non-3D animations. This combination maximizes developer productivity, visual polish, and maintainability while staying fully within the mandated stack. ---

### Key Findings

- The spec requires 7 distinct dashboard sections (Project Overview, 3D Hierarchy, Sprint Metrics, Risk Radar, Team Activity, Timeline, Detail Panel) plus 7 REST endpoints—this is moderate complexity favoring a component-based frontend over vanilla JS.
- **Three.js r170+** is the mandated 3D engine; wrapping it with **React Three Fiber (R3F) v9** eliminates imperative boilerplate and integrates cleanly with React's component lifecycle.
- The 3D Project Hierarchy View (epics → features → stories as floating connected nodes) is the highest-risk, highest-effort section—it requires force-directed graph layout in 3D space.
- Mock data volume (4 epics, 12 features, 40+ stories, 8 risks, 10 team members) is small enough that no database or pagination is needed—in-memory JSON files served by Express suffice.
- The "dark futuristic SaaS" aesthetic (glassmorphism, bloom, glow, particles) maps directly to Three.js post-processing effects (`UnrealBloomPass`, `EffectComposer`) and CSS `backdrop-filter`.
- No authentication or external services are required—this dramatically simplifies the backend to a thin mock-data API layer.
- The biggest technical risk is **3D performance on low-end hardware**—bloom, particles, and 50+ animated nodes can drop below 60fps without careful optimization.
- Vite's HMR and React Three Fiber's declarative model will accelerate iteration on visual polish—the spec's quality bar ("premium futuristic product demo") demands rapid visual experimentation. ---
- Set up monorepo (Vite + Express + concurrently)
- Build Express server with all 7 endpoints returning mock JSON
- Create R3F Canvas with particle background, bloom post-processing, and orbit controls
- Build one glassmorphism `<GlassCard>` component
- **Deliverable:** Spinning 3D scene with glowing particles + one data card fetching from API
- **Why first:** Validates the entire stack end-to-end and proves the visual aesthetic early
- Project Overview panel (animated counters, health ring)
- Sprint Metrics (Chart.js burndown, velocity bars)
- Team Activity feed (animated list)
- Detail Panel (slide-out on click)
- **Deliverable:** Functional 2D dashboard overlaying the 3D background
- **Particle background + bloom** (2 hours): Immediately makes the app look "futuristic" with drei's `<Stars>` + `<EffectComposer><Bloom /></EffectComposer>`.
- **Glassmorphism CSS utility class** (30 min): `backdrop-filter: blur(16px); background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1)` applied to all cards.
- **Animated counters with GSAP** (1 hour): Numbers that count up from 0 on load—cheap but impressive.
- **Leva debug panel** (30 min): Add runtime sliders for bloom intensity, particle count, glow color. Accelerates all subsequent visual tuning.
- **3D Force-Directed Graph:** Build a standalone prototype with `three-forcegraph` using 5 nodes before integrating into the dashboard. Validate that node count (56+ items), label rendering, and click interaction perform acceptably.
- **Bloom + Glassmorphism Interaction:** Test that CSS `backdrop-filter` glass panels render correctly *over* a bloomed Three.js Canvas. Some browsers composite these differently.
- **Camera Transition System:** Prototype the fly-in and section-to-section camera animation before building all sections. The camera controller pattern will inform how sections are structured.

### Recommended Tools & Technologies

- | Tool | Version | Role | Why | |------|---------|------|-----| | **React** | 18.3.x | UI framework | Component model handles 7 sections + detail panels; hooks simplify state; massive ecosystem | | **TypeScript** | 5.5.x | Language | Type safety for complex 3D scene props, API response types, mock data schemas | | **Vite** | 6.x | Bundler/dev server | Spec-mandated; instant HMR critical for visual iteration; native TS support | | **React Router** | 7.x | Routing (optional) | Only if deep-linking to sections is desired; otherwise use scroll/camera navigation | | Library | Version | Role | Why | |---------|---------|------|-----| | **Three.js** | r170+ | WebGL engine | Spec-mandated; mature, 95k+ GitHub stars | | **@react-three/fiber** | 9.x | React ↔ Three.js bridge | Declarative 3D components; hooks for animation loops; eliminates manual scene management | | **@react-three/drei** | 10.x | Three.js helpers | Pre-built `OrbitControls`, `Text`, `Float`, `MeshTransmissionMaterial` (glassmorphism), `Stars` (particle bg) | | **@react-three/postprocessing** | 3.x | Post-processing | `Bloom`, `Vignette`, `ChromaticAberration` for the glow/neon aesthetic | | **three-forcegraph** | 1.77+ | 3D force-directed graph | Purpose-built for the 3D hierarchy view (epics → features → stories as connected nodes) | | **three-spritetext** | 1.9+ | 3D text labels | Performant text labels on 3D nodes without geometry overhead | **Alternative considered:** Using Three.js imperatively without R3F. Rejected because the spec requires 7 interactive sections with state—managing this imperatively leads to spaghetti code and slower iteration. | Library | Version | Role | Why | |---------|---------|------|-----| | **Chart.js** | 4.4.x | 2D charts (burndown, velocity) | Spec-listed; lightweight (60KB gzipped); good animation support | | **react-chartjs-2** | 5.2.x | React wrapper for Chart.js | Clean declarative API | | **D3.js** | 7.9.x | Custom SVG (radar, timeline) | Needed for the Risk Radar orbit visualization and custom timeline; spec-listed | **Recommendation:** Use Chart.js for standard charts (burndown, bar charts) and D3 only for the radar/orbit visualization and custom timeline. Avoid using D3 for everything—its learning curve is steep and Chart.js covers 70% of needs with less code. | Library | Version | Role | Why | |---------|---------|------|-----| | **GSAP** | 3.12.x | DOM/CSS animations | Animated counters, panel transitions, progress rings; `ScrollTrigger` for section reveals | | **Framer Motion** | 11.x | React animations | Alternative to GSAP; better React integration but heavier. **Pick one—recommend GSAP for its timeline control** | | **@react-spring/three** | 9.7.x | 3D spring animations | Smooth camera fly-in, node hover effects within R3F scenes | | Tool | Version | Role | Why | |------|---------|------|-----| | **Node.js** | 22 LTS | Runtime | Spec-mandated; LTS for stability | | **Express** | 4.21.x | HTTP server | Spec-mandated; mature, minimal | | **cors** | 2.8.x | CORS middleware | Required for Vite dev server (port 5173) → Express (port 3001) | | **nodemon** | 3.1.x | Dev auto-reload | DX improvement for backend changes | | **concurrently** | 9.x | Script runner | Single `npm run dev` starts both Vite + Express | **Note on Express 5:** Express 5.x is now GA but ecosystem middleware compatibility is still catching up. Stick with Express 4.21.x for stability; the migration path is trivial when ready. | Tool | Version | Role | |------|---------|------| | **Vitest** | 2.x | Unit/integration tests (Vite-native, Jest-compatible) | | **React Testing Library** | 16.x | Component testing | | **Playwright** | 1.48.x | E2E visual testing (verify 3D renders, animations) | | **MSW (Mock Service Worker)** | 2.x | API mocking in tests | | Tool | Version | Role | |------|---------|------| | **ESLint** | 9.x | Linting (flat config) | | **Prettier** | 3.4.x | Formatting | | **TypeScript ESLint** | 8.x | TS-specific linting rules | | **Leva** | 0.10.x | Runtime GUI controls for tweaking 3D params (bloom intensity, particle count, colors)—invaluable for visual polish | ---
```
┌─────────────────────────────────────────────┐
│  Browser (Vite dev server :5173)            │
│  ┌───────────────────────────────────────┐  │
│  │  React App                            │  │
│  │  ├── AppShell (layout, nav, theme)    │  │
│  │  ├── R3F Canvas (3D scene)            │  │
│  │  │   ├── HierarchyView (force graph)  │  │
│  │  │   ├── RiskRadar (orbit viz)        │  │
│  │  │   ├── Timeline3D                   │  │
│  │  │   ├── PostProcessing (bloom, etc)  │  │
│  │  │   └── ParticleBackground           │  │
│  │  ├── 2D Overlay Panels               │  │
│  │  │   ├── ProjectOverview              │  │
│  │  │   ├── SprintMetrics (Chart.js)     │  │
│  │  │   ├── TeamActivity (feed)          │  │
│  │  │   └── DetailPanel (slide-out)      │  │
│  │  └── API Layer (fetch + SWR)          │  │
│  └───────────────────────────────────────┘  │
│                    ↕ REST                    │
│  ┌───────────────────────────────────────┐  │
│  │  Express Server (:3001)               │  │
│  │  ├── /api/project-summary             │  │
│  │  ├── /api/project-items               │  │
│  │  ├── /api/sprint-metrics              │  │
│  │  ├── /api/risks                       │  │
│  │  ├── /api/team-activity               │  │
│  │  ├── /api/roadmap                     │  │
│  │  └── /api/report/:id                  │  │
│  │  └── data/mockData.js (in-memory)     │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```
- **Hybrid 3D + 2D Overlay Pattern:** The 3D scene (R3F Canvas) occupies the full viewport as background. 2D UI panels (project overview, sprint metrics, detail panel) are positioned as CSS overlays with `pointer-events: none` (except interactive elements). This avoids rendering text as 3D geometry (expensive, blurry) while maintaining the futuristic aesthetic via glassmorphism CSS.
- **Data Fetching with SWR:** Use `swr` (4.x) for data fetching—provides caching, revalidation, and loading/error states out of the box. Since this is mock data, caching is trivial, but the pattern is correct for future real-API integration.
- **State Management:** React Context + `useReducer` for global state (selected node, active section, detail panel open/closed). No Redux needed—the app is read-heavy with minimal write operations.
- **3D Scene Organization:** One R3F `<Canvas>` with scene sections toggled via state. Camera transitions between sections using `@react-spring/three` for smooth fly-in/fly-out. Each 3D section (Hierarchy, Radar, Timeline) is a separate React component mounted inside the Canvas.
- **Mock Data Architecture:** Single `mockData.ts` file exporting typed objects. Express routes simply import and return slices. This makes it trivial to "swap in real APIs later" (spec deliverable #5) by changing the import to a database call.
```
/
├── server/
│   ├── index.ts                 # Express app entry
│   ├── routes/
│   │   ├── projectSummary.ts
│   │   ├── projectItems.ts
│   │   ├── sprintMetrics.ts
│   │   ├── risks.ts
│   │   ├── teamActivity.ts
│   │   ├── roadmap.ts
│   │   └── report.ts
│   └── data/
│       └── mockData.ts          # All mock data, typed
├── client/
│   ├── index.html
│   ├── vite.config.ts
│   ├── tsconfig.json
│   ├── src/
│   │   ├── main.tsx             # React entry
│   │   ├── App.tsx              # Layout shell
│   │   ├── api/
│   │   │   └── client.ts        # SWR hooks for each endpoint
│   │   ├── types/
│   │   │   └── index.ts         # Shared TypeScript interfaces
│   │   ├── scene/
│   │   │   ├── MainCanvas.tsx   # R3F Canvas + postprocessing
│   │   │   ├── HierarchyGraph.tsx
│   │   │   ├── RiskRadar.tsx
│   │   │   ├── Timeline3D.tsx
│   │   │   ├── ParticleField.tsx
│   │   │   └── CameraController.tsx
│   │   ├── components/
│   │   │   ├── ProjectOverview.tsx
│   │   │   ├── SprintMetrics.tsx
│   │   │   ├── TeamActivity.tsx
│   │   │   ├── DetailPanel.tsx
│   │   │   └── ui/              # Reusable: GlassCard, AnimatedCounter, ProgressRing
│   │   ├── hooks/
│   │   │   └── useAnimatedValue.ts
│   │   └── styles/
│   │       ├── global.css
│   │       └── theme.ts         # Color tokens, glow values
│   └── public/
│       └── fonts/
├── package.json                 # Root: concurrently runs both
├── README.md
└── tsconfig.json
```
- App mounts → SWR hooks fire `GET` requests to all 7 endpoints
- Express returns mock JSON from `mockData.ts`
- React components render 2D overlays with Chart.js/GSAP
- R3F Canvas renders 3D hierarchy, radar, timeline with Three.js
- User clicks 3D node → `onClick` handler sets selected ID in React Context → DetailPanel slides in with `GET /api/report/:id` data
- Camera animates to focused section via `@react-spring/three` ---

### Considerations & Risks

- The spec explicitly states **no authentication** and **no external services**. However, for production-readiness later:
- **CORS:** Lock down to `localhost:5173` in dev. Structure middleware so origin whitelist is configurable via environment variable for future deployment.
- **Input Validation:** Even with mock data, validate the `:id` param in `GET /api/report/:id` to prevent path traversal if data source changes later.
- **CSP Headers:** Add `helmet` (8.x) middleware with permissive WebGL policy (`script-src 'unsafe-eval'` needed for Three.js shader compilation). Worth adding now—costs nothing and establishes the pattern.
- **No secrets:** Zero API keys, tokens, or credentials in the codebase. For demo purposes (the spec's primary use case): | Option | Cost | Complexity | Best For | |--------|------|-----------|----------| | **Local only** (`npm run dev`) | $0 | None | Spec default—this is sufficient | | **Azure Static Web Apps + Azure Functions** | ~$0 (free tier) | Low | If deployed for remote executive access | | **Vercel** (frontend) + **Railway** (Express) | ~$0 free tier | Low | Quick public demo URL | | **Single Docker container** | Varies | Medium | Portable demo | **Recommendation:** Build for local-only per spec. Add a `Dockerfile` as a bonus for portability:
```dockerfile
FROM node:22-alpine
WORKDIR /app
COPY . .
RUN npm ci && cd client && npm run build
EXPOSE 3001
CMD ["node", "server/index.js"]
``` Not applicable—the spec mandates local-only, mock-data operation. If deployed:
- **Small scale** (< 100 users, demo): $0 on free tiers
- **Medium scale** (CDN-served SPA + API): ~$20/month on Azure App Service B1 --- | Risk | Impact | Likelihood | Mitigation | |------|--------|-----------|------------| | **3D Hierarchy View complexity** | Could consume 60%+ of dev time | High | Use `three-forcegraph` library rather than building force-directed layout from scratch. Prototype this section first. | | **WebGL performance on low-end devices** | Bloom + particles + 50 animated nodes = frame drops | Medium | Implement quality settings (low/medium/high). Use `InstancedMesh` for repeated geometries. Cap particle count. Use `drei`'s `<PerformanceMonitor>` to auto-degrade. | | **Visual polish scope creep** | "Premium futuristic demo" is subjective; could iterate endlessly | High | Define a visual reference (Figma mockup or screenshot from similar dashboards) before coding. Set a time-box for polish. | | Risk | Impact | Mitigation | |------|--------|------------| | **R3F + drei version churn** | Breaking changes between minor versions | Pin exact versions in `package.json`. Test upgrades explicitly. | | **CSS glassmorphism browser support** | `backdrop-filter` not supported in some browsers | All modern browsers support it. Add fallback `background: rgba(0,0,0,0.8)` for safety. | | **Three.js shader compilation stalls** | First render may be slow due to shader compilation | Use `drei`'s `<Preload>` component. Show a loading screen during initial compilation. |
- **React Three Fiber vs. imperative Three.js:** R3F adds ~30KB bundle but saves hundreds of lines of boilerplate. Worth it at this complexity level.
- **Chart.js vs. all-D3:** Chart.js is faster to implement for standard charts but less customizable. Use D3 only where Chart.js can't deliver (radar orbit, 3D-styled timeline).
- **GSAP vs. Framer Motion:** GSAP has better timeline control for sequenced animations (fly-in → counters → rings). Framer Motion integrates more naturally with React. **Recommend GSAP** for this project because the spec demands choreographed animation sequences.
- **TypeScript overhead:** Adds ~10% dev time for type definitions but prevents runtime errors in complex 3D prop passing. Non-negotiable for a project with this many data shapes. ---
- **Target browsers/devices?** The spec says "responsive desktop browser layout" but doesn't specify minimum GPU capability. Should we support integrated Intel graphics? This affects particle count and post-processing budget.
- **Deployment target?** Spec says local-only, but should we also prepare a one-click deploy (e.g., Azure Static Web Apps) for remote executive demos?
- **Real data integration timeline?** The spec mentions "notes on how to replace mock data with real APIs." Is this a future phase, and if so, what's the data source (Azure DevOps, Jira, GitHub Projects)?
- **Accessibility requirements?** The 3D-heavy aesthetic conflicts with screen readers and keyboard navigation. Is WCAG compliance expected, or is this purely a visual demo tool?
- **Performance budget?** What's the minimum acceptable FPS? 60fps on a MacBook Pro M1? 30fps on a Surface Pro?
- **Branding/color scheme?** The spec says "neon accents" and "dark mode" but doesn't specify exact brand colors. Should we match an existing design system?
- **Multi-project support?** Spec says "1 project" in mock data. Should the architecture support switching between multiple projects in the future? ---
- Integrate `three-forcegraph` for epic → feature → story node graph
- Color-code nodes by status
- Implement click-to-focus camera animation
- Connect node clicks to Detail Panel
- **Deliverable:** Interactive 3D project hierarchy with drill-down
- D3-based radar/orbit visualization for risks
- 3D horizontal timeline with milestones and sprint boundaries
- Animated transitions between dashboard sections
- Initial camera fly-in sequence
- Section transition animations
- Hover glow effects on all interactive elements
- Performance optimization (InstancedMesh, LOD, quality settings)
- README with install/run instructions and design decision notes

### Detailed Analysis

# Research: Technology Stack for ReportingDashboard

## Executive Summary

The ReportingDashboard is a full-stack web application that serves as a futuristic 3D "project command center" for visualizing project management data during executive demos and engineering leadership reviews. The spec mandates **Node.js + Express** backend, **Three.js** for WebGL 3D rendering, optional **React + TypeScript** frontend with **Vite** bundler, and **mock JSON data only** (no database, no auth, no external services). This research evaluates the best libraries, patterns, and tooling within this mandated stack to deliver a premium, animated, interactive dashboard with glassmorphism aesthetics, 3D node hierarchies, animated charts, and smooth camera controls—all runnable locally with `npm install && npm run dev`.

**Primary Recommendation:** Use **React 18 + TypeScript + Vite** for the frontend (structured component model pays off at this complexity), **React Three Fiber** as the React-idiomatic Three.js wrapper, **Express 4** for the REST API, and **GSAP** for non-3D animations. This combination maximizes developer productivity, visual polish, and maintainability while staying fully within the mandated stack.

---

## Key Findings

- The spec requires 7 distinct dashboard sections (Project Overview, 3D Hierarchy, Sprint Metrics, Risk Radar, Team Activity, Timeline, Detail Panel) plus 7 REST endpoints—this is moderate complexity favoring a component-based frontend over vanilla JS.
- **Three.js r170+** is the mandated 3D engine; wrapping it with **React Three Fiber (R3F) v9** eliminates imperative boilerplate and integrates cleanly with React's component lifecycle.
- The 3D Project Hierarchy View (epics → features → stories as floating connected nodes) is the highest-risk, highest-effort section—it requires force-directed graph layout in 3D space.
- Mock data volume (4 epics, 12 features, 40+ stories, 8 risks, 10 team members) is small enough that no database or pagination is needed—in-memory JSON files served by Express suffice.
- The "dark futuristic SaaS" aesthetic (glassmorphism, bloom, glow, particles) maps directly to Three.js post-processing effects (`UnrealBloomPass`, `EffectComposer`) and CSS `backdrop-filter`.
- No authentication or external services are required—this dramatically simplifies the backend to a thin mock-data API layer.
- The biggest technical risk is **3D performance on low-end hardware**—bloom, particles, and 50+ animated nodes can drop below 60fps without careful optimization.
- Vite's HMR and React Three Fiber's declarative model will accelerate iteration on visual polish—the spec's quality bar ("premium futuristic product demo") demands rapid visual experimentation.

---

## Recommended Technology Stack

### Frontend Framework & Build

| Tool | Version | Role | Why |
|------|---------|------|-----|
| **React** | 18.3.x | UI framework | Component model handles 7 sections + detail panels; hooks simplify state; massive ecosystem |
| **TypeScript** | 5.5.x | Language | Type safety for complex 3D scene props, API response types, mock data schemas |
| **Vite** | 6.x | Bundler/dev server | Spec-mandated; instant HMR critical for visual iteration; native TS support |
| **React Router** | 7.x | Routing (optional) | Only if deep-linking to sections is desired; otherwise use scroll/camera navigation |

### 3D Rendering & Visualization

| Library | Version | Role | Why |
|---------|---------|------|-----|
| **Three.js** | r170+ | WebGL engine | Spec-mandated; mature, 95k+ GitHub stars |
| **@react-three/fiber** | 9.x | React ↔ Three.js bridge | Declarative 3D components; hooks for animation loops; eliminates manual scene management |
| **@react-three/drei** | 10.x | Three.js helpers | Pre-built `OrbitControls`, `Text`, `Float`, `MeshTransmissionMaterial` (glassmorphism), `Stars` (particle bg) |
| **@react-three/postprocessing** | 3.x | Post-processing | `Bloom`, `Vignette`, `ChromaticAberration` for the glow/neon aesthetic |
| **three-forcegraph** | 1.77+ | 3D force-directed graph | Purpose-built for the 3D hierarchy view (epics → features → stories as connected nodes) |
| **three-spritetext** | 1.9+ | 3D text labels | Performant text labels on 3D nodes without geometry overhead |

**Alternative considered:** Using Three.js imperatively without R3F. Rejected because the spec requires 7 interactive sections with state—managing this imperatively leads to spaghetti code and slower iteration.

### 2D Charts

| Library | Version | Role | Why |
|---------|---------|------|-----|
| **Chart.js** | 4.4.x | 2D charts (burndown, velocity) | Spec-listed; lightweight (60KB gzipped); good animation support |
| **react-chartjs-2** | 5.2.x | React wrapper for Chart.js | Clean declarative API |
| **D3.js** | 7.9.x | Custom SVG (radar, timeline) | Needed for the Risk Radar orbit visualization and custom timeline; spec-listed |

**Recommendation:** Use Chart.js for standard charts (burndown, bar charts) and D3 only for the radar/orbit visualization and custom timeline. Avoid using D3 for everything—its learning curve is steep and Chart.js covers 70% of needs with less code.

### Animation

| Library | Version | Role | Why |
|---------|---------|------|-----|
| **GSAP** | 3.12.x | DOM/CSS animations | Animated counters, panel transitions, progress rings; `ScrollTrigger` for section reveals |
| **Framer Motion** | 11.x | React animations | Alternative to GSAP; better React integration but heavier. **Pick one—recommend GSAP for its timeline control** |
| **@react-spring/three** | 9.7.x | 3D spring animations | Smooth camera fly-in, node hover effects within R3F scenes |

### Backend

| Tool | Version | Role | Why |
|------|---------|------|-----|
| **Node.js** | 22 LTS | Runtime | Spec-mandated; LTS for stability |
| **Express** | 4.21.x | HTTP server | Spec-mandated; mature, minimal |
| **cors** | 2.8.x | CORS middleware | Required for Vite dev server (port 5173) → Express (port 3001) |
| **nodemon** | 3.1.x | Dev auto-reload | DX improvement for backend changes |
| **concurrently** | 9.x | Script runner | Single `npm run dev` starts both Vite + Express |

**Note on Express 5:** Express 5.x is now GA but ecosystem middleware compatibility is still catching up. Stick with Express 4.21.x for stability; the migration path is trivial when ready.

### Testing

| Tool | Version | Role |
|------|---------|------|
| **Vitest** | 2.x | Unit/integration tests (Vite-native, Jest-compatible) |
| **React Testing Library** | 16.x | Component testing |
| **Playwright** | 1.48.x | E2E visual testing (verify 3D renders, animations) |
| **MSW (Mock Service Worker)** | 2.x | API mocking in tests |

### Dev Tooling

| Tool | Version | Role |
|------|---------|------|
| **ESLint** | 9.x | Linting (flat config) |
| **Prettier** | 3.4.x | Formatting |
| **TypeScript ESLint** | 8.x | TS-specific linting rules |
| **Leva** | 0.10.x | Runtime GUI controls for tweaking 3D params (bloom intensity, particle count, colors)—invaluable for visual polish |

---

## Architecture Recommendations

### Overall Architecture

```
┌─────────────────────────────────────────────┐
│  Browser (Vite dev server :5173)            │
│  ┌───────────────────────────────────────┐  │
│  │  React App                            │  │
│  │  ├── AppShell (layout, nav, theme)    │  │
│  │  ├── R3F Canvas (3D scene)            │  │
│  │  │   ├── HierarchyView (force graph)  │  │
│  │  │   ├── RiskRadar (orbit viz)        │  │
│  │  │   ├── Timeline3D                   │  │
│  │  │   ├── PostProcessing (bloom, etc)  │  │
│  │  │   └── ParticleBackground           │  │
│  │  ├── 2D Overlay Panels               │  │
│  │  │   ├── ProjectOverview              │  │
│  │  │   ├── SprintMetrics (Chart.js)     │  │
│  │  │   ├── TeamActivity (feed)          │  │
│  │  │   └── DetailPanel (slide-out)      │  │
│  │  └── API Layer (fetch + SWR)          │  │
│  └───────────────────────────────────────┘  │
│                    ↕ REST                    │
│  ┌───────────────────────────────────────┐  │
│  │  Express Server (:3001)               │  │
│  │  ├── /api/project-summary             │  │
│  │  ├── /api/project-items               │  │
│  │  ├── /api/sprint-metrics              │  │
│  │  ├── /api/risks                       │  │
│  │  ├── /api/team-activity               │  │
│  │  ├── /api/roadmap                     │  │
│  │  └── /api/report/:id                  │  │
│  │  └── data/mockData.js (in-memory)     │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```

### Key Architectural Decisions

1. **Hybrid 3D + 2D Overlay Pattern:** The 3D scene (R3F Canvas) occupies the full viewport as background. 2D UI panels (project overview, sprint metrics, detail panel) are positioned as CSS overlays with `pointer-events: none` (except interactive elements). This avoids rendering text as 3D geometry (expensive, blurry) while maintaining the futuristic aesthetic via glassmorphism CSS.

2. **Data Fetching with SWR:** Use `swr` (4.x) for data fetching—provides caching, revalidation, and loading/error states out of the box. Since this is mock data, caching is trivial, but the pattern is correct for future real-API integration.

3. **State Management:** React Context + `useReducer` for global state (selected node, active section, detail panel open/closed). No Redux needed—the app is read-heavy with minimal write operations.

4. **3D Scene Organization:** One R3F `<Canvas>` with scene sections toggled via state. Camera transitions between sections using `@react-spring/three` for smooth fly-in/fly-out. Each 3D section (Hierarchy, Radar, Timeline) is a separate React component mounted inside the Canvas.

5. **Mock Data Architecture:** Single `mockData.ts` file exporting typed objects. Express routes simply import and return slices. This makes it trivial to "swap in real APIs later" (spec deliverable #5) by changing the import to a database call.

### Recommended Folder Structure

```
/
├── server/
│   ├── index.ts                 # Express app entry
│   ├── routes/
│   │   ├── projectSummary.ts
│   │   ├── projectItems.ts
│   │   ├── sprintMetrics.ts
│   │   ├── risks.ts
│   │   ├── teamActivity.ts
│   │   ├── roadmap.ts
│   │   └── report.ts
│   └── data/
│       └── mockData.ts          # All mock data, typed
├── client/
│   ├── index.html
│   ├── vite.config.ts
│   ├── tsconfig.json
│   ├── src/
│   │   ├── main.tsx             # React entry
│   │   ├── App.tsx              # Layout shell
│   │   ├── api/
│   │   │   └── client.ts        # SWR hooks for each endpoint
│   │   ├── types/
│   │   │   └── index.ts         # Shared TypeScript interfaces
│   │   ├── scene/
│   │   │   ├── MainCanvas.tsx   # R3F Canvas + postprocessing
│   │   │   ├── HierarchyGraph.tsx
│   │   │   ├── RiskRadar.tsx
│   │   │   ├── Timeline3D.tsx
│   │   │   ├── ParticleField.tsx
│   │   │   └── CameraController.tsx
│   │   ├── components/
│   │   │   ├── ProjectOverview.tsx
│   │   │   ├── SprintMetrics.tsx
│   │   │   ├── TeamActivity.tsx
│   │   │   ├── DetailPanel.tsx
│   │   │   └── ui/              # Reusable: GlassCard, AnimatedCounter, ProgressRing
│   │   ├── hooks/
│   │   │   └── useAnimatedValue.ts
│   │   └── styles/
│   │       ├── global.css
│   │       └── theme.ts         # Color tokens, glow values
│   └── public/
│       └── fonts/
├── package.json                 # Root: concurrently runs both
├── README.md
└── tsconfig.json
```

### Data Flow

1. App mounts → SWR hooks fire `GET` requests to all 7 endpoints
2. Express returns mock JSON from `mockData.ts`
3. React components render 2D overlays with Chart.js/GSAP
4. R3F Canvas renders 3D hierarchy, radar, timeline with Three.js
5. User clicks 3D node → `onClick` handler sets selected ID in React Context → DetailPanel slides in with `GET /api/report/:id` data
6. Camera animates to focused section via `@react-spring/three`

---

## Security & Infrastructure

### Security (Minimal — Spec Says No Auth)

The spec explicitly states **no authentication** and **no external services**. However, for production-readiness later:

- **CORS:** Lock down to `localhost:5173` in dev. Structure middleware so origin whitelist is configurable via environment variable for future deployment.
- **Input Validation:** Even with mock data, validate the `:id` param in `GET /api/report/:id` to prevent path traversal if data source changes later.
- **CSP Headers:** Add `helmet` (8.x) middleware with permissive WebGL policy (`script-src 'unsafe-eval'` needed for Three.js shader compilation). Worth adding now—costs nothing and establishes the pattern.
- **No secrets:** Zero API keys, tokens, or credentials in the codebase.

### Hosting & Deployment

For demo purposes (the spec's primary use case):

| Option | Cost | Complexity | Best For |
|--------|------|-----------|----------|
| **Local only** (`npm run dev`) | $0 | None | Spec default—this is sufficient |
| **Azure Static Web Apps + Azure Functions** | ~$0 (free tier) | Low | If deployed for remote executive access |
| **Vercel** (frontend) + **Railway** (Express) | ~$0 free tier | Low | Quick public demo URL |
| **Single Docker container** | Varies | Medium | Portable demo |

**Recommendation:** Build for local-only per spec. Add a `Dockerfile` as a bonus for portability:
```dockerfile
FROM node:22-alpine
WORKDIR /app
COPY . .
RUN npm ci && cd client && npm run build
EXPOSE 3001
CMD ["node", "server/index.js"]
```

### Infrastructure Costs

Not applicable—the spec mandates local-only, mock-data operation. If deployed:
- **Small scale** (< 100 users, demo): $0 on free tiers
- **Medium scale** (CDN-served SPA + API): ~$20/month on Azure App Service B1

---

## Risks & Trade-offs

### High Risk

| Risk | Impact | Likelihood | Mitigation |
|------|--------|-----------|------------|
| **3D Hierarchy View complexity** | Could consume 60%+ of dev time | High | Use `three-forcegraph` library rather than building force-directed layout from scratch. Prototype this section first. |
| **WebGL performance on low-end devices** | Bloom + particles + 50 animated nodes = frame drops | Medium | Implement quality settings (low/medium/high). Use `InstancedMesh` for repeated geometries. Cap particle count. Use `drei`'s `<PerformanceMonitor>` to auto-degrade. |
| **Visual polish scope creep** | "Premium futuristic demo" is subjective; could iterate endlessly | High | Define a visual reference (Figma mockup or screenshot from similar dashboards) before coding. Set a time-box for polish. |

### Medium Risk

| Risk | Impact | Mitigation |
|------|--------|------------|
| **R3F + drei version churn** | Breaking changes between minor versions | Pin exact versions in `package.json`. Test upgrades explicitly. |
| **CSS glassmorphism browser support** | `backdrop-filter` not supported in some browsers | All modern browsers support it. Add fallback `background: rgba(0,0,0,0.8)` for safety. |
| **Three.js shader compilation stalls** | First render may be slow due to shader compilation | Use `drei`'s `<Preload>` component. Show a loading screen during initial compilation. |

### Trade-offs

1. **React Three Fiber vs. imperative Three.js:** R3F adds ~30KB bundle but saves hundreds of lines of boilerplate. Worth it at this complexity level.
2. **Chart.js vs. all-D3:** Chart.js is faster to implement for standard charts but less customizable. Use D3 only where Chart.js can't deliver (radar orbit, 3D-styled timeline).
3. **GSAP vs. Framer Motion:** GSAP has better timeline control for sequenced animations (fly-in → counters → rings). Framer Motion integrates more naturally with React. **Recommend GSAP** for this project because the spec demands choreographed animation sequences.
4. **TypeScript overhead:** Adds ~10% dev time for type definitions but prevents runtime errors in complex 3D prop passing. Non-negotiable for a project with this many data shapes.

---

## Open Questions

1. **Target browsers/devices?** The spec says "responsive desktop browser layout" but doesn't specify minimum GPU capability. Should we support integrated Intel graphics? This affects particle count and post-processing budget.

2. **Deployment target?** Spec says local-only, but should we also prepare a one-click deploy (e.g., Azure Static Web Apps) for remote executive demos?

3. **Real data integration timeline?** The spec mentions "notes on how to replace mock data with real APIs." Is this a future phase, and if so, what's the data source (Azure DevOps, Jira, GitHub Projects)?

4. **Accessibility requirements?** The 3D-heavy aesthetic conflicts with screen readers and keyboard navigation. Is WCAG compliance expected, or is this purely a visual demo tool?

5. **Performance budget?** What's the minimum acceptable FPS? 60fps on a MacBook Pro M1? 30fps on a Surface Pro?

6. **Branding/color scheme?** The spec says "neon accents" and "dark mode" but doesn't specify exact brand colors. Should we match an existing design system?

7. **Multi-project support?** Spec says "1 project" in mock data. Should the architecture support switching between multiple projects in the future?

---

## Implementation Recommendations

### Phasing

#### Phase 1: Skeleton + 3D Proof of Concept (3–4 days)
- Set up monorepo (Vite + Express + concurrently)
- Build Express server with all 7 endpoints returning mock JSON
- Create R3F Canvas with particle background, bloom post-processing, and orbit controls
- Build one glassmorphism `<GlassCard>` component
- **Deliverable:** Spinning 3D scene with glowing particles + one data card fetching from API
- **Why first:** Validates the entire stack end-to-end and proves the visual aesthetic early

#### Phase 2: 2D Dashboard Panels (2–3 days)
- Project Overview panel (animated counters, health ring)
- Sprint Metrics (Chart.js burndown, velocity bars)
- Team Activity feed (animated list)
- Detail Panel (slide-out on click)
- **Deliverable:** Functional 2D dashboard overlaying the 3D background

#### Phase 3: 3D Hierarchy View (3–4 days) ⚠️ Highest Risk
- Integrate `three-forcegraph` for epic → feature → story node graph
- Color-code nodes by status
- Implement click-to-focus camera animation
- Connect node clicks to Detail Panel
- **Deliverable:** Interactive 3D project hierarchy with drill-down

#### Phase 4: Risk Radar + Timeline (2–3 days)
- D3-based radar/orbit visualization for risks
- 3D horizontal timeline with milestones and sprint boundaries
- Animated transitions between dashboard sections

#### Phase 5: Polish + Camera Choreography (2–3 days)
- Initial camera fly-in sequence
- Section transition animations
- Hover glow effects on all interactive elements
- Performance optimization (InstancedMesh, LOD, quality settings)
- README with install/run instructions and design decision notes

### Quick Wins

1. **Particle background + bloom** (2 hours): Immediately makes the app look "futuristic" with drei's `<Stars>` + `<EffectComposer><Bloom /></EffectComposer>`.
2. **Glassmorphism CSS utility class** (30 min): `backdrop-filter: blur(16px); background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1)` applied to all cards.
3. **Animated counters with GSAP** (1 hour): Numbers that count up from 0 on load—cheap but impressive.
4. **Leva debug panel** (30 min): Add runtime sliders for bloom intensity, particle count, glow color. Accelerates all subsequent visual tuning.

### Prototype Before Committing

- **3D Force-Directed Graph:** Build a standalone prototype with `three-forcegraph` using 5 nodes before integrating into the dashboard. Validate that node count (56+ items), label rendering, and click interaction perform acceptably.
- **Bloom + Glassmorphism Interaction:** Test that CSS `backdrop-filter` glass panels render correctly *over* a bloomed Three.js Canvas. Some browsers composite these differently.
- **Camera Transition System:** Prototype the fly-in and section-to-section camera animation before building all sections. The camera controller pattern will inform how sections are structured.
