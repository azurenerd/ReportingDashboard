# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-16 16:49 UTC_

### Summary

This project is a **single-page executive reporting dashboard** that visualizes project milestones, progress status, and monthly heatmaps. The design reference (`OriginalDesignConcept.html`) defines a fixed 1920×1080 layout with a timeline/Gantt SVG header, a four-row heatmap grid (Shipped, In Progress, Carryover, Blockers), and clean typography optimized for PowerPoint screenshot capture. **Primary Recommendation:** Build this as a minimal Blazor Server application with a single Razor component page that reads from a local `data.json` file. No database is needed. No authentication is needed. The entire application should be ~5-8 files total. Use Blazor's built-in rendering with raw CSS (ported from the HTML reference) and inline SVG for the timeline. Avoid heavy component libraries—the design is simple enough that hand-crafted CSS matching the reference will produce the cleanest, most screenshot-friendly result. --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | (none beyond default template) | — | — | — | The .NET 8 Blazor Server template includes everything needed: Kestrel, Razor components, CSS isolation, `System.Text.Json`, `IConfiguration`, `FileSystemWatcher`. **Zero additional NuGet packages required.** If testing is desired: | Package | Version | Purpose | |---------|---------|---------| | `xunit` | 2.7.0+ | Test runner | | `bUnit` | 1.25.0+ | Blazor component testing | | `FluentAssertions` | 6.12.0+ | Readable assertions | | `Microsoft.NET.Test.Sdk` | 17.9.0+ | Test host |

### Key Findings

- The design is a **pixel-perfect, fixed-resolution layout** (1920×1080) intended for screenshot capture into PowerPoint. This means responsive design is unnecessary—target the exact viewport size.
- The HTML reference uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) for the heatmap and **Flexbox** for header/layout sections. Both are natively supported in Blazor via standard CSS.
- The timeline section uses **inline SVG** with simple shapes: `<line>`, `<circle>`, `<polygon>` (diamond milestones), `<text>`, and a `<feDropShadow>` filter. This can be rendered directly in Razor markup—no charting library needed.
- The color system is well-defined with **four semantic color tracks**: green (Shipped), blue (In Progress), amber (Carryover), red (Blockers). Each has background, text, and bullet colors.
- A **JSON file** (`data.json`) is the ideal data source given the simplicity requirement. `System.Text.Json` (built into .NET 8) handles deserialization with zero additional dependencies.
- Blazor Server's **SignalR circuit** is overkill for a static dashboard, but the project simplicity means this overhead is negligible and the developer experience (hot reload, C# everywhere) justifies it.
- The total NuGet dependency count should be **zero beyond the default Blazor Server template**. This project needs no ORM, no auth library, no charting library, no CSS framework. ---
- `dotnet new blazor --interactivity Server -n ReportingDashboard`
- Delete default Counter/Weather pages
- Create `Dashboard.razor` with hardcoded HTML ported from `OriginalDesignConcept.html`
- Port CSS into `Dashboard.razor.css`
- **Verify:** Page renders at 1920×1080 matching the reference screenshot
- Define `DashboardData` record types in `Models/`
- Create `data.json` with fictional project data
- Create `DashboardDataService` that reads and deserializes `data.json`
- Register service in `Program.cs` as singleton
- **Verify:** `DashboardDataService` correctly deserializes all fields
- Replace hardcoded HTML in `Dashboard.razor` with `@foreach` loops over model data
- Implement `DateToX()` calculation for SVG timeline positioning
- Render milestone diamonds, checkpoints, and track lines from data
- Render heatmap grid cells from data
- **Verify:** Changing `data.json` values changes the rendered dashboard
- Add CSS custom properties for color theming
- Add `FileSystemWatcher` for auto-reload on `data.json` changes (optional)
- Add `@media print` styles for clean printing
- Fine-tune spacing, font sizes, and colors to match reference exactly
- Document the screenshot capture procedure
- **Verify:** Final screenshot is indistinguishable from the reference design
- **Hot reload workflow:** `dotnet watch` gives instant CSS feedback, critical for matching the pixel-perfect reference.
- **CSS custom properties:** Define once, reference everywhere. Changing the shipped color from green to teal is a single-line change.
- **JSON IntelliSense:** If using VS Code, a `data.schema.json` file gives autocomplete when editing `data.json`.
- ❌ Authentication or authorization
- ❌ A database or ORM
- ❌ An admin UI for editing data
- ❌ Responsive/mobile layout
- ❌ API endpoints
- ❌ Logging infrastructure
- ❌ CI/CD pipeline
- ❌ Docker containerization
- ❌ Unit tests (unless JSON deserialization needs validation) This is a **screenshot generator for PowerPoint decks**. Scope discipline is the most important architectural decision. ---

### Recommended Tools & Technologies

- **Project:** Executive Milestone & Progress Reporting Dashboard **Date:** April 16, 2026 **Stack:** C# .NET 8 · Blazor Server · Local Only · .sln Structure --- | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **UI Framework** | Blazor Server (built-in) | .NET 8.0 LTS | Ships with `Microsoft.AspNetCore.App` | | **CSS Approach** | Hand-written CSS, ported from `OriginalDesignConcept.html` | N/A | Use CSS isolation (`Dashboard.razor.css`) for component-scoped styles | | **SVG Timeline** | Inline SVG in Razor markup | N/A | No library needed; the reference SVG is ~50 lines | | **Layout System** | CSS Grid + Flexbox | N/A | Grid for heatmap, Flexbox for header/chrome | | **Font** | Segoe UI (system font) | N/A | Already specified in design; no web font loading needed | | **Icons/Shapes** | Inline SVG shapes | N/A | Diamonds (`<polygon>`), circles, lines per the reference | **Why no component library (MudBlazor, Radzen, etc.):** The design is a single fixed-layout page with no forms, no data tables, no dropdowns, no modals. Adding a component library introduces 500KB+ of CSS/JS that would interfere with the pixel-perfect screenshot requirement. The reference HTML achieves the entire design in ~120 lines of CSS. **Why no charting library (ApexCharts.Blazor, ChartJs.Blazor):** The timeline is a simple SVG with positioned shapes at calculated X coordinates. A charting library would impose its own styling, axis logic, and responsiveness—all counterproductive for a fixed screenshot layout. | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Runtime** | .NET 8.0 LTS | 8.0.x | Long-term support until Nov 2026 | | **Web Framework** | ASP.NET Core Blazor Server | 8.0.x | Included in SDK | | **JSON Deserialization** | `System.Text.Json` | Built-in | Use source generators for AOT-friendly deserialization | | **File Watching** | `FileSystemWatcher` | Built-in | Optional: auto-reload dashboard when `data.json` changes | | **Configuration** | `IConfiguration` / direct file read | Built-in | Read `data.json` from a known path | | Component | Recommendation | Notes | |-----------|---------------|-------| | **Storage** | `data.json` flat file | No database. Period. | | **Format** | JSON | Human-editable, version-controllable | | **Location** | `wwwroot/data/data.json` or project root | Configurable via `appsettings.json` | | **Schema** | Strongly-typed C# record classes | Deserialized at startup and on file change | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Unit Testing** | xUnit | 2.7+ | .NET ecosystem standard | | **Blazor Component Testing** | bUnit | 1.25+ | If component tests are desired (optional for this scope) | | **Assertions** | FluentAssertions | 6.12+ | Readable assertion syntax | **Pragmatic note:** For a single-page screenshot dashboard, automated testing has low ROI. Focus testing effort on the JSON deserialization logic (ensuring malformed `data.json` doesn't crash the app). | Component | Recommendation | Notes | |-----------|---------------|-------| | **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Standard .NET development | | **Hot Reload** | `dotnet watch` | Built-in; essential for CSS/layout iteration | | **Build** | `dotnet build` | No custom build pipeline needed | | **Run** | `dotnet run` → `https://localhost:5001` | Kestrel dev server | ---
```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs                     # Minimal hosting setup
│   ├── Models/
│   │   └── DashboardData.cs           # C# record types matching data.json schema
│   ├── Services/
│   │   └── DashboardDataService.cs    # Reads & caches data.json
│   ├── Components/
│   │   ├── App.razor                  # Root component
│   │   ├── Routes.razor               # Router (single route)
│   │   └── Pages/
│   │       ├── Dashboard.razor        # The single page
│   │       └── Dashboard.razor.css    # Scoped CSS (ported from reference)
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── app.css               # Global resets, font stack
│   │   └── data/
│   │       └── data.json             # Dashboard data source
│   └── Properties/
│       └── launchSettings.json
``` **Why single project, no layers:** This is a read-only dashboard with one page, one data source, and no business logic. A multi-project architecture (Domain/Application/Infrastructure) would be absurd over-engineering. One `.csproj`, one `Services/` folder, one `Models/` folder.
```
data.json (file on disk)
    → DashboardDataService.LoadAsync() [System.Text.Json]
    → DashboardData record (immutable model)
    → Dashboard.razor [renders HTML/SVG]
    → Browser [1920×1080 fixed layout]
    → Screenshot → PowerPoint
```
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "timelineMonths": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"],
  "currentMonth": "Apr",
  "milestones": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
        { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
      ]
    }
  ],
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "rows": [
      {
        "category": "shipped",
        "items": {
          "Jan": ["Item A", "Item B"],
          "Feb": ["Item C"],
          "Mar": ["Item D", "Item E"],
          "Apr": ["Item F"]
        }
      },
      {
        "category": "in-progress",
        "items": { ... }
      },
      {
        "category": "carryover",
        "items": { ... }
      },
      {
        "category": "blockers",
        "items": { ... }
      }
    ]
  }
}
```
```csharp
public record DashboardData(
    string Title,
    string Subtitle,
    string? BacklogUrl,
    string[] TimelineMonths,
    string CurrentMonth,
    MilestoneTrack[] Milestones,
    HeatmapData Heatmap
);

public record MilestoneTrack(
    string Id, string Label, string Color,
    MilestoneEvent[] Events
);

public record MilestoneEvent(
    DateOnly Date, string Type, string Label
);

public record HeatmapData(
    string[] Columns, HeatmapRow[] Rows
);

public record HeatmapRow(
    string Category,
    Dictionary<string, string[]> Items
);
``` The timeline SVG in the reference places elements at X positions calculated from dates. In Blazor:
```csharp
private double DateToX(DateOnly date)
{
    var totalDays = (endDate.DayNumber - startDate.DayNumber);
    var elapsed = (date.DayNumber - startDate.DayNumber);
    return (elapsed / (double)totalDays) * svgWidth;
}
``` Render SVG directly in Razor:
```razor
<svg width="1560" height="185" style="overflow:visible">
    @foreach (var track in Data.Milestones)
    {
        var y = GetTrackY(track);
        <line x1="0" y1="@y" x2="1560" y2="@y"
              stroke="@track.Color" stroke-width="3" />
        @foreach (var evt in track.Events)
        {
            var x = DateToX(evt.Date);
            @if (evt.Type == "poc")
            {
                <polygon points="@Diamond(x, y, 11)" fill="#F4B400" />
            }
        }
    }
</svg>
``` This approach gives **full control** over pixel placement, matching the reference design exactly. Port the CSS directly from `OriginalDesignConcept.html` into `Dashboard.razor.css` (Blazor CSS isolation). Key decisions:
- **Fixed dimensions:** Keep `width: 1920px; height: 1080px; overflow: hidden` on the body/root. This is intentional for screenshot fidelity.
- **No CSS framework:** Bootstrap, Tailwind, etc. would fight the fixed layout. Raw CSS is correct here.
- **CSS custom properties** for the color palette (improvement over the reference):
```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-accent: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
}
``` This makes theming trivial for different projects without editing multiple CSS rules.
- **Data-driven rendering:** The reference is hardcoded HTML. The Blazor version renders from `data.json`, so updating the dashboard is a JSON edit, not an HTML edit.
- **CSS custom properties:** Easier color theming per-project.
- **Computed timeline positions:** Milestones auto-position based on dates rather than hardcoded SVG coordinates.
- **Optional: FileSystemWatcher auto-reload:** Edit `data.json` → dashboard updates live without browser refresh (leveraging Blazor Server's SignalR push).
- **Print-friendly:** Add a `@media print` stylesheet that removes browser chrome for cleaner screenshot/PDF capture. ---

### Considerations & Risks

- **None.** This is explicitly out of scope. The dashboard is a local-only tool for generating screenshots. No login, no roles, no cookies, no tokens. If this ever needs to be shared on a network, the simplest option is Windows Authentication via Kestrel's `HttpSys` server (one line in `Program.cs`), but **do not build this until needed**.
- `data.json` may contain project names and milestone descriptions. If these are sensitive, ensure the file is `.gitignore`'d and not committed to source control.
- No PII is expected in this application.
- No encryption needed for local file storage. | Concern | Approach | |---------|----------| | **Runtime** | Local `dotnet run` on developer machine | | **Port** | `https://localhost:5001` (default Kestrel) | | **Browser** | Open Chrome/Edge at 1920×1080 for screenshots | | **Deployment** | None. This runs locally. Optionally `dotnet publish -c Release` for a self-contained exe. | | **Containerization** | Not needed. If desired later, standard `mcr.microsoft.com/dotnet/aspnet:8.0` base image. | **$0.** This runs on the developer's existing machine. No cloud services, no hosting, no licenses beyond existing Visual Studio/VS Code. --- | Risk | Severity | Mitigation | |------|----------|------------| | **Blazor Server overhead for a static page** | Low | The SignalR circuit is unnecessary for a read-only page, but the overhead is negligible (~50KB JS, one WebSocket). The developer experience benefit (hot reload, C# models, scoped CSS) outweighs the cost. | | **SVG rendering differences across browsers** | Low | Target a single browser (Edge/Chrome). The SVG elements used (line, circle, polygon, text) are universally supported. Test the `<feDropShadow>` filter in the target browser. | | **JSON schema drift** | Medium | Define C# record types with `[JsonPropertyName]` attributes. Add a startup validation step that logs warnings for missing/unexpected fields. | | **Screenshot fidelity at non-1920×1080 resolutions** | Medium | Set the browser to exactly 1920×1080 with 100% zoom. Consider adding a `<meta viewport>` tag and documenting the screenshot procedure. | | **FileSystemWatcher reliability on Windows** | Low | Known to occasionally miss events. For a local dev tool, a manual browser refresh is an acceptable fallback. |
- **Blazor Server vs. static HTML:** A plain HTML file with JavaScript `fetch()` for `data.json` would be simpler. Blazor Server is chosen because (a) the team's stack is .NET/C#, (b) strongly-typed models prevent JSON typos, and (c) the .sln structure is required. The trade-off is a heavier runtime for a simple page.
- **No database:** If the dashboard eventually needs historical data (e.g., "show me last month's dashboard"), a SQLite database would be needed. For now, `data.json` is sufficient and dramatically simpler.
- **No responsive design:** The 1920×1080 fixed layout is intentional for screenshot fidelity. This means the page will not look good on mobile or small screens. This is acceptable per requirements.
- **No component library:** Hand-written CSS requires more upfront effort than using MudBlazor/Radzen, but produces cleaner output for screenshot capture with no framework visual artifacts. Not applicable. This is a single-user, local-only, read-only dashboard. If it ever needs to serve multiple users, Blazor Server can handle dozens of concurrent connections on a single machine without architectural changes. ---
- **Screenshot tooling:** Should the project include automated screenshot capture (e.g., Playwright in a `dotnet test` project), or is manual browser screenshot sufficient?
- **Multiple projects:** Will there be multiple `data.json` files for different projects? If so, should the dashboard support a project selector dropdown, or should each project be a separate URL route (`/dashboard/project-a`)?
- **Historical snapshots:** Is there a need to view previous months' dashboards? If yes, the data model needs versioning (e.g., `data-2026-03.json`, `data-2026-04.json`).
- **Data authoring workflow:** Who edits `data.json`? Should there be a simple admin form for editing milestone/heatmap data, or is direct JSON editing acceptable?
- **Color scheme customization:** Should different projects have different color themes, or is the green/blue/amber/red scheme universal?
- **Additional design reference:** The prompt mentions `C:/Pics/ReportingDashboardDesign.png` as a second design reference. This file needs to be reviewed alongside `OriginalDesignConcept.html` to identify any design differences or additions. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Milestone & Progress Reporting Dashboard
**Date:** April 16, 2026
**Stack:** C# .NET 8 · Blazor Server · Local Only · .sln Structure

---

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** that visualizes project milestones, progress status, and monthly heatmaps. The design reference (`OriginalDesignConcept.html`) defines a fixed 1920×1080 layout with a timeline/Gantt SVG header, a four-row heatmap grid (Shipped, In Progress, Carryover, Blockers), and clean typography optimized for PowerPoint screenshot capture.

**Primary Recommendation:** Build this as a minimal Blazor Server application with a single Razor component page that reads from a local `data.json` file. No database is needed. No authentication is needed. The entire application should be ~5-8 files total. Use Blazor's built-in rendering with raw CSS (ported from the HTML reference) and inline SVG for the timeline. Avoid heavy component libraries—the design is simple enough that hand-crafted CSS matching the reference will produce the cleanest, most screenshot-friendly result.

---

## 2. Key Findings

- The design is a **pixel-perfect, fixed-resolution layout** (1920×1080) intended for screenshot capture into PowerPoint. This means responsive design is unnecessary—target the exact viewport size.
- The HTML reference uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) for the heatmap and **Flexbox** for header/layout sections. Both are natively supported in Blazor via standard CSS.
- The timeline section uses **inline SVG** with simple shapes: `<line>`, `<circle>`, `<polygon>` (diamond milestones), `<text>`, and a `<feDropShadow>` filter. This can be rendered directly in Razor markup—no charting library needed.
- The color system is well-defined with **four semantic color tracks**: green (Shipped), blue (In Progress), amber (Carryover), red (Blockers). Each has background, text, and bullet colors.
- A **JSON file** (`data.json`) is the ideal data source given the simplicity requirement. `System.Text.Json` (built into .NET 8) handles deserialization with zero additional dependencies.
- Blazor Server's **SignalR circuit** is overkill for a static dashboard, but the project simplicity means this overhead is negligible and the developer experience (hot reload, C# everywhere) justifies it.
- The total NuGet dependency count should be **zero beyond the default Blazor Server template**. This project needs no ORM, no auth library, no charting library, no CSS framework.

---

## 3. Recommended Technology Stack

### Frontend Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 LTS | Ships with `Microsoft.AspNetCore.App` |
| **CSS Approach** | Hand-written CSS, ported from `OriginalDesignConcept.html` | N/A | Use CSS isolation (`Dashboard.razor.css`) for component-scoped styles |
| **SVG Timeline** | Inline SVG in Razor markup | N/A | No library needed; the reference SVG is ~50 lines |
| **Layout System** | CSS Grid + Flexbox | N/A | Grid for heatmap, Flexbox for header/chrome |
| **Font** | Segoe UI (system font) | N/A | Already specified in design; no web font loading needed |
| **Icons/Shapes** | Inline SVG shapes | N/A | Diamonds (`<polygon>`), circles, lines per the reference |

**Why no component library (MudBlazor, Radzen, etc.):** The design is a single fixed-layout page with no forms, no data tables, no dropdowns, no modals. Adding a component library introduces 500KB+ of CSS/JS that would interfere with the pixel-perfect screenshot requirement. The reference HTML achieves the entire design in ~120 lines of CSS.

**Why no charting library (ApexCharts.Blazor, ChartJs.Blazor):** The timeline is a simple SVG with positioned shapes at calculated X coordinates. A charting library would impose its own styling, axis logic, and responsiveness—all counterproductive for a fixed screenshot layout.

### Backend Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Runtime** | .NET 8.0 LTS | 8.0.x | Long-term support until Nov 2026 |
| **Web Framework** | ASP.NET Core Blazor Server | 8.0.x | Included in SDK |
| **JSON Deserialization** | `System.Text.Json` | Built-in | Use source generators for AOT-friendly deserialization |
| **File Watching** | `FileSystemWatcher` | Built-in | Optional: auto-reload dashboard when `data.json` changes |
| **Configuration** | `IConfiguration` / direct file read | Built-in | Read `data.json` from a known path |

### Data Layer

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **Storage** | `data.json` flat file | No database. Period. |
| **Format** | JSON | Human-editable, version-controllable |
| **Location** | `wwwroot/data/data.json` or project root | Configurable via `appsettings.json` |
| **Schema** | Strongly-typed C# record classes | Deserialized at startup and on file change |

### Testing Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | 2.7+ | .NET ecosystem standard |
| **Blazor Component Testing** | bUnit | 1.25+ | If component tests are desired (optional for this scope) |
| **Assertions** | FluentAssertions | 6.12+ | Readable assertion syntax |

**Pragmatic note:** For a single-page screenshot dashboard, automated testing has low ROI. Focus testing effort on the JSON deserialization logic (ensuring malformed `data.json` doesn't crash the app).

### Infrastructure / Tooling

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Standard .NET development |
| **Hot Reload** | `dotnet watch` | Built-in; essential for CSS/layout iteration |
| **Build** | `dotnet build` | No custom build pipeline needed |
| **Run** | `dotnet run` → `https://localhost:5001` | Kestrel dev server |

---

## 4. Architecture Recommendations

### Overall Architecture: Single-Project Razor Page

```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj
│   ├── Program.cs                     # Minimal hosting setup
│   ├── Models/
│   │   └── DashboardData.cs           # C# record types matching data.json schema
│   ├── Services/
│   │   └── DashboardDataService.cs    # Reads & caches data.json
│   ├── Components/
│   │   ├── App.razor                  # Root component
│   │   ├── Routes.razor               # Router (single route)
│   │   └── Pages/
│   │       ├── Dashboard.razor        # The single page
│   │       └── Dashboard.razor.css    # Scoped CSS (ported from reference)
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── app.css               # Global resets, font stack
│   │   └── data/
│   │       └── data.json             # Dashboard data source
│   └── Properties/
│       └── launchSettings.json
```

**Why single project, no layers:** This is a read-only dashboard with one page, one data source, and no business logic. A multi-project architecture (Domain/Application/Infrastructure) would be absurd over-engineering. One `.csproj`, one `Services/` folder, one `Models/` folder.

### Data Flow

```
data.json (file on disk)
    → DashboardDataService.LoadAsync() [System.Text.Json]
    → DashboardData record (immutable model)
    → Dashboard.razor [renders HTML/SVG]
    → Browser [1920×1080 fixed layout]
    → Screenshot → PowerPoint
```

### Data Model Design (data.json Schema)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "timelineMonths": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"],
  "currentMonth": "Apr",
  "milestones": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
        { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
        { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
      ]
    }
  ],
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "rows": [
      {
        "category": "shipped",
        "items": {
          "Jan": ["Item A", "Item B"],
          "Feb": ["Item C"],
          "Mar": ["Item D", "Item E"],
          "Apr": ["Item F"]
        }
      },
      {
        "category": "in-progress",
        "items": { ... }
      },
      {
        "category": "carryover",
        "items": { ... }
      },
      {
        "category": "blockers",
        "items": { ... }
      }
    ]
  }
}
```

### C# Model Records

```csharp
public record DashboardData(
    string Title,
    string Subtitle,
    string? BacklogUrl,
    string[] TimelineMonths,
    string CurrentMonth,
    MilestoneTrack[] Milestones,
    HeatmapData Heatmap
);

public record MilestoneTrack(
    string Id, string Label, string Color,
    MilestoneEvent[] Events
);

public record MilestoneEvent(
    DateOnly Date, string Type, string Label
);

public record HeatmapData(
    string[] Columns, HeatmapRow[] Rows
);

public record HeatmapRow(
    string Category,
    Dictionary<string, string[]> Items
);
```

### SVG Timeline Rendering Strategy

The timeline SVG in the reference places elements at X positions calculated from dates. In Blazor:

```csharp
private double DateToX(DateOnly date)
{
    var totalDays = (endDate.DayNumber - startDate.DayNumber);
    var elapsed = (date.DayNumber - startDate.DayNumber);
    return (elapsed / (double)totalDays) * svgWidth;
}
```

Render SVG directly in Razor:
```razor
<svg width="1560" height="185" style="overflow:visible">
    @foreach (var track in Data.Milestones)
    {
        var y = GetTrackY(track);
        <line x1="0" y1="@y" x2="1560" y2="@y"
              stroke="@track.Color" stroke-width="3" />
        @foreach (var evt in track.Events)
        {
            var x = DateToX(evt.Date);
            @if (evt.Type == "poc")
            {
                <polygon points="@Diamond(x, y, 11)" fill="#F4B400" />
            }
        }
    }
</svg>
```

This approach gives **full control** over pixel placement, matching the reference design exactly.

### CSS Architecture

Port the CSS directly from `OriginalDesignConcept.html` into `Dashboard.razor.css` (Blazor CSS isolation). Key decisions:

1. **Fixed dimensions:** Keep `width: 1920px; height: 1080px; overflow: hidden` on the body/root. This is intentional for screenshot fidelity.
2. **No CSS framework:** Bootstrap, Tailwind, etc. would fight the fixed layout. Raw CSS is correct here.
3. **CSS custom properties** for the color palette (improvement over the reference):

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-accent: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
}
```

This makes theming trivial for different projects without editing multiple CSS rules.

### Improvements Over the Reference Design

1. **Data-driven rendering:** The reference is hardcoded HTML. The Blazor version renders from `data.json`, so updating the dashboard is a JSON edit, not an HTML edit.
2. **CSS custom properties:** Easier color theming per-project.
3. **Computed timeline positions:** Milestones auto-position based on dates rather than hardcoded SVG coordinates.
4. **Optional: FileSystemWatcher auto-reload:** Edit `data.json` → dashboard updates live without browser refresh (leveraging Blazor Server's SignalR push).
5. **Print-friendly:** Add a `@media print` stylesheet that removes browser chrome for cleaner screenshot/PDF capture.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope. The dashboard is a local-only tool for generating screenshots. No login, no roles, no cookies, no tokens.

If this ever needs to be shared on a network, the simplest option is Windows Authentication via Kestrel's `HttpSys` server (one line in `Program.cs`), but **do not build this until needed**.

### Data Protection

- `data.json` may contain project names and milestone descriptions. If these are sensitive, ensure the file is `.gitignore`'d and not committed to source control.
- No PII is expected in this application.
- No encryption needed for local file storage.

### Hosting & Deployment

| Concern | Approach |
|---------|----------|
| **Runtime** | Local `dotnet run` on developer machine |
| **Port** | `https://localhost:5001` (default Kestrel) |
| **Browser** | Open Chrome/Edge at 1920×1080 for screenshots |
| **Deployment** | None. This runs locally. Optionally `dotnet publish -c Release` for a self-contained exe. |
| **Containerization** | Not needed. If desired later, standard `mcr.microsoft.com/dotnet/aspnet:8.0` base image. |

### Infrastructure Costs

**$0.** This runs on the developer's existing machine. No cloud services, no hosting, no licenses beyond existing Visual Studio/VS Code.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **Blazor Server overhead for a static page** | Low | The SignalR circuit is unnecessary for a read-only page, but the overhead is negligible (~50KB JS, one WebSocket). The developer experience benefit (hot reload, C# models, scoped CSS) outweighs the cost. |
| **SVG rendering differences across browsers** | Low | Target a single browser (Edge/Chrome). The SVG elements used (line, circle, polygon, text) are universally supported. Test the `<feDropShadow>` filter in the target browser. |
| **JSON schema drift** | Medium | Define C# record types with `[JsonPropertyName]` attributes. Add a startup validation step that logs warnings for missing/unexpected fields. |
| **Screenshot fidelity at non-1920×1080 resolutions** | Medium | Set the browser to exactly 1920×1080 with 100% zoom. Consider adding a `<meta viewport>` tag and documenting the screenshot procedure. |
| **FileSystemWatcher reliability on Windows** | Low | Known to occasionally miss events. For a local dev tool, a manual browser refresh is an acceptable fallback. |

### Trade-offs Accepted

1. **Blazor Server vs. static HTML:** A plain HTML file with JavaScript `fetch()` for `data.json` would be simpler. Blazor Server is chosen because (a) the team's stack is .NET/C#, (b) strongly-typed models prevent JSON typos, and (c) the .sln structure is required. The trade-off is a heavier runtime for a simple page.

2. **No database:** If the dashboard eventually needs historical data (e.g., "show me last month's dashboard"), a SQLite database would be needed. For now, `data.json` is sufficient and dramatically simpler.

3. **No responsive design:** The 1920×1080 fixed layout is intentional for screenshot fidelity. This means the page will not look good on mobile or small screens. This is acceptable per requirements.

4. **No component library:** Hand-written CSS requires more upfront effort than using MudBlazor/Radzen, but produces cleaner output for screenshot capture with no framework visual artifacts.

### Scalability Considerations

Not applicable. This is a single-user, local-only, read-only dashboard. If it ever needs to serve multiple users, Blazor Server can handle dozens of concurrent connections on a single machine without architectural changes.

---

## 7. Open Questions

1. **Screenshot tooling:** Should the project include automated screenshot capture (e.g., Playwright in a `dotnet test` project), or is manual browser screenshot sufficient?

2. **Multiple projects:** Will there be multiple `data.json` files for different projects? If so, should the dashboard support a project selector dropdown, or should each project be a separate URL route (`/dashboard/project-a`)?

3. **Historical snapshots:** Is there a need to view previous months' dashboards? If yes, the data model needs versioning (e.g., `data-2026-03.json`, `data-2026-04.json`).

4. **Data authoring workflow:** Who edits `data.json`? Should there be a simple admin form for editing milestone/heatmap data, or is direct JSON editing acceptable?

5. **Color scheme customization:** Should different projects have different color themes, or is the green/blue/amber/red scheme universal?

6. **Additional design reference:** The prompt mentions `C:/Pics/ReportingDashboardDesign.png` as a second design reference. This file needs to be reviewed alongside `OriginalDesignConcept.html` to identify any design differences or additions.

---

## 8. Implementation Recommendations

### Phase 1: Skeleton & Layout (Day 1)

1. `dotnet new blazor --interactivity Server -n ReportingDashboard`
2. Delete default Counter/Weather pages
3. Create `Dashboard.razor` with hardcoded HTML ported from `OriginalDesignConcept.html`
4. Port CSS into `Dashboard.razor.css`
5. **Verify:** Page renders at 1920×1080 matching the reference screenshot

### Phase 2: Data Model & JSON Loading (Day 1-2)

1. Define `DashboardData` record types in `Models/`
2. Create `data.json` with fictional project data
3. Create `DashboardDataService` that reads and deserializes `data.json`
4. Register service in `Program.cs` as singleton
5. **Verify:** `DashboardDataService` correctly deserializes all fields

### Phase 3: Dynamic Rendering (Day 2)

1. Replace hardcoded HTML in `Dashboard.razor` with `@foreach` loops over model data
2. Implement `DateToX()` calculation for SVG timeline positioning
3. Render milestone diamonds, checkpoints, and track lines from data
4. Render heatmap grid cells from data
5. **Verify:** Changing `data.json` values changes the rendered dashboard

### Phase 4: Polish & Improvements (Day 2-3)

1. Add CSS custom properties for color theming
2. Add `FileSystemWatcher` for auto-reload on `data.json` changes (optional)
3. Add `@media print` styles for clean printing
4. Fine-tune spacing, font sizes, and colors to match reference exactly
5. Document the screenshot capture procedure
6. **Verify:** Final screenshot is indistinguishable from the reference design

### Quick Wins

- **Hot reload workflow:** `dotnet watch` gives instant CSS feedback, critical for matching the pixel-perfect reference.
- **CSS custom properties:** Define once, reference everywhere. Changing the shipped color from green to teal is a single-line change.
- **JSON IntelliSense:** If using VS Code, a `data.schema.json` file gives autocomplete when editing `data.json`.

### What NOT to Build

- ❌ Authentication or authorization
- ❌ A database or ORM
- ❌ An admin UI for editing data
- ❌ Responsive/mobile layout
- ❌ API endpoints
- ❌ Logging infrastructure
- ❌ CI/CD pipeline
- ❌ Docker containerization
- ❌ Unit tests (unless JSON deserialization needs validation)

This is a **screenshot generator for PowerPoint decks**. Scope discipline is the most important architectural decision.

---

## Appendix: NuGet Package Summary

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| (none beyond default template) | — | — | — |

The .NET 8 Blazor Server template includes everything needed: Kestrel, Razor components, CSS isolation, `System.Text.Json`, `IConfiguration`, `FileSystemWatcher`. **Zero additional NuGet packages required.**

If testing is desired:
| Package | Version | Purpose |
|---------|---------|---------|
| `xunit` | 2.7.0+ | Test runner |
| `bUnit` | 1.25.0+ | Blazor component testing |
| `FluentAssertions` | 6.12.0+ | Readable assertions |
| `Microsoft.NET.Test.Sdk` | 17.9.0+ | Test host |

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/4233377953d70949ea8bfda5efe6766ee9b29a9e/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
