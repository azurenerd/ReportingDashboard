# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-17 07:10 UTC_

### Summary

This project is a **single-page executive reporting dashboard** that visualizes project milestones, progress, and status in a screenshot-friendly format optimized for PowerPoint decks. The design is driven by the `OriginalDesignConcept.html` reference, featuring a timeline/Gantt section with SVG milestones and a color-coded heatmap grid showing Shipped, In Progress, Carryover, and Blocker items by month. **Primary Recommendation:** Build this as a minimal Blazor Server application with a single Razor component page, reading all data from a local `data.json` file. No database, no authentication, no cloud services. Use pure CSS (Grid + Flexbox) matching the reference design and inline SVG rendered via Blazor's markup capabilities. The entire application should be ~5-8 files total and deployable via `dotnet run`. Given the simplicity requirement (screenshot for PowerPoint), resist over-engineering. A single `.razor` page with a JSON model, a service to load it, and CSS closely mirroring the HTML reference is the optimal approach. ---

### Key Findings

- The reference design (`OriginalDesignConcept.html`) uses a fixed 1920×1080 layout with CSS Grid (`160px repeat(4,1fr)`) and Flexbox — both fully supported in Blazor Server's rendered HTML output.
- SVG is used for the timeline/milestone visualization with diamonds (PoC milestones), circles (checkpoints), colored horizontal lines (workstreams), and a dashed "NOW" line — all achievable with inline SVG in Blazor `.razor` files.
- The color palette is well-defined with semantic meaning: green (shipped), blue (in progress), amber/yellow (carryover), red (blockers) — this maps cleanly to a CSS class strategy.
- A `data.json` file approach eliminates all database complexity; `System.Text.Json` (built into .NET 8) handles deserialization with zero additional dependencies.
- Blazor Server's SignalR connection is unnecessary for this use case (static data, no interactivity), but the overhead is negligible and the developer experience is superior to Blazor WebAssembly for local-only scenarios.
- No external charting library is needed — the SVG in the reference is hand-crafted and simple enough to template directly in Razor markup with data binding.
- The project can be fully functional with **zero NuGet package dependencies** beyond the default Blazor Server template.
- Screenshot fidelity is the primary UX requirement — the page must render identically at 1920×1080 with pixel-perfect alignment to the design reference. --- **Goal:** A working page that matches the reference design with fake data.
- **Scaffold the project** — `dotnet new blazor --interactivity Server -n ReportingDashboard`
- **Define the data model** — Create `DashboardData.cs` with record types matching the JSON structure
- **Create `data.json`** — Fake data for a fictional "Project Phoenix" with 3 workstreams, 6 months of timeline, and heatmap entries across Shipped/InProgress/Carryover/Blockers
- **Build `DashboardDataService`** — Singleton that reads and deserializes `data.json` on startup
- **Port the CSS** — Copy reference CSS into `dashboard.css`, adjust class names to match Blazor conventions
- **Build the three components:**
- `DashboardHeader.razor` — Static header with data-bound title and legend
- `TimelineSection.razor` — SVG with data-driven milestones
- `HeatmapGrid.razor` — CSS Grid with data-driven cells
- **Verify at 1920×1080** — Screenshot and compare side-by-side with the reference
- **JSON Schema** — Add `data.schema.json` for editor validation
- **Auto "NOW" line** — Calculate from system date with JSON override option
- **Current month highlighting** — Dynamically apply `.apr`-style highlight based on report date
- **File watcher** — Optional `FileSystemWatcher` on `data.json` to auto-refresh on change (nice for editing workflow)
- **Print/PDF CSS** — Add `@media print` rules for direct browser printing
- **Automated screenshot** — PowerShell script using Playwright to capture `localhost:5001` at 1920×1080 to PNG
- **Multiple projects** — Support multiple `data-{project}.json` files, selectable via URL parameter
- **Tooltip interactivity** — Hover over heatmap items for details (Blazor Server makes this trivial with `@onmouseover`)
- **ADO integration** — Import from Azure DevOps work item queries to auto-populate `data.json` (separate console tool)
- The CSS from the reference HTML is directly portable — this alone gets you 60% of the visual result.
- `System.Text.Json` source generators in .NET 8 give compile-time JSON deserialization with zero reflection — fast and AOT-friendly.
- `dotnet watch` provides instant feedback during CSS/layout tweaking — critical for matching the reference pixel-by-pixel.
- Blazor's `@foreach` and `@if` make the heatmap grid trivial to data-drive compared to hand-coding HTML cells.
- **Don't add MudBlazor, Radzen, or Syncfusion** — component libraries add hundreds of KB of CSS/JS that will conflict with the reference design's minimal, custom layout.
- **Don't use a charting library** — the SVG is 30 lines of simple shapes. A charting library would make it harder, not easier.
- **Don't add Entity Framework or SQLite** — a JSON file read once at startup is orders of magnitude simpler.
- **Don't add authentication middleware** — it's local-only. Bind to `localhost` in `launchSettings.json` and you're done.
- **Don't make it responsive** — the explicit goal is a fixed 1920×1080 layout for screenshots. Responsive design adds complexity with zero value here.

### Recommended Tools & Technologies

- **Project:** Executive Project Reporting Dashboard **Date:** April 17, 2026 **Stack:** C# .NET 8 · Blazor Server · Local Only · .sln Structure --- | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **UI Framework** | Blazor Server (built-in) | .NET 8.0 | Ships with `Microsoft.AspNetCore.App` — no extra packages | | **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Matches reference design exactly; no CSS framework needed | | **SVG Rendering** | Inline SVG in Razor markup | N/A | Timeline milestones, diamonds, circles, lines — all via `<svg>` elements with Blazor data binding | | **Fonts** | Segoe UI (system font) | N/A | Already specified in reference; available on Windows without hosting | | **Icons** | None needed | N/A | Design uses simple CSS shapes (rotated squares for diamonds, circles) | **Why no CSS framework (Bootstrap, Tailwind, MudBlazor)?** The reference design is a fixed-dimension, pixel-specific layout. Adding a CSS framework would fight the design rather than help it. The reference CSS is ~80 lines — just port it directly. **Why no charting library (Chart.js, Plotly, Radzen Charts)?** The timeline visualization is a simple SVG with lines, circles, diamonds, and text. A charting library would add complexity and reduce control over the exact pixel placement needed to match the reference. Hand-crafted SVG in Razor gives full control. | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Runtime** | .NET 8.0 LTS | 8.0.x | Long-term support until November 2026 | | **Web Host** | ASP.NET Core (Kestrel) | Built-in | Default Blazor Server host; runs locally on `https://localhost:5001` | | **JSON Parsing** | `System.Text.Json` | Built-in (.NET 8) | Native, high-performance, zero-allocation; use source generators for AOT compatibility | | **File Watching** | `FileSystemWatcher` | Built-in (.NET 8) | Optional: auto-reload when `data.json` changes | | **DI Container** | Built-in ASP.NET Core DI | Built-in | Register `DashboardDataService` as singleton | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Storage** | `data.json` flat file | N/A | Single JSON file in project root or `wwwroot/data/` | | **Schema** | C# record types + `System.Text.Json` | Built-in | Strongly-typed models deserialized at startup | | **Database** | **None** | N/A | Explicitly not needed — JSON file is the data store | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Unit Testing** | xUnit | 2.7.x | .NET ecosystem standard; best tooling support | | **Blazor Component Testing** | bUnit | 1.25.x+ | Test Razor components in isolation; verify SVG output, data binding | | **Assertions** | FluentAssertions | 6.12.x | Readable assertion syntax | | **Mocking** | NSubstitute | 5.1.x | Lightweight; preferred over Moq due to Moq's SponsorLink controversy | | Tool | Purpose | Notes | |------|---------|-------| | **`dotnet watch`** | Hot reload during development | Built-in; auto-refreshes on `.razor`/`.css` changes | | **Visual Studio 2022 / VS Code** | IDE | VS 2022 has best Blazor tooling; VS Code works with C# Dev Kit | | **Browser DevTools** | CSS debugging, screenshot verification | Use Chrome's device emulation at 1920×1080 for screenshot fidelity testing | ---
```
ReportingDashboard.sln
│
├── ReportingDashboard/                  # Single Blazor Server project
│   ├── Program.cs                       # Minimal hosting setup
│   ├── Models/
│   │   └── DashboardData.cs             # Record types for JSON schema
│   ├── Services/
│   │   └── DashboardDataService.cs      # Loads & caches data.json
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Dashboard.razor          # Main (and only) page
│   │   ├── Layout/
│   │   │   └── MainLayout.razor         # Minimal layout (no nav)
│   │   ├── DashboardHeader.razor        # Header with title, legend
│   │   ├── TimelineSection.razor        # SVG milestone timeline
│   │   └── HeatmapGrid.razor           # Status heatmap grid
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── dashboard.css            # Ported from reference HTML
│   │   └── data/
│   │       └── data.json                # Dashboard configuration data
│   └── Properties/
│       └── launchSettings.json
│
├── ReportingDashboard.Tests/            # Optional test project
│   └── ...
│
└── ReportingDashboard.sln
```
```
data.json → DashboardDataService (singleton, loaded once at startup)
         → Injected into Dashboard.razor via @inject
         → Dashboard.razor distributes data to child components via [Parameter]
         → Components render HTML/SVG matching the reference design
``` **Three Blazor components** map directly to the three visual sections of the reference design:
- **`DashboardHeader.razor`** — Title, subtitle (workstream + date), legend icons
- **`TimelineSection.razor`** — SVG-based milestone visualization with:
- Workstream labels (left column, 230px)
- SVG canvas with month gridlines, milestone markers, "NOW" line
- Data-driven: iterate over milestones/workstreams from JSON
- **`HeatmapGrid.razor`** — CSS Grid heatmap with:
- Column headers (months), row headers (Shipped/In Progress/Carryover/Blockers)
- Data cells with bullet-pointed items, color-coded by category
- Current month highlighted with darker background
```csharp
public record DashboardData(
    string Title,
    string Subtitle,
    string BacklogUrl,
    DateOnly ReportDate,
    List<Workstream> Workstreams,
    List<MonthColumn> Months,
    HeatmapData Heatmap
);

public record Workstream(
    string Id,
    string Label,
    string Description,
    string Color,
    List<Milestone> Milestones
);

public record Milestone(
    DateOnly Date,
    string Label,
    MilestoneType Type  // PoC, Production, Checkpoint
);

public record MonthColumn(
    string Label,
    DateOnly StartDate,
    bool IsCurrent
);

public record HeatmapData(
    List<HeatmapRow> Rows
);

public record HeatmapRow(
    string Category,  // "Shipped", "InProgress", "Carryover", "Blockers"
    List<HeatmapCell> Cells
);

public record HeatmapCell(
    string Month,
    List<string> Items
);
``` The reference SVG is 1560×185px with month gridlines at 260px intervals. Replicate this in Blazor:
```razor
<svg width="1560" height="185" style="overflow:visible">
    @foreach (var month in Data.Months)
    {
        var x = CalculateMonthX(month);
        <line x1="@x" y1="0" x2="@x" y2="185" stroke="#bbb" stroke-opacity="0.4"/>
        <text x="@(x+5)" y="14" fill="#666" font-size="11">@month.Label</text>
    }
    @* NOW line *@
    <line x1="@nowX" y1="0" x2="@nowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
    @* Milestones rendered per workstream *@
</svg>
``` Key calculation: Map dates to X coordinates proportionally across the 1560px width based on the date range. Port the reference CSS nearly verbatim. Key patterns:
- **Fixed dimensions:** `body { width: 1920px; height: 1080px; overflow: hidden; }` — essential for screenshot consistency
- **CSS Grid for heatmap:** `grid-template-columns: 160px repeat(4, 1fr);`
- **Flexbox for header and timeline area**
- **Semantic color classes:** `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` with corresponding header and bullet colors
- **Current month highlighting:** `.apr` class (or dynamically applied based on `IsCurrent`) Place CSS in `wwwroot/css/dashboard.css` and reference in `MainLayout.razor` or `App.razor`. ---

### Considerations & Risks

- **None.** This is explicitly a local-only, no-auth application. The Blazor Server template's default authentication scaffolding should be **removed** to keep the project minimal. In `Program.cs`, the setup should be as simple as:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```
- **No sensitive data** — the dashboard displays project status information, not PII or secrets.
- The `data.json` file lives on the local filesystem with standard OS-level file permissions.
- If the JSON ever contains sensitive project names, simply don't commit it to source control (add to `.gitignore`). | Aspect | Recommendation | |--------|---------------| | **Runtime** | Local Kestrel server via `dotnet run` | | **Port** | `https://localhost:5001` (default) or configure in `launchSettings.json` | | **Deployment** | `dotnet publish -c Release` → copy to target machine → run executable | | **Containerization** | Not needed for local-only use; optionally a simple Dockerfile if sharing across machines | | **Infrastructure Cost** | **$0** — runs on developer's local machine | Since the primary output is PowerPoint screenshots:
- Run `dotnet run`
- Open `https://localhost:5001` in Chrome
- Set browser window to 1920×1080 (use DevTools device emulation or full-screen on a 1080p monitor)
- Take screenshot (Win+Shift+S or Chrome DevTools capture)
- Paste into PowerPoint Optional enhancement: Add a `dotnet tool` or PowerShell script that uses Playwright to auto-capture the screenshot headlessly. --- | Risk | Impact | Mitigation | |------|--------|------------| | **Blazor Server requires SignalR connection** | Minor overhead for a static page | Negligible; no real-time features used. Consider `@rendermode InteractiveServer` only on interactive components if any. | | **Fixed 1920×1080 dimensions** | Won't render well on smaller screens | Acceptable — the explicit goal is screenshot fidelity, not responsive design. | | **No database** | Can't query historical data | By design — each `data.json` represents a point-in-time snapshot. Archive old JSONs manually if history needed. | | Risk | Impact | Mitigation | |------|--------|------------| | **Manual data.json editing** | Prone to typos, invalid JSON | Add JSON schema validation; provide a `data.schema.json` file. VS Code gives inline validation with `$schema` reference. | | **SVG coordinate math** | Timeline positioning bugs if date ranges change | Write a helper method `DateToX(DateOnly date, DateOnly rangeStart, DateOnly rangeEnd, double svgWidth)` and unit test it. | | **.NET 8 LTS end-of-life** | November 2026 | Migrate to .NET 10 LTS when available (November 2025 release). Blazor Server API is stable across versions. | | Decision | Trade-off | Justification | |----------|-----------|---------------| | **No charting library** | Must hand-code SVG positioning | Full pixel control; reference design's SVG is simple enough; avoids 500KB+ JS dependency | | **No CSS framework** | Must write all CSS manually | Reference CSS is <100 lines; a framework would conflict with the fixed layout | | **No database** | No querying, no history | Simplicity is the #1 requirement; a JSON file is the simplest possible data source | | **No authentication** | Anyone on the network could access it | Local-only; bind to `localhost` only in `launchSettings.json` | | **Blazor Server over static HTML** | Requires `dotnet` runtime | Enables future interactivity (tooltips, filters) if needed; structured component model; strong typing of data | ---
- **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always show a fixed window?
- **How many workstreams in the timeline?** The reference shows 3 (M1, M2, M3). If more are needed, the SVG height and vertical spacing must scale. Define a max in the data model?
- **Should the "NOW" line position auto-calculate from system date or be specified in `data.json`?** Recommendation: Auto-calculate from `DateTime.Now`, but allow override in JSON.
- **Will the `data.json` be hand-edited or generated by another tool?** This affects whether we need a JSON schema, a companion editor UI, or an import from ADO/Jira.
- **Are there additional visual sections beyond the reference design?** The design shows header + timeline + heatmap. Should we plan for a summary stats row, risk callouts, or a notes section?
- **What is the archival strategy?** Should old `data.json` files be kept alongside the project (e.g., `data-2026-03.json`, `data-2026-04.json`) for historical comparison?
- **Should the page support printing to PDF directly?** If so, add a `@media print` CSS block with the same fixed dimensions. This could replace the screenshot workflow entirely. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Project Reporting Dashboard
**Date:** April 17, 2026
**Stack:** C# .NET 8 · Blazor Server · Local Only · .sln Structure

---

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** that visualizes project milestones, progress, and status in a screenshot-friendly format optimized for PowerPoint decks. The design is driven by the `OriginalDesignConcept.html` reference, featuring a timeline/Gantt section with SVG milestones and a color-coded heatmap grid showing Shipped, In Progress, Carryover, and Blocker items by month.

**Primary Recommendation:** Build this as a minimal Blazor Server application with a single Razor component page, reading all data from a local `data.json` file. No database, no authentication, no cloud services. Use pure CSS (Grid + Flexbox) matching the reference design and inline SVG rendered via Blazor's markup capabilities. The entire application should be ~5-8 files total and deployable via `dotnet run`.

Given the simplicity requirement (screenshot for PowerPoint), resist over-engineering. A single `.razor` page with a JSON model, a service to load it, and CSS closely mirroring the HTML reference is the optimal approach.

---

## 2. Key Findings

- The reference design (`OriginalDesignConcept.html`) uses a fixed 1920×1080 layout with CSS Grid (`160px repeat(4,1fr)`) and Flexbox — both fully supported in Blazor Server's rendered HTML output.
- SVG is used for the timeline/milestone visualization with diamonds (PoC milestones), circles (checkpoints), colored horizontal lines (workstreams), and a dashed "NOW" line — all achievable with inline SVG in Blazor `.razor` files.
- The color palette is well-defined with semantic meaning: green (shipped), blue (in progress), amber/yellow (carryover), red (blockers) — this maps cleanly to a CSS class strategy.
- A `data.json` file approach eliminates all database complexity; `System.Text.Json` (built into .NET 8) handles deserialization with zero additional dependencies.
- Blazor Server's SignalR connection is unnecessary for this use case (static data, no interactivity), but the overhead is negligible and the developer experience is superior to Blazor WebAssembly for local-only scenarios.
- No external charting library is needed — the SVG in the reference is hand-crafted and simple enough to template directly in Razor markup with data binding.
- The project can be fully functional with **zero NuGet package dependencies** beyond the default Blazor Server template.
- Screenshot fidelity is the primary UX requirement — the page must render identically at 1920×1080 with pixel-perfect alignment to the design reference.

---

## 3. Recommended Technology Stack

### Frontend (UI Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 | Ships with `Microsoft.AspNetCore.App` — no extra packages |
| **CSS Layout** | Pure CSS Grid + Flexbox | N/A | Matches reference design exactly; no CSS framework needed |
| **SVG Rendering** | Inline SVG in Razor markup | N/A | Timeline milestones, diamonds, circles, lines — all via `<svg>` elements with Blazor data binding |
| **Fonts** | Segoe UI (system font) | N/A | Already specified in reference; available on Windows without hosting |
| **Icons** | None needed | N/A | Design uses simple CSS shapes (rotated squares for diamonds, circles) |

**Why no CSS framework (Bootstrap, Tailwind, MudBlazor)?** The reference design is a fixed-dimension, pixel-specific layout. Adding a CSS framework would fight the design rather than help it. The reference CSS is ~80 lines — just port it directly.

**Why no charting library (Chart.js, Plotly, Radzen Charts)?** The timeline visualization is a simple SVG with lines, circles, diamonds, and text. A charting library would add complexity and reduce control over the exact pixel placement needed to match the reference. Hand-crafted SVG in Razor gives full control.

### Backend (Application Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Runtime** | .NET 8.0 LTS | 8.0.x | Long-term support until November 2026 |
| **Web Host** | ASP.NET Core (Kestrel) | Built-in | Default Blazor Server host; runs locally on `https://localhost:5001` |
| **JSON Parsing** | `System.Text.Json` | Built-in (.NET 8) | Native, high-performance, zero-allocation; use source generators for AOT compatibility |
| **File Watching** | `FileSystemWatcher` | Built-in (.NET 8) | Optional: auto-reload when `data.json` changes |
| **DI Container** | Built-in ASP.NET Core DI | Built-in | Register `DashboardDataService` as singleton |

### Data Layer

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Storage** | `data.json` flat file | N/A | Single JSON file in project root or `wwwroot/data/` |
| **Schema** | C# record types + `System.Text.Json` | Built-in | Strongly-typed models deserialized at startup |
| **Database** | **None** | N/A | Explicitly not needed — JSON file is the data store |

### Testing

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | 2.7.x | .NET ecosystem standard; best tooling support |
| **Blazor Component Testing** | bUnit | 1.25.x+ | Test Razor components in isolation; verify SVG output, data binding |
| **Assertions** | FluentAssertions | 6.12.x | Readable assertion syntax |
| **Mocking** | NSubstitute | 5.1.x | Lightweight; preferred over Moq due to Moq's SponsorLink controversy |

### Development Tooling

| Tool | Purpose | Notes |
|------|---------|-------|
| **`dotnet watch`** | Hot reload during development | Built-in; auto-refreshes on `.razor`/`.css` changes |
| **Visual Studio 2022 / VS Code** | IDE | VS 2022 has best Blazor tooling; VS Code works with C# Dev Kit |
| **Browser DevTools** | CSS debugging, screenshot verification | Use Chrome's device emulation at 1920×1080 for screenshot fidelity testing |

---

## 4. Architecture Recommendations

### Overall Architecture: Minimal Single-Page App

```
ReportingDashboard.sln
│
├── ReportingDashboard/                  # Single Blazor Server project
│   ├── Program.cs                       # Minimal hosting setup
│   ├── Models/
│   │   └── DashboardData.cs             # Record types for JSON schema
│   ├── Services/
│   │   └── DashboardDataService.cs      # Loads & caches data.json
│   ├── Components/
│   │   ├── Pages/
│   │   │   └── Dashboard.razor          # Main (and only) page
│   │   ├── Layout/
│   │   │   └── MainLayout.razor         # Minimal layout (no nav)
│   │   ├── DashboardHeader.razor        # Header with title, legend
│   │   ├── TimelineSection.razor        # SVG milestone timeline
│   │   └── HeatmapGrid.razor           # Status heatmap grid
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── dashboard.css            # Ported from reference HTML
│   │   └── data/
│   │       └── data.json                # Dashboard configuration data
│   └── Properties/
│       └── launchSettings.json
│
├── ReportingDashboard.Tests/            # Optional test project
│   └── ...
│
└── ReportingDashboard.sln
```

### Data Flow

```
data.json → DashboardDataService (singleton, loaded once at startup)
         → Injected into Dashboard.razor via @inject
         → Dashboard.razor distributes data to child components via [Parameter]
         → Components render HTML/SVG matching the reference design
```

### Component Architecture

**Three Blazor components** map directly to the three visual sections of the reference design:

1. **`DashboardHeader.razor`** — Title, subtitle (workstream + date), legend icons
2. **`TimelineSection.razor`** — SVG-based milestone visualization with:
   - Workstream labels (left column, 230px)
   - SVG canvas with month gridlines, milestone markers, "NOW" line
   - Data-driven: iterate over milestones/workstreams from JSON
3. **`HeatmapGrid.razor`** — CSS Grid heatmap with:
   - Column headers (months), row headers (Shipped/In Progress/Carryover/Blockers)
   - Data cells with bullet-pointed items, color-coded by category
   - Current month highlighted with darker background

### Data Model (C# Records)

```csharp
public record DashboardData(
    string Title,
    string Subtitle,
    string BacklogUrl,
    DateOnly ReportDate,
    List<Workstream> Workstreams,
    List<MonthColumn> Months,
    HeatmapData Heatmap
);

public record Workstream(
    string Id,
    string Label,
    string Description,
    string Color,
    List<Milestone> Milestones
);

public record Milestone(
    DateOnly Date,
    string Label,
    MilestoneType Type  // PoC, Production, Checkpoint
);

public record MonthColumn(
    string Label,
    DateOnly StartDate,
    bool IsCurrent
);

public record HeatmapData(
    List<HeatmapRow> Rows
);

public record HeatmapRow(
    string Category,  // "Shipped", "InProgress", "Carryover", "Blockers"
    List<HeatmapCell> Cells
);

public record HeatmapCell(
    string Month,
    List<string> Items
);
```

### SVG Timeline Rendering Strategy

The reference SVG is 1560×185px with month gridlines at 260px intervals. Replicate this in Blazor:

```razor
<svg width="1560" height="185" style="overflow:visible">
    @foreach (var month in Data.Months)
    {
        var x = CalculateMonthX(month);
        <line x1="@x" y1="0" x2="@x" y2="185" stroke="#bbb" stroke-opacity="0.4"/>
        <text x="@(x+5)" y="14" fill="#666" font-size="11">@month.Label</text>
    }
    @* NOW line *@
    <line x1="@nowX" y1="0" x2="@nowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
    @* Milestones rendered per workstream *@
</svg>
```

Key calculation: Map dates to X coordinates proportionally across the 1560px width based on the date range.

### CSS Strategy

Port the reference CSS nearly verbatim. Key patterns:

- **Fixed dimensions:** `body { width: 1920px; height: 1080px; overflow: hidden; }` — essential for screenshot consistency
- **CSS Grid for heatmap:** `grid-template-columns: 160px repeat(4, 1fr);`
- **Flexbox for header and timeline area**
- **Semantic color classes:** `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` with corresponding header and bullet colors
- **Current month highlighting:** `.apr` class (or dynamically applied based on `IsCurrent`)

Place CSS in `wwwroot/css/dashboard.css` and reference in `MainLayout.razor` or `App.razor`.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly a local-only, no-auth application. The Blazor Server template's default authentication scaffolding should be **removed** to keep the project minimal.

In `Program.cs`, the setup should be as simple as:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

### Data Protection

- **No sensitive data** — the dashboard displays project status information, not PII or secrets.
- The `data.json` file lives on the local filesystem with standard OS-level file permissions.
- If the JSON ever contains sensitive project names, simply don't commit it to source control (add to `.gitignore`).

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Local Kestrel server via `dotnet run` |
| **Port** | `https://localhost:5001` (default) or configure in `launchSettings.json` |
| **Deployment** | `dotnet publish -c Release` → copy to target machine → run executable |
| **Containerization** | Not needed for local-only use; optionally a simple Dockerfile if sharing across machines |
| **Infrastructure Cost** | **$0** — runs on developer's local machine |

### Screenshot Workflow

Since the primary output is PowerPoint screenshots:
1. Run `dotnet run`
2. Open `https://localhost:5001` in Chrome
3. Set browser window to 1920×1080 (use DevTools device emulation or full-screen on a 1080p monitor)
4. Take screenshot (Win+Shift+S or Chrome DevTools capture)
5. Paste into PowerPoint

Optional enhancement: Add a `dotnet tool` or PowerShell script that uses Playwright to auto-capture the screenshot headlessly.

---

## 6. Risks & Trade-offs

### Low-Risk Items (Acceptable)

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Blazor Server requires SignalR connection** | Minor overhead for a static page | Negligible; no real-time features used. Consider `@rendermode InteractiveServer` only on interactive components if any. |
| **Fixed 1920×1080 dimensions** | Won't render well on smaller screens | Acceptable — the explicit goal is screenshot fidelity, not responsive design. |
| **No database** | Can't query historical data | By design — each `data.json` represents a point-in-time snapshot. Archive old JSONs manually if history needed. |

### Medium-Risk Items (Monitor)

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Manual data.json editing** | Prone to typos, invalid JSON | Add JSON schema validation; provide a `data.schema.json` file. VS Code gives inline validation with `$schema` reference. |
| **SVG coordinate math** | Timeline positioning bugs if date ranges change | Write a helper method `DateToX(DateOnly date, DateOnly rangeStart, DateOnly rangeEnd, double svgWidth)` and unit test it. |
| **.NET 8 LTS end-of-life** | November 2026 | Migrate to .NET 10 LTS when available (November 2025 release). Blazor Server API is stable across versions. |

### Trade-offs Made

| Decision | Trade-off | Justification |
|----------|-----------|---------------|
| **No charting library** | Must hand-code SVG positioning | Full pixel control; reference design's SVG is simple enough; avoids 500KB+ JS dependency |
| **No CSS framework** | Must write all CSS manually | Reference CSS is <100 lines; a framework would conflict with the fixed layout |
| **No database** | No querying, no history | Simplicity is the #1 requirement; a JSON file is the simplest possible data source |
| **No authentication** | Anyone on the network could access it | Local-only; bind to `localhost` only in `launchSettings.json` |
| **Blazor Server over static HTML** | Requires `dotnet` runtime | Enables future interactivity (tooltips, filters) if needed; structured component model; strong typing of data |

---

## 7. Open Questions

1. **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always show a fixed window?

2. **How many workstreams in the timeline?** The reference shows 3 (M1, M2, M3). If more are needed, the SVG height and vertical spacing must scale. Define a max in the data model?

3. **Should the "NOW" line position auto-calculate from system date or be specified in `data.json`?** Recommendation: Auto-calculate from `DateTime.Now`, but allow override in JSON.

4. **Will the `data.json` be hand-edited or generated by another tool?** This affects whether we need a JSON schema, a companion editor UI, or an import from ADO/Jira.

5. **Are there additional visual sections beyond the reference design?** The design shows header + timeline + heatmap. Should we plan for a summary stats row, risk callouts, or a notes section?

6. **What is the archival strategy?** Should old `data.json` files be kept alongside the project (e.g., `data-2026-03.json`, `data-2026-04.json`) for historical comparison?

7. **Should the page support printing to PDF directly?** If so, add a `@media print` CSS block with the same fixed dimensions. This could replace the screenshot workflow entirely.

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** A working page that matches the reference design with fake data.

1. **Scaffold the project** — `dotnet new blazor --interactivity Server -n ReportingDashboard`
2. **Define the data model** — Create `DashboardData.cs` with record types matching the JSON structure
3. **Create `data.json`** — Fake data for a fictional "Project Phoenix" with 3 workstreams, 6 months of timeline, and heatmap entries across Shipped/InProgress/Carryover/Blockers
4. **Build `DashboardDataService`** — Singleton that reads and deserializes `data.json` on startup
5. **Port the CSS** — Copy reference CSS into `dashboard.css`, adjust class names to match Blazor conventions
6. **Build the three components:**
   - `DashboardHeader.razor` — Static header with data-bound title and legend
   - `TimelineSection.razor` — SVG with data-driven milestones
   - `HeatmapGrid.razor` — CSS Grid with data-driven cells
7. **Verify at 1920×1080** — Screenshot and compare side-by-side with the reference

### Phase 2: Polish (1 day)

1. **JSON Schema** — Add `data.schema.json` for editor validation
2. **Auto "NOW" line** — Calculate from system date with JSON override option
3. **Current month highlighting** — Dynamically apply `.apr`-style highlight based on report date
4. **File watcher** — Optional `FileSystemWatcher` on `data.json` to auto-refresh on change (nice for editing workflow)
5. **Print/PDF CSS** — Add `@media print` rules for direct browser printing

### Phase 3: Enhancements (Optional, future)

1. **Automated screenshot** — PowerShell script using Playwright to capture `localhost:5001` at 1920×1080 to PNG
2. **Multiple projects** — Support multiple `data-{project}.json` files, selectable via URL parameter
3. **Tooltip interactivity** — Hover over heatmap items for details (Blazor Server makes this trivial with `@onmouseover`)
4. **ADO integration** — Import from Azure DevOps work item queries to auto-populate `data.json` (separate console tool)

### Quick Wins

- The CSS from the reference HTML is directly portable — this alone gets you 60% of the visual result.
- `System.Text.Json` source generators in .NET 8 give compile-time JSON deserialization with zero reflection — fast and AOT-friendly.
- `dotnet watch` provides instant feedback during CSS/layout tweaking — critical for matching the reference pixel-by-pixel.
- Blazor's `@foreach` and `@if` make the heatmap grid trivial to data-drive compared to hand-coding HTML cells.

### What NOT to Do

- **Don't add MudBlazor, Radzen, or Syncfusion** — component libraries add hundreds of KB of CSS/JS that will conflict with the reference design's minimal, custom layout.
- **Don't use a charting library** — the SVG is 30 lines of simple shapes. A charting library would make it harder, not easier.
- **Don't add Entity Framework or SQLite** — a JSON file read once at startup is orders of magnitude simpler.
- **Don't add authentication middleware** — it's local-only. Bind to `localhost` in `launchSettings.json` and you're done.
- **Don't make it responsive** — the explicit goal is a fixed 1920×1080 layout for screenshots. Responsive design adds complexity with zero value here.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/b46427f60e24137d2639c947d81f73ce890e1862/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
