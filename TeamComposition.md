# Team Composition

**Project:** A single-page Blazor Server dashboard that reads data.json and renders a pixel-perfect 1920x1080 executive reporting view with milestone timeline and execution heatmap, optimized for PowerPoint screenshot capture.

## Rationale
This is a low-complexity, high-polish project where the primary challenge is CSS fidelity to a design mockup, not architecture. The built-in Architect and SoftwareEngineer can handle the minimal Blazor scaffolding and data model, but a Frontend Engineer specialist is warranted because pixel-perfect CSS Grid/Flexbox layout, inline SVG rendering, and screenshot optimization require deep visual implementation expertise that generic backend-oriented engineers often lack. No TestEngineer is needed for MVP since testing is explicitly out of scope.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to define the minimal Blazor Server project structure, data model (DashboardData.cs records), DashboardDataService singleton pattern, and JSON schema. The architecture is simple but must be defined correctly upfront to keep the project under 10 files. |
| SoftwareEngineer | 1 | Needed as the lead engineer to coordinate implementation, build the C# backend (data model, JSON deserialization service, Program.cs setup), and integrate the frontend Blazor component with the data layer. |

## Specialist Engineers & SME Agents
### Frontend Engineer
- **Type:** Specialist Engineer (full engineering capabilities)
- **Tier:** standard
- **Mode:** Continuous
- **Capabilities:** frontend, css, svg, blazor, ui-implementation, dashboard, css-grid, flexbox

---
_Generated at 2026-04-17 14:49:23 UTC_
