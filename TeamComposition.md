# Team Composition

**Project:** A single-page Blazor Server dashboard that reads project status from a JSON file and renders a pixel-perfect 1920x1080 executive reporting view with milestone timelines and status heatmaps.

## Rationale
This is a small, well-scoped project (1-2 day effort) with a clear reference design. The built-in Architect and SoftwareEngineer can handle the Blazor Server setup, data modeling, and component structure. One Frontend Engineer SME is justified because the project's core value is pixel-perfect CSS Grid/Flexbox layout and hand-crafted SVG generation — skills that require specialist attention to match the reference design exactly for screenshot fidelity.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to define the component hierarchy (Dashboard → Header/Timeline/Heatmap), data model design (DashboardData, MilestoneStream, StatusCategory POCOs), service layer (DashboardDataService singleton), and JSON schema. This is a simple architecture but getting the component boundaries and data flow right upfront prevents rework. |
| SoftwareEngineer | 1 | Needed to lead implementation: scaffold the Blazor Server project, build C# model classes, implement DashboardDataService, create the Razor components, wire up dependency injection, and create the sample data.json with fictional project data. Also handles build verification and integration. |
| TestEngineer | 1 | Needed to verify the dashboard renders correctly with various data.json configurations — empty states, missing fields, variable item counts, edge cases like no blockers or maximum milestone streams. Also validates screenshot fidelity at 1920x1080 and cross-browser consistency between Edge and Chrome. |

## Specialist Engineers & SME Agents
### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, css, svg, blazor, razor, ui-design, css-grid, flexbox, data-visualization

---
_Generated at 2026-04-17 10:57:23 UTC_
