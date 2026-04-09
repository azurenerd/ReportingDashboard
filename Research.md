# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-09 17:28 UTC_

### Summary

The Executive Reporting Dashboard is a simple, single-page Blazor Server application that visualizes project milestones and progress for executive stakeholders. It reads data from a JSON file (`data.json`), requires no authentication, and targets local-only deployment with no cloud services. The recommended technology stack leverages C# .NET 8 with built-in capabilities (System.Text.Json, FileSystemWatcher) to minimize external dependencies, avoiding complexity that contradicts the "super simple" design goal. Key success factors: (1) data-driven JSON schema that executives can edit directly, (2) Chart.js for lightweight, screenshot-friendly visualizations, (3) Bootstrap 5.3 for polished, print-friendly styling, and (4) self-contained `.exe` deployment for one-click installation. This approach prioritizes simplicity, visual clarity for PowerPoint decks, and rapid development over enterprise features. ---

### Key Findings

- **Blazor Server's built-in `@bind` directive and FileSystemWatcher eliminate external dependencies** for real-time data binding and file monitoring. No need for complex state management or SignalR for a single-user dashboard.
- **System.Text.Json (built-in to .NET 8) is faster and lighter than Newtonsoft.Json** for JSON serialization and deserialization; source generators provide compile-time validation without external schema libraries.
- **Chart.js (v4.4.x via CDN) is the optimal charting solution** for this project: lightweight (35KB), screenshot-friendly for PowerPoint, industry-standard for dashboards (Grafana, DataDog use it), and requires zero build pipeline overhead.
- **Bootstrap 5.3 (via CDN) provides professional defaults with excellent print/screenshot support**, eliminating the need for Node.js build tools (Tailwind) or hand-rolled CSS.
- **FileSystemWatcher with 500ms debouncing is the native .NET solution** for monitoring data.json; avoids polling overhead and external library dependencies while remaining responsive to file edits.
- **Self-contained `.exe` deployment (win-x64) is optimal for non-technical executives**: no .NET runtime installation required, one-click launch, and embedded Kestrel server eliminates IIS complexity.
- **Data schema should remain flat, CEO-editable JSON** (not a database) to allow non-developers to modify milestones, status, and KPIs directly in VS Code or Notepad without code deployment.
- **Print CSS and viewport constraints are critical for screenshot quality**; executives will use native browser tools (Ctrl+P, Cmd+Shift+P screenshot) to generate PowerPoint visuals, so app must render cleanly at 1080p and 1440p.
- **Testing strategy prioritizes JSON validation and component rendering** (MVP) over visual regression and load testing (Phase 2), aligned with single-user, local-only use case.
- **No authentication, encryption, or enterprise security overhead** is justified; app is localhost-only, non-networked, and runs on corporate machines with physical access controls.
- ---
- Create new Blazor Server project (.NET 8)
- Set up solution structure: `Models/`, `Services/`, `Components/`, `Pages/`
- Create POCO classes: `ProjectReport`, `Milestone`, `StatusSnapshot`
- Wire up CDN references (Bootstrap 5.3, Chart.js 4.4.x) in `App.razor`
- Implement `DataLoaderService` (JSON deserialization via System.Text.Json)
- Implement `DataWatcherService` (FileSystemWatcher, 500ms debounce)
- Create `data.json` template with sample project (2-3 milestones, 5-10 status items)
- Wire services into `Program.cs` (singleton, scoped)
- Build `MilestoneTimeline.razor` (Chart.js integration, cascading parameters)
- Build `StatusSnapshot.razor` (three-column layout, Bootstrap cards)
- Build `ProjectMetadata.razor` (KPI display, responsive grid)
- Build `Index.razor` orchestration (subscribe to DataWatcherService, StateHasChanged)
- Implement `app.css` (custom colors, spacing, responsive tweaks)
- Implement `print.css` (page breaks, margins, color adjustments)
- Test screenshot rendering at 1080p and 1440p in Chrome/Edge
- Iterate on visual hierarchy (milestones prominent, status items readable)
- Write unit tests for JSON deserialization (xUnit + FluentAssertions)
- Write unit tests for data validation logic (invalid status enums, missing required fields)
- Write component tests for MilestoneTimeline rendering (bUnit + Moq)
- Achieve 80% code coverage for data layer, 70% for components
- Generate self-contained .exe: `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true`
- Document system requirements (Windows 10+, 1GB RAM minimum)
- Document user workflow: "Download .exe → Double-click → Edit data.json in VS Code → App auto-refreshes"
- Create sample data.json for fictional project (e.g., "Project Phoenix: Q2 2026 Roadmap")
- Write README.md with setup instructions, data schema documentation, print/screenshot guide
- ✅ Blazor Server project (GitHub or ADO repo)
- ✅ Self-contained .exe ready for distribution
- ✅ Sample data.json with realistic example project
- ✅ Unit + component tests (MVP coverage targets)
- ✅ README.md, data schema documentation, user guide
- ---
- **Data auto-refresh indicator:** Add "Data refreshed at HH:mm:ss" text below dashboard header (reassures executives that app is monitoring data.json). **Effort:** 30 min.
- **Status color coding:** Milestone status (on-track = green, at-risk = yellow, delayed = red, completed = gray) using Bootstrap color utilities. **Effort:** 1 hour.
- **Milestone progress visualization:** Use Chart.js horizontal bar chart (progress bar) for visual milestone status. **Effort:** 2 hours (if Chart.js integration already done).
- **Print button in UI:** Add "Print (Ctrl+P)" button for easy access; documents workflow for executives unfamiliar with browser shortcuts. **Effort:** 30 min.
- **Data validation error display:** If data.json is malformed, show helpful error message in red banner at top of dashboard (vs. blank screen). **Effort:** 1 hour.
- ---
- **Chart.js rendering quality:** Create minimal Blazor component with Chart.js test chart (horizontal bar). Verify rendering quality in print preview and 1080p/1440p screenshots before committing to full timeline component. **Recommended effort:** 2-3 hours (do in Week 3).
- **FileSystemWatcher race conditions:** Write quick test to verify debouncing works correctly when data.json is edited 3-4 times in rapid succession. Confirm no duplicate UI updates. **Recommended effort:** 2 hours (do in Week 2).
- **Print CSS layout:** Test Bootstrap grid responsiveness at 1080p and 1440p using Chrome DevTools responsive design mode (F12 → Ctrl+Shift+M). Capture screenshots and iterate on margin/spacing. **Recommended effort:** 3-4 hours (do in Week 4).
- **Self-contained .exe size & startup:** Build initial version, measure .exe size, test startup time on clean Windows 10 machine (if possible). Confirm <5MB startup overhead vs. baseline .NET 8 runtime. **Recommended effort:** 1 hour (do at end of Phase 1).
- ---
- ---
- ✅ Dashboard renders milestone timeline (Chart.js horizontal bar chart) with 3+ milestones
- ✅ Status snapshot displays shipped/in-progress/carried-over items (Bootstrap three-column layout)
- ✅ Data.json file edits trigger auto-refresh within 1 second (FileSystemWatcher + debounce)
- ✅ Print preview (Ctrl+P) renders cleanly at 1080p and 1440p without overflow or unwanted page breaks
- ✅ Screenshots capture clean visuals suitable for PowerPoint insertion (no artifacts, readable text, professional appearance)
- ✅ Self-contained .exe can be downloaded and launched by non-technical executive in <5 minutes
- ✅ Unit tests cover JSON validation and component rendering (80% data layer, 70% components)
- ✅ README.md documents data schema, user workflow, and system requirements
- ✅ Sample data.json provided with realistic fictional project
- ---
- ```
- ReportingDashboard/
- ├── README.md                          # Setup, usage, schema documentation
- ├── ReportingDashboard.sln
- ├── ReportingDashboard/
- │   ├── ReportingDashboard.csproj      # <PublishSingleFile>, <SelfContained>
- │   ├── Program.cs                     # Service registration, Kestrel config
- │   ├── App.razor                      # CDN links, layout, DataWatcherService injection
- │   ├── Models/
- │   │   ├── ProjectReport.cs           # Root POCO, cascading parameters
- │   │   ├── Milestone.cs               # id, name, targetDate, status, progress
- │   │   └── StatusSnapshot.cs          # shipped[], inProgress[], carriedOver[]
- │   ├── Services/
- │   │   ├── DataLoaderService.cs       # System.Text.Json deserialization
- │   │   ├── DataWatcherService.cs      # FileSystemWatcher, debounce, events
- │   │   └── DataValidator.cs           # Schema validation (status enums, etc.)
- │   ├── Components/
- │   │   ├── MilestoneTimeline.razor    # Chart.js horizontal bar chart
- │   │   ├── StatusSnapshot.razor       # Three-column layout
- │   │   └── ProjectMetadata.razor      # KPIs, project name, period
- │   ├── Pages/
- │   │   └── Index.razor                # Orchestration, StateHasChanged, layout
- │   ├── wwwroot/
- │   │   ├── app.css                    # Custom styles
- │   │   ├── print.css                  # Print media queries
- │   │   └── sample-data.json           # Example data for demo
- │   └── appsettings.json               # Data path, port config
- ├── ReportingDashboard.Tests/
- │   ├── ReportingDashboard.Tests.csproj
- │   ├── Models/
- │   │   └── ProjectReportTests.cs      # JSON deserialization tests
- │   ├── Services/
- │   │   ├── DataValidatorTests.cs      # Schema validation (happy path + edge cases)
- │   │   └── DataLoaderServiceTests.cs  # Error handling
- │   └── Components/
- │       └── MilestoneTimelineTests.cs  # bUnit component rendering tests
- └── .gitignore                         # bin/, obj/, publish/
- ```
- ---
- The Executive Reporting Dashboard is a straightforward, single-page Blazor Server application optimized for simplicity and visual impact. By leveraging .NET 8's built-in capabilities (System.Text.Json, FileSystemWatcher) and lightweight open-source libraries (Chart.js, Bootstrap), the implementation avoids unnecessary complexity while delivering a professional, screenshot-friendly interface for executive stakeholders. The 4-6 week MVP timeline prioritizes core functionality: data-driven JSON schema, real-time file monitoring, milestone visualization, and print-optimized styling. Phase 2 enhancements (visual regression testing, PDF export, MSI installer) defer lower-priority features, allowing rapid delivery of business value. Success hinges on treating data.json as the single source of truth—executives edit JSON directly, app refreshes automatically, and PowerPoint visuals are generated via native browser screenshot tools. This approach aligns with the project's ethos of simplicity, local-only deployment, and minimal security overhead.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Justification |
- |-------|-----------|---------|---------------|
- | **UI Framework** | Blazor Server | .NET 8.0+ | Built-in two-way data binding, server-side component lifecycle, perfect for stateful dashboards |
- | **CSS Framework** | Bootstrap | 5.3.x (CDN) | Professional defaults, print-friendly, zero build pipeline needed |
- | **Charting Library** | Chart.js | 4.4.x (CDN) | Lightweight (35KB), screenshot-optimized, industry-standard, no API calls required |
- | **Progress/Status UI** | Native HTML + Bootstrap utilities | N/A | `<progress>` elements, Bootstrap flex/grid utilities, no additional libraries needed |
- | Layer | Technology | Version | Justification |
- |-------|-----------|---------|---------------|
- | **JSON Serialization** | System.Text.Json | Built-in (.NET 8) | Zero external dependency, fast, source generators for compile-time validation |
- | **File Monitoring** | FileSystemWatcher | Built-in (System.IO) | Native .NET, zero overhead, debounce 500ms for file write race conditions |
- | **Data Schema** | Flat JSON (data.json) | N/A | CEO-editable in VS Code/Notepad, no database overhead, supports 3-5 concurrent project files |
- | **Configuration** | appsettings.json | .NET 8 standard | Project name, data file path, port configuration |
- | Layer | Technology | Version | Justification |
- |-------|-----------|---------|---------------|
- | **Runtime** | .NET 8 | 8.0+ (LTS) | Mandatory; mature Blazor Server support, 3-year support window |
- | **Web Server** | Kestrel (embedded) | Built-in | Lightweight, localhost-only, no IIS required, starts automatically |
- | **Deployment Model** | Self-contained .exe | win-x64 | Single file (~100-150MB), no .NET runtime install required, one-click launch |
- | **Hosting** | Localhost (http://127.0.0.1:5000) | N/A | No firewall complexity, no cloud provider, no external routing |
- | Layer | Technology | Version | Justification |
- |-------|-----------|---------|---------------|
- | **Unit Testing** | xUnit | 2.7.x | .NET standard, well-documented, JSON validation tests |
- | **Component Testing** | bUnit | 1.28.x | Blazor-specific, test components in isolation with mocked services |
- | **Assertions** | FluentAssertions | 6.x | Readable assertion syntax, easier debugging |
- | **Mocking** | Moq | 4.x | Lightweight, standard .NET mocking library |
- | **Visual Regression** | Playwright (Phase 2) | 1.45.x | Screenshot automation, ensures 1080p/1440p consistency across updates |
- | **JSON Validation** | Manual (System.Text.Json) | Built-in | Custom validator service; schema validation overhead unjustified for simple structure |
- | Tool | Version | Purpose |
- |------|---------|---------|
- | Visual Studio / VS Code | Latest | Development IDE |
- | .NET SDK | 8.0+ | Build, test, publish |
- | Chrome / Edge | Latest | Print preview testing, native screenshots |
- ---
- Loads Bootstrap 5.3 and Chart.js via CDN
- Injects `DataWatcherService` (singleton)
- Injects `DataLoaderService` (scoped)
- Orchestrates three child components: `MilestoneTimeline`, `StatusSnapshot`, `ProjectMetadata`
- Subscribes to `DataWatcherService.OnDataChanged` event
- Calls `StateHasChanged()` on file write, triggers JSON reload
- Displays "Data refreshed at HH:mm:ss" timestamp
- Renders `<canvas>` element
- Receives `milestones[]` as cascading parameter
- Uses `IJSRuntime.InvokeAsync()` to populate Chart.js horizontal bar chart
- Shows progress (0-100%) and status (on-track, at-risk, delayed, completed)
- Three-column layout: Shipped | In Progress | Carried Over
- Each column displays list of items from `data.json`
- Uses Bootstrap cards for clean visual separation
- Header section: Project name, reporting period, KPIs (on-time delivery %, team capacity %)
- Responsive grid layout, screenshot-friendly at 1080p/1440p
- ```
- data.json (file system)
- ↓
- FileSystemWatcher (monitors LastWrite, debounce 500ms)
- ↓
- DataWatcherService (singleton event emitter)
- ↓
- Index.razor (StateHasChanged() + reload JSON)
- ↓
- DataLoaderService (System.Text.Json deserialization)
- ↓
- ProjectReport POCO (validated via custom validator)
- ↓
- Child components (MilestoneTimeline, StatusSnapshot) render from state
- ```
- ```json
- {
- "projectName": "Project Horizon",
- "reportingPeriod": "2026-Q2",
- "kpis": {
- "onTimeDelivery": 85,
- "teamCapacity": 92
- },
- "milestones": [
- {
- "id": "m1",
- "name": "Platform Launch",
- "targetDate": "2026-05-15",
- "status": "on-track",
- "progress": 75,
- "description": "Core platform infrastructure and APIs"
- }
- ],
- "statusSnapshot": {
- "shipped": ["Feature X", "Integration Y"],
- "inProgress": ["Feature Z", "Performance optimization"],
- "carriedOver": ["Deferred item A", "Deferred item B"]
- }
- }
- ```
- **bootstrap.min.css** (CDN) — Base framework, typography, utilities
- **app.css** — Custom color scheme, spacing adjustments, grid overrides
- **print.css** — Print media queries, page breaks, margin adjustments
- ```css
- @media print {
- nav, footer, .refresh-indicator { display: none; }
- body { margin: 0; padding: 20px; font-size: 11pt; width: 8.5in; }
- .timeline, .milestone-block { page-break-inside: avoid; }
- canvas { print-color-adjust: exact; }
- }
- ```
- Test layouts at 1080p (1920×1080) and 1440p (2560×1440)
- Constrain `.container` max-width: 1300px for consistent scaling
- Use responsive Bootstrap grid (`col-12`, `col-lg-4`) for adaptive layouts
- ---

### Considerations & Risks

- **Not required.** App is localhost-only, runs on corporate machines with physical access controls.
- No user accounts, tokens, or role-based access needed.
- **Assumption:** Executives have Windows login; no additional authentication layer justified.
- **At rest:** data.json stored in local directory (same machine running app). No encryption needed for internal corporate metrics.
- **In transit:** None (localhost only). No network communication outside the machine.
- **Recommendation:** Encourage IT to secure the data folder with NTFS permissions (restrict read/write to authorized users).
- **Model:** Self-contained executable (win-x64)
- **Command:** `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true`
- **Output:** Single `.exe` file (~120-150MB) bundling .NET 8 runtime + app binaries
- **Launch:** Double-click `.exe`; app opens http://localhost:5000 in default browser automatically
- **Distribution:** Upload to shared drive (\\corp-intranet\tools\ReportingDashboard.exe); email download link to executives
- **Port:** 5000 (localhost only, no external routing)
- **Startup time:** ~3-5 seconds (cold start); ~1-2 seconds (warm start)
- **Memory footprint:** ~150MB (Blazor Server + .NET 8 runtime)
- **CPU:** Minimal (idle loop, no background workers)
- **Development:** Zero (open-source .NET SDK, free Visual Studio Community if desired)
- **Hosting:** Zero (local machine only, no cloud provider)
- **Distribution:** Zero (file share or email delivery)
- **Estimated annual cost:** $0
- MSI installer packaging (Wix Toolset) for corporate deployment via Group Policy
- PDF export capability (wkhtmltopdf integration)
- Data.json versioning & backup (Git integration or S3 sync—breaks "local only" constraint, require stakeholder approval)
- ---
- | Risk | Impact | Probability | Mitigation |
- |------|--------|-------------|-----------|
- | **FileSystemWatcher misses rapid file writes** | data.json updates lost, UI out-of-sync | Medium | Implement 500ms debounce, log all refresh events, add UI "last refreshed" timestamp |
- | **Chart.js rendering fails in print/screenshot** | Broken PowerPoint visuals (timeline not visible) | Low | Test print preview in Chrome/Edge, verify CDN fallback, test at 1080p & 1440p |
- | **Self-contained .exe size (150MB) too large** | Slow distribution, user friction | Low | Acceptable for corporate intranet (typically <5min download). Compress with 7-Zip if needed (~40MB). |
- | **JSON schema validation too strict** | Executives cannot edit data.json, blocked workflow | Medium | Keep schema loose (nullable fields where sensible), thorough documentation of required fields |
- | **Timestamp misalignment (data edited but UI stale)** | Executives trust outdated metrics | Medium | Implement cache buster (file write time as versioning key), log all changes to browser console |
- | **Kestrel web server crashes** | App becomes unresponsive until manually restarted | Low | Handle unhandled exceptions in Blazor components, log to console, add error boundary component |
- | **Windows .NET 8 runtime issues** | App won't run on older Windows 7/8 machines | Medium | Target Windows 10+ (document system requirements). Use Framework-dependent model as fallback if needed. |
- **Single-file JSON limitation:** Data.json cannot exceed ~50MB in size without performance degradation (JSON deserialization, DOM rendering). For multi-year project history, consider Phase 2 database (SQLite local).
- **Single-page app:** No pagination, no lazy loading. All milestones/status items load into memory. For >100 milestones, consider pagination component (Phase 2).
- **FileSystemWatcher is single-machine only:** Does not sync across network drives or cloud storage. Ideal for local development; not suitable for multi-user shared editing.
- **Chart.js rendering:** For >20 milestones, consider server-side chart generation (Phase 2) to improve initial load time.
- **Blazor Server:** 1-2 weeks to proficiency for C# developers unfamiliar with Blazor
- **Component architecture:** 2-3 days to understand parent/child data flow with cascading parameters
- **Print CSS tuning:** 1-2 days of trial-and-error for 1080p/1440p optimization
- **Team readiness:** Assume developers have C# basics and HTML/CSS familiarity; no Blazor experience required (ramp-up included in Phase 1 timeline)
- | Decision | Trade-off | Rationale |
- |----------|-----------|-----------|
- | **Chart.js (CDN) vs. SVG native** | CDN requires internet; SVG requires custom math | Chart.js preferred: minimal dev overhead, proven rendering quality, faster time-to-market |
- | **Bootstrap 5.3 vs. Tailwind CSS** | Bootstrap heavier (~400KB), Tailwind requires Node build | Bootstrap preferred: print-friendly defaults, no build pipeline, simpler deployment |
- | **FileSystemWatcher vs. polling** | FSW may miss rapid writes; polling has CPU overhead | FSW preferred: zero-dependency, debounce mitigates race conditions, acceptable trade-off for local app |
- | **JSON file vs. SQLite** | JSON not queryable, no transactions; SQLite adds complexity | JSON preferred: CEO-editable, zero database overhead, sufficient for MVP. Defer SQLite to Phase 2 if needed. |
- | **Self-contained .exe vs. Framework-dependent** | .exe larger (150MB), FD requires .NET 8 install | .exe preferred: one-click launch, no IT involvement, eliminates friction for non-technical users |
- ---
- **What is the expected lifespan of data.json?** Should the app archive historical versions, or is it always a single "current" snapshot? (Impacts schema complexity, potential Phase 2 versioning feature)
- **How frequently will executives edit data.json?** Daily, weekly, or ad-hoc? (Determines if auto-save UI feedback is sufficient, or if multi-user conflict resolution is needed)
- **Are there non-Windows executives?** (Current design targets win-x64. If macOS/Linux executives exist, consider .NET app packaging for those platforms in Phase 2)
- **What is the maximum number of milestones** per project report? (Affects Chart.js rendering performance; >50 milestones may require pagination or server-side generation)
- **Should executives be able to export reports to PDF?** (Current design supports browser print → PDF via Ctrl+P. If direct PDF button is required, consider wkhtmltopdf integration in Phase 2)
- **Is there a desire to share reports across teams?** (Current design is single-machine. Multi-user access would require network sharing or cloud sync—conflicts with "local only" constraint; clarify intent)
- **Should the app support multiple projects simultaneously** (tabs/menu) or always single-project-per-run? (Impacts component structure and routing; current design assumes one project per app instance)
- **What is the rollout plan for >50 executives?** (Self-contained .exe suitable for ad-hoc distribution. If coordinated IT deployment needed, Plan Phase 2 MSI installer with Group Policy integration)
- ---

### Detailed Analysis

# Executive Reporting Dashboard Research

## Sub-Question 1: Blazor Server Data Binding & Component Patterns

### Key Findings
Blazor Server with C# .NET 8 provides strong two-way data binding via `@bind` directive and reactive component lifecycle. For a JSON-driven dashboard, StateHasChanged() paired with FileSystemWatcher enables efficient UI updates without page reloads. Blazor's async ComponentInitializedAsync lifecycle is ideal for initial JSON loading; cascading parameters enable hierarchical data flow.

### Tools & Libraries
- **System.Text.Json** (built-in, .NET 8) - No external dependency; fast, modern JSON deserialization
- **FileSystemWatcher** (System.IO.FileSystemWatching, built-in) - Local file monitoring
- **SignalR** (built-in to Blazor Server) - Real-time component state updates across browser tabs

### Trade-offs & Alternatives
| Approach | Pros | Cons | Verdict |
|----------|------|------|---------|
| `@bind` + polling | Simple, familiar | CPU overhead | Avoid |
| `@bind` + FileSystemWatcher | Zero external deps, fast | Single-file limitation | **Recommended** |
| SignalR + server-side events | Multi-tab sync, enterprise-grade | Overkill for single-page local app | Defer |
| Newtonsoft.Json (v13.x) | Flexible | Slight performance hit vs System.Text.Json | Avoid |

### Concrete Recommendations
- Use `System.Text.Json` with `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` for schema flexibility
- Implement FileSystemWatcher in a scoped service listening to data.json directory
- Trigger `StateHasChanged()` on file write events (debounce 500ms to avoid duplicate updates)
- Load JSON in `OnInitializedAsync()`, store in component state

### Evidence & Reasoning
Blazor Server is architected for stateful, server-side component rendering with bidirectional binding. For a local, single-user dashboard, FileSystemWatcher + System.Text.Json avoids external NuGet dependencies, reducing attack surface and deployment complexity. This aligns with "super simple" requirement and no cloud/enterprise security constraints.

---

## Sub-Question 2: Data.json Schema Design & Structure

### Key Findings
Executive dashboards require four key data domains: project metadata, milestones (timeline), status snapshots (shipped/in-progress/carried-over), and KPIs. A flat-with-nesting schema (JSON array + objects) is optimal—avoids query complexity while remaining CEO-readable in text editors. No database overhead needed for this use case.

### Tools & Libraries
- **JSON Schema Draft 2020-12** - Validation framework (not a library; use as spec)
- **NJsonSchema** (v10.9.x) - C# library for schema validation
- **Newtonsoft.Json.Schema** (v3.x) - Alternative validator (heavier)

### Recommended Schema Structure
```json
{
  "projectName": "Project Horizon",
  "reportingPeriod": "2026-Q2",
  "milestones": [
    {
      "id": "m1",
      "name": "Platform Launch",
      "targetDate": "2026-05-15",
      "status": "on-track|at-risk|delayed|completed",
      "progress": 75
    }
  ],
  "statusSnapshot": {
    "shipped": ["Feature X", "Integration Y"],
    "inProgress": ["Feature Z"],
    "carriedOver": ["Deferred Item"]
  },
  "kpis": {
    "onTimeDelivery": 85,
    "teamCapacity": 92
  }
}
```

### Trade-offs & Alternatives
| Approach | Pros | Cons | Verdict |
|----------|------|------|---------|
| **Flat JSON array** | CEO-editable, no SQL | No relational integrity | **Recommended** |
| SQLite local DB | Queryable, versioning | Overkill, requires migrations | Avoid (phase 2) |
| YAML config | Human-friendly | Requires additional parser | Not needed |

### Concrete Recommendations
- Create `ProjectReport` POCO classes with `[JsonPropertyName]` attributes
- Implement `JsonValidator` service using **System.Text.Json** with manual validation (schema library overhead not justified for simple structure)
- Add JSON file watcher that triggers schema validation on change; log validation errors to UI
- Support 3–5 concurrent project files (naming: `data-{projectName}.json`)

### Evidence & Reasoning
Executives will edit JSON in VS Code or Notepad. Flat schema = minimal friction. System.Text.Json's source generators (C# 11+, available in .NET 8) provide compile-time serialization validation. Manual validation (5-10 checks) is lighter than external schema library for this scope.

---

## Sub-Question 3: Charting & Visualization Libraries

### Key Findings
Blazor Server has three viable charting paths: (1) JavaScript interop (Chart.js, Plotly), (2) C# native SVG generation, (3) HTML/CSS tables. For executive dashboards, timeline visualization is critical; Gantt-style charts and progress bars are cleaner than pie charts for this domain.

### Tools & Libraries

**Option A: Chart.js 4.x via JavaScript Interop** (RECOMMENDED)
- Package: `Chart.js` (v4.4.x) via CDN; wrapper: no NuGet needed, direct JS interop
- Strengths: Lightweight (35KB), screenshot-friendly, responsive, no external API calls
- Maturity: Extremely mature (10+ years), >100k NPM weekly downloads
- Integration: Use Blazor `IJSRuntime.InvokeAsync()` to instantiate charts

**Option B: Plotly.js (heavier alternative)**
- Package: `Plotly.js` (v2.x) via CDN
- Strengths: Interactive Gantt charts (Timeline Plotly layout), publication-quality
- Cons: ~3MB bundle, overkill for simple progress bars; slower rendering

**Option C: Native Blazor SVG (lightweight alternative)**
- No external library; render SVG directly from C# data
- Strengths: Zero dependencies, screenshot-perfect, full control
- Cons: Timeline rendering requires custom math; steeper dev learning curve

### Trade-offs & Alternatives
| Library | Use Case | Fit for Project |
|---------|----------|-----------------|
| Chart.js | Bar/line progress over time | **Best for timeline + progress** |
| Plotly.js | Complex interactive Gantt | Overkill for simple milestones |
| SVG native | Pixel-perfect minimalism | Only if design requires custom rendering |
| PdfSharp (rendering to PDF) | Export capability | Phase 2 feature |

### Concrete Recommendations
- **Use Chart.js 4.4.x** for milestone timeline (horizontal bar chart) and progress (progress bars in HTML)
- Load Chart.js from CDN (jsDelivr): `https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js`
- Create Blazor component `MilestoneTimeline.razor` that exposes `IAsyncJSRuntime` and populates canvas via interop
- For progress bars: use native HTML `<progress>` element (no library needed) or simple CSS bar

### Evidence & Reasoning
Chart.js is industry-standard for dashboards (used by Grafana, DataDog). For executive screenshots, lightweight rendering (no animations needed) and CDN hosting avoid build complexity. Blazor interop pattern is well-documented and stable in .NET 8. No authentication/security overhead—CDN is acceptable for local corporate use.

---

## Sub-Question 4: CSS & Styling Approach

### Key Findings
For screenshot-friendly dashboards destined for PowerPoint, consistency and simplicity outweigh flexibility. Bootstrap 5.3 provides polished defaults; Tailwind requires custom print CSS tuning. The original OriginalDesignConcept likely uses minimal CSS—preserve that ethos.

### Tools & Libraries

**Option A: Bootstrap 5.3.x** (RECOMMENDED)
- NuGet: `Bootstrap` (v5.3.x) - ships CSS + JS
- Strengths: Professional defaults, print-friendly, extensive utilities, <400KB gzipped
- Community: Massive (millions of sites), backward-compatible

**Option B: Tailwind CSS 3.x**
- NPM: `tailwindcss` (v3.4.x)
- Strengths: Minimal CSS footprint, highly customizable
- Cons: Requires build pipeline (Node.js); overkill for single-page app; print support needs manual work

**Option C: No CSS framework (hand-rolled)**
- Strengths: Minimal, matches OriginalDesignConcept simplicity
- Cons: Inconsistent spacing, typography; longer dev time

### Recommended CSS Structure
```css
/* Print media for screenshot quality */
@media print {
  body { margin: 0; padding: 20px; font-size: 14px; }
  .timeline { page-break-inside: avoid; }
  .milestone { page-break-inside: avoid; }
}

/* Viewport constraints for 1080p/1440p screenshots */
@media (max-width: 1920px) {
  .container { max-width: 95vw; }
}
```

### Trade-offs & Alternatives
| Framework | Print Support | Setup Complexity | Screenshot Fit |
|-----------|---------------|------------------|----------------|
| **Bootstrap 5.3** | Built-in | 1 line CDN | **Excellent** |
| Tailwind 3.x | Manual config | Requires Node build | Good (extra effort) |
| Hand-rolled CSS | Manual | Medium | Depends on skill |

### Concrete Recommendations
- Include Bootstrap 5.3 via CDN in `App.razor` head: `https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css`
- Add a custom `print.css` file that removes navigation, sets print margins, and ensures milestones/status blocks stay on-page
- Use Bootstrap utility classes (`container-fluid`, `row`, `col-*`, `mb-3`) for layout
- Test screenshots at 1080p and 1440p resolutions to ensure executives see consistent layouts

### Evidence & Reasoning
Bootstrap is battle-tested for dashboards and reports (used by Datadog, Mixpanel). For single-page local apps, CDN delivery eliminates build complexity. Print media queries are essential—executives will print or screenshot; custom CSS ensures clean output. Avoids Node.js build pipeline, keeping deployment simple (just .NET).

---

## Sub-Question 5: File Monitoring & Real-Time Refresh

### Key Findings
FileSystemWatcher (System.IO) is the native .NET solution for local file monitoring. Debouncing prevents multiple rapid updates from file write events. For a single-page app, SignalR is unnecessary; direct `StateHasChanged()` in a Blazor component is sufficient.

### Tools & Libraries
- **FileSystemWatcher** (System.IO.FileSystemWatching, built-in) - No external dependency
- **System.IO.Abstractions** (v21.x, NuGet optional) - For testability; abstracts file I/O
- **Reactive Extensions (Rx.NET)** (v5.x, NuGet optional) - For elegant debouncing

### Implementation Pattern
```csharp
public class DataWatcherService : IDisposable
{
    private FileSystemWatcher _watcher;
    private Timer _debounceTimer;
    
    public event Func<Task> OnDataChanged;
    
    public void Start(string dataPath)
    {
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(dataPath), Path.GetFileName(dataPath))
        {
            NotifyFilter = NotifyFilters.LastWrite
        };
        _watcher.Changed += (s, e) => DebounceRefresh();
        _watcher.EnableRaisingEvents = true;
    }
    
    private void DebounceRefresh()
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ => OnDataChanged?.Invoke(), null, TimeSpan.FromMilliseconds(500), Timeout.Infinite);
    }
}
```

### Trade-offs & Alternatives
| Approach | Pros | Cons | Fit |
|----------|------|------|-----|
| **FileSystemWatcher** | Built-in, zero deps | Windows-specific quirks | **Recommended** |
| Timer polling | Portable, simple | CPU overhead (1s interval) | Acceptable fallback |
| System.IO.Abstractions | Testable, mockable | Small overhead | Optional; use if TDD required |
| Rx.NET debouncing | Elegant, reactive | Overkill; adds NuGet dep | Avoid |

### Concrete Recommendations
- Inject `DataWatcherService` as a singleton in Program.cs
- Subscribe to `OnDataChanged` event in `Index.razor`, call `StateHasChanged()` + reload JSON
- Debounce interval: **500ms** (balances responsiveness vs. file write race conditions)
- Log file watcher events (INFO level) for debugging; handle errors gracefully (log as WARNING, don't crash)
- Add UI feedback (e.g., "Data refreshed at HH:mm:ss") to confirm updates

### Evidence & Reasoning
FileSystemWatcher is the .NET standard for local file monitoring. For a single-user, single-machine app, the API is sufficient and requires zero external dependencies. Debouncing prevents cascading updates from rapid writes. Singleton scope ensures one monitor per app instance, avoiding resource leaks.

---

## Sub-Question 6: Local Deployment & Distribution Model

### Key Findings
For non-technical executives, self-contained .NET 8 deployment (.exe bundle) is optimal. Avoids requiring .NET runtime installation. Alternative: MSI installer for corporate deployment. Web server approach (localhost) is simplest—no firewall complexity.

### Tools & Libraries
- **Blazor Server** (built-in to .NET SDK) - Embedded Kestrel web server
- **System.Net.HttpListener** (built-in) - Alternative lightweight HTTP server
- **Wix Toolset v4.x** (free, open-source) - MSI installer generation (optional)
- **NSIS 3.x** (free) - Lightweight EXE installer alternative

### Deployment Options

**Option A: Self-Contained Executable** (RECOMMENDED)
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```
- Output: Single `.exe` file (~100–150MB with .NET runtime)
- Pros: No .NET install required; one-click launch
- Cons: Larger file size; longer first startup

**Option B: Framework-Dependent Executable** (Requires .NET 8 Runtime)
```bash
dotnet publish -c Release
```
- Output: Small `.exe` + runtime requirement
- Pros: ~10MB download; faster startup
- Cons: Requires users to install .NET 8 Runtime

**Option C: MSI Installer (Optional; Phase 2)**
- Use Wix Toolset to bundle self-contained exe + desktop shortcut
- Enables corporate deployment via Group Policy

### Trade-offs & Alternatives
| Model | Size | Setup Friction | Corporate Deployment |
|-------|------|-----------------|----------------------|
| **Self-contained .exe** | 150MB | 1-click; 5–10s startup | Low friction |
| Framework-dependent .exe | 10MB | Requires .NET 8 install | Medium friction |
| MSI installer | 150MB | Standard corporate process | High polish |
| Web server (IIS) | Variable | Requires IT; overkill | Avoid |

### Concrete Recommendations
- Target: **Self-contained single-file executable** (win-x64)
- Hosting: Embedded Kestrel (Blazor Server default); no IIS needed
- Launch behavior: App opens `http://localhost:5000` in default browser on startup
- Project file settings:
  ```xml
  <PropertyGroup>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <DebugType>embedded</DebugType>
  </PropertyGroup>
  ```
- Distribution: Upload `.exe` to shared drive or intranet; document "download and run" instructions
- Phase 2: Consider Wix MSI if roll-out to >50 executives

### Evidence & Reasoning
Self-contained deployment is industry standard for desktop .NET apps (used by NuGet Package Manager, Visual Studio). For non-technical users, a single executable eliminates environment setup. Embedded Kestrel is battle-tested, secure for localhost-only scenarios (no external routing). No IIS/Azure complexity aligns with "local only" requirement.

---

## Sub-Question 7: Print & Screenshot Rendering

### Key Findings
Browser print CSS and viewport constraints ensure consistent PowerPoint screenshots. Key levers: print margins, page-break-inside, font sizing, and color scheme. Most executives use Chrome/Edge native screenshot tools; app must render consistently at 1080p and 1440p.

### Tools & Libraries
- **CSS `@media print`** (built-in) - Print-specific styles
- **Playwright** (v1.45.x, NuGet `PlaywrightSharp`) - Automated screenshot testing (Phase 2)
- **wkhtmltopdf** (free, external) - HTML-to-PDF conversion (optional Phase 2)

### Recommended Print CSS
```css
@media print {
  /* Remove nav, footer, interactive elements */
  nav, footer, .sidebar { display: none !important; }
  
  /* Set print dimensions (8.5" x 11" standard) */
  body { 
    width: 8.5in;
    height: 11in;
    margin: 0.5in;
    font-size: 11pt;
    color: #000; /* Force black text for print */
  }
  
  /* Prevent orphaned timeline/milestone blocks */
  .timeline, .milestone-block { page-break-inside: avoid; }
  
  /* Force color printing for charts */
  canvas { print-color-adjust: exact; }
}

/* Screenshot viewport optimization */
@media (min-width: 1400px) and (max-width: 1920px) {
  .container { max-width: 1300px; }
}
```

### Screenshot Quality Checklist
- [ ] Test at 1080p (1920x1080): Text readable, chart visible
- [ ] Test at 1440p (2560x1440): No text overflow, spacing balanced
- [ ] Test Chrome DevTools screenshot tool (Cmd+Shift+P → Screenshot)
- [ ] Verify milestone timeline renders without scroll in viewport
- [ ] Confirm status boxes (Shipped/In Progress/Carried Over) fit without wrapping
- [ ] Check color contrast for accessibility + printing

### Trade-offs & Alternatives
| Approach | Effort | Output Quality | Automation |
|----------|--------|----------------|------------|
| **Manual print CSS** | Low | Good | Manual screenshots |
| Playwright screenshot automation | Medium | Excellent | Automated visual regression |
| wkhtmltopdf export | Medium | Publication-grade | Export to PDF |
| Client-side HTML2Canvas | Low | Medium | Client-based, no server |

### Concrete Recommendations
- Implement `print.css` separate from main stylesheet (cleaner maintenance)
- Add UI button: "Download Screenshot as PNG" → uses browser's native download (no backend)
- Test print layout by opening DevTools (F12 → Ctrl+Shift+P → "Emulate CSS media" → Print)
- Document executive workflow: "Print preview (Ctrl+P) → Save as PDF → Insert into PowerPoint"
- Phase 2: Add Playwright-based visual regression tests to CI/CD (ensures screenshots remain consistent)

### Evidence & Reasoning
Print CSS is web standard; supported in all modern browsers. For single-page dashboards, manual print styling is lightweight. Executives will use native browser tools (Ctrl+P, screenshot), so app must render cleanly in print preview. Avoiding external PDF tools (wkhtmltopdf) keeps deployment simple; browser printing is sufficient for PowerPoint use case.

---

## Sub-Question 8: Testing & Validation Strategy

### Key Findings
For a data-driven dashboard, testing focuses on JSON schema validation, component rendering accuracy, and screenshot consistency. Unit testing Blazor components is straightforward with xUnit; visual regression testing (Phase 2) prevents layout regressions.

### Tools & Libraries

**Testing Tier 1: Data Validation** (MUST HAVE)
- **xUnit** (v2.7.x, NuGet) - Test framework
- **FluentAssertions** (v6.x, NuGet) - Assertion library
- **System.Text.Json** validation (built-in) - JSON deserialization

**Testing Tier 2: Component Rendering** (SHOULD HAVE)
- **bUnit** (v1.28.x, NuGet) - Blazor component testing library
- **Moq** (v4.x, NuGet) - Mocking; test in isolation

**Testing Tier 3: Visual Regression** (NICE TO HAVE, Phase 2)
- **Playwright** (v1.45.x, NuGet `PlaywrightSharp`) - Screenshot automation + assertions

### Test Coverage Strategy

**Tier 1: Unit Tests (Data Layer)**
```csharp
[Fact]
public void JsonDeserialization_ValidMilestones_ParsesCorrectly()
{
    var json = """{"milestones": [{"id": "m1", "name": "Launch", "status": "on-track"}]}""";
    var report = JsonSerializer.Deserialize<ProjectReport>(json);
    
    Assert.NotNull(report);
    Assert.Single(report.Milestones);
    Assert.Equal("m1", report.Milestones[0].Id);
}

[Fact]
public void InvalidMilestoneStatus_ThrowsValidationError()
{
    var json = """{"milestones": [{"id": "m1", "status": "invalid-status"}]}""";
    Assert.Throws<ArgumentException>(() => ValidateSchema(json));
}
```

**Tier 2: Component Tests (Rendering)**
```csharp
[Fact]
public async Task MilestoneTimeline_RendersMilestones_AsExpected()
{
    var cut = RenderComponent<MilestoneTimeline>(
        parameters => parameters.Add(m => m.Milestones, new[] { /* test data */ })
    );
    
    Assert.Contains("Launch", cut.Markup);
    Assert.Contains("on-track", cut.Markup);
}
```

**Tier 3: Visual Regression (Automated Screenshots)**
```csharp
// Phase 2: Playwright-based screenshot diff
[Fact]
public async Task Dashboard_ScreenshotConsistency_Matches1080pBaseline()
{
    var page = await browser.NewPageAsync(new() { ViewportSize = new(1920, 1080) });
    await page.GotoAsync("http://localhost:5000");
    var screenshot = await page.ScreenshotAsync();
    
    AssertScreenshotMatches(screenshot, "baseline-1080p.png");
}
```

### Trade-offs & Alternatives
| Testing Approach | Coverage | Effort | Phase |
|------------------|----------|--------|-------|
| **JSON validation** | 80% of bugs | Low | MVP |
| **Component unit tests** | 90% | Medium | MVP |
| **Visual regression** | 95% | High | Phase 2 |
| **Manual QA testing** | 70% | Very High | Avoid |
| **Load/stress testing** | N/A (single-user) | Zero | Skip |

### Concrete Recommendations
- **MVP Phase:** Unit tests (Tier 1) + component tests for MilestoneTimeline, StatusSnapshot components
- Write JSON schema validator as standalone service; test all edge cases (missing fields, invalid enums, date formats)
- Target minimum: **80% code coverage** for data layer, **70% for components**
- Add data validation tests before any feature development; test "happy path" + edge cases per milestone/status
- Phase 2 CI/CD: Add Playwright visual regression tests; capture 1080p + 1440p baselines
- Log all validation errors to browser console (executive users can screenshot errors for support)

### Evidence & Reasoning
For data-driven dashboards, schema validation prevents silent failures (corrupted JSON → blank UI). xUnit + bUnit are .NET standard for Blazor testing; well-documented, proven libraries. Visual regression testing defers to Phase 2 (post-launch) because initial version is feature-complete; regression prevention becomes valuable once design stabilizes. Skipping load testing aligns with single-user, local-only use case.

---

## Summary Table: Recommended Tech Stack

| Component | Technology | Version | Rationale |
|-----------|-----------|---------|-----------|
| **Runtime** | .NET 8 | 8.0+ | Mandatory; LTS support, Blazor Server stable |
| **UI Framework** | Blazor Server | 8.0+ | Built-in data binding, FileSystemWatcher integration |
| **JSON Serialization** | System.Text.Json | Built-in | Zero external deps, source generators, fast |
| **Charting** | Chart.js (CDN) | 4.4.x | Lightweight, screenshot-friendly, no build pipeline |
| **CSS Framework** | Bootstrap 5.3 (CDN) | 5.3.x | Professional defaults, print-friendly, no build pipeline |
| **File Monitoring** | FileSystemWatcher | Built-in | Native, zero overhead, debounce 500ms |
| **Deployment** | Self-contained .exe | win-x64 | Single-click launch, no runtime install required |
| **Testing** | xUnit + bUnit | 2.7 / 1.28 | Standard Blazor testing; Playwright Phase 2 |

---

## Key Decisions: Upfront vs. Deferred

### Decide Now (MVP)
✅ Data.json schema and validation logic  
✅ Blazor component architecture (MilestoneTimeline, StatusSnapshot)  
✅ Bootstrap print CSS for screenshot quality  
✅ FileSystemWatcher for file monitoring  
✅ Self-contained deployment model  
✅ Unit + component tests (Tier 1 & 2)  

### Defer to Phase 2
⏸ PDF export (wkhtmltopdf integration)  
⏸ Visual regression testing (Playwright)  
⏸ MSI installer packaging (Wix)  
⏸ Multi-user sync (SignalR)  
⏸ Cloud backup of data.json (breaks "local only" constraint)  
⏸ Performance metrics & observability  

---

## Critical Risks & Mitigation

| Risk | Impact | Mitigation |
|------|--------|-----------|
| **FileSystemWatcher misses rapid file writes** | Data out-of-sync | Implement 500ms debounce; log all changes |
| **Chart.js rendering fails in print/screenshot** | Broken PowerPoint visuals | Test print preview in Chrome/Edge; use CDN with fallback |
| **JSON schema validation too strict** | Executives can't edit data.json | Keep schema loose (nullable fields where sensible); doc required fields |
| **Single-file .exe too large (150MB)** | Slow distribution | Acceptable for corporate intranet; compress if needed (7-Zip to ~40MB) |
| **Timestamp misalignment between data.json edits and UI** | Stale data displayed | Use file write time as cache buster; log refresh events to UI |

---

## Team Skills & Onboarding

**Required Skills:**
- C# basics (POCO serialization, async/await)
- Blazor Server fundamentals (components, data binding, lifecycle)
- HTML/CSS (Bootstrap, print media)

**Learning Curve:**
- **Blazor Server novice → proficient:** 1–2 weeks
- **Component architecture (MilestoneTimeline):** 2–3 days
- **Print CSS tuning:** 1–2 days

**Recommended Pre-Work:**
- Microsoft Learn: "Build web applications with Blazor" (2–3 hours)
- Bootstrap documentation: Print CSS section (30 min)
- FileSystemWatcher code samples (30 min)
