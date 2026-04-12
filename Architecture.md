# Architecture

## Overview & Goals

This document defines the system architecture for the **Executive Reporting Dashboard**, a single-page Blazor Server application that renders project milestones, shipping status, and monthly execution progress at a fixed 1920×1080 viewport optimized for PowerPoint screenshot capture.

**Core architectural principles:**

1. **Zero-dependency simplicity** — No third-party NuGet packages, no database, no authentication. The entire data layer is a single `data.json` file.
2. **Visual fidelity** — The rendered output must be pixel-identical to `OriginalDesignConcept.html` at 1920×1080.
3. **One-command startup** — `dotnet run` → browser → screenshot → PowerPoint. No setup, no accounts, no infrastructure.
4. **Data-driven rendering** — 100% of displayed content is sourced from `wwwroot/data/data.json`. Changing the JSON changes the dashboard.
5. **Flat architecture** — A single `.razor` page, a single CSS file, a service, and a model. No over-engineering.

**Business goals supported:**

| Goal | Architectural Decision |
|------|----------------------|
| Reduce reporting prep time by 75% | JSON-edit → screenshot workflow; no manual slide construction |
| Consistent professional format | CSS ported verbatim from approved HTML design reference |
| Real-time status visibility | `dotnet watch run` enables live reload during editing |
| Zero tooling friction | No NuGet dependencies beyond .NET 8 SDK built-ins |
| Multi-project reuse | `data.json` schema is generic; any team can fork and edit |

---

## System Components

### Component 1: `Dashboard.razor` — Single Page Renderer

**Responsibility:** Renders the complete dashboard UI including all three visual sections (header, timeline, heatmap). This is the only routable page in the application.

**Interfaces:**
- Injects `DashboardDataService` to obtain the deserialized `DashboardConfig`
- Uses `@layout EmptyLayout` to suppress default Blazor chrome
- Route: `/` (default route, no navigation required)

**Dependencies:** `DashboardDataService`, `DashboardConfig` model

**Data:** Reads `DashboardConfig` on `OnInitializedAsync`. All rendering is server-side via Blazor Server's SignalR connection.

**Internal structure (regions, not components):**
- **Header region** — Title, subtitle, backlog link, legend icons
- **Timeline region** — SVG milestone visualization with date-to-pixel calculations
- **Heatmap region** — CSS Grid with color-coded status rows

**Key methods:**

```csharp
// Date-to-pixel conversion for SVG timeline positioning
private double DateToX(DateTime date)
{
    double totalDays = (timelineEnd - timelineStart).TotalDays;
    double dayOffset = (date - timelineStart).TotalDays;
    return Math.Clamp(dayOffset / totalDays * SvgWidth, 0, SvgWidth);
}

// Y-position for each milestone track (evenly spaced)
private double MilestoneY(int index)
{
    return 42 + (index * 56); // 42px initial offset, 56px spacing
}

// Diamond polygon points for PoC/Production markers
private string DiamondPoints(double cx, double cy, double size = 11)
{
    return $"{cx},{cy - size} {cx + size},{cy} {cx},{cy + size} {cx - size},{cy}";
}

// "NOW" line x-position from DateTime.Today
private double NowLineX => DateToX(DateTime.Today);
```

**Error handling:** Wraps the entire render in a `try`/`catch`. If `DashboardConfig` is `null` or loading failed, renders an error panel with the exception message instead of the dashboard content.

---

### Component 2: `DashboardDataService` — Data Access Layer

**Responsibility:** Reads, deserializes, and validates `wwwroot/data/data.json` on application startup. Provides the parsed `DashboardConfig` to consuming components.

**Interfaces:**
- Registered as a **Singleton** in DI (`builder.Services.AddSingleton<DashboardDataService>()`)
- Public method: `Task<DashboardConfig> GetDashboardConfigAsync()`
- Public property: `string? LoadError` — populated if JSON loading/parsing failed

**Dependencies:** `IWebHostEnvironment` (to resolve `wwwroot` path), `System.Text.Json`

**Data flow:**
1. On first call to `GetDashboardConfigAsync()`, reads `wwwroot/data/data.json` from disk
2. Deserializes using `System.Text.Json` with `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`
3. Validates required fields (`Title`, `Months`, `HeatmapRows`) — throws descriptive error if missing
4. Caches the result in a private field for subsequent requests
5. If deserialization fails, stores the error message in `LoadError` and returns `null`

```csharp
public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private DashboardConfig? _cachedConfig;
    public string? LoadError { get; private set; }

    public DashboardDataService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<DashboardConfig?> GetDashboardConfigAsync()
    {
        if (_cachedConfig != null) return _cachedConfig;

        var path = Path.Combine(_env.WebRootPath, "data", "data.json");
        try
        {
            var json = await File.ReadAllTextAsync(path);
            _cachedConfig = JsonSerializer.Deserialize<DashboardConfig>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (_cachedConfig?.Title == null || _cachedConfig?.Months == null)
                throw new InvalidDataException("Required fields 'title' and 'months' are missing.");

            return _cachedConfig;
        }
        catch (Exception ex)
        {
            LoadError = $"Failed to load data.json: {ex.Message}";
            return null;
        }
    }
}
```

---

### Component 3: `DashboardConfig` Model — Data Schema (POCOs)

**Responsibility:** Defines the C# type system for `data.json` deserialization. Serves as the contract between the JSON file and the Razor page.

**File:** `Models/DashboardConfig.cs`

```csharp
using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record DashboardConfig
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogLink { get; init; } = "";
    public string CurrentMonth { get; init; } = "";
    public List<string> Months { get; init; } = new();
    public List<Milestone> Milestones { get; init; } = new();
    public List<HeatmapRow> HeatmapRows { get; init; } = new();
}

public record Milestone
{
    public string Label { get; init; } = "";
    public string Description { get; init; } = "";
    public string Color { get; init; } = "#888";
    public List<MilestoneEvent> Events { get; init; } = new();
}

public record MilestoneEvent
{
    public string Date { get; init; } = "";
    public string Type { get; init; } = "checkpoint";
    public string Label { get; init; } = "";
}

public record HeatmapRow
{
    public string Category { get; init; } = "";

    [JsonPropertyName("label")]
    public string Label { get; init; } = "";

    public Dictionary<string, List<string>> Items { get; init; } = new();
}
```

**Design decisions:**
- `record` types for immutability — data is read-once, never mutated
- Default values on all properties to prevent null reference exceptions on partial JSON
- `MilestoneEvent.Date` is stored as `string` and parsed to `DateTime` in the Razor page's calculation methods, keeping the model serialization-friendly
- `HeatmapRow.Items` uses `Dictionary<string, List<string>>` where keys are month abbreviations ("Jan", "Feb", etc.) matching entries in the `Months` array

---

### Component 4: `EmptyLayout.razor` — Blank Layout

**Responsibility:** Replaces the default Blazor `MainLayout.razor` to eliminate the sidebar, nav menu, and all default template chrome. Renders only the page body.

**File:** `Layout/EmptyLayout.razor`

```razor
@inherits LayoutComponentBase

@Body
```

No wrapper `<div>`, no `<nav>`, no `<main>` — just the raw page content. This ensures the fixed 1920×1080 viewport is not disrupted by layout wrappers.

---

### Component 5: `dashboard.css` — Stylesheet

**Responsibility:** All visual styling for the dashboard. Ported directly from `OriginalDesignConcept.html`'s `<style>` block with CSS custom properties added for maintainability.

**File:** `wwwroot/css/dashboard.css`

**Structure:**
1. **CSS Custom Properties** (`:root` block) — Color palette, font stack, page dimensions
2. **Reset & Page Layout** — `*`, `body`, `a` base styles; fixed 1920×1080 viewport
3. **Header styles** — `.hdr`, `.sub`, legend items
4. **Timeline styles** — `.tl-area`, `.tl-svg-box`, milestone sidebar
5. **Heatmap styles** — `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`
6. **Row category variants** — `.ship-*`, `.prog-*`, `.carry-*`, `.block-*` with header, cell, current-month, and dot colors
7. **Error state** — `.error-panel` for JSON loading failures

---

### Component 6: `data.json` — Configuration File

**Responsibility:** The sole data source for all dashboard content. Edited by project leads to update the dashboard.

**File:** `wwwroot/data/data.json`

**Schema:** Maps 1:1 to `DashboardConfig` C# model (see Component 3).

**Constraints:**
- `months` array determines the number of heatmap columns (grid dynamically sizes to `repeat(N, 1fr)`)
- `currentMonth` value must exactly match one entry in the `months` array
- `milestones[].events[].date` must be ISO 8601 format (`YYYY-MM-DD`)
- `milestones[].events[].type` must be one of: `"checkpoint"`, `"poc"`, `"production"`
- `heatmapRows[].category` must be one of: `"shipped"`, `"in-progress"`, `"carryover"`, `"blockers"`
- `heatmapRows[].items` keys must match entries in the `months` array

---

### Component 7: `Program.cs` — Application Entry Point

**Responsibility:** Configures the Blazor Server application, registers services, maps routes, and starts Kestrel.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

**Key decisions:**
- `AddSingleton<DashboardDataService>()` — data is loaded once and cached; no per-request overhead
- No authentication middleware, no CORS, no additional middleware
- `MapFallbackToPage("/_Host")` — standard Blazor Server host page

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────┐    ┌──────────────────────┐    ┌────────────────────┐
│   data.json     │───>│ DashboardDataService  │───>│  Dashboard.razor   │
│ (wwwroot/data/) │    │  (Singleton, cached)  │    │  (Single Page)     │
└─────────────────┘    └──────────────────────┘    └────────────────────┘
                              │                            │
                              │ Reads file on first        │ Renders three sections:
                              │ request, caches result     │  ├─ Header (HTML)
                              │                            │  ├─ Timeline (SVG)
                              │                            │  └─ Heatmap (CSS Grid)
                              │                            │
                              │                     ┌──────┴───────┐
                              │                     │ EmptyLayout  │
                              │                     │ (@Body only) │
                              │                     └──────────────┘
                              │
                       ┌──────┴───────┐
                       │ DashboardConfig │
                       │ (POCO Model)    │
                       └────────────────┘
```

### Request Lifecycle

1. **Browser** navigates to `https://localhost:5001/`
2. **Kestrel** serves `_Host.cshtml` (Blazor Server host page)
3. **Blazor** establishes SignalR WebSocket connection
4. **Dashboard.razor** `OnInitializedAsync()` fires
5. **DashboardDataService** is injected; `GetDashboardConfigAsync()` is called
6. **First call:** Reads `wwwroot/data/data.json` from disk, deserializes to `DashboardConfig`, caches
7. **Subsequent calls:** Returns cached `DashboardConfig` immediately
8. **Dashboard.razor** receives `DashboardConfig`, calculates timeline positions, renders HTML/SVG
9. **Browser** receives the rendered DOM via SignalR and paints the 1920×1080 dashboard

### Error Flow

1. `DashboardDataService` catches `JsonException`, `FileNotFoundException`, `InvalidDataException`
2. Stores error message in `LoadError` property, returns `null`
3. `Dashboard.razor` checks for `null` config or non-null `LoadError`
4. Renders error panel with red background and descriptive message instead of dashboard
5. User fixes `data.json`, restarts app (or `dotnet watch` auto-restarts)

---

## Data Model

### Entity Relationship

```
DashboardConfig (root)
├── Title: string
├── Subtitle: string
├── BacklogLink: string
├── CurrentMonth: string ──────────────┐
├── Months: List<string> ◄────────────┤ (CurrentMonth must match one entry)
├── Milestones: List<Milestone>        │
│   ├── Label: string                  │
│   ├── Description: string            │
│   ├── Color: string (hex)            │
│   └── Events: List<MilestoneEvent>   │
│       ├── Date: string (ISO 8601)    │
│       ├── Type: string (enum-like)   │
│       └── Label: string              │
└── HeatmapRows: List<HeatmapRow>     │
    ├── Category: string (enum-like)   │
    ├── Label: string                  │
    └── Items: Dict<string, List<string>>
         └── Keys ─────────────────────┘ (must match Months entries)
```

### Storage

- **Format:** JSON flat file
- **Location:** `wwwroot/data/data.json`
- **Size constraint:** < 100KB (validated by design; 50 events + 32 heatmap cells ≈ 3-5KB)
- **Persistence:** File system only. No database, no API, no cache layer.
- **Concurrency:** Single-user, single-reader. No write contention.

### Validation Rules

| Field | Rule | Error Behavior |
|-------|------|---------------|
| `title` | Required, non-empty string | Error panel: "Required field 'title' is missing" |
| `months` | Required, non-empty array | Error panel: "Required field 'months' is missing" |
| `currentMonth` | Must match an entry in `months` | Current month column highlighting silently disabled |
| `milestones` | Optional, defaults to empty array | Timeline renders with grid lines only, no tracks |
| `milestones[].events[].date` | Must parse as `DateTime` | Event marker skipped with console warning |
| `milestones[].events[].type` | Must be "checkpoint", "poc", or "production" | Defaults to checkpoint rendering |
| `heatmapRows` | Optional, defaults to empty array | Heatmap grid renders headers only |
| `heatmapRows[].items[month]` | Key must match entry in `months` | Cell renders dash (–) for unmatched months |

---

## API Contracts

This application has **no REST API, no Web API controllers, and no HTTP endpoints** beyond the Blazor Server defaults. All data flows through the in-process `DashboardDataService` singleton.

### Internal Service Contract

```csharp
public interface IDashboardDataService
{
    /// Returns the parsed dashboard configuration, or null if loading failed.
    Task<DashboardConfig?> GetDashboardConfigAsync();

    /// Non-null if data.json loading or parsing failed. Contains the error message.
    string? LoadError { get; }
}
```

### Blazor Server Endpoints (Framework-Provided)

| Endpoint | Purpose | Notes |
|----------|---------|-------|
| `GET /` | Serves `_Host.cshtml` (Blazor host page) | MapFallbackToPage |
| `/_blazor` | SignalR WebSocket hub | MapBlazorHub, framework-managed |
| `/_framework/*` | Blazor framework JS files | Static files middleware |
| `/css/dashboard.css` | Dashboard stylesheet | Static files middleware |
| `/data/data.json` | Raw JSON file (publicly accessible) | Static files middleware |

### Error Response Contract

When `data.json` fails to load or parse, the page renders an inline error panel (not an HTTP error code):

```html
<div class="error-panel">
    <h2>Dashboard Configuration Error</h2>
    <p>Failed to load data.json: [specific error message]</p>
    <p>Please check wwwroot/data/data.json and restart the application.</p>
</div>
```

The error panel uses CSS class `.error-panel` with red border, light red background, and monospace font for the error detail.

---

## Infrastructure Requirements

### Runtime Requirements

| Requirement | Specification |
|-------------|---------------|
| **.NET SDK** | .NET 8.0.x LTS (any 8.0 patch version) |
| **Operating System** | Windows 10/11 (primary), macOS/Linux (secondary) |
| **Browser** | Chrome 120+ or Edge 120+ at 100% zoom |
| **Display** | 1920×1080 minimum resolution |
| **Font** | Segoe UI (pre-installed on Windows 10/11) |
| **Network** | Localhost only; no internet required at runtime |
| **Disk** | < 10MB for the project; < 100MB runtime memory |

### Hosting

- **Server:** Kestrel (built into .NET 8), localhost only
- **Port:** `https://localhost:5001` (configurable via `launchSettings.json`)
- **HTTPS:** Kestrel's default development certificate (`dotnet dev-certs https --trust` on first setup)
- **No reverse proxy**, no IIS, no Nginx, no Docker

### Build & Run

```bash
# First-time setup (one-time)
dotnet dev-certs https --trust

# Development (with live reload)
dotnet watch run

# Production-like run
dotnet run -c Release

# Self-contained publish (optional, for distribution)
dotnet publish -c Release -r win-x64 --self-contained
```

### CI/CD

Not required for v1. If added later:

```yaml
# Minimal GitHub Actions workflow
- dotnet restore
- dotnet build --no-restore
- dotnet publish -c Release
```

### Project File Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── _Imports.razor
    ├── App.razor
    ├── Pages/
    │   ├── _Host.cshtml
    │   └── Dashboard.razor
    ├── Layout/
    │   └── EmptyLayout.razor
    ├── Models/
    │   └── DashboardConfig.cs
    ├── Services/
    │   └── DashboardDataService.cs
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css
    │   └── data/
    │       └── data.json
    └── Properties/
        └── launchSettings.json
```

### `.csproj` Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**Zero NuGet package references.** All dependencies (`System.Text.Json`, `Microsoft.AspNetCore.*`, `Microsoft.Extensions.*`) are included in the .NET 8 SDK.

---

## Technology Stack Decisions

| Layer | Technology | Justification |
|-------|-----------|---------------|
| **Application Framework** | Blazor Server (.NET 8 LTS) | Mandatory per project constraints. Enables `dotnet run` one-command startup, `.sln` project structure, and C# model classes for type-safe JSON deserialization. |
| **CSS Layout** | Native CSS Grid + Flexbox | The original HTML design uses only Grid and Flexbox. Any UI framework (Bootstrap, MudBlazor) would conflict with the custom grid layout and add unnecessary bundle weight. |
| **Timeline Rendering** | Inline SVG in Razor | The design uses hand-drawn SVG elements (lines, circles, diamonds, text). Blazor's Razor syntax can emit SVG markup directly with `@foreach` loops and C# expressions for calculated positions. A charting library would fight the custom layout. |
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8, high-performance, zero dependencies. The data schema is flat and simple — no need for Newtonsoft.Json. |
| **Hosting** | Kestrel (built-in) | Local-only tool; no need for IIS, Nginx, or containerization. Kestrel starts in < 2 seconds. |
| **Styling Strategy** | Single CSS file with custom properties | CSS ported verbatim from the approved HTML design ensures pixel-fidelity. Custom properties enable easy color palette changes for reuse by other teams. |
| **State Management** | Singleton service with cached config | Data is read-once on first request. No Flux/Redux pattern, no state containers — the data is static for the lifetime of the process. |

### Technologies Explicitly Rejected

| Technology | Reason for Rejection |
|-----------|---------------------|
| MudBlazor / Radzen / Syncfusion | 500KB+ CSS overhead, theming conflicts with custom design, component abstraction hides CSS control needed for pixel-perfect rendering |
| Chart.js / ApexCharts | Timeline has ~20 SVG elements; a charting library adds 200KB+ and cannot replicate the exact marker shapes (diamonds, open circles) without extensive customization |
| Entity Framework | No database exists. `data.json` is the entire data layer. |
| Bootstrap / Tailwind | The design uses a custom grid (`160px repeat(N, 1fr)`) that doesn't map to Bootstrap's 12-column system. Tailwind's utility classes would duplicate the existing hand-written CSS. |
| Blazor WebAssembly | Adds WASM download time (~2-5MB), complicates the project structure, and provides no benefit for a localhost-only tool. Server-side rendering is faster for this use case. |
| SignalR customization | Blazor Server includes SignalR by default. Single-user localhost usage needs no tuning. |

---

## Security Considerations

### Threat Model

This is a **local-only, single-user, no-auth, no-internet** tool. The threat surface is minimal.

| Threat | Risk Level | Mitigation |
|--------|-----------|------------|
| Malicious `data.json` injection | None | The only user is the developer who edits the file. No untrusted input. |
| XSS via JSON content | Very Low | Blazor Server HTML-encodes all rendered strings by default. Even if `data.json` contains `<script>` tags, they render as escaped text. |
| CSRF / session hijacking | None | No authentication, no sessions, no cookies (beyond Blazor's anti-forgery token). |
| Data exfiltration | None | No network egress. No telemetry. No external API calls. |
| Supply chain attack | Very Low | Zero third-party NuGet packages. Only .NET 8 SDK built-ins. |
| Unauthorized access | None | Localhost-only. Kestrel binds to `127.0.0.1` by default. |

### Input Validation

The only external input is `data.json`. Validation strategy:

1. **JSON syntax validation:** `System.Text.Json` throws `JsonException` with line/column info on malformed JSON
2. **Schema validation:** Required fields (`Title`, `Months`) checked after deserialization; missing fields produce descriptive error
3. **Date parsing:** `MilestoneEvent.Date` values parsed via `DateTime.TryParse` — invalid dates are skipped with console warning
4. **Type enumeration:** `MilestoneEvent.Type` values outside `{"checkpoint", "poc", "production"}` default to checkpoint rendering
5. **Output encoding:** Blazor's default Razor rendering HTML-encodes all interpolated strings — no raw HTML injection risk

### HTTPS

Kestrel's development HTTPS certificate is used by default. For first-time setup: `dotnet dev-certs https --trust`. This is adequate for a localhost tool; no production TLS configuration is needed.

---

## Scaling Strategy

### Scaling Is Not a Concern

This is a local, single-user tool that renders a static page from a JSON file. There is no scaling requirement.

| Dimension | Approach |
|-----------|----------|
| **Users** | Single user per instance. No multi-user support needed. If multiple team members need dashboards, each clones the repo and runs locally. |
| **Data volume** | `data.json` is expected to be < 10KB. The model supports up to 10 milestones, 50 events, 6 months, and 8 items per heatmap cell. This fits in memory trivially. |
| **Concurrency** | Single SignalR connection (one browser tab). No connection pooling, no backplane. |
| **Multi-project** | v1 supports one `data.json` → one dashboard. Future enhancement: route parameter (`/dashboard/{project}`) mapping to `data/{project}.json`. The architecture supports this with a path parameter in `DashboardDataService`. |
| **Performance** | Page load target: < 1 second. JSON deserialization target: < 50ms. Both are trivially met with a singleton cached service and < 10KB data file. |

### Future Multi-Project Extension Point

If multi-project support is added in v2, the change is isolated to:

1. `DashboardDataService` — accept a `projectName` parameter, resolve to `wwwroot/data/{projectName}.json`
2. `Dashboard.razor` — add `@page "/dashboard/{ProjectName}"` route parameter
3. Cache strategy — change from single cached config to `Dictionary<string, DashboardConfig>`

No architectural changes required. The model, CSS, and rendering logic are project-agnostic.

---

## UI Component Architecture

The dashboard is a **single Razor page** with three distinct visual regions. Per the PM spec's maintainability requirement ("Single `.razor` page — no component fragmentation"), these are rendered as regions within `Dashboard.razor`, not as separate Blazor components.

### Visual Section → Implementation Mapping

#### Section 1: Header Bar → `Dashboard.razor` Header Region

**Visual reference:** `.hdr` div in `OriginalDesignConcept.html`

| Visual Element | Implementation | CSS Class | Data Binding |
|----------------|---------------|-----------|-------------|
| Project title | `<h1>` with inline `<a>` | `.hdr h1` | `@Config.Title`, `@Config.BacklogLink` |
| Subtitle | `<div>` below title | `.sub` | `@Config.Subtitle` |
| Legend: PoC Milestone | `<span>` with rotated `<span>` (12×12px, `#F4B400`, `rotate(45deg)`) | Inline styles | Static text |
| Legend: Production Release | `<span>` with rotated `<span>` (12×12px, `#34A853`, `rotate(45deg)`) | Inline styles | Static text |
| Legend: Checkpoint | `<span>` with `<span>` (8×8px, `border-radius: 50%`, `#999`) | Inline styles | Static text |
| Legend: Now indicator | `<span>` with `<span>` (2×14px, `#EA4335`) + "Now" text | Inline styles | Static text with dynamic month |

**CSS layout strategy:** `display: flex; align-items: center; justify-content: space-between`. Padding `12px 44px 10px`. Bottom border `1px solid #E0E0E0`.

**Interactions:** Backlog link is a real `<a href="@Config.BacklogLink">` — clickable when viewing live, decorative in screenshots.

---

#### Section 2: Timeline Area → `Dashboard.razor` Timeline Region

**Visual reference:** `.tl-area` div and inline `<svg>` in `OriginalDesignConcept.html`

| Visual Element | Implementation | Data Binding |
|----------------|---------------|-------------|
| Milestone sidebar labels | `<div>` per milestone, 230px fixed width | `@foreach (milestone in Config.Milestones)` → `@milestone.Label`, `@milestone.Description`, `style="color: @milestone.Color"` |
| Month grid lines | `<line>` SVG elements at calculated x-positions | `@foreach (month in timelineMonths)` → `x1="@MonthToX(month)"` |
| Month labels | `<text>` SVG elements | Month name text at each grid line x-position |
| Milestone track lines | `<line>` from x=0 to x=1560 at each milestone's y-position | `@foreach (milestone, index)` → `y1="@MilestoneY(index)"`, `stroke="@milestone.Color"` |
| Checkpoint circles | `<circle>` with white fill and colored stroke | `@evt.Type == "checkpoint"` → `cx="@DateToX(date)"`, `stroke="@milestone.Color"` |
| PoC diamonds | `<polygon>` with 4 points, fill `#F4B400`, drop shadow filter | `@evt.Type == "poc"` → `points="@DiamondPoints(x, y)"` |
| Production diamonds | `<polygon>` with 4 points, fill `#34A853`, drop shadow filter | `@evt.Type == "production"` → `points="@DiamondPoints(x, y)"` |
| Event date labels | `<text>` positioned near markers | `@evt.Label` at calculated position |
| NOW line | `<line>` dashed vertical, stroke `#EA4335` | `x1="@NowLineX"`, calculated from `DateTime.Today` |
| NOW label | `<text>` "NOW" in bold red | Positioned at `NowLineX` |

**CSS layout strategy:** `.tl-area` is `display: flex; align-items: stretch; height: 196px; background: #FAFAFA`. The SVG container (`.tl-svg-box`) is `flex: 1`.

**SVG rendering strategy:**
- SVG element: `width="1560" height="185" style="overflow: visible"`
- Drop shadow filter defined in `<defs>`: `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3">`
- Timeline months derived from the `data.json` milestone date range (first event date to last event date, rounded to month boundaries)
- Grid line spacing: `svgWidth / numberOfMonths` (260px per month for 6 months)
- Milestone Y-positions: `42 + (index * 56)` for up to 3 milestones
- Date-to-X calculation: linear interpolation from date range to SVG width

**Interactions:** None (static SVG, no hover effects or click handlers).

---

#### Section 3: Heatmap Grid → `Dashboard.razor` Heatmap Region

**Visual reference:** `.hm-wrap` and `.hm-grid` in `OriginalDesignConcept.html`

| Visual Element | Implementation | CSS Class | Data Binding |
|----------------|---------------|-----------|-------------|
| Section title | `<div>` uppercase text | `.hm-title` | Static: "Monthly Execution Heatmap" |
| Corner cell | `<div>` in grid position (1,1) | `.hm-corner` | Static text or empty |
| Month column headers | `<div>` per month in header row | `.hm-col-hdr`, `.hm-col-hdr.apr-hdr` (current) | `@foreach (month in Config.Months)` → `@month`, conditional class if `month == Config.CurrentMonth` |
| Row headers | `<div>` per category | `.hm-row-hdr`, `.ship-hdr` / `.prog-hdr` / `.carry-hdr` / `.block-hdr` | `@row.Label` with category-specific CSS class |
| Data cells | `<div>` per month×category | `.hm-cell`, `.ship-cell` / `.prog-cell` / `.carry-cell` / `.block-cell` | `@foreach (item in row.Items[month])` → `<div class="it">@item</div>` |
| Item bullets | `::before` pseudo-element (6×6px circle) | `.hm-cell .it::before` | Color set per row category via CSS |
| Empty cell placeholder | Dash character "–" in muted gray | Inline style `color: #AAA` | Rendered when `row.Items[month]` is empty or missing |
| Current month highlight | Darker background on cells in current month column | `.ship-cell.apr` / `.prog-cell.apr` / `.carry-cell.apr` / `.block-cell.apr` | Conditional CSS class when `month == Config.CurrentMonth` |

**CSS layout strategy:** `.hm-grid` uses `display: grid; grid-template-columns: 160px repeat(@Config.Months.Count, 1fr); grid-template-rows: 36px repeat(4, 1fr)`. The grid fills remaining viewport height via `flex: 1; min-height: 0` on `.hm-wrap`.

**Dynamic grid sizing:** The `grid-template-columns` value is set inline in the Razor markup: `style="grid-template-columns: 160px repeat(@Config.Months.Count, 1fr)"`. This allows the number of month columns to be driven entirely by `data.json`.

**Category-to-CSS class mapping (in C# helper method):**

```csharp
private string CategoryCssPrefix(string category) => category switch
{
    "shipped" => "ship",
    "in-progress" => "prog",
    "carryover" => "carry",
    "blockers" => "block",
    _ => "ship"
};
```

**Interactions:** None (static grid, no hover effects, no click handlers, no sorting/filtering).

---

#### Error State → `Dashboard.razor` Error Panel

**Visual reference:** Not in original design (error state is an architectural addition per US-4).

| Visual Element | Implementation | CSS Class |
|----------------|---------------|-----------|
| Error container | `<div>` centered in viewport | `.error-panel` |
| Error title | `<h2>` "Dashboard Configuration Error" | — |
| Error message | `<p>` with specific error detail | `font-family: monospace` |
| Recovery instructions | `<p>` with guidance | — |

**CSS:** `.error-panel` — `max-width: 600px; margin: 200px auto; padding: 32px; border: 2px solid #EA4335; background: #FFF5F5; border-radius: 8px;`

---

## Risks & Mitigations

### Risk 1: SVG Timeline Date-to-Pixel Calculation Complexity

**Likelihood:** Medium | **Impact:** Medium

**Description:** The original HTML design hardcodes pixel positions for all SVG elements. The dynamic version must calculate positions from ISO 8601 dates, mapping a date range to 0–1560px. Edge cases include events before the visible range, events after the visible range, and milestones with a single event.

**Mitigation:**
- Implement `DateToX()` as a pure function with `Math.Clamp` to handle out-of-range dates
- Derive timeline start/end from the `months` array, not from event dates, ensuring consistent grid alignment
- Timeline start = first day of first month in `months`; timeline end = last day of last month in `months` + 1 month (to show events in the month after the last displayed month)
- Use `Math.Clamp(calculatedX, 0, 1560)` to keep markers within bounds

### Risk 2: Screenshot Fidelity at 1920×1080

**Likelihood:** Medium | **Impact:** High

**Description:** The page must render identically to the HTML mockup at exactly 1920×1080. Browser zoom level, OS display scaling, and browser UI chrome can all affect the rendered output.

**Mitigation:**
- Fixed `body { width: 1920px; height: 1080px; overflow: hidden; }` — no responsive breakpoints
- CSS ported verbatim from the approved HTML design, minimizing visual drift
- Test at 100% browser zoom in Chrome/Edge with DevTools device toolbar set to 1920×1080
- Document the screenshot workflow: "Open Chrome → F12 → Toggle Device Toolbar → Set to 1920×1080 → Ctrl+Shift+P → Capture full size screenshot"

### Risk 3: Malformed `data.json` Crashes the Page

**Likelihood:** Medium | **Impact:** Medium

**Description:** If a project lead edits `data.json` incorrectly (trailing comma, missing bracket, wrong field name), the page could render blank or throw an unhandled exception.

**Mitigation:**
- `DashboardDataService` wraps all JSON operations in try/catch
- `JsonException` provides line number and byte position — surfaced in the error panel
- Missing optional fields (`milestones`, `heatmapRows`) default to empty collections via model defaults
- Required fields (`title`, `months`) validated post-deserialization with descriptive error messages

### Risk 4: Blazor Server SignalR Overhead

**Likelihood:** Low | **Impact:** Low

**Description:** Blazor Server maintains a persistent SignalR connection, which is overkill for a page that renders once and never changes.

**Mitigation:**
- Acceptable trade-off. The SignalR connection adds < 1KB/s overhead on localhost.
- The alternative (Blazor WebAssembly) adds 2-5MB WASM download time and complicates the project structure.
- The alternative (static HTML generation) loses the `dotnet run` workflow and .sln structure requirement.

### Risk 5: Heatmap Grid Overflow with Many Items

**Likelihood:** Low | **Impact:** Medium

**Description:** If a heatmap cell contains more than ~8 items, the text overflows the cell's allocated height (each row is `1fr` of the remaining viewport space after header/timeline/section-title).

**Mitigation:**
- CSS `overflow: hidden` on `.hm-cell` prevents visual overflow
- Document in `data.json` schema: "Each cell supports up to 8 items at 12px font. Additional items are clipped."
- For power users: reduce `font-size` in CSS or increase `page-height` beyond 1080px (with corresponding screenshot adjustment)

### Risk 6: Dynamic Month Column Count Breaks Grid Layout

**Likelihood:** Low | **Impact:** Medium

**Description:** The original design uses 4 months. If `data.json` specifies 6+ months, column widths may become too narrow for item text.

**Mitigation:**
- Grid uses `repeat(N, 1fr)` — columns auto-size proportionally
- 4 months: ~380px per column (comfortable). 6 months: ~253px per column (acceptable). 8+ months: may require smaller font.
- Document recommended range: 3–6 months in the `data.json` schema comments