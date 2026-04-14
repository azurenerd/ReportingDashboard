# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-14 20:41 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The primary use case is generating pixel-perfect screenshots for PowerPoint decks to executive stakeholders. **Primary Recommendation:** Build a minimal Blazor Server application with a single Razor component page. Use inline SVG rendering for the timeline (no charting library needed — the design is simple enough to generate SVG directly in Razor). Use pure CSS Grid and Flexbox for the heatmap layout, matching the `OriginalDesignConcept.html` reference exactly. Read `data.json` at startup via `System.Text.Json` deserialization into strongly-typed C# models. No database, no authentication, no API layer. The entire application should be ~5 files beyond scaffolding. **Why this is the right approach:** The original HTML design is already a self-contained static page. Blazor Server adds just enough dynamism to load data from JSON without requiring a full SPA framework. The SignalR circuit overhead is negligible for a single-user local tool. This keeps the project dead simple while allowing future enhancements (e.g., editing data in the UI) without a rewrite. ---

### Key Findings

- The `OriginalDesignConcept.html` design uses CSS Grid (`160px repeat(4,1fr)`), Flexbox, and inline SVG — all of which translate directly to Blazor Razor components with zero JavaScript interop required.
- The color palette, fonts (Segoe UI), and layout dimensions are fully specified in the HTML reference; no design system library is needed — raw CSS matches the spec exactly.
- `System.Text.Json` (built into .NET 8) is the only serialization library needed; no NuGet packages are required for JSON handling.
- The SVG timeline with milestones (diamonds, circles, date labels, dashed "NOW" line) can be rendered purely in Razor markup using `<svg>` elements with calculated positions — no charting library adds value here and would overcomplicate positioning.
- Blazor Server's hot reload (`dotnet watch`) provides a fast inner loop for iterating on the pixel-perfect layout, which is critical since the output will be screenshot-captured.
- The fixed 1920×1080 viewport specified in the design aligns perfectly with screenshot capture — the page should use a fixed-width layout, not a responsive one.
- A `data.json` file-watcher pattern (using `IFileSystemWatcher` or `FileSystemWatcher`) would allow live-reloading data without restarting the app, which is a valuable quality-of-life feature for the user editing JSON.
- No authentication, no database, no API, no middleware complexity — this is intentionally a "tool" not a "product."
- The heatmap grid has exactly 4 status rows × 4 month columns with a row header column — this is a fixed structure that maps cleanly to a nested `@foreach` in Razor.
- Total estimated implementation effort: **4–8 hours** for an engineer familiar with Blazor, including fake data generation and CSS polish. --- **Goal:** A running Blazor Server app that renders the dashboard identically to the HTML reference, driven by `data.json`.
- **Scaffold project** — `dotnet new blazor --interactivity Server -n ReportingDashboard`
- **Define C# models** — `DashboardData`, `Workstream`, `Milestone`, `StatusCategory` records
- **Create DataService** — Reads `wwwroot/data.json` on startup, exposes typed data
- **Port CSS** — Copy styles from `OriginalDesignConcept.html` into `dashboard.css`, convert to CSS custom properties for colors
- **Build Dashboard.razor** — Single page with three sections: Header, Timeline SVG, Heatmap Grid
- **Create sample data.json** — Fictional project ("Project Atlas") with realistic milestones and status items
- **Test** — Verify visual match against the HTML reference at 1920×1080 in Edge **Deliverable:** `dotnet run` → open `http://localhost:5050` → screenshot-ready page.
- Add `FileSystemWatcher` for live data reload
- Add a print-friendly CSS `@media print` stylesheet
- Add basic error handling for malformed JSON with a user-friendly error page
- Add `schemaVersion` validation
- Improve SVG label collision detection (offset overlapping milestone labels)
- **In-browser data editor** — Blazor form to edit `data.json` fields without touching the file directly
- **Multi-project support** — Dropdown to switch between multiple JSON files in a `data/` directory
- **PDF export** — Use `Playwright` or `PuppeteerSharp` to programmatically capture the page as PDF
- **Historical snapshots** — Save dated copies of `data.json` and allow browsing past reports
- **CSS custom properties for colors** — Enables instant theme changes (e.g., dark mode for projector presentations) by swapping ~10 variable values
- **Auto-calculated "NOW" line** — `DateOnly.FromDateTime(DateTime.Today)` positions the red dashed line automatically, no manual JSON updates needed each month
- **Current month highlighting** — The `.apr` CSS class (amber highlight on current month column) can be auto-applied based on system date
- **Browser full-page screenshot** — Document the Edge shortcut (`Ctrl+Shift+S` → "Capture full page") in the README for instant PowerPoint-ready images
```xml
<!-- ReportingDashboard.csproj — no additional NuGet packages needed for Phase 1 -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```
```xml
<!-- ReportingDashboard.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
    <PackageReference Include="bunit" Version="1.28.9" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
  </ItemGroup>
</Project>
``` **Zero external dependencies for the main project.** This is the strongest possible position for a simple, maintainable tool.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **UI Framework** | Blazor Server (built-in) | .NET 8.0 | Server-side rendering with SignalR, single Razor page | | **CSS Layout** | Pure CSS Grid + Flexbox | CSS3 | Matches `OriginalDesignConcept.html` grid exactly | | **Timeline Visualization** | Inline SVG in Razor | SVG 1.1 | Milestone diamonds, progress lines, "NOW" marker | | **Icons/Shapes** | Raw SVG markup | — | Diamond (`<polygon>`), circles (`<circle>`), lines | | **Font** | Segoe UI (system font) | — | Already specified in design, no web font loading needed | The timeline is not a standard chart — it's a custom SVG with specific diamond milestones at pixel-precise positions, colored progress lines per workstream, and a dashed "NOW" marker. Every charting library would fight you on custom positioning. The SVG is ~60 lines of markup. Writing it directly in Razor with `@` expressions for data-driven positions is simpler, more maintainable, and produces the exact output needed. The design is a bespoke executive report, not a forms-driven application. Component libraries add 500KB+ of CSS/JS, impose their own design language, and require overriding styles to match the spec. Pure CSS is lighter and gives pixel-perfect control. The total CSS needed is ~150 lines (it's already written in the HTML reference). | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **Runtime** | .NET 8.0 SDK | 8.0.x (latest patch) | LTS release, Blazor Server hosting | | **JSON Deserialization** | System.Text.Json | Built-in (.NET 8) | Deserialize `data.json` to C# models | | **File Watching** | `FileSystemWatcher` | Built-in (.NET 8) | Optional: auto-reload when `data.json` changes | | **Configuration** | `IConfiguration` + JSON provider | Built-in | App settings (port, file path) | | Layer | Technology | Purpose | |-------|-----------|---------| | **Primary Data** | `data.json` (flat file) | All dashboard data: milestones, heatmap items, metadata | | **Schema Validation** | C# model classes with `required` properties | Compile-time safety, clear error messages on bad JSON | **No database.** This is deliberate. The data set is small (dozens of items), changes infrequently (manually edited), and has no relational complexity. A database adds startup overhead, migration management, and connection string configuration — all unnecessary for a screenshot tool. | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **Unit Tests** | xUnit | 2.7.x | Model deserialization, date calculation logic | | **Component Tests** | bUnit | 1.28.x+ | Blazor component rendering verification | | **Snapshot Tests** | Verify | 24.x | Optional: catch unintended layout regressions | | Tool | Purpose | |------|---------| | `dotnet watch` | Hot reload during development | | Visual Studio 2022 / VS Code + C# Dev Kit | IDE | | Browser DevTools (F12) | CSS Grid inspector, screenshot tool |
```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/           # Blazor Server project
│       ├── ReportingDashboard.csproj
│       ├── Program.cs                 # Minimal hosting setup
│       ├── Models/
│       │   └── DashboardData.cs       # Strongly-typed JSON models
│       ├── Services/
│       │   └── DataService.cs         # Reads & watches data.json
│       ├── Components/
│       │   ├── App.razor              # Root component
│       │   ├── Pages/
│       │   │   └── Dashboard.razor    # Single page — the entire dashboard
│       │   └── Layout/
│       │       └── MainLayout.razor   # Minimal layout wrapper
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css      # All styles (ported from HTML reference)
│       │   └── data.json              # Dashboard data file
│       └── Properties/
│           └── launchSettings.json
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── DataServiceTests.cs
``` --- This project does not need MVC, MVVM, Clean Architecture, CQRS, or any layered pattern. The entire architecture is:
```
data.json → DataService → Dashboard.razor → Browser
``` **DataService** (registered as Singleton):
- Reads `data.json` on startup using `System.Text.Json.JsonSerializer.Deserialize<DashboardData>()`
- Optionally watches the file with `FileSystemWatcher` and raises a change event
- Exposes `DashboardData GetData()` method **Dashboard.razor** (single page):
- Injects `DataService`
- Renders three visual sections:
- **Header** — Title, subtitle, legend icons
- **Timeline** — SVG with calculated positions for milestones
- **Heatmap Grid** — CSS Grid with data-driven cells
```csharp
public record DashboardData
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required DateOnly ReportDate { get; init; }
    public required string[] MonthColumns { get; init; }  // e.g., ["Jan", "Feb", "Mar", "Apr"]
    public required string CurrentMonth { get; init; }     // Highlights current column
    public required Workstream[] Workstreams { get; init; }
    public required StatusCategory[] Categories { get; init; }
}

public record Workstream
{
    public required string Id { get; init; }        // "M1", "M2", "M3"
    public required string Name { get; init; }       // "Chatbot & MS Role"
    public required string Color { get; init; }      // "#0078D4"
    public required Milestone[] Milestones { get; init; }
}

public record Milestone
{
    public required string Label { get; init; }      // "Mar 26 PoC"
    public required DateOnly Date { get; init; }
    public required string Type { get; init; }       // "poc" | "production" | "checkpoint" | "start"
}

public record StatusCategory
{
    public required string Name { get; init; }       // "Shipped", "In Progress", "Carryover", "Blockers"
    public required string CssClass { get; init; }   // "ship", "prog", "carry", "block"
    public required MonthItems[] Months { get; init; }
}

public record MonthItems
{
    public required string Month { get; init; }
    public required string[] Items { get; init; }
}
``` The timeline maps dates to X-coordinates within the SVG viewport. The calculation is straightforward:
```
x = (date - startDate).TotalDays / (endDate - startDate).TotalDays * svgWidth
``` Where `svgWidth` is ~1560px (matching the original), `startDate` is the first month, and `endDate` is the last month. This is pure C# math in the Razor component — no JS interop needed. **Port the existing CSS directly.** The `OriginalDesignConcept.html` contains ~100 lines of well-structured CSS that already produces the exact desired output. The strategy is:
- Copy the CSS into `dashboard.css` verbatim
- Replace hardcoded content with Razor `@foreach` loops and `@` expressions
- Use CSS custom properties (variables) for the color palette to enable easy theming if needed later
- Keep the fixed `1920px` width — this is designed for screenshot capture, not responsive viewing
```
Startup:
  Program.cs → builder.Services.AddSingleton<DataService>()
  DataService reads wwwroot/data.json → deserializes to DashboardData

Request:
  Browser → Blazor Server → Dashboard.razor
  Dashboard.razor calls DataService.GetData()
  Razor renders HTML + inline SVG
  CSS Grid + Flexbox handle layout
  Browser renders at 1920×1080

Optional file watch:
  User edits data.json
  FileSystemWatcher fires Changed event
  DataService reloads data
  Dashboard.razor re-renders via StateHasChanged()
``` ---

### Considerations & Risks

- **None.** This is explicitly a local-only, no-auth tool. The Blazor Server app binds to `localhost` only. No login, no roles, no tokens. In `Program.cs`:
```csharp
app.Urls.Add("http://localhost:5050");
```
- `data.json` contains project status data, not secrets. No encryption needed.
- If the data ever contains sensitive project names, the mitigation is filesystem ACLs (OS-level), not application-level encryption. | Aspect | Recommendation | |--------|---------------| | **Host** | Local machine only — `dotnet run` or compiled executable | | **Deployment** | `dotnet publish -c Release -o ./publish` → run `ReportingDashboard.exe` | | **Self-contained** | Publish as self-contained (`-r win-x64 --self-contained`) for machines without .NET 8 SDK | | **Port** | `http://localhost:5050` (configurable in `launchSettings.json`) | | **Process** | Run on demand, not as a Windows service | **$0.** This runs on the developer's workstation. No servers, no cloud, no containers.
- **Startup time:** <2 seconds for a minimal Blazor Server app
- **Memory:** ~50MB working set (Blazor Server baseline)
- **SignalR circuit:** Single user, single connection — negligible overhead
- **Hot reload:** `dotnet watch` works out of the box for CSS and Razor changes --- **Risk:** The dashboard will be screenshotted for PowerPoint. Different browsers render CSS Grid, SVG text, and font metrics slightly differently. A layout that looks perfect in Edge may shift 1–2px in Chrome. **Mitigation:** Standardize on a single browser for screenshots (recommend **Microsoft Edge** — Segoe UI renders best on Windows in Edge). Document this as a known constraint. Use the browser's built-in screenshot tool (`Ctrl+Shift+S` in Edge) or a fixed-viewport screenshot extension. **Risk:** As the dashboard evolves, the JSON schema may change, breaking existing `data.json` files. **Mitigation:** Use `required` properties in C# records. Add a `"schemaVersion": 1` field to `data.json`. Validate on load and throw a clear error message pointing to the expected format. **Trade-off:** Blazor Server maintains a SignalR WebSocket connection, which is overkill for what is essentially a static page. A pure Razor Pages or even a static HTML generator would have lower overhead. **Why it's acceptable:** The overhead is negligible for single-user local use (~2MB memory for the circuit). Blazor Server provides hot reload, easy data binding, and a path to interactivity (e.g., editing data in the UI, filtering by date range) without a rewrite. The developer experience is worth the minor overhead. **Risk:** Date-to-pixel mapping could produce off-by-one errors or overlapping labels for milestones that are close together. **Mitigation:** Use `DateOnly` arithmetic in C# (exact day-count division). Add a minimum pixel gap constant (e.g., 40px) and offset overlapping labels vertically. Test with edge cases: milestones on the same day, milestones at month boundaries. **Risk:** `FileSystemWatcher` can occasionally miss events or fire duplicates on Windows, especially with certain text editors that use atomic saves (write to temp file, then rename). **Mitigation:** Debounce the file change handler (e.g., ignore events within 500ms of each other). Alternatively, skip file watching entirely and require a browser refresh — this is simpler and perfectly adequate for a screenshot tool. --- | # | Question | Stakeholder | Impact | |---|----------|------------|--------| | 1 | **How many months should the heatmap display?** The current design shows 4 (Jan–Apr). Should this be configurable in `data.json`, or always show the last N months? | Product Owner | Affects grid column calculation and CSS | | 2 | **Should the timeline span be auto-calculated from milestone dates, or manually specified?** Auto-calc is smarter but may produce awkward date ranges. | Product Owner | Affects SVG rendering logic | | 3 | **Is there a need to support multiple projects in one instance?** E.g., a dropdown to switch between project JSON files, or always one project per instance? | User | Affects routing and data loading | | 4 | **What browser will be used for screenshots?** This affects font rendering and CSS Grid behavior. Recommend standardizing on Edge. | User | Affects CSS testing | | 5 | **Should the "NOW" line position be auto-calculated from system date, or specified in data.json?** Auto-calc is more convenient; manual gives control for generating historical snapshots. | User | Affects timeline logic | | 6 | **Will the data.json be hand-edited, or is there a future need for a UI editor?** This determines whether we invest in JSON schema documentation or a Blazor form. | Product Owner | Affects Phase 2 scope | ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The primary use case is generating pixel-perfect screenshots for PowerPoint decks to executive stakeholders.

**Primary Recommendation:** Build a minimal Blazor Server application with a single Razor component page. Use inline SVG rendering for the timeline (no charting library needed — the design is simple enough to generate SVG directly in Razor). Use pure CSS Grid and Flexbox for the heatmap layout, matching the `OriginalDesignConcept.html` reference exactly. Read `data.json` at startup via `System.Text.Json` deserialization into strongly-typed C# models. No database, no authentication, no API layer. The entire application should be ~5 files beyond scaffolding.

**Why this is the right approach:** The original HTML design is already a self-contained static page. Blazor Server adds just enough dynamism to load data from JSON without requiring a full SPA framework. The SignalR circuit overhead is negligible for a single-user local tool. This keeps the project dead simple while allowing future enhancements (e.g., editing data in the UI) without a rewrite.

---

## 2. Key Findings

- The `OriginalDesignConcept.html` design uses CSS Grid (`160px repeat(4,1fr)`), Flexbox, and inline SVG — all of which translate directly to Blazor Razor components with zero JavaScript interop required.
- The color palette, fonts (Segoe UI), and layout dimensions are fully specified in the HTML reference; no design system library is needed — raw CSS matches the spec exactly.
- `System.Text.Json` (built into .NET 8) is the only serialization library needed; no NuGet packages are required for JSON handling.
- The SVG timeline with milestones (diamonds, circles, date labels, dashed "NOW" line) can be rendered purely in Razor markup using `<svg>` elements with calculated positions — no charting library adds value here and would overcomplicate positioning.
- Blazor Server's hot reload (`dotnet watch`) provides a fast inner loop for iterating on the pixel-perfect layout, which is critical since the output will be screenshot-captured.
- The fixed 1920×1080 viewport specified in the design aligns perfectly with screenshot capture — the page should use a fixed-width layout, not a responsive one.
- A `data.json` file-watcher pattern (using `IFileSystemWatcher` or `FileSystemWatcher`) would allow live-reloading data without restarting the app, which is a valuable quality-of-life feature for the user editing JSON.
- No authentication, no database, no API, no middleware complexity — this is intentionally a "tool" not a "product."
- The heatmap grid has exactly 4 status rows × 4 month columns with a row header column — this is a fixed structure that maps cleanly to a nested `@foreach` in Razor.
- Total estimated implementation effort: **4–8 hours** for an engineer familiar with Blazor, including fake data generation and CSS polish.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components + CSS)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 | Server-side rendering with SignalR, single Razor page |
| **CSS Layout** | Pure CSS Grid + Flexbox | CSS3 | Matches `OriginalDesignConcept.html` grid exactly |
| **Timeline Visualization** | Inline SVG in Razor | SVG 1.1 | Milestone diamonds, progress lines, "NOW" marker |
| **Icons/Shapes** | Raw SVG markup | — | Diamond (`<polygon>`), circles (`<circle>`), lines |
| **Font** | Segoe UI (system font) | — | Already specified in design, no web font loading needed |

**Why no charting library (e.g., Radzen Charts, ApexCharts.Blazor, ChartJs.Blazor)?**
The timeline is not a standard chart — it's a custom SVG with specific diamond milestones at pixel-precise positions, colored progress lines per workstream, and a dashed "NOW" marker. Every charting library would fight you on custom positioning. The SVG is ~60 lines of markup. Writing it directly in Razor with `@` expressions for data-driven positions is simpler, more maintainable, and produces the exact output needed.

**Why no component library (e.g., MudBlazor, Radzen, Syncfusion)?**
The design is a bespoke executive report, not a forms-driven application. Component libraries add 500KB+ of CSS/JS, impose their own design language, and require overriding styles to match the spec. Pure CSS is lighter and gives pixel-perfect control. The total CSS needed is ~150 lines (it's already written in the HTML reference).

### Backend (Minimal)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Runtime** | .NET 8.0 SDK | 8.0.x (latest patch) | LTS release, Blazor Server hosting |
| **JSON Deserialization** | System.Text.Json | Built-in (.NET 8) | Deserialize `data.json` to C# models |
| **File Watching** | `FileSystemWatcher` | Built-in (.NET 8) | Optional: auto-reload when `data.json` changes |
| **Configuration** | `IConfiguration` + JSON provider | Built-in | App settings (port, file path) |

### Data Storage

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Primary Data** | `data.json` (flat file) | All dashboard data: milestones, heatmap items, metadata |
| **Schema Validation** | C# model classes with `required` properties | Compile-time safety, clear error messages on bad JSON |

**No database.** This is deliberate. The data set is small (dozens of items), changes infrequently (manually edited), and has no relational complexity. A database adds startup overhead, migration management, and connection string configuration — all unnecessary for a screenshot tool.

### Testing

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Unit Tests** | xUnit | 2.7.x | Model deserialization, date calculation logic |
| **Component Tests** | bUnit | 1.28.x+ | Blazor component rendering verification |
| **Snapshot Tests** | Verify | 24.x | Optional: catch unintended layout regressions |

### Development Tooling

| Tool | Purpose |
|------|---------|
| `dotnet watch` | Hot reload during development |
| Visual Studio 2022 / VS Code + C# Dev Kit | IDE |
| Browser DevTools (F12) | CSS Grid inspector, screenshot tool |

### Project Structure (.sln)

```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/           # Blazor Server project
│       ├── ReportingDashboard.csproj
│       ├── Program.cs                 # Minimal hosting setup
│       ├── Models/
│       │   └── DashboardData.cs       # Strongly-typed JSON models
│       ├── Services/
│       │   └── DataService.cs         # Reads & watches data.json
│       ├── Components/
│       │   ├── App.razor              # Root component
│       │   ├── Pages/
│       │   │   └── Dashboard.razor    # Single page — the entire dashboard
│       │   └── Layout/
│       │       └── MainLayout.razor   # Minimal layout wrapper
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css      # All styles (ported from HTML reference)
│       │   └── data.json              # Dashboard data file
│       └── Properties/
│           └── launchSettings.json
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── DataServiceTests.cs
```

---

## 4. Architecture Recommendations

### Pattern: Single-Component Page with Service-Injected Data

This project does not need MVC, MVVM, Clean Architecture, CQRS, or any layered pattern. The entire architecture is:

```
data.json → DataService → Dashboard.razor → Browser
```

**DataService** (registered as Singleton):
- Reads `data.json` on startup using `System.Text.Json.JsonSerializer.Deserialize<DashboardData>()`
- Optionally watches the file with `FileSystemWatcher` and raises a change event
- Exposes `DashboardData GetData()` method

**Dashboard.razor** (single page):
- Injects `DataService`
- Renders three visual sections:
  1. **Header** — Title, subtitle, legend icons
  2. **Timeline** — SVG with calculated positions for milestones
  3. **Heatmap Grid** — CSS Grid with data-driven cells

### Data Model Design

```csharp
public record DashboardData
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required DateOnly ReportDate { get; init; }
    public required string[] MonthColumns { get; init; }  // e.g., ["Jan", "Feb", "Mar", "Apr"]
    public required string CurrentMonth { get; init; }     // Highlights current column
    public required Workstream[] Workstreams { get; init; }
    public required StatusCategory[] Categories { get; init; }
}

public record Workstream
{
    public required string Id { get; init; }        // "M1", "M2", "M3"
    public required string Name { get; init; }       // "Chatbot & MS Role"
    public required string Color { get; init; }      // "#0078D4"
    public required Milestone[] Milestones { get; init; }
}

public record Milestone
{
    public required string Label { get; init; }      // "Mar 26 PoC"
    public required DateOnly Date { get; init; }
    public required string Type { get; init; }       // "poc" | "production" | "checkpoint" | "start"
}

public record StatusCategory
{
    public required string Name { get; init; }       // "Shipped", "In Progress", "Carryover", "Blockers"
    public required string CssClass { get; init; }   // "ship", "prog", "carry", "block"
    public required MonthItems[] Months { get; init; }
}

public record MonthItems
{
    public required string Month { get; init; }
    public required string[] Items { get; init; }
}
```

### SVG Timeline Calculation

The timeline maps dates to X-coordinates within the SVG viewport. The calculation is straightforward:

```
x = (date - startDate).TotalDays / (endDate - startDate).TotalDays * svgWidth
```

Where `svgWidth` is ~1560px (matching the original), `startDate` is the first month, and `endDate` is the last month. This is pure C# math in the Razor component — no JS interop needed.

### CSS Strategy

**Port the existing CSS directly.** The `OriginalDesignConcept.html` contains ~100 lines of well-structured CSS that already produces the exact desired output. The strategy is:

1. Copy the CSS into `dashboard.css` verbatim
2. Replace hardcoded content with Razor `@foreach` loops and `@` expressions
3. Use CSS custom properties (variables) for the color palette to enable easy theming if needed later
4. Keep the fixed `1920px` width — this is designed for screenshot capture, not responsive viewing

### Data Flow

```
Startup:
  Program.cs → builder.Services.AddSingleton<DataService>()
  DataService reads wwwroot/data.json → deserializes to DashboardData

Request:
  Browser → Blazor Server → Dashboard.razor
  Dashboard.razor calls DataService.GetData()
  Razor renders HTML + inline SVG
  CSS Grid + Flexbox handle layout
  Browser renders at 1920×1080

Optional file watch:
  User edits data.json
  FileSystemWatcher fires Changed event
  DataService reloads data
  Dashboard.razor re-renders via StateHasChanged()
```

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly a local-only, no-auth tool. The Blazor Server app binds to `localhost` only. No login, no roles, no tokens.

In `Program.cs`:
```csharp
app.Urls.Add("http://localhost:5050");
```

### Data Protection

- `data.json` contains project status data, not secrets. No encryption needed.
- If the data ever contains sensitive project names, the mitigation is filesystem ACLs (OS-level), not application-level encryption.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Host** | Local machine only — `dotnet run` or compiled executable |
| **Deployment** | `dotnet publish -c Release -o ./publish` → run `ReportingDashboard.exe` |
| **Self-contained** | Publish as self-contained (`-r win-x64 --self-contained`) for machines without .NET 8 SDK |
| **Port** | `http://localhost:5050` (configurable in `launchSettings.json`) |
| **Process** | Run on demand, not as a Windows service |

### Infrastructure Costs

**$0.** This runs on the developer's workstation. No servers, no cloud, no containers.

### Operational Concerns

- **Startup time:** <2 seconds for a minimal Blazor Server app
- **Memory:** ~50MB working set (Blazor Server baseline)
- **SignalR circuit:** Single user, single connection — negligible overhead
- **Hot reload:** `dotnet watch` works out of the box for CSS and Razor changes

---

## 6. Risks & Trade-offs

### Risk 1: CSS Pixel-Perfection Across Browsers (Medium)

**Risk:** The dashboard will be screenshotted for PowerPoint. Different browsers render CSS Grid, SVG text, and font metrics slightly differently. A layout that looks perfect in Edge may shift 1–2px in Chrome.

**Mitigation:** Standardize on a single browser for screenshots (recommend **Microsoft Edge** — Segoe UI renders best on Windows in Edge). Document this as a known constraint. Use the browser's built-in screenshot tool (`Ctrl+Shift+S` in Edge) or a fixed-viewport screenshot extension.

### Risk 2: data.json Schema Drift (Low)

**Risk:** As the dashboard evolves, the JSON schema may change, breaking existing `data.json` files.

**Mitigation:** Use `required` properties in C# records. Add a `"schemaVersion": 1` field to `data.json`. Validate on load and throw a clear error message pointing to the expected format.

### Risk 3: Blazor Server Overhead for a Static Page (Low, Acceptable)

**Trade-off:** Blazor Server maintains a SignalR WebSocket connection, which is overkill for what is essentially a static page. A pure Razor Pages or even a static HTML generator would have lower overhead.

**Why it's acceptable:** The overhead is negligible for single-user local use (~2MB memory for the circuit). Blazor Server provides hot reload, easy data binding, and a path to interactivity (e.g., editing data in the UI, filtering by date range) without a rewrite. The developer experience is worth the minor overhead.

### Risk 4: SVG Timeline Positioning Math (Low)

**Risk:** Date-to-pixel mapping could produce off-by-one errors or overlapping labels for milestones that are close together.

**Mitigation:** Use `DateOnly` arithmetic in C# (exact day-count division). Add a minimum pixel gap constant (e.g., 40px) and offset overlapping labels vertically. Test with edge cases: milestones on the same day, milestones at month boundaries.

### Risk 5: FileSystemWatcher Reliability on Windows (Low)

**Risk:** `FileSystemWatcher` can occasionally miss events or fire duplicates on Windows, especially with certain text editors that use atomic saves (write to temp file, then rename).

**Mitigation:** Debounce the file change handler (e.g., ignore events within 500ms of each other). Alternatively, skip file watching entirely and require a browser refresh — this is simpler and perfectly adequate for a screenshot tool.

---

## 7. Open Questions

| # | Question | Stakeholder | Impact |
|---|----------|------------|--------|
| 1 | **How many months should the heatmap display?** The current design shows 4 (Jan–Apr). Should this be configurable in `data.json`, or always show the last N months? | Product Owner | Affects grid column calculation and CSS |
| 2 | **Should the timeline span be auto-calculated from milestone dates, or manually specified?** Auto-calc is smarter but may produce awkward date ranges. | Product Owner | Affects SVG rendering logic |
| 3 | **Is there a need to support multiple projects in one instance?** E.g., a dropdown to switch between project JSON files, or always one project per instance? | User | Affects routing and data loading |
| 4 | **What browser will be used for screenshots?** This affects font rendering and CSS Grid behavior. Recommend standardizing on Edge. | User | Affects CSS testing |
| 5 | **Should the "NOW" line position be auto-calculated from system date, or specified in data.json?** Auto-calc is more convenient; manual gives control for generating historical snapshots. | User | Affects timeline logic |
| 6 | **Will the data.json be hand-edited, or is there a future need for a UI editor?** This determines whether we invest in JSON schema documentation or a Blazor form. | Product Owner | Affects Phase 2 scope |

---

## 8. Implementation Recommendations

### Phase 1: MVP (4–8 hours) — Screenshot-Ready Dashboard

**Goal:** A running Blazor Server app that renders the dashboard identically to the HTML reference, driven by `data.json`.

**Steps:**
1. **Scaffold project** — `dotnet new blazor --interactivity Server -n ReportingDashboard`
2. **Define C# models** — `DashboardData`, `Workstream`, `Milestone`, `StatusCategory` records
3. **Create DataService** — Reads `wwwroot/data.json` on startup, exposes typed data
4. **Port CSS** — Copy styles from `OriginalDesignConcept.html` into `dashboard.css`, convert to CSS custom properties for colors
5. **Build Dashboard.razor** — Single page with three sections: Header, Timeline SVG, Heatmap Grid
6. **Create sample data.json** — Fictional project ("Project Atlas") with realistic milestones and status items
7. **Test** — Verify visual match against the HTML reference at 1920×1080 in Edge

**Deliverable:** `dotnet run` → open `http://localhost:5050` → screenshot-ready page.

### Phase 2: Polish & Quality-of-Life (2–4 hours, optional)

- Add `FileSystemWatcher` for live data reload
- Add a print-friendly CSS `@media print` stylesheet
- Add basic error handling for malformed JSON with a user-friendly error page
- Add `schemaVersion` validation
- Improve SVG label collision detection (offset overlapping milestone labels)

### Phase 3: Future Enhancements (if needed)

- **In-browser data editor** — Blazor form to edit `data.json` fields without touching the file directly
- **Multi-project support** — Dropdown to switch between multiple JSON files in a `data/` directory
- **PDF export** — Use `Playwright` or `PuppeteerSharp` to programmatically capture the page as PDF
- **Historical snapshots** — Save dated copies of `data.json` and allow browsing past reports

### Quick Wins

1. **CSS custom properties for colors** — Enables instant theme changes (e.g., dark mode for projector presentations) by swapping ~10 variable values
2. **Auto-calculated "NOW" line** — `DateOnly.FromDateTime(DateTime.Today)` positions the red dashed line automatically, no manual JSON updates needed each month
3. **Current month highlighting** — The `.apr` CSS class (amber highlight on current month column) can be auto-applied based on system date
4. **Browser full-page screenshot** — Document the Edge shortcut (`Ctrl+Shift+S` → "Capture full page") in the README for instant PowerPoint-ready images

### NuGet Packages Required

```xml
<!-- ReportingDashboard.csproj — no additional NuGet packages needed for Phase 1 -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```

```xml
<!-- ReportingDashboard.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
    <PackageReference Include="bunit" Version="1.28.9" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
  </ItemGroup>
</Project>
```

**Zero external dependencies for the main project.** This is the strongest possible position for a simple, maintainable tool.

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
