# Team Composition

**Project:** A single-page Blazor Server dashboard that reads project data from a JSON file and renders a pixel-perfect 1920x1080 executive reporting view with milestone timeline and monthly execution heatmap, optimized for PowerPoint screenshot capture.

## Rationale
This is a small, well-scoped project (~5-8 files) with no backend complexity, no auth, and no database. A lean team of Architect, PrincipalEngineer, and SeniorEngineer covers design, planning, and implementation. The JuniorEngineer handles sample data and documentation. No SME agents are needed since this is standard Blazor/CSS/SVG work with no specialized domain expertise required.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to define the data model schema (data.json structure, C# record types), component layout, SVG timeline rendering strategy, and CSS architecture ported from the HTML reference design. Critical for ensuring the single-page architecture stays simple. |
| PrincipalEngineer | 1 | Needed to create the engineering plan, decompose the PM spec into implementation tasks, review code quality, and ensure the SVG date-to-pixel calculations and CSS Grid layout match the reference design exactly. |
| SeniorEngineer | 1 | Primary implementer for the Blazor Server application: Dashboard.razor component, DashboardDataService, inline SVG timeline rendering, CSS Grid heatmap, and CSS isolation. Handles the core feature implementation. |
| JuniorEngineer | 1 | Handles creating the sample data.json with fictional project data, writing the data model classes, porting CSS from the HTML reference into Dashboard.razor.css, and documenting the screenshot capture procedure. |

---
_Generated at 2026-04-16 16:54:02 UTC_
