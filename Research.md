# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-15 05:40 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The design is intentionally simple—optimized for taking 1920×1080 screenshots for PowerPoint decks. **Primary Recommendation:** Use a minimal Blazor Server app with zero third-party UI frameworks. The original HTML/CSS design is already pixel-perfect and production-ready. Translate it directly into Blazor components using inline SVG for the timeline and CSS Grid/Flexbox for the heatmap. Read `data.json` via `System.Text.Json` at startup. No database, no authentication, no API layer. This should be a single-project `.sln` with ~5 Razor components and one model file. ---

### Key Findings

- The original `OriginalDesignConcept.html` is a self-contained, well-structured design using CSS Grid (`grid-template-columns: 160px repeat(4,1fr)`), Flexbox, and inline SVG—all of which Blazor Server renders natively with zero JavaScript dependencies.
- **No charting library is needed.** The timeline uses hand-crafted SVG elements (lines, circles, polygons, text). This is simpler and more controllable than any charting library, and Blazor renders SVG natively in Razor components.
- **No database is needed.** A `data.json` file is the correct storage mechanism for this use case—it's human-editable, version-controllable, and trivially loaded with `System.Text.Json`.
- **No UI component library is needed.** MudBlazor, Radzen, and similar libraries would add 500KB+ of CSS/JS overhead and fight against the custom design. The design is simple enough that raw HTML/CSS in Razor components is the fastest and most maintainable approach.
- Blazor Server's SignalR circuit is irrelevant for this use case (single user, local only), but it does provide live-reload during development via `dotnet watch`, which accelerates iteration.
- The fixed 1920×1080 viewport in the design means **responsive design is unnecessary**. The layout should be fixed-width, matching the screenshot target resolution exactly.
- `System.Text.Json` (built into .NET 8) is the only serialization library needed. Do not use Newtonsoft.Json—it adds an unnecessary dependency.
- The color palette, font (Segoe UI), and spacing values are fully specified in the HTML reference and should be transferred directly into a scoped CSS file or `<style>` block. ---
- Create the `.sln` and Blazor Server project
- Port the HTML/CSS from `OriginalDesignConcept.html` directly into Razor components
- Hardcode sample data in the components (no JSON yet)
- Verify pixel-perfect match at 1920×1080
- **Deliverable:** Screenshot-ready static dashboard
- Define `DashboardData` C# models
- Create `data.json` with fictional project data
- Build `DashboardDataService` to load and deserialize
- Wire components to render from data instead of hardcoded values
- Dynamic SVG timeline (date-to-pixel mapping)
- Dynamic heatmap grid (rows and items from JSON)
- **Deliverable:** Fully data-driven dashboard
- Improve upon the original design: better spacing, subtle shadows, cleaner typography
- Add SVG label collision avoidance for overlapping milestones
- Add `@media print` styles if screenshots via browser print are desired
- Add a "last updated" timestamp to the footer
- **Deliverable:** Production-quality executive dashboard
- **Copy the CSS verbatim** from the HTML design file into `dashboard.css`. This gets you 80% of the visual fidelity in 5 minutes.
- **Use `dotnet watch`** during development for instant CSS/Razor reload without restarting the app.
- **Use browser DevTools screenshot** (Ctrl+Shift+P → "Capture full size screenshot" in Chrome) for pixel-perfect 1920×1080 captures.
- **Start with the heatmap grid** before the timeline—it's the highest-information-density section and validates the CSS Grid approach immediately.
- **SVG Timeline:** The date-to-pixel mapping and label positioning logic should be prototyped in isolation (a standalone Razor component with test data) before integrating into the full layout. Edge cases include: milestones on the same date, milestones at the far edges of the date range, and tracks with zero milestones.
```xml
<!-- ReportingDashboard.csproj — NONE beyond the default SDK -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>

<!-- ReportingDashboard.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="bunit" Version="1.31.3" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
  </ItemGroup>
</Project>
``` **The main project has zero NuGet dependencies beyond the .NET 8 SDK.** This is intentional and correct for this scope.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **UI Framework** | Blazor Server (built-in) | .NET 8.0 | Server-side rendering, component model | | **CSS Layout** | CSS Grid + Flexbox (native) | CSS3 | Heatmap grid, header layout, timeline area | | **Timeline Visualization** | Inline SVG in Razor | SVG 1.1 | Milestone diamonds, progress lines, date markers | | **Icons/Shapes** | SVG primitives | — | Diamond milestones (`<polygon>`), circles (`<circle>`), lines (`<line>`) | | **Font** | Segoe UI (system font) | — | Matches design spec; no web font loading needed | **Why no UI library:** MudBlazor (v7.x), Radzen (v5.x), and Syncfusion all provide chart and grid components, but they impose their own design systems. The original design is a custom layout that doesn't map to standard grid/chart widgets. Using a library would mean fighting its styles to match the design. Raw Razor + CSS is faster to build and easier to maintain for this scope. **Why no charting library:** The timeline is ~30 lines of SVG. Libraries like `BlazorApexCharts` (v3.x) or `ChartJs.Blazor` (v2.x) are designed for data-driven charts (bar, line, pie), not custom milestone timelines with diamonds, dashed "now" lines, and positioned labels. Hand-crafted SVG gives pixel-perfect control. | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Deserialize `data.json` | | **File I/O** | `System.IO` | Built into .NET 8 | Read `data.json` from disk | | **DI Service** | Custom `DashboardDataService` | — | Singleton service registered in DI, loads and caches data | | **Configuration** | `appsettings.json` | Built into .NET 8 | Path to `data.json`, optional settings | | Layer | Technology | Purpose | |-------|-----------|---------| | **Primary Store** | `data.json` (flat file) | All dashboard data: milestones, work items, metadata | | **Format** | JSON | Human-editable, version-controllable | | **Schema** | C# record types + `JsonSerializerOptions` | Type-safe deserialization with camelCase support | **No database.** SQLite, LiteDB, and similar would add unnecessary complexity. The data volume is tiny (dozens of items), updates are manual (edit the JSON), and there's no concurrent write scenario. | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **Unit Tests** | xUnit | 2.9.x | Model deserialization, date calculations | | **Component Tests** | bUnit | 1.31.x | Blazor component rendering verification | | **Assertions** | FluentAssertions | 7.x | Readable test assertions | | Tool | Version | Purpose | |------|---------|---------| | **SDK** | .NET 8.0.x (latest patch) | Build, run, publish | | **Hot Reload** | `dotnet watch` (built-in) | Live reload during CSS/Razor iteration | | **IDE** | Visual Studio 2022 17.9+ or VS Code + C# Dev Kit | Razor editing, IntelliSense | --- This is not a microservices project. It's not even a multi-layer project. The entire app should live in **one project** with this structure:
```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Components/
│       │   ├── App.razor
│       │   ├── Pages/
│       │   │   └── Dashboard.razor          ← Main (only) page
│       │   └── Layout/
│       │       ├── MainLayout.razor
│       │       ├── DashboardHeader.razor     ← Title, subtitle, legend
│       │       ├── TimelineSection.razor     ← SVG milestone timeline
│       │       └── HeatmapGrid.razor         ← Status × Month grid
│       ├── Models/
│       │   └── DashboardData.cs             ← C# records for JSON shape
│       ├── Services/
│       │   └── DashboardDataService.cs      ← Loads & caches data.json
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css            ← All custom styles (from HTML design)
│       │   └── data.json                    ← Dashboard data file
│       └── Properties/
│           └── launchSettings.json
└── tests/
    └── ReportingDashboard.Tests/
        └── ReportingDashboard.Tests.csproj
```
```
data.json (wwwroot or configurable path)
    ↓  Read at startup (or on-demand with caching)
DashboardDataService (Singleton, registered in DI)
    ↓  Injected into Razor components
Dashboard.razor → DashboardHeader.razor
                → TimelineSection.razor (renders SVG from milestone data)
                → HeatmapGrid.razor (renders CSS Grid from work items)
``` Map directly to the visual sections in the HTML design: | Component | Responsibility | Key Rendering | |-----------|---------------|---------------| | `DashboardHeader` | Project title, subtitle, legend icons | Flexbox row, styled spans | | `TimelineSection` | SVG milestone visualization | `<svg>` with `<line>`, `<circle>`, `<polygon>`, `<text>` | | `HeatmapGrid` | Status × Month grid with work items | CSS Grid, color-coded cells | | `Dashboard` | Page composition | Stacks the three sections vertically |
```csharp
public record DashboardData
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineTrack> Tracks { get; init; }
    public HeatmapData Heatmap { get; init; }
}

public record ProjectInfo
{
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string BacklogUrl { get; init; }
    public string CurrentMonth { get; init; }  // e.g., "April 2026"
}

public record Milestone
{
    public string Label { get; init; }
    public string Date { get; init; }
    public string Type { get; init; }  // "poc", "production", "checkpoint"
    public string TrackId { get; init; }
}

public record TimelineTrack
{
    public string Id { get; init; }
    public string Label { get; init; }
    public string Sublabel { get; init; }
    public string Color { get; init; }
    public List<Milestone> Milestones { get; init; }
}

public record HeatmapData
{
    public List<string> Columns { get; init; }  // Month names
    public string HighlightColumn { get; init; }  // Current month
    public List<HeatmapRow> Rows { get; init; }
}

public record HeatmapRow
{
    public string Category { get; init; }  // "Shipped", "In Progress", etc.
    public string ColorTheme { get; init; }  // "green", "blue", "amber", "red"
    public Dictionary<string, List<string>> Items { get; init; }  // month → items
}
``` The timeline SVG should be generated dynamically from data, not hardcoded. Key calculations:
- **Time-to-X mapping:** Define a date range (e.g., Jan 1 – Jun 30). Map each milestone's date to an X coordinate: `x = (date - startDate) / (endDate - startDate) * svgWidth`.
- **"Now" line:** Calculate position of current date using the same mapping. Render as a dashed red vertical line.
- **Track Y positions:** Each track gets a fixed Y offset (e.g., track 1 at y=42, track 2 at y=98, track 3 at y=154).
- **Milestone shapes:** Render as `<polygon>` (diamond) for PoC/Production milestones, `<circle>` for checkpoints. This is straightforward arithmetic in a Razor component—no library needed. **Transfer the CSS from the HTML design file directly** into `dashboard.css`. Key patterns to preserve:
- `body` fixed at `width: 1920px; height: 1080px; overflow: hidden` — this ensures screenshot-perfect output
- CSS Grid for heatmap: `grid-template-columns: 160px repeat(4, 1fr)`
- Color classes per row category (`.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`)
- The `::before` pseudo-element pattern for bullet dots on work items **Do not use CSS isolation (`.razor.css` files)** for the main layout styles. The design is page-level and interconnected. A single `dashboard.css` is simpler and matches the source HTML design 1:1. ---

### Considerations & Risks

- **None.** This is explicitly a local-only, no-auth application. Do not add ASP.NET Identity, cookie auth, or any middleware. The app runs on `localhost` and is accessed by the person running it. If future requirements add auth, the simplest path would be a shared secret via `appsettings.json` checked in middleware—but do not build this now.
- `data.json` contains project status information, not PII or secrets. No encryption needed.
- If the JSON ever contains sensitive data, use DPAPI via `Microsoft.AspNetCore.DataProtection` (built into .NET 8) for at-rest encryption. But this is unlikely given the use case. | Concern | Approach | |---------|----------| | **Runtime** | `dotnet run` locally, or `dotnet publish` → run the executable | | **Port** | Default Kestrel on `https://localhost:5001` or `http://localhost:5000` | | **Deployment** | Self-contained publish: `dotnet publish -c Release -r win-x64 --self-contained` | | **Distribution** | Zip the publish output; recipient runs the `.exe` directly | | **No containers** | Docker is unnecessary for a single-user local app | | **No reverse proxy** | No IIS, no Nginx—Kestrel is sufficient | **$0.** This runs on the developer's workstation. No cloud, no hosting, no licenses. --- | Risk | Severity | Mitigation | |------|----------|------------| | **Blazor Server requires a persistent SignalR connection** | Low | Irrelevant for local use. The connection is localhost-to-localhost. No network latency or disconnection concerns. | | **SVG timeline rendering edge cases** | Medium | Milestones that overlap in time may produce overlapping labels. Implement a simple collision-avoidance offset (shift label above/below track line). | | **JSON schema drift** | Low | Define C# record types as the schema. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` for resilience. Add a validation step at load time. | | **CSS specificity conflicts if a UI library is later added** | Low | Avoided entirely by not using a UI library. | | **Fixed 1920×1080 layout breaks on smaller screens** | Low | This is intentional—the app is designed for screenshots, not responsive use. Document this clearly. | | Decision | Trade-off | Rationale | |----------|-----------|-----------| | No UI component library | Must write all CSS by hand | Design is custom; library CSS would conflict and add bloat | | No database | Can't query historical data | Data volume is tiny; JSON is simpler; history can be managed via Git | | No charting library | Must calculate SVG positions manually | Timeline is ~30 lines of SVG math; no library provides this exact visualization | | Single-project structure | No separation of concerns via project boundaries | App has ~5 components and 1 service; multiple projects would be over-engineering | | Blazor Server (not Static SSR or WASM) | Requires `dotnet` process running | Provides hot reload, simpler development model, and component interactivity if needed later | The only performance-sensitive operation is **initial page load**, which involves reading `data.json` and rendering ~50 SVG elements + ~20 grid cells. This will complete in <50ms. There are no scalability concerns. --- | # | Question | Stakeholder | Impact | |---|----------|-------------|--------| | 1 | **How many months should the heatmap display?** The HTML design shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always the current month and 3 prior? | Product Owner | Affects grid column count and CSS | | 2 | **How many timeline tracks?** The design shows 3 (M1, M2, M3). Should the app support N tracks dynamically, or is 3 fixed? | Product Owner | Affects SVG height calculation | | 3 | **Should `data.json` support multiple projects?** Currently scoped to one project per file. If multiple dashboards are needed, should the app accept a query parameter for which JSON to load? | Product Owner | Affects routing and data loading | | 4 | **What is the "now" line behavior?** Should it always reflect `DateTime.Now`, or should `data.json` specify a report date (e.g., for generating historical snapshots)? | Product Owner | Affects timeline rendering logic | | 5 | **Will the app ever need to be shared on a network?** If yes, we'd need to bind Kestrel to `0.0.0.0` instead of `localhost` and potentially add basic auth. | Technical Lead | Affects hosting configuration | | 6 | **Print/export support?** The current plan is screenshots. Should the app include a "Print to PDF" button or `@media print` styles? | Product Owner | Adds ~2 hours of CSS work | ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The design is intentionally simple—optimized for taking 1920×1080 screenshots for PowerPoint decks.

**Primary Recommendation:** Use a minimal Blazor Server app with zero third-party UI frameworks. The original HTML/CSS design is already pixel-perfect and production-ready. Translate it directly into Blazor components using inline SVG for the timeline and CSS Grid/Flexbox for the heatmap. Read `data.json` via `System.Text.Json` at startup. No database, no authentication, no API layer. This should be a single-project `.sln` with ~5 Razor components and one model file.

---

## 2. Key Findings

- The original `OriginalDesignConcept.html` is a self-contained, well-structured design using CSS Grid (`grid-template-columns: 160px repeat(4,1fr)`), Flexbox, and inline SVG—all of which Blazor Server renders natively with zero JavaScript dependencies.
- **No charting library is needed.** The timeline uses hand-crafted SVG elements (lines, circles, polygons, text). This is simpler and more controllable than any charting library, and Blazor renders SVG natively in Razor components.
- **No database is needed.** A `data.json` file is the correct storage mechanism for this use case—it's human-editable, version-controllable, and trivially loaded with `System.Text.Json`.
- **No UI component library is needed.** MudBlazor, Radzen, and similar libraries would add 500KB+ of CSS/JS overhead and fight against the custom design. The design is simple enough that raw HTML/CSS in Razor components is the fastest and most maintainable approach.
- Blazor Server's SignalR circuit is irrelevant for this use case (single user, local only), but it does provide live-reload during development via `dotnet watch`, which accelerates iteration.
- The fixed 1920×1080 viewport in the design means **responsive design is unnecessary**. The layout should be fixed-width, matching the screenshot target resolution exactly.
- `System.Text.Json` (built into .NET 8) is the only serialization library needed. Do not use Newtonsoft.Json—it adds an unnecessary dependency.
- The color palette, font (Segoe UI), and spacing values are fully specified in the HTML reference and should be transferred directly into a scoped CSS file or `<style>` block.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 | Server-side rendering, component model |
| **CSS Layout** | CSS Grid + Flexbox (native) | CSS3 | Heatmap grid, header layout, timeline area |
| **Timeline Visualization** | Inline SVG in Razor | SVG 1.1 | Milestone diamonds, progress lines, date markers |
| **Icons/Shapes** | SVG primitives | — | Diamond milestones (`<polygon>`), circles (`<circle>`), lines (`<line>`) |
| **Font** | Segoe UI (system font) | — | Matches design spec; no web font loading needed |

**Why no UI library:** MudBlazor (v7.x), Radzen (v5.x), and Syncfusion all provide chart and grid components, but they impose their own design systems. The original design is a custom layout that doesn't map to standard grid/chart widgets. Using a library would mean fighting its styles to match the design. Raw Razor + CSS is faster to build and easier to maintain for this scope.

**Why no charting library:** The timeline is ~30 lines of SVG. Libraries like `BlazorApexCharts` (v3.x) or `ChartJs.Blazor` (v2.x) are designed for data-driven charts (bar, line, pie), not custom milestone timelines with diamonds, dashed "now" lines, and positioned labels. Hand-crafted SVG gives pixel-perfect control.

### Backend (Data Loading)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Deserialize `data.json` |
| **File I/O** | `System.IO` | Built into .NET 8 | Read `data.json` from disk |
| **DI Service** | Custom `DashboardDataService` | — | Singleton service registered in DI, loads and caches data |
| **Configuration** | `appsettings.json` | Built into .NET 8 | Path to `data.json`, optional settings |

### Data Storage

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Primary Store** | `data.json` (flat file) | All dashboard data: milestones, work items, metadata |
| **Format** | JSON | Human-editable, version-controllable |
| **Schema** | C# record types + `JsonSerializerOptions` | Type-safe deserialization with camelCase support |

**No database.** SQLite, LiteDB, and similar would add unnecessary complexity. The data volume is tiny (dozens of items), updates are manual (edit the JSON), and there's no concurrent write scenario.

### Testing

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Unit Tests** | xUnit | 2.9.x | Model deserialization, date calculations |
| **Component Tests** | bUnit | 1.31.x | Blazor component rendering verification |
| **Assertions** | FluentAssertions | 7.x | Readable test assertions |

### Development Tooling

| Tool | Version | Purpose |
|------|---------|---------|
| **SDK** | .NET 8.0.x (latest patch) | Build, run, publish |
| **Hot Reload** | `dotnet watch` (built-in) | Live reload during CSS/Razor iteration |
| **IDE** | Visual Studio 2022 17.9+ or VS Code + C# Dev Kit | Razor editing, IntelliSense |

---

## 4. Architecture Recommendations

### Overall Pattern: **Minimal Single-Project Blazor Server App**

This is not a microservices project. It's not even a multi-layer project. The entire app should live in **one project** with this structure:

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Components/
│       │   ├── App.razor
│       │   ├── Pages/
│       │   │   └── Dashboard.razor          ← Main (only) page
│       │   └── Layout/
│       │       ├── MainLayout.razor
│       │       ├── DashboardHeader.razor     ← Title, subtitle, legend
│       │       ├── TimelineSection.razor     ← SVG milestone timeline
│       │       └── HeatmapGrid.razor         ← Status × Month grid
│       ├── Models/
│       │   └── DashboardData.cs             ← C# records for JSON shape
│       ├── Services/
│       │   └── DashboardDataService.cs      ← Loads & caches data.json
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css            ← All custom styles (from HTML design)
│       │   └── data.json                    ← Dashboard data file
│       └── Properties/
│           └── launchSettings.json
└── tests/
    └── ReportingDashboard.Tests/
        └── ReportingDashboard.Tests.csproj
```

### Data Flow

```
data.json (wwwroot or configurable path)
    ↓  Read at startup (or on-demand with caching)
DashboardDataService (Singleton, registered in DI)
    ↓  Injected into Razor components
Dashboard.razor → DashboardHeader.razor
                → TimelineSection.razor (renders SVG from milestone data)
                → HeatmapGrid.razor (renders CSS Grid from work items)
```

### Component Design

Map directly to the visual sections in the HTML design:

| Component | Responsibility | Key Rendering |
|-----------|---------------|---------------|
| `DashboardHeader` | Project title, subtitle, legend icons | Flexbox row, styled spans |
| `TimelineSection` | SVG milestone visualization | `<svg>` with `<line>`, `<circle>`, `<polygon>`, `<text>` |
| `HeatmapGrid` | Status × Month grid with work items | CSS Grid, color-coded cells |
| `Dashboard` | Page composition | Stacks the three sections vertically |

### Data Model (C# Records)

```csharp
public record DashboardData
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineTrack> Tracks { get; init; }
    public HeatmapData Heatmap { get; init; }
}

public record ProjectInfo
{
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string BacklogUrl { get; init; }
    public string CurrentMonth { get; init; }  // e.g., "April 2026"
}

public record Milestone
{
    public string Label { get; init; }
    public string Date { get; init; }
    public string Type { get; init; }  // "poc", "production", "checkpoint"
    public string TrackId { get; init; }
}

public record TimelineTrack
{
    public string Id { get; init; }
    public string Label { get; init; }
    public string Sublabel { get; init; }
    public string Color { get; init; }
    public List<Milestone> Milestones { get; init; }
}

public record HeatmapData
{
    public List<string> Columns { get; init; }  // Month names
    public string HighlightColumn { get; init; }  // Current month
    public List<HeatmapRow> Rows { get; init; }
}

public record HeatmapRow
{
    public string Category { get; init; }  // "Shipped", "In Progress", etc.
    public string ColorTheme { get; init; }  // "green", "blue", "amber", "red"
    public Dictionary<string, List<string>> Items { get; init; }  // month → items
}
```

### SVG Timeline Rendering Strategy

The timeline SVG should be generated dynamically from data, not hardcoded. Key calculations:

1. **Time-to-X mapping:** Define a date range (e.g., Jan 1 – Jun 30). Map each milestone's date to an X coordinate: `x = (date - startDate) / (endDate - startDate) * svgWidth`.
2. **"Now" line:** Calculate position of current date using the same mapping. Render as a dashed red vertical line.
3. **Track Y positions:** Each track gets a fixed Y offset (e.g., track 1 at y=42, track 2 at y=98, track 3 at y=154).
4. **Milestone shapes:** Render as `<polygon>` (diamond) for PoC/Production milestones, `<circle>` for checkpoints.

This is straightforward arithmetic in a Razor component—no library needed.

### CSS Strategy

**Transfer the CSS from the HTML design file directly** into `dashboard.css`. Key patterns to preserve:

- `body` fixed at `width: 1920px; height: 1080px; overflow: hidden` — this ensures screenshot-perfect output
- CSS Grid for heatmap: `grid-template-columns: 160px repeat(4, 1fr)`
- Color classes per row category (`.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`)
- The `::before` pseudo-element pattern for bullet dots on work items

**Do not use CSS isolation (`.razor.css` files)** for the main layout styles. The design is page-level and interconnected. A single `dashboard.css` is simpler and matches the source HTML design 1:1.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly a local-only, no-auth application. Do not add ASP.NET Identity, cookie auth, or any middleware. The app runs on `localhost` and is accessed by the person running it.

If future requirements add auth, the simplest path would be a shared secret via `appsettings.json` checked in middleware—but do not build this now.

### Data Protection

- `data.json` contains project status information, not PII or secrets. No encryption needed.
- If the JSON ever contains sensitive data, use DPAPI via `Microsoft.AspNetCore.DataProtection` (built into .NET 8) for at-rest encryption. But this is unlikely given the use case.

### Hosting & Deployment

| Concern | Approach |
|---------|----------|
| **Runtime** | `dotnet run` locally, or `dotnet publish` → run the executable |
| **Port** | Default Kestrel on `https://localhost:5001` or `http://localhost:5000` |
| **Deployment** | Self-contained publish: `dotnet publish -c Release -r win-x64 --self-contained` |
| **Distribution** | Zip the publish output; recipient runs the `.exe` directly |
| **No containers** | Docker is unnecessary for a single-user local app |
| **No reverse proxy** | No IIS, no Nginx—Kestrel is sufficient |

### Infrastructure Costs

**$0.** This runs on the developer's workstation. No cloud, no hosting, no licenses.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **Blazor Server requires a persistent SignalR connection** | Low | Irrelevant for local use. The connection is localhost-to-localhost. No network latency or disconnection concerns. |
| **SVG timeline rendering edge cases** | Medium | Milestones that overlap in time may produce overlapping labels. Implement a simple collision-avoidance offset (shift label above/below track line). |
| **JSON schema drift** | Low | Define C# record types as the schema. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` for resilience. Add a validation step at load time. |
| **CSS specificity conflicts if a UI library is later added** | Low | Avoided entirely by not using a UI library. |
| **Fixed 1920×1080 layout breaks on smaller screens** | Low | This is intentional—the app is designed for screenshots, not responsive use. Document this clearly. |

### Trade-offs Made

| Decision | Trade-off | Rationale |
|----------|-----------|-----------|
| No UI component library | Must write all CSS by hand | Design is custom; library CSS would conflict and add bloat |
| No database | Can't query historical data | Data volume is tiny; JSON is simpler; history can be managed via Git |
| No charting library | Must calculate SVG positions manually | Timeline is ~30 lines of SVG math; no library provides this exact visualization |
| Single-project structure | No separation of concerns via project boundaries | App has ~5 components and 1 service; multiple projects would be over-engineering |
| Blazor Server (not Static SSR or WASM) | Requires `dotnet` process running | Provides hot reload, simpler development model, and component interactivity if needed later |

### Potential Bottleneck

The only performance-sensitive operation is **initial page load**, which involves reading `data.json` and rendering ~50 SVG elements + ~20 grid cells. This will complete in <50ms. There are no scalability concerns.

---

## 7. Open Questions

| # | Question | Stakeholder | Impact |
|---|----------|-------------|--------|
| 1 | **How many months should the heatmap display?** The HTML design shows 4 months (Jan–Apr). Should this be configurable in `data.json`, or always the current month and 3 prior? | Product Owner | Affects grid column count and CSS |
| 2 | **How many timeline tracks?** The design shows 3 (M1, M2, M3). Should the app support N tracks dynamically, or is 3 fixed? | Product Owner | Affects SVG height calculation |
| 3 | **Should `data.json` support multiple projects?** Currently scoped to one project per file. If multiple dashboards are needed, should the app accept a query parameter for which JSON to load? | Product Owner | Affects routing and data loading |
| 4 | **What is the "now" line behavior?** Should it always reflect `DateTime.Now`, or should `data.json` specify a report date (e.g., for generating historical snapshots)? | Product Owner | Affects timeline rendering logic |
| 5 | **Will the app ever need to be shared on a network?** If yes, we'd need to bind Kestrel to `0.0.0.0` instead of `localhost` and potentially add basic auth. | Technical Lead | Affects hosting configuration |
| 6 | **Print/export support?** The current plan is screenshots. Should the app include a "Print to PDF" button or `@media print` styles? | Product Owner | Adds ~2 hours of CSS work |

---

## 8. Implementation Recommendations

### Phasing

#### Phase 1: Static Layout (Day 1) — MVP
- Create the `.sln` and Blazor Server project
- Port the HTML/CSS from `OriginalDesignConcept.html` directly into Razor components
- Hardcode sample data in the components (no JSON yet)
- Verify pixel-perfect match at 1920×1080
- **Deliverable:** Screenshot-ready static dashboard

#### Phase 2: Data-Driven Rendering (Day 1–2)
- Define `DashboardData` C# models
- Create `data.json` with fictional project data
- Build `DashboardDataService` to load and deserialize
- Wire components to render from data instead of hardcoded values
- Dynamic SVG timeline (date-to-pixel mapping)
- Dynamic heatmap grid (rows and items from JSON)
- **Deliverable:** Fully data-driven dashboard

#### Phase 3: Polish & Improvements (Day 2–3)
- Improve upon the original design: better spacing, subtle shadows, cleaner typography
- Add SVG label collision avoidance for overlapping milestones
- Add `@media print` styles if screenshots via browser print are desired
- Add a "last updated" timestamp to the footer
- **Deliverable:** Production-quality executive dashboard

### Quick Wins

1. **Copy the CSS verbatim** from the HTML design file into `dashboard.css`. This gets you 80% of the visual fidelity in 5 minutes.
2. **Use `dotnet watch`** during development for instant CSS/Razor reload without restarting the app.
3. **Use browser DevTools screenshot** (Ctrl+Shift+P → "Capture full size screenshot" in Chrome) for pixel-perfect 1920×1080 captures.
4. **Start with the heatmap grid** before the timeline—it's the highest-information-density section and validates the CSS Grid approach immediately.

### Areas Where Prototyping Is Recommended

- **SVG Timeline:** The date-to-pixel mapping and label positioning logic should be prototyped in isolation (a standalone Razor component with test data) before integrating into the full layout. Edge cases include: milestones on the same date, milestones at the far edges of the date range, and tracks with zero milestones.

### NuGet Packages Required

```xml
<!-- ReportingDashboard.csproj — NONE beyond the default SDK -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>

<!-- ReportingDashboard.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="bunit" Version="1.31.3" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
  </ItemGroup>
</Project>
```

**The main project has zero NuGet dependencies beyond the .NET 8 SDK.** This is intentional and correct for this scope.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/e839069812bb99ffef5c922d2bfebfe68558e8e5/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
