# Architecture

## Overview & Goals

This document defines the complete system architecture for the **Executive Reporting Dashboard** — a single-page, screenshot-optimized Blazor Server application that visualizes project milestones on an SVG timeline and displays monthly work item status in a color-coded heatmap grid. The application reads all data from a local `data.json` file, runs on `http://localhost:5050` with zero cloud dependencies, and is designed to produce pixel-perfect 1920×1080 screenshots for PowerPoint decks.

### Architectural Principles

1. **Radical simplicity** — This is a data-visualization-as-code tool, not an enterprise application. No database, no auth, no API layer, no JavaScript.
2. **Design fidelity** — The `OriginalDesignConcept.html` is the canonical visual spec. CSS is ported verbatim; deviations are bugs.
3. **Data-driven rendering** — Every pixel on screen is controlled by `data.json`. Changing the file changes the output. No code changes required.
4. **Zero external dependencies** — The main project uses only what ships with .NET 8. No NuGet packages, no npm, no CDN references.
5. **File count discipline** — 8–12 source files maximum. If you're adding a 13th file, you're over-engineering.

### Goals Traceability

| Business Goal | Architectural Response |
|---|---|
| At-a-glance project visibility | Single-page layout with header, timeline, and heatmap — all visible without scrolling at 1920×1080 |
| Eliminate manual slide creation | Data-driven rendering from `data.json` produces screenshot-ready output |
| Reduce reporting cycle time | `FileSystemWatcher` enables edit-save-screenshot workflow in under 2 minutes |
| Standardize reporting format | Reusable template — swap `data.json` for any project |
| Zero operational cost | Localhost-only, no cloud, no licenses, no subscriptions |

---

## System Components

### 1. `Program.cs` — Application Entry Point

- **Responsibility:** Configure Kestrel, register DI services, set up the Blazor Server pipeline, configure the listening port to `http://localhost:5050`.
- **Interfaces:** None (top-level entry point).
- **Dependencies:** `DashboardDataService` (registers as singleton).
- **Key Behaviors:**
  - Reads the `data.json` file path from `appsettings.json` (key: `DashboardDataFile`, default: `data.json` relative to content root).
  - Configures Kestrel to listen on port 5050 (HTTP only, no HTTPS).
  - Registers `DashboardDataService` as a singleton in `IServiceCollection`.
  - Maps Blazor Server hub and fallback to `Dashboard.razor`.
  - Does NOT configure authentication, authorization, CORS, or HTTPS redirection.

```csharp
// Pseudocode for Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5050");
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

### 2. `DashboardDataService` — Data Loading & File Watching

- **Responsibility:** Read `data.json`, deserialize to `DashboardData`, validate required fields, watch for file changes, notify subscribers.
- **Interfaces:**
  - `DashboardData? Data { get; }` — Current deserialized data (null if error).
  - `string? ErrorMessage { get; }` — Human-readable error if data loading failed.
  - `event Action? OnDataChanged` — Fired when `data.json` is modified and re-loaded.
- **Dependencies:** `System.Text.Json`, `System.IO.FileSystemWatcher`, `IWebHostEnvironment` (for content root path).
- **Data:** Holds the in-memory `DashboardData` object. No persistence beyond the JSON file.
- **Key Behaviors:**
  - On construction: reads and deserializes `data.json`. If missing or malformed, sets `ErrorMessage` instead of throwing.
  - Creates a `FileSystemWatcher` on the directory containing `data.json`, filtering for the specific filename.
  - On file change: debounces rapid saves (~500ms via `System.Threading.Timer`), re-reads the file, re-deserializes, validates, and fires `OnDataChanged`.
  - File read uses retry logic (up to 3 attempts with 100ms delay) to handle file locks during save operations.
  - Validation checks: `title` is required, `months` array must have 1+ entries, `categories` must have 1+ entries, `timelineStart` < `timelineEnd`, `currentMonth` must exist in `months` array.
  - Implements `IDisposable` to clean up the `FileSystemWatcher` and debounce timer.

```csharp
// DashboardDataService lifecycle
public class DashboardDataService : IDisposable
{
    public DashboardData? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public event Action? OnDataChanged;

    private readonly FileSystemWatcher _watcher;
    private readonly Timer _debounce;
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Constructor: load initial data, start watcher
    // OnFileChanged: debounce → reload → validate → notify
    // Dispose: stop watcher, dispose timer
}
```

### 3. `Dashboard.razor` — Main Page Component

- **Responsibility:** Top-level page that composes all visual sections. Subscribes to `DashboardDataService.OnDataChanged` and calls `StateHasChanged()` to trigger re-render via Blazor Server's SignalR connection.
- **Interfaces:** Blazor `@page "/"` route. Accepts `[SupplyParameterFromQuery] bool Screenshot` for the `?screenshot=true` parameter.
- **Dependencies:** `DashboardDataService` (injected via `@inject`).
- **Data:** Reads `DashboardDataService.Data` and passes sub-objects to child components via `[Parameter]`.
- **Key Behaviors:**
  - If `DashboardDataService.ErrorMessage` is non-null, renders an error panel instead of the dashboard.
  - If data is valid, renders: Header → Timeline → Heatmap in a flex-column layout.
  - Subscribes to `OnDataChanged` in `OnInitialized`; unsubscribes in `Dispose` (implements `IDisposable`).
  - Uses `InvokeAsync(StateHasChanged)` in the event handler since `FileSystemWatcher` fires on a background thread.

### 4. `MainLayout.razor` — Minimal Layout Shell

- **Responsibility:** Provides the HTML `<head>` with CSS link and the `<body>` wrapper with the fixed 1920×1080 viewport styling.
- **Interfaces:** Standard Blazor layout component with `@Body` render fragment.
- **Dependencies:** `dashboard.css` (static file reference).
- **Key Behaviors:**
  - Sets `<meta name="viewport" content="width=1920">` to force the fixed layout.
  - References `css/dashboard.css` as the sole stylesheet.
  - No navigation, no sidebar, no Blazor default chrome.

### 5. `HeaderSection.razor` — Header Bar Component

- **Responsibility:** Renders the project title, subtitle, backlog link, and milestone legend.
- **Interfaces:**
  - `[Parameter] string Title` — Project title text.
  - `[Parameter] string Subtitle` — Org/workstream/date line.
  - `[Parameter] string? BacklogUrl` — Optional ADO backlog URL.
  - `[Parameter] LegendConfig Legend` — Legend items configuration.
- **Dependencies:** None (pure presentational component).
- **Visual Mapping:** `.hdr` section from `OriginalDesignConcept.html`.

### 6. `Timeline.razor` — SVG Milestone Timeline Component

- **Responsibility:** Renders the full timeline area including the left sidebar labels and the SVG canvas with gridlines, NOW line, stream tracks, and milestone markers.
- **Interfaces:**
  - `[Parameter] List<MilestoneStream> Streams` — Milestone stream data.
  - `[Parameter] DateTime TimelineStart` — Left edge date.
  - `[Parameter] DateTime TimelineEnd` — Right edge date.
  - `[Parameter] DateTime CurrentDate` — Date for the NOW line.
  - `[Parameter] List<TimelineMonth> TimelineMonths` — Month gridline labels and positions.
- **Dependencies:** None (pure presentational, generates SVG markup inline).
- **Key Behaviors:**
  - **X-position formula:** `X = (date - timelineStart).TotalDays / (timelineEnd - timelineStart).TotalDays * 1560.0`
  - **Y-position formula:** For N streams, `Y[i] = 42 + (i * (185 - 42*2) / max(N-1, 1))` — evenly distributed within the 185px SVG height, defaulting to Y=42, 98, 154 for 3 streams.
  - **Month gridlines:** Computed from timeline date range. Each month boundary gets a vertical line. SVG width is fixed at 1560px; gridline X positions are calculated using the same date-to-X formula applied to the 1st of each month.
  - **Milestone markers:** Switch on `type`: `checkpoint` → `<circle>` with white fill and stream-colored stroke; `poc` → `<polygon>` diamond with `fill="#F4B400"` and drop shadow; `production` → `<polygon>` diamond with `fill="#34A853"` and drop shadow; `dot` → small `<circle>` with `fill="#999"`.
  - **NOW line:** Red dashed vertical line at the X position of `currentDate`, with "NOW" label.
  - **SVG drop shadow filter:** Defined once in `<defs>` and referenced via `filter="url(#sh)"`.
- **Visual Mapping:** `.tl-area`, `.tl-svg-box`, and inline SVG from `OriginalDesignConcept.html`.

### 7. `Heatmap.razor` — Heatmap Grid Component

- **Responsibility:** Renders the section title and the CSS Grid containing the corner cell, month headers, row headers, and data cells.
- **Interfaces:**
  - `[Parameter] List<string> Months` — Month column names.
  - `[Parameter] string CurrentMonth` — Which month to highlight.
  - `[Parameter] List<HeatmapCategory> Categories` — The four status rows with items per month.
- **Dependencies:** `HeatmapCell.razor` (child component).
- **Key Behaviors:**
  - Dynamically sets `grid-template-columns: 160px repeat(N, 1fr)` via inline `style` attribute where N = `Months.Count`.
  - Dynamically sets `grid-template-rows: 36px repeat(M, 1fr)` where M = `Categories.Count` (always 4 per spec).
  - Iterates categories and months to emit row headers and data cells.
  - Applies the `.current-month-hdr` CSS class (maps to `.apr-hdr` styling) to the current month's column header.
- **Visual Mapping:** `.hm-wrap`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr` from `OriginalDesignConcept.html`.

### 8. `HeatmapCell.razor` — Individual Grid Cell Component

- **Responsibility:** Renders a single heatmap data cell with its list of work items (or a gray dash if empty).
- **Interfaces:**
  - `[Parameter] List<string> Items` — Work item names for this cell.
  - `[Parameter] string CssClass` — Status-specific CSS class (e.g., `ship-cell`).
  - `[Parameter] bool IsCurrentMonth` — Whether to apply the current-month highlight class.
- **Dependencies:** None.
- **Key Behaviors:**
  - If `Items` is empty, renders `<span style="color:#AAA">-</span>`.
  - Otherwise, renders each item as `<div class="it">item text</div>` — the `::before` pseudo-element provides the colored bullet dot via CSS.
  - Computes the combined CSS class: `"{CssClass}{(IsCurrentMonth ? " current" : "")}"`.
- **Visual Mapping:** `.hm-cell`, `.it`, `::before` pseudo-elements from `OriginalDesignConcept.html`.

### 9. `dashboard.css` — Stylesheet

- **Responsibility:** All visual styling. Ported directly from `OriginalDesignConcept.html` with enhancements for CSS custom properties and dynamic grid columns.
- **Key Design Decisions:**
  - Body: `width: 1920px; height: 1080px; overflow: hidden` — hard-coded for screenshot fidelity.
  - CSS custom properties defined in `:root` for the color palette (enables future theming).
  - The `.current` class replaces the hard-coded `.apr` class from the reference, making current-month highlighting data-driven.
  - Grid column count is set via inline `style` from `Heatmap.razor`, not hard-coded in CSS.

### 10. `data.json` — Data File

- **Responsibility:** Single source of truth for all dashboard content.
- **Location:** Project content root (configurable via `appsettings.json`).
- **Schema:** Defined by the C# data models (see Data Model section).

---

## Component Interactions

### Data Flow Diagram

```
┌──────────────┐     read/watch      ┌────────────────────────┐
│  data.json   │ ──────────────────→  │  DashboardDataService  │
│  (flat file) │                      │  (singleton)           │
└──────────────┘                      │                        │
                                      │  - DashboardData? Data │
                                      │  - string? ErrorMessage│
                                      │  - OnDataChanged event │
                                      └───────────┬────────────┘
                                                  │
                                          inject + subscribe
                                                  │
                                                  ▼
                                      ┌────────────────────────┐
                                      │   Dashboard.razor      │
                                      │   (@page "/")          │
                                      │                        │
                                      │  if error → ErrorPanel │
                                      │  if data  → sections   │
                                      └───┬────┬────┬──────────┘
                                          │    │    │
                              [Parameter] │    │    │ [Parameter]
                                          ▼    │    ▼
                              ┌──────────┐ │  ┌──────────────┐
                              │ Header   │ │  │  Heatmap     │
                              │ Section  │ │  │  .razor      │
                              └──────────┘ │  └──────┬───────┘
                                           │         │
                                           ▼         ▼
                                   ┌──────────┐ ┌──────────────┐
                                   │ Timeline │ │ HeatmapCell  │
                                   │ .razor   │ │ .razor       │
                                   └──────────┘ └──────────────┘
```

### Communication Patterns

1. **Startup Load:**
   - `Program.cs` registers `DashboardDataService` as singleton.
   - On first injection, the service constructor reads `data.json` synchronously and deserializes it.
   - If the file is missing or malformed, `Data` is null and `ErrorMessage` is set.

2. **Initial Page Render:**
   - Browser connects to `http://localhost:5050`.
   - Blazor Server renders `Dashboard.razor` on the server.
   - `Dashboard.razor` reads `Data` from the injected service.
   - If `Data` is null, renders the error panel.
   - If `Data` is present, passes sub-properties to child components via `[Parameter]`.
   - Rendered HTML is pushed to the browser over the SignalR connection.

3. **Live Reload (File Change):**
   - User saves `data.json` in a text editor.
   - `FileSystemWatcher` fires `Changed` event.
   - `DashboardDataService` debounces (500ms timer reset on each event).
   - After debounce: re-reads file (with retry for file locks), deserializes, validates.
   - If valid: updates `Data`, clears `ErrorMessage`, fires `OnDataChanged`.
   - If invalid: sets `ErrorMessage`, fires `OnDataChanged`.
   - `Dashboard.razor` event handler calls `InvokeAsync(StateHasChanged)`.
   - Blazor Server diff-renders and pushes the delta to the browser via SignalR.
   - Browser updates in-place — no full page refresh.

4. **Parameter Cascading:**
   - `Dashboard.razor` does NOT use `CascadingValue`. Instead, it passes explicit `[Parameter]` props to each child component. This keeps data flow explicit and debuggable for a small component tree.

---

## Data Model

### C# Model Classes

All models reside in the `Data/Models/` directory. They map 1:1 to the `data.json` schema.

#### `DashboardData.cs` — Root Model

```csharp
public class DashboardData
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string? BacklogUrl { get; set; }
    public DateTime CurrentDate { get; set; }
    public DateTime TimelineStart { get; set; }
    public DateTime TimelineEnd { get; set; }
    public List<MilestoneStream> MilestoneStreams { get; set; } = new();
    public List<string> Months { get; set; } = new();
    public string CurrentMonth { get; set; } = "";
    public List<HeatmapCategory> Categories { get; set; } = new();
}
```

#### `MilestoneStream.cs`

```csharp
public class MilestoneStream
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
    public string Type { get; set; } = "checkpoint"; // "checkpoint" | "poc" | "production" | "dot"
    public string? LabelPosition { get; set; } // "above" | "below" (default: alternating)
}
```

#### `HeatmapCategory.cs`

```csharp
public class HeatmapCategory
{
    public string Name { get; set; } = "";       // Display name: "Shipped", "In Progress", etc.
    public string CssClass { get; set; } = "";    // CSS prefix: "ship", "prog", "carry", "block"
    public Dictionary<string, List<string>> Items { get; set; } = new();
}
```

### JSON Schema Contract (`data.json`)

```json
{
  "title": "string (required) — Project title displayed in header",
  "subtitle": "string (required) — Org/workstream/date line",
  "backlogUrl": "string? (optional) — URL for the ADO backlog link",
  "currentDate": "date string YYYY-MM-DD (required) — Position of the NOW line",
  "timelineStart": "date string YYYY-MM-DD (required) — Left edge of timeline",
  "timelineEnd": "date string YYYY-MM-DD (required) — Right edge of timeline",
  "milestoneStreams": [
    {
      "id": "string (required) — Stream identifier e.g. 'M1'",
      "name": "string (required) — Stream description",
      "color": "string (required) — Hex color for the stream track",
      "milestones": [
        {
          "date": "date string YYYY-MM-DD (required)",
          "label": "string (required) — Text shown near the marker",
          "type": "string (required) — 'checkpoint' | 'poc' | 'production' | 'dot'",
          "labelPosition": "string? (optional) — 'above' | 'below'"
        }
      ]
    }
  ],
  "months": ["string array (required) — Month names for heatmap columns"],
  "currentMonth": "string (required) — Must match one entry in months array",
  "categories": [
    {
      "name": "string (required) — Display name",
      "cssClass": "string (required) — CSS class prefix: 'ship'|'prog'|'carry'|'block'",
      "items": {
        "<monthName>": ["array of strings — work item titles"]
      }
    }
  ]
}
```

### Validation Rules (Enforced by `DashboardDataService`)

| Field | Rule | Error Message |
|---|---|---|
| `title` | Non-empty string | `"Missing required field: 'title'"` |
| `subtitle` | Non-empty string | `"Missing required field: 'subtitle'"` |
| `currentDate` | Valid date, within timeline range | `"'currentDate' must be between timelineStart and timelineEnd"` |
| `timelineStart` | Valid date | `"Missing or invalid field: 'timelineStart'"` |
| `timelineEnd` | Valid date, after `timelineStart` | `"'timelineEnd' must be after 'timelineStart'"` |
| `months` | Non-empty array | `"'months' must contain at least one entry"` |
| `currentMonth` | Exists in `months` array | `"'currentMonth' value '{value}' not found in months array"` |
| `categories` | Non-empty array | `"'categories' must contain at least one entry"` |
| `categories[].cssClass` | One of: `ship`, `prog`, `carry`, `block` | `"Invalid cssClass '{value}'. Expected: ship, prog, carry, block"` |
| `milestoneStreams` | 1–5 entries | `"'milestoneStreams' must contain 1 to 5 entries"` |
| `milestones[].type` | One of: `checkpoint`, `poc`, `production`, `dot` | `"Invalid milestone type '{value}'"` |

### Storage

- **No database.** The only persistent storage is the `data.json` flat file.
- **In-memory state:** `DashboardDataService` holds the deserialized `DashboardData` object in memory. It is re-created on each file change.
- **No caching layer.** The data set is trivially small (<50KB). Deserialization takes <100ms.

---

## API Contracts

This application has **no REST, GraphQL, or Web API endpoints**. It is a Blazor Server application that renders HTML server-side and pushes updates over a SignalR WebSocket connection.

### Implicit Contracts

| Endpoint | Protocol | Purpose |
|---|---|---|
| `GET /` | HTTP → Blazor Server | Serves the dashboard page. Returns server-rendered HTML, then upgrades to SignalR for live updates. |
| `GET /?screenshot=true` | HTTP → Blazor Server | Same as above, but the `Screenshot` query parameter is captured via `[SupplyParameterFromQuery]` for future use (hides non-essential UI if any is added). |
| `/_blazor` | WebSocket (SignalR) | Blazor Server's built-in SignalR hub for UI interactivity and server-push re-renders. Managed entirely by the framework — no custom hub code. |
| `GET /css/dashboard.css` | HTTP (static file) | Serves the dashboard stylesheet from `wwwroot/css/`. |

### Error Handling Contract

The application does NOT return HTTP error codes for data issues. Instead, errors are rendered inline in the page:

| Condition | Rendered Output |
|---|---|
| `data.json` not found | Centered error panel: `"Configuration Error: data.json not found at [absolute path]. Please create the file and restart."` |
| `data.json` has invalid JSON syntax | Centered error panel: `"Invalid JSON in data.json: [System.Text.Json error message, sanitized to remove stack traces]"` |
| `data.json` missing required field | Centered error panel: `"Validation Error: [specific field message from validation rules table]"` |
| `data.json` is valid | Full dashboard renders normally |

The error panel uses a simple centered `<div>` with a red border and clear text. It replaces the entire dashboard — no partial rendering with missing sections.

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|---|---|
| **Operating System** | Windows 10/11 (required for Segoe UI font; macOS/Linux degrades to Arial) |
| **.NET SDK** | .NET 8.0.x (LTS) — must be pre-installed |
| **Browser** | Microsoft Edge or Google Chrome (latest stable) |
| **Display** | 1920×1080 minimum resolution for screenshot capture |
| **RAM** | <100MB for the running application |
| **Disk** | <10MB for the published application |

### Hosting

| Aspect | Configuration |
|---|---|
| **Web Server** | Kestrel (built into .NET 8, no IIS or reverse proxy) |
| **URL** | `http://localhost:5050` |
| **Protocol** | HTTP only (no HTTPS — localhost-only access) |
| **Binding** | `localhost` only — not accessible from network |

### Networking

- **Inbound:** HTTP on port 5050, localhost only.
- **Outbound:** None. Zero network calls. No telemetry, no NuGet restore at runtime, no CDN.
- **SignalR:** Blazor Server uses a WebSocket connection between the browser and Kestrel on localhost. This is automatic and requires no configuration.

### Storage

- **`data.json`:** Located in the project content root directory (same directory as `ReportingDashboard.csproj`). Path is configurable in `appsettings.json`.
- **`wwwroot/css/dashboard.css`:** Static CSS file served by Kestrel's static file middleware.
- **No logs to disk** — Console logging only (default .NET 8 behavior).

### CI/CD

Not required for v1. If added later:

- **Build:** `dotnet build src/ReportingDashboard/ReportingDashboard.csproj`
- **Test:** `dotnet test tests/ReportingDashboard.Tests/ReportingDashboard.Tests.csproj`
- **Publish:** `dotnet publish src/ReportingDashboard/ReportingDashboard.csproj -c Release -o ./publish`
- **No container, no cloud deployment, no artifact registry.**

### Project Structure (Mandatory)

```
ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj          # Sdk="Microsoft.NET.Sdk.Web", net8.0, zero NuGet deps
│       ├── Program.cs                          # Entry point, DI, Kestrel config
│       ├── appsettings.json                    # DashboardDataFile path config
│       ├── Properties/
│       │   └── launchSettings.json             # Port 5050 config
│       ├── Data/
│       │   ├── DashboardDataService.cs         # JSON loading, FileSystemWatcher, validation
│       │   └── Models/
│       │       ├── DashboardData.cs            # Root data model
│       │       ├── MilestoneStream.cs          # Stream with milestones
│       │       ├── Milestone.cs                # Individual milestone marker
│       │       └── HeatmapCategory.cs          # Status row with items per month
│       ├── Components/
│       │   ├── App.razor                       # Root component (<html>, <head>, <body>)
│       │   ├── Routes.razor                    # Router component
│       │   ├── Layout/
│       │   │   └── MainLayout.razor            # Minimal layout, CSS reference
│       │   ├── Pages/
│       │   │   └── Dashboard.razor             # Main page, composes all sections
│       │   ├── HeaderSection.razor             # Title, subtitle, backlog link, legend
│       │   ├── Timeline.razor                  # SVG timeline with milestone streams
│       │   ├── Heatmap.razor                   # CSS Grid heatmap container
│       │   └── HeatmapCell.razor               # Individual data cell
│       ├── wwwroot/
│       │   └── css/
│       │       └── dashboard.css               # All styles, ported from reference HTML
│       └── data.json                           # Sample "Project Phoenix" data
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj      # xUnit, FluentAssertions, bUnit
        └── DashboardDataServiceTests.cs         # Deserialization & validation tests
```

**Total source files (main project):** 12 files + 1 CSS + 1 JSON = 14 files (within the 8–12 source file target when counting only `.cs` and `.razor`).

---

## Technology Stack Decisions

### Chosen Technologies

| Layer | Technology | Version | Justification |
|---|---|---|---|
| **Runtime** | .NET 8 (LTS) | 8.0.x | Mandated by project requirements. LTS support through Nov 2026. |
| **UI Framework** | Blazor Server | Built into .NET 8 | Server-side rendering ensures consistent screenshot output. SignalR connection enables live-reload without page refresh. No WASM download penalty. |
| **CSS Layout** | CSS Grid + Flexbox | Native browser | Reference design already uses these. No framework needed for ~80 lines of CSS. |
| **SVG Rendering** | Inline SVG via Razor syntax | Native browser | Timeline markers (diamonds, circles, lines) are simple geometric shapes. A charting library would be 100x more code for the same output. |
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Zero-dependency JSON deserialization. `PropertyNameCaseInsensitive = true` handles camelCase JSON ↔ PascalCase C#. |
| **File Watching** | `System.IO.FileSystemWatcher` | Built into .NET 8 | Monitors `data.json` for changes. Combined with Blazor Server's SignalR push, enables sub-2-second live reload. |
| **Web Server** | Kestrel | Built into .NET 8 | Lightweight, high-performance, zero-config for localhost scenarios. |
| **Fonts** | Segoe UI (system) | Windows built-in | No web font loading. Fallback to Arial on non-Windows (acceptable degradation). |
| **Testing** | xUnit + FluentAssertions + bUnit | 2.7.x / 6.12.x / 1.25.x | Test project only. Main project has zero NuGet dependencies. |

### Rejected Alternatives

| Alternative | Why Rejected |
|---|---|
| **MudBlazor / Radzen** | Component libraries fight custom CSS layouts. The reference design is already pixel-specified — a component library adds weight without value. |
| **Chart.js / Plotly** | Requires JavaScript interop, which violates the "no JavaScript" constraint. The SVG is simple enough to generate in Razor. |
| **Bootstrap / Tailwind** | 80 lines of purpose-built CSS don't need a framework. Adding one introduces class conflicts and bundle size. |
| **Blazor WebAssembly** | Adds a ~2MB WASM download. No benefit for a localhost tool. Server mode gives free SignalR push for live reload. |
| **Static SSR (Blazor .NET 8)** | Cannot push updates to the browser without a full page refresh. Loses the live-reload capability. |
| **SQLite / LiteDB** | A database for <100 data points is over-engineering. JSON is human-editable in any text editor. |
| **IConfiguration / Options pattern** | `data.json` is not app configuration — it's domain data that changes at runtime. Direct file read with `System.Text.Json` is simpler and avoids `IOptionsMonitor` complexity. |
| **MediatR / CQRS** | Architectural patterns for enterprise apps with complex domain logic. This app has one operation: "read a file and render it." |

---

## Security Considerations

### Threat Model

This application has a minimal threat surface because it runs exclusively on localhost and is accessed only by the person who launched it.

| Threat | Assessment | Mitigation |
|---|---|---|
| **Unauthorized network access** | N/A | Kestrel binds to `localhost` only. Not accessible from the network. |
| **Credential exposure** | N/A | No credentials, API keys, or secrets exist anywhere in the project. |
| **PII in data** | N/A | `data.json` contains only project names, dates, and work item titles. No personal data. |
| **XSS via data.json** | LOW | Blazor Server automatically HTML-encodes all rendered text. A malicious string in `data.json` (e.g., `<script>alert(1)</script>`) would render as literal text, not executed script. |
| **Path traversal via config** | LOW | The `DashboardDataFile` path in `appsettings.json` is set by the developer who also runs the app. No user-supplied input reaches the file path. |
| **Denial of service** | N/A | Single-user localhost tool. No external traffic. |
| **Man-in-the-middle** | N/A | HTTP on localhost. No sensitive data in transit. |
| **Supply chain (NuGet)** | MINIMAL | Main project has zero NuGet dependencies. Test project dependencies (xUnit, etc.) are well-established, audited packages. |

### Authentication & Authorization

**None.** Explicitly out of scope. No middleware, no identity provider, no cookies, no tokens.

**Future consideration:** If the dashboard is ever exposed on a network, add `builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate()` for Windows Integrated Authentication. This is a one-line change.

### Data Protection

- No encryption at rest (plain JSON file on developer's local disk).
- No encryption in transit (HTTP on localhost).
- No secrets management needed.
- `data.json` should NOT contain PII, credentials, or sensitive business data beyond project status.

### Input Validation

- `System.Text.Json` deserialization rejects malformed JSON with a clear error.
- `DashboardDataService` validates all required fields and value constraints (see Validation Rules table).
- Invalid data results in a user-friendly error message in the browser, not a stack trace.
- Blazor's built-in HTML encoding prevents XSS from data values rendered in the UI.

---

## UI Component Architecture

This section maps each visual section from the `OriginalDesignConcept.html` design to a specific Blazor component, its CSS layout strategy, data bindings, and interactions.

### Visual Section → Component Mapping

#### 1. Header Bar (`.hdr`)

| Attribute | Value |
|---|---|
| **Component** | `HeaderSection.razor` |
| **CSS Layout** | `display: flex; align-items: center; justify-content: space-between` on `.hdr` container |
| **CSS Classes** | `.hdr`, `.hdr h1`, `.sub` |
| **Dimensions** | Full width, auto height (~48px), `padding: 12px 44px 10px`, `border-bottom: 1px solid #E0E0E0` |
| **Data Bindings** | `Title` → `<h1>` text; `BacklogUrl` → `<a href>` (conditionally rendered if non-null); `Subtitle` → `.sub` div text |
| **Interactions** | Backlog link opens in new tab (`target="_blank"`). No other interactivity. |

**Legend sub-section (right side of header):**

| Attribute | Value |
|---|---|
| **Rendered in** | `HeaderSection.razor` (inline, not a separate component) |
| **CSS Layout** | `display: flex; gap: 22px; align-items: center` |
| **Legend Items** | Four inline `<span>` elements, each with an icon + label at `font-size: 12px` |
| **Icon Rendering** | PoC: 12×12px amber square with `transform: rotate(45deg)`. Production: same, green. Checkpoint: 8×8px gray circle. Now: 2×14px red vertical bar. All rendered as inline `<span>` with CSS — no SVG, no images. |

#### 2. Timeline Area (`.tl-area`)

| Attribute | Value |
|---|---|
| **Component** | `Timeline.razor` |
| **CSS Layout** | Outer: `display: flex; align-items: stretch`. Left sidebar: `width: 230px; flex-shrink: 0`. Right: `flex: 1`. |
| **CSS Classes** | `.tl-area`, `.tl-svg-box` |
| **Dimensions** | `height: 196px`, `padding: 6px 44px 0`, `background: #FAFAFA`, `border-bottom: 2px solid #E8E8E8` |
| **Data Bindings** | `Streams` → sidebar labels + SVG track lines/markers; `TimelineStart`/`TimelineEnd` → X-axis scale; `CurrentDate` → NOW line position; `TimelineMonths` → gridline positions and labels |
| **Interactions** | None. Static SVG. No hover, no click, no tooltip. |

**Left sidebar (stream labels):**
- Rendered as `<div>` elements inside the 230px sidebar.
- Each stream: `<div>` with stream ID in stream color (`font-weight: 600`) and name in `color: #444; font-weight: 400`.
- Vertically distributed: `justify-content: space-around`.

**SVG canvas (`.tl-svg-box`):**
- `<svg width="1560" height="185">` rendered inline in Razor.
- `<defs>` block contains the drop shadow filter (`<filter id="sh"><feDropShadow ...>`).
- Month gridlines: `<line>` + `<text>` pairs, X positions from date calculation.
- NOW line: `<line>` with `stroke-dasharray="5,3"` + `<text>NOW</text>`.
- Stream tracks: `<line>` from x1=0 to x2=1560 at computed Y.
- Checkpoint markers: `<circle>` with `fill="white"`, `stroke="{streamColor}"`, `stroke-width="2.5"`.
- PoC diamonds: `<polygon points="{x},{y-11} {x+11},{y} {x},{y+11} {x-11},{y}">` with `fill="#F4B400"` and `filter="url(#sh)"`.
- Production diamonds: Same polygon, `fill="#34A853"`.
- Dot markers: `<circle r="4" fill="#999">`.
- Date labels: `<text>` with `text-anchor="middle"`, positioned above or below markers.

#### 3. Heatmap Section (`.hm-wrap`)

| Attribute | Value |
|---|---|
| **Component** | `Heatmap.razor` (container) + `HeatmapCell.razor` (cells) |
| **CSS Layout** | Section: `flex: 1; display: flex; flex-direction: column`. Grid: `display: grid; grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr)` |
| **CSS Classes** | `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`, `.it` |
| **Dimensions** | Fills remaining vertical space below timeline. `padding: 10px 44px 10px`. Grid has `border: 1px solid #E0E0E0`. |
| **Data Bindings** | `Months` → column headers; `CurrentMonth` → highlighted column; `Categories` → rows (name → row header, items[month] → cell content) |
| **Interactions** | None. Static grid. |

**Section title (`.hm-title`):**
- Static text: `"MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS"`
- `font-size: 14px; font-weight: 700; color: #888; text-transform: uppercase; letter-spacing: 0.5px`

**Grid header row:**
- Corner cell: `.hm-corner` — `"STATUS"` label.
- Month headers: `.hm-col-hdr` — each month name. Current month gets additional class for `background: #FFF0D0; color: #C07700`.

**Status rows (rendered by `Heatmap.razor` loop):**
- Row header: `.hm-row-hdr` + status-specific class (`.ship-hdr`, `.prog-hdr`, `.carry-hdr`, `.block-hdr`).
- Data cells: `HeatmapCell.razor` instances, one per month per category.

**Individual cells (`HeatmapCell.razor`):**
- CSS class: `{cssClass}-cell` (e.g., `ship-cell`), plus `current` class if current month.
- Items: `<div class="it">{item text}</div>` with `::before` pseudo-element providing the 6px colored dot.
- Empty state: `<span style="color:#AAA">-</span>`.

#### 4. Error Panel (inline in `Dashboard.razor`)

| Attribute | Value |
|---|---|
| **Component** | Inline conditional block in `Dashboard.razor` |
| **CSS Layout** | `display: flex; align-items: center; justify-content: center; height: 100%` |
| **Styling** | Red-bordered `<div>` with `max-width: 600px; padding: 32px; border: 2px solid #EA4335; border-radius: 8px; background: #FFF5F5` |
| **Data Bindings** | `DashboardDataService.ErrorMessage` → error text |
| **Interactions** | None. User must fix `data.json` and save; live-reload will re-render. |

---

## Scaling Strategy

This application is explicitly **not designed to scale** beyond a single user on a single machine. This is an intentional architectural decision, not a limitation.

### Current Scale

| Dimension | Capacity |
|---|---|
| **Users** | 1 (the person running `dotnet run`) |
| **Data volume** | <100 work items, <5 milestone streams, <12 months |
| **Concurrent connections** | 1 browser tab (Blazor Server SignalR) |
| **Throughput** | 1 page render per data change |

### Data Scaling Boundaries

| Input | Tested Range | Visual Limit | Notes |
|---|---|---|---|
| Milestone streams | 1–5 | 5 | Beyond 5, Y-spacing in the 185px SVG becomes too compressed for readable labels |
| Milestones per stream | 1–20 | ~15 | Markers overlap at high density on a 1560px canvas |
| Months (heatmap) | 1–12 | 6 | Beyond 6, column widths become too narrow for item text at 12px |
| Items per cell | 0–10 | ~8 | Items overflow the cell height at ~8 entries (depends on row height) |
| Total work items | 0–100 | ~60 across visible cells | 1920×1080 viewport constrains total visible text |

### If Scale Requirements Change

| Scenario | Recommended Approach |
|---|---|
| Multiple projects | Separate `data.json` files per project. Launch separate instances on different ports, or add a project selector dropdown (Phase 3). |
| Team-wide access | Deploy behind a reverse proxy with Windows Authentication. Blazor Server handles multiple SignalR connections natively. No code change needed. |
| Larger data sets | Pagination within heatmap cells (show top N items with a count). Not needed for v1. |
| High availability | Not applicable. This is a local reporting tool, not a service. |

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| 1 | **Over-engineering / scope creep** | HIGH | Schedule slip, unnecessary complexity | Enforce constraints: no database, no auth, no API, no JS. If a feature doesn't improve the screenshot, cut it. Architecture is intentionally minimal. |
| 2 | **`data.json` schema drift** | MEDIUM | Runtime deserialization failures, confusing errors | Strong C# model with explicit types. Startup validation with field-specific error messages. Sample `data.json` serves as a living schema reference. |
| 3 | **`FileSystemWatcher` reliability on Windows** | MEDIUM | Missed file changes, duplicate events | 500ms debounce timer to coalesce rapid events. Retry logic (3 attempts, 100ms delay) for file lock issues. `FileSystemWatcher` is well-tested on Windows NTFS. |
| 4 | **SVG rendering inconsistencies across browsers** | LOW | Visual differences between Edge and Chrome screenshots | Standardize on Edge for all screenshots. SVG is simple geometry (lines, circles, polygons) with no advanced features. Test once and lock. |
| 5 | **Blazor Server SignalR connection drop** | LOW | Dashboard stops receiving live updates | Blazor's built-in reconnection logic handles transient disconnects. For a localhost tool, this is rare. User can refresh the browser as a fallback. |
| 6 | **CSS layout pixel mismatch vs. reference** | MEDIUM | Visual fidelity failure | CSS is ported verbatim from the reference HTML. Visual QA step compares rendered output to `OriginalDesignConcept.png` at 1920×1080. |
| 7 | **Font rendering differences** | LOW | Subtle text layout shifts | Segoe UI is the primary font (Windows system font). Arial fallback covers non-Windows. Both are web-safe with predictable metrics. |
| 8 | **Large `data.json` causes slow render** | LOW | Dashboard takes >2s to update | Data size is bounded by spec (<100 items, <50KB JSON). Deserialization target is <100ms. If exceeded, profile and optimize the Razor loops. |
| 9 | **Developer forgets to run `dotnet restore`** | LOW | Build failure on fresh clone | Main project has zero NuGet dependencies — `dotnet build` works without `dotnet restore`. Test project restore is handled automatically by `dotnet test`. |
| 10 | **JSON editing errors by project lead** | HIGH | Dashboard shows error instead of data | Clear, specific validation error messages guide the user to the exact field with the problem. Live reload means fixing the JSON immediately updates the dashboard. |