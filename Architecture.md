# Architecture

## Overview & Goals

This system is a **single-page executive reporting dashboard** that renders project milestones, shipping status, and monthly execution progress as a pixel-perfect 1920×1080 static page optimized for PowerPoint screenshot capture. It is a local-only Blazor Server (.NET 8) application that reads all data from a single `data.json` file and produces a fully rendered HTML/CSS/SVG page with zero interactivity, zero authentication, zero cloud dependencies, and zero third-party UI libraries.

### Architectural Goals

1. **Pixel-perfect visual fidelity** — The rendered output must match `OriginalDesignConcept.html` at ≥95% visual parity across header, SVG timeline, and heatmap grid sections.
2. **Data-driven rendering** — All visible content is sourced from `data.json`; no hardcoded project data in components.
3. **Zero-dependency simplicity** — No NuGet packages beyond the default Blazor Server template; no JavaScript; no external CSS frameworks.
4. **Sub-second page load** — Static SSR renders the complete page in a single server pass in <500ms on localhost.
5. **Edit-refresh workflow** — PMs edit `data.json`, save, and see changes via `dotnet watch` hot reload in <5 seconds.

### Architectural Style

**Static Data Renderer** — This is not a CRUD application. The architecture is a unidirectional pipeline:

```
data.json → DashboardDataService → Blazor Components → Fixed-size HTML/CSS/SVG
```

There is no user input, no state mutation, no database, no API. The application is a server-side template engine that transforms structured JSON into a single static HTML page.

---

## System Components

### 1. Data Layer

#### 1.1 `data.json` (Data Source)

- **Location:** `Data/data.json` relative to project root
- **Responsibility:** Single source of truth for all dashboard content — project metadata, timeline configuration, milestone tracks, and heatmap items.
- **Format:** UTF-8 JSON file, max 100KB
- **Schema:** Documented in Data Model section below
- **Interfaces:** Read by `DashboardDataService` at application startup via `System.Text.Json`
- **Dependencies:** None (static file on disk)

#### 1.2 `DashboardData.cs` (Data Models)

- **Location:** `Models/DashboardData.cs`
- **Responsibility:** Strongly-typed C# record types that mirror the `data.json` schema. Provide compile-time safety and self-documenting data contracts.
- **Interfaces:** Used as `[Parameter]` types passed from parent to child Blazor components.
- **Dependencies:** `System.Text.Json.Serialization` for attribute annotations.

```csharp
namespace ReportingDashboard.Models;

public record DashboardData(
    ProjectInfo Project,
    TimelineConfig Timeline,
    List<MilestoneTrack> Tracks,
    HeatmapData Heatmap
);

public record ProjectInfo(
    string Title,
    string Subtitle,
    string BacklogUrl,
    string CurrentMonth
);

public record TimelineConfig(
    List<string> Months,
    double NowPosition
);

public record MilestoneTrack(
    string Id,
    string Label,
    string Color,
    List<Milestone> Milestones
);

public record Milestone(
    string Date,
    string Type,        // "checkpoint" | "poc" | "production"
    double Position,    // 0.0–1.0 relative X position on timeline
    string? Label       // optional display label (e.g., "PoC", "Prod (TBD)")
);

public record HeatmapData(
    List<string> Months,
    List<HeatmapCategory> Categories
);

public record HeatmapCategory(
    string Name,        // "Shipped" | "In Progress" | "Carryover" | "Blockers"
    string CssClass,    // "ship" | "prog" | "carry" | "block"
    string Emoji,       // "✅" | "🔄" | "🔁" | "🚫"
    Dictionary<string, List<string>> Items  // month name → list of item strings
);
```

### 2. Service Layer

#### 2.1 `DashboardDataService`

- **Location:** `Services/DashboardDataService.cs`
- **Responsibility:** Reads `data.json` from disk, deserializes it into `DashboardData`, validates required fields, and caches the result for the lifetime of the application.
- **Registration:** Singleton in DI container (`builder.Services.AddSingleton<DashboardDataService>()`)
- **Interfaces:**
  - `DashboardData? GetDashboardData()` — Returns the loaded data or null if loading failed.
  - `string? GetError()` — Returns an error message string if data loading failed.
- **Dependencies:** `System.Text.Json`, `IWebHostEnvironment` (to resolve file path)
- **Error Handling:**
  - File not found → returns descriptive error: "data.json not found at {path}"
  - Invalid JSON → returns `JsonException` message
  - Missing required fields → returns field-level validation error
  - Never throws; all errors captured and exposed via `GetError()`

```csharp
namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly DashboardData? _data;
    private readonly string? _error;

    public DashboardDataService(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "data.json");
        try
        {
            if (!File.Exists(path))
            {
                _error = $"data.json not found at: {path}";
                return;
            }
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _data = JsonSerializer.Deserialize<DashboardData>(json, options);
            if (_data is null)
                _error = "data.json deserialized to null.";
            else
                _error = Validate(_data);
        }
        catch (JsonException ex)
        {
            _error = $"Invalid JSON in data.json: {ex.Message}";
        }
    }

    public DashboardData? GetDashboardData() => _data;
    public string? GetError() => _error;

    private static string? Validate(DashboardData data)
    {
        if (string.IsNullOrWhiteSpace(data.Project?.Title))
            return "Missing required field: project.title";
        if (data.Timeline?.Months is null || data.Timeline.Months.Count == 0)
            return "Missing required field: timeline.months";
        if (data.Heatmap?.Categories is null)
            return "Missing required field: heatmap.categories";
        return null;
    }
}
```

### 3. Presentation Layer (Blazor Components)

#### 3.1 `App.razor` (Application Root)

- **Responsibility:** Blazor application shell. Renders `<head>` content (CSS link, viewport meta), applies Static SSR render mode (no SignalR circuit).
- **Key detail:** No `@rendermode` attribute — .NET 8 defaults to Static SSR, which renders once on the server with no persistent WebSocket.

#### 3.2 `MainLayout.razor` (Page Container)

- **Responsibility:** Fixed 1920×1080 pixel container. Sets `width: 1920px; height: 1080px; overflow: hidden` on the outermost `<div>`. Contains the `@Body` render fragment.
- **CSS:** Inline or layout-scoped — `display: flex; flex-direction: column;` matching the original design's `<body>` layout.

#### 3.3 `Dashboard.razor` (Page Orchestrator)

- **Route:** `@page "/"`
- **Responsibility:** Top-level page component. Injects `DashboardDataService`, checks for errors (renders error message if present), otherwise passes data down to child components via `[Parameter]`.
- **Dependencies:** `DashboardDataService` (injected)
- **Data flow:** Reads `DashboardData` once, distributes to children:
  - `DashboardHeader` ← `ProjectInfo`, `TimelineConfig`, `string CurrentMonth`
  - `TimelineSection` ← `TimelineConfig`, `List<MilestoneTrack>`
  - `HeatmapSection` ← `HeatmapData`, `string CurrentMonth`

#### 3.4 `DashboardHeader.razor`

- **Responsibility:** Renders project title (H1, 24px bold), inline ADO Backlog link, subtitle, and legend icons.
- **Parameters:** `ProjectInfo Project`, `string CurrentMonth`
- **CSS classes:** `.hdr`, `.sub` — matching original design
- **Legend markers:** Four inline `<span>` elements with CSS-styled icons (rotated squares for diamonds, circle for checkpoint, vertical bar for NOW)

#### 3.5 `TimelineSection.razor`

- **Responsibility:** Renders the complete timeline area: left sidebar with track labels + right SVG area with month dividers, track lines, milestone markers, and NOW line.
- **Parameters:** `TimelineConfig Timeline`, `List<MilestoneTrack> Tracks`
- **CSS classes:** `.tl-area`, `.tl-svg-box`
- **Sub-rendering:** Iterates over `Tracks` to render inline SVG elements. No child components needed — the SVG is simple enough to render in a single `.razor` file with `@foreach` loops.

**SVG Coordinate System:**
- Total SVG width: 1560px, height: 185px
- Month divider interval: `1560 / (monthCount)` pixels (260px for 6 months)
- Track Y positions: Evenly distributed across height. For N tracks: `y = 42 + (trackIndex * (185 - 42) / (N - 1))` when N > 1; centered at y=92 when N = 1.
- Milestone X position: `position * 1560` (position is 0.0–1.0 from `data.json`)
- NOW line X position: `nowPosition * 1560`

**Milestone Rendering Logic:**
| Type | Shape | Fill | Stroke | Size | Filter |
|------|-------|------|--------|------|--------|
| `checkpoint` | `<circle>` | `white` | track color, 2.5px | r=7 | none |
| `poc` | `<polygon>` (diamond) | `#F4B400` | none | ±11px | drop-shadow |
| `production` | `<polygon>` (diamond) | `#34A853` | none | ±11px | drop-shadow |

Diamond polygon formula for center (cx, cy): `"cx,cy-11 cx+11,cy cx,cy+11 cx-11,cy"`

#### 3.6 `HeatmapSection.razor`

- **Responsibility:** Renders the heatmap title and CSS Grid container. Delegates row rendering to `HeatmapRow`.
- **Parameters:** `HeatmapData Heatmap`, `string CurrentMonth`
- **CSS classes:** `.hm-wrap`, `.hm-title`, `.hm-grid`
- **Grid definition:** `grid-template-columns: 160px repeat({monthCount}, 1fr)` and `grid-template-rows: 36px repeat({categoryCount}, 1fr)` — dynamically set via inline `style` attribute based on data.

**Header Row Rendering:**
- Corner cell: `.hm-corner` with "STATUS" label
- Month headers: `.hm-col-hdr` for each month; current month gets additional `.apr-hdr` class (renamed to a generic current-month highlight class)

#### 3.7 `HeatmapRow.razor`

- **Responsibility:** Renders one status row (e.g., "Shipped") — the row header cell + one data cell per month.
- **Parameters:** `HeatmapCategory Category`, `List<string> Months`, `string CurrentMonth`
- **CSS classes:** Row header uses `{cssClass}-hdr` (e.g., `.ship-hdr`), data cells use `{cssClass}-cell` with `.current` modifier for the current month column.

#### 3.8 `HeatmapCell.razor`

- **Responsibility:** Renders individual work items within a month×status cell, or a muted dash if the cell is empty.
- **Parameters:** `List<string>? Items`, `string CssClass`, `bool IsCurrent`
- **CSS classes:** `.hm-cell`, `.{cssClass}-cell`, `.current` (for current month emphasis)
- **Item rendering:** Each item as `<div class="it">` with a `::before` pseudo-element for the colored dot bullet.
- **Empty state:** When `Items` is null or empty, renders `<div style="color:#AAA;font-size:12px;">—</div>`

### 4. Static Assets

#### 4.1 `app.css` (Global Stylesheet)

- **Location:** `wwwroot/css/app.css`
- **Responsibility:** All CSS rules for the dashboard. Contains CSS custom properties (`:root` variables), class definitions matching the original design's naming, and the color scheme for all four heatmap row types.
- **Architecture:** Single file, no component-scoped `.razor.css` files. This matches the original design's single-stylesheet approach and simplifies debugging.

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────┐     startup      ┌──────────────────────┐
│  data.json  │ ───────────────→ │  DashboardDataService │
│  (on disk)  │   File.ReadAll   │  (Singleton in DI)    │
└─────────────┘   + Deserialize  └──────────┬───────────┘
                                            │ GetDashboardData()
                                            ▼
                                 ┌──────────────────────┐
                                 │   Dashboard.razor     │
                                 │   (Page, route="/")   │
                                 └──┬───────┬────────┬──┘
                    [Parameter]     │       │        │     [Parameter]
                   ┌────────────────┘       │        └────────────────┐
                   ▼                        ▼                         ▼
          ┌─────────────────┐   ┌───────────────────┐   ┌─────────────────────┐
          │ DashboardHeader │   │ TimelineSection    │   │ HeatmapSection      │
          │ .razor          │   │ .razor             │   │ .razor              │
          │                 │   │ (inline SVG)       │   │                     │
          │ ← ProjectInfo   │   │ ← TimelineConfig   │   │ ← HeatmapData       │
          │ ← CurrentMonth  │   │ ← List<Track>      │   │ ← CurrentMonth      │
          └─────────────────┘   └───────────────────┘   └──┬──────────────────┘
                                                           │ @foreach category
                                                           ▼
                                                  ┌─────────────────┐
                                                  │ HeatmapRow.razor│
                                                  │ ← Category      │
                                                  │ ← Months        │
                                                  │ ← CurrentMonth  │
                                                  └──┬──────────────┘
                                                     │ @foreach month
                                                     ▼
                                                  ┌─────────────────┐
                                                  │ HeatmapCell     │
                                                  │ .razor          │
                                                  │ ← Items         │
                                                  │ ← CssClass      │
                                                  │ ← IsCurrent     │
                                                  └─────────────────┘
```

### Communication Pattern

All communication is **top-down, parameter-based, read-only**:

1. `DashboardDataService` loads data once at construction (singleton lifetime = app lifetime).
2. `Dashboard.razor` calls `GetDashboardData()` during `OnInitialized`.
3. Data flows downward via `[Parameter]` properties — no callbacks, no events, no two-way binding.
4. Each component renders its HTML/CSS/SVG fragment synchronously in a single pass.
5. No component maintains mutable state. All rendering is deterministic from the input parameters.

### Request Lifecycle

```
Browser GET /  →  Kestrel  →  Blazor Static SSR Pipeline
                                    │
                                    ├─ MainLayout.razor (1920×1080 container)
                                    │   └─ Dashboard.razor
                                    │       ├─ DashboardHeader.razor  → HTML
                                    │       ├─ TimelineSection.razor  → HTML + inline SVG
                                    │       └─ HeatmapSection.razor   → HTML (CSS Grid)
                                    │           └─ HeatmapRow × 4     → HTML
                                    │               └─ HeatmapCell × N → HTML
                                    │
                                    ▼
                              Complete HTML response (single round-trip)
```

No SignalR WebSocket is established. No subsequent network calls. The browser receives a fully rendered static HTML page.

---

## Data Model

### `data.json` Schema

```json
{
  "$schema": "data-schema",
  "project": {
    "title": "string (required) — Main heading displayed in header",
    "subtitle": "string (required) — Organization · Workstream · Month Year",
    "backlogUrl": "string (required) — URL for the ADO Backlog link",
    "currentMonth": "string (required) — Short month name matching timeline/heatmap (e.g., 'Apr')"
  },
  "timeline": {
    "months": ["string[] (required) — Ordered short month names, e.g., ['Jan','Feb','Mar','Apr','May','Jun']"],
    "nowPosition": "number (required) — 0.0–1.0 relative X position of the NOW line"
  },
  "tracks": [
    {
      "id": "string (required) — Track identifier, e.g., 'M1'",
      "label": "string (required) — Track description, e.g., 'Chatbot & MS Role'",
      "color": "string (required) — CSS hex color for track line and markers, e.g., '#0078D4'",
      "milestones": [
        {
          "date": "string (required) — Display date label, e.g., 'Jan 12'",
          "type": "string (required) — One of: 'checkpoint', 'poc', 'production'",
          "position": "number (required) — 0.0–1.0 relative X position on timeline",
          "label": "string (optional) — Additional text after date, e.g., 'PoC', 'Prod (TBD)'"
        }
      ]
    }
  ],
  "heatmap": {
    "months": ["string[] (required) — Full month names for column headers, e.g., ['January','February','March','April']"],
    "categories": [
      {
        "name": "string (required) — Display name: 'Shipped' | 'In Progress' | 'Carryover' | 'Blockers'",
        "cssClass": "string (required) — CSS class prefix: 'ship' | 'prog' | 'carry' | 'block'",
        "emoji": "string (required) — Status emoji: '✅' | '🔄' | '🔁' | '🚫'",
        "items": {
          "<monthName>": ["string[] — List of work item names for this month"]
        }
      }
    ]
  }
}
```

### Entity Relationships

```
DashboardData (root)
├── 1:1  ProjectInfo
├── 1:1  TimelineConfig
│         └── 1:N  months (string list)
├── 1:N  MilestoneTrack
│         └── 1:N  Milestone
└── 1:1  HeatmapData
          ├── 1:N  months (string list)
          └── 1:N  HeatmapCategory
                    └── 1:N  Items (Dictionary<string, List<string>>)
```

### Storage

- **Storage engine:** Flat file (`data.json`) on local filesystem
- **Location:** `{ContentRootPath}/Data/data.json`
- **Access pattern:** Read-once at application startup (singleton service constructor)
- **Concurrency:** None — single user, single reader, no writes from the application
- **Backup/versioning:** Manual (user manages file copies or uses git)

### Validation Rules

| Field | Rule | Error Message |
|-------|------|---------------|
| `project.title` | Non-null, non-empty | "Missing required field: project.title" |
| `project.subtitle` | Non-null, non-empty | "Missing required field: project.subtitle" |
| `project.currentMonth` | Non-null, non-empty | "Missing required field: project.currentMonth" |
| `timeline.months` | Non-null, at least 1 element | "Missing required field: timeline.months" |
| `timeline.nowPosition` | 0.0 ≤ value ≤ 1.0 | "timeline.nowPosition must be between 0.0 and 1.0" |
| `tracks` | Non-null (may be empty) | "Missing required field: tracks" |
| `tracks[].milestones[].position` | 0.0 ≤ value ≤ 1.0 | "Milestone position must be between 0.0 and 1.0" |
| `tracks[].milestones[].type` | One of: checkpoint, poc, production | "Invalid milestone type: {value}" |
| `heatmap.categories` | Non-null (may be empty) | "Missing required field: heatmap.categories" |
| `heatmap.categories[].cssClass` | One of: ship, prog, carry, block | "Invalid cssClass: {value}" |

---

## API Contracts

This application has **no REST API, no GraphQL, no WebSocket endpoints**. It serves a single HTML page.

### HTTP Endpoints

| Method | Path | Response | Description |
|--------|------|----------|-------------|
| `GET` | `/` | `200 OK` with `text/html` | Full dashboard page (Static SSR) |
| `GET` | `/_framework/*` | Static files | Blazor framework assets (auto-generated) |
| `GET` | `/css/app.css` | Static file | Global stylesheet |

### Error Responses

| Condition | Rendered Output |
|-----------|----------------|
| `data.json` not found | Centered error message: "Error: data.json not found at {path}. Create a data.json file in the Data/ directory." |
| Invalid JSON syntax | Centered error message: "Error loading data.json: {JsonException.Message}" |
| Missing required field | Centered error message: "Error in data.json: Missing required field '{fieldName}'" |
| Valid data | Full dashboard renders normally |

Error messages render as a centered `<div>` with 18px red text on a white background within the 1920×1080 container. No stack traces are exposed.

### `Program.cs` Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>();

app.Run();
```

Port binding in `Properties/launchSettings.json`:
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5000"
    }
  }
}
```

---

## Infrastructure Requirements

### Runtime Environment

| Requirement | Specification |
|-------------|---------------|
| **Operating System** | Windows 10 or later |
| **SDK** | .NET 8.0.x (any patch version) |
| **Runtime** | ASP.NET Core 8.0 (included with SDK) |
| **Port** | `http://localhost:5000` (HTTP only, no TLS) |
| **Font** | Segoe UI (ships with Windows; Arial CSS fallback) |
| **Browser** | Microsoft Edge (latest) or Google Chrome (latest), 100% zoom |
| **Resolution** | Browser viewport set to 1920×1080 |

### Hosting

- **Server:** Kestrel (built into ASP.NET Core), localhost-only binding
- **Process:** `dotnet run` or `dotnet watch` from the project directory
- **Deployment:** None required — run from source. Optionally `dotnet publish -c Release -o ./publish` for a self-contained folder.
- **Infrastructure cost:** $0

### Networking

- No external network calls
- No CORS configuration
- No reverse proxy
- Binds to `localhost` only (not `0.0.0.0`)

### Storage

- Disk: `data.json` file (~1–100KB), project source files (~500KB total)
- Memory: <50MB process footprint (Kestrel + single page render)
- No database, no cache, no message queue

### CI/CD

Out of scope. No pipeline required. The application is run locally from source.

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **Framework** | Blazor Server (.NET 8 Static SSR) | Mandatory per requirements. Static SSR eliminates SignalR overhead for this zero-interactivity page. |
| **Render mode** | Static Server-Side Rendering | No `@rendermode` attribute needed — .NET 8 defaults to static SSR. No WebSocket connection, no reconnection UI, no Blazor chrome. |
| **CSS layout** | Pure CSS Grid + Flexbox | Matches the original HTML design exactly. CSS Grid for heatmap, Flexbox for header and timeline area. No framework overhead. |
| **SVG rendering** | Inline SVG in `.razor` markup | Timeline uses basic primitives (line, circle, polygon, text). Blazor emits SVG natively — no charting library needed. |
| **JSON parsing** | `System.Text.Json` | Built into .NET 8. Zero additional packages. Supports records, case-insensitive deserialization, nullable properties. |
| **CSS architecture** | Single global `app.css` with CSS custom properties | Simpler than component-scoped CSS for a small app. Custom properties enable single-point color changes. Class names match original design for traceability. |
| **Data storage** | Flat `data.json` file | Requirement-mandated. No database, no config files. Direct `File.ReadAllText` + deserialize. |
| **Project structure** | Single `.sln` / single `.csproj` | Mandatory per requirements. No need for class libraries — the app is too small to benefit from multi-project separation. |
| **Third-party UI libraries** | None (no MudBlazor, Radzen, etc.) | They override styles, add JS dependencies, and fight against pixel-perfect custom CSS. The design is simple enough for raw HTML/CSS. |
| **JavaScript** | None | All rendering is server-side. No JS interop, no client-side scripts, no npm packages. |

### Rejected Alternatives

| Alternative | Why Rejected |
|-------------|--------------|
| MudBlazor / Radzen | Adds style conflicts, JS dependencies, and bloat for a simple static page |
| Chart.js / ApexCharts | Requires JS interop; timeline SVG is trivial to hand-code in Blazor |
| Blazor WebAssembly | Adds 5–10MB download; unnecessary for server-rendered static content |
| Blazor Interactive Server | Establishes SignalR circuit; adds reconnection UI risk; zero interactivity needed |
| Newtonsoft.Json | Extra dependency; System.Text.Json is built-in and sufficient |
| Entity Framework | No database exists; data source is a JSON file |
| Razor Pages (non-Blazor) | Blazor components provide better composition for nested UI hierarchy |

---

## Security Considerations

### Threat Model

This is a **localhost-only, single-user, read-only** application with no sensitive data. The attack surface is minimal.

| Threat | Risk Level | Mitigation |
|--------|-----------|------------|
| **Network exposure** | None | Kestrel binds to `localhost` only; not accessible from network |
| **Authentication bypass** | N/A | No auth required — intentional design for local tool |
| **JSON injection** | Low | `System.Text.Json` deserializes into typed records; no raw string interpolation into SQL or HTML |
| **XSS via data.json** | Low | Blazor's Razor engine HTML-encodes all `@` expressions by default. Milestone labels and item names are safely encoded. |
| **Path traversal** | None | File path is hardcoded to `Data/data.json`; no user-supplied path input |
| **Denial of service** | None | Single local user; no concurrent access |
| **Supply chain** | Minimal | Zero third-party NuGet packages; only `Microsoft.AspNetCore.App` shared framework |
| **HTTPS** | Disabled | Intentional — localhost traffic only; no sensitive data in transit |
| **Secrets in data.json** | Low | `data.json` contains project names and milestones — organizational data, not PII or credentials |

### Input Validation

- `data.json` is validated at startup by `DashboardDataService.Validate()`.
- Malformed JSON produces a user-friendly error page, not an exception page.
- All string values rendered via Blazor's `@` syntax are automatically HTML-encoded.
- SVG attribute values (positions, colors) are emitted via `@` expressions and are numeric/hex — no script injection vector.

### Security Headers

Not configured — localhost-only tool does not benefit from CSP, HSTS, or X-Frame-Options. If future deployment to a network is needed, add middleware:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    await next();
});
```

---

## Scaling Strategy

### Current Scale

| Dimension | Value | Notes |
|-----------|-------|-------|
| Users | 1 | Single developer/PM on localhost |
| Pages | 1 | Single dashboard page |
| Data size | <100KB | One `data.json` file |
| Concurrent requests | 1 | Browser reload only |
| Render time | <300ms | Single SSR pass |

### This Application Does Not Need to Scale

By design, this is a local-only, single-user tool. There is no multi-user scenario, no concurrent access, no database contention, no network traffic beyond localhost. The architecture is intentionally simple and does not include scaling provisions.

### If Future Scaling Were Required

If the tool were ever adapted for multi-user network access (not currently in scope):

| Scenario | Approach |
|----------|----------|
| Multiple simultaneous viewers | Static SSR already handles this — each request renders independently with no shared state |
| Multiple project datasets | Add URL routing: `/project/{name}` loading from `Data/{name}.json` |
| Larger data volumes (>500 items) | Paginate heatmap cells or add scrollable overflow per cell |
| Faster iteration | Pre-render to static HTML file with `dotnet publish`; serve with any static file server |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component, its CSS layout strategy, data bindings, and interactions.

### Component-to-Visual Mapping

#### Section 1: Header → `DashboardHeader.razor`

| Visual Element | HTML/CSS | Data Binding | Interaction |
|---------------|----------|--------------|-------------|
| Project title | `<h1>` in `.hdr`, 24px bold | `@Project.Title` | None (static text) |
| ADO Backlog link | `<a href>` inline in H1, `#0078D4` | `@Project.BacklogUrl` | Clickable link (cosmetic for screenshots) |
| Subtitle | `<div class="sub">`, 12px `#888` | `@Project.Subtitle` | None |
| Legend: PoC diamond | `<span>` with 12×12px rotated square, `#F4B400` | None (static) | None |
| Legend: Prod diamond | `<span>` with 12×12px rotated square, `#34A853` | None (static) | None |
| Legend: Checkpoint | `<span>` with 8×8px circle, `#999` | None (static) | None |
| Legend: Now indicator | `<span>` with 2×14px bar, `#EA4335` | `"Now (@Project.CurrentMonth ...)"` | None |

**CSS Layout:** Flexbox row (`display: flex; justify-content: space-between; align-items: center`). Left side: title + subtitle stacked. Right side: legend items in flex row with 22px gap. Bottom border: 1px solid `#E0E0E0`.

**Padding:** 12px top, 44px horizontal, 10px bottom. `flex-shrink: 0`.

#### Section 2: Timeline → `TimelineSection.razor`

| Visual Element | HTML/CSS | Data Binding | Interaction |
|---------------|----------|--------------|-------------|
| Track labels sidebar | `<div>` 230px wide, flex column, `space-around` | `@foreach track in Tracks` → `track.Id`, `track.Label`, `track.Color` | None |
| SVG container | `<svg>` 1560×185, `overflow: visible` | — | None |
| Month divider lines | `<line>` at 260px intervals | `@foreach month in Timeline.Months` | None |
| Month labels | `<text>` at y=14, 11px | `@month` | None |
| NOW line | `<line>` dashed, `#EA4335`, stroke-width 2 | X = `Timeline.NowPosition * 1560` | None |
| NOW label | `<text>` "NOW", 10px bold `#EA4335` | Same X position | None |
| Track lines | `<line>` full width, stroke-width 3 | `track.Color`, Y = computed per track index | None |
| Checkpoint markers | `<circle>` r=7, white fill, colored stroke | `milestone.Position * 1560`, `milestone.Date` | None |
| PoC diamonds | `<polygon>` diamond, `#F4B400`, drop-shadow | `milestone.Position * 1560`, `milestone.Date + milestone.Label` | None |
| Production diamonds | `<polygon>` diamond, `#34A853`, drop-shadow | `milestone.Position * 1560`, `milestone.Date + milestone.Label` | None |
| Date labels | `<text>` 10px `#666`, text-anchor middle | `milestone.Date` + optional `milestone.Label` | None |

**CSS Layout:** Flexbox row. Left sidebar: 230px fixed width, flex-shrink 0, vertical flex column with `justify-content: space-around`, right border 1px `#E0E0E0`. Right SVG box: `flex: 1`, padding-left 12px, padding-top 6px.

**Container:** Height 196px, `flex-shrink: 0`, background `#FAFAFA`, border-bottom 2px solid `#E8E8E8`, padding 6px top 44px horizontal.

**SVG Coordinate Computation (in `@code` block):**

```csharp
private double GetMonthX(int index) => index * (1560.0 / Timeline.Months.Count);
private double GetNowX() => Timeline.NowPosition * 1560.0;
private double GetTrackY(int index, int total) => total == 1 ? 92 : 42 + index * ((185.0 - 42 - 31) / (total - 1));
private double GetMilestoneX(double position) => position * 1560.0;
private string GetDiamondPoints(double cx, double cy, double r = 11) =>
    $"{cx},{cy - r} {cx + r},{cy} {cx},{cy + r} {cx - r},{cy}";
```

#### Section 3: Heatmap → `HeatmapSection.razor` + `HeatmapRow.razor` + `HeatmapCell.razor`

| Visual Element | Component | HTML/CSS | Data Binding |
|---------------|-----------|----------|--------------|
| Section title | `HeatmapSection` | `<div class="hm-title">`, 14px uppercase `#888` | Static text with dynamic category names |
| Grid container | `HeatmapSection` | `<div class="hm-grid">` with CSS Grid | Column count from `Heatmap.Months.Count` |
| Corner cell ("STATUS") | `HeatmapSection` | `<div class="hm-corner">` | None (static label) |
| Month column headers | `HeatmapSection` | `<div class="hm-col-hdr">` per month | `@month`, current month gets `.apr-hdr` class |
| Row header (status label) | `HeatmapRow` | `<div class="hm-row-hdr {cssClass}-hdr">` | `Category.Emoji + Category.Name` |
| Data cells | `HeatmapCell` | `<div class="hm-cell {cssClass}-cell">` + `.current` | Items from `Category.Items[month]` |
| Work items | `HeatmapCell` | `<div class="it">` with `::before` dot | `@foreach item in Items` → `@item` |
| Empty cell dash | `HeatmapCell` | `<div>—</div>` in `#AAA` | When Items is null/empty |

**CSS Grid Layout:**

```css
.hm-grid {
    flex: 1;
    min-height: 0;
    display: grid;
    grid-template-columns: 160px repeat(var(--month-count), 1fr);
    grid-template-rows: 36px repeat(var(--category-count), 1fr);
    border: 1px solid #E0E0E0;
}
```

The `--month-count` and `--category-count` CSS variables are set via inline `style` on `.hm-grid` from the Blazor component:

```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@Heatmap.Months.Count, 1fr);
                             grid-template-rows: 36px repeat(@Heatmap.Categories.Count, 1fr);">
```

**Current Month Detection:** In `HeatmapSection`, each month column header is compared against `CurrentMonth`. The comparison uses case-insensitive `StartsWith` (e.g., "April".StartsWith("Apr")) to match short-form `currentMonth` in `data.json` against full month names in `heatmap.months`.

```csharp
private bool IsCurrentMonth(string fullMonthName) =>
    fullMonthName.StartsWith(CurrentMonth, StringComparison.OrdinalIgnoreCase);
```

**Heatmap Color Application:** Each row's CSS class prefix (`ship`, `prog`, `carry`, `block`) is applied dynamically:

```razor
<div class="hm-row-hdr @($"{Category.CssClass}-hdr")">
    @Category.Emoji @Category.Name.ToUpper()
</div>
@foreach (var month in Months)
{
    var isCurrent = IsCurrentMonth(month);
    <HeatmapCell Items="@GetItems(month)"
                 CssClass="@Category.CssClass"
                 IsCurrent="@isCurrent" />
}
```

### Full Component Tree with CSS Class Mapping

```
MainLayout.razor
│ style="width:1920px;height:1080px;overflow:hidden;display:flex;flex-direction:column"
│
├── DashboardHeader.razor ──── .hdr
│   ├── <h1> ──── .hdr h1
│   ├── <a> ──── color: #0078D4
│   ├── <div> ──── .sub
│   └── Legend <div> ──── flex row, gap: 22px
│       ├── PoC icon ──── 12px rotated square, #F4B400
│       ├── Prod icon ──── 12px rotated square, #34A853
│       ├── Checkpoint icon ──── 8px circle, #999
│       └── Now icon ──── 2×14px bar, #EA4335
│
├── TimelineSection.razor ──── .tl-area
│   ├── Labels sidebar ──── 230px, flex column
│   │   └── @foreach track ──── track.Id, track.Label (colored)
│   └── <svg> ──── .tl-svg-box, 1560×185
│       ├── <defs><filter id="sh"> ──── drop-shadow
│       ├── Month dividers ──── <line> at intervals
│       ├── Month labels ──── <text> at y=14
│       ├── NOW line ──── <line> dashed #EA4335
│       ├── NOW label ──── <text> "NOW"
│       └── @foreach track
│           ├── Track line ──── <line> full width, track.Color
│           └── @foreach milestone
│               ├── checkpoint ──── <circle> r=7, white fill
│               ├── poc ──── <polygon> diamond #F4B400
│               ├── production ──── <polygon> diamond #34A853
│               └── Date label ──── <text> 10px #666
│
└── HeatmapSection.razor ──── .hm-wrap
    ├── <div> ──── .hm-title
    └── <div> ──── .hm-grid (CSS Grid)
        ├── Corner cell ──── .hm-corner ("STATUS")
        ├── @foreach month ──── .hm-col-hdr (+ .apr-hdr if current)
        └── @foreach category → HeatmapRow.razor
            ├── Row header ──── .hm-row-hdr + .{css}-hdr
            └── @foreach month → HeatmapCell.razor
                └── .hm-cell + .{css}-cell (+ .current if current month)
                    ├── @foreach item ──── .it (with ::before dot)
                    └── or empty ──── "—" in #AAA
```

---

## Risks & Mitigations

| # | Risk | Severity | Probability | Impact | Mitigation |
|---|------|----------|-------------|--------|------------|
| 1 | **SVG milestone positions drift between Edge and Chrome** | Medium | Low | Milestone markers appear offset by 1–2px between browsers | Use integer pixel values for all SVG coordinates (not floating point). Test in both browsers. Accept ≤1px variance as within tolerance. |
| 2 | **Heatmap cells overflow with too many items** | Medium | Medium | Items in a cell with 10+ entries are clipped by `overflow: hidden` | Set max ~8 items per cell in documentation. Apply `overflow: hidden` (matches original design). Optionally reduce font-size to 11px for cells with >6 items. |
| 3 | **data.json schema change breaks existing data files** | Medium | Medium | Users updating the app see errors after code changes | Version the schema. Validate with fallback defaults for new optional fields. Document schema changes in README. |
| 4 | **Blazor Server reconnection UI appears in screenshots** | High | Low | Blue "Attempting to reconnect" banner ruins the screenshot | Use Static SSR (no `@rendermode`), which disables SignalR entirely. No circuit = no reconnection UI. Also add CSS to hide `.components-reconnect-modal` as a safety net. |
| 5 | **Segoe UI unavailable on non-Windows systems** | Low | Low | Font falls back to Arial; minor visual difference | CSS fallback chain: `'Segoe UI', Arial, sans-serif`. Documented as Windows-only tool. |
| 6 | **Hot reload does not reflect data.json changes** | Medium | High | PM edits JSON but browser still shows old data | `DashboardDataService` is a singleton — it reads at startup only. **Mitigation:** Re-read `data.json` on each request (trivial for <100KB file), or document that `dotnet watch` restart is needed after JSON edits. Recommend re-reading on each request for best DX. |
| 7 | **CSS Grid layout breaks with different month counts** | Medium | Medium | Changing from 4 to 6 months distorts column widths | Grid template uses dynamic `repeat(@count, 1fr)` — inherently flexible. Test with 3, 4, and 6 months. Document supported range (2–6 months). |
| 8 | **SVG track lines overlap with >3 tracks** | Low | Low | Track Y positions collide within 185px height | Compute Y positions dynamically: `42 + index * (available_height / (trackCount - 1))`. Document max 5 tracks. |

### Critical Design Decision: Data Reload Strategy

**Risk #6 above requires an architectural decision.** Two options:

**Option A: Singleton (read once at startup)**
- Pro: Simplest, fastest
- Con: Requires app restart to see JSON changes
- Implementation: Current `DashboardDataService` design

**Option B: Read on each request (recommended)**
- Pro: Edit JSON → refresh browser → see changes instantly. Best DX for the primary use case.
- Con: Reads file on every page load (~1ms for 100KB — negligible)
- Implementation: Change `DashboardDataService` to a scoped service, or keep singleton with a `Reload()` method called per-request.

**Recommendation:** Option B. Change the service to read `data.json` on each call to `GetDashboardData()` with a simple file read. The file is tiny (<100KB) and this eliminates the #1 friction point in the PM workflow.

```csharp
public class DashboardDataService
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public DashboardDataService(IWebHostEnvironment env)
    {
        _path = Path.Combine(env.ContentRootPath, "Data", "data.json");
    }

    public (DashboardData? Data, string? Error) Load()
    {
        try
        {
            if (!File.Exists(_path))
                return (null, $"data.json not found at: {_path}");
            var json = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<DashboardData>(json, _options);
            if (data is null)
                return (null, "data.json deserialized to null.");
            var error = Validate(data);
            return (error is null ? data : null, error);
        }
        catch (JsonException ex)
        {
            return (null, $"Invalid JSON in data.json: {ex.Message}");
        }
    }
}
```

---

## File Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── README.md
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── Properties/
    │   └── launchSettings.json
    ├── Data/
    │   └── data.json
    ├── Models/
    │   └── DashboardData.cs
    ├── Services/
    │   └── DashboardDataService.cs
    ├── Components/
    │   ├── App.razor
    │   ├── _Imports.razor
    │   ├── Layout/
    │   │   └── MainLayout.razor
    │   └── Pages/
    │       ├── Dashboard.razor
    │       ├── DashboardHeader.razor
    │       ├── TimelineSection.razor
    │       ├── HeatmapSection.razor
    │       ├── HeatmapRow.razor
    │       └── HeatmapCell.razor
    └── wwwroot/
        └── css/
            └── app.css
```

### File Responsibilities

| File | Purpose | Size Estimate |
|------|---------|---------------|
| `ReportingDashboard.sln` | Solution file | Auto-generated |
| `ReportingDashboard.csproj` | Project file — zero NuGet packages | ~10 lines |
| `Program.cs` | App bootstrap — register services, configure pipeline | ~15 lines |
| `launchSettings.json` | Set `http://localhost:5000`, disable HTTPS | ~15 lines |
| `data.json` | Sample project data (fictional) | ~3KB |
| `DashboardData.cs` | 8 record types for JSON deserialization | ~60 lines |
| `DashboardDataService.cs` | Load + validate `data.json` | ~50 lines |
| `App.razor` | HTML shell, `<head>`, CSS link, `<Routes>` | ~20 lines |
| `_Imports.razor` | Global `@using` statements | ~10 lines |
| `MainLayout.razor` | 1920×1080 container div | ~10 lines |
| `Dashboard.razor` | Page orchestrator, error display, data distribution | ~40 lines |
| `DashboardHeader.razor` | Title, subtitle, legend | ~40 lines |
| `TimelineSection.razor` | SVG timeline with tracks and milestones | ~120 lines |
| `HeatmapSection.razor` | Grid container, headers, row iteration | ~60 lines |
| `HeatmapRow.razor` | Status row header + cell iteration | ~30 lines |
| `HeatmapCell.razor` | Item list or empty dash | ~25 lines |
| `app.css` | Complete stylesheet with CSS custom properties | ~200 lines |
| `README.md` | Setup, usage, JSON schema, screenshot workflow | ~150 lines |

**Total estimated lines of code:** ~850 (excluding auto-generated files)