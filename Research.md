# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-17 10:53 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, progress status, and team delivery metrics. The dashboard reads from a local `data.json` configuration file and renders a pixel-perfect view optimized for 1920×1080 screenshots destined for PowerPoint executive decks. **Primary Recommendation:** Build a minimal Blazor Server application with a single Razor component page, no database, no authentication, and no external cloud dependencies. The entire data layer is a single JSON file deserialized at startup. Use inline CSS or a single scoped CSS file to replicate the reference design's Grid/Flexbox layout. For the timeline/milestone SVG visualization, use Blazor's native `MarkupString` rendering with a lightweight SVG builder — no charting library is needed given the design's simplicity. This project should be deliverable in **1–2 days** by a single developer. The architecture is intentionally over-simple: one `.sln`, one project, one page, one data file. This is the correct choice for a screenshot-oriented reporting tool with no multi-user, persistence, or scaling requirements. ---

### Key Findings

- The reference design (`OriginalDesignConcept.html`) is a static 1920×1080 layout using CSS Grid (5-column heatmap), Flexbox (header/timeline), and inline SVG (timeline with milestones). All of this maps directly to Blazor Razor components with zero JavaScript interop needed.
- **No charting library is required.** The timeline is a hand-crafted SVG with lines, circles, diamonds, and text — easily generated from C# code using Blazor's `RenderTreeBuilder` or `MarkupString`. Adding a full charting library (e.g., Radzen, MudBlazor charts) would be unnecessary complexity.
- **JSON file as data source is ideal** for this use case. `System.Text.Json` (built into .NET 8) handles deserialization with zero additional dependencies. No database, no ORM, no migrations.
- The design uses exactly **four status categories** (Shipped, In Progress, Carryover, Blockers) mapped across monthly columns — this is a fixed schema that maps cleanly to strongly-typed C# models.
- **Blazor Server is slightly overqualified** for a static reporting page, but it satisfies the stack requirement and provides hot-reload during development, easy CSS isolation, and a familiar .NET development experience. The SignalR circuit overhead is irrelevant for a local, single-user tool.
- The color palette is well-defined in the reference HTML (greens for shipped, blues for in-progress, ambers for carryover, reds for blockers). These should be extracted into CSS custom properties for maintainability.
- The design targets a **fixed 1920×1080 viewport** for screenshot fidelity — responsive design is explicitly not needed and would add unnecessary complexity.
- **No npm, no Node.js, no webpack** — the entire frontend is Blazor Razor components with CSS. This keeps the build pipeline as simple as `dotnet build && dotnet run`. --- **Goal:** Pixel-perfect reproduction of the reference design with fake data.
- `dotnet new blazorserver -n ReportingDashboard -f net8.0`
- Create `data.json` with fictional project data (e.g., "Project Phoenix — Cloud Migration")
- Define C# model classes: `DashboardData`, `Milestone`, `MilestoneStream`, `TimelineEvent`, `StatusCategory`
- Build `DashboardDataService` — reads and deserializes `data.json`
- Build 5 Razor components:
- `DashboardHeader.razor` — title, subtitle, legend icons
- `TimelineSection.razor` — SVG generation from milestone streams
- `HeatmapGrid.razor` — CSS Grid container
- `HeatmapRow.razor` — single status row (Shipped/InProgress/etc.)
- `HeatmapCell.razor` — cell with bullet items
- Wire up `Dashboard.razor` as the single page
- Style with scoped CSS matching reference design colors and layout exactly
- Verify at 1920×1080 in Edge **Deliverable:** Running dashboard at `localhost:5000` that matches the reference design. **Goal:** Enhance the reference design for better executive readability.
- **Auto-highlight current month column** — compute from system date, apply the `.apr` highlight class dynamically
- **Item count badges** — show count per cell (e.g., "3 items") in the column header for quick scanning
- **Tooltip on milestone diamonds** — CSS-only tooltip showing milestone description on hover (`:hover::after` pseudo-element, no JS)
- **Progress summary bar** — thin horizontal bar under the header showing shipped% / in-progress% / carryover% / blocked% as colored segments
- **Empty state handling** — if a category has no items for a month, show a subtle "—" instead of blank space
- **JSON validation on startup** — log clear error messages if `data.json` is malformed
- **Multi-project support:** CLI argument to specify data file, or a simple project selector dropdown
- **Screenshot automation:** Playwright script to capture `dashboard.png` on `dotnet run`
- **ADO integration:** Script that queries Azure DevOps API and generates `data.json` automatically
- **Print stylesheet:** `@media print` CSS for direct browser-to-PDF export
- **History view:** Load multiple JSON files (one per month) and show a trend over time | Win | Effort | Impact | |-----|--------|--------| | CSS custom properties for all colors | 15 min | Easy theme changes, dark mode ready | | Auto-detect current month highlighting | 10 min | No manual config for "which month is current" | | Self-contained publish as single EXE | 5 min | Share dashboard tool with non-developers | | `dotnet watch` during development | 0 min | Hot reload is built into .NET 8 Blazor |
- ❌ Authentication/authorization
- ❌ Database (SQLite, SQL Server, or otherwise)
- ❌ REST API endpoints
- ❌ Admin UI for editing data
- ❌ Responsive/mobile layout
- ❌ Dark mode (unless explicitly requested)
- ❌ Logging infrastructure (Serilog, Application Insights)
- ❌ Docker containerization
- ❌ CI/CD pipeline Every item above can be added later if needed, but none are justified for a local screenshot tool. --- | Package | Version | Purpose | License | |---------|---------|---------|---------| | `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Blazor Server runtime | MIT | | `System.Text.Json` | Built-in | JSON deserialization | MIT | | `Microsoft.Playwright` | 1.41.0 | Optional screenshot automation | Apache 2.0 | | `xunit` | 2.7.0 | Unit testing | Apache 2.0 | | `bunit` | 1.25.3 | Blazor component testing | MIT | | `FluentAssertions` | 6.12.0 | Test assertions | Apache 2.0 | **Total required NuGet packages beyond the default template: 0.** The MVP needs nothing beyond what `dotnet new blazorserver` provides.

### Recommended Tools & Technologies

- | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Framework** | Blazor Server (.NET 8) | `net8.0` | LTS release, supported through Nov 2026. Use the default Blazor Server template. | | **CSS Strategy** | Scoped CSS (`.razor.css` files) + CSS Custom Properties | Built-in | Blazor's CSS isolation compiles scoped styles automatically. Define color tokens as `--color-shipped: #34A853` etc. in a root stylesheet. | | **CSS Layout** | CSS Grid + Flexbox | Native | Grid for the heatmap (5-column: `160px repeat(4, 1fr)`), Flexbox for header and timeline area. Matches reference design exactly. | | **SVG Timeline** | Hand-built SVG via Blazor `MarkupString` | N/A | Generate SVG markup in C# from milestone data. No library needed — the reference SVG is ~60 lines. | | **Font** | Segoe UI (system font) | N/A | Already available on Windows. No web font loading needed. | | **Component Library** | **None** | — | MudBlazor, Radzen, and Syncfusion are all overkill. The design is custom and simple enough that a component library would fight the layout rather than help it. | **Why no component library:** The reference design has exactly two visual sections (timeline SVG + heatmap grid) with very specific styling. Component libraries impose their own design systems, spacing, and typography that would require extensive overriding to match the reference. Raw HTML/CSS in Razor components gives pixel-perfect control with less code. | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Runtime** | .NET 8 SDK | `8.0.x` (latest patch) | Use `dotnet new blazorserver` template. | | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Source generators available for AOT-friendly serialization via `[JsonSerializable]`. Not needed here but available. | | **Configuration** | `IOptions<T>` pattern or direct file read | Built-in | Load `data.json` via `builder.Configuration.AddJsonFile("data.json")` or manual `File.ReadAllText` + `JsonSerializer.Deserialize<T>()`. | | **Dependency Injection** | Built-in DI container | Built-in | Register a `DashboardDataService` as Singleton that loads and caches the JSON data. | | Component | Recommendation | Notes | |-----------|---------------|-------| | **Primary Store** | `data.json` flat file | Single JSON file in the project root or `wwwroot/data/` directory. No database whatsoever. | | **Schema Validation** | C# model classes with `System.Text.Json` attributes | Define `DashboardData`, `Milestone`, `StatusCategory`, `StatusItem` POCOs. JSON deserialization will fail fast on schema mismatch. | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Unit Testing** | xUnit | `2.7.x` | .NET ecosystem standard. | | **Blazor Component Testing** | bUnit | `1.25.x` | Purpose-built for Blazor component testing. Test that components render correct HTML structure from given data models. | | **Assertions** | FluentAssertions | `6.12.x` | Readable assertion syntax. MIT licensed. | | **Snapshot Testing** | Verify | `23.x` | Optional — useful for asserting SVG output hasn't changed unexpectedly. | | Component | Recommendation | Notes | |-----------|---------------|-------| | **IDE** | Visual Studio 2022 or VS Code + C# Dev Kit | Hot reload support for Blazor. | | **Build** | `dotnet build` / `dotnet run` | No additional build tools needed. | | **Screenshot Capture** | Browser → Print/Screenshot at 1920×1080 | Or use Playwright (`Microsoft.Playwright` 1.41.x) to automate headless screenshot capture if needed frequently. | ---
```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── data.json                          ← Data source
    ├── Models/
    │   ├── DashboardData.cs               ← Root model
    │   ├── Milestone.cs                   ← Timeline milestones
    │   ├── StatusCategory.cs              ← Shipped/InProgress/Carryover/Blockers
    │   └── StatusItem.cs                  ← Individual items within a category
    ├── Services/
    │   └── DashboardDataService.cs        ← Loads & caches data.json
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor            ← The single page
    │   ├── Layout/
    │   │   ├── MainLayout.razor           ← Minimal layout wrapper
    │   │   └── MainLayout.razor.css
    │   ├── DashboardHeader.razor          ← Title, subtitle, legend
    │   ├── DashboardHeader.razor.css
    │   ├── TimelineSection.razor          ← SVG milestone timeline
    │   ├── TimelineSection.razor.css
    │   ├── HeatmapGrid.razor              ← Status heatmap grid
    │   ├── HeatmapGrid.razor.css
    │   ├── HeatmapRow.razor               ← Single status row (reusable)
    │   └── HeatmapCell.razor              ← Single cell with items
    ├── wwwroot/
    │   └── css/
    │       └── app.css                    ← Global styles, CSS custom properties
    └── Properties/
        └── launchSettings.json
```
- **5 Razor components** map 1:1 to visual sections of the reference design, making it trivial to locate and modify any part of the UI.
- **No `Pages/` folder bloat** — there's literally one page.
- **Services layer** is a single class that reads `data.json` once and holds it in memory. No repository pattern, no unit-of-work — that's all unnecessary for a JSON file reader.
```
data.json → DashboardDataService (Singleton, loaded once at startup)
          → Dashboard.razor (injected via @inject)
          → Child components receive data via [Parameter] properties
``` There is no two-way data binding, no event handling, no forms, no user input. This is a **read-only rendering pipeline**.
```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentMonth": "Apr 2026",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "milestoneStreams": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
      ]
    }
  ],
  "nowDate": "2026-04-10",
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Privacy chatbot v1 deployed", "MS role mapping complete"],
        "Feb": ["Data inventory schema finalized"],
        "Mar": ["PDS integration PoC passed"],
        "Apr": ["Auto-review pipeline live"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": { ... }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": { ... }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": { ... }
    }
  ]
}
```
- `months` array is explicit (not computed) because executives may want to show a rolling window, not always Jan–Apr.
- `cssClass` on categories allows the Razor component to apply the correct color scheme without a switch statement.
- `milestoneStreams` support multiple parallel timeline tracks (M1, M2, M3) as shown in the reference design.
- `nowDate` drives the red "NOW" vertical line position in the SVG. The SVG timeline in the reference design is a coordinate-based visualization. The recommended approach:
- **Calculate x-position** by mapping dates to pixel positions: `x = (date - startDate) / (endDate - startDate) * svgWidth`
- **Render each milestone stream** as a horizontal line with events plotted along it
- **Event types** map to SVG shapes:
- `checkpoint` → `<circle>` (gray dot)
- `poc` → `<polygon>` (yellow diamond)
- `production` → `<polygon>` (green diamond)
- **"NOW" line** is a dashed red vertical line at the computed x-position of `nowDate` This is ~80 lines of C# in `TimelineSection.razor` using string interpolation to build SVG markup. Example:
```csharp
private string GetEventSvg(TimelineEvent evt, double x, double y) => evt.Type switch
{
    "checkpoint" => $"<circle cx=\"{x}\" cy=\"{y}\" r=\"5\" fill=\"white\" stroke=\"#888\" stroke-width=\"2.5\"/>",
    "poc" => $"<polygon points=\"{x},{y-11} {x+11},{y} {x},{y+11} {x-11},{y}\" fill=\"#F4B400\" filter=\"url(#sh)\"/>",
    "production" => $"<polygon points=\"{x},{y-11} {x+11},{y} {x},{y+11} {x-11},{y}\" fill=\"#34A853\" filter=\"url(#sh)\"/>",
    _ => ""
};
``` Define in `app.css`:
```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-shipped-header: #E8F5E9;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-progress-header: #E3F2FD;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-carryover-header: #FFF8E1;
    --color-blocker: #EA4335;
    --color-blocker-bg: #FFF5F5;
    --color-blocker-bg-current: #FFE4E4;
    --color-blocker-header: #FEF2F2;
    --color-border: #E0E0E0;
    --color-text-primary: #111;
    --color-text-secondary: #888;
    --color-link: #0078D4;
    --font-family: 'Segoe UI', Arial, sans-serif;
}
``` This makes theme adjustments trivial (e.g., dark mode for a different executive audience) without touching component CSS. ---

### Considerations & Risks

- This is explicitly a local-only, single-user tool for generating screenshots. Adding authentication would be unjustified complexity. The application should bind to `localhost` only:
```csharp
// Program.cs
app.Urls.Add("http://localhost:5000");
``` If there's ever a need to share the running instance on a network, add a simple shared-secret middleware later (5 lines of code). Do not pre-build auth infrastructure.
- `data.json` contains project status information, not PII or credentials. No encryption needed.
- If the JSON ever contains sensitive project names, keep it out of source control via `.gitignore` and distribute separately. | Concern | Approach | |---------|----------| | **Runtime** | Local `dotnet run` or published self-contained executable | | **Publishing** | `dotnet publish -c Release -r win-x64 --self-contained` produces a single folder that runs anywhere on Windows without .NET installed | | **Port** | `http://localhost:5000` (configurable in `launchSettings.json`) | | **Process Lifetime** | Run on-demand when preparing executive decks. Kill when done. No daemon, no service, no IIS. | **Estimated infrastructure cost: $0.** This runs on the developer's laptop. If screenshots are taken frequently, automate with Playwright:
```csharp
// Separate console app or script
using var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();
await page.SetViewportSizeAsync(1920, 1080);
await page.GotoAsync("http://localhost:5000");
await page.ScreenshotAsync(new() { Path = "dashboard.png" });
``` Package: `Microsoft.Playwright` version `1.41.0`. This could live in a separate `ReportingDashboard.Screenshots` project in the solution, or just be a PowerShell one-liner using Edge's built-in screenshot capability. --- **Severity: Low.** Blazor Server maintains a SignalR WebSocket connection for interactivity. For a read-only page, this is wasted overhead (extra ~50KB JS, persistent connection). However:
- The stack is mandated, so this is accepted.
- For a local single-user tool, the overhead is imperceptible.
- **Mitigation:** If page load speed matters for screenshot automation, consider Blazor Static SSR (available in .NET 8) which renders the page server-side without SignalR. Use `@rendermode` attribute: `@attribute [StreamRendering(false)]` and no interactive render mode. **Severity: Low.** The SVG timeline uses basic primitives (lines, circles, polygons, text). These render identically across Chrome, Edge, and Firefox. The drop shadow filter (`<feDropShadow>`) is widely supported.
- **Mitigation:** Standardize on Edge/Chrome for screenshots. Test once, then it's deterministic. **Severity: Low-Medium.** As the dashboard evolves, `data.json` schema changes could break deserialization silently (e.g., a new field defaults to `null` instead of failing).
- **Mitigation:** Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and add `[Required]` attributes on model properties. Add a startup validation step that logs warnings for missing data. **Severity: Low.** The design is intentionally fixed-width for screenshot consistency. On a 1080p monitor, it fills the screen. On a 4K monitor, it'll be small. On a laptop, it'll scroll.
- **Mitigation:** This is by design. Add a `<meta name="viewport" content="width=1920">` equivalent or use browser zoom. For screenshots, Playwright controls viewport size precisely. The dashboard reads a static JSON file, not a live ADO/Jira/GitHub API. This means manual data updates.
- **Accepted:** The user explicitly wants simplicity. Live API integration is a future enhancement, not MVP.
- **Migration path:** Replace `DashboardDataService.LoadFromFile()` with `DashboardDataService.LoadFromApi()` later. The component layer doesn't change. Without MudBlazor or Radzen, all styling is hand-written CSS. This is ~150 lines of CSS for this design.
- **Accepted:** 150 lines of purpose-built CSS is less complexity than importing a 500KB component library and fighting its defaults. ---
- **How frequently will `data.json` be updated?** If weekly, manual editing is fine. If daily, consider a small admin UI or a script that generates JSON from an ADO query.
- **Should the "current month" column highlighting be automatic** (based on system date) or manually set in `data.json`? Recommendation: Automatic, with manual override capability.
- **How many milestone streams (M1, M2, M3…) should be supported?** The reference design shows 3. The data model should support N streams, but the SVG vertical spacing needs a reasonable upper bound (suggest max 5 for visual clarity).
- **Will the same dashboard template be reused for multiple projects?** If yes, support a `--data` CLI argument: `dotnet run -- --data=projectA.json`. This is trivial to implement.
- **Is dark mode desired** for any executive audience? The CSS custom properties architecture supports this, but it should be decided before implementation to avoid rework.
- **Should the "ADO Backlog" link in the header be functional?** It's a simple `<a href>` — just needs the URL in `data.json`. Confirm whether this should open in a new tab or if it's just for display in the screenshot.
- **Print/PDF export:** Some executives prefer PDF over PowerPoint screenshots. Blazor Server can serve a print-friendly CSS stylesheet (`@media print`) with minimal additional effort. Worth doing? ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, progress status, and team delivery metrics. The dashboard reads from a local `data.json` configuration file and renders a pixel-perfect view optimized for 1920×1080 screenshots destined for PowerPoint executive decks.

**Primary Recommendation:** Build a minimal Blazor Server application with a single Razor component page, no database, no authentication, and no external cloud dependencies. The entire data layer is a single JSON file deserialized at startup. Use inline CSS or a single scoped CSS file to replicate the reference design's Grid/Flexbox layout. For the timeline/milestone SVG visualization, use Blazor's native `MarkupString` rendering with a lightweight SVG builder — no charting library is needed given the design's simplicity. This project should be deliverable in **1–2 days** by a single developer.

The architecture is intentionally over-simple: one `.sln`, one project, one page, one data file. This is the correct choice for a screenshot-oriented reporting tool with no multi-user, persistence, or scaling requirements.

---

## 2. Key Findings

- The reference design (`OriginalDesignConcept.html`) is a static 1920×1080 layout using CSS Grid (5-column heatmap), Flexbox (header/timeline), and inline SVG (timeline with milestones). All of this maps directly to Blazor Razor components with zero JavaScript interop needed.
- **No charting library is required.** The timeline is a hand-crafted SVG with lines, circles, diamonds, and text — easily generated from C# code using Blazor's `RenderTreeBuilder` or `MarkupString`. Adding a full charting library (e.g., Radzen, MudBlazor charts) would be unnecessary complexity.
- **JSON file as data source is ideal** for this use case. `System.Text.Json` (built into .NET 8) handles deserialization with zero additional dependencies. No database, no ORM, no migrations.
- The design uses exactly **four status categories** (Shipped, In Progress, Carryover, Blockers) mapped across monthly columns — this is a fixed schema that maps cleanly to strongly-typed C# models.
- **Blazor Server is slightly overqualified** for a static reporting page, but it satisfies the stack requirement and provides hot-reload during development, easy CSS isolation, and a familiar .NET development experience. The SignalR circuit overhead is irrelevant for a local, single-user tool.
- The color palette is well-defined in the reference HTML (greens for shipped, blues for in-progress, ambers for carryover, reds for blockers). These should be extracted into CSS custom properties for maintainability.
- The design targets a **fixed 1920×1080 viewport** for screenshot fidelity — responsive design is explicitly not needed and would add unnecessary complexity.
- **No npm, no Node.js, no webpack** — the entire frontend is Blazor Razor components with CSS. This keeps the build pipeline as simple as `dotnet build && dotnet run`.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Framework** | Blazor Server (.NET 8) | `net8.0` | LTS release, supported through Nov 2026. Use the default Blazor Server template. |
| **CSS Strategy** | Scoped CSS (`.razor.css` files) + CSS Custom Properties | Built-in | Blazor's CSS isolation compiles scoped styles automatically. Define color tokens as `--color-shipped: #34A853` etc. in a root stylesheet. |
| **CSS Layout** | CSS Grid + Flexbox | Native | Grid for the heatmap (5-column: `160px repeat(4, 1fr)`), Flexbox for header and timeline area. Matches reference design exactly. |
| **SVG Timeline** | Hand-built SVG via Blazor `MarkupString` | N/A | Generate SVG markup in C# from milestone data. No library needed — the reference SVG is ~60 lines. |
| **Font** | Segoe UI (system font) | N/A | Already available on Windows. No web font loading needed. |
| **Component Library** | **None** | — | MudBlazor, Radzen, and Syncfusion are all overkill. The design is custom and simple enough that a component library would fight the layout rather than help it. |

**Why no component library:** The reference design has exactly two visual sections (timeline SVG + heatmap grid) with very specific styling. Component libraries impose their own design systems, spacing, and typography that would require extensive overriding to match the reference. Raw HTML/CSS in Razor components gives pixel-perfect control with less code.

### Backend

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Runtime** | .NET 8 SDK | `8.0.x` (latest patch) | Use `dotnet new blazorserver` template. |
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Source generators available for AOT-friendly serialization via `[JsonSerializable]`. Not needed here but available. |
| **Configuration** | `IOptions<T>` pattern or direct file read | Built-in | Load `data.json` via `builder.Configuration.AddJsonFile("data.json")` or manual `File.ReadAllText` + `JsonSerializer.Deserialize<T>()`. |
| **Dependency Injection** | Built-in DI container | Built-in | Register a `DashboardDataService` as Singleton that loads and caches the JSON data. |

### Data Storage

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **Primary Store** | `data.json` flat file | Single JSON file in the project root or `wwwroot/data/` directory. No database whatsoever. |
| **Schema Validation** | C# model classes with `System.Text.Json` attributes | Define `DashboardData`, `Milestone`, `StatusCategory`, `StatusItem` POCOs. JSON deserialization will fail fast on schema mismatch. |

### Testing

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | `2.7.x` | .NET ecosystem standard. |
| **Blazor Component Testing** | bUnit | `1.25.x` | Purpose-built for Blazor component testing. Test that components render correct HTML structure from given data models. |
| **Assertions** | FluentAssertions | `6.12.x` | Readable assertion syntax. MIT licensed. |
| **Snapshot Testing** | Verify | `23.x` | Optional — useful for asserting SVG output hasn't changed unexpectedly. |

### Infrastructure & Tooling

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **IDE** | Visual Studio 2022 or VS Code + C# Dev Kit | Hot reload support for Blazor. |
| **Build** | `dotnet build` / `dotnet run` | No additional build tools needed. |
| **Screenshot Capture** | Browser → Print/Screenshot at 1920×1080 | Or use Playwright (`Microsoft.Playwright` 1.41.x) to automate headless screenshot capture if needed frequently. |

---

## 4. Architecture Recommendations

### Overall Architecture: Single-Project Monolith

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── data.json                          ← Data source
    ├── Models/
    │   ├── DashboardData.cs               ← Root model
    │   ├── Milestone.cs                   ← Timeline milestones
    │   ├── StatusCategory.cs              ← Shipped/InProgress/Carryover/Blockers
    │   └── StatusItem.cs                  ← Individual items within a category
    ├── Services/
    │   └── DashboardDataService.cs        ← Loads & caches data.json
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor            ← The single page
    │   ├── Layout/
    │   │   ├── MainLayout.razor           ← Minimal layout wrapper
    │   │   └── MainLayout.razor.css
    │   ├── DashboardHeader.razor          ← Title, subtitle, legend
    │   ├── DashboardHeader.razor.css
    │   ├── TimelineSection.razor          ← SVG milestone timeline
    │   ├── TimelineSection.razor.css
    │   ├── HeatmapGrid.razor              ← Status heatmap grid
    │   ├── HeatmapGrid.razor.css
    │   ├── HeatmapRow.razor               ← Single status row (reusable)
    │   └── HeatmapCell.razor              ← Single cell with items
    ├── wwwroot/
    │   └── css/
    │       └── app.css                    ← Global styles, CSS custom properties
    └── Properties/
        └── launchSettings.json
```

**Why this structure:**
- **5 Razor components** map 1:1 to visual sections of the reference design, making it trivial to locate and modify any part of the UI.
- **No `Pages/` folder bloat** — there's literally one page.
- **Services layer** is a single class that reads `data.json` once and holds it in memory. No repository pattern, no unit-of-work — that's all unnecessary for a JSON file reader.

### Data Flow

```
data.json → DashboardDataService (Singleton, loaded once at startup)
          → Dashboard.razor (injected via @inject)
          → Child components receive data via [Parameter] properties
```

There is no two-way data binding, no event handling, no forms, no user input. This is a **read-only rendering pipeline**.

### Data Model Design (`data.json`)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/...",
  "currentMonth": "Apr 2026",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "milestoneStreams": [
    {
      "id": "M1",
      "label": "Chatbot & MS Role",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production" }
      ]
    }
  ],
  "nowDate": "2026-04-10",
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "categories": [
    {
      "name": "Shipped",
      "cssClass": "ship",
      "items": {
        "Jan": ["Privacy chatbot v1 deployed", "MS role mapping complete"],
        "Feb": ["Data inventory schema finalized"],
        "Mar": ["PDS integration PoC passed"],
        "Apr": ["Auto-review pipeline live"]
      }
    },
    {
      "name": "In Progress",
      "cssClass": "prog",
      "items": { ... }
    },
    {
      "name": "Carryover",
      "cssClass": "carry",
      "items": { ... }
    },
    {
      "name": "Blockers",
      "cssClass": "block",
      "items": { ... }
    }
  ]
}
```

**Design decisions:**
- `months` array is explicit (not computed) because executives may want to show a rolling window, not always Jan–Apr.
- `cssClass` on categories allows the Razor component to apply the correct color scheme without a switch statement.
- `milestoneStreams` support multiple parallel timeline tracks (M1, M2, M3) as shown in the reference design.
- `nowDate` drives the red "NOW" vertical line position in the SVG.

### SVG Timeline Generation Strategy

The SVG timeline in the reference design is a coordinate-based visualization. The recommended approach:

1. **Calculate x-position** by mapping dates to pixel positions: `x = (date - startDate) / (endDate - startDate) * svgWidth`
2. **Render each milestone stream** as a horizontal line with events plotted along it
3. **Event types** map to SVG shapes:
   - `checkpoint` → `<circle>` (gray dot)
   - `poc` → `<polygon>` (yellow diamond)
   - `production` → `<polygon>` (green diamond)
4. **"NOW" line** is a dashed red vertical line at the computed x-position of `nowDate`

This is ~80 lines of C# in `TimelineSection.razor` using string interpolation to build SVG markup. Example:

```csharp
private string GetEventSvg(TimelineEvent evt, double x, double y) => evt.Type switch
{
    "checkpoint" => $"<circle cx=\"{x}\" cy=\"{y}\" r=\"5\" fill=\"white\" stroke=\"#888\" stroke-width=\"2.5\"/>",
    "poc" => $"<polygon points=\"{x},{y-11} {x+11},{y} {x},{y+11} {x-11},{y}\" fill=\"#F4B400\" filter=\"url(#sh)\"/>",
    "production" => $"<polygon points=\"{x},{y-11} {x+11},{y} {x},{y+11} {x-11},{y}\" fill=\"#34A853\" filter=\"url(#sh)\"/>",
    _ => ""
};
```

### CSS Custom Properties (Color Tokens)

Define in `app.css`:

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-shipped-header: #E8F5E9;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-progress-header: #E3F2FD;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-carryover-header: #FFF8E1;
    --color-blocker: #EA4335;
    --color-blocker-bg: #FFF5F5;
    --color-blocker-bg-current: #FFE4E4;
    --color-blocker-header: #FEF2F2;
    --color-border: #E0E0E0;
    --color-text-primary: #111;
    --color-text-secondary: #888;
    --color-link: #0078D4;
    --font-family: 'Segoe UI', Arial, sans-serif;
}
```

This makes theme adjustments trivial (e.g., dark mode for a different executive audience) without touching component CSS.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**Recommendation: None.**

This is explicitly a local-only, single-user tool for generating screenshots. Adding authentication would be unjustified complexity. The application should bind to `localhost` only:

```csharp
// Program.cs
app.Urls.Add("http://localhost:5000");
```

If there's ever a need to share the running instance on a network, add a simple shared-secret middleware later (5 lines of code). Do not pre-build auth infrastructure.

### Data Protection

- `data.json` contains project status information, not PII or credentials. No encryption needed.
- If the JSON ever contains sensitive project names, keep it out of source control via `.gitignore` and distribute separately.

### Hosting & Deployment

| Concern | Approach |
|---------|----------|
| **Runtime** | Local `dotnet run` or published self-contained executable |
| **Publishing** | `dotnet publish -c Release -r win-x64 --self-contained` produces a single folder that runs anywhere on Windows without .NET installed |
| **Port** | `http://localhost:5000` (configurable in `launchSettings.json`) |
| **Process Lifetime** | Run on-demand when preparing executive decks. Kill when done. No daemon, no service, no IIS. |

**Estimated infrastructure cost: $0.** This runs on the developer's laptop.

### Screenshot Automation (Optional Enhancement)

If screenshots are taken frequently, automate with Playwright:

```csharp
// Separate console app or script
using var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();
await page.SetViewportSizeAsync(1920, 1080);
await page.GotoAsync("http://localhost:5000");
await page.ScreenshotAsync(new() { Path = "dashboard.png" });
```

Package: `Microsoft.Playwright` version `1.41.0`. This could live in a separate `ReportingDashboard.Screenshots` project in the solution, or just be a PowerShell one-liner using Edge's built-in screenshot capability.

---

## 6. Risks & Trade-offs

### Risk: Blazor Server Overhead for a Static Page

**Severity: Low.** Blazor Server maintains a SignalR WebSocket connection for interactivity. For a read-only page, this is wasted overhead (extra ~50KB JS, persistent connection). However:
- The stack is mandated, so this is accepted.
- For a local single-user tool, the overhead is imperceptible.
- **Mitigation:** If page load speed matters for screenshot automation, consider Blazor Static SSR (available in .NET 8) which renders the page server-side without SignalR. Use `@rendermode` attribute: `@attribute [StreamRendering(false)]` and no interactive render mode.

### Risk: SVG Rendering Differences Across Browsers

**Severity: Low.** The SVG timeline uses basic primitives (lines, circles, polygons, text). These render identically across Chrome, Edge, and Firefox. The drop shadow filter (`<feDropShadow>`) is widely supported.
- **Mitigation:** Standardize on Edge/Chrome for screenshots. Test once, then it's deterministic.

### Risk: JSON Schema Drift

**Severity: Low-Medium.** As the dashboard evolves, `data.json` schema changes could break deserialization silently (e.g., a new field defaults to `null` instead of failing).
- **Mitigation:** Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and add `[Required]` attributes on model properties. Add a startup validation step that logs warnings for missing data.

### Risk: Fixed 1920×1080 Layout on Different Screens

**Severity: Low.** The design is intentionally fixed-width for screenshot consistency. On a 1080p monitor, it fills the screen. On a 4K monitor, it'll be small. On a laptop, it'll scroll.
- **Mitigation:** This is by design. Add a `<meta name="viewport" content="width=1920">` equivalent or use browser zoom. For screenshots, Playwright controls viewport size precisely.

### Trade-off: No Live Data Connection

The dashboard reads a static JSON file, not a live ADO/Jira/GitHub API. This means manual data updates.
- **Accepted:** The user explicitly wants simplicity. Live API integration is a future enhancement, not MVP.
- **Migration path:** Replace `DashboardDataService.LoadFromFile()` with `DashboardDataService.LoadFromApi()` later. The component layer doesn't change.

### Trade-off: No Component Library = More CSS

Without MudBlazor or Radzen, all styling is hand-written CSS. This is ~150 lines of CSS for this design.
- **Accepted:** 150 lines of purpose-built CSS is less complexity than importing a 500KB component library and fighting its defaults.

---

## 7. Open Questions

1. **How frequently will `data.json` be updated?** If weekly, manual editing is fine. If daily, consider a small admin UI or a script that generates JSON from an ADO query.

2. **Should the "current month" column highlighting be automatic** (based on system date) or manually set in `data.json`? Recommendation: Automatic, with manual override capability.

3. **How many milestone streams (M1, M2, M3…) should be supported?** The reference design shows 3. The data model should support N streams, but the SVG vertical spacing needs a reasonable upper bound (suggest max 5 for visual clarity).

4. **Will the same dashboard template be reused for multiple projects?** If yes, support a `--data` CLI argument: `dotnet run -- --data=projectA.json`. This is trivial to implement.

5. **Is dark mode desired** for any executive audience? The CSS custom properties architecture supports this, but it should be decided before implementation to avoid rework.

6. **Should the "ADO Backlog" link in the header be functional?** It's a simple `<a href>` — just needs the URL in `data.json`. Confirm whether this should open in a new tab or if it's just for display in the screenshot.

7. **Print/PDF export:** Some executives prefer PDF over PowerPoint screenshots. Blazor Server can serve a print-friendly CSS stylesheet (`@media print`) with minimal additional effort. Worth doing?

---

## 8. Implementation Recommendations

### Phase 1: MVP (Day 1) — Core Dashboard Rendering

**Goal:** Pixel-perfect reproduction of the reference design with fake data.

1. `dotnet new blazorserver -n ReportingDashboard -f net8.0`
2. Create `data.json` with fictional project data (e.g., "Project Phoenix — Cloud Migration")
3. Define C# model classes: `DashboardData`, `Milestone`, `MilestoneStream`, `TimelineEvent`, `StatusCategory`
4. Build `DashboardDataService` — reads and deserializes `data.json`
5. Build 5 Razor components:
   - `DashboardHeader.razor` — title, subtitle, legend icons
   - `TimelineSection.razor` — SVG generation from milestone streams
   - `HeatmapGrid.razor` — CSS Grid container
   - `HeatmapRow.razor` — single status row (Shipped/InProgress/etc.)
   - `HeatmapCell.razor` — cell with bullet items
6. Wire up `Dashboard.razor` as the single page
7. Style with scoped CSS matching reference design colors and layout exactly
8. Verify at 1920×1080 in Edge

**Deliverable:** Running dashboard at `localhost:5000` that matches the reference design.

### Phase 2: Polish (Day 2) — Improvements Over Reference

**Goal:** Enhance the reference design for better executive readability.

1. **Auto-highlight current month column** — compute from system date, apply the `.apr` highlight class dynamically
2. **Item count badges** — show count per cell (e.g., "3 items") in the column header for quick scanning
3. **Tooltip on milestone diamonds** — CSS-only tooltip showing milestone description on hover (`:hover::after` pseudo-element, no JS)
4. **Progress summary bar** — thin horizontal bar under the header showing shipped% / in-progress% / carryover% / blocked% as colored segments
5. **Empty state handling** — if a category has no items for a month, show a subtle "—" instead of blank space
6. **JSON validation on startup** — log clear error messages if `data.json` is malformed

### Phase 3: Optional Enhancements (Future)

- **Multi-project support:** CLI argument to specify data file, or a simple project selector dropdown
- **Screenshot automation:** Playwright script to capture `dashboard.png` on `dotnet run`
- **ADO integration:** Script that queries Azure DevOps API and generates `data.json` automatically
- **Print stylesheet:** `@media print` CSS for direct browser-to-PDF export
- **History view:** Load multiple JSON files (one per month) and show a trend over time

### Quick Wins

| Win | Effort | Impact |
|-----|--------|--------|
| CSS custom properties for all colors | 15 min | Easy theme changes, dark mode ready |
| Auto-detect current month highlighting | 10 min | No manual config for "which month is current" |
| Self-contained publish as single EXE | 5 min | Share dashboard tool with non-developers |
| `dotnet watch` during development | 0 min | Hot reload is built into .NET 8 Blazor |

### What NOT to Build

- ❌ Authentication/authorization
- ❌ Database (SQLite, SQL Server, or otherwise)
- ❌ REST API endpoints
- ❌ Admin UI for editing data
- ❌ Responsive/mobile layout
- ❌ Dark mode (unless explicitly requested)
- ❌ Logging infrastructure (Serilog, Application Insights)
- ❌ Docker containerization
- ❌ CI/CD pipeline

Every item above can be added later if needed, but none are justified for a local screenshot tool.

---

## Appendix: Key NuGet Packages

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Blazor Server runtime | MIT |
| `System.Text.Json` | Built-in | JSON deserialization | MIT |
| `Microsoft.Playwright` | 1.41.0 | Optional screenshot automation | Apache 2.0 |
| `xunit` | 2.7.0 | Unit testing | Apache 2.0 |
| `bunit` | 1.25.3 | Blazor component testing | MIT |
| `FluentAssertions` | 6.12.0 | Test assertions | Apache 2.0 |

**Total required NuGet packages beyond the default template: 0.** The MVP needs nothing beyond what `dotnet new blazorserver` provides.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/9022ab0efe0e5f013abe1dea58ae07a05e474c15/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
