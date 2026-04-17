# Team Composition

**Project:** A single-page Blazor Server dashboard that reads project data from a JSON file and renders a pixel-perfect executive reporting view with milestone timeline and status heatmap, optimized for PowerPoint screenshots.

## Rationale
This is a small, well-scoped project (~15 files, 3-4 dev-days) where the core challenge is faithful CSS/SVG reproduction of an existing HTML design in Blazor. The built-in Architect and SoftwareEngineer can handle the Blazor component architecture, data modeling, and implementation. A Frontend Engineer SME is warranted because the project's primary success metric is pixel-perfect visual fidelity — requiring deep expertise in CSS Grid, Flexbox, inline SVG coordinate math, and design-to-code translation that goes beyond typical backend-oriented .NET development.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to design the Blazor component hierarchy (Header, Timeline, HeatmapGrid, HeatmapCell), define the data.json schema and C# record models, establish the CSS architecture with custom properties, and ensure the SVG timeline rendering strategy is sound. The architecture is simple but the component decomposition and data flow design are critical to get right upfront. |
| SoftwareEngineer | 1 | Needed as the lead engineer to create the engineering plan, scaffold the Blazor Server project, implement the ProjectDataService, wire up data binding, create the example data.json, and coordinate implementation across components. Handles the C# backend work (JSON deserialization, service registration, data flow) and oversees the overall solution structure. |
| TestEngineer | 1 | Needed to verify visual fidelity against the design reference, validate JSON schema handling (missing fields, malformed JSON), test edge cases (minimal data, maximum data, empty categories), and ensure the dashboard renders correctly at 1920x1080 without overflow or scrollbars. Even though the project is small, the acceptance criteria are highly specific and measurable. |

## Specialist Engineers & SME Agents
### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, blazor, css, svg, css-grid, flexbox, ui-design, razor-components

---
_Generated at 2026-04-17 09:13:43 UTC_
