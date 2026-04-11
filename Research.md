# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-11 18:00 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 Blazor Server** that visualizes project milestones, progress status, and timeline data. It reads from a local `data.json` configuration file and renders a pixel-perfect view optimized for screenshots destined for PowerPoint executive decks. **Primary Recommendation:** Keep this extremely simple. Use a single Blazor Server project with zero external UI component libraries. The design is fundamentally CSS Grid + inline SVG — Blazor's built-in rendering is more than sufficient. Use `System.Text.Json` for data loading, raw CSS for styling (matching the reference HTML), and generate the timeline SVG directly in Razor components. No database, no authentication, no cloud services. The entire solution should be one `.sln` with one project, deployable via `dotnet run`. ---

### Key Findings

- The original `OriginalDesignConcept.html` is a self-contained static page using CSS Grid, Flexbox, and inline SVG — all of which Blazor Server renders natively without any JavaScript interop or third-party libraries.
- The design targets a fixed **1920×1080** viewport (screenshot-optimized), eliminating the need for responsive design breakpoints or mobile considerations.
- The heatmap grid uses a simple `grid-template-columns: 160px repeat(4, 1fr)` layout with color-coded status rows — this maps directly to Razor `@foreach` loops with CSS class binding.
- The timeline/Gantt section is pure SVG with lines, circles, diamonds, and text — Blazor can render SVG elements directly in `.razor` files without any charting library.
- A local `data.json` file is the sole data source, meaning no Entity Framework, no database migrations, no connection strings — just `System.Text.Json` deserialization at startup.
- The "screenshot for PowerPoint" use case means print/render fidelity matters more than interactivity. Avoid JavaScript-heavy charting libraries that might render asynchronously or with animations.
- .NET 8 LTS (Long Term Support until November 2026) is the correct target — stable, well-supported, and does not require upgrading to .NET 9 for this scope.
- Blazor Server's SignalR circuit is irrelevant for this use case (single local user, no real-time collaboration), but it's still the simplest hosting model for a local-only app since it avoids WASM download overhead. ---
- `dotnet new blazorserver-empty -n ReportingDashboard` — create project
- Strip default content; create `DashboardLayout.razor` with no navigation
- Create `Models/DashboardData.cs` — C# records matching `data.json` schema
- Create `data.json` with fictional project data (e.g., "Project Phoenix — Cloud Migration")
- Create `DashboardDataService.cs` — singleton that reads and deserializes `data.json`
- Create `Dashboard.razor` as the single page at route `/`
- Verify the app loads and displays raw data
- Port the CSS from `OriginalDesignConcept.html` into `wwwroot/css/dashboard.css`
- Build `Header.razor` — title, subtitle, legend items
- Build `HeatmapGrid.razor` — CSS Grid with month columns and status rows
- Build `HeatmapCell.razor` — bullet-pointed items with colored dots
- Verify visual match against the reference HTML by opening both side-by-side
- Build `Timeline.razor` — SVG element with computed width
- Implement date-to-X-position calculation: `x = (date - start).TotalDays / (end - start).TotalDays * width`
- Render month grid lines, track lines, milestone markers (circles, diamonds)
- Render the "NOW" dashed red line
- Verify against the reference SVG
- Fine-tune spacing, font sizes, colors to match reference exactly
- Hide Blazor reconnection UI via CSS: `.components-reconnect-modal { display: none; }`
- Test at 1920×1080 in Edge/Chrome
- Take screenshot, paste into PowerPoint, verify readability
- **Fictional data that looks realistic** — Use project names like "Project Phoenix," milestones like "Architecture Review," "Beta Launch," "GA Release." Executives respond better to demos with plausible data.
- **Color-coded status rows** — The four-category model (Shipped/In Progress/Carryover/Blockers) maps perfectly to executive-level status reporting. This is immediately recognizable.
- **`dotnet watch` during development** — Instant feedback loop. Edit a Razor file, save, see the change in the browser within 1-2 seconds. --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.Components` | 8.0.x (included in SDK) | Blazor Server framework | Included by default | | `System.Text.Json` | 8.0.x (included in SDK) | JSON deserialization | Included by default | This is intentional. The project scope does not warrant any third-party packages. Every capability needed — HTTP serving, HTML rendering, JSON parsing, CSS styling, SVG generation — is built into .NET 8 and the browser.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | Runtime | .NET SDK | **8.0.x LTS** | Latest 8.0 patch (8.0.404+ as of April 2026) | | Web Framework | Blazor Server | Built into .NET 8 | No separate package needed | | Project Template | `blazorserver-empty` | — | Use empty template to avoid default Bootstrap/theme bloat | | Component | Technology | Version | Notes | |-----------|-----------|---------|-------| | JSON Serialization | `System.Text.Json` | Built into .NET 8 | Zero external dependencies; use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` | | Data Loading | `IConfiguration` or direct file read | Built-in | For a single JSON file, `File.ReadAllText` + `JsonSerializer.Deserialize<T>()` is simplest. Alternatively, register as a singleton service. | | File Watching (optional) | `FileSystemWatcher` | Built-in | If you want hot-reload of `data.json` without restarting the app | | Component | Technology | Notes | |-----------|-----------|-------| | CSS Layout | Raw CSS (Grid + Flexbox) | Copy directly from the reference HTML `<style>` block. No CSS framework needed. | | SVG Timeline | Inline SVG in Razor | Render `<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` directly in `.razor` components | | Font | Segoe UI | System font on Windows; no web font loading needed | | Icons/Shapes | CSS transforms + SVG primitives | Diamond shapes via `transform: rotate(45deg)` on `<span>` or SVG `<polygon>` | | Commonly Suggested | Why Skip It | |-------------------|-------------| | MudBlazor / Radzen / Syncfusion | Massive overkill — adds hundreds of KB of CSS/JS for components you won't use | | Chart.js / ApexCharts / Plotly | The timeline is simple SVG geometry, not statistical charts | | Entity Framework Core | No database; reading a flat JSON file | | AutoMapper | Data model is simple enough for manual mapping | | MediatR / CQRS | No commands, no events — just read a file and render | | Serilog / structured logging | Single-user local app; `Console.WriteLine` suffices for debugging | | Bootstrap / Tailwind | The reference design has its own CSS; adding a framework creates conflicts | | SignalR customization | Default Blazor Server SignalR config is fine | | Authentication (any) | Explicitly out of scope per requirements | ---
```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css        ← Ported from OriginalDesignConcept.html <style>
    │   └── data.json                ← Project data (or place in project root)
    ├── Models/
    │   ├── DashboardData.cs         ← Root model matching data.json schema
    │   ├── Milestone.cs
    │   ├── StatusCategory.cs        ← Shipped / InProgress / Carryover / Blockers
    │   └── StatusItem.cs
    ├── Services/
    │   └── DashboardDataService.cs  ← Reads & deserializes data.json, registered as Singleton
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor      ← Single page, route "/"
    │   ├── Layout/
    │   │   └── DashboardLayout.razor ← Minimal layout (no nav, no sidebar)
    │   ├── Header.razor             ← Title, subtitle, legend
    │   ├── Timeline.razor           ← SVG milestone visualization
    │   ├── HeatmapGrid.razor        ← CSS Grid status matrix
    │   ├── HeatmapRow.razor         ← Single status row (Shipped/InProgress/etc.)
    │   └── HeatmapCell.razor        ← Single month×status cell with item bullets
    └── App.razor
```
```
data.json → DashboardDataService (Singleton) → Inject into Dashboard.razor → Pass to child components via [Parameter]
``` This is a **read-only, single-direction data flow**. No state management library needed. No event callbacks. No two-way binding.
- **One component per visual section** — Header, Timeline, HeatmapGrid map directly to the design's `<div class="hdr">`, `<div class="tl-area">`, `<div class="hm-wrap">`.
- **CSS classes match the reference** — Keep `.hdr`, `.tl-area`, `.hm-wrap`, `.hm-grid`, `.ship-cell`, `.prog-cell`, etc. Identical class names make it trivial to verify parity with the HTML reference.
- **SVG is data-driven** — The Timeline component should compute X positions from dates: `xPos = (date - startDate) / (endDate - startDate) * svgWidth`. Milestone type (diamond vs. circle) determined by a `Type` enum in the model.
- **No JavaScript interop** — Everything renders server-side. The SVG, CSS Grid, and all layout are pure HTML/CSS delivered via Blazor's SignalR connection.
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "nowDate": "2026-04-11",
    "tracks": [
      {
        "name": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "label": "Jan 12", "type": "Checkpoint" },
          { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "PoC" },
          { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "Production" }
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
``` ---

### Considerations & Risks

- | Concern | Approach | |---------|----------| | Authentication | **None.** Explicitly out of scope. No `[Authorize]`, no Identity, no cookies. | | Authorization | **None.** Single anonymous user. | | Input validation | Minimal — `data.json` is author-controlled. Validate JSON deserialization with try/catch. | | XSS | Blazor auto-encodes rendered strings by default. Do not use `MarkupString` unless needed for SVG. | | HTTPS | Not required for local-only use. Kestrel HTTP on `localhost:5000` is fine. | | CORS | Not applicable — no cross-origin requests. | | Aspect | Recommendation | |--------|---------------| | Hosting | **Local Kestrel** via `dotnet run` — no IIS, no Docker, no reverse proxy | | Port | Default `http://localhost:5000` or configure in `launchSettings.json` | | Deployment | `dotnet publish -c Release -o ./publish` → run `ReportingDashboard.exe` | | Self-contained option | `dotnet publish -r win-x64 --self-contained` for machines without .NET SDK | | CI/CD | Not needed for this scope. Manual `dotnet build && dotnet run`. | | Monitoring | Not needed. Console output for errors. | | Cost | **$0.** Runs on the developer's local machine. | Since the primary output is screenshots for PowerPoint:
- Set the browser window to exactly **1920×1080** (the design's target resolution).
- Use Chrome DevTools → Device Toolbar → set custom resolution 1920×1080, then full-page screenshot.
- Alternatively, use `Ctrl+Shift+I` → `Ctrl+Shift+P` → "Capture full size screenshot" in Edge/Chrome.
- Consider adding a `?print=true` query parameter that hides the Blazor reconnection UI overlay. --- | Risk | Likelihood | Impact | Mitigation | |------|-----------|--------|------------| | Blazor Server reconnection overlay appearing in screenshots | Medium | Low | Add CSS to hide `.components-reconnect-modal` or use `dotnet watch` for stability | | SVG text positioning differences across browsers | Low | Low | Test in both Edge and Chrome; use `text-anchor` and explicit `x/y` coordinates | | `data.json` schema changes breaking the app | Low | Medium | Validate with `JsonSerializer.Deserialize<T>()` and provide clear error messages | | Segoe UI font not available on non-Windows machines | Low | Low | The requirement is Windows-only (screenshots from dev machine) | | Decision | Benefit | Cost | |----------|---------|------| | No charting library | Zero dependencies, pixel-perfect control, fast load | Must hand-code SVG coordinate math for timeline | | No CSS framework | Exact design match, no class conflicts | Must write all CSS manually (but we're copying from the reference) | | No database | Zero setup, edit `data.json` in any text editor | No query capabilities, no history tracking | | Blazor Server over Blazor WASM | Simpler deployment, no WASM download delay | Requires `dotnet run` process to be active | | Single project (no class library split) | Simplest possible solution structure | If scope grows, would need refactoring |
- **SignalR connection for a single user** — No scalability concern. One circuit, one connection, effectively zero overhead.
- **Server-side rendering performance** — The page has ~50-100 DOM elements total. Renders in <10ms.
- **Hot reload during development** — `dotnet watch` works out of the box with Blazor Server in .NET 8. --- | # | Question | Who Decides | Default If No Answer | |---|----------|-------------|---------------------| | 1 | Should `data.json` live in `wwwroot/` (downloadable) or project root (server-only)? | Developer | Project root (more secure, not exposed via HTTP) | | 2 | How many timeline tracks are typical? 2-3 or 10+? | Stakeholder | Design for 3-5 tracks; vertically scroll if more | | 3 | Should the "NOW" line auto-calculate from system date or be set in `data.json`? | Developer | Auto-calculate from `DateTime.Now`, with optional override in JSON | | 4 | Is the April "highlight" column always the current month, or manually specified? | Stakeholder | Auto-detect current month, highlight with `.apr` CSS class equivalent | | 5 | Will this ever need to show multiple projects on one page? | Stakeholder | No — one project per page, switch via different `data.json` files | | 6 | Should the page auto-refresh when `data.json` changes? | Developer | Nice-to-have; implement with `FileSystemWatcher` + `InvokeAsync(StateHasChanged)` | ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 Blazor Server** that visualizes project milestones, progress status, and timeline data. It reads from a local `data.json` configuration file and renders a pixel-perfect view optimized for screenshots destined for PowerPoint executive decks.

**Primary Recommendation:** Keep this extremely simple. Use a single Blazor Server project with zero external UI component libraries. The design is fundamentally CSS Grid + inline SVG — Blazor's built-in rendering is more than sufficient. Use `System.Text.Json` for data loading, raw CSS for styling (matching the reference HTML), and generate the timeline SVG directly in Razor components. No database, no authentication, no cloud services. The entire solution should be one `.sln` with one project, deployable via `dotnet run`.

---

## 2. Key Findings

- The original `OriginalDesignConcept.html` is a self-contained static page using CSS Grid, Flexbox, and inline SVG — all of which Blazor Server renders natively without any JavaScript interop or third-party libraries.
- The design targets a fixed **1920×1080** viewport (screenshot-optimized), eliminating the need for responsive design breakpoints or mobile considerations.
- The heatmap grid uses a simple `grid-template-columns: 160px repeat(4, 1fr)` layout with color-coded status rows — this maps directly to Razor `@foreach` loops with CSS class binding.
- The timeline/Gantt section is pure SVG with lines, circles, diamonds, and text — Blazor can render SVG elements directly in `.razor` files without any charting library.
- A local `data.json` file is the sole data source, meaning no Entity Framework, no database migrations, no connection strings — just `System.Text.Json` deserialization at startup.
- The "screenshot for PowerPoint" use case means print/render fidelity matters more than interactivity. Avoid JavaScript-heavy charting libraries that might render asynchronously or with animations.
- .NET 8 LTS (Long Term Support until November 2026) is the correct target — stable, well-supported, and does not require upgrading to .NET 9 for this scope.
- Blazor Server's SignalR circuit is irrelevant for this use case (single local user, no real-time collaboration), but it's still the simplest hosting model for a local-only app since it avoids WASM download overhead.

---

## 3. Recommended Technology Stack

### Runtime & Framework

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| Runtime | .NET SDK | **8.0.x LTS** | Latest 8.0 patch (8.0.404+ as of April 2026) |
| Web Framework | Blazor Server | Built into .NET 8 | No separate package needed |
| Project Template | `blazorserver-empty` | — | Use empty template to avoid default Bootstrap/theme bloat |

### Data Layer

| Component | Technology | Version | Notes |
|-----------|-----------|---------|-------|
| JSON Serialization | `System.Text.Json` | Built into .NET 8 | Zero external dependencies; use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` |
| Data Loading | `IConfiguration` or direct file read | Built-in | For a single JSON file, `File.ReadAllText` + `JsonSerializer.Deserialize<T>()` is simplest. Alternatively, register as a singleton service. |
| File Watching (optional) | `FileSystemWatcher` | Built-in | If you want hot-reload of `data.json` without restarting the app |

### Frontend / UI

| Component | Technology | Notes |
|-----------|-----------|-------|
| CSS Layout | Raw CSS (Grid + Flexbox) | Copy directly from the reference HTML `<style>` block. No CSS framework needed. |
| SVG Timeline | Inline SVG in Razor | Render `<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` directly in `.razor` components |
| Font | Segoe UI | System font on Windows; no web font loading needed |
| Icons/Shapes | CSS transforms + SVG primitives | Diamond shapes via `transform: rotate(45deg)` on `<span>` or SVG `<polygon>` |

### What You Do NOT Need

| Commonly Suggested | Why Skip It |
|-------------------|-------------|
| MudBlazor / Radzen / Syncfusion | Massive overkill — adds hundreds of KB of CSS/JS for components you won't use |
| Chart.js / ApexCharts / Plotly | The timeline is simple SVG geometry, not statistical charts |
| Entity Framework Core | No database; reading a flat JSON file |
| AutoMapper | Data model is simple enough for manual mapping |
| MediatR / CQRS | No commands, no events — just read a file and render |
| Serilog / structured logging | Single-user local app; `Console.WriteLine` suffices for debugging |
| Bootstrap / Tailwind | The reference design has its own CSS; adding a framework creates conflicts |
| SignalR customization | Default Blazor Server SignalR config is fine |
| Authentication (any) | Explicitly out of scope per requirements |

---

## 4. Architecture Recommendations

### Solution Structure

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css        ← Ported from OriginalDesignConcept.html <style>
    │   └── data.json                ← Project data (or place in project root)
    ├── Models/
    │   ├── DashboardData.cs         ← Root model matching data.json schema
    │   ├── Milestone.cs
    │   ├── StatusCategory.cs        ← Shipped / InProgress / Carryover / Blockers
    │   └── StatusItem.cs
    ├── Services/
    │   └── DashboardDataService.cs  ← Reads & deserializes data.json, registered as Singleton
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor      ← Single page, route "/"
    │   ├── Layout/
    │   │   └── DashboardLayout.razor ← Minimal layout (no nav, no sidebar)
    │   ├── Header.razor             ← Title, subtitle, legend
    │   ├── Timeline.razor           ← SVG milestone visualization
    │   ├── HeatmapGrid.razor        ← CSS Grid status matrix
    │   ├── HeatmapRow.razor         ← Single status row (Shipped/InProgress/etc.)
    │   └── HeatmapCell.razor        ← Single month×status cell with item bullets
    └── App.razor
```

### Data Flow Pattern

```
data.json → DashboardDataService (Singleton) → Inject into Dashboard.razor → Pass to child components via [Parameter]
```

This is a **read-only, single-direction data flow**. No state management library needed. No event callbacks. No two-way binding.

### Component Design Principles

1. **One component per visual section** — Header, Timeline, HeatmapGrid map directly to the design's `<div class="hdr">`, `<div class="tl-area">`, `<div class="hm-wrap">`.
2. **CSS classes match the reference** — Keep `.hdr`, `.tl-area`, `.hm-wrap`, `.hm-grid`, `.ship-cell`, `.prog-cell`, etc. Identical class names make it trivial to verify parity with the HTML reference.
3. **SVG is data-driven** — The Timeline component should compute X positions from dates: `xPos = (date - startDate) / (endDate - startDate) * svgWidth`. Milestone type (diamond vs. circle) determined by a `Type` enum in the model.
4. **No JavaScript interop** — Everything renders server-side. The SVG, CSS Grid, and all layout are pure HTML/CSS delivered via Blazor's SignalR connection.

### `data.json` Schema Design

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "nowDate": "2026-04-11",
    "tracks": [
      {
        "name": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "label": "Jan 12", "type": "Checkpoint" },
          { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "PoC" },
          { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "Production" }
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

---

## 5. Security & Infrastructure

### Security

| Concern | Approach |
|---------|----------|
| Authentication | **None.** Explicitly out of scope. No `[Authorize]`, no Identity, no cookies. |
| Authorization | **None.** Single anonymous user. |
| Input validation | Minimal — `data.json` is author-controlled. Validate JSON deserialization with try/catch. |
| XSS | Blazor auto-encodes rendered strings by default. Do not use `MarkupString` unless needed for SVG. |
| HTTPS | Not required for local-only use. Kestrel HTTP on `localhost:5000` is fine. |
| CORS | Not applicable — no cross-origin requests. |

### Infrastructure & Deployment

| Aspect | Recommendation |
|--------|---------------|
| Hosting | **Local Kestrel** via `dotnet run` — no IIS, no Docker, no reverse proxy |
| Port | Default `http://localhost:5000` or configure in `launchSettings.json` |
| Deployment | `dotnet publish -c Release -o ./publish` → run `ReportingDashboard.exe` |
| Self-contained option | `dotnet publish -r win-x64 --self-contained` for machines without .NET SDK |
| CI/CD | Not needed for this scope. Manual `dotnet build && dotnet run`. |
| Monitoring | Not needed. Console output for errors. |
| Cost | **$0.** Runs on the developer's local machine. |

### Screenshot Workflow Optimization

Since the primary output is screenshots for PowerPoint:
- Set the browser window to exactly **1920×1080** (the design's target resolution).
- Use Chrome DevTools → Device Toolbar → set custom resolution 1920×1080, then full-page screenshot.
- Alternatively, use `Ctrl+Shift+I` → `Ctrl+Shift+P` → "Capture full size screenshot" in Edge/Chrome.
- Consider adding a `?print=true` query parameter that hides the Blazor reconnection UI overlay.

---

## 6. Risks & Trade-offs

### Low Risks (Manageable)

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Blazor Server reconnection overlay appearing in screenshots | Medium | Low | Add CSS to hide `.components-reconnect-modal` or use `dotnet watch` for stability |
| SVG text positioning differences across browsers | Low | Low | Test in both Edge and Chrome; use `text-anchor` and explicit `x/y` coordinates |
| `data.json` schema changes breaking the app | Low | Medium | Validate with `JsonSerializer.Deserialize<T>()` and provide clear error messages |
| Segoe UI font not available on non-Windows machines | Low | Low | The requirement is Windows-only (screenshots from dev machine) |

### Trade-offs Made

| Decision | Benefit | Cost |
|----------|---------|------|
| No charting library | Zero dependencies, pixel-perfect control, fast load | Must hand-code SVG coordinate math for timeline |
| No CSS framework | Exact design match, no class conflicts | Must write all CSS manually (but we're copying from the reference) |
| No database | Zero setup, edit `data.json` in any text editor | No query capabilities, no history tracking |
| Blazor Server over Blazor WASM | Simpler deployment, no WASM download delay | Requires `dotnet run` process to be active |
| Single project (no class library split) | Simplest possible solution structure | If scope grows, would need refactoring |

### Non-Risks (Things That Sound Concerning But Aren't)

- **SignalR connection for a single user** — No scalability concern. One circuit, one connection, effectively zero overhead.
- **Server-side rendering performance** — The page has ~50-100 DOM elements total. Renders in <10ms.
- **Hot reload during development** — `dotnet watch` works out of the box with Blazor Server in .NET 8.

---

## 7. Open Questions

| # | Question | Who Decides | Default If No Answer |
|---|----------|-------------|---------------------|
| 1 | Should `data.json` live in `wwwroot/` (downloadable) or project root (server-only)? | Developer | Project root (more secure, not exposed via HTTP) |
| 2 | How many timeline tracks are typical? 2-3 or 10+? | Stakeholder | Design for 3-5 tracks; vertically scroll if more |
| 3 | Should the "NOW" line auto-calculate from system date or be set in `data.json`? | Developer | Auto-calculate from `DateTime.Now`, with optional override in JSON |
| 4 | Is the April "highlight" column always the current month, or manually specified? | Stakeholder | Auto-detect current month, highlight with `.apr` CSS class equivalent |
| 5 | Will this ever need to show multiple projects on one page? | Stakeholder | No — one project per page, switch via different `data.json` files |
| 6 | Should the page auto-refresh when `data.json` changes? | Developer | Nice-to-have; implement with `FileSystemWatcher` + `InvokeAsync(StateHasChanged)` |

---

## 8. Implementation Recommendations

### Phase 1: Skeleton (2–3 hours)

1. `dotnet new blazorserver-empty -n ReportingDashboard` — create project
2. Strip default content; create `DashboardLayout.razor` with no navigation
3. Create `Models/DashboardData.cs` — C# records matching `data.json` schema
4. Create `data.json` with fictional project data (e.g., "Project Phoenix — Cloud Migration")
5. Create `DashboardDataService.cs` — singleton that reads and deserializes `data.json`
6. Create `Dashboard.razor` as the single page at route `/`
7. Verify the app loads and displays raw data

### Phase 2: CSS & Layout (2–3 hours)

1. Port the CSS from `OriginalDesignConcept.html` into `wwwroot/css/dashboard.css`
2. Build `Header.razor` — title, subtitle, legend items
3. Build `HeatmapGrid.razor` — CSS Grid with month columns and status rows
4. Build `HeatmapCell.razor` — bullet-pointed items with colored dots
5. Verify visual match against the reference HTML by opening both side-by-side

### Phase 3: Timeline SVG (2–3 hours)

1. Build `Timeline.razor` — SVG element with computed width
2. Implement date-to-X-position calculation: `x = (date - start).TotalDays / (end - start).TotalDays * width`
3. Render month grid lines, track lines, milestone markers (circles, diamonds)
4. Render the "NOW" dashed red line
5. Verify against the reference SVG

### Phase 4: Polish & Screenshot (1 hour)

1. Fine-tune spacing, font sizes, colors to match reference exactly
2. Hide Blazor reconnection UI via CSS: `.components-reconnect-modal { display: none; }`
3. Test at 1920×1080 in Edge/Chrome
4. Take screenshot, paste into PowerPoint, verify readability

### Quick Wins

- **Fictional data that looks realistic** — Use project names like "Project Phoenix," milestones like "Architecture Review," "Beta Launch," "GA Release." Executives respond better to demos with plausible data.
- **Color-coded status rows** — The four-category model (Shipped/In Progress/Carryover/Blockers) maps perfectly to executive-level status reporting. This is immediately recognizable.
- **`dotnet watch` during development** — Instant feedback loop. Edit a Razor file, save, see the change in the browser within 1-2 seconds.

### Total Estimated Effort: 8–12 hours for a polished, screenshot-ready dashboard.

---

## Appendix: Key NuGet Packages

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.Components` | 8.0.x (included in SDK) | Blazor Server framework | Included by default |
| `System.Text.Json` | 8.0.x (included in SDK) | JSON deserialization | Included by default |

**Total external NuGet dependencies: 0.**

This is intentional. The project scope does not warrant any third-party packages. Every capability needed — HTTP serving, HTML rendering, JSON parsing, CSS styling, SVG generation — is built into .NET 8 and the browser.

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
