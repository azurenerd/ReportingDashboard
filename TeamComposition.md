# Team Composition

**Project:** A single-page Blazor Server dashboard that reads data.json to render a project milestone timeline and monthly execution heatmap, optimized for 1920x1080 PowerPoint screenshots.

## Rationale
This is a small, well-scoped project (~8-12 files) with a clear HTML reference design and no backend complexity. A lean team of Architect, PrincipalEngineer, and SeniorEngineer can handle the data model design, engineering plan, and implementation respectively. No SME agents are needed since the project uses only built-in .NET 8 capabilities with pure CSS/SVG — no specialized domains like security, ML, or compliance are involved.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to define the data model for data.json, design the Blazor component hierarchy (Dashboard, Timeline, Heatmap), and establish the CSS architecture ported from the HTML reference. Critical for getting the SVG coordinate calculation strategy and grid layout right before coding begins. |
| PrincipalEngineer | 1 | Needed to decompose the architecture into an ordered engineering plan with clear tasks, define coding standards for the Razor components and CSS, and review the implementation for visual fidelity against the reference design. |
| SeniorEngineer | 1 | Primary implementer who will build the Blazor Server project, port the CSS from the HTML reference, implement the SVG timeline generation, build the heatmap grid components, create the DashboardDataService with FileSystemWatcher, and author the sample data.json with fictional project data. |

---
_Generated at 2026-04-16 19:21:06 UTC_
