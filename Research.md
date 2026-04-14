# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-14 00:45 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard renders a timeline/Gantt view of project milestones and a heatmap grid showing shipped, in-progress, carryover, and blocker items — designed for 1920×1080 screenshot capture for PowerPoint decks. **Primary Recommendation:** Use a minimal Blazor Server app with zero JavaScript framework dependencies. Render the timeline SVG directly in Razor components, style with scoped CSS matching the design spec, and read all data from a local `data.json` file at startup. No database, no auth, no external services. The entire solution should be a single `.sln` with one Blazor Server project, deployable via `dotnet run`. This is deliberately a simple, high-fidelity rendering tool — not a full enterprise application. Every technology choice prioritizes simplicity, visual accuracy, and ease of screenshot capture. ---

### Key Findings

- The original `OriginalDesignConcept.html` is a static 1920×1080 layout using CSS Grid, Flexbox, and inline SVG — all of which Blazor Server can reproduce natively without third-party charting libraries.
- Blazor Server in .NET 8 supports static SSR (server-side rendering) which eliminates SignalR overhead for a read-only dashboard, though interactive mode is fine for local use.
- No database is needed; `System.Text.Json` deserialization of a local `data.json` file is sufficient and eliminates all data layer complexity.
- No authentication or authorization is required per the project scope — this is a local tool for one user.
- SVG generation in Razor components is straightforward and avoids any npm/JavaScript build pipeline.
- CSS isolation (scoped CSS per component) in Blazor is mature in .NET 8 and maps perfectly to the design's section-based styling.
- The design's color palette, grid layout, and typography can be implemented with pure CSS — no CSS framework (Bootstrap, Tailwind) is needed or recommended.
- The entire project can be scaffolded and running in under an hour given the constrained scope.
- Screenshot fidelity is best achieved by keeping the layout at a fixed 1920×1080 viewport with `overflow:hidden`, matching the original HTML design exactly. ---
- `dotnet new blazor -n ReportingDashboard --interactivity Server`
- Create solution: `dotnet new sln` → `dotnet sln add src/ReportingDashboard`
- Define `DashboardData` model classes (records)
- Create `data.json` with fictional project data
- Create `DashboardDataService` that reads and deserializes `data.json`
- Build `Dashboard.razor` as a single monolithic component first — get the layout right
- Match the CSS from `OriginalDesignConcept.html` line-by-line
- Extract `DashboardHeader`, `TimelineSection`, `HeatmapSection` components
- Move inline styles to scoped `.razor.css` files
- Extract CSS variables to `app.css` or a shared stylesheet
- Verify visual fidelity against original design at 1920×1080
- Build `TimelineCalculator` helper (date-to-pixel mapping)
- Implement `TimelineLane` and `MilestoneMarker` components
- Render the "Now" indicator line
- Test with varying date ranges to ensure robustness
- Create `data.template.json` with inline comments explaining each field
- Add `README.md` with setup and screenshot instructions
- Optional: Add `FileSystemWatcher` for live data reload
- Optional: Add `dotnet publish` script for single-file distribution
- **Immediate visual impact**: Get the heatmap grid rendering first — it's the largest visual area and uses simple CSS Grid. This validates the approach in minutes.
- **Fictional data**: Use a fun project name (e.g., "Project Phoenix — Next-Gen Customer Portal") with realistic milestones to make the demo compelling.
- **Screenshot helper**: Add a `/screenshot` route that strips all padding and renders at exactly 1920×1080 for clean capture.
```xml
<!-- ReportingDashboard.csproj — no additional packages beyond the default template -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
``` **Zero additional NuGet packages.** Everything needed — Blazor, JSON serialization, Kestrel, CSS isolation — ships in the .NET 8 SDK. This is the simplest possible dependency footprint.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Ships with `Microsoft.AspNetCore.App`. No additional packages needed. | | **Rendering** | Razor Components (.razor) | Built-in | Component-per-section architecture. | | **Styling** | Scoped CSS (`.razor.css`) | Built-in | CSS isolation ships with Blazor. No preprocessor needed. | | **Layout Engine** | CSS Grid + Flexbox | Native CSS | Matches original design exactly. Grid for heatmap, Flex for header/timeline. | | **Timeline/Gantt** | Inline SVG in Razor | Native | Generate `<svg>` elements directly in `.razor` files with C# loop logic. No charting library. | | **Icons/Shapes** | Inline SVG primitives | Native | Diamonds (`<polygon>`), circles (`<circle>`), lines — all from the original design. | | **Font** | Segoe UI | System font | Already specified in design. No web font loading needed (available on Windows). | **Why no charting library?** The timeline is a simple horizontal bar chart with milestone markers. Libraries like `Radzen`, `MudBlazor`, or `ApexCharts.Blazor` add 200KB+ of JavaScript and impose their own styling opinions. The original HTML achieves the entire visualization in ~40 lines of SVG. Reproducing this directly in Razor is simpler, lighter, and gives pixel-perfect control. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Data Format** | JSON file (`data.json`) | — | Single flat file in project root or `wwwroot/data/`. | | **Deserialization** | `System.Text.Json` | Built-in (.NET 8) | Zero additional packages. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`. | | **Data Service** | Singleton `DashboardDataService` | Custom | Reads and caches `data.json` on startup. Registered in DI as singleton. | | **File Watching (optional)** | `FileSystemWatcher` | Built-in (.NET 8) | Auto-reload data when `data.json` is edited. Nice for iterating on content. | **No database.** SQLite, LiteDB, or any persistence layer would add complexity with zero benefit. The data is read-only, small (< 100 items), and edited manually in JSON. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **SDK** | .NET 8.0 SDK | 8.0.x LTS | `dotnet new blazorserver` or `dotnet new blazor` (unified in .NET 8). | | **Solution** | `.sln` with single `.csproj` | — | `ReportingDashboard.sln` → `src/ReportingDashboard/ReportingDashboard.csproj` | | **Launch** | `dotnet run` | — | Opens on `https://localhost:5001` or configurable port. | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Tests** | xUnit | 2.7.x | For data model deserialization tests. | | **Component Tests** | bUnit | 1.28.x+ | For verifying Razor component rendering. Optional given project simplicity. | | **Snapshot Tests** | Verify | 24.x | Optional: snapshot-test rendered HTML for regression. | Given the project's simplicity (read-only, single-page, local-only), a dedicated test project is optional. If added, a single `ReportingDashboard.Tests.csproj` in the solution suffices. ---
```
data.json
   ↓ (read at startup)
DashboardDataService (singleton, DI-registered)
   ↓ (injected into components)
Razor Components (.razor + .razor.css)
   ↓ (rendered server-side)
Browser (1920×1080 viewport)
   ↓ (screenshot)
PowerPoint deck
``` No MVC, no MVVM, no CQRS — those patterns are for applications with user interaction, state mutation, and business logic. This is a **read-only rendering pipeline**.
```
App.razor
└── MainLayout.razor
    └── Dashboard.razor (the single page)
        ├── DashboardHeader.razor        ← Title, subtitle, legend
        ├── TimelineSection.razor        ← Milestone swim lanes + SVG
        │   ├── TimelineLane.razor       ← One horizontal track per milestone group
        │   └── MilestoneMarker.razor    ← Diamond/circle/checkpoint SVG elements
        ├── HeatmapSection.razor         ← Grid container
        │   ├── HeatmapHeader.razor      ← Month column headers
        │   └── HeatmapRow.razor         ← One row per category (Shipped/InProgress/Carryover/Blockers)
        │       └── HeatmapCell.razor    ← Individual cell with item bullets
        └── (optional) FooterSection.razor
``` Each component gets a `.razor.css` scoped stylesheet. This keeps styles isolated and maintainable.
```csharp
public record DashboardData
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogLink { get; init; } = "";
    public DateOnly ReportDate { get; init; }
    public List<MilestoneTrack> Tracks { get; init; } = [];
    public List<HeatmapCategory> Categories { get; init; } = [];
    public List<string> Months { get; init; } = []; // Column headers
}

public record MilestoneTrack
{
    public string Id { get; init; } = "";        // e.g., "M1"
    public string Label { get; init; } = "";      // e.g., "Chatbot & MS Role"
    public string Color { get; init; } = "";      // e.g., "#0078D4"
    public List<Milestone> Milestones { get; init; } = [];
}

public record Milestone
{
    public DateOnly Date { get; init; }
    public string Label { get; init; } = "";
    public string Type { get; init; } = "";       // "poc", "production", "checkpoint", "start"
}

public record HeatmapCategory
{
    public string Name { get; init; } = "";       // "Shipped", "In Progress", etc.
    public string Type { get; init; } = "";       // "shipped", "in-progress", "carryover", "blockers"
    public Dictionary<string, List<string>> ItemsByMonth { get; init; } = new();
}
``` The timeline is the most complex visual element. Recommended approach:
- **Calculate pixel positions** in C# based on date ranges: `pixelX = (date - startDate).Days / totalDays * svgWidth`
- **Render SVG elements** in a `@foreach` loop inside a `<svg>` tag in Razor
- **Use the exact SVG primitives** from the original HTML: `<line>`, `<circle>`, `<polygon>`, `<text>`
- **"Now" indicator**: Calculate position from `ReportDate` and render a dashed red vertical line This approach gives sub-pixel positioning control and matches the original design exactly.
- **No CSS framework.** Bootstrap, MudBlazor, or Tailwind would fight the custom design. The original HTML uses ~80 lines of hand-written CSS. Replicate it.
- **Use CSS custom properties** for the color palette (makes theming trivial):
  ```css
  :root {
      --color-shipped: #34A853;
      --color-shipped-bg: #F0FBF0;
      --color-progress: #0078D4;
      --color-progress-bg: #EEF4FE;
      --color-carryover: #F4B400;
      --color-carryover-bg: #FFFDE7;
      --color-blocker: #EA4335;
      --color-blocker-bg: #FFF5F5;
  }
  ```
- **Fixed viewport**: Set `body { width: 1920px; height: 1080px; overflow: hidden; }` for screenshot-perfect rendering.
- **CSS Grid** for the heatmap: `grid-template-columns: 160px repeat(N, 1fr)` where N comes from data.
- **Flexbox** for header and timeline layout areas. ---

### Considerations & Risks

- **None.** Per project requirements, this is a local-only tool with no auth. The Blazor Server app binds to `localhost` only. If you want to prevent accidental exposure:
```csharp
// Program.cs
builder.WebHost.UseUrls("https://localhost:5001");
``` This ensures the dashboard is only accessible from the local machine.
- `data.json` contains project status information, not PII or secrets.
- No encryption needed. No HTTPS certificate management needed for local use (development cert is fine).
- If the JSON contains sensitive project names, add `data.json` to `.gitignore` and ship a `data.template.json` with example data. | Aspect | Recommendation | |--------|---------------| | **Runtime** | `dotnet run` from command line, or publish as self-contained single-file executable | | **Self-Contained Publish** | `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` | | **No Docker** | Unnecessary for a local single-user tool | | **No IIS/Kestrel config** | Default Kestrel is sufficient | | **Port** | Default 5001 (HTTPS) or configure in `launchSettings.json` | For maximum simplicity, the user runs `dotnet run` and opens a browser. For distribution to non-developers, publish as a single-file executable with a batch script launcher. **$0.** This runs entirely on the user's local machine. No cloud services, no hosting, no subscriptions. --- Blazor Server maintains a SignalR connection per client, which is overkill for a read-only dashboard. However, since this is local with a single user, the overhead is negligible (~50KB memory for the circuit). **Mitigation:** In .NET 8, you can use Static SSR mode (`@rendermode` not specified) for the dashboard page, which renders pure HTML with zero SignalR overhead. This is ideal for a screenshot-capture use case. Generating SVG in Razor requires careful handling of coordinate calculations and string formatting. **Mitigation:** Isolate all SVG math into a `TimelineCalculator` helper class. Keep Razor templates focused on markup, not math. Use `FormattableString` or explicit `CultureInfo.InvariantCulture` to avoid locale-specific decimal separator issues in SVG coordinates. The dashboard will be screenshotted from a browser. Different browsers render fonts and SVG slightly differently. **Mitigation:** Standardize on Edge/Chrome (Chromium-based) for screenshots. The design uses Segoe UI which renders identically on Windows across Chromium browsers. Document the screenshot process: open `https://localhost:5001`, set viewport to 1920×1080, capture. As the dashboard evolves, the `data.json` schema will change. **Mitigation:** Use `JsonSerializerOptions` with `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull` for forward compatibility. Add default values to all model properties. Ship a `data.template.json` with documentation comments. **Decision: No component library.** Libraries like MudBlazor or Radzen provide pre-built grids and charts, but they impose styling opinions that conflict with the pixel-perfect design requirement. The custom CSS is small (~100 lines) and the components are few (~8 files). Building from scratch is faster than fighting a component library's theme system. **Decision: Accept.** Editing `data.json` requires a page refresh (or app restart if not using `FileSystemWatcher`). This is acceptable for a tool used monthly to generate screenshots. **Optional enhancement:** Add `FileSystemWatcher` on `data.json` and trigger a Blazor re-render via `InvokeAsync(StateHasChanged)`. This is ~15 lines of code and makes iteration faster. ---
- **How many milestone tracks?** The design shows 3 (M1, M2, M3). Should the data model support an arbitrary number, or is 3 the max? *Recommendation: Support arbitrary N, cap visual display at 5 for readability.*
- **How many months in the heatmap?** The design shows 4 columns (Jan–Apr). Should this auto-calculate from data, or be explicitly configured? *Recommendation: Drive from data — the `months` array in JSON determines columns.*
- **"Now" line position**: Should this be auto-calculated from system date, or explicitly set in `data.json`? *Recommendation: Auto-calculate from `DateTime.Now` but allow override in JSON for generating historical snapshots.*
- **Color customization**: Should category colors be configurable per-project in `data.json`, or fixed? *Recommendation: Configurable in JSON with sensible defaults matching the original design.*
- **Multiple projects**: Will this ever need to show multiple projects on one page, or is it always one project per dashboard instance? *Recommendation: One project per instance. For multiple projects, run multiple instances or generate multiple JSONs.*
- **Print/Export**: Beyond screenshots, is PDF export or direct PowerPoint generation desired? *Recommendation: Defer. Screenshots are the stated workflow. If needed later, consider `Puppeteer Sharp` for automated screenshot capture.* ---

### Detailed Analysis

# Technology Stack Research: Executive Reporting Dashboard

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard renders a timeline/Gantt view of project milestones and a heatmap grid showing shipped, in-progress, carryover, and blocker items — designed for 1920×1080 screenshot capture for PowerPoint decks.

**Primary Recommendation:** Use a minimal Blazor Server app with zero JavaScript framework dependencies. Render the timeline SVG directly in Razor components, style with scoped CSS matching the design spec, and read all data from a local `data.json` file at startup. No database, no auth, no external services. The entire solution should be a single `.sln` with one Blazor Server project, deployable via `dotnet run`.

This is deliberately a simple, high-fidelity rendering tool — not a full enterprise application. Every technology choice prioritizes simplicity, visual accuracy, and ease of screenshot capture.

---

## 2. Key Findings

- The original `OriginalDesignConcept.html` is a static 1920×1080 layout using CSS Grid, Flexbox, and inline SVG — all of which Blazor Server can reproduce natively without third-party charting libraries.
- Blazor Server in .NET 8 supports static SSR (server-side rendering) which eliminates SignalR overhead for a read-only dashboard, though interactive mode is fine for local use.
- No database is needed; `System.Text.Json` deserialization of a local `data.json` file is sufficient and eliminates all data layer complexity.
- No authentication or authorization is required per the project scope — this is a local tool for one user.
- SVG generation in Razor components is straightforward and avoids any npm/JavaScript build pipeline.
- CSS isolation (scoped CSS per component) in Blazor is mature in .NET 8 and maps perfectly to the design's section-based styling.
- The design's color palette, grid layout, and typography can be implemented with pure CSS — no CSS framework (Bootstrap, Tailwind) is needed or recommended.
- The entire project can be scaffolded and running in under an hour given the constrained scope.
- Screenshot fidelity is best achieved by keeping the layout at a fixed 1920×1080 viewport with `overflow:hidden`, matching the original HTML design exactly.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components + CSS)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (.NET 8) | .NET 8.0.x LTS | Ships with `Microsoft.AspNetCore.App`. No additional packages needed. |
| **Rendering** | Razor Components (.razor) | Built-in | Component-per-section architecture. |
| **Styling** | Scoped CSS (`.razor.css`) | Built-in | CSS isolation ships with Blazor. No preprocessor needed. |
| **Layout Engine** | CSS Grid + Flexbox | Native CSS | Matches original design exactly. Grid for heatmap, Flex for header/timeline. |
| **Timeline/Gantt** | Inline SVG in Razor | Native | Generate `<svg>` elements directly in `.razor` files with C# loop logic. No charting library. |
| **Icons/Shapes** | Inline SVG primitives | Native | Diamonds (`<polygon>`), circles (`<circle>`), lines — all from the original design. |
| **Font** | Segoe UI | System font | Already specified in design. No web font loading needed (available on Windows). |

**Why no charting library?** The timeline is a simple horizontal bar chart with milestone markers. Libraries like `Radzen`, `MudBlazor`, or `ApexCharts.Blazor` add 200KB+ of JavaScript and impose their own styling opinions. The original HTML achieves the entire visualization in ~40 lines of SVG. Reproducing this directly in Razor is simpler, lighter, and gives pixel-perfect control.

### Backend / Data Layer

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Data Format** | JSON file (`data.json`) | — | Single flat file in project root or `wwwroot/data/`. |
| **Deserialization** | `System.Text.Json` | Built-in (.NET 8) | Zero additional packages. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`. |
| **Data Service** | Singleton `DashboardDataService` | Custom | Reads and caches `data.json` on startup. Registered in DI as singleton. |
| **File Watching (optional)** | `FileSystemWatcher` | Built-in (.NET 8) | Auto-reload data when `data.json` is edited. Nice for iterating on content. |

**No database.** SQLite, LiteDB, or any persistence layer would add complexity with zero benefit. The data is read-only, small (< 100 items), and edited manually in JSON.

### Project Structure

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **SDK** | .NET 8.0 SDK | 8.0.x LTS | `dotnet new blazorserver` or `dotnet new blazor` (unified in .NET 8). |
| **Solution** | `.sln` with single `.csproj` | — | `ReportingDashboard.sln` → `src/ReportingDashboard/ReportingDashboard.csproj` |
| **Launch** | `dotnet run` | — | Opens on `https://localhost:5001` or configurable port. |

### Testing (Lightweight)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Tests** | xUnit | 2.7.x | For data model deserialization tests. |
| **Component Tests** | bUnit | 1.28.x+ | For verifying Razor component rendering. Optional given project simplicity. |
| **Snapshot Tests** | Verify | 24.x | Optional: snapshot-test rendered HTML for regression. |

Given the project's simplicity (read-only, single-page, local-only), a dedicated test project is optional. If added, a single `ReportingDashboard.Tests.csproj` in the solution suffices.

---

## 4. Architecture Recommendations

### Overall Pattern: Minimal Layered Architecture

```
data.json
   ↓ (read at startup)
DashboardDataService (singleton, DI-registered)
   ↓ (injected into components)
Razor Components (.razor + .razor.css)
   ↓ (rendered server-side)
Browser (1920×1080 viewport)
   ↓ (screenshot)
PowerPoint deck
```

No MVC, no MVVM, no CQRS — those patterns are for applications with user interaction, state mutation, and business logic. This is a **read-only rendering pipeline**.

### Component Architecture (Maps to Design Sections)

```
App.razor
└── MainLayout.razor
    └── Dashboard.razor (the single page)
        ├── DashboardHeader.razor        ← Title, subtitle, legend
        ├── TimelineSection.razor        ← Milestone swim lanes + SVG
        │   ├── TimelineLane.razor       ← One horizontal track per milestone group
        │   └── MilestoneMarker.razor    ← Diamond/circle/checkpoint SVG elements
        ├── HeatmapSection.razor         ← Grid container
        │   ├── HeatmapHeader.razor      ← Month column headers
        │   └── HeatmapRow.razor         ← One row per category (Shipped/InProgress/Carryover/Blockers)
        │       └── HeatmapCell.razor    ← Individual cell with item bullets
        └── (optional) FooterSection.razor
```

Each component gets a `.razor.css` scoped stylesheet. This keeps styles isolated and maintainable.

### Data Model Design

```csharp
public record DashboardData
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogLink { get; init; } = "";
    public DateOnly ReportDate { get; init; }
    public List<MilestoneTrack> Tracks { get; init; } = [];
    public List<HeatmapCategory> Categories { get; init; } = [];
    public List<string> Months { get; init; } = []; // Column headers
}

public record MilestoneTrack
{
    public string Id { get; init; } = "";        // e.g., "M1"
    public string Label { get; init; } = "";      // e.g., "Chatbot & MS Role"
    public string Color { get; init; } = "";      // e.g., "#0078D4"
    public List<Milestone> Milestones { get; init; } = [];
}

public record Milestone
{
    public DateOnly Date { get; init; }
    public string Label { get; init; } = "";
    public string Type { get; init; } = "";       // "poc", "production", "checkpoint", "start"
}

public record HeatmapCategory
{
    public string Name { get; init; } = "";       // "Shipped", "In Progress", etc.
    public string Type { get; init; } = "";       // "shipped", "in-progress", "carryover", "blockers"
    public Dictionary<string, List<string>> ItemsByMonth { get; init; } = new();
}
```

### SVG Timeline Rendering Strategy

The timeline is the most complex visual element. Recommended approach:

1. **Calculate pixel positions** in C# based on date ranges: `pixelX = (date - startDate).Days / totalDays * svgWidth`
2. **Render SVG elements** in a `@foreach` loop inside a `<svg>` tag in Razor
3. **Use the exact SVG primitives** from the original HTML: `<line>`, `<circle>`, `<polygon>`, `<text>`
4. **"Now" indicator**: Calculate position from `ReportDate` and render a dashed red vertical line

This approach gives sub-pixel positioning control and matches the original design exactly.

### CSS Strategy

- **No CSS framework.** Bootstrap, MudBlazor, or Tailwind would fight the custom design. The original HTML uses ~80 lines of hand-written CSS. Replicate it.
- **Use CSS custom properties** for the color palette (makes theming trivial):
  ```css
  :root {
      --color-shipped: #34A853;
      --color-shipped-bg: #F0FBF0;
      --color-progress: #0078D4;
      --color-progress-bg: #EEF4FE;
      --color-carryover: #F4B400;
      --color-carryover-bg: #FFFDE7;
      --color-blocker: #EA4335;
      --color-blocker-bg: #FFF5F5;
  }
  ```
- **Fixed viewport**: Set `body { width: 1920px; height: 1080px; overflow: hidden; }` for screenshot-perfect rendering.
- **CSS Grid** for the heatmap: `grid-template-columns: 160px repeat(N, 1fr)` where N comes from data.
- **Flexbox** for header and timeline layout areas.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** Per project requirements, this is a local-only tool with no auth. The Blazor Server app binds to `localhost` only. If you want to prevent accidental exposure:

```csharp
// Program.cs
builder.WebHost.UseUrls("https://localhost:5001");
```

This ensures the dashboard is only accessible from the local machine.

### Data Protection

- `data.json` contains project status information, not PII or secrets.
- No encryption needed. No HTTPS certificate management needed for local use (development cert is fine).
- If the JSON contains sensitive project names, add `data.json` to `.gitignore` and ship a `data.template.json` with example data.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | `dotnet run` from command line, or publish as self-contained single-file executable |
| **Self-Contained Publish** | `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` |
| **No Docker** | Unnecessary for a local single-user tool |
| **No IIS/Kestrel config** | Default Kestrel is sufficient |
| **Port** | Default 5001 (HTTPS) or configure in `launchSettings.json` |

For maximum simplicity, the user runs `dotnet run` and opens a browser. For distribution to non-developers, publish as a single-file executable with a batch script launcher.

### Infrastructure Costs

**$0.** This runs entirely on the user's local machine. No cloud services, no hosting, no subscriptions.

---

## 6. Risks & Trade-offs

### Risk: Blazor Server Overhead for a Static Page

**Risk Level: Low**
Blazor Server maintains a SignalR connection per client, which is overkill for a read-only dashboard. However, since this is local with a single user, the overhead is negligible (~50KB memory for the circuit).

**Mitigation:** In .NET 8, you can use Static SSR mode (`@rendermode` not specified) for the dashboard page, which renders pure HTML with zero SignalR overhead. This is ideal for a screenshot-capture use case.

### Risk: SVG Complexity in Razor

**Risk Level: Low-Medium**
Generating SVG in Razor requires careful handling of coordinate calculations and string formatting.

**Mitigation:** Isolate all SVG math into a `TimelineCalculator` helper class. Keep Razor templates focused on markup, not math. Use `FormattableString` or explicit `CultureInfo.InvariantCulture` to avoid locale-specific decimal separator issues in SVG coordinates.

### Risk: Browser Rendering Differences for Screenshots

**Risk Level: Low**
The dashboard will be screenshotted from a browser. Different browsers render fonts and SVG slightly differently.

**Mitigation:** Standardize on Edge/Chrome (Chromium-based) for screenshots. The design uses Segoe UI which renders identically on Windows across Chromium browsers. Document the screenshot process: open `https://localhost:5001`, set viewport to 1920×1080, capture.

### Risk: JSON Schema Evolution

**Risk Level: Low**
As the dashboard evolves, the `data.json` schema will change.

**Mitigation:** Use `JsonSerializerOptions` with `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull` for forward compatibility. Add default values to all model properties. Ship a `data.template.json` with documentation comments.

### Trade-off: No Component Library vs. Faster Development

**Decision: No component library.** Libraries like MudBlazor or Radzen provide pre-built grids and charts, but they impose styling opinions that conflict with the pixel-perfect design requirement. The custom CSS is small (~100 lines) and the components are few (~8 files). Building from scratch is faster than fighting a component library's theme system.

### Trade-off: No Hot Reload for Data Changes

**Decision: Accept.** Editing `data.json` requires a page refresh (or app restart if not using `FileSystemWatcher`). This is acceptable for a tool used monthly to generate screenshots.

**Optional enhancement:** Add `FileSystemWatcher` on `data.json` and trigger a Blazor re-render via `InvokeAsync(StateHasChanged)`. This is ~15 lines of code and makes iteration faster.

---

## 7. Open Questions

1. **How many milestone tracks?** The design shows 3 (M1, M2, M3). Should the data model support an arbitrary number, or is 3 the max? *Recommendation: Support arbitrary N, cap visual display at 5 for readability.*

2. **How many months in the heatmap?** The design shows 4 columns (Jan–Apr). Should this auto-calculate from data, or be explicitly configured? *Recommendation: Drive from data — the `months` array in JSON determines columns.*

3. **"Now" line position**: Should this be auto-calculated from system date, or explicitly set in `data.json`? *Recommendation: Auto-calculate from `DateTime.Now` but allow override in JSON for generating historical snapshots.*

4. **Color customization**: Should category colors be configurable per-project in `data.json`, or fixed? *Recommendation: Configurable in JSON with sensible defaults matching the original design.*

5. **Multiple projects**: Will this ever need to show multiple projects on one page, or is it always one project per dashboard instance? *Recommendation: One project per instance. For multiple projects, run multiple instances or generate multiple JSONs.*

6. **Print/Export**: Beyond screenshots, is PDF export or direct PowerPoint generation desired? *Recommendation: Defer. Screenshots are the stated workflow. If needed later, consider `Puppeteer Sharp` for automated screenshot capture.*

---

## 8. Implementation Recommendations

### Phase 1: Skeleton + Static Rendering (2-3 hours)

1. `dotnet new blazor -n ReportingDashboard --interactivity Server`
2. Create solution: `dotnet new sln` → `dotnet sln add src/ReportingDashboard`
3. Define `DashboardData` model classes (records)
4. Create `data.json` with fictional project data
5. Create `DashboardDataService` that reads and deserializes `data.json`
6. Build `Dashboard.razor` as a single monolithic component first — get the layout right
7. Match the CSS from `OriginalDesignConcept.html` line-by-line

### Phase 2: Component Decomposition (1-2 hours)

1. Extract `DashboardHeader`, `TimelineSection`, `HeatmapSection` components
2. Move inline styles to scoped `.razor.css` files
3. Extract CSS variables to `app.css` or a shared stylesheet
4. Verify visual fidelity against original design at 1920×1080

### Phase 3: SVG Timeline Logic (1-2 hours)

1. Build `TimelineCalculator` helper (date-to-pixel mapping)
2. Implement `TimelineLane` and `MilestoneMarker` components
3. Render the "Now" indicator line
4. Test with varying date ranges to ensure robustness

### Phase 4: Polish + Documentation (1 hour)

1. Create `data.template.json` with inline comments explaining each field
2. Add `README.md` with setup and screenshot instructions
3. Optional: Add `FileSystemWatcher` for live data reload
4. Optional: Add `dotnet publish` script for single-file distribution

### Quick Wins

- **Immediate visual impact**: Get the heatmap grid rendering first — it's the largest visual area and uses simple CSS Grid. This validates the approach in minutes.
- **Fictional data**: Use a fun project name (e.g., "Project Phoenix — Next-Gen Customer Portal") with realistic milestones to make the demo compelling.
- **Screenshot helper**: Add a `/screenshot` route that strips all padding and renders at exactly 1920×1080 for clean capture.

### NuGet Packages Required

```xml
<!-- ReportingDashboard.csproj — no additional packages beyond the default template -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```

**Zero additional NuGet packages.** Everything needed — Blazor, JSON serialization, Kestrel, CSS isolation — ships in the .NET 8 SDK. This is the simplest possible dependency footprint.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/995900cb8a94e874f0ca80a2af774147a1882e61/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
