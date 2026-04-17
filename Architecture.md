# Architecture

**Document Version:** 1.0 · **Date:** April 17, 2026 · **Status:** Approved
**Design Reference:** `OriginalDesignConcept.html` · **Stack:** C# .NET 8, Blazor Server, localhost-only

---

## Overview & Goals

This architecture defines a **single-page executive reporting dashboard** built as a .NET 8 Blazor Server application. The dashboard renders a project milestone timeline (inline SVG) and a monthly execution heatmap (CSS Grid) at a fixed 1920×1080 viewport, optimized for PowerPoint screenshot capture. All data is read from a local `data.json` file. There is no authentication, no database, no cloud infrastructure, and zero external NuGet dependencies.

**Architecture Principles:**

1. **Deliberately minimal.** This is a data visualization page, not a web application. Every architectural decision optimizes for simplicity and file count (target: <15 files).
2. **Data-driven rendering.** The PM edits `data.json`; the dashboard renders it. No code changes required for data updates.
3. **Pixel-perfect fidelity.** The rendered output at 1920×1080 must be visually indistinguishable from `OriginalDesignConcept.html`.
4. **Zero external dependencies.** Only packages shipping with the .NET 8 SDK are permitted.
5. **Localhost-only.** Kestrel binds to `localhost`. No network exposure.

**Data Flow (end-to-end):**

```
wwwroot/data/data.json
        │
        ▼
 DashboardDataService          (Reads file, deserializes via System.Text.Json)
        │
        ▼
 Dashboard.razor               (Page component, composes sub-components)
   ├── HeaderSection.razor      (Title, subtitle, legend)
   ├── TimelineSection.razor    (Inline SVG with date math)
   └── HeatmapSection.razor     (CSS Grid with color-coded cells)
        │
        ▼
 Browser @ 1920×1080            (Chrome, screenshot capture)
```

---

## System Components

### 1. `Program.cs` — Application Entry Point

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Configure Kestrel, register services, set up middleware, map Blazor hub |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService`, ASP.NET Core middleware pipeline |
| **Data** | None directly |

**Implementation specification:**

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

**Key decisions:**
- `AddSingleton<DashboardDataService>()` — service is stateless and reads files on each request (no caching across requests, ensuring `data.json` edits are picked up on refresh).
- No `app.UseAuthentication()` or `app.UseAuthorization()` — explicitly omitted.
- No `builder.WebHost.UseUrls()` override — use `launchSettings.json` for port config (`http://localhost:5000`).

---

### 2. `Models/DashboardData.cs` — Data Model Records

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Strongly-typed C# records matching the `data.json` schema |
| **Interfaces** | Consumed by `DashboardDataService` (deserialization target) and all Razor components (parameter types) |
| **Dependencies** | None |
| **Data** | Immutable records with `required` properties |

**Full record definitions:**

```csharp
namespace ReportingDashboard.Models;

using System.Text.Json.Serialization;

public record DashboardData
{
    public required int SchemaVersion { get; init; }
    public required HeaderInfo Header { get; init; }
    public required TimelineData Timeline { get; init; }
    public required HeatmapData Heatmap { get; init; }
}

public record HeaderInfo
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string BacklogLink { get; init; }
    public required string CurrentDate { get; init; }
}

public record TimelineData
{
    public required string StartDate { get; init; }
    public required string EndDate { get; init; }
    public required List<TimelineTrack> Tracks { get; init; }
}

public record TimelineTrack
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Description { get; init; }
    public required string Color { get; init; }
    public required List<Milestone> Milestones { get; init; }
}

public record Milestone
{
    public required string Label { get; init; }
    public required string Date { get; init; }
    public required string Type { get; init; }  // "checkpoint", "poc", "production"
}

public record HeatmapData
{
    public required List<string> Columns { get; init; }
    public string? HighlightColumn { get; init; }
    public required List<HeatmapRow> Rows { get; init; }
}

public record HeatmapRow
{
    public required string Category { get; init; }
    public required string CategoryStyle { get; init; }  // "ship", "prog", "carry", "block"
    public required List<List<string>> Cells { get; init; }
}
```

**Design decisions:**
- **`string` for dates** (not `DateOnly`): `System.Text.Json` in .NET 8 handles `DateOnly` deserialization, but using `string` with explicit `DateOnly.Parse()` in the service layer gives clearer error messages on malformed dates and avoids JSON serializer configuration.
- **`required` keyword on all properties**: Forces `System.Text.Json` to throw `JsonException` with a specific property name if any field is missing from the JSON, providing actionable error messages.
- **`string?` for `HighlightColumn`**: Optional — if omitted, the service defaults to the current month name.
- **No `[JsonPropertyName]` attributes needed**: The JSON uses camelCase, and `System.Text.Json` defaults to camelCase deserialization with `PropertyNameCaseInsensitive = true`.

---

### 3. `Services/DashboardDataService.cs` — Data Loading Service

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Read `data.json` from disk, deserialize, validate schema version, sanitize file paths for `?data=` query parameter |
| **Interfaces** | `LoadAsync(string? filename)` → returns `DashboardData` or throws descriptive exception |
| **Dependencies** | `IWebHostEnvironment` (for `WebRootPath`), `System.Text.Json` |
| **Data** | Reads from `wwwroot/data/*.json` |

**Implementation specification:**

```csharp
namespace ReportingDashboard.Services;

using System.Text.Json;
using ReportingDashboard.Models;

public class DashboardDataService
{
    private readonly string _dataDirectory;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataDirectory = Path.Combine(env.WebRootPath, "data");
    }

    public async Task<DashboardData> LoadAsync(string? filename = null)
    {
        var targetFile = SanitizeFilename(filename ?? "data.json");
        var filePath = Path.Combine(_dataDirectory, targetFile);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"{targetFile} not found. Place your data file at wwwroot/data/{targetFile}.");
        }

        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions)
            ?? throw new InvalidOperationException("Deserialization returned null.");

        if (data.SchemaVersion != 1)
        {
            throw new InvalidOperationException(
                $"Unsupported schema version: {data.SchemaVersion}. Expected: 1.");
        }

        return data;
    }

    private static string SanitizeFilename(string filename)
    {
        // Reject directory traversal attempts
        if (filename.Contains("..") || filename.Contains('/') || filename.Contains('\\'))
        {
            throw new ArgumentException(
                $"Invalid filename: '{filename}'. Path separators and '..' are not allowed.");
        }

        // Must end in .json
        if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Invalid filename: '{filename}'. Only .json files are supported.");
        }

        return filename;
    }
}
```

**Key behaviors:**
- **No caching.** Each page load reads the file fresh from disk, ensuring edits to `data.json` are reflected on browser refresh without restarting the app.
- **Path sanitization.** The `?data=` query parameter is stripped of `..`, `/`, and `\` characters. Only flat filenames within `wwwroot/data/` are allowed.
- **Descriptive exceptions.** `FileNotFoundException` and `JsonException` messages are surfaced to the user in the error UI (see `Dashboard.razor` error handling).
- **Registered as Singleton** in DI, but stateless — safe for concurrent access since it creates no shared mutable state.

---

### 4. `Components/Pages/Dashboard.razor` — Main Page Component

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Route handler for `/`, orchestrates data loading, handles `?data=` query parameter, composes sub-components, renders error states |
| **Interfaces** | Blazor page route `/` |
| **Dependencies** | `DashboardDataService`, `NavigationManager` |
| **Data** | Holds loaded `DashboardData` or error message string |

**Component structure:**

```razor
@page "/"
@inject DashboardDataService DataService
@inject NavigationManager Navigation

@if (!string.IsNullOrEmpty(_errorMessage))
{
    <div class="error-container">
        <p class="error-message">@_errorMessage</p>
    </div>
}
else if (_data is not null)
{
    <HeaderSection Header="_data.Header" />
    <TimelineSection Timeline="_data.Timeline"
                     CurrentDate="@_currentDate" />
    <HeatmapSection Heatmap="_data.Heatmap" />
}

@code {
    private DashboardData? _data;
    private string? _errorMessage;
    private DateOnly _currentDate;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var uri = new Uri(Navigation.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var filename = query["data"];

            _data = await DataService.LoadAsync(filename);
            _currentDate = DateOnly.Parse(_data.Header.CurrentDate);
        }
        catch (FileNotFoundException ex)
        {
            _errorMessage = ex.Message;
        }
        catch (JsonException ex)
        {
            _errorMessage = $"Failed to load dashboard data: {ex.Message}";
        }
        catch (Exception ex)
        {
            _errorMessage = $"Unexpected error: {ex.Message}";
        }
    }
}
```

**Error handling strategy:**
- `FileNotFoundException` → "data.json not found" message with path guidance.
- `JsonException` → "Failed to load dashboard data" with the JSON parse error (line/position).
- `ArgumentException` (from path sanitization) → "Invalid filename" message.
- All errors render in a centered `.error-container` div using the same Segoe UI font.
- No stack traces are shown to the user.

---

### 5. `Components/HeaderSection.razor` — Header Bar Component

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Render project title with ADO backlog link, subtitle, and legend symbols |
| **Interfaces** | `[Parameter] HeaderInfo Header` |
| **Dependencies** | None |
| **Data** | Read-only from `HeaderInfo` |

**Parameter contract:**

```csharp
[Parameter, EditorRequired]
public required HeaderInfo Header { get; set; }
```

**Rendering logic:**
- Title: `<h1>` with inline `<a href="@Header.BacklogLink">` styled `#0078D4`.
- Subtitle: `<div class="sub">@Header.Subtitle</div>`.
- Legend: Four inline `<span>` elements with CSS-shaped symbols (diamonds via `transform: rotate(45deg)`, circle via `border-radius: 50%`, rectangle for NOW line).
- All markup mirrors the `.hdr` section of `OriginalDesignConcept.html` exactly.

---

### 6. `Components/TimelineSection.razor` — SVG Timeline Component

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Render the milestone timeline as inline SVG: month grid, track lines, milestone markers, NOW line |
| **Interfaces** | `[Parameter] TimelineData Timeline`, `[Parameter] DateOnly CurrentDate` |
| **Dependencies** | None |
| **Data** | Read-only from `TimelineData` |

**Parameter contract:**

```csharp
[Parameter, EditorRequired]
public required TimelineData Timeline { get; set; }

[Parameter, EditorRequired]
public required DateOnly CurrentDate { get; set; }
```

**SVG rendering logic (C# code block):**

```csharp
private const double SvgWidth = 1560.0;
private const double SvgHeight = 185.0;

private double DateToX(DateOnly date)
{
    var start = DateOnly.Parse(Timeline.StartDate);
    var end = DateOnly.Parse(Timeline.EndDate);
    var totalDays = end.DayNumber - start.DayNumber;
    if (totalDays <= 0) return 0;

    var dayOffset = date.DayNumber - start.DayNumber;
    var ratio = (double)dayOffset / totalDays;

    // Clamp to SVG bounds
    return Math.Clamp(ratio * SvgWidth, 0, SvgWidth);
}

private double TrackY(int trackIndex, int trackCount)
{
    // Distribute tracks evenly: first at ~42px, last at ~154px
    var usableHeight = SvgHeight - 60;  // top/bottom margins
    var spacing = trackCount > 1 ? usableHeight / (trackCount - 1) : 0;
    return 42 + (trackIndex * spacing);
}

private string DiamondPoints(double cx, double cy, double radius = 11)
{
    return $"{cx},{cy - radius} {cx + radius},{cy} {cx},{cy + radius} {cx - radius},{cy}";
}
```

**SVG elements rendered:**
1. **Month grid lines:** `@for` loop generating 6 vertical `<line>` elements at `DateToX(firstOfMonth)` intervals, with `<text>` month abbreviations.
2. **Track lines:** `@foreach` over `Timeline.Tracks`, rendering `<line x1="0" y1="@TrackY(i)" x2="1560" y2="@TrackY(i)" stroke="@track.Color" stroke-width="3"/>`.
3. **Milestone markers per track:**
   - `type == "checkpoint"`: `<circle cx="@x" cy="@y" r="7" fill="white" stroke="@track.Color" stroke-width="2.5"/>`
   - `type == "poc"`: `<polygon points="@DiamondPoints(x, y)" fill="#F4B400" filter="url(#sh)"/>`
   - `type == "production"`: `<polygon points="@DiamondPoints(x, y)" fill="#34A853" filter="url(#sh)"/>`
4. **Date labels:** `<text>` positioned above or below each marker (alternating by index to avoid overlap).
5. **NOW line:** `<line x1="@nowX" y1="0" x2="@nowX" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>` with `<text>NOW</text>`.
6. **Drop shadow filter:** Single `<defs><filter id="sh">` in the SVG, referenced by diamond milestones.

**Edge case handling:**
- Milestones before `startDate` are clamped to x=0.
- Milestones after `endDate` are clamped to x=1560.
- If `endDate == startDate`, all milestones render at x=0 (prevents division by zero).

---

### 7. `Components/HeatmapSection.razor` — CSS Grid Heatmap Component

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Render the monthly execution heatmap as a CSS Grid with color-coded rows and highlighted column |
| **Interfaces** | `[Parameter] HeatmapData Heatmap` |
| **Dependencies** | None |
| **Data** | Read-only from `HeatmapData` |

**Parameter contract:**

```csharp
[Parameter, EditorRequired]
public required HeatmapData Heatmap { get; set; }
```

**Rendering logic:**

The CSS Grid `grid-template-columns` is set dynamically:

```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Heatmap.Columns.Count, 1fr);">
```

**Grid cell rendering (pseudocode):**

```
Row 0: Corner cell ("STATUS") + Column headers (month names)
  - If column name == HighlightColumn → add "apr-hdr" CSS class (gold background)
Row 1-4: Row header (category name) + Data cells
  - Row header: CSS class = "{categoryStyle}-hdr" (e.g., "ship-hdr")
  - Data cell: CSS class = "{categoryStyle}-cell" + optional highlight class
  - If cell items is empty → render dash "-" in #AAA
  - If cell has items → render each as <div class="it">item name</div>
```

**Highlight column logic:**

```csharp
private string ResolveHighlightColumn()
{
    if (!string.IsNullOrEmpty(Heatmap.HighlightColumn))
        return Heatmap.HighlightColumn;

    // Default: current month name
    return DateTime.Now.ToString("MMMM");
}

private bool IsHighlighted(string columnName)
{
    return string.Equals(columnName, ResolveHighlightColumn(),
        StringComparison.OrdinalIgnoreCase);
}
```

**Cell CSS class resolution:**

```csharp
private string CellClass(string categoryStyle, string columnName)
{
    var baseClass = $"{categoryStyle}-cell";
    return IsHighlighted(columnName) ? $"{baseClass} apr" : baseClass;
}
```

> Note: The CSS class `apr` is reused from the design reference for any highlighted column, regardless of actual month name. The CSS defines the highlight colors per category.

---

### 8. `Components/Layout/MainLayout.razor` — Layout Wrapper

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Minimal layout that renders the `@Body` with no chrome, no nav, no Blazor error UI |
| **Interfaces** | Blazor `LayoutComponentBase` |
| **Dependencies** | None |

```razor
@inherits LayoutComponentBase

@Body
```

**No `<div class="page">` wrapper, no sidebar, no `<NavMenu>`.** The layout is intentionally empty to ensure the dashboard fills the full 1920×1080 viewport.

---

### 9. `wwwroot/css/dashboard.css` — Stylesheet

| Attribute | Value |
|-----------|-------|
| **Responsibility** | All visual styling for the dashboard, ported from `OriginalDesignConcept.html` |
| **Interfaces** | Referenced in `App.razor` `<head>` |
| **Dependencies** | None |
| **Size** | ~200 lines |

**CSS architecture:**
- **CSS custom properties** in `:root` for the full color palette (40+ variables).
- **No CSS modules, no scoped CSS, no CSS-in-JS.** Single flat file.
- **Class names match the original design** (`.hdr`, `.sub`, `.tl-area`, `.hm-wrap`, `.hm-grid`, `.hm-cell`, `.it`, etc.).
- **`body` fixed at 1920×1080** with `overflow: hidden`.
- **`@media print` rules** for browser print (hide Blazor reconnection UI, ensure colors print).

**Critical CSS rules:**

```css
* { margin: 0; padding: 0; box-sizing: border-box; }

body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
    background: var(--bg-white);
    font-family: 'Segoe UI', Arial, sans-serif;
    color: var(--text-primary);
    display: flex;
    flex-direction: column;
}

/* Suppress Blazor reconnection overlay */
#components-reconnect-modal { display: none !important; }

@media print {
    body { width: auto; height: auto; overflow: visible; }
    #components-reconnect-modal { display: none !important; }
}
```

---

### 10. `wwwroot/data/data.json` — Dashboard Data File

| Attribute | Value |
|-----------|-------|
| **Responsibility** | Source of all dashboard content — header metadata, timeline tracks/milestones, heatmap rows |
| **Interfaces** | Read by `DashboardDataService` |
| **Dependencies** | Must conform to `DashboardData` record schema |
| **Size** | Typically 2-10KB; supports up to 500KB |

Ships with fictional "Project Phoenix" example data: 3 timeline tracks, 4 months of heatmap data, 4 status categories with realistic work item names.

---

### 11. `App.razor` / `_Imports.razor` / `ReportingDashboard.csproj` — Scaffolding Files

**`App.razor`:** Hosts `<HeadContent>` with `<link href="css/dashboard.css" rel="stylesheet" />`. References `<Routes />` and `<script src="_framework/blazor.web.js" />`.

**`_Imports.razor`:** Global `@using` for `ReportingDashboard.Models`, `ReportingDashboard.Services`, `Microsoft.AspNetCore.Components.Web`.

**`ReportingDashboard.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**Zero `<PackageReference>` elements.** Everything needed ships with the SDK.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        Browser (Chrome)                      │
│  http://localhost:5000?data=project-a.json                  │
└─────────────────┬───────────────────────────────────────────┘
                  │ HTTP GET /
                  ▼
┌─────────────────────────────────────────────────────────────┐
│  Kestrel → Blazor Server Pipeline                           │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Dashboard.razor (Page Component)                      │  │
│  │  1. Extract ?data= from NavigationManager.Uri          │  │
│  │  2. Call DataService.LoadAsync(filename)                │  │
│  │  3. On success: set _data, render sub-components       │  │
│  │  4. On failure: set _errorMessage, render error div    │  │
│  │                                                         │  │
│  │  ┌──────────────┐ ┌────────────────┐ ┌──────────────┐ │  │
│  │  │ HeaderSection│ │TimelineSection │ │HeatmapSection│ │  │
│  │  │              │ │                │ │              │  │  │
│  │  │ HeaderInfo   │ │ TimelineData   │ │ HeatmapData  │ │  │
│  │  │ (Parameter)  │ │ CurrentDate    │ │ (Parameter)  │ │  │
│  │  │              │ │ (Parameters)   │ │              │  │  │
│  │  └──────────────┘ └────────────────┘ └──────────────┘ │  │
│  └───────────────────────────────────────────────────────┘  │
│                          │                                    │
│                          ▼                                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  DashboardDataService (Singleton)                      │  │
│  │  1. SanitizeFilename(filename)                         │  │
│  │  2. File.ReadAllTextAsync(wwwroot/data/{filename})     │  │
│  │  3. JsonSerializer.Deserialize<DashboardData>(json)    │  │
│  │  4. Validate schemaVersion == 1                        │  │
│  │  5. Return DashboardData                               │  │
│  └───────────────────────────────────────────────────────┘  │
│                          │                                    │
│                          ▼                                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  wwwroot/data/data.json  (or custom filename)          │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Communication Patterns

| From | To | Mechanism | Data |
|------|----|-----------|------|
| Browser | Kestrel | HTTP GET `/` | Query string `?data=filename.json` |
| Kestrel | Blazor pipeline | ASP.NET middleware | Request context |
| `Dashboard.razor` | `DashboardDataService` | DI injection + method call | `LoadAsync(filename)` → `DashboardData` |
| `DashboardDataService` | File system | `File.ReadAllTextAsync` | JSON string |
| `Dashboard.razor` | Sub-components | Blazor `[Parameter]` binding | `HeaderInfo`, `TimelineData`, `HeatmapData` |
| Blazor Server | Browser | SignalR (automatic) | Rendered HTML diff |

### Lifecycle Sequence

1. User navigates to `http://localhost:5000` (or with `?data=` parameter).
2. Blazor Server establishes SignalR circuit.
3. `Dashboard.razor.OnInitializedAsync()` fires.
4. Query parameter is extracted from `NavigationManager.Uri`.
5. `DashboardDataService.LoadAsync()` reads and deserializes `data.json`.
6. `DashboardData` is stored in component state.
7. Blazor renders `HeaderSection`, `TimelineSection`, `HeatmapSection` with parameter binding.
8. Rendered HTML is sent to the browser via SignalR.
9. Browser paints the dashboard. Total time: <2 seconds.

---

## Data Model

### Entity Relationship

```
DashboardData (root)
├── schemaVersion: int (always 1)
├── HeaderInfo (1:1)
│   ├── title: string
│   ├── subtitle: string
│   ├── backlogLink: string (URL)
│   └── currentDate: string (ISO 8601 date, e.g., "2026-04-17")
├── TimelineData (1:1)
│   ├── startDate: string (ISO 8601 date)
│   ├── endDate: string (ISO 8601 date)
│   └── TimelineTrack[] (1:N, max 10)
│       ├── id: string (unique track identifier, e.g., "m1")
│       ├── label: string (display label, e.g., "M1")
│       ├── description: string (track description)
│       ├── color: string (CSS hex color, e.g., "#0078D4")
│       └── Milestone[] (1:N, max 20 per track)
│           ├── label: string (display text)
│           ├── date: string (ISO 8601 date)
│           └── type: string ("checkpoint" | "poc" | "production")
└── HeatmapData (1:1)
    ├── columns: string[] (month names, 1-8 entries)
    ├── highlightColumn: string? (optional, defaults to current month)
    └── HeatmapRow[] (exactly 4)
        ├── category: string ("Shipped", "In Progress", "Carryover", "Blockers")
        ├── categoryStyle: string ("ship", "prog", "carry", "block")
        └── cells: string[][] (one inner array per column, items are work item names)
```

### Storage

| Aspect | Detail |
|--------|--------|
| **Format** | JSON file on local filesystem |
| **Location** | `wwwroot/data/data.json` (default) or `wwwroot/data/{custom}.json` |
| **Max size** | 500KB (soft limit; no enforcement, just tested performance boundary) |
| **Persistence** | File is edited manually by PM in a text editor |
| **Versioning** | Git-trackable; `schemaVersion` field for format evolution |
| **Backup** | User responsibility (Git, file copy) |

### Schema Validation Rules

| Rule | Enforcement |
|------|-------------|
| `schemaVersion` must equal `1` | Checked in `DashboardDataService.LoadAsync()` after deserialization |
| All `required` properties must be present | `System.Text.Json` throws `JsonException` with property name |
| `heatmap.rows[].cells.Length` must equal `heatmap.columns.Length` | Validated in `HeatmapSection.razor` — renders empty cells for missing entries |
| `milestone.type` must be `"checkpoint"`, `"poc"`, or `"production"` | Handled in `TimelineSection.razor` — unknown types render as small gray dots |
| Dates must be valid ISO 8601 format | `DateOnly.Parse()` throws `FormatException`, caught and surfaced as error message |

---

## API Contracts

This application has **no REST API, no GraphQL, no WebSocket endpoints**. It is a server-rendered Blazor application with a single page route.

### Route Contract

| Route | Method | Parameters | Response |
|-------|--------|------------|----------|
| `/` | GET | `?data={filename}` (optional query parameter) | Full-page Blazor Server HTML (1920×1080 dashboard or error message) |

### Query Parameter: `?data=`

| Aspect | Specification |
|--------|---------------|
| **Name** | `data` |
| **Required** | No. Defaults to `data.json` if omitted. |
| **Value** | Filename only (e.g., `project-b.json`). No path separators. |
| **Validation** | Must not contain `..`, `/`, or `\`. Must end in `.json`. |
| **Error on invalid** | Renders inline error message: "Invalid filename: '{value}'. Path separators and '..' are not allowed." |
| **Error on missing file** | Renders inline error message: "{filename} not found. Place your data file at wwwroot/data/{filename}." |

### Error Response Contract

Errors are rendered as HTML within the page (not as HTTP error codes). The SignalR circuit always returns HTTP 200. Error states render a centered `<div class="error-container">` with:

```html
<div class="error-container">
    <p class="error-message">{error text}</p>
</div>
```

| Error Condition | Message Template |
|-----------------|-----------------|
| File not found | `"{filename} not found. Place your data file at wwwroot/data/{filename}."` |
| Invalid JSON syntax | `"Failed to load dashboard data: '{' is invalid after a value. Path: $.header | LineNumber: 5 | BytePositionInLine: 2."` (actual `JsonException.Message`) |
| Missing required field | `"Failed to load dashboard data: JSON deserialization for type 'DashboardData' was missing required properties including: 'header'."` |
| Invalid filename | `"Invalid filename: '../etc/passwd'. Path separators and '..' are not allowed."` |
| Schema version mismatch | `"Unsupported schema version: 2. Expected: 1."` |

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|---------------|
| **Operating System** | Windows 10/11 |
| **Runtime** | .NET 8 SDK (8.0.x) |
| **Web Server** | Kestrel (built-in, no IIS/nginx/Apache) |
| **Browser** | Chrome (latest), 1920×1080 viewport |
| **Font** | Segoe UI (pre-installed on Windows) |

### Network

| Aspect | Specification |
|--------|---------------|
| **Binding** | `localhost` only (127.0.0.1) |
| **Ports** | `http://localhost:5000` (HTTP), `https://localhost:5001` (HTTPS, optional) |
| **External network** | Zero outbound connections. No CDN, no API calls, no telemetry. |
| **Firewall** | No ports need to be opened. |

### Storage

| Aspect | Specification |
|--------|---------------|
| **Disk space** | <5MB for project files + .NET SDK (~500MB, pre-installed) |
| **Data files** | `wwwroot/data/*.json`, each <500KB |
| **Temp files** | None created by the application |
| **Database** | None |

### CI/CD

**Not required for MVP.** The application is built and run locally via:

```bash
dotnet build        # Compile
dotnet run          # Start on localhost
dotnet watch        # Start with hot reload
```

If CI/CD is added later, a single `dotnet build` step is sufficient. No Docker, no deployment pipeline, no artifact publishing.

---

## Technology Stack Decisions

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Runtime** | .NET 8 | 8.0.x | Mandated by project requirements. LTS release with Blazor Server support. |
| **UI Framework** | Blazor Server | .NET 8.0 | Mandated. Provides C# model binding, component composition, hot reload. No JS required. |
| **Rendering: Timeline** | Inline SVG in Razor | SVG 1.1 | ~15 SVG primitives total. Hand-written SVG gives pixel-perfect control. A charting library would add 200KB+ JS and less positioning control. |
| **Rendering: Heatmap** | CSS Grid | CSS3 | Directly maps to the design's `grid-template-columns: 160px repeat(N,1fr)`. Native browser support, no library needed. |
| **Rendering: Layout** | CSS Flexbox | CSS3 | Used for header bar, timeline sidebar, and vertical page layout. Matches the original design's approach exactly. |
| **Styling** | Single `dashboard.css` with CSS custom properties | CSS3 | ~200 lines. Custom properties enable future theming (dark mode). No CSS framework needed for a fixed-layout screenshot tool. |
| **JSON** | `System.Text.Json` | Built-in | Default .NET 8 serializer. Faster and lighter than Newtonsoft. Handles this simple schema without configuration. |
| **Data Models** | C# records with `required` | C# 12 | Immutable, concise, catches missing JSON fields at deserialization time. Perfect for read-only display data. |
| **Web Server** | Kestrel | Built-in | Default for .NET 8. No reverse proxy needed for localhost. |
| **Font** | Segoe UI (system) | — | Ships with Windows. No web font loading, no external CDN, no FOUT. |

### What Was Explicitly Rejected

| Technology | Reason for Rejection |
|-----------|---------------------|
| **Bootstrap / Tailwind** | Fixed 1920×1080 layout. CSS frameworks add unnecessary classes and fight the fixed layout. The design is achievable in ~200 lines of hand-written CSS. |
| **Chart.js / Plotly / Radzen** | The timeline is ~15 SVG elements. A charting library adds 200KB+ JS, a learning curve, and less positioning control. |
| **Newtonsoft.Json** | `System.Text.Json` is built-in, faster, and sufficient. Zero reason to add a dependency. |
| **Entity Framework / SQLite** | No database is needed. Data is a single JSON file. |
| **JavaScript / TypeScript** | The entire UI is implementable in Razor + CSS + SVG. No JS interop required. |
| **Docker** | Adds complexity without benefit for a localhost-only tool. |
| **React / Vue / Angular** | Mandated stack is Blazor Server. |

---

## Security Considerations

### Authentication & Authorization

**None.** This is by design. The application:
- Binds to `localhost` only — not accessible from the network.
- Has a single user (the PM taking screenshots).
- Displays no sensitive data (project names, dates, work item titles only).
- Has no user accounts, no roles, no tokens.

**Future consideration:** If the dashboard is ever shared beyond localhost, add `Microsoft.AspNetCore.Authentication.Negotiate` for Windows Integrated Auth (zero-config on corporate domain). Do not build this now.

### Input Validation

| Input | Validation | Threat Mitigated |
|-------|-----------|-----------------|
| `?data=` query parameter | Reject `..`, `/`, `\`; require `.json` extension | Directory traversal (reading files outside `wwwroot/data/`) |
| `data.json` content | `System.Text.Json` deserialization with `required` properties | Malformed data causing null reference exceptions |
| `schemaVersion` field | Must equal `1` | Schema mismatch causing rendering errors |
| Date strings in JSON | `DateOnly.Parse()` with try/catch | Invalid dates causing math errors in timeline |
| URLs in `header.backlogLink` | Rendered as `href` in an `<a>` tag — standard browser navigation | No server-side URL fetching; XSS mitigated by Blazor's built-in HTML encoding |

### Data Protection

- **No sensitive data.** Dashboard content is project status information (milestone names, dates, work item titles). Not PII, not financial data, not credentials.
- **No encryption.** `data.json` is a local file. NTFS permissions are sufficient.
- **No cookies, no session state, no local storage.** The application is stateless.
- **Blazor's built-in XSS protection:** All `@` expressions in Razor are HTML-encoded by default. `MarkupString` is not used.

### Network Security

- **Kestrel binds to localhost only.** Not accessible from other machines on the network.
- **Zero outbound HTTP requests.** No telemetry, no CDN, no API calls.
- **HTTPS available** via Kestrel's default development certificate (`https://localhost:5001`), but HTTP is sufficient for localhost use.

---

## Scaling Strategy

**This application does not scale. By design.**

It is a single-user, localhost-only tool for generating screenshots. There is no multi-user scenario, no concurrent access, no load balancing, no horizontal scaling.

### Performance Boundaries (Tested Limits)

| Dimension | Supported Range | Behavior at Limit |
|-----------|----------------|-------------------|
| Timeline tracks | 1–10 | Y spacing compresses; labels overlap at >6 tracks |
| Milestones per track | 1–20 | Labels may overlap at high density; date clamping prevents overflow |
| Heatmap columns (months) | 1–8 | Column width shrinks via `1fr`; text truncates at >8 columns |
| Items per heatmap cell | 0–10 | Cell content overflows are hidden via `overflow: hidden` |
| `data.json` file size | Up to 500KB | Deserialization completes in <100ms |
| Concurrent users | 1 | SignalR circuit is per-user; no shared state contention |

### If Scaling Were Ever Needed

| Scenario | Approach |
|----------|----------|
| Multiple PMs on same machine | Already supported via `?data=` query parameter (different browser tabs) |
| Dashboard shared on intranet | Add Windows Integrated Auth; Kestrel can bind to `0.0.0.0` |
| Automated screenshot generation | Add Playwright (.NET) for headless Chrome at 1920×1080 |
| Real-time data updates | Add `FileSystemWatcher` on `data.json` + `InvokeAsync(StateHasChanged)` |

---

## UI Component Architecture

### Visual Section → Component Mapping

| Visual Section (from `OriginalDesignConcept.html`) | Blazor Component | CSS Layout Strategy | Data Bindings | Interactions |
|-----------------------------------------------------|-----------------|---------------------|---------------|-------------|
| **Header bar** (`.hdr`) — title, subtitle, legend | `HeaderSection.razor` | **Flexbox**: `display: flex; align-items: center; justify-content: space-between` | `Header.Title`, `Header.Subtitle`, `Header.BacklogLink` | Click on ADO backlog link (standard `<a>` navigation) |
| **Timeline sidebar** (230px left panel with track labels) | `TimelineSection.razor` (rendered as part of the timeline component's outer `<div>`) | **Flexbox column**: `display: flex; flex-direction: column; justify-content: space-around` | `Timeline.Tracks[].Label`, `Timeline.Tracks[].Description`, `Timeline.Tracks[].Color` | None (read-only) |
| **Timeline SVG chart** (`.tl-svg-box` — month grid, track lines, milestones, NOW line) | `TimelineSection.razor` (SVG block) | **Inline SVG**: Fixed `width="1560" height="185"`, absolute positioning via x/y attributes | `Timeline.StartDate`, `Timeline.EndDate`, `Timeline.Tracks[].Milestones[]`, `CurrentDate` for NOW line | None for MVP (tooltips in Phase 3) |
| **Heatmap title** (`.hm-title`) | `HeatmapSection.razor` (rendered as a `<div>` above the grid) | **Block**: Static text, `flex-shrink: 0` | Static text (hardcoded) | None |
| **Heatmap grid** (`.hm-grid` — corner, column headers, row headers, data cells) | `HeatmapSection.razor` | **CSS Grid**: `grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr)` | `Heatmap.Columns[]` → column headers, `Heatmap.Rows[]` → row headers + cells, `Heatmap.HighlightColumn` → gold highlight | None (read-only) |
| **Error state** (missing/malformed data) | `Dashboard.razor` (inline conditional) | **Flexbox centered**: `display: flex; align-items: center; justify-content: center; height: 100vh` | `_errorMessage` string | None |

### Component Hierarchy

```
App.razor
└── MainLayout.razor (empty wrapper)
    └── Dashboard.razor (page, route="/")
        ├── [Error state div]          — if _errorMessage is set
        └── [Normal render]            — if _data is loaded
            ├── HeaderSection.razor     — Header bar
            ├── TimelineSection.razor   — Timeline sidebar + SVG
            └── HeatmapSection.razor    — Heatmap title + CSS Grid
```

### CSS Class Mapping (Design → Implementation)

| Original CSS Class | Component | Usage |
|--------------------|-----------|-------|
| `.hdr` | `HeaderSection.razor` | Outer `<div>` for header bar |
| `.sub` | `HeaderSection.razor` | Subtitle text below title |
| `.tl-area` | `TimelineSection.razor` | Outer container for timeline section |
| `.tl-svg-box` | `TimelineSection.razor` | SVG container `<div>` |
| `.hm-wrap` | `HeatmapSection.razor` | Outer container for heatmap section |
| `.hm-title` | `HeatmapSection.razor` | Section title text |
| `.hm-grid` | `HeatmapSection.razor` | CSS Grid container |
| `.hm-corner` | `HeatmapSection.razor` | Top-left "STATUS" cell |
| `.hm-col-hdr` | `HeatmapSection.razor` | Month column headers |
| `.hm-row-hdr` | `HeatmapSection.razor` | Category row headers |
| `.hm-cell` | `HeatmapSection.razor` | Data cells |
| `.it` | `HeatmapSection.razor` | Individual work item within a cell |
| `.ship-hdr`, `.ship-cell` | `HeatmapSection.razor` | Shipped row styling |
| `.prog-hdr`, `.prog-cell` | `HeatmapSection.razor` | In Progress row styling |
| `.carry-hdr`, `.carry-cell` | `HeatmapSection.razor` | Carryover row styling |
| `.block-hdr`, `.block-cell` | `HeatmapSection.razor` | Blockers row styling |
| `.apr` (modifier) | `HeatmapSection.razor` | Highlighted column cell modifier |
| `.apr-hdr` (modifier) | `HeatmapSection.razor` | Highlighted column header modifier |
| `.error-container` | `Dashboard.razor` | Error state wrapper |
| `.error-message` | `Dashboard.razor` | Error text |

---

## Risks & Mitigations

### Risk 1: Screenshot Fidelity Across Machines

| Attribute | Value |
|-----------|-------|
| **Impact** | Medium — screenshots may look different on different machines |
| **Probability** | Medium |
| **Trigger** | Different Chrome versions, DPI settings, or Segoe UI font rendering |

**Mitigations:**
1. Document the exact screenshot procedure: "Chrome → F12 → Device Toolbar → 1920×1080 → 100% zoom → Ctrl+Shift+P → 'Capture full size screenshot'."
2. Segoe UI is installed on all Windows 10/11 machines — font rendering is consistent within the Windows ecosystem.
3. Phase 3: Add Playwright screenshot automation for bit-identical output.

### Risk 2: `data.json` Schema Drift

| Attribute | Value |
|-----------|-------|
| **Impact** | Low — dashboard fails to render with unhelpful error |
| **Probability** | Medium — schema will evolve over time |

**Mitigations:**
1. `schemaVersion` field in JSON, validated on load.
2. C# `required` properties produce specific error messages naming the missing field.
3. Example `data.json` ships with the project as a template.
4. If schema changes are needed, increment `schemaVersion` and add migration logic in `DashboardDataService`.

### Risk 3: SVG Timeline Date Math Edge Cases

| Attribute | Value |
|-----------|-------|
| **Impact** | Low — milestones render in wrong positions or overflow |
| **Probability** | Low |

**Mitigations:**
1. `Math.Clamp()` constrains all X positions to `[0, SvgWidth]`.
2. Division-by-zero guard when `startDate == endDate` (all milestones render at x=0).
3. Configurable `startDate`/`endDate` in `data.json` gives the PM full control.

### Risk 4: Blazor Server Overhead for a Static Page

| Attribute | Value |
|-----------|-------|
| **Impact** | Low — unnecessary SignalR connection uses ~50KB memory |
| **Probability** | Certain (known trade-off) |

**Mitigations:**
- Accept the overhead. Blazor Server provides C# model binding, component composition, and hot reload — all valuable for development velocity. The overhead is negligible for a single-user localhost tool.
- Suppress the Blazor reconnection UI via CSS (`#components-reconnect-modal { display: none !important; }`).

### Risk 5: PM Edits Break JSON Syntax

| Attribute | Value |
|-----------|-------|
| **Impact** | Low — dashboard shows error instead of data |
| **Probability** | High — JSON is easy to break (trailing commas, missing quotes) |

**Mitigations:**
1. Friendly error messages surface the exact JSON parse error (line number, character position).
2. Example `data.json` is well-formatted and serves as a template.
3. Recommend the PM use VS Code with JSON validation for editing.
4. No application crash — all JSON errors are caught and rendered as user-friendly messages.

---

## File Inventory

```
ReportingDashboard.sln                              # Solution file
└── src/ReportingDashboard/
    ├── ReportingDashboard.csproj                    # Project file (zero NuGet refs)
    ├── Program.cs                                   # Entry point (~15 lines)
    ├── Models/
    │   └── DashboardData.cs                         # C# records (~60 lines)
    ├── Services/
    │   └── DashboardDataService.cs                  # Data loading (~50 lines)
    ├── Components/
    │   ├── App.razor                                # Root component
    │   ├── Routes.razor                             # Router
    │   ├── _Imports.razor                           # Global usings
    │   ├── Layout/
    │   │   └── MainLayout.razor                     # Empty layout (~3 lines)
    │   ├── Pages/
    │   │   └── Dashboard.razor                      # Main page (~40 lines)
    │   ├── HeaderSection.razor                      # Header component (~35 lines)
    │   ├── TimelineSection.razor                    # Timeline SVG (~100 lines)
    │   └── HeatmapSection.razor                     # Heatmap grid (~60 lines)
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css                        # All styles (~200 lines)
    │   └── data/
    │       └── data.json                            # Example data (~80 lines)
    └── Properties/
        └── launchSettings.json                      # Dev server config

Total: 15 files (within the <15 file target)
```