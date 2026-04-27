# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server application that renders project milestone timelines and monthly execution health data from a local JSON file into a pixel-perfect 1920×1080 layout optimized for screenshot capture. The architecture prioritizes extreme simplicity: one solution, one project, zero external dependencies, zero cloud services, and a single `dotnet run` command to launch.

**Primary architectural goals:**

1. **Visual fidelity** — Pixel-accurate reproduction of `OriginalDesignConcept.html` using Blazor components with scoped CSS and inline SVG
2. **Data-driven rendering** — All display content sourced from `wwwroot/data/dashboard-data.json` with zero hardcoded project data in components
3. **Live reload** — File-watch mechanism pushes JSON changes to the browser via SignalR within 2 seconds, no restart required
4. **Error resilience** — Graceful degradation for missing/malformed JSON; the app never crashes or shows stack traces
5. **Zero infrastructure** — No database, no authentication, no cloud services, no additional NuGet packages beyond the .NET 8 Blazor Server template

**Architecture style:** Single-project monolith with a flat service-and-components pattern. No layered architecture, no CQRS, no repository pattern — the data volume (~50 items) and read-only access pattern make these unnecessary.

---

## System Components

### 1. Program.cs — Application Bootstrap

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure Kestrel, register DI services, set up the Blazor Server pipeline |
| **Interface** | N/A (entry point) |
| **Dependencies** | `DashboardDataService` (registered as Singleton) |
| **Data** | None directly; configures the service that loads data |

**Key implementation details:**
- Registers `DashboardDataService` as a Singleton in the DI container
- Configures Kestrel to bind to `http://localhost:5000` only
- Uses the minimal hosting model (`WebApplication.CreateBuilder`)
- Maps Blazor Hub (`app.MapBlazorHub()`) and fallback to `_Host` page
- No authentication middleware, no CORS, no HTTPS redirection for local-only use

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

---

### 2. DashboardDataService — Data Loading & File Watch

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Load, deserialize, cache, and live-reload `dashboard-data.json`; expose current data and error state to components |
| **Interface** | `DashboardData? GetData()`, `string? GetError()`, `event Action? OnDataChanged` |
| **Dependencies** | `System.Text.Json`, `Microsoft.Extensions.FileProviders.PhysicalFileProvider`, `IWebHostEnvironment` |
| **Data** | Holds the current `DashboardData` instance in memory and the last error message |

**Key implementation details:**

```csharp
public class DashboardDataService : IDisposable
{
    private DashboardData? _currentData;
    private string? _error;
    private readonly string _filePath;
    private readonly PhysicalFileProvider _fileProvider;
    private IChangeToken? _changeToken;

    public event Action? OnDataChanged;

    public DashboardData? GetData() => _currentData;
    public string? GetError() => _error;
}
```

**Lifecycle:**
1. On construction (via DI), resolves the path to `wwwroot/data/dashboard-data.json` using `IWebHostEnvironment.WebRootPath`
2. Performs initial load: reads file → deserializes with `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` → stores in `_currentData`
3. If file is missing: `_currentData` remains `null`, `_error` is set to a friendly message
4. If JSON is malformed: `_currentData` remains `null` (or retains last valid), `_error` captures the parse error
5. Registers a `PhysicalFileProvider` watching the `wwwroot/data/` directory
6. On `IChangeToken` callback: re-reads file, attempts deserialization; on success updates `_currentData` and clears `_error`; on failure retains last valid `_currentData` and sets `_error`
7. Fires `OnDataChanged` event after every reload attempt (success or failure)
8. `IDisposable` implementation disposes the `PhysicalFileProvider`

**Thread safety:** The service is a Singleton accessed by Blazor circuits on the server thread pool. Use `lock` or `Interlocked` patterns around `_currentData` and `_error` to prevent torn reads during file reload.

**JsonSerializer configuration:**
```csharp
private static readonly JsonSerializerOptions JsonOptions = new()
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

---

### 3. App.razor — Root Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define the HTML document shell (`<html>`, `<head>`, `<body>`), reference CSS, set viewport meta tag |
| **Interface** | Blazor root component |
| **Dependencies** | `MainLayout.razor` |
| **Data** | None |

**Key markup requirements:**
- `<meta name="viewport" content="width=1920">` to lock viewport
- `<link href="css/app.css" rel="stylesheet" />` for global styles
- `<link href="ReportingDashboard.styles.css" rel="stylesheet" />` for scoped component styles
- `<HeadOutlet>` and `<Routes>` components

---

### 4. MainLayout.razor — Page Shell

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Provide a minimal layout wrapper with no navigation, sidebar, or chrome |
| **Interface** | Blazor layout component with `@Body` render fragment |
| **Dependencies** | None |
| **Data** | None |

**Implementation:** Strip the default Blazor template's `NavMenu`, `<aside>`, and navigation. The layout contains only `@Body` — a bare pass-through to the routed page.

```razor
@inherits LayoutComponentBase
@Body
```

---

### 5. Dashboard.razor — Page Orchestrator

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Inject `DashboardDataService`, subscribe to data changes, distribute data to child components, handle null/error states |
| **Interface** | Blazor page component, route: `"/"` |
| **Dependencies** | `DashboardDataService`, `Header.razor`, `Timeline.razor`, `Heatmap.razor` |
| **Data** | Receives `DashboardData` from service; passes subsections to children via `[Parameter]` |

**State management:**
- `OnInitialized`: calls `DashboardDataService.GetData()` and subscribes to `OnDataChanged`
- `OnDataChanged` handler: calls `InvokeAsync(StateHasChanged)` to trigger re-render via SignalR
- `Dispose`: unsubscribes from `OnDataChanged` to prevent memory leaks

**Rendering logic:**
```
IF data is null AND error is null:
    Render "Loading..." indicator
ELSE IF data is null AND error is not null:
    Render friendly setup message (file missing scenario)
ELSE:
    Render Header, Timeline, Heatmap
    IF error is not null:
        Render error banner at bottom (malformed JSON scenario)
```

---

### 6. Header.razor — Project Title & Legend

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render project title, subtitle, backlog link, and timeline legend icons |
| **Interface** | `[Parameter] ProjectInfo Project` |
| **Dependencies** | None |
| **Data** | `ProjectInfo` record (title, subtitle, backlogUrl, currentMonth) |

**CSS:** Scoped `Header.razor.css` containing `.hdr`, `.sub` classes from reference design.

**Legend items** are rendered from static markup (the four marker types are fixed by design, not data-driven).

---

### 7. Timeline.razor — SVG Milestone Visualization

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the left sidebar track labels and the SVG timeline with gridlines, track lines, markers, and NOW indicator |
| **Interface** | `[Parameter] TimelineData Timeline`, `[Parameter] string? NowOverride` |
| **Dependencies** | None |
| **Data** | `TimelineData` record containing tracks, markers, start/end dates, month definitions |

**SVG coordinate calculation:**

```csharp
private double DateToX(DateTime date)
{
    var totalDays = (Timeline.EndDate - Timeline.StartDate).TotalDays;
    var elapsed = (date - Timeline.StartDate).TotalDays;
    return Math.Clamp(elapsed / totalDays * SvgWidth, 0, SvgWidth);
}

private double TrackY(int trackIndex)
{
    // First track at y=42, spacing of 56px between tracks (matching reference)
    return 42 + (trackIndex * 56);
}
```

**Constants (from reference):**
- `SvgWidth = 1560`, `SvgHeight = 185`
- Month gridline spacing: `SvgWidth / monthCount` (260px for 6 months)
- Diamond radius: 11px (polygon points offset from center)
- Drop-shadow filter: `<filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter>`

**NOW line position:**
- If `NowOverride` is provided in JSON (e.g., `"2026-04-15"`), parse and use that date
- Otherwise, use `DateTime.Now`
- Calculate X position via `DateToX()`

**CSS:** Scoped `Timeline.razor.css` containing `.tl-area`, `.tl-svg-box`, and sidebar styles.

---

### 8. Heatmap.razor — Grid Container

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the heatmap section title, CSS Grid container with header row, and delegate each data row to `HeatmapRow` |
| **Interface** | `[Parameter] HeatmapData Heatmap` |
| **Dependencies** | `HeatmapRow.razor` |
| **Data** | `HeatmapData` record containing months array, currentMonthIndex, and four category arrays |

**Dynamic grid columns:**
```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Heatmap.Months.Length, 1fr);">
```

**Header row rendering:**
- Corner cell: "STATUS" label
- Month headers via `@for` loop with index comparison to `currentMonthIndex` for highlight class

**CSS:** Scoped `Heatmap.razor.css` containing `.hm-wrap`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-col-hdr.current-month` classes.

---

### 9. HeatmapRow.razor — Single Status Row

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render one heatmap row (e.g., Shipped) with row header and data cells for each month |
| **Interface** | `[Parameter] string CategoryName`, `[Parameter] string CssPrefix`, `[Parameter] List<List<string>> ItemsByMonth`, `[Parameter] int CurrentMonthIndex`, `[Parameter] int MonthCount` |
| **Dependencies** | None |
| **Data** | Items organized as a list of lists (one inner list per month) |

**Cell rendering logic:**
```
FOR each month index 0..N:
    IF items list is empty:
        Render single <div class="it"> with "-" text in #AAA
    ELSE:
        FOR each item string:
            Render <div class="it">item text</div>
    Apply "current" CSS class if month index == currentMonthIndex
```

**CSS:** Scoped `HeatmapRow.razor.css` containing `.hm-row-hdr`, `.hm-cell`, `.it`, `.it::before` styles. Each row category (shipped, progress, carryover, blockers) uses CSS custom properties for its color scheme, passed via the `CssPrefix` parameter mapped to class names.

---

### 10. CSS Architecture

**`wwwroot/css/app.css`** — Global styles:
```css
* { margin: 0; padding: 0; box-sizing: border-box; }
body {
    width: 1920px; height: 1080px; overflow: hidden;
    background: #FFFFFF;
    font-family: 'Segoe UI', Arial, sans-serif;
    color: #111;
    display: flex; flex-direction: column;
}
a { color: #0078D4; text-decoration: none; }

:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-current: #D8F2DA;
    --color-shipped-header-bg: #E8F5E9;
    --color-shipped-header-text: #1B7A28;

    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-current: #DAE8FB;
    --color-progress-header-bg: #E3F2FD;
    --color-progress-header-text: #1565C0;

    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-current: #FFF0B0;
    --color-carryover-header-bg: #FFF8E1;
    --color-carryover-header-text: #B45309;

    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-current: #FFE4E4;
    --color-blockers-header-bg: #FEF2F2;
    --color-blockers-header-text: #991B1B;

    --color-now-line: #EA4335;
    --color-poc-diamond: #F4B400;
    --color-prod-diamond: #34A853;
}
```

**Scoped `.razor.css` files** — One per component, containing only that component's styles. Blazor CSS isolation rewrites selectors at build time to scope them automatically.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────┐
│                  File System                         │
│   wwwroot/data/dashboard-data.json                  │
└──────────────┬──────────────────────────────────────┘
               │ PhysicalFileProvider watches
               │ IChangeToken fires on save
               ▼
┌─────────────────────────────────────────────────────┐
│          DashboardDataService (Singleton)            │
│                                                     │
│  ┌─────────────┐    ┌──────────────┐                │
│  │ _currentData │    │ _error       │                │
│  │ DashboardData│    │ string?      │                │
│  └─────────────┘    └──────────────┘                │
│                                                     │
│  Event: OnDataChanged ──────────────────────┐       │
└──────────────┬──────────────────────────────┘       │
               │ DI injection                  │       │
               ▼                               │       │
┌─────────────────────────────────┐            │       │
│     Dashboard.razor (Page)      │◄───────────┘       │
│     Route: "/"                  │  subscribes to     │
│                                 │  OnDataChanged     │
│  Calls: GetData(), GetError()   │                    │
│  Calls: StateHasChanged()       │                    │
└──────┬──────────┬───────────────┘
       │          │           │
       ▼          ▼           ▼
┌──────────┐ ┌──────────┐ ┌──────────────┐
│ Header   │ │ Timeline │ │ Heatmap      │
│ .razor   │ │ .razor   │ │ .razor       │
│          │ │          │ │              │
│[Parameter│ │[Parameter│ │ [Parameter]  │
│ProjectInfo│ │TimelineData│ │ HeatmapData │
└──────────┘ └──────────┘ └──────┬───────┘
                                  │
                    ┌─────────────┼─────────────┐
                    ▼             ▼             ▼
              ┌──────────┐ ┌──────────┐  ┌──────────┐
              │HeatmapRow│ │HeatmapRow│  │HeatmapRow│  (x4)
              │ Shipped  │ │InProgress│  │ Blockers │
              └──────────┘ └──────────┘  └──────────┘
```

### Communication Patterns

1. **Service → Components (Push via Event):** `DashboardDataService` fires `OnDataChanged` when JSON reloads. `Dashboard.razor` subscribes in `OnInitialized` and calls `InvokeAsync(StateHasChanged)` to trigger Blazor's diffing engine. The updated UI is pushed to the browser over the existing SignalR WebSocket — no browser refresh needed.

2. **Components → Components (Parameters):** All data flows downward via `[Parameter]` properties. No cascading values, no shared state, no two-way binding. Each child component is a pure function of its parameters.

3. **Browser ← Server (SignalR):** Blazor Server's built-in SignalR circuit handles all UI updates. When `StateHasChanged()` fires, Blazor computes a render diff and sends minimal DOM patches over WebSocket.

4. **No upward data flow:** Components do not modify data or communicate upward. The dashboard is read-only.

---

## Data Model

### JSON Schema (`dashboard-data.json`)

```json
{
  "schemaVersion": 1,
  "project": {
    "title": "Contoso Platform Modernization Release Roadmap",
    "subtitle": "Engineering Excellence · Platform Team · April 2026",
    "backlogUrl": "https://dev.azure.com/contoso/platform/_backlogs",
    "currentMonth": "Apr 2026"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "nowOverride": null,
    "months": [
      { "label": "Jan", "date": "2026-01-01" },
      { "label": "Feb", "date": "2026-02-01" },
      { "label": "Mar", "date": "2026-03-01" },
      { "label": "Apr", "date": "2026-04-01" },
      { "label": "May", "date": "2026-05-01" },
      { "label": "Jun", "date": "2026-06-01" }
    ],
    "tracks": [
      {
        "id": "M1",
        "name": "Auth & Identity",
        "color": "#0078D4",
        "markers": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12", "style": "open" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "May Prod" }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonthIndex": 3,
    "shipped": [
      ["Item A", "Item B"],
      ["Item C"],
      ["Item D", "Item E", "Item F"],
      ["Item G"]
    ],
    "inProgress": [
      ["Task 1"],
      ["Task 2", "Task 3"],
      ["Task 4"],
      ["Task 5", "Task 6"]
    ],
    "carryover": [
      [],
      ["Slip 1"],
      ["Slip 2"],
      ["Slip 3"]
    ],
    "blockers": [
      [],
      [],
      ["Block 1"],
      ["Block 2"]
    ]
  }
}
```

### C# Data Models

All models are defined as C# records for immutability and value equality:

```csharp
// Models/DashboardData.cs
public record DashboardData(
    int SchemaVersion,
    ProjectInfo Project,
    TimelineData Timeline,
    HeatmapData Heatmap
);

// Models/ProjectInfo.cs
public record ProjectInfo(
    string Title,
    string? Subtitle,
    string? BacklogUrl,
    string? CurrentMonth
);

// Models/TimelineData.cs
public record TimelineData(
    string StartDate,
    string EndDate,
    string? NowOverride,
    List<TimelineMonth> Months,
    List<TimelineTrack> Tracks
);

public record TimelineMonth(
    string Label,
    string Date
);

public record TimelineTrack(
    string Id,
    string Name,
    string Color,
    List<TimelineMarker> Markers
);

public record TimelineMarker(
    string Date,
    string Type,       // "checkpoint" | "poc" | "production"
    string? Label,
    string? Style      // "open" | "filled" (for checkpoints)
);

// Models/HeatmapData.cs
public record HeatmapData(
    List<string> Months,
    int CurrentMonthIndex,
    List<List<string>> Shipped,
    List<List<string>> InProgress,
    List<List<string>> Carryover,
    List<List<string>> Blockers
);
```

### Entity Relationships

```
DashboardData (1)
├── ProjectInfo (1)          — title, subtitle, backlogUrl, currentMonth
├── TimelineData (1)         — date range, NOW override
│   ├── TimelineMonth (N)    — label + date per month gridline
│   └── TimelineTrack (N)    — id, name, color per workstream
│       └── TimelineMarker (N) — date, type, label per marker
└── HeatmapData (1)          — months list, current month index
    ├── Shipped (N months × M items)
    ├── InProgress (N months × M items)
    ├── Carryover (N months × M items)
    └── Blockers (N months × M items)
```

### Storage

- **At rest:** Single `wwwroot/data/dashboard-data.json` file, plain-text UTF-8, no encryption
- **In memory:** Single `DashboardData` record instance held by `DashboardDataService` Singleton
- **No database.** No ORM. No migration scripts. The JSON file is the entire persistence layer.

---

## API Contracts

### Internal Service API

This application has **no external HTTP API**. There are no REST endpoints, no controllers, no Swagger/OpenAPI specification. Components consume the `DashboardDataService` directly via dependency injection.

**Service contract (C# interface):**

```csharp
// This is the implicit contract of DashboardDataService.
// No formal interface is extracted because there is only one implementation.

public DashboardData? GetData();
// Returns the current deserialized dashboard data, or null if no valid data has been loaded.

public string? GetError();
// Returns a user-friendly error message if the last load/reload failed, or null if data is healthy.

public event Action? OnDataChanged;
// Fires after every file-watch reload attempt (success or failure).
// Subscribers must call InvokeAsync(StateHasChanged) to update the UI.
```

### Blazor Component Parameter Contracts

| Component | Parameters | Types |
|-----------|-----------|-------|
| `Header` | `Project` | `ProjectInfo` (required) |
| `Timeline` | `Timeline` | `TimelineData` (required) |
| `Heatmap` | `Heatmap` | `HeatmapData` (required) |
| `HeatmapRow` | `CategoryName` | `string` — display name (e.g., "Shipped") |
| | `CssPrefix` | `string` — CSS class prefix (e.g., "ship") |
| | `ItemsByMonth` | `List<List<string>>` — items per month column |
| | `CurrentMonthIndex` | `int` — which column to highlight |
| | `MonthCount` | `int` — total month columns |

### Error Handling Contract

| Condition | Behavior |
|-----------|----------|
| JSON file missing at startup | `GetData()` returns `null`, `GetError()` returns setup instructions message. Dashboard renders "No data found" with a helpful hint pointing to the sample file. |
| JSON file malformed at startup | `GetData()` returns `null`, `GetError()` returns parse error summary. Dashboard renders the error message. |
| JSON file becomes malformed during live-reload | `GetData()` returns **last valid data** (not null), `GetError()` returns parse error. Dashboard continues showing previous data with a red error banner. |
| JSON file is fixed after error | `GetData()` returns new valid data, `GetError()` returns `null`. Error banner disappears. |
| JSON has missing optional fields | Nullable record properties default to `null`. Components render defaults (empty strings, hidden elements). |

---

## Infrastructure Requirements

### Hosting

| Aspect | Requirement |
|--------|-------------|
| **Web Server** | Kestrel (built into ASP.NET Core 8) — no IIS, no Nginx, no reverse proxy |
| **Binding** | `http://localhost:5000` only (configured in `launchSettings.json` and/or `Program.cs`) |
| **Protocol** | HTTP only for local development. HTTPS optional via `https://localhost:5001` |
| **Process Model** | Single Kestrel process, single Blazor Server hub, single SignalR circuit |

### Networking

- **Inbound:** localhost only. No firewall rules. No external network access.
- **Outbound:** None. The application makes zero network calls. All data is local.
- **SignalR:** WebSocket connection from browser to Kestrel on localhost. Automatic fallback to Server-Sent Events or Long Polling if WebSocket is unavailable (unlikely on localhost).

### Storage

| Item | Location | Size |
|------|----------|------|
| `dashboard-data.json` | `wwwroot/data/` | ~5-20 KB |
| `dashboard-data.sample.json` | `wwwroot/data/` | ~5-20 KB |
| Application binaries | `bin/Debug/net8.0/` | ~5 MB |
| Published output | `bin/Release/net8.0/publish/` | ~5 MB (framework-dependent) or ~80 MB (self-contained) |

### Development Prerequisites

| Prerequisite | Version | Required |
|-------------|---------|----------|
| .NET 8 SDK | 8.0.x LTS | Yes |
| Windows OS | 10/11 | Yes (Segoe UI font dependency) |
| Microsoft Edge or Chrome | Latest | Yes (for viewing/screenshots) |
| VS Code or Visual Studio 2022 | Latest | Recommended |
| Git | Any | Recommended |

### CI/CD

**Not required.** This is a local-only, single-developer tool. No build pipelines, no deployment targets, no artifact registries. The app is built and run on the same machine.

If CI/CD is ever added in the future, a single GitHub Actions step would suffice:
```yaml
- run: dotnet build --configuration Release
- run: dotnet test
```

---

## Technology Stack Decisions

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Runtime** | .NET 8 LTS | 8.0.x | Mandatory stack. Long-term support until Nov 2026. |
| **UI Framework** | Blazor Server | .NET 8 built-in | Mandatory stack. Server-side rendering eliminates WASM download. SignalR enables live-reload push. |
| **Rendering** | Razor Components (.razor) | .NET 8 built-in | Component model maps 1:1 to the HTML reference design sections. |
| **CSS** | Blazor CSS Isolation + global `app.css` | .NET 8 built-in | Scoped styles per component prevent naming collisions. No CSS framework needed. |
| **SVG** | Inline SVG in Razor markup | N/A | Reference design uses raw SVG. Hand-coded SVG gives pixel-perfect control. No charting library. |
| **Layout** | CSS Grid + Flexbox | CSS3 | Direct match to the reference HTML. No CSS framework overhead. |
| **Data Format** | JSON | N/A | Human-editable, natively supported by `System.Text.Json`, universally understood. |
| **Serialization** | `System.Text.Json` | .NET 8 built-in | Zero-dependency JSON handling with excellent performance. |
| **File Watching** | `PhysicalFileProvider` + `IChangeToken` | .NET 8 built-in | Detects file changes and triggers callback. Built into ASP.NET Core, no packages needed. |
| **Web Server** | Kestrel | .NET 8 built-in | Default ASP.NET Core server. Lightweight, fast, localhost-only. |
| **Font** | Segoe UI (system) | N/A | Pre-installed on Windows. Falls back to Arial/sans-serif. No web font loading. |
| **Testing** | xUnit + bUnit | 2.7+ / 1.25+ | Standard .NET testing. bUnit for Razor component rendering tests. (Test project only.) |

### Technologies Explicitly Rejected

| Technology | Reason |
|-----------|--------|
| MudBlazor / Radzen / Syncfusion | Adds 500KB+ of CSS/JS, fights pixel-precise design, maintenance burden |
| Chart.js / Plotly via JS Interop | Overkill for ~20 SVG elements; introduces JS interop complexity |
| Tailwind CSS | Requires PostCSS build pipeline; ~50 CSS rules don't justify it |
| Entity Framework / SQLite | Massive overkill for ~50 read-only data points in a flat file |
| Serilog / Application Insights | Out of scope; local-only app with no operational monitoring needs |
| Docker | Not needed; `dotnet run` is the deployment strategy |
| YAML / TOML / XML | JSON has native .NET support; alternatives add dependencies |

### NuGet Package Policy

**Zero additional packages for the application project.** The `.csproj` contains only the default Blazor Server SDK reference:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

Test project packages (separate `.csproj`, only if tests are written):
```xml
<PackageReference Include="bunit" Version="1.*" />
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
```

---

## Security Considerations

### Threat Model

**Attack surface: Minimal.** This is a single-user application binding to `localhost` only, with no authentication, no data modification endpoints, and no external network access.

| Threat | Risk Level | Mitigation |
|--------|-----------|------------|
| **Unauthorized network access** | Low | Kestrel binds to `localhost` only. No external interfaces are exposed. Configure in `launchSettings.json`: `"applicationUrl": "http://localhost:5000"` |
| **JSON injection / XSS** | Low | Blazor's Razor rendering engine HTML-encodes all bound expressions by default. Even if the JSON contains `<script>` tags, they render as escaped text, not executable markup. SVG attribute bindings use string interpolation into numeric contexts (coordinates), further reducing injection risk. |
| **File system access** | Low | The app reads one file (`dashboard-data.json`) in `wwwroot/data/`. No user input controls file paths. No file upload capability. |
| **Denial of service** | N/A | Single-user local app. No network exposure. |
| **Data leakage** | Low | `dashboard-data.json` is in `.gitignore`. Only the sample file is committed. Real project data stays on the local machine. |
| **Supply chain** | Low | Zero additional NuGet packages. Attack surface is limited to the .NET 8 SDK itself, which is Microsoft-maintained. |
| **SignalR circuit hijacking** | Low | SignalR runs over localhost WebSocket. No cross-origin access. Blazor's built-in antiforgery token protects the circuit. |

### Authentication & Authorization

**None.** Explicitly out of scope. The app has no login page, no user sessions, no role-based access, no API keys. Anyone with access to the machine can view the dashboard.

**Future extension (if ever needed):** Add `builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate()` for Windows Integrated Auth. This is a one-line addition that requires no UI changes.

### Data Protection

- **Data classification:** Non-sensitive. The JSON contains project status information that is shared in executive meetings.
- **Encryption at rest:** Not required. Plain-text JSON.
- **Encryption in transit:** Not required (localhost only). HTTPS is available via Kestrel's default dev certificate if desired.
- **Input validation:** JSON deserialization uses `System.Text.Json` which rejects malformed input. Nullable record properties with sensible defaults handle missing fields gracefully.

### `.gitignore` Policy

```gitignore
# Prevent committing real project data
wwwroot/data/dashboard-data.json
```

Only `dashboard-data.sample.json` is committed to the repository.

---

## Scaling Strategy

### Current Design: Single-User Local

This application is architecturally designed for **exactly one user on one machine**. There is no scaling requirement, no horizontal scaling capability, and no need for either.

| Dimension | Strategy |
|-----------|----------|
| **Users** | 1 (the PM taking screenshots) |
| **Connections** | 1 SignalR circuit |
| **Data volume** | ~50 items in a ~10KB JSON file |
| **Compute** | < 100 MB RAM, negligible CPU |
| **Storage** | Single JSON file on local disk |

### If Scaling Were Ever Needed (Not Planned)

| Scenario | Approach |
|----------|----------|
| **Multiple simultaneous viewers** | Blazor Server natively supports multiple SignalR circuits. The Singleton `DashboardDataService` is already shared. No code changes needed for ~10 concurrent viewers. |
| **Remote access (non-localhost)** | Change Kestrel binding to `0.0.0.0:5000`. Add Windows Auth. No architecture change. |
| **Multiple projects** | Add a route parameter (`/dashboard/{project}`) and load different JSON files per route. Modest refactor of `DashboardDataService`. |
| **Large data volumes** | Not applicable. Executive dashboards have inherently small data sets (fits on one screen). |

### Performance Targets (Already Met by Architecture)

| Metric | Target | Why Architecture Meets It |
|--------|--------|--------------------------|
| Cold start | < 3 seconds | Blazor Server has no WASM download. Kestrel starts in ~1 second. JSON deserialization is < 1ms for a 10KB file. |
| Render time | < 500ms | All rendering is server-side string concatenation (Razor) + diff patch via SignalR. No complex computation. |
| Reload time | < 2 seconds | `PhysicalFileProvider` fires `IChangeToken` on file save. Deserialization + `StateHasChanged` + SignalR diff patch < 100ms. |
| Memory | < 100 MB | Kestrel + one Blazor circuit + one small POCO in memory. .NET 8 has a ~30 MB baseline. |

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|------------|
| 1 | **SVG timeline marker positions don't match reference design** | Medium | Medium | Use the reference HTML's exact coordinate math (260px per month, y=42 for first track, 56px track spacing). Write bUnit tests that assert SVG element attributes (cx, cy, x1, y1 values). Perform visual comparison against `OriginalDesignConcept.png` at 1920×1080. |
| 2 | **JSON schema evolves and breaks deserialization** | Medium | Medium | Add `"schemaVersion": 1` field. Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`. Define all non-critical fields as nullable with defaults. Document the schema in README.md. |
| 3 | **File watcher fires multiple times on a single save** | Medium | Low | Text editors (VS Code, Notepad) may trigger multiple write events per save. Debounce the reload: ignore change tokens that fire within 500ms of the last successful reload using a `Timer` or `DateTime` check. |
| 4 | **Blazor CSS isolation breaks scoped styles** | Low | Medium | Blazor CSS isolation works via build-time attribute rewriting. Ensure `<link href="ReportingDashboard.styles.css" rel="stylesheet" />` is in `App.razor`. Test scoped styles in the browser. Fallback: move styles to global `app.css` if isolation causes issues. |
| 5 | **Browser rendering differences between Edge and Chrome** | Low | Low | Both are Chromium-based with identical rendering engines. Use explicit pixel values (not relative units) for critical layout. Set `body { width: 1920px; height: 1080px }` as the hard constraint. |
| 6 | **SignalR circuit disconnects during long idle periods** | Low | Low | Blazor Server's default circuit timeout is 3 minutes of inactivity. For this use case (active editing + screenshot), disconnects are unlikely. If needed, extend via `builder.Services.AddServerSideBlazor(o => o.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(30))`. |
| 7 | **Heatmap grid overflows 1080px height with too many items** | Medium | Medium | The heatmap uses `flex: 1` with `min-height: 0` and `overflow: hidden`. Excess items are clipped, not scrolled. Document the practical limit (~8 items per cell at 12px line-height) in README. |
| 8 | **`PhysicalFileProvider` fails on certain Windows file systems** | Low | High | `PhysicalFileProvider` uses `ReadDirectoryChangesW` which works on NTFS, FAT32, and ReFS. If the project is on a network share or unusual mount, file watching may not fire. Mitigation: document that the project must be on a local NTFS drive. |
| 9 | **Developer accidentally commits real project data** | Medium | Low | `dashboard-data.json` is in `.gitignore`. Only `dashboard-data.sample.json` is tracked. Document this in README. Add a pre-commit check reminder. |
| 10 | **The Blazor Server template adds unnecessary boilerplate** | Low | Low | After scaffolding, immediately remove: `NavMenu.razor`, `Counter.razor`, `Weather.razor`, `Error.razor` (or replace), Bootstrap CSS, `WeatherForecast.cs`, and the `HttpClient` registration. Document the cleanup steps in the implementation plan. |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to specific Blazor components, their CSS strategies, data bindings, and interactions.

### Component Tree

```
App.razor
└── MainLayout.razor
    └── Dashboard.razor  (@page "/")
        ├── Header.razor
        ├── Timeline.razor
        └── Heatmap.razor
            ├── HeatmapRow.razor  (Shipped)
            ├── HeatmapRow.razor  (In Progress)
            ├── HeatmapRow.razor  (Carryover)
            └── HeatmapRow.razor  (Blockers)
```

### Visual Section → Component Mapping

#### Section 1: Header Bar → `Header.razor`

| Aspect | Specification |
|--------|--------------|
| **HTML Reference** | `.hdr` div — the top bar of `OriginalDesignConcept.html` |
| **CSS Layout** | Flexbox row: `display: flex; align-items: center; justify-content: space-between` |
| **Fixed Dimensions** | Height: auto (~50px content-driven). Padding: `12px 44px 10px`. Bottom border: `1px solid #E0E0E0`. |
| **Data Bindings** | `@Project.Title` → `<h1>` text. `@Project.BacklogUrl` → `<a href>`. `@Project.Subtitle` → `.sub` div text. |
| **Left Side Markup** | `<h1>` at 24px/700 weight with inline `<a>` link in #0078D4. Subtitle `<div class="sub">` in 12px #888. |
| **Right Side (Legend)** | Static markup: four `<span>` items in a flex row with 22px gap. Gold rotated square (PoC), green rotated square (Prod), gray circle (Checkpoint), red bar (NOW). |
| **Interactions** | Backlog link opens `target="_blank"`. No other interactions. |
| **Scoped CSS File** | `Header.razor.css` — contains `.hdr`, `.hdr h1`, `.sub`, legend item styles |

#### Section 2: Timeline Area → `Timeline.razor`

| Aspect | Specification |
|--------|--------------|
| **HTML Reference** | `.tl-area` div containing left sidebar + `.tl-svg-box` with inline `<svg>` |
| **CSS Layout** | Outer: Flexbox row, `align-items: stretch`. Inner sidebar: 230px fixed width. SVG box: `flex: 1`. |
| **Fixed Dimensions** | Height: 196px. Background: #FAFAFA. Bottom border: `2px solid #E8E8E8`. Padding: `6px 44px 0`. |
| **Left Sidebar Bindings** | `@foreach track in Timeline.Tracks` → renders track ID (`M1`) in track color + track name in #444. Sidebar has `justify-content: space-around` for even spacing. |
| **SVG Dimensions** | `width="1560" height="185"` with `overflow: visible` |
| **SVG Data Bindings** | Month gridlines: `@foreach month` → `<line>` + `<text>`. Track lines: `@foreach track` → horizontal `<line>` at calculated Y. Markers: `@foreach marker in track.Markers` → checkpoint `<circle>`, poc/prod `<polygon>` diamond, + `<text>` date label. NOW line: dashed `<line>` at `DateToX(nowDate)`. |
| **Position Calculation** | X: `(markerDate - startDate).TotalDays / (endDate - startDate).TotalDays * 1560`. Y: `42 + (trackIndex * 56)`. Diamond points: `x,y-11 x+11,y x,y+11 x-11,y`. |
| **SVG Filter** | `<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>` applied to diamond `<polygon>` elements |
| **Interactions** | SVG `<title>` child elements on diamonds for native browser tooltip on hover. No click handlers. |
| **Scoped CSS File** | `Timeline.razor.css` — contains `.tl-area`, `.tl-svg-box`, sidebar styles |

#### Section 3: Heatmap Grid → `Heatmap.razor` + `HeatmapRow.razor`

| Aspect | Specification |
|--------|--------------|
| **HTML Reference** | `.hm-wrap` containing `.hm-title` + `.hm-grid` |
| **CSS Layout** | Outer: `flex: 1; min-height: 0; display: flex; flex-direction: column`. Grid: CSS Grid with dynamic columns. |
| **Grid Columns** | `grid-template-columns: 160px repeat(@monthCount, 1fr)` — computed from `Heatmap.Months.Length` |
| **Grid Rows** | `grid-template-rows: 36px repeat(4, 1fr)` — 36px header + 4 equal data rows |
| **Title Binding** | Static text: "MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS" in `.hm-title` |
| **Header Row Bindings** | Corner cell: "STATUS". Month headers: `@for (int i = 0; i < months.Length; i++)` with current-month highlight class when `i == currentMonthIndex`. Current month header: background #FFF0D0, color #C07700, with "◀ Now" text. |
| **Data Row Delegation** | Four `<HeatmapRow>` instances with parameters: `CategoryName="Shipped"`, `CssPrefix="ship"`, `ItemsByMonth=@Heatmap.Shipped`, etc. |
| **Scoped CSS File** | `Heatmap.razor.css` — `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.current-month` |

#### HeatmapRow Component Detail

| Aspect | Specification |
|--------|--------------|
| **Row Header** | `<div class="hm-row-hdr {CssPrefix}-hdr">` — uppercase category name, colored per category via CSS prefix class |
| **Data Cells** | `@for` loop over months. Each cell: `<div class="hm-cell {CssPrefix}-cell @(isCurrent ? "current" : "")">`. Items: `@foreach item` → `<div class="it">@item</div>`. Empty: `<div class="it empty">-</div>`. |
| **Bullet Styling** | `.it::before` pseudo-element: 6×6px circle, colored per category. Positioned absolute at `left: 0; top: 7px`. |
| **Current Month Highlight** | Cell class toggles `.current` which applies darker background tint per category |
| **Scoped CSS File** | `HeatmapRow.razor.css` — `.hm-row-hdr`, `.hm-cell`, `.it`, `.it::before`, color variant classes (`.ship-hdr`, `.ship-cell`, `.prog-hdr`, `.prog-cell`, `.carry-hdr`, `.carry-cell`, `.block-hdr`, `.block-cell`, `.current`) |

### Error/Empty State Components (within Dashboard.razor)

| State | Visual |
|-------|--------|
| **No JSON file** | Centered `<div>` with message: "No dashboard data found. Place a `dashboard-data.json` file in `wwwroot/data/` to get started." Gray text, no grid, no timeline. |
| **Malformed JSON (live-reload)** | Full dashboard renders with last valid data. A 32px-high red banner fixed to bottom: "⚠ Data file error — showing last valid data" in white text on #EA4335 background. |
| **Empty tracks** | Timeline renders gridlines and NOW line only. Sidebar is empty. |
| **Empty heatmap items** | Grid structure renders normally. Every cell shows "-" in #AAA. |

---

## Solution Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── .gitignore
├── README.md
├── OriginalDesignConcept.html              (reference design)
├── docs/
│   └── design-screenshots/
│       └── OriginalDesignConcept.png       (reference screenshot)
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs
│       ├── Models/
│       │   ├── DashboardData.cs
│       │   ├── ProjectInfo.cs
│       │   ├── TimelineData.cs
│       │   └── HeatmapData.cs
│       ├── Services/
│       │   └── DashboardDataService.cs
│       ├── Components/
│       │   ├── App.razor
│       │   ├── Routes.razor
│       │   ├── _Imports.razor
│       │   ├── Pages/
│       │   │   └── Dashboard.razor
│       │   ├── Layout/
│       │   │   └── MainLayout.razor
│       │   └── Shared/
│       │       ├── Header.razor
│       │       ├── Header.razor.css
│       │       ├── Timeline.razor
│       │       ├── Timeline.razor.css
│       │       ├── Heatmap.razor
│       │       ├── Heatmap.razor.css
│       │       ├── HeatmapRow.razor
│       │       └── HeatmapRow.razor.css
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── app.css
│       │   └── data/
│       │       ├── dashboard-data.json         (gitignored)
│       │       └── dashboard-data.sample.json  (committed)
│       └── Properties/
│           └── launchSettings.json
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        ├── Services/
        │   └── DashboardDataServiceTests.cs
        └── Components/
            ├── HeaderTests.cs
            ├── HeatmapRowTests.cs
            └── TimelineTests.cs
```

**Total source files:** ~15 (meeting the ≤ 15 file target from success metrics).

**Build & Run:**
```bash
cd src/ReportingDashboard
dotnet run
# Dashboard available at http://localhost:5000
```

**Test:**
```bash
cd tests/ReportingDashboard.Tests
dotnet test
```