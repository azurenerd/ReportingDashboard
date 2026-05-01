# Research

_No research has been documented yet._

## Research technology stack for ReportingDashboard

_Researched on 2026-05-01 09:38 UTC_

### Summary

The ReportingDashboard is a Blazor Server (.NET 8) executive reporting application that visualizes project milestone timelines, monthly execution status, and key deliverables in a screenshot-friendly 1920×1080 format. The underlying project it tracks — the Automated Event-Driven User Access Review (UAR) system for Azure Core Trusted Platform — operates at significant scale (~1,100 services, ~50,000 users, ~12M privileged access instances) and must satisfy SOC 2, PCI DSS, FedRAMP, and NIST compliance controls. **Primary recommendation:** Extend the existing Blazor Server / .NET 8 stack with targeted library additions for charting (Radzen/MudBlazor), data persistence (SQLite or Azure SQL), and structured export (PuppeteerSharp for automated screenshots). Keep the zero-external-dependency philosophy for the MVP but plan a clear upgrade path for multi-user scenarios and live data integration as the UAR platform matures through its 3-phase rollout. ---

### Key Findings

- The current architecture is intentionally minimal — Blazor Server, JSON-file-driven, no database, no auth, no cloud dependencies — which is appropriate for a single-user executive reporting tool but will not scale to multi-stakeholder UAR reporting across ~8,300 managers.
- Blazor Server's real-time SignalR push model is a strong fit for a live dashboard that reflects UAR event-driven triggers (role changes, anomaly signals) without polling.
- The UAR feature spec demands rich audit evidence (trigger, criteria, decision, remediation, timestamp) — the dashboard must eventually consume structured audit trail data, not just hand-edited JSON.
- .NET 8 is the current LTS release (supported through November 2026); .NET 9 is available but not LTS. Staying on .NET 8 is the correct choice for stability.
- The fixed 1920×1080 layout optimized for PowerPoint screenshots is a differentiating feature — automated screenshot generation via headless Chromium should be a priority enhancement.
- Compliance frameworks (SOC 2 CC6.2, PCI DSS 7.2.4, FedRAMP KSI-IAM-07) require audit evidence that the dashboard can help surface, but the dashboard itself must not become a compliance-critical system without proper auth and access logging.
- Blazor's component model maps cleanly to the dashboard's visual structure (Header, Timeline, Heatmap, HeatmapCell) — no architectural rework is needed. ---
```
┌─────────────────────────────────────────────┐
│              Browser (1920×1080)             │
│  ┌─────────┐ ┌──────────┐ ┌──────────────┐  │
│  │ Header  │ │ Timeline │ │   Heatmap    │  │
│  └─────────┘ └──────────┘ └──────────────┘  │
└──────────────────┬──────────────────────────┘
                   │ SignalR (WebSocket)
┌──────────────────▼──────────────────────────┐
│           Blazor Server (.NET 8)            │
│  ┌──────────────────────────────────────┐   │
│  │      DashboardDataService            │   │
│  │  (JSON load + FileSystemWatcher)     │   │
│  └──────────────┬───────────────────────┘   │
└─────────────────┼───────────────────────────┘
                  │ File I/O
        ┌─────────▼─────────┐
        │ dashboard-data.json│
        └───────────────────┘
```
```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│  Managers    │    │  Executives  │    │  Auditors    │
│  (Email UAR) │    │  (Dashboard) │    │  (Reports)   │
└──────┬───────┘    └──────┬───────┘    └──────┬───────┘
       │                   │ SignalR            │ Export
       │            ┌──────▼───────┐    ┌──────▼───────┐
       │            │ Blazor Server│    │ PDF/PPT Gen  │
       │            │  Dashboard   │    │(PuppeteerSharp│
       │            └──────┬───────┘    └──────────────┘
       │                   │
       │            ┌──────▼───────┐
       │            │  Data Service │◄── FileSystemWatcher (JSON)
       │            │  + API Client │◄── UAR Event API
       │            └──────┬───────┘
       │                   │
       │            ┌──────▼───────┐
       │            │  SQLite/SQL  │
       │            │  (Snapshots  │
       │            │   + Audits)  │
       │            └──────────────┘
       │
┌──────▼──────────────────────────────┐
│       UAR Platform (CoreIdentity)   │
│  Event triggers │ AI anomaly detect │
│  Automated revocation APIs          │
└─────────────────────────────────────┘
```
- **Keep Blazor Server, not Blazor WebAssembly.** Server-side rendering is correct here: the dashboard handles sensitive access data, benefits from SignalR push, and doesn't need offline capability. WASM would expose data processing logic to the client.
- **Data layering strategy:** Abstract `IDashboardDataService` is already in place. Add a second implementation (`ApiDashboardDataService`) that pulls from UAR APIs when ready, selected via configuration. No breaking changes to components.
- **Caching:** Use `IMemoryCache` (built-in) with a 30-second sliding expiration for API-sourced data. The FileSystemWatcher approach already handles cache invalidation for JSON data.
- **No microservices.** This is a read-only dashboard — a single Blazor Server process is sufficient. Avoid premature distribution.
- **API design:** If exposing data to other consumers, use minimal API endpoints (`app.MapGet`) rather than full controllers. Return DTOs matching the existing `DashboardData` model hierarchy. --- **Goal:** Make the existing dashboard production-quality for executive use.
- **Add MudBlazor** for polished UI components (data grids, tooltips, responsive improvements within the fixed 1920×1080 frame).
- **Add PuppeteerSharp** for automated screenshot generation — expose a `/api/screenshot` endpoint that returns a PNG.
- **Add structured logging** with Serilog to track data load events and errors.
- **Add bUnit tests** for all Razor components (Header, Timeline, Heatmap, HeatmapCell).
- **Add Dockerfile** for consistent deployment across environments. **Goal:** Connect to live UAR data for pilot teams (200–300 users).
- Implement `ApiDashboardDataService` behind feature flag.
- Add SQLite for local caching and historical snapshots.
- Add Entra ID authentication (`Microsoft.Identity.Web`).
- Add role-based authorization (Executive, Manager, Auditor views).
- Build UAR-specific dashboard components: trigger event timeline, access anomaly heatmap, compliance coverage gauge. **Goal:** Support parallel UAR operation across broader user base.
- Migrate to Azure SQL for centralized storage.
- Deploy to Azure App Service or Container Apps.
- Add Azure SignalR Service for connection scaling.
- Build automated report generation (PDF/PPT export on schedule).
- Add audit logging for all dashboard access (compliance evidence).
- Performance testing at target scale (100+ concurrent dashboard users).
- **Automated screenshots** via PuppeteerSharp — eliminates the manual browser screenshot workflow described in the README. High visibility, low effort.
- **Dark mode toggle** — executives present in various lighting conditions. MudBlazor theming makes this trivial.
- **JSON schema validation** — add a JSON Schema file and validate on load. Prevents silent data errors from hand-editing.
- **Health check endpoint** — `app.MapHealthChecks("/health")` for monitoring when deployed.
- **Real-time UAR event streaming to dashboard:** Prototype SignalR hub that receives UAR events and updates dashboard components incrementally. Validate that 50+ events/minute renders smoothly.
- **AI anomaly visualization:** The UAR spec mentions AI-based anomaly detection. Prototype how to visualize anomaly scores and peer-comparison data in the heatmap format.
- **Export pipeline:** Test PuppeteerSharp screenshot quality at 1920×1080 vs. Open XML SDK for native PowerPoint generation. Choose based on fidelity and maintenance cost.

### Recommended Tools & Technologies

- | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Framework** | Blazor Server (Interactive Server) | .NET 8.0 LTS | Already in use. SignalR-based real-time updates. | | **Component Library** | MudBlazor | 7.x | MIT license. Rich data grids, charts, theming. 4.5k+ GitHub stars. Alternative: Radzen Blazor (free tier). | | **Charting** | MudBlazor Charts or ApexCharts.Blazor | ApexCharts 3.x | For richer visualizations beyond the SVG timeline. ApexCharts.Blazor wraps ApexCharts.js. MIT license. | | **Icons** | MudBlazor built-in (Material Icons) | — | Bundled with MudBlazor. No additional dependency. | | **CSS Approach** | Scoped CSS (Blazor built-in) + app.css | — | Already in use. No need for Tailwind/Bootstrap. | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Runtime** | ASP.NET Core 8.0 | 8.0.x | LTS. Already in use. | | **Data Loading** | System.Text.Json | Built-in | Already in use via DashboardDataService. High performance, no extra dependency. | | **File Watching** | FileSystemWatcher + polling fallback | Built-in | Already implemented. Reliable for local JSON editing. | | **Configuration** | IOptions pattern + appsettings.json | Built-in | Standard .NET pattern. | | **Logging** | Microsoft.Extensions.Logging + Serilog | Serilog 4.x | Add Serilog for structured logging when audit trail integration begins. | | **HTTP Client** | IHttpClientFactory | Built-in | For future UAR API integration (CoreIdentity, anomaly detection signals). | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **MVP (Current)** | JSON file (`dashboard-data.json`) | — | Already in use. Appropriate for single-user, hand-edited data. | | **Phase 2** | SQLite via EF Core | EF Core 8.0, Microsoft.EntityFrameworkCore.Sqlite 8.0.x | For local persistence of historical snapshots, audit trails. Zero-server deployment. | | **Phase 3** | Azure SQL or Azure Cosmos DB | — | When multi-user access and UAR platform integration require centralized storage. Azure SQL for relational audit data; Cosmos DB if event-sourced. | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Containerization** | Docker (multi-stage build) | .NET 8 SDK image | `mcr.microsoft.com/dotnet/aspnet:8.0` for runtime. | | **CI/CD** | GitHub Actions | — | Already in repo (`.github/`). Use `dotnet build` / `dotnet test` / `dotnet publish`. | | **Hosting (Dev)** | Local Kestrel | Built-in | Current approach. Port 5000. | | **Hosting (Prod)** | Azure App Service (Linux) or Azure Container Apps | — | App Service for simplicity; Container Apps if scaling to multiple dashboard instances. | | **Screenshot Automation** | PuppeteerSharp | 19.x | .NET wrapper for headless Chromium. Automate 1920×1080 screenshot capture for PowerPoint exports. MIT license. | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Unit Testing** | xUnit | 2.9.x | .NET standard. Already likely in `ReportingDashboard.Tests`. | | **Blazor Component Testing** | bUnit | 1.31.x | Purpose-built for Blazor component testing. MIT license. 1.1k+ GitHub stars. | | **Mocking** | Moq or NSubstitute | Moq 4.20.x / NSubstitute 5.x | NSubstitute preferred (simpler API, no Castle.Core issues). | | **Integration Testing** | Microsoft.AspNetCore.Mvc.Testing | Built-in | For testing the full Blazor Server pipeline. | | **Snapshot Testing** | Verify | 26.x | Approval-based testing for rendered HTML output. Useful for regression-detecting layout changes. | ---

### Considerations & Risks

- | Concern | Recommendation | |---------|---------------| | **MVP** | None required. Dashboard is local-only, single-user. Current approach is correct. | | **Phase 2+** | Microsoft Entra ID (Azure AD) via `Microsoft.Identity.Web` 2.x. Mandatory when the dashboard displays UAR audit data for ~8,300 managers. | | **Authorization** | Role-based: `Executive` (full view), `Manager` (own-team view), `Auditor` (read-only + export). Use ASP.NET Core `[Authorize(Roles = "...")]`. | | **API auth** | Managed Identity for Azure-hosted services calling UAR APIs. No secrets in config. |
- **In transit:** HTTPS enforced via `app.UseHttpsRedirection()` (add to Program.cs for production).
- **At rest:** SQLite databases encrypted via SQLCipher if storing access review data locally. Azure SQL uses TDE by default.
- **PII handling:** UAR data contains employee names, roles, access levels. Classify as Microsoft Confidential. Ensure no PII in client-side logs or browser cache.
- **Audit logging:** Every data access should be logged with timestamp, user identity, and action for compliance evidence. | Scale | Hosting | Estimated Monthly Cost | |-------|---------|----------------------| | **Dev/Single-user** | Local Kestrel | $0 | | **Small (1–10 users)** | Azure App Service B1 (Linux) | ~$13/month | | **Medium (50–100 users)** | Azure App Service S1 + Azure SQL Basic | ~$70/month | | **Large (1,000+ users)** | Azure Container Apps + Azure SQL S2 | ~$200–400/month |
```yaml
# GitHub Actions workflow (simplified)
- dotnet restore
- dotnet build --configuration Release
- dotnet test --no-build
- dotnet publish -c Release -o ./publish
- Deploy to Azure App Service via az webapp deploy
``` --- | Risk | Severity | Mitigation | |------|----------|------------| | **SignalR connection limits** — Blazor Server holds a WebSocket per user. At 1,000+ concurrent users, this becomes expensive. | Medium | Monitor connection count. Plan migration to Blazor WebAssembly + API for high-scale scenarios. Azure SignalR Service ($50/month) for 1,000+ connections. | | **FileSystemWatcher reliability** — Known to miss events on network drives and some Linux filesystems. | Low | Already mitigated with polling fallback in current implementation. | | **UAR API dependency** — Dashboard becomes coupled to CoreIdentity APIs that are being built in parallel (Phase 1 pilot). | High | Abstract behind `IDashboardDataService`. Maintain JSON fallback. Use feature flags to toggle data sources. | | **Screenshot fidelity** — PuppeteerSharp headless rendering may differ slightly from real browser rendering. | Low | Pin Chromium version. Add visual regression tests with Verify. | | **Compliance scope creep** — If the dashboard becomes the "official" audit evidence viewer, it inherits compliance requirements (SOC 2 CC6.2, PCI DSS 7.2.4). | High | Keep the dashboard as a visualization layer only. Audit evidence of record should live in the UAR platform, not the dashboard. |
- **Blazor Server over WASM:** Accepts server resource cost per connection in exchange for simpler security model and faster initial load.
- **JSON file over database (MVP):** Accepts manual editing workflow in exchange for zero infrastructure.
- **MudBlazor over custom components:** Accepts bundle size increase (~300KB) in exchange for rapid UI development. ---
- **Data source integration timeline:** When will the UAR CoreIdentity APIs be available for the dashboard to consume? This determines when to implement `ApiDashboardDataService`.
- **Multi-tenancy requirements:** Will different service teams (~1,100) need isolated dashboard views, or is this a single executive view with filters?
- **Export format:** Is automated PowerPoint generation required, or are screenshots sufficient? If PPT, consider `DocumentFormat.OpenXml` (Open XML SDK) for native .pptx generation.
- **Refresh cadence:** Should the dashboard update in real-time (SignalR push on every UAR event) or on a scheduled cadence (hourly/daily aggregation)? Real-time at 12M access instances could be noisy.
- **Auditor access model:** Do external auditors need direct dashboard access, or will audit evidence be exported as static reports? This affects auth and hosting decisions.
- **Historical snapshots:** Should the dashboard support "show me the state as of March 15" time-travel, or only current state? Time-travel requires snapshot storage (SQLite/SQL).
- **Branding and theming:** Does the dashboard need to conform to Microsoft Fluent UI guidelines, or is the current custom CSS approach acceptable? ---

### Detailed Analysis

# Research: Technology Stack for ReportingDashboard

## Executive Summary

The ReportingDashboard is a Blazor Server (.NET 8) executive reporting application that visualizes project milestone timelines, monthly execution status, and key deliverables in a screenshot-friendly 1920×1080 format. The underlying project it tracks — the Automated Event-Driven User Access Review (UAR) system for Azure Core Trusted Platform — operates at significant scale (~1,100 services, ~50,000 users, ~12M privileged access instances) and must satisfy SOC 2, PCI DSS, FedRAMP, and NIST compliance controls.

**Primary recommendation:** Extend the existing Blazor Server / .NET 8 stack with targeted library additions for charting (Radzen/MudBlazor), data persistence (SQLite or Azure SQL), and structured export (PuppeteerSharp for automated screenshots). Keep the zero-external-dependency philosophy for the MVP but plan a clear upgrade path for multi-user scenarios and live data integration as the UAR platform matures through its 3-phase rollout.

---

## Key Findings

- The current architecture is intentionally minimal — Blazor Server, JSON-file-driven, no database, no auth, no cloud dependencies — which is appropriate for a single-user executive reporting tool but will not scale to multi-stakeholder UAR reporting across ~8,300 managers.
- Blazor Server's real-time SignalR push model is a strong fit for a live dashboard that reflects UAR event-driven triggers (role changes, anomaly signals) without polling.
- The UAR feature spec demands rich audit evidence (trigger, criteria, decision, remediation, timestamp) — the dashboard must eventually consume structured audit trail data, not just hand-edited JSON.
- .NET 8 is the current LTS release (supported through November 2026); .NET 9 is available but not LTS. Staying on .NET 8 is the correct choice for stability.
- The fixed 1920×1080 layout optimized for PowerPoint screenshots is a differentiating feature — automated screenshot generation via headless Chromium should be a priority enhancement.
- Compliance frameworks (SOC 2 CC6.2, PCI DSS 7.2.4, FedRAMP KSI-IAM-07) require audit evidence that the dashboard can help surface, but the dashboard itself must not become a compliance-critical system without proper auth and access logging.
- Blazor's component model maps cleanly to the dashboard's visual structure (Header, Timeline, Heatmap, HeatmapCell) — no architectural rework is needed.

---

## Recommended Technology Stack

### Frontend (UI Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Framework** | Blazor Server (Interactive Server) | .NET 8.0 LTS | Already in use. SignalR-based real-time updates. |
| **Component Library** | MudBlazor | 7.x | MIT license. Rich data grids, charts, theming. 4.5k+ GitHub stars. Alternative: Radzen Blazor (free tier). |
| **Charting** | MudBlazor Charts or ApexCharts.Blazor | ApexCharts 3.x | For richer visualizations beyond the SVG timeline. ApexCharts.Blazor wraps ApexCharts.js. MIT license. |
| **Icons** | MudBlazor built-in (Material Icons) | — | Bundled with MudBlazor. No additional dependency. |
| **CSS Approach** | Scoped CSS (Blazor built-in) + app.css | — | Already in use. No need for Tailwind/Bootstrap. |

### Backend (Application Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Runtime** | ASP.NET Core 8.0 | 8.0.x | LTS. Already in use. |
| **Data Loading** | System.Text.Json | Built-in | Already in use via DashboardDataService. High performance, no extra dependency. |
| **File Watching** | FileSystemWatcher + polling fallback | Built-in | Already implemented. Reliable for local JSON editing. |
| **Configuration** | IOptions pattern + appsettings.json | Built-in | Standard .NET pattern. |
| **Logging** | Microsoft.Extensions.Logging + Serilog | Serilog 4.x | Add Serilog for structured logging when audit trail integration begins. |
| **HTTP Client** | IHttpClientFactory | Built-in | For future UAR API integration (CoreIdentity, anomaly detection signals). |

### Data Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **MVP (Current)** | JSON file (`dashboard-data.json`) | — | Already in use. Appropriate for single-user, hand-edited data. |
| **Phase 2** | SQLite via EF Core | EF Core 8.0, Microsoft.EntityFrameworkCore.Sqlite 8.0.x | For local persistence of historical snapshots, audit trails. Zero-server deployment. |
| **Phase 3** | Azure SQL or Azure Cosmos DB | — | When multi-user access and UAR platform integration require centralized storage. Azure SQL for relational audit data; Cosmos DB if event-sourced. |

### Infrastructure & DevOps

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Containerization** | Docker (multi-stage build) | .NET 8 SDK image | `mcr.microsoft.com/dotnet/aspnet:8.0` for runtime. |
| **CI/CD** | GitHub Actions | — | Already in repo (`.github/`). Use `dotnet build` / `dotnet test` / `dotnet publish`. |
| **Hosting (Dev)** | Local Kestrel | Built-in | Current approach. Port 5000. |
| **Hosting (Prod)** | Azure App Service (Linux) or Azure Container Apps | — | App Service for simplicity; Container Apps if scaling to multiple dashboard instances. |
| **Screenshot Automation** | PuppeteerSharp | 19.x | .NET wrapper for headless Chromium. Automate 1920×1080 screenshot capture for PowerPoint exports. MIT license. |

### Testing

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | 2.9.x | .NET standard. Already likely in `ReportingDashboard.Tests`. |
| **Blazor Component Testing** | bUnit | 1.31.x | Purpose-built for Blazor component testing. MIT license. 1.1k+ GitHub stars. |
| **Mocking** | Moq or NSubstitute | Moq 4.20.x / NSubstitute 5.x | NSubstitute preferred (simpler API, no Castle.Core issues). |
| **Integration Testing** | Microsoft.AspNetCore.Mvc.Testing | Built-in | For testing the full Blazor Server pipeline. |
| **Snapshot Testing** | Verify | 26.x | Approval-based testing for rendered HTML output. Useful for regression-detecting layout changes. |

---

## Architecture Recommendations

### Current Architecture (Appropriate for MVP)

```
┌─────────────────────────────────────────────┐
│              Browser (1920×1080)             │
│  ┌─────────┐ ┌──────────┐ ┌──────────────┐  │
│  │ Header  │ │ Timeline │ │   Heatmap    │  │
│  └─────────┘ └──────────┘ └──────────────┘  │
└──────────────────┬──────────────────────────┘
                   │ SignalR (WebSocket)
┌──────────────────▼──────────────────────────┐
│           Blazor Server (.NET 8)            │
│  ┌──────────────────────────────────────┐   │
│  │      DashboardDataService            │   │
│  │  (JSON load + FileSystemWatcher)     │   │
│  └──────────────┬───────────────────────┘   │
└─────────────────┼───────────────────────────┘
                  │ File I/O
        ┌─────────▼─────────┐
        │ dashboard-data.json│
        └───────────────────┘
```

### Target Architecture (Phase 2–3, UAR Integration)

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│  Managers    │    │  Executives  │    │  Auditors    │
│  (Email UAR) │    │  (Dashboard) │    │  (Reports)   │
└──────┬───────┘    └──────┬───────┘    └──────┬───────┘
       │                   │ SignalR            │ Export
       │            ┌──────▼───────┐    ┌──────▼───────┐
       │            │ Blazor Server│    │ PDF/PPT Gen  │
       │            │  Dashboard   │    │(PuppeteerSharp│
       │            └──────┬───────┘    └──────────────┘
       │                   │
       │            ┌──────▼───────┐
       │            │  Data Service │◄── FileSystemWatcher (JSON)
       │            │  + API Client │◄── UAR Event API
       │            └──────┬───────┘
       │                   │
       │            ┌──────▼───────┐
       │            │  SQLite/SQL  │
       │            │  (Snapshots  │
       │            │   + Audits)  │
       │            └──────────────┘
       │
┌──────▼──────────────────────────────┐
│       UAR Platform (CoreIdentity)   │
│  Event triggers │ AI anomaly detect │
│  Automated revocation APIs          │
└─────────────────────────────────────┘
```

### Key Architectural Decisions

1. **Keep Blazor Server, not Blazor WebAssembly.** Server-side rendering is correct here: the dashboard handles sensitive access data, benefits from SignalR push, and doesn't need offline capability. WASM would expose data processing logic to the client.

2. **Data layering strategy:** Abstract `IDashboardDataService` is already in place. Add a second implementation (`ApiDashboardDataService`) that pulls from UAR APIs when ready, selected via configuration. No breaking changes to components.

3. **Caching:** Use `IMemoryCache` (built-in) with a 30-second sliding expiration for API-sourced data. The FileSystemWatcher approach already handles cache invalidation for JSON data.

4. **No microservices.** This is a read-only dashboard — a single Blazor Server process is sufficient. Avoid premature distribution.

5. **API design:** If exposing data to other consumers, use minimal API endpoints (`app.MapGet`) rather than full controllers. Return DTOs matching the existing `DashboardData` model hierarchy.

---

## Security & Infrastructure

### Authentication & Authorization

| Concern | Recommendation |
|---------|---------------|
| **MVP** | None required. Dashboard is local-only, single-user. Current approach is correct. |
| **Phase 2+** | Microsoft Entra ID (Azure AD) via `Microsoft.Identity.Web` 2.x. Mandatory when the dashboard displays UAR audit data for ~8,300 managers. |
| **Authorization** | Role-based: `Executive` (full view), `Manager` (own-team view), `Auditor` (read-only + export). Use ASP.NET Core `[Authorize(Roles = "...")]`. |
| **API auth** | Managed Identity for Azure-hosted services calling UAR APIs. No secrets in config. |

### Data Protection

- **In transit:** HTTPS enforced via `app.UseHttpsRedirection()` (add to Program.cs for production).
- **At rest:** SQLite databases encrypted via SQLCipher if storing access review data locally. Azure SQL uses TDE by default.
- **PII handling:** UAR data contains employee names, roles, access levels. Classify as Microsoft Confidential. Ensure no PII in client-side logs or browser cache.
- **Audit logging:** Every data access should be logged with timestamp, user identity, and action for compliance evidence.

### Hosting & Deployment

| Scale | Hosting | Estimated Monthly Cost |
|-------|---------|----------------------|
| **Dev/Single-user** | Local Kestrel | $0 |
| **Small (1–10 users)** | Azure App Service B1 (Linux) | ~$13/month |
| **Medium (50–100 users)** | Azure App Service S1 + Azure SQL Basic | ~$70/month |
| **Large (1,000+ users)** | Azure Container Apps + Azure SQL S2 | ~$200–400/month |

### Deployment Strategy

```yaml
# GitHub Actions workflow (simplified)
- dotnet restore
- dotnet build --configuration Release
- dotnet test --no-build
- dotnet publish -c Release -o ./publish
- Deploy to Azure App Service via az webapp deploy
```

---

## Risks & Trade-offs

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **SignalR connection limits** — Blazor Server holds a WebSocket per user. At 1,000+ concurrent users, this becomes expensive. | Medium | Monitor connection count. Plan migration to Blazor WebAssembly + API for high-scale scenarios. Azure SignalR Service ($50/month) for 1,000+ connections. |
| **FileSystemWatcher reliability** — Known to miss events on network drives and some Linux filesystems. | Low | Already mitigated with polling fallback in current implementation. |
| **UAR API dependency** — Dashboard becomes coupled to CoreIdentity APIs that are being built in parallel (Phase 1 pilot). | High | Abstract behind `IDashboardDataService`. Maintain JSON fallback. Use feature flags to toggle data sources. |
| **Screenshot fidelity** — PuppeteerSharp headless rendering may differ slightly from real browser rendering. | Low | Pin Chromium version. Add visual regression tests with Verify. |
| **Compliance scope creep** — If the dashboard becomes the "official" audit evidence viewer, it inherits compliance requirements (SOC 2 CC6.2, PCI DSS 7.2.4). | High | Keep the dashboard as a visualization layer only. Audit evidence of record should live in the UAR platform, not the dashboard. |

### Trade-offs Accepted

1. **Blazor Server over WASM:** Accepts server resource cost per connection in exchange for simpler security model and faster initial load.
2. **JSON file over database (MVP):** Accepts manual editing workflow in exchange for zero infrastructure.
3. **MudBlazor over custom components:** Accepts bundle size increase (~300KB) in exchange for rapid UI development.

---

## Open Questions

1. **Data source integration timeline:** When will the UAR CoreIdentity APIs be available for the dashboard to consume? This determines when to implement `ApiDashboardDataService`.

2. **Multi-tenancy requirements:** Will different service teams (~1,100) need isolated dashboard views, or is this a single executive view with filters?

3. **Export format:** Is automated PowerPoint generation required, or are screenshots sufficient? If PPT, consider `DocumentFormat.OpenXml` (Open XML SDK) for native .pptx generation.

4. **Refresh cadence:** Should the dashboard update in real-time (SignalR push on every UAR event) or on a scheduled cadence (hourly/daily aggregation)? Real-time at 12M access instances could be noisy.

5. **Auditor access model:** Do external auditors need direct dashboard access, or will audit evidence be exported as static reports? This affects auth and hosting decisions.

6. **Historical snapshots:** Should the dashboard support "show me the state as of March 15" time-travel, or only current state? Time-travel requires snapshot storage (SQLite/SQL).

7. **Branding and theming:** Does the dashboard need to conform to Microsoft Fluent UI guidelines, or is the current custom CSS approach acceptable?

---

## Implementation Recommendations

### Phase 1: Enhance Current MVP (2–3 weeks)

**Goal:** Make the existing dashboard production-quality for executive use.

- **Add MudBlazor** for polished UI components (data grids, tooltips, responsive improvements within the fixed 1920×1080 frame).
- **Add PuppeteerSharp** for automated screenshot generation — expose a `/api/screenshot` endpoint that returns a PNG.
- **Add structured logging** with Serilog to track data load events and errors.
- **Add bUnit tests** for all Razor components (Header, Timeline, Heatmap, HeatmapCell).
- **Add Dockerfile** for consistent deployment across environments.

### Phase 2: Data Integration (4–6 weeks, aligned with UAR Phase 1 Pilot)

**Goal:** Connect to live UAR data for pilot teams (200–300 users).

- Implement `ApiDashboardDataService` behind feature flag.
- Add SQLite for local caching and historical snapshots.
- Add Entra ID authentication (`Microsoft.Identity.Web`).
- Add role-based authorization (Executive, Manager, Auditor views).
- Build UAR-specific dashboard components: trigger event timeline, access anomaly heatmap, compliance coverage gauge.

### Phase 3: Scale & Compliance (6–8 weeks, aligned with UAR Phase 2 Parallel Operation)

**Goal:** Support parallel UAR operation across broader user base.

- Migrate to Azure SQL for centralized storage.
- Deploy to Azure App Service or Container Apps.
- Add Azure SignalR Service for connection scaling.
- Build automated report generation (PDF/PPT export on schedule).
- Add audit logging for all dashboard access (compliance evidence).
- Performance testing at target scale (100+ concurrent dashboard users).

### Quick Wins

1. **Automated screenshots** via PuppeteerSharp — eliminates the manual browser screenshot workflow described in the README. High visibility, low effort.
2. **Dark mode toggle** — executives present in various lighting conditions. MudBlazor theming makes this trivial.
3. **JSON schema validation** — add a JSON Schema file and validate on load. Prevents silent data errors from hand-editing.
4. **Health check endpoint** — `app.MapHealthChecks("/health")` for monitoring when deployed.

### Areas Requiring Prototyping

- **Real-time UAR event streaming to dashboard:** Prototype SignalR hub that receives UAR events and updates dashboard components incrementally. Validate that 50+ events/minute renders smoothly.
- **AI anomaly visualization:** The UAR spec mentions AI-based anomaly detection. Prototype how to visualize anomaly scores and peer-comparison data in the heatmap format.
- **Export pipeline:** Test PuppeteerSharp screenshot quality at 1920×1080 vs. Open XML SDK for native PowerPoint generation. Choose based on fidelity and maintenance cost.
