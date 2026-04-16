# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-16 22:21 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The primary use case is generating pixel-perfect screenshots for PowerPoint executive decks. **Primary Recommendation:** Build a minimal Blazor Server application with a single Razor component page, inline SVG for the timeline, CSS Grid/Flexbox for the heatmap, and `System.Text.Json` for deserializing `data.json`. No database, no authentication, no external services. The entire solution should be under 10 files and deployable with `dotnet run`. This is intentionally a "small tool done well" — resist the urge to over-engineer. --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.Components` | 8.0.x (SDK-included) | Blazor Server framework | ✅ Included in SDK | | `System.Text.Json` | 8.0.x (SDK-included) | JSON deserialization | ✅ Included in SDK | | `bUnit` | 1.25+ | Component testing | ❌ Optional | | `xUnit` | 2.7+ | Unit testing | ❌ Optional | **Total external NuGet dependencies: 0.** Everything needed ships with the .NET 8 SDK. This is by design — fewer dependencies means fewer breaking changes, fewer security patches, and a simpler project for a tool that should "just work."

### Key Findings

- The original HTML design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) and **Flexbox** for layout — both work natively in Blazor Server with standard CSS, no third-party CSS framework needed.
- The timeline section uses **inline SVG** with circles, diamonds (polygons), lines, and text — Blazor can render SVG directly in Razor markup with data-bound attributes, no charting library required.
- The color palette is fixed and semantic (green=shipped, blue=in-progress, amber=carryover, red=blockers) — this maps cleanly to a CSS class strategy matching the original design's `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` pattern.
- The data model is simple and flat: milestones (name, date, type, track), work items (name, status, month), and metadata (title, subtitle, current month) — a single `data.json` file with 3-4 arrays is sufficient.
- **No database is needed.** File-based JSON is the correct storage for this use case. Adding SQLite or any DB would be over-engineering.
- The design targets **1920×1080 fixed resolution** for screenshot capture — this simplifies responsive design concerns (there are none; design for exactly one viewport).
- Blazor Server's **SignalR connection** is irrelevant for this use case (no interactivity needed beyond page load), but it's the mandated stack and works fine for serving a static-data page.
- The `Segoe UI` font specified in the design is available on all Windows machines (the target environment), so no web font loading is needed.
- Third-party charting libraries (Radzen, MudBlazor Charts, Syncfusion) are **overkill** for this project — the SVG timeline and CSS heatmap are simpler to build by hand and give exact pixel control needed for screenshot fidelity. ---
- `dotnet new blazor --interactivity Server -n ReportingDashboard.Web`
- Create the `.sln` file and project structure.
- Port the original HTML/CSS into `Dashboard.razor` and `dashboard.css` as a **static page** (hardcoded data, no JSON loading).
- Verify visual fidelity against the original design at 1920×1080. **Exit criteria:** The page renders identically to the original HTML design in Edge at 1920×1080.
- Define `DashboardData` record types.
- Create `data.json` with example data matching the original design's content.
- Build `DashboardDataService` to load and cache the JSON.
- Replace hardcoded HTML with `@foreach` loops and data binding. **Exit criteria:** The page renders the same visual output, but all content comes from `data.json`.
- Build the `TimelineCalculator` helper (date → x-coordinate mapping).
- Replace hardcoded SVG coordinates with calculated positions.
- Render milestones, checkpoints, and track lines from data. **Exit criteria:** Adding/removing milestones in `data.json` correctly updates the timeline SVG.
- Create compelling fictional project data (e.g., "Project Atlas — Next-Gen Customer Platform").
- Refine spacing, typography, and colors for maximum executive readability.
- Test screenshot capture workflow (Edge → Snipping Tool → PowerPoint).
- Document the `data.json` schema for future editors. **Exit criteria:** A polished screenshot that looks like it belongs in a VP-level status deck.
- **Immediate:** The original HTML is already 90% of the CSS needed. Porting it is a copy-paste + minor adjustments job.
- **High-impact, low-effort:** The `data.json` approach means non-developers can update project status by editing a simple text file.
- **Visual polish:** Adding subtle `box-shadow` or `border-radius` refinements to the heatmap cells can elevate the design from "internal tool" to "executive-grade" with minimal CSS changes.
- ❌ No database (SQLite, LiteDB, or otherwise)
- ❌ No authentication or authorization
- ❌ No API endpoints (no controllers, no minimal API)
- ❌ No client-side JavaScript (Blazor handles everything)
- ❌ No third-party charting library
- ❌ No third-party CSS framework (Bootstrap, Tailwind, etc.)
- ❌ No Docker container
- ❌ No CI/CD pipeline
- ❌ No configuration UI (edit `data.json` directly)
- ❌ No export/print functionality (use browser/OS screenshot tools) ---

### Recommended Tools & Technologies

- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (.NET 8) | `net8.0` | Mandated stack. Single `.razor` page component. | | **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches original design exactly. No CSS framework needed. | | **Timeline Visualization** | Inline SVG in Razor | N/A | Render `<svg>` elements directly with `@foreach` loops over milestone data. No JS charting library. | | **Heatmap Grid** | CSS Grid | N/A | `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months from data. | | **Icons/Shapes** | SVG primitives | N/A | Diamonds via `<polygon>`, circles via `<circle>`, lines via `<line>`. Same approach as original HTML. | | **Component Library** | **None** | — | MudBlazor, Radzen, etc. are unnecessary overhead. Raw HTML/CSS gives better screenshot fidelity. | **Why no component library:** The design is a single static page with no forms, no dialogs, no data tables, no user input. A component library would add 2-5 MB of CSS/JS and fight you on pixel-exact styling. Hand-written CSS matching the original HTML design is faster to build and easier to control. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | No NuGet package needed. Use `JsonSerializer.Deserialize<T>()`. | | **File Access** | `System.IO.File.ReadAllTextAsync()` | Built into .NET 8 | Read `data.json` from the project's content root. | | **Data Models** | C# records | Built into .NET 8 | Use `record` types for immutable data models. | | **Data Service** | Singleton `DashboardDataService` | Custom | Registered in DI, reads and caches `data.json` on startup. | | Technology | Recommendation | |-----------|---------------| | **None** | ✅ Correct choice. A `data.json` file is the entire data store. No SQLite, no LiteDB, no EF Core. | | Layer | Technology | Notes | |-------|-----------|-------| | **Runtime** | .NET 8 SDK | `dotnet run` from command line. | | **Hosting** | Kestrel (built-in) | Default Blazor Server hosting. Runs on `https://localhost:5001`. | | **Screenshot Capture** | Browser print / Snipping Tool | Manual process per user's stated workflow. | | **IDE** | Visual Studio 2022 or VS Code + C# Dev Kit | Standard .NET development. | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Tests** | xUnit | 2.7+ | Test data deserialization and model mapping. | | **Component Tests** | bUnit | 1.25+ | Test Razor component rendering if needed. | | **Snapshot Testing** | Manual visual inspection | — | Given the screenshot-for-PPT use case, visual verification is primary QA. | **Honest assessment on testing:** For a project this small (one page, read-only, local-only), formal automated testing has minimal ROI. A single integration test verifying `data.json` loads correctly is sufficient. The real "test" is whether the screenshot looks right. --- This is not a microservices project. It's not even a multi-page app. The architecture should be:
```
data.json (file on disk)
    ↓ read on startup
DashboardDataService (singleton, DI-registered)
    ↓ injected into
Dashboard.razor (single page component)
    ↓ renders
HTML + CSS + inline SVG (pixel-perfect executive dashboard)
```
```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard.Web/
│       ├── ReportingDashboard.Web.csproj    (net8.0, Blazor Server)
│       ├── Program.cs                        (minimal hosting setup)
│       ├── Components/
│       │   ├── App.razor                     (root component)
│       │   ├── Routes.razor                  (router)
│       │   └── Pages/
│       │       └── Dashboard.razor           (THE page - all rendering here)
│       ├── Models/
│       │   └── DashboardData.cs              (record types for JSON shape)
│       ├── Services/
│       │   └── DashboardDataService.cs       (reads data.json, caches in memory)
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css             (all styles, ported from design HTML)
│       │   └── data.json                     (the data file)
│       └── Properties/
│           └── launchSettings.json
└── tests/  (optional)
    └── ReportingDashboard.Tests/
        └── DataServiceTests.cs
```
```csharp
// Models/DashboardData.cs

public record DashboardData
{
    public DashboardMetadata Metadata { get; init; }
    public List<Milestone> Milestones { get; init; } = [];
    public List<Track> Tracks { get; init; } = [];
    public List<WorkItem> Items { get; init; } = [];
}

public record DashboardMetadata
{
    public string Title { get; init; }           // "Privacy Automation Release Roadmap"
    public string Subtitle { get; init; }        // "Trusted Platform · Privacy Automation..."
    public string CurrentMonth { get; init; }    // "Apr 2026"
    public string BacklogUrl { get; init; }      // ADO link
    public List<string> Months { get; init; }    // ["Jan", "Feb", "Mar", "Apr"]
}

public record Track
{
    public string Id { get; init; }              // "m1", "m2", "m3"
    public string Name { get; init; }            // "Chatbot & MS Role"
    public string Color { get; init; }           // "#0078D4"
}

public record Milestone
{
    public string TrackId { get; init; }         // references Track.Id
    public string Label { get; init; }           // "Mar 26 PoC"
    public string Date { get; init; }            // "2026-03-26"
    public string Type { get; init; }            // "poc" | "production" | "checkpoint" | "start"
}

public record WorkItem
{
    public string Name { get; init; }            // "Chatbot v2 GA"
    public string Status { get; init; }          // "shipped" | "in-progress" | "carryover" | "blocked"
    public string Month { get; init; }           // "Jan" | "Feb" | "Mar" | "Apr"
}
```
- **Startup:** `Program.cs` registers `DashboardDataService` as a singleton.
- **First request:** Service reads `wwwroot/data.json` via `IWebHostEnvironment.WebRootFileProvider`, deserializes to `DashboardData`, caches in a private field.
- **Rendering:** `Dashboard.razor` injects `DashboardDataService`, calls `GetDataAsync()`, renders the timeline SVG and heatmap grid using `@foreach` over the data collections.
- **Updates:** Edit `data.json`, restart the app (or add a file watcher for hot reload during development). **Port the original HTML's `<style>` block directly into `dashboard.css`.** The design already has well-named classes (`.hm-grid`, `.ship-cell`, `.prog-cell`, etc.). Changes needed:
- Remove the fixed `width:1920px; height:1080px` from `body` — use Blazor's layout instead, but keep the content area at 1920px max-width for screenshot consistency.
- Keep CSS Grid for the heatmap: `grid-template-columns: 160px repeat(N, 1fr)` where N is dynamically set via inline style from the months count.
- Keep Flexbox for header and timeline area.
- All color variables should remain hardcoded (not CSS custom properties) — this matches the simplicity goal and the colors are semantic, not themeable. The original design renders the timeline as a hand-crafted `<svg>` with computed x-positions for each month. In Blazor, this becomes:
```razor
<svg width="@svgWidth" height="185" style="overflow:visible;display:block">
    @foreach (var month in Data.Metadata.Months)
    {
        var x = GetMonthX(month);
        <line x1="@x" y1="0" x2="@x" y2="185" stroke="#bbb" stroke-opacity="0.4" />
        <text x="@(x+5)" y="14" fill="#666" font-size="11">@month</text>
    }
    @foreach (var milestone in Data.Milestones)
    {
        @* Render diamond, circle, or line based on milestone.Type *@
    }
</svg>
``` The x-position calculation divides the SVG width by the number of months and places milestones proportionally by date. This is ~20 lines of C# helper logic. ---

### Considerations & Risks

- **None.** This is explicitly out of scope per the project requirements. The app runs locally, accessed only by the person running it. No login, no roles, no middleware. If this ever needs to be shared (e.g., on a team server), the simplest addition would be Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (one line in `Program.cs`), but this is not recommended for the MVP.
- `data.json` contains project status information, not PII or secrets. No encryption needed.
- The JSON file lives in `wwwroot/` which means it's publicly accessible via HTTP. For a local-only app this is fine. If deployed to a shared server, move it to a non-wwwroot path. | Aspect | Recommendation | |--------|---------------| | **Local Dev** | `dotnet run` or `dotnet watch` for hot reload during CSS/Razor changes. | | **"Production"** | There is no production. This runs on the developer's workstation. | | **Port** | Default Kestrel: `https://localhost:5001` or `http://localhost:5000`. | | **Container** | Not needed. `dotnet run` is the entire deployment. | **$0.** This is a local application. No cloud resources, no hosting fees, no licenses. --- **Severity: Low.** Blazor Server establishes a SignalR WebSocket connection for interactivity. This page has zero interactivity — it's a read-only dashboard. The SignalR overhead is wasted but negligible for a local app. The benefit is staying within the mandated stack. **Mitigation:** None needed. If this were a concern, Blazor Static SSR (new in .NET 8) could render the page without SignalR, but the default Blazor Server template is simpler to scaffold. **Severity: Medium.** The original HTML hardcodes x-positions for milestones (e.g., `x="745"` for "Mar 26"). Making this dynamic requires calculating pixel positions from dates within a date range. This is the most complex logic in the project. **Mitigation:** Create a simple `TimelineCalculator` helper that maps `DateTime` values to x-coordinates within the SVG viewport width. Unit test this helper. Formula: `x = (date - startDate) / (endDate - startDate) * svgWidth`. **Severity: Low.** Different browsers render fonts and SVG slightly differently. Since the user is taking screenshots for PowerPoint, they'll use one consistent browser. **Mitigation:** Recommend Microsoft Edge (Chromium) for consistent rendering of Segoe UI. Document the recommended browser and viewport size (1920×1080). **Severity: Low.** If someone edits `data.json` with invalid structure, the app will crash on startup with a deserialization error. **Mitigation:** Add a try/catch in `DashboardDataService` with a clear error message. Optionally validate required fields on load. Keep the JSON schema documented with a sample file. **Chosen: Hand-written SVG.** A charting library (e.g., Radzen Charts, ApexCharts.Blazor) would automate axis drawing and milestone placement but would fight you on exact pixel positioning, diamond shapes, and the specific visual style. For a screenshot-targeted dashboard, precise control over every element matters more than development speed. **Chosen: Raw HTML/CSS.** This trades developer convenience (prebuilt components) for exact visual control. For a single page with no forms or complex interactions, this is the correct trade-off. ---
- **How many months should the timeline span?** The original design shows Jan–Jun (6 months). Should this be configurable in `data.json`, or always show a fixed window? **Recommendation:** Make it data-driven — the months array in the JSON determines the columns.
- **Should the "Now" line position be automatic or manual?** The original HTML hardcodes the "NOW" marker position. Should the app calculate it from `DateTime.Now`, or should the user specify the current date in `data.json`? **Recommendation:** Auto-calculate from system date, but allow an override in `data.json` for generating historical snapshots.
- **How frequently will data.json be updated?** If it's weekly/monthly, manual editing is fine. If it needs to be updated from ADO or another source, a future enhancement could add a simple import script. **Recommendation:** Start with manual editing; don't build an import pipeline until the need is proven.
- **Should the page auto-refresh when data.json changes?** Useful during data entry sessions. **Recommendation:** Defer. `dotnet watch` already handles hot reload during development. For data-only changes, a manual browser refresh is acceptable.
- **What is the exact set of milestone types?** The original design shows: PoC Milestone (diamond, gold), Production Release (diamond, green), Checkpoint (circle, gray), and Start (circle, colored). Are these the complete set? **Recommendation:** Codify these four types and use the `Type` field in the milestone JSON to select the SVG shape and color. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The primary use case is generating pixel-perfect screenshots for PowerPoint executive decks.

**Primary Recommendation:** Build a minimal Blazor Server application with a single Razor component page, inline SVG for the timeline, CSS Grid/Flexbox for the heatmap, and `System.Text.Json` for deserializing `data.json`. No database, no authentication, no external services. The entire solution should be under 10 files and deployable with `dotnet run`. This is intentionally a "small tool done well" — resist the urge to over-engineer.

---

## 2. Key Findings

- The original HTML design uses **CSS Grid** (`grid-template-columns: 160px repeat(4,1fr)`) and **Flexbox** for layout — both work natively in Blazor Server with standard CSS, no third-party CSS framework needed.
- The timeline section uses **inline SVG** with circles, diamonds (polygons), lines, and text — Blazor can render SVG directly in Razor markup with data-bound attributes, no charting library required.
- The color palette is fixed and semantic (green=shipped, blue=in-progress, amber=carryover, red=blockers) — this maps cleanly to a CSS class strategy matching the original design's `.ship-cell`, `.prog-cell`, `.carry-cell`, `.block-cell` pattern.
- The data model is simple and flat: milestones (name, date, type, track), work items (name, status, month), and metadata (title, subtitle, current month) — a single `data.json` file with 3-4 arrays is sufficient.
- **No database is needed.** File-based JSON is the correct storage for this use case. Adding SQLite or any DB would be over-engineering.
- The design targets **1920×1080 fixed resolution** for screenshot capture — this simplifies responsive design concerns (there are none; design for exactly one viewport).
- Blazor Server's **SignalR connection** is irrelevant for this use case (no interactivity needed beyond page load), but it's the mandated stack and works fine for serving a static-data page.
- The `Segoe UI` font specified in the design is available on all Windows machines (the target environment), so no web font loading is needed.
- Third-party charting libraries (Radzen, MudBlazor Charts, Syncfusion) are **overkill** for this project — the SVG timeline and CSS heatmap are simpler to build by hand and give exact pixel control needed for screenshot fidelity.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Server UI)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (.NET 8) | `net8.0` | Mandated stack. Single `.razor` page component. |
| **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches original design exactly. No CSS framework needed. |
| **Timeline Visualization** | Inline SVG in Razor | N/A | Render `<svg>` elements directly with `@foreach` loops over milestone data. No JS charting library. |
| **Heatmap Grid** | CSS Grid | N/A | `grid-template-columns: 160px repeat(N, 1fr)` where N = number of months from data. |
| **Icons/Shapes** | SVG primitives | N/A | Diamonds via `<polygon>`, circles via `<circle>`, lines via `<line>`. Same approach as original HTML. |
| **Component Library** | **None** | — | MudBlazor, Radzen, etc. are unnecessary overhead. Raw HTML/CSS gives better screenshot fidelity. |

**Why no component library:** The design is a single static page with no forms, no dialogs, no data tables, no user input. A component library would add 2-5 MB of CSS/JS and fight you on pixel-exact styling. Hand-written CSS matching the original HTML design is faster to build and easier to control.

### Backend (Data Layer)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | No NuGet package needed. Use `JsonSerializer.Deserialize<T>()`. |
| **File Access** | `System.IO.File.ReadAllTextAsync()` | Built into .NET 8 | Read `data.json` from the project's content root. |
| **Data Models** | C# records | Built into .NET 8 | Use `record` types for immutable data models. |
| **Data Service** | Singleton `DashboardDataService` | Custom | Registered in DI, reads and caches `data.json` on startup. |

### Database

| Technology | Recommendation |
|-----------|---------------|
| **None** | ✅ Correct choice. A `data.json` file is the entire data store. No SQLite, no LiteDB, no EF Core. |

### Infrastructure

| Layer | Technology | Notes |
|-------|-----------|-------|
| **Runtime** | .NET 8 SDK | `dotnet run` from command line. |
| **Hosting** | Kestrel (built-in) | Default Blazor Server hosting. Runs on `https://localhost:5001`. |
| **Screenshot Capture** | Browser print / Snipping Tool | Manual process per user's stated workflow. |
| **IDE** | Visual Studio 2022 or VS Code + C# Dev Kit | Standard .NET development. |

### Testing

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Tests** | xUnit | 2.7+ | Test data deserialization and model mapping. |
| **Component Tests** | bUnit | 1.25+ | Test Razor component rendering if needed. |
| **Snapshot Testing** | Manual visual inspection | — | Given the screenshot-for-PPT use case, visual verification is primary QA. |

**Honest assessment on testing:** For a project this small (one page, read-only, local-only), formal automated testing has minimal ROI. A single integration test verifying `data.json` loads correctly is sufficient. The real "test" is whether the screenshot looks right.

---

## 4. Architecture Recommendations

### Overall Pattern: **Minimal Single-Page Application**

This is not a microservices project. It's not even a multi-page app. The architecture should be:

```
data.json (file on disk)
    ↓ read on startup
DashboardDataService (singleton, DI-registered)
    ↓ injected into
Dashboard.razor (single page component)
    ↓ renders
HTML + CSS + inline SVG (pixel-perfect executive dashboard)
```

### Solution Structure

```
ReportingDashboard/
├── ReportingDashboard.sln
├── src/
│   └── ReportingDashboard.Web/
│       ├── ReportingDashboard.Web.csproj    (net8.0, Blazor Server)
│       ├── Program.cs                        (minimal hosting setup)
│       ├── Components/
│       │   ├── App.razor                     (root component)
│       │   ├── Routes.razor                  (router)
│       │   └── Pages/
│       │       └── Dashboard.razor           (THE page - all rendering here)
│       ├── Models/
│       │   └── DashboardData.cs              (record types for JSON shape)
│       ├── Services/
│       │   └── DashboardDataService.cs       (reads data.json, caches in memory)
│       ├── wwwroot/
│       │   ├── css/
│       │   │   └── dashboard.css             (all styles, ported from design HTML)
│       │   └── data.json                     (the data file)
│       └── Properties/
│           └── launchSettings.json
└── tests/  (optional)
    └── ReportingDashboard.Tests/
        └── DataServiceTests.cs
```

### Data Model Design

```csharp
// Models/DashboardData.cs

public record DashboardData
{
    public DashboardMetadata Metadata { get; init; }
    public List<Milestone> Milestones { get; init; } = [];
    public List<Track> Tracks { get; init; } = [];
    public List<WorkItem> Items { get; init; } = [];
}

public record DashboardMetadata
{
    public string Title { get; init; }           // "Privacy Automation Release Roadmap"
    public string Subtitle { get; init; }        // "Trusted Platform · Privacy Automation..."
    public string CurrentMonth { get; init; }    // "Apr 2026"
    public string BacklogUrl { get; init; }      // ADO link
    public List<string> Months { get; init; }    // ["Jan", "Feb", "Mar", "Apr"]
}

public record Track
{
    public string Id { get; init; }              // "m1", "m2", "m3"
    public string Name { get; init; }            // "Chatbot & MS Role"
    public string Color { get; init; }           // "#0078D4"
}

public record Milestone
{
    public string TrackId { get; init; }         // references Track.Id
    public string Label { get; init; }           // "Mar 26 PoC"
    public string Date { get; init; }            // "2026-03-26"
    public string Type { get; init; }            // "poc" | "production" | "checkpoint" | "start"
}

public record WorkItem
{
    public string Name { get; init; }            // "Chatbot v2 GA"
    public string Status { get; init; }          // "shipped" | "in-progress" | "carryover" | "blocked"
    public string Month { get; init; }           // "Jan" | "Feb" | "Mar" | "Apr"
}
```

### Data Flow

1. **Startup:** `Program.cs` registers `DashboardDataService` as a singleton.
2. **First request:** Service reads `wwwroot/data.json` via `IWebHostEnvironment.WebRootFileProvider`, deserializes to `DashboardData`, caches in a private field.
3. **Rendering:** `Dashboard.razor` injects `DashboardDataService`, calls `GetDataAsync()`, renders the timeline SVG and heatmap grid using `@foreach` over the data collections.
4. **Updates:** Edit `data.json`, restart the app (or add a file watcher for hot reload during development).

### CSS Architecture

**Port the original HTML's `<style>` block directly into `dashboard.css`.** The design already has well-named classes (`.hm-grid`, `.ship-cell`, `.prog-cell`, etc.). Changes needed:

- Remove the fixed `width:1920px; height:1080px` from `body` — use Blazor's layout instead, but keep the content area at 1920px max-width for screenshot consistency.
- Keep CSS Grid for the heatmap: `grid-template-columns: 160px repeat(N, 1fr)` where N is dynamically set via inline style from the months count.
- Keep Flexbox for header and timeline area.
- All color variables should remain hardcoded (not CSS custom properties) — this matches the simplicity goal and the colors are semantic, not themeable.

### SVG Timeline Rendering Strategy

The original design renders the timeline as a hand-crafted `<svg>` with computed x-positions for each month. In Blazor, this becomes:

```razor
<svg width="@svgWidth" height="185" style="overflow:visible;display:block">
    @foreach (var month in Data.Metadata.Months)
    {
        var x = GetMonthX(month);
        <line x1="@x" y1="0" x2="@x" y2="185" stroke="#bbb" stroke-opacity="0.4" />
        <text x="@(x+5)" y="14" fill="#666" font-size="11">@month</text>
    }
    @foreach (var milestone in Data.Milestones)
    {
        @* Render diamond, circle, or line based on milestone.Type *@
    }
</svg>
```

The x-position calculation divides the SVG width by the number of months and places milestones proportionally by date. This is ~20 lines of C# helper logic.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope per the project requirements. The app runs locally, accessed only by the person running it. No login, no roles, no middleware.

If this ever needs to be shared (e.g., on a team server), the simplest addition would be Windows Authentication via `Microsoft.AspNetCore.Authentication.Negotiate` (one line in `Program.cs`), but this is not recommended for the MVP.

### Data Protection

- `data.json` contains project status information, not PII or secrets. No encryption needed.
- The JSON file lives in `wwwroot/` which means it's publicly accessible via HTTP. For a local-only app this is fine. If deployed to a shared server, move it to a non-wwwroot path.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Local Dev** | `dotnet run` or `dotnet watch` for hot reload during CSS/Razor changes. |
| **"Production"** | There is no production. This runs on the developer's workstation. |
| **Port** | Default Kestrel: `https://localhost:5001` or `http://localhost:5000`. |
| **Container** | Not needed. `dotnet run` is the entire deployment. |

### Infrastructure Costs

**$0.** This is a local application. No cloud resources, no hosting fees, no licenses.

---

## 6. Risks & Trade-offs

### Risk: Blazor Server Is Heavyweight for This Use Case

**Severity: Low.** Blazor Server establishes a SignalR WebSocket connection for interactivity. This page has zero interactivity — it's a read-only dashboard. The SignalR overhead is wasted but negligible for a local app. The benefit is staying within the mandated stack.

**Mitigation:** None needed. If this were a concern, Blazor Static SSR (new in .NET 8) could render the page without SignalR, but the default Blazor Server template is simpler to scaffold.

### Risk: SVG Timeline Positioning Math

**Severity: Medium.** The original HTML hardcodes x-positions for milestones (e.g., `x="745"` for "Mar 26"). Making this dynamic requires calculating pixel positions from dates within a date range. This is the most complex logic in the project.

**Mitigation:** Create a simple `TimelineCalculator` helper that maps `DateTime` values to x-coordinates within the SVG viewport width. Unit test this helper. Formula: `x = (date - startDate) / (endDate - startDate) * svgWidth`.

### Risk: Screenshot Fidelity Across Browsers

**Severity: Low.** Different browsers render fonts and SVG slightly differently. Since the user is taking screenshots for PowerPoint, they'll use one consistent browser.

**Mitigation:** Recommend Microsoft Edge (Chromium) for consistent rendering of Segoe UI. Document the recommended browser and viewport size (1920×1080).

### Risk: data.json Schema Changes Break the UI

**Severity: Low.** If someone edits `data.json` with invalid structure, the app will crash on startup with a deserialization error.

**Mitigation:** Add a try/catch in `DashboardDataService` with a clear error message. Optionally validate required fields on load. Keep the JSON schema documented with a sample file.

### Trade-off: Hand-Written SVG vs. Charting Library

**Chosen: Hand-written SVG.** A charting library (e.g., Radzen Charts, ApexCharts.Blazor) would automate axis drawing and milestone placement but would fight you on exact pixel positioning, diamond shapes, and the specific visual style. For a screenshot-targeted dashboard, precise control over every element matters more than development speed.

### Trade-off: No Component Library

**Chosen: Raw HTML/CSS.** This trades developer convenience (prebuilt components) for exact visual control. For a single page with no forms or complex interactions, this is the correct trade-off.

---

## 7. Open Questions

1. **How many months should the timeline span?** The original design shows Jan–Jun (6 months). Should this be configurable in `data.json`, or always show a fixed window? **Recommendation:** Make it data-driven — the months array in the JSON determines the columns.

2. **Should the "Now" line position be automatic or manual?** The original HTML hardcodes the "NOW" marker position. Should the app calculate it from `DateTime.Now`, or should the user specify the current date in `data.json`? **Recommendation:** Auto-calculate from system date, but allow an override in `data.json` for generating historical snapshots.

3. **How frequently will data.json be updated?** If it's weekly/monthly, manual editing is fine. If it needs to be updated from ADO or another source, a future enhancement could add a simple import script. **Recommendation:** Start with manual editing; don't build an import pipeline until the need is proven.

4. **Should the page auto-refresh when data.json changes?** Useful during data entry sessions. **Recommendation:** Defer. `dotnet watch` already handles hot reload during development. For data-only changes, a manual browser refresh is acceptable.

5. **What is the exact set of milestone types?** The original design shows: PoC Milestone (diamond, gold), Production Release (diamond, green), Checkpoint (circle, gray), and Start (circle, colored). Are these the complete set? **Recommendation:** Codify these four types and use the `Type` field in the milestone JSON to select the SVG shape and color.

---

## 8. Implementation Recommendations

### Phase 1: Skeleton + Static Rendering (Day 1)

1. `dotnet new blazor --interactivity Server -n ReportingDashboard.Web`
2. Create the `.sln` file and project structure.
3. Port the original HTML/CSS into `Dashboard.razor` and `dashboard.css` as a **static page** (hardcoded data, no JSON loading).
4. Verify visual fidelity against the original design at 1920×1080.

**Exit criteria:** The page renders identically to the original HTML design in Edge at 1920×1080.

### Phase 2: Data Model + JSON Loading (Day 1-2)

1. Define `DashboardData` record types.
2. Create `data.json` with example data matching the original design's content.
3. Build `DashboardDataService` to load and cache the JSON.
4. Replace hardcoded HTML with `@foreach` loops and data binding.

**Exit criteria:** The page renders the same visual output, but all content comes from `data.json`.

### Phase 3: Dynamic Timeline SVG (Day 2)

1. Build the `TimelineCalculator` helper (date → x-coordinate mapping).
2. Replace hardcoded SVG coordinates with calculated positions.
3. Render milestones, checkpoints, and track lines from data.

**Exit criteria:** Adding/removing milestones in `data.json` correctly updates the timeline SVG.

### Phase 4: Fictional Project Data + Polish (Day 2-3)

1. Create compelling fictional project data (e.g., "Project Atlas — Next-Gen Customer Platform").
2. Refine spacing, typography, and colors for maximum executive readability.
3. Test screenshot capture workflow (Edge → Snipping Tool → PowerPoint).
4. Document the `data.json` schema for future editors.

**Exit criteria:** A polished screenshot that looks like it belongs in a VP-level status deck.

### Quick Wins

- **Immediate:** The original HTML is already 90% of the CSS needed. Porting it is a copy-paste + minor adjustments job.
- **High-impact, low-effort:** The `data.json` approach means non-developers can update project status by editing a simple text file.
- **Visual polish:** Adding subtle `box-shadow` or `border-radius` refinements to the heatmap cells can elevate the design from "internal tool" to "executive-grade" with minimal CSS changes.

### What NOT to Build

- ❌ No database (SQLite, LiteDB, or otherwise)
- ❌ No authentication or authorization
- ❌ No API endpoints (no controllers, no minimal API)
- ❌ No client-side JavaScript (Blazor handles everything)
- ❌ No third-party charting library
- ❌ No third-party CSS framework (Bootstrap, Tailwind, etc.)
- ❌ No Docker container
- ❌ No CI/CD pipeline
- ❌ No configuration UI (edit `data.json` directly)
- ❌ No export/print functionality (use browser/OS screenshot tools)

---

## Appendix: NuGet Package Summary

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.Components` | 8.0.x (SDK-included) | Blazor Server framework | ✅ Included in SDK |
| `System.Text.Json` | 8.0.x (SDK-included) | JSON deserialization | ✅ Included in SDK |
| `bUnit` | 1.25+ | Component testing | ❌ Optional |
| `xUnit` | 2.7+ | Unit testing | ❌ Optional |

**Total external NuGet dependencies: 0.** Everything needed ships with the .NET 8 SDK. This is by design — fewer dependencies means fewer breaking changes, fewer security patches, and a simpler project for a tool that should "just work."

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/532d79001b08ac0ad3dc103856e88b294506e1cb/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
