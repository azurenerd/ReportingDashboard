# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Static SSR web application built on .NET 8 that visualizes project milestone timelines and categorizes work items into a color-coded heatmap grid. The application reads all data from a local `data.json` file and renders a pixel-perfect 1920×1080 page optimized for screenshot capture and inclusion in PowerPoint executive decks.

**Primary Goals:**

1. **Single-page executive visibility** — Render a complete project health summary (timeline + heatmap) on one page with zero navigation.
2. **Screenshot-ready output** — Fixed 1920×1080 layout with no scrollbars, no loading states, no browser artifacts.
3. **Data-driven via JSON** — All content driven by a single `data.json` file; swap the file to report on a different project.
4. **Zero infrastructure** — No cloud services, no databases, no authentication, no external dependencies. `dotnet run` and screenshot.
5. **Visual fidelity** — Pixel-for-pixel match to the `OriginalDesignConcept.html` reference at 1920×1080.

**Architecture Principles:**

- **Simplicity over abstraction** — This is a reporting tool, not a platform. Minimize layers.
- **CSS from the design file** — Copy CSS verbatim from the HTML reference; do not abstract until baseline fidelity is confirmed.
- **Deterministic rendering** — Same `data.json` always produces identical visual output.
- **Zero external NuGet packages** — The web project uses only what ships with the default Blazor template.

---

## System Components

### 1. `ReportingDashboard.Web` — Blazor Static SSR Application

**Responsibility:** Serve the single-page dashboard at `http://localhost:5000`.

**Sub-components:**

#### 1.1 `Program.cs` — Application Host

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Configure Kestrel, register services, set up the Blazor pipeline |
| **Interfaces** | None (entry point) |
| **Dependencies** | `IDataService` registration, `appsettings.json` configuration binding |
| **Key Behavior** | Reads `Dashboard:DataFilePath` from configuration; registers `JsonFileDataService` as singleton; configures Kestrel to listen on `http://localhost:5000`; maps Blazor static SSR endpoints |

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.Configure<DashboardOptions>(
    builder.Configuration.GetSection("Dashboard"));
builder.Services.AddSingleton<IDataService, JsonFileDataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>();
app.Run();
```

#### 1.2 `DashboardOptions` — Configuration POCO

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Bind the `Dashboard` section of `appsettings.json` |
| **Properties** | `DataFilePath` (string, default `"./data.json"`) |
| **File** | `Models/DashboardOptions.cs` |

#### 1.3 `IDataService` / `JsonFileDataService` — Data Loading

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Read and deserialize `data.json` into a strongly-typed `DashboardData` model |
| **Interface** | `IDataService { DashboardData? GetData(); string? GetError(); }` |
| **Dependencies** | `IOptions<DashboardOptions>`, `System.Text.Json`, file system |
| **Lifecycle** | Singleton — reads file once at construction time |
| **Error Handling** | If file missing: stores error message "Could not load data.json — file not found at [path]". If malformed JSON: stores deserialization exception message. Never throws to callers. |
| **Data** | Returns `DashboardData` on success, `null` on failure. `GetError()` returns the error string. |

```csharp
public interface IDataService
{
    DashboardData? GetData();
    string? GetError();
}

public class JsonFileDataService : IDataService
{
    private readonly DashboardData? _data;
    private readonly string? _error;

    public JsonFileDataService(IOptions<DashboardOptions> options)
    {
        var path = options.Value.DataFilePath ?? "./data.json";
        try
        {
            if (!File.Exists(path))
            {
                _error = $"Could not load data.json — file not found at {Path.GetFullPath(path)}. "
                       + "Please create a data.json file. See data.sample.json for the expected schema.";
                return;
            }
            var json = File.ReadAllText(path);
            _data = JsonSerializer.Deserialize<DashboardData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _error = $"Could not parse data.json: {ex.Message}";
        }
    }

    public DashboardData? GetData() => _data;
    public string? GetError() => _error;
}
```

#### 1.4 Blazor Components (UI Layer)

Each component maps to a visual section of the `OriginalDesignConcept.html` design. All components are server-rendered with no interactivity mode (Static SSR). Data flows downward via `[Parameter]` attributes.

| Component | File | Design Section | Responsibility |
|-----------|------|---------------|----------------|
| `App` | `Components/App.razor` | `<html>` shell | Root component; renders `<head>`, CSS links, `<body>` with `@Body` |
| `MainLayout` | `Components/Layout/MainLayout.razor` | `<body>` | Minimal layout — no sidebar, no nav; just `@Body` |
| `Dashboard` | `Components/Pages/Dashboard.razor` | Full page | Route `/`; injects `IDataService`; passes data to children; shows error if data load failed |
| `DashboardHeader` | `Components/DashboardHeader.razor` | `.hdr` div | Title, subtitle, backlog link, legend icons |
| `TimelineSection` | `Components/TimelineSection.razor` | `.tl-area` div | Track labels sidebar + SVG timeline with computed milestone positions |
| `HeatmapGrid` | `Components/HeatmapGrid.razor` | `.hm-wrap` + `.hm-grid` | Heatmap title, column headers, row iteration, cell rendering |
| `HeatmapCell` | `Components/HeatmapCell.razor` | `.hm-cell` | Individual cell: bullet list of work items or gray dash for empty |

#### 1.5 Static Assets

| File | Purpose |
|------|---------|
| `wwwroot/css/app.css` | Global styles: CSS reset, body sizing (1920×1080), font stack, link color, print stylesheet |
| `*.razor.css` | Scoped component styles copied verbatim from `OriginalDesignConcept.html` CSS classes |

### 2. `ReportingDashboard.Tests` — Test Project

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Unit tests for data service and coordinate calculations |
| **Framework** | xUnit 2.7+ |
| **Additional packages** | bUnit 1.25+ (component tests), FluentAssertions 6.12+ |
| **Test Classes** | `JsonFileDataServiceTests`, `DateToXCalculationTests`, `DashboardComponentTests` |

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────┐    startup     ┌──────────────────────┐
│  data.json   │──────────────▶│  JsonFileDataService  │
│  (on disk)   │   File.Read   │  (Singleton)          │
└─────────────┘    + Deserialize└──────────┬───────────┘
                                           │
                                           │ DI injection
                                           ▼
┌─────────────┐    HTTP GET /  ┌──────────────────────┐
│   Browser    │──────────────▶│  Dashboard.razor      │
│  (Chrome)    │◀──────────────│  (OnInitialized)      │
└─────────────┘    HTML response└──────────┬───────────┘
                                           │
                              [Parameter]  │  [Parameter]
                         ┌─────────────────┼─────────────────┐
                         ▼                 ▼                 ▼
                ┌────────────────┐ ┌──────────────┐ ┌──────────────┐
                │DashboardHeader │ │TimelineSection│ │ HeatmapGrid  │
                │  .razor        │ │  .razor       │ │  .razor      │
                └────────────────┘ └──────────────┘ └──────┬───────┘
                                                           │
                                                    [Parameter]
                                                           ▼
                                                   ┌──────────────┐
                                                   │ HeatmapCell  │
                                                   │  .razor      │
                                                   └──────────────┘
```

### Communication Patterns

1. **File → Service (startup only):** `JsonFileDataService` reads `data.json` once during construction. No file watching. No re-reads. The user refreshes the browser to pick up changes (app restart via `dotnet watch` handles file changes during development).

2. **Service → Page (DI injection):** `Dashboard.razor` receives `IDataService` via `@inject`. In `OnInitialized()`, it calls `GetData()` and `GetError()`. If error is non-null, it renders an error message instead of the dashboard.

3. **Page → Child Components (parameters):** `Dashboard.razor` passes model slices to children:
   - `DashboardHeader` receives: `Title`, `Subtitle`, `BacklogUrl`, `CurrentDate`
   - `TimelineSection` receives: `TimelineData` (tracks, start/end dates, current date)
   - `HeatmapGrid` receives: `HeatmapData` (months, current month, categories)

4. **No upward communication:** There are no callbacks, no events, no state mutations. Data flows strictly downward.

5. **No inter-component communication:** Components do not communicate with each other. All coordination happens through the shared `DashboardData` model passed from the page.

### Request Lifecycle

```
1. User navigates to http://localhost:5000
2. Kestrel receives HTTP GET /
3. Blazor Static SSR renders Dashboard.razor
4. Dashboard.razor calls IDataService.GetData() (already loaded in memory)
5. Component tree renders synchronously — no async, no loading states
6. Complete HTML response sent to browser
7. Browser renders page — no JavaScript needed (Static SSR)
8. User sees fully-formed dashboard in < 2 seconds
```

---

## Data Model

### Entity Relationship Diagram

```
DashboardData (root)
├── title: string
├── subtitle: string
├── backlogUrl: string
├── currentDate: string (ISO date)
├── timeline: TimelineData
│   ├── startDate: string (ISO date)
│   ├── endDate: string (ISO date)
│   └── tracks[]: TimelineTrack
│       ├── id: string
│       ├── label: string
│       ├── color: string (hex)
│       └── milestones[]: Milestone
│           ├── date: string (ISO date)
│           ├── type: string (checkpoint|poc|production)
│           └── label: string
└── heatmap: HeatmapData
    ├── months[]: string
    ├── currentMonth: string
    └── categories[]: HeatmapCategory
        ├── name: string
        ├── colorClass: string (ship|prog|carry|block)
        └── items: Dictionary<string, string[]>
```

### C# Model Definitions

```csharp
// Models/DashboardData.cs
public class DashboardData
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string BacklogUrl { get; set; } = "";
    public string CurrentDate { get; set; } = "";
    public TimelineData Timeline { get; set; } = new();
    public HeatmapData Heatmap { get; set; } = new();
}

// Models/TimelineData.cs
public class TimelineData
{
    public string StartDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public List<TimelineTrack> Tracks { get; set; } = new();
}

// Models/TimelineTrack.cs
public class TimelineTrack
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Color { get; set; } = "";
    public List<Milestone> Milestones { get; set; } = new();
}

// Models/Milestone.cs
public class Milestone
{
    public string Date { get; set; } = "";
    public string Type { get; set; } = "";   // "checkpoint", "poc", "production"
    public string Label { get; set; } = "";
}

// Models/HeatmapData.cs
public class HeatmapData
{
    public List<string> Months { get; set; } = new();
    public string CurrentMonth { get; set; } = "";
    public List<HeatmapCategory> Categories { get; set; } = new();
}

// Models/HeatmapCategory.cs
public class HeatmapCategory
{
    public string Name { get; set; } = "";
    public string ColorClass { get; set; } = "";
    public Dictionary<string, List<string>> Items { get; set; } = new();
}
```

### Storage

- **Primary store:** `data.json` file on local disk.
- **Location:** Configurable via `appsettings.json` → `Dashboard:DataFilePath`. Default: `./data.json` (relative to working directory).
- **Format:** UTF-8 encoded JSON matching the schema above.
- **Size constraint:** Up to 1 MB without degradation.
- **No database.** No SQLite, no SQL Server, no in-memory DB.
- **Version control:** `data.json` is in `.gitignore`. `data.sample.json` is committed with fictional example data.

### `data.json` Schema Contract

The `data.sample.json` file serves as the living schema documentation. The C# POCO models are the authoritative schema definition — if JSON properties don't map to model properties, they are silently ignored. If required data is missing, the component renders gracefully (empty strings, empty lists).

---

## API Contracts

This application has **no REST API, no GraphQL endpoint, and no backend-for-frontend API**. It is a server-rendered HTML page.

### HTTP Endpoints

| Method | Path | Response | Description |
|--------|------|----------|-------------|
| `GET` | `/` | `200 OK` + HTML | Renders the full dashboard page via Blazor Static SSR |
| `GET` | `/_framework/*` | Static files | Blazor framework assets (auto-generated) |
| `GET` | `/css/app.css` | Static CSS | Global stylesheet |
| `GET` | `/*.styles.css` | Static CSS | Blazor CSS isolation bundle |

### Error Responses

| Condition | Behavior |
|-----------|----------|
| `data.json` not found | `GET /` returns `200 OK` with HTML containing a centered error message: "Error: Could not load data.json — file not found at [absolute path]. Please create a data.json file. See data.sample.json for the expected schema." |
| `data.json` malformed | `200 OK` with HTML error message including the `JsonException` message |
| Invalid route (e.g., `/foo`) | Default Blazor 404 handling — returns empty page or Blazor's built-in "not found" |

### No Outbound HTTP Calls

The application makes **zero outbound network requests**. No telemetry, no CDN fonts, no analytics, no API calls. All resources are served locally by Kestrel.

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|---------------|
| **Operating System** | Windows 10/11 (Segoe UI font dependency) |
| **Runtime** | .NET 8 SDK 8.0.x (LTS) |
| **Memory** | < 100 MB RAM |
| **Disk** | < 50 MB for application + data |
| **Network** | Loopback only (`localhost:5000`). No external network required. |
| **Display** | 1920×1080 capable display or virtual display for screenshot capture |
| **Browser** | Chrome 120+ or Edge 120+ (screenshot target) |

### Hosting

- **Development:** `dotnet watch run` from the `src/ReportingDashboard.Web/` directory. Kestrel serves on `http://localhost:5000`.
- **"Production":** `dotnet publish -c Release -o ./publish` → run `ReportingDashboard.Web.exe`. No IIS, no Docker, no reverse proxy.
- **No cloud services.** No Azure, no AWS, no GCP. Runs on the developer's local machine.

### Networking

- Kestrel binds to `http://localhost:5000` only.
- No HTTPS required for local-only use (optional: Kestrel auto-generates dev cert on `https://localhost:5001`).
- No firewall rules needed — loopback traffic only.

### Storage

- `data.json` — The sole data source. Read once at startup.
- `data.sample.json` — Committed to repo. Fictional example data for schema reference.
- `appsettings.json` — Configuration file with data file path.
- `wwwroot/` — Static assets served by Kestrel's static file middleware.

### CI/CD

**Out of scope for initial delivery.** No automated build/deploy pipeline.

Recommended future setup:
- `dotnet build` + `dotnet test` in a GitHub Actions workflow
- No deployment step — the artifact is a local developer tool

---

## Technology Stack Decisions

| Layer | Choice | Version | Justification |
|-------|--------|---------|---------------|
| **Runtime** | .NET 8 LTS | 8.0.x | Mandated by project requirements. LTS support through Nov 2026. |
| **UI Framework** | Blazor Static SSR | Ships with .NET 8 | Provides component model without SignalR overhead. No WebAssembly, no WebSocket — pure server-rendered HTML. Fastest possible page load. |
| **Layout** | Pure CSS Grid + Flexbox | N/A | Matches `OriginalDesignConcept.html` exactly. No CSS framework (Bootstrap, Tailwind) needed — they would fight the pixel-precise design. |
| **Timeline Rendering** | Inline SVG via Razor | N/A | The original design uses hand-authored SVG. Generated via `@foreach` loops in `TimelineSection.razor`. No charting library — they impose axes, legends, and responsive behavior that conflict with the fixed layout. |
| **Heatmap Rendering** | CSS Grid with semantic HTML | N/A | `grid-template-columns: 160px repeat(N, 1fr)` set dynamically. Matches original design's `.hm-grid`. |
| **CSS Architecture** | Blazor CSS isolation (`.razor.css`) | Built-in | One scoped CSS file per component. No Sass, no PostCSS, no build step. |
| **JSON Deserialization** | `System.Text.Json` | Ships with .NET 8 | Native, fast, zero dependencies. `PropertyNameCaseInsensitive = true` for flexible JSON authoring. |
| **Configuration** | `appsettings.json` + `IOptions<T>` | Built-in | Standard .NET configuration pattern. Supports environment overrides. |
| **Web Server** | Kestrel | Ships with ASP.NET Core | Built-in, no IIS or Nginx needed for local use. |
| **Testing** | xUnit + bUnit + FluentAssertions | 2.7+ / 1.25+ / 6.12+ | Industry standard .NET test stack. bUnit enables component testing without a browser. |

### Decisions Against Alternatives

| Rejected Option | Reason |
|----------------|--------|
| **MudBlazor / Radzen** | Adds CSS resets and theme systems that conflict with pixel-precise design. More effort to override than to write ~150 lines of CSS. |
| **Chart.js / ApexCharts** | Timeline is not a chart — it's positioned SVG markers. Charting libraries impose responsive behavior incompatible with fixed 1920×1080 layout. |
| **Blazor Server (interactive)** | Maintains unnecessary SignalR WebSocket. Static SSR produces pure HTML with zero JavaScript — ideal for a screenshot target. |
| **Blazor WebAssembly** | Downloads .NET runtime to browser. Adds loading delay. No benefit for a read-only, server-rendered page. |
| **SQLite / EF Core** | Zero benefit over a JSON file for a single-user, single-file data source. Adds migration complexity. |
| **Sass / PostCSS** | Adds build toolchain. CSS is small (~200 lines) and copied from the HTML template. No variables or mixins needed. |

---

## Security Considerations

### Authentication & Authorization

**None.** This is an intentional design decision — the application runs on `localhost` only, accessed by a single user on their own machine. Adding authentication would be wasted complexity with no security benefit.

### Data Protection

| Concern | Mitigation |
|---------|------------|
| **Sensitive project data in `data.json`** | `.gitignore` excludes `data.json` from version control. Only `data.sample.json` (fictional data) is committed. |
| **Data at rest** | `data.json` is a plaintext file on the user's local disk. Encryption is delegated to OS-level disk encryption (BitLocker). |
| **Data in transit** | Loopback only — data never leaves the machine. HTTPS optional via Kestrel dev cert. |

### Input Validation

| Vector | Mitigation |
|--------|------------|
| **Malformed JSON** | `System.Text.Json` throws `JsonException` on parse failure. Error message displayed to user. App does not crash. |
| **Missing fields** | C# models use default values (empty strings, empty lists). Components render gracefully with missing data. |
| **XSS via data.json** | Blazor's Razor engine HTML-encodes all `@` expressions by default. Even if `data.json` contains `<script>` tags in item text, they render as escaped text, not executable HTML. |
| **Path traversal** | `DataFilePath` is read from `appsettings.json` (developer-controlled). No user-facing file path input. |

### Network Security

- **No outbound calls.** The application makes zero HTTP requests to external services. No telemetry, no CDN fonts, no analytics scripts.
- **No inbound exposure.** Kestrel binds to `localhost` only. Not accessible from the network.
- **No secrets.** No API keys, no connection strings, no tokens. Nothing to leak.

### Supply Chain

- **Zero external NuGet packages** in the web project. Only the framework-provided `Microsoft.AspNetCore.App` implicit reference.
- **Test project** uses xUnit, bUnit, and FluentAssertions — well-established, widely-audited packages.

---

## Scaling Strategy

### Current Scale

This application is designed for **exactly one user on one machine**. It is a local developer tool, not a web service.

| Dimension | Current | Maximum Supported |
|-----------|---------|-------------------|
| **Concurrent users** | 1 | 1 (local only) |
| **Data size** | ~10 KB typical | 1 MB `data.json` |
| **Timeline tracks** | 3 typical | 5 (SVG height auto-scales) |
| **Heatmap months** | 4 typical | 6 (grid columns are dynamic) |
| **Items per cell** | 2-5 typical | 10 (overflow hidden at cell boundary) |

### Scaling Levers (if future needs arise)

| Need | Approach |
|------|----------|
| **Multiple projects** | Swap `data.json` files. Future: command-line `--data` argument or dropdown in UI. |
| **Team sharing** | `dotnet publish` → copy folder to another machine → `dotnet ReportingDashboard.Web.dll`. No infrastructure changes. |
| **Automated screenshots** | Phase 4: Add `Microsoft.Playwright` NuGet package. `/screenshot` endpoint generates PNG server-side. |
| **Live data refresh** | Future: Add `FileSystemWatcher` to `JsonFileDataService` to re-read `data.json` on file change. Requires switching to Blazor Server (interactive) for SignalR push to browser. |
| **Larger datasets** | If >10 items per cell, add CSS `overflow-y: auto` to `.hm-cell`. If >6 months, reduce `font-size` or paginate by quarter. |

### What We Explicitly Do NOT Scale For

- Multiple simultaneous users
- High availability or redundancy
- Geographic distribution
- Database sharding
- Load balancing
- Container orchestration

This is a screenshot tool, not a web service.

---

## Risks & Mitigations

### Risk 1: CSS Fidelity to Original Design

| Aspect | Detail |
|--------|--------|
| **Impact** | **High.** The deliverable is a pixel-perfect screenshot. Any visual deviation is a bug. |
| **Likelihood** | Medium. Blazor adds its own `<head>` content, CSS isolation scoping, and layout structure that could shift pixels. |
| **Mitigation** | (1) Copy CSS from `OriginalDesignConcept.html` verbatim — do not abstract or "improve" until baseline matches. (2) Use a side-by-side screenshot comparison at 1920×1080 as the acceptance test. (3) Disable all default Blazor template styles (remove Bootstrap, remove default layout margins). (4) Test in the same browser (Chrome/Edge) used for final screenshots. |

### Risk 2: SVG Coordinate Precision

| Aspect | Detail |
|--------|--------|
| **Impact** | **Medium.** Incorrect date-to-pixel math misplaces milestones, making the timeline misleading. |
| **Likelihood** | Low. The math is straightforward linear interpolation. |
| **Mitigation** | (1) Use `double` for all coordinate calculations. (2) Round only at render time: `x="@(Math.Round(xPos, 1))"`. (3) Write unit tests for `DateToX()` with known input/output pairs derived from the original SVG (e.g., Jan 1 → x=0, Jul 1 → x=1560, Apr 10 → x≈823). Minimum 5 test cases. |

**`DateToX` test cases:**

```csharp
[Theory]
[InlineData("2026-01-01", 0.0)]        // Start date → x=0
[InlineData("2026-06-30", 1560.0)]     // End date → x=SvgWidth
[InlineData("2026-04-01", 780.0)]      // Midpoint (approx)
[InlineData("2026-01-12", 104.0)]      // Known checkpoint from design SVG
[InlineData("2026-03-26", 745.0)]      // Known PoC milestone from design SVG
public void DateToX_ReturnsCorrectPosition(string date, double expectedX)
```

### Risk 3: data.json Schema Drift

| Aspect | Detail |
|--------|--------|
| **Impact** | **Low.** Only one user edits the file. |
| **Likelihood** | Low. |
| **Mitigation** | (1) Strongly-typed C# models are the schema contract. (2) `data.sample.json` committed to repo as reference. (3) `JsonFileDataService` catches deserialization errors and displays a human-readable message. (4) Default values on all model properties prevent null reference exceptions. |

### Risk 4: Blazor Static SSR Limitations

| Aspect | Detail |
|--------|--------|
| **Impact** | **Low.** Static SSR cannot do interactive features (no `@onclick`, no form handling). |
| **Likelihood** | Certain. This is by design. |
| **Mitigation** | CSS-only tooltips (`:hover` pseudo-class) work without JavaScript. If future interactivity is needed (e.g., filtering, project switching), switch the `Dashboard.razor` page to `@rendermode InteractiveServer`. This is a one-line change. |

### Risk 5: Font Rendering Consistency

| Aspect | Detail |
|--------|--------|
| **Impact** | **Low.** Segoe UI is a Windows system font. |
| **Likelihood** | Very low on Windows. Certain to differ on macOS/Linux. |
| **Mitigation** | (1) Fallback chain: `'Segoe UI', -apple-system, 'Helvetica Neue', Arial, sans-serif`. (2) Only Windows is in scope. (3) No web fonts to load — eliminates FOUT/FOIT rendering delays. |

### Risk 6: Large data.json Causes Layout Overflow

| Aspect | Detail |
|--------|--------|
| **Impact** | **Medium.** Too many items in a heatmap cell could overflow the fixed 1080px height. |
| **Likelihood** | Low. PM spec assumes 0-10 items per cell. |
| **Mitigation** | (1) `.hm-cell` has `overflow: hidden` (from original CSS). Content beyond the cell boundary is clipped. (2) `data.sample.json` documents the recommended maximum items per cell. (3) The heatmap rows use `1fr` sizing — they distribute available space equally. |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component with its CSS layout strategy, data bindings, and interactions.

### Component Tree

```
App.razor
└── MainLayout.razor                    <body> flex column
    └── Dashboard.razor                 Route "/" — full page
        ├── DashboardHeader.razor       .hdr section
        ├── TimelineSection.razor       .tl-area section
        └── HeatmapGrid.razor           .hm-wrap section
            └── HeatmapCell.razor       .hm-cell (repeated)
```

### Component 1: `DashboardHeader.razor` → `.hdr` div

| Aspect | Detail |
|--------|--------|
| **Design Section** | `.hdr` — top bar with title and legend |
| **CSS Layout** | `display: flex; align-items: center; justify-content: space-between; padding: 12px 44px 10px; border-bottom: 1px solid #E0E0E0; flex-shrink: 0` |
| **Parameters** | `[Parameter] string Title`, `[Parameter] string Subtitle`, `[Parameter] string BacklogUrl`, `[Parameter] string CurrentDate` |
| **Data Bindings** | `<h1>@Title <a href="@BacklogUrl">↗ ADO Backlog</a></h1>`, `<div class="sub">@Subtitle</div>` |
| **Interactions** | Backlog link is a standard `<a href>` — opens in browser. No JavaScript. |
| **Legend** | Four inline-flex spans with CSS-shaped icons: gold rotated square (PoC), green rotated square (Production), gray circle (Checkpoint), red rectangle (Now). All hardcoded — legend items are not data-driven. The "Now" label includes the month derived from `CurrentDate`. |
| **Scoped CSS** | `DashboardHeader.razor.css` — contains `.hdr`, `.hdr h1`, `.sub`, and inline legend styles. |

### Component 2: `TimelineSection.razor` → `.tl-area` div

| Aspect | Detail |
|--------|--------|
| **Design Section** | `.tl-area` — timeline with track labels and SVG |
| **CSS Layout** | `display: flex; align-items: stretch; padding: 6px 44px 0; height: 196px; background: #FAFAFA; border-bottom: 2px solid #E8E8E8; flex-shrink: 0` |
| **Parameters** | `[Parameter] TimelineData Timeline`, `[Parameter] string CurrentDate` |
| **Sub-sections** | **Left sidebar** (230px, flex-shrink: 0): Track labels rendered via `@foreach (var track in Timeline.Tracks)`. Each label shows `track.Id` in `track.Color` and `track.Label` in `#444`. **Right area** (flex: 1): Inline `<svg width="1560" height="185">` with computed elements. |
| **SVG Generation** | Month gridlines: computed from `Timeline.StartDate`/`EndDate`, one `<line>` + `<text>` per month at `DateToX(monthStart)`. Track lines: horizontal `<line>` at computed Y positions (`SvgHeight / (TrackCount + 1) * (trackIndex + 1)`). Milestones: positioned at `DateToX(milestone.Date)` with shape determined by `milestone.Type`. NOW line: dashed red `<line>` at `DateToX(CurrentDate)`. |
| **Key Algorithm** | `DateToX(DateTime date) → double`: Linear interpolation. `(date - startDate).TotalDays / (endDate - startDate).TotalDays * 1560.0`. All `double` precision; rounded to 1 decimal at render. |
| **Interactions** | **CSS-only tooltips on milestone hover.** Each milestone SVG group (`<g>`) contains a hidden `<title>` element for native browser tooltips, plus a CSS `:hover` rule that shows a positioned `<text>` or `<foreignObject>` with label + date + type. Tooltips are `:hover`-only and do not appear in screenshots. |
| **Scoped CSS** | `TimelineSection.razor.css` — contains `.tl-area`, `.tl-svg-box`, track label styles, tooltip styles. |

**Milestone rendering by type:**

| Type | SVG Element | Attributes |
|------|------------|------------|
| `checkpoint` (large) | `<circle>` | `r="7"`, `fill="white"`, `stroke="{track.Color}"`, `stroke-width="2.5"` |
| `checkpoint` (small) | `<circle>` | `r="4"`, `fill="#999"` |
| `poc` | `<polygon>` (diamond) | `points` computed from center ± 11px, `fill="#F4B400"`, `filter="url(#sh)"` |
| `production` | `<polygon>` (diamond) | Same geometry, `fill="#34A853"`, `filter="url(#sh)"` |

**Diamond point calculation:**

```csharp
private string DiamondPoints(double cx, double cy, double radius = 11)
{
    return $"{cx},{cy - radius} {cx + radius},{cy} {cx},{cy + radius} {cx - radius},{cy}";
}
```

### Component 3: `HeatmapGrid.razor` → `.hm-wrap` + `.hm-grid`

| Aspect | Detail |
|--------|--------|
| **Design Section** | `.hm-wrap` (wrapper) + `.hm-grid` (CSS Grid) |
| **CSS Layout** | Wrapper: `flex: 1; min-height: 0; display: flex; flex-direction: column; padding: 10px 44px 10px`. Grid: `display: grid; grid-template-columns: 160px repeat(@monthCount, 1fr); grid-template-rows: 36px repeat(4, 1fr); border: 1px solid #E0E0E0` |
| **Parameters** | `[Parameter] HeatmapData Heatmap` |
| **Dynamic Grid** | `monthCount` = `Heatmap.Months.Count`. The `grid-template-columns` style is set via inline `style` attribute: `style="grid-template-columns: 160px repeat(@(Heatmap.Months.Count), 1fr)"` |
| **Title** | `.hm-title`: "MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS" in uppercase gray. Hardcoded text. |
| **Corner Cell** | `.hm-corner`: Empty or "STATUS / MONTH" label. |
| **Column Headers** | `@foreach month in Heatmap.Months` → `.hm-col-hdr`. If `month == Heatmap.CurrentMonth`, add class `current-month-hdr` (maps to `.apr-hdr` styling: `background: #FFF0D0; color: #C07700`) and append "▸ Now" to text. |
| **Row Rendering** | `@foreach category in Heatmap.Categories` → Row header (`.hm-row-hdr` + `{colorClass}-hdr`) showing `@category.Name.ToUpper() (@totalCount)` where `totalCount = category.Items.Values.Sum(v => v.Count)`. Then `@foreach month in Heatmap.Months` → `<HeatmapCell>` component. |
| **Data Bindings** | Row header: `category.Name`, `category.ColorClass`, computed total count. Column headers: `Heatmap.Months[i]`, `Heatmap.CurrentMonth`. |
| **Scoped CSS** | `HeatmapGrid.razor.css` — contains `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, and all color variants (`.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr`). |

### Component 4: `HeatmapCell.razor` → `.hm-cell`

| Aspect | Detail |
|--------|--------|
| **Design Section** | `.hm-cell` — individual data cell in the heatmap |
| **CSS Layout** | `padding: 8px 12px; border-right: 1px solid #E0E0E0; border-bottom: 1px solid #E0E0E0; overflow: hidden` |
| **Parameters** | `[Parameter] List<string> Items`, `[Parameter] string ColorClass`, `[Parameter] bool IsCurrentMonth` |
| **CSS Classes** | Base: `hm-cell {colorClass}-cell`. If `IsCurrentMonth`: add `current-month` class (maps to darker background per color). |
| **Data Bindings** | If `Items` is empty: render `<div class="empty-dash" style="color:#AAA">-</div>`. Otherwise: `@foreach item in Items` → `<div class="it">@item</div>`. The `.it::before` pseudo-element renders the colored bullet dot. |
| **Scoped CSS** | `HeatmapCell.razor.css` — contains `.hm-cell`, `.it`, `.it::before`, empty dash styling, and all color variants (`.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`, plus `.current-month` variants for each). |

### Global Styles: `app.css`

```css
/* Reset */
* { margin: 0; padding: 0; box-sizing: border-box; }

/* Fixed page size for screenshot capture */
body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
    background: #FFFFFF;
    font-family: 'Segoe UI', -apple-system, 'Helvetica Neue', Arial, sans-serif;
    color: #111;
    display: flex;
    flex-direction: column;
}

a { color: #0078D4; text-decoration: none; }

/* Print stylesheet — hide browser chrome */
@media print {
    @page { size: 1920px 1080px; margin: 0; }
    body { width: 1920px; height: 1080px; }
}
```

### Layout: `MainLayout.razor`

```razor
@inherits LayoutComponentBase

@Body
```

No sidebar, no navigation, no header chrome. The layout is an empty shell — all visual structure lives in the `Dashboard.razor` page and its child components.

---

## Solution Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── .gitignore                              # Includes: data.json, bin/, obj/
├── src/
│   └── ReportingDashboard.Web/
│       ├── ReportingDashboard.Web.csproj   # net8.0, no extra NuGet packages
│       ├── Program.cs                       # Host config, DI, Kestrel on :5000
│       ├── appsettings.json                 # { "Dashboard": { "DataFilePath": "./data.json" } }
│       ├── appsettings.Development.json     # Development overrides
│       ├── data.sample.json                 # Fictional example data (committed)
│       ├── data.json                        # Real data (gitignored)
│       ├── Models/
│       │   ├── DashboardData.cs
│       │   ├── DashboardOptions.cs
│       │   ├── TimelineData.cs
│       │   ├── TimelineTrack.cs
│       │   ├── Milestone.cs
│       │   ├── HeatmapData.cs
│       │   └── HeatmapCategory.cs
│       ├── Services/
│       │   ├── IDataService.cs
│       │   └── JsonFileDataService.cs
│       ├── Components/
│       │   ├── App.razor                    # <html> root
│       │   ├── _Imports.razor               # Global @using statements
│       │   ├── Pages/
│       │   │   └── Dashboard.razor          # Route "/"
│       │   ├── Layout/
│       │   │   └── MainLayout.razor         # Empty shell
│       │   ├── DashboardHeader.razor        # + DashboardHeader.razor.css
│       │   ├── TimelineSection.razor        # + TimelineSection.razor.css
│       │   ├── HeatmapGrid.razor            # + HeatmapGrid.razor.css
│       │   └── HeatmapCell.razor            # + HeatmapCell.razor.css
│       └── wwwroot/
│           ├── css/
│           │   └── app.css                  # Global reset + body sizing + print
│           └── favicon.ico
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj   # xUnit, bUnit, FluentAssertions
        ├── Services/
        │   └── JsonFileDataServiceTests.cs   # Valid file, missing file, malformed JSON
        ├── Calculations/
        │   └── DateToXTests.cs               # 5+ test cases for coordinate math
        └── Components/
            └── DashboardComponentTests.cs    # bUnit render tests
```

### `appsettings.json`

```json
{
  "Dashboard": {
    "DataFilePath": "./data.json"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

### `.csproj` (Web)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

Zero additional NuGet package references. Everything ships with the framework.

### `.csproj` (Tests)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="bunit" Version="1.25.*" />
    <PackageReference Include="xunit" Version="2.7.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.*" />
    <PackageReference Include="FluentAssertions" Version="6.12.*" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\ReportingDashboard.Web\ReportingDashboard.Web.csproj" />
  </ItemGroup>
</Project>
```