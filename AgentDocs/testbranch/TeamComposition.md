# Team Composition

**Project:** Internal React dashboard visualizing a Privacy Automation Release Roadmap with an SVG Gantt timeline and CSS Grid heatmap, sourced from Azure DevOps.

## Rationale
This project demands pixel-perfect SVG/CSS visualization work (D3 scales, custom SVG milestones, CSS Grid heatmap) that is the core deliverable and primary acceptance criterion — a frontend visualization specialist will dramatically improve quality and velocity. The backend is a thin Azure Functions proxy with MSAL auth, well within the built-in team's capabilities, so no backend SME is needed.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| ProgramManager | 1 | PMSpec is already drafted but contains open questions. PM coordinates phased delivery (static MVP → live data → polish) and manages stakeholder acceptance of visual fidelity. |
| Architect | 1 | Designs the component architecture, data flow (ADO API → Azure Function → React Query → components), TypeScript type system (RoadmapData, Milestone, StatusRow), and Azure Static Web Apps deployment configuration. |
| SoftwareEngineer | 1 | Leads engineering plan, decomposes user stories into tasks, implements the Azure Function API proxy, MSAL auth integration, React Query data layer, and coordinates with the frontend specialist. |
| TestEngineer | 1 | Playwright visual regression testing is the primary quality gate (0.1% diff threshold against OriginalDesignConcept.png at 1920x1080). Also covers Vitest unit tests, React Testing Library component tests, and CI pipeline setup. |

## Specialist Engineers & SME Agents
### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, react, typescript, css, svg, d3, visualization, css-grid, vite

---
_Generated at 2026-05-01 08:29:41 UTC_
