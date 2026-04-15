# Team Composition

**Project:** A single-page Blazor Server dashboard that reads data.json to render an executive-facing project milestone timeline and status heatmap, optimized for 1920x1080 PowerPoint screenshots.

## Rationale
This is a small, well-scoped Blazor Server project with clear design specs and no complex infrastructure. A lean team of Architect, PrincipalEngineer, and SeniorEngineer can handle the data model, component design, SVG timeline rendering, and CSS Grid heatmap. No SME agents are needed since the project uses only built-in .NET technologies with no databases, auth, or specialized domains.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to define the Blazor component hierarchy, C# record data models, DashboardDataService design, and SVG timeline rendering strategy. Must translate the HTML design reference into a clean component architecture. |
| PrincipalEngineer | 1 | Needed to create the engineering plan, decompose user stories into implementation tasks, establish coding standards for the Razor components, and review the SVG date-to-pixel mapping logic and CSS Grid implementation. |
| SeniorEngineer | 2 | Two engineers to parallelize work: one focused on the SVG timeline component (date-to-pixel mapping, milestone rendering, label collision avoidance) and one on the heatmap grid component (CSS Grid, color-coded rows, data binding). Both also handle the shared data model, DashboardDataService, and dashboard.css port from the HTML design. |
| TestEngineer | 1 | Needed to write xUnit tests for JSON deserialization and date-to-pixel calculations, plus bUnit component tests to verify the Blazor components render correctly from data. Ensures the dashboard matches the design spec. |

---
_Generated at 2026-04-15 05:49:17 UTC_
