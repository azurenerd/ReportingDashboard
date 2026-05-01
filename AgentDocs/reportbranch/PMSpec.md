# PM Specification: ReportingDashboard

**Document Version:** 1.0
**Date:** May 1, 2026
**Author:** Program Management
**Status:** Draft
**Stack:** .NET 8 / Blazor Server / C# 12

---

## Executive Summary

The ReportingDashboard is a local-first Blazor Server application that renders project milestone timelines, monthly execution heatmaps, and key deliverables into a fixed 1920×1080 layout optimized for PowerPoint screenshot capture. The project enhances an existing, functional zero-dependency prototype by adding structured component testing, automated screenshot/PDF export, JSON data validation, and optional multi-project support — eliminating the manual, error-prone browser-screenshot workflow used today for executive status reporting.

---

## Business Goals

1. **Eliminate manual screenshot workflow** — Automate the capture of pixel-perfect 1920×1080 PNG/PDF dashboard images, reducing the time to produce executive status slides from ~10 minutes of manual steps to a single CLI command or CI artifact.
2. **Ensure visual correctness through testing** — Establish component-level test coverage (bUnit) for all Razor visualization components so that SVG timeline, heatmap, and header rendering regressions are caught before they reach stakeholders.
3. **Validate data on load** — Prevent malformed JSON data (invalid dates, missing fields, bad color values) from silently corrupting the rendered dashboard by adding structured validation with actionable error messages.
4. **Support multiple project dashboards** — Enable a single running instance to serve dashboards for different projects via URL parameter, allowing teams to maintain separate JSON data files without duplicating the application.
5. **Preserve zero-ops simplicity** — Maintain the local-first, zero-database, zero-auth, JSON-file-driven architecture that allows any developer to run the dashboard with `dotnet run` and no external dependencies.

---

## User Stories & Acceptance Criteria

### US-1: View Project Dashboard

**As a** program manager, **I want** to open a browser to `localhost:5000` and see the current project's milestone timeline, execution heatmap, and header summary, **so that** I can review project status at a glance.

- [ ] Dashboard renders at exactly 1920×1080 with `overflow: hidden` (no scrollbars).
- [ ] Header component displays project title, subtitle, backlog URL, and current date from JSON data.
- [ ] Timeline component renders SVG tracks with milestones (checkpoint, POC, production types) color-coded per track.
- [ ] Heatmap component renders monthly execution cells with correct status colors and highlights the current month.
- [ ] Page loads in under 2 seconds on localhost.

### US-2: Live Reload on Data Change

**As a** program manager, **I want** the dashboard to automatically re-render when I save changes to `dashboard-data.json`, **so that** I can iterate on status data without manually refreshing the browser.

- [ ] Saving `dashboard-data.json` triggers a re-render within 1 second (debounced at 300ms).
- [ ] If the JSON file has a parse error, the previous valid dashboard remains visible and an error panel is displayed with the specific error.
- [ ] When the file is corrected and saved, the dashboard recovers automatically without browser refresh.
- [ ] FileSystemWatcher is backed by a 5-second polling fallback for reliability.

### US-3: Automated Screenshot Export

**As a** program manager, **I want** to run a CLI command that captures the dashboard as a 1920×1080 PNG image, **so that** I can paste it directly into PowerPoint without manual browser screenshots.

- [ ] A `ScreenshotExporter` service or CLI command launches headless Chromium via Playwright at 1920×1080.
- [ ] The export waits for the dashboard to fully render (detected via a CSS class marker on the root element).
- [ ] Output PNG is saved to a configurable file path (default: `./output/dashboard.png`).
- [ ] PNG output is visually identical to a manual browser screenshot (SVG fidelity, fonts, colors validated during prototyping).
- [ ] Optional `--pdf` flag produces a single-page PDF export.

### US-4: CI-Generated Screenshot Artifact

**As a** development team member, **I want** GitHub Actions to automatically generate a fresh dashboard screenshot whenever `dashboard-data.json` changes on the main branch, **so that** the latest status image is always available as a build artifact.

- [ ] A GitHub Actions workflow triggers on pushes that modify `wwwroot/data/dashboard-data.json`.
- [ ] The workflow starts the Blazor Server app, runs the screenshot exporter, and uploads the PNG as a workflow artifact.
- [ ] The workflow completes in under 3 minutes.
- [ ] Failed screenshot generation fails the workflow with a descriptive error.

### US-5: JSON Data Validation

**As a** program manager, **I want** the dashboard to validate my JSON data on load and show specific, actionable error messages, **so that** I can quickly fix data issues instead of seeing a broken or blank dashboard.

- [ ] A `DashboardDataValidator` runs on every data load (initial and file-change reload).
- [ ] Validation checks: required fields present, date formats valid (ISO 8601), track colors are valid hex codes, heatmap status values are within the allowed set.
- [ ] Validation errors are displayed in the error panel with field-level specificity (e.g., "Track M1 milestone date '2025-13-01' is not a valid date").
- [ ] Valid data that passes all checks renders normally with no validation overhead visible to the user.

### US-6: Multi-Project Dashboard Support

**As a** program manager managing multiple projects, **I want** to switch between project dashboards via URL parameter (e.g., `/?project=agentsquad`), **so that** I can maintain separate status dashboards without running multiple application instances.

- [ ] `DashboardDataService` watches the entire `wwwroot/data/` directory for `*.json` files.
- [ ] Navigating to `/{projectName}` loads `wwwroot/data/{projectName}.json`.
- [ ] Navigating to `/` (no parameter) loads a default project or shows a project selector.
- [ ] Header component displays a project switcher dropdown listing all available JSON files.
- [ ] File changes to any watched JSON file trigger re-render only for the currently viewed project.

### US-7: Component Test Coverage

**As a** developer, **I want** bUnit component tests for Timeline, Heatmap, HeatmapCell, and Header components, **so that** I can refactor visualization logic with confidence that regressions are caught.

- [ ] `ReportingDashboard.Tests` project includes `bunit` NuGet package.
- [ ] `Timeline.razor` tests verify: correct number of SVG track elements rendered, milestone positions calculated correctly, current-date marker positioned accurately.
- [ ] `Heatmap.razor` tests verify: correct number of rows and cells rendered, highlight month applied correctly.
- [ ] `Header.razor` tests verify: title, subtitle, and date bound from model data.
- [ ] All component tests pass in CI (`dotnet test`).

### US-8: Health Check Endpoint

**As a** CI/automation engineer, **I want** a `/health` endpoint that returns HTTP 200 when the application is running, **so that** automation scripts can verify the server is ready before taking screenshots.

- [ ] `GET /health` returns HTTP 200 with an empty or minimal JSON body.
- [ ] The endpoint responds within 100ms.
- [ ] No authentication required.

---

## Scope

### In Scope

- Automated PNG screenshot export via Playwright for .NET (headless Chromium at 1920×1080)
- Optional PDF export via Playwright
- GitHub Actions workflow for CI-generated screenshot artifacts
- JSON data validation with actionable error messages (`DashboardDataValidator`)
- bUnit component tests for `Timeline.razor`, `Heatmap.razor`, `HeatmapCell.razor`, and `Header.razor`
- Multi-project support via directory-based JSON files and URL route parameter
- Project switcher dropdown in `Header.razor`
- Health check endpoint (`/health`)
- Preservation of existing zero-external-dependency core architecture
- Preservation of fixed 1920×1080 viewport and custom CSS

### Out of Scope

- **Database integration** — No SQLite, LiteDB, EF Core, or any persistence layer. JSON files are the data store.
- **Authentication / Authorization** — No login, tokens, or access control. The app runs on localhost only.
- **Blazor WebAssembly migration** — Server-side rendering with SignalR remains the architecture.
- **CSS framework adoption** — No Bootstrap, Tailwind, or MudBlazor. Custom CSS is purpose-built for the fixed layout.
- **Charting library integration** — No ApexCharts, Chart.js, or Radzen charts. Hand-crafted SVG is retained.
- **Cloud hosting / deployment** — No Azure App Service, container, or cloud infrastructure.
- **Responsive design / mobile support** — Fixed 1920×1080 only; no breakpoints.
- **Multi-user concurrent editing** — Single-user local tool; no real-time collaboration.
- **Data entry UI** — Users edit `dashboard-data.json` directly; no in-app editing forms.
- **.NET 9/10 migration** — Stays on .NET 8 LTS for this release.

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Initial page load (localhost) | < 2 seconds |
| Re-render after JSON file save | < 1 second (debounced at 300ms) |
| Health check response time | < 100ms |
| Automated screenshot capture (CLI) | < 15 seconds end-to-end |
| CI workflow total duration | < 3 minutes |
| Memory per SignalR circuit | < 5 MB |

### Reliability

- JSON parse errors must never crash the application; previous valid data persists.
- FileSystemWatcher failures are mitigated by 5-second polling fallback (already implemented).
- `DashboardDataService` retry-on-IOException behavior is preserved.
- Automated screenshot export retries up to 3 times on transient failures (browser launch, navigation timeout).

### Security

- Kestrel binds to `localhost` only (verified in `launchSettings.json`).
- No PII, secrets, or credentials in JSON data files.
- No authentication required for the current local-only deployment model.
- If network exposure is ever needed, Windows Integrated Auth (`Microsoft.AspNetCore.Authentication.Negotiate`) is the recommended path — but is explicitly out of scope for this release.

### Maintainability

- Zero external NuGet dependencies for core rendering (preserve current state).
- Test dependencies (bUnit, Playwright, FluentAssertions) added to test project only.
- All new code follows existing patterns: service-component separation, event-driven re-render, C# 12 conventions.

---

## Success Metrics

| Metric | Definition | Target |
|--------|-----------|--------|
| **Screenshot automation adoption** | % of status report cycles using automated export instead of manual screenshot | 100% within 2 weeks of delivery |
| **Time to produce status slide** | Time from JSON data update to PowerPoint-ready PNG | < 30 seconds (down from ~10 minutes) |
| **Component test coverage** | % of Razor visualization components with bUnit tests | 100% (Timeline, Heatmap, HeatmapCell, Header) |
| **Data validation error rate** | % of data load errors caught by validator before rendering | 100% of detectable schema violations |
| **Zero regression on export fidelity** | Automated PNG matches manual screenshot at pixel level (SSIM > 0.98) | Verified during prototyping phase |
| **CI pipeline reliability** | % of CI screenshot jobs that succeed on valid data | > 99% |

---

## Constraints & Assumptions

### Technical Constraints

- **Runtime:** .NET 8 LTS (8.0.x latest patch). No migration to .NET 9/10 in this release.
- **Rendering:** Fixed 1920×1080 viewport. All visualization must fit without scrolling.
- **Data format:** JSON files in `wwwroot/data/`. Schema must remain backward-compatible with existing `dashboard-data.json`.
- **Export tool:** Playwright for .NET (Chromium). SkiaSharp or server-side alternatives are not in scope unless Playwright prototyping reveals rendering fidelity issues.
- **Hosting:** Local Kestrel only. No reverse proxy, no TLS, no cloud infrastructure.
- **.NET 8 LTS EOL:** November 2026. Migration to .NET 10 LTS must be planned by Q3 2026 but is outside this specification.

### Timeline Assumptions

- **Phase 1 (Testing & Validation):** 1 week — bUnit tests, JSON validator, health endpoint.
- **Phase 2 (Automated Export):** 1 week — Playwright screenshot CLI, CI workflow.
- **Phase 3 (Multi-Project):** 1 week — directory watching, route parameter, project switcher. Contingent on stakeholder confirmation that multi-project is needed.

### Dependency Assumptions

- Playwright for .NET (`Microsoft.Playwright` 1.47.x) headless Chromium renders SVG with sufficient fidelity for screenshot use. **Requires prototyping validation before committing to Phase 2 implementation.**
- GitHub Actions free tier provides sufficient minutes for CI screenshot generation.
- The existing `DashboardDataService` debounce/retry/fallback logic is correct and does not need redesign — only extension for directory watching and validation hooks.
- Stakeholders confirm that automated screenshots are a pain point worth solving (open question from research). If not confirmed, Phase 2 is deprioritized and Phase 1 (testing/validation) delivers standalone value.

### Open Decisions (Require Stakeholder Input)

1. Is multi-project support needed now, or is single-project sufficient for the next 6 months?
2. Is PDF export required in addition to PNG?
3. Should `dashboard-data.json` be version-controlled in a separate config repo for historical tracking?
4. Should the dashboard support `@media print` for direct browser printing?