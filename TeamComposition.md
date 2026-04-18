# Team Composition

**Project:** A single-page Blazor Server (.NET 8) executive reporting dashboard that reads data.json and renders a pixel-faithful 1920x1080 screenshot-optimized view of project milestones, heatmap, and delivery status modeled on OriginalDesignConcept.html.

## Rationale
The project is primarily a frontend-heavy UI fidelity challenge — pixel-perfect SVG timeline rendering, CSS Grid heatmap, and exact color palette matching from a reference HTML design file. A Frontend Engineer SME is justified because the core risk is visual accuracy of inline SVG math and scoped Blazor CSS, which requires deeper CSS/SVG expertise than a generic Software Engineer typically applies. The built-in SE, Architect, and TE cover the Blazor data pipeline, DashboardDataService, and unit testing respectively.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| ProgramManager | 1 | Owns the PMSpec, user story tracking, PR reviews, and acceptance criteria verification against the visual design spec. |
| Architect | 1 | Defines the data.json schema (ProjectConfig, Milestone, HeatmapRow, HeatmapCell POCOs), DashboardDataService singleton pattern, TimelineCalculator helper design, and Blazor project structure. |
| SoftwareEngineer | 1 | Leads engineering plan decomposition, task assignment to the Frontend Engineer SME, DashboardDataService implementation, and overall code quality. |
| TestEngineer | 1 | Owns bUnit + xUnit test strategy for TimelineCalculator date-range edge cases and DashboardDataService deserialization error handling. |

## Specialist Engineers & SME Agents
### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, blazor, dotnet, svg, css, css-grid, razor, html

---
_Generated at 2026-04-18 07:01:00 UTC_
