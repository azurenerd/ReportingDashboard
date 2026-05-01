# PM Specification: ReportingDashboard

**Version:** 1.0
**Date:** May 1, 2026
**Author:** Program Management
**Status:** Draft

---

## Executive Summary

ReportingDashboard is an executive-facing, single-page Blazor Server application (.NET 8) that renders project milestone timelines and execution heatmaps in a fixed 1920×1080 layout optimized for PowerPoint screenshot capture. The project aims to harden the existing MVP with automated testing and CI/CD, eliminate the manual screenshot workflow through programmatic export, and introduce multi-dashboard support—all while preserving the application's deliberate simplicity and zero-external-dependency architecture.

---

## Business Goals

1. **Eliminate manual screenshot workflow** — Provide a one-click (or API-driven) export of pixel-perfect 1920×1080 PNG/PDF dashboard images, removing the need for browser screenshot extensions and manual cropping.
2. **Harden the MVP for production readiness** — Achieve ≥80% code coverage with bUnit component tests, add JSON schema validation, and establish a CI/CD pipeline so the dashboard can be reliably deployed beyond a single developer laptop.
3. **Enable multi-project visibility** — Support multiple concurrent dashboard views (e.g., AgentSquad, Platform) from a single running instance, allowing executives to switch between project views without redeployment.
4. **Maintain architectural simplicity** — Preserve the zero-database, zero-authentication, JSON-file-driven design as the default operating mode; any enhancements must be additive and opt-in.
5. **Reduce time-to-slide** — Cut the end-to-end time from "data updated" to "screenshot in PowerPoint deck" from ~5 minutes (manual) to <30 seconds (automated export).
6. **Enable team-wide access** — Provide a containerized deployment option so the dashboard can be shared with a small team (1–50 users) at minimal cost (<$15/month).

---

## User Stories & Acceptance Criteria

### US-1: View Project Dashboard

**As an** executive or program manager, **I want** to view a project's milestone timeline and execution heatmap on a single page, **so that** I can quickly assess project health at a glance.

- [ ] Dashboard renders at exactly 1920×1080 pixels with no horizontal or vertical scrolling.
- [ ] Header section displays project name, date range, and last-updated timestamp.
- [ ] Timeline section renders all milestones from the JSON data as an SVG timeline with correct date positioning.
- [ ] Heatmap section renders all execution categories with color-coded cells reflecting status values from JSON.
- [ ] Page loads in under 2 seconds on localhost.
- [ ] A Blazor `<ErrorBoundary>` wraps the dashboard and displays a user-friendly error message if JSON parsing fails.

### US-2: Live Data Reload

**As a** program manager, **I want** the dashboard to automatically refresh when I update the JSON data file, **so that** I see changes immediately without restarting the server or refreshing the browser.

- [ ] Modifying `dashboard-data.json` triggers an automatic UI update within 5 seconds.
- [ ] `FileSystemWatcher` is the primary mechanism; a polling fallback (≤5-second interval) handles environments where FSW is unreliable.
- [ ] No full-page reload occurs; only changed components re-render via SignalR push.

### US-3: Export Dashboard as PNG

**As an** executive, **I want** to export the dashboard as a pixel-perfect 1920×1080 PNG image via a single action, **so that** I can paste it directly into a PowerPoint slide without manual cropping.

- [ ] `GET /api/export/{slug}?format=png` returns a `200 OK` with `Content-Type: image/png`.
- [ ] The exported image is exactly 1920×1080 pixels.
- [ ] The export visually matches the browser-rendered dashboard (no missing fonts, broken SVGs, or layout shifts).
- [ ] Export completes in under 10 seconds.
- [ ] A `404` is returned if the slug does not match any existing dashboard data file.

### US-4: Export Dashboard as PDF

**As an** executive, **I want** to export the dashboard as a single-page PDF, **so that** I can attach it to email updates or archive it.

- [ ] `GET /api/export/{slug}?format=pdf` returns a `200 OK` with `Content-Type: application/pdf`.
- [ ] The PDF contains a single page at 1920×1080 landscape dimensions.
- [ ] No browser chrome, headers, or footers appear in the PDF.
- [ ] Alternatively, `Ctrl+P` with the print stylesheet produces equivalent output with zero dependencies.

### US-5: Multi-Dashboard Support

**As a** program manager managing multiple projects, **I want** to host multiple dashboards from a single application instance, **so that** I can switch between project views without maintaining separate deployments.

- [ ] Each dashboard is accessible at `/dashboard/{slug}` (e.g., `/dashboard/agentsquad`, `/dashboard/platform`).
- [ ] Dashboard data files are stored as `wwwroot/data/{slug}.json`.
- [ ] An index page at `/` lists all available dashboards with links.
- [ ] Adding a new JSON file to `wwwroot/data/` makes a new dashboard available without restarting the server.
- [ ] Each dashboard's data is independently cached and watched for changes.

### US-6: Print-Friendly Output

**As a** user, **I want** to print the dashboard from my browser and get a clean 1920×1080 output, **so that** I have a zero-dependency fallback for generating slide images.

- [ ] A `@media print` stylesheet hides browser chrome, navigation elements, and scrollbars.
- [ ] Printed output matches the on-screen layout at 1920×1080.
- [ ] No additional packages or tools are required.

### US-7: JSON Data Validation

**As a** developer or PM editing the JSON data file, **I want** clear error messages when the JSON is malformed or missing required fields, **so that** I can fix data issues without debugging the application.

- [ ] On load, the application validates the JSON structure against expected schema (project name, milestones array, heatmap categories).
- [ ] If validation fails, the dashboard displays a human-readable error message identifying the specific issue (e.g., "Missing required field: projectName").
- [ ] The application does not crash or show a blank page on invalid JSON.

### US-8: Health Check Endpoint

**As a** DevOps engineer, **I want** a `/health` endpoint that returns the application's readiness status, **so that** container orchestrators can perform liveness and readiness probes.

- [ ] `GET /health` returns `200 OK` with a JSON body when the application is ready.
- [ ] The health check verifies that at least one dashboard data file is loadable.
- [ ] Response time is under 100ms.

### US-9: CI/CD Pipeline

**As a** developer, **I want** automated build and test execution on every push, **so that** regressions are caught before merging.

- [ ] A GitHub Actions workflow runs `dotnet build` and `dotnet test` on every push to `main` and on all pull requests.
- [ ] Build failures block PR merges.
- [ ] Test results and code coverage reports are visible in the GitHub Actions summary.

### US-10: Containerized Deployment

**As a** team lead, **I want** to deploy the dashboard as a Docker container, **so that** my team can access it from a shared URL without installing .NET locally.

- [ ] A `Dockerfile` exists at the repository root and produces a working image based on `mcr.microsoft.com/dotnet/aspnet:8.0`.
- [ ] `docker build` and `docker run` produce a running dashboard accessible on port 8080.
- [ ] Dashboard data can be mounted as a volume at `/app/wwwroot/data/`.

---

## Scope

### In Scope

- **Phase 1 — MVP Hardening (Weeks 1–2)**
  - bUnit component tests for `Timeline.razor`, `Heatmap.razor`, and `Header.razor`
  - JSON schema validation with user-friendly error messages
  - Blazor `<ErrorBoundary>` wrapper
  - Print stylesheet (`@media print`) for 1920×1080 output
  - `<meta>` viewport tag for consistent cross-monitor rendering
  - `JsonSerializerOptions` with `ReadCommentHandling = Skip`
  - `/health` endpoint for container readiness probes
  - Dockerfile and GitHub Actions CI pipeline (`dotnet build` + `dotnet test`)
  - Code coverage collection via coverlet

- **Phase 2 — Export & Multi-Dashboard (Weeks 3–5)**
  - Playwright-based export endpoint (`GET /api/export/{slug}?format=png|pdf`)
  - Multi-dashboard routing (`/dashboard/{slug}`) with index page
  - `ConcurrentDictionary`-based multi-file caching with per-file watchers
  - NSubstitute mocking and integration tests for the export pipeline
  - Prototype JSON editor page (`/edit/{slug}`) using Blazor `EditForm` — ship only if stakeholder feedback is positive

### Out of Scope

- **Database integration** — No SQLite, LiteDB, or any persistent store. JSON files remain the sole data source.
- **Authentication & authorization** — No Entra ID, no login, no RBAC. Deferred until a shared deployment is confirmed.
- **Azure DevOps API integration** — No automatic data population from ADO work items or pipelines. Deferred to Phase 3+.
- **Scheduled export automation** — No timer-triggered PDF generation or email delivery. Deferred to Phase 3+.
- **Real-time data connectors** — No live integration with Jira, ADO, or other project management tools.
- **Drill-down, filtering, or interactive analytics** — The dashboard is a static executive view, not a BI tool.
- **Custom branding/theming per team** — One fixed design for all dashboards.
- **Blazor WebAssembly (WASM) migration** — Blazor Server is correct for the current user base (<50 users).
- **OpenAPI/Swagger documentation** — No external API consumers expected.
- **Mobile or responsive layout** — Fixed 1920×1080 only.

---

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Dashboard page load (localhost) | < 2 seconds |
| Dashboard page load (containerized, same network) | < 3 seconds |
| Live reload after JSON file change | < 5 seconds |
| PNG export via Playwright | < 10 seconds |
| PDF export via Playwright | < 10 seconds |
| `/health` endpoint response | < 100ms |
| Memory footprint (idle, single dashboard) | < 150 MB |

### Scalability

- Support up to **50 concurrent SignalR connections** without degradation.
- Support up to **20 simultaneous dashboard slugs** in the multi-dashboard cache.
- JSON data files up to **1 MB** each without performance impact; files exceeding 1 MB trigger `IMemoryCache` with 60-second sliding expiration.

### Reliability

- `FileSystemWatcher` with polling fallback ensures data reload works on all supported platforms (Windows, Linux, macOS).
- Blazor `<ErrorBoundary>` prevents blank-page failures on malformed JSON.
- Health check endpoint enables automatic container restart on failure.
- No external service dependencies — the application runs fully offline.

### Security

- No PII or secrets in dashboard data files.
- HTTPS enabled via `dotnet dev-certs` for local development.
- If deployed to a network, TLS termination at reverse proxy (YARP, nginx, or Azure App Service).
- JSON data files must not contain credentials; if internal ADO URLs are present, the dashboard must not be publicly exposed.

### Compatibility

- **.NET 8 LTS** — supported until November 2026; plan .NET 10 LTS migration within 12 months.
- **Browsers**: Latest versions of Chrome, Edge, and Firefox (desktop only).
- **Containers**: Compatible with `mcr.microsoft.com/dotnet/aspnet:8.0` base image on Linux (amd64).

---

## Success Metrics

| Metric | Target | Measurement Method |
|--------|--------|--------------------|
| Component test coverage | ≥ 80% line coverage across `Timeline.razor`, `Heatmap.razor`, `Header.razor` | `dotnet test` with coverlet; reported in CI |
| CI pipeline green rate | ≥ 95% of builds on `main` pass | GitHub Actions history |
| Export accuracy | Exported PNG/PDF visually matches browser rendering in 100% of test cases | Manual visual inspection + snapshot tests via Verify |
| Time-to-slide reduction | From ~5 min (manual screenshot) to < 30 sec (API export) | Timed user workflow comparison |
| JSON validation error clarity | 100% of intentionally malformed test files produce a human-readable error message | Integration test suite with bad-data fixtures |
| Multi-dashboard discovery | New JSON file added to `wwwroot/data/` appears on index page within 10 seconds without restart | Manual + automated integration test |
| Container startup | `docker run` to healthy `/health` response in < 15 seconds | CI smoke test |

---

## Constraints & Assumptions

### Technical Constraints

- **Stack is fixed**: .NET 8 / Blazor Server / C#. No framework migration or alternative frontend (React, Angular) will be considered.
- **Zero external NuGet dependencies in Phase 1**: Only built-in .NET 8 SDK libraries. Playwright (Phase 2) and test libraries (bUnit, NSubstitute, coverlet) are the only permitted additions.
- **Fixed layout**: 1920×1080 pixels, non-responsive. This is a deliberate design decision for PowerPoint compatibility.
- **No database**: JSON files are the single source of truth. Database migration is explicitly out of scope.
- **Playwright dependency in Phase 2**: The export endpoint requires headless Chromium (~400 MB). This increases container image size and must be accounted for in deployment planning.

### Timeline Assumptions

- **Phase 1**: 1–2 weeks with a single developer.
- **Phase 2**: 2–3 weeks with a single developer, starting after Phase 1 is merged and stable.
- **Phase 3 (deferred)**: No timeline commitment. Features are gated on explicit stakeholder demand.

### Dependency Assumptions

- The GitHub repository and Actions runners are available and configured.
- The existing `dashboard-data.json` schema is stable and representative of production data.
- Stakeholders (executives/PMs) will provide feedback on the JSON editor prototype within 1 week of delivery to determine whether it ships or is dropped.
- Playwright headless Chromium is installable in the target CI and deployment environments (no corporate proxy blocking Chromium download).

### Organizational Assumptions

- The dashboard remains an **internal tool** with no external user access.
- The target audience is **< 50 users** (executives and program managers).
- Data updates are infrequent (1–5 times per week) and performed by a single author at a time.
- There is no requirement for audit trails, version history, or data rollback in the current scope.