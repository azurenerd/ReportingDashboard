# Team Composition

**Project:** A single-page Blazor Server dashboard that reads data.json and renders a pixel-perfect 1920x1080 executive reporting view with milestone timeline and status heatmap, optimized for PowerPoint screenshot capture.

## Rationale
This is a straightforward single-page web application with well-defined visual requirements and no complex backend, auth, or infrastructure needs. The built-in Architect and SoftwareEngineer can handle the Blazor project scaffold, data model, SVG timeline, and CSS Grid heatmap. No SME agents are needed since the project uses only standard .NET 8 Blazor with raw HTML/CSS/SVG—no specialized domains like ML, security, or databases are involved.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to design the Blazor component hierarchy (Header, TimelineSvg, HeatmapGrid), define the data.json schema and C# record types, establish the CSS architecture, and ensure the SVG coordinate mapping strategy is sound before implementation begins. |
| SoftwareEngineer | 2 | Two engineers allow parallel workstreams: one focuses on the SVG timeline component (date-to-X mapping, milestone rendering, NOW marker) while the other builds the CSS Grid heatmap component (status rows, month columns, current-month highlighting) and data loading service. This matches the natural division of the two main visual sections. |

---
_Generated at 2026-04-16 21:59:15 UTC_
