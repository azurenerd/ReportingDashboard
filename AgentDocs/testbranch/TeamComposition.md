# Team Composition

**Project:** A local-only dashboard tool that visualizes Azure DevOps work-item data as a Gantt timeline with milestone markers and a color-coded heatmap grid, powered by ASP.NET Core 8 + TypeScript + D3.js.

## Rationale
This project has two distinct technical domains—a pixel-perfect SVG/CSS frontend requiring D3.js expertise and a .NET backend with ADO REST API integration—that benefit from dedicated specialists. The built-in Architect and TestEngineer cover system design and quality assurance, while two SME engineers handle the frontend visualization and backend data pipeline respectively, enabling parallel Phase 1-2 development.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| ProgramManager | 1 | PMSpec is already created but needs ongoing coordination across 4 phases, user story decomposition into engineering tasks, and PR reviews to verify acceptance criteria compliance against the visual design reference. |
| Architect | 1 | Needs to produce the architecture document defining the API contract (RoadmapData JSON shape), EF Core entity model, Vite-to-wwwroot build pipeline integration, and the frontend component boundaries (header.ts, timeline.ts, heatmap.ts). Critical for ensuring the sample-data.json contract works as both the Phase 1 fixture and the Phase 2 API response schema. |
| SoftwareEngineer | 1 | Lead SE to own engineering plan creation, task decomposition across the 4 phases, code review of specialist PRs, and direct implementation of cross-cutting concerns (project scaffolding, MSBuild targets, CI pipeline, Serilog configuration). |
| TestEngineer | 1 | The spec requires >70% coverage on both backend (xUnit) and frontend (Vitest), plus specific test scenarios for ADO state mapping, API endpoint validation, and SVG rendering verification. Test strategy must cover offline operation, sync failure modes, and visual regression. |

## Specialist Engineers & SME Agents
### Frontend Visualization Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, typescript, d3js, svg, css-grid, data-visualization, vite, html, css

### Backend Integration Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** backend, dotnet, aspnet-core, efcore, sqlite, azure-devops-api, rest-api, csharp

---
_Generated at 2026-05-01 06:16:15 UTC_
