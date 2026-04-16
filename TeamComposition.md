# Team Composition

**Project:** A single-page Blazor Server dashboard that reads project data from a JSON file and renders a milestone timeline with execution heatmap, optimized for PowerPoint screenshot capture at 1920x1080.

## Rationale
This is a straightforward single-page Blazor app with well-defined visual requirements and no complex backend. The Architect defines the component structure and data model, the SoftwareEngineer implements the Blazor components and CSS/SVG rendering, and the TestEngineer validates screenshot fidelity and data-driven rendering. No SME agents are needed since the project uses standard .NET 8 Blazor with built-in CSS Grid, SVG, and System.Text.Json — all well within the built-in team's expertise.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to design the Blazor component hierarchy (Header, Timeline, Heatmap), define the data.json schema and DashboardData model classes, establish the CSS strategy (scoped CSS vs app.css custom properties), and determine the SVG coordinate calculation approach for the timeline. |
| SoftwareEngineer | 1 | Core implementer for the Blazor Server project: scaffolding, Razor components, inline SVG timeline rendering, CSS Grid heatmap, DashboardDataService, data.json deserialization, error handling, and pixel-perfect CSS matching the OriginalDesignConcept.html reference. |
| TestEngineer | 1 | Validates that the rendered dashboard matches the design reference at 1920x1080, tests data.json schema edge cases (empty arrays, missing fields, malformed JSON), verifies error state rendering, and confirms the SVG timeline positions milestones correctly based on date calculations. |

---
_Generated at 2026-04-16 23:40:43 UTC_
