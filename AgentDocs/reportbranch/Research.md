# Research

_No research has been documented yet._

## Research technology stack for ReportingDashboard

_Researched on 2026-05-01 09:01 UTC_

### Summary

The ReportingDashboard is a single-page Blazor Server application that renders project milestone timelines, monthly execution heatmaps, and key deliverables into a fixed 1920×1080 layout optimized for PowerPoint screenshots. The existing codebase already runs on .NET 8 with Blazor Interactive Server render mode, System.Text.Json for data binding, and FileSystemWatcher for live-reload — with zero external NuGet dependencies. **Primary recommendation**: Evolve the existing Blazor Server architecture incrementally. The current zero-dependency, JSON-file-driven design is a strength — not a limitation. Enhancements should focus on (1) adding charting/visualization libraries for richer data presentation, (2) structured testing with bUnit, (3) optional PDF/image export for automated screenshot generation, and (4) a lightweight API layer if multi-user access is needed. Do not migrate to Blazor WASM or add a database unless requirements explicitly demand multi-user concurrent editing or cloud hosting. ---

### Key Findings

- The existing application is fully functional with zero external NuGet packages beyond the default Blazor Server template — an unusually clean dependency surface that should be preserved where possible.
- Blazor Server Interactive render mode with SignalR is the correct choice for this use case: real-time updates when JSON data changes, server-side SVG rendering, and no WASM download penalty for a tool used intermittently by executives.
- The JSON-file-as-database pattern (`dashboard-data.json` + `FileSystemWatcher`) is appropriate for single-user local tooling but becomes a bottleneck if the dashboard needs to serve multiple concurrent users or run in a containerized environment.
- SVG-based timeline rendering in `Timeline.razor` is the right approach for pixel-perfect, scalable visualizations at fixed 1920×1080 — CSS canvas or third-party chart libraries would add complexity without benefit for this layout.
- The `DashboardDataService` already implements debounced reload, retry-on-IOException, and graceful fallback to previous valid data — production-quality resilience for a file-based data source.
- bUnit (the standard Blazor component testing library) is absent from the test project and should be added for component-level testing of `Timeline.razor`, `Heatmap.razor`, and `Header.razor`.
- The .NET 8 LTS target is correct; .NET 9 (released Nov 2024) offers incremental Blazor improvements but is STS (Standard Term Support) — migration should wait for .NET 10 LTS (Nov 2025) unless a specific .NET 9 feature is needed.
- No authentication, authorization, or cloud hosting is needed for the current local-only use case; adding these prematurely would violate YAGNI.
- Automated screenshot/PDF export via Playwright for .NET or PuppeteerSharp would eliminate the manual browser screenshot workflow described in the README. ---
- **Add bUnit to `ReportingDashboard.Tests`**:
   ```xml
   <PackageReference Include="bunit" Version="1.31.3" />
   ``` Write component tests for `Timeline.razor` (SVG output correctness), `Heatmap.razor` (cell rendering), and `Header.razor` (data binding).
- **Add JSON schema validation**: Create a `DashboardDataValidator` that runs on load and reports specific errors (e.g., "Track M1 has milestone date '2025-13-01' which is not a valid date").
- **Add a health-check endpoint**: `app.MapGet("/health", () => Results.Ok())` — trivial but useful for automation.
- **Add Playwright for .NET** (`Microsoft.Playwright` 1.47.x) as a dev/CLI dependency
- **Create a `ScreenshotExporter` service** or CLI command that:
- Launches headless Chromium at 1920×1080
- Navigates to `http://localhost:5000`
- Waits for data load (check for a CSS class on the rendered dashboard)
- Captures PNG and optionally PDF
- Saves to a configurable output path
- **Wire into CI**: GitHub Actions job that runs on `dashboard-data.json` changes → produces screenshot artifact
- Extend `DashboardDataService` to accept a project name parameter
- Watch `wwwroot/data/*.json` directory instead of a single file
- Add route parameter: `Dashboard.razor` at `/{projectName?}`
- Add a simple project switcher dropdown in `Header.razor`
- **Playwright screenshot quality**: Prototype the headless screenshot flow to verify SVG rendering fidelity, font rendering, and color accuracy match the manual browser screenshot. Playwright's Chromium may render fonts slightly differently than Edge/Chrome.
- **PDF export**: If PDF is needed, prototype both Playwright PDF and QuestPDF server-side rendering to compare output quality and complexity.
- **Do not add a database.** The JSON file model works. A database adds migration tooling, connection management, and deployment complexity for zero user benefit.
- **Do not add authentication** unless the dashboard will be network-accessible.
- **Do not switch to Blazor WASM.** Server-side rendering with SignalR push on file change is the correct architecture for live-reloading local data.
- **Do not add a CSS framework** (Bootstrap, Tailwind). The existing custom CSS is purpose-built for 1920×1080 fixed layout. A framework would fight this constraint.

### Recommended Tools & Technologies

- > **Project**: Executive Reporting Dashboard — Milestone Timeline & Execution Status Visualization > **Date**: May 2026 > **Status**: Research Complete > **Mandatory Stack**: .NET 8 / Blazor Server / C# 12 --- | Component | Choice | Version | Rationale | |-----------|--------|---------|-----------| | **Runtime** | .NET 8 LTS | 8.0.x (latest patch) | Already in use; LTS support through Nov 2026 | | **Web Framework** | Blazor Server (Interactive) | Ships with .NET 8 | Real-time SignalR updates, server-rendered SVG, no WASM payload | | **Language** | C# 12 | Ships with .NET 8 | File-scoped namespaces, records, nullable refs already in use | | **Serialization** | System.Text.Json | Built-in | Already in use; supports comments and trailing commas via options | | Library | Version | Purpose | License | Alternative | |---------|---------|---------|---------|-------------| | **None (keep SVG)** | N/A | Timeline rendering | N/A | Custom SVG in `Timeline.razor` is already superior to chart libraries for this fixed layout | | **MudBlazor** | 7.x | Only if expanding beyond current scope (tables, dialogs, theming) | MIT | Radzen Blazor (free tier), Blazorise | | **SkiaSharp** | 2.88.x | Server-side image generation if automated PNG export is needed | MIT | ImageSharp (SixLabors, split license) | **Opinionated stance**: Do NOT add a charting library (ApexCharts.Blazor, ChartJs.Blazor, Radzen charts). The current hand-crafted SVG approach in `Timeline.razor` gives pixel-perfect control at 1920×1080 that no charting library can match for this fixed-layout, screenshot-optimized use case. | Tool | Version | Purpose | |------|---------|---------| | **xUnit** | 2.9.x | Test runner (already in use across AgentSquad) | | **bUnit** | 1.31.x+ | Blazor component unit testing — **currently missing, should add** | | **FluentAssertions** | 7.x | Readable assertions | | **Playwright for .NET** | 1.47.x | Automated browser screenshot/PDF export and E2E testing | | **Verify.Blazor** | 26.x | Snapshot testing for rendered component output | | Tool | Purpose | |------|---------| | **dotnet CLI** | Build, test, publish — already configured | | **GitHub Actions** | CI pipeline (repo already uses GitHub) | | **dotnet publish --self-contained** | Single-file deployment for local distribution | | Component | Choice | Notes | |-----------|--------|-------| | **Hosting** | Local Kestrel | `localhost:5000`, no reverse proxy needed | | **Data store** | JSON file (`dashboard-data.json`) | FileSystemWatcher + polling for live reload | | **CDN/Cloud** | None | Local-only tool; no cloud dependency | ---
```
┌─────────────────────────────────────────────────┐
│  Browser (1920×1080 fixed viewport)              │
│  ┌─────────────────────────────────────────────┐ │
│  │  Dashboard.razor (route: /)                  │ │
│  │  ├── Header.razor (title, subtitle, legend)  │ │
│  │  ├── Timeline.razor (SVG milestone tracks)   │ │
│  │  └── Heatmap.razor → HeatmapCell.razor       │ │
│  └─────────────────────────────────────────────┘ │
│         ▲ SignalR (real-time re-render)           │
└─────────┼───────────────────────────────────────┘
          │
┌─────────┼───────────────────────────────────────┐
│  Blazor Server (Kestrel, localhost:5000)         │
│  ├── DashboardDataService (singleton)            │
│  │   ├── FileSystemWatcher (primary)             │
│  │   ├── Timer polling (5s fallback)             │
│  │   └── Debounced reload (300ms)                │
│  └── JSON file: wwwroot/data/dashboard-data.json │
└─────────────────────────────────────────────────┘
```
- **Service-Component separation**: `DashboardDataService` owns all data loading/caching; Razor components are pure renderers. Preserve this.
- **Event-driven re-render**: `OnDataChanged` event triggers component re-render via SignalR without full page reload. This is the correct Blazor Server pattern.
- **Graceful degradation**: On JSON parse error, previous valid data persists. This is production-quality behavior.
- **Fixed-viewport rendering**: CSS `overflow: hidden` at 1920×1080 ensures screenshot fidelity. Do not add responsive breakpoints.
- **Automated export endpoint**: Add a `/api/screenshot` endpoint that uses Playwright headless to capture the dashboard as PNG/PDF, eliminating manual browser screenshots.
- **Multi-dashboard support**: Extend `DashboardDataService` to watch a directory of JSON files, allowing switching between projects via URL parameter (`/?project=agentsquad`).
- **Data validation on load**: Add JSON Schema validation or a `DashboardDataValidator` that checks for date format consistency, missing required fields, and track color validity before accepting new data. **Keep the JSON file**. This is not a database problem. The data volume is trivially small (single JSON file, <50KB), write frequency is human-paced (edited by hand), and the single-user local model means no concurrency. Adding SQLite, LiteDB, or any persistence layer would be over-engineering. If multi-project support is added, use a directory of JSON files (`wwwroot/data/{project-name}.json`), not a database. ---

### Considerations & Risks

- **Not needed.** The dashboard runs on `localhost:5000` for a single user generating PowerPoint screenshots. Adding auth would create friction with zero security benefit for a local tool. If the dashboard is ever exposed on a network:
- Add `Microsoft.AspNetCore.Authentication.Negotiate` for Windows Integrated Auth (NTLM/Kerberos) — zero-config for corporate environments
- Or add a simple shared-secret header check for API access
- The JSON data file contains project names, dates, and status labels — no PII, no secrets, no sensitive data
- If the dashboard ever displays confidential project data, ensure the Kestrel binding stays on `localhost` (already the case in `launchSettings.json`) | Scenario | Approach | Cost | |----------|----------|------| | **Current (local dev)** | `dotnet run` on developer machine | $0 | | **Team sharing** | `dotnet publish -c Release --self-contained -r win-x64` → single executable, shared via file share | $0 | | **CI-generated screenshots** | GitHub Actions with Playwright → auto-generate PNG artifacts on JSON change | $0 (free tier) | | **Intranet hosting** | Azure App Service B1 or internal IIS | ~$13/mo (B1) |
- **Memory**: Blazor Server holds one SignalR circuit per connected browser tab (~2-5MB each). For 1-3 concurrent users, this is negligible.
- **No database migrations, no schema management, no connection strings** — the JSON-file model means zero ops overhead.
- **Log output**: ASP.NET Core default logging to console is sufficient. If needed, add `Serilog.AspNetCore` 8.x for structured file logging. --- | Risk | Impact | Mitigation | |------|--------|------------| | **.NET 8 LTS end-of-life** (Nov 2026) | Must migrate to .NET 10 LTS | Plan migration in Q3 2026; expect minimal breaking changes | | **SignalR circuit limits** | >10 concurrent tabs degrades Kestrel | Not a realistic scenario for a local screenshot tool | | **FileSystemWatcher unreliability** | Known edge cases on network drives and some Linux filesystems | Already mitigated with 5-second polling fallback | | Risk | Impact | Mitigation | |------|--------|------------| | **Scope creep into multi-user/cloud** | Adds auth, database, hosting complexity disproportionate to value | Define a hard boundary: this is a local screenshot tool. If multi-user is needed, build a separate app | | **Manual screenshot workflow** | Error-prone, non-reproducible, time-consuming for frequent updates | Automate with Playwright; highest-ROI enhancement available | | **No component tests** | Regression risk as `Timeline.razor` SVG logic grows | Add bUnit tests as the first engineering task |
- **Blazor Server over Blazor WASM**: Trades offline capability for simpler deployment and instant server-side JSON reload. Correct trade-off for a local tool.
- **JSON file over database**: Trades query capability for human-editable, git-trackable, zero-setup data management. Correct for current scale.
- **Custom SVG over chart library**: Trades development speed for pixel-perfect control. Correct for fixed-layout screenshot optimization. ---
- **Should screenshot generation be automated?** If yes, adding a Playwright-based export endpoint or CLI command is the highest-value enhancement. Needs stakeholder confirmation that manual browser screenshots are a pain point worth solving.
- **Will the dashboard need to support multiple projects simultaneously?** The current single-JSON-file model works for one project. Multi-project needs a directory-based approach and a project selector UI. This should be decided before adding features.
- **Is there a need to serve the dashboard to other team members over the network?** This changes hosting requirements (TLS, auth, port binding) and should be explicitly decided rather than evolved into.
- **Should the dashboard data be version-controlled alongside the project it tracks?** Currently `dashboard-data.json` lives in `wwwroot/`. Moving it to a project-specific config repo would enable historical tracking of status over time.
- **Is PDF export needed in addition to PNG screenshots?** PDF would allow multi-page dashboards and vector-quality output. Impacts technology choice (Playwright PDF vs. server-side PDF generation with QuestPDF).
- **Should the heatmap and timeline support print-friendly output?** Adding a `@media print` stylesheet would allow direct browser printing without screenshots. ---

### Detailed Analysis

Now I have a thorough understanding of the project. The ReportingDashboard is a Blazor Server (.NET 8) application for executive reporting visualization. Let me produce the research document.

# Research: ReportingDashboard Technology Stack

> **Project**: Executive Reporting Dashboard — Milestone Timeline & Execution Status Visualization
> **Date**: May 2026
> **Status**: Research Complete
> **Mandatory Stack**: .NET 8 / Blazor Server / C# 12

---

## Executive Summary

The ReportingDashboard is a single-page Blazor Server application that renders project milestone timelines, monthly execution heatmaps, and key deliverables into a fixed 1920×1080 layout optimized for PowerPoint screenshots. The existing codebase already runs on .NET 8 with Blazor Interactive Server render mode, System.Text.Json for data binding, and FileSystemWatcher for live-reload — with zero external NuGet dependencies.

**Primary recommendation**: Evolve the existing Blazor Server architecture incrementally. The current zero-dependency, JSON-file-driven design is a strength — not a limitation. Enhancements should focus on (1) adding charting/visualization libraries for richer data presentation, (2) structured testing with bUnit, (3) optional PDF/image export for automated screenshot generation, and (4) a lightweight API layer if multi-user access is needed. Do not migrate to Blazor WASM or add a database unless requirements explicitly demand multi-user concurrent editing or cloud hosting.

---

## Key Findings

- The existing application is fully functional with zero external NuGet packages beyond the default Blazor Server template — an unusually clean dependency surface that should be preserved where possible.
- Blazor Server Interactive render mode with SignalR is the correct choice for this use case: real-time updates when JSON data changes, server-side SVG rendering, and no WASM download penalty for a tool used intermittently by executives.
- The JSON-file-as-database pattern (`dashboard-data.json` + `FileSystemWatcher`) is appropriate for single-user local tooling but becomes a bottleneck if the dashboard needs to serve multiple concurrent users or run in a containerized environment.
- SVG-based timeline rendering in `Timeline.razor` is the right approach for pixel-perfect, scalable visualizations at fixed 1920×1080 — CSS canvas or third-party chart libraries would add complexity without benefit for this layout.
- The `DashboardDataService` already implements debounced reload, retry-on-IOException, and graceful fallback to previous valid data — production-quality resilience for a file-based data source.
- bUnit (the standard Blazor component testing library) is absent from the test project and should be added for component-level testing of `Timeline.razor`, `Heatmap.razor`, and `Header.razor`.
- The .NET 8 LTS target is correct; .NET 9 (released Nov 2024) offers incremental Blazor improvements but is STS (Standard Term Support) — migration should wait for .NET 10 LTS (Nov 2025) unless a specific .NET 9 feature is needed.
- No authentication, authorization, or cloud hosting is needed for the current local-only use case; adding these prematurely would violate YAGNI.
- Automated screenshot/PDF export via Playwright for .NET or PuppeteerSharp would eliminate the manual browser screenshot workflow described in the README.

---

## Recommended Technology Stack

### Runtime & Framework

| Component | Choice | Version | Rationale |
|-----------|--------|---------|-----------|
| **Runtime** | .NET 8 LTS | 8.0.x (latest patch) | Already in use; LTS support through Nov 2026 |
| **Web Framework** | Blazor Server (Interactive) | Ships with .NET 8 | Real-time SignalR updates, server-rendered SVG, no WASM payload |
| **Language** | C# 12 | Ships with .NET 8 | File-scoped namespaces, records, nullable refs already in use |
| **Serialization** | System.Text.Json | Built-in | Already in use; supports comments and trailing commas via options |

### Visualization & UI (Recommended Additions)

| Library | Version | Purpose | License | Alternative |
|---------|---------|---------|---------|-------------|
| **None (keep SVG)** | N/A | Timeline rendering | N/A | Custom SVG in `Timeline.razor` is already superior to chart libraries for this fixed layout |
| **MudBlazor** | 7.x | Only if expanding beyond current scope (tables, dialogs, theming) | MIT | Radzen Blazor (free tier), Blazorise |
| **SkiaSharp** | 2.88.x | Server-side image generation if automated PNG export is needed | MIT | ImageSharp (SixLabors, split license) |

**Opinionated stance**: Do NOT add a charting library (ApexCharts.Blazor, ChartJs.Blazor, Radzen charts). The current hand-crafted SVG approach in `Timeline.razor` gives pixel-perfect control at 1920×1080 that no charting library can match for this fixed-layout, screenshot-optimized use case.

### Testing

| Tool | Version | Purpose |
|------|---------|---------|
| **xUnit** | 2.9.x | Test runner (already in use across AgentSquad) |
| **bUnit** | 1.31.x+ | Blazor component unit testing — **currently missing, should add** |
| **FluentAssertions** | 7.x | Readable assertions | 
| **Playwright for .NET** | 1.47.x | Automated browser screenshot/PDF export and E2E testing |
| **Verify.Blazor** | 26.x | Snapshot testing for rendered component output |

### Build & CI/CD

| Tool | Purpose |
|------|---------|
| **dotnet CLI** | Build, test, publish — already configured |
| **GitHub Actions** | CI pipeline (repo already uses GitHub) |
| **dotnet publish --self-contained** | Single-file deployment for local distribution |

### Infrastructure (Current)

| Component | Choice | Notes |
|-----------|--------|-------|
| **Hosting** | Local Kestrel | `localhost:5000`, no reverse proxy needed |
| **Data store** | JSON file (`dashboard-data.json`) | FileSystemWatcher + polling for live reload |
| **CDN/Cloud** | None | Local-only tool; no cloud dependency |

---

## Architecture Recommendations

### Current Architecture (Preserve)

```
┌─────────────────────────────────────────────────┐
│  Browser (1920×1080 fixed viewport)              │
│  ┌─────────────────────────────────────────────┐ │
│  │  Dashboard.razor (route: /)                  │ │
│  │  ├── Header.razor (title, subtitle, legend)  │ │
│  │  ├── Timeline.razor (SVG milestone tracks)   │ │
│  │  └── Heatmap.razor → HeatmapCell.razor       │ │
│  └─────────────────────────────────────────────┘ │
│         ▲ SignalR (real-time re-render)           │
└─────────┼───────────────────────────────────────┘
          │
┌─────────┼───────────────────────────────────────┐
│  Blazor Server (Kestrel, localhost:5000)         │
│  ├── DashboardDataService (singleton)            │
│  │   ├── FileSystemWatcher (primary)             │
│  │   ├── Timer polling (5s fallback)             │
│  │   └── Debounced reload (300ms)                │
│  └── JSON file: wwwroot/data/dashboard-data.json │
└─────────────────────────────────────────────────┘
```

### Design Patterns in Use

1. **Service-Component separation**: `DashboardDataService` owns all data loading/caching; Razor components are pure renderers. Preserve this.
2. **Event-driven re-render**: `OnDataChanged` event triggers component re-render via SignalR without full page reload. This is the correct Blazor Server pattern.
3. **Graceful degradation**: On JSON parse error, previous valid data persists. This is production-quality behavior.
4. **Fixed-viewport rendering**: CSS `overflow: hidden` at 1920×1080 ensures screenshot fidelity. Do not add responsive breakpoints.

### Recommended Enhancements

1. **Automated export endpoint**: Add a `/api/screenshot` endpoint that uses Playwright headless to capture the dashboard as PNG/PDF, eliminating manual browser screenshots.
2. **Multi-dashboard support**: Extend `DashboardDataService` to watch a directory of JSON files, allowing switching between projects via URL parameter (`/?project=agentsquad`).
3. **Data validation on load**: Add JSON Schema validation or a `DashboardDataValidator` that checks for date format consistency, missing required fields, and track color validity before accepting new data.

### Data Storage Strategy

**Keep the JSON file**. This is not a database problem. The data volume is trivially small (single JSON file, <50KB), write frequency is human-paced (edited by hand), and the single-user local model means no concurrency. Adding SQLite, LiteDB, or any persistence layer would be over-engineering.

If multi-project support is added, use a directory of JSON files (`wwwroot/data/{project-name}.json`), not a database.

---

## Security & Infrastructure

### Authentication & Authorization

**Not needed.** The dashboard runs on `localhost:5000` for a single user generating PowerPoint screenshots. Adding auth would create friction with zero security benefit for a local tool.

If the dashboard is ever exposed on a network:
- Add `Microsoft.AspNetCore.Authentication.Negotiate` for Windows Integrated Auth (NTLM/Kerberos) — zero-config for corporate environments
- Or add a simple shared-secret header check for API access

### Data Protection

- The JSON data file contains project names, dates, and status labels — no PII, no secrets, no sensitive data
- If the dashboard ever displays confidential project data, ensure the Kestrel binding stays on `localhost` (already the case in `launchSettings.json`)

### Hosting & Deployment

| Scenario | Approach | Cost |
|----------|----------|------|
| **Current (local dev)** | `dotnet run` on developer machine | $0 |
| **Team sharing** | `dotnet publish -c Release --self-contained -r win-x64` → single executable, shared via file share | $0 |
| **CI-generated screenshots** | GitHub Actions with Playwright → auto-generate PNG artifacts on JSON change | $0 (free tier) |
| **Intranet hosting** | Azure App Service B1 or internal IIS | ~$13/mo (B1) |

### Operational Concerns

- **Memory**: Blazor Server holds one SignalR circuit per connected browser tab (~2-5MB each). For 1-3 concurrent users, this is negligible.
- **No database migrations, no schema management, no connection strings** — the JSON-file model means zero ops overhead.
- **Log output**: ASP.NET Core default logging to console is sufficient. If needed, add `Serilog.AspNetCore` 8.x for structured file logging.

---

## Risks & Trade-offs

### Low Risk (Monitor)

| Risk | Impact | Mitigation |
|------|--------|------------|
| **.NET 8 LTS end-of-life** (Nov 2026) | Must migrate to .NET 10 LTS | Plan migration in Q3 2026; expect minimal breaking changes |
| **SignalR circuit limits** | >10 concurrent tabs degrades Kestrel | Not a realistic scenario for a local screenshot tool |
| **FileSystemWatcher unreliability** | Known edge cases on network drives and some Linux filesystems | Already mitigated with 5-second polling fallback |

### Medium Risk (Mitigate)

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Scope creep into multi-user/cloud** | Adds auth, database, hosting complexity disproportionate to value | Define a hard boundary: this is a local screenshot tool. If multi-user is needed, build a separate app |
| **Manual screenshot workflow** | Error-prone, non-reproducible, time-consuming for frequent updates | Automate with Playwright; highest-ROI enhancement available |
| **No component tests** | Regression risk as `Timeline.razor` SVG logic grows | Add bUnit tests as the first engineering task |

### Trade-offs Accepted

1. **Blazor Server over Blazor WASM**: Trades offline capability for simpler deployment and instant server-side JSON reload. Correct trade-off for a local tool.
2. **JSON file over database**: Trades query capability for human-editable, git-trackable, zero-setup data management. Correct for current scale.
3. **Custom SVG over chart library**: Trades development speed for pixel-perfect control. Correct for fixed-layout screenshot optimization.

---

## Open Questions

1. **Should screenshot generation be automated?** If yes, adding a Playwright-based export endpoint or CLI command is the highest-value enhancement. Needs stakeholder confirmation that manual browser screenshots are a pain point worth solving.

2. **Will the dashboard need to support multiple projects simultaneously?** The current single-JSON-file model works for one project. Multi-project needs a directory-based approach and a project selector UI. This should be decided before adding features.

3. **Is there a need to serve the dashboard to other team members over the network?** This changes hosting requirements (TLS, auth, port binding) and should be explicitly decided rather than evolved into.

4. **Should the dashboard data be version-controlled alongside the project it tracks?** Currently `dashboard-data.json` lives in `wwwroot/`. Moving it to a project-specific config repo would enable historical tracking of status over time.

5. **Is PDF export needed in addition to PNG screenshots?** PDF would allow multi-page dashboards and vector-quality output. Impacts technology choice (Playwright PDF vs. server-side PDF generation with QuestPDF).

6. **Should the heatmap and timeline support print-friendly output?** Adding a `@media print` stylesheet would allow direct browser printing without screenshots.

---

## Implementation Recommendations

### Phase 1: Testing & Quality (Week 1)

**Quick wins that demonstrate value immediately:**

1. **Add bUnit to `ReportingDashboard.Tests`**:
   ```xml
   <PackageReference Include="bunit" Version="1.31.3" />
   ```
   Write component tests for `Timeline.razor` (SVG output correctness), `Heatmap.razor` (cell rendering), and `Header.razor` (data binding).

2. **Add JSON schema validation**: Create a `DashboardDataValidator` that runs on load and reports specific errors (e.g., "Track M1 has milestone date '2025-13-01' which is not a valid date").

3. **Add a health-check endpoint**: `app.MapGet("/health", () => Results.Ok())` — trivial but useful for automation.

### Phase 2: Automated Export (Week 2)

**Highest ROI enhancement — eliminates manual screenshot workflow:**

1. **Add Playwright for .NET** (`Microsoft.Playwright` 1.47.x) as a dev/CLI dependency
2. **Create a `ScreenshotExporter` service** or CLI command that:
   - Launches headless Chromium at 1920×1080
   - Navigates to `http://localhost:5000`
   - Waits for data load (check for a CSS class on the rendered dashboard)
   - Captures PNG and optionally PDF
   - Saves to a configurable output path
3. **Wire into CI**: GitHub Actions job that runs on `dashboard-data.json` changes → produces screenshot artifact

### Phase 3: Multi-Dashboard Support (Week 3, if needed)

1. Extend `DashboardDataService` to accept a project name parameter
2. Watch `wwwroot/data/*.json` directory instead of a single file
3. Add route parameter: `Dashboard.razor` at `/{projectName?}`
4. Add a simple project switcher dropdown in `Header.razor`

### Areas Requiring Prototyping Before Committing

- **Playwright screenshot quality**: Prototype the headless screenshot flow to verify SVG rendering fidelity, font rendering, and color accuracy match the manual browser screenshot. Playwright's Chromium may render fonts slightly differently than Edge/Chrome.
- **PDF export**: If PDF is needed, prototype both Playwright PDF and QuestPDF server-side rendering to compare output quality and complexity.

### What NOT to Build

- **Do not add a database.** The JSON file model works. A database adds migration tooling, connection management, and deployment complexity for zero user benefit.
- **Do not add authentication** unless the dashboard will be network-accessible.
- **Do not switch to Blazor WASM.** Server-side rendering with SignalR push on file change is the correct architecture for live-reloading local data.
- **Do not add a CSS framework** (Bootstrap, Tailwind). The existing custom CSS is purpose-built for 1920×1080 fixed layout. A framework would fight this constraint.
