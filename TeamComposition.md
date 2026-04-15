# Team Composition

**Project:** A single-page Blazor Server dashboard that reads data.json to render a screenshot-ready 1920x1080 executive project status view with SVG timeline and color-coded heatmap grid.

## Rationale
This is a small, well-scoped UI project that needs an Architect for the component/data model design, a PrincipalEngineer to plan tasks and ensure quality, and a SeniorEngineer to implement the Blazor components, CSS Grid heatmap, and SVG timeline. No SME agents are needed — the project uses standard .NET/Blazor/CSS/SVG technologies with no specialized domains like security, ML, or compliance.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| ProgramManager | 1 | Already produced the PMSpec. Needed for ongoing coordination, user story tracking, and final acceptance review against the design reference. |
| Architect | 1 | Design the component hierarchy, data model (C# records for DashboardData, Timeline, Heatmap), data.json schema, CSS architecture, and SVG coordinate system. Critical for getting the structure right before coding begins. |
| PrincipalEngineer | 1 | Create the engineering plan with phased task breakdown, review the SeniorEngineer's implementation against the HTML reference design, and ensure pixel-fidelity and code quality standards. |
| SeniorEngineer | 1 | Primary implementer — builds the Blazor Server project, Razor components (DashboardHeader, TimelineSection, HeatmapGrid), CSS Grid layout, inline SVG timeline with date-to-pixel math, DataService for JSON loading, and sample data.json. |
| JuniorEngineer | 1 | Handle straightforward tasks: scaffold the Blazor project, strip the default template, create the data.json sample file, implement simple components like HeatmapCell, and write CSS custom properties. Frees the SeniorEngineer to focus on the complex SVG timeline. |

---
_Generated at 2026-04-15 17:42:32 UTC_
