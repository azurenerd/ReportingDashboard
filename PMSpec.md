# PM Specification: Executive Project Reporting Dashboard

## Executive Summary

We are building a single-page, local-only Blazor Server web application that renders an executive-friendly project status dashboard, designed to be screenshotted and embedded into PowerPoint decks for leadership reporting. The dashboard reads all project data from a flat `data.json` configuration file and displays milestones, shipped work, in-progress items, carried-over items, and a visual timeline—optimized for visual clarity, print-readiness, and zero operational complexity. The design is based on an existing proven HTML template (`OriginalDesignConcept.html`) and a companion design mockup (`ReportingDashboardDesign.png`), which serve as the authoritative visual references.

## Business Goals

1. **Reduce executive reporting preparation time** by providing a consistently formatted, screenshot-ready dashboard that eliminates manual slide creation for project status updates.
2. **Improve project visibility** by presenting milestones, shipped deliverables, in-progress work, and carryover items in a single, scannable view tailored for executive consumption.
3. **Minimize operational overhead** by using a flat-file data model (`data.json`) with no database, no authentication, no cloud dependencies, and no deployment pipeline—runnable with a single `dotnet run` command.
4. **Ensure visual consistency** across reporting periods by enforcing a fixed-width (1200px), high-contrast, print-friendly layout that produces identical screenshots regardless of monitor size or browser window dimensions.
5. **Enable rapid iteration** on report content by supporting hot reload (`dotnet watch`) and optional auto-refresh when the data file changes, so updates to `data.json` are reflected immediately in the browser.

## User Stories & Acceptance Criteria

### US-1: View Project Summary at a Glance

**As an** executive report author, **I want** to see the project name, lead, overall health status, last-updated date, and a brief summary at the top of the dashboard, **so that** I can immediately communicate the project's current state without scrolling.

**Acceptance Criteria:**
- [ ] The dashboard header displays the project name, project lead, overall status (e.g., "On Track"), and the last-updated date, all sourced from `data.json`.
- [ ] An overall health indicator is visible using color-coded styling (green = on-track, orange = at-risk, red = behind).
- [ ] A project summary paragraph is rendered directly below the header metadata.
- [ ] All header information is visible without scrolling on a 1920×1080 display.

### US-2: View Milestone Timeline

**As an** executive report author, **I want** to see a horizontal visual timeline of project milestones at the top of the page, **so that** I can communicate the project's key dates and progress through major phases.

**Acceptance Criteria:**
- [ ] A horizontal timeline renders all milestones from `data.json` in chronological order.
- [ ] Each milestone displays its title, target date, and current status.
- [ ] Completed milestones are visually distinct (e.g., green) from in-progress (blue), upcoming (gray), and at-risk (orange) milestones.
- [ ] The timeline uses CSS Grid and does not require JavaScript or any external charting library.
- [ ] The timeline is fully visible and legible when screenshotted at 1200px width.

### US-3: View Shipped Items

**As an** executive report author, **I want** to see a section listing all items shipped/completed this reporting period, **so that** I can highlight team accomplishments to leadership.

**Acceptance Criteria:**
- [ ] A "Shipped" section lists all items from the `shipped` array in `data.json`.
- [ ] Each item displays its title, description, category, and a 100% completion indicator.
- [ ] Items are styled with a colored left border indicating "completed" status (green).
- [ ] The section is clearly labeled and visually separated from other sections.

### US-4: View In-Progress Items

**As an** executive report author, **I want** to see all currently in-progress work items with their completion percentage, **so that** I can communicate active workstreams and their progress.

**Acceptance Criteria:**
- [ ] An "In Progress" section lists all items from the `inProgress` array in `data.json`.
- [ ] Each item displays its title, description, category, and a visual progress bar showing `percentComplete`.
- [ ] Progress bars render proportionally (e.g., 65% fills 65% of the bar width) using pure CSS.
- [ ] Items are styled with a colored left border indicating "in-progress" status (blue).

### US-5: View Carried-Over Items

**As an** executive report author, **I want** to see items carried over from the previous reporting period, **so that** I can transparently communicate deferred work and its reason.

**Acceptance Criteria:**
- [ ] A "Carried Over" section lists all items from the `carriedOver` array in `data.json`.
- [ ] Each item displays its title, description, category, and the reason for carryover.
- [ ] Items are styled with a colored left border indicating "carried-over" status (orange).
- [ ] The section is visually distinct so executives can quickly identify deferred work.

### US-6: View Monthly Summary Metrics

**As an** executive report author, **I want** to see a summary of the current month's metrics (total items, completed, carried over, overall health), **so that** I can provide a quick quantitative snapshot.

**Acceptance Criteria:**
- [ ] A summary section or card displays `totalItems`, `completedItems`, `carriedItems`, and `overallHealth` from `data.json`.
- [ ] The overall health value is color-coded (on-track = green, at-risk = orange, behind = red).
- [ ] Numeric values are prominently displayed (large font, easy to read in a screenshot).

### US-7: Configure Dashboard via JSON File

**As a** report author, **I want** to update the dashboard content by editing a single `data.json` file in a text editor, **so that** I can update project data without modifying any code.

**Acceptance Criteria:**
- [ ] All displayed data is sourced exclusively from `data.json`; no data is hardcoded in Razor or C# files.
- [ ] The `data.json` file uses a clear, documented schema matching the C# record types.
- [ ] A `data.template.json` file is provided with all fields documented as a reference.
- [ ] Invalid or missing optional fields in `data.json` do not crash the application (graceful degradation with nullable properties).

### US-8: Screenshot-Ready Layout

**As a** report author, **I want** the dashboard to render at a fixed width with high-contrast, print-friendly styling, **so that** my screenshots are consistent and look professional in PowerPoint decks.

**Acceptance Criteria:**
- [ ] The page layout is fixed at `max-width: 1200px`, centered horizontally.
- [ ] Body text is 14px or larger; headings are 18px or larger.
- [ ] No information is hidden behind hover states, tooltips, or interactive elements.
- [ ] Status colors meet WCAG AA contrast ratios against their backgrounds.
- [ ] A `@media print` stylesheet is included for clean Ctrl+P output.
- [ ] The entire dashboard fits on a single scrollable page with no horizontal overflow.

### US-9: Run Locally with Zero Configuration

**As a** developer/report author, **I want** to start the dashboard with a single `dotnet run` command and view it at `http://localhost:5000`, **so that** I don't need to configure servers, databases, or cloud services.

**Acceptance Criteria:**
- [ ] The application starts with `dotnet run` (or `dotnet watch` for hot reload) with no additional setup.
- [ ] The dashboard is accessible at `http://localhost:5000` (or configured port) in any modern browser.
- [ ] No database, cloud service, API key, or authentication is required.
- [ ] The application runs on .NET 8 SDK with zero external NuGet dependencies beyond the framework reference.

### US-10: Auto-Refresh on Data Change (Optional Enhancement)

**As a** report author actively editing `data.json`, **I want** the browser to automatically reflect my changes without manually refreshing, **so that** I can iterate quickly on report content.

**Acceptance Criteria:**
- [ ] A `FileSystemWatcher` monitors `data.json` for changes.
- [ ] When `data.json` is saved, the Blazor Server page re-renders with updated data via SignalR push.
- [ ] No manual browser refresh is required after editing and saving `data.json`.

## Scope

### In Scope

- Single-page Blazor Server dashboard rendering project status data from `data.json`
- Visual design ported from `OriginalDesignConcept.html` and `ReportingDashboardDesign.png` with improvements
- Horizontal milestone timeline using CSS Grid
- Sections for: project summary, milestones, shipped items, in-progress items (with progress bars), carried-over items, and monthly summary metrics
- Color-coded status indicators (completed, in-progress, at-risk, carried-over, upcoming)
- Fixed-width (1200px) layout optimized for screenshots and print
- C# record-based data models matching the `data.json` schema
- `DashboardDataService` for reading and deserializing `data.json`
- Sample `data.json` with fictional "Project Atlas" data
- `data.template.json` for documentation of all supported fields
- Custom CSS with CSS custom properties for easy theming
- `@media print` stylesheet for Ctrl+P output
- System font stack (no external font dependencies)
- `FileSystemWatcher`-based auto-refresh when `data.json` changes (optional but recommended)
- Hot reload support via `dotnet watch`
- Solution structure following .NET conventions (`ReportingDashboard.sln` with `src/` project folder)

### Out of Scope

- **Authentication or authorization** — no login, no user accounts, no role-based access; the app is local-only
- **Database** — no SQLite, SQL Server, CosmosDB, or any persistent store; `data.json` is the sole data source
- **REST API or Web API endpoints** — no controllers, no API surface; data is read server-side only
- **Admin UI for editing data** — users edit `data.json` directly in a text editor or VS Code
- **Multi-user or networked access** — designed for single-user localhost use only
- **Client-side Blazor (WebAssembly)** — Blazor Server is the specified and correct rendering mode
- **UI component libraries** (MudBlazor, Radzen, Telerik, etc.) — custom CSS only, matching the original HTML design
- **JavaScript frameworks or libraries** — zero JS is the target; Blazor Server handles all rendering
- **Docker containerization** — unnecessary for a local-only tool
- **CI/CD pipeline** — no automated build, test, or deployment infrastructure
- **Cloud deployment** (Azure App Service, AWS, etc.) — runs exclusively on localhost
- **HTTPS/TLS** — HTTP-only to avoid localhost certificate warnings
- **Logging infrastructure** — console output is sufficient
- **Error pages or 404 handling** — single-page application with one route
- **Multiple simultaneous projects on one page** — one project per `data.json` (future extension: multiple JSON files)
- **Real-time data integrations** (Azure DevOps, Jira, GitHub Projects) — data is manually curated in JSON
- **Accessibility compliance beyond color contrast** — optimized for screenshot consumption, not screen reader use
- **Mobile-responsive design** — fixed 1200px width, desktop-only consumption model
- **Internationalization / localization** — English only

## Non-Functional Requirements

### Performance

| Metric | Target | Rationale |
|--------|--------|-----------|
| **Application startup time** | < 5 seconds from `dotnet run` to page-ready | Developer experience; fast iteration cycle |
| **Page load time** | < 1 second (localhost, first meaningful paint) | Instant visual feedback when loading the dashboard |
| **Data file read** | < 100ms for `data.json` deserialization | File is expected to be < 50KB; `System.Text.Json` handles this trivially |
| **Hot reload cycle** | < 2 seconds from file save to browser update | `dotnet watch` provides this out of the box for `.razor` and `.css` files |

### Reliability

| Metric | Target | Rationale |
|--------|--------|-----------|
| **Availability** | Available whenever the local machine is running `dotnet run` | No SLA required; local-only tool |
| **Graceful degradation** | Missing optional fields in `data.json` must not crash the app | Use nullable C# properties; render "N/A" or hide sections for missing data |
| **Data validation** | Malformed `data.json` displays a user-friendly error message, not an unhandled exception | Wrap deserialization in try-catch; show a clear "Invalid data.json" message |

### Security

| Requirement | Implementation |
|-------------|---------------|
| **No authentication** | App binds to `localhost` only; no network exposure by default |
| **No sensitive data** | `data.json` contains project status metadata, not PII or credentials |
| **Source control safety** | `.gitignore` should include `data.json` if project names are sensitive; `data.template.json` is committed |
| **No external network calls** | The application makes zero outbound HTTP requests; no CDN, no analytics, no telemetry |

### Scalability

- **Not applicable.** This is a single-user, single-page, local-only application. It will never need to handle concurrent users, horizontal scaling, or load balancing.
- **Data volume:** A single project's data (`data.json`) will be < 50KB. The application does not need to handle large datasets.

### Maintainability

| Requirement | Target |
|-------------|--------|
| **External dependencies** | Zero NuGet packages beyond `Microsoft.AspNetCore.App` framework reference |
| **Code complexity** | < 5 C# files total (models, service, Program.cs); < 200 lines of C# code |
| **CSS maintainability** | Use CSS custom properties for all colors and key spacing values |
| **.NET version** | .NET 8 LTS (supported through November 2026) |
| **Upgrade path** | When .NET 8 reaches end-of-life, upgrade to the next LTS version with minimal effort due to zero external dependencies |

### Visual Quality (Screenshot-Specific)

| Requirement | Target |
|-------------|--------|
| **Fixed layout width** | 1200px max-width, centered |
| **Minimum font size** | 14px body, 18px headings |
| **Color contrast** | WCAG AA minimum (4.5:1 for normal text, 3:1 for large text) |
| **Print output** | Clean Ctrl+P output via `@media print` rules (no navigation chrome, no SignalR artifacts) |
| **Cross-browser consistency** | Tested and optimized for Chrome/Edge (primary screenshot browser) |

## Success Metrics

### MVP Completion Criteria

1. **Dashboard renders correctly** — All six data sections (header, timeline, shipped, in-progress, carried-over, monthly summary) display data from `data.json` accurately and match the visual intent of `OriginalDesignConcept.html` and `ReportingDashboardDesign.png`.
2. **Screenshot quality** — A full-page screenshot taken in Chrome at 1200px width produces a clean, professional image suitable for direct embedding in a PowerPoint slide without post-processing.
3. **Zero-config startup** — A new developer can clone the repo, run `dotnet run`, navigate to `http://localhost:5000`, and see the fully rendered dashboard in under 60 seconds.
4. **Data-driven rendering** — Changing values in `data.json` and refreshing the browser (or via auto-refresh) updates all displayed content with zero code changes.
5. **Sample data provided** — The repository includes a complete `data.json` with the fictional "Project Atlas" data and a documented `data.template.json` for creating new project reports.

### Quality Gates

| Gate | Criteria | Verification Method |
|------|----------|-------------------|
| **Build** | `dotnet build` completes with zero errors and zero warnings | Command-line build |
| **Run** | `dotnet run` starts Kestrel and serves the dashboard on the configured port | Manual verification |
| **Visual fidelity** | Dashboard layout matches the design intent from the HTML template and PNG mockup | Side-by-side visual comparison |
| **Screenshot test** | Full-page screenshot at 1200px width is clean, complete, and readable | Manual screenshot in Chrome |
| **Data independence** | Replacing `data.json` with different valid data renders correctly without code changes | Swap `data.json`, refresh browser |
| **Print output** | Ctrl+P produces a clean, single-page (or minimal multi-page) printout | Browser print preview |

## Constraints & Assumptions

### Technical Constraints

1. **Technology stack is fixed:** C# .NET 8, Blazor Server, `.sln` project structure. These are non-negotiable project parameters.
2. **Design source of truth:** `OriginalDesignConcept.html` and `ReportingDashboardDesign.png` are the authoritative visual references. All agents must read the HTML design file before producing any plan or code.
3. **No external runtime dependencies:** The application must run with only the .NET 8 SDK installed. No Node.js, no npm, no Python, no Docker.
4. **No external NuGet packages:** The entire application is built using only the `Microsoft.AspNetCore.App` framework reference. No third-party packages.
5. **No JavaScript:** The dashboard must function with zero client-side JavaScript. All rendering is handled by Blazor Server.
6. **Fixed-width layout:** The page must be designed for 1200px width to ensure screenshot consistency. No responsive breakpoints below 1200px.
7. **HTTP only:** No HTTPS configuration to avoid localhost certificate complexity.
8. **Single route:** The application serves one page at one URL. No routing complexity.

### Timeline Assumptions

1. **Phase 1 (Core MVP):** Estimated 2–4 hours — scaffold project, create models/service, build the Razor page, port CSS, create sample data.
2. **Phase 2 (Visual Polish):** Estimated 1–2 hours — refine the timeline, progress bars, status indicators, print styles, and typography.
3. **Phase 3 (Quality of Life, Optional):** Estimated 1 hour — auto-refresh, CSS custom properties, `data.template.json`, potential multi-project routing.
4. **Total estimated effort:** 4–7 hours for a complete, polished dashboard.

### Dependency Assumptions

1. **.NET 8 SDK** is installed on the development machine (version 8.0.x, any recent patch).
2. **A modern browser** (Chrome or Edge recommended) is available for viewing and screenshotting.
3. **`OriginalDesignConcept.html`** exists in the `ReportingDashboard` repository and is accessible to all agents at build time.
4. **`ReportingDashboardDesign.png`** exists at `C:/Pics/ReportingDashboardDesign.png` and represents the target visual layout.
5. **Visual Studio 2022 (17.8+)** or **VS Code with C# Dev Kit** is available for development (not strictly required; any text editor + CLI works).
6. **No network access required** at runtime — the application operates entirely offline after `dotnet restore` downloads the SDK workload.

### Operational Assumptions

1. The dashboard is consumed by a **single user** (the report author) on their local machine.
2. The primary output is **screenshots** embedded in PowerPoint; the browser view is a means to an end, not the final deliverable.
3. Data in `data.json` is **manually curated** by the report author; there is no automated data pipeline.
4. The application will be **started and stopped on demand** — it does not need to run as a persistent service or daemon.
5. **Report frequency** is assumed to be monthly, aligning with typical executive reporting cadence.
6. The report author has **basic familiarity** with JSON syntax for editing `data.json` (or uses VS Code with JSON validation).