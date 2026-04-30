# Architecture

## Overview & Goals

The Reporting Dashboard is a single-page, read-only Blazor Server application that renders an executive project status view at a fixed 1920×1080 resolution, optimized for direct screenshot capture into PowerPoint slides. The system loads all display data from a single checked-in JSON configuration file and requires zero authentication, zero database, and zero cloud infrastructure.

**Primary Goals:**
1. Pixel-perfect rendering of a milestone timeline and execution heatmap matching the canonical HTML design reference
2. Zero-dependency local execution via `dotnet run`
3. Non-developer data updates through a single JSON file edit + app restart
4. Screenshot-ready output at exactly 1920×1080 with no scrollbars or loading artifacts

**Architecture Style:** Monolithic single-project Blazor Server application with a flat component hierarchy and file-based data loading. No layers, no abstractions beyond what is needed for clean code organization.

---

## System Components

### 1. `Program.cs` — Application Host

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure Kestrel, register DI services, map Blazor endpoints |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService`, ASP.NET Core hosting |
| **Data** | None directly; wires up service registration |

**Key behaviors:**
- Registers `DashboardDataService` as a Singleton
- Configures Kestrel to listen on `http://localhost:5000` and `https://localhost:5001`
- Maps Blazor Server hub and static files
- Catches and logs startup errors from JSON loading gracefully

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();
var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

---

### 2. `DashboardDataService` — Data Loading & Caching

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Load, validate, and cache the JSON configuration file at startup |
| **Interfaces** | `DashboardData GetDashboardData()` |
| **Dependencies** | `IWebHostEnvironment` (for `ContentRootPath`), `System.Text.Json` |
| **Data** | Reads `Data/dashboard-data.json`; caches deserialized `DashboardData` in memory |

**Key behaviors:**
- Reads file once during construction (Singleton lifetime = once per app start)
- Uses `JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip }`
- On `FileNotFoundException`: logs clear message with expected path, sets `Data` to null
- On `JsonException`: logs parse error with message and byte position, sets `Data` to null
- Exposes `bool HasError` and `string? ErrorMessage` for the page to render a fallback

```csharp
public class DashboardDataService
{
    public DashboardData? Data { get; }
    public bool HasError { get; }
    public string? ErrorMessage { get; }

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "dashboard-data.json");
        try
        {
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            Data = JsonSerializer.Deserialize<DashboardData>(json, options);
        }
        catch (FileNotFoundException)
        {
            ErrorMessage = $"Configuration file not found: {path}";
            HasError = true;
            logger.LogError(ErrorMessage);
        }
        catch (JsonException ex)
        {
            ErrorMessage = $"JSON parse error in {path}: {ex.Message}";
            HasError = true;
            logger.LogError(ErrorMessage);
        }
    }
}
```

---

### 3. `Dashboard.razor` — Page Component (Route: "/")

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Compose the three visual sections; inject data service; handle error state |
| **Interfaces** | Blazor route `/` |
| **Dependencies** | `DashboardDataService` (injected) |
| **Data** | Receives `DashboardData` from service, passes slices to child components |

**Key behaviors:**
- If `DashboardDataService.HasError`, renders a centered error message (development aid only)
- Otherwise renders `DashboardHeader`, `TimelineSection`, `HeatmapGrid` in a vertical flex column
- Sets `<meta name="viewport" content="width=1920">` in the page head

---

### 4. `DashboardHeader.razor` — Header Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render project title, ADO backlog link, subtitle, and legend icons |
| **Interfaces** | `[Parameter] DashboardData Data` |
| **Dependencies** | None (pure rendering) |
| **Data** | `Data.Title`, `Data.Subtitle`, `Data.BacklogUrl` |

---

### 5. `TimelineSection.razor` — Timeline Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render left sidebar with stream labels + right SVG area with timeline visualization |
| **Interfaces** | `[Parameter] List<MilestoneStream> Streams`, `[Parameter] DateTime CurrentDate`, `[Parameter] DateTime StartDate`, `[Parameter] DateTime EndDate` |
| **Dependencies** | None (pure rendering with calculation logic) |
| **Data** | Milestone streams, dates for position calculations |

**Key calculation:**
```csharp
private double GetXPosition(DateTime date)
{
    var totalDays = (EndDate - StartDate).TotalDays;
    var elapsed = (date - StartDate).TotalDays;
    return (elapsed / totalDays) * SvgWidth;
}

private string DiamondPoints(double cx, double cy, double size = 11)
{
    return $"{cx},{cy - size} {cx + size},{cy} {cx},{cy + size} {cx - size},{cy}";
}
```

**SVG elements generated:**
- Month grid lines (vertical, evenly spaced)
- Month labels (3-letter abbreviations)
- NOW dashed line (red, positioned by `currentDate`)
- Horizontal track lines (one per stream, colored)
- Checkpoint circles (white fill, colored stroke)
- Small checkpoint dots (gray fill, no stroke)
- PoC diamond polygons (amber `#F4B400`, with drop shadow)
- Production diamond polygons (green `#34A853`, with drop shadow)
- Milestone text labels (positioned above/below markers)

---

### 6. `HeatmapGrid.razor` — Heatmap Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the CSS Grid with month columns, category rows, and work item bullets |
| **Interfaces** | `[Parameter] List<HeatmapRow> Rows`, `[Parameter] List<string> Months`, `[Parameter] string CurrentMonth` |
| **Dependencies** | None (pure rendering) |
| **Data** | Heatmap rows with per-month item lists |

**Key behaviors:**
- Dynamically sets `--month-count` CSS variable via inline `style` attribute
- Applies current-month highlight class when `month == CurrentMonth`
- Maps category string to CSS class prefix (`shipped` → `ship`, `in-progress` → `prog`, `carryover` → `carry`, `blockers` → `block`)
- Renders dash (`–`) in muted color for empty cells

---

### 7. `MainLayout.razor` — Layout Shell

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Minimal HTML shell; links `dashboard.css`; sets viewport dimensions |
| **Interfaces** | Blazor layout component |
| **Dependencies** | None |
| **Data** | None |

**Key behaviors:**
- Sets `<body>` to `1920px × 1080px`, `overflow: hidden`
- Links single CSS file `css/dashboard.css`
- No navigation, no sidebar, no Blazor default chrome

---

### 8. `dashboard.css` — Stylesheet

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | All visual styling; CSS custom properties for color palette; print styles |
| **Interfaces** | CSS classes consumed by all Blazor components |
| **Dependencies** | System font (Segoe UI) |
| **Data** | None |

---

### 9. `Data/dashboard-data.json` — Configuration File

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Single source of truth for all dashboard display data |
| **Interfaces** | JSON file read by `DashboardDataService` |
| **Dependencies** | Must conform to JSON Schema (`Data/dashboard-data.schema.json`) |
| **Data** | Title, subtitle, URL, dates, milestone streams, heatmap months, heatmap rows |

---

### 10. `Data/dashboard-data.schema.json` — JSON Schema

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Provide IDE validation and auto-complete for the JSON config file |
| **Interfaces** | Referenced via `$schema` property in `dashboard-data.json` |
| **Dependencies** | JSON Schema Draft 2020-12 |
| **Data** | Schema definitions for all config properties |

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Startup                        │
└──────────────────────────────┬──────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│  DashboardDataService (Singleton)                                │
│  ┌───────────────────────┐                                       │
│  │ File.ReadAllText(      │◄── Data/dashboard-data.json          │
│  │   "Data/dashboard-     │                                      │
│  │    data.json")         │                                      │
│  └───────────┬───────────┘                                       │
│              │ JsonSerializer.Deserialize<DashboardData>()        │
│              ▼                                                    │
│  ┌───────────────────────┐                                       │
│  │ DashboardData (cached) │ ← In-memory for app lifetime         │
│  └───────────────────────┘                                       │
└──────────────────────────────┬──────────────────────────────────┘
                               │
                               │ @inject DashboardDataService
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│  Dashboard.razor (Page — route "/")                              │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │ DashboardHeader.razor                                     │    │
│  │ [Parameter] Data → renders title, subtitle, link, legend  │    │
│  └──────────────────────────────────────────────────────────┘    │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │ TimelineSection.razor                                     │    │
│  │ [Parameter] Streams, CurrentDate, StartDate, EndDate      │    │
│  │ → renders left sidebar labels + SVG timeline              │    │
│  └──────────────────────────────────────────────────────────┘    │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │ HeatmapGrid.razor                                         │    │
│  │ [Parameter] Rows, Months, CurrentMonth                    │    │
│  │ → renders CSS Grid with category rows × month columns     │    │
│  └──────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

### Communication Patterns

| From | To | Mechanism | Data |
|------|----|-----------|------|
| `Program.cs` | `DashboardDataService` | DI registration (Singleton) | Service lifetime |
| `DashboardDataService` | File system | `File.ReadAllText()` | JSON string |
| `Dashboard.razor` | `DashboardDataService` | `@inject` | `DashboardData` object |
| `Dashboard.razor` | Child components | `[Parameter]` binding | Typed data slices |
| Browser | Blazor Server | SignalR WebSocket | Initial render + circuit maintenance |

### Rendering Pipeline

1. Browser requests `http://localhost:5000/`
2. Kestrel serves the Blazor Server HTML shell (includes `blazor.server.js`)
3. SignalR connection established
4. `Dashboard.razor` `OnInitializedAsync` reads from injected `DashboardDataService`
5. Component tree renders: Header → Timeline → Heatmap
6. Complete HTML diff sent to browser over SignalR
7. Browser paints the page (target: < 2 seconds total)

---

## Data Model

### Entity Definitions

```csharp
public class DashboardData
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string BacklogUrl { get; set; } = "";
    public DateTime CurrentDate { get; set; }
    public DateTime TimelineStartDate { get; set; }
    public DateTime TimelineEndDate { get; set; }
    public List<MilestoneStream> MilestoneStreams { get; set; } = new();
    public List<string> HeatmapMonths { get; set; } = new();
    public string CurrentMonth { get; set; } = "";
    public List<HeatmapRow> HeatmapRows { get; set; } = new();
}

public class MilestoneStream
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Color { get; set; } = "";
    public List<Milestone> Milestones { get; set; } = new();
}

public class Milestone
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = "";
    public string Type { get; set; } = "";  // "checkpoint", "poc", "production"
    public string? Position { get; set; }    // "above" or "below" (label placement)
}

public class HeatmapRow
{
    public string Category { get; set; } = "";  // "shipped", "in-progress", "carryover", "blockers"
    public Dictionary<string, List<string>> Items { get; set; } = new();
}
```

### Relationships

```
DashboardData (1)
  ├── MilestoneStream (1..5)
  │     └── Milestone (0..N)
  └── HeatmapRow (exactly 4)
        └── Items[month] → List<string> (0..10 per cell)
```

### Storage

- **Runtime:** In-memory `DashboardData` object cached in Singleton service
- **Persistent:** `Data/dashboard-data.json` file checked into Git
- **No database.** No SQLite. No file-based DB. The JSON file IS the database.

### Constraints

| Rule | Enforcement |
|------|-------------|
| Exactly 4 heatmap categories | Fixed in code; JSON must provide all 4 rows |
| 3–6 heatmap months | Dynamic from JSON; grid columns adjust |
| 1–5 milestone streams | Dynamic from JSON; timeline tracks adjust |
| Max 10 items per cell | Soft limit (CSS `overflow: hidden` clips excess) |
| JSON file < 50KB | Operational guideline; no hard enforcement |

---

## API Contracts

This application has **no REST API**. It serves a single Blazor Server page. However, the following contracts define the system's interfaces:

### HTTP Endpoints (Kestrel)

| Endpoint | Method | Response | Purpose |
|----------|--------|----------|---------|
| `/` | GET | HTML (Blazor page) | Dashboard render |
| `/_blazor` | WebSocket | SignalR protocol | Blazor Server circuit |
| `/css/dashboard.css` | GET | CSS file | Stylesheet |
| `/_framework/blazor.server.js` | GET | JavaScript | Blazor runtime (built-in) |

### Configuration File Contract (JSON Schema)

**File:** `Data/dashboard-data.json`

**Required top-level properties:**

```json
{
  "$schema": "./dashboard-data.schema.json",
  "title": "string (required)",
  "subtitle": "string (required)",
  "backlogUrl": "string, valid URL (required)",
  "currentDate": "string, ISO 8601 date YYYY-MM-DD (required)",
  "timelineStartDate": "string, ISO 8601 date (required)",
  "timelineEndDate": "string, ISO 8601 date (required)",
  "milestoneStreams": "array of MilestoneStream (required, 1-5 items)",
  "heatmapMonths": "array of strings (required, 3-6 items)",
  "currentMonth": "string, must match one of heatmapMonths (required)",
  "heatmapRows": "array of HeatmapRow (required, exactly 4 items)"
}
```

**MilestoneStream schema:**
```json
{
  "id": "string (e.g., 'M1')",
  "label": "string (e.g., 'Chatbot & MS Role')",
  "color": "string, CSS hex color (e.g., '#0078D4')",
  "milestones": [
    {
      "date": "string, ISO 8601 date",
      "label": "string (display text near marker)",
      "type": "enum: 'checkpoint' | 'poc' | 'production'",
      "position": "enum: 'above' | 'below' (optional, default: 'above')"
    }
  ]
}
```

**HeatmapRow schema:**
```json
{
  "category": "enum: 'shipped' | 'in-progress' | 'carryover' | 'blockers'",
  "items": {
    "<month-abbrev>": ["string", "string", "..."]
  }
}
```

### Error Handling

| Error Condition | Behavior | User Impact |
|----------------|----------|-------------|
| Missing JSON file | Log error to console; render "Configuration file not found" message on page | Developer sees clear message in terminal |
| Invalid JSON syntax | Log parse error with position; render error message on page | Developer sees error details in terminal |
| Missing required field | `System.Text.Json` returns null for missing properties; page renders with empty data | Partial render (graceful degradation) |
| File > 50KB | No enforcement; may cause slow deserialization | Negligible (< 100ms even at 100KB) |

---

## Infrastructure Requirements

### Hosting

| Requirement | Specification |
|-------------|---------------|
| **Runtime** | .NET 8.0 SDK (LTS) |
| **Server** | Kestrel (built into ASP.NET Core) |
| **Ports** | HTTP: `5000`, HTTPS: `5001` |
| **OS** | Windows 10/11 (primary), macOS/Linux (secondary) |
| **Hardware** | Any modern developer machine; no minimum specs |

### Networking

- **Local only.** Binds to `localhost` by default.
- No ingress/egress rules required.
- No load balancer, reverse proxy, or CDN.
- No DNS configuration.
- Optional: change to `0.0.0.0:5000` in `launchSettings.json` for LAN access.

### Storage

- **Disk:** ~5MB for project files + NuGet cache (restored once)
- **Memory:** < 50MB runtime (Blazor Server + cached data model)
- **No database server.** No file-based DB. JSON file on filesystem.

### CI/CD

- **Not required for MVP.** This is a local development tool.
- Optional future: GitHub Actions with `dotnet build` + `dotnet test`
- No deployment pipeline needed (no target environment beyond developer laptops)

### Build & Run Commands

```bash
# First-time setup
git clone <repo-url>
cd src/ReportingDashboard
dotnet restore

# Run
dotnet run

# Publish self-contained (optional)
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
```

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **UI Framework** | Blazor Server (.NET 8) | Mandated stack. Renders server-side HTML via SignalR. No JS framework needed. |
| **CSS Layout** | Native CSS Grid + Flexbox | Exact 1:1 match with the HTML reference design. No CSS framework overhead. |
| **Timeline Rendering** | Hand-coded inline SVG in Razor | Full pixel-level control; zero JS dependencies; ~50 lines of rendering logic. Charting libraries rejected (500KB+ JS, less control). |
| **Data Format** | JSON | Native `System.Text.Json` in .NET 8; zero dependencies; easy to hand-edit; diffs cleanly in Git. YAML/XML/TOML all require third-party packages. |
| **Data Loading** | `File.ReadAllText` + `JsonSerializer.Deserialize` | Simplest possible approach for a single file. No `IOptions<T>` pattern, no configuration providers. |
| **DI Lifetime** | Singleton for `DashboardDataService` | Data is static per app start. Read once, serve to all circuits. |
| **CSS Architecture** | Single `dashboard.css` file with CSS custom properties | One file is sufficient for a single-page app. Custom properties enable easy color palette changes. |
| **Font** | Segoe UI (system font) | Pre-installed on Windows (target platform). No web font downloads. Zero latency. |
| **JavaScript** | None (beyond Blazor's built-in `blazor.server.js`) | No JS interop, no charting libraries, no third-party scripts. |
| **Testing** | Optional (bUnit + xUnit) | Not required for MVP. Architecture supports it via DI and parameterized components. |
| **Project Structure** | Single project, flat folders | Appropriate for a ~10-file application. Multi-layer architecture rejected as over-engineering. |

### Packages (Production)

| Package | Source | Notes |
|---------|--------|-------|
| `Microsoft.AspNetCore.App` | .NET 8 SDK (implicit) | Blazor Server, Kestrel, DI, logging |
| `System.Text.Json` | .NET 8 SDK (implicit) | JSON deserialization |

**Total third-party NuGet packages: 0**

---

## Security Considerations

### Authentication & Authorization

**None required.** This is explicitly a local development/screenshot tool with no sensitive data.

| Threat | Assessment | Mitigation |
|--------|------------|------------|
| Unauthorized access | N/A — localhost only | Default binding to `127.0.0.1` prevents network access |
| Data exposure | Low — project status only, no PII | Git repo permissions control who sees the JSON file |
| XSS via JSON data | Low — Blazor's Razor engine HTML-encodes all output by default | No `@((MarkupString)...)` usage; all data rendered through Blazor's encoding |
| CSRF | N/A — no forms, no mutations, no state changes | Read-only dashboard |
| Dependency supply chain | Minimal — zero third-party packages | Only framework packages from Microsoft |

### Input Validation

- JSON file is read server-side only (not user-uploaded)
- `System.Text.Json` rejects malformed JSON with descriptive errors
- All string values rendered through Blazor's built-in HTML encoding
- No SQL, no file path concatenation, no command execution from config values
- `backlogUrl` rendered as `href` attribute — Blazor encodes this safely; no `javascript:` protocol risk since it's developer-controlled data

### Data Protection

- No encryption at rest (unnecessary for non-sensitive project status data)
- No encryption in transit requirement (localhost HTTP is acceptable)
- HTTPS available via default Kestrel development certificate if desired
- No secrets in the JSON configuration file
- No credentials stored anywhere in the application

---

## Scaling Strategy

### Current Design (1–5 users)

This application is designed for **1–5 concurrent local users**. No scaling is needed or planned.

| Dimension | Capacity | Notes |
|-----------|----------|-------|
| Concurrent connections | 1–10 | Blazor Server holds one SignalR connection per client |
| Data volume | < 50KB JSON file | ~240 data items maximum (6 months × 4 categories × 10 items) |
| Render time | < 2 seconds | All data in memory; no I/O on render |
| Memory per connection | ~2MB | Blazor Server circuit overhead |

### If Scale Is Ever Needed (Future)

| Scenario | Solution |
|----------|----------|
| 10–50 concurrent viewers | Switch to Blazor Static SSR (removes SignalR). Add `@rendermode` directive. |
| Multiple projects | Add route parameter: `/{project}`. Load `Data/{project}.json`. |
| Frequent data updates | Add `FileSystemWatcher` on JSON file to reload without restart. |
| Non-Windows users | Font fallback to Arial already in CSS. No other OS-specific dependencies. |
| Public internet access | Add reverse proxy (YARP/nginx) + basic auth. Out of current scope. |

### What We Explicitly Do NOT Scale For

- High availability or redundancy
- Geographic distribution
- Database sharding or replication
- Microservice decomposition
- Container orchestration
- Auto-scaling

---

## Risks & Mitigations

### Risk 1: Scope Creep Toward Full PM Tool

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | High |
| **Impact** | High — could derail the project timeline |
| **Description** | Executives see polished dashboard and request real-time ADO integration, historical trends, multi-project views, automated data population |
| **Mitigation** | Document in README that this is a manual, config-file-driven screenshot tool. Future enhancements are separate work items with separate effort estimates. Architecture is intentionally simple to resist feature accumulation. |

### Risk 2: JSON Editing Errors by Non-Developers

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium |
| **Impact** | Low — caught immediately on restart |
| **Description** | Missing commas, unclosed brackets, wrong field names in `dashboard-data.json` |
| **Mitigation** | (1) JSON Schema file for VS Code auto-validation. (2) `ReadCommentHandling.Skip` allows `//` comments for documentation. (3) Clear console error messages on parse failure with file path and error position. (4) Sample file with extensive inline comments. |

### Risk 3: Blazor Server SignalR Overhead

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Low — only affects screenshot timing if circuit takes > 1s |
| **Description** | SignalR WebSocket is unnecessary for a read-only page with no interactivity |
| **Mitigation** | Negligible for local use. If problematic, switch to Static SSR render mode (add `@rendermode` attribute — a one-line change). No architectural rework needed. |

### Risk 4: SVG Rendering Differences Across Browsers

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Low — primary target is single browser (Edge) |
| **Description** | SVG text positioning and font metrics vary between browsers |
| **Mitigation** | Primary screenshot browser is Edge (Chromium). Segoe UI renders identically on all Windows + Edge combos. Document Edge as the "screenshot browser" in README. |

### Risk 5: Visual Regression After Data Changes

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium |
| **Impact** | Medium — long item text could overflow cells |
| **Description** | Changing data (longer item names, more items per cell) could break the fixed-height layout |
| **Mitigation** | CSS `overflow: hidden` on cells prevents layout breakage. Document max recommended items per cell (10) and max character length (~40 chars) in the JSON Schema description. |

### Risk 6: .NET 8 End of Support

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Certain (November 2026) |
| **Impact** | Low — trivial to upgrade |
| **Description** | .NET 8 LTS support ends November 2026; will need migration to .NET 10 |
| **Mitigation** | Zero third-party packages means upgrade is `<TargetFramework>net10.0</TargetFramework>` and `dotnet build`. No breaking changes expected for this simple app. |

---

## UI Component Architecture

This section maps each visual section from the `OriginalDesignConcept.html` design to specific Blazor components.

### Component-to-Visual Mapping

| Visual Section | Component | CSS Classes | Layout Strategy |
|----------------|-----------|-------------|-----------------|
| Header bar (`.hdr`) | `DashboardHeader.razor` | `.hdr`, `.sub` | Flexbox row, `justify-content: space-between` |
| Legend icons (right side of header) | Inside `DashboardHeader.razor` | Inline styles matching reference | Flex row, `gap: 22px` |
| Timeline area (`.tl-area`) | `TimelineSection.razor` | `.tl-area`, `.tl-svg-box` | Flexbox row, fixed 196px height |
| Timeline sidebar (230px labels) | Inside `TimelineSection.razor` | Inline styles | Flex column, `justify-content: space-around`, 230px fixed width |
| SVG timeline (month lines, tracks, markers) | Inside `TimelineSection.razor` | N/A (SVG attributes) | Inline `<svg>` with calculated positions |
| Heatmap wrapper (`.hm-wrap`) | `HeatmapGrid.razor` | `.hm-wrap`, `.hm-title` | Flex column, `flex: 1` fills remaining space |
| Heatmap grid (`.hm-grid`) | Inside `HeatmapGrid.razor` | `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell` | CSS Grid: `160px repeat(N, 1fr)` columns, `36px repeat(4, 1fr)` rows |
| Category row cells | Inside `HeatmapGrid.razor` | `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` | Grid cells with category-colored backgrounds |
| Item bullets | Inside `HeatmapGrid.razor` | `.it` with `::before` pseudo-element | Relative positioning; 6px colored circle at left |

### DashboardHeader.razor — Detail

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ [Title + Link]                                    [Legend: ◆ ◆ ● | icons]   │
│ [Subtitle]                                                                   │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Data bindings:**
- `@Data.Title` → `<h1>` text content
- `@Data.BacklogUrl` → `<a href="...">` attribute
- `@Data.Subtitle` → `.sub` div text

**CSS strategy:** Flexbox row with `padding: 12px 44px 10px`, `border-bottom: 1px solid #E0E0E0`.

**Interactions:** ADO Backlog link is a standard `<a>` tag — browser handles navigation. No Blazor event handlers.

### TimelineSection.razor — Detail

```
┌──────────┬──────────────────────────────────────────────────────────────────┐
│ M1 Label │  ┃Jan   ┃Feb   ┃Mar   ┃Apr ┊NOW┃May   ┃Jun                      │
│ M2 Label │  ═══●═══════════◆════════◆══════════════════════  (SVG tracks)   │
│ M3 Label │  ═══○═══●═══○○○○◆════════════◆═══════════════════               │
└──────────┴──────────────────────────────────────────────────────────────────┘
```

**Data bindings:**
- `@Streams[i].Id` and `@Streams[i].Label` → sidebar labels
- `@Streams[i].Color` → sidebar label color + SVG track stroke
- `@Streams[i].Milestones` → positioned SVG shapes
- `@CurrentDate` → NOW line X position
- `@StartDate` / `@EndDate` → X-axis scale

**CSS strategy:** Outer flex row (`.tl-area`). Left sidebar 230px fixed, right SVG area flex-grows. Height 196px, background `#FAFAFA`.

**SVG rendering logic:**
- Month columns: `svgWidth / numberOfMonths` spacing
- Track Y positions: evenly distributed across SVG height with padding
- Marker X positions: proportional date calculation
- Diamond size: 11px radius (22px diagonal)
- Drop shadow: SVG `<filter>` with `feDropShadow`

### HeatmapGrid.razor — Detail

```
┌──────────┬──────────┬──────────┬──────────┬──────────┐
│  STATUS  │   Jan    │   Feb    │   Mar    │  ★ Apr   │  ← Column headers
├──────────┼──────────┼──────────┼──────────┼──────────┤
│ SHIPPED  │ • Item   │ • Item   │ • Item   │ • Item   │  ← Green row
│ IN PROG  │ • Item   │ • Item   │ • Item   │ • Item   │  ← Blue row
│ CARRYOVR │ • Item   │ –        │ • Item   │ • Item   │  ← Amber row
│ BLOCKERS │ –        │ –        │ • Item   │ • Item   │  ← Red row
└──────────┴──────────┴──────────┴──────────┴──────────┘
```

**Data bindings:**
- `@Months` → column headers (dynamic count)
- `@CurrentMonth` → highlight class applied to matching column
- `@Rows[i].Category` → row header text + CSS class prefix
- `@Rows[i].Items[month]` → cell content (list of strings or empty → dash)

**CSS strategy:** CSS Grid with dynamic column count set via `style="--month-count: @Months.Count"`. Rows are `36px` header + 4 equal-height data rows filling remaining space.

**Dynamic class application:**
```razor
@{
    var cellClass = $"{categoryPrefix}-cell";
    if (month == CurrentMonth) cellClass += " apr";
}
<div class="hm-cell @cellClass">
    @if (items.Any())
    {
        @foreach (var item in items)
        {
            <div class="it">@item</div>
        }
    }
    else
    {
        <div class="it" style="color:#AAA;">–</div>
    }
</div>
```

### Print Styles

```css
@media print {
    * { -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; }
    body { width: 1920px; height: 1080px; }
    .hdr, .tl-area, .hm-wrap { break-inside: avoid; }
}
```

---

## Solution Structure

```
ReportingDashboard.sln
│
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj          Target: net8.0
│       ├── Program.cs                          Host configuration
│       ├── Data/
│       │   ├── dashboard-data.json            Configuration (checked in)
│       │   └── dashboard-data.schema.json     JSON Schema for IDE validation
│       ├── Models/
│       │   ├── DashboardData.cs               Root model
│       │   ├── MilestoneStream.cs             Timeline stream
│       │   ├── Milestone.cs                   Individual milestone marker
│       │   └── HeatmapRow.cs                  Heatmap category row
│       ├── Services/
│       │   └── DashboardDataService.cs        JSON loading + caching
│       ├── Components/
│       │   ├── App.razor                      Root component
│       │   ├── Routes.razor                   Router
│       │   ├── _Imports.razor                 Global usings
│       │   ├── Pages/
│       │   │   └── Dashboard.razor            Single page (route: "/")
│       │   ├── Layout/
│       │   │   └── MainLayout.razor           Minimal HTML shell
│       │   ├── DashboardHeader.razor          Header section
│       │   ├── TimelineSection.razor          SVG timeline section
│       │   └── HeatmapGrid.razor              CSS Grid heatmap section
│       ├── wwwroot/
│       │   └── css/
│       │       └── dashboard.css              All styles (single file)
│       └── Properties/
│           └── launchSettings.json            Dev server configuration
│
└── tests/ (optional)
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj
        └── ...                                 bUnit component tests
```

### `.csproj` Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <Content Include="Data\**" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
```

### `launchSettings.json`

```json
{
  "profiles": {
    "ReportingDashboard": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```