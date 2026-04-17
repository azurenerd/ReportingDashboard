# Team Composition

**Project:** A single-page Blazor Server dashboard that reads project data from a JSON file and renders an executive-friendly milestone timeline and status heatmap optimized for 1920x1080 PowerPoint screenshots.

## Rationale
This is a small, well-scoped Blazor Server project with no infrastructure complexity — the built-in team of Architect and SoftwareEngineer can handle the data model, Blazor components, inline SVG rendering, and CSS Grid layout. A dedicated Blazor UI/SVG specialist is warranted because the project's core challenge is pixel-perfect visual fidelity to a reference HTML design, requiring deep expertise in inline SVG coordinate math, CSS Grid layout, and Blazor component rendering that goes beyond typical backend-focused .NET engineering.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to design the data model (DashboardData, Milestone, HeatmapCategory), define the data.json schema, plan the component hierarchy (DashboardHeader, TimelineSection, HeatmapGrid, HeatmapCell), and establish the SVG date-to-pixel mapping strategy. This is a small project but the architecture decisions around JSON schema design and component decomposition set the foundation. |
| SoftwareEngineer | 1 | Serves as engineering lead to implement the core Blazor Server application: Program.cs setup, DashboardDataService with System.Text.Json deserialization, FileSystemWatcher for auto-reload, strongly-typed C# models, and overall project scaffolding. Coordinates with the UI specialist on component integration. |
| TestEngineer | 1 | Needed to verify JSON deserialization correctness, date-to-pixel math accuracy, component rendering with bUnit smoke tests, and visual fidelity against the reference design. Also validates error handling for missing/invalid data.json scenarios. |

## Specialist Engineers & SME Agents
### Blazor UI Developer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** blazor, svg, css, frontend, ui, razor

---
_Generated at 2026-04-17 02:47:01 UTC_
