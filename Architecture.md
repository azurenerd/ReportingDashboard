# Architecture

## Overview & Goals

This document defines the complete system architecture for the **Executive Reporting Dashboard** — a single-page, screenshot-ready Blazor Server application that renders project milestone timelines and monthly execution heatmaps at exactly 1920×1080 pixels. The dashboard reads all content from a local `data.json` file and produces a polished visual suitable for direct capture into PowerPoint decks.

### Architectural Goals

1. **Pixel-perfect visual fidelity** — Match `OriginalDesignConcept.html` exactly at 1920×1080. The rendered output is the product.
2. **Data-driven rendering** — 100% of displayed content sourced from `data.json`. Zero hardcoded project-specific text in components.
3. **Zero operational overhead** — No database, no authentication, no deployment pipeline. Clone, `dotnet run`, screenshot.
4. **Flat simplicity** — ≤8 Razor components, <1000 lines of C#/Razor. No enterprise patterns, no unnecessary abstractions.
5. **Sub-2-second render** — Full dashboard renders within 2 seconds on localhost.

### Architectural Pattern

**Flat Component Architecture with JSON Data Source.** The entire data flow is unidirectional:

```
data.json → DataService (singleton) → Dashboard.razor → Child Components → HTML/CSS/SVG
```

This is deliberately not Clean Architecture, CQRS, or any layered enterprise pattern. The application is a data-to-pixels pipeline.

---

## System Components

### 1. Solution Structure

```
ReportingDashboard.sln
├── ReportingDashboard.Web/          # Blazor Server application (Microsoft.NET.Sdk.Web)
│   ├── Program.cs                   # Host builder, service registration
│   ├── Pages/
│   │   └── Dashboard.razor          # Single page at route "/"
│   ├── Components/
│   │   ├── DashboardHeader.razor    # Header bar: title, subtitle, legend
│   │   ├── DashboardHeader.razor.css
│   │   ├── TimelineSection.razor    # SVG timeline with tracks and milestones
│   │   ├── TimelineSection.razor.css
│   │   ├── HeatmapGrid.razor       # CSS Grid heatmap container
│   │   ├── HeatmapGrid.razor.css
│   │   ├── HeatmapRow.razor        # Single status row (Shipped/Progress/etc.)
│   │   ├── HeatmapRow.razor.css
│   │   ├── HeatmapCell.razor       # Single month cell with bullet items
│   │   └── HeatmapCell.razor.css
│   ├── Services/
│   │   ├── IDataService.cs          # Interface for data access
│   │   └── DataService.cs           # JSON file reader with FileSystemWatcher
│   ├── wwwroot/
│   │   └── css/
│   │       └── app.css              # Global styles, CSS custom properties, body fixed viewport
│   ├── _Imports.razor               # Global using directives
│   ├── App.razor                    # Root Blazor component (router shell)
│   ├── MainLayout.razor             # Minimal layout (no nav, no sidebar)
│   ├── MainLayout.razor.css
│   ├── data.json                    # Dashboard configuration data
│   └── ReportingDashboard.Web.csproj
│
└── ReportingDashboard.Models/       # Shared data model records
    ├── DashboardData.cs             # Top-level model
    ├── ProjectHeader.cs
    ├── TimelineConfig.cs
    ├── Track.cs
    ├── Milestone.cs
    ├── MilestoneType.cs             # Enum: Poc, Production, Checkpoint, CheckpointMinor
    ├── HeatmapData.cs
    ├── StatusRow.cs
    ├── MonthCell.cs
    └── ReportingDashboard.Models.csproj
```

### 2. Component Specifications

#### 2.1 `DataService` — Data Access Singleton

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Load, deserialize, cache, and optionally watch `data.json` |
| **Interface** | `IDataService` with `DashboardData? GetData()` and `string? GetError()` |
| **Lifecycle** | Registered as `Singleton` in DI container |
| **Dependencies** | `System.Text.Json`, `System.IO.FileSystemWatcher`, `ReportingDashboard.Models` |
| **Thread safety** | Uses `volatile` field for cached data; `FileSystemWatcher` callback reloads atomically |
| **Error handling** | Catches `JsonException` and `FileNotFoundException`, stores error message for display |

```csharp
public interface IDataService
{
    DashboardData? GetData();
    string? GetError();
    event Action? OnDataChanged;
}
```

**Implementation details:**

- Reads `data.json` from `AppContext.BaseDirectory` (or configurable path via `appsettings.json`)
- Deserializes with `JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }`
- `FileSystemWatcher` monitors for changes and triggers `OnDataChanged` event
- Debounces file change events (300ms) to avoid duplicate `Changed` notifications from OS
- On error, `GetData()` returns `null` and `GetError()` returns the exception message

#### 2.2 `Dashboard.razor` — Page Compositor

| Aspect | Detail |
|--------|--------|
| **Route** | `@page "/"` |
| **Responsibility** | Inject `IDataService`, check for errors, compose child components |
| **Parameters** | None (root page) |
| **Renders** | Error panel if data is null; otherwise `DashboardHeader` + `TimelineSection` + `HeatmapGrid` |
| **Lifecycle** | Subscribes to `IDataService.OnDataChanged` in `OnInitialized`; calls `StateHasChanged` on data reload |

#### 2.3 `DashboardHeader.razor` — Header Bar

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render project title, ADO backlog link, subtitle, and milestone legend |
| **Parameters** | `[Parameter] ProjectHeader Header` |
| **HTML structure** | `<div class="hdr">` with flexbox row layout |
| **CSS isolation** | `DashboardHeader.razor.css` — matches `.hdr`, `.sub` classes from reference |

#### 2.4 `TimelineSection.razor` — SVG Timeline

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render track label sidebar + inline SVG with gridlines, track lines, NOW indicator, and milestone markers |
| **Parameters** | `[Parameter] TimelineConfig Timeline`, `[Parameter] List<Milestone> Milestones` |
| **HTML structure** | `<div class="tl-area">` containing sidebar div + SVG element |
| **SVG dimensions** | 1560px × 185px, `overflow: visible` |
| **Computed values** | `DateToX()` method for date→pixel mapping; `TrackToY()` for track→Y-position mapping |
| **CSS isolation** | `TimelineSection.razor.css` — matches `.tl-area`, `.tl-svg-box` classes |

**Key computation methods (defined as `private` in the component `@code` block):**

```csharp
private const double SvgWidth = 1560.0;
private const double SvgHeight = 185.0;

private double DateToX(DateTime date)
{
    double totalDays = (Timeline.EndDate - Timeline.StartDate).TotalDays;
    double dayOffset = (date - Timeline.StartDate).TotalDays;
    return Math.Clamp((dayOffset / totalDays) * SvgWidth, 0, SvgWidth);
}

private double TrackToY(int trackIndex, int trackCount)
{
    // Evenly distribute tracks within SVG height, offset from top
    // For 3 tracks: y ≈ 42, 98, 154 (matching reference)
    double spacing = SvgHeight / (trackCount + 1);
    return spacing * (trackIndex + 1);
}

private string DiamondPoints(double cx, double cy, double size = 11)
{
    return $"{cx},{cy - size} {cx + size},{cy} {cx},{cy + size} {cx - size},{cy}";
}
```

#### 2.5 `HeatmapGrid.razor` — Heatmap Container

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render CSS Grid container with header row (corner + column headers) and compose `HeatmapRow` children |
| **Parameters** | `[Parameter] HeatmapData Heatmap` |
| **HTML structure** | `<div class="hm-wrap">` → title div → `<div class="hm-grid">` |
| **CSS Grid** | `grid-template-columns: 160px repeat(N, 1fr)` where N = `Heatmap.Columns.Count` |
| **CSS Grid rows** | `grid-template-rows: 36px repeat(4, 1fr)` |
| **Dynamic style** | Grid column template set via inline `style` attribute bound to column count |

#### 2.6 `HeatmapRow.razor` — Status Row

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render row header cell + N `HeatmapCell` children |
| **Parameters** | `[Parameter] StatusRow Row`, `[Parameter] int HighlightColumnIndex`, `[Parameter] int ColumnCount` |
| **CSS class mapping** | `colorTheme` value maps to CSS class prefix: `shipped` → `.ship-*`, `progress` → `.prog-*`, `carryover` → `.carry-*`, `blockers` → `.block-*` |

#### 2.7 `HeatmapCell.razor` — Month Cell

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render bullet-pointed items or empty-state dash |
| **Parameters** | `[Parameter] MonthCell Cell`, `[Parameter] string ColorTheme`, `[Parameter] bool IsHighlighted` |
| **Empty state** | If `Cell.Items` is empty, render single `<div class="it">` with text `"-"` in color `#AAA` |
| **CSS class** | Applies highlight class (e.g., `.apr`) when `IsHighlighted` is true |

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────┐     ┌──────────────────┐     ┌─────────────────────┐
│  data.json   │────▶│   DataService    │────▶│   Dashboard.razor    │
│  (on disk)   │     │  (Singleton DI)  │     │   (Page, route /)    │
└─────────────┘     │                  │     └─────────┬───────────┘
       ▲             │  - Load + cache  │               │
       │             │  - FileWatcher   │               │ [Parameter] passing
  User edits         │  - Error capture │               │
  file manually      └──────────────────┘               ▼
                                              ┌─────────────────────┐
                                              │  DashboardHeader     │◀── ProjectHeader
                                              ├─────────────────────┤
                                              │  TimelineSection     │◀── TimelineConfig + List<Milestone>
                                              ├─────────────────────┤
                                              │  HeatmapGrid         │◀── HeatmapData
                                              │   ├─ HeatmapRow ×4   │◀── StatusRow + highlight index
                                              │   │   └─ HeatmapCell │◀── MonthCell + theme + highlight flag
                                              └─────────────────────┘
```

### Communication Patterns

1. **Startup flow:** `Program.cs` → registers `DataService` as singleton → `DataService` constructor calls `LoadData()` → reads and deserializes `data.json` → caches `DashboardData` instance.

2. **Render flow:** Browser connects via SignalR → Blazor renders `Dashboard.razor` → `Dashboard` injects `IDataService` → calls `GetData()` → if null, renders error panel with `GetError()` message → if valid, passes sub-objects to child components via `[Parameter]` → components render HTML/CSS/SVG.

3. **Live reload flow (optional):** User edits `data.json` on disk → `FileSystemWatcher.Changed` fires → `DataService.LoadData()` re-reads file → fires `OnDataChanged` event → `Dashboard.razor` handler calls `InvokeAsync(StateHasChanged)` → Blazor re-renders component tree via SignalR → browser updates.

4. **Error flow:** `data.json` missing or malformed → `DataService` catches exception → stores error string → `GetData()` returns null → `Dashboard.razor` renders error panel with red-bordered message box showing file path and parse error details.

### Component Dependency Graph

```
Program.cs
  └── registers IDataService → DataService (singleton)

Dashboard.razor
  ├── @inject IDataService
  ├── DashboardHeader.razor  (leaf component)
  ├── TimelineSection.razor  (leaf component, contains inline SVG rendering logic)
  └── HeatmapGrid.razor
       └── HeatmapRow.razor (×4)
            └── HeatmapCell.razor (×N per row)
```

All components are **pure renderers** — they receive data via `[Parameter]` and emit markup. No component calls services directly except `Dashboard.razor`.

---

## Data Model

### Entity Definitions

All models live in the `ReportingDashboard.Models` project as C# records for immutability and value-based equality.

```csharp
// Root container — maps to entire data.json
public record DashboardData(
    ProjectHeader Header,
    TimelineConfig Timeline,
    HeatmapData Heatmap
);

// Header section data
public record ProjectHeader(
    string Title,           // e.g., "Project Phoenix Release Roadmap"
    string Subtitle,        // e.g., "Platform Engineering · Phoenix Workstream · April 2026"
    string BacklogUrl,      // e.g., "https://dev.azure.com/org/project"
    string CurrentMonth     // e.g., "April 2026" — used in legend "Now" label
);

// Timeline configuration and data
public record TimelineConfig(
    DateTime StartDate,     // Left edge of timeline (e.g., 2026-01-01)
    DateTime EndDate,       // Right edge of timeline (e.g., 2026-06-30)
    DateTime NowDate,       // Position of the "NOW" indicator line
    List<Track> Tracks,     // Horizontal track lines (M1, M2, M3, etc.)
    List<Milestone> Milestones  // All markers placed on tracks
);

// A horizontal track line in the timeline
public record Track(
    string Id,              // e.g., "m1" — matches Milestone.TrackId
    string Label,           // e.g., "M1"
    string Description,     // e.g., "Core API & Auth"
    string Color            // Hex color, e.g., "#0078D4"
);

// A single milestone marker on a track
public record Milestone(
    string TrackId,         // References Track.Id
    DateTime Date,          // Position on the timeline
    string Label,           // Display text, e.g., "Mar 20 PoC"
    string Type,            // "poc", "production", "checkpoint", "checkpoint-minor"
    string? Description     // Optional longer description
);

// Heatmap grid data
public record HeatmapData(
    List<string> Columns,           // Month names: ["January", "February", ...]
    int HighlightColumnIndex,       // 0-based index of current month column
    List<StatusRow> Rows            // Exactly 4 rows: Shipped, In Progress, Carryover, Blockers
);

// One row in the heatmap (one status category)
public record StatusRow(
    string Category,        // Display name: "Shipped", "In Progress", "Carryover", "Blockers"
    string ColorTheme,      // CSS class prefix: "shipped", "progress", "carryover", "blockers"
    List<MonthCell> Cells   // One cell per month column
);

// One cell in the heatmap grid
public record MonthCell(
    List<string> Items      // Bullet-point text items; empty list = show dash
);
```

### Entity Relationships

```
DashboardData (1)
  ├── ProjectHeader (1)
  ├── TimelineConfig (1)
  │     ├── Track (1..5)          # Supports 2-5 tracks
  │     └── Milestone (0..N)      # Each references a Track.Id
  └── HeatmapData (1)
        └── StatusRow (4, fixed)  # Shipped, In Progress, Carryover, Blockers
              └── MonthCell (3..6) # One per month column
```

### Storage

- **Persistence layer:** Single `data.json` file on disk, located in the application's base directory
- **Format:** UTF-8 encoded JSON, camelCase property names
- **Size:** Expected <10KB; no pagination or streaming needed
- **Schema validation:** Implicit via `System.Text.Json` deserialization to strongly-typed records. Missing required properties throw `JsonException`, caught by `DataService`
- **No database.** No SQLite, no Entity Framework, no migration scripts. The JSON file IS the database.

### `data.json` Canonical Schema

```json
{
  "header": {
    "title": "string (required)",
    "subtitle": "string (required)",
    "backlogUrl": "string (required, URL)",
    "currentMonth": "string (required, display text)"
  },
  "timeline": {
    "startDate": "string (required, ISO 8601 date: YYYY-MM-DD)",
    "endDate": "string (required, ISO 8601 date: YYYY-MM-DD)",
    "nowDate": "string (required, ISO 8601 date: YYYY-MM-DD)",
    "tracks": [
      {
        "id": "string (required, unique identifier)",
        "label": "string (required, short display label)",
        "description": "string (required, track description)",
        "color": "string (required, hex color #RRGGBB)"
      }
    ],
    "milestones": [
      {
        "trackId": "string (required, references tracks[].id)",
        "date": "string (required, ISO 8601 date)",
        "label": "string (required, display text for marker)",
        "type": "string (required, one of: poc | production | checkpoint | checkpoint-minor)",
        "description": "string (optional, longer description)"
      }
    ]
  },
  "heatmap": {
    "columns": ["string (month names)"],
    "highlightColumnIndex": "integer (0-based, required)",
    "rows": [
      {
        "category": "string (required, display name)",
        "colorTheme": "string (required, one of: shipped | progress | carryover | blockers)",
        "cells": [
          {
            "items": ["string (bullet text items, empty array for no items)"]
          }
        ]
      }
    ]
  }
}
```

---

## API Contracts

This application has **no REST API, no Web API controllers, and no HTTP endpoints** beyond the default Blazor Server infrastructure. The "API" is the contract between `data.json` and the `DataService`.

### Internal Service Contract

```csharp
public interface IDataService
{
    /// Returns the deserialized dashboard data, or null if data.json is missing/malformed.
    DashboardData? GetData();

    /// Returns the error message if GetData() returns null; otherwise null.
    string? GetError();

    /// Fired when data.json changes on disk and is successfully reloaded.
    event Action? OnDataChanged;
}
```

### Blazor Routes

| Route | Component | Description |
|-------|-----------|-------------|
| `/` | `Dashboard.razor` | Single page — renders the full dashboard or error panel |

No other routes exist. The default Blazor template's `Counter`, `FetchData`, and `NavMenu` are removed.

### Error Handling Contract

| Condition | Behavior |
|-----------|----------|
| `data.json` not found | `GetError()` returns `"data.json not found at {absolutePath}. Create this file with your dashboard data."` |
| `data.json` contains invalid JSON | `GetError()` returns `"Failed to parse data.json: {JsonException.Message}"` |
| `data.json` has missing required fields | `GetError()` returns the `JsonException` message from deserialization (e.g., "JSON deserialization for type 'DashboardData' was missing required properties") |
| `data.json` valid | `GetData()` returns `DashboardData` instance; `GetError()` returns `null` |

### Error Display

When `GetData()` returns null, `Dashboard.razor` renders:

```html
<div style="padding: 60px; font-family: 'Segoe UI', Arial, sans-serif;">
    <h1 style="color: #EA4335;">Dashboard Configuration Error</h1>
    <p style="font-size: 16px; color: #333; margin-top: 16px;">@errorMessage</p>
    <p style="font-size: 13px; color: #888; margin-top: 12px;">
        Edit data.json and refresh this page.
    </p>
</div>
```

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|---------------|
| **Operating System** | Windows 10/11 (Segoe UI font dependency) |
| **.NET SDK** | .NET 8.0 LTS (8.0.x) |
| **IDE** | Visual Studio 2022 or VS Code with C# Dev Kit |
| **Browser** | Microsoft Edge or Google Chrome (latest stable) |
| **Display** | Any resolution — browser DevTools device emulation at 1920×1080 for screenshots |

### Hosting

| Aspect | Specification |
|--------|---------------|
| **Web server** | Kestrel (built into ASP.NET Core) |
| **Ports** | `https://localhost:5001` / `http://localhost:5000` (default Kestrel) |
| **Process model** | Single process, in-process Kestrel |
| **Launch command** | `dotnet run --project ReportingDashboard.Web` or `dotnet watch --project ReportingDashboard.Web` |

### Storage

| Item | Location | Size |
|------|----------|------|
| `data.json` | `ReportingDashboard.Web/data.json` (copied to output directory) | <10KB |
| Application binaries | `bin/Debug/net8.0/` | ~5MB (framework-dependent) |
| Static assets | `wwwroot/css/app.css` | <5KB |

### Networking

- **Localhost only.** No inbound or outbound network connections required.
- Blazor Server uses a SignalR WebSocket connection between browser and Kestrel — all on localhost.
- No external API calls, no CDN references, no remote font loading.

### CI/CD

- **None required.** This is a local development tool.
- `dotnet build` should produce zero errors and zero warnings — this is the sole build quality gate.
- Optional: A `build.ps1` or `Makefile` for convenience, but not required.

### File System Layout (Runtime)

```
ReportingDashboard.Web/
├── bin/Debug/net8.0/
│   ├── ReportingDashboard.Web.dll
│   ├── ReportingDashboard.Models.dll
│   ├── data.json          ← CopyToOutputDirectory: PreserveNewest
│   └── wwwroot/
│       └── css/app.css
├── data.json              ← Source file (user edits this)
└── appsettings.json
```

**`data.json` copy strategy:** The `.csproj` must include:

```xml
<ItemGroup>
    <None Update="data.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

However, `DataService` should resolve the file path relative to the **project root** (not output directory) during development, so edits to the source `data.json` are reflected immediately. Use `IWebHostEnvironment.ContentRootPath` to locate the file.

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **Application framework** | Blazor Server (.NET 8) | Mandated by project requirements. Provides component model, DI, and hot reload. Overkill for static content but acceptable for local use. |
| **CSS layout** | Native CSS Grid + Flexbox | The reference design uses exactly these primitives. No library can match a bespoke design faster than copying the proven CSS. |
| **Timeline rendering** | Inline SVG in Razor markup | The timeline uses `<line>`, `<circle>`, `<polygon>`, `<text>` — basic SVG primitives. No charting library needed. Inline SVG allows data binding in Razor. |
| **JSON handling** | `System.Text.Json` (built-in) | Ships with .NET 8. Fast, zero-dependency, native support for records. No reason to add Newtonsoft.Json. |
| **CSS architecture** | Blazor CSS isolation (`.razor.css`) + global `app.css` | Scoped styles prevent leakage between components. Global `app.css` holds CSS custom properties and body-level styles. |
| **Data storage** | `data.json` file on disk | <100 items, hand-edited by one person. A database adds migration complexity for zero benefit. JSON is human-readable and version-controllable. |
| **Component library** | None (no MudBlazor, no Radzen) | The design is pixel-specific. Third-party libraries impose their own design language and add 500KB+ of CSS. Matching the reference with vanilla CSS is faster and more accurate. |
| **State management** | Singleton `DataService` + `[Parameter]` drilling | The data is read-only and small. No need for Fluxor, cascading values, or state containers. Simple parameter passing is sufficient for ≤3 levels of nesting. |
| **Font** | Segoe UI (system font) | Windows-only tool; Segoe UI is pre-installed. No web font loading, no FOUT, no external requests. |
| **Additional NuGet packages** | Zero | Everything needed ships with the .NET 8 SDK. No package restore surprises, no version conflicts. |
| **Testing** | Manual visual inspection (primary); optional xUnit for `DateToX()` math | The product is a visual screenshot. Automated UI testing costs more than the bugs it would catch in a <1000-line app. |

---

## Security Considerations

### Authentication & Authorization

**None.** This is explicitly a single-user, localhost-only tool. No authentication middleware is registered. No `[Authorize]` attributes exist. The `builder.Services.AddAuthentication()` call is **not present** in `Program.cs`.

### Data Protection

| Concern | Assessment |
|---------|------------|
| **Data classification** | `data.json` contains non-sensitive project status information — milestone names, status items, dates. No PII, no credentials, no secrets. |
| **Encryption at rest** | Not required. Data is non-sensitive and stored on the user's local disk. |
| **Encryption in transit** | Kestrel defaults to HTTPS with a dev cert. Acceptable for localhost; not a hard requirement. |
| **Data exfiltration** | Not applicable — no network egress, no telemetry, no analytics. |

### Input Validation

| Input | Validation Strategy |
|-------|-------------------|
| `data.json` content | `System.Text.Json` strongly-typed deserialization rejects structurally invalid data. Missing required properties throw `JsonException`. |
| `data.json` dates | `DateTime` deserialization validates ISO 8601 format. Invalid dates throw during deserialization. |
| `highlightColumnIndex` | Must be 0 ≤ index < `columns.Count`. Bounds-checked in `HeatmapGrid.razor` before use. Out-of-range values default to -1 (no highlight). |
| `colorTheme` values | Mapped to known CSS class prefixes via a `switch` expression. Unknown values fall through to a neutral/unstyled default. |
| Track/Milestone references | `Milestone.TrackId` is matched against `Track.Id`. Unmatched milestones are silently skipped (not rendered). |
| SVG coordinate values | All computed via `DateToX()` and `TrackToY()` with `Math.Clamp()` to prevent coordinates outside the SVG viewport. |

### Supply Chain

- Zero third-party NuGet packages = zero supply chain attack surface beyond the .NET SDK itself.
- No JavaScript libraries, no npm dependencies, no CDN references.

### HTTPS Configuration

`Program.cs` uses the default Kestrel HTTPS setup with the .NET dev certificate. No custom certificate configuration is needed:

```csharp
var builder = WebApplication.CreateBuilder(args);
// No explicit HTTPS configuration — Kestrel defaults are sufficient
```

---

## Scaling Strategy

### Scaling Is Not Applicable

This application is a **single-user, local-only tool** that runs on one developer's workstation. There is no multi-user scenario, no concurrent access, no production deployment, and no load to scale for.

| Scaling Dimension | Assessment |
|-------------------|------------|
| **Horizontal scaling** | Not applicable. Single process, single user, localhost. |
| **Vertical scaling** | Not needed. The app uses <100MB RAM and renders in <2 seconds. |
| **Data scaling** | `data.json` will never exceed ~10KB. No pagination, streaming, or indexing needed. |
| **Concurrent users** | One. Blazor Server's SignalR circuit handles one browser tab. |
| **Content scaling** | The dashboard supports 2–5 timeline tracks and 3–6 heatmap month columns. Beyond 6 months, columns become too narrow for readable text — this is a design constraint, not a technical limitation. |

### Performance Budget

| Metric | Target | Mechanism |
|--------|--------|-----------|
| Page load | <2 seconds | Data is pre-loaded in singleton; no database queries, no API calls |
| Memory | <100MB | Trivial data size; Blazor Server baseline is ~50MB |
| `data.json` read | <1ms | File is <10KB, read synchronously on startup |
| SVG render | <100ms | <50 SVG elements; no complex paths or animations |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component with its CSS layout strategy, data bindings, and interactions.

### Visual-to-Component Mapping

#### Section 1: Header Bar → `DashboardHeader.razor`

| Visual Element | HTML/CSS Strategy | Data Binding | Interactions |
|----------------|-------------------|--------------|--------------|
| **Outer container** (`.hdr`) | `display: flex; align-items: center; justify-content: space-between; padding: 12px 44px 10px; border-bottom: 1px solid #E0E0E0` | — | None (static render) |
| **Project title** | `<h1>` at `font-size: 24px; font-weight: 700` | `@Header.Title` | None |
| **ADO Backlog link** | `<a>` inline in h1, `color: #0078D4; text-decoration: none` | `href="@Header.BacklogUrl"` with text `"→ ADO Backlog"` | Clickable link (opens in browser) |
| **Subtitle** | `<div class="sub">` at `font-size: 12px; color: #888; margin-top: 2px` | `@Header.Subtitle` | None |
| **Legend (right side)** | `display: flex; gap: 22px; align-items: center` | `@Header.CurrentMonth` in NOW label | None |
| **PoC diamond icon** | `width: 12px; height: 12px; background: #F4B400; transform: rotate(45deg); display: inline-block` | — | None |
| **Production diamond icon** | Same as PoC but `background: #34A853` | — | None |
| **Checkpoint circle icon** | `width: 8px; height: 8px; border-radius: 50%; background: #999` | — | None |
| **NOW indicator icon** | `width: 2px; height: 14px; background: #EA4335` | — | None |

#### Section 2: Timeline Area → `TimelineSection.razor`

| Visual Element | HTML/CSS Strategy | Data Binding | Interactions |
|----------------|-------------------|--------------|--------------|
| **Outer container** (`.tl-area`) | `display: flex; align-items: stretch; height: 196px; background: #FAFAFA; border-bottom: 2px solid #E8E8E8; padding: 6px 44px 0` | — | None |
| **Track labels sidebar** | `width: 230px; flex-shrink: 0; display: flex; flex-direction: column; justify-content: space-around; border-right: 1px solid #E0E0E0; padding: 16px 12px 16px 0` | `@foreach (var track in Timeline.Tracks)` | None |
| **Individual track label** | `font-size: 12px; font-weight: 600; line-height: 1.4; color: {track.Color}` with description span at `font-weight: 400; color: #444` | `@track.Label`, `@track.Description`, `style="color: @track.Color"` | None |
| **SVG container** (`.tl-svg-box`) | `flex: 1; padding-left: 12px; padding-top: 6px` | — | None |
| **SVG element** | `<svg width="1560" height="185" style="overflow:visible;display:block">` | — | None |
| **Drop shadow filter** | `<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>` | — | — |
| **Month gridlines** | `<line x1="{x}" y1="0" x2="{x}" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>` | X computed: iterate months between `StartDate` and `EndDate`, call `DateToX(firstOfMonth)` | None |
| **Month labels** | `<text x="{x+5}" y="14" fill="#666" font-size="11" font-weight="600">` | Month abbreviation from `DateTime` | None |
| **NOW line** | `<line x1="{nowX}" y1="0" x2="{nowX}" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>` | `nowX = DateToX(Timeline.NowDate)` | None |
| **NOW label** | `<text x="{nowX+4}" y="14" fill="#EA4335" font-size="10" font-weight="700">NOW</text>` | Same X as NOW line | None |
| **Track lines** | `<line x1="0" y1="{y}" x2="1560" y2="{y}" stroke="{track.Color}" stroke-width="3"/>` | `y = TrackToY(index, count)`, `stroke` from `track.Color` | None |
| **PoC milestone diamond** | `<polygon points="{DiamondPoints(x, y)}" fill="#F4B400" filter="url(#sh)"/>` | `x = DateToX(milestone.Date)`, `y = TrackToY(trackIndex)` | None |
| **Production milestone diamond** | Same as PoC but `fill="#34A853"` | Same computation | None |
| **Major checkpoint circle** | `<circle cx="{x}" cy="{y}" r="7" fill="white" stroke="{trackColor}" stroke-width="2.5"/>` | Same computation | None |
| **Minor checkpoint circle** | `<circle cx="{x}" cy="{y}" r="4" fill="#999"/>` | Same computation | None |
| **Milestone labels** | `<text x="{x}" y="{labelY}" text-anchor="middle" fill="#666" font-size="10">` | `@milestone.Label`; `labelY = y - 16` (above) or `y + 24` (below), alternating per milestone index to avoid overlap | None |

#### Section 3: Heatmap Area → `HeatmapGrid.razor` + `HeatmapRow.razor` + `HeatmapCell.razor`

| Visual Element | Component | CSS Strategy | Data Binding |
|----------------|-----------|--------------|--------------|
| **Heatmap wrapper** (`.hm-wrap`) | `HeatmapGrid` | `flex: 1; min-height: 0; display: flex; flex-direction: column; padding: 10px 44px 10px` | — |
| **Section title** (`.hm-title`) | `HeatmapGrid` | `font-size: 14px; font-weight: 700; color: #888; letter-spacing: 0.5px; text-transform: uppercase; margin-bottom: 8px` | Static text: `"Monthly Execution Heatmap"` |
| **Grid container** (`.hm-grid`) | `HeatmapGrid` | `display: grid; grid-template-columns: 160px repeat(@columnCount, 1fr); grid-template-rows: 36px repeat(4, 1fr); border: 1px solid #E0E0E0` | `columnCount = Heatmap.Columns.Count` (inline style) |
| **Corner cell** (`.hm-corner`) | `HeatmapGrid` | `background: #F5F5F5; font-size: 11px; font-weight: 700; color: #999; text-transform: uppercase; text-align: center; border-right: 1px solid #E0E0E0; border-bottom: 2px solid #CCC` | Static text: `"Status"` |
| **Column headers** (`.hm-col-hdr`) | `HeatmapGrid` | `font-size: 16px; font-weight: 700; background: #F5F5F5; text-align: center; border-right: 1px solid #E0E0E0; border-bottom: 2px solid #CCC` | `@foreach Heatmap.Columns`; highlighted column gets additional class with `background: #FFF0D0; color: #C07700` |
| **Row header** (`.hm-row-hdr`) | `HeatmapRow` | `font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.7px; padding: 0 12px; border-right: 2px solid #CCC; border-bottom: 1px solid #E0E0E0` + theme-specific background and text color | `@Row.Category`, CSS class from `Row.ColorTheme` |
| **Data cell** (`.hm-cell`) | `HeatmapCell` | `padding: 8px 12px; border-right: 1px solid #E0E0E0; border-bottom: 1px solid #E0E0E0; overflow: hidden` + theme background + highlight variant | `@foreach Cell.Items` |
| **Cell item bullet** (`.it::before`) | `HeatmapCell` | `content: ''; position: absolute; left: 0; top: 7px; width: 6px; height: 6px; border-radius: 50%; background: {themeColor}` | Color set by parent `ColorTheme` CSS class |
| **Empty cell dash** | `HeatmapCell` | Same `.it` styling but text content is `"-"` with `color: #AAA` | Rendered when `Cell.Items.Count == 0` |

### CSS Class-to-Theme Mapping

The `colorTheme` string from `data.json` maps to CSS class prefixes used across `HeatmapRow` and `HeatmapCell`:

| `colorTheme` | Row Header CSS | Cell CSS | Highlight Cell CSS | Bullet `::before` |
|---------------|---------------|----------|-------------------|--------------------|
| `"shipped"` | `.ship-hdr` — bg `#E8F5E9`, color `#1B7A28` | `.ship-cell` — bg `#F0FBF0` | `.ship-cell.highlight` — bg `#D8F2DA` | `#34A853` |
| `"progress"` | `.prog-hdr` — bg `#E3F2FD`, color `#1565C0` | `.prog-cell` — bg `#EEF4FE` | `.prog-cell.highlight` — bg `#DAE8FB` | `#0078D4` |
| `"carryover"` | `.carry-hdr` — bg `#FFF8E1`, color `#B45309` | `.carry-cell` — bg `#FFFDE7` | `.carry-cell.highlight` — bg `#FFF0B0` | `#F4B400` |
| `"blockers"` | `.block-hdr` — bg `#FEF2F2`, color `#991B1B` | `.block-cell` — bg `#FFF5F5` | `.block-cell.highlight` — bg `#FFE4E4` | `#EA4335` |

### Global CSS Custom Properties (`wwwroot/css/app.css`)

```css
*, *::before, *::after { margin: 0; padding: 0; box-sizing: border-box; }

:root {
    --font-primary: 'Segoe UI', Arial, sans-serif;
    --color-text: #111;
    --color-link: #0078D4;
    --color-border: #E0E0E0;
    --color-border-heavy: #CCC;
    --color-subtitle: #888;
    --color-now: #EA4335;
    --color-poc: #F4B400;
    --color-production: #34A853;
    --color-checkpoint: #999;
}

html, body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
    background: #FFFFFF;
    font-family: var(--font-primary);
    color: var(--color-text);
    display: flex;
    flex-direction: column;
}

a { color: var(--color-link); text-decoration: none; }

#blazor-error-ui { display: none !important; }
```

The `#blazor-error-ui { display: none !important; }` rule hides the default Blazor reconnection/error banner to keep screenshots clean.

---

## Risks & Mitigations

### Technical Risks

| # | Risk | Severity | Likelihood | Impact | Mitigation |
|---|------|----------|------------|--------|------------|
| 1 | **SVG date-to-pixel math errors** — Milestones render at wrong positions | Medium | Medium | Timeline is visually incorrect; defeats purpose of the tool | Implement `DateToX()` as a pure function with clamping. Validate with known dates from the reference HTML (e.g., Jan gridline at x=0, Apr at x=780 for a Jan–Jun range). |
| 2 | **CSS Grid inconsistency between Edge and Chrome** — Heatmap renders differently in each browser | Low | Low | Screenshots look different depending on browser | Target a single browser for captures. Test both initially; pick one as canonical. CSS Grid is well-standardized in modern browsers. |
| 3 | **`FileSystemWatcher` double-firing** — OS fires multiple `Changed` events for one save | Low | High | `DataService.LoadData()` called multiple times; brief read error if file is mid-write | Debounce with 300ms `Timer`. Read file in a try-catch; retry once on `IOException`. |
| 4 | **Malformed `data.json` crashes app** — User typo causes unhandled exception | Medium | Medium | Blank page or cryptic ASP.NET error | `DataService` wraps all JSON operations in try-catch. `Dashboard.razor` checks `GetData() == null` and renders error panel. Never let exceptions escape to the user. |
| 5 | **Blazor CSS isolation selector specificity conflicts** — Scoped styles don't apply as expected | Low | Low | Visual mismatch with reference | Use `::deep` combinator sparingly. Keep component HTML structure flat. Test each component in isolation. |
| 6 | **SVG text label overlap** — Milestone labels collide when dates are close together | Medium | Medium | Labels are unreadable in screenshots | Alternate label positions above/below track lines based on milestone index. For tightly packed milestones, this is a data authoring concern — document the limitation. |

### Project Risks

| # | Risk | Severity | Likelihood | Impact | Mitigation |
|---|------|----------|------------|--------|------------|
| 7 | **Scope creep** — Stakeholders request filtering, drill-down, database, multi-project | High | Medium | Simple tool balloons into a full application; timeline extends from 2 days to 2 weeks | Hard boundary: one page, one JSON file, no database, no auth. This document is the scope authority. |
| 8 | **Over-engineering** — Developer introduces Clean Architecture, repositories, CQRS for a 500-line app | Medium | Medium | 3× development time for zero user-facing benefit | Architecture explicitly mandates flat structure. If the `Services/` folder has more than 2 files, something has gone wrong. |
| 9 | **Pixel-matching perfectionism** — Infinite iteration on 1px differences between reference and implementation | Medium | High | Schedule overrun on visual polish | Time-box CSS tuning to 4 hours. Use browser DevTools overlay comparison. Accept "visually indistinguishable at normal viewing distance" as done. |
| 10 | **`data.json` schema changes** — PM wants new fields after initial build | Low | Medium | Deserialization breaks; components need updates | Use `JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }` for forward compatibility. New optional fields won't break existing JSON files. |

### Accepted Trade-offs

1. **Blazor Server for static content:** The SignalR circuit adds ~50MB memory overhead for a page that never interacts. Accepted because: mandated stack, enables data binding, and overhead is negligible on a local machine.

2. **No automated tests:** Visual inspection is the QA method. Accepted because: the product is literally a screenshot, the codebase is <1000 lines, and test maintenance cost exceeds bug-finding value.

3. **Windows-only:** Segoe UI dependency and local-only hosting make this non-portable. Accepted because: the tool is built by and for Windows users creating PowerPoint slides.

4. **No hot-reload of data without FileSystemWatcher:** Without the watcher, users must refresh the browser after editing `data.json`. Accepted as baseline; FileSystemWatcher is a low-cost enhancement.