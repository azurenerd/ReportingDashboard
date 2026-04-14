# Architecture

## Overview & Goals

The Executive Reporting Dashboard is a single-page Blazor Server application that renders a pixel-perfect 1920×1080 project status view for screenshot capture into PowerPoint decks. It visualizes project milestones on an SVG timeline and displays a color-coded heatmap grid of work items organized by status (Shipped, In Progress, Carryover, Blockers) and month.

**Architecture Philosophy:** Intentionally minimal. This is a local developer tool, not a distributed system. The entire data flow is `data.json → DataService → Dashboard.razor → Browser`. No database, no API, no authentication, no cloud infrastructure.

**Primary Goals:**

1. **Visual fidelity** — Pixel-perfect match to `OriginalDesignConcept.html` at 1920×1080 in Microsoft Edge
2. **Data-driven rendering** — All content sourced from a single `wwwroot/data.json` file with strongly-typed C# models
3. **Live reload** — `FileSystemWatcher` detects `data.json` edits and pushes re-renders via Blazor's SignalR circuit within 1 second
4. **Zero dependencies** — Main project has no NuGet packages beyond the .NET 8 SDK; no JavaScript, no JS interop
5. **Graceful error handling** — Malformed or missing JSON produces user-friendly messages, never stack traces

---

## System Components

### 1. Program.cs — Application Host

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure and start the Blazor Server application |
| **Interfaces** | None (entry point) |
| **Dependencies** | .NET 8 WebApplication builder, `DataService` |
| **Data** | Reads `appsettings.json` for configuration |

**Behavior:**
- Registers `DataService` as a singleton in the DI container
- Configures Kestrel to bind exclusively to `http://localhost:5050`
- Adds Blazor Server services (`AddRazorComponents().AddInteractiveServerComponents()`)
- Maps Razor components and static file middleware
- No authentication middleware, no CORS, no HTTPS redirection

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<DataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Urls.Clear();
app.Urls.Add("http://localhost:5050");
app.Run();
```

### 2. DataService — Data Loading & File Watching

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Load, parse, validate, cache, and watch `data.json`; notify subscribers on change |
| **Interfaces** | `DashboardData? GetData()`, `string? GetError()`, `event Action? OnDataChanged` |
| **Dependencies** | `System.Text.Json`, `System.IO.FileSystemWatcher`, `IWebHostEnvironment` |
| **Data** | Reads `wwwroot/data.json`; holds last-known-good `DashboardData` in memory |
| **Lifetime** | Singleton (one instance for the application lifetime) |

**Behavior:**

- **Startup load:** Reads `wwwroot/data.json`, deserializes to `DashboardData` using `System.Text.Json`. On success, stores the result. On failure (missing file, malformed JSON, schema version mismatch), stores `null` data and a human-readable error string.
- **File watching:** Creates a `FileSystemWatcher` on the `wwwroot` directory, filtering for `data.json`. On `Changed`/`Created`/`Renamed` events, reloads after a 500ms debounce (using a `System.Threading.Timer`).
- **Error resilience:** If a reload fails (bad JSON), the last-known-good data is preserved and `GetError()` returns the parse error message. If the reload succeeds, `GetError()` returns `null`.
- **Change notification:** Fires `OnDataChanged` event after every reload attempt. Dashboard components subscribe to this event and call `InvokeAsync(StateHasChanged)`.
- **Schema validation:** Checks `schemaVersion` field in JSON. If it does not equal the expected version (1), returns an error message.
- **Thread safety:** Uses `lock` on read/write of `_currentData` and `_currentError` fields since `FileSystemWatcher` callbacks run on thread pool threads.

### 3. DashboardData Models — Strongly-Typed JSON Schema

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define the shape of `data.json` as immutable C# records |
| **Interfaces** | Plain data records (no methods beyond properties) |
| **Dependencies** | `System.Text.Json.Serialization` for `[JsonPropertyName]` attributes |
| **Data** | Deserialized from `data.json` |

**All models defined in a single file: `Models/DashboardData.cs`**

```csharp
public record DashboardData
{
    public required int SchemaVersion { get; init; }
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string BacklogUrl { get; init; }
    public required TimelineConfig Timeline { get; init; }
    public required HeatmapConfig Heatmap { get; init; }
    public string? NowDateOverride { get; init; }  // ISO date string, e.g. "2026-04-14"
}

public record TimelineConfig
{
    public required string StartDate { get; init; }   // "2026-01-01"
    public required string EndDate { get; init; }     // "2026-07-01"
    public required Workstream[] Workstreams { get; init; }
}

public record Workstream
{
    public required string Id { get; init; }          // "M1"
    public required string Name { get; init; }        // "Chatbot & MS Role"
    public required string Color { get; init; }       // "#0078D4"
    public required Milestone[] Milestones { get; init; }
}

public record Milestone
{
    public required string Label { get; init; }       // "Mar 26 PoC"
    public required string Date { get; init; }        // "2026-03-26"
    public required string Type { get; init; }        // "start" | "checkpoint" | "poc" | "production"
    public string? LabelPosition { get; init; }       // "above" | "below" (default: auto-alternate)
}

public record HeatmapConfig
{
    public required string[] MonthColumns { get; init; }  // ["Jan", "Feb", "Mar", "Apr"]
    public required StatusCategory[] Categories { get; init; }
}

public record StatusCategory
{
    public required string Name { get; init; }        // "Shipped"
    public required string Emoji { get; init; }       // "✅"
    public required string CssClass { get; init; }    // "ship"
    public required MonthItems[] Months { get; init; }
}

public record MonthItems
{
    public required string Month { get; init; }       // "Jan"
    public required string[] Items { get; init; }     // ["Item A", "Item B"] or []
}
```

### 4. Dashboard.razor — Single-Page UI Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the entire dashboard: header, timeline SVG, and heatmap grid |
| **Interfaces** | Blazor component (`@page "/"`) |
| **Dependencies** | `DataService` (injected), `NavigationManager` (for base URI) |
| **Data** | Reads `DashboardData` from `DataService` on each render |

**Behavior:**

- **OnInitialized:** Subscribes to `DataService.OnDataChanged`. Calls `StateHasChanged` on data changes (via `InvokeAsync` to marshal to the Blazor sync context).
- **Render logic:** If `DataService.GetData()` returns `null` and there is no error, shows "Loading...". If there is an error and no data, shows a full-page error message. If there is an error but data exists (last-known-good), renders the dashboard with an error banner at the top.
- **Three rendering sections:** Header band, Timeline band, Heatmap band — each implemented as inline markup blocks within the single component (no sub-components needed for this scale).
- **Dispose:** Unsubscribes from `OnDataChanged` in `Dispose()` to prevent memory leaks.

### 5. App.razor — Root Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Blazor application root; references the layout, routes, and static assets |
| **Dependencies** | `MainLayout.razor` |

Renders the `<html>`, `<head>` (with `<link>` to `css/dashboard.css`), and `<body>` tags. Uses `<HeadOutlet>` and `<Routes>` Blazor components. Sets `rendermode="InteractiveServer"` on the `<Routes>` component.

### 6. MainLayout.razor — Layout Wrapper

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Minimal layout that renders `@Body` with no chrome |

Contains only `@inherits LayoutComponentBase` and `@Body`. No navigation menu, no sidebar, no footer. The dashboard is the entire page.

### 7. dashboard.css — All Visual Styles

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define all CSS rules for the 1920×1080 dashboard layout |
| **Source** | Ported directly from `OriginalDesignConcept.html` with CSS custom properties added |

**CSS Custom Properties (top of file):**

```css
:root {
    --bg: #FFFFFF;
    --text-primary: #111;
    --text-secondary: #888;
    --text-tertiary: #666;
    --text-item: #333;
    --text-muted: #AAA;
    --link: #0078D4;
    --border-light: #E0E0E0;
    --border-medium: #CCC;
    --border-heavy: #E8E8E8;
    --surface: #FAFAFA;
    --surface-header: #F5F5F5;
    --now-bg: #FFF0D0;
    --now-text: #C07700;
    --green: #34A853;
    --green-hdr-bg: #E8F5E9;
    --green-hdr-text: #1B7A28;
    --green-cell: #F0FBF0;
    --green-cell-active: #D8F2DA;
    --blue: #0078D4;
    --blue-hdr-bg: #E3F2FD;
    --blue-hdr-text: #1565C0;
    --blue-cell: #EEF4FE;
    --blue-cell-active: #DAE8FB;
    --amber: #F4B400;
    --amber-hdr-bg: #FFF8E1;
    --amber-hdr-text: #B45309;
    --amber-cell: #FFFDE7;
    --amber-cell-active: #FFF0B0;
    --red: #EA4335;
    --red-hdr-bg: #FEF2F2;
    --red-hdr-text: #991B1B;
    --red-cell: #FFF5F5;
    --red-cell-active: #FFE4E4;
    --teal: #00897B;
    --blue-gray: #546E7A;
    --checkpoint: #999;
}
```

All remaining CSS rules are copied verbatim from the HTML reference, replacing hardcoded hex values with `var(--token)` references where practical. The `body` rule enforces `width: 1920px; height: 1080px; overflow: hidden`.

### 8. data.json — Dashboard Data File

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Single source of truth for all dashboard content |
| **Location** | `wwwroot/data.json` |
| **Format** | JSON, validated against `DashboardData` C# record |

Ships with sample "Project Atlas" data containing 3 workstreams, ~15 milestones, 4 status categories, and 4 months of heatmap items.

### 9. ReportingDashboard.Tests — Test Project

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Unit tests for data deserialization, date-to-pixel math, error handling; component tests for Dashboard rendering |
| **Dependencies** | xUnit 2.7.x, bUnit 1.28.x, Microsoft.NET.Test.Sdk 17.9.x |

**Test classes:**

| Class | Tests |
|-------|-------|
| `DataServiceTests` | Valid JSON loads correctly; malformed JSON returns error; missing file returns error; schema version mismatch returns error; file change triggers reload |
| `TimelineCalculationTests` | Date-to-X-position math for start of range, end of range, mid-range, edge dates; month gridline positions |
| `DashboardComponentTests` | (bUnit) Dashboard renders header with title from data; renders correct number of workstream labels; renders correct number of heatmap rows and columns; current month gets highlight class; empty cells show dash |

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│  Startup                                                            │
│                                                                     │
│  Program.cs                                                         │
│    ├─ builder.Services.AddSingleton<DataService>()                  │
│    ├─ app.UseStaticFiles()      ← serves dashboard.css              │
│    └─ app.MapRazorComponents()  ← routes "/" to Dashboard.razor     │
│                                                                     │
│  DataService (constructor)                                          │
│    ├─ Reads wwwroot/data.json                                       │
│    ├─ Deserializes → DashboardData                                  │
│    ├─ Validates schemaVersion                                       │
│    └─ Starts FileSystemWatcher on wwwroot/data.json                 │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│  Request Flow (browser → server → browser)                          │
│                                                                     │
│  Browser GET http://localhost:5050/                                  │
│    │                                                                │
│    ▼                                                                │
│  Blazor Server establishes SignalR WebSocket                        │
│    │                                                                │
│    ▼                                                                │
│  Dashboard.razor.OnInitialized()                                    │
│    ├─ Injects DataService                                           │
│    ├─ Calls DataService.GetData() → DashboardData                   │
│    ├─ Subscribes to DataService.OnDataChanged                       │
│    └─ Renders HTML + inline SVG                                     │
│         ├─ Section 1: Header (title, subtitle, legend)              │
│         ├─ Section 2: SVG Timeline (workstream lines, milestones)   │
│         └─ Section 3: CSS Grid Heatmap (status × month)             │
│    │                                                                │
│    ▼                                                                │
│  Browser renders at 1920×1080 with dashboard.css                    │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│  Live Reload Flow                                                   │
│                                                                     │
│  User edits data.json in text editor → saves                        │
│    │                                                                │
│    ▼                                                                │
│  FileSystemWatcher fires Changed event                              │
│    │                                                                │
│    ▼                                                                │
│  DataService debounce timer (500ms)                                 │
│    │                                                                │
│    ▼                                                                │
│  DataService reloads & re-parses data.json                          │
│    ├─ Success: updates _currentData, clears _currentError           │
│    └─ Failure: keeps _currentData, sets _currentError               │
│    │                                                                │
│    ▼                                                                │
│  DataService fires OnDataChanged event                              │
│    │                                                                │
│    ▼                                                                │
│  Dashboard.razor.InvokeAsync(StateHasChanged)                       │
│    │                                                                │
│    ▼                                                                │
│  Blazor diffs and pushes DOM updates via SignalR                    │
│    │                                                                │
│    ▼                                                                │
│  Browser re-renders (< 1 second from file save)                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Communication Patterns

| From | To | Mechanism | Frequency |
|------|----|-----------|-----------|
| Browser | Blazor Server | SignalR WebSocket | Persistent connection (1 circuit) |
| Blazor Server | Browser | SignalR DOM diffs | On initial render + each data reload |
| FileSystemWatcher | DataService | .NET event (`Changed`) | On each `data.json` file save |
| DataService | Dashboard.razor | C# event (`OnDataChanged`) | On each data reload (debounced) |
| Dashboard.razor | DataService | Method call (`GetData()`, `GetError()`) | On each render cycle |

---

## Data Model

### Entity: DashboardData (Root)

The entire data model is a single JSON document deserialized into an immutable C# record graph. There are no relational entities, no IDs for cross-referencing, and no normalization — the data is a tree.

```
DashboardData
├── SchemaVersion: int (must equal 1)
├── Title: string
├── Subtitle: string
├── BacklogUrl: string
├── NowDateOverride: string? (ISO date, nullable)
├── Timeline: TimelineConfig
│   ├── StartDate: string (ISO date)
│   ├── EndDate: string (ISO date)
│   └── Workstreams: Workstream[]
│       ├── Id: string
│       ├── Name: string
│       ├── Color: string (hex)
│       └── Milestones: Milestone[]
│           ├── Label: string
│           ├── Date: string (ISO date)
│           ├── Type: string (enum: "start"|"checkpoint"|"poc"|"production")
│           └── LabelPosition: string? ("above"|"below")
└── Heatmap: HeatmapConfig
    ├── MonthColumns: string[] (e.g., ["Jan","Feb","Mar","Apr"])
    └── Categories: StatusCategory[]
        ├── Name: string
        ├── Emoji: string
        ├── CssClass: string
        └── Months: MonthItems[]
            ├── Month: string
            └── Items: string[]
```

### Storage

| Aspect | Detail |
|--------|--------|
| **Format** | JSON file (`wwwroot/data.json`) |
| **Size** | ~2-10 KB (dozens of items) |
| **Access pattern** | Read-only from application; hand-edited by user |
| **Caching** | In-memory singleton (`DataService._currentData`) |
| **Persistence** | File on disk; survives app restarts |
| **Backup** | User responsibility (git, file copy) |

### Sample data.json Structure

```json
{
  "schemaVersion": 1,
  "title": "Project Atlas Release Roadmap",
  "subtitle": "Platform Engineering · Atlas Workstream · April 2026",
  "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
  "nowDateOverride": null,
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-07-01",
    "workstreams": [
      {
        "id": "M1",
        "name": "Chatbot & MS Role",
        "color": "#0078D4",
        "milestones": [
          { "label": "Jan 12", "date": "2026-01-12", "type": "start" },
          { "label": "Mar 26 PoC", "date": "2026-03-26", "type": "poc" },
          { "label": "May Prod", "date": "2026-05-01", "type": "production" }
        ]
      }
    ]
  },
  "heatmap": {
    "monthColumns": ["Jan", "Feb", "Mar", "Apr"],
    "categories": [
      {
        "name": "Shipped",
        "emoji": "✅",
        "cssClass": "ship",
        "months": [
          { "month": "Jan", "items": ["Auth module v2", "Config migration"] },
          { "month": "Feb", "items": ["API gateway"] },
          { "month": "Mar", "items": [] },
          { "month": "Apr", "items": ["Dashboard MVP"] }
        ]
      }
    ]
  }
}
```

### Date Handling

All dates in `data.json` use ISO 8601 format (`YYYY-MM-DD`). The application parses them to `DateOnly` using `DateOnly.ParseExact(value, "yyyy-MM-dd")`. The "current date" for the NOW line and current-month highlighting is determined by:

1. If `nowDateOverride` is non-null, parse it as the effective date
2. Otherwise, use `DateOnly.FromDateTime(DateTime.Today)`

The current month for heatmap highlighting is derived from the effective date's month name (abbreviated, e.g., "Apr") and matched against `monthColumns`.

---

## API Contracts

This application has **no REST, GraphQL, or HTTP API**. It is a Blazor Server application with a single page route. All data flows through in-process C# method calls.

### Internal Service Contract: DataService

```csharp
public class DataService : IDisposable
{
    /// Returns the current dashboard data, or null if no valid data has been loaded.
    public DashboardData? GetData();

    /// Returns the current error message, or null if the last load was successful.
    public string? GetError();

    /// Fired after every data reload attempt (success or failure).
    /// Subscribers MUST marshal to their own sync context (e.g., InvokeAsync).
    public event Action? OnDataChanged;

    /// Returns the effective "now" date (from override or system clock).
    public DateOnly GetEffectiveDate();
}
```

### Blazor Route

| Route | Component | Render Mode |
|-------|-----------|-------------|
| `/` | `Dashboard.razor` | Interactive Server |

### Static Assets

| Path | Content |
|------|---------|
| `/css/dashboard.css` | All dashboard styles |
| `/data.json` | Dashboard data (also read server-side by DataService) |

### Error Responses

| Scenario | User-Visible Behavior |
|----------|----------------------|
| `data.json` missing at startup | Full-page error: "No data.json found. Place a valid data.json file in the wwwroot/ directory and restart." |
| `data.json` malformed at startup | Full-page error: "data.json contains invalid JSON: {details}. Fix the file and restart." |
| Schema version mismatch | Full-page error: "data.json schemaVersion is {n}, expected 1. Update your data.json to match the current schema." |
| `data.json` malformed after live edit | Error banner: "⚠ data.json has errors — showing last valid data. Error: {details}" |
| `data.json` fixed after live edit | Error banner disappears; new data renders |

---

## Infrastructure Requirements

### Hosting

| Requirement | Specification |
|-------------|---------------|
| **Host machine** | Developer workstation (Windows 10/11) |
| **Runtime** | .NET 8.0 SDK (any patch version of 8.0.x LTS) |
| **Web server** | Kestrel (built into .NET 8), no IIS, no reverse proxy |
| **Network binding** | `http://localhost:5050` only — not exposed to LAN or internet |
| **Process model** | On-demand (`dotnet run`); not a Windows service or daemon |
| **Port configuration** | Hardcoded to 5050 in `Program.cs`; overridable via `launchSettings.json` or `--urls` CLI arg |

### Networking

- **Inbound:** Localhost only. Kestrel listens on `127.0.0.1:5050`.
- **Outbound:** Zero. The application makes no HTTP requests, no DNS lookups, no telemetry. Fully air-gap compatible.
- **WebSocket:** One SignalR circuit per browser tab (Blazor Server default).

### Storage

| Item | Location | Size | Access |
|------|----------|------|--------|
| Application binaries | `bin/` or `publish/` directory | ~5 MB (framework-dependent) or ~80 MB (self-contained) |  Read + Execute |
| `data.json` | `wwwroot/data.json` | 2-10 KB | Read-only by app; read-write by user |
| `dashboard.css` | `wwwroot/css/dashboard.css` | ~5 KB | Read-only (static file) |
| Application logs | Console (stdout) | N/A | No file logging |

### CI/CD

Not required for MVP. The application is built and run locally:

```bash
# Development
dotnet watch --project src/ReportingDashboard

# Production build
dotnet publish src/ReportingDashboard -c Release -o ./publish

# Self-contained build (no SDK required on target machine)
dotnet publish src/ReportingDashboard -c Release -r win-x64 --self-contained -o ./publish

# Run tests
dotnet test tests/ReportingDashboard.Tests
```

### Infrastructure Cost

**$0.** No servers, no cloud resources, no licenses, no containers.

---

## Technology Stack Decisions

### Chosen Technologies

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Runtime** | .NET 8.0 LTS | 8.0.x | Mandatory stack. LTS ensures long-term support through Nov 2026. |
| **UI Framework** | Blazor Server | Built-in (.NET 8) | Mandatory stack. Server-side rendering with SignalR push enables live reload without JS. Hot reload via `dotnet watch` accelerates CSS/layout iteration. |
| **CSS Layout** | Pure CSS Grid + Flexbox | CSS3 | The `OriginalDesignConcept.html` already uses these layouts. Porting directly ensures pixel-perfect match. No component library overhead. |
| **Timeline Rendering** | Inline SVG in Razor | SVG 1.1 | The timeline is a bespoke visualization (colored lines, diamonds, circles, dashed markers). Charting libraries would fight custom positioning. Raw SVG in Razor is ~60 lines and gives exact control. |
| **JSON Parsing** | System.Text.Json | Built-in (.NET 8) | Zero external dependencies. High performance. Source-generator-ready for AOT if needed later. |
| **File Watching** | FileSystemWatcher | Built-in (.NET 8) | Enables live reload. Debounced at 500ms to handle editor atomic saves. |
| **Font** | Segoe UI (system) | Pre-installed on Windows | Design spec requires it. No web font loading = no FOUT, no render delay. |
| **Unit Testing** | xUnit | 2.7.x | Industry standard for .NET. Zero config. |
| **Component Testing** | bUnit | 1.28.x | Purpose-built for Blazor component testing. Renders components in-memory without a browser. |
| **Test Runner** | Microsoft.NET.Test.Sdk | 17.9.x | Required by Visual Studio and `dotnet test`. |

### Rejected Alternatives

| Alternative | Why Rejected |
|-------------|-------------|
| **Blazor WebAssembly** | Adds ~2 MB download, slower startup, no `FileSystemWatcher` access from browser sandbox. Server-side is simpler for local-only use. |
| **Razor Pages / MVC** | No SignalR circuit = no live push on data change. Would require polling or manual refresh. |
| **MudBlazor / Radzen** | 500KB+ CSS/JS overhead. Imposes design language that must be overridden. The total CSS needed is 150 lines — a component library adds complexity without value. |
| **Chart.js / ApexCharts** | The timeline is not a standard chart. Libraries would require extensive customization to produce the exact diamond/circle/line layout. Direct SVG is simpler. |
| **SignalR Hub (custom)** | Blazor Server already includes a SignalR circuit. Adding a custom hub is unnecessary — `StateHasChanged()` handles push updates. |
| **SQLite / LiteDB** | The data is a single JSON document of ~5 KB with no relational complexity. A database adds startup cost, migration management, and connection strings for zero benefit. |
| **JavaScript / JS Interop** | Not needed. SVG rendering, CSS Grid layout, and Blazor's `StateHasChanged` cover all requirements. Zero JS is a hard constraint from the PM spec. |

### Project Structure

```
ReportingDashboard.sln
│
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj          # SDK Web project, net8.0, zero NuGet deps
│       ├── Program.cs                          # Host configuration, DI, Kestrel setup
│       ├── Models/
│       │   └── DashboardData.cs               # All record types (DashboardData, Workstream, etc.)
│       ├── Services/
│       │   └── DataService.cs                 # JSON loading, FileSystemWatcher, change events
│       ├── Components/
│       │   ├── App.razor                      # HTML root, <head>, CSS link, render mode
│       │   ├── Routes.razor                   # <Router> component
│       │   ├── Pages/
│       │   │   └── Dashboard.razor            # The single page — header, timeline, heatmap
│       │   └── Layout/
│       │       └── MainLayout.razor           # Minimal @Body wrapper
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css              # All styles (ported from OriginalDesignConcept.html)
│       │   └── data.json                      # Sample data (Project Atlas)
│       └── Properties/
│           └── launchSettings.json            # Dev profile: http://localhost:5050
│
├── tests/
│   └── ReportingDashboard.Tests/
│       ├── ReportingDashboard.Tests.csproj     # xUnit + bUnit + Test SDK
│       ├── DataServiceTests.cs                # JSON loading, error handling, schema validation
│       ├── TimelineCalculationTests.cs        # Date-to-pixel math
│       └── DashboardComponentTests.cs         # bUnit render verification
│
├── OriginalDesignConcept.html                  # Visual reference (DO NOT MODIFY)
└── README.md                                   # Setup, usage, data.json schema, screenshot guide
```

---

## Security Considerations

### Authentication & Authorization

**None.** This is a single-user local tool. No login, no roles, no tokens, no OAuth, no cookies. The application binds to `localhost` only, making it inaccessible from other machines on the network.

```csharp
// Program.cs — explicit localhost binding
app.Urls.Clear();
app.Urls.Add("http://localhost:5050");
```

### Data Protection

| Concern | Mitigation |
|---------|-----------|
| **Sensitive data in data.json** | `data.json` contains project status text (work item names, dates). It does not contain credentials, PII, or secrets. If project names are sensitive, rely on OS filesystem ACLs. |
| **No encryption at rest** | Not needed. The file lives on the user's local machine under their user profile. |
| **No encryption in transit** | HTTP (not HTTPS) on localhost. Traffic never leaves the loopback adapter. HTTPS adds certificate management complexity for zero security benefit on localhost. |
| **No secrets in source** | The project contains no connection strings, API keys, or tokens. `data.json` is user-generated content, not application secrets. |

### Input Validation

| Input | Validation |
|-------|-----------|
| `data.json` content | Deserialized via `System.Text.Json` with `required` properties. Missing fields throw `JsonException` caught by `DataService`. |
| `schemaVersion` | Must equal `1`. Other values produce a clear error message. |
| Date strings | Parsed via `DateOnly.ParseExact(value, "yyyy-MM-dd")`. Invalid dates caught and reported. |
| URL (`backlogUrl`) | Rendered as an `<a href>`. No server-side URL validation needed — it's a client-side navigation. XSS risk is zero because Blazor auto-encodes all rendered strings. |
| Text content | All strings rendered through Blazor's Razor engine, which HTML-encodes by default. No raw HTML injection is possible. |

### Blazor-Specific Security

- **SignalR circuit:** Blazor Server's built-in circuit management handles connection lifecycle. No custom SignalR hubs are exposed.
- **Antiforgery:** Enabled by default in .NET 8 Blazor (`app.UseAntiforgery()`). Not critical for a local tool but left enabled as defense-in-depth.
- **Static file serving:** Only `wwwroot/` contents are served. The `FileSystemWatcher` reads from `wwwroot/data.json` — it cannot be tricked into reading files outside `wwwroot` because the path is hardcoded.

---

## Scaling Strategy

### This Application Does Not Scale — By Design

The Executive Reporting Dashboard is a **single-user, single-machine, localhost-only tool**. Scaling is explicitly out of scope. The architecture is optimized for simplicity, not throughput.

| Dimension | Current Capacity | Why Scaling Is Unnecessary |
|-----------|-----------------|---------------------------|
| **Users** | 1 concurrent browser tab | Single PM on their laptop |
| **Data volume** | ~100 work items, ~20 milestones | Executive dashboards summarize, not enumerate |
| **Instances** | 1 per machine | Each PM runs their own copy |
| **Multi-project** | 1 project per `data.json` | Swap the file or run multiple instances on different ports |

### If Future Scaling Were Needed

| Scenario | Approach |
|----------|---------|
| **Multiple projects** | Phase 3: Add a `data/` directory with multiple JSON files; add a Blazor dropdown to select the active file. No architectural change to `DataService` (parameterize the file path). |
| **Team-wide access** | Change binding from `localhost` to `0.0.0.0`. Add HTTPS. Consider authentication (Azure AD with MSAL). This is a Phase 3+ rewrite of the hosting model, not the rendering logic. |
| **Larger data sets** | The rendering model (CSS Grid + SVG) handles up to ~200 heatmap items and ~50 milestones before the 1920×1080 viewport runs out of physical space. Beyond that, the PM spec needs to change (e.g., pagination, filtering). |

---

## UI Component Architecture

This section maps each visual section from `OriginalDesignConcept.html` to its Blazor implementation, CSS layout strategy, data bindings, and interactions.

### Component: Dashboard.razor (Single Page, Three Sections)

The entire UI is rendered in one `Dashboard.razor` file. Sub-components are not needed at this scale — the file will be ~200 lines of Razor markup.

---

### Section 1: Header Bar → `<div class="hdr">`

| Aspect | Detail |
|--------|--------|
| **Visual reference** | `.hdr` in `OriginalDesignConcept.html` |
| **CSS layout** | `display: flex; align-items: center; justify-content: space-between` |
| **Height** | Auto (~50px), flex-shrink: 0 |
| **Padding** | `12px 44px 10px` |
| **Border** | `1px solid var(--border-light)` bottom |

**Left side (title + subtitle):**

```razor
<div class="hdr">
  <div>
    <h1>@data.Title <a href="@data.BacklogUrl">↗ ADO Backlog</a></h1>
    <div class="sub">@data.Subtitle</div>
  </div>
```

| Data Binding | Source |
|-------------|--------|
| Title text | `data.Title` |
| Backlog URL | `data.BacklogUrl` |
| Subtitle | `data.Subtitle` |

**Right side (legend):**

Four inline `<span>` elements with icon + label. These are **not data-driven** — the legend is fixed (PoC, Production, Checkpoint, Now). Rendered as static markup with CSS-styled icons:

- Gold diamond: `width:12px; height:12px; background: var(--amber); transform: rotate(45deg)`
- Green diamond: same with `var(--green)`
- Gray circle: `width:8px; height:8px; border-radius:50%; background: var(--checkpoint)`
- Red line: `width:2px; height:14px; background: var(--red)`

---

### Section 2: Timeline Area → `<div class="tl-area">`

| Aspect | Detail |
|--------|--------|
| **Visual reference** | `.tl-area` in `OriginalDesignConcept.html` |
| **CSS layout** | `display: flex; align-items: stretch` |
| **Height** | Fixed 196px, flex-shrink: 0 |
| **Background** | `var(--surface)` (`#FAFAFA`) |
| **Border** | `2px solid var(--border-heavy)` bottom |
| **Padding** | `6px 44px 0` |

**Sub-section 2a: Workstream Label Sidebar (230px)**

```razor
<div class="tl-sidebar">
  @foreach (var ws in data.Timeline.Workstreams)
  {
    <div style="color: @ws.Color;">
      @ws.Id<br/><span class="tl-ws-name">@ws.Name</span>
    </div>
  }
</div>
```

| CSS | Detail |
|-----|--------|
| Width | `230px`, flex-shrink: 0 |
| Layout | `flex-direction: column; justify-content: space-around` |
| Border | `1px solid var(--border-light)` right |
| Typography | ID: 12px, weight 600, color = workstream color. Name: 12px, weight 400, `#444` |

| Data Binding | Source |
|-------------|--------|
| Workstream IDs | `data.Timeline.Workstreams[].Id` |
| Workstream names | `data.Timeline.Workstreams[].Name` |
| Workstream colors | `data.Timeline.Workstreams[].Color` |

**Sub-section 2b: SVG Timeline Canvas**

Rendered as an inline `<svg>` element with calculated positions:

```razor
<div class="tl-svg-box">
  <svg xmlns="http://www.w3.org/2000/svg" width="1560" height="185"
       style="overflow:visible;display:block">
    <defs>
      <filter id="sh">
        <feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/>
      </filter>
    </defs>

    @* Month gridlines *@
    @foreach (var (label, x) in monthGridlines)
    {
      <line x1="@x" y1="0" x2="@x" y2="185" stroke="#bbb"
            stroke-opacity="0.4" stroke-width="1"/>
      <text x="@(x+5)" y="14" fill="#666" font-size="11" font-weight="600"
            font-family="Segoe UI,Arial">@label</text>
    }

    @* NOW line *@
    <line x1="@nowX" y1="0" x2="@nowX" y2="185" stroke="#EA4335"
          stroke-width="2" stroke-dasharray="5,3"/>
    <text x="@(nowX+4)" y="14" fill="#EA4335" font-size="10"
          font-weight="700" font-family="Segoe UI,Arial">NOW</text>

    @* Workstream lines and milestones *@
    @for (int i = 0; i < workstreams.Length; i++)
    {
      var ws = workstreams[i];
      var y = yLanes[i];  // 42, 98, 154

      <line x1="0" y1="@y" x2="1560" y2="@y"
            stroke="@ws.Color" stroke-width="3"/>

      @foreach (var ms in ws.Milestones)
      {
        var x = CalculateX(ms.Date);
        @* Render marker based on ms.Type *@
      }
    }
  </svg>
</div>
```

**Timeline calculation (C# in `@code` block):**

```csharp
private const double SvgWidth = 1560.0;
private static readonly int[] YLanes = { 42, 98, 154 };

private double CalculateX(string dateStr)
{
    var date = DateOnly.ParseExact(dateStr, "yyyy-MM-dd");
    var start = DateOnly.ParseExact(data.Timeline.StartDate, "yyyy-MM-dd");
    var end = DateOnly.ParseExact(data.Timeline.EndDate, "yyyy-MM-dd");
    var totalDays = end.DayNumber - start.DayNumber;
    var elapsed = date.DayNumber - start.DayNumber;
    return (double)elapsed / totalDays * SvgWidth;
}
```

**Milestone marker rendering by type:**

| Type | SVG Element | Style |
|------|-------------|-------|
| `start` | `<circle r="7" fill="white" stroke="{wsColor}" stroke-width="2.5"/>` | Open circle |
| `checkpoint` | `<circle r="4" fill="#999"/>` | Small filled gray dot |
| `poc` | `<polygon points="{diamond}" fill="#F4B400" filter="url(#sh)"/>` | Gold diamond with shadow |
| `production` | `<polygon points="{diamond}" fill="#34A853" filter="url(#sh)"/>` | Green diamond with shadow |

Diamond polygon points for center (cx, cy): `"cx,cy-11 cx+11,cy cx,cy+11 cx-11,cy"`

**Milestone label positioning:**
- Default: alternate above (y - 16) and below (y + 24) per milestone to avoid overlap
- Override: use `milestone.LabelPosition` if specified in JSON
- Text: `<text text-anchor="middle" fill="#666" font-size="10">`

---

### Section 3: Heatmap Grid → `<div class="hm-wrap">`

| Aspect | Detail |
|--------|--------|
| **Visual reference** | `.hm-wrap`, `.hm-grid` in `OriginalDesignConcept.html` |
| **CSS layout** | `display: flex; flex-direction: column; flex: 1; min-height: 0` |
| **Padding** | `10px 44px 10px` |

**Section title:**

```razor
<div class="hm-title">
  Monthly Execution Heatmap — Shipped · In Progress · Carryover · Blockers
</div>
```

**Grid structure:**

```razor
<div class="hm-grid" style="grid-template-columns: 160px repeat(@monthCount, 1fr);">
  @* Header row *@
  <div class="hm-corner">Status</div>
  @foreach (var month in data.Heatmap.MonthColumns)
  {
    var isNow = month == currentMonthName;
    <div class="hm-col-hdr @(isNow ? "now-hdr" : "")">
      @month @(isNow ? "◀ Now" : "")
    </div>
  }

  @* Status rows *@
  @foreach (var cat in data.Heatmap.Categories)
  {
    <div class="hm-row-hdr @(cat.CssClass)-hdr">@cat.Emoji @cat.Name</div>
    @foreach (var mi in cat.Months)
    {
      var isNow = mi.Month == currentMonthName;
      <div class="hm-cell @(cat.CssClass)-cell @(isNow ? "now" : "")">
        @if (mi.Items.Length == 0)
        {
          <div class="it empty">—</div>
        }
        else
        {
          @foreach (var item in mi.Items)
          {
            <div class="it">@item</div>
          }
        }
      </div>
    }
  }
</div>
```

**CSS Grid dynamic column count:**

The `grid-template-columns` is set via an inline `style` attribute because the number of months is data-driven: `160px repeat(@monthCount, 1fr)`.

The `grid-template-rows` is always `36px repeat(4, 1fr)` (hardcoded in CSS for 4 status categories).

**Current-month highlighting:**

The `currentMonthName` is computed from the effective date:

```csharp
private string currentMonthName =>
    effectiveDate.ToString("MMM");  // "Jan", "Feb", etc.
```

CSS classes for current-month cells use the `.now` modifier instead of the hardcoded `.apr` from the reference. This is the key change from the static HTML — the highlight is dynamic:

```css
.ship-cell.now { background: var(--green-cell-active); }
.prog-cell.now { background: var(--blue-cell-active); }
.carry-cell.now { background: var(--amber-cell-active); }
.block-cell.now { background: var(--red-cell-active); }
.hm-col-hdr.now-hdr { background: var(--now-bg); color: var(--now-text); }
```

**Data bindings for heatmap:**

| Binding | Source |
|---------|--------|
| Month column headers | `data.Heatmap.MonthColumns[]` |
| Status row names | `data.Heatmap.Categories[].Name` |
| Status row CSS class | `data.Heatmap.Categories[].CssClass` |
| Status row emoji | `data.Heatmap.Categories[].Emoji` |
| Cell items | `data.Heatmap.Categories[].Months[].Items[]` |
| Current month | Auto-calculated from effective date |

---

### Error Banner Component (Inline)

When `DataService.GetError()` is non-null but `DataService.GetData()` returns valid last-known-good data, an error banner is rendered above the header:

```razor
@if (error != null)
{
  <div class="error-banner">
    ⚠ data.json has errors — showing last valid data. Error: @error
  </div>
}
```

```css
.error-banner {
    background: #FFF3CD;
    color: #856404;
    padding: 8px 44px;
    font-size: 12px;
    border-bottom: 1px solid #FFE69C;
    flex-shrink: 0;
}
```

### Full-Page Error State

When there is no valid data at all (missing file or first-load parse failure):

```razor
@if (data == null)
{
  <div class="error-page">
    <div class="error-message">
      <h2>⚠ Dashboard Data Error</h2>
      <p>@error</p>
      <p>Place a valid <code>data.json</code> file in the <code>wwwroot/</code> directory.</p>
    </div>
  </div>
}
```

Styled with the same Segoe UI font and color scheme, centered vertically and horizontally in the 1920×1080 viewport.

---

## Risks & Mitigations

### Risk 1: CSS Pixel-Perfection Across Browsers

| Attribute | Detail |
|-----------|--------|
| **Severity** | Medium |
| **Probability** | Medium |
| **Impact** | Screenshots may differ 1-2px between Edge and Chrome due to font rendering, sub-pixel anti-aliasing, and CSS Grid rounding |

**Mitigation:**
- Standardize on **Microsoft Edge** as the screenshot browser (Segoe UI renders best on Windows in Edge)
- Document Edge as the reference browser in the README
- Use Edge's built-in `Ctrl+Shift+S → Capture full page` for screenshots
- Test visual output in Edge during development; Chrome parity is "best effort"
- Use fixed pixel values (not `em`/`rem`) for all layout-critical dimensions

### Risk 2: FileSystemWatcher Reliability on Windows

| Attribute | Detail |
|-----------|--------|
| **Severity** | Low |
| **Probability** | Low-Medium |
| **Impact** | Missed or duplicate file change events; dashboard doesn't update or updates twice |

**Mitigation:**
- Debounce file change events with a 500ms `System.Threading.Timer` — collapse rapid-fire events into a single reload
- Watch for `Changed`, `Created`, and `Renamed` events (editors like VS Code use atomic rename-on-save)
- If `FileSystemWatcher` fails entirely, the user can fall back to `F5` browser refresh — the data is always re-read from disk on component initialization
- Log file watch events to console for debugging

### Risk 3: data.json Schema Drift

| Attribute | Detail |
|-----------|--------|
| **Severity** | Low |
| **Probability** | Low (schema is simple and stable) |
| **Impact** | Existing `data.json` files break after application updates |

**Mitigation:**
- `schemaVersion` field in `data.json` (starts at `1`)
- `DataService` validates the version on load and produces a clear error: "Expected schemaVersion 1, got {n}"
- `required` properties on all C# records — missing fields produce descriptive `JsonException` messages
- Sample `data.json` ships with the project as a reference

### Risk 4: SVG Timeline Label Overlap

| Attribute | Detail |
|-----------|--------|
| **Severity** | Low |
| **Probability** | Medium (depends on milestone density) |
| **Impact** | Milestone date labels overlap and become unreadable |

**Mitigation:**
- Default behavior: alternate label placement above/below the workstream line
- `LabelPosition` field in `Milestone` model allows manual override per milestone
- Minimum X-gap constant (40px): if two milestones on the same workstream are within 40px, offset the second label to the opposite side
- Unit tests verify label positions for edge cases (same-day milestones, month-boundary milestones)

### Risk 5: Blazor Server SignalR Overhead

| Attribute | Detail |
|-----------|--------|
| **Severity** | Low |
| **Probability** | Certain (inherent to Blazor Server) |
| **Impact** | ~2MB additional memory per circuit; WebSocket kept alive for a static page |

**Mitigation:**
- Acceptable trade-off: the overhead is negligible for single-user local use
- Benefits outweigh costs: `StateHasChanged()` enables live reload push without JavaScript
- `dotnet watch` hot reload works seamlessly with Blazor Server for development iteration
- If overhead ever becomes a concern (it won't for this use case), the Razor markup can be migrated to Static SSR mode in .NET 8 with minimal changes

### Risk 6: Large Heatmap Overflows 1080px Viewport

| Attribute | Detail |
|-----------|--------|
| **Severity** | Medium |
| **Probability** | Low (depends on item count) |
| **Impact** | Content exceeds the 1080px height; gets clipped by `overflow: hidden` |

**Mitigation:**
- The heatmap uses `flex: 1` to fill remaining viewport height after header (50px) and timeline (196px) — leaving ~834px for the heatmap including padding
- With 4 status rows, each row gets ~190px of height — sufficient for 10-12 bulleted items at 12px font + 1.35 line-height
- Document maximum item counts per cell in README: recommend ≤ 8 items per cell for clean rendering
- If more items are needed, the PM should split into sub-categories or summarize