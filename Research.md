# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-17 14:43 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipped items, in-progress work, carryover, and blockers. The dashboard reads from a local `data.json` file and renders a pixel-perfect view optimized for 1920×1080 screenshots destined for PowerPoint executive decks. **Primary Recommendation:** Build a minimal Blazor Server application with zero authentication, no database, and no cloud dependencies. Use a single Razor component that reads `data.json` on startup via `System.Text.Json`, renders the timeline via inline SVG in Blazor markup, and styles the heatmap grid with scoped CSS matching the design spec. The entire solution should be under 10 files and deployable with `dotnet run`. This is intentionally a **low-complexity, high-polish** project. The technical risk is near zero. The main challenge is CSS fidelity to the design mockup—not architecture. --- | Package | Version | Required? | Purpose | |---------|---------|-----------|---------| | *None beyond the default Blazor Server template* | — | — | The default `dotnet new blazor` template includes everything needed | | `Microsoft.Playwright` | 1.41+ | Optional | Automated screenshot capture | | `bUnit` | 1.25+ | Optional | Component testing | | `xUnit` | 2.7+ | Optional | Unit testing | **Total additional dependencies for MVP: Zero.** This is an intentionally dependency-free project using only what ships with .NET 8.

### Key Findings

- The original `OriginalDesignConcept.html` is a static 1920×1080 layout using CSS Grid (`grid-template-columns: 160px repeat(4,1fr)`), Flexbox, inline SVG for the timeline, and the Segoe UI font stack. All of this translates directly to Blazor components with scoped CSS.
- **No database is needed.** A flat `data.json` file is the entire data layer. `System.Text.Json` (built into .NET 8) handles deserialization with no additional packages.
- **No charting library is needed.** The timeline is a simple SVG with lines, circles, diamonds, and text—all of which can be rendered as raw `<svg>` markup in a Blazor `.razor` file with `@foreach` loops over milestone data.
- **Blazor Server is ideal for local-only use.** It runs on `localhost`, requires no static file hosting strategy, and supports hot reload during development via `dotnet watch`.
- The heatmap grid is pure CSS Grid with color-coded rows. No JavaScript interop or third-party grid component is required.
- The design targets a fixed 1920×1080 viewport. Responsive design is explicitly **not required**—this simplifies CSS significantly.
- **No authentication, no authorization, no HTTPS certificate management** is needed per requirements. The app runs on `http://localhost:5000`.
- The `.sln` structure should contain a single project: the Blazor Server app. No class libraries, no shared projects, no test projects for MVP.
- **Screenshot optimization** is a key non-functional requirement. The page must render cleanly at exactly 1920×1080 with no scrollbars, no loading spinners, and no SignalR connection indicators visible. --- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Tests** | xUnit | 2.7+ | If testing JSON deserialization logic | | **Component Tests** | bUnit | 1.25+ | If testing Blazor component rendering | | **Screenshot Tests** | Playwright for .NET | 1.41+ | If automating screenshot capture for PowerPoint | | Tool | Purpose | Notes | |------|---------|-------| | `dotnet watch` | Hot reload during development | `dotnet watch run` for live CSS/markup changes | | Visual Studio 2022 / VS Code | IDE | VS 2022 has best Blazor tooling; VS Code + C# Dev Kit works | | Browser DevTools | CSS debugging | F12 → inspect grid/flexbox layout | --- **Goal:** Render the dashboard from `data.json` with visual fidelity to the design. | Step | Task | Estimated Time | |------|------|---------------| | 1 | `dotnet new blazor -n ReportingDashboard --interactivity Server` | 5 min | | 2 | Define `DashboardData.cs` model classes matching `data.json` schema | 30 min | | 3 | Create `data.json` with fictional project data (match design content) | 30 min | | 4 | Build `DashboardDataService.cs` to read and deserialize JSON | 20 min | | 5 | Build `Dashboard.razor` with header section | 30 min | | 6 | Build SVG timeline section with milestone rendering | 1–2 hours | | 7 | Build heatmap grid with CSS Grid, row coloring, cell items | 1–2 hours | | 8 | Fine-tune CSS to match design pixel-for-pixel | 1–2 hours | | 9 | Test screenshot at 1920×1080 | 15 min | | Enhancement | Value | |-------------|-------| | Add CSS custom properties for theming | Easy color changes for different projects | | Add `FileSystemWatcher` for live `data.json` reload | Edit JSON → page updates automatically | | Add Playwright screenshot automation | One-command screenshot generation | | Support multiple report files via URL parameter | `/dashboard?report=project-alpha` | | Add print stylesheet | `Ctrl+P` produces clean PDF | | Enhancement | Notes | |-------------|-------| | SQLite storage for historical data | Track month-over-month changes | | Data editor UI | Edit `data.json` through a form instead of raw JSON | | Multiple dashboard layouts | Different views for different audiences | | Export to PNG/PDF from the app | Eliminate manual screenshot step |
- **Start with the CSS.** Port the `<style>` block from `OriginalDesignConcept.html` directly into `Dashboard.razor.css`. This gives you a working layout immediately.
- **Use the HTML design as a template.** Copy the HTML structure from the design file into `Dashboard.razor`, then replace hardcoded text with `@Data.Property` bindings. This is faster than building from scratch.
- **Fictional data should mirror the design.** Use the same categories (Shipped, In Progress, Carryover, Blockers) and similar item counts so the visual density matches the reference.
```json
{
  "header": {
    "title": "Project Phoenix Release Roadmap",
    "subtitle": "Engineering Division • Platform Modernization • April 2026",
    "backlogLink": "https://dev.azure.com/org/project",
    "reportDate": "2026-04-17"
  },
  "timelineTracks": [
    {
      "id": "m1",
      "name": "M1",
      "description": "Core API & Auth",
      "color": "#0078D4",
      "milestones": [
        { "label": "Jan 15", "date": "2026-01-15", "type": "checkpoint" },
        { "label": "Mar 20 PoC", "date": "2026-03-20", "type": "poc" },
        { "label": "May Prod", "date": "2026-05-01", "type": "production" }
      ]
    }
  ],
  "heatmap": {
    "columns": ["January", "February", "March", "April"],
    "highlightColumnIndex": 3,
    "rows": [
      {
        "category": "shipped",
        "label": "Shipped",
        "cellItems": [
          ["Auth service v1", "CI/CD pipeline"],
          ["User management API", "Logging framework"],
          ["Search indexer", "Rate limiting"],
          ["Dashboard v1", "Notification service"]
        ]
      },
      {
        "category": "in-progress",
        "label": "In Progress",
        "cellItems": [
          ["Data migration tool"],
          ["Caching layer"],
          ["Performance testing"],
          ["Mobile API gateway", "Batch processor"]
        ]
      },
      {
        "category": "carryover",
        "label": "Carryover",
        "cellItems": [
          [],
          ["Legacy API deprecation"],
          ["Schema migration v2"],
          ["Documentation update"]
        ]
      },
      {
        "category": "blockers",
        "label": "Blockers",
        "cellItems": [
          [],
          ["Vendor SDK delay"],
          [],
          ["Compliance review pending"]
        ]
      }
    ]
  }
}
```
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.ListenLocalhost(5000));
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
``` ---

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Framework** | Blazor Server (.NET 8) | `net8.0` (LTS, supported through Nov 2026) | Ships with ASP.NET Core; no separate install | | **CSS Strategy** | Scoped CSS (`.razor.css` files) | Built-in to Blazor | Component-isolated styles, no CSS framework needed | | **Layout** | CSS Grid + Flexbox | Native browser | Matches the design spec exactly | | **Timeline Visualization** | Inline SVG in Razor markup | N/A | No charting library—raw `<svg>` with Blazor `@foreach` | | **Font** | Segoe UI | System font on Windows | No web font loading needed for local Windows use | | **Icons/Shapes** | SVG primitives (`<polygon>`, `<circle>`, `<line>`) | N/A | Diamonds, circles, dashed lines per design | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Zero additional packages; use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` | | **File Reading** | `System.IO.File.ReadAllTextAsync` | Built-in | Read `data.json` from app root or configurable path | | **Configuration** | `appsettings.json` | Built-in | Store `data.json` file path; use `IConfiguration` | | **Data Refresh** | Manual (`F5` browser refresh) | N/A | No polling, no SignalR push—intentionally simple | | Layer | Technology | Notes | |-------|-----------|-------| | **Storage** | `data.json` flat file | No database. Period. This is a read-only dashboard over a JSON config file. | | Layer | Technology | Notes | |-------|-----------|-------| | **Hosting** | `dotnet run` on localhost | Kestrel dev server on `http://localhost:5000` | | **Deployment** | None (local only) | No Docker, no IIS, no cloud | | **OS** | Windows (primary) | Segoe UI font dependency; Linux would need font substitution | This is not a CRUD app. It's a **read-only data visualization page**. The architecture should be as flat as possible:
```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs                  # Minimal hosting setup
    ├── Properties/
    │   └── launchSettings.json     # localhost:5000, no HTTPS
    ├── Data/
    │   ├── DashboardData.cs        # C# model classes matching data.json schema
    │   ├── DashboardDataService.cs # Reads and deserializes data.json
    │   └── data.json               # The actual report data
    ├── Components/
    │   ├── App.razor               # Root component
    │   ├── Pages/
    │   │   └── Dashboard.razor     # The single page
    │   │   └── Dashboard.razor.css # Scoped styles
    │   └── Layout/
    │       ├── MainLayout.razor    # Minimal shell (no nav, no sidebar)
    │       └── MainLayout.razor.css
    └── wwwroot/
        └── css/
            └── app.css             # Global resets only
```
```
data.json  →  DashboardDataService (DI singleton)  →  Dashboard.razor (renders HTML/SVG)
```
- **`DashboardDataService`** is registered as a singleton in `Program.cs`. On first access, it reads `data.json` via `File.ReadAllTextAsync`, deserializes it with `System.Text.Json`, and caches the result in memory.
- **`Dashboard.razor`** injects `DashboardDataService`, calls `await service.GetDataAsync()` in `OnInitializedAsync`, and renders the full page.
- **No API controllers.** No REST endpoints. No HTTP calls. The Blazor component reads the service directly via DI.
```csharp
public record DashboardReport
{
    public HeaderInfo Header { get; init; }
    public List<Milestone> Milestones { get; init; } = [];
    public List<TimelineTrack> TimelineTracks { get; init; } = [];
    public HeatmapData Heatmap { get; init; }
}

public record HeaderInfo
{
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string BacklogLink { get; init; }
    public DateOnly ReportDate { get; init; }
}

public record Milestone
{
    public string Label { get; init; }
    public DateOnly Date { get; init; }
    public string Type { get; init; }  // "poc", "production", "checkpoint"
}

public record TimelineTrack
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string Color { get; init; }
    public List<Milestone> Milestones { get; init; } = [];
}

public record HeatmapData
{
    public List<string> Columns { get; init; } = [];  // Month names
    public int HighlightColumnIndex { get; init; }     // Current month (e.g., 3 for April)
    public List<HeatmapRow> Rows { get; init; } = [];
}

public record HeatmapRow
{
    public string Category { get; init; }  // "shipped", "in-progress", "carryover", "blockers"
    public string Label { get; init; }
    public List<List<string>> CellItems { get; init; } = [];  // Items per column
}
``` The timeline section in the design uses a `1560×185` SVG. In Blazor, render this as:
```razor
<svg width="1560" height="185" style="overflow:visible">
    @foreach (var track in Data.TimelineTracks)
    {
        <line x1="0" y1="@trackY" x2="1560" y2="@trackY" 
              stroke="@track.Color" stroke-width="3" />
        @foreach (var ms in track.Milestones)
        {
            @if (ms.Type == "production")
            {
                <polygon points="@DiamondPoints(msX, trackY)" fill="#34A853" />
            }
            // ... other milestone types
        }
    }
</svg>
``` Use a helper method to convert `DateOnly` to X pixel position based on the date range of the report (e.g., Jan–Jun = 0–1560px linear interpolation).
- **Global resets only** in `wwwroot/css/app.css`: `* { margin: 0; padding: 0; box-sizing: border-box; }`
- **All design styles** in `Dashboard.razor.css` using Blazor's scoped CSS isolation
- **Fixed viewport**: `body { width: 1920px; height: 1080px; overflow: hidden; }` — this is a screenshot-optimized page, not a responsive web app
- **Color tokens** as CSS custom properties in `:root` for easy theme adjustment:
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
``` | Decision | Choice | Rationale | |----------|--------|-----------| | Component granularity | Single `Dashboard.razor` with helper methods | Under 300 lines; splitting into sub-components adds complexity without value | | SVG vs. charting library | Raw SVG | The design has ~20 SVG elements. A charting library would be overkill and harder to pixel-match | | CSS framework | None | The design is fixed-width with ~100 CSS rules. Bootstrap/Tailwind would add bloat | | State management | None | Read-only data, no user interaction, no state to manage | | Error handling | Graceful fallback | If `data.json` is missing, show a single error message; no retry logic | ---

### Considerations & Risks

- **None.** This is explicitly out of scope per requirements. The app runs on `localhost` and is accessed only by the person running it. No login page, no cookies, no tokens. To ensure the app is truly local-only, configure Kestrel in `Program.cs`:
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // Binds only to 127.0.0.1, not 0.0.0.0
});
``` This prevents the dashboard from being accessible on the network.
- `data.json` contains project names and status—potentially sensitive in a corporate context. Since it's local-only, file system permissions are the only protection needed.
- **Do not commit `data.json` to source control** if it contains real project data. Add it to `.gitignore` and provide a `data.example.json` with the fictional sample data. | Aspect | Approach | |--------|----------| | **Runtime** | .NET 8 SDK or Runtime must be installed on the user's machine | | **Launch** | `dotnet run` from the project directory, or publish as a self-contained executable | | **Self-contained publish** | `dotnet publish -c Release -r win-x64 --self-contained` produces a single folder with no .NET install required | | **Port** | `http://localhost:5000` (configurable in `launchSettings.json`) | **$0.** This is a local application. No servers, no cloud, no subscriptions. Since the primary output is PowerPoint screenshots:
- Open `http://localhost:5000` in Chrome/Edge
- Set browser window to 1920×1080 (use DevTools → Device Toolbar → Responsive → 1920×1080)
- Take screenshot with DevTools: `Ctrl+Shift+P` → "Capture full size screenshot"
- Optional: automate with Playwright (see Testing section) For automated screenshot capture, add a simple script:
```csharp
// Screenshot helper (optional, using Playwright)
// dotnet add package Microsoft.Playwright --version 1.41.0
var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();
await page.SetViewportSizeAsync(1920, 1080);
await page.GotoAsync("http://localhost:5000");
await page.ScreenshotAsync(new() { Path = "dashboard.png" });
``` --- | Risk | Impact | Mitigation | |------|--------|------------| | **CSS fidelity to design** | Medium — executives notice pixel differences | Develop with browser DevTools open; compare screenshots side-by-side with the design reference | | **Segoe UI not available on non-Windows** | Low — primarily Windows use case | Add fallback: `font-family: 'Segoe UI', -apple-system, Arial, sans-serif` | | **`data.json` schema drift** | Low — manual file editing could introduce errors | Validate on load with `try/catch`; show clear error message with the JSON path that failed | | **SVG rendering differences across browsers** | Low — Chrome and Edge use same engine | Standardize on Edge/Chrome for screenshots; test once in both | | Trade-off | What We Gain | What We Lose | |-----------|-------------|-------------| | No database | Zero setup, zero dependencies, instant portability | No historical data, no trend analysis, no multi-user access | | No authentication | Simplicity, no login friction | Anyone on the machine can access the page | | Fixed 1920×1080 layout | Pixel-perfect screenshots every time | Not usable on smaller screens or mobile | | Single JSON file for data | Easy to edit, easy to version, easy to understand | No validation UI, no structured editor, risk of malformed JSON | | Blazor Server (not Static SSR) | Hot reload, familiar C# patterns, SignalR just works | Heavier than needed for a static page; SignalR connection banner may flash on load | **Important:** Blazor Server shows a reconnection UI when the SignalR connection drops. For a screenshot-optimized page, suppress this in `App.razor`:
```html
<div id="blazor-error-ui" style="display:none">
    <!-- Suppress default error UI -->
</div>
``` Or use Blazor Static SSR (available in .NET 8) for the dashboard page since it has no interactivity:
```csharp
// In Dashboard.razor
@attribute [StreamRendering(false)]
@attribute [RenderModeServer] // or omit for static SSR
``` Since the page is read-only with no user interaction, **Static SSR is actually the better render mode**—it sends plain HTML with no SignalR connection, eliminating the reconnection banner issue entirely. --- | # | Question | Who Decides | Impact | |---|----------|-------------|--------| | 1 | **Should the dashboard auto-refresh when `data.json` changes?** Using `FileSystemWatcher` + SignalR push is trivial in Blazor Server but adds complexity. For screenshot use, manual F5 is likely sufficient. | Product Owner | Low—affects developer experience only | | 2 | **How many months should the heatmap show?** The design shows 4 columns (Jan–Apr). Should this be configurable in `data.json` (e.g., 3, 6, or 12 months)? | Product Owner | Medium—affects grid CSS and data model | | 3 | **Should the timeline date range be configurable?** Currently hardcoded to Jan–Jun in the SVG. Making it dynamic requires date-to-pixel math. | Engineer | Low—straightforward to implement | | 4 | **Will there be multiple projects/reports?** If yes, should the app support switching between different `data.json` files (e.g., via URL parameter `?report=project-a`)? | Product Owner | Medium—affects routing and file loading | | 5 | **What is the target browser?** Edge and Chrome render identically. Firefox has minor SVG differences. | User | Low—standardize on Edge for internal Microsoft use | | 6 | **Should the "Now" line position be calculated from the system date or specified in `data.json`?** Auto-calculation is convenient; manual specification allows generating reports for past/future dates. | Product Owner | Low—trivial either way | | 7 | **Is dark mode needed?** The design is light-only. Adding dark mode doubles the CSS effort. | Product Owner | Medium—recommend deferring to post-MVP | ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipped items, in-progress work, carryover, and blockers. The dashboard reads from a local `data.json` file and renders a pixel-perfect view optimized for 1920×1080 screenshots destined for PowerPoint executive decks.

**Primary Recommendation:** Build a minimal Blazor Server application with zero authentication, no database, and no cloud dependencies. Use a single Razor component that reads `data.json` on startup via `System.Text.Json`, renders the timeline via inline SVG in Blazor markup, and styles the heatmap grid with scoped CSS matching the design spec. The entire solution should be under 10 files and deployable with `dotnet run`.

This is intentionally a **low-complexity, high-polish** project. The technical risk is near zero. The main challenge is CSS fidelity to the design mockup—not architecture.

---

## 2. Key Findings

- The original `OriginalDesignConcept.html` is a static 1920×1080 layout using CSS Grid (`grid-template-columns: 160px repeat(4,1fr)`), Flexbox, inline SVG for the timeline, and the Segoe UI font stack. All of this translates directly to Blazor components with scoped CSS.
- **No database is needed.** A flat `data.json` file is the entire data layer. `System.Text.Json` (built into .NET 8) handles deserialization with no additional packages.
- **No charting library is needed.** The timeline is a simple SVG with lines, circles, diamonds, and text—all of which can be rendered as raw `<svg>` markup in a Blazor `.razor` file with `@foreach` loops over milestone data.
- **Blazor Server is ideal for local-only use.** It runs on `localhost`, requires no static file hosting strategy, and supports hot reload during development via `dotnet watch`.
- The heatmap grid is pure CSS Grid with color-coded rows. No JavaScript interop or third-party grid component is required.
- The design targets a fixed 1920×1080 viewport. Responsive design is explicitly **not required**—this simplifies CSS significantly.
- **No authentication, no authorization, no HTTPS certificate management** is needed per requirements. The app runs on `http://localhost:5000`.
- The `.sln` structure should contain a single project: the Blazor Server app. No class libraries, no shared projects, no test projects for MVP.
- **Screenshot optimization** is a key non-functional requirement. The page must render cleanly at exactly 1920×1080 with no scrollbars, no loading spinners, and no SignalR connection indicators visible.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server UI)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Framework** | Blazor Server (.NET 8) | `net8.0` (LTS, supported through Nov 2026) | Ships with ASP.NET Core; no separate install |
| **CSS Strategy** | Scoped CSS (`.razor.css` files) | Built-in to Blazor | Component-isolated styles, no CSS framework needed |
| **Layout** | CSS Grid + Flexbox | Native browser | Matches the design spec exactly |
| **Timeline Visualization** | Inline SVG in Razor markup | N/A | No charting library—raw `<svg>` with Blazor `@foreach` |
| **Font** | Segoe UI | System font on Windows | No web font loading needed for local Windows use |
| **Icons/Shapes** | SVG primitives (`<polygon>`, `<circle>`, `<line>`) | N/A | Diamonds, circles, dashed lines per design |

### Backend (Data Layer)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Zero additional packages; use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` |
| **File Reading** | `System.IO.File.ReadAllTextAsync` | Built-in | Read `data.json` from app root or configurable path |
| **Configuration** | `appsettings.json` | Built-in | Store `data.json` file path; use `IConfiguration` |
| **Data Refresh** | Manual (`F5` browser refresh) | N/A | No polling, no SignalR push—intentionally simple |

### Database

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Storage** | `data.json` flat file | No database. Period. This is a read-only dashboard over a JSON config file. |

### Infrastructure

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Hosting** | `dotnet run` on localhost | Kestrel dev server on `http://localhost:5000` |
| **Deployment** | None (local only) | No Docker, no IIS, no cloud |
| **OS** | Windows (primary) | Segoe UI font dependency; Linux would need font substitution |

### Testing (Optional, Post-MVP)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Tests** | xUnit | 2.7+ | If testing JSON deserialization logic |
| **Component Tests** | bUnit | 1.25+ | If testing Blazor component rendering |
| **Screenshot Tests** | Playwright for .NET | 1.41+ | If automating screenshot capture for PowerPoint |

### Development Tooling

| Tool | Purpose | Notes |
|------|---------|-------|
| `dotnet watch` | Hot reload during development | `dotnet watch run` for live CSS/markup changes |
| Visual Studio 2022 / VS Code | IDE | VS 2022 has best Blazor tooling; VS Code + C# Dev Kit works |
| Browser DevTools | CSS debugging | F12 → inspect grid/flexbox layout |

---

## 4. Architecture Recommendations

### Overall Pattern: **Single-Component Read-Only Dashboard**

This is not a CRUD app. It's a **read-only data visualization page**. The architecture should be as flat as possible:

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs                  # Minimal hosting setup
    ├── Properties/
    │   └── launchSettings.json     # localhost:5000, no HTTPS
    ├── Data/
    │   ├── DashboardData.cs        # C# model classes matching data.json schema
    │   ├── DashboardDataService.cs # Reads and deserializes data.json
    │   └── data.json               # The actual report data
    ├── Components/
    │   ├── App.razor               # Root component
    │   ├── Pages/
    │   │   └── Dashboard.razor     # The single page
    │   │   └── Dashboard.razor.css # Scoped styles
    │   └── Layout/
    │       ├── MainLayout.razor    # Minimal shell (no nav, no sidebar)
    │       └── MainLayout.razor.css
    └── wwwroot/
        └── css/
            └── app.css             # Global resets only
```

### Data Flow

```
data.json  →  DashboardDataService (DI singleton)  →  Dashboard.razor (renders HTML/SVG)
```

1. **`DashboardDataService`** is registered as a singleton in `Program.cs`. On first access, it reads `data.json` via `File.ReadAllTextAsync`, deserializes it with `System.Text.Json`, and caches the result in memory.
2. **`Dashboard.razor`** injects `DashboardDataService`, calls `await service.GetDataAsync()` in `OnInitializedAsync`, and renders the full page.
3. **No API controllers.** No REST endpoints. No HTTP calls. The Blazor component reads the service directly via DI.

### Data Model Design (`DashboardData.cs`)

```csharp
public record DashboardReport
{
    public HeaderInfo Header { get; init; }
    public List<Milestone> Milestones { get; init; } = [];
    public List<TimelineTrack> TimelineTracks { get; init; } = [];
    public HeatmapData Heatmap { get; init; }
}

public record HeaderInfo
{
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string BacklogLink { get; init; }
    public DateOnly ReportDate { get; init; }
}

public record Milestone
{
    public string Label { get; init; }
    public DateOnly Date { get; init; }
    public string Type { get; init; }  // "poc", "production", "checkpoint"
}

public record TimelineTrack
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string Color { get; init; }
    public List<Milestone> Milestones { get; init; } = [];
}

public record HeatmapData
{
    public List<string> Columns { get; init; } = [];  // Month names
    public int HighlightColumnIndex { get; init; }     // Current month (e.g., 3 for April)
    public List<HeatmapRow> Rows { get; init; } = [];
}

public record HeatmapRow
{
    public string Category { get; init; }  // "shipped", "in-progress", "carryover", "blockers"
    public string Label { get; init; }
    public List<List<string>> CellItems { get; init; } = [];  // Items per column
}
```

### SVG Timeline Rendering Strategy

The timeline section in the design uses a `1560×185` SVG. In Blazor, render this as:

```razor
<svg width="1560" height="185" style="overflow:visible">
    @foreach (var track in Data.TimelineTracks)
    {
        <line x1="0" y1="@trackY" x2="1560" y2="@trackY" 
              stroke="@track.Color" stroke-width="3" />
        @foreach (var ms in track.Milestones)
        {
            @if (ms.Type == "production")
            {
                <polygon points="@DiamondPoints(msX, trackY)" fill="#34A853" />
            }
            // ... other milestone types
        }
    }
</svg>
```

Use a helper method to convert `DateOnly` to X pixel position based on the date range of the report (e.g., Jan–Jun = 0–1560px linear interpolation).

### CSS Architecture

- **Global resets only** in `wwwroot/css/app.css`: `* { margin: 0; padding: 0; box-sizing: border-box; }`
- **All design styles** in `Dashboard.razor.css` using Blazor's scoped CSS isolation
- **Fixed viewport**: `body { width: 1920px; height: 1080px; overflow: hidden; }` — this is a screenshot-optimized page, not a responsive web app
- **Color tokens** as CSS custom properties in `:root` for easy theme adjustment:

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

### Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Component granularity | Single `Dashboard.razor` with helper methods | Under 300 lines; splitting into sub-components adds complexity without value |
| SVG vs. charting library | Raw SVG | The design has ~20 SVG elements. A charting library would be overkill and harder to pixel-match |
| CSS framework | None | The design is fixed-width with ~100 CSS rules. Bootstrap/Tailwind would add bloat |
| State management | None | Read-only data, no user interaction, no state to manage |
| Error handling | Graceful fallback | If `data.json` is missing, show a single error message; no retry logic |

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope per requirements. The app runs on `localhost` and is accessed only by the person running it. No login page, no cookies, no tokens.

To ensure the app is truly local-only, configure Kestrel in `Program.cs`:

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // Binds only to 127.0.0.1, not 0.0.0.0
});
```

This prevents the dashboard from being accessible on the network.

### Data Protection

- `data.json` contains project names and status—potentially sensitive in a corporate context. Since it's local-only, file system permissions are the only protection needed.
- **Do not commit `data.json` to source control** if it contains real project data. Add it to `.gitignore` and provide a `data.example.json` with the fictional sample data.

### Hosting & Deployment

| Aspect | Approach |
|--------|----------|
| **Runtime** | .NET 8 SDK or Runtime must be installed on the user's machine |
| **Launch** | `dotnet run` from the project directory, or publish as a self-contained executable |
| **Self-contained publish** | `dotnet publish -c Release -r win-x64 --self-contained` produces a single folder with no .NET install required |
| **Port** | `http://localhost:5000` (configurable in `launchSettings.json`) |

### Infrastructure Costs

**$0.** This is a local application. No servers, no cloud, no subscriptions.

### Screenshot Workflow Optimization

Since the primary output is PowerPoint screenshots:

1. Open `http://localhost:5000` in Chrome/Edge
2. Set browser window to 1920×1080 (use DevTools → Device Toolbar → Responsive → 1920×1080)
3. Take screenshot with DevTools: `Ctrl+Shift+P` → "Capture full size screenshot"
4. Optional: automate with Playwright (see Testing section)

For automated screenshot capture, add a simple script:

```csharp
// Screenshot helper (optional, using Playwright)
// dotnet add package Microsoft.Playwright --version 1.41.0
var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();
await page.SetViewportSizeAsync(1920, 1080);
await page.GotoAsync("http://localhost:5000");
await page.ScreenshotAsync(new() { Path = "dashboard.png" });
```

---

## 6. Risks & Trade-offs

### Low-Risk Items (Manageable)

| Risk | Impact | Mitigation |
|------|--------|------------|
| **CSS fidelity to design** | Medium — executives notice pixel differences | Develop with browser DevTools open; compare screenshots side-by-side with the design reference |
| **Segoe UI not available on non-Windows** | Low — primarily Windows use case | Add fallback: `font-family: 'Segoe UI', -apple-system, Arial, sans-serif` |
| **`data.json` schema drift** | Low — manual file editing could introduce errors | Validate on load with `try/catch`; show clear error message with the JSON path that failed |
| **SVG rendering differences across browsers** | Low — Chrome and Edge use same engine | Standardize on Edge/Chrome for screenshots; test once in both |

### Trade-offs Made

| Trade-off | What We Gain | What We Lose |
|-----------|-------------|-------------|
| No database | Zero setup, zero dependencies, instant portability | No historical data, no trend analysis, no multi-user access |
| No authentication | Simplicity, no login friction | Anyone on the machine can access the page |
| Fixed 1920×1080 layout | Pixel-perfect screenshots every time | Not usable on smaller screens or mobile |
| Single JSON file for data | Easy to edit, easy to version, easy to understand | No validation UI, no structured editor, risk of malformed JSON |
| Blazor Server (not Static SSR) | Hot reload, familiar C# patterns, SignalR just works | Heavier than needed for a static page; SignalR connection banner may flash on load |

### SignalR Connection Indicator

**Important:** Blazor Server shows a reconnection UI when the SignalR connection drops. For a screenshot-optimized page, suppress this in `App.razor`:

```html
<div id="blazor-error-ui" style="display:none">
    <!-- Suppress default error UI -->
</div>
```

Or use Blazor Static SSR (available in .NET 8) for the dashboard page since it has no interactivity:

```csharp
// In Dashboard.razor
@attribute [StreamRendering(false)]
@attribute [RenderModeServer] // or omit for static SSR
```

Since the page is read-only with no user interaction, **Static SSR is actually the better render mode**—it sends plain HTML with no SignalR connection, eliminating the reconnection banner issue entirely.

---

## 7. Open Questions

| # | Question | Who Decides | Impact |
|---|----------|-------------|--------|
| 1 | **Should the dashboard auto-refresh when `data.json` changes?** Using `FileSystemWatcher` + SignalR push is trivial in Blazor Server but adds complexity. For screenshot use, manual F5 is likely sufficient. | Product Owner | Low—affects developer experience only |
| 2 | **How many months should the heatmap show?** The design shows 4 columns (Jan–Apr). Should this be configurable in `data.json` (e.g., 3, 6, or 12 months)? | Product Owner | Medium—affects grid CSS and data model |
| 3 | **Should the timeline date range be configurable?** Currently hardcoded to Jan–Jun in the SVG. Making it dynamic requires date-to-pixel math. | Engineer | Low—straightforward to implement |
| 4 | **Will there be multiple projects/reports?** If yes, should the app support switching between different `data.json` files (e.g., via URL parameter `?report=project-a`)? | Product Owner | Medium—affects routing and file loading |
| 5 | **What is the target browser?** Edge and Chrome render identically. Firefox has minor SVG differences. | User | Low—standardize on Edge for internal Microsoft use |
| 6 | **Should the "Now" line position be calculated from the system date or specified in `data.json`?** Auto-calculation is convenient; manual specification allows generating reports for past/future dates. | Product Owner | Low—trivial either way |
| 7 | **Is dark mode needed?** The design is light-only. Adding dark mode doubles the CSS effort. | Product Owner | Medium—recommend deferring to post-MVP |

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** Render the dashboard from `data.json` with visual fidelity to the design.

| Step | Task | Estimated Time |
|------|------|---------------|
| 1 | `dotnet new blazor -n ReportingDashboard --interactivity Server` | 5 min |
| 2 | Define `DashboardData.cs` model classes matching `data.json` schema | 30 min |
| 3 | Create `data.json` with fictional project data (match design content) | 30 min |
| 4 | Build `DashboardDataService.cs` to read and deserialize JSON | 20 min |
| 5 | Build `Dashboard.razor` with header section | 30 min |
| 6 | Build SVG timeline section with milestone rendering | 1–2 hours |
| 7 | Build heatmap grid with CSS Grid, row coloring, cell items | 1–2 hours |
| 8 | Fine-tune CSS to match design pixel-for-pixel | 1–2 hours |
| 9 | Test screenshot at 1920×1080 | 15 min |

### Phase 2: Polish (Optional, 1 day)

| Enhancement | Value |
|-------------|-------|
| Add CSS custom properties for theming | Easy color changes for different projects |
| Add `FileSystemWatcher` for live `data.json` reload | Edit JSON → page updates automatically |
| Add Playwright screenshot automation | One-command screenshot generation |
| Support multiple report files via URL parameter | `/dashboard?report=project-alpha` |
| Add print stylesheet | `Ctrl+P` produces clean PDF |

### Phase 3: Future Enhancements (Deferred)

| Enhancement | Notes |
|-------------|-------|
| SQLite storage for historical data | Track month-over-month changes |
| Data editor UI | Edit `data.json` through a form instead of raw JSON |
| Multiple dashboard layouts | Different views for different audiences |
| Export to PNG/PDF from the app | Eliminate manual screenshot step |

### Quick Wins

1. **Start with the CSS.** Port the `<style>` block from `OriginalDesignConcept.html` directly into `Dashboard.razor.css`. This gives you a working layout immediately.
2. **Use the HTML design as a template.** Copy the HTML structure from the design file into `Dashboard.razor`, then replace hardcoded text with `@Data.Property` bindings. This is faster than building from scratch.
3. **Fictional data should mirror the design.** Use the same categories (Shipped, In Progress, Carryover, Blockers) and similar item counts so the visual density matches the reference.

### `data.json` Example Structure

```json
{
  "header": {
    "title": "Project Phoenix Release Roadmap",
    "subtitle": "Engineering Division • Platform Modernization • April 2026",
    "backlogLink": "https://dev.azure.com/org/project",
    "reportDate": "2026-04-17"
  },
  "timelineTracks": [
    {
      "id": "m1",
      "name": "M1",
      "description": "Core API & Auth",
      "color": "#0078D4",
      "milestones": [
        { "label": "Jan 15", "date": "2026-01-15", "type": "checkpoint" },
        { "label": "Mar 20 PoC", "date": "2026-03-20", "type": "poc" },
        { "label": "May Prod", "date": "2026-05-01", "type": "production" }
      ]
    }
  ],
  "heatmap": {
    "columns": ["January", "February", "March", "April"],
    "highlightColumnIndex": 3,
    "rows": [
      {
        "category": "shipped",
        "label": "Shipped",
        "cellItems": [
          ["Auth service v1", "CI/CD pipeline"],
          ["User management API", "Logging framework"],
          ["Search indexer", "Rate limiting"],
          ["Dashboard v1", "Notification service"]
        ]
      },
      {
        "category": "in-progress",
        "label": "In Progress",
        "cellItems": [
          ["Data migration tool"],
          ["Caching layer"],
          ["Performance testing"],
          ["Mobile API gateway", "Batch processor"]
        ]
      },
      {
        "category": "carryover",
        "label": "Carryover",
        "cellItems": [
          [],
          ["Legacy API deprecation"],
          ["Schema migration v2"],
          ["Documentation update"]
        ]
      },
      {
        "category": "blockers",
        "label": "Blockers",
        "cellItems": [
          [],
          ["Vendor SDK delay"],
          [],
          ["Compliance review pending"]
        ]
      }
    ]
  }
}
```

### Minimal `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.ListenLocalhost(5000));
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

---

### Summary of NuGet Packages Required

| Package | Version | Required? | Purpose |
|---------|---------|-----------|---------|
| *None beyond the default Blazor Server template* | — | — | The default `dotnet new blazor` template includes everything needed |
| `Microsoft.Playwright` | 1.41+ | Optional | Automated screenshot capture |
| `bUnit` | 1.25+ | Optional | Component testing |
| `xUnit` | 2.7+ | Optional | Unit testing |

**Total additional dependencies for MVP: Zero.** This is an intentionally dependency-free project using only what ships with .NET 8.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/74396611117aa6e7f3c413495db8c158575098e1/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
