# Architecture

## Overview & Goals

This document defines the complete system architecture for the **Executive Reporting Dashboard** — a single-page Blazor Server application that renders project milestone timelines and work item status heatmaps, driven entirely by a local `data.json` file. The dashboard produces pixel-perfect 1920×1080 screenshots for direct insertion into PowerPoint executive decks.

### Primary Goals

1. **Executive-ready visualization** — Render a single-page dashboard with an SVG milestone timeline and color-coded heatmap grid that communicates project health in under 10 seconds of viewing.
2. **Data-driven rendering** — All content is sourced from `wwwroot/data.json`; zero hardcoded data in components.
3. **Zero operational overhead** — No cloud services, no database, no authentication, no external dependencies. Runs via `dotnet run` on localhost.
4. **Screenshot fidelity** — Fixed 1920×1080 viewport with no scrollbars, producing clean DevTools screenshots matching `OriginalDesignConcept.html` exactly.
5. **Rapid update cycle** — Edit `data.json` → refresh browser → updated dashboard in seconds.

### Architecture Principles

- **Simplicity over extensibility** — This is a one-page app. Do not over-architect.
- **CSS verbatim from design** — Styles are extracted directly from `OriginalDesignConcept.html` without modernization.
- **Zero JavaScript** — Pure server-rendered HTML/CSS/SVG via Blazor Server SSR.
- **Single project structure** — One `.sln`, one `.csproj`, zero external NuGet packages beyond the default template.

---

## System Components

### 1. `Program.cs` — Application Entry Point

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure DI container, register services, set up middleware pipeline, start Kestrel |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService` (registered as singleton) |
| **Data** | None directly; configures the service that loads `data.json` |

**Implementation Contract:**

```csharp
var builder = WebApplication.CreateBuilder(args);
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

No authentication middleware. No CORS. No HTTPS redirection. No database context. Bind to `http://localhost:5000` only.

---

### 2. `DashboardDataService` — Data Access Layer

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Read `wwwroot/data.json` from disk, deserialize into `DashboardData` POCO, cache in memory, provide error state on failure |
| **Interfaces** | `DashboardData? GetData()` and `string? GetError()` |
| **Dependencies** | `IWebHostEnvironment` (to resolve `wwwroot` path), `System.Text.Json` |
| **Data** | Reads `wwwroot/data.json`; holds deserialized `DashboardData` in a private field |
| **Lifetime** | Singleton — data loaded once at construction time |
| **Location** | `Services/DashboardDataService.cs` |

**Behavior:**

- Constructor resolves `Path.Combine(env.WebRootPath, "data.json")`.
- If file missing: stores error string `"No data.json found. Please create wwwroot/data.json with your project data."`
- If JSON malformed: catches `JsonException`, stores error string `"Unable to load dashboard data. Please check data.json for errors."` with the exception message appended.
- If successful: stores deserialized `DashboardData` instance.
- For v1, data is loaded once at startup. Changing `data.json` requires an app restart (or browser refresh if using `dotnet watch`).

**Implementation:**

```csharp
public class DashboardDataService
{
    private readonly DashboardData? _data;
    private readonly string? _error;

    public DashboardDataService(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.WebRootPath, "data.json");
        if (!File.Exists(path))
        {
            _error = "No data.json found. Please create wwwroot/data.json with your project data.";
            return;
        }
        try
        {
            var json = File.ReadAllText(path);
            _data = JsonSerializer.Deserialize<DashboardData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _error = $"Unable to load dashboard data. Please check data.json for errors. Details: {ex.Message}";
        }
    }

    public DashboardData? GetData() => _data;
    public string? GetError() => _error;
}
```

---

### 3. `DashboardData` Model Classes — Data Model Layer

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define C# POCOs that map 1:1 to the `data.json` schema |
| **Interfaces** | Plain record types with `init` properties |
| **Dependencies** | None |
| **Data** | Deserialized from JSON |
| **Location** | `Models/DashboardData.cs` |

See [Data Model](#data-model) section for full class definitions.

---

### 4. `App.razor` — Blazor Root Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define the HTML document shell (`<html>`, `<head>`, `<body>`), reference `dashboard.css`, set the root render tree |
| **Interfaces** | Blazor component tree root |
| **Dependencies** | `DashboardLayout.razor` (via `Routes.razor`) |
| **Location** | `Components/App.razor` |

**Key requirements:**
- Reference `css/dashboard.css` in `<head>` — no Bootstrap, no `app.css`, no `blazor.css`
- No `<link>` to any framework CSS
- The `<body>` tag receives the fixed viewport styles via `dashboard.css`

---

### 5. `DashboardLayout.razor` — Minimal Layout

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Provide a bare-minimum Blazor layout with no navigation, no sidebar, no header chrome — just `@Body` |
| **Interfaces** | `@inherits LayoutComponentBase` |
| **Dependencies** | None |
| **Location** | `Components/Layout/DashboardLayout.razor` |

**Implementation:**

```razor
@inherits LayoutComponentBase
@Body
```

No `<nav>`, no `<aside>`, no `<footer>`. This replaces the default `MainLayout.razor` entirely.

---

### 6. `Dashboard.razor` — Page Compositor

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Route handler for `/`; injects `DashboardDataService`; renders error state or composes child components |
| **Interfaces** | `@page "/"` route; cascading `DashboardData` parameter to children |
| **Dependencies** | `DashboardDataService`, `Header.razor`, `Timeline.razor`, `HeatmapGrid.razor` |
| **Location** | `Components/Pages/Dashboard.razor` |

**Behavior:**
- If `GetError()` is non-null: render a centered error message on white background (no stack trace).
- If `GetData()` returns valid data: render `<Header>`, `<Timeline>`, `<HeatmapGrid>` in sequence, each receiving the `DashboardData` as a `[Parameter]`.

---

### 7. `Header.razor` — Header Bar Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render project title with ADO backlog link, subtitle, and the 4-item legend |
| **Interfaces** | `[Parameter] public DashboardData Data { get; set; }` |
| **Dependencies** | `DashboardData.Project` (title, subtitle, backlogUrl, currentMonth) |
| **CSS classes** | `.hdr`, `.sub` |
| **Location** | `Components/Header.razor` |

**Data bindings:**
- `Data.Project.Title` → `<h1>` text content
- `Data.Project.BacklogUrl` → `<a href>` with `target="_blank"`
- `Data.Project.Subtitle` → `.sub` div text
- `Data.Project.CurrentMonth` → Legend "Now" label (e.g., "Now (Apr 2026)")

---

### 8. `Timeline.razor` — SVG Milestone Timeline Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the milestone label panel (left) and the SVG timeline (right) with dynamically positioned markers |
| **Interfaces** | `[Parameter] public DashboardData Data { get; set; }` |
| **Dependencies** | `DashboardData.Milestones[]`, `DashboardData.TimelineRange`, `DashboardData.Project.CurrentDate` |
| **CSS classes** | `.tl-area`, `.tl-svg-box` |
| **Location** | `Components/Timeline.razor` |

**`@code` block helper methods:**

| Method | Signature | Purpose |
|--------|-----------|---------|
| `DateToX` | `int DateToX(string dateStr)` | Linear interpolation: `x = (dayOffset / totalDays) * svgWidth` using `timelineRange.startDate` and `endDate` |
| `GetMilestoneY` | `int GetMilestoneY(int index)` | Returns y-coordinate for swim lane: index 0 → 42, index 1 → 98, index 2 → 154. For dynamic milestone counts: `y = 42 + index * 56` (spacing 56px per lane within 185px SVG height) |
| `DiamondPoints` | `string DiamondPoints(int cx, int cy)` | Returns SVG polygon points for an 11px-radius rotated square: `"{cx},{cy-11} {cx+11},{cy} {cx},{cy+11} {cx-11},{cy}"` |
| `NowX` | `int NowX` (computed property) | Calls `DateToX(Data.Project.CurrentDate)` |

**SVG element rendering order:**
1. `<defs>` with drop shadow filter `id="sh"`
2. Month grid lines (vertical, at computed x positions for each month label)
3. Month labels (text elements)
4. NOW dashed line + "NOW" label
5. For each milestone: horizontal swim lane line → event markers (checkpoint circles, PoC diamonds, production diamonds) → event date labels
6. Tooltip `<title>` elements inside each marker group for hover behavior

**Tooltip implementation (CSS-only via SVG `<title>`):**
Each event marker is wrapped in a `<g>` element containing a `<title>` child with text like `"M1 - API Gateway & Auth: Mar 20, 2026 - PoC Milestone"`. The browser renders this as a native tooltip on hover. No JavaScript required.

---

### 9. `HeatmapGrid.razor` — Status Heatmap Grid Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the CSS Grid heatmap with header row, 4 status rows, and work item bullets |
| **Interfaces** | `[Parameter] public DashboardData Data { get; set; }` |
| **Dependencies** | `DashboardData.Months[]`, `DashboardData.Shipped`, `DashboardData.InProgress`, `DashboardData.Carryover`, `DashboardData.Blockers`, `DashboardData.Project.CurrentMonth` |
| **CSS classes** | `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`, `.it`, and all row-specific classes |
| **Location** | `Components/HeatmapGrid.razor` |

**Rendering logic:**

1. **Grid title** — Static text with `·` separators
2. **Header row** — Corner cell ("STATUS") + one column header per month from `Data.Months[]`. If `month == Data.Project.CurrentMonth`, apply `.apr-hdr` class and append `" ◀ Now"` text.
3. **Status rows** — Iterate over a local array of 4 row definitions:

```csharp
private readonly record struct StatusRowDef(
    string Label,         // "✅ Shipped"
    string HdrClass,      // "ship-hdr"
    string CellClass,     // "ship-cell"
    string CurrentClass,  // "apr"
    Dictionary<string, List<string>>? Items  // Data.Shipped
);
```

4. **Cell rendering** — For each month in `Data.Months[]`:
   - Look up items from the status dictionary by month name
   - If items exist and count > 0: render each as `<div class="it">item text</div>`
   - If no items or empty list: render `<div style="color:#CCC;font-size:12px;">-</div>`
   - Apply current-month class (`.apr`) if `month == Data.Project.CurrentMonth`

---

### 10. `dashboard.css` — Stylesheet

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define all visual styles for the dashboard, extracted verbatim from `OriginalDesignConcept.html` |
| **Location** | `wwwroot/css/dashboard.css` |

**Critical CSS rules:**

```css
/* Viewport lock */
* { margin: 0; padding: 0; box-sizing: border-box; }
body { width: 1920px; height: 1080px; overflow: hidden; background: #FFFFFF;
       font-family: 'Segoe UI', Arial, sans-serif; color: #111;
       display: flex; flex-direction: column; }
```

The full CSS is copied directly from the `<style>` block in `OriginalDesignConcept.html`. Additionally, a tooltip style is added for SVG hover behavior:

```css
/* SVG tooltip styling (native browser tooltip via <title>) */
svg text { pointer-events: none; }
svg polygon, svg circle { cursor: default; }
```

**No CSS isolation files** (`.razor.css`). Single file for the entire application.

---

### 11. `data.json` — Configuration Data

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Store all dashboard content: project metadata, milestones, timeline range, and heatmap work items |
| **Location** | `wwwroot/data.json` |
| **Format** | JSON, UTF-8, < 10KB |

See [Data Model](#data-model) section for the full schema.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Startup                       │
│                                                                   │
│  ┌──────────┐    File.ReadAllText    ┌──────────────┐            │
│  │data.json │ ──────────────────────▶│DashboardData │            │
│  │(wwwroot) │                        │  Service     │            │
│  └──────────┘                        │  (Singleton) │            │
│                                      └──────┬───────┘            │
└─────────────────────────────────────────────┼────────────────────┘
                                              │
                    ┌─────────────────────────┼─────────────────────┐
                    │        HTTP Request: GET /                     │
                    │                                               │
                    │  ┌────────────┐  GetData()   ┌────────────┐  │
                    │  │ Dashboard  │◄─────────────│DashboardData│  │
                    │  │   .razor   │              │  Service    │  │
                    │  └─────┬──────┘              └────────────┘  │
                    │        │ [Parameter] Data                     │
                    │   ┌────┼─────────────┐                       │
                    │   ▼    ▼             ▼                        │
                    │ Header Timeline  HeatmapGrid                  │
                    │ .razor  .razor     .razor                     │
                    │   │      │           │                        │
                    │   ▼      ▼           ▼                        │
                    │ HTML   HTML+SVG    HTML (CSS Grid)            │
                    │                                               │
                    │  ──────────── Rendered HTML ──────────────▶   │
                    │              (1920×1080 viewport)             │
                    └───────────────────────────────────────────────┘
```

### Communication Patterns

| From | To | Mechanism | Data |
|------|----|-----------|------|
| `Program.cs` | `DashboardDataService` | DI registration (Singleton) | N/A |
| `DashboardDataService` | `data.json` | `File.ReadAllText()` at construction | Raw JSON string |
| `Dashboard.razor` | `DashboardDataService` | `@inject` DI | `DashboardData?` or error string |
| `Dashboard.razor` | `Header.razor` | `[Parameter]` | `DashboardData` |
| `Dashboard.razor` | `Timeline.razor` | `[Parameter]` | `DashboardData` |
| `Dashboard.razor` | `HeatmapGrid.razor` | `[Parameter]` | `DashboardData` |
| Browser | Kestrel | HTTP GET `/` | Server-rendered HTML response |

### Error Flow

```
data.json missing ──▶ DashboardDataService._error set
                          │
data.json malformed ──▶ DashboardDataService._error set
                          │
                          ▼
Dashboard.razor checks GetError() != null
    │
    ▼
Renders: <div style="...centered...">Error message text</div>
(No stack trace, no Blazor error UI)
```

---

## Data Model

### Entity Definitions

All models are defined in `Models/DashboardData.cs` as C# `record` types with `init`-only properties for immutability. Property names use `PascalCase` in C# and map to `camelCase` in JSON via `PropertyNameCaseInsensitive = true`.

```csharp
using System.Text.Json.Serialization;

public record DashboardData
{
    public ProjectInfo? Project { get; init; }
    public List<string>? Months { get; init; }
    public TimelineRange? TimelineRange { get; init; }
    public List<Milestone>? Milestones { get; init; }
    public Dictionary<string, List<string>>? Shipped { get; init; }
    public Dictionary<string, List<string>>? InProgress { get; init; }
    public Dictionary<string, List<string>>? Carryover { get; init; }
    public Dictionary<string, List<string>>? Blockers { get; init; }
}

public record ProjectInfo
{
    public string? Title { get; init; }
    public string? Subtitle { get; init; }
    public string? BacklogUrl { get; init; }
    public string? CurrentMonth { get; init; }
    public string? CurrentDate { get; init; }
}

public record TimelineRange
{
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public int SvgWidth { get; init; } = 1560;
    public List<string>? MonthLabels { get; init; }
}

public record Milestone
{
    public string? Id { get; init; }
    public string? Label { get; init; }
    public string? Color { get; init; }
    public List<MilestoneEvent>? Events { get; init; }
}

public record MilestoneEvent
{
    public string? Date { get; init; }
    public string? Label { get; init; }
    public string? Type { get; init; }  // "checkpoint" | "poc" | "production"
}
```

### Design Decisions

- **All properties are nullable** — Enables graceful degradation when JSON fields are missing. Components use null-conditional operators (`?.`) and null-coalescing (`??`) throughout.
- **`Dictionary<string, List<string>>`** for status sections — Matches the JSON structure directly (`"shipped": { "Jan": [...], "Feb": [...] }`). No intermediate adapter or transform layer.
- **`string` for dates** — Parsed to `DateOnly` in the `Timeline.razor` `@code` block when needed for coordinate calculations. Kept as strings in the model to simplify deserialization and avoid format issues.
- **No `[JsonPropertyName]` attributes** — `PropertyNameCaseInsensitive = true` handles the camelCase → PascalCase mapping.

### JSON Schema (Reference)

```json
{
  "project": {
    "title": "string",
    "subtitle": "string",
    "backlogUrl": "string (URL)",
    "currentMonth": "string (e.g., 'Apr')",
    "currentDate": "string (ISO date, e.g., '2026-04-16')"
  },
  "months": ["string", "string", "string", "string"],
  "timelineRange": {
    "startDate": "string (ISO date)",
    "endDate": "string (ISO date)",
    "svgWidth": "number (default 1560)",
    "monthLabels": ["string", ...]
  },
  "milestones": [
    {
      "id": "string",
      "label": "string",
      "color": "string (hex color)",
      "events": [
        {
          "date": "string (ISO date)",
          "label": "string",
          "type": "checkpoint | poc | production"
        }
      ]
    }
  ],
  "shipped": { "<month>": ["string", ...] },
  "inProgress": { "<month>": ["string", ...] },
  "carryover": { "<month>": ["string", ...] },
  "blockers": { "<month>": ["string", ...] }
}
```

### Storage

- **Type:** Flat JSON file on local disk
- **Path:** `wwwroot/data.json` (served as a static file, but read via filesystem by the service)
- **Size constraint:** < 10KB (typically 2-5KB for ~50 work items)
- **Encoding:** UTF-8
- **No database, no ORM, no migrations**

---

## API Contracts

This application exposes **no REST, GraphQL, or WebSocket API**. It is a server-rendered Blazor application with a single HTTP endpoint.

### HTTP Endpoints

| Method | Path | Response | Content-Type |
|--------|------|----------|-------------|
| GET | `/` | Full HTML page (server-rendered dashboard) | `text/html` |
| GET | `/_blazor` | Blazor Server SignalR negotiation (framework-managed) | `application/json` |
| GET | `/css/dashboard.css` | Static CSS file | `text/css` |
| GET | `/data.json` | Static JSON file (also accessible via browser, but primarily read server-side) | `application/json` |
| GET | `/_framework/*` | Blazor framework static assets | Various |

### Error Handling

| Condition | HTTP Status | Rendered Output |
|-----------|------------|-----------------|
| Normal operation | 200 | Full dashboard HTML |
| `data.json` missing | 200 | HTML page with centered error message: "No data.json found..." |
| `data.json` malformed | 200 | HTML page with centered error message: "Unable to load dashboard data..." |
| Null/missing JSON fields | 200 | Dashboard renders with empty sections (graceful degradation) |

Error states are handled at the **application layer** (inside `Dashboard.razor`), not at the HTTP layer. The page always returns 200 OK — errors are rendered as user-friendly messages within the page body.

### Internal Service Contract

```csharp
// DashboardDataService public interface
public DashboardData? GetData();   // Returns null if load failed
public string? GetError();          // Returns error message if load failed, null otherwise
```

**Invariant:** Exactly one of `GetData()` or `GetError()` will be non-null.

---

## Infrastructure Requirements

### Development Environment

| Requirement | Specification |
|-------------|--------------|
| **OS** | Windows 10/11 (Segoe UI font required) |
| **SDK** | .NET 8 SDK (8.0.x latest patch) |
| **IDE** | Any text editor; VS Code or Visual Studio recommended |
| **Browser** | Edge or Chrome (for DevTools screenshots at 1920×1080) |
| **Network** | Not required at runtime; only for initial `dotnet new` template download |

### Runtime Environment

| Aspect | Specification |
|--------|--------------|
| **Host** | Kestrel (built into ASP.NET Core) |
| **Binding** | `http://localhost:5000` (HTTP only, no HTTPS required) |
| **Process** | Single `dotnet run` process |
| **Memory** | < 50MB (Blazor Server baseline + tiny data model) |
| **Disk** | < 5MB (published output) |

### Networking

- **Localhost only** — No external network access, no firewall rules, no DNS
- **No TLS/HTTPS** — Local tool; `http://` is acceptable
- **No reverse proxy** — No IIS, no nginx, no Azure App Service
- **No CORS** — No cross-origin requests

### Storage

- **`wwwroot/data.json`** — Single flat file, < 10KB, manually edited
- **No database** — No SQLite, no SQL Server, no connection strings
- **No file system writes** — Application only reads `data.json`; never writes

### CI/CD

- **Not required** — Local development only
- **Build command:** `dotnet build`
- **Run command:** `dotnet run` or `dotnet watch` (preferred for development)
- **Publish command:** `dotnet publish -c Release` (optional, for sharing the compiled binary)

### Project Structure on Disk

```
ReportingDashboard/
├── ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css
    │   └── data.json
    ├── Models/
    │   └── DashboardData.cs
    ├── Services/
    │   └── DashboardDataService.cs
    ├── Components/
    │   ├── App.razor
    │   ├── Routes.razor
    │   ├── _Imports.razor
    │   ├── Pages/
    │   │   └── Dashboard.razor
    │   ├── Layout/
    │   │   └── DashboardLayout.razor
    │   ├── Header.razor
    │   ├── Timeline.razor
    │   └── HeatmapGrid.razor
    └── Properties/
        └── launchSettings.json
```

---

## Technology Stack Decisions

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Runtime** | .NET 8 | 8.0.x | Mandated stack. LTS release with native AOT support and `System.Text.Json` built-in. |
| **UI Framework** | Blazor Server | Built into .NET 8 | Mandated stack. Server-side rendering eliminates WASM download time. Single-user local tool means SignalR overhead is negligible. |
| **CSS** | Raw CSS (Grid + Flexbox) | N/A | Design is pixel-specific with exact hex colors and pixel dimensions. Any framework (Bootstrap, Tailwind) would fight the design and add unnecessary weight. CSS is ~120 lines — trivially maintainable. |
| **SVG** | Hand-crafted inline SVG | N/A | Timeline has ~20 elements (lines, circles, polygons, text). No charting library justified. Hand-crafted SVG matches the original design exactly and produces clean screenshots. |
| **JSON** | `System.Text.Json` | Built into .NET 8 | Native, zero-dependency, fast. Deserializes < 10KB in < 1ms. No need for Newtonsoft.Json. |
| **Font** | Segoe UI (system) | N/A | Windows system font. No web font download, no FOUT, no CDN dependency. Matches the design spec exactly. |
| **Build** | `dotnet build` / `dotnet run` | N/A | No webpack, no npm, no bundler. Single command builds and runs. |

### Explicitly Rejected Technologies

| Technology | Reason for Rejection |
|-----------|---------------------|
| **MudBlazor / Radzen / Syncfusion / Telerik** | Adds theming conflicts, unpredictable screenshot output, and unnecessary complexity for a 120-line CSS design |
| **Chart.js / Highcharts / D3.js** | Requires JavaScript interop (prohibited). The SVG timeline is simpler without a charting library. |
| **Entity Framework / SQLite / SQL Server** | No database needed. A JSON file is the correct storage for < 50 items of config data. |
| **Bootstrap / Tailwind CSS** | Conflicts with the pixel-exact design. Would require extensive overrides to match the original. |
| **Newtonsoft.Json** | Unnecessary external dependency. `System.Text.Json` is built-in and sufficient. |
| **Blazor WebAssembly** | Mandated stack is Blazor Server. WASM would add download latency for no benefit in a local-only scenario. |

### NuGet Package Policy

**Zero additional packages.** The default Blazor Server template (`.NET 8`) includes everything needed:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

No `<PackageReference>` elements required beyond what the SDK provides implicitly.

---

## Security Considerations

### Threat Model

This is a **local-only, single-user tool** with no network exposure, no authentication, and no sensitive data. The threat surface is minimal.

| Threat | Applicability | Mitigation |
|--------|--------------|------------|
| **Unauthorized access** | N/A — localhost only, single user | Kestrel binds to `localhost`; not accessible from network |
| **Data exfiltration** | N/A — no PII, no secrets in `data.json` | Data contains only project status text and ADO URLs |
| **XSS** | Low — Blazor auto-escapes rendered content | Blazor's `@` syntax HTML-encodes by default. No `@((MarkupString)...)` used for user data. |
| **CSRF** | Low — no state-changing forms | Blazor's antiforgery middleware is included by default |
| **Supply chain** | Low — zero external NuGet packages | Only the .NET 8 SDK's built-in packages are used |
| **Injection** | N/A — no database, no SQL, no shell commands | All data flows from `data.json` → C# model → rendered HTML |

### Authentication & Authorization

**None.** No login page, no identity provider, no tokens, no cookies (beyond Blazor's circuit cookie). The application is designed for a single user running it on their own machine.

### Data Protection

- `data.json` contains project names, work item descriptions, and ADO backlog URLs — no PII, no credentials, no secrets.
- File is stored on the local filesystem with standard user permissions.
- No encryption at rest or in transit (localhost HTTP is acceptable per requirements).

### Input Validation

- **`data.json` deserialization** — `System.Text.Json` handles malformed JSON by throwing `JsonException`, which is caught and surfaced as a user-friendly error.
- **Null checks** — All model properties are nullable. Components use `?.` and `??` operators to prevent `NullReferenceException`.
- **No user-generated input** — The dashboard has no forms, no text fields, no file uploads. All data comes from a developer-controlled JSON file.

### SVG Injection Prevention

- SVG content is generated server-side from typed C# data (strings, ints, DateOnly).
- Blazor's `@` syntax automatically HTML/XML-encodes string values rendered into SVG attributes and text content.
- No raw HTML/SVG injection (`@((MarkupString)...)`) is used for any data-driven content.

---

## Scaling Strategy

### Current Scale

| Dimension | Capacity |
|-----------|----------|
| **Users** | 1 (single developer/PM on localhost) |
| **Pages** | 1 (single dashboard route) |
| **Data volume** | ~50 work items, ~3 milestones, ~4 months |
| **Concurrent requests** | 1 |

### Scaling Is Not Applicable

This is a local, single-user, single-page tool designed for screenshot generation. There is no production deployment, no multi-user access, and no cloud hosting. Traditional scaling concerns (horizontal scaling, load balancing, caching, CDN) do not apply.

### Data Volume Scaling

The dashboard layout accommodates **up to ~50 work items** across all status categories and months without vertical overflow at 1080px height. If more items are needed:

- **Heatmap cells** use `overflow: hidden` — items beyond the cell height are clipped
- **Milestone count** is dynamic — the SVG swim lane spacing formula (`y = 42 + index * 56`) supports up to ~3 milestones in the 185px SVG height; more than 3 would require adjusting the SVG height in `data.json`'s `timelineRange`
- **Month columns** are configurable via `data.json`'s `months[]` array; the CSS grid uses `repeat(N, 1fr)` where N matches the array length

### Future Multi-Project Scaling (Phase 3)

If the dashboard is later extended to support multiple projects:

- Route: `/project/{name}` loads `wwwroot/data.{name}.json`
- Each project has its own independent `data.json` file
- No shared state between projects
- Still single-user, single-machine

---

## Risks & Mitigations

### Risk 1: Screenshot Fidelity Mismatch

| Attribute | Detail |
|-----------|--------|
| **Severity** | High |
| **Probability** | Medium |
| **Impact** | Dashboard screenshots don't match `OriginalDesignConcept.html` design, defeating the core business goal |

**Mitigations:**
1. CSS is extracted **verbatim** from the HTML design — no rewriting, no framework substitution
2. `<body>` is locked to `width: 1920px; height: 1080px; overflow: hidden`
3. All default Blazor template styles (Bootstrap, `app.css`) are removed entirely
4. Acceptance testing uses DevTools device toolbar at 1920×1080 with "Capture full size screenshot"
5. Side-by-side comparison with `OriginalDesignConcept.png` is the final validation step

### Risk 2: SVG Timeline Date-to-Pixel Accuracy

| Attribute | Detail |
|-----------|--------|
| **Severity** | Medium |
| **Probability** | Low |
| **Impact** | Milestone markers appear at wrong positions on the timeline |

**Mitigations:**
1. Linear interpolation formula is simple and deterministic:
   ```csharp
   int DateToX(string dateStr)
   {
       var date = DateOnly.Parse(dateStr);
       var start = DateOnly.Parse(Data.TimelineRange.StartDate);
       var end = DateOnly.Parse(Data.TimelineRange.EndDate);
       var totalDays = end.DayNumber - start.DayNumber;
       var dayOffset = date.DayNumber - start.DayNumber;
       return (int)(dayOffset / (double)totalDays * Data.TimelineRange.SvgWidth);
   }
   ```
2. "NOW" line uses the same formula with `Data.Project.CurrentDate` (not system clock), ensuring reproducible positioning
3. Acceptance criteria: markers within ±5px of expected position (verifiable by manual calculation)

### Risk 3: Malformed `data.json` Crashes the App

| Attribute | Detail |
|-----------|--------|
| **Severity** | Medium |
| **Probability** | Medium (PM manually edits JSON) |
| **Impact** | Blazor shows unhandled exception page instead of the dashboard |

**Mitigations:**
1. `DashboardDataService` wraps deserialization in `try/catch (JsonException)` — stores a user-friendly error message
2. `Dashboard.razor` checks for error state before rendering components — displays the error message on a clean white background
3. All model properties are nullable with `?` — missing fields produce `null` instead of exceptions
4. Components use `?.` and `??` operators: `@(Data.Project?.Title ?? "Untitled Project")`
5. Empty collections default to `[]` not `null` where possible

### Risk 4: Blazor Template Boilerplate Leaks into Dashboard

| Attribute | Detail |
|-----------|--------|
| **Severity** | Medium |
| **Probability** | Medium (easy to miss during template cleanup) |
| **Impact** | Default Blazor nav sidebar, counter page, or Bootstrap styles appear in screenshots |

**Mitigations:**
1. Replace `MainLayout.razor` with `DashboardLayout.razor` containing only `@Body`
2. Delete all default pages: `Counter.razor`, `Weather.razor`, `Home.razor`, `Error.razor`
3. Remove all `<link>` references to `bootstrap.min.css`, `app.css`, and `blazor.css` from `App.razor`
4. Only `dashboard.css` is referenced in the `<head>`
5. Acceptance criteria: visual inspection confirms zero Blazor chrome

### Risk 5: Blazor Server SignalR Overhead

| Attribute | Detail |
|-----------|--------|
| **Severity** | Low |
| **Probability** | Low |
| **Impact** | Slight overhead from maintaining a SignalR circuit for a static page |

**Mitigations:**
1. The page can optionally use static SSR (omit `@rendermode`) since there is no interactivity beyond the initial render
2. For v1 with tooltips, `@rendermode InteractiveServer` is acceptable — the overhead is negligible for a single local user
3. Blazor Server is the mandated stack; the SignalR circuit cost (~100KB memory) is immaterial for this use case

### Risk 6: `data.json` Changes Not Reflected Without Restart

| Attribute | Detail |
|-----------|--------|
| **Severity** | Low |
| **Probability** | Certain (by design in v1) |
| **Impact** | PM must restart `dotnet run` after editing `data.json` |

**Mitigations:**
1. `dotnet watch` provides automatic restart on file changes during development
2. Phase 2 enhancement: Add `FileSystemWatcher` to detect `data.json` changes and re-read without restart
3. Documented in README: "After editing `data.json`, restart the application or use `dotnet watch`"

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component, its CSS layout strategy, data bindings, and interactions.

### Component-to-Design Mapping

| Visual Section | Component | CSS Layout | Design Classes |
|---------------|-----------|-----------|----------------|
| **Header bar** (top strip with title + legend) | `Header.razor` | Flexbox row (`display:flex; justify-content:space-between`) | `.hdr`, `.sub` |
| **Timeline area** (milestone labels + SVG) | `Timeline.razor` | Flexbox row (`display:flex; align-items:stretch`) | `.tl-area`, `.tl-svg-box` |
| **Heatmap grid** (status rows × month columns) | `HeatmapGrid.razor` | CSS Grid (`grid-template-columns: 160px repeat(N,1fr)`) | `.hm-wrap`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell` |
| **Page compositor** (vertical stack of all sections) | `Dashboard.razor` | Inherits `body` flex-column | `body { display:flex; flex-direction:column }` |

### Header.razor — Design Mapping

```
┌──────────────────────────────────────────────────────────────────┐
│ .hdr (padding: 12px 44px 10px, border-bottom: 1px solid #E0E0E0)│
│ ┌─────────────────────────────┐  ┌──────────────────────────────┐│
│ │ Left side                   │  │ Right side (legend)          ││
│ │ <h1>Title <a>ADO Backlog</a>│  │ ◆PoC  ◆Prod  ●Check  |Now  ││
│ │ <div class="sub">Subtitle   │  │ (flex row, gap: 22px)       ││
│ └─────────────────────────────┘  └──────────────────────────────┘│
└──────────────────────────────────────────────────────────────────┘
```

| Element | Data Binding | CSS |
|---------|-------------|-----|
| Title text | `@Data.Project?.Title` | `h1 { font-size:24px; font-weight:700 }` |
| ADO link | `href="@Data.Project?.BacklogUrl"` with `target="_blank"` | `a { color:#0078D4; text-decoration:none }` |
| Subtitle | `@Data.Project?.Subtitle` | `.sub { font-size:12px; color:#888; margin-top:2px }` |
| Legend "Now" label | `"Now (@(Data.Project?.CurrentMonth) 2026)"` | `font-size:12px` |
| PoC diamond icon | Static HTML: `<span style="width:12px;height:12px;background:#F4B400;transform:rotate(45deg);display:inline-block">` | Inline styles (matching design) |
| Prod diamond icon | Same as PoC with `background:#34A853` | Inline styles |
| Checkpoint circle | `<span style="width:8px;height:8px;border-radius:50%;background:#999">` | Inline styles |
| Now bar | `<span style="width:2px;height:14px;background:#EA4335">` | Inline styles |

**Interactions:** ADO Backlog link opens in new tab (`target="_blank"`). No other interactions.

---

### Timeline.razor — Design Mapping

```
┌──────────────────────────────────────────────────────────────────┐
│ .tl-area (height:196px, background:#FAFAFA, border-bottom:2px)   │
│ ┌────────────┐ ┌────────────────────────────────────────────────┐│
│ │ Left panel │ │ .tl-svg-box                                    ││
│ │ (230px)    │ │ <svg width="1560" height="185">                ││
│ │            │ │   Month grid lines (vertical)                  ││
│ │ M1 label   │ │   Month labels (Jan-Jun)                      ││
│ │ M2 label   │ │   NOW dashed line (red)                       ││
│ │ M3 label   │ │   Swim lane lines (horizontal, per milestone) ││
│ │            │ │   Event markers (circles, diamonds)            ││
│ │            │ │   Event date labels                            ││
│ └────────────┘ └────────────────────────────────────────────────┘│
└──────────────────────────────────────────────────────────────────┘
```

| Element | Data Binding | Rendering Logic |
|---------|-------------|-----------------|
| Left panel labels | `@foreach milestone in Data.Milestones` | `<div style="color:@milestone.Color">@milestone.Id<br/><span>@milestone.Label</span></div>` |
| Month grid lines | `@foreach label in Data.TimelineRange.MonthLabels` | X positions computed: `index * (svgWidth / monthCount)` |
| NOW line | `DateToX(Data.Project.CurrentDate)` | `<line stroke="#EA4335" stroke-dasharray="5,3">` |
| Swim lane lines | `@for (i = 0; i < milestones.Count; i++)` | `<line y1="@GetY(i)" y2="@GetY(i)" stroke="@milestone.Color">` |
| Checkpoint markers | `evt.Type == "checkpoint"` | `<circle fill="white" stroke="@milestone.Color" r="5-7">` |
| PoC markers | `evt.Type == "poc"` | `<polygon points="@DiamondPoints(x,y)" fill="#F4B400" filter="url(#sh)">` |
| Production markers | `evt.Type == "production"` | `<polygon points="@DiamondPoints(x,y)" fill="#34A853" filter="url(#sh)">` |
| Event labels | `evt.Label` | `<text x="@x" y="@(above ? y-16 : y+24)" text-anchor="middle">` |

**Interactions:** Native SVG `<title>` tooltips on hover over diamond/circle markers. No click handlers.

**Coordinate calculation:**
- SVG width: `Data.TimelineRange.SvgWidth` (default 1560)
- Month grid spacing: `svgWidth / (monthLabels.Count)` ≈ 260px per month
- Date-to-X: `(dayOffset / totalDays) * svgWidth`
- Swim lane Y: `42 + (milestoneIndex * 56)`

---

### HeatmapGrid.razor — Design Mapping

```
┌──────────────────────────────────────────────────────────────────┐
│ .hm-wrap (flex:1, padding:10px 44px)                             │
│ ┌────────────────────────────────────────────────────────────────┐│
│ │ .hm-title: "MONTHLY EXECUTION HEATMAP - SHIPPED · ..."        ││
│ └────────────────────────────────────────────────────────────────┘│
│ ┌────────────────────────────────────────────────────────────────┐│
│ │ .hm-grid (CSS Grid: 160px + repeat(4, 1fr))                   ││
│ │ ┌─────────┬────────┬────────┬────────┬────────┐               ││
│ │ │ STATUS  │  Jan   │  Feb   │  Mar   │ Apr◀Now│  ← header row ││
│ │ ├─────────┼────────┼────────┼────────┼────────┤               ││
│ │ │✅SHIPPED│ items  │ items  │ items  │ items  │  ← green row  ││
│ │ ├─────────┼────────┼────────┼────────┼────────┤               ││
│ │ │🔄IN PROG│ items  │ items  │ items  │ items  │  ← blue row   ││
│ │ ├─────────┼────────┼────────┼────────┼────────┤               ││
│ │ │🔁CARRY  │ items  │ items  │ items  │ items  │  ← amber row  ││
│ │ ├─────────┼────────┼────────┼────────┼────────┤               ││
│ │ │🚫BLOCK  │ items  │ items  │ items  │ items  │  ← red row    ││
│ │ └─────────┴────────┴────────┴────────┴────────┘               ││
│ └────────────────────────────────────────────────────────────────┘│
└──────────────────────────────────────────────────────────────────┘
```

| Element | Data Binding | CSS Class Logic |
|---------|-------------|-----------------|
| Grid columns | `grid-template-columns: 160px repeat(@Data.Months.Count, 1fr)` | Dynamic `style` attribute if months ≠ 4; otherwise use CSS default |
| Column headers | `@foreach month in Data.Months` | `class="hm-col-hdr @(month == currentMonth ? "apr-hdr" : "")"` |
| Current month label | `@month ◀ Now` (when `month == Data.Project.CurrentMonth`) | `.apr-hdr { background:#FFF0D0; color:#C07700 }` |
| Row headers | Static labels with emoji prefix | `.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr` |
| Data cells | `Data.Shipped?[month]`, etc. | `class="@cellClass @(month == currentMonth ? "apr" : "")"` |
| Work items | `@foreach item in items` → `<div class="it">@item</div>` | `.it { font-size:12px }` with `::before` colored bullet |
| Empty cells | `items == null || items.Count == 0` → `<div style="color:#CCC">-</div>` | N/A |

**Row definition data structure in `@code`:**

```csharp
private record StatusRowConfig(
    string Label,
    string HeaderClass,
    string CellClass,
    Func<DashboardData, Dictionary<string, List<string>>?> GetItems
);

private static readonly StatusRowConfig[] Rows = new[]
{
    new StatusRowConfig("✅ Shipped",     "ship-hdr",  "ship-cell",  d => d.Shipped),
    new StatusRowConfig("🔄 In Progress", "prog-hdr",  "prog-cell",  d => d.InProgress),
    new StatusRowConfig("🔁 Carryover",   "carry-hdr", "carry-cell", d => d.Carryover),
    new StatusRowConfig("🚫 Blockers",    "block-hdr", "block-cell", d => d.Blockers),
};
```

**Interactions:** None. The heatmap is a static read-only grid.

---

### CSS Architecture Summary

**Single file:** `wwwroot/css/dashboard.css`

**No CSS isolation, no CSS modules, no Sass, no CSS-in-JS.**

**CSS specificity order:**
1. Reset (`* { margin:0; padding:0; box-sizing:border-box }`)
2. Body viewport lock (`body { width:1920px; height:1080px; overflow:hidden }`)
3. Layout sections (`.hdr`, `.tl-area`, `.hm-wrap`)
4. Grid structure (`.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`)
5. Row color themes (`.ship-*`, `.prog-*`, `.carry-*`, `.block-*`)
6. Content styling (`.it`, `.it::before`, `.sub`)
7. Current month highlight (`.apr-hdr`, `.ship-cell.apr`, `.prog-cell.apr`, `.carry-cell.apr`, `.block-cell.apr`)