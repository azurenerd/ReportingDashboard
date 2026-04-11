# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server application (.NET 8) that renders a pixel-perfect 1920×1080 project status view from a local `data.json` file. The architecture is intentionally minimal: a JSON file reader, a strongly-typed data model, and a single Blazor component that produces HTML/SVG/CSS output optimized for browser screenshots destined for PowerPoint decks.

**Architecture Principles:**
1. **No unnecessary abstraction** — One implementation per concern; no interfaces for single implementations, no repository pattern, no mediator.
2. **File-driven data** — All dashboard content comes from `wwwroot/data.json`. No database, no API, no external services.
3. **Render fidelity over interactivity** — Every architectural decision optimizes for static visual output quality at 1920×1080.
4. **Zero-config operation** — `dotnet run` is the only command needed. No environment variables, no connection strings, no API keys.

**Data Flow:**
```
data.json → DashboardDataService → Dashboard.razor → HTML/SVG/CSS → Browser → Screenshot → PowerPoint
```

---

## System Components

### 1. Solution Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj          # .NET 8, Blazor Server
│       ├── Program.cs                          # Host builder, service registration
│       ├── Properties/
│       │   └── launchSettings.json             # Port configuration
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── app.css                     # Global reset + body styles
│       │   └── data.json                       # Dashboard data (user-editable)
│       ├── Models/
│       │   └── DashboardData.cs                # Record types for JSON deserialization
│       ├── Services/
│       │   └── DashboardDataService.cs         # File reader + deserializer
│       ├── Components/
│       │   ├── App.razor                       # Root component (<html>, <head>, <body>)
│       │   ├── Layout/
│       │   │   └── MainLayout.razor            # Minimal layout wrapper
│       │   └── Pages/
│       │       ├── Dashboard.razor             # Single page: header + timeline + heatmap
│       │       └── Dashboard.razor.css         # Scoped CSS (ported from OriginalDesignConcept.html)
│       └── appsettings.json                    # Minimal config (logging level only)
```

### 2. Component: `Program.cs` — Application Host

**Responsibility:** Configure the Blazor Server host, register services, map routes.

**Dependencies:** None external.

**Interface:**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<DashboardDataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
```

**Key Decisions:**
- `AddScoped<DashboardDataService>()` — Scoped lifetime ensures one data load per circuit. No singleton caching needed for a single-user local tool.
- No `AddAuthentication`, no `AddDbContext`, no `AddHttpClient` — nothing beyond the Blazor essentials.
- No HTTPS redirect — localhost HTTP is sufficient per requirements.

### 3. Component: `DashboardDataService` — Data Loading

**Responsibility:** Read `wwwroot/data.json`, deserialize into `DashboardData`, and return a structured error if the file is missing or malformed.

**Dependencies:** `IWebHostEnvironment` (injected, for resolving `WebRootPath`).

**Interface:**
```csharp
public class DashboardDataService
{
    private readonly string _dataPath;

    public DashboardDataService(IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.WebRootPath, "data.json");
    }

    public async Task<DashboardLoadResult> LoadAsync()
    {
        if (!File.Exists(_dataPath))
        {
            return DashboardLoadResult.NotFound(
                "Dashboard data not found. Please ensure data.json exists in the wwwroot folder.");
        }

        try
        {
            var json = await File.ReadAllTextAsync(_dataPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<DashboardData>(json, options);
            return DashboardLoadResult.Success(data!);
        }
        catch (JsonException ex)
        {
            return DashboardLoadResult.ParseError($"Error reading dashboard data: {ex.Message}");
        }
    }
}
```

**Result type:**
```csharp
public record DashboardLoadResult
{
    public bool IsSuccess { get; init; }
    public DashboardData? Data { get; init; }
    public string? ErrorMessage { get; init; }

    public static DashboardLoadResult Success(DashboardData data) =>
        new() { IsSuccess = true, Data = data };
    public static DashboardLoadResult NotFound(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
    public static DashboardLoadResult ParseError(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}
```

**Key Decisions:**
- Returns a result object rather than throwing exceptions — the Razor component can branch on `IsSuccess` to show either the dashboard or an error message.
- Reads the file on every call (no caching). For a local single-user tool, file I/O is sub-millisecond. Caching adds complexity for no measurable benefit.
- `PropertyNameCaseInsensitive = true` ensures JSON field casing flexibility.

### 4. Component: `DashboardData.cs` — Data Model

**Responsibility:** Strongly-typed C# records representing the full `data.json` schema.

**Dependencies:** None.

**Full Model:**
```csharp
public record DashboardData
{
    public HeaderData Header { get; init; } = new();
    public TimelineData Timeline { get; init; } = new();
    public HeatmapData Heatmap { get; init; } = new();
}

public record HeaderData
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogUrl { get; init; } = "#";
    public string BacklogLabel { get; init; } = "ADO Backlog";
}

public record TimelineData
{
    public string StartDate { get; init; } = "";    // ISO 8601: "2026-01-01"
    public string EndDate { get; init; } = "";      // ISO 8601: "2026-06-30"
    public List<MilestoneTrack> Tracks { get; init; } = new();
}

public record MilestoneTrack
{
    public string Id { get; init; } = "";            // e.g., "M1"
    public string Label { get; init; } = "";         // e.g., "Chatbot & MS Role"
    public string Color { get; init; } = "#0078D4";  // Track line color
    public List<MilestoneMarker> Markers { get; init; } = new();
}

public record MilestoneMarker
{
    public string Date { get; init; } = "";          // ISO 8601: "2026-03-26"
    public string Label { get; init; } = "";         // e.g., "Mar 26 PoC"
    public string Type { get; init; } = "checkpoint"; // "checkpoint" | "poc" | "production" | "dot"
    public string LabelPosition { get; init; } = "above"; // "above" | "below"
}

public record HeatmapData
{
    public List<TimePeriodColumn> Periods { get; init; } = new();
    public StatusRow Shipped { get; init; } = new();
    public StatusRow InProgress { get; init; } = new();
    public StatusRow Carryover { get; init; } = new();
    public StatusRow Blockers { get; init; } = new();
}

public record TimePeriodColumn
{
    public string Label { get; init; } = "";       // e.g., "Jan", "Feb", "Mar", "Apr"
    public bool IsCurrent { get; init; } = false;  // Highlights column with gold header
}

public record StatusRow
{
    public string Emoji { get; init; } = "";       // e.g., "✅", "🔄", "📋", "🚫"
    public List<List<string>> ItemsByPeriod { get; init; } = new();
    // ItemsByPeriod[0] = items for first period, [1] = second period, etc.
    // Each string is a short work item label (< 60 chars)
}
```

**Key Decisions:**
- All properties use `init` setters with defaults — missing JSON fields produce empty/default values rather than null reference exceptions.
- `ItemsByPeriod` is a `List<List<string>>` (not a dictionary) — preserves ordering and maps directly to grid column indices.
- `MilestoneMarker.Type` is a string enum (`"checkpoint"`, `"poc"`, `"production"`, `"dot"`) — the Razor component switches on this to render different SVG shapes.
- Date strings are ISO 8601 — parsed in the component's `@code` block using `DateOnly.Parse()` for X-position calculations.

### 5. Component: `Dashboard.razor` — The Single Page

**Responsibility:** Render the complete dashboard in three sections: Header, Timeline SVG, and Heatmap Grid.

**Dependencies:** `DashboardDataService` (injected via `@inject`).

**Structure:**
```razor
@page "/"
@inject DashboardDataService DataService

@if (_error is not null)
{
    <div class="error-container">
        <p class="error-message">@_error</p>
    </div>
}
else if (_data is not null)
{
    <!-- Section 1: Header -->
    <div class="hdr">...</div>

    <!-- Section 2: Timeline -->
    <div class="tl-area">
        <div class="tl-sidebar">...</div>
        <div class="tl-svg-box">
            <svg xmlns="http://www.w3.org/2000/svg" width="1560" height="185"
                 style="overflow:visible;display:block">
                @* Month grid lines, track lines, markers, NOW line *@
            </svg>
        </div>
    </div>

    <!-- Section 3: Heatmap -->
    <div class="hm-wrap">
        <div class="hm-title">...</div>
        <div class="hm-grid">
            @* Corner, column headers, row headers, data cells *@
        </div>
    </div>
}

@code {
    private DashboardData? _data;
    private string? _error;

    // Computed from Timeline.StartDate / EndDate
    private DateOnly _timelineStart;
    private DateOnly _timelineEnd;
    private int _totalDays;
    private const double SvgWidth = 1560.0;

    protected override async Task OnInitializedAsync()
    {
        var result = await DataService.LoadAsync();
        if (result.IsSuccess)
        {
            _data = result.Data;
            _timelineStart = DateOnly.Parse(_data!.Timeline.StartDate);
            _timelineEnd = DateOnly.Parse(_data.Timeline.EndDate);
            _totalDays = _timelineEnd.DayNumber - _timelineStart.DayNumber;
        }
        else
        {
            _error = result.ErrorMessage;
        }
    }

    private double DateToX(string isoDate)
    {
        var date = DateOnly.Parse(isoDate);
        var dayOffset = date.DayNumber - _timelineStart.DayNumber;
        return (dayOffset / (double)_totalDays) * SvgWidth;
    }

    private double NowX()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var dayOffset = today.DayNumber - _timelineStart.DayNumber;
        return Math.Clamp((dayOffset / (double)_totalDays) * SvgWidth, 0, SvgWidth);
    }
}
```

**Key Decisions:**
- All logic in `@code` block, no code-behind file — per PM spec's maintainability requirement.
- `OnInitializedAsync` loads data once. No `OnAfterRenderAsync`, no JS interop, no SignalR callbacks.
- `DateToX()` helper converts ISO date strings to SVG X coordinates using proportional mapping: `x = (daysSinceStart / totalDays) * 1560`.
- `NowX()` uses `DateTime.Now` to auto-position the red dashed "NOW" line on each page load.
- Error state and data state are mutually exclusive — the component renders either the error message or the dashboard, never both.

### 6. Component: `Dashboard.razor.css` — Scoped Styles

**Responsibility:** All visual styling for the dashboard, ported directly from `OriginalDesignConcept.html`.

**Key Decisions:**
- Use Blazor CSS isolation (automatic via file naming convention) — styles are scoped to the component, no global leakage.
- Port the original CSS verbatim with minimal changes:
  - Remove `body` styles (those go in `app.css` as global)
  - All `.hdr`, `.tl-area`, `.hm-wrap`, `.hm-grid`, `.hm-cell`, `.it`, and row-category classes are preserved exactly
- The `::deep` combinator is not needed — all elements are within the single component.

### 7. Component: `app.css` — Global Styles

**Responsibility:** CSS reset and body-level styles.

```css
*, *::before, *::after {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
    background: #FFFFFF;
    font-family: 'Segoe UI', Arial, sans-serif;
    color: #111;
    display: flex;
    flex-direction: column;
}

a {
    color: #0078D4;
    text-decoration: none;
}
```

### 8. Component: `App.razor` — Root Component

**Responsibility:** Define the HTML document shell (`<html>`, `<head>`, `<body>`) and reference stylesheets.

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=1920" />
    <title>Executive Reporting Dashboard</title>
    <link rel="stylesheet" href="css/app.css" />
    <link rel="stylesheet" href="ReportingDashboard.styles.css" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.server.js"></script>
</body>
</html>
```

### 9. Component: `MainLayout.razor` — Layout Wrapper

**Responsibility:** Minimal pass-through layout with no chrome (no nav, no sidebar).

```razor
@inherits LayoutComponentBase
@Body
```

---

## Component Interactions

### Request Lifecycle

```
Browser GET /  →  Blazor Server  →  Dashboard.razor.OnInitializedAsync()
                                          │
                                          ▼
                                  DashboardDataService.LoadAsync()
                                          │
                                          ▼
                                  File.ReadAllTextAsync("wwwroot/data.json")
                                          │
                                          ▼
                                  JsonSerializer.Deserialize<DashboardData>()
                                          │
                                          ▼
                                  DashboardLoadResult (Success or Error)
                                          │
                                          ▼
                                  Dashboard.razor renders HTML/SVG/CSS
                                          │
                                          ▼
                                  SignalR pushes DOM to browser
                                          │
                                          ▼
                                  Browser renders at 1920x1080
```

### Interaction Matrix

| Component | Depends On | Communicates With | Protocol |
|-----------|-----------|-------------------|----------|
| `Program.cs` | .NET 8 SDK | Blazor pipeline | Host builder API |
| `DashboardDataService` | `IWebHostEnvironment` | File system (`wwwroot/data.json`) | `System.IO` |
| `Dashboard.razor` | `DashboardDataService` | Blazor render tree | `@inject`, `await` |
| `App.razor` | Blazor framework | `Dashboard.razor` via routing | `<Routes />` |
| Browser | Blazor Server | SignalR hub | WebSocket |

### Error Flow

```
data.json missing  →  DashboardLoadResult.NotFound()  →  _error set  →  Error div rendered
data.json malformed  →  JsonException caught  →  DashboardLoadResult.ParseError()  →  Error div rendered
data.json valid  →  DashboardLoadResult.Success()  →  _data set  →  Dashboard rendered
```

---

## Data Model

### `data.json` Schema

```json
{
  "header": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "backlogLabel": "→ ADO Backlog"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "label": "Chatbot & MS Role",
        "color": "#0078D4",
        "markers": [
          { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint", "labelPosition": "above" },
          { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc", "labelPosition": "below" },
          { "date": "2026-05-01", "label": "Apr Prod (TBD)", "type": "production", "labelPosition": "above" }
        ]
      },
      {
        "id": "M2",
        "label": "PDS & Data Inventory",
        "color": "#00897B",
        "markers": [
          { "date": "2025-12-19", "label": "Dec 19", "type": "checkpoint", "labelPosition": "above" },
          { "date": "2026-02-11", "label": "Feb 11", "type": "checkpoint", "labelPosition": "above" },
          { "date": "2026-03-10", "label": "", "type": "dot", "labelPosition": "above" },
          { "date": "2026-03-15", "label": "", "type": "dot", "labelPosition": "above" },
          { "date": "2026-03-20", "label": "", "type": "dot", "labelPosition": "above" },
          { "date": "2026-03-22", "label": "", "type": "dot", "labelPosition": "above" },
          { "date": "2026-03-24", "label": "Mar 24 PoC", "type": "poc", "labelPosition": "above" }
        ]
      },
      {
        "id": "M3",
        "label": "Auto Review DFD",
        "color": "#546E7A",
        "markers": []
      }
    ]
  },
  "heatmap": {
    "periods": [
      { "label": "Jan", "isCurrent": false },
      { "label": "Feb", "isCurrent": false },
      { "label": "Mar", "isCurrent": false },
      { "label": "Apr", "isCurrent": true }
    ],
    "shipped": {
      "emoji": "✅",
      "itemsByPeriod": [
        ["Item A completed", "Item B completed"],
        ["Item C completed"],
        ["Item D completed", "Item E completed", "Item F completed"],
        ["Item G completed"]
      ]
    },
    "inProgress": {
      "emoji": "🔄",
      "itemsByPeriod": [
        ["Feature X started"],
        ["Feature X continued", "Feature Y started"],
        ["Feature Y continued"],
        ["Feature Z in dev", "Feature AA in review"]
      ]
    },
    "carryover": {
      "emoji": "📋",
      "itemsByPeriod": [
        [],
        ["Delayed item 1"],
        ["Delayed item 1", "Delayed item 2"],
        ["Delayed item 2"]
      ]
    },
    "blockers": {
      "emoji": "🚫",
      "itemsByPeriod": [
        [],
        [],
        ["Dependency on Team X"],
        ["Dependency on Team X", "Waiting on legal review"]
      ]
    }
  }
}
```

### Entity Relationships

```
DashboardData
├── HeaderData (1:1)
├── TimelineData (1:1)
│   └── MilestoneTrack[] (1:N, max ~5)
│       └── MilestoneMarker[] (1:N, max ~10 per track)
└── HeatmapData (1:1)
    ├── TimePeriodColumn[] (1:N, exactly 4)
    ├── StatusRow: Shipped (1:1)
    │   └── ItemsByPeriod: List<List<string>> (exactly 4 lists)
    ├── StatusRow: InProgress (1:1)
    ├── StatusRow: Carryover (1:1)
    └── StatusRow: Blockers (1:1)
```

### Storage

- **Format:** Single JSON file at `wwwroot/data.json`
- **Size constraint:** Up to 500 KB (per NFR)
- **Access pattern:** Read-only from application; hand-edited by project managers
- **Versioning:** Git-tracked for historical snapshots
- **No database.** No SQLite, no LiteDB, no EF Core. A flat file is the correct choice for this use case.

---

## API Contracts

This application has **no API endpoints**. It is a server-rendered Blazor application with a single page route.

### Route: `/`

| Property | Value |
|----------|-------|
| Method | GET (initial page load via HTTP), then WebSocket (SignalR) |
| Response | Full HTML page with embedded SVG and CSS |
| Content-Type | `text/html` |
| Authentication | None |
| Authorization | None |

### Internal Service Contract

```csharp
// DashboardDataService.LoadAsync()
// Input: None (reads from pre-configured file path)
// Output: DashboardLoadResult

public record DashboardLoadResult
{
    public bool IsSuccess { get; init; }
    public DashboardData? Data { get; init; }      // Non-null when IsSuccess = true
    public string? ErrorMessage { get; init; }      // Non-null when IsSuccess = false
}
```

### Error Responses (Rendered in HTML, not HTTP status codes)

| Condition | Displayed Message |
|-----------|-------------------|
| `data.json` missing | "Dashboard data not found. Please ensure data.json exists in the wwwroot folder." |
| `data.json` malformed | "Error reading dashboard data: {JsonException.Message}" |
| Empty heatmap cells | Dash placeholder "–" in muted gray (#AAA) |

---

## Infrastructure Requirements

### Development Environment

| Requirement | Specification |
|-------------|---------------|
| .NET SDK | 8.0.x (LTS) |
| IDE | Visual Studio 2022 or VS Code + C# Dev Kit |
| OS | Windows (primary — Segoe UI font); macOS/Linux (functional with Arial fallback) |
| Browser | Microsoft Edge or Google Chrome, latest stable |
| Network | None required — fully offline operation |

### Runtime Environment

| Requirement | Specification |
|-------------|---------------|
| RAM | < 100 MB (per NFR) |
| Disk | < 100 MB (self-contained publish ~70 MB) |
| CPU | Minimal; single-user, no computation-heavy workloads |
| Network | Localhost only; no external network access |
| Port | Default `http://localhost:5000` (configurable in `launchSettings.json`) |

### Build & Publish

**Development:**
```bash
dotnet watch run
```

**Release build (self-contained):**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

**Output:** Single `.exe` file (~70 MB) that runs on any Windows machine without .NET installed. The `wwwroot/data.json` file is embedded but can be overridden by placing a `data.json` alongside the executable (using `IWebHostEnvironment.WebRootPath` resolution).

### `ReportingDashboard.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

No NuGet packages. Zero external dependencies.

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

**Key Decision:** HTTP only, no HTTPS. This is a localhost-only tool — TLS adds certificate management complexity for zero security benefit.

### CI/CD

Not required for MVP. If added later:
- `dotnet build` → `dotnet test` → `dotnet publish`
- Single GitHub Actions workflow, Windows runner
- Artifact: self-contained `.exe` attached to release

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| Runtime | .NET 8 LTS | Mandatory per project constraints. LTS support through Nov 2026. |
| Web framework | Blazor Server | Aligns with existing .NET ecosystem. Server-side rendering avoids WASM download. SignalR overhead is negligible for single-user local tool. |
| Project template | `blazorserver-empty` | Minimal starting point — no sample pages, no Weather API, no Bootstrap. |
| Data format | JSON flat file | Correct complexity level. No database needed for a single config file read on page load. |
| JSON library | `System.Text.Json` | Built into .NET 8, zero NuGet dependency. Handles the simple schema with no custom converters. |
| Data models | C# `record` types | Immutable, value equality, concise syntax. Perfect for read-only deserialized config. |
| Timeline rendering | Inline SVG via Blazor markup | Full pixel-level control. The original design is ~40 lines of SVG — a charting library would add 2MB+ for zero benefit. |
| Heatmap rendering | CSS Grid | Matches original design exactly. 5-column grid with `grid-template-columns: 160px repeat(4, 1fr)`. |
| Layout system | CSS Flexbox + Grid | Original design uses both. No CSS framework needed (no Bootstrap, no Tailwind). |
| CSS architecture | Blazor CSS isolation | `Dashboard.razor.css` scopes automatically. No preprocessor (SASS/LESS) needed. |
| Typography | Segoe UI (system font) | No web font loading. Available on all Windows machines (primary target). Arial fallback for non-Windows. |
| Charting library | **None** | Explicitly rejected Radzen, MudBlazor, ApexCharts, BlazorD3. The visualization is simple geometric shapes — libraries add complexity without value. |
| JavaScript interop | **None** | Zero JS. The original design uses no JS; the Blazor implementation should not introduce any. |
| Testing | xUnit (optional) | Unit test `DashboardDataService` and date-to-X-position calculation. bUnit for component rendering (optional). |
| Package manager | NuGet (none added) | Zero external NuGet packages. All functionality is in the .NET 8 base class library. |

---

## Security Considerations

### Threat Model

This application has a **minimal attack surface** by design:

| Vector | Risk | Mitigation |
|--------|------|------------|
| Network exposure | Low | Binds to `localhost` only. Not accessible from other machines. |
| User input | None | No forms, no query parameters, no user-supplied data from the browser. |
| File injection via `data.json` | Low | JSON is deserialized into typed records. Blazor's Razor engine HTML-encodes all `@variable` output by default, preventing XSS even if malicious strings exist in the JSON. |
| SignalR WebSocket | Low | Default Blazor Server configuration. No custom hub endpoints. Local-only access. |
| Dependency supply chain | None | Zero NuGet packages beyond the .NET 8 SDK itself. |
| Data confidentiality | N/A | Dashboard data is local files on the user's own machine. No network transmission of project data. |

### Input Validation

The only "input" is `data.json`, which is validated as follows:

1. **File existence check** — `File.Exists()` before attempting to read.
2. **JSON schema validation** — `System.Text.Json` deserialization fails with `JsonException` on malformed JSON. The exception message is displayed to the user.
3. **Default values** — All record properties have defaults. Missing fields produce empty strings or empty lists, not null reference exceptions.
4. **HTML encoding** — Blazor's Razor syntax (`@variable`) automatically HTML-encodes output. If `data.json` contains `<script>alert('xss')</script>` as an item title, it renders as literal text, not executable HTML.

### No Authentication / Authorization

Intentional. This is a single-user, localhost-only tool. Adding auth would violate the "zero operational overhead" business goal. If network exposure is ever needed (not in scope), add `[Authorize]` and ASP.NET Core Identity at that time.

---

## Scaling Strategy

### Current Scale (By Design)

| Dimension | Current | Notes |
|-----------|---------|-------|
| Users | 1 | Single user on localhost |
| Pages | 1 | Single dashboard page |
| Data sources | 1 | Single `data.json` file |
| Projects | 1 | One dashboard instance per project |
| Concurrent connections | 1 | Blazor Server SignalR circuit |

### This Application Does Not Need to Scale

The architecture is intentionally optimized for simplicity, not scalability. The business requirements explicitly state:
- Local-only operation
- Single user at a time
- No concurrent access scenarios
- No multi-project switching

### If Scale Were Ever Needed (Future, Out of Scope)

| Scenario | Approach |
|----------|----------|
| Multiple projects | Support `data.{project}.json` files with a route parameter: `/{project}` |
| Multiple users | Switch to Blazor Static SSR (`@rendermode="Static"`) to eliminate SignalR per-circuit overhead |
| Automated screenshots | Add Playwright script: `playwright screenshot http://localhost:5000 dashboard.png --viewport-size="1920,1080"` |
| Real-time updates | Add `FileSystemWatcher` to hot-reload `data.json` changes without browser refresh |

---

## Risks & Mitigations

### Risk 1: Over-Engineering (HIGH probability)

**Description:** The biggest risk is adding unnecessary abstraction — interfaces for single implementations, repository patterns, mediator, DI containers beyond basics, or splitting the single component into sub-components prematurely.

**Impact:** Increased development time, cognitive load, and maintenance burden with zero user value.

**Mitigation:**
- Enforce the rule: if a component has one implementation, do not create an interface for it.
- Keep all dashboard logic in a single `Dashboard.razor` file with `@code` block.
- The service layer is one class (`DashboardDataService`) with one method (`LoadAsync`).
- No sub-components until a single component exceeds ~300 lines of markup.

### Risk 2: SVG Rendering Inconsistency Across Browsers (MEDIUM impact)

**Description:** SVG drop shadows, text positioning, and font metrics vary between Chrome, Edge, and Firefox. This could produce visual differences from the reference design.

**Impact:** Screenshots may not match the `OriginalDesignConcept.html` reference pixel-for-pixel.

**Mitigation:**
- Target Microsoft Edge as the primary browser (Microsoft shop).
- Test at exactly 1920×1080 using Chrome DevTools device toolbar.
- Use the `<feDropShadow>` filter from the original design verbatim.
- Pin `font-family: 'Segoe UI', Arial, sans-serif` which renders identically on Windows across Edge and Chrome.

### Risk 3: `data.json` Schema Drift (LOW impact)

**Description:** As the dashboard evolves, JSON fields may be added that don't match the C# model, or existing fields may be renamed.

**Impact:** Silent data loss (new fields ignored) or deserialization failures (field renames).

**Mitigation:**
- `PropertyNameCaseInsensitive = true` handles casing variations.
- All record properties have default values — missing fields don't crash.
- Document the full JSON schema in the README with field descriptions and examples.
- Version the schema: add a `"schemaVersion": 1` field for future migration detection.

### Risk 4: Self-Contained Publish Size (~70 MB) (LOW impact)

**Description:** A self-contained single-file publish of a .NET 8 Blazor Server app is approximately 70 MB due to the bundled runtime.

**Impact:** Large file to share with colleagues. May trigger email attachment limits.

**Mitigation:**
- Use `PublishTrimmed=true` to reduce size to ~30-40 MB (test that trimming doesn't break `System.Text.Json` reflection-based deserialization — may need `[JsonSerializable]` source generator).
- Distribute via file share or Teams, not email.
- Acceptable trade-off: a 70 MB exe that runs anywhere vs. requiring .NET 8 SDK installation.

### Risk 5: Blazor Server SignalR Overhead (LOW impact)

**Description:** Blazor Server maintains a WebSocket connection for a page that is essentially static after initial render.

**Impact:** Negligible for single-user local use (~2 MB RAM overhead).

**Mitigation:**
- Accept for MVP. If ever a concern, switch to `@rendermode InteractiveServer` with `prerender: true`, or use Static SSR mode entirely.
- The SignalR connection provides one benefit: if we later add a `FileSystemWatcher` for hot-reloading `data.json`, the existing WebSocket can push updates to the browser without polling.

### Risk 6: Heatmap Cell Overflow with Long Text (MEDIUM probability)

**Description:** If work item text exceeds ~60 characters, it may overflow the heatmap cell, breaking the grid layout.

**Impact:** Visual overflow or text truncation in screenshots.

**Mitigation:**
- CSS: `overflow: hidden` on `.hm-cell` (already in original design).
- Document the 60-character recommendation in the `data.json` schema.
- Optionally add `text-overflow: ellipsis; white-space: nowrap` as a CSS fallback, though multi-line items are preferred for readability.

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to specific Blazor markup, CSS layout strategy, data bindings, and interactions — all within the single `Dashboard.razor` component.

### Section 1: Header (`.hdr`)

| Property | Value |
|----------|-------|
| **Visual reference** | `.hdr` div in `OriginalDesignConcept.html` |
| **Blazor markup** | `<div class="hdr">` block in `Dashboard.razor` |
| **CSS layout** | `display: flex; align-items: center; justify-content: space-between; padding: 12px 44px 10px; border-bottom: 1px solid #E0E0E0;` |
| **Data bindings** | `@_data.Header.Title`, `@_data.Header.Subtitle`, `@_data.Header.BacklogUrl`, `@_data.Header.BacklogLabel` |
| **Interactions** | None (static render). Backlog link is a standard `<a href>`. |

**Markup Structure:**
```razor
<div class="hdr">
    <div>
        <h1>@_data.Header.Title <a href="@_data.Header.BacklogUrl">@_data.Header.BacklogLabel</a></h1>
        <div class="sub">@_data.Header.Subtitle</div>
    </div>
    <div class="legend">
        <!-- Four legend items: PoC diamond, Production diamond, Checkpoint circle, Now line -->
        <!-- These are static HTML with CSS shapes — not data-driven -->
    </div>
</div>
```

**Legend sub-elements (static, not data-driven):**
- PoC Milestone: `<span>` with 12×12px gold (#F4B400) square, `transform: rotate(45deg)`
- Production Release: `<span>` with 12×12px green (#34A853) square, `transform: rotate(45deg)`
- Checkpoint: `<span>` with 8×8px circle, background #999
- Now indicator: `<span>` with 2×14px bar, background #EA4335

### Section 2: Timeline Sidebar (`.tl-area` left panel)

| Property | Value |
|----------|-------|
| **Visual reference** | 230px left sidebar in `.tl-area` |
| **CSS layout** | `width: 230px; flex-shrink: 0; display: flex; flex-direction: column; justify-content: space-around; padding: 16px 12px 16px 0; border-right: 1px solid #E0E0E0;` |
| **Data bindings** | `@foreach (var track in _data.Timeline.Tracks)` → `track.Id`, `track.Label`, `track.Color` |

**Markup:**
```razor
<div class="tl-sidebar">
    @foreach (var track in _data.Timeline.Tracks)
    {
        <div style="font-size:12px;font-weight:600;line-height:1.4;color:@track.Color;">
            @track.Id<br/>
            <span style="font-weight:400;color:#444;">@track.Label</span>
        </div>
    }
</div>
```

### Section 3: Timeline SVG (`.tl-svg-box`)

| Property | Value |
|----------|-------|
| **Visual reference** | `<svg>` inside `.tl-svg-box` |
| **SVG viewBox** | `0 0 1560 185` |
| **CSS layout** | `flex: 1; padding-left: 12px; padding-top: 6px;` |
| **Data bindings** | Timeline start/end dates for X-position calculation; `@foreach` over tracks and markers |

**Rendering Logic:**

1. **Month grid lines & labels:** Calculate X positions by iterating from `startDate` to `endDate` month by month. Each month boundary gets a vertical line and text label.

```razor
@{
    var current = new DateOnly(_timelineStart.Year, _timelineStart.Month, 1);
    while (current <= _timelineEnd)
    {
        var x = DateToX(current.ToString("yyyy-MM-dd"));
        <line x1="@x" y1="0" x2="@x" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
        <text x="@(x + 5)" y="14" fill="#666" font-size="11" font-weight="600"
              font-family="Segoe UI,Arial">@current.ToString("MMM")</text>
        current = current.AddMonths(1);
    }
}
```

2. **Track lines:** Each track renders as a horizontal line at a computed Y position. Tracks are spaced evenly across the 185px height, offset below the month labels (~y=42 for first track, spacing = `(185 - 30) / trackCount`).

```razor
@{
    var trackSpacing = 155.0 / _data.Timeline.Tracks.Count;
    var trackIndex = 0;
}
@foreach (var track in _data.Timeline.Tracks)
{
    var y = 42 + (trackIndex * trackSpacing);
    <line x1="0" y1="@y" x2="1560" y2="@y" stroke="@track.Color" stroke-width="3"/>

    @foreach (var marker in track.Markers)
    {
        var mx = DateToX(marker.Date);
        @switch (marker.Type)
        {
            case "checkpoint":
                <circle cx="@mx" cy="@y" r="7" fill="white" stroke="@track.Color" stroke-width="2.5"/>
                break;
            case "dot":
                <circle cx="@mx" cy="@y" r="4" fill="#999"/>
                break;
            case "poc":
                var pocPoints = $"{mx},{y-11} {mx+11},{y} {mx},{y+11} {mx-11},{y}";
                <polygon points="@pocPoints" fill="#F4B400" filter="url(#sh)"/>
                break;
            case "production":
                var prodPoints = $"{mx},{y-11} {mx+11},{y} {mx},{y+11} {mx-11},{y}";
                <polygon points="@prodPoints" fill="#34A853" filter="url(#sh)"/>
                break;
        }

        @if (!string.IsNullOrEmpty(marker.Label))
        {
            var ly = marker.LabelPosition == "below" ? y + 24 : y - 16;
            <text x="@mx" y="@ly" text-anchor="middle" fill="#666" font-size="10"
                  font-family="Segoe UI,Arial">@marker.Label</text>
        }
    }
    trackIndex++;
}
```

3. **NOW line:** Red dashed vertical line at `NowX()` position.

```razor
<line x1="@NowX()" y1="0" x2="@NowX()" y2="185" stroke="#EA4335"
      stroke-width="2" stroke-dasharray="5,3"/>
<text x="@(NowX() + 4)" y="14" fill="#EA4335" font-size="10" font-weight="700"
      font-family="Segoe UI,Arial">NOW</text>
```

### Section 4: Heatmap Title (`.hm-title`)

| Property | Value |
|----------|-------|
| **Visual reference** | `.hm-title` in `OriginalDesignConcept.html` |
| **CSS** | `font-size: 14px; font-weight: 700; color: #888; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 8px;` |
| **Data bindings** | Static text: "Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers" |

### Section 5: Heatmap Grid (`.hm-grid`)

| Property | Value |
|----------|-------|
| **Visual reference** | `.hm-grid` in `OriginalDesignConcept.html` |
| **CSS layout** | `display: grid; grid-template-columns: 160px repeat(4, 1fr); grid-template-rows: 36px repeat(4, 1fr); border: 1px solid #E0E0E0; flex: 1; min-height: 0;` |
| **Data bindings** | `@_data.Heatmap.Periods` for column headers; `@_data.Heatmap.Shipped`, `.InProgress`, `.Carryover`, `.Blockers` for row data |

**Rendering Logic:**

1. **Header row:**
```razor
<div class="hm-corner">Status</div>
@foreach (var period in _data.Heatmap.Periods)
{
    <div class="hm-col-hdr @(period.IsCurrent ? "apr-hdr" : "")">
        @period.Label @(period.IsCurrent ? "← Now" : "")
    </div>
}
```

2. **Data rows** (repeated for each status category with different CSS classes):

```razor
@{
    var rows = new[]
    {
        ("Shipped", "ship", _data.Heatmap.Shipped),
        ("In Progress", "prog", _data.Heatmap.InProgress),
        ("Carryover", "carry", _data.Heatmap.Carryover),
        ("Blockers", "block", _data.Heatmap.Blockers)
    };
}
@foreach (var (label, prefix, row) in rows)
{
    <div class="hm-row-hdr @(prefix)-hdr">@row.Emoji @label</div>
    @for (var i = 0; i < _data.Heatmap.Periods.Count; i++)
    {
        var isCurrent = _data.Heatmap.Periods[i].IsCurrent;
        var items = i < row.ItemsByPeriod.Count ? row.ItemsByPeriod[i] : new List<string>();
        <div class="hm-cell @(prefix)-cell @(isCurrent ? "apr" : "")">
            @if (items.Count == 0)
            {
                <div class="it" style="color:#AAA;">–</div>
            }
            else
            {
                @foreach (var item in items)
                {
                    <div class="it">@item</div>
                }
            }
        </div>
    }
}
```

### Section 6: Error State

| Property | Value |
|----------|-------|
| **Visual reference** | Not in original design (new) |
| **CSS** | Centered container, Segoe UI, muted colors matching dashboard theme |
| **Data bindings** | `@_error` string from `DashboardLoadResult.ErrorMessage` |
| **Trigger** | `_error is not null` (file missing or JSON parse failure) |

```razor
@if (_error is not null)
{
    <div class="error-container">
        <p class="error-message">@_error</p>
    </div>
}
```

```css
.error-container {
    display: flex;
    align-items: center;
    justify-content: center;
    height: 100%;
    padding: 44px;
}
.error-message {
    font-size: 18px;
    color: #666;
    text-align: center;
    max-width: 600px;
    line-height: 1.6;
}
```

### Component-to-CSS Class Mapping (Complete)

| Visual Section | Primary CSS Class | Grid/Flex Strategy | Height |
|----------------|-------------------|-------------------|--------|
| Header | `.hdr` | Flex row, space-between | ~50px (auto) |
| Timeline container | `.tl-area` | Flex row | 196px (fixed) |
| Timeline sidebar | (inline style) | Flex column, space-around | 196px (parent) |
| Timeline SVG | `.tl-svg-box` | Flex: 1 | 185px (SVG viewBox) |
| Heatmap wrapper | `.hm-wrap` | Flex: 1, column | Fills remaining |
| Heatmap title | `.hm-title` | Block | Auto (~22px) |
| Heatmap grid | `.hm-grid` | CSS Grid 5×5 | Flex: 1 (fills rest) |
| Grid corner | `.hm-corner` | Flex center | 36px (row 1) |
| Column headers | `.hm-col-hdr` | Flex center | 36px (row 1) |
| Row headers | `.hm-row-hdr` | Flex, align center | 1fr (equal rows) |
| Data cells | `.hm-cell` | Block, overflow hidden | 1fr (equal rows) |
| Work items | `.it` | Relative position, left padding | Auto per item |
| Item dots | `.it::before` | Absolute, 6×6px circle | — |
| Error state | `.error-container` | Flex center | 100% |