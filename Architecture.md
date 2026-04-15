# Architecture

## Overview & Goals

This document defines the complete system architecture for the **Executive Reporting Dashboard** — a single-page Blazor Server application that visualizes project milestone timelines and monthly execution status in a screenshot-friendly format optimized for 1920×1080 PowerPoint slides.

### Architectural Philosophy

**Radical simplicity.** This is a data-driven static rendering problem. The architecture uses the minimum viable abstraction: a single Blazor Server project, a flat JSON file for data, inline SVG for the timeline, vanilla CSS for styling, and zero external dependencies in production.

### Goals

| # | Goal | Architecture Response |
|---|------|---------------------|
| 1 | Executive visibility into project health | Three-section layout: Header → Timeline → Heatmap, all data-driven from `data.json` |
| 2 | Eliminate manual slide creation | Fixed 1920×1080 viewport with `overflow: hidden`; pixel-perfect CSS matching `OriginalDesignConcept.html` |
| 3 | Rapid data updates | Single `data.json` file; no code changes needed; browser refresh picks up edits |
| 4 | Zero operational cost | Local-only execution via `dotnet run`; no cloud, no database, no authentication |
| 5 | Screenshot fidelity | System font (Segoe UI), fixed viewport, no JavaScript, deterministic server-side rendering |

### Constraints

- **Runtime:** .NET 8 SDK, Blazor Server
- **Viewport:** Fixed 1920×1080, non-responsive
- **Data source:** Local `data.json` file only
- **Dependencies:** Zero production NuGet packages beyond `Microsoft.AspNetCore.App`
- **Browser support:** Microsoft Edge and Google Chrome (latest stable)

---

## System Components

### Solution Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj          # net8.0, no extra NuGet refs
│       ├── Program.cs                          # Minimal hosting setup
│       ├── appsettings.json                    # DataFilePath configuration
│       ├── appsettings.Development.json        # Development overrides
│       ├── data.json                           # Dashboard data (sample included)
│       ├── Models/
│       │   └── DashboardData.cs                # C# record types for JSON schema
│       ├── Services/
│       │   ├── IDashboardDataService.cs        # Service interface
│       │   └── DashboardDataService.cs         # File read + deserialization
│       ├── Components/
│       │   ├── App.razor                       # Root component
│       │   ├── Pages/
│       │   │   └── Dashboard.razor             # Single page — orchestrates layout
│       │   ├── Layout/
│       │   │   └── MainLayout.razor            # Minimal layout (no nav, no sidebar)
│       │   └── Shared/
│       │       ├── Header.razor                # Title, subtitle, backlog link, legend
│       │       ├── TimelineSvg.razor           # SVG milestone timeline
│       │       ├── TimelineSidebar.razor        # Milestone labels sidebar
│       │       ├── HeatmapGrid.razor           # CSS Grid container + column headers
│       │       └── HeatmapRow.razor            # Single status row (reused 4×)
│       └── wwwroot/
│           └── css/
│               └── dashboard.css               # All styles — ported from design HTML
└── tests/
    └── ReportingDashboard.Tests/
        ├── ReportingDashboard.Tests.csproj      # xUnit + bUnit + FluentAssertions
        ├── Services/
        │   └── DashboardDataServiceTests.cs     # Unit tests for data loading
        └── Components/
            ├── HeaderTests.cs                   # bUnit component tests
            ├── HeatmapRowTests.cs
            └── TimelineSvgTests.cs
```

### Component Catalog

#### 1. `Program.cs` — Application Entry Point

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Configure Kestrel, register services, build and run the host |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService`, ASP.NET Core hosting |
| **Key Behavior** | Binds to `http://localhost:5000`; registers `IDashboardDataService` as scoped; configures Blazor Server with extended circuit timeout |

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<IDashboardDataService, DashboardDataService>();
builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
```

#### 2. `DashboardDataService` — Data Access Layer

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Read `data.json` from disk, deserialize to `DashboardData`, handle errors |
| **Interface** | `IDashboardDataService` with `Task<DashboardDataResult> LoadAsync()` |
| **Dependencies** | `IConfiguration` (for file path), `IWebHostEnvironment` (for content root), `System.Text.Json` |
| **Data** | Returns `DashboardDataResult` — either a valid `DashboardData` object or an error message string |
| **Error Handling** | `FileNotFoundException` → error message; `JsonException` → error message; never throws to caller |

```csharp
public interface IDashboardDataService
{
    Task<DashboardDataResult> LoadAsync();
}

public record DashboardDataResult(
    DashboardData? Data,
    string? ErrorMessage
)
{
    public bool IsSuccess => Data is not null;
}
```

**File path resolution order:**
1. `appsettings.json` → `"Dashboard:DataFilePath"` (if set)
2. Default: `"data.json"` resolved relative to `IWebHostEnvironment.ContentRootPath`

**Deserialization configuration:**
```csharp
private static readonly JsonSerializerOptions JsonOptions = new()
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
};
```

#### 3. `Dashboard.razor` — Page Orchestrator

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Inject `IDashboardDataService`, call `LoadAsync()` in `OnInitializedAsync`, render child components or error state |
| **Route** | `@page "/"` |
| **Dependencies** | `IDashboardDataService`, all child components |
| **Data** | Receives `DashboardData` from service; passes slices to children via `[Parameter]` |

**Rendering logic:**
- If `DashboardDataResult.IsSuccess` → render `Header`, timeline area (`TimelineSidebar` + `TimelineSvg`), and `HeatmapGrid`
- If error → render centered error message div with the error text
- Data is read fresh on every page load (no caching); editing `data.json` + browser refresh shows updated data

#### 4. `Header.razor` — Project Header & Legend

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render project title (H1), ADO backlog link, subtitle, and four-item legend |
| **Parameters** | `string Title`, `string Subtitle`, `string BacklogUrl`, `string CurrentMonthLabel` |
| **Dependencies** | None (pure presentation) |
| **CSS Classes** | `.hdr`, `.sub` |
| **Design Mapping** | Maps to `.hdr` div in `OriginalDesignConcept.html` |

#### 5. `TimelineSidebar.razor` — Milestone Labels

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render the 230px-wide left sidebar listing milestone labels and sublabels |
| **Parameters** | `Milestone[] Milestones` |
| **Dependencies** | None (pure presentation) |
| **Design Mapping** | Maps to the 230px sidebar div inside `.tl-area` |

#### 6. `TimelineSvg.razor` — SVG Timeline

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Generate inline SVG (1560×185) with month gridlines, milestone swim lanes, markers, date labels, and NOW line |
| **Parameters** | `Milestone[] Milestones`, `string TimelineStartDate`, `string TimelineEndDate` |
| **Dependencies** | None (computed logic + SVG markup) |
| **Key Logic** | X-position calculation, Y-position spacing, marker type rendering (checkpoint/PoC/production) |
| **Design Mapping** | Maps to `.tl-svg-box > svg` in `OriginalDesignConcept.html` |

**X-Position Calculation:**
```csharp
@code {
    private const double SvgWidth = 1560.0;
    private const double SvgHeight = 185.0;

    private double DateToX(DateTime date)
    {
        var start = DateTime.Parse(TimelineStartDate);
        var end = DateTime.Parse(TimelineEndDate);
        var totalDays = (end - start).TotalDays;
        var dayOffset = (date - start).TotalDays;
        return Math.Clamp(dayOffset / totalDays * SvgWidth, 0, SvgWidth);
    }

    private double NowX => DateToX(DateTime.Now);

    private double GetRowY(int index, int total)
    {
        // Space swim lanes evenly: first at ~42px, spaced ~56px apart for 3 lanes
        var usableHeight = SvgHeight - 30; // reserve top 30px for month labels
        var spacing = usableHeight / (total + 1);
        return 30 + spacing * (index + 1);
    }
}
```

**Marker rendering by type:**
- `"checkpoint"` → `<circle>` with white fill, milestone-colored stroke, radius 7, stroke-width 2.5
- `"checkpoint-minor"` → `<circle>` with gray fill (`#999`), radius 4, no stroke
- `"poc"` → `<polygon>` diamond with fill `#F4B400` and `filter="url(#sh)"` drop shadow
- `"production"` → `<polygon>` diamond with fill `#34A853` and `filter="url(#sh)"` drop shadow

**Diamond polygon formula** (centered at cx, cy with radius r=11):
```
points="cx,(cy-r) (cx+r),cy cx,(cy+r) (cx-r),cy"
```

#### 7. `HeatmapGrid.razor` — Grid Container

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render CSS Grid container with corner cell, month column headers (highlighting current month), and four `HeatmapRow` children |
| **Parameters** | `string[] Months`, `int CurrentMonthIndex`, `StatusCategory Shipped`, `StatusCategory InProgress`, `StatusCategory Carryover`, `StatusCategory Blockers` |
| **Dependencies** | `HeatmapRow.razor` |
| **CSS Classes** | `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.apr-hdr` |
| **Dynamic Style** | `style="grid-template-columns: 160px repeat(@Months.Length, 1fr)"` on `.hm-grid` |
| **Design Mapping** | Maps to `.hm-wrap` and `.hm-grid` in `OriginalDesignConcept.html` |

#### 8. `HeatmapRow.razor` — Status Row

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Render one status row: row header + N data cells with colored bullet items |
| **Parameters** | `StatusCategory Category`, `int CurrentMonthIndex`, `string RowCssPrefix` (e.g., `"ship"`, `"prog"`, `"carry"`, `"block"`) |
| **Dependencies** | None (pure presentation) |
| **CSS Classes** | `.{prefix}-hdr`, `.{prefix}-cell`, `.apr` (current month modifier), `.it` (item with `::before` bullet) |
| **Empty Cell** | Renders `<span style="color:#AAA">—</span>` when `items.Length == 0` |
| **Design Mapping** | Maps to each row group in the heatmap grid |

#### 9. `MainLayout.razor` — Minimal Layout

| Aspect | Detail |
|--------|--------|
| **Responsibility** | Provide the minimal Blazor layout wrapper — just `@Body` inside a container, plus CSS link |
| **Key Markup** | No navigation, no sidebar, no footer. Links `dashboard.css`. |

#### 10. `dashboard.css` — Stylesheet

| Aspect | Detail |
|--------|--------|
| **Responsibility** | All visual styling — ported nearly verbatim from `OriginalDesignConcept.html` `<style>` block |
| **Architecture** | CSS custom properties for color tokens at `:root`; class names match original HTML exactly |
| **Size** | ~150 lines |
| **No framework** | No Bootstrap, Tailwind, or component library CSS |

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────┐     ┌─────────────────────┐     ┌──────────────────┐
│  data.json   │────▶│ DashboardDataService │────▶│  Dashboard.razor  │
│  (flat file) │     │  (read + deserialize)│     │  (page component) │
└─────────────┘     └─────────────────────┘     └────────┬─────────┘
                                                          │
                              ┌────────────────────────────┼──────────────────┐
                              │                            │                  │
                    ┌─────────▼────┐          ┌────────────▼─────┐   ┌───────▼────────┐
                    │ Header.razor  │          │ Timeline Area     │   │ HeatmapGrid    │
                    │              │          │ ┌───────────────┐ │   │ .razor          │
                    │ Title        │          │ │TimelineSidebar│ │   │                │
                    │ Subtitle     │          │ │ .razor        │ │   │ ┌────────────┐ │
                    │ BacklogUrl   │          │ ├───────────────┤ │   │ │HeatmapRow  │ │
                    │ Legend       │          │ │TimelineSvg    │ │   │ │(×4: Ship,  │ │
                    └──────────────┘          │ │ .razor        │ │   │ │Prog, Carry,│ │
                                              │ └───────────────┘ │   │ │Block)      │ │
                                              └──────────────────┘   │ └────────────┘ │
                                                                      └────────────────┘
```

### Request Lifecycle

1. **Browser → Kestrel:** HTTP GET `http://localhost:5000/`
2. **Kestrel → Blazor Server:** Routes to `Dashboard.razor` via `@page "/"`
3. **Dashboard.razor → DashboardDataService:** Calls `LoadAsync()` in `OnInitializedAsync`
4. **DashboardDataService → File System:** Reads `data.json` via `File.ReadAllTextAsync`
5. **DashboardDataService → Dashboard.razor:** Returns `DashboardDataResult` (data or error)
6. **Dashboard.razor → Child Components:** Passes data slices as `[Parameter]` properties
7. **Blazor Server → Browser:** Renders HTML over SignalR circuit
8. **Browser:** Paints the fixed 1920×1080 layout

### Component Parameter Contracts

```
Dashboard.razor
├── Header.razor
│   ├── [Parameter] string Title
│   ├── [Parameter] string Subtitle
│   ├── [Parameter] string BacklogUrl
│   └── [Parameter] string CurrentMonthLabel
├── TimelineSidebar.razor
│   └── [Parameter] Milestone[] Milestones
├── TimelineSvg.razor
│   ├── [Parameter] Milestone[] Milestones
│   ├── [Parameter] string TimelineStartDate
│   └── [Parameter] string TimelineEndDate
└── HeatmapGrid.razor
    ├── [Parameter] string[] Months
    ├── [Parameter] int CurrentMonthIndex
    ├── [Parameter] StatusCategory Shipped
    ├── [Parameter] StatusCategory InProgress
    ├── [Parameter] StatusCategory Carryover
    └── [Parameter] StatusCategory Blockers
        └── HeatmapRow.razor (×4)
            ├── [Parameter] StatusCategory Category
            ├── [Parameter] int CurrentMonthIndex
            └── [Parameter] string RowCssPrefix
```

### Refresh Behavior

- **No caching.** `DashboardDataService.LoadAsync()` reads the file from disk on every call.
- **Every page load** (browser refresh, F5, or initial navigation) triggers a fresh file read.
- **No file watcher.** The PM edits `data.json`, then refreshes the browser manually.
- **SignalR circuit** remains open but is effectively unused — no interactive events, no state changes after initial render.

---

## Data Model

### C# Record Types (`Models/DashboardData.cs`)

```csharp
namespace ReportingDashboard.Models;

/// <summary>
/// Root data model deserialized from data.json.
/// All dashboard content is driven by this structure.
/// </summary>
public record DashboardData(
    string Title,                    // "Project Phoenix Release Roadmap"
    string Subtitle,                 // "Engineering Platform · Core Infrastructure · April 2026"
    string BacklogUrl,               // "https://dev.azure.com/org/project/_backlogs"
    string TimelineStartDate,        // "2026-01-01" (ISO 8601)
    string TimelineEndDate,          // "2026-06-30" (ISO 8601)
    string[] Months,                 // ["Jan", "Feb", "Mar", "Apr"]
    int CurrentMonthIndex,           // 3 (0-based index into Months array)
    Milestone[] Milestones,          // 1-5 milestone swim lanes
    StatusCategory Shipped,          // Green row
    StatusCategory InProgress,       // Blue row
    StatusCategory Carryover,        // Amber row
    StatusCategory Blockers          // Red row
);

/// <summary>
/// A milestone swim lane on the timeline.
/// </summary>
public record Milestone(
    string Label,                    // "M1"
    string Sublabel,                 // "API Gateway & Auth"
    string Color,                    // "#0078D4" (CSS color)
    MilestoneMarker[] Markers        // Ordered list of markers on this lane
);

/// <summary>
/// A single marker on a milestone swim lane.
/// </summary>
public record MilestoneMarker(
    string Date,                     // "2026-01-15" (ISO 8601)
    string Label,                    // "Jan 15" or "Mar 26 PoC"
    string Type,                     // "checkpoint" | "checkpoint-minor" | "poc" | "production"
    string? LabelPosition = null     // "above" | "below" (default: "above")
);

/// <summary>
/// A status category (Shipped, In Progress, Carryover, or Blockers).
/// </summary>
public record StatusCategory(
    string Label,                    // "SHIPPED"
    string[][] ItemsByMonth          // Items grouped by month index; each item is a string name
);
```

### JSON Schema (`data.json`)

```json
{
  "title": "Project Phoenix Release Roadmap",
  "subtitle": "Engineering Platform · Core Infrastructure · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "timelineStartDate": "2026-01-01",
  "timelineEndDate": "2026-06-30",
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "currentMonthIndex": 3,
  "milestones": [
    {
      "label": "M1",
      "sublabel": "API Gateway & Auth",
      "color": "#0078D4",
      "markers": [
        { "date": "2026-01-12", "label": "Jan 12", "type": "checkpoint" },
        { "date": "2026-03-26", "label": "Mar 26 PoC", "type": "poc", "labelPosition": "below" },
        { "date": "2026-05-01", "label": "May Prod", "type": "production", "labelPosition": "above" }
      ]
    },
    {
      "label": "M2",
      "sublabel": "PDS & Data Inventory",
      "color": "#00897B",
      "markers": [
        { "date": "2025-12-19", "label": "Dec 19", "type": "checkpoint" },
        { "date": "2026-02-11", "label": "Feb 11", "type": "checkpoint" },
        { "date": "2026-03-10", "label": "", "type": "checkpoint-minor" },
        { "date": "2026-03-18", "label": "", "type": "checkpoint-minor" },
        { "date": "2026-03-23", "label": "", "type": "checkpoint-minor" },
        { "date": "2026-03-25", "label": "", "type": "checkpoint-minor" },
        { "date": "2026-03-28", "label": "Mar 28 PoC", "type": "poc" },
        { "date": "2026-05-15", "label": "May Prod", "type": "production", "labelPosition": "below" }
      ]
    },
    {
      "label": "M3",
      "sublabel": "Auto Review DFD",
      "color": "#546E7A",
      "markers": [
        { "date": "2026-04-15", "label": "Apr 15", "type": "checkpoint" },
        { "date": "2026-05-30", "label": "May 30 PoC", "type": "poc", "labelPosition": "below" }
      ]
    }
  ],
  "shipped": {
    "label": "SHIPPED",
    "itemsByMonth": [
      ["Auth Service v2", "Rate Limiting"],
      ["SDK v3.1", "Logging Pipeline"],
      ["Cache Layer", "Health Checks"],
      ["Config Service"]
    ]
  },
  "inProgress": {
    "label": "IN PROGRESS",
    "itemsByMonth": [[], [], ["API Gateway"], ["Gateway Beta", "Load Testing"]]
  },
  "carryover": {
    "label": "CARRYOVER",
    "itemsByMonth": [[], [], [], ["Schema Migration"]]
  },
  "blockers": {
    "label": "BLOCKERS",
    "itemsByMonth": [[], [], [], ["Vendor SDK Delay"]]
  }
}
```

### Data Validation Rules

| Rule | Enforcement | Error Behavior |
|------|-------------|----------------|
| `data.json` must exist at configured path | `DashboardDataService` checks `File.Exists()` | Returns `DashboardDataResult` with error message |
| JSON must be valid | `JsonSerializer.Deserialize` catches `JsonException` | Returns error message: "Invalid JSON in data.json: {details}" |
| `months` array must not be empty | Validated after deserialization | Returns error message |
| `currentMonthIndex` must be within `months` bounds | Validated after deserialization | Clamps to valid range with warning logged |
| `itemsByMonth` length should match `months` length | Tolerated if shorter (pads with empty arrays) | Logs warning; does not error |
| `timelineStartDate` / `timelineEndDate` must be valid ISO dates | `DateTime.TryParse` validation | Returns error message if unparseable |
| `milestones` array length: 1-5 | Soft limit; renders whatever is provided | May visually clip beyond 5 |

### Entity Relationship

```
DashboardData (1)
├── Milestone (1..5)
│   └── MilestoneMarker (1..N)
├── StatusCategory "Shipped" (1)
│   └── string[][] ItemsByMonth
├── StatusCategory "InProgress" (1)
│   └── string[][] ItemsByMonth
├── StatusCategory "Carryover" (1)
│   └── string[][] ItemsByMonth
└── StatusCategory "Blockers" (1)
    └── string[][] ItemsByMonth
```

**Storage:** Single `data.json` flat file. No database. No migrations. No ORM.

---

## API Contracts

This application has no REST API, no Web API controllers, and no external service calls. It is a Blazor Server application with a single page route.

### Routes

| Route | Method | Component | Description |
|-------|--------|-----------|-------------|
| `/` | GET (HTTP) + SignalR | `Dashboard.razor` | Single page — renders the entire dashboard |

### Internal Service Contract

```csharp
public interface IDashboardDataService
{
    /// <summary>
    /// Reads and deserializes data.json from disk.
    /// Called on every page load. Never throws.
    /// </summary>
    /// <returns>
    /// DashboardDataResult with either valid Data or an ErrorMessage.
    /// </returns>
    Task<DashboardDataResult> LoadAsync();
}
```

**Success response shape:** `DashboardDataResult { Data: DashboardData, ErrorMessage: null }`

**Error response shape:** `DashboardDataResult { Data: null, ErrorMessage: "Unable to load dashboard data. Please check that data.json exists and contains valid JSON." }`

### Error Display Contract

When `DashboardDataResult.IsSuccess == false`, `Dashboard.razor` renders:

```html
<div style="display:flex;align-items:center;justify-content:center;
            width:1920px;height:1080px;font-family:'Segoe UI',Arial,sans-serif;">
  <div style="text-align:center;color:#991B1B;font-size:18px;max-width:600px;">
    <p style="font-size:24px;font-weight:700;margin-bottom:12px;">⚠ Dashboard Error</p>
    <p>@result.ErrorMessage</p>
  </div>
</div>
```

### Static Assets

| Path | Content Type | Description |
|------|-------------|-------------|
| `/css/dashboard.css` | `text/css` | All dashboard styles |
| `/_blazor` | SignalR | Blazor Server circuit (framework-managed) |

---

## Infrastructure Requirements

### Runtime Requirements

| Requirement | Specification |
|-------------|---------------|
| **Runtime** | .NET 8 SDK (for `dotnet run`) or self-contained executable (no SDK needed) |
| **OS** | Windows 10/11 (primary — Segoe UI font); macOS/Linux (works with Arial fallback) |
| **Port** | `http://localhost:5000` (HTTP only) |
| **Browser** | Microsoft Edge or Google Chrome, latest stable |
| **Disk** | < 5MB for project files; ~80MB for self-contained publish |
| **Memory** | < 100MB working set |
| **Network** | None required — fully offline capable |

### Hosting Configuration

**`appsettings.json`:**
```json
{
  "Dashboard": {
    "DataFilePath": "data.json"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

**`Program.cs` Kestrel configuration:**
```csharp
builder.WebHost.UseUrls("http://localhost:5000");
```

### Project File (`ReportingDashboard.csproj`)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**Zero NuGet `<PackageReference>` elements.** The `Microsoft.NET.Sdk.Web` SDK provides everything: Blazor Server, Kestrel, System.Text.Json, configuration, logging.

### Test Project File (`ReportingDashboard.Tests.csproj`)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
    <PackageReference Include="bunit" Version="1.25.3" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\ReportingDashboard\ReportingDashboard.csproj" />
  </ItemGroup>
</Project>
```

### Build & Run Commands

| Command | Purpose |
|---------|---------|
| `dotnet run --project src/ReportingDashboard` | Start dashboard at localhost:5000 |
| `dotnet watch run --project src/ReportingDashboard` | Start with hot reload |
| `dotnet test` | Run all unit tests |
| `dotnet build` | Build without running |
| `dotnet publish src/ReportingDashboard -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish` | Produce single-file executable |

### CI/CD

**Not required.** This is a local-only tool. If CI is desired in the future:

```yaml
# Minimal GitHub Actions (optional, not in initial scope)
steps:
  - uses: actions/setup-dotnet@v4
    with: { dotnet-version: '8.0.x' }
  - run: dotnet build --configuration Release
  - run: dotnet test --configuration Release --no-build
```

### Distribution

For sharing with non-developers:
1. Run `dotnet publish` (see above)
2. Copy `publish/ReportingDashboard.exe` + `data.json` into a zip
3. Recipient runs `ReportingDashboard.exe`, opens `http://localhost:5000`
4. No .NET SDK installation required (self-contained)

---

## Technology Stack Decisions

| Layer | Choice | Justification |
|-------|--------|---------------|
| **Framework** | Blazor Server (.NET 8) | Mandated stack. Provides hot reload, C# data binding, server-side rendering. SignalR overhead is negligible for local single-user use. |
| **CSS** | Vanilla CSS with custom properties | The design reference is ~120 lines of plain CSS. A framework (Bootstrap, Tailwind, MudBlazor) would add complexity, bundle size, and override conflicts with zero benefit. |
| **SVG** | Inline SVG via Razor markup | The timeline contains ~20 SVG elements. Generating `<line>`, `<circle>`, `<polygon>`, and `<text>` in Razor `@foreach` loops gives pixel-perfect control. A charting library would be overkill and reduce fidelity. |
| **Data Format** | JSON flat file (`data.json`) | Human-editable, version-controllable, trivially deserialized with `System.Text.Json`. A database would add migration overhead for ~100 data items. |
| **JSON Library** | `System.Text.Json` (built-in) | Ships with .NET 8. Zero additional dependencies. Supports `camelCase`, comments, trailing commas. |
| **Font** | Segoe UI (system font) | Available on all Windows machines. No web font loading, no FOUT, no CDN dependency. Arial fallback on other OSes. |
| **Web Server** | Kestrel (built-in) | Ships with ASP.NET Core. Binds to localhost:5000. No IIS, no reverse proxy, no Docker needed. |
| **Testing** | xUnit + bUnit + FluentAssertions | Industry standard for .NET testing. bUnit enables testing Razor components without a browser. Test-project-only dependencies. |
| **No JavaScript** | Deliberate exclusion | All rendering is server-side. No JS interop, no JS libraries, no `<script>` tags beyond Blazor's own `_blazor` script. |
| **No Database** | Deliberate exclusion | Data volume is < 200 items. JSON is simpler to edit than SQL. No migrations, no connection strings, no ORM. |
| **No Authentication** | Deliberate exclusion | Local-only tool on localhost. Auth would be pure overhead with no security benefit. |

### Rejected Alternatives

| Alternative | Why Rejected |
|------------|--------------|
| **MudBlazor / Radzen** | Adds 500KB+ CSS/JS bundle; design requires exact pixel matching that component libraries make harder, not easier |
| **Chart.js / ApexCharts via JS interop** | Timeline has <20 elements; JS interop adds complexity and breaks the "no JavaScript" constraint |
| **SQLite / LiteDB** | Single JSON file with ~100 items doesn't justify database overhead |
| **Blazor WebAssembly** | Larger download size (~10MB); no advantage for local-only tool; Server provides better hot reload |
| **Static SSR (Blazor .NET 8)** | Would work, but Server mode provides SignalR-based hot reload which is valuable during development |
| **Tailwind CSS** | Requires build tooling (PostCSS); ~120 lines of vanilla CSS don't justify a framework |

---

## Security Considerations

### Threat Model

This is a **local-only, single-user, read-only** dashboard. The threat surface is minimal:

| Threat | Risk Level | Mitigation |
|--------|-----------|------------|
| **Network exposure** | Low | Kestrel binds to `localhost` only. Not accessible from other machines on the network. |
| **Data sensitivity** | Low | `data.json` contains project names and milestone labels. No PII, no credentials, no secrets. |
| **Code injection via data.json** | Low | Blazor's Razor rendering HTML-encodes all `@variable` output by default. No `@((MarkupString)...)` usage on user data. |
| **XSS via backlogUrl** | Low | The URL is rendered in an `href` attribute. Blazor encodes it. Only `http://` and `https://` schemes should be accepted. |
| **File system access** | Low | Service reads one file at a configured path. No user input determines the file path at runtime. |
| **Denial of service** | N/A | Single-user local tool. No network exposure. |
| **Supply chain** | Low | Zero production NuGet dependencies beyond the framework SDK. |

### Mitigations Implemented

1. **HTML encoding by default:** Blazor Razor syntax (`@variable`) HTML-encodes output. No raw HTML rendering of data from `data.json`.
2. **Localhost binding:** Kestrel is configured to `http://localhost:5000` only — not `0.0.0.0` or `*`.
3. **No secrets in data.json:** The schema contains no fields for credentials, API keys, or tokens.
4. **Input validation:** `DashboardDataService` validates that required fields are present and dates are parseable before passing data to components.
5. **URL validation:** `BacklogUrl` is rendered in an `<a href="">` — if a malicious `javascript:` URL is injected, the browser will handle it, but Blazor's encoding mitigates most vectors.

### Recommendations for Sensitive Environments

- Add `data.json` to `.gitignore` if project names are sensitive
- Distribute `data.json` separately from the codebase
- If network access is needed in the future, add `builder.Services.AddAuthentication()` with Windows Authentication (single line)

---

## Scaling Strategy

### Current Scale

| Dimension | Value |
|-----------|-------|
| **Users** | 1 (single user on local machine) |
| **Data volume** | < 200 items, < 100KB JSON file |
| **Concurrent requests** | 1 |
| **Pages** | 1 |
| **Infrastructure** | Developer's local machine |

### Why Scaling Is Not Applicable

This is a local-only screenshot tool. It runs on the developer's laptop, serves one page to one browser tab, and reads one small file. Scaling concerns (load balancing, caching, database sharding, CDN) do not apply.

### If Requirements Change

| Future Need | Scaling Approach |
|------------|-----------------|
| **Multiple users on a team** | Publish as self-contained `.exe` and distribute. Each user runs their own instance. |
| **Shared hosting** | Deploy to a single IIS or Kestrel instance behind Windows Authentication. No architectural changes needed. |
| **Multiple projects** | Add a route parameter (`/dashboard/{projectName}`) and load `{projectName}.json`. Component architecture is unchanged. |
| **Larger data sets (1000+ items)** | Add `IMemoryCache` with file-watcher invalidation. Current architecture supports this as a drop-in addition to `DashboardDataService`. |
| **Real-time updates** | Leverage Blazor Server's existing SignalR circuit to push updates when `data.json` changes (via `FileSystemWatcher`). No client-side changes needed. |

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|-----------|--------|------------|
| 1 | **SVG text positioning differs between Edge and Chrome** | Medium | Medium | Use explicit `font-family="Segoe UI,Arial"` on every `<text>` element. Set `text-anchor` explicitly. Test both browsers during development. Document known rendering differences. |
| 2 | **Screenshot inconsistency across display scales** | Medium | High | **Always use browser Device Mode** (1920×1080) for screenshots — not window resizing. Document exact capture steps: `F12 → Device Mode → 1920×1080 → Capture full size screenshot`. |
| 3 | **data.json schema drift breaks deserialization** | Medium | Medium | Use `PropertyNameCaseInsensitive = true` and nullable properties on records. Validate after deserialization. Log warnings for missing optional fields. Include a `data.schema.json` reference file. |
| 4 | **Blazor Server SignalR circuit disconnects after idle** | Low | Low | Increase retention: `services.AddServerSideBlazor(o => o.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(30))`. User can always refresh. |
| 5 | **CSS Grid column count doesn't match data** | Medium | Medium | Generate `grid-template-columns` dynamically from `Months.Length` in Razor. Don't hardcode `repeat(4, 1fr)`. |
| 6 | **Heatmap cell overflow with many items** | Low | Low | CSS `overflow: hidden` is intentional per design spec. Document that data authors should list most important items first. |
| 7 | **Segoe UI not available on macOS/Linux** | Low | Low | Font stack includes Arial fallback. Minor visual differences on non-Windows systems are acceptable since the primary use case is Windows + PowerPoint. |
| 8 | **Hot reload changes don't appear** | Low | Low | Use `dotnet watch run` during development. If CSS changes don't reflect, hard-refresh with `Ctrl+Shift+R` to bypass browser cache. |
| 9 | **Self-contained publish size (~80MB)** | Low | Low | Acceptable for a local tool. Can enable trimming (`PublishTrimmed=true`) to reduce to ~30MB if needed, but requires testing. |
| 10 | **data.json file encoding issues** | Low | Medium | Document that `data.json` must be UTF-8. `File.ReadAllTextAsync` defaults to UTF-8. Add BOM detection in `DashboardDataService`. |

### Accepted Trade-offs

| Decision | What We Give Up | Why It's Worth It |
|----------|----------------|-------------------|
| No database | Can't query or filter data dynamically | Data volume is tiny; JSON is faster to edit than SQL |
| No component library | Must hand-write ~120 lines of CSS | Pixel-perfect control; no framework overrides to fight |
| No charting library | Must generate SVG manually in Razor | <20 SVG elements; Razor loops are clearer than chart config objects |
| Blazor Server (not Static SSR) | SignalR overhead for a read-only page | Hot reload during development; overhead is negligible locally |
| Fixed 1920×1080 layout | Unusable on mobile/tablets | Intentional — this is a screenshot capture tool, not a web app |
| No caching | File read on every page load | File is < 100KB; disk read is < 1ms on SSD; caching adds complexity |
| No JavaScript | Can't do client-side interactivity | No interactivity needed — the page is a visual snapshot |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to a specific Blazor component, its CSS layout strategy, data bindings, and interactions.

### Visual Section → Component Mapping

```
┌─────────────────────────────────────── 1920px ────────────────────────────────────┐
│ HEADER BAR (.hdr)                                        → Header.razor           │
│ ┌─ Left ─────────────────────────────┐ ┌─ Right ────────────────────────────────┐ │
│ │ H1: @Data.Title  ADO Backlog      │ │ Legend: ◆PoC  ◆Prod  ●Check  │Now     │ │
│ │ Sub: @Data.Subtitle                │ └─────────────────────────────────────────┘ │
│ └─────────────────────────────────────┘                                            │
├────────────────────────────────────────────────────────────────────────────────────┤
│ TIMELINE AREA (.tl-area)                                 → Dashboard.razor (div)   │
│ ┌── Sidebar ──┐┌─── SVG ──────────────────────────────────────────────────────┐   │
│ │ M1: ...     ││ Jan│Feb│Mar│Apr│May│Jun                                      │   │
│ │ M2: ...     ││ ───●────────◆────────◆──── (swim lanes)                     │   │
│ │ M3: ...     ││ NOW line (red dashed)                                        │   │
│ │             ││                                                               │   │
│ │ Timeline-   ││ TimelineSvg.razor                                            │   │
│ │ Sidebar     ││                                                               │   │
│ │ .razor      ││                                                               │   │
│ └─────────────┘└───────────────────────────────────────────────────────────────┘   │
├────────────────────────────────────────────────────────────────────────────────────┤
│ HEATMAP (.hm-wrap)                                       → HeatmapGrid.razor      │
│ ┌──────┬──────┬──────┬──────┬──────┐                                               │
│ │STATUS│ Jan  │ Feb  │ Mar  │*Apr* │  ← Column headers                             │
│ ├──────┼──────┼──────┼──────┼──────┤                                               │
│ │SHIP  │ ● ●  │ ● ●  │ ● ●  │ ●    │  ← HeatmapRow.razor (prefix: "ship")        │
│ ├──────┼──────┼──────┼──────┼──────┤                                               │
│ │PROG  │  —   │  —   │ ●    │ ● ●  │  ← HeatmapRow.razor (prefix: "prog")        │
│ ├──────┼──────┼──────┼──────┼──────┤                                               │
│ │CARRY │  —   │  —   │  —   │ ●    │  ← HeatmapRow.razor (prefix: "carry")       │
│ ├──────┼──────┼──────┼──────┼──────┤                                               │
│ │BLOCK │  —   │  —   │  —   │ ●    │  ← HeatmapRow.razor (prefix: "block")       │
│ └──────┴──────┴──────┴──────┴──────┘                                               │
└────────────────────────────────────────────────────────────────────────────────────┘
```

### Component Detail: Header.razor

| Aspect | Specification |
|--------|---------------|
| **Design Section** | `.hdr` element in `OriginalDesignConcept.html` |
| **CSS Layout** | `display: flex; align-items: center; justify-content: space-between` |
| **CSS Classes** | `.hdr`, `.sub` |
| **Data Bindings** | `@Title` → H1 text, `@BacklogUrl` → anchor href, `@Subtitle` → subtitle div, `@CurrentMonthLabel` → legend "Now" label |
| **Interactions** | Backlog link is a standard `<a>` tag; no JS events |
| **Dimensions** | Full width, ~50px height auto, padding `12px 44px 10px` |

**Legend markup** (inline, not data-driven — these are fixed visual indicators):
- PoC: 12×12 gold diamond (`transform: rotate(45deg)`)
- Production: 12×12 green diamond
- Checkpoint: 8×8 gray circle
- Now: 2×14 red bar + `"Now (@CurrentMonthLabel)"` text

### Component Detail: TimelineSidebar.razor

| Aspect | Specification |
|--------|---------------|
| **Design Section** | 230px sidebar inside `.tl-area` |
| **CSS Layout** | `display: flex; flex-direction: column; justify-content: space-around` |
| **Dimensions** | 230px wide, flex-shrink 0, padding `16px 12px 16px 0` |
| **Data Bindings** | `@foreach milestone in Milestones` → label in `milestone.Color`, sublabel in `#444` |
| **Interactions** | None (read-only labels) |

### Component Detail: TimelineSvg.razor

| Aspect | Specification |
|--------|---------------|
| **Design Section** | `.tl-svg-box > svg` element |
| **CSS Layout** | SVG element, 1560×185px, `overflow: visible` |
| **Data Bindings** | Month gridlines computed from `TimelineStartDate`/`TimelineEndDate`; swim lanes from `Milestones[]`; markers from `Milestone.Markers[]`; NOW line from `DateTime.Now` |
| **Interactions** | None (static SVG) |
| **SVG Elements** | `<defs>` with drop shadow filter; `<line>` for gridlines, swim lanes, NOW; `<text>` for labels; `<circle>` for checkpoints; `<polygon>` for diamonds |
| **Key Computation** | `DateToX(date)` converts ISO date string to X pixel coordinate (0–1560) |

**SVG `<defs>` block:**
```xml
<defs>
  <filter id="sh">
    <feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>
  </filter>
</defs>
```

### Component Detail: HeatmapGrid.razor

| Aspect | Specification |
|--------|---------------|
| **Design Section** | `.hm-wrap` and `.hm-grid` elements |
| **CSS Layout** | Outer: `display: flex; flex-direction: column`. Inner: CSS Grid with `grid-template-columns: 160px repeat(N, 1fr); grid-template-rows: 36px repeat(4, 1fr)` |
| **CSS Classes** | `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.apr-hdr` (current month) |
| **Data Bindings** | `@Months` → column headers; `@CurrentMonthIndex` → `.apr-hdr` class toggle; status categories → four `HeatmapRow` instances |
| **Dynamic Style** | Grid columns set via inline `style` attribute based on `Months.Length` |
| **Interactions** | None (static grid) |

**Heatmap title text:**
```
MONTHLY EXECUTION HEATMAP — SHIPPED · IN PROGRESS · CARRYOVER · BLOCKERS
```

### Component Detail: HeatmapRow.razor

| Aspect | Specification |
|--------|---------------|
| **Design Section** | Each status row within `.hm-grid` |
| **CSS Layout** | Inherits CSS Grid placement from parent |
| **CSS Classes** | Row header: `.hm-row-hdr .{prefix}-hdr`; Cells: `.hm-cell .{prefix}-cell` with `.apr` modifier for current month |
| **Data Bindings** | `@Category.Label` → row header text; `@Category.ItemsByMonth[i]` → items per cell; each item → `.it` span with `::before` bullet |
| **Parameters** | `StatusCategory Category`, `int CurrentMonthIndex`, `string RowCssPrefix` |
| **Empty Cells** | Renders `—` in `#AAA` when month array is empty |
| **Overflow** | CSS `overflow: hidden` on `.hm-cell` truncates excess items |

**CSS class mapping per row type:**

| RowCssPrefix | Header Class | Cell Class | Current Month Class |
|-------------|-------------|-----------|-------------------|
| `ship` | `.ship-hdr` | `.ship-cell` | `.ship-cell.apr` |
| `prog` | `.prog-hdr` | `.prog-cell` | `.prog-cell.apr` |
| `carry` | `.carry-hdr` | `.carry-cell` | `.carry-cell.apr` |
| `block` | `.block-hdr` | `.block-cell` | `.block-cell.apr` |

### CSS Custom Properties (`dashboard.css`)

```css
:root {
  /* Layout */
  --page-width: 1920px;
  --page-height: 1080px;
  --page-padding: 44px;
  --timeline-height: 196px;
  --sidebar-width: 230px;
  --heatmap-row-header-width: 160px;
  --heatmap-header-row-height: 36px;

  /* Colors — Shipped (green) */
  --color-shipped-hdr-bg: #E8F5E9;
  --color-shipped-hdr-text: #1B7A28;
  --color-shipped-cell-bg: #F0FBF0;
  --color-shipped-cell-current: #D8F2DA;
  --color-shipped-dot: #34A853;

  /* Colors — In Progress (blue) */
  --color-progress-hdr-bg: #E3F2FD;
  --color-progress-hdr-text: #1565C0;
  --color-progress-cell-bg: #EEF4FE;
  --color-progress-cell-current: #DAE8FB;
  --color-progress-dot: #0078D4;

  /* Colors — Carryover (amber) */
  --color-carry-hdr-bg: #FFF8E1;
  --color-carry-hdr-text: #B45309;
  --color-carry-cell-bg: #FFFDE7;
  --color-carry-cell-current: #FFF0B0;
  --color-carry-dot: #F4B400;

  /* Colors — Blockers (red) */
  --color-block-hdr-bg: #FEF2F2;
  --color-block-hdr-text: #991B1B;
  --color-block-cell-bg: #FFF5F5;
  --color-block-cell-current: #FFE4E4;
  --color-block-dot: #EA4335;

  /* Colors — General */
  --color-current-month-hdr-bg: #FFF0D0;
  --color-current-month-hdr-text: #C07700;
  --color-link: #0078D4;
  --color-text-primary: #111;
  --color-text-secondary: #888;
  --color-text-item: #333;
  --color-border: #E0E0E0;
  --color-border-heavy: #CCC;
  --color-timeline-bg: #FAFAFA;
  --color-now-line: #EA4335;
  --color-poc-marker: #F4B400;
  --color-prod-marker: #34A853;
  --color-checkpoint: #999;
  --color-empty-cell: #AAA;
}
```