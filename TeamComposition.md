# Team Composition

**Project:** A single-page Blazor Server dashboard that reads data.json to render an executive-ready project milestone timeline and status heatmap, optimized for 1920x1080 PowerPoint screenshots.

## Rationale
This is a straightforward single-page Blazor app with well-defined visual specs, requiring no specialized domain expertise. The core built-in team of Architect, SoftwareEngineer, and TestEngineer covers system design, implementation, and validation. Research is already complete and the PM spec is finalized, so those roles are not needed for active development. No SME agents are warranted given the project's simplicity — no auth, no database, no cloud services, no specialized domains.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to design the Blazor component hierarchy, data model (C# records for data.json), CSS architecture with custom properties, and SVG timeline rendering approach. Must translate the OriginalDesignConcept.html into a clean component-based architecture. |
| SoftwareEngineer | 1 | Primary implementer responsible for scaffolding the Blazor Server project, building Dashboard.razor and child components (Header, Timeline SVG, Heatmap Grid), porting CSS from the HTML design, creating the DashboardDataService, implementing DateToX() linear interpolation, and populating data.json with fictional project data. |
| TestEngineer | 1 | Validates visual fidelity against the OriginalDesignConcept.html reference, tests JSON deserialization with valid/invalid data, verifies the page renders at exactly 1920x1080 with no scrollbars, and confirms dynamic data rendering (variable stream counts, column counts, empty states). |

---
_Generated at 2026-04-16 20:40:25 UTC_
