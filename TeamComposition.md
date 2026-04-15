# Team Composition

**Project:** A single-page Blazor Server dashboard that reads data.json to render a project milestone timeline and execution heatmap, optimized for 1920x1080 PowerPoint screenshots.

## Rationale
This is a straightforward single-page Blazor Server app with vanilla CSS/SVG and no external dependencies, so a lean team is optimal. An Architect defines the component structure and data model, a PrincipalEngineer creates the engineering plan and ensures quality, a SeniorEngineer handles the core Blazor/CSS/SVG implementation, a JuniorEngineer handles the sample data and documentation, and a TestEngineer validates screenshot fidelity and data service correctness. No SME agents are needed since the project uses standard .NET, CSS Grid, and inline SVG with no specialized domain expertise required.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Defines the Blazor component decomposition (Header, TimelineSvg, HeatmapGrid, HeatmapRow), data model records (DashboardData, Milestone, StatusCategory), CSS strategy porting from the HTML design, and SVG coordinate computation approach. Critical for translating the HTML reference into a clean Blazor architecture. |
| PrincipalEngineer | 1 | Creates the engineering plan with task breakdown, reviews the CSS port for pixel-perfect fidelity against OriginalDesignConcept.html, ensures SVG timeline positioning math is correct, and validates that the final output matches the 1920x1080 design reference. |
| SeniorEngineer | 1 | Implements the core deliverables: Blazor project scaffolding, DashboardDataService for JSON deserialization, Dashboard.razor page with child components, dashboard.css ported from the HTML design, and inline SVG timeline generation with computed marker positions. |
| JuniorEngineer | 1 | Creates the fictional Project Phoenix data.json with realistic sample data, writes the README.md with setup and screenshot capture instructions, handles appsettings.json configuration, and assists with CSS fine-tuning and edge case data files. |
| TestEngineer | 1 | Implements xUnit tests for DashboardDataService (valid data, missing file, malformed JSON, empty arrays), bUnit component tests for key Razor components, and validates 1920x1080 rendering consistency across Edge and Chrome. |

---
_Generated at 2026-04-15 10:13:41 UTC_
