# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-16 18:04 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, and displays work items in a color-coded heatmap grid (Shipped, In Progress, Carryover, Blockers) across monthly columns. Data is sourced from a local `data.json` file — no database required. **Primary recommendation:** Keep this dead simple. Use a single Blazor Server project with no external UI component libraries. The original HTML/CSS design is already clean and executive-friendly. Translate it directly into Blazor components using raw HTML/CSS (no component framework overhead). Use `System.Text.Json` to deserialize `data.json` at startup. Render the SVG timeline inline in a Blazor component. The entire solution should be **one .sln with one project**, runnable via `dotnet run` with zero configuration. This approach yields a solution that is trivial to maintain, easy to update (edit JSON, refresh browser), and produces pixel-perfect screenshots for PowerPoint decks. ---

### Key Findings

- The original `OriginalDesignConcept.html` is a self-contained 1920×1080 static layout using CSS Grid, Flexbox, and inline SVG — all of which translate directly to Blazor components with zero JavaScript interop needed.
- **No charting library is necessary.** The timeline is a simple inline SVG with lines, circles, diamonds, and text. This is far simpler and more screenshot-friendly than any charting library output.
- **No database is needed.** A `data.json` file is the ideal storage for this use case — it's human-editable, version-controllable, and requires no connection strings or migrations.
- Blazor Server's SignalR circuit is irrelevant for this use case (single local user), but it's the mandated stack and works fine. The page will load fast locally.
- The design targets a fixed 1920×1080 viewport for screenshot purposes. Responsive design is explicitly **not** a priority — the page should look perfect at exactly that resolution.
- No authentication, authorization, or security hardening is needed per requirements. This is a local tool for one person.
- The heatmap grid uses a 5-column CSS Grid (`160px repeat(4, 1fr)`) with 4 status rows, each color-coded. This maps cleanly to a Blazor `@foreach` loop over data model collections.
- **Existing competitors/tools:** PowerBI, Jira dashboards, Monday.com, Smartsheet. All are overkill for this use case. The value here is simplicity and full visual control for executive screenshots. --- **Goal:** Working dashboard that renders from `data.json` with the exact visual design.
- **Create solution structure** — `dotnet new blazorserver -n ReportingDashboard` + `.sln`
- **Define data model** — `DashboardData.cs` with all POCOs matching the JSON schema
- **Create `data.json`** — Fictional project data (e.g., "Project Phoenix — Cloud Migration Platform")
- **Build `DashboardDataService`** — Singleton service that reads and deserializes `data.json`
- **Port CSS** — Copy all styles from `OriginalDesignConcept.html` into `dashboard.css`
- **Build components:**
- `Header.razor` — Title, subtitle, legend icons
- `Timeline.razor` — SVG rendering with milestone data
- `HeatmapGrid.razor` — CSS Grid with color-coded rows and data items
- `Dashboard.razor` — Composes all components on the page
- **Remove template boilerplate** — Delete default nav, counter page, weather page, Bootstrap references
- **Test at 1920×1080** — Verify screenshot fidelity
- Add `FileSystemWatcher` to auto-reload `data.json` changes without restarting the app
- Add subtle hover tooltips on milestone markers showing full date/description
- Add print-friendly `@media print` CSS rules
- Add a `?theme=dark` query parameter for dark-mode screenshots
- Support multiple JSON files: `data.phoenix.json`, `data.aurora.json`
- Add route: `/project/{name}` → loads `data.{name}.json`
- Add a simple project selector dropdown (or keep it URL-driven) | Win | Effort | Impact | |-----|--------|--------| | Port HTML design to Blazor components | 1 hour | Core deliverable | | Create fictional `data.json` with realistic project data | 15 min | Demonstrates the dashboard immediately | | Fixed 1920×1080 viewport for screenshots | 5 min | Ensures PowerPoint-ready output | | Remove all Blazor template boilerplate | 10 min | Clean, focused codebase |
```json
{
  "project": {
    "title": "Project Phoenix — Cloud Migration Platform",
    "subtitle": "Enterprise Engineering · Platform Modernization Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "currentMonth": "Apr",
    "currentDate": "2026-04-16"
  },
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "timelineRange": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "svgWidth": 1560,
    "monthLabels": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"]
  },
  "milestones": [
    {
      "id": "M1",
      "label": "API Gateway & Auth",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-15", "label": "Jan 15", "type": "checkpoint" },
        { "date": "2026-03-20", "label": "Mar 20 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "May Prod", "type": "production" }
      ]
    },
    {
      "id": "M2",
      "label": "Data Pipeline & ETL",
      "color": "#00897B",
      "events": [
        { "date": "2026-01-05", "label": "Jan 5", "type": "checkpoint" },
        { "date": "2026-02-14", "label": "Feb 14", "type": "checkpoint" },
        { "date": "2026-03-25", "label": "Mar 25 PoC", "type": "poc" }
      ]
    },
    {
      "id": "M3",
      "label": "Dashboard & Reporting",
      "color": "#546E7A",
      "events": [
        { "date": "2026-02-01", "label": "Feb 1", "type": "checkpoint" },
        { "date": "2026-04-10", "label": "Apr 10 PoC", "type": "poc" },
        { "date": "2026-06-01", "label": "Jun Prod", "type": "production" }
      ]
    }
  ],
  "shipped": {
    "Jan": ["Service mesh rollout", "Legacy API deprecation"],
    "Feb": ["Auth token migration", "Rate limiter v2"],
    "Mar": ["Terraform modules", "CI/CD pipeline v3"],
    "Apr": ["Monitoring dashboards"]
  },
  "inProgress": {
    "Jan": [],
    "Feb": ["Data lake schema design"],
    "Mar": ["API Gateway beta"],
    "Apr": ["Load testing framework", "SDK documentation"]
  },
  "carryover": {
    "Jan": [],
    "Feb": [],
    "Mar": ["Compliance audit prep"],
    "Apr": ["Partner API integration", "SLA definition doc"]
  },
  "blockers": {
    "Jan": [],
    "Feb": [],
    "Mar": ["Vendor contract delay"],
    "Apr": ["Capacity approval pending", "Security review backlog"]
  }
}
```
- **CSS must be extracted verbatim** from `OriginalDesignConcept.html` — do not rewrite or "modernize" it. The design is already optimized for its purpose.
- **The SVG timeline must be dynamically generated** from milestone data, not hardcoded. Use C# helper methods in the `Timeline.razor` `@code` block for coordinate calculations.
- **The "NOW" line** in the timeline should be calculated from `project.currentDate` in `data.json`, not from `DateTime.Now`, so screenshots are reproducible.
- **The current month column** in the heatmap should have the highlighted/darker background (`.apr` class in original CSS), driven by `project.currentMonth`.
- **`dotnet watch`** is the recommended development workflow — it provides hot reload when Razor files change, making design iteration fast.
- **To take screenshots:** Open the page in Edge, press F12, toggle device toolbar (Ctrl+Shift+M), set to 1920×1080, then use Ctrl+Shift+P → "Capture full size screenshot." This produces a clean PNG without browser chrome.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (built-in) | .NET 8.0 | No additional packages needed | | **CSS Layout** | Raw CSS Grid + Flexbox | N/A | Matches the original design exactly | | **SVG Timeline** | Inline SVG in Razor markup | N/A | No charting library — hand-crafted SVG as in original | | **Font** | Segoe UI (system font) | N/A | Already available on Windows | | **Icons/Shapes** | SVG primitives (circles, diamonds, lines) | N/A | As defined in the HTML reference | **Do NOT use:** MudBlazor, Radzen, Syncfusion, Telerik, or any component library. They add complexity, theming conflicts, and make screenshot output unpredictable. The design is simple enough to implement with raw HTML/CSS. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Native, fast, zero dependencies | | **Data Storage** | `data.json` flat file | N/A | Loaded at startup via `IConfiguration` or direct file read | | **File Watching (optional)** | `FileSystemWatcher` | Built into .NET 8 | Auto-reload data when JSON changes without restart | **Do NOT use:** Entity Framework, SQLite, LiteDB, or any database. A JSON file is the correct choice for this scope. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **SDK** | .NET 8 SDK | 8.0.x (latest patch) | `dotnet --version` to verify | | **Project Type** | Blazor Server App | `blazorserver` template | Single `.csproj` in a single `.sln` | | **Build** | `dotnet build` / `dotnet run` | N/A | No webpack, no npm, no bundler | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Tests** | xUnit | 2.7.x | For data model deserialization tests if desired | | **Component Tests** | bUnit | 1.25.x+ | For Blazor component rendering tests if desired | Testing is optional given the project's simplicity, but if added, xUnit + bUnit is the standard Blazor testing stack.
```xml
<!-- Only required package (already included by template): -->
<PackageReference Include="Microsoft.AspNetCore.Components" Version="8.0.*" />

<!-- No additional packages needed. The default Blazor Server template is sufficient. -->
``` This is not a typo. **Zero additional NuGet packages are recommended.** The .NET 8 Blazor Server template includes everything needed. --- This is a one-page app. Do not over-architect it.
```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css        ← All styles from the HTML design
    │   └── data.json                 ← Project data (editable)
    ├── Models/
    │   └── DashboardData.cs          ← POCO classes matching data.json schema
    ├── Services/
    │   └── DashboardDataService.cs   ← Reads & deserializes data.json
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor       ← Main page (route: "/")
    │   ├── Layout/
    │   │   └── DashboardLayout.razor ← Minimal layout (no nav, no sidebar)
    │   ├── Header.razor              ← Title, subtitle, legend
    │   ├── Timeline.razor            ← SVG milestone timeline
    │   └── HeatmapGrid.razor         ← Status grid (Shipped/InProgress/Carryover/Blockers)
    └── Properties/
        └── launchSettings.json
```
```csharp
public record DashboardData
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<MonthColumn> Months { get; init; }
    public StatusSection Shipped { get; init; }
    public StatusSection InProgress { get; init; }
    public StatusSection Carryover { get; init; }
    public StatusSection Blockers { get; init; }
}

public record ProjectInfo
{
    public string Title { get; init; }         // "Privacy Automation Release Roadmap"
    public string Subtitle { get; init; }      // "Trusted Platform · Privacy Automation..."
    public string BacklogUrl { get; init; }    // Link to ADO backlog
    public string CurrentMonth { get; init; }  // "Apr 2026"
}

public record Milestone
{
    public string Id { get; init; }            // "M1", "M2", "M3"
    public string Label { get; init; }         // "Chatbot & MS Role"
    public string Color { get; init; }         // "#0078D4"
    public List<MilestoneEvent> Events { get; init; }
}

public record MilestoneEvent
{
    public string Date { get; init; }          // "2026-01-12"
    public string Label { get; init; }         // "Jan 12"
    public string Type { get; init; }          // "checkpoint" | "poc" | "production"
}

public record MonthColumn
{
    public string Name { get; init; }          // "Jan", "Feb", "Mar", "Apr"
    public bool IsCurrentMonth { get; init; }  // true for Apr
}

public record StatusSection
{
    public Dictionary<string, List<string>> ItemsByMonth { get; init; }
    // Key = month name, Value = list of work items
}
```
```
data.json (wwwroot/)
    ↓ FileRead at startup + cached
DashboardDataService (Singleton, registered in DI)
    ↓ Injected into components
Dashboard.razor → Header.razor + Timeline.razor + HeatmapGrid.razor
    ↓ Rendered as HTML/CSS/SVG
Browser (1920×1080 viewport) → Screenshot for PowerPoint
``` **Single CSS file** (`dashboard.css`) containing all styles from the original HTML design. Do not use CSS isolation (`.razor.css` files) for this project — it adds complexity and makes it harder to maintain a single cohesive design. Key CSS patterns to preserve from the original design:
- **Fixed viewport:** `body { width: 1920px; height: 1080px; overflow: hidden; }`
- **CSS Grid for heatmap:** `grid-template-columns: 160px repeat(4, 1fr);`
- **Flexbox for header and timeline area**
- **Color-coded rows:** Green (Shipped), Blue (In Progress), Amber (Carryover), Red (Blockers)
- **Bullet indicators:** `::before` pseudo-elements with colored circles The timeline SVG should be generated dynamically in `Timeline.razor` based on milestone data:
```razor
<svg width="1560" height="185" style="overflow:visible;display:block">
    @foreach (var milestone in Data.Milestones)
    {
        var y = GetMilestoneY(milestone);  // 42, 98, 154 for M1, M2, M3
        <line x1="0" y1="@y" x2="1560" y2="@y" 
              stroke="@milestone.Color" stroke-width="3"/>
        
        @foreach (var evt in milestone.Events)
        {
            var x = DateToX(evt.Date);  // Map date to x-coordinate
            @if (evt.Type == "poc")
            {
                // Diamond shape
                <polygon points="@DiamondPoints(x, y)" fill="#F4B400"/>
            }
            else if (evt.Type == "production")
            {
                <polygon points="@DiamondPoints(x, y)" fill="#34A853"/>
            }
            else
            {
                // Circle checkpoint
                <circle cx="@x" cy="@y" r="5" fill="white" 
                        stroke="#888" stroke-width="2.5"/>
            }
        }
    }
    @* "NOW" line *@
    <line x1="@NowX" y1="0" x2="@NowX" y2="185" 
          stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
</svg>
``` The entire dashboard can be rendered server-side with zero JavaScript. Blazor Server will handle the initial render, and since this is a static display page (no user interactions beyond loading), there is no need for:
- JS interop
- SignalR real-time updates
- Interactive event handlers
- Client-side state management Set `@rendermode InteractiveServer` on the page, or even use static SSR (`@rendermode` omitted) since there's no interactivity needed. ---

### Considerations & Risks

- **None.** Per requirements, this is a local-only tool with no auth. The Blazor Server template's default authentication scaffolding should be removed or never added.
```csharp
// Program.cs - keep it minimal
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
- `data.json` contains project status information, not PII or secrets. No encryption needed.
- If the JSON ever contains sensitive project names, the file is local-only and not exposed to any network. | Aspect | Recommendation | |--------|---------------| | **Runtime** | Local `dotnet run` or `dotnet watch` during development | | **Production (local)** | `dotnet publish -c Release` → run the executable directly | | **Port** | Default `https://localhost:5001` or `http://localhost:5000` | | **Browser** | Open Edge/Chrome at 1920×1080, take screenshot | **No containerization, no CDN, no reverse proxy, no IIS.** This runs on the developer's machine. **$0.** This is a local application with no cloud dependencies, no database server, no hosting fees. --- **Severity:** Low **Impact:** Slightly more complex than a plain HTML file, but manageable. **Mitigation:** The benefit is type-safe C# data binding and clean component architecture. The overhead is minimal for a single-page app. The alternative (static HTML) would require manually editing HTML every time data changes. **Trade-off accepted:** The small overhead of Blazor Server is worth the data-driven rendering from `data.json`. **Severity:** Medium **Impact:** The dashboard must look pixel-perfect at 1920×1080 for PowerPoint screenshots.
- Use fixed `width: 1920px; height: 1080px` on body (matching original design)
- Use `overflow: hidden` to prevent scrollbars
- Test in Edge/Chrome with DevTools device toolbar set to 1920×1080
- Consider using browser's built-in screenshot (F12 → Ctrl+Shift+P → "Capture full size screenshot") **Severity:** Medium **Impact:** Milestone markers must be positioned accurately on the timeline. **Mitigation:** Implement a simple linear interpolation function:
```csharp
private int DateToX(DateOnly date)
{
    var startDate = new DateOnly(2026, 1, 1);
    var endDate = new DateOnly(2026, 6, 30);
    var totalDays = endDate.DayNumber - startDate.DayNumber;
    var dayOffset = date.DayNumber - startDate.DayNumber;
    return (int)(dayOffset / (double)totalDays * 1560); // 1560px SVG width
}
``` **Severity:** Low **Impact:** If someone edits `data.json` incorrectly, the page may crash or render incorrectly. **Mitigation:** Add null checks and fallback rendering in components. Optionally add a JSON Schema file for editor validation. Blazor Server requires the `dotnet` process to be running. Blazor WASM would produce a static site. However, **Blazor Server is the mandated stack**, and for local use the always-running process is not a burden. Blazor Server also renders faster on first load (no WASM download). **Decision: Raw CSS.** The original design uses custom CSS Grid and Flexbox with specific pixel values and colors. Introducing a CSS framework would fight with the design's exact specifications and add unnecessary weight. The CSS is ~100 lines — entirely manageable. ---
- **Date range:** Should the timeline always show 6 months (Jan–Jun), or should the month range be configurable in `data.json`? The original design shows Jan–Jun 2026.
- **Number of milestones:** The design shows 3 milestone swim lanes (M1, M2, M3). Should this be fixed at 3, or should the layout accommodate a variable number?
- **Heatmap columns:** The design shows 4 month columns in the heatmap (Jan–Apr). Should this always show the last 4 months, or be configurable?
- **ADO Backlog link:** The header includes a link to "ADO Backlog." Should this URL be in `data.json`, or is it static?
- **Multiple projects:** Will there ever be multiple dashboards for different projects (each with their own `data.json`), or is this always a single project? If multiple, we could support a route like `/project/{name}` loading `{name}.json`.
- **Update workflow:** Who edits `data.json`? Is it manually edited in a text editor, or should there be a simple admin form? (Recommendation: keep it manual for v1.)
- **Color theme:** The original design uses Microsoft's color palette (#0078D4 blue, #34A853 green, etc.). Should the colors be hardcoded or configurable per project in `data.json`? ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, and displays work items in a color-coded heatmap grid (Shipped, In Progress, Carryover, Blockers) across monthly columns. Data is sourced from a local `data.json` file — no database required.

**Primary recommendation:** Keep this dead simple. Use a single Blazor Server project with no external UI component libraries. The original HTML/CSS design is already clean and executive-friendly. Translate it directly into Blazor components using raw HTML/CSS (no component framework overhead). Use `System.Text.Json` to deserialize `data.json` at startup. Render the SVG timeline inline in a Blazor component. The entire solution should be **one .sln with one project**, runnable via `dotnet run` with zero configuration.

This approach yields a solution that is trivial to maintain, easy to update (edit JSON, refresh browser), and produces pixel-perfect screenshots for PowerPoint decks.

---

## 2. Key Findings

- The original `OriginalDesignConcept.html` is a self-contained 1920×1080 static layout using CSS Grid, Flexbox, and inline SVG — all of which translate directly to Blazor components with zero JavaScript interop needed.
- **No charting library is necessary.** The timeline is a simple inline SVG with lines, circles, diamonds, and text. This is far simpler and more screenshot-friendly than any charting library output.
- **No database is needed.** A `data.json` file is the ideal storage for this use case — it's human-editable, version-controllable, and requires no connection strings or migrations.
- Blazor Server's SignalR circuit is irrelevant for this use case (single local user), but it's the mandated stack and works fine. The page will load fast locally.
- The design targets a fixed 1920×1080 viewport for screenshot purposes. Responsive design is explicitly **not** a priority — the page should look perfect at exactly that resolution.
- No authentication, authorization, or security hardening is needed per requirements. This is a local tool for one person.
- The heatmap grid uses a 5-column CSS Grid (`160px repeat(4, 1fr)`) with 4 status rows, each color-coded. This maps cleanly to a Blazor `@foreach` loop over data model collections.
- **Existing competitors/tools:** PowerBI, Jira dashboards, Monday.com, Smartsheet. All are overkill for this use case. The value here is simplicity and full visual control for executive screenshots.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components + CSS)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 | No additional packages needed |
| **CSS Layout** | Raw CSS Grid + Flexbox | N/A | Matches the original design exactly |
| **SVG Timeline** | Inline SVG in Razor markup | N/A | No charting library — hand-crafted SVG as in original |
| **Font** | Segoe UI (system font) | N/A | Already available on Windows |
| **Icons/Shapes** | SVG primitives (circles, diamonds, lines) | N/A | As defined in the HTML reference |

**Do NOT use:** MudBlazor, Radzen, Syncfusion, Telerik, or any component library. They add complexity, theming conflicts, and make screenshot output unpredictable. The design is simple enough to implement with raw HTML/CSS.

### Backend (Data Layer)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Native, fast, zero dependencies |
| **Data Storage** | `data.json` flat file | N/A | Loaded at startup via `IConfiguration` or direct file read |
| **File Watching (optional)** | `FileSystemWatcher` | Built into .NET 8 | Auto-reload data when JSON changes without restart |

**Do NOT use:** Entity Framework, SQLite, LiteDB, or any database. A JSON file is the correct choice for this scope.

### Project Structure

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **SDK** | .NET 8 SDK | 8.0.x (latest patch) | `dotnet --version` to verify |
| **Project Type** | Blazor Server App | `blazorserver` template | Single `.csproj` in a single `.sln` |
| **Build** | `dotnet build` / `dotnet run` | N/A | No webpack, no npm, no bundler |

### Testing (Minimal)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Tests** | xUnit | 2.7.x | For data model deserialization tests if desired |
| **Component Tests** | bUnit | 1.25.x+ | For Blazor component rendering tests if desired |

Testing is optional given the project's simplicity, but if added, xUnit + bUnit is the standard Blazor testing stack.

### Specific NuGet Packages

```xml
<!-- Only required package (already included by template): -->
<PackageReference Include="Microsoft.AspNetCore.Components" Version="8.0.*" />

<!-- No additional packages needed. The default Blazor Server template is sufficient. -->
```

This is not a typo. **Zero additional NuGet packages are recommended.** The .NET 8 Blazor Server template includes everything needed.

---

## 4. Architecture Recommendations

### Overall Pattern: Single-Component Page with JSON Data Service

This is a one-page app. Do not over-architect it.

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css        ← All styles from the HTML design
    │   └── data.json                 ← Project data (editable)
    ├── Models/
    │   └── DashboardData.cs          ← POCO classes matching data.json schema
    ├── Services/
    │   └── DashboardDataService.cs   ← Reads & deserializes data.json
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor       ← Main page (route: "/")
    │   ├── Layout/
    │   │   └── DashboardLayout.razor ← Minimal layout (no nav, no sidebar)
    │   ├── Header.razor              ← Title, subtitle, legend
    │   ├── Timeline.razor            ← SVG milestone timeline
    │   └── HeatmapGrid.razor         ← Status grid (Shipped/InProgress/Carryover/Blockers)
    └── Properties/
        └── launchSettings.json
```

### Data Model Design

```csharp
public record DashboardData
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<MonthColumn> Months { get; init; }
    public StatusSection Shipped { get; init; }
    public StatusSection InProgress { get; init; }
    public StatusSection Carryover { get; init; }
    public StatusSection Blockers { get; init; }
}

public record ProjectInfo
{
    public string Title { get; init; }         // "Privacy Automation Release Roadmap"
    public string Subtitle { get; init; }      // "Trusted Platform · Privacy Automation..."
    public string BacklogUrl { get; init; }    // Link to ADO backlog
    public string CurrentMonth { get; init; }  // "Apr 2026"
}

public record Milestone
{
    public string Id { get; init; }            // "M1", "M2", "M3"
    public string Label { get; init; }         // "Chatbot & MS Role"
    public string Color { get; init; }         // "#0078D4"
    public List<MilestoneEvent> Events { get; init; }
}

public record MilestoneEvent
{
    public string Date { get; init; }          // "2026-01-12"
    public string Label { get; init; }         // "Jan 12"
    public string Type { get; init; }          // "checkpoint" | "poc" | "production"
}

public record MonthColumn
{
    public string Name { get; init; }          // "Jan", "Feb", "Mar", "Apr"
    public bool IsCurrentMonth { get; init; }  // true for Apr
}

public record StatusSection
{
    public Dictionary<string, List<string>> ItemsByMonth { get; init; }
    // Key = month name, Value = list of work items
}
```

### Data Flow

```
data.json (wwwroot/)
    ↓ FileRead at startup + cached
DashboardDataService (Singleton, registered in DI)
    ↓ Injected into components
Dashboard.razor → Header.razor + Timeline.razor + HeatmapGrid.razor
    ↓ Rendered as HTML/CSS/SVG
Browser (1920×1080 viewport) → Screenshot for PowerPoint
```

### CSS Architecture

**Single CSS file** (`dashboard.css`) containing all styles from the original HTML design. Do not use CSS isolation (`.razor.css` files) for this project — it adds complexity and makes it harder to maintain a single cohesive design.

Key CSS patterns to preserve from the original design:
- **Fixed viewport:** `body { width: 1920px; height: 1080px; overflow: hidden; }`
- **CSS Grid for heatmap:** `grid-template-columns: 160px repeat(4, 1fr);`
- **Flexbox for header and timeline area**
- **Color-coded rows:** Green (Shipped), Blue (In Progress), Amber (Carryover), Red (Blockers)
- **Bullet indicators:** `::before` pseudo-elements with colored circles

### SVG Timeline Rendering

The timeline SVG should be generated dynamically in `Timeline.razor` based on milestone data:

```razor
<svg width="1560" height="185" style="overflow:visible;display:block">
    @foreach (var milestone in Data.Milestones)
    {
        var y = GetMilestoneY(milestone);  // 42, 98, 154 for M1, M2, M3
        <line x1="0" y1="@y" x2="1560" y2="@y" 
              stroke="@milestone.Color" stroke-width="3"/>
        
        @foreach (var evt in milestone.Events)
        {
            var x = DateToX(evt.Date);  // Map date to x-coordinate
            @if (evt.Type == "poc")
            {
                // Diamond shape
                <polygon points="@DiamondPoints(x, y)" fill="#F4B400"/>
            }
            else if (evt.Type == "production")
            {
                <polygon points="@DiamondPoints(x, y)" fill="#34A853"/>
            }
            else
            {
                // Circle checkpoint
                <circle cx="@x" cy="@y" r="5" fill="white" 
                        stroke="#888" stroke-width="2.5"/>
            }
        }
    }
    @* "NOW" line *@
    <line x1="@NowX" y1="0" x2="@NowX" y2="185" 
          stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
</svg>
```

### Key Design Decision: No JavaScript Interop

The entire dashboard can be rendered server-side with zero JavaScript. Blazor Server will handle the initial render, and since this is a static display page (no user interactions beyond loading), there is no need for:
- JS interop
- SignalR real-time updates
- Interactive event handlers
- Client-side state management

Set `@rendermode InteractiveServer` on the page, or even use static SSR (`@rendermode` omitted) since there's no interactivity needed.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** Per requirements, this is a local-only tool with no auth. The Blazor Server template's default authentication scaffolding should be removed or never added.

```csharp
// Program.cs - keep it minimal
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

### Data Protection

- `data.json` contains project status information, not PII or secrets. No encryption needed.
- If the JSON ever contains sensitive project names, the file is local-only and not exposed to any network.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Local `dotnet run` or `dotnet watch` during development |
| **Production (local)** | `dotnet publish -c Release` → run the executable directly |
| **Port** | Default `https://localhost:5001` or `http://localhost:5000` |
| **Browser** | Open Edge/Chrome at 1920×1080, take screenshot |

**No containerization, no CDN, no reverse proxy, no IIS.** This runs on the developer's machine.

### Infrastructure Costs

**$0.** This is a local application with no cloud dependencies, no database server, no hosting fees.

---

## 6. Risks & Trade-offs

### Risk: Blazor Server Is Overkill for a Static Page

**Severity:** Low
**Impact:** Slightly more complex than a plain HTML file, but manageable.
**Mitigation:** The benefit is type-safe C# data binding and clean component architecture. The overhead is minimal for a single-page app. The alternative (static HTML) would require manually editing HTML every time data changes.
**Trade-off accepted:** The small overhead of Blazor Server is worth the data-driven rendering from `data.json`.

### Risk: Screenshot Fidelity

**Severity:** Medium
**Impact:** The dashboard must look pixel-perfect at 1920×1080 for PowerPoint screenshots.
**Mitigation:**
- Use fixed `width: 1920px; height: 1080px` on body (matching original design)
- Use `overflow: hidden` to prevent scrollbars
- Test in Edge/Chrome with DevTools device toolbar set to 1920×1080
- Consider using browser's built-in screenshot (F12 → Ctrl+Shift+P → "Capture full size screenshot")

### Risk: SVG Timeline Date-to-Pixel Mapping

**Severity:** Medium
**Impact:** Milestone markers must be positioned accurately on the timeline.
**Mitigation:** Implement a simple linear interpolation function:
```csharp
private int DateToX(DateOnly date)
{
    var startDate = new DateOnly(2026, 1, 1);
    var endDate = new DateOnly(2026, 6, 30);
    var totalDays = endDate.DayNumber - startDate.DayNumber;
    var dayOffset = date.DayNumber - startDate.DayNumber;
    return (int)(dayOffset / (double)totalDays * 1560); // 1560px SVG width
}
```

### Risk: data.json Schema Drift

**Severity:** Low
**Impact:** If someone edits `data.json` incorrectly, the page may crash or render incorrectly.
**Mitigation:** Add null checks and fallback rendering in components. Optionally add a JSON Schema file for editor validation.

### Trade-off: Blazor Server vs. Blazor WebAssembly

Blazor Server requires the `dotnet` process to be running. Blazor WASM would produce a static site. However, **Blazor Server is the mandated stack**, and for local use the always-running process is not a burden. Blazor Server also renders faster on first load (no WASM download).

### Trade-off: Raw CSS vs. Tailwind/Bootstrap

**Decision: Raw CSS.** The original design uses custom CSS Grid and Flexbox with specific pixel values and colors. Introducing a CSS framework would fight with the design's exact specifications and add unnecessary weight. The CSS is ~100 lines — entirely manageable.

---

## 7. Open Questions

1. **Date range:** Should the timeline always show 6 months (Jan–Jun), or should the month range be configurable in `data.json`? The original design shows Jan–Jun 2026.

2. **Number of milestones:** The design shows 3 milestone swim lanes (M1, M2, M3). Should this be fixed at 3, or should the layout accommodate a variable number?

3. **Heatmap columns:** The design shows 4 month columns in the heatmap (Jan–Apr). Should this always show the last 4 months, or be configurable?

4. **ADO Backlog link:** The header includes a link to "ADO Backlog." Should this URL be in `data.json`, or is it static?

5. **Multiple projects:** Will there ever be multiple dashboards for different projects (each with their own `data.json`), or is this always a single project? If multiple, we could support a route like `/project/{name}` loading `{name}.json`.

6. **Update workflow:** Who edits `data.json`? Is it manually edited in a text editor, or should there be a simple admin form? (Recommendation: keep it manual for v1.)

7. **Color theme:** The original design uses Microsoft's color palette (#0078D4 blue, #34A853 green, etc.). Should the colors be hardcoded or configurable per project in `data.json`?

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 hours)

**Goal:** Working dashboard that renders from `data.json` with the exact visual design.

1. **Create solution structure** — `dotnet new blazorserver -n ReportingDashboard` + `.sln`
2. **Define data model** — `DashboardData.cs` with all POCOs matching the JSON schema
3. **Create `data.json`** — Fictional project data (e.g., "Project Phoenix — Cloud Migration Platform")
4. **Build `DashboardDataService`** — Singleton service that reads and deserializes `data.json`
5. **Port CSS** — Copy all styles from `OriginalDesignConcept.html` into `dashboard.css`
6. **Build components:**
   - `Header.razor` — Title, subtitle, legend icons
   - `Timeline.razor` — SVG rendering with milestone data
   - `HeatmapGrid.razor` — CSS Grid with color-coded rows and data items
   - `Dashboard.razor` — Composes all components on the page
7. **Remove template boilerplate** — Delete default nav, counter page, weather page, Bootstrap references
8. **Test at 1920×1080** — Verify screenshot fidelity

### Phase 2: Polish (Optional, 30 min)

- Add `FileSystemWatcher` to auto-reload `data.json` changes without restarting the app
- Add subtle hover tooltips on milestone markers showing full date/description
- Add print-friendly `@media print` CSS rules
- Add a `?theme=dark` query parameter for dark-mode screenshots

### Phase 3: Multi-Project Support (Optional, 30 min)

- Support multiple JSON files: `data.phoenix.json`, `data.aurora.json`
- Add route: `/project/{name}` → loads `data.{name}.json`
- Add a simple project selector dropdown (or keep it URL-driven)

### Quick Wins

| Win | Effort | Impact |
|-----|--------|--------|
| Port HTML design to Blazor components | 1 hour | Core deliverable |
| Create fictional `data.json` with realistic project data | 15 min | Demonstrates the dashboard immediately |
| Fixed 1920×1080 viewport for screenshots | 5 min | Ensures PowerPoint-ready output |
| Remove all Blazor template boilerplate | 10 min | Clean, focused codebase |

### Sample `data.json` Structure

```json
{
  "project": {
    "title": "Project Phoenix — Cloud Migration Platform",
    "subtitle": "Enterprise Engineering · Platform Modernization Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project/_backlogs",
    "currentMonth": "Apr",
    "currentDate": "2026-04-16"
  },
  "months": ["Jan", "Feb", "Mar", "Apr"],
  "timelineRange": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "svgWidth": 1560,
    "monthLabels": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"]
  },
  "milestones": [
    {
      "id": "M1",
      "label": "API Gateway & Auth",
      "color": "#0078D4",
      "events": [
        { "date": "2026-01-15", "label": "Jan 15", "type": "checkpoint" },
        { "date": "2026-03-20", "label": "Mar 20 PoC", "type": "poc" },
        { "date": "2026-05-01", "label": "May Prod", "type": "production" }
      ]
    },
    {
      "id": "M2",
      "label": "Data Pipeline & ETL",
      "color": "#00897B",
      "events": [
        { "date": "2026-01-05", "label": "Jan 5", "type": "checkpoint" },
        { "date": "2026-02-14", "label": "Feb 14", "type": "checkpoint" },
        { "date": "2026-03-25", "label": "Mar 25 PoC", "type": "poc" }
      ]
    },
    {
      "id": "M3",
      "label": "Dashboard & Reporting",
      "color": "#546E7A",
      "events": [
        { "date": "2026-02-01", "label": "Feb 1", "type": "checkpoint" },
        { "date": "2026-04-10", "label": "Apr 10 PoC", "type": "poc" },
        { "date": "2026-06-01", "label": "Jun Prod", "type": "production" }
      ]
    }
  ],
  "shipped": {
    "Jan": ["Service mesh rollout", "Legacy API deprecation"],
    "Feb": ["Auth token migration", "Rate limiter v2"],
    "Mar": ["Terraform modules", "CI/CD pipeline v3"],
    "Apr": ["Monitoring dashboards"]
  },
  "inProgress": {
    "Jan": [],
    "Feb": ["Data lake schema design"],
    "Mar": ["API Gateway beta"],
    "Apr": ["Load testing framework", "SDK documentation"]
  },
  "carryover": {
    "Jan": [],
    "Feb": [],
    "Mar": ["Compliance audit prep"],
    "Apr": ["Partner API integration", "SLA definition doc"]
  },
  "blockers": {
    "Jan": [],
    "Feb": [],
    "Mar": ["Vendor contract delay"],
    "Apr": ["Capacity approval pending", "Security review backlog"]
  }
}
```

### Key Implementation Notes

1. **CSS must be extracted verbatim** from `OriginalDesignConcept.html` — do not rewrite or "modernize" it. The design is already optimized for its purpose.

2. **The SVG timeline must be dynamically generated** from milestone data, not hardcoded. Use C# helper methods in the `Timeline.razor` `@code` block for coordinate calculations.

3. **The "NOW" line** in the timeline should be calculated from `project.currentDate` in `data.json`, not from `DateTime.Now`, so screenshots are reproducible.

4. **The current month column** in the heatmap should have the highlighted/darker background (`.apr` class in original CSS), driven by `project.currentMonth`.

5. **`dotnet watch`** is the recommended development workflow — it provides hot reload when Razor files change, making design iteration fast.

6. **To take screenshots:** Open the page in Edge, press F12, toggle device toolbar (Ctrl+Shift+M), set to 1920×1080, then use Ctrl+Shift+P → "Capture full size screenshot." This produces a clean PNG without browser chrome.

## Visual Design References

The following design reference files were found in the repository. These MUST be used as the canonical visual specification when building UI components.

### `OriginalDesignConcept.html`

**Type:** HTML Design Template

**Layout Structure:**
- **Header section** with title, subtitle, and legend
- **Timeline/Gantt section** with SVG milestone visualization
- **Heatmap grid** — status rows × month columns, color-coded by category
  - Shipped row (green tones)
  - In Progress row (blue tones)
  - Carryover row (yellow/amber tones)
  - Blockers row (red tones)

**Key CSS Patterns:**
- Uses CSS Grid layout
- Uses Flexbox layout
- Color palette: #FFFFFF, #111, #0078D4, #888, #FAFAFA, #F5F5F5, #999, #FFF0D0, #C07700, #333, #1B7A28, #E8F5E9, #F0FBF0, #D8F2DA, #34A853
- Font: Segoe UI
- Grid columns: `160px repeat(4,1fr)`
- Designed for 1920×1080 screenshot resolution

<details><summary>Full HTML Source</summary>

```html
<!DOCTYPE html><html lang="en"><head><meta charset="UTF-8">
<style>
*{margin:0;padding:0;box-sizing:border-box;}
body{width:1920px;height:1080px;overflow:hidden;background:#FFFFFF;
     font-family:'Segoe UI',Arial,sans-serif;color:#111;display:flex;flex-direction:column;}
a{color:#0078D4;text-decoration:none;}
.hdr{padding:12px 44px 10px;border-bottom:1px solid #E0E0E0;display:flex;
      align-items:center;justify-content:space-between;flex-shrink:0;}
.hdr h1{font-size:24px;font-weight:700;}
.sub{font-size:12px;color:#888;margin-top:2px;}
.tl-area{display:flex;align-items:stretch;padding:6px 44px 0;flex-shrink:0;height:196px;
          border-bottom:2px solid #E8E8E8;background:#FAFAFA;}
.tl-svg-box{flex:1;padding-left:12px;padding-top:6px;}
/* heatmap */
.hm-wrap{flex:1;min-height:0;display:flex;flex-direction:column;padding:10px 44px 10px;}
.hm-title{font-size:14px;font-weight:700;color:#888;letter-spacing:.5px;text-transform:uppercase;margin-bottom:8px;flex-shrink:0;}
.hm-grid{flex:1;min-height:0;display:grid;
          grid-template-columns:160px repeat(4,1fr);
          grid-template-rows:36px repeat(4,1fr);
          border:1px solid #E0E0E0;}
/* header cells */
.hm-corner{background:#F5F5F5;display:flex;align-items:center;justify-content:center;
            font-size:11px;font-weight:700;color:#999;text-transform:uppercase;
            border-right:1px solid #E0E0E0;border-bottom:2px solid #CCC;}
.hm-col-hdr{display:flex;align-items:center;justify-content:center;
             font-size:16px;font-weight:700;background:#F5F5F5;
             border-right:1px solid #E0E0E0;border-bottom:2px solid #CCC;}
.hm-col-hdr.apr-hdr{background:#FFF0D0;color:#C07700;}
/* row header */
.hm-row-hdr{display:flex;align-items:center;padding:0 12px;
             font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.7px;
             border-right:2px solid #CCC;border-bottom:1px solid #E0E0E0;}
/* data cells */
.hm-cell{padding:8px 12px;border-right:1px solid #E0E0E0;border-bottom:1px solid #E0E0E0;overflow:hidden;}
.hm-cell .it{font-size:12px;color:#333;padding:2px 0 2px 12px;position:relative;line-height:1.35;}
.hm-cell .it::before{content:'';position:absolute;left:0;top:7px;width:6px;height:6px;border-radius:50%;}
/* row colors */
.ship-hdr{color:#1B7A28;background:#E8F5E9;border-right:2px solid #CCC;}
.ship-cell{background:#F0FBF0;} .ship-cell.apr{background:#D8F2DA;}
.ship-cell .it::before{background:#34A853;}
.prog-hdr{color:#1565C0;background:#E3F2FD;border-right:2px solid #CCC;}
.prog-cell{background:#EEF4FE;} .prog-cell.apr{background:#DAE8FB;}
.prog-cell .it::before{background:#0078D4;}
.carry-hdr{color:#B45309;background:#FFF8E1;border-right:2px solid #CCC;}
.carry-cell{background:#FFFDE7;} .carry-cell.apr{background:#FFF0B0;}
.carry-cell .it::before{background:#F4B400;}
.block-hdr{color:#991B1B;background:#FEF2F2;border-right:2px solid #CCC;}
.block-cell{background:#FFF5F5;} .block-cell.apr{background:#FFE4E4;}
.block-cell .it::before{background:#EA4335;}
</style></head><body>
<div class="hdr">
  <div>
    <h1>Privacy Automation Release Roadmap <a href="#">⧉ ADO Backlog</a></h1>
    <div class="sub">Trusted Platform · Privacy Automation Workstream · April 2026</div>
  </div>
  
<div style="display:flex;gap:22px;align-items:center;">
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:12px;height:12px;background:#F4B400;transform:rotate(45deg);display:inline-block;flex-shrink:0;"></span>PoC Milestone
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:12px;height:12px;background:#34A853;transform:rotate(45deg);display:inline-block;flex-shrink:0;"></span>Production Release
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:8px;height:8px;border-radius:50%;background:#999;display:inline-block;flex-shrink:0;"></span>Checkpoint
  </span>
  <span style="display:flex;align-items:center;gap:6px;font-size:12px;">
    <span style="width:2px;height:14px;background:#EA4335;display:inline-block;flex-shrink:0;"></span>Now (Apr 2026)
  </span>
</div>
</div>
<div class="tl-area">
  
<div style="width:230px;flex-shrink:0;display:flex;flex-direction:column;
            justify-content:space-around;padding:16px 12px 16px 0;
            border-right:1px solid #E0E0E0;">
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#0078D4;">
    M1<br/><span style="font-weight:400;color:#444;">Chatbot &amp; MS Role</span></div>
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#00897B;">
    M2<br/><span style="font-weight:400;color:#444;">PDS &amp; Data Inventory</span></div>
  <div style="font-size:12px;font-weight:600;line-height:1.4;color:#546E7A;">
    M3<br/><span style="font-weight:400;color:#444;">Auto Review DFD</span></div>
</div>
  <div class="tl-svg-box"><svg xmlns="http://www.w3.org/2000/svg" width="1560" height="185" style="overflow:visible;display:block">
<defs><filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter></defs>
<line x1="0" y1="0" x2="0" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="5" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Jan</text>
<line x1="260" y1="0" x2="260" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="265" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Feb</text>
<line x1="520" y1="0" x2="520" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="525" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Mar</text>
<line x1="780" y1="0" x2="780" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="785" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Apr</text>
<line x1="1040" y1="0" x2="1040" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="1045" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">May</text>
<line x1="1300" y1="0" x2="1300" y2="185" stroke="#bbb" stroke-opacity="0.4" stroke-width="1"/>
<text x="1305" y="14" fill="#666" font-size="11" font-weight="600" font-family="Segoe UI,Arial">Jun</text>
<line x1="823" y1="0" x2="823" y2="185" stroke="#EA4335" stroke-width="2" stroke-dasharray="5,3"/>
<text x="827" y="14" fill="#EA4335" font-size="10" font-weight="700" font-family="Segoe UI,Arial">NOW</text>
<line x1="0" y1="42" x2="1560" y2="42" stroke="#0078D4" stroke-width="3"/>
<circle cx="104" cy="42" r="7" fill="white" stroke="#0078D4" stroke-width="2.5"/>
<text x="104" y="26" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Jan 12</text>
<polygon points="745,31 756,42 745,53 734,42" fill="#F4B400" filter="url(#sh)"/><text x="745" y="66" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Mar 26 PoC</text>
<polygon points="1040,31 1051,42 1040,53 1029,42" fill="#34A853" filter="url(#sh)"/><text x="1040" y="18" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Apr Prod (TBD)</text>
<line x1="0" y1="98" x2="1560" y2="98" stroke="#00897B" stroke-width="3"/>
<circle cx="0" cy="98" r="7" fill="white" stroke="#00897B" stroke-width="2.5"/>
<text x="10" y="82" fill="#666" font-size="10" font-family="Segoe UI,Arial">Dec 19</text>
<circle cx="355" cy="98" r="5" fill="white" stroke="#888" stroke-width="2.5"/>
<text x="355" y="82" text-anchor="middle" fill="#666" font-size="10" font-family="Segoe UI,Arial">Feb 11</text>
<circle cx="546" cy="98" r="4" fill="#999"/>
<circle cx="607" cy="98" r="4" fill="#999"/>
<circle cx="650" cy="98" r="4" fill="#999"/>
<circle cx="667" cy="98" r="4" fill="#999"/>
<polygon points="693,87 704,98 693,109 682,98" fill="#F4B400" filter="url(#sh)"/><text x="693" y="74" text-anchor="middle" fill="#666" font-size="10" font-family="Seg
<!-- truncated -->
```
</details>


## Design Visual Previews

The following screenshots were rendered from the HTML design reference files. Engineers MUST match these visuals exactly.

### OriginalDesignConcept.html

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/597c0ff3fee08a4c7cdf4b69f3baadf09a3dbd28/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
