# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-16 21:54 UTC_

### Summary

This project is a **single-page executive reporting dashboard** that visualizes project milestones, progress status, and categorical heatmaps (Shipped, In Progress, Carryover, Blockers). The output is optimized for **1920×1080 screenshots** destined for PowerPoint decks. Given the simplicity mandate—no auth, no cloud, no enterprise infrastructure—the recommended approach is a **minimal Blazor Server application** that reads a `data.json` config file at startup and renders a pixel-perfect dashboard matching the HTML design reference. **Primary recommendation:** Use a single Blazor Server project with zero database dependencies, inline SVG rendering for the timeline, pure CSS Grid/Flexbox for the heatmap layout, and `System.Text.Json` for data loading. No charting libraries are needed—the design is achievable with raw SVG and CSS, which gives full pixel-level control required for screenshot fidelity. ---

### Key Findings

- The original design uses **CSS Grid** (`160px repeat(4,1fr)`) for the heatmap and **inline SVG** for the timeline—both are natively renderable in Blazor without any third-party libraries.
- The design targets a **fixed 1920×1080 viewport**, meaning responsive design is unnecessary. A fixed-width layout simplifies implementation dramatically.
- **No charting library is needed.** The timeline is a simple SVG with lines, circles, diamonds, and text. Hand-crafted SVG in Blazor markup gives exact control and avoids library overhead.
- The color palette is well-defined (16 colors) and maps directly to four status categories. A CSS custom properties approach enables easy theming if the dashboard is reused for other projects.
- **`System.Text.Json`** (built into .NET 8) is sufficient for deserializing `data.json`—no need for Newtonsoft.Json or any ORM.
- The design includes a "NOW" marker on the timeline that must be dynamically positioned based on the current date relative to the project's date range.
- Blazor Server's **SignalR circuit** is irrelevant here since this is a read-only, single-user, screenshot-oriented tool. The server-side rendering model is fine but adds no interactive benefit. Consider Blazor SSR (static server-side rendering in .NET 8) for even simpler deployment.
- The font **Segoe UI** is a Windows system font, so it will render correctly on the developer's machine. For cross-platform consistency, bundle or specify fallbacks. ---
- `dotnet new blazor -n ReportingDashboard -o src/ReportingDashboard`
- Create solution: `dotnet new sln` → `dotnet sln add src/ReportingDashboard`
- Port the reference HTML/CSS verbatim into `MainDashboard.razor` as static markup
- Verify pixel-perfect match at 1920×1080 in browser
- **Deliverable:** Static page that looks identical to the reference design with hardcoded data
- Define C# record types for `DashboardData`, `Milestone`, `HeatmapCategory`, etc.
- Create `data.json` with fictional project data (e.g., "Project Phoenix")
- Implement `DashboardDataService` to load and deserialize JSON at startup
- Replace hardcoded markup with `@foreach` loops over the data model
- **Deliverable:** Dashboard renders from `data.json`; changing the file changes the output
- Implement date-to-X coordinate mapping in `TimelineSvg.razor`
- Render milestone shapes (diamonds, circles) from data model
- Position "NOW" marker based on current date or config
- Draw track lanes with correct colors
- **Deliverable:** Timeline section fully data-driven
- Fine-tune spacing, colors, and typography to match reference exactly
- Add `FileSystemWatcher` for live `data.json` reload during development
- (Optional) Add Playwright script for automated 1920×1080 PNG capture
- Create `data.sample.json` with documentation comments
- Write README with setup and usage instructions
- **Deliverable:** Production-ready reporting tool
- **Copy-paste the reference CSS** — The design's CSS is well-structured and can be used almost verbatim. Don't over-engineer the styling.
- **Use `dotnet watch`** — Instant feedback loop for CSS and markup changes.
- **Playwright screenshot script** — A 10-line C# script that launches the app, navigates to `localhost:5000`, and saves a 1920×1080 PNG. This automates the PowerPoint workflow.
- **SVG date mapping** — The coordinate math for placing milestones on the timeline is the trickiest part. Prototype this in isolation before integrating into the full page.
- **CSS Grid heatmap** — Verify that Blazor's rendered HTML produces the same grid layout as the reference HTML. Test with 0, 1, 5, and 10+ items per cell to ensure overflow handling. ---
```
ReportingDashboard.sln
└── src/
    └── ReportingDashboard/
        ├── ReportingDashboard.csproj
        ├── Program.cs
        ├── Components/
        │   ├── App.razor
        │   ├── Routes.razor
        │   ├── Layout/
        │   │   └── MainLayout.razor
        │   └── Pages/
        │       └── Dashboard.razor
        ├── Components/Dashboard/
        │   ├── DashboardHeader.razor
        │   ├── TimelineSvg.razor
        │   ├── HeatmapGrid.razor
        │   ├── HeatmapRow.razor
        │   └── HeatmapCell.razor
        ├── Models/
        │   └── DashboardData.cs
        ├── Services/
        │   └── DashboardDataService.cs
        ├── wwwroot/
        │   ├── css/
        │   │   └── dashboard.css
        │   └── data.json
        ├── appsettings.json
        └── Properties/
            └── launchSettings.json
``` | Package | Version | Purpose | Required? | |---------|---------|---------|-----------| | `Microsoft.AspNetCore.Components.Web` | 8.0.x | Blazor Server (included in SDK) | Yes (implicit) | | `System.Text.Json` | 8.0.x | JSON deserialization (included in SDK) | Yes (implicit) | | `Microsoft.Playwright` | 1.41+ | Automated screenshot capture | Optional | | `bUnit` | 1.25+ | Component unit testing | Optional | | `xunit` | 2.7+ | Test framework | Optional |

### Recommended Tools & Technologies

- **Date:** April 16, 2026 **Stack:** C# .NET 8 · Blazor Server · Local Only · .sln Structure --- | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **UI Framework** | Blazor Server (or Blazor SSR) | .NET 8.0 | Built-in, no additional packages | | **CSS Layout** | CSS Grid + Flexbox | Native | Matches the design's `grid-template-columns: 160px repeat(4,1fr)` exactly | | **Timeline Rendering** | Inline SVG in Razor markup | N/A | No library needed; design uses simple shapes (lines, circles, diamonds) | | **Icons/Shapes** | SVG primitives | N/A | Diamond milestones via `<polygon>`, circles via `<circle>`, "NOW" line via `<line>` | | **CSS Isolation** | Blazor scoped CSS (`.razor.css`) | Built-in | Per-component styles prevent leakage | **No component library recommended.** MudBlazor, Radzen, and Syncfusion are overkill for this project. The design is a single fixed-layout page—raw HTML/CSS in Blazor gives the exact pixel control needed for screenshot fidelity. Adding a component library introduces theming conflicts, bundle size, and visual deviations from the reference design. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Source-generated serialization for performance (optional) | | **Data Storage** | `data.json` flat file | N/A | Read at startup via `IConfiguration` or direct file read | | **File Watching (optional)** | `FileSystemWatcher` | Built into .NET 8 | Auto-reload dashboard when `data.json` is edited | | **Configuration** | `appsettings.json` + `data.json` | Built-in | App settings for server config; `data.json` for dashboard content | **No database.** The project reads a single JSON file. SQLite, LiteDB, and EF Core are unnecessary complexity. | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **SDK** | .NET 8.0 SDK | 8.0.x (latest patch) | LTS release, supported through Nov 2026 | | **Project Type** | Blazor Web App (Server render mode) | `dotnet new blazor` | .NET 8 unified Blazor template | | **Solution Structure** | Single `.sln` with single `.csproj` | N/A | No need for multi-project; this is a single-page app | | **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Latest | Full Blazor tooling support | | Layer | Technology | Version | Notes | |-------|-----------|---------|-------| | **Unit Testing** | xUnit | 2.7+ | .NET ecosystem standard | | **Blazor Component Testing** | bUnit | 1.25+ | Test Razor components in isolation | | **Assertion Library** | FluentAssertions | 6.12+ | Readable test assertions | | **Screenshot Testing (optional)** | Playwright for .NET | 1.41+ | Automate 1920×1080 screenshot capture for PowerPoint | | Tool | Purpose | Notes | |------|---------|-------| | **Hot Reload** | `dotnet watch` | Built into .NET 8; instant CSS/markup changes | | **Browser DevTools** | Layout debugging | Chrome/Edge F12 for CSS Grid inspection | | **Playwright** | Automated screenshots | `await page.ScreenshotAsync()` at 1920×1080 for PowerPoint | --- This is not a CRUD app. It is a **data visualization page** that reads JSON and renders HTML. The architecture should reflect this simplicity.
```
data.json ──► DashboardDataService ──► MainDashboard.razor
                                          ├── Header.razor
                                          ├── TimelineSvg.razor
                                          └── HeatmapGrid.razor
                                                ├── HeatmapRow.razor (×4: Shipped, InProgress, Carryover, Blockers)
                                                └── HeatmapCell.razor
```
```csharp
public record DashboardData
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineTrack> Tracks { get; init; }
    public HeatmapData Heatmap { get; init; }
}

public record ProjectInfo
{
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string BacklogUrl { get; init; }
    public string CurrentMonth { get; init; }
}

public record Milestone
{
    public string Label { get; init; }
    public DateTime Date { get; init; }
    public string Type { get; init; }  // "poc", "production", "checkpoint"
}

public record HeatmapData
{
    public List<string> Columns { get; init; }  // Month names
    public string HighlightColumn { get; init; }  // Current month
    public HeatmapCategory Shipped { get; init; }
    public HeatmapCategory InProgress { get; init; }
    public HeatmapCategory Carryover { get; init; }
    public HeatmapCategory Blockers { get; init; }
}

public record HeatmapCategory
{
    public Dictionary<string, List<string>> Items { get; init; }  // Column → items
}
```
- **Startup:** `DashboardDataService` reads `data.json` from disk using `System.Text.Json`.
- **Registration:** Service registered as `Singleton` (data is static for the app's lifetime) or `Scoped` with `FileSystemWatcher` for live reload.
- **Rendering:** `MainDashboard.razor` injects the service, passes data to child components.
- **SVG Timeline:** `TimelineSvg.razor` computes X-positions by mapping dates to a 1560px-wide SVG coordinate space. The "NOW" line position is calculated from `DateTime.Today`.
- **Heatmap:** `HeatmapGrid.razor` iterates over categories and months, applying CSS classes (`ship-cell`, `prog-cell`, `carry-cell`, `block-cell`) per the design spec. Replicate the reference design's CSS almost verbatim. Key decisions:
- **Use a single `app.css`** rather than scoped CSS, since the design is one page with tightly coupled styles.
- **CSS custom properties** for the 16-color palette enable easy project-to-project customization:
  ```css
  :root {
      --color-shipped: #34A853;
      --color-shipped-bg: #F0FBF0;
      --color-shipped-highlight: #D8F2DA;
      --color-progress: #0078D4;
      --color-progress-bg: #EEF4FE;
      --color-progress-highlight: #DAE8FB;
      --color-carryover: #F4B400;
      --color-carryover-bg: #FFFDE7;
      --color-carryover-highlight: #FFF0B0;
      --color-blocker: #EA4335;
      --color-blocker-bg: #FFF5F5;
      --color-blocker-highlight: #FFE4E4;
  }
  ```
- **Fixed 1920×1080 body size** with `overflow: hidden` as in the reference. This is intentional for screenshot capture. The reference SVG is 1560×185px. Key implementation details:
- **Date-to-X mapping:** Given a date range (e.g., Jan–Jun), compute `x = (date - startDate) / (endDate - startDate) * 1560`.
- **Month grid lines:** Evenly spaced at 260px intervals (1560 / 6 months).
- **Milestone shapes:**
- Checkpoint: `<circle>` with white fill, colored stroke
- PoC: `<polygon>` diamond with `#F4B400` fill + drop shadow filter
- Production: `<polygon>` diamond with `#34A853` fill + drop shadow filter
- **"NOW" line:** Dashed red vertical line at computed X position, with "NOW" label.
- **Track lanes:** Horizontal colored lines at Y = 42, 98, 154 (3 tracks spaced ~56px apart).
```json
{
  "project": {
    "title": "Project Phoenix Release Roadmap",
    "subtitle": "Engineering Platform • Core Infrastructure • April 2026",
    "backlogUrl": "#",
    "currentMonth": "Apr"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "label": "API Gateway & Auth",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "May Prod" }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "highlightColumn": "Apr",
    "shipped": {
      "Jan": ["Auth service v2", "Rate limiter"],
      "Feb": ["Token refresh flow"],
      "Mar": ["OAuth2 PKCE", "Session mgmt"],
      "Apr": ["Gateway routing"]
    },
    "inProgress": { ... },
    "carryover": { ... },
    "blockers": { ... }
  }
}
``` ---

### Considerations & Risks

- **None.** Per requirements, this is a no-auth, local-only tool. No login, no RBAC, no tokens. If future needs arise, the simplest addition would be a single shared secret via `appsettings.json` checked by middleware—but this is explicitly out of scope.
- `data.json` sits on the local filesystem. No encryption needed for a local reporting tool.
- If the JSON contains sensitive project names, ensure the file is in `.gitignore` and provide a `data.sample.json` template in the repo.
- The dashboard renders read-only data. There are no write operations, no user input, and no injection surfaces. | Approach | Command | Notes | |----------|---------|-------| | **Development** | `dotnet watch --project src/Dashboard` | Hot reload, default port 5000/5001 | | **Local Production** | `dotnet run --configuration Release` | Kestrel serves directly, no IIS needed | | **Screenshot Automation** | Playwright script captures page at 1920×1080 | Optional: `dotnet run` + `playwright screenshot` in a script | **No cloud, no containers, no CDN.** The app runs on `localhost`. Kestrel (built into .NET 8) is the web server. No reverse proxy needed. **$0.** This runs on the developer's local machine. No servers, no subscriptions, no licenses. --- **Impact:** Low **Description:** Blazor Server maintains a SignalR WebSocket connection per client. For a screenshot tool used by one person, this is irrelevant—but it means the app uses more memory than a static HTML file. **Mitigation:** Consider using **Blazor Static SSR** (new in .NET 8) which renders HTML on the server without maintaining a circuit. This is ideal for a read-only dashboard. Set `@rendermode` to `null` (static) on the page. **Impact:** Medium **Description:** SVG text positioning and drop shadow filters can render differently in Chrome vs Edge vs Firefox. **Mitigation:** Standardize on **Edge/Chrome** for screenshot capture (both use Chromium). Document the target browser. Playwright uses Chromium by default. **Impact:** Low (Windows only) **Description:** Segoe UI is installed on Windows by default but not on macOS/Linux. **Mitigation:** Since the requirement is local-only and the user is on Windows, this is a non-issue. The CSS already includes Arial as a fallback. **Impact:** Medium **Description:** As the dashboard evolves, the JSON structure may change, breaking existing data files. **Mitigation:** Define a `$schema` property in `data.json` and validate on load. Use C# record types with `required` properties so deserialization fails fast on schema mismatches. **Accepted.** The design is fixed at 1920×1080 with specific colors, spacing, and layout. A component library would fight this specificity. Manual CSS matching the reference HTML is faster and more accurate. **Accepted.** The user edits `data.json` manually each month. If historical snapshots are needed later, the simplest approach is versioning `data.json` files by date (`data.2026-04.json`) and adding a month selector dropdown. ---
- **How many timeline tracks should be supported?** The reference shows 3 (M1, M2, M3). Should the data model cap at 3, or allow N tracks with dynamic SVG height?
- **Should the "NOW" marker auto-calculate from system date, or be specified in `data.json`?** Auto-calculation is simpler but means screenshots change daily. A fixed date in JSON gives reproducible screenshots.
- **Is the heatmap always 4 columns (months)?** The reference shows Jan–Apr. Should the grid support variable column counts (e.g., 6 months, 12 months)?
- **How will `data.json` be updated?** Manual text editing? A companion CLI tool? This affects whether we need JSON schema validation and error messages.
- **Should the app support multiple projects?** Currently scoped to one `data.json` = one project. If multiple projects are needed, a project selector or multiple JSON files would be the simplest extension.
- **What is the screenshot capture workflow?** Manual browser screenshot, or should the app include a "Download as PNG" button using Playwright or html2canvas?
- **Do the heatmap cells need tooltips or click-through links?** The reference design has no interactivity, but ADO backlog links could be useful for executives viewing the live page. ---

### Detailed Analysis

# Research: Technology Stack for Executive Reporting Dashboard

**Date:** April 16, 2026
**Stack:** C# .NET 8 · Blazor Server · Local Only · .sln Structure

---

## 1. Executive Summary

This project is a **single-page executive reporting dashboard** that visualizes project milestones, progress status, and categorical heatmaps (Shipped, In Progress, Carryover, Blockers). The output is optimized for **1920×1080 screenshots** destined for PowerPoint decks. Given the simplicity mandate—no auth, no cloud, no enterprise infrastructure—the recommended approach is a **minimal Blazor Server application** that reads a `data.json` config file at startup and renders a pixel-perfect dashboard matching the HTML design reference.

**Primary recommendation:** Use a single Blazor Server project with zero database dependencies, inline SVG rendering for the timeline, pure CSS Grid/Flexbox for the heatmap layout, and `System.Text.Json` for data loading. No charting libraries are needed—the design is achievable with raw SVG and CSS, which gives full pixel-level control required for screenshot fidelity.

---

## 2. Key Findings

- The original design uses **CSS Grid** (`160px repeat(4,1fr)`) for the heatmap and **inline SVG** for the timeline—both are natively renderable in Blazor without any third-party libraries.
- The design targets a **fixed 1920×1080 viewport**, meaning responsive design is unnecessary. A fixed-width layout simplifies implementation dramatically.
- **No charting library is needed.** The timeline is a simple SVG with lines, circles, diamonds, and text. Hand-crafted SVG in Blazor markup gives exact control and avoids library overhead.
- The color palette is well-defined (16 colors) and maps directly to four status categories. A CSS custom properties approach enables easy theming if the dashboard is reused for other projects.
- **`System.Text.Json`** (built into .NET 8) is sufficient for deserializing `data.json`—no need for Newtonsoft.Json or any ORM.
- The design includes a "NOW" marker on the timeline that must be dynamically positioned based on the current date relative to the project's date range.
- Blazor Server's **SignalR circuit** is irrelevant here since this is a read-only, single-user, screenshot-oriented tool. The server-side rendering model is fine but adds no interactive benefit. Consider Blazor SSR (static server-side rendering in .NET 8) for even simpler deployment.
- The font **Segoe UI** is a Windows system font, so it will render correctly on the developer's machine. For cross-platform consistency, bundle or specify fallbacks.

---

## 3. Recommended Technology Stack

### Frontend (Blazor Components + CSS)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **UI Framework** | Blazor Server (or Blazor SSR) | .NET 8.0 | Built-in, no additional packages |
| **CSS Layout** | CSS Grid + Flexbox | Native | Matches the design's `grid-template-columns: 160px repeat(4,1fr)` exactly |
| **Timeline Rendering** | Inline SVG in Razor markup | N/A | No library needed; design uses simple shapes (lines, circles, diamonds) |
| **Icons/Shapes** | SVG primitives | N/A | Diamond milestones via `<polygon>`, circles via `<circle>`, "NOW" line via `<line>` |
| **CSS Isolation** | Blazor scoped CSS (`.razor.css`) | Built-in | Per-component styles prevent leakage |

**No component library recommended.** MudBlazor, Radzen, and Syncfusion are overkill for this project. The design is a single fixed-layout page—raw HTML/CSS in Blazor gives the exact pixel control needed for screenshot fidelity. Adding a component library introduces theming conflicts, bundle size, and visual deviations from the reference design.

### Backend (Data Layer)

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **JSON Deserialization** | `System.Text.Json` | Built into .NET 8 | Source-generated serialization for performance (optional) |
| **Data Storage** | `data.json` flat file | N/A | Read at startup via `IConfiguration` or direct file read |
| **File Watching (optional)** | `FileSystemWatcher` | Built into .NET 8 | Auto-reload dashboard when `data.json` is edited |
| **Configuration** | `appsettings.json` + `data.json` | Built-in | App settings for server config; `data.json` for dashboard content |

**No database.** The project reads a single JSON file. SQLite, LiteDB, and EF Core are unnecessary complexity.

### Project Structure

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **SDK** | .NET 8.0 SDK | 8.0.x (latest patch) | LTS release, supported through Nov 2026 |
| **Project Type** | Blazor Web App (Server render mode) | `dotnet new blazor` | .NET 8 unified Blazor template |
| **Solution Structure** | Single `.sln` with single `.csproj` | N/A | No need for multi-project; this is a single-page app |
| **IDE** | Visual Studio 2022 17.8+ or VS Code + C# Dev Kit | Latest | Full Blazor tooling support |

### Testing

| Layer | Technology | Version | Notes |
|-------|-----------|---------|-------|
| **Unit Testing** | xUnit | 2.7+ | .NET ecosystem standard |
| **Blazor Component Testing** | bUnit | 1.25+ | Test Razor components in isolation |
| **Assertion Library** | FluentAssertions | 6.12+ | Readable test assertions |
| **Screenshot Testing (optional)** | Playwright for .NET | 1.41+ | Automate 1920×1080 screenshot capture for PowerPoint |

### Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| **Hot Reload** | `dotnet watch` | Built into .NET 8; instant CSS/markup changes |
| **Browser DevTools** | Layout debugging | Chrome/Edge F12 for CSS Grid inspection |
| **Playwright** | Automated screenshots | `await page.ScreenshotAsync()` at 1920×1080 for PowerPoint |

---

## 4. Architecture Recommendations

### Overall Pattern: **Single-Component Read-Only Dashboard**

This is not a CRUD app. It is a **data visualization page** that reads JSON and renders HTML. The architecture should reflect this simplicity.

```
data.json ──► DashboardDataService ──► MainDashboard.razor
                                          ├── Header.razor
                                          ├── TimelineSvg.razor
                                          └── HeatmapGrid.razor
                                                ├── HeatmapRow.razor (×4: Shipped, InProgress, Carryover, Blockers)
                                                └── HeatmapCell.razor
```

### Data Model (`DashboardData.cs`)

```csharp
public record DashboardData
{
    public ProjectInfo Project { get; init; }
    public List<Milestone> Milestones { get; init; }
    public List<TimelineTrack> Tracks { get; init; }
    public HeatmapData Heatmap { get; init; }
}

public record ProjectInfo
{
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string BacklogUrl { get; init; }
    public string CurrentMonth { get; init; }
}

public record Milestone
{
    public string Label { get; init; }
    public DateTime Date { get; init; }
    public string Type { get; init; }  // "poc", "production", "checkpoint"
}

public record HeatmapData
{
    public List<string> Columns { get; init; }  // Month names
    public string HighlightColumn { get; init; }  // Current month
    public HeatmapCategory Shipped { get; init; }
    public HeatmapCategory InProgress { get; init; }
    public HeatmapCategory Carryover { get; init; }
    public HeatmapCategory Blockers { get; init; }
}

public record HeatmapCategory
{
    public Dictionary<string, List<string>> Items { get; init; }  // Column → items
}
```

### Data Flow

1. **Startup:** `DashboardDataService` reads `data.json` from disk using `System.Text.Json`.
2. **Registration:** Service registered as `Singleton` (data is static for the app's lifetime) or `Scoped` with `FileSystemWatcher` for live reload.
3. **Rendering:** `MainDashboard.razor` injects the service, passes data to child components.
4. **SVG Timeline:** `TimelineSvg.razor` computes X-positions by mapping dates to a 1560px-wide SVG coordinate space. The "NOW" line position is calculated from `DateTime.Today`.
5. **Heatmap:** `HeatmapGrid.razor` iterates over categories and months, applying CSS classes (`ship-cell`, `prog-cell`, `carry-cell`, `block-cell`) per the design spec.

### CSS Architecture

Replicate the reference design's CSS almost verbatim. Key decisions:

- **Use a single `app.css`** rather than scoped CSS, since the design is one page with tightly coupled styles.
- **CSS custom properties** for the 16-color palette enable easy project-to-project customization:
  ```css
  :root {
      --color-shipped: #34A853;
      --color-shipped-bg: #F0FBF0;
      --color-shipped-highlight: #D8F2DA;
      --color-progress: #0078D4;
      --color-progress-bg: #EEF4FE;
      --color-progress-highlight: #DAE8FB;
      --color-carryover: #F4B400;
      --color-carryover-bg: #FFFDE7;
      --color-carryover-highlight: #FFF0B0;
      --color-blocker: #EA4335;
      --color-blocker-bg: #FFF5F5;
      --color-blocker-highlight: #FFE4E4;
  }
  ```
- **Fixed 1920×1080 body size** with `overflow: hidden` as in the reference. This is intentional for screenshot capture.

### SVG Timeline Rendering Strategy

The reference SVG is 1560×185px. Key implementation details:

- **Date-to-X mapping:** Given a date range (e.g., Jan–Jun), compute `x = (date - startDate) / (endDate - startDate) * 1560`.
- **Month grid lines:** Evenly spaced at 260px intervals (1560 / 6 months).
- **Milestone shapes:**
  - Checkpoint: `<circle>` with white fill, colored stroke
  - PoC: `<polygon>` diamond with `#F4B400` fill + drop shadow filter
  - Production: `<polygon>` diamond with `#34A853` fill + drop shadow filter
- **"NOW" line:** Dashed red vertical line at computed X position, with "NOW" label.
- **Track lanes:** Horizontal colored lines at Y = 42, 98, 154 (3 tracks spaced ~56px apart).

### Recommended `data.json` Structure

```json
{
  "project": {
    "title": "Project Phoenix Release Roadmap",
    "subtitle": "Engineering Platform • Core Infrastructure • April 2026",
    "backlogUrl": "#",
    "currentMonth": "Apr"
  },
  "timeline": {
    "startDate": "2026-01-01",
    "endDate": "2026-06-30",
    "tracks": [
      {
        "id": "M1",
        "label": "API Gateway & Auth",
        "color": "#0078D4",
        "milestones": [
          { "date": "2026-01-12", "type": "checkpoint", "label": "Jan 12" },
          { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" },
          { "date": "2026-05-01", "type": "production", "label": "May Prod" }
        ]
      }
    ]
  },
  "heatmap": {
    "columns": ["Jan", "Feb", "Mar", "Apr"],
    "highlightColumn": "Apr",
    "shipped": {
      "Jan": ["Auth service v2", "Rate limiter"],
      "Feb": ["Token refresh flow"],
      "Mar": ["OAuth2 PKCE", "Session mgmt"],
      "Apr": ["Gateway routing"]
    },
    "inProgress": { ... },
    "carryover": { ... },
    "blockers": { ... }
  }
}
```

---

## 5. Security & Infrastructure

### Authentication & Authorization

**None.** Per requirements, this is a no-auth, local-only tool. No login, no RBAC, no tokens.

If future needs arise, the simplest addition would be a single shared secret via `appsettings.json` checked by middleware—but this is explicitly out of scope.

### Data Protection

- `data.json` sits on the local filesystem. No encryption needed for a local reporting tool.
- If the JSON contains sensitive project names, ensure the file is in `.gitignore` and provide a `data.sample.json` template in the repo.
- The dashboard renders read-only data. There are no write operations, no user input, and no injection surfaces.

### Hosting & Deployment

| Approach | Command | Notes |
|----------|---------|-------|
| **Development** | `dotnet watch --project src/Dashboard` | Hot reload, default port 5000/5001 |
| **Local Production** | `dotnet run --configuration Release` | Kestrel serves directly, no IIS needed |
| **Screenshot Automation** | Playwright script captures page at 1920×1080 | Optional: `dotnet run` + `playwright screenshot` in a script |

**No cloud, no containers, no CDN.** The app runs on `localhost`. Kestrel (built into .NET 8) is the web server. No reverse proxy needed.

### Infrastructure Costs

**$0.** This runs on the developer's local machine. No servers, no subscriptions, no licenses.

---

## 6. Risks & Trade-offs

### Risk: Blazor Server Overhead for a Static Page

**Impact:** Low
**Description:** Blazor Server maintains a SignalR WebSocket connection per client. For a screenshot tool used by one person, this is irrelevant—but it means the app uses more memory than a static HTML file.
**Mitigation:** Consider using **Blazor Static SSR** (new in .NET 8) which renders HTML on the server without maintaining a circuit. This is ideal for a read-only dashboard. Set `@rendermode` to `null` (static) on the page.

### Risk: SVG Rendering Discrepancies Across Browsers

**Impact:** Medium
**Description:** SVG text positioning and drop shadow filters can render differently in Chrome vs Edge vs Firefox.
**Mitigation:** Standardize on **Edge/Chrome** for screenshot capture (both use Chromium). Document the target browser. Playwright uses Chromium by default.

### Risk: Segoe UI Font Availability

**Impact:** Low (Windows only)
**Description:** Segoe UI is installed on Windows by default but not on macOS/Linux.
**Mitigation:** Since the requirement is local-only and the user is on Windows, this is a non-issue. The CSS already includes Arial as a fallback.

### Risk: data.json Schema Drift

**Impact:** Medium
**Description:** As the dashboard evolves, the JSON structure may change, breaking existing data files.
**Mitigation:** Define a `$schema` property in `data.json` and validate on load. Use C# record types with `required` properties so deserialization fails fast on schema mismatches.

### Trade-off: No Component Library = More Manual CSS

**Accepted.** The design is fixed at 1920×1080 with specific colors, spacing, and layout. A component library would fight this specificity. Manual CSS matching the reference HTML is faster and more accurate.

### Trade-off: No Database = No Historical Tracking

**Accepted.** The user edits `data.json` manually each month. If historical snapshots are needed later, the simplest approach is versioning `data.json` files by date (`data.2026-04.json`) and adding a month selector dropdown.

---

## 7. Open Questions

1. **How many timeline tracks should be supported?** The reference shows 3 (M1, M2, M3). Should the data model cap at 3, or allow N tracks with dynamic SVG height?

2. **Should the "NOW" marker auto-calculate from system date, or be specified in `data.json`?** Auto-calculation is simpler but means screenshots change daily. A fixed date in JSON gives reproducible screenshots.

3. **Is the heatmap always 4 columns (months)?** The reference shows Jan–Apr. Should the grid support variable column counts (e.g., 6 months, 12 months)?

4. **How will `data.json` be updated?** Manual text editing? A companion CLI tool? This affects whether we need JSON schema validation and error messages.

5. **Should the app support multiple projects?** Currently scoped to one `data.json` = one project. If multiple projects are needed, a project selector or multiple JSON files would be the simplest extension.

6. **What is the screenshot capture workflow?** Manual browser screenshot, or should the app include a "Download as PNG" button using Playwright or html2canvas?

7. **Do the heatmap cells need tooltips or click-through links?** The reference design has no interactivity, but ADO backlog links could be useful for executives viewing the live page.

---

## 8. Implementation Recommendations

### Phase 1: Scaffold & Static Layout (Day 1)

- `dotnet new blazor -n ReportingDashboard -o src/ReportingDashboard`
- Create solution: `dotnet new sln` → `dotnet sln add src/ReportingDashboard`
- Port the reference HTML/CSS verbatim into `MainDashboard.razor` as static markup
- Verify pixel-perfect match at 1920×1080 in browser
- **Deliverable:** Static page that looks identical to the reference design with hardcoded data

### Phase 2: Data Model & JSON Loading (Day 1–2)

- Define C# record types for `DashboardData`, `Milestone`, `HeatmapCategory`, etc.
- Create `data.json` with fictional project data (e.g., "Project Phoenix")
- Implement `DashboardDataService` to load and deserialize JSON at startup
- Replace hardcoded markup with `@foreach` loops over the data model
- **Deliverable:** Dashboard renders from `data.json`; changing the file changes the output

### Phase 3: Dynamic SVG Timeline (Day 2)

- Implement date-to-X coordinate mapping in `TimelineSvg.razor`
- Render milestone shapes (diamonds, circles) from data model
- Position "NOW" marker based on current date or config
- Draw track lanes with correct colors
- **Deliverable:** Timeline section fully data-driven

### Phase 4: Polish & Screenshot Automation (Day 2–3)

- Fine-tune spacing, colors, and typography to match reference exactly
- Add `FileSystemWatcher` for live `data.json` reload during development
- (Optional) Add Playwright script for automated 1920×1080 PNG capture
- Create `data.sample.json` with documentation comments
- Write README with setup and usage instructions
- **Deliverable:** Production-ready reporting tool

### Quick Wins

1. **Copy-paste the reference CSS** — The design's CSS is well-structured and can be used almost verbatim. Don't over-engineer the styling.
2. **Use `dotnet watch`** — Instant feedback loop for CSS and markup changes.
3. **Playwright screenshot script** — A 10-line C# script that launches the app, navigates to `localhost:5000`, and saves a 1920×1080 PNG. This automates the PowerPoint workflow.

### Areas to Prototype First

1. **SVG date mapping** — The coordinate math for placing milestones on the timeline is the trickiest part. Prototype this in isolation before integrating into the full page.
2. **CSS Grid heatmap** — Verify that Blazor's rendered HTML produces the same grid layout as the reference HTML. Test with 0, 1, 5, and 10+ items per cell to ensure overflow handling.

---

## Appendix: Recommended Solution Structure

```
ReportingDashboard.sln
└── src/
    └── ReportingDashboard/
        ├── ReportingDashboard.csproj
        ├── Program.cs
        ├── Components/
        │   ├── App.razor
        │   ├── Routes.razor
        │   ├── Layout/
        │   │   └── MainLayout.razor
        │   └── Pages/
        │       └── Dashboard.razor
        ├── Components/Dashboard/
        │   ├── DashboardHeader.razor
        │   ├── TimelineSvg.razor
        │   ├── HeatmapGrid.razor
        │   ├── HeatmapRow.razor
        │   └── HeatmapCell.razor
        ├── Models/
        │   └── DashboardData.cs
        ├── Services/
        │   └── DashboardDataService.cs
        ├── wwwroot/
        │   ├── css/
        │   │   └── dashboard.css
        │   └── data.json
        ├── appsettings.json
        └── Properties/
            └── launchSettings.json
```

## Appendix: Key NuGet Packages

| Package | Version | Purpose | Required? |
|---------|---------|---------|-----------|
| `Microsoft.AspNetCore.Components.Web` | 8.0.x | Blazor Server (included in SDK) | Yes (implicit) |
| `System.Text.Json` | 8.0.x | JSON deserialization (included in SDK) | Yes (implicit) |
| `Microsoft.Playwright` | 1.41+ | Automated screenshot capture | Optional |
| `bUnit` | 1.25+ | Component unit testing | Optional |
| `xunit` | 2.7+ | Test framework | Optional |

**Total required NuGet packages beyond the .NET 8 SDK: Zero.**

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

![OriginalDesignConcept design](https://raw.githubusercontent.com/azurenerd/ReportingDashboard/62dc7dd0b787b86b86fa5b07f88beeba4e7b7498/docs/design-screenshots/OriginalDesignConcept.png)

*Rendered from `OriginalDesignConcept.html` at 1920×1080*
