# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-21 04:56 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipping status, in-progress work, carryover items, and blockers. The dashboard reads from a local `data.json` configuration file and renders a pixel-perfect view optimized for 1920×1080 screenshots destined for PowerPoint executive decks. **Primary Recommendation:** Build this as a minimal Blazor Server application with zero authentication, no database, and no external cloud dependencies. Use `System.Text.Json` for data loading, pure CSS Grid/Flexbox for layout (matching the reference HTML design), and inline SVG rendering via Blazor components for the timeline/Gantt visualization. No charting library is needed — the design is simple enough that hand-crafted SVG in Razor components will produce cleaner, more controllable output than any third-party library. The entire solution should be 5–8 files and deliverable in a single sprint. ---

### Key Findings

- The reference design (`OriginalDesignConcept.html`) uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`), **Flexbox**, and **inline SVG** — all of which Blazor Server renders natively without any JavaScript interop.
- The design targets a **fixed 1920×1080 viewport** for screenshot capture, eliminating the need for responsive design or mobile considerations.
- **No charting library is warranted.** The timeline is a simple SVG with lines, circles, diamonds, and text labels. A Blazor component generating `<svg>` markup from `data.json` milestones is simpler, lighter, and more controllable than pulling in a charting dependency.
- The heatmap grid is pure **CSS Grid with colored cells** — no canvas, no WebGL, no charting framework. Each cell contains a small list of text items with colored bullet indicators.
- **`System.Text.Json`** (built into .NET 8) is the only data access layer needed. The `data.json` file is read once at startup and optionally watched for changes via `FileSystemWatcher`.
- Blazor Server's **SignalR circuit** is irrelevant for this use case (single local user, no real-time collaboration), but it's the mandated stack and works fine for local-only operation.
- The color palette is fully defined in the HTML reference: greens for shipped, blues for in-progress, ambers for carryover, reds for blockers. These should be extracted into CSS custom properties for maintainability.
- **No authentication, no authorization, no HTTPS complexity.** This is a local tool for one person to generate screenshots.
- The `.sln` structure should contain a single project (`ReportingDashboard.csproj`) — adding a class library for models is over-engineering for this scope. --- **Goal:** Pixel-perfect match of the reference design, driven by `data.json`.
- **Scaffold the project** — `dotnet new blazor --interactivity Server -n ReportingDashboard`
- **Define `data.json` schema and C# models** — Start with the data model, create sample data matching the reference design's fictional project
- **Build `DashboardDataService`** — Singleton service that reads and deserializes `data.json` at startup
- **Implement `Header.razor`** — Title, subtitle, legend icons (diamond/circle/line)
- **Implement `Timeline.razor`** — SVG generation with date-to-pixel positioning, track lines, milestone shapes
- **Implement `HeatmapGrid.razor` + `HeatmapCell.razor`** — CSS Grid matching the reference layout exactly
- **Style with `app.css`** — Port all styles from `OriginalDesignConcept.html`, using CSS custom properties for colors
- **Test with browser** — Open at 1920×1080, compare side-by-side with reference screenshot
- **Error handling** — Friendly error display if `data.json` is missing or malformed
- **File watching** — Auto-reload dashboard when `data.json` is saved (via `FileSystemWatcher` + `InvokeAsync(StateHasChanged)`)
- **Print stylesheet** — `@media print` rules for clean PDF export if needed
- **Sample data** — Create a compelling fictional project ("Project Phoenix — Cloud Migration Platform") with realistic milestones and status items
- **Automated screenshots** — Puppeteer-Sharp utility to generate PNGs on demand
- **Multiple projects** — Dropdown selector loading different JSON files from a `data/` directory
- **Date auto-advance** — "NOW" marker and current-month highlighting derived from system clock
- **Subtle animations** — CSS transitions on page load for a polished feel (fade-in cells) | Quick Win | Effort | Impact | |-----------|--------|--------| | Port reference CSS directly into `app.css` | 30 min | Immediate visual fidelity | | Create `data.json` with fictional but realistic sample data | 30 min | Demo-ready from day one | | Use `dotnet watch` for hot reload during development | 0 min (built-in) | Fast iteration on CSS/layout | | Add CSS custom properties for the color palette | 15 min | Easy theming, maintainable code |
- ❌ No authentication or login page
- ❌ No database (SQLite, LiteDB, or otherwise)
- ❌ No REST API or Web API controllers
- ❌ No SignalR hubs (Blazor Server's built-in circuit is sufficient)
- ❌ No Docker containerization
- ❌ No CI/CD pipeline (local tool, manual deployment)
- ❌ No third-party CSS framework (Bootstrap, Tailwind, MudBlazor)
- ❌ No JavaScript charting library
- ❌ No responsive/mobile design (fixed 1920×1080 target)
- ❌ No unit test project (optional for this scope — add only if time permits)
```xml
<!-- ReportingDashboard.csproj — the ONLY packages needed -->
<ItemGroup>
    <!-- None beyond the default Blazor Server template! -->
    <!-- System.Text.Json is included in the framework -->
    <!-- All rendering is native Razor + CSS + SVG -->
</ItemGroup>

<!-- Optional, only if automated screenshots are desired -->
<ItemGroup Condition="'$(Configuration)' == 'Debug'">
    <PackageReference Include="PuppeteerSharp" Version="4.0.0" />
</ItemGroup>
``` **Total external dependencies: Zero (for the core app).** This is intentional and correct for the scope.

### Recommended Tools & Technologies

- | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **UI Framework** | Blazor Server (.NET 8) | `net8.0` / `Microsoft.AspNetCore.Components 8.0.x` | Server-side rendering with Razor components | | **Layout** | CSS Grid + Flexbox | Native CSS3 | Matches reference design exactly | | **Timeline Visualization** | Inline SVG via Razor | Native HTML5 SVG | Milestone diamonds, checkpoint circles, progress lines | | **Icons/Shapes** | SVG primitives (`<polygon>`, `<circle>`, `<line>`) | N/A | Diamond milestones, circular checkpoints, dashed "now" line | | **Fonts** | Segoe UI (system font) | N/A | Matches reference; no web font loading needed | | **CSS Architecture** | Single `app.css` + CSS custom properties | N/A | Color tokens, grid definitions | **Why no component library (MudBlazor, Radzen, etc.):** These libraries add 200KB+ of CSS/JS, impose their own design language, and make pixel-perfect matching of the reference design harder, not easier. The reference design is 100% achievable with plain HTML/CSS rendered by Razor components. Adding a UI framework would be negative ROI. **Why no charting library (ApexCharts.Blazor, ChartJs.Blazor, etc.):** The "chart" in the design is a horizontal timeline with positioned SVG shapes. Charting libraries are designed for bar charts, line charts, and pie charts — forcing them to render a custom Gantt-style timeline would require more configuration code than just writing the SVG directly. Direct SVG gives pixel-perfect control. | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Deserialize `data.json` into C# models | | **File Watching** | `FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload when `data.json` changes | | **DI Service** | `IHostedService` or scoped service | Built into .NET 8 | Load data at startup, provide to components | | **Configuration** | `appsettings.json` | Built into .NET 8 | Path to `data.json`, Kestrel port | **No database.** The data source is a single JSON file. Adding SQLite, LiteDB, or any persistence layer is unnecessary complexity. | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **SDK** | .NET 8 SDK | `8.0.400+` | Build and run | | **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Latest | Development | | **Hot Reload** | `dotnet watch` | Built into .NET 8 SDK | Iterative CSS/Razor development | | **Screenshot** | Browser print / Snipping Tool / Puppeteer-Sharp (optional) | N/A | Capture 1920×1080 PNG for PowerPoint | | Layer | Technology | Version | Purpose | |-------|-----------|---------|---------| | **Unit Testing** | xUnit | `2.9.x` | Test JSON deserialization, data model logic | | **Component Testing** | bUnit | `1.31.x` | Test Razor component rendering | | **Assertions** | FluentAssertions | `6.12.x` | Readable test assertions | Given the project's simplicity (single-page, read-only, local), testing is optional but recommended for the data deserialization layer. --- This is a read-only, single-page, single-user dashboard reading one JSON file. The appropriate architecture is the simplest one that works:
```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs                    # Minimal hosting, register services
    ├── Data/
    │   ├── data.json                 # The data source (editable by user)
    │   └── DashboardData.cs          # C# POCO models matching JSON schema
    ├── Services/
    │   └── DashboardDataService.cs   # Loads & caches data.json, exposes to components
    ├── Components/
    │   ├── App.razor                 # Root component
    │   ├── Layout/
    │   │   └── MainLayout.razor      # Full-page layout (no nav, no sidebar)
    │   └── Pages/
    │       └── Dashboard.razor       # The single page
    ├── Components/Shared/
    │   ├── Header.razor              # Title, subtitle, legend
    │   ├── Timeline.razor            # SVG milestone visualization
    │   ├── HeatmapGrid.razor         # CSS Grid status matrix
    │   └── HeatmapCell.razor         # Individual cell with item bullets
    └── wwwroot/
        └── css/
            └── app.css               # All styles (CSS Grid, colors, typography)
```
```
data.json  →  DashboardDataService (singleton, loaded at startup)
                    ↓
           Dashboard.razor (injects service)
                    ↓
    ┌───────────────┼───────────────┐
    ↓               ↓               ↓
Header.razor   Timeline.razor   HeatmapGrid.razor
                                     ↓
                              HeatmapCell.razor (×N)
```
```csharp
public class DashboardData
{
    public ProjectInfo Project { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<TimelineTrack> TimelineTracks { get; set; }
    public HeatmapData Heatmap { get; set; }
}

public class ProjectInfo
{
    public string Title { get; set; }          // "Privacy Automation Release Roadmap"
    public string Subtitle { get; set; }       // "Trusted Platform · Privacy Automation..."
    public string BacklogUrl { get; set; }     // ADO link
    public string CurrentMonth { get; set; }   // "April 2026"
}

public class Milestone
{
    public string Label { get; set; }          // "Mar 26 PoC"
    public DateTime Date { get; set; }
    public string Type { get; set; }           // "poc" | "production" | "checkpoint"
}

public class TimelineTrack
{
    public string Id { get; set; }             // "M1", "M2", "M3"
    public string Name { get; set; }           // "Chatbot & MS Role"
    public string Color { get; set; }          // "#0078D4"
    public List<Milestone> Milestones { get; set; }
}

public class HeatmapData
{
    public List<string> Columns { get; set; }  // ["Jan", "Feb", "Mar", "Apr"]
    public string CurrentColumn { get; set; }  // "Apr"
    public List<HeatmapRow> Rows { get; set; }
}

public class HeatmapRow
{
    public string Category { get; set; }       // "Shipped" | "In Progress" | "Carryover" | "Blockers"
    public Dictionary<string, List<string>> Items { get; set; } // month → items
}
``` Extract the reference design's color palette into CSS custom properties for maintainability:
```css
:root {
    /* Category colors */
    --shipped-primary: #34A853;
    --shipped-bg: #F0FBF0;
    --shipped-bg-current: #D8F2DA;
    --shipped-header-bg: #E8F5E9;
    --shipped-text: #1B7A28;

    --progress-primary: #0078D4;
    --progress-bg: #EEF4FE;
    --progress-bg-current: #DAE8FB;
    --progress-header-bg: #E3F2FD;
    --progress-text: #1565C0;

    --carryover-primary: #F4B400;
    --carryover-bg: #FFFDE7;
    --carryover-bg-current: #FFF0B0;
    --carryover-header-bg: #FFF8E1;
    --carryover-text: #B45309;

    --blocker-primary: #EA4335;
    --blocker-bg: #FFF5F5;
    --blocker-bg-current: #FFE4E4;
    --blocker-header-bg: #FEF2F2;
    --blocker-text: #991B1B;

    /* Layout */
    --page-width: 1920px;
    --page-height: 1080px;
    --side-padding: 44px;
    --grid-label-width: 160px;
}
``` The Timeline.razor component should:
- Accept `List<TimelineTrack>` and a `DateRange` (start/end months)
- Calculate X positions proportionally: `xPos = (date - startDate) / (endDate - startDate) * svgWidth`
- Render each track as a horizontal `<line>` with milestones placed at calculated positions
- Render the "NOW" line as a dashed vertical `<line>` at today's calculated X position
- Use `<polygon>` for diamond milestones, `<circle>` for checkpoints This is ~80 lines of Razor markup with `@foreach` loops — no JS interop needed. ---

### Considerations & Risks

- | Concern | Approach | |---------|----------| | **Authentication** | None. No login, no cookies, no tokens. | | **Authorization** | None. Single-user local tool. | | **HTTPS** | Not required. Run on `http://localhost:5000`. | | **CORS** | Not applicable (no API consumers). | | **Input validation** | Validate `data.json` schema on load; display friendly error if malformed. | | **Data sensitivity** | The JSON file contains project names and status — no PII, no secrets. | | Concern | Approach | |---------|----------| | **Hosting** | Kestrel on localhost, launched via `dotnet run` | | **Deployment** | `dotnet publish -c Release -o ./publish` → run the executable | | **Port** | Default `5000` (HTTP only), configurable via `appsettings.json` | | **Process lifetime** | Run when needed, close when done. No daemon, no service. | | **Auto-start** | Optional: Windows shortcut or batch file in Startup folder | | **Cost** | $0. No cloud, no hosting, no licenses. |
```bash
# Development
dotnet watch --project ReportingDashboard

# Production (screenshot capture)
dotnet run --project ReportingDashboard --configuration Release
# → Open http://localhost:5000 → Screenshot → Close
``` If manual screenshots become tedious, add **Puppeteer-Sharp** (`4.x`) as a dev dependency to programmatically capture 1920×1080 PNGs:
```csharp
// Optional utility - not part of the main app
await new BrowserFetcher().DownloadAsync();
var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
var page = await browser.NewPageAsync();
await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });
await page.GoToAsync("http://localhost:5000");
await page.ScreenshotAsync("dashboard.png", new ScreenshotOptions { FullPage = false });
``` This is a "nice to have" — not a core requirement. --- | Risk | Impact | Mitigation | |------|--------|------------| | **Blazor Server overhead for a static page** | Unnecessary SignalR connection for what's essentially a static render | Acceptable trade-off — it's the mandated stack, and the overhead is invisible for local single-user use | | **Browser rendering differences** | Screenshot may look different across Edge/Chrome/Firefox | Standardize on one browser (Edge Chromium recommended on Windows) for all screenshots | | **`data.json` schema drift** | User edits JSON incorrectly, page crashes | Add `try/catch` on deserialization with a friendly error page showing what's wrong | | **SVG positioning math** | Timeline dates-to-pixels calculation could have off-by-one issues | Unit test the date-to-pixel calculation function with known inputs | | Concern | Why it's not a risk | |---------|-------------------| | **Scalability** | Single user, single machine. Not applicable. | | **High availability** | Run on demand. No uptime requirement. | | **Data integrity** | JSON file is the source of truth, edited manually. No concurrent writes. | | **Performance** | One page, one data file, one user. Sub-100ms render guaranteed. | | **Security breaches** | localhost-only, no network exposure, no sensitive data. | The reference design is already a working static HTML file. Using Blazor Server adds:
- **Benefit:** Data-driven rendering from `data.json` (the core requirement), component reuse if more dashboards are needed, familiar C# tooling
- **Cost:** Requires `dotnet` runtime, SignalR connection overhead, slightly more complex than opening an HTML file
- **Verdict:** The benefit justifies the cost. Manually editing HTML for each reporting period is error-prone and slow. Blazor + JSON is the right call. --- | # | Question | Recommended Default | Needs Input? | |---|----------|-------------------|--------------| | 1 | **How many months should the heatmap show?** The reference shows 4 (Jan–Apr). Should this be configurable? | Default to 4 most recent months, configurable in `data.json` | Low priority — default is fine | | 2 | **How many timeline tracks?** The reference shows 3 (M1, M2, M3). Is this fixed or variable? | Variable, driven by `data.json` array length | No — make it data-driven | | 3 | **Should the "NOW" line auto-calculate from system date or be set in `data.json`?** | Auto-calculate from `DateTime.Now` but allow override in JSON | No — auto-calculate | | 4 | **Will multiple projects/dashboards be needed?** | Design for one, but the component structure supports adding a project selector later | Worth asking stakeholder | | 5 | **Is there a preferred browser for screenshots?** | Edge Chromium at 100% zoom, 1920×1080 | Worth confirming | | 6 | **Should `data.json` support comments or be strict JSON?** | Strict JSON (use `//` comments if switching to JSON5, but not recommended) | No — keep it simple | | 7 | **What date range should the timeline cover?** Reference shows Jan–Jun (6 months). | Configurable in `data.json` with `startDate` and `endDate` fields | No — make it configurable | ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, shipping status, in-progress work, carryover items, and blockers. The dashboard reads from a local `data.json` configuration file and renders a pixel-perfect view optimized for 1920×1080 screenshots destined for PowerPoint executive decks.

**Primary Recommendation:** Build this as a minimal Blazor Server application with zero authentication, no database, and no external cloud dependencies. Use `System.Text.Json` for data loading, pure CSS Grid/Flexbox for layout (matching the reference HTML design), and inline SVG rendering via Blazor components for the timeline/Gantt visualization. No charting library is needed — the design is simple enough that hand-crafted SVG in Razor components will produce cleaner, more controllable output than any third-party library. The entire solution should be 5–8 files and deliverable in a single sprint.

---

## 2. Key Findings

- The reference design (`OriginalDesignConcept.html`) uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`), **Flexbox**, and **inline SVG** — all of which Blazor Server renders natively without any JavaScript interop.
- The design targets a **fixed 1920×1080 viewport** for screenshot capture, eliminating the need for responsive design or mobile considerations.
- **No charting library is warranted.** The timeline is a simple SVG with lines, circles, diamonds, and text labels. A Blazor component generating `<svg>` markup from `data.json` milestones is simpler, lighter, and more controllable than pulling in a charting dependency.
- The heatmap grid is pure **CSS Grid with colored cells** — no canvas, no WebGL, no charting framework. Each cell contains a small list of text items with colored bullet indicators.
- **`System.Text.Json`** (built into .NET 8) is the only data access layer needed. The `data.json` file is read once at startup and optionally watched for changes via `FileSystemWatcher`.
- Blazor Server's **SignalR circuit** is irrelevant for this use case (single local user, no real-time collaboration), but it's the mandated stack and works fine for local-only operation.
- The color palette is fully defined in the HTML reference: greens for shipped, blues for in-progress, ambers for carryover, reds for blockers. These should be extracted into CSS custom properties for maintainability.
- **No authentication, no authorization, no HTTPS complexity.** This is a local tool for one person to generate screenshots.
- The `.sln` structure should contain a single project (`ReportingDashboard.csproj`) — adding a class library for models is over-engineering for this scope.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server Components + CSS)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **UI Framework** | Blazor Server (.NET 8) | `net8.0` / `Microsoft.AspNetCore.Components 8.0.x` | Server-side rendering with Razor components |
| **Layout** | CSS Grid + Flexbox | Native CSS3 | Matches reference design exactly |
| **Timeline Visualization** | Inline SVG via Razor | Native HTML5 SVG | Milestone diamonds, checkpoint circles, progress lines |
| **Icons/Shapes** | SVG primitives (`<polygon>`, `<circle>`, `<line>`) | N/A | Diamond milestones, circular checkpoints, dashed "now" line |
| **Fonts** | Segoe UI (system font) | N/A | Matches reference; no web font loading needed |
| **CSS Architecture** | Single `app.css` + CSS custom properties | N/A | Color tokens, grid definitions |

**Why no component library (MudBlazor, Radzen, etc.):** These libraries add 200KB+ of CSS/JS, impose their own design language, and make pixel-perfect matching of the reference design harder, not easier. The reference design is 100% achievable with plain HTML/CSS rendered by Razor components. Adding a UI framework would be negative ROI.

**Why no charting library (ApexCharts.Blazor, ChartJs.Blazor, etc.):** The "chart" in the design is a horizontal timeline with positioned SVG shapes. Charting libraries are designed for bar charts, line charts, and pie charts — forcing them to render a custom Gantt-style timeline would require more configuration code than just writing the SVG directly. Direct SVG gives pixel-perfect control.

### Backend (Data Layer)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **JSON Parsing** | `System.Text.Json` | Built into .NET 8 | Deserialize `data.json` into C# models |
| **File Watching** | `FileSystemWatcher` | Built into .NET 8 | Optional: auto-reload when `data.json` changes |
| **DI Service** | `IHostedService` or scoped service | Built into .NET 8 | Load data at startup, provide to components |
| **Configuration** | `appsettings.json` | Built into .NET 8 | Path to `data.json`, Kestrel port |

**No database.** The data source is a single JSON file. Adding SQLite, LiteDB, or any persistence layer is unnecessary complexity.

### Infrastructure / Tooling

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **SDK** | .NET 8 SDK | `8.0.400+` | Build and run |
| **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Latest | Development |
| **Hot Reload** | `dotnet watch` | Built into .NET 8 SDK | Iterative CSS/Razor development |
| **Screenshot** | Browser print / Snipping Tool / Puppeteer-Sharp (optional) | N/A | Capture 1920×1080 PNG for PowerPoint |

### Testing (Lightweight)

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Unit Testing** | xUnit | `2.9.x` | Test JSON deserialization, data model logic |
| **Component Testing** | bUnit | `1.31.x` | Test Razor component rendering |
| **Assertions** | FluentAssertions | `6.12.x` | Readable test assertions |

Given the project's simplicity (single-page, read-only, local), testing is optional but recommended for the data deserialization layer.

---

## 4. Architecture Recommendations

### Overall Pattern: **Minimal Blazor Server — No Layers, No Abstractions**

This is a read-only, single-page, single-user dashboard reading one JSON file. The appropriate architecture is the simplest one that works:

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs                    # Minimal hosting, register services
    ├── Data/
    │   ├── data.json                 # The data source (editable by user)
    │   └── DashboardData.cs          # C# POCO models matching JSON schema
    ├── Services/
    │   └── DashboardDataService.cs   # Loads & caches data.json, exposes to components
    ├── Components/
    │   ├── App.razor                 # Root component
    │   ├── Layout/
    │   │   └── MainLayout.razor      # Full-page layout (no nav, no sidebar)
    │   └── Pages/
    │       └── Dashboard.razor       # The single page
    ├── Components/Shared/
    │   ├── Header.razor              # Title, subtitle, legend
    │   ├── Timeline.razor            # SVG milestone visualization
    │   ├── HeatmapGrid.razor         # CSS Grid status matrix
    │   └── HeatmapCell.razor         # Individual cell with item bullets
    └── wwwroot/
        └── css/
            └── app.css               # All styles (CSS Grid, colors, typography)
```

### Data Flow

```
data.json  →  DashboardDataService (singleton, loaded at startup)
                    ↓
           Dashboard.razor (injects service)
                    ↓
    ┌───────────────┼───────────────┐
    ↓               ↓               ↓
Header.razor   Timeline.razor   HeatmapGrid.razor
                                     ↓
                              HeatmapCell.razor (×N)
```

### Data Model Design (matching the reference HTML)

```csharp
public class DashboardData
{
    public ProjectInfo Project { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<TimelineTrack> TimelineTracks { get; set; }
    public HeatmapData Heatmap { get; set; }
}

public class ProjectInfo
{
    public string Title { get; set; }          // "Privacy Automation Release Roadmap"
    public string Subtitle { get; set; }       // "Trusted Platform · Privacy Automation..."
    public string BacklogUrl { get; set; }     // ADO link
    public string CurrentMonth { get; set; }   // "April 2026"
}

public class Milestone
{
    public string Label { get; set; }          // "Mar 26 PoC"
    public DateTime Date { get; set; }
    public string Type { get; set; }           // "poc" | "production" | "checkpoint"
}

public class TimelineTrack
{
    public string Id { get; set; }             // "M1", "M2", "M3"
    public string Name { get; set; }           // "Chatbot & MS Role"
    public string Color { get; set; }          // "#0078D4"
    public List<Milestone> Milestones { get; set; }
}

public class HeatmapData
{
    public List<string> Columns { get; set; }  // ["Jan", "Feb", "Mar", "Apr"]
    public string CurrentColumn { get; set; }  // "Apr"
    public List<HeatmapRow> Rows { get; set; }
}

public class HeatmapRow
{
    public string Category { get; set; }       // "Shipped" | "In Progress" | "Carryover" | "Blockers"
    public Dictionary<string, List<string>> Items { get; set; } // month → items
}
```

### CSS Architecture

Extract the reference design's color palette into CSS custom properties for maintainability:

```css
:root {
    /* Category colors */
    --shipped-primary: #34A853;
    --shipped-bg: #F0FBF0;
    --shipped-bg-current: #D8F2DA;
    --shipped-header-bg: #E8F5E9;
    --shipped-text: #1B7A28;

    --progress-primary: #0078D4;
    --progress-bg: #EEF4FE;
    --progress-bg-current: #DAE8FB;
    --progress-header-bg: #E3F2FD;
    --progress-text: #1565C0;

    --carryover-primary: #F4B400;
    --carryover-bg: #FFFDE7;
    --carryover-bg-current: #FFF0B0;
    --carryover-header-bg: #FFF8E1;
    --carryover-text: #B45309;

    --blocker-primary: #EA4335;
    --blocker-bg: #FFF5F5;
    --blocker-bg-current: #FFE4E4;
    --blocker-header-bg: #FEF2F2;
    --blocker-text: #991B1B;

    /* Layout */
    --page-width: 1920px;
    --page-height: 1080px;
    --side-padding: 44px;
    --grid-label-width: 160px;
}
```

### SVG Timeline Rendering Strategy

The Timeline.razor component should:
1. Accept `List<TimelineTrack>` and a `DateRange` (start/end months)
2. Calculate X positions proportionally: `xPos = (date - startDate) / (endDate - startDate) * svgWidth`
3. Render each track as a horizontal `<line>` with milestones placed at calculated positions
4. Render the "NOW" line as a dashed vertical `<line>` at today's calculated X position
5. Use `<polygon>` for diamond milestones, `<circle>` for checkpoints

This is ~80 lines of Razor markup with `@foreach` loops — no JS interop needed.

---

## 5. Security & Infrastructure

### Security: Intentionally Minimal

| Concern | Approach |
|---------|----------|
| **Authentication** | None. No login, no cookies, no tokens. |
| **Authorization** | None. Single-user local tool. |
| **HTTPS** | Not required. Run on `http://localhost:5000`. |
| **CORS** | Not applicable (no API consumers). |
| **Input validation** | Validate `data.json` schema on load; display friendly error if malformed. |
| **Data sensitivity** | The JSON file contains project names and status — no PII, no secrets. |

### Infrastructure: Local Only

| Concern | Approach |
|---------|----------|
| **Hosting** | Kestrel on localhost, launched via `dotnet run` |
| **Deployment** | `dotnet publish -c Release -o ./publish` → run the executable |
| **Port** | Default `5000` (HTTP only), configurable via `appsettings.json` |
| **Process lifetime** | Run when needed, close when done. No daemon, no service. |
| **Auto-start** | Optional: Windows shortcut or batch file in Startup folder |
| **Cost** | $0. No cloud, no hosting, no licenses. |

### Operational Simplicity

```bash
# Development
dotnet watch --project ReportingDashboard

# Production (screenshot capture)
dotnet run --project ReportingDashboard --configuration Release
# → Open http://localhost:5000 → Screenshot → Close
```

### Optional: Automated Screenshot Capture

If manual screenshots become tedious, add **Puppeteer-Sharp** (`4.x`) as a dev dependency to programmatically capture 1920×1080 PNGs:

```csharp
// Optional utility - not part of the main app
await new BrowserFetcher().DownloadAsync();
var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
var page = await browser.NewPageAsync();
await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });
await page.GoToAsync("http://localhost:5000");
await page.ScreenshotAsync("dashboard.png", new ScreenshotOptions { FullPage = false });
```

This is a "nice to have" — not a core requirement.

---

## 6. Risks & Trade-offs

### Low-Risk Items (Manageable)

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Blazor Server overhead for a static page** | Unnecessary SignalR connection for what's essentially a static render | Acceptable trade-off — it's the mandated stack, and the overhead is invisible for local single-user use |
| **Browser rendering differences** | Screenshot may look different across Edge/Chrome/Firefox | Standardize on one browser (Edge Chromium recommended on Windows) for all screenshots |
| **`data.json` schema drift** | User edits JSON incorrectly, page crashes | Add `try/catch` on deserialization with a friendly error page showing what's wrong |
| **SVG positioning math** | Timeline dates-to-pixels calculation could have off-by-one issues | Unit test the date-to-pixel calculation function with known inputs |

### Non-Risks (Explicitly Scoped Out)

| Concern | Why it's not a risk |
|---------|-------------------|
| **Scalability** | Single user, single machine. Not applicable. |
| **High availability** | Run on demand. No uptime requirement. |
| **Data integrity** | JSON file is the source of truth, edited manually. No concurrent writes. |
| **Performance** | One page, one data file, one user. Sub-100ms render guaranteed. |
| **Security breaches** | localhost-only, no network exposure, no sensitive data. |

### Trade-off: Blazor Server vs. Static HTML

The reference design is already a working static HTML file. Using Blazor Server adds:
- **Benefit:** Data-driven rendering from `data.json` (the core requirement), component reuse if more dashboards are needed, familiar C# tooling
- **Cost:** Requires `dotnet` runtime, SignalR connection overhead, slightly more complex than opening an HTML file
- **Verdict:** The benefit justifies the cost. Manually editing HTML for each reporting period is error-prone and slow. Blazor + JSON is the right call.

---

## 7. Open Questions

| # | Question | Recommended Default | Needs Input? |
|---|----------|-------------------|--------------|
| 1 | **How many months should the heatmap show?** The reference shows 4 (Jan–Apr). Should this be configurable? | Default to 4 most recent months, configurable in `data.json` | Low priority — default is fine |
| 2 | **How many timeline tracks?** The reference shows 3 (M1, M2, M3). Is this fixed or variable? | Variable, driven by `data.json` array length | No — make it data-driven |
| 3 | **Should the "NOW" line auto-calculate from system date or be set in `data.json`?** | Auto-calculate from `DateTime.Now` but allow override in JSON | No — auto-calculate |
| 4 | **Will multiple projects/dashboards be needed?** | Design for one, but the component structure supports adding a project selector later | Worth asking stakeholder |
| 5 | **Is there a preferred browser for screenshots?** | Edge Chromium at 100% zoom, 1920×1080 | Worth confirming |
| 6 | **Should `data.json` support comments or be strict JSON?** | Strict JSON (use `//` comments if switching to JSON5, but not recommended) | No — keep it simple |
| 7 | **What date range should the timeline cover?** Reference shows Jan–Jun (6 months). | Configurable in `data.json` with `startDate` and `endDate` fields | No — make it configurable |

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** Pixel-perfect match of the reference design, driven by `data.json`.

1. **Scaffold the project** — `dotnet new blazor --interactivity Server -n ReportingDashboard`
2. **Define `data.json` schema and C# models** — Start with the data model, create sample data matching the reference design's fictional project
3. **Build `DashboardDataService`** — Singleton service that reads and deserializes `data.json` at startup
4. **Implement `Header.razor`** — Title, subtitle, legend icons (diamond/circle/line)
5. **Implement `Timeline.razor`** — SVG generation with date-to-pixel positioning, track lines, milestone shapes
6. **Implement `HeatmapGrid.razor` + `HeatmapCell.razor`** — CSS Grid matching the reference layout exactly
7. **Style with `app.css`** — Port all styles from `OriginalDesignConcept.html`, using CSS custom properties for colors
8. **Test with browser** — Open at 1920×1080, compare side-by-side with reference screenshot

### Phase 2: Polish (Day 3)

1. **Error handling** — Friendly error display if `data.json` is missing or malformed
2. **File watching** — Auto-reload dashboard when `data.json` is saved (via `FileSystemWatcher` + `InvokeAsync(StateHasChanged)`)
3. **Print stylesheet** — `@media print` rules for clean PDF export if needed
4. **Sample data** — Create a compelling fictional project ("Project Phoenix — Cloud Migration Platform") with realistic milestones and status items

### Phase 3: Nice-to-Haves (Optional)

1. **Automated screenshots** — Puppeteer-Sharp utility to generate PNGs on demand
2. **Multiple projects** — Dropdown selector loading different JSON files from a `data/` directory
3. **Date auto-advance** — "NOW" marker and current-month highlighting derived from system clock
4. **Subtle animations** — CSS transitions on page load for a polished feel (fade-in cells)

### Quick Wins

| Quick Win | Effort | Impact |
|-----------|--------|--------|
| Port reference CSS directly into `app.css` | 30 min | Immediate visual fidelity |
| Create `data.json` with fictional but realistic sample data | 30 min | Demo-ready from day one |
| Use `dotnet watch` for hot reload during development | 0 min (built-in) | Fast iteration on CSS/layout |
| Add CSS custom properties for the color palette | 15 min | Easy theming, maintainable code |

### What NOT to Build

- ❌ No authentication or login page
- ❌ No database (SQLite, LiteDB, or otherwise)
- ❌ No REST API or Web API controllers
- ❌ No SignalR hubs (Blazor Server's built-in circuit is sufficient)
- ❌ No Docker containerization
- ❌ No CI/CD pipeline (local tool, manual deployment)
- ❌ No third-party CSS framework (Bootstrap, Tailwind, MudBlazor)
- ❌ No JavaScript charting library
- ❌ No responsive/mobile design (fixed 1920×1080 target)
- ❌ No unit test project (optional for this scope — add only if time permits)

### NuGet Packages Required

```xml
<!-- ReportingDashboard.csproj — the ONLY packages needed -->
<ItemGroup>
    <!-- None beyond the default Blazor Server template! -->
    <!-- System.Text.Json is included in the framework -->
    <!-- All rendering is native Razor + CSS + SVG -->
</ItemGroup>

<!-- Optional, only if automated screenshots are desired -->
<ItemGroup Condition="'$(Configuration)' == 'Debug'">
    <PackageReference Include="PuppeteerSharp" Version="4.0.0" />
</ItemGroup>
```

**Total external dependencies: Zero (for the core app).** This is intentional and correct for the scope.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/69d83848dd419051b7080124d605bf1da5c3e51e/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
