# Team Composition

**Project:** A single-page Blazor Server dashboard that reads project data from a JSON file and renders an executive-friendly milestone timeline and color-coded heatmap for screenshot capture into PowerPoint decks.

## Rationale
This is a small, well-defined project with a clear HTML reference design, a single data source (JSON file), and minimal backend complexity. The built-in Architect and SoftwareEngineer can handle the Blazor component structure, data model, and CSS/SVG porting. One Frontend Engineer SME is warranted because pixel-perfect CSS Grid/Flexbox layout, inline SVG timeline rendering with date-to-pixel math, and screenshot-fidelity visual matching require specialist UI skills beyond typical full-stack .NET work.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to define the Blazor component hierarchy, data model (C# record types for DashboardData, TimelineConfig, HeatmapConfig), service layer design (DashboardDataService singleton), and JSON schema. The architecture is simple but must be defined correctly upfront to avoid over-engineering. |
| SoftwareEngineer | 1 | Needed as technical lead to coordinate implementation, build the Blazor project scaffold, implement the DashboardDataService, create the data model classes, wire up dependency injection, and integrate all components. Will delegate pixel-perfect CSS/SVG work to the Frontend Engineer SME. |
| TestEngineer | 1 | Needed to write unit tests for JSON deserialization, data model validation, and bUnit component tests to verify SVG output and grid rendering. Ensures the dashboard renders correctly with sample data and handles edge cases (missing JSON, empty arrays, varying track counts). |

## Specialist Engineers & SME Agents
### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, css, svg, blazor, razor, ui, layout, design-implementation

---
_Generated at 2026-04-17 09:49:52 UTC_
