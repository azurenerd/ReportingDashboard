# Architecture

## Overview & Goals

This document defines the complete system architecture for the **Executive Reporting Dashboard**, a single-page Blazor Server application (.NET 8) that renders a pixel-perfect 1920×1080 project roadmap visualization. The dashboard reads all content from a local `data.json` file and produces a screenshot-ready view for PowerPoint insertion.

**Primary architectural goals:**

1. **Pixel-perfect fidelity** — Match the `OriginalDesignConcept.html` reference design within 95% visual accuracy across all three sections (header, timeline, heatmap).
2. **Zero-dependency local execution** — Run with `dotnet run` on any Windows machine with .NET 8 SDK. No NuGet packages beyond the default Blazor Server template.
3. **Data-driven rendering** — All visual content is controlled by a single `data.json` file. No code changes required to update reports.
4. **Minimal codebase** — Fewer than 15 source files. No over-engineering. The architecture is deliberately simple.
5. **Sub-2-second page load** — Full dashboard rendered and screenshot-ready in under 2 seconds.

**Architecture pattern:** File → Service → Component → Browser

```
data.json → DashboardDataService (singleton) → Dashboard.razor (component tree) → 1920×1080 Browser Render
```

---

## System Components

### 1. `ReportingDashboard.sln` — Solution Root

**Responsibility:** Single solution file containing one project.

**Structure:**
```
ReportingDashboard/
├── ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── data.json
    ├── data.template.json
    ├── Components/
    │   ├── App.razor
    │   ├── Routes.razor
    │   ├── Layout/
    │   │   └── MainLayout.razor
    │   └── Pages/
    │       ├── Dashboard.razor
    │       └── Dashboard.razor.css
    ├── Models/
    │   └── DashboardData.cs
    ├── Services/
    │   └── DashboardDataService.cs
    └── wwwroot/
        └── app.css
```

**File count:** 12 source files (well within the ≤15 constraint).

---

### 2. `Program.cs` — Application Entry Point

**Responsibility:** Configure Kestrel, register services, set up the Blazor Server pipeline.

**Interfaces:** None (entry point only).

**Dependencies:** `DashboardDataService`, ASP.NET Core middleware pipeline.

**Key behaviors:**
- Registers `DashboardDataService` as a **singleton** in the DI container.
- Configures Kestrel to listen on `http://localhost:5000` and `https://localhost:5001`.
- Adds Blazor Server services (`builder.Services.AddRazorComponents().AddInteractiveServerComponents()`).
- Maps Razor components and static files.
- No authentication middleware. No database context. No CORS.

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

---

### 3. `DashboardDataService` — Data Access Layer

**Responsibility:** Read, parse, cache, and serve `data.json` content to Blazor components. Handle file errors gracefully.

**Interfaces:**
```csharp
public class DashboardDataService
{
    public DashboardModel? GetDashboardData();
    public string? GetError();
    public void Reload();
}
```

**Dependencies:** `IWebHostEnvironment` (for `ContentRootPath`), `System.Text.Json`.

**Data:** Reads `data.json` from the project's content root directory (not `wwwroot`—the file is server-side only, never exposed via URL).

**Key behaviors:**
- **Constructor load:** Reads and deserializes `data.json` on first access. Caches the result in a private field.
- **Error capture:** If `data.json` is missing, malformed, or contains invalid data, the service captures the exception message in a `string? _error` field and returns `null` from `GetDashboardData()`.
- **Reload on refresh:** The `Reload()` method re-reads `data.json` from disk. Called on each page render to pick up file changes without restarting the app.
- **Thread safety:** Uses a simple `lock` around file reads. Concurrent access is not a concern (1-3 users), but the lock prevents partial reads during file saves.
- **JSON options:** Uses `JsonSerializerOptions` with `PropertyNameCaseInsensitive = true` and `JsonNamingPolicy.CamelCase` for forgiving deserialization.

```csharp
public class DashboardDataService
{
    private readonly string _dataFilePath;
    private DashboardModel? _cachedData;
    private string? _error;
    private readonly object _lock = new();

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataFilePath = Path.Combine(env.ContentRootPath, "data.json");
        Reload();
    }

    public DashboardModel? GetDashboardData() => _cachedData;
    public string? GetError() => _error;

    public void Reload()
    {
        lock (_lock)
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    _error = "data.json not found. Please create a data.json file in the project root.";
                    _cachedData = null;
                    return;
                }

                var json = File.ReadAllText(_dataFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                _cachedData = JsonSerializer.Deserialize<DashboardModel>(json, options);
                _error = null;
            }
            catch (JsonException ex)
            {
                _error = $"Error parsing data.json: {ex.Message}";
                _cachedData = null;
            }
            catch (Exception ex)
            {
                _error = $"Error loading data.json: {ex.Message}";
                _cachedData = null;
            }
        }
    }
}
```

---

### 4. `Dashboard.razor` — Main Page Component

**Responsibility:** Top-level page component. Orchestrates the three visual sections. Handles error display. Injects the data service and passes data to child components.

**Route:** `@page "/"` — the only page in the application.

**Render mode:** Static SSR (no `@rendermode` attribute). The page is read-only; no WebSocket connection is needed. This eliminates SignalR overhead and produces a clean, static HTML response.

**Dependencies:** `DashboardDataService` (injected via `@inject`).

**Key behaviors:**
- On initialization (`OnInitialized`), calls `_dataService.Reload()` then reads the data. This ensures every browser refresh picks up `data.json` changes.
- If `GetError()` returns non-null, renders a centered red error message instead of the dashboard.
- If data is valid, renders three child components in a flex column layout.
- Sets `<body>` dimensions to `1920px × 1080px` via `app.css`.

**Template structure:**
```razor
@page "/"
@inject DashboardDataService DataService

@if (_error is not null)
{
    <div class="error-display">
        <p>⚠ @_error</p>
        <p>Please check your data.json file and refresh.</p>
    </div>
}
else if (_data is not null)
{
    <DashboardHeader Data="@_data" />
    <TimelineSection Data="@_data" />
    <HeatmapGrid Data="@_data" />
}
```

---

### 5. `DashboardHeader.razor` — Header Component

**Responsibility:** Render the top header bar with project title, ADO backlog link, subtitle, and milestone legend.

**Maps to:** `.hdr` element in `OriginalDesignConcept.html`.

**Parameters:**
```csharp
[Parameter] public DashboardModel Data { get; set; }
```

**Visual specification:**
- Flexbox row, `justify-content: space-between`, `align-items: center`.
- Padding: `12px 44px 10px`. Bottom border: `1px solid #E0E0E0`.
- Left: `<h1>` (24px bold) with title text + ` ↗ ADO Backlog` link (`#0078D4`, `target="_blank"`).
- Left below: subtitle div (12px, `#888`).
- Right: Legend flex row (22px gap) with four items:
  - Gold diamond (12×12 rotated square, `#F4B400`) — "PoC Milestone"
  - Green diamond (12×12 rotated square, `#34A853`) — "Production Release"
  - Gray circle (8×8, `#999`) — "Checkpoint"
  - Red bar (2×14, `#EA4335`) — "Now" with current date context

**Data bindings:** `Data.Title`, `Data.Subtitle`, `Data.BacklogUrl`.

---

### 6. `TimelineSection.razor` — Timeline Component

**Responsibility:** Render the SVG milestone timeline with track sidebar, month gridlines, milestone markers, and NOW indicator.

**Maps to:** `.tl-area` element in `OriginalDesignConcept.html`.

**Parameters:**
```csharp
[Parameter] public DashboardModel Data { get; set; }
```

**Sub-structure (rendered inline, not separate components):**
- **Track sidebar** (230px, flex-shrink 0): Lists `Data.Tracks` vertically with `justify-content: space-around`. Each track shows ID in its color + name in `#444`.
- **SVG area** (flex 1, 1560×185): Generated dynamically using date-to-pixel mapping.

**SVG rendering logic:**

1. **Date-to-X mapping function:**
   ```csharp
   private double DateToX(DateOnly date)
   {
       var totalDays = (_dateRangeEnd.DayNumber - _dateRangeStart.DayNumber);
       if (totalDays == 0) return 0;
       var dayOffset = date.DayNumber - _dateRangeStart.DayNumber;
       return (dayOffset / (double)totalDays) * SvgWidth;
   }
   ```
   - `_dateRangeStart` = first day of the first month in `Data.Months`.
   - `_dateRangeEnd` = last day of the last month in `Data.Months`.
   - `SvgWidth` = 1560 (constant matching reference design).

2. **Month gridlines:** For each month, draw a vertical `<line>` at the X position of the 1st of that month. Add a `<text>` label (11px, `#666`, font-weight 600) 5px to the right.

3. **Track lines:** For each track in `Data.Tracks`, draw a horizontal `<line>` from x=0 to x=1560. Y positions are evenly distributed: `y = 28 + (trackIndex * trackSpacing)` where `trackSpacing = (185 - 28) / trackCount` (leaving room for month labels at top).

4. **Milestone markers:** For each milestone in each track:
   - `Checkpoint` → `<circle>` with `r="5-7"`, white fill, track-color stroke, stroke-width 2.5. Small gray dots (`r=4, fill=#999`) for minor checkpoints.
   - `PoC` → `<polygon>` diamond shape, fill `#F4B400`, with drop shadow filter `url(#sh)`.
   - `Production` → `<polygon>` diamond shape, fill `#34A853`, with drop shadow filter.
   - Diamond points formula: `points="{x},{y-11} {x+11},{y} {x},{y+11} {x-11},{y}"`.

5. **Date labels:** `<text>` elements (10px, `#666`, `text-anchor: middle`) positioned above or below the track line. Alternate above/below by milestone index to avoid overlap.

6. **NOW indicator:** Dashed red vertical line (`stroke: #EA4335, width 2, dasharray 5,3`) at `DateToX(Data.CurrentDate)`. "NOW" label in 10px bold red.

7. **Drop shadow filter:** Defined in `<defs>`: `<feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>`.

---

### 7. `HeatmapGrid.razor` — Heatmap Component

**Responsibility:** Render the CSS Grid status matrix with four status rows × N month columns.

**Maps to:** `.hm-wrap` and `.hm-grid` elements in `OriginalDesignConcept.html`.

**Parameters:**
```csharp
[Parameter] public DashboardModel Data { get; set; }
```

**Layout:**
- Outer wrapper: `flex: 1; min-height: 0; padding: 10px 44px 10px; flex-direction: column`.
- Title bar: `"MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS"` (14px bold, `#888`, uppercase).
- Grid: Dynamic `grid-template-columns: 160px repeat(@Data.Months.Count, 1fr)` via inline style binding.
- Grid: `grid-template-rows: 36px repeat(4, 1fr)`.

**Grid cell rendering (by row, by column):**

| Cell Position | Component/Logic |
|---|---|
| Corner (0,0) | Static div `.hm-corner` displaying "STATUS" |
| Column headers (0, 1..N) | Month name. If month matches `Data.CurrentMonth`, apply `.apr-hdr` class (gold background `#FFF0D0`, text `#C07700`, append " ★ Now") |
| Row headers (1..4, 0) | Status label with category-specific styling. Prefixed with emoji: ✅ Shipped, 🔄 In Progress, 📋 Carryover, 🚫 Blockers |
| Data cells (row, col) | List of items from `Data.StatusRows[category][month]`. Each item rendered as `<div class="it">` with `::before` bullet. Empty → gray dash "-" |

**Status row configuration (hardcoded, maps to CSS classes):**

```csharp
private static readonly StatusRowConfig[] Rows = new[]
{
    new StatusRowConfig("shipped",    "✅ SHIPPED",      "ship-hdr",   "ship-cell"),
    new StatusRowConfig("inProgress", "🔄 IN PROGRESS",  "prog-hdr",   "prog-cell"),
    new StatusRowConfig("carryover",  "📋 CARRYOVER",    "carry-hdr",  "carry-cell"),
    new StatusRowConfig("blockers",   "🚫 BLOCKERS",     "block-hdr",  "block-cell"),
};
```

**Current month highlighting:** For each data cell, if the column month matches `Data.CurrentMonth`, add the `.apr` CSS class (which applies a slightly darker background tint per row).

---

### 8. `App.razor` — Blazor Application Shell

**Responsibility:** HTML document shell. Sets `<head>` metadata, references `app.css`, defines the component render tree root.

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <base href="/" />
    <link rel="stylesheet" href="app.css" />
    <link rel="stylesheet" href="ReportingDashboard.styles.css" />
    <HeadOutlet />
</head>
<body>
    <Routes />
</body>
</html>
```

---

### 9. `MainLayout.razor` — Layout Component

**Responsibility:** Minimal layout wrapper. No navigation, no sidebar, no chrome—just renders the page body directly.

```razor
@inherits LayoutComponentBase
@Body
```

---

### 10. `app.css` — Global Styles

**Responsibility:** Port the `OriginalDesignConcept.html` CSS into global styles. This is the **primary CSS file** for the application.

**Key decisions:**
- Global CSS is preferred over scoped CSS isolation for this project because the entire application is a single page with a unified visual language. Component isolation adds complexity with no benefit.
- CSS custom properties are defined in `:root` for the color palette.
- The `body` tag is fixed at `1920px × 1080px` with `overflow: hidden`.

**CSS custom properties:**
```css
:root {
    /* Status colors */
    --shipped-color: #34A853;
    --shipped-bg: #F0FBF0;
    --shipped-bg-current: #D8F2DA;
    --shipped-hdr-bg: #E8F5E9;
    --shipped-hdr-text: #1B7A28;

    --progress-color: #0078D4;
    --progress-bg: #EEF4FE;
    --progress-bg-current: #DAE8FB;
    --progress-hdr-bg: #E3F2FD;
    --progress-hdr-text: #1565C0;

    --carryover-color: #F4B400;
    --carryover-bg: #FFFDE7;
    --carryover-bg-current: #FFF0B0;
    --carryover-hdr-bg: #FFF8E1;
    --carryover-hdr-text: #B45309;

    --blockers-color: #EA4335;
    --blockers-bg: #FFF5F5;
    --blockers-bg-current: #FFE4E4;
    --blockers-hdr-bg: #FEF2F2;
    --blockers-hdr-text: #991B1B;

    /* Milestone markers */
    --poc-color: #F4B400;
    --production-color: #34A853;
    --checkpoint-color: #999;
    --now-color: #EA4335;

    /* Layout */
    --current-month-hdr-bg: #FFF0D0;
    --current-month-hdr-text: #C07700;
    --border-color: #E0E0E0;
    --border-heavy: #CCC;
}
```

**Full CSS structure:** Port every class from `OriginalDesignConcept.html` verbatim: `.hdr`, `.sub`, `.tl-area`, `.tl-svg-box`, `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`, `.it`, and all row-specific classes (`.ship-*`, `.prog-*`, `.carry-*`, `.block-*`). Replace hardcoded color values with CSS custom property references.

---

## Component Interactions

### Data Flow (Request Path)

```
Browser GET /
    → Kestrel
    → Blazor Static SSR Pipeline
    → Dashboard.razor.OnInitialized()
        → DashboardDataService.Reload()
            → File.ReadAllText("data.json")
            → JsonSerializer.Deserialize<DashboardModel>()
            → Cache result in memory
        → DashboardDataService.GetDashboardData()
        → If error: render error div
        → If data: render component tree
            → DashboardHeader.razor [Parameter: DashboardModel]
            → TimelineSection.razor [Parameter: DashboardModel]
            → HeatmapGrid.razor [Parameter: DashboardModel]
    → HTML Response (complete 1920×1080 page)
    → Browser renders
```

### Component Parameter Flow

```
Dashboard.razor (owns DashboardModel)
  │
  ├──[Data]──→ DashboardHeader.razor
  │               Reads: Title, Subtitle, BacklogUrl, CurrentDate
  │
  ├──[Data]──→ TimelineSection.razor
  │               Reads: Tracks[], Months[], CurrentDate
  │               Computes: DateToX() mapping, track Y positions
  │
  └──[Data]──→ HeatmapGrid.razor
                  Reads: Months[], StatusRows{}, CurrentMonth
                  Computes: Grid template columns, cell CSS classes
```

### Refresh Cycle

1. User edits `data.json` in a text editor.
2. User refreshes browser (`F5` or `Ctrl+R`).
3. Browser sends GET `/`.
4. `Dashboard.razor.OnInitialized()` fires → calls `DashboardDataService.Reload()`.
5. Service re-reads `data.json` from disk, re-deserializes.
6. New data flows through component tree.
7. Updated HTML renders in < 1 second.

No file watcher is needed. The `Reload()` call on every page initialization is sufficient for the use case (manual refresh workflow).

---

## Data Model

### `DashboardModel` (root)

```csharp
public class DashboardModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string BacklogUrl { get; set; } = string.Empty;
    public string CurrentDate { get; set; } = string.Empty;  // "2026-04-14" ISO format
    public List<string> Months { get; set; } = new();         // ["Jan", "Feb", "Mar", "Apr"]
    public List<TrackModel> Tracks { get; set; } = new();
    public StatusRowsModel StatusRows { get; set; } = new();
}
```

### `TrackModel` (milestone track)

```csharp
public class TrackModel
{
    public string Id { get; set; } = string.Empty;        // "M1"
    public string Label { get; set; } = string.Empty;     // "API Gateway & Auth"
    public string Color { get; set; } = string.Empty;     // "#0078D4"
    public List<MilestoneModel> Milestones { get; set; } = new();
}
```

### `MilestoneModel` (individual milestone)

```csharp
public class MilestoneModel
{
    public string Date { get; set; } = string.Empty;      // "2026-03-20" ISO format
    public string Type { get; set; } = string.Empty;      // "Checkpoint" | "PoC" | "Production"
    public string Label { get; set; } = string.Empty;     // "Mar 20 PoC"
}
```

### `StatusRowsModel` (heatmap data)

```csharp
public class StatusRowsModel
{
    public Dictionary<string, List<string>> Shipped { get; set; } = new();
    public Dictionary<string, List<string>> InProgress { get; set; } = new();
    public Dictionary<string, List<string>> Carryover { get; set; } = new();
    public Dictionary<string, List<string>> Blockers { get; set; } = new();
}
```

### Design Decisions

- **Classes, not records:** Using `class` with default values instead of `record` ensures `System.Text.Json` deserialization works without custom converters. Nullable defaults prevent crashes on missing fields.
- **Strings for dates:** `CurrentDate` and milestone `Date` are stored as ISO strings and parsed to `DateOnly` in the rendering layer. This keeps the JSON schema simple.
- **Strings for milestone types:** Using `string` instead of an `enum` for `Type` avoids deserialization failures on unrecognized values. The rendering layer uses a `switch` expression with a default fallback.
- **Dictionary<string, List<string>> for status rows:** Each status category maps month names to item lists. This matches the JSON structure directly and keeps deserialization trivial.
- **No database. No Entity Framework. No migrations.** The single `data.json` file is the entire data store.

### `data.json` Schema

```json
{
  "title": "string — Dashboard title (required)",
  "subtitle": "string — Organization context line (required)",
  "backlogUrl": "string — URL for ADO Backlog link (optional, renders '#' if empty)",
  "currentDate": "string — ISO date 'YYYY-MM-DD' for NOW indicator (required)",
  "months": ["string — 3-letter month abbreviations, ordered chronologically"],
  "tracks": [
    {
      "id": "string — Short identifier e.g. 'M1'",
      "label": "string — Track description",
      "color": "string — Hex color e.g. '#0078D4'",
      "milestones": [
        {
          "date": "string — ISO date 'YYYY-MM-DD'",
          "type": "string — 'Checkpoint' | 'PoC' | 'Production'",
          "label": "string — Display label near marker"
        }
      ]
    }
  ],
  "statusRows": {
    "shipped": { "MonthAbbrev": ["item1", "item2"] },
    "inProgress": { "MonthAbbrev": ["item1"] },
    "carryover": { "MonthAbbrev": ["item1"] },
    "blockers": { "MonthAbbrev": ["item1"] }
  }
}
```

### Data Constraints

| Field | Constraint | Enforced By |
|---|---|---|
| `months` | 3–8 items | Rendering logic (visual balance degrades outside this range) |
| `tracks` | 1–5 items | SVG spacing calculation (Y positions compress below usable size at >5) |
| `statusRows.*` keys | Must match values in `months` array | Rendering logic (missing keys render as empty "-" cells) |
| `currentDate` | Must fall within date range implied by `months` | NOW line renders off-screen if outside range (non-fatal) |
| `milestones[].date` | Must be parseable as `DateOnly` | `DateOnly.TryParse` with fallback (skip unparseable milestones) |

---

## API Contracts

This application has **no REST API, no GraphQL, no gRPC**. It is a server-rendered Blazor application that serves HTML directly.

### HTTP Endpoints

| Method | Path | Response | Purpose |
|--------|------|----------|---------|
| `GET` | `/` | `text/html` — Full 1920×1080 dashboard page | Primary (and only) endpoint |
| `GET` | `/_framework/*` | Blazor framework scripts (if interactive mode) | Framework internals |
| `GET` | `/app.css` | Stylesheet | Static file serving |
| `GET` | `/{anything}` | 404 or redirect to `/` | No other routes exist |

### Error Responses

| Condition | Behavior |
|---|---|
| `data.json` missing | Renders HTML page with centered error: "⚠ data.json not found..." |
| `data.json` malformed JSON | Renders HTML page with centered error: "⚠ Error parsing data.json: {details}" |
| `data.json` valid JSON but missing fields | Renders dashboard with defaults (empty strings, empty lists). No crash. |
| Server not running | Browser shows standard connection refused |

### Error Display Component

When an error occurs, the page renders a centered message instead of the dashboard:

```html
<div style="display:flex; align-items:center; justify-content:center;
            width:1920px; height:1080px; font-family:'Segoe UI',Arial,sans-serif;">
    <div style="text-align:center; color:#991B1B; max-width:800px;">
        <p style="font-size:24px; font-weight:700;">⚠ Dashboard Error</p>
        <p style="font-size:16px; margin-top:12px;">{error message}</p>
        <p style="font-size:14px; color:#888; margin-top:8px;">
            Please check your data.json file and refresh the browser.
        </p>
    </div>
</div>
```

---

## Infrastructure Requirements

### Runtime Requirements

| Requirement | Specification |
|---|---|
| **.NET SDK** | 8.0.x LTS (any patch version) |
| **OS** | Windows 10/11 (primary, Segoe UI font required). Linux/macOS functional but font fallback to Arial. |
| **Browser** | Chromium-based (Chrome 120+, Edge 120+) for screenshot fidelity |
| **Memory** | < 50 MB working set |
| **Disk** | < 10 MB (project files + build output) |
| **Network** | Localhost only. No outbound connections. No inbound beyond `localhost:5000/5001`. |

### Hosting

- **Kestrel** (built into .NET 8 ASP.NET Core). No IIS, no Nginx, no reverse proxy.
- Listens on `http://localhost:5000` by default.
- HTTPS on `https://localhost:5001` using the .NET dev certificate (optional, not required for local use).

### Storage

- **`data.json`** — Single file in project content root. Read on every page load. ~1-50 KB.
- **No database.** No SQLite, no LiteDB, no Entity Framework.
- **No file uploads.** No user-generated content.

### CI/CD

Not required. This is a local development tool. If desired in the future:

```yaml
# Minimal GitHub Actions (optional, not in scope)
- dotnet build --no-restore --warnaserror
- dotnet test (if tests exist)
```

### Build Commands

```bash
# First time
dotnet restore
dotnet build

# Run
dotnet run

# Development with hot reload
dotnet watch run
```

---

## Technology Stack Decisions

| Layer | Technology | Justification |
|---|---|---|
| **Runtime** | .NET 8.0.x LTS | Mandated by requirements. LTS support until Nov 2026. |
| **UI Framework** | Blazor Server (Static SSR) | Mandated. Static SSR eliminates SignalR overhead for a read-only page. |
| **CSS Layout** | Native CSS Grid + Flexbox | Reference design already uses these. No framework needed. |
| **SVG Rendering** | Inline SVG in Razor markup | Timeline geometry (lines, diamonds, circles) is trivial. No charting library justified. |
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8. Zero additional packages. Handles all deserialization needs. |
| **Data Storage** | Flat `data.json` file | Explicit requirement. Read from disk on each request. |
| **Web Server** | Kestrel (built-in) | Default for .NET 8. No external server needed for localhost. |
| **CSS Strategy** | Single global `app.css` | One page, one visual language. Component isolation adds complexity with no benefit. |
| **Font** | Segoe UI (system) | Windows system font. No web font downloads. Fallback: Arial. |
| **Testing** | None (optional bUnit/xUnit) | Not required per scope. Manual visual testing against reference design is the validation method. |

### Rejected Alternatives

| Alternative | Reason Rejected |
|---|---|
| **Blazor WebAssembly** | Slower initial load. Server-side rendering is simpler for a local tool. |
| **Interactive Server mode** | Adds SignalR WebSocket. Unnecessary for a static, read-only page. |
| **Chart.js / D3.js** | JavaScript dependency. The SVG timeline is simple enough to render in Razor. |
| **Bootstrap / Tailwind** | Would fight the pixel-perfect reference CSS. More code to override than to write from scratch. |
| **Entity Framework + SQLite** | Data lives in a flat JSON file. A database adds startup complexity and migration overhead for zero benefit. |
| **Separate API project** | No consumers. The data service is internal to the Blazor app. |
| **Newtonsoft.Json** | Third-party package. `System.Text.Json` is built-in and sufficient. |
| **CSS Modules / Scoped CSS** | Single-page app with unified styling. Isolation adds build complexity with no encapsulation benefit. |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component.

### Component Map

```
┌─────────────────────────────────────────────────────────────────┐
│  DashboardHeader.razor                                   .hdr  │
│  ┌──────────────────────────┐  ┌──────────────────────────────┐│
│  │ Title + Link + Subtitle  │  │ Legend (PoC/Prod/Check/Now)  ││
│  └──────────────────────────┘  └──────────────────────────────┘│
├─────────────────────────────────────────────────────────────────┤
│  TimelineSection.razor                              .tl-area   │
│  ┌────────┐┌──────────────────────────────────────────────────┐│
│  │Sidebar ││ SVG Timeline (inline in TimelineSection.razor)   ││
│  │Track   ││ - Month gridlines    - Track lines              ││
│  │Labels  ││ - Milestone markers  - NOW indicator             ││
│  │230px   ││ - Date labels        - Drop shadows              ││
│  └────────┘└──────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────────────┤
│  HeatmapGrid.razor                                  .hm-wrap   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Title: "MONTHLY EXECUTION HEATMAP..."       .hm-title    │  │
│  ├────────┬──────┬──────┬──────┬──────┐                     │  │
│  │ STATUS │ Jan  │ Feb  │ Mar  │ Apr★ │        .hm-grid     │  │
│  ├────────┼──────┼──────┼──────┼──────┤                     │  │
│  │SHIPPED │items │items │items │items │  ← green row        │  │
│  ├────────┼──────┼──────┼──────┼──────┤                     │  │
│  │IN PROG │items │items │items │items │  ← blue row         │  │
│  ├────────┼──────┼──────┼──────┼──────┤                     │  │
│  │CARRY   │  -   │  -   │items │items │  ← amber row        │  │
│  ├────────┼──────┼──────┼──────┼──────┤                     │  │
│  │BLOCKED │  -   │  -   │  -   │items │  ← red row          │  │
│  └────────┴──────┴──────┴──────┴──────┘                     │  │
│                                                              │  │
└─────────────────────────────────────────────────────────────────┘
```

### Detailed Component Specifications

#### `DashboardHeader.razor`

| Aspect | Specification |
|---|---|
| **Visual section** | `.hdr` in reference HTML |
| **CSS layout** | `display: flex; align-items: center; justify-content: space-between; padding: 12px 44px 10px; border-bottom: 1px solid #E0E0E0; flex-shrink: 0;` |
| **Data bindings** | `@Data.Title`, `@Data.BacklogUrl`, `@Data.Subtitle` |
| **Interactions** | ADO Backlog link opens `Data.BacklogUrl` in new tab (`target="_blank"`) |
| **Legend items** | Static HTML with CSS-shaped indicators (rotated squares for diamonds, circle for checkpoint, bar for NOW). Legend text is hardcoded; indicator shapes use CSS `transform: rotate(45deg)`. |

#### `TimelineSection.razor`

| Aspect | Specification |
|---|---|
| **Visual section** | `.tl-area` in reference HTML |
| **CSS layout** | `display: flex; align-items: stretch; padding: 6px 44px 0; height: 196px; flex-shrink: 0; border-bottom: 2px solid #E8E8E8; background: #FAFAFA;` |
| **Sidebar CSS** | `width: 230px; flex-shrink: 0; display: flex; flex-direction: column; justify-content: space-around; padding: 16px 12px 16px 0; border-right: 1px solid #E0E0E0;` |
| **SVG CSS** | `flex: 1; padding-left: 12px; padding-top: 6px;` containing `<svg width="1560" height="185" style="overflow:visible; display:block;">` |
| **Data bindings** | `@foreach (var track in Data.Tracks)` for sidebar labels and SVG track lines. `@foreach (var milestone in track.Milestones)` for markers. |
| **Computed values** | `DateToX()` method for horizontal positioning. Track Y = `28 + (index * (157 / trackCount))` for vertical spacing. |
| **Interactions** | None. Read-only SVG. |

#### `HeatmapGrid.razor`

| Aspect | Specification |
|---|---|
| **Visual section** | `.hm-wrap` and `.hm-grid` in reference HTML |
| **CSS layout** | Outer: `flex: 1; min-height: 0; display: flex; flex-direction: column; padding: 10px 44px 10px;` |
| **Grid CSS** | `display: grid; grid-template-columns: 160px repeat(@Data.Months.Count, 1fr); grid-template-rows: 36px repeat(4, 1fr); border: 1px solid #E0E0E0; flex: 1; min-height: 0;` — **column count is dynamic** via inline Razor `style` attribute. |
| **Data bindings** | Corner cell: static "STATUS". Column headers: `@foreach (var month in Data.Months)`. Row headers: hardcoded 4 status categories. Data cells: `@foreach` nested loop (4 rows × N months), reading `Data.StatusRows.Shipped[month]` etc. |
| **Current month detection** | Compare each month string against `Data.CurrentMonth` (derived from `Data.CurrentDate` parsing). Apply `.apr-hdr` to matching column header and `.apr` to matching data cells. |
| **Empty cell handling** | If `statusRows[category]` does not contain the month key, or the list is empty, render `<span style="color:#AAA">–</span>`. |
| **Interactions** | None. Read-only grid. |

### CSS Class Mapping (Reference → Blazor)

| Reference CSS Class | Blazor Component | Notes |
|---|---|---|
| `.hdr`, `.sub` | `DashboardHeader.razor` | Direct port |
| `.tl-area`, `.tl-svg-box` | `TimelineSection.razor` | Direct port |
| `.hm-wrap`, `.hm-title`, `.hm-grid` | `HeatmapGrid.razor` | Direct port |
| `.hm-corner`, `.hm-col-hdr`, `.apr-hdr` | `HeatmapGrid.razor` | Column headers |
| `.hm-row-hdr`, `.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr` | `HeatmapGrid.razor` | Row headers with status colors |
| `.hm-cell`, `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` | `HeatmapGrid.razor` | Data cells with row-specific backgrounds |
| `.it`, `.it::before` | `HeatmapGrid.razor` | Item text + colored bullet pseudo-element |
| `.apr` (on cells) | `HeatmapGrid.razor` | Current month highlight (darker background) |

---

## Security Considerations

### Threat Model

This application runs **exclusively on localhost** with **no authentication**, accessed by **1-3 trusted users** on the same machine. The threat surface is minimal.

| Threat | Risk Level | Mitigation |
|---|---|---|
| **Unauthorized network access** | Low | Kestrel binds to `localhost` only by default. Not accessible from other machines. |
| **Malicious data.json** | Very Low | File is manually authored by the user. No external input vector. |
| **XSS via data.json content** | Low | Blazor's Razor engine **HTML-encodes all `@` expressions by default**. A title containing `<script>` renders as literal text, not executable code. |
| **Path traversal** | None | The service reads a hardcoded filename (`data.json`), not user-supplied paths. |
| **Credential exposure** | None | No credentials exist. No auth tokens, no API keys, no connection strings. |
| **Data sensitivity** | Low | If `data.json` contains real project names, add it to `.gitignore`. Provide `data.template.json` with fictional data for the repo. |

### Security Controls

1. **Blazor auto-encoding:** All `@variable` expressions in Razor templates are HTML-encoded. No raw HTML injection is possible unless `@((MarkupString)...)` is explicitly used (which this architecture does not use).
2. **Localhost binding:** Kestrel defaults to `localhost:5000/5001`. No `--urls 0.0.0.0:*` configuration.
3. **`.gitignore` guidance:** `data.json` should be gitignored if it contains real project data. `data.template.json` is committed.
4. **No secrets:** No `appsettings.json` secrets, no connection strings, no API keys. The only configuration is `data.json` content data.

### Input Validation

The `DashboardDataService` performs defensive parsing:

- `JsonSerializer.Deserialize` with `AllowTrailingCommas = true` and `ReadCommentHandling = JsonCommentHandling.Skip` for forgiving JSON parsing.
- All model properties have default values (`string.Empty`, `new()`) to prevent `NullReferenceException` on missing fields.
- `DateOnly.TryParse` is used for date fields; unparseable dates are skipped with no crash.
- The service catches `JsonException` and general `Exception`, storing a user-friendly error message.

---

## Scaling Strategy

**This application does not scale.** This is intentional and correct for the requirements.

| Dimension | Current Capacity | Scaling Path (if ever needed) |
|---|---|---|
| **Users** | 1-3 concurrent (one machine) | Not applicable |
| **Data volume** | 50 KB JSON max | Not applicable |
| **Pages** | 1 | Not applicable |
| **Projects** | 1 per `data.json` file | Swap `data.json` files or add a query parameter to select files |
| **Months** | 3-8 columns | CSS Grid handles dynamically; visual balance degrades beyond 8 |
| **Tracks** | 1-5 | SVG spacing formula handles dynamically; tracks compress at >5 |

### Performance Budget

| Metric | Budget | Achieved Via |
|---|---|---|
| **Page load** | < 2s | Static SSR (no SignalR handshake), single file read, minimal CSS |
| **Data refresh** | < 1s | `Reload()` reads ~50 KB file, deserializes in < 100ms |
| **Build** | < 10s | Single project, zero third-party packages |
| **Memory** | < 50 MB | One cached `DashboardModel` object, no database context |

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| 1 | **SVG date-to-pixel math produces incorrect positions** | Medium | Medium | Implement `DateToX()` as a pure function. Test with known dates against the reference design's pixel positions (e.g., Jan 1 = x:0, Apr 14 ≈ x:823). Hardcode reference values as comments for validation. |
| 2 | **CSS Grid heatmap doesn't fill remaining vertical space** | Medium | High | Use `flex: 1; min-height: 0` on `.hm-wrap` and `.hm-grid` exactly as the reference design does. Test with 3, 4, 6, and 8 month columns. |
| 3 | **Blazor component rendering differs from raw HTML** | Low | Medium | Blazor's Razor engine produces standard HTML. Compare rendered output against reference using browser DevTools element inspector. |
| 4 | **`data.json` schema drift causes silent rendering errors** | Medium | Low | Default values on all model properties prevent crashes. Missing data renders as empty cells ("-") rather than exceptions. Add schema documentation in `data.template.json`. |
| 5 | **Font rendering differences (Segoe UI availability)** | Low | Low | Segoe UI is pre-installed on Windows (primary target). CSS `font-family` includes Arial fallback. Visual difference is negligible for screenshots. |
| 6 | **Over-engineering the solution** | Medium | Medium | Strict file count limit (≤15). No packages beyond template defaults. No database. No API. Code review against this architecture doc to catch scope creep. |
| 7 | **Browser print/screenshot produces unexpected dimensions** | Low | High | Fixed `body` at `1920px × 1080px` with `overflow: hidden`. Document the exact Chrome DevTools screenshot command in the README. Test with both Chrome and Edge. |
| 8 | **`System.Text.Json` fails on edge-case JSON** | Low | Low | Enable `AllowTrailingCommas` and `ReadCommentHandling.Skip`. Catch `JsonException` with descriptive error message. Provide `data.template.json` as a known-good starting point. |
| 9 | **Static SSR doesn't support interactive features later** | Low | Low | Current scope has zero interactivity. If needed later, adding `@rendermode InteractiveServer` to `Dashboard.razor` is a one-line change. |
| 10 | **Milestone labels overlap on the SVG timeline** | Medium | Low | Alternate label positions above/below track lines by milestone index. For dense milestone clusters, truncate labels or reduce font size. This is a visual polish concern, not a blocking issue. |

### Critical Path

The highest-risk implementation path is the **SVG timeline rendering** (Risk #1 + #10). Recommended mitigation sequence:

1. **Phase 1:** Hardcode SVG positions matching the reference design pixel-for-pixel. Validate visually.
2. **Phase 2:** Replace hardcoded positions with `DateToX()` function. Compare output against Phase 1 known-good positions.
3. **Phase 3:** Test with varied `data.json` configurations (different date ranges, 1 track, 5 tracks, milestones at month boundaries).