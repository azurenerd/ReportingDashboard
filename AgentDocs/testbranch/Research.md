# Research

_No research has been documented yet._

## Research technology stack for ReportingDashboard

_Researched on 2026-05-01 19:57 UTC_

### Summary

The ReportingDashboard is a polished, 3D animated full-stack web application serving as a "project command center" that visualizes project management data with a futuristic dark-mode aesthetic. The spec mandates **Three.js** for WebGL 3D rendering, **Node.js with Express** for the backend, and **Vanilla JS or React** for the frontend, with **Vite** as the build tool. All data is mock JSON—no database required. The primary recommendation is to use **Vanilla TypeScript + Vite** for the frontend to minimize complexity, **Three.js r170** for all 3D/WebGL scenes, **Chart.js 4.x** for 2D chart overlays, and **Express 4.x** serving mock JSON files. This stack keeps dependencies minimal, startup fast, and the codebase easy for any web developer to extend. ---

### Key Findings

- The feature spec explicitly requires **Three.js** for WebGL-powered 3D scenes including floating panels, animated node graphs, particle backgrounds, and camera fly-ins.
- **No database** is needed—all data is mock JSON served from Express API endpoints. This dramatically simplifies the backend.
- The spec defines **7 exact REST API endpoints** that the Express backend must serve (`/api/project-summary`, `/api/project-items`, `/api/sprint-metrics`, `/api/risks`, `/api/team-activity`, `/api/roadmap`, `/api/report/:id`).
- The mock dataset is substantial: 1 project, 4 epics, 12 features, 40+ stories/tasks/bugs, 8 risks, 10 team members, 20 activity events, 6 roadmap milestones.
- Visual requirements are demanding: glassmorphism, bloom/glow post-processing, neon accents, animated counters, progress rings, hover effects, and smooth camera transitions—all requiring deep Three.js expertise.
- **Vanilla JS/TS is preferred** over React per the spec ("preferred if simpler to run"), reducing build complexity.
- The project workspace is configured with `dotnet run` as the app start command but the actual app is a Node.js/Express project—this will need workspace config adjustment.
- No authentication, no external services, and no production deployment are required—this is a self-contained demo application. ---
- Set up Vite + TypeScript + Express project structure per spec's folder layout
- Implement all 7 Express API endpoints with complete mock dataset
- Create Three.js scene with camera, renderer, post-processing pipeline (bloom)
- Build particle background and basic dark-mode CSS
- **Deliverable**: Running app with 3D canvas and API data loading
- Project Overview HUD overlay with animated counters and health ring
- 3D Project Hierarchy node graph (epics → features → stories) with color-coded statuses
- Sprint Metrics panel with Chart.js burndown and velocity charts
- Raycaster-based click interaction → Detail Panel slide-out
- **Deliverable**: Interactive dashboard with 3 of 7 sections functional
- Risk & Blocker Radar (animated orbit visualization)
- Team Activity feed with animated pulses
- 3D Timeline/Roadmap with milestones
- Camera fly-in animation on load
- Hover glow effects, floating motion, transition polish
- **Deliverable**: Feature-complete dashboard matching spec
- Playwright visual regression tests (screenshot comparison)
- Vitest unit tests for API layer and data transformation
- Supertest for Express endpoint validation
- README with install, run, customization, and real-API migration notes
- **Deliverable**: Production-ready codebase with documentation
- **Particle background + bloom**: Visually impressive with ~50 lines of Three.js code. Demo-worthy on day 1.
- **Animated counters**: GSAP `countTo` on the overview panel numbers creates immediate "wow" factor.
- **Mock data**: Front-load mock data creation—realistic data makes every subsequent visual more compelling.
- **3D node graph layout**: Prototype the force-directed or hierarchical layout for the project hierarchy view. Layouts with 40+ nodes need performance validation and visual tuning before building the full interaction model.
- **Bloom + glassmorphism interaction**: Test that CSS glassmorphism `backdrop-filter` composites correctly over a bloomed Three.js canvas. Some browser/GPU combinations produce artifacts.

### Recommended Tools & Technologies

- | Layer | Library | Version | Rationale | |-------|---------|---------|-----------| | Language | **TypeScript** | 5.5+ | Type safety for complex 3D scene code; caught errors early in animation logic | | Build Tool | **Vite** | 6.x | Spec-required if using TS; fast HMR, minimal config | | 3D Engine | **Three.js** | r170 (0.170.0) | Spec-mandated; mature WebGL library, excellent for all required 3D effects | | Post-Processing | **three/examples/jsm/postprocessing** | (bundled with Three.js) | Bloom/glow via `UnrealBloomPass`, required for neon aesthetic | | 2D Charts | **Chart.js** | 4.4+ | Burndown, velocity, and sprint metric charts; lightweight, canvas-based | | CSS | **Vanilla CSS with CSS custom properties** | — | Dark theme variables, glassmorphism via `backdrop-filter`, no framework needed | | Animation | **GSAP** | 3.12+ | Smooth counter animations, panel transitions, camera easing; MIT-like license for non-commercial | | Orbit Controls | **three/examples/jsm/controls/OrbitControls** | (bundled) | Optional user camera control as spec allows | | Layer | Library | Version | Rationale | |-------|---------|---------|-----------| | Runtime | **Node.js** | 20 LTS or 22 LTS | Spec-mandated; even-numbered LTS for stability | | Framework | **Express** | 4.21+ | Spec-mandated; serves 7 REST endpoints + static files | | CORS | **cors** | 2.8+ | Enable frontend dev server to call backend API | | Static Serving | Express `express.static()` | (built-in) | Serve production frontend build from Express in production mode | | Concern | Approach | |---------|----------| | Storage | In-memory JSON files in `/server/data/` | | Format | Structured mock JSON with realistic project management data | | Database | **None required** — spec explicitly states mock data only | | Type | Tool | Version | Rationale | |------|------|---------|-----------| | Unit (logic) | **Vitest** | 3.x | Native Vite integration, fast, TypeScript-first | | Component/Visual | **Playwright** | 1.49+ | Already configured in workspace; headless browser for WebGL screenshot testing | | API | **Supertest** | 7.x | Express endpoint validation | | Linting | **ESLint** | 9.x (flat config) | TypeScript plugin for type-aware linting | | Formatting | **Prettier** | 3.x | Consistent code style | | Concern | Tool | |---------|------| | Package Manager | **npm** (workspace-native) | | Dev Server (frontend) | Vite dev server with proxy to Express | | Dev Server (backend) | **nodemon** or **tsx --watch** for auto-restart | | CI | GitHub Actions (repo is on GitHub) | | Bundling | Vite production build → Express serves `/dist` | ---
```
/
├── server/
│   ├── index.js              # Express entry point
│   └── data/
│       └── mockData.js       # All mock JSON datasets
├── client/
│   ├── index.html            # Vite entry
│   ├── package.json
│   └── src/
│       ├── main.ts           # App bootstrap, scene init
│       ├── api.ts            # Fetch wrapper for all 7 endpoints
│       ├── scene/
│       │   ├── SceneManager.ts       # Three.js scene, camera, renderer, post-processing
│       │   ├── ProjectHierarchy.ts   # 3D node graph (epics → features → stories)
│       │   ├── RiskRadar.ts          # Animated radar/orbit visualization
│       │   ├── Timeline.ts           # 3D horizontal roadmap
│       │   ├── ParticleBackground.ts # Ambient particle system
│       │   └── CameraController.ts   # Fly-in, click-to-focus, orbit
│       ├── components/
│       │   ├── OverviewPanel.ts      # Project summary HUD overlay
│       │   ├── SprintMetrics.ts      # Chart.js burndown/velocity
│       │   ├── TeamActivity.ts       # Activity feed with animated pulses
│       │   ├── DetailPanel.ts        # Slide-out panel for clicked items
│       │   └── LoadingScreen.ts      # Loading state with animation
│       └── styles.css
├── README.md
└── package.json              # Root scripts: `npm run dev` starts both
```
- **Startup**: Vite dev server proxies `/api/*` to Express on port 3001
- **Frontend boot**: `main.ts` calls all API endpoints in parallel via `Promise.all`
- **Scene construction**: Data populates Three.js scene objects (nodes, rings, charts)
- **Interaction**: Raycaster detects hover/click on 3D objects → triggers detail panel or camera focus
- **Animation loop**: `requestAnimationFrame` drives continuous floating motion, particle updates, counter animations
- **HTML overlay for 2D elements**: Use CSS-positioned HTML panels overlaid on the Three.js canvas rather than rendering text in WebGL. This keeps text crisp, accessible, and easy to style with glassmorphism CSS.
- **Raycasting for interaction**: Three.js `Raycaster` maps mouse events to 3D objects. Each interactive mesh carries a `userData` property linking to the mock data record.
- **Post-processing pipeline**: `EffectComposer` → `RenderPass` → `UnrealBloomPass` for glow. Apply selectively using layer masking to avoid blooming the entire scene.
- **Module-per-section**: Each dashboard section (hierarchy, radar, timeline) is a self-contained class that owns its Three.js objects and exposes `update(dt)` for the animation loop. ---

### Considerations & Risks

- **Not required**. The spec explicitly states no external services or authentication.
- If future integration with real project APIs is needed, recommend OAuth2/OIDC via Azure AD (MSAL.js 2.x) at that point.
- All data is mock—no PII, no sensitive data, no encryption needed.
- When transitioning to real APIs, enforce HTTPS and token-based API auth.
- **Local-only** per spec. Run with `npm install && npm run dev`.
- **If deployed for demos**: Azure Static Web Apps (free tier) for frontend + Azure App Service (B1, ~$13/mo) for Express backend. Or a single App Service serving both.
- **Containerization** (optional): Single Dockerfile with multi-stage build—Vite builds frontend, then Node serves the bundle + API. Image size ~150MB. | Scale | Setup | Monthly Cost | |-------|-------|-------------| | Local demo | localhost | $0 | | Small (shared demo) | Azure App Service B1 | ~$13/mo | | Medium (team access) | Azure App Service S1 + CDN | ~$75/mo | --- | Risk | Severity | Mitigation | |------|----------|------------| | **Three.js performance with 40+ animated nodes + particles + bloom** | Medium | Use `InstancedMesh` for repeated geometries; limit particle count; use `LOD` for distant nodes; profile with Chrome DevTools GPU panel | | **WebGL compatibility across browsers** | Low | Three.js abstracts well; test on Chrome, Edge, Firefox. Safari WebGL2 support is now solid. | | **GSAP licensing** | Medium | GSAP's "no charge" license covers internal tools and non-commercial use. If commercializing, need Business license ($199). Alternative: use Three.js's built-in `Clock` + custom easing or `anime.js` (MIT). | | **Scope creep from visual polish** | High | The spec's visual requirements are extensive. Timebox each section's animation work. Ship functional-but-plain first, then layer effects. | | **Workspace config mismatch** | Low | The AgentSquad workspace is configured for `dotnet run` but this is a Node.js project. Update `AppStartCommand` to `npm run dev` and adjust build/test commands. | | Factor | Vanilla TS | React | |--------|-----------|-------| | Bundle size | Smaller (~5KB app + Three.js) | +40KB for React runtime | | Three.js integration | Direct, no abstraction layer | Needs `@react-three/fiber` which adds complexity | | DOM updates | Manual but minimal (few HTML panels) | Overkill for 6-7 overlay panels | | Developer familiarity | Lower barrier | Higher if team knows React | **Recommendation**: Vanilla TS. The app is primarily a Three.js scene with thin HTML overlays. React adds indirection with no proportional benefit. ---
- **Target browsers/devices**: Is this desktop-only (spec says "desktop browser") or should tablet/touch be supported? Touch affects orbit controls and hover interactions.
- **Performance floor**: What's the minimum acceptable GPU? Integrated graphics (Intel UHD) or discrete only? This affects particle count and post-processing decisions.
- **Future real-data integration**: Which project management tool (Azure DevOps, Jira, GitHub Projects) will eventually replace mock data? This affects the shape of the API contract.
- **Branding**: Should the dashboard carry specific brand colors/logos, or is the spec's "neon futuristic" aesthetic the final direction?
- **Accessibility**: The spec doesn't mention a11y. Should we provide a non-3D fallback view for screen readers or reduced-motion preferences?
- **GSAP licensing**: Confirm the project's commercial/non-commercial status to determine if the free GSAP license applies or if an alternative (anime.js) should be used. ---

### Detailed Analysis

# Research: Technology Stack for ReportingDashboard

## Executive Summary

The ReportingDashboard is a polished, 3D animated full-stack web application serving as a "project command center" that visualizes project management data with a futuristic dark-mode aesthetic. The spec mandates **Three.js** for WebGL 3D rendering, **Node.js with Express** for the backend, and **Vanilla JS or React** for the frontend, with **Vite** as the build tool. All data is mock JSON—no database required. The primary recommendation is to use **Vanilla TypeScript + Vite** for the frontend to minimize complexity, **Three.js r170** for all 3D/WebGL scenes, **Chart.js 4.x** for 2D chart overlays, and **Express 4.x** serving mock JSON files. This stack keeps dependencies minimal, startup fast, and the codebase easy for any web developer to extend.

---

## Key Findings

- The feature spec explicitly requires **Three.js** for WebGL-powered 3D scenes including floating panels, animated node graphs, particle backgrounds, and camera fly-ins.
- **No database** is needed—all data is mock JSON served from Express API endpoints. This dramatically simplifies the backend.
- The spec defines **7 exact REST API endpoints** that the Express backend must serve (`/api/project-summary`, `/api/project-items`, `/api/sprint-metrics`, `/api/risks`, `/api/team-activity`, `/api/roadmap`, `/api/report/:id`).
- The mock dataset is substantial: 1 project, 4 epics, 12 features, 40+ stories/tasks/bugs, 8 risks, 10 team members, 20 activity events, 6 roadmap milestones.
- Visual requirements are demanding: glassmorphism, bloom/glow post-processing, neon accents, animated counters, progress rings, hover effects, and smooth camera transitions—all requiring deep Three.js expertise.
- **Vanilla JS/TS is preferred** over React per the spec ("preferred if simpler to run"), reducing build complexity.
- The project workspace is configured with `dotnet run` as the app start command but the actual app is a Node.js/Express project—this will need workspace config adjustment.
- No authentication, no external services, and no production deployment are required—this is a self-contained demo application.

---

## Recommended Technology Stack

### Frontend
| Layer | Library | Version | Rationale |
|-------|---------|---------|-----------|
| Language | **TypeScript** | 5.5+ | Type safety for complex 3D scene code; caught errors early in animation logic |
| Build Tool | **Vite** | 6.x | Spec-required if using TS; fast HMR, minimal config |
| 3D Engine | **Three.js** | r170 (0.170.0) | Spec-mandated; mature WebGL library, excellent for all required 3D effects |
| Post-Processing | **three/examples/jsm/postprocessing** | (bundled with Three.js) | Bloom/glow via `UnrealBloomPass`, required for neon aesthetic |
| 2D Charts | **Chart.js** | 4.4+ | Burndown, velocity, and sprint metric charts; lightweight, canvas-based |
| CSS | **Vanilla CSS with CSS custom properties** | — | Dark theme variables, glassmorphism via `backdrop-filter`, no framework needed |
| Animation | **GSAP** | 3.12+ | Smooth counter animations, panel transitions, camera easing; MIT-like license for non-commercial |
| Orbit Controls | **three/examples/jsm/controls/OrbitControls** | (bundled) | Optional user camera control as spec allows |

### Backend
| Layer | Library | Version | Rationale |
|-------|---------|---------|-----------|
| Runtime | **Node.js** | 20 LTS or 22 LTS | Spec-mandated; even-numbered LTS for stability |
| Framework | **Express** | 4.21+ | Spec-mandated; serves 7 REST endpoints + static files |
| CORS | **cors** | 2.8+ | Enable frontend dev server to call backend API |
| Static Serving | Express `express.static()` | (built-in) | Serve production frontend build from Express in production mode |

### Data
| Concern | Approach |
|---------|----------|
| Storage | In-memory JSON files in `/server/data/` |
| Format | Structured mock JSON with realistic project management data |
| Database | **None required** — spec explicitly states mock data only |

### Testing
| Type | Tool | Version | Rationale |
|------|------|---------|-----------|
| Unit (logic) | **Vitest** | 3.x | Native Vite integration, fast, TypeScript-first |
| Component/Visual | **Playwright** | 1.49+ | Already configured in workspace; headless browser for WebGL screenshot testing |
| API | **Supertest** | 7.x | Express endpoint validation |
| Linting | **ESLint** | 9.x (flat config) | TypeScript plugin for type-aware linting |
| Formatting | **Prettier** | 3.x | Consistent code style |

### Infrastructure / Tooling
| Concern | Tool |
|---------|------|
| Package Manager | **npm** (workspace-native) |
| Dev Server (frontend) | Vite dev server with proxy to Express |
| Dev Server (backend) | **nodemon** or **tsx --watch** for auto-restart |
| CI | GitHub Actions (repo is on GitHub) |
| Bundling | Vite production build → Express serves `/dist` |

---

## Architecture Recommendations

### Overall Pattern: Monorepo with Client/Server Split

```
/
├── server/
│   ├── index.js              # Express entry point
│   └── data/
│       └── mockData.js       # All mock JSON datasets
├── client/
│   ├── index.html            # Vite entry
│   ├── package.json
│   └── src/
│       ├── main.ts           # App bootstrap, scene init
│       ├── api.ts            # Fetch wrapper for all 7 endpoints
│       ├── scene/
│       │   ├── SceneManager.ts       # Three.js scene, camera, renderer, post-processing
│       │   ├── ProjectHierarchy.ts   # 3D node graph (epics → features → stories)
│       │   ├── RiskRadar.ts          # Animated radar/orbit visualization
│       │   ├── Timeline.ts           # 3D horizontal roadmap
│       │   ├── ParticleBackground.ts # Ambient particle system
│       │   └── CameraController.ts   # Fly-in, click-to-focus, orbit
│       ├── components/
│       │   ├── OverviewPanel.ts      # Project summary HUD overlay
│       │   ├── SprintMetrics.ts      # Chart.js burndown/velocity
│       │   ├── TeamActivity.ts       # Activity feed with animated pulses
│       │   ├── DetailPanel.ts        # Slide-out panel for clicked items
│       │   └── LoadingScreen.ts      # Loading state with animation
│       └── styles.css
├── README.md
└── package.json              # Root scripts: `npm run dev` starts both
```

### Data Flow

1. **Startup**: Vite dev server proxies `/api/*` to Express on port 3001
2. **Frontend boot**: `main.ts` calls all API endpoints in parallel via `Promise.all`
3. **Scene construction**: Data populates Three.js scene objects (nodes, rings, charts)
4. **Interaction**: Raycaster detects hover/click on 3D objects → triggers detail panel or camera focus
5. **Animation loop**: `requestAnimationFrame` drives continuous floating motion, particle updates, counter animations

### Key Architectural Decisions

- **HTML overlay for 2D elements**: Use CSS-positioned HTML panels overlaid on the Three.js canvas rather than rendering text in WebGL. This keeps text crisp, accessible, and easy to style with glassmorphism CSS.
- **Raycasting for interaction**: Three.js `Raycaster` maps mouse events to 3D objects. Each interactive mesh carries a `userData` property linking to the mock data record.
- **Post-processing pipeline**: `EffectComposer` → `RenderPass` → `UnrealBloomPass` for glow. Apply selectively using layer masking to avoid blooming the entire scene.
- **Module-per-section**: Each dashboard section (hierarchy, radar, timeline) is a self-contained class that owns its Three.js objects and exposes `update(dt)` for the animation loop.

---

## Security & Infrastructure

### Authentication & Authorization
- **Not required**. The spec explicitly states no external services or authentication.
- If future integration with real project APIs is needed, recommend OAuth2/OIDC via Azure AD (MSAL.js 2.x) at that point.

### Data Protection
- All data is mock—no PII, no sensitive data, no encryption needed.
- When transitioning to real APIs, enforce HTTPS and token-based API auth.

### Hosting & Deployment
- **Local-only** per spec. Run with `npm install && npm run dev`.
- **If deployed for demos**: Azure Static Web Apps (free tier) for frontend + Azure App Service (B1, ~$13/mo) for Express backend. Or a single App Service serving both.
- **Containerization** (optional): Single Dockerfile with multi-stage build—Vite builds frontend, then Node serves the bundle + API. Image size ~150MB.

### Estimated Infrastructure Costs
| Scale | Setup | Monthly Cost |
|-------|-------|-------------|
| Local demo | localhost | $0 |
| Small (shared demo) | Azure App Service B1 | ~$13/mo |
| Medium (team access) | Azure App Service S1 + CDN | ~$75/mo |

---

## Risks & Trade-offs

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **Three.js performance with 40+ animated nodes + particles + bloom** | Medium | Use `InstancedMesh` for repeated geometries; limit particle count; use `LOD` for distant nodes; profile with Chrome DevTools GPU panel |
| **WebGL compatibility across browsers** | Low | Three.js abstracts well; test on Chrome, Edge, Firefox. Safari WebGL2 support is now solid. |
| **GSAP licensing** | Medium | GSAP's "no charge" license covers internal tools and non-commercial use. If commercializing, need Business license ($199). Alternative: use Three.js's built-in `Clock` + custom easing or `anime.js` (MIT). |
| **Scope creep from visual polish** | High | The spec's visual requirements are extensive. Timebox each section's animation work. Ship functional-but-plain first, then layer effects. |
| **Workspace config mismatch** | Low | The AgentSquad workspace is configured for `dotnet run` but this is a Node.js project. Update `AppStartCommand` to `npm run dev` and adjust build/test commands. |

### Trade-off: Vanilla TS vs React

| Factor | Vanilla TS | React |
|--------|-----------|-------|
| Bundle size | Smaller (~5KB app + Three.js) | +40KB for React runtime |
| Three.js integration | Direct, no abstraction layer | Needs `@react-three/fiber` which adds complexity |
| DOM updates | Manual but minimal (few HTML panels) | Overkill for 6-7 overlay panels |
| Developer familiarity | Lower barrier | Higher if team knows React |

**Recommendation**: Vanilla TS. The app is primarily a Three.js scene with thin HTML overlays. React adds indirection with no proportional benefit.

---

## Open Questions

1. **Target browsers/devices**: Is this desktop-only (spec says "desktop browser") or should tablet/touch be supported? Touch affects orbit controls and hover interactions.
2. **Performance floor**: What's the minimum acceptable GPU? Integrated graphics (Intel UHD) or discrete only? This affects particle count and post-processing decisions.
3. **Future real-data integration**: Which project management tool (Azure DevOps, Jira, GitHub Projects) will eventually replace mock data? This affects the shape of the API contract.
4. **Branding**: Should the dashboard carry specific brand colors/logos, or is the spec's "neon futuristic" aesthetic the final direction?
5. **Accessibility**: The spec doesn't mention a11y. Should we provide a non-3D fallback view for screen readers or reduced-motion preferences?
6. **GSAP licensing**: Confirm the project's commercial/non-commercial status to determine if the free GSAP license applies or if an alternative (anime.js) should be used.

---

## Implementation Recommendations

### Phasing

**Phase 1 — Scaffold & Core Scene (Week 1)**
- Set up Vite + TypeScript + Express project structure per spec's folder layout
- Implement all 7 Express API endpoints with complete mock dataset
- Create Three.js scene with camera, renderer, post-processing pipeline (bloom)
- Build particle background and basic dark-mode CSS
- **Deliverable**: Running app with 3D canvas and API data loading

**Phase 2 — Dashboard Sections (Weeks 2–3)**
- Project Overview HUD overlay with animated counters and health ring
- 3D Project Hierarchy node graph (epics → features → stories) with color-coded statuses
- Sprint Metrics panel with Chart.js burndown and velocity charts
- Raycaster-based click interaction → Detail Panel slide-out
- **Deliverable**: Interactive dashboard with 3 of 7 sections functional

**Phase 3 — Remaining Sections & Polish (Week 4)**
- Risk & Blocker Radar (animated orbit visualization)
- Team Activity feed with animated pulses
- 3D Timeline/Roadmap with milestones
- Camera fly-in animation on load
- Hover glow effects, floating motion, transition polish
- **Deliverable**: Feature-complete dashboard matching spec

**Phase 4 — Testing & Documentation (Week 5)**
- Playwright visual regression tests (screenshot comparison)
- Vitest unit tests for API layer and data transformation
- Supertest for Express endpoint validation
- README with install, run, customization, and real-API migration notes
- **Deliverable**: Production-ready codebase with documentation

### Quick Wins
- **Particle background + bloom**: Visually impressive with ~50 lines of Three.js code. Demo-worthy on day 1.
- **Animated counters**: GSAP `countTo` on the overview panel numbers creates immediate "wow" factor.
- **Mock data**: Front-load mock data creation—realistic data makes every subsequent visual more compelling.

### Prototype Before Committing
- **3D node graph layout**: Prototype the force-directed or hierarchical layout for the project hierarchy view. Layouts with 40+ nodes need performance validation and visual tuning before building the full interaction model.
- **Bloom + glassmorphism interaction**: Test that CSS glassmorphism `backdrop-filter` composites correctly over a bloomed Three.js canvas. Some browser/GPU combinations produce artifacts.
