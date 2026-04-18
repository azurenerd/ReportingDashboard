# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server (.NET 8) web application that reads `data.json` at startup and renders a 1920×1080-optimized visual report for screenshot capture. The system has no authentication, no database, and no cloud dependencies. It runs exclusively on a developer workstation via `dotnet run`.

**Goals:**
- Render a pixel-perfect 1920×1080 dashboard matching `OriginalDesignConcept.html`
- Drive all content from `data.json` with zero code changes required for data updates
- Serve at `http://localhost:5000` with sub-2-second first load
- Provide clear startup errors when `data.json` is missing or malformed

---

## System Components

### 1. `Program.cs` — Application Bootstrap

**Responsibilities:**
- Configure Kestrel to bind `127.0.0.1:5000` only
- Register `DashboardDataService` as a singleton
- Invoke `DashboardDataService.Load()` at startup; catch and log `DataLoadException` with `ILogger`
- Configure ASP.NET Core pipeline: static files, Blazor Server hub, routing to `Dashboard.razor`
- Set fallback error page route (`/error`) when data load fails

**Registration pattern:**
```csharp
builder.Services.AddSingleton<DashboardDataService>();
var app = builder.Build();
// Force eager initialization
app.Services.GetRequiredService<DashboardDataService>();
```

**Dependencies:** `DashboardDataService`, `ILogger<Program>`

---

### 2. `DashboardDataService` — Data Access Layer

**Responsibilities:**
- Read `data.json` from `Directory.GetCurrentDirectory()` on construction
- Deserialize JSON into `DashboardConfig` using `System.Text.Json`
- Validate required fields (throw `DataLoadException` with descriptive message on failure)
- Expose `DashboardConfig Data { get; }` and `bool IsLoaded { get; }` and `string? ErrorMessage { get; }`
- Compute derived values: `NowDate` (fallback to `DateOnly.FromDateTime(DateTime.Today)`), `CurrentColumn` (fallback to current month abbreviation)

**Interface:**
```csharp
public interface IDashboardDataService
{
    DashboardConfig? Data { get; }
    bool IsLoaded { get; }
    string? ErrorMessage { get; }
}
```

**Dependencies:** `System.Text.Json`, `ILogger<DashboardDataService>`

**Error handling:** Catches `FileNotFoundException`, `JsonException`, field-missing validation; sets `IsLoaded = false` and `ErrorMessage`; never throws past the service boundary.

---

### 3. `Dashboard.razor` — Main Page Component

**Responsibilities:**
- Inject `IDashboardDataService`; redirect render to `ErrorDisplay.razor` if `!IsLoaded`
- Compose the three layout sections: `DashboardHeader`, `MilestoneTimeline`, `ExecutionHeatmap`
- Apply body-level CSS: `width:1920px; height:1080px; overflow:hidden; display:flex; flex-direction:column`
- Set page `<title>` from `Data.Project.Title`

**Route:** `@page "/"` — single route, no navigation

**Dependencies:** `IDashboardDataService`, child components

---

### 4. `DashboardHeader.razor` — Header Component

**Responsibilities:**
- Render project title (`<h1>`), ADO backlog link, subtitle
- Render static legend (PoC diamond, Production diamond, Checkpoint circle, Now bar)
- Apply `.hdr` CSS class matching `OriginalDesignConcept.html`

**Parameters:**
```csharp
[Parameter] public ProjectConfig Project { get; set; }
```

**CSS:** `.hdr` — `padding:12px 44px 10px`, `border-bottom:1px solid #E0E0E0`, flex, `justify-content:space-between`

---

### 5. `MilestoneTimeline.razor` — Timeline Component

**Responsibilities:**
- Render left label panel: foreach `milestone` in `Data.Timeline.Milestones`, render label + description with milestone `color`
- Render inline SVG canvas (1560×185): month gridlines, NOW line, per-milestone track lines and events
- Compute SVG x-positions in `@code` block: `xPos = (date - startDate).TotalDays / totalDays * svgWidth`
- Render events by type: `checkpoint` → `<circle>`, `poc` → `<polygon fill="#F4B400">`, `release` → `<polygon fill="#34A853">`
- Include SVG `<defs>` drop-shadow filter

**Parameters:**
```csharp
[Parameter] public TimelineConfig Timeline { get; set; }
[Parameter] public DateOnly NowDate { get; set; }
```

**SVG constants (computed at render):**
- `svgWidth = 1560`
- `svgHeight = 185`
- `totalDays = (Timeline.EndDate - Timeline.StartDate).TotalDays`
- Y positions distributed evenly across milestones: `yBase + index * yStep`

**CSS:** `.tl-area` — `height:196px; background:#FAFAFA; padding:6px 44px 0; border-bottom:2px solid #E8E8E8; display:flex`

---

### 6. `ExecutionHeatmap.razor` — Heatmap Component

**Responsibilities:**
- Render section title "MONTHLY EXECUTION HEATMAP"
- Render CSS Grid: `grid-template-columns: 160px repeat(@columnCount, 1fr)`
- Render corner cell, column headers (highlight current month), row headers, data cells
- For each cell: lookup items from `HeatmapRow.Cells` by month; if empty, render dash `.it` with `color:#AAA`
- Apply row-specific CSS class pairs (`ship-hdr`/`ship-cell`, `prog-hdr`/`prog-cell`, `carry-hdr`/`carry-cell`, `block-hdr`/`block-cell`)
- Apply current-month cell variant class (`.apr` pattern — named by current month abbreviation)

**Parameters:**
```csharp
[Parameter] public HeatmapConfig Heatmap { get; set; }
[Parameter] public string CurrentColumn { get; set; }
```

**CSS:** `.hm-wrap` — `flex:1; min-height:0; display:flex; flex-direction:column; padding:10px 44px 10px`

---

### 7. `ErrorDisplay.razor` — Error State Component

**Responsibilities:**
- Render plain error message from `IDashboardDataService.ErrorMessage`
- No stack trace exposed; user-friendly language only
- Styled minimally to be clearly an error state, not a blank page

---

### 8. `TimelineCalculator` — SVG Math Helper (static class)

**Responsibilities:**
- `double DateToX(DateOnly date, DateOnly start, DateOnly end, double svgWidth)`
- `string DiamondPoints(double cx, double cy, double halfSize)` — returns SVG polygon points string
- `double[] DistributeYPositions(int count, double svgHeight)` — returns evenly spaced Y values
- Pure static methods; no dependencies; fully testable

---

## Component Interactions

```
Program.cs
  └─► DashboardDataService (singleton, constructed eagerly)
        └─► reads data.json → DashboardConfig
              └─► exposes via IDashboardDataService

HTTP Request → ASP.NET Core → Blazor Server Hub (SignalR)
  └─► Dashboard.razor
        ├─► [inject] IDashboardDataService
        ├─► DashboardHeader.razor ← ProjectConfig
        ├─► MilestoneTimeline.razor ← TimelineConfig + NowDate
        │     └─► TimelineCalculator (static SVG math)
        └─► ExecutionHeatmap.razor ← HeatmapConfig + CurrentColumn
```

**Data flow summary:**
1. `Program.cs` boots → `DashboardDataService` constructed → `data.json` read and deserialized → `DashboardConfig` stored in singleton
2. Browser connects → Blazor Server SignalR circuit established → `Dashboard.razor` renders
3. `Dashboard.razor` injects `IDashboardDataService` → passes sub-configs as `[Parameter]` to child components
4. Child components render HTML/SVG from parameters; no further I/O
5. If `IsLoaded == false`, `Dashboard.razor` renders `ErrorDisplay.razor` instead

**No inter-component events or callbacks.** Data flows strictly parent → child via `[Parameter]`. No `EventCallback`, no `CascadingValue` needed.

---

## Data Model

### C# Model Classes (`Models/` namespace)

```csharp
public class DashboardConfig
{
    public ProjectConfig Project { get; set; } = new();
    public TimelineConfig Timeline { get; set; } = new();
    public HeatmapConfig Heatmap { get; set; } = new();
}

public class ProjectConfig
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? AdoLink { get; set; }
}

public class TimelineConfig
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly? NowDate { get; set; }      // null → DateOnly.FromDateTime(DateTime.Today)
    public List<MilestoneTrack> Milestones { get; set; } = new();
}

public class MilestoneTrack
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#888";
    public List<MilestoneEvent> Events { get; set; } = new();
}

public enum MilestoneEventType { Checkpoint, Poc, Release }

public class MilestoneEvent
{
    public DateOnly Date { get; set; }
    public MilestoneEventType Type { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class HeatmapConfig
{
    public List<string> Columns { get; set; } = new();         // ["Jan","Feb","Mar","Apr","May","Jun"]
    public string? CurrentColumn { get; set; }                  // null → computed from DateTime.Today
    public List<HeatmapRow> Rows { get; set; } = new();
}

public enum HeatmapRowType { Shipped, InProgress, Carryover, Blocker }

public class HeatmapRow
{
    public HeatmapRowType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public List<HeatmapCell> Cells { get; set; } = new();
}

public class HeatmapCell
{
    public string Month { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new();
}
```

### `data.json` Schema

```json
{
  "project": {
    "title": "string (required)",
    "subtitle": "string (required)",
    "adoLink": "string (optional, URL)"
  },
  "timeline": {
    "startDate": "YYYY-MM-DD (required)",
    "endDate": "YYYY-MM-DD (required)",
    "nowDate": "YYYY-MM-DD (optional, defaults to today)",
    "milestones": [
      {
        "id": "string (required)",
        "label": "string (required)",
        "description": "string (required)",
        "color": "#RRGGBB (required)",
        "events": [
          {
            "date": "YYYY-MM-DD (required)",
            "type": "checkpoint | poc | release (required)",
            "label": "string (required)"
          }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["Jan", "Feb", ...],
    "currentColumn": "string (optional, e.g. 'Apr', defaults to current month)",
    "rows": [
      {
        "type": "Shipped | InProgress | Carryover | Blocker (required)",
        "label": "string (required)",
        "cells": [
          {
            "month": "string matching a columns entry (required)",
            "items": ["string", ...]
          }
        ]
      }
    ]
  }
}
```

**Storage:** Single flat file `data.json` in application working directory. No database. No migration tooling.

**Deserialization config:**
```csharp
new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter() }
}
```

---

## API Contracts

This application has no REST API. It is a Blazor Server application using SignalR for UI rendering. The only "contracts" are:

### HTTP Endpoints (ASP.NET Core routing)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/` | Serves `Dashboard.razor` (full dashboard or error state) |
| GET | `/_blazor` | Blazor Server SignalR hub endpoint (framework-managed) |
| GET | `/_framework/blazor.server.js` | Blazor JS bootstrap (framework-managed) |
| GET | `/error` | Error display fallback (rendered by `ErrorDisplay.razor` via router) |

### Internal Component Parameter Contracts

**`DashboardHeader`**
- Input: `ProjectConfig Project` — required, non-null
- Output: none (read-only render)

**`MilestoneTimeline`**
- Input: `TimelineConfig Timeline`, `DateOnly NowDate`
- Precondition: `Timeline.EndDate > Timeline.StartDate`
- Output: none

**`ExecutionHeatmap`**
- Input: `HeatmapConfig Heatmap`, `string CurrentColumn`
- Precondition: `Heatmap.Columns.Count >= 1`, all `HeatmapRow.Cells[*].Month` values must exist in `Heatmap.Columns`
- Output: none

### Error Contract

If `data.json` is unreadable or invalid, `IDashboardDataService.IsLoaded == false` and `ErrorMessage` contains a human-readable description. `Dashboard.razor` renders the error page. HTTP status code remains 200 (Blazor Server circuit behavior); no 500 is thrown to the client.

---

## Infrastructure Requirements

### Runtime
- **.NET 8 SDK** installed on developer workstation (Windows assumed; Segoe UI required)
- No Docker, no IIS, no Azure required

### Hosting
- **Kestrel** (built-in ASP.NET Core web server)
- Bind address: `127.0.0.1:5000` only
- `appsettings.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://127.0.0.1:5000"
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "ReportingDashboard": "Information"
    }
  }
}
```

### File System
- `data.json` co-located with app in working directory (e.g., project root when running `dotnet run`)
- No write access required; app is read-only at runtime

### Solution Structure
```
ReportingDashboard.sln
├── ReportingDashboard/                         (Blazor Server app)
│   ├── ReportingDashboard.csproj
│   ├── Program.cs
│   ├── data.json
│   ├── Models/
│   │   └── DashboardConfig.cs                  (all model classes)
│   ├── Services/
│   │   ├── IDashboardDataService.cs
│   │   └── DashboardDataService.cs
│   ├── Helpers/
│   │   └── TimelineCalculator.cs
│   ├── Components/
│   │   ├── App.razor
│   │   ├── Routes.razor
│   │   ├── Dashboard.razor
│   │   ├── Dashboard.razor.css
│   │   ├── DashboardHeader.razor
│   │   ├── DashboardHeader.razor.css
│   │   ├── MilestoneTimeline.razor
│   │   ├── MilestoneTimeline.razor.css
│   │   ├── ExecutionHeatmap.razor
│   │   ├── ExecutionHeatmap.razor.css
│   │   └── ErrorDisplay.razor
│   └── wwwroot/
│       └── app.css                             (global resets matching OriginalDesignConcept.html)
└── ReportingDashboard.Tests/                   (xUnit + bUnit)
    ├── ReportingDashboard.Tests.csproj
    ├── DashboardDataServiceTests.cs
    ├── TimelineCalculatorTests.cs
    ├── MilestoneTimelineComponentTests.cs
    └── ExecutionHeatmapComponentTests.cs
```

### CI/CD
- None required. Build verification: `dotnet build` and `dotnet test` run locally.
- Optional: add a `build.ps1` script that runs `dotnet build && dotnet test` for developer convenience.

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| Web framework | **Blazor Server (.NET 8)** | Mandatory. Enables component-based Razor rendering without WASM complexity. Server-side C# for SVG math. |
| Data deserialization | **`System.Text.Json` (built-in)** | No external package needed; handles `DateOnly`, enums, case-insensitive binding natively in .NET 8. |
| CSS approach | **Scoped `.razor.css` + global `app.css`** | Zero dependencies; CSS classes copied directly from `OriginalDesignConcept.html`. Scoped CSS prevents bleed. |
| SVG rendering | **Inline Razor SVG** | No charting library needed; design uses hand-crafted SVG. Razor loops emit SVG elements with C#-computed coordinates. |
| Font | **`'Segoe UI', Arial, sans-serif`** | System font on Windows; no CDN, no download. |
| Testing | **xUnit 2.9.x + bUnit 1.x** | bUnit renders Blazor components without a browser; validates data binding and DOM structure. xUnit for pure C# unit tests. |
| Logging | **`ILogger<T>` (ASP.NET Core built-in)** | Console provider included by default; zero extra packages for startup error reporting. |
| Dependency injection | **ASP.NET Core built-in DI** | `DashboardDataService` registered as singleton; no third-party DI container needed. |
| Date arithmetic | **`DateOnly` (.NET 6+)** | Avoids `DateTime` time-zone hazards in date-to-pixel mapping; `(end - start).TotalDays` is correct for calendar spans. |

**Explicitly rejected:**
- Bootstrap / Tailwind — zero-dependency requirement
- MudBlazor / Radzen — no component library; design is fully custom
- Chart.js / D3.js — SVG is hand-coded per design spec
- EF Core / SQLite — no persistence layer needed
- Playwright — deferred; bUnit sufficient for data-binding coverage

---

## Security Considerations

### Network Exposure
- Kestrel binds exclusively to `127.0.0.1:5000`. No LAN or internet exposure.
- Configured in `appsettings.json` Kestrel endpoints block, not hardcoded.

### Authentication & Authorization
- None by design. Single local user on own workstation. No user sessions, no cookies beyond Blazor's SignalR circuit cookie.

### Input Validation
- `data.json` is the only input surface. Validated on startup:
  - Required string fields checked for null/empty
  - `EndDate > StartDate` enforced (throws descriptive `DataLoadException` otherwise)
  - `HeatmapRow.Cells[*].Month` values validated against `Heatmap.Columns` list
  - `MilestoneEvent.Type` validated as known enum value
- No user-supplied input at runtime (read-only dashboard, no forms)

### Data Sensitivity
- `data.json` is plaintext project metadata; assumed non-sensitive (no PII, no secrets)
- ADO link in `data.json` is display-only; no credentials stored

### Dependency Surface
- Zero third-party NuGet packages for the app project (only `Microsoft.*` SDK packages)
- Test project adds `bunit` and `xunit` — test-only dependencies, not in production binary

### Error Disclosure
- `ErrorDisplay.razor` shows only `ErrorMessage` string (human-written); never exposes stack traces, file paths, or internal exception details to the browser

---

## Scaling Strategy

This is an intentionally single-user, single-instance, local workstation tool. There is no scaling requirement or concern. The following reflects the design ceiling:

**Concurrency:** One user, one browser tab, one SignalR circuit. Singleton `DashboardDataService` is read-only after construction; no thread-safety concerns.

**Data volume:** `data.json` is expected to be < 50KB for any realistic project dashboard. Deserialization is instantaneous.

**Performance levers if needed:**
- `DashboardDataService` reads `data.json` once at startup (singleton). If hot-reload is desired in the future, a `FileSystemWatcher` can be added to re-read the file and invalidate the cached `DashboardConfig`, triggering a `StateHasChanged` call on the circuit.
- SVG coordinate math is O(n) over milestone events (expected < 50 events). No optimization needed.
- Blazor Server SignalR circuit overhead is ~50ms on localhost; acceptable for sub-2s load requirement.

**No horizontal scaling, load balancing, or caching layer is required or appropriate for this tool.**

---

## Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| `data.json` missing at startup | Medium | High — blank/broken page | `DashboardDataService` catches `FileNotFoundException`, sets `ErrorMessage`; `Dashboard.razor` renders `ErrorDisplay.razor`; descriptive console log at `Error` level |
| `data.json` malformed JSON | Medium | High | `JsonException` caught; `ErrorMessage` includes field path from `JsonException.Path`; same error display flow |
| SVG x-position math produces out-of-bounds coordinates | Medium | Medium — markers render off-canvas | `TimelineCalculator.DateToX` clamps output to `[0, svgWidth]`; events outside date range are skipped with a console warning |
| `EndDate <= StartDate` in `data.json` | Low | High — division by zero in x-calc | Validated in `DashboardDataService`; throws `DataLoadException("EndDate must be after StartDate")` before any rendering occurs |
| Layout breaks on non-1920px display | High | Low — by design | Documented in README; users instructed to use browser devtools 1920×1080 emulation for screenshot capture |
| Blazor Server SignalR disconnects during screenshot | Low | Low | Screenshot is taken of static rendered DOM; no ongoing interactivity required |
| `data.json` `cells[*].month` value not in `columns` array | Medium | Low — cell silently empty | Validated at startup; warning logged; cell renders dash placeholder |
| Future schema changes break existing `data.json` files | Low | Medium | `DashboardDataService` validation produces specific field-level error messages; README documents schema with versioning note |
| Segoe UI not available (non-Windows OS) | Low | Low — falls back to Arial | `font-family: 'Segoe UI', Arial, sans-serif` fallback chain; visual degradation minor |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component, CSS layout strategy, data bindings, and render behavior.

### Section 1: Header (`DashboardHeader.razor`)

| Attribute | Specification |
|-----------|--------------|
| **Visual region** | Top bar, full width, ~50px tall |
| **CSS class** | `.hdr` — flex row, `justify-content:space-between`, `padding:12px 44px 10px`, `border-bottom:1px solid #E0E0E0`, `flex-shrink:0` |
| **Left side** | `<div>` containing `<h1>@Project.Title <a href="@Project.AdoLink">↗ ADO Backlog</a></h1>` + `<div class="sub">@Project.Subtitle</div>` |
| **Right side** | Four inline legend items; static HTML (icons are CSS-styled `<span>` elements); no data binding needed |
| **Data bindings** | `Project.Title`, `Project.AdoLink` (conditional render if non-null), `Project.Subtitle` |
| **Interactions** | ADO link is `<a href="..." target="_blank">` — no JS handlers |

### Section 2: Milestone Timeline (`MilestoneTimeline.razor`)

| Attribute | Specification |
|-----------|--------------|
| **Visual region** | Second row, full width, fixed 196px height |
| **Container CSS** | `.tl-area` — `height:196px; background:#FAFAFA; padding:6px 44px 0; border-bottom:2px solid #E8E8E8; display:flex; align-items:stretch; flex-shrink:0` |
| **Left label panel** | `width:230px; flex-shrink:0; display:flex; flex-direction:column; justify-content:space-around; padding:16px 12px 16px 0; border-right:1px solid #E0E0E0` — `@foreach (var m in Timeline.Milestones)` renders label div with `color:@m.Color` |
| **SVG box** | `.tl-svg-box` — `flex:1; padding-left:12px; padding-top:6px` — contains `<svg width="1560" height="185">` |
| **SVG data bindings** | `Timeline.StartDate`, `Timeline.EndDate`, `Timeline.Milestones[*].Color`, `Timeline.Milestones[*].Events[*].Date/Type/Label`, `NowDate` |
| **SVG gridlines** | `@foreach` over months between `StartDate` and `EndDate`; x = `TimelineCalculator.DateToX(monthStart, ...)` |
| **NOW line** | x = `TimelineCalculator.DateToX(NowDate, ...)` — `stroke="#EA4335" stroke-dasharray="5,3"` |
| **Track lines** | One `<line x1="0" y1="@y" x2="1560" y2="@y" stroke="@m.Color" stroke-width="3"/>` per milestone |
| **Event markers** | Switch on `event.Type`: `checkpoint` → `<circle>`, `poc` → `<polygon fill="#F4B400">`, `release` → `<polygon fill="#34A853">` — x from `TimelineCalculator.DateToX`, y from milestone Y position |
| **Interactions** | None — static render |

### Section 3: Execution Heatmap (`ExecutionHeatmap.razor`)

| Attribute | Specification |
|-----------|--------------|
| **Visual region** | Bottom section, fills remaining height (`flex:1`) |
| **Container CSS** | `.hm-wrap` — `flex:1; min-height:0; display:flex; flex-direction:column; padding:10px 44px 10px` |
| **Title** | `.hm-title` — static text "MONTHLY EXECUTION HEATMAP" |
| **Grid CSS** | `.hm-grid` — `display:grid; grid-template-columns:160px repeat(@Heatmap.Columns.Count, 1fr); grid-template-rows:36px repeat(4, 1fr); border:1px solid #E0E0E0` — applied via `style` attribute for dynamic column count |
| **Corner cell** | `.hm-corner` — static "STATUS" label |
| **Column headers** | `@foreach (var col in Heatmap.Columns)` — class = `hm-col-hdr` + (`col == CurrentColumn ? " apr-hdr" : ""`) |
| **Row headers** | `@foreach (var row in Heatmap.Rows)` — class = `hm-row-hdr ` + `GetRowHeaderClass(row.Type)` (e.g., `ship-hdr`) |
| **Data cells** | For each `(row, col)` pair: lookup `row.Cells.FirstOrDefault(c => c.Month == col)` → if null/empty items, render `<div class="it" style="color:#AAA">-</div>`; else `@foreach (var item in cell.Items)` → `<div class="it">@item</div>` |
| **Cell CSS class** | `hm-cell ` + `GetRowCellClass(row.Type)` + (col == CurrentColumn ? `" " + currentVariantClass` : "") — e.g., `hm-cell ship-cell` or `hm-cell ship-cell apr` |
| **Data bindings** | `Heatmap.Columns`, `Heatmap.Rows[*].Type/Label/Cells`, `CurrentColumn` |
| **Interactions** | None — static render |

### CSS Class Mapping Table

| Row Type | Header Class | Cell Class | Current-Month Cell Class |
|----------|-------------|-----------|--------------------------|
| `Shipped` | `ship-hdr` | `ship-cell` | `ship-cell apr` |
| `InProgress` | `prog-hdr` | `prog-cell` | `prog-cell apr` |
| `Carryover` | `carry-hdr` | `carry-cell` | `carry-cell apr` |
| `Blocker` | `block-hdr` | `block-cell` | `block-cell apr` |

> Note: The `apr` class name is reused from `OriginalDesignConcept.html` as a generic "current column" variant. Engineers must apply this class whenever `col == CurrentColumn`, regardless of the actual month name. Do not rename it to the current month dynamically — the CSS is static and references `.apr` as the current-month modifier class.