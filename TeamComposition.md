# Team Composition

**Project:** Single-page Blazor Server dashboard that renders project milestones and execution status from a JSON file, optimized for 1920x1080 PowerPoint screenshots.

## Rationale
This is a straightforward single-page Blazor app with no auth, no database, and zero external dependencies — the built-in Architect and SoftwareEngineer can handle the design-to-code translation, data modeling, and CSS/SVG implementation. No SME agents are needed since the project involves standard web UI development with well-understood technologies (CSS Grid, inline SVG, System.Text.Json) and the reference HTML design already provides the exact visual specification.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to define the Blazor component architecture, data model for data.json, service layer design, and CSS strategy. Will translate the reference HTML design into a clean component hierarchy (Header, Timeline, HeatmapGrid) and establish the SVG coordinate calculation approach. |
| SoftwareEngineer | 1 | Primary implementer — will scaffold the Blazor Server project, build Razor components matching the reference design pixel-for-pixel, implement the DashboardDataService for JSON loading, hand-code the inline SVG timeline, and adapt the CSS from OriginalDesignConcept.html. This is the core of the project. |

---
_Generated at 2026-04-16 21:21:25 UTC_
