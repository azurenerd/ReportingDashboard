# Architecture

## Overview & Goals

This document defines the system architecture for the **Executive Reporting Dashboard**, a single-page Blazor Server application that renders project milestones, shipping status, and monthly execution health into a fixed 1920×1080 layout optimized for PowerPoint screenshot capture.

### Primary Goals

1. **Pixel-perfect visual fidelity** — match `OriginalDesignConcept.html` exactly, producing a screenshot-ready dashboard with no scrollbars, no overflow, and no browser chrome artifacts.
2. **JSON-driven content** — all display data sourced from a single `wwwroot/data.json` file editable by non-developers.
3. **Zero infrastructure** — no cloud services, no database, no authentication, no third-party NuGet packages. Runs locally via `dotnet run`.
4. **Minimal file count** — ~6 core application files plus standard Blazor scaffolding.
5. **Sub-2-second render** — full dashboard visible within 2 seconds of navigation on localhost.

### Architectural Style

File-read-and-render. The application follows a trivial three-layer flow:

```
data.json → DashboardDataService → Dashboard.razor → Browser
```

There is no API layer, no persistence layer, no background processing, and no multi-user coordination. This is intentional — the architecture matches the problem's inherent simplicity.

---

## System Components

### 1. `Program.cs` — Application Host

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Configure Kestrel, register services, set up Blazor Server middleware |
| **Interfaces** | None exposed; configures the DI container and HTTP pipeline |
| **Dependencies** | `Microsoft.AspNetCore.Builder`, `DashboardDataService` |
| **Data** | Reads `appsettings.json` for optional configuration (e.g., data file path, port) |

**Key behaviors:**
- Registers `DashboardDataService` as a **Scoped** service (re-reads `data.json` on each circuit/page load so browser refresh picks up file changes).
- Configures Kestrel to listen on `http://localhost:5000`.
- Maps Blazor Server hub (`_blazor`) and fallback to `App.razor`.
- Disables HTTPS redirection (localhost-only, no TLS needed).

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<DashboardDataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
```

### 2. `DashboardData.cs` — Data Models (POCOs)

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Define the strongly-typed shape of `data.json` for deserialization |
| **Interfaces** | Pure data records; no behavior |
| **Dependencies** | None (System.Text.Json attributes only) |
| **Data** | Mirrors the `data.json` schema exactly |

**Records defined:**

| Record | Purpose | Key Properties |
|--------|---------|----------------|
| `DashboardConfig` | Root object | `ProjectName`, `Workstream`, `ReportMonth`, `BacklogUrl`, `Timeline`, `Heatmap` |
| `TimelineConfig` | Timeline date range and milestones | `StartDate`, `EndDate`, `NowDate`, `Milestones` |
| `Milestone` | A single milestone swim lane | `Id`, `Label`, `Color`, `Events` |
| `TimelineEvent` | A marker on a milestone lane | `Date`, `Label`, `Type` (checkpoint/poc/production) |
| `HeatmapConfig` | Heatmap grid structure | `Columns`, `CurrentColumn`, `Shipped`, `InProgress`, `Carryover`, `Blockers` |

**Design decisions:**
- All properties use `{ get; init; }` for immutability after deserialization.
- All reference-type properties are **nullable** (`string?`, `List<T>?`, `Dictionary<K,V>?`) to support graceful degradation when fields are missing from JSON.
- `TimelineEvent.Type` is a `string` (not an enum) to avoid deserialization failures on unexpected values; validation happens at render time.
- `Date` fields are `string` in the model (ISO 8601 format), parsed to `DateOnly` in the service layer to keep models pure.

```csharp
public record DashboardConfig
{
    public string? ProjectName { get; init; }
    public string? Workstream { get; init; }
    public string? ReportMonth { get; init; }
    public string? BacklogUrl { get; init; }
    public TimelineConfig? Timeline { get; init; }
    public HeatmapConfig? Heatmap { get; init; }
}

public record TimelineConfig
{
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public string? NowDate { get; init; }
    public List<Milestone>? Milestones { get; init; }
}

public record Milestone
{
    public string? Id { get; init; }
    public string? Label { get; init; }
    public string? Color { get; init; }
    public List<TimelineEvent>? Events { get; init; }
}

public record TimelineEvent
{
    public string? Date { get; init; }
    public string? Label { get; init; }
    public string? Type { get; init; }
}

public record HeatmapConfig
{
    public List<string>? Columns { get; init; }
    public string? CurrentColumn { get; init; }
    public Dictionary<string, List<string>>? Shipped { get; init; }
    public Dictionary<string, List<string>>? InProgress { get; init; }
    public Dictionary<string, List<string>>? Carryover { get; init; }
    public Dictionary<string, List<string>>? Blockers { get; init; }
}
```

### 3. `DashboardDataService.cs` — JSON Reader Service

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Read `data.json` from disk, deserialize to `DashboardConfig`, handle errors gracefully |
| **Interfaces** | `LoadAsync()` → returns `DashboardConfig` or throws `DashboardDataException` |
| **Dependencies** | `System.Text.Json`, `System.IO`, `IWebHostEnvironment`, `ILogger<DashboardDataService>` |
| **Data** | Reads from `wwwroot/data.json` (path configurable via `IConfiguration`) |

**Key behaviors:**
- Registered as **Scoped** — each page load re-reads the file, ensuring browser refresh picks up edits without app restart.
- Uses `IWebHostEnvironment.WebRootPath` to resolve the absolute path to `data.json`.
- Configures `JsonSerializerOptions` with `PropertyNameCaseInsensitive = true` and `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`.
- Wraps all I/O and deserialization in try/catch; surfaces a `DashboardDataException` with a user-friendly message (file not found, parse error) that the Razor page can display.
- Provides a helper method `GetXPosition(DateOnly date, DateOnly start, DateOnly end, double totalWidth)` for timeline coordinate math, keeping this calculation testable and out of the Razor file.
- Provides a helper method `GetMilestoneLaneY(int index, int totalMilestones, double totalHeight)` returning the Y coordinate for each swim lane.

```csharp
public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<DashboardConfig> LoadAsync()
    {
        var path = Path.Combine(_env.WebRootPath, "data.json");
        if (!File.Exists(path))
            throw new DashboardDataException($"Data file not found: {path}");

        try
        {
            var json = await File.ReadAllTextAsync(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Deserialize<DashboardConfig>(json, options)
                ?? throw new DashboardDataException("data.json deserialized to null.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json");
            throw new DashboardDataException(
                $"Invalid JSON in data.json: {ex.Message}. Check for syntax errors.");
        }
    }

    public static double GetXPosition(DateOnly date, DateOnly start, DateOnly end, double totalWidth)
    {
        if (end == start) return 0;
        var fraction = (double)(date.DayNumber - start.DayNumber)
                     / (end.DayNumber - start.DayNumber);
        return Math.Clamp(fraction * totalWidth, 0, totalWidth);
    }

    public static double GetMilestoneLaneY(int index, int totalMilestones, double svgHeight)
    {
        if (totalMilestones <= 0) return svgHeight / 2;
        var spacing = svgHeight / (totalMilestones + 1);
        return spacing * (index + 1);
    }
}

public class DashboardDataException : Exception
{
    public DashboardDataException(string message) : base(message) { }
    public DashboardDataException(string message, Exception inner) : base(message, inner) { }
}
```

### 4. `Dashboard.razor` — The Single Page Component

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Render the complete dashboard UI: header, SVG timeline, CSS Grid heatmap |
| **Interfaces** | Blazor component; route `/` |
| **Dependencies** | `DashboardDataService` (injected), `DashboardConfig` model |
| **Data** | Holds deserialized `DashboardConfig` in component state; error string for fault display |

**Key behaviors:**
- `@page "/"` — the only routable page in the application.
- In `OnInitializedAsync`, calls `DashboardDataService.LoadAsync()` and stores the result. On exception, captures the error message string for display.
- **Header section**: Renders project name, backlog link, subtitle, and legend from `DashboardConfig` properties with null-coalescing defaults.
- **Timeline section**: Renders an inline `<svg>` element (width=1560, height=185) containing:
  - Month grid lines via `@for` loop over the date range
  - Milestone swim lanes via `@foreach` over `config.Timeline.Milestones`
  - Event markers (checkpoint circles, PoC diamonds, production diamonds) via `@foreach` over each milestone's events
  - NOW dashed line calculated from `config.Timeline.NowDate`
- **Heatmap section**: Renders a CSS Grid with `grid-template-columns: 160px repeat(N, 1fr)` where N = number of columns from `config.Heatmap.Columns`. Iterates over four status categories (Shipped, InProgress, Carryover, Blockers), applying row-specific CSS classes.
- **Error state**: If `DashboardDataException` is caught, renders a centered error message on a light red background instead of the dashboard.
- **No JavaScript interop**: All rendering is pure server-side Razor markup and CSS.

**Component state:**

```csharp
@code {
    private DashboardConfig? config;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            config = await DataService.LoadAsync();
        }
        catch (DashboardDataException ex)
        {
            errorMessage = ex.Message;
        }
    }

    private double GetX(string? dateStr) { /* delegate to DashboardDataService.GetXPosition */ }
    private double GetLaneY(int index) { /* delegate to DashboardDataService.GetMilestoneLaneY */ }
    private string GetDiamondPoints(double cx, double cy, double size = 11) { /* SVG polygon points */ }
    private List<string> GetCellItems(Dictionary<string, List<string>>? dict, string column) { /* null-safe lookup */ }
    private bool IsCurrentColumn(string column) => column == config?.Heatmap?.CurrentColumn;
}
```

### 5. `Dashboard.razor.css` — Scoped Styles

| Attribute | Value |
|-----------|-------|
| **Responsibility** | All visual styling for the dashboard, ported from `OriginalDesignConcept.html` |
| **Interfaces** | Blazor CSS isolation (auto-scoped to `Dashboard.razor` via `b-{hash}` attributes) |
| **Dependencies** | None |
| **Data** | N/A |

**Key behaviors:**
- Contains the complete CSS from the original HTML design, adapted for Blazor's CSS isolation (replacing `body` selectors with a wrapper `div`, since Blazor components cannot style `<body>` directly).
- Uses `::deep` combinator sparingly — only for the SVG text elements where Blazor's scoping attribute doesn't penetrate into raw SVG.
- Defines CSS custom properties (`:root` equivalent via the scoped wrapper) for the color palette.
- The page wrapper div has `width: 1920px; height: 1080px; overflow: hidden` to enforce the fixed viewport.

### 6. `wwwroot/data.json` — Dashboard Data File

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Single source of truth for all dashboard display content |
| **Interfaces** | JSON file conforming to the `DashboardConfig` schema |
| **Dependencies** | None |
| **Data** | Project metadata, timeline milestones/events, heatmap items by month and category |

**Schema contract:** See the `DashboardConfig` record hierarchy in Component 2 above. The JSON uses camelCase property names.

### 7. `App.razor` — Root Component

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Blazor root component; provides `<head>`, CSS references, and the `<Routes>` component |
| **Interfaces** | Standard Blazor `App.razor` |
| **Dependencies** | `Routes.razor` |

**Key behaviors:**
- References `wwwroot/css/app.css` for global resets (margin/padding/box-sizing only).
- Sets `<meta charset="UTF-8">` and viewport meta tag.
- Includes `_framework/blazor.web.js` for the SignalR circuit.

### 8. `wwwroot/css/app.css` — Global Stylesheet

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Minimal global CSS resets that apply outside Blazor component scope |
| **Dependencies** | None |

**Contents (intentionally minimal):**
```css
*, *::before, *::after { margin: 0; padding: 0; box-sizing: border-box; }
body { font-family: 'Segoe UI', Arial, sans-serif; color: #111; background: #FFFFFF; }
a { color: #0078D4; text-decoration: none; }
```

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────┐
│  Browser (Edge/Chrome)                                   │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  Rendered HTML (1920×1080 fixed layout)              │ │
│  │  ┌──────────────┐ ┌──────────┐ ┌─────────────────┐ │ │
│  │  │ Header Bar   │ │ Timeline │ │ Heatmap Grid    │ │ │
│  │  │ (metadata +  │ │ (SVG)    │ │ (CSS Grid)      │ │ │
│  │  │  legend)     │ │          │ │                 │ │ │
│  │  └──────────────┘ └──────────┘ └─────────────────┘ │ │
│  └────────────────────────┬────────────────────────────┘ │
│                           │ SignalR (Blazor Server)       │
└───────────────────────────┼──────────────────────────────┘
                            │
┌───────────────────────────┼──────────────────────────────┐
│  Kestrel (localhost:5000) │                               │
│                           ▼                               │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  Dashboard.razor                                      │ │
│  │  OnInitializedAsync() ──→ DashboardDataService        │ │
│  │                            .LoadAsync()               │ │
│  │  ◄── DashboardConfig ────────────────┘                │ │
│  │                                                       │ │
│  │  Renders: Header + SVG Timeline + CSS Grid Heatmap    │ │
│  └──────────────────────────────────────────────────────┘ │
│                           │                               │
│                           ▼                               │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  DashboardDataService                                 │ │
│  │  File.ReadAllTextAsync("wwwroot/data.json")           │ │
│  │  JsonSerializer.Deserialize<DashboardConfig>(json)    │ │
│  └──────────────────────────┬───────────────────────────┘ │
│                             │                             │
│                             ▼                             │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  wwwroot/data.json                                    │ │
│  │  (flat file on local filesystem)                      │ │
│  └──────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────┘
```

### Sequence: Initial Page Load

```
Browser                    Kestrel/Blazor              DashboardDataService        Filesystem
  │                            │                            │                         │
  │── GET / ──────────────────→│                            │                         │
  │                            │── OnInitializedAsync() ───→│                         │
  │                            │                            │── ReadAllTextAsync() ───→│
  │                            │                            │◄── JSON string ─────────│
  │                            │                            │── Deserialize() ────────→│
  │                            │◄── DashboardConfig ────────│                         │
  │                            │                            │                         │
  │                            │── Render Razor markup ────→│                         │
  │◄── HTML + SignalR setup ───│                            │                         │
  │                            │                            │                         │
```

### Sequence: Data Update (Manual Refresh)

```
User                    Filesystem              Browser                 Kestrel
  │                         │                      │                      │
  │── Edit data.json ──────→│                      │                      │
  │                         │                      │                      │
  │── Press F5 ────────────────────────────────────→│                      │
  │                         │                      │── GET / ────────────→│
  │                         │                      │                      │── Read data.json ──→
  │                         │                      │                      │◄── Updated JSON ────
  │                         │                      │◄── Re-rendered HTML ─│
  │                         │                      │                      │
```

### Sequence: Malformed JSON Error

```
Browser                    Kestrel/Blazor              DashboardDataService
  │                            │                            │
  │── GET / ──────────────────→│                            │
  │                            │── LoadAsync() ────────────→│
  │                            │                            │── ReadAllTextAsync() ──→ OK
  │                            │                            │── Deserialize() ──→ JsonException!
  │                            │◄── DashboardDataException ─│
  │                            │                            │
  │                            │── Render error message ───→│
  │◄── "Invalid JSON..." ─────│                            │
```

---

## Data Model

### Entity Relationship

```
DashboardConfig
 ├── projectName: string
 ├── workstream: string
 ├── reportMonth: string
 ├── backlogUrl: string
 ├── timeline: TimelineConfig
 │    ├── startDate: string (ISO 8601)
 │    ├── endDate: string (ISO 8601)
 │    ├── nowDate: string (ISO 8601)
 │    └── milestones[]: Milestone
 │         ├── id: string
 │         ├── label: string
 │         ├── color: string (hex)
 │         └── events[]: TimelineEvent
 │              ├── date: string (ISO 8601)
 │              ├── label: string
 │              └── type: string (checkpoint|poc|production)
 └── heatmap: HeatmapConfig
      ├── columns[]: string
      ├── currentColumn: string
      ├── shipped: { [month]: string[] }
      ├── inProgress: { [month]: string[] }
      ├── carryover: { [month]: string[] }
      └── blockers: { [month]: string[] }
```

### Storage

- **Type:** Single flat JSON file (`wwwroot/data.json`)
- **Size constraint:** ≤50KB (per NFR)
- **Access pattern:** Read-only from the application; write access is manual (text editor)
- **Caching:** None required. The service is Scoped, so each page load re-reads the file. For a single-user local tool, the filesystem read (~1ms for a 50KB file) is negligible.
- **No database:** Explicitly excluded. No Entity Framework, no SQLite, no cloud storage.

### JSON Schema Validation Rules (Enforced at Render Time)

| Field | Required | Default if Missing |
|-------|----------|--------------------|
| `projectName` | No | `"Untitled Dashboard"` |
| `workstream` | No | `""` |
| `reportMonth` | No | `""` |
| `backlogUrl` | No | `"#"` (non-navigating) |
| `timeline` | No | Timeline section hidden |
| `timeline.startDate` | Yes (if timeline present) | Error displayed |
| `timeline.endDate` | Yes (if timeline present) | Error displayed |
| `timeline.nowDate` | No | NOW line not rendered |
| `timeline.milestones` | No | Empty timeline (grid lines only) |
| `heatmap` | No | Heatmap section hidden |
| `heatmap.columns` | Yes (if heatmap present) | Error displayed |
| `heatmap.currentColumn` | No | No column highlighted |
| `heatmap.shipped/inProgress/carryover/blockers` | No | Empty dictionary; cells show `"-"` |

---

## API Contracts

This application has **no REST API, no GraphQL, no gRPC, and no external service calls**. The only "contract" is between the JSON file and the data model.

### Internal Contract: `data.json` ↔ `DashboardConfig`

**File location:** `wwwroot/data.json`
**Deserialization:** `System.Text.Json` with `PropertyNameCaseInsensitive = true`

**Request equivalent (file read):**
```
File.ReadAllTextAsync("{WebRootPath}/data.json") → string
```

**Response equivalent (deserialized object):**
```csharp
JsonSerializer.Deserialize<DashboardConfig>(json, options) → DashboardConfig
```

**Error handling:**

| Error Condition | Exception Type | User-Facing Message |
|----------------|----------------|---------------------|
| File not found | `DashboardDataException` | "Data file not found: {path}. Ensure data.json exists in wwwroot/." |
| Invalid JSON syntax | `DashboardDataException` (wraps `JsonException`) | "Invalid JSON in data.json: {details}. Check for syntax errors." |
| Null deserialization | `DashboardDataException` | "data.json deserialized to null. Ensure the file contains valid JSON." |
| Missing required field | Handled in Razor with `??` defaults | Graceful degradation with placeholder content |

### Internal Contract: `DashboardDataService` ↔ `Dashboard.razor`

```csharp
// Service method signature
public async Task<DashboardConfig> LoadAsync();

// Static helper methods (pure functions, no I/O)
public static double GetXPosition(DateOnly date, DateOnly start, DateOnly end, double totalWidth);
public static double GetMilestoneLaneY(int index, int totalMilestones, double svgHeight);
```

### Blazor Server SignalR Contract (Framework-Managed)

The Blazor Server framework manages the SignalR circuit between browser and server. No custom hub methods, no custom serialization. This is entirely transparent to our application code.

- **Protocol:** WebSocket (SignalR)
- **Endpoint:** `/_blazor` (auto-configured)
- **Reconnection:** Default Blazor reconnect UI on circuit loss (acceptable for local-only use)

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|---------------|
| **.NET SDK** | .NET 8.0.x LTS (build + run) |
| **.NET Runtime** | ASP.NET Core 8.0.x (run only, for published deployments) |
| **OS** | Windows 10/11 (primary), macOS/Linux (untested but functional) |
| **Memory** | <100MB RAM at steady state |
| **Disk** | <50MB total (published output + data file) |
| **Network** | Localhost only; no outbound network required |
| **Ports** | TCP 5000 (configurable via `appsettings.json` or `ASPNETCORE_URLS`) |

### Hosting

- **Server:** Kestrel (built into ASP.NET Core), HTTP only, no reverse proxy needed.
- **Process model:** Single process, single-threaded request handling (Blazor Server with one concurrent user).
- **Startup command:** `dotnet run --project src/ReportingDashboard` or `dotnet ReportingDashboard.dll` from publish output.

### Networking

- **Binding:** `http://localhost:5000` (or `http://0.0.0.0:5000` if configurable for LAN access — not required).
- **TLS:** Not required. HTTP is sufficient for localhost.
- **Firewall:** No inbound rules needed; localhost traffic only.

### Storage

- **Application files:** Standard `dotnet publish` output (~15MB).
- **Data file:** `wwwroot/data.json` (user-managed, ≤50KB).
- **Logs:** Console stdout only; no log files persisted.
- **Temp files:** None.

### CI/CD

Not required for MVP. The build and run process is:

```bash
# Development
dotnet watch --project src/ReportingDashboard

# Production build
dotnet publish src/ReportingDashboard -c Release -o ./publish

# Run published
cd publish && dotnet ReportingDashboard.dll
```

### Development Environment

| Tool | Purpose |
|------|---------|
| Visual Studio 2022 or VS Code + C# Dev Kit | IDE |
| `dotnet watch` | Hot reload during development |
| Edge or Chrome DevTools (F12) | CSS Grid inspection, 1920px viewport verification |

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **Runtime** | .NET 8 LTS | Mandated by project requirements. Supported through Nov 2026. |
| **UI Framework** | Blazor Server | Mandated. Server-side rendering avoids WASM download overhead. SignalR circuit is irrelevant for single-user local use but harmless. |
| **Project Template** | `blazorserver-empty` (or minimal Web App template) | Avoids sample pages, Bootstrap, and other bloat from the full template. |
| **JSON Library** | `System.Text.Json` | Built into .NET 8. Faster than Newtonsoft. Zero NuGet dependency. |
| **CSS Strategy** | Blazor CSS Isolation + global resets | Component-scoped styles prevent leakage. Original design CSS ports directly. |
| **SVG Rendering** | Inline Razor markup | The timeline is simple geometry (lines, circles, diamonds, text). A charting library would add 500KB+ for zero benefit. |
| **CSS Layout** | CSS Grid + Flexbox | Matches the original design exactly. Native browser support, no preprocessor needed. |
| **Fonts** | System fonts (`Segoe UI`, `Arial`) | No web font loading. Consistent rendering on Windows. |
| **Third-party NuGet** | **None** | Explicit requirement. All functionality uses .NET 8 built-in libraries. |
| **Component Library** | **None** (no MudBlazor, Radzen, etc.) | The design is bespoke. Component libraries impose their own design language and would fight the pixel-perfect layout. |
| **Charting Library** | **None** (no Chart.js, ApexCharts, etc.) | The "chart" is a hand-drawn SVG timeline with ~20 elements. A charting library is massive overkill. |
| **JavaScript Interop** | **None** | The original design is JS-free. Blazor Server renders everything server-side. No custom JS needed. |
| **Database / ORM** | **None** | No persistent storage needed. Data comes from a flat JSON file. |
| **Authentication** | **None** | Explicitly out of scope. Single-user local tool. |

---

## Security Considerations

### Threat Model

This is a **localhost-only, single-user, read-only dashboard with no authentication, no external network access, and no user input beyond a trusted JSON file**. The attack surface is minimal.

| Threat | Risk Level | Mitigation |
|--------|-----------|------------|
| **Malicious `data.json` content** | Low | Data is authored by a trusted internal user. Blazor's Razor engine HTML-encodes all `@` expressions by default, preventing XSS from JSON string values. |
| **Network exposure** | Low | Kestrel binds to `localhost` only. No LAN or internet exposure. |
| **SignalR circuit hijacking** | Negligible | Localhost-only; no external actors can reach the SignalR endpoint. |
| **Dependency supply chain** | None | Zero third-party NuGet packages. Only Microsoft.AspNetCore framework packages. |
| **Path traversal via data file path** | Low | The data file path is hardcoded to `wwwroot/data.json` or configured in `appsettings.json` by a developer, not by user input. |
| **Denial of service** | Negligible | Single-user localhost tool. No public endpoints. |

### Data Protection

- **No PII processed or stored.** The dashboard displays project work item descriptions only.
- **No secrets in `data.json`.** The file contains display text, dates, and URLs only.
- **No HTTPS required.** Localhost HTTP traffic is not exposed to network sniffing.

### Input Validation

- **JSON schema:** Validated implicitly by `System.Text.Json` deserialization. Malformed JSON throws `JsonException`, caught and displayed as a user-friendly error.
- **Null fields:** All model properties are nullable. Razor markup uses `??` null-coalescing and `?.` null-conditional operators throughout.
- **No user-submitted input:** There are no forms, no text fields, no file upload endpoints. The only "input" is the pre-authored `data.json` file.

### Blazor-Specific Protections

- **Razor encoding:** All `@variable` expressions in Razor are automatically HTML-encoded. This prevents XSS even if `data.json` contains `<script>` tags in string values.
- **No `MarkupString` usage:** The architecture avoids `@((MarkupString)untrustedContent)` to maintain automatic encoding. SVG elements are rendered with explicit Razor markup, not injected as raw HTML strings.

---

## Scaling Strategy

### Current Scale (By Design)

This application is **intentionally non-scalable**. It serves a single user on localhost. There is no scaling strategy because scaling is explicitly out of scope.

| Dimension | Current Capacity | Scaling Path (If Ever Needed) |
|-----------|-----------------|-------------------------------|
| **Users** | 1 (single browser tab) | Blazor Server supports ~5K concurrent circuits per server with default settings. Sufficient for team-wide access if ever exposed on LAN. |
| **Data volume** | ≤50KB JSON file, ≤5 items per heatmap cell | JSON parsing is O(n); a 500KB file would still parse in <100ms. |
| **Milestones** | 2–5 swim lanes | SVG scales linearly; 10+ lanes would need vertical spacing adjustment (out of MVP scope). |
| **Heatmap columns** | 4 months (configurable) | CSS Grid `repeat(N, 1fr)` handles any column count. Beyond ~8 columns, cell width becomes too narrow for text at 1920px. |
| **Projects** | 1 (`data.json`) | Swap the file, refresh browser. No multi-project UI. If needed later, add a file picker or config parameter. |

### Performance Budget

| Metric | Target | Approach |
|--------|--------|----------|
| Page load | <2 seconds | Single file read (~1ms) + server-side Razor render (~50ms) + HTML transfer over localhost (~10ms). Well within budget. |
| JSON parse | <100ms for 50KB | `System.Text.Json` benchmarks at ~0.5ms for 50KB. 200× headroom. |
| Memory | <100MB | Blazor Server baseline is ~30MB. A single `DashboardConfig` in memory is <1KB. |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to specific Blazor component structure, CSS layout strategy, data bindings, and interactions.

> **Note:** The entire UI is rendered within a single `Dashboard.razor` component. There are no child components — the dashboard is simple enough that component decomposition would add complexity without benefit. Logical sections are separated by commented regions within the Razor file.

### Page Wrapper

| Property | Value |
|----------|-------|
| **HTML Element** | `<div class="dashboard-root">` (replaces `<body>` styling from original) |
| **CSS Layout** | `display: flex; flex-direction: column; width: 1920px; height: 1080px; overflow: hidden; background: #FFFFFF;` |
| **Data Bindings** | None (structural wrapper) |
| **Interactions** | None |

### Section 1: Header Bar → `.hdr` region

**Maps to:** Original `.hdr` div

| Property | Value |
|----------|-------|
| **HTML Element** | `<div class="hdr">` |
| **CSS Layout** | `display: flex; align-items: center; justify-content: space-between; padding: 12px 44px 10px; border-bottom: 1px solid #E0E0E0; flex-shrink: 0;` |
| **Left Side** | `<div>` containing `<h1>` (project name + backlog link) and `<div class="sub">` (subtitle) |
| **Right Side** | `<div>` with flexbox legend row (4 indicator spans) |

**Data bindings:**

| Element | Binding | Default |
|---------|---------|---------|
| Project name `<h1>` | `@(config.ProjectName ?? "Untitled Dashboard")` | `"Untitled Dashboard"` |
| ADO Backlog link `<a>` | `href="@(config.BacklogUrl ?? "#")"` | `"#"` |
| Subtitle `.sub` | `@(config.Workstream ?? "") · @(config.ReportMonth ?? "")` | Empty string |
| Legend "Now" label | `Now (@(config.ReportMonth ?? ""))` | `"Now ()"` |

**Interactions:** Backlog link is a standard `<a href>`. No Blazor event handlers.

### Section 2: Timeline Area → `.tl-area` region

**Maps to:** Original `.tl-area` div

| Property | Value |
|----------|-------|
| **HTML Element** | `<div class="tl-area">` |
| **CSS Layout** | `display: flex; align-items: stretch; padding: 6px 44px 0; flex-shrink: 0; height: 196px; border-bottom: 2px solid #E8E8E8; background: #FAFAFA;` |

#### 2a: Milestone Sidebar (Left)

| Property | Value |
|----------|-------|
| **HTML Element** | `<div class="tl-sidebar">` |
| **CSS Layout** | `width: 230px; flex-shrink: 0; display: flex; flex-direction: column; justify-content: space-around; padding: 16px 12px 16px 0; border-right: 1px solid #E0E0E0;` |
| **Data Binding** | `@foreach (var ms in config.Timeline.Milestones)` renders `<div>` with `ms.Id`, `ms.Label`, and `style="color: @ms.Color"` |
| **Interactions** | None |

#### 2b: SVG Timeline Panel (Right)

| Property | Value |
|----------|-------|
| **HTML Element** | `<div class="tl-svg-box">` containing `<svg width="1560" height="185">` |
| **CSS Layout** | `flex: 1; padding-left: 12px; padding-top: 6px;` |

**SVG sub-elements and data bindings:**

| SVG Element | Binding | Rendering Logic |
|-------------|---------|-----------------|
| **Drop shadow filter** | Static | `<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>` |
| **Month grid lines** | Computed from `startDate`/`endDate` | `@for` loop: calculate X position for the 1st of each month in range. Render `<line>` and `<text>` label. |
| **NOW line** | `config.Timeline.NowDate` | `<line>` at `GetXPosition(nowDate)` with `stroke="#EA4335" stroke-dasharray="5,3" stroke-width="2"`. `<text>` label "NOW" above. |
| **Milestone lanes** | `@foreach milestone` with index | `<line x1="0" y1="{laneY}" x2="1560" y2="{laneY}" stroke="{ms.Color}" stroke-width="3"/>` |
| **Checkpoint markers** | `event.Type == "checkpoint"` | `<circle cx="{x}" cy="{laneY}" r="7" fill="white" stroke="{ms.Color}" stroke-width="2.5"/>` |
| **PoC diamond markers** | `event.Type == "poc"` | `<polygon points="{GetDiamondPoints(x, laneY)}" fill="#F4B400" filter="url(#sh)"/>` |
| **Production diamond markers** | `event.Type == "production"` | `<polygon points="{GetDiamondPoints(x, laneY)}" fill="#34A853" filter="url(#sh)"/>` |
| **Event labels** | `event.Label` | `<text x="{x}" y="{labelY}" text-anchor="middle" fill="#666" font-size="10">@event.Label</text>` |

**Coordinate calculation:**
```csharp
// In @code block
private double GetX(string? dateStr)
{
    if (dateStr is null || config?.Timeline is null) return 0;
    var date = DateOnly.Parse(dateStr);
    var start = DateOnly.Parse(config.Timeline.StartDate!);
    var end = DateOnly.Parse(config.Timeline.EndDate!);
    return DashboardDataService.GetXPosition(date, start, end, 1560);
}
```

**Interactions:** None. Read-only SVG.

### Section 3: Heatmap Grid → `.hm-wrap` region

**Maps to:** Original `.hm-wrap` and `.hm-grid` divs

| Property | Value |
|----------|-------|
| **HTML Element** | `<div class="hm-wrap">` containing `<div class="hm-title">` and `<div class="hm-grid">` |
| **CSS Layout** | Wrapper: `flex: 1; min-height: 0; display: flex; flex-direction: column; padding: 10px 44px 10px;` |
| **Grid** | `display: grid; grid-template-columns: 160px repeat(@columnCount, 1fr); grid-template-rows: 36px repeat(4, 1fr); border: 1px solid #E0E0E0; flex: 1; min-height: 0;` |

**Data binding:** `@{ var columns = config.Heatmap?.Columns ?? new List<string>(); var columnCount = columns.Count; }`

#### 3a: Grid Header Row

| Cell | CSS Class | Binding |
|------|-----------|---------|
| Corner cell | `.hm-corner` | Static text: "Status / Month" |
| Month headers | `.hm-col-hdr` + conditional `.apr-hdr` | `@foreach (var col in columns)` — applies `.apr-hdr` when `IsCurrentColumn(col)` |

#### 3b: Grid Data Rows (×4)

Each row is rendered via a helper pattern:

```razor
@{ var rows = new[] {
    ("Shipped", "ship", config.Heatmap?.Shipped),
    ("In Progress", "prog", config.Heatmap?.InProgress),
    ("Carryover", "carry", config.Heatmap?.Carryover),
    ("Blockers", "block", config.Heatmap?.Blockers)
}; }

@foreach (var (label, prefix, data) in rows)
{
    <div class="hm-row-hdr @(prefix)-hdr">@label</div>
    @foreach (var col in columns)
    {
        var items = GetCellItems(data, col);
        var isCurrent = IsCurrentColumn(col);
        <div class="hm-cell @(prefix)-cell @(isCurrent ? "apr" : "")">
            @if (items.Count == 0)
            {
                <span style="color:#999;">-</span>
            }
            else
            {
                @foreach (var item in items)
                {
                    <div class="it">@item</div>
                }
            }
        </div>
    }
}
```

**Row-specific CSS classes (from original design):**

| Row | Header Class | Cell Class | Current Month Class | Dot `::before` Color |
|-----|-------------|-----------|--------------------|--------------------|
| Shipped | `.ship-hdr` (`bg: #E8F5E9, color: #1B7A28`) | `.ship-cell` (`bg: #F0FBF0`) | `.ship-cell.apr` (`bg: #D8F2DA`) | `#34A853` |
| In Progress | `.prog-hdr` (`bg: #E3F2FD, color: #1565C0`) | `.prog-cell` (`bg: #EEF4FE`) | `.prog-cell.apr` (`bg: #DAE8FB`) | `#0078D4` |
| Carryover | `.carry-hdr` (`bg: #FFF8E1, color: #B45309`) | `.carry-cell` (`bg: #FFFDE7`) | `.carry-cell.apr` (`bg: #FFF0B0`) | `#F4B400` |
| Blockers | `.block-hdr` (`bg: #FEF2F2, color: #991B1B`) | `.block-cell` (`bg: #FFF5F5`) | `.block-cell.apr` (`bg: #FFE4E4`) | `#EA4335` |

**Interactions:** None. Read-only grid.

### Error State

| Property | Value |
|----------|-------|
| **Condition** | `errorMessage != null` |
| **HTML** | `<div class="error-state">` with centered error icon and message |
| **CSS** | Centered within the 1920×1080 viewport, light red background (`#FEF2F2`), red border, 16px text |
| **Data Binding** | `@errorMessage` |
| **Interactions** | None (user must fix `data.json` and refresh) |

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|------------|
| 1 | **SVG coordinate math errors** misalign milestones on the timeline | Medium | Medium | Write unit tests for `GetXPosition()` and `GetMilestoneLaneY()` with known date/pixel pairs from the original design. Test edge cases: dates before `startDate`, dates after `endDate`, same start/end date. |
| 2 | **CSS Grid rendering differences** between Edge and Chrome cause layout drift | Low | Medium | Test in Edge (primary screenshot browser) and Chrome. The CSS Grid spec is well-standardized; differences are rare for this level of complexity. |
| 3 | **Blazor CSS isolation** scoping doesn't reach SVG child elements | Medium | Low | Use `::deep` combinator for SVG text styling, or apply inline `style` attributes on SVG elements (which Blazor doesn't scope-restrict). |
| 4 | **`data.json` schema evolution** introduces nullable fields that cause `NullReferenceException` at runtime | Medium | Medium | All model properties are nullable. All Razor bindings use `??` and `?.` operators. The error boundary catches and displays any unhandled exceptions. |
| 5 | **Blazor Server SignalR circuit drops** blank the page | Low | Low | Acceptable for local-only use. Default Blazor reconnect UI handles recovery. User can also just refresh. |
| 6 | **Fixed 1920×1080 layout clips on smaller monitors** | Low | Low | This is by design for screenshot capture. Document in README that the dashboard requires a 1920×1080 or larger display at 100% zoom. |
| 7 | **`dotnet watch` hot reload** doesn't pick up `data.json` changes | Medium | Low | Hot reload applies to Razor/CSS/C# files, not static files. Documented behavior: user must manually refresh the browser after editing `data.json`. |
| 8 | **Heatmap cell overflow** when >5 items are listed | Low | Medium | Design constraint: ≤5 items per cell. If exceeded, `overflow: hidden` on `.hm-cell` prevents layout breakage. Items beyond the cell height are clipped — acceptable for MVP. |
| 9 | **Pre-range timeline dates** (events before `startDate`) render off-screen | Low | Low | `GetXPosition` uses `Math.Clamp` to pin values between 0 and `totalWidth`. Events before the range render at x=0 (left edge). |
| 10 | **Port 5000 conflict** with another local service | Low | Low | Configurable via `ASPNETCORE_URLS` environment variable or `appsettings.json`. Document in README. |