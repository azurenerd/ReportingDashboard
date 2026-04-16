# Team Composition

**Project:** A single-page .NET 8 Blazor Server dashboard that renders project milestones and execution status from a JSON file, optimized for 1920x1080 PowerPoint screenshots.

## Rationale
This is a straightforward single-page Blazor app with no database, no auth, and no complex integrations—well within the built-in team's capabilities. The Architect defines the simple component structure and data model, the PrincipalEngineer plans tasks and ensures pixel-fidelity to the reference design, and a SeniorEngineer handles the bulk of implementation (Razor components, inline SVG timeline, CSS Grid heatmap). No SME agents are needed since there are no specialized domains like security, ML, or compliance involved.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| ProgramManager | 1 | Already produced the PMSpec; continues to coordinate work, validate acceptance criteria, and ensure the final dashboard matches the design reference. |
| Architect | 1 | Defines the Blazor component tree, data model (C# records for data.json), CSS strategy, and SVG timeline calculation approach. Ensures the solution stays within the <15 file complexity budget. |
| PrincipalEngineer | 1 | Creates the engineering plan, decomposes user stories into implementation tasks, reviews code for pixel-fidelity against OriginalDesignConcept.html, and enforces the zero-JS/zero-NuGet constraints. |
| SeniorEngineer | 1 | Primary implementer: builds the Blazor components (Header, Timeline SVG, Heatmap grid), data service, CSS stylesheet, and sample data.json. The SVG timeline with date-to-pixel math is the most complex piece and warrants senior-level skill. |
| JuniorEngineer | 1 | Handles straightforward tasks: scaffold the project, create the sample data.json with fictional data, write the README.md, implement error display components, and extract/organize CSS from the reference HTML. |
| TestEngineer | 1 | Validates screenshot fidelity against the reference design, tests error handling scenarios (missing/malformed data.json), verifies cross-browser rendering consistency, and confirms the data-driven rendering acceptance criteria. |

---
_Generated at 2026-04-16 19:55:03 UTC_
