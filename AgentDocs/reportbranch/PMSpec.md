# PM Specification: ReportingDashboard

## Executive Summary

The ReportingDashboard is a polished, futuristic 3D animated project reporting dashboard built as a self-contained full-stack web application. It visualizes project management data—hierarchy, sprint metrics, risks, timelines, and team activity—through an immersive WebGL-powered "project command center" experience designed to impress executive audiences while remaining practical for engineering and product managers. The application runs entirely on mock data with zero external dependencies, launchable via a single `npm install && npm run dev` command.

## Business Goals

1. **Deliver a premium demo asset** — Produce a visually stunning, executive-ready dashboard that can serve as a flagship demonstration of project reporting capabilities at leadership reviews, customer pitches, and trade shows.
2. **Prove 3D visualization feasibility** — Validate that Three.js/WebGL-based 3D project visualization is technically viable, performant, and adds meaningful value over traditional 2D dashboards for communicating project health.
3. **Enable rapid project health comprehension** — Allow a viewer to understand overall project status, risks, sprint progress, and team activity within 30 seconds of viewing the dashboard, without requiring training or onboarding.
4. **Create a reusable foundation** — Architect the application so that mock data can be replaced with real project management APIs (Azure DevOps, Jira, etc.) with minimal refactoring, enabling future productization.
5. **Minimize deployment friction** — Ensure the application is fully self-contained with no database, no authentication, and no external service dependencies, so any stakeholder can run it locally in under 2 minutes.

## User Stories & Acceptance Criteria

### US-01: Initial Dashboard Load Experience

**As an** executive viewer, **I want** the dashboard to open with a cinematic 3D camera fly-in animation over a dark futuristic scene, **so that** I am immediately impressed and engaged before any data is presented.

- [ ] Application loads and displays a Three.js canvas with a dark-mode background
- [ ] A subtle particle background is visible and continuously animating
- [ ] A smooth camera fly-in animation plays automatically on initial load (duration 2–4 seconds)
- [ ] Bloom/glow post-processing effects are active on accent elements
- [ ] Loading states are displayed while backend data is being fetched
- [ ] The scene achieves ≥30 fps on integrated GPUs during the fly-in animation

### US-02: Project Overview Section

**As an** engineering manager, **I want** to see a high-level project overview with key health indicators displayed in glassmorphism cards, **so that** I can assess overall project status at a glance.

*References: Section 5.1 — Project Overview*

- [ ] Displays project name, status, and current sprint
- [ ] Shows completion percentage with an animated counter (0% → actual value)
- [ ] Shows delivery confidence indicator
- [ ] Shows days remaining in current sprint
- [ ] Shows overall health score with color coding (green/yellow/red)
- [ ] All cards use glassmorphism styling (backdrop blur, semi-transparent background, border glow)
- [ ] Data is fetched from `GET /api/project-summary`
- [ ] Graceful error state shown if API call fails

### US-03: 3D Project Hierarchy View

**As a** product manager, **I want** to see epics, features, and stories rendered as an interactive 3D node graph with animated connections, **so that** I can visually understand the project structure and identify problem areas by status color.

*References: Section 5.2 — 3D Project Hierarchy View*

- [ ] Epics render as large floating 3D nodes
- [ ] Features render as medium child nodes connected to their parent epic
- [ ] Stories/tasks render as smaller nodes connected to their parent feature
- [ ] Connections between hierarchy levels are animated (pulsing or flowing)
- [ ] Nodes are color-coded by status: Done (green), In Progress (blue), Blocked (red), Not Started (gray), At Risk (orange)
- [ ] Minimum data: 4 epics, 12 features, 40+ stories/tasks
- [ ] Clicking any node triggers the detail panel (US-07)
- [ ] Camera smoothly animates to focus on clicked node
- [ ] Nodes have subtle floating motion and hover glow effects
- [ ] Data is fetched from `GET /api/project-items`

### US-04: Sprint Metrics Section

**As an** engineering manager, **I want** to view sprint velocity, burndown trends, and blocker counts in clear 2D charts, **so that** I can evaluate current sprint health and team throughput.

*References: Section 5.3 — Sprint Metrics*

- [ ] Displays velocity chart (planned vs. completed story points)
- [ ] Displays burndown trend chart
- [ ] Shows open bug count
- [ ] Shows blocker count
- [ ] Shows carryover items count
- [ ] Charts rendered using Chart.js (or equivalent) in HTML overlay panels
- [ ] Data is fetched from `GET /api/sprint-metrics`

### US-05: Risk & Blocker Radar

**As a** program manager, **I want** risks and blockers displayed in an animated radar or orbital visualization grouped by severity, **so that** I can quickly identify and prioritize the most critical project threats.

*References: Section 5.4 — Risk & Blocker Radar*

- [ ] Risks displayed as animated orbital or radar-style 3D nodes
- [ ] Risks grouped visually by severity level
- [ ] High-risk items glow red/orange with pronounced visual emphasis
- [ ] Each risk node shows a mock description and owner
- [ ] Minimum 8 risks/blockers displayed
- [ ] Clicking a risk node opens the detail panel (US-07)
- [ ] Data is fetched from `GET /api/risks`

### US-06: Timeline / Roadmap View

**As an** executive viewer, **I want** to see a 3D horizontal timeline showing milestones, releases, and sprint boundaries, **so that** I can understand the project's past progress and upcoming trajectory.

*References: Section 5.6 — Timeline / Roadmap*

- [ ] Renders a 3D horizontal timeline (TubeGeometry or equivalent path)
- [ ] Shows milestone markers along the path
- [ ] Visual distinction between completed phases, active phase, and upcoming phases
- [ ] Minimum 6 roadmap milestones displayed
- [ ] Sprint boundaries are visually indicated
- [ ] Data is fetched from `GET /api/roadmap`

### US-07: Report Detail Panel

**As a** user, **I want** to click on any card, node, or 3D object and see a slide-in detail panel with full item information, **so that** I can drill into specifics without losing context of the overall dashboard.

*References: Section 5.7 — Report Detail Panel*

- [ ] Panel slides in smoothly from the side when an item is clicked
- [ ] Displays: title, description, owner, status, priority, estimate, remaining work, dependencies, and recent activity
- [ ] Panel closes smoothly with a dismiss action
- [ ] Data is fetched from `GET /api/report/:id`
- [ ] Transitions are smooth and polished (no jarring cuts)

### US-08: Team Activity Feed

**As an** engineering manager, **I want** to see a scrollable feed of recent team activity including PR completions, task updates, and contributor actions, **so that** I can monitor team momentum in real time.

*References: Section 5.5 — Team Activity*

- [ ] Displays a scrollable list of recent activity events
- [ ] Includes mock pull request, task completion, and contributor events
- [ ] Shows animated activity pulses for new events
- [ ] Entry transitions are animated (fade-in or slide-in)
- [ ] Minimum 20 activity feed events
- [ ] Minimum 10 team members represented
- [ ] Data is fetched from `GET /api/team-activity`

### US-09: Backend API

**As a** frontend developer, **I want** a Node.js/Express backend serving all required mock data through well-defined REST endpoints, **so that** the frontend can be developed and tested against a realistic API contract.

- [ ] Express server starts on port 3001 (or configurable)
- [ ] All 7 endpoints return valid JSON with appropriate HTTP status codes:
  - `GET /api/project-summary`
  - `GET /api/project-items`
  - `GET /api/sprint-metrics`
  - `GET /api/risks`
  - `GET /api/team-activity`
  - `GET /api/roadmap`
  - `GET /api/report/:id`
- [ ] `/api/report/:id` returns 404 for unknown IDs
- [ ] Mock data meets minimum volume requirements (see US-03, US-05, US-06, US-08)
- [ ] CORS is configured for local development

### US-10: Developer Setup & Documentation

**As a** developer, **I want** to clone the repo, run `npm install && npm run dev`, and have the full application running locally, **so that** I can get started with zero friction.

- [ ] Single `npm install` at root installs all dependencies
- [ ] Single `npm run dev` starts both frontend and backend concurrently
- [ ] README includes: setup instructions, architecture overview, folder structure explanation
- [ ] README includes notes on how to replace mock data with real project APIs
- [ ] README includes explanation of major design decisions
- [ ] Code includes inline comments explaining major sections
- [ ] Mock data is easily customizable (clearly structured JSON/TypeScript files)

## Scope

### In Scope

- Full-stack web application with Node.js/Express backend and Three.js/React frontend
- 7 REST API endpoints serving typed mock JSON data
- 7 dashboard visualization sections: Project Overview, 3D Hierarchy, Sprint Metrics, Risk Radar, Timeline, Team Activity, Detail Panel
- Dark-mode futuristic visual design with glassmorphism, bloom, particles, and neon accents
- Interactive 3D scene with camera fly-in, click-to-focus, hover glow, and orbit controls
- 2D chart overlays (burndown, velocity) using Chart.js
- Animated counters, progress rings, and floating motion effects
- Responsive desktop browser layout
- Loading states and graceful error handling for all API calls
- Complete README with setup instructions, architecture notes, and mock data replacement guide
- Single-command local development setup (`npm install && npm run dev`)
- TypeScript throughout (frontend and backend)
- Monorepo structure with Vite for frontend build tooling

### Out of Scope

- **Authentication & authorization** — No login, roles, or access control
- **Real data integration** — No connections to Azure DevOps, Jira, or any external project management API
- **Database** — No persistent storage; all data is in-memory mock JSON
- **Mobile/tablet responsive design** — Desktop browser only
- **Server-side rendering (SSR)** — Client-side rendering only
- **Automated CI/CD pipeline** — No GitHub Actions workflows required for MVP
- **Docker containerization** — Optional; not required for delivery
- **Hosted deployment** — Local-only execution; no cloud hosting required
- **Accessibility/WCAG compliance** — Not targeted for this demo-focused deliverable
- **Internationalization (i18n)** — English only
- **Unit/integration/E2E test suites** — Not required for MVP delivery
- **Real-time data updates / WebSocket connections** — Static data fetched on load
- **User preferences or settings persistence** — No local storage or cookies
- **Print or export functionality** — No PDF/image export of dashboard views

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Initial load to interactive | ≤ 5 seconds on modern hardware (discrete GPU, Chrome) |
| Frame rate during normal interaction | ≥ 60 fps on discrete GPU; ≥ 30 fps on integrated GPU |
| Camera fly-in animation | Smooth at ≥ 30 fps, completes in 2–4 seconds |
| API response time (mock data) | < 50ms per endpoint |
| Frontend bundle size (gzipped) | < 2 MB (excluding Three.js textures) |
| WebGL draw calls per frame | < 100 (use instanced meshes for particles and nodes) |

### Browser Compatibility

- **Primary:** Google Chrome (latest), Microsoft Edge (latest)
- **Secondary:** Mozilla Firefox (latest), Safari (latest)
- **Fallback:** Display a user-friendly message if WebGL2 is not supported

### Code Quality

- TypeScript strict mode enabled for both frontend and backend
- Modular component-based architecture (one component per file)
- Inline comments on major sections and non-obvious logic
- Consistent code formatting (Prettier) and linting (ESLint)

### Security

- No authentication required (mock data only)
- If extended to real data in the future: add API key auth, HTTPS, CORS lockdown, and input validation on `:id` parameters
- No sensitive data in the codebase

### Reliability

- Application must start without errors via `npm install && npm run dev`
- Frontend must handle backend unavailability gracefully (show error states, not crash)
- 3D scene must not freeze or crash the browser tab under normal interaction

## Success Metrics

| # | Metric | Target | Measurement Method |
|---|--------|--------|--------------------|
| 1 | **Zero-friction setup** | Clone → running app in < 2 minutes | Manual test: `git clone`, `npm install`, `npm run dev` |
| 2 | **All 7 API endpoints functional** | 7/7 endpoints return valid JSON | Curl or browser test against each endpoint |
| 3 | **All 7 dashboard sections rendered** | All sections visible and populated with data | Visual inspection of running application |
| 4 | **3D scene performance** | ≥ 30 fps on integrated GPU hardware | Chrome DevTools Performance tab measurement |
| 5 | **Interactive elements working** | Click-to-focus, detail panel open/close, hover effects all functional | Manual interaction test |
| 6 | **Visual quality bar** | Dashboard looks "premium futuristic product demo" quality | Stakeholder review sign-off |
| 7 | **Mock data completeness** | Meets all minimum volume requirements (4 epics, 12 features, 40+ stories, 8 risks, 10 team members, 20 activities, 6 milestones) | Count entities in mock data files |
| 8 | **Documentation complete** | README covers setup, architecture, design decisions, and mock data replacement | Review README against checklist |
| 9 | **Cross-browser basic function** | App loads and renders in Chrome, Edge, Firefox, Safari | Manual test on each browser |
| 10 | **Camera fly-in plays on load** | Cinematic entry animation plays automatically | Visual verification |

## Constraints & Assumptions

### Technical Constraints

- **Three.js is mandatory** for 3D/WebGL rendering per the feature specification
- **Node.js/Express is mandatory** for the backend per the feature specification
- **Mock data only** — no database, no external API integrations, no authentication
- **Single-command startup** — the entire application must launch via `npm run dev`
- **Desktop browser only** — no mobile or tablet layout required
- **WebGL2 required** — the application will not function in browsers without WebGL2 support
- **GSAP licensing** — if used for animations, must verify license compatibility; `@react-spring/three` (MIT) is a fallback alternative

### Timeline Assumptions

- **Phase 1 (Weeks 1–2):** Project scaffold, all 7 API endpoints, basic 3D scene, Project Overview and Sprint Metrics sections
- **Phase 2 (Weeks 2–3):** 3D Hierarchy View, Health Rings, Bar Towers, Risk Radar — the core 3D visualizations
- **Phase 3 (Weeks 3–4):** Timeline/Roadmap, Team Activity Feed, Detail Panel, camera fly-in, cross-browser testing, performance optimization, documentation
- Total estimated duration: **4 weeks** for a developer experienced with React and Three.js; add 2–3 weeks ramp-up if the team has no prior Three.js/R3F experience

### Dependency Assumptions

- Development machines have a GPU capable of WebGL2 rendering (discrete or recent integrated)
- Node.js 22 LTS is installed on development machines
- No external services, APIs, or accounts are required to develop or run the application
- The Vite dev server proxy will handle cross-origin requests between frontend (port 5173) and backend (port 3001) during development
- All third-party libraries listed in the technology stack are available via npm and have compatible open-source licenses (note: GSAP requires license review for commercial use)

### Data Assumptions

- Mock data is static and deterministic (same data on every server start)
- Data volumes are small (< 100 total entities) — no pagination or infinite scroll required
- The `/api/report/:id` endpoint can look up any entity type (epic, feature, story, risk) by a universal ID