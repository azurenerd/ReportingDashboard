# Architecture

## Overview & Goals

This document defines the complete system architecture for the **Executive Reporting Dashboard** — a single-page Blazor Server application that renders project milestones, execution status, and team health into a pixel-perfect 1920×1080 view optimized for PowerPoint screenshot capture.

**Architecture Philosophy:** Minimal, flat, zero-dependency. This is a read-only data visualization page, not a CRUD application. Every architectural decision optimizes for simplicity, visual fidelity, and instant local startup.

**Goals:**

1. **Single-page rendering** — One Blazor page reads `data.json` and renders the complete dashboard with no navigation, no routing, no multi-page flows.
2. **Screenshot-perfect output** — Fixed 1920×1080 viewport with no scrollbars, no framework chrome, no loading states visible at render time.
3. **Zero infrastructure** — No database, no cloud, no authentication, no external API calls. `dotnet run` + browser = working dashboard.
4. **Data-driven content** — All visible text, milestones, and heatmap items come from `data.json`. Changing the file and refreshing the browser updates the dashboard instantly.
5. **≤ 10 source files, ≤ 500 lines of code** — The architecture enforces radical simplicity. No abstraction layers, no patterns beyond what the problem demands.

---

## System Components

### Component 1: `Program.cs` — Application Host

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure Kestrel, register services, map Razor components, start the application |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService` (registered as singleton) |
| **Data** | Reads `appsettings.json` for configuration (data file path) |

**Key behaviors:**
- Binds Kestrel to `127.0.0.1:5000` only (not `0.0.0.0`)
- Registers `AddRazorComponents()` without interactive server components (Static SSR — no SignalR)
- Registers `DashboardDataService` as a **scoped** service so each request re-reads `data.json` (supports F5 refresh)
- No HTTPS, no authentication middleware, no CORS, no API controllers

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.ListenLocalhost(5000));
builder.Services.AddRazorComponents();  // Static SSR only — no SignalR
builder.Services.AddScoped<DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>();  // No .AddInteractiveServerRenderMode()

app.Run();
```

**Design decision — Static SSR over Interactive Server:** The dashboard has zero interactivity (no buttons, no forms, no user input). Static SSR renders pure HTML on the server and sends it to the browser with no SignalR WebSocket connection. This eliminates the Blazor reconnection banner problem entirely and produces a lighter, faster page load.

---

### Component 2: `DashboardData.cs` — Data Model

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define C# record types that map 1:1 to the `data.json` schema |
| **Interfaces** | Public record types consumed by `DashboardDataService` and `Dashboard.razor` |
| **Dependencies** | None (pure data model, no logic) |
| **Data** | Deserialized from `data.json` via `System.Text.Json` |
| **Location** | `Data/DashboardData.cs` |

**Records defined (see [Data Model](#data-model) section for full detail):**
- `DashboardReport` — root object
- `HeaderInfo` — title, subtitle, backlog link, report date, timeline date range
- `TimelineTrack` — workstream track with color and milestones
- `Milestone` — individual milestone with date, label, type, and label position
- `HeatmapData` — column definitions and highlight index
- `HeatmapRow` — category row with per-column item lists

---

### Component 3: `DashboardDataService.cs` — Data Access Service

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Read `data.json` from disk, deserialize to `DashboardReport`, handle errors gracefully |
| **Interfaces** | `Task<DashboardReport> GetDataAsync()` — returns deserialized data or throws with a user-friendly message |
| **Dependencies** | `IConfiguration` (to read file path from `appsettings.json`), `System.Text.Json`, `System.IO` |
| **Data** | Reads `Data/data.json` (configurable path) |
| **Location** | `Data/DashboardDataService.cs` |
| **Lifetime** | **Scoped** — one instance per HTTP request, ensuring F5 always re-reads the file |

```csharp
public class DashboardDataService
{
    private readonly string _filePath;

    public DashboardDataService(IConfiguration config)
    {
        _filePath = config.GetValue<string>("DashboardDataPath") ?? "Data/data.json";
    }

    public async Task<DashboardReport> GetDataAsync()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException(
                $"Dashboard data file not found at: {Path.GetFullPath(_filePath)}");

        var json = await File.ReadAllTextAsync(_filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<DashboardReport>(json, options)
            ?? throw new InvalidOperationException("data.json deserialized to null");
    }
}
```

**Error handling strategy:** The service throws typed exceptions. `Dashboard.razor` catches them in `OnInitializedAsync` and renders a user-friendly error message instead of the dashboard. No retry logic — the user fixes `data.json` and hits F5.

---

### Component 4: `Dashboard.razor` + `Dashboard.razor.css` — The Single Page

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the complete dashboard: header, SVG timeline, and CSS Grid heatmap |
| **Interfaces** | Blazor page at route `/` |
| **Dependencies** | `DashboardDataService` (injected via `@inject`) |
| **Data** | `DashboardReport` instance loaded in `OnInitializedAsync` |
| **Location** | `Components/Pages/Dashboard.razor` and `.razor.css` |

**Key behaviors:**
- Loads data in `OnInitializedAsync`, stores in a `DashboardReport? Data` field and an `string? ErrorMessage` field
- If data loads successfully, renders three sections: Header, Timeline, Heatmap
- If data fails to load, renders a centered error message with the exception detail
- Contains `@code` block with helper methods for SVG coordinate calculation:
  - `DateToX(DateOnly date)` — linear interpolation of date to pixel X within the SVG
  - `DiamondPoints(double x, double y)` — returns SVG polygon points string for a diamond marker
  - `TrackY(int index)` — calculates Y position for track N (evenly spaced)
- All CSS is in the scoped `.razor.css` file, ported directly from `OriginalDesignConcept.html`

---

### Component 5: `App.razor` — Root Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | HTML document shell: `<html>`, `<head>`, `<body>`, stylesheet links, Blazor script |
| **Dependencies** | References `MainLayout` as the default layout |
| **Location** | `Components/App.razor` |

**Key behaviors:**
- Links `css/app.css` (global resets) and the Blazor scoped CSS bundle
- Includes `<script src="_framework/blazor.web.js"></script>` for Static SSR hydration
- Suppresses `#blazor-error-ui` with `display:none`
- Sets viewport meta tag (though not strictly needed for fixed-width)

---

### Component 6: `MainLayout.razor` + `MainLayout.razor.css` — Layout Shell

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Minimal layout wrapper — just `@Body`, no navigation, no sidebar, no chrome |
| **Location** | `Components/Layout/MainLayout.razor` |

```razor
@inherits LayoutComponentBase
<main>@Body</main>
```

The `.razor.css` sets `main` to fill the viewport:
```css
main {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
}
```

---

### Component 7: `app.css` — Global Stylesheet

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | CSS reset and body-level styles only |
| **Location** | `wwwroot/css/app.css` |

```css
*, *::before, *::after { margin: 0; padding: 0; box-sizing: border-box; }
body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
    background: #FFFFFF;
    font-family: 'Segoe UI', -apple-system, Arial, sans-serif;
    color: #111;
}
a { color: #0078D4; text-decoration: none; }
#blazor-error-ui { display: none !important; }
```

---

### Component 8: `data.json` — Dashboard Data File

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Single source of truth for all dashboard content |
| **Location** | `Data/data.json` (gitignored for real data) |
| **Format** | JSON matching the `DashboardReport` schema |

A `Data/data.example.json` with fictional "Project Phoenix" data is committed to the repo as a reference.

---

### Component 9: `appsettings.json` — Application Configuration

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Store configurable settings (data file path) |
| **Location** | Project root `appsettings.json` |

```json
{
  "DashboardDataPath": "Data/data.json",
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

---

## Component Interactions

### Data Flow Diagram

```
┌──────────────┐     ┌──────────────────────┐     ┌───────────────────┐
│  data.json   │────▶│ DashboardDataService  │────▶│  Dashboard.razor  │
│  (flat file) │     │ (scoped, reads file   │     │  (renders HTML +  │
│              │     │  per request)          │     │   inline SVG)     │
└──────────────┘     └──────────────────────┘     └───────────────────┘
                              │                            │
                     ┌────────┘                            │
                     ▼                                     ▼
              ┌──────────────┐                   ┌──────────────────┐
              │ appsettings  │                   │  Browser (Edge/  │
              │   .json      │                   │  Chrome) @       │
              │ (file path)  │                   │  localhost:5000  │
              └──────────────┘                   └──────────────────┘
```

### Request Lifecycle

1. **User navigates to `http://localhost:5000`** — Kestrel receives the HTTP GET request.
2. **Blazor Static SSR pipeline activates** — The framework instantiates `Dashboard.razor` within `MainLayout.razor`.
3. **`OnInitializedAsync` fires** — `Dashboard.razor` calls `DashboardDataService.GetDataAsync()`.
4. **Service reads `data.json`** — `File.ReadAllTextAsync` reads the file, `JsonSerializer.Deserialize` parses it into `DashboardReport`.
5. **Component renders** — Razor markup iterates over the data model, producing:
   - HTML `<div>` elements for the header and heatmap
   - Inline `<svg>` elements for the timeline
   - Scoped CSS classes for color-coding
6. **Server sends complete HTML** — Static SSR sends the fully-rendered HTML page to the browser in a single response. No WebSocket connection is established.
7. **Browser renders the page** — The user sees the complete dashboard. No JavaScript interaction occurs beyond Blazor's minimal static SSR script.

### Communication Patterns

| From | To | Protocol | Pattern |
|------|----|----------|---------|
| Browser | Kestrel | HTTP GET | Single request/response (Static SSR) |
| `Dashboard.razor` | `DashboardDataService` | C# method call | Direct DI injection, `await GetDataAsync()` |
| `DashboardDataService` | File system | `System.IO` | `File.ReadAllTextAsync("Data/data.json")` |
| `DashboardDataService` | `IConfiguration` | C# DI | Reads `DashboardDataPath` setting |

**No SignalR.** No WebSocket. No polling. No external HTTP calls. The entire data flow is: file → C# deserialization → Razor rendering → HTML response.

---

## Data Model

### Entity: `DashboardReport` (Root)

The root object deserialized from `data.json`.

```csharp
public record DashboardReport
{
    public HeaderInfo Header { get; init; } = new();
    public List<TimelineTrack> TimelineTracks { get; init; } = [];
    public HeatmapData Heatmap { get; init; } = new();
}
```

### Entity: `HeaderInfo`

| Property | Type | JSON Key | Description |
|----------|------|----------|-------------|
| `Title` | `string` | `title` | Project title displayed in `<h1>` (e.g., "Project Phoenix Release Roadmap") |
| `Subtitle` | `string` | `subtitle` | Organizational context line (e.g., "Engineering Division · Platform Modernization · April 2026") |
| `BacklogLink` | `string` | `backlogLink` | URL for the "→ ADO Backlog" hyperlink |
| `ReportDate` | `string` | `reportDate` | ISO date string (e.g., "2026-04-17"); used to calculate the NOW line position |
| `TimelineStartDate` | `string` | `timelineStartDate` | Start of the timeline range (e.g., "2026-01-01") |
| `TimelineEndDate` | `string` | `timelineEndDate` | End of the timeline range (e.g., "2026-07-01") |
| `TimelineMonths` | `List<string>` | `timelineMonths` | Month labels for the SVG gridlines (e.g., `["Jan","Feb","Mar","Apr","May","Jun"]`) |

```csharp
public record HeaderInfo
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogLink { get; init; } = "#";
    public string ReportDate { get; init; } = "";
    public string TimelineStartDate { get; init; } = "";
    public string TimelineEndDate { get; init; } = "";
    public List<string> TimelineMonths { get; init; } = [];
}
```

**Design decision — `string` dates instead of `DateOnly`:** `System.Text.Json` in .NET 8 supports `DateOnly` deserialization, but using `string` avoids serialization edge cases and keeps the JSON schema obvious. The `Dashboard.razor` `@code` block parses to `DateOnly` in the `DateToX()` helper.

### Entity: `TimelineTrack`

| Property | Type | JSON Key | Description |
|----------|------|----------|-------------|
| `Id` | `string` | `id` | Track identifier (e.g., "M1") |
| `Name` | `string` | `name` | Display name (e.g., "M1") |
| `Description` | `string` | `description` | Track description (e.g., "Core API & Auth") |
| `Color` | `string` | `color` | CSS hex color for the track line and markers (e.g., "#0078D4") |
| `Milestones` | `List<Milestone>` | `milestones` | Ordered list of milestones on this track |

```csharp
public record TimelineTrack
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Color { get; init; } = "#999";
    public List<Milestone> Milestones { get; init; } = [];
}
```

### Entity: `Milestone`

| Property | Type | JSON Key | Description |
|----------|------|----------|-------------|
| `Label` | `string` | `label` | Text displayed near the marker (e.g., "Mar 26 PoC") |
| `Date` | `string` | `date` | ISO date string for X-position calculation |
| `Type` | `string` | `type` | One of: `"checkpoint"`, `"minor"`, `"poc"`, `"production"` |
| `LabelPosition` | `string?` | `labelPosition` | Optional: `"above"` or `"below"` (default: `"above"`) |

```csharp
public record Milestone
{
    public string Label { get; init; } = "";
    public string Date { get; init; } = "";
    public string Type { get; init; } = "checkpoint";
    public string? LabelPosition { get; init; }
}
```

**Milestone type → SVG rendering map:**

| Type | SVG Element | Fill | Stroke | Size | Shadow |
|------|------------|------|--------|------|--------|
| `checkpoint` | `<circle>` | `#FFFFFF` | Track color or `#888` | r=5–7 | No |
| `minor` | `<circle>` | `#999` | None | r=4 | No |
| `poc` | `<polygon>` (diamond) | `#F4B400` | None | 11px diagonal | Yes |
| `production` | `<polygon>` (diamond) | `#34A853` | None | 11px diagonal | Yes |

### Entity: `HeatmapData`

| Property | Type | JSON Key | Description |
|----------|------|----------|-------------|
| `Columns` | `List<string>` | `columns` | Month names for column headers (e.g., `["January","February","March","April"]`) |
| `HighlightColumnIndex` | `int` | `highlightColumnIndex` | 0-based index of the "current month" column to highlight |
| `Rows` | `List<HeatmapRow>` | `rows` | Four rows: shipped, in-progress, carryover, blockers |

```csharp
public record HeatmapData
{
    public List<string> Columns { get; init; } = [];
    public int HighlightColumnIndex { get; init; }
    public List<HeatmapRow> Rows { get; init; } = [];
}
```

### Entity: `HeatmapRow`

| Property | Type | JSON Key | Description |
|----------|------|----------|-------------|
| `Category` | `string` | `category` | One of: `"shipped"`, `"in-progress"`, `"carryover"`, `"blockers"` |
| `Label` | `string` | `label` | Display label for the row header (e.g., "Shipped") |
| `CellItems` | `List<List<string>>` | `cellItems` | Items per column; outer list = columns, inner list = item strings |

```csharp
public record HeatmapRow
{
    public string Category { get; init; } = "";
    public string Label { get; init; } = "";
    public List<List<string>> CellItems { get; init; } = [];
}
```

### Category → CSS Class Mapping

The `Category` string maps to CSS class prefixes used in `Dashboard.razor.css`:

| `category` value | Row header class | Cell class | Highlight cell class | Bullet color |
|------------------|-----------------|------------|---------------------|-------------|
| `shipped` | `ship-hdr` | `ship-cell` | `ship-cell highlight` | `#34A853` |
| `in-progress` | `prog-hdr` | `prog-cell` | `prog-cell highlight` | `#0078D4` |
| `carryover` | `carry-hdr` | `carry-cell` | `carry-cell highlight` | `#F4B400` |
| `blockers` | `block-hdr` | `block-cell` | `block-cell highlight` | `#EA4335` |

### Entity Relationship Diagram

```
DashboardReport
├── HeaderInfo (1:1)
├── TimelineTrack[] (1:N, typically 3)
│   └── Milestone[] (1:N per track, typically 2-6)
└── HeatmapData (1:1)
    └── HeatmapRow[] (1:N, exactly 4)
        └── CellItems: string[][] (1:N columns × M items)
```

### Storage

**No database.** The single `data.json` flat file in `Data/` is the complete persistence layer. The file is read on every HTTP request (scoped service lifetime) to support edit-and-refresh workflows.

### Canonical `data.json` Schema

```json
{
  "header": {
    "title": "string — project title",
    "subtitle": "string — org context line",
    "backlogLink": "string — URL",
    "reportDate": "string — ISO date (YYYY-MM-DD)",
    "timelineStartDate": "string — ISO date",
    "timelineEndDate": "string — ISO date",
    "timelineMonths": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"]
  },
  "timelineTracks": [
    {
      "id": "string",
      "name": "string",
      "description": "string",
      "color": "string — CSS hex color",
      "milestones": [
        {
          "label": "string",
          "date": "string — ISO date",
          "type": "checkpoint | minor | poc | production",
          "labelPosition": "above | below (optional, default: above)"
        }
      ]
    }
  ],
  "heatmap": {
    "columns": ["string — month names"],
    "highlightColumnIndex": 0,
    "rows": [
      {
        "category": "shipped | in-progress | carryover | blockers",
        "label": "string — display label",
        "cellItems": [
          ["string — item text per cell"],
          []
        ]
      }
    ]
  }
}
```

---

## API Contracts

### HTTP Endpoints

This application has **no REST API, no GraphQL, and no custom HTTP endpoints**. The only HTTP contract is the Blazor Static SSR page rendering:

| Method | Path | Response | Content-Type |
|--------|------|----------|-------------|
| `GET` | `/` | Full HTML page (dashboard) | `text/html` |
| `GET` | `/_framework/blazor.web.js` | Blazor Static SSR runtime | `application/javascript` |
| `GET` | `/css/app.css` | Global stylesheet | `text/css` |
| `GET` | `/{project}.styles.css` | Scoped CSS bundle | `text/css` |

### Internal Service Contract

**`DashboardDataService.GetDataAsync()`**

```csharp
public async Task<DashboardReport> GetDataAsync()
```

| Scenario | Behavior |
|----------|----------|
| `data.json` exists and is valid | Returns `DashboardReport` instance |
| `data.json` does not exist | Throws `FileNotFoundException` with full path |
| `data.json` contains invalid JSON | Throws `JsonException` with line/position detail |
| `data.json` deserializes to `null` | Throws `InvalidOperationException` |

### Error Rendering Contract

When `Dashboard.razor` catches an exception from `GetDataAsync()`, it renders a centered error panel instead of the dashboard:

```html
<div class="error-container">
  <h2>Unable to load dashboard data</h2>
  <p>Please check that data.json exists and contains valid JSON.</p>
  <p class="error-detail">{exception.Message}</p>
</div>
```

The error container is styled with centered text on a white background — no Blazor error boundaries, no stack traces exposed.

---

## Infrastructure Requirements

### Hosting

| Requirement | Specification |
|-------------|--------------|
| **Runtime** | .NET 8 SDK (for `dotnet run`) or .NET 8 Runtime (for published app) |
| **Web server** | Kestrel (built into ASP.NET Core) |
| **Binding** | `127.0.0.1:5000` (localhost only) |
| **Protocol** | HTTP only (no HTTPS, no TLS certificates) |
| **OS** | Windows 10/11 (primary); macOS/Linux compatible with font fallback |

### Networking

| Requirement | Specification |
|-------------|--------------|
| **External network** | None — no outbound HTTP calls, no CDN, no telemetry |
| **Inbound access** | Localhost only — Kestrel bound to `127.0.0.1`, not `0.0.0.0` |
| **Ports** | `5000` (configurable via `launchSettings.json` or `--urls` flag) |
| **Firewall** | No rules needed — traffic stays on loopback |

### Storage

| Requirement | Specification |
|-------------|--------------|
| **Data file** | `Data/data.json` — flat JSON file, typically 2-10KB |
| **Disk space** | < 50MB for the entire published application |
| **Database** | None |

### CI/CD

**Not required for MVP.** This is a local-only tool. However, the `.sln` structure supports future CI if desired:

```
dotnet build ReportingDashboard.sln
dotnet publish ReportingDashboard/ReportingDashboard.csproj -c Release -r win-x64 --self-contained
```

The self-contained publish produces a standalone executable requiring no .NET SDK on the target machine.

### Project Structure

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj          # 1. Project file (net8.0)
    ├── Program.cs                          # 2. Host configuration
    ├── Properties/
    │   └── launchSettings.json            #    Dev server settings
    ├── Data/
    │   ├── DashboardData.cs               # 3. C# data model records
    │   ├── DashboardDataService.cs        # 4. JSON file reader service
    │   ├── data.json                      #    (gitignored) Real report data
    │   └── data.example.json              # 5. Sample data (committed)
    ├── Components/
    │   ├── App.razor                      # 6. Root HTML document shell
    │   ├── _Imports.razor                 # 7. Global using directives
    │   ├── Pages/
    │   │   ├── Dashboard.razor            # 8. The single dashboard page
    │   │   └── Dashboard.razor.css        # 9. Scoped CSS (all design styles)
    │   └── Layout/
    │       ├── MainLayout.razor           # 10. Minimal layout (just @Body)
    │       └── MainLayout.razor.css       #     Layout CSS
    └── wwwroot/
        └── css/
            └── app.css                    #     Global resets
```

**File count: 10 application files** (excluding `bin/`, `obj/`, `Properties/`, `.json` configs). Meets the ≤ 10 file constraint.

---

## Technology Stack Decisions

| Layer | Choice | Justification |
|-------|--------|---------------|
| **Framework** | .NET 8 LTS with Blazor Server (Static SSR mode) | Mandatory stack. Static SSR eliminates SignalR overhead and reconnection UI — ideal for a read-only screenshot page. LTS support through Nov 2026. |
| **Render mode** | Static SSR (no `@rendermode InteractiveServer`) | The dashboard has zero interactivity. Static SSR sends complete HTML in one response with no WebSocket. Eliminates the Blazor reconnection banner problem entirely. |
| **JSON parsing** | `System.Text.Json` (built-in) | Ships with .NET 8. Zero additional NuGet packages. `PropertyNameCaseInsensitive = true` handles camelCase JSON to PascalCase C# mapping. |
| **CSS strategy** | Scoped CSS (`.razor.css` files) | Built into Blazor. Component-isolated styles prevent leaking. All design styles ported from `OriginalDesignConcept.html` into `Dashboard.razor.css`. |
| **Layout engine** | CSS Grid (heatmap) + Flexbox (header, timeline, page layout) | Direct match to the design reference HTML. No CSS framework needed for a fixed-width layout. |
| **Timeline rendering** | Inline `<svg>` in Razor markup | The design uses ~30 SVG elements (lines, circles, polygons, text). Raw SVG with `@foreach` loops is simpler and more pixel-accurate than any charting library. |
| **Service lifetime** | Scoped (`AddScoped<DashboardDataService>`) | Re-reads `data.json` on every request, supporting the edit-and-F5 workflow. Cost is negligible — reading a 10KB file takes < 1ms. |
| **Configuration** | `appsettings.json` via `IConfiguration` | Standard ASP.NET Core pattern. Stores the `data.json` file path. No custom config providers. |
| **Font** | Segoe UI (system font on Windows) | No web font loading. Fallback: `-apple-system, Arial, sans-serif` for non-Windows. |
| **External NuGet packages** | Zero | Everything needed ships with the Blazor Server template. |

### Rejected Alternatives

| Alternative | Why Rejected |
|-------------|-------------|
| Blazor Interactive Server mode | Adds SignalR WebSocket, reconnection banner, and connection state — unnecessary for a read-only page |
| Blazor WebAssembly | Requires downloading .NET runtime to browser; overkill for local-only, adds load time |
| MVC / Razor Pages (non-Blazor) | Could work but loses scoped CSS and component model; Blazor is the mandated stack |
| Chart.js / D3.js via JS interop | The design has ~30 SVG elements; a charting library adds complexity and makes pixel-matching harder |
| Bootstrap / Tailwind CSS | The design is fixed 1920×1080 with ~100 CSS rules; a framework adds bloat with no benefit |
| Entity Framework / SQLite | No database is needed; `data.json` is the entire data layer |
| Singleton service with caching | Would require cache invalidation logic; scoped service re-reads the file every request, which is simpler and meets performance targets |

---

## Security Considerations

### Network Security

| Control | Implementation |
|---------|---------------|
| **Localhost binding** | `builder.WebHost.ConfigureKestrel(o => o.ListenLocalhost(5000))` — binds to `127.0.0.1` only; not accessible from LAN or internet |
| **No HTTPS** | Intentional — no TLS certificates to manage; traffic never leaves the loopback interface |
| **No external calls** | No CDN links, no analytics, no telemetry, no external API calls at runtime |

### Authentication & Authorization

**None.** Intentionally omitted per requirements. The application is accessible only to the user running it on their local machine. The OS login is the only access control.

### Data Protection

| Concern | Mitigation |
|---------|-----------|
| **Sensitive project data in `data.json`** | Add `Data/data.json` to `.gitignore`; commit only `data.example.json` with fictional data |
| **Secrets in source control** | No secrets exist — no API keys, no connection strings, no tokens |
| **File system access** | The service reads only the configured `data.json` path; no user-controlled file paths |

### Input Validation

| Vector | Mitigation |
|--------|-----------|
| **Malformed JSON** | `JsonSerializer.Deserialize` throws `JsonException` with line/position; caught and displayed as user-friendly error |
| **Missing required fields** | C# record defaults (`= ""`, `= []`) prevent null reference exceptions; empty strings render as empty UI elements |
| **XSS via data.json** | Blazor's Razor rendering automatically HTML-encodes all `@variable` output; raw HTML injection is not possible through data binding |
| **Path traversal** | `DashboardDataPath` is read from `appsettings.json` (not user input); no URL parameter controls file paths |

### Blazor Framework Security

| Concern | Mitigation |
|---------|-----------|
| **Antiforgery** | `app.UseAntiforgery()` is included (required by Blazor in .NET 8) even though there are no forms |
| **Error exposure** | `#blazor-error-ui` hidden via CSS; custom error rendering shows file path and JSON parse errors (acceptable for local-only use) |
| **SignalR attack surface** | Eliminated by using Static SSR — no WebSocket connection exists |

---

## Scaling Strategy

### Current Scale (MVP)

This application is designed for **single-user, local-only use**. It does not need to scale.

| Dimension | Current Capacity | Scaling Approach |
|-----------|-----------------|-----------------|
| **Concurrent users** | 1 (the person running `dotnet run`) | N/A — local-only tool |
| **Data volume** | Single `data.json`, typically 2-10KB | File I/O is sub-millisecond; supports files up to 100KB per NFR |
| **Timeline tracks** | Up to 10 tracks × 20 milestones (per NFR) | SVG rendering is O(n) with n < 200 elements; trivially fast |
| **Heatmap columns** | Dynamic N columns (default 4) | CSS Grid `repeat(N, 1fr)` handles any reasonable count; practical limit ~12 (monthly for a year) |
| **Page requests** | 1 request per F5 refresh | Scoped service reads file per request; Kestrel handles this instantly |

### Future Scaling Paths (Not in MVP)

| Scenario | Approach |
|----------|---------|
| **Multiple reports** | Add URL parameter `?report=filename` to load different JSON files; service resolves to `Data/{filename}.json` |
| **Team access** | Change Kestrel binding to `0.0.0.0:5000` and add reverse proxy with auth; or deploy as a container |
| **Historical data** | Add SQLite for storing monthly snapshots; `DashboardDataService` reads from DB instead of file |
| **Automated screenshots** | Add Playwright as a post-build step or CLI command |

### Performance Budget

| Metric | Budget | Expected Actual |
|--------|--------|----------------|
| **Time to first byte** | < 200ms | ~50ms (file read + render) |
| **Full page render** | < 1 second | ~100ms on localhost |
| **JSON parse time** | < 50ms | < 5ms for 10KB file |
| **Memory (RSS)** | < 100MB | ~40MB for bare Blazor Server |
| **Disk** | < 50MB published | ~15MB self-contained publish |

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|-----------|
| 1 | **CSS fidelity to design reference** — The dashboard screenshot doesn't match `OriginalDesignConcept.png` pixel-for-pixel | Medium | High (executives notice visual differences) | Port CSS directly from the HTML design file. Develop with browser DevTools open. Compare screenshots side-by-side at every milestone. Use Chrome DevTools device mode at exactly 1920×1080. |
| 2 | **`data.json` schema errors** — Users edit JSON by hand and introduce typos, missing commas, or wrong field names | Medium | Medium (dashboard shows error instead of data) | Catch `JsonException` in service and render a user-friendly error message including the line number and character position. Provide `data.example.json` as a copy-paste template. |
| 3 | **Segoe UI unavailable on non-Windows** — macOS/Linux users see fallback font with different metrics | Low | Medium (layout shifts break 1920×1080 fit) | Font stack includes `-apple-system, Arial, sans-serif` as fallback. Document Windows as the primary OS. Font metric differences are unlikely to break layout at this scale. |
| 4 | **SVG rendering differences across browsers** — Chrome and Edge render SVG slightly differently | Very Low | Low (both use Chromium/Blink) | Standardize on Edge for internal Microsoft use. Both browsers use the same rendering engine. Test once in both to confirm. |
| 5 | **Blazor Static SSR limitations** — Some Blazor features (e.g., `@onclick`, `@bind`) don't work in Static SSR | Low | Low (dashboard has no interactivity) | The dashboard is read-only. The only interactive element is the ADO backlog `<a href>` link, which is a standard HTML anchor and works without Blazor interactivity. |
| 6 | **Scoped CSS `::deep` limitations** — Blazor scoped CSS may not style child elements inside `@foreach` rendered markup as expected | Medium | Medium (styles don't apply) | Use explicit CSS class names on all elements rather than relying on `::deep` combinators. Test scoped CSS isolation during development. Fall back to `app.css` for any styles that don't scope correctly. |
| 7 | **Large `data.json` causes layout overflow** — Too many items in a heatmap cell push content beyond the cell boundary | Low | Medium (content clipped or overflows at 1920×1080) | Cells use `overflow: hidden` per the design spec. Document recommended item limits (≤ 8 items per cell). The JSON author controls content density. |
| 8 | **`DateOnly` parsing fails for non-ISO date strings** — Users enter dates like "April 17, 2026" instead of "2026-04-17" | Low | Medium (timeline renders incorrectly or crashes) | Document ISO 8601 format requirement in `data.example.json` comments. Use `DateOnly.TryParse` with fallback and error messaging. |

### Trade-off Decisions (Accepted)

| Trade-off | Accepted Risk | Benefit |
|-----------|--------------|---------|
| No database | No historical tracking, no trend analysis | Zero setup, zero dependencies, instant portability |
| No authentication | Anyone on the machine can view the page | No login friction; local-only access is sufficient |
| Fixed 1920×1080 | Not usable on smaller screens | Guaranteed screenshot fidelity |
| Scoped service (reads file every request) | Slightly more I/O than singleton with caching | Always shows latest data; no cache invalidation bugs |
| Static SSR (no interactivity) | Cannot add click handlers or dynamic UI without switching render mode | No SignalR connection, no reconnection banner, lighter page |
| No unit tests in MVP | Defects caught later | Faster initial delivery; ≤ 500 LOC is manually verifiable |

---

## UI Component Architecture

The following maps each visual section from `OriginalDesignConcept.html` to a specific implementation element within `Dashboard.razor` and `Dashboard.razor.css`.

### Section Map: Design → Component

| Design Section | CSS Class in Design | Implementation Element | Layout Strategy |
|---------------|--------------------|-----------------------|----------------|
| **Page shell** | `body` | `MainLayout.razor` → `<main>` | Flexbox column, fixed 1920×1080, `overflow: hidden` |
| **Header bar** | `.hdr` | `Dashboard.razor` → `<div class="hdr">` | Flexbox row, `justify-content: space-between`, padding `12px 44px 10px` |
| **Title + link** | `.hdr h1`, `.sub` | `<h1>@Data.Header.Title <a href="@Data.Header.BacklogLink">→ ADO Backlog</a></h1>` | Block flow, `<h1>` is 24px bold, subtitle is 12px `#888` |
| **Legend** | Inline styles in design | `<div class="legend">` with four `<span>` items | Flexbox row, `gap: 22px`, each item has inline shape + 12px label |
| **Legend: PoC diamond** | Inline `transform:rotate(45deg)` | `<span class="legend-diamond poc"></span>` | 12×12px square rotated 45°, `#F4B400` |
| **Legend: Production diamond** | Inline `transform:rotate(45deg)` | `<span class="legend-diamond prod"></span>` | 12×12px square rotated 45°, `#34A853` |
| **Legend: Checkpoint circle** | Inline `border-radius:50%` | `<span class="legend-circle"></span>` | 8×8px circle, `#999` |
| **Legend: Now line** | Inline styles | `<span class="legend-now-bar"></span>` | 2×14px vertical bar, `#EA4335` |
| **Timeline area** | `.tl-area` | `<div class="tl-area">` | Flexbox row, height `196px`, bg `#FAFAFA`, border-bottom `2px solid #E8E8E8` |
| **Track label panel** | Inline styles (230px div) | `<div class="tl-labels">` with `@foreach (track in Data.TimelineTracks)` | Flexbox column, 230px fixed width, `justify-content: space-around` |
| **Track label item** | Inline styles | `<div><span style="color:@track.Color">@track.Name</span><br/><span>@track.Description</span></div>` | 12px, font-weight 600, track ID in track color, description in `#444` |
| **SVG timeline** | `.tl-svg-box` | `<div class="tl-svg-box"><svg width="1560" height="185">` | `flex: 1`, padding-left 12px, padding-top 6px |
| **Month gridlines** | SVG `<line>` elements | `@foreach` over `TimelineMonths`, X = index × (1560 / monthCount) | Vertical lines, stroke `#BBB`, opacity 0.4 |
| **Month labels** | SVG `<text>` elements | Positioned 5px right of gridline, y=14, 11px bold `#666` | `font-family: 'Segoe UI', Arial` |
| **NOW line** | SVG `<line>` + `<text>` | `DateToX(reportDate)` calculates X position | Dashed red line (`#EA4335`, dasharray `5,3`), "NOW" label 10px bold |
| **Track lines** | SVG `<line>` elements | `@foreach` with `TrackY(index)` = 42 + (index × 56) for 3 tracks | Horizontal full-width, stroke = `track.Color`, stroke-width 3 |
| **Checkpoint markers** | SVG `<circle>` | `@if (ms.Type == "checkpoint")` → `<circle r="6" fill="white" stroke="@track.Color" stroke-width="2.5" />` | Open circle at `(DateToX(ms.Date), TrackY(i))` |
| **Minor markers** | SVG `<circle>` | `@if (ms.Type == "minor")` → `<circle r="4" fill="#999" />` | Small filled gray circle |
| **PoC diamond** | SVG `<polygon>` | `@if (ms.Type == "poc")` → `<polygon points="@DiamondPoints(x, y)" fill="#F4B400" filter="url(#sh)" />` | Diamond shape with drop shadow |
| **Production diamond** | SVG `<polygon>` | `@if (ms.Type == "production")` → `<polygon points="@DiamondPoints(x, y)" fill="#34A853" filter="url(#sh)" />` | Diamond shape with drop shadow |
| **Milestone labels** | SVG `<text>` | `<text x="@x" y="@(labelAbove ? y-16 : y+24)" text-anchor="middle">@ms.Label</text>` | 10px `#666`, positioned above or below track |
| **Drop shadow filter** | SVG `<defs><filter>` | `<filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter>` | Applied to diamond markers via `filter="url(#sh)"` |
| **Heatmap wrapper** | `.hm-wrap` | `<div class="hm-wrap">` | Flexbox column, `flex: 1`, padding `10px 44px` |
| **Heatmap title** | `.hm-title` | `<div class="hm-title">MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS</div>` | 14px bold, `#888`, uppercase, letter-spacing 0.5px |
| **Heatmap grid** | `.hm-grid` | `<div class="hm-grid" style="grid-template-columns: 160px repeat(@N, 1fr)">` | CSS Grid, dynamic column count from `Data.Heatmap.Columns.Count` |
| **Corner cell** | `.hm-corner` | `<div class="hm-corner">MONTH</div>` | 11px bold uppercase `#999`, bg `#F5F5F5` |
| **Column headers** | `.hm-col-hdr` | `@for (i = 0; i < columns.Count; i++)` → apply `.highlight` class if `i == highlightIndex` | 16px bold, centered; highlight: bg `#FFF0D0`, color `#C07700` |
| **Row headers** | `.hm-row-hdr` + category class | `<div class="hm-row-hdr @CategoryHeaderClass(row.Category)">@row.Label</div>` | 11px bold uppercase, category-colored bg and text |
| **Data cells** | `.hm-cell` + category class | `@foreach` over `CellItems[colIndex]` → `<div class="it">@item</div>` or dash if empty | 12px `#333`, `::before` pseudo-element for colored bullet |
| **Empty cell** | `.hm-cell` with dash | `@if (items.Count == 0) { <span class="empty">—</span> }` | Gray dash `#AAA`, maintaining grid structure |
| **Error state** | N/A in design | `<div class="error-container">` (replaces entire dashboard when `data.json` fails) | Centered on white background, no dashboard rendered |

### SVG Helper Methods (in `@code` block)

```csharp
// Linear interpolation: date → pixel X within the 1560px SVG
private double DateToX(string dateStr)
{
    var date = DateOnly.Parse(dateStr);
    var start = DateOnly.Parse(Data.Header.TimelineStartDate);
    var end = DateOnly.Parse(Data.Header.TimelineEndDate);
    var totalDays = end.DayNumber - start.DayNumber;
    var elapsed = date.DayNumber - start.DayNumber;
    return (elapsed / (double)totalDays) * 1560.0;
}

// Y position for track at given 0-based index (3 tracks: y=42, y=98, y=154)
private double TrackY(int index)
{
    return 42 + (index * 56);
}

// Diamond polygon points centered at (cx, cy) with 11px radius
private string DiamondPoints(double cx, double cy)
{
    var r = 11;
    return $"{cx},{cy - r} {cx + r},{cy} {cx},{cy + r} {cx - r},{cy}";
}

// Map category string to CSS class prefix
private string CategoryCellClass(string category) => category switch
{
    "shipped" => "ship-cell",
    "in-progress" => "prog-cell",
    "carryover" => "carry-cell",
    "blockers" => "block-cell",
    _ => ""
};

private string CategoryHeaderClass(string category) => category switch
{
    "shipped" => "ship-hdr",
    "in-progress" => "prog-hdr",
    "carryover" => "carry-hdr",
    "blockers" => "block-hdr",
    _ => ""
};
```

### CSS Architecture in `Dashboard.razor.css`

All styles are ported directly from the `<style>` block in `OriginalDesignConcept.html`. The scoped CSS file contains approximately 80-100 rules organized by section:

1. **Header styles** (`.hdr`, `.sub`, `.legend`, `.legend-diamond`, `.legend-circle`, `.legend-now-bar`)
2. **Timeline styles** (`.tl-area`, `.tl-labels`, `.tl-svg-box`)
3. **Heatmap structure** (`.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`, `.it`, `.empty`)
4. **Category color overrides** (`.ship-hdr`, `.ship-cell`, `.prog-hdr`, `.prog-cell`, `.carry-hdr`, `.carry-cell`, `.block-hdr`, `.block-cell`)
5. **Highlight state** (`.highlight` modifier class for current-month column cells)
6. **Error state** (`.error-container`)

The highlight column uses a `.highlight` CSS class applied conditionally:
```razor
<div class="@CategoryCellClass(row.Category) @(colIndex == Data.Heatmap.HighlightColumnIndex ? "highlight" : "")">
```