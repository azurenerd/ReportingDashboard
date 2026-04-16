# Team Composition

**Project:** A single-page Blazor Server dashboard that reads project data from a JSON file and renders an executive-grade milestone timeline and status heatmap optimized for PowerPoint screenshots.

## Rationale
This is a small, well-scoped project (single page, ~10 files, zero external dependencies) that needs strong architecture for the data model/SVG rendering, solid engineering for the Blazor implementation and CSS fidelity, and lightweight testing. No SME agents are needed because the project involves standard web development with no specialized domains like security, ML, or compliance.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| Architect | 1 | Needed to design the data model (DashboardData records), the DashboardDataService singleton pattern, the TimelineCalculator helper for date-to-pixel mapping, and the CSS architecture ported from the original HTML design. Must review the OriginalDesignConcept.html to produce a faithful architecture document. |
| SoftwareEngineer | 1 | Single engineer is sufficient for this small project. Will scaffold the Blazor Server app, implement the Dashboard.razor component with inline SVG timeline and CSS Grid heatmap, build the JSON data service, create the data.json with fictional project data, and port/refine the CSS from the original HTML design. |
| TestEngineer | 1 | Lightweight testing role to verify data.json deserialization works correctly, validate error handling for missing/malformed JSON, and perform visual verification that the rendered dashboard matches the design reference at 1920x1080. Formal automated testing has low ROI here but basic validation is still needed. |

---
_Generated at 2026-04-16 22:25:38 UTC_
