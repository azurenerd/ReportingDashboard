# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-17 11:35 UTC_

### Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The primary use case is generating pixel-perfect screenshots for PowerPoint decks to executives. **Primary recommendation:** Build a minimal Blazor Server app with a single Razor component page, inline SVG for the timeline, CSS Grid/Flexbox for the heatmap, and `System.Text.Json` for data loading. No database, no authentication, no external services. The entire solution should be under 10 files and deployable with `dotnet run`. Given the simplicity requirements—no auth, no persistence beyond a JSON file, single-page, screenshot-oriented—the architecture should be deliberately minimal. Blazor Server is appropriate because it provides hot-reload during development, C# model binding for the JSON data, and Razor component composition, while running entirely on `localhost`. ---

### Key Findings

- **The design is fundamentally a static data visualization.** The original HTML template uses inline SVG for the timeline and CSS Grid for the heatmap. Blazor Server can replicate this 1:1 using Razor markup with no JavaScript required.
- **No charting library is needed.** The timeline uses simple SVG primitives (lines, circles, polygons, text). Hand-written SVG in Razor templates gives full control over positioning and is far simpler than integrating a charting library for this use case.
- **CSS Grid is the correct layout strategy for the heatmap.** The original design uses `grid-template-columns: 160px repeat(4,1fr)` with 4 data rows. This maps directly to Blazor Razor markup with `@foreach` loops over the data model.
- **`data.json` as the sole data source eliminates all persistence complexity.** Use `System.Text.Json` (built into .NET 8) to deserialize on startup. No database, no EF Core, no migrations.
- **Screenshot fidelity is the primary quality metric.** The dashboard must render at 1920×1080 with exact colors, fonts, and spacing matching the design. This means pixel-level CSS attention and testing in a browser at that resolution.
- **Blazor Server's SignalR circuit is irrelevant here.** Since this is a read-only, single-user, local-only page, the persistent connection adds no value but also no harm. It just works out of the box.
- **Hot reload is a major development productivity win.** .NET 8 Blazor Server supports `dotnet watch` with CSS and Razor hot reload, enabling rapid iteration on the visual design.
- **No JavaScript interop is required.** The entire design can be implemented in pure Razor + CSS + inline SVG. --- The timeline is the most complex visual element. Key implementation details:
- **Date-to-X mapping:** Calculate pixel position as `(date - startDate) / (endDate - startDate) * svgWidth`. The original uses a 6-month range (Jan–Jun) across 1560px.
- **Track rendering:** Each track is a horizontal line at a fixed Y offset with milestones plotted along it.
- **Milestone shapes:**
- Checkpoint: `<circle>` with white fill and colored stroke
- PoC: `<polygon>` diamond shape (rotated square) in amber (#F4B400)
- Production: `<polygon>` diamond in green (#34A853)
- **"NOW" line:** Dashed vertical line at the current date's X position, colored #EA4335.
- **Drop shadow filter:** `<filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter>` applied to diamond milestones. **Implement this as a Razor component** that takes the track data and computes SVG elements in C#, rendering them in the markup. No JavaScript or charting library needed. --- **Goal:** Pixel-perfect reproduction of the original HTML design, powered by `data.json`.
- **Scaffold the project** — `dotnet new blazorserver -n ReportingDashboard -f net8.0`, create `.sln`.
- **Define data models** — C# records matching the JSON schema above.
- **Create `data.json`** — Fictional project data mimicking the original design's structure (3 timeline tracks, 4 heatmap rows × 4 months).
- **Build `DashboardDataService`** — Read and deserialize `data.json`, register as Singleton.
- **Implement `Dashboard.razor`** — Single page with three sections:
- Header (title, subtitle, legend)
- Timeline (SVG)
- Heatmap (CSS Grid)
- **Write `dashboard.css`** — Port the original CSS verbatim, using CSS custom properties for colors.
- **Verify at 1920×1080** — Open in Chrome, set viewport, compare visually to the original design.
- **Responsive date calculation** — Timeline SVG positions computed from actual dates in the data.
- **Dynamic heatmap columns** — Support variable number of months.
- **Current month auto-detection** — Highlight the correct column based on system date.
- **Error handling** — Friendly error page if `data.json` is missing or malformed.
- **Hot reload verification** — Ensure `dotnet watch` picks up CSS and Razor changes.
- **Playwright screenshot automation** — Script that launches the app, waits for render, takes a 1920×1080 screenshot, saves as PNG. Eliminates manual browser setup.
- **Print CSS** — `@media print` rules for direct browser printing.
- **Multiple data files** — Query parameter or CLI argument to switch between `data.json` files.
- **Dark mode** — Swap CSS custom properties for a dark palette (useful for some presentation contexts).
- **Tooltip hover details** — Blazor `@onmouseover` to show additional milestone details (light interactivity, still screenshot-friendly).
- **`dotnet watch run`** gives instant feedback during CSS/layout iteration.
- **CSS custom properties** make color theming a 5-minute task.
- **C# records with `required`** catch `data.json` schema errors at startup, not at render time.
- **Putting `data.json` in `wwwroot/data/`** makes it editable without rebuilding, and hot reload picks up changes.
- ❌ Authentication / authorization
- ❌ Database (SQLite, SQL Server, or otherwise)
- ❌ REST API endpoints
- ❌ Admin panel for data entry
- ❌ Multi-user support
- ❌ Docker containerization
- ❌ CI/CD pipeline
- ❌ Logging infrastructure (beyond `Console.WriteLine` for debugging)
- ❌ Unit tests for MVP (add in Phase 3 if the project persists) ---
```json
{
  "schemaVersion": 1,
  "header": {
    "title": "Project Phoenix Release Roadmap",
    "subtitle": "Engineering Platform • Core Infrastructure Workstream • April 2026",
    "backlogLink": "https://dev.azure.com/org/project/_backlogs",
    "currentDate": "2026-04-17"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "m1",
        "label": "M1",
        "description": "API Gateway & Auth",
        "color": "#0078D4",
        "milestones": [
          { "label": "Kickoff", "date": "2026-01-12", "type": "checkpoint" },
          { "label": "Mar 26 PoC", "date": "2026-03-26", "type": "poc" },
          { "label": "May Prod", "date": "2026-05-01", "type": "production" }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["January", "February", "March", "April"],
    "highlightColumn": "April",
    "rows": [
      {
        "category": "Shipped",
        "categoryStyle": "ship",
        "cells": [
          ["API scaffolding", "CI pipeline"],
          ["Auth module v1", "DB schema"],
          ["Load testing", "Monitoring setup"],
          ["Gateway PoC", "SDK v1 release"]
        ]
      },
      {
        "category": "In Progress",
        "categoryStyle": "prog",
        "cells": [
          ["Auth design"],
          ["Gateway prototype"],
          ["SDK integration"],
          ["Perf optimization", "Docs site"]
        ]
      },
      {
        "category": "Carryover",
        "categoryStyle": "carry",
        "cells": [
          [],
          ["Config service"],
          ["Config service", "Rate limiting"],
          ["Rate limiting"]
        ]
      },
      {
        "category": "Blockers",
        "categoryStyle": "block",
        "cells": [
          [],
          ["Vendor API delay"],
          [],
          ["Capacity approval pending"]
        ]
      }
    ]
  }
}
``` | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.App` | 8.0.x | Blazor Server runtime | ✅ Built-in | | `System.Text.Json` | 8.0.x | JSON deserialization | ✅ Built-in | | `Microsoft.Playwright` | 1.41+ | Screenshot automation | ❌ Optional | | `bunit` | 1.25+ | Component testing | ❌ Optional | | `xunit` | 2.7+ | Unit testing | ❌ Optional | **Total required external NuGet packages: 0.** Everything needed ships with the .NET 8 SDK.

### Recommended Tools & Technologies

- | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **UI Framework** | Blazor Server (built-in) | .NET 8.0 | Ships with `Microsoft.AspNetCore.App` | | **CSS Layout** | Hand-written CSS (Grid + Flexbox) | CSS3 | Matches original design exactly; no CSS framework needed | | **Timeline Visualization** | Inline SVG in Razor | SVG 1.1 | Simple primitives: `<line>`, `<circle>`, `<polygon>`, `<text>` | | **Heatmap Grid** | CSS Grid | CSS3 | `grid-template-columns: 160px repeat(N,1fr)` | | **Font** | Segoe UI (system font) | — | Already on Windows; no web font loading needed | | **Icons/Symbols** | Inline SVG shapes | — | Diamond (rotated square), circles per the design | **Why no CSS framework (Bootstrap, Tailwind)?** The design is a fixed 1920×1080 layout optimized for screenshots. Adding Bootstrap would fight the fixed layout and introduce unnecessary classes. The original HTML achieves the design in ~100 lines of CSS. Keep it minimal. **Why no charting library (Chart.js, Plotly, Radzen Charts)?** The timeline is ~15 SVG elements. A charting library would add 200KB+ of JS, a learning curve, and less control over exact positioning. Inline SVG in Razor is simpler, faster, and gives pixel-perfect control. | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Native, high-performance, zero dependencies | | **Data Loading** | `IConfiguration` or direct file read | .NET 8 | Load `data.json` at startup via `JsonSerializer.Deserialize<T>()` | | **Data Models** | C# records | C# 12 | Immutable, clean, perfect for read-only display data | | **Dependency Injection** | Built-in DI | .NET 8 | Register a `DashboardDataService` as Singleton | **Why not `Newtonsoft.Json`?** `System.Text.Json` is the default in .NET 8, faster, allocates less, and handles this simple schema without issue. No reason to add a dependency. | Component | Recommendation | Notes | |-----------|---------------|-------| | **Primary Store** | `data.json` file | Flat file in project root or `wwwroot/data/` | | **Format** | JSON | Human-editable, version-controllable | | **Schema** | Strongly-typed C# records | Compile-time safety for data structure | **No database is needed.** The data is read-only, small (likely <50KB), and edited by hand or by a separate process. A JSON file is the simplest, most transparent approach. | Component | Recommendation | Notes | |-----------|---------------|-------| | **Solution Structure** | Single `.sln` with one `.csproj` | `ReportingDashboard.sln` → `src/ReportingDashboard/ReportingDashboard.csproj` | | **Project Template** | `dotnet new blazorserver` | .NET 8 Blazor Server template | | **Build Tool** | `dotnet` CLI | `dotnet build`, `dotnet run`, `dotnet watch` | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Unit Testing** | xUnit | 2.7+ | For data model and service tests | | **Blazor Component Testing** | bUnit | 1.25+ | For Razor component rendering tests | | **Visual Regression** | Playwright for .NET | 1.41+ | Screenshot comparison at 1920×1080 (optional but valuable) | **Testing is optional for MVP** given the simplicity, but if added, Playwright screenshot tests would directly validate the primary use case (screenshot fidelity). | Tool | Purpose | Notes | |------|---------|-------| | `dotnet watch` | Hot reload during development | CSS + Razor changes reflected instantly | | Browser DevTools | CSS debugging, 1920×1080 viewport testing | Use Chrome's device toolbar to set exact resolution | | VS Code / Visual Studio | IDE | Either works; VS has better Blazor tooling | --- This is deliberately not a "real" web application. It's a **data visualization page** that reads a JSON file and renders HTML/SVG. The architecture should reflect this simplicity.
```
data.json ──► DashboardDataService ──► DashboardPage.razor ──► Browser (1920×1080)
                (Singleton)              (Single Razor Component)
```
```
ReportingDashboard.sln
└── src/ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs                    # Minimal startup, register services
    ├── Models/
    │   └── DashboardData.cs          # C# records for JSON schema
    ├── Services/
    │   └── DashboardDataService.cs   # Loads and caches data.json
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor       # Main (and only) page
    │   ├── Layout/
    │   │   └── MainLayout.razor      # Minimal layout wrapper
    │   ├── TimelineSection.razor     # SVG timeline component
    │   └── HeatmapSection.razor      # CSS Grid heatmap component
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css         # All styles in one file
    │   └── data/
    │       └── data.json             # Dashboard data
    └── Properties/
        └── launchSettings.json
``` Based on the original HTML design, the `data.json` schema should capture:
```csharp
// Models/DashboardData.cs
public record DashboardData
{
    public required HeaderInfo Header { get; init; }
    public required List<Milestone> Milestones { get; init; }
    public required List<TimelineTrack> TimelineTracks { get; init; }
    public required HeatmapData Heatmap { get; init; }
}

public record HeaderInfo
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string BacklogLink { get; init; }
    public required DateOnly CurrentDate { get; init; }
}

public record Milestone
{
    public required string Label { get; init; }
    public required DateOnly Date { get; init; }
    public required string Type { get; init; }  // "poc", "production", "checkpoint"
}

public record TimelineTrack
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Description { get; init; }
    public required string Color { get; init; }
    public required List<Milestone> Milestones { get; init; }
}

public record HeatmapData
{
    public required List<string> Columns { get; init; }  // Month names
    public required string HighlightColumn { get; init; } // Current month
    public required List<HeatmapRow> Rows { get; init; }
}

public record HeatmapRow
{
    public required string Category { get; init; }  // "Shipped", "In Progress", etc.
    public required string CategoryStyle { get; init; } // "ship", "prog", "carry", "block"
    public required List<List<string>> Cells { get; init; } // Items per column
}
``` **Dashboard.razor** (the page) should compose three sub-components:
- **HeaderSection** — Title, subtitle, legend. Pure HTML/CSS, bound to `HeaderInfo`.
- **TimelineSection** — SVG rendering. Takes `List<TimelineTrack>`, `DateOnly currentDate`, and a date range. Calculates X positions based on date math. Renders SVG lines, circles, diamonds, and text.
- **HeatmapSection** — CSS Grid rendering. Takes `HeatmapData`. Renders column headers, row headers, and cells with colored bullet items.
- `Program.cs` registers `DashboardDataService` as a Singleton.
- `DashboardDataService` reads `wwwroot/data/data.json` on first access, deserializes to `DashboardData`, caches in memory.
- `Dashboard.razor` injects the service, calls `GetData()` in `OnInitializedAsync`.
- Sub-components receive data as `[Parameter]` properties and render.
- No interactivity, no form submissions, no state changes. The original HTML uses a flat, specific CSS structure. Replicate this in `dashboard.css`:
- Use the **exact same color values** from the original design (see color palette below).
- Use **CSS Grid** for the heatmap with the same column template.
- Use **Flexbox** for header layout and timeline sidebar.
- Set `body` to `width: 1920px; height: 1080px; overflow: hidden;` for screenshot consistency.
- Use **CSS custom properties** (variables) for the color palette to enable easy theming.
```css
:root {
    /* Base */
    --bg-white: #FFFFFF;
    --text-primary: #111;
    --text-secondary: #888;
    --text-dark: #333;
    --link-blue: #0078D4;
    --border-light: #E0E0E0;
    --border-heavy: #CCC;
    --bg-subtle: #FAFAFA;
    --bg-header: #F5F5F5;
    --text-muted: #999;

    /* Shipped (Green) */
    --ship-text: #1B7A28;
    --ship-header-bg: #E8F5E9;
    --ship-cell-bg: #F0FBF0;
    --ship-cell-highlight: #D8F2DA;
    --ship-dot: #34A853;

    /* In Progress (Blue) */
    --prog-text: #1565C0;
    --prog-header-bg: #E3F2FD;
    --prog-cell-bg: #EEF4FE;
    --prog-cell-highlight: #DAE8FB;
    --prog-dot: #0078D4;

    /* Carryover (Amber) */
    --carry-text: #B45309;
    --carry-header-bg: #FFF8E1;
    --carry-cell-bg: #FFFDE7;
    --carry-cell-highlight: #FFF0B0;
    --carry-dot: #F4B400;

    /* Blockers (Red) */
    --block-text: #991B1B;
    --block-header-bg: #FEF2F2;
    --block-cell-bg: #FFF5F5;
    --block-cell-highlight: #FFE4E4;
    --block-dot: #EA4335;

    /* Timeline */
    --now-line: #EA4335;
    --milestone-poc: #F4B400;
    --milestone-prod: #34A853;
    --checkpoint: #999;
}
```

### Considerations & Risks

- **None.** This is explicitly out of scope. The dashboard runs on `localhost` for a single user generating screenshots. No login, no roles, no tokens. If future requirements demand sharing, the simplest addition would be `Microsoft.AspNetCore.Authentication.Negotiate` for Windows Integrated Auth (zero-config on corporate networks), but do not build this now.
- **No sensitive data.** The dashboard displays project status information (milestone names, dates, work item titles). This is not PII or confidential financial data.
- **`data.json` is a local file.** No encryption needed. It lives in the project directory and is version-controllable.
- If sensitivity increases later, the simplest protection is Windows NTFS permissions on the directory. | Aspect | Recommendation | |--------|---------------| | **Runtime** | Kestrel (built-in, default) | | **Port** | `https://localhost:5001` or `http://localhost:5000` | | **Deployment** | `dotnet run` from source, or `dotnet publish -c Release` for a self-contained binary | | **No reverse proxy needed** | Single user, local only | | **No Docker needed** | Adds complexity without benefit for local-only use | | **No IIS needed** | Kestrel is sufficient | **$0.** Everything runs locally on the developer's machine. No cloud services, no subscriptions, no licenses beyond what's already available with a .NET 8 SDK installation. --- **Impact:** Low | **Probability:** N/A (it's a known trade-off) A static HTML file would technically suffice. However, Blazor Server provides: (a) C# model binding for `data.json`, (b) component composition for maintainability, (c) hot reload for rapid iteration, and (d) a path to future interactivity if needed. The overhead is a ~30MB runtime and a SignalR connection that's unused. This is acceptable for a local-only tool. **Mitigation:** None needed. The benefits outweigh the overhead. **Impact:** Medium | **Probability:** Medium The design uses Segoe UI (Windows-only font) and fixed 1920×1080 dimensions. Screenshots taken on different machines or browsers may vary slightly.
- Document the exact browser and viewport settings for screenshots (e.g., "Chrome, F12 → Device Toolbar → 1920×1080, 100% zoom").
- Optionally add Playwright screenshot automation for consistent output.
- Segoe UI is available on all Windows machines in a corporate environment. **Impact:** Low | **Probability:** Medium As the dashboard evolves, the JSON schema may change, breaking existing `data.json` files.
- Use C# records with `required` properties for compile-time validation.
- Add a `"schemaVersion"` field to `data.json`.
- Validate on load with clear error messages. **Impact:** Low | **Probability:** Low Date-to-pixel calculations could produce unexpected results for milestones outside the visible date range, or for very short/long time spans.
- Clamp milestone X positions to the SVG viewport bounds.
- Make the date range configurable in `data.json` (startDate, endDate).
- Test with edge cases: milestones on range boundaries, single-day range, multi-year range. The dashboard reads `data.json` on page load (or app start). Changes to the file require a page refresh. This is acceptable for the screenshot use case. If real-time updates are later desired, `FileSystemWatcher` + Blazor `StateHasChanged()` is a simple addition. Users edit `data.json` by hand or with a text editor. This is intentional simplicity. If a data entry form is later needed, it's a straightforward Blazor form addition. ---
- **Date range for the timeline:** Should it always show 6 months? Should it auto-calculate from the earliest to latest milestone? Or should it be configurable in `data.json`?
- **Recommendation:** Make it configurable with a sensible default (current month ± 3 months).
- **Number of heatmap columns:** The original shows 4 months (Jan–Apr). Should this be dynamic based on the data, or fixed?
- **Recommendation:** Dynamic based on `data.json` columns array, with a practical maximum of 6–8 for readability at 1920px.
- **"Current month" highlighting:** The original highlights April cells with a different background. Should this be automatic (based on system date) or explicit in `data.json`?
- **Recommendation:** Configurable in `data.json` via a `highlightColumn` field, defaulting to current month if omitted.
- **Multiple projects:** Will the user ever need to switch between different `data.json` files for different projects?
- **Recommendation:** Support a `--data` command-line argument or query parameter (`?data=project-a.json`) for easy switching.
- **Print/export:** Beyond screenshots, is there a need for PDF export or print-optimized CSS?
- **Recommendation:** Add `@media print` CSS rules as a quick win. PDF export can be deferred. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** built with **C# .NET 8 and Blazor Server**, running entirely locally with no cloud dependencies. The dashboard visualizes project milestones on a timeline, displays a heatmap grid of work items by status (Shipped, In Progress, Carryover, Blockers), and reads all data from a local `data.json` file. The primary use case is generating pixel-perfect screenshots for PowerPoint decks to executives.

**Primary recommendation:** Build a minimal Blazor Server app with a single Razor component page, inline SVG for the timeline, CSS Grid/Flexbox for the heatmap, and `System.Text.Json` for data loading. No database, no authentication, no external services. The entire solution should be under 10 files and deployable with `dotnet run`.

Given the simplicity requirements—no auth, no persistence beyond a JSON file, single-page, screenshot-oriented—the architecture should be deliberately minimal. Blazor Server is appropriate because it provides hot-reload during development, C# model binding for the JSON data, and Razor component composition, while running entirely on `localhost`.

---

## 2. Key Findings

- **The design is fundamentally a static data visualization.** The original HTML template uses inline SVG for the timeline and CSS Grid for the heatmap. Blazor Server can replicate this 1:1 using Razor markup with no JavaScript required.
- **No charting library is needed.** The timeline uses simple SVG primitives (lines, circles, polygons, text). Hand-written SVG in Razor templates gives full control over positioning and is far simpler than integrating a charting library for this use case.
- **CSS Grid is the correct layout strategy for the heatmap.** The original design uses `grid-template-columns: 160px repeat(4,1fr)` with 4 data rows. This maps directly to Blazor Razor markup with `@foreach` loops over the data model.
- **`data.json` as the sole data source eliminates all persistence complexity.** Use `System.Text.Json` (built into .NET 8) to deserialize on startup. No database, no EF Core, no migrations.
- **Screenshot fidelity is the primary quality metric.** The dashboard must render at 1920×1080 with exact colors, fonts, and spacing matching the design. This means pixel-level CSS attention and testing in a browser at that resolution.
- **Blazor Server's SignalR circuit is irrelevant here.** Since this is a read-only, single-user, local-only page, the persistent connection adds no value but also no harm. It just works out of the box.
- **Hot reload is a major development productivity win.** .NET 8 Blazor Server supports `dotnet watch` with CSS and Razor hot reload, enabling rapid iteration on the visual design.
- **No JavaScript interop is required.** The entire design can be implemented in pure Razor + CSS + inline SVG.

---

## 3. Recommended Technology Stack

### Frontend (UI Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 | Ships with `Microsoft.AspNetCore.App` |
| **CSS Layout** | Hand-written CSS (Grid + Flexbox) | CSS3 | Matches original design exactly; no CSS framework needed |
| **Timeline Visualization** | Inline SVG in Razor | SVG 1.1 | Simple primitives: `<line>`, `<circle>`, `<polygon>`, `<text>` |
| **Heatmap Grid** | CSS Grid | CSS3 | `grid-template-columns: 160px repeat(N,1fr)` |
| **Font** | Segoe UI (system font) | — | Already on Windows; no web font loading needed |
| **Icons/Symbols** | Inline SVG shapes | — | Diamond (rotated square), circles per the design |

**Why no CSS framework (Bootstrap, Tailwind)?** The design is a fixed 1920×1080 layout optimized for screenshots. Adding Bootstrap would fight the fixed layout and introduce unnecessary classes. The original HTML achieves the design in ~100 lines of CSS. Keep it minimal.

**Why no charting library (Chart.js, Plotly, Radzen Charts)?** The timeline is ~15 SVG elements. A charting library would add 200KB+ of JS, a learning curve, and less control over exact positioning. Inline SVG in Razor is simpler, faster, and gives pixel-perfect control.

### Backend (Data Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Native, high-performance, zero dependencies |
| **Data Loading** | `IConfiguration` or direct file read | .NET 8 | Load `data.json` at startup via `JsonSerializer.Deserialize<T>()` |
| **Data Models** | C# records | C# 12 | Immutable, clean, perfect for read-only display data |
| **Dependency Injection** | Built-in DI | .NET 8 | Register a `DashboardDataService` as Singleton |

**Why not `Newtonsoft.Json`?** `System.Text.Json` is the default in .NET 8, faster, allocates less, and handles this simple schema without issue. No reason to add a dependency.

### Data Storage

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **Primary Store** | `data.json` file | Flat file in project root or `wwwroot/data/` |
| **Format** | JSON | Human-editable, version-controllable |
| **Schema** | Strongly-typed C# records | Compile-time safety for data structure |

**No database is needed.** The data is read-only, small (likely <50KB), and edited by hand or by a separate process. A JSON file is the simplest, most transparent approach.

### Project Structure

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **Solution Structure** | Single `.sln` with one `.csproj` | `ReportingDashboard.sln` → `src/ReportingDashboard/ReportingDashboard.csproj` |
| **Project Template** | `dotnet new blazorserver` | .NET 8 Blazor Server template |
| **Build Tool** | `dotnet` CLI | `dotnet build`, `dotnet run`, `dotnet watch` |

### Testing

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | 2.7+ | For data model and service tests |
| **Blazor Component Testing** | bUnit | 1.25+ | For Razor component rendering tests |
| **Visual Regression** | Playwright for .NET | 1.41+ | Screenshot comparison at 1920×1080 (optional but valuable) |

**Testing is optional for MVP** given the simplicity, but if added, Playwright screenshot tests would directly validate the primary use case (screenshot fidelity).

### Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| `dotnet watch` | Hot reload during development | CSS + Razor changes reflected instantly |
| Browser DevTools | CSS debugging, 1920×1080 viewport testing | Use Chrome's device toolbar to set exact resolution |
| VS Code / Visual Studio | IDE | Either works; VS has better Blazor tooling |

---

## 4. Architecture Recommendations

### Overall Pattern: **Minimal Single-Page Read-Only Dashboard**

This is deliberately not a "real" web application. It's a **data visualization page** that reads a JSON file and renders HTML/SVG. The architecture should reflect this simplicity.

```
data.json ──► DashboardDataService ──► DashboardPage.razor ──► Browser (1920×1080)
                (Singleton)              (Single Razor Component)
```

### Recommended File Structure

```
ReportingDashboard.sln
└── src/ReportingDashboard/
    ├── ReportingDashboard.csproj
    ├── Program.cs                    # Minimal startup, register services
    ├── Models/
    │   └── DashboardData.cs          # C# records for JSON schema
    ├── Services/
    │   └── DashboardDataService.cs   # Loads and caches data.json
    ├── Components/
    │   ├── Pages/
    │   │   └── Dashboard.razor       # Main (and only) page
    │   ├── Layout/
    │   │   └── MainLayout.razor      # Minimal layout wrapper
    │   ├── TimelineSection.razor     # SVG timeline component
    │   └── HeatmapSection.razor      # CSS Grid heatmap component
    ├── wwwroot/
    │   ├── css/
    │   │   └── dashboard.css         # All styles in one file
    │   └── data/
    │       └── data.json             # Dashboard data
    └── Properties/
        └── launchSettings.json
```

### Data Model Design

Based on the original HTML design, the `data.json` schema should capture:

```csharp
// Models/DashboardData.cs
public record DashboardData
{
    public required HeaderInfo Header { get; init; }
    public required List<Milestone> Milestones { get; init; }
    public required List<TimelineTrack> TimelineTracks { get; init; }
    public required HeatmapData Heatmap { get; init; }
}

public record HeaderInfo
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string BacklogLink { get; init; }
    public required DateOnly CurrentDate { get; init; }
}

public record Milestone
{
    public required string Label { get; init; }
    public required DateOnly Date { get; init; }
    public required string Type { get; init; }  // "poc", "production", "checkpoint"
}

public record TimelineTrack
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Description { get; init; }
    public required string Color { get; init; }
    public required List<Milestone> Milestones { get; init; }
}

public record HeatmapData
{
    public required List<string> Columns { get; init; }  // Month names
    public required string HighlightColumn { get; init; } // Current month
    public required List<HeatmapRow> Rows { get; init; }
}

public record HeatmapRow
{
    public required string Category { get; init; }  // "Shipped", "In Progress", etc.
    public required string CategoryStyle { get; init; } // "ship", "prog", "carry", "block"
    public required List<List<string>> Cells { get; init; } // Items per column
}
```

### Component Architecture

**Dashboard.razor** (the page) should compose three sub-components:

1. **HeaderSection** — Title, subtitle, legend. Pure HTML/CSS, bound to `HeaderInfo`.
2. **TimelineSection** — SVG rendering. Takes `List<TimelineTrack>`, `DateOnly currentDate`, and a date range. Calculates X positions based on date math. Renders SVG lines, circles, diamonds, and text.
3. **HeatmapSection** — CSS Grid rendering. Takes `HeatmapData`. Renders column headers, row headers, and cells with colored bullet items.

### Data Flow

1. `Program.cs` registers `DashboardDataService` as a Singleton.
2. `DashboardDataService` reads `wwwroot/data/data.json` on first access, deserializes to `DashboardData`, caches in memory.
3. `Dashboard.razor` injects the service, calls `GetData()` in `OnInitializedAsync`.
4. Sub-components receive data as `[Parameter]` properties and render.
5. No interactivity, no form submissions, no state changes.

### CSS Strategy

**Recommendation: Single CSS file mirroring the original design's approach.**

The original HTML uses a flat, specific CSS structure. Replicate this in `dashboard.css`:

- Use the **exact same color values** from the original design (see color palette below).
- Use **CSS Grid** for the heatmap with the same column template.
- Use **Flexbox** for header layout and timeline sidebar.
- Set `body` to `width: 1920px; height: 1080px; overflow: hidden;` for screenshot consistency.
- Use **CSS custom properties** (variables) for the color palette to enable easy theming.

### Color Palette (from original design)

```css
:root {
    /* Base */
    --bg-white: #FFFFFF;
    --text-primary: #111;
    --text-secondary: #888;
    --text-dark: #333;
    --link-blue: #0078D4;
    --border-light: #E0E0E0;
    --border-heavy: #CCC;
    --bg-subtle: #FAFAFA;
    --bg-header: #F5F5F5;
    --text-muted: #999;

    /* Shipped (Green) */
    --ship-text: #1B7A28;
    --ship-header-bg: #E8F5E9;
    --ship-cell-bg: #F0FBF0;
    --ship-cell-highlight: #D8F2DA;
    --ship-dot: #34A853;

    /* In Progress (Blue) */
    --prog-text: #1565C0;
    --prog-header-bg: #E3F2FD;
    --prog-cell-bg: #EEF4FE;
    --prog-cell-highlight: #DAE8FB;
    --prog-dot: #0078D4;

    /* Carryover (Amber) */
    --carry-text: #B45309;
    --carry-header-bg: #FFF8E1;
    --carry-cell-bg: #FFFDE7;
    --carry-cell-highlight: #FFF0B0;
    --carry-dot: #F4B400;

    /* Blockers (Red) */
    --block-text: #991B1B;
    --block-header-bg: #FEF2F2;
    --block-cell-bg: #FFF5F5;
    --block-cell-highlight: #FFE4E4;
    --block-dot: #EA4335;

    /* Timeline */
    --now-line: #EA4335;
    --milestone-poc: #F4B400;
    --milestone-prod: #34A853;
    --checkpoint: #999;
}
```

### SVG Timeline Implementation Notes

The timeline is the most complex visual element. Key implementation details:

1. **Date-to-X mapping:** Calculate pixel position as `(date - startDate) / (endDate - startDate) * svgWidth`. The original uses a 6-month range (Jan–Jun) across 1560px.
2. **Track rendering:** Each track is a horizontal line at a fixed Y offset with milestones plotted along it.
3. **Milestone shapes:**
   - Checkpoint: `<circle>` with white fill and colored stroke
   - PoC: `<polygon>` diamond shape (rotated square) in amber (#F4B400)
   - Production: `<polygon>` diamond in green (#34A853)
4. **"NOW" line:** Dashed vertical line at the current date's X position, colored #EA4335.
5. **Drop shadow filter:** `<filter id="sh"><feDropShadow dx="0" dy="1" stdDeviation="1.5" flood-opacity="0.3"/></filter>` applied to diamond milestones.

**Implement this as a Razor component** that takes the track data and computes SVG elements in C#, rendering them in the markup. No JavaScript or charting library needed.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope. The dashboard runs on `localhost` for a single user generating screenshots. No login, no roles, no tokens.

If future requirements demand sharing, the simplest addition would be `Microsoft.AspNetCore.Authentication.Negotiate` for Windows Integrated Auth (zero-config on corporate networks), but do not build this now.

### Data Protection

- **No sensitive data.** The dashboard displays project status information (milestone names, dates, work item titles). This is not PII or confidential financial data.
- **`data.json` is a local file.** No encryption needed. It lives in the project directory and is version-controllable.
- If sensitivity increases later, the simplest protection is Windows NTFS permissions on the directory.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Kestrel (built-in, default) |
| **Port** | `https://localhost:5001` or `http://localhost:5000` |
| **Deployment** | `dotnet run` from source, or `dotnet publish -c Release` for a self-contained binary |
| **No reverse proxy needed** | Single user, local only |
| **No Docker needed** | Adds complexity without benefit for local-only use |
| **No IIS needed** | Kestrel is sufficient |

### Infrastructure Cost

**$0.** Everything runs locally on the developer's machine. No cloud services, no subscriptions, no licenses beyond what's already available with a .NET 8 SDK installation.

---

## 6. Risks & Trade-offs

### Risk 1: Blazor Server Is Overkill for a Static Page
**Impact:** Low | **Probability:** N/A (it's a known trade-off)

A static HTML file would technically suffice. However, Blazor Server provides: (a) C# model binding for `data.json`, (b) component composition for maintainability, (c) hot reload for rapid iteration, and (d) a path to future interactivity if needed. The overhead is a ~30MB runtime and a SignalR connection that's unused. This is acceptable for a local-only tool.

**Mitigation:** None needed. The benefits outweigh the overhead.

### Risk 2: Screenshot Fidelity Across Browsers/Machines
**Impact:** Medium | **Probability:** Medium

The design uses Segoe UI (Windows-only font) and fixed 1920×1080 dimensions. Screenshots taken on different machines or browsers may vary slightly.

**Mitigation:**
- Document the exact browser and viewport settings for screenshots (e.g., "Chrome, F12 → Device Toolbar → 1920×1080, 100% zoom").
- Optionally add Playwright screenshot automation for consistent output.
- Segoe UI is available on all Windows machines in a corporate environment.

### Risk 3: `data.json` Schema Drift
**Impact:** Low | **Probability:** Medium

As the dashboard evolves, the JSON schema may change, breaking existing `data.json` files.

**Mitigation:**
- Use C# records with `required` properties for compile-time validation.
- Add a `"schemaVersion"` field to `data.json`.
- Validate on load with clear error messages.

### Risk 4: SVG Timeline Date Math Edge Cases
**Impact:** Low | **Probability:** Low

Date-to-pixel calculations could produce unexpected results for milestones outside the visible date range, or for very short/long time spans.

**Mitigation:**
- Clamp milestone X positions to the SVG viewport bounds.
- Make the date range configurable in `data.json` (startDate, endDate).
- Test with edge cases: milestones on range boundaries, single-day range, multi-year range.

### Trade-off: No Real-Time Updates
The dashboard reads `data.json` on page load (or app start). Changes to the file require a page refresh. This is acceptable for the screenshot use case. If real-time updates are later desired, `FileSystemWatcher` + Blazor `StateHasChanged()` is a simple addition.

### Trade-off: No Data Entry UI
Users edit `data.json` by hand or with a text editor. This is intentional simplicity. If a data entry form is later needed, it's a straightforward Blazor form addition.

---

## 7. Open Questions

1. **Date range for the timeline:** Should it always show 6 months? Should it auto-calculate from the earliest to latest milestone? Or should it be configurable in `data.json`?
   - **Recommendation:** Make it configurable with a sensible default (current month ± 3 months).

2. **Number of heatmap columns:** The original shows 4 months (Jan–Apr). Should this be dynamic based on the data, or fixed?
   - **Recommendation:** Dynamic based on `data.json` columns array, with a practical maximum of 6–8 for readability at 1920px.

3. **"Current month" highlighting:** The original highlights April cells with a different background. Should this be automatic (based on system date) or explicit in `data.json`?
   - **Recommendation:** Configurable in `data.json` via a `highlightColumn` field, defaulting to current month if omitted.

4. **Multiple projects:** Will the user ever need to switch between different `data.json` files for different projects?
   - **Recommendation:** Support a `--data` command-line argument or query parameter (`?data=project-a.json`) for easy switching.

5. **Print/export:** Beyond screenshots, is there a need for PDF export or print-optimized CSS?
   - **Recommendation:** Add `@media print` CSS rules as a quick win. PDF export can be deferred.

---

## 8. Implementation Recommendations

### Phase 1: MVP (1–2 days)

**Goal:** Pixel-perfect reproduction of the original HTML design, powered by `data.json`.

1. **Scaffold the project** — `dotnet new blazorserver -n ReportingDashboard -f net8.0`, create `.sln`.
2. **Define data models** — C# records matching the JSON schema above.
3. **Create `data.json`** — Fictional project data mimicking the original design's structure (3 timeline tracks, 4 heatmap rows × 4 months).
4. **Build `DashboardDataService`** — Read and deserialize `data.json`, register as Singleton.
5. **Implement `Dashboard.razor`** — Single page with three sections:
   - Header (title, subtitle, legend)
   - Timeline (SVG)
   - Heatmap (CSS Grid)
6. **Write `dashboard.css`** — Port the original CSS verbatim, using CSS custom properties for colors.
7. **Verify at 1920×1080** — Open in Chrome, set viewport, compare visually to the original design.

### Phase 2: Polish & Improvements (1 day)

1. **Responsive date calculation** — Timeline SVG positions computed from actual dates in the data.
2. **Dynamic heatmap columns** — Support variable number of months.
3. **Current month auto-detection** — Highlight the correct column based on system date.
4. **Error handling** — Friendly error page if `data.json` is missing or malformed.
5. **Hot reload verification** — Ensure `dotnet watch` picks up CSS and Razor changes.

### Phase 3: Nice-to-Haves (optional, 0.5–1 day each)

1. **Playwright screenshot automation** — Script that launches the app, waits for render, takes a 1920×1080 screenshot, saves as PNG. Eliminates manual browser setup.
2. **Print CSS** — `@media print` rules for direct browser printing.
3. **Multiple data files** — Query parameter or CLI argument to switch between `data.json` files.
4. **Dark mode** — Swap CSS custom properties for a dark palette (useful for some presentation contexts).
5. **Tooltip hover details** — Blazor `@onmouseover` to show additional milestone details (light interactivity, still screenshot-friendly).

### Quick Wins

- **`dotnet watch run`** gives instant feedback during CSS/layout iteration.
- **CSS custom properties** make color theming a 5-minute task.
- **C# records with `required`** catch `data.json` schema errors at startup, not at render time.
- **Putting `data.json` in `wwwroot/data/`** makes it editable without rebuilding, and hot reload picks up changes.

### What NOT to Build

- ❌ Authentication / authorization
- ❌ Database (SQLite, SQL Server, or otherwise)
- ❌ REST API endpoints
- ❌ Admin panel for data entry
- ❌ Multi-user support
- ❌ Docker containerization
- ❌ CI/CD pipeline
- ❌ Logging infrastructure (beyond `Console.WriteLine` for debugging)
- ❌ Unit tests for MVP (add in Phase 3 if the project persists)

---

## Appendix A: Example `data.json` Structure

```json
{
  "schemaVersion": 1,
  "header": {
    "title": "Project Phoenix Release Roadmap",
    "subtitle": "Engineering Platform • Core Infrastructure Workstream • April 2026",
    "backlogLink": "https://dev.azure.com/org/project/_backlogs",
    "currentDate": "2026-04-17"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "m1",
        "label": "M1",
        "description": "API Gateway & Auth",
        "color": "#0078D4",
        "milestones": [
          { "label": "Kickoff", "date": "2026-01-12", "type": "checkpoint" },
          { "label": "Mar 26 PoC", "date": "2026-03-26", "type": "poc" },
          { "label": "May Prod", "date": "2026-05-01", "type": "production" }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["January", "February", "March", "April"],
    "highlightColumn": "April",
    "rows": [
      {
        "category": "Shipped",
        "categoryStyle": "ship",
        "cells": [
          ["API scaffolding", "CI pipeline"],
          ["Auth module v1", "DB schema"],
          ["Load testing", "Monitoring setup"],
          ["Gateway PoC", "SDK v1 release"]
        ]
      },
      {
        "category": "In Progress",
        "categoryStyle": "prog",
        "cells": [
          ["Auth design"],
          ["Gateway prototype"],
          ["SDK integration"],
          ["Perf optimization", "Docs site"]
        ]
      },
      {
        "category": "Carryover",
        "categoryStyle": "carry",
        "cells": [
          [],
          ["Config service"],
          ["Config service", "Rate limiting"],
          ["Rate limiting"]
        ]
      },
      {
        "category": "Blockers",
        "categoryStyle": "block",
        "cells": [
          [],
          ["Vendor API delay"],
          [],
          ["Capacity approval pending"]
        ]
      }
    ]
  }
}
```

## Appendix B: Key NuGet Packages

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.App` | 8.0.x | Blazor Server runtime | ✅ Built-in |
| `System.Text.Json` | 8.0.x | JSON deserialization | ✅ Built-in |
| `Microsoft.Playwright` | 1.41+ | Screenshot automation | ❌ Optional |
| `bunit` | 1.25+ | Component testing | ❌ Optional |
| `xunit` | 2.7+ | Unit testing | ❌ Optional |

**Total required external NuGet packages: 0.** Everything needed ships with the .NET 8 SDK.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/caa9b00bc6763437692f416ccc1d666c311b1418/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
