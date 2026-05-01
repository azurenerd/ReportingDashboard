# PM Specification: ReportingDashboard

## Executive Summary

The ReportingDashboard is a polished, 3D animated web-based project reporting dashboard that serves as a futuristic "project command center" for executive presentations. Built on Node.js/Express (backend) and Three.js/Vite (frontend) with mock JSON data, it visualizes project management data — hierarchy, sprint metrics, risks, team activity, and roadmap — through interactive WebGL-powered 3D scenes with glassmorphism styling, bloom effects, and smooth animations, delivering a premium demo experience that is both visually impressive and practically readable for PMs and engineering managers.

## Business Goals

1. **Deliver a visually compelling executive demo tool** that communicates project health, status, and risk at a glance in a futuristic, premium aesthetic suitable for leadership presentations.
2. **Prove the viability of 3D data visualization** for project management by building a fully functional prototype that demonstrates interactive WebGL dashboards can be both beautiful and usable.
3. **Establish a reusable dashboard foundation** with a documented mock-data-to-real-API migration path, enabling future integration with live project management systems (e.g., Azure DevOps, Jira).
4. **Showcase modern web engineering capabilities** by delivering a full-stack application (Node.js + Three.js + React) that runs locally with zero external dependencies, demonstrating team proficiency with cutting-edge front-end technologies.
5. **Enable instant comprehension of project status** across seven key dimensions (overview, hierarchy, sprint metrics, risks, team activity, roadmap, and drill-down detail) without requiring the viewer to navigate multiple tools.

## User Stories & Acceptance Criteria

### US-1: Project Overview at a Glance

**As an** executive viewer, **I want** to see a high-level project overview card displaying key health indicators, **so that** I can instantly assess project status without reading a report.

**Acceptance Criteria:**
- [ ] Dashboard displays project name, status, completion percentage, delivery confidence, current sprint, days remaining, and overall health score
- [ ] All metrics are fetched from `GET /api/project-summary`
- [ ] Numeric values animate on load using animated counters
- [ ] Health score is rendered as an animated progress ring with meaningful color coding (green/yellow/red)
- [ ] Card uses glassmorphism styling consistent with the dark-mode futuristic theme

### US-2: Interactive 3D Project Hierarchy

**As a** PM or engineering manager, **I want** to explore the project's work item hierarchy (Epics → Features → Stories/Tasks) as an interactive 3D node graph, **so that** I can visually understand the project's structure and identify problem areas.

**Acceptance Criteria:**
- [ ] Epics render as large floating nodes, features as medium child nodes, and stories/tasks as smaller connected nodes
- [ ] Nodes are color-coded by status: Done (green), In Progress (blue), Blocked (red), Not Started (gray), At Risk (orange)
- [ ] Animated connections link parent-child relationships between hierarchy levels
- [ ] Hovering a node triggers a glow effect
- [ ] Clicking a node opens the Report Detail Panel (US-7) with that item's details
- [ ] Camera smoothly animates to focus on the clicked node (click-to-focus)
- [ ] Data is fetched from `GET /api/project-items`
- [ ] Minimum data: 4 epics, 12 features, 40+ stories/tasks/bugs

### US-3: Sprint Metrics Dashboard

**As a** PM, **I want** to view current sprint metrics including velocity, burndown, blockers, and carryover, **so that** I can assess sprint health and delivery trajectory.

**Acceptance Criteria:**
- [ ] Displays velocity, planned vs. completed work, burndown trend, open bugs, blockers, and carryover items
- [ ] Burndown and velocity are rendered as 2D charts (Chart.js, D3, Recharts, or equivalent)
- [ ] Data is fetched from `GET /api/sprint-metrics`
- [ ] Charts animate on initial render
- [ ] Section is styled with glassmorphism cards consistent with the overall theme

### US-4: Risk & Blocker Radar

**As an** executive viewer, **I want** to see risks and blockers displayed in an animated radar or orbital visualization grouped by severity, **so that** I can immediately identify the most critical threats to the project.

**Acceptance Criteria:**
- [ ] Risks are visualized in an animated radar or orbit-style layout
- [ ] Risks are grouped by severity (Critical, High, Medium, Low)
- [ ] High-risk items glow red/orange
- [ ] Each risk includes a description and owner (mock data)
- [ ] Data is fetched from `GET /api/risks`
- [ ] Minimum data: 8 risks or blockers
- [ ] Clicking a risk item opens the Detail Panel with full information

### US-5: Team Activity Feed

**As a** PM, **I want** to see a real-time-style feed of recent team activity (PRs, task completions, updates), **so that** I can understand team momentum and recent contributions.

**Acceptance Criteria:**
- [ ] Displays a scrollable/animated feed of recent updates
- [ ] Shows contributor activity with mock PR/task completion events
- [ ] Activity entries appear with animated pulses
- [ ] Data is fetched from `GET /api/team-activity`
- [ ] Minimum data: 10 team members, 20 activity feed events

### US-6: 3D Timeline / Roadmap

**As an** executive viewer, **I want** to see a 3D horizontal timeline showing milestones, releases, and sprint boundaries, **so that** I can understand the project's trajectory and key upcoming dates.

**Acceptance Criteria:**
- [ ] Renders as a 3D horizontal timeline with depth and glow effects
- [ ] Shows milestones, releases, and sprint boundaries as distinct visual markers
- [ ] Completed phases, active phases, and upcoming phases are visually differentiated
- [ ] Timeline path glows
- [ ] Data is fetched from `GET /api/roadmap`
- [ ] Minimum data: 6 roadmap milestones

### US-7: Report Detail Panel

**As a** user, **I want** to click any card or 3D node to open a detail panel with comprehensive item information, **so that** I can drill into specifics without leaving the dashboard.

**Acceptance Criteria:**
- [ ] Panel slides in smoothly with a transition animation when a node/card is clicked
- [ ] Displays: title, description, owner, status, priority, estimate, remaining work, dependencies, and recent activity
- [ ] Panel closes smoothly when dismissed
- [ ] Data is fetched from `GET /api/report/:id`
- [ ] Panel uses glassmorphism styling consistent with the overall theme

### US-8: Cinematic Load Experience

**As an** executive viewer, **I want** the dashboard to open with a smooth cinematic camera fly-in animation, **so that** the initial impression is visually stunning and sets the tone for the presentation.

**Acceptance Criteria:**
- [ ] On page load, the camera executes a smooth fly-in sequence revealing the full dashboard
- [ ] A loading state is displayed while data is being fetched from the backend
- [ ] All sections animate into view as the camera arrives at its resting position
- [ ] Particle background is visible throughout the animation
- [ ] The entire sequence completes within 3–5 seconds

### US-9: Interactive 3D Navigation

**As a** user, **I want** orbit controls to freely explore the 3D scene, **so that** I can inspect the dashboard from different angles and focus on areas of interest.

**Acceptance Criteria:**
- [ ] Orbit controls (rotate, zoom, pan) are available after the initial fly-in completes
- [ ] 3D objects respond to hover with glow effects and subtle floating motion
- [ ] Clicking a node smoothly transitions the camera to focus on it
- [ ] Bloom and glow post-processing effects are active across the scene
- [ ] Controls are smooth and do not exhibit jitter or lag at 60 FPS on a modern desktop GPU

### US-10: Zero-Configuration Local Setup

**As a** developer, **I want** to clone the repo and run the full application with `npm install && npm run dev`, **so that** I can demo or develop without configuring databases, services, or credentials.

**Acceptance Criteria:**
- [ ] Running `npm install` at the root installs all client and server dependencies
- [ ] Running `npm run dev` starts both frontend (Vite) and backend (Express) concurrently
- [ ] Frontend is accessible at `localhost` (e.g., port 5173)
- [ ] Backend API is accessible at `localhost:3001` (or configured port)
- [ ] No database, authentication, or external service is required
- [ ] A polished README documents install, run, and customization instructions

## Scope

### In Scope

- Full-stack web application: Node.js/Express backend + Three.js/Vite frontend
- 7 REST API endpoints serving mock JSON data (in-memory or static JSON files)
- 7 dashboard sections: Project Overview, 3D Hierarchy, Sprint Metrics, Risk Radar, Team Activity, Timeline/Roadmap, Detail Panel
- Dark-mode futuristic visual theme with glassmorphism, neon accents, bloom/glow effects
- Interactive 3D scene with orbit controls, click-to-focus, hover glow
- Cinematic camera fly-in animation on load
- Animated counters, progress rings, particle background, floating motion
- Mock data meeting minimum volume requirements (1 project, 4 epics, 12 features, 40+ tasks, 8 risks, 10 team members, 20 activity events, 6 milestones)
- Loading states and graceful error handling
- Clean, modular, component-based code with inline comments on major sections
- Polished README with install/run instructions and design decision notes
- Documentation on how to replace mock data with real project APIs
- Responsive desktop layout
- Complete source code deliverable

### Out of Scope

- **Authentication and authorization** — no login, tokens, or role-based access
- **Database integration** — no SQL, NoSQL, or any persistent storage
- **External service integrations** — no Azure DevOps, Jira, GitHub API, or other live data sources
- **Mobile or tablet responsive design** — desktop-only layout
- **Accessibility (WCAG compliance)** — this is a visual demo, not a production accessibility target
- **Automated CI/CD pipelines** — local development only
- **Production deployment infrastructure** — no Dockerfiles, Kubernetes, or cloud provisioning
- **Real-time data or WebSocket connections** — static mock data served via REST
- **User preferences or settings persistence** — no localStorage, cookies, or profiles
- **Multi-project support** — single project dashboard only
- **Internationalization (i18n)** — English only
- **Browser compatibility testing** — optimized for modern Chromium-based browsers; Firefox/Safari support is not guaranteed
- **Replacing or removing the existing .NET 8 Blazor scaffold** — disposition of `src/ReportingDashboard/` is deferred as an open question

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Initial page load (with fly-in animation) | < 5 seconds on modern desktop with dedicated GPU |
| Frame rate during idle scene | ≥ 60 FPS on dedicated GPU; ≥ 30 FPS on integrated GPU |
| API response time (mock data) | < 50 ms per endpoint |
| Time to interactive (all sections rendered) | < 8 seconds |
| Client bundle size (gzipped) | < 500 KB excluding Three.js (Three.js ~150 KB gzipped is acceptable) |

### Visual Quality

- Bloom/glow post-processing must not cause text to become unreadable
- Glassmorphism panels must maintain legible contrast ratios for all text content
- Animations must be smooth (no frame drops during transitions on target hardware)
- Particle background must not obscure dashboard content

### Reliability

- Application must not crash or show WebGL context loss errors during a 30-minute demo session
- All 7 API endpoints must return valid JSON with correct schema on every request
- Frontend must display a meaningful loading state while data loads and a graceful error message if the backend is unreachable

### Code Quality

- TypeScript strict mode enabled for all client code
- Component-based architecture with clear separation of concerns (scene components vs. UI overlays vs. data fetching)
- Inline comments explaining major sections and design decisions
- All mock data is centralized and easy to customize

### Security

- No authentication or data protection required (mock data only)
- No secrets, credentials, or API keys in the codebase
- Future-proofing: API calls are centralized in a single module (`api.ts`) to enable adding authentication headers when transitioning to real APIs

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **All 7 dashboard sections functional** | 7/7 sections render with correct data | Manual verification against section requirements |
| 2 | **All 7 API endpoints operational** | 7/7 endpoints return valid mock JSON | HTTP request testing (curl or browser dev tools) |
| 3 | **Mock data volume met** | ≥ 1 project, 4 epics, 12 features, 40 tasks, 8 risks, 10 team members, 20 activities, 6 milestones | Count items in mock data files |
| 4 | **Zero-config setup works** | `npm install && npm run dev` launches full app | Fresh clone test on a clean machine |
| 5 | **Cinematic fly-in plays** | Camera animation executes on load without stutter | Visual demo on target hardware |
| 6 | **All mandatory interactions work** | Orbit controls, click-to-focus, hover glow, detail panel open/close, animated counters | Manual interaction testing |
| 7 | **Frame rate on target hardware** | ≥ 30 FPS sustained on integrated GPU | Browser dev tools FPS monitor |
| 8 | **Executive demo readiness** | Stakeholder sign-off that visual quality meets "premium demo" bar | Stakeholder review session |
| 9 | **README completeness** | Includes install, run, architecture overview, design decisions, and mock-to-real migration notes | Document review |
| 10 | **Code modularity** | Each dashboard section is an independent component; mock data is centralized | Code review |

## Constraints & Assumptions

### Technical Constraints

- **Tech stack is prescribed:** Node.js/Express backend, Three.js frontend, Vite build tool — these are non-negotiable per the feature specification
- **No database:** All data must be mock JSON served from in-memory objects or static files
- **No external services:** The application must be fully self-contained and run locally
- **No authentication:** No login flow, tokens, or user identity
- **WebGL required:** The target browser must support WebGL 2.0; there is no fallback rendering path
- **Desktop only:** No mobile or tablet layout is required
- **Existing repo context:** The codebase lives in the AgentSquad monorepo alongside a .NET 8 solution; the Node.js project must coexist without breaking the existing solution structure

### Timeline Assumptions

- **Phase 1 (Foundation & Data):** 3 days — monorepo setup, all API endpoints, basic R3F canvas with data flow
- **Phase 2 (3D Scenes & Interactivity):** 5 days — all 7 sections functional with interactions
- **Phase 3 (Visual Polish & Animation):** 4 days — cinematic fly-in, bloom/glow, glassmorphism, particles, performance tuning
- **Total estimated effort:** 12 working days for a single full-stack developer
- Timeline assumes developer familiarity with React and Three.js; ramp-up on R3F or GSAP may add 1–2 days

### Dependency Assumptions

- **Node.js 22 LTS** is available on the development machine
- **Modern Chromium-based browser** (Chrome, Edge) with hardware-accelerated WebGL is available for development and demo
- **No approval gates** are required for open-source library selection (React, Three.js, GSAP, etc.)
- **Mock data schema** is self-defined (no dependency on an external system's API contract), but should resemble realistic project management data structures to ease future integration
- **Presentation hardware** has at minimum an integrated GPU capable of WebGL 2.0 at 1080p resolution; dedicated GPU is preferred for optimal visual quality

### Open Questions (Requiring Stakeholder Input)

1. **Existing .NET project disposition:** Should `src/ReportingDashboard/` be replaced, retained as a parallel implementation, or removed?
2. **Target presentation hardware:** What specific GPU/browser/resolution will be used for the executive demo?
3. **Real API integration target:** Is there a known system (Azure DevOps, Jira, etc.) whose data model should influence the mock data schema?
4. **Repository structure:** Should the Node.js dashboard live inside the AgentSquad monorepo or be extracted to a standalone repository?
5. **Browser support scope:** Is Chrome-only acceptable, or must Firefox and Edge be fully supported?