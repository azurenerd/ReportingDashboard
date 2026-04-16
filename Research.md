# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-16 19:50 UTC_

### Summary

This project is a single-page executive reporting dashboard that visualizes project milestones, status heatmaps, and progress tracking. The design is intentionally simple—optimized for 1920×1080 screenshots destined for PowerPoint decks. The mandatory stack of .NET 8 Blazor Server is well-suited: it provides full-stack C# with server-side rendering, eliminating JavaScript build tooling entirely. Data is sourced from a local `data.json` file—no database required. **Primary recommendation:** Build a single Blazor Server project with zero external UI component libraries. The original HTML/CSS design is clean enough to implement with pure Blazor components and inline SVG generation. Use `System.Text.Json` for deserialization, CSS Grid/Flexbox for layout (matching the reference design exactly), and a simple `IConfiguration`-bound or file-read model for `data.json`. The entire solution should be < 15 files and deployable with `dotnet run`. ---

### Key Findings

- The reference design (`OriginalDesignConcept.html`) uses only CSS Grid, Flexbox, and inline SVG—no charting library is needed. All visual elements (diamond milestones, timeline lines, heatmap cells) are achievable with pure HTML/CSS/SVG in Blazor components.
- Blazor Server in .NET 8 supports **static server-side rendering (SSR)** via the new render mode system, which is ideal for a screenshot-oriented dashboard—no WebSocket connection needed for a static view.
- A JSON file (`data.json`) as the sole data source eliminates all database complexity. `System.Text.Json` (built into .NET 8) handles deserialization with source generators for AOT-friendly, fast parsing.
- The 1920×1080 fixed-width design means no responsive breakpoints are needed—simplifying CSS significantly.
- No authentication, no authorization, no HTTPS certificates needed for local-only use. This dramatically reduces infrastructure.
- The heatmap grid (4 status rows × N month columns) maps directly to nested `@foreach` loops in Razor—no component library needed.
- SVG timeline with milestones is the most complex visual element; recommend building a dedicated `Timeline.razor` component that generates `<svg>` markup from milestone data.
- The color palette is fully defined in the HTML reference (16 specific hex colors). These should be extracted into CSS custom properties for maintainability. --- The reference design's timeline is the most complex component. Strategy:
- Define timeline parameters: `StartDate`, `EndDate`, `WidthPx` (1560px from reference), `HeightPx` (185px)
- Calculate X positions: `xPos = (date - startDate) / (endDate - startDate) * widthPx`
- Render month grid lines at calculated positions
- Render milestone markers:
- **Checkpoint** = `<circle>` with stroke
- **PoC Milestone** = `<polygon>` (diamond) with `fill="#F4B400"`
- **Production Release** = `<polygon>` (diamond) with `fill="#34A853"`
- Render "NOW" line as dashed `<line>` with `stroke="#EA4335"`
- Each track is a horizontal `<line>` with markers positioned on it All SVG is generated in Razor markup with `@foreach` loops—no JS interop needed. Extract the CSS from `OriginalDesignConcept.html` into `dashboard.css` with these improvements:
- **CSS Custom Properties** for the color palette:
```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-highlight: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-highlight: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-highlight: #FFF0B0;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-highlight: #FFE4E4;
}
```
- **Fixed viewport**: `body { width: 1920px; height: 1080px; overflow: hidden; }` — keeps the screenshot-friendly fixed layout.
- **Grid layout** for heatmap: `grid-template-columns: 160px repeat(N, 1fr)` where N is dynamic based on `data.json` column count. --- | Step | Deliverable | Details | |------|------------|---------| | 1 | Project scaffold | `dotnet new blazor -n ReportingDashboard --interactivity None` (static SSR) | | 2 | Data models + `data.json` | Define C# records, create sample JSON with fictional project data | | 3 | `DashboardDataService` | Singleton service, reads JSON on startup, exposes typed data | | 4 | `dashboard.css` | Extract and refine CSS from reference HTML; add custom properties | | 5 | `Header.razor` | Title, subtitle, backlog link, legend icons | | 6 | `Heatmap.razor` + children | CSS Grid heatmap with dynamic rows/columns from data | | 7 | `Timeline.razor` | SVG generation with date-positioned milestones | | 8 | Integration | Wire everything in `Dashboard.razor`, verify against reference screenshot | | Step | Deliverable | |------|------------| | 9 | JSON schema (`data.schema.json`) for editor IntelliSense | | 10 | Error handling for malformed JSON | | 11 | bUnit snapshot tests for key components | | 12 | `README.md` with usage instructions | | Enhancement | Effort | Value | |-------------|--------|-------| | `FileSystemWatcher` for live reload | 30 min | Edit JSON, see changes without browser refresh | | Playwright screenshot automation | 1 hour | `dotnet run --screenshot` generates PNG | | Print-friendly CSS (`@media print`) | 30 min | Direct browser print to PDF | | Dark mode variant | 1 hour | For teams that prefer dark presentations | | Multiple project support | 2 hours | Dropdown or URL parameter to select project |
- **Start from the reference CSS.** Copy the `<style>` block from `OriginalDesignConcept.html` directly into `dashboard.css`. Refactor into custom properties afterward—don't rewrite from scratch.
- **Use the reference SVG as a test fixture.** The inline SVG in the HTML file has exact coordinates. Use these as expected values in unit tests for the timeline calculation logic.
- **`dotnet watch` from minute one.** Hot reload makes CSS/Razor iteration extremely fast.
- ❌ Don't add Entity Framework—there's no database
- ❌ Don't add MudBlazor/Radzen/Syncfusion—the CSS is simpler without them
- ❌ Don't add JavaScript interop—everything is achievable in Razor + CSS + inline SVG
- ❌ Don't add authentication middleware
- ❌ Don't create separate class library projects—one web project is sufficient
- ❌ Don't add Swagger/OpenAPI—there's no API
- ❌ Don't add logging frameworks (Serilog, etc.)—`Console.WriteLine` suffices for local debugging

### Recommended Tools & Technologies

- **Project:** Executive Project Reporting Dashboard **Date:** April 16, 2026 **Stack:** C# .NET 8 · Blazor Server · Local Only · .sln Structure --- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (.NET 8) | 8.0.x | Use static SSR render mode for screenshot-friendly output | | **CSS Layout** | CSS Grid + Flexbox (no framework) | Native | Matches reference design exactly; no Bootstrap/Tailwind needed | | **Charting/Timeline** | Hand-rolled inline SVG | N/A | Reference design uses `<svg>` with `<line>`, `<circle>`, `<polygon>`, `<text>`—replicate directly | | **Icons** | None required | — | Design uses CSS shapes (rotated squares for diamonds, circles for checkpoints) | | **Font** | Segoe UI (system font) | — | Already specified in reference; no web font loading needed | **Why no UI component library (MudBlazor, Radzen, etc.):** The design is a fixed-layout, screenshot-optimized single page. Adding a component library introduces 200KB+ of CSS/JS, theming conflicts with the precise color palette, and layout opinions that fight the pixel-exact reference. The HTML reference is directly translatable to Razor markup—a component library adds complexity with zero benefit here. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Use source generators (`[JsonSerializable]`) for performance | | **Configuration** | `IConfiguration` or direct `File.ReadAllText` | Built-in | Bind `data.json` to strongly-typed C# models | | **File Watching** | `FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload dashboard when `data.json` changes | **No database.** The `data.json` file is the single source of truth. This is correct for the use case—executive dashboards are updated infrequently (weekly/monthly) and a JSON file is human-editable, version-controllable, and trivially portable.
```csharp
public record DashboardData
{
    public ProjectHeader Header { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineTrack> Tracks { get; init; }
    public HeatmapData Heatmap { get; init; }
}

public record ProjectHeader
{
    public string Title { get; init; }          // "Privacy Automation Release Roadmap"
    public string Subtitle { get; init; }       // "Trusted Platform · Privacy Automation..."
    public string BacklogUrl { get; init; }     // ADO link
    public string CurrentMonth { get; init; }   // "April 2026"
}

public record Milestone
{
    public string Label { get; init; }
    public DateTime Date { get; init; }
    public string Type { get; init; }           // "poc", "production", "checkpoint"
}

public record HeatmapData
{
    public List<string> Columns { get; init; }  // ["Jan", "Feb", "Mar", "Apr"]
    public string HighlightColumn { get; init; } // "Apr" (current month)
    public List<HeatmapRow> Rows { get; init; }
}

public record HeatmapRow
{
    public string Category { get; init; }       // "Shipped", "In Progress", "Carryover", "Blockers"
    public string ColorTheme { get; init; }     // "green", "blue", "amber", "red"
    public Dictionary<string, List<string>> Items { get; init; } // column → items
}
``` | Tool | Version | Purpose | |------|---------|---------| | **xUnit** | 2.7+ | Unit testing framework | | **bUnit** | 1.25+ | Blazor component testing (renders components in-memory) | | **FluentAssertions** | 6.12+ | Readable assertions | | **Verify** | 23.x | Snapshot testing for rendered HTML output—ideal for verifying dashboard layout | | Tool | Version | Purpose | |------|---------|---------| | **.NET SDK** | 8.0.300+ | Build, run, publish | | **dotnet watch** | Built-in | Hot reload during development | | **Playwright for .NET** | 1.41+ | Optional: automated screenshot capture at 1920×1080 | ---
```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Data/
│       │   ├── data.json                    ← the single data source
│       │   ├── DashboardData.cs             ← strongly-typed models
│       │   └── DashboardDataService.cs      ← reads & deserializes JSON
│       ├── Components/
│       │   ├── App.razor                    ← root component
│       │   ├── Routes.razor
│       │   ├── Layout/
│       │   │   └── MainLayout.razor
│       │   └── Pages/
│       │       └── Dashboard.razor          ← the single page
│       ├── Components/Dashboard/
│       │   ├── Header.razor                 ← title, subtitle, legend
│       │   ├── Timeline.razor               ← SVG milestone visualization
│       │   ├── TimelineTrack.razor           ← individual track row
│       │   ├── Heatmap.razor                ← grid container
│       │   ├── HeatmapRow.razor             ← status row (Shipped, In Progress, etc.)
│       │   └── HeatmapCell.razor            ← individual cell with items
│       └── wwwroot/
│           └── css/
│               └── dashboard.css            ← extracted from reference HTML
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── Components/
            └── DashboardTests.cs
``` This project does **not** need:
- ❌ Repository pattern (no database)
- ❌ CQRS/MediatR (no commands, just reads)
- ❌ Dependency injection abstractions beyond one service
- ❌ State management (Fluxor, etc.)—the page is stateless
- ✅ One `DashboardDataService` registered as a singleton that reads `data.json` on startup and exposes `DashboardData`
- ✅ A component tree: `Dashboard.razor` → `Header.razor` + `Timeline.razor` + `Heatmap.razor`
- ✅ Parameters passed down via `[Parameter]` attributes—no cascading values, no state containers
```
data.json → DashboardDataService (singleton, reads on startup)
         → Dashboard.razor (injects service, passes data to children)
         → Header.razor, Timeline.razor, Heatmap.razor (pure render components)
``` Use **Static Server-Side Rendering (static SSR)** — the .NET 8 default. This means:
- No SignalR WebSocket connection maintained
- Page renders as pure HTML on first request
- Perfect for screenshot capture—no hydration delays
- If live-reload of `data.json` is desired later, switch to `InteractiveServer` render mode on just the `Dashboard.razor` component In `Program.cs`:
```csharp
builder.Services.AddRazorComponents();  // No .AddInteractiveServerComponents() needed for static SSR
```

### Considerations & Risks

- **None.** This is explicitly out of scope. The app runs locally (`localhost:5000` or similar) and is accessed only by the person running it. No login, no cookies, no tokens.
- `data.json` contains project names and status items—no PII, no secrets
- If sensitive project data is a concern, the file stays local on the user's machine
- No data leaves the machine; no telemetry, no external API calls | Aspect | Recommendation | |--------|---------------| | **Runtime** | `dotnet run` from command line, or `dotnet watch` during development | | **Port** | Default Kestrel on `http://localhost:5000` | | **HTTPS** | Not needed for local-only use; disable in `launchSettings.json` | | **Publishing** | `dotnet publish -c Release -o ./publish` → single folder, run `ReportingDashboard.exe` | | **Distribution** | Zip the publish folder; recipient needs .NET 8 runtime (or publish as self-contained ~65MB) | For automated PowerPoint screenshot generation:
```bash
# Using Playwright for .NET
dotnet tool install --global Microsoft.Playwright.CLI
playwright screenshot --viewport-size="1920,1080" http://localhost:5000 dashboard.png
``` **$0.** Everything runs locally. No cloud services, no hosting fees, no licenses. --- **Likelihood:** Medium | **Impact:** Medium The SVG timeline with positioned milestones, multi-track lanes, and date-based X calculations is the hardest part of the UI. Date-to-pixel math must account for variable month lengths. **Mitigation:** Build the `Timeline.razor` component first as a spike. Use the reference SVG coordinates as test fixtures. The reference design hardcodes pixel positions—our implementation calculates them, which is more flexible but needs careful testing. **Likelihood:** Low | **Impact:** Low Blazor Server renders standard HTML—CSS behaves identically to the reference HTML file. No framework CSS will interfere because we're not using a component library. **Mitigation:** Diff the rendered HTML against the reference. Use bUnit snapshot tests. **Likelihood:** Low | **Impact:** Medium If someone hand-edits `data.json` with incorrect structure, the page will fail silently or crash. **Mitigation:** Add `System.ComponentModel.DataAnnotations` validation on the model. Display a clear error message on the dashboard if deserialization fails. Consider adding a `data.schema.json` for editor IntelliSense. **Likelihood:** High | **Impact:** High The biggest risk is making this more complex than it needs to be. This is a single-page, read-only, local dashboard. Every added abstraction (DI layers, service interfaces, state management, database) is unnecessary complexity. **Mitigation:** Enforce a complexity budget: < 15 files, < 1000 lines of code, zero JavaScript, zero NuGet packages beyond the SDK. If a feature requires adding a NuGet package, question whether it's truly needed. Static SSR means no live updates—if `data.json` changes, the user must refresh the browser. This is acceptable for a screenshot-oriented workflow. If auto-refresh is later desired, switching to `@rendermode InteractiveServer` on the Dashboard page is a one-line change, plus adding a `FileSystemWatcher` to trigger `StateHasChanged()`. We gain: exact CSS control, smaller payload, no version upgrade churn, no theming conflicts. We lose: pre-built tooltips, animations, accessibility features. For a screenshot dashboard, this trade-off is strongly in favor of no library. ---
- **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json` (e.g., rolling 4 months, or full fiscal year)?
- **How many timeline tracks?** The reference shows 3 tracks (M1, M2, M3). Is there a maximum, or should it scroll/compress if more are added?
- **Should `data.json` support multiple projects?** Current design is one project per dashboard instance. If multi-project is needed, the architecture changes slightly (project selector or multiple JSON files).
- **Automated screenshot capture?** Should the build process generate a PNG automatically (using Playwright), or is manual browser screenshot sufficient?
- **Distribution model:** Will this run only on the author's machine, or will it be shared with other PMs/leads who need to generate their own dashboards with different `data.json` files? This affects whether to publish as self-contained.
- **Design file at `C:/Pics/ReportingDashboardDesign.png`:** This file needs to be reviewed alongside `OriginalDesignConcept.html` to identify any design differences. Are they identical, or does the PNG represent a revised/improved layout? ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Project:** Executive Project Reporting Dashboard
**Date:** April 16, 2026
**Stack:** C# .NET 8 · Blazor Server · Local Only · .sln Structure

---

## 1. Executive Summary

This project is a single-page executive reporting dashboard that visualizes project milestones, status heatmaps, and progress tracking. The design is intentionally simple—optimized for 1920×1080 screenshots destined for PowerPoint decks. The mandatory stack of .NET 8 Blazor Server is well-suited: it provides full-stack C# with server-side rendering, eliminating JavaScript build tooling entirely. Data is sourced from a local `data.json` file—no database required.

**Primary recommendation:** Build a single Blazor Server project with zero external UI component libraries. The original HTML/CSS design is clean enough to implement with pure Blazor components and inline SVG generation. Use `System.Text.Json` for deserialization, CSS Grid/Flexbox for layout (matching the reference design exactly), and a simple `IConfiguration`-bound or file-read model for `data.json`. The entire solution should be < 15 files and deployable with `dotnet run`.

---

## 2. Key Findings

- The reference design (`OriginalDesignConcept.html`) uses only CSS Grid, Flexbox, and inline SVG—no charting library is needed. All visual elements (diamond milestones, timeline lines, heatmap cells) are achievable with pure HTML/CSS/SVG in Blazor components.
- Blazor Server in .NET 8 supports **static server-side rendering (SSR)** via the new render mode system, which is ideal for a screenshot-oriented dashboard—no WebSocket connection needed for a static view.
- A JSON file (`data.json`) as the sole data source eliminates all database complexity. `System.Text.Json` (built into .NET 8) handles deserialization with source generators for AOT-friendly, fast parsing.
- The 1920×1080 fixed-width design means no responsive breakpoints are needed—simplifying CSS significantly.
- No authentication, no authorization, no HTTPS certificates needed for local-only use. This dramatically reduces infrastructure.
- The heatmap grid (4 status rows × N month columns) maps directly to nested `@foreach` loops in Razor—no component library needed.
- SVG timeline with milestones is the most complex visual element; recommend building a dedicated `Timeline.razor` component that generates `<svg>` markup from milestone data.
- The color palette is fully defined in the HTML reference (16 specific hex colors). These should be extracted into CSS custom properties for maintainability.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (.NET 8) | 8.0.x | Use static SSR render mode for screenshot-friendly output |
| **CSS Layout** | CSS Grid + Flexbox (no framework) | Native | Matches reference design exactly; no Bootstrap/Tailwind needed |
| **Charting/Timeline** | Hand-rolled inline SVG | N/A | Reference design uses `<svg>` with `<line>`, `<circle>`, `<polygon>`, `<text>`—replicate directly |
| **Icons** | None required | — | Design uses CSS shapes (rotated squares for diamonds, circles for checkpoints) |
| **Font** | Segoe UI (system font) | — | Already specified in reference; no web font loading needed |

**Why no UI component library (MudBlazor, Radzen, etc.):** The design is a fixed-layout, screenshot-optimized single page. Adding a component library introduces 200KB+ of CSS/JS, theming conflicts with the precise color palette, and layout opinions that fight the pixel-exact reference. The HTML reference is directly translatable to Razor markup—a component library adds complexity with zero benefit here.

### Backend (Data Layer)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Use source generators (`[JsonSerializable]`) for performance |
| **Configuration** | `IConfiguration` or direct `File.ReadAllText` | Built-in | Bind `data.json` to strongly-typed C# models |
| **File Watching** | `FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload dashboard when `data.json` changes |

**No database.** The `data.json` file is the single source of truth. This is correct for the use case—executive dashboards are updated infrequently (weekly/monthly) and a JSON file is human-editable, version-controllable, and trivially portable.

### Data Model (`data.json` → C# Models)

```csharp
public record DashboardData
{
    public ProjectHeader Header { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineTrack> Tracks { get; init; }
    public HeatmapData Heatmap { get; init; }
}

public record ProjectHeader
{
    public string Title { get; init; }          // "Privacy Automation Release Roadmap"
    public string Subtitle { get; init; }       // "Trusted Platform · Privacy Automation..."
    public string BacklogUrl { get; init; }     // ADO link
    public string CurrentMonth { get; init; }   // "April 2026"
}

public record Milestone
{
    public string Label { get; init; }
    public DateTime Date { get; init; }
    public string Type { get; init; }           // "poc", "production", "checkpoint"
}

public record HeatmapData
{
    public List<string> Columns { get; init; }  // ["Jan", "Feb", "Mar", "Apr"]
    public string HighlightColumn { get; init; } // "Apr" (current month)
    public List<HeatmapRow> Rows { get; init; }
}

public record HeatmapRow
{
    public string Category { get; init; }       // "Shipped", "In Progress", "Carryover", "Blockers"
    public string ColorTheme { get; init; }     // "green", "blue", "amber", "red"
    public Dictionary<string, List<string>> Items { get; init; } // column → items
}
```

### Testing

| Tool | Version | Purpose |
|------|---------|---------|
| **xUnit** | 2.7+ | Unit testing framework |
| **bUnit** | 1.25+ | Blazor component testing (renders components in-memory) |
| **FluentAssertions** | 6.12+ | Readable assertions |
| **Verify** | 23.x | Snapshot testing for rendered HTML output—ideal for verifying dashboard layout |

### Build & Tooling

| Tool | Version | Purpose |
|------|---------|---------|
| **.NET SDK** | 8.0.300+ | Build, run, publish |
| **dotnet watch** | Built-in | Hot reload during development |
| **Playwright for .NET** | 1.41+ | Optional: automated screenshot capture at 1920×1080 |

---

## 4. Architecture Recommendations

### Overall Architecture: Single-Project Minimal Blazor Server App

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Data/
│       │   ├── data.json                    ← the single data source
│       │   ├── DashboardData.cs             ← strongly-typed models
│       │   └── DashboardDataService.cs      ← reads & deserializes JSON
│       ├── Components/
│       │   ├── App.razor                    ← root component
│       │   ├── Routes.razor
│       │   ├── Layout/
│       │   │   └── MainLayout.razor
│       │   └── Pages/
│       │       └── Dashboard.razor          ← the single page
│       ├── Components/Dashboard/
│       │   ├── Header.razor                 ← title, subtitle, legend
│       │   ├── Timeline.razor               ← SVG milestone visualization
│       │   ├── TimelineTrack.razor           ← individual track row
│       │   ├── Heatmap.razor                ← grid container
│       │   ├── HeatmapRow.razor             ← status row (Shipped, In Progress, etc.)
│       │   └── HeatmapCell.razor            ← individual cell with items
│       └── wwwroot/
│           └── css/
│               └── dashboard.css            ← extracted from reference HTML
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── Components/
            └── DashboardTests.cs
```

### Pattern: Simple Service + Component Tree

This project does **not** need:
- ❌ Repository pattern (no database)
- ❌ CQRS/MediatR (no commands, just reads)
- ❌ Dependency injection abstractions beyond one service
- ❌ State management (Fluxor, etc.)—the page is stateless

**What it needs:**
- ✅ One `DashboardDataService` registered as a singleton that reads `data.json` on startup and exposes `DashboardData`
- ✅ A component tree: `Dashboard.razor` → `Header.razor` + `Timeline.razor` + `Heatmap.razor`
- ✅ Parameters passed down via `[Parameter]` attributes—no cascading values, no state containers

### Data Flow

```
data.json → DashboardDataService (singleton, reads on startup)
         → Dashboard.razor (injects service, passes data to children)
         → Header.razor, Timeline.razor, Heatmap.razor (pure render components)
```

### Render Mode Decision

Use **Static Server-Side Rendering (static SSR)** — the .NET 8 default. This means:
- No SignalR WebSocket connection maintained
- Page renders as pure HTML on first request
- Perfect for screenshot capture—no hydration delays
- If live-reload of `data.json` is desired later, switch to `InteractiveServer` render mode on just the `Dashboard.razor` component

In `Program.cs`:
```csharp
builder.Services.AddRazorComponents();  // No .AddInteractiveServerComponents() needed for static SSR
```

### SVG Timeline Implementation Strategy

The reference design's timeline is the most complex component. Strategy:

1. Define timeline parameters: `StartDate`, `EndDate`, `WidthPx` (1560px from reference), `HeightPx` (185px)
2. Calculate X positions: `xPos = (date - startDate) / (endDate - startDate) * widthPx`
3. Render month grid lines at calculated positions
4. Render milestone markers:
   - **Checkpoint** = `<circle>` with stroke
   - **PoC Milestone** = `<polygon>` (diamond) with `fill="#F4B400"`
   - **Production Release** = `<polygon>` (diamond) with `fill="#34A853"`
5. Render "NOW" line as dashed `<line>` with `stroke="#EA4335"`
6. Each track is a horizontal `<line>` with markers positioned on it

All SVG is generated in Razor markup with `@foreach` loops—no JS interop needed.

### CSS Strategy

Extract the CSS from `OriginalDesignConcept.html` into `dashboard.css` with these improvements:

1. **CSS Custom Properties** for the color palette:
```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-highlight: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-highlight: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-highlight: #FFF0B0;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-highlight: #FFE4E4;
}
```

2. **Fixed viewport**: `body { width: 1920px; height: 1080px; overflow: hidden; }` — keeps the screenshot-friendly fixed layout.

3. **Grid layout** for heatmap: `grid-template-columns: 160px repeat(N, 1fr)` where N is dynamic based on `data.json` column count.

---

## 5. Security & Infrastructure

### Authentication & Authorization
**None.** This is explicitly out of scope. The app runs locally (`localhost:5000` or similar) and is accessed only by the person running it. No login, no cookies, no tokens.

### Data Protection
- `data.json` contains project names and status items—no PII, no secrets
- If sensitive project data is a concern, the file stays local on the user's machine
- No data leaves the machine; no telemetry, no external API calls

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | `dotnet run` from command line, or `dotnet watch` during development |
| **Port** | Default Kestrel on `http://localhost:5000` |
| **HTTPS** | Not needed for local-only use; disable in `launchSettings.json` |
| **Publishing** | `dotnet publish -c Release -o ./publish` → single folder, run `ReportingDashboard.exe` |
| **Distribution** | Zip the publish folder; recipient needs .NET 8 runtime (or publish as self-contained ~65MB) |

### Screenshot Automation (Optional Enhancement)
For automated PowerPoint screenshot generation:
```bash
# Using Playwright for .NET
dotnet tool install --global Microsoft.Playwright.CLI
playwright screenshot --viewport-size="1920,1080" http://localhost:5000 dashboard.png
```

### Infrastructure Cost
**$0.** Everything runs locally. No cloud services, no hosting fees, no licenses.

---

## 6. Risks & Trade-offs

### Risk: SVG Timeline Complexity
**Likelihood:** Medium | **Impact:** Medium
The SVG timeline with positioned milestones, multi-track lanes, and date-based X calculations is the hardest part of the UI. Date-to-pixel math must account for variable month lengths.
**Mitigation:** Build the `Timeline.razor` component first as a spike. Use the reference SVG coordinates as test fixtures. The reference design hardcodes pixel positions—our implementation calculates them, which is more flexible but needs careful testing.

### Risk: CSS Pixel-Perfection with Blazor
**Likelihood:** Low | **Impact:** Low
Blazor Server renders standard HTML—CSS behaves identically to the reference HTML file. No framework CSS will interfere because we're not using a component library.
**Mitigation:** Diff the rendered HTML against the reference. Use bUnit snapshot tests.

### Risk: data.json Schema Drift
**Likelihood:** Low | **Impact:** Medium
If someone hand-edits `data.json` with incorrect structure, the page will fail silently or crash.
**Mitigation:** Add `System.ComponentModel.DataAnnotations` validation on the model. Display a clear error message on the dashboard if deserialization fails. Consider adding a `data.schema.json` for editor IntelliSense.

### Risk: Over-Engineering
**Likelihood:** High | **Impact:** High
The biggest risk is making this more complex than it needs to be. This is a single-page, read-only, local dashboard. Every added abstraction (DI layers, service interfaces, state management, database) is unnecessary complexity.
**Mitigation:** Enforce a complexity budget: < 15 files, < 1000 lines of code, zero JavaScript, zero NuGet packages beyond the SDK. If a feature requires adding a NuGet package, question whether it's truly needed.

### Trade-off: Static SSR vs Interactive Server
Static SSR means no live updates—if `data.json` changes, the user must refresh the browser. This is acceptable for a screenshot-oriented workflow. If auto-refresh is later desired, switching to `@rendermode InteractiveServer` on the Dashboard page is a one-line change, plus adding a `FileSystemWatcher` to trigger `StateHasChanged()`.

### Trade-off: No Component Library
We gain: exact CSS control, smaller payload, no version upgrade churn, no theming conflicts.
We lose: pre-built tooltips, animations, accessibility features. For a screenshot dashboard, this trade-off is strongly in favor of no library.

---

## 7. Open Questions

1. **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in `data.json` (e.g., rolling 4 months, or full fiscal year)?

2. **How many timeline tracks?** The reference shows 3 tracks (M1, M2, M3). Is there a maximum, or should it scroll/compress if more are added?

3. **Should `data.json` support multiple projects?** Current design is one project per dashboard instance. If multi-project is needed, the architecture changes slightly (project selector or multiple JSON files).

4. **Automated screenshot capture?** Should the build process generate a PNG automatically (using Playwright), or is manual browser screenshot sufficient?

5. **Distribution model:** Will this run only on the author's machine, or will it be shared with other PMs/leads who need to generate their own dashboards with different `data.json` files? This affects whether to publish as self-contained.

6. **Design file at `C:/Pics/ReportingDashboardDesign.png`:** This file needs to be reviewed alongside `OriginalDesignConcept.html` to identify any design differences. Are they identical, or does the PNG represent a revised/improved layout?

---

## 8. Implementation Recommendations

### Phase 1: Core Dashboard (MVP) — ~4 hours

| Step | Deliverable | Details |
|------|------------|---------|
| 1 | Project scaffold | `dotnet new blazor -n ReportingDashboard --interactivity None` (static SSR) |
| 2 | Data models + `data.json` | Define C# records, create sample JSON with fictional project data |
| 3 | `DashboardDataService` | Singleton service, reads JSON on startup, exposes typed data |
| 4 | `dashboard.css` | Extract and refine CSS from reference HTML; add custom properties |
| 5 | `Header.razor` | Title, subtitle, backlog link, legend icons |
| 6 | `Heatmap.razor` + children | CSS Grid heatmap with dynamic rows/columns from data |
| 7 | `Timeline.razor` | SVG generation with date-positioned milestones |
| 8 | Integration | Wire everything in `Dashboard.razor`, verify against reference screenshot |

### Phase 2: Polish & Tooling — ~2 hours

| Step | Deliverable |
|------|------------|
| 9 | JSON schema (`data.schema.json`) for editor IntelliSense |
| 10 | Error handling for malformed JSON |
| 11 | bUnit snapshot tests for key components |
| 12 | `README.md` with usage instructions |

### Phase 3: Optional Enhancements

| Enhancement | Effort | Value |
|-------------|--------|-------|
| `FileSystemWatcher` for live reload | 30 min | Edit JSON, see changes without browser refresh |
| Playwright screenshot automation | 1 hour | `dotnet run --screenshot` generates PNG |
| Print-friendly CSS (`@media print`) | 30 min | Direct browser print to PDF |
| Dark mode variant | 1 hour | For teams that prefer dark presentations |
| Multiple project support | 2 hours | Dropdown or URL parameter to select project |

### Quick Wins
1. **Start from the reference CSS.** Copy the `<style>` block from `OriginalDesignConcept.html` directly into `dashboard.css`. Refactor into custom properties afterward—don't rewrite from scratch.
2. **Use the reference SVG as a test fixture.** The inline SVG in the HTML file has exact coordinates. Use these as expected values in unit tests for the timeline calculation logic.
3. **`dotnet watch` from minute one.** Hot reload makes CSS/Razor iteration extremely fast.

### What NOT To Do
- ❌ Don't add Entity Framework—there's no database
- ❌ Don't add MudBlazor/Radzen/Syncfusion—the CSS is simpler without them
- ❌ Don't add JavaScript interop—everything is achievable in Razor + CSS + inline SVG
- ❌ Don't add authentication middleware
- ❌ Don't create separate class library projects—one web project is sufficient
- ❌ Don't add Swagger/OpenAPI—there's no API
- ❌ Don't add logging frameworks (Serilog, etc.)—`Console.WriteLine` suffices for local debugging

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/39153922407673781d6a83e555d7efa24e691f52/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
