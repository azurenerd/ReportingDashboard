# Team Composition

**Project:** A single-page Blazor Server dashboard that reads data.json to render an executive-friendly project status view with SVG milestone timeline and color-coded heatmap grid, designed for 1920x1080 PowerPoint screenshots.

## Rationale
This is a straightforward single-page Blazor Server app with no external dependencies, no database, and no auth. The built-in agents can handle the full lifecycle: architecture for the simple component structure, a senior engineer to implement the Blazor components/CSS/SVG, and a test engineer for visual verification. No SME agents are needed since this is standard web development with no specialized domain expertise required.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| ProgramManager | 1 | Already produced the PMSpec. Needed for coordination, tracking acceptance criteria across the 6 user stories, and final review of the delivered dashboard against the design reference. |
| Architect | 1 | Define the Blazor component hierarchy (Header, Timeline, HeatmapGrid), data model POCOs matching the data.json schema, DashboardDataService design, and CSS architecture. Simple project but the SVG coordinate system and data flow need a clear blueprint before coding. |
| PrincipalEngineer | 1 | Create the engineering plan with task decomposition, establish the project scaffold (dotnet new blazorserver, remove boilerplate), and review the senior engineer's implementation for pixel-perfect fidelity to the HTML design reference. |
| SeniorEngineer | 1 | Primary implementer. Build all Blazor components (Header.razor, Timeline.razor, HeatmapGrid.razor, Dashboard.razor), port CSS verbatim from OriginalDesignConcept.html, implement the SVG timeline with dynamic date-to-pixel mapping, create the data.json with fictional project data, and wire up DashboardDataService. |
| TestEngineer | 1 | Verify the dashboard renders correctly at 1920x1080, validate that all data.json fields render in the correct locations, test error handling for malformed/missing JSON, and confirm no Blazor boilerplate leaks through. Screenshot fidelity is the primary quality gate. |

---
_Generated at 2026-04-16 18:09:13 UTC_
