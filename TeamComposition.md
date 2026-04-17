# Team Composition

**Project:** A single-page Blazor Server dashboard that reads project data from a JSON file and renders a pixel-perfect 1920x1080 executive reporting view with milestone timeline and status heatmap, optimized for PowerPoint screenshot capture.

## Rationale
This is a small, well-scoped utility project (~5-8 files) that primarily involves porting an existing HTML design into Blazor with data binding. The built-in Architect and SoftwareEngineer can handle the Blazor component structure, data models, and CSS porting. One Frontend Engineer SME is justified because the project's core success criterion is pixel-perfect visual fidelity — precise SVG timeline rendering, CSS Grid heatmap layout, and fixed-viewport screenshot optimization require specialist-level CSS/SVG expertise that generic backend-oriented engineers may lack.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to design the minimal Blazor component structure (Dashboard, Header, Timeline, Heatmap), define the data.json schema and C# POCO models, and establish the DashboardDataService pattern. The architect ensures the project stays simple and avoids over-engineering. |
| SoftwareEngineer | 1 | Lead engineer to implement the Blazor Server app, data models, DashboardDataService, Razor components with data binding, and coordinate with the Frontend Engineer on CSS/SVG integration. Handles Program.cs setup, JSON deserialization, and overall project structure. |

## Specialist Engineers & SME Agents
### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, css, svg, blazor, razor, ui-implementation, css-grid, flexbox, data-visualization

---
_Generated at 2026-04-17 06:32:33 UTC_
