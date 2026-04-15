# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-15 14:37 UTC_

### Summary

This project is a lightweight, single-page executive reporting dashboard built with **Blazor Server on .NET 8**, designed to be screenshot-friendly for PowerPoint decks. It renders a timeline/Gantt view of project milestones and a color-coded heatmap grid showing shipped, in-progress, carryover, and blocked items — all driven by a local `data.json` file with zero authentication or cloud dependencies. **Primary recommendation:** Keep the architecture dead simple. Use a single Blazor Server project with no database, no authentication, and no external services. Render the timeline SVG inline using Blazor component markup. Read `data.json` via `System.Text.Json` at startup and on file-change. Use pure CSS (Grid + Flexbox) matching the design reference — no CSS framework needed. The entire solution should be one `.sln` with one project, deployable via `dotnet run`. --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.Components` | 8.0.x (built-in) | Blazor Server framework | Yes (implicit) | | `System.Text.Json` | 8.0.x (built-in) | JSON deserialization | Yes (implicit) | | `bunit` | 1.25+ | Blazor component unit testing | Optional (Phase 2) | | `xunit` | 2.7+ | Test runner | Optional (Phase 2) | | `Microsoft.Playwright` | 1.40+ | Automated screenshot capture | Optional (Phase 2) | **Total external dependencies for MVP: 0 NuGet packages.** Everything needed is included in the .NET 8 SDK.

### Key Findings

- The original `OriginalDesignConcept.html` is a static 1920×1080 layout using CSS Grid (`160px repeat(4,1fr)`), Flexbox, and inline SVG — all of which translate directly to Blazor `.razor` components with zero JavaScript required.
- **No charting library is needed.** The timeline visualization is a simple inline SVG with lines, circles, diamonds, and text — easily rendered as a Blazor component with `@foreach` loops over milestone data.
- **No database is needed.** A `data.json` file is the correct storage mechanism for this use case — it's human-editable, version-controllable, and trivially deserialized with `System.Text.Json`.
- Blazor Server's SignalR circuit is irrelevant for this use case (single user, local), but it provides hot-reload during development which accelerates iteration on the visual layout.
- The design targets a fixed 1920×1080 viewport for screenshot fidelity — responsive design is explicitly **not** a priority. The CSS should use fixed pixel widths matching the reference.
- `System.Text.Json` source generators (available in .NET 8) provide AOT-friendly, zero-reflection JSON deserialization — ideal even for this simple project.
- A `FileSystemWatcher` on `data.json` enables live-reload of dashboard data without restarting the app.
- The color palette and typography (Segoe UI) from the HTML reference are directly usable in Blazor's scoped CSS via `::deep` or global stylesheets. --- **Goal:** Pixel-faithful reproduction of the HTML reference, data-driven from `data.json`.
- **Scaffold the project** — `dotnet new blazor -n ReportingDashboard --interactivity Server` (creates .NET 8 Blazor Server app).
- **Define the data model** — Create `DashboardData.cs` with POCOs matching the JSON schema above.
- **Create `data.json`** — Populate with fictional project data (e.g., "Project Phoenix" with milestones for an API platform migration).
- **Build `DashboardDataService`** — Singleton service, reads JSON on startup, exposes `DashboardData` property.
- **Port the CSS** — Copy styles from `OriginalDesignConcept.html` into `dashboard.css`, adjusting class names to match Blazor conventions.
- **Build components top-down:**
- `Dashboard.razor` (page, injects data service)
- `Header.razor` (title, subtitle, legend)
- `Timeline.razor` (SVG rendering with date math)
- `Heatmap.razor` + `HeatmapCell.razor` (CSS Grid)
- **Test at 1920×1080** — Open in browser, resize to 1920×1080 (or use DevTools device emulation), compare against reference screenshot.
- **FileSystemWatcher** — Auto-reload dashboard when `data.json` changes. Inject a state change notification via `InvokeAsync(StateHasChanged)`.
- **CLI argument for data file** — Support `dotnet run -- --data ./other-project.json`.
- **Improve the design** beyond the reference:
- Add subtle hover tooltips on heatmap items (Blazor `@onmouseover`).
- Add a "Last Updated" timestamp in the footer.
- Animate the NOW line with a subtle CSS pulse.
- **Playwright screenshot script** — Headless Chromium captures the page at 1920×1080 as `dashboard.png`.
- Multiple project support (dropdown or tabbed navigation).
- Historical snapshots (`data/2026-04.json`, `data/2026-03.json`) with month navigation.
- Print-friendly CSS (`@media print`).
- Export to PDF via browser print dialog.
- **Hot reload is built in.** Run `dotnet watch run` and iterate on the layout in real time.
- **The hardest part is the SVG timeline.** Start with a hardcoded SVG that looks right, then parameterize it with data. Don't try to make it data-driven on the first pass.
- **Copy the CSS first.** The reference HTML's CSS is 90% of the visual design. Port it verbatim, then refactor.
- **Use browser DevTools at 1920×1080** from the very first build to catch layout issues immediately. ---

### Recommended Tools & Technologies

- **Project:** Executive Reporting Dashboard — Single-Page Milestone & Progress Viewer **Date:** April 15, 2026 **Stack:** C# .NET 8 · Blazor Server · Local-only · .sln Structure --- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (.NET 8) | `net8.0` | Built-in, no additional package. Server-side rendering with SignalR. | | **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Matches the reference design exactly. No CSS framework (Bootstrap/Tailwind) needed — they'd add bloat and fight the pixel-perfect layout. | | **SVG Timeline** | Inline SVG in Razor markup | N/A | Render `<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` elements directly in `.razor` files. No charting library needed. | | **Icons** | None required | N/A | The design uses colored shapes (diamonds, circles, lines) as legend indicators — all achievable with CSS/SVG. | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`. Consider source generators for clean code. | | **File Watching** | `System.IO.FileSystemWatcher` | Built into .NET 8 | Watch `data.json` for changes; trigger UI refresh via injected state service. | | **Configuration** | `IConfiguration` / direct file read | Built into .NET 8 | For `data.json`, direct `File.ReadAllText` + deserialize is simpler than `IConfiguration` binding. | | Layer | Technology | Notes | |-------|-----------|-------| | **Primary Store** | `data.json` flat file | No database. JSON is human-editable, Git-friendly, and sufficient for a single-page dashboard. | | **Schema** | C# POCO classes | `DashboardData`, `Milestone`, `HeatmapRow`, `HeatmapItem` — strongly typed models. | | Tool | Package / Version | Purpose | |------|------------------|---------| | **SDK** | .NET 8 SDK (`8.0.x`) | Build and run | | **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Development | | **Unit Testing** | `xunit` 2.7+ / `bunit` 1.25+ | Component testing for Blazor | | **Hot Reload** | Built into `dotnet watch run` | Iterate on layout without restart | | **Screenshot Testing** (optional) | Playwright for .NET (`Microsoft.Playwright` 1.40+) | Automated screenshot capture at 1920×1080 for regression testing | | Concern | Approach | |---------|----------| | **Hosting** | `dotnet run` on localhost, Kestrel default | | **Deployment** | Copy + run, or `dotnet publish -c Release` | | **No containerization needed** | Single-user local tool | --- This is intentionally the simplest possible architecture. No layers, no abstractions, no repositories, no CQRS. The entire app is:
```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── Program.cs                  # Minimal hosting setup
    ├── data.json                   # Dashboard data (editable)
    ├── Models/
    │   └── DashboardData.cs        # POCO models for JSON shape
    ├── Services/
    │   └── DashboardDataService.cs # Reads & caches data.json, watches for changes
    ├── Components/
    │   ├── App.razor               # Root component
    │   ├── Layout/
    │   │   └── MainLayout.razor    # Shell layout (minimal)
    │   └── Pages/
    │       └── Dashboard.razor     # The single page
    ├── Components/Shared/
    │   ├── Header.razor            # Title bar + legend
    │   ├── Timeline.razor          # SVG milestone timeline
    │   ├── Heatmap.razor           # CSS Grid status matrix
    │   └── HeatmapCell.razor       # Individual cell with item bullets
    └── wwwroot/
        └── css/
            └── dashboard.css       # Global styles matching design reference
```
```
data.json  →  DashboardDataService (singleton, cached)  →  Dashboard.razor  →  Child components
                     ↑
           FileSystemWatcher triggers reload
```
- **Header.razor** — Project title, subtitle (team/workstream/date), and legend (PoC Milestone diamond, Production Release diamond, Checkpoint circle, Now line).
- **Timeline.razor** — Accepts `List<Milestone>` and renders an inline `<svg>` with:
- Month grid lines (vertical `<line>` elements at calculated X positions)
- Horizontal track lines per milestone stream (colored `<line>`)
- Diamond markers (`<polygon>`) for PoC and Production milestones
- Circle markers (`<circle>`) for checkpoints
- "NOW" dashed vertical line at calculated position based on current date
- Date labels (`<text>`)
- **Heatmap.razor** — Accepts `HeatmapData` with four category rows (Shipped, In Progress, Carryover, Blockers) × N month columns. Renders using CSS Grid matching the `160px repeat(N, 1fr)` pattern.
- **HeatmapCell.razor** — Renders bulleted item list with category-colored dot indicators.
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "timeline": {
    "startMonth": "2026-01",
    "endMonth": "2026-06",
    "tracks": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["January", "February", "March", "April"],
    "currentMonth": "April",
    "categories": [
      {
        "name": "Shipped",
        "cssClass": "ship",
        "items": {
          "January": ["Item A", "Item B"],
          "February": ["Item C"],
          "March": [],
          "April": ["Item D", "Item E"]
        }
      }
    ]
  }
}
```
- **Do NOT use Bootstrap, Tailwind, or MudBlazor.** The design is pixel-specific and any component library would fight the layout rather than help it.
- Copy the CSS from `OriginalDesignConcept.html` as the baseline, then refactor into scoped component CSS or a single `dashboard.css`.
- Use CSS Grid for the heatmap: `grid-template-columns: 160px repeat(var(--month-count), 1fr)`.
- Use Flexbox for the header and timeline label sidebar.
- Set `body { width: 1920px; height: 1080px; overflow: hidden; }` to ensure screenshot-perfect rendering.
- Font: `'Segoe UI', Arial, sans-serif` — available on all Windows machines (the target environment). The timeline is simple enough to render with raw SVG markup in Razor. Key calculations:
```csharp
// In Timeline.razor
double GetXPosition(DateOnly date) {
    var totalDays = (endDate.DayNumber - startDate.DayNumber);
    var dayOffset = (date.DayNumber - startDate.DayNumber);
    return (dayOffset / (double)totalDays) * svgWidth;
}
``` Milestone types map to SVG shapes:
- **Checkpoint** → `<circle>` (small, gray fill)
- **PoC** → `<polygon>` (diamond, `#F4B400` gold fill)
- **Production** → `<polygon>` (diamond, `#34A853` green fill)
- **Now line** → `<line>` (dashed, `#EA4335` red) ---

### Considerations & Risks

- **None.** This is explicitly a local-only, single-user tool. No auth middleware, no identity providers, no cookies.
- `Program.cs` should be minimal: `builder.Services.AddRazorComponents().AddInteractiveServerComponents();` — no auth services registered.
- `data.json` contains project status information, not PII or secrets. No encryption needed.
- If the JSON ever contains sensitive project names, rely on OS-level file permissions (NTFS ACLs on Windows).
- **Local Kestrel only.** Bind to `localhost:5000` (HTTP) or `localhost:5001` (HTTPS).
- For HTTPS, use the .NET dev cert: `dotnet dev-certs https --trust`.
- No reverse proxy, no IIS, no Docker needed.
- Deployment: `dotnet publish -c Release -o ./publish` → copy folder → `dotnet ReportingDashboard.dll`.
- Or simply `dotnet run` during active use.
- **$0.** Everything runs on the developer's existing Windows machine. No cloud, no licenses, no infrastructure.
- The only "operations" concern is ensuring .NET 8 runtime is installed on any machine where this runs.
- Consider publishing as a self-contained executable: `dotnet publish -c Release --self-contained -r win-x64` (~65MB output, but zero dependency on pre-installed runtime). --- | Risk | Severity | Mitigation | |------|----------|------------| | **Blazor Server is overkill for a static page** | Low | The alternative (static HTML + JS) was considered, but Blazor provides hot reload, strong typing on the data model, and keeps the team in C#. The overhead is minimal for a local app. | | **SignalR circuit timeout** | Low | For a local app left open in a browser tab, the circuit may disconnect after inactivity. Set `options.DisconnectedCircuitRetentionPeriod` to a long value, or add a simple reconnect script. | | **FileSystemWatcher reliability on Windows** | Low | `FileSystemWatcher` can occasionally miss events. Mitigate by also supporting manual browser refresh (F5). | | **SVG complexity grows with many milestones** | Low | The inline SVG approach works well for 3-5 tracks with ~10-20 milestones. If tracks exceed 10+, consider horizontal scrolling. Unlikely for executive reporting. | | **Screenshot fidelity across machines** | Medium | The design assumes Segoe UI font and 1920×1080. If screenshotting on a non-Windows machine or different DPI, the layout may shift. Mitigate by documenting the screenshot procedure. | | Decision | Trade-off | Justification | |----------|-----------|---------------| | No database | Can't query historical data | Simplicity wins. If history is needed later, add a `data/` folder with dated JSON files. | | No component library (MudBlazor, Radzen) | Must write CSS manually | The pixel-perfect design doesn't map to any component library's grid. Manual CSS is actually less work than fighting a library. | | No JavaScript charting (Chart.js, D3) | SVG must be hand-rendered | The timeline is simple enough. A charting library would add 200KB+ of JS and require JS interop for no benefit. | | Blazor Server over Blazor WebAssembly | Requires .NET runtime on host | WASM would produce a fully static site, but Server gives hot reload and simpler debugging. For local use, the runtime requirement is fine. | | Fixed 1920×1080 layout | Not responsive | This is a feature, not a bug. The page is designed to be screenshotted at a specific resolution for slides. | ---
- **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always show a fixed quarter? **Recommendation:** Make it data-driven — render as many columns as months present in the JSON.
- **Should the "NOW" line auto-calculate from system date or be specified in `data.json`?** **Recommendation:** Auto-calculate by default, but allow an override in JSON for generating historical snapshots.
- **Will multiple dashboards (multiple projects) be needed?** If so, support a `--data` CLI argument: `dotnet run -- --data ./project-alpha.json`. **Recommendation:** Build this in from the start — it's trivial.
- **What's the screenshot workflow?** Manual browser screenshot, or automated via Playwright? If Playwright, we can add a `screenshot` command that launches headless Chromium at 1920×1080 and saves a PNG. **Recommendation:** Defer Playwright automation to Phase 2.
- **Should the design diverge from the HTML reference?** The reference uses a specific color palette and layout. Are improvements to typography, spacing, or color welcome, or should we match pixel-for-pixel? **Recommendation:** Match the structure and palette, but refine spacing and typography for polish.
- **How should the "ADO Backlog" link behave?** In the reference it's a hyperlink. Should it open the browser, or is it just informational text in screenshot mode? **Recommendation:** Keep it as a real `<a>` tag — it works when viewed live and is just blue text in screenshots. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Reporting Dashboard — Single-Page Milestone & Progress Viewer
**Date:** April 15, 2026
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln Structure

---

## 1. Executive Summary

This project is a lightweight, single-page executive reporting dashboard built with **Blazor Server on .NET 8**, designed to be screenshot-friendly for PowerPoint decks. It renders a timeline/Gantt view of project milestones and a color-coded heatmap grid showing shipped, in-progress, carryover, and blocked items — all driven by a local `data.json` file with zero authentication or cloud dependencies.

**Primary recommendation:** Keep the architecture dead simple. Use a single Blazor Server project with no database, no authentication, and no external services. Render the timeline SVG inline using Blazor component markup. Read `data.json` via `System.Text.Json` at startup and on file-change. Use pure CSS (Grid + Flexbox) matching the design reference — no CSS framework needed. The entire solution should be one `.sln` with one project, deployable via `dotnet run`.

---

## 2. Key Findings

- The original `OriginalDesignConcept.html` is a static 1920×1080 layout using CSS Grid (`160px repeat(4,1fr)`), Flexbox, and inline SVG — all of which translate directly to Blazor `.razor` components with zero JavaScript required.
- **No charting library is needed.** The timeline visualization is a simple inline SVG with lines, circles, diamonds, and text — easily rendered as a Blazor component with `@foreach` loops over milestone data.
- **No database is needed.** A `data.json` file is the correct storage mechanism for this use case — it's human-editable, version-controllable, and trivially deserialized with `System.Text.Json`.
- Blazor Server's SignalR circuit is irrelevant for this use case (single user, local), but it provides hot-reload during development which accelerates iteration on the visual layout.
- The design targets a fixed 1920×1080 viewport for screenshot fidelity — responsive design is explicitly **not** a priority. The CSS should use fixed pixel widths matching the reference.
- `System.Text.Json` source generators (available in .NET 8) provide AOT-friendly, zero-reflection JSON deserialization — ideal even for this simple project.
- A `FileSystemWatcher` on `data.json` enables live-reload of dashboard data without restarting the app.
- The color palette and typography (Segoe UI) from the HTML reference are directly usable in Blazor's scoped CSS via `::deep` or global stylesheets.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (.NET 8) | `net8.0` | Built-in, no additional package. Server-side rendering with SignalR. |
| **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Matches the reference design exactly. No CSS framework (Bootstrap/Tailwind) needed — they'd add bloat and fight the pixel-perfect layout. |
| **SVG Timeline** | Inline SVG in Razor markup | N/A | Render `<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` elements directly in `.razor` files. No charting library needed. |
| **Icons** | None required | N/A | The design uses colored shapes (diamonds, circles, lines) as legend indicators — all achievable with CSS/SVG. |

### Backend (Data Layer)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`. Consider source generators for clean code. |
| **File Watching** | `System.IO.FileSystemWatcher` | Built into .NET 8 | Watch `data.json` for changes; trigger UI refresh via injected state service. |
| **Configuration** | `IConfiguration` / direct file read | Built into .NET 8 | For `data.json`, direct `File.ReadAllText` + deserialize is simpler than `IConfiguration` binding. |

### Data Storage

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Primary Store** | `data.json` flat file | No database. JSON is human-editable, Git-friendly, and sufficient for a single-page dashboard. |
| **Schema** | C# POCO classes | `DashboardData`, `Milestone`, `HeatmapRow`, `HeatmapItem` — strongly typed models. |

### Development & Testing

| Tool | Package / Version | Purpose |
|------|------------------|---------|
| **SDK** | .NET 8 SDK (`8.0.x`) | Build and run |
| **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Development |
| **Unit Testing** | `xunit` 2.7+ / `bunit` 1.25+ | Component testing for Blazor |
| **Hot Reload** | Built into `dotnet watch run` | Iterate on layout without restart |
| **Screenshot Testing** (optional) | Playwright for .NET (`Microsoft.Playwright` 1.40+) | Automated screenshot capture at 1920×1080 for regression testing |

### Infrastructure

| Concern | Approach |
|---------|----------|
| **Hosting** | `dotnet run` on localhost, Kestrel default | 
| **Deployment** | Copy + run, or `dotnet publish -c Release` |
| **No containerization needed** | Single-user local tool |

---

## 4. Architecture Recommendations

### Overall Pattern: **Single-Project, File-Driven Blazor Server App**

This is intentionally the simplest possible architecture. No layers, no abstractions, no repositories, no CQRS. The entire app is:

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── Program.cs                  # Minimal hosting setup
    ├── data.json                   # Dashboard data (editable)
    ├── Models/
    │   └── DashboardData.cs        # POCO models for JSON shape
    ├── Services/
    │   └── DashboardDataService.cs # Reads & caches data.json, watches for changes
    ├── Components/
    │   ├── App.razor               # Root component
    │   ├── Layout/
    │   │   └── MainLayout.razor    # Shell layout (minimal)
    │   └── Pages/
    │       └── Dashboard.razor     # The single page
    ├── Components/Shared/
    │   ├── Header.razor            # Title bar + legend
    │   ├── Timeline.razor          # SVG milestone timeline
    │   ├── Heatmap.razor           # CSS Grid status matrix
    │   └── HeatmapCell.razor       # Individual cell with item bullets
    └── wwwroot/
        └── css/
            └── dashboard.css       # Global styles matching design reference
```

### Data Flow

```
data.json  →  DashboardDataService (singleton, cached)  →  Dashboard.razor  →  Child components
                     ↑
           FileSystemWatcher triggers reload
```

### Component Decomposition (Mapped to Design)

1. **Header.razor** — Project title, subtitle (team/workstream/date), and legend (PoC Milestone diamond, Production Release diamond, Checkpoint circle, Now line).

2. **Timeline.razor** — Accepts `List<Milestone>` and renders an inline `<svg>` with:
   - Month grid lines (vertical `<line>` elements at calculated X positions)
   - Horizontal track lines per milestone stream (colored `<line>`)
   - Diamond markers (`<polygon>`) for PoC and Production milestones
   - Circle markers (`<circle>`) for checkpoints
   - "NOW" dashed vertical line at calculated position based on current date
   - Date labels (`<text>`)

3. **Heatmap.razor** — Accepts `HeatmapData` with four category rows (Shipped, In Progress, Carryover, Blockers) × N month columns. Renders using CSS Grid matching the `160px repeat(N, 1fr)` pattern.

4. **HeatmapCell.razor** — Renders bulleted item list with category-colored dot indicators.

### Data Model (`data.json` Schema)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "timeline": {
    "startMonth": "2026-01",
    "endMonth": "2026-06",
    "tracks": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["January", "February", "March", "April"],
    "currentMonth": "April",
    "categories": [
      {
        "name": "Shipped",
        "cssClass": "ship",
        "items": {
          "January": ["Item A", "Item B"],
          "February": ["Item C"],
          "March": [],
          "April": ["Item D", "Item E"]
        }
      }
    ]
  }
}
```

### CSS Strategy

- **Do NOT use Bootstrap, Tailwind, or MudBlazor.** The design is pixel-specific and any component library would fight the layout rather than help it.
- Copy the CSS from `OriginalDesignConcept.html` as the baseline, then refactor into scoped component CSS or a single `dashboard.css`.
- Use CSS Grid for the heatmap: `grid-template-columns: 160px repeat(var(--month-count), 1fr)`.
- Use Flexbox for the header and timeline label sidebar.
- Set `body { width: 1920px; height: 1080px; overflow: hidden; }` to ensure screenshot-perfect rendering.
- Font: `'Segoe UI', Arial, sans-serif` — available on all Windows machines (the target environment).

### SVG Timeline Rendering

The timeline is simple enough to render with raw SVG markup in Razor. Key calculations:

```csharp
// In Timeline.razor
double GetXPosition(DateOnly date) {
    var totalDays = (endDate.DayNumber - startDate.DayNumber);
    var dayOffset = (date.DayNumber - startDate.DayNumber);
    return (dayOffset / (double)totalDays) * svgWidth;
}
```

Milestone types map to SVG shapes:
- **Checkpoint** → `<circle>` (small, gray fill)
- **PoC** → `<polygon>` (diamond, `#F4B400` gold fill)
- **Production** → `<polygon>` (diamond, `#34A853` green fill)
- **Now line** → `<line>` (dashed, `#EA4335` red)

---

## 5. Security & Infrastructure

### Authentication & Authorization
- **None.** This is explicitly a local-only, single-user tool. No auth middleware, no identity providers, no cookies.
- `Program.cs` should be minimal: `builder.Services.AddRazorComponents().AddInteractiveServerComponents();` — no auth services registered.

### Data Protection
- `data.json` contains project status information, not PII or secrets. No encryption needed.
- If the JSON ever contains sensitive project names, rely on OS-level file permissions (NTFS ACLs on Windows).

### Hosting & Deployment
- **Local Kestrel only.** Bind to `localhost:5000` (HTTP) or `localhost:5001` (HTTPS).
- For HTTPS, use the .NET dev cert: `dotnet dev-certs https --trust`.
- No reverse proxy, no IIS, no Docker needed.
- Deployment: `dotnet publish -c Release -o ./publish` → copy folder → `dotnet ReportingDashboard.dll`.
- Or simply `dotnet run` during active use.

### Infrastructure Costs
- **$0.** Everything runs on the developer's existing Windows machine. No cloud, no licenses, no infrastructure.

### Operational Concerns
- The only "operations" concern is ensuring .NET 8 runtime is installed on any machine where this runs.
- Consider publishing as a self-contained executable: `dotnet publish -c Release --self-contained -r win-x64` (~65MB output, but zero dependency on pre-installed runtime).

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **Blazor Server is overkill for a static page** | Low | The alternative (static HTML + JS) was considered, but Blazor provides hot reload, strong typing on the data model, and keeps the team in C#. The overhead is minimal for a local app. |
| **SignalR circuit timeout** | Low | For a local app left open in a browser tab, the circuit may disconnect after inactivity. Set `options.DisconnectedCircuitRetentionPeriod` to a long value, or add a simple reconnect script. |
| **FileSystemWatcher reliability on Windows** | Low | `FileSystemWatcher` can occasionally miss events. Mitigate by also supporting manual browser refresh (F5). |
| **SVG complexity grows with many milestones** | Low | The inline SVG approach works well for 3-5 tracks with ~10-20 milestones. If tracks exceed 10+, consider horizontal scrolling. Unlikely for executive reporting. |
| **Screenshot fidelity across machines** | Medium | The design assumes Segoe UI font and 1920×1080. If screenshotting on a non-Windows machine or different DPI, the layout may shift. Mitigate by documenting the screenshot procedure. |

### Trade-offs Made

| Decision | Trade-off | Justification |
|----------|-----------|---------------|
| No database | Can't query historical data | Simplicity wins. If history is needed later, add a `data/` folder with dated JSON files. |
| No component library (MudBlazor, Radzen) | Must write CSS manually | The pixel-perfect design doesn't map to any component library's grid. Manual CSS is actually less work than fighting a library. |
| No JavaScript charting (Chart.js, D3) | SVG must be hand-rendered | The timeline is simple enough. A charting library would add 200KB+ of JS and require JS interop for no benefit. |
| Blazor Server over Blazor WebAssembly | Requires .NET runtime on host | WASM would produce a fully static site, but Server gives hot reload and simpler debugging. For local use, the runtime requirement is fine. |
| Fixed 1920×1080 layout | Not responsive | This is a feature, not a bug. The page is designed to be screenshotted at a specific resolution for slides. |

---

## 7. Open Questions

1. **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always show a fixed quarter? **Recommendation:** Make it data-driven — render as many columns as months present in the JSON.

2. **Should the "NOW" line auto-calculate from system date or be specified in `data.json`?** **Recommendation:** Auto-calculate by default, but allow an override in JSON for generating historical snapshots.

3. **Will multiple dashboards (multiple projects) be needed?** If so, support a `--data` CLI argument: `dotnet run -- --data ./project-alpha.json`. **Recommendation:** Build this in from the start — it's trivial.

4. **What's the screenshot workflow?** Manual browser screenshot, or automated via Playwright? If Playwright, we can add a `screenshot` command that launches headless Chromium at 1920×1080 and saves a PNG. **Recommendation:** Defer Playwright automation to Phase 2.

5. **Should the design diverge from the HTML reference?** The reference uses a specific color palette and layout. Are improvements to typography, spacing, or color welcome, or should we match pixel-for-pixel? **Recommendation:** Match the structure and palette, but refine spacing and typography for polish.

6. **How should the "ADO Backlog" link behave?** In the reference it's a hyperlink. Should it open the browser, or is it just informational text in screenshot mode? **Recommendation:** Keep it as a real `<a>` tag — it works when viewed live and is just blue text in screenshots.

---

## 8. Implementation Recommendations

### Phase 1: MVP (1-2 days)

**Goal:** Pixel-faithful reproduction of the HTML reference, data-driven from `data.json`.

1. **Scaffold the project** — `dotnet new blazor -n ReportingDashboard --interactivity Server` (creates .NET 8 Blazor Server app).
2. **Define the data model** — Create `DashboardData.cs` with POCOs matching the JSON schema above.
3. **Create `data.json`** — Populate with fictional project data (e.g., "Project Phoenix" with milestones for an API platform migration).
4. **Build `DashboardDataService`** — Singleton service, reads JSON on startup, exposes `DashboardData` property.
5. **Port the CSS** — Copy styles from `OriginalDesignConcept.html` into `dashboard.css`, adjusting class names to match Blazor conventions.
6. **Build components top-down:**
   - `Dashboard.razor` (page, injects data service)
   - `Header.razor` (title, subtitle, legend)
   - `Timeline.razor` (SVG rendering with date math)
   - `Heatmap.razor` + `HeatmapCell.razor` (CSS Grid)
7. **Test at 1920×1080** — Open in browser, resize to 1920×1080 (or use DevTools device emulation), compare against reference screenshot.

### Phase 2: Polish & Automation (1 day, optional)

1. **FileSystemWatcher** — Auto-reload dashboard when `data.json` changes. Inject a state change notification via `InvokeAsync(StateHasChanged)`.
2. **CLI argument for data file** — Support `dotnet run -- --data ./other-project.json`.
3. **Improve the design** beyond the reference:
   - Add subtle hover tooltips on heatmap items (Blazor `@onmouseover`).
   - Add a "Last Updated" timestamp in the footer.
   - Animate the NOW line with a subtle CSS pulse.
4. **Playwright screenshot script** — Headless Chromium captures the page at 1920×1080 as `dashboard.png`.

### Phase 3: Future Enhancements (defer)

- Multiple project support (dropdown or tabbed navigation).
- Historical snapshots (`data/2026-04.json`, `data/2026-03.json`) with month navigation.
- Print-friendly CSS (`@media print`).
- Export to PDF via browser print dialog.

### Quick Wins

- **Hot reload is built in.** Run `dotnet watch run` and iterate on the layout in real time.
- **The hardest part is the SVG timeline.** Start with a hardcoded SVG that looks right, then parameterize it with data. Don't try to make it data-driven on the first pass.
- **Copy the CSS first.** The reference HTML's CSS is 90% of the visual design. Port it verbatim, then refactor.
- **Use browser DevTools at 1920×1080** from the very first build to catch layout issues immediately.

---

## Appendix: NuGet Package Summary

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.Components` | 8.0.x (built-in) | Blazor Server framework | Yes (implicit) |
| `System.Text.Json` | 8.0.x (built-in) | JSON deserialization | Yes (implicit) |
| `bunit` | 1.25+ | Blazor component unit testing | Optional (Phase 2) |
| `xunit` | 2.7+ | Test runner | Optional (Phase 2) |
| `Microsoft.Playwright` | 1.40+ | Automated screenshot capture | Optional (Phase 2) |

**Total external dependencies for MVP: 0 NuGet packages.** Everything needed is included in the .NET 8 SDK.

## Visual Design References

The following design reference files were found in the repository. These MUST be used as the canonical visual specification when building UI components.

### `OriginalDesignConcept.html`

**Type:** HTML Design Template

**Layout Structure:**
- **Header section** with title, subtitle, and legend
- **Timeline/Gantt section** with SVG milestone visualization
- **Heatmap grid** — status rows × month columns, color-coded by category
  - Shipped row (green tones)
  - In Progress row (blue tones)
  - Carryover row (yellow/amber tones)
  - Blockers row (red tones)

**Key CSS Patterns:**
- Uses CSS Grid layout
- Uses Flexbox layout
- Color palette: #FFFFFF, #111, #0078D4, #888, #FAFAFA, #F5F5F5, #999, #FFF0D0, #C07700, #333, #1B7A28, #E8F5E9, #F0FBF0, #D8F2DA, #34A853
- Font: Segoe UI
- Grid columns: `160px repeat(4,1fr)`
- Designed for 1920×1080 screenshot resolution

<details><summary>Full HTML Source</summary>

```html
<!DOCTYPE html><html lang="en"><head><meta charset="UTF-8">
<style>
*{margin:0;padding:0;box-sizing:border-box;}
body{width:1920px;height:1080px;overflow:hidden;background:#FFFFFF;
     font-family:'Segoe UI',Arial,sans-serif;color:#111;display:flex;flex-direction:column;}
a{color:#0078D4;text-decoration:none;}
.hdr{padding:12px 44px 10px;border-bottom:1px solid #E0E0E0;display:flex;
      align-items:center;justify-content:space-between;flex-shrink:0;}
.hdr h1{font-size:24px;font-weight:700;}
.sub{font-size:12px;color:#888;margin-top:2px;}
.tl-area{display:flex;align-items:stretch;padding:6px 44px 0;flex-shrink:0;height:196px;
          border-bottom:2px solid #E8E8E8;background:#FAFAFA;}
.tl-svg-box{flex:1;padding-left:12px;padding-top:6px;}
/* heatmap */
.hm-wrap{flex:1;min-height:0;display:flex;flex-direction:column;padding:10px 44px 10px;}
.hm-title{font-size:14px;font-weight:700;color:#888;letter-spacing:.5px;text-transform:uppercase;margin-bottom:8px;flex-shrink:0;}
.hm-grid{flex:1;min-height:0;display:grid;
          grid-template-columns:160px repeat(4,1fr);
          grid-template-rows:36px repeat(4,1fr);
          border:1px solid #E0E0E0;}
/* header cells */
.hm-corner{background:#F5F5F5;display:flex;align-items:center;justify-content:center;
            font-size:11px;font-weight:700;color:#999;text-transform:uppercase;
            border-right:1px solid #E0E0E0;border-bottom:2px solid #CCC;}
.hm-col-hdr{display:flex;align-items:center;justify-content:center;
             font-size:16px;font-weight:700;background:#F5F5F5;
             border-right:1px solid #E0E0E0;border-bottom:2px solid #CCC;}
.hm-col-hdr.apr-hdr{background:#FFF0D0;color:#C07700;}
/* row header */
.hm-row-hdr{display:flex;align-items:center;padding:0 12px;
             font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.7px;
             border-right:2px solid #CCC;border-bottom:1px solid #E0E0E0;}
/* data cells */
.hm-cell{padding:8px 12px;border-right:1px solid #E0E0E0;border-bottom:1px solid #E0E0E0;overflow:hidden;}
.hm-cell .it{font-size:12px;color:#333;padding:2px 0 2px 12px;position:relative;line-height:1.35;}
.hm-cell .it::before{content:'';position:absolute;left:0;top:7px;width:6px;height:6px;border-radius:50%;}
/* row colors */
.ship-hdr{color:#1B7A28;background:#E8F5E9;border-right:2px solid #CCC;}
.ship-cell{background:#F0FBF0;} .ship-cell.apr{background:#D8F2DA;}
.ship-cell .it::before{background:#34A853;}
.prog-hdr{color:#1565C0;background:#E3F2FD;border-right:2px solid #CCC;}
.prog-cell{background:#EEF4FE;} .prog-cell.apr{background:#DAE8FB;}
.prog-cell .it::before{background:#0078D4;}
.carry-hdr{color:#B45309;background:#FFF8E1;border-right:2px solid #CCC;}
.carry-cell{background:#FFFDE7;} .carry-cell.apr{background:#FFF0B0;}
.carry-cell .it::before{background:#F4B400;}
.block-hdr{color:#991B1B;background:#FEF2F2;border-right:2px solid #CCC;}
.block-cell{background:#FFF5F5;} .block-cell.apr{background:#FFE4E4;}
.block-cell .it::before{background:#EA4335;}
</style></head><body>
<div class="hdr">
  <div>
    <h1>Privacy Automation Release Roadmap <a href="#">⧉ ADO Backlog</a></h1>
    <div class="sub">Trusted Platform · Privacy Automation Workstream · April 2026</div>
  </div>
  
<div style="display:flex;gap:22px;align-items:center;">
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:12px;height:12px;background:#F4B400;transform:rotate(45deg);display:inline-block;flex-shrink:0;"></span>PoC Milestone
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:12px;height:12px;background:#34A853;transform:rotate(45deg);display:inline-block;flex-shrink:0;"></span>Production Release
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:8px;height:8px;border-radius:50%;background:#999;display:inline-block;flex-shrink:0;"></span>Checkpoint
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:2px;height:14px;background:#EA4335;display:inline-block;flex-shrink:0;"></span>Now (Apr 2026)
  </span>
</div>
</div>
<div class="tl-area">
  
<div style="width:230px;flex-shrink:0;display:flex;flex-direction:column;
            justify-content:space-around;padding:16px 12px 16px 0;
            border-right:1px solid #E0E0E0;">
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#0078D4;">
    M1<br/><span style="font-weight:400;color:#444;">Chatbot &amp; MS Role</span></div>
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#00897B;">
    M2<br/><span style="font-weight:400;color:#444;">PDS &amp; Data Inventory</span></div>
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#546E7A;">
    M3<br/><span style="font-weight:400;color:#444;">Auto Review DFD</span></div>
</div>
  <div class="tl-svg-box"><svg xmlns="http://www.w3.org/2000/svg" width="1560" height="185" style="overflow:visible;display:block">
<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>
<line x1="0" y1="0" x2="0" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="5" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Jan</text>
<line x1="260" y1="0" x2="260" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="265" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Feb</text>
<line x1="520" y1="0" x2="520" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="525" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Mar</text>
<line x1="780" y1="0" x2="780" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="785" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Apr</text>
<line x1="1040" y1="0" x2="1040" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="1045" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">May</text>
<line x1="1300" y1="0" x2="1300" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="1305" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Jun</text>
<line x1="823" y1="0" x2="823" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
<text x="827" y="14" fill="#EA4335" font-size="10" font-weight="700" font-family="Segoe UI,Arial">NOW</text>
<line x1="0" y1="42" x2="1560" y2="42" stroke="#0078D4" stroke-width="3"/>
<circle cx="104" cy="42" r="7" fill="white" stroke="#0078D4" stroke-width="2.5"/>
<text x="104" y="26" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Jan 12</text>
<polygon points="745,31 756,42 745,53 734,42" fill="#F4B400" filter="url(#sh)"/><text x="745" y="66" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Mar 26 PoC</text>
<polygon points="1040,31 1051,42 1040,53 1029,42" fill="#34A853" filter="url(#sh)"/><text x="1040" y="18" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Apr Prod (TBD)</text>
<line x1="0" y1="98" x2="1560" y2="98" stroke="#00897B" stroke-width="3"/>
<circle cx="0" cy="98" r="7" fill="white" stroke="#00897B" stroke-width="2.5"/>
<text x="10" y="82" fill="#666" font-size="10" font-family="Segoe UI,Arial">Dec 19</text>
<circle cx="355" cy="98" r="5" fill="white" stroke="#888" stroke-width="2.5"/>
<text x="355" y="82" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Feb 11</text>
<circle cx="546" cy="98" r="4" fill="#999"/>
<circle cx="607" cy="98" r="4" fill="#999"/>
<circle cx="650" cy="98" r="4" fill="#999"/>
<circle cx="667" cy="98" r="4" fill="#999"/>
<polygon points="693,87 704,98 693,109 682,98" fill="#F4B400" filter="url(#sh)"/><text x="693" y="74" text-anchor="middle" fill="#666" font-size="10" font-family="Seg
<!-- truncated -->
```
</details>


## Design Visual Previews

The following screenshots were rendered from the HTML design reference files. Engineers MUST match these visuals exactly.

### OriginalDesignConcept.html

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/0a1f0e75eea35cba28fd39aa2c9cbff31220ee80/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
