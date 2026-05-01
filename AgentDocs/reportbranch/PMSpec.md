# PM Specification: ReportingDashboard

## Executive Summary

The ReportingDashboard is a futuristic 3D animated web application that serves as an interactive "project command center" for visualizing project management data during executive demos and engineering leadership reviews. Built on Node.js + Express with a Three.js-powered WebGL frontend, it renders mock project data—epics, features, stories, risks, sprint metrics, and team activity—as an immersive, glassmorphism-styled dashboard that is both visually impressive and functionally informative. The application runs entirely locally with mock JSON data, requiring only `npm install && npm run dev` to launch.

## Business Goals

1. **Deliver a high-impact demo tool** that showcases project health, progress, and risks in a visually compelling 3D interface suitable for executive and engineering leadership presentations.
2. **Provide at-a-glance project insight** by consolidating project overview, hierarchy, sprint metrics, risks, team activity, and roadmap into a single interactive dashboard.
3. **Establish a reusable visualization platform** with a clean architecture and documented data layer that can be extended to consume real APIs (Azure DevOps, Jira, GitHub Projects) in future phases.
4. **Demonstrate technical excellence** through premium visual quality—glassmorphism, bloom effects, animated counters, 3D node graphs, and smooth camera transitions—setting a standard for internal tooling aesthetics.
5. **Minimize operational overhead** by requiring zero infrastructure, zero authentication, and zero external service dependencies; the entire application runs locally from source.

## User Stories & Acceptance Criteria

### US-1: Project Overview at a Glance

**As an** engineering leader, **I want** to see a high-level project summary (name, status, completion %, delivery confidence, current sprint, days remaining, health score) on the main dashboard, **so that** I can assess overall project health in seconds.

- [ ] Project Overview panel displays all 7 required data points: project name, status, completion percentage, delivery confidence, current sprint, days remaining, overall health score
- [ ] Completion percentage renders as an animated progress ring that counts up from 0 on load
- [ ] Health score is color-coded (green/yellow/red) based on threshold values
- [ ] Data is fetched from `GET /api/project-summary` endpoint
- [ ] A loading state is shown while data is being fetched
- [ ] Panel uses glassmorphism styling consistent with the dark futuristic theme

### US-2: 3D Project Hierarchy Exploration

**As a** program manager, **I want** to view the project hierarchy (epics → features → stories/tasks) as an interactive 3D force-directed node graph, **so that** I can understand work breakdown structure and status distribution visually.

- [ ] Epics render as large floating nodes, features as medium nodes, stories/tasks as smaller nodes
- [ ] Animated connections (edges) link parent nodes to child nodes across hierarchy levels
- [ ] Nodes are color-coded by status: Done (green), In Progress (blue), Blocked (red), Not Started (gray), At Risk (orange)
- [ ] Clicking a node opens the Report Detail Panel (US-7) with that item's details
- [ ] Camera smoothly animates to focus on the clicked node
- [ ] Graph renders at least 56 nodes (4 epics + 12 features + 40 stories) without dropping below 30fps on a modern desktop browser
- [ ] Data is fetched from `GET /api/project-items` endpoint
- [ ] Optional orbit controls allow the user to rotate and zoom the 3D graph

### US-3: Sprint Metrics Review

**As an** engineering manager, **I want** to view sprint velocity, planned vs. completed work, burndown trend, open bugs, blockers, and carryover items, **so that** I can evaluate sprint execution effectiveness.

- [ ] Velocity is displayed as an animated bar chart comparing last 3+ sprints
- [ ] Burndown chart shows planned ideal line vs. actual remaining work trend
- [ ] Planned vs. completed work is visualized as a comparative chart
- [ ] Open bug count, blocker count, and carryover item count are displayed as animated counters
- [ ] Data is fetched from `GET /api/sprint-metrics` endpoint
- [ ] Charts animate on initial render with smooth transitions
- [ ] Panel uses glassmorphism card styling

### US-4: Risk & Blocker Radar

**As a** program manager, **I want** to see risks and blockers displayed in an animated radar or orbit-style visualization grouped by severity, **so that** I can quickly identify and prioritize the highest-impact risks.

- [ ] Risks render in a radar/orbit visualization with severity-based grouping (inner orbit = critical, outer = low)
- [ ] High-severity risk items glow red/orange with neon accent effects
- [ ] Each risk node displays or tooltips the risk description and owner
- [ ] At least 8 risks/blockers are rendered from mock data
- [ ] Data is fetched from `GET /api/risks` endpoint
- [ ] Visualization animates (orbit rotation or pulse) continuously

### US-5: Team Activity Feed

**As an** engineering leader, **I want** to see a real-time-style feed of recent team activity (PR completions, task updates, deployments), **so that** I can gauge team momentum and engagement.

- [ ] Activity feed displays at least 20 mock events (pull requests, task completions, comments)
- [ ] Each event shows contributor avatar/name, action type, timestamp, and target item
- [ ] New events appear with animated entry transitions (pulse or slide-in)
- [ ] Contributor activity is visually attributed to the 10 mock team members
- [ ] Data is fetched from `GET /api/team-activity` endpoint
- [ ] Feed auto-scrolls or reveals items with animated pulses

### US-6: Timeline / Roadmap View

**As a** program manager, **I want** to see a 3D horizontal timeline showing milestones, releases, and sprint boundaries, **so that** I can communicate project trajectory and upcoming deliverables to stakeholders.

- [ ] Timeline renders as a 3D horizontal visualization with at least 6 milestones
- [ ] Completed phases are visually distinct from active and upcoming phases (color/opacity/glow differentiation)
- [ ] Sprint boundaries are marked along the timeline
- [ ] Milestones and releases are labeled and interactive (click to see details)
- [ ] Data is fetched from `GET /api/roadmap` endpoint
- [ ] Timeline supports smooth camera movement/scrolling along the time axis

### US-7: Report Detail Panel

**As a** user, **I want** to click any card or 3D node and see a slide-out detail panel showing title, description, owner, status, priority, estimate, remaining work, dependencies, and recent activity, **so that** I can drill down into any work item without leaving the dashboard.

- [ ] Detail panel slides in from the right with a smooth open/close animation
- [ ] Panel displays all 9 required fields: title, description, owner, status, priority, estimate, remaining work, dependencies, recent activity
- [ ] Panel is triggered by clicking any 3D hierarchy node (US-2) or interactive card
- [ ] Data is fetched from `GET /api/report/:id` endpoint using the selected item's ID
- [ ] Panel can be closed by clicking outside it or pressing a close button
- [ ] Panel uses glassmorphism styling consistent with dashboard theme

### US-8: Immersive Visual Experience

**As a** presenter, **I want** the dashboard to feature a dark futuristic aesthetic with particle backgrounds, bloom/glow effects, smooth camera fly-in, floating motion, and hover glow effects, **so that** it creates a premium, memorable impression during executive demos.

- [ ] Initial page load triggers a smooth camera fly-in animation to the main dashboard view
- [ ] Particle background renders behind all dashboard content with continuous subtle motion
- [ ] Bloom/glow post-processing effects are applied to the 3D scene
- [ ] All interactive cards and nodes exhibit hover glow effects
- [ ] Floating/subtle bobbing motion is applied to 3D panels and nodes
- [ ] Glassmorphism styling (backdrop blur, translucent backgrounds, subtle borders) is applied to all card elements
- [ ] Soft shadows are rendered on 3D objects
- [ ] Typography is clean and legible against the dark background

### US-9: Local Development Setup

**As a** developer, **I want** to clone the repo, run `npm install && npm run dev`, and have both the frontend and backend start automatically, **so that** I can get the dashboard running with zero configuration.

- [ ] Single `npm install` at the root installs all dependencies (client + server)
- [ ] Single `npm run dev` starts both the Vite dev server (port 5173) and Express server (port 3001) concurrently
- [ ] README includes clear install and run instructions
- [ ] README documents major design decisions
- [ ] README includes notes on how to replace mock data with real APIs
- [ ] Mock data is easy to locate and customize (single `mockData.ts` file)

## Scope

### In Scope

- Full-stack web application with Node.js + Express backend and React + TypeScript + Vite frontend
- Three.js / React Three Fiber 3D rendering with bloom, particles, and post-processing effects
- 7 dashboard sections: Project Overview, 3D Hierarchy View, Sprint Metrics, Risk Radar, Team Activity, Timeline/Roadmap, Detail Panel
- 7 REST API endpoints serving mock JSON data from in-memory objects
- Dark futuristic glassmorphism visual theme with neon accents
- Animation system: camera fly-in, animated counters, progress rings, hover glow, floating motion, smooth panel transitions
- Mock dataset: 1 project, 4 epics, 12 features, 40+ stories/tasks/bugs, 8 risks, 10 team members, 20 activity events, 6 milestones
- Interactive 3D force-directed node graph for project hierarchy
- Animated radar/orbit visualization for risks
- 3D horizontal timeline for roadmap
- Chart.js-based burndown and velocity charts
- Loading states and graceful error handling for all API calls
- Responsive desktop browser layout
- Clean code organization with comments on major sections
- README with installation, running instructions, design decisions, and API replacement guide
- Optional: Dockerfile for portable deployment

### Out of Scope

- **Authentication and authorization** — no login, no user roles, no session management
- **Database integration** — no SQL, NoSQL, or any persistent storage; mock JSON only
- **External service integrations** — no Azure DevOps, Jira, GitHub, or any third-party API calls
- **Real-time data / WebSocket connections** — all data is static mock JSON
- **Multi-project support** — dashboard displays exactly 1 mock project
- **Mobile or tablet layouts** — desktop browser only
- **Accessibility / WCAG compliance** — this is a visual demo tool, not a production accessibility-compliant application
- **Automated CI/CD pipelines** — local development only
- **Production deployment infrastructure** — no cloud hosting, CDN, or domain setup
- **User preferences or settings persistence** — no localStorage or cookie-based state
- **Internationalization (i18n)** — English only
- **Print or export functionality** — no PDF generation or data export

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Initial load to interactive | < 5 seconds on a modern desktop (M1 MacBook Pro or equivalent) |
| 3D scene frame rate | ≥ 30fps sustained with all sections rendered; ≥ 60fps target on discrete GPU |
| API response time | < 100ms for all mock data endpoints (in-memory JSON) |
| Time to first meaningful paint | < 2 seconds (loading screen shown immediately, 3D scene initializes behind it) |
| Bundle size (frontend) | < 2MB gzipped total (including Three.js and dependencies) |

### Performance Safeguards

- Implement adaptive quality settings (low/medium/high) to degrade particle count and post-processing on lower-end hardware
- Use `InstancedMesh` for repeated 3D geometries to reduce draw calls
- Show a loading screen during initial Three.js shader compilation
- Cap particle count at a configurable maximum (default: 1000)

### Security

- CORS middleware restricts origins to `localhost:5173` in development; origin whitelist configurable via environment variable
- Input validation on `GET /api/report/:id` parameter to prevent injection if data source changes later
- CSP headers via `helmet` middleware with WebGL-compatible policy
- Zero secrets, API keys, or credentials in the codebase

### Reliability

- Graceful error handling: if any API call fails, the corresponding dashboard section shows an error state rather than crashing the application
- 3D scene failures (WebGL context loss) are caught and display a fallback message
- Application functions fully offline after initial `npm install` — no runtime network dependencies

### Browser Compatibility

- Chrome 120+ (primary target)
- Edge 120+
- Firefox 120+
- Safari 17+ (with `backdrop-filter` support)
- WebGL 2.0 required; fallback message displayed if unavailable

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **All 7 dashboard sections render correctly** | 100% of sections display data from their respective API endpoints | Manual verification against acceptance criteria checklist |
| 2 | **All 7 API endpoints return valid JSON** | 100% success rate with correct schema | Automated API tests or manual curl verification |
| 3 | **Mock data completeness** | ≥ 1 project, 4 epics, 12 features, 40 stories, 8 risks, 10 team members, 20 events, 6 milestones | Count verification against mockData.ts |
| 4 | **Zero-config startup** | `npm install && npm run dev` launches full application successfully | Fresh clone test on clean machine |
| 5 | **Visual quality bar** | Dashboard is deemed "executive demo ready" by project stakeholder | Stakeholder sign-off |
| 6 | **3D performance** | ≥ 30fps on target hardware with all sections active | Browser DevTools FPS counter |
| 7 | **All 9 animation types implemented** | Camera fly-in, hover glow, animated counters, progress rings, floating motion, particle background, click-to-focus, panel transitions, activity pulses | Manual verification |
| 8 | **Detail panel drill-down works** | Clicking any 3D node or card opens detail panel with correct item data | Manual interaction test |
| 9 | **Documentation complete** | README covers install, run, design decisions, and API replacement guide | Document review |
| 10 | **All 5 deliverables shipped** | Source code, README, mock data, design decision notes, API replacement notes | Deliverable checklist |

## Constraints & Assumptions

### Technical Constraints

- **Mandated stack:** Node.js 22 LTS, Express 4.x, Three.js r170+, Vite (if using React/TypeScript). No substitutions for core technologies.
- **No database:** All data must be served from in-memory JSON objects or static JSON files. No persistence layer.
- **No authentication:** No login flow, tokens, sessions, or user identity of any kind.
- **No external services:** Application must function with zero network calls beyond localhost. No third-party APIs, CDNs (at runtime), or cloud services.
- **Local-only operation:** The application is designed to run on `localhost` via `npm run dev`. Production deployment is out of scope.
- **WebGL dependency:** Application requires WebGL 2.0 support in the browser. No fallback rendering mode is required beyond an error message.
- **Single-page application:** The dashboard is a single view with section navigation via camera movement and panel overlays, not multi-page routing.

### Timeline Assumptions

- **Phase 1 (Skeleton + 3D POC):** 3–4 days — monorepo setup, Express endpoints, R3F canvas with particles/bloom, first glassmorphism card
- **Phase 2 (2D Dashboard Panels):** 2–3 days — Project Overview, Sprint Metrics, Team Activity, Detail Panel
- **Phase 3 (3D Hierarchy View):** 3–4 days — force-directed graph, node coloring, click-to-focus, detail panel integration (highest risk)
- **Phase 4 (Risk Radar + Timeline):** 2–3 days — D3 radar visualization, 3D timeline, section transitions
- **Phase 5 (Polish + Camera Choreography):** 2–3 days — fly-in sequence, hover effects, performance optimization, README
- **Total estimated duration:** 12–17 working days

### Dependency Assumptions

- Developers have Node.js 22 LTS installed locally
- Modern desktop browser with WebGL 2.0 and `backdrop-filter` CSS support is available for testing
- No design system or Figma mockups are provided; visual direction is derived from the spec's description ("dark futuristic SaaS aesthetic") and developer judgment
- The `three-forcegraph` library adequately handles 56+ nodes with acceptable performance; if not, a simpler tree layout will be substituted
- GSAP's free license is sufficient for this internal/demo use case (no GSAP Business license features required)
- Mock data schemas are defined by the development team based on the spec's minimum data requirements; no external schema contract exists
- Stakeholder review for "executive demo ready" quality bar will be conducted at the end of Phase 5; rework budget is assumed to be ≤ 2 additional days