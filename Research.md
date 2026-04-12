# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-12 10:39 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipping status, and progress in a format optimized for PowerPoint screenshot capture at 1920×1080. The dashboard reads from a local `data.json` file—no database, no authentication, no cloud services. **Primary Recommendation:** Use a minimal Blazor Server app with zero third-party UI frameworks. The design is simple enough (CSS Grid/Flexbox layout, inline SVG for the timeline, color-coded heatmap cells) that native Blazor components with hand-written CSS will produce the cleanest, most maintainable result. Adding a charting library would be overengineering. The entire solution should be a single `.razor` page, a CSS file, a `data.json` config, and a model class—deliverable in a single sprint. ---

### Key Findings

- The original HTML design is entirely CSS Grid + Flexbox + inline SVG—no JavaScript frameworks or charting libraries are needed.
- Blazor Server's real-time SignalR connection is irrelevant here; the page is effectively static once rendered. Blazor Server is still the right choice for the .sln constraint and ease of local `dotnet run`.
- The timeline/Gantt section uses hand-drawn SVG elements (lines, circles, diamonds, text)—this maps perfectly to Blazor's ability to render raw SVG markup in `.razor` files.
- The heatmap grid is pure CSS Grid with `grid-template-columns: 160px repeat(4, 1fr)` and color-coded rows—no library needed.
- `System.Text.Json` (built into .NET 8) handles `data.json` deserialization with zero additional dependencies.
- The design targets exactly 1920×1080 for screenshot capture, so responsive design is unnecessary—fixed viewport is correct.
- The font stack (`Segoe UI`) is a Windows system font, requiring no web font loading.
- The color palette is small and well-defined (15 colors), best managed via CSS custom properties. --- | Step | Deliverable | Time | |------|-------------|------| | 1 | Scaffold Blazor Server project (`dotnet new blazorserver -n ReportingDashboard`) | 10 min | | 2 | Define C# model classes matching `data.json` schema | 30 min | | 3 | Create `data.json` with fictional project data | 30 min | | 4 | Port CSS from `OriginalDesignConcept.html` into `dashboard.css` | 1 hr | | 5 | Build `Dashboard.razor` — header + heatmap grid (skip timeline) | 2 hr | | 6 | Build timeline SVG rendering with date-to-pixel math | 2 hr | | 7 | Verify pixel-perfect match at 1920×1080 | 30 min |
- Add `data.json` file watcher for live reload without restart
- Add a route parameter for multi-project support (`/dashboard/project-alpha`)
- Add a "Last updated" timestamp in the footer
- Add subtle CSS transitions for a polished feel
- Print-to-PDF via browser's native print (add `@media print` CSS)
- Multiple dashboard themes (light/dark) via CSS custom property toggling
- Historical snapshots: save dated copies of `data.json` and add a date picker
- **Start by copying the HTML design's CSS verbatim** into `dashboard.css`. This guarantees visual fidelity from the first render. Refactor to CSS custom properties afterward.
- **Use `dotnet watch run`** during development for automatic rebuild on file changes.
- **Create the `data.json` first** with all fictional data before writing any Razor code. This forces you to finalize the data schema upfront.
- **Strip the default Blazor template** immediately—remove `NavMenu`, `MainLayout` sidebar, `Counter.razor`, `Weather.razor`, and `FetchData.razor`. Replace `MainLayout.razor` with a blank layout that just renders `@Body`.
```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Engineering Platform · Core Services · April 2026",
  "backlogLink": "https://dev.azure.com/contoso/Phoenix/_backlogs",
  "currentMonth": "Apr",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "milestones": [
    {
      "label": "M1",
      "description": "API Gateway & Auth",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-15", "type": "checkpoint", "label": "Jan 15" },
        { "date": "2026-03-20", "type": "poc", "label": "Mar 20 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "May Prod" }
      ]
    },
    {
      "label": "M2",
      "description": "Data Pipeline & ETL",
      "color": "#00897B",
      "events": [
        { "date": "2025-12-19", "type": "checkpoint", "label": "Dec 19" },
        { "date": "2026-02-11", "type": "checkpoint", "label": "Feb 11" },
        { "date": "2026-03-15", "type": "poc", "label": "Mar 15 PoC" },
        { "date": "2026-06-01", "type": "production", "label": "Jun Prod" }
      ]
    }
  ],
  "heatmapRows": [
    {
      "category": "shipped",
      "label": "SHIPPED",
      "items": {
        "Jan": ["Auth service v2", "Token refresh flow"],
        "Feb": ["Rate limiter", "Health checks"],
        "Mar": ["Retry policies", "Circuit breaker"],
        "Apr": ["Dashboard v1"]
      }
    },
    {
      "category": "in-progress",
      "label": "IN PROGRESS",
      "items": {
        "Jan": ["API Gateway design"],
        "Feb": ["Gateway prototype"],
        "Mar": ["Load testing"],
        "Apr": ["Gateway hardening", "Docs update"]
      }
    },
    {
      "category": "carryover",
      "label": "CARRYOVER",
      "items": {
        "Jan": [],
        "Feb": ["Schema migration"],
        "Mar": ["Schema migration"],
        "Apr": ["Perf benchmarks"]
      }
    },
    {
      "category": "blockers",
      "label": "BLOCKERS",
      "items": {
        "Jan": [],
        "Feb": [],
        "Mar": ["Vendor SDK delay"],
        "Apr": ["Vendor SDK delay", "Test env down"]
      }
    }
  ]
}
```
```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── Models/
│   │   └── DashboardConfig.cs
│   ├── Services/
│   │   └── DashboardDataService.cs
│   ├── Pages/
│   │   └── Dashboard.razor
│   ├── Layout/
│   │   └── EmptyLayout.razor
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── dashboard.css
│   │   └── data/
│   │       └── data.json
│   └── Properties/
│       └── launchSettings.json
``` --- *Research completed April 2026. All recommendations target .NET 8 LTS (supported through November 2026). No third-party NuGet packages are required for the MVP.*

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Built-in, no additional packages | | **CSS Layout** | Native CSS Grid + Flexbox | CSS3 | Matches original design exactly | | **Timeline/Gantt** | Inline SVG in Razor | SVG 1.1 | Hand-drawn lines, diamonds, circles—no library | | **Icons/Shapes** | SVG primitives | — | `<polygon>` for diamonds, `<circle>` for milestones | | **Styling** | Single CSS file (`app.css`) | — | CSS custom properties for the color palette | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Native, high-performance, zero dependencies | | **Data Source** | `data.json` flat file | — | Read on startup via `IConfiguration` or direct file read | | **Hosting** | Kestrel (built-in) | .NET 8 | `dotnet run` serves locally on `https://localhost:5001` | | Component | Technology | Notes | |-----------|-----------|-------| | **Solution** | `.sln` with single `.csproj` | `ReportingDashboard.sln` → `ReportingDashboard.csproj` | | **Build** | `dotnet build` / `dotnet run` | No additional tooling | | **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Full .NET 8 Blazor support | | Library | Why Skip It | |---------|------------| | **MudBlazor / Radzen / Syncfusion** | Massive overkill for a single static-layout page. Adds 500KB+ CSS, component overhead, and theming complexity. | | **Chart.js / ApexCharts / Plotly** | The timeline is simple SVG geometry (6 lines, 8 shapes, 12 text labels). A charting library would fight the custom layout. | | **Entity Framework** | No database. It's a JSON file. | | **Blazorise / Bootstrap** | The design uses a custom grid layout. Bootstrap's 12-column grid would conflict. | | **SignalR customization** | Blazor Server includes SignalR by default. No tuning needed for a single-user local page. | | Library | Version | Purpose | Verdict | |---------|---------|---------|---------| | **`Microsoft.Extensions.FileProviders.Physical`** | Built into .NET 8 | Watch `data.json` for changes and auto-refresh | Nice-to-have: use `IFileProvider` + `Watch()` to detect edits to `data.json` and trigger a re-render without restarting the app | ---
```
┌─────────────────────────────────────┐
│         Browser (1920×1080)          │
│  ┌─────────────────────────────────┐ │
│  │   Dashboard.razor (single page) │ │
│  │   ├── Header section            │ │
│  │   ├── Timeline SVG section      │ │
│  │   └── Heatmap Grid section      │ │
│  └─────────────────────────────────┘ │
│         ▲ SignalR (Blazor Server)     │
└─────────┼───────────────────────────┘
          │
┌─────────┴───────────────────────────┐
│         Kestrel (.NET 8)             │
│  ┌──────────┐  ┌──────────────────┐ │
│  │ Dashboard │  │ DashboardData    │ │
│  │ Service   │──│ Model (POCOs)    │ │
│  └─────┬────┘  └──────────────────┘ │
│        │                             │
│  ┌─────┴────┐                        │
│  │data.json │                        │
│  └──────────┘                        │
└──────────────────────────────────────┘
```
```csharp
public record DashboardConfig
{
    public string Title { get; init; }           // "Privacy Automation Release Roadmap"
    public string Subtitle { get; init; }        // "Trusted Platform · Privacy Automation · April 2026"
    public string BacklogLink { get; init; }     // URL to ADO backlog
    public string CurrentMonth { get; init; }    // "Apr" — highlights the current column
    public List<string> Months { get; init; }    // ["Jan", "Feb", "Mar", "Apr"]
    public List<Milestone> Milestones { get; init; }
    public List<HeatmapRow> HeatmapRows { get; init; }
}

public record Milestone
{
    public string Label { get; init; }           // "M1 — Chatbot & MS Role"
    public string Color { get; init; }           // "#0078D4"
    public List<MilestoneEvent> Events { get; init; }
}

public record MilestoneEvent
{
    public string Date { get; init; }            // "2026-01-12"
    public string Type { get; init; }            // "checkpoint" | "poc" | "production"
    public string Label { get; init; }           // "Jan 12" or "Mar 26 PoC"
}

public record HeatmapRow
{
    public string Category { get; init; }        // "shipped" | "in-progress" | "carryover" | "blockers"
    public string Label { get; init; }           // "SHIPPED"
    public Dictionary<string, List<string>> Items { get; init; } // month → list of item names
}
``` Keep it **flat**—avoid over-componentizing a single page: | File | Purpose | |------|---------| | `Pages/Dashboard.razor` | The single page. Reads data, renders all three sections. | | `Models/DashboardConfig.cs` | POCOs for `data.json` deserialization | | `Services/DashboardDataService.cs` | Reads and deserializes `data.json`, registered as singleton | | `wwwroot/css/dashboard.css` | All styles, ported from the original HTML `<style>` block | | `wwwroot/data/data.json` | The configuration file with all dashboard content | | `Layout/EmptyLayout.razor` | A blank layout (no nav, no sidebar—just the page) | The timeline section is the most complex visual element. Recommended approach:
- **Calculate positions mathematically** in C# based on date ranges: `xPosition = (date - startDate) / (endDate - startDate) * svgWidth`
- **Render SVG directly in Razor** using `@foreach` loops over milestones and events
- **Use SVG primitives**: `<line>` for tracks, `<circle>` for checkpoints, `<polygon>` for diamonds (PoC/Production), `<text>` for labels
- **"Now" indicator**: Vertical dashed red line calculated from `DateTime.Today`
```razor
<svg width="1560" height="185" style="overflow:visible">
    @foreach (var milestone in Data.Milestones)
    {
        <line x1="0" y1="@yPos" x2="1560" y2="@yPos" 
              stroke="@milestone.Color" stroke-width="3"/>
        @foreach (var evt in milestone.Events)
        {
            @if (evt.Type == "poc")
            {
                <polygon points="@DiamondPoints(xPos, yPos)" fill="#F4B400"/>
            }
        }
    }
</svg>
``` Port the original HTML's `<style>` block directly into `dashboard.css` with these improvements:
```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-current: #FFE4E4;
    --color-now-line: #EA4335;
    --font-primary: 'Segoe UI', Arial, sans-serif;
    --page-width: 1920px;
    --page-height: 1080px;
}
``` ---

### Considerations & Risks

- Per requirements, this is a **local-only, no-auth, no-enterprise-security** application. | Concern | Decision | Rationale | |---------|----------|-----------| | **Authentication** | None | Local tool, single user, screenshot capture use case | | **Authorization** | None | No roles, no permissions | | **HTTPS** | Use Kestrel's default dev cert | `dotnet dev-certs https --trust` on first run | | **Input validation** | Validate `data.json` schema on load | Prevent malformed JSON from crashing the page | | **CORS** | Not applicable | No API calls from external origins | | **CSP headers** | Not needed | No user-generated content, no external scripts | | Aspect | Decision | |--------|----------| | **Hosting** | `dotnet run` on localhost, Kestrel | | **Port** | Default `https://localhost:5001` or configure in `launchSettings.json` | | **Deployment** | None. This is a local tool. Optionally `dotnet publish -c Release` for a self-contained binary. | | **CI/CD** | Not needed for v1. If desired later: `dotnet build && dotnet test` in a GitHub Actions workflow. | | **Monitoring** | Console logging via `ILogger<T>` (built-in). No telemetry. | | **Database** | None. `data.json` is the entire data layer. | | **Containerization** | Not needed. If desired: `mcr.microsoft.com/dotnet/aspnet:8.0` base image, but overkill for local use. | | Scale | Cost | |-------|------| | **Local development** | $0 — runs on developer's machine | | **Shared team use** | $0 — could run on any Windows/Mac/Linux box with .NET 8 SDK | --- | Risk | Likelihood | Impact | Mitigation | |------|-----------|--------|------------| | **SVG timeline calculation complexity** | Medium | Medium | Pre-calculate all positions in C# service; unit test the math. The original HTML hardcodes pixel positions—the dynamic version needs date-to-pixel mapping. | | **Blazor Server SignalR overhead for a static page** | Low | Low | Acceptable. The alternative (Blazor WebAssembly) adds WASM download time and complicates the .sln structure. Server-side rendering is fine for a local tool. | | **`data.json` schema drift** | Low | Medium | Define a strict C# model and validate on load. Throw a clear error message if required fields are missing. | | **Screenshot fidelity** | Medium | High | The page must render identically to the HTML mockup at exactly 1920×1080. Use a fixed `body` width/height, not responsive design. Test in Chrome at 100% zoom. | | **Font rendering differences** | Low | Low | Segoe UI is pre-installed on Windows. On Mac/Linux, it falls back to Arial. For PowerPoint screenshots on Windows, this is fine. | | Decision | Trade-off | Why It's Right | |----------|-----------|---------------| | No charting library | Manual SVG means more code for the timeline | The timeline has ~20 SVG elements. A charting library would add 200KB+ and fight the custom layout. | | Single `.razor` page, no component splitting | Harder to reuse parts | There's nothing to reuse. It's one page for one purpose. | | No database | Can't query historical data | The use case is "edit JSON, screenshot, paste in PowerPoint." A database adds complexity for zero value. | | No hot-reload of `data.json` | Must restart app to see changes | Could add `IFileProvider.Watch()` as a v1.1 enhancement, but `dotnet watch` already handles this during development. | | Blazor Server over static HTML | More infrastructure than needed | Meets the .sln constraint, enables future enhancements (e.g., print-to-PDF button, multiple projects), and `dotnet run` is one command. | --- | # | Question | Who Decides | Default If No Answer | |---|----------|-------------|---------------------| | 1 | **How many months should the heatmap display?** The original shows 4 (Jan–Apr). Should it always show the last 4 months, or be configurable? | Product Owner | Configurable in `data.json` via the `Months` array | | 2 | **Should the timeline auto-calculate the "Now" line position from today's date, or should it be manually set in `data.json`?** | Product Owner | Auto-calculate from `DateTime.Today` | | 3 | **Will there be multiple projects/dashboards, or always a single one?** This affects whether `data.json` becomes `data/project-alpha.json`, `data/project-beta.json`, etc. | Product Owner | Single project for v1; structure `data.json` to allow future multi-project support | | 4 | **Is there a need for a "print" or "export to image" button, or is browser screenshot (Ctrl+Shift+S in Edge) sufficient?** | End User | No export button in v1; manual screenshot | | 5 | **Should the backlog link ("→ ADO Backlog") actually navigate somewhere, or is it decorative for the screenshot?** | Product Owner | Make it a real `<a href>` from `data.json`; it's useful when viewing the live page even if screenshots don't use it | ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipping status, and progress in a format optimized for PowerPoint screenshot capture at 1920×1080. The dashboard reads from a local `data.json` file—no database, no authentication, no cloud services.

**Primary Recommendation:** Use a minimal Blazor Server app with zero third-party UI frameworks. The design is simple enough (CSS Grid/Flexbox layout, inline SVG for the timeline, color-coded heatmap cells) that native Blazor components with hand-written CSS will produce the cleanest, most maintainable result. Adding a charting library would be overengineering. The entire solution should be a single `.razor` page, a CSS file, a `data.json` config, and a model class—deliverable in a single sprint.

---

## 2. Key Findings

- The original HTML design is entirely CSS Grid + Flexbox + inline SVG—no JavaScript frameworks or charting libraries are needed.
- Blazor Server's real-time SignalR connection is irrelevant here; the page is effectively static once rendered. Blazor Server is still the right choice for the .sln constraint and ease of local `dotnet run`.
- The timeline/Gantt section uses hand-drawn SVG elements (lines, circles, diamonds, text)—this maps perfectly to Blazor's ability to render raw SVG markup in `.razor` files.
- The heatmap grid is pure CSS Grid with `grid-template-columns: 160px repeat(4, 1fr)` and color-coded rows—no library needed.
- `System.Text.Json` (built into .NET 8) handles `data.json` deserialization with zero additional dependencies.
- The design targets exactly 1920×1080 for screenshot capture, so responsive design is unnecessary—fixed viewport is correct.
- The font stack (`Segoe UI`) is a Windows system font, requiring no web font loading.
- The color palette is small and well-defined (15 colors), best managed via CSS custom properties.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Built-in, no additional packages |
| **CSS Layout** | Native CSS Grid + Flexbox | CSS3 | Matches original design exactly |
| **Timeline/Gantt** | Inline SVG in Razor | SVG 1.1 | Hand-drawn lines, diamonds, circles—no library |
| **Icons/Shapes** | SVG primitives | — | `<polygon>` for diamonds, `<circle>` for milestones |
| **Styling** | Single CSS file (`app.css`) | — | CSS custom properties for the color palette |

### Backend / Data Layer

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Native, high-performance, zero dependencies |
| **Data Source** | `data.json` flat file | — | Read on startup via `IConfiguration` or direct file read |
| **Hosting** | Kestrel (built-in) | .NET 8 | `dotnet run` serves locally on `https://localhost:5001` |

### Project Structure

| Component | Technology | Notes |
|-----------|-----------|-------|
| **Solution** | `.sln` with single `.csproj` | `ReportingDashboard.sln` → `ReportingDashboard.csproj` |
| **Build** | `dotnet build` / `dotnet run` | No additional tooling |
| **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Full .NET 8 Blazor support |

### Libraries: What NOT to Add

| Library | Why Skip It |
|---------|------------|
| **MudBlazor / Radzen / Syncfusion** | Massive overkill for a single static-layout page. Adds 500KB+ CSS, component overhead, and theming complexity. |
| **Chart.js / ApexCharts / Plotly** | The timeline is simple SVG geometry (6 lines, 8 shapes, 12 text labels). A charting library would fight the custom layout. |
| **Entity Framework** | No database. It's a JSON file. |
| **Blazorise / Bootstrap** | The design uses a custom grid layout. Bootstrap's 12-column grid would conflict. |
| **SignalR customization** | Blazor Server includes SignalR by default. No tuning needed for a single-user local page. |

### The One Optional Library Worth Considering

| Library | Version | Purpose | Verdict |
|---------|---------|---------|---------|
| **`Microsoft.Extensions.FileProviders.Physical`** | Built into .NET 8 | Watch `data.json` for changes and auto-refresh | Nice-to-have: use `IFileProvider` + `Watch()` to detect edits to `data.json` and trigger a re-render without restarting the app |

---

## 4. Architecture Recommendations

### Overall Architecture

```
┌─────────────────────────────────────┐
│         Browser (1920×1080)          │
│  ┌─────────────────────────────────┐ │
│  │   Dashboard.razor (single page) │ │
│  │   ├── Header section            │ │
│  │   ├── Timeline SVG section      │ │
│  │   └── Heatmap Grid section      │ │
│  └─────────────────────────────────┘ │
│         ▲ SignalR (Blazor Server)     │
└─────────┼───────────────────────────┘
          │
┌─────────┴───────────────────────────┐
│         Kestrel (.NET 8)             │
│  ┌──────────┐  ┌──────────────────┐ │
│  │ Dashboard │  │ DashboardData    │ │
│  │ Service   │──│ Model (POCOs)    │ │
│  └─────┬────┘  └──────────────────┘ │
│        │                             │
│  ┌─────┴────┐                        │
│  │data.json │                        │
│  └──────────┘                        │
└──────────────────────────────────────┘
```

### Data Model (`data.json` → C# POCOs)

```csharp
public record DashboardConfig
{
    public string Title { get; init; }           // "Privacy Automation Release Roadmap"
    public string Subtitle { get; init; }        // "Trusted Platform · Privacy Automation · April 2026"
    public string BacklogLink { get; init; }     // URL to ADO backlog
    public string CurrentMonth { get; init; }    // "Apr" — highlights the current column
    public List<string> Months { get; init; }    // ["Jan", "Feb", "Mar", "Apr"]
    public List<Milestone> Milestones { get; init; }
    public List<HeatmapRow> HeatmapRows { get; init; }
}

public record Milestone
{
    public string Label { get; init; }           // "M1 — Chatbot & MS Role"
    public string Color { get; init; }           // "#0078D4"
    public List<MilestoneEvent> Events { get; init; }
}

public record MilestoneEvent
{
    public string Date { get; init; }            // "2026-01-12"
    public string Type { get; init; }            // "checkpoint" | "poc" | "production"
    public string Label { get; init; }           // "Jan 12" or "Mar 26 PoC"
}

public record HeatmapRow
{
    public string Category { get; init; }        // "shipped" | "in-progress" | "carryover" | "blockers"
    public string Label { get; init; }           // "SHIPPED"
    public Dictionary<string, List<string>> Items { get; init; } // month → list of item names
}
```

### Component Breakdown

Keep it **flat**—avoid over-componentizing a single page:

| File | Purpose |
|------|---------|
| `Pages/Dashboard.razor` | The single page. Reads data, renders all three sections. |
| `Models/DashboardConfig.cs` | POCOs for `data.json` deserialization |
| `Services/DashboardDataService.cs` | Reads and deserializes `data.json`, registered as singleton |
| `wwwroot/css/dashboard.css` | All styles, ported from the original HTML `<style>` block |
| `wwwroot/data/data.json` | The configuration file with all dashboard content |
| `Layout/EmptyLayout.razor` | A blank layout (no nav, no sidebar—just the page) |

### SVG Timeline Rendering Strategy

The timeline section is the most complex visual element. Recommended approach:

1. **Calculate positions mathematically** in C# based on date ranges: `xPosition = (date - startDate) / (endDate - startDate) * svgWidth`
2. **Render SVG directly in Razor** using `@foreach` loops over milestones and events
3. **Use SVG primitives**: `<line>` for tracks, `<circle>` for checkpoints, `<polygon>` for diamonds (PoC/Production), `<text>` for labels
4. **"Now" indicator**: Vertical dashed red line calculated from `DateTime.Today`

```razor
<svg width="1560" height="185" style="overflow:visible">
    @foreach (var milestone in Data.Milestones)
    {
        <line x1="0" y1="@yPos" x2="1560" y2="@yPos" 
              stroke="@milestone.Color" stroke-width="3"/>
        @foreach (var evt in milestone.Events)
        {
            @if (evt.Type == "poc")
            {
                <polygon points="@DiamondPoints(xPos, yPos)" fill="#F4B400"/>
            }
        }
    }
</svg>
```

### CSS Architecture

Port the original HTML's `<style>` block directly into `dashboard.css` with these improvements:

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-current: #FFE4E4;
    --color-now-line: #EA4335;
    --font-primary: 'Segoe UI', Arial, sans-serif;
    --page-width: 1920px;
    --page-height: 1080px;
}
```

---

## 5. Security & Infrastructure

### Security: Intentionally Minimal

Per requirements, this is a **local-only, no-auth, no-enterprise-security** application.

| Concern | Decision | Rationale |
|---------|----------|-----------|
| **Authentication** | None | Local tool, single user, screenshot capture use case |
| **Authorization** | None | No roles, no permissions |
| **HTTPS** | Use Kestrel's default dev cert | `dotnet dev-certs https --trust` on first run |
| **Input validation** | Validate `data.json` schema on load | Prevent malformed JSON from crashing the page |
| **CORS** | Not applicable | No API calls from external origins |
| **CSP headers** | Not needed | No user-generated content, no external scripts |

### Infrastructure: Local Development Only

| Aspect | Decision |
|--------|----------|
| **Hosting** | `dotnet run` on localhost, Kestrel |
| **Port** | Default `https://localhost:5001` or configure in `launchSettings.json` |
| **Deployment** | None. This is a local tool. Optionally `dotnet publish -c Release` for a self-contained binary. |
| **CI/CD** | Not needed for v1. If desired later: `dotnet build && dotnet test` in a GitHub Actions workflow. |
| **Monitoring** | Console logging via `ILogger<T>` (built-in). No telemetry. |
| **Database** | None. `data.json` is the entire data layer. |
| **Containerization** | Not needed. If desired: `mcr.microsoft.com/dotnet/aspnet:8.0` base image, but overkill for local use. |

### Estimated Costs

| Scale | Cost |
|-------|------|
| **Local development** | $0 — runs on developer's machine |
| **Shared team use** | $0 — could run on any Windows/Mac/Linux box with .NET 8 SDK |

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| **SVG timeline calculation complexity** | Medium | Medium | Pre-calculate all positions in C# service; unit test the math. The original HTML hardcodes pixel positions—the dynamic version needs date-to-pixel mapping. |
| **Blazor Server SignalR overhead for a static page** | Low | Low | Acceptable. The alternative (Blazor WebAssembly) adds WASM download time and complicates the .sln structure. Server-side rendering is fine for a local tool. |
| **`data.json` schema drift** | Low | Medium | Define a strict C# model and validate on load. Throw a clear error message if required fields are missing. |
| **Screenshot fidelity** | Medium | High | The page must render identically to the HTML mockup at exactly 1920×1080. Use a fixed `body` width/height, not responsive design. Test in Chrome at 100% zoom. |
| **Font rendering differences** | Low | Low | Segoe UI is pre-installed on Windows. On Mac/Linux, it falls back to Arial. For PowerPoint screenshots on Windows, this is fine. |

### Trade-offs Made

| Decision | Trade-off | Why It's Right |
|----------|-----------|---------------|
| No charting library | Manual SVG means more code for the timeline | The timeline has ~20 SVG elements. A charting library would add 200KB+ and fight the custom layout. |
| Single `.razor` page, no component splitting | Harder to reuse parts | There's nothing to reuse. It's one page for one purpose. |
| No database | Can't query historical data | The use case is "edit JSON, screenshot, paste in PowerPoint." A database adds complexity for zero value. |
| No hot-reload of `data.json` | Must restart app to see changes | Could add `IFileProvider.Watch()` as a v1.1 enhancement, but `dotnet watch` already handles this during development. |
| Blazor Server over static HTML | More infrastructure than needed | Meets the .sln constraint, enables future enhancements (e.g., print-to-PDF button, multiple projects), and `dotnet run` is one command. |

---

## 7. Open Questions

| # | Question | Who Decides | Default If No Answer |
|---|----------|-------------|---------------------|
| 1 | **How many months should the heatmap display?** The original shows 4 (Jan–Apr). Should it always show the last 4 months, or be configurable? | Product Owner | Configurable in `data.json` via the `Months` array |
| 2 | **Should the timeline auto-calculate the "Now" line position from today's date, or should it be manually set in `data.json`?** | Product Owner | Auto-calculate from `DateTime.Today` |
| 3 | **Will there be multiple projects/dashboards, or always a single one?** This affects whether `data.json` becomes `data/project-alpha.json`, `data/project-beta.json`, etc. | Product Owner | Single project for v1; structure `data.json` to allow future multi-project support |
| 4 | **Is there a need for a "print" or "export to image" button, or is browser screenshot (Ctrl+Shift+S in Edge) sufficient?** | End User | No export button in v1; manual screenshot |
| 5 | **Should the backlog link ("→ ADO Backlog") actually navigate somewhere, or is it decorative for the screenshot?** | Product Owner | Make it a real `<a href>` from `data.json`; it's useful when viewing the live page even if screenshots don't use it |

---

## 8. Implementation Recommendations

### Phasing

#### Phase 1: MVP (1–2 days)

| Step | Deliverable | Time |
|------|-------------|------|
| 1 | Scaffold Blazor Server project (`dotnet new blazorserver -n ReportingDashboard`) | 10 min |
| 2 | Define C# model classes matching `data.json` schema | 30 min |
| 3 | Create `data.json` with fictional project data | 30 min |
| 4 | Port CSS from `OriginalDesignConcept.html` into `dashboard.css` | 1 hr |
| 5 | Build `Dashboard.razor` — header + heatmap grid (skip timeline) | 2 hr |
| 6 | Build timeline SVG rendering with date-to-pixel math | 2 hr |
| 7 | Verify pixel-perfect match at 1920×1080 | 30 min |

**MVP Total: ~7 hours of focused work.**

#### Phase 2: Polish (Optional, 1 day)

- Add `data.json` file watcher for live reload without restart
- Add a route parameter for multi-project support (`/dashboard/project-alpha`)
- Add a "Last updated" timestamp in the footer
- Add subtle CSS transitions for a polished feel

#### Phase 3: Future Enhancements (If Needed)

- Print-to-PDF via browser's native print (add `@media print` CSS)
- Multiple dashboard themes (light/dark) via CSS custom property toggling
- Historical snapshots: save dated copies of `data.json` and add a date picker

### Quick Wins

1. **Start by copying the HTML design's CSS verbatim** into `dashboard.css`. This guarantees visual fidelity from the first render. Refactor to CSS custom properties afterward.
2. **Use `dotnet watch run`** during development for automatic rebuild on file changes.
3. **Create the `data.json` first** with all fictional data before writing any Razor code. This forces you to finalize the data schema upfront.
4. **Strip the default Blazor template** immediately—remove `NavMenu`, `MainLayout` sidebar, `Counter.razor`, `Weather.razor`, and `FetchData.razor`. Replace `MainLayout.razor` with a blank layout that just renders `@Body`.

### Recommended `data.json` Example (Fictional Project)

```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Engineering Platform · Core Services · April 2026",
  "backlogLink": "https://dev.azure.com/contoso/Phoenix/_backlogs",
  "currentMonth": "Apr",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "milestones": [
    {
      "label": "M1",
      "description": "API Gateway & Auth",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-15", "type": "checkpoint", "label": "Jan 15" },
        { "date": "2026-03-20", "type": "poc", "label": "Mar 20 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "May Prod" }
      ]
    },
    {
      "label": "M2",
      "description": "Data Pipeline & ETL",
      "color": "#00897B",
      "events": [
        { "date": "2025-12-19", "type": "checkpoint", "label": "Dec 19" },
        { "date": "2026-02-11", "type": "checkpoint", "label": "Feb 11" },
        { "date": "2026-03-15", "type": "poc", "label": "Mar 15 PoC" },
        { "date": "2026-06-01", "type": "production", "label": "Jun Prod" }
      ]
    }
  ],
  "heatmapRows": [
    {
      "category": "shipped",
      "label": "SHIPPED",
      "items": {
        "Jan": ["Auth service v2", "Token refresh flow"],
        "Feb": ["Rate limiter", "Health checks"],
        "Mar": ["Retry policies", "Circuit breaker"],
        "Apr": ["Dashboard v1"]
      }
    },
    {
      "category": "in-progress",
      "label": "IN PROGRESS",
      "items": {
        "Jan": ["API Gateway design"],
        "Feb": ["Gateway prototype"],
        "Mar": ["Load testing"],
        "Apr": ["Gateway hardening", "Docs update"]
      }
    },
    {
      "category": "carryover",
      "label": "CARRYOVER",
      "items": {
        "Jan": [],
        "Feb": ["Schema migration"],
        "Mar": ["Schema migration"],
        "Apr": ["Perf benchmarks"]
      }
    },
    {
      "category": "blockers",
      "label": "BLOCKERS",
      "items": {
        "Jan": [],
        "Feb": [],
        "Mar": ["Vendor SDK delay"],
        "Apr": ["Vendor SDK delay", "Test env down"]
      }
    }
  ]
}
```

### File Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── Models/
│   │   └── DashboardConfig.cs
│   ├── Services/
│   │   └── DashboardDataService.cs
│   ├── Pages/
│   │   └── Dashboard.razor
│   ├── Layout/
│   │   └── EmptyLayout.razor
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── dashboard.css
│   │   └── data/
│   │       └── data.json
│   └── Properties/
│       └── launchSettings.json
```

---

*Research completed April 2026. All recommendations target .NET 8 LTS (supported through November 2026). No third-party NuGet packages are required for the MVP.*

## Visual Design References

The following design reference files were found in the repository. These MUST be used as the canonical visual specification when building UI components.

### `docs/design-screenshots/OriginalDesignConcept.png`

**Type:** Design Image — engineers should reference this file visually

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/daed009bbbb7cf24b9208f8b5f664300e5ed1468/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
