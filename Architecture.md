# Architecture

## Overview & Goals

This architecture defines a lightweight, locally-deployed executive dashboard for project status reporting. The system reads project metadata and status from a JSON configuration file (`data.json`) and renders a fixed 1920x1080 viewport containing: (1) a project header with legend, (2) a timeline visualization with milestone markers, and (3) a monthly status heatmap color-coded by status category (Shipped, In Progress, Carryover, Blockers).

The architecture prioritizes:
- **Visual Fidelity**: Pixel-perfect rendering matching `OriginalDesignConcept.html` design specification
- **Simplicity**: No authentication, no cloud services, no external APIs—local file-based data only
- **Rapid Iteration**: Server-side rendering with Blazor Server eliminates frontend/backend complexity
- **Screenshot-Readiness**: Fixed viewport and print-optimized styling for executive PowerPoint integration

## System Components

### 1. **Blazor Server Application (MyProject.Reporting)**

**Responsibility:**  
Host the ASP.NET Core Kestrel web server running on `localhost:5001` (HTTPS) or `localhost:5000` (HTTP). Render server-side components and serve static assets.

**Key Characteristics:**
- Single-page application with no client-side SPA framework (React, Vue, Angular)
- Server-side rendering via `.razor` components compiled to C#
- Built-in Flexbox/CSS Grid layout—no additional CSS framework required
- Integrated error handling and graceful degradation

**Dependencies:**
- `.NET 8 SDK` (minimum v8.0.0)
- `ASP.NET Core` runtime (included in SDK)
- `System.Text.Json` for JSON deserialization (built-in)

---

### 2. **DashboardDataService**

**Responsibility:**  
Load, parse, and cache project metadata from `data.json` file. Expose deserialized data to Blazor components via dependency injection.

**Interfaces & Contracts:**

```csharp
public interface IDashboardDataService
{
    Task<DashboardModel> GetDashboardDataAsync(CancellationToken cancellationToken = default);
    Task ReloadDataAsync(CancellationToken cancellationToken = default);
}

public class DashboardDataService : IDashboardDataService
{
    private readonly ILogger<DashboardDataService> _logger;
    private readonly IConfiguration _config;
    private DashboardModel _cachedData;

    public DashboardDataService(ILogger<DashboardDataService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<DashboardModel> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedData != null)
            return _cachedData;

        string dataPath = _config.GetValue<string>("DataFilePath", "./wwwroot/data/data.json");
        try
        {
            string json = await File.ReadAllTextAsync(dataPath, cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _cachedData = JsonSerializer.Deserialize<DashboardModel>(json, options)
                ?? throw new InvalidOperationException("Deserialized data is null");
            
            _logger.LogInformation("Dashboard data loaded successfully from {Path}", dataPath);
            return _cachedData;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Data file not found at {Path}", dataPath);
            throw new DashboardDataException($"Unable to load dashboard data. File not found: {dataPath}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in data file at {Path}", dataPath);
            throw new DashboardDataException("Unable to load dashboard data. JSON format is invalid.", ex);
        }
    }

    public async Task ReloadDataAsync(CancellationToken cancellationToken = default)
    {
        _cachedData = null;
        await GetDashboardDataAsync(cancellationToken);
    }
}
```

**Responsibilities:**
1. Read `data.json` from file system (path configurable via `appsettings.json`)
2. Deserialize JSON into strongly-typed `DashboardModel` POCO objects
3. Cache deserialized data in memory to avoid repeated file I/O
4. Provide `ReloadDataAsync()` method for manual refresh triggered by UI button
5. Log all errors with context (file path, line numbers, etc.)
6. Throw `DashboardDataException` for user-friendly error messages (not stack traces)

**Dependencies:**
- `System.IO.File` (built-in)
- `System.Text.Json` (built-in)
- `ILogger<T>` (injected from ASP.NET Core DI container)
- `IConfiguration` (injected for `appsettings.json` config values)

**Storage:**
- In-memory cache (`_cachedData` field); no database needed
- Cache invalidation: Manual via `ReloadDataAsync()` call from Refresh button

---

### 3. **DashboardHeader.razor Component**

**Responsibility:**  
Render the fixed header section (top ~60px) with project title, subtitle, ADO Backlog link, and legend.

**Visual Mapping:**  
Corresponds to `.hdr` section in `OriginalDesignConcept.html`

**Data Bindings:**
- `@inject IDashboardDataService DataService`
- `@inject NavigationManager Navigation`
- Properties: `DashboardModel Data { get; set; }`

**UI Structure:**
```
┌─────────────────────────────────────────────────────────┐
│ [Left]                                    [Right Legend] │
│ Title (h1, 24px bold)                     ◇ PoC Milestone│
│ Subtitle (12px gray)                      ◇ Prod Release │
│ "? ADO Backlog" link (optional, blue)     ○ Checkpoint   │
│                                           ▮ Now (Apr 26) │
└─────────────────────────────────────────────────────────┘
```

**Key Implementation Details:**
- **Title:** Font 24px, weight 700, color #111
- **Subtitle:** Font 12px, color #888, margin-top 2px
- **Legend Items:** Display Flex with 22px gap between items
  - PoC Milestone: 12x12px diamond (rotated 45°), fill #F4B400
  - Production Release: 12x12px diamond (rotated 45°), fill #34A853
  - Checkpoint: 8x8px circle, fill #999
  - Now Marker: 2px × 14px line, fill #EA4335
- **ADO Link:** Color #0078D4, target="_blank" for external navigation

**Rendering Logic:**
1. In `OnInitializedAsync()`, call `DataService.GetDashboardDataAsync()`
2. Extract `Data.Project.Name`, `Data.Project.Subtitle`, `Data.Project.AdoBacklogUrl`
3. Render title, subtitle, optional link
4. Render legend with hardcoded icon styles and labels

**CSS:**
```css
.hdr {
    padding: 12px 44px 10px;
    border-bottom: 1px solid #E0E0E0;
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-shrink: 0;
}

.hdr h1 {
    font-size: 24px;
    font-weight: 700;
    color: #111;
}

.legend {
    display: flex;
    gap: 22px;
    align-items: center;
}

.legend-item {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 12px;
    color: #111;
}

.legend-icon {
    flex-shrink: 0;
}

.legend-diamond {
    width: 12px;
    height: 12px;
    transform: rotate(45deg);
    display: inline-block;
}

.legend-circle {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    display: inline-block;
}

.legend-line {
    width: 2px;
    height: 14px;
    display: inline-block;
}
```

---

### 4. **TimelineChart.razor Component**

**Responsibility:**  
Render the timeline section (fixed height 196px) with SVG-based milestone bars, markers, and "Now" indicator.

**Visual Mapping:**  
Corresponds to `.tl-area` and `.tl-svg-box` sections in `OriginalDesignConcept.html`

**Layout Structure:**
```
┌──────────────────────────────────────────────────────────┐
│ [Left Sidebar]          [SVG Timeline Visualization]     │
│ M1: Chatbot & MS Role   Jan │ Feb │ Mar │ Apr │ May │ Jun│
│ M2: PDS & Inventory                 NOW ↑                 │
│ M3: Auto Review DFD                                       │
└──────────────────────────────────────────────────────────┘
```

**Data Bindings:**
- `@inject IDashboardDataService DataService`
- Properties: `DashboardModel Data { get; set; }`
- State: `MarkupString SvgMarkup { get; set; }`

**Key Implementation Details:**

**Left Sidebar (Milestone Labels):**
- Width: 230px, flex-shrink: 0
- For each milestone in `Data.Milestones`:
  - Render ID (e.g., "M1") in milestone-specific color (e.g., #0078D4)
  - Render title below ID in lighter color (#444)
  - Font: 12px, weight 600 for ID, weight 400 for title
  - Line-height: 1.4

**SVG Timeline Area:**
- Width: 1560px, Height: 185px
- Coordinate System:
  - X-axis: 260px per month (Jan=0, Feb=260, Mar=520, Apr=780, May=1040, Jun=1300)
  - Y-axis: 56px per milestone track (M1=42, M2=98, M3=154)
- Month Dividers: Vertical lines at month boundaries, gray (#bbb), stroke-width 1px
- Month Labels: Text at top (y=14), font 11px, weight 600, color #666
- Timeline Bars: Lines (stroke-width 3px) colored per milestone
- Milestone Markers:
  - Checkpoint: Circle (r=7 for start, r=5 for intermediate, r=4 for checkpoint), white fill, colored stroke
  - PoC: Diamond (10×10px), filled with #F4B400, drop shadow filter
  - Production Release: Diamond (10×10px), filled with #34A853, drop shadow filter
- Date Labels: Text above/below markers, font 10px, color #666
- Now Marker: Red dashed vertical line (#EA4335, stroke-dasharray "5,3"), with "NOW" label

**SVG Generation Logic:**

```csharp
private MarkupString GenerateSvgTimeline()
{
    var svg = new StringBuilder();
    svg.Append(@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""1560"" height=""185"" style=""overflow:visible;display:block"">");
    
    // Add filter for drop shadow
    svg.Append(@"<defs><filter id=""sh""><feDropShadow dx=""0"" dy=""1"" stdDeviation=""1.5"" flood-opacity=""0.3""/></filter></defs>");
    
    // Month dividers and labels
    var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
    for (int i = 0; i < months.Length; i++)
    {
        int x = i * 260;
        svg.AppendFormat(@"<line x1=""{0}"" y1=""0"" x2=""{0}"" y2=""185"" stroke=""#bbb"" stroke-opacity=""0.4"" stroke-width=""1""/>", x);
        svg.AppendFormat(@"<text x=""{0}"" y=""14"" fill=""#666"" font-size=""11"" font-weight=""600"">{1}</text>", x + 5, months[i]);
    }
    
    // Timeline bars for each milestone
    foreach (var milestone in Data.Milestones)
    {
        int yPos = GetMilestoneYPosition(milestone.Id);
        string color = milestone.Color ?? "#999";
        svg.AppendFormat(@"<line x1=""0"" y1=""{0}"" x2=""1560"" y2=""{0}"" stroke=""{1}"" stroke-width=""3""/>", yPos, color);
    }
    
    // Milestone markers (diamonds, circles)
    foreach (var milestone in Data.Milestones)
    {
        int xPos = GetPixelPositionForDate(milestone.Date);
        int yPos = GetMilestoneYPosition(milestone.Id);
        
        if (milestone.Type == "checkpoint")
        {
            svg.AppendFormat(@"<circle cx=""{0}"" cy=""{1}"" r=""5"" fill=""white"" stroke=""#888"" stroke-width=""2.5""/>", xPos, yPos);
        }
        else if (milestone.Type == "poc" || milestone.Type == "release")
        {
            string fillColor = milestone.Type == "poc" ? "#F4B400" : "#34A853";
            svg.AppendFormat(@"<polygon points=""{0},{1} {2},{1} {0},{3} {4},{1}"" fill=""{5}"" filter=""url(#sh)""/>",
                xPos, yPos - 11, xPos + 11, yPos + 11, xPos - 11, fillColor);
        }
    }
    
    // Now marker
    int nowX = GetPixelPositionForDate(Data.NowMarker);
    svg.AppendFormat(@"<line x1=""{0}"" y1=""0"" x2=""{0}"" y2=""185"" stroke=""#EA4335"" stroke-width=""2"" stroke-dasharray=""5,3""/>", nowX);
    svg.AppendFormat(@"<text x=""{0}"" y=""14"" fill=""#EA4335"" font-size=""10"" font-weight=""700"">NOW</text>", nowX + 4);
    
    svg.Append("</svg>");
    
    return new MarkupString(svg.ToString());
}

private int GetPixelPositionForDate(string isoDate)
{
    // Parse date, calculate days from timeline start, multiply by pixels-per-day
    var date = DateTime.ParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    var timelineStart = new DateTime(2026, 1, 1);
    int daysDiff = (date - timelineStart).Days;
    int pixelsPerDay = 260 / 31; // Approximate; refine based on actual month lengths
    return daysDiff * pixelsPerDay;
}

private int GetMilestoneYPosition(string milestoneId)
{
    return milestoneId switch
    {
        "M1" => 42,
        "M2" => 98,
        "M3" => 154,
        _ => 42
    };
}
```

**CSS:**
```css
.tl-area {
    display: flex;
    align-items: stretch;
    padding: 6px 44px 0;
    flex-shrink: 0;
    height: 196px;
    border-bottom: 2px solid #E8E8E8;
    background: #FAFAFA;
}

.tl-sidebar {
    width: 230px;
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    padding: 16px 12px 16px 0;
    border-right: 1px solid #E0E0E0;
}

.tl-milestone-label {
    font-size: 12px;
    font-weight: 600;
    line-height: 1.4;
}

.tl-milestone-title {
    font-size: 12px;
    font-weight: 400;
    color: #444;
}

.tl-svg-box {
    flex: 1;
    padding-left: 12px;
    padding-top: 6px;
}
```

---

### 5. **HeatmapGrid.razor Component**

**Responsibility:**  
Render the monthly status heatmap as a CSS Grid with 5 columns (row label + 4 months) and 5 rows (header + 4 status categories).

**Visual Mapping:**  
Corresponds to `.hm-wrap` and `.hm-grid` sections in `OriginalDesignConcept.html`

**Layout Structure:**
```
┌────────────┬─────────┬─────────┬─────────┬─────────┐
│ (corner)   │   Mar   │   Apr   │   May   │   Jun   │
├────────────┼─────────┼─────────┼─────────┼─────────┤
│ SHIPPED    │ Feature │ Release │ Module  │ API     │
│ (green)    │    A    │    1    │    X    │   Y     │
├────────────┼─────────┼─────────┼─────────┼─────────┤
│ IN PROGRES │ Work A  │ Work B  │ Work C  │   -     │
│ (blue)     │         │         │         │         │
├────────────┼─────────┼─────────┼─────────┼─────────┤
│ CARRYOVER  │ Old A   │ Old B   │   -     │   -     │
│ (amber)    │         │         │         │         │
├────────────┼─────────┼─────────┼─────────┼─────────┤
│ BLOCKERS   │   -     │ Issue X │   -     │   -     │
│ (red)      │         │         │         │         │
└────────────┴─────────┴─────────┴─────────┴─────────┘
```

**Data Bindings:**
- `@inject IDashboardDataService DataService`
- Properties: `DashboardModel Data { get; set; }`
- Computed: `string CurrentMonth { get; set; }` (extracted from `Data.NowMarker`)

**Grid Layout:**
```css
.hm-grid {
    flex: 1;
    min-height: 0;
    display: grid;
    grid-template-columns: 160px repeat(4, 1fr);
    grid-template-rows: 36px repeat(4, 1fr);
    border: 1px solid #E0E0E0;
}
```

**Cell Types & Styling:**

**1. Corner Cell (top-left, status label header):**
- CSS class: `hm-corner`
- Background: #F5F5F5
- Font: 11px, weight 700, color #999, uppercase
- Border-right: 1px solid #E0E0E0
- Border-bottom: 2px solid #CCC

**2. Column Header Cells (month names):**
- CSS class: `hm-col-hdr`
- Background: #F5F5F5 (or #FFF0D0 for current month, April)
- Font: 16px, weight 700, color #111 (or #C07700 for current month)
- Border-right: 1px solid #E0E0E0
- Border-bottom: 2px solid #CCC
- Display: Flex with center alignment

**3. Row Header Cells (status categories):**
- CSS classes: `hm-row-hdr`, `ship-hdr` | `prog-hdr` | `carry-hdr` | `block-hdr`
- Background colors:
  - Shipped: #E8F5E9 (light green)
  - In Progress: #E3F2FD (light blue)
  - Carryover: #FFF8E1 (light amber)
  - Blockers: #FEF2F2 (light red)
- Text colors:
  - Shipped: #1B7A28 (dark green)
  - In Progress: #1565C0 (dark blue)
  - Carryover: #B45309 (dark amber)
  - Blockers: #991B1B (dark red)
- Font: 11px, weight 700, uppercase, letter-spacing 0.7px
- Border-right: 2px solid #CCC
- Border-bottom: 1px solid #E0E0E0

**4. Data Cells:**
- CSS classes: `hm-cell`, `ship-cell` | `prog-cell` | `carry-cell` | `block-cell`
- Background colors:
  - Shipped: #F0FBF0 (normal), #D8F2DA (current month)
  - In Progress: #EEF4FE (normal), #DAE8FB (current month)
  - Carryover: #FFFDE7 (normal), #FFF0B0 (current month)
  - Blockers: #FFF5F5 (normal), #FFE4E4 (current month)
- Padding: 8px 12px
- Border-right: 1px solid #E0E0E0
- Border-bottom: 1px solid #E0E0E0
- Overflow: hidden

**5. Cell Item (text + bullet):**
- CSS class: `it` (item)
- Font: 12px, color #333
- Padding: 2px 0 2px 12px (left-align with bullet)
- Line-height: 1.35
- Position: relative (for ::before pseudo-element)

**6. Bullet Marker (::before pseudo-element):**
- Content: '' (empty)
- Position: absolute, left: 0, top: 7px
- Width: 6px, height: 6px
- Border-radius: 50% (circle)
- Background colors per status:
  - Shipped: #34A853 (green)
  - In Progress: #0078D4 (blue)
  - Carryover: #F4B400 (amber)
  - Blockers: #EA4335 (red)

**Rendering Logic:**

```csharp
@implements IAsyncDisposable

@page "/"
@layout MainLayout
@inject IDashboardDataService DataService

<div class="hm-wrap">
    <div class="hm-title">Monthly Execution Heatmap - Shipped ● In Progress ? Carryover ? Blockers ?</div>
    
    @if (Data != null)
    {
        <div class="hm-grid">
            <!-- Corner cell -->
            <div class="hm-corner"></div>
            
            <!-- Column headers (months) -->
            @foreach (var month in GetMonthHeaders())
            {
                string headerClass = month == CurrentMonth ? "hm-col-hdr apr-hdr" : "hm-col-hdr";
                <div class="@headerClass">@month</div>
            }
            
            <!-- Status rows -->
            @foreach (var statusRow in Data.StatusRows)
            {
                string rowHeaderClass = $"hm-row-hdr {GetRowHeaderClass(statusRow.Category)}";
                <div class="@rowHeaderClass">
                    @GetStatusLabel(statusRow.Category)
                </div>
                
                @foreach (var month in GetMonthHeaders())
                {
                    string cellClass = $"hm-cell {GetCellClass(statusRow.Category, month)}";
                    <div class="@cellClass">
                        @RenderCellItems(statusRow, month)
                    </div>
                }
            }
        </div>
    }
    else
    {
        <div style="padding: 20px; color: #EA4335;">
            Unable to load dashboard data. Please check that data.json is properly formatted.
        </div>
    }
    
    <button @onclick="RefreshData" style="margin-top: 12px;">Refresh Data</button>
</div>

@code {
    private DashboardModel Data { get; set; }
    private string CurrentMonth { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            Data = await DataService.GetDashboardDataAsync();
            CurrentMonth = ExtractMonthFromDate(Data.NowMarker); // "Apr"
        }
        catch (Exception ex)
        {
            // Error handled in UI above
        }
    }
    
    private List<string> GetMonthHeaders()
    {
        return new List<string> { "Mar", "Apr", "May", "Jun" };
    }
    
    private string GetRowHeaderClass(string category)
    {
        return category switch
        {
            "shipped" => "ship-hdr",
            "inprogress" => "prog-hdr",
            "carryover" => "carry-hdr",
            "blockers" => "block-hdr",
            _ => ""
        };
    }
    
    private string GetStatusLabel(string category)
    {
        return category switch
        {
            "shipped" => "● SHIPPED",
            "inprogress" => "? IN PROGRESS",
            "carryover" => "? CARRYOVER",
            "blockers" => "? BLOCKERS",
            _ => ""
        };
    }
    
    private string GetCellClass(string category, string month)
    {
        string baseClass = category switch
        {
            "shipped" => "ship-cell",
            "inprogress" => "prog-cell",
            "carryover" => "carry-cell",
            "blockers" => "block-cell",
            _ => ""
        };
        
        if (month == CurrentMonth)
            baseClass += " apr";
        
        return baseClass;
    }
    
    private MarkupString RenderCellItems(StatusRow statusRow, string month)
    {
        var items = statusRow.Items
            ?.Where(i => i.Month == month)
            ?.Select(i => $"<div class=\"it\">{HtmlEncoder.Default.Encode(i.Value)}</div>")
            ?.ToList() ?? new List<string>();
        
        if (items.Count == 0)
            return new MarkupString("<div class=\"it\" style=\"color: #AAA;\">-</div>");
        
        return new MarkupString(string.Join("", items));
    }
    
    private string ExtractMonthFromDate(string isoDate)
    {
        var date = DateTime.ParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        return date.ToString("MMM", CultureInfo.InvariantCulture);
    }
    
    private async Task RefreshData()
    {
        try
        {
            await DataService.ReloadDataAsync();
            Data = await DataService.GetDashboardDataAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            // Error handling
        }
    }
    
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        // Cleanup if needed
    }
}
```

**CSS:**
```css
.hm-wrap {
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;
    padding: 10px 44px 10px;
}

.hm-title {
    font-size: 14px;
    font-weight: 700;
    color: #888;
    letter-spacing: 0.5px;
    text-transform: uppercase;
    margin-bottom: 8px;
    flex-shrink: 0;
}

.hm-grid {
    flex: 1;
    min-height: 0;
    display: grid;
    grid-template-columns: 160px repeat(4, 1fr);
    grid-template-rows: 36px repeat(4, 1fr);
    border: 1px solid #E0E0E0;
}

/* All cell styles from design specification */
```

---

### 6. **Index.razor Page**

**Responsibility:**  
Container page that composes the three major components (Header, Timeline, Heatmap) into the complete dashboard layout.

**Layout:**
```html
<div style="width: 1920px; height: 1080px; overflow: hidden; background: white; display: flex; flex-direction: column;">
    <DashboardHeader />
    <TimelineChart />
    <HeatmapGrid />
</div>
```

**CSS:**
```css
body {
    width: 1920px;
    height: 1080px;
    overflow: hidden;
    background: #FFFFFF;
    font-family: 'Segoe UI', Arial, sans-serif;
    color: #111;
    display: flex;
    flex-direction: column;
    margin: 0;
    padding: 0;
}
```

---

## Component Interactions

### Data Flow Diagram

```
┌─────────────────────┐
│  data.json File     │
│ (local filesystem)  │
└──────────────┬──────┘
               │
               ▼
┌──────────────────────────────┐
│ DashboardDataService         │
│ - Load from file             │
│ - Deserialize JSON           │
│ - Cache in memory            │
│ - Provide ReloadDataAsync()  │
└──────────┬───────────────────┘
           │
    ┌──────┴──────┐
    │             │
    ▼             ▼
┌──────────────┐ ┌──────────────────┐
│Index.razor   │ │TimelineChart.razor│
│(Page)        │ │  (Component)      │
│ OnInit: Load │ │ OnInit: Get data  │
└──────┬───────┘ │ Render: SVG       │
       │         └──────────────────┘
       │
    ┌──────┴────────┬────────────────┐
    │               │                │
    ▼               ▼                ▼
┌──────────┐ ┌──────────┐  ┌────────────┐
│Dashboard │ │ Timeline │  │HeatmapGrid │
│ Header   │ │  Chart   │  │ Component  │
│Component │ │Component │  │            │
└──────────┘ └──────────┘  └────────────┘
    │               │                │
    └───────┬───────┴────────────────┘
            │
            ▼
    ┌──────────────────┐
    │  Browser Render  │
    │  (HTML + CSS)    │
    └──────────────────┘
```

### Interaction Sequences

**1. Page Load (OnInitializedAsync):**
1. Browser requests `https://localhost:5001/`
2. Kestrel serves `Index.razor` (compiled C# component)
3. `Index.razor` calls `OnInitializedAsync()`
4. Injects `DashboardDataService`, calls `GetDashboardDataAsync()`
5. Service reads `./wwwroot/data/data.json` asynchronously
6. Deserializes JSON → `DashboardModel` POCO
7. Caches result in memory
8. Returns data to `Index.razor`
9. Components (`DashboardHeader`, `TimelineChart`, `HeatmapGrid`) receive `@parameters`
10. Each component renders server-side, generates HTML/SVG
11. Complete page sent to browser as single HTML document
12. Browser renders CSS Grid layout, displays dashboard

**Timing:**
- Cold start: ~2 seconds (file I/O + deserialization)
- Warm start (cached): <100ms

---

**2. Manual Refresh (RefreshData button click):**
1. User clicks "Refresh Data" button on UI
2. Blazor event binding triggers `RefreshData()` method in `HeatmapGrid.razor`
3. `RefreshData()` calls `DataService.ReloadDataAsync()`
4. Service clears cache (`_cachedData = null`)
5. Service calls `GetDashboardDataAsync()` → re-reads file
6. Returns fresh data
7. Component receives updated data
8. Calls `StateHasChanged()` to trigger re-render
9. Blazor rerenders affected components only (affected: HeatmapGrid + TimelineChart)
10. Browser receives HTML update (via SignalR), re-renders heatmap

**Timing:**
- Refresh: ~1 second (file I/O + re-render)

---

**3. Milestone Hover Tooltip (Client-Side Interactivity - Optional):**
1. User moves mouse over milestone marker (diamond/circle) in SVG
2. JavaScript listener (via `@onmouseover`) triggers
3. Blazor captures mouse coordinates via `MouseEventArgs`
4. Calls C# method `ShowTooltip(milestone)` (optional, can be pure JS)
5. Tooltip renders as absolutely-positioned `<div>` near cursor
6. Shows: Milestone title, date, type (PoC/Release/Checkpoint)
7. User moves mouse away → `@onmouseout` fires, tooltip hidden

*Note: This is optional interactivity; MVP may omit tooltips.*

---

## Data Model

### JSON Schema (data.json)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["project", "milestones", "statusRows", "nowMarker"],
  "properties": {
    "project": {
      "type": "object",
      "required": ["name", "subtitle"],
      "properties": {
        "name": {
          "type": "string",
          "description": "Project title, e.g., 'Privacy Automation Release Roadmap'"
        },
        "subtitle": {
          "type": "string",
          "description": "Subtitle with org, workstream, and date, e.g., 'Trusted Platform • Privacy Automation Workstream • April 2026'"
        },
        "adoBacklogUrl": {
          "type": "string",
          "description": "Optional URL to Azure DevOps backlog (opens in new tab)"
        }
      }
    },
    "milestones": {
      "type": "array",
      "minItems": 1,
      "items": {
        "type": "object",
        "required": ["id", "title", "date", "type"],
        "properties": {
          "id": {
            "type": "string",
            "description": "Unique identifier, e.g., 'M1', 'PoC', 'Prod'"
          },
          "title": {
            "type": "string",
            "description": "Milestone title, e.g., 'Chatbot & MS Role'"
          },
          "date": {
            "type": "string",
            "format": "date",
            "description": "ISO 8601 date, e.g., '2026-01-12'"
          },
          "type": {
            "type": "string",
            "enum": ["checkpoint", "poc", "release"],
            "description": "Milestone type (checkpoint=circle, poc/release=diamond)"
          },
          "color": {
            "type": "string",
            "pattern": "^#[0-9A-Fa-f]{6}$",
            "description": "Hex color for timeline bar, e.g., '#0078D4'"
          }
        }
      }
    },
    "statusRows": {
      "type": "array",
      "minItems": 4,
      "items": {
        "type": "object",
        "required": ["label", "category", "items"],
        "properties": {
          "label": {
            "type": "string",
            "description": "Row header text, e.g., 'SHIPPED' (auto-generated if omitted)"
          },
          "category": {
            "type": "string",
            "enum": ["shipped", "inprogress", "carryover", "blockers"],
            "description": "Status category (determines row color)"
          },
          "items": {
            "type": "array",
            "items": {
              "type": "object",
              "required": ["month", "value"],
              "properties": {
                "month": {
                  "type": "string",
                  "enum": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"],
                  "description": "Month name (3 letters)"
                },
                "value": {
                  "type": "string",
                  "maxLength": 100,
                  "description": "Single-line status text, e.g., 'Feature A shipped'"
                }
              }
            }
          }
        }
      }
    },
    "nowMarker": {
      "type": "string",
      "format": "date",
      "description": "Current date in ISO 8601 format (red dashed line position on timeline)"
    }
  }
}
```

### C# POCO Models

```csharp
using System;
using System.Collections.Generic;

namespace MyProject.Reporting.Models
{
    /// <summary>
    /// Root model for dashboard data from data.json
    /// </summary>
    public class DashboardModel
    {
        public ProjectInfo Project { get; set; }
        public List<Milestone> Milestones { get; set; }
        public List<StatusRow> StatusRows { get; set; }
        public string NowMarker { get; set; } // ISO 8601 date string
    }

    /// <summary>
    /// Project metadata
    /// </summary>
    public class ProjectInfo
    {
        public string Name { get; set; }
        public string Subtitle { get; set; }
        public string AdoBacklogUrl { get; set; }
    }

    /// <summary>
    /// Milestone marker on timeline
    /// </summary>
    public class Milestone
    {
        public string Id { get; set; } // e.g., "M1", "PoC"
        public string Title { get; set; }
        public string Date { get; set; } // ISO 8601: "2026-01-12"
        public string Type { get; set; } // "checkpoint", "poc", "release"
        public string Color { get; set; } // Hex color for timeline bar, e.g., "#0078D4"
    }

    /// <summary>
    /// Status row in heatmap
    /// </summary>
    public class StatusRow
    {
        public string Label { get; set; } // e.g., "SHIPPED"
        public string Category { get; set; } // "shipped", "inprogress", "carryover", "blockers"
        public List<StatusItem> Items { get; set; }
    }

    /// <summary>
    /// Individual status item in heatmap cell
    /// </summary>
    public class StatusItem
    {
        public string Month { get; set; } // "Jan", "Feb", etc.
        public string Value { get; set; } // Text label, e.g., "Feature A shipped"
    }
}
```

### Data Validation

```csharp
public class DashboardModelValidator
{
    public static void Validate(DashboardModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));
        
        if (string.IsNullOrWhiteSpace(model.Project?.Name))
            throw new ValidationException("Project.Name is required");
        
        if (model.Milestones == null || model.Milestones.Count < 1)
            throw new ValidationException("At least one milestone is required");
        
        if (model.StatusRows == null || model.StatusRows.Count < 4)
            throw new ValidationException("Exactly 4 status rows required (shipped, inprogress, carryover, blockers)");
        
        var validCategories = new[] { "shipped", "inprogress", "carryover", "blockers" };
        foreach (var row in model.StatusRows)
        {
            if (!validCategories.Contains(row.Category))
                throw new ValidationException($"Invalid status category: {row.Category}");
        }
        
        if (string.IsNullOrWhiteSpace(model.NowMarker))
            throw new ValidationException("NowMarker date is required");
        
        // Validate ISO 8601 date format
        if (!DateTime.TryParseExact(model.NowMarker, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new ValidationException($"NowMarker date must be ISO 8601 format (yyyy-MM-dd): {model.NowMarker}");
        
        foreach (var milestone in model.Milestones)
        {
            if (!DateTime.TryParseExact(milestone.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                throw new ValidationException($"Milestone date must be ISO 8601 format: {milestone.Date}");
            
            if (!new[] { "checkpoint", "poc", "release" }.Contains(milestone.Type))
                throw new ValidationException($"Invalid milestone type: {milestone.Type}");
        }
    }
}
```

---

## UI Component Architecture

### Component-to-Design Mapping

| Design Section | Visual Reference | Component | Responsibilities | Key Bindings |
|---|---|---|---|---|
| **Header** | `.hdr` container | `DashboardHeader.razor` | Title, subtitle, ADO link, legend | `Data.Project.Name`, `Data.Project.Subtitle`, `Data.Project.AdoBacklogUrl` |
| **Timeline Labels** | `.tl-area` left sidebar | Part of `TimelineChart.razor` | Milestone IDs + titles (M1, M2, M3...) | `Data.Milestones[*].Id`, `.Title`, `.Color` |
| **Timeline SVG** | `.tl-svg-box` SVG | Part of `TimelineChart.razor` | Month dividers, milestone bars, markers, "Now" line | `Data.Milestones[*].Date`, `.Type`, `.Color` |
| **Heatmap Title** | `.hm-title` | `HeatmapGrid.razor` | "Monthly Execution Heatmap..." text | Hardcoded |
| **Heatmap Grid** | `.hm-grid` + cells | `HeatmapGrid.razor` | 5×5 grid: corner + headers + status rows | `Data.StatusRows[*].Items`, current month from `Data.NowMarker` |
| **Heatmap Cells** | `.hm-cell`, `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` | `HeatmapGrid.razor` (grid cells) | Status text items with bullet markers | Item text from `Data.StatusRows[*].Items` |

### Layout Hierarchy

```
Index.razor (Page)
├── style (body: 1920×1080, flex column)
├── DashboardHeader.razor
│   ├── .hdr (flex, space-between)
│   ├── Left: h1 + subtitle
│   └── Right: .legend (flex, gap 22px)
│       └── 4× .legend-item (PoC, Prod, Checkpoint, Now)
├── TimelineChart.razor
│   ├── .tl-area (flex, height 196px)
│   ├── .tl-sidebar (width 230px)
│   │   └── 3× milestone labels (M1, M2, M3)
│   └── .tl-svg-box
│       └── SVG (1560×185)
│           ├── Month dividers + labels
│           ├── Timeline bars × 3
│           ├── Milestone markers (diamonds, circles)
│           └── "NOW" line + label
└── HeatmapGrid.razor
    ├── .hm-wrap (flex column, flex:1)
    ├── .hm-title
    └── .hm-grid (CSS grid, 5 cols × 5 rows)
        ├── .hm-corner
        ├── 4× .hm-col-hdr (Mar, Apr, May, Jun)
        ├── 4× row sets:
        │   ├── .hm-row-hdr + ship-hdr | prog-hdr | carry-hdr | block-hdr
        │   └── 4× .hm-cell (ship-cell | prog-cell | carry-cell | block-cell)
        │       └── 0-3× .it (item + ::before bullet)
        └── Refresh button
```

### CSS Class Structure

**Global Classes (app.css):**
- `.hdr` — Header container
- `.hm-wrap` — Heatmap wrapper
- `.hm-grid` — Heatmap grid container
- `.tl-area` — Timeline section container

**Status Category Classes (applied to rows/cells):**
- `.ship-*` — Shipped (green)
- `.prog-*` — In Progress (blue)
- `.carry-*` — Carryover (amber)
- `.block-*` — Blockers (red)

**Cell Classes:**
- `.hm-corner`, `.hm-col-hdr`, `.hm-row-hdr`, `.hm-cell`
- `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell`

**Item Classes:**
- `.it` — Individual status item with bullet

---

## API Contracts

### DashboardDataService Interface

```csharp
public interface IDashboardDataService
{
    /// <summary>
    /// Load and deserialize dashboard data from data.json.
    /// Result is cached in memory; subsequent calls return cached value.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>DashboardModel instance</returns>
    /// <exception cref="DashboardDataException">Thrown if file not found or JSON invalid</exception>
    Task<DashboardModel> GetDashboardDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reload data from disk, invalidating cache.
    /// Triggered by manual "Refresh Data" button on UI.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Completed task</returns>
    /// <exception cref="DashboardDataException">Thrown if file not found or JSON invalid</exception>
    Task ReloadDataAsync(CancellationToken cancellationToken = default);
}
```

### Error Handling

**Exception Hierarchy:**
```csharp
public class DashboardDataException : Exception
{
    public DashboardDataException(string message) : base(message) { }
    public DashboardDataException(string message, Exception inner) : base(message, inner) { }
}
```

**Error Response (UI):**
When `DashboardDataException` is caught during `GetDashboardDataAsync()`:
1. Log full exception details (including stack trace) to server logs
2. Display user-friendly message in UI:
   ```
   Unable to load dashboard data. Please check that data.json is properly formatted.
   ```
3. No stack trace or technical details shown to end user
4. Manual refresh button remains functional to retry after fixing data file

---

### Configuration (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*",
  "DataFilePath": "./wwwroot/data/data.json"
}
```

**Configuration Sources:**
- `appsettings.json` (default, checked into repo)
- `appsettings.Development.json` (dev overrides, local-only)
- `appsettings.Production.json` (prod overrides, optional)

**Usage in Code:**
```csharp
string dataPath = _config.GetValue<string>("DataFilePath", "./wwwroot/data/data.json");
```

---

## Infrastructure Requirements

### Hosting Environment

**Local Development:**
- Machine: Windows 10/11 or macOS/Linux with .NET 8 SDK
- Port: `https://localhost:5001` (default Kestrel HTTPS)
- Certificate: Self-signed (auto-generated by ASP.NET Core on first run)
- Command: `dotnet run`

**Local Production (Internal Network):**
- **Option A: Windows Service** (recommended for always-on availability)
  - Tool: TopShelf NuGet package (v4.4.1+)
  - Setup: Install via PowerShell, auto-start on Windows boot
  - Port: `http://192.168.x.x:5000` (or HTTPS with CA certificate)
  - Binding: Configure `appsettings.json` for network interface IP
  
- **Option B: Console Application**
  - Tool: Task Scheduler or manual launch
  - Port: `https://localhost:5001`
  - Reliability: Requires manual restart on crash
  
- **Option C: Docker Container**
  - Base Image: `mcr.microsoft.com/dotnet/aspnet:8.0`
  - Build: `docker build -t reporting-dashboard .`
  - Run: `docker run -p 5001:5001 reporting-dashboard`
  - Binding: Network accessible within Docker host or LAN (via host network)

### File System

**Required Directories:**
```
MyProject.Reporting/
├── wwwroot/
│   ├── data/
│   │   └── data.json                 [Project data, editable by PM]
│   ├── css/
│   │   └── app.css                   [Global styles]
│   └── js/
│       └── interop.js                [Optional JS interop]
└── appsettings.json                  [Config file paths, logging]
```

**File Permissions:**
- Application process: Read-only access to `data.json`
- No write operations on `data.json` (manual edits by PM)
- No database files or persistent state

**Data.json Location:**
- Default: `./wwwroot/data/data.json` (relative to app root)
- Configurable: `appsettings.json` key `DataFilePath`
- Example absolute path: `C:\Projects\MyProject.Reporting\wwwroot\data\data.json`

### Network & Ports

**Local Machine Only (Development):**
- Localhost: `https://localhost:5001`
- No external network access
- Certificate: Self-signed, trusted by OS

**Internal Corporate Network (Optional Production):**
- IP Binding: `http://192.168.1.100:5000` (example)
- Firewall: Add exception to Windows Firewall
- Certificate: Optional, self-signed or CA-issued
- DNS (optional): Map domain `reporting.internal` to IP via hosts file or DNS server

**No Internet Exposure:**
- Application not accessible from public internet
- No CDN, no cloud proxy, no DDoS protection needed

### Scalability & Resource Limits

**Single-Instance Deployment:**
- Concurrent Users: 1–5 typical (internal dashboard)
- Memory: ~100–200 MB RAM at idle, <500 MB under load
- Disk: ~50 MB for app binaries, data.json <1 MB
- CPU: Minimal; no background jobs or heavy computation
- Network: LAN latency only (<100 ms)

**Performance Targets:**
- Cold Start: <2 seconds (file I/O + component render)
- Page Render: Complete within 1920×1080 viewport (no scroll)
- Manual Refresh: <1 second (incremental re-render)
- Concurrent Connections: Blazor Server can handle ~100 concurrent clients per 1 GB RAM (rough estimate; test with load tools)

**Scaling Path (if needed >10 concurrent users):**
1. Migrate to Blazor WebAssembly (static HTML/JS compiled from C#)
2. Separate backend API service (ASP.NET Core Web API)
3. Deploy to cloud or internal load balancer
4. Add database for data persistence (currently file-based only)

---

## Technology Stack Decisions

### Frontend Layer

| Component | Technology | Version | Justification |
|---|---|---|---|
| **Web Framework** | Blazor Server | (built-in with .NET 8) | Server-side rendering eliminates need for React/Vue; full C# type safety; ideal for simple dashboards with no real-time updates |
| **Layout** | CSS Grid + Flexbox | native browser | No external framework (Tailwind optional); design already specifies Grid/Flex layout; minimal CSS overhead |
| **SVG Rendering** | Inline SVG (C# StringBuilder) | native browser | Direct control over timeline positioning; no D3.js or heavy charting library; simple SVG generation sufficient |
| **Styling** | app.css (global) + scoped Razor styles | native CSS | Scoped component styles prevent CSS conflicts; global palette via CSS variables recommended |

### Backend Layer

| Component | Technology | Version | Justification |
|---|---|---|---|
| **Web Server** | Kestrel (ASP.NET Core) | .NET 8.0+ | Built-in, high-performance, no IIS dependency; supports localhost + network binding |
| **Runtime** | .NET 8 LTS | 8.0.0+ | Latest long-term support release; modern async/await, LINQ, nullable reference types |
| **JSON Processing** | System.Text.Json | native (.NET 8) | Built-in, no external NuGet dependency; high performance; POCO deserialization support |
| **Dependency Injection** | ASP.NET Core DI Container | native (.NET 8) | Built-in, supports singleton/scoped/transient lifetimes; used for `IDashboardDataService` registration |
| **Logging** | ILogger (Microsoft.Extensions.Logging) | native (.NET 8) | Built-in, structured logging; supports console, file, custom sinks |
| **Configuration** | IConfiguration (appsettings.json) | native (.NET 8) | Built-in, supports environment-specific overrides; simple key-value binding |

### Data Layer

| Component | Technology | Version | Justification |
|---|---|---|---|
| **Data Source** | JSON File (data.json) | (standard format) | No database setup/maintenance; human-readable; version-controlled in Git; PM can edit manually |
| **Serialization** | System.Text.Json | native (.NET 8) | No external dependency; supports strongly-typed POCO deserialization |
| **Caching** | In-Memory Cache | native C# | Single-machine deployment; no distributed cache needed; `DashboardDataService` holds reference to deserialized model |

### Infrastructure Layer

| Component | Technology | Version | Justification |
|---|---|---|---|
| **Local Deployment** | .NET Runtime (.exe) | .NET 8 LTS | No Docker required; simple `dotnet run` command; minimal setup overhead |
| **Windows Service** (optional) | TopShelf | 4.4.1+ | NuGet package; wraps Kestrel in Windows Service; auto-start on boot |
| **Docker** (optional) | Docker + Linux container | latest | Simple `Dockerfile`; portable across machines; optional if team prefers containers |

### Testing & Development

| Component | Technology | Version | Justification |
|---|---|---|---|
| **Unit Testing** | xUnit | 2.7.0+ | Modern, async-friendly, parallel test execution |
| **Mocking** | Moq | 4.20.0+ | Simple mock generation; supports `IAsyncEnumerable` and complex scenarios |
| **Component Testing** | bUnit | 1.27.0+ | Blazor-specific testing library; verifies component lifecycle, event bindings, parameter passing |
| **IDE** | Visual Studio 2022 | 17.8+ | Full Blazor debugging, hot reload, integrated package manager |
| **CLI** | dotnet CLI | 8.0+ | Build, test, publish from PowerShell; no GUI required |

### Not Included (Intentionally Out of Scope)

- **Cloud Services**: No Azure App Service, Azure SQL, Application Insights (local-only)
- **Authentication**: No Azure AD, OAuth, JWT (localhost-only, no login required)
- **Frontend SPA Framework**: No React, Vue, Angular (server-side Blazor sufficient)
- **Heavy Charting Library**: No OxyPlot, Chart.js, ECharts (simple inline SVG sufficient)
- **CSS Framework**: No Bootstrap, Tailwind CSS (custom CSS Grid/Flexbox sufficient)
- **Build Tools**: No Webpack, npm, Gulp (ASP.NET Core build pipeline sufficient)
- **Database**: No SQL Server, PostgreSQL, MongoDB (JSON file sufficient)
- **Message Queue**: No RabbitMQ, ServiceBus (no real-time updates required)
- **CDN**: No CloudFlare, Akamai (local network only)

---

## Security Considerations

### Authentication & Authorization

**Current Design: No Authentication Required**
- Dashboard is localhost-only or corporate LAN-only (internal access)
- All users see identical dashboard content
- No user roles, permissions, or access control needed

**If Extended to External Network:**
Option 1: **Windows Authentication (Recommended for enterprise)**
```csharp
// In Program.cs
builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);
app.UseAuthentication();
app.UseAuthorization();

// In appsettings.json
"iisSettings": {
  "windowsAuthentication": true,
  "anonymousAuthentication": false
}
```

Option 2: **API Key (Simple alternative)**
```csharp
// Middleware to check hardcoded key
app.Use(async (context, next) =>
{
    string apiKey = context.Request.Headers["X-API-Key"];
    if (apiKey != "your-secret-key-here")
    {
        context.Response.StatusCode = 401;
        return;
    }
    await next();
});
```

### Data Protection

**Output Encoding:**
- Blazor automatically HTML-encodes all `@` expressions
- No inline HTML rendering without explicit `MarkupString`
- No XSS vulnerability by default

**Input Validation:**
- `data.json` is read-only from app perspective
- No user input on dashboard (no forms, no searches)
- JSON schema validation recommended (see `DashboardModelValidator` above)

**File Access:**
- Application reads `data.json` with minimal file permissions
- No write access to configuration files
- No access to sensitive OS files or system directories

**Secrets Management:**
- `appsettings.json` contains no secrets (file paths, log levels only)
- API keys (if needed) stored in `appsettings.Production.json` or environment variables
- Never commit sensitive data to Git; use `.gitignore`

### Network Security

**HTTPS (Recommended):**
- Blazor Server defaults to HTTPS on port 5001
- Self-signed certificate auto-generated on first run
- Certificate trusted by OS for localhost
- No certificate pinning or advanced validation needed for internal use

**HTTP (Development Only):**
- Can disable HTTPS in `appsettings.Development.json` for faster iteration
- Not recommended for shared network access

**Network Isolation:**
- No firewall exceptions needed for localhost (`127.0.0.1`)
- Corporate LAN access: Add Windows Firewall exception to Kestrel port
- No public internet exposure; IP whitelisting not required

**Transport Security:**
- All data transmitted via HTTPS (or HTTP on localhost)
- No sensitive data in URL query strings
- No API keys or credentials in HTTP headers (not applicable here)

### Deployment Security

**Windows Service (if deployed):**
- Run as least-privileged service account (not SYSTEM)
- Service account needs read-only access to `data.json`
- No network service account hardening required for internal LAN

**Docker Container (if deployed):**
- Use official `mcr.microsoft.com/dotnet/aspnet:8.0` base image
- Run container as non-root user
- Bind only necessary ports (5000/5001)
- Mount `data.json` as read-only volume

### Logging & Monitoring

**Server-Side Logging:**
```csharp
_logger.LogError(ex, "Error loading dashboard data from {Path}", dataPath);
_logger.LogInformation("Dashboard data reloaded; {ItemCount} status items", totalItems);
```

**What to Log:**
- File I/O errors (missing file, permission denied)
- JSON deserialization errors (malformed data)
- Data validation failures (missing required fields)
- Manual refresh requests (timestamp, success/failure)

**What NOT to Log:**
- User IP addresses (no authentication concept)
- HTTP request headers (no sensitive headers)
- Full data.json contents (no privacy risk, but verbose)

**Log Output:**
- Console (development): `dotnet run` output
- Event Viewer (Windows Service): Application event log
- Docker: `docker logs <container_id>`
- Optional: File-based logging via Serilog (NuGet package)

### Vulnerability Mitigation

| Vulnerability | Risk | Mitigation |
|---|---|---|
| **XSS (Cross-Site Scripting)** | Low | Blazor HTML-encodes by default; no user input accepted |
| **SQL Injection** | N/A | No database; JSON file only |
| **CSRF** | Low | No form submissions; read-only dashboard |
| **File Traversal** | Low | File path hardcoded in config; no user-supplied paths |
| **Denial of Service** | Low | Single-machine, internal use; no rate limiting needed |
| **Data Leakage** | Low | No sensitive data in JSON; no external APIs |

---

## Scaling Strategy

### Current Architecture (Single-Instance)

**Capacity:**
- Concurrent Users: 1–5 (internal dashboard)
- Data Size: `data.json` <1 MB (12 months × 10 items per cell)
- Memory: 100–200 MB RAM
- Network: LAN only (<100 ms latency)
- Compute: Minimal; no background jobs

**Bottlenecks:**
- File I/O: If `data.json` is edited frequently, file locks could cause issues
- Blazor Server WebSocket: ~100 concurrent connections per 1 GB RAM (rough estimate)
- SVG Rendering: Large timelines (20+ milestones) may cause client-side lag

### Scaling Path (If Usage Exceeds Current Capacity)

**Phase 1: Increase Single-Instance Capacity**
- Move to dedicated server (2+ GB RAM, modern CPU)
- Use Windows Service for better resource management
- Implement file caching strategy (e.g., 5-minute TTL)
- Monitor memory/CPU with Windows Performance Monitor

**Phase 2: Migrate to Blazor WebAssembly (Static + API)**
- Pros: Scalable frontend (static HTML/CSS/JS compiled from C#); backend can handle 100+ concurrent users
- Cons: Requires separate API service; more deployment complexity
- Architecture:
  ```
  [Browser]
    ↓ (WASM app + Blazor runtime)
  [API Service] ← /api/dashboard (JSON response)
    ↓
  [Data Source] (File, DB, or service)
  ```

**Phase 3: Add Backend Database**
- Replace JSON file with SQL Server / PostgreSQL
- Benefits: Concurrent write support, transaction safety, query optimization
- Cons: Adds operational complexity, requires DBA support
- Implementation: Replace `DashboardDataService` to read from database instead of file

**Phase 4: Implement Caching Layer**
- Add Redis or in-memory cache with distributed invalidation
- Cache dashboard data for 5–10 minutes
- Benefits: Reduced database load, faster page loads
- Cons: Adds cache invalidation complexity

**Phase 5: Horizontal Scaling (Cloud or On-Premise)**
- Deploy multiple API instances behind load balancer (nginx, HAProxy)
- Use centralized database (shared by all instances)
- Add monitoring & auto-scaling rules
- Cons: Significant operational overhead; recommend only if organization has DevOps capability

### Recommended Scaling Approach for This Project

**MVP (Current): Single Kestrel Instance**
- Simple, no operational overhead
- Sufficient for 1–5 concurrent users
- Deployment: `dotnet run` or Windows Service

**Growth (Year 1):**
- Monitor usage; if exceeds 10 concurrent users, evaluate Phase 1–2
- Likely unnecessary for internal executive dashboard (low concurrency)
- Consider only if dashboard becomes shared across multiple teams/time zones

**Long-Term (Year 2+):**
- If dashboard becomes strategic asset across organization, migrate to Blazor WebAssembly + API
- Consider database if data changes frequently or requires historical tracking

---

## Risks & Mitigations

### Technical Risks

| Risk | Severity | Likelihood | Mitigation |
|---|---|---|---|
| **Blazor Component Lifecycle Complexity** | Medium | Medium | Document lifecycle clearly; prototype components early; use bUnit tests to verify behavior |
| **SignalR Connection Drops** | Medium | Low | Implement reconnection logic with exponential backoff; show "Reconnecting..." banner; manual refresh button |
| **SVG Rendering Performance** | Medium | Low | Profile with DevTools; limit timeline to 12 months; pre-render static SVG if unchanged |
| **data.json File Locks** | Medium | Low | Implement retry logic with exponential backoff; cache data in memory; notify PM if update fails |
| **JSON Deserialization Errors** | High | Medium | Validate JSON schema at startup; catch exceptions; display user-friendly error message; log details |
| **Memory Leak in Blazor Components** | Low | Low | Follow Microsoft best practices for component disposal; use `@implements IAsyncDisposable` if needed |

### Data Risks

| Risk | Severity | Likelihood | Mitigation |
|---|---|---|---|
| **data.json Missing or Deleted** | High | Low | Check file existence at startup; cache previous version if available; fallback to empty dashboard with error message |
| **data.json Malformed (Invalid JSON)** | High | Medium | Use `JsonSerializerOptions.PropertyNameCaseInsensitive` for flexibility; validate with JSON schema; log parsing errors |
| **Milestone Dates Out of Order** | Medium | Low | Sort milestones by date on load; log warnings for data quality issues |
| **Missing Required Fields** | High | Medium | Validate all required fields on deserialization; throw `DashboardDataException` with field name; display user-friendly error |
| **Very Long Status Item Text** | Low | Medium | Set `overflow: hidden` on cells; add optional tooltip for full text; truncate at 100 characters in validation |

### Infrastructure Risks

| Risk | Severity | Likelihood | Mitigation |
|---|---|---|---|
| **Kestrel Server Crashes** | High | Low | Wrap in Windows Service with auto-restart; monitor with health-check endpoint; log unhandled exceptions |
| **Port Already in Use** | Medium | Medium | Use configurable port in `appsettings.json`; document port number in README; check `netstat` if conflict occurs |
| **Self-Signed Certificate Untrusted** | Low | Low | Auto-generated by ASP.NET Core for localhost; no issue on initial machine; recreate if moved to new machine |
| **Network Firewall Blocks Kestrel** | Medium | Low | Add Windows Firewall exception during setup; document in deployment guide; test connectivity before rollout |
| **Insufficient Disk Space** | Low | Low | App is ~50 MB; data.json <1 MB; no risk unless drive is nearly full |

### Operational Risks

| Risk | Severity | Likelihood | Mitigation |
|---|---|---|---|
| **PM Accidentally Deletes data.json** | Medium | Low | Keep backup in version control (Git); restore from repo if deleted; send PM update guide emphasizing caution |
| **Multiple PMs Edit data.json Simultaneously** | Low | Low | Use file locking mechanism (e.g., `.lock` file) or centralized system; document PM edit workflow |
| **Executives Confused by Dashboard Visuals** | Low | Medium | Provide legend and tooltip explanations; include brief user guide in first email; host walkthrough meeting |
| **No One Knows How to Restart Service** | Medium | Low | Document Windows Service restart steps in README; create PowerShell script (`restart-dashboard.ps1`) |
| **data.json Format Changes Break App** | Medium | Low | Use semantic versioning for `data.json` schema; validate schema version on load; provide migration guide for schema changes |

### User Experience Risks

| Risk | Severity | Likelihood | Mitigation |
|---|---|---|---|
| **Dashboard Doesn't Fit 1920×1080 (Overflow)** | High | Low | Test layout thoroughly at exactly 1920×1080 in Chrome DevTools; use `overflow: hidden` on body; document browser zoom requirement |
| **Colors Don't Match Design Mock** | Medium | Medium | Extract CSS directly from `OriginalDesignConcept.html`; use color picker to verify hex codes; side-by-side visual comparison test |
| **Fonts Look Different** | Low | Medium | Verify Segoe UI font is installed on target machines; use system font stack fallback; test on multiple machines |
| **Tooltip Doesn't Appear on Hover** | Low | Low | Implement hover tooltips as optional feature; document as stretch goal; not critical for MVP |
| **Screenshot Includes Browser Tabs/URL Bar** | Low | Low | Provide instructions for full-page screenshot (Chrome DevTools, Win+Shift+S); document in user guide |

### Mitigation Strategies (General)

1. **Early Prototype:** Build static HTML mock in Phase 1 before committing to Blazor complexity
2. **Load Testing:** Use Apache JMeter or similar to test concurrent user limits before deployment
3. **Error Budget:** Plan for ~5 hours of troubleshooting during implementation and deployment
4. **Documentation:** Provide README with setup, configuration, troubleshooting, and PM edit guide
5. **Monitoring:** Log all errors to file or Windows Event Viewer; review logs weekly during first month
6. **Fallback Plan:** If Blazor component lifecycle causes issues, fall back to pure HTML/CSS + minimal C# (simpler but less maintainable)
7. **Version Control:** Store `data.json` samples and configuration examples in Git; enable rollback

---

**Architecture Version:** 1.0  
**Date:** 2026-04-12  
**Technology Stack:** C# .NET 8 Blazor Server, Local File-Based Data, Fixed 1920×1080 Viewport  
**Status:** Ready for Implementation