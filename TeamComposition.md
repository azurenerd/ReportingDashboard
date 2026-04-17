# Team Composition

**Project:** A single-page Blazor Server dashboard that renders project milestones and execution status from a JSON file, optimized for 1920x1080 PowerPoint screenshots.

## Rationale
This is a small, well-scoped project (~5-8 files) with a clear reference design. The built-in Architect and SoftwareEngineer can handle the Blazor scaffolding, data model, and service layer. A Frontend Engineer SME is justified because the core deliverable is pixel-perfect CSS Grid/Flexbox layout and hand-crafted SVG timeline rendering — skills that require specialist attention beyond generic backend-oriented software engineering.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to define the minimal Blazor Server architecture, data model (C# records for DashboardData, Workstream, Milestone, HeatmapData), component decomposition (Header, Timeline, Heatmap), and the DashboardDataService singleton pattern. Lightweight but important for keeping the project simple and well-structured. |
| SoftwareEngineer | 1 | Lead engineer to scaffold the Blazor project, implement Program.cs, DashboardDataService, data models, create the sample data.json, and coordinate with the frontend specialist on component integration. Handles backend plumbing and project structure. |
| TestEngineer | 1 | Needed to verify the dashboard renders correctly at 1920x1080, validate that data.json changes reflect in the UI, test error states (missing/malformed JSON), and confirm cross-browser screenshot fidelity. Even without a formal test project, manual verification against the reference design is critical. |

## Specialist Engineers & SME Agents
### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, css, svg, blazor, razor, css-grid, flexbox, html, ui-layout

---
_Generated at 2026-04-17 07:14:47 UTC_
