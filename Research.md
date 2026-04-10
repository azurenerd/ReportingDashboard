# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-10 23:41 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipped items, in-progress work, carryover, and blockers. The dashboard reads from a local `data.json` file and renders a screenshot-friendly view optimized for PowerPoint decks. Given the explicit simplicity requirements—no auth, no cloud, no enterprise security—the architecture is intentionally minimal: a single Blazor Server project with inline SVG rendering, CSS Grid/Flexbox layout, and `System.Text.Json` for data binding. No charting library is needed; the original HTML design uses hand-crafted SVG for the timeline, and Blazor components can replicate this directly. The entire solution fits in one `.sln` with one project and can run with `dotnet run`.

### Key Findings

- The original `OriginalDesignConcept.html` uses **pure CSS Grid + Flexbox + inline SVG**—no JavaScript charting libraries. This is directly reproducible in Blazor components without any third-party UI framework.
- The design targets a **fixed 1920×1080 viewport** for screenshot capture, meaning responsive design is unnecessary. A fixed-width layout simplifies implementation significantly.
- The heatmap grid uses a **`grid-template-columns: 160px repeat(4, 1fr)`** pattern with color-coded rows (green=shipped, blue=in-progress, amber=carryover, red=blockers). Each cell contains bullet-pointed work items.
- The timeline is a **horizontal SVG with month columns, milestone diamonds, checkpoint circles, and a "NOW" marker line**. This is best rendered as a Blazor component that generates SVG markup from data.
- **No database is needed.** A single `data.json` file is the data source, read at startup and optionally watched for changes via `FileSystemWatcher`.
- Blazor Server's **SignalR connection** is irrelevant for this use case (single local user), but it's the simplest Blazor hosting model for .NET 8 and requires zero additional configuration.
- The **Segoe UI** font specified in the design is available natively on Windows, which is the target platform.
- No authentication, authorization, or HTTPS configuration is needed per requirements.
- **Scaffold project** — `dotnet new blazor -n ReportingDashboard --interactivity Server`
- **Define models** — Create C# classes matching the `data.json` schema above
- **Create `data.json`** — Populate with fictional project data matching the original design's structure
- **Port CSS** — Extract styles from the HTML template into `dashboard.css`
- **Build components** — Header → Timeline (SVG) → Heatmap (CSS Grid) → HeatmapRow → HeatmapCell
- **Wire data service** — Read `data.json` on startup, inject into page
- **Strip default Blazor chrome** — Remove nav sidebar, default layout, counter/weather pages
- **Improve upon original design** — Better spacing, subtle shadows, refined typography weights
- **Add legend component** — Dynamic legend based on milestone types present in data
- **Add "NOW" line** — Calculate position from `DateTime.Now` relative to timeline range
- **Add hover tooltips** — Optional: show full item details on hover (useful before screenshot)
- **Validate JSON on load** — Log clear error messages if `data.json` is malformed
- **Auto-screenshot endpoint** — Use `Microsoft.Playwright` to render the page as PNG on demand
- **Multiple project support** — Route parameter `/project/{name}` loads different JSON files
- **Historical snapshots** — Save dated copies of `data.json` for month-over-month comparison
- **SQLite backend** — Migrate from JSON to SQLite if data complexity grows; use `Microsoft.Data.Sqlite` (no EF needed)
- **Reuse the original CSS verbatim.** The class names in `OriginalDesignConcept.html` are well-structured. Copy them directly into `dashboard.css` to guarantee visual fidelity.
- **Use `@inject` for the data service.** Blazor's built-in DI makes this trivial—register `DashboardDataService` as singleton in `Program.cs`.
- **Use `RenderFragment` for heatmap cells.** This keeps the grid component generic while allowing per-row color theming via CSS classes.
- ```xml
- <!-- ReportingDashboard.csproj — MINIMAL dependencies -->
- <Project Sdk="Microsoft.NET.Sdk.Web">
- <PropertyGroup>
- <TargetFramework>net8.0</TargetFramework>
- </PropertyGroup>
- <!-- No additional NuGet packages needed for MVP -->
- <!-- Optional for Phase 3: -->
- <!-- <PackageReference Include="Microsoft.Playwright" Version="1.40+" /> -->
- <!-- <PackageReference Include="Microsoft.Data.Sqlite" Version="8.0+" /> -->
- <!-- <PackageReference Include="QuestPDF" Version="2024.3+" /> -->
- </Project>
- ```

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes |
- |-------|-----------|---------|-------|
- | **UI Framework** | Blazor Server (.NET 8) | .NET 8.0 LTS | Built-in, no additional packages needed |
- | **CSS Layout** | CSS Grid + Flexbox | Native CSS | Matches original design exactly |
- | **Timeline Visualization** | Inline SVG via Blazor `MarkupString` or `RenderTreeBuilder` | N/A | No charting library needed; hand-crafted SVG as in original HTML |
- | **Icons/Shapes** | Inline SVG diamonds, circles | N/A | Milestone diamonds, checkpoint circles rendered via `<polygon>` and `<circle>` |
- | **Font** | Segoe UI (system font) | N/A | Native on Windows; no web font loading needed |
- | Layer | Technology | Version | Notes |
- |-------|-----------|---------|-------|
- | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Native, fast, zero dependencies |
- | **File Watching** | `System.IO.FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload when `data.json` changes |
- | **Configuration** | `IConfiguration` or direct file read | Built into .NET 8 | Can bind `data.json` via `ConfigurationBuilder` or deserialize directly |
- | **Hosting** | Kestrel (built-in) | .NET 8 | `dotnet run` serves the app on localhost |
- | Layer | Technology | Version | Notes |
- |-------|-----------|---------|-------|
- | **Unit Testing** | xUnit | 2.7+ | .NET ecosystem standard |
- | **Blazor Component Testing** | bUnit | 1.25+ | If component-level testing is desired |
- | **Assertions** | FluentAssertions | 6.12+ | Optional, cleaner assertion syntax |
- | Library | Why Not |
- |---------|---------|
- | **MudBlazor / Radzen / Syncfusion** | Overkill for a single-page static-data dashboard. Adds bundle size, complexity, and opinionated styling that conflicts with the pixel-precise design. |
- | **Chart.js / Plotly / ApexCharts** | The timeline is custom SVG, not a standard chart type. These libraries would fight the design rather than help. |
- | **Entity Framework Core** | No database. JSON file is the data source. |
- | **Blazor WebAssembly** | Server model is simpler for local-only use; no need for client-side compilation. |
- ```
- ReportingDashboard.sln
- └── ReportingDashboard/
- ├── ReportingDashboard.csproj
- ├── Program.cs
- ├── Models/
- │   ├── DashboardData.cs          # Root model matching data.json
- │   ├── Milestone.cs              # Timeline milestone (name, date, type)
- │   ├── HeatmapCategory.cs        # Shipped/InProgress/Carryover/Blockers
- │   └── WorkItem.cs               # Individual item in a heatmap cell
- ├── Services/
- │   └── DashboardDataService.cs   # Reads & deserializes data.json, optional file watch
- ├── Components/
- │   ├── Pages/
- │   │   └── Dashboard.razor       # Main page (single page app)
- │   ├── Layout/
- │   │   └── DashboardLayout.razor # Minimal layout (no nav, no sidebar)
- │   ├── Header.razor              # Title, subtitle, legend
- │   ├── Timeline.razor            # SVG timeline with milestones
- │   ├── Heatmap.razor             # CSS Grid heatmap container
- │   ├── HeatmapRow.razor          # Single status row (shipped/progress/etc.)
- │   └── HeatmapCell.razor         # Single month×status cell with work items
- ├── wwwroot/
- │   ├── css/
- │   │   └── dashboard.css         # All styles (ported from original HTML <style>)
- │   └── data.json                 # Data file (served as static file or read from disk)
- └── Properties/
- └── launchSettings.json       # localhost:5000 or similar
- ```
- ```
- data.json → DashboardDataService (reads + deserializes at startup)
- → Injected into Dashboard.razor as scoped/singleton service
- → Dashboard.razor passes data to child components via [Parameter]
- → Components render HTML/SVG directly from model properties
- ```
- **No SPA routing needed.** Single page, single route (`/`). Remove default Blazor nav/sidebar scaffolding.
- **Static rendering preferred.** Since data doesn't change during a session, use `StreamRendering` or simply load data in `OnInitializedAsync`. No need for `StateHasChanged` loops or real-time updates.
- **CSS ported directly from original HTML.** The `<style>` block in `OriginalDesignConcept.html` should be extracted into `dashboard.css` with minimal modifications. Class names can remain the same for fidelity.
- **SVG generated in Blazor.** The timeline SVG is rendered via Blazor markup (`<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` elements) with data-driven coordinates. Month positions are calculated as percentage offsets or fixed pixel positions based on the 1920px width.
- **Fixed viewport.** Add `<meta name="viewport" content="width=1920">` and constrain `body` to 1920×1080. This ensures screenshots are pixel-consistent.
- ```json
- {
- "title": "Privacy Automation Release Roadmap",
- "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
- "backlogLink": "https://dev.azure.com/...",
- "months": ["Jan", "Feb", "Mar", "Apr"],
- "currentMonth": "Apr",
- "timeline": {
- "startMonth": "Jan",
- "endMonth": "Jun",
- "nowDate": "2026-04-10",
- "tracks": [
- {
- "name": "M1",
- "label": "Chatbot & MS Role",
- "color": "#0078D4",
- "milestones": [
- { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
- { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
- { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
- ]
- }
- ]
- },
- "heatmap": {
- "shipped": {
- "jan": ["Item A", "Item B"],
- "feb": ["Item C"],
- "mar": ["Item D", "Item E"],
- "apr": ["Item F"]
- },
- "inProgress": { ... },
- "carryover": { ... },
- "blockers": { ... }
- }
- }
- ```

### Considerations & Risks

- **No authentication.** Per requirements, this is a local-only tool. The app binds to `localhost` only.
- **No HTTPS required.** Running on `http://localhost:5000` is sufficient for local screenshot capture.
- **No CORS.** Single origin, no API consumers.
- **No input sanitization concerns.** Data comes from a trusted local JSON file, not user input.
- **Recommendation:** In `Program.cs`, explicitly bind to localhost only:
- ```csharp
- builder.WebHost.UseUrls("http://localhost:5000");
- ```
- **Hosting:** Local Kestrel server via `dotnet run`. No IIS, no Docker, no reverse proxy needed.
- **Deployment:** `dotnet publish -c Release` produces a self-contained folder. Can be copied to any Windows machine with .NET 8 runtime.
- **Optional: Self-contained publish** with `dotnet publish -c Release --self-contained -r win-x64` for zero-dependency deployment.
- **Infrastructure cost:** $0. Runs on developer's local machine.
- Since the primary output is PowerPoint screenshots:
- Run `dotnet run`
- Open `http://localhost:5000` in Edge/Chrome
- Use browser's built-in screenshot or `Ctrl+Shift+S` (Edge) for full-page capture at 1920×1080
- Paste into PowerPoint
- | Risk | Likelihood | Impact | Mitigation |
- |------|-----------|--------|------------|
- | **SVG timeline complexity** | Medium | Low | The original HTML already has working SVG. Port coordinates directly; calculate positions with simple linear interpolation from date ranges. |
- | **Blazor Server overhead for static content** | Low | Low | For a single-user local app, SignalR overhead is negligible. The alternative (Blazor Static SSR in .NET 8) could eliminate it, but adds complexity for no real benefit. |
- | **JSON schema drift** | Low | Medium | Define strongly-typed C# models with `System.Text.Json` attributes. Add a simple validation step on load that logs warnings for missing fields. |
- | **Browser rendering differences** | Low | Low | Fixed 1920×1080 viewport with Segoe UI eliminates most cross-browser issues. Recommend Edge or Chrome for screenshots. |
- **No charting library → manual SVG.** This means the timeline component requires manual coordinate math, but gives pixel-perfect control matching the original design. A charting library would fight the custom layout.
- **No database → JSON file.** Limits future extensibility (e.g., historical tracking, multi-project views) but keeps the system trivially simple. If needed later, SQLite via `Microsoft.Data.Sqlite` is a one-file addition.
- **Blazor Server vs. static HTML.** Blazor adds a runtime dependency but provides component reusability, strong typing, and the ability to add interactivity later (e.g., click a milestone for details). For a screenshot-only tool, static HTML would suffice, but Blazor enables future enhancements without a rewrite.
- **Fixed viewport vs. responsive.** Optimized for screenshot fidelity at the cost of mobile/tablet viewing. This is correct given the stated use case.
- **How many months should the heatmap display?** The original shows 4 months (Jan–Apr). Should this be configurable in `data.json` (e.g., trailing 4 months, full fiscal year)?
- **How many timeline tracks?** The original shows 3 milestones (M1, M2, M3). Is there a maximum? This affects SVG height calculation.
- **Update frequency.** How often will `data.json` be updated? If weekly/monthly, manual editing is fine. If daily, consider a simpler editor UI or structured template.
- **Multiple projects.** Will there ever be multiple dashboards (one per project)? If so, the data service should support loading different JSON files (e.g., `?project=alpha`).
- **Color scheme customization.** Should the color palette (green/blue/amber/red) be configurable in `data.json`, or hardcoded as in the original design?
- **Print/export.** Beyond screenshots, is PDF export needed? If so, consider `QuestPDF` (MIT license, .NET native) for server-side PDF generation.

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipped items, in-progress work, carryover, and blockers. The dashboard reads from a local `data.json` file and renders a screenshot-friendly view optimized for PowerPoint decks. Given the explicit simplicity requirements—no auth, no cloud, no enterprise security—the architecture is intentionally minimal: a single Blazor Server project with inline SVG rendering, CSS Grid/Flexbox layout, and `System.Text.Json` for data binding. No charting library is needed; the original HTML design uses hand-crafted SVG for the timeline, and Blazor components can replicate this directly. The entire solution fits in one `.sln` with one project and can run with `dotnet run`.

## 2. Key Findings

- The original `OriginalDesignConcept.html` uses **pure CSS Grid + Flexbox + inline SVG**—no JavaScript charting libraries. This is directly reproducible in Blazor components without any third-party UI framework.
- The design targets a **fixed 1920×1080 viewport** for screenshot capture, meaning responsive design is unnecessary. A fixed-width layout simplifies implementation significantly.
- The heatmap grid uses a **`grid-template-columns: 160px repeat(4, 1fr)`** pattern with color-coded rows (green=shipped, blue=in-progress, amber=carryover, red=blockers). Each cell contains bullet-pointed work items.
- The timeline is a **horizontal SVG with month columns, milestone diamonds, checkpoint circles, and a "NOW" marker line**. This is best rendered as a Blazor component that generates SVG markup from data.
- **No database is needed.** A single `data.json` file is the data source, read at startup and optionally watched for changes via `FileSystemWatcher`.
- Blazor Server's **SignalR connection** is irrelevant for this use case (single local user), but it's the simplest Blazor hosting model for .NET 8 and requires zero additional configuration.
- The **Segoe UI** font specified in the design is available natively on Windows, which is the target platform.
- No authentication, authorization, or HTTPS configuration is needed per requirements.

## 3. Recommended Technology Stack

### Frontend (Blazor Server)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (.NET 8) | .NET 8.0 LTS | Built-in, no additional packages needed |
| **CSS Layout** | CSS Grid + Flexbox | Native CSS | Matches original design exactly |
| **Timeline Visualization** | Inline SVG via Blazor `MarkupString` or `RenderTreeBuilder` | N/A | No charting library needed; hand-crafted SVG as in original HTML |
| **Icons/Shapes** | Inline SVG diamonds, circles | N/A | Milestone diamonds, checkpoint circles rendered via `<polygon>` and `<circle>` |
| **Font** | Segoe UI (system font) | N/A | Native on Windows; no web font loading needed |

### Backend / Data Layer

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Native, fast, zero dependencies |
| **File Watching** | `System.IO.FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload when `data.json` changes |
| **Configuration** | `IConfiguration` or direct file read | Built into .NET 8 | Can bind `data.json` via `ConfigurationBuilder` or deserialize directly |
| **Hosting** | Kestrel (built-in) | .NET 8 | `dotnet run` serves the app on localhost |

### Testing (Optional, Lightweight)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Testing** | xUnit | 2.7+ | .NET ecosystem standard |
| **Blazor Component Testing** | bUnit | 1.25+ | If component-level testing is desired |
| **Assertions** | FluentAssertions | 6.12+ | Optional, cleaner assertion syntax |

### Libraries NOT Recommended

| Library | Why Not |
|---------|---------|
| **MudBlazor / Radzen / Syncfusion** | Overkill for a single-page static-data dashboard. Adds bundle size, complexity, and opinionated styling that conflicts with the pixel-precise design. |
| **Chart.js / Plotly / ApexCharts** | The timeline is custom SVG, not a standard chart type. These libraries would fight the design rather than help. |
| **Entity Framework Core** | No database. JSON file is the data source. |
| **Blazor WebAssembly** | Server model is simpler for local-only use; no need for client-side compilation. |

## 4. Architecture Recommendations

### Project Structure

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── Models/
    │   ├── DashboardData.cs          # Root model matching data.json
    │   ├── Milestone.cs              # Timeline milestone (name, date, type)
    │   ├── HeatmapCategory.cs        # Shipped/InProgress/Carryover/Blockers
    │   └── WorkItem.cs               # Individual item in a heatmap cell
    ├── Services/
    │   └── DashboardDataService.cs   # Reads & deserializes data.json, optional file watch
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor       # Main page (single page app)
    │   ├── Layout/
    │   │   └── DashboardLayout.razor # Minimal layout (no nav, no sidebar)
    │   ├── Header.razor              # Title, subtitle, legend
    │   ├── Timeline.razor            # SVG timeline with milestones
    │   ├── Heatmap.razor             # CSS Grid heatmap container
    │   ├── HeatmapRow.razor          # Single status row (shipped/progress/etc.)
    │   └── HeatmapCell.razor         # Single month×status cell with work items
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css         # All styles (ported from original HTML <style>)
    │   └── data.json                 # Data file (served as static file or read from disk)
    └── Properties/
        └── launchSettings.json       # localhost:5000 or similar
```

### Data Flow

```
data.json → DashboardDataService (reads + deserializes at startup)
          → Injected into Dashboard.razor as scoped/singleton service
          → Dashboard.razor passes data to child components via [Parameter]
          → Components render HTML/SVG directly from model properties
```

### Key Architecture Decisions

1. **No SPA routing needed.** Single page, single route (`/`). Remove default Blazor nav/sidebar scaffolding.

2. **Static rendering preferred.** Since data doesn't change during a session, use `StreamRendering` or simply load data in `OnInitializedAsync`. No need for `StateHasChanged` loops or real-time updates.

3. **CSS ported directly from original HTML.** The `<style>` block in `OriginalDesignConcept.html` should be extracted into `dashboard.css` with minimal modifications. Class names can remain the same for fidelity.

4. **SVG generated in Blazor.** The timeline SVG is rendered via Blazor markup (`<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>` elements) with data-driven coordinates. Month positions are calculated as percentage offsets or fixed pixel positions based on the 1920px width.

5. **Fixed viewport.** Add `<meta name="viewport" content="width=1920">` and constrain `body` to 1920×1080. This ensures screenshots are pixel-consistent.

### `data.json` Schema Design

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogLink": "https://dev.azure.com/...",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonth": "Apr",
  "timeline": {
    "startMonth": "Jan",
    "endMonth": "Jun",
    "nowDate": "2026-04-10",
    "tracks": [
      {
        "name": "M1",
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
    "shipped": {
      "jan": ["Item A", "Item B"],
      "feb": ["Item C"],
      "mar": ["Item D", "Item E"],
      "apr": ["Item F"]
    },
    "inProgress": { ... },
    "carryover": { ... },
    "blockers": { ... }
  }
}
```

## 5. Security & Infrastructure

### Security (Intentionally Minimal)

- **No authentication.** Per requirements, this is a local-only tool. The app binds to `localhost` only.
- **No HTTPS required.** Running on `http://localhost:5000` is sufficient for local screenshot capture.
- **No CORS.** Single origin, no API consumers.
- **No input sanitization concerns.** Data comes from a trusted local JSON file, not user input.
- **Recommendation:** In `Program.cs`, explicitly bind to localhost only:
  ```csharp
  builder.WebHost.UseUrls("http://localhost:5000");
  ```

### Infrastructure

- **Hosting:** Local Kestrel server via `dotnet run`. No IIS, no Docker, no reverse proxy needed.
- **Deployment:** `dotnet publish -c Release` produces a self-contained folder. Can be copied to any Windows machine with .NET 8 runtime.
- **Optional: Self-contained publish** with `dotnet publish -c Release --self-contained -r win-x64` for zero-dependency deployment.
- **Infrastructure cost:** $0. Runs on developer's local machine.

### Screenshot Workflow

Since the primary output is PowerPoint screenshots:
1. Run `dotnet run`
2. Open `http://localhost:5000` in Edge/Chrome
3. Use browser's built-in screenshot or `Ctrl+Shift+S` (Edge) for full-page capture at 1920×1080
4. Paste into PowerPoint

**Alternative:** Consider adding a `/screenshot` endpoint later using `Playwright` for automated PNG generation, but this is out of scope for MVP.

## 6. Risks & Trade-offs

### Low Risks (Given Simplicity)

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| **SVG timeline complexity** | Medium | Low | The original HTML already has working SVG. Port coordinates directly; calculate positions with simple linear interpolation from date ranges. |
| **Blazor Server overhead for static content** | Low | Low | For a single-user local app, SignalR overhead is negligible. The alternative (Blazor Static SSR in .NET 8) could eliminate it, but adds complexity for no real benefit. |
| **JSON schema drift** | Low | Medium | Define strongly-typed C# models with `System.Text.Json` attributes. Add a simple validation step on load that logs warnings for missing fields. |
| **Browser rendering differences** | Low | Low | Fixed 1920×1080 viewport with Segoe UI eliminates most cross-browser issues. Recommend Edge or Chrome for screenshots. |

### Trade-offs Made

1. **No charting library → manual SVG.** This means the timeline component requires manual coordinate math, but gives pixel-perfect control matching the original design. A charting library would fight the custom layout.

2. **No database → JSON file.** Limits future extensibility (e.g., historical tracking, multi-project views) but keeps the system trivially simple. If needed later, SQLite via `Microsoft.Data.Sqlite` is a one-file addition.

3. **Blazor Server vs. static HTML.** Blazor adds a runtime dependency but provides component reusability, strong typing, and the ability to add interactivity later (e.g., click a milestone for details). For a screenshot-only tool, static HTML would suffice, but Blazor enables future enhancements without a rewrite.

4. **Fixed viewport vs. responsive.** Optimized for screenshot fidelity at the cost of mobile/tablet viewing. This is correct given the stated use case.

## 7. Open Questions

1. **How many months should the heatmap display?** The original shows 4 months (Jan–Apr). Should this be configurable in `data.json` (e.g., trailing 4 months, full fiscal year)?

2. **How many timeline tracks?** The original shows 3 milestones (M1, M2, M3). Is there a maximum? This affects SVG height calculation.

3. **Update frequency.** How often will `data.json` be updated? If weekly/monthly, manual editing is fine. If daily, consider a simpler editor UI or structured template.

4. **Multiple projects.** Will there ever be multiple dashboards (one per project)? If so, the data service should support loading different JSON files (e.g., `?project=alpha`).

5. **Color scheme customization.** Should the color palette (green/blue/amber/red) be configurable in `data.json`, or hardcoded as in the original design?

6. **Print/export.** Beyond screenshots, is PDF export needed? If so, consider `QuestPDF` (MIT license, .NET native) for server-side PDF generation.

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** Pixel-accurate reproduction of `OriginalDesignConcept.html` as a Blazor app reading from `data.json`.

1. **Scaffold project** — `dotnet new blazor -n ReportingDashboard --interactivity Server`
2. **Define models** — Create C# classes matching the `data.json` schema above
3. **Create `data.json`** — Populate with fictional project data matching the original design's structure
4. **Port CSS** — Extract styles from the HTML template into `dashboard.css`
5. **Build components** — Header → Timeline (SVG) → Heatmap (CSS Grid) → HeatmapRow → HeatmapCell
6. **Wire data service** — Read `data.json` on startup, inject into page
7. **Strip default Blazor chrome** — Remove nav sidebar, default layout, counter/weather pages

### Phase 2: Polish (1 day)

1. **Improve upon original design** — Better spacing, subtle shadows, refined typography weights
2. **Add legend component** — Dynamic legend based on milestone types present in data
3. **Add "NOW" line** — Calculate position from `DateTime.Now` relative to timeline range
4. **Add hover tooltips** — Optional: show full item details on hover (useful before screenshot)
5. **Validate JSON on load** — Log clear error messages if `data.json` is malformed

### Phase 3: Future Enhancements (Optional)

1. **Auto-screenshot endpoint** — Use `Microsoft.Playwright` to render the page as PNG on demand
2. **Multiple project support** — Route parameter `/project/{name}` loads different JSON files
3. **Historical snapshots** — Save dated copies of `data.json` for month-over-month comparison
4. **SQLite backend** — Migrate from JSON to SQLite if data complexity grows; use `Microsoft.Data.Sqlite` (no EF needed)

### Quick Wins

- **Reuse the original CSS verbatim.** The class names in `OriginalDesignConcept.html` are well-structured. Copy them directly into `dashboard.css` to guarantee visual fidelity.
- **Use `@inject` for the data service.** Blazor's built-in DI makes this trivial—register `DashboardDataService` as singleton in `Program.cs`.
- **Use `RenderFragment` for heatmap cells.** This keeps the grid component generic while allowing per-row color theming via CSS classes.

### NuGet Packages Required

```xml
<!-- ReportingDashboard.csproj — MINIMAL dependencies -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <!-- No additional NuGet packages needed for MVP -->
  <!-- Optional for Phase 3: -->
  <!-- <PackageReference Include="Microsoft.Playwright" Version="1.40+" /> -->
  <!-- <PackageReference Include="Microsoft.Data.Sqlite" Version="8.0+" /> -->
  <!-- <PackageReference Include="QuestPDF" Version="2024.3+" /> -->
</Project>
```

**The entire MVP requires zero third-party NuGet packages.** Everything needed—Blazor Server, System.Text.Json, Kestrel, CSS Grid, inline SVG—is included in the .NET 8 SDK.

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
