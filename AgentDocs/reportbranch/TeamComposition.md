# Team Composition

**Project:** Enhance a Blazor Server executive reporting dashboard with automated screenshot export, JSON validation, component testing, and multi-project support.

## Rationale
This is a focused .NET 8 Blazor Server project with a small, well-defined scope (3 weeks across 3 phases). The built-in team covers architecture, engineering, and testing needs. No SME agents are needed because the work is standard C#/Blazor development, bUnit testing, and Playwright integration — all within a generic Software Engineer's competency.

## Built-in Agents
| Role | Count | Justification |
|------|-------|---------------|
| ProgramManager | 1 | PMSpec is drafted; PM needed for user story decomposition into tasks, PR reviews, and stakeholder coordination on open decisions (multi-project support, PDF export). |
| Architect | 1 | Needed to design the DashboardDataValidator integration, multi-project directory-watching extension to DashboardDataService, and the ScreenshotExporter service/CLI architecture while preserving existing patterns. |
| SoftwareEngineer | 2 | Two engineers enable parallel work: one on Phase 1 (bUnit tests, JSON validator, health endpoint) while the other handles Phase 2 (Playwright screenshot exporter, CI workflow). Phase 3 multi-project support follows sequentially. |
| TestEngineer | 1 | Responsible for bUnit test strategy, Playwright E2E test coverage, screenshot fidelity validation (SSIM comparison), and CI test pipeline configuration. |

---
_Generated at 2026-05-01 09:04:47 UTC_
