# Architecture

## Overview & Goals

**My Project** is a lightweight, screenshot-optimized executive reporting dashboard built as a Blazor Server web application running on .NET 8 with no cloud dependencies. The system enables executives to view consolidated project health across four status categories (Shipped, In Progress, Carryover, Blockers) and timeline-based milestones through a single, scannable interface optimized for 1920x1080 resolution and PowerPoint exports.

### Primary Goals
1. **Real-time executive visibility**: Display project status without requiring code changes or redeployment
2. **Zero operational complexity**: No databases, no authentication, no caching layers, no external service dependencies
3. **Pixel-perfect reproducibility**: Ensure deterministic rendering suitable for PowerPoint screenshots across different machines
4. **Simple data management**: Project managers update only data.json; no database maintenance required
5. **Single-page accessibility**: Complete project health visible on initial load without navigation or configuration

### Design Principles
- **Stateless rendering**: Each page load fetches fresh data from data.json; no server-side session state
- **Deterministic output**: SVG timelines and CSS Grid heatmaps produce identical visuals every time
- **Visual compliance**: Exact pixel-for-pixel implementation of OriginalDesignConcept.html design specification
- **Operational simplicity**: Minimal infrastructure, maximal reliability through simplicity

---

## System Components

### 1. **Dashboard.razor** (Primary Page Component)
**Purpose**: Root Blazor Server component that orchestrates all sub-components and manages overall page layout.

**Responsibilities**:
- Load and initialize dashboard configuration on page load
- Manage component lifecycle and re-render triggers (data refresh)
- Coordinate between sub-components (header, timeline, heatmap)
- Handle error states and graceful degradation
- Implement page-level styling and layout structure (flexbox column)

**Interfaces**:
```csharp
public partial class Dashboard : ComponentBase
{
    [Inject] private DashboardDataService DataService { get; set; }
    [Inject] private DateCalculationService DateService { get; set; }
    
    private DashboardConfig config;
    private string errorMessage;
    
    protected override async Task OnInitializedAsync();
    private async Task LoadDashboardAsync();
}
```

**Dependencies**:
- `DashboardDataService`: Load and parse data.json
- `DateCalculationService`: Calculate month boundaries and milestone positions
- Blazor runtime and component lifecycle

**Data Input**: None (loads from DashboardDataService)
**Data Output**: Renders HTML structure with sub-component props

---

### 2. **Header.razor** (Sub-component)
**Purpose**: Display project title, subtitle, and legend showing milestone type indicators.

**Responsibilities**:
- Render project name and subtitle from DashboardConfig
- Display legend with four milestone types (PoC, Production Release, Checkpoint, Now marker)
- Show "Last Updated" timestamp from data.json file modification time
- Render ADO Backlog hyperlink

**Interfaces**:
```csharp
public partial class Header : ComponentBase
{
    [Parameter]
    public DashboardConfig Config { get; set; }
    
    [Parameter]
    public DateTime LastUpdated { get; set; }
    
    [Inject] private VisualizationService VisualizationService { get; set; }
}
```

**Dependencies**:
- `VisualizationService`: Legend color and shape definitions
- CSS classes from dashboard.css (.hdr, .sub classes)

**Data Input**: 
- DashboardConfig (projectName, description)
- LastUpdated DateTime

**Data Output**: 
- Rendered header HTML with legend elements

---

### 3. **TimelineChart.razor** (Sub-component)
**Purpose**: Render SVG timeline with milestone markers, month gridlines, and "Now" indicator.

**Responsibilities**:
- Generate SVG with programmatic coordinates based on milestone dates
- Calculate month gridline positions (Jan=0, Feb=260, Mar=520, Apr=780, May=1040, Jun=1300 pixels)
- Position "Now" marker (red dashed line) at current date
- Render milestone lines, start circles, checkpoints, and end diamonds
- Handle tooltip display on hover (future enhancement)

**Interfaces**:
```csharp
public partial class TimelineChart : ComponentBase
{
    [Parameter]
    public List<Milestone> Milestones { get; set; }
    
    [Parameter]
    public DateTime CurrentDate { get; set; }
    
    [Inject] private DateCalculationService DateService { get; set; }
    [Inject] private VisualizationService VisualizationService { get; set; }
    
    private string TimelineSvg { get; set; }
    
    protected override void OnParametersSet();
    private string GenerateTimelineSvg();
}
```

**Dependencies**:
- `DateCalculationService`: Convert milestone dates to SVG x-coordinates
- `VisualizationService`: SVG shape generation and color mappings
- NodaTime: Date arithmetic for month boundary calculations

**Data Input**:
- Milestones list (id, label, date, type)
- CurrentDate (system date)

**Data Output**:
- SVG markup as string
- HTML with left milestone label sidebar and SVG timeline box

---

### 4. **HeatmapGrid.razor** (Sub-component)
**Purpose**: Render CSS Grid heatmap displaying status items in month-by-status matrix.

**Responsibilities**:
- Generate 4-month x 4-status grid from quarters and status data
- Apply color-coded backgrounds (green/blue/yellow/red) based on status type
- Highlight current month column with darker background
- Render item names with colored dot indicators
- Display dashes ("-") in empty cells
- Handle item overflow with text truncation and tooltips (future)

**Interfaces**:
```csharp
public partial class HeatmapGrid : ComponentBase
{
    [Parameter]
    public List<Quarter> Quarters { get; set; }
    
    [Parameter]
    public DateTime CurrentDate { get; set; }
    
    [Inject] private VisualizationService VisualizationService { get; set; }
    [Inject] private DateCalculationService DateService { get; set; }
    
    private List<Quarter> DisplayQuarters { get; set; }
    
    protected override void OnParametersSet();
    private string GetCellClass(string status, bool isCurrentMonth);
    private string GetDotColor(string status);
}
```

**Dependencies**:
- `VisualizationService`: Color and class name mappings for status types
- `DateCalculationService`: Determine current month for highlighting
- CSS classes from dashboard.css (.hm-grid, .hm-cell, .ship-cell, .prog-cell, .carry-cell, .block-cell, etc.)

**Data Input**:
- Quarters list with shipped[], inProgress[], carryover[], blockers[] arrays
- CurrentDate (for highlighting current month)

**Data Output**:
- HTML CSS Grid markup with data cells

---

### 5. **DashboardDataService** (Service Layer)
**Purpose**: Load, parse, and provide access to dashboard configuration from data.json file.

**Responsibilities**:
- Read data.json from file system (configurable path via appsettings.json)
- Deserialize JSON into DashboardConfig POCO objects using System.Text.Json
- Validate JSON schema and required fields; raise exceptions for invalid data
- Provide method to refresh data (re-read from disk)
- Cache parsed config in memory; invalidate on refresh request
- Track file modification time for "Last Updated" timestamp

**Interfaces**:
```csharp
public class DashboardDataService
{
    private readonly IConfiguration configuration;
    private DashboardConfig cachedConfig;
    private DateTime lastLoadTime;
    
    public async Task<DashboardConfig> GetDashboardConfigAsync();
    public async Task RefreshAsync();
    public DateTime GetLastModifiedTime();
    
    private async Task ValidateConfigSchema(DashboardConfig config);
}
```

**Dependencies**:
- IConfiguration (appsettings.json for data file path)
- System.Text.Json (JSON deserialization)
- File system (read data.json)

**Data Input**: 
- data.json file from disk

**Data Output**:
- DashboardConfig object (deserialized)
- DateTime of last file modification

**Error Handling**:
- FileNotFoundException: data.json does not exist → Throw exception with helpful message
- JsonException: Invalid JSON → Throw exception with line/column information
- SchemaValidationException: Missing required fields → Throw exception listing missing fields

---

### 6. **DateCalculationService** (Service Layer)
**Purpose**: Handle all date and timeline calculations, including month boundaries, milestone positioning, and "Now" marker placement.

**Responsibilities**:
- Calculate 4-month display window based on current date
- Determine month start/end dates using NodaTime
- Convert DateTime to SVG x-coordinates based on month grid (260px per month)
- Calculate "Now" marker x-position relative to Jan 1 baseline
- Validate milestone dates fall within a reasonable range
- Determine which quarter contains current date for highlighting

**Interfaces**:
```csharp
public class DateCalculationService
{
    private const int PixelsPerMonth = 260;
    private const int SvgWidth = 1560;
    private readonly ILogger<DateCalculationService> logger;
    
    public List<MonthInfo> GetDisplayMonths(DateTime currentDate);
    public int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate);
    public int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate);
    public bool IsCurrentMonth(YearMonth month, DateTime currentDate);
    public (int startX, int endX) GetMonthBounds(int monthIndex);
    
    private DateTime GetJanuaryFirstOfYear(DateTime date);
}
```

**Dependencies**:
- NodaTime (YearMonth, LocalDate, Period for calendar math)
- ILogger (diagnostic logging for date calculations)

**Data Input**:
- Milestone dates (ISO 8601 strings from data.json)
- Current system date

**Data Output**:
- MonthInfo objects (month name, year, start/end dates)
- SVG x-coordinates (integers)
- Boolean flags for highlighting logic

**Assumptions**:
- Baseline reference date is January 1 of the display year
- Timeline displays Jan-Jun (6 months, 1560px total = 260px per month)
- Milestones can be positioned within Jan-Jun range
- Current month is always one of the 4 display months

---

### 7. **VisualizationService** (Service Layer)
**Purpose**: Provide color, CSS class, and SVG shape definitions for visual consistency across components.

**Responsibilities**:
- Map status types to CSS class names (ship-cell, prog-cell, carry-cell, block-cell)
- Map status types to indicator dot colors (#34A853 green, #0078D4 blue, #F4B400 gold, #EA4335 red)
- Generate SVG shapes (diamonds, circles, lines) with proper attributes
- Provide milestone type to shape/color mappings
- Centralize all color hex codes as constants

**Interfaces**:
```csharp
public class VisualizationService
{
    // Color constants
    private const string ColorShippedDot = "#34A853";
    private const string ColorInProgressDot = "#0078D4";
    private const string ColorCarryoverDot = "#F4B400";
    private const string ColorBlockersDot = "#EA4335";
    
    public string GetCellClassName(string status, bool isCurrentMonth);
    public string GetDotColor(string status);
    public string GetStatusHeaderClassName(string status);
    public string GenerateSvgDiamond(int cx, int cy, string fill, bool withFilter = true);
    public string GenerateSvgCircle(int cx, int cy, int radius, string fill, string stroke, int strokeWidth);
    public string GenerateSvgLine(int x1, int y1, int x2, int y2, string stroke, int strokeWidth, string dasharray = null);
    
    public Dictionary<string, MilestoneShapeInfo> GetMilestoneShapes();
}
```

**Dependencies**:
- No external dependencies; pure utility class

**Data Input**:
- Status type strings ("shipped", "inProgress", "carryover", "blockers")
- Milestone type strings ("poc", "release", "checkpoint")
- Coordinate values (cx, cy, radius)

**Data Output**:
- CSS class names
- Hex color codes
- SVG markup strings

---

### 8. **Error Handler Middleware**
**Purpose**: Catch and log unhandled exceptions; display user-friendly error page.

**Responsibilities**:
- Intercept exceptions during request processing
- Log error details (timestamp, stack trace, user context)
- Display graceful error message to user (not stack trace)
- Distinguish between recoverable errors (missing data.json) and fatal errors (code bugs)

**Implementation**:
- Blazor error boundary component (catch in Dashboard.razor)
- Global exception handler middleware in Program.cs
- Custom error page with troubleshooting steps

**Data Input**: Exception object
**Data Output**: User-friendly error message HTML

---

## Component Interactions

### Data Flow: Page Load Sequence
1. **User navigates to dashboard URL** → Browser requests `/dashboard` route
2. **Dashboard.razor OnInitializedAsync()** triggered:
   - Calls `DashboardDataService.GetDashboardConfigAsync()`
3. **DashboardDataService** execution:
   - Reads data.json from file path in appsettings.json
   - Deserializes JSON to DashboardConfig POCO
   - Validates schema (all required fields present)
   - Returns DashboardConfig object
4. **Dashboard.razor** receives config:
   - Extracts lastUpdatedTime via `File.GetLastWriteTime()`
   - Passes config to child components via [@Parameters]
5. **Header.razor** renders:
   - Displays projectName and description from config
   - Shows lastUpdatedTime
   - Renders legend with hardcoded colors/shapes
6. **TimelineChart.razor** renders:
   - Receives Milestones list from config.milestones
   - Calls `DateCalculationService.GetDisplayMonths()` → Get Jan-Jun month info
   - Calls `DateCalculationService.GetMilestoneXPosition()` for each milestone
   - Calls `DateCalculationService.GetNowMarkerXPosition()` → Get red line x-coord
   - Calls `VisualizationService.GenerateSvgDiamond/Circle/Line()` → Build SVG markup
   - Renders SVG string to HTML
7. **HeatmapGrid.razor** renders:
   - Receives Quarters list from config.quarters
   - Calls `DateCalculationService.GetDisplayMonths()` → Get month headers
   - For each quarter/status combination:
     - Calls `VisualizationService.GetCellClassName()` → Determine background color CSS
     - Calls `VisualizationService.GetDotColor()` → Determine dot colors
     - Renders items with colored dots
   - Highlights current month column (darker background)
   - Fills empty cells with "-"
8. **Page render complete** → Browser displays full dashboard in < 2 seconds

### Data Refresh Flow (User Manually Updates data.json)
1. **User edits data.json** (add/remove items, change dates, etc.)
2. **User presses F5** to refresh browser
3. **Dashboard.razor re-initializes**:
   - Calls `DashboardDataService.GetDashboardConfigAsync()` again
   - Service detects file modification time has changed
   - Re-reads and re-parses data.json
4. **All child components re-render** with new data
5. **Timeline markers and heatmap items** update to reflect changes

### Component Communication Pattern
```
Dashboard (parent)
├── Header (prop: config, lastUpdated)
├── TimelineChart (prop: milestones[], currentDate)
└── HeatmapGrid (prop: quarters[], currentDate)

Services (injected into components):
├── DashboardDataService → Provides config
├── DateCalculationService → Calculates dates/positions
└── VisualizationService → Provides colors/CSS/SVG
```

---

## Data Model

### 1. **DashboardConfig** (Root POCO)
```csharp
public class DashboardConfig
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("quarters")]
    public List<Quarter> Quarters { get; set; } = new();
    
    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();
}
```

### 2. **Quarter** (Status Data by Month)
```csharp
public class Quarter
{
    [JsonPropertyName("month")]
    public string Month { get; set; }
    
    [JsonPropertyName("year")]
    public int Year { get; set; }
    
    [JsonPropertyName("shipped")]
    public List<string> Shipped { get; set; } = new();
    
    [JsonPropertyName("inProgress")]
    public List<string> InProgress { get; set; } = new();
    
    [JsonPropertyName("carryover")]
    public List<string> Carryover { get; set; } = new();
    
    [JsonPropertyName("blockers")]
    public List<string> Blockers { get; set; } = new();
}
```

### 3. **Milestone** (Timeline Event)
```csharp
public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("label")]
    public string Label { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; } // ISO 8601 format: "2026-04-30"
    
    [JsonPropertyName("type")]
    public string Type { get; set; } // "poc" | "release" | "checkpoint"
}
```

### 4. **MonthInfo** (Calculated Month Data)
```csharp
public class MonthInfo
{
    public string Name { get; set; }
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int GridColumnIndex { get; set; }
    public bool IsCurrentMonth { get; set; }
}
```

### 5. **MilestoneShapeInfo** (Visualization Metadata)
```csharp
public class MilestoneShapeInfo
{
    public string Type { get; set; } // "poc", "release", "checkpoint"
    public string Shape { get; set; } // "diamond", "circle"
    public string Color { get; set; } // Hex color code
    public int Size { get; set; } // Pixels (7 for start, 5 for checkpoint, etc.)
}
```

### Storage Model
**Data Storage**: data.json file in wwwroot/data/ directory
- **File format**: JSON (UTF-8)
- **Maximum file size**: < 10 KB (supports up to 50 items per status type across 12 months)
- **Access pattern**: Read-only on application startup; file system watcher optional for auto-refresh
- **Backup strategy**: Version controlled in Git; no separate backup system needed
- **Concurrency**: Single file, single reader; no locking mechanism needed (assume sequential access)

### Data Relationships
```
DashboardConfig
├── Quarters[] (4-6 items, one per month)
│   ├── Shipped[] (0-20 item names)
│   ├── InProgress[] (0-20 item names)
│   ├── Carryover[] (0-20 item names)
│   └── Blockers[] (0-20 item names)
└── Milestones[] (3-10 items)
    ├── id (unique identifier)
    ├── label (display name)
    ├── date (ISO 8601 string)
    └── type ("poc" | "release" | "checkpoint")
```

### Data Validation Rules
1. **DashboardConfig**:
   - `ProjectName`: Required, max 255 characters, non-empty string
   - `Description`: Required, max 255 characters
   - `Quarters`: Required, 1-12 items, sorted by month/year
   - `Milestones`: Required, 0-50 items

2. **Quarter**:
   - `Month`: Required, valid month name (January-December)
   - `Year`: Required, 2000-2099 (reasonable range)
   - Status arrays: Optional, 0-50 items each

3. **Milestone**:
   - `Id`: Required, unique within milestones array
   - `Label`: Required, max 100 characters
   - `Date`: Required, valid ISO 8601 date (YYYY-MM-DD)
   - `Type`: Required, one of ["poc", "release", "checkpoint"]

### Example data.json
```json
{
  "projectName": "Privacy Automation Release Roadmap",
  "description": "Trusted Platform - Privacy Automation Workstream - April 2026",
  "quarters": [
    {
      "month": "March",
      "year": 2026,
      "shipped": ["Chatbot MVP", "Role Templates"],
      "inProgress": ["Data Inventory", "Policy Engine"],
      "carryover": ["DFD Automation"],
      "blockers": []
    },
    {
      "month": "April",
      "year": 2026,
      "shipped": [],
      "inProgress": ["Integration Testing"],
      "carryover": ["DFD Automation"],
      "blockers": ["Azure AD Sync Dependency"]
    },
    {
      "month": "May",
      "year": 2026,
      "shipped": [],
      "inProgress": [],
      "carryover": [],
      "blockers": []
    },
    {
      "month": "June",
      "year": 2026,
      "shipped": [],
      "inProgress": [],
      "carryover": [],
      "blockers": []
    }
  ],
  "milestones": [
    {
      "id": "m1",
      "label": "Chatbot & MS Role",
      "date": "2026-01-12",
      "type": "checkpoint"
    },
    {
      "id": "m2-poc",
      "label": "PoC Milestone",
      "date": "2026-03-26",
      "type": "poc"
    },
    {
      "id": "m3-prod",
      "label": "Production Release",
      "date": "2026-04-30",
      "type": "release"
    }
  ]
}
```

---

## API Contracts

### DashboardDataService Contracts

#### GetDashboardConfigAsync()
**Purpose**: Load and return parsed dashboard configuration from data.json

**Request**: None (reads from file system)

**Response** (Success):
```csharp
Task<DashboardConfig>
{
    ProjectName: "Privacy Automation Release Roadmap",
    Description: "Trusted Platform - Privacy Automation Workstream - April 2026",
    Quarters: [ ... ],
    Milestones: [ ... ]
}
```

**Response** (Error):
```csharp
throw new FileNotFoundException(
    "data.json not found at path: ./wwwroot/data/data.json. " +
    "Please ensure the file exists and the path in appsettings.json is correct."
);

throw new JsonException(
    "Invalid JSON in data.json. Error at line X, column Y: " + message
);

throw new InvalidOperationException(
    "data.json schema validation failed. Missing required fields: [fieldName1, fieldName2]"
);
```

#### RefreshAsync()
**Purpose**: Re-read data.json from disk, clearing in-memory cache

**Request**: None

**Response** (Success): Returns refreshed DashboardConfig

**Response** (Error): Same as GetDashboardConfigAsync()

#### GetLastModifiedTime()
**Purpose**: Return the file system modification time of data.json

**Request**: None

**Response** (Success):
```csharp
DateTime // e.g., 2026-04-12T02:51:33.000Z (UTC)
```

---

### DateCalculationService Contracts

#### GetDisplayMonths(DateTime currentDate)
**Purpose**: Calculate 4-month display window based on current date

**Request**:
```csharp
currentDate: DateTime (e.g., 2026-04-12)
```

**Response** (Success):
```csharp
List<MonthInfo>
[
    {
        Name: "March",
        Year: 2026,
        StartDate: 2026-03-01,
        EndDate: 2026-03-31,
        GridColumnIndex: 0,
        IsCurrentMonth: false
    },
    {
        Name: "April",
        Year: 2026,
        StartDate: 2026-04-01,
        EndDate: 2026-04-30,
        GridColumnIndex: 1,
        IsCurrentMonth: true
    },
    ...
]
```

**Response** (Error): Never throws; always returns valid 4-month window

#### GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate)
**Purpose**: Convert milestone date to SVG x-coordinate (pixels)

**Request**:
```csharp
milestoneDate: DateTime (e.g., 2026-03-26)
baselineDate: DateTime (e.g., 2026-01-01) // Reference point for calculation
```

**Response** (Success):
```csharp
int // Pixels from left edge (0-1560), e.g., 745 for mid-March
```

**Response** (Error):
```csharp
throw new ArgumentOutOfRangeException(
    $"Milestone date {milestoneDate:yyyy-MM-dd} is outside timeline range (Jan 1 - Jun 30)"
);
```

#### GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate)
**Purpose**: Calculate x-coordinate of red "Now" dashed line

**Request**:
```csharp
currentDate: DateTime (e.g., 2026-04-12)
baselineDate: DateTime (e.g., 2026-01-01)
```

**Response** (Success):
```csharp
int // Pixels from left edge, e.g., 823 for April 12
```

---

### VisualizationService Contracts

#### GetCellClassName(string status, bool isCurrentMonth)
**Purpose**: Return CSS class name for heatmap cell

**Request**:
```csharp
status: string // "shipped" | "inProgress" | "carryover" | "blockers"
isCurrentMonth: bool
```

**Response**:
```csharp
string // e.g., "ship-cell apr" or "prog-cell"
```

#### GetDotColor(string status)
**Purpose**: Return hex color code for item indicator dot

**Request**:
```csharp
status: string // "shipped" | "inProgress" | "carryover" | "blockers"
```

**Response**:
```csharp
string // e.g., "#34A853" (shipped green)
```

---

## UI Component Architecture

### Header Section Mapping
| Design Section | Blazor Component | CSS Classes | Data Binding | Interaction |
|---|---|---|---|---|
| Project Title | `Header.razor` | `.hdr h1` | `Config.ProjectName` | Display only |
| Subtitle/Context | `Header.razor` | `.sub` | `Config.Description` | Display only |
| Legend: PoC Milestone | `Header.razor` inline | Diamond shape + "PoC Milestone" text | Hardcoded yellow #F4B400 | Display only |
| Legend: Production Release | `Header.razor` inline | Diamond shape + "Production Release" text | Hardcoded green #34A853 | Display only |
| Legend: Checkpoint | `Header.razor` inline | Circle shape + "Checkpoint" text | Hardcoded gray #999 | Display only |
| Legend: Now Marker | `Header.razor` inline | Vertical line + "Now ([Month] [Year])" text | Hardcoded red #EA4335 + current month | Display only |
| Last Updated Timestamp | `Header.razor` | Positioned in header right area | `DashboardDataService.GetLastModifiedTime()` | Display only |
| ADO Backlog Link | `Header.razor` | Hyperlink in title | Hardcoded URL (configurable) | Click to navigate |

### Timeline Section Mapping
| Design Section | Blazor Component | CSS Classes | Data Binding | Interaction |
|---|---|---|---|---|
| Left Milestone Labels (M1, M2, M3) | `TimelineChart.razor` sidebar | Div with color-coded labels | `Milestones[*].Label` grouped by type | Display only |
| Month Gridlines (Jan-Jun) | `TimelineChart.razor` SVG | SVG `<line>` elements | Static positions (0, 260, 520, 780, 1040, 1300 px) | Display only |
| Month Labels (Jan, Feb, Mar, Apr, May, Jun) | `TimelineChart.razor` SVG | SVG `<text>` elements | Hardcoded text, positioned at gridline x-coords | Display only |
| "Now" Marker (red dashed line) | `TimelineChart.razor` SVG | SVG `<line>` with stroke-dasharray | `DateCalculationService.GetNowMarkerXPosition(currentDate)` | Display only (future: hover tooltip) |
| Milestone Timeline Lines (M1, M2, M3) | `TimelineChart.razor` SVG | SVG `<line>` elements | One per milestone type; colors #0078D4 (M1), #00897B (M2), #546E7A (M3) | Display only |
| Milestone Start Circles | `TimelineChart.razor` SVG | SVG `<circle>` elements with stroke | Positioned at milestone start dates | Display only |
| Milestone Checkpoints | `TimelineChart.razor` SVG | SVG `<circle>` elements (smaller) | Positioned at checkpoint dates | Display only (future: tooltip) |
| Milestone End Diamonds (PoC/Prod) | `TimelineChart.razor` SVG | SVG `<polygon>` elements | Positioned at milestone end dates; color per type | Display only (future: click to expand) |
| Milestone Date Labels | `TimelineChart.razor` SVG | SVG `<text>` elements | `Milestone.Date` formatted as "MMM DD" or "Month YYYY" | Display only |

### Heatmap Section Mapping
| Design Section | Blazor Component | CSS Classes | Data Binding | Interaction |
|---|---|---|---|---|
| Grid Title (MONTHLY EXECUTION HEATMAP) | `HeatmapGrid.razor` | `.hm-title` | Hardcoded text | Display only |
| Grid Container | `HeatmapGrid.razor` | `.hm-grid` (CSS Grid layout) | Dynamic layout based on quarters count | Display only |
| Corner Cell (Status label header) | `HeatmapGrid.razor` | `.hm-corner` | Hardcoded "Status" | Display only |
| Month Header Cells (March, April, May, June) | `HeatmapGrid.razor` | `.hm-col-hdr`, `.apr-hdr` (for current month) | `Quarter.Month` + " " + `Quarter.Year`; current month adds " ★ Now" | Display only |
| Shipped Row Header | `HeatmapGrid.razor` | `.hm-row-hdr .ship-hdr` | Hardcoded "● Shipped" with icon | Display only |
| In Progress Row Header | `HeatmapGrid.razor` | `.hm-row-hdr .prog-hdr` | Hardcoded "⊕ In Progress" with icon | Display only |
| Carryover Row Header | `HeatmapGrid.razor` | `.hm-row-hdr .carry-hdr` | Hardcoded "▬ Carryover" with icon | Display only |
| Blockers Row Header | `HeatmapGrid.razor` | `.hm-row-hdr .block-hdr` | Hardcoded "⛔ Blockers" with icon | Display only |
| Shipped Data Cells | `HeatmapGrid.razor` | `.hm-cell .ship-cell`, `.ship-cell.apr` | `Quarter.Shipped[]` items | Display with tooltip on hover (future) |
| In Progress Data Cells | `HeatmapGrid.razor` | `.hm-cell .prog-cell`, `.prog-cell.apr` | `Quarter.InProgress[]` items | Display with tooltip on hover (future) |
| Carryover Data Cells | `HeatmapGrid.razor` | `.hm-cell .carry-cell`, `.carry-cell.apr` | `Quarter.Carryover[]` items | Display with tooltip on hover (future) |
| Blockers Data Cells | `HeatmapGrid.razor` | `.hm-cell .block-cell`, `.block-cell.apr` | `Quarter.Blockers[]` items | Display with tooltip on hover (future) |
| Item Text with Dot Indicator | `HeatmapGrid.razor` | `.it`, `.it::before` (pseudo-element for dot) | Item name string; dot color via `VisualizationService.GetDotColor()` | Display only |
| Empty Cell Indicator | `HeatmapGrid.razor` | `.it` with dash content | "-" when status array is empty | Display only |

### CSS Layout Strategy
- **Header**: Flexbox row (space-between) with fixed height 60px
- **Timeline**: Flexbox row with fixed height 196px; left sidebar 230px fixed, SVG flex-grow
- **Heatmap**: Flexbox column (flex-grow) with CSS Grid child (5 columns, 5 rows)
- **Overall Page**: Flexbox column with body width 1920px, height 1080px

### Data Binding Strategy
- **@bind**: Not used (dashboard is read-only on user side)
- **@Parameter**: Pass data from parent Dashboard to Header/TimelineChart/HeatmapGrid
- **@inject**: Inject services (DashboardDataService, DateCalculationService, VisualizationService)
- **String interpolation**: Display item names, dates, labels directly in HTML/SVG

### Interaction Pattern
- **MVP**: Read-only dashboard; no click handlers or event bindings
- **Future (Post-MVP)**: Hover tooltips, click to expand details, filter by status
- **Current Implementation**: No @onclick, no @onmouseenter/leave; all CSS-based styling

---

## Infrastructure Requirements

### Runtime Environment
- **Operating System**: Windows 10/11 (primary), Windows Server 2019+ (production)
- **.NET Runtime**: .NET 8 LTS (8.0.x or higher)
- **Web Server**: Kestrel (built-in) or IIS with ASP.NET Core module
- **RAM**: Minimum 512 MB; recommended 2 GB for typical usage
- **Disk Space**: 200 MB for application files + data.json (typically < 10 KB)
- **Network**: Local network (10 Mbps minimum); no internet required

### Deployment Architecture
```
Windows Server / Desktop Machine
├── .NET 8 Runtime (via self-contained publish or system-wide)
├── Application Binary (AgentSquad.Runner.exe)
├── wwwroot/
│   ├── css/dashboard.css
│   ├── js/ (optional for future interactivity)
│   └── data/data.json (read by application on startup)
├── appsettings.json (configurable paths)
├── appsettings.Development.json (local dev overrides)
└── logs/ (application diagnostics)
```

### Network Configuration
- **Deployment**: Self-contained .exe file
- **Port**: 5000 (default for Kestrel)
- **Binding**: 0.0.0.0 (listen on all network interfaces for internal access)
- **Protocol**: HTTP (HTTPS optional with self-signed certificate)
- **Firewall**: Restrict port 5000 to trusted subnets; no internet-facing exposure
- **DNS**: Not required; access via IP address (e.g., http://192.168.1.100:5000)

### Storage Configuration
- **data.json Location**: `wwwroot/data/data.json` (default, configurable in appsettings.json)
- **Storage Type**: Local file system
- **Backup**: Not automated; rely on Git version control
- **Permissions**: Application process requires read-only access to data.json

### Configuration Management
**appsettings.json** (committed to repo):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  },
  "DashboardDataPath": "./wwwroot/data/data.json",
  "AllowRemoteAccess": false,
  "Port": 5000
}
```

**appsettings.Development.json** (local development override):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

**appsettings.Production.json** (optional, for shared network deployment):
```json
{
  "AllowRemoteAccess": true,
  "Port": 80
}
```

### Build Pipeline
- **Build Tool**: .NET CLI (dotnet build, dotnet publish)
- **Target Framework**: net8.0
- **Output Type**: Executable (.exe)
- **Configuration**: Release
- **Self-Contained**: Yes (includes .NET 8 runtime)
- **Runtime Identifier**: win-x64 (Windows 64-bit)
- **Publish Command**: `dotnet publish -c Release -r win-x64 --self-contained`

### Publish Artifact
- **Filename**: AgentSquad.Runner.exe (self-contained executable)
- **Size**: ~120 MB (includes .NET runtime + app)
- **Distribution**: Copy single .exe + appsettings.json + wwwroot/ folder to target machine
- **Execution**: Double-click .exe or run from command line; listens on port 5000

### Version Control & Deployment
- **Repository**: Git (internal, private)
- **Branches**: main (production), develop (staging), feature/* (in-progress)
- **Artifacts**: Build outputs stored in bin/Release/net8.0/win-x64/publish/
- **Release Process**: Git tag (v1.0, v1.1, etc.) + publish executable to shared network folder

### Monitoring & Logging
- **Logging Framework**: Microsoft.Extensions.Logging (built-in)
- **Log Level**: Warning (production), Debug (development)
- **Log Output**: Console (Kestrel stdout) or file (future enhancement)
- **Health Checks**: None required for MVP (stateless, no dependencies)
- **Diagnostics**: Inspect browser console for JavaScript errors (expected: none)

### CI/CD (Optional, Future)
- **Build**: GitHub Actions or Azure Pipelines (dotnet build, dotnet test)
- **Test**: xUnit tests (unit tests only; no integration tests needed)
- **Publish**: Self-contained .exe artifact
- **Deploy**: Copy to shared network folder or server

---

## Technology Stack Decisions

### Frontend
**Chosen**: Blazor Server (C# + HTML + CSS)
- **Justification**: Native HTML/CSS rendering for pixel-perfect screenshot quality; no JavaScript framework complexity; full C# in the browser reduces context switching
- **Alternative Considered**: React/Vue.js (rejected: adds Node.js build pipeline, increases bundle size, reduces screenshot determinism)

**Styling**: Inline CSS + extracted dashboard.css (no Tailwind, no SASS)
- **Justification**: OriginalDesignConcept.html CSS is production-ready; minimal tooling required
- **File Organization**: All colors, fonts, and layouts in single dashboard.css file

**Component Architecture**: Razor component hierarchy
- **Root**: Dashboard.razor (page)
- **Children**: Header.razor, TimelineChart.razor, HeatmapGrid.razor (reusable sub-components)
- **Justification**: Encourages separation of concerns; each component owns its section of the UI

### Backend
**Chosen**: C# .NET 8 LTS (ASP.NET Core)
- **Justification**: Matches organizational skillset; long-term support; performance-optimized
- **Minimum Framework**: net8.0

**Configuration Management**: IConfiguration + appsettings.json
- **Data File Path**: Configurable per environment (appsettings.Development.json, appsettings.Production.json)
- **Logging**: Built-in Microsoft.Extensions.Logging (no Serilog or third-party libraries)

### Data Handling
**Chosen**: System.Text.Json (built-in .NET 8)
- **Justification**: No external NuGet dependency; integrated with ASP.NET Core; sufficient for simple schema
- **Deserialization**: POCO objects with [JsonPropertyName] attributes

**Date/Time Handling**: NodaTime 4.1+
- **Justification**: Handles calendar math (month boundaries, leap years) without DateTime complexity
- **Alternative Considered**: Built-in DateTime (rejected: more error-prone for month calculations)
- **Functions Used**: YearMonth, LocalDate, Period arithmetic

**Data Storage**: Local file system (data.json)
- **Justification**: No infrastructure complexity; human-readable; version-controllable
- **File Format**: UTF-8 JSON
- **Location**: wwwroot/data/data.json (web-accessible but not served as static asset)

### Testing
**Chosen**: xUnit 2.x (unit tests only)
- **Justification**: Industry standard in .NET; clean assertion syntax; integration with Visual Studio
- **Test Scope**: Data deserialization, date calculations, color/CSS mappings
- **No Integration Tests**: Dashboard is stateless and read-only; no database, API, or external service to test

**Visual Regression Testing** (Optional, Future):
- **Tool**: Playwright.NET or Selenium
- **Approach**: Automated screenshot comparison against baseline PNG
- **Justification**: Ensures visual consistency across machines; catches CSS regressions

### Web Server & Hosting
**Chosen**: Kestrel (built-in)
- **Justification**: Ships with .NET 8 SDK; zero additional infrastructure; sufficient for internal network
- **Port**: 5000 (default)
- **Binding**: 0.0.0.0 (listen on all interfaces)
- **HTTPS**: Optional; self-signed certificate on localhost only

**Alternative Considered**: IIS (rejected for MVP: adds Windows Server management overhead; Kestrel is simpler)

### Deployment
**Chosen**: Self-contained .exe (dotnet publish)
- **Command**: `dotnet publish -c Release -r win-x64 --self-contained`
- **Justification**: Single executable with embedded .NET 8 runtime; no system-wide .NET SDK required on target machine
- **Distribution**: Copy .exe + appsettings.json + wwwroot/ to shared network folder
- **Size**: ~120 MB (acceptable for internal network)

### IDE & Tools
- **IDE**: Visual Studio 2022 Community (or JetBrains Rider)
- **.NET SDK**: 8.0.x (latest LTS)
- **Package Manager**: NuGet (via Visual Studio or dotnet CLI)
- **Version Control**: Git
- **Build System**: .NET CLI (dotnet build, dotnet run, dotnet test, dotnet publish)

### Excluded Technologies
- **Authentication Libraries**: No identity services (open access)
- **ORM/Database**: No Entity Framework, Dapper, or SQL (file-based only)
- **Caching**: No Redis, no in-memory cache (stateless rendering)
- **Message Queues**: No RabbitMQ, Service Bus (no async background tasks)
- **Cloud Services**: No Azure, AWS, GCP (local/network only)
- **Real-time Updates**: No SignalR (future enhancement if FileSystemWatcher auto-refresh needed)
- **CSS Frameworks**: No Tailwind, Bootstrap, or Foundation (custom CSS)
- **JavaScript Libraries**: No jQuery, D3.js, Chart.js (SVG hand-coded in Blazor)
- **API Documentation**: No Swagger/OpenAPI (no public API)

---

## Security Considerations

### Authentication & Authorization
- **Model**: No authentication required
- **Access Control**: Open to anyone with network access to port 5000
- **Rationale**: Dashboard is internal-only, read-only executive tool; no sensitive operations
- **Future Enhancement**: If multi-user or role-based access needed, implement JWT bearer token validation (no database lookup)

### Data Protection
- **Data Sensitivity**: Low (project status strings, milestone names; no PII, passwords, credentials)
- **Encryption at Rest**: Not required; data.json is unencrypted plaintext
- **Encryption in Transit**: HTTP (Kestrel default); HTTPS optional with self-signed certificate
- **Rationale**: Internal network only; no internet-facing exposure

### Input Validation
- **JSON Schema Validation**: Validate data.json structure on load
  - Required fields: projectName, description, quarters[], milestones[]
  - Field types: strings, integers, arrays
  - Optional fields allowed (future extensibility)
- **Data Constraints**:
  - Month names must be valid (January-December)
  - Years must be in range 2000-2099
  - Dates must be valid ISO 8601 (YYYY-MM-DD)
  - Milestone IDs must be unique
- **Error Handling**: If validation fails, display user-friendly message (not stack trace)

### Output Encoding
- **HTML Escaping**: Razor handles all @variable interpolation; no manual HTML encoding needed
- **SVG**: All SVG generated via VisualizationService with safe coordinate injection (numbers only)
- **CSS Class Names**: Map to hardcoded class list; no user input in class names

### Code Security
- **No SQL Injection**: Application uses no SQL queries
- **No CSRF**: No state-changing operations; read-only dashboard
- **No XSS**: Razor escapes all user input; no inline JavaScript
- **Secrets Management**: No API keys, connection strings, or credentials in code
  - appsettings.json committed to repo (only contains file path and port; no secrets)
  - Deployment environment variables unused for MVP

### Network Security
- **Port Binding**: 0.0.0.0:5000 (internal network only)
- **Firewall Rules**: Restrict port 5000 to trusted subnets; block all inbound from internet
- **Certificate**: Self-signed HTTPS optional for MITM protection on untrusted networks
- **Assumptions**:
  - Network is trusted (corporate LAN)
  - No confidential project data in dashboard
  - Users are authenticated to network (not dashboard)

### File System Security
- **data.json Permissions**: Application runs with restricted account (no admin privileges)
- **Read-Only Access**: Application reads data.json on startup; no write operations
- **Path Validation**: Reject relative paths outside wwwroot/ (no directory traversal)
- **Backup**: Keep data.json in Git for version history and recovery

### Logging & Monitoring
- **Logged Data**: Application startup, errors, configuration paths (no user data or secrets)
- **Log Level**: Warning (production), Debug (development)
- **Log Destination**: Console output (stdout) captured by system logs
- **No PII**: Logs contain no project-specific data or personal information
- **Retention**: No centralized log storage; application logs to console only

### Future Security Enhancements (Post-MVP)
1. **HTTPS with self-signed certificate** (when deploying to untrusted networks)
2. **API key validation** (if data.json endpoint is exposed separately)
3. **Request rate limiting** (if dashboard experiences high traffic)
4. **Audit logging** (track who accessed dashboard and when)

---

## Scaling Strategy

### Current Scope (MVP)
- **Concurrent Users**: 10+ on same local network (stateless design supports this)
- **Data Size**: Up to 50 items per status type across 12 months
- **Heatmap Display**: 4-6 month columns (fixed window)
- **Timeline Range**: Jan-Jun (6 months, fixed)
- **File Size**: data.json < 10 KB

### Scaling Limitations & Mitigations

#### 1. **Heatmap Column Scaling** (Beyond 12 Months)
**Problem**: CSS Grid with 4 fixed columns cannot display > 12 months without pagination or truncation

**Mitigation**:
- **Short-term**: Limit display to rolling 6-month window (previous 1 month + current month + next 4 months)
- **Medium-term**: Implement month navigation (previous/next quarter buttons)
- **Long-term**: Horizontal scrollable grid or paginated view (post-MVP)

**Implementation**: Modify DateCalculationService to accept month offset parameter
```csharp
public List<MonthInfo> GetDisplayMonths(DateTime baseDate, int monthOffset = 0)
{
    // Shift 4-month window by monthOffset
    // Return quarters matching shifted window
}
```

#### 2. **Item Count Scaling** (Heatmap Cell Overflow)
**Problem**: More than 4 items per cell causes text overflow; truncation or tooltips needed

**Current Behavior**: Up to 4 items per cell fit within visible area

**Mitigation**:
- **Short-term**: Truncate to 3-4 items; add "... +N more" link (display overflow count)
- **Medium-term**: Hover tooltip showing all items (JavaScript event listener)
- **Long-term**: Click to expand modal with full item list and details

**Implementation**: Modify HeatmapGrid.razor to check item count
```csharp
@if (items.Count > 4)
{
    <span class="overflow-indicator">... +@(items.Count - 3) more</span>
}
```

#### 3. **Milestone Count Scaling** (Timeline Congestion)
**Problem**: More than 10 milestones causes timeline crowding; text/shape overlap

**Current Behavior**: Support 3-10 milestones across 6-month timeline

**Mitigation**:
- **Short-term**: Increase timeline height (adjust SVG height from 185px to 250px+)
- **Medium-term**: Distribute milestones across multiple timeline rows (stack vertically)
- **Long-term**: Zoom/pan controls for timeline detail

**Implementation**: Modify TimelineChart.razor to increase SVG height
```csharp
<svg width="1560" height="@(MilestoneCount * 50 + 50)" ...>
```

#### 4. **Concurrent User Scaling** (Network Bandwidth)
**Problem**: 100+ concurrent users on slow network (< 1 Mbps) may experience page load delays

**Mitigation**:
- **Short-term**: Optimize Blazor bundle size (already small; ~5 MB for dotnet 8 core)
- **Medium-term**: CDN for static assets (css, images) on larger networks
- **Long-term**: Load balancing with multiple application instances (requires state sync)

**Assumption**: MVP assumes internal corporate network with 10 Mbps minimum; no scaling concern for typical 10-50 concurrent users

#### 5. **Data.json File Size Scaling**
**Current**: < 10 KB typical

**Limits**:
- 50 items per status type per month × 4 status types × 12 months = 2,400 items (max)
- ~200 bytes per item average (name + type) = 480 KB worst-case
- Still easily handled by System.Text.Json

**Mitigation**: No action needed; JSON deserialization is fast for files < 10 MB

### Performance Optimizations (If Needed)
1. **Lazy-load Blazor scripts** (JavaScript interop only if needed; future)
2. **Compress wwwroot assets** (CSS, images) with gzip
3. **Minimize SVG regeneration** (cache generated SVG in memory between renders)
4. **Pagination for milestones** (display only 5 most relevant milestones if > 10)
5. **Connection pooling** (if future database added; not applicable to MVP)

### Load Testing Plan
1. **Baseline**: Measure page load time and memory usage with sample data (1 project, 4 quarters, 3 milestones)
2. **Stress Test**: Simulate 50 concurrent users accessing dashboard
3. **Scale Test**: Max out data.json (50 items per status, 12 months, 10 milestones)
4. **Measurements**: Page load time, server CPU, memory usage, error rates
5. **Success Criteria**: Page load < 2 seconds, < 100 MB memory, zero 500 errors at 50 concurrent users

---

## Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| **data.json becomes corrupted** | Low | High (dashboard blank) | (1) Validate schema on load; (2) Keep backup in Git; (3) Display clear error message with recovery steps |
| **File modification time inaccurate** | Low | Low (timestamp shows wrong time) | Use UTC timestamp; verify system clock is correct |
| **Timeline SVG coordinates miscalculated** | Medium | Medium (milestones misaligned) | Write unit tests for date-to-coordinate conversion; verify with known test dates; manual visual inspection |
| **Heatmap cell colors don't match design** | Medium | Medium (visual mismatch) | Extract all colors to VisualizationService constants; compare hex values against OriginalDesignConcept.html; automated visual regression testing |
| **Screenshot not reproducible** | Low | High (PowerPoint exports differ) | All rendering is deterministic (no animations, random numbers, or lazy-loading); use Playwright automated screenshot test to verify baseline stability |
| **Performance degradation at 50+ concurrent users** | Low | Medium (slow page loads) | Optimize Blazor bundle size; monitor memory usage; implement load testing before production |
| **data.json path incorrect in appsettings.json** | Medium | High (file not found error) | Default path is relative to app root; validate path exists on startup; provide clear error message with troubleshooting steps |
| **Month names in data.json don't match expected format** | Medium | Medium (heatmap headers misaligned) | Validate month names against hardcoded list (January-December); reject invalid months with error message |
| **Milestone date format invalid (not ISO 8601)** | Medium | High (date parsing fails) | Use DateTime.Parse with strict format ("yyyy-MM-dd"); catch FormatException and report error with expected format |
| **Browser cache stale after data.json update** | Low | Low (user sees old data) | Add cache-busting headers (Cache-Control: no-cache); document user should do Ctrl+F5 hard refresh |
| **CSS class name conflicts** | Low | Low (styling broken) | All classes prefixed with .hm- or .tl- to avoid conflicts; no global class names |
| **SVG filter effects not supported** | Low | Low (drop shadows missing) | Graceful degradation: filter is optional; milestones still visible without shadow |
| **Timezone confusion in "Now" marker** | Medium | Low (marker off by hours) | Always use UTC; store and calculate in UTC; clearly label "Last Updated" as UTC time |

### Top 3 Critical Risks
1. **data.json corruption or parsing error** → Mitigation: Schema validation + clear error message
2. **Timeline coordinate miscalculation** → Mitigation: Unit tests + manual visual verification
3. **Screenshot irreproducibility** → Mitigation: Deterministic rendering + automated screenshot baseline test

### Risk Management Plan
- **Weekly**: Manual visual inspection of dashboard against OriginalDesignConcept.html
- **Per Release**: Run full test suite (unit tests + screenshot baseline tests)
- **Per Deployment**: Backup data.json before overwriting; test on staging network first
- **Documentation**: Troubleshooting guide for common errors (missing file, invalid JSON, color mismatch)

---

## Deployment Checklist

### Pre-Deployment
- [ ] All unit tests pass (xUnit)
- [ ] Code review completed (style, security, logic)
- [ ] Visual regression tests pass (screenshot matches baseline)
- [ ] data.json example validates against schema
- [ ] appsettings.json configured for target environment
- [ ] Documentation complete (data.json schema, deployment steps, troubleshooting)

### Deployment Steps
1. **Build**: `dotnet publish -c Release -r win-x64 --self-contained`
2. **Copy Artifacts**:
   - AgentSquad.Runner.exe → target machine
   - wwwroot/ folder → target machine
   - appsettings.json → target machine (customize for environment)
   - appsettings.Development.json (optional, for dev only)
3. **Verify File Structure**:
   - wwwroot/css/dashboard.css exists
   - wwwroot/data/data.json exists (or create from template)
   - appsettings.json points to correct data file path
4. **Start Application**: `AgentSquad.Runner.exe` (or double-click)
5. **Verify Accessibility**: Open http://localhost:5000 in browser
6. **Test Data Load**: Verify header shows project name, timeline renders milestones, heatmap shows items
7. **Screenshot Test**: Press Print Screen; verify output is readable and matches design

### Post-Deployment
- [ ] Application listening on port 5000
- [ ] Dashboard loads in < 2 seconds
- [ ] All data from data.json displays correctly
- [ ] No console errors in browser dev tools
- [ ] Screenshot exports cleanly to PowerPoint
- [ ] Stakeholders receive access instructions (URL, troubleshooting contact)

### Troubleshooting
| Error | Cause | Solution |
|-------|-------|----------|
| "Unable to load project data. Please check that data.json exists..." | File not found | Verify wwwroot/data/data.json exists; check path in appsettings.json |
| "Invalid JSON in data.json" | Malformed JSON | Use online JSON validator; check for missing commas, quotes |
| "data.json schema validation failed" | Missing required fields | Ensure projectName, description, quarters, milestones fields present |
| Timeline milestones not visible | Date parsing error | Verify dates are ISO 8601 format (YYYY-MM-DD) |
| Heatmap colors don't match design | CSS conflict | Clear browser cache (Ctrl+F5); verify dashboard.css is loaded |
| "Address already in use" on port 5000 | Port conflict | Change port in appsettings.json; restart application |
| Slow page load (> 5 seconds) | Network latency or large data.json | Check network speed; reduce data.json size if > 100 KB |

---

## Summary

The **My Project** reporting dashboard is a stateless, deterministic Blazor Server application designed for simplicity and screenshot reproducibility. By eliminating databases, authentication, caching, and external dependencies, the architecture prioritizes operational stability and developer maintainability over feature richness. The data model is flexible JSON, services are independently testable, and the UI is a direct port of OriginalDesignConcept.html. This design enables rapid development (2 weeks MVP), easy data updates (edit data.json), and reliable PowerPoint exports (pixel-perfect rendering). Scaling is possible through pagination and UI enhancements if needed, but the MVP assumes small datasets (< 50 items per status) and 10-50 concurrent users on a corporate network.