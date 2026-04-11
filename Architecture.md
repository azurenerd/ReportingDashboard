# Architecture

## Overview & Goals

This architecture defines a minimal, single-page Blazor Server application (.NET 8) that renders an executive reporting dashboard from a local `data.json` file. The dashboard produces a pixel-perfect 1920×1080 view optimized for screenshotting into PowerPoint decks.

**Primary Goals:**
1. **Data-driven rendering** — All content sourced from a single `data.json` file; zero hardcoded text in markup
2. **Pixel-perfect fidelity** — Output visually identical to `OriginalDesignConcept.html` at 1920×1080
3. **Zero infrastructure** — No database, no cloud services, no authentication; runs via `dotnet run`
4. **Static SSR** — Pure server-side HTML rendering with no JavaScript, no SignalR circuit, no interactive render modes
5. **Sub-60-second updates** — Edit JSON, refresh browser, screenshot

**Architecture Style:** Monolithic single-project Blazor Server application using Static Server-Side Rendering. Component-based UI with a flat service layer reading from a local JSON file. No layered architecture, no repository pattern, no dependency injection abstractions beyond what's needed for testability.

---

## System Components

### 1. Solution Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard.Web/
│       ├── ReportingDashboard.Web.csproj
│       ├── Program.cs
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── appsettings.json
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css
│       │   ├── data.json              ← .gitignored
│       │   └── data.sample.json       ← committed
│       ├── Models/
│       │   └── DashboardData.cs       ← all record types in one file
│       ├── Services/
│       │   ├── IDashboardDataService.cs
│       │   └── DashboardDataService.cs
│       └── Components/
│           ├── App.razor
│           ├── _Imports.razor
│           ├── Layout/
│           │   └── DashboardLayout.razor
│           └── Pages/
│               └── Dashboard.razor
│               └── DashboardHeader.razor
│               └── DashboardTimeline.razor
│               └── DashboardHeatmap.razor
│               └── HeatmapCell.razor
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── Services/
            └── DashboardDataServiceTests.cs
```

### 2. Component Catalog

#### 2.1 `Program.cs` — Application Entry Point

**Responsibilities:**
- Configure Kestrel to listen on `http://localhost:5000`
- Register `IDashboardDataService` as a singleton in DI
- Add Razor Components services
- Map Razor Components with `App.razor` as root
- Suppress HTTPS redirection, developer exception pages, and all middleware not needed for static HTML serving

**Key Configuration:**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddSingleton<IDashboardDataService, DashboardDataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>();
app.Run();
```

**Dependencies:** None beyond ASP.NET Core 8 SDK

---

#### 2.2 `IDashboardDataService` / `DashboardDataService` — Data Access

**Responsibilities:**
- Read `wwwroot/data.json` from disk on each HTTP request (enabling live refresh without restart)
- Deserialize JSON into `DashboardData` record using `System.Text.Json`
- Return a structured error result if the file is missing or malformed
- Provide date-to-SVG-coordinate calculation helper methods

**Interface:**
```csharp
public interface IDashboardDataService
{
    Task<DashboardDataResult> LoadDashboardDataAsync();
}

public record DashboardDataResult
{
    public bool Success { get; init; }
    public DashboardData? Data { get; init; }
    public string? ErrorMessage { get; init; }
}
```

**Implementation Details:**
- Uses `IWebHostEnvironment.WebRootPath` to locate `data.json`
- Reads file with `File.ReadAllTextAsync` on every request (file is <10KB; no caching needed)
- Wraps deserialization in try/catch; catches `JsonException` and `FileNotFoundException`
- Uses `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` for flexible JSON field casing
- Does NOT cache — this ensures edits to `data.json` are reflected on browser refresh without restart

**Why read on every request?** The PM spec explicitly requires "modify data.json and refresh the browser reflects changes immediately (no rebuild required)." Caching would break this. The file is trivially small, so disk I/O cost is negligible.

---

#### 2.3 `App.razor` — Root Component

**Responsibilities:**
- Define the HTML document structure (`<!DOCTYPE html>`, `<html>`, `<head>`, `<body>`)
- Reference `dashboard.css` stylesheet
- Set the `<body>` to `width:1920px; height:1080px; overflow:hidden` inline or via CSS class
- Render `<Routes />` component for page routing
- Suppress all Blazor framework scripts (no `blazor.web.js` — static SSR doesn't need it)

**Critical:** Must NOT include `<script src="_framework/blazor.web.js"></script>` since we use static SSR with no interactivity. The `.csproj` should not reference any interactive render mode packages.

---

#### 2.4 `DashboardLayout.razor` — Layout

**Responsibilities:**
- Provide a minimal layout with zero navigation chrome
- Render only `@Body` — no sidebar, no nav bar, no header bar, no footer
- No `<NavMenu>`, no hamburger icon, no Blazor default layout elements

```razor
@inherits LayoutComponentBase
@Body
```

This is intentionally minimal. The dashboard page itself contains all visual structure.

---

#### 2.5 `Dashboard.razor` — Main Page (`/` route)

**Responsibilities:**
- Route: `@page "/"`
- Inject `IDashboardDataService`
- Call `LoadDashboardDataAsync()` in `OnInitializedAsync`
- If data load fails, render an error message div (styled, human-readable)
- If data load succeeds, render three child components in a vertical flex column:
  1. `DashboardHeader` — project title, subtitle, legend
  2. `DashboardTimeline` — SVG milestone visualization
  3. `DashboardHeatmap` — CSS Grid status heatmap

**Parameters passed to children:** Each child component receives its relevant slice of `DashboardData` via `[Parameter]` properties. The parent does not pass the entire model to every child.

**Error State Rendering:**
```html
<div style="display:flex;align-items:center;justify-content:center;
            width:1920px;height:1080px;font-family:'Segoe UI',Arial,sans-serif;">
    <div style="text-align:center;color:#991B1B;">
        <h2>Dashboard Error</h2>
        <p>@errorMessage</p>
    </div>
</div>
```

---

#### 2.6 `DashboardHeader.razor` — Header Bar Component

**Visual Reference:** `.hdr` section of `OriginalDesignConcept.html`

**Responsibilities:**
- Render project title as `<h1>` at 24px bold
- Render inline ADO Backlog hyperlink (color `#0078D4`, no underline)
- Render subtitle line (12px, color `#888`)
- Render right-aligned legend with four marker types

**Parameters:**
```csharp
[Parameter] public string Title { get; set; }
[Parameter] public string Subtitle { get; set; }
[Parameter] public string BacklogUrl { get; set; }
[Parameter] public string CurrentMonth { get; set; }
```

**Legend items** are hardcoded in the component markup since they represent the fixed visual vocabulary of the design. The "Now" label dynamically includes the current month from data.

---

#### 2.7 `DashboardTimeline.razor` — SVG Timeline Component

**Visual Reference:** `.tl-area` section of `OriginalDesignConcept.html`

**Responsibilities:**
- Render the left-side track label panel (230px wide) with milestone IDs, descriptions, and track colors
- Render an inline `<svg>` element (1560×185px) containing:
  - Vertical month gridlines with abbreviated month labels
  - Horizontal track lines (one per track, colored)
  - Checkpoint markers (circles: white fill, track-color stroke)
  - PoC milestone diamonds (gold `#F4B400`, drop shadow)
  - Production milestone diamonds (green `#34A853`, drop shadow)
  - Small dot markers (gray `#999`, r=4)
  - "NOW" dashed vertical line (red `#EA4335`, stroke-dasharray 5,3)
  - Date labels for each marker

**Parameters:**
```csharp
[Parameter] public List<TimelineTrack> Tracks { get; set; }
[Parameter] public string TimelineStart { get; set; }    // "2026-01-01"
[Parameter] public string TimelineEnd { get; set; }      // "2026-06-30"
[Parameter] public string CurrentDate { get; set; }      // "2026-04-11"
```

**SVG Coordinate Calculation:**
```csharp
private double DateToX(DateOnly date)
{
    var start = DateOnly.Parse(TimelineStart);
    var end = DateOnly.Parse(TimelineEnd);
    return (date.DayNumber - start.DayNumber) / (double)(end.DayNumber - start.DayNumber) * SvgWidth;
}
```

Constants: `SvgWidth = 1560`, `SvgHeight = 185`. Track Y positions calculated as: first track at y=42, subsequent tracks spaced evenly based on track count (e.g., 3 tracks → y=42, y=98, y=154).

**Month Gridlines:** Iterate from timeline start to end by month. For each month boundary, calculate X position using `DateToX` and render a vertical `<line>` with abbreviated month name `<text>`.

**Diamond Marker Rendering:**
```razor
@{
    var pts = $"{x},{y-11} {x+11},{y} {x},{y+11} {x-11},{y}";
}
<polygon points="@pts" fill="@color" filter="url(#sh)" />
```

**Drop Shadow Filter** (defined once in `<defs>`):
```svg
<filter id="sh">
    <feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>
</filter>
```

---

#### 2.8 `DashboardHeatmap.razor` — Heatmap Grid Component

**Visual Reference:** `.hm-wrap` and `.hm-grid` sections of `OriginalDesignConcept.html`

**Responsibilities:**
- Render the section title ("MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS")
- Render a CSS Grid with dynamic column count: `grid-template-columns: 160px repeat(N, 1fr)` where N = months count
- Render the header row: corner cell + month column headers (highlighting current month with `.apr-hdr`)
- Render 4 category rows, each containing: row header + N data cells
- Delegate cell rendering to `HeatmapCell` child component

**Parameters:**
```csharp
[Parameter] public List<string> Months { get; set; }
[Parameter] public string CurrentMonth { get; set; }
[Parameter] public List<HeatmapCategory> Categories { get; set; }
```

**Dynamic Grid Style:**
```csharp
private string GridStyle =>
    $"grid-template-columns: 160px repeat({Months.Count}, 1fr); " +
    $"grid-template-rows: 36px repeat({Categories.Count}, 1fr);";
```

**Category-to-CSS Mapping:**
```csharp
private string GetRowHeaderClass(string type) => type switch
{
    "shipped" => "hm-row-hdr ship-hdr",
    "progress" => "hm-row-hdr prog-hdr",
    "carryover" => "hm-row-hdr carry-hdr",
    "blocker" => "hm-row-hdr block-hdr",
    _ => "hm-row-hdr"
};

private string GetCellClass(string type, bool isCurrent) => (type, isCurrent) switch
{
    ("shipped", true) => "hm-cell ship-cell apr",
    ("shipped", false) => "hm-cell ship-cell",
    ("progress", true) => "hm-cell prog-cell apr",
    ("progress", false) => "hm-cell prog-cell",
    ("carryover", true) => "hm-cell carry-cell apr",
    ("carryover", false) => "hm-cell carry-cell",
    ("blocker", true) => "hm-cell block-cell apr",
    ("blocker", false) => "hm-cell block-cell",
    _ => "hm-cell"
};
```

---

#### 2.9 `HeatmapCell.razor` — Individual Cell Component

**Responsibilities:**
- Receive a list of work item strings and render each as a `<div class="it">` element
- If the item list is empty or null, render a muted dash `—` in light gray
- Apply the appropriate CSS class for category color and current-month highlighting

**Parameters:**
```csharp
[Parameter] public List<string>? Items { get; set; }
[Parameter] public string CssClass { get; set; }
```

**Rendering Logic:**
```razor
<div class="@CssClass">
    @if (Items is { Count: > 0 })
    {
        @foreach (var item in Items)
        {
            <div class="it">@item</div>
        }
    }
    else
    {
        <div class="it" style="color:#CCC;">—</div>
    }
</div>
```

---

#### 2.10 `dashboard.css` — Stylesheet

**Responsibilities:**
- Contain ALL visual styling for the dashboard
- Direct port of the `<style>` block from `OriginalDesignConcept.html`
- Use CSS custom properties for the color palette (optional enhancement)

**Strategy:** Copy the CSS verbatim from the HTML design file. Do NOT rewrite or "improve" it. The goal is pixel-perfect fidelity. Changes allowed:
- Add CSS custom properties (`:root` block) for color values
- Rename `.apr` to a more semantic name only if a mapping layer handles it
- Add any additional utility classes needed for the error state

**File Size:** ~3KB estimated. Single file, no CSS modules, no preprocessor.

---

#### 2.11 `data.sample.json` — Sample Data File

**Responsibilities:**
- Provide a complete, fictional "Project Phoenix" dataset exercising all dashboard features
- Demonstrate all supported fields, tracks, milestones, and category types
- Serve as the out-of-box experience when a developer clones and runs

**Location:** `wwwroot/data.sample.json` (committed to source control)

---

### 3. Project File (`ReportingDashboard.Web.csproj`)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**Zero NuGet dependencies.** The project uses only SDK-included packages: `Microsoft.AspNetCore.Components`, `System.Text.Json`.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│  Browser (Edge/Chrome @ 1920x1080)                          │
│  GET http://localhost:5000/                                  │
└─────────────┬───────────────────────────────────────────────┘
              │ HTTP Request
              ▼
┌─────────────────────────────────────────────────────────────┐
│  Kestrel Web Server (localhost:5000)                         │
│  ├─ Static Files Middleware → wwwroot/css/dashboard.css      │
│  └─ Razor Components Middleware → Dashboard.razor            │
└─────────────┬───────────────────────────────────────────────┘
              │ Calls OnInitializedAsync
              ▼
┌─────────────────────────────────────────────────────────────┐
│  Dashboard.razor (Page Component)                           │
│  Injects IDashboardDataService                               │
│  Calls LoadDashboardDataAsync()                              │
└─────────────┬───────────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────────────┐
│  DashboardDataService                                        │
│  ├─ Reads wwwroot/data.json from disk                       │
│  ├─ Deserializes with System.Text.Json                      │
│  └─ Returns DashboardDataResult (success or error)          │
└─────────────┬───────────────────────────────────────────────┘
              │ DashboardData record
              ▼
┌─────────────────────────────────────────────────────────────┐
│  Dashboard.razor distributes data to child components:       │
│  ├─ DashboardHeader ← (Title, Subtitle, BacklogUrl,        │
│  │                      CurrentMonth)                        │
│  ├─ DashboardTimeline ← (Tracks[], TimelineStart,          │
│  │                        TimelineEnd, CurrentDate)          │
│  └─ DashboardHeatmap ← (Months[], CurrentMonth,            │
│       │                   Categories[])                      │
│       └─ HeatmapCell ← (Items[], CssClass)                 │
└─────────────┬───────────────────────────────────────────────┘
              │ Rendered HTML + CSS
              ▼
┌─────────────────────────────────────────────────────────────┐
│  Browser renders static HTML                                 │
│  No JavaScript, no SignalR, no WebSocket                     │
│  Screenshot-ready on first paint                             │
└─────────────────────────────────────────────────────────────┘
```

### Request Lifecycle

1. **Browser** sends `GET /` to Kestrel
2. **Kestrel** routes to Razor Components middleware
3. **App.razor** renders `<head>` (with CSS link) and `<body>` with `<Routes />`
4. **DashboardLayout.razor** renders `@Body`
5. **Dashboard.razor** executes `OnInitializedAsync`:
   - Calls `IDashboardDataService.LoadDashboardDataAsync()`
   - Service reads `wwwroot/data.json` from disk
   - Deserializes into `DashboardData` record graph
   - Returns `DashboardDataResult`
6. **Dashboard.razor** checks `result.Success`:
   - **If false:** Renders error message div
   - **If true:** Renders `DashboardHeader`, `DashboardTimeline`, `DashboardHeatmap` with data parameters
7. **Each child component** renders its HTML fragment using the passed parameters
8. **Kestrel** sends the complete HTML response (no JavaScript payload)
9. **Browser** paints the dashboard — done. No hydration, no AJAX, no loading states.

**Total server-side work per request:** ~5ms (file read + JSON deserialization + Razor rendering)

---

## Data Model

### Entity Definitions

All models are C# `record` types in a single file (`Models/DashboardData.cs`). Records are ideal here: immutable, concise, value-equality semantics, built into C# 12.

```csharp
namespace ReportingDashboard.Web.Models;

public record DashboardData
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string BacklogUrl { get; init; } = string.Empty;
    public string TimelineStart { get; init; } = string.Empty;
    public string TimelineEnd { get; init; } = string.Empty;
    public string CurrentDate { get; init; } = string.Empty;
    public string CurrentMonth { get; init; } = string.Empty;
    public List<TimelineTrack> Tracks { get; init; } = new();
    public List<string> Months { get; init; } = new();
    public List<HeatmapCategory> Categories { get; init; } = new();
    public LegendConfig? Legend { get; init; }
}

public record TimelineTrack
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public List<MilestoneMarker> Markers { get; init; } = new();
}

public record MilestoneMarker
{
    public string Date { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;   // "checkpoint" | "poc" | "production" | "dot"
    public string Label { get; init; } = string.Empty;
    public string? LabelPosition { get; init; }          // "above" | "below" (default: "above")
}

public record HeatmapCategory
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;   // "shipped" | "progress" | "carryover" | "blocker"
    public Dictionary<string, List<string>> ItemsByMonth { get; init; } = new();
}

public record LegendConfig
{
    public List<LegendItem> Items { get; init; } = new();
}

public record LegendItem
{
    public string Label { get; init; } = string.Empty;
    public string MarkerType { get; init; } = string.Empty; // "poc" | "production" | "checkpoint" | "now"
}
```

### Entity Relationships

```
DashboardData (root)
├── 1:N → TimelineTrack (1-5 tracks)
│         └── 1:N → MilestoneMarker (0-10 markers per track)
├── 1:N → string Months (1-6 month names, ordered)
├── 1:N → HeatmapCategory (1-4 categories)
│         └── 1:N → Dictionary<month, List<string>> items
└── 1:1 → LegendConfig (optional)
          └── 1:N → LegendItem
```

### Storage

**Storage engine:** Local filesystem. Single JSON file at `wwwroot/data.json`.

- **No database** — not SQLite, not LiteDB, not Entity Framework
- **No caching layer** — file is read fresh on each request (~5KB, <1ms read time)
- **No write operations** — the application is strictly read-only
- **File watching:** Not implemented. The PM manually refreshes the browser after editing.

### JSON Schema Contract

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["title", "subtitle", "backlogUrl", "timelineStart", "timelineEnd",
               "currentDate", "currentMonth", "tracks", "months", "categories"],
  "properties": {
    "title": { "type": "string" },
    "subtitle": { "type": "string" },
    "backlogUrl": { "type": "string", "format": "uri" },
    "timelineStart": { "type": "string", "format": "date" },
    "timelineEnd": { "type": "string", "format": "date" },
    "currentDate": { "type": "string", "format": "date" },
    "currentMonth": { "type": "string" },
    "tracks": {
      "type": "array", "minItems": 1, "maxItems": 5,
      "items": {
        "type": "object",
        "required": ["id", "label", "description", "color", "markers"],
        "properties": {
          "id": { "type": "string" },
          "label": { "type": "string" },
          "description": { "type": "string" },
          "color": { "type": "string", "pattern": "^#[0-9A-Fa-f]{6}$" },
          "markers": {
            "type": "array",
            "items": {
              "type": "object",
              "required": ["date", "type", "label"],
              "properties": {
                "date": { "type": "string", "format": "date" },
                "type": { "type": "string", "enum": ["checkpoint", "poc", "production", "dot"] },
                "label": { "type": "string" },
                "labelPosition": { "type": "string", "enum": ["above", "below"] }
              }
            }
          }
        }
      }
    },
    "months": { "type": "array", "minItems": 1, "maxItems": 6, "items": { "type": "string" } },
    "categories": {
      "type": "array", "minItems": 1, "maxItems": 4,
      "items": {
        "type": "object",
        "required": ["name", "type", "itemsByMonth"],
        "properties": {
          "name": { "type": "string" },
          "type": { "type": "string", "enum": ["shipped", "progress", "carryover", "blocker"] },
          "itemsByMonth": {
            "type": "object",
            "additionalProperties": { "type": "array", "items": { "type": "string" } }
          }
        }
      }
    }
  }
}
```

---

## API Contracts

### HTTP Endpoints

This application exposes exactly **one** endpoint:

| Method | Path | Response | Content-Type |
|--------|------|----------|-------------|
| `GET` | `/` | Complete HTML page (dashboard) | `text/html` |

There is no REST API, no JSON API, no GraphQL. The application is a server-rendered HTML page.

### Static File Endpoints (Automatic via `UseStaticFiles`)

| Method | Path | Response |
|--------|------|----------|
| `GET` | `/css/dashboard.css` | CSS stylesheet |
| `GET` | `/data.json` | Raw JSON data (direct file access) |
| `GET` | `/data.sample.json` | Sample JSON data |

### Response Shapes

**Success (200 OK):**
Complete HTML document containing the rendered dashboard. ~30-80KB depending on data volume. No JavaScript. No `<script>` tags. Pure HTML + inline SVG + CSS class references.

**Error — Missing data.json:**
200 OK with an HTML page showing a centered error message:
```
Dashboard Error
Error: Could not load data.json. Please ensure the file exists in wwwroot/ and contains valid JSON.
```

**Error — Malformed JSON:**
200 OK with an HTML page showing:
```
Dashboard Error
Error: data.json contains invalid JSON. [JsonException message details]
```

**Design decision:** Errors return 200 with a styled error page rather than 500/404 because the dashboard is intended for human viewing in a browser, not API consumption. A styled error page is more useful than a stack trace or generic error page.

### Error Handling Strategy

```csharp
public async Task<DashboardDataResult> LoadDashboardDataAsync()
{
    var path = Path.Combine(_webHostEnvironment.WebRootPath, "data.json");

    if (!File.Exists(path))
    {
        return new DashboardDataResult
        {
            Success = false,
            ErrorMessage = "Could not load data.json. Please ensure the file exists in wwwroot/ and contains valid JSON."
        };
    }

    try
    {
        var json = await File.ReadAllTextAsync(path);
        var data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);

        if (data is null)
        {
            return new DashboardDataResult
            {
                Success = false,
                ErrorMessage = "data.json was empty or could not be parsed."
            };
        }

        return new DashboardDataResult { Success = true, Data = data };
    }
    catch (JsonException ex)
    {
        return new DashboardDataResult
        {
            Success = false,
            ErrorMessage = $"data.json contains invalid JSON: {ex.Message}"
        };
    }
}
```

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component, its CSS layout strategy, data bindings, and interactions.

### Visual-to-Component Mapping

```
┌──────────────────────────────────────────────────────────────────┐
│  <body> — 1920x1080, flex column, overflow:hidden               │
│  Component: App.razor + DashboardLayout.razor                    │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │ .hdr — Header Bar (~46px)                                  │  │
│  │ Component: DashboardHeader.razor                           │  │
│  │ CSS: flex row, space-between, padding 12px 44px            │  │
│  │ Data: Title, Subtitle, BacklogUrl, CurrentMonth            │  │
│  │ Left: <h1> + <a> + <div class="sub">                      │  │
│  │ Right: Legend flex row (4 marker types)                     │  │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │ .tl-area — Timeline Area (196px)                           │  │
│  │ Component: DashboardTimeline.razor                         │  │
│  │ CSS: flex row, background #FAFAFA, border-bottom 2px       │  │
│  │ ┌──────────┐ ┌──────────────────────────────────────────┐  │  │
│  │ │ Track    │ │ .tl-svg-box — SVG Container              │  │  │
│  │ │ Labels   │ │ <svg> 1560x185                           │  │  │
│  │ │ (230px)  │ │ - Month gridlines (vertical lines)       │  │  │
│  │ │          │ │ - Track lines (horizontal, colored)       │  │  │
│  │ │ M1       │ │ - Diamond markers (PoC/Production)       │  │  │
│  │ │ M2       │ │ - Circle markers (Checkpoints)           │  │  │
│  │ │ M3       │ │ - Dot markers (minor checkpoints)        │  │  │
│  │ │          │ │ - NOW dashed line (red)                   │  │  │
│  │ │          │ │ - Date labels (above/below track)         │  │  │
│  │ └──────────┘ └──────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │ .hm-wrap — Heatmap Section (flex:1, fills remaining)       │  │
│  │ Component: DashboardHeatmap.razor                          │  │
│  │ ┌──────────────────────────────────────────────────────┐   │  │
│  │ │ .hm-title — Section Title (14px uppercase gray)      │   │  │
│  │ └──────────────────────────────────────────────────────┘   │  │
│  │ ┌──────────────────────────────────────────────────────┐   │  │
│  │ │ .hm-grid — CSS Grid                                  │   │  │
│  │ │ Columns: 160px + repeat(N, 1fr)                      │   │  │
│  │ │ Rows: 36px header + repeat(4, 1fr)                   │   │  │
│  │ │ ┌────┬──────┬──────┬──────┬──────┐                   │   │  │
│  │ │ │Cor.│ Jan  │ Feb  │ Mar  │*Apr* │ ← .hm-col-hdr    │   │  │
│  │ │ ├────┼──────┼──────┼──────┼──────┤    (.apr-hdr)     │   │  │
│  │ │ │Ship│      │      │      │      │ ← .ship-cell      │   │  │
│  │ │ │    │ Cell │ Cell │ Cell │ Cell │   HeatmapCell.razor│   │  │
│  │ │ ├────┼──────┼──────┼──────┼──────┤                   │   │  │
│  │ │ │Prog│      │      │      │      │ ← .prog-cell      │   │  │
│  │ │ ├────┼──────┼──────┼──────┼──────┤                   │   │  │
│  │ │ │Car.│      │      │      │      │ ← .carry-cell     │   │  │
│  │ │ ├────┼──────┼──────┼──────┼──────┤                   │   │  │
│  │ │ │Blk.│      │      │      │      │ ← .block-cell     │   │  │
│  │ │ └────┴──────┴──────┴──────┴──────┘                   │   │  │
│  │ └──────────────────────────────────────────────────────┘   │  │
│  └────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

### Component Detail Table

| Visual Section | Component | CSS Classes | Layout Strategy | Data Bindings | Interactions |
|---|---|---|---|---|---|
| `<body>` wrapper | `App.razor` | (inline or class) `width:1920px; height:1080px; overflow:hidden; display:flex; flex-direction:column` | Vertical flex column, fixed dimensions | None (structural only) | None |
| `.hdr` header bar | `DashboardHeader.razor` | `.hdr`, `.sub` | Flex row, `space-between`, padding `12px 44px 10px` | `Title` → `<h1>`, `BacklogUrl` → `<a href>`, `Subtitle` → `.sub` div, `CurrentMonth` → legend "Now" label | ADO link is a standard `<a>` tag (navigates on click) |
| Legend (right side of header) | Inline in `DashboardHeader.razor` | Inline styles matching design | Flex row, `gap: 22px` | `CurrentMonth` → "Now ({month} 2026)" label text | None (static display) |
| `.tl-area` timeline container | `DashboardTimeline.razor` | `.tl-area`, `.tl-svg-box` | Flex row: 230px label panel + flex:1 SVG box | `Tracks[]` → label panel items; `Tracks[].Color` → label color | None |
| Track label panel (230px) | Inline in `DashboardTimeline.razor` | Inline styles | Flex column, `justify-content: space-around` | `Tracks[].Label` → ID text, `Tracks[].Description` → description, `Tracks[].Color` → font color | None |
| `<svg>` timeline | Inline SVG in `DashboardTimeline.razor` | N/A (SVG attributes) | 1560×185px SVG with computed positions | `TimelineStart/End` → gridline positions; `Tracks[].Markers[]` → shapes; `CurrentDate` → NOW line X position | None |
| `.hm-wrap` heatmap wrapper | `DashboardHeatmap.razor` | `.hm-wrap`, `.hm-title` | Flex column, `flex:1` fills remaining height | `Months.Count` → grid column count | None |
| `.hm-grid` grid | Inline in `DashboardHeatmap.razor` | `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.apr-hdr` | CSS Grid: `160px repeat(N, 1fr)` columns, `36px repeat(4, 1fr)` rows | `Months[]` → column headers; `CurrentMonth` → `.apr-hdr` class toggle | None |
| Row headers | Inline in `DashboardHeatmap.razor` | `.hm-row-hdr`, `.ship-hdr` / `.prog-hdr` / `.carry-hdr` / `.block-hdr` | Grid cell, flex alignment | `Categories[].Name` → row header text; `Categories[].Type` → CSS class selection | None |
| Data cells | `HeatmapCell.razor` | `.hm-cell`, `.ship-cell` / `.prog-cell` / `.carry-cell` / `.block-cell`, `.apr` | Grid cell, internal vertical stack | `Categories[].ItemsByMonth[month]` → list of `.it` divs; empty → muted dash | None |
| Work items in cells | Inline in `HeatmapCell.razor` | `.it`, `::before` pseudo-element (colored dot) | Relative positioning for dot, `padding-left: 12px` | Each string in items array → one `.it` div | None |

### CSS Class Inheritance from Design

The following CSS classes are ported **verbatim** from `OriginalDesignConcept.html`:

| CSS Class | Used By | Purpose |
|-----------|---------|---------|
| `.hdr` | `DashboardHeader` | Header bar layout |
| `.sub` | `DashboardHeader` | Subtitle text style |
| `.tl-area` | `DashboardTimeline` | Timeline area container |
| `.tl-svg-box` | `DashboardTimeline` | SVG container within timeline |
| `.hm-wrap` | `DashboardHeatmap` | Heatmap wrapper |
| `.hm-title` | `DashboardHeatmap` | Section title above grid |
| `.hm-grid` | `DashboardHeatmap` | CSS Grid container |
| `.hm-corner` | `DashboardHeatmap` | Top-left corner cell |
| `.hm-col-hdr` | `DashboardHeatmap` | Month column headers |
| `.apr-hdr` | `DashboardHeatmap` | Current month highlight on header |
| `.hm-row-hdr` | `DashboardHeatmap` | Category row headers |
| `.hm-cell` | `HeatmapCell` | Base data cell |
| `.it` | `HeatmapCell` | Individual work item |
| `.ship-hdr`, `.ship-cell` | Heatmap | Shipped row styling |
| `.prog-hdr`, `.prog-cell` | Heatmap | In Progress row styling |
| `.carry-hdr`, `.carry-cell` | Heatmap | Carryover row styling |
| `.block-hdr`, `.block-cell` | Heatmap | Blockers row styling |
| `.apr` | `HeatmapCell` | Current month cell highlight |

---

## Infrastructure Requirements

### Hosting

| Requirement | Specification |
|-------------|--------------|
| **Runtime** | .NET 8 SDK (LTS, 8.0.x) — must be pre-installed |
| **Web Server** | Kestrel (built into ASP.NET Core, no IIS/nginx needed) |
| **Port** | `http://localhost:5000` (configured in `launchSettings.json`) |
| **Protocol** | HTTP only — no HTTPS, no TLS certificates |
| **OS** | Windows (primary), macOS/Linux (secondary) |

### Networking

- **Localhost only** — no external network access required
- **No inbound firewall rules** — Kestrel binds to localhost, not `0.0.0.0`
- **No outbound connections** — the app makes zero HTTP calls, zero DNS lookups
- **No SignalR WebSocket** — static SSR mode eliminates the persistent connection

### Storage

- **Disk:** ~5MB for the compiled application + runtime assets
- **Memory:** ~30-50MB process memory (minimal ASP.NET Core footprint)
- **File I/O:** One read of `data.json` (~5KB) per HTTP request

### CI/CD

- **Not required.** This is a local-only tool.
- If CI is desired in the future: `dotnet build` and `dotnet test` are sufficient. No container builds, no deployment pipelines.

### `launchSettings.json`

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

### `.gitignore` Entries

```gitignore
# User-specific data file (may contain internal project names)
wwwroot/data.json

# Standard .NET
bin/
obj/
*.user
```

---

## Technology Stack Decisions

| Decision | Chosen | Alternatives Considered | Justification |
|----------|--------|------------------------|---------------|
| **Framework** | Blazor Server (.NET 8) Static SSR | Blazor WASM, Razor Pages, MVC | Mandatory per project requirements. Static SSR is ideal — zero JavaScript, pure HTML response, fastest time-to-first-paint. |
| **Render Mode** | Static SSR (no `@rendermode`) | Interactive Server, Interactive WASM | No interactivity needed. Static SSR eliminates SignalR circuit, reduces response size, eliminates `blazor.web.js` payload. |
| **Data Format** | JSON flat file (`data.json`) | SQLite, LiteDB, YAML, TOML | JSON is human-readable, editable in any text editor, natively supported by `System.Text.Json`. No ORM or connection string needed. |
| **JSON Library** | `System.Text.Json` (built-in) | Newtonsoft.Json | Built into .NET 8 SDK — zero additional NuGet dependency. Sufficient for this simple schema. |
| **CSS Architecture** | Single `dashboard.css` file, direct port from design HTML | CSS Modules, Scoped CSS, Tailwind, SASS | The original HTML design already contains the complete CSS. Direct copy ensures pixel-perfect fidelity. No build pipeline needed. |
| **Timeline Rendering** | Inline SVG in Razor (server-side) | Chart.js, D3.js, MudBlazor charts, Syncfusion | The timeline is ~50 lines of SVG. A charting library would add 500KB+ of JavaScript and fight the pixel-perfect design. Hand-coded SVG gives full control. |
| **Heatmap Rendering** | CSS Grid with `@foreach` loops | HTML `<table>`, MudBlazor DataGrid | CSS Grid matches the original design exactly. `@foreach` over categories and months maps cleanly to the grid structure. |
| **Data Model** | C# `record` types | `class` types, anonymous types | Records are immutable, concise, have value equality. Perfect for read-only deserialized data. Built into C# 12. |
| **Dependency Injection** | Singleton `IDashboardDataService` | Transient, Scoped, static methods | Singleton is appropriate — the service has no per-request state (it reads the file fresh each time). Interface enables testing. |
| **Component Library** | None | MudBlazor, Radzen, Syncfusion | Overkill for a static dashboard. These libraries add CSS/JS bloat and component abstractions that conflict with the pixel-perfect CSS port requirement. |
| **Testing** | xUnit (optional, low priority) | NUnit, MSTest | xUnit is the de facto standard for .NET 8. Only needed for data service and model validation. |

---

## Security Considerations

### Threat Model

This application has an intentionally minimal threat surface:

| Threat | Applicable? | Rationale |
|--------|-------------|-----------|
| **Authentication bypass** | No | No authentication exists. The app is local-only, single-user. |
| **SQL injection** | No | No database. No SQL queries. |
| **XSS (Cross-Site Scripting)** | Minimal | Blazor Razor automatically HTML-encodes all `@` expressions. `data.json` values are rendered via `@item` which is auto-escaped. No `MarkupString` or `@((MarkupString)...)` should be used. |
| **CSRF** | No | No forms, no POST endpoints, no state-changing operations. |
| **Path traversal** | No | The file path to `data.json` is hardcoded to `wwwroot/data.json`. No user input influences file paths. |
| **Denial of service** | No | Single-user, localhost-only. No public exposure. |
| **Data exfiltration** | No | No outbound network calls. No telemetry. No logging of data content. |
| **Supply chain** | Minimal | Zero external NuGet packages. Only SDK-included assemblies. |

### Implemented Safeguards

1. **Auto-escaping:** All data from `data.json` rendered via Razor's `@` syntax is automatically HTML-encoded. Never use `@((MarkupString)userContent)` for JSON-sourced values.

2. **SVG attribute encoding:** SVG attributes rendered from data (e.g., `fill="@track.Color"`) are safe because Blazor encodes attribute values. Color strings like `#0078D4` contain no injection vectors.

3. **No `MarkupString` for user data:** The SVG `points` attribute is computed from numeric coordinates, not user strings. Date labels are auto-encoded.

4. **`.gitignore` for data.json:** Prevents accidental commit of internal project names to source control.

5. **No HTTPS:** Intentionally omitted. The app serves only to localhost and contains no sensitive data. Adding HTTPS would require certificate management with zero security benefit.

### Recommendation

**Do NOT add authentication, HTTPS, or CORS to this application.** These add complexity without value for a localhost-only screenshot tool. If the application ever becomes network-accessible, a new security review and architecture revision would be required.

---

## Scaling Strategy

### Current Scale

| Dimension | Current | Maximum Supported |
|-----------|---------|-------------------|
| Concurrent users | 1 | 1 (local machine) |
| Timeline tracks | 3 | 5 |
| Heatmap months | 4 | 6 |
| Status categories | 4 | 4 |
| Items per cell | ~5 | ~10 (before text overflow) |
| Total work items | ~40 | ~240 (6 months × 4 categories × 10 items) |
| data.json size | ~5KB | ~50KB |

### Scaling Is Not Required

This application is explicitly designed for **one user, one machine, one project**. The PM spec states:

> *"The dashboard will be used by 1 person at a time on their local machine - no concurrent access concerns."*

There is no scaling strategy because scaling is out of scope. If the need arises for multi-user or multi-project support, the correct approach is to build a new application — not to scale this one.

### Data Volume Limits

The design constrains data volume through the fixed 1920×1080 viewport:

- **Max 6 months:** Beyond 6, column widths become too narrow for readable 12px text
- **Max 5 tracks:** Beyond 5, track lines overlap in the 185px SVG height
- **Max ~10 items per cell:** Beyond 10, text overflows the cell height (hidden by `overflow: hidden`)
- **Max 4 categories:** The color scheme supports exactly 4 rows (green, blue, amber, red)

These are visual constraints, not technical ones. The code should handle edge cases gracefully (truncation, not crashes).

---

## Risks & Mitigations

### Risk Registry

| # | Risk | Probability | Impact | Mitigation |
|---|------|------------|--------|------------|
| 1 | **CSS fidelity drift** — Blazor's HTML output differs subtly from the original design concept, causing visual discrepancies | Medium | High | Port CSS verbatim from `OriginalDesignConcept.html`. Do not rewrite or "improve" it. Verify with side-by-side screenshot comparison at 1920×1080 during development. |
| 2 | **SVG coordinate miscalculation** — Milestones appear at wrong positions on the timeline due to date-to-pixel math errors | Medium | Medium | Use `DateOnly` for all date arithmetic (avoids timezone issues). Write unit tests for `DateToX()` with known input/output pairs. Visually verify against the original design's hardcoded positions. |
| 3 | **Font rendering differences** — Segoe UI unavailable on macOS/Linux causes spacing shifts and text wrapping differences | Low | Low | Accept Arial fallback on non-Windows. Primary use case is Windows + Edge. Document this in README. |
| 4 | **Blazor framework chrome** — Default Blazor template includes navigation sidebar, error UI, or `blazor.web.js` that pollutes the screenshot | Medium | High | Use `--interactivity None` template. Strip `DashboardLayout.razor` to bare `@Body`. Verify no `<script>` tags in rendered HTML. Remove all default template files (NavMenu, Counter, Weather, etc.). |
| 5 | **JSON schema mismatch** — PM edits `data.json` with wrong field names or types, causing silent rendering failures | Medium | Medium | Validate required fields after deserialization. Display clear error messages for null/empty required fields. Provide `data.sample.json` as a template. |
| 6 | **Overflow/truncation at edge data volumes** — 6 months × 10 items per cell exceeds visible area, producing a broken layout | Low | Medium | CSS `overflow: hidden` on cells prevents layout breakage. Document max recommended items per cell in `data.sample.json` comments. Test with maximum data volume. |
| 7 | **`dotnet run` startup confusion** — New developer doesn't have .NET 8 SDK installed, or runs from wrong directory | Low | Low | Document prerequisites and exact run command in README. `launchSettings.json` ensures correct port. Error message if `data.json` missing guides user to copy sample file. |
| 8 | **Browser zoom/DPI scaling** — Screenshot taken at non-100% zoom produces wrong dimensions | Low | Medium | Document in README: "Set browser zoom to 100% before screenshotting." The fixed 1920×1080 CSS ensures correct dimensions at 100% zoom only. |

### Decisions Log

| Decision | Rationale | Reversibility |
|----------|-----------|---------------|
| Read `data.json` on every request (no caching) | Enables instant refresh after file edit; file is <10KB so disk I/O is negligible | Easy — add `IMemoryCache` with file watcher if needed |
| Single CSS file (no scoped CSS) | Original design is one file; splitting adds complexity without benefit for 3 components | Easy — extract into component-scoped `.razor.css` files |
| All models in one file | ~60 lines of record types; separate files would be over-engineering | Easy — split into individual files |
| No JavaScript at all | Static SSR mandate; no interactive features needed | Medium — adding interactivity requires switching render mode |
| Error messages as styled HTML (not HTTP error codes) | Target audience is a PM viewing in a browser, not an API consumer | Easy — add proper HTTP status codes if needed |