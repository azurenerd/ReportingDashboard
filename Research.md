# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-15 10:09 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard renders a project milestone timeline, status heatmap (Shipped / In Progress / Carryover / Blockers), and is designed to be screenshot-friendly for PowerPoint decks at 1920×1080 resolution. **Primary recommendation:** Keep the architecture radically simple. Use a single Blazor Server project with one Razor page, read data from a local `data.json` file using `System.Text.Json`, render the timeline via inline SVG generated in Razor, and style everything with a single CSS file that mirrors the original HTML design. No database, no authentication, no external services. The entire app should be runnable with `dotnet run` and viewable at `localhost:5000`. This is fundamentally a **data-driven static rendering problem** — Blazor Server is slightly over-engineered for it, but it provides a clean .NET developer experience, hot reload during development, and easy extensibility if the dashboard grows later. --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.App` (framework ref) | 8.0 | Blazor Server runtime | Yes (implicit) | | `System.Text.Json` | 8.0 (built-in) | JSON deserialization | Yes (built-in) | | `xunit` | 2.7.0 | Unit testing | Test project only | | `xunit.runner.visualstudio` | 2.5.7 | VS Test integration | Test project only | | `bunit` | 1.25.3 | Blazor component testing | Test project only | | `FluentAssertions` | 6.12.0 | Readable assertions | Test project only |

### Key Findings

- The original `OriginalDesignConcept.html` is a self-contained static HTML/CSS/SVG page at exactly 1920×1080. The Blazor implementation must preserve this pixel-perfect layout for screenshot capture.
- The design uses **CSS Grid** (5-column: `160px repeat(4,1fr)`) for the heatmap and **Flexbox** for header/layout. Both are fully supported in modern browsers and require no JavaScript frameworks.
- The timeline section uses **inline SVG** with basic shapes: `<line>`, `<circle>`, `<polygon>` (diamond milestones), `<text>`, and a `<feDropShadow>` filter. This is trivially reproducible in Razor markup with `@foreach` loops over milestone data.
- **No charting library is needed.** The SVG is simple enough to generate directly from Razor templates. Adding a charting library (e.g., Chart.js via JS interop) would add complexity with zero benefit for this use case.
- A flat `data.json` file is the ideal data source — no database overhead, easy to edit, version-controllable, and trivially deserialized with `System.Text.Json`.
- Blazor Server's SignalR circuit is unnecessary for this read-only dashboard but does no harm. The app will work fine as a local tool.
- The color palette is well-defined in the HTML reference (greens for shipped, blues for in-progress, ambers for carryover, reds for blockers). These should be extracted into CSS custom properties for maintainability.
- The design is **not responsive** — it targets a fixed 1920×1080 viewport. This is intentional for screenshot fidelity and should be preserved, not "fixed." ---
- Create solution structure: `dotnet new sln`, `dotnet new blazorserver`
- Define `DashboardData` C# records matching the JSON schema
- Create `data.json` with fictional project data (e.g., "Project Phoenix")
- Implement `DashboardDataService` to read and deserialize `data.json`
- Port the original HTML `<style>` block to `dashboard.css`
- Build `Dashboard.razor` with inline header, timeline SVG, and heatmap grid
- Verify 1920×1080 screenshot matches original design quality
- Extract `Header.razor`, `TimelineSvg.razor`, `HeatmapGrid.razor`, `HeatmapRow.razor`
- Make month count and timeline span data-driven from `data.json`
- Auto-calculate "NOW" line position from system date
- Add CSS custom properties for the color palette
- Add xUnit + bUnit tests for `DashboardDataService` and key components
- Create a sample `data.json` with edge cases (empty months, long item names, many items per cell)
- Test screenshot capture in Edge and Chrome at 1920×1080
- Document the screenshot workflow in a `README.md`
- Publish as self-contained single-file executable for distribution
- **Copy the CSS verbatim** from `OriginalDesignConcept.html` into `dashboard.css`. This gets 90% of the visual design correct on the first render.
- **Start with hardcoded Razor markup** that mirrors the original HTML structure, then progressively replace static values with `@model.Property` bindings. This ensures the layout is correct before introducing data binding complexity.
- **Use `dotnet watch run`** during development for instant feedback on `.razor` and `.css` changes.
- **SVG timeline positioning**: Before building the full data model, prototype the timeline SVG with 3 hardcoded milestones to validate that computed X positions render correctly. The SVG coordinate math is the trickiest part of this project.
- **CSS Grid heatmap with variable content**: Test what happens when a cell has 0 items, 1 item, or 8+ items. The original design uses `overflow: hidden` on cells — verify this truncation looks acceptable or if scrolling/truncation needs adjustment.
```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Engineering Platform • Core Infrastructure • April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "currentDate": "2026-04-15",
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonthIndex": 3,
  "milestones": [
    {
      "label": "M1",
      "sublabel": "API Gateway & Auth",
      "color": "#0078D4",
      "markers": [
        { "date": "2026-01-15", "label": "Jan 15", "type": "checkpoint" },
        { "date": "2026-03-20", "label": "Mar 20 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "May Prod", "type": "production" }
      ]
    }
  ],
  "shipped": {
    "label": "SHIPPED",
    "itemsByMonth": [
      ["Auth Service v2", "Rate Limiting"],
      ["SDK v3.1", "Logging Pipeline"],
      ["Cache Layer", "Health Checks"],
      ["Config Service"]
    ]
  },
  "inProgress": {
    "label": "IN PROGRESS",
    "itemsByMonth": [[], [], ["API Gateway"], ["Gateway Beta", "Load Testing"]]
  },
  "carryover": {
    "label": "CARRYOVER",
    "itemsByMonth": [[], [], [], ["Schema Migration"]]
  },
  "blockers": {
    "label": "BLOCKERS",
    "itemsByMonth": [[], [], [], ["Vendor SDK Delay"]]
  }
}
```

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Framework** | Blazor Server (.NET 8) | `net8.0` | Ships with `Microsoft.AspNetCore.App` — no extra NuGet packages needed | | **CSS Layout** | Vanilla CSS (Grid + Flexbox) | N/A | Matches original HTML design exactly; no CSS framework needed | | **SVG Rendering** | Inline SVG via Razor | N/A | Generate `<svg>` elements directly in `.razor` files with `@foreach` over milestones | | **CSS Architecture** | Single `dashboard.css` file with CSS Custom Properties | N/A | Extract color tokens as `--color-shipped`, `--color-progress`, etc. | | **Font** | Segoe UI (system font) | N/A | Already available on Windows; no web font loading needed | **Why no CSS framework (Bootstrap, Tailwind, MudBlazor)?** The design is a fixed-viewport, single-page layout with ~100 lines of CSS. Adding a framework adds bundle size, class name conflicts, and overrides to fight. The original HTML proves vanilla CSS is sufficient. **Why no charting library?** The timeline is ~20 SVG elements. Libraries like `Radzen.Blazor` or `ApexCharts.Blazor` are designed for interactive charts with tooltips, zoom, and animation — none of which matter for a screenshot-oriented dashboard. Hand-crafted SVG in Razor gives pixel-perfect control. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Zero dependencies; use `JsonSerializer.Deserialize<T>()` with source generators for AOT compatibility | | **File I/O** | `System.IO.File.ReadAllTextAsync` | Built into .NET 8 | Read `data.json` from disk on page load | | **Data Models** | C# records | Built into .NET 8 | Immutable POCOs: `DashboardData`, `Milestone`, `StatusItem` | | **Configuration** | `appsettings.json` for file path | Built into .NET 8 | Store `data.json` path in config so it's overridable | **Why no database?** The data source is a single JSON file with ~50-100 items. SQLite, LiteDB, or EF Core would add migration complexity, connection management, and schema overhead for zero benefit. If requirements grow, SQLite can be added later without architectural changes. | Aspect | Decision | Rationale | |--------|----------|-----------| | **Primary store** | `data.json` flat file | Simplest possible approach; human-editable; version-controllable | | **Format** | JSON with `System.Text.Json` conventions | `camelCase` property naming; strongly-typed C# records | | **Location** | Project root or configurable path via `appsettings.json` | Default to `./data.json` relative to content root | | **Caching** | Read file once at startup, re-read on page refresh | Use `IMemoryCache` only if file reads become a bottleneck (they won't) | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Testing** | xUnit | `2.7.0+` | .NET ecosystem standard; first-class `dotnet test` support | | **Assertions** | FluentAssertions | `6.12.0` | Readable assertion syntax: `data.Milestones.Should().HaveCount(3)` | | **Blazor Component Testing** | bUnit | `1.25.3+` | Test Razor components in isolation without a browser; MIT license | | **Snapshot Testing** | Verify | `23.5.2+` | Optional — snapshot test rendered HTML to catch visual regressions | | Tool | Purpose | Notes | |------|---------|-------| | **`dotnet run`** | Local execution | No Docker, no IIS, no reverse proxy needed | | **`dotnet watch`** | Hot reload during development | Blazor Server supports hot reload for .razor and .css files | | **Kestrel** | Web server | Built into .NET 8; binds to `localhost:5000` by default | | **Screenshot capture** | Browser DevTools or `Ctrl+Shift+S` in Edge/Chrome | Set viewport to 1920×1080 Device Mode for consistent captures | --- This is deliberately the simplest viable architecture. No layers, no services, no abstractions beyond what Razor naturally provides.
```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj        # net8.0, no extra NuGet refs
│   ├── Program.cs                        # Minimal hosting: builder.Build().Run()
│   ├── appsettings.json                  # DataFilePath: "./data.json"
│   ├── data.json                         # Dashboard data (milestones, status items)
│   ├── Models/
│   │   └── DashboardData.cs             # C# records for JSON shape
│   ├── Services/
│   │   └── DashboardDataService.cs      # Reads & deserializes data.json
│   ├── Components/
│   │   ├── App.razor                    # Root component
│   │   ├── Pages/
│   │   │   └── Dashboard.razor          # Single page — the entire UI
│   │   ├── Layout/
│   │   │   └── MainLayout.razor         # Minimal layout wrapper
│   │   └── Shared/
│   │       ├── TimelineSvg.razor        # SVG timeline component
│   │       ├── HeatmapGrid.razor        # CSS Grid heatmap component
│   │       ├── HeatmapRow.razor         # Single status row (Shipped/InProgress/etc.)
│   │       └── Header.razor             # Title, subtitle, legend
│   └── wwwroot/
│       └── css/
│           └── dashboard.css            # All styles — mirrors original HTML
├── ReportingDashboard.Tests/
│   ├── ReportingDashboard.Tests.csproj
│   └── DashboardDataServiceTests.cs
```
```
data.json → DashboardDataService (read + deserialize) → Dashboard.razor (inject)
    ↓                                                         ↓
DashboardData record                              Renders child components:
    - Title, Subtitle, Date                        - Header.razor
    - Milestones[]                                 - TimelineSvg.razor (SVG generation)
    - StatusCategories[]                           - HeatmapGrid.razor → HeatmapRow.razor
        - Shipped[]
        - InProgress[]
        - Carryover[]
        - Blockers[]
    - Months[] (column headers)
```
```csharp
public record DashboardData(
    string Title,
    string Subtitle,
    string CurrentDate,        // "April 2026"
    string BacklogUrl,
    string[] Months,           // ["Jan", "Feb", "Mar", "Apr"]
    int CurrentMonthIndex,     // 3 (0-based, for "Apr" highlight)
    Milestone[] Milestones,
    StatusCategory Shipped,
    StatusCategory InProgress,
    StatusCategory Carryover,
    StatusCategory Blockers
);

public record Milestone(
    string Label,
    string Sublabel,
    string Color,              // "#0078D4"
    MilestoneMarker[] Markers
);

public record MilestoneMarker(
    string Date,               // "2026-01-12"
    string Label,              // "Jan 12" or "Mar 26 PoC"
    string Type,               // "checkpoint" | "poc" | "production"
    double Position            // 0.0–1.0 normalized X position on timeline
);

public record StatusCategory(
    string Label,              // "SHIPPED"
    StatusItem[][] ItemsByMonth // Items grouped by month column index
);

public record StatusItem(
    string Name,
    string? Url                // Optional ADO link
);
``` The design has three clear visual sections — map them 1:1 to Razor components:
- **Header.razor** — Title, subtitle, legend icons. Pure Razor markup, no logic.
- **TimelineSvg.razor** — Receives `Milestone[]` and `CurrentDate`. Generates `<svg>` with month gridlines, milestone swim lanes, diamond/circle markers, and the "NOW" indicator line. Key logic: computing X positions from `MilestoneMarker.Position` × SVG width.
- **HeatmapGrid.razor** — Receives `StatusCategory[]` and `string[] Months`. Renders CSS Grid container with column headers, row headers, and cells. Delegates each row to **HeatmapRow.razor**.
- **HeatmapRow.razor** — Receives `StatusCategory` and renders the row header + 4 month cells with colored bullet items. Extract the original HTML `<style>` block nearly verbatim into `dashboard.css`. Key adaptations:
- Replace magic color values with CSS custom properties: `--color-shipped-bg: #F0FBF0; --color-shipped-dot: #34A853;`
- Keep `body { width: 1920px; height: 1080px; overflow: hidden; }` for screenshot fidelity
- The `.hm-grid` grid template (`grid-template-columns: 160px repeat(4, 1fr)`) should dynamically match the number of months. If always 4, hardcode it. If variable, generate the `style` attribute in Razor: `style="grid-template-columns: 160px repeat(@Months.Length, 1fr)"`. The timeline SVG in the original design is 1560×185 pixels. Key rendering logic:
```razor
@* In TimelineSvg.razor *@
<svg width="1560" height="185" style="overflow:visible;display:block">
    @foreach (var month in MonthGridlines)
    {
        <line x1="@month.X" y1="0" x2="@month.X" y2="185" stroke="#bbb" stroke-opacity="0.4"/>
        <text x="@(month.X + 5)" y="14" fill="#666" font-size="11">@month.Label</text>
    }
    @* "NOW" indicator *@
    <line x1="@NowX" y1="0" x2="@NowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
    @foreach (var (milestone, rowY) in MilestoneRows)
    {
        <line x1="0" y1="@rowY" x2="1560" y2="@rowY" stroke="@milestone.Color" stroke-width="3"/>
        @foreach (var marker in milestone.Markers)
        {
            @* Render circle, diamond, or labeled diamond based on marker.Type *@
        }
    }
</svg>
``` ---

### Considerations & Risks

- **Recommendation: None.** This is a local-only tool for generating screenshots. Adding authentication would be pure overhead. If sharing is needed later, put it behind a VPN or Windows Authentication with one line in `Program.cs`.
- `data.json` contains project names and milestone labels — no PII, no secrets.
- If sensitive project names are a concern, don't commit `data.json` to source control (add to `.gitignore`) and distribute it separately.
- No encryption needed for local file reads. | Aspect | Recommendation | |--------|---------------| | **Runtime** | `dotnet run` locally, or publish as self-contained single-file executable | | **Port** | `localhost:5000` (HTTP only — no HTTPS needed locally) | | **Publish command** | `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` | | **Distribution** | Zip the publish output + `data.json` and share with stakeholders | | **No containers** | Docker is unnecessary for a local single-user tool | For sharing with non-developers:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
``` This produces a single `.exe` (~80MB) that runs without .NET installed. Include `data.json` alongside it. **$0.** This runs on the developer's machine. No cloud services, no hosting fees, no licenses. --- | Risk | Likelihood | Impact | Mitigation | |------|-----------|--------|------------| | **SVG layout drift between browsers** | Medium | Medium | Test in both Edge and Chrome; use explicit `font-family` on all `<text>` elements; specify `width`/`height` on `<svg>` | | **CSS Grid rendering differences** | Low | Low | The design targets modern browsers only; Grid is fully supported in Chrome/Edge 88+ | | **Blazor Server circuit timeout** | Low | Low | Default idle timeout is 3 minutes; increase in `Program.cs` if needed: `options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(30)` | | **data.json schema changes break deserialization** | Medium | Medium | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and nullable properties; add a JSON schema file for validation | | **Screenshot inconsistency across displays** | Medium | High | Always capture at 1920×1080 using browser Device Mode (not window resize); document the exact capture steps | | Decision | Trade-off | Why It's Acceptable | |----------|-----------|-------------------| | **No database** | Can't query/filter data dynamically | Data volume is tiny; JSON is simpler to edit | | **No component library (MudBlazor, Radzen)** | Must write CSS manually | ~100 lines of CSS, already provided in the HTML reference | | **No JS charting library** | Must generate SVG manually in Razor | Timeline has <20 elements; Razor loops are clearer than library configuration | | **Blazor Server instead of Static SSR** | SignalR circuit overhead for a read-only page | Negligible for single-user local use; provides hot reload during development | | **Fixed 1920×1080 layout** | Not usable on mobile/small screens | Intentional — this is a screenshot tool, not a responsive web app | The only potential issue is **SVG text positioning**. The original HTML uses hardcoded X/Y coordinates for milestone labels (e.g., `x="745" y="66"`). The Blazor version must compute these from data. **Recommendation:** Use the `MilestoneMarker.Position` field (0.0–1.0 normalized) and multiply by SVG width (1560px) at render time. This makes the timeline data-driven without hardcoding pixel positions. --- | # | Question | Who Decides | Default If Unanswered | |---|----------|------------|----------------------| | 1 | **How many months should the heatmap show?** The original design shows 4 (Jan–Apr). Should this be configurable in `data.json`? | Product Owner | Yes, make it configurable with 4 as default | | 2 | **Should the timeline span be configurable?** Original shows Jan–Jun (6 months). | Product Owner | Yes, driven by `data.json` date range | | 3 | **Is the "ADO Backlog" link functional or decorative?** | Product Owner | Make it a real hyperlink from `data.json`; works when clicked in browser, inert in screenshots | | 4 | **Will multiple dashboards exist for different projects?** If so, should the app support switching between `data.json` files? | Product Owner | Start with single file; add a file picker later if needed | | 5 | **What's the screenshot capture workflow?** Manual browser screenshot, or automated via Playwright? | User | Manual for now; document browser Device Mode steps | | 6 | **Should the "NOW" line position auto-calculate from system date, or be specified in `data.json`?** | Engineer | Auto-calculate from `DateTime.Now` relative to the timeline date range | ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard renders a project milestone timeline, status heatmap (Shipped / In Progress / Carryover / Blockers), and is designed to be screenshot-friendly for PowerPoint decks at 1920×1080 resolution.

**Primary recommendation:** Keep the architecture radically simple. Use a single Blazor Server project with one Razor page, read data from a local `data.json` file using `System.Text.Json`, render the timeline via inline SVG generated in Razor, and style everything with a single CSS file that mirrors the original HTML design. No database, no authentication, no external services. The entire app should be runnable with `dotnet run` and viewable at `localhost:5000`.

This is fundamentally a **data-driven static rendering problem** — Blazor Server is slightly over-engineered for it, but it provides a clean .NET developer experience, hot reload during development, and easy extensibility if the dashboard grows later.

---

## 2. Key Findings

- The original `OriginalDesignConcept.html` is a self-contained static HTML/CSS/SVG page at exactly 1920×1080. The Blazor implementation must preserve this pixel-perfect layout for screenshot capture.
- The design uses **CSS Grid** (5-column: `160px repeat(4,1fr)`) for the heatmap and **Flexbox** for header/layout. Both are fully supported in modern browsers and require no JavaScript frameworks.
- The timeline section uses **inline SVG** with basic shapes: `<line>`, `<circle>`, `<polygon>` (diamond milestones), `<text>`, and a `<feDropShadow>` filter. This is trivially reproducible in Razor markup with `@foreach` loops over milestone data.
- **No charting library is needed.** The SVG is simple enough to generate directly from Razor templates. Adding a charting library (e.g., Chart.js via JS interop) would add complexity with zero benefit for this use case.
- A flat `data.json` file is the ideal data source — no database overhead, easy to edit, version-controllable, and trivially deserialized with `System.Text.Json`.
- Blazor Server's SignalR circuit is unnecessary for this read-only dashboard but does no harm. The app will work fine as a local tool.
- The color palette is well-defined in the HTML reference (greens for shipped, blues for in-progress, ambers for carryover, reds for blockers). These should be extracted into CSS custom properties for maintainability.
- The design is **not responsive** — it targets a fixed 1920×1080 viewport. This is intentional for screenshot fidelity and should be preserved, not "fixed."

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server UI)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Framework** | Blazor Server (.NET 8) | `net8.0` | Ships with `Microsoft.AspNetCore.App` — no extra NuGet packages needed |
| **CSS Layout** | Vanilla CSS (Grid + Flexbox) | N/A | Matches original HTML design exactly; no CSS framework needed |
| **SVG Rendering** | Inline SVG via Razor | N/A | Generate `<svg>` elements directly in `.razor` files with `@foreach` over milestones |
| **CSS Architecture** | Single `dashboard.css` file with CSS Custom Properties | N/A | Extract color tokens as `--color-shipped`, `--color-progress`, etc. |
| **Font** | Segoe UI (system font) | N/A | Already available on Windows; no web font loading needed |

**Why no CSS framework (Bootstrap, Tailwind, MudBlazor)?** The design is a fixed-viewport, single-page layout with ~100 lines of CSS. Adding a framework adds bundle size, class name conflicts, and overrides to fight. The original HTML proves vanilla CSS is sufficient.

**Why no charting library?** The timeline is ~20 SVG elements. Libraries like `Radzen.Blazor` or `ApexCharts.Blazor` are designed for interactive charts with tooltips, zoom, and animation — none of which matter for a screenshot-oriented dashboard. Hand-crafted SVG in Razor gives pixel-perfect control.

### Backend (Data Layer)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Zero dependencies; use `JsonSerializer.Deserialize<T>()` with source generators for AOT compatibility |
| **File I/O** | `System.IO.File.ReadAllTextAsync` | Built into .NET 8 | Read `data.json` from disk on page load |
| **Data Models** | C# records | Built into .NET 8 | Immutable POCOs: `DashboardData`, `Milestone`, `StatusItem` |
| **Configuration** | `appsettings.json` for file path | Built into .NET 8 | Store `data.json` path in config so it's overridable |

**Why no database?** The data source is a single JSON file with ~50-100 items. SQLite, LiteDB, or EF Core would add migration complexity, connection management, and schema overhead for zero benefit. If requirements grow, SQLite can be added later without architectural changes.

### Data Storage

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Primary store** | `data.json` flat file | Simplest possible approach; human-editable; version-controllable |
| **Format** | JSON with `System.Text.Json` conventions | `camelCase` property naming; strongly-typed C# records |
| **Location** | Project root or configurable path via `appsettings.json` | Default to `./data.json` relative to content root |
| **Caching** | Read file once at startup, re-read on page refresh | Use `IMemoryCache` only if file reads become a bottleneck (they won't) |

### Testing

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Testing** | xUnit | `2.7.0+` | .NET ecosystem standard; first-class `dotnet test` support |
| **Assertions** | FluentAssertions | `6.12.0` | Readable assertion syntax: `data.Milestones.Should().HaveCount(3)` |
| **Blazor Component Testing** | bUnit | `1.25.3+` | Test Razor components in isolation without a browser; MIT license |
| **Snapshot Testing** | Verify | `23.5.2+` | Optional — snapshot test rendered HTML to catch visual regressions |

### Infrastructure & Tooling

| Tool | Purpose | Notes |
|------|---------|-------|
| **`dotnet run`** | Local execution | No Docker, no IIS, no reverse proxy needed |
| **`dotnet watch`** | Hot reload during development | Blazor Server supports hot reload for .razor and .css files |
| **Kestrel** | Web server | Built into .NET 8; binds to `localhost:5000` by default |
| **Screenshot capture** | Browser DevTools or `Ctrl+Shift+S` in Edge/Chrome | Set viewport to 1920×1080 Device Mode for consistent captures |

---

## 4. Architecture Recommendations

### Overall Pattern: **Single-Project, File-Driven Razor Page**

This is deliberately the simplest viable architecture. No layers, no services, no abstractions beyond what Razor naturally provides.

```
ReportingDashboard/
├── ReportingDashboard.sln
├── ReportingDashboard/
│   ├── ReportingDashboard.csproj        # net8.0, no extra NuGet refs
│   ├── Program.cs                        # Minimal hosting: builder.Build().Run()
│   ├── appsettings.json                  # DataFilePath: "./data.json"
│   ├── data.json                         # Dashboard data (milestones, status items)
│   ├── Models/
│   │   └── DashboardData.cs             # C# records for JSON shape
│   ├── Services/
│   │   └── DashboardDataService.cs      # Reads & deserializes data.json
│   ├── Components/
│   │   ├── App.razor                    # Root component
│   │   ├── Pages/
│   │   │   └── Dashboard.razor          # Single page — the entire UI
│   │   ├── Layout/
│   │   │   └── MainLayout.razor         # Minimal layout wrapper
│   │   └── Shared/
│   │       ├── TimelineSvg.razor        # SVG timeline component
│   │       ├── HeatmapGrid.razor        # CSS Grid heatmap component
│   │       ├── HeatmapRow.razor         # Single status row (Shipped/InProgress/etc.)
│   │       └── Header.razor             # Title, subtitle, legend
│   └── wwwroot/
│       └── css/
│           └── dashboard.css            # All styles — mirrors original HTML
├── ReportingDashboard.Tests/
│   ├── ReportingDashboard.Tests.csproj
│   └── DashboardDataServiceTests.cs
```

### Data Flow

```
data.json → DashboardDataService (read + deserialize) → Dashboard.razor (inject)
    ↓                                                         ↓
DashboardData record                              Renders child components:
    - Title, Subtitle, Date                        - Header.razor
    - Milestones[]                                 - TimelineSvg.razor (SVG generation)
    - StatusCategories[]                           - HeatmapGrid.razor → HeatmapRow.razor
        - Shipped[]
        - InProgress[]
        - Carryover[]
        - Blockers[]
    - Months[] (column headers)
```

### Data Model Design

```csharp
public record DashboardData(
    string Title,
    string Subtitle,
    string CurrentDate,        // "April 2026"
    string BacklogUrl,
    string[] Months,           // ["Jan", "Feb", "Mar", "Apr"]
    int CurrentMonthIndex,     // 3 (0-based, for "Apr" highlight)
    Milestone[] Milestones,
    StatusCategory Shipped,
    StatusCategory InProgress,
    StatusCategory Carryover,
    StatusCategory Blockers
);

public record Milestone(
    string Label,
    string Sublabel,
    string Color,              // "#0078D4"
    MilestoneMarker[] Markers
);

public record MilestoneMarker(
    string Date,               // "2026-01-12"
    string Label,              // "Jan 12" or "Mar 26 PoC"
    string Type,               // "checkpoint" | "poc" | "production"
    double Position            // 0.0–1.0 normalized X position on timeline
);

public record StatusCategory(
    string Label,              // "SHIPPED"
    StatusItem[][] ItemsByMonth // Items grouped by month column index
);

public record StatusItem(
    string Name,
    string? Url                // Optional ADO link
);
```

### Component Decomposition

The design has three clear visual sections — map them 1:1 to Razor components:

1. **Header.razor** — Title, subtitle, legend icons. Pure Razor markup, no logic.
2. **TimelineSvg.razor** — Receives `Milestone[]` and `CurrentDate`. Generates `<svg>` with month gridlines, milestone swim lanes, diamond/circle markers, and the "NOW" indicator line. Key logic: computing X positions from `MilestoneMarker.Position` × SVG width.
3. **HeatmapGrid.razor** — Receives `StatusCategory[]` and `string[] Months`. Renders CSS Grid container with column headers, row headers, and cells. Delegates each row to **HeatmapRow.razor**.
4. **HeatmapRow.razor** — Receives `StatusCategory` and renders the row header + 4 month cells with colored bullet items.

### CSS Strategy

Extract the original HTML `<style>` block nearly verbatim into `dashboard.css`. Key adaptations:

- Replace magic color values with CSS custom properties: `--color-shipped-bg: #F0FBF0; --color-shipped-dot: #34A853;`
- Keep `body { width: 1920px; height: 1080px; overflow: hidden; }` for screenshot fidelity
- The `.hm-grid` grid template (`grid-template-columns: 160px repeat(4, 1fr)`) should dynamically match the number of months. If always 4, hardcode it. If variable, generate the `style` attribute in Razor: `style="grid-template-columns: 160px repeat(@Months.Length, 1fr)"`.

### SVG Timeline Generation

The timeline SVG in the original design is 1560×185 pixels. Key rendering logic:

```razor
@* In TimelineSvg.razor *@
<svg width="1560" height="185" style="overflow:visible;display:block">
    @foreach (var month in MonthGridlines)
    {
        <line x1="@month.X" y1="0" x2="@month.X" y2="185" stroke="#bbb" stroke-opacity="0.4"/>
        <text x="@(month.X + 5)" y="14" fill="#666" font-size="11">@month.Label</text>
    }
    @* "NOW" indicator *@
    <line x1="@NowX" y1="0" x2="@NowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
    @foreach (var (milestone, rowY) in MilestoneRows)
    {
        <line x1="0" y1="@rowY" x2="1560" y2="@rowY" stroke="@milestone.Color" stroke-width="3"/>
        @foreach (var marker in milestone.Markers)
        {
            @* Render circle, diamond, or labeled diamond based on marker.Type *@
        }
    }
</svg>
```

---

## 5. Security & Infrastructure

### Authentication & Authorization

**Recommendation: None.** This is a local-only tool for generating screenshots. Adding authentication would be pure overhead. If sharing is needed later, put it behind a VPN or Windows Authentication with one line in `Program.cs`.

### Data Protection

- `data.json` contains project names and milestone labels — no PII, no secrets.
- If sensitive project names are a concern, don't commit `data.json` to source control (add to `.gitignore`) and distribute it separately.
- No encryption needed for local file reads.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | `dotnet run` locally, or publish as self-contained single-file executable |
| **Port** | `localhost:5000` (HTTP only — no HTTPS needed locally) |
| **Publish command** | `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` |
| **Distribution** | Zip the publish output + `data.json` and share with stakeholders |
| **No containers** | Docker is unnecessary for a local single-user tool |

### Deployment as Self-Contained Executable

For sharing with non-developers:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```
This produces a single `.exe` (~80MB) that runs without .NET installed. Include `data.json` alongside it.

### Infrastructure Costs

**$0.** This runs on the developer's machine. No cloud services, no hosting fees, no licenses.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| **SVG layout drift between browsers** | Medium | Medium | Test in both Edge and Chrome; use explicit `font-family` on all `<text>` elements; specify `width`/`height` on `<svg>` |
| **CSS Grid rendering differences** | Low | Low | The design targets modern browsers only; Grid is fully supported in Chrome/Edge 88+ |
| **Blazor Server circuit timeout** | Low | Low | Default idle timeout is 3 minutes; increase in `Program.cs` if needed: `options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(30)` |
| **data.json schema changes break deserialization** | Medium | Medium | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and nullable properties; add a JSON schema file for validation |
| **Screenshot inconsistency across displays** | Medium | High | Always capture at 1920×1080 using browser Device Mode (not window resize); document the exact capture steps |

### Trade-offs Made

| Decision | Trade-off | Why It's Acceptable |
|----------|-----------|-------------------|
| **No database** | Can't query/filter data dynamically | Data volume is tiny; JSON is simpler to edit |
| **No component library (MudBlazor, Radzen)** | Must write CSS manually | ~100 lines of CSS, already provided in the HTML reference |
| **No JS charting library** | Must generate SVG manually in Razor | Timeline has <20 elements; Razor loops are clearer than library configuration |
| **Blazor Server instead of Static SSR** | SignalR circuit overhead for a read-only page | Negligible for single-user local use; provides hot reload during development |
| **Fixed 1920×1080 layout** | Not usable on mobile/small screens | Intentional — this is a screenshot tool, not a responsive web app |

### Potential Bottleneck

The only potential issue is **SVG text positioning**. The original HTML uses hardcoded X/Y coordinates for milestone labels (e.g., `x="745" y="66"`). The Blazor version must compute these from data. **Recommendation:** Use the `MilestoneMarker.Position` field (0.0–1.0 normalized) and multiply by SVG width (1560px) at render time. This makes the timeline data-driven without hardcoding pixel positions.

---

## 7. Open Questions

| # | Question | Who Decides | Default If Unanswered |
|---|----------|------------|----------------------|
| 1 | **How many months should the heatmap show?** The original design shows 4 (Jan–Apr). Should this be configurable in `data.json`? | Product Owner | Yes, make it configurable with 4 as default |
| 2 | **Should the timeline span be configurable?** Original shows Jan–Jun (6 months). | Product Owner | Yes, driven by `data.json` date range |
| 3 | **Is the "ADO Backlog" link functional or decorative?** | Product Owner | Make it a real hyperlink from `data.json`; works when clicked in browser, inert in screenshots |
| 4 | **Will multiple dashboards exist for different projects?** If so, should the app support switching between `data.json` files? | Product Owner | Start with single file; add a file picker later if needed |
| 5 | **What's the screenshot capture workflow?** Manual browser screenshot, or automated via Playwright? | User | Manual for now; document browser Device Mode steps |
| 6 | **Should the "NOW" line position auto-calculate from system date, or be specified in `data.json`?** | Engineer | Auto-calculate from `DateTime.Now` relative to the timeline date range |

---

## 8. Implementation Recommendations

### Phasing

#### Phase 1: Core Dashboard (MVP) — ~4 hours
1. Create solution structure: `dotnet new sln`, `dotnet new blazorserver`
2. Define `DashboardData` C# records matching the JSON schema
3. Create `data.json` with fictional project data (e.g., "Project Phoenix")
4. Implement `DashboardDataService` to read and deserialize `data.json`
5. Port the original HTML `<style>` block to `dashboard.css`
6. Build `Dashboard.razor` with inline header, timeline SVG, and heatmap grid
7. Verify 1920×1080 screenshot matches original design quality

#### Phase 2: Component Refactor — ~2 hours
1. Extract `Header.razor`, `TimelineSvg.razor`, `HeatmapGrid.razor`, `HeatmapRow.razor`
2. Make month count and timeline span data-driven from `data.json`
3. Auto-calculate "NOW" line position from system date
4. Add CSS custom properties for the color palette

#### Phase 3: Polish & Testing — ~2 hours
1. Add xUnit + bUnit tests for `DashboardDataService` and key components
2. Create a sample `data.json` with edge cases (empty months, long item names, many items per cell)
3. Test screenshot capture in Edge and Chrome at 1920×1080
4. Document the screenshot workflow in a `README.md`
5. Publish as self-contained single-file executable for distribution

### Quick Wins

1. **Copy the CSS verbatim** from `OriginalDesignConcept.html` into `dashboard.css`. This gets 90% of the visual design correct on the first render.
2. **Start with hardcoded Razor markup** that mirrors the original HTML structure, then progressively replace static values with `@model.Property` bindings. This ensures the layout is correct before introducing data binding complexity.
3. **Use `dotnet watch run`** during development for instant feedback on `.razor` and `.css` changes.

### Prototyping Recommendations

- **SVG timeline positioning**: Before building the full data model, prototype the timeline SVG with 3 hardcoded milestones to validate that computed X positions render correctly. The SVG coordinate math is the trickiest part of this project.
- **CSS Grid heatmap with variable content**: Test what happens when a cell has 0 items, 1 item, or 8+ items. The original design uses `overflow: hidden` on cells — verify this truncation looks acceptable or if scrolling/truncation needs adjustment.

### `data.json` Example Schema

```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Engineering Platform • Core Infrastructure • April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "currentDate": "2026-04-15",
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonthIndex": 3,
  "milestones": [
    {
      "label": "M1",
      "sublabel": "API Gateway & Auth",
      "color": "#0078D4",
      "markers": [
        { "date": "2026-01-15", "label": "Jan 15", "type": "checkpoint" },
        { "date": "2026-03-20", "label": "Mar 20 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "May Prod", "type": "production" }
      ]
    }
  ],
  "shipped": {
    "label": "SHIPPED",
    "itemsByMonth": [
      ["Auth Service v2", "Rate Limiting"],
      ["SDK v3.1", "Logging Pipeline"],
      ["Cache Layer", "Health Checks"],
      ["Config Service"]
    ]
  },
  "inProgress": {
    "label": "IN PROGRESS",
    "itemsByMonth": [[], [], ["API Gateway"], ["Gateway Beta", "Load Testing"]]
  },
  "carryover": {
    "label": "CARRYOVER",
    "itemsByMonth": [[], [], [], ["Schema Migration"]]
  },
  "blockers": {
    "label": "BLOCKERS",
    "itemsByMonth": [[], [], [], ["Vendor SDK Delay"]]
  }
}
```

### NuGet Packages Summary

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.App` (framework ref) | 8.0 | Blazor Server runtime | Yes (implicit) |
| `System.Text.Json` | 8.0 (built-in) | JSON deserialization | Yes (built-in) |
| `xunit` | 2.7.0 | Unit testing | Test project only |
| `xunit.runner.visualstudio` | 2.5.7 | VS Test integration | Test project only |
| `bunit` | 1.25.3 | Blazor component testing | Test project only |
| `FluentAssertions` | 6.12.0 | Readable assertions | Test project only |

**Total production dependencies beyond .NET 8 SDK: Zero.**

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/9fa102d2a984fd378ac354d306102969d68fe82e/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
