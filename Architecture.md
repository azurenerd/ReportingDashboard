# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server application that renders a pixel-perfect 1920×1080 project status visualization from a local `data.json` file. It is optimized for browser screenshots destined for PowerPoint executive decks.

**Primary Goals:**
1. Render a visually exact replica of `OriginalDesignConcept.html` using Blazor Server components
2. Drive all content from a single `data.json` file — zero code changes required for data updates
3. Achieve zero-dependency, zero-cost, zero-ops local execution via `dotnet run`
4. Produce screenshot-ready output at 1920×1080 with no scrollbars, overlays, or artifacts

**Architectural Principles:**
- **Read-only, unidirectional data flow:** JSON file → Service → Page → Child Components via `[Parameter]`
- **One component per visual section:** Direct mapping between design sections and `.razor` files
- **CSS class parity:** Dashboard CSS classes match the reference HTML identically for visual verification
- **Zero external dependencies:** Only built-in .NET 8 SDK libraries; no NuGet packages

---

## System Components

### 1. `Program.cs` — Application Entry Point

**Responsibilities:**
- Configure Kestrel to listen on `http://localhost:5000` (HTTP only, no HTTPS)
- Register `DashboardDataService` as a singleton in the DI container
- Map Blazor Server endpoints and static files
- Disable HTTPS redirection and HSTS

**Interfaces:** None (bootstrap only)

**Dependencies:** `DashboardDataService`, Blazor Server middleware

**Key Configuration:**
```csharp
builder.WebHost.UseUrls("http://localhost:5000");
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();
```

---

### 2. `Services/DashboardDataService.cs` — Data Loading Service

**Responsibilities:**
- Read and deserialize `data.json` from the project root directory on first access
- Provide the deserialized `DashboardData` model to consuming components
- Handle missing/malformed JSON gracefully by populating an error message instead of throwing
- Resolve `nowDate` (use JSON value if present, else `DateTime.Now`)
- Resolve `currentMonth` (use JSON value if present, else derive from `nowDate`)

**Interfaces:**
```csharp
public class DashboardDataService
{
    public DashboardData? Data { get; }
    public string? ErrorMessage { get; }
    public bool HasError => ErrorMessage is not null;
    public DateTime NowDate { get; }
    public string CurrentMonth { get; }
}
```

**Dependencies:** `System.Text.Json`, `System.IO`

**Data:** Reads `data.json` from `AppContext.BaseDirectory` or the content root path. The file is NOT in `wwwroot` — it is server-side only and not exposed via HTTP.

**Error Handling Strategy:**
| Condition | Behavior |
|-----------|----------|
| `data.json` missing | `ErrorMessage = "data.json not found. Place it in the application root directory."` |
| Invalid JSON syntax | `ErrorMessage = "data.json contains invalid JSON: {details}"` |
| Schema mismatch | `ErrorMessage = "data.json structure is invalid: {details}"` |
| Valid JSON | `Data` populated, `ErrorMessage` is null |

**Lifecycle:** Singleton — loaded once at first injection. Data persists for the application lifetime. Restart `dotnet run` to reload.

---

### 3. `Models/` — Data Model Classes

**Responsibilities:** Strongly-typed C# representations of the `data.json` schema. Used for JSON deserialization and component parameter binding.

**Classes:**

#### `DashboardData.cs` (Root)
```csharp
public class DashboardData
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string BacklogUrl { get; set; } = "";
    public TimelineData Timeline { get; set; } = new();
    public HeatmapData Heatmap { get; set; } = new();
}
```

#### `TimelineData.cs`
```csharp
public class TimelineData
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? NowDate { get; set; }
    public List<Track> Tracks { get; set; } = new();
}
```

#### `Track.cs`
```csharp
public class Track
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Color { get; set; } = "#0078D4";
    public List<Milestone> Milestones { get; set; } = new();
}
```

#### `Milestone.cs`
```csharp
public class Milestone
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = "";
    public MilestoneType Type { get; set; }
}

public enum MilestoneType
{
    Checkpoint,
    PoC,
    Production
}
```

#### `HeatmapData.cs`
```csharp
public class HeatmapData
{
    public List<string> Months { get; set; } = new();
    public string? CurrentMonth { get; set; }
    public List<StatusCategory> Categories { get; set; } = new();
}
```

#### `StatusCategory.cs`
```csharp
public class StatusCategory
{
    public string Name { get; set; } = "";
    public string CssClass { get; set; } = "";
    public string Icon { get; set; } = "";
    public Dictionary<string, List<string>> Items { get; set; } = new();
}
```

---

### 4. `Components/Layout/DashboardLayout.razor` — Minimal Layout

**Responsibilities:**
- Provide the outermost HTML structure for the page
- Reference `dashboard.css`
- Suppress Blazor's default navigation sidebar and top bar
- Hide the Blazor reconnection modal via CSS

**Interfaces:** Standard Blazor `LayoutComponentBase` with `@Body` render fragment

**Dependencies:** `wwwroot/css/dashboard.css`

**Key Markup:**
```razor
@inherits LayoutComponentBase

<div class="page">
    @Body
</div>
```

No `<NavMenu>`, no `<NavLink>`, no sidebar. The layout is a transparent passthrough to the page content.

---

### 5. `Components/Pages/Dashboard.razor` — Main Page (Route `/`)

**Responsibilities:**
- Serve as the single routable page at `@page "/"`
- Inject `DashboardDataService` and check for errors
- If error: render a centered error message
- If success: render `Header`, `Timeline`, and `HeatmapGrid` components, passing data via `[Parameter]`

**Interfaces:**
```razor
@page "/"
@inject DashboardDataService DataService
```

**Dependencies:** `DashboardDataService`, `Header`, `Timeline`, `HeatmapGrid`

**Data Flow:**
```
DashboardDataService.Data
  ├── → Header [Title, Subtitle, BacklogUrl]
  ├── → Timeline [TimelineData, NowDate]
  └── → HeatmapGrid [HeatmapData, CurrentMonth]
```

**Error State Rendering:**
```html
<div style="display:flex;align-items:center;justify-content:center;
            width:1920px;height:1080px;font-size:18px;color:#666;">
    Unable to load dashboard data. Please check data.json.
</div>
```

---

### 6. `Components/Header.razor` — Header Bar

**Responsibilities:**
- Render the project title as an `<h1>` (24px, bold)
- Render the backlog URL as a blue link adjacent to the title
- Render the subtitle in 12px gray text
- Render the legend bar (PoC diamond, Production diamond, Checkpoint circle, Now line)

**Parameters:**
```csharp
[Parameter] public string Title { get; set; }
[Parameter] public string Subtitle { get; set; }
[Parameter] public string BacklogUrl { get; set; }
```

**CSS Classes:** `.hdr`, `.sub` — matches reference HTML exactly

---

### 7. `Components/Timeline.razor` — SVG Timeline Visualization

**Responsibilities:**
- Render an SVG element (width 1560px, height 185px) inside the `.tl-area` container
- Render the left panel (230px) with track labels (ID, name, color)
- Compute X positions from dates: `x = (date - startDate).TotalDays / (endDate - startDate).TotalDays * svgWidth`
- Render vertical month grid lines at each month boundary
- Render month abbreviation labels (Jan, Feb, Mar, etc.)
- Render horizontal track lines (3px stroke, track color)
- Render milestone markers by type:
  - **Checkpoint:** White-filled circle with track-colored stroke
  - **PoC:** Gold (`#F4B400`) diamond polygon with drop shadow
  - **Production:** Green (`#34A853`) diamond polygon with drop shadow
  - **Small dot:** Solid `#999` fill, 4px radius
- Render date labels above/below markers
- Render dashed red NOW line at the `NowDate` X position
- Add `<title>` elements to milestone markers for hover tooltips

**Parameters:**
```csharp
[Parameter] public TimelineData Timeline { get; set; }
[Parameter] public DateTime NowDate { get; set; }
```

**Key Computations:**
```csharp
private double DateToX(DateTime date)
{
    double totalDays = (Timeline.EndDate - Timeline.StartDate).TotalDays;
    double elapsed = (date - Timeline.StartDate).TotalDays;
    return Math.Clamp(elapsed / totalDays * SvgWidth, 0, SvgWidth);
}

private double TrackY(int trackIndex, int totalTracks)
{
    double spacing = (SvgHeight - TopMargin) / (totalTracks + 1);
    return TopMargin + spacing * (trackIndex + 1);
}

private string DiamondPoints(double cx, double cy, double size = 11)
{
    return $"{cx},{cy - size} {cx + size},{cy} {cx},{cy + size} {cx - size},{cy}";
}
```

**SVG Filter Definition:**
```svg
<defs>
    <filter id="sh">
        <feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>
    </filter>
</defs>
```

**CSS Classes:** `.tl-area`, `.tl-svg-box` — matches reference HTML

**Rendering Note:** SVG elements are rendered directly in Razor markup. No `MarkupString` is needed because Blazor natively supports SVG elements in `.razor` files. All coordinate values are computed in C# and interpolated into the markup.

---

### 8. `Components/HeatmapGrid.razor` — Status Heatmap Container

**Responsibilities:**
- Render the section title "Monthly Execution Heatmap" in uppercase gray
- Render the CSS Grid container with dynamic column count based on months
- Render the header row: corner cell ("STATUS") + month column headers
- Highlight the current month header with `.apr-hdr` styling (gold background, amber text, "▸ Now" suffix)
- Render one `HeatmapRow` per status category
- Dynamically set `grid-template-columns` based on the number of months: `160px repeat(N, 1fr)`

**Parameters:**
```csharp
[Parameter] public HeatmapData Heatmap { get; set; }
[Parameter] public string CurrentMonth { get; set; }
```

**CSS Classes:** `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.apr-hdr`

**Grid Template Rows:** `36px repeat(C, 1fr)` where C = number of categories (typically 4)

---

### 9. `Components/HeatmapRow.razor` — Single Status Row

**Responsibilities:**
- Render the row header cell with status name, icon, and row-specific color styling
- Render one `HeatmapCell` per month
- Apply current-month highlighting to the appropriate cell

**Parameters:**
```csharp
[Parameter] public StatusCategory Category { get; set; }
[Parameter] public List<string> Months { get; set; }
[Parameter] public string CurrentMonth { get; set; }
```

**CSS Classes:** `.hm-row-hdr`, `.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr`

---

### 10. `Components/HeatmapCell.razor` — Individual Month×Status Cell

**Responsibilities:**
- Render bullet-pointed work items with colored dot prefix
- If no items exist for the cell, render a gray dash `—`
- Apply current-month background tint when applicable

**Parameters:**
```csharp
[Parameter] public List<string> Items { get; set; }
[Parameter] public string CssClass { get; set; }
[Parameter] public bool IsCurrentMonth { get; set; }
```

**CSS Classes:** `.hm-cell`, `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`, `.apr`, `.it`

**Item Rendering:**
```razor
@if (Items.Any())
{
    @foreach (var item in Items)
    {
        <div class="it">@item</div>
    }
}
else
{
    <div style="color:#999;text-align:center;">—</div>
}
```

---

### 11. `wwwroot/css/dashboard.css` — Stylesheet

**Responsibilities:**
- Port all CSS from `OriginalDesignConcept.html` `<style>` block verbatim
- Add Blazor-specific overrides:
  - `.components-reconnect-modal { display: none !important; }` — hide reconnection overlay
  - `#blazor-error-ui { display: none !important; }` — hide error overlay
- Maintain identical class names to the reference HTML for side-by-side verification

**Size Estimate:** ~120 lines of CSS

---

### 12. `App.razor` — Blazor Root Component

**Responsibilities:**
- Define the `<html>`, `<head>`, and `<body>` structure
- Reference `dashboard.css` in `<head>`
- Render the Blazor `<Routes>` component
- Include the Blazor Server script tag

---

### 13. `data.json` — Dashboard Configuration File

**Responsibilities:**
- Serve as the single source of truth for all dashboard content
- Ship with fictional demo data ("Project Phoenix") for immediate evaluation
- Reside in the project root directory (NOT `wwwroot`) so it is not HTTP-accessible

---

## Component Interactions

### Data Flow Diagram

```
┌──────────────────────────────────────────────────────────┐
│                     data.json (file)                      │
│            Project root, read at startup                  │
└──────────────────────┬───────────────────────────────────┘
                       │ File.ReadAllText + JsonSerializer.Deserialize<DashboardData>
                       ▼
┌──────────────────────────────────────────────────────────┐
│             DashboardDataService (Singleton)               │
│  • DashboardData? Data                                    │
│  • string? ErrorMessage                                   │
│  • DateTime NowDate                                       │
│  • string CurrentMonth                                    │
└──────────────────────┬───────────────────────────────────┘
                       │ @inject (DI)
                       ▼
┌──────────────────────────────────────────────────────────┐
│              Dashboard.razor (Page, route "/")             │
│  Checks HasError → renders error or components            │
├──────────────┬──────────────────┬────────────────────────┤
│  [Parameter] │   [Parameter]    │     [Parameter]         │
│      ▼       │       ▼          │          ▼              │
│  Header.razor│  Timeline.razor  │  HeatmapGrid.razor      │
│  • Title     │  • TimelineData  │  • HeatmapData          │
│  • Subtitle  │  • NowDate       │  • CurrentMonth         │
│  • BacklogUrl│                  │         │                │
│              │                  │    ┌────┴────┐           │
│              │                  │    ▼         ▼           │
│              │                  │ HeatmapRow  (×4)         │
│              │                  │    │                     │
│              │                  │    ▼                     │
│              │                  │ HeatmapCell (×months)    │
└──────────────┴──────────────────┴────────────────────────┘
```

### Communication Pattern

All communication is **top-down, read-only parameter passing**:

1. **Startup:** `Program.cs` registers `DashboardDataService` as singleton. The service reads `data.json` in its constructor.
2. **Page Load:** Browser navigates to `/`. Blazor renders `Dashboard.razor`. The component receives `DashboardDataService` via `@inject`.
3. **Render Tree:** `Dashboard.razor` passes data slices to child components via `[Parameter]` properties. No callbacks, no events, no two-way binding.
4. **No Re-renders:** After initial render, no component state changes. The page is static once loaded.

### Request/Response Lifecycle

```
Browser GET http://localhost:5000/
  → Kestrel receives request
  → Blazor Server establishes SignalR WebSocket connection
  → Server renders Dashboard.razor component tree
  → HTML streamed to browser via SignalR
  → Browser renders DOM (single paint, no progressive loading)
  → Page visible in <2 seconds
```

---

## Data Model

### `data.json` Schema (Canonical)

```json
{
  "title": "string — Dashboard heading text",
  "subtitle": "string — Organization / workstream / date subheading",
  "backlogUrl": "string — URL to Azure DevOps backlog (or empty)",
  "timeline": {
    "startDate": "ISO 8601 date (YYYY-MM-DD) — left edge of timeline",
    "endDate": "ISO 8601 date (YYYY-MM-DD) — right edge of timeline",
    "nowDate": "ISO 8601 date (optional) — override for NOW line position",
    "tracks": [
      {
        "id": "string — short track identifier (e.g., 'M1')",
        "name": "string — track display name",
        "color": "string — CSS hex color for the track line",
        "milestones": [
          {
            "date": "ISO 8601 date — milestone position",
            "label": "string — text displayed near marker",
            "type": "Checkpoint | PoC | Production"
          }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["string array — ordered month names for column headers"],
    "currentMonth": "string (optional) — month name to highlight; auto-detected if omitted",
    "categories": [
      {
        "name": "string — display name (e.g., 'Shipped')",
        "cssClass": "string — CSS prefix (e.g., 'ship', 'prog', 'carry', 'block')",
        "icon": "string — emoji prefix for row header (e.g., '✅')",
        "items": {
          "MonthName": ["string array — work item descriptions for this month"]
        }
      }
    ]
  }
}
```

### Entity Relationships

```
DashboardData (1)
  ├── TimelineData (1)
  │     └── Track (1..5)
  │           └── Milestone (0..n)
  └── HeatmapData (1)
        └── StatusCategory (4: Shipped, InProgress, Carryover, Blockers)
              └── Items[Month] → string[] (0..n per month)
```

### Storage

- **Format:** Single JSON file (`data.json`)
- **Location:** Project root directory (e.g., `ReportingDashboard/data.json`)
- **Access Pattern:** Read-once at application startup
- **Persistence:** File system only. No database. No cache.
- **Security:** Not served via HTTP. Located outside `wwwroot`.

---

## API Contracts

This application has **no REST API, no Web API controllers, and no HTTP endpoints** beyond the Blazor Server page itself. All data flows internally from file → service → components.

### Blazor Server Endpoints (Framework-Provided)

| Endpoint | Purpose | Notes |
|----------|---------|-------|
| `GET /` | Serves the Blazor application shell | Returns HTML with SignalR bootstrap |
| `/_blazor` | SignalR WebSocket hub | Handles component rendering and DOM diffing |
| `/_framework/blazor.server.js` | Blazor client-side runtime | Auto-served by framework |
| `/css/dashboard.css` | Dashboard stylesheet | Static file from `wwwroot/css/` |

### Internal Service Contract

```csharp
// DashboardDataService — injected into Dashboard.razor
public class DashboardDataService
{
    // Populated on construction; null if loading failed
    public DashboardData? Data { get; }

    // Non-null if data.json is missing, malformed, or schema-invalid
    public string? ErrorMessage { get; }

    // Convenience property
    public bool HasError => ErrorMessage is not null;

    // Resolved NOW date: from JSON override or DateTime.Now
    public DateTime NowDate { get; }

    // Resolved current month name: from JSON override or derived from NowDate
    public string CurrentMonth { get; }
}
```

### Error Handling Contract

| Error Condition | User-Visible Behavior |
|----------------|----------------------|
| `data.json` not found | Centered message: "Unable to load dashboard data. Please check that data.json exists in the application directory." |
| JSON parse failure | Centered message: "Unable to load dashboard data. data.json contains invalid JSON." |
| Schema validation failure | Centered message: "Unable to load dashboard data. data.json structure is not recognized." |
| Empty tracks/categories | Renders normally with empty sections (no crash) |
| Future month with no items | Gray dash `—` in the heatmap cell |

---

## Infrastructure Requirements

### Runtime Requirements

| Requirement | Specification |
|------------|---------------|
| Operating System | Windows 10/11 (developer workstation) |
| .NET SDK | 8.0.x LTS (any patch version) |
| Browser | Microsoft Edge or Google Chrome (latest stable) |
| Display Resolution | 1920×1080 for screenshot capture |
| Network | None required — fully offline-capable |
| Disk Space | < 10 MB for the entire solution |

### Hosting

| Aspect | Configuration |
|--------|--------------|
| Web Server | Kestrel (built into .NET 8) |
| Protocol | HTTP only (no HTTPS, no TLS) |
| Binding | `http://localhost:5000` |
| Process | `dotnet run` from project directory |
| Reverse Proxy | None |
| Container | None |

### Build & Run Commands

```bash
# Development
cd ReportingDashboard
dotnet run
# → Listening on http://localhost:5000

# Development with hot-reload
dotnet watch run

# Production build
dotnet publish -c Release -o ./publish

# Self-contained build (no SDK required on target)
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
```

### CI/CD

Not required. Manual build and run workflow. If desired in the future:

```bash
dotnet build --no-restore
dotnet run --no-build
```

### Project Structure on Disk

```
ReportingDashboard/
├── ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── App.razor
    ├── _Imports.razor
    ├── data.json
    ├── wwwroot/
    │   └── css/
    │       └── dashboard.css
    ├── Models/
    │   ├── DashboardData.cs
    │   ├── TimelineData.cs
    │   ├── Track.cs
    │   ├── Milestone.cs
    │   ├── HeatmapData.cs
    │   └── StatusCategory.cs
    ├── Services/
    │   └── DashboardDataService.cs
    └── Components/
        ├── Pages/
        │   └── Dashboard.razor
        ├── Layout/
        │   └── DashboardLayout.razor
        ├── Header.razor
        ├── Timeline.razor
        ├── HeatmapGrid.razor
        ├── HeatmapRow.razor
        └── HeatmapCell.razor
```

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component.

### Section → Component Mapping

| Visual Section (HTML) | Blazor Component | CSS Classes | Layout Strategy |
|----------------------|-----------------|-------------|-----------------|
| `.hdr` (header bar) | `Header.razor` | `.hdr`, `.sub` | Flexbox: `justify-content: space-between; align-items: center` |
| `.hdr` right legend | Inline in `Header.razor` | Inline styles matching reference | Flexbox: `gap: 22px; align-items: center` |
| `.tl-area` (timeline container) | `Timeline.razor` | `.tl-area`, `.tl-svg-box` | Flexbox: fixed height 196px, two children (label panel + SVG) |
| Timeline left panel (track labels) | Inline in `Timeline.razor` | Inline styles | Flexbox column: `justify-content: space-around`, width 230px |
| Timeline SVG (milestones) | Inline SVG in `Timeline.razor` | None (SVG attributes) | SVG coordinate system: 1560×185, computed positions |
| `.hm-wrap` (heatmap section) | `HeatmapGrid.razor` | `.hm-wrap`, `.hm-title`, `.hm-grid` | CSS Grid: `160px repeat(N, 1fr)` columns, `36px repeat(4, 1fr)` rows |
| `.hm-corner` + `.hm-col-hdr` | Inline in `HeatmapGrid.razor` | `.hm-corner`, `.hm-col-hdr`, `.apr-hdr` | Grid cells, flex centering |
| `.hm-row-hdr` + row cells | `HeatmapRow.razor` | `.hm-row-hdr`, `.[prefix]-hdr` | Grid cells spanning one row |
| Individual data cells | `HeatmapCell.razor` | `.hm-cell`, `.[prefix]-cell`, `.apr` | Block layout with `.it` items |

### Component Data Bindings

#### `Header.razor`
```razor
<div class="hdr">
    <div>
        <h1>@Title <a href="@BacklogUrl">→ ADO Backlog</a></h1>
        <div class="sub">@Subtitle</div>
    </div>
    <div><!-- Legend items with inline CSS shapes --></div>
</div>
```

#### `Timeline.razor`
```razor
<div class="tl-area">
    <!-- Left panel: @foreach track in Timeline.Tracks -->
    <div class="tl-svg-box">
        <svg width="1560" height="185">
            <!-- Month grid lines: computed from StartDate/EndDate -->
            <!-- Track lines: @foreach with computed Y positions -->
            <!-- Milestones: switch on Type for circle/diamond -->
            <!-- NOW line: DateToX(NowDate) -->
        </svg>
    </div>
</div>
```

#### `HeatmapGrid.razor`
```razor
<div class="hm-wrap">
    <div class="hm-title">Monthly Execution Heatmap</div>
    <div class="hm-grid" style="grid-template-columns: 160px repeat(@MonthCount, 1fr);">
        <!-- Corner cell -->
        <!-- Month headers: @foreach month, apply .apr-hdr if current -->
        <!-- Category rows: @foreach category → <HeatmapRow> -->
    </div>
</div>
```

#### `HeatmapCell.razor`
```razor
<div class="hm-cell @CssClass @(IsCurrentMonth ? "apr" : "")">
    @if (Items.Any())
    {
        @foreach (var item in Items)
        {
            <div class="it">@item</div>
        }
    }
    else
    {
        <div style="color:#999;text-align:center;">—</div>
    }
</div>
```

### Component Interaction Patterns

- **No user-initiated events** — The dashboard is read-only. No click handlers, no form inputs, no state mutations.
- **No JavaScript interop** — All rendering is pure server-side HTML/CSS/SVG.
- **SVG tooltips** — `<title>` child elements on milestone markers provide native browser tooltips on hover. No custom tooltip component.
- **CSS class toggling** — Current month highlighting is achieved by conditionally adding CSS classes (e.g., `.apr`) based on `CurrentMonth` comparison.

---

## Technology Stack Decisions

| Layer | Choice | Justification |
|-------|--------|---------------|
| **Runtime** | .NET 8 LTS | Mandated by requirements. LTS ensures support through Nov 2026. |
| **Web Framework** | Blazor Server | Simplest hosting model for local-only use. No WASM download overhead. Server-side rendering ensures consistent output. |
| **Project Template** | `blazorserver-empty` | Avoids Bootstrap, jQuery, and sample pages from the default template. Clean starting point. |
| **JSON Library** | `System.Text.Json` | Built into .NET 8. Zero external dependencies. Sufficient for flat JSON deserialization. |
| **CSS Approach** | Hand-written CSS matching reference | The reference HTML provides complete CSS. Copying it verbatim ensures pixel-perfect parity. No CSS framework conflicts. |
| **SVG Generation** | Inline SVG in Razor | Blazor natively renders SVG elements. No charting library needed for lines, circles, diamonds, and text. Full control over coordinates. |
| **Data Storage** | Local `data.json` file | Mandated by requirements. Simplest possible data source. Editable in any text editor. |
| **State Management** | None (read-only) | Unidirectional data flow via `[Parameter]`. No Flux, no Redux, no cascading values needed. |
| **Authentication** | None | Explicitly out of scope. Local-only tool. |
| **External NuGet Packages** | Zero | All capabilities (HTTP, HTML, JSON, CSS, SVG) are built into .NET 8 and the browser. |

### Rejected Alternatives

| Alternative | Reason for Rejection |
|------------|---------------------|
| MudBlazor / Radzen | Adds hundreds of KB of CSS/JS. Would conflict with custom CSS. Overkill for this scope. |
| Chart.js / Plotly | Requires JS interop. Timeline is simple geometry, not statistical charts. Async rendering breaks screenshot fidelity. |
| Blazor WebAssembly | WASM download adds startup latency. No benefit for a local-only, single-user app. |
| Entity Framework | No database exists. Reading a flat JSON file does not warrant an ORM. |
| Bootstrap / Tailwind | The reference design has its own CSS. A framework creates class name conflicts and overrides. |

---

## Security Considerations

### Threat Model

This is a **zero-attack-surface application**:
- Runs on `localhost` only — not network-accessible
- No authentication — no credentials to steal
- No user input — no forms, no query parameters processed
- No database — no injection vectors
- No external API calls — no SSRF risk

### Mitigations

| Concern | Mitigation |
|---------|-----------|
| **XSS** | Blazor auto-encodes all rendered strings by default. The `@` syntax in Razor escapes HTML entities. |
| **MarkupString usage** | Not required. SVG elements are rendered directly in Razor markup, not via raw HTML strings. If ever needed for SVG, only developer-controlled `data.json` content is rendered — never user-submitted input. |
| **data.json exposure** | File is stored in the project root, NOT in `wwwroot`. It is not served as a static file and cannot be accessed via HTTP. |
| **Dependency supply chain** | Zero external NuGet packages = zero supply chain risk. |
| **HTTPS** | Not required. `localhost` traffic never leaves the machine. |
| **SignalR** | Default Blazor Server configuration. No custom hub. No sensitive data transmitted. |

---

## Scaling Strategy

### Current Scale Target

| Metric | Target |
|--------|--------|
| Concurrent users | 1 (single developer/PM on localhost) |
| Data size | ~5-50 KB JSON file (3-5 tracks, 4 months, ~50 items) |
| DOM elements | ~100-200 |
| Render time | < 50ms server-side, < 2 seconds browser paint |
| Memory | < 50 MB application process |

### Scaling is Not Required

This is a local-only, single-user tool. There are no scaling concerns. The architecture deliberately avoids any infrastructure that would imply multi-user or distributed deployment.

### If Scope Grows (Future Considerations)

| Scenario | Approach |
|----------|---------|
| More than 5 tracks | Increase `.tl-area` height; accept >1080px or add vertical scrolling |
| More than 4 months | Adjust `grid-template-columns` dynamically; the component already computes column count from data |
| Multiple projects | Swap `data.json` files; or extend the schema to support a `projects[]` array with tabs |
| Team access | Deploy to an internal web server; add a read-only authentication layer |
| Data from APIs | Replace `DashboardDataService` file read with an HTTP client; keep the same model classes |

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|------------|
| 1 | **Blazor reconnection modal appears in screenshots** | Medium | Medium | CSS rule `.components-reconnect-modal { display: none !important; }` in `dashboard.css`. Also hide `#blazor-error-ui`. |
| 2 | **SVG text/position rendering varies between Edge and Chrome** | Low | Low | Use explicit `x`, `y`, `text-anchor` attributes. Test in both browsers. Avoid `dominant-baseline` (inconsistent). |
| 3 | **data.json schema drift breaks rendering** | Low | Medium | Defensive deserialization with `try/catch`. Default values on all model properties. Clear error message on failure. |
| 4 | **Heatmap items overflow cell height at 1080px** | Medium | Medium | Limit items per cell to ~4-5 in `data.json`. Add `overflow: hidden` on cells (already in CSS). Document the constraint. |
| 5 | **Developer forgets to place data.json in correct directory** | Medium | Low | `DashboardDataService` checks multiple paths: content root, base directory, working directory. Clear error message with expected path. |
| 6 | **Segoe UI unavailable on non-Windows** | Low | Low | Requirements specify Windows-only. CSS fallback to Arial is already in the font stack. |
| 7 | **dotnet watch causes layout flicker during development** | Low | Low | Use `dotnet run` (not `watch`) for final screenshot capture. Flicker only affects dev workflow. |
| 8 | **Timeline date calculations off by timezone** | Low | Medium | Use `DateTime` (not `DateTimeOffset`). Parse dates as date-only (no time component). All comparisons are date-level. |
| 9 | **CSS Grid not rendering identically to reference HTML** | Low | High | Copy CSS verbatim from reference. Use identical class names. Side-by-side browser comparison during development. |
| 10 | **Project template includes unwanted defaults** | Low | Low | Use `blazorserver-empty` template. Strip any remaining default content in Phase 1. |