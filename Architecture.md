# Architecture

## Overview & Goals

This document defines the complete system architecture for the Privacy Automation Release Roadmap Dashboard—a single-page executive reporting application built on C# .NET 8 with Blazor Server. The system visualizes project milestones and monthly execution status across four categorical statuses (Shipped, In Progress, Carryover, Blockers) for presentation-ready screenshots and stakeholder communication.

### Primary Goals

1. **Single-source-of-truth project visibility**: Centralized, clean dashboard eliminating manual report aggregation.
2. **Screenshot-friendly design**: Full-page render at 1920x1080 without scrolling, suitable for PowerPoint decks without manual image editing.
3. **Minimal deployment friction**: File-based JSON configuration (no database, no authentication, no cloud infrastructure).
4. **Rapid iteration support**: Simple technology stack enabling quick updates to data and visual design.
5. **Zero external dependencies**: Leverage .NET 8.0 built-ins (System.Text.Json) for maximum simplicity and maintainability.

### Non-Goals

- Multi-user authentication or role-based access control (internal tool only)
- Real-time data synchronization or WebSocket-driven updates (monthly/quarterly refresh cycle)
- Cloud deployment or scalability beyond 100 concurrent users
- Interactive drill-down, hover tooltips, or complex analytics features (read-only dashboard)
- Mobile or responsive design (desktop 1920x1080 target only)
- Multi-project support (single project per instance)

---

## System Components

### 1. Web Application Host (Blazor Server)

**Responsibility**: Host Blazor Server application on ASP.NET Core .NET 8.0 runtime. Serve HTML, manage component lifecycle, handle WebSocket connections for Blazor interactivity.

**Key Responsibilities**:
- Listen on HTTP/HTTPS (default localhost:5000 or configurable port)
- Manage Blazor circuit lifecycle and component rendering
- Serve static assets (CSS, favicon, app.js)
- Handle graceful error recovery if WebSocket disconnects
- Provide dependency injection container for services

**Interface**:
```
HTTP GET / → Render DashboardPage.razor (HTML response)
WebSocket /blazor → Blazor runtime connection
HTTP GET /css/app.css → Static stylesheet
```

**Dependencies**:
- .NET 8.0 runtime
- ASP.NET Core framework (built-in)
- Blazor Server runtime

**Data Flow**:
1. User opens browser to `http://localhost:5000`
2. HTTP request routed to default `DashboardPage.razor` page
3. Blazor Server renders HTML response with initial component state
4. Browser loads response; WebSocket connection established for interactivity
5. JavaScript runtime on client manages DOM updates via Blazor protocol

---

### 2. DashboardPage.razor (Root Page Component)

**Responsibility**: Orchestrate page-level layout, data fetching, and render header, timeline, heatmap, and legend components.

**Key Responsibilities**:
- Inject `ReportDataService` dependency
- Call `OnInitializedAsync()` to fetch project report data on page load
- Handle JSON parsing errors and display user-friendly error messages
- Render page header with title, subtitle, legend
- Render timeline section with milestone visualization
- Render heatmap grid with status categories and monthly data
- Manage print/export button (future enhancement)

**Interface**:
```csharp
public class DashboardPage : ComponentBase
{
    [Inject] ReportDataService ReportService { get; set; }
    
    ProjectReport Report { get; set; }
    string ErrorMessage { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            Report = ReportService.GetReport();
        }
        catch (JsonException ex)
        {
            ErrorMessage = $"Error parsing data.json: {ex.Message}";
        }
        catch (FileNotFoundException ex)
        {
            ErrorMessage = $"Error: data.json not found. {ex.Message}";
        }
    }
}
```

**Renders**:
- Header section (title, subtitle, legend)
- TimelineComponent with milestones
- HeatmapComponent with status grid
- Error alert if data loading fails

**Dependencies**:
- ReportDataService (injected)
- TimelineComponent (child)
- HeatmapComponent (child)
- Blazor framework

**Data Received**:
```csharp
public class ProjectReport
{
    public string ProjectName { get; set; }
    public string ProjectContext { get; set; }
    public List<string> TimePeriods { get; set; }
    public List<Milestone> Milestones { get; set; }
    public Dictionary<string, StatusCategory> StatusCategories { get; set; }
}
```

---

### 3. TimelineComponent.razor (Milestone Gantt Visualization)

**Responsibility**: Render SVG timeline visualization showing milestone tracks, checkpoints, and date markers. Generate month dividers, milestone markers (PoC/Production diamonds), checkpoint circles, and "NOW" indicator.

**Key Responsibilities**:
- Accept milestones and time periods as parameters
- Dynamically generate SVG markup based on milestone data
- Render month dividers at regular intervals (260px per month in SVG viewBox)
- Position milestone markers (yellow PoC diamonds, green Production diamonds)
- Render checkpoint circles (7px for start, 5px for intermediate, 4px for end)
- Position "NOW" indicator (red dashed vertical line) at current month
- Render month labels (Jan, Feb, Mar, etc.) above timeline
- Use `@((MarkupString)svgContent)` to render dynamic SVG
- Apply drop shadow filter to milestone diamonds via SVG `<filter>`

**Interface**:
```csharp
public partial class TimelineComponent : ComponentBase
{
    [Parameter]
    public List<Milestone> Milestones { get; set; }
    
    [Parameter]
    public List<string> TimePeriods { get; set; }
    
    [Parameter]
    public int CurrentMonthIndex { get; set; } = 3; // April (0-indexed)
    
    private MarkupString SvgContent { get; set; }
    
    protected override void OnInitialized()
    {
        SvgContent = GenerateSvgTimeline();
    }
    
    private MarkupString GenerateSvgTimeline()
    {
        // Build SVG string with month dividers, milestones, checkpoints, "NOW" line
        // Return as MarkupString for safe rendering
    }
}
```

**Renders**: Inline SVG with:
- Month dividers (vertical gray lines)
- Month labels (Jan, Feb, Mar, Apr, May, Jun)
- Milestone tracks (horizontal colored lines per milestone)
- Checkpoint markers (circles at key dates)
- Milestone markers (colored diamond polygons)
- "NOW" indicator (red dashed line)
- Date labels above/below markers

**Dependencies**:
- No external libraries; pure SVG generation in C#
- String manipulation for coordinate calculation

**Data Received**:
```csharp
public class Milestone
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public int StartMonth { get; set; }
    public int EndMonth { get; set; }
    public string Type { get; set; } // "checkpoint", "poc", "production"
    public List<Checkpoint> Checkpoints { get; set; }
}

public class Checkpoint
{
    public string Name { get; set; }
    public int MonthIndex { get; set; }
    public string Date { get; set; }
    public string MarkerType { get; set; } // "start", "intermediate", "end"
}
```

**SVG Coordinate System**:
- viewBox: 1560 × 185 (width × height)
- Month spacing: 260px per month
- Milestone y-positions: M1=42, M2=98, M3=154 (evenly distributed)
- "NOW" x-position: 823px (April month at 780 + half-month offset)

---

### 4. HeatmapComponent.razor (Status Grid)

**Responsibility**: Render CSS Grid displaying 4 status rows (Shipped, In Progress, Carryover, Blockers) × 4+ month columns, with color-coded cells, item names, and dot indicators.

**Key Responsibilities**:
- Accept status categories, items, and time periods as parameters
- Build 2D grid structure: fixed 160px left column + proportional month columns
- Render row headers with category names and background colors
- Render column headers with month names (April highlighted in yellow)
- Populate data cells with item names and colored dot indicators
- Display "-" (dash) in empty cells
- Apply darker background to April column (current month)
- Use scoped CSS for styling (component-level CSS isolation)

**Interface**:
```csharp
public partial class HeatmapComponent : ComponentBase
{
    [Parameter]
    public Dictionary<string, StatusCategory> StatusCategories { get; set; }
    
    [Parameter]
    public List<string> TimePeriods { get; set; }
    
    [Parameter]
    public int CurrentMonthIndex { get; set; } = 2; // March (0-indexed)
    
    private const string CornerCellText = "Status";
    
    private string GetCellClass(string categoryName, int monthIndex)
    {
        var baseClass = $"{categoryName.ToLower()}-cell";
        if (monthIndex == CurrentMonthIndex)
            return $"{baseClass} apr";
        return baseClass;
    }
}
```

**Renders**: HTML using CSS Grid:
```html
<div class="hm-grid">
  <!-- Corner cell (Status label) -->
  <div class="hm-corner">Status</div>
  
  <!-- Column headers (months) -->
  @foreach (var month in TimePeriods)
  {
    <div class="hm-col-hdr @(month == "April" ? "apr-hdr" : "")">@month</div>
  }
  
  <!-- Row: Shipped -->
  <div class="hm-row-hdr ship-hdr">✓ Shipped</div>
  @foreach (var month in TimePeriods)
  {
    <div class="hm-cell ship-cell @(month == "April" ? "apr" : "")">
      @if (ShippedItems.TryGetValue(month, out var items) && items.Count > 0)
      {
        @foreach (var item in items)
        {
          <div class="it">@item.Name</div>
        }
      }
      else
      {
        <span style="color:#AAA;">-</span>
      }
    </div>
  }
  
  <!-- Similar rows for In Progress, Carryover, Blockers -->
</div>
```

**CSS Grid Layout**:
```css
.hm-grid {
    display: grid;
    grid-template-columns: 160px repeat(4, 1fr);
    grid-template-rows: 36px repeat(4, 1fr);
    border: 1px solid #E0E0E0;
    flex: 1;
    min-height: 0;
}
```

**Dependencies**:
- Scoped CSS (HeatmapComponent.razor.css)
- Bootstrap not needed; pure CSS Grid

**Data Received**:
```csharp
public class StatusCategory
{
    public string Name { get; set; }
    public string Color { get; set; } // e.g., "#1B7A28" for green
    public string BackgroundColor { get; set; } // e.g., "#E8F5E9"
    public string DotColor { get; set; } // e.g., "#34A853" for dot marker
    public List<StatusItem> Items { get; set; }
}

public class StatusItem
{
    public string Name { get; set; }
    public bool[] Periods { get; set; } // Per-month boolean or string marker
}
```

---

### 5. ReportDataService (Business Logic / Data Layer)

**Responsibility**: Load, parse, and serve project report data from `data.json`. Handle JSON deserialization, error recovery, and caching.

**Key Responsibilities**:
- Load `data.json` from filesystem on first access (singleton pattern)
- Parse JSON into strongly-typed `ProjectReport` domain objects
- Handle JSON parsing errors gracefully (return error message to UI)
- Handle missing file gracefully (file not found)
- Cache loaded data in memory (no reload until app restart)
- Provide simple GetReport() synchronous method (no async needed for small JSON)

**Interface**:
```csharp
public class ReportDataService
{
    private ProjectReport _cachedReport;
    private readonly ILogger<ReportDataService> _logger;
    
    public ReportDataService(ILogger<ReportDataService> logger)
    {
        _logger = logger;
    }
    
    public ProjectReport GetReport()
    {
        if (_cachedReport != null)
            return _cachedReport;
            
        try
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "data.json");
            var json = File.ReadAllText(filePath);
            _cachedReport = JsonSerializer.Deserialize<ProjectReport>(json);
            _logger.LogInformation("Report data loaded successfully");
            return _cachedReport;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError($"data.json not found: {ex.Message}");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"JSON parsing error: {ex.Message}");
            throw;
        }
    }
}
```

**Dependencies**:
- System.Text.Json (built-in to .NET 8.0)
- ILogger (ASP.NET Core built-in)
- File I/O (System.IO)

**Data Returned**:
```csharp
public class ProjectReport
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; }
    
    [JsonPropertyName("projectContext")]
    public string ProjectContext { get; set; }
    
    [JsonPropertyName("timePeriods")]
    public List<string> TimePeriods { get; set; }
    
    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; }
    
    [JsonPropertyName("statusCategories")]
    public Dictionary<string, StatusCategory> StatusCategories { get; set; }
}
```

**Error Handling**:
- FileNotFoundException: Catch and log; return error message to UI
- JsonException: Catch and log; return error message to UI
- Invalid data structure: Validation in deserializer or catch at usage point

---

### 6. Static Assets (CSS, Favicon)

**Responsibility**: Serve CSS stylesheets, favicon, and other static assets.

**Key Files**:
- `wwwroot/css/app.css` – Global styles (colors, fonts, spacing)
- `wwwroot/css/site.css` – Deprecated by Blazor (optional cleanup)
- `HeatmapComponent.razor.css` – Scoped CSS for heatmap grid
- `TimelineComponent.razor.css` – Scoped CSS for timeline (if needed)
- `wwwroot/favicon.ico` – Browser tab icon

**Layout**:
```
wwwroot/
├── css/
│   ├── app.css                 (global styles)
│   └── bootstrap.min.css       (removed if unused)
├── images/
│   └── favicon.svg             (optional)
├── js/
│   └── (Blazor auto-injected)
└── index.html                  (root page template)
```

**Dependencies**:
- None (static files served by ASP.NET Core)

---

## Component Interactions

### Data Flow Diagram

```
User Opens Browser
    ↓
HTTP GET /
    ↓
DashboardPage.razor (initial render)
    ↓
OnInitializedAsync() → Inject ReportDataService
    ↓
ReportDataService.GetReport()
    ↓
Load data.json from filesystem
    ↓
JsonSerializer.Deserialize<ProjectReport>()
    ↓
Return ProjectReport object
    ↓
DashboardPage renders child components:
    ├─ TimelineComponent (pass Milestones, TimePeriods)
    │   ├─ GenerateSvgTimeline() (calculate coordinates)
    │   └─ Render @((MarkupString)svgContent)
    │
    ├─ HeatmapComponent (pass StatusCategories, TimePeriods)
    │   ├─ Build CSS Grid structure
    │   └─ Render cells with item names and dots
    │
    └─ Header + Legend (render static sections)
    
Browser receives HTML response
    ↓
Blazor WebSocket connection established
    ↓
Dashboard displayed on screen (no interactivity needed for MVP)
    ↓
User takes screenshot → Send to PowerPoint
```

### Component Lifecycle

1. **Page Load**:
   - Browser requests `GET /`
   - ASP.NET Core routes to default `DashboardPage.razor`
   - Component tree initialized

2. **OnInitializedAsync** (DashboardPage):
   - `ReportDataService.GetReport()` called
   - JSON parsed from disk
   - `Report` property set with deserialized data
   - Component state-changed notification triggers render

3. **Render** (DashboardPage):
   - HTML template evaluated
   - TimelineComponent receives `Milestones` and `TimePeriods` parameters
   - HeatmapComponent receives `StatusCategories` and `TimePeriods` parameters
   - Both child components render

4. **Render** (TimelineComponent):
   - `OnInitialized()` lifecycle hook fires
   - `GenerateSvgTimeline()` builds SVG string based on milestone data
   - SVG rendered via `@((MarkupString)svgContent)`

5. **Render** (HeatmapComponent):
   - `OnParametersSet()` fires (if parameters changed)
   - CSS Grid structure built from status categories
   - Data cells populated from StatusItem arrays
   - Scoped CSS applied

6. **Display**:
   - HTML + SVG + CSS sent to browser
   - Browser renders page
   - Blazor WebSocket connection maintains interactivity
   - User sees dashboard

### Error Handling Flow

```
DashboardPage.OnInitializedAsync()
    ↓
ReportDataService.GetReport() throws exception
    ↓
Catch in DashboardPage:
    ├─ FileNotFoundException
    │   └─ Set ErrorMessage = "data.json not found..."
    │
    ├─ JsonException
    │   └─ Set ErrorMessage = "Invalid JSON syntax..."
    │
    └─ Other exception
        └─ Set ErrorMessage = "Unexpected error..."
        
StateHasChanged() triggered
    ↓
Conditional render in template:
    @if (!string.IsNullOrEmpty(ErrorMessage))
    {
        <div class="error-alert">@ErrorMessage</div>
    }
    else
    {
        <!-- Render normal dashboard -->
    }
```

---

## Data Model

### Core Entities

#### ProjectReport (Root Aggregate)

```csharp
public class ProjectReport
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; }
    
    [JsonPropertyName("projectContext")]
    public string ProjectContext { get; set; }
    
    [JsonPropertyName("timePeriods")]
    public List<string> TimePeriods { get; set; }
    
    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; }
    
    [JsonPropertyName("statusCategories")]
    public Dictionary<string, StatusCategory> StatusCategories { get; set; }
}
```

**Semantics**:
- `ProjectName`: Display name (e.g., "Privacy Automation Release Roadmap")
- `ProjectContext`: Subtitle or context line (e.g., "Trusted Platform – Privacy Automation Workstream – April 2026")
- `TimePeriods`: Array of month/quarter names in chronological order (e.g., ["January", "February", "March", "April"])
- `Milestones`: List of major project gates and releases (M1, M2, M3)
- `StatusCategories`: Map of status category names to category objects (keys: "Shipped", "InProgress", "Carryover", "Blockers")

---

#### Milestone

```csharp
public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("color")]
    public string Color { get; set; } // Hex color code (e.g., "#0078D4")
    
    [JsonPropertyName("startMonth")]
    public int StartMonth { get; set; } // 0-indexed month in TimePeriods array
    
    [JsonPropertyName("endMonth")]
    public int EndMonth { get; set; } // 0-indexed
    
    [JsonPropertyName("milestoneType")]
    public string MilestoneType { get; set; } // "poc" or "production"
    
    [JsonPropertyName("checkpoints")]
    public List<Checkpoint> Checkpoints { get; set; } // Key dates on timeline
}
```

**Semantics**:
- `Id`: Unique identifier (e.g., "m1", "m2", "m3")
- `Name`: Display name (e.g., "Chatbot & MS Role")
- `Description`: Brief scope or context (e.g., "Initial chatbot development and Microsoft role integration")
- `Color`: Hex code for milestone line on timeline (e.g., "#0078D4" for blue, "#00897B" for teal)
- `StartMonth`, `EndMonth`: Span of milestone on timeline (indices into TimePeriods)
- `MilestoneType`: "poc" (yellow diamond) or "production" (green diamond)
- `Checkpoints`: Array of intermediate dates and gates

---

#### Checkpoint

```csharp
public class Checkpoint
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("monthIndex")]
    public int MonthIndex { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; } // ISO 8601 or display format (e.g., "Jan 12", "Mar 26")
    
    [JsonPropertyName("markerType")]
    public string MarkerType { get; set; } // "start", "poc", "production"
}
```

**Semantics**:
- `Name`: Human-readable checkpoint name (e.g., "Design complete", "Beta launch")
- `MonthIndex`: Position on timeline (index into TimePeriods)
- `Date`: Display date (e.g., "Jan 12", "Mar 26", "Apr TBD")
- `MarkerType`: Visual representation (start = large white circle with colored stroke, poc = yellow diamond, production = green diamond)

---

#### StatusCategory

```csharp
public class StatusCategory
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("color")]
    public string Color { get; set; } // Hex code for row header text
    
    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } // Hex code for row header background
    
    [JsonPropertyName("cellColor")]
    public string CellColor { get; set; } // Light background for data cells
    
    [JsonPropertyName("currentMonthColor")]
    public string CurrentMonthColor { get; set; } // Darker shade for April/current month
    
    [JsonPropertyName("dotColor")]
    public string DotColor { get; set; } // Color for item bullet dots
    
    [JsonPropertyName("items")]
    public List<StatusItem> Items { get; set; }
}
```

**Example (Shipped category)**:
```json
{
  "name": "Shipped",
  "color": "#1B7A28",
  "backgroundColor": "#E8F5E9",
  "cellColor": "#F0FBF0",
  "currentMonthColor": "#D8F2DA",
  "dotColor": "#34A853",
  "items": [
    { "name": "Feature A", "periods": ["x", "x", "", "x"] },
    { "name": "Feature B", "periods": ["", "x", "x", "x"] }
  ]
}
```

**Semantics**:
- `Name`: Display name in row header (e.g., "✓ Shipped", " In Progress", "? Carryover", "? Blockers")
- `Color`: Text color for row header (e.g., "#1B7A28" for dark green)
- `BackgroundColor`: Background color for row header (e.g., "#E8F5E9" for light green)
- `CellColor`: Background for data cells in non-current months (e.g., "#F0FBF0" for shipped)
- `CurrentMonthColor`: Darker background for April/current month cells (e.g., "#D8F2DA")
- `DotColor`: Color of bullet dots in cells (e.g., "#34A853" for green)
- `Items`: List of status items belonging to this category

---

#### StatusItem

```csharp
public class StatusItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("periods")]
    public string[] Periods { get; set; } // Per-month markers
}
```

**Semantics**:
- `Name`: Item display name (e.g., "Feature A", "API Integration", "Security Review")
- `Periods`: Array with one entry per month in TimePeriods
  - "x" or truthy value = item present in this month
  - "" or falsy value = item not present
  - Array length must match TimePeriods.Count

**Example**:
```json
{
  "name": "Feature A",
  "periods": ["x", "x", "", "x"]
}
```
This item shipped in January, February, and April, but not in March.

---

### Data Storage

**Location**: `data.json` in application root directory (next to executable or wwwroot/)

**Format**: Valid JSON 5.0 compatible with System.Text.Json

**Example Structure**:
```json
{
  "projectName": "Privacy Automation Release Roadmap",
  "projectContext": "Trusted Platform – Privacy Automation Workstream – April 2026",
  "timePeriods": ["January", "February", "March", "April", "May", "June"],
  "milestones": [
    {
      "id": "m1",
      "name": "Chatbot & MS Role",
      "description": "Initial chatbot development and Microsoft role integration",
      "color": "#0078D4",
      "startMonth": 0,
      "endMonth": 1,
      "milestoneType": "poc",
      "checkpoints": [
        {
          "name": "Start",
          "monthIndex": 0,
          "date": "Jan 12",
          "markerType": "start"
        },
        {
          "name": "PoC Gate",
          "monthIndex": 2,
          "date": "Mar 26",
          "markerType": "poc"
        },
        {
          "name": "Production Release",
          "monthIndex": 3,
          "date": "Apr TBD",
          "markerType": "production"
        }
      ]
    }
  ],
  "statusCategories": {
    "Shipped": {
      "name": "Shipped",
      "color": "#1B7A28",
      "backgroundColor": "#E8F5E9",
      "cellColor": "#F0FBF0",
      "currentMonthColor": "#D8F2DA",
      "dotColor": "#34A853",
      "items": [
        { "name": "Core API", "periods": ["x", "x", "", "x"] },
        { "name": "Auth Module", "periods": ["x", "", "x", "x"] }
      ]
    },
    "InProgress": {
      "name": "In Progress",
      "color": "#1565C0",
      "backgroundColor": "#E3F2FD",
      "cellColor": "#EEF4FE",
      "currentMonthColor": "#DAE8FB",
      "dotColor": "#0078D4",
      "items": [
        { "name": "Dashboard UI", "periods": ["", "x", "x", "x"] },
        { "name": "Reporting Engine", "periods": ["", "", "x", "x"] }
      ]
    },
    "Carryover": {
      "name": "Carryover",
      "color": "#B45309",
      "backgroundColor": "#FFF8E1",
      "cellColor": "#FFFDE7",
      "currentMonthColor": "#FFF0B0",
      "dotColor": "#F4B400",
      "items": [
        { "name": "Mobile Support", "periods": ["", "", "x", "x"] }
      ]
    },
    "Blockers": {
      "name": "Blockers",
      "color": "#991B1B",
      "backgroundColor": "#FEF2F2",
      "cellColor": "#FFF5F5",
      "currentMonthColor": "#FFE4E4",
      "dotColor": "#EA4335",
      "items": [
        { "name": "Vendor API delay", "periods": ["", "", "x", "x"] }
      ]
    }
  }
}
```

---

## API Contracts

### Page Rendering (HTTP)

#### GET / (Root Page)

**Request**:
```
GET / HTTP/1.1
Host: localhost:5000
User-Agent: Mozilla/5.0
```

**Response**:
```
HTTP/1.1 200 OK
Content-Type: text/html; charset=utf-8
Transfer-Encoding: chunked

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>Privacy Automation Release Roadmap</title>
    <base href="/" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <script src="_framework/blazor.server.js"></script>
</head>
<body>
    <div id="app">Loading...</div>
</body>
</html>
```

The HTML response triggers Blazor component rendering server-side. DashboardPage.razor loads, calls ReportDataService, and renders full HTML with header, timeline, heatmap.

---

### Blazor Server WebSocket Protocol

#### WebSocket Connection: /blazor

**Purpose**: Maintain stateful component rendering and interactivity between client and server.

**Flow**:
1. Browser establishes WebSocket to `ws://localhost:5000/blazor` (or `wss://` over HTTPS)
2. Server maintains Blazor circuit (component state)
3. Client sends interaction events; server updates component state and sends DOM patches
4. For MVP (read-only dashboard), WebSocket carries no user interaction events—only keep-alive frames

**Auto-reconnect**: If connection drops, Blazor automatically attempts reconnection with exponential backoff. User sees "reconnecting..." message briefly. Acceptable for internal tool.

---

### Error Response Contract

#### JSON Parsing Error

**Scenario**: User opens dashboard; data.json contains invalid JSON syntax (e.g., trailing comma, unclosed brace).

**Response in DashboardPage**:
```html
<div class="error-alert" style="padding:20px; background:#FFF5F5; border:1px solid #EA4335; color:#991B1B;">
  <strong>Error loading project data:</strong> 
  Invalid JSON in data.json. Please check file syntax.
  <!-- Details may include JsonException message if in Debug mode -->
</div>
```

**HTTP Status**: Still 200 OK (page loads; error rendered in component)

---

#### File Not Found Error

**Scenario**: data.json is missing from application root.

**Response in DashboardPage**:
```html
<div class="error-alert" style="padding:20px; background:#FFF5F5; border:1px solid #EA4335; color:#991B1B;">
  <strong>Error loading project data:</strong> 
  data.json not found in application root. Please ensure data.json exists in: [AppContext.BaseDirectory]
</div>
```

**HTTP Status**: Still 200 OK

---

### Static Asset Requests

#### GET /css/app.css

**Request**:
```
GET /css/app.css HTTP/1.1
Host: localhost:5000
```

**Response**:
```
HTTP/1.1 200 OK
Content-Type: text/css
Cache-Control: public, max-age=31536000

/* Global styles */
* { margin: 0; padding: 0; box-sizing: border-box; }
body { font-family: Segoe UI, Arial, sans-serif; color: #111; }
/* ... rest of CSS ... */
```

---

### Print/Export Flow (Future Enhancement)

#### POST /api/export/png (Not in MVP)

**Request**:
```
POST /api/export/png HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{}
```

**Response**:
```
HTTP/1.1 200 OK
Content-Type: image/png

[Binary PNG image data]
```

**Note**: Out of MVP scope. MVP uses browser's native `print` function and print-to-PDF.

---

## Infrastructure Requirements

### Hosting Environment

**Type**: On-premises or local machine (no cloud)

**Operating System**: Windows, macOS, or Linux (any OS supporting .NET 8.0)

**Runtime**: .NET 8.0 runtime (LTS, supported until 2026)

**Hardware Minimum**:
- CPU: 1 core (x86_64 or ARM64)
- RAM: 256 MB (typical usage ~100-150 MB)
- Disk: 500 MB (application + .NET runtime)
- Network: LAN or localhost only

**Ports**:
- Default HTTP: 5000
- Default HTTPS: 5001
- Configurable in `appsettings.json` or environment variables

---

### Project Structure

```
MyProject.Reporting/
├── MyProject.Reporting.csproj       (MSBuild project file)
├── Program.cs                        (Blazor Server startup)
├── appsettings.json                  (configuration)
├── appsettings.Development.json      (dev overrides)
├── data.json                         (project data)
├── Pages/
│   ├── DashboardPage.razor          (root page component)
│   └── _Host.cshtml                 (Blazor Server host page)
├── Components/
│   ├── TimelineComponent.razor      (SVG timeline)
│   ├── TimelineComponent.razor.css  (scoped styles)
│   ├── HeatmapComponent.razor       (CSS grid)
│   ├── HeatmapComponent.razor.css   (scoped styles)
│   ├── HeaderComponent.razor        (page header)
│   └── ErrorAlert.razor             (error display)
├── Services/
│   └── ReportDataService.cs         (data loading)
├── Models/
│   ├── ProjectReport.cs             (DTO)
│   ├── Milestone.cs                 (DTO)
│   ├── StatusCategory.cs            (DTO)
│   ├── StatusItem.cs                (DTO)
│   └── Checkpoint.cs                (DTO)
├── wwwroot/
│   ├── index.html                   (root HTML)
│   ├── css/
│   │   ├── app.css                  (global styles)
│   │   └── bootstrap.min.css        (optional, if using Bootstrap)
│   ├── js/
│   │   └── (auto-generated by Blazor)
│   └── favicon.ico
├── Properties/
│   └── launchSettings.json          (development run config)
└── obj/ & bin/                       (build artifacts)
```

---

### Configuration

#### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### appsettings.Development.json

```json
{
  "DetailedErrors": true,
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Debug"
    }
  }
}
```

#### Program.cs Startup Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Blazor Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register custom services
builder.Services.AddSingleton<ReportDataService>();

// Configure logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

---

### Deployment

#### Local Development

```bash
cd C:\Git\AgentSquad\src\AgentSquad.Runner
dotnet watch run
```

App runs on `https://localhost:7123` (Blazor Server default HTTPS port).

#### Release Build

```bash
dotnet publish -c Release -o ./publish
```

Outputs optimized binaries to `./publish` folder. Ready for deployment.

#### Run Published Application

```bash
dotnet publish/MyProject.Reporting.dll
```

App listens on configured port (default 5000 HTTP, 5001 HTTPS).

---

### Data Management

**File Location**: `data.json` in application root (same directory as executable or wwwroot/)

**Refresh Strategy**:
- MVP: Manual refresh via application restart
- Future: File watcher + auto-reload (not implemented for MVP)

**Backup & Version Control**:
- Commit `data.json` to Git for audit trail
- Keep backups of previous versions in Git history
- Product Manager updates via Git commit (provides traceability)

---

### Logging & Monitoring

**Minimum Logging** (MVP):
- Console output: Application startup, data loading, errors
- No persistent log file required
- No centralized logging service needed

**Optional Enhancements**:
- Serilog for structured logging to file
- Application Insights for Azure monitoring (post-MVP if needed)

---

### CI/CD (Out of MVP Scope)

**Manual Deployment Process**:
1. Commit `data.json` updates to Git
2. Run `dotnet publish -c Release`
3. Deploy `publish/` folder to target machine
4. Run `dotnet MyProject.Reporting.dll`

**Future CI/CD** (not required for MVP):
- GitHub Actions to build and test on push
- Automated deployment to staging environment
- Release notes generation

---

## Technology Stack Decisions

### Blazor Server on .NET 8.0

**Decision**: Use Blazor Server (not Blazor WASM or ASP.NET Core API + SPA)

**Justification**:
- **Server-side rendering**: HTML/CSS generated server-side, ensuring consistent rendering for screenshots
- **Simple component model**: C# components map directly to visual sections (header, timeline, heatmap)
- **No JavaScript complexity**: Timeline SVG and heatmap CSS Grid generated in C#, no frontend build tools needed
- **Built-in dependency injection**: Services registered in Program.cs; components inject dependencies
- **Instant time-to-interactive**: No SPA hydration or JavaScript bundle parsing
- **Screenshot-friendly**: Full page renders server-side; screenshots are pixel-accurate without client-side rendering variance

**Alternatives Considered**:
- **Blazor WASM**: Adds complexity (bundle size, .wasm runtime); no benefit for read-only dashboard
- **ASP.NET Core API + React/Vue**: Adds JavaScript dependency; increases development/testing burden
- **MVC Razor Pages**: Less componentized; harder to manage timeline/heatmap rendering
- **Static HTML generator**: No data binding; requires manual HTML generation for each update

---

### System.Text.Json for JSON Deserialization

**Decision**: Use System.Text.Json (built-in to .NET 8.0)

**Justification**:
- **No external dependencies**: JSON deserialization built into .NET 8.0 framework
- **Performance**: Faster than Newtonsoft.Json for simple data structures
- **Simplicity**: Minimal configuration; attribute-based property mapping via `[JsonPropertyName]`
- **Works with records or classes**: Flexible type definition

**Alternatives Considered**:
- **Newtonsoft.Json (JSON.NET)**: Mature but adds external dependency (violates zero-dependency goal)
- **Manual JSON parsing**: Error-prone; use built-in deserializer instead

---

### CSS Grid + Flexbox for Layout

**Decision**: Use native CSS Grid and Flexbox (no CSS framework)

**Justification**:
- **Grid dimensions hardcoded in design**: 160px left column + repeat(N, 1fr) for months matches OriginalDesignConcept.html exactly
- **No framework overhead**: Bootstrap, Tailwind add unnecessary bloat for simple 3-section layout
- **Scoped CSS**: Component-level .razor.css files isolate styles, avoiding global conflicts
- **Full control**: Easy to match design pixel-for-pixel without framework abstractions

**Alternatives Considered**:
- **Bootstrap 5**: Adds ~200 KB CSS; overkill for simple dashboard
- **Tailwind CSS**: Adds build-tool complexity (requires PostCSS); simple layout doesn't benefit
- **Custom CSS**: Already chosen; no framework layer

---

### Inline SVG Generation in C#

**Decision**: Generate SVG markup as strings in C#, render via `@((MarkupString)svgContent)`

**Justification**:
- **No charting library overhead**: D3.js, Chart.js, Plotly add significant bundle size
- **Simple timeline logic**: Month dividers, milestone markers, checkpoints—easy to calculate in C#
- **Direct integration with Blazor**: String generation + MarkupString rendering is native Blazor pattern
- **Full visual control**: No abstraction between C# data and SVG output

**Implementation**:
```csharp
private MarkupString GenerateSvgTimeline()
{
    var svg = new StringBuilder();
    svg.Append("<svg xmlns='http://www.w3.org/2000/svg' width='1560' height='185'>");
    
    // Month dividers
    for (int i = 0; i < TimePeriods.Count; i++)
    {
        int x = i * 260;
        svg.Append($"<line x1='{x}' y1='0' x2='{x}' y2='185' stroke='#bbb' />");
        svg.Append($"<text x='{x + 5}' y='14'>{TimePeriods[i]}</text>");
    }
    
    // Milestones (similar coordinate calculations)
    
    svg.Append("</svg>");
    return (MarkupString)svg.ToString();
}
```

**Alternatives Considered**:
- **D3.js**: Adds JavaScript framework; conflicting with server-side rendering goal
- **Chart.js**: Designed for charts, not timelines; requires DOM element and JavaScript
- **Plotly.js**: Heavy library; overkill for simple milestone timeline
- **Static SVG file**: No data binding; requires separate SVG per project

---

### File-Based JSON Configuration

**Decision**: Store all project data in single `data.json` file, loaded on application startup

**Justification**:
- **Simplicity**: No database setup, migrations, or SQL
- **Version controllable**: `data.json` in Git provides audit trail of changes
- **Manual update process**: Product Manager edits JSON, commits, app restarts—low friction
- **Read-only dashboard**: No concurrent writes or multi-user conflicts

**Alternative Data Sources Considered**:
- **SQL Server / SQLite**: Adds database setup burden; overkill for static reporting
- **CSV file**: Less structured; harder to represent nested milestones/checkpoints
- **Excel file**: Requires EPPlus or similar library; adds dependency
- **REST API**: Adds network complexity; violates "local-only" constraint
- **Database with ORM**: Adds Entity Framework Core dependency; unnecessary for read-only data

---

### No External NuGet Dependencies

**Decision**: Minimize external package usage; rely on .NET 8.0 built-ins

**Justification**:
- **Reduced attack surface**: Fewer external packages = fewer vulnerabilities
- **Simplified deployment**: Single executable + runtime; no package restore on target machine
- **Faster builds**: No transitive dependency resolution
- **Easier maintenance**: Team controls all code; no upstream breaking changes

**Packages Allowed** (minimal):
- System.Text.Json (built-in)
- Microsoft.Extensions.* (built-in)
- Blazor Server runtime (built-in)

**Packages Avoided** (deferred post-MVP):
- Serilog (structured logging; optional)
- xUnit / bUnit (testing; optional)
- Automapper (entity mapping; not needed)

---

## Security Considerations

### Authentication & Authorization

**MVP Scope**: No authentication required.

**Justification**:
- Internal tool (employees only)
- No sensitive PII (fictional project data)
- Local deployment on secure network
- Read-only dashboard (no data modification risk)

**Future Requirements** (if needed):
- Add ASP.NET Core Identity (local user database)
- Implement `[Authorize]` attribute on DashboardPage.razor
- OAuth would violate "local-only" constraint

---

### Data Protection

**JSON File Permissions**:
- On Windows: Use NTFS ACLs to restrict data.json read access to authorized group
- On Linux/macOS: Use file mode (chmod 600) to restrict to owner only

**Encryption**:
- Not required for fictional project data
- If storing sensitive milestone information, encrypt data.json at rest using OS-level encryption (BitLocker, FileVault, LUKS)

**Data Handling**:
- No personally identifiable information (PII) in dashboard
- Project milestones and status are non-sensitive
- No passwords, API keys, or credentials stored in data.json

---

### Input Validation

**JSON Validation**:
- System.Text.Json automatically validates JSON syntax
- Invalid JSON throws JsonException; caught and displayed to user as error message
- Schema validation: Blazor deserializer validates structure against C# types (missing properties handled gracefully)

**File Path Security**:
- data.json loaded from fixed application root: `Path.Combine(AppContext.BaseDirectory, "data.json")`
- No user input for file path (no path traversal vulnerability)

---

### HTTPS / TLS

**Local Development**:
- HTTPS on localhost via self-signed certificate (provided by Blazor template)
- Acceptable for internal use; certificate warnings are expected

**Production Deployment**:
- If exposed beyond localhost, obtain valid certificate (e.g., Let's Encrypt)
- Configure in appsettings.json or environment variables

**HTTP Only**:
- For internal network deployment, HTTP is acceptable (no encryption needed if network is isolated)

---

### Dependency Security

**Zero External NuGet Packages**:
- No supply chain risk (only .NET 8.0 framework dependencies)
- All code ownership is team-controlled

**Framework Security**:
- Keep .NET 8.0 runtime updated via OS package manager
- Subscribe to Microsoft security bulletins for .NET patches

---

### Logging & Audit

**Minimal Logging** (MVP):
- Console output of startup and errors
- No request logging (read-only dashboard, no sensitive operations)

**Future Audit Trail**:
- Git history of data.json changes (via commit log)
- Optional Serilog integration for structured logging to file

---

## Scaling Strategy

### Expected Load

**MVP Target**: 1 concurrent user (presenter) + 0-10 simultaneous viewers (internal stakeholders)

**Scalability Assumptions**:
- Blazor Server supports ~100 concurrent WebSocket connections per instance
- Typical dashboard data < 100 KB JSON
- Page load time < 2 seconds
- No real-time updates (read-only, refreshed monthly)

### Horizontal Scaling

**Not Required for MVP** but possible post-launch:

1. **Multiple Instances**: Deploy same application to multiple servers
2. **Load Balancer**: Route requests via HAProxy or nginx
3. **Session Management**: Blazor Server sessions are in-process; requires sticky session routing OR move to Blazor WASM
4. **Shared Data**: All instances read same data.json from shared network drive or file server

---

### Vertical Scaling

**For Higher Concurrent Users**:
1. Increase server RAM (Blazor Server sessions consume ~1 MB each)
2. Upgrade CPU (more cores = more request parallelism)
3. Optimize JSON deserialization if dataset grows > 5 MB

---

### Data Growth

**Timeline**: 1-2 months per project dashboard

**Expected Data Volume**:
- 4-6 time periods (months/quarters)
- 3-5 milestones
- 20-50 status items across 4 categories
- Total JSON: 50-100 KB

**Scaling Considerations**:
- If dataset grows beyond 1 MB, consider pagination (tab selector or dropdown to filter months)
- If milestonelist exceeds 100, consider clustering on timeline or interactive legend

---

### Performance Monitoring

**Key Metrics** (monitor in production):
- Page load time (target: < 2 seconds)
- Blazor WebSocket connection time
- JSON deserialization time (should be < 10ms for small files)
- Peak memory usage per request

**Tools**:
- Browser DevTools Network tab (development)
- Application Insights (if deployed to Azure)
- Dynatrace or New Relic (for detailed APM, future enhancement)

---

### Future Enhancements for Scalability

1. **Caching Layer**: Redis for data.json cache (if reloading frequently)
2. **CDN**: CloudFlare or similar for static assets (CSS, favicon)
3. **Pagination**: Multiple pages or tab-based filtering for large datasets
4. **API Gateway**: Kong or Envoy for request routing and rate limiting
5. **Kubernetes**: Container orchestration for multi-instance deployments (requires Dockerfile)

---

## Risks & Mitigations

### Risk 1: JSON File Corruption

**Severity**: Medium

**Scenario**: Project Manager accidentally introduces invalid JSON (trailing comma, missing quote, etc.)

**Impact**:
- Dashboard fails to load
- Users see error message
- No data until JSON is corrected

**Mitigation**:
1. Include schema documentation in data.json template with inline comments
2. Implement schema validation in ReportDataService (optional but recommended)
3. Provide example JSON file with comments
4. Add unit tests to verify parsing of valid/invalid JSON
5. Implement graceful error handling (error message displayed, not crash)

**Implementation**:
```csharp
catch (JsonException ex)
{
    _logger.LogError($"JSON parsing error at line {ex.LineNumber}: {ex.Message}");
    throw new ApplicationException("Invalid JSON in data.json", ex);
}
```

---

### Risk 2: Missing data.json File

**Severity**: Medium

**Scenario**: Application deployed without data.json in root directory

**Impact**:
- FileNotFoundException on startup
- Dashboard displays error message
- No fallback data available

**Mitigation**:
1. Include default data.json in repository
2. Copy data.json to output directory during build (via .csproj configuration)
3. Document deployment checklist (include data.json)
4. Add startup validation: check for data.json existence, display friendly error if missing
5. Provide example data for quick testing

**Implementation in .csproj**:
```xml
<ItemGroup>
  <None Update="data.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

---

### Risk 3: SVG Timeline Rendering Performance Degradation

**Severity**: Low

**Scenario**: Timeline grows to 100+ milestones or very long date range (2+ years)

**Impact**:
- SVG rendering takes > 5 seconds
- Browser becomes unresponsive while rendering
- User sees visual jank

**Mitigation**:
1. Keep timeline within 12-24 month range (by design)
2. Limit milestones to 10-15 per project (reasonable for executive dashboard)
3. Test performance with large datasets (100 milestones, 24 months) before scaling
4. If performance degrades, consider:
   - Switching to D3.js or Plotly for optimized rendering
   - Implementing timeline pagination or zoom controls
   - Pre-calculating SVG coordinates to avoid expensive calculations

**Testing**:
```csharp
[Test]
public void GenerateSvgTimeline_With100Milestones_RendersIn2Seconds()
{
    var sw = Stopwatch.StartNew();
    var svg = _component.GenerateSvgTimeline();
    sw.Stop();
    
    Assert.IsTrue(sw.ElapsedMilliseconds < 2000, 
        $"SVG generation took {sw.ElapsedMilliseconds}ms");
}
```

---

### Risk 4: CSS Grid Layout Breaks on Narrow Screens

**Severity**: Low

**Scenario**: Dashboard displayed on narrower than 1920px viewport (presentation via projector at 1600px)

**Impact**:
- Heatmap grid overflows horizontally
- Horizontal scrollbar appears
- Cannot take clean screenshot

**Mitigation**:
1. Design for 1920x1080 only (as specified); do not support responsive design in MVP
2. Document viewport requirement in README
3. Test on target display resolution before presentation
4. If narrower viewport needed, implement CSS media query + font scaling (post-MVP)
5. Alternative: Full-screen browser (F11 in Chrome) to maximize available width

**Fallback CSS** (future):
```css
@media (max-width: 1600px) {
  .hm-grid {
    grid-template-columns: 120px repeat(4, 1fr);
  }
  body { font-size: 14px; }
}
```

---

### Risk 5: Blazor Server WebSocket Disconnection

**Severity**: Low

**Scenario**: Network interruption or firewall timeout disconnects WebSocket

**Impact**:
- Dashboard becomes unresponsive
- User sees "disconnecting... reconnecting..." message
- Manual page refresh required if auto-reconnect fails

**Mitigation**:
1. Blazor Server includes built-in auto-reconnect with exponential backoff
2. Acceptable for internal tool (rare in stable network)
3. If needed, implement circuit fallback to show cached data
4. Monitor WebSocket connection logs; alert on frequent disconnections

**Blazor Configuration** (in Program.cs):
```csharp
services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });
```

---

### Risk 6: Concurrent Data.json Updates

**Severity**: Low

**Scenario**: Two team members edit data.json simultaneously in Git, causing merge conflict

**Impact**:
- Git merge conflict must be resolved manually
- Merge conflict marker syntax breaks JSON
- Application fails to load until conflict is resolved

**Mitigation**:
1. Use Git locking or turn-based access (product manager = single person updating data.json)
2. Code review process: PR review + approval before merge
3. Communication: Team announces data.json changes to avoid overlapping edits
4. Branch strategy: Feature branches for large changes; merge to main after review
5. Automated tests: CI pipeline validates JSON syntax on push (future enhancement)

---

### Risk 7: Lost Data Due to Accidental Delete or Overwrite

**Severity**: Medium

**Scenario**: Product Manager accidentally deletes data.json or overwrites with empty file

**Impact**:
- Historical data lost if not in Git history
- Need to restore from backup or Git previous commit

**Mitigation**:
1. Require commits for all data.json changes (no direct file edits on server)
2. Git branch protection: Require pull request review before merge
3. Git history: Preserve all previous versions in commit log
4. Backups: Keep Git repository backed up (GitHub, Bitbucket, or local mirror)
5. Recovery: Restore previous version via `git revert <commit>`

**Git Workflow** (enforce):
```bash
# Good: Create feature branch, edit, push, create PR, review, merge
git checkout -b update/april-data
# ... edit data.json ...
git commit -m "Update April status data"
git push origin update/april-data
# Create PR, get review, merge to main

# Bad: Direct edit on main branch (prevented by branch protection)
```

---

### Risk 8: Stale Data Display

**Severity**: Low

**Scenario**: Data.json updated but application not restarted; users see outdated dashboard

**Impact**:
- Mismatch between data.json and rendered dashboard
- Executive presents outdated information in meeting
- Requires application restart to see updates

**Mitigation**:
1. Document in README: Data changes require application restart
2. Implement file watcher + auto-reload (future enhancement)
3. Monitoring: Log last data load time; display in footer (optional)
4. Automation: Auto-restart application on data.json change (post-MVP)

**Future Implementation** (file watcher):
```csharp
public class ReportDataService
{
    private FileSystemWatcher _watcher;
    
    public void StartWatching()
    {
        _watcher = new FileSystemWatcher(AppContext.BaseDirectory);
        _watcher.NotifyFilter = NotifyFilters.LastWrite;
        _watcher.Filter = "data.json";
        _watcher.Changed += (sender, e) => 
        {
            _cachedReport = null; // Invalidate cache
            _logger.LogInformation("data.json changed; cache cleared");
        };
        _watcher.EnableRaisingEvents = true;
    }
}
```

---

### Risk 9: Browser Print Layout Issues

**Severity**: Low

**Scenario**: Print to PDF via browser cuts off sidebar, heatmap grid wraps unexpectedly

**Impact**:
- Screenshot exported to PDF is not presentation-ready
- User must manually edit image in PowerPoint
- Defeats "screenshot-friendly" design goal

**Mitigation**:
1. Test browser print-to-PDF with Chrome, Edge, Firefox
2. Set print styles to prevent page breaks mid-grid
3. Use CSS `@media print` to optimize layout for PDF output
4. Provide CSS print stylesheet

**Print CSS**:
```css
@media print {
  body { margin: 0; padding: 0; }
  .hm-grid { page-break-inside: avoid; }
  * { -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; }
}
```

---

### Risk 10: Color Blindness / Accessibility Issues

**Severity**: Low

**Scenario**: Executive with red-green color blindness cannot distinguish Shipped (green) from Blockers (red)

**Impact**:
- Dashboard is unusable for some users
- Violates accessibility standards (WCAG 2.1)

**Mitigation**:
1. Use both color AND icons/text to denote status (avoid color alone)
2. Ensure 4.5:1 contrast ratio for all text (WCAG AA compliant)
3. Use colorblind-friendly palette (e.g., Okabe-Ito colors for accessibility)
4. Add text labels in row headers: "✓ Shipped", "⚡ In Progress", "⬆ Carryover", "🚫 Blockers"
5. Test with accessibility checker (WebAIM, Lighthouse)

**Alternative Color Palette** (future):
- Red → Magenta/Purple (for blockers)
- Green → Teal (for shipped)
- Blue → Cyan (for in-progress)
- Yellow → Orange (for carryover)

---

## Deployment & Operations

### Development Workflow

```bash
# 1. Clone repository
git clone <repo> MyProject.Reporting
cd MyProject.Reporting

# 2. Restore dependencies & build
dotnet restore
dotnet build

# 3. Run application
dotnet watch run

# 4. Open in browser
# Navigate to https://localhost:7123
```

### Production Deployment

```bash
# 1. Publish release build
dotnet publish -c Release -o ./publish

# 2. Copy to target machine
scp -r ./publish/ user@target:/opt/MyProject.Reporting/

# 3. Run on target
cd /opt/MyProject.Reporting
dotnet MyProject.Reporting.dll --urls=http://0.0.0.0:5000
```

### Data Update Workflow

```bash
# 1. Edit data.json
nano data.json

# 2. Validate JSON (optional)
jq . data.json > /dev/null && echo "Valid JSON" || echo "Invalid JSON"

# 3. Commit to Git
git add data.json
git commit -m "Update May status data"
git push origin main

# 4. Restart application (manual or automated)
systemctl restart MyProject.Reporting
# OR: Kill dotnet process and restart
pkill -f "dotnet MyProject.Reporting.dll"
dotnet MyProject.Reporting.dll
```

---

## Conclusion

This architecture provides a simple, maintainable, and screenshot-friendly executive reporting dashboard built on Blazor Server .NET 8.0. The design leverages:

- **Server-side rendering** for consistent, exportable HTML/CSS
- **File-based JSON configuration** for zero-database complexity
- **Native SVG and CSS Grid** for timeline and heatmap visualization
- **Zero external dependencies** for security and simplicity
- **Local-only deployment** for enterprise network compliance

The system supports the PM's business goals of visibility, rapid iteration, and presentation-ready screenshots. MVP scope is achievable in 1-2 weeks with careful component design and testing. Post-MVP enhancements (multi-project, real-time sync, interactive drill-down) are designed to integrate without architectural rework.