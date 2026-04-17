# Architecture

**Project:** Executive Project Reporting Dashboard
**Version:** 1.0 | **Date:** April 17, 2026 | **Stack:** C# .NET 8 · Blazor Server · Local Only

---

## Overview & Goals

This architecture defines a single-page Blazor Server application that renders an executive project reporting dashboard at a fixed 1920×1080 resolution, optimized for PowerPoint screenshot capture. The system reads all data from a local `data.json` file and produces a pixel-perfect rendering matching the `OriginalDesignConcept.html` reference design.

**Architectural principles:**

1. **Minimal by design** — Zero external NuGet dependencies, no database, no authentication, no cloud services. The entire application is ≤10 source files.
2. **Data-driven rendering** — Every visual element (title, milestones, heatmap items) is driven by `data.json`. No hardcoded content in components.
3. **CSS fidelity** — The stylesheet is a direct port of the reference HTML's CSS. Component markup mirrors the reference's DOM structure to ensure class reuse.
4. **Single-concern components** — Three child components map 1:1 to the three visual sections of the design (header, timeline, heatmap).
5. **Read-only data flow** — Data flows one direction: JSON file → service → page → child components via `[Parameter]`. No mutations, no state management.

**Business goals addressed:**

| Goal | Architectural Response |
|------|----------------------|
| Eliminate manual slides | Live-rendered dashboard at screenshot resolution |
| Standardize reporting | Consistent component structure + CSS color system |
| Maximize clarity | Three-section layout with color-coded status semantics |
| Minimize overhead | `dotnet run` startup, zero infrastructure |
| Enable rapid updates | Single `data.json` edit-and-restart cycle |

---

## System Components

### Solution Structure

```
ReportingDashboard.sln
│
├── ReportingDashboard/                     # Blazor Server project
│   ├── Program.cs                          # Host builder, DI registration
│   ├── Models/
│   │   └── DashboardData.cs                # All C# record types (single file)
│   ├── Services/
│   │   └── DashboardDataService.cs         # JSON loader + date-to-coordinate math
│   ├── Components/
│   │   ├── App.razor                       # Root component, <head> refs
│   │   ├── Routes.razor                    # Router setup
│   │   ├── Pages/
│   │   │   └── Dashboard.razor             # Single page, orchestrates children
│   │   ├── Layout/
│   │   │   └── MainLayout.razor            # Minimal layout (no nav, no sidebar)
│   │   ├── DashboardHeader.razor           # Header + legend
│   │   ├── TimelineSection.razor           # SVG milestone timeline
│   │   └── HeatmapGrid.razor              # CSS Grid status heatmap
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── dashboard.css              # Ported from reference HTML
│   │   └── data/
│   │       └── data.json                   # Dashboard configuration
│   └── Properties/
│       └── launchSettings.json             # localhost-only binding
│
└── ReportingDashboard.sln
```

**Total source files: 10** (Program.cs, DashboardData.cs, DashboardDataService.cs, App.razor, Routes.razor, MainLayout.razor, Dashboard.razor, DashboardHeader.razor, TimelineSection.razor, HeatmapGrid.razor) + 1 CSS file + 1 JSON data file.

---

### Component 1: `Program.cs` — Application Host

**Responsibility:** Configure and start the Kestrel web server with minimal middleware.

**Implementation:**

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
- `AddInteractiveServerComponents()` enables Blazor Server's SignalR-based rendering. Although this dashboard is static, the interactive server render mode is required for Blazor Server's component lifecycle.
- `DashboardDataService` is registered as **singleton** — data is loaded once and shared across all requests (which in practice means one browser tab).
- No authentication middleware, no CORS, no custom middleware pipeline.

**Dependencies:** ASP.NET Core framework (built-in), `DashboardDataService`.

---

### Component 2: `DashboardDataService` — Data Access Layer

**Responsibility:** Load `data.json` from disk, deserialize into strongly-typed records, compute derived values (NOW line position, current month detection), and expose data to components.

**Interface:**

```csharp
public class DashboardDataService
{
    public DashboardData? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool HasError => ErrorMessage is not null;

    // SVG coordinate helpers
    public double DateToX(DateOnly date);
    public double GetNowX();
    public double GetWorkstreamY(int index, int totalWorkstreams);
    public string GetCurrentMonthLabel();
}
```

**Behavior:**
1. Constructor receives `IWebHostEnvironment` to resolve `wwwroot/data/data.json` path.
2. On construction (or via an `Initialize()` called from `Program.cs`), reads and deserializes JSON.
3. If file is missing: sets `ErrorMessage = "Unable to load dashboard data. Please check data.json."`.
4. If JSON is malformed: catches `JsonException`, sets `ErrorMessage` with details.
5. If successful: populates `Data` property and pre-computes the timeline date range.

**Date-to-X coordinate calculation:**

```csharp
public double DateToX(DateOnly date)
{
    var rangeStart = Data!.TimelineRange.Start;
    var rangeEnd = Data!.TimelineRange.End;
    double totalDays = rangeEnd.DayNumber - rangeStart.DayNumber;
    double dayOffset = date.DayNumber - rangeStart.DayNumber;
    return (dayOffset / totalDays) * SvgWidth; // SvgWidth = 1560.0
}
```

**Workstream Y-position calculation:**

```csharp
public double GetWorkstreamY(int index, int totalWorkstreams)
{
    // Distribute evenly across 185px SVG height, offset from top
    double usableHeight = 185.0 - 28.0; // 28px reserved for month labels at top
    double spacing = usableHeight / (totalWorkstreams + 1);
    return 28.0 + spacing * (index + 1);
}
```

**Dependencies:** `IWebHostEnvironment` (built-in), `System.Text.Json` (built-in).

**Data:** Reads `wwwroot/data/data.json`. Read-only — never writes.

---

### Component 3: `DashboardData.cs` — Data Model Records

**Responsibility:** Define the strongly-typed schema for `data.json` deserialization.

See [Data Model](#data-model) section below for complete record definitions.

---

### Component 4: `Dashboard.razor` — Page Orchestrator

**Responsibility:** The single routable page (`@page "/"`). Injects `DashboardDataService`, handles the error/success branching, and distributes data to child components via `[Parameter]`.

**Behavior:**
- If `DashboardDataService.HasError` → renders a centered error message (red text on white, full viewport).
- If data loaded successfully → renders three child components in a flex column.

**Markup structure:**

```razor
@page "/"
@inject DashboardDataService DataService

@if (DataService.HasError)
{
    <div class="error-state">@DataService.ErrorMessage</div>
}
else
{
    <DashboardHeader Data="@DataService.Data!" />
    <TimelineSection Data="@DataService.Data!" DataService="@DataService" />
    <HeatmapGrid Data="@DataService.Data!" />
}
```

**Dependencies:** `DashboardDataService`, all three child components.

**Render mode:** `@rendermode InteractiveServer` (set at the `Routes.razor` or `App.razor` level).

---

### Component 5: `DashboardHeader.razor` — Header Section

**Responsibility:** Render the project title, subtitle, ADO backlog link, and milestone legend. Maps to the `.hdr` section of the reference design.

**Parameters:**

```csharp
[Parameter] public required DashboardData Data { get; set; }
```

**Markup produces:**
- `<div class="hdr">` container with flexbox row layout.
- Left side: `<h1>` with title text and inline `<a>` for backlog URL; `<div class="sub">` with subtitle.
- Right side: Four legend items using inline CSS shapes (rotated squares for diamonds, circles, vertical bars).

**No code-behind logic.** Pure data binding — all values from `Data.Title`, `Data.Subtitle`, `Data.BacklogUrl`.

---

### Component 6: `TimelineSection.razor` — SVG Timeline

**Responsibility:** Render the milestone timeline with workstream swim lanes, milestone markers, and NOW line. Maps to the `.tl-area` section of the reference design.

**Parameters:**

```csharp
[Parameter] public required DashboardData Data { get; set; }
[Parameter] public required DashboardDataService DataService { get; set; }
```

**Markup structure:**
1. **Left panel** (230px): `@foreach` over `Data.Workstreams` to render labels with workstream-specific colors.
2. **SVG canvas** (1560×185):
   - **Month gridlines:** `@foreach` over `Data.TimelineRange.MonthGridlines` → `<line>` + `<text>`.
   - **NOW line:** `<line>` at `DataService.GetNowX()` with dashed red stroke + "NOW" label.
   - **Workstream swim lanes:** `@foreach` over workstreams → horizontal `<line>` at computed Y.
   - **Milestones:** Nested `@foreach` over each workstream's milestones:
     - `MilestoneType.PoC` → `<polygon>` diamond, fill `#F4B400`, with `filter="url(#sh)"`.
     - `MilestoneType.Production` → `<polygon>` diamond, fill `#34A853`, with `filter="url(#sh)"`.
     - `MilestoneType.CheckpointMajor` → `<circle>` r=5, white fill, `#888` stroke.
     - `MilestoneType.CheckpointMinor` → `<circle>` r=4, filled `#999`.
     - `MilestoneType.Start` → `<circle>` r=7, white fill, workstream-colored stroke.
   - **Milestone labels:** `<text>` positioned above or below based on `Milestone.LabelPosition`.

**SVG diamond helper (in `@code` block):**

```csharp
private string DiamondPoints(double cx, double cy, double size = 11)
{
    return $"{cx},{cy - size} {cx + size},{cy} {cx},{cy + size} {cx - size},{cy}";
}
```

---

### Component 7: `HeatmapGrid.razor` — Status Heatmap

**Responsibility:** Render the monthly execution heatmap with four status rows and N month columns. Maps to the `.hm-wrap` section of the reference design.

**Parameters:**

```csharp
[Parameter] public required DashboardData Data { get; set; }
```

**Markup structure:**
1. **Title bar:** `<div class="hm-title">` with static text.
2. **CSS Grid container:** `<div class="hm-grid">` with dynamic `grid-template-columns` style.
3. **Header row:** Corner cell + `@foreach` month column headers. Current month gets `.current-hdr` class.
4. **Data rows:** `@foreach` over `Data.Heatmap.Rows` (4 rows: Shipped, InProgress, Carryover, Blockers):
   - Row header with category-specific CSS class (`.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr`).
   - `@foreach` cell in row → `<div class="hm-cell {categoryClass} {currentClass}">`:
     - If items exist: `@foreach` item → `<div class="it" title="@item">@item</div>`.
     - If no items and month is future: `<div class="it" style="color:#999">-</div>`.

**Dynamic grid columns (in `@code` block):**

```csharp
private string GridTemplateColumns =>
    $"160px repeat({Data.Heatmap.Months.Count}, 1fr)";
```

**Category-to-CSS mapping:**

```csharp
private static readonly Dictionary<string, (string CellClass, string HdrClass)> CategoryStyles = new()
{
    ["Shipped"]    = ("ship-cell", "ship-hdr"),
    ["InProgress"] = ("prog-cell", "prog-hdr"),
    ["Carryover"]  = ("carry-cell", "carry-hdr"),
    ["Blockers"]   = ("block-cell", "block-hdr"),
};
```

---

### Component 8: `MainLayout.razor` — Minimal Layout

**Responsibility:** Provide the outermost HTML layout with no navigation chrome.

```razor
@inherits LayoutComponentBase

@Body
```

No sidebar, no nav menu, no header bar. The `<body>` styling (1920×1080, overflow hidden) is applied via `dashboard.css`.

---

### Component 9: `dashboard.css` — Stylesheet

**Responsibility:** Define all visual styling. This file is a near-verbatim port of the `<style>` block from `OriginalDesignConcept.html`.

**Strategy:** Preserve the reference design's class names (`.hdr`, `.tl-area`, `.hm-wrap`, `.hm-grid`, `.ship-cell`, etc.) so the CSS and markup are directly cross-referenceable with the design.

**Additions beyond reference:**
- `.error-state` — Centered error message styling: `display:flex; align-items:center; justify-content:center; height:100vh; color:#EA4335; font-size:18px;`.
- `.current-hdr` — Dynamic replacement for the hardcoded `.apr-hdr` class: `background:#FFF0D0; color:#C07700;`.
- `.current-cell` — Dynamic replacement for the hardcoded `.apr` class: applies the darker current-month background per row category.

**Body override for Blazor:** The Blazor Server template injects its own `<div id="blazor-error-ui">` and scripts. Ensure `body` style overrides take precedence:

```css
html, body { margin: 0; padding: 0; }
body { width: 1920px; height: 1080px; overflow: hidden; background: #FFFFFF;
       font-family: 'Segoe UI', Arial, sans-serif; color: #111;
       display: flex; flex-direction: column; }
```

---

### Component 10: `data.json` — Configuration Data

**Responsibility:** Single source of truth for all dashboard content. Hand-edited by the project lead.

**Location:** `wwwroot/data/data.json` — served as a static file but primarily read server-side by `DashboardDataService`.

See [Data Model](#data-model) section for the complete JSON schema and a sample document.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│  wwwroot/data/data.json                                      │
│  (static JSON file on disk)                                  │
└──────────────┬──────────────────────────────────────────────┘
               │ File.ReadAllTextAsync() at startup
               ▼
┌─────────────────────────────────────────────────────────────┐
│  DashboardDataService  (Singleton)                           │
│  ┌─────────────────────────────────────────────────────┐     │
│  │ DashboardData? Data                                  │     │
│  │ string? ErrorMessage                                 │     │
│  │ DateToX(), GetNowX(), GetWorkstreamY()              │     │
│  └─────────────────────────────────────────────────────┘     │
└──────────────┬──────────────────────────────────────────────┘
               │ @inject (DI)
               ▼
┌─────────────────────────────────────────────────────────────┐
│  Dashboard.razor  (@page "/")                                │
│  ┌──────────┐  ┌──────────────────┐  ┌───────────────┐      │
│  │ if error  │  │ DashboardHeader  │  │ (error state) │      │
│  │ → show    │  │ [Parameter] Data │  │               │      │
│  │   error   │  └────────┬─────────┘  └───────────────┘      │
│  └──────────┘           │                                    │
│              ┌──────────┴──────────────────┐                 │
│              ▼                             ▼                  │
│  ┌──────────────────────┐   ┌──────────────────────┐         │
│  │ TimelineSection      │   │ HeatmapGrid          │         │
│  │ [Parameter] Data     │   │ [Parameter] Data     │         │
│  │ [Parameter] Service  │   │                      │         │
│  └──────────────────────┘   └──────────────────────┘         │
└─────────────────────────────────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────────────────┐
│  Browser (Chrome/Edge/Firefox @ 1920×1080)                   │
│  ← HTML + CSS + Blazor SignalR connection                    │
│  → Screenshot → PowerPoint                                   │
└─────────────────────────────────────────────────────────────┘
```

### Communication Patterns

| From | To | Mechanism | Data |
|------|----|-----------|------|
| `data.json` | `DashboardDataService` | `File.ReadAllTextAsync` + `JsonSerializer.Deserialize` | Raw JSON bytes |
| `DashboardDataService` | `Dashboard.razor` | `@inject` (ASP.NET Core DI) | `DashboardData` record graph |
| `Dashboard.razor` | Child components | `[Parameter]` binding | `DashboardData` (immutable records) |
| Blazor Server | Browser | SignalR WebSocket | Rendered DOM diffs |
| Browser | User | Visual render at 1920×1080 | Screenshot-ready dashboard |

### Lifecycle Sequence

1. `dotnet run` starts Kestrel on `https://localhost:5001`.
2. `DashboardDataService` constructor reads and deserializes `wwwroot/data/data.json`.
3. User opens browser to `https://localhost:5001`.
4. Blazor Server establishes SignalR connection, routes to `Dashboard.razor`.
5. `Dashboard.razor` checks `DataService.HasError`:
   - **Error path:** Renders `<div class="error-state">` with error message. Done.
   - **Success path:** Renders `DashboardHeader`, `TimelineSection`, `HeatmapGrid` with data parameters.
6. Components render HTML/SVG. CSS applies styling. Browser paints at 1920×1080.
7. User screenshots and pastes into PowerPoint.

---

## Data Model

### C# Record Types

All records live in a single file: `Models/DashboardData.cs`.

```csharp
using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record DashboardData(
    string Title,
    string Subtitle,
    string BacklogUrl,
    string ReportDate,
    TimelineRange TimelineRange,
    List<Workstream> Workstreams,
    HeatmapData Heatmap
);

public record TimelineRange(
    string Start,
    string End,
    List<MonthGridline> MonthGridlines
);

public record MonthGridline(
    string Label,
    string Date
);

public record Workstream(
    string Id,
    string Label,
    string Description,
    string Color,
    List<Milestone> Milestones
);

public record Milestone(
    string Date,
    string Label,
    string Type,
    string? LabelPosition
);

public record HeatmapData(
    string Title,
    List<string> Months,
    string CurrentMonth,
    List<HeatmapRow> Rows
);

public record HeatmapRow(
    string Category,
    string DisplayLabel,
    List<HeatmapCell> Cells
);

public record HeatmapCell(
    string Month,
    List<string> Items
);
```

**Design decisions:**

- **Strings for dates** (`"2026-01-01"`) rather than `DateOnly`: Simplifies JSON authoring for project leads who hand-edit `data.json`. The service parses these into `DateOnly` internally.
- **`LabelPosition`** on `Milestone`: Optional `"above"` or `"below"` — allows the JSON author to control label placement to avoid SVG text overlap.
- **`Type`** as string enum: Values are `"Start"`, `"PoC"`, `"Production"`, `"CheckpointMajor"`, `"CheckpointMinor"`. Parsed in the component's rendering logic.
- **Flat `Months` list** on `HeatmapData`: The month column order is explicit (e.g., `["Jan", "Feb", "Mar", "Apr"]`), with `CurrentMonth` marking which gets the highlight treatment.

### JSON Schema (`data.json`)

```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Engineering Platform · Phoenix Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "reportDate": "2026-04-17",
  "timelineRange": {
    "start": "2026-01-01",
    "end": "2026-06-30",
    "monthGridlines": [
      { "label": "Jan", "date": "2026-01-01" },
      { "label": "Feb", "date": "2026-02-01" },
      { "label": "Mar", "date": "2026-03-01" },
      { "label": "Apr", "date": "2026-04-01" },
      { "label": "May", "date": "2026-05-01" },
      { "label": "Jun", "date": "2026-06-01" }
    ]
  },
  "workstreams": [
    {
      "id": "M1",
      "label": "M1",
      "description": "Chatbot & MS Role",
      "color": "#0078D4",
      "milestones": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "Start", "labelPosition": "above" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "PoC", "labelPosition": "below" },
        { "date": "2026-05-01", "label": "May Prod (TBD)", "type": "Production", "labelPosition": "above" }
      ]
    },
    {
      "id": "M2",
      "label": "M2",
      "description": "PDS & Data Inventory",
      "color": "#00897B",
      "milestones": [
        { "date": "2025-12-19", "label": "Dec 19", "type": "Start", "labelPosition": "above" },
        { "date": "2026-02-11", "label": "Feb 11", "type": "CheckpointMajor", "labelPosition": "above" },
        { "date": "2026-03-10", "label": "", "type": "CheckpointMinor" },
        { "date": "2026-03-20", "label": "Mar 20 PoC", "type": "PoC", "labelPosition": "above" },
        { "date": "2026-04-15", "label": "Apr Prod", "type": "Production", "labelPosition": "below" }
      ]
    },
    {
      "id": "M3",
      "label": "M3",
      "description": "Auto Review DFD",
      "color": "#546E7A",
      "milestones": [
        { "date": "2026-02-03", "label": "Feb 3", "type": "Start", "labelPosition": "above" },
        { "date": "2026-04-25", "label": "Apr 25 PoC", "type": "PoC", "labelPosition": "below" },
        { "date": "2026-06-15", "label": "Jun Prod (TBD)", "type": "Production", "labelPosition": "above" }
      ]
    }
  ],
  "heatmap": {
    "title": "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers",
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonth": "Apr",
    "rows": [
      {
        "category": "Shipped",
        "displayLabel": "✅ SHIPPED",
        "cells": [
          { "month": "Jan", "items": ["Auth module v2", "Config pipeline"] },
          { "month": "Feb", "items": ["Data sync API", "Schema migration tool"] },
          { "month": "Mar", "items": ["Review workflow v1", "Dashboard prototype"] },
          { "month": "Apr", "items": ["PDS integration", "Role mapping engine"] }
        ]
      },
      {
        "category": "InProgress",
        "displayLabel": "🔄 IN PROGRESS",
        "cells": [
          { "month": "Jan", "items": ["Chatbot framework"] },
          { "month": "Feb", "items": ["Chatbot NLU training", "PDS connector"] },
          { "month": "Mar", "items": ["Data inventory scan", "Auto-review engine"] },
          { "month": "Apr", "items": ["DFD generator", "Chatbot v2 polish"] }
        ]
      },
      {
        "category": "Carryover",
        "displayLabel": "⚠️ CARRYOVER",
        "cells": [
          { "month": "Jan", "items": [] },
          { "month": "Feb", "items": ["Legacy API deprecation"] },
          { "month": "Mar", "items": ["Legacy API deprecation", "Perf benchmarks"] },
          { "month": "Apr", "items": ["Perf benchmarks"] }
        ]
      },
      {
        "category": "Blockers",
        "displayLabel": "🚫 BLOCKERS",
        "cells": [
          { "month": "Jan", "items": [] },
          { "month": "Feb", "items": [] },
          { "month": "Mar", "items": ["Compliance team review pending"] },
          { "month": "Apr", "items": ["Compliance sign-off delayed"] }
        ]
      }
    ]
  }
}
```

### Entity Relationships

```
DashboardData (root)
├── title, subtitle, backlogUrl, reportDate
├── TimelineRange
│   ├── start, end
│   └── MonthGridline[] (label, date)
├── Workstream[] (1..N)
│   ├── id, label, description, color
│   └── Milestone[] (0..N)
│       ├── date, label, type, labelPosition?
└── HeatmapData
    ├── title, months[], currentMonth
    └── HeatmapRow[] (exactly 4)
        ├── category, displayLabel
        └── HeatmapCell[] (one per month)
            └── items[] (0..N strings)
```

### Storage

- **Persistence:** Single `data.json` file at `wwwroot/data/data.json`.
- **Access pattern:** Read once at application startup. Immutable in memory thereafter.
- **Size constraint:** < 50KB (per NFR). Typical realistic dashboard data is 2-5KB.
- **No database.** No write operations. No caching layer (unnecessary for single-read).

---

## API Contracts

### HTTP Endpoints

This application has **no custom API endpoints**. It serves a single HTML page via Blazor Server's built-in routing.

| Endpoint | Method | Response | Notes |
|----------|--------|----------|-------|
| `/` | GET | HTML (Blazor Server page) | The dashboard. SignalR negotiation follows automatically. |
| `/_blazor` | WebSocket | Blazor Server SignalR hub | Managed by framework. Not custom. |
| `/css/dashboard.css` | GET | CSS static file | Served by `UseStaticFiles()`. |
| `/data/data.json` | GET | JSON static file | Served by `UseStaticFiles()`. Not consumed client-side — read server-side only. |

### Error Handling Contract

| Condition | Behavior |
|-----------|----------|
| `data.json` missing | Service sets `ErrorMessage`. Page renders: `<div class="error-state">Unable to load dashboard data. Please check data.json.</div>` |
| `data.json` malformed JSON | Service catches `JsonException`. Same error rendering with parse details in log. |
| `data.json` missing required field | `System.Text.Json` throws during deserialization. Caught, logged, error state rendered. |
| Blazor SignalR disconnected | Default Blazor reconnection UI appears (framework-managed). |

### Data Contract: `data.json`

The JSON schema serves as the contract between the project lead (data author) and the application. Required fields and their types:

| Field Path | Type | Required | Notes |
|------------|------|----------|-------|
| `title` | string | ✅ | H1 title text |
| `subtitle` | string | ✅ | Org · workstream · month subtitle |
| `backlogUrl` | string | ✅ | Full URL for ADO backlog link |
| `reportDate` | string (YYYY-MM-DD) | ✅ | Used for NOW line position |
| `timelineRange.start` | string (YYYY-MM-DD) | ✅ | Left edge of timeline |
| `timelineRange.end` | string (YYYY-MM-DD) | ✅ | Right edge of timeline |
| `timelineRange.monthGridlines[]` | array | ✅ | Month dividers with label + date |
| `workstreams[]` | array | ✅ | 1-5 workstreams |
| `workstreams[].milestones[]` | array | ✅ | 0+ milestones per workstream |
| `workstreams[].milestones[].type` | string | ✅ | `Start`, `PoC`, `Production`, `CheckpointMajor`, `CheckpointMinor` |
| `heatmap.months[]` | array | ✅ | Column labels, left to right |
| `heatmap.currentMonth` | string | ✅ | Must match one of `months[]` |
| `heatmap.rows[]` | array | ✅ | Exactly 4 rows |
| `heatmap.rows[].category` | string | ✅ | `Shipped`, `InProgress`, `Carryover`, `Blockers` |
| `heatmap.rows[].cells[]` | array | ✅ | One cell per month |

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|--------------|
| **Operating System** | Windows 10/11 (primary). macOS/Linux supported with Arial font fallback. |
| **Runtime** | .NET 8.0 SDK (LTS) |
| **Memory** | < 100MB (target per NFR) |
| **Disk** | < 10MB application footprint |
| **Network** | Localhost only. No outbound connections. |

### Hosting

| Aspect | Configuration |
|--------|--------------|
| **Server** | Kestrel (built-in, default) |
| **Binding** | `https://localhost:5001` and `http://localhost:5000` |
| **TLS** | .NET dev certificate (`dotnet dev-certs https --trust`) |
| **Process model** | Single process, in-process hosting |

### `launchSettings.json`

```json
{
  "profiles": {
    "ReportingDashboard": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Critical:** Only `localhost` binding — no `0.0.0.0`, no `*`, no external interface exposure.

### Storage

- **Application files:** Local filesystem, project directory.
- **Data file:** `wwwroot/data/data.json` — co-located with the application.
- **No database.** No connection strings. No migrations.

### CI/CD

**Out of scope for MVP.** The application is cloned and run locally via:

```bash
git clone <repo-url>
cd ReportingDashboard
dotnet run
```

Future enhancement: GitHub Actions workflow for build validation (`dotnet build` + `dotnet test`).

### Development Workflow

```bash
# First time setup
dotnet dev-certs https --trust

# Development (hot reload)
dotnet watch --project ReportingDashboard

# Production-like run
dotnet run --project ReportingDashboard -c Release
```

---

## Technology Stack Decisions

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Runtime** | .NET 8.0 LTS | 8.0.x | Mandatory stack. LTS support through Nov 2026. |
| **UI Framework** | Blazor Server | Built-in | Component model with strong typing. Server-side rendering means no WASM download delay. Enables future interactivity. |
| **CSS** | Pure CSS (Grid + Flexbox) | N/A | Reference design CSS is <100 lines. A framework would add conflict and complexity. |
| **SVG** | Inline SVG in Razor | N/A | Timeline needs pixel-precise diamond/circle/line placement. Inline SVG gives full control via Blazor data binding. |
| **JSON Parsing** | `System.Text.Json` | Built-in | Zero-dependency, high-performance. Supports records natively in .NET 8. |
| **DI Container** | ASP.NET Core DI | Built-in | Singleton registration for data service. No third-party container needed. |
| **Web Server** | Kestrel | Built-in | Default for ASP.NET Core. Minimal config for localhost. |
| **Fonts** | Segoe UI (system) | N/A | Ships with Windows. No web font hosting needed. |

### Technologies Explicitly Rejected

| Technology | Reason for Rejection |
|------------|---------------------|
| **MudBlazor / Radzen / Syncfusion** | CSS framework would conflict with the reference design's custom layout. Adds 500KB+ of CSS/JS. |
| **Chart.js / Plotly** | The SVG is ~30 lines of shapes. A charting library removes pixel control and adds JS dependencies. |
| **Entity Framework / SQLite** | No database needed. JSON file read once is infinitely simpler. |
| **Bootstrap / Tailwind** | Fixed 1920×1080 layout. Responsive utilities add zero value and override conflicts. |
| **SignalR (custom hubs)** | No real-time data. Blazor Server's built-in SignalR is sufficient for rendering. |
| **Blazor WebAssembly** | Requires WASM download. Server-side rendering is faster for this local-only, single-user scenario. |

---

## Security Considerations

### Network Security

| Control | Implementation |
|---------|---------------|
| **Localhost binding** | `launchSettings.json` restricts to `localhost` only. No `0.0.0.0` or wildcard binding. Prevents LAN/internet exposure. |
| **HTTPS** | Default Kestrel HTTPS with .NET dev certificate. Prevents local packet sniffing in shared environments. |
| **No external connections** | Application makes zero outbound HTTP calls. All data is local. |

### Authentication & Authorization

**Explicitly none.** This is a deliberate architectural decision:
- The tool runs locally on the developer's own machine.
- There is no multi-user access scenario.
- Adding auth would increase setup friction (contradicting Business Goal #4).
- The `localhost`-only binding provides sufficient access control.

### Data Protection

| Risk | Mitigation |
|------|------------|
| `data.json` contains project names | Add `wwwroot/data/data.json` to `.gitignore` if project names are sensitive. Ship a `data.sample.json` as a template. |
| No PII or credentials in data | By design — the data model contains only project status text and dates. |
| No write operations | Application is read-only. No mutation endpoints. No form submissions. |

### Input Validation

| Input Source | Validation |
|-------------|------------|
| `data.json` | Deserialized via `System.Text.Json` with strict typing. Malformed JSON throws `JsonException`, caught and surfaced as error state. |
| Browser requests | No user input accepted. No forms, no query parameters, no POST endpoints. |
| Blazor SignalR | Framework-managed. Antiforgery token middleware (`UseAntiforgery()`) enabled by default. |

### Dependency Supply Chain

- **Zero third-party NuGet packages.** The only dependency is `Microsoft.AspNetCore.App` (the framework itself).
- No npm packages. No client-side JavaScript libraries.
- Attack surface from third-party code: **none**.

---

## Scaling Strategy

### Current Scale

This application is designed for **exactly one user on one machine**. There is no scaling requirement.

| Dimension | Current | Future (if needed) |
|-----------|---------|-------------------|
| **Users** | 1 (local) | N/A — each user runs their own instance |
| **Data size** | < 50KB JSON | Sufficient for 12+ months, 10+ workstreams |
| **Concurrent requests** | 1 | Blazor Server supports hundreds per instance, but irrelevant here |

### Scaling Axes

**Horizontal (more users):** Not applicable. Each project lead runs their own local instance. No shared server. If a shared scenario were needed in the future, the same application could be deployed to an internal VM or App Service with no code changes — only a `launchSettings.json` update to bind to `0.0.0.0`.

**Vertical (more data):** The dashboard supports a configurable number of:
- **Workstreams** (1–5 recommended; SVG Y-positions auto-distribute via `GetWorkstreamY()`).
- **Heatmap months** (1–12; CSS Grid column count is dynamic: `repeat(N, 1fr)`).
- **Items per cell** (limited by the ~160px cell height at 4 rows in 1080px viewport; approximately 8–10 items before overflow clips).

**Data refresh:** Edit `data.json` → restart `dotnet run` → dashboard reflects changes. Total cycle time: ~5 seconds.

---

## Risks & Mitigations

### Technical Risks

| # | Risk | Probability | Impact | Mitigation |
|---|------|-------------|--------|------------|
| R1 | **SVG coordinate math produces misaligned milestones** | Medium | High (visual fidelity is the #1 requirement) | Isolate `DateToX()` and `GetWorkstreamY()` in `DashboardDataService` as pure functions. Verify with known reference values: e.g., Jan 1 → x=0, Jul 1 → x=1560, midpoint → x=780. Cross-reference with the reference SVG's hardcoded coordinates. |
| R2 | **CSS differences between reference HTML and Blazor rendering** | Medium | Medium | Blazor Server renders standard HTML — no shadow DOM, no web components. Port CSS class names verbatim from reference. Use browser DevTools element comparison. Test in Chrome, Edge, Firefox. |
| R3 | **Blazor Server injects unexpected DOM elements** | Low | Medium | The framework adds `<script src="_framework/blazor.server.js">` and error UI divs. Ensure `body` flex layout isn't disrupted. Set `overflow: hidden` on body. Hide `#blazor-error-ui` with `display: none` in CSS. |
| R4 | **data.json hand-editing errors break the dashboard** | High | Low (error state is graceful) | Wrap deserialization in try-catch. Display clear error message with guidance. Provide a well-documented `data.sample.json` as a template. Future: add `$schema` reference for VS Code IntelliSense. |
| R5 | **Heatmap content overflows cells when too many items** | Medium | Low (visual degradation, not failure) | CSS `overflow: hidden` on `.hm-cell` prevents layout breakage. `title` attribute on items provides tooltip access to clipped text. Document recommended max ~8 items per cell. |

### Operational Risks

| # | Risk | Probability | Impact | Mitigation |
|---|------|-------------|--------|------------|
| R6 | **.NET 8 SDK not installed on user's machine** | Medium | High (app won't run) | Document prerequisite in README. Provide `dotnet --version` check in a setup script. |
| R7 | **Browser not at 1920×1080 for screenshot** | High | Medium (screenshot doesn't fit PowerPoint) | Document the screenshot workflow: Chrome DevTools → Device Toolbar → 1920×1080 → "Capture full size screenshot". Provide step-by-step in README. |
| R8 | **Segoe UI not available (macOS/Linux)** | Low | Low (falls back to Arial) | Acceptable per PM spec. Primary target is Windows. Font stack includes Arial fallback. |

### Architectural Trade-off Log

| Decision | What We Gain | What We Lose | Acceptable? |
|----------|-------------|-------------|-------------|
| No database | Zero setup, zero config, instant startup | No history, no querying across reports | ✅ Yes — archive old `data.json` files manually |
| No CSS framework | Pixel-perfect control, zero CSS conflicts | Must write all styles manually | ✅ Yes — reference CSS is <100 lines |
| No charting library | Full SVG control, zero JS dependencies | Must hand-code coordinate math | ✅ Yes — SVG is simple shapes only |
| Blazor Server (not static HTML) | Strong typing, component reuse, future interactivity | Requires .NET runtime, SignalR overhead | ✅ Yes — overhead is negligible locally |
| Fixed 1920×1080 | Screenshot-perfect for PowerPoint | Unusable on smaller screens | ✅ Yes — this is the explicit design requirement |
| `System.Text.Json` strings for dates | Simple JSON editing for project leads | Must parse strings to `DateOnly` in service | ✅ Yes — parsing is trivial and centralized |

---

## UI Component Architecture

This section maps every visual section from the `OriginalDesignConcept.html` design to a specific Blazor component, its CSS strategy, data bindings, and interactions.

### Visual Section → Component Mapping

```
┌─────────────────────────────────────────────────────────────┐
│  HEADER (.hdr)                          ~46px fixed height  │
│  Component: DashboardHeader.razor                           │
│  ┌─────────────────────────┬───────────────────────────┐    │
│  │ Title (H1) + ADO Link   │   Legend (4 items)        │    │
│  │ Subtitle                 │   ◇PoC ◇Prod ●Chk |Now  │    │
│  └─────────────────────────┴───────────────────────────┘    │
├─────────────────────────────────────────────────────────────┤
│  TIMELINE (.tl-area)                   196px fixed height   │
│  Component: TimelineSection.razor                           │
│  ┌──────────┬──────────────────────────────────────────┐    │
│  │ Workstream│  SVG Canvas (1560×185)                   │    │
│  │ Labels    │  Month gridlines, swim lanes,            │    │
│  │ (230px)   │  milestone markers, NOW line             │    │
│  └──────────┴──────────────────────────────────────────┘    │
├─────────────────────────────────────────────────────────────┤
│  HEATMAP (.hm-wrap)                    flex:1 (remaining)   │
│  Component: HeatmapGrid.razor                               │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Title bar                                             │   │
│  │ ┌───────┬────────┬────────┬────────┬────────┐        │   │
│  │ │STATUS │  Jan   │  Feb   │  Mar   │ ◄Apr►  │        │   │
│  │ ├───────┼────────┼────────┼────────┼────────┤        │   │
│  │ │SHIPPED│ items  │ items  │ items  │ items  │        │   │
│  │ ├───────┼────────┼────────┼────────┼────────┤        │   │
│  │ │IN PROG│ items  │ items  │ items  │ items  │        │   │
│  │ ├───────┼────────┼────────┼────────┼────────┤        │   │
│  │ │CARRY  │ items  │ items  │ items  │ items  │        │   │
│  │ ├───────┼────────┼────────┼────────┼────────┤        │   │
│  │ │BLOCKERS│ items │ items  │ items  │ items  │        │   │
│  │ └───────┴────────┴────────┴────────┴────────┘        │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### Component Detail: `DashboardHeader.razor`

| Aspect | Specification |
|--------|--------------|
| **Design section** | `.hdr` (header bar, ~46px) |
| **CSS layout** | `display: flex; align-items: center; justify-content: space-between; padding: 12px 44px 10px;` |
| **CSS class** | `.hdr` (container), `.sub` (subtitle) |
| **Data bindings** | `Data.Title` → H1 text, `Data.BacklogUrl` → `<a href>`, `Data.Subtitle` → `.sub` div |
| **Interactions** | ADO backlog link: standard `<a>` navigation (no Blazor interception, opens in same tab) |
| **Legend rendering** | Four inline `<span>` elements with CSS shapes: rotated 12px squares (diamonds), 8px circle, 2×14px bar. All hardcoded — legend items are not data-driven. |

### Component Detail: `TimelineSection.razor`

| Aspect | Specification |
|--------|--------------|
| **Design section** | `.tl-area` (timeline, 196px fixed) |
| **CSS layout** | Outer: `display: flex; height: 196px; background: #FAFAFA;`. Left panel: `width: 230px; flex-direction: column; justify-content: space-around;`. Right: `flex: 1;` containing SVG. |
| **CSS classes** | `.tl-area`, `.tl-svg-box` |
| **Data bindings** | `Data.Workstreams[]` → left panel labels (id, description, color) + SVG swim lanes. `Data.Workstreams[].Milestones[]` → SVG markers. `Data.ReportDate` → NOW line X position. |
| **SVG structure** | `<svg width="1560" height="185">` with: `<defs><filter id="sh">` (drop shadow), `<line>` (gridlines, swim lanes, NOW), `<polygon>` (diamonds), `<circle>` (checkpoints/starts), `<text>` (labels). |
| **Coordinate logic** | `DateToX(date)` maps date to 0–1560px. `GetWorkstreamY(index, total)` distributes swim lanes vertically. Both in `DashboardDataService`. |
| **Interactions** | None. Pure visual rendering. |

### Component Detail: `HeatmapGrid.razor`

| Aspect | Specification |
|--------|--------------|
| **Design section** | `.hm-wrap` (heatmap, fills remaining viewport) |
| **CSS layout** | Outer: `flex: 1; min-height: 0; display: flex; flex-direction: column;`. Grid: `display: grid; grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr);` |
| **CSS classes** | `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.current-hdr`, `.hm-row-hdr`, `.hm-cell`, `.it`, `.ship-*`, `.prog-*`, `.carry-*`, `.block-*`, `.current-cell` |
| **Data bindings** | `Data.Heatmap.Months[]` → column headers. `Data.Heatmap.CurrentMonth` → `.current-hdr` / `.current-cell` class application. `Data.Heatmap.Rows[]` → row headers + cells. `HeatmapCell.Items[]` → `.it` divs within cells. |
| **Dynamic CSS** | `grid-template-columns` is computed: `160px repeat({monthCount}, 1fr)`. Current month class is applied conditionally: `class="hm-col-hdr @(month == Data.Heatmap.CurrentMonth ? "current-hdr" : "")"`. |
| **Interactions** | `title="@item"` on each `.it` div → native browser tooltip on hover for truncated text. |
| **Empty state** | If `cell.Items` is empty and month is after current: render `<div class="it" style="color:#999">-</div>`. |

### CSS Class Strategy

The CSS file preserves the reference design's class naming convention exactly. This allows engineers to visually diff the reference HTML against the Blazor-rendered output using browser DevTools.

**Static classes** (always applied): `.hdr`, `.tl-area`, `.tl-svg-box`, `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`, `.it`.

**Category classes** (applied per row): `.ship-hdr`/`.ship-cell`, `.prog-hdr`/`.prog-cell`, `.carry-hdr`/`.carry-cell`, `.block-hdr`/`.block-cell`.

**Dynamic classes** (applied conditionally in Razor):
- `.current-hdr` — replaces the reference's hardcoded `.apr-hdr`. Applied when `month == Data.Heatmap.CurrentMonth`.
- `.current-cell` — replaces the reference's hardcoded `.apr`. Applied to data cells in the current month column. CSS rule varies per category row (uses compound selectors: `.ship-cell.current-cell`, `.prog-cell.current-cell`, etc.).