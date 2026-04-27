# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-27 22:54 UTC_

### Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, progress status, and delivery health. The dashboard will be screenshot-friendly (1920×1080), load data from a local JSON configuration file, and require zero authentication or cloud dependencies. The design follows a proven layout: a timeline/Gantt header with milestone diamonds, and a color-coded heatmap grid showing Shipped/In Progress/Carryover/Blockers by month. **Primary Recommendation:** Build this as a single Blazor Server project with no database, no authentication, and no external service dependencies. Use a `dashboard-data.json` file in the `wwwroot/data/` folder as the data source. Render the timeline SVG inline using Blazor components (no charting library needed — the design is simple enough to hand-craft with SVG markup). Use pure CSS Grid and Flexbox for layout, matching the reference design's approach exactly. The entire solution should be one `.sln` with one project, deployable via `dotnet run`. ---

### Key Findings

- The reference HTML design is entirely CSS Grid + Flexbox + inline SVG — no JavaScript frameworks or charting libraries are used. This means **Blazor Server can replicate it 1:1** with `.razor` components and scoped CSS, with no JS interop needed.
- **JSON is the optimal data format** for the configuration file: it's natively supported in .NET 8 via `System.Text.Json`, human-editable, and maps cleanly to C# POCOs with minimal boilerplate.
- The design targets a **fixed 1920×1080 viewport** for screenshot capture. This eliminates the need for responsive design breakpoints — the layout should be pixel-precise at that resolution.
- **No charting library is necessary.** The timeline uses simple SVG primitives (lines, circles, polygons, text). Hand-crafted SVG in Razor components gives full control over positioning and styling, exactly matching the reference.
- The heatmap grid is a straightforward CSS Grid with 5 columns (`160px repeat(4, 1fr)`) and color-coded rows. This is trivially implementable with Blazor `@foreach` loops over data-bound collections.
- **Blazor Server's SignalR dependency is irrelevant** for this use case — the app runs locally, serves one user, and has no real-time collaboration requirements. It simply provides the simplest Blazor hosting model with no WASM download overhead.
- The color palette, typography (Segoe UI), and spacing values are fully specified in the reference HTML — no design system or UI component library is needed.
- **File watching with `IFileSystemWatcher` or `PhysicalFileProvider`** can enable live-reload of the JSON data file without restarting the app, which is useful during data entry.
- Existing open-source competitors (Meziantou's Blazor components, MudBlazor dashboards) are overkill for this use case. A from-scratch approach with zero dependencies is faster and more maintainable.
- The total codebase should be **under 15 files** and buildable/runnable in under 30 seconds. --- **Goal:** Render the complete dashboard from a JSON file, matching the reference design.
- **Scaffold the project**: `dotnet new blazorserver -n ReportingDashboard -f net8.0`
- **Define data models**: Create C# records matching the JSON schema (`DashboardData`, `ProjectInfo`, `Milestone`, `HeatmapCategory`, `HeatmapItem`)
- **Create `DashboardDataService`**: Load and deserialize `dashboard-data.json` at startup
- **Build the `Header` component**: Title, subtitle, legend icons (match reference exactly)
- **Build the `Heatmap` component**: CSS Grid with 4 rows × N month columns, color-coded cells with bullet-pointed items
- **Build the `Timeline` component**: Inline SVG with month gridlines, track lines, milestone markers, NOW line
- **Create `dashboard-data.sample.json`**: Fictional project with realistic data across all 4 categories
- **Test in browser**: Verify pixel-accuracy against the reference screenshot at 1920×1080
- **Add file-watch reload**: Use `PhysicalFileProvider` so JSON edits appear without restarting the app
- **Add error handling**: Graceful display when JSON is malformed or missing (show a friendly error message, not a crash)
- **Add CSS custom properties**: Extract all colors to `:root` variables for easy theming
- **Write basic tests**: bUnit tests for component rendering, xUnit tests for JSON deserialization edge cases
- **Add `launchSettings.json`**: Configure default browser URL and port
- **Write README.md**: Setup instructions, JSON schema documentation, screenshot workflow
- **Copy-paste the reference CSS directly** into scoped `.razor.css` files. The reference HTML's styles are already production-quality and pixel-precise. Don't reinvent them.
- **Use `@foreach` with index tracking** to highlight the "current month" column (the reference uses `.apr` CSS class for this). In Blazor, compute this from the `currentMonthIndex` JSON field.
- **Set the browser viewport via CSS** (`body { width: 1920px; height: 1080px; overflow: hidden; }`) so screenshots are always consistent regardless of actual browser window size.
- ❌ No authentication system
- ❌ No database
- ❌ No admin/editor UI (edit JSON directly)
- ❌ No responsive design (fixed 1920×1080)
- ❌ No dark mode
- ❌ No print stylesheet
- ❌ No API endpoints
- ❌ No logging/telemetry infrastructure
- ❌ No Docker container
- ❌ No CI/CD pipeline (single developer, local-only)
```xml
<!-- ReportingDashboard.csproj -->
<ItemGroup>
  <!-- No additional NuGet packages required for the app itself -->
  <!-- .NET 8 Blazor Server includes everything needed -->
</ItemGroup>

<!-- Test project (if created) -->
<ItemGroup>
  <PackageReference Include="bunit" Version="1.25.3" />
  <PackageReference Include="xunit" Version="2.7.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
</ItemGroup>
``` **The zero-dependency app project is a feature, not a limitation.** Every NuGet package is a maintenance burden. For an app this simple, the .NET 8 SDK provides everything needed.

### Recommended Tools & Technologies

- | Component | Choice | Version | Rationale | |-----------|--------|---------|-----------| | **UI Framework** | Blazor Server | .NET 8.0 (LTS) | Mandatory stack. Server-side rendering, no WASM payload, instant startup. | | **CSS Layout** | Pure CSS Grid + Flexbox | CSS3 | Matches reference design exactly. No CSS framework needed. | | **SVG Rendering** | Inline SVG in Razor components | N/A | Timeline milestones, diamonds, and date markers. Full design control. | | **CSS Approach** | Blazor CSS Isolation (scoped `.razor.css`) | Built-in .NET 8 | Per-component styles, no naming collisions, clean separation. | | **Fonts** | Segoe UI (system font) | N/A | Pre-installed on Windows. No web font loading needed. | | **Icons** | None (CSS shapes only) | N/A | Reference uses CSS `transform: rotate(45deg)` squares for diamonds, `border-radius: 50%` for dots. |
- **MudBlazor / Radzen / Syncfusion**: Overkill. These add 500KB+ of CSS/JS, introduce upgrade maintenance, and fight against the pixel-precise reference design. The design is simple enough that raw HTML/CSS in Razor is faster to build and easier to maintain.
- **Chart.js via JS Interop**: Unnecessary. The timeline is 3 horizontal lines with positioned markers — hand-coded SVG is simpler and avoids JS interop complexity.
- **Tailwind CSS**: Adds build tooling (PostCSS pipeline) for no benefit. The reference design has ~50 CSS rules total. | Component | Choice | Version | Rationale | |-----------|--------|---------|-----------| | **Data Format** | JSON file | `System.Text.Json` (built-in .NET 8) | Zero dependencies. Human-editable. Native deserialization. | | **Data File** | `wwwroot/data/dashboard-data.json` | N/A | Convention: `wwwroot` for static assets, `data/` subdirectory for configuration. | | **Data Loading** | `IConfiguration` + custom `DashboardDataService` | Built-in DI | Singleton service, loads JSON at startup, optional file-watch reload. | | **File Watching** | `PhysicalFileProvider` + `IChangeToken` | Built-in .NET 8 | Detects `dashboard-data.json` changes, reloads without restart. | | **POCO Models** | C# records with `System.Text.Json` attributes | Built-in | Immutable data models, clean serialization. |
- **YAML (YamlDotNet 16.x)**: More human-readable for some, but adds a NuGet dependency and .NET's JSON support is superior. JSON is universally understood.
- **SQLite (via EF Core)**: Massive overkill. This is read-only display of ~50 data points. A flat file is simpler to edit, version-control, and distribute.
- **TOML**: Poor .NET ecosystem support. No first-party library.
- **XML**: Verbose, harder to hand-edit, no advantage over JSON for this data shape.
```
wwwroot/
  data/
    dashboard-data.json          ← Active data file
    dashboard-data.sample.json   ← Shipped example with fictional project
``` **Recommended JSON schema** (top-level structure):
```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project",
    "currentMonth": "Apr 2026"
  },
  "milestones": [...],
  "timeline": { "startMonth": "Jan", "endMonth": "Jun", "tracks": [...] },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonthIndex": 3,
    "categories": {
      "shipped": [...],
      "inProgress": [...],
      "carryover": [...],
      "blockers": [...]
    }
  }
}
``` | Component | Choice | Version | Rationale | |-----------|--------|---------|-----------| | **Unit Testing** | xUnit | 2.7+ | .NET standard. Tests JSON deserialization, data models, service logic. | | **Blazor Component Testing** | bUnit | 1.25+ | Renders Razor components in-memory, asserts markup output. | | **Assertions** | FluentAssertions | 6.12+ | Readable test assertions. MIT licensed. | **Note:** Given the extreme simplicity of this app (read JSON → render HTML), testing should focus on:
- JSON deserialization correctness (malformed files, missing fields)
- Component rendering (bUnit snapshot tests for heatmap grid output)
- Edge cases (empty categories, zero milestones, long text truncation) | Component | Choice | Version | |-----------|--------|---------| | **SDK** | .NET 8 SDK | 8.0.x LTS | | **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Latest | | **Build** | `dotnet build` | Built-in | | **Run** | `dotnet run` (Kestrel) | Built-in | | **Package Manager** | NuGet | Built-in | ---
```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── Program.cs                     ← Minimal hosting setup
    ├── ReportingDashboard.csproj      ← Single project file
    ├── Models/
    │   ├── DashboardData.cs           ← Root POCO (C# record)
    │   ├── ProjectInfo.cs
    │   ├── Milestone.cs
    │   ├── TimelineTrack.cs
    │   └── HeatmapCategory.cs
    ├── Services/
    │   └── DashboardDataService.cs    ← Loads & caches JSON, file-watch reload
    ├── Components/
    │   ├── App.razor                  ← Root component
    │   ├── Pages/
    │   │   └── Dashboard.razor        ← Main (only) page
    │   ├── Layout/
    │   │   └── MainLayout.razor       ← Minimal shell (no nav, no sidebar)
    │   └── Shared/
    │       ├── Header.razor           ← Title, subtitle, legend
    │       ├── Timeline.razor         ← SVG timeline with milestones
    │       ├── Heatmap.razor          ← Grid container
    │       └── HeatmapRow.razor       ← Single status row (Shipped/InProgress/etc.)
    ├── wwwroot/
    │   ├── css/
    │   │   └── app.css                ← Global styles (body, reset, fonts)
    │   └── data/
    │       ├── dashboard-data.json
    │       └── dashboard-data.sample.json
    └── Properties/
        └── launchSettings.json
``` This project is too small for CQRS, Mediator, Repository, or Clean Architecture layers. The recommended pattern is:
- **`DashboardDataService`** (registered as Singleton): Reads `dashboard-data.json` at startup, deserializes to `DashboardData` record, exposes `DashboardData GetData()`. Optionally watches the file for changes using `PhysicalFileProvider`.
- **Component Tree**: `Dashboard.razor` injects the service, passes data downward via `[Parameter]` cascading. Each sub-component (`Header`, `Timeline`, `Heatmap`, `HeatmapRow`) is a pure rendering component with no logic beyond display.
- **No API Layer**: Components consume the service directly. There are no HTTP endpoints, no controllers, no middleware beyond the default Blazor pipeline.
```
dashboard-data.json
       │
       ▼
DashboardDataService (Singleton, DI-injected)
       │
       ▼
Dashboard.razor (page)
       │
   ┌───┼───────────┐
   ▼   ▼           ▼
Header Timeline  Heatmap
                    │
              ┌─────┼─────┬─────┐
              ▼     ▼     ▼     ▼
         HeatmapRow (×4: Shipped, InProgress, Carryover, Blockers)
``` The reference design's timeline is an SVG element at `1560×185` pixels containing:
- Vertical month gridlines at equal intervals
- 3 horizontal "track" lines (one per milestone stream), color-coded
- Positioned markers: circles (checkpoints), diamonds (PoC/Production milestones)
- A dashed red "NOW" vertical line **Recommended approach:** Build a `Timeline.razor` component that:
- Accepts `TimelineData` as a parameter (month range, tracks, markers)
- Calculates X positions proportionally: `xPos = (dayOfYear - startDay) / totalDays * svgWidth`
- Renders SVG primitives via Razor `@foreach` loops
- Uses CSS filter `drop-shadow` for diamond markers (matching the reference `<filter id="sh">`) This avoids any charting library dependency while giving pixel-perfect control.
- **Global `app.css`**: Reset (`* { margin:0; padding:0; box-sizing:border-box; }`), body styles (fixed 1920×1080, Segoe UI font), link colors.
- **Scoped `.razor.css` files**: Per-component styles. The reference CSS maps cleanly to components:
- `Header.razor.css`: `.hdr`, `.sub` classes
- `Timeline.razor.css`: `.tl-area`, `.tl-svg-box` classes
- `Heatmap.razor.css`: `.hm-wrap`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr` classes
- `HeatmapRow.razor.css`: `.hm-row-hdr`, `.hm-cell`, `.it` classes, color variants Define CSS custom properties in `app.css` for the 4 status categories:
```css
:root {
  --color-shipped: #34A853;
  --color-shipped-bg: #F0FBF0;
  --color-shipped-bg-current: #D8F2DA;
  --color-shipped-header-bg: #E8F5E9;

  --color-progress: #0078D4;
  --color-progress-bg: #EEF4FE;
  --color-progress-bg-current: #DAE8FB;
  --color-progress-header-bg: #E3F2FD;

  --color-carryover: #F4B400;
  --color-carryover-bg: #FFFDE7;
  --color-carryover-bg-current: #FFF0B0;
  --color-carryover-header-bg: #FFF8E1;

  --color-blockers: #EA4335;
  --color-blockers-bg: #FFF5F5;
  --color-blockers-bg-current: #FFE4E4;
  --color-blockers-header-bg: #FEF2F2;

  --color-now-line: #EA4335;
  --color-poc-diamond: #F4B400;
  --color-prod-diamond: #34A853;
}
``` This allows easy rebranding without touching component markup. ---

### Considerations & Risks

- **None.** This is explicitly out of scope per requirements. The app runs locally, serves one user (the person preparing the PowerPoint deck), and contains no sensitive data beyond project status that's already shared in executive meetings. If auth is ever needed in the future, the simplest addition would be `Microsoft.AspNetCore.Authentication.Negotiate` for Windows Integrated Auth (one line in `Program.cs`).
- The `dashboard-data.json` file is plain text. No encryption needed — the data is the same information that will appear in PowerPoint slides to executives.
- If the file contains ADO URLs or internal links, ensure the machine running the app is on the corporate network. No data leaves the machine.
- Add `dashboard-data.json` to `.gitignore` so real project data isn't committed. Ship only `dashboard-data.sample.json` in the repo. | Aspect | Recommendation | |--------|---------------| | **Runtime** | Local Kestrel (built into ASP.NET Core) | | **Port** | `https://localhost:5001` or `http://localhost:5000` | | **Deployment** | `dotnet run` from the project directory, or `dotnet publish -c Release` + run the executable | | **Distribution** | Self-contained publish: `dotnet publish -r win-x64 --self-contained -c Release` produces a single-folder deployment requiring no .NET SDK on the target machine | | **Screenshot Workflow** | User navigates to `http://localhost:5000`, takes browser screenshot at 1920×1080. Consider adding a `<meta name="viewport" content="width=1920">` to lock the viewport. | **$0.** The app runs on the developer's existing workstation. No cloud services, no databases, no external dependencies. --- | Risk | Likelihood | Impact | Mitigation | |------|-----------|--------|------------| | **SVG timeline positioning is off by a few pixels** | Medium | Low | Use the reference HTML's exact coordinate math. Write bUnit tests that assert SVG element positions. | | **JSON schema evolves and breaks deserialization** | Medium | Medium | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and nullable properties with defaults. Add a `"schemaVersion": 1` field to the JSON. | | **Blazor Server adds unnecessary complexity for a static page** | Low | Low | This is the mandated stack. The overhead is minimal — SignalR circuit stays open but consumes negligible resources for one local user. | | **Browser rendering differences affect screenshot quality** | Low | Medium | Target a single browser (Edge/Chrome). Set explicit `width: 1920px; height: 1080px` on `<body>`. Use browser DevTools device emulation for consistent screenshots. | | **Segoe UI font not available on non-Windows machines** | Low | Low | App is local-only on Windows. If Mac support is ever needed, add Arial/Helvetica as CSS fallback (already in the reference: `font-family: 'Segoe UI', Arial, sans-serif`). | | Decision | Trade-off | Rationale | |----------|-----------|-----------| | JSON file instead of SQLite | No query capability, no relational integrity | Data is tiny (~50 items), read-only, and hand-edited. Simplicity wins. | | No UI component library | Must hand-code all UI elements | The design is bespoke and pixel-precise. A library would fight the layout, not help it. | | No authentication | Anyone on the machine can access | Single-user local app. Security adds complexity with zero benefit. | | Fixed 1920×1080 layout | Not responsive on mobile/tablet | Explicitly designed for screenshot capture. Responsive design would compromise the pixel-perfect layout. | | Inline SVG instead of charting library | More manual positioning code | Full control over every pixel. The chart has ~20 elements total — a library's abstraction would add more code than it saves. |
- **Required skills**: Basic C#, Razor syntax, CSS Grid/Flexbox, SVG fundamentals
- **Learning curve**: Minimal for any .NET developer. Blazor Server is the simplest Blazor model. SVG basics can be learned in an hour.
- **Estimated development time**: 1-2 days for a competent .NET developer ---
- **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in the JSON, or always show a fixed window? **Recommendation:** Make it data-driven — the JSON defines which months appear, and the grid auto-sizes.
- **Should the "NOW" line position be auto-calculated or manually set?** The reference hardcodes it. **Recommendation:** Auto-calculate from `DateTime.Now` relative to the timeline's date range, but allow a `"nowOverride": "2026-04-15"` field in JSON for screenshot consistency.
- **How will the JSON file be edited?** Direct text editor? A separate admin page? **Recommendation:** Start with hand-editing in VS Code (JSON IntelliSense is excellent). If demand arises, add a simple Blazor form page later — but don't build it upfront.
- **Should the app support multiple projects (multiple JSON files)?** The reference shows one project. **Recommendation:** Start with one file. If multi-project is needed later, add a file picker or route parameter (`/dashboard/project-alpha`).
- **What's the target browser for screenshots?** **Recommendation:** Microsoft Edge (Chromium), which ships with Windows and renders identically to Chrome. Use `F12 → Device Emulation → 1920×1080` for consistent captures.
- **Should the sample data file reflect a real project structure or be purely fictional?** **Recommendation:** Use a realistic but fictional project (e.g., "Contoso Platform Modernization") so the sample is immediately useful as a template without exposing internal project names. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

## 1. Executive Summary

This project is a single-page executive reporting dashboard built with **C# .NET 8 and Blazor Server**, designed to visualize project milestones, progress status, and delivery health. The dashboard will be screenshot-friendly (1920×1080), load data from a local JSON configuration file, and require zero authentication or cloud dependencies. The design follows a proven layout: a timeline/Gantt header with milestone diamonds, and a color-coded heatmap grid showing Shipped/In Progress/Carryover/Blockers by month.

**Primary Recommendation:** Build this as a single Blazor Server project with no database, no authentication, and no external service dependencies. Use a `dashboard-data.json` file in the `wwwroot/data/` folder as the data source. Render the timeline SVG inline using Blazor components (no charting library needed — the design is simple enough to hand-craft with SVG markup). Use pure CSS Grid and Flexbox for layout, matching the reference design's approach exactly. The entire solution should be one `.sln` with one project, deployable via `dotnet run`.

---

## 2. Key Findings

- The reference HTML design is entirely CSS Grid + Flexbox + inline SVG — no JavaScript frameworks or charting libraries are used. This means **Blazor Server can replicate it 1:1** with `.razor` components and scoped CSS, with no JS interop needed.
- **JSON is the optimal data format** for the configuration file: it's natively supported in .NET 8 via `System.Text.Json`, human-editable, and maps cleanly to C# POCOs with minimal boilerplate.
- The design targets a **fixed 1920×1080 viewport** for screenshot capture. This eliminates the need for responsive design breakpoints — the layout should be pixel-precise at that resolution.
- **No charting library is necessary.** The timeline uses simple SVG primitives (lines, circles, polygons, text). Hand-crafted SVG in Razor components gives full control over positioning and styling, exactly matching the reference.
- The heatmap grid is a straightforward CSS Grid with 5 columns (`160px repeat(4, 1fr)`) and color-coded rows. This is trivially implementable with Blazor `@foreach` loops over data-bound collections.
- **Blazor Server's SignalR dependency is irrelevant** for this use case — the app runs locally, serves one user, and has no real-time collaboration requirements. It simply provides the simplest Blazor hosting model with no WASM download overhead.
- The color palette, typography (Segoe UI), and spacing values are fully specified in the reference HTML — no design system or UI component library is needed.
- **File watching with `IFileSystemWatcher` or `PhysicalFileProvider`** can enable live-reload of the JSON data file without restarting the app, which is useful during data entry.
- Existing open-source competitors (Meziantou's Blazor components, MudBlazor dashboards) are overkill for this use case. A from-scratch approach with zero dependencies is faster and more maintainable.
- The total codebase should be **under 15 files** and buildable/runnable in under 30 seconds.

---

## 3. Recommended Technology Stack

### Frontend (UI Layer)

| Component | Choice | Version | Rationale |
|-----------|--------|---------|-----------|
| **UI Framework** | Blazor Server | .NET 8.0 (LTS) | Mandatory stack. Server-side rendering, no WASM payload, instant startup. |
| **CSS Layout** | Pure CSS Grid + Flexbox | CSS3 | Matches reference design exactly. No CSS framework needed. |
| **SVG Rendering** | Inline SVG in Razor components | N/A | Timeline milestones, diamonds, and date markers. Full design control. |
| **CSS Approach** | Blazor CSS Isolation (scoped `.razor.css`) | Built-in .NET 8 | Per-component styles, no naming collisions, clean separation. |
| **Fonts** | Segoe UI (system font) | N/A | Pre-installed on Windows. No web font loading needed. |
| **Icons** | None (CSS shapes only) | N/A | Reference uses CSS `transform: rotate(45deg)` squares for diamonds, `border-radius: 50%` for dots. |

**Alternatives Considered and Rejected:**
- **MudBlazor / Radzen / Syncfusion**: Overkill. These add 500KB+ of CSS/JS, introduce upgrade maintenance, and fight against the pixel-precise reference design. The design is simple enough that raw HTML/CSS in Razor is faster to build and easier to maintain.
- **Chart.js via JS Interop**: Unnecessary. The timeline is 3 horizontal lines with positioned markers — hand-coded SVG is simpler and avoids JS interop complexity.
- **Tailwind CSS**: Adds build tooling (PostCSS pipeline) for no benefit. The reference design has ~50 CSS rules total.

### Backend (Data Layer)

| Component | Choice | Version | Rationale |
|-----------|--------|---------|-----------|
| **Data Format** | JSON file | `System.Text.Json` (built-in .NET 8) | Zero dependencies. Human-editable. Native deserialization. |
| **Data File** | `wwwroot/data/dashboard-data.json` | N/A | Convention: `wwwroot` for static assets, `data/` subdirectory for configuration. |
| **Data Loading** | `IConfiguration` + custom `DashboardDataService` | Built-in DI | Singleton service, loads JSON at startup, optional file-watch reload. |
| **File Watching** | `PhysicalFileProvider` + `IChangeToken` | Built-in .NET 8 | Detects `dashboard-data.json` changes, reloads without restart. |
| **POCO Models** | C# records with `System.Text.Json` attributes | Built-in | Immutable data models, clean serialization. |

**Alternatives Considered and Rejected:**
- **YAML (YamlDotNet 16.x)**: More human-readable for some, but adds a NuGet dependency and .NET's JSON support is superior. JSON is universally understood.
- **SQLite (via EF Core)**: Massive overkill. This is read-only display of ~50 data points. A flat file is simpler to edit, version-control, and distribute.
- **TOML**: Poor .NET ecosystem support. No first-party library.
- **XML**: Verbose, harder to hand-edit, no advantage over JSON for this data shape.

### Data File Naming Convention

```
wwwroot/
  data/
    dashboard-data.json          ← Active data file
    dashboard-data.sample.json   ← Shipped example with fictional project
```

**Recommended JSON schema** (top-level structure):

```json
{
  "project": {
    "title": "Privacy Automation Release Roadmap",
    "subtitle": "Trusted Platform · Privacy Automation Workstream · April 2026",
    "backlogUrl": "https://dev.azure.com/org/project",
    "currentMonth": "Apr 2026"
  },
  "milestones": [...],
  "timeline": { "startMonth": "Jan", "endMonth": "Jun", "tracks": [...] },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "currentMonthIndex": 3,
    "categories": {
      "shipped": [...],
      "inProgress": [...],
      "carryover": [...],
      "blockers": [...]
    }
  }
}
```

### Testing

| Component | Choice | Version | Rationale |
|-----------|--------|---------|-----------|
| **Unit Testing** | xUnit | 2.7+ | .NET standard. Tests JSON deserialization, data models, service logic. |
| **Blazor Component Testing** | bUnit | 1.25+ | Renders Razor components in-memory, asserts markup output. |
| **Assertions** | FluentAssertions | 6.12+ | Readable test assertions. MIT licensed. |

**Note:** Given the extreme simplicity of this app (read JSON → render HTML), testing should focus on:
1. JSON deserialization correctness (malformed files, missing fields)
2. Component rendering (bUnit snapshot tests for heatmap grid output)
3. Edge cases (empty categories, zero milestones, long text truncation)

### Build & Tooling

| Component | Choice | Version |
|-----------|--------|---------|
| **SDK** | .NET 8 SDK | 8.0.x LTS |
| **IDE** | Visual Studio 2022 / VS Code + C# Dev Kit | Latest |
| **Build** | `dotnet build` | Built-in |
| **Run** | `dotnet run` (Kestrel) | Built-in |
| **Package Manager** | NuGet | Built-in |

---

## 4. Architecture Recommendations

### Overall Architecture: Single-Project Monolith

```
ReportingDashboard.sln
└── ReportingDashboard/
    ├── Program.cs                     ← Minimal hosting setup
    ├── ReportingDashboard.csproj      ← Single project file
    ├── Models/
    │   ├── DashboardData.cs           ← Root POCO (C# record)
    │   ├── ProjectInfo.cs
    │   ├── Milestone.cs
    │   ├── TimelineTrack.cs
    │   └── HeatmapCategory.cs
    ├── Services/
    │   └── DashboardDataService.cs    ← Loads & caches JSON, file-watch reload
    ├── Components/
    │   ├── App.razor                  ← Root component
    │   ├── Pages/
    │   │   └── Dashboard.razor        ← Main (only) page
    │   ├── Layout/
    │   │   └── MainLayout.razor       ← Minimal shell (no nav, no sidebar)
    │   └── Shared/
    │       ├── Header.razor           ← Title, subtitle, legend
    │       ├── Timeline.razor         ← SVG timeline with milestones
    │       ├── Heatmap.razor          ← Grid container
    │       └── HeatmapRow.razor       ← Single status row (Shipped/InProgress/etc.)
    ├── wwwroot/
    │   ├── css/
    │   │   └── app.css                ← Global styles (body, reset, fonts)
    │   └── data/
    │       ├── dashboard-data.json
    │       └── dashboard-data.sample.json
    └── Properties/
        └── launchSettings.json
```

### Design Pattern: Simple Service + Component Tree

This project is too small for CQRS, Mediator, Repository, or Clean Architecture layers. The recommended pattern is:

1. **`DashboardDataService`** (registered as Singleton): Reads `dashboard-data.json` at startup, deserializes to `DashboardData` record, exposes `DashboardData GetData()`. Optionally watches the file for changes using `PhysicalFileProvider`.

2. **Component Tree**: `Dashboard.razor` injects the service, passes data downward via `[Parameter]` cascading. Each sub-component (`Header`, `Timeline`, `Heatmap`, `HeatmapRow`) is a pure rendering component with no logic beyond display.

3. **No API Layer**: Components consume the service directly. There are no HTTP endpoints, no controllers, no middleware beyond the default Blazor pipeline.

### Data Flow

```
dashboard-data.json
       │
       ▼
DashboardDataService (Singleton, DI-injected)
       │
       ▼
Dashboard.razor (page)
       │
   ┌───┼───────────┐
   ▼   ▼           ▼
Header Timeline  Heatmap
                    │
              ┌─────┼─────┬─────┐
              ▼     ▼     ▼     ▼
         HeatmapRow (×4: Shipped, InProgress, Carryover, Blockers)
```

### SVG Timeline Strategy

The reference design's timeline is an SVG element at `1560×185` pixels containing:
- Vertical month gridlines at equal intervals
- 3 horizontal "track" lines (one per milestone stream), color-coded
- Positioned markers: circles (checkpoints), diamonds (PoC/Production milestones)
- A dashed red "NOW" vertical line

**Recommended approach:** Build a `Timeline.razor` component that:
1. Accepts `TimelineData` as a parameter (month range, tracks, markers)
2. Calculates X positions proportionally: `xPos = (dayOfYear - startDay) / totalDays * svgWidth`
3. Renders SVG primitives via Razor `@foreach` loops
4. Uses CSS filter `drop-shadow` for diamond markers (matching the reference `<filter id="sh">`)

This avoids any charting library dependency while giving pixel-perfect control.

### CSS Strategy

- **Global `app.css`**: Reset (`* { margin:0; padding:0; box-sizing:border-box; }`), body styles (fixed 1920×1080, Segoe UI font), link colors.
- **Scoped `.razor.css` files**: Per-component styles. The reference CSS maps cleanly to components:
  - `Header.razor.css`: `.hdr`, `.sub` classes
  - `Timeline.razor.css`: `.tl-area`, `.tl-svg-box` classes
  - `Heatmap.razor.css`: `.hm-wrap`, `.hm-grid`, `.hm-corner`, `.hm-col-hdr` classes
  - `HeatmapRow.razor.css`: `.hm-row-hdr`, `.hm-cell`, `.it` classes, color variants

### Color Theming

Define CSS custom properties in `app.css` for the 4 status categories:

```css
:root {
  --color-shipped: #34A853;
  --color-shipped-bg: #F0FBF0;
  --color-shipped-bg-current: #D8F2DA;
  --color-shipped-header-bg: #E8F5E9;

  --color-progress: #0078D4;
  --color-progress-bg: #EEF4FE;
  --color-progress-bg-current: #DAE8FB;
  --color-progress-header-bg: #E3F2FD;

  --color-carryover: #F4B400;
  --color-carryover-bg: #FFFDE7;
  --color-carryover-bg-current: #FFF0B0;
  --color-carryover-header-bg: #FFF8E1;

  --color-blockers: #EA4335;
  --color-blockers-bg: #FFF5F5;
  --color-blockers-bg-current: #FFE4E4;
  --color-blockers-header-bg: #FEF2F2;

  --color-now-line: #EA4335;
  --color-poc-diamond: #F4B400;
  --color-prod-diamond: #34A853;
}
```

This allows easy rebranding without touching component markup.

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** This is explicitly out of scope per requirements. The app runs locally, serves one user (the person preparing the PowerPoint deck), and contains no sensitive data beyond project status that's already shared in executive meetings.

If auth is ever needed in the future, the simplest addition would be `Microsoft.AspNetCore.Authentication.Negotiate` for Windows Integrated Auth (one line in `Program.cs`).

### Data Protection

- The `dashboard-data.json` file is plain text. No encryption needed — the data is the same information that will appear in PowerPoint slides to executives.
- If the file contains ADO URLs or internal links, ensure the machine running the app is on the corporate network. No data leaves the machine.
- Add `dashboard-data.json` to `.gitignore` so real project data isn't committed. Ship only `dashboard-data.sample.json` in the repo.

### Hosting & Deployment

| Aspect | Recommendation |
|--------|---------------|
| **Runtime** | Local Kestrel (built into ASP.NET Core) |
| **Port** | `https://localhost:5001` or `http://localhost:5000` |
| **Deployment** | `dotnet run` from the project directory, or `dotnet publish -c Release` + run the executable |
| **Distribution** | Self-contained publish: `dotnet publish -r win-x64 --self-contained -c Release` produces a single-folder deployment requiring no .NET SDK on the target machine |
| **Screenshot Workflow** | User navigates to `http://localhost:5000`, takes browser screenshot at 1920×1080. Consider adding a `<meta name="viewport" content="width=1920">` to lock the viewport. |

### Infrastructure Costs

**$0.** The app runs on the developer's existing workstation. No cloud services, no databases, no external dependencies.

---

## 6. Risks & Trade-offs

### Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| **SVG timeline positioning is off by a few pixels** | Medium | Low | Use the reference HTML's exact coordinate math. Write bUnit tests that assert SVG element positions. |
| **JSON schema evolves and breaks deserialization** | Medium | Medium | Use `JsonSerializerOptions { PropertyNameCaseInsensitive = true }` and nullable properties with defaults. Add a `"schemaVersion": 1` field to the JSON. |
| **Blazor Server adds unnecessary complexity for a static page** | Low | Low | This is the mandated stack. The overhead is minimal — SignalR circuit stays open but consumes negligible resources for one local user. |
| **Browser rendering differences affect screenshot quality** | Low | Medium | Target a single browser (Edge/Chrome). Set explicit `width: 1920px; height: 1080px` on `<body>`. Use browser DevTools device emulation for consistent screenshots. |
| **Segoe UI font not available on non-Windows machines** | Low | Low | App is local-only on Windows. If Mac support is ever needed, add Arial/Helvetica as CSS fallback (already in the reference: `font-family: 'Segoe UI', Arial, sans-serif`). |

### Trade-offs Made

| Decision | Trade-off | Rationale |
|----------|-----------|-----------|
| JSON file instead of SQLite | No query capability, no relational integrity | Data is tiny (~50 items), read-only, and hand-edited. Simplicity wins. |
| No UI component library | Must hand-code all UI elements | The design is bespoke and pixel-precise. A library would fight the layout, not help it. |
| No authentication | Anyone on the machine can access | Single-user local app. Security adds complexity with zero benefit. |
| Fixed 1920×1080 layout | Not responsive on mobile/tablet | Explicitly designed for screenshot capture. Responsive design would compromise the pixel-perfect layout. |
| Inline SVG instead of charting library | More manual positioning code | Full control over every pixel. The chart has ~20 elements total — a library's abstraction would add more code than it saves. |

### Skills Assessment

- **Required skills**: Basic C#, Razor syntax, CSS Grid/Flexbox, SVG fundamentals
- **Learning curve**: Minimal for any .NET developer. Blazor Server is the simplest Blazor model. SVG basics can be learned in an hour.
- **Estimated development time**: 1-2 days for a competent .NET developer

---

## 7. Open Questions

1. **How many months should the heatmap display?** The reference shows 4 months (Jan–Apr). Should this be configurable in the JSON, or always show a fixed window? **Recommendation:** Make it data-driven — the JSON defines which months appear, and the grid auto-sizes.

2. **Should the "NOW" line position be auto-calculated or manually set?** The reference hardcodes it. **Recommendation:** Auto-calculate from `DateTime.Now` relative to the timeline's date range, but allow a `"nowOverride": "2026-04-15"` field in JSON for screenshot consistency.

3. **How will the JSON file be edited?** Direct text editor? A separate admin page? **Recommendation:** Start with hand-editing in VS Code (JSON IntelliSense is excellent). If demand arises, add a simple Blazor form page later — but don't build it upfront.

4. **Should the app support multiple projects (multiple JSON files)?** The reference shows one project. **Recommendation:** Start with one file. If multi-project is needed later, add a file picker or route parameter (`/dashboard/project-alpha`).

5. **What's the target browser for screenshots?** **Recommendation:** Microsoft Edge (Chromium), which ships with Windows and renders identically to Chrome. Use `F12 → Device Emulation → 1920×1080` for consistent captures.

6. **Should the sample data file reflect a real project structure or be purely fictional?** **Recommendation:** Use a realistic but fictional project (e.g., "Contoso Platform Modernization") so the sample is immediately useful as a template without exposing internal project names.

---

## 8. Implementation Recommendations

### Phase 1: Core MVP (Day 1)

**Goal:** Render the complete dashboard from a JSON file, matching the reference design.

1. **Scaffold the project**: `dotnet new blazorserver -n ReportingDashboard -f net8.0`
2. **Define data models**: Create C# records matching the JSON schema (`DashboardData`, `ProjectInfo`, `Milestone`, `HeatmapCategory`, `HeatmapItem`)
3. **Create `DashboardDataService`**: Load and deserialize `dashboard-data.json` at startup
4. **Build the `Header` component**: Title, subtitle, legend icons (match reference exactly)
5. **Build the `Heatmap` component**: CSS Grid with 4 rows × N month columns, color-coded cells with bullet-pointed items
6. **Build the `Timeline` component**: Inline SVG with month gridlines, track lines, milestone markers, NOW line
7. **Create `dashboard-data.sample.json`**: Fictional project with realistic data across all 4 categories
8. **Test in browser**: Verify pixel-accuracy against the reference screenshot at 1920×1080

### Phase 2: Polish & Usability (Day 2)

1. **Add file-watch reload**: Use `PhysicalFileProvider` so JSON edits appear without restarting the app
2. **Add error handling**: Graceful display when JSON is malformed or missing (show a friendly error message, not a crash)
3. **Add CSS custom properties**: Extract all colors to `:root` variables for easy theming
4. **Write basic tests**: bUnit tests for component rendering, xUnit tests for JSON deserialization edge cases
5. **Add `launchSettings.json`**: Configure default browser URL and port
6. **Write README.md**: Setup instructions, JSON schema documentation, screenshot workflow

### Quick Wins

- **Copy-paste the reference CSS directly** into scoped `.razor.css` files. The reference HTML's styles are already production-quality and pixel-precise. Don't reinvent them.
- **Use `@foreach` with index tracking** to highlight the "current month" column (the reference uses `.apr` CSS class for this). In Blazor, compute this from the `currentMonthIndex` JSON field.
- **Set the browser viewport via CSS** (`body { width: 1920px; height: 1080px; overflow: hidden; }`) so screenshots are always consistent regardless of actual browser window size.

### What NOT to Build

- ❌ No authentication system
- ❌ No database
- ❌ No admin/editor UI (edit JSON directly)
- ❌ No responsive design (fixed 1920×1080)
- ❌ No dark mode
- ❌ No print stylesheet
- ❌ No API endpoints
- ❌ No logging/telemetry infrastructure
- ❌ No Docker container
- ❌ No CI/CD pipeline (single developer, local-only)

### NuGet Packages (Complete List)

```xml
<!-- ReportingDashboard.csproj -->
<ItemGroup>
  <!-- No additional NuGet packages required for the app itself -->
  <!-- .NET 8 Blazor Server includes everything needed -->
</ItemGroup>

<!-- Test project (if created) -->
<ItemGroup>
  <PackageReference Include="bunit" Version="1.25.3" />
  <PackageReference Include="xunit" Version="2.7.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
</ItemGroup>
```

**The zero-dependency app project is a feature, not a limitation.** Every NuGet package is a maintenance burden. For an app this simple, the .NET 8 SDK provides everything needed.

## Visual Design References

The following design reference files were found in the repository. These MUST be used as the canonical visual specification when building UI components.

### `docs/design-screenshots/OriginalDesignConcept.png`

**Type:** Design Image — engineers should reference this file visually

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/6d42ab598cfff4a5d1fd2f286405d20f4ba5c3a5/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
