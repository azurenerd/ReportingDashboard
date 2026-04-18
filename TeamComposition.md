# Team Composition

**Project:** A single-page Blazor Server executive reporting dashboard that reads data.json and renders a 1920x1080-optimized SVG milestone timeline and color-coded heatmap grid for screenshot-ready PowerPoint slides.

## Rationale
The project is primarily a frontend/UI rendering challenge with precise pixel-fidelity requirements against an HTML design reference — the critical skill gap is advanced CSS Grid, inline SVG math, and Blazor Razor component layout, which warrants a dedicated Frontend Engineer SME. The built-in SE, Architect, PM, and TE cover the .NET 8 scaffolding, data modeling, and test strategy needs adequately. No infrastructure, cloud, or database specialists are needed given the zero-dependency local-only architecture.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| ProgramManager | 1 | Owns the PMSpec, tracks user story completion against acceptance criteria, and coordinates handoffs between Architect, SE, and TE. |
| Architect | 1 | Defines the DashboardDataService singleton pattern, data.json schema, C# model classes (DashboardConfig, Milestone, HeatmapRow, HeatmapCell), and the single-page component architecture. |
| SoftwareEngineer | 1 | Leads engineering plan, decomposes tasks, implements .NET 8 project scaffolding, DashboardDataService, startup validation, error handling, and coordinates the Frontend Engineer SME. |
| TestEngineer | 1 | Defines bUnit test strategy covering data deserialization, heatmap cell rendering, milestone label rendering, error state handling, and empty cell fallback behavior. |

## Specialist Engineers & SME Agents
### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, blazor, razor, css, css-grid, flexbox, svg, dotnet, html

---
_Generated at 2026-04-18 03:11:48 UTC_
