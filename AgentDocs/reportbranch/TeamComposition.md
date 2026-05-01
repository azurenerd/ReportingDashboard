# Team Composition

**Project:** A futuristic 3D animated reporting dashboard built with Three.js, React, and Express that visualizes mock project management data as an immersive WebGL command center for executive demos.

## Rationale
This project demands two rare specialist skill sets beyond standard software engineering: 3D WebGL rendering (Three.js, React Three Fiber, post-processing, force-directed graphs) and advanced frontend visualization (D3.js, Chart.js, GSAP animation choreography, glassmorphism CSS). The built-in Architect is essential for the hybrid 3D+2D overlay architecture and API design, while the SE leads integration. Two SME engineers cover the 3D and visualization gaps that are the project's highest-risk areas.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| ProgramManager | 1 | Coordinates the 5-phase delivery, manages the PMSpec with 9 user stories, and ensures the 'executive demo ready' quality bar is met through stakeholder review. |
| Architect | 1 | Critical for designing the hybrid 3D+2D overlay architecture, R3F Canvas organization, state management pattern (Context+useReducer), API contract for 7 endpoints, and the mock data schema that must support 56+ hierarchical items. |
| SoftwareEngineer | 1 | Leads engineering plan, task decomposition across 5 phases, monorepo setup (Vite+Express+concurrently), Express backend with 7 endpoints, React app shell, and integration of specialist work into a cohesive application. |
| TestEngineer | 1 | Needed for Vitest unit tests on API endpoints and components, Playwright E2E tests verifying 3D renders and animations, and performance validation (30fps target, <5s load time). |

## Specialist Engineers & SME Agents
### Graphics Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** premium
- **Mode:** Continuous
- **Capabilities:** threejs, webgl, react-three-fiber, 3d-rendering, post-processing, force-graph, camera-animation, particle-systems, performance-optimization

### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, react, typescript, css, glassmorphism, chartjs, d3js, gsap, animation, data-visualization, responsive-layout

---
_Generated at 2026-05-01 10:53:24 UTC_
