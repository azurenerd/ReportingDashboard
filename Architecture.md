# Architecture

**Project:** Executive Project Reporting Dashboard (ReportingDashboard)
**Version:** 1.0
**Date:** April 17, 2026
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure

---

## Overview & Goals

The ReportingDashboard is a single-page Blazor Server application that renders an executive project status dashboard at a fixed 1920×1080 resolution, optimized for screenshot capture and direct paste into PowerPoint decks. All content is driven by a single `data.json` file — no database, no cloud services, no authentication.

### Architectural Goals

1. **Pixel-perfect visual fidelity** — Reproduce `OriginalDesignConcept.html` at ≥95% accuracy using Blazor components and CSS extracted from the design reference.
2. **Data-driven rendering** — 100% of visible content sourced from `wwwroot/data/data.json`; zero hardcoded display values in components.
3. **Zero-dependency simplicity** — No external NuGet packages, no JavaScript, no CSS frameworks. Build and run with `dotnet run` using only the .NET 8 SDK.
4. **Minimal footprint** — ≤15 files in the core project (excluding `obj/`, `bin/`). Single CSS file. Single data file. Five Razor components.
5. **Sub-second render** — Page load under 1 second on localhost; startup under 3 seconds.

### Architectural Style

**Monolithic single-project Blazor Server application** with a flat component hierarchy. No layers, no abstractions beyond a single data service. The architecture is intentionally simple — this is a read-only visualization tool, not a CRUD application.

```
┌─────────────────────────────────────────────────────┐
│                    Browser (1920×1080)                │
│  ┌─────────────────────────────────────────────────┐ │
│  │              Blazor Server Circuit               │ │
│  │  ┌──────────┬──────────────┬────────────────┐   │ │
│  │  │ Header   │  Timeline    │  HeatmapGrid   │   │ │
│  │  │ .razor   │  .razor      │  .razor         │   │ │
│  │  │          │  (inline SVG)│  ├─HeatmapCell  │   │ │
│  │  └──────────┴──────────────┴────────────────┘   │ │
│  │                     ▲                             │ │
│  │                     │ ProjectData (cascading)     │ │
│  │              Dashboard.razor                      │ │
│  └─────────────────────────────────────────────────┘ │
│                        ▲                              │
│                        │ SignalR (default circuit)    │
├────────────────────────┼──────────────────────────────┤
│                 Kestrel (localhost:5000)               │
│                        ▲                              │
│              ProjectDataService (singleton)            │
│                        ▲                              │
│              wwwroot/data/data.json                    │
└─────────────────────────────────────────────────────┘
```

---

## System Components

### 1. `Program.cs` — Application Entry Point

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure Kestrel, register DI services, set up Blazor Server middleware, configure static file serving |
| **Interfaces** | None (entry point) |
| **Dependencies** | `ProjectDataService`, ASP.NET Core built-in middleware |
| **Data** | Reads `appsettings.json` for port configuration |

**Key behaviors:**
- Registers `ProjectDataService` as a singleton via `builder.Services.AddSingleton<ProjectDataService>()`
- Calls `app.UseStaticFiles()` to serve `wwwroot/` content (CSS, data.json)
- Maps Blazor Server hub at `/_blazor`
- Configures Kestrel to listen on `http://localhost:5000`
- Disables HTTPS redirection (localhost-only, no certificate management)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<ProjectDataService>();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

### 2. `ProjectDataService` — Data Access Layer

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Read, deserialize, cache, and expose `data.json` as a strongly-typed `ProjectData` object. Handle parse errors gracefully. |
| **Interfaces** | `ProjectData? GetData()` / `string? GetError()` |
| **Dependencies** | `IWebHostEnvironment` (to resolve `wwwroot` path), `System.Text.Json` |
| **Data** | Reads `wwwroot/data/data.json` once at construction time |
| **Lifetime** | Singleton — constructed once, cached for app lifetime |

**Key behaviors:**
- Constructor reads `data.json` synchronously from `wwwroot/data/data.json` using `IWebHostEnvironment.WebRootPath`
- Deserializes using `System.Text.Json` with `PropertyNameCaseInsensitive = true`
- On success: stores `ProjectData` in a private field, `GetError()` returns `null`
- On failure (file missing, invalid JSON): stores the exception message, `GetData()` returns `null`
- No file watching — data is read once. Restart app to pick up changes.

```csharp
public class ProjectDataService
{
    private readonly ProjectData? _data;
    private readonly string? _error;

    public ProjectDataService(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.WebRootPath, "data", "data.json");
        try
        {
            var json = File.ReadAllText(path);
            _data = JsonSerializer.Deserialize<ProjectData>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _error = $"Error reading data.json: {ex.Message}";
        }
    }

    public ProjectData? GetData() => _data;
    public string? GetError() => _error;
}
```

### 3. `ProjectData.cs` — Data Model Records

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define the strongly-typed shape of `data.json` using C# records with safe defaults |
| **Interfaces** | Immutable record types (init-only properties) |
| **Dependencies** | None (pure data) |
| **Data** | Deserialized from JSON; held in memory by `ProjectDataService` |

Six record types in a single file (see [Data Model](#data-model) section for full definitions).

### 4. `Dashboard.razor` — Page Component (Route: `/`)

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Inject `ProjectDataService`, handle error state, compose child components, pass data as parameters |
| **Interfaces** | Blazor page at route `/` |
| **Dependencies** | `ProjectDataService` (injected), `Header`, `Timeline`, `HeatmapGrid` child components |
| **Data** | Receives `ProjectData` from service; passes to children as component parameters |

**Key behaviors:**
- If `GetError()` is non-null, renders a styled error message div instead of the dashboard
- If `GetData()` returns valid data, renders the three-section layout: `<Header>`, `<Timeline>`, `<HeatmapGrid>`
- Passes `ProjectData` to each child component via `[Parameter]` attributes
- No cascading parameters needed — direct parameter passing keeps the component tree explicit

```razor
@page "/"
@inject ProjectDataService DataService

@if (DataService.GetError() is string error)
{
    <div class="error-banner">@error</div>
}
else if (DataService.GetData() is ProjectData data)
{
    <Header Data="data" />
    <Timeline Data="data" />
    <HeatmapGrid Data="data" />
}
```

### 5. `Header.razor` — Header Bar Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render project name, ADO Backlog link (conditional), subtitle, and legend icons |
| **Interfaces** | `[Parameter] ProjectData Data` |
| **Dependencies** | None |
| **Data** | `Data.ProjectName`, `Data.BacklogUrl`, `Data.Workstream`, `Data.Period`, `Data.Organization` |
| **CSS classes** | `.hdr`, `.sub` |

**Key behaviors:**
- Conditionally renders the `🔗 ADO Backlog` link only when `Data.BacklogUrl` is non-empty
- Legend items are hardcoded HTML (PoC diamond, Production diamond, Checkpoint circle, Now bar) — these are fixed design elements, not data-driven
- Subtitle formatted as `{Organization} · {Workstream} · {Period}`

### 6. `Timeline.razor` — SVG Timeline Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the complete milestone timeline as inline SVG: month gridlines, track lines, event markers (checkpoint/PoC/production), NOW line, date labels, track sidebar |
| **Interfaces** | `[Parameter] ProjectData Data` |
| **Dependencies** | None (pure rendering) |
| **Data** | `Data.Tracks`, `Data.TimelineStartDate`, `Data.TimelineEndDate`, `Data.NowDate`, `Data.TimelineMonths` |
| **CSS classes** | `.tl-area`, `.tl-svg-box` |

**Key behaviors:**
- **Date-to-pixel mapping** in `@code` block:
  ```csharp
  private const int SvgWidth = 1560;
  private const int SvgHeight = 185;
  
  private double DateToX(DateOnly date)
  {
      var totalDays = Data.TimelineEndDate.DayNumber - Data.TimelineStartDate.DayNumber;
      if (totalDays <= 0) return 0;
      var dayOffset = date.DayNumber - Data.TimelineStartDate.DayNumber;
      return Math.Clamp((double)dayOffset / totalDays * SvgWidth, 0, SvgWidth);
  }
  
  private double GetTrackY(int trackIndex, int totalTracks)
  {
      var usableHeight = SvgHeight - 28; // reserve top for labels
      var spacing = usableHeight / (totalTracks + 1);
      return 28 + spacing * (trackIndex + 1);
  }
  ```
- **Track sidebar** rendered as a `<div>` outside the SVG (230px wide, flex-shrink 0)
- **SVG elements** rendered via `@foreach` loops over tracks and events
- **Diamond polygon** helper: generates `points` attribute for a diamond centered at (cx, cy) with radius r
  ```csharp
  private string DiamondPoints(double cx, double cy, double r = 11) =>
      $"{cx},{cy - r} {cx + r},{cy} {cx},{cy + r} {cx - r},{cy}";
  ```
- **Drop shadow filter** declared in `<defs>` for milestone diamonds
- **SVG `<title>` elements** on milestone markers for native browser tooltips on hover

### 7. `HeatmapGrid.razor` — Heatmap Container Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the section title, CSS Grid container, header row (corner + month headers), and delegate each data cell to `HeatmapCell` |
| **Interfaces** | `[Parameter] ProjectData Data` |
| **Dependencies** | `HeatmapCell` child component |
| **Data** | `Data.Months`, `Data.CurrentMonthIndex`, `Data.Categories` |
| **CSS classes** | `.hm-wrap`, `.hm-grid`, `.hm-title`, `.hm-corner`, `.hm-col-hdr`, `.hm-col-hdr.current-month-hdr`, `.hm-row-hdr` |

**Key behaviors:**
- Sets `grid-template-columns` dynamically: `160px repeat({Data.Months.Count}, 1fr)`
- Renders header row: corner cell ("STATUS") + one `.hm-col-hdr` per month
- Current month header gets `.current-month-hdr` class (amber background) and "▸ Now" suffix
- For each category, renders one `.hm-row-hdr` + N `HeatmapCell` components

### 8. `HeatmapCell.razor` — Individual Grid Cell Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render a single heatmap cell with its list of work items or an empty-state dash |
| **Interfaces** | `[Parameter] string CssClass`, `[Parameter] List<string> Items`, `[Parameter] bool IsCurrentMonth` |
| **Dependencies** | None |
| **Data** | List of item strings from the parent component |
| **CSS classes** | `.hm-cell`, `.{category}-cell`, `.current`, `.it` |

**Key behaviors:**
- Applies category CSS class (e.g., `ship-cell`, `prog-cell`) and optional `current` class for current month
- If `Items` is empty, renders a gray dash `<span style="color:#999">—</span>`
- Otherwise, renders each item as `<div class="it">@item</div>` — the `::before` pseudo-element provides the colored bullet via CSS

### 9. `MainLayout.razor` — Layout Wrapper

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Minimal layout that renders `@Body` with no navigation, no sidebar, no framework chrome |
| **Interfaces** | Blazor layout component |
| **Dependencies** | None |
| **Data** | None |

```razor
@inherits LayoutComponentBase
@Body
```

The associated `MainLayout.razor.css` is empty or contains only the body reset — all styling lives in `app.css`.

### 10. `App.razor` — Root Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | HTML `<head>` setup (charset, viewport meta, CSS link, title), render `<Routes>` with error boundary |
| **Interfaces** | Blazor root component |
| **Dependencies** | `Routes.razor` |

**Key behaviors:**
- Links to `css/app.css`
- Sets `<meta name="viewport" content="width=1920">` for fixed viewport
- Wraps `<Routes>` in an `<ErrorBoundary>` with a fallback that shows a styled error message
- Does NOT include any Blazor reconnect UI customization (suppress default reconnect dialog via CSS)

### 11. `app.css` — Complete Stylesheet

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | All visual styling for the dashboard, extracted verbatim from `OriginalDesignConcept.html` with CSS custom properties added |
| **Size** | ~200 lines |
| **Strategy** | Copy CSS from the design HTML, add `:root` custom properties block, add `.error-banner` styling, add `@media print` rules |

### 12. `data.json` — Project Data File

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Single source of truth for all dashboard content |
| **Location** | `wwwroot/data/data.json` |
| **Size** | <50KB |
| **Format** | JSON matching the `ProjectData` record schema |

---

## Component Interactions

### Data Flow Diagram

```
                    ┌──────────────────┐
                    │   data.json      │
                    │ (wwwroot/data/)   │
                    └────────┬─────────┘
                             │ File.ReadAllText (once, at startup)
                             ▼
                    ┌──────────────────┐
                    │ProjectDataService│
                    │   (singleton)    │
                    │                  │
                    │ _data: ProjectData│
                    │ _error: string?  │
                    └────────┬─────────┘
                             │ DI injection
                             ▼
                    ┌──────────────────┐
                    │ Dashboard.razor  │
                    │   (page, "/")    │
                    └──┬─────┬─────┬──┘
                       │     │     │    [Parameter] Data
                       ▼     ▼     ▼
               ┌───────┐ ┌──────┐ ┌────────────┐
               │Header │ │Time- │ │HeatmapGrid │
               │.razor │ │line  │ │.razor       │
               └───────┘ │.razor│ └──────┬─────┘
                          └──────┘        │ [Parameter] per cell
                                          ▼
                                  ┌─────────────┐
                                  │HeatmapCell  │
                                  │.razor (×N)  │
                                  └─────────────┘
```

### Communication Patterns

| Pattern | Description |
|---------|-------------|
| **DI Injection** | `Dashboard.razor` receives `ProjectDataService` via `@inject`. One-way, read-only. |
| **Component Parameters** | Parent→child data flow via `[Parameter]` properties. No callbacks, no two-way binding, no events. |
| **SignalR Circuit** | Default Blazor Server circuit. Used only for initial page render. No interactive updates. |
| **Static Files** | `app.css` served via `UseStaticFiles()` middleware from `wwwroot/css/`. |

### Render Pipeline (Single Request)

1. Browser navigates to `http://localhost:5000/`
2. Kestrel receives HTTP request
3. Blazor Server middleware handles the request
4. `App.razor` renders the HTML shell with CSS `<link>`
5. `Dashboard.razor` calls `DataService.GetData()` synchronously
6. If data is valid: `Header`, `Timeline`, and `HeatmapGrid` render in sequence
7. `HeatmapGrid` renders N×4 `HeatmapCell` instances (N = number of months)
8. Complete HTML is sent to browser via SignalR
9. Browser renders at 1920×1080 — ready for screenshot

**There are no subsequent interactions.** The page is static after initial render. No button clicks, no form submissions, no AJAX calls, no periodic refreshes.

---

## Data Model

### Entity Relationship

```
ProjectData (root)
├── 1:N  MilestoneTrack
│         ├── 1:N  MilestoneEvent
├── 1:N  StatusCategory
│         ├── 1:N  MonthItems
│                   ├── 1:N  string (work item text)
├── 1:N  TimelineMonth
└── scalar fields (projectName, workstream, period, etc.)
```

### Record Definitions

```csharp
// Models/ProjectData.cs — all records in a single file

public record ProjectData
{
    public int SchemaVersion { get; init; } = 1;
    public string ProjectName { get; init; } = "";
    public string Organization { get; init; } = "";
    public string Workstream { get; init; } = "";
    public string Period { get; init; } = "";           // "April 2026"
    public string BacklogUrl { get; init; } = "";       // optional; empty = no link
    public string NowDate { get; init; } = "";          // "2026-04-17" ISO format
    public string TimelineStartDate { get; init; } = "";// "2026-01-01"
    public string TimelineEndDate { get; init; } = "";  // "2026-06-30"
    public List<string> Months { get; init; } = [];     // ["Jan","Feb","Mar","Apr"]
    public int CurrentMonthIndex { get; init; }          // 0-based
    public List<TimelineMonth> TimelineMonths { get; init; } = [];
    public List<MilestoneTrack> Tracks { get; init; } = [];
    public List<StatusCategory> Categories { get; init; } = [];
}

public record TimelineMonth
{
    public string Label { get; init; } = "";            // "Jan", "Feb"
    public string StartDate { get; init; } = "";        // "2026-01-01"
}

public record MilestoneTrack
{
    public string Id { get; init; } = "";               // "M1", "M2", "M3"
    public string Label { get; init; } = "";            // "Chatbot & MS Role"
    public string Color { get; init; } = "";            // "#0078D4"
    public List<MilestoneEvent> Events { get; init; } = [];
}

public record MilestoneEvent
{
    public string Date { get; init; } = "";             // "2026-01-12"
    public string Label { get; init; } = "";            // "Jan 12"
    public string Type { get; init; } = "";             // "checkpoint", "checkpoint-minor", "poc", "production"
    public string LabelPosition { get; init; } = "above"; // "above" or "below" — avoid overlap
    public int Radius { get; init; } = 7;               // checkpoint circle radius (default 7)
}

public record StatusCategory
{
    public string Name { get; init; } = "";             // "Shipped"
    public string Emoji { get; init; } = "";            // "✅"
    public string CssClass { get; init; } = "";         // "ship", "prog", "carry", "block"
    public List<MonthItems> MonthData { get; init; } = [];
}

public record MonthItems
{
    public string Month { get; init; } = "";            // "Jan"
    public List<string> Items { get; init; } = [];      // ["API Gateway v2 deployed", ...]
}
```

### Design Decisions on Data Model

| Decision | Rationale |
|----------|-----------|
| **`DateOnly` parsing in components, not records** | Records store dates as ISO strings for JSON simplicity. Components parse to `DateOnly` in `@code` blocks using `DateOnly.Parse()`. This keeps the data model JSON-friendly without custom converters. |
| **`LabelPosition` on events** | The design shows date labels above or below track lines. Rather than implement collision detection, let the data author specify position explicitly. |
| **`Type: "checkpoint-minor"`** | The design has two checkpoint styles: major (white fill, colored stroke, r=5-7) and minor (solid #999 fill, r=4). Distinguishing them in data avoids guessing. |
| **`TimelineMonths` separate from `Months`** | Heatmap months (e.g., 4 months) may differ from timeline months (e.g., 6 months). Timeline months include start dates for gridline positioning. |
| **All defaults are empty/zero** | Missing fields produce empty strings and empty lists — not nulls. This prevents `NullReferenceException` throughout the component tree. |

### Storage

| Aspect | Detail |
|--------|--------|
| **Storage engine** | Local filesystem — single JSON file |
| **Location** | `wwwroot/data/data.json` (served as static file, also read by service) |
| **Read pattern** | Read once at app startup; cached in memory for app lifetime |
| **Write pattern** | Manual edit by project manager in any text editor |
| **Backup** | Source control (git) — the `data.json` file is committed to the repo |
| **Migration** | `schemaVersion` field enables future migration logic in `ProjectDataService` |

---

## API Contracts

### HTTP Endpoints

This application exposes **no custom API endpoints**. All communication happens through the default Blazor Server SignalR circuit.

| Endpoint | Method | Purpose | Source |
|----------|--------|---------|--------|
| `/` | GET | Serves the dashboard page (Blazor Server HTML) | Blazor routing |
| `/_blazor` | WebSocket | Blazor Server SignalR circuit (framework-managed) | ASP.NET Core |
| `/css/app.css` | GET | Static file serving | `UseStaticFiles()` |
| `/data/data.json` | GET | Static file serving (browser-accessible but unused by app) | `UseStaticFiles()` |

### Internal Service Contract

```csharp
// ProjectDataService — injected into Dashboard.razor
public class ProjectDataService
{
    /// Returns the parsed project data, or null if parsing failed.
    public ProjectData? GetData();
    
    /// Returns the error message if data.json could not be read/parsed, or null on success.
    public string? GetError();
}
```

### Error Handling Contract

| Scenario | Behavior | User-Visible Output |
|----------|----------|-------------------|
| `data.json` missing | `GetError()` returns `"Error reading data.json: Could not find file '...data.json'"` | Red error banner on dashboard page |
| `data.json` invalid JSON | `GetError()` returns `"Error reading data.json: '{char}' is invalid after a value. LineNumber: {n}"` | Red error banner with line number |
| `data.json` valid but empty | `GetData()` returns `ProjectData` with all defaults | Dashboard renders with empty content (no crash) |
| Missing optional field | Default value used (empty string, empty list, 0) | Graceful degradation — feature simply not displayed |
| Unhandled exception in component | Blazor `<ErrorBoundary>` catches and displays fallback | Styled error message instead of blank page |

### data.json Schema Contract (v1)

```json
{
  "schemaVersion": 1,
  "projectName": "string (required)",
  "organization": "string (required)",
  "workstream": "string (required)",
  "period": "string (required, e.g., 'April 2026')",
  "backlogUrl": "string (optional, empty = no link)",
  "nowDate": "string (ISO date, e.g., '2026-04-17')",
  "timelineStartDate": "string (ISO date)",
  "timelineEndDate": "string (ISO date)",
  "months": ["string array (heatmap column labels)"],
  "currentMonthIndex": "integer (0-based)",
  "timelineMonths": [
    { "label": "string", "startDate": "string (ISO date)" }
  ],
  "tracks": [
    {
      "id": "string",
      "label": "string",
      "color": "string (hex)",
      "events": [
        {
          "date": "string (ISO date)",
          "label": "string",
          "type": "checkpoint | checkpoint-minor | poc | production",
          "labelPosition": "above | below (default: above)",
          "radius": "integer (default: 7, for checkpoints)"
        }
      ]
    }
  ],
  "categories": [
    {
      "name": "string",
      "emoji": "string",
      "cssClass": "ship | prog | carry | block",
      "monthData": [
        { "month": "string", "items": ["string array"] }
      ]
    }
  ]
}
```

---

## Infrastructure Requirements

### Runtime Requirements

| Requirement | Detail |
|-------------|--------|
| **.NET SDK** | .NET 8.0 LTS (any 8.0.x patch) |
| **OS** | Windows 10/11 (primary), macOS/Linux (compatible but untested for font fidelity) |
| **Font** | Segoe UI (pre-installed on Windows; falls back to Arial on other OS) |
| **Browser** | Chrome 120+ (primary for screenshots), Edge, Firefox (secondary) |
| **Disk** | <10MB total (source + build output) |
| **RAM** | <100MB RSS at runtime (including .NET runtime overhead) |
| **Network** | None — fully offline after `dotnet restore` (which uses cached SDK packages) |

### Hosting

| Aspect | Configuration |
|--------|---------------|
| **Web server** | Kestrel (built-in, no IIS, no reverse proxy) |
| **Binding** | `http://localhost:5000` (HTTP only, no TLS) |
| **Process model** | Single process, started/stopped manually via `dotnet run` / Ctrl+C |
| **Static files** | Served from `wwwroot/` by Kestrel's static file middleware |

### Port Configuration

Default port in `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "ReportingDashboard": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### Build & Deploy

| Command | Purpose |
|---------|---------|
| `dotnet build` | Compile the project (verifies no errors) |
| `dotnet run` | Build + run locally on `http://localhost:5000` |
| `dotnet publish -c Release -o ./publish` | Produce a deployment-ready folder |
| `dotnet publish -c Release --self-contained -r win-x64` | Self-contained exe for machines without .NET SDK |

### CI/CD

**Not required.** This is a local-only tool for a single developer/PM. Version control via git is sufficient. If CI is desired in the future:

- A single GitHub Actions workflow running `dotnet build` and `dotnet test` would suffice
- No deployment pipeline needed (there's nothing to deploy to)

### File System Layout (Deployed)

```
ReportingDashboard/
├── ReportingDashboard.sln              # Solution file
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj    # Project file (.NET 8, no external packages)
│       ├── Program.cs                   # Entry point + DI + middleware
│       ├── Models/
│       │   └── ProjectData.cs           # 6 record types
│       ├── Services/
│       │   └── ProjectDataService.cs    # JSON file reader (singleton)
│       ├── Components/
│       │   ├── App.razor                # Root component (HTML head, CSS link)
│       │   ├── Routes.razor             # Router configuration
│       │   ├── Pages/
│       │   │   └── Dashboard.razor      # The single page
│       │   ├── Layout/
│       │   │   └── MainLayout.razor     # Minimal layout (@Body only)
│       │   ├── Header.razor             # Title, subtitle, legend
│       │   ├── Timeline.razor           # SVG milestone timeline
│       │   ├── HeatmapGrid.razor        # CSS Grid container
│       │   └── HeatmapCell.razor        # Individual grid cell
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── app.css              # All styles
│       │   └── data/
│       │       └── data.json            # Project data
│       ├── Properties/
│       │   └── launchSettings.json      # Port configuration
│       └── appsettings.json             # Minimal (logging level only)
```

**Core file count: 14 files** (excluding `obj/`, `bin/`, `.sln`, `Properties/`, and test project) — within the ≤15 file budget.

---

## Technology Stack Decisions

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Runtime** | .NET 8 LTS | 8.0.x | Long-term support until Nov 2026. Required by project mandate. |
| **UI Framework** | Blazor Server | 8.0.x (built-in) | Server-side rendering with zero client payload. Simplest hosting model for local use. No WASM download delay. |
| **Markup** | Razor Components (.razor) | Built-in | C# + HTML in a single file. Direct SVG markup emission without JS interop. |
| **CSS** | Single `app.css` + CSS custom properties | N/A | Verbatim reproduction of `OriginalDesignConcept.html` styles. No framework needed for a fixed-width, single-page layout. |
| **Layout** | CSS Grid + Flexbox | N/A | Direct match to the design reference. Grid for the heatmap matrix, Flexbox for page-level vertical stacking and header horizontal alignment. |
| **Timeline rendering** | Inline SVG in Razor | N/A | The original design already uses SVG. Blazor emits SVG elements via `@foreach` loops — cleaner and more controllable than any charting library. Zero JavaScript. |
| **JSON** | `System.Text.Json` | Built-in (8.0.x) | Native, fast, zero-dependency JSON parsing. `PropertyNameCaseInsensitive` handles camelCase/PascalCase flexibility. |
| **DI** | Built-in DI container | Built-in | Standard ASP.NET Core DI. Single singleton registration for `ProjectDataService`. |
| **Web Server** | Kestrel | Built-in | Default ASP.NET Core web server. No IIS, no Nginx configuration needed. |
| **Testing** | xUnit + bUnit (optional) | 2.7.x / 1.25.x | Industry-standard .NET test framework. bUnit enables component-level testing of rendered HTML output. |

### Explicitly Rejected Technologies

| Technology | Reason for Rejection |
|-----------|---------------------|
| **Blazor WebAssembly** | Adds client-side download latency (~2-5MB). No benefit for a local tool with no offline requirement. |
| **MudBlazor / Radzen** | Component libraries impose their own design system. Fighting a component library to match a custom pixel-perfect design is harder than writing 5 small components from scratch. |
| **Chart.js / D3 / ApexCharts** | Adds 200KB+ of JavaScript. The timeline SVG is ~50 lines of Razor — a charting library would be over-engineering. |
| **Bootstrap / Tailwind** | CSS frameworks add unused weight. The design is a single fixed-width page with ~150 lines of custom CSS. A grid system adds nothing. |
| **Entity Framework** | No database exists. Zero entities to map. |
| **SQLite** | A flat JSON file is simpler and sufficient for <50KB of project data edited by one person. |
| **SignalR (custom hubs)** | No real-time communication needed. The default Blazor circuit handles initial page render. |
| **SASS / LESS** | No build step complexity for ~200 lines of CSS. CSS custom properties provide the variable support needed. |

---

## Security Considerations

### Threat Model

This is a **localhost-only, single-user, read-only** application displaying non-sensitive project status data. The attack surface is minimal.

| Threat | Risk Level | Mitigation |
|--------|-----------|------------|
| **Unauthorized network access** | Low | Kestrel binds to `localhost` only by default. Not accessible from LAN unless explicitly reconfigured. |
| **Malicious data.json injection** | Very Low | JSON is deserialized into strongly-typed records. No SQL, no eval, no dynamic code execution. HTML output is Razor-encoded by default (XSS-safe). |
| **Path traversal via data.json** | N/A | `ProjectDataService` reads a hardcoded path (`wwwroot/data/data.json`). No user-supplied file paths. |
| **Secrets in source** | N/A | No API keys, no connection strings, no credentials anywhere in the project. |
| **HTTPS / TLS** | Not required | HTTP on localhost is standard practice. No data in transit leaves the machine. |
| **Dependency vulnerabilities** | Very Low | Zero external NuGet packages. The only dependency is the .NET 8 SDK itself, patched via standard Windows Update. |

### Input Validation

| Input | Validation |
|-------|-----------|
| `data.json` | Deserialized via `System.Text.Json` with strict typing. Invalid JSON throws `JsonException` — caught and displayed as error message. No `dynamic`, no `JObject`, no untyped access. |
| URL parameters | None accepted. Single route `/` with no query parameters. |
| Form inputs | None exist. The dashboard is read-only. |
| File uploads | None exist. |

### Future Network Sharing (Deferred)

If the dashboard ever needs to be accessible on a local network:

```csharp
// Add to Program.cs (future, not implemented now)
app.UseMiddleware<BasicAuthMiddleware>();

public class BasicAuthMiddleware
{
    // Single shared password from appsettings.json
    // HTTP Basic Auth challenge/response
}
```

**This is explicitly out of scope for v1.** Document it here for future reference only.

### Blazor-Specific Security

| Concern | Approach |
|---------|----------|
| **XSS** | Razor automatically HTML-encodes all `@variable` output. No `@((MarkupString)...)` used except for the SVG (which contains only numeric coordinates from parsed data, not user-supplied HTML). |
| **SignalR circuit** | Default Blazor Server circuit with default settings. No custom hub methods exposed. |
| **Error exposure** | `<ErrorBoundary>` wraps the entire page. Unhandled exceptions show a generic message, not stack traces (in Production environment). In Development, full error details are shown for debugging. |
| **Reconnect UI** | Blazor's default reconnect dialog is suppressed via CSS (`#components-reconnect-modal { display: none; }`) to maintain screenshot cleanliness. |

---

## Scaling Strategy

### Current Scale (v1)

| Dimension | Capacity |
|-----------|----------|
| **Users** | 1 (single developer/PM on localhost) |
| **Data size** | <50KB JSON (sufficient for 6 months × 4 categories × 10 items each) |
| **Pages** | 1 |
| **Projects** | 1 per `data.json` (swap the file for a different project) |
| **Concurrent connections** | 1 (Blazor Server circuit) |

### Scaling is Not a Goal

This is a screenshot tool. It does not need to scale. The architecture is explicitly designed for **one user on one machine viewing one page with one data file**.

### If Scale Were Ever Needed (Hypothetical)

| Scenario | Approach |
|----------|----------|
| **Multiple projects** | Add a `projects/` directory with one JSON file per project. Add a dropdown or route parameter (`/project/{name}`) to select which file to load. ~30 minutes of work. |
| **Multiple users on LAN** | Change Kestrel binding from `localhost` to `0.0.0.0`. Add `BasicAuthMiddleware`. Blazor Server handles multiple SignalR circuits natively. |
| **Larger data sets** | The current architecture handles 50KB+ JSON trivially. For 500KB+ (unlikely), add pagination to the heatmap or switch to a streaming JSON reader. |
| **Cloud deployment** | Publish as an Azure App Service or container. The app is already a standard ASP.NET Core app — no code changes needed. Add HTTPS and authentication at that point. |

### Performance Budget

| Metric | Budget | Architecture Impact |
|--------|--------|-------------------|
| **Startup** | <3s | Singleton service reads JSON synchronously at startup. No async initialization needed. |
| **First render** | <1s | All data is in memory. Component tree is 5 components deep. No database queries, no HTTP calls. |
| **Memory** | <100MB | One `ProjectData` object in memory (~10KB). Rest is .NET runtime overhead. |
| **SVG complexity** | 5 tracks × 20 events | Linear rendering: `O(tracks × events)` for SVG elements. Even 100 events would render in <50ms. |

---

## Risks & Mitigations

### Risk 1: SVG Date-to-Pixel Mapping Errors

| Attribute | Detail |
|-----------|--------|
| **Severity** | Medium |
| **Probability** | Medium |
| **Impact** | Milestones appear at wrong positions on the timeline; visually incorrect screenshots |
| **Root Cause** | Off-by-one errors in date arithmetic, incorrect handling of month boundaries, division-by-zero on zero-length date ranges |

**Mitigations:**
1. Use `DateOnly` (not `DateTime`) for all date operations — eliminates time zone and time-of-day issues
2. Write unit tests for `DateToX()` with edge cases: start of month, end of month, same-day events, events outside range, single-day range
3. Clamp output to `[0, SvgWidth]` to prevent markers from rendering outside the SVG viewport
4. Include a `timelineStartDate` and `timelineEndDate` in `data.json` rather than computing from event dates — gives the author explicit control

### Risk 2: CSS Grid Visual Inconsistencies Across Browsers

| Attribute | Detail |
|-----------|--------|
| **Severity** | Low |
| **Probability** | Low |
| **Impact** | Heatmap grid cells have slightly different sizes in Firefox vs Chrome |

**Mitigations:**
1. Standardize on Chrome for all screenshots (documented in README)
2. Use explicit `grid-template-rows: 36px repeat(4, 1fr)` to minimize browser interpretation differences
3. Test in Chrome DevTools device mode at exactly 1920×1080 before release

### Risk 3: data.json Schema Drift

| Attribute | Detail |
|-----------|--------|
| **Severity** | Low |
| **Probability** | Medium (over time) |
| **Impact** | Existing `data.json` files break after code changes to the data model |

**Mitigations:**
1. `schemaVersion: 1` field in JSON enables future migration logic
2. All record properties have safe defaults (`""`, `[]`, `0`) — missing fields never crash the app
3. `PropertyNameCaseInsensitive = true` in JSON options — tolerates minor casing inconsistencies

### Risk 4: Over-Engineering / Scope Creep

| Attribute | Detail |
|-----------|--------|
| **Severity** | High |
| **Probability** | High |
| **Impact** | Project takes 2-3x longer than estimated; introduces unnecessary complexity |

**Mitigations:**
1. **Hard constraint:** If it doesn't improve the PowerPoint screenshot, don't build it
2. ≤15 file budget enforces simplicity — every new file must justify its existence
3. Zero external NuGet packages — prevents dependency rabbit holes
4. No database, no auth, no API controllers — these are out-of-scope guardrails, not future considerations

### Risk 5: Blazor Reconnect Dialog Appearing in Screenshots

| Attribute | Detail |
|-----------|--------|
| **Severity** | Medium |
| **Probability** | Low |
| **Impact** | Screenshot includes a "Attempting to reconnect..." overlay, ruining the image |

**Mitigations:**
1. CSS rule: `#components-reconnect-modal { display: none !important; }` in `app.css`
2. Screenshots are taken immediately after page load on localhost — SignalR connection is stable
3. If persistent issues, add `autostart="false"` to the Blazor script and manually start after render (stretch mitigation)

### Risk 6: Heatmap Cell Text Overflow

| Attribute | Detail |
|-----------|--------|
| **Severity** | Low |
| **Probability** | Medium |
| **Impact** | Work item text is too long for a cell, causing overflow or layout distortion |

**Mitigations:**
1. `.hm-cell { overflow: hidden }` — matches the design reference
2. Data author is responsible for keeping item text concise (12-40 characters)
3. CSS `line-height: 1.35` and `font-size: 12px` allow ~8-10 items per cell at the design's row height

### Accepted Trade-offs

| Trade-off | Justification |
|-----------|--------------|
| **Blazor Server vs. static HTML** | Blazor adds runtime overhead (~60MB RSS) and SignalR circuit complexity. Justified by: data binding from JSON (no manual HTML editing), component reuse (HeatmapCell × N), and a path to future interactivity. |
| **No hot-reload of data.json** | Requires app restart to see data changes. Justified by: simplicity (no `FileSystemWatcher` complexity), and the update workflow is fast (`Ctrl+C` → `dotnet run` → refresh). |
| **No responsive design** | Fixed 1920×1080 viewport. Justified by: the sole output is a PowerPoint screenshot at this exact resolution. Responsive breakpoints would add complexity with zero user benefit. |
| **Dates as strings in JSON** | Could use typed `DateOnly` with custom JSON converter. String format is simpler for the PM editing JSON manually, and parsing happens once at startup. |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to specific Blazor components with CSS layout strategies, data bindings, and interactions.

### Component Map

| Visual Section | Component | Design CSS Classes | Layout Strategy | Data Bindings |
|---------------|-----------|-------------------|----------------|---------------|
| **Header bar** (top strip) | `Header.razor` | `.hdr`, `.sub` | Flexbox row, `justify-content: space-between`, `align-items: center` | `Data.ProjectName`, `Data.BacklogUrl`, `Data.Organization`, `Data.Workstream`, `Data.Period` |
| **Legend** (right side of header) | `Header.razor` (inline) | Inline styles matching design | Flexbox row, `gap: 22px` | None (hardcoded legend items: PoC, Production, Checkpoint, Now) |
| **Timeline sidebar** (track labels) | `Timeline.razor` (left div) | Inline styles (230px wide sidebar) | Flexbox column, `justify-content: space-around` | `Data.Tracks[].Id`, `Data.Tracks[].Label`, `Data.Tracks[].Color` |
| **Timeline SVG** (gridlines, tracks, markers) | `Timeline.razor` (SVG element) | `.tl-area`, `.tl-svg-box` | Inline SVG, 1560×185px, `overflow: visible` | `Data.Tracks[]`, `Data.Tracks[].Events[]`, `Data.TimelineMonths[]`, `Data.NowDate`, `Data.TimelineStartDate`, `Data.TimelineEndDate` |
| **Heatmap title** | `HeatmapGrid.razor` (div) | `.hm-title` | Block element, `flex-shrink: 0` | None (static text with category labels) |
| **Heatmap header row** (corner + months) | `HeatmapGrid.razor` (grid cells) | `.hm-corner`, `.hm-col-hdr`, `.current-month-hdr` | CSS Grid first row (`36px` height) | `Data.Months[]`, `Data.CurrentMonthIndex` |
| **Heatmap row headers** (Shipped, In Progress, etc.) | `HeatmapGrid.razor` (grid cells) | `.hm-row-hdr`, `.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr` | CSS Grid first column (160px) | `Data.Categories[].Name`, `Data.Categories[].Emoji`, `Data.Categories[].CssClass` |
| **Heatmap data cells** | `HeatmapCell.razor` (×N instances) | `.hm-cell`, `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`, `.current`, `.it` | CSS Grid cells, `overflow: hidden` | `Items[]` (list of strings), `CssClass`, `IsCurrentMonth` |

### Detailed Component Specifications

#### `Header.razor`

```
┌─────────────────────────────────────────────────────────────────────────┐
│ [H1: ProjectName] [Link: 🔗 ADO Backlog]        [Legend: ◆ ◆ ● │ ]   │
│ [Sub: Organization · Workstream · Period]                               │
└─────────────────────────────────────────────────────────────────────────┘
```

- **CSS layout:** `.hdr` — `display: flex; align-items: center; justify-content: space-between; padding: 12px 44px 10px; border-bottom: 1px solid #E0E0E0;`
- **Conditional rendering:** `@if (!string.IsNullOrEmpty(Data.BacklogUrl))` wraps the `<a>` tag
- **Legend shapes:** CSS-only diamonds (`transform: rotate(45deg)` on 12×12px `<span>`), circle (`border-radius: 50%`), vertical bar (2×14px `<span>`)
- **Interactions:** Clicking ADO Backlog link navigates to external URL (standard `<a href>`, opens in same tab)

#### `Timeline.razor`

```
┌──────────┬──────────────────────────────────────────────────────────────┐
│ M1       │  ──●──────────────◆──────────────◆────────────────          │
│ Label    │  Jan 12        Mar 26 PoC    Apr Prod                       │
│          │                    ┊                                         │
│ M2       │  ──●────●──●●●●◆──┊──────────────────◆────────             │
│ Label    │                    ┊ NOW                                     │
│          │                    ┊                                         │
│ M3       │  ────────●────◆───┊────◆─────────────────────              │
│ Label    │                    ┊                                         │
└──────────┴──────────────────────────────────────────────────────────────┘
```

- **CSS layout:** `.tl-area` — `display: flex; height: 196px; background: #FAFAFA; border-bottom: 2px solid #E8E8E8;`
- **SVG rendering:** All elements emitted via Razor `@foreach` loops with computed coordinates
- **Date-to-pixel:** `DateToX(DateOnly date)` method in `@code` block, using linear interpolation across the full SVG width
- **Track Y positions:** Computed dynamically based on track count: `GetTrackY(index, total)` distributes tracks evenly in the 185px SVG height
- **Interactions:** SVG `<title>` elements on milestone markers provide native browser tooltips on hover (e.g., `<title>Mar 26 - PoC Complete</title>`)

#### `HeatmapGrid.razor`

```
┌──────────┬──────────┬──────────┬──────────┬──────────┐
│  STATUS  │   Jan    │   Feb    │   Mar    │ Apr ▸Now │  ← header row (36px)
├──────────┼──────────┼──────────┼──────────┼──────────┤
│ ✅ SHIPPED│ (cell)   │ (cell)   │ (cell)   │ (cell)   │  ← 1fr
├──────────┼──────────┼──────────┼──────────┼──────────┤
│ 🔄 IN PRO│ (cell)   │ (cell)   │ (cell)   │ (cell)   │  ← 1fr
├──────────┼──────────┼──────────┼──────────┼──────────┤
│ 📋 CARRY │ (cell)   │ (cell)   │ (cell)   │ (cell)   │  ← 1fr
├──────────┼──────────┼──────────┼──────────┼──────────┤
│ 🚫 BLOCK │ (cell)   │ (cell)   │ (cell)   │ (cell)   │  ← 1fr
└──────────┴──────────┴──────────┴──────────┴──────────┘
```

- **CSS layout:** `.hm-grid` — `display: grid; grid-template-columns: 160px repeat(@Data.Months.Count, 1fr); grid-template-rows: 36px repeat(4, 1fr); flex: 1;`
- **Dynamic columns:** `style="grid-template-columns: 160px repeat(@Data.Months.Count, 1fr)"` set inline because column count varies per data
- **Current month:** Month header at `Data.CurrentMonthIndex` gets `.current-month-hdr` class; data cells at that column index get `.current` class
- **Interactions:** None — pure read-only display

#### `HeatmapCell.razor`

- **CSS layout:** `.hm-cell` — `padding: 8px 12px; overflow: hidden;`
- **Bullet rendering:** Each item wrapped in `<div class="it">` — the `::before` pseudo-element (6×6px colored circle) is defined per-category in CSS
- **Empty state:** If `Items.Count == 0`, renders `<span style="color:#999">—</span>`
- **Category styling:** Dynamic CSS class composition: `@($"{CssClass}-cell{(IsCurrentMonth ? " current" : "")}")`
- **Interactions:** None

### CSS Custom Properties (Complete Set)

```css
:root {
    /* Category colors */
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-shipped-hdr-bg: #E8F5E9;
    --color-shipped-hdr-text: #1B7A28;

    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-progress-hdr-bg: #E3F2FD;
    --color-progress-hdr-text: #1565C0;

    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-carryover-hdr-bg: #FFF8E1;
    --color-carryover-hdr-text: #B45309;

    --color-blocker: #EA4335;
    --color-blocker-bg: #FFF5F5;
    --color-blocker-bg-current: #FFE4E4;
    --color-blocker-hdr-bg: #FEF2F2;
    --color-blocker-hdr-text: #991B1B;

    /* Milestone markers */
    --color-now-line: #EA4335;
    --color-poc-milestone: #F4B400;
    --color-prod-milestone: #34A853;
    --color-checkpoint: #999;

    /* UI chrome */
    --color-link: #0078D4;
    --color-border: #E0E0E0;
    --color-border-heavy: #CCC;
    --color-text-primary: #111;
    --color-text-secondary: #888;
    --color-text-body: #333;
    --color-bg-header-cell: #F5F5F5;
    --color-current-month-hdr-bg: #FFF0D0;
    --color-current-month-hdr-text: #C07700;
    --color-timeline-bg: #FAFAFA;
}
```

### Page-Level Layout (Body)

```css
body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
    background: #FFFFFF;
    font-family: 'Segoe UI', Arial, sans-serif;
    color: var(--color-text-primary);
    display: flex;
    flex-direction: column;
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}
```

The three sections stack vertically:
1. **Header** — `flex-shrink: 0` (~50px natural height)
2. **Timeline** — `flex-shrink: 0; height: 196px`
3. **Heatmap** — `flex: 1; min-height: 0` (fills remaining ~834px)