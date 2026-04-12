# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-12 01:19 UTC_

### Summary

This project requires building a lightweight, single-page dashboard for executive visibility into project progress and milestones. Given the mandatory C# .NET 8 with Blazor Server stack and local-only deployment, the recommended approach combines Blazor Server for rendering, Entity Framework Core or direct JSON deserialization for data access, and CSS Grid/Flexbox for clean, printable UI suitable for PowerPoint screenshots. The solution prioritizes simplicity, maintainability, and visual clarity over enterprise complexity. Implementation is straightforward with minimal dependencies, achievable within 1-2 sprints.

### Key Findings

- Blazor Server is optimal for this use case—no SPA complexity, built-in component reusability, and server-side rendering naturally support clean HTML output for screenshots.
- Dashboard data can be loaded directly from JSON files via `System.Text.Json` without ORM overhead; no database layer needed for read-only reporting.
- CSS Grid excels at creating timeline layouts and status cards; minimal CSS framework required (vanilla CSS recommended to keep design simple and controllable).
- Printable, mobile-responsive design using CSS `@media print` rules ensures screenshots capture professional layouts suitable for executive presentations.
- No authentication required per requirements; component-level data binding simplifies state management.
- Local SQLite database (included in project) can optionally store parsed JSON for incremental updates, or data can remain purely file-based.
- Chart visualization library: `Chart.js` via interop is lightweight and popular; alternatively, Blazor components like `ApexCharts.Blazor` or `Syncfusion Blazor Charts` for richer interactions.
- Testing: xUnit with Moq for unit tests, Selenium or PlaywrightSharp for integration tests of rendered HTML.
- **Data Model** - Define `ProjectData`, `Milestone`, `WorkItem` classes.
- **DataService** - Load & deserialize `data.json` using `System.Text.Json`.
- **Dashboard.razor** - Render project header (name, status, dates).
- **Mock data.json** - Create example file with 5 milestones, 20 work items across three categories.
- **Basic CSS** - Grid layout, card styles, typography for readability.
- **Test** - Write xUnit tests for `DataService` deserialization; manual browser test of rendering. **Deliverable**: Single-page dashboard showing project info and work items by category. Rough layout.
- **Timeline Component** - Render milestones horizontally with visual indicators (complete/incomplete).
- **StatusCards** - Individual cards for each work item with story points, assignee.
- **MetricsPanel** - KPI summary: % complete, velocity, etc.
- **CSS Refinement** - Polish layout, spacing, typography; ensure print-friendly.
- **Error Handling** - Graceful fallback if `data.json` missing or malformed. **Deliverable**: Fully styled, print-ready dashboard. Screenshot-ready for PowerPoint.
- **Chart Integration** - Add Chart.js for burndown or velocity chart (if required).
- **Filtering** - Button toggles to show/hide categories (e.g., "Hide Carried Over").
- **Refresh Button** - Manual reload of `data.json` without page refresh.
- **Dark Mode** - CSS custom properties for theming (if executives request).
- **Responsive Tweaks** - Mobile layout fallback (less priority since mainly desktop/print).
- **Week 1**: Basic HTML dashboard with mock data renders correctly; can take first screenshot.
- **Week 2**: Timeline and metrics visible; executives see project status at a glance.
- **Week 3**: Polished CSS; ready for PowerPoint integration.
- **Print Layout** - Create HTML mockup; test `@media print` in Chrome, Firefox, Edge to confirm output matches expectations.
- **Chart Library Choice** - If visualizations required, build small proof-of-concept: Chart.js vs. SkiaSharp vs. SVG-based component. Compare render time and output quality.
- **Data Integration** - If `data.json` sourced from external system, prototype JSON parsing with real API response format.
- `dotnet new blazorserver -n AgentSquad.Dashboard` (or extend existing project).
- Create `Models/` folder and classes.
- Create `Services/DataService.cs`, load from `wwwroot/data/data.json`.
- Build `Components/Dashboard.razor` as root component.
- Add child components as outlined.
- Test in `dotnet watch`.
- Run `dotnet publish -c Release` for production build.
- Verify output in browser; take screenshots. ---

### Recommended Tools & Technologies

- **Blazor Server** 8.0.x - Core rendering engine; built-in component composition, two-way data binding, no JavaScript required for basic functionality.
- **Razor Components** - Standard .NET approach for reusable UI components (Timeline, StatusCard, MilestoneIndicator).
- **CSS** (vanilla) - No framework overhead; CSS Grid and Flexbox provide layout control. Recommend BEM naming convention for clarity.
- **Chart.js 4.x** (via JavaScript interop) - Lightweight charting for burn-down, velocity, or roadmap visualizations. Alternative: `SkiaSharp` for server-side chart rendering if avoiding JavaScript.
- **ASP.NET Core 8.0** - Built into Blazor Server; handles HTTP requests, static files, and component rendering.
- **System.Text.Json** (.NET 8 built-in) - Deserialize `data.json` without external dependencies. Native to .NET 8 and performant.
- **Entity Framework Core 8.0.x** (optional) - Only if incremental data persistence needed; otherwise, skip for simplicity.
- **Newtonsoft.Json 13.x** (optional alternative) - If complex JSON parsing required, though `System.Text.Json` is preferred in .NET 8.
- **JSON files** (data.json) - Primary data source per requirements; stored in project root or `wwwroot/data/` directory. No schema versioning complexity needed.
- **SQLite (optional)** - Local, lightweight database if periodic data snapshots required. Already present in project (`agentsquad_azurenerd_ReportingDashboard.db`); can be reused or separate instance created.
- **No cloud services** - All data remains local; no connectivity dependencies.
- **xUnit 2.6.x** - Standard .NET testing framework; integrates with Visual Studio, supports async tests.
- **Moq 4.20.x** - Mocking library for isolating data loading and calculation logic.
- **Selenium 4.x** or **PlaywrightSharp 1.40.x** - Browser automation for visual regression testing of dashboard screenshots.
- **NUnit** (alternative) - If team preference; equally supported.
- **Visual Studio 2022** (Community, Professional, or Enterprise) - Full Blazor Server tooling support.
- **Visual Studio Code** - Alternative with C# Dev Kit extension; supported but less integrated.
- **.NET CLI 8.0.x** - Command-line build and run (`dotnet run`, `dotnet build`, `dotnet watch`).
- **Git** - Version control (already in use).
- **Humanizer 2.14.x** - Format dates, timespan, and counts for readability ("2 days ago", "5 completed items").
- **Serilog 3.1.x** (optional) - Structured logging for debugging; minimal overhead.
```
AgentSquad.Runner/
├── Components/
│   ├── Dashboard.razor          // Main page
│   ├── Timeline.razor           // Milestone timeline
│   ├── StatusCard.razor         // Milestone/task status card
│   ├── ProgressIndicator.razor  // Circular/linear progress
│   └── MetricsPanel.razor       // KPI summary
├── Models/
│   ├── ProjectData.cs           // Root data model
│   ├── Milestone.cs
│   ├── WorkItem.cs              // Shipped, In Progress, Carried Over
│   └── Metric.cs
├── Services/
│   ├── DataService.cs           // Load & parse data.json
│   └── CalculationService.cs    // Aggregations (% complete, burn-down)
├── wwwroot/
│   ├── css/
│   │   └── dashboard.css        // Grid, typography, print styles
│   ├── data/
│   │   └── data.json            // Configuration & mock data
│   └── img/
│       └── (logos, icons)
├── Program.cs                    // Blazor Server startup
└── appsettings.json             // Non-sensitive config
``` **ProjectData.cs** (root):
```csharp
public class ProjectData
{
    public ProjectMetadata Project { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<WorkItem> WorkItems { get; set; }
    public ProjectMetrics Metrics { get; set; }
}

public class ProjectMetadata
{
    public string Name { get; set; }
    public string Status { get; set; }       // "On Track", "At Risk", "Off Track"
    public string Owner { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime TargetEndDate { get; set; }
}

public class Milestone
{
    public string Id { get; set; }
    public string Title { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public int Order { get; set; }
}

public class WorkItem
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }    // "Shipped", "InProgress", "CarriedOver"
    public string Status { get; set; }      // "Done", "In Progress", etc.
    public int StoryPoints { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class ProjectMetrics
{
    public int TotalStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }
    public int InProgressStoryPoints { get; set; }
    public int CarriedOverCount { get; set; }
    public double VelocityPerSprint { get; set; }
}
```
- **Page Load** → `Dashboard.razor` OnInitializedAsync()
- **DataService.LoadProjectDataAsync()** → Reads `data.json` from `wwwroot/data/`
- **System.Text.Json.JsonSerializer.DeserializeAsync<ProjectData>(stream)**
- **CalculationService** computes aggregates (% complete, status, velocity)
- **Child Components** render via property binding: `Timeline`, `StatusCard`, `MetricsPanel`
- **CSS Grid** positions components for responsive, print-friendly layout
```json
{
  "project": {
    "name": "Project Alpha",
    "status": "On Track",
    "owner": "Jane Doe",
    "startDate": "2026-01-01",
    "targetEndDate": "2026-06-30"
  },
  "milestones": [
    {
      "id": "m1",
      "title": "Requirements & Design",
      "dueDate": "2026-02-15",
      "isCompleted": true,
      "order": 1
    }
  ],
  "workItems": [
    {
      "id": "wi1",
      "title": "User authentication module",
      "category": "Shipped",
      "status": "Done",
      "storyPoints": 8,
      "completedDate": "2026-02-10"
    }
  ],
  "metrics": {
    "totalStoryPoints": 100,
    "completedStoryPoints": 45,
    "inProgressStoryPoints": 25,
    "carriedOverCount": 5,
    "velocityPerSprint": 20
  }
}
```
```
Dashboard
├─ Header (Project name, status, dates)
├─ Timeline (Milestones in horizontal row)
├─ Metrics Panel (KPI cards: % complete, shipped count, etc.)
├─ Status Board (Three columns: Shipped, In Progress, Carried Over)
└─ Footer (Last updated date)
``` **CSS Grid Example**:
```css
.dashboard {
    display: grid;
    grid-template-columns: 1fr;
    gap: 2rem;
    padding: 2rem;
    max-width: 1400px;
    margin: 0 auto;
}

.timeline {
    display: grid;
    grid-auto-flow: column;
    gap: 1rem;
    overflow-x: auto;
}

.status-board {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 2rem;
}

@media print {
    body { margin: 0; padding: 0.5in; }
    .dashboard { max-width: 100%; }
    .timeline { overflow: visible; }
}
```
- Data loaded once on page init; no real-time updates required.
- Blazor's built-in `@bind` suffices for any interactive filtering (e.g., show/hide by milestone).
- No Redux, Flux, or third-party state lib needed.

### Considerations & Risks

- **None required** per requirements. Dashboard is read-only, internal-facing, no multi-user concerns.
- If future access control needed (e.g., role-based view filtering), add simple claim-based authorization in middleware without external identity provider.
- **JSON files** stored in `wwwroot/data/` (part of deployed application folder).
- **No encryption** at rest; data is non-sensitive mock/example project metrics.
- **HTTPS not required** for local deployment; HTTP sufficient.
- If real sensitive data used, encrypt `data.json` on disk and decrypt in `DataService`; use `System.Security.Cryptography` (built-in, no external dependency).
- **Local Windows/Linux/macOS** via Blazor Server host process.
- **IIS (optional)** - Can deploy as ASP.NET Core application to local or internal IIS if required.
- **Docker (optional)** - Containerize for consistent local environments; simple Dockerfile:
  ```dockerfile
  FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
  WORKDIR /src
  COPY . .
  RUN dotnet publish -c Release -o /app
  
  FROM mcr.microsoft.com/dotnet/aspnet:8.0
  WORKDIR /app
  COPY --from=build /app .
  ENTRYPOINT ["dotnet", "AgentSquad.Runner.dll"]
  ```
- **No cloud costs** - Everything runs locally.
- **Serilog** (optional) - Log to file or console; no centralized logging needed.
- **Performance** - No instrumentation required; Blazor Server handles rendering efficiently.
- **Backup** - Standard file backup of `data.json` and database (if used); no special strategy needed.
- **Blazor Server Limitations** - If dashboard needs offline capability, Blazor Server won't work (no server connection = no rendering). **Mitigation**: Confirm always-online assumption; if not, pivot to Blazor WASM with pre-cached data.
- **JSON Schema Evolution** - As project metrics change, `data.json` structure may shift. **Mitigation**: Use nullable properties in C# model; implement backward-compatible deserialization.
- **Large Dataset Performance** - If 1000+ work items displayed, rendering may slow. **Mitigation**: Paginate or virtualize lists; use Blazor's `Virtualize` component. For MVP, assume <500 items.
- **Print Layout Fragility** - CSS `@media print` can break across browsers. **Mitigation**: Test print output in target browsers; use tool like `PrintJS` or headless browser screenshot if needed.
- **Single JSON file** - No database means querying/filtering done in-memory. For <10K work items, negligible; beyond that, consider SQLite.
- **Blazor Server connection** - Multiple dashboard instances require persistent WebSocket connections. For 10+ concurrent users, monitor server resources; add caching layer if needed.
- **data.json availability** - If file missing/corrupted, dashboard fails to load. **Mitigation**: Validate JSON on startup; provide fallback "no data" UI.
- **Component rendering** - If a child component throws, entire dashboard may crash. **Mitigation**: Wrap components in error boundary (`try/catch` in Razor, or custom `ErrorBoundary` component). | Choice | Benefit | Cost | |--------|---------|------| | JSON data source | Simple, no DB setup | Manual data management, no queries | | Vanilla CSS | Full control, minimal overhead | More CSS to write; no dark mode out-of-box | | Blazor Server | Integrated, no JavaScript | Requires .NET runtime; connection dependency | | No auth | Fast to build | No audit trail or multi-user segregation |
- **Real or Mock Data?** - Is `data.json` generated from another system (e.g., JIRA API, Azure DevOps) or hand-maintained? If generated, what's the integration plan?
- **Update Frequency** - How often does data refresh? Real-time, daily, manual? Affects caching and refresh button design.
- **Historical Trend** - Should dashboard show velocity trend (burndown), or just current state? Required? If yes, may need date-stamped snapshots in data.
- **Chart Interactivity** - Are charts/timelines interactive (hover tooltips, click to filter), or purely visual/static? Affects Chart.js vs. simple SVG decision.
- **Multiple Projects** - Is this dashboard for one project, or a selector for many? Affects data model and navigation.
- **Customization Level** - Will executives customize colors, logos, layout per project? If yes, add configuration section to `data.json`.
- **Offline Capability** - Must work without internet/server? (Unlikely for local app, but confirm.)
- **Browser Requirements** - Any old IE/legacy browser support needed? (Unlikely for internal tool; assume modern browsers.)

### Detailed Analysis

I'm encountering permission restrictions. However, based on the user's detailed context provided, I can produce the research document. The user has explicitly described:

- A reporting dashboard for executives
- Shows project milestones and progress
- Displays what was shipped, in progress, carried over
- Includes a timeline of milestones
- Reads from a JSON config file (data.json)
- Should be simple for PowerPoint screenshots
- Mandatory stack: C# .NET 8 with Blazor Server, local only, .sln structure

Here is the comprehensive research document:

---

# Technology Stack Research: Executive Project Reporting Dashboard

## Executive Summary

This project requires building a lightweight, single-page dashboard for executive visibility into project progress and milestones. Given the mandatory C# .NET 8 with Blazor Server stack and local-only deployment, the recommended approach combines Blazor Server for rendering, Entity Framework Core or direct JSON deserialization for data access, and CSS Grid/Flexbox for clean, printable UI suitable for PowerPoint screenshots. The solution prioritizes simplicity, maintainability, and visual clarity over enterprise complexity. Implementation is straightforward with minimal dependencies, achievable within 1-2 sprints.

## Key Findings

- Blazor Server is optimal for this use case—no SPA complexity, built-in component reusability, and server-side rendering naturally support clean HTML output for screenshots.
- Dashboard data can be loaded directly from JSON files via `System.Text.Json` without ORM overhead; no database layer needed for read-only reporting.
- CSS Grid excels at creating timeline layouts and status cards; minimal CSS framework required (vanilla CSS recommended to keep design simple and controllable).
- Printable, mobile-responsive design using CSS `@media print` rules ensures screenshots capture professional layouts suitable for executive presentations.
- No authentication required per requirements; component-level data binding simplifies state management.
- Local SQLite database (included in project) can optionally store parsed JSON for incremental updates, or data can remain purely file-based.
- Chart visualization library: `Chart.js` via interop is lightweight and popular; alternatively, Blazor components like `ApexCharts.Blazor` or `Syncfusion Blazor Charts` for richer interactions.
- Testing: xUnit with Moq for unit tests, Selenium or PlaywrightSharp for integration tests of rendered HTML.

## Recommended Technology Stack

### Frontend (Blazor Server)
- **Blazor Server** 8.0.x - Core rendering engine; built-in component composition, two-way data binding, no JavaScript required for basic functionality.
- **Razor Components** - Standard .NET approach for reusable UI components (Timeline, StatusCard, MilestoneIndicator).
- **CSS** (vanilla) - No framework overhead; CSS Grid and Flexbox provide layout control. Recommend BEM naming convention for clarity.
- **Chart.js 4.x** (via JavaScript interop) - Lightweight charting for burn-down, velocity, or roadmap visualizations. Alternative: `SkiaSharp` for server-side chart rendering if avoiding JavaScript.

### Backend (C# .NET 8)
- **ASP.NET Core 8.0** - Built into Blazor Server; handles HTTP requests, static files, and component rendering.
- **System.Text.Json** (.NET 8 built-in) - Deserialize `data.json` without external dependencies. Native to .NET 8 and performant.
- **Entity Framework Core 8.0.x** (optional) - Only if incremental data persistence needed; otherwise, skip for simplicity.
- **Newtonsoft.Json 13.x** (optional alternative) - If complex JSON parsing required, though `System.Text.Json` is preferred in .NET 8.

### Data Storage
- **JSON files** (data.json) - Primary data source per requirements; stored in project root or `wwwroot/data/` directory. No schema versioning complexity needed.
- **SQLite (optional)** - Local, lightweight database if periodic data snapshots required. Already present in project (`agentsquad_azurenerd_ReportingDashboard.db`); can be reused or separate instance created.
- **No cloud services** - All data remains local; no connectivity dependencies.

### Testing & Validation
- **xUnit 2.6.x** - Standard .NET testing framework; integrates with Visual Studio, supports async tests.
- **Moq 4.20.x** - Mocking library for isolating data loading and calculation logic.
- **Selenium 4.x** or **PlaywrightSharp 1.40.x** - Browser automation for visual regression testing of dashboard screenshots.
- **NUnit** (alternative) - If team preference; equally supported.

### Development & Build
- **Visual Studio 2022** (Community, Professional, or Enterprise) - Full Blazor Server tooling support.
- **Visual Studio Code** - Alternative with C# Dev Kit extension; supported but less integrated.
- **.NET CLI 8.0.x** - Command-line build and run (`dotnet run`, `dotnet build`, `dotnet watch`).
- **Git** - Version control (already in use).

### Utilities & Helpers
- **Humanizer 2.14.x** - Format dates, timespan, and counts for readability ("2 days ago", "5 completed items").
- **Serilog 3.1.x** (optional) - Structured logging for debugging; minimal overhead.

## Architecture Recommendations

### 1. Project Structure
```
AgentSquad.Runner/
├── Components/
│   ├── Dashboard.razor          // Main page
│   ├── Timeline.razor           // Milestone timeline
│   ├── StatusCard.razor         // Milestone/task status card
│   ├── ProgressIndicator.razor  // Circular/linear progress
│   └── MetricsPanel.razor       // KPI summary
├── Models/
│   ├── ProjectData.cs           // Root data model
│   ├── Milestone.cs
│   ├── WorkItem.cs              // Shipped, In Progress, Carried Over
│   └── Metric.cs
├── Services/
│   ├── DataService.cs           // Load & parse data.json
│   └── CalculationService.cs    // Aggregations (% complete, burn-down)
├── wwwroot/
│   ├── css/
│   │   └── dashboard.css        // Grid, typography, print styles
│   ├── data/
│   │   └── data.json            // Configuration & mock data
│   └── img/
│       └── (logos, icons)
├── Program.cs                    // Blazor Server startup
└── appsettings.json             // Non-sensitive config
```

### 2. Data Model (C# Classes)
**ProjectData.cs** (root):
```csharp
public class ProjectData
{
    public ProjectMetadata Project { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<WorkItem> WorkItems { get; set; }
    public ProjectMetrics Metrics { get; set; }
}

public class ProjectMetadata
{
    public string Name { get; set; }
    public string Status { get; set; }       // "On Track", "At Risk", "Off Track"
    public string Owner { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime TargetEndDate { get; set; }
}

public class Milestone
{
    public string Id { get; set; }
    public string Title { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public int Order { get; set; }
}

public class WorkItem
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }    // "Shipped", "InProgress", "CarriedOver"
    public string Status { get; set; }      // "Done", "In Progress", etc.
    public int StoryPoints { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class ProjectMetrics
{
    public int TotalStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }
    public int InProgressStoryPoints { get; set; }
    public int CarriedOverCount { get; set; }
    public double VelocityPerSprint { get; set; }
}
```

### 3. Data Flow
1. **Page Load** → `Dashboard.razor` OnInitializedAsync()
2. **DataService.LoadProjectDataAsync()** → Reads `data.json` from `wwwroot/data/`
3. **System.Text.Json.JsonSerializer.DeserializeAsync<ProjectData>(stream)**
4. **CalculationService** computes aggregates (% complete, status, velocity)
5. **Child Components** render via property binding: `Timeline`, `StatusCard`, `MetricsPanel`
6. **CSS Grid** positions components for responsive, print-friendly layout

### 4. Data Source Format (data.json)
```json
{
  "project": {
    "name": "Project Alpha",
    "status": "On Track",
    "owner": "Jane Doe",
    "startDate": "2026-01-01",
    "targetEndDate": "2026-06-30"
  },
  "milestones": [
    {
      "id": "m1",
      "title": "Requirements & Design",
      "dueDate": "2026-02-15",
      "isCompleted": true,
      "order": 1
    }
  ],
  "workItems": [
    {
      "id": "wi1",
      "title": "User authentication module",
      "category": "Shipped",
      "status": "Done",
      "storyPoints": 8,
      "completedDate": "2026-02-10"
    }
  ],
  "metrics": {
    "totalStoryPoints": 100,
    "completedStoryPoints": 45,
    "inProgressStoryPoints": 25,
    "carriedOverCount": 5,
    "velocityPerSprint": 20
  }
}
```

### 5. Component Layout (CSS Grid)
```
Dashboard
├─ Header (Project name, status, dates)
├─ Timeline (Milestones in horizontal row)
├─ Metrics Panel (KPI cards: % complete, shipped count, etc.)
├─ Status Board (Three columns: Shipped, In Progress, Carried Over)
└─ Footer (Last updated date)
```

**CSS Grid Example**:
```css
.dashboard {
    display: grid;
    grid-template-columns: 1fr;
    gap: 2rem;
    padding: 2rem;
    max-width: 1400px;
    margin: 0 auto;
}

.timeline {
    display: grid;
    grid-auto-flow: column;
    gap: 1rem;
    overflow-x: auto;
}

.status-board {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 2rem;
}

@media print {
    body { margin: 0; padding: 0.5in; }
    .dashboard { max-width: 100%; }
    .timeline { overflow: visible; }
}
```

### 6. No State Management Complexity
- Data loaded once on page init; no real-time updates required.
- Blazor's built-in `@bind` suffices for any interactive filtering (e.g., show/hide by milestone).
- No Redux, Flux, or third-party state lib needed.

## Security & Infrastructure

### Authentication & Authorization
- **None required** per requirements. Dashboard is read-only, internal-facing, no multi-user concerns.
- If future access control needed (e.g., role-based view filtering), add simple claim-based authorization in middleware without external identity provider.

### Data Protection
- **JSON files** stored in `wwwroot/data/` (part of deployed application folder).
- **No encryption** at rest; data is non-sensitive mock/example project metrics.
- **HTTPS not required** for local deployment; HTTP sufficient.
- If real sensitive data used, encrypt `data.json` on disk and decrypt in `DataService`; use `System.Security.Cryptography` (built-in, no external dependency).

### Hosting & Deployment
- **Local Windows/Linux/macOS** via Blazor Server host process.
- **IIS (optional)** - Can deploy as ASP.NET Core application to local or internal IIS if required.
- **Docker (optional)** - Containerize for consistent local environments; simple Dockerfile:
  ```dockerfile
  FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
  WORKDIR /src
  COPY . .
  RUN dotnet publish -c Release -o /app
  
  FROM mcr.microsoft.com/dotnet/aspnet:8.0
  WORKDIR /app
  COPY --from=build /app .
  ENTRYPOINT ["dotnet", "AgentSquad.Runner.dll"]
  ```
- **No cloud costs** - Everything runs locally.

### Infrastructure & Observability
- **Serilog** (optional) - Log to file or console; no centralized logging needed.
- **Performance** - No instrumentation required; Blazor Server handles rendering efficiently.
- **Backup** - Standard file backup of `data.json` and database (if used); no special strategy needed.

## Risks & Trade-offs

### Technical Risks
1. **Blazor Server Limitations** - If dashboard needs offline capability, Blazor Server won't work (no server connection = no rendering). **Mitigation**: Confirm always-online assumption; if not, pivot to Blazor WASM with pre-cached data.
2. **JSON Schema Evolution** - As project metrics change, `data.json` structure may shift. **Mitigation**: Use nullable properties in C# model; implement backward-compatible deserialization.
3. **Large Dataset Performance** - If 1000+ work items displayed, rendering may slow. **Mitigation**: Paginate or virtualize lists; use Blazor's `Virtualize` component. For MVP, assume <500 items.
4. **Print Layout Fragility** - CSS `@media print` can break across browsers. **Mitigation**: Test print output in target browsers; use tool like `PrintJS` or headless browser screenshot if needed.

### Scalability Bottlenecks
- **Single JSON file** - No database means querying/filtering done in-memory. For <10K work items, negligible; beyond that, consider SQLite.
- **Blazor Server connection** - Multiple dashboard instances require persistent WebSocket connections. For 10+ concurrent users, monitor server resources; add caching layer if needed.

### Single Points of Failure
- **data.json availability** - If file missing/corrupted, dashboard fails to load. **Mitigation**: Validate JSON on startup; provide fallback "no data" UI.
- **Component rendering** - If a child component throws, entire dashboard may crash. **Mitigation**: Wrap components in error boundary (`try/catch` in Razor, or custom `ErrorBoundary` component).

### Trade-offs
| Choice | Benefit | Cost |
|--------|---------|------|
| JSON data source | Simple, no DB setup | Manual data management, no queries |
| Vanilla CSS | Full control, minimal overhead | More CSS to write; no dark mode out-of-box |
| Blazor Server | Integrated, no JavaScript | Requires .NET runtime; connection dependency |
| No auth | Fast to build | No audit trail or multi-user segregation |

## Open Questions

1. **Real or Mock Data?** - Is `data.json` generated from another system (e.g., JIRA API, Azure DevOps) or hand-maintained? If generated, what's the integration plan?
2. **Update Frequency** - How often does data refresh? Real-time, daily, manual? Affects caching and refresh button design.
3. **Historical Trend** - Should dashboard show velocity trend (burndown), or just current state? Required? If yes, may need date-stamped snapshots in data.
4. **Chart Interactivity** - Are charts/timelines interactive (hover tooltips, click to filter), or purely visual/static? Affects Chart.js vs. simple SVG decision.
5. **Multiple Projects** - Is this dashboard for one project, or a selector for many? Affects data model and navigation.
6. **Customization Level** - Will executives customize colors, logos, layout per project? If yes, add configuration section to `data.json`.
7. **Offline Capability** - Must work without internet/server? (Unlikely for local app, but confirm.)
8. **Browser Requirements** - Any old IE/legacy browser support needed? (Unlikely for internal tool; assume modern browsers.)

## Implementation Recommendations

### MVP Scope (Sprint 1: Week 1-2)
1. **Data Model** - Define `ProjectData`, `Milestone`, `WorkItem` classes.
2. **DataService** - Load & deserialize `data.json` using `System.Text.Json`.
3. **Dashboard.razor** - Render project header (name, status, dates).
4. **Mock data.json** - Create example file with 5 milestones, 20 work items across three categories.
5. **Basic CSS** - Grid layout, card styles, typography for readability.
6. **Test** - Write xUnit tests for `DataService` deserialization; manual browser test of rendering.

**Deliverable**: Single-page dashboard showing project info and work items by category. Rough layout.

### Phase 2 (Week 3-4)
1. **Timeline Component** - Render milestones horizontally with visual indicators (complete/incomplete).
2. **StatusCards** - Individual cards for each work item with story points, assignee.
3. **MetricsPanel** - KPI summary: % complete, velocity, etc.
4. **CSS Refinement** - Polish layout, spacing, typography; ensure print-friendly.
5. **Error Handling** - Graceful fallback if `data.json` missing or malformed.

**Deliverable**: Fully styled, print-ready dashboard. Screenshot-ready for PowerPoint.

### Phase 3 (Optional; Post-MVP)
1. **Chart Integration** - Add Chart.js for burndown or velocity chart (if required).
2. **Filtering** - Button toggles to show/hide categories (e.g., "Hide Carried Over").
3. **Refresh Button** - Manual reload of `data.json` without page refresh.
4. **Dark Mode** - CSS custom properties for theming (if executives request).
5. **Responsive Tweaks** - Mobile layout fallback (less priority since mainly desktop/print).

### Quick Wins (Demonstrable Value Early)
- **Week 1**: Basic HTML dashboard with mock data renders correctly; can take first screenshot.
- **Week 2**: Timeline and metrics visible; executives see project status at a glance.
- **Week 3**: Polished CSS; ready for PowerPoint integration.

### Areas for Prototyping Before Committing
1. **Print Layout** - Create HTML mockup; test `@media print` in Chrome, Firefox, Edge to confirm output matches expectations.
2. **Chart Library Choice** - If visualizations required, build small proof-of-concept: Chart.js vs. SkiaSharp vs. SVG-based component. Compare render time and output quality.
3. **Data Integration** - If `data.json` sourced from external system, prototype JSON parsing with real API response format.

### Development Workflow
1. `dotnet new blazorserver -n AgentSquad.Dashboard` (or extend existing project).
2. Create `Models/` folder and classes.
3. Create `Services/DataService.cs`, load from `wwwroot/data/data.json`.
4. Build `Components/Dashboard.razor` as root component.
5. Add child components as outlined.
6. Test in `dotnet watch`.
7. Run `dotnet publish -c Release` for production build.
8. Verify output in browser; take screenshots.

---
