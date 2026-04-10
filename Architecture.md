# Architecture

## Overview & Goals

This document defines the system architecture for the **Executive Reporting Dashboard**, a single-page Blazor Server application that renders project status data from a local `data.json` file into a pixel-precise 1920×1080 visualization optimized for PowerPoint screenshot capture.

### Primary Goals

1. **Pixel-fidelity rendering** — Reproduce the `OriginalDesignConcept.html` layout exactly at 1920×1080 using Blazor Server components, CSS Grid/Flexbox, and inline SVG.
2. **Data-driven content** — All dashboard text, milestones, and work items are sourced from a single `data.json` file with no code changes required for updates.
3. **Zero-dependency local execution** — Run with `dotnet run` on any Windows machine with .NET 8 SDK. No cloud, no database, no third-party NuGet packages.
4. **Component isolation** — Each visual section (Header, Timeline, Heatmap) is an independent Blazor component with well-defined parameter contracts.

### Architecture Style

The application follows a **layered component architecture** with three tiers:

```
┌─────────────────────────────────────────────────┐
│  Presentation Layer (Blazor Components + CSS)   │
├─────────────────────────────────────────────────┤
│  Service Layer (DashboardDataService)            │
├─────────────────────────────────────────────────┤
│  Data Layer (data.json file on disk)             │
└─────────────────────────────────────────────────┘
```

Data flows unidirectionally: `data.json` → `DashboardDataService` → `Dashboard.razor` → child components via `[Parameter]` cascading. No bidirectional binding, no state mutation, no user input handling.

---

## System Components

### 1. `Program.cs` — Application Entry Point

**Responsibilities:**
- Configure Kestrel to bind exclusively to `http://localhost:5000`
- Register `DashboardDataService` as a singleton in the DI container
- Add Blazor Server services and map the Blazor hub
- Configure static file serving for `wwwroot/`

**Interfaces:** None (entry point only)

**Dependencies:** `DashboardDataService`, ASP.NET Core built-in middleware

**Key Implementation Details:**
```csharp
builder.WebHost.UseUrls("http://localhost:5000");
builder.Services.AddSingleton<DashboardDataService>();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
```

---

### 2. `DashboardDataService` — Data Access Service

**Responsibilities:**
- Read and deserialize `wwwroot/data.json` at application startup
- Validate the deserialized model for structural correctness (non-null required fields, non-empty arrays)
- Expose the parsed `DashboardData` model to consuming components
- Log clear, actionable error messages on parse failure
- Provide an `IsError` flag and `ErrorMessage` string for UI error state rendering

**Interfaces:**
```csharp
public class DashboardDataService
{
    public DashboardData? Data { get; }
    public bool IsError { get; }
    public string? ErrorMessage { get; }
    public Task LoadAsync(string filePath);
}
```

**Dependencies:** `System.Text.Json`, `System.IO`, `ILogger<DashboardDataService>`, `IWebHostEnvironment`

**Data:** Reads `wwwroot/data.json` (configurable path via `IWebHostEnvironment.WebRootPath`)

**Error Handling Strategy:**
| Failure Mode | Service Behavior | UI Behavior |
|---|---|---|
| File not found | `IsError = true`, `ErrorMessage = "data.json not found at {path}"` | Renders centered error panel |
| Invalid JSON syntax | `IsError = true`, `ErrorMessage = "Failed to parse data.json: {details}"` | Renders centered error panel |
| Missing required fields | `IsError = true`, `ErrorMessage = "data.json validation: {field} is required"` | Renders centered error panel |
| Valid JSON | `Data` populated, `IsError = false` | Renders dashboard |

**Lifecycle:** Registered as singleton. `LoadAsync` is called once during `Program.cs` startup (before `app.Run()`), ensuring data is available before any HTTP request is served.

---

### 3. `DashboardLayout.razor` — Minimal Layout

**Responsibilities:**
- Provide a bare HTML shell with no navigation sidebar, no footer, no default Blazor chrome
- Include the `dashboard.css` stylesheet reference
- Set the `<meta name="viewport" content="width=1920">` tag
- Render `@Body` as the sole content

**Interfaces:** Blazor layout component (inherits `LayoutComponentBase`)

**Dependencies:** `dashboard.css`

**Template Structure:**
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=1920" />
    <link rel="stylesheet" href="css/dashboard.css" />
    <title>Executive Dashboard</title>
</head>
<body>
    @Body
    <script src="_framework/blazor.server.js"></script>
</body>
</html>
```

---

### 4. `Dashboard.razor` — Main Page Component

**Route:** `/`

**Responsibilities:**
- Inject `DashboardDataService` and read its state
- If `IsError`, render the error panel component
- If data is valid, render `Header`, `Timeline`, and `Heatmap` components, passing data via `[Parameter]`
- Act as the sole orchestrator — no business logic, only data delegation

**Interfaces:**
```csharp
@inject DashboardDataService DataService

// Passes to children:
<Header Data="@DataService.Data" />
<Timeline TimelineData="@DataService.Data.Timeline" />
<Heatmap HeatmapData="@DataService.Data.Heatmap"
         Months="@DataService.Data.Months"
         CurrentMonth="@DataService.Data.CurrentMonth" />
```

**Dependencies:** `DashboardDataService`, `Header`, `Timeline`, `Heatmap`

---

### 5. `Header.razor` — Header Component

**Responsibilities:**
- Render project title (24px bold) with inline ADO Backlog link
- Render subtitle (team, workstream, month)
- Render the legend (PoC diamond, Production diamond, Checkpoint circle, NOW line)

**Parameters:**
```csharp
[Parameter] public DashboardData Data { get; set; }
```

**CSS Classes:** `.hdr`, `.sub`

**Visual Mapping:** Maps directly to the `.hdr` div in `OriginalDesignConcept.html`. The legend is rendered inline using `<span>` elements with CSS transforms for diamond shapes.

---

### 6. `Timeline.razor` — SVG Timeline Component

**Responsibilities:**
- Render the 230px track label sidebar (milestone names, colors)
- Generate the inline SVG containing:
  - Vertical month grid lines at calculated intervals
  - Month labels (Jan–Jun or as configured)
  - Horizontal track lines (one per milestone track, colored)
  - Milestone markers: checkpoint circles, PoC diamonds (`#F4B400`), production diamonds (`#34A853`)
  - "NOW" dashed vertical line at the date-calculated X position
  - Milestone date labels with `<title>` elements for native browser tooltips
- Calculate X positions via linear interpolation: `x = ((date - startDate) / (endDate - startDate)) * svgWidth`
- Calculate Y positions dynamically: `y = 42 + (trackIndex * verticalSpacing)` where `verticalSpacing = 185 / (trackCount + 1)` adjusted to ~56px

**Parameters:**
```csharp
[Parameter] public TimelineData TimelineData { get; set; }
```

**CSS Classes:** `.tl-area`, `.tl-svg-box`

**SVG Rendering Approach:** All SVG elements are rendered as native Blazor markup (`<svg>`, `<line>`, `<circle>`, `<polygon>`, `<text>`, `<defs>`, `<filter>`). No `MarkupString` or raw HTML injection. This ensures type safety and avoids XSS vectors (even though data is local).

**Coordinate Calculation Methods (private):**
```csharp
private double DateToX(DateTime date)
{
    double totalDays = (EndDate - StartDate).TotalDays;
    double elapsed = (date - StartDate).TotalDays;
    return (elapsed / totalDays) * SvgWidth;
}

private double TrackToY(int trackIndex, int totalTracks)
{
    double spacing = 185.0 / (totalTracks + 1);
    return spacing * (trackIndex + 1);
}

private string DiamondPoints(double cx, double cy, double size = 11)
{
    return $"{cx},{cy - size} {cx + size},{cy} {cx},{cy + size} {cx - size},{cy}";
}
```

---

### 7. `Heatmap.razor` — Heatmap Container Component

**Responsibilities:**
- Render the heatmap section title ("MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS")
- Generate the CSS Grid container with dynamic column count: `grid-template-columns: 160px repeat({monthCount}, 1fr)`
- Render corner cell ("STATUS"), column headers (month names with current-month highlighting), and four `HeatmapRow` components

**Parameters:**
```csharp
[Parameter] public HeatmapData HeatmapData { get; set; }
[Parameter] public List<string> Months { get; set; }
[Parameter] public string CurrentMonth { get; set; }
```

**CSS Classes:** `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.apr-hdr`

**Dynamic Grid Columns:** The `grid-template-columns` value is computed in the component based on `Months.Count`:
```csharp
private string GridColumns => $"160px repeat({Months.Count}, 1fr)";
private string GridRows => $"36px repeat(4, 1fr)";
```

---

### 8. `HeatmapRow.razor` — Status Row Component

**Responsibilities:**
- Render the row header cell with category-specific styling (color, background, icon)
- Render one `HeatmapCell` per month for this status category
- Apply row-specific CSS class prefixes (`ship-`, `prog-`, `carry-`, `block-`)

**Parameters:**
```csharp
[Parameter] public string CategoryKey { get; set; }       // "shipped", "inProgress", "carryover", "blockers"
[Parameter] public string CategoryLabel { get; set; }      // "✓ SHIPPED", "► IN PROGRESS", etc.
[Parameter] public string CssPrefix { get; set; }          // "ship", "prog", "carry", "block"
[Parameter] public Dictionary<string, List<string>> Items { get; set; }
[Parameter] public List<string> Months { get; set; }
[Parameter] public string CurrentMonth { get; set; }
```

**CSS Classes:** `.hm-row-hdr`, `.{prefix}-hdr`, `.{prefix}-cell`

---

### 9. `HeatmapCell.razor` — Individual Cell Component

**Responsibilities:**
- Render work items as bulleted list entries (6px colored dot + 12px text)
- Apply current-month highlight CSS class when applicable
- Render a gray dash (`—` in `#AAA`) when the items list is empty

**Parameters:**
```csharp
[Parameter] public List<string> Items { get; set; }
[Parameter] public string CssPrefix { get; set; }          // "ship", "prog", "carry", "block"
[Parameter] public bool IsCurrentMonth { get; set; }
```

**CSS Classes:** `.hm-cell`, `.{prefix}-cell`, `.apr` (current month modifier), `.it`

---

### 10. `ErrorPanel.razor` — Error State Component

**Responsibilities:**
- Display a centered, styled error message when `data.json` cannot be loaded
- Show the specific error reason (file not found, parse error, validation error)
- Provide guidance text: "Check data.json for errors and restart the application."

**Parameters:**
```csharp
[Parameter] public string ErrorMessage { get; set; }
```

---

### 11. `dashboard.css` — Stylesheet

**Responsibilities:**
- Contains ALL visual styles for the dashboard, ported directly from `OriginalDesignConcept.html`
- Maintains identical CSS class names (`.hdr`, `.sub`, `.tl-area`, `.hm-wrap`, `.hm-grid`, etc.)
- Defines the fixed 1920×1080 body constraint
- Contains color theme definitions for all four heatmap status rows
- Includes the SVG drop shadow filter reference styles

**Source:** Extracted verbatim from the `<style>` block in `OriginalDesignConcept.html` with the following additions:
- Error panel centering styles
- Any Blazor-specific overrides (e.g., removing default Blazor isolation styles)

---

### 12. `data.json` — Data Configuration File

**Responsibilities:**
- Single source of truth for all dashboard content
- Editable by project leads without developer intervention
- Located at `wwwroot/data.json`

**Schema:** See [Data Model](#data-model) section below.

---

## Component Interactions

### Data Flow Diagram

```
┌──────────────┐     Startup       ┌──────────────────────┐
│  data.json   │ ──────────────►   │  DashboardDataService │
│  (wwwroot/)  │   File.ReadAll    │  (Singleton)          │
└──────────────┘   + Deserialize   └──────────┬───────────┘
                                               │ @inject
                                               ▼
                                   ┌──────────────────────┐
                                   │   Dashboard.razor     │
                                   │   (Route: /)          │
                                   └──┬──────┬──────┬─────┘
                                      │      │      │
                          [Parameter] │      │      │ [Parameter]
                                      ▼      ▼      ▼
                              ┌───────┐ ┌────────┐ ┌─────────┐
                              │Header │ │Timeline│ │ Heatmap │
                              └───────┘ └────────┘ └──┬──┬───┘
                                                      │  │
                                          [Parameter] │  │ [Parameter]
                                                      ▼  ▼
                                              ┌────────────────┐
                                              │  HeatmapRow ×4 │
                                              └───────┬────────┘
                                                      │ [Parameter]
                                                      ▼
                                              ┌────────────────┐
                                              │ HeatmapCell ×N │
                                              └────────────────┘
```

### Communication Patterns

| Source | Target | Mechanism | Data Passed |
|--------|--------|-----------|-------------|
| `Program.cs` | `DashboardDataService` | `LoadAsync()` call at startup | File path string |
| `Dashboard.razor` | `DashboardDataService` | `@inject` (DI) | None (reads properties) |
| `Dashboard.razor` | `Header` | `[Parameter]` | `DashboardData` |
| `Dashboard.razor` | `Timeline` | `[Parameter]` | `TimelineData` |
| `Dashboard.razor` | `Heatmap` | `[Parameter]` | `HeatmapData`, `Months`, `CurrentMonth` |
| `Heatmap` | `HeatmapRow` | `[Parameter]` | Category items, months, CSS prefix |
| `HeatmapRow` | `HeatmapCell` | `[Parameter]` | Item list, CSS prefix, current month flag |

### Rendering Lifecycle

1. `Program.cs` calls `DashboardDataService.LoadAsync()` **before** `app.Run()` — data is ready before any request.
2. User navigates to `http://localhost:5000`.
3. Blazor Server renders `Dashboard.razor` via `OnInitializedAsync`.
4. `Dashboard.razor` reads `DataService.Data` (already populated) — no async wait needed at render time.
5. Child components receive data via `[Parameter]` and render synchronously.
6. Full page renders in a single pass. No subsequent `StateHasChanged` calls needed.

---

## Data Model

### Entity Relationship

```
DashboardData (root)
├── title: string
├── subtitle: string
├── backlogLink: string
├── currentMonth: string
├── months: string[]
├── timeline: TimelineData
│   ├── startDate: string (ISO date)
│   ├── endDate: string (ISO date)
│   ├── nowDate: string (ISO date)
│   └── tracks: TimelineTrack[]
│       ├── name: string
│       ├── label: string
│       ├── color: string (hex)
│       └── milestones: Milestone[]
│           ├── date: string (ISO date)
│           ├── type: string ("checkpoint" | "poc" | "production")
│           └── label: string
└── heatmap: HeatmapData
    ├── shipped: Dictionary<string, string[]>
    ├── inProgress: Dictionary<string, string[]>
    ├── carryover: Dictionary<string, string[]>
    └── blockers: Dictionary<string, string[]>
```

### C# Model Definitions

#### `Models/DashboardData.cs`

```csharp
using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class DashboardData
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;

    [JsonPropertyName("backlogLink")]
    public string BacklogLink { get; set; } = string.Empty;

    [JsonPropertyName("currentMonth")]
    public string CurrentMonth { get; set; } = string.Empty;

    [JsonPropertyName("months")]
    public List<string> Months { get; set; } = new();

    [JsonPropertyName("timeline")]
    public TimelineData Timeline { get; set; } = new();

    [JsonPropertyName("heatmap")]
    public HeatmapData Heatmap { get; set; } = new();
}
```

#### `Models/TimelineData.cs`

```csharp
public class TimelineData
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;

    [JsonPropertyName("nowDate")]
    public string NowDate { get; set; } = string.Empty;

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; set; } = new();
}

public class TimelineTrack
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#999";

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();
}

public class Milestone
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "checkpoint";

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}
```

#### `Models/HeatmapData.cs`

```csharp
public class HeatmapData
{
    [JsonPropertyName("shipped")]
    public Dictionary<string, List<string>> Shipped { get; set; } = new();

    [JsonPropertyName("inProgress")]
    public Dictionary<string, List<string>> InProgress { get; set; } = new();

    [JsonPropertyName("carryover")]
    public Dictionary<string, List<string>> Carryover { get; set; } = new();

    [JsonPropertyName("blockers")]
    public Dictionary<string, List<string>> Blockers { get; set; } = new();
}
```

### JSON Schema (`data.json`)

```json
{
  "title": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
  "backlogLink": "https://dev.azure.com/org/project/_backlogs",
  "currentMonth": "Apr",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "nowDate": "2026-04-10",
    "tracks": [
      {
        "name": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "Apr Prod (TBD)" }
        ]
      }
    ]
  },
  "heatmap": {
    "shipped": {
      "jan": ["Item A", "Item B"],
      "feb": ["Item C"],
      "mar": ["Item D", "Item E"],
      "apr": ["Item F"]
    },
    "inProgress": {
      "jan": [],
      "feb": [],
      "mar": ["Item G"],
      "apr": ["Item H", "Item I"]
    },
    "carryover": {
      "jan": [],
      "feb": [],
      "mar": [],
      "apr": ["Item J"]
    },
    "blockers": {
      "jan": [],
      "feb": [],
      "mar": [],
      "apr": ["Item K"]
    }
  }
}
```

### Data Validation Rules

| Field | Required | Constraint |
|-------|----------|------------|
| `title` | Yes | Non-empty string |
| `subtitle` | Yes | Non-empty string |
| `backlogLink` | Yes | Valid URL string |
| `currentMonth` | Yes | Must exist in `months` array |
| `months` | Yes | Non-empty array, 1–12 entries |
| `timeline.startDate` | Yes | Valid ISO 8601 date |
| `timeline.endDate` | Yes | Valid ISO 8601 date, after `startDate` |
| `timeline.nowDate` | Yes | Valid ISO 8601 date |
| `timeline.tracks` | Yes | Non-empty array, 1–10 entries |
| `timeline.tracks[].milestones[].type` | Yes | One of: `"checkpoint"`, `"poc"`, `"production"` |
| `heatmap` categories | Yes | Keys must be lowercase month abbreviations matching `months` array |

---

## API Contracts

This application has no REST API, no Web API controllers, and no external-facing endpoints. It is a single-page Blazor Server application with one route.

### Routes

| Route | Component | Method | Description |
|-------|-----------|--------|-------------|
| `/` | `Dashboard.razor` | GET (Blazor page) | Renders the full dashboard |
| `/_framework/blazor.server.js` | Built-in | GET | Blazor Server SignalR bootstrap script |
| `/css/dashboard.css` | Static file | GET | Dashboard stylesheet |
| `/data.json` | Static file | GET | Data file (also read server-side at startup) |

### Error Responses

| Condition | HTTP Status | Rendered Output |
|-----------|-------------|-----------------|
| Normal operation | 200 | Full dashboard HTML |
| `data.json` missing/invalid | 200 | Error panel HTML with message |
| Route not found | 404 | Default Blazor 404 (not customized) |

### Internal Service Contract

The `DashboardDataService` acts as the internal "API" between data and presentation:

```csharp
// Called once at startup
public async Task LoadAsync(string? dataFilePath = null)

// Read-only properties for components
public DashboardData? Data { get; }
public bool IsError { get; }
public string? ErrorMessage { get; }
```

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|--------------|
| **Runtime** | .NET 8.0 SDK (LTS) |
| **OS** | Windows 10/11 (required for Segoe UI font) |
| **Browser** | Microsoft Edge or Google Chrome (latest stable) |
| **Network** | Localhost only — no external network required |
| **Disk** | < 50 MB for published application |
| **Memory** | < 100 MB runtime footprint |

### Hosting Configuration

**Kestrel (built-in web server):**
- Bind address: `http://localhost:5000`
- No HTTPS, no certificates
- No reverse proxy (no IIS, no nginx)
- No Docker container

**`Program.cs` Configuration:**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

// Load data before serving requests
var dataService = app.Services.GetRequiredService<DashboardDataService>();
await dataService.LoadAsync(Path.Combine(app.Environment.WebRootPath, "data.json"));

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
```

### Project Structure on Disk

```
ReportingDashboard/
├── ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── Models/
    │   ├── DashboardData.cs
    │   ├── TimelineData.cs
    │   └── HeatmapData.cs
    ├── Services/
    │   └── DashboardDataService.cs
    ├── Components/
    │   ├── App.razor
    │   ├── _Imports.razor
    │   ├── Pages/
    │   │   └── Dashboard.razor
    │   ├── Layout/
    │   │   └── DashboardLayout.razor
    │   ├── Header.razor
    │   ├── Timeline.razor
    │   ├── Heatmap.razor
    │   ├── HeatmapRow.razor
    │   ├── HeatmapCell.razor
    │   └── ErrorPanel.razor
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css
    │   └── data.json
    └── Properties/
        └── launchSettings.json
```

### `.csproj` File

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <!-- Zero third-party NuGet packages -->
</Project>
```

### Build & Run Commands

| Action | Command |
|--------|---------|
| Build | `dotnet build` |
| Run (development) | `dotnet run` |
| Publish (self-contained) | `dotnet publish -c Release --self-contained -r win-x64` |
| Publish (framework-dependent) | `dotnet publish -c Release` |

### CI/CD

Not required for MVP. If added later:
- `dotnet build` → `dotnet test` → `dotnet publish`
- No deployment target (local-only application)

---

## Technology Stack Decisions

| Layer | Choice | Justification |
|-------|--------|---------------|
| **Framework** | Blazor Server (.NET 8 LTS) | Mandated by project requirements. Provides component model, strong typing, and server-side rendering without client-side JS framework. |
| **CSS Layout** | CSS Grid + Flexbox (native) | Exact match to `OriginalDesignConcept.html`. No abstraction layer or CSS framework needed. Grid for heatmap, Flexbox for header and timeline layout. |
| **Timeline Rendering** | Inline SVG via Blazor markup | The original design uses hand-crafted SVG. Blazor natively renders SVG elements. No charting library can replicate the custom diamond/circle/line layout without fighting it. |
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8. Zero dependencies. Fast enough for <50KB files (<1ms parse time). Strongly-typed deserialization with `[JsonPropertyName]` attributes. |
| **Web Server** | Kestrel (built-in) | Included in .NET 8 SDK. No IIS or external web server needed. Binds to localhost only. |
| **Font** | Segoe UI (system) | Native on Windows 10/11. No web font loading, no CDN dependency, no FOUT. |

### Rejected Alternatives

| Alternative | Reason for Rejection |
|-------------|---------------------|
| **MudBlazor / Radzen** | Adds 500KB+ of CSS/JS. Opinionated component styling conflicts with pixel-precise design. Overkill for a single static page. |
| **Chart.js / ApexCharts** | Timeline is not a standard chart type. These libraries cannot render custom diamond markers, track lines, and NOW indicators without extensive workarounds. |
| **Blazor WebAssembly** | Adds 5-10MB download, slower startup, requires WASM runtime. Server model is simpler for local single-user use. |
| **Entity Framework Core** | No database exists. JSON file is the data source. EF adds complexity with zero benefit. |
| **Blazor Static SSR** | Could eliminate SignalR overhead but adds routing complexity. For a single-user local app, the SignalR overhead is <1MB memory. |
| **React / Vue / Angular** | Not in the mandated technology stack. Would require npm, Node.js, and a JS build pipeline. |

---

## Security Considerations

### Threat Model

This application has an intentionally minimal threat surface:

| Vector | Risk Level | Assessment |
|--------|-----------|------------|
| **Network exposure** | None | Binds to `localhost:5000` only. Not reachable from other machines. |
| **User input** | None | No forms, no text inputs, no query parameters processed. All data from local file. |
| **Authentication** | N/A | No auth required. Single local user. |
| **Data sensitivity** | Low | Dashboard data is project status info, not PII or secrets. |
| **Supply chain** | Minimal | Zero third-party NuGet packages. Only .NET 8 SDK built-in libraries. |
| **XSS** | Minimal | Blazor Server renders via the Razor engine which HTML-encodes output by default. No raw HTML injection. SVG content is generated via typed Blazor markup, not string concatenation. |
| **CSRF** | Minimal | Blazor Server includes built-in antiforgery. No state-mutating operations exist. |

### Security Implementation

1. **Localhost binding** — `builder.WebHost.UseUrls("http://localhost:5000")` prevents external access.
2. **No secrets in code or config** — `data.json` contains only project status data. No API keys, no connection strings.
3. **No `MarkupString` usage** — All SVG and HTML is rendered through Blazor's built-in rendering pipeline, which auto-encodes text content. This prevents injection even if `data.json` contained malicious strings.
4. **File path restriction** — `DashboardDataService` reads only from `wwwroot/data.json` (within the application's web root). No user-specified file paths.
5. **Static file middleware** — Only serves files from `wwwroot/`. No directory browsing enabled.

### Data Protection

- No encryption needed (data is local, non-sensitive).
- No backup strategy needed (data is a single JSON file the user maintains).
- No audit logging needed (single local user, no mutations).

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component.

### Visual Section → Component Mapping

#### Section 1: Header Bar (`<div class="hdr">`)

**Component:** `Header.razor`

| Visual Element | HTML Structure | CSS Strategy | Data Binding | Interactions |
|---|---|---|---|---|
| Title text | `<h1>` | `font-size: 24px; font-weight: 700` | `@Data.Title` | None |
| ADO Backlog link | `<a>` inside `<h1>` | `color: #0078D4; text-decoration: none` | `@Data.BacklogLink` | Opens new tab (`target="_blank"`) |
| Subtitle | `<div class="sub">` | `font-size: 12px; color: #888; margin-top: 2px` | `@Data.Subtitle` | None |
| Legend container | `<div>` (right side) | `display: flex; gap: 22px; align-items: center` | Static (hardcoded symbols) | None |
| PoC diamond | `<span>` | `width: 12px; height: 12px; background: #F4B400; transform: rotate(45deg)` | Static | None |
| Production diamond | `<span>` | `width: 12px; height: 12px; background: #34A853; transform: rotate(45deg)` | Static | None |
| Checkpoint circle | `<span>` | `width: 8px; height: 8px; border-radius: 50%; background: #999` | Static | None |
| NOW line symbol | `<span>` | `width: 2px; height: 14px; background: #EA4335` | Static | None |

**Layout:** Flexbox row (`display: flex; justify-content: space-between; align-items: center`). Padding `12px 44px 10px`. Bottom border `1px solid #E0E0E0`.

---

#### Section 2: Timeline Area (`<div class="tl-area">`)

**Component:** `Timeline.razor`

| Visual Element | HTML Structure | CSS Strategy | Data Binding | Interactions |
|---|---|---|---|---|
| Track labels sidebar | `<div>` (230px fixed) | `width: 230px; flex-shrink: 0; display: flex; flex-direction: column; justify-content: space-around` | `@foreach track in TimelineData.Tracks` | None |
| Track ID (M1, M2...) | `<div>` | `font-size: 12px; font-weight: 600; color: {track.Color}` | `@track.Name` | None |
| Track label | `<span>` | `font-weight: 400; color: #444` | `@track.Label` | None |
| SVG container | `<div class="tl-svg-box">` | `flex: 1; padding-left: 12px; padding-top: 6px` | — | — |
| Month grid lines | `<line>` in SVG | Inline SVG attributes: `stroke="#bbb" stroke-opacity="0.4"` | Calculated from `startDate`/`endDate` with 260px intervals | None |
| Month labels | `<text>` in SVG | `fill="#666" font-size="11" font-weight="600"` | Generated from date range | None |
| Track horizontal lines | `<line>` in SVG | `stroke="{track.Color}" stroke-width="3"` | Y position calculated per track index | None |
| Checkpoint circles | `<circle>` in SVG | `fill="white" stroke="{track.Color}" stroke-width="2.5" r="7"` | X from `DateToX(milestone.Date)` | Tooltip via `<title>` |
| Small checkpoints | `<circle>` in SVG | `fill="#999" r="4"` | X from `DateToX(milestone.Date)` | Tooltip via `<title>` |
| PoC diamonds | `<polygon>` in SVG | `fill="#F4B400" filter="url(#sh)"` | Points from `DiamondPoints(x, y)` | Tooltip via `<title>` |
| Production diamonds | `<polygon>` in SVG | `fill="#34A853" filter="url(#sh)"` | Points from `DiamondPoints(x, y)` | Tooltip via `<title>` |
| NOW line | `<line>` in SVG | `stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"` | X from `DateToX(nowDate)` | None |
| NOW label | `<text>` in SVG | `fill="#EA4335" font-size="10" font-weight="700"` | Static text "NOW" | None |
| Milestone labels | `<text>` in SVG | `text-anchor="middle" fill="#666" font-size="10"` | `@milestone.Label` | None |
| Drop shadow filter | `<defs><filter>` in SVG | `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>` | Static | None |

**Layout:** Flexbox row (`display: flex; align-items: stretch`). Height `196px`. Background `#FAFAFA`. Bottom border `2px solid #E8E8E8`.

**SVG Dimensions:** `width` = available width after sidebar (~1560px for default 1920px viewport minus 44px padding × 2 minus 230px sidebar minus 12px padding). `height` = dynamic based on track count (`max(185, tracks.Count * 56)`).

---

#### Section 3: Heatmap (`<div class="hm-wrap">`)

**Component:** `Heatmap.razor` (container) → `HeatmapRow.razor` (×4) → `HeatmapCell.razor` (×N per row)

| Visual Element | Component | CSS Strategy | Data Binding | Interactions |
|---|---|---|---|---|
| Section title | `Heatmap.razor` | `.hm-title`: `font-size: 14px; font-weight: 700; color: #888; text-transform: uppercase` | Static text | None |
| Grid container | `Heatmap.razor` | `.hm-grid`: `display: grid; grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr)` | `N` = `Months.Count` | None |
| Corner cell ("STATUS") | `Heatmap.razor` | `.hm-corner`: `background: #F5F5F5; font-size: 11px; font-weight: 700; color: #999; text-transform: uppercase` | Static | None |
| Month column headers | `Heatmap.razor` | `.hm-col-hdr`: `font-size: 16px; font-weight: 700; background: #F5F5F5`. Current month: `.apr-hdr`: `background: #FFF0D0; color: #C07700` | `@foreach month in Months` | None |
| Shipped row header | `HeatmapRow.razor` | `.ship-hdr`: `color: #1B7A28; background: #E8F5E9` | Static "✓ SHIPPED" | None |
| In Progress row header | `HeatmapRow.razor` | `.prog-hdr`: `color: #1565C0; background: #E3F2FD` | Static "► IN PROGRESS" | None |
| Carryover row header | `HeatmapRow.razor` | `.carry-hdr`: `color: #B45309; background: #FFF8E1` | Static "⟳ CARRYOVER" | None |
| Blockers row header | `HeatmapRow.razor` | `.block-hdr`: `color: #991B1B; background: #FEF2F2` | Static "⊘ BLOCKERS" | None |
| Data cells (items) | `HeatmapCell.razor` | `.hm-cell`: `padding: 8px 12px`. `.it`: `font-size: 12px; color: #333; padding: 2px 0 2px 12px`. `.it::before`: `6px circle` with row-specific color | `@foreach item in Items` | None |
| Current month cells | `HeatmapCell.razor` | `.{prefix}-cell.apr`: uses highlighted background per row color scheme | `IsCurrentMonth` flag | None |
| Empty cells | `HeatmapCell.razor` | Gray dash: `color: #AAA; text-align: center` | When `Items.Count == 0` | None |

**Layout:** Outer wrapper is Flexbox column (`flex: 1; min-height: 0`). Inner grid fills remaining vertical space.

**Dynamic Grid Computation:**
```csharp
// In Heatmap.razor
private string GridStyle => 
    $"grid-template-columns: 160px repeat({Months.Count}, 1fr); " +
    $"grid-template-rows: 36px repeat(4, 1fr);";
```

**Current Month Detection in Cells:**
```csharp
// In HeatmapRow.razor, when rendering cells
bool isCurrentMonth = month.Equals(CurrentMonth, StringComparison.OrdinalIgnoreCase);
string cellClass = $"{CssPrefix}-cell{(isCurrentMonth ? " apr" : "")}";
```

---

#### Error State

**Component:** `ErrorPanel.razor`

| Visual Element | CSS Strategy | Data Binding |
|---|---|---|
| Centered container | `display: flex; align-items: center; justify-content: center; width: 1920px; height: 1080px` | — |
| Error icon | `font-size: 48px; color: #EA4335` | Static "⚠" |
| Error title | `font-size: 20px; font-weight: 700; color: #333` | Static "Dashboard data could not be loaded" |
| Error details | `font-size: 14px; color: #666; font-family: monospace` | `@ErrorMessage` |
| Help text | `font-size: 12px; color: #888` | Static "Check data.json for errors and restart the application." |

---

## Scaling Strategy

### Current Scale (MVP)

- **Users:** 1 (single local user)
- **Data size:** <50 KB JSON file
- **Concurrent connections:** 1
- **Tracks:** 1–10 timeline tracks
- **Heatmap items:** Up to 200 total across all cells
- **Months:** 4–6 columns

### Vertical Scaling (Within Current Architecture)

The architecture handles increased data volume without code changes:

| Dimension | Current | Max Supported | Limiting Factor |
|-----------|---------|---------------|-----------------|
| Timeline tracks | 3 | 10 | SVG height grows with tracks; at >10, timeline section exceeds 196px fixed height. Adjust to dynamic height. |
| Months in heatmap | 4 | 12 | CSS Grid columns shrink proportionally. At >8, cell text may truncate. |
| Items per cell | 3–5 | 8 | Cell height is `(1080 - 46 - 196 - 44 - 36) / 4 ≈ 189px`. At 12px line-height × 1.35 ≈ 16px per item, max ~11 items visible. Beyond 8, `overflow: hidden` clips. |
| JSON file size | ~5 KB | 50 KB | `System.Text.Json` parses 50KB in <1ms. No concern. |

### Horizontal Scaling (Future, Out of Scope)

If multi-project or multi-user support is needed:

1. **Multiple JSON files** — Load `/data/{project}.json` based on route parameter. Requires adding routing (`/project/{name}`) and updating `DashboardDataService` to accept a parameter.
2. **SQLite backend** — Replace JSON with `Microsoft.Data.Sqlite` for structured queries. Add a thin repository layer. No EF needed.
3. **Multi-instance** — Run multiple instances on different ports (`localhost:5001`, `localhost:5002`) for different projects. Zero-cost approach.

### Performance Budget

| Metric | Budget | Achieved By |
|--------|--------|-------------|
| Startup to first request | < 5 seconds | Pre-loading data before `app.Run()`. Kestrel starts in <1s. |
| First contentful paint | < 2 seconds | Single-pass server render. No async data fetching in render path. |
| JSON parse time | < 100ms | `System.Text.Json` with <50KB file. Typically <1ms. |
| SVG render (browser) | < 500ms | Max ~150 SVG elements (10 tracks × 10 milestones + grid lines). Trivial for modern browsers. |
| Memory footprint | < 100 MB | Blazor Server baseline ~50MB. Single-user, no state accumulation. |

---

## Risks & Mitigations

### Risk Register

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|------------|
| R1 | **SVG coordinate math errors** — Milestone markers render at wrong positions due to date-to-pixel calculation bugs | Medium | Medium | Unit test `DateToX()` and `DiamondPoints()` methods independently. Compare rendered output against `OriginalDesignConcept.html` coordinates for known dates. |
| R2 | **CSS class name mismatch** — Styles don't apply because Blazor component class names differ from `dashboard.css` | Low | High | Port CSS class names verbatim from `OriginalDesignConcept.html`. Do not rename classes. Use the same `.hdr`, `.tl-area`, `.hm-grid` names. |
| R3 | **Blazor default chrome leaks** — Default navigation sidebar or footer appears despite custom layout | Low | Medium | Create `DashboardLayout.razor` as a bare layout inheriting `LayoutComponentBase`. Delete `MainLayout.razor`, `NavMenu.razor`, and all default scaffold components. Set `@layout DashboardLayout` on `Dashboard.razor`. |
| R4 | **JSON schema drift** — `data.json` structure diverges from C# models over time, causing silent deserialization failures | Low | Medium | Use `[JsonPropertyName]` attributes on all properties. Initialize collections to empty (not null) so missing JSON keys produce empty collections rather than `NullReferenceException`. Add startup validation logging for missing required fields. |
| R5 | **SignalR connection overhead** — Blazor Server's persistent WebSocket connection adds unnecessary complexity for a static page | Low | Low | Accept the overhead (~1 WebSocket connection, <1MB memory). The alternative (Blazor Static SSR) adds routing complexity. For a single local user, this is negligible. |
| R6 | **Browser rendering inconsistency** — Dashboard looks different in Edge vs Chrome, especially SVG text positioning | Low | Low | Target Edge as primary browser per requirements. Test in Chrome as secondary. Use `font-family: 'Segoe UI', Arial, sans-serif` consistently. Avoid browser-specific CSS features. |
| R7 | **Heatmap cell overflow** — Too many work items in a cell cause text to overflow and break layout | Medium | Low | CSS `overflow: hidden` is already set on `.hm-cell`. Document the 8-item-per-cell practical limit. Data validation could warn when items exceed this. |
| R8 | **data.json not found on first run** — User clones the repo but `data.json` is missing or `.gitignore`d | Low | High | Include a complete sample `data.json` in the repo under `wwwroot/`. Ensure it is NOT in `.gitignore`. The `ErrorPanel` component provides a clear message if the file is missing. |
| R9 | **Port 5000 already in use** — Another application occupies port 5000 on the developer's machine | Low | Low | Kestrel will log a clear "address already in use" error. Document that users can change the port in `Properties/launchSettings.json` or via `--urls` command-line argument. |
| R10 | **Segoe UI unavailable** — Running on a non-Windows machine where Segoe UI is not installed | Low | Medium | CSS fallback chain includes `Arial, sans-serif`. Document that Windows 10/11 is the target platform. Arial provides acceptable visual fidelity as a fallback. |

### Dependency Risk Assessment

| Dependency | Risk | Notes |
|------------|------|-------|
| .NET 8 SDK | Minimal | LTS release, supported until November 2026. |
| `System.Text.Json` | None | Built into .NET 8. No external package. |
| Kestrel | None | Built into .NET 8. No external package. |
| Segoe UI font | Low | Pre-installed on all Windows 10/11 systems. |
| CSS Grid / Flexbox / SVG | None | Supported in all modern browsers since 2018+. |

**Total external NuGet dependencies: 0**

This architecture achieves zero third-party dependency risk while providing a fully functional, pixel-precise executive dashboard.