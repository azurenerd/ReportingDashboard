# Research

_No research has been documented yet._

## Research technology stack for ReportingDashboard

_Researched on 2026-05-01 09:13 UTC_

### Summary

The ReportingDashboard is an executive-facing, single-page Blazor Server application (.NET 8) that renders project milestone timelines and execution heatmaps in a fixed 1920×1080 layout optimized for PowerPoint screenshot capture. The current implementation is intentionally minimal — zero external NuGet dependencies, no database, no authentication, and JSON-file-driven data. **The primary recommendation is to preserve this simplicity as a core architectural principle.** Future enhancements should be additive and opt-in. Where the dashboard needs to evolve (multi-project support, PDF export, data editing), the .NET 8 Blazor Server ecosystem provides mature, well-supported libraries that integrate without architectural disruption.

### Key Findings

- The mandatory stack is **.NET 8 / Blazor Server (Interactive Server render mode)** with C#, Razor components, and SignalR for real-time push — this is already implemented and running.
- The dashboard is **stateless by design**: all data lives in a single `dashboard-data.json` file monitored by `FileSystemWatcher`; there is no database, no user authentication, and no cloud dependency.
- **Blazor Server is the correct render mode** for this use case: the dashboard is an internal tool used by a small number of executives/PMs, so the persistent SignalR connection per client is not a scalability concern; it enables instant server-push on data file changes without client-side WASM overhead.
- The existing test infrastructure uses **xUnit 2.7 + Microsoft.NET.Test.Sdk 17.9**, which is the standard .NET testing stack and requires no changes.
- The biggest near-term enhancement opportunities are: (1) **PDF/image export** to eliminate the manual screenshot workflow, (2) **multi-dashboard support** to manage multiple project views from the same instance, and (3) **a simple data editor UI** to replace raw JSON editing.
- Competitive/comparable tools include Grafana dashboards, PowerBI embedded, and custom React dashboard frameworks — but the current Blazor Server approach is deliberately simpler and avoids external service dependencies, which is its key differentiator for this use case.
- No licensing concerns exist: the project uses only the default .NET 8 SDK (MIT-licensed) and xUnit (Apache 2.0). **Goal**: Stabilize what exists, add tests, make deployment-ready.
- Add **bUnit component tests** for `Timeline.razor`, `Heatmap.razor`, and `Header.razor` — validate rendering against known JSON input
- Add **JSON schema validation** on load (use `System.Text.Json` `JsonSchema` or manual checks) to catch malformed data with clear error messages
- Add a **Dockerfile** and **GitHub Actions CI pipeline** (`dotnet build` + `dotnet test` on push)
- Add a `/health` endpoint for container readiness probes
- **Quick win**: Add a print stylesheet (`@media print`) so `Ctrl+P` produces a clean 1920×1080 PDF without browser chrome — zero-dependency export solution **Goal**: Eliminate manual screenshot workflow, support multiple projects.
- Implement **Playwright-based export endpoint** (`GET /api/export/{slug}?format=png`)
- Add **multi-dashboard routing** (`/dashboard/{slug}`) with an index page
- Add **NSubstitute** mocking and integration tests for the export pipeline
- Prototype a **simple JSON editor** page using Blazor `EditForm` — if stakeholders find it valuable, invest further; if not, drop it **Goal**: Only pursue if clear demand emerges.
- Azure DevOps API integration for automatic data population
- Scheduled PDF email delivery (Azure Functions timer trigger + SendGrid)
- Entra ID authentication for shared deployment
- SQLite migration for audit trail / version history of dashboard data
- **Print stylesheet** — 30 minutes of CSS work, eliminates the need for screenshot extensions
- **`<meta>` viewport tag** set to 1920×1080 — ensures consistent rendering across different monitors
- **Error boundary component** — wrap the dashboard in a Blazor `<ErrorBoundary>` to show a friendly message instead of a blank page on JSON parse errors
- **JSON comments stripping** — the README mentions JSON comment support; ensure `System.Text.Json` is configured with `JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip }`

### Recommended Tools & Technologies

- | Layer | Library / Tool | Version | Purpose | Notes | |-------|---------------|---------|---------|-------| | UI Framework | **Blazor Server** (built-in) | .NET 8.0 | Interactive Razor components with SignalR | Already in use; no change needed | | CSS Framework | **Custom CSS** (built-in) | N/A | Fixed 1920×1080 layout | Scoped `.razor.css` files already in use | | Charting (if needed) | **Blazor.ApexCharts** | 3.6.0 | Rich SVG charting | Alternative: `BlazorChartjs` 3.2.0. Only add if timeline/heatmap needs bar/line charts beyond current SVG | | PDF/Image Export | **Playwright for .NET** | 1.42.0 | Headless Chromium screenshot/PDF of the dashboard page | Best option for pixel-perfect 1920×1080 capture; MIT license | | Icons (if needed) | **Blazor Heroicons** | 1.1.0 | Lightweight SVG icon set | Only if UI polish requires iconography | | Layer | Library / Tool | Version | Purpose | Notes | |-------|---------------|---------|---------|-------| | Web Host | **ASP.NET Core** (built-in) | 8.0 | Kestrel server, DI, middleware | Already in use | | JSON Handling | **System.Text.Json** (built-in) | 8.0 | JSON deserialization of dashboard data | Already in use; zero-dependency | | Configuration | **Microsoft.Extensions.Configuration** | 8.0.0 | Multi-environment settings | Already in use | | Logging | **Microsoft.Extensions.Logging** | 8.0.0 | Structured logging | Built-in; add Serilog 3.1.1 only if file/seq sink is needed | | File Watching | **FileSystemWatcher** (built-in) | .NET 8 | Live reload on JSON changes | Already implemented with polling fallback | | Layer | Recommendation | Rationale | |-------|---------------|-----------| | Primary | **JSON file** (`dashboard-data.json`) | Current approach; correct for single-user, low-write workload | | Future (if multi-user editing needed) | **SQLite via EF Core** | `Microsoft.EntityFrameworkCore.Sqlite` 8.0.4 — zero-infrastructure relational store; file-based; no server process | | Future (if audit trail needed) | **LiteDB** 5.0.21 | Embedded NoSQL document DB; single-file; good for append-only audit logs | | Layer | Library / Tool | Version | Purpose | |-------|---------------|---------|---------| | Test Framework | **xUnit** | 2.7.0 | Unit/integration tests (already in use) | | Test Runner | **Microsoft.NET.Test.Sdk** | 17.9.0 | VS Test Platform (already in use) | | Mocking | **NSubstitute** | 5.1.0 | Interface mocking; cleaner API than Moq; no castle dependency | | Blazor Component Testing | **bUnit** | 1.28.9 | Render and assert Blazor components in isolation | | Snapshot Testing | **Verify** | 24.2.0 | Snapshot approval tests for rendered HTML output | | Code Coverage | **coverlet.collector** | 6.0.2 | Inline coverage collection for `dotnet test` | | Layer | Tool | Notes | |-------|------|-------| | CI/CD | **GitHub Actions** | Repo already on GitHub; `.github/` directory exists | | Container (optional) | **Dockerfile** with `mcr.microsoft.com/dotnet/aspnet:8.0` | Only if deployment beyond localhost is needed | | Local Dev | `dotnet watch` | Already documented in README |
```
[dashboard-data.json] → [FileSystemWatcher] → [DashboardDataService (Singleton)]
                                                        ↓
                                              [SignalR Push to Clients]
                                                        ↓
                                          [Blazor Components: Dashboard.razor]
                                            ├── Header.razor
                                            ├── Timeline.razor (SVG)
                                            └── Heatmap.razor → HeatmapCell.razor
``` **This architecture is correct and should not be over-engineered.** Key principles to maintain:
- **Single data source**: One JSON file per dashboard. No database unless multi-user concurrent editing is required.
- **Server-side rendering**: Blazor Server mode keeps all logic on the server; the browser receives only DOM diffs via SignalR. This is ideal for an internal tool.
- **Singleton data service**: `DashboardDataService` as a singleton with in-memory caching is correct for a read-heavy, single-file workload.
- **Component composition**: Each visual section (Header, Timeline, Heatmap) is an independent Razor component with scoped CSS — this is already well-structured.
- Change `dashboard-data.json` to `wwwroot/data/{slug}.json` (e.g., `agentsquad.json`, `platform.json`)
- Add route parameter: `@page "/dashboard/{Slug}"` in `Dashboard.razor`
- Add a lightweight index page listing available dashboards
- `DashboardDataService` becomes a `ConcurrentDictionary<string, DashboardData>` cache with per-file watchers
- Add an API endpoint: `GET /api/export/{slug}?format=png|pdf`
- Use Playwright headless Chromium to navigate to `http://localhost:5000/dashboard/{slug}`, set viewport to 1920×1080, and capture
- Return the binary as `application/pdf` or `image/png`
- This eliminates the manual browser screenshot workflow entirely
- Add a `/edit/{slug}` route with a form-based editor for the JSON structure
- Use Blazor's built-in `EditForm` with `DataAnnotationsValidator`
- Write changes back to the JSON file; `FileSystemWatcher` handles propagation If REST API endpoints are added (e.g., for export or external data ingestion):
- Use **Minimal APIs** (built-in to .NET 8) — not MVC controllers. Example:
  ```csharp
  app.MapGet("/api/dashboards", (IDashboardDataService svc) => svc.ListAll());
  app.MapGet("/api/dashboards/{slug}", (string slug, IDashboardDataService svc) => svc.Get(slug));
  app.MapGet("/api/export/{slug}", async (string slug, ExportService export) => ...);
  ```
- Return `Results.File()` for binary export, `Results.Json()` for data queries
- No need for OpenAPI/Swagger unless the API is consumed by external clients
- **In-memory cache** via the singleton `DashboardDataService` is sufficient. No Redis, no distributed cache.
- Cache invalidation is already handled by `FileSystemWatcher` — file change → reload → SignalR push.
- If JSON files grow large (>1 MB), add `IMemoryCache` with a 60-second sliding expiration as a safety net.

### Considerations & Risks

- **Current state**: None — the dashboard is a local-only tool. This is correct for the current use case.
- **If deployed to a network**: Add Azure AD / Entra ID authentication via `Microsoft.Identity.Web` 2.17.0. Blazor Server integrates natively with ASP.NET Core authentication middleware.
- **Authorization**: Use a simple policy-based check (`[Authorize(Policy = "DashboardViewer")]`) if role-based access is needed. No complex RBAC required for a read-only dashboard.
- Dashboard data is non-sensitive project status information. No PII, no secrets.
- If the JSON file contains URLs to internal ADO boards, ensure the dashboard is not publicly exposed.
- **HTTPS**: Enable in `launchSettings.json` with `dotnet dev-certs https --trust` for local dev. For production, terminate TLS at a reverse proxy (YARP, nginx, or Azure App Service). | Scenario | Recommendation | Monthly Cost Estimate | |----------|---------------|----------------------| | **Local only** (current) | `dotnet run` on developer laptop | $0 | | **Small team (1-10 users)** | Azure App Service B1 (Linux) | ~$13/month | | **Medium (10-50 users)** | Azure App Service S1 (Linux) + Azure CDN for static files | ~$70/month | | **Container** | Azure Container Apps (consumption plan) | ~$0-$5/month (pay per use) |
- **Recommended for team sharing**: Deploy as a Docker container on Azure Container Apps (consumption tier). Push JSON updates via mounted Azure File Share or Git-based deploy.
- Dockerfile:
  ```dockerfile
  FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
  FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
  WORKDIR /src
  COPY . .
  RUN dotnet publish src/ReportingDashboard -c Release -o /app
  FROM base
  WORKDIR /app
  COPY --from=build /app .
  ENTRYPOINT ["dotnet", "ReportingDashboard.dll"]
  ```
- **Logging**: Built-in `ILogger<T>` to console is sufficient for local use.
- **If deployed**: Add `Microsoft.ApplicationInsights.AspNetCore` 2.22.0 for Application Insights integration — one NuGet package, one line of config.
- **Health check**: Add `app.MapHealthChecks("/health")` via `Microsoft.Extensions.Diagnostics.HealthChecks` (built-in) for container orchestrator probes. | Risk | Severity | Mitigation | |------|----------|------------| | **SignalR connection limits** | Low | Blazor Server holds one WebSocket per client. At <50 concurrent users this is irrelevant. If usage scales beyond 100, consider Blazor WebAssembly (WASM) mode — but this is unlikely for an executive dashboard. | | **FileSystemWatcher reliability** | Medium | FSW is known to miss events on network shares and some Linux filesystems. The existing polling fallback mitigates this. Ensure polling interval is ≤5 seconds. | | **JSON file as single point of truth** | Low | Acceptable for current scope. Risk increases if multiple users edit simultaneously — mitigate with file locking or migrate to SQLite. | | **Manual screenshot workflow** | Medium | This is the #1 user friction point. Automate with Playwright export endpoint (see Architecture section). Prototype this early. | | **Scope creep toward full BI tool** | High | The dashboard's value is its simplicity. Resist adding drill-down, filtering, or real-time data connectors — use Power BI or Grafana for those needs. Keep this tool focused on static executive slide generation. | | **Blazor Server in .NET 8 LTS** | Low | .NET 8 is LTS (supported until November 2026). Plan upgrade to .NET 10 LTS (November 2025 release) within the next year. Migration is typically trivial (TFM change + package updates). |
- **Multi-project support**: Will the dashboard need to serve multiple project views simultaneously, or is one JSON file per deployment sufficient? (Drives routing and caching design.)
- **Data authoring workflow**: Who edits the JSON file? Should there be a web-based editor, or is direct file editing acceptable for the target audience?
- **Export automation**: Should screenshot/PDF export be triggered manually via a button, or automated on a schedule (e.g., every Monday at 9 AM, email PDF to stakeholders)?
- **Deployment target**: Will this remain localhost-only, or does it need to be deployed to a shared server for the team? (Drives authentication and hosting decisions.)
- **Data source integration**: Is there appetite to pull data directly from Azure DevOps APIs instead of manually maintaining JSON? (Significant scope increase — recommend deferring to Phase 3.)
- **Branding/theming**: Do different teams need different color schemes or layouts, or is one fixed design sufficient?

### Detailed Analysis

# Research: Technology Stack for ReportingDashboard

## Executive Summary

The ReportingDashboard is an executive-facing, single-page Blazor Server application (.NET 8) that renders project milestone timelines and execution heatmaps in a fixed 1920×1080 layout optimized for PowerPoint screenshot capture. The current implementation is intentionally minimal — zero external NuGet dependencies, no database, no authentication, and JSON-file-driven data. **The primary recommendation is to preserve this simplicity as a core architectural principle.** Future enhancements should be additive and opt-in. Where the dashboard needs to evolve (multi-project support, PDF export, data editing), the .NET 8 Blazor Server ecosystem provides mature, well-supported libraries that integrate without architectural disruption.

## Key Findings

- The mandatory stack is **.NET 8 / Blazor Server (Interactive Server render mode)** with C#, Razor components, and SignalR for real-time push — this is already implemented and running.
- The dashboard is **stateless by design**: all data lives in a single `dashboard-data.json` file monitored by `FileSystemWatcher`; there is no database, no user authentication, and no cloud dependency.
- **Blazor Server is the correct render mode** for this use case: the dashboard is an internal tool used by a small number of executives/PMs, so the persistent SignalR connection per client is not a scalability concern; it enables instant server-push on data file changes without client-side WASM overhead.
- The existing test infrastructure uses **xUnit 2.7 + Microsoft.NET.Test.Sdk 17.9**, which is the standard .NET testing stack and requires no changes.
- The biggest near-term enhancement opportunities are: (1) **PDF/image export** to eliminate the manual screenshot workflow, (2) **multi-dashboard support** to manage multiple project views from the same instance, and (3) **a simple data editor UI** to replace raw JSON editing.
- Competitive/comparable tools include Grafana dashboards, PowerBI embedded, and custom React dashboard frameworks — but the current Blazor Server approach is deliberately simpler and avoids external service dependencies, which is its key differentiator for this use case.
- No licensing concerns exist: the project uses only the default .NET 8 SDK (MIT-licensed) and xUnit (Apache 2.0).

## Recommended Technology Stack

### Frontend (Blazor Server Components)

| Layer | Library / Tool | Version | Purpose | Notes |
|-------|---------------|---------|---------|-------|
| UI Framework | **Blazor Server** (built-in) | .NET 8.0 | Interactive Razor components with SignalR | Already in use; no change needed |
| CSS Framework | **Custom CSS** (built-in) | N/A | Fixed 1920×1080 layout | Scoped `.razor.css` files already in use |
| Charting (if needed) | **Blazor.ApexCharts** | 3.6.0 | Rich SVG charting | Alternative: `BlazorChartjs` 3.2.0. Only add if timeline/heatmap needs bar/line charts beyond current SVG |
| PDF/Image Export | **Playwright for .NET** | 1.42.0 | Headless Chromium screenshot/PDF of the dashboard page | Best option for pixel-perfect 1920×1080 capture; MIT license |
| Icons (if needed) | **Blazor Heroicons** | 1.1.0 | Lightweight SVG icon set | Only if UI polish requires iconography |

### Backend

| Layer | Library / Tool | Version | Purpose | Notes |
|-------|---------------|---------|---------|-------|
| Web Host | **ASP.NET Core** (built-in) | 8.0 | Kestrel server, DI, middleware | Already in use |
| JSON Handling | **System.Text.Json** (built-in) | 8.0 | JSON deserialization of dashboard data | Already in use; zero-dependency |
| Configuration | **Microsoft.Extensions.Configuration** | 8.0.0 | Multi-environment settings | Already in use |
| Logging | **Microsoft.Extensions.Logging** | 8.0.0 | Structured logging | Built-in; add Serilog 3.1.1 only if file/seq sink is needed |
| File Watching | **FileSystemWatcher** (built-in) | .NET 8 | Live reload on JSON changes | Already implemented with polling fallback |

### Data Storage

| Layer | Recommendation | Rationale |
|-------|---------------|-----------|
| Primary | **JSON file** (`dashboard-data.json`) | Current approach; correct for single-user, low-write workload |
| Future (if multi-user editing needed) | **SQLite via EF Core** | `Microsoft.EntityFrameworkCore.Sqlite` 8.0.4 — zero-infrastructure relational store; file-based; no server process |
| Future (if audit trail needed) | **LiteDB** 5.0.21 | Embedded NoSQL document DB; single-file; good for append-only audit logs |

### Testing

| Layer | Library / Tool | Version | Purpose |
|-------|---------------|---------|---------|
| Test Framework | **xUnit** | 2.7.0 | Unit/integration tests (already in use) |
| Test Runner | **Microsoft.NET.Test.Sdk** | 17.9.0 | VS Test Platform (already in use) |
| Mocking | **NSubstitute** | 5.1.0 | Interface mocking; cleaner API than Moq; no castle dependency |
| Blazor Component Testing | **bUnit** | 1.28.9 | Render and assert Blazor components in isolation |
| Snapshot Testing | **Verify** | 24.2.0 | Snapshot approval tests for rendered HTML output |
| Code Coverage | **coverlet.collector** | 6.0.2 | Inline coverage collection for `dotnet test` |

### Infrastructure & CI/CD

| Layer | Tool | Notes |
|-------|------|-------|
| CI/CD | **GitHub Actions** | Repo already on GitHub; `.github/` directory exists |
| Container (optional) | **Dockerfile** with `mcr.microsoft.com/dotnet/aspnet:8.0` | Only if deployment beyond localhost is needed |
| Local Dev | `dotnet watch` | Already documented in README |

## Architecture Recommendations

### Current Architecture (Preserve)

```
[dashboard-data.json] → [FileSystemWatcher] → [DashboardDataService (Singleton)]
                                                        ↓
                                              [SignalR Push to Clients]
                                                        ↓
                                          [Blazor Components: Dashboard.razor]
                                            ├── Header.razor
                                            ├── Timeline.razor (SVG)
                                            └── Heatmap.razor → HeatmapCell.razor
```

**This architecture is correct and should not be over-engineered.** Key principles to maintain:

1. **Single data source**: One JSON file per dashboard. No database unless multi-user concurrent editing is required.
2. **Server-side rendering**: Blazor Server mode keeps all logic on the server; the browser receives only DOM diffs via SignalR. This is ideal for an internal tool.
3. **Singleton data service**: `DashboardDataService` as a singleton with in-memory caching is correct for a read-heavy, single-file workload.
4. **Component composition**: Each visual section (Header, Timeline, Heatmap) is an independent Razor component with scoped CSS — this is already well-structured.

### Recommended Enhancements (Additive)

**Multi-Dashboard Support:**
- Change `dashboard-data.json` to `wwwroot/data/{slug}.json` (e.g., `agentsquad.json`, `platform.json`)
- Add route parameter: `@page "/dashboard/{Slug}"` in `Dashboard.razor`
- Add a lightweight index page listing available dashboards
- `DashboardDataService` becomes a `ConcurrentDictionary<string, DashboardData>` cache with per-file watchers

**PDF/Screenshot Export Endpoint:**
- Add an API endpoint: `GET /api/export/{slug}?format=png|pdf`
- Use Playwright headless Chromium to navigate to `http://localhost:5000/dashboard/{slug}`, set viewport to 1920×1080, and capture
- Return the binary as `application/pdf` or `image/png`
- This eliminates the manual browser screenshot workflow entirely

**Data Editor (Phase 2):**
- Add a `/edit/{slug}` route with a form-based editor for the JSON structure
- Use Blazor's built-in `EditForm` with `DataAnnotationsValidator`
- Write changes back to the JSON file; `FileSystemWatcher` handles propagation

### API Design

If REST API endpoints are added (e.g., for export or external data ingestion):

- Use **Minimal APIs** (built-in to .NET 8) — not MVC controllers. Example:
  ```csharp
  app.MapGet("/api/dashboards", (IDashboardDataService svc) => svc.ListAll());
  app.MapGet("/api/dashboards/{slug}", (string slug, IDashboardDataService svc) => svc.Get(slug));
  app.MapGet("/api/export/{slug}", async (string slug, ExportService export) => ...);
  ```
- Return `Results.File()` for binary export, `Results.Json()` for data queries
- No need for OpenAPI/Swagger unless the API is consumed by external clients

### Caching Strategy

- **In-memory cache** via the singleton `DashboardDataService` is sufficient. No Redis, no distributed cache.
- Cache invalidation is already handled by `FileSystemWatcher` — file change → reload → SignalR push.
- If JSON files grow large (>1 MB), add `IMemoryCache` with a 60-second sliding expiration as a safety net.

## Security & Infrastructure

### Authentication & Authorization

- **Current state**: None — the dashboard is a local-only tool. This is correct for the current use case.
- **If deployed to a network**: Add Azure AD / Entra ID authentication via `Microsoft.Identity.Web` 2.17.0. Blazor Server integrates natively with ASP.NET Core authentication middleware.
- **Authorization**: Use a simple policy-based check (`[Authorize(Policy = "DashboardViewer")]`) if role-based access is needed. No complex RBAC required for a read-only dashboard.

### Data Protection

- Dashboard data is non-sensitive project status information. No PII, no secrets.
- If the JSON file contains URLs to internal ADO boards, ensure the dashboard is not publicly exposed.
- **HTTPS**: Enable in `launchSettings.json` with `dotnet dev-certs https --trust` for local dev. For production, terminate TLS at a reverse proxy (YARP, nginx, or Azure App Service).

### Hosting & Deployment

| Scenario | Recommendation | Monthly Cost Estimate |
|----------|---------------|----------------------|
| **Local only** (current) | `dotnet run` on developer laptop | $0 |
| **Small team (1-10 users)** | Azure App Service B1 (Linux) | ~$13/month |
| **Medium (10-50 users)** | Azure App Service S1 (Linux) + Azure CDN for static files | ~$70/month |
| **Container** | Azure Container Apps (consumption plan) | ~$0-$5/month (pay per use) |

- **Recommended for team sharing**: Deploy as a Docker container on Azure Container Apps (consumption tier). Push JSON updates via mounted Azure File Share or Git-based deploy.
- Dockerfile:
  ```dockerfile
  FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
  FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
  WORKDIR /src
  COPY . .
  RUN dotnet publish src/ReportingDashboard -c Release -o /app
  FROM base
  WORKDIR /app
  COPY --from=build /app .
  ENTRYPOINT ["dotnet", "ReportingDashboard.dll"]
  ```

### Monitoring & Observability

- **Logging**: Built-in `ILogger<T>` to console is sufficient for local use.
- **If deployed**: Add `Microsoft.ApplicationInsights.AspNetCore` 2.22.0 for Application Insights integration — one NuGet package, one line of config.
- **Health check**: Add `app.MapHealthChecks("/health")` via `Microsoft.Extensions.Diagnostics.HealthChecks` (built-in) for container orchestrator probes.

## Risks & Trade-offs

| Risk | Severity | Mitigation |
|------|----------|------------|
| **SignalR connection limits** | Low | Blazor Server holds one WebSocket per client. At <50 concurrent users this is irrelevant. If usage scales beyond 100, consider Blazor WebAssembly (WASM) mode — but this is unlikely for an executive dashboard. |
| **FileSystemWatcher reliability** | Medium | FSW is known to miss events on network shares and some Linux filesystems. The existing polling fallback mitigates this. Ensure polling interval is ≤5 seconds. |
| **JSON file as single point of truth** | Low | Acceptable for current scope. Risk increases if multiple users edit simultaneously — mitigate with file locking or migrate to SQLite. |
| **Manual screenshot workflow** | Medium | This is the #1 user friction point. Automate with Playwright export endpoint (see Architecture section). Prototype this early. |
| **Scope creep toward full BI tool** | High | The dashboard's value is its simplicity. Resist adding drill-down, filtering, or real-time data connectors — use Power BI or Grafana for those needs. Keep this tool focused on static executive slide generation. |
| **Blazor Server in .NET 8 LTS** | Low | .NET 8 is LTS (supported until November 2026). Plan upgrade to .NET 10 LTS (November 2025 release) within the next year. Migration is typically trivial (TFM change + package updates). |

## Open Questions

1. **Multi-project support**: Will the dashboard need to serve multiple project views simultaneously, or is one JSON file per deployment sufficient? (Drives routing and caching design.)
2. **Data authoring workflow**: Who edits the JSON file? Should there be a web-based editor, or is direct file editing acceptable for the target audience?
3. **Export automation**: Should screenshot/PDF export be triggered manually via a button, or automated on a schedule (e.g., every Monday at 9 AM, email PDF to stakeholders)?
4. **Deployment target**: Will this remain localhost-only, or does it need to be deployed to a shared server for the team? (Drives authentication and hosting decisions.)
5. **Data source integration**: Is there appetite to pull data directly from Azure DevOps APIs instead of manually maintaining JSON? (Significant scope increase — recommend deferring to Phase 3.)
6. **Branding/theming**: Do different teams need different color schemes or layouts, or is one fixed design sufficient?

## Implementation Recommendations

### Phase 1: MVP Hardening (1-2 weeks)
**Goal**: Stabilize what exists, add tests, make deployment-ready.

- Add **bUnit component tests** for `Timeline.razor`, `Heatmap.razor`, and `Header.razor` — validate rendering against known JSON input
- Add **JSON schema validation** on load (use `System.Text.Json` `JsonSchema` or manual checks) to catch malformed data with clear error messages
- Add a **Dockerfile** and **GitHub Actions CI pipeline** (`dotnet build` + `dotnet test` on push)
- Add a `/health` endpoint for container readiness probes
- **Quick win**: Add a print stylesheet (`@media print`) so `Ctrl+P` produces a clean 1920×1080 PDF without browser chrome — zero-dependency export solution

### Phase 2: Export & Multi-Dashboard (2-3 weeks)
**Goal**: Eliminate manual screenshot workflow, support multiple projects.

- Implement **Playwright-based export endpoint** (`GET /api/export/{slug}?format=png`)
- Add **multi-dashboard routing** (`/dashboard/{slug}`) with an index page
- Add **NSubstitute** mocking and integration tests for the export pipeline
- Prototype a **simple JSON editor** page using Blazor `EditForm` — if stakeholders find it valuable, invest further; if not, drop it

### Phase 3: Optional Enhancements (Defer)
**Goal**: Only pursue if clear demand emerges.

- Azure DevOps API integration for automatic data population
- Scheduled PDF email delivery (Azure Functions timer trigger + SendGrid)
- Entra ID authentication for shared deployment
- SQLite migration for audit trail / version history of dashboard data

### Quick Wins (Implement Immediately)

1. **Print stylesheet** — 30 minutes of CSS work, eliminates the need for screenshot extensions
2. **`<meta>` viewport tag** set to 1920×1080 — ensures consistent rendering across different monitors
3. **Error boundary component** — wrap the dashboard in a Blazor `<ErrorBoundary>` to show a friendly message instead of a blank page on JSON parse errors
4. **JSON comments stripping** — the README mentions JSON comment support; ensure `System.Text.Json` is configured with `JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip }`
