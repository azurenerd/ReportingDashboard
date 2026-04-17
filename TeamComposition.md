# Team Composition

**Project:** A single-page Blazor Server dashboard that reads project data from a JSON file and renders an SVG milestone timeline with a color-coded heatmap grid, optimized for 1920x1080 PowerPoint screenshots.

## Rationale
This is a small-scope, single-page Blazor application with no backend complexity, no cloud services, and no database. The core challenge is pixel-perfect CSS/SVG rendering matching an HTML design reference, plus clean data modeling for the JSON config. The built-in Architect and SoftwareEngineer can handle the Blazor scaffolding, data model, and service layer, while a Front-End/CSS specialist ensures the visual output matches the design exactly — the #1 quality metric per the PM spec.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to design the data model (DashboardData.cs POCOs), define the data.json schema, establish the component hierarchy (Dashboard.razor → Timeline.razor → Heatmap.razor → HeatmapCell.razor), and make the Blazor Server project structure decisions. Small project but the data model and component decomposition need to be right before coding starts. |
| SoftwareEngineer | 1 | Core implementation lead for the Blazor Server project scaffold, C# data models, DashboardDataService (System.Text.Json deserialization), FileSystemWatcher hot-reload, and wiring data into Razor components. Handles Program.cs, service registration, and overall project structure. |
| TestEngineer | 1 | Needed for basic xUnit tests on data deserialization (valid JSON, malformed JSON, missing file), bUnit component rendering tests, and visual verification that the output matches the design reference at 1920x1080. |

## Specialist Engineers & SME Agents
### Front-End CSS/SVG Developer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, css, svg, blazor, razor, ui-design, css-grid, flexbox

---
_Generated at 2026-04-17 04:09:17 UTC_
