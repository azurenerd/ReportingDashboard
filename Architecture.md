# Architecture

## Overview & Goals

**Executive Dashboard** is a lightweight, single-page reporting application built on Blazor Server (.NET 8) that enables executives to view project milestones, task progress, and shipping status at a glance. The application reads static project data from a JSON configuration file and renders a clean, screenshot-optimized view suitable for PowerPoint integration.

**Primary Goals:**
1. Enable executives to view complete project progress without manual report creation
2. Provide pixel-perfect, screenshot-ready visuals for PowerPoint decks
3. Reduce reporting friction by eliminating manual slide creation and data entry
4. Create maintainable reporting using JSON data as single source of truth
5. Deploy as self-contained Windows .exe with zero infrastructure dependencies

**Architecture Principles:**
- Simplicity over enterprise patterns (minimize dependencies, cognitive load)
- Static data model (no real-time updates, polling, or database)
- Unidirectional data flow (singleton load → cascading parameters → stateless components)
- Screenshot consistency (fixed 1200px layout, custom CSS, no framework overhead)
- Single-project structure (<500 LOC total, colocated components/models/styles)

---

## System Components

### 1. Program.cs (Application Startup & DI Configuration)

**Responsibility:** Initialize Kestrel server, load JSON data, register services, launch browser.

**Key Behaviors:**
- Read data.json from disk at startup (eager load, one-time)
- Deserialize JSON to strongly-typed ProjectData POCO via System.Text.Json
- Register ProjectData as singleton in DI container
- Configure Kestrel to listen on http://localhost:5000
- Auto-launch browser via Process.Start("http://localhost:5000")
- Catch JsonException, FileNotFoundException; log errors; exit with code 1

**Dependencies:**
- System.Text.Json (built-in, .NET 8)
- Microsoft.Extensions.Logging (built-in)
- Microsoft.AspNetCore.Builder (Blazor Server host)

**Data Ownership:**
- Loads and deserializes data.json into memory
- Registers ProjectData singleton in service container
- Owns appsettings.json configuration (logging levels, Kestrel port)

---

### 2. App.razor (Root Component & Cascading Parameter Provider)

**Responsibility:** Root Blazor component; injects ProjectData from DI; cascades to children via CascadingParameter.

**Key Behaviors:**
- Receive ProjectData singleton from DI via @inject
- Cascade ProjectData to all child components
- Call OnInitializedAsync() to verify ProjectData is not null
- Render MainLayout and Dashboard child components
- Display error banner if ProjectData is null

**Dependencies:**
- ProjectData (injected from DI container)
- MainLayout.razor (parent layout)
- Dashboard.razor (main content page)

**Data Ownership:**
- Holds reference to injected ProjectData singleton
- Cascades unchanged to children (read-only reference)

---

### 3. MainLayout.razor (Page Shell & Layout Container)

**Responsibility:** Define consistent page structure (header, navigation, footer, main content area).

**Key Behaviors:**
- Render <div class="dashboard-container"> with fixed 1200px max-width
- Render <header> for logo, title, project name
- Render <main>@Body</main> for page content
- Render <footer> with optional timestamp
- Apply CSS grid/flexbox for responsive sections (mobile-disabled via fixed width)

**Dependencies:**
- RenderFragment Body (injected child content)
- app.css (all styling)

**Data Ownership:**
- Owns HTML structure and CSS layout
- Optional: static header/footer text (project name, company branding)

---

### 4. Dashboard.razor (Main Page & Data Distributor)

**Responsibility:** Orchestrate dashboard layout; distribute ProjectData to child visualization components.

**Key Behaviors:**
- Receive ProjectData via CascadingParameter
- Pass Milestones to MilestoneTimeline component
- Pass Tasks to ProgressSummary, StatusCards, MetricsPanel components
- Pass ProjectName, StartDate, TargetCompletion to MetricsPanel
- Arrange components in logical order (timeline → metrics → progress → status cards)

**Dependencies:**
- ProjectData (cascading parameter)
- MilestoneTimeline.razor
- MetricsPanel.razor
- ProgressSummary.razor
- StatusCards.razor

**Data Ownership:**
- Receives read-only ProjectData reference
- Distributes data slices to children via [Parameter] attributes
- No state mutations

---

### 5. MilestoneTimeline.razor (Custom HTML/CSS Timeline Visualization)

**Responsibility:** Render chronological milestone timeline using custom HTML/CSS (no charting library).

**Key Behaviors:**
- Receive List<Milestone> via [Parameter]
- Sort milestones by Date (ascending)
- Render each milestone as <div class="timeline-item">
- Display milestone.Date (formatted as "MMM dd") and milestone.Name
- Use CSS Grid/flexbox for horizontal timeline layout
- Support print media query: page-break-inside: avoid

**Dependencies:**
- app.css (timeline styling: CSS Grid, flexbox, colors)
- No external charting library

**Data Ownership:**
- Receives read-only Milestone collection
- Owns rendering logic and timeline layout

---

### 6. MetricsPanel.razor (Project Metadata & KPI Cards)

**Responsibility:** Display project-level metadata and summary statistics.

**Key Behaviors:**
- Receive ProjectName, StartDate, TargetCompletion, Tasks via [Parameter]
- Compute: ShippedCount, InProgressCount, CarriedOverCount (LINQ queries)
- Compute: CompletionPercentage = (ShippedCount / TotalCount) * 100
- Compute: DaysRemaining = (TargetCompletion - DateTime.UtcNow).Days
- Render metric cards: project name, start/target dates, task counts, completion %
- Use CSS cards for visual grouping

**Dependencies:**
- app.css (card styling, colors, typography)

**Data Ownership:**
- Receives read-only project metadata and task list
- Computes aggregate statistics (no state mutations)

---

### 7. ProgressSummary.razor (Bar Chart via ChartJs Blazor)

**Responsibility:** Render bar chart showing task count distribution by status.

**Key Behaviors:**
- Receive List<Task> via [Parameter]
- Count tasks by Status: Shipped, InProgress, CarriedOver
- Configure BarChartConfig with labels ["Shipped", "In Progress", "Carried Over"]
- Populate chart dataset with counts as double values
- Initialize Chart.js via ChartJs Blazor interop
- Render chart in <div> with CSS class "chart-container"

**Dependencies:**
- ChartJs.Blazor NuGet v4.1.2
- app.css (chart container sizing, responsive wrapper)
- Chart.js JavaScript library (auto-loaded by Blazor)

**Data Ownership:**
- Receives read-only Task collection
- Transforms data for chart consumption (counts by status)

---

### 8. StatusCards.razor (Three-Column Task Breakdown Layout)

**Responsibility:** Display tasks grouped by status in three-column card layout.

**Key Behaviors:**
- Receive List<Task> via [Parameter]
- Group tasks: ShippedTasks, InProgressTasks, CarriedOverTasks (LINQ)
- Render three <div class="status-column"> sections (shipped, in-progress, carried-over)
- Display count in column header: "Shipped (3)"
- Render each task as <div class="task-card">@task.Name</div>
- Use CSS flexbox for three-column responsive layout (fixed width)

**Dependencies:**
- app.css (column layout, card styling, colors)

**Data Ownership:**
- Receives read-only Task collection
- Groups by Status (no state mutations)

---

### 9. ProjectData (Singleton Model & Data Service)

**Responsibility:** Hold deserialized JSON data in memory; provide strongly-typed access throughout application.

**Definition:**
```csharp
public class ProjectData
{
    [Required]
    public string ProjectName { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime TargetCompletion { get; set; }
    
    [Required]
    public List<Milestone> Milestones { get; set; } = new();
    
    [Required]
    public List<Task> Tasks { get; set; } = new();
}
```

**Lifespan:** Singleton (lives for entire application runtime)
**Mutability:** Read-only (deserialized once; no setters; components consume data only)
**Storage:** In-memory (loaded from data.json at startup)

**Dependencies:**
- Owned by Program.cs (instantiated, registered in DI)
- Referenced by all components via CascadingParameter or @inject

---

### 10. Milestone (POCO Model)

**Responsibility:** Represent single project milestone with date and description.

**Definition:**
```csharp
public class Milestone
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}
```

**Constraints:**
- Name: required, max 100 characters
- Date: required, ISO 8601 format in JSON
- Description: optional, max 500 characters

**Dependencies:**
- Owned by ProjectData (child collection)
- Referenced by MilestoneTimeline component

---

### 11. Task (POCO Model)

**Responsibility:** Represent single project task with status.

**Definition:**
```csharp
public class Task
{
    [Required]
    [MaxLength(50)]
    public string Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }
    
    [Required]
    [EnumDataType(typeof(TaskStatus))]
    public TaskStatus Status { get; set; }
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}

public enum TaskStatus
{
    Shipped = 0,
    InProgress = 1,
    CarriedOver = 2
}
```

**Constraints:**
- Id: required, unique within project, max 50 characters (alphanumeric + hyphen)
- Name: required, max 200 characters
- Status: required, enum value (Shipped | InProgress | CarriedOver)
- Description: optional, max 500 characters

**Dependencies:**
- Owned by ProjectData (child collection)
- Referenced by ProgressSummary, StatusCards, MetricsPanel components

---

### 12. CSS Styling Layer (app.css)

**Responsibility:** Define all visual styling, layout structure, and print optimization.

**File Location:** `wwwroot/app.css` (~200 lines)

**Sections:**
- **Reset/Base:** fonts, colors, box-sizing, margins/padding
- **Layout Container:** max-width 1200px, margin 0 auto, background white
- **Header/Footer:** project title, optional branding, spacing
- **Timeline:** CSS Grid layout for horizontal milestone sequence
- **Cards:** metric cards, task cards, border, shadow, hover states
- **Columns:** flexbox three-column layout for status breakdown
- **Chart Container:** responsive wrapper for ChartJs chart
- **Print Media Queries:** @media print { hide non-essential UI, ensure monochrome, page-break settings }

**Print Optimization:**
```css
@media print {
  body { background: white; color: #000; margin: 0; padding: 0; }
  .dashboard-container { max-width: 100%; }
  .no-print { display: none; }
  .timeline-item { page-break-inside: avoid; }
  .status-column { page-break-inside: avoid; }
}
```

---

## Component Interactions

### Primary Use Case: Application Startup & Dashboard Render

```
1. User double-clicks ExecDashboard.exe

2. Program.cs Main()
   ├─ File.ReadAllText("data.json")
   ├─ JsonSerializer.Deserialize<ProjectData>(json, options)
   ├─ builder.Services.AddSingleton(projectData)
   ├─ var app = builder.Build()
   ├─ app.MapBlazorHub()
   ├─ Process.Start("http://localhost:5000")
   └─ await app.RunAsync()

3. Browser loads http://localhost:5000
   ├─ index.html loads Blazor.js
   ├─ SignalR WebSocket connection established
   └─ Kestrel pre-renders HTML circuit

4. Blazor renders App.razor
   ├─ [Inject] ProjectData from DI container
   ├─ OnInitializedAsync() verifies ProjectData != null
   ├─ [CascadingParameter] passes ProjectData to children
   └─ Render MainLayout component

5. MainLayout.razor renders container
   └─ Render Dashboard via @Body RenderFragment

6. Dashboard.razor receives ProjectData
   ├─ Pass Milestones to MilestoneTimeline
   ├─ Pass Tasks, ProjectName, StartDate, TargetCompletion to MetricsPanel
   ├─ Pass Tasks to ProgressSummary
   └─ Pass Tasks to StatusCards

7. Child components render in parallel
   ├─ MilestoneTimeline: sort milestones by date, render CSS Grid timeline
   ├─ MetricsPanel: compute aggregate stats (shipped/in-progress/carried-over counts, %), render cards
   ├─ ProgressSummary: count tasks by status, initialize ChartJs bar chart
   └─ StatusCards: group tasks by status, render three-column flexbox layout

8. Browser displays fully rendered dashboard
   ├─ Timeline at top showing 5-10 milestones chronologically
   ├─ Metrics panel showing project name, dates, task counts
   ├─ Bar chart showing shipped/in-progress/carried-over distribution
   └─ Three-column task status breakdown

9. User workflow: screenshots/prints for PowerPoint
   ├─ Press F12 (DevTools) or Ctrl+P (Print)
   ├─ Print preview renders fixed 1200px layout identically
   ├─ User exports to PDF via native browser print
   └─ User inserts PDF/screenshot into PowerPoint deck
```

### Secondary Use Case: Operator Updates data.json & Restarts

```
1. Operator edits data.json (update milestone dates, task counts, status)
2. Operator closes application (closes browser tab or Ctrl+C in console)
3. Operator double-clicks ExecDashboard.exe again
4. Program.cs loads NEW data.json from disk
5. JsonSerializer deserializes updated ProjectData
6. New ProjectData singleton registered in DI
7. Blazor re-renders App.razor with new data
8. All child components receive updated data via parameters
9. Dashboard displays updated milestones, tasks, metrics
```

### Data Flow Diagram

```
data.json (file system)
    ↓
Program.cs (deserialize)
    ↓
ProjectData singleton (DI container)
    ↓
App.razor (@inject)
    ↓
[CascadingParameter] ProjectData
    ↓
MainLayout.razor → Dashboard.razor
                        ↓
                ┌───────┬──────┬──────────┐
                ↓       ↓      ↓          ↓
            Timeline  Metrics Progress  Status
                       Chart    Cards
```

**Data Flow Characteristics:**
- **Unidirectional:** data flows downward only (no upward binding)
- **Stateless:** components receive data via parameters; no state mutations
- **Immutable:** ProjectData singleton never modified after creation
- **Eager Load:** all data loaded once at startup; no polling/refresh
- **No Callbacks:** child components don't communicate back to parent

---

## Data Model

### Entity Relationships

```
ProjectData (1)
├─ Milestones (0..*)
│  ├─ Name: string (required, max 100)
│  ├─ Date: DateTime (required, ISO 8601)
│  └─ Description: string (optional, max 500)
└─ Tasks (0..*)
   ├─ Id: string (required, max 50, unique)
   ├─ Name: string (required, max 200)
   ├─ Status: TaskStatus enum (required)
   └─ Description: string (optional, max 500)

TaskStatus enum
├─ Shipped = 0
├─ InProgress = 1
└─ CarriedOver = 2
```

**Relationship Type:** Composition (Milestones and Tasks are owned by ProjectData)
**Multiplicity:** 1-to-many (one ProjectData has many Milestones and Tasks)
**Cross-References:** None (Milestones and Tasks are independent; no foreign keys)

### JSON Storage Format

**File:** `data.json` (application root or `wwwroot/data/data.json`)
**Format:** JSON (System.Text.Json compatible, camelCase property names)
**Size Limit:** <1MB
**Encoding:** UTF-8
**Date Format:** ISO 8601 (e.g., "2026-01-15T00:00:00Z")

**Sample Structure:**
```json
{
  "projectName": "Q1 2026 Platform Release",
  "startDate": "2026-01-15T00:00:00Z",
  "targetCompletion": "2026-04-15T00:00:00Z",
  "milestones": [
    {
      "name": "Design Phase Complete",
      "date": "2026-01-31T00:00:00Z",
      "description": "UI/UX design finalized and approved"
    }
  ],
  "tasks": [
    {
      "id": "core-auth",
      "name": "Authentication Module",
      "status": "Shipped",
      "description": "JWT-based authentication system"
    }
  ]
}
```

### Deserialization Configuration

```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,     // Allow camelCase in JSON
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
};

var projectData = JsonSerializer.Deserialize<ProjectData>(json, options);
```

### Validation Rules

**At Deserialization Time:**
- All [Required] fields must be present in JSON
- TaskStatus enum values must match case-insensitively: "Shipped", "InProgress", "CarriedOver"
- Dates must parse as valid DateTime (ISO 8601 format)
- String properties must not exceed [MaxLength] constraints
- Empty collections (Milestones, Tasks) are allowed (no minimum size)

**Error Handling:**
```csharp
try 
{
    var data = JsonSerializer.Deserialize<ProjectData>(json, options);
    if (data == null) throw new InvalidOperationException("Deserialized data is null");
    logger.LogInformation("✓ Loaded {Name} ({M} milestones, {T} tasks)",
        data.ProjectName, data.Milestones.Count, data.Tasks.Count);
}
catch (JsonException ex)
{
    logger.LogError("✗ JSON syntax error: {Message}", ex.Message);
    Console.WriteLine($"ERROR: data.json is malformed.\n\nDetails:\n{ex.Message}");
    Environment.Exit(1);
}
catch (FileNotFoundException ex)
{
    logger.LogError("✗ data.json not found in application directory");
    Console.WriteLine("ERROR: data.json missing. Location: application root directory");
    Environment.Exit(1);
}
```

---

## API Contracts

### Component Parameter Contracts (Blazor Parameters)

**MilestoneTimeline.razor:**
```csharp
[Parameter]
public List<Milestone> Milestones { get; set; }  // read-only
```

**ProgressSummary.razor:**
```csharp
[Parameter]
public List<Task> Tasks { get; set; }  // read-only
```

**StatusCards.razor:**
```csharp
[Parameter]
public List<Task> Tasks { get; set; }  // read-only
```

**MetricsPanel.razor:**
```csharp
[Parameter] public string ProjectName { get; set; }
[Parameter] public DateTime StartDate { get; set; }
[Parameter] public DateTime TargetCompletion { get; set; }
[Parameter] public List<Task> Tasks { get; set; }  // all read-only
```

### Cascading Parameter Contract

**App.razor → Dashboard.razor:**
```csharp
[CascadingParameter]
public ProjectData Data { get; set; }  // read-only reference
```

### Dependency Injection Contract

**Program.cs → All Components:**
```csharp
// Registration
builder.Services.AddSingleton<ProjectData>(projectData);

// Consumption
@inject ProjectData Data  // or [Inject] ProjectData Data in code-behind
```

**Lifecycle:** Singleton (same instance for app lifetime)
**Access:** Application-wide

### No REST API Endpoints

**Important:** This application has NO REST API endpoints. All communication is internal to Blazor Server (component parameters, DI injection). The architecture is strictly:
- Server-side rendering (Blazor Server, not WASM)
- Unidirectional data flow (parameters, cascading parameters)
- No WebSocket/SignalR message passing (data is static)
- No fetch/HTTP calls from components

---

## Infrastructure Requirements

### Hosting & Deployment

**Deployment Target:** Windows 10 (v1809+), Windows 11 (win-x64 architecture)
**Runtime:** .NET 8.0.x Runtime (bundled in self-contained .exe)
**Web Server:** Kestrel (built-in to Blazor Server template, no IIS/nginx required)
**Port:** 5000 (localhost only, no external network)
**Execution Model:** Self-contained executable (110-140MB)

### Network Configuration

**Kestrel Server Binding (appsettings.json):**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

**Port:** 5000 (hardcoded, single port)
**Host:** 127.0.0.1 (localhost only; no external/remote access)
**Protocol:** HTTP (no HTTPS; local-only tool)
**Concurrent Connections:** Default Kestrel configuration (sufficient for 5-20 users on single machine)

### Local Storage

**File Locations:**
- `data.json` → Application root directory or `wwwroot/data/data.json`
- `app.css` → `wwwroot/app.css` (200 lines, custom CSS only)
- `index.html` → `wwwroot/index.html` (minimal Blazor host)
- `appsettings.json` → Application root (logging levels, Kestrel config)

**File Permissions:** Windows NTFS ACL on `data.json` (if data contains confidential info)

**No Database:** Zero database infrastructure (data.json is source of truth)
**No Cloud Storage:** All data local to machine

### Application Configuration

**appsettings.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
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

**Logging:** Console output only (no file logging in MVP)

### Build & Publishing

**Build Commands:**
```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Self-contained executable (win-x64)
dotnet publish -c Release -r win-x64 --self-contained
```

**Output Structure:**
```
bin/Release/net8.0/win-x64/publish/
├── ExecDashboard.exe (110-140MB, includes .NET 8 runtime)
├── data.json
├── wwwroot/
│   ├── app.css
│   ├── app.js (empty, optional for future interop)
│   └── index.html
└── [.NET 8 runtime binaries and dependencies]
```

### Project File Configuration

**ExecDashboard.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <OutputType>WinExe</OutputType>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <PublishTrimmed>true</PublishTrimmed>
    <PublishReadyToRun>true</PublishReadyToRun>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ChartJs.Blazor" Version="4.1.2" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="data.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

</Project>
```

**Key Settings:**
- `<TargetFramework>net8.0</TargetFramework>` — Lock to .NET 8 LTS (do NOT use net8.* or net9.0 without testing)
- `<OutputType>WinExe</OutputType>` — Windows executable (no console window)
- `<RuntimeIdentifier>win-x64</RuntimeIdentifier>` — 64-bit Windows only
- `<PublishTrimmed>true</PublishTrimmed>` — Remove unused runtime code (reduces size)
- `<PublishReadyToRun>true</PublishReadyToRun>` — Pre-JIT compilation (faster startup)

### Testing Infrastructure

**Test Project:** ExecDashboard.Tests.csproj
**Framework:** xUnit 2.6.x (unit tests only; no integration or E2E tests in MVP)
**Test Location:** Project root as separate project

**Sample Test:**
```csharp
public class ProjectDataDeserializationTests
{
    [Fact]
    public void ValidJson_Deserializes_Successfully()
    {
        var json = File.ReadAllText("../ExecDashboard/data.json");
        var data = JsonSerializer.Deserialize<ProjectData>(json, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(data);
        Assert.NotEmpty(data.ProjectName);
    }

    [Fact]
    public void MalformedJson_Throws_JsonException()
    {
        var json = "{ invalid json }";
        Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<ProjectData>(json));
    }
}
```

### CI/CD (Optional GitHub Actions)

**Build & Test Pipeline:**
```yaml
name: Build & Test

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
      - run: dotnet restore
      - run: dotnet build -c Release
      - run: dotnet test ExecDashboard.Tests/ExecDashboard.Tests.csproj
      - run: dotnet publish -c Release -r win-x64 --self-contained
      - uses: actions/upload-artifact@v3
        with:
          name: ExecDashboard-exe
          path: bin/Release/net8.0/win-x64/publish/ExecDashboard.exe
```

### Monitoring & Logging

**Logger Configuration:**
```csharp
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddConfiguration(configuration.GetSection("Logging"));
```

**Log Output (Console):**
```
info: ExecDashboard.Program[0]
      Application starting...
info: ExecDashboard.Program[0]
      ✓ Loaded Q1 2026 Platform Release (3 milestones, 15 tasks)
info: ExecDashboard.Program[0]
      Kestrel server listening on http://localhost:5000
```

### Performance Baselines

| Metric | Target | Implementation |
|--------|--------|-----------------|
| Data Load Time | <100ms | System.Text.Json eager deserialization at startup |
| App Cold Start | <5s | Kestrel init (~1s) + browser launch (~2-3s) |
| Page Load (Browser) | <2s | Server pre-renders HTML; minimal JavaScript |
| Component Render | <10ms | Blazor diffing algorithm, small component tree |
| JSON File Size | <1MB | No performance bottleneck; <50ms deserialize time |

### System Requirements

**Minimum:**
- Windows 10 (v1809+) or Windows 11
- 500MB free disk space (for .exe)
- 1GB RAM available (baseline: ~200MB Kestrel + runtime, +50KB per concurrent user)
- Browser: Chrome 100+, Edge 100+, Firefox 100+
- No .NET SDK required on end-user machine (bundled in self-contained .exe)

**Network:**
- Localhost only (no internet required)
- Port 5000 available and not blocked by firewall
- Check: `netstat -ano | findstr :5000`

---

## Technology Stack Decisions

### Framework & Runtime

**Choice:** Blazor Server, .NET 8.0.x LTS

**Rationale:**
- Server-side rendering eliminates client-side runtime installation (zero .NET pre-install required)
- Built-in component model, DI, logging, Kestrel server (minimal dependencies)
- Strongly-typed C# eliminates runtime errors in data binding
- .NET 8 LTS (support until Nov 2026) provides stability for MVP

**Alternatives Considered:**
- Blazor WASM: Requires .NET 8 runtime installed on client; rejected (violates zero-infrastructure goal)
- ASP.NET Core MVC: Less suitable for client-side interactivity (no component hierarchy)
- Next.js/React: Requires Node.js build pipeline; incompatible with mandate for C# .NET 8

---

### Data Serialization

**Choice:** System.Text.Json (built-in to .NET 8)

**Rationale:**
- Native to .NET 8; no external dependency
- 3x faster than Newtonsoft.Json; lighter (no bloat)
- Sufficient for simple JSON deserialization (no advanced schema validation needed)
- Supports required attributes ([Required], [MaxLength]) for validation

**Alternatives Considered:**
- Newtonsoft.Json (Json.NET): Slower, heavier, deprecated for .NET 8; rejected
- No custom JSON parsing: requires string manipulation; error-prone

---

### Charting & Visualization

**Choice:** ChartJs Blazor 4.1.2 (bar chart) + Custom HTML/CSS (timeline)

**Rationale:**
- ChartJs Blazor: Lightweight (~50KB), proven in 300+ Blazor projects, clean bar chart rendering
- Custom CSS timeline: 20 lines of code, full control for screenshot consistency, zero external dependency
- Avoids expensive enterprise libraries (Syncfusion ~$1,000/dev/yr) and niche solutions (OxyPlot)

**Alternatives Considered:**
- Syncfusion Blazor Charts: Enterprise-grade, but commercial license + overkill complexity; rejected
- OxyPlot: Limited Blazor documentation; niche community; rejected
- All JavaScript charting (Chart.js, Recharts): Requires JavaScript interop; higher complexity

---

### Styling & CSS

**Choice:** Plain CSS (~200 lines, custom, no framework)

**Rationale:**
- No framework overhead (Bootstrap ~200KB, Tailwind requires Node.js build)
- Fixed 1200px layout ensures screenshot consistency across browsers
- Full control for print optimization via @media print
- CSS media queries handle responsive sections naturally

**Alternatives Considered:**
- Bootstrap 5: Justified only if OriginalDesignConcept.html uses Bootstrap classes; otherwise bloat; rejected
- Tailwind CSS: Requires Node.js/PostCSS build pipeline; adds 5+ minutes setup; overkill for 1-page; rejected
- Material Design: Excessive component library for simple dashboard; rejected

---

### State Management

**Choice:** Unidirectional component parameters (no state containers)

**Rationale:**
- Simplest pattern for read-only dashboard
- Data flows: Program.cs → DI singleton → App.razor (inject) → Dashboard (cascade) → Children (parameters)
- No two-way binding (@bind); no event callbacks; minimal mental overhead
- Stateless components render predictably for screenshot consistency

**Alternatives Considered:**
- Two-way binding (@bind): Simpler syntax but harder to trace data flow; obscures updates; rejected
- Services & event callbacks: Unnecessary complexity for static data model; rejected
- MVVM (CommunityToolkit.Mvvm): Useful if state complexity grows; defer to Phase 3 if needed

---

### Deployment Model

**Choice:** Self-contained .exe (win-x64, 110-140MB)

**Rationale:**
- Zero end-user infrastructure: no .NET runtime pre-install, no IIS/nginx, no Docker
- Double-click user experience: immediate launch without configuration
- 110-140MB size acceptable for internal tool distribution (email, shared drive)
- Kestrel built-in to Blazor Server template; no additional server required

**Alternatives Considered:**
- Framework-dependent .exe: Requires .NET 8 runtime on client; rejected (violates zero-infrastructure goal)
- IIS Express: Professional but requires Windows developer setup; rejected
- Docker: Adds 5+ minute Docker Desktop setup for users; self-contained .exe cleaner; rejected
- Wix installer (.msi): Professional but adds complexity; defer to Phase 3 if multi-machine deployment needed

---

### Testing Framework

**Choice:** xUnit 2.6.x (unit tests only)

**Rationale:**
- Unit tests validate JSON deserialization (primary failure mode)
- Unit tests catch model schema mismatches before deployment
- xUnit: lightweight, widely adopted, no unnecessary dependencies
- Defer integration/E2E tests to Phase 3 (low ROI for simple 1-page dashboard)

**Alternatives Considered:**
- MSTest: Heavier; overkill for MVP; rejected
- NUnit: Older; xUnit more modern; rejected
- Integration/E2E tests: Expensive setup (Blazor host, Selenium); low ROI; defer

---

### Logging

**Choice:** Microsoft.Extensions.Logging (built-in to .NET)

**Rationale:**
- Built-in to ASP.NET Core; zero additional dependency
- Console output sufficient for diagnostics (no file logging in MVP)
- Supports log levels (Information, Warning, Error, Critical)
- Enables structured logging for future observability

**Alternatives Considered:**
- Serilog: Additional dependency for logging; overkill for console output; defer
- NLog: Similar to Serilog; defer if file/remote logging needed in Phase 3

---

## Security Considerations

### Authentication & Authorization

**Current Model:** None required (internal tool, single machine, trusted team)

**Rationale:**
- Dashboard contains non-sensitive project metadata (milestone names, task counts, status)
- Single-user per .exe instance; no multi-user sharing
- Windows file-level permissions (NTFS ACL) sufficient to control data.json access

**If Future Cloud Deployment Needed (Phase 3+):**
- Implement Windows Authentication (IIS) via `[Authorize]` attribute
- Integrate with Azure AD via OpenIdConnect middleware
- Add role-based access control (RBAC) for executive vs. operator roles

---

### Data Protection

**Current Model:** No encryption (local-only tool)

**Rationale:**
- HTTP only on localhost:5000 (no external network exposure)
- No sensitive PII in sample data (no passwords, API keys, personal information)
- Windows NTFS ACL controls file-level access to data.json

**If data.json Contains Confidential Info:**
- Apply Windows file-level permissions: `icacls "C:\data.json" /grant "Domain\Users":R`
- No application-level encryption needed for local file (NTFS handles it)
- No TLS/HTTPS required (localhost, no remote access)

**If Future Remote Access Needed:**
- Add TLS via reverse proxy (nginx, IIS) with valid certificate
- Implement client certificate authentication or API key validation
- Encrypt data at rest using Windows BitLocker or application-level encryption

---

### Input Validation

**JSON Deserialization:**
- System.Text.Json validates schema automatically via POCO attributes
- [Required] attributes enforce mandatory fields; deserialization fails if missing
- [MaxLength(n)] constraints prevent buffer overflows
- [EnumDataType(typeof(TaskStatus))] validates enum values only accept Shipped/InProgress/CarriedOver

**Component Rendering:**
- Razor components HTML-encode output by default (`@task.Name` rendered as text, not HTML)
- No user input accepted in components (read-only dashboard)
- No XSS risk; Blazor Server context isolation prevents script injection

**Malformed JSON Handling:**
```csharp
try 
{
    var data = JsonSerializer.Deserialize<ProjectData>(json, options);
}
catch (JsonException ex)
{
    logger.LogError("Invalid data.json: {Message}", ex.Message);
    // User-friendly error message; do NOT expose stack trace to user
    Console.WriteLine("ERROR: data.json is malformed. Please check syntax.");
    Environment.Exit(1);
}
```

---

### Denial of Service (DoS) Prevention

**Risk Level:** Low (local-only, single machine)

**Mitigation:**
- JSON file size limited to <1MB (system constraint)
- Deserialization fails fast on invalid structure (no parsing loops)
- One-time load at startup; no continuous JSON parsing in runtime
- File size validation:
```csharp
var fileInfo = new FileInfo("data.json");
if (fileInfo.Length > 1_000_000)  // 1MB max
{
    logger.LogError("data.json exceeds 1MB limit");
    Environment.Exit(1);
}
```

---

### Secrets Management

**Current Model:** No secrets (no API keys, passwords, database credentials)

**If Future Integration Needed (Phase 3+):**
- Use .NET User Secrets API: `dotnet user-secrets init`
- Never commit secrets to source control (.gitignore `secrets.json`)
- Store production secrets in Azure Key Vault (if cloud deployment added)
- Use environment variables for deployment-time config (database URL, API keys, etc.)

---

## Scaling Strategy

### Horizontal Scaling (Not Required for MVP)

**Current Capacity:** Single Kestrel instance on one Windows machine
**Target Load:** 5-20 concurrent users (same machine)
**Assessment:** No horizontal scaling needed for MVP; Kestrel handles 10,000+ concurrent connections per machine

**If Future Multi-Machine Deployment Needed (Phase 3+):**
- Deploy separate .exe instances on multiple Windows machines
- Use network load balancer (nginx, HAProxy, Azure Load Balancer) to distribute incoming connections
- Each instance reads same data.json from centralized SMB network share
- No state sharing required (stateless dashboard design enables trivial scale-out)
- Example load balancer config (nginx):
```nginx
upstream dashboard {
    server machine1:5000;
    server machine2:5000;
    server machine3:5000;
}
server {
    listen 80;
    location / {
        proxy_pass http://dashboard;
        proxy_set_header Host $host;
    }
}
```

---

### Vertical Scaling (Single Machine Optimization)

**Memory Estimation:**
- Baseline: ~200MB (Kestrel + .NET 8 runtime)
- ProjectData singleton: <1MB (JSON < 1MB)
- Per-connection overhead: ~50KB (SignalR session + component state)
- Projected for 20 users: ~200MB + (20 × 50KB) = ~1.2GB
- **Mitigation:** None needed; available on any modern Windows machine (min 2GB RAM recommended)

**CPU Analysis:**
- JSON deserialization: <50ms (one-time at startup)
- Component render per user: <10ms (Blazor diffing algorithm)
- Kestrel serving static assets: negligible CPU
- Projected for 20 concurrent users: <1% CPU utilization
- **Mitigation:** None needed

**Disk I/O:**
- Read data.json once at startup: <100ms
- No subsequent disk access (all data cached in memory)
- Console logging only (no file I/O in MVP)
- **Mitigation:** None needed

---

### Caching Strategy

**Data Caching:**
- ProjectData loaded once at app startup via eager load
- Stored as singleton in DI container; survives application lifetime
- No cache invalidation logic; user triggers refresh via application restart (intentional design)
- **Assessment:** Optimal for static reporting use case

**Browser Caching:**
- Static assets (app.css, JS bundles) cached via HTTP headers
- Kestrel default: `CacheControl: max-age=31536000` (1 year)
- **Assessment:** Sufficient; minimal assets (~50KB total)

**Component Virtualization:**
- Not needed for MVP (5-10 milestones + 10-100 tasks << Blazor limit of 50-100 components)
- If task count exceeds 500 in future: add Blazor `Virtualize` component to StatusCards
- **Assessment:** Defer to Phase 3 if performance metrics indicate slowdown

---

### Load Balancing (Not Required for MVP)

**Current Model:** Single Kestrel server on localhost:5000

**If Multi-Machine Deployment Added (Phase 3+):**
- Deploy N identical .exe instances on separate Windows machines
- Place network load balancer in front: nginx (reverse proxy), HAProxy, or Azure Load Balancer
- Enable sticky sessions (IP hash) to maintain SignalR connection affinity
- Centralize data source: data.json on SMB network share or via API endpoint
- Each instance reads from shared data source; no inter-instance communication required

---

### Bottleneck Mitigation

| Bottleneck | Risk | Mitigation Strategy | Priority |
|-----------|------|-------------------|----------|
| JSON deserialization | None (<100ms, one-time) | Monitor startup time in logs; defer optimization to Phase 3 | Low |
| Component render | None (<10ms per user) | Browser DevTools CPU profiling; Blazor diffing is efficient | Low |
| Kestrel memory | None (<1.2GB for 20 users) | Monitor via Windows Task Manager; scale vertically if needed | Low |
| data.json file I/O | None (single read at startup) | Cache as singleton; no runtime refresh needed | N/A |
| SignalR connection | Low (rare dropout) | Blazor framework auto-reconnect; user refreshes page | Low |
| Browser rendering | None (<2s page load) | Fixed layout ensures consistent render; no responsive breakpoints | N/A |

---

## Risks & Mitigations

### Technical Risks

#### Risk 1: JSON Deserialization Fails on Startup

**Severity:** HIGH (application fails to start)
**Likelihood:** MEDIUM (operator edits JSON incorrectly)
**Impact:** Dashboard inaccessible; executives see blank page or error message

**Concrete Mitigation:**

1. **Startup Validation** (Program.cs):
```csharp
try 
{
    var json = File.ReadAllText("data.json");
    var projectData = JsonSerializer.Deserialize<ProjectData>(json, options);
    
    if (projectData == null)
        throw new InvalidOperationException("Deserialized ProjectData is null");
    
    builder.Services.AddSingleton(projectData);
    logger.LogInformation("✓ Loaded {ProjectName} ({Milestones} milestones, {Tasks} tasks)",
        projectData.ProjectName, projectData.Milestones.Count, projectData.Tasks.Count);
}
catch (FileNotFoundException)
{
    logger.LogCritical("❌ data.json not found");
    Console.WriteLine("ERROR: data.json not found in application root directory");
    Environment.Exit(1);
}
catch (JsonException ex)
{
    logger.LogCritical("❌ JSON syntax error: {Message}", ex.Message);
    Console.WriteLine($"ERROR: data.json is malformed.\n\nDetails:\n{ex.Message}\n\nAction: Fix JSON syntax and restart.");
    Environment.Exit(1);
}
```

2. **Unit Test** (validate before deployment):
```csharp
[Fact]
public void SampleDataJson_Deserializes_Successfully()
{
    var json = File.ReadAllText("../ExecDashboard/data.json");
    var data = JsonSerializer.Deserialize<ProjectData>(json, options);
    Assert.NotNull(data);
    Assert.NotEmpty(data.ProjectName);
    Assert.NotEmpty(data.Milestones);
    Assert.NotEmpty(data.Tasks);
}
```

**Implementation Timeline:** Phase 1

---

#### Risk 2: ChartJs Blazor Breaking Change on .NET 8.1+ Update

**Severity:** MEDIUM (chart component fails to render)
**Likelihood:** LOW (ChartJs Blazor 4.1.2 is stable; breaking changes unlikely in 12 months)
**Impact:** Progress bar chart disappears; executives can't see task status metrics

**Concrete Mitigation:**

1. **Pin Version** (ExecDashboard.csproj):
```xml
<ItemGroup>
    <PackageReference Include="ChartJs.Blazor" Version="4.1.2" />
    <!-- DO NOT use Version="4.*" or Version="*"; lock to 4.1.2 -->
</ItemGroup>
```

2. **Unit Tests** (verify chart renders):
```csharp
[Fact]
public void ChartJsComponent_Renders_Without_Error()
{
    // Arrange: mock component with sample tasks
    var tasks = new List<Task>
    {
        new Task { Id = "1", Name = "Feature A", Status = TaskStatus.Shipped },
        new Task { Id = "2", Name = "Feature B", Status = TaskStatus.InProgress }
    };
    
    // Act: render ProgressSummary component
    // (Blazor component tests require ComponentTestBase or bUnit; optional in MVP)
    
    // Assert: chart config has correct dataset values
}
```

3. **Monitoring Plan:**
- Subscribe to ChartJs Blazor GitHub releases: https://github.com/ChartJs2Blazor/ChartJs2Blazor/releases
- Quarterly review: check .NET 8.1/8.2 release notes for compatibility warnings
- Defer NuGet upgrade until breaking change verified in local testing

**Implementation Timeline:** Phase 1 (pinning); ongoing (quarterly monitoring)

---

#### Risk 3: Print/Screenshot Renders Differently Across Windows Versions

**Severity:** MEDIUM (PowerPoint deck looks wrong)
**Likelihood:** MEDIUM (CSS rendering varies; fixed 1200px layout should mitigate)
**Impact:** Executives spend time reformatting in PowerPoint instead of presenting

**Concrete Mitigation:**

1. **Fixed-Width CSS** (wwwroot/app.css):
```css
.dashboard-container {
    max-width: 1200px;
    margin: 0 auto;
    background: white;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Arial, sans-serif;
}

@media print {
    body {
        background: white;
        color: #000;
        margin: 0;
        padding: 0;
    }
    
    .dashboard-container {
        max-width: 100%;
        page-break-after: avoid;
    }
    
    .no-print {
        display: none;
    }
    
    .timeline-item { page-break-inside: avoid; }
    .status-column { page-break-inside: avoid; }
}

* { box-sizing: border-box; }
html, body { margin: 0; padding: 0; }
```

2. **Cross-Platform Testing** (Phase 2 checkpoint):
- Test print on Windows 10 v1809, Windows 10 latest, Windows 11 (3 versions minimum)
- Use Chrome DevTools (F12 → Print) and native Windows Print dialog (Ctrl+P)
- Compare browser render vs. print preview vs. PDF export (side-by-side)
- Compare vs. OriginalDesignConcept.html visual (95%+ match target)
- Create test report: "Print output matches design template on [3 OSes tested] ✓"

3. **User Documentation:**
```
Print/Export Workflow (PowerPoint Integration):
1. Open dashboard (double-click ExecDashboard.exe)
2. Press F12 or Ctrl+P to open Print dialog
3. Select "Save as PDF" option in print dialog
4. Choose location and save
5. Insert PDF into PowerPoint deck
6. If layout appears off, zoom 100% and check margins in print preview
```

**Implementation Timeline:** Phase 2

---

#### Risk 4: Kestrel Self-Hosted .exe Blocked by Windows Defender SmartScreen

**Severity:** LOW (user sees warning; can override with one click)
**Likelihood:** LOW (unsigned .exe may trigger SmartScreen on first run from internet)
**Impact:** Non-technical executives may panic; user delay (~5 seconds)

**Concrete Mitigation (MVP):**

1. **Accept SmartScreen Warning:**
   - Unsigned .exe is expected for internal tools
   - SmartScreen shows: "Windows protected your PC"
   - User clicks "More info" → "Run anyway" (takes 5 seconds)

2. **User Documentation:**
```
Troubleshooting: "Windows protected your PC" Warning

If Windows shows a SmartScreen warning when launching ExecDashboard.exe:

1. Click "More info" in the warning dialog
2. Click "Run anyway" to proceed
3. Dashboard will launch normally

This warning is normal for unsigned applications. 
To avoid this in future, ensure .exe is from a trusted source (company IT).
```

**Concrete Mitigation (Phase 3+, Optional):**

1. **Code-Sign .exe:**
   - Purchase code-signing certificate from DigiCert (~$100-300/year)
   - Integrate into CI/CD pipeline (GitHub Actions add `signtool.exe` step)
   - Benefit: No SmartScreen warning; professional appearance
   - Cost: ~$100-300/year certificate + 1 day setup

2. **CI/CD Signing Example:**
```yaml
- name: Sign executable
  if: runner.os == 'Windows'
  run: |
    signtool sign /f certificate.pfx /p ${{ secrets.CERT_PASSWORD }} \
      bin/Release/net8.0/win-x64/publish/ExecDashboard.exe
```

**Implementation Timeline:** MVP (accept warning + document); Phase 3+ (code-sign if needed)

---

### Dependency Risks

#### Risk 5: System.Text.Json Deserialization Changes in .NET 8.1+

**Severity:** LOW (core framework; breaking changes rare)
**Likelihood:** VERY LOW (Microsoft prioritizes backward compatibility)
**Impact:** JSON parsing fails; application won't start

**Concrete Mitigation:**

1. **Lock to .NET 8 LTS:**
```xml
<PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <!-- Lock to .NET 8 LTS (support until Nov 2026); do NOT use net8.* or net9.0 -->
</PropertyGroup>
```

2. **Unit Tests Catch Breaking Changes:**
- Unit test deserializes sample data.json
- If breaking change occurs, test fails in CI/CD before release
- Developer investigates, updates POCO models if needed

3. **Upgrade Policy:**
- Do NOT auto-upgrade to .NET 8.1/8.2 without testing
- Quarterly review of .NET LTS patch releases
- Test locally on clean Windows machine before deploying to users

**Implementation Timeline:** Phase 1 (locking); ongoing (quarterly monitoring)

---

#### Risk 6: Missing or Corrupted data.json on User Machine

**Severity:** HIGH (app crashes on startup)
**Likelihood:** MEDIUM (file deleted by user, antivirus, or Windows Update)
**Impact:** Dashboard inaccessible; executives can't report on project status

**Concrete Mitigation:**

1. **File Existence & Size Check** (Program.cs):
```csharp
if (!File.Exists("data.json"))
{
    logger.LogCritical("data.json not found in application directory");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\n❌ STARTUP FAILED");
    Console.WriteLine("File not found: data.json");
    Console.WriteLine("Location: application root directory");
    Console.WriteLine("Action: Restore data.json from backup and restart");
    Console.ResetColor();
    Environment.Exit(1);
}

var fileInfo = new FileInfo("data.json");
if (fileInfo.Length == 0)
{
    logger.LogError("data.json is empty (0 bytes)");
    Console.WriteLine("ERROR: data.json is empty. Restore from backup.");
    Environment.Exit(1);
}
```

2. **User Documentation:**
```
Data Recovery Procedure

If ExecDashboard fails to start with error "data.json not found":

1. Check application root directory for data.json file
2. If missing, restore from backup:
   - Location: \\networkshare\backups\data.json (example)
   - Copy backup file to application directory
   - Rename to "data.json"
3. Restart ExecDashboard.exe

If backup unavailable, recreate data.json from last known state
or contact project operator to regenerate sample data.
```

3. **Backup Strategy:**
- Keep backup copy on network drive: `\\networkshare\backups\data.json`
- Keep backup in email (forwarded daily to project operator)
- Version control in Git (if repository available)

**Implementation Timeline:** Phase 1

---

### Operational Risks

#### Risk 7: Operator Updates data.json While Dashboard is Running

**Severity:** LOW (dashboard shows stale data until restart)
**Likelihood:** MEDIUM (operator edits file expecting auto-refresh)
**Impact:** Dashboard shows outdated project status; executives make decisions on old data

**Concrete Mitigation (MVP):**

1. **Document Expected Behavior:**
```
Data Refresh Workflow

Important: Dashboard data is static and loaded at startup.
Changes to data.json require application restart.

Procedure to update project data:
1. Close ExecDashboard (close browser tab or press Ctrl+C in console)
2. Edit data.json in text editor (JSON format)
3. Double-click ExecDashboard.exe to restart
4. Dashboard displays updated data

Attempting to edit data.json while dashboard is running
will NOT automatically refresh the display.
```

2. **User Guide Addition:**
- Add screenshot showing data.json edit workflow
- Emphasize "Restart Required" after edit
- Provide data.json format reference

**Concrete Mitigation (Phase 3+, Optional):**

1. **FileSystemWatcher Auto-Reload:**
```csharp
// Program.cs: detect data.json changes
var watcher = new FileSystemWatcher(".")
{
    Filter = "data.json",
    NotifyFilter = NotifyFilters.LastWrite
};
watcher.Changed += async (s, e) =>
{
    logger.LogInformation("data.json changed; reloading...");
    var newJson = File.ReadAllText("data.json");
    var newData = JsonSerializer.Deserialize<ProjectData>(newJson, options);
    // Update ProjectData singleton (thread-safe)
    // Notify all connected clients via SignalR to refresh UI
};
watcher.EnableRaisingEvents = true;
```

2. **UI Notification:**
- Show toast: "Project data has been updated. Reload dashboard?"
- User clicks "Reload" to refresh display with new data
- No application restart required

**Implementation Timeline:** MVP (document); Phase 3+ (FileSystemWatcher if needed)

---

#### Risk 8: Multiple .exe Instances Running Simultaneously

**Severity:** LOW (each reads same data.json; no lock conflicts)
**Likelihood:** LOW (user starts .exe twice by accident)
**Impact:** Resource usage doubles; user confusion

**Concrete Mitigation:**

1. **Accept as Low-Risk:**
   - No functional harm (read-only file access is atomic at OS level)
   - Both instances read same data.json concurrently; no lock conflicts
   - No data corruption risk

2. **User Guidance:**
```
Troubleshooting: Multiple Dashboard Windows

If dashboard appears to launch twice or shows duplicate windows:

1. Only one ExecDashboard.exe is needed per user
2. Close extra windows by clicking the X button
3. Single dashboard window will continue running normally

Having multiple instances has no negative impact on functionality.
```

**Implementation Timeline:** MVP (accept + document); no code change required

---

### Risk Matrix

| Risk ID | Risk Description | Severity | Likelihood | Impact | Effort | Priority | Status | Owner | Due Date |
|---------|------------------|----------|-----------|--------|--------|----------|--------|-------|----------|
| R1 | JSON deserialization fails | HIGH | MEDIUM | Critical startup failure | LOW | **P1** | Implement | Dev | Phase 1 |
| R2 | ChartJs Blazor breaking change | MEDIUM | LOW | Chart disappears | LOW | P2 | Implement | Dev | Phase 1 |
| R3 | Print/screenshot inconsistency | MEDIUM | MEDIUM | PowerPoint deck broken | MEDIUM | **P1** | Implement | QA | Phase 2 |
| R4 | SmartScreen blocks .exe | LOW | LOW | User delay (~5s) | LOW | P3 | Document | Doc | Phase 3 |
| R5 | System.Text.Json changes | LOW | VERY LOW | Startup failure | LOW | P3 | Monitor | Dev | Ongoing |
| R6 | data.json missing/corrupted | HIGH | MEDIUM | App won't start | LOW | **P1** | Implement | Dev | Phase 1 |
| R7 | Stale data (no auto-refresh) | LOW | MEDIUM | Outdated metrics | N/A | N/A | Document | Doc | MVP |
| R8 | Multiple .exe instances | LOW | LOW | Confusion | N/A | N/A | Accept | N/A | N/A |

---

## Implementation Roadmap

### Phase 1 (Days 1-2): Component Structure & Data Loading

**Deliverables:**
- Blazor Server project scaffold (`dotnet new blazorserver -n ExecDashboard`)
- Program.cs with JSON loading, DI registration, browser auto-launch
- Models: ProjectData, Milestone, Task, TaskStatus
- App.razor with CascadingParameter injection
- MainLayout.razor, Dashboard.razor stub
- Unit tests: JSON deserialization, valid/invalid data
- Error handling: FileNotFoundException, JsonException

**Checkpoint:** Dashboard loads without errors; component hierarchy visible in browser DevTools

**Risks Addressed:** R1 (JSON failure), R6 (missing data.json)

### Phase 2 (Days 3-5): Visualization, Styling, Print Optimization

**Deliverables:**
- MilestoneTimeline.razor (custom HTML/CSS, CSS Grid)
- ProgressSummary.razor (ChartJs Blazor bar chart)
- StatusCards.razor (three-column flexbox layout)
- MetricsPanel.razor (KPI cards, aggregate statistics)
- app.css (~200 lines, fixed 1200px layout, @media print)
- Print testing: F12 DevTools, Windows 10/11 print dialog, PDF export
- Visual comparison: screenshot vs. OriginalDesignConcept.html (95%+ match)

**Checkpoint:** Pixel-perfect dashboard rendering; print preview matches design; executives can screenshot for PowerPoint

**Risks Addressed:** R3 (print inconsistency)

### Phase 3 (Days 6-8): Deployment, Validation, Documentation

**Deliverables:**
- Self-contained .exe: `dotnet publish -c Release -r win-x64 --self-contained`
- Windows 10/11 validation: test .exe without .NET pre-install
- User documentation: "How to Run Dashboard", "How to Export to PowerPoint"
- Optional: Wix installer (.msi) for professional deployment
- Optional: Code-signing certificate to avoid SmartScreen warning

**Checkpoint:** Shippable .exe; documentation complete; tested on clean Windows

**Risks Addressed:** R4 (SmartScreen), R5 (version monitoring), R8 (multiple instances)

---

## Monitoring & Observability

### Logging

**Configuration:**
- Console output only (no file I/O in MVP)
- Log levels: Information (default), Warning, Error, Critical
- Structured logging enabled for future centralization

**Key Log Messages:**
```
[INFO] Application starting...
[INFO] ✓ Loaded Q1 2026 Platform Release (3 milestones, 15 tasks)
[INFO] Kestrel server listening on http://localhost:5000
[WARN] (optional alerts for slow operations >1000ms)
[ERROR] JSON syntax error: Expected closing brace at line 5
[CRITICAL] ❌ data.json not found; exiting
```

### Performance Metrics (Browser DevTools)

- Page Load Time: <2s target
- Component Render: <10ms target (measure via React DevTools equivalent or Blazor profiling)
- Cold Start: <5s target (Kestrel init + browser launch)
- Network: minimal JS (~50KB), CSS (~20KB), HTML (~30KB)

### Alerts (for Phase 3+)

- If JSON deserialization takes >1000ms, log warning
- If Kestrel startup takes >5s, log warning
- If page load takes >2s, log warning

---

## Success Criteria

**User Stories:** All 6 stories (US-001 through US-006) pass acceptance criteria ✓

**Visual Fidelity:** Dashboard screenshot matches OriginalDesignConcept.html (95%+ match) ✓

**Deployment:** Self-contained .exe runs on clean Windows 10/11 without .NET pre-install ✓

**Performance:** Page load <2s, render <10ms, cold start <5s ✓

**Code Quality:** <500 LOC total, single project, unit tests pass ✓

**Print/Screenshot:** PowerPoint workflow completes in <5 minutes without manual adjustments ✓

**JSON Validation:** Malformed data.json displays error (no crash); valid data loads correctly ✓

**Documentation:** User guide covers "How to Run" and "How to Export to PowerPoint" ✓