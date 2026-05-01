# PM Specification: ReportingDashboard

## Executive Summary

The ReportingDashboard is an executive reporting application that visualizes project milestone timelines, monthly execution status, and key deliverables for the Automated Event-Driven User Access Review (UAR) system within Azure Core Trusted Platform. Built on Blazor Server (.NET 8), it renders a fixed 1920×1080 layout optimized for PowerPoint screenshot capture, and is designed to evolve from a single-user JSON-driven tool into a multi-stakeholder, compliance-aware reporting platform serving ~8,300 managers across ~1,100 services.

## Business Goals

1. **Provide executive visibility into UAR program execution** — deliver a single-pane-of-glass view of milestone progress, monthly status, and key deliverables for the UAR initiative spanning ~50,000 users and ~12M privileged access instances.
2. **Eliminate manual screenshot workflows** — automate the capture of dashboard views at 1920×1080 resolution for direct inclusion in PowerPoint executive briefings.
3. **Surface compliance evidence** — visualize audit trail data (trigger, criteria, decision, remediation, timestamp) required by SOC 2 CC6.2, PCI DSS 7.2.4, FedRAMP KSI-IAM-07, and NIST controls without the dashboard itself becoming a compliance-critical system.
4. **Enable phased scaling** — evolve from a local, zero-dependency MVP to a multi-user, authenticated, cloud-hosted dashboard aligned with the UAR platform's 3-phase rollout (Pilot → Parallel Operation → Full Deployment).
5. **Reduce reporting cycle time** — move from hand-edited JSON and manual report assembly to live or near-live data integration with UAR CoreIdentity APIs, cutting executive report preparation from hours to minutes.
6. **Support role-based stakeholder access** — provide differentiated views for Executives (full program view), Managers (own-team access reviews), and Auditors (read-only export) as the platform matures.

## User Stories & Acceptance Criteria

### US-1: View Project Milestone Timeline

**As an** executive, I want to see a visual timeline of UAR project milestones with current status indicators, so that I can assess program progress at a glance.

- [ ] Timeline component renders all milestones from data source with start/end dates
- [ ] Each milestone displays status (Not Started, In Progress, Complete, At Risk) with distinct color coding
- [ ] Timeline fits within the 1920×1080 fixed layout without scrolling
- [ ] Current date marker is visible on the timeline
- [ ] Milestone data updates automatically when `dashboard-data.json` changes (via FileSystemWatcher)

### US-2: View Monthly Execution Heatmap

**As an** executive, I want to see a month-by-month heatmap of execution status across workstreams, so that I can identify problem areas requiring intervention.

- [ ] Heatmap renders rows (workstreams) × columns (months) grid with color-coded cells
- [ ] Cell colors correspond to status values (Green = On Track, Yellow = At Risk, Red = Blocked, Grey = Not Started)
- [ ] Hovering over a cell displays a tooltip with details (owner, notes, key risks)
- [ ] Heatmap renders within the Header → Timeline → Heatmap visual hierarchy at 1920×1080

### US-3: Automated Screenshot Export

**As an** executive, I want to generate a pixel-perfect 1920×1080 PNG screenshot of the dashboard via an API call, so that I can embed it directly into PowerPoint without manual browser screenshots.

- [ ] `GET /api/screenshot` endpoint returns a PNG image at exactly 1920×1080 resolution
- [ ] Screenshot captures the full dashboard layout as rendered in a browser
- [ ] Response time is under 10 seconds for screenshot generation
- [ ] Screenshot output matches browser rendering with no significant visual discrepancies
- [ ] Endpoint is accessible without authentication in MVP phase

### US-4: Edit Dashboard Data via JSON

**As a** program manager, I want to update dashboard data by editing a JSON file, so that I can refresh the executive view without needing a database or deployment.

- [ ] `dashboard-data.json` schema supports milestones, monthly status, deliverables, and metadata
- [ ] Changes to the JSON file are reflected in the dashboard within 5 seconds
- [ ] Malformed JSON displays a user-friendly error message rather than crashing the application
- [ ] JSON schema validation runs on file load and reports specific validation errors via structured logging

### US-5: Dark Mode Toggle

**As an** executive, I want to toggle between light and dark display themes, so that the dashboard is readable in various presentation environments.

- [ ] A toggle control switches the entire dashboard between light and dark themes
- [ ] Theme preference persists across browser sessions (local storage)
- [ ] Both themes maintain readability and color contrast for all status indicators
- [ ] Screenshots via `/api/screenshot` respect the currently active theme (or accept a query parameter)

### US-6: Authenticated Multi-User Access (Phase 2)

**As a** platform administrator, I want users to authenticate via Microsoft Entra ID, so that only authorized personnel can view sensitive UAR data.

- [ ] Dashboard requires Entra ID sign-in when authentication is enabled
- [ ] Authentication can be toggled on/off via configuration (feature flag)
- [ ] Authenticated user identity is captured in all audit log entries
- [ ] Unauthenticated requests receive a 401 response and redirect to login

### US-7: Role-Based Dashboard Views (Phase 2)

**As a** manager, I want to see only my team's access review data, so that I am not overwhelmed by organization-wide information irrelevant to my responsibilities.

- [ ] Executive role sees full program-level dashboard (all workstreams, all teams)
- [ ] Manager role sees filtered view scoped to their direct reports and team services
- [ ] Auditor role sees read-only view with export capability but no data modification
- [ ] Role assignment is driven by Entra ID group membership
- [ ] Unauthorized role access returns a 403 Forbidden response

### US-8: Live UAR Data Integration (Phase 2)

**As an** executive, I want the dashboard to pull data from UAR CoreIdentity APIs, so that the view reflects real-time access review status rather than manually maintained JSON.

- [ ] `ApiDashboardDataService` implementation retrieves data from UAR event APIs
- [ ] Data source (JSON vs. API) is selectable via configuration/feature flag
- [ ] API-sourced data is cached with 30-second sliding expiration via `IMemoryCache`
- [ ] JSON fallback remains fully functional when API is unavailable
- [ ] Dashboard components require no changes when switching data sources

### US-9: Historical Snapshot Retrieval (Phase 2)

**As an** auditor, I want to view the dashboard state as of a specific past date, so that I can provide point-in-time compliance evidence.

- [ ] Dashboard snapshots are persisted to SQLite on a configurable schedule (at minimum daily)
- [ ] A date picker allows selection of any historical snapshot date
- [ ] Historical view is clearly labeled with the snapshot timestamp
- [ ] Snapshot data is read-only and cannot be modified

### US-10: Automated Report Generation (Phase 3)

**As an** executive, I want the dashboard to automatically generate and distribute PDF/PPT reports on a schedule, so that stakeholders receive updates without manually visiting the dashboard.

- [ ] Scheduled report generation runs at configurable intervals (daily, weekly)
- [ ] Reports are generated as PNG screenshots or native PPT files
- [ ] Reports are stored and accessible via a download endpoint
- [ ] Report generation failures are logged and do not affect live dashboard availability

## Scope

### In Scope

- Blazor Server (.NET 8) dashboard with fixed 1920×1080 layout
- Project milestone timeline visualization component
- Monthly execution status heatmap with color-coded cells and tooltips
- Header component with project metadata and key deliverables
- JSON file-based data loading with FileSystemWatcher hot-reload
- JSON schema validation on data load
- Automated screenshot generation via PuppeteerSharp (`/api/screenshot` endpoint)
- MudBlazor integration for polished UI components and dark/light theming
- Structured logging with Serilog
- bUnit component tests for all Razor components
- Dockerfile for containerized deployment
- Health check endpoint (`/health`)
- Phase 2: `ApiDashboardDataService` with feature flag toggle
- Phase 2: SQLite persistence for historical snapshots
- Phase 2: Entra ID authentication via `Microsoft.Identity.Web`
- Phase 2: Role-based authorization (Executive, Manager, Auditor)
- Phase 3: Azure SQL migration for centralized storage
- Phase 3: Azure App Service / Container Apps deployment
- Phase 3: Azure SignalR Service for connection scaling
- Phase 3: Automated PDF/PPT report generation on schedule
- Phase 3: Audit logging for all dashboard access

### Out of Scope

- **Write-back to UAR platform** — the dashboard is strictly read-only and does not initiate access reviews, revocations, or any UAR workflow actions
- **Mobile-responsive layouts** — the dashboard is fixed at 1920×1080; mobile or tablet optimization is not planned
- **Blazor WebAssembly migration** — Blazor Server is the chosen rendering model for all phases
- **Custom charting library development** — use MudBlazor/ApexCharts; no bespoke SVG charting framework
- **Multi-tenant data isolation at the database level** — role-based filtering is in scope; physical tenant separation is not
- **Real-time collaboration features** — no simultaneous editing, commenting, or annotation capabilities
- **External auditor self-service portal** — auditors receive exported reports; direct dashboard access is an open question
- **AI anomaly detection logic** — the dashboard may visualize anomaly scores from UAR APIs but does not perform its own anomaly detection
- **Compliance certification of the dashboard itself** — the dashboard surfaces compliance evidence but is not itself a SOC 2/PCI/FedRAMP certified system

## Non-Functional Requirements

### Performance

| Metric | Target |
|--------|--------|
| Initial page load (Blazor Server) | < 2 seconds on LAN |
| Data refresh after JSON file change | < 5 seconds |
| Screenshot generation (`/api/screenshot`) | < 10 seconds |
| API data fetch + render (Phase 2) | < 3 seconds with 30s cache |
| Concurrent users (Phase 3) | 100+ simultaneous WebSocket connections |
| SignalR event throughput | 50+ UAR events/minute rendered smoothly |

### Security

- HTTPS enforced in all non-local environments via `app.UseHttpsRedirection()`
- No PII in client-side logs, browser cache, or console output
- UAR data classified as Microsoft Confidential
- SQLite encrypted via SQLCipher when storing access review data locally (Phase 2)
- Azure SQL uses Transparent Data Encryption (TDE) by default (Phase 3)
- Managed Identity for all Azure-hosted service-to-service authentication — no secrets in configuration
- All data access logged with timestamp, user identity, and action (Phase 2+)

### Scalability

| Scale Tier | Architecture | Target Users |
|-----------|-------------|-------------|
| MVP | Local Kestrel, JSON file | 1 (single-user) |
| Small | Azure App Service B1 | 1–10 |
| Medium | Azure App Service S1 + Azure SQL Basic | 50–100 |
| Large | Azure Container Apps + Azure SQL S2 + Azure SignalR Service | 1,000+ |

### Reliability

- Dashboard must gracefully degrade if the UAR API is unavailable (fall back to cached or JSON data)
- FileSystemWatcher failures must not crash the application; polling fallback is required
- Screenshot endpoint failures return a 500 with structured error details, not an unhandled exception
- Health check endpoint (`/health`) returns 200 when the application is operational

## Success Metrics

1. **MVP delivery** — Dashboard renders milestone timeline, heatmap, and header from JSON data at 1920×1080 with < 2s load time; verified by bUnit tests passing for all four core components (Header, Timeline, Heatmap, HeatmapCell).
2. **Screenshot automation adoption** — `/api/screenshot` endpoint produces pixel-accurate PNGs that are used in at least 2 executive briefing cycles, replacing manual browser screenshots.
3. **Phase 2 data integration** — `ApiDashboardDataService` successfully retrieves and renders live UAR data from CoreIdentity APIs for pilot teams (200–300 users) with feature flag enabled; JSON fallback verified functional when flag is disabled.
4. **Authentication rollout** — Entra ID authentication enabled with zero unauthorized access incidents; 100% of dashboard access logged with user identity.
5. **Phase 3 scale target** — Dashboard supports 100+ concurrent users on Azure Container Apps with < 3s page load under load test; Azure SignalR Service maintains stable WebSocket connections.
6. **Report automation** — Automated weekly reports generated and distributed for at least 4 consecutive weeks without manual intervention or failure.
7. **Compliance evidence** — Audit log covers 100% of data access events with required fields (timestamp, identity, action); auditors confirm the dashboard output is accepted as supplementary compliance evidence.

## Constraints & Assumptions

### Technical Constraints

- **Framework locked to .NET 8 LTS** — no migration to .NET 9 until .NET 10 LTS is available (November 2025+); .NET 8 support extends through November 2026.
- **Fixed 1920×1080 layout** — the dashboard is designed for a single resolution to ensure screenshot fidelity; responsive design is explicitly excluded.
- **Blazor Server only** — WebSocket connection per user is an accepted cost; no WASM migration planned within this specification's scope.
- **JSON file as MVP data source** — the `dashboard-data.json` hand-editing workflow is retained until UAR APIs are available.
- **PuppeteerSharp requires headless Chromium** — deployment environments must support Chromium binary installation (~300MB); serverless hosting (e.g., Azure Functions) is not viable for screenshot generation.

### Timeline Assumptions

- **Phase 1 (MVP Enhancement):** 2–3 weeks — MudBlazor integration, PuppeteerSharp screenshot endpoint, Serilog logging, bUnit tests, Dockerfile.
- **Phase 2 (Data Integration):** 4–6 weeks — aligned with UAR Phase 1 Pilot availability; dependent on CoreIdentity API readiness.
- **Phase 3 (Scale & Compliance):** 6–8 weeks — aligned with UAR Phase 2 Parallel Operation; dependent on Azure subscription provisioning and SignalR Service availability.

### Dependency Assumptions

- UAR CoreIdentity APIs will expose structured data (trigger, criteria, decision, remediation, timestamp) consumable via REST by Phase 2 start.
- Entra ID app registration and group-based role assignment will be approved by identity team within 2 weeks of Phase 2 kickoff.
- Azure subscription with sufficient quota for App Service/Container Apps and Azure SQL will be available for Phase 3.
- The existing `IDashboardDataService` abstraction is stable and will not require interface changes for API integration.
- GitHub Actions CI/CD pipeline in the repository is functional and will be extended (not replaced) for containerized deployment.

### Open Dependencies Requiring Resolution

| Dependency | Owner | Decision Needed By |
|-----------|-------|-------------------|
| UAR CoreIdentity API availability and schema | CoreIdentity team | Phase 2 start |
| Multi-tenancy model (filters vs. isolated views) | PM + Architecture | Phase 2 design |
| Export format (PNG screenshots vs. native PPT) | PM + Executives | Phase 1 end |
| Dashboard refresh cadence (real-time vs. scheduled) | PM + Engineering | Phase 2 design |
| External auditor access model (direct vs. export-only) | PM + Compliance | Phase 2 design |
| Fluent UI branding compliance requirement | PM + Design | Phase 1 start |