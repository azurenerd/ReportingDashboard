# Team Composition

**Project:** A single-page Blazor Server dashboard that reads project data from a JSON file and renders a pixel-perfect 1920x1080 executive reporting view with an SVG milestone timeline and color-coded heatmap grid.

## Rationale
This is a small, well-scoped project with a single Blazor page, one JSON data source, and no backend complexity. The built-in Architect and SoftwareEngineer can handle the data model, service layer, and Razor components. A Frontend Engineer SME is justified because the project's #1 success criterion is pixel-perfect visual fidelity — matching an exact HTML/CSS design with precise color codes, SVG rendering, CSS Grid layout, and screenshot-optimized viewport — which requires specialist CSS/SVG expertise beyond typical full-stack .NET development.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to design the data model (DashboardData.cs), define the JSON schema for data.json, structure the Blazor component hierarchy (Dashboard.razor, Timeline.razor, Heatmap.razor), and establish the DashboardDataService singleton pattern. Simple project but the architecture decisions around component boundaries and data flow need to be right from the start. |
| SoftwareEngineer | 1 | Needed to implement the C# backend: data model classes, DashboardDataService, Program.cs setup, JSON deserialization with System.Text.Json, error handling for missing/malformed data.json, and the overall Blazor project scaffolding. Also handles the data.sample.json template creation. |
| TestEngineer | 1 | Needed to verify JSON deserialization correctness, date-to-pixel math accuracy, error handling for missing/malformed data.json, and visual rendering at 1920x1080. The screenshot fidelity requirement makes testing critical — must validate that the rendered output matches the design reference. |

## Specialist Engineers & SME Agents
### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, css, svg, blazor, razor, css-grid, flexbox, responsive-layout, visual-fidelity

---
_Generated at 2026-04-17 14:25:12 UTC_
