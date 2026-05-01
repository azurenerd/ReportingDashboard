# Team Composition

**Project:** A futuristic 3D animated project management dashboard using Three.js/WebGL and Node.js/Express with mock data, targeting executive demos.

## Rationale
This project's core challenge is building complex 3D WebGL visualizations (force-directed graphs, animated torus rings, particle systems, bloom effects) with React Three Fiber—a niche skill set that generic engineers rarely possess. A dedicated 3D Graphics Engineer is essential to deliver the premium visual quality bar, while a Frontend Engineer handles the 2D UI layer (glassmorphism cards, Chart.js charts, detail panels). The backend is trivial (7 endpoints returning static JSON) and can be handled by the lead Software Engineer.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| ProgramManager | 1 | Coordinates delivery across 3 phases, manages scope against the prescriptive spec, and ensures the visual quality bar meets executive demo standards. |
| Architect | 1 | Designs the monorepo structure, API contracts for all 7 endpoints, 3D scene graph architecture (Canvas/HtmlOverlay split), and the hybrid 2D+3D rendering pipeline with selective bloom layers. |
| SoftwareEngineer | 1 | Leads engineering execution, builds the Express backend and mock data factory, scaffolds the monorepo with Vite/concurrently, and coordinates work across the two specialist engineers. |
| TestEngineer | 1 | Validates cross-browser WebGL compatibility (Chrome, Edge, Firefox, Safari), measures 60fps performance targets, and verifies all 7 API endpoints return correct mock data volumes. |

## Specialist Engineers & SME Agents
### Graphics Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** premium
- **Mode:** Continuous
- **Capabilities:** threejs, webgl, react-three-fiber, 3d-graphics, shaders, animation, gsap, performance-optimization

### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, react, typescript, tailwindcss, css, chartjs, framer-motion, ui-design, glassmorphism

---
_Generated at 2026-05-01 11:49:02 UTC_
