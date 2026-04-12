# Architecture

## Overview & Goals

The Executive Project Reporting Dashboard is a single-page, desktop-optimized web application built with Blazor Server (.NET 8) that provides executives with real-time visibility into project milestones and monthly execution status. The system reads project data from a JSON configuration file (`data.json`) and renders an interactive, print-friendly dashboard matching the OriginalDesignConcept.html design specification.

**Key architectural goals:**
- **Simplicity**: Minimize dependencies and operational complexity. No database, authentication, or cloud infrastructure required.
- **Portability**: Package as a single self-contained `.exe` file for distribution to any Windows machine without installation prerequisites.
- **Visual fidelity**: Achieve pixel-perfect match to OriginalDesignConcept.html design at 1920x1080 resolution.
- **Print optimization**: Ensure dashboard layout and colors render identically in browser print preview and screenshot capture.
- **Maintainability**: Use modular Blazor component architecture with clear separation of concerns for future enhancements.
- **Data flexibility**: Support configuration of milestones, status rows, and timeline data through a simple JSON schema.

---

## System Components

### 1. **Dashboard Root Component** (`Dashboard.razor`)

**Responsibility:** Orchestrate page layout and compose child components; manage top-level state and data loading.

**Key responsibilities:**
- Load `ReportData` from `ReportService` on initialization
- Render page structure: header, timeline, and heatmap sections
- Pass `ReportData` as cascading parameter to child components
- Handle errors from service (file not found, invalid JSON, null data)
- Display error or loading states if necessary

**Interfaces:**
- **Dependency:** Injects `IReportService`
- **Cascading parameter:** `ReportData` passed to children
- **Lifecycle:** `OnInitializedAsync()` calls `ReportService.LoadReportDataAsync()`

**Data inputs:** None from user; loads JSON file via service

**Output:** HTML page structure with child components

---

### 2. **Header Component** (`Header.razor`)

**Responsibility:** Display project title, subtitle, ADO backlog link, and legend explaining visual symbols.

**Key responsibilities:**
- Render page title (24px, bold, from `ReportData.ReportTitle`)
- Render subtitle (12px, gray, from `ReportData.Subtitle`)
- Render clickable ADO backlog link (blue #0078D4, from `ReportData.AdoBacklogUrl`)
- Render legend with four symbols and labels:
  - Yellow diamond (12x12px rotated square, #F4B400) + "PoC Milestone"
  - Green diamond (12x12px rotated square, #34A853) + "Production Release"
  - Gray circle (8x8px, #999) + "Checkpoint"
  - Red vertical line (2x14px, #EA4335) + "Now (current date)"
- Maintain fixed position and proper spacing per OriginalDesignConcept.html

**Interfaces:**
- **Cascading parameter:** `ReportData` (read-only, uses `ReportTitle`, `Subtitle`, `AdoBacklogUrl`)
- **Child components:** None

**Data inputs:** 
- `ReportData.ReportTitle` (string)
- `ReportData.Subtitle` (string)
- `ReportData.AdoBacklogUrl` (URL string)

**Output:** HTML header section with title, subtitle, link, and legend

**CSS classes:** `.hdr`, `.hdr h1`, `.sub`, legend flex container

---

### 3. **Timeline Component** (`Timeline.razor`)

**Responsibility:** Render milestone timeline with SVG visualization, dates, and status markers.

**Key responsibilities:**
- Load milestone definitions from `ReportData.Milestones`
- Generate SVG string dynamically for:
  - Month gridlines (vertical lines at calculated positions for Jan, Feb, Mar, Apr, May, Jun)
  - Month labels (text positioned above gridlines)
  - "Now" indicator (red dashed vertical line at current month position, color #EA4335)
  - Milestone lines (horizontal colored lines, stroke-width 3px, color per milestone)
  - Start circle (white fill with milestone color stroke, r=7px at milestone start date)
  - Checkpoints (smaller circles, r=4-5px, positioned at checkpoint dates)
  - PoC milestone markers (yellow diamonds, 12x12px rotated squares, #F4B400, drop-shadowed)
  - Production release markers (green diamonds, #34A853, drop-shadowed)
  - Date labels (text positioned relative to markers, 10px font, #666)
- Render milestone label panel on left (M1, M2, M3 with titles and colors)
- Calculate x-coordinates for dates using linear date-to-pixel mapping
- Handle edge cases: missing dates, checkpoints beyond timeline range, null markers

**Interfaces:**
- **Cascading parameter:** `ReportData` (read-only, uses `Milestones`, derives "Now" date from current month)
- **Child components:** None
- **Dependency (optional):** `ITimelineCalculationService` for date-to-coordinate mapping

**Data inputs:**
- `ReportData.Milestones[].Id` (string, e.g., "M1")
- `ReportData.Milestones[].Title` (string, e.g., "Chatbot & MS Role")
- `ReportData.Milestones[].Color` (hex color string, e.g., "#0078D4")
- `ReportData.Milestones[].StartDate` (DateTime)
- `ReportData.Milestones[].Checkpoints[]` (DateTime, optional label)
- `ReportData.Milestones[].PocMilestone` (DateTime, label, color #F4B400)
- `ReportData.Milestones[].ProductionRelease` (DateTime, label, color #34A853)

**Output:** SVG timeline markup rendered as `MarkupString` in HTML

**Key methods:**
- `GenerateTimelineSvg()` - Builds SVG string using StringBuilder
- `DateToPixel(DateTime date)` - Maps date to x-coordinate (accounting for month offset from timeline start)
- `GetMonthPosition(int monthIndex)` - Calculates x position of month gridline (260px spacing)
- `GetNowLinePosition()` - Calculates x position of "Now" line (current date relative to timeline range)

**CSS classes:** `.tl-area`, `.tl-svg-box`, SVG with inline styling

---

### 4. **Heatmap Component** (`Heatmap.razor`)

**Responsibility:** Render status grid showing work items by month and status category (Shipped, In Progress, Carryover, Blockers).

**Key responsibilities:**
- Render CSS Grid layout (160px + 4 equal-width columns, 36px header row + 4 data rows)
- Render corner cell ("Status" label, background #F5F5F5)
- Render column headers (month names: "March", "April (Now)", "May", "June"; April column has #FFF0D0 background)
- Iterate `ReportData.StatusRows` and render row headers with status-specific colors and CSS classes
- Iterate month columns and populate cells with work items from `StatusRow.Items` filtered by month
- Pass each cell data to `HeatmapCell` child component
- Apply status-specific CSS classes for color coding (`.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`)

**Interfaces:**
- **Cascading parameter:** `ReportData` (read-only, uses `StatusRows`, derives month list from items)
- **Child components:** `HeatmapCell.razor` (one per cell)

**Data inputs:**
- `ReportData.StatusRows[].Category` (string: "Shipped", "In Progress", "Carryover", "Blockers")
- `ReportData.StatusRows[].Items[]` (objects with `Month` and `Value` strings)
- `ReportData.StatusRows[].HeaderCssClass` (string: "ship-hdr", "prog-hdr", "carry-hdr", "block-hdr")
- `ReportData.StatusRows[].CellCssClass` (string: "ship-cell", "prog-cell", "carry-cell", "block-cell")
- `ReportData.StatusRows[].HeaderColor` (hex color string, applied to header text)

**Output:** HTML CSS Grid layout with rendered cells

**Key methods:**
- `GetMonthsFromData()` - Extracts unique month names from all status items, returns ordered list
- `GetItemsForCell(StatusRow row, string month)` - Filters items by month, returns list or empty
- `GetCellCssClass(string baseClass, bool isAprilMonth)` - Combines base class with `.apr` modifier if applicable

**CSS classes:** `.hm-wrap`, `.hm-title`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`, status-specific classes

---

### 5. **HeatmapCell Component** (`HeatmapCell.razor`)

**Responsibility:** Render individual heatmap cell with work items and status-specific styling.

**Key responsibilities:**
- Render single grid cell with appropriate CSS classes
- Iterate items list and render each as `<div class="it">` with colored circle bullet
- Render "-" (dash) in light gray if items list is empty
- HTML-escape item values to prevent XSS vulnerabilities
- Apply status-specific color to circle bullet (via `:before` pseudo-element)

**Interfaces:**
- **Parameters (inputs):** 
  - `string CellCssClass` (e.g., "ship-cell", "prog-cell")
  - `List<StatusItem> Items` (work items for this cell)
- **Child components:** None

**Data inputs:**
- `Items[].Value` (string, work item description)

**Output:** HTML cell div with item list or empty indicator

**Key methods:**
- `RenderItems()` - Returns list of `<div>` elements for each item, or single "-" element if empty

**CSS classes:** `.hm-cell`, `.it`, `:before` pseudo-element for colored bullet

---

### 6. **ReportService** (Service class)

**Responsibility:** Load and deserialize `data.json` file; expose report data to components; handle errors.

**Key responsibilities:**
- Load JSON file from `wwwroot/data/data.json` (or path from `appsettings.json`)
- Deserialize JSON string into `ReportData` object using `System.Text.Json`
- Validate data structure and provide sensible defaults (e.g., empty items list if missing)
- Catch and log JSON parsing errors; return error message to UI
- Cache deserialized `ReportData` in memory (singleton pattern)
- Optionally watch file system for changes and reload (Phase 2 enhancement)

**Interfaces:**
- **Public method:** `Task<ReportData> LoadReportDataAsync()`
  - **Returns:** `ReportData` object or throws exception with descriptive error message
  - **Exceptions:** `FileNotFoundException`, `JsonException` (caught and wrapped)
- **Public property:** `ReportData CurrentData` (cached after first load)

**Dependencies:**
- `IWebHostEnvironment` (injected, provides `wwwroot` path)
- `ILogger<ReportService>` (injected, logs errors)
- `IConfiguration` (injected, reads data file path from `appsettings.json`)

**Data flow:**
1. Component calls `LoadReportDataAsync()`
2. Service checks if data is already cached; return if yes
3. Service reads `data.json` file from disk
4. Service deserializes JSON string to `ReportData` using `JsonSerializer.Deserialize<ReportData>(jsonString, options)`
5. Service applies defaults (empty lists for null properties)
6. Service validates dates and calculates derived fields (if needed)
7. Service caches result and returns to component

**Error handling:**
- File not found → Log error, throw `FileNotFoundException` with friendly message
- Invalid JSON → Log error, throw `JsonException` with line/column information
- Null/missing fields → Apply defaults, log warning
- Invalid date format → Log error, skip invalid date (or throw if critical)

**Configuration:**
- Read data file path from `appsettings.json` setting: `"DataFilePath": "wwwroot/data/data.json"`
- Default fallback: `"wwwroot/data/data.json"`

---

### 7. **DesignConstants Class** (Utility)

**Responsibility:** Centralize color palette, grid dimensions, and other design-related constants.

**Key responsibilities:**
- Define color palette as constants (ship green, progress blue, carryover amber, blocker red)
- Define CSS class names for status rows
- Define grid dimensions (column width, row height, gap)
- Define typography constants (font sizes, weights)
- Define timeline parameters (month width in pixels, total timeline width, height)
- Provide utility methods for color lookups by status

**Properties:**
```csharp
// Colors
public const string ColorShipped = "#1B7A28";
public const string ColorProgress = "#1565C0";
public const string ColorCarryover = "#B45309";
public const string ColorBlocker = "#991B1B";
public const string ColorPoc = "#F4B400";
public const string ColorProduction = "#34A853";
public const string ColorNow = "#EA4335";

// Background colors
public const string BgShipped = "#E8F5E9";
public const string BgProgress = "#E3F2FD";
public const string BgCarryover = "#FFF8E1";
public const string BgBlocker = "#FEF2F2";

// CSS classes
public const string CssShipHdr = "ship-hdr";
public const string CssProgHdr = "prog-hdr";
// ... etc

// Grid layout
public const int HeatmapRowHeaderWidth = 160;
public const int HeatmapHeaderRowHeight = 36;
public const int HeatmapDataRowHeight = 80; // approximately

// Timeline
public const int TimelineMonthWidth = 260;
public const int TimelineSvgWidth = 1560;
public const int TimelineSvgHeight = 185;
```

**Methods:**
- `GetColorByStatus(string status)` → Returns hex color code
- `GetCssClassByStatus(string status)` → Returns CSS class name

---

## Component Interactions

### Data Flow Diagram

```
Browser HTTP Request
    ↓
Blazor Server App (Program.cs initializes DI)
    ↓
Dashboard.razor loads
    ↓
Dashboard.OnInitializedAsync()
    ↓
Calls ReportService.LoadReportDataAsync()
    ↓
ReportService reads wwwroot/data/data.json
    ↓
System.Text.Json deserializes to ReportData
    ↓
ReportService returns ReportData (cached)
    ↓
Dashboard receives ReportData
    ↓
Dashboard renders child components as cascading parameters
    ├─→ Header.razor (receives ReportData)
    │   └─→ Renders title, subtitle, legend (static HTML)
    │
    ├─→ Timeline.razor (receives ReportData)
    │   ├─→ Iterates Milestones
    │   ├─→ Generates SVG string dynamically
    │   ├─→ Calculates month positions, date-to-pixel mapping
    │   └─→ Renders SVG with lines, circles, diamonds, labels
    │
    └─→ Heatmap.razor (receives ReportData)
        ├─→ Iterates StatusRows
        ├─→ Extracts unique months from items
        ├─→ Renders CSS Grid layout
        ├─→ For each (row, month) combination:
        │   └─→ HeatmapCell.razor (receives Items list)
        │       └─→ Renders items with colored bullets or "-"
        └─→ Applies status-specific CSS classes for colors
```

### Component Communication

**Parent → Child (Cascading Parameters):**
- `Dashboard` passes `ReportData` to `Header`, `Timeline`, `Heatmap`
- `Heatmap` passes `Items` list to `HeatmapCell`

**Service → Components (Dependency Injection):**
- `Dashboard` injects `ReportService` to load data
- `ReportService` injects `IWebHostEnvironment` to locate `data.json`
- `ReportService` injects `ILogger<ReportService>` for error logging
- `ReportService` injects `IConfiguration` to read settings

**Async Data Loading:**
- `Dashboard.OnInitializedAsync()` awaits `ReportService.LoadReportDataAsync()`
- Once awaited, `StateHasChanged()` is called implicitly by Blazor to re-render
- Child components receive data via cascading parameters and render synchronously

**Error Propagation:**
- If `ReportService` throws exception during load, Dashboard catches it (in try-catch within `OnInitializedAsync`)
- Dashboard sets error state and renders error message component instead of full layout
- User sees friendly error message (e.g., "Error loading dashboard data. Please check data.json is valid.")

---

## Data Model

### ReportData (Root Object)

```csharp
public class ReportData
{
    [JsonPropertyName("reportTitle")]
    public string ReportTitle { get; set; }

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; }

    [JsonPropertyName("adoBacklogUrl")]
    public string AdoBacklogUrl { get; set; }

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("statusRows")]
    public List<StatusRow> StatusRows { get; set; } = new();
}
```

### Milestone

```csharp
public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } // Hex color: #0078D4

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("checkpoints")]
    public List<Checkpoint> Checkpoints { get; set; } = new();

    [JsonPropertyName("pocMilestone")]
    public MilestoneMarker PocMilestone { get; set; }

    [JsonPropertyName("productionRelease")]
    public MilestoneMarker ProductionRelease { get; set; }
}
```

### Checkpoint

```csharp
public class Checkpoint
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }
}
```

### MilestoneMarker

```csharp
public class MilestoneMarker
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } // Hex color: #F4B400 (PoC) or #34A853 (Prod)
}
```

### StatusRow

```csharp
public class StatusRow
{
    [JsonPropertyName("category")]
    public string Category { get; set; } // "Shipped", "In Progress", "Carryover", "Blockers"

    [JsonPropertyName("headerCssClass")]
    public string HeaderCssClass { get; set; } // "ship-hdr", "prog-hdr", "carry-hdr", "block-hdr"

    [JsonPropertyName("cellCssClass")]
    public string CellCssClass { get; set; } // "ship-cell", "prog-cell", "carry-cell", "block-cell"

    [JsonPropertyName("headerColor")]
    public string HeaderColor { get; set; } // Hex color for header text

    [JsonPropertyName("items")]
    public List<StatusItem> Items { get; set; } = new();
}
```

### StatusItem

```csharp
public class StatusItem
{
    [JsonPropertyName("month")]
    public string Month { get; set; } // e.g., "March", "April", "May", "June"

    [JsonPropertyName("value")]
    public string Value { get; set; } // e.g., "API v1 Released", null if empty
}
```

### Data Validation Rules

- **ReportData.ReportTitle**: Required, non-empty string (min length 1, max 255)
- **ReportData.Subtitle**: Optional, string (max 500 characters)
- **ReportData.AdoBacklogUrl**: Optional, valid URL format (validates as URI)
- **Milestone.Id**: Required, unique within list, alphanumeric + hyphens (e.g., "M1", "M2-Phase2")
- **Milestone.Title**: Required, non-empty string (max 100 characters)
- **Milestone.Color**: Required, valid hex color format (#RRGGBB)
- **Milestone.StartDate**: Required, valid ISO 8601 date (YYYY-MM-DD)
- **Checkpoint.Date**: Required, valid ISO 8601 date, >= Milestone.StartDate
- **MilestoneMarker**: Optional fields; if present, Date must be valid
- **StatusRow.Category**: Required, one of predefined values ("Shipped", "In Progress", "Carryover", "Blockers")
- **StatusRow.Items**: Optional, empty list acceptable
- **StatusItem.Month**: Required, string (e.g., "March", "April")
- **StatusItem.Value**: Optional, string (null treated as empty cell, display "-")

### Data Storage

- **Format:** JSON (plain text file)
- **Location:** `wwwroot/data/data.json` (served as static file by Blazor Server)
- **Encoding:** UTF-8
- **File permissions:** Read-only for end users; writable for project managers via text editor
- **Backup:** No automatic backup by application; user is responsible (via Git or file backup)
- **Concurrency:** No file locking; concurrent writes may cause data loss (acceptable risk for MVP)

### Example data.json

```json
{
  "reportTitle": "Privacy Automation Release Roadmap",
  "subtitle": "Trusted Platform – Privacy Automation Workstream – April 2026",
  "adoBacklogUrl": "https://dev.azure.com/...",
  "milestones": [
    {
      "id": "M1",
      "title": "Chatbot & MS Role",
      "color": "#0078D4",
      "startDate": "2026-01-12",
      "checkpoints": [],
      "pocMilestone": {
        "date": "2026-03-26",
        "label": "Mar 26 PoC",
        "color": "#F4B400"
      },
      "productionRelease": {
        "date": "2026-04-15",
        "label": "Apr Prod",
        "color": "#34A853"
      }
    },
    {
      "id": "M2",
      "title": "PDS & Data Inventory",
      "color": "#00897B",
      "startDate": "2025-12-19",
      "checkpoints": [
        {
          "date": "2026-02-11",
          "label": "Feb 11 Checkpoint"
        }
      ],
      "pocMilestone": {
        "date": "2026-03-20",
        "label": "Mar 20 PoC",
        "color": "#F4B400"
      },
      "productionRelease": {
        "date": "2026-03-30",
        "label": "Mar 30 Prod",
        "color": "#34A853"
      }
    }
  ],
  "statusRows": [
    {
      "category": "Shipped",
      "headerCssClass": "ship-hdr",
      "cellCssClass": "ship-cell",
      "headerColor": "#1B7A28",
      "items": [
        { "month": "March", "value": "Chatbot Phase 1" },
        { "month": "April", "value": "Data Inventory v1" },
        { "month": "May", "value": null },
        { "month": "June", "value": null }
      ]
    },
    {
      "category": "In Progress",
      "headerCssClass": "prog-hdr",
      "cellCssClass": "prog-cell",
      "headerColor": "#1565C0",
      "items": [
        { "month": "March", "value": "Integration testing" },
        { "month": "April", "value": "Performance optimization" },
        { "month": "May", "value": null },
        { "month": "June", "value": null }
      ]
    },
    {
      "category": "Carryover",
      "headerCssClass": "carry-hdr",
      "cellCssClass": "carry-cell",
      "headerColor": "#B45309",
      "items": [
        { "month": "May", "value": "Mobile app redesign" },
        { "month": "June", "value": null }
      ]
    },
    {
      "category": "Blockers",
      "headerCssClass": "block-hdr",
      "cellCssClass": "block-cell",
      "headerColor": "#991B1B",
      "items": []
    }
  ]
}
```

---

## API Contracts

### ReportService Interface

**Namespace:** `AgentSquad.Runner.Services`

#### Method: LoadReportDataAsync

```csharp
public async Task<ReportData> LoadReportDataAsync()
```

**Purpose:** Load and deserialize project dashboard data from JSON file.

**Request:** None (reads from configured file path)

**Response:**
```csharp
ReportData {
  string ReportTitle,
  string Subtitle,
  string AdoBacklogUrl,
  List<Milestone> Milestones,
  List<StatusRow> StatusRows
}
```

**Status Codes / Outcomes:**
- **Success (200):** Returns fully populated `ReportData` object
- **FileNotFound:** Throws `FileNotFoundException` with message "Data file not found at path: {path}"
- **InvalidJSON:** Throws `JsonException` with message "Invalid JSON in data file: {exception detail}"
- **ValidationError:** Throws `ArgumentException` with message "Invalid data: {field} {reason}"

**Error Response Format (in HTML error page):**
```html
<div class="error-container">
  <h2>Error loading dashboard data</h2>
  <p>{{ErrorMessage}}</p>
  <p style="font-size: 12px; color: #888;">
    Please check that data.json exists in wwwroot/data/ and contains valid JSON.
  </p>
</div>
```

**Caching:** Result is cached after first successful load (singleton pattern). Subsequent calls return cached data without re-reading file.

**Timeout:** No explicit timeout; file I/O uses default async file operations (typically complete within 100ms for files <1MB).

---

### HTTP Endpoints

**Base URL:** `http://localhost:5000` (Blazor Server default)

#### GET /

**Purpose:** Render main dashboard page

**Response:** HTML page with Blazor Server app bootstrap

**Status:** 200 OK (on success), 500 Internal Server Error (if ReportService fails during component initialization)

**Headers:**
```
Content-Type: text/html; charset=utf-8
Cache-Control: no-cache, no-store, must-revalidate
```

#### GET /css/app.css

**Purpose:** Main stylesheet (static file)

**Response:** CSS content matching OriginalDesignConcept.html design

**Status:** 200 OK

#### GET /_framework/blazor.server.js

**Purpose:** Blazor Server runtime JavaScript (auto-served by Blazor)

**Response:** Minified JavaScript bundle

**Status:** 200 OK

---

## Infrastructure Requirements

### Hosting Environment

**Target OS:** Windows 10, Windows 11, Windows Server 2019+

**Runtime:** .NET 8.0 runtime (included in self-contained `.exe`)

**Memory:** Minimum 200 MB RAM at rest; typical usage < 500 MB

**Disk:** 150 MB for `.exe` file + 10 KB for `data.json`

**CPU:** Single-core 2+ GHz sufficient; no heavy compute workload

**Network:** Not required; runs on localhost or intranet only

### Deployment Model

**Executable Type:** Self-contained, single-file Windows executable

**Build command:**
```powershell
dotnet publish -c Release --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=false
```

**Output:** `ReportingDashboard.exe` (~150 MB)

**Distribution:** Copy `.exe` and `data.json` to target machine; no installation needed

**Execution:** Double-click `.exe` or run from command line; browser opens automatically to `http://localhost:5000`

### File System Structure (Runtime)

```
C:\Users\<user>\
├── ReportingDashboard.exe          (executable, redistributable)
└── data.json                       (configuration file, editable)
```

Or in installed location:
```
C:\Program Files\AgentSquad\ReportingDashboard\
├── ReportingDashboard.exe
└── data.json
```

### Network Requirements

- **Local machine only (MVP):** No network access required; Blazor Server communicates via localhost loopback
- **Intranet deployment (optional):** If deployed to shared machine or intranet server:
  - Bind to port 5000 (default) or configured port
  - Intranet security via Windows Authentication or IP-based access control (handled externally)
  - No HTTPS required for intranet-only use
- **Firewall:** Localhost loopback (127.0.0.1) is not blocked by default Windows Defender

### File Permissions

**Windows NTFS ACLs:**
- `ReportingDashboard.exe`: Read + Execute for target users
- `data.json`: Read for end users; Read + Write for project managers
- Restrict access using standard Windows permission dialogs if sensitive data is added in future

### Monitoring & Logging

**Application Logs:** Directed to console output (visible in command prompt if run from terminal)

**Log level:** `Information` (default); configurable in `appsettings.json`

**Log destinations:**
- Console (stdout)
- File (optional, `appsettings.json` can configure Serilog)

**Metrics tracked (optional, Phase 2):**
- Page load time
- File read duration
- JSON deserialization duration
- Component render time

### CI/CD Pipeline (Optional)

**Build:** Manual via Visual Studio or `dotnet publish` command

**Testing:** Local unit tests via `dotnet test`

**Deployment:** Manual copy of `.exe` to target machine (or via network share, cloud storage, etc.)

**No automated deployment** required for MVP; single `.exe` simplifies distribution

---

## Technology Stack Decisions

### Frontend Framework: Blazor Server (ASP.NET Core 8.0)

**Decision:** Use Blazor Server for interactive HTML rendering with C# code-behind.

**Justification:**
- **Simplicity:** No need for separate JavaScript frontend framework; use C# and Razor templates for all logic
- **Component reusability:** Blazor components encapsulate HTML, CSS, and C# in single `.razor` files
- **Hot reload support:** Faster development iteration during build phase
- **Built-in DI:** Dependency injection native to .NET; simplifies service registration and testing
- **No WASM overhead:** Server-side rendering is simpler than client-side compilation of WebAssembly
- **Mature ecosystem:** Blazor Server is production-ready in .NET 8; excellent documentation and community support

**Alternative considered:** Blazor WebAssembly (WASM)
- **Rejected:** Requires client-side .NET runtime (~2 MB overhead); slower initial load; more complex deployment
- **WASM not needed** for this simple dashboard; server-side rendering is sufficient and simpler

**Alternative considered:** JavaScript framework (React, Vue, Angular)
- **Rejected:** Mandate is C# .NET 8; would require separate JavaScript build pipeline and learning curve for team
- **Blazor avoids:** Polyglot codebase, duplicate models/validation, separate frontend and backend repositories

---

### Rendering: CSS Grid + Flexbox (No UI Framework)

**Decision:** Use native CSS Grid and Flexbox layouts without Bootstrap, Tailwind, or Material UI.

**Justification:**
- **Design fidelity:** OriginalDesignConcept.html uses CSS Grid and Flexbox exclusively; native CSS ensures pixel-perfect match
- **Minimal payload:** No CSS framework overhead; single `app.css` file (~50 KB) vs. Bootstrap/Tailwind (~100+ KB minified)
- **Simplicity:** Single-page layout does not benefit from component library; custom CSS is cleaner
- **Print optimization:** Full control over print styles (`@media print`) without framework constraints
- **Maintenance:** Easy to modify colors, spacing, and layout by editing CSS variables and grid templates

**Alternative considered:** Bootstrap or Tailwind CSS
- **Rejected:** Additional complexity; pre-built components not needed for single-page dashboard
- **Rejected:** May impose design constraints (e.g., forced breakpoints) that conflict with fixed-width design

---

### Data Serialization: System.Text.Json

**Decision:** Use `System.Text.Json` (built-in to .NET 8) for JSON deserialization.

**Justification:**
- **Zero external dependencies:** Part of .NET Standard Library; no NuGet packages required
- **Performance:** Faster than Newtonsoft.Json in .NET 8 benchmarks; optimized for modern CPUs
- **Native support:** Full support for `[JsonPropertyName]` attributes, custom converters, and async deserialization
- **Simplicity:** Minimal configuration required; works out-of-the-box with POCO classes

**Alternative considered:** Newtonsoft.Json (JSON.NET)
- **Rejected:** Additional dependency; System.Text.Json is sufficient and faster in .NET 8
- **Newtonsoft slower:** Third-party library, not recommended for new projects targeting .NET 8

---

### SVG Generation: C# String Builders

**Decision:** Generate SVG markup dynamically in C# using `StringBuilder` and interpolated strings.

**Justification:**
- **No external dependencies:** SVG is plain text; no charting library required
- **Control:** Full control over coordinates, colors, and styling; can match design precisely
- **Simplicity:** Easier to debug than abstracted charting API; coordinate math is straightforward
- **Performance:** String concatenation is fast for timeline with <20 milestones

**Alternative considered:** Chart.js, D3.js, or Plotly
- **Rejected:** Adds JavaScript dependency and complexity; overkill for simple milestone timeline
- **Rejected:** Client-side charting libraries require browser rendering; SVG pre-generated on server is more reliable

**Alternative considered:** Razor components for SVG
- **Not recommended:** SVG is generated once and doesn't need interactive updates; string builder is cleaner

---

### Data Persistence: JSON File (No Database)

**Decision:** Store all data in `data.json` file; no SQL or NoSQL database.

**Justification:**
- **Zero infrastructure:** No server, no setup, no licensing
- **Version control:** JSON file can be committed to Git for historical tracking and rollback
- **Portability:** `data.json` is self-contained; can be copied independently of executable
- **Simplicity:** Project managers can edit with any text editor (Notepad, VS Code, etc.)
- **MVP scope:** Single user/small team does not require ACID guarantees or concurrent write handling

**Alternative considered:** SQL Server or PostgreSQL
- **Rejected:** Adds deployment complexity; requires database installation and credentials
- **Rejected:** Overkill for simple configuration file; JSON is sufficient and more portable

**Alternative considered:** Azure Cosmos DB or DynamoDB
- **Rejected:** Violates mandate of "no cloud services"; local-only deployment required

---

### Testing Framework: xUnit + Moq + bunit

**Decision:** Use xUnit for unit tests, Moq for mocking, bunit for Blazor component testing.

**Justification:**
- **xUnit:** Industry standard for .NET; excellent assertion library and test discovery
- **Moq:** Lightweight mocking for service dependencies; simplifies unit test isolation
- **bunit:** Specialized testing framework for Blazor components; supports async rendering, event handling, and parameter changes
- **All built-in:** No additional learning curve for .NET team

**Test coverage targets:**
- `ReportService` (JSON loading, deserialization): 100% coverage
- `Timeline` coordinate calculations: 100% coverage
- `Heatmap` cell rendering logic: 80% coverage
- Blazor components (render, event handling): 70% coverage (component testing is optional for MVP)

---

### Build & Deployment: dotnet publish

**Decision:** Use standard `dotnet publish` command to produce self-contained `.exe`.

**Justification:**
- **Built-in to .NET SDK:** No additional tooling required
- **Single command:** `dotnet publish -c Release --self-contained -p:PublishSingleFile=true`
- **Reproducible:** Produces identical executable on any machine with same .NET 8 SDK
- **Self-contained runtime:** Includes .NET 8 runtime in `.exe`; no prerequisites for end users

**Alternative considered:** GitHub Actions / Azure Pipelines
- **Not used for MVP:** Manual local builds are sufficient
- **Can be added later** if need for automated builds arises

---

## Security Considerations

### Data Classification

- **Report data (dashboard content):** Internal project metadata; not classified as confidential
- **User data:** None collected; no user IDs, IP addresses, or analytics
- **System data:** No passwords, API keys, or PII stored in `data.json`

### Input Validation

**JSON deserialization:**
- `System.Text.Json` validates JSON syntax automatically
- Custom validation rules enforce field types (string, DateTime) and ranges
- Invalid dates are caught and logged; friendly error message displayed to user

**HTML escaping:**
- All work item values, milestone titles, and subtitles HTML-escaped before rendering
- Uses Blazor's built-in `@Html.Encode()` or automatic escaping in `@` expressions
- Prevents XSS vulnerabilities from malicious JSON content

**Example safe rendering:**
```razor
@foreach (var item in items)
{
    <div class="it">@item.Value</div>  @* Automatically HTML-escaped *@
}
```

### Access Control

**No authentication required (MVP):**
- Dashboard is read-only; no sensitive operations
- Intended for internal use on trusted network only
- File-level permissions (Windows NTFS ACLs) control who can edit `data.json`

**Future enhancement (Phase 2):**
- If dashboard is exposed beyond intranet, implement Windows Authentication or Active Directory integration
- Use `[Authorize]` attribute on components to restrict access

### File System Security

**`data.json` permissions:**
- Set NTFS read-only for end users viewing dashboard
- Grant read-write to project managers who edit data
- Use Windows permission dialogs (Properties → Security tab) to configure ACLs

**Backup & recovery:**
- No automatic backup by application
- Recommend: Git version control for `data.json` (track changes, rollback capability)
- Recommend: Manual backup by user or IT team

### Network Security

**Local/localhost (MVP):**
- Blazor Server WebSocket connection is loopback (127.0.0.1) only
- No network exposure; no HTTPS needed
- No firewall rules required; localhost is always trusted

**Intranet deployment (optional):**
- Secure network via Windows Authentication or IP-based firewall rules
- If HTTPS required, generate self-signed certificate or use organizational CA
- Blazor Server supports HTTPS via `appsettings.json` (e.g., `"Https": "https://localhost:7001"`)

**No cloud exposure:**
- Mandate: local/intranet only
- If future requirement to expose to internet, implement:
  - HTTPS with valid certificate
  - OAuth2 / OpenID Connect authentication
  - WAF (Web Application Firewall)
  - DDoS protection

### Error Handling & Logging

**Exception handling:**
- All exceptions caught and logged with error details
- User-facing error messages are friendly and non-technical
- Stack traces logged to server console/logs only (not exposed to UI)

**Logging:**
- `ILogger<ReportService>` used throughout for debugging
- Log levels: `Information` (default), `Warning` (validation issues), `Error` (failures)
- No sensitive data (passwords, keys) logged

**Error page example:**
```html
<div class="error-panel">
  <h2>Dashboard Error</h2>
  <p>Error loading data: File not found at wwwroot/data/data.json</p>
  <p>Please ensure the data.json file exists in the same directory as the executable.</p>
</div>
```

### Dependency Security

**Minimal dependencies:**
- Core: `Microsoft.AspNetCore.App` (built-in to .NET 8)
- Testing: `xunit`, `Moq`, `bunit` (community-maintained, widely used)
- No third-party UI frameworks with unknown vulnerabilities

**Vulnerability scanning:**
- Use `dotnet list package --vulnerable` to check for known CVEs
- Update packages regularly via `dotnet package update`

### Future Security Enhancements

- **Rate limiting:** If exposed to internet, implement rate limiting to prevent abuse
- **Audit logging:** Track who accessed dashboard and when (timestamps)
- **Encryption at rest:** If data becomes sensitive, encrypt `data.json` with Windows DPAPI
- **Multi-user access control:** Role-based access (read-only, edit, admin) if team grows

---

## Scaling Strategy

### Data Volume Scaling

**Current design handles:**
- Up to 20 milestones per project
- Up to 24 months of timeline history
- Up to 100 work items across all status rows

**For larger datasets (Phase 3):**
- **Pagination:** Implement month pagination or horizontal scroll for timeline >12 months
- **Lazy loading:** Only render visible months in heatmap (virtual scrolling)
- **JSON compression:** Use gzip compression for `data.json` if file size exceeds 1 MB

**Memory optimization:**
- Current: Load entire `ReportData` into memory (~1 MB for 100 items)
- No optimization needed for MVP; memory usage remains <200 MB
- If scaling to 10,000+ items: Implement streaming JSON deserialization or pagination

### Throughput Scaling

**Current design:**
- Single user or small team (<5 concurrent browser tabs)
- No concurrency optimization needed
- File I/O is fast for <1 MB files (~100 ms)

**For higher concurrency (Phase 3):**
- **Caching:** Cache `ReportData` in memory (already singleton pattern); serve all users from cache
- **File watching:** Use `FileSystemWatcher` to detect file changes; invalidate cache and reload
- **In-memory database:** If data becomes larger, consider in-memory cache (Redis) for distributed deployments

**Current limitation:** Single-threaded file access; concurrent writes to `data.json` may cause data loss
- **Mitigation:** Implement file-based locking (lock file pattern) if multiple writers needed
- **MVP acceptable:** Manual editing; no concurrent writes expected

### UI Scaling

**Current design:**
- Fixed 1920x1080 layout; no responsive breakpoints
- CSS Grid with 4 columns (months); heatmap width is fixed
- SVG timeline has fixed dimensions (1560x185px)

**For responsive design (Phase 3):**
- Add media queries for tablet and mobile (optional for MVP)
- Implement horizontal scrolling for heatmap on smaller screens
- Collapse timeline to vertical orientation on mobile

**Print scaling:**
- Current: Single-page output (1920x1080 fits on A4 landscape)
- For multiple pages: Add explicit page breaks in CSS or implement PDF export

### Timeline SVG Scaling

**Current formula:**
- Month width: 260 pixels
- Total months: 6 (Jan-Jun)
- Total SVG width: 1560 pixels

**For more months:**
- Adjust month width or total SVG width based on data
- Example: 12 months = 12 * 260 = 3120 pixels (requires horizontal scroll)
- Implement dynamic width calculation: `svgWidth = monthCount * monthWidth`

**For more milestones:**
- Add milestone lines and markers without changing SVG height
- Current: 3 milestones at y=42, 98, 154
- Scale: Y-spacing = 185 / (milestoneCount + 1)

### Horizontal Scaling

**Current architecture:**
- Single-instance application; no horizontal scaling needed for MVP
- Each `.exe` is independent; no shared state

**For multi-instance deployment (Phase 3):**
- Deploy multiple `.exe` instances on different machines (e.g., for high availability)
- Share `data.json` via network file share or centralized storage
- Implement file locking or polling to detect changes

**Load balancer (optional):**
- Not needed for MVP; single executable serves all users
- If scaling to 100+ concurrent users, deploy behind reverse proxy (IIS, nginx)
- Proxy distributes requests across multiple application instances

### Database Scaling

**Current:** File-based JSON; no database
- **Pros:** Zero infrastructure, easy version control
- **Cons:** Limited concurrency, no transactions

**Future options (Phase 3+):**
- **SQL Server:** Migrate to database for better concurrency, transactions, and querying
- **Azure Cosmos DB:** Cloud option (violates local-only mandate but possible future direction)
- **SQLite:** Embedded database option (local file-based, similar to JSON but with SQL)

**Migration path:**
1. Keep JSON file as canonical source of truth
2. Add `IDataStore` interface with both `FileDataStore` and `DatabaseDataStore` implementations
3. Allow configuration to select which store to use
4. Implement backward compatibility during transition

---

## Risks & Mitigations

### Risk 1: File Corruption or Data Loss

**Severity:** High (dashboard becomes unusable if `data.json` is malformed)

**Cause:**
- User accidentally deletes `data.json`
- Incomplete file write (process crash during save)
- Manual editing error (invalid JSON syntax)
- Concurrent writes overwrite data

**Mitigation:**
- **MVP:** Add friendly error message if file is missing or invalid
- **Phase 2:** Implement file backup (e.g., `data.json.backup`) on successful load
- **Phase 2:** Use Git version control for `data.json` (track changes, enable rollback)
- **Phase 2:** Implement file locking (`FileLock` pattern) to prevent concurrent writes
- **Future:** Add simple backup/restore feature in UI (if file editing moves to app)

**Detection:**
- Log exception details when JSON deserialization fails
- Display error page with guidance (e.g., "Check JSON syntax at jsonlint.com")

---

### Risk 2: Date-to-Pixel Calculation Errors in Timeline

**Severity:** Medium (visual misalignment, but dashboard still loads and displays)

**Cause:**
- Incorrect date range or month boundaries
- Timezone issues (system clock set incorrectly)
- Leap year or month-end edge cases
- Floating-point rounding errors in pixel calculations

**Mitigation:**
- **MVP:** Unit tests for `DateToPixel()` with hardcoded dates and expected pixel positions
- **MVP:** Validate all dates during JSON deserialization (throw error if invalid)
- **Phase 2:** Add visual debug mode to show pixel coordinates and date calculations
- **Phase 2:** Use `DateTimeOffset` instead of `DateTime` for timezone-aware calculations

**Detection:**
- Compare generated SVG coordinates with expected values in tests
- Manual visual inspection: markers should align with month gridlines

---

### Risk 3: Browser Compatibility Issues

**Severity:** Medium (design may break on older browsers)

**Cause:**
- CSS Grid not supported in IE 11 or older Firefox/Chrome versions
- Blazor Server incompatible with IE 11
- Missing polyfills for modern CSS features
- WebSocket not supported on restricted networks

**Mitigation:**
- **MVP:** Document that modern browsers only are supported (Chrome 90+, Edge 90+, Firefox 88+, Safari 14+)
- **MVP:** No IE 11 support; explicitly exclude it from testing
- **Phase 2:** Add browser version check and friendly message if unsupported browser detected
- **Phase 2:** Test on restricted networks; implement long-polling fallback if WebSocket fails

**Detection:**
- Test on target machines and browsers before release
- Check browser console for CSS parsing errors or WebSocket failures

---

### Risk 4: Performance Degradation with Large Datasets

**Severity:** Low (unlikely for MVP; acceptable limits are documented)

**Cause:**
- SVG string concatenation is slow with 100+ milestones
- Large `data.json` file (>10 MB) takes long time to deserialize
- Heatmap rendering slow with 1000+ work items
- Memory exhaustion on resource-constrained machines

**Limits:**
- SVG performance acceptable up to 20 milestones
- File size acceptable up to 10 MB (deserializes in <500ms)
- Heatmap cells acceptable up to 500 total items
- Memory usage acceptable up to 500 MB

**Mitigation:**
- **MVP:** Document limits in user guide
- **Phase 2:** Add performance benchmarks (load time, render time) to test suite
- **Phase 2:** Implement lazy loading for SVG timeline (render only visible months)
- **Phase 3:** Implement pagination or filtering if limits are exceeded

**Detection:**
- Monitor load time and memory usage; add performance logging
- Test with maximum expected dataset size

---

### Risk 5: Inconsistency Between Visual Design and Implementation

**Severity:** Medium (embarrassment factor; affects executive perception)

**Cause:**
- CSS class names mistyped or incorrect selectors
- Color hex values incorrectly transcribed
- Font sizes, padding, or margins off by few pixels
- SVG coordinates calculated incorrectly

**Mitigation:**
- **MVP:** Pixel-perfect comparison against OriginalDesignConcept.html screenshot
- **MVP:** Automated visual testing (screenshot comparison, optional)
- **Phase 2:** CSS variables for all color palette values (centralize in `DesignConstants.cs`)
- **Phase 2:** Create unit tests for color lookups and CSS class generation

**Detection:**
- Manual side-by-side comparison of dashboard vs. design reference
- Browser developer tools to inspect computed styles and verify colors, fonts, spacing
- Print preview screenshot comparison

---

### Risk 6: JSON Schema Mismatch with Components

**Severity:** Medium (components may fail to render if data structure is unexpected)

**Cause:**
- User adds new fields to `data.json` that components don't expect
- Components expect fields that are missing from `data.json`
- Data types mismatch (string expected, number provided)
- Array of items changes structure

**Mitigation:**
- **MVP:** Strong typing via C# model classes; JSON deserialization will fail loudly if structure doesn't match
- **MVP:** Document schema clearly with required/optional fields
- **Phase 2:** Implement schema validation with detailed error messages (e.g., "Missing field: Milestones[0].Color")
- **Phase 2:** Add migration logic if schema changes (e.g., backward compatibility for old formats)

**Detection:**
- JSON deserialization errors logged with field path (e.g., "Error at Milestones[0].StartDate")
- User-facing error message guides them to correct `data.json`

---

### Risk 7: Browser Print/Screenshot Output Differs from On-Screen

**Severity:** Medium (primary use case is compromised)

**Cause:**
- Print stylesheet rules not applied correctly
- Colors print differently than on-screen (grayscale conversion)
- Margins or page breaks cut off content
- Browser zoom affecting screenshot dimensions
- Print background color not preserved

**Mitigation:**
- **MVP:** Test print preview early in development; iterate CSS until match
- **MVP:** Add CSS `@media print` rules to preserve colors and layout
- **MVP:** Document print instructions in user guide (e.g., "Enable background graphics in print settings")
- **Phase 2:** Automate screenshot testing with Selenium/Puppeteer-sharp

**Detection:**
- Manual print preview (Ctrl+P in browser) comparison with on-screen
- Screenshot comparison (use screenshot tool, paste into PowerPoint, verify)

---

### Risk 8: Executable Distribution & Execution Issues

**Severity:** Medium (prevents end users from running dashboard)

**Cause:**
- `.exe` is blocked by Windows SmartScreen on first run
- Missing `.NET 8 runtime dependencies in self-contained build
- Antivirus flags executable as suspicious
- Path or environment variables not set correctly
- Port 5000 already in use by another application

**Mitigation:**
- **MVP:** Test `.exe` on clean Windows 10+ machine (no dev tools installed)
- **MVP:** Sign executable with code signing certificate (optional, reduces SmartScreen warnings)
- **MVP:** Include instructions for SmartScreen bypass ("Click 'More info' → 'Run anyway'")
- **MVP:** Configure Blazor to use dynamic port if 5000 is occupied
- **Phase 2:** Add installer (MSI or Windows App Installer) for easier distribution

**Detection:**
- Run `.exe` on target machine; verify browser opens and dashboard loads
- Check Windows Event Viewer for errors
- Document any unusual warnings or failures in release notes

---

### Risk 9: Security Vulnerability in Dependencies

**Severity:** Low (minimal dependencies; all are from Microsoft or widely trusted sources)

**Cause:**
- Vulnerability in `System.Text.Json` or ASP.NET Core 8.0
- Vulnerability in testing libraries (xUnit, Moq, bunit)

**Mitigation:**
- **MVP:** Use latest patch version of .NET 8.0 SDK and runtime
- **MVP:** Regularly check for CVEs using `dotnet list package --vulnerable`
- **Phase 2:** Implement automated dependency scanning in CI/CD
- **Phase 2:** Set up security alerts (GitHub Security Advisory)

**Detection:**
- Monitor NuGet.org and Microsoft Security Advisories
- Review dependency updates before upgrading

---

### Risk 10: User Edits `data.json` with Syntax Errors

**Severity:** Low (non-technical users may struggle, but error message helps)

**Cause:**
- User forgets to quote strings (JSON syntax requires quotes)
- User adds trailing comma (invalid JSON)
- User deletes closing bracket or brace
- User uses single quotes instead of double quotes

**Mitigation:**
- **MVP:** Provide error message with line/column number of JSON error
- **MVP:** Create JSON schema documentation with clear examples
- **Phase 2:** Provide JSON syntax validation tool or online editor link
- **Phase 2:** Add simple UI editor for milestone/item data (no text editing required)

**Detection:**
- `System.Text.Json` catches and reports JSON syntax errors
- Error message example: "Invalid JSON at line 15, column 42: Unexpected character: ','"

---

### Risk 11: Milestone Date Out of Timeline Range

**Severity:** Low (timeline is flexible; out-of-range dates handled gracefully)

**Cause:**
- Milestone start date is before January (timeline begins)
- Milestone end date is after June (timeline ends)
- Checkpoint or marker date is outside milestone range

**Mitigation:**
- **MVP:** Accept dates outside timeline range; SVG rendering clips or expands as needed
- **MVP:** Log warning if date is significantly outside expected range
- **Phase 2:** Dynamically calculate timeline range based on data (start = min date, end = max date)

**Detection:**
- Visual inspection: markers outside visible timeline area
- Unit tests with edge-case dates

---

### Risk 12: Concurrent File Edits to `data.json`

**Severity:** Low (acceptable for MVP; documented limitation)

**Cause:**
- User edits `data.json` while dashboard is running
- Two users edit simultaneously on network share
- Scheduled script updates `data.json` while user is viewing dashboard

**Mitigation:**
- **MVP:** Accept data loss risk; document limitation in release notes
- **MVP:** Recommend manual browser refresh (F5) after editing `data.json`
- **Phase 2:** Implement `FileSystemWatcher` to auto-reload dashboard when file changes
- **Phase 2:** Implement file locking or version checking to detect conflicts

**Detection:**
- Stale data displayed if file changed after last load
- User can manually refresh to see latest data

---

## Conclusion

The Executive Project Reporting Dashboard is architected as a simple, maintainable Blazor Server application optimized for single-user deployment and executive-focused reporting. By leveraging C# .NET 8, CSS Grid, and JSON-based configuration, the system achieves visual fidelity to the design specification while maintaining operational simplicity and portability. Key risks are mitigated through early testing, clear error handling, and comprehensive documentation. The architecture supports future enhancements (file watching, multi-project support, PDF export) without requiring major restructuring.