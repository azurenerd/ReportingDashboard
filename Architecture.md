# Architecture

## Overview & Goals

The Executive Project Reporting Dashboard is a single-page, local-only Blazor Server web application that renders a screenshot-ready project status dashboard for executive reporting. The system reads all data from a flat `data.json` file and renders milestones, shipped work, in-progress items, carried-over items, and a visual timeline—optimized for visual clarity, print-readiness, and zero operational complexity.

### Architectural Principles

1. **Simplicity over abstraction.** No repository pattern, no CQRS, no mediator. The data flow is linear: file → service → component → HTML/CSS.
2. **Zero external dependencies.** The application uses only `Microsoft.AspNetCore.App` framework reference. No NuGet packages, no NPM, no CDN resources.
3. **Visual fidelity is the primary quality attribute.** Every architectural decision serves the goal of producing consistent, professional screenshots at 1200px width.
4. **File-driven configuration.** `data.json` is the sole data source. No database, no API, no admin UI.
5. **Local-only execution.** HTTP on localhost, single user, no network exposure, no authentication.

### Architecture Style

**Flat File → Service → Server-Rendered Component**

```
data.json ──→ DashboardDataService ──→ Dashboard.razor ──→ HTML/CSS (browser)
  (file)        (C# singleton, DI)      (Razor page)       (Kestrel → SignalR)
```

This is a deliberate single-tier, single-process architecture. There is no client/server separation beyond what Blazor Server provides natively via its SignalR circuit.

---

## System Components

### Component 1: `Program.cs` — Application Host

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Configure and start the Blazor Server application host |
| **Interfaces** | None (entry point) |
| **Dependencies** | `DashboardDataService` (registers into DI), Blazor Server middleware |
| **Data** | Reads `appsettings.json` for optional port/path configuration |

**Behavior:**
- Registers Razor components with interactive server rendering mode.
- Registers `DashboardDataService` as a singleton in the DI container.
- Configures Kestrel to listen on `http://localhost:5000` (HTTP only, no HTTPS).
- Wires up static file serving, antiforgery, and Razor component mapping.
- Total: ~15 lines of code.

```
WebApplicationBuilder
  ├── AddRazorComponents().AddInteractiveServerComponents()
  ├── AddSingleton<DashboardDataService>()
  └── Build() → WebApplication
        ├── UseStaticFiles()
        ├── UseAntiforgery()
        ├── MapRazorComponents<App>().AddInteractiveServerRenderMode()
        └── Run()
```

---

### Component 2: `DashboardData.cs` — Data Models

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define the strongly-typed shape of `data.json` using C# records |
| **Interfaces** | Consumed by `DashboardDataService` and all Razor components |
| **Dependencies** | None (pure data types) |
| **Data** | Immutable record types with nullable properties for graceful degradation |

**Record Hierarchy:**

| Record | Fields | Notes |
|--------|--------|-------|
| `DashboardData` | `Project`, `Milestones`, `Shipped`, `InProgress`, `CarriedOver`, `CurrentMonth` | Root object; all collections default to empty lists |
| `ProjectInfo` | `Name`, `Lead`, `Status`, `LastUpdated`, `OverallHealth`, `Summary` | `Summary` and `OverallHealth` are nullable |
| `Milestone` | `Title`, `Date`, `Status`, `Description` | `Status`: `"completed"`, `"in-progress"`, `"upcoming"`, `"at-risk"` |
| `WorkItem` | `Title`, `Description`, `Status`, `Category`, `PercentComplete` | `PercentComplete` is `int?` (nullable); `Category` is nullable |
| `MonthSummary` | `TotalItems`, `CompletedItems`, `CarriedItems`, `OverallHealth` | `OverallHealth`: `"on-track"`, `"at-risk"`, `"behind"` |

**Design Decisions:**
- All records use `{ get; init; }` property syntax (not positional records) for clarity and JSON deserialization compatibility.
- Collection properties default to `[]` (empty list) so null arrays in JSON don't cause `NullReferenceException`.
- String properties that represent enumerated states (`Status`, `OverallHealth`) remain `string` rather than C# enums, to avoid deserialization friction and allow forward-compatible new values.

---

### Component 3: `DashboardDataService.cs` — Data Access Service

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Read, deserialize, validate, and optionally watch `data.json` |
| **Interfaces** | `GetDashboardDataAsync(): Task<DashboardData>`, `OnDataChanged` event |
| **Dependencies** | `IWebHostEnvironment` (for path resolution), `System.Text.Json`, `FileSystemWatcher` |
| **Data** | Reads from `wwwroot/data/data.json` (configurable via `appsettings.json`) |
| **Lifetime** | Singleton (one instance for the app's lifetime) |

**Subcomponents:**

1. **Data Reader** — `File.ReadAllTextAsync` → `JsonSerializer.Deserialize<DashboardData>`.
2. **Error Handler** — Wraps deserialization in try-catch; returns a sentinel error object with a user-friendly message instead of throwing.
3. **File Watcher** (optional enhancement) — `FileSystemWatcher` monitors `data.json` for changes, debounces rapid saves (300ms), and raises `OnDataChanged` event.

**Data Path Resolution (priority order):**
1. `appsettings.json` → `"Dashboard:DataFilePath"` (absolute or relative path)
2. Default: `{WebRootPath}/data/data.json`

**Deserialization Options:**
```
PropertyNameCaseInsensitive = true
PropertyNamingPolicy = JsonNamingPolicy.CamelCase
DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
```

**Error Model:**
When `data.json` is missing, malformed, or unreadable, the service returns a `DashboardData` object with a populated `ErrorMessage` property (nullable string on the root record). The Razor page checks this field and renders a user-friendly error banner instead of the dashboard content.

---

### Component 4: `App.razor` — Root Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Define the HTML document shell (`<html>`, `<head>`, `<body>`), reference CSS, set up Blazor routing |
| **Interfaces** | Blazor component tree root |
| **Dependencies** | `Routes.razor`, `dashboard.css` |
| **Data** | None directly |

**Structure:**
- `<head>`: references `css/dashboard.css`, sets viewport and charset.
- `<body>`: renders `<Routes />` component, includes Blazor Server script tag (`_framework/blazor.server.js`).
- No layout component needed (single page, no navigation chrome).

---

### Component 5: `Dashboard.razor` — Main Page Component

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | Render the entire dashboard UI from `DashboardData` |
| **Interfaces** | Blazor page at route `/` |
| **Dependencies** | `DashboardDataService` (injected), `DashboardData` models |
| **Data** | `DashboardData` loaded in `OnInitializedAsync` |

**Internal Sections (rendered top-to-bottom):**

| Section | Data Source | Visual Treatment |
|---------|-----------|-----------------|
| **Project Header** | `DashboardData.Project` | Project name, lead, status badge, last-updated date, summary paragraph |
| **Monthly Summary Metrics** | `DashboardData.CurrentMonth` | 4-card metric strip: total items, completed, carried, overall health |
| **Milestone Timeline** | `DashboardData.Milestones` | Horizontal CSS Grid timeline with color-coded status markers |
| **Shipped Items** | `DashboardData.Shipped` | Card list with green left border, 100% progress bars |
| **In-Progress Items** | `DashboardData.InProgress` | Card list with blue left border, proportional progress bars |
| **Carried-Over Items** | `DashboardData.CarriedOver` | Card list with orange left border, carryover reason text |

**Lifecycle:**
1. `OnInitializedAsync`: call `DashboardDataService.GetDashboardDataAsync()`, store result.
2. If `ErrorMessage` is set, render error banner instead of dashboard.
3. If `OnDataChanged` (FileSystemWatcher), call `InvokeAsync(StateHasChanged)` to re-render.

**Auto-Refresh Wiring (optional enhancement):**
- In `OnInitializedAsync`, subscribe to `DashboardDataService.OnDataChanged`.
- Event handler calls `await InvokeAsync(StateHasChanged)` to push updates over SignalR.
- In `Dispose`, unsubscribe from the event to prevent memory leaks. Component implements `IDisposable`.

---

### Component 6: `dashboard.css` — Stylesheet

| Attribute | Detail |
|-----------|--------|
| **Responsibility** | All visual styling: layout, typography, colors, timeline, progress bars, print styles |
| **Interfaces** | Referenced by `App.razor` via `<link>` |
| **Dependencies** | None (pure CSS, no preprocessor) |
| **Data** | CSS custom properties for theming |

**CSS Architecture:**

```
dashboard.css
├── CSS Custom Properties (:root)
│   ├── Color palette (--color-success, --color-info, --color-warning, --color-danger, --color-muted)
│   ├── Typography (--font-family, --font-size-body, --font-size-heading)
│   ├── Layout (--max-width, --spacing-*)
│   └── Status colors (--status-completed, --status-in-progress, --status-at-risk, --status-upcoming, --status-carried-over)
├── Base Styles
│   ├── Box-sizing reset (* { box-sizing: border-box; })
│   ├── System font stack
│   ├── Body: background, font-size 14px minimum
│   └── Fixed-width container: max-width 1200px, margin 0 auto
├── Header Section
│   ├── Project name (h1, 24px+)
│   ├── Metadata row (lead, status badge, date)
│   ├── Status badge (color-coded pill)
│   └── Summary paragraph
├── Metrics Strip
│   ├── CSS Flexbox, 4 equal cards
│   ├── Large numeric values (28px+)
│   └── Health indicator color-coding
├── Milestone Timeline
│   ├── CSS Grid container
│   ├── Connecting line (border-top or ::before pseudo-element)
│   ├── Milestone markers (circles with status colors)
│   ├── Date labels below markers
│   └── Title labels above/below alternating
├── Work Item Cards
│   ├── Card container (border-left: 4px solid [status-color])
│   ├── Title, description, category tag
│   ├── Progress bar (pure CSS: outer track + inner fill via width percentage)
│   └── Status-specific variants (green/blue/orange borders)
├── Section Headers
│   ├── Section title (h2, 18px+)
│   └── Item count badge
└── @media print
    ├── Hide Blazor SignalR artifacts
    ├── Remove box-shadows
    ├── Force white background
    ├── Ensure page breaks between sections
    └── Hide any browser chrome
```

---

## Component Interactions

### Primary Data Flow

```
┌──────────────┐     read file      ┌──────────────────────┐
│  data.json   │ ──────────────────→ │  DashboardDataService │
│  (flat file) │                     │  (singleton, DI)      │
└──────────────┘                     └──────────┬───────────┘
                                                │
                                     deserialize to DashboardData
                                                │
                                                ▼
                                     ┌──────────────────────┐
                                     │   Dashboard.razor     │
                                     │   (Blazor component)  │
                                     └──────────┬───────────┘
                                                │
                                      render HTML + CSS
                                                │
                                                ▼
                                     ┌──────────────────────┐
                                     │   Browser (Chrome)    │
                                     │   via SignalR circuit  │
                                     └──────────────────────┘
```

### Auto-Refresh Flow (Optional Enhancement)

```
┌──────────────┐   file saved    ┌─────────────────┐  OnDataChanged   ┌─────────────────┐
│  data.json   │ ──────────────→ │ FileSystemWatcher│ ───────────────→ │ Dashboard.razor  │
│  (edited by  │                 │ (in service)     │   (C# event)     │ StateHasChanged()│
│   user)      │                 └─────────────────┘                   └────────┬────────┘
└──────────────┘                                                                │
                                                                     SignalR push to browser
                                                                                │
                                                                                ▼
                                                                     ┌──────────────────┐
                                                                     │  Browser re-renders│
                                                                     │  (no manual refresh)│
                                                                     └──────────────────┘
```

### Startup Sequence

```
1. dotnet run
2. Program.cs: WebApplicationBuilder configures services
3. Kestrel starts listening on http://localhost:5000
4. DashboardDataService instantiated (singleton)
5. FileSystemWatcher begins monitoring data.json (if implemented)
6. User navigates to http://localhost:5000
7. Blazor Server establishes SignalR circuit
8. Dashboard.razor.OnInitializedAsync() fires
9. DashboardDataService.GetDashboardDataAsync() reads data.json
10. System.Text.Json deserializes to DashboardData
11. Razor renders HTML, Blazor diffs and sends to browser
12. Browser paints the dashboard — ready for screenshot
```

### Error Flow

```
data.json missing/malformed
        │
        ▼
DashboardDataService catches JsonException / FileNotFoundException
        │
        ▼
Returns DashboardData with ErrorMessage populated
        │
        ▼
Dashboard.razor checks ErrorMessage != null
        │
        ▼
Renders error banner: "Unable to load dashboard data. 
Check that data.json exists and is valid JSON."
```

---

## Data Model

### Entity Relationship Diagram

```
DashboardData (root)
├── Project: ProjectInfo (1:1)
├── Milestones: List<Milestone> (1:N)
├── Shipped: List<WorkItem> (1:N)
├── InProgress: List<WorkItem> (1:N)
├── CarriedOver: List<WorkItem> (1:N)
├── CurrentMonth: MonthSummary (1:1)
└── ErrorMessage: string? (null when healthy)
```

### Record Definitions

```csharp
public record DashboardData
{
    public ProjectInfo? Project { get; init; }
    public List<Milestone> Milestones { get; init; } = [];
    public List<WorkItem> Shipped { get; init; } = [];
    public List<WorkItem> InProgress { get; init; } = [];
    public List<WorkItem> CarriedOver { get; init; } = [];
    public MonthSummary? CurrentMonth { get; init; }
    public string? ErrorMessage { get; init; }
}

public record ProjectInfo
{
    public string Name { get; init; } = "Untitled Project";
    public string? Lead { get; init; }
    public string Status { get; init; } = "Unknown";
    public string? LastUpdated { get; init; }
    public string? OverallHealth { get; init; }
    public string? Summary { get; init; }
}

public record Milestone
{
    public string Title { get; init; } = "";
    public string? Date { get; init; }
    public string Status { get; init; } = "upcoming";
    public string? Description { get; init; }
}

public record WorkItem
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public string Status { get; init; } = "in-progress";
    public string? Category { get; init; }
    public int? PercentComplete { get; init; }
}

public record MonthSummary
{
    public int TotalItems { get; init; }
    public int CompletedItems { get; init; }
    public int CarriedItems { get; init; }
    public string? OverallHealth { get; init; }
}
```

### Storage: `data.json`

| Attribute | Detail |
|-----------|--------|
| **Format** | JSON (UTF-8, no BOM) |
| **Location** | `wwwroot/data/data.json` (default), overridable via `appsettings.json` |
| **Max expected size** | < 50 KB |
| **Editing** | Manual, via any text editor or VS Code |
| **Schema enforcement** | Implicit via C# record types; unknown fields are silently ignored |
| **Backup** | Not managed by the application; user's responsibility |

### Status Value Enumerations (String Constants)

**Milestone Status:** `"completed"` | `"in-progress"` | `"upcoming"` | `"at-risk"`

**Work Item Status:** `"completed"` | `"in-progress"` | `"carried-over"` | `"at-risk"`

**Overall Health:** `"on-track"` | `"at-risk"` | `"behind"`

These are intentionally kept as strings (not C# enums) for JSON forward-compatibility and to avoid deserialization failures when new values are added.

### CSS Color Mapping for Status Values

| Status Value | CSS Custom Property | Color | Usage |
|-------------|-------------------|-------|-------|
| `completed` | `--status-completed` | `#2E7D32` (green) | Shipped items, completed milestones |
| `in-progress` | `--status-in-progress` | `#1565C0` (blue) | Active work items, current milestones |
| `at-risk` | `--status-at-risk` | `#E65100` (orange) | At-risk milestones, at-risk health |
| `carried-over` | `--status-carried-over` | `#F57C00` (amber) | Deferred items |
| `upcoming` | `--status-upcoming` | `#757575` (gray) | Future milestones |
| `behind` | `--status-behind` | `#C62828` (red) | Behind-schedule health status |
| `on-track` | `--status-on-track` | `#2E7D32` (green) | On-track health status |

---

## API Contracts

### External APIs

**None.** This application exposes no HTTP API endpoints, REST or otherwise. All data flows are internal: file → service → component. There are no controllers, no minimal API endpoints, and no Web API surfaces.

### Internal Service Contract

#### `DashboardDataService`

```csharp
public class DashboardDataService : IDisposable
{
    // Reads and deserializes data.json. Never throws; returns ErrorMessage on failure.
    public Task<DashboardData> GetDashboardDataAsync();

    // Raised when FileSystemWatcher detects data.json has changed.
    // Subscribers should call InvokeAsync(StateHasChanged) in Blazor components.
    public event Action? OnDataChanged;

    // Cleans up FileSystemWatcher resources.
    public void Dispose();
}
```

**GetDashboardDataAsync Contract:**

| Scenario | Return Value |
|----------|-------------|
| `data.json` exists and is valid | Fully populated `DashboardData` with `ErrorMessage = null` |
| `data.json` exists but is malformed JSON | `DashboardData` with `ErrorMessage = "Invalid JSON in data.json: {details}"` |
| `data.json` does not exist | `DashboardData` with `ErrorMessage = "data.json not found at {path}"` |
| File read permission denied | `DashboardData` with `ErrorMessage = "Unable to read data.json: {details}"` |
| `data.json` exists but missing required sections | Partially populated `DashboardData`; missing sections render as empty (graceful degradation) |

### Static File Serving

| Path | Serves |
|------|--------|
| `/css/dashboard.css` | Main stylesheet |
| `/data/data.json` | Project data (also readable by service via file system) |
| `/_framework/blazor.server.js` | Blazor Server SignalR client (framework-provided) |

---

## Infrastructure Requirements

### Runtime Requirements

| Requirement | Specification |
|------------|---------------|
| **.NET SDK** | .NET 8.0.x (any recent patch) |
| **Operating System** | Windows 10/11, macOS 12+, or Linux (any OS supported by .NET 8) |
| **Memory** | < 100 MB (Kestrel + Blazor Server overhead for single user) |
| **Disk** | < 10 MB for application files |
| **Network** | None required at runtime; localhost-only binding |
| **Browser** | Chrome 90+ or Edge 90+ (for viewing/screenshotting) |

### Hosting Configuration

| Setting | Value | Rationale |
|---------|-------|-----------|
| **Protocol** | HTTP only | No HTTPS to avoid localhost certificate complexity |
| **Bind address** | `http://localhost:5000` | Localhost only; not exposed to network |
| **Server** | Kestrel (in-process) | No IIS, no reverse proxy |
| **Static files** | Served from `wwwroot/` | Default Blazor Server behavior |

### `launchSettings.json`

```json
{
  "profiles": {
    "ReportingDashboard": {
      "commandName": "Project",
      "applicationUrl": "http://localhost:5000",
      "environmentName": "Development"
    }
  }
}
```

### CI/CD

**Not required.** This is a local-only tool. No build pipeline, no deployment infrastructure, no container registry.

If future sharing is desired:
- `dotnet publish -c Release -o ./publish`
- Copy the `publish/` folder to the target machine
- Run `dotnet ReportingDashboard.dll`

### Storage

| Storage | Technology | Location |
|---------|-----------|----------|
| **Application data** | Single JSON file | `wwwroot/data/data.json` |
| **Configuration** | `appsettings.json` | Project root (optional) |
| **Static assets** | CSS file | `wwwroot/css/dashboard.css` |

No database. No file-based database (SQLite). No cache. No session storage beyond Blazor Server's in-memory circuit state.

---

## Technology Stack Decisions

| Decision | Choice | Justification |
|----------|--------|---------------|
| **Application framework** | Blazor Server (.NET 8) | Mandated by project requirements. Server-side rendering is ideal for screenshot-oriented output. |
| **Rendering mode** | Interactive Server | Enables SignalR-based auto-refresh when `data.json` changes. Static SSR would also work but loses live update capability. |
| **JSON library** | `System.Text.Json` | Built into .NET 8. Zero additional dependency. Preferred over Newtonsoft.Json for new projects. |
| **Data models** | C# `record` types | Immutable, concise, perfect for deserialized configuration data. Value equality semantics are a bonus. |
| **CSS approach** | Custom CSS with CSS Custom Properties | No framework (Bootstrap, Tailwind) to avoid fighting the custom design from the HTML template. CSS Grid for timeline, Flexbox for cards. |
| **UI component library** | None | MudBlazor/Radzen would add 500KB+ of CSS/JS, fight the custom design, and complicate screenshot consistency. |
| **Font strategy** | System font stack | `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif`. Zero external font loading, instant rendering, no network dependency. |
| **Icons** | Unicode symbols / inline SVG | `✓`, `●`, `▶` for status indicators. No icon font library dependency. |
| **JavaScript** | Zero | Blazor Server handles all interactivity. No charting libraries, no jQuery, no external JS. |
| **File watching** | `FileSystemWatcher` (.NET built-in) | Enables auto-refresh on `data.json` edit. ~15 lines of code, no external dependency. |
| **Web server** | Kestrel (built-in) | Ships with .NET 8. No IIS, no Nginx, no reverse proxy. |
| **Testing** | Manual visual inspection | For a single-page dashboard consumed via screenshots, automated testing provides minimal ROI. If needed later: xUnit + bUnit. |
| **NuGet packages** | Zero (beyond framework reference) | Zero supply-chain risk, zero version conflicts, zero restore time. Everything needed ships with `Microsoft.AspNetCore.App`. |

### Rejected Alternatives

| Alternative | Reason for Rejection |
|-------------|---------------------|
| Blazor WebAssembly | Larger download, slower startup, no benefit for localhost use. Blazor Server is specified. |
| MudBlazor / Radzen | Fights custom design, bloated CSS/JS, harder to screenshot consistently. |
| SQLite / EF Core | Over-engineering for a < 50KB JSON file. Adds migration complexity, connection strings, NuGet dependencies. |
| ASP.NET MVC / Razor Pages | Would work, but Blazor Server is the mandated stack. Blazor also provides live update via SignalR for free. |
| Node.js / React | Not the mandated stack. |
| Tailwind CSS | Utility-first CSS is harder to maintain for a single-page app than a purpose-built stylesheet. Adds a build step. |
| SASS/LESS | Adds a preprocessor build step. CSS Custom Properties provide sufficient abstraction for this scope. |

---

## Security Considerations

### Threat Model

**Attack surface: Effectively zero.** The application binds exclusively to `localhost`, serves no API endpoints, accepts no user input beyond `data.json` file edits, and makes no outbound network calls.

| Threat | Risk Level | Mitigation |
|--------|-----------|------------|
| **Network access to dashboard** | None | Kestrel binds to `localhost` only; not reachable from other machines |
| **Data exposure** | None | `data.json` contains project metadata, not PII or credentials |
| **Supply-chain attack** | None | Zero external NuGet packages; only `Microsoft.AspNetCore.App` framework reference |
| **Injection attacks** | None | No user input fields, no forms, no query parameters, no database queries |
| **Cross-site scripting** | Negligible | Blazor Server auto-encodes rendered output; no raw HTML injection |
| **Denial of service** | None | Single-user localhost; no external traffic |
| **HTTPS/TLS** | Not needed | Localhost-only; no data in transit over network |

### Authentication & Authorization

**Not implemented. Not needed.**

- The application runs on `localhost` for a single user.
- If someone has access to your localhost, they already have full access to your machine and the `data.json` file.
- Adding authentication would add complexity with zero security benefit.

**Future consideration:** If the dashboard is ever shared on an intranet, add Windows Authentication:
```csharp
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
```

### Data Protection

| Data | Sensitivity | Protection |
|------|------------|------------|
| `data.json` (project status) | Low (no PII, no secrets) | None required |
| `data.json` (if project names are sensitive) | Medium | Add to `.gitignore`; `data.template.json` is committed instead |
| Application source code | Low | Standard source control practices |

### Input Validation

- `data.json` is deserialized via `System.Text.Json` with case-insensitive property matching.
- Malformed JSON is caught by try-catch and displayed as a user-friendly error message.
- Unknown/extra fields in JSON are silently ignored (no strict schema validation).
- Nullable C# properties ensure missing optional fields don't crash the app.
- No sanitization of string content is needed because Blazor's Razor engine auto-encodes all rendered text, preventing XSS even if `data.json` contained malicious HTML.

### Network Isolation

| Rule | Implementation |
|------|---------------|
| **No outbound HTTP calls** | No `HttpClient` usage anywhere in the application |
| **No CDN resources** | System fonts, no external CSS/JS, no analytics |
| **No telemetry** | No Application Insights, no usage tracking |
| **Localhost binding** | `applicationUrl: "http://localhost:5000"` in `launchSettings.json` |

---

## Scaling Strategy

### Current Scope: Not Applicable

This is a single-user, single-page, local-only application. It will never need to handle concurrent users, horizontal scaling, or load balancing. The "scaling strategy" is: **it doesn't need to scale.**

| Dimension | Current | If Ever Needed |
|-----------|---------|---------------|
| **Concurrent users** | 1 | Share via `dotnet publish` output on a shared network drive; each user runs their own instance |
| **Data volume** | < 50 KB (one project) | Multiple JSON files with route parameter: `/dashboard/{projectName}` loading `{projectName}.json` |
| **Multiple projects** | 1 per `data.json` | Add parameterized routing: `@page "/dashboard/{ProjectName}"` |
| **Team access** | localhost only | Deploy to intranet IIS with Windows Auth; or use `dotnet publish` + xcopy |

### Performance Targets

| Metric | Target | How Achieved |
|--------|--------|-------------|
| Startup time | < 5 seconds | Minimal DI registrations, no database connections, no package restore at runtime |
| Page load (first paint) | < 1 second | Server-side rendering, no external resources, < 50 KB data file |
| `data.json` read | < 100 ms | `System.Text.Json` deserializing < 50 KB from local SSD |
| Hot reload cycle | < 2 seconds | `dotnet watch` built-in capability for `.razor` and `.css` files |

---

## Risks & Mitigations

### Risk 1: Over-Engineering Scope Creep

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium |
| **Impact** | High (weeks of unnecessary work) |
| **Description** | Temptation to add database, admin UI, authentication, REST API, or UI component library |
| **Mitigation** | Architecture explicitly scopes out these features. `data.json` is the database. A text editor is the admin UI. The PM specification's "Out of Scope" section is the authority. |

### Risk 2: CSS Cross-Browser Screenshot Inconsistency

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low-Medium |
| **Impact** | Medium (visual inconsistencies in reports) |
| **Description** | Screenshots taken in different browsers may render slightly differently |
| **Mitigation** | Standardize on Chrome/Edge for screenshots. Fixed 1200px width eliminates responsive layout variability. Avoid browser-specific CSS features. Test in one browser and commit to it. |

### Risk 3: JSON Schema Evolution

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Medium |
| **Impact** | Low (graceful degradation built in) |
| **Description** | As the dashboard evolves, `data.json` may need new fields, breaking existing files |
| **Mitigation** | All new fields use nullable C# properties with defaults. `data.template.json` documents all fields. `JsonSerializerOptions` ignores unknown fields. Old `data.json` files continue to work without modification. |

### Risk 4: Blazor Server SignalR Circuit Staleness

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Negligible |
| **Description** | Blazor Server's SignalR connection goes stale if the server stops while the browser tab is open |
| **Mitigation** | Irrelevant for screenshot workflow: user starts server → loads page → screenshots → done. If the circuit drops, a browser refresh reconnects. |

### Risk 5: FileSystemWatcher Reliability

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Low |
| **Impact** | Low (fallback: manual browser refresh) |
| **Description** | `FileSystemWatcher` can miss events or fire duplicates on some OS/filesystem combinations |
| **Mitigation** | Debounce rapid events (300ms delay). If auto-refresh fails, manual F5 always works. This is a convenience feature, not a critical path. |

### Risk 6: Large data.json Degrading Render Performance

| Attribute | Detail |
|-----------|--------|
| **Likelihood** | Very Low |
| **Impact** | Low |
| **Description** | Extremely large `data.json` (hundreds of items) could slow rendering |
| **Mitigation** | Executive dashboards are inherently summary-level (5-20 items per section). If needed, add section-level item limits in the Razor template. The 50 KB target is well within `System.Text.Json` fast-path thresholds. |

---

## Appendix A: Solution Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard/
│       ├── ReportingDashboard.csproj
│       ├── Program.cs                          # Application host (~15 lines)
│       ├── Models/
│       │   └── DashboardData.cs                # All record types (~50 lines)
│       ├── Services/
│       │   └── DashboardDataService.cs         # Data reader + file watcher (~60 lines)
│       ├── Components/
│       │   ├── App.razor                       # HTML document shell
│       │   ├── Routes.razor                    # Blazor router
│       │   └── Pages/
│       │       └── Dashboard.razor             # The single dashboard page
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css               # All custom styles (~300 lines)
│       │   └── data/
│       │       └── data.json                   # Project data (user-edited)
│       └── Properties/
│           └── launchSettings.json             # HTTP localhost:5000
├── data.template.json                          # Documented schema template (committed)
├── .gitignore                                  # Includes wwwroot/data/data.json
└── README.md                                   # Setup and usage instructions
```

**Total C# code estimate:** < 150 lines across 3 files.
**Total CSS estimate:** < 300 lines in 1 file.
**Total Razor markup estimate:** < 200 lines in 1 page + 2 shell components.

## Appendix B: Configuration Schema

### `appsettings.json` (Optional Overrides)

```json
{
  "Dashboard": {
    "DataFilePath": "wwwroot/data/data.json",
    "EnableAutoRefresh": true,
    "AutoRefreshDebounceMs": 300
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

All settings have sensible defaults and are optional. The application runs with zero configuration out of the box.

## Appendix C: CSS Custom Properties Contract

```css
:root {
    /* Status Colors */
    --status-completed: #2E7D32;
    --status-in-progress: #1565C0;
    --status-at-risk: #E65100;
    --status-carried-over: #F57C00;
    --status-upcoming: #757575;
    --status-behind: #C62828;
    --status-on-track: #2E7D32;

    /* Layout */
    --max-width: 1200px;
    --spacing-xs: 4px;
    --spacing-sm: 8px;
    --spacing-md: 16px;
    --spacing-lg: 24px;
    --spacing-xl: 32px;

    /* Typography */
    --font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
    --font-size-body: 14px;
    --font-size-h1: 24px;
    --font-size-h2: 18px;
    --font-size-metric: 28px;

    /* Surfaces */
    --color-background: #FFFFFF;
    --color-surface: #F8F9FA;
    --color-border: #E0E0E0;
    --color-text-primary: #212121;
    --color-text-secondary: #616161;
}
```

Theming the dashboard to match a corporate color scheme requires editing only these custom properties. No other CSS changes needed.