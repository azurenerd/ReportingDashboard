# Team Composition

**Project:** A single-page Blazor Server dashboard that renders project milestones and execution status from a JSON file, optimized for 1920x1080 PowerPoint screenshot capture.

## Rationale
This is a small, well-scoped project with a clear design reference and no external dependencies. The built-in Architect and SoftwareEngineer can handle the Blazor Server setup, data modeling, and component structure. A single Blazor/UI specialist is warranted because the project's core challenge is pixel-perfect SVG timeline rendering and CSS Grid heatmap reproduction from an HTML design reference — skills that require focused front-end and SVG expertise beyond general-purpose engineering.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to define the Blazor Server component hierarchy, data model records for data.json, DashboardDataService design, and SVG coordinate calculation strategy. The architecture is simple but must be defined cleanly to avoid over-engineering. |
| SoftwareEngineer | 1 | Lead engineer to scaffold the Blazor Server project, implement Program.cs, data models, DashboardDataService, and coordinate the overall build. Will handle backend data flow and project structure while the UI specialist focuses on visual components. |
| TestEngineer | 1 | Needed to verify visual fidelity against the design reference at 1920x1080, validate data.json schema handling, test error states (missing/malformed JSON), and confirm the screenshot capture workflow produces clean results. |

## Specialist Engineers & SME Agents
### Blazor UI Developer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, blazor, css, svg, razor-components, css-grid, flexbox, ui-layout

---
_Generated at 2026-04-17 03:40:48 UTC_
