# Team Composition

**Project:** A single-page Blazor Server dashboard that reads project data from a JSON file and renders a pixel-perfect executive reporting view with milestone timeline and monthly execution heatmap, optimized for 1920x1080 PowerPoint screenshots.

## Rationale
This is a small, well-scoped project with a clear design reference and minimal backend complexity. The built-in Architect and SoftwareEngineer can handle the Blazor Server scaffolding, data models, and component architecture. A Frontend Engineer specialist is warranted because the project's primary success metric is pixel-perfect visual fidelity—requiring deep expertise in inline SVG rendering, CSS Grid layout, and precise color/spacing matching that goes beyond typical full-stack engineering.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to define the Blazor Server project structure, C# record data models for the JSON schema, component architecture (Dashboard page composing Header, Timeline, and Heatmap sub-components), and the DashboardDataService singleton pattern. The architecture is simple but must be defined cleanly to keep the project under 15 files. |
| SoftwareEngineer | 1 | Needed as the lead engineer to coordinate implementation, build the data models, DashboardDataService, Program.cs setup, error handling, and query parameter support for multiple data files. Will delegate visual implementation tasks to the Frontend Engineer specialist. |
| TestEngineer | 1 | While unit tests are out of scope for MVP, the TestEngineer is needed to verify visual fidelity against the design reference at 1920x1080, validate error handling scenarios (missing/malformed data.json), and confirm the query parameter switching works correctly. Manual QA is the primary testing mode for this screenshot-oriented project. |

## Specialist Engineers & SME Agents
### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, css, svg, blazor, razor, ui, visualization, layout

---
_Generated at 2026-04-17 11:39:52 UTC_
