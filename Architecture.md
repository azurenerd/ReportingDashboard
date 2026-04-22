# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server (.NET 8) application that renders a pixel-perfect 1920×1080 project status view from a local JSON file. It is designed for screenshot capture and direct insertion into PowerPoint executive decks.

**Architectural Principles:**

1. **Radical Simplicity** — Under 15 source files, zero third-party runtime dependencies, no database, no authentication, no API layer.
2. **Data-Driven Rendering** — Every visual element (titles, milestones, heatmap items, colors, month counts) is driven by `dashboard-data.json`. No hardcoded content.
3. **Screenshot Fidelity** — Fixed 1920×1080 viewport with no scrolling, no responsive breakpoints, and pixel-exact CSS matching `OriginalDesignConcept.html`.
4. **Live Reload** — `FileSystemWatcher` with polling fallback enables edit-save-see workflow without app restarts.
5. **Graceful Degradation** — Missing or malformed JSON produces clean, in-page error messages that auto-recover when the file is fixed.

**Architecture Pattern:** Monolithic single-project Blazor Server application with a flat component hierarchy. No layers, no abstractions beyond a single data service. The simplicity IS the architecture.

---

## System Components

### 1. `Program.cs` — Application Entry Point

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Configure DI, register services, configure Kestrel for localhost, build and run the web host |
| **Interfaces** | None (entry point) |
| **Dependencies** | `Microsoft.AspNetCore.App` framework, `DashboardDataService` |
| **Data** | Reads `appsettings.json` for data file path configuration |

**Key Implementation Details:**

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();
builder.Services.AddSingleton<IDashboardDataService, DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
```

Kestrel binds to `https://localhost:5001` only. No external network exposure.

---

### 2. `DashboardDataService` — Data Loading & File Watching

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Read, deserialize, cache, and auto-reload `dashboard-data.json`; notify subscribers of data changes |
| **Interface** | `IDashboardDataService` |
| **Dependencies** | `System.Text.Json`, `System.IO.FileSystemWatcher`, `IConfiguration` |
| **Data** | In-memory `DashboardData` object (cached); raw JSON file on disk |
| **Lifetime** | Singleton (one instance for the application lifetime) |

**Interface Contract:**

```csharp
public interface IDashboardDataService : IDisposable
{
    DashboardData? GetData();
    string? GetError();
    event Action? OnDataChanged;
}
```

**Behavior:**

- On construction: reads file path from `IConfiguration["DashboardDataFile"]` (default: `wwwroot/data/dashboard-data.json`)
- On first call or file change: reads file, deserializes via `System.Text.Json`, caches result
- `FileSystemWatcher` monitors the file for `Changed`, `Created`, and `Renamed` events
- Polling fallback: a `Timer` checks `File.GetLastWriteTimeUtc()` every 5 seconds
- File lock handling: on `IOException`, retry up to 3 times with 200ms delay
- On deserialization failure: caches error message string, returns `null` data
- On file not found: caches error message with expected path, returns `null` data
- Fires `OnDataChanged` event after any data or error state change
- Debounces file change events (300ms) to avoid multiple reloads during rapid saves

---

### 3. `DashboardData.cs` — Data Model (POCOs)

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Strongly-typed C# records for JSON deserialization |
| **Interface** | Public record types consumed by all Razor components |
| **Dependencies** | `System.Text.Json.Serialization` for attribute decorators |
| **Data** | Immutable record instances deserialized from JSON |

*Full model specification in the [Data Model](#data-model) section below.*

---

### 4. `MainLayout.razor` — Layout Wrapper

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Minimal Blazor layout that renders `@Body` with no chrome, no nav, no sidebar |
| **Interface** | Blazor `LayoutComponentBase` |
| **Dependencies** | None |
| **Data** | None |

Strips all default Blazor template content. Renders only `@Body` inside a minimal HTML structure. The associated `MainLayout.razor.css` is empty or contains only a reset to remove any default Blazor layout styling.

---

### 5. `App.razor` — Root Component

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Blazor root component, references `<HeadOutlet>`, sets render mode, links CSS |
| **Interface** | Blazor root |
| **Dependencies** | `MainLayout.razor` |
| **Data** | None |

Configures `<HeadOutlet>` for `<head>` content and `<Routes>` for component routing. References `app.css` and the Blazor scoped CSS bundle.

---

### 6. `Dashboard.razor` — Main Page (the entire application)

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Single page at route `/`. Injects data service, subscribes to changes, orchestrates child components, handles error display |
| **Interface** | `@page "/"` route; `@inject IDashboardDataService` |
| **Dependencies** | `IDashboardDataService`, `Header.razor`, `Timeline.razor`, `Heatmap.razor` |
| **Data** | `DashboardData` passed down as `[Parameter]` to children |

**Behavior:**

- On `OnInitialized`: subscribes to `IDashboardDataService.OnDataChanged`
- On data change: calls `InvokeAsync(StateHasChanged)` to trigger re-render via SignalR
- If `GetData()` returns null: renders error panel using `GetError()` message
- If data is valid: renders `<Header>`, `<Timeline>`, `<Heatmap>` in sequence
- Implements `IDisposable` to unsubscribe from data change events

**Error Display:**

```html
<div class="error-panel">
    <div class="error-icon">⚠</div>
    <div class="error-message">@errorMessage</div>
    <div class="error-hint">Fix the file and save — the dashboard will reload automatically.</div>
</div>
```

Styled with centered layout, Segoe UI font, muted colors — clean enough for accidental screenshot.

---

### 7. `Header.razor` — Header Bar Component

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render project title, subtitle, backlog link, and milestone legend |
| **Interface** | `[Parameter] public ProjectInfo Project { get; set; }` |
| **Dependencies** | None |
| **Data** | `ProjectInfo` record |

Renders the `.hdr` section from the design reference. The backlog link opens in `target="_blank"`. Legend icons are pure CSS (rotated squares for diamonds, border-radius for circles, rectangles for the NOW bar).

---

### 8. `Timeline.razor` — SVG Timeline Component

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render the horizontal milestone Gantt chart as inline SVG |
| **Interface** | `[Parameter] public TimelineData Timeline { get; set; }` |
| **Dependencies** | None |
| **Data** | `TimelineData` record containing tracks, milestones, date range, currentDate |

**Key Rendering Logic:**

- **SVG dimensions:** width=1560, height=185 (hardcoded for 1920px layout)
- **X-position calculation:** `x = ((date - startDate).TotalDays / (endDate - startDate).TotalDays) * 1560`
- **Y-position calculation:** tracks are evenly spaced: `y = 28 + (trackIndex * (185 - 28)) / max(trackCount - 1, 1)` for 1 track centered, 2+ tracks distributed
- **Month gridlines:** iterate from `startDate` to `endDate` by month, draw vertical `<line>` + `<text>`
- **NOW line:** dashed red vertical line at `GetXPosition(currentDate)` with "NOW" label
- **Track lines:** horizontal `<line>` per track, full SVG width, stroke = track color, stroke-width = 3
- **Milestone markers:**
  - `checkpoint` → `<circle>` r=5-7, fill=white, stroke=track color or #888, stroke-width=2.5
  - `poc` → `<polygon>` diamond ±11px, fill=#F4B400, filter=drop shadow
  - `production` → `<polygon>` diamond ±11px, fill=#34A853, filter=drop shadow
- **Label placement:** alternating above/below the track line per milestone (odd index above, even index below) to minimize overlap. Labels use `text-anchor: middle`, font-size 10px, fill #666.
- **Drop shadow filter:** defined once in `<defs>`: `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3">`

---

### 9. `Heatmap.razor` — Execution Heatmap Component

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render the CSS Grid heatmap with section title, column/row headers, and data cells |
| **Interface** | `[Parameter] public HeatmapData Heatmap { get; set; }` |
| **Dependencies** | `HeatmapCell.razor` |
| **Data** | `HeatmapData` record with months, highlightMonth, rows |

**Key Rendering Logic:**

- Dynamic `grid-template-columns` via inline style: `160px repeat(@Heatmap.Months.Count, 1fr)`
- Fixed `grid-template-rows`: `36px repeat(4, 1fr)`
- Corner cell: "STATUS" label
- Month headers: apply `.apr-hdr` class equivalent when month matches `highlightMonth`
- Row headers: category-specific CSS classes with emoji prefixes (✅ Shipped, 🔄 In Progress, 🔁 Carryover, 🚫 Blockers)
- Data cells: delegate to `<HeatmapCell>` for each (row, month) intersection
- Category-to-CSS mapping is handled via a helper method that returns the appropriate CSS class prefix (`ship`, `prog`, `carry`, `block`) based on category name

---

### 10. `HeatmapCell.razor` — Individual Heatmap Cell

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render a single cell's bulleted item list or empty dash |
| **Interface** | `[Parameter] public List<string> Items { get; set; }`, `[Parameter] public string CssClass { get; set; }`, `[Parameter] public bool IsHighlighted { get; set; }` |
| **Dependencies** | None |
| **Data** | List of string items for one category × one month |

**Behavior:**

- If `Items` is null or empty: render `<div class="it empty">-</div>` in #AAA
- Otherwise: render each item as `<div class="it">@item</div>` with colored bullet via CSS `::before`
- The `CssClass` parameter determines the row color family (shipped, progress, carry, block)
- The `IsHighlighted` parameter applies the highlight background variant

---

### 11. `app.css` — Global Stylesheet

| Aspect | Detail |
|--------|--------|
| **Responsibility** | CSS reset, body sizing (1920×1080), font declaration, CSS custom properties for the entire color system, print styles |
| **Dependencies** | None |
| **Data** | N/A |

**Contents:**

- Universal reset (`* { margin: 0; padding: 0; box-sizing: border-box; }`)
- Body: `width: 1920px; height: 1080px; overflow: hidden; background: #FFFFFF; font-family: 'Segoe UI', Arial, sans-serif; color: #111; display: flex; flex-direction: column;`
- All CSS custom properties as specified in the PM spec `:root` block
- Link styles: `a { color: var(--link-color); text-decoration: none; }`
- `@media print` rules: hide `.blazor-error-boundary`, `#blazor-error-ui`, connection indicators; set `body { -webkit-print-color-adjust: exact; print-color-adjust: exact; }`

---

### 12. `dashboard-data.json` — Data Configuration File

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Single source of all dashboard display data |
| **Interface** | JSON file conforming to `DashboardData` schema |
| **Dependencies** | None |
| **Location** | `wwwroot/data/dashboard-data.json` (configurable via `appsettings.json`) |

Full schema specified in [Data Model](#data-model) section.

---

### 13. `appsettings.json` — Application Configuration

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Configure data file path and logging |
| **Content** | `DashboardDataFile` path setting, standard ASP.NET Core logging config |

```json
{
  "DashboardDataFile": "wwwroot/data/dashboard-data.json",
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
┌─────────────────────────────────────────────────────────────┐
│                    Local File System                         │
│   dashboard-data.json                                       │
└──────────┬──────────────────────────────────┬───────────────┘
           │ Read on startup                  │ FileSystemWatcher
           │ Read on change                   │ + Polling (5s)
           ▼                                  │
┌──────────────────────────────────────┐      │
│   DashboardDataService (Singleton)   │◄─────┘
│                                      │
│  ┌─ DashboardData? (cached)          │
│  ├─ string? Error (cached)           │
│  └─ event OnDataChanged              │
└──────────┬───────────────────────────┘
           │ @inject IDashboardDataService
           │ Subscribe to OnDataChanged
           ▼
┌──────────────────────────────────────┐
│   Dashboard.razor (Page: "/")        │
│                                      │
│  if error → render ErrorPanel        │
│  if data  → render child components  │
│                                      │
│  ┌─ [Parameter] ProjectInfo ────────►│ Header.razor
│  │                                   │   ├─ Title, Subtitle, Link
│  │                                   │   └─ Legend Icons (CSS)
│  │                                   │
│  ├─ [Parameter] TimelineData ───────►│ Timeline.razor
│  │                                   │   ├─ Track Labels (left panel)
│  │                                   │   └─ SVG Canvas (right panel)
│  │                                   │       ├─ Gridlines + Month Labels
│  │                                   │       ├─ NOW Line
│  │                                   │       ├─ Track Lines
│  │                                   │       └─ Milestone Markers + Labels
│  │                                   │
│  └─ [Parameter] HeatmapData ───────►│ Heatmap.razor
│                                      │   ├─ Section Title
│                                      │   ├─ CSS Grid Header Row
│                                      │   └─ HeatmapCell.razor × (4 rows × N months)
└──────────────────────────────────────┘
```

### Communication Patterns

| From | To | Mechanism | Payload |
|------|----|-----------|---------|
| File System | `DashboardDataService` | `FileSystemWatcher` events + polling timer | File change notification |
| `DashboardDataService` | `Dashboard.razor` | C# `event Action OnDataChanged` | (no payload; component re-reads from service) |
| `Dashboard.razor` | `Header.razor` | Blazor `[Parameter]` | `ProjectInfo` record |
| `Dashboard.razor` | `Timeline.razor` | Blazor `[Parameter]` | `TimelineData` record |
| `Dashboard.razor` | `Heatmap.razor` | Blazor `[Parameter]` | `HeatmapData` record |
| `Heatmap.razor` | `HeatmapCell.razor` | Blazor `[Parameter]` | `List<string>`, CSS class, highlight flag |
| `Dashboard.razor` | Browser | Blazor Server SignalR | Rendered HTML diff |

### Auto-Reload Sequence

```
User saves dashboard-data.json in VS Code
        │
        ▼
FileSystemWatcher fires Changed event
        │
        ▼
DashboardDataService debounces (300ms)
        │
        ▼
DashboardDataService reads file (with retry on IOException)
        │
        ▼
System.Text.Json deserializes to DashboardData
        │
        ├─ Success: cache data, clear error
        └─ Failure: cache error message, null data
        │
        ▼
DashboardDataService fires OnDataChanged event
        │
        ▼
Dashboard.razor calls InvokeAsync(StateHasChanged)
        │
        ▼
Blazor Server sends DOM diff via SignalR to browser
        │
        ▼
Browser updates in-place (no full page reload)
```

---

## Data Model

### JSON Schema → C# POCO Mapping

```csharp
// File: Models/DashboardData.cs

using System.Text.Json.Serialization;

/// <summary>
/// Root object for dashboard-data.json.
/// </summary>
public record DashboardData
{
    [JsonPropertyName("project")]
    public ProjectInfo Project { get; init; } = new();

    [JsonPropertyName("timeline")]
    public TimelineData Timeline { get; init; } = new();

    [JsonPropertyName("heatmap")]
    public HeatmapData Heatmap { get; init; } = new();
}

/// <summary>
/// Project metadata displayed in the header.
/// </summary>
public record ProjectInfo
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; init; } = string.Empty;

    [JsonPropertyName("backlogUrl")]
    public string? BacklogUrl { get; init; }

    [JsonPropertyName("currentDate")]
    public string CurrentDate { get; init; } = string.Empty;
}

/// <summary>
/// Timeline section: date range and project tracks with milestones.
/// </summary>
public record TimelineData
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; init; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; init; } = string.Empty;

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; init; } = new();
}

/// <summary>
/// A single horizontal track in the timeline (e.g., M1, M2).
/// </summary>
public record TimelineTrack
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; init; } = "#999";

    [JsonPropertyName("milestones")]
    public List<MilestoneItem> Milestones { get; init; } = new();
}

/// <summary>
/// A single milestone on a timeline track.
/// Type must be one of: "checkpoint", "poc", "production".
/// </summary>
public record MilestoneItem
{
    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = "checkpoint";
}

/// <summary>
/// Heatmap section: months, highlight, and status rows.
/// </summary>
public record HeatmapData
{
    [JsonPropertyName("months")]
    public List<string> Months { get; init; } = new();

    [JsonPropertyName("highlightMonth")]
    public string HighlightMonth { get; init; } = string.Empty;

    [JsonPropertyName("rows")]
    public List<StatusRow> Rows { get; init; } = new();
}

/// <summary>
/// A single status category row (Shipped, In Progress, Carryover, Blockers).
/// </summary>
public record StatusRow
{
    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("items")]
    public Dictionary<string, List<string>> Items { get; init; } = new();
}
```

### Entity Relationships

```
DashboardData (root)
├── 1:1  ProjectInfo
│         ├─ Title (string)
│         ├─ Subtitle (string)
│         ├─ BacklogUrl (string?)
│         └─ CurrentDate (string, "YYYY-MM-DD")
├── 1:1  TimelineData
│         ├─ StartDate (string, "YYYY-MM-DD")
│         ├─ EndDate (string, "YYYY-MM-DD")
│         └─ 1:N  TimelineTrack
│                  ├─ Id (string, e.g., "M1")
│                  ├─ Name (string)
│                  ├─ Color (string, hex color)
│                  └─ 1:N  MilestoneItem
│                           ├─ Date (string, "YYYY-MM-DD")
│                           ├─ Label (string)
│                           └─ Type (string: "checkpoint"|"poc"|"production")
└── 1:1  HeatmapData
          ├─ Months (string[], 3-6 items)
          ├─ HighlightMonth (string, matches one entry in Months)
          └─ 1:N  StatusRow (exactly 4: Shipped, In Progress, Carryover, Blockers)
                   ├─ Category (string)
                   └─ Items (Dictionary<month, List<string>>)
```

### Storage

| Aspect | Detail |
|--------|--------|
| **Format** | JSON (UTF-8, `.json` file extension) |
| **Location** | `wwwroot/data/dashboard-data.json` (default, configurable) |
| **Access pattern** | Read-only from application; write by human in text editor |
| **Caching** | In-memory singleton; refreshed on file change |
| **Backup** | Version-controlled in Git alongside source code |
| **Size** | < 10 KB typical; < 100 KB maximum for 6 months × 4 categories × 20 items each |

### Date Handling

All dates in JSON are stored as strings in `YYYY-MM-DD` format. The `DashboardDataService` or `Timeline.razor` parses them to `DateOnly` for position calculations:

```csharp
private DateOnly ParseDate(string dateStr) => DateOnly.Parse(dateStr);
```

`DateOnly` is used (not `DateTime`) because the dashboard operates at day granularity. No time zones are relevant.

---

## API Contracts

This application has **no REST API, no GraphQL, and no HTTP endpoints** beyond the Blazor Server page rendering and SignalR connection.

### Internal Service Contract

The only programmatic contract is the `IDashboardDataService` interface:

```csharp
public interface IDashboardDataService : IDisposable
{
    /// <summary>
    /// Returns the current dashboard data, or null if unavailable.
    /// </summary>
    DashboardData? GetData();

    /// <summary>
    /// Returns the current error message, or null if data loaded successfully.
    /// </summary>
    string? GetError();

    /// <summary>
    /// Fired when data or error state changes (file modified, created, or became invalid).
    /// </summary>
    event Action? OnDataChanged;
}
```

**Error States:**

| Condition | `GetData()` | `GetError()` |
|-----------|-------------|--------------|
| File loaded successfully | `DashboardData` instance | `null` |
| File not found | `null` | `"Dashboard data file not found. Expected location: {absolutePath}"` |
| JSON parse error | `null` | `"Error reading dashboard data: {exceptionMessage}"` |
| File locked (transient) | Previous cached data | Previous cached error (retry in progress) |

### JSON Data File Contract

The `dashboard-data.json` file must conform to the following structure. Missing optional fields use defaults; missing required fields produce a deserialization error.

| Field Path | Type | Required | Default | Constraints |
|------------|------|----------|---------|-------------|
| `project.title` | string | Yes | — | Non-empty |
| `project.subtitle` | string | Yes | — | Non-empty |
| `project.backlogUrl` | string | No | `null` | Valid URL or omitted |
| `project.currentDate` | string | Yes | — | `YYYY-MM-DD` format |
| `timeline.startDate` | string | Yes | — | `YYYY-MM-DD`, before `endDate` |
| `timeline.endDate` | string | Yes | — | `YYYY-MM-DD`, after `startDate` |
| `timeline.tracks` | array | Yes | — | 1–5 items |
| `timeline.tracks[].id` | string | Yes | — | Short identifier (e.g., "M1") |
| `timeline.tracks[].name` | string | Yes | — | Display name |
| `timeline.tracks[].color` | string | Yes | — | Hex color (e.g., "#0078D4") |
| `timeline.tracks[].milestones` | array | Yes | — | 0+ items |
| `timeline.tracks[].milestones[].date` | string | Yes | — | `YYYY-MM-DD` |
| `timeline.tracks[].milestones[].label` | string | Yes | — | Display label |
| `timeline.tracks[].milestones[].type` | string | Yes | — | `"checkpoint"`, `"poc"`, or `"production"` |
| `heatmap.months` | array | Yes | — | 3–6 month abbreviation strings |
| `heatmap.highlightMonth` | string | Yes | — | Must match one entry in `months` |
| `heatmap.rows` | array | Yes | — | Exactly 4 items |
| `heatmap.rows[].category` | string | Yes | — | `"Shipped"`, `"In Progress"`, `"Carryover"`, or `"Blockers"` |
| `heatmap.rows[].items` | object | Yes | — | Keys = month names from `months`; values = string arrays |

---

## Infrastructure Requirements

### Hosting

| Aspect | Requirement |
|--------|-------------|
| **Host machine** | Windows 10/11 developer workstation |
| **Runtime** | .NET 8.0.x SDK (for `dotnet run`) or published self-contained executable |
| **Web server** | Kestrel (built into ASP.NET Core), localhost only |
| **Port** | `https://localhost:5001` (configurable via `launchSettings.json`) |
| **Process model** | Single Kestrel process, single SignalR circuit |
| **TLS** | .NET dev certificate (`dotnet dev-certs https --trust`) |

### Networking

| Aspect | Requirement |
|--------|-------------|
| **Binding** | `localhost` / `127.0.0.1` only — no LAN or internet exposure |
| **Protocols** | HTTPS (TLS 1.2+) for page load; WSS for Blazor Server SignalR |
| **Firewall** | No inbound rules needed; localhost traffic only |
| **DNS** | None — direct `localhost` access |

### Storage

| Aspect | Requirement |
|--------|-------------|
| **Disk space** | < 5 MB for published application + data file |
| **Data file** | Single `dashboard-data.json` file (< 100 KB) |
| **Static assets** | `app.css` + Blazor framework files in `wwwroot/` |
| **Temp files** | None created by application |

### CI/CD

| Aspect | Requirement |
|--------|-------------|
| **Build** | Manual: `dotnet build` (zero errors, zero warnings) |
| **Run** | Manual: `dotnet run` or `dotnet watch` |
| **Publish** | Optional: `dotnet publish -c Release -o ./publish` |
| **Automated pipeline** | Not required (out of scope) |
| **Version control** | Git; solution and data file checked in together |

### Development Environment

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 8.0.x | Build and run |
| VS Code or Visual Studio 2022 | Latest | IDE |
| Edge or Chrome | Latest stable | View and screenshot |
| Windows 10/11 | — | Host OS (Segoe UI font) |

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **UI Framework** | Blazor Server (.NET 8) | Team has C#/.NET expertise. Blazor Server provides component-based rendering with C# data binding, hot reload, and zero JS requirement. The parent repo (AgentSquad) is .NET-based. |
| **Render Mode** | Interactive Server (SignalR) | Simplest to implement. Required for `FileSystemWatcher` → `StateHasChanged` → live DOM updates without page refresh. Static SSR would require a manual refresh mechanism. The SignalR overhead is negligible for a single local user. |
| **CSS Layout** | CSS Grid + Flexbox (native) | The original HTML design already uses these. Direct translation to Blazor scoped CSS with zero abstraction overhead. No CSS framework needed. |
| **SVG Rendering** | Inline SVG via Razor markup | Timeline milestones are simple geometric shapes (lines, circles, diamonds, text). Hand-coded SVG in Razor gives pixel-perfect control. A charting library would add dependencies and fight the custom design. |
| **CSS Architecture** | Scoped `.razor.css` + global `app.css` | Blazor's built-in CSS isolation prevents style conflicts between components. Global `app.css` handles resets, body sizing, and CSS custom properties. |
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8. Zero additional dependency. Fast, supports records, supports `JsonPropertyName` attributes. |
| **File Watching** | `FileSystemWatcher` + polling fallback | Built into .NET. `FileSystemWatcher` provides near-instant detection. Polling at 5-second intervals via `Timer` handles the known edge case of missed events on Windows. |
| **Data Storage** | Flat JSON file | The dashboard is read-only. A single human-editable JSON file is the simplest possible data store. No database overhead, no migration tooling, no connection strings. |
| **Authentication** | None | Local-only tool on a developer machine. No network exposure. Adding auth would violate the simplicity constraint for zero benefit. |
| **Testing** | xUnit + bUnit + FluentAssertions | Standard .NET testing stack. bUnit enables in-memory Blazor component rendering. These are test-project-only dependencies (zero runtime impact). |
| **Third-party packages** | Zero runtime dependencies | Every additional package is a maintenance burden. Everything needed (JSON, file I/O, Blazor, CSS) is built into .NET 8 / the browser. |

---

## Security Considerations

### Authentication & Authorization

**None required.** The application binds exclusively to `localhost` and is intended for single-user, single-machine operation. There are no user accounts, no roles, no sessions, and no tokens.

### Network Security

| Control | Implementation |
|---------|---------------|
| **Localhost binding** | Kestrel configured to listen on `https://localhost:5001` only |
| **TLS** | .NET development certificate provides HTTPS by default |
| **No external endpoints** | No API controllers, no webhook receivers, no public routes |
| **SignalR circuit** | Single circuit for the sole browser tab; no cross-origin concerns |

### Data Protection

| Aspect | Assessment |
|--------|------------|
| **Data classification** | Non-sensitive. Project names, milestone labels, and status descriptions only |
| **PII** | None. No user data, no email addresses, no personal identifiers |
| **Credentials** | None stored. No API keys, no connection strings, no secrets |
| **Encryption at rest** | Not required. JSON file contains only project status text |
| **Encryption in transit** | HTTPS via .NET dev cert (localhost only) |

### Input Validation

Although the application reads from a trusted local file (not user-submitted web input), the following defenses are applied:

| Vector | Mitigation |
|--------|------------|
| **Malformed JSON** | `System.Text.Json` throws `JsonException`; caught and displayed as friendly error |
| **Oversized file** | Practical limit: if file > 1 MB, log a warning (defense against accidental wrong file) |
| **Path traversal** | Data file path is read from `appsettings.json` (developer-controlled config); not from query string or user input |
| **XSS via JSON content** | Blazor's Razor rendering automatically HTML-encodes all `@` expressions. Raw HTML injection is not possible unless `@((MarkupString)...)` is explicitly used (which this application does not do) |
| **File lock race condition** | Retry reads up to 3 times with 200ms delay on `IOException` |

### Future Security Considerations

If the application ever needs to be exposed beyond localhost (e.g., shared on a team LAN):

1. Add Windows Authentication: `builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();`
2. Configure Kestrel to bind to a specific IP instead of `localhost`
3. Consider read-only authorization policy to prevent any future write endpoints

---

## Scaling Strategy

### Current Scale (V1)

This application is designed for **exactly one concurrent user on one machine**. There is no scaling requirement.

| Dimension | V1 Target | Capacity |
|-----------|-----------|----------|
| **Concurrent users** | 1 | Single Blazor Server SignalR circuit |
| **Data size** | 1 JSON file, < 100 KB | In-memory cache, < 1 MB RAM |
| **Tracks** | 1–5 | SVG layout adapts proportionally |
| **Months** | 3–6 | CSS Grid adapts via `repeat(N, 1fr)` |
| **Items per cell** | 0–10 | Overflow hidden; vertical space constrains practical limit to ~8 |
| **Milestones per track** | 0–10 | SVG handles unlimited; label overlap becomes an issue beyond ~8 |

### Vertical Scaling (if needed)

The application runs within a single Kestrel process. The .NET 8 runtime can handle significantly more load than this application requires:

- **Memory:** Current target < 100 MB RSS. Blazor Server allocates ~250 KB per circuit. Even 10 simultaneous viewers would use < 5 MB additional.
- **CPU:** SVG generation and JSON deserialization are trivially fast (< 10ms). No optimization needed.

### Horizontal Scaling (not applicable)

This is a local-only tool. There is no load balancer, no multiple instances, no distributed state. If multi-machine access is ever needed, the recommendation is to commit `dashboard-data.json` to a Git repo and have each user run the application locally — not to deploy a shared server.

### Data Scaling

| Scenario | Impact | Mitigation |
|----------|--------|------------|
| 6 months × 4 rows × 20 items | JSON grows to ~50 KB; renders fine | No action needed |
| 5 tracks × 10 milestones each | SVG has 50 elements; renders in < 50ms | No action needed |
| 12+ months | Grid columns become very narrow | PM spec limits to 3–6 months; enforce via documentation |
| 6+ tracks | Track lines become cramped in 185px SVG height | PM spec limits to 1–5 tracks; enforce via documentation |

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|------------|
| 1 | **FileSystemWatcher misses change events on Windows** | Medium | Low | Polling fallback checks `File.GetLastWriteTimeUtc()` every 5 seconds. Both mechanisms run concurrently; either one triggers a reload. |
| 2 | **JSON schema drift** — data file structure diverges from C# models after manual edits | Medium | Medium | Provide a sample `dashboard-data.json` with comments. Use `System.Text.Json` with explicit `[JsonPropertyName]` attributes. Log specific deserialization errors with field names. Phase 2: add JSON Schema (`$schema`) for VS Code autocomplete. |
| 3 | **File locked during save** — editor holds a write lock when the watcher fires | Medium | Low | Retry file reads up to 3 times with 200ms delay on `IOException`. Most editors (VS Code, Notepad++) use atomic write-rename which minimizes lock duration. |
| 4 | **SVG milestone label overlap** when milestones are close together on the same track | Medium | Medium | Alternate label placement above/below track line. For V1, this simple heuristic handles most cases. If overlap persists, the PM can adjust milestone dates or abbreviate labels in JSON. |
| 5 | **Blazor Server SignalR connection drops** (browser idle, network change) | Low | Low | Blazor's built-in reconnection UI handles this automatically. For a local-only app, the browser and server are on the same machine — connection drops are extremely unlikely. |
| 6 | **Over-engineering / scope creep** | High | Medium | The architecture enforces < 15 files. Code reviews should reject additions of databases, APIs, auth, charting libraries, or CSS frameworks. The PM spec explicitly lists these as out of scope. |
| 7 | **Visual discrepancy with design reference** | Medium | High | Engineers MUST open `OriginalDesignConcept.html` in a browser and do pixel-level comparison. CSS values (padding, colors, font sizes) are specified to exact pixels in this document. Use browser DevTools to measure rendered output. |
| 8 | **Heatmap vertical overflow** with many items per cell | Low | Medium | Cells have `overflow: hidden`. Practical limit is ~8 items per cell at 12px font with 1.35 line-height in the available vertical space. Document this limit for PMs. |
| 9 | **Date parsing failures** from invalid date strings in JSON | Low | Medium | Wrap `DateOnly.Parse()` in try-catch. Display the specific parse error in the error panel with the offending field name and value. |
| 10 | **Browser zoom not at 100%** causes screenshot size mismatch | Medium | Medium | The fixed 1920×1080 body ensures correct layout at any zoom. Document the 100% zoom requirement for screenshot capture. The CSS prevents scrollbars regardless of zoom. |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component with its CSS strategy, data bindings, and interactions.

### Component → Design Section Mapping

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ MainLayout.razor                                                             │
│ CSS: None (bare wrapper, renders @Body only)                                 │
│                                                                              │
│ ┌──────────────────────────────────────────────────────────────────────────┐ │
│ │ Dashboard.razor — Route: "/"                                             │ │
│ │ CSS: None (structural only, layout handled by app.css body flex)         │ │
│ │ Data: @inject IDashboardDataService → DashboardData                      │ │
│ │ Interaction: Subscribes to OnDataChanged, calls StateHasChanged          │ │
│ │                                                                          │ │
│ │ ┌─── .hdr ──────────────────────────────────────────────────────────┐   │ │
│ │ │ Header.razor                                                      │   │ │
│ │ │ Design: `.hdr` element — full-width flexbox header bar            │   │ │
│ │ │ CSS Layout: `display:flex; justify-content:space-between;`        │   │ │
│ │ │   padding: 12px 44px 10px; border-bottom: 1px solid #E0E0E0      │   │ │
│ │ │ Data Binding: [Parameter] ProjectInfo Project                     │   │ │
│ │ │   Left: <h1>@Project.Title <a>↗ ADO Backlog</a></h1>            │   │ │
│ │ │         <div class="sub">@Project.Subtitle</div>                  │   │ │
│ │ │   Right: Legend items (pure CSS shapes, no data binding)          │   │ │
│ │ │     - Diamond (#F4B400): 12px square, transform:rotate(45deg)     │   │ │
│ │ │     - Diamond (#34A853): 12px square, transform:rotate(45deg)     │   │ │
│ │ │     - Circle (#999): 8px, border-radius:50%                      │   │ │
│ │ │     - Bar (#EA4335): 2×14px rectangle                            │   │ │
│ │ │ Interaction: Backlog link → target="_blank"                       │   │ │
│ │ └──────────────────────────────────────────────────────────────────┘   │ │
│ │                                                                          │ │
│ │ ┌─── .tl-area ─────────────────────────────────────────────────────┐   │ │
│ │ │ Timeline.razor                                                    │   │ │
│ │ │ Design: `.tl-area` — 196px fixed height, #FAFAFA background      │   │ │
│ │ │ CSS Layout: `display:flex; align-items:stretch;`                  │   │ │
│ │ │   padding: 6px 44px 0; border-bottom: 2px solid #E8E8E8          │   │ │
│ │ │ Data Binding: [Parameter] TimelineData Timeline                   │   │ │
│ │ │                                                                    │   │ │
│ │ │ Left Panel (230px, flex-shrink:0):                                │   │ │
│ │ │   flex-direction:column; justify-content:space-around             │   │ │
│ │ │   @foreach track in Timeline.Tracks:                              │   │ │
│ │ │     <div style="color:@track.Color">@track.Id</div>              │   │ │
│ │ │     <span style="color:#444">@track.Name</span>                  │   │ │
│ │ │                                                                    │   │ │
│ │ │ Right Panel (flex:1, SVG):                                        │   │ │
│ │ │   <svg width="1560" height="185">                                │   │ │
│ │ │     <defs><filter id="sh"><feDropShadow .../></filter></defs>    │   │ │
│ │ │     @foreach month gridline → <line> + <text>                     │   │ │
│ │ │     NOW line → <line stroke="#EA4335" stroke-dasharray="5,3"/>    │   │ │
│ │ │     @foreach track → <line stroke="@track.Color" stroke-width=3> │   │ │
│ │ │     @foreach milestone:                                           │   │ │
│ │ │       checkpoint → <circle r=5-7 fill=white stroke=color>        │   │ │
│ │ │       poc → <polygon fill="#F4B400" filter="url(#sh)">           │   │ │
│ │ │       production → <polygon fill="#34A853" filter="url(#sh)">    │   │ │
│ │ │       + <text> label above or below                               │   │ │
│ │ │   </svg>                                                          │   │ │
│ │ │ Interaction: None (read-only SVG)                                 │   │ │
│ │ └──────────────────────────────────────────────────────────────────┘   │ │
│ │                                                                          │ │
│ │ ┌─── .hm-wrap ─────────────────────────────────────────────────────┐   │ │
│ │ │ Heatmap.razor                                                     │   │ │
│ │ │ Design: `.hm-wrap` — flex:1, fills remaining vertical space       │   │ │
│ │ │ CSS Layout: `display:flex; flex-direction:column; min-height:0`   │   │ │
│ │ │   padding: 10px 44px 10px                                         │   │ │
│ │ │ Data Binding: [Parameter] HeatmapData Heatmap                     │   │ │
│ │ │                                                                    │   │ │
│ │ │ Title: <div class="hm-title">MONTHLY EXECUTION HEATMAP...</div>  │   │ │
│ │ │                                                                    │   │ │
│ │ │ Grid: <div class="hm-grid" style="grid-template-columns:         │   │ │
│ │ │         160px repeat(@Heatmap.Months.Count, 1fr)">                │   │ │
│ │ │                                                                    │   │ │
│ │ │   Row 0 (header, 36px):                                          │   │ │
│ │ │     [0,0] Corner: "STATUS" (.hm-corner)                          │   │ │
│ │ │     [0,1..N] Month headers (.hm-col-hdr)                         │   │ │
│ │ │       if month == highlightMonth → add .apr-hdr class             │   │ │
│ │ │                                                                    │   │ │
│ │ │   Rows 1-4 (data, 1fr each):                                     │   │ │
│ │ │     [r,0] Row header (.hm-row-hdr + category class)              │   │ │
│ │ │       Category → CSS class mapping:                               │   │ │
│ │ │         "Shipped"     → ship-hdr, ship-cell                      │   │ │
│ │ │         "In Progress" → prog-hdr, prog-cell                      │   │ │
│ │ │         "Carryover"   → carry-hdr, carry-cell                    │   │ │
│ │ │         "Blockers"    → block-hdr, block-cell                    │   │ │
│ │ │                                                                    │   │ │
│ │ │     [r,1..N] → HeatmapCell.razor                                 │   │ │
│ │ │                                                                    │   │ │
│ │ │ ┌─────────────────────────────────────────────────────────┐      │   │ │
│ │ │ │ HeatmapCell.razor                                       │      │   │ │
│ │ │ │ Design: `.hm-cell` — individual grid cell               │      │   │ │
│ │ │ │ CSS: padding 8px 12px, borders, overflow:hidden         │      │   │ │
│ │ │ │ Data Binding:                                            │      │   │ │
│ │ │ │   [Parameter] List<string> Items                         │      │   │ │
│ │ │ │   [Parameter] string CssClass (e.g., "ship-cell")       │      │   │ │
│ │ │ │   [Parameter] bool IsHighlighted                         │      │   │ │
│ │ │ │ Rendering:                                               │      │   │ │
│ │ │ │   if Items empty → <div class="it empty">-</div>        │      │   │ │
│ │ │ │   else → @foreach item:                                  │      │   │ │
│ │ │ │     <div class="it">@item</div>                          │      │   │ │
│ │ │ │     (6px colored circle via CSS ::before pseudo-element) │      │   │ │
│ │ │ │ Interaction: None (read-only)                            │      │   │ │
│ │ │ └─────────────────────────────────────────────────────────┘      │   │ │
│ │ └──────────────────────────────────────────────────────────────────┘   │ │
│ └──────────────────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────────────────┘
```

### CSS File Mapping

| File | Scope | Key Rules |
|------|-------|-----------|
| `wwwroot/css/app.css` | Global | Body sizing (1920×1080), font, CSS reset, `:root` custom properties, `@media print` rules, link styles |
| `MainLayout.razor.css` | Layout | Minimal; removes default Blazor layout chrome |
| `Header.razor.css` | Header | `.hdr` padding/border, `h1` sizing, `.sub` styles, legend flex layout, diamond/circle/bar icon shapes |
| `Timeline.razor.css` | Timeline | `.tl-area` height/background/border, left panel width/flex, `.tl-svg-box` padding |
| `Heatmap.razor.css` | Heatmap | `.hm-wrap` flex, `.hm-title` typography, `.hm-grid` grid setup, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, row color families (ship/prog/carry/block), highlight classes |
| `HeatmapCell.razor.css` | Cell | `.hm-cell` padding/borders, `.it` bullet styles with `::before` pseudo-element, empty state |

### Complete File Inventory (< 15 files target)

```
src/ReportingDashboard/
├── ReportingDashboard.csproj        #  1 - Project file
├── Program.cs                       #  2 - Entry point & DI
├── appsettings.json                 #  3 - Config
├── Models/
│   └── DashboardData.cs             #  4 - All POCO records
├── Services/
│   └── DashboardDataService.cs      #  5 - Data loading + file watch
├── Components/
│   ├── App.razor                    #  6 - Root component
│   ├── Routes.razor                 #  7 - Router
│   ├── Layout/
│   │   ├── MainLayout.razor         #  8 - Layout wrapper
│   │   └── MainLayout.razor.css     #  9 - Layout styles
│   ├── Pages/
│   │   └── Dashboard.razor          # 10 - Main page
│   ├── Header.razor                 # 11 - Header component
│   ├── Header.razor.css             # (scoped CSS, not counted separately)
│   ├── Timeline.razor               # 12 - Timeline SVG component
│   ├── Timeline.razor.css           # (scoped CSS)
│   ├── Heatmap.razor                # 13 - Heatmap grid component
│   ├── Heatmap.razor.css            # (scoped CSS)
│   └── HeatmapCell.razor            # 14 - Cell component
├── wwwroot/
│   ├── css/
│   │   └── app.css                  # (static asset)
│   └── data/
│       └── dashboard-data.json      # (data file)
                                     ─────
                              Total: 14 source files ✓
```