# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-15 17:38 UTC_

### Summary

This project is a **lightweight, single-page Blazor Server dashboard** designed for one purpose: generating polished, screenshot-ready project status views for executive PowerPoint decks. The design features a timeline/Gantt section with SVG milestones, a color-coded heatmap grid (Shipped/In Progress/Carryover/Blockers), and reads all data from a local `data.json` file. **Primary recommendation:** Keep this dead simple. Use vanilla Blazor Server with zero third-party UI component libraries. The design is a static-data render — no real-time updates, no databases, no auth. Inline SVG for the timeline, CSS Grid for the heatmap, and `System.Text.Json` deserialization of `data.json` is the entire architecture. Any additional complexity is over-engineering for a tool whose output is literally a screenshot. The entire solution should be completable in **1–2 days** by a single developer, resulting in a `.sln` with one Blazor Server project, one shared models library, and one `data.json` file. --- | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Blazor Server runtime | Yes (implicit) | | `System.Text.Json` | 8.0.x | JSON deserialization | Yes (built-in) | | `bUnit` | 1.25+ | Component testing | Optional | | `xUnit` | 2.7+ | Unit test framework | Optional | | `Microsoft.Playwright` | 1.40+ | Automated screenshot capture | Optional | **Total additional NuGet packages required: 0.** Everything needed ships with the .NET 8 SDK.

### Key Findings

- The original `OriginalDesignConcept.html` is a fully self-contained static HTML/CSS/SVG page at 1920×1080 — the Blazor version should preserve this exact resolution and visual fidelity for screenshot capture.
- No charting library is needed. The timeline is a simple SVG with lines, circles, diamonds (polygons), and text — all trivially reproducible with inline `<svg>` in Blazor markup.
- The heatmap is pure CSS Grid (`grid-template-columns: 160px repeat(4, 1fr)`) with colored cells and bullet-prefixed text items — no component library required.
- Data volume is trivially small (dozens of items at most). A `data.json` file read via `System.Text.Json` on startup is sufficient and ideal.
- Blazor Server's SignalR circuit is irrelevant here — the page is effectively a static render. But Blazor Server is chosen (per mandate), and it works fine for local use.
- No authentication, authorization, or security hardening is needed — this is a local dev tool for one person.
- The color palette, font (Segoe UI), and layout dimensions are fully specified in the HTML reference and must be matched exactly.
- CSS isolation (`.razor.css` files) in Blazor is the right approach for component-scoped styles that mirror the reference HTML's class structure. ---
- `dotnet new blazorserver -n ReportingDashboard.Web --framework net8.0`
- Create solution: `dotnet new sln`, add projects
- Strip default template (remove Counter, Weather, NavMenu, Sidebar)
- Create `Dashboard.razor` as the single page at `/`
- Set `body` to fixed 1920×1080
- Implement `DashboardHeader` component with static text
- Implement `HeatmapGrid` with CSS Grid matching reference layout
- Verify visual match against reference at 1920×1080
- Define C# model records (`DashboardData`, `Milestone`, `StatusRow`, etc.)
- Create `data.json` with fictional project data
- Implement `DataService` to load and deserialize JSON
- Wire data into components via `[Parameter]` cascading
- Implement `HeatmapRow` and `HeatmapCell` with dynamic item rendering
- Apply color theming via CSS classes matching reference
- Implement `TimelineSection` with inline SVG
- Build date-to-X-position calculation
- Render month gridlines, track lines, "NOW" indicator
- Render milestone markers (diamonds, circles) with labels
- Implement milestone track labels (left sidebar of timeline area)
- Fine-tune spacing, colors, typography against reference
- Add CSS custom properties for easy color palette changes
- Implement `FileSystemWatcher` for live JSON reload (optional)
- Document screenshot capture workflow
- Create example `data.json` for a second fictional project to validate flexibility
- **Fastest path to visual output:** Start with hardcoded HTML/CSS matching the reference exactly, then extract into components and data-bind afterward. This validates the layout before adding complexity.
- **`dotnet watch` for iteration:** CSS changes reflect instantly without full rebuild — critical for pixel-matching the reference design.
- **Browser DevTools device emulation:** Set to 1920×1080 for consistent preview regardless of monitor resolution.
- **SVG timeline:** Prototype the date-to-pixel math in a standalone `.razor` component with test data before integrating. The coordinate system is the trickiest part of the build.
- **CSS Grid heatmap:** Copy the reference HTML's CSS classes verbatim into a `.razor.css` file first, then adapt. Starting from proven CSS is faster than writing from scratch. ---
```json
{
  "header": {
    "title": "Project Phoenix Release Roadmap",
    "subtitle": "Platform Engineering · Phoenix Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project",
    "currentMonth": "April 2026"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "nowDate": "2026-04-15",
    "tracks": [
      {
        "id": "m1",
        "label": "M1",
        "description": "Core API & Auth",
        "color": "#0078D4"
      },
      {
        "id": "m2",
        "label": "M2",
        "description": "Data Pipeline",
        "color": "#00897B"
      },
      {
        "id": "m3",
        "label": "M3",
        "description": "Dashboard UI",
        "color": "#546E7A"
      }
    ],
    "milestones": [
      {
        "trackId": "m1",
        "date": "2026-01-15",
        "label": "Jan 15",
        "type": "checkpoint",
        "description": "API Design Complete"
      },
      {
        "trackId": "m1",
        "date": "2026-03-20",
        "label": "Mar 20 PoC",
        "type": "poc",
        "description": "Auth PoC Demo"
      },
      {
        "trackId": "m1",
        "date": "2026-05-01",
        "label": "May Prod",
        "type": "production",
        "description": "Production Release"
      }
    ]
  },
  "heatmap": {
    "columns": ["January", "February", "March", "April"],
    "highlightColumnIndex": 3,
    "rows": [
      {
        "category": "Shipped",
        "colorTheme": "shipped",
        "cells": [
          { "items": ["API v1 endpoint", "Auth module"] },
          { "items": ["Data ingestion pipeline"] },
          { "items": ["Dashboard wireframes", "CI/CD pipeline"] },
          { "items": ["Search indexing", "Monitoring alerts"] }
        ]
      },
      {
        "category": "In Progress",
        "colorTheme": "progress",
        "cells": [
          { "items": ["Schema design"] },
          { "items": ["API v2 planning", "Load testing"] },
          { "items": ["Role-based access"] },
          { "items": ["Performance optimization", "E2E tests"] }
        ]
      },
      {
        "category": "Carryover",
        "colorTheme": "carryover",
        "cells": [
          { "items": [] },
          { "items": ["Legacy migration script"] },
          { "items": ["Legacy migration script"] },
          { "items": ["Documentation update"] }
        ]
      },
      {
        "category": "Blockers",
        "colorTheme": "blockers",
        "cells": [
          { "items": [] },
          { "items": [] },
          { "items": ["Vendor API access pending"] },
          { "items": ["Vendor API access pending", "Capacity approval"] }
        ]
      }
    ]
  }
}
``` ---

### Recommended Tools & Technologies

- **Date:** April 15, 2026 **Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure **Project:** Single-page executive milestone & progress reporting dashboard --- | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **UI Framework** | Blazor Server (built-in) | .NET 8.0 | No additional UI library needed | | **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches the reference design exactly | | **SVG Timeline** | Inline `<svg>` in Razor markup | N/A | No charting library — the design uses simple SVG primitives (line, circle, polygon, text) | | **CSS Isolation** | Blazor scoped CSS (`.razor.css`) | Built-in .NET 8 | One CSS file per component, mirrors the reference HTML's class structure | | **Font** | Segoe UI (system font) | N/A | Already available on Windows; specify fallbacks `'Segoe UI', Arial, sans-serif` | | **Icons/Shapes** | Pure CSS + inline SVG | N/A | Diamond shapes via CSS `transform: rotate(45deg)`, circles via `border-radius` | **Why no UI component library (MudBlazor, Radzen, etc.):** The design is bespoke and pixel-specific. Component libraries add 500KB+ of CSS, impose their own design language, and create fights when matching a custom layout. The reference HTML is ~200 lines of simple CSS — replicating it directly is faster and more accurate than configuring a library to match. | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built-in .NET 8 | Native, fast, zero dependencies | | **Configuration** | `IConfiguration` + custom JSON | Built-in | Load `data.json` via `AddJsonFile()` or manual `JsonSerializer.Deserialize<T>()` | | **Data Models** | C# records / POCOs | N/A | Strongly-typed models for milestones, status items, timeline events | | **Data Service** | Singleton `IDataService` | N/A | Reads and caches `data.json` on startup, injectable into components | **Why not a database:** The data is a handful of project status items edited by hand in JSON. SQLite, LiteDB, or any database adds migration complexity, tooling, and a dependency — all for data that fits in a 2KB file. `data.json` is the right answer. | Component | Recommendation | Notes | |-----------|---------------|-------| | **Solution structure** | `.sln` with 2 projects | `ReportingDashboard.Web` (Blazor Server) + `ReportingDashboard.Models` (shared POCOs) | | **SDK** | `Microsoft.NET.Sdk.Web` | Standard Blazor Server template | | **Target framework** | `net8.0` | LTS release, supported through November 2026 | | Component | Recommendation | Version | Notes | |-----------|---------------|---------|-------| | **Unit Testing** | xUnit | 2.7+ | For model deserialization tests | | **Blazor Component Testing** | bUnit | 1.25+ | For rendering verification (optional — low priority given simplicity) | | **Snapshot Testing** | Verify | 23.x | Optional — compare rendered HTML snapshots | **Honest assessment:** For a project this simple, automated tests are optional. The "test" is: does the screenshot look right? Manual visual verification is the primary QA method here. | Component | Recommendation | Notes | |-----------|---------------|-------| | **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Standard .NET development | | **Hot Reload** | `dotnet watch` | Built-in .NET 8, essential for rapid CSS/layout iteration | | **Screenshot Capture** | Browser DevTools or Playwright | For consistent 1920×1080 captures | | **Package Manager** | NuGet (built-in) | No additional package managers needed | --- This is explicitly **not** a place for Clean Architecture, CQRS, MediatR, or any enterprise pattern. The entire application is:
```
data.json → DataService → Blazor Components → Rendered HTML/CSS/SVG
``` That's it. Three layers, one direction, no abstractions beyond what's naturally needed. Map components directly to the visual sections of the reference design:
```
Pages/
  Dashboard.razor              — Main page, composes all sections

Components/
  DashboardHeader.razor        — Title, subtitle, legend (header bar)
  TimelineSection.razor        — SVG milestone timeline (the Gantt area)
  TimelineMilestone.razor      — Individual milestone marker (diamond/circle)
  HeatmapGrid.razor            — CSS Grid heatmap container
  HeatmapRow.razor             — Single status row (Shipped/In Progress/etc.)
  HeatmapCell.razor             — Single month cell with item bullets
```
- **Startup:** `Program.cs` registers `DataService` as singleton
- **DataService:** Reads `data.json` from disk via `System.Text.Json`, deserializes into strongly-typed models, caches in memory
- **Dashboard.razor:** Injects `DataService`, passes data down to child components via parameters
- **Components:** Pure rendering — receive data via `[Parameter]`, emit HTML/CSS/SVG
```csharp
// Top-level config
public record DashboardData(
    ProjectHeader Header,
    TimelineConfig Timeline,
    List<Milestone> Milestones,
    HeatmapData Heatmap
);

public record ProjectHeader(
    string Title,
    string Subtitle,
    string BacklogUrl,
    string CurrentMonth
);

public record TimelineConfig(
    DateTime StartDate,
    DateTime EndDate,
    DateTime NowDate,
    List<string> MonthLabels
);

public record Milestone(
    string Id,
    string Label,
    string Description,
    DateTime Date,
    MilestoneType Type,    // PoC, Production, Checkpoint
    string Color,
    string TrackId          // Which horizontal track line
);

public record HeatmapData(
    List<string> ColumnHeaders,    // Month names
    int HighlightColumnIndex,      // "Current month" highlight
    List<StatusRow> Rows
);

public record StatusRow(
    string Category,               // Shipped, InProgress, Carryover, Blockers
    string ColorTheme,             // Maps to CSS class prefix
    List<MonthCell> Cells
);

public record MonthCell(
    List<string> Items             // Bullet point text items
);
``` **Approach:** Mirror the reference HTML's class naming, adapted to Blazor CSS isolation.
- Each `.razor` component gets a matching `.razor.css` file
- Use CSS custom properties (variables) for the color palette, defined in `wwwroot/css/app.css`:
```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-highlight: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-highlight: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-highlight: #FFF0B0;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-highlight: #FFE4E4;
    --font-primary: 'Segoe UI', Arial, sans-serif;
}
```
- Fixed viewport: `width: 1920px; height: 1080px; overflow: hidden;` on `body` — this ensures consistent screenshot output regardless of browser window size. The timeline is the most complex visual element, but it's still straightforward inline SVG:
- **Horizontal track lines:** `<line>` elements, one per milestone track (M1, M2, M3), colored by track
- **Month gridlines:** Vertical `<line>` elements at calculated X positions
- **"NOW" indicator:** Dashed vertical `<line>` in red (#EA4335) at the calculated current-date X position
- **Milestones:** `<polygon>` for diamonds (PoC/Production), `<circle>` for checkpoints, with `<text>` labels
- **X-position calculation:** Linear interpolation based on date within the timeline's start/end range
```csharp
private double DateToX(DateTime date, DateTime start, DateTime end, double svgWidth)
{
    double totalDays = (end - start).TotalDays;
    double dayOffset = (date - start).TotalDays;
    return (dayOffset / totalDays) * svgWidth;
}
``` All SVG values should be computed from `data.json` — no hardcoded pixel positions. ---

### Considerations & Risks

- **None.** This is explicitly a local-only tool for a single user. Adding auth would be over-engineering.
- No `[Authorize]` attributes
- No Identity framework
- No cookie/token authentication
- `builder.Services.AddAuthentication()` is **not called**
- `data.json` contains project status information — not PII, not secrets
- No encryption needed
- No HTTPS required for local use (though Kestrel defaults to it; keep the dev cert for convenience) | Aspect | Recommendation | |--------|---------------| | **Runtime** | Local Kestrel server via `dotnet run` | | **Port** | Default `https://localhost:5001` or `http://localhost:5000` | | **Deployment** | None — run from source with `dotnet run` or `dotnet watch` | | **Containerization** | Not needed — local dev tool | | **Reverse proxy** | Not needed |
- **Hot reload:** Use `dotnet watch` during development for instant CSS/markup iteration
- **Screenshot workflow:** Open browser at 1920×1080, navigate to dashboard, capture via browser DevTools device emulation (set to 1920×1080 with no device frame) or Playwright script
- **Data updates:** Edit `data.json`, restart app (or implement file watcher for auto-reload) For convenience, implement `FileSystemWatcher` on `data.json` to auto-reload data without restarting:
```csharp
public class DataService : IDisposable
{
    private FileSystemWatcher _watcher;
    private DashboardData _data;

    public DataService()
    {
        LoadData();
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(DataFilePath)!);
        _watcher.Filter = "data.json";
        _watcher.Changed += (_, _) => LoadData();
        _watcher.EnableRaisingEvents = true;
    }
}
``` This is a nice-to-have that costs ~10 lines of code and significantly improves the edit-preview workflow. --- | Risk | Impact | Mitigation | |------|--------|------------| | **SVG layout math errors** | Timeline milestones positioned incorrectly | Compute positions from data with a pure function; easy to unit test | | **CSS Grid cross-browser differences** | Heatmap renders differently in Edge vs Chrome | Target a single browser (Edge/Chrome); this is for screenshots, not public web | | **Segoe UI unavailable on non-Windows** | Font rendering differs on macOS/Linux | Not a concern — mandated local Windows development | | **`data.json` schema drift** | App crashes on malformed JSON | Use strongly-typed deserialization with `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`; fail fast with clear error | | Risk | Impact | Mitigation | |------|--------|------------| | **Scope creep** | "Simple dashboard" grows into a full project management tool | Hard constraint: one page, one JSON file, no database, no auth. Reject feature requests that violate this. | | **Over-engineering the architecture** | Developer spends 3 days on DI, patterns, and abstractions for a 500-line app | This document explicitly recommends flat architecture. If it feels like enterprise software, you've gone too far. | | **Blazor Server overhead for static content** | SignalR circuit is unnecessary weight for a page that doesn't interact | Acceptable trade-off — it's the mandated stack, and the overhead is negligible for local single-user use |
- **Blazor Server vs. static HTML:** The reference design is pure HTML/CSS/SVG — a static file would suffice. Blazor Server adds a runtime, SignalR connection, and process. We accept this because: (a) it's the mandated stack, (b) it enables data-driven rendering from `data.json`, and (c) future enhancements (interactivity, filtering) are trivial to add.
- **No database:** We lose queryability and relational integrity. We gain simplicity, zero-config setup, and human-readable/editable data. For <100 items, this is the right trade-off.
- **No testing investment:** For a tool this simple, the cost of writing and maintaining tests exceeds the cost of the bugs they'd catch. Visual inspection is the primary QA. --- | # | Question | Who Decides | Impact | |---|----------|-------------|--------| | 1 | **How many milestone tracks (horizontal lines) should the timeline support?** The reference shows 3 (M1, M2, M3). Should this be dynamic from `data.json`? | Product Owner | Affects SVG layout calculations | | 2 | **Should the heatmap rows be fixed (Shipped/In Progress/Carryover/Blockers) or configurable?** | Product Owner | Affects data model and CSS | | 3 | **How many months should the heatmap display?** Reference shows 4 columns. Should this be configurable? | Product Owner | Affects grid layout | | 4 | **Should the "current month" highlight column be auto-detected from system date or specified in `data.json`?** | Developer discretion | Minor implementation detail | | 5 | **Is the C:/Pics/ReportingDashboardDesign.png design a modification of the HTML reference, or a completely different layout?** | Product Owner | Could significantly affect component structure if layout differs | | 6 | **Should the dashboard support printing (Ctrl+P) in addition to screenshots?** | Product Owner | Would require print-specific CSS media queries | | 7 | **Will multiple projects need separate dashboards, or is this always one project at a time?** | Product Owner | Affects whether we need project selection or multiple `data.json` files | ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Date:** April 15, 2026
**Stack:** C# .NET 8 · Blazor Server · Local-only · .sln structure
**Project:** Single-page executive milestone & progress reporting dashboard

---

## 1. Executive Summary

This project is a **lightweight, single-page Blazor Server dashboard** designed for one purpose: generating polished, screenshot-ready project status views for executive PowerPoint decks. The design features a timeline/Gantt section with SVG milestones, a color-coded heatmap grid (Shipped/In Progress/Carryover/Blockers), and reads all data from a local `data.json` file.

**Primary recommendation:** Keep this dead simple. Use vanilla Blazor Server with zero third-party UI component libraries. The design is a static-data render — no real-time updates, no databases, no auth. Inline SVG for the timeline, CSS Grid for the heatmap, and `System.Text.Json` deserialization of `data.json` is the entire architecture. Any additional complexity is over-engineering for a tool whose output is literally a screenshot.

The entire solution should be completable in **1–2 days** by a single developer, resulting in a `.sln` with one Blazor Server project, one shared models library, and one `data.json` file.

---

## 2. Key Findings

- The original `OriginalDesignConcept.html` is a fully self-contained static HTML/CSS/SVG page at 1920×1080 — the Blazor version should preserve this exact resolution and visual fidelity for screenshot capture.
- No charting library is needed. The timeline is a simple SVG with lines, circles, diamonds (polygons), and text — all trivially reproducible with inline `<svg>` in Blazor markup.
- The heatmap is pure CSS Grid (`grid-template-columns: 160px repeat(4, 1fr)`) with colored cells and bullet-prefixed text items — no component library required.
- Data volume is trivially small (dozens of items at most). A `data.json` file read via `System.Text.Json` on startup is sufficient and ideal.
- Blazor Server's SignalR circuit is irrelevant here — the page is effectively a static render. But Blazor Server is chosen (per mandate), and it works fine for local use.
- No authentication, authorization, or security hardening is needed — this is a local dev tool for one person.
- The color palette, font (Segoe UI), and layout dimensions are fully specified in the HTML reference and must be matched exactly.
- CSS isolation (`.razor.css` files) in Blazor is the right approach for component-scoped styles that mirror the reference HTML's class structure.

---

## 3. Recommended Technology Stack

### Frontend (UI Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **UI Framework** | Blazor Server (built-in) | .NET 8.0 | No additional UI library needed |
| **CSS Layout** | Native CSS Grid + Flexbox | N/A | Matches the reference design exactly |
| **SVG Timeline** | Inline `<svg>` in Razor markup | N/A | No charting library — the design uses simple SVG primitives (line, circle, polygon, text) |
| **CSS Isolation** | Blazor scoped CSS (`.razor.css`) | Built-in .NET 8 | One CSS file per component, mirrors the reference HTML's class structure |
| **Font** | Segoe UI (system font) | N/A | Already available on Windows; specify fallbacks `'Segoe UI', Arial, sans-serif` |
| **Icons/Shapes** | Pure CSS + inline SVG | N/A | Diamond shapes via CSS `transform: rotate(45deg)`, circles via `border-radius` |

**Why no UI component library (MudBlazor, Radzen, etc.):** The design is bespoke and pixel-specific. Component libraries add 500KB+ of CSS, impose their own design language, and create fights when matching a custom layout. The reference HTML is ~200 lines of simple CSS — replicating it directly is faster and more accurate than configuring a library to match.

### Backend (Data Layer)

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built-in .NET 8 | Native, fast, zero dependencies |
| **Configuration** | `IConfiguration` + custom JSON | Built-in | Load `data.json` via `AddJsonFile()` or manual `JsonSerializer.Deserialize<T>()` |
| **Data Models** | C# records / POCOs | N/A | Strongly-typed models for milestones, status items, timeline events |
| **Data Service** | Singleton `IDataService` | N/A | Reads and caches `data.json` on startup, injectable into components |

**Why not a database:** The data is a handful of project status items edited by hand in JSON. SQLite, LiteDB, or any database adds migration complexity, tooling, and a dependency — all for data that fits in a 2KB file. `data.json` is the right answer.

### Project Structure

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **Solution structure** | `.sln` with 2 projects | `ReportingDashboard.Web` (Blazor Server) + `ReportingDashboard.Models` (shared POCOs) |
| **SDK** | `Microsoft.NET.Sdk.Web` | Standard Blazor Server template |
| **Target framework** | `net8.0` | LTS release, supported through November 2026 |

### Testing

| Component | Recommendation | Version | Notes |
|-----------|---------------|---------|-------|
| **Unit Testing** | xUnit | 2.7+ | For model deserialization tests |
| **Blazor Component Testing** | bUnit | 1.25+ | For rendering verification (optional — low priority given simplicity) |
| **Snapshot Testing** | Verify | 23.x | Optional — compare rendered HTML snapshots |

**Honest assessment:** For a project this simple, automated tests are optional. The "test" is: does the screenshot look right? Manual visual verification is the primary QA method here.

### Infrastructure & Tooling

| Component | Recommendation | Notes |
|-----------|---------------|-------|
| **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Standard .NET development |
| **Hot Reload** | `dotnet watch` | Built-in .NET 8, essential for rapid CSS/layout iteration |
| **Screenshot Capture** | Browser DevTools or Playwright | For consistent 1920×1080 captures |
| **Package Manager** | NuGet (built-in) | No additional package managers needed |

---

## 4. Architecture Recommendations

### Overall Pattern: **Flat Component Architecture with JSON Data Source**

This is explicitly **not** a place for Clean Architecture, CQRS, MediatR, or any enterprise pattern. The entire application is:

```
data.json → DataService → Blazor Components → Rendered HTML/CSS/SVG
```

That's it. Three layers, one direction, no abstractions beyond what's naturally needed.

### Component Structure

Map components directly to the visual sections of the reference design:

```
Pages/
  Dashboard.razor              — Main page, composes all sections

Components/
  DashboardHeader.razor        — Title, subtitle, legend (header bar)
  TimelineSection.razor        — SVG milestone timeline (the Gantt area)
  TimelineMilestone.razor      — Individual milestone marker (diamond/circle)
  HeatmapGrid.razor            — CSS Grid heatmap container
  HeatmapRow.razor             — Single status row (Shipped/In Progress/etc.)
  HeatmapCell.razor             — Single month cell with item bullets
```

### Data Flow

1. **Startup:** `Program.cs` registers `DataService` as singleton
2. **DataService:** Reads `data.json` from disk via `System.Text.Json`, deserializes into strongly-typed models, caches in memory
3. **Dashboard.razor:** Injects `DataService`, passes data down to child components via parameters
4. **Components:** Pure rendering — receive data via `[Parameter]`, emit HTML/CSS/SVG

### Data Model Design

```csharp
// Top-level config
public record DashboardData(
    ProjectHeader Header,
    TimelineConfig Timeline,
    List<Milestone> Milestones,
    HeatmapData Heatmap
);

public record ProjectHeader(
    string Title,
    string Subtitle,
    string BacklogUrl,
    string CurrentMonth
);

public record TimelineConfig(
    DateTime StartDate,
    DateTime EndDate,
    DateTime NowDate,
    List<string> MonthLabels
);

public record Milestone(
    string Id,
    string Label,
    string Description,
    DateTime Date,
    MilestoneType Type,    // PoC, Production, Checkpoint
    string Color,
    string TrackId          // Which horizontal track line
);

public record HeatmapData(
    List<string> ColumnHeaders,    // Month names
    int HighlightColumnIndex,      // "Current month" highlight
    List<StatusRow> Rows
);

public record StatusRow(
    string Category,               // Shipped, InProgress, Carryover, Blockers
    string ColorTheme,             // Maps to CSS class prefix
    List<MonthCell> Cells
);

public record MonthCell(
    List<string> Items             // Bullet point text items
);
```

### CSS Architecture

**Approach:** Mirror the reference HTML's class naming, adapted to Blazor CSS isolation.

- Each `.razor` component gets a matching `.razor.css` file
- Use CSS custom properties (variables) for the color palette, defined in `wwwroot/css/app.css`:

```css
:root {
    --color-shipped: #34A853;
    --color-shipped-bg: #F0FBF0;
    --color-shipped-bg-highlight: #D8F2DA;
    --color-progress: #0078D4;
    --color-progress-bg: #EEF4FE;
    --color-progress-bg-highlight: #DAE8FB;
    --color-carryover: #F4B400;
    --color-carryover-bg: #FFFDE7;
    --color-carryover-bg-highlight: #FFF0B0;
    --color-blockers: #EA4335;
    --color-blockers-bg: #FFF5F5;
    --color-blockers-bg-highlight: #FFE4E4;
    --font-primary: 'Segoe UI', Arial, sans-serif;
}
```

- Fixed viewport: `width: 1920px; height: 1080px; overflow: hidden;` on `body` — this ensures consistent screenshot output regardless of browser window size.

### SVG Timeline Rendering

The timeline is the most complex visual element, but it's still straightforward inline SVG:

- **Horizontal track lines:** `<line>` elements, one per milestone track (M1, M2, M3), colored by track
- **Month gridlines:** Vertical `<line>` elements at calculated X positions
- **"NOW" indicator:** Dashed vertical `<line>` in red (#EA4335) at the calculated current-date X position
- **Milestones:** `<polygon>` for diamonds (PoC/Production), `<circle>` for checkpoints, with `<text>` labels
- **X-position calculation:** Linear interpolation based on date within the timeline's start/end range

```csharp
private double DateToX(DateTime date, DateTime start, DateTime end, double svgWidth)
{
    double totalDays = (end - start).TotalDays;
    double dayOffset = (date - start).TotalDays;
    return (dayOffset / totalDays) * svgWidth;
}
```

All SVG values should be computed from `data.json` — no hardcoded pixel positions.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly a local-only tool for a single user. Adding auth would be over-engineering.

- No `[Authorize]` attributes
- No Identity framework
- No cookie/token authentication
- `builder.Services.AddAuthentication()` is **not called**

### Data Protection

- `data.json` contains project status information — not PII, not secrets
- No encryption needed
- No HTTPS required for local use (though Kestrel defaults to it; keep the dev cert for convenience)

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Local Kestrel server via `dotnet run` |
| **Port** | Default `https://localhost:5001` or `http://localhost:5000` |
| **Deployment** | None — run from source with `dotnet run` or `dotnet watch` |
| **Containerization** | Not needed — local dev tool |
| **Reverse proxy** | Not needed |

### Operational Concerns

- **Hot reload:** Use `dotnet watch` during development for instant CSS/markup iteration
- **Screenshot workflow:** Open browser at 1920×1080, navigate to dashboard, capture via browser DevTools device emulation (set to 1920×1080 with no device frame) or Playwright script
- **Data updates:** Edit `data.json`, restart app (or implement file watcher for auto-reload)

### Optional: File Watcher for Live JSON Reload

For convenience, implement `FileSystemWatcher` on `data.json` to auto-reload data without restarting:

```csharp
public class DataService : IDisposable
{
    private FileSystemWatcher _watcher;
    private DashboardData _data;

    public DataService()
    {
        LoadData();
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(DataFilePath)!);
        _watcher.Filter = "data.json";
        _watcher.Changed += (_, _) => LoadData();
        _watcher.EnableRaisingEvents = true;
    }
}
```

This is a nice-to-have that costs ~10 lines of code and significantly improves the edit-preview workflow.

---

## 6. Risks & Trade-offs

### Low-Risk Items (Mitigations Straightforward)

| Risk | Impact | Mitigation |
|------|--------|------------|
| **SVG layout math errors** | Timeline milestones positioned incorrectly | Compute positions from data with a pure function; easy to unit test |
| **CSS Grid cross-browser differences** | Heatmap renders differently in Edge vs Chrome | Target a single browser (Edge/Chrome); this is for screenshots, not public web |
| **Segoe UI unavailable on non-Windows** | Font rendering differs on macOS/Linux | Not a concern — mandated local Windows development |
| **`data.json` schema drift** | App crashes on malformed JSON | Use strongly-typed deserialization with `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`; fail fast with clear error |

### Medium-Risk Items

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Scope creep** | "Simple dashboard" grows into a full project management tool | Hard constraint: one page, one JSON file, no database, no auth. Reject feature requests that violate this. |
| **Over-engineering the architecture** | Developer spends 3 days on DI, patterns, and abstractions for a 500-line app | This document explicitly recommends flat architecture. If it feels like enterprise software, you've gone too far. |
| **Blazor Server overhead for static content** | SignalR circuit is unnecessary weight for a page that doesn't interact | Acceptable trade-off — it's the mandated stack, and the overhead is negligible for local single-user use |

### Trade-offs Accepted

1. **Blazor Server vs. static HTML:** The reference design is pure HTML/CSS/SVG — a static file would suffice. Blazor Server adds a runtime, SignalR connection, and process. We accept this because: (a) it's the mandated stack, (b) it enables data-driven rendering from `data.json`, and (c) future enhancements (interactivity, filtering) are trivial to add.

2. **No database:** We lose queryability and relational integrity. We gain simplicity, zero-config setup, and human-readable/editable data. For <100 items, this is the right trade-off.

3. **No testing investment:** For a tool this simple, the cost of writing and maintaining tests exceeds the cost of the bugs they'd catch. Visual inspection is the primary QA.

---

## 7. Open Questions

| # | Question | Who Decides | Impact |
|---|----------|-------------|--------|
| 1 | **How many milestone tracks (horizontal lines) should the timeline support?** The reference shows 3 (M1, M2, M3). Should this be dynamic from `data.json`? | Product Owner | Affects SVG layout calculations |
| 2 | **Should the heatmap rows be fixed (Shipped/In Progress/Carryover/Blockers) or configurable?** | Product Owner | Affects data model and CSS |
| 3 | **How many months should the heatmap display?** Reference shows 4 columns. Should this be configurable? | Product Owner | Affects grid layout |
| 4 | **Should the "current month" highlight column be auto-detected from system date or specified in `data.json`?** | Developer discretion | Minor implementation detail |
| 5 | **Is the C:/Pics/ReportingDashboardDesign.png design a modification of the HTML reference, or a completely different layout?** | Product Owner | Could significantly affect component structure if layout differs |
| 6 | **Should the dashboard support printing (Ctrl+P) in addition to screenshots?** | Product Owner | Would require print-specific CSS media queries |
| 7 | **Will multiple projects need separate dashboards, or is this always one project at a time?** | Product Owner | Affects whether we need project selection or multiple `data.json` files |

---

## 8. Implementation Recommendations

### Phasing

#### Phase 1: Skeleton & Layout (Day 1, Morning) — MVP

1. `dotnet new blazorserver -n ReportingDashboard.Web --framework net8.0`
2. Create solution: `dotnet new sln`, add projects
3. Strip default template (remove Counter, Weather, NavMenu, Sidebar)
4. Create `Dashboard.razor` as the single page at `/`
5. Set `body` to fixed 1920×1080
6. Implement `DashboardHeader` component with static text
7. Implement `HeatmapGrid` with CSS Grid matching reference layout
8. Verify visual match against reference at 1920×1080

#### Phase 2: Data-Driven Rendering (Day 1, Afternoon)

1. Define C# model records (`DashboardData`, `Milestone`, `StatusRow`, etc.)
2. Create `data.json` with fictional project data
3. Implement `DataService` to load and deserialize JSON
4. Wire data into components via `[Parameter]` cascading
5. Implement `HeatmapRow` and `HeatmapCell` with dynamic item rendering
6. Apply color theming via CSS classes matching reference

#### Phase 3: SVG Timeline (Day 2, Morning)

1. Implement `TimelineSection` with inline SVG
2. Build date-to-X-position calculation
3. Render month gridlines, track lines, "NOW" indicator
4. Render milestone markers (diamonds, circles) with labels
5. Implement milestone track labels (left sidebar of timeline area)

#### Phase 4: Polish & Screenshot Workflow (Day 2, Afternoon)

1. Fine-tune spacing, colors, typography against reference
2. Add CSS custom properties for easy color palette changes
3. Implement `FileSystemWatcher` for live JSON reload (optional)
4. Document screenshot capture workflow
5. Create example `data.json` for a second fictional project to validate flexibility

### Quick Wins

- **Fastest path to visual output:** Start with hardcoded HTML/CSS matching the reference exactly, then extract into components and data-bind afterward. This validates the layout before adding complexity.
- **`dotnet watch` for iteration:** CSS changes reflect instantly without full rebuild — critical for pixel-matching the reference design.
- **Browser DevTools device emulation:** Set to 1920×1080 for consistent preview regardless of monitor resolution.

### Prototyping Recommendations

- **SVG timeline:** Prototype the date-to-pixel math in a standalone `.razor` component with test data before integrating. The coordinate system is the trickiest part of the build.
- **CSS Grid heatmap:** Copy the reference HTML's CSS classes verbatim into a `.razor.css` file first, then adapt. Starting from proven CSS is faster than writing from scratch.

---

## Appendix: Recommended `data.json` Schema

```json
{
  "header": {
    "title": "Project Phoenix Release Roadmap",
    "subtitle": "Platform Engineering · Phoenix Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project",
    "currentMonth": "April 2026"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "nowDate": "2026-04-15",
    "tracks": [
      {
        "id": "m1",
        "label": "M1",
        "description": "Core API & Auth",
        "color": "#0078D4"
      },
      {
        "id": "m2",
        "label": "M2",
        "description": "Data Pipeline",
        "color": "#00897B"
      },
      {
        "id": "m3",
        "label": "M3",
        "description": "Dashboard UI",
        "color": "#546E7A"
      }
    ],
    "milestones": [
      {
        "trackId": "m1",
        "date": "2026-01-15",
        "label": "Jan 15",
        "type": "checkpoint",
        "description": "API Design Complete"
      },
      {
        "trackId": "m1",
        "date": "2026-03-20",
        "label": "Mar 20 PoC",
        "type": "poc",
        "description": "Auth PoC Demo"
      },
      {
        "trackId": "m1",
        "date": "2026-05-01",
        "label": "May Prod",
        "type": "production",
        "description": "Production Release"
      }
    ]
  },
  "heatmap": {
    "columns": ["January", "February", "March", "April"],
    "highlightColumnIndex": 3,
    "rows": [
      {
        "category": "Shipped",
        "colorTheme": "shipped",
        "cells": [
          { "items": ["API v1 endpoint", "Auth module"] },
          { "items": ["Data ingestion pipeline"] },
          { "items": ["Dashboard wireframes", "CI/CD pipeline"] },
          { "items": ["Search indexing", "Monitoring alerts"] }
        ]
      },
      {
        "category": "In Progress",
        "colorTheme": "progress",
        "cells": [
          { "items": ["Schema design"] },
          { "items": ["API v2 planning", "Load testing"] },
          { "items": ["Role-based access"] },
          { "items": ["Performance optimization", "E2E tests"] }
        ]
      },
      {
        "category": "Carryover",
        "colorTheme": "carryover",
        "cells": [
          { "items": [] },
          { "items": ["Legacy migration script"] },
          { "items": ["Legacy migration script"] },
          { "items": ["Documentation update"] }
        ]
      },
      {
        "category": "Blockers",
        "colorTheme": "blockers",
        "cells": [
          { "items": [] },
          { "items": [] },
          { "items": ["Vendor API access pending"] },
          { "items": ["Vendor API access pending", "Capacity approval"] }
        ]
      }
    ]
  }
}
```

---

## Appendix: NuGet Package Summary

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.App` (framework ref) | 8.0.x | Blazor Server runtime | Yes (implicit) |
| `System.Text.Json` | 8.0.x | JSON deserialization | Yes (built-in) |
| `bUnit` | 1.25+ | Component testing | Optional |
| `xUnit` | 2.7+ | Unit test framework | Optional |
| `Microsoft.Playwright` | 1.40+ | Automated screenshot capture | Optional |

**Total additional NuGet packages required: 0.** Everything needed ships with the .NET 8 SDK.

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/2551ebb5e180ecedca0b79409dbea9ee6d309873/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
